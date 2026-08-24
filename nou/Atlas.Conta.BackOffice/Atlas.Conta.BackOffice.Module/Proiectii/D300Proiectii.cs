using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// DECONTUL DE TVA — formularul 300 (OPANAF 174/2026) ca PROIECȚIE peste
// `RegistruTva` (felia 12, D3-D3). Închide lanțul TVA structural (36) → registru
// (68) → declarație: aceleași cifre, așezate pe rândurile formularului.
//
// ═══ Forma (b): listă în MEMORIE, fără `DataSourceLoader` ═══
// Precedentul e `ContabilProiectii.BalantaPlan`, cu același motiv scris altfel:
// **un formular nu se paginează**. Rd. 19 fără rd. 9 nu e „pagina 1 dintr-un
// decont", e un decont fals. Mărginirea vine din DATE (55 de poziții fixate de
// lege), nu din `LIMIT/OFFSET`, iar totalurile se calculează peste TOATE
// rândurile — o agregare paginată n-ar avea ce să adune.
//
// ═══ Trei mecanisme, o singură trecere ═══
// `RandD300.Fel` e singura axă după care un rând știe de unde-i vine cifra:
//   Operatiuni → din mapările `(TipTva × Sens)` ale profilului (politică = date);
//   Total      → formula legii, în COD (structura nu e configurabilă);
//   Oglinda    → copie din rândul-sursă (zona deductibilă a taxării inverse);
//   Extern     → parametru al cererii sau 0 (fără sursă în model).
// Ordinea celor cinci pași de mai jos NU e stilistică: părinții „din care" se
// adună înaintea oglinzilor (rd. 26 e copia lui rd. 12 DUPĂ ce 12 și-a strâns
// copiii), iar totalurile vin ultimele, peste tot ce s-a așezat.
//
// ═══ Ce NU face ═══
// Nu produce fișierul XML și nu persistă „declarația" ca entitate (35c: D300 e
// checklist de completitudine, nu model de date). Nu rotunjește nimic: cifrele
// vin deja rotunjite la bani din registru, iar totalurile sunt sume exacte de
// bani. Nu filtrează `Storno` — registrul e append-only și suma lui algebrică e
// adevărul (R-D7); un rând de operațiuni poate ieși NET NEGATIV, și nu se
// trunchiază (doar rd. 36/37/44/45 au `max(…, 0)`, fiindcă asta scrie legea).

// Cifrele pe care formularul le cere, dar modelul nu le are (D3-D3 pasul 6):
// soldurile decontului precedent și diferențele stabilite de organul fiscal.
// Toate `decimal`, toate implicit 0 — un decont fără istoric e un decont valid.
public sealed record ParametriD300(
    decimal SoldPlataPrecedent = 0m,
    decimal DiferentePlata = 0m,
    decimal SoldNegativPrecedent = 0m,
    decimal DiferenteNegative = 0m);

// Un rând al formularului, PLAT prin construcție (deciziile 6/7).
public sealed class D300Rand {
    // Numărul din formular, ca text („9", „12.1"). Sortarea e pe `Ordine`.
    public string Cod { get; set; }
    public string Denumire { get; set; }
    // Enum-urile pleacă STRING (57a): contractul de sârmă nu depinde de ordinea
    // membrilor, iar clientul citește exact ce-i trimite `metadata.json`.
    public string Sectiune { get; set; }
    public string Fel { get; set; }
    // 0 = rând al formularului; 1 = sub-rând „din care". Ecranul îl indentează.
    public int Nivel { get; set; }
    public int Ordine { get; set; }
    // NULL = rândul n-are coloana în formular (rd. 13/14/15/29 n-au TVA;
    // rd. 27/28/31/32/34 și 36-45 n-au bază). **Niciodată 0 în locul lui null**:
    // un ecran care afișează „0,00" într-o casetă inexistentă minte (D3-D1).
    // Zero e o cifră adevărată — „coloana există și e goală".
    public decimal? Baza { get; set; }
    public decimal? Tva { get; set; }
    // Câte rânduri de REGISTRU stau în spatele cifrei — urma către granularitatea
    // SAF-T. 0 pe totaluri, oglinzi și externi: acolo nu intră rânduri NOI de
    // registru (cele ale sursei sunt deja numărate pe sursă).
    public int Randuri { get; set; }
    // Codurile `TipTva` care au alimentat rândul, distincte și ordonate („N21,
    // NED21") — transparența cerută de D3-D4: cifra spune și DE UNDE vine.
    // Oglinda o MOȘTENEȘTE de la sursă (spre deosebire de `Randuri`): e o
    // etichetă de identitate, nu un contor aditiv.
    public string Surse { get; set; }
}

// O operațiune taxabilă care NU are unde să cadă în formular (D3-D4). Nu e un
// log: e parte din contract, cu cifrele ei, fiindcă un gard care tace devine
// capcană (62f). Cauze legitime: un `TipTva` propriu al clientului, încă
// nemapat; sau o gaură deliberată a profilului (achiziția cu cota tranzitorie de
// 9% n-are rând în forma 2026).
public sealed class D300Nemapat {
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    // LEFT join: un `TipTva` șters logic nu face grupul să dispară — rămâne cu
    // eticheta goală. Lecția review-ului D4 al feliei 9, aplicată la literă: un
    // rând nu are voie să iasă dintr-un raport fiscal fiindcă i-a murit eticheta.
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    // SNAPSHOT-uri de pe rândul de registru (JT-D3), nu din nomenclatorul de azi.
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public int Randuri { get; set; }
}

public sealed class D300Dto {
    public List<D300Rand> Randuri { get; set; } = [];
    public List<D300Nemapat> Nemapate { get; set; } = [];
    public List<string> Avertismente { get; set; } = [];
}

public static class D300Proiectii {
    // Prima perioadă fiscală pentru care formularul din nomenclator e cel în
    // vigoare (OPANAF 174/2026, M.Of. 105/09.02.2026). Sub ea, proiecția rămâne
    // corectă ca ARITMETICĂ, dar formularul e altul decât cel depus atunci —
    // restanța D3-r1 (versionarea pe an fiscal), semnalată, nu ascunsă.
    static readonly DateOnly PrimaPerioada2026 = new(2026, 1, 1);

    // Cultura mesajelor: RO fixată, nu cea a serverului. Un avertisment care
    // scrie „210.00" pe o mașină și „210,00" pe alta e același defect ca o cifră
    // care depinde de locale — iar ăsta ajunge sub ochii contabilului.
    static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");

    // Operanzii totalurilor, EXACT cum îi numește ordinul (§2 din
    // `d300-structura-2026.md`) — liste explicite, nu intervale deduse din
    // `Ordine`. Legea enumeră rândurile; o formulă „toate rândurile secțiunii de
    // dinaintea totalului" ar fi părut mai deșteaptă și ar fi înghițit tăcut
    // rd. 29 (informativ, NU intră în rd. 30) la prima renumerotare.
    //
    // Sub-rândurile „din care" (3.1, 5.1, 7.1, 12.1, 12.2, 20.1, 22.1, 26.1,
    // 26.2) LIPSESC din amândouă, deliberat: sunt deja în părinții lor. Ăsta e
    // gardul contra dublei numărări (riscul 1 din design).
    static readonly string[] OperanziRd19 =
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18"];
    static readonly string[] OperanziRd30 =
        ["20", "21", "22", "23", "24", "25", "26", "27", "28"];

    // Rândurile pe care formulele le ating; absența oricăruia oprește calculul cu
    // un avertisment, nu cu o cifră inventată.
    static readonly string[] CoduriFormule =
        ["19", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45"];

    // Acumulatorul unui rând: `D300Rand` de sârmă poartă `decimal?`, iar
    // aritmetica pe nullable ar fi presărat `?? 0m` peste tot calculul. Aici
    // cifrele sunt `decimal` simple, plus memoria despre ce coloane EXISTĂ —
    // conversia în null se face O SINGURĂ dată, la ieșire.
    sealed class Nod {
        public Guid Id;
        public string Cod, Denumire;
        public SectiuneD300 Sectiune;
        public FelRandD300 Fel;
        public int Ordine, Nivel, Randuri;
        public bool AreBaza, AreTva;
        public Guid? ParinteId, OglindaAId;
        public decimal Baza, Tva;
        public readonly SortedSet<string> Surse = new(StringComparer.Ordinal);
    }

    /// <summary>
    /// Decontul de TVA pe o perioadă: cele 55 de poziții ale formularului, plus
    /// operațiunile care n-au unde să cadă și avertismentele proiecției.
    /// Ambele capete ale perioadei sunt INCLUSIVE.
    /// </summary>
    public static D300Dto D300(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd,
        ParametriD300 externi) {
        externi ??= new ParametriD300();
        var rezultat = new D300Dto();

        // ── 1. Agregatul de registru (D3-D3 pasul 1) ────────────────────────
        //
        // Filtru pe `Data` a RÂNDULUI (data stornării pentru rândurile inverse,
        // 25d) — coerent cu jurnalele: decontul lunii deja depuse rămâne cum a
        // fost depus, iar stornarea se declară în luna în care s-a făcut.
        //
        // `Storno` NU intră în filtru și nici în cheie: spre deosebire de jurnal
        // (unde separarea ține granularitatea per document cerută de D394), aici
        // adevărul e chiar suma ALGEBRICĂ — un storno de N19 se scade din rd. 16
        // prin `TipTvaId`-ul lui snapshot, fără niciun mecanism nou.
        //
        // `Regim` e în cheie fiindcă rd. 31 îl consumă (nedeductibilul se scade
        // pe regimul de pe RÂND, nu pe cel al nomenclatorului de azi — riscul 2).
        // `Cota` intră lângă el pentru același motiv pentru care e în cheia
        // `DecontTva`: e SNAPSHOT, iar peste o perioadă care se întinde pe mai
        // multe luni o cotă editată între timp ar fi trebuit altfel „aleasă" de
        // un `MIN`. Cheia mai fină nu schimbă NICIO cifră de rând (sumele sunt
        // aceleași oricât de fin grupezi), dar face `D300Nemapat.Cota` să fie
        // cifra care a intrat în calcul, nu cea de azi.
        var agregate = os.GetObjectsQuery<RegistruTva>()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd)
            .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota })
            .Select(g => new {
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Regim,
                g.Key.Cota,
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            })
            .ToList();

        // Nomenclatorul și politica, în două citiri PLATE (fără navigații lazy —
        // 25b: apelantul poate fi un ObjectSpace fără lazy loading). De aici
        // încolo totul e în memorie: 55 de rânduri și câteva zeci de mapări.
        var noduri = os.GetObjectsQuery<RandD300>()
            .Select(r => new {
                r.ID, r.Cod, r.Denumire, r.Sectiune, r.Ordine,
                r.AreBaza, r.AreTva, r.Fel, r.ParinteId, r.OglindaAId
            })
            .ToList()
            .Select(r => new Nod {
                Id = r.ID, Cod = r.Cod, Denumire = r.Denumire, Sectiune = r.Sectiune,
                Ordine = r.Ordine, AreBaza = r.AreBaza, AreTva = r.AreTva, Fel = r.Fel,
                ParinteId = r.ParinteId, OglindaAId = r.OglindaAId
            })
            .ToList();
        var dupaId = noduri.ToDictionary(n => n.Id);
        // `Cod` e unic prin schemă, dar `null` ar arunca la `ToDictionary` — pe o
        // bază seed-uită parțial preferăm un decont cu avertisment unui 500.
        var dupaCod = noduri.Where(n => n.Cod != null)
            .GroupBy(n => n.Cod).ToDictionary(g => g.Key, g => g.First());

        // Adâncimea pe lanțul de părinți VIZIBILI, cu gardă de ciclu — aceeași
        // precauție ca `BalantaPlan`: `Parinte` e o navigație, iar un ciclu
        // introdus din greșeală ar transforma raportul într-o buclă infinită.
        foreach (var nod in noduri) {
            var vizitate = new HashSet<Guid> { nod.Id };
            var parinte = nod.ParinteId;
            while (parinte is Guid pid && dupaId.TryGetValue(pid, out var sus) && vizitate.Add(pid)) {
                nod.Nivel++;
                parinte = sus.ParinteId;
            }
        }

        // Etichetele tipurilor de TVA: join la CITIRE (JT-D3), și LEFT — un tip
        // șters logic lasă eticheta goală, nu scoate cifra din decont.
        var idsTip = agregate.Select(a => a.TipTvaId).Distinct().ToList();
        var etichete = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Cod, t.Denumire })
            .ToList()
            .ToDictionary(t => t.ID, t => (t.Cod, t.Denumire));

        var mapari = os.GetObjectsQuery<MapareD300>()
            .Select(m => new { m.TipTvaId, m.Sens, m.RandId })
            .ToList();

        // ── 2. Așezarea grupurilor pe rânduri (D3-D3 pasul 2) ───────────────
        //
        // O pereche `(TipTva, Sens)` poate cădea pe MAI MULTE rânduri — e chiar
        // motivul pentru care maparea e un nomenclator și nu o coloană (TI19 pe
        // achiziție e și rd. 16, și rd. 33). Grupul se adună pe fiecare, iar
        // suma peste rânduri depășește deliberat suma registrului: rd. 16 și
        // rd. 33 sunt laturi diferite ale aceleiași operațiuni, nu o dublare.
        var pierdutTva = new Dictionary<Guid, decimal>();
        var pierdutBaza = new Dictionary<Guid, decimal>();
        // Nedeductibilul care se scade la rd. 31: TVA-ul grupurilor cu
        // `Regim = Capitalizat` care CHIAR au ajuns în secțiunea deductibilă.
        // O SINGURĂ dată per grup, oricâte rânduri deductibile ar atinge —
        // altfel scăderea ar depăși ce s-a adunat la rd. 30.
        var nedeductibil = 0m;

        foreach (var a in agregate) {
            var eticheta = etichete.TryGetValue(a.TipTvaId, out var e) ? e : (Cod: null, Denumire: null);
            var tinte = mapari
                .Where(m => m.TipTvaId == a.TipTvaId && m.Sens == a.Sens)
                .Select(m => m.RandId).Distinct()
                .Select(id => dupaId.GetValueOrDefault(id))
                .Where(n => n != null)
                .ToList();

            if (tinte.Count == 0) {
                // D3-D4: nu se pierde nimic — se RAPORTEAZĂ, cu cifrele lui.
                rezultat.Nemapate.Add(new D300Nemapat {
                    // În memorie, deci `ToString()` e sigur și dă exact numele
                    // membrului — spre deosebire de proiecțiile `IQueryable` din
                    // `TvaProiectii`, unde lanțul de `?:` există fiindcă trebuie
                    // să se traducă în `CASE`.
                    Sens = a.Sens.ToString(),
                    TipTvaId = a.TipTvaId,
                    TipTvaCod = eticheta.Cod,
                    TipTvaDenumire = eticheta.Denumire,
                    Regim = a.Regim.ToString(),
                    Cota = a.Cota,
                    Baza = a.Baza,
                    Tva = a.Tva,
                    Randuri = a.Randuri
                });
                continue;
            }

            if (a.Regim == RegimTva.Capitalizat
                    && tinte.Any(n => n.Sectiune == SectiuneD300.Deductibila))
                nedeductibil += a.Tva;

            foreach (var tinta in tinte) {
                // Coloana care nu există nu primește cifra — dar nici nu o
                // înghite: se ține deoparte și iese ca avertisment (riscul 4).
                if (tinta.AreBaza)
                    tinta.Baza += a.Baza;
                else if (a.Baza != 0m)
                    pierdutBaza[tinta.Id] = pierdutBaza.GetValueOrDefault(tinta.Id) + a.Baza;
                if (tinta.AreTva)
                    tinta.Tva += a.Tva;
                else if (a.Tva != 0m)
                    pierdutTva[tinta.Id] = pierdutTva.GetValueOrDefault(tinta.Id) + a.Tva;
                tinta.Randuri += a.Randuri;
                if (eticheta.Cod != null)
                    tinta.Surse.Add(eticheta.Cod);
            }
        }

        // ── 3. Părinții „din care" (D3-D3 pasul 3) ──────────────────────────
        //
        // Rândul-mamă = mapările lui DIRECTE + Σ copiii (formula legii e „rd. 12
        // ≥ rd. 12.1 + rd. 12.2": egalitate la noi, inegalitate dacă cineva mapează
        // ceva direct pe 12). De la adâncime spre rădăcină, ca un lanț de trei
        // niveluri să funcționeze fără să depindă de ordinea din nomenclator.
        foreach (var nod in noduri.OrderByDescending(n => n.Nivel)) {
            if (nod.ParinteId is not Guid pid || !dupaId.TryGetValue(pid, out var parinte))
                continue;
            parinte.Baza += nod.Baza;
            parinte.Tva += nod.Tva;
            parinte.Randuri += nod.Randuri;
            foreach (var sursa in nod.Surse)
                parinte.Surse.Add(sursa);
        }

        // ── 4. Oglinzile (D3-D3 pasul 4) ────────────────────────────────────
        //
        // Zona deductibilă a taxării inverse e COPIA exactă a zonei colectate
        // (rd. 20 = rd. 5, rd. 26 = rd. 12 …) — formularul cere egalitatea ca
        // validare blocantă, iar taxarea inversă se colectează și se deduce în
        // aceeași perioadă. Copia se face DUPĂ pasul 3, ca rd. 26 să primească
        // rd. 12 deja complet cu copiii lui.
        //
        // `Randuri` se ZEROIZEAZĂ (inclusiv ce a urcat de la copiii-oglindă la
        // pasul 3): oglinda nu aduce rânduri NOI de registru, ele sunt deja
        // numărate pe sursă. `Surse` se moștenește — e identitate, nu contor.
        foreach (var nod in noduri.Where(n => n.OglindaAId != null)) {
            nod.Randuri = 0;
            nod.Surse.Clear();
            if (!dupaId.TryGetValue(nod.OglindaAId.Value, out var sursa))
                continue;
            nod.Baza = sursa.AreBaza && nod.AreBaza ? sursa.Baza : 0m;
            nod.Tva = sursa.AreTva && nod.AreTva ? sursa.Tva : 0m;
            foreach (var cod in sursa.Surse)
                nod.Surse.Add(cod);
        }

        // ── 5+6. Externii și totalurile (D3-D3 pașii 5 și 6) ────────────────
        //
        // Externii ÎNAINTEA totalurilor: rd. 38/39 intră în rd. 40, iar rd. 41/42
        // în rd. 43. Ordinea formulelor de mai jos e cea a formularului și e
        // load-bearing: fiecare consumă rezultatul celei dinainte.
        var lipsa = CoduriFormule.Where(c => !dupaCod.ContainsKey(c)).ToList();
        if (lipsa.Count > 0) {
            // Formulele nu se aplică pe operanzi inventați: rândurile care CHIAR
            // s-au citit rămân corecte, totalurile lipsesc, motivul se scrie.
            //
            // Mesajul nu ACUZĂ seed-ul, fiindcă proiecția nu poate ști care din
            // două cauze e: o bază neseed-uită la zi SAU un utilizator care n-are
            // drept de citire pe nomenclator (ObjectSpace-ul e SECURIZAT, deci
            // rândurile invizibile pur și simplu nu vin — măsurat pe calea reală,
            // HTTP cu userul fără permisiuni: `Randuri` iese gol, exact ca la
            // celelalte proiecții de registru). Un avertisment care numește
            // greșit cauza e mai rău decât unul care spune doar faptul.
            rezultat.Avertismente.Add(
                $"Rândurile {string.Join(", ", lipsa)} nu s-au putut citi din nomenclatorul D300, deci "
                + "totalurile formularului nu s-au calculat — fie baza nu e seed-uită la zi, fie "
                + "utilizatorul curent n-are drept de citire pe nomenclator.");
        }
        else {
            decimal T(string cod) => dupaCod[cod].Tva;

            // Totalul unei secțiuni: aceiași operanzi pe ambele coloane, dar
            // numai cei care CHIAR au coloana. Așa iese mecanic formula
            // oficială a coloanei TVA de la rd. 19 (rd. 1-4, 13, 14, 15 n-au
            // TVA, deci nu contribuie) — fără o a doua listă de întreținut.
            void Aduna(string codTotal, string[] operanzi) {
                var total = dupaCod[codTotal];
                foreach (var cod in operanzi) {
                    if (!dupaCod.TryGetValue(cod, out var operand))
                        continue;
                    if (total.AreBaza && operand.AreBaza)
                        total.Baza += operand.Baza;
                    if (total.AreTva && operand.AreTva)
                        total.Tva += operand.Tva;
                }
            }

            dupaCod["38"].Tva = externi.SoldPlataPrecedent;
            dupaCod["39"].Tva = externi.DiferentePlata;
            dupaCod["41"].Tva = externi.SoldNegativPrecedent;
            dupaCod["42"].Tva = externi.DiferenteNegative;
            // Rd. 27/28 (compensația forfetară a agricultorilor), 32 (restituiri
            // către cumpărători străini) și 34 (pro-rata / ajustări) rămân 0:
            // n-au sursă în model (36f), dar EXISTĂ în listă — un formular din
            // care lipsesc rânduri nu mai e formularul.

            Aduna("19", OperanziRd19);
            Aduna("30", OperanziRd30);
            // Rd. 31 — singurul loc din proiecție unde o cifră se SCADE, și
            // miezul lui §4.1 din structură: rd. 30 e taxa DEDUCTIBILĂ, rd. 31 e
            // taxa DEDUSĂ. TVA-ul fără drept de deducere (regimul `Capitalizat`,
            // capitalizat în costul bunului) a intrat în rd. 24, deci în rd. 30,
            // dar „nu se preia" în rd. 31. Scăderea se face pe `Regim`-ul
            // SNAPSHOT al grupului — nu pe `TipTva.Regim` de azi, care e
            // nomenclator editabil (riscul 2 din design).
            dupaCod["31"].Tva = T("30") - nedeductibil;
            dupaCod["35"].Tva = T("31") + T("32") + T("33") + T("34");
            // Perechi mutual exclusive prin `max(…, 0)` — exact ce scrie legea.
            // Singurele trunchieri din tot calculul: un rând de operațiuni cu
            // storno poate ieși net negativ și rămâne negativ (riscul 3).
            dupaCod["36"].Tva = Math.Max(T("35") - T("19"), 0m);
            dupaCod["37"].Tva = Math.Max(T("19") - T("35"), 0m);
            dupaCod["40"].Tva = T("37") + T("38") + T("39");
            dupaCod["43"].Tva = T("36") + T("41") + T("42");
            dupaCod["44"].Tva = Math.Max(T("40") - T("43"), 0m);
            dupaCod["45"].Tva = Math.Max(T("43") - T("40"), 0m);
        }

        // ── Avertismentele ──────────────────────────────────────────────────
        //
        // Versiunea formularului (restanța D3-r1): o perioadă din 2025 proiectată
        // pe forma 2026 pune cota de 19% pe rd. 16/33 — corect pentru formularul
        // de azi, greșit față de decontul care s-a depus atunci.
        if (dataStart < PrimaPerioada2026)
            rezultat.Avertismente.Add(
                $"Perioada începe la {dataStart:dd.MM.yyyy}, înaintea anului 2026, dar formularul e cel în "
                + "vigoare (OPANAF 174/2026): cotele istorice apar pe rd. 16 și rd. 33, nu pe rândurile lor "
                + "de atunci. Cifrele sunt corecte; așezarea e a formularului de azi.");

        // Pierderea pe coloană absentă (riscul 4): cineva a mapat un tip cu TVA
        // pe un rând care n-are coloană de TVA (rd. 13/14/15/29). Cifra NU se
        // strecoară nicăieri — și tocmai de aceea trebuie strigată: altfel
        // decontul ar fi mai mic decât registrul, fără nicio urmă.
        foreach (var (id, suma) in pierdutTva.OrderBy(p => dupaId[p.Key].Ordine))
            rezultat.Avertismente.Add(
                $"rd. {dupaId[id].Cod} a primit TVA {suma.ToString("N2", Ro)} pe care nu-l poate purta — "
                + "rândul n-are coloană de TVA în formular. Verificați maparea tipurilor de TVA.");
        foreach (var (id, suma) in pierdutBaza.OrderBy(p => dupaId[p.Key].Ordine))
            rezultat.Avertismente.Add(
                $"rd. {dupaId[id].Cod} a primit bază impozabilă {suma.ToString("N2", Ro)} pe care nu o poate "
                + "purta — rândul n-are coloană de valoare în formular. Verificați maparea tipurilor de TVA.");

        // ── Ieșirea: ordinea FORMULARULUI, lista întreagă ───────────────────
        // `Ordine`, nu `Cod`: codul e text, iar alfabetic „10" ar veni înaintea
        // lui „9" și „12.1" nu s-ar așeza nicăieri.
        rezultat.Randuri = noduri
            .OrderBy(n => n.Ordine)
            .Select(n => new D300Rand {
                Cod = n.Cod,
                Denumire = n.Denumire,
                Sectiune = n.Sectiune.ToString(),
                Fel = n.Fel.ToString(),
                Nivel = n.Nivel,
                Ordine = n.Ordine,
                Baza = n.AreBaza ? n.Baza : null,
                Tva = n.AreTva ? n.Tva : null,
                Randuri = n.Fel == FelRandD300.Operatiuni ? n.Randuri : 0,
                Surse = n.Surse.Count == 0 ? null : string.Join(", ", n.Surse)
            })
            .ToList();
        return rezultat;
    }
}
