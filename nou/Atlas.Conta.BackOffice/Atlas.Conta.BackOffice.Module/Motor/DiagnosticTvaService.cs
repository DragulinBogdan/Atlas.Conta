using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Motor;

public sealed record RandDiagnosticTva(Guid DocumentId, string Numar, Guid LinieId, Guid TipTvaId,
    string TipTvaCod, DateOnly Exigibilitate, string Registru, string Stare, string Cod, string Motiv,
    CalificareTva Calificare, CalificareTva CalificareActuala, decimal Baza, decimal Taxa);

public static class DiagnosticTvaService {
    public static IReadOnlyList<RandDiagnosticTva> Citeste(IObjectSpace os, DateOnly? deLa = null,
            DateOnly? panaLa = null, Guid? tipTvaId = null, bool includeCompensate = false,
            Guid? documentId = null, Func<object, string, bool> poateCiti = null) {
        using var citire = Fiscale.DeschideCitirea(os);
        bool Permis(object obiect, params string[] membri) => obiect != null
            && (poateCiti == null || poateCiti(obiect, null) && membri.All(m => poateCiti(obiect, m)));
        var fapteQuery = Fiscale.Fapte(os);
        var idsOperate = Fiscale.Postari(os).Where(p => p.Tranzactie.Fel != N.FelTranzactie.Storno
            && (deLa == null || p.DataExigibilitate >= deLa)
            && (panaLa == null || p.DataExigibilitate <= panaLa)
            && (tipTvaId == null || p.TipTvaId == tipTvaId)
            && (documentId == null || p.DocumentId == documentId))
            .Select(p => p.DocumentId).Distinct().ToArray().OfType<Guid>().ToArray();
        var documente = os.GetObjectsQuery<Document>().Where(d => idsOperate.Contains(d.ID)
            || d.Stare == StareDocument.Draft && (documentId == null || d.ID == documentId)).ToArray()
            .Where(d => Permis(d, nameof(Document.Stare), nameof(Document.Data), nameof(Document.Numar),
                nameof(Document.PredatorId), nameof(Document.PrimitorId)))
            .Where(d => d.Stare != StareDocument.Draft || d.CuTva()
                && IntervalTva.Contine((d as IDocumentFiscal)?.DataExigibilitate ?? d.Data, deLa, panaLa))
            .ToDictionary(d => d.ID);
        if (documente.Count == 0) return [];
        var idsDocument = documente.Keys.ToArray();
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(l => idsDocument.Contains(l.DocumentId))
            .OrderBy(l => l.ID).ToArray();
        var surseId = linii.OfType<ILinieCuAvans>().Select(l => l.LinieAvansId).OfType<Guid>().Distinct().ToArray();
        var surse = os.GetObjectsQuery<DocumentDetaliu>().Where(l => surseId.Contains(l.ID)).ToArray();
        bool LiniePermisa(DocumentDetaliu l) => Permis(l, nameof(l.DocumentId), nameof(l.TipMaterialId),
            nameof(l.TipTvaId), nameof(l.Valoare), nameof(l.ValoareTva), nameof(l.Cantitate), nameof(l.TvaCules))
            && (l is not ILinieCuAvans || Permis(l, nameof(ILinieCuAvans.LinieAvansId)))
            && (l is not ILinieCuPretUnitar || Permis(l, nameof(ILinieCuPretUnitar.PretUnitar)))
            && Permis(l, nameof(l.LotId));
        var inaccesibile = linii.Where(l => !LiniePermisa(l)).Select(l => l.DocumentId).ToHashSet();
        var liniiCitibile = linii.Concat(surse).Where(LiniePermisa).DistinctBy(l => l.ID).ToDictionary(l => l.ID);
        var tipuri = os.GetObjectsQuery<TipTva>().ToArray()
            .Where(t => Permis(t, nameof(t.Cod), nameof(t.Regim), nameof(t.Cota), nameof(t.DeImport),
                nameof(t.ValabilDeLa), nameof(t.ValabilPanaLa))).ToDictionary(t => t.ID);
        var tipuriMaterial = os.GetObjectsQuery<TipMaterial>().Where(t => t.RegularizareAvans).ToArray()
            .Where(t => Permis(t, nameof(t.RegularizareAvans))).Select(t => t.ID).ToHashSet();
        var idsSurseDoc = surse.Select(l => l.DocumentId).Distinct().ToArray();
        var docsSursa = os.GetObjectsQuery<Document>().Where(d => idsSurseDoc.Contains(d.ID)).ToArray()
            .Where(d => Permis(d, nameof(d.Stare), nameof(d.PredatorId), nameof(d.PrimitorId)))
            .ToDictionary(d => d.ID);
        var toateId = idsDocument.Concat(idsSurseDoc).Distinct().ToArray();
        var postari = Fiscale.Postari(os);
        if (poateCiti != null) {
            foreach (var p in postari.Where(p => toateId.Contains(p.DocumentId.Value)).ToArray())
                if (!Permis(p, nameof(p.ID), nameof(p.Spatiu), nameof(p.Partener), nameof(p.TranzactieId), nameof(p.DocumentId), nameof(p.LinieId),
                        nameof(p.TipTvaId), nameof(p.SensTva), nameof(p.RegimTva), nameof(p.CotaTva),
                        nameof(p.DeImport), nameof(p.DataExigibilitate), nameof(p.Valoare), nameof(p.RolTva),
                        nameof(p.Latura), nameof(p.InversaDinId), nameof(p.InversaDinSpatiu),
                        nameof(p.DocumentFiscalId), nameof(p.DataDocument), nameof(p.DataInregistrare),
                        nameof(p.PerioadaDeclarare), nameof(p.PerioadaD394))
                        || !Permis(p.Tranzactie, nameof(p.Tranzactie.Fel), nameof(p.Tranzactie.ScrisLa)))
                    if (p.DocumentId is Guid ascuns) inaccesibile.Add(ascuns);
        }
        toateId = toateId.Where(id => !inaccesibile.Contains(id)).ToArray();
        var compensate = (from original in postari
                          join inversa in postari on new { Id = (Guid?)original.ID, Spatiu = (N.Spatiu?)original.Spatiu }
                              equals new { Id = inversa.InversaDinId, Spatiu = inversa.InversaDinSpatiu }
                          where toateId.Contains(original.DocumentId.Value)
                          select new { original.TranzactieId, original.LinieId }).ToArray()
            .Select(p => (p.TranzactieId, p.LinieId.Value)).ToHashSet();
        var fapte = fapteQuery.Where(f => toateId.Contains(f.DocumentId) && !f.Storno).ToArray();
        var fapteSurse = fapte.Where(f => surseId.Contains(f.DetaliuId)).GroupBy(f => f.DetaliuId)
            .Where(g => g.Count() == 1).ToDictionary(g => g.Key, g => g.Single());
        SursaDiagnosticTva Sursa(Guid? id) {
            if (id is not Guid linie || !liniiCitibile.TryGetValue(linie, out var l)
                    || !fapteSurse.TryGetValue(linie, out var f) || !docsSursa.TryGetValue(l.DocumentId, out var d)
                    || d.Stare == StareDocument.Draft || inaccesibile.Contains(d.ID)) return null;
            return new(f.DocumentId, f.PartenerId, f.Sens, Calificare(f), f.DataExigibilitate,
                l is ILinieCuAvans && tipuriMaterial.Contains(l.TipMaterialId) && f.Baza > 0m,
                compensate.Contains((f.TranzactieId, f.DetaliuId)));
        }
        TipDiagnosticTva Actual(Guid id) => tipuri.TryGetValue(id, out var t)
            ? new(new(t.Regim, t.Cota, t.DeImport), t.ValabilDeLa, t.ValabilPanaLa) : null;
        var mapari = Saft.SaftProiectii.SeAplica(os) ? new Saft.MapariFiscale(os) : null;
        var rezultat = new List<RandDiagnosticTva>();
        foreach (var doc in documente.Values.Where(d => !inaccesibile.Contains(d.ID))) {
            var date = new List<(LinieDiagnosticTva Linie, Guid Tip, bool Compensata)>();
            if (doc.Stare == StareDocument.Draft) {
                if (doc is IDocumentFiscal && !Permis(doc, nameof(IDocumentFiscal.DataExigibilitate))) continue;
                var td = MotorOperare.GasesteTipDocument(os, doc);
                var politica = os.GetObjectsQuery<PoliticaTva>().FirstOrDefault(p => p.TipDocumentId == td.ID);
                if (politica == null) continue;
                var sens = politica.Directie == DirectieTva.Deductibil ? SensTva.Achizitie : SensTva.Livrare;
                Guid? partener = politica.SursaContrapartida switch {
                    SursaCont.RepartitorPredator => doc.PredatorId,
                    SursaCont.RepartitorPrimitor => doc.PrimitorId, _ => null };
                foreach (var l in linii.Where(l => l.DocumentId == doc.ID && doc.LinieFiscala(l))) {
                    if (l.TipTvaId is not Guid tip || Actual(tip) is not { } actual) continue;
                    var baza = Scara.RotunjesteBani(doc.BazaLinie(os, l) ?? l.Valoare);
                    if (doc.SemnulEAlOperarii()) baza = -Math.Abs(baza);
                    var referinta = (l as ILinieCuAvans)?.LinieAvansId;
                    date.Add((new(l.ID, doc.ID, partener, sens, actual.Calificare,
                        (doc as IDocumentFiscal)?.DataExigibilitate ?? doc.Data, baza,
                        doc.SemnulEAlOperarii() ? -Math.Abs(l.ValoareTva) : l.ValoareTva,
                        !l.TvaCules, l is ILinieCuAvans && tipuriMaterial.Contains(l.TipMaterialId),
                        doc.SemnulEAlOperarii() || l is ILinieCuAvans && baza < 0m,
                        referinta, Sursa(referinta), actual), tip, false));
                }
            } else {
                foreach (var f in fapte.Where(f => f.DocumentId == doc.ID).OrderBy(f => f.DetaliuId)) {
                    if (!liniiCitibile.TryGetValue(f.DetaliuId, out var l)) continue;
                    var referinta = (l as ILinieCuAvans)?.LinieAvansId;
                    date.Add((new(l.ID, f.DocumentId, f.PartenerId, f.Sens, Calificare(f), f.DataExigibilitate,
                        f.Baza, f.Tva, false, l is ILinieCuAvans && tipuriMaterial.Contains(l.TipMaterialId),
                        doc.SemnulEAlOperarii() || l is ILinieCuAvans && f.Baza < 0m,
                        referinta, Sursa(referinta), Actual(f.TipTvaId)), f.TipTvaId,
                        compensate.Contains((f.TranzactieId, f.DetaliuId))));
                }
            }
            if (doc.Stare == StareDocument.Draft && date.Count > 0) {
                var sens = date[0].Linie.Sens;
                var calcul = N.Tva.PeDocument(date.Select(d => new N.LinieTva(d.Linie.Id, d.Linie.Baza,
                    (N.RegimTva)d.Linie.Calificare.Regim, d.Linie.Calificare.Cota)).ToArray(),
                    sens == SensTva.Achizitie ? N.DirectieTva.Deductibil : N.DirectieTva.Colectat,
                    new N.Rotunjire(Scara.ConventieBani));
                date = date.Select(d => (d.Linie.Automata ? d.Linie with { Taxa = calcul.PerLinie[d.Linie.Id] } : d.Linie,
                    d.Tip, d.Compensata)).ToList();
            }
            var avertismente = DiagnosticTva.Verifica(date.Select(d => d.Linie).ToArray(), Scara.ConventieBani).ToList();
            foreach (var d in date) {
                if (mapari == null) break;
                var l = d.Linie;
                var fapt = new FaptFiscal { TipTvaId = d.Tip, Regim = l.Calificare.Regim, Cota = l.Calificare.Cota,
                    DeImport = l.Calificare.DeImport, Sens = l.Sens };
                var lipsuri = new List<string>();
                if (mapari.Pentru(fapt, SectiuneTvaSaft.Facturi) == null) lipsuri.Add("Facturi");
                if (l.Taxa != 0m && mapari.Pentru(fapt, SectiuneTvaSaft.GeneralLedger) == null)
                    lipsuri.Add("GeneralLedger/Taxa");
                if (l.Taxa != 0m && l.Calificare.Regim == RegimTva.TaxareInversa
                        && l.Sens == SensTva.Achizitie
                        && mapari.Pentru(fapt, SectiuneTvaSaft.GeneralLedger, N.RolTva.Autocolectare) == null)
                    lipsuri.Add("GeneralLedger/Autocolectare");
                if (lipsuri.Count > 0)
                    avertismente.Add(new(l.Id, "TipTvaFaraCodSaft",
                        $"Lipsește maparea SAF-T pentru calificarea și sensul folosite ({string.Join(", ", lipsuri)})."));
            }
            foreach (var d in date.Where(d => (tipTvaId == null || d.Tip == tipTvaId)
                    && IntervalTva.Contine(d.Linie.Exigibilitate, deLa, panaLa)
                    && (includeCompensate || !d.Compensata))) {
                var l = d.Linie;
                var avertismenteLinie = avertismente.Where(a => a.LinieId == l.Id).ToArray();
                foreach (var a in avertismenteLinie)
                    rezultat.Add(new(doc.ID, doc.Numar, l.Id, d.Tip, tipuri.GetValueOrDefault(d.Tip)?.Cod,
                        l.Exigibilitate, l.Sens == SensTva.Livrare ? "Emise" : "Primite-Vamă",
                        d.Compensata ? "Compensat" : doc.Stare.ToString(), a.Cod, a.Motiv,
                        l.Calificare, l.TipActual?.Calificare, l.Baza, l.Taxa));
            }
        }
        return rezultat;
    }

    static CalificareTva Calificare(FaptFiscal f) => new(f.Regim, f.Cota, f.DeImport);
}
