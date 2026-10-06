using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Tipul care nu declară, regula fără consumator și ieșirile după stornarea recepției (TR-D9a).</summary>
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
        const string faraDeclaratie = CoduriRefuz.TipFaraDeclaratie;
        Verifica("SC-X-25", "BPR nu declară și nu are politici", CuSpatiu(os =>
            os.GetObjectByKey<Document>(raport).Declarant() == null
            && !os.GetObjectsQuery<PoliticaNumerotare>().Any(p => p.TipDocument.Cod == "BPR")));
        Verifica("SC-X-25", $"BPR: dry-run refuzat cu {faraDeclaratie}",
            CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(raport)).Any(m => m.Contains(faraDeclaratie)));
        Refuza("SC-X-25", () => Opereaza(raport), faraDeclaratie); FaraEfecte("SC-X-25", raport);
        Numeroteaza(raport, "BPR");
        Refuza("SC-X-25", () => Opereaza(raport), faraDeclaratie); FaraEfecte("SC-X-25", raport);
        if (Privat) return;

        var lot = Receptioneaza(new LinieFctScena(10, 10)).Linii[0];
        var descarcare = Iesire(false, (lot, 2)).Id;
        Verifica("SC-DSC-09", "bugetar: DSC fără reguli", CuSpatiu(os =>
            !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DSC")
            && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "DSC")));
        Verifica("SC-DSC-09", $"dry-run refuzat cu {faraDeclaratie}", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(descarcare))
            .Any(m => m.Contains(faraDeclaratie)));
        Refuza("SC-DSC-09", () => Opereaza(descarcare), faraDeclaratie); FaraEfecte("SC-DSC-09", descarcare);
        Numeroteaza(descarcare, "DSC");
        Refuza("SC-DSC-09", () => Opereaza(descarcare), faraDeclaratie); FaraEfecte("SC-DSC-09", descarcare);
        SoldLot("SC-DSC-09", lot.Lot!.Value, Magazie, Ianuarie, 10, 100);
    }

    // D9-A4: regula pe un tip al cărui declarant nu contează prin reguli.
    void RegulaFaraConsumator() {
        const string faraConsumator = CoduriRefuz.RegulaContareFaraConsumator;
        var cheltuiala = Cont(Privat ? "602" : "602.01.00");
        RegulaContare Regula(IObjectSpace os, string tip) {
            var r = os.CreateObject<RegulaContare>();
            r.TipDocument = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == tip);
            r.NaturaFiltru = NaturaClasa.Stoc;
            r.SursaContDebit = SursaCont.Explicit; r.ContDebitId = cheltuiala;
            r.SursaContCredit = SursaCont.Explicit; r.ContCreditId = Cont(Stoc);
            return r;
        }
        static string Gard(IObjectSpace os) {
            try { GardianEditare.Verifica(os); return null; }
            catch (OperareException e) { return e.Message; }
        }
        Comanda(os => {
            Regula(os, "BTR");
            var refuz = Gard(os);
            Verifica("SC-X-26", $"regula de contare pe BTR e refuzată la editare cu {faraConsumator}" + (refuz == null ? "" : " — " + refuz),
                refuz != null && refuz.Contains(faraConsumator));
        });
        Verifica("SC-X-26", "regula refuzată nu s-a persistat", CuSpatiu(os =>
            !os.GetObjectsQuery<RegulaContare>().Any(r => !r.DinSeed && r.TipDocument.Cod == "BTR")));
        Comanda(os => {
            Regula(os, "BCS");
            var refuz = Gard(os);
            Verifica("SC-X-26", "regula de contare pe BCS (declarant care contează prin reguli) trece gardul" + (refuz == null ? "" : " — " + refuz),
                refuz == null);
        });
        Comanda(os => { Regula(os, "BTR"); os.CommitChanges(); });
        Comanda(os => {
            os.Delete(os.GetObjectsQuery<RegulaContare>().Where(r => !r.DinSeed && r.TipDocument.Cod == "BTR").ToList());
            var refuz = Gard(os);
            Verifica("SC-X-26", "ștergerea unei reguli existente pe BTR nu e refuzată de gardul consumatorului" + (refuz == null ? "" : " — " + refuz),
                refuz == null);
            os.CommitChanges();
        });
        Verifica("SC-X-26", "seed-ul nu are nicio regulă de contare pe un tip al cărui declarant nu contează prin reguli", CuSpatiu(os =>
            os.GetObjectsQuery<RegulaContare>().Where(r => r.DinSeed).Select(r => r.TipDocument.ClrType).Distinct().ToList()
                .All(clr => Contractare.DeclarantulTipului(clr) is { ConteazaPrinReguli: true })));
    }

    // 098-r3: după stornarea recepției acoperite cu consum, ieșirile ulterioare pe lot nu sunt refuzate.
    void RegistruNegativ() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10));
        var nir = Opereaza(f.Id).ConexId!.Value; Opereaza(nir);
        var lot = f.Linii[0].Lot!.Value;
        Opereaza(Consum(lot, 4));
        Comanda(os => ComenziDocument.Sistem(os).Storneaza(nir, new(An, 1, 20)));
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
    }

    RandScena[] Iesirea(Guid doc, Guid lot, Guid? produs, decimal q, decimal v) {
        var linie = CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == doc).ID);
        var cheltuiala = Cont(Privat ? "602" : "602.01.00");
        return [new(cheltuiala, N.Latura.Debit, v, q, Loc, lot, produs, Linie: linie, Spatiu: N.Spatiu.Stoc),
            new(Cont(Stoc), N.Latura.Credit, v, -q, Magazie, lot, produs, Linie: linie, Spatiu: N.Spatiu.Stoc)];
    }
}
