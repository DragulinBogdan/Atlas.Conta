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
                r.DinSeed, r.DimensiuniComun(), r.DimensiuniOverrideDebit(), r.DimensiuniOverrideCredit())
                { Versiune = r.OptimisticLockField })
            .ToList();

    public static List<RegulaStocFapt> ReguliStoc(IObjectSpace os, Guid tipDocumentId) =>
        os.GetObjectsQuery<RegulaStoc>()
            .Where(r => r.TipDocumentId == tipDocumentId)
            .Select(r => new { r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed, r.OptimisticLockField })
            .ToList()
            .Select(r => new RegulaStocFapt(r.ID, r.Latura, r.ClasaId, r.TipStoc, r.Semn, r.DinSeed) { Versiune = r.OptimisticLockField })
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
                .Select(t => new { t.ID, t.Cod, t.Regim, t.Activ, t.Cota, t.ContTvaDeductibilId, t.ContTvaColectatId, t.DeImport, t.ValabilDeLa, t.ValabilPanaLa })
                .ToList()
                .Select(t => new TipTvaFapt(t.ID, t.Cod, t.Regim, t.Activ, t.Cota,
                    t.ContTvaDeductibilId, t.ContTvaColectatId, t.DeImport, t.ValabilDeLa, t.ValabilPanaLa))
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
        // S-D6: `Detalii` vine fără ORDER BY, iar secvența liniilor decide evaluarea
        // (N-D7) și nominalizarea — aceeași ordine ca în motorul vechi.
        var linii = doc.Detalii.OrderBy(d => d.Pozitie).ThenBy(d => d.ID).ToList();

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
            return new Declaratii.LotFapt(l.Id, l.ProdusId, tipProdus, contImplicit, l.Data, l.PretUnitar) {
                LinieIntrareId = l.LinieIntrareId, GestiuneId = l.GestiuneId,
            };
        });

        var reguliContare = ReguliContare(os, tipDoc.ID);
        var reguliStoc = ReguliStoc(os, tipDoc.ID);
        var politicaTvaEntitate = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDoc.ID);
        var politicaTva = Tva(politicaTvaEntitate);
        var tipuriTva = TipuriTva(os,
            linii.Where(d => d.TipTvaId != null).Select(d => d.TipTvaId.Value).Distinct().ToList());
        var explicite = linii.OfType<ILinieCuPostareExplicita>().ToList();
        var repartitori = Repartitori(os, [doc.PredatorId, doc.PrimitorId,
            .. explicite.SelectMany(l => new[] { l.RepartitorDebitId, l.RepartitorCreditId }).OfType<Guid>(),
            .. reguliContare.SelectMany(r => new[] { r.OverrideDebit?.RepartitorId,
                r.OverrideCredit?.RepartitorId, r.Comun?.RepartitorId }).OfType<Guid>()]);
        var sursa = Sursa(os, doc);
        var conturi = Conturi(os,
            ConturiAtinse(linii, claseTip, repartitori, reguliContare, politicaTva, tipuriTva, sursa.Partide));

        var solduriLoturi = SolduriLoturi(os, doc, idsLot);

        var partideSursa = sursa.Partide
            .Where(p => conturi.GetValueOrDefault(p.Cont)?.UrmarestePartide == true)
            .ToList();

        var fiscal = politicaTva == null ? null : FiscalitateService.Atribuie(os, doc, politicaTva.Directie);
        var perioadaDeclarare = fiscal?.PerioadaD300;

        return Cub.ReceptiiConexe.Completeaza(os, doc, ImobilizariFapte.Completeaza(os, new Declaratii.Operand(
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
            politicaTvaEntitate?.TolerantaTaxa,
            new N.PerioadaDeschisa(doc.DataInregistrare.Year, doc.DataInregistrare.Month)) {
                ReperFiscal = fiscal?.Reper,
                Repartitori = repartitori,
                PartideDisponibile = PartideDisponibile(os, doc, explicite, conturi, repartitori),
                UnitatiSursa = UnitatiSursa(os, doc),
                DisponibilPartideSursa = sursa.Disponibil,
            }));
    }

    static IReadOnlyList<N.Unitate> UnitatiSursa(IObjectSpace os, Document doc) =>
        !doc.Autogenerat || doc.DocumentSursaId is not Guid sursa ? [] :
        Cub.Citiri.Partide.Postari(os)
            .Where(p => p.DocumentId == sursa && p.Spatiu == N.Spatiu.Contabil)
            .Select(p => new { p.Unitate, p.Cont, p.Partener, p.UnitateDeschisa }).Distinct().ToList()
            .Select(p => new N.Unitate(p.Unitate.Value, N.FelUnitate.Partida, p.Cont,
                p.Partener, null, p.UnitateDeschisa.Value)).ToList();

    static IReadOnlyList<Declaratii.SoldPartidaFapt> PartideDisponibile(IObjectSpace os, Document doc,
            IReadOnlyList<ILinieCuPostareExplicita> linii,
            IReadOnlyDictionary<Guid, Declaratii.ContFapt> conturi,
            IReadOnlyDictionary<Guid, Declaratii.RepartitorFapt> repartitori) {
        var perechi = linii.SelectMany(l => new[] {
            (Cont: l.ContDebitId, Repartitor: l.RepartitorDebitId),
            (Cont: l.ContCreditId, Repartitor: l.RepartitorCreditId) })
            .Where(p => p.Cont is Guid c && p.Repartitor is Guid r
                && conturi.GetValueOrDefault(c)?.UrmarestePartide == true
                && repartitori.GetValueOrDefault(r)?.Parte == Declaratii.Parte.Extern).ToList();
        if (perechi.Count == 0) return [];
        var idsCont = perechi.Select(p => p.Cont.Value).Distinct().ToList();
        var idsTert = perechi.Select(p => p.Repartitor.Value).Distinct().ToList();
        var zi = doc.DataInregistrare;
        return Cub.Citiri.Partide.Postari(os)
            .Where(p => p.DocumentId != doc.ID && idsCont.Contains(p.Cont) && idsTert.Contains(p.Partener.Value))
            .GroupBy(p => new { p.Unitate, p.Cont, p.Partener, p.Data })
            .Select(g => new { g.Key.Unitate, g.Key.Cont, g.Key.Partener, g.Key.Data,
                Deschisa = g.Min(p => p.UnitateDeschisa.Value),
                Debit = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : 0m),
                Credit = g.Sum(p => p.Latura == N.Latura.Credit ? p.Valoare : 0m) })
            .ToList()
            .GroupBy(p => new { p.Unitate, p.Cont, p.Partener })
            .Where(g => g.Any(p => p.Data <= zi))
            .Select(g => {
                var pana = g.Where(p => p.Data <= zi).ToArray();
                var sold = new N.Sold(pana.Sum(p => p.Debit), pana.Sum(p => p.Credit), 0m, 0m);
                var disponibil = Cub.Citiri.Partide.DisponibilTemporal(
                    g.Select(p => (p.Data, p.Debit - p.Credit)), zi, Math.Sign(sold.Net));
                return new Declaratii.SoldPartidaFapt(
                    new N.Unitate(g.Key.Unitate.Value, N.FelUnitate.Partida, g.Key.Cont,
                        g.Key.Partener.Value, null, g.Min(p => p.Deschisa)),
                    sold, disponibil);
            })
            .OrderBy(p => p.Unitate.Deschisa).ThenBy(p => p.Unitate.Id).ToList();
    }

    // Restul documentului-sursă FĂRĂ stingerile documentului curent: ca soldurile de
    // lot, starea se citește dinaintea documentului care o schimbă — la probă
    // împerecherea automată e deja scrisă, iar `Ramas` ar întoarce 0 (B-D5).
    // Soldul PER CONT al sursei e cel care spune ce poate ține fiecare partidă a ei
    // (MAJOR-1): restul e o cifră a documentului, partida e a contului.
    static (decimal? Ramas, DateOnly? Data, List<(Guid Cont, decimal Sold)> Partide, Dictionary<Guid, decimal> Disponibil) Sursa(
            IObjectSpace os, Document doc) {
        if (!doc.Autogenerat || doc.DocumentSursaId is not Guid sursaId)
            return (null, null, [], []);
        var sursa = os.GetObjectsQuery<Document>()
            .Where(d => d.ID == sursaId)
            .Select(d => new { d.DataInregistrare })
            .SingleOrDefault();
        if (sursa == null)
            return (null, null, [], []);
        // Factura nominalizează și recepția în cub înaintea NIR-ului conex.
        // Restul sursei este al unităților ei, cu efectul documentului curent exclus.
        var zi = doc.DataInregistrare;
        var peZile = Cub.Citiri.Partide.MiscariPePartidele(os, sursaId, doc.ID).ToArray();
        var partide = peZile.GroupBy(p => new { p.UnitateId, p.ContId, p.PartenerId }).Select(g => {
            var miscari = g.Select(p => (p.Data, p.Net)).ToArray();
            var initial = Cub.Citiri.Partide.Evolutie(miscari, zi).First().Sold;
            return (Cont: g.Key.ContId, Net: initial, Disponibil: Cub.Citiri.Partide.DisponibilTemporal(miscari, zi, Math.Sign(initial)));
        }).ToArray();
        return (partide.Sum(p => p.Disponibil), sursa.DataInregistrare,
            [.. partide.GroupBy(p => p.Cont).Select(g => (g.Key, g.Sum(p => p.Net)))
                .Where(p => p.Item2 != 0m).OrderBy(p => p.Key)],
            partide.GroupBy(p => p.Cont).ToDictionary(g => g.Key, g => g.Sum(p => p.Disponibil)));
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

    // T-D13: faptele laturilor pentru ușa entității.
    internal static (Declaratii.RepartitorFapt Predator, Declaratii.RepartitorFapt Primitor) Laturile(
            IObjectSpace os, Document doc) {
        var repartitori = Repartitori(os, [doc.PredatorId, doc.PrimitorId]);
        return (Repartitor(repartitori, doc.PredatorId), Repartitor(repartitori, doc.PrimitorId));
    }

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
            d.AngajamentId) {
                Transformare = d is ILinieCuTransformare transformare ? transformare.TransformareCuleasa() : null,
                DiferentaInventar = d is ILinieCuDiferentaInventar inventar ? inventar.DiferentaCuleasa() : null,
                Imobilizare = d is ILinieCuImobilizare imobilizare ? imobilizare.ImobilizareCuleasa() : null,
            };
    }

    static N.Analiza Analiza(Dimensiuni dim) =>
        new(dim.CodFunctionalId, dim.CodEconomicId, dim.SursaFinantareId,
            dim.UnitateId, dim.ProiectId, dim.CentruCostId);

    static Dictionary<Declaratii.CheieLotFapt, N.Sold> SolduriLoturi(
            IObjectSpace os, Document doc, IReadOnlyList<Guid> idsLot) => idsLot.Count == 0 ? [] :
        Cub.Citiri.Loturi.Cumulate(os, Cub.Citiri.CitireCumul.Integrala, doc.DataInregistrare, doc.ID,
                granita: doc.DataInregistrare == DateOnly.MinValue ? DateOnly.MinValue : doc.DataInregistrare.AddDays(-1))
            .Where(s => idsLot.Contains(s.LotId) && s.GestiuneId == doc.PredatorId)
            .ToList().ToDictionary(s => new Declaratii.CheieLotFapt(s.LotId, s.ContId, s.ProdusId, s.GestiuneId),
                s => new N.Sold(s.Valoare > 0m ? s.Valoare : 0m,
                    s.Valoare < 0m ? -s.Valoare : 0m, s.Cantitate, 0m));

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

    // Singura citire a operandului care se face pe ENTITATE, nu pe proiecție (S-D4):
    // materializarea declarației e înaintea commit-ului, iar motorul tocmai a pus
    // prețul și data pe lotul născut de document — o proiecție ar citi rândul din
    // bază, adică starea dinaintea operării.
    static List<(Guid Id, Guid ProdusId, DateOnly Data, decimal PretUnitar, Guid? LinieIntrareId, Guid? GestiuneId)> DateLot(
            IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Lot>()
                .Where(l => ids.Contains(l.ID))
                .ToList()
                .Select(l => (l.ID, l.ProdusId, l.Data, l.PretUnitar, l.LinieIntrareId, (Guid?)l.GestiuneId))
                .ToList();

    static Dictionary<Guid, Guid?> TipuriProdus(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Produs>()
                .Where(p => ids.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToList()
                .ToDictionary(p => p.ID, p => p.TipMaterialId);

    internal static Dictionary<Guid, Declaratii.RepartitorFapt> Repartitori(IObjectSpace os, IReadOnlyList<Guid> ids) {
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

    internal static Dictionary<Guid, Declaratii.ContFapt> Conturi(IObjectSpace os, IReadOnlyList<Guid> ids) =>
        ids.Count == 0
            ? []
            : os.GetObjectsQuery<Cont>()
                .Where(c => ids.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol, c.UrmarestePartide, c.DimensiuniObligatorii })
                .ToList()
                .Select(c => new Declaratii.ContFapt(c.ID, c.Simbol, c.UrmarestePartide,
                    c.DimensiuniObligatorii.HasFlag(DimensiuneFlags.Repartitor)))
                .ToDictionary(c => c.Id);

    static Declaratii.PoliticaTvaFapt Tva(PoliticaTva politica) =>
        politica == null
            ? null
            : new Declaratii.PoliticaTvaFapt(politica.Directie, politica.SursaContrapartida,
                politica.ContrapartidaFallbackId) { Id = politica.ID, Versiune = politica.OptimisticLockField };
}
