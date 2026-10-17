using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Irz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Familia EX (E3): închiderea lunară a conturilor de rezultat, REZ-NEINCHIS, politica și seed-ul.</summary>
sealed partial class ScenariiEx(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "EX", 1975) {
    DateOnly Sfarsit => new(An, 1, 31);
    string Venit => Privat ? "704" : "714.00.00";
    string Rezultat => Privat ? "121" : "121.00.00";

    FacturaScena Cheltuiala(decimal v, Guid? repartitor = null) {
        var d = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContPartideLiber, v, RepartitorDebit: repartitor));
        Opereaza(d.Id); return d;
    }
    FacturaScena Castig(decimal v) {
        var d = Nota(Ianuarie, new LinieNtcScena(ContPartideLiber, Venit, v)); Opereaza(d.Id); return d;
    }
    Guid Genereaza(int luna = 1, int? an = null) => CuSpatiu(os => {
        var d = InchidereRezultatService.Genereaza(os, an ?? An, luna, Loc)
            ?? throw new InvalidOperationException("Scena cere un draft IRZ.");
        os.CommitChanges(); return d.ID;
    });
    InchidereRezultatService.RezultatInchidere Previzualizeaza(int luna = 1) =>
        CuSpatiu(os => InchidereRezultatService.Previzualizeaza(os, An, luna));
    decimal Net(string cont, DateOnly data) {
        var id = Cont(cont);
        return CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
            .Where(p => p.Cont == id && p.Data <= data && p.Carte == N.Carte.Contabil)
            .Sum(p => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare));
    }
    PerioadaService.ConstatareInchidere RezNeinchis() => CuSpatiu(os =>
        PerioadaService.Verifica(os, An, 1).SingleOrDefault(c => c.Fel == nameof(FelConstatareInchidere.RezNeinchis)));
    Guid Linie(Guid doc, string debit, string credit) => CuSpatiu(os => os.GetObjectsQuery<InchidereRezultatDetaliu>()
        .Where(l => l.DocumentId == doc && l.ContDebitId == Cont(debit) && l.ContCreditId == Cont(credit))
        .Select(l => l.ID).Single());

    protected override void Executa() {
        Seed();
        var curat = Previzualizeaza();
        Verifica("SC-EX-00", "istoric curat înaintea anului scenei", curat.Motiv == MotivNegenerare.FaraSold && curat.Linii.Count == 0);
        Verifica("SC-EX-00", "REZ-NEINCHIS lipsește pe luna fără sold", RezNeinchis() == null);
        Inchidere();
        Pliere();
        Stale();
        Cronologie();
        Exercitiu();
    }

    void Seed() {
        var randuri = CuSpatiu(os => os.GetObjectsQuery<PoliticaInchidereConturi>()
            .Select(p => new { Sursa = p.ContSursa.Simbol, Destinatie = p.ContDestinatie.Simbol, p.Moment, p.DinSeed,
                p.PastreazaRepartitor, p.CoordonatePastrate }).ToList());
        var lunare = randuri.Where(r => r.Moment == MomentInchidere.Lunar).Select(r => $"{r.Sursa}>{r.Destinatie}").OrderBy(s => s);
        Verifica("SC-EX-10", "rândurile lunare 6/7 → rezultat, din seed",
            lunare.SequenceEqual([$"6>{Rezultat}", $"7>{Rezultat}"]) && randuri.All(r => r.DinSeed));
        var anuale = randuri.Where(r => r.Moment == MomentInchidere.Anual).Select(r => $"{r.Sursa}>{r.Destinatie}").OrderBy(s => s);
        Verifica("SC-EX-10", Privat ? "privatul n-are rânduri anuale (E3-D2)" : "rândurile anuale ale bugetarului (E3-D4, E3-D9)",
            anuale.SequenceEqual(Privat ? [] : ["121>117.00.00", "489>117.00.00", "520.02>521.03.00", "521.02>521.03.00"]));
        var severitate = CuSpatiu(os => os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.RezNeinchis)?.Severitate);
        Verifica("SC-EX-10", "REZ-NEINCHIS e blocant din seed (E3-D5)", severitate == SeveritateConstatare.Blocant);
    }

    void Inchidere() {
        var chelt = Cheltuiala(100); var venit = Castig(40);
        var lipsa = RezNeinchis();
        Verifica("SC-EX-05", "REZ-NEINCHIS blocant fără document", lipsa is { Cheie: "REZ-NEINCHIS", Severitate: SeveritateConstatare.Blocant, ObiectId: null });

        var raport = Previzualizeaza();
        Verifica("SC-EX-01", "previzualizarea: două linii, fără document", raport.Motiv == null && raport.Document == null
            && raport.Linii.Count == 2 && raport.Linii.Sum(l => l.Valoare) == 140);
        var doc = Genereaza();
        Verifica("SC-EX-01", "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(doc)).Count == 0);
        FaraEfecte("SC-EX-01", doc);
        var draft = RezNeinchis();
        Verifica("SC-EX-05", "draftul e obiectul constatării și nu se repetă ca DRAFT-IN-PERIOADA",
            draft?.ObiectId == doc && CuSpatiu(os => !PerioadaService.Verifica(os, An, 1).Any(c => c.Cheie.EndsWith(doc.ToString()))));
        var vie = CuSpatiu(os => InchidereRezultatService.Incearca(os, An, 1, Loc));
        Verifica("SC-EX-02", "nu dublează închiderea vie", vie.Document == null && vie.Motiv == MotivNegenerare.InchidereVie && vie.InchidereVieId == doc);

        Opereaza(doc);
        var economic = Privat ? null : Economic;
        var cheltRezultat = Linie(doc, Rezultat, Serviciu);
        var venitRezultat = Linie(doc, Venit, Rezultat);
        var repartitorDestinatie = Privat ? (Guid?)null : Loc;
        Postari("SC-EX-01", doc, N.FelTranzactie.Operare, Sfarsit,
            new(Cont(Serviciu), N.Sens.Credit, 100, Repartitor: Loc, Linie: cheltRezultat, Economic: economic),
            new(Cont(Rezultat), N.Sens.Debit, 100, Repartitor: repartitorDestinatie, Linie: cheltRezultat, Economic: economic),
            new(Cont(Venit), N.Sens.Debit, 40, Repartitor: Loc, Linie: venitRezultat, Economic: economic),
            new(Cont(Rezultat), N.Sens.Credit, 40, Repartitor: repartitorDestinatie, Linie: venitRezultat, Economic: economic));
        Verifica("SC-EX-01", "6 și 7 la zero, rezultatul = pierderea lunii",
            Net(Serviciu, Sfarsit) == 0 && Net(Venit, Sfarsit) == 0 && Net(Rezultat, Sfarsit) == 60);
        Verifica("SC-EX-05", "REZ-NEINCHIS dispare după operare", RezNeinchis() == null);

        var tarziu = Cheltuiala(7);
        var rest = RezNeinchis();
        Verifica("SC-EX-05", "documentul operat după închidere lasă sold, iar constatarea arată închiderea",
            rest is { Severitate: SeveritateConstatare.Blocant } && rest.ObiectId == doc && rest.Text.Contains("operată"));
        Storneaza(tarziu.Id, Sfarsit);

        Storneaza(doc, Sfarsit);
        Verifica("SC-EX-03", "stornoul readuce soldurile", Net(Serviciu, Sfarsit) == 100 && Net(Venit, Sfarsit) == -40 && Net(Rezultat, Sfarsit) == 0);
        var regenerat = Genereaza();
        Verifica("SC-EX-03", "după storno luna se generează din nou", regenerat != doc);
        Comanda(os => InchidereRezultatApply.Sterge(os, regenerat));
        Storneaza(chelt.Id, Sfarsit); Storneaza(venit.Id, Sfarsit);
        Verifica("SC-EX-00", "scena lasă luna fără sold", Previzualizeaza().Motiv == MotivNegenerare.FaraSold);
    }

    void Pliere() {
        var a = Cheltuiala(30); var b = Cheltuiala(20, Destinatie);
        var raport = Previzualizeaza();
        Verifica("SC-EX-04", "o linie per cheie-sursă", raport.Linii.Count == 2
            && raport.Linii.Select(l => l.Sursa.Repartitor).ToHashSet().SetEquals(new Guid?[] { Loc, Destinatie }));
        var doc = Genereaza(); Opereaza(doc);
        var cheiRezultat = CuSpatiu(os => {
            var id = Cont(Rezultat);
            return os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Cont == id)
                .Select(p => new { p.Repartitor, p.CodEconomic }).Distinct().Count();
        });
        Verifica("SC-EX-04", Privat ? "destinația fără coordonate pliază cheile într-una singură"
                : "destinația păstrează repartitorul declarat, deci rămân două chei",
            cheiRezultat == (Privat ? 1 : 2) && Net(Rezultat, Sfarsit) == 50);
        Storneaza(doc, Sfarsit); Storneaza(a.Id, Sfarsit); Storneaza(b.Id, Sfarsit);
    }

    void Stale() {
        var baza = Cheltuiala(10); var doc = Genereaza();
        var plus = Cheltuiala(5);
        RefuzaOperarea("SC-EX-06", doc, "s-au schimbat"); FaraEfecte("SC-EX-06", doc);
        Verifica("SC-EX-06", "draftul e raportat stale", CuSpatiu(os => InchidereRezultatApply.Stale(os, doc)) == true);
        var nou = CuSpatiu(os => InchidereRezultatApply.Regenereaza(os, doc));
        Verifica("SC-EX-06", "regenerarea înlocuiește draftul", nou.DocumentId is Guid id && id != doc
            && CuSpatiu(os => os.GetObjectByKey<Document>(doc)) == null);
        Opereaza(nou.DocumentId!.Value);
        Verifica("SC-EX-06", "regenerarea închide și documentul nou", Net(Serviciu, Sfarsit) == 0 && Net(Rezultat, Sfarsit) == 15);
        Storneaza(nou.DocumentId.Value, Sfarsit); Storneaza(plus.Id, Sfarsit); Storneaza(baza.Id, Sfarsit);
    }

    void Cronologie() {
        var baza = Cheltuiala(10);
        var anterior = Genereaza();
        Refuza("SC-EX-07", () => Genereaza(2), "draft");
        Verifica("SC-EX-07", "raportul spune draftul anterior", Previzualizeaza(2) is { Motiv: MotivNegenerare.DraftAnterior } r && r.InchidereVieId == anterior);
        Comanda(os => InchidereRezultatApply.Sterge(os, anterior));
        var ulterior = Genereaza(2);
        Refuza("SC-EX-07", () => Genereaza(), "ulterioară");
        Verifica("SC-EX-07", "raportul spune închiderea ulterioară", Previzualizeaza() is { Motiv: MotivNegenerare.NeCronologica } u && u.InchidereVieId == ulterior);
        Comanda(os => InchidereRezultatApply.Sterge(os, ulterior));
        Storneaza(baza.Id, Sfarsit);
    }
}
