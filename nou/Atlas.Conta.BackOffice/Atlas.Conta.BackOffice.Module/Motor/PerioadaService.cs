using System.Runtime.CompilerServices;
using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Comenzile perioadei (F27-D1/D2).
public static class PerioadaService {
    // `Cheie` = constatarea concretă (acceptarea se dă pe ea), `Fel` = familia (F27-D2).
    // `Fel` e numele membrului `FelConstatareInchidere` pentru constatările DE
    // CONȚINUT și cheia structurală pentru celelalte: blocantele lanțului nu se
    // configurează, deci n-au membru de enum și nici rând de politică.
    public sealed record ConstatareInchidere(string Cheie, string Fel, SeveritateConstatare Severitate,
        string Text, Guid? ObiectId, string ObiectEticheta);

    const string FelNedefinita = "PERIOADA-NEDEFINITA";
    const string FelInchisa = "PERIOADA-INCHISA";
    const string FelPrecedentaDeschisa = "PRECEDENTA-DESCHISA";

    // Câte constatări individuale se enumeră per familie înainte de rândul de
    // rezumat (tiparul `VerificareProfilService`): o listă de mii de drafturi nu
    // se citește, iar acceptarea pe cheie ar deveni imposibilă oricum.
    const int MaximPerFel = 200;

    /// <summary>Lanțul perioadelor definite, în ordine cronologică.</summary>
    public static IReadOnlyList<PerioadaFiscala> Lant(IObjectSpace os) =>
        os.GetObjectsQuery<PerioadaFiscala>().OrderBy(p => p.An).ThenBy(p => p.Luna).ToList();

    /// <summary>Ce împiedică închiderea lunii, fără să scrie nimic.</summary>
    public static IReadOnlyList<ConstatareInchidere> Verifica(IObjectSpace os, int an, int luna) {
        var constatari = new List<ConstatareInchidere>();
        var perioada = Gaseste(os, an, luna);
        if (perioada == null) {
            constatari.Add(new ConstatareInchidere(FelNedefinita, FelNedefinita, SeveritateConstatare.Blocant,
                $"Perioada {Eticheta(an, luna)} nu e definită — o perioadă inexistentă e tratată ca închisă și nu se poate închide.",
                null, Eticheta(an, luna)));
            return constatari;
        }
        if (perioada.Inchisa)
            constatari.Add(new ConstatareInchidere(FelInchisa, FelInchisa, SeveritateConstatare.Blocant,
                $"Perioada {Eticheta(an, luna)} e deja închisă.", perioada.ID, Eticheta(an, luna)));

        // Precedenta absentă = închisă prin absență (capătul lanțului, F27-D1).
        var (anPrecedent, lunaPrecedenta) = Precedenta(an, luna);
        var precedenta = Gaseste(os, anPrecedent, lunaPrecedenta);
        if (precedenta != null && !precedenta.Inchisa)
            constatari.Add(new ConstatareInchidere(
                $"{FelPrecedentaDeschisa}:{anPrecedent}-{lunaPrecedenta:00}", FelPrecedentaDeschisa,
                SeveritateConstatare.Blocant,
                $"Perioada precedentă {Eticheta(anPrecedent, lunaPrecedenta)} e deschisă — perioadele se închid în lanț, în ordine.",
                precedenta.ID, Eticheta(anPrecedent, lunaPrecedenta)));

        // Conținutul se caută DOAR pe o lună care altfel s-ar putea închide: pe
        // una deja închisă sau cu precedenta deschisă verdictul e dat, iar cele
        // patru interogări de mai jos n-ar adăuga decât cost.
        if (constatari.Count == 0)
            Continut(os, an, luna, constatari);
        return constatari;
    }

    // ═══════════════════ Constatările DE CONȚINUT (F27-D2) ═════════════════════
    //
    // Severitatea lor e POLITICĂ (`PoliticaInchidere`), nu cod: același fapt e
    // blocant pe un profil și inert pe altul (ITV pe privat vs. bugetar). Un fel
    // fără rând de politică iese ca avertisment, dar SPUNE că politica lipsește —
    // un default tăcut ar fi ascuns o configurație pe jumătate.
    static void Continut(IObjectSpace os, int an, int luna, List<ConstatareInchidere> constatari) {
        var politici = os.GetObjectsQuery<PoliticaInchidere>()
            .Select(p => new { p.Fel, p.Severitate }).ToList()
            .ToDictionary(p => p.Fel, p => p.Severitate);
        var ultimaZi = UltimaZi(an, luna);
        var primaZi = new DateOnly(an, luna, 1);

        // `Ignorat` nu e „constatare tăcută", e una care NU EXISTĂ: felul nici
        // nu se caută, deci nici nu costă. Felul fără rând de politică se caută
        // și iese ca avertisment, dar SPUNE că politica lipsește — un default
        // tăcut ar fi ascuns o configurație pe jumătate.
        bool Activ(FelConstatareInchidere fel) =>
            !politici.TryGetValue(fel, out var severitate) || severitate != SeveritateConstatare.Ignorat;

        (SeveritateConstatare Severitate, string Sufix) Politica(FelConstatareInchidere fel) =>
            politici.TryGetValue(fel, out var severitate)
                ? (severitate, "")
                : (SeveritateConstatare.Avertisment, " (fără politică — avertisment implicit)");

        void Adauga(FelConstatareInchidere fel, string cheie, string text, Guid? obiectId, string eticheta) {
            var (severitate, sufix) = Politica(fel);
            constatari.Add(new ConstatareInchidere(cheie, fel.ToString(), severitate, text + sufix,
                obiectId, eticheta));
        }

        // Rândul de rezumat se acceptă, iar acceptarea lui e ÎN BLOC: acoperă
        // toate rândurile nelistate ale familiei, la severitatea ei (F27-D2).
        void Rezumat(FelConstatareInchidere fel, int total) {
            if (total <= MaximPerFel)
                return;
            var (severitate, sufix) = Politica(fel);
            constatari.Add(new ConstatareInchidere($"{Cheie(fel)}:REZUMAT", fel.ToString(), severitate,
                $"Încă {Randuri(total - MaximPerFel)} de același fel (din {total} în total) — nelistate; "
                + "acceptarea acestui rând le acceptă pe toate." + sufix,
                null, "(rezumat)"));
        }

        // Un fapt, o constatare: draftul deja raportat de familia lui nu se mai
        // repetă ca `DRAFT-IN-PERIOADA` (F27-D2).
        var raportate = new List<Guid>(2);
        if (Activ(FelConstatareInchidere.ItvLipsa)
                && ItvLipsa(os, an, luna, primaZi, ultimaZi, Adauga) is { } itvId)
            raportate.Add(itvId);
        if (Activ(FelConstatareInchidere.AmoLipsa)
                && AmoLipsa(os, an, luna, primaZi, ultimaZi, Adauga) is { } amoId)
            raportate.Add(amoId);
        if (Activ(FelConstatareInchidere.DraftInPerioada))
            DraftInPerioada(os, an, luna, primaZi, ultimaZi, raportate, Adauga, Rezumat);
        if (Activ(FelConstatareInchidere.RestScadent))
            RestScadent(os, an, luna, ultimaZi, Adauga, Rezumat);
    }

    // ITV: luna se închide cu decontul ei făcut. Profilul fără politică de
    // închidere de TVA (bugetarul) n-are ce deconta — tip inert, nicio
    // constatare; iar o lună fără sold pe cele două conturi n-avea ce închide.
    static Guid? ItvLipsa(IObjectSpace os, int an, int luna, DateOnly primaZi, DateOnly ultimaZi,
            Action<FelConstatareInchidere, string, string, Guid?, string> adauga) {
        var previzualizare = InchidereTvaService.Previzualizeaza(os, an, luna);
        if (previzualizare.Motiv is MotivNegenerare.ProfilInert or MotivNegenerare.FaraSold)
            return null;
        var vie = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat)
            .Select(d => new { d.ID, d.Numar, d.Stare })
            .FirstOrDefault();
        if (vie is { Stare: StareDocument.Operat })
            return null;
        var text = vie == null
            ? $"Închiderea de TVA pentru {Eticheta(an, luna)} lipsește — soldurile de TVA ale lunii rămân "
                + "deschise, iar după închiderea perioadei nu se mai pot deconta pe ea."
            : $"Închiderea de TVA pentru {Eticheta(an, luna)} există ca DRAFT neoperat "
                + $"({vie.Numar ?? "fără număr"}) — un draft nu scrie registre, deci soldurile lunii rămân deschise.";
        adauga(FelConstatareInchidere.ItvLipsa, Cheie(FelConstatareInchidere.ItvLipsa), text,
            vie?.ID, vie?.Numar ?? Eticheta(an, luna));
        return vie?.ID;
    }

    // AMO: calculul lunii e al SOCIETĂȚII, nu al unei unități interne
    // (`AmortizareService.CalculeazaLinii` n-are parametru de unitate — unitatea
    // e doar latura documentului generat), deci cheia n-are sufix.
    static Guid? AmoLipsa(IObjectSpace os, int an, int luna, DateOnly primaZi, DateOnly ultimaZi,
            Action<FelConstatareInchidere, string, string, Guid?, string> adauga) {
        if (AmortizareService.CalculeazaLinii(os, an, luna).Count == 0)
            return null;
        var vie = os.GetObjectsQuery<AmortizareLunara>()
            .Where(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat)
            .Select(d => new { d.ID, d.Numar, d.Stare })
            .FirstOrDefault();
        if (vie is { Stare: StareDocument.Operat })
            return null;
        var text = vie == null
            ? $"Amortizarea lunară pentru {Eticheta(an, luna)} lipsește, deși există fișe de amortizat — "
                + "după închidere luna nu mai poate primi amortizarea ei."
            : $"Amortizarea lunară pentru {Eticheta(an, luna)} există ca DRAFT neoperat "
                + $"({vie.Numar ?? "fără număr"}) — un draft nu scrie registrul imobilizărilor.";
        adauga(FelConstatareInchidere.AmoLipsa, Cheie(FelConstatareInchidere.AmoLipsa), text,
            vie?.ID, vie?.Numar ?? Eticheta(an, luna));
        return vie?.ID;
    }

    // Draftul din perioadă NU e un document mort (F27-D4): după închidere rămâne
    // operabil, cu o dată de înregistrare ulterioară. Constatarea spune exact
    // asta — altfel operatorul ar crede că pierde culegerea.
    static void DraftInPerioada(IObjectSpace os, int an, int luna, DateOnly primaZi, DateOnly ultimaZi,
            List<Guid> raportate,
            Action<FelConstatareInchidere, string, string, Guid?, string> adauga,
            Action<FelConstatareInchidere, int> rezumat) {
        var interogare = os.GetObjectsQuery<Document>()
            .Where(d => d.Stare == StareDocument.Draft
                && d.DataInregistrare >= primaZi && d.DataInregistrare <= ultimaZi
                && !raportate.Contains(d.ID));
        var drafturi = interogare
            .OrderBy(d => d.DataInregistrare).ThenBy(d => d.Numar)
            .Take(MaximPerFel + 1)
            .ToList();
        var total = drafturi.Count <= MaximPerFel ? drafturi.Count : interogare.Count();
        foreach (var draft in drafturi.Take(MaximPerFel)) {
            var eticheta = $"{MotorOperare.ClasaReala(draft).Name} {draft.Numar ?? "fără număr"}";
            adauga(FelConstatareInchidere.DraftInPerioada,
                $"{Cheie(FelConstatareInchidere.DraftInPerioada)}:{draft.ID}",
                $"Documentul {eticheta} e în lucru cu data înregistrării în {Eticheta(an, luna)} — după "
                + "închidere rămâne operabil, dar cu o dată de înregistrare ULTERIOARĂ, deci va cădea în "
                + "altă perioadă decât cea a documentului fizic.",
                draft.ID, eticheta);
        }
        rezumat(FelConstatareInchidere.DraftInPerioada, total);
    }

    // Restul scadent: informativ prin seed (`Ignorat`), fiindcă o factură
    // neîncasată nu împiedică închiderea. Cine îl vrea, îl ridică din politică.
    static void RestScadent(IObjectSpace os, int an, int luna, DateOnly ultimaZi,
            Action<FelConstatareInchidere, string, string, Guid?, string> adauga,
            Action<FelConstatareInchidere, int> rezumat) {
        var scadente = os.GetObjectsQuery<FacturaIntrare>()
                .Where(d => d.Stare == StareDocument.Operat && d.DataScadenta <= ultimaZi)
                .Select(d => d.ID)
            .Concat(os.GetObjectsQuery<FacturaIesire>()
                .Where(d => d.Stare == StareDocument.Operat && d.DataScadenta <= ultimaZi)
                .Select(d => d.ID))
            .ToList().ToHashSet();
        if (scadente.Count == 0)
            return;
        var restante = ImperecheriProiectii.DocumenteCuRest(os, laData: ultimaZi)
            .Select(r => new { r.DocumentId, r.Tip, r.Numar, r.Rest, r.ContrapartidaDenumire })
            .ToList()
            .Where(r => scadente.Contains(r.DocumentId))
            .OrderBy(r => r.Numar)
            .ToList();
        foreach (var rand in restante.Take(MaximPerFel)) {
            var eticheta = $"{rand.Tip} {rand.Numar ?? "fără număr"}";
            adauga(FelConstatareInchidere.RestScadent,
                $"{Cheie(FelConstatareInchidere.RestScadent)}:{rand.DocumentId}",
                $"Documentul {eticheta} ({rand.ContrapartidaDenumire}) e scadent și are rest "
                + $"{rand.Rest:0.00} la sfârșitul lui {Eticheta(an, luna)} — restul se îngheață în partidele "
                + "deschise ale perioadei.",
                rand.DocumentId, eticheta);
        }
        rezumat(FelConstatareInchidere.RestScadent, restante.Count);
    }

    // Acordul numeralului: 1 rând, 2–19 rânduri, 20+ „de rânduri".
    static string Randuri(int n) =>
        n == 1 ? "1 rând" : $"{n}{(n % 100 is >= 1 and <= 19 ? "" : " de")} rânduri";

    static string Cheie(FelConstatareInchidere fel) => fel switch {
        FelConstatareInchidere.ItvLipsa => "ITV-LIPSA",
        FelConstatareInchidere.AmoLipsa => "AMO-LIPSA",
        FelConstatareInchidere.DraftInPerioada => "DRAFT-IN-PERIOADA",
        FelConstatareInchidere.RestScadent => "REST-SCADENT",
        _ => fel.ToString().ToUpperInvariant(),
    };

    static DateOnly UltimaZi(int an, int luna) => new(an, luna, DateTime.DaysInMonth(an, luna));

    /// <summary>Închide luna dacă verificarea o permite; scrie rândul de istoric. Comite.</summary>
    public static InchiderePerioada Inchide(IObjectSpace os, int an, int luna,
            IReadOnlyCollection<string> acceptate, Guid? deId, string de) {
        using var tx = TranzactieComanda.Incepe(os);
        Blocheaza(os, an, luna);
        // Verificarea rulează DIN NOU, în tranzacție: raportul pe care l-a citit
        // operatorul e o fotografie, refuzul e al stării de acum (F27-D2).
        var constatari = Verifica(os, an, luna);
        var acceptateCurate = (acceptate ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).ToHashSet();
        var neacoperite = constatari
            .Where(c => c.Severitate == SeveritateConstatare.Blocant
                || !acceptateCurate.Contains(c.Cheie))
            .ToList();
        // Refuzul poartă lista ÎNTREAGĂ, nu doar rândurile vinovate: ecranul o
        // arată ca să se accepte constatări concrete, nu un flag de forțare.
        if (neacoperite.Count > 0)
            throw new OperareException(string.Join("\n",
                constatari.Select(c => $"{c.Severitate}: {c.Text} [{c.Cheie}]")));

        var perioada = Gaseste(os, an, luna);
        // Referințele de dinaintea lui P, citite ÎNAINTE ca P să devină închisă.
        // Snapshot(P) = snapshot(P−1) + rulaje(P); după el, ce nu e decembrie
        // iese din referințe (F27-D3). Se elimină TOATE, nu doar P−1: cu o lună
        // NEDEFINITĂ între ele (închisă prin absență), ultima referință poate fi
        // mai veche de o lună, iar snapshot-ul ei ar rămâne orfan.
        var vechi = SolduriService.Referinte(os)
            .Where(r => r.An * 12 + r.Luna < an * 12 + luna && r.Luna != 12)
            .ToList();
        SolduriService.Materializeaza(os, an, luna);
        foreach (var (anVechi, lunaVeche) in vechi)
            SolduriService.Elimina(os, anVechi, lunaVeche);

        var acum = DateTime.UtcNow;
        perioada.Inchisa = true;
        perioada.InchisaLa = acum;
        perioada.InchisaPrimaOara ??= acum;
        var rand = Istoric(os, perioada, FelInchiderePerioada.Inchidere, acum, deId, de);
        // Doar cheile care CORESPUND unei constatări de acum: cele rămase dintr-un
        // raport vechi s-au ignorat la verificare, deci n-au ce căuta în istoric.
        var scrise = constatari.Select(c => c.Cheie).Where(acceptateCurate.Contains)
            .Distinct().OrderBy(c => c, StringComparer.Ordinal).ToArray();
        if (scrise.Length > 0)
            rand.Acceptari = JsonSerializer.Serialize(scrise);
        os.CommitChanges();
        tx.Commit();
        return rand;
    }

    /// <summary>Redeschide ULTIMA perioadă închisă, cu motiv. Comite.</summary>
    public static InchiderePerioada Redeschide(IObjectSpace os, int an, int luna,
            string motiv, Guid? deId, string de) {
        using var tx = TranzactieComanda.Incepe(os);
        Blocheaza(os, an, luna);
        var erori = new List<string>();
        var perioada = Gaseste(os, an, luna);
        if (perioada == null)
            erori.Add($"Perioada {Eticheta(an, luna)} nu e definită — nu se poate redeschide.");
        else if (!perioada.Inchisa)
            erori.Add($"Perioada {Eticheta(an, luna)} e deschisă — nu are ce redeschide.");
        if (string.IsNullOrWhiteSpace(motiv))
            erori.Add("Redeschiderea unei perioade cere un motiv scris.");

        var (anUrmator, lunaUrmatoare) = Urmatoarea(an, luna);
        var urmatoarea = Gaseste(os, anUrmator, lunaUrmatoare);
        if (urmatoarea != null && urmatoarea.Inchisa)
            erori.Add($"Perioada {Eticheta(anUrmator, lunaUrmatoare)} e închisă — se redeschide doar ultima perioadă închisă.");
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));

        // Snapshot(P) dispare cu închiderea lui; P−1 redevine ultima închisă,
        // deci referință — reconstruită prin `SUM` integral, fiindcă P−2 nu mai
        // are snapshot din care să pornească (F27-D3).
        SolduriService.Elimina(os, an, luna);
        var (anPrecedent, lunaPrecedenta) = Precedenta(an, luna);
        if (Gaseste(os, anPrecedent, lunaPrecedenta) is { Inchisa: true }
                && !SolduriService.AreSnapshot(os, anPrecedent, lunaPrecedenta))
            SolduriService.Materializeaza(os, anPrecedent, lunaPrecedenta);

        var acum = DateTime.UtcNow;
        perioada.Inchisa = false;
        perioada.InchisaLa = null;
        var rand = Istoric(os, perioada, FelInchiderePerioada.Redeschidere, acum, deId, de);
        rand.Motiv = motiv.Trim();
        os.CommitChanges();
        tx.Commit();
        return rand;
    }

    // Prima instrucțiune a comenzii (F27-D1): `FOR UPDATE` pe rândul perioadei,
    // ca `SUM`-ul să nu ruleze înaintea blocării. `FOR SHARE`-ul gardianului de
    // operare așteaptă aici, iar două operări concurente nu se blochează între
    // ele (spike A.0). `"GCRecord" = 0` explicit — SQL brut, fără filtru global.
    static void Blocheaza(IObjectSpace os, int an, int luna) {
        const string sql = """
            SELECT "ID" AS "Value"
            FROM "PerioadeFiscale"
            WHERE "An" = {0} AND "Luna" = {1} AND "GCRecord" = 0
            FOR UPDATE
            """;
        if (os is not EFCoreObjectSpace efCore)
            throw new InvalidOperationException(
                $"Comanda de perioadă cere un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`.");
        efCore.DbContext.Database
            .SqlQuery<Guid>(FormattableStringFactory.Create(sql, an, luna))
            .ToList();
    }

    static InchiderePerioada Istoric(IObjectSpace os, PerioadaFiscala perioada, FelInchiderePerioada fel,
            DateTime la, Guid? deId, string de) {
        var rand = os.CreateObject<InchiderePerioada>();
        rand.PerioadaId = perioada.ID;
        rand.Fel = fel;
        rand.La = la;
        rand.DeId = deId;
        rand.De = de;
        return rand;
    }

    static PerioadaFiscala Gaseste(IObjectSpace os, int an, int luna) =>
        os.FirstOrDefault<PerioadaFiscala>(p => p.An == an && p.Luna == luna);

    static (int An, int Luna) Precedenta(int an, int luna) =>
        luna == 1 ? (an - 1, 12) : (an, luna - 1);

    static (int An, int Luna) Urmatoarea(int an, int luna) =>
        luna == 12 ? (an + 1, 1) : (an, luna + 1);

    static string Eticheta(int an, int luna) => $"{luna:00}/{an}";
}
