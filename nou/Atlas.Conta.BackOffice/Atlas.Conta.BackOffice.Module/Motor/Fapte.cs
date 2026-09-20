using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Singura ortografie a mapării entitate → fapt (F24-D5): impură (citește), fără
// nicio decizie — deciziile stau în `Potrivire`. Tot ce se citește aici e pe
// FK-uri și proiecții, fără navigații lazy (25b).
internal static class Fapte {
    public static List<RegulaContareFapt> ReguliContare(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .ToList()
            .Select(r => new RegulaContareFapt(r.ID, r.TipMaterialId, r.NaturaFiltru, r.SemnFiltru,
                r.PastreazaSemn, r.SursaContDebit, r.ContDebitId, r.SursaContCredit, r.ContCreditId,
                r.DinSeed, r.DimensiuniComun(), r.DimensiuniOverrideDebit(), r.DimensiuniOverrideCredit()))
            .ToList();

    public static List<RegulaStocFapt> ReguliStoc(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<RegulaStoc>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .Select(r => new { r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed })
            .ToList()
            .Select(r => new RegulaStocFapt(r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed))
            .ToList();

    public static PoliticaConexFapt? Conex(IObjectSpace os, Guid tipDocumentId) {
        var politica = os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipDocumentId);
        return politica == null
            ? null
            : new PoliticaConexFapt(politica.ID, politica.TipDocumentTintaId,
                politica.InverseazaLaturi, politica.NaturaFiltru);
    }

    public static List<PoliticaTvaImplicitFapt> PoliticiTvaImplicit(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .Select(r => new { r.ID, r.ClasaFiscala, r.ValabilDeLa, r.TipTvaId, r.DinSeed })
            .ToList()
            .Select(r => new PoliticaTvaImplicitFapt(r.ID, r.ClasaFiscala, r.ValabilDeLa, r.TipTvaId, r.DinSeed))
            .ToList();

    public static Dictionary<Guid, TipTvaFapt> TipuriTva(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<TipTva>()
                .Where(t => ids.Contains(t.ID))
                .Select(t => new { t.ID, t.Cod, t.Regim, t.Activ, t.Cota, t.ContTvaDeductibilId, t.ContTvaColectatId })
                .ToList()
                .Select(t => new TipTvaFapt(t.ID, t.Cod, t.Regim, t.Activ, t.Cota,
                    t.ContTvaDeductibilId, t.ContTvaColectatId))
                .ToDictionary(t => t.Id);

    public static Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)>
        ClaseTip(IObjectSpace os, IEnumerable<Guid> idsTip) {
        var ids = idsTip.Distinct().ToList();
        return os.GetObjectsQuery<TipMaterial>()
            .Where(t => ids.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura, t.Denumire, t.ContImplicitId })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura, t.Denumire, t.ContImplicitId));
    }

    public static LinieFapt Linie(DocumentDetaliu d,
            IReadOnlyDictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip) {
        var gasit = claseTip.TryGetValue(d.TipMaterialId, out var info);
        return new LinieFapt(d.TipMaterialId, gasit ? info.ClasaId : null, gasit ? info.Natura : null,
            Math.Sign(d.Cantitate), d.LotId, gasit ? info.ContImplicitId : null);
    }

    public static LaturiFapt Laturi(Repartitor predator, Repartitor primitor) =>
        new(predator?.ContImplicitId, primitor?.ContImplicitId);

    // ───────────── B-D3: operandul ÎNCHIS al unui document ─────────────
    //
    // Citire pe SETURI, o interogare per tabelă (invariantul VI): declarantul
    // primește tot ce influențează rezultatul și nu mai atinge ObjectSpace-ul.
    public static Declaratii.Operand Operand(IObjectSpace os, Document doc) {
        var tipDoc = MotorOperare.GasesteTipDocument(os, doc);
        // MEDIU-3: `Detalii` vine fără ORDER BY, iar secvența liniilor decide
        // evaluarea (N-D7) și nominalizarea; `Pozitie` pe linie e restanță la TR-D7.
        var linii = doc.Detalii.OrderBy(d => d.ID).ToList();

        var idsLot = linii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var dateLot = DateLot(os, idsLot);

        var idsProdus = dateLot.Select(l => l.ProdusId)
            .Concat(linii.Select(d => d.ProdusCules()).Where(p => p != null).Select(p => p.Value))
            .Distinct().ToList();
        var tipPerProdus = TipuriProdus(os, idsProdus);

        var claseTip = ClaseTip(os, linii.Select(d => d.TipMaterialId)
            .Concat(tipPerProdus.Values.Where(t => t != null).Select(t => t.Value)));

        var loturi = dateLot.ToDictionary(l => l.Id, l => {
            var tipProdus = tipPerProdus.GetValueOrDefault(l.ProdusId);
            Guid? contImplicit = null;
            if (tipProdus != null && claseTip.TryGetValue(tipProdus.Value, out var info))
                contImplicit = info.ContImplicitId;
            return new Declaratii.LotFapt(l.Id, l.ProdusId, tipProdus, contImplicit, l.Data, l.PretUnitar);
        });

        var reguliContare = ReguliContare(os, tipDoc.ID);
        var reguliStoc = ReguliStoc(os, tipDoc.ID);
        var politicaTva = Tva(os, tipDoc.ID);
        var tipuriTva = TipuriTva(os,
            linii.Where(d => d.TipTvaId != null).Select(d => d.TipTvaId.Value).Distinct().ToList());
        var repartitori = Repartitori(os, [doc.PredatorId, doc.PrimitorId]);
        var sursa = Sursa(os, doc);
        var conturi = Conturi(os,
            ConturiAtinse(linii, claseTip, repartitori, reguliContare, politicaTva, tipuriTva, sursa.Partide));

        var solduriLoturi = SolduriLoturi(os, doc, linii, claseTip, reguliStoc, idsLot);

        // MAJOR-1: partidele sursei sunt ale conturilor cu rol de terț; pe celelalte
        // soldul e al contului, nu al unei partide de nominalizat.
        var partideSursa = sursa.Partide
            .Where(p => conturi.GetValueOrDefault(p.Cont)?.RolTert is not (null or RolTertCont.Niciunul))
            .ToList();

        // F27-D5: aceeași regulă pe tot documentul (`dataFapt` = `doc.Data`), deci
        // un singur fapt, nu unul per linie.
        int? perioadaDeclarare = null;
        if (politicaTva != null) {
            var (an, luna) = RegistruTvaService.PerioadaDeclarare(
                os, doc, doc.Data, doc.DataInregistrare, politicaTva.DeclarareIntarziata);
            perioadaDeclarare = (an * 100) + luna;
        }

        return new Declaratii.Operand(
            Document(doc, tipDoc, repartitori),
            [.. linii.Select(d => Linie(d, claseTip, loturi, tipPerProdus))],
            reguliContare,
            reguliStoc,
            politicaTva,
            tipuriTva,
            conturi,
            solduriLoturi,
            sursa.Ramas,
            partideSursa,
            sursa.Data,
            perioadaDeclarare,
            // Deriva maximă explicabilă prin rotunjirea PER LINIE de azi; declarantul o
            // înmulțește cu liniile cotei. Devine rând de politică la TR-D7 (B-D6).
            0.01m,
            new N.PerioadaDeschisa(doc.DataInregistrare.Year, doc.DataInregistrare.Month),
            new N.VersiunePolitica("seed", doc.DataInregistrare));
    }

    // Restul documentului-sursă FĂRĂ stingerile documentului curent: ca soldurile de
    // lot, starea se citește dinaintea documentului care o schimbă — la probă
    // împerecherea automată e deja scrisă, iar `Ramas` ar întoarce 0 (B-D5).
    // Soldul PER CONT al sursei e cel care spune ce poate ține fiecare partidă a ei
    // (MAJOR-1): restul e o cifră a documentului, partida e a contului.
    static (decimal? Ramas, DateOnly? Data, List<(Guid Cont, decimal Sold)> Partide) Sursa(
            IObjectSpace os, Document doc) {
        if (!doc.Autogenerat || doc.DocumentSursaId is not Guid sursaId)
            return (null, null, []);
        var sursa = os.GetObjectsQuery<Document>()
            .Where(d => d.ID == sursaId)
            .Select(d => new { d.TotalStingere, d.DataInregistrare })
            .ToList()
            .FirstOrDefault();
        if (sursa == null)
            return (null, null, []);
        var asignat = os.GetObjectsQuery<Imperechere>()
            .Where(i => (i.DocumentStingatorId == sursaId || i.DocumentId == sursaId)
                && i.DocumentStingatorId != doc.ID)
            .Select(i => (decimal?)i.Suma)
            .Sum() ?? 0m;
        var note = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId == sursaId && !r.Storno)
            .Select(r => new { r.ContDebitId, r.ContCreditId, r.Valoare })
            .ToList();
        var sume = new Dictionary<Guid, decimal>();
        foreach (var n in note) {
            sume[n.ContDebitId] = sume.GetValueOrDefault(n.ContDebitId) + n.Valoare;
            sume[n.ContCreditId] = sume.GetValueOrDefault(n.ContCreditId) - n.Valoare;
        }
        return (
            (sursa.TotalStingere ?? 0m) - asignat,
            sursa.DataInregistrare,
            [.. sume.Where(s => s.Value != 0m).OrderBy(s => s.Key).Select(s => (s.Key, s.Value))]);
    }

    static Declaratii.DocumentFapt Document(Document doc, TipDocument tipDoc,
            IReadOnlyDictionary<Guid, Declaratii.RepartitorFapt> repartitori) =>
        new(doc.ID, tipDoc.Cod, tipDoc.ID, doc.Data, doc.DataInregistrare, doc.Numar,
            doc.Autogenerat, doc.DocumentSursaId,
            Repartitor(repartitori, doc.PredatorId), Repartitor(repartitori, doc.PrimitorId),
            // Valuta/Cursul sunt câmpuri de FRUNZĂ (FacturaIntrare), fără interfață
            // declarată: pilotul nu le citește (B-D2, raportat main-ului).
            null, null,
            doc is IDocumentCuScadenta scadenta ? scadenta.DataScadenta : null);

    static Declaratii.RepartitorFapt Repartitor(
            IReadOnlyDictionary<Guid, Declaratii.RepartitorFapt> repartitori, Guid id) =>
        repartitori.TryGetValue(id, out var fapt) ? fapt : new Declaratii.RepartitorFapt(id, null, null, default);

    static Declaratii.LinieOperand Linie(DocumentDetaliu d,
            IReadOnlyDictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip,
            IReadOnlyDictionary<Guid, Declaratii.LotFapt> loturi,
            IReadOnlyDictionary<Guid, Guid?> tipPerProdus) {
        var gasit = claseTip.TryGetValue(d.TipMaterialId, out var info);
        var explicita = d as ILinieCuPostareExplicita;
        var produs = d.ProdusCules();
        return new Declaratii.LinieOperand(
            d.ID,
            d.TipMaterialId,
            gasit ? info.ClasaId : null,
            gasit ? info.Natura : null,
            gasit ? info.ContImplicitId : null,
            d.LotId,
            d.LotId != null ? loturi.GetValueOrDefault(d.LotId.Value) : null,
            d.Cantitate,
            d.Valoare,
            d.ValoareTva,
            d.TipTvaId,
            d is ILinieCuPretUnitar pret ? pret.PretUnitar : null,
            produs,
            produs != null ? tipPerProdus.GetValueOrDefault(produs.Value) : null,
            explicita?.ContDebitId,
            explicita?.ContCreditId,
            explicita?.RepartitorDebitId,
            explicita?.RepartitorCreditId,
            Analiza(d.DimensiuniCulese()),
            d.AngajamentId);
    }

    static N.Analiza Analiza(Dimensiuni dim) =>
        new(dim.CodFunctionalId, dim.CodEconomicId, dim.SursaFinantareId,
            dim.UnitateId, dim.ProiectId, dim.CentruCostId);

    // Soldul lotului pe cheia de stoc a laturii PREDATOARE, la `DataInregistrare`
    // și FĂRĂ documentul curent: `TipStoc`-ul e al regulii potrivite, deci cheia
    // se află aici (citirea are nevoie de ea), iar mișcarea o declară frunza.
    static Dictionary<Guid, N.Sold> SolduriLoturi(IObjectSpace os, Document doc,
            IReadOnlyList<DocumentDetaliu> linii,
            IReadOnlyDictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip,
            IReadOnlyList<RegulaStocFapt> reguliStoc, IReadOnlyList<Guid> idsLot) {
        var solduri = new Dictionary<Guid, N.Sold>();
        if (idsLot.Count == 0)
            return solduri;
        // Cheia se află înaintea citirii: fără nicio latură predatoare (factura își
        // naște loturile) nu e nimic de citit, deci nici interogare (MINOR-7).
        var chei = new List<(Guid Lot, CheieStoc Cheie)>();
        foreach (var d in linii) {
            if (d.LotId is not Guid lotId || chei.Any(c => c.Lot == lotId))
                continue;
            var tipStoc = Potrivire.Stoc(reguliStoc, Linie(d, claseTip))
                .Where(p => p.Latura == LaturaDocument.Predator)
                .SelectMany(p => p.Reguli)
                .Select(r => (TipStoc?)r.TipStoc)
                .FirstOrDefault();
            if (tipStoc is TipStoc tip)
                chei.Add((lotId, new CheieStoc(lotId, doc.PredatorId, tip)));
        }
        if (chei.Count == 0)
            return solduri;
        var peCheie = StocService.SolduriLaData(os, idsLot, doc.DataInregistrare, doc.ID);
        foreach (var (lotId, cheie) in chei)
            solduri[lotId] = Sold(peCheie.GetValueOrDefault(cheie));
        return solduri;
    }

    static N.Sold Sold(SoldStoc sold) =>
        new(sold.Valoare > 0m ? sold.Valoare : 0m, sold.Valoare < 0m ? -sold.Valoare : 0m, sold.Cantitate, 0m);

    static List<Guid> ConturiAtinse(IReadOnlyList<DocumentDetaliu> linii,
            IReadOnlyDictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip,
            IReadOnlyDictionary<Guid, Declaratii.RepartitorFapt> repartitori,
            IReadOnlyList<RegulaContareFapt> reguliContare,
            Declaratii.PoliticaTvaFapt politicaTva,
            IReadOnlyDictionary<Guid, TipTvaFapt> tipuriTva,
            IReadOnlyList<(Guid Cont, decimal Sold)> partideSursa) {
        var ids = new List<Guid?>();
        ids.AddRange(partideSursa.Select(p => (Guid?)p.Cont));
        ids.AddRange(claseTip.Values.Select(c => c.ContImplicitId));
        ids.AddRange(repartitori.Values.Select(r => r.ContImplicitId));
        foreach (var r in reguliContare) {
            ids.Add(r.ContDebitId);
            ids.Add(r.ContCreditId);
        }
        ids.Add(politicaTva?.ContrapartidaFallbackId);
        foreach (var t in tipuriTva.Values) {
            ids.Add(t.ContTvaDeductibilId);
            ids.Add(t.ContTvaColectatId);
        }
        foreach (var d in linii)
            if (d is ILinieCuPostareExplicita explicita) {
                ids.Add(explicita.ContDebitId);
                ids.Add(explicita.ContCreditId);
            }
        return ids.Where(id => id != null).Select(id => id.Value).Distinct().ToList();
    }

    static List<(Guid Id, Guid ProdusId, DateOnly Data, decimal PretUnitar)> DateLot(
            IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Lot>()
                .Where(l => ids.Contains(l.ID))
                .Select(l => new { l.ID, l.ProdusId, l.Data, l.PretUnitar })
                .ToList()
                .Select(l => (l.ID, l.ProdusId, l.Data, l.PretUnitar))
                .ToList();

    static Dictionary<Guid, Guid?> TipuriProdus(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Produs>()
                .Where(p => ids.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToList()
                .ToDictionary(p => p.ID, p => p.TipMaterialId);

    static Dictionary<Guid, Declaratii.RepartitorFapt> Repartitori(IObjectSpace os, IReadOnlyList<Guid> ids) {
        var cerute = ids.Where(id => id != Guid.Empty).Distinct().ToList();
        return cerute.Count == 0
            ? []
            : os.GetObjectsQuery<Repartitor>()
                .Where(r => cerute.Contains(r.ID))
                .Select(r => new { r.ID, r.ClrType, r.ContImplicitId, r.Calitati })
                .ToList()
                .Select(r => new Declaratii.RepartitorFapt(r.ID,
                    Enum.TryParse<Declaratii.FelRepartitor>(r.ClrType, out var fel)
                        ? fel
                        : (Declaratii.FelRepartitor?)null,
                    r.ContImplicitId, r.Calitati))
                .ToDictionary(r => r.Id);
    }

    static Dictionary<Guid, Declaratii.ContFapt> Conturi(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Cont>()
                .Where(c => ids.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol, c.RolTert })
                .ToList()
                .Select(c => new Declaratii.ContFapt(c.ID, c.Simbol, c.RolTert))
                .ToDictionary(c => c.Id);

    static Declaratii.PoliticaTvaFapt Tva(IObjectSpace os, Guid tipDocumentId) {
        var politica = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDocumentId);
        return politica == null
            ? null
            : new Declaratii.PoliticaTvaFapt(politica.Directie, politica.SursaContrapartida,
                politica.ContrapartidaFallbackId, politica.DeclarareIntarziata);
    }
}
