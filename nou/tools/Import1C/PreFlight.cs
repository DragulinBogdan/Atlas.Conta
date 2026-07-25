namespace Import1C;

// Faza PRE-FLIGHT (decizia 48c): înainte de PRIMUL document se mătură tot ce ar
// putea lipsi și se emite un raport UNIC.
//
// Rostul e ordinea descoperirii: fără el, fiecare cod de cont nemapat și fiecare
// tip de document neacoperit ar apărea ca eșec în mijlocul unei rulări de ore,
// unul câte unul, în ordinea aleatoare a datelor. Cu el, triajul e o sesiune de
// decizii pe un tabel (deciziile intră în `mapari-conturi.csv`), iar rularea
// pornește doar când tabelul e curat.
//
// Sugestiile mecanice se AFIȘEAZĂ, nu se aplică (48c): tăierea de cifre e
// demonstrabil nesigură exact pe volum (43111 → 4311 „corect mecanic", 4316
// corect semantic), iar unealta nu are voie să ia decizii de profil singură.
static class PreFlight {

    public record Rezultat(
        int Coduri, int PrinDictionar, int PrinMecanica, int Nerezolvabile, int PeSumator,
        int RanduriReziduale,
        int Tipuri, int TipuriNecunoscute, int TipuriNeimplementate, int DocumenteNeacoperite,
        int Antete, int AntetePostate, int PostateFaraNote, int LiniiFaraCota);

    public static Rezultat Executa(
            FlaxDb flax, int an, int panaLa, MapariConturi mapari,
            IReadOnlyDictionary<string, Guid> plan,
            IReadOnlySet<string> sumatori,
            IReadOnlyDictionary<string, string> denumiriOmfp,
            IReadOnlyDictionary<string, List<string>> copiiPlan,
            IReadOnlyList<FlaxCont> plan1C,
            IReadOnlySet<string> tipuriCunoscute,
            IReadOnlySet<string> tipuriImplementate,
            Action<string> avert, Action<string, bool> check) {

        Console.WriteLine($"\n=== PRE-FLIGHT (mișcările {an}, decizia 48c) ===");
        var denumiri1C = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var c in plan1C)
            if (c.Cod != null)
                denumiri1C.TryAdd(c.Cod, c.Denumire);

        // ---- 0. Igiena dicționarului însuși ----
        // Un rând care trimite pe un cont inexistent sau SUMATOR e o gaură care
        // s-ar manifesta abia la primul document care folosește codul — și atunci
        // ar arăta ca o gaură de date, nu ca o greșeală de dicționar.
        var randuriRele = 0;
        foreach (var r in mapari.Randuri) {
            if (!plan.ContainsKey(r.ContOmfp)) {
                randuriRele++;
                check($"mapari-conturi.csv:{r.Linie} — {r.Cod1C} → {r.ContOmfp}: contul există în "
                    + "planul OMFP", false);
            }
            else if (sumatori.Contains(r.ContOmfp)) {
                randuriRele++;
                check($"mapari-conturi.csv:{r.Linie} — {r.Cod1C} → {r.ContOmfp}: contul NU e sumator "
                    + $"(„{denumiriOmfp.GetValueOrDefault(r.ContOmfp)}”)", false);
            }
        }
        check($"Dicționarul de mapare: {mapari.Randuri.Count} rânduri, toate pe conturi ne-sumatoare "
            + $"din plan ({randuriRele} rânduri rele)", randuriRele == 0);

        // ---- 1. Codurile de cont ale mișcărilor ----
        var coduri = flax.CoduriConturiMiscari(an);
        var reziduale = flax.RanduriPerioadeReziduale();
        var prinDictionar = 0;
        var prinMecanica = 0;
        var probleme = new List<(FlaxCodMiscare Cod, string Simbol, string Motiv, string Sugestie)>();
        foreach (var c in coduri) {
            var simbol = mapari.Mapeaza(c.Cod, plan);
            if (simbol == null)
                probleme.Add((c, null, "NEREZOLVABIL", SugereazaPrinTaiere(c.Cod, plan, sumatori, denumiriOmfp)));
            else if (sumatori.Contains(simbol))
                probleme.Add((c, simbol, "SUMATOR", SugereazaCopii(simbol, copiiPlan, sumatori, denumiriOmfp)));
            else if (mapari.EstePrinDictionar(c.Cod))
                prinDictionar++;
            else
                prinMecanica++;
        }

        Console.WriteLine($"Coduri de cont distincte pe mișcările {an}: {coduri.Count} "
            + $"({prinDictionar} prin dicționar, {prinMecanica} prin mecanica generică, "
            + $"{probleme.Count} probleme).");
        if (reziduale > 0)
            Console.WriteLine($"Rânduri în perioade reziduale (an > 3000, artefacte 1C): {reziduale} "
                + "— excluse din măturare și din import.");

        if (probleme.Count > 0) {
            Console.WriteLine("\n  TRIAJ CONTURI (sugestiile NU se aplică automat — decizia 48c; "
                + "fiecare rând = o decizie de scris în mapari-conturi.csv):");
            Console.WriteLine($"  {"Cod 1C",-12} {"Motiv",-12} {"Rânduri",8} {"Doc",7}  denumire 1C → sugestie");
            foreach (var p in probleme.OrderByDescending(p => p.Cod.Randuri)) {
                Console.WriteLine($"  {p.Cod.Cod,-12} {p.Motiv,-12} {p.Cod.Randuri,8} {p.Cod.Documente,7}  "
                    + $"„{denumiri1C.GetValueOrDefault(p.Cod.Cod) ?? "(fără denumire în planul 1C)"}”"
                    + (p.Simbol != null ? $" → {p.Simbol} „{denumiriOmfp.GetValueOrDefault(p.Simbol)}”" : "")
                    + $"  ⇒ {p.Sugestie}");
                avert($"Cont 1C {p.Cod.Cod} ({p.Motiv}, {p.Cod.Randuri} rânduri / {p.Cod.Documente} "
                    + $"documente în {an}): „{denumiri1C.GetValueOrDefault(p.Cod.Cod)}” — cere un rând "
                    + $"în mapari-conturi.csv. Sugestie mecanică (neaplicată): {p.Sugestie}");
            }
        }
        check($"Pre-flight conturi: toate cele {coduri.Count} coduri ale mișcărilor {an} se rezolvă pe "
            + $"conturi OMFP ne-sumatoare ({probleme.Count(p => p.Motiv == "NEREZOLVABIL")} nerezolvabile, "
            + $"{probleme.Count(p => p.Motiv == "SUMATOR")} pe sumator)", probleme.Count == 0);

        // ---- 2. Tipurile de document-sursă (Recorder) ----
        var tipuri = flax.TipuriRecorder(an);
        var necunoscute = new List<FlaxTipRecorder>();
        var neimplementate = new List<FlaxTipRecorder>();
        Console.WriteLine($"\n  TIPURI RECORDER pe mișcările {an} ({tipuri.Count} distincte):");
        Console.WriteLine($"  {"TypeRef",-10} {"Tip 1C",-40} {"Doc",8} {"Rânduri",9}  stare");
        foreach (var t in tipuri.OrderByDescending(t => t.Randuri)) {
            string stare;
            if (t.Nume == null || !tipuriCunoscute.Contains(t.Nume)) {
                necunoscute.Add(t);
                stare = t.Nume == null
                    ? "NECUNOSCUT (fără coloană tipizată în view)"
                    : "NECUNOSCUT (lipsește din maparea §4)";
            }
            else if (!tipuriImplementate.Contains(t.Nume)) {
                neimplementate.Add(t);
                stare = "cunoscut, FĂRĂ handler (pașii 3–5)";
            }
            else
                stare = "implementat";
            Console.WriteLine($"  {t.TypeRef,-10} {t.Nume ?? "(necunoscut)",-40} {t.Documente,8} "
                + $"{t.Randuri,9}  {stare}");
        }
        foreach (var t in necunoscute)
            avert($"Tip Recorder 1C {t.TypeRef} „{t.Nume ?? "(fără coloană tipizată)"}”: "
                + $"{t.Documente} documente / {t.Randuri} rânduri în {an} — NU e în lista de tipuri "
                + "cunoscute a uneltei; cere o decizie de mapare (design §4).");
        var docNeacoperite = necunoscute.Concat(neimplementate).Sum(t => t.Documente);
        check($"Pre-flight Recorder: toate cele {tipuri.Count} tipuri ale mișcărilor {an} sunt "
            + $"CUNOSCUTE ({necunoscute.Count} necunoscute)", necunoscute.Count == 0);
        check($"Pre-flight Recorder: toate tipurile cunoscute au handler în buclă "
            + $"({neimplementate.Count} fără handler, {docNeacoperite} documente neacoperite)",
            neimplementate.Count == 0);

        // ---- 3. Volumul per view: antetele pe care le vor citi cititorii ----
        // Cititorii documentelor filtrează pe `Posted <> 0x00` (FlaxDocumente.cs).
        // Ipoteza pe care stă filtrul e că un document NEPOSTAT nu are rânduri în
        // registrul contabil — dacă ar avea, importul ar pierde tăcut postări.
        // Se verifică din cele două numărători deja făcute: documentele care
        // POSTEAZĂ (recensământul Recorder) trebuie să încapă în antetele Posted.
        var volume = flax.VolumeAntete(an);
        var postePeTip = tipuri.Where(t => t.Nume != null)
            .ToDictionary(t => t.Nume, t => t.Documente, StringComparer.Ordinal);
        Console.WriteLine($"\n  VOLUM DOCUMENTE {an} (antete în sursă vs. documente care postează):");
        Console.WriteLine($"  {"View 1C",-42} {"Antete",8} {"Posted",8} {"Postează",9} {"Δ",7}  notă");
        var acoperireRea = 0;
        foreach (var v in volume.OrderByDescending(v => v.Postate)) {
            var posteaza = postePeTip.GetValueOrDefault(v.View);
            var delta = v.Postate - posteaza;
            if (delta < 0) {
                acoperireRea++;
                avert($"View {v.View}: {posteaza} documente postează în {an}, dar doar {v.Postate} "
                    + "antete au Posted=1 — filtrul cititorilor ar ascunde postări (FlaxDocumente.cs).");
            }
            Console.WriteLine($"  {v.View,-42} {v.Antete,8} {v.Postate,8} {posteaza,9} {delta,7}  "
                + (delta < 0 ? "ACOPERIRE RUPTĂ" : delta > 0 ? "antete Posted fără rânduri contabile" : ""));
        }
        var postateFaraNote = volume.Sum(v => Math.Max(0, v.Postate - postePeTip.GetValueOrDefault(v.View)));
        check($"Pre-flight volume: fiecare view are documentele care postează în interiorul "
            + $"antetelor Posted ({acoperireRea} view-uri cu acoperire ruptă; {postateFaraNote} "
            + "antete Posted fără rânduri contabile — documente goale sau neutre contabil)",
            acoperireRea == 0);

        // ---- 4. View-uri STALE: cotele de TVA de 21% ----
        // `CotaTVA` e decodată în view printr-un CASE peste GUID-urile cotelor,
        // fixat la generare; elementele de 21% (1 august 2025) nu sunt în el și
        // ies NULL. Ocolirea în conector ar rupe contractul de coloane (§2) —
        // singura reparație e regenerarea view-urilor. Verificarea depinde de
        // `--pana-la`: lunile 1–7 sunt curate, din august pică.
        var faraCota = flax.LiniiFaraCotaTva(an, panaLa);
        var totalFaraCota = faraCota.Sum(x => x.Linii);
        if (totalFaraCota > 0) {
            Console.WriteLine($"\n  LINII FĂRĂ COTĂ DE TVA (lunile 1..{panaLa}):");
            foreach (var x in faraCota.Where(x => x.Linii > 0).OrderByDescending(x => x.Linii))
                Console.WriteLine($"  {x.Sectiune,-52} {x.Linii,8}");
            avert($"View-urile SkyConta sunt generate ÎNAINTEA cotelor de 21%: {totalFaraCota} linii "
                + $"de document din lunile 1..{panaLa} ies cu CotaTVA NULL. Regenerează view-urile "
                + "(design §2, rețeta 8.A din inventar) — conectorul NU are voie să ocolească prin "
                + "dicționar propriu de GUID-uri.");
        }
        check($"Pre-flight cote TVA: toate liniile de document din lunile 1..{panaLa} au cotă "
            + $"decodată în view ({totalFaraCota} linii fără cotă)", totalFaraCota == 0);

        return new Rezultat(coduri.Count, prinDictionar, prinMecanica,
            probleme.Count(p => p.Motiv == "NEREZOLVABIL"), probleme.Count(p => p.Motiv == "SUMATOR"),
            reziduale, tipuri.Count, necunoscute.Count, neimplementate.Count, docNeacoperite,
            volume.Sum(v => v.Antete), volume.Sum(v => v.Postate), postateFaraNote, totalFaraCota);
    }

    // Sugestia pentru un cod nerezolvabil: forma concatenată, cu cifre terminale
    // tăiate până cade pe un cont ne-sumator. E EXACT regula respinsă la 48c —
    // se afișează ca punct de plecare al deciziei, nu ca răspuns.
    static string SugereazaPrinTaiere(string cod, IReadOnlyDictionary<string, Guid> plan,
            IReadOnlySet<string> sumatori, IReadOnlyDictionary<string, string> denumiri) {
        var concat = string.Concat((MapariConturi.Normalizeaza(cod) ?? "")
            .Split('.', StringSplitOptions.RemoveEmptyEntries));
        for (var s = concat; s.Length > 0; s = s[..^1])
            if (plan.ContainsKey(s) && !sumatori.Contains(s))
                return $"tăiere de cifre → {s} „{denumiri.GetValueOrDefault(s)}” (VERIFICĂ semantica)";
        return "niciun candidat prin tăiere — decizie pur semantică";
    }

    // Sugestia pentru o cădere pe sumator: copiii ne-sumatori ai contului. Cu un
    // singur copil alegerea e aproape sigură; cu mai mulți, e o decizie de profil.
    static string SugereazaCopii(string simbol, IReadOnlyDictionary<string, List<string>> copii,
            IReadOnlySet<string> sumatori, IReadOnlyDictionary<string, string> denumiri) {
        var lista = (copii.GetValueOrDefault(simbol) ?? [])
            .Where(c => !sumatori.Contains(c)).OrderBy(c => c, StringComparer.Ordinal).ToList();
        if (lista.Count == 0)
            return "sumator fără copii ne-sumatori — decizie pur semantică";
        if (lista.Count == 1)
            return $"copil unic → {lista[0]} „{denumiri.GetValueOrDefault(lista[0])}”";
        return "alege dintre copii: " + string.Join(", ",
            lista.Take(6).Select(c => $"{c} „{denumiri.GetValueOrDefault(c)}”"))
            + (lista.Count > 6 ? $" … (+{lista.Count - 6})" : "");
    }
}
