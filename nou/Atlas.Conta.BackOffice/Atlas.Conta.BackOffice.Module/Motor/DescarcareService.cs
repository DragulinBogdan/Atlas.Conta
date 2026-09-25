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
    // Contul cerut de DSC se rezolvă din politica lui, nu din TipStoc și nu
    // prin reclasificarea loturilor istorice. Aceeași rezolvare pentru pin/FIFO.
    internal static Dictionary<Guid, Guid> ConturiStoc(IObjectSpace os, FacturaIesire fcl) {
        var tip = MotorOperare.GasesteTipDocument(os, nameof(DescarcareGestiune));
        var reguli = Fapte.ReguliContare(os, tip.ID);
        var clase = Fapte.ClaseTip(os, fcl.Detalii.Select(d => d.TipMaterialId));
        var laturi = Fapte.Laturi(fcl.GestiuneDescarcare, fcl.Primitor);
        var rezultat = new Dictionary<Guid, Guid>();
        foreach (var linie in fcl.Detalii) {
            var fapt = Fapte.Linie(linie, clase);
            if (fapt.Natura != NaturaClasa.Stoc) continue;
            if (Potrivire.Contare(reguli, fapt).Castigator is not { } regula)
                throw new OperareException("CONT_STOC_LIPSA: linia de stoc nu are regulă de contare DSC.");
            var cont = Potrivire.Cont(regula.SursaContCredit, regula.ContCreditId, fapt.ContImplicitTipId, laturi);
            if (cont.ContId is not Guid id)
                throw new OperareException("CONT_STOC_LIPSA: contul de ieșire DSC nu este rezolvat.");
            rezultat[linie.ID] = id;
        }
        return rezultat;
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
    public static DescarcareGestiune Genereaza(IObjectSpace os, FacturaIesire fcl, DateOnly data,
            DateOnly? dataInregistrare = null) {
        if (fcl.GestiuneDescarcareId == null)
            return null;
        var gestiuneId = fcl.GestiuneDescarcareId.Value;

        var resturi = RestNedescarcat(os, fcl).Where(x => x.RestNeacoperit > 0).ToList();
        if (resturi.Count == 0)
            return null;

        // Regulile de stoc ale DSC-ului: −1 pe Predator. Fără ele (bugetar) → tip
        // inert, nu se descarcă nimic (design §7).
        var tipDsc = MotorOperare.GasesteTipDocument(os, nameof(DescarcareGestiune));
        var reguliDsc = Fapte.ReguliStoc(os, tipDsc.ID)
            .Where(r => r.Latura == LaturaDocument.Predator && r.Semn < 0)
            .ToList();
        if (reguliDsc.Count == 0)
            return null;

        // Soldurile sunt citite o singură dată pentru toate liniile, pe cheia
        // completă. Alocările locale nu devin rezervări pentru alte documente.
        var liniiSursa = fcl.Detalii.OfType<FacturaIesireDetaliu>().ToDictionary(d => d.ID);
        var claseTip = Fapte.ClaseTip(os, resturi.Select(x => liniiSursa[x.LinieId].TipMaterialId));
        var conturi = ConturiStoc(os, fcl);
        var produse = resturi.Select(r => r.ProdusId).OfType<Guid>().Distinct().ToArray();
        var solduri = Cub.Citiri.Loturi.Cumulate(os, dataInregistrare ?? data)
            .Where(s => s.GestiuneId == gestiuneId && produse.Contains(s.ProdusId) && s.Cantitate > 0m)
            .OrderBy(s => s.Deschisa).ThenBy(s => s.LotId).ToList();

        // Alocarea: mapa `dejaAlocat` (per lot, necomisă) se scade din solduri pe
        // parcurs. Contenția intra-draft (pin 2): PIN-urile întâi (identificarea
        // specifică bate FIFO), apoi liniile doar-produs.
        var dejaAlocat = new Dictionary<(Guid Lot, Guid Cont), decimal>();
        var alocari = new List<(Guid LinieId, Guid TipMaterialId, Guid LotId, decimal Cantitate)>();
        void Adauga(Guid linieId, Guid tipMaterialId, Guid lotId, Guid contId, decimal cantitate) {
            alocari.Add((linieId, tipMaterialId, lotId, cantitate));
            dejaAlocat[(lotId, contId)] = dejaAlocat.GetValueOrDefault((lotId, contId)) + cantitate;
        }

        foreach (var r in resturi.OrderByDescending(x => x.LotId != null)) {
            var linie = liniiSursa[r.LinieId];
            if (!Potrivire.Stoc(reguliDsc, Fapte.Linie(linie, claseTip))
                    .Any(p => p.Latura == LaturaDocument.Predator && p.Reguli.Count > 0)) continue;
            var cont = conturi[linie.ID];
            var ramas = r.RestNeacoperit;
            foreach (var sold in solduri.Where(s => s.ContId == cont && s.ProdusId == r.ProdusId
                    && (r.LotId == null || s.LotId == r.LotId))) {
                var disponibil = sold.Cantitate - dejaAlocat.GetValueOrDefault((sold.LotId, cont));
                var alocat = Math.Min(ramas, disponibil);
                if (alocat <= 0) continue;
                Adauga(r.LinieId, linie.TipMaterialId, sold.LotId, cont, alocat);
                ramas -= alocat;
                if (ramas == 0) break;
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
        dsc.DataInregistrare = dataInregistrare ?? data;
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
            d.PreiaDimensiuni(liniiSursa[a.LinieId].DimensiuniCulese());
            d.Valoare = Scara.RotunjesteBani(a.Cantitate * pretPerLot.GetValueOrDefault(a.LotId));
        }
        return dsc;
    }
}
