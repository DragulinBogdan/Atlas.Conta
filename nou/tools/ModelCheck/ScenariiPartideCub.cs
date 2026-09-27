using CitireCumul = Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiPartideCub(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "PARTIDE", 1998) {
    protected override void Executa() {
        Politica();
        IdentitateSql();
        EfectObligatoriu();
        EfectPartialSiTemporal();
        NominalizareAutomata();
        NotaInainteaStingerii();
        StingereDupaDesfacere(nota: false);
        StingereDupaDesfacere(nota: true);
        StingereInainteaPartideiProprii();
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var plata = Trezorerie(false, 40); Opereaza(plata.Id);
        var imp = Imperecheaza(plata.Id, f.Id, 40, Ianuarie);
        Rest(f.Id, -60); Rest(plata.Id, 0);
        Comanda(os => ImperechereService.Desfa(os, imp, Ianuarie));
        Rest(f.Id, -100); Rest(plata.Id, 40);
        Acoperire(f.Id);
        Furnizor = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-DES"; p.Denumire = p.Cod;
            os.CommitChanges(); return p.ID;
        });
        Deschidere();
        var imo = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false, Tip: Privat ? "214" : "214.00.00"));
        if (!Privat) Comanda(os => {
            var functional = os.CreateObject<CodFunctional>(); functional.Cod = Marcaj; functional.Denumire = Marcaj;
            var sursa = os.CreateObject<SursaFinantare>(); sursa.Cod = Marcaj; sursa.Denumire = Marcaj;
            var proiect = os.CreateObject<Proiect>(); proiect.Cod = Marcaj; proiect.Denumire = Marcaj;
            var linie = os.GetObjectByKey<FacturaIntrareDetaliu>(imo.Linii.Single().Id);
            linie.CodFunctionalId = functional.ID; linie.SursaFinantareId = sursa.ID; linie.ProiectId = proiect.ID;
            os.CommitChanges();
        });
        Opereaza(imo.Id);
        var contImo = Cont(Privat ? "404" : "404.01.00");
        Verifica("SC-CIT-45", "FCT imobilizare: partidă 404 creditoare 100", CuSpatiu(os =>
            P.Solduri(os, Ianuarie).Single(s => s.ContId == contImo && s.PartenerId == Furnizor).Credit == 100));
        Storneaza(imo.Id, Ianuarie);
        Verifica("SC-CIT-45", "storno: rest zero pe 404", CuSpatiu(os =>
            P.Solduri(os, Ianuarie).Single(s => s.ContId == contImo && s.PartenerId == Furnizor).Credit == 0));
        PestePerioada();
    }

    void EfectObligatoriu() {
        var p = Trezorerie(false, 70); Opereaza(p.Id);
        var casa = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID);
        var n = Nota(Ianuarie, new LinieNtcScena(Privat ? "581" : "581.01.01",
            Privat ? "5311" : "531.01.01", 200, casa, casa)); Opereaza(n.Id);
        var amprenta = Amprenta(n.Id);
        Comanda(os => {
            for (var i = 0; i < 2; i++) Refuza("SC-CIT-55", () => ImperechereService.Imperecheaza(os,
                os.GetObjectByKey<Document>(n.Id), os.GetObjectByKey<Document>(p.Id), 50, data: Ianuarie), "restul");
            os.CommitChanges();
            Verifica("SC-CIT-59", "refuz repetat fără obiecte latente", !os.GetObjectsQuery<Imperechere>()
                .Any(i => i.DocumentStingatorId == n.Id));
            Verifica("SC-CIT-55", "candidatul fără partidă comună lipsește",
                !ImperecheriProiectii.DocumenteCuRest(os, documentCurentId: n.Id).Any(r => r.DocumentId == p.Id));
        });
        Verifica("SC-CIT-55", "cub intact", amprenta == Amprenta(n.Id));
        Anuleaza(n.Id); Anuleaza(p.Id);

        var inc = Trezorerie(true, 100); Opereaza(inc.Id);
        var gresita = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 60, Client)); Opereaza(gresita.Id);
        Refuza("SC-CIT-56", () => Imperecheaza(gresita.Id, inc.Id, 60, Ianuarie), "IMPERECHERE_FARA_EFECT");
        Verifica("SC-CIT-56", "rest 100, candidat incompatibil absent", CuSpatiu(os => ImperechereService.Ramas(os, inc.Id) == 100
            && !ImperecheriProiectii.DocumenteCuRest(os, documentCurentId: gresita.Id).Any(r => r.DocumentId == inc.Id)));
        Anuleaza(gresita.Id);
        var corecta = Nota(Ianuarie, new LinieNtcScena(ContClient, Serviciu, 60, Client)); Opereaza(corecta.Id);
        var asociere = Imperecheaza(corecta.Id, inc.Id, 60, Ianuarie);
        Verifica("SC-CIT-56", "nota pe contul corect lasă rest 40", CuSpatiu(os => ImperechereService.Ramas(os, inc.Id) == 40));
        Comanda(os => ImperechereService.Sterge(os, asociere)); Anuleaza(corecta.Id); Anuleaza(inc.Id);

        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        n = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 75, Furnizor)); Opereaza(n.Id);
        amprenta = Amprenta(n.Id);
        var a = Imperecheaza(n.Id, f.Id, 50, Ianuarie);
        var b = Imperecheaza(n.Id, f.Id, 25, Ianuarie);
        Refuza("SC-CIT-57", () => Imperecheaza(n.Id, f.Id, 1, Ianuarie), "restul");
        Verifica("SC-CIT-57", "75 nominalizat, fără Transfer duplicat", amprenta == Amprenta(n.Id));
        Comanda(os => ImperechereService.Sterge(os, a));
        p = Trezorerie(false, 25); Opereaza(p.Id);
        var t1 = Imperecheaza(p.Id, f.Id, 10, Ianuarie);
        var t2 = Imperecheaza(p.Id, f.Id, 15, Ianuarie);
        Comanda(os => ImperechereService.Sterge(os, b)); Rest(f.Id, 0);
        Comanda(os => ImperechereService.Sterge(os, t1)); Rest(f.Id, -10);
        Comanda(os => ImperechereService.Sterge(os, t2)); Rest(f.Id, -25);
        Verifica("SC-CIT-60", "asocieri fără efect secundar și desfacere pe transferul propriu", true);
        Storneaza(n.Id, Ianuarie); Rest(f.Id, -100);
        Anuleaza(p.Id); Anuleaza(f.Id);
    }

    void EfectPartialSiTemporal() {
        var contImo = Privat ? "404" : "404.01.00";
        var f = Factura(Ianuarie, new LinieFctScena(1, 40, Stoc: false),
            new LinieFctScena(1, 60, Stoc: false, Tip: Privat ? "214" : "214.00.00"));
        if (!Privat) Comanda(os => {
            var cf = os.CreateObject<CodFunctional>(); cf.Cod = Marcaj + "-MIX"; cf.Denumire = cf.Cod;
            var sf = os.CreateObject<SursaFinantare>(); sf.Cod = cf.Cod; sf.Denumire = cf.Cod;
            var pr = os.CreateObject<Proiect>(); pr.Cod = cf.Cod; pr.Denumire = cf.Cod;
            foreach (var linie in f.Linii) {
                var d = os.GetObjectByKey<FacturaIntrareDetaliu>(linie.Id);
                d.CodFunctionalId = cf.ID; d.SursaFinantareId = sf.ID; d.ProiectId = pr.ID;
            }
            os.CommitChanges();
        });
        Opereaza(f.Id);
        var sursa = Guid.NewGuid(); var tinta = Guid.NewGuid();
        N.Postare Rand(Guid doc, string simbol, N.Latura latura, decimal valoare) => new(
            new N.Capat { Cont = Cont(simbol), Partener = Furnizor,
                Unitate = N.Unitate.DeschidePartida(Cont(simbol), Furnizor, doc, Ianuarie) }.Pe(latura, Ianuarie),
            0, 0, valoare, new(doc, null));
        var ambigua = C.Transferuri.Muta(new(sursa, Ianuarie,
            [Rand(sursa, ContFurnizor, N.Latura.Debit, 40), Rand(sursa, contImo, N.Latura.Debit, 60)], [],
            tinta, Ianuarie, [Rand(tinta, ContFurnizor, N.Latura.Credit, 40), Rand(tinta, contImo, N.Latura.Credit, 60)], [], 40, Ianuarie));
        Verifica("SC-CIT-58", "funcția pură refuză două perechi compatibile fără alegere implicită",
            ambigua.Mutare == null && ambigua.Refuz?.Cod == "IMPERECHERE_AMBIGUA");
        var p = Trezorerie(false, 100); Opereaza(p.Id);
        Refuza("SC-CIT-58", () => Imperecheaza(p.Id, f.Id, 100, Ianuarie), "PARTIDA_PROPRIE_INSUFICIENTA");
        Verifica("SC-CIT-58", "candidatul oferă exact contul comun: 40", CuSpatiu(os =>
            ImperecheriProiectii.DocumenteCuRest(os, documentCurentId: p.Id).Single(r => r.DocumentId == f.Id).Disponibil == 40));
        var imp = Imperecheaza(p.Id, f.Id, 40, Ianuarie);
        Verifica("SC-CIT-58", "transfer exact 40: factura 60, plata 60", CuSpatiu(os =>
            ImperechereService.Ramas(os, f.Id) == 60 && ImperechereService.Ramas(os, p.Id) == 60));
        Comanda(os => ImperechereService.Sterge(os, imp)); Anuleaza(p.Id); Anuleaza(f.Id);

        f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        p = Trezorerie(false, 100); Opereaza(p.Id);
        imp = Imperecheaza(p.Id, f.Id, 50, Ianuarie);
        Comanda(os => ImperechereService.Desfa(os, imp, new(An, 1, 20)));
        Refuza("SC-CIT-61", () => Imperecheaza(p.Id, f.Id, 80, new(An, 1, 10)), "PARTIDA_PROPRIE_INSUFICIENTA");
        Verifica("SC-CIT-61", "refuz retroactiv: 50 la 10 ianuarie, 100 după desfacere", CuSpatiu(os =>
            P.Proprii(os, new(An, 1, 10)).Single(r => r.DocumentId == p.Id).Net == 50 && ImperechereService.Ramas(os, p.Id) == 100));
    }

    void PestePerioada() {
        var cazuri = new List<(Guid Factura, Guid Plata, Guid Imp)>();
        for (var i = 0; i < 2; i++) {
            var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
            var p = Trezorerie(false, 100);
            Comanda(os => {
                var d = os.GetObjectByKey<Document>(p.Id); d.Autogenerat = true; d.DocumentSursaId = f.Id;
                os.CommitChanges();
            });
            Opereaza(p.Id);
            cazuri.Add((f.Id, p.Id, CuSpatiu(os => os.GetObjectsQuery<Imperechere>()
                .Single(r => r.DocumentStingatorId == p.Id).ID)));
        }
        InchideIanuarie();
        Snapshot();
        for (var i = 0; i < cazuri.Count; i++) {
            var c = cazuri[i];
            var pf = Partida(c.Factura, ContFurnizor).Value;
            if (i == 0) {
                Comanda(os => ImperechereService.Desfa(os, c.Imp, Februarie));
                Verifica("SC-CIT-47", "desfacere în februarie: factura redeschisă 100", CuSpatiu(os =>
                    P.Solduri(os, Februarie).Single(s => s.UnitateId == pf).Credit
                    - P.Solduri(os, Februarie).Single(s => s.UnitateId == pf).Debit == 100));
            }
            Storneaza(c.Plata, Februarie);
            Verifica("SC-CIT-51", "raport istoric zero, apoi factura 100", CuSpatiu(os =>
                !ImperecheriProiectii.PartideCuRest(os, laData: new(An, 1, 31)).Any(s => s.UnitateId == pf)
                && ImperecheriProiectii.PartideCuRest(os, laData: Februarie).Single(s => s.UnitateId == pf).Rest == 100));
            Verifica("SC-CIT-47", "ianuarie rămâne stins; februarie factura −100, plata zero", CuSpatiu(os => {
                var initial = P.Solduri(os, Ianuarie).Single(s => s.UnitateId == pf);
                var final = P.Solduri(os, Februarie).Single(s => s.UnitateId == pf);
                var pp = Partida(c.Plata, ContFurnizor).Value;
                return initial.Debit == initial.Credit && final.Credit - final.Debit == 100
                    && !P.Solduri(os, Februarie).Any(s => s.UnitateId == pp && s.Debit != s.Credit);
            }));
        }
    }

    void NominalizareAutomata() {
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var p = Trezorerie(false, 100);
        Comanda(os => {
            var doc = os.GetObjectByKey<Document>(p.Id);
            doc.Autogenerat = true; doc.DocumentSursaId = f.Id; os.CommitChanges();
        });
        Opereaza(p.Id);
        Rest(f.Id, 0);
        var original = Amprenta(p.Id);
        var imp = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == p.Id).ID);
        Comanda(os => {
            os.Delete(os.GetObjectByKey<Imperechere>(imp));
            Refuza("SC-CIT-48", () => GardianEditare.Verifica(os), "IMPERECHERE_COMANDA_OBLIGATORIE");
        });
        Verifica("SC-CIT-48", "refuzul CRUD păstrează legătura și postările",
            CuSpatiu(os => os.GetObjectByKey<Imperechere>(imp) != null) && Amprenta(p.Id) == original);
        Comanda(os => Atlas.Conta.BackOffice.Module.Api.Trz.ImperechereApply.Sterge(os, imp));
        Rest(f.Id, -100); Rest(p.Id, 100);
        Verifica("SC-CIT-46", "postările inițiale rămân după desfacere",
            original.Split('\n', StringSplitOptions.RemoveEmptyEntries).All(Amprenta(p.Id).Contains));
        var m = Trezorerie(false, 40); Opereaza(m.Id);
        var manuala = Imperecheaza(m.Id, f.Id, 40, Ianuarie); Rest(f.Id, -60); Rest(m.Id, 0);
        Comanda(os => Atlas.Conta.BackOffice.Module.Api.Trz.ImperechereApply.Sterge(os, manuala));
        Rest(f.Id, -100); Rest(m.Id, 40);
        var reluata = Imperecheaza(p.Id, f.Id, 40, Ianuarie); Rest(f.Id, -60); Rest(p.Id, 60);
        Comanda(os => ImperechereService.Sterge(os, reluata));
        Rest(f.Id, -100); Rest(p.Id, 100);
        Storneaza(p.Id, Ianuarie); Rest(p.Id, 0); Rest(f.Id, -100);
        Storneaza(m.Id, Ianuarie);
        Storneaza(f.Id, Ianuarie); Rest(f.Id, 0);
    }

    Guid PartenerNou(string sufix) => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + sufix; p.Denumire = p.Cod;
        os.CommitChanges(); return p.ID;
    });

    void NotaInainteaStingerii() {
        var furnizor = Furnizor; Furnizor = PartenerNou("-D2");
        var d1 = new DateOnly(An, 1, 7); var d3 = new DateOnly(An, 1, 15);
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var p = Trezorerie(false, 100); Opereaza(p.Id);
        var imp = Imperecheaza(p.Id, f.Id, 100, d3);
        var n = Nota(d1, new LinieNtcScena(ContFurnizor, Serviciu, 100, Furnizor)); Opereaza(n.Id);
        var pf = Partida(f.Id, ContFurnizor).Value;
        Verifica("SC-CIT-66", "NTC la d1 nu nominalizează factura stinsă la d3", CuSpatiu(os =>
            !P.Postari(os).Any(x => x.DocumentId == n.Id && x.Unitate == pf)));
        Verifica("SC-CIT-66", "NTC deschide partidă proprie 100", CuSpatiu(os => ImperechereService.Ramas(os, n.Id) == 100));
        Verifica("SC-CIT-66", "factura −100 la d1, 0 de la d3, niciodată creanță", CuSpatiu(os => {
            decimal La(DateOnly zi) => P.Solduri(os, zi).Where(x => x.UnitateId == pf)
                .Select(x => x.Debit - x.Credit).SingleOrDefault();
            return La(d1) == -100 && La(d3) == 0 && La(DateOnly.MaxValue) == 0;
        }));
        Anuleaza(n.Id);
        Comanda(os => ImperechereService.Sterge(os, imp)); Anuleaza(p.Id); Anuleaza(f.Id);
        Furnizor = furnizor;
    }

    void StingereDupaDesfacere(bool nota) {
        var id = nota ? "SC-CIT-67/NTC" : "SC-CIT-67";
        var furnizor = Furnizor; Furnizor = PartenerNou(nota ? "-D3N" : "-D3");
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var p = Trezorerie(false, 100);
        Comanda(os => {
            var doc = os.GetObjectByKey<Document>(p.Id);
            doc.Autogenerat = true; doc.DocumentSursaId = f.Id; os.CommitChanges();
        });
        Opereaza(p.Id);
        Guid s = default;
        if (nota) { s = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100, RepartitorCredit: Furnizor)).Id; Opereaza(s); }
        var automata = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == p.Id).ID);
        Comanda(os => Atlas.Conta.BackOffice.Module.Api.Trz.ImperechereApply.Sterge(os, automata));
        Rest(p.Id, 100);
        if (!nota) {
            var inc = Trezorerie(true, 100);
            Comanda(os => {
                os.GetObjectByKey<Incasare>(inc.Id).PredatorId = Furnizor;
                os.GetObjectByKey<Partener>(Furnizor).ContImplicit = os.GetObjectByKey<Cont>(Cont(ContFurnizor));
                os.CommitChanges();
            });
            Opereaza(inc.Id); s = inc.Id;
        }
        Verifica(id, "panoul oferă plata cu disponibil 100", CuSpatiu(os => ImperecheriProiectii
            .DocumenteCuRest(os, documentCurentId: s).SingleOrDefault(r => r.DocumentId == p.Id)?.Disponibil == 100));
        Guid? imp = null;
        try { imp = Imperecheaza(s, p.Id, 100, Ianuarie); }
        catch (OperareException e) { Console.WriteLine($"     {id}: {e.Message.Split('\n')[0]}"); }
        Verifica(id, "comanda acceptă candidatul panoului", imp != null);
        if (imp is Guid legatura) {
            Rest(p.Id, 0);
            Comanda(os => ImperechereService.Sterge(os, legatura));
        }
        Anuleaza(s); Anuleaza(p.Id); Anuleaza(f.Id);
        Furnizor = furnizor;
    }

    void StingereInainteaPartideiProprii() {
        var furnizor = Furnizor; Furnizor = PartenerNou("-TMP");
        var d7 = new DateOnly(An, 1, 7); var d10 = new DateOnly(An, 1, 10);
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f.Id);
        var p = Trezorerie(false, 100);
        Comanda(os => {
            var doc = os.GetObjectByKey<Document>(p.Id);
            doc.Autogenerat = true; doc.DocumentSursaId = f.Id; os.CommitChanges();
        });
        Opereaza(p.Id);
        var automata = CuSpatiu(os => os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == p.Id).ID);
        Comanda(os => ImperechereService.Desfa(os, automata, d10));
        var f2 = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false)); Opereaza(f2.Id);
        Refuza("SC-CIT-68", () => Imperecheaza(p.Id, f2.Id, 50, d7), "PARTIDA_PROPRIE_INSUFICIENTA");
        Imperecheaza(p.Id, f2.Id, 50, d10);
        Verifica("SC-CIT-68", "partida proprie născută la 10 finanțează numai de la 10: FCT2 −50, plata 50",
            CuSpatiu(os => ImperechereService.Ramas(os, f2.Id) == 50 && ImperechereService.Ramas(os, p.Id) == 50));
        Furnizor = furnizor;
    }

    void Rest(Guid doc, decimal net) {
        Verifica("SC-CIT-42", $"rest partidă {net}", CuSpatiu(os => {
            var id = Partida(doc, ContFurnizor).Value;
            var s = P.Solduri(os, Ianuarie).Single(s => s.UnitateId == id);
            return s.Debit - s.Credit == net && s.ContId == Cont(ContFurnizor) && s.PartenerId == Furnizor;
        }));
        Verifica("SC-CIT-49", $"raport partidă {net}", CuSpatiu(os => {
            var id = Partida(doc, ContFurnizor).Value;
            var rand = ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: Ianuarie)
                .SingleOrDefault(s => s.UnitateId == id);
            return net == 0 ? rand == null : rand != null && rand.DocumentId == doc
                && rand.Rest == Math.Abs(net) && rand.Sens == (net > 0 ? "Creanta" : "Datorie");
        }));
    }

    void IdentitateSql() {
        using var os = Deschide();
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        for (var i = 0; i < 8; i++) {
            var doc = Guid.NewGuid(); var cont = Guid.NewGuid(); var partener = Guid.NewGuid();
            var sql = ctx.Database.SqlQuery<Guid>($"SELECT cub_partida_id({doc}, {cont}, {partener}) AS \"Value\"").Single();
            Verifica("SC-CIT-53", "SQL = nucleu; identitatea inițială distinctă", sql == P.Identitate(doc, cont, partener)
                && sql != N.Unitate.DeschidePartidaInitiala(cont, partener, doc, Ianuarie).Id);
        }
    }

    void Snapshot() {
        using var os = Deschide();
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        using var tx = ctx.Database.BeginTransaction();
        var zi = new DateOnly(An, 1, 31);
        var directe = P.Solduri(os, zi).Where(s => s.PartenerId == Furnizor && s.Debit != s.Credit)
            .Select(s => new { s.UnitateId, Net = s.Debit - s.Credit }).ToList().OrderBy(s => s.UnitateId).ToArray();
        var cumulate = P.Cumulate(os, CitireCumul.Integrala, zi).Where(s => s.PartenerId == Furnizor && s.Debit != s.Credit)
            .Select(s => new { s.UnitateId, Net = s.Debit - s.Credit }).ToList().OrderBy(s => s.UnitateId).ToArray();
        Verifica("SC-CIT-50", "snapshot = cub direct, inclusiv două deschideri fără document", directe.SequenceEqual(cumulate)
            && os.GetObjectsQuery<PartidaDeschisa>().Count(s => s.An == An && s.Luna == 1 && s.PartenerId == Furnizor && s.DocumentId == null) == 2);
        var id = os.GetObjectsQuery<PartidaDeschisa>().Where(s => s.An == An && s.Luna == 1 && s.PartenerId == Furnizor).Select(s => s.ID).First();
        ctx.Database.ExecuteSqlInterpolated($"UPDATE \"PartideDeschise\" SET \"Rest\" = \"Rest\" + 7 WHERE \"ID\" = {id}");
        var raport = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
        Verifica("SC-CIT-52", "raportează diferența 7 înainte de reparare", raport.PartideDiferite == 1 && raport.DiferentaRest == 7);
        raport = SolduriService.Reconstruieste(os).Referinte.Single(r => r.An == An && r.Luna == 1);
        Verifica("SC-CIT-52", "a doua reconstrucție fără diferențe", raport.PartideDiferite == 0 && raport.DiferentaRest == 0);
        tx.Rollback();
    }

    void Politica() {
        if (Privat) return;
        using var os = Deschide();
        var ctx = ((EFCoreObjectSpace)os).DbContext;
        using var tx = ctx.Database.BeginTransaction();
        string[] simboluri = ["401.01.00", "404.01.00", "411.01.01"];
        var conturi = os.GetObjectsQuery<Cont>().Where(c => simboluri.Contains(c.Simbol)).ToArray();
        Verifica("SC-CIT-41", "trei conturi urmărite, fără rol comercial", conturi.Length == 3
            && conturi.All(c => c.DinSeed && c.UrmarestePartide && c.RolTert == RolTertCont.Niciunul));
        foreach (var c in conturi) { c.DinSeed = false; c.UrmarestePartide = false; }
        os.CommitChanges();
        ContaSeeder.Seed(os, ProfilContabil.Bugetar);
        Verifica("SC-CIT-41", "seed respectă toate cele trei conturi devenite manuale",
            conturi.All(c => !c.DinSeed && !c.UrmarestePartide && c.RolTert == RolTertCont.Niciunul));
        foreach (var c in conturi) c.DinSeed = true;
        os.CommitChanges();
        ContaSeeder.Seed(os, ProfilContabil.Bugetar);
        ContaSeeder.Seed(os, ProfilContabil.Bugetar);
        Verifica("SC-CIT-41", "aliniere și re-seed idempotent fără rol SAF-T",
            conturi.All(c => c.UrmarestePartide && c.RolTert == RolTertCont.Niciunul));
        tx.Rollback();
    }

    void Acoperire(Guid factura) {
        var nota = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 10)); Opereaza(nota.Id);
        Comanda(os => {
            P.VerificaAcoperire(os);
            Verifica("SC-CIT-43", "nota fără partener explicit nu cere partidă", true);
            var ctx = ((EFCoreObjectSpace)os).DbContext;
            using var tx = ctx.Database.BeginTransaction();
            var cont = Cont(ContFurnizor);
            ctx.Database.ExecuteSqlInterpolated($"UPDATE \"Postare\" SET \"Unitate\" = null, \"FelUnitate\" = null, \"UnitateDeschisa\" = null, \"Partener\" = null WHERE \"DocumentId\" = {factura} AND \"Cont\" = {cont}");
            Refuza("SC-CIT-43", () => C.Citiri.Invarianti.Verifica(os), "CITIRE_PARTIDE_INCOMPLETE");
            ctx.Database.ExecuteSqlInterpolated($"UPDATE \"Conturi\" SET \"UrmarestePartide\" = false WHERE \"ID\" = {cont}");
            Refuza("SC-CIT-43", () => C.Citiri.Invarianti.Verifica(os), "CITIRE_PARTIDE_POLITICA");
            tx.Rollback();
        });
        Anuleaza(nota.Id);
    }

    void Deschidere() {
        var ref1 = Guid.NewGuid(); var ref2 = Guid.NewGuid();
        if (N.Unitate.DeschidePartidaInitiala(Cont(ContFurnizor), Furnizor, ref1, Ianuarie).Id
            .CompareTo(N.Unitate.DeschidePartidaInitiala(Cont(ContFurnizor), Furnizor, ref2, Ianuarie).Id) > 0)
            (ref1, ref2) = (ref2, ref1);
        var cont = Cont(ContFurnizor);
        Comanda(os => {
            var ancora = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891") && !c.Sumator).ID;
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Deschide(os, Ianuarie,
                [new(cont, N.Latura.Credit, 100, true), new(ancora, N.Latura.Debit, 100)], [],
                [new(cont, Furnizor, ref1, N.Latura.Credit, 60) { Analiza = new(null, Economic, null, null, null, null) },
                 new(cont, Furnizor, ref2, N.Latura.Credit, 40) { Analiza = new(null, Economic, null, null, null, null) }]);
            os.CommitChanges(); tx.Commit();
        });
        Verifica("SC-CIT-50", "raport: două deschideri fără document, 60 + 40", CuSpatiu(os => {
            var r = ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: Ianuarie).ToArray();
            return r.Length == 2 && r.All(r => r.DocumentId == null) && r.Select(r => r.Rest).Order().SequenceEqual(new[] { 40m, 60m });
        }));
        var nota = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 75, Furnizor)); Opereaza(nota.Id);
        Verifica("SC-CIT-50", "NTC 75: raport păstrează a doua deschidere cu rest 25", CuSpatiu(os => {
            var r = ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: Ianuarie).Single();
            return r.DocumentId == null && r.Rest == 25;
        }));
        Verifica("SC-CIT-44", "deschidere 100, NTC 75: rest total −25", CuSpatiu(os => {
            var s = P.Solduri(os, Ianuarie).Where(s => s.ContId == cont && s.PartenerId == Furnizor).ToArray();
            return s.Length == 2 && s.Sum(s => s.Debit - s.Credit) == -25
                && s.Count(s => s.Debit == s.Credit) == 1;
        }));
        Storneaza(nota.Id, Ianuarie);
        Verifica("SC-CIT-44", "storno NTC: partide inițiale −60 și −40", CuSpatiu(os => {
            var s = P.Solduri(os, Ianuarie).Where(s => s.ContId == cont && s.PartenerId == Furnizor)
                .Select(s => s.Debit - s.Credit).ToArray();
            return s.Order().SequenceEqual(new[] { -60m, -40m });
        }));
    }

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja purja) {
        var terti = os.GetObjectsQuery<Partener>()
            .Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID);
        var ids = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == null && terti.Contains(p.Partener.Value))
            .Select(p => p.TranzactieId).Distinct().ToList();
        purja.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => ids.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        purja.AdaugaCheie<C.Tranzactie>(ids);
    }
}
