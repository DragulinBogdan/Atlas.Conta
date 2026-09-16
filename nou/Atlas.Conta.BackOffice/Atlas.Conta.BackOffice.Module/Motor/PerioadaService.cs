using System.Runtime.CompilerServices;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Comenzile perioadei (F27-D1/D2); constatările de conținut și `acceptate` vin la pasul 7.
public static class PerioadaService {
    // `Cheie` = constatarea concretă (acceptarea se dă pe ea), `Fel` = familia (F27-D2).
    public sealed record ConstatareInchidere(string Cheie, string Fel, SeveritateConstatare Severitate,
        string Text, Guid? ObiectId, string ObiectEticheta);

    const string FelNedefinita = "PERIOADA-NEDEFINITA";
    const string FelInchisa = "PERIOADA-INCHISA";
    const string FelPrecedentaDeschisa = "PRECEDENTA-DESCHISA";

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
        return constatari;
    }

    /// <summary>Închide luna dacă verificarea o permite; scrie rândul de istoric. Comite.</summary>
    public static InchiderePerioada Inchide(IObjectSpace os, int an, int luna,
            IReadOnlyCollection<string> acceptate, Guid? deId, string de) {
        using var tx = TranzactieComanda.Incepe(os);
        Blocheaza(os, an, luna);
        var blocante = Verifica(os, an, luna)
            .Where(c => c.Severitate == SeveritateConstatare.Blocant)
            .ToList();
        if (blocante.Count > 0)
            throw new OperareException(string.Join("\n", blocante.Select(c => c.Text)));

        var perioada = Gaseste(os, an, luna);
        // Snapshot(P) = snapshot(P−1) + rulaje(P); P−1 iese din referințe dacă
        // nu e capăt de an (F27-D3).
        SolduriService.Materializeaza(os, an, luna);
        var (anPrecedent, lunaPrecedenta) = Precedenta(an, luna);
        if (lunaPrecedenta != 12 && Gaseste(os, anPrecedent, lunaPrecedenta) is { Inchisa: true })
            SolduriService.Elimina(os, anPrecedent, lunaPrecedenta);

        var acum = DateTime.UtcNow;
        perioada.Inchisa = true;
        perioada.InchisaLa = acum;
        perioada.InchisaPrimaOara ??= acum;
        var rand = Istoric(os, perioada, FelInchiderePerioada.Inchidere, acum, deId, de);
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
