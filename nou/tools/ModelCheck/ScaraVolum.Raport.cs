using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class ScaraVolum {
    static string Nr(decimal v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    static string Ms(double v) => v.ToString(v >= 100 ? "0" : "0.0", CultureInfo.InvariantCulture);
    static string Sec(double ms) => (ms / 1000).ToString("0.0", CultureInfo.InvariantCulture) + " s";
    static string Mib(double octeti) => (octeti / 1048576.0).ToString("0.0", CultureInfo.InvariantCulture);

    static void Scrie(string director, Rulare r) {
        var sb = new StringBuilder("# Scara de volum — privat (TR-D9a pas 5b, D9-A1)\n\n");
        sb.AppendLine($"- Baza `{r.Baza}`, pornită la {r.Inceput:yyyy-MM-dd HH:mm} UTC. Stare: {r.Oprire ?? "încheiată"}.");
        sb.AppendLine($"- Scena: `PerfCub` privat, m = {Istoric} luni închise × {UnitatiIstoric} unități, luna măsurată cu k = {K} unități, păstrată "
            + $"(fără purjă, cu utilizatorul `{PerfCub.Utilizator}`): {r.PostariScena} postări, {r.OriginiScena} partide cu origine, construită în {Sec(r.MsScena)}.");
        sb.AppendLine($"- Factorul F = cel mai mic întreg cu F × {r.PostariScena} ≥ ținta de postări: **{r.FactorF}**. Trepte: {string.Join(", ", r.Factori)}.");
        sb.AppendLine("- Fiecare treaptă: copiile noi (direct în SQL, `ScaraVolum.sql`), `VACUUM (ANALYZE)` pe tabelele atinse, probele de corectitudine, "
            + "prima reconstrucție a snapshot-urilor prin `PerioadeApply.Reconstruieste` (cronometrată separat, în proces propriu), `ANALYZE` pe tabelele de "
            + "snapshot, apoi fiecare operație în procesul-copil `--perf-cub-masura`: rece, cald, cu planul ales și cu `enable_seqscan = off`.");
        sb.AppendLine("- E raport, fără prag. O execuție de peste 10 minute se oprește și se consemnează.\n");

        foreach (var t in r.Trepte) {
            sb.AppendLine($"## Treapta f = {t.Factor}\n");
            sb.AppendLine("### Forma bazei\n\n| mărime | valoare |\n|---|---|");
            foreach (var (nume, valoare) in t.Forma.Where(x => !x.Key.StartsWith("octeți", StringComparison.Ordinal)))
                sb.AppendLine($"| {nume} | {Nr(valoare)} |");
            foreach (var (cod, nume) in new[] { ("BAL-NV", "chei de balanță analitică (rânduri livrate de `BAL-NV`)"),
                         ("PREST-NV", "partide cu rest la sfârșitul lunii măsurate (rânduri livrate de `PREST-NV`)"),
                         ("STOC-NV", "poziții de stoc (rânduri livrate de `STOC-NV`)") })
                if (t.Masurari.FirstOrDefault(m => m.Operatie == cod && m.Masura.Faza == "cald" && m.Masura.Eroare == null) is { } m)
                    sb.AppendLine($"| {nume} | {m.Masura.Cardinal} |");
            foreach (var (nume, valoare) in t.Forma.Where(x => x.Key.StartsWith("octeți", StringComparison.Ordinal)))
                sb.AppendLine($"| MiB {nume["octeți ".Length..]} | {Mib((double)valoare)} |");

            sb.AppendLine("\n### Durate\n\n| pas | durată |\n|---|---|");
            sb.AppendLine($"| multiplicarea până la f = {t.Factor} | {Sec(t.MsMultiplicare)} |");
            sb.AppendLine($"| `VACUUM (ANALYZE)` pe {string.Join(", ", TabeleAtinse.Select(x => $"`{x}`"))} | {Sec(t.MsVacuum)} |");
            sb.AppendLine($"| prima reconstrucție a snapshot-urilor | {t.StareReconstructie ?? "nepornită"}"
                + (t.Reconstructie is { } rc ? $": {rc.Comenzi} comenzi SQL, {rc.Referinte} referințe, {rc.Diferite} rânduri diferite față de snapshot-ul găsit; "
                    + $"recalculate {rc.Contabil} contabil / {rc.Stoc} stoc / {rc.Partide} partide" : "") + " |");
            sb.AppendLine($"| `ANALYZE` pe tabelele de snapshot | {Sec(t.MsAnalyzeSnapshot)} |");

            sb.AppendLine("\n### Cititorii\n");
            sb.AppendLine("| operație | rută | fază | ms | comenzi SQL | ms SQL | rânduri citite | rânduri livrate | alocați MiB | vârf gestionat MiB | server max ms | "
                + "plan ales: scanări Postare | rânduri atinse | buffers | rânduri snapshot | fără secvențial: scanări (secvențiale) | rânduri atinse | buffers | control | stare |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach (var x in t.Masurari) {
                var m = x.Masura;
                var plan = m.PlanuriCerute > 0;
                var masurat = m.Eroare == null || m.Ms > 0;
                sb.AppendLine($"| {x.Operatie} | {x.Ruta} | {m.Faza} | " + (masurat
                        ? $"{Ms(m.Ms)} | {m.Comenzi} | {Ms(m.MsSql)} | {m.Randuri} | {m.Cardinal} | {Mib(m.Alocati)} | {Mib(m.VarfGestionat)} | " : "| | | | | | | ")
                    + (plan ? $"{Ms(m.MsServerMax)} | {m.ScanariPostare} | {m.RanduriPostare:0} | {m.BuffersPostare} | {m.RanduriSnapshot:0} | "
                        + $"{m.ScanariIndex} ({m.SecventialeIndex}) | {m.RanduriIndex:0} | {m.BuffersIndex} | " : "| | | | | | | | ")
                    + $"{(x.ControlOk switch { true => "ok", false => "DIFERIT", _ => "—" })} | {m.Eroare ?? ""} |");
            }
            foreach (var n in t.Nemasurate) sb.AppendLine($"\n- {n}");

            sb.AppendLine("\n### Probele de corectitudine\n\n| probă | cheie | scena | așteptat la f | în bază | |\n|---|---|---|---|---|---|");
            foreach (var p in t.Probe)
                sb.AppendLine($"| {p.Nume} | {p.Cheie} | {Nr(p.Scena)} | {Nr(p.Asteptat)} | {Nr(p.Baza)} | {(p.Ok ? "ok" : "**PICATĂ**")} |");
            sb.AppendLine("\nCifrele de control, pe operație (așteptarea e `PerfCub.Asteptat` la k = 64, după documentul lung, cu factorul cheii):\n");
            sb.AppendLine("| operație | așteptat | citit (cald) | rece / cald |\n|---|---|---|---|");
            foreach (var g in t.Masurari.GroupBy(x => x.Operatie)) {
                var cald = g.FirstOrDefault(x => x.Masura.Faza == "cald") ?? g.First();
                string Cifre(Dictionary<string, decimal> d) => string.Join("; ", d.Select(c => $"{c.Key} {Nr(c.Value)}"));
                sb.AppendLine($"| {g.Key} | {Cifre(cald.Asteptat)} | {(cald.Masura.Control.Count == 0 ? "—" : Cifre(cald.Masura.Control))} | "
                    + string.Join(" / ", g.Select(x => x.ControlOk switch { true => "ok", false => "DIFERIT", _ => "nemăsurat" })) + " |");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## D9-D10 (b): `PREST` pe trepte (numai cifrele)\n");
        sb.AppendLine("| f | operație | fază | ms | rânduri livrate | plan ales: rânduri atinse pe `Postare` | fără secvențial: rânduri atinse pe `Postare` | server max ms | stare |");
        sb.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (var t in r.Trepte)
        foreach (var x in t.Masurari.Where(x => x.Operatie.StartsWith("PREST", StringComparison.Ordinal))) {
            var m = x.Masura;
            sb.AppendLine($"| {t.Factor} | {x.Operatie} | {m.Faza} | {(m.Eroare == null || m.Ms > 0 ? Ms(m.Ms) : "")} | {m.Cardinal} | "
                + (m.PlanuriCerute > 0 ? $"{m.RanduriPostare:0} | {m.RanduriIndex:0} | {Ms(m.MsServerMax)}" : " | | ") + $" | {m.Eroare ?? ""} |");
        }

        sb.AppendLine("\n## Cheia de control → factor\n\n| operație | chei | factor |\n|---|---|---|");
        foreach (var (cod, creste) in PerfCub.CresteCuFactorul)
            sb.AppendLine($"| {cod} | {(cod switch { "BAL" or "FISA" or "SPART" or "PREST" => "toate, pe partenerul scenei (furnizorul și clientul unităților)",
                "JRN" => "`echilibru`, `furnizor.credit`, `client.debit`: pe cont, fără partener",
                "STOC" => "`stoc.cantitate`, `stoc.valoare`, `consum.cantitate`, `consum.valoare`: pe gestiune și pe locul de consum, comune copiilor",
                "RECON" => "`referinte`, `diferite`", _ => "`refuzuri`" })} | {(creste ? "× f" : "neschimbat")} |");
        sb.AppendLine("\nPe `STOC`, așteptarea scade din stoc și adaugă la consum câte o bucată de 10 lei pentru fiecare dintre cele "
            + $"{K} de loturi ale lunii măsurate: documentul lung al scenei se operează după ultima treaptă `PerfCub`, înaintea scării.");

        sb.AppendLine("\n## Configurația\n\n| ce | valoare |\n|---|---|");
        foreach (var (nume, valoare) in r.Configuratie) sb.AppendLine($"| `{nume}` | {valoare} |");
        sb.AppendLine($"| mediul (din scriptul de rulare) | {r.Mediu ?? "nedeclarat"} |");
        sb.AppendLine($"| conexiunea uneltei | `MODELCHECK_CONEXIUNE_EXTRA` = `{Env("MODELCHECK_CONEXIUNE_EXTRA") ?? ""}` |");

        sb.AppendLine("\n## Ce nu măsoară scara\n");
        sb.AppendLine("- **Forma sintetică.** Copiile repetă scena la aceleași date calendaristice: fiecare lot al unei copii rămâne cu stoc, fiecare "
            + "partener clonat are aceeași activitate, iar conturile, produsele, gestiunile, locul de consum și perioadele sunt comune tuturor copiilor. "
            + "Numărul de produse nu crește; numărul de loturi, de parteneri și de partide crește liniar.");
        sb.AppendLine("- **O singură tranzacție de deschidere.** Indexul unic `IX_Tranzactie_Fel` (`Fel = 4`) admite o singură `Deschidere` pe bază: "
            + "postările ei clonate rămân pe tranzacția originală, deci numărul tranzacțiilor de deschidere nu se înmulțește.");
        sb.AppendLine("- **Utilizatorul administrativ.** Ruta securizată e `SecuredEFCoreObjectSpaceProvider` cu un utilizator administrativ: "
            + "nu măsoară filtrele unui rol cu permisiuni pe criterii.");
        sb.AppendLine("- **Configurația implicită.** Postgres rulează pe setările de mai sus, nereglate; nu s-au adăugat indecși.");
        sb.AppendLine($"- **Cititorii excluși.** {string.Join(", ", Excluse.Where(x => x != "INCH").Select(x => $"`{x}`"))} citesc linii de document, "
            + "care nu se clonează; sunt cititori pe interval, măsurați de `PerfCub`.");
        sb.AppendLine("- **Scrierea.** `INCH-N` și comenzile de document nu se măsoară: baza multiplicată e bază de citire (fără `DocumentDetalii`, "
            + "registre, legături de împerechere și fișe ale copiilor). `FIFO-N` și `PDISP-N` validează drafturile scenei originale.");
        sb.AppendLine("- **Limita de timp a comenzii.** Unealta rulează cu `Command Timeout=0`; fără el, Npgsql oprește o comandă după 30 de secunde.");
        File.WriteAllText(Path.Combine(director, "scara-volum-privat.md"), sb.ToString());
        File.WriteAllText(Path.Combine(director, "scara-volum-privat.json"), JsonSerializer.Serialize(r));
    }
}
