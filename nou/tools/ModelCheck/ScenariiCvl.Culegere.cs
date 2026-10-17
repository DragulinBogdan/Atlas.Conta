using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SC-CVL-25 și 18 pe ușile culegerii: linia de corecție culeasă prin `FacturaIntrareApply` și prin evenimentele
// ecranului XAF (`CulegereDocument`), apoi operată; corecția cu TVA ajunge la declarant (CV_STRUCTURA).
sealed partial class ScenariiCvl {
    FacturaIntrareWriteDto CorectieApi(DateOnly data, FacturaIntrareLinieWriteDto linie) {
        linie.CodEconomicId ??= Economic;
        return new() { Numar = $"{Marcaj}-C{++numar}", Data = data, PredatorId = Furnizor, PrimitorId = Magazie, Linii = { linie } };
    }

    void CulegereApi() {
        var caz = Partial();
        var candidati = CuSpatiu(os => {
            var doc = os.CreateObject<FacturaIntrare>();
            doc.PredatorId = Furnizor; doc.PrimitorId = Magazie;
            var proprii = LoturiCorectabile.Candidati(os, doc).Select(l => l.ID).ToList();
            doc.PrimitorId = Destinatie;
            var altaGestiune = LoturiCorectabile.Candidati(os, doc).Count();
            doc.PrimitorId = Magazie; doc.PredatorId = Client;
            var altFurnizor = LoturiCorectabile.Candidati(os, doc).Count();
            os.Rollback();
            return (proprii, altaGestiune, altFurnizor);
        });
        Verifica("SC-CVL-25", "lookup-ul liniei de corecție oferă loturile recepționate de la furnizor în gestiunea facturii",
            candidati.proprii.Contains(caz.Lot) && candidati.altaGestiune == 0 && candidati.altFurnizor == 0);

        var id = CuSpatiu(os => FacturaIntrareApply.Aplica(os, null,
            CorectieApi(Zi(15), new() { CorectieValoareLot = true, LotId = caz.Lot, Valoare = 200m })));
        caz.Documente.Add(id);
        var citita = CuSpatiu(os => FacturaIntrareApply.Citeste(os, id)).Linii.Single();
        var tipLot = CuSpatiu(os => os.GetObjectsQuery<Lot>().Where(l => l.ID == caz.Lot).Select(l => l.Produs.TipMaterialId).Single());
        Verifica("SC-CVL-25", "API: linia culeasă păstrează lotul și valoarea, ia tipul din produsul lotului, fără produs, cantitate și TVA implicit",
            citita.CorectieValoareLot && citita.LotId == caz.Lot && citita.Valoare == 200m && citita.Cantitate == 0m
            && citita.ProdusId == null && citita.TipTvaId == null && citita.ValoareTva == 0m && citita.TipMaterialId == tipLot);
        Opereaza(id);
        Este("SC-CVL-25", "API: corecția culeasă și operată", caz, 40m, 480m, 720m, 1200m);

        Refuza("SC-CVL-25", () => CuSpatiu(os => FacturaIntrareApply.Aplica(os, null,
            CorectieApi(Zi(16), new() { CorectieValoareLot = true, LotId = caz.Lot, Valoare = 50m, Cantitate = 1m }))), "nu poartă produs");
        var tva = CuSpatiu(os => os.GetObjectsQuery<TipTva>().First().ID);
        var cuTva = CuSpatiu(os => FacturaIntrareApply.Aplica(os, null,
            CorectieApi(Zi(16), new() { CorectieValoareLot = true, LotId = caz.Lot, Valoare = 50m, TipTvaId = tva })));
        Verifica("SC-CVL-25", "API: tipul de TVA cerut pe linia de corecție rămâne pe draft",
            CuSpatiu(os => FacturaIntrareApply.Citeste(os, cuTva)).Linii.Single().TipTvaId == tva);
        RefuzaOperarea("SC-CVL-25", cuTva, CoduriRefuz.CvStructura);
        Refuza("SC-CVL-25", () => CuSpatiu(os => FacturaIntrareApply.Aplica(os, null,
            CorectieApi(Zi(16), new() { TipMaterialId = tipLot, Cantitate = 1m, PretUnitar = 10m, LotId = caz.Lot }))), "numai pe linia de corecție");
        Refuza("SC-CVL-18", () => CuSpatiu(os => FacturaIntrareApply.Aplica(os, null,
            CorectieApi(Zi(16), new() { CorectieValoareLot = true, LotId = caz.Lot, Valoare = 200.001m }))), "zecimale");
        Este("SC-CVL-25", "refuzurile culegerii nu lasă efecte", caz, 40m, 480m, 720m, 1200m);
    }

    (FacturaIntrare, FacturaIntrareDetaliu) DocumentXaf(IObjectSpace os) {
        var doc = os.CreateObject<FacturaIntrare>();
        CulegereDocument.Nou(doc);
        var azi = doc.Data;
        doc.Data = Zi(15);
        CulegereDocument.DataMutata(doc, azi);
        doc.Numar = $"{Marcaj}-C{++numar}";
        doc.Predator = os.GetObjectByKey<Partener>(Furnizor);
        doc.Primitor = os.GetObjectByKey<Gestiune>(Magazie);
        var linie = os.CreateObject<FacturaIntrareDetaliu>();
        linie.Document = doc;
        doc.Detalii.Add(linie);
        if (Economic is Guid economic) linie.CodEconomicId = economic;
        return (doc, linie);
    }

    void CulegereXaf() {
        var caz = Partial();
        var (idXaf, obisnuita, golita) = CuSpatiu(os => {
            var lot = os.GetObjectByKey<Lot>(caz.Lot);
            var (doc, linie) = DocumentXaf(os);
            void Schimba(string proprietate) => CulegereDocument.LinieSchimbata(os, doc, linie, proprietate);
            linie.Produs = os.GetObjectByKey<Produs>(lot.ProdusId); Schimba(nameof(FacturaIntrareDetaliu.Produs));
            linie.Cantitate = 5m; Schimba(nameof(DocumentDetaliu.Cantitate));
            linie.PretUnitar = 10m; Schimba(nameof(FacturaIntrareDetaliu.PretUnitar));
            var obisnuita = linie.Valoare;
            linie.CorectieValoareLot = true; Schimba(nameof(FacturaIntrareDetaliu.CorectieValoareLot));
            var golita = (linie.ProdusId, linie.Cantitate, linie.PretUnitar, linie.TipTvaId, linie.ValoareTva, linie.Valoare);
            linie.Lot = lot; Schimba(nameof(DocumentDetaliu.Lot));
            linie.Valoare = 200m; Schimba(nameof(DocumentDetaliu.Valoare));
            CulegereDocument.InainteDeSalvare(os);
            os.CommitChanges();
            return (doc.ID, obisnuita, golita);
        });
        caz.Documente.Add(idXaf);
        var salvata = CuSpatiu(os => FacturaIntrareApply.Citeste(os, idXaf)).Linii.Single();
        Verifica("SC-CVL-25", "XAF: comutarea golește linia obișnuită culeasă (produs, cantitate, preț, TVA, valoare), iar salvarea păstrează lotul "
            + $"și valoarea (obișnuită {obisnuita}, golită {golita}, salvată {salvata.LotId == caz.Lot}/{salvata.Valoare}/{salvata.TipTvaId})",
            obisnuita >= 50m && golita == (null, 0m, 0m, null, 0m, 0m)
            && salvata.CorectieValoareLot && salvata.LotId == caz.Lot && salvata.Valoare == 200m && salvata.TipTvaId == null
            && CuSpatiu(os => !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId == salvata.Id)));
        Opereaza(idXaf);
        Este("SC-CVL-25", "XAF: corecția culeasă și operată", caz, 40m, 480m, 720m, 1200m);

        var (idTva, tvaAles) = CuSpatiu(os => {
            var (doc, linie) = DocumentXaf(os);
            void Schimba(string proprietate) => CulegereDocument.LinieSchimbata(os, doc, linie, proprietate);
            linie.CorectieValoareLot = true; Schimba(nameof(FacturaIntrareDetaliu.CorectieValoareLot));
            linie.Lot = os.GetObjectByKey<Lot>(caz.Lot); Schimba(nameof(DocumentDetaliu.Lot));
            linie.Valoare = 50m; Schimba(nameof(DocumentDetaliu.Valoare));
            var tva = os.GetObjectsQuery<TipTva>().First();
            linie.TipTva = tva; linie.TipTvaId = tva.ID; Schimba(nameof(DocumentDetaliu.TipTva));
            CulegereDocument.InainteDeSalvare(os);
            os.CommitChanges();
            return (doc.ID, tva.ID);
        });
        Verifica("SC-CVL-25", "XAF: tipul de TVA ales pe linia de corecție rămâne la salvare",
            CuSpatiu(os => FacturaIntrareApply.Citeste(os, idTva)).Linii.Single().TipTvaId == tvaAles);
        RefuzaOperarea("SC-CVL-25", idTva, CoduriRefuz.CvStructura);
        Este("SC-CVL-25", "XAF: corecția cu TVA refuzată nu lasă efecte", caz, 40m, 480m, 720m, 1200m);

        // Linia salvată obișnuită și-a născut lotul; comutată pe corecție, lotul ei nefinalizat dispare.
        var produs = CuSpatiu(os => os.GetObjectsQuery<Lot>().Where(l => l.ID == caz.Lot)
            .Select(l => new { l.ProdusId, l.Produs.TipMaterialId }).Single());
        var comutata = CuSpatiu(os => FacturaIntrareApply.Aplica(os, null, CorectieApi(Zi(16),
            new() { TipMaterialId = produs.TipMaterialId, ProdusId = produs.ProdusId, Cantitate = 5m, PretUnitar = 10m })));
        caz.Documente.Add(comutata);
        var linieComutata = CuSpatiu(os => FacturaIntrareApply.Citeste(os, comutata)).Linii.Single();
        var nascut = linieComutata.LotId;
        CuSpatiu(os => FacturaIntrareApply.Aplica(os, comutata, CorectieApi(Zi(16),
            new() { Id = linieComutata.Id, CorectieValoareLot = true, LotId = caz.Lot, Valoare = 100m })));
        var dupaComutare = CuSpatiu(os => FacturaIntrareApply.Citeste(os, comutata)).Linii.Single();
        Verifica("SC-CVL-25", "linia obișnuită salvată, comutată pe corecție: lotul ei nefinalizat dispare, linia referă lotul corectat",
            nascut != null && nascut != caz.Lot && dupaComutare.LotId == caz.Lot && dupaComutare.Valoare == 100m
            && dupaComutare.Cantitate == 0m && dupaComutare.ProdusId == null
            && CuSpatiu(os => !os.GetObjectsQuery<Lot>().Any(l => l.ID == nascut)));

        var inapoi = CuSpatiu(os => {
            var linie = os.GetObjectByKey<FacturaIntrare>(comutata).Detalii.OfType<FacturaIntrareDetaliu>().Single();
            linie.CorectieValoareLot = false;
            CulegereDocument.LinieSchimbata(os, linie.Document, linie, nameof(FacturaIntrareDetaliu.CorectieValoareLot));
            var stare = (linie.LotId, linie.Valoare);
            os.Rollback();
            return stare;
        });
        Verifica("SC-CVL-25", "XAF: comutarea înapoi pe linia obișnuită rupe lotul ales și recalculează valoarea din preț × cantitate",
            inapoi == (null, 0m));
    }

    void StructuraPeUsiNoi() {
        var caz = Partial();
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (_, doc, _) => doc.PredatorId = Client, (caz, 200m))), CoduriRefuz.CvLotNeeligibil);
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (_, doc, _) => doc.PrimitorId = Destinatie, (caz, 200m))), CoduriRefuz.CvLotNeeligibil);
        Este("SC-CVL-18", "refuzurile de furnizor și de gestiune nu lasă efecte", caz, 40m, 400m, 600m, 1000m);

        if (!Privat)
            return;
        var descarcat = Lot();
        Opereaza(Iesire(false, (new LinieScena(descarcat.Linie, descarcat.Lot, null), 10m)).Id);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (descarcat, 200m))), CoduriRefuz.CvLotNeeligibil);
    }
}
