using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Comenzile exercițiului (E3-D7): starea stă pe decembrie, istoricul pe `InchiderePerioada`.
public static partial class PerioadaService {
    const string FelDecembrieNedefinit = "DECEMBRIE-NEDEFINIT";
    const string FelDecembrieDeschis = "DECEMBRIE-DESCHIS";
    const string FelExercitiuInchis = "EXERCITIU-INCHIS";
    const string FelExercitiuPrecedentDeschis = "EXERCITIU-PRECEDENT-DESCHIS";

    /// <summary>Anul ultimului exercițiu închis; null când niciunul nu e închis.</summary>
    public static int? UltimulExercitiuInchis(IObjectSpace os) =>
        os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.Luna == 12 && p.ExercitiuInchis).Max(p => (int?)p.An);

    /// <summary>Ce împiedică închiderea exercițiului, fără să scrie nimic.</summary>
    public static IReadOnlyList<ConstatareInchidere> VerificaExercitiul(IObjectSpace os, int an) {
        var constatari = new List<ConstatareInchidere>();
        var eticheta = Eticheta(an, 12);
        var decembrie = Gaseste(os, an, 12);
        if (decembrie == null) {
            constatari.Add(new ConstatareInchidere(FelDecembrieNedefinit, FelDecembrieNedefinit, SeveritateConstatare.Blocant,
                $"Perioada {eticheta} nu e definită — exercițiul {an} se închide pe decembrie închis.", null, eticheta));
            return constatari;
        }
        if (decembrie.ExercitiuInchis)
            constatari.Add(new ConstatareInchidere(FelExercitiuInchis, FelExercitiuInchis, SeveritateConstatare.Blocant,
                $"Exercițiul {an} e deja închis.", decembrie.ID, eticheta));
        else if (!decembrie.Inchisa)
            constatari.Add(new ConstatareInchidere(FelDecembrieDeschis, FelDecembrieDeschis, SeveritateConstatare.Blocant,
                $"Perioada {eticheta} e deschisă — exercițiul se închide după închiderea lui decembrie.", decembrie.ID, eticheta));

        if (Gaseste(os, an - 1, 12) is { ExercitiuInchis: false } precedent)
            constatari.Add(new ConstatareInchidere($"{FelExercitiuPrecedentDeschis}:{an - 1}", FelExercitiuPrecedentDeschis,
                SeveritateConstatare.Blocant,
                $"Exercițiul {an - 1} e deschis — exercițiile se închid în ordine.", precedent.ID, Eticheta(an - 1, 12)));

        if (constatari.Count == 0)
            SoldRezidual(os, an, constatari);
        return constatari;
    }

    // E3-D7: conturile din politică (lunare și anuale) au sold la 31.12.
    static void SoldRezidual(IObjectSpace os, int an, List<ConstatareInchidere> constatari) {
        const FelConstatareInchidere fel = FelConstatareInchidere.ExSoldRezidual;
        var politica = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == fel);
        if (politica?.Severitate == SeveritateConstatare.Ignorat)
            return;
        var conturi = InchidereRezultatService.CalculeazaLinii(os, new DateOnly(an, 12, 31), cuAnuale: true)
            .Select(l => l.Sursa.Cont).Distinct().ToList();
        if (conturi.Count == 0)
            return;
        var simboluri = os.GetObjectsQuery<Cont>().Where(c => conturi.Contains(c.ID))
            .Select(c => c.Simbol).ToList().OrderBy(s => s, StringComparer.Ordinal).ToList();
        var lista = string.Join(", ", simboluri.Take(10)) + (simboluri.Count > 10 ? $" și încă {simboluri.Count - 10}" : "");
        constatari.Add(new ConstatareInchidere(Cheie(fel), fel.ToString(),
            politica?.Severitate ?? SeveritateConstatare.Avertisment,
            $"Conturile din politica închiderii au sold la 31.12.{an} ({lista}) — redeschideți decembrie, stornați "
                + "închiderea lui operată, apoi generați-o și operați-o din nou, cu rândurile anuale."
                + (politica == null ? " (fără politică — avertisment implicit)" : ""),
            null, Eticheta(an, 12)));
    }

    /// <summary>Închide exercițiul dacă verificarea o permite; scrie rândul de istoric pe decembrie. Comite.</summary>
    public static InchiderePerioada InchideExercitiul(IObjectSpace os, int an,
            IReadOnlyCollection<string> acceptate, Guid? deId, string de) {
        using var tx = TranzactieComanda.Incepe(os);
        Blocheaza(os, an - 1, 12);
        Blocheaza(os, an, 12);
        var constatari = VerificaExercitiul(os, an);
        var acceptateCurate = Acceptate(acceptate);
        RefuzaNeacoperite(constatari, acceptateCurate);

        var decembrie = Gaseste(os, an, 12);
        var acum = DateTime.UtcNow;
        decembrie.ExercitiuInchis = true;
        decembrie.ExercitiuInchisLa = acum;
        var rand = Istoric(os, decembrie, FelInchiderePerioada.InchidereExercitiu, acum, deId, de);
        rand.Acceptari = Acceptari(constatari, acceptateCurate);
        os.CommitChanges();
        tx.Commit();
        return rand;
    }

    /// <summary>Redeschide ULTIMUL exercițiu închis, cu motiv. Nu stornează nimic: rândurile anuale sunt datate 31.12. Comite.</summary>
    public static InchiderePerioada RedeschideExercitiul(IObjectSpace os, int an, string motiv, Guid? deId, string de) {
        using var tx = TranzactieComanda.Incepe(os);
        Blocheaza(os, an, 12);
        Blocheaza(os, an + 1, 12);
        var erori = new List<string>();
        var decembrie = Gaseste(os, an, 12);
        if (decembrie is not { ExercitiuInchis: true })
            erori.Add($"Exercițiul {an} nu e închis — nu are ce redeschide.");
        if (string.IsNullOrWhiteSpace(motiv))
            erori.Add("Redeschiderea exercițiului cere un motiv scris.");
        if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.Luna == 12 && p.ExercitiuInchis && p.An > an))
            erori.Add($"Un exercițiu ulterior lui {an} e închis — se redeschide doar ultimul exercițiu închis.");
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));

        var acum = DateTime.UtcNow;
        decembrie.ExercitiuInchis = false;
        decembrie.ExercitiuInchisLa = null;
        var rand = Istoric(os, decembrie, FelInchiderePerioada.RedeschidereExercitiu, acum, deId, de);
        rand.Motiv = motiv.Trim();
        os.CommitChanges();
        tx.Commit();
        return rand;
    }
}
