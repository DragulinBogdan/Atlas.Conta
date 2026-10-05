using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Comportamentul dinaintea tăierii registrelor, fixat numeric (TR-D9a pasul 2).</summary>
sealed class ScenariiTaiere(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "TAI", 1996) {
    protected override void Executa() {
        TipFaraPolitica();
        RegulaFaraConsumator();
        RegistruNegativ();
    }

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        purja.Adauga(os.GetObjectsQuery<RegulaContare>().Where(r => !r.DinSeed && r.TipDocument.Cod == "BTR"));

    void Numeroteaza(Guid doc, string numar) =>
        Comanda(os => { os.GetObjectByKey<Document>(doc).Numar = Marcaj + "-" + numar; os.CommitChanges(); });

    void TipFaraPolitica() {
        var raport = CuSpatiu(os => {
            var d = os.CreateObject<RaportProductie>(); d.Data = Ianuarie; d.PredatorId = Magazie; d.PrimitorId = Destinatie;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, Serviciu); l.Cantitate = 1; l.Valoare = 100;
            os.CommitChanges(); return d.ID;
        });
        Verifica("SC-X-25", "BPR nu declară și nu are politici", CuSpatiu(os =>
            os.GetObjectByKey<Document>(raport).Declarant() == null
            && !os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "BPR").PosteazaInCub
            && !os.GetObjectsQuery<PoliticaNumerotare>().Any(p => p.TipDocument.Cod == "BPR")));
        Verifica("SC-X-25", "BPR: dry-run fără refuz", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(raport)).Count == 0);
        Refuza("SC-X-25", () => Opereaza(raport), "politică de numerotare"); FaraEfecte("SC-X-25", raport);
        Numeroteaza(raport, "BPR");
        Opereaza(raport);
        OperatFaraEfecte("SC-X-25", "BPR cu număr cules: Operat, fără nicio postare și fără niciun rând de registru", raport);
        if (Privat) return;

        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var descarcare = Iesire(false, (lot, 2)).Id;
        Verifica("SC-DSC-09", "bugetar: DSC fără reguli și în afara cubului", CuSpatiu(os =>
            !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DSC")
            && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "DSC")
            && !os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "DSC").PosteazaInCub));
        const string faraRegula = "nu are regulă de contare de cost";
        Verifica("SC-DSC-09", "dry-run refuzat de frunză", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(descarcare))
            .Any(m => m.Contains(faraRegula)));
        Refuza("SC-DSC-09", () => Opereaza(descarcare), faraRegula); FaraEfecte("SC-DSC-09", descarcare);
        Numeroteaza(descarcare, "DSC");
        Refuza("SC-DSC-09", () => Opereaza(descarcare), faraRegula); FaraEfecte("SC-DSC-09", descarcare);
        SoldLot("SC-DSC-09", lot.Lot!.Value, Magazie, Ianuarie, 10, 100);
    }

    // D9-A4: regula pe un tip al cărui declarant nu contează prin reguli.
    void RegulaFaraConsumator() {
        var cheltuiala = Cont(Privat ? "602" : "602.01.00");
        Comanda(os => {
            var r = os.CreateObject<RegulaContare>();
            r.TipDocument = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "BTR");
            r.NaturaFiltru = NaturaClasa.Stoc;
            r.SursaContDebit = SursaCont.Explicit; r.ContDebitId = cheltuiala;
            r.SursaContCredit = SursaCont.Explicit; r.ContCreditId = Cont(Stoc);
            string refuz = null;
            try { GardianEditare.Verifica(os); }
            catch (OperareException e) { refuz = e.Message; }
            Verifica("SC-X-26", "regula de contare pe BTR e acceptată la editare" + (refuz == null ? "" : " — " + refuz), refuz == null);
            os.CommitChanges();
        });
        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var transfer = Iesire(true, (lot, 4)).Id;
        Opereaza(transfer);
        Verifica("SC-X-26", "regula produce o notă 40 numai în registru; cubul are doar mutarea, fără contul regulii", CuSpatiu(os => {
            var note = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == transfer).ToList();
            var postari = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == transfer).ToList();
            return note.Count == 1 && note[0].ContDebitId == cheltuiala && note[0].ContCreditId == Cont(Stoc) && note[0].Valoare == 40
                && postari.Count == 2 && postari.All(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Cont == Cont(Stoc));
        }));
        string semnalat = null;
        Comanda(os => { try { C.Citiri.Invarianti.Verifica(os); } catch (OperareException e) { semnalat = e.Message; } });
        Verifica("SC-X-26", "acoperirea registru → cub semnalează nota fără postare — " + semnalat,
            semnalat != null && semnalat.StartsWith("CITIRE_ISTORIC_INCOMPLET") && semnalat.Contains(transfer.ToString()));
        Anuleaza(transfer); FaraEfecte("SC-X-26", transfer);
        Comanda(os => {
            os.Delete(os.GetObjectsQuery<RegulaContare>().Where(r => !r.DinSeed && r.TipDocument.Cod == "BTR").ToList());
            os.CommitChanges();
        });
    }

    // 098-r3: registrul lotului rămâne negativ după stornarea recepției acoperite cu consum.
    void RegistruNegativ() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10));
        var nir = Opereaza(f.Id).ConexId!.Value; Opereaza(nir);
        var lot = f.Linii[0].Lot!.Value;
        Opereaza(Consum(lot, 4));
        Comanda(os => ComenziDocument.Sistem(os).Storneaza(nir, new(An, 1, 20)));
        decimal Registru() => CuSpatiu(os => os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.LotId == lot && r.RepartitorId == Magazie).Sum(r => r.Cantitate));
        Verifica("SC-X-27", $"după storno NIR: registrul lotului −4 (obținut {Registru()}), cubul 6/60", Registru() == -4);
        SoldLot("SC-X-27", lot, Magazie, new(An, 1, 20), 6, 60);
        var inainte = Consum(lot, 2);
        Verifica("SC-X-27", "consum datat înaintea stornării: dry-run fără refuz",
            CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(inainte)).Count == 0);
        Opereaza(inainte);
        Postari("SC-X-27", inainte, N.FelTranzactie.Operare, new(An, 1, 10), Iesirea(inainte, lot, f.Linii[0].Produs, 2, 20));
        var dupa = CuSpatiu(os => {
            var d = os.CreateObject<BonConsum>(); d.Data = new(An, 1, 25); d.PredatorId = Magazie; d.PrimitorId = Loc;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = 4;
            os.CommitChanges(); return d.ID;
        });
        Opereaza(dupa);
        Postari("SC-X-27", dupa, N.FelTranzactie.Operare, new(An, 1, 25), Iesirea(dupa, lot, f.Linii[0].Produs, 4, 40));
        SoldLot("SC-X-27", lot, Magazie, new(An, 1, 31), 0, 0);
        Verifica("SC-X-27", $"registrul lotului ajunge −10 (obținut {Registru()}) și nu refuză nimic", Registru() == -10);
    }

    RandScena[] Iesirea(Guid doc, Guid lot, Guid? produs, decimal q, decimal v) {
        var linie = CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == doc).ID);
        var cheltuiala = Cont(Privat ? "602" : "602.01.00");
        return [new(cheltuiala, N.Latura.Debit, v, q, Loc, lot, produs, Linie: linie, Spatiu: N.Spatiu.Stoc),
            new(Cont(Stoc), N.Latura.Credit, v, -q, Magazie, lot, produs, Linie: linie, Spatiu: N.Spatiu.Stoc)];
    }
}
