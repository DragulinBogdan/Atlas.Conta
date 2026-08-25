using System.Diagnostics;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL FINAL OPȚIONAL al importului (felia 15, D15-D6): registrul ANAF al
// plătitorilor de TVA peste partenerii aduși din 1C.
//
// De ce e AICI și nu în serviciu: `SincronizareAnafService` știe să sincronizeze
// un LOT de ID-uri (calea umană din XAF, comanda REST de ≤ 500). „Toți
// partenerii unei baze de import" e altceva — 20.000 de fișe, ~200 de apeluri la
// 1/s, ~4 minute — și e treaba unui conector de consolă, nu a unui request
// (D15-D4 spune exact asta). Unealta pune peste serviciu doar ce lipsește la
// scara asta: TRANȘAREA, RELUAREA unei tranșe căzute tranzitoriu, PULSUL în
// consolă și AGREGAREA rezultatelor per cauză.
//
// Ce NU face: nu suprascrie nimic (`suprascrie = false`). Pe axa TVA registrul
// ANAF e canonicul (D4-r1) și se scrie oricum; pe restul — denumire, registrul
// comerțului, adresă — o valoare culeasă și diferită se RAPORTEAZĂ. Importul e
// evidență (21/35b), inclusiv când sursa e ANAF: ce a introdus contabilul nu se
// rescrie automat dintr-o rulare de noapte.
static class Anaf1C {
    // Tranșa de parteneri per ObjectSpace. Nu e plafonul ANAF (ăla e 100 de CUI
    // per APEL și îl respectă clientul): e plafonul de MEMORIE — orchestratorul
    // comite per partener, deci un OS deschis peste toți cei 20.000 ar ține
    // entitățile urmărite până la final degeaba.
    const int Transa = 1000;

    // Reluarea unei tranșe cu eroare TRANZITORIE (5xx, timeout, rețea, 429): o
    // singură dată, după o pauză. Contractul (D15-D2) lasă deliberat retry-ul
    // apelantului — clientul nu-l face automat, fiindcă „ce înseamnă să reiei"
    // diferă între un request REST (503, reîncearcă omul) și o rulare de import.
    static readonly TimeSpan PauzaReluare = TimeSpan.FromSeconds(5);

    public static async Task<RaportAnaf> Executa(IObjectSpaceProvider provider,
            IReadOnlyCollection<Guid> ids, string url, Action<string> avert, CancellationToken ct) {
        var raport = new RaportAnaf();
        var cronometru = Stopwatch.StartNew();
        // Timeout generos: un lot de 100 de CUI-uri la ANAF răspunde în secunde,
        // dar serviciul are zile proaste, iar o expirare la 100 s e clasificată
        // TRANZITORIU și reluată — mai bine să aștepte decât să rateze tranșa.
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        var client = new PlatitorTvaClient(http, url);

        var transe = ids.Chunk(Transa).ToList();
        Console.WriteLine($"\n=== SINCRONIZARE ANAF (--anaf): {ids.Count} parteneri legați în "
            + $"{transe.Count} tranșe de {Transa} ===");
        Console.WriteLine($"    URL: {(string.IsNullOrWhiteSpace(url) ? PlatitorTvaClient.UrlImplicit : url)}; "
            + $"≤ {PlatitorTvaClient.MaximPerLot} CUI/apel, 1 apel/s, suprascrie = false.");

        for (var i = 0; i < transe.Count; i++) {
            var transa = transe[i];
            var rezultat = await Ruleaza(transa).ConfigureAwait(false);

            // Reluarea: numai partenerii al căror LOT a picat tranzitoriu. A
            // relua toată tranșa ar re-aplica peste cei deja scriși — idempotent
            // ca valori, dar ar dubla `Modificari` în raport, adică ar minți
            // exact cifra pentru care există raportul.
            var deReluat = rezultat.Sarite
                .Where(s => s.Motiv != null && s.Motiv.StartsWith("lotul ANAF", StringComparison.Ordinal))
                .Select(s => s.Id).ToList();
            if (deReluat.Count > 0 && rezultat.Erori.Any(e => e.Tranzitorie)) {
                Console.WriteLine($"    tranșa {i + 1}: {deReluat.Count} parteneri pe loturi căzute "
                    + $"tranzitoriu — se reia o dată peste {PauzaReluare.TotalSeconds:N0} s.");
                await Task.Delay(PauzaReluare, ct).ConfigureAwait(false);
                var reluat = await Ruleaza(deReluat).ConfigureAwait(false);
                // Săriții „lot eșuat" ai primei încercări dispar din raport: ei
                // au fost re-întrebați, iar soarta lor e cea din reluare.
                rezultat.Sarite.RemoveAll(s => s.Motiv != null
                    && s.Motiv.StartsWith("lotul ANAF", StringComparison.Ordinal));
                raport.Reluate += deReluat.Count;
                Aduna(raport, reluat);
            }
            Aduna(raport, rezultat);

            // Pulsul: ~4 minute de tăcere pe o consolă e indistinct de un proces
            // blocat (rularea e detașată — jurnal 50d).
            Console.WriteLine($"    tranșa {i + 1}/{transe.Count}: {raport.Gasiti} găsiți, "
                + $"{raport.Negasiti} negăsiți, {raport.Sariti.Values.Sum()} săriți, "
                + $"{raport.Modificari.Values.Sum()} modificări, {raport.Diferente.Values.Sum()} diferențe "
                + $"({cronometru.Elapsed:hh\\:mm\\:ss}).");

            // Pauza dintre tranșe: clientul respectă 1 apel/s ÎN INTERIORUL unei
            // interogări, dar tranșa următoare pornește cu un apel imediat, iar
            // commit-urile dintre ele nu garantează o secundă.
            if (i + 1 < transe.Count)
                await Task.Delay(PlatitorTvaClient.PauzaIntreApeluri, ct).ConfigureAwait(false);
        }

        foreach (var e in raport.Erori)
            avert($"ANAF: lot de {e.Lot.Count} CUI-uri — {e.Mesaj} "
                + $"({(e.Tranzitorie ? "tranzitorie" : "fatală")}).");
        // Candidat = partenerul care a trecut de filtrul `Interogabilitate` (RO,
        // CUI de 2–10 cifre, nu CNP) și a ajuns deci într-un lot ANAF. Se
        // DERIVĂ din soarta lui, nu se numără separat: altfel ar exista două
        // cifre pentru același lucru, care s-ar putea despărți la prima
        // reluare.
        raport.Candidati = raport.Gasiti + raport.Negasiti
            + raport.Sariti.GetValueOrDefault("lot ANAF eșuat (partener neatins)")
            + raport.Sariti.GetValueOrDefault("commit eșuat")
            + raport.Sariti.GetValueOrDefault("partener dispărut între selecție și scriere");
        raport.Durata = cronometru.Elapsed;
        return raport;

        async Task<RezultatLot> Ruleaza(IReadOnlyCollection<Guid> lot) {
            // Ușa NON-SECURED (58c): `DataSincronizareAnaf` e server-owned și
            // `GardianEditare` o refuză pe orice cale securizată. Import1C n-are
            // securitate deloc — OS-ul lui e non-secured prin construcție — dar
            // asta rămâne o proprietate a UȘII, nu o scutire a uneltei.
            using var os = provider.CreateObjectSpace();
            return await SincronizareAnafService
                .SincronizeazaAsync(os, client, lot, suprascrie: false, ct).ConfigureAwait(false);
        }
    }

    static void Aduna(RaportAnaf raport, RezultatLot rezultat) {
        foreach (var r in rezultat.Rezultate) {
            if (r.Gasit)
                raport.Gasiti++;
            else
                raport.Negasiti++;
            foreach (var m in r.Modificari)
                Numara(raport.Modificari, m.Camp);
            foreach (var d in r.Diferente)
                Numara(raport.Diferente, d.Camp);
            foreach (var a in r.Avertismente)
                Numara(raport.Avertismente, CauzaAvertisment(a));
        }
        foreach (var s in rezultat.Sarite)
            Numara(raport.Sariti, CauzaSarit(s.Motiv));
        raport.Erori.AddRange(rezultat.Erori);
    }

    static void Numara(Dictionary<string, int> unde, string cheie) =>
        unde[cheie] = unde.GetValueOrDefault(cheie) + 1;

    // Motivele și avertismentele serviciului poartă CUI-uri, denumiri și
    // valori — utile pe un lot de zece, inutilizabile pe douăzeci de mii (fix 7
    // al review-ului D394: „avertismente per rând, nemărginite"). Aici se taie
    // la CAUZĂ, iar cifra de lângă cauză e cea care se citește.
    static string CauzaSarit(string motiv) => motiv switch {
        null => "motiv absent",
        var m when m.StartsWith("țara ", StringComparison.Ordinal) => "țară ≠ RO",
        "fără cod fiscal" => "fără cod fiscal",
        var m when m.StartsWith("codul fiscal „", StringComparison.Ordinal) => "cod fiscal fără cifre",
        var m when m.StartsWith("cod numeric personal", StringComparison.Ordinal)
            => "CNP (persoană fizică fără CUI)",
        var m when m.StartsWith("codul fiscal are ", StringComparison.Ordinal)
            => "CUI în afara intervalului 2–10 cifre",
        var m when m.StartsWith("partenerul nu există", StringComparison.Ordinal)
            => "partener inexistent sau șters",
        var m when m.StartsWith("partenerul a dispărut", StringComparison.Ordinal)
            => "partener dispărut între selecție și scriere",
        var m when m.StartsWith("lotul ANAF", StringComparison.Ordinal)
            => "lot ANAF eșuat (partener neatins)",
        var m when m.StartsWith("salvarea a eșuat", StringComparison.Ordinal) => "commit eșuat",
        _ => "alt motiv",
    };

    static string CauzaAvertisment(string avertisment) => avertisment switch {
        null => "avertisment gol",
        var a when a.StartsWith("CUI-ul ", StringComparison.Ordinal) => "CUI-ul nu figurează la ANAF",
        var a when a.StartsWith("ANAF n-a raportat", StringComparison.Ordinal)
            => $"ANAF n-a raportat statutul {IntreGhilimele(a)}",
        var a when a.StartsWith("ANAF n-a întors nicio adresă", StringComparison.Ordinal)
            => "ANAF fără adresă utilizabilă (fără localitate)",
        var a when a.StartsWith("Județ nerezolvat: ANAF", StringComparison.Ordinal)
            => "indicativ auto ANAF în afara ISO 3166-2:RO",
        var a when a.StartsWith("Județ nerezolvat: nomenclatorul", StringComparison.Ordinal)
            => "cod de județ absent din nomenclator (seed)",
        var a when a.StartsWith("Județul ", StringComparison.Ordinal)
            => "județ nescris (partenerul are altă țară)",
        var a when a.Contains("tăiată la", StringComparison.Ordinal)
            => $"trunchiere la lungimea SAF-T: {a.Split(':')[0]}",
        _ => "alt avertisment",
    };

    static string IntreGhilimele(string text) {
        var i = text.IndexOf('„');
        var j = text.IndexOf('”');
        return i >= 0 && j > i ? text[i..(j + 1)] : "(necunoscut)";
    }

    // ---------------- Proba D394 a rulării: distribuția partenerilor ----------------
    //
    // D394 clasifică rândul după tipul de partener, iar tipul e o funcție de
    // exact trei date (71b): `InregistratTva` (bate tot), `TipPersoana` și
    // `Tara`. O sincronizare ANAF care mișcă `InregistratTva` mișcă implicit
    // cartușele formularului — deci distribuția ASTA, luată înainte și după, e
    // proba că schimbarea e cea așteptată, măsurată pe baza, nu pe HTTP.
    public static Dictionary<string, int> Distributie(IObjectSpaceProvider provider) {
        using var os = provider.CreateObjectSpace();
        var randuri = os.GetObjectsQuery<Partener>()
            .Select(p => new { p.TipPersoana, p.Tara, p.InregistratTva })
            .ToList();
        var rezultat = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var r in randuri) {
            // Eticheta e chiar regula „înregistrat bate tot" din 71b, scrisă ca
            // text: tipul D394 pe care l-ar primi partenerul azi.
            var tip = r.InregistratTva ? 1
                : r.TipPersoana == TipPersoana.Fizica && r.Tara == "RO" ? 2
                : r.Tara == "RO" ? 4
                : TariUe.Contine(r.Tara) ? 3 : 4;
            Numara(rezultat, $"tip {tip} · {r.TipPersoana} · {(r.Tara == "RO" ? "RO" : "străin")} · "
                + $"{(r.InregistratTva ? "înregistrat" : "neînregistrat")}");
        }
        return rezultat;
    }
}

// Raportul agregat al fazei `--anaf`, ca DATE (numărat per cauză, nu per rând).
sealed class RaportAnaf {
    public int Candidati { get; set; }
    public int Gasiti { get; set; }
    public int Negasiti { get; set; }
    public int Reluate { get; set; }
    public TimeSpan Durata { get; set; }
    public Dictionary<string, int> Sariti { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Modificari { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Diferente { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> Avertismente { get; } = new(StringComparer.Ordinal);
    public List<EroareLotAnaf> Erori { get; } = [];
}
