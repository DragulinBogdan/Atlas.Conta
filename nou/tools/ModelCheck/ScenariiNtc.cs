using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiNtc(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "NTC", 2006) {
    int numarPartener;
    Guid PartenerNou() => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-TERT-" + ++numarPartener;
        p.Denumire = p.Cod; os.CommitChanges(); return p.ID;
    });
    Guid P(Guid doc, string cont, Guid tert) => N.Unitate.DeschidePartida(Cont(cont), tert, doc, Ianuarie).Id;
    RandScena[] R(FacturaScena d, int i, string debit, string credit, decimal v,
            Guid? ud = null, Guid? uc = null, Guid? pd = null, Guid? pc = null) => [
        new(Cont(debit), N.Latura.Debit, v, Gestiune: pd == null ? Loc : null,
            Unitate: ud, Partener: pd, Linie: d.Linii[i].Id, Economic: Economic),
        new(Cont(credit), N.Latura.Credit, v, Gestiune: pc == null ? Loc : null,
            Unitate: uc, Partener: pc, Linie: d.Linii[i].Id, Economic: Economic)];
    FacturaScena Fct(decimal v, int zi) {
        var f = Factura(new(An, 1, zi), new LinieFctScena(1, v, Stoc: false)); Opereaza(f.Id); return f;
    }

    protected override void Executa() {
        var d = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100));
        Verifica("SC-NTC-01", "validare fără refuz", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-NTC-01", d.Id); Opereaza(d.Id);
        Postari("SC-NTC-01", d.Id, N.FelTranzactie.Operare, Ianuarie, R(d, 0, Serviciu, ContFurnizor, 100));
        Storneaza(d.Id, new(An, 1, 20));
        Postari("SC-NTC-03", d.Id, N.FelTranzactie.Storno, new(An, 1, 20), R(d, 0, Serviciu, ContFurnizor, -100));
        var amprenta = Amprenta(d.Id);
        Refuza("SC-NTC-03", () => Storneaza(d.Id, new(An, 1, 20)), "Operat");
        Verifica("SC-NTC-03", "storno repetat fără efecte", Amprenta(d.Id) == amprenta);
        var a = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100)); Opereaza(a.Id); Anuleaza(a.Id);
        FaraEfecte("SC-NTC-05", a.Id);
        var m = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100), new LinieNtcScena(Serviciu, ContFurnizor, -25), new LinieNtcScena(ContFurnizor, ContFurnizor, 40));
        Opereaza(m.Id);
        Postari("SC-NTC-02", m.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. R(m, 0, Serviciu, ContFurnizor, 100), .. R(m, 1, Serviciu, ContFurnizor, -25), .. R(m, 2, ContFurnizor, ContFurnizor, 40)]);
        var stoc = Nota(Ianuarie, new LinieNtcScena(Serviciu, Stoc, 50)); Opereaza(stoc.Id);
        Postari("SC-NTC-14", stoc.Id, N.FelTranzactie.Operare, Ianuarie, R(stoc, 0, Serviciu, Stoc, 50));
        Verifica("SC-NTC-14", "fără registru de stoc", CuSpatiu(os => !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == stoc.Id)));
        Refuzuri();
        Fifo(); Parteneri(); Compatibilitate(); DependentaInTimp();
        if (Privat) Avans();
        PestePerioada();
    }

    void Refuzuri() {
        var fara = Nota(Ianuarie);
        var zero = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 0));
        var cont = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 10));
        Comanda(os => { os.GetObjectsQuery<NotaContabilaDetaliu>().Single(l => l.DocumentId == cont.Id).ContDebitId = null; os.CommitChanges(); });
        var externul = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 10));
        Comanda(os => { os.GetObjectByKey<Document>(externul.Id).PredatorId = Furnizor; os.CommitChanges(); });
        foreach (var (doc, cod, mesaj) in new[] { (fara, "LINII_LIPSA", "linie"), (zero, "VALOARE_ZERO", "nenulă"),
                     (cont, "CONT_EXPLICIT_LIPSA", "cont"), (externul, "PREDATOR_NEPOTRIVIT", "PREDATOR_NEPOTRIVIT") }) {
            RefuzDeclaratie("SC-NTC-18", doc.Id, cod);
            Refuza("SC-NTC-18", () => Opereaza(doc.Id), mesaj); FaraEfecte("SC-NTC-18", doc.Id);
        }
    }

    void Fifo() {
        Furnizor = PartenerNou();
        var f1 = Fct(60, 3); var f2 = Fct(40, 4);
        var p1 = P(f1.Id, ContFurnizor, Furnizor); var p2 = P(f2.Id, ContFurnizor, Furnizor);
        var n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 75, Furnizor));
        Verifica("SC-NTC-07", "dry-run FIFO acceptat", CuSpatiu(os => OperareApi.Valideaza(os, n.Id)).Count == 0);
        FaraEfecte("SC-NTC-07", n.Id); Opereaza(n.Id);
        Postari("SC-NTC-07", n.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. R(n, 0, ContFurnizor, Serviciu, 60, ud: p1, pd: Furnizor), .. R(n, 0, ContFurnizor, Serviciu, 15, ud: p2, pd: Furnizor)]);
        SoldPartida("SC-NTC-07", p1, Ianuarie, 0); SoldPartida("SC-NTC-07", p2, Ianuarie, -25);
        var intact = Amprenta(f1.Id);
        Refuza("SC-NTC-16", () => Anuleaza(f1.Id), "PARTIDA_CU_DEPENDENTI");
        Refuza("SC-NTC-16", () => Storneaza(f1.Id, Ianuarie), "PARTIDA_CU_DEPENDENTI");
        Verifica("SC-NTC-16", "refuz atomic", Amprenta(f1.Id) == intact);
        Imperecheaza(n.Id, f1.Id, 60, Ianuarie);
        Verifica("SC-NTC-17", "nominalizarea nu este dublată de transfer", CuSpatiu(os =>
            !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == n.Id && t.Fel == N.FelTranzactie.Transfer)));
        SoldPartida("SC-NTC-17", p1, Ianuarie, 0);
        var imp = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == n.Id).ID);
        Comanda(os => ImperechereService.Desfa(os, imp, Ianuarie));
        Storneaza(n.Id, Ianuarie); SoldPartida("SC-NTC-16", p1, Ianuarie, -60);
        Storneaza(f1.Id, Ianuarie); SoldPartida("SC-NTC-16", p1, Ianuarie, 0);

        Furnizor = PartenerNou(); f1 = Fct(60, 3); f2 = Fct(40, 4);
        p1 = P(f1.Id, ContFurnizor, Furnizor); p2 = P(f2.Id, ContFurnizor, Furnizor);
        n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 70, Furnizor), new LinieNtcScena(ContFurnizor, Serviciu, 50, Furnizor)); Opereaza(n.Id);
        var proprie = P(n.Id, ContFurnizor, Furnizor);
        Postari("SC-NTC-08", n.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. R(n, 0, ContFurnizor, Serviciu, 60, ud: p1, pd: Furnizor), .. R(n, 0, ContFurnizor, Serviciu, 10, ud: p2, pd: Furnizor),
             .. R(n, 1, ContFurnizor, Serviciu, 30, ud: p2, pd: Furnizor), .. R(n, 1, ContFurnizor, Serviciu, 20, ud: proprie, pd: Furnizor)]);
        SoldPartida("SC-NTC-08", p1, Ianuarie, 0); SoldPartida("SC-NTC-08", p2, Ianuarie, 0); SoldPartida("SC-NTC-08", proprie, Ianuarie, 20);

        Furnizor = PartenerNou(); f1 = Fct(60, 3); f2 = Fct(40, 10);
        n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 75, Furnizor)); Opereaza(n.Id);
        Postari("SC-NTC-09", n.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. R(n, 0, ContFurnizor, Serviciu, 60, ud: P(f1.Id, ContFurnizor, Furnizor), pd: Furnizor),
             .. R(n, 0, ContFurnizor, Serviciu, 15, ud: P(n.Id, ContFurnizor, Furnizor), pd: Furnizor)]);
        SoldPartida("SC-NTC-09", P(f2.Id, ContFurnizor, Furnizor), new(An, 1, 10), -40);
        Furnizor = PartenerNou(); f1 = Fct(60, 3); f2 = Fct(40, 3);
        var prima = new[] { (P(f1.Id, ContFurnizor, Furnizor), 60m), (P(f2.Id, ContFurnizor, Furnizor), 40m) }.OrderBy(p => p.Item1).First();
        n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 30, Furnizor)); Opereaza(n.Id);
        Postari("SC-NTC-09", n.Id, N.FelTranzactie.Operare, Ianuarie, R(n, 0, ContFurnizor, Serviciu, 30, ud: prima.Item1, pd: Furnizor));

        Furnizor = PartenerNou(); f1 = Fct(100, 3); p1 = P(f1.Id, ContFurnizor, Furnizor);
        n = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, -30, RepartitorCredit: Furnizor)); Opereaza(n.Id);
        Postari("SC-NTC-10", n.Id, N.FelTranzactie.Operare, Ianuarie, R(n, 0, Serviciu, ContFurnizor, -30, uc: p1, pc: Furnizor));
        SoldPartida("SC-NTC-10", p1, Ianuarie, -70);
        Furnizor = PartenerNou(); f1 = Fct(100, 3); p1 = P(f1.Id, ContFurnizor, Furnizor);
        n = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 20, RepartitorCredit: Furnizor)); Opereaza(n.Id);
        SoldPartida("SC-NTC-11", p1, Ianuarie, -100); SoldPartida("SC-NTC-11", P(n.Id, ContFurnizor, Furnizor), Ianuarie, -20);
        Postari("SC-NTC-11", n.Id, N.FelTranzactie.Operare, Ianuarie, R(n, 0, Serviciu, ContFurnizor, 20, uc: P(n.Id, ContFurnizor, Furnizor), pc: Furnizor));
        var fara = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 100)); Opereaza(fara.Id);
        Postari("SC-NTC-15", fara.Id, N.FelTranzactie.Operare, Ianuarie, R(fara, 0, ContFurnizor, Serviciu, 100));
        SoldPartida("SC-NTC-15", p1, Ianuarie, -100);
    }

    void DependentaInTimp() {
        Furnizor = PartenerNou();
        var f = Fct(100, 3); var partida = P(f.Id, ContFurnizor, Furnizor);
        var n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 40, Furnizor)); Opereaza(n.Id);
        var original = Amprenta(f.Id);
        Refuza("SC-NTC-16", () => Anuleaza(f.Id), "PARTIDA_CU_DEPENDENTI");
        Refuza("SC-NTC-16", () => Storneaza(f.Id, Ianuarie), "PARTIDA_CU_DEPENDENTI");
        Verifica("SC-NTC-16", "FCT100 cu nominalizare40: refuz atomic", Amprenta(f.Id) == original);
        var imp = Imperecheaza(n.Id, f.Id, 40, Ianuarie);
        Verifica("SC-NTC-17", "nominalizarea40 nu produce transfer suplimentar", CuSpatiu(os =>
            !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == n.Id && t.Fel == N.FelTranzactie.Transfer)));
        SoldPartida("SC-NTC-17", partida, Ianuarie, -60);
        Comanda(os => ImperechereService.Desfa(os, imp, Ianuarie));
        Storneaza(n.Id, new(An, 1, 20));
        SoldPartida("SC-NTC-16", partida, new(An, 1, 20), -100);
        var intact = Amprenta(f.Id);
        Refuza("SC-NTC-22", () => Storneaza(f.Id, new(An, 1, 10)), "PARTIDA_CU_DEPENDENTI");
        Verifica("SC-NTC-22", "refuz atomic înainte de inversarea dependentului", Amprenta(f.Id) == intact);
        SoldPartida("SC-NTC-22", partida, new(An, 1, 10), -60);
        Storneaza(f.Id, new(An, 1, 20));
        SoldPartida("SC-NTC-22", partida, new(An, 1, 20), 0);
    }

    void Avans() {
        var tert = PartenerNou();
        var s = Nota(Ianuarie, new LinieNtcScena("4111", "419", 100, tert, tert)); Opereaza(s.Id);
        var creanta = P(s.Id, "4111", tert); var avans = P(s.Id, "419", tert);
        Postari("SC-NTC-12", s.Id, N.FelTranzactie.Operare, Ianuarie, R(s, 0, "4111", "419", 100, creanta, avans, tert, tert));
        var n = Nota(Ianuarie, new LinieNtcScena("419", "4111", 60, tert, tert)); Opereaza(n.Id);
        Postari("SC-NTC-12", n.Id, N.FelTranzactie.Operare, Ianuarie, R(n, 0, "419", "4111", 60, avans, creanta, tert, tert));
        SoldPartida("SC-NTC-12", creanta, Ianuarie, 40); SoldPartida("SC-NTC-12", avans, Ianuarie, -40);
        Storneaza(n.Id, Ianuarie);
        Postari("SC-NTC-12", n.Id, N.FelTranzactie.Storno, Ianuarie, R(n, 0, "419", "4111", -60, avans, creanta, tert, tert));
        SoldPartida("SC-NTC-12", creanta, Ianuarie, 100); SoldPartida("SC-NTC-12", avans, Ianuarie, -100);
    }

    void Parteneri() {
        var pa = PartenerNou(); var pb = PartenerNou();
        var n = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100, RepartitorCredit: pa), new LinieNtcScena(Serviciu, ContFurnizor, 100, RepartitorCredit: pb)); Opereaza(n.Id);
        var a = P(n.Id, ContFurnizor, pa); var b = P(n.Id, ContFurnizor, pb);
        Verifica("SC-NTC-13", "parteneri diferiți, identități diferite", a != b);
        Postari("SC-NTC-13", n.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. R(n, 0, Serviciu, ContFurnizor, 100, uc: a, pc: pa), .. R(n, 1, Serviciu, ContFurnizor, 100, uc: b, pc: pb)]);
        var stingere = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 100, pa)); Opereaza(stingere.Id);
        SoldPartida("SC-NTC-13", a, Ianuarie, 0); SoldPartida("SC-NTC-13", b, Ianuarie, -100);
        var proprie = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100, RepartitorCredit: PartenerNou())); Opereaza(proprie.Id); Anuleaza(proprie.Id);
        FaraEfecte("SC-NTC-05", proprie.Id);
    }

    void PestePerioada() {
        var p = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100)); Opereaza(p.Id);
        var c = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100)); Opereaza(c.Id);
        var tarziu = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100));
        Comanda(os => { os.GetObjectByKey<Document>(tarziu.Id).DataInregistrare = Februarie; os.CommitChanges(); });
        InchideIanuarie(); Refuza("SC-NTC-04", () => Anuleaza(p.Id), "închis");
        Storneaza(p.Id, Februarie);
        Postari("SC-NTC-04", p.Id, N.FelTranzactie.Storno, Februarie, R(p, 0, Serviciu, ContFurnizor, -100));
        NetDocument("SC-NTC-04", p.Id, new(An, 1, 31), 100); NetDocument("SC-NTC-04", p.Id, Februarie, 0);
        var nou = Corecteaza(c.Id); FaraEfecte("SC-NTC-06", nou);
        var corectie = CuSpatiu(os => {
            var doc = os.GetObjectByKey<Document>(nou);
            Verifica("SC-NTC-06", "legătură, motiv, date", doc.CorecteazaId == c.Id && doc.MotivCorectie == MotivCorectie.EroareMateriala
                && doc.Data == Ianuarie && doc.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<NotaContabilaDetaliu>().Single(l => l.DocumentId == nou); l.Valoare = 80;
            os.CommitChanges(); return new FacturaScena(nou, [new(l.ID, null, null)]);
        });
        Opereaza(nou);
        Postari("SC-NTC-06", c.Id, N.FelTranzactie.Storno, Februarie, R(c, 0, Serviciu, ContFurnizor, -100));
        Postari("SC-NTC-06", nou, N.FelTranzactie.Operare, Februarie, R(corectie, 0, Serviciu, ContFurnizor, 80));
        NetDocument("SC-NTC-06", c.Id, new(An, 1, 31), 100); NetDocument("SC-NTC-06", c.Id, Februarie, 0);
        NetDocument("SC-NTC-06", nou, Februarie, 80);
        Opereaza(tarziu.Id);
        Postari("SC-NTC-19", tarziu.Id, N.FelTranzactie.Operare, Februarie, R(tarziu, 0, Serviciu, ContFurnizor, 100));
        NetDocument("SC-NTC-19", tarziu.Id, new(An, 1, 31), 0); NetDocument("SC-NTC-19", tarziu.Id, Februarie, 100);
    }

    void NetDocument(string id, Guid doc, DateOnly data, decimal net) => Verifica(id, $"sold document pe cheltuială la {data}: {net}",
        CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Cont == Cont(Serviciu) && p.Data <= data)
            .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)) == net);

    void Compatibilitate() {
        var doc = Guid.Parse("00000005-0000-0000-0000-000000000000");
        var cont = Guid.Parse("00000001-0000-0000-0000-000000000000");
        var tert = Guid.Parse("00000002-0000-0000-0000-000000000000");
        var veche = new N.Unitate(Guid.Parse("6a865294-d354-8a72-4249-185390c213bb"), N.FelUnitate.Partida, cont, tert, null, Ianuarie);
        Verifica("SC-NTC-20", "identitatea veche păstrată", C.IdentitatiPartide.Gaseste([veche], doc, cont, tert) == veche);
        Verifica("SC-NTC-20", "partenerul greșit nu preia identitatea", C.IdentitatiPartide.Gaseste([veche], doc, cont, Client) == null);
        var plata = Guid.NewGuid(); var proprie = N.Unitate.DeschidePartida(cont, tert, plata, Ianuarie);
        N.Postare Post(N.Unitate u, Guid cauza, N.Latura l, decimal v) => new(
            new N.Coordonate { Cont = cont, Latura = l, Partener = tert, Unitate = u, Data = Ianuarie }, 0, 0, v, new(cauza, null));
        var rezultat = C.Transferuri.Muta(new(plata, Ianuarie, [Post(proprie, plata, N.Latura.Debit, 40)], [],
            doc, Ianuarie, [Post(veche, doc, N.Latura.Credit, 100)], [], 40, Ianuarie));
        Verifica("SC-NTC-20", "plată nouă transferată pe partida istorică", rezultat.Refuz == null
            && rezultat.Mutare?.La.Unitate == veche && rezultat.Mutare.Valoare == 40);
        var nominalizata = C.Transferuri.Muta(new(plata, Ianuarie, [Post(veche, plata, N.Latura.Debit, 40)], [],
            doc, Ianuarie, [Post(veche, doc, N.Latura.Credit, 100)], [], 40, Ianuarie));
        Verifica("SC-NTC-17", "transfer suplimentar refuzat când nominalizarea nu lasă partidă proprie (101)",
            nominalizata.Mutare == null && nominalizata.Refuz?.Cod == "IMPERECHERE_FARA_EFECT"
            && nominalizata.Sarit == null);
    }
}
