using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
// `IgnoreQueryFilters` — singurul loc din seed care întreabă tabela ÎNTREAGĂ,
// peste filtrul global de ștergere amânată pus de XAF (`GCRecord = 0`).
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Pachetul de profil PRIVAT (P1, deciziile 29/35): plan OMFP 1802 (sintetice
// grad 1–3), Clasă/Tip minimal pe simboluri OMFP, TVA structural (TipTva cu
// conturile 4426/4427 ca date + PoliticaTva per tip de document) și politicile
// de contare cu derivările PROPRII profilului (371→607, 345→711, 381→608;
// plus de inventar 3xx = 7588, nu 791). Diferă de bugetar prin CONȚINUT, nu
// prin mecanisme (decizia 29c) — mecanismele stau în ContaSeeder.
internal static class ProfilPrivat {
    // Mapările 3xx→6xx/7xx care nu urmează schimbarea primei cifre (29c):
    // mărfuri 371→607, produse finite 345→711 (descărcarea inversează venitul
    // aferent costurilor), ambalaje 381→608.
    static readonly Dictionary<string, string> Derivari6xxExceptii = new() {
        ["371"] = "607",
        ["345"] = "711",
        ["381"] = "608",
    };

    internal static void Seed(IObjectSpace os) {
        SeedClasaTip(os);
        SeedPlanConturi(os);
        ContaSeeder.SeedRepartitoriMinimali(os);
        SeedPartenerRetail(os);
        // Derivările interoghează BAZA — nomenclatoarele se comit întâi (30e).
        os.CommitChanges();
        // Rolul de terț al conturilor (felia 16, D16-D3) — tot o DERIVARE peste
        // planul comis, deci după commit, lângă `SeedContImplicitTipMaterial`.
        SeedRolTert(os);
        ContaSeeder.SeedContImplicitTipMaterial(os);
        SeedContImplicitPunteStoc(os);
        SeedTipTva(os);
        SeedPoliticiNotaTransfer(os);
        SeedPoliticiFacturaIntrareNir(os);
        SeedPoliticiBonConsum(os);
        SeedPoliticiListaDiferente(os);
        SeedPoliticiFacturaIesire(os);
        SeedPoliticiTrezorerie(os);
        SeedPoliticiDecont(os);
        SeedPoliticiNotaContabila(os);
        SeedPoliticiInchidereTva(os);
        SeedPoliticiAsamblare(os);
        // După FCL: derivarea de vânzare presupune genericul FCL deja creat
        // (altfel guard-ul „fără regulă FCL" din SeedPoliticiFacturaIesire ar
        // vedea rândurile de vânzare și n-ar mai crea genericul de servicii).
        SeedPoliticiDescarcare(os);
        // Retururile derivă 6xx = 3xx pe RDC (independent de FCL/DSC — cheia e
        // TipDocument), deci pot sta oriunde după nomenclatoare.
        SeedPoliticiRetururi(os);
        // Cum se citește în SAF-T S ce a scris politica de stoc de mai sus
        // (felia 17, D17-D1): cheia e `TipDocument`, deci trebuie să vină după
        // tipurile de document (comise de nucleu), nu după regulile de stoc.
        SeedPoliticiMiscareSaft(os);
        os.CommitChanges();
        // PoliticaTva referă TipTva comise mai sus.
        SeedPoliticiTva(os);
        // Default TipTva de CULEGERE: N21 referit — după commit-ul TipTva.
        SeedTipTvaImplicit(os);
        // Așezarea pe decontul de TVA (D3-D2): referă tipurile comise mai sus și
        // rândurile D300 comise de nucleu înaintea pachetului de profil.
        SeedMapareD300(os);
        // Așezarea pe D394 (D4-D2): aceeași dependență de tipurile comise.
        SeedMapareD394(os);
        os.CommitChanges();
    }

    // Clasă/Tip minimal privat: Cod tip = simbol OMFP (același mecanism de
    // derivare a contului implicit ca la bugetar — 10 §2 / decizia 26b).
    static void SeedClasaTip(IObjectSpace os) {
        (string Cod, string Denumire, NaturaClasa Natura)[] clase = [
            ("MP", "Materii prime", NaturaClasa.Stoc),
            ("M", "Materiale consumabile", NaturaClasa.Stoc),
            ("OI", "Obiecte de inventar", NaturaClasa.Stoc),
            ("PF", "Produse finite", NaturaClasa.Stoc),
            ("MF", "Mărfuri", NaturaClasa.Stoc),
            ("AMB", "Ambalaje", NaturaClasa.Stoc),
            ("S", "Servicii și utilități", NaturaClasa.Serviciu),
            ("C", "Alte cheltuieli", NaturaClasa.Cheltuiala),
            ("F", "Imobilizări", NaturaClasa.Imobilizare),
            ("VEN", "Venituri", NaturaClasa.Serviciu),
            // G3 (decizia 50f): clasa terților și a regularizărilor — conturile de
            // decontare pe care cad liniile de import (avansuri, facturi nesosite,
            // fonduri speciale, clarificări). Natura=Serviciu DELIBERAT, nu
            // Tehnica: regulile de contare se potrivesc pe natură (FCT are un rând
            // per Serviciu/Cheltuiala/Imobilizare), iar Tehnica le-ar scoate din
            // joc — e paritate cu clasificarea ad-hoc pe care o promovează.
            ("TER", "Terți și regularizări", NaturaClasa.Serviciu),
            ("T", "TVA", NaturaClasa.Tehnica),
            ("TRZ", "Trezorerie", NaturaClasa.Tehnica),
        ];
        var claseMap = new Dictionary<string, ClasaProdus>();
        foreach (var c in clase) {
            var clasa = os.FirstOrDefault<ClasaProdus>(x => x.Cod == c.Cod);
            if (clasa == null) {
                clasa = os.CreateObject<ClasaProdus>();
                clasa.Cod = c.Cod;
                clasa.Denumire = c.Denumire;
                clasa.Natura = c.Natura;
            }
            claseMap[c.Cod] = clasa;
        }

        (string Clasa, string Cod, string Denumire)[] tipuri = [
            ("MP", "301", "Materii prime"),
            ("M", "302", "Materiale consumabile"),
            // Gradul II al lui 302: 1C ține stocul pe `302.1`/`302.8`, iar Cod-ul
            // Tipului E simbolul de cont (decizia 26b) — fără ele, pozițiile de
            // deschidere de pe aceste conturi n-ar putea deveni loturi. Derivările
            // (ContImplicit din simbol, 6xx=3xx → 6021/6028) le prind automat.
            ("M", "3021", "Materiale auxiliare"),
            ("M", "3024", "Piese de schimb"),
            ("M", "3028", "Alte materiale consumabile"),
            ("OI", "303", "Materiale de natura obiectelor de inventar"),
            ("PF", "345", "Produse finite"),
            ("MF", "371", "Mărfuri"),
            ("AMB", "381", "Ambalaje"),
            ("S", "605", "Energie și apă"),
            ("S", "611", "Întreținere și reparații"),
            ("S", "612", "Redevențe, locații de gestiune și chirii"),
            ("S", "613", "Prime de asigurare"),
            ("S", "614", "Studii și cercetări"),
            ("S", "622", "Comisioane și onorarii"),
            ("S", "623", "Protocol, reclamă și publicitate"),
            ("S", "625", "Deplasări, detașări și transferări"),
            ("S", "626", "Poștale și telecomunicații"),
            ("S", "628", "Alte servicii executate de terți"),
            ("C", "635", "Alte impozite, taxe și vărsăminte asimilate"),
            ("F", "208", "Alte imobilizări necorporale"),
            ("F", "214", "Mobilier, aparatură birotică, alte active corporale"),
            // Nivelul de contare al facturării (30b): contul de venit = alegerea
            // Tipului; simboluri OMFP (704/706/707/708 — nu 751/750 ca la bugetar).
            ("VEN", "704", "Venituri din servicii prestate"),
            ("VEN", "706", "Venituri din redevențe, locații de gestiune și chirii"),
            ("VEN", "707", "Venituri din vânzarea mărfurilor"),
            ("VEN", "708", "Venituri din activități diverse"),
            // Tipul convențional al liniilor de trezorerie culese manual (31c):
            // codul nu e simbol de cont — rămâne fără ContImplicit.
            ("TRZ", "TRZ", "Operațiune de trezorerie"),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipMaterial>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipMaterial>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.Clasa = claseMap[t.Clasa];
            }
        }

        // G3 (decizia 50f): Tipurile pe care importul 1C le crea AD-HOC („gaură
        // de profil completată ad-hoc", Denumire „1C: cont X", clasa ghicită din
        // prima cifră) devin seed EXPLICIT — profilul privat își declară singur
        // conturile pe care cad liniile reale, iar importul nu mai inventează
        // nomenclator (decizia 21: politicile se definesc curat, 1C e evidență).
        // Denumirile sunt cele din planul OMFP; clasa e alegerea profilului, nu a
        // primei cifre. UPSERT pe Cod, nu „creează dacă lipsește": bazele deja
        // importate le au cu denumirea și clasa ad-hoc, iar seed-ul le corectează
        // în loc să le lase divergente. Paritate de comportament verificată:
        // Cheltuiala și Serviciu se contează identic pe FCT (rând per natură, cu
        // același fallback 401), iar DEC/FCL au reguli generice fără filtru.
        (string Clasa, string Cod, string Denumire)[] tipuriPromovate = [
            // Terți și regularizări (decontări, nu consum): natura Serviciu.
            ("TER", "408", "Furnizori - facturi nesosite"),
            ("TER", "4091", "Furnizori-debitori pentru cumpărări de bunuri de natura stocurilor"),
            ("TER", "4092", "Furnizori-debitori pentru prestări de servicii"),
            ("TER", "419", "Clienţi – creditori"),
            ("TER", "447", "Fonduri speciale - taxe şi vărsăminte asimilate"),
            ("TER", "473", "Decontări din operaţii în curs de clarificare"),
            ("TER", "5328", "Alte valori"),
            // Puntea de stoc a importului: simbolul 371 e deja luat de Tipul de
            // STOC, iar o linie de punte pe contul de marfă nu are lot — geamănul
            // „S<simbol>" o ține în afara filtrului de natură al conexului NIR.
            // Cod-ul nu e simbol de cont valid, deci derivarea contului implicit
            // nu-l poate rezolva: se leagă explicit (vezi SeedContImplicitPunteStoc).
            ("TER", "S371", "Punte de stoc 371 (tehnic import)"),
            // Cheltuieli propriu-zise.
            ("C", "6021", "Cheltuieli cu materialele auxiliare"),
            ("C", "6022", "Cheltuieli privind combustibilii"),
            ("C", "6028", "Cheltuieli privind alte materiale consumabile"),
            ("C", "604", "Cheltuieli privind materialele nestocate"),
            ("C", "6422", "Cheltuieli cu tichetele acordate salariaților"),
            ("C", "6458", "Alte cheltuieli privind asigurările și protecția socială"),
            ("C", "6581", "Despăgubiri, amenzi şi penalităţi"),
            ("C", "6651", "Diferenţe nefavorabile de curs valutar legate de elementele monetare exprimate în valută"),
            ("C", "667", "Cheltuieli privind sconturile acordate"),
            // Servicii și utilități (gradul II al lui 605/623 + transport/bancă).
            ("S", "6051", "Cheltuieli privind consumul de energie"),
            ("S", "6052", "Cheltuieli privind consumul de apă"),
            ("S", "6053", "Cheltuieli privind consumul de gaze naturale"),
            ("S", "6231", "Cheltuieli de protocol"),
            ("S", "6232", "Cheltuieli de reclamă şi publicitate"),
            ("S", "624", "Cheltuieli cu transportul de bunuri şi personal"),
            ("S", "627", "Cheltuieli cu serviciile bancare şi asimilate"),
            // Venituri (7588 e și creditul plusului de inventar — aici e Tipul
            // liniei, nu contul regulii).
            ("VEN", "767", "Venituri din sconturi obţinute"),
            ("VEN", "7581", "Venituri din despăgubiri, amenzi şi penalităţi"),
            ("VEN", "7588", "Alte venituri din exploatare"),
        ];
        foreach (var t in tipuriPromovate) {
            var tip = os.FirstOrDefault<TipMaterial>(x => x.Cod == t.Cod) ?? os.CreateObject<TipMaterial>();
            tip.Cod = t.Cod;
            tip.Denumire = t.Denumire;
            tip.Clasa = claseMap[t.Clasa];
        }
    }

    // Puntea de stoc (G3): Cod-ul „S371" nu e simbol de cont, deci derivarea
    // generică (`SeedContImplicitTipMaterial`) îl lasă gol — se leagă explicit de
    // contul 371, exact ce făcea importul când îl crea ad-hoc (Catalog citește
    // simbolul Tipului din ContImplicit). Rulează după commit-ul planului.
    static void SeedContImplicitPunteStoc(IObjectSpace os) {
        var punte = os.FirstOrDefault<TipMaterial>(t => t.Cod == "S371");
        if (punte != null && punte.ContImplicitId == null)
            punte.ContImplicitId = os.FirstOrDefault<Cont>(c => c.Simbol == "371")?.ID;
    }

    // Partenerul generic de retail (decizia 48b): vânzarea cu amănuntul nu are
    // client identificat, dar factura are nevoie de o latură — surogatul RVA
    // (raportul zilnic de vânzări → FCL) o primește pe acesta. Fără ContImplicit
    // propriu: fallback-ul 4111 al regulii de facturare e exact contul corect.
    static void SeedPartenerRetail(IObjectSpace os) {
        if (os.FirstOrDefault<Partener>(p => p.Cod == "CF") != null)
            return;
        var consumatorFinal = os.CreateObject<Partener>();
        consumatorFinal.Cod = "CF";
        consumatorFinal.Denumire = "CONSUMATOR FINAL";
    }

    // Planul OMFP 1802 (sursa: nomenclatorul ANAF PlanConturiBalSocCom, format
    // Account,ParentAccount,Functie,Denumire — numele poate conține virgule, deci
    // split cu limită). OMFP nu poartă defalcare: DimensiuniObligatorii
    // pornesc goale (design §5 — editabile ca date când apare nevoia);
    // Sumator = are copii.
    //
    // `Functie` (felia 16): funcția contabilă e a LEGII (anexa OMFP 1802), nu a
    // clientului — de aceea stă în resursă, nu în UI. Notația din anexă e
    // A(ctiv) / P(asiv) / A/P (bifuncțional), notația MODELULUI e D/C/B
    // (FCTCONT legacy, aceeași cu resursa profilului bugetar): resursa poartă
    // deja D/C/B, ca modelul să aibă O SINGURĂ convenție și `SaftReguli.TipCont`
    // să rămână neatins (D ⇒ Activ, C ⇒ Pasiv, B ⇒ Bifunctional — `AccountType`
    // din MF.GLA.7). Conturile bifuncționale sunt cele pe care anexa le dă „A/P"
    // (117, 121, 4428, 5121, diferențele de preț, decontările din grup…).
    //
    // AUTORITAR pe funcție, ca `SeedRolTert`: câmpul se REscrie la fiecare
    // trecere (o corectare a resursei trebuie să ajungă pe bazele existente, nu
    // doar pe cele noi), restul câmpurilor se scriu doar la CREARE — denumirile
    // și legăturile de părinte sunt deja pe bază, iar planul e nomenclator viu.
    // Cititorul resursei: aceeași sursă pentru `SeedPlanConturi` (care creează și
    // rescrie `Functie`) și pentru `SeedRolTert` (care rescrie `RolTert`) — un al
    // doilea loc care ar ști „ce e în plan" ar putea rămâne în urmă tăcut.
    static StreamReader CititorPlanConturi() {
        var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Atlas.Conta.BackOffice.Module.DatabaseUpdate.SeedData.plan-conturi-omfp.csv")
            ?? throw new InvalidOperationException("Resursa plan-conturi-omfp.csv lipsește.");
        return new StreamReader(stream);
    }

    /// <summary>Simbolurile care sunt ÎN planul seed-uit (prima coloană a CSV-ului).</summary>
    static HashSet<string> SimboluriPlan() {
        using var reader = CititorPlanConturi();
        var simboluri = new HashSet<string>(StringComparer.Ordinal);
        reader.ReadLine(); // header
        string linie;
        while ((linie = reader.ReadLine()) != null) {
            if (string.IsNullOrWhiteSpace(linie))
                continue;
            simboluri.Add(linie.Split(',', 4)[0]);
        }
        return simboluri;
    }

    static void SeedPlanConturi(IObjectSpace os) {
        using var reader = CititorPlanConturi();

        var conturi = os.GetObjectsQuery<Cont>().ToList()
            .GroupBy(c => c.Simbol ?? "", StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        reader.ReadLine(); // header
        string line;
        while ((line = reader.ReadLine()) != null) {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var f = line.Split(',', 4);
            if (!conturi.TryGetValue(f[0], out var cont)) {
                cont = os.CreateObject<Cont>();
                cont.Simbol = f[0];
                cont.Denumire = f[3];
                // CSV-ul e ordonat pe nivel (părinții înaintea copiilor).
                if (f[1].Length > 0 && conturi.TryGetValue(f[1], out var parinte)) {
                    cont.Parinte = parinte;
                    parinte.Sumator = true;
                }
                conturi[f[0]] = cont;
            }
            cont.Functie = f[2];
        }
    }

    // ROLUL DE TERȚ al conturilor (felia 16, D16-D3): pe ce conturi din OMFP
    // 1802 „stă" un client și pe ce conturi un furnizor. Sursa listelor
    // `Customers`/`Suppliers` din SAF-T și a perechii `CustomerID`/`SupplierID`
    // de pe rândul de registru — rolul e al CONTULUI, nu al laturii (partenerul
    // de pe DEBITUL unui 401, la plata datoriei, iese tot `SupplierID`).
    //
    // Potrivirea e pe PREFIX de simbol, ca să prindă și analiticele planului
    // (411 → 411, 4111, 4118; 409 → 409, 4091…4094) fără să le enumere: OMFP
    // adaugă grade la fiecare revizuire, iar o listă exhaustivă ar rămâne în urmă
    // tăcut. Grupele (40, 41, 46) NU intră: sunt sumatoare, iar soldurile de terți
    // se citesc pe conturile care chiar poartă partenerul.
    //
    // Idempotent și AUTORITAR — dar DOAR pe conturile PLANULUI (fixul F8 al
    // review-ului). Rolul se REscrie la fiecare trecere, inclusiv la `Niciunul`
    // pe ce nu mai e în listele de mai jos: e o derivare de seed, ca 6xx=3xx, iar
    // o corectare a listei trebuie să ajungă pe bazele existente, nu doar pe cele
    // noi. EF nu emite UPDATE pentru o valoare identică.
    //
    // Autoritatea se OPREȘTE însă la granița resursei: `Cont` e nomenclator VIU
    // (planul se completează pe bază — analitice proprii, conturi de decontare
    // ale clientului), iar un cont adăugat de operator, cu `RolTert` pus cu mâna,
    // nu e „ceva ce seed-ul nu mai vrea", ci ceva ce seed-ul n-a văzut niciodată.
    // Trecerea anterioară îl ștergea la fiecare `--updateDatabase`, tăcut, iar
    // efectul se vedea abia în D406, ca partener dispărut din `Customers`.
    // Aceeași regulă ca la `Functie` din `SeedPlanConturi`: se rescrie ce e în
    // CSV, restul rămâne al bazei.
    internal static void SeedRolTert(IObjectSpace os) {
        // CLIENȚI — grupa 41 „Clienți și conturi asimilate", fără grupa însăși:
        //   411  Clienți (+ 4111 Clienți, 4118 Clienți incerți sau în litigiu)
        //   413  Efecte de primit de la clienți
        //   418  Clienți — facturi de întocmit
        //   419  Clienți — creditori (avansurile ÎNCASATE: tot ale clientului)
        string[] client = ["411", "413", "418", "419"];
        // FURNIZORI — grupa 40 „Furnizori și conturi asimilate", fără grupa însăși:
        //   401  Furnizori
        //   403  Efecte de plătit
        //   404  Furnizori de imobilizări
        //   405  Efecte de plătit pentru imobilizări
        //   408  Furnizori — facturi nesosite
        //   409  Furnizori — debitori (+ 4091…4094: avansurile ACORDATE)
        string[] furnizor = ["401", "403", "404", "405", "408", "409"];
        // DELIBERAT în afara listelor: 461 „Debitori diverși" și 462 „Creditori
        // diverși". Sunt conturi de DECONTĂRI cu oricine (angajați pe decont,
        // asociați, despăgubiri), nu de terți comerciali — un rol pe ele ar
        // împinge în `Customers`/`Suppliers` repartitori care nu sunt parteneri.
        // Contractul cere explicit ca angajatul de pe 461 să iasă în `Neincluse`
        // cu cauză; cu rol pe 461 n-ar mai fi ajuns acolo, ci în listă, greșit.
        // Restul grupei 46 (463, 466, 467) — la fel.

        var plan = SimboluriPlan();
        foreach (var cont in os.GetObjectsQuery<Cont>().ToList()) {
            var simbol = cont.Simbol ?? "";
            // Contul din afara CSV-ului e al BAZEI, nu al seed-ului: nu se atinge.
            if (!plan.Contains(simbol))
                continue;
            cont.RolTert =
                client.Any(p => simbol.StartsWith(p, StringComparison.Ordinal)) ? RolTertCont.Client
                : furnizor.Any(p => simbol.StartsWith(p, StringComparison.Ordinal)) ? RolTertCont.Furnizor
                : RolTertCont.Niciunul;
        }
    }

    // Nomenclatorul TipTva (design §2): cotele Legii 141/2025 (21 standard,
    // 11 redusă, 9 tranzitoriu locuințe până la 31.07.2026) + regimurile.
    // Conturile de TVA sunt DATE per profil; codurile SAF-T (D406) vin din
    // nomenclatorul ANAF (RO_SAFT_SchemaDefCod 16.02.2026), direcționale:
    // livrare (seria 310xxx) / achiziție deductibilă integral (301xxx) /
    // nedeductibilă (351xxx). Tipul de operațiune D394 e direcțional, deci e
    // politică (`MapareD394`, felia 14), nu atribut al tipului.
    static void SeedTipTva(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>()
            .Where(c => c.Simbol == "4426" || c.Simbol == "4427" || c.Simbol == "4428")
            .ToDictionary(c => c.Simbol, c => c);
        var tva4426 = conturi["4426"];
        var tva4427 = conturi["4427"];
        var tva4428 = conturi["4428"];

        (string Cod, string Denumire, decimal Cota, RegimTva Regim,
            bool Conturi, string SafTLivrare, string SafTAchizitie)[] tipuri = [
            ("N21", "TVA 21% (standard)", 21m, RegimTva.Normal, true, "310344", "301104"),
            ("N11", "TVA 11% (redusă)", 11m, RegimTva.Normal, true, "310351", "301105"),
            ("N9", "TVA 9% (tranzitoriu locuințe, până la 31.07.2026)", 9m, RegimTva.Normal, true, "310310", "301102"),
            ("TI21", "Taxare inversă 21%", 21m, RegimTva.TaxareInversa, true, "310312", "300906"),
            // Cotele ISTORICE (standard 19% până la 31.07.2025) — necesare
            // importului 1C, care aduce un an fiscal complet dinaintea Legii
            // 141/2025 (FAZA 1C §1). Codurile SAF-T rămân NULL: nomenclatorul
            // ANAF e cel în vigoare, iar D406-ul pe perioade vechi nu e o
            // proiecție a acestui sistem; se completează dacă apare vreodată.
            ("N19", "TVA 19% (standard, istoric — până la 31.07.2025)", 19m, RegimTva.Normal, true, null, null),
            ("TI19", "Taxare inversă 19% (istoric — până la 31.07.2025)", 19m, RegimTva.TaxareInversa, true, null, null),
            ("NED21", "Achiziție fără drept de deducere 21% (TVA capitalizat)", 21m, RegimTva.Capitalizat, false, null, "351104"),
            ("SDD", "Scutit cu drept de deducere", 0m, RegimTva.Scutit, false, "310314", null),
            ("SFD", "Scutit fără drept de deducere", 0m, RegimTva.Scutit, false, "310326", null),
            ("NIM", "Neimpozabil (în afara sferei TVA)", 0m, RegimTva.Neimpozabil, false, "310324", null),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipTva>(x => x.Cod == t.Cod) != null)
                continue;
            var tip = os.CreateObject<TipTva>();
            tip.Cod = t.Cod;
            tip.Denumire = t.Denumire;
            tip.Cota = t.Cota;
            tip.Regim = t.Regim;
            tip.CodSafTLivrare = t.SafTLivrare;
            tip.CodSafTAchizitie = t.SafTAchizitie;
            if (t.Conturi) {
                tip.ContTvaDeductibil = tva4426;
                tip.ContTvaColectat = tva4427;
                // REZERVAT (design §8): TVA la încasare / facturi nesosite.
                tip.ContTvaNeexigibil = tva4428;
            }
        }
    }

    // Postarea TVA per tip (design §4): FCT/DEC deduc contra pasivului laturii
    // predator (furnizor 401 / titular 542), FCL colectează contra creanței
    // laturii primitor (client 4111). Tipurile fără rând (NIR, BTR, BCS, LDI,
    // PLT, INC) nu postează TVA.
    static void SeedPoliticiTva(IObjectSpace os) {
        void Politica(string codTip, DirectieTva directie, SursaCont sursa, string fallback) {
            if (os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == codTip) != null)
                return;
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == codTip);
            p.Directie = directie;
            p.SursaContrapartida = sursa;
            p.ContrapartidaFallback = os.FirstOrDefault<Cont>(c => c.Simbol == fallback);
        }
        Politica("FCT", DirectieTva.Deductibil, SursaCont.RepartitorPredator, "401");
        Politica("DEC", DirectieTva.Deductibil, SursaCont.RepartitorPredator, "542");
        Politica("FCL", DirectieTva.Colectat, SursaCont.RepartitorPrimitor, "4111");
        // Retururile (FAZA 1C §7): aceeași direcție ca documentul stornat, dar
        // contrapartida stă pe latura INVERSĂ (RLF: furnizorul e primitor;
        // RDC: clientul e predator). ValoareTva negativă ⇒ 4426 = 401 cu −TVA,
        // respectiv 4111 = 4427 cu −TVA — exact rândurile 1C.
        Politica("RLF", DirectieTva.Deductibil, SursaCont.RepartitorPrimitor, "401");
        Politica("RDC", DirectieTva.Colectat, SursaCont.RepartitorPredator, "4111");
    }

    // Profilul de validare privat: la P2 nu mai are NICIUN rând — clasificația
    // bugetară nu se aplică (33c), iar interdicția FCL⊘Stoc a fost preluată de
    // descărcarea de gestiune (37e); curățarea rândului P1 stă în
    // SeedPoliticiDescarcare, ca pas explicit de updater.

    // Datoria P1 (design §8): default TipTva per tip de document, aplicat la
    // CULEGERE (nu în motor — un rând PoliticaTva doar-pentru-default ar activa
    // pasul TVA). FCT/FCL/DEC + retururile RLF/RDC → N21; setat DOAR unde null (editările manuale nu
    // se ating). Rulează după commit-ul TipTva (N21 referit prin FK).
    static void SeedTipTvaImplicit(IObjectSpace os) {
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        if (n21 == null)
            return;
        foreach (var cod in new[] { "FCT", "FCL", "DEC", "RLF", "RDC" }) {
            var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == cod);
            if (tip != null && tip.TipTvaImplicitId == null)
                tip.TipTvaImplicitId = n21.ID;
        }
    }

    // Tabelul D3-D2 — așezarea operațiunilor profilului privat pe rândurile
    // decontului. SINGURA sursă: și seed-ul, și gardianul de profil îl citesc,
    // ca lista nemapatelor deliberate să nu poată diverge de mapările reale.
    //
    // Cazurile care nu sunt „un tip → un rând":
    //   TI21/achiziție → 12.1 singur; rd. 26.1 e OGLINDĂ, o pune proiecția din
    //     cod (a doua mapare ar fi dublat cifra, nu ar fi completat-o);
    //   TI19/achiziție → 16 ȘI 33 — cotele istorice n-au rânduri proprii în
    //     forma 2026, deci taxarea inversă de 19% se declară integral prin
    //     regularizări, pe ambele laturi (exact cazul „n rânduri");
    //   N19 → 16 (colectat) / 33 (dedus), din același motiv;
    //   NED21/achiziție → 24: nedeductibilul intră în rd. 30 (taxă
    //     DEDUCTIBILĂ), dar nu în rd. 31 (taxă DEDUSĂ) — scăderea o face
    //     proiecția pe `Regim = Capitalizat`, nu o mapare lipsă.
    static readonly (string TipTva, SensTva Sens, string Rand)[] MapariD300 = [
        ("N21", SensTva.Livrare, "9"),
        ("N21", SensTva.Achizitie, "24"),
        ("N11", SensTva.Livrare, "10"),
        ("N11", SensTva.Achizitie, "25"),
        ("N9", SensTva.Livrare, "11"),
        ("TI21", SensTva.Livrare, "13"),
        ("TI21", SensTva.Achizitie, "12.1"),
        ("N19", SensTva.Livrare, "16"),
        ("N19", SensTva.Achizitie, "33"),
        ("TI19", SensTva.Livrare, "13"),
        ("TI19", SensTva.Achizitie, "16"),
        ("TI19", SensTva.Achizitie, "33"),
        ("NED21", SensTva.Achizitie, "24"),
        ("SDD", SensTva.Livrare, "14"),
        ("SDD", SensTva.Achizitie, "29"),
        ("SFD", SensTva.Livrare, "15"),
        ("SFD", SensTva.Achizitie, "29"),
        ("NIM", SensTva.Achizitie, "29"),
    ];

    // Perechile lăsate DELIBERAT nemapate (D3-D2), cu motivul lângă ele — o
    // gaură fără nume e o gaură pe care nimeni n-o mai găsește (decizia 21:
    // fiecare gaură de profil = decizie explicită, nu omisiune).
    static readonly (string TipTva, SensTva Sens, string Motiv)[] NemapateDeliberat = [
        // Formularul 2026 n-are rând de achiziție cu cota 9% (cea tranzitorie
        // e doar pentru livrări de locuințe, art. III din Legea 141/2025).
        ("N9", SensTva.Achizitie, "formularul n-are rând de achiziție cu cota 9%"),
        // Fără drept de deducere ⇒ nu există latură de livrare cu acest regim.
        ("NED21", SensTva.Livrare, "regimul e exclusiv de achiziție"),
        // În afara sferei TVA: nu se declară pe latura de livrare.
        ("NIM", SensTva.Livrare, "operațiunea e în afara sferei TVA"),
    ];

    // Politica de așezare pe decont (D3-D2), idempotentă pe tripletă. Rulează
    // după `SeedTipTva` (referă tipurile prin FK) și după nomenclatorul de
    // rânduri comis de nucleu (`ContaSeeder.SeedRandD300`).
    //
    // ȘTERGEREA LOGICĂ E O DECIZIE A UTILIZATORULUI, nu o gaură (fix F5 al
    // review-ului advers). Maparea e POLITICĂ — editabilă din XAF exact ca să
    // poată fi și ștearsă (decizia 4). Ștergerea în XAF e AMÂNATĂ (60a): rândul
    // rămâne în tabelă cu `GCRecord` setat. Prima formă a seed-ului îl căuta
    // doar printre cele VII, nu-l găsea, și îl recrea la fiecare
    // `--updateDatabase` — adică desfăcea tăcut o decizie luată deliberat, de
    // fiecare dată, iar contabilul care scosese SFD de pe rd. 29 îl regăsea
    // acolo după orice release. De aceea tripleta se caută IGNORÂND filtrul de
    // ștergere: „a existat vreodată" bate „există acum", fiindcă întrebarea nu e
    // dacă maparea lipsește, ci dacă lipsa ei e intenționată.
    // Public: ModelCheck (alt assembly) probează re-seed-ul pe CALEA REALĂ —
    // aceeași funcție pe care o cheamă `--updateDatabase`, nu o imitație.
    public static void SeedMapareD300(IObjectSpace os) {
        foreach (var m in MapariD300) {
            var tip = os.FirstOrDefault<TipTva>(t => t.Cod == m.TipTva);
            var rand = os.FirstOrDefault<RandD300>(r => r.Cod == m.Rand);
            if (tip == null || rand == null)
                throw new InvalidOperationException(
                    $"Maparea D300 {m.TipTva}/{m.Sens} → rd. {m.Rand} nu se poate seed-ui: lipsește din bază "
                    + (tip == null ? $"tipul de TVA {m.TipTva}" : $"rândul de decont {m.Rand}") + ".");
            var sens = m.Sens;
            if (os.GetObjectsQuery<MapareD300>()
                    .Any(x => x.TipTvaId == tip.ID && x.Sens == sens && x.RandId == rand.ID))
                continue;
            // A doua întrebare, pe tabela ÎNTREAGĂ: dacă rândul e acolo dar
            // șters, tăcerea ar fi la fel de rea ca recrearea — decizia rămâne
            // respectată, dar se SPUNE, ca o mapare lipsă din decont să aibă o
            // urmă în log-ul care a produs baza.
            if (os.GetObjectsQuery<MapareD300>().IgnoreQueryFilters()
                    .Any(x => x.TipTvaId == tip.ID && x.Sens == sens && x.RandId == rand.ID)) {
                Console.WriteLine($"  Mapare D300 {m.TipTva}/{m.Sens} → rd. {m.Rand}: ȘTEARSĂ de utilizator, "
                    + "nu se recreează (politica e date — decizia 4).");
                continue;
            }
            var mapare = os.CreateObject<MapareD300>();
            mapare.TipTva = tip;
            mapare.Sens = sens;
            mapare.Rand = rand;
        }
    }

    // Jumătatea de profil a gardianului D300 (`ContaSeeder.VerificaD300`):
    // fiecare pereche `(TipTva seed-uit × Sens)` are cel puțin o mapare SAU e
    // declarată nemapată. Domeniul e limitat la tipurile pe care le scrie ACEST
    // seed: un `TipTva` adăugat de client nu e obligat să aibă mapare — apare la
    // prima cifră în panoul „Operațiuni neincluse în decont" (D3-D8: raportarea
    // nu refuză ce operarea a acceptat).
    internal static void VerificaMapariD300(IObjectSpace os, IReadOnlyCollection<MapareD300> mapari) {
        var coduri = MapariD300.Select(m => m.TipTva)
            .Concat(NemapateDeliberat.Select(n => n.TipTva)).Distinct().ToList();
        // Perechile a căror mapare a fost ȘTEARSĂ de utilizator (fix F5): a
        // treia categorie, lângă „mapată" și „nemapată deliberat". Gardianul
        // apără seed-ul de propriile GĂURI, nu utilizatorul de propriile
        // decizii — un profil pe care contabilul l-a subțiat intenționat n-are
        // voie să facă `--updateDatabase` să arunce, adică să blocheze orice
        // release viitor pe baza aceea.
        var stersDeUtilizator = os.GetObjectsQuery<MapareD300>().IgnoreQueryFilters()
            .Select(m => new { m.TipTvaId, m.Sens })
            .ToList()
            .Select(m => (m.TipTvaId, m.Sens))
            .ToHashSet();
        foreach (var cod in coduri) {
            var tip = os.FirstOrDefault<TipTva>(t => t.Cod == cod);
            if (tip == null)
                throw new InvalidOperationException(
                    $"Tabelul de mapare D300 referă tipul de TVA {cod}, care nu există în bază.");
            foreach (var sens in new[] { SensTva.Achizitie, SensTva.Livrare }) {
                if (mapari.Any(m => m.TipTvaId == tip.ID && m.Sens == sens))
                    continue;
                if (stersDeUtilizator.Contains((tip.ID, sens)))
                    continue;
                var deliberat = NemapateDeliberat
                    .Where(n => n.TipTva == cod && n.Sens == sens).Select(n => n.Motiv).FirstOrDefault();
                if (deliberat == null)
                    throw new InvalidOperationException(
                        $"Tipul de TVA {cod} nu are nicio mapare D300 pe sensul {sens} și nici nu e "
                        + "declarat nemapat deliberat — operațiunile lui ar cădea tăcut în afara decontului.");
            }
        }
    }

    // ---------------- D394 (felia 14, D4-D2) ----------------
    //
    // Tabelul D4-D2: cotele normale = L/A; taxarea inversă = V (livrare, fără
    // TVA — beneficiarul o autolichidează) / C (achiziție, cu TVA autolichidată);
    // NED21 doar pe achiziție = A (achiziție taxabilă, se declară cu TVA-ul
    // facturat de furnizor, chiar dacă noi nu-l deducem). `AI` NU se mapează:
    // se derivă din `Partener.TvaLaIncasare` la proiecție.
    static readonly (string TipTva, SensTva Sens, TipOperatiuneD394 Tip)[] MapariD394 = [
        ("N21", SensTva.Livrare, TipOperatiuneD394.L),
        ("N21", SensTva.Achizitie, TipOperatiuneD394.A),
        ("N11", SensTva.Livrare, TipOperatiuneD394.L),
        ("N11", SensTva.Achizitie, TipOperatiuneD394.A),
        ("N9", SensTva.Livrare, TipOperatiuneD394.L),
        ("N9", SensTva.Achizitie, TipOperatiuneD394.A),
        ("N19", SensTva.Livrare, TipOperatiuneD394.L),
        ("N19", SensTva.Achizitie, TipOperatiuneD394.A),
        ("TI21", SensTva.Livrare, TipOperatiuneD394.V),
        ("TI21", SensTva.Achizitie, TipOperatiuneD394.C),
        ("TI19", SensTva.Livrare, TipOperatiuneD394.V),
        ("TI19", SensTva.Achizitie, TipOperatiuneD394.C),
        ("NED21", SensTva.Achizitie, TipOperatiuneD394.A),
    ];

    // Perechile lăsate DELIBERAT în afara declarației 394, cu motivul lângă ele
    // (decizia 21: fiecare gaură de profil = decizie explicită). Cifrele lor
    // apar în panoul `Neincluse` al proiecției (D4-D4), nu dispar.
    static readonly (string TipTva, SensTva Sens, string Motiv)[] NemapateDeliberatD394 = [
        ("NED21", SensTva.Livrare, "nedeductibilul e exclusiv de achiziție — nu se livrează"),
        // Scutitele și neimpozabilele nu se declară în 394 (formularul cuprinde
        // doar operațiunile taxabile/cu taxare inversă/regim special).
        ("SDD", SensTva.Livrare, "operațiune scutită — nu se declară în 394"),
        ("SDD", SensTva.Achizitie, "operațiune scutită — nu se declară în 394"),
        ("SFD", SensTva.Livrare, "operațiune scutită — nu se declară în 394"),
        ("SFD", SensTva.Achizitie, "operațiune scutită — nu se declară în 394"),
        ("NIM", SensTva.Livrare, "operațiunea e în afara sferei TVA — nu se declară în 394"),
        ("NIM", SensTva.Achizitie, "operațiunea e în afara sferei TVA — nu se declară în 394"),
    ];

    // Lista nemapatelor e parte din CONTRACT (69e/D4-D2): proiecția o citește ca
    // să deosebească „nemapat deliberat" de „nemapat din greșeală" în `Neincluse`.
    public static IReadOnlyCollection<(string TipTva, SensTva Sens, string Motiv)> NemapateD394 =>
        NemapateDeliberatD394;

    // Idempotent pe PERECHE și cu respectarea ștergerii logice a utilizatorului
    // (precedentul F5/69b, motivat la `SeedMapareD300`): o mapare ștearsă din
    // XAF nu se recreează la `--updateDatabase`, dar se SPUNE. Public: ModelCheck
    // probează re-seed-ul pe funcția reală.
    public static void SeedMapareD394(IObjectSpace os) {
        foreach (var m in MapariD394) {
            if (!MapareD394.TintaPermisa(m.Tip, m.Sens))
                throw new InvalidOperationException(
                    $"Tabelul de seed D394 țintește {m.Tip} pentru {m.TipTva}/{m.Sens} — AI/N nu se mapează, "
                    + "iar tipul trebuie să fie coerent cu sensul (D4-D2).");
            var tip = os.FirstOrDefault<TipTva>(t => t.Cod == m.TipTva)
                ?? throw new InvalidOperationException(
                    $"Maparea D394 {m.TipTva}/{m.Sens} → {m.Tip} nu se poate seed-ui: lipsește din bază tipul de TVA {m.TipTva}.");
            var sens = m.Sens;
            if (os.GetObjectsQuery<MapareD394>().Any(x => x.TipTvaId == tip.ID && x.Sens == sens))
                continue;
            if (os.GetObjectsQuery<MapareD394>().IgnoreQueryFilters()
                    .Any(x => x.TipTvaId == tip.ID && x.Sens == sens)) {
                Console.WriteLine($"  Mapare D394 {m.TipTva}/{m.Sens} → {m.Tip}: ȘTEARSĂ de utilizator, "
                    + "nu se recreează (politica e date — decizia 4).");
                continue;
            }
            var mapare = os.CreateObject<MapareD394>();
            mapare.TipTva = tip;
            mapare.Sens = sens;
            mapare.Tip = m.Tip;
        }
    }

    // Jumătatea de profil a gardianului D394 (`ContaSeeder.VerificaD394`): fiecare
    // pereche `(TipTva seed-uit × Sens)` e mapată, declarată nemapată sau ștearsă
    // de utilizator (a treia categorie, F5). Domeniul = tipurile scrise de ACEST
    // seed; un `TipTva` al clientului fără mapare apare în `Neincluse`, nu e refuzat.
    internal static void VerificaMapariD394(IObjectSpace os, IReadOnlyCollection<MapareD394> mapari) {
        var coduri = MapariD394.Select(m => m.TipTva)
            .Concat(NemapateDeliberatD394.Select(n => n.TipTva)).Distinct().ToList();
        var stersDeUtilizator = os.GetObjectsQuery<MapareD394>().IgnoreQueryFilters()
            .Select(m => new { m.TipTvaId, m.Sens })
            .ToList()
            .Select(m => (m.TipTvaId, m.Sens))
            .ToHashSet();
        foreach (var cod in coduri) {
            var tip = os.FirstOrDefault<TipTva>(t => t.Cod == cod)
                ?? throw new InvalidOperationException(
                    $"Tabelul de mapare D394 referă tipul de TVA {cod}, care nu există în bază.");
            foreach (var sens in new[] { SensTva.Achizitie, SensTva.Livrare }) {
                if (mapari.Any(m => m.TipTvaId == tip.ID && m.Sens == sens))
                    continue;
                if (stersDeUtilizator.Contains((tip.ID, sens)))
                    continue;
                if (!NemapateDeliberatD394.Any(n => n.TipTva == cod && n.Sens == sens))
                    throw new InvalidOperationException(
                        $"Tipul de TVA {cod} nu are mapare D394 pe sensul {sens} și nici nu e declarat nemapat "
                        + "deliberat — operațiunile lui ar cădea tăcut în afara declarației.");
            }
        }
    }

    // ── Politica de mișcare SAF-T S (felia 17, D17-D1) ──────────────────────
    //
    // Tabelul e derivat pe FUNCȚIONALITATE (decizia 21), din perechea
    // „ce scrie tipul în registru" (`SeedReguliStoc` de mai jos) × „cum se
    // numește mișcarea aia în D406": NIR aduce marfă de la furnizor (`10`), DSC o
    // dă clientului (`30`), BTR o mută între gestiuni (`80`), BCS o consumă
    // (`70`), LDI o găsește în plus (`110`) sau în minus (`120`), ASM produce
    // (`20`) consumând (`70`), RLF o întoarce furnizorului (`50`), RDC o
    // primește înapoi de la client (`40`).
    //
    // `Semn` NULL = „orice semn" acolo unde tipul are o singură direcție per
    // registru (NIR intră mereu, BTR are ACEEAȘI mișcare pe ambele picioare);
    // ±1 acolo unde direcția schimbă codul (LDI, ASM).
    //
    // Registrele: `Magazie` + `Marfuri` peste tot, fiindcă exact aceeași pereche
    // o scrie `SeedReguliStoc` la privat (genericul + clasa MF). Singura excepție
    // e `Consum`, pe care doar BCS îl atinge.
    //
    // Cod NULL pe `BCS/Consum/+1`: rândul de intrare în consum e o mutare de
    // RESPONSABILITATE (27a), nu o mișcare de stoc în magazie — declarat, ar
    // dubla ieșirea `70` a aceleiași linii. Excludere DELIBERATĂ, cu motiv, nu
    // omisiune: cifrele ei apar în `Excluse`.
    //
    // Codurile fără sursă în model azi (60 reduceri comerciale, 90 capitalizări,
    // 100/101 diferențe de preț, 130–180) NU se seed-uiesc: niciun tip de
    // document nu le produce. Politica le poate primi fără release când apar.
    static readonly (string Tip, TipStoc TipStoc, int? Semn, string Cod, RolTertSaft Rol, string Motiv)[]
        PoliticiMiscareSaft = [
            ("NIR", TipStoc.Magazie, null, "10", RolTertSaft.Furnizor, null),
            ("NIR", TipStoc.Marfuri, null, "10", RolTertSaft.Furnizor, null),
            ("BTR", TipStoc.Magazie, null, "80", RolTertSaft.Niciunul, null),
            ("BTR", TipStoc.Marfuri, null, "80", RolTertSaft.Niciunul, null),
            ("BCS", TipStoc.Magazie, -1, "70", RolTertSaft.Niciunul, null),
            ("BCS", TipStoc.Marfuri, -1, "70", RolTertSaft.Niciunul, null),
            ("BCS", TipStoc.Consum, +1, null, RolTertSaft.Niciunul,
                "Consumul pe responsabil nu e stoc în magazie (27a)"),
            ("LDI", TipStoc.Magazie, +1, "110", RolTertSaft.Niciunul, null),
            ("LDI", TipStoc.Marfuri, +1, "110", RolTertSaft.Niciunul, null),
            ("LDI", TipStoc.Magazie, -1, "120", RolTertSaft.Niciunul, null),
            ("LDI", TipStoc.Marfuri, -1, "120", RolTertSaft.Niciunul, null),
            ("DSC", TipStoc.Magazie, -1, "30", RolTertSaft.Client, null),
            ("DSC", TipStoc.Marfuri, -1, "30", RolTertSaft.Client, null),
            ("ASM", TipStoc.Magazie, +1, "20", RolTertSaft.Niciunul, null),
            ("ASM", TipStoc.Marfuri, +1, "20", RolTertSaft.Niciunul, null),
            ("ASM", TipStoc.Magazie, -1, "70", RolTertSaft.Niciunul, null),
            ("ASM", TipStoc.Marfuri, -1, "70", RolTertSaft.Niciunul, null),
            ("RLF", TipStoc.Magazie, -1, "50", RolTertSaft.Furnizor, null),
            ("RLF", TipStoc.Marfuri, -1, "50", RolTertSaft.Furnizor, null),
            ("RDC", TipStoc.Magazie, +1, "40", RolTertSaft.Client, null),
            ("RDC", TipStoc.Marfuri, +1, "40", RolTertSaft.Client, null),
        ];

    // Tabelul e parte din contract (D17-D1): ModelCheck îl citește prin nucleu
    // (`ContaSeeder`), ca să nu-și scrie propria copie a cifrelor.
    public static IReadOnlyCollection<(string Tip, TipStoc TipStoc, int? Semn, string Cod, RolTertSaft Rol, string Motiv)>
        MiscariSaft => PoliticiMiscareSaft;

    // Idempotent pe CHEIE (tip × TipStoc × Semn) și cu respectarea ștergerii
    // logice a utilizatorului — același tipar ca `SeedMapareD394` (precedentul
    // F5/69b): politica e date (decizia 4), deci un rând șters din XAF nu se
    // recreează la `--updateDatabase`, dar se SPUNE. Public: ModelCheck probează
    // re-seed-ul pe funcția reală.
    public static void SeedPoliticiMiscareSaft(IObjectSpace os) {
        foreach (var p in PoliticiMiscareSaft) {
            if (p.Cod != null && !SaftReguli.EsteCodMiscare(p.Cod))
                throw new InvalidOperationException(
                    $"Tabelul de seed al mișcărilor SAF-T dă codul „{p.Cod}” pe {p.Tip}/{p.TipStoc} — "
                    + "codul nu e în nomenclatorul D406 (D17-D2).");
            if (p.Cod == null && string.IsNullOrWhiteSpace(p.Motiv))
                throw new InvalidOperationException(
                    $"Tabelul de seed al mișcărilor SAF-T exclude {p.Tip}/{p.TipStoc} fără motiv — "
                    + "excluderea deliberată se citește pe ecran lângă cifrele ei (D17-D1).");
            var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == p.Tip)
                ?? throw new InvalidOperationException(
                    $"Politica de mișcare SAF-T {p.Tip}/{p.TipStoc} nu se poate seed-ui: "
                    + $"lipsește din bază tipul de document {p.Tip}.");
            var tipStoc = p.TipStoc;
            var semn = p.Semn;
            if (os.GetObjectsQuery<PoliticaMiscareSaft>()
                    .Any(x => x.TipDocumentId == tip.ID && x.TipStoc == tipStoc && x.Semn == semn))
                continue;
            if (os.GetObjectsQuery<PoliticaMiscareSaft>().IgnoreQueryFilters()
                    .Any(x => x.TipDocumentId == tip.ID && x.TipStoc == tipStoc && x.Semn == semn)) {
                Console.WriteLine($"  Politica de mișcare SAF-T {p.Tip}/{p.TipStoc}/{p.Semn?.ToString() ?? "orice"}: "
                    + "ȘTEARSĂ de utilizator, nu se recreează (politica e date — decizia 4).");
                continue;
            }
            var politica = os.CreateObject<PoliticaMiscareSaft>();
            politica.TipDocument = tip;
            politica.TipStoc = tipStoc;
            politica.Semn = semn;
            politica.CodMiscare = p.Cod;
            politica.RolTert = p.Rol;
            politica.Motiv = p.Motiv;
        }
    }

    // Registrele private, ca rânduri de politică: generic → Magazie, mărfurile pe
    // registrul propriu. INCREMENTAL per (latură × clasă), nu „există un rând ⇒
    // gata": tipurile seed-uite înaintea P2 (BTR/BCS — feliile 3b/3c) au primit
    // doar rândul generic, iar un lot de MARFĂ trăiește în registrul Marfuri (așa
    // îl scriu NIR/LDI/DSC/ASM/retururile și deschiderea importului 1C). Fără
    // rândul MF, orice transfer sau consum de marfă ar căuta soldul în Magazie și
    // ar cădea pe gardianul de sold — gaură de profil scoasă la iveală de import
    // (decizia 21/45f), nu schimbare de semantică.
    static void SeedReguliStoc(IObjectSpace os, TipDocument tipDoc, LaturaDocument latura, int semn,
            params (string Clasa, TipStoc TipStoc)[] reguli) {
        foreach (var r in reguli) {
            var clasaId = r.Clasa == null
                ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa)?.ID;
            var exista = clasaId == null
                ? os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID
                    && x.Latura == latura && x.ClasaId == null)
                : os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID
                    && x.Latura == latura && x.ClasaId == clasaId);
            if (exista != null)
                continue;
            var regula = os.CreateObject<RegulaStoc>();
            regula.TipDocument = tipDoc;
            regula.Latura = latura;
            regula.ClasaId = clasaId;
            regula.TipStoc = r.TipStoc;
            regula.Semn = semn;
        }
    }

    // Registrele „generic + mărfuri" folosite de aproape toate tipurile private.
    static readonly (string Clasa, TipStoc TipStoc)[] MagazieSiMarfuri =
        [(null, TipStoc.Magazie), ("MF", TipStoc.Marfuri)];

    // Transferul (23c): ± pe același registru, fără contare la plan sintetic.
    static void SeedPoliticiNotaTransfer(IObjectSpace os) {
        var btr = os.FirstOrDefault<TipDocument>(x => x.Cod == "BTR");
        ContaSeeder.SeedNumerotare(os, "BTR", "BTR-");
        SeedReguliStoc(os, btr, LaturaDocument.Predator, -1, MagazieSiMarfuri);
        SeedReguliStoc(os, btr, LaturaDocument.Primitor, +1, MagazieSiMarfuri);
    }

    // Lanțul de cumpărare (26a, sub TVA structural — design §6): recepția
    // contează pe NIR la NET (3xx = 401), factura postează liniile non-stoc
    // net + rândurile 4426 per linie (inclusiv ale liniilor de stoc).
    static void SeedPoliticiFacturaIntrareNir(IObjectSpace os) {
        var fct = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCT");
        var nir = os.FirstOrDefault<TipDocument>(x => x.Cod == "NIR");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
        var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404");

        ContaSeeder.StergeReguliContareStricate(os);
        ContaSeeder.SeedNumerotare(os, "NIR", "NIR-");

        var conex = os.FirstOrDefault<PoliticaConex>(x => x.TipDocumentSursa.Cod == "FCT");
        if (conex == null) {
            conex = os.CreateObject<PoliticaConex>();
            conex.TipDocumentSursa = fct;
            conex.TipDocumentTinta = nir;
        }
        conex.InverseazaLaturi = false;
        conex.NaturaFiltru = NaturaClasa.Stoc;

        // Stoc NIR: +1 pe primitor; generic → Magazie, mărfurile pe registrul
        // propriu (restul claselor speciale bugetare nu există la privat).
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "NIR") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = nir;
                regula.Latura = LaturaDocument.Primitor;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = +1;
            }
        }

        // Contare NIR: 3xx (contul Tipului) = furnizor, la NET.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "NIR") == null) {
            var receptie = os.CreateObject<RegulaContare>();
            receptie.TipDocument = nir;
            receptie.NaturaFiltru = NaturaClasa.Stoc;
            receptie.SursaContDebit = SursaCont.TipMaterial;
            receptie.SursaContCredit = SursaCont.RepartitorPredator;
            receptie.ContCredit = cont401;
        }

        // Contare FCT: doar naturile care NU trec pe NIR, la net; 404 la imobilizări.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "FCT") == null) {
            (NaturaClasa Natura, Cont Fallback)[] reguli = [
                (NaturaClasa.Serviciu, cont401),
                (NaturaClasa.Cheltuiala, cont401),
                (NaturaClasa.Imobilizare, cont404),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaContare>();
                regula.TipDocument = fct;
                regula.NaturaFiltru = r.Natura;
                regula.SursaContDebit = SursaCont.TipMaterial;
                regula.SursaContCredit = SursaCont.RepartitorPredator;
                regula.ContCredit = r.Fallback;
            }
        }
    }

    // Consumul (27): −Magazie / +Consum; contarea 6xx = 3xx derivată din simbol,
    // cu excepțiile profilului (607/711/608).
    static void SeedPoliticiBonConsum(IObjectSpace os) {
        var bcs = os.FirstOrDefault<TipDocument>(x => x.Cod == "BCS");
        ContaSeeder.SeedNumerotare(os, "BCS", "BCS-");
        // Ieșirea din registrul în care STĂ lotul (generic Magazie, marfă
        // Marfuri — vezi nota de la SeedReguliStoc); intrarea în Consum e
        // aceeași pentru orice clasă, deci un singur rând generic.
        SeedReguliStoc(os, bcs, LaturaDocument.Predator, -1, MagazieSiMarfuri);
        SeedReguliStoc(os, bcs, LaturaDocument.Primitor, +1, (null, TipStoc.Consum));
        ContaSeeder.SeedContare6xxDin3xx(os, bcs, null, Derivari6xxExceptii);
    }

    // Inventarierea (28): +1 pe predator, direcția în semn. Minus = 6xx = 3xx
    // (cu excepțiile profilului), plus = 3xx = 7588 „Alte venituri din
    // exploatare" (decizia 29c — la privat NU 791).
    static void SeedPoliticiListaDiferente(IObjectSpace os) {
        var ldi = os.FirstOrDefault<TipDocument>(x => x.Cod == "LDI");
        ContaSeeder.SeedNumerotare(os, "LDI", "LDI-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "LDI") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = ldi;
                regula.Latura = LaturaDocument.Predator;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = +1;
            }
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "LDI" && x.TipMaterialId == null) == null) {
            var plus = os.CreateObject<RegulaContare>();
            plus.TipDocument = ldi;
            plus.NaturaFiltru = NaturaClasa.Stoc;
            plus.SemnFiltru = +1;
            plus.SursaContDebit = SursaCont.TipMaterial;
            plus.SursaContCredit = SursaCont.Explicit;
            plus.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "7588");
        }
        ContaSeeder.SeedContare6xxDin3xx(os, ldi, -1, Derivari6xxExceptii);
    }

    // Facturarea (30): pur creanță până la P2; 4111 = 7xx net + 4111 = 4427
    // per linie (prin PoliticaTva). Scadență default +30.
    static void SeedPoliticiFacturaIesire(IObjectSpace os) {
        var fcl = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCL");
        ContaSeeder.SeedNumerotare(os, "FCL", "FCL-");
        if (os.FirstOrDefault<PoliticaScadenta>(x => x.TipDocument.Cod == "FCL") == null) {
            var scadenta = os.CreateObject<PoliticaScadenta>();
            scadenta.TipDocument = fcl;
            scadenta.ZileDefault = 30;
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "FCL") == null) {
            var facturare = os.CreateObject<RegulaContare>();
            facturare.TipDocument = fcl;
            facturare.SursaContDebit = SursaCont.RepartitorPrimitor;
            facturare.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            facturare.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    // Descărcarea de gestiune (P2, design §3/§6): tip nou DSC generat pe loturi
    // din FCL (DescarcareService). Stoc: −1 pe predator (gestiunea de
    // descărcare), aceeași mapare de registre ca NIR/LDI privat (generic →
    // Magazie, MF → Marfuri). Cost: 6xx = 3xx per Tip cu excepțiile profilului
    // (607=371, 711=345, 608=381). Pe FCL: derivarea de VÂNZARE — pe liniile de
    // stoc creditul e venitul (371→707, 345→701, 381→708), nu contul de stoc;
    // genericul FCL rămâne pentru servicii. Plus curățarea rândului P1 care
    // interzicea natura Stoc pe FCL (37e — descărcarea o preia acum).
    static void SeedPoliticiDescarcare(IObjectSpace os) {
        var dsc = os.FirstOrDefault<TipDocument>(x => x.Cod == "DSC");
        var fcl = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCL");

        ContaSeeder.SeedNumerotare(os, "DSC", "DSC-");

        // Stoc DSC: −1 pe predator; generic → Magazie, mărfurile pe registrul
        // propriu (oglindește EXACT rândurile NIR/LDI privat).
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "DSC") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = dsc;
                regula.Latura = LaturaDocument.Predator;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = -1;
            }
        }

        // Costul descărcării: 6xx = 3xx per Tip, excepțiile profilului (60x=30x).
        ContaSeeder.SeedContare6xxDin3xx(os, dsc, null, Derivari6xxExceptii);

        // Vânzarea pe FCL: creditul = venitul (nu contul de stoc); fallback 708.
        ContaSeeder.SeedContareVanzare(os, fcl, "4111",
            new Dictionary<string, string> { ["371"] = "707", ["345"] = "701", ["381"] = "708" }, "708");

        // Pas explicit de updater: FCL nu mai interzice natura Stoc (rândul P1
        // există în bazele seed-uite atunci). CereClasificatieBugetara nu se
        // setează la privat, deci rândul se șterge întreg; idempotent.
        var validareFcl = os.FirstOrDefault<PoliticaValidare>(
            x => x.TipDocument.Cod == "FCL" && x.NaturaInterzisa == NaturaClasa.Stoc);
        if (validareFcl != null) {
            if (validareFcl.CereClasificatieBugetara)
                validareFcl.NaturaInterzisa = null;
            else
                os.Delete(validareFcl);
        }
    }

    // Trezoreria (31): conturi proprii OMFP (casa 5311, banca 5121); contare
    // din laturi — PLT: beneficiar (fallback 401) = cont propriu (fără
    // fallback); INC: oglindit (fallback 4111 pe plătitor).
    static void SeedPoliticiTrezorerie(IObjectSpace os) {
        var casa = os.FirstOrDefault<ContPropriu>(x => x.Cod == "CASA");
        if (casa == null) {
            casa = os.CreateObject<ContPropriu>();
            casa.Cod = "CASA";
            casa.Denumire = "Casa în lei";
            casa.EsteBanca = false;
            casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "5311");
        }
        var banca = os.FirstOrDefault<ContPropriu>(x => x.Cod == "BANCA");
        if (banca == null) {
            banca = os.CreateObject<ContPropriu>();
            banca.Cod = "BANCA";
            banca.Denumire = "Cont curent la bancă în lei";
            banca.EsteBanca = true;
            banca.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "5121");
        }

        ContaSeeder.SeedNumerotare(os, "PLT", "PLT-");
        ContaSeeder.SeedNumerotare(os, "INC", "INC-");

        var plt = os.FirstOrDefault<TipDocument>(x => x.Cod == "PLT");
        var inc = os.FirstOrDefault<TipDocument>(x => x.Cod == "INC");

        // Rândurile GENERICE (plata/încasarea obișnuită). Garda e PER RÂND, pe
        // cheia lui (tip document + fără filtre): garda veche „există vreo regulă
        // pe PLT" ar sări rândul de virament de mai jos pe orice bază existentă.
        if (ContaSeeder.RegulaContareLipsa(os, plt, null, null)) {
            var plata = os.CreateObject<RegulaContare>();
            plata.TipDocument = plt;
            plata.SursaContDebit = SursaCont.RepartitorPrimitor;
            plata.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            plata.SursaContCredit = SursaCont.RepartitorPredator;
        }
        if (ContaSeeder.RegulaContareLipsa(os, inc, null, null)) {
            var incasare = os.CreateObject<RegulaContare>();
            incasare.TipDocument = inc;
            incasare.SursaContDebit = SursaCont.RepartitorPrimitor;
            incasare.SursaContCredit = SursaCont.RepartitorPredator;
            incasare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
        }

        // Viramentul intern (F7-D6): contul de tranzit 581 ca DATE, prin Clasa/
        // Tipul „VIR" — motorul nu cunoaște niciun simbol (decizia 29).
        ContaSeeder.SeedContareVirament(os, plt, inc, "581");
    }

    // Nota contabilă (FAZA 1C §5): SINGURA politică e numerotarea — identic cu
    // bugetarul (nota e neutră față de profil: fără stoc, fără contare, fără
    // TVA/scadență/validare; postarea explicită a liniei e completă).
    static void SeedPoliticiNotaContabila(IObjectSpace os) {
        ContaSeeder.SeedNumerotare(os, "NTC", "NTC-");
    }

    // Închiderea lunară de TVA (FAZA 1C §6): numerotare proprie + conturile
    // închiderii ca DATE (4426/4427/4423/4424 OMFP). Motorul nu cunoaște niciun
    // simbol — fără rândul ăsta ITV e tip inert (cazul bugetar).
    static void SeedPoliticiInchidereTva(IObjectSpace os) {
        ContaSeeder.SeedNumerotare(os, "ITV", "ITV-");
        if (os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocument.Cod == "ITV") != null)
            return;
        var politica = os.CreateObject<PoliticaInchidereTva>();
        politica.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
        politica.ContDeductibila = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
        politica.ContColectata = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
        politica.ContDePlata = os.FirstOrDefault<Cont>(c => c.Simbol == "4423");
        politica.ContDeRecuperat = os.FirstOrDefault<Cont>(c => c.Simbol == "4424");
    }

    // Asamblarea (FAZA 1C §7): kitting n→m pe stoc, într-o gestiune. Stoc: UN
    // SINGUR set de reguli, +1 pe predator — SEMNUL LINIEI dă direcția (consum
    // −, produs +, materializat în PregatesteOperare, mecanismul LDI 28a);
    // regula spune doar latura și registrul. Aceeași mapare de registre ca
    // NIR/LDI/DSC privat (generic → Magazie, MF → Marfuri).
    // FĂRĂ RegulaContare: la plan sintetic marfă→marfă (371=371) e zgomot
    // (raționamentul 23c, ca la NotaTransfer) — valoarea se mută între loturi,
    // nu între conturi. Producția reală (345=711) primește reguli la cerință.
    static void SeedPoliticiAsamblare(IObjectSpace os) {
        var asm = os.FirstOrDefault<TipDocument>(x => x.Cod == "ASM");
        ContaSeeder.SeedNumerotare(os, "ASM", "ASM-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "ASM") != null)
            return;
        (string Clasa, TipStoc TipStoc)[] reguli = [
            (null, TipStoc.Magazie),
            ("MF", TipStoc.Marfuri),
        ];
        foreach (var r in reguli) {
            var regula = os.CreateObject<RegulaStoc>();
            regula.TipDocument = asm;
            regula.Latura = LaturaDocument.Predator;
            regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
            regula.TipStoc = r.TipStoc;
            regula.Semn = +1;
        }
    }

    // Retururile (FAZA 1C §7, rezoluția spike-ului storno): corespondența
    // ORIGINALĂ cu valori NEGATIVE. Liniile se culeg pozitive și se semnează la
    // operare (PregatesteOperare), deci regulile spun doar LATURA și registrul —
    // semnul liniei face direcția. `PastreazaSemn` scoate normalizarea de semn
    // din motor pe rândurile astea (singura extensie de motor a feliei).
    //   RLF: stoc +1 pe PREDATOR (gestiunea) × linia −q ⇒ −q (marfa iese);
    //        contare 3xx = 401 cu −V; TVA 4426 = 401 cu −TVA (PoliticaTva).
    //   RDC: stoc −1 pe PRIMITOR (gestiunea) × linia −q ⇒ +q (marfa revine pe
    //        lotul original); venit 4111 = 70x cu −V, cost 607 = 371 cu −cost,
    //        TVA 4111 = 4427 cu −TVA. Liniile de venit (Natura=Serviciu) nu
    //        sunt atinse de regulile generice de stoc (Natura=Stoc).
    // Mapările de registre oglindesc NIR/LDI/DSC privat (generic → Magazie,
    // MF → Marfuri).
    static void SeedPoliticiRetururi(IObjectSpace os) {
        var rlf = os.FirstOrDefault<TipDocument>(x => x.Cod == "RLF");
        var rdc = os.FirstOrDefault<TipDocument>(x => x.Cod == "RDC");
        ContaSeeder.SeedNumerotare(os, "RLF", "RLF-");
        ContaSeeder.SeedNumerotare(os, "RDC", "RDC-");

        void ReguliStoc(TipDocument tipDoc, LaturaDocument latura, int semn) {
            if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID) != null)
                return;
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = tipDoc;
                regula.Latura = latura;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = semn;
            }
        }
        ReguliStoc(rlf, LaturaDocument.Predator, +1);
        ReguliStoc(rdc, LaturaDocument.Primitor, -1);

        // RLF: stornarea achiziției — contul de stoc al Tipului = furnizorul.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocumentId == rlf.ID) == null) {
            var retur = os.CreateObject<RegulaContare>();
            retur.TipDocument = rlf;
            retur.NaturaFiltru = NaturaClasa.Stoc;
            retur.PastreazaSemn = true;
            retur.SursaContDebit = SursaCont.TipMaterial;
            retur.SursaContCredit = SursaCont.RepartitorPrimitor;
            retur.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
        }

        // RDC, liniile de venit: stornarea vânzării — clientul (predator) =
        // contul de venit al Tipului, FĂRĂ fallback (Tip fără cont = eroare
        // clară la operare, filozofia 30b).
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocumentId == rdc.ID && x.TipMaterialId == null) == null) {
            var venit = os.CreateObject<RegulaContare>();
            venit.TipDocument = rdc;
            venit.NaturaFiltru = NaturaClasa.Serviciu;
            venit.PastreazaSemn = true;
            venit.SursaContDebit = SursaCont.RepartitorPredator;
            venit.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            venit.SursaContCredit = SursaCont.TipMaterial;
        }

        // RDC, liniile de cost: costul REVINE — 6xx = 3xx per Tip cu excepțiile
        // profilului (607=371, 711=345, 608=381), valoarea negativă a liniei.
        ContaSeeder.SeedContare6xxDin3xx(os, rdc, semnFiltru: null, Derivari6xxExceptii, pastreazaSemn: true);
    }

    // Decontul (32): debit din contul Tipului (fără fallback), credit = avansul
    // titularului (542 OMFP ca fallback); TVA-ul justificat postează 4426 = 542
    // prin PoliticaTva.
    static void SeedPoliticiDecont(IObjectSpace os) {
        var dec = os.FirstOrDefault<TipDocument>(x => x.Cod == "DEC");
        ContaSeeder.SeedNumerotare(os, "DEC", "DEC-");
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "DEC") == null) {
            var justificare = os.CreateObject<RegulaContare>();
            justificare.TipDocument = dec;
            justificare.SursaContDebit = SursaCont.TipMaterial;
            justificare.SursaContCredit = SursaCont.RepartitorPredator;
            justificare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "542");
        }
    }
}
