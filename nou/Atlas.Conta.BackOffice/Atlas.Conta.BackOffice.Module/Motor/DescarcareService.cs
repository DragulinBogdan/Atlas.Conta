using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// P2 (design §5): generatorul descărcării de gestiune (DSC). NU e clona
// PoliticaConex — valorile diferă (cost din lot, nu prețul de vânzare), liniile
// se sparg 1→N pe loturi, iar predatorul se ÎNLOCUIEȘTE cu gestiunea (nu se
// inversează). Spargerea pe loturi se face la GENERARE (draftul concret e
// condiția override-ului manual — decizia 13); operarea nu creează linii.
// Generatorul NU aruncă niciodată la lipsă de stoc — restul e backorder normal
// (design §5): refuzul „lot fără sold în gestiune" trăiește în
// FacturaIesire.ValideazaOperare, gardianul de sold rămâne autoritatea la operare.
public static class DescarcareService {
    // TipStoc-ul căutării de stoc pentru clasa unei linii (design §5): regula DSC
    // specifică pe ClasaId bate generica (ClasaId null = orice Natura=Stoc), ca în
    // motor. Null = nicio regulă pentru clasa asta (bugetar → tip inert).
    internal static TipStoc? TipStocPentruClasa(IReadOnlyList<RegulaStoc> reguli, Guid? clasaId) {
        var specifica = reguli.FirstOrDefault(r => r.ClasaId != null && r.ClasaId == clasaId);
        if (specifica != null)
            return specifica.TipStoc;
        var generica = reguli.FirstOrDefault(r => r.ClasaId == null);
        return generica?.TipStoc;
    }

    // Restul nedescărcat per linie FCL de STOC (cusătura §2.2: interogabilă per
    // linie). Acoperit = Σ cantități pe liniile DSC cu LinieSursaId == linia, din
    // documente Draft SAU Operat (Stornat nu acoperă; draftul contează — altfel a
    // doua generare ar dubla alocarea), DOAR din DSC-urile copil ale ACESTEI
    // facturi (DocumentSursa == fcl — un DSC străin nu poate otrăvi acoperirea;
    // review P2 defect 2). Liniile manuale DSC (LinieSursa null) nu intră.
    // Totul pe proiecții server-side (25b).
    public static IReadOnlyList<(Guid LinieId, Guid? ProdusId, Guid? LotId, decimal Cantitate, decimal Acoperit, decimal RestNeacoperit)>
        RestNedescarcat(IObjectSpace os, FacturaIesire fcl) {
        var idsTip = fcl.Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var natura = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);

        var liniiStoc = fcl.Detalii.OfType<FacturaIesireDetaliu>()
            .Where(d => natura.GetValueOrDefault(d.TipMaterialId) == NaturaClasa.Stoc)
            .Select(d => new { d.ID, d.ProdusId, d.LotId, d.Cantitate })
            .ToList();
        if (liniiStoc.Count == 0)
            return Array.Empty<(Guid, Guid?, Guid?, decimal, decimal, decimal)>();

        var idsLinii = liniiStoc.Select(x => x.ID).ToList();
        var acoperit = os.GetObjectsQuery<DescarcareGestiuneDetaliu>()
            .Where(dd => dd.LinieSursaId != null && idsLinii.Contains(dd.LinieSursaId.Value)
                && dd.Document.Stare != StareDocument.Stornat
                && dd.Document.DocumentSursaId == fcl.ID)
            .GroupBy(dd => dd.LinieSursaId.Value)
            .Select(g => new { LinieId = g.Key, Suma = g.Sum(x => x.Cantitate) })
            .ToDictionary(x => x.LinieId, x => x.Suma);

        return liniiStoc.Select(x => {
            var acop = acoperit.GetValueOrDefault(x.ID);
            return (x.ID, x.ProdusId, x.LotId, x.Cantitate, acop, x.Cantitate - acop);
        }).ToList();
    }

    // Generatorul (design §5): pornit din FacturaIesire.GenereazaSecundar la
    // operarea FCL și din acțiunea manuală „Generează descărcarea" (backorder).
    // Întoarce un draft DSC (marcat Autogenerat + DocumentSursa) sau null; NU
    // comite (commit-ul aparține apelantului — în hook e tranzacția operării).
    public static DescarcareGestiune Genereaza(IObjectSpace os, FacturaIesire fcl, DateOnly data) {
        if (fcl.GestiuneDescarcareId == null)
            return null;
        var gestiuneId = fcl.GestiuneDescarcareId.Value;

        var resturi = RestNedescarcat(os, fcl).Where(x => x.RestNeacoperit > 0).ToList();
        if (resturi.Count == 0)
            return null;

        // Regulile de stoc ale DSC-ului: −1 pe Predator. Fără ele (bugetar) → tip
        // inert, nu se descarcă nimic (design §7).
        var tipDsc = MotorOperare.GasesteTipDocument(os, nameof(DescarcareGestiune));
        var reguliDsc = os.GetObjectsQuery<RegulaStoc>()
            .Where(r => r.TipDocumentId == tipDsc.ID && r.Latura == LaturaDocument.Predator && r.Semn < 0)
            .ToList();
        if (reguliDsc.Count == 0)
            return null;

        // Liniile sursă materializate (TipMaterialId + Dimensiuni de clonat) și
        // clasa per Tip (pentru TipStoc — proiecție, fără a atinge navigații).
        var liniiSursa = fcl.Detalii.OfType<FacturaIesireDetaliu>().ToDictionary(d => d.ID);
        var idsTip = resturi.Select(x => liniiSursa[x.LinieId].TipMaterialId).Distinct().ToList();
        var clasaPerTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId })
            .ToDictionary(t => t.ID, t => t.ClasaId);

        // Alocarea: mapa `dejaAlocat` (per lot, necomisă) se scade din solduri pe
        // parcurs. Contenția intra-draft (pin 2): PIN-urile întâi (identificarea
        // specifică bate FIFO), apoi liniile doar-produs.
        var dejaAlocat = new Dictionary<Guid, decimal>();
        var alocari = new List<(Guid LinieId, Guid TipMaterialId, Guid LotId, decimal Cantitate)>();
        void Adauga(Guid linieId, Guid tipMaterialId, Guid lotId, decimal cantitate) {
            alocari.Add((linieId, tipMaterialId, lotId, cantitate));
            dejaAlocat[lotId] = dejaAlocat.GetValueOrDefault(lotId) + cantitate;
        }

        foreach (var r in resturi.OrderByDescending(x => x.LotId != null)) {
            var linie = liniiSursa[r.LinieId];
            var tipStoc = TipStocPentruClasa(reguliDsc, clasaPerTip.GetValueOrDefault(linie.TipMaterialId));
            if (tipStoc == null)
                continue;
            if (r.LotId != null) {
                // Pin: alocă DOAR din acel lot, fără fallback FIFO pe restul lui
                // (pinul e intenția magazinului — deblocarea = scoaterea pinului).
                var lotId = r.LotId.Value;
                var disponibil = StocService.Sold(os, new CheieStoc(lotId, gestiuneId, tipStoc.Value), data)
                    - dejaAlocat.GetValueOrDefault(lotId);
                var alocat = Math.Min(r.RestNeacoperit, disponibil);
                if (alocat > 0)
                    Adauga(r.LinieId, linie.TipMaterialId, lotId, alocat);
            }
            else {
                var (parti, _) = StocService.AlocaFifoTolerant(
                    os, r.ProdusId.Value, gestiuneId, tipStoc.Value, data, r.RestNeacoperit, dejaAlocat);
                foreach (var p in parti)
                    Adauga(r.LinieId, linie.TipMaterialId, p.LotId, p.Cantitate);
            }
        }

        if (alocari.Count == 0)
            return null;

        // Prețul lotului pentru precompletarea Valorii (lizibilitatea draftului;
        // PregatesteOperare al DSC rămâne autoritatea la operare).
        var idsLot = alocari.Select(a => a.LotId).Distinct().ToList();
        var pretPerLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.PretUnitar })
            .ToDictionary(l => l.ID, l => l.PretUnitar);

        var dsc = os.CreateObject<DescarcareGestiune>();
        dsc.Data = data;
        dsc.PredatorId = gestiuneId;          // gestiunea de descărcare
        dsc.PrimitorId = fcl.PrimitorId;      // clientul de pe FCL
        dsc.DocumentSursa = fcl;
        dsc.Autogenerat = true;
        foreach (var a in alocari) {
            var d = os.CreateObject<DescarcareGestiuneDetaliu>();
            d.Document = dsc;
            d.TipMaterialId = a.TipMaterialId;
            d.LotId = a.LotId;
            d.Cantitate = a.Cantitate;
            d.LinieSursaId = a.LinieId;
            // Dimensiuni clonate de pe linia FCL (mecanismul din GenereazaConex);
            // NU se clonează TipTva/ValoareTva/Angajament (TVA rămâne pe FCL).
            d.Dimensiuni = DimensiuniResolver.Rezolva(liniiSursa[a.LinieId].Dimensiuni);
            d.Valoare = Scara.RotunjesteBani(a.Cantitate * pretPerLot.GetValueOrDefault(a.LotId));
        }
        return dsc;
    }
}
