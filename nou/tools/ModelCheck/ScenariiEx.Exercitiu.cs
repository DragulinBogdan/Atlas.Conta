using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Exercițiul închis pe decembrie (E3-D7), rândurile anuale (E3-D9) și SC-X-12, pe 1975 și 1976.</summary>
sealed partial class ScenariiEx {
    protected override int UltimulAn => An + 1;
    int Urmator => An + 1;
    string Report => Privat ? "1171" : "117.00.00";
    // E3-D9: un rând anual pe care seed-ul nu-l are.
    (string Sursa, string Destinatie, DimensiuneFlags Pastrate) RandNou => Privat
        ? ("121", "1171", DimensiuneFlags.Niciuna)
        : ("520.01", "521.01.00", DimensiuneFlags.CodFunctional | DimensiuneFlags.CodEconomic | DimensiuneFlags.SursaFinantare);

    static DateOnly Capat(int an) => new(an, 12, 31);
    static DateOnly Decembrie(int an) => new(an, 12, 15);

    void NotaOperata(int an, string debit, string credit, decimal v) =>
        Opereaza(Nota(Decembrie(an), new LinieNtcScena(debit, credit, v)).Id);

    IReadOnlyList<PerioadaService.ConstatareInchidere> VerificaExercitiul(int an) =>
        CuSpatiu(os => PerioadaService.VerificaExercitiul(os, an));

    void InchideExercitiul(int an) => Comanda(os => PerioadaService.InchideExercitiul(os, an, [], null, "ModelCheck"));
    void RedeschideExercitiul(int an, string motiv) =>
        Comanda(os => PerioadaService.RedeschideExercitiul(os, an, motiv, null, "ModelCheck"));

    Guid InchideDecembrie(int an) {
        var doc = Genereaza(12, an); Opereaza(doc); Inchide(an, 12); return doc;
    }

    PerioadaFiscala DecembrieDin(IObjectSpace os, int an) => os.FirstOrDefault<PerioadaFiscala>(p => p.An == an && p.Luna == 12);

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        var sursa = RandNou.Sursa;
        purja.Adauga(os.GetObjectsQuery<PoliticaInchidereConturi>().Where(p => !p.DinSeed && p.ContSursa.Simbol == sursa));
    }

    void Exercitiu() {
        Verifica("SC-EX-20", "exercițiul fără decembrie definit: o singură constatare blocantă",
            VerificaExercitiul(An) is [{ Cheie: "DECEMBRIE-NEDEFINIT", Severitate: SeveritateConstatare.Blocant }]);
        Comanda(os => {
            foreach (var an in new[] { An, Urmator }) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = an; p.Luna = 12;
            }
            os.CommitChanges();
        });
        Verifica("SC-EX-20", "decembrie deschis blochează exercițiul",
            VerificaExercitiul(An) is [{ Cheie: "DECEMBRIE-DESCHIS", Severitate: SeveritateConstatare.Blocant }]);
        Verifica("SC-EX-20", "exercițiul următor așteaptă exercițiul precedent",
            VerificaExercitiul(Urmator).Select(c => c.Cheie).Order()
                .SequenceEqual(["DECEMBRIE-DESCHIS", $"EXERCITIU-PRECEDENT-DESCHIS:{An}"]));

        NotaOperata(An, Serviciu, ContPartideLiber, 100);
        NotaOperata(An, ContPartideLiber, Venit, 40);
        if (!Privat) {
            NotaOperata(An, ContPartideLiber, "489.01.01", 30);
            NotaOperata(An, "520.02.00", ContPartideLiber, 25);
        }
        SoldRezidual();
        Anuale();
        Inchis();
        RandNeseed();
        Redeschidere();
    }

    void SoldRezidual() {
        Inchide(An, 12);
        var constatari = VerificaExercitiul(An);
        Verifica("SC-EX-23", "decembrie închis fără închiderea conturilor: EX-SOLD-REZIDUAL blocant, cu conturile-sursă",
            constatari is [{ Cheie: "EX-SOLD-REZIDUAL", Severitate: SeveritateConstatare.Blocant } c] && c.Text.Contains(Serviciu));
        Refuza("SC-EX-23", () => Comanda(os => PerioadaService.InchideExercitiul(os, An, ["EX-SOLD-REZIDUAL"], null, "ModelCheck")),
            "EX-SOLD-REZIDUAL");
        Comanda(os => PerioadaService.Redeschide(os, An, 12, "SC-EX-23: închiderea conturilor lipsește", null, "ModelCheck"));
    }

    void Anuale() {
        var raport = Previzualizeaza(12);
        var momente = CuSpatiu(os => os.GetObjectsQuery<PoliticaInchidereConturi>().ToDictionary(p => p.ID, p => p.Moment));
        var ordine = raport.Linii.Select(l => momente[l.PoliticaId]).ToList();
        Verifica("SC-EX-21", Privat ? "decembrie privat: numai rândurile lunare" : "decembrie bugetar: rândurile anuale după cele lunare",
            raport.Motiv == null && ordine.SequenceEqual(ordine.Order())
            && ordine.Count(m => m == MomentInchidere.Anual) == (Privat ? 0 : 3));
        var doc = InchideDecembrie(An);
        Verifica("SC-EX-21", "închiderea e datată 31.12 și nu postează în anul nou",
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc).All(p => p.Data == Capat(An))));
        var capat = Capat(An);
        Verifica("SC-X-12", "6 și 7 la zero la 31.12", Net(Serviciu, capat) == 0 && Net(Venit, capat) == 0);
        Verifica("SC-X-12", Privat ? "121 rămâne cu rezultatul anului (E3-D2)" : "121 și 489 trec în 117 (E3-D1)",
            Privat ? Net(Rezultat, capat) == 60 && Net(Report, capat) == 0
                : Net(Rezultat, capat) == 0 && Net("489.01.01", capat) == 0 && Net(Report, capat) == 30);
        if (!Privat)
            Verifica("SC-EX-21", "execuția bugetară: 520.02 trece în 521.03 (E3-D9)",
                Net("520.02.00", capat) == 0 && Net("521.03.00", capat) == 25);
    }

    void Inchis() {
        Verifica("SC-EX-25", "decembrie închis cu închiderea operată: exercițiul n-are constatări", VerificaExercitiul(An).Count == 0);
        InchideExercitiul(An);
        var (stare, istoric) = CuSpatiu(os => {
            var d = DecembrieDin(os, An);
            return ((d.Inchisa, d.ExercitiuInchis, d.ExercitiuInchisLa),
                os.GetObjectsQuery<InchiderePerioada>().Where(i => i.PerioadaId == d.ID).OrderBy(i => i.La).Select(i => i.Fel).ToList());
        });
        Verifica("SC-EX-25", "exercițiul închis e starea lui decembrie, cu momentul și rândul de istoric",
            stare is (true, true, not null) && istoric.LastOrDefault() == FelInchiderePerioada.InchidereExercitiu);
        Verifica("SC-EX-25", "exercițiul deja închis nu se închide a doua oară",
            VerificaExercitiul(An) is [{ Cheie: "EXERCITIU-INCHIS", Severitate: SeveritateConstatare.Blocant }]);
        Verifica("SC-X-12", "închiderea exercițiului nu scrie nicio tranzacție de deschidere",
            CuSpatiu(os => !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.Fel == N.FelTranzactie.Deschidere
                && t.Data >= new DateOnly(An, 1, 1) && t.Data <= Capat(Urmator))));
        Refuza("SC-EX-24", () => Comanda(os => PerioadaService.Redeschide(os, An, 12, "probă", null, "ModelCheck")),
            "redeschideți întâi exercițiul");
    }

    void RandNeseed() {
        var (sursa, destinatie, pastrate) = RandNou;
        Comanda(os => {
            var r = os.CreateObject<PoliticaInchidereConturi>();
            r.Moment = MomentInchidere.Anual; r.Ordine = 99;
            r.ContSursaId = Cont(sursa); r.ContDestinatieId = Cont(destinatie); r.CoordonatePastrate = pastrate;
            os.CommitChanges();
        });
        NotaOperata(Urmator, Serviciu, ContPartideLiber, 10);
        if (!Privat)
            NotaOperata(Urmator, "520.01.00", ContPartideLiber, 15);
        var doc = InchideDecembrie(Urmator);
        var capat = Capat(Urmator);
        Verifica("SC-EX-22", Privat ? "rândul anual adăugat fără cod: 121 trece în 1171 cu rezultatul cumulat"
                : "rândul anual adăugat fără cod: 520.01 trece în 521.01",
            Privat ? Net(Rezultat, capat) == 0 && Net(Report, capat) == 70
                : Net("520.01.00", capat) == 0 && Net("521.01.00", capat) == 15 && Net(Report, capat) == 40);
        Verifica("SC-EX-22", "liniile rândului nou poartă coordonatele lui",
            CuSpatiu(os => os.GetObjectsQuery<InchidereRezultatDetaliu>().Where(l => l.DocumentId == doc
                && (l.ContDebitId == Cont(destinatie) || l.ContCreditId == Cont(destinatie)))
                .Select(l => l.CoordonatePastrate).ToList()) is [var p] && p == pastrate);
        Verifica("SC-EX-20", "exercițiul următor se închide după cel precedent", VerificaExercitiul(Urmator).Count == 0);
        InchideExercitiul(Urmator);
    }

    void Redeschidere() {
        Refuza("SC-EX-24", () => RedeschideExercitiul(An, "probă"), "doar ultimul exercițiu închis");
        Refuza("SC-EX-24", () => RedeschideExercitiul(Urmator, " "), "motiv");
        RedeschideExercitiul(Urmator, "SC-EX-24: ajustare");
        RedeschideExercitiul(An, "SC-EX-24: ajustare");
        var stari = CuSpatiu(os => new[] { An, Urmator }.Select(an => {
            var d = DecembrieDin(os, an);
            var ultim = os.GetObjectsQuery<InchiderePerioada>().Where(i => i.PerioadaId == d.ID).OrderByDescending(i => i.La).First();
            return (d.Inchisa, d.ExercitiuInchis, d.ExercitiuInchisLa, ultim.Fel, ultim.Motiv);
        }).ToList());
        Verifica("SC-EX-24", "redeschiderea, de la ultimul exercițiu: decembrie rămâne închis, istoricul poartă motivul",
            stari.All(s => s is (true, false, null, FelInchiderePerioada.RedeschidereExercitiu, "SC-EX-24: ajustare")));
        Refuza("SC-EX-24", () => RedeschideExercitiul(An, "probă"), "nu e închis");
    }
}
