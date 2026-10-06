using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using Atlas.Conta.BackOffice.Module.Proiectii;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiTrezorerie(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "TRZ", 2003) {
    Guid casa;
    string Numerar => Privat ? "5311" : "531.01.01";
    string ContTert(bool inc) => inc ? ContClient : ContFurnizor;
    string Id(bool inc, string nr) => $"SC-{(inc ? "INC" : "PLT")}-{nr}";

    RandScena[] Randuri(FacturaScena f, bool inc, params decimal[] sume) => sume.SelectMany((v, i) => new[] {
        new RandScena(Cont(ContTert(inc)), inc ? N.Latura.Credit : N.Latura.Debit, v,
            Unitate: Partida(f.Id, ContTert(inc), inc ? Client : Furnizor), Partener: inc ? Client : Furnizor,
            Linie: f.Linii[i].Id, Economic: Economic),
        new RandScena(Cont(Numerar), inc ? N.Latura.Debit : N.Latura.Credit, v,
            Gestiune: casa, Linie: f.Linii[i].Id, Economic: Economic)
    }).ToArray();

    protected override void Executa() {
        casa = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID);
        foreach (var inc in new[] { false, true }) CicluDeschis(inc);
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var p1 = Trezorerie(false, 40); Opereaza(p1.Id);
        var p2 = Trezorerie(false, 60); Opereaza(p2.Id);
        var amprenta = Amprenta(f.Id);
        Refuza("SC-X-04", () => Imperecheaza(p1.Id, f.Id, 101, Ianuarie), "depăș");
        Verifica("SC-X-04", "depășirea nu creează împerechere", CuSpatiu(os =>
            !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentId == f.Id)) && Amprenta(f.Id) == amprenta);
        Imperecheaza(p1.Id, f.Id, 40, Ianuarie);
        TransferPartida("SC-X-04", p1.Id, f.Id, ContFurnizor, Furnizor, N.Latura.Debit, 40, Ianuarie);
        SoldPartida("SC-X-04", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -60);
        Imperecheaza(p2.Id, f.Id, 60, Ianuarie);
        TransferPartida("SC-X-04", p2.Id, f.Id, ContFurnizor, Furnizor, N.Latura.Debit, 60, Ianuarie);
        SoldPartida("SC-X-04", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, 0);
        var nemodificat = Amprenta(p2.Id);
        Refuza("SC-X-04", () => Storneaza(p2.Id, Ianuarie), "imperecheri");
        Verifica("SC-X-04", "storno refuzat nu scrie", Amprenta(p2.Id) == nemodificat);
        var peste = new[] { Trezorerie(false, 100), Trezorerie(true, 100) };
        var corectate = new[] { Trezorerie(false, 100), Trezorerie(true, 100) };
        foreach (var d in peste.Concat(corectate)) Opereaza(d.Id);
        InchideIanuarie();
        Storneaza(p2.Id, Februarie);
        TransferPartida("SC-X-04", p2.Id, f.Id, ContFurnizor, Furnizor, N.Latura.Debit, -60, Februarie);
        Postari("SC-X-04", p2.Id, N.FelTranzactie.Storno, Februarie, Randuri(p2, false, -60));
        TransferPeCititori("SC-CIT-101", "Partide.Postari", 6, 0, 6, p1.Id, p2.Id);
        ListareTransfer("SC-CIT-107", "Partide.Postari", p1.Id, p2.Id);
        Verifica("SC-CIT-97", "transferul împerecherii nu are explicație; operarea plății o are pe a ei",
            Tranzactii(p1.Id).Where(t => t.Fel == N.FelTranzactie.Transfer).All(t => !t.Explicata && t.Din == null)
            && Explicatia(p1.Id, N.FelTranzactie.Transfer).Origini.Count == 0
            && Explicatia(p1.Id, N.FelTranzactie.Operare).Origini.Count == 1);
        Verifica("SC-X-04", "factura nu este rescrisă; împerecherea netă rămasă40", Amprenta(f.Id) == amprenta
            && CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentId == f.Id).Sum(i => i.Suma)) == 40);
        {
            SoldPartida("SC-X-04", Partida(f.Id, ContFurnizor)!.Value, new(An, 1, 31), 0);
            SoldPartida("SC-X-04", Partida(f.Id, ContFurnizor)!.Value, Februarie, -60);
            SoldPartida("SC-X-04", Partida(p1.Id, ContFurnizor)!.Value, Februarie, 0);
            SoldPartida("SC-X-04", Partida(p2.Id, ContFurnizor)!.Value, Februarie, 0);
        }
        for (var i = 0; i < 2; i++) PestePerioada(peste[i], corectate[i], i == 1);
        RepartitorPeTert();
    }

    // D9-A10: piciorul de terț al contului care cere repartitor poartă partenerul și fără partidă;
    // contul fără flag și fără partide rămâne fără coordonată.
    void RepartitorPeTert() {
        var faraFlag = Cont(Privat ? "462" : "462.01.03");
        var cuFlag = Cont(Privat ? "462" : "462.01.09");
        var flags = CuSpatiu(os => os.GetObjectByKey<Cont>(cuFlag).DimensiuniObligatorii);
        Guid Creditor(Guid cont) => CuSpatiu(os => {
            var p = os.CreateObject<Partener>();
            p.Cod = Marcaj + "-CRED-" + os.GetObjectByKey<Cont>(cont).Simbol; p.Denumire = p.Cod; p.ContImplicitId = cont;
            os.CommitChanges(); return p.ID;
        });
        FacturaScena Plata(Guid creditor) => CuSpatiu(os => {
            var doc = os.CreateObject<Plata>();
            doc.Data = Februarie; doc.PredatorId = casa; doc.PrimitorId = creditor;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc;
            l.Pozitie = 1; l.TipMaterialId = Tip(os, "TRZ"); l.Valoare = 100; l.CodEconomicId = Economic;
            os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, null, null)]);
        });
        var liber = Creditor(faraFlag);
        var p1 = Plata(liber); Opereaza(p1.Id);
        Postari("SC-PLT-08", p1.Id, N.FelTranzactie.Operare, Februarie,
            new RandScena(faraFlag, N.Latura.Debit, 100, Linie: p1.Linii[0].Id, Economic: Economic),
            new RandScena(Cont(Numerar), N.Latura.Credit, 100, Gestiune: casa, Linie: p1.Linii[0].Id, Economic: Economic));
        try {
            if (Privat) Comanda(os => { os.GetObjectByKey<Cont>(cuFlag).DimensiuniObligatorii = flags | DimensiuneFlags.Repartitor; os.CommitChanges(); });
            var creditor = Creditor(cuFlag);
            var p2 = Plata(creditor); Opereaza(p2.Id);
            Postari("SC-PLT-08", p2.Id, N.FelTranzactie.Operare, Februarie,
                new RandScena(cuFlag, N.Latura.Debit, 100, Partener: creditor, Linie: p2.Linii[0].Id, Economic: Economic),
                new RandScena(Cont(Numerar), N.Latura.Credit, 100, Gestiune: casa, Linie: p2.Linii[0].Id, Economic: Economic));
            Verifica("SC-PLT-08", "pe cititori: repartitorul piciorului de terț e creditorul, fără partidă deschisă; "
                + "pe contul fără flag repartitorul lipsește", CuSpatiu(os => {
                    var tert = CubScena.Note(os, p2.Id).Single(p => p.Cont == cuFlag);
                    var liberul = CubScena.Note(os, p1.Id).Single(p => p.Cont == faraFlag);
                    var partide = C.Citiri.Partide.Postari(os).Count(p => p.DocumentId == p2.Id || p.DocumentId == p1.Id);
                    var balanta = ContabilProiectii.Balanta(os, Februarie, Februarie, analitic: true, repartitorId: creditor).ToList();
                    return tert.Repartitor == creditor && tert.Unitate == null && liberul.Repartitor == null && partide == 0
                        && balanta.Count == 1 && balanta[0].ContId == cuFlag && balanta[0].RulajDebit == 100;
                }));
        }
        finally { if (Privat) Comanda(os => { os.GetObjectByKey<Cont>(cuFlag).DimensiuniObligatorii = flags; os.CommitChanges(); }); }
    }

    void CicluDeschis(bool inc) {
        var f = Trezorerie(inc, 100);
        Verifica(Id(inc, "01"), "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(f.Id)).Count == 0);
        FaraEfecte(Id(inc, "01"), f.Id); Opereaza(f.Id);
        Postari(Id(inc, "01"), f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, inc, 100));
        SoldPartida(Id(inc, "01"), Partida(f.Id, ContTert(inc), inc ? Client : Furnizor)!.Value, Ianuarie, inc ? -100 : 100);
        var m = Trezorerie(inc, 40, 60); Opereaza(m.Id);
        Postari(Id(inc, "02"), m.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(m, inc, 40, 60));
        Storneaza(f.Id, new(An, 1, 20));
        Postari(Id(inc, "03"), f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, inc, 100));
        Postari(Id(inc, "03"), f.Id, N.FelTranzactie.Storno, new(An, 1, 20), Randuri(f, inc, -100));
        SoldPartida(Id(inc, "03"), Partida(f.Id, ContTert(inc), inc ? Client : Furnizor)!.Value, new(An, 1, 20), 0);
        var inainte = Amprenta(f.Id);
        Refuza(Id(inc, "03"), () => Storneaza(f.Id, new(An, 1, 20)), "Operat");
        Verifica(Id(inc, "03"), "repetarea nu scrie", Amprenta(f.Id) == inainte);
        Anuleaza(m.Id); FaraEfecte(Id(inc, "05"), m.Id);
        var invalid = Trezorerie(inc, 0);
        RefuzDeclaratie(Id(inc, "07"), invalid.Id, CoduriRefuz.ValoareNepozitiva);
        Refuza(Id(inc, "07"), () => Opereaza(invalid.Id), "pozitiv");
        FaraEfecte(Id(inc, "07"), invalid.Id);
    }

    void PestePerioada(FacturaScena f, FacturaScena c, bool inc) {
        Refuza(Id(inc, "04"), () => Anuleaza(f.Id), "închis");
        Storneaza(f.Id, Februarie);
        Postari(Id(inc, "04"), f.Id, N.FelTranzactie.Storno, Februarie, Randuri(f, inc, -100));
        {
            SoldPartida(Id(inc, "04"), Partida(f.Id, ContTert(inc), inc ? Client : Furnizor)!.Value, new(An, 1, 31), inc ? -100 : 100);
            SoldPartida(Id(inc, "04"), Partida(f.Id, ContTert(inc), inc ? Client : Furnizor)!.Value, Februarie, 0);
        }
        var nou = Corecteaza(c.Id); FaraEfecte(Id(inc, "06"), nou);
        FacturaScena corectie;
        using (var os = Deschide()) {
            var d = os.GetObjectsQuery<DocumentTrezorerie>().Single(d => d.ID == nou);
            Verifica(Id(inc, "06"), "corecție legată și datată", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<DocumentTrezorerieDetaliu>().Single(l => l.DocumentId == nou);
            l.Valoare = 80; corectie = new(nou, [new(l.ID, null, null)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari(Id(inc, "06"), nou, N.FelTranzactie.Operare, Februarie, Randuri(corectie, inc, 80));
        Postari(Id(inc, "06"), c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, inc, -100));
        Postari(Id(inc, "06"), c.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(c, inc, 100));
        SoldPartida(Id(inc, "06"), Partida(nou, ContTert(inc), inc ? Client : Furnizor)!.Value, Februarie, inc ? -80 : 80);
    }
}
