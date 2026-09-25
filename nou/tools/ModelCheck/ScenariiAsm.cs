using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiAsm(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "ASM", 2014) {
    string Materie => Privat ? "301" : "302.03.00";
    string Fabricat => Privat ? "345" : "303.01.00";
    int numarProdus;

    FacturaScena Culege((LinieScena Lot, decimal Q)[] consumuri,
            (decimal Q, decimal V, string Tip)[] produse, DateOnly? data = null) {
        using var os = Deschide();
        var d = os.CreateObject<Asamblare>();
        d.Data = data ?? Ianuarie; d.DataInregistrare = d.Data;
        d.PredatorId = Magazie; d.PrimitorId = Magazie;
        var rezultat = new List<LinieScena>();
        foreach (var (lot, q) in consumuri) {
            var l = os.CreateObject<AsamblareDetaliu>(); l.Document = d;
            l.Pozitie = rezultat.Count + 1; l.Directie = DirectieAsamblare.Consum;
            l.LotId = lot.Lot; l.Cantitate = q;
            l.TipMaterialId = os.GetObjectByKey<Produs>(lot.Produs!.Value).TipMaterialId!.Value;
            rezultat.Add(new(l.ID, l.LotId, lot.Produs));
        }
        foreach (var (q, v, tip) in produse) {
            var l = os.CreateObject<AsamblareDetaliu>(); l.Document = d;
            l.Pozitie = rezultat.Count + 1; l.Directie = DirectieAsamblare.Produs;
            l.TipMaterialId = Tip(os, tip ?? Stoc); l.Cantitate = q; l.PretEvaluare = v / q;
            var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-ASM-" + ++numarProdus;
            p.Denumire = p.Cod; p.UM = "BUC"; p.TipMaterialId = l.TipMaterialId;
            l.ProdusId = p.ID;
            var lot = l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
            rezultat.Add(new(l.ID, lot.ID, p.ID));
        }
        os.CommitChanges(); return new(d.ID, rezultat.ToArray());
    }

    RandScena[] Rand(FacturaScena d, int index, decimal q, decimal v,
            N.FelTranzactie fel = N.FelTranzactie.Transfer, string tip = null) {
        var l = d.Linii[index];
        var latura = q < 0m && fel == N.FelTranzactie.Operare ? N.Latura.Credit : N.Latura.Debit;
        var valoare = q < 0m && fel == N.FelTranzactie.Transfer ? -v : v;
        return [new(Cont(tip ?? Stoc), latura, valoare, q, Magazie, l.Lot, l.Produs,
                Linie: l.Id, Spatiu: N.Spatiu.Stoc),
            new(Cont(tip ?? Stoc), latura, 0m, -q, N.GestiuniVirtuale.Transformare,
                Produs: l.Produs, Linie: l.Id)];
    }
    static RandScena[] Inverse(params RandScena[] randuri) =>
        [.. randuri.Select(r => r with { Cantitate = -r.Cantitate, Valoare = -r.Valoare })];
    void Sold(string id, LinieScena l, decimal q, decimal v, DateOnly? data = null) =>
        SoldLot(id, l.Lot!.Value, Magazie, data ?? new DateOnly(An, 1, 31), q, v);

    protected override void Executa() {
        ProbeAsmOperand.Ruleaza((nume, rezultat) => Verifica("SC-ASM-PUR", nume, rezultat));
        var lot = Receptioneaza(new LinieFctScena(2, 50)).Linii[0];
        var d = Culege([(lot, 2)], [(1, 100, null)]);
        var citiriMici = ContractPur(d.Id);
        var mareLot = Receptioneaza(new LinieFctScena(50, 1)).Linii[0];
        var mare = Culege(Enumerable.Repeat((mareLot, 1m), 50).ToArray(), [(1, 50, null)]);
        var citiriMari = ContractPur(mare.Id);
        Verifica("SC-ASM/PERF", $"2 și 51 linii: {citiriMici}/{citiriMari} interogări", citiriMari == citiriMici
            && citiriMari > 0 && citiriMari <= ProbeNucleu.PragInterogari);
        Verifica("SC-ASM-01", "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        FaraEfecte("SC-ASM-01", d.Id); Opereaza(d.Id);
        RandScena[] r = [.. Rand(d, 0, -2, 100), .. Rand(d, 1, 1, 100)];
        Postari("SC-ASM-01", d.Id, N.FelTranzactie.Transfer, Ianuarie, r);
        Verifica("SC-ASM-01", "numai Transfer, rulaj contabil zero", CuSpatiu(os =>
            os.GetObjectsQuery<C.Tranzactie>().Count(t => t.DocumentId == d.Id) == 1));
        Sold("SC-ASM-01", lot, 0, 0); Sold("SC-ASM-01", d.Linii[1], 1, 100);
        var dataStorno = new DateOnly(An, 1, 20);
        Storneaza(d.Id, dataStorno);
        Postari("SC-ASM-07", d.Id, N.FelTranzactie.Storno, dataStorno, Inverse(r));
        Sold("SC-ASM-07", lot, 2, 100); Sold("SC-ASM-07", d.Linii[1], 0, 0);
        var stamp = Amprenta(d.Id);
        Refuza("SC-ASM-07", () => Storneaza(d.Id, dataStorno), "Operat");
        Verifica("SC-ASM-07", "repetarea nu scrie", stamp == Amprenta(d.Id));
        Multiple(); Refuzuri(); Rotunjiri(); Capcana(); DeltaFaraAncora(); Dependenti(); LantRetur();
        var mixt = Mixt("SC-ASM-05");
        Rapoarte(mixt.Doc, false);
        Verifica("SC-CIT-04", "ASM mixt: două postări economice de 40, fără Transfer/contrapondere", CuSpatiu(os => {
            var randuri = C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == mixt.Doc.Id).ToList();
            return randuri.Count == 2 && randuri.All(p => p.Valoare == 40 && p.Gestiune == Magazie)
                && randuri.Single(p => p.Latura == N.Latura.Debit).Cont == Cont(Fabricat)
                && randuri.Single(p => p.Latura == N.Latura.Credit).Cont == Cont(Materie);
        }));
        ProbaReconciliere(mixt.Doc.Id);
        Anuleaza(mixt.Doc.Id); FaraEfecte("SC-ASM-09", mixt.Doc.Id);
        foreach (var l in mixt.Doc.Linii.Skip(2)) Sold("SC-ASM-09", l, 0, 0);
        Opereaza(mixt.Doc.Id);
        Postari("SC-ASM-09", mixt.Doc.Id, N.FelTranzactie.Transfer, Ianuarie, mixt.Transfer);
        Postari("SC-ASM-09", mixt.Doc.Id, N.FelTranzactie.Operare, Ianuarie, mixt.Operare);
        var corectieLot = Receptioneaza(new LinieFctScena(2, 50)).Linii[0];
        var corectieDoc = Culege([(corectieLot, 2)], [(1, 100, null)]); Opereaza(corectieDoc.Id);
        var dualCorectie = DualCorectie();
        var intarziat = Culege([(Receptioneaza(new LinieFctScena(2, 50)).Linii[0], 2)], [(1, 100, null)]);
        InchideIanuarie();
        Storneaza(mixt.Doc.Id, Februarie);
        Rapoarte(mixt.Doc, true);
        Postari("SC-ASM-08", mixt.Doc.Id, N.FelTranzactie.Storno, Februarie,
            Inverse([.. mixt.Transfer, .. mixt.Operare]));
        Verifica("SC-CIT-04", "storno mixt: două inverse economice −40; transferul rămâne exclus", CuSpatiu(os => {
            var randuri = C.Citiri.Contabil.Postari(os).Where(p => p.DocumentId == mixt.Doc.Id).ToList();
            return randuri.Count == 4 && randuri.Count(p => p.Valoare == -40) == 2
                && randuri.All(p => p.Gestiune == Magazie);
        }));
        Sold("SC-ASM-08", mixt.Doc.Linii[0], 0, 0);
        Sold("SC-ASM-08", mixt.Doc.Linii[0], 1, 60, Februarie);
        Corectie("SC-ASM-10", corectieDoc, corectieLot, 1, 50, 1, 50);
        Corectie("SC-ASM-18", dualCorectie.Doc, dualCorectie.Lot, 2, 6.67m, 0, 0);
        Refuza("SC-ASM-15", () => Opereaza(intarziat.Id), "închis"); FaraEfecte("SC-ASM-15", intarziat.Id);
        Comanda(os => { var doc = os.GetObjectByKey<Document>(intarziat.Id); doc.DataInregistrare = Februarie; os.CommitChanges(); });
        Opereaza(intarziat.Id);
        Postari("SC-ASM-15", intarziat.Id, N.FelTranzactie.Transfer, Februarie,
            [.. Rand(intarziat, 0, -2, 100), .. Rand(intarziat, 1, 1, 100)]);
        Sold("SC-ASM-15", intarziat.Linii[0], 2, 100); Sold("SC-ASM-15", intarziat.Linii[0], 0, 0, Februarie);
    }

    void Rapoarte(FacturaScena doc, bool inversat) => Comanda(os => {
        var data = inversat ? Februarie : Ianuarie;
        var jurnal = ContabilProiectii.RegistruJurnal(os, data, data).Where(r => r.DocumentId == doc.Id).ToList();
        var fisa = ContabilProiectii.FisaCont(os, Cont(Fabricat), data, data, materialId: doc.Linii[3].Produs).ToList();
        var balanta = ContabilProiectii.Balanta(os, data, data, materialId: doc.Linii[3].Produs).Single();
        var valoare = inversat ? -40 : 40;
        Verifica("SC-CIT-15", $"ASM mixt: jurnal/fișă/balanță {valoare}, fără transfer sau contraponderi",
            jurnal.Count == 2 && jurnal.Sum(r => r.Debit) == valoare && jurnal.Sum(r => r.Credit) == valoare
            && fisa.Count == 1 && fisa[0].Debit == valoare && fisa[0].SoldCurent == (inversat ? 0 : 40)
            && balanta.RulajDebit == valoare && balanta.RulajCredit == 0);
    });

    void Multiple() {
        var f = Receptioneaza(new LinieFctScena(2, 30), new LinieFctScena(3, 13.333333m));
        var d = Culege([(f.Linii[0], 2), (f.Linii[1], 3)], [(1, 100, null)]); Opereaza(d.Id);
        Postari("SC-ASM-02", d.Id, N.FelTranzactie.Transfer, Ianuarie,
            [.. Rand(d, 0, -2, 60), .. Rand(d, 1, -3, 40), .. Rand(d, 2, 1, 100)]);
        Sold("SC-ASM-02", f.Linii[0], 0, 0); Sold("SC-ASM-02", f.Linii[1], 0, 0); Sold("SC-ASM-02", d.Linii[2], 1, 100);
        var e = Culege([(d.Linii[2], 1)], [(2, 60, null), (4, 40, null)]); Opereaza(e.Id);
        Postari("SC-ASM-03", e.Id, N.FelTranzactie.Transfer, Ianuarie,
            [.. Rand(e, 0, -1, 100), .. Rand(e, 1, 2, 60), .. Rand(e, 2, 4, 40)]);
        Sold("SC-ASM-03", e.Linii[1], 2, 60); Sold("SC-ASM-03", e.Linii[2], 4, 40);
        var a = Receptioneaza(new LinieFctScena(2, 50, Tip: Materie)).Linii[0];
        var b = Culege([(a, 2)], [(1, 100, Fabricat)]); Opereaza(b.Id);
        Postari("SC-ASM-04", b.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Rand(b, 0, -2, 100, N.FelTranzactie.Operare, Materie), .. Rand(b, 1, 1, 100, N.FelTranzactie.Operare, Fabricat)]);
        var x = Receptioneaza(new LinieFctScena(1, 60), new LinieFctScena(1, 40, Tip: Materie));
        var y = Culege([(x.Linii[0], 1), (x.Linii[1], 1)], [(1, 100, null)]); Opereaza(y.Id);
        Postari("SC-ASM-06", y.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Rand(y, 0, -1, 60, N.FelTranzactie.Operare), .. Rand(y, 1, -1, 40, N.FelTranzactie.Operare, Materie),
             .. Rand(y, 2, 1, 100, N.FelTranzactie.Operare)]);
    }

    (FacturaScena Doc, RandScena[] Transfer, RandScena[] Operare) Mixt(string id) {
        var f = Receptioneaza(new LinieFctScena(1, 60), new LinieFctScena(1, 40, Tip: Materie));
        var d = Culege([(f.Linii[0], 1), (f.Linii[1], 1)], [(1, 60, null), (1, 40, Fabricat)]); Opereaza(d.Id);
        RandScena[] t = [.. Rand(d, 0, -1, 60), .. Rand(d, 2, 1, 60)];
        RandScena[] o = [.. Rand(d, 1, -1, 40, N.FelTranzactie.Operare, Materie), .. Rand(d, 3, 1, 40, N.FelTranzactie.Operare, Fabricat)];
        Postari(id, d.Id, N.FelTranzactie.Transfer, Ianuarie, t); Postari(id, d.Id, N.FelTranzactie.Operare, Ianuarie, o);
        return (d, t, o);
    }

    void Refuzuri() {
        var lot = Receptioneaza(new LinieFctScena(2, 50)).Linii[0];
        var rau = Culege([(lot, 2)], [(1, 99.99m, null)]);
        RefuzDeclaratie("SC-ASM-12", rau.Id, CoduriRefuz.AsamblareNebalansata);
        Refuza("SC-ASM-12", () => Opereaza(rau.Id), "Valoarea produsă"); FaraEfecte("SC-ASM-12", rau.Id);
        Sold("SC-ASM-12", lot, 2, 100); Sold("SC-ASM-12", rau.Linii[1], 0, 0);
        foreach (var caz in new[] { "lot", "zero", "consum", "produs", "propriu", "insuficient" }) {
            var d = Culege([(lot, caz == "insuficient" ? 3 : 2)], [(1, caz == "insuficient" ? 150 : 100, null)]);
            Comanda(os => {
                var c = os.GetObjectByKey<AsamblareDetaliu>(d.Linii[0].Id);
                var p = os.GetObjectByKey<AsamblareDetaliu>(d.Linii[1].Id);
                if (caz == "lot") c.LotId = null;
                if (caz == "zero") c.Cantitate = 0;
                if (caz == "consum") p.Directie = DirectieAsamblare.Consum;
                if (caz == "produs") c.Directie = DirectieAsamblare.Produs;
                if (caz == "propriu") c.LotId = p.LotId;
                os.CommitChanges();
            });
            var cod = caz switch { "lot" => CoduriRefuz.LotLipsa, "zero" => CoduriRefuz.CantitateNepozitiva,
                "insuficient" => "STOC_INSUFICIENT", _ => CoduriRefuz.AsamblareStructuraInvalida };
            RefuzDeclaratie("SC-ASM-13/" + caz, d.Id, cod);
            Refuza("SC-ASM-13/" + caz, () => Opereaza(d.Id), ""); FaraEfecte("SC-ASM-13/" + caz, d.Id);
        }
    }

    void ProbaReconciliere(Guid document) {
        using var os = Deschide();
        var note = new List<string>();
        var randuri = ReconciliereCub.Ruleaza(((EFCoreObjectSpace)os).DbContext, [document], note);
        foreach (var rand in randuri) Console.WriteLine("     " + rand);
        foreach (var nota in note) Console.WriteLine("     " + nota);
        var diagnostic = ReconciliereCub.Asm(((EFCoreObjectSpace)os).DbContext, [document]);
        var asteptat = new[] {
            new ReconciliereCub.Rand("(h) ASM contabil", $"ASM C {Materie} {An}-01", 40m, 0m),
            new ReconciliereCub.Rand("(h) ASM contabil", $"ASM D {Fabricat} {An}-01", 40m, 0m),
        };
        Verifica("NUC-ASM-RECONCILIERE", "(h): exact D 40/C 40 față de registre, 1 document și 4 postări Operare",
            diagnostic.Documente == 1 && diagnostic.Postari == 4 && diagnostic.Diferente.Count == 2
            && asteptat.All(diagnostic.Diferente.Contains));
        Verifica("NUC-ASM-RECONCILIERE", "(a)–(g): zero diferențe și exit 0; raportul păstrează excepția și cifrele",
            randuri.Count == 0 && ReconciliereCub.CodIesire(randuri) == 0
            && diagnostic.Linii().All(note.Contains)
            && ReconciliereCub.Raport(randuri).Contains("exclude nominal ASM Operare"));
        Verifica("NUC-ASM-RECONCILIERE", "orice abatere comparabilă (a)–(g) păstrează exit 1",
            new[] { "(a) contabil", "(b) stoc", "(c) fiscal", "(d) balanță", "(e) număr", "(f) partide", "(g) fiscal storno" }
                .All(litera => ReconciliereCub.CodIesire([new(litera, "probă de abatere", 1m, 0m)]) == 1));
        var gol = ReconciliereCub.Asm(((EFCoreObjectSpace)os).DbContext, []);
        Verifica("NUC-ASM-RECONCILIERE", "filtrul de documente izolează și numărătoarea și diferențele (h)",
            gol.Documente == 0 && gol.Postari == 0 && gol.Diferente.Count == 0);
    }

    void Rotunjiri() {
        var lot = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        var a = Culege([(lot, 1)], [(1, 3.33m, null)]); Opereaza(a.Id);
        var rau = Culege([(lot, 2)], [(1, 6.66m, null)]);
        RefuzDeclaratie("SC-ASM-11", rau.Id, CoduriRefuz.AsamblareNebalansata); FaraEfecte("SC-ASM-11", rau.Id);
        var b = Culege([(lot, 2)], [(1, 6.67m, null)]); Opereaza(b.Id);
        Postari("SC-ASM-11", b.Id, N.FelTranzactie.Transfer, Ianuarie,
            [.. Rand(b, 0, -2, 6.67m), .. Rand(b, 1, 1, 6.67m)]); Sold("SC-ASM-11", lot, 0, 0);
        var dual = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        var p = new[] { 3.33m, 3.33m, 3.34m }; var c = new[] { 3.33m, 3.34m, 3.33m };
        FacturaScena alDoilea = null;
        for (var i = 0; i < 3; i++) {
            var d = Culege([(dual, 1)], [(1, p[i], null)]);
            var contract = CuSpatiu(os => Contractare.Contracteaza(os, os.GetObjectByKey<Document>(d.Id)));
            Verifica("SC-ASM-17", "decizie Δ exactă", contract.EsteAcceptat && contract.Decizii.OfType<N.AbsorbtieEvaluare>().Sum(x => x.Delta) == c[i] - p[i]);
            Opereaza(d.Id);
            Verifica("SC-ASM-17", "absorbția nu schimbă prețul cules, valoarea liniei sau prețul lotului", CuSpatiu(os => {
                var l = os.GetObjectByKey<AsamblareDetaliu>(d.Linii[1].Id);
                return l.PretEvaluare == p[i] && l.Valoare == p[i]
                    && os.GetObjectByKey<Lot>(d.Linii[1].Lot!.Value).PretUnitar == p[i];
            }));
            Postari("SC-ASM-17", d.Id, N.FelTranzactie.Transfer, Ianuarie,
                [.. Rand(d, 0, -1, c[i]), .. Rand(d, 1, 1, c[i])]);
            if (i == 1) alDoilea = d;
        }
        Sold("SC-ASM-17/T-r13: evaluare din cub", dual, 0, 0);
        Diagnostic("SC-ASM-17", dual, 0, 0);
        var iesire = Consum(alDoilea!.Linii[1].Lot!.Value, 1); Opereaza(iesire);
        Sold("SC-ASM-25/T-r13", alDoilea.Linii[1], 0, 0);
        Diagnostic("SC-ASM-25", alDoilea.Linii[1], 0, 0);
        DeltaLocal();
    }

    void DeltaLocal() {
        var f = Receptioneaza(new LinieFctScena(3, 3.333333m), new LinieFctScena(1, 10, Tip: Materie));
        var initial = Culege([(f.Linii[0], 1)], [(1, 3.33m, null)]); Opereaza(initial.Id);
        var d = Culege([(f.Linii[0], 1), (f.Linii[1], 1)], [(1, 3.33m, null), (1, 10, Materie)]); Opereaza(d.Id);
        Postari("SC-ASM-20", d.Id, N.FelTranzactie.Transfer, Ianuarie,
            [.. Rand(d, 0, -1, 3.34m), .. Rand(d, 1, -1, 10, tip: Materie),
             .. Rand(d, 2, 1, 3.34m), .. Rand(d, 3, 1, 10, tip: Materie)]);
        var lot = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        var prim = Culege([(lot, 1)], [(1, 3.33m, null)]); Opereaza(prim.Id);
        var alt = Culege([(lot, 1)], [(1, 3.33m, Fabricat)]); Opereaza(alt.Id);
        Postari("SC-ASM-21", alt.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Rand(alt, 0, -1, 3.34m, N.FelTranzactie.Operare), .. Rand(alt, 1, 1, 3.34m, N.FelTranzactie.Operare, Fabricat)]);
    }

    (FacturaScena Doc, LinieScena Lot) DualCorectie() {
        var lot = Receptioneaza(new LinieFctScena(3, 3.333333m)).Linii[0];
        var a = Culege([(lot, 1)], [(1, 3.33m, null)]); Opereaza(a.Id);
        var b = Culege([(lot, 1)], [(1, 3.33m, null)]); Opereaza(b.Id);
        return (b, lot);
    }

    void Corectie(string id, FacturaScena original, LinieScena lot, decimal q, decimal v, decimal restQ, decimal restV) {
        var nou = Corecteaza(original.Id); FaraEfecte(id, nou);
        FacturaScena corectie;
        using (var os = Deschide()) {
            var doc = os.GetObjectByKey<Asamblare>(nou);
            var l = doc.Detalii.OfType<AsamblareDetaliu>().OrderBy(l => l.Pozitie).ToArray();
            l[0].Cantitate = q; l[1].Cantitate = 1; l[1].PretEvaluare = v;
            corectie = new(nou, l.Select(l => new LinieScena(l.ID, l.LotId, os.GetObjectByKey<Lot>(l.LotId!.Value).ProdusId)).ToArray());
            Verifica(id, "corecție legată, lot produs nou", doc.CorecteazaId == original.Id && doc.DataInregistrare == Februarie
                && corectie.Linii[1].Lot != original.Linii[1].Lot);
            os.CommitChanges();
        }
        var vechi = id == "SC-ASM-18" ? 3.34m : 100m;
        Postari(id, original.Id, N.FelTranzactie.Storno, Februarie,
            Inverse([.. Rand(original, 0, id == "SC-ASM-18" ? -1 : -2, vechi), .. Rand(original, 1, 1, vechi)]));
        Opereaza(nou);
        Postari(id, nou, N.FelTranzactie.Transfer, Februarie, [.. Rand(corectie, 0, -q, v), .. Rand(corectie, 1, 1, v)]);
        Sold(id, lot, restQ, restV, Februarie); Sold(id, original.Linii[1], 0, 0, Februarie);
        Sold(id, corectie.Linii[1], 1, v, Februarie);
    }

    void Capcana() {
        var lot = Receptioneaza(new LinieFctScena(2, 5.005m)).Linii[0];
        var data = new DateOnly(An, 1, 11);
        var d = Culege([(lot, 1)], [(1, 5m, null)], data);
        Distribuie(d.Id);
        Verifica("SC-ASM-19", "valid înaintea consumului intermediar", CuSpatiu(os => OperareApi.Valideaza(os, d.Id)).Count == 0);
        var bcs = Consum(lot.Lot!.Value, 1); Opereaza(bcs);
        var stamp = Amprenta(bcs);
        RefuzDeclaratie("SC-ASM-19", d.Id, CoduriRefuz.AsamblareNebalansata);
        Refuza("SC-ASM-19", () => Opereaza(d.Id), "Valoarea produsă"); FaraEfecte("SC-ASM-19", d.Id);
        Verifica("SC-ASM-19", "BCS intermediar păstrat", stamp == Amprenta(bcs));
        var rest = Scara.ConventieBani == MidpointRounding.ToEven ? 5.01m : 5m;
        Sold("SC-ASM-19", lot, 1, rest);
        Distribuie(d.Id); Opereaza(d.Id);
        Postari("SC-ASM-19", d.Id, N.FelTranzactie.Transfer, data,
            [.. Rand(d, 0, -1, rest), .. Rand(d, 1, 1, rest)]);
        Sold("SC-ASM-19", lot, 0, 0);
    }

    void Distribuie(Guid doc) => Comanda(os => {
        AsamblareApply.DistribuieValoarea(os, () => Deschide(), doc);
        os.CommitChanges();
    });

    void Diagnostic(string id, LinieScena lot, decimal q, decimal delta) {
        using var os = Deschide();
        var raport = DiagnosticValoriStoc.Citeste(((EFCoreObjectSpace)os).DbContext,
            new DateOnly(An, 1, 31), [lot.Lot!.Value]);
        var p = raport.Pozitii.Single(x => x.Cheie.Gestiune == Magazie);
        Verifica(id, "diagnostic valoric: reziduu exact, istoric complet, proveniență și identitate D", !p.Incomplet
            && p.Qcub == q && p.Qregistru == q && p.Delta == delta && p.Reziduu == (delta != 0m)
            && p.Delta == p.Initial + p.Intrari - p.Iesiri
            && p.Contributii.All(c => c.Provenienta.Contains("doc ") && c.AreCub && c.AreRegistru));
        foreach (var linie in raport.Linii()) Console.WriteLine("     " + linie);
    }

    int ContractPur(Guid id) {
        using var os = Deschide(); var doc = os.GetObjectByKey<Document>(id);
        NumaratorSql.Instanta.Reseteaza();
        var contract = Contractare.Contracteaza(os, doc); var citiri = NumaratorSql.Instanta.Numar;
        Verifica("SC-ASM/PERF", "declarație acceptată și deterministă pe draft fără pregătire",
            contract.EsteAcceptat && contract == Contractare.Contracteaza(os, doc));
        return citiri;
    }

    void LantRetur() {
        if (!Privat) return;
        var surse = Receptioneaza(new LinieFctScena(4, 50), new LinieFctScena(3, 20));
        var sursa = surse.Linii[0];
        var asm = Culege([(sursa, 2), (surse.Linii[1], 3)], [(4, 160, "371")]); Opereaza(asm.Id);
        RandScena[] a = [.. Rand(asm, 0, -2, 100, N.FelTranzactie.Operare),
            .. Rand(asm, 1, -3, 60, N.FelTranzactie.Operare),
            .. Rand(asm, 2, 4, 160, N.FelTranzactie.Operare, "371")];
        Postari("SC-X-09", asm.Id, N.FelTranzactie.Operare, Ianuarie, a);
        var lot = asm.Linii[2]; var dsc = Iesire(false, (lot, 1)); Opereaza(dsc.Id);
        RandScena[] d = [new(Cont("607"), N.Latura.Debit, 40, 1, N.GestiuniVirtuale.Client,
                Produs: lot.Produs, Partener: Client, Linie: dsc.Linii[0].Id),
            new(Cont("371"), N.Latura.Credit, 40, -1, Magazie, lot.Lot, lot.Produs,
                Linie: dsc.Linii[0].Id, Spatiu: N.Spatiu.Stoc)];
        Postari("SC-X-09", dsc.Id, N.FelTranzactie.Operare, Ianuarie, d);
        var inainteAsm = Amprenta(asm.Id); var inainteDsc = Amprenta(dsc.Id);
        var rlf = CuSpatiu(os => {
            var doc = os.CreateObject<ReturFurnizor>(); doc.Data = Ianuarie;
            doc.PredatorId = Magazie; doc.PrimitorId = Furnizor;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = doc; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = sursa.Lot; l.Cantitate = 1;
            os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, sursa.Lot, sursa.Produs)]);
        });
        Opereaza(rlf.Id); var partida = Partida(rlf.Id, "401")!.Value;
        RandScena[] r = [new(Cont(Stoc), N.Latura.Debit, -50, -1, Magazie, sursa.Lot, sursa.Produs,
                Linie: rlf.Linii[0].Id, Spatiu: N.Spatiu.Stoc),
            new(Cont("401"), N.Latura.Credit, -50, 1, N.GestiuniVirtuale.Furnizor, partida,
                sursa.Produs, Furnizor, Linie: rlf.Linii[0].Id)];
        Postari("SC-X-09", rlf.Id, N.FelTranzactie.Operare, Ianuarie, r);
        Sold("SC-X-09", sursa, 1, 50); Sold("SC-X-09", lot, 3, 120); SoldPartida("SC-X-09", partida, Ianuarie, 50);
        Verifica("SC-X-09", "returul componentei păstrează ASM și DSC", inainteAsm == Amprenta(asm.Id) && inainteDsc == Amprenta(dsc.Id));
        var stamp = Amprenta(asm.Id);
        Refuza("SC-X-09", () => Storneaza(asm.Id, Ianuarie), "");
        Verifica("SC-X-09", "refuzul păstrează originalul ASM", stamp == Amprenta(asm.Id));
        Storneaza(rlf.Id, Ianuarie); Storneaza(dsc.Id, Ianuarie); Storneaza(asm.Id, Ianuarie);
        Postari("SC-X-09", rlf.Id, N.FelTranzactie.Storno, Ianuarie, Inverse(r));
        Postari("SC-X-09", dsc.Id, N.FelTranzactie.Storno, Ianuarie, Inverse(d));
        Postari("SC-X-09", asm.Id, N.FelTranzactie.Storno, Ianuarie, Inverse(a));
        Sold("SC-X-09", sursa, 4, 200); Sold("SC-X-09", surse.Linii[1], 3, 60);
        Sold("SC-X-09", lot, 0, 0); SoldPartida("SC-X-09", partida, Ianuarie, 0);
    }

    void DeltaFaraAncora() {
        var lot = Receptioneaza(new LinieFctScena(5, 0.004m)).Linii[0];
        Opereaza(Consum(lot.Lot!.Value, .5m)); Opereaza(Consum(lot.Lot.Value, 1));
        var alt = Receptioneaza(new LinieFctScena(1, 1, Tip: Materie)).Linii[0];
        var d = Culege([(lot, 1), (alt, 1)], [(1, 1, Materie)], new(An, 1, 11));
        RefuzDeclaratie("SC-ASM-23", d.Id, CoduriRefuz.AsamblareDeltaFaraAncora);
        Refuza("SC-ASM-23", () => Opereaza(d.Id), CoduriRefuz.AsamblareDeltaFaraAncora);
        FaraEfecte("SC-ASM-23", d.Id);
    }

    void Dependenti() {
        var lot = Receptioneaza(new LinieFctScena(1, 100)).Linii[0];
        var d = Culege([(lot, 1)], [(1, 100, null)]); Opereaza(d.Id);
        var b = Consum(d.Linii[1].Lot!.Value, 1); Opereaza(b);
        var stamp = Amprenta(d.Id);
        Refuza("SC-ASM-14", () => Storneaza(d.Id, new(An, 1, 20)), "Sold negativ");
        Verifica("SC-ASM-14", "original intact", stamp == Amprenta(d.Id));
        Storneaza(b, new(An, 1, 20)); Storneaza(d.Id, new(An, 1, 21));
        Sold("SC-ASM-14", lot, 1, 100); Sold("SC-ASM-14", d.Linii[1], 0, 0);
    }
}
