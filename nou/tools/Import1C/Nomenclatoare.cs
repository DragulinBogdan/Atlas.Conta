using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.EF;

namespace Import1C;

// PASUL 2 al feliei 1C-b: nomenclatoarele.
//
// Împărțirea e dictată de volum, nu de gust: nomenclatoarele MICI (depozite 102,
// casierii 8, conturi bancare proprii 46, persoane fizice 122) se importă
// integral — sunt laturi de document și trebuie să existe înainte de orice
// import de documente. Cele MARI (Partenerii 129k, Nomenclator 312k) se aduc LA
// CERERE, pe măsură ce deschiderea și documentele le referă — precedentul
// „doar cele referite de deschidere" al pasului 4 (decizia 34e).
//
// Idempotența e uniformă: `MigrareLegatura` cheiată „1C:<view>" pe hex-ul
// KeyField (Legaturi.cs). Sursele 1C au coduri naturale UNICE (verificat pe
// date: 102/102, 8/8, 46/46, 122/122) — codul e folosit ca a doua ancoră, la
// recuperarea unei rulări întrerupte între commit-ul entității și cel al
// legăturii.
static class Nomenclatoare {

    // Upsert prin legătură (mecanica Migrare, decizia 34b): entitatea existentă
    // se RE-actualizează la fiecare rulare (o corecție în 1C ajunge în Atlas),
    // cea nouă se creează, iar legătura se scrie DUPĂ `CommitChanges` — ID-ul
    // entității e contractul ei, nu se anticipează.
    static (int Procesate, int Noi) Upsert<T>(IObjectSpace os, string view,
            IReadOnlyList<(string Cheie, string Cod, Action<T> Aplica)> randuri, Action<string> avert)
            where T : Repartitor {
        var tabela = Legaturi.Tabela(view);
        var legaturi = Legaturi.Incarca(os, view);
        var deLegat = new List<(string Cheie, T Entitate)>();

        // Recuperarea rulării întrerupte între cele două commit-uri (fix review
        // 1C-b; mecanica Materializeaza): o entitate de tipul T cu același Cod,
        // nelegată de NICIO legătură, nu poate proveni decât dintr-un kill între
        // commit-ul entității și cel al legăturii — se adoptă, nu se dublează.
        // (Entitățile seed — MAG1/CASA/… — sunt și ele nelegate, dar codurile lor
        // nu apar printre codurile sursei 1C, deci nu pot fi adoptate.)
        var legateGlobal = os.GetObjectsQuery<MigrareLegatura>().Select(m => m.TintaId).ToHashSet();
        var nelegate = os.GetObjectsQuery<T>().ToList()
            .Where(e => !legateGlobal.Contains(e.ID) && e.Cod != null)
            .GroupBy(e => e.Cod)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var (cheie, cod, aplica) in randuri) {
            T entitate = null;
            if (legaturi.TryGetValue(cheie, out var tinta)) {
                entitate = os.GetObjectByKey<T>(tinta);
                if (entitate == null) {
                    // Legătură fără entitate: baza a fost golită parțial sau
                    // entitatea ștearsă din UI. Se recreează, legătura moartă se
                    // șterge — altfel a doua rulare ar cădea la fel.
                    avert($"Legătură orfană {tabela}/{cheie} — entitatea lipsește; recreată.");
                    var moarta = os.FirstOrDefault<MigrareLegatura>(
                        m => m.Tabela == tabela && m.CheieLegacy == cheie);
                    if (moarta != null)
                        os.Delete(moarta);
                }
            }
            if (entitate == null && cod != null && nelegate.TryGetValue(cod, out var candidat)) {
                avert($"{tabela}/{cheie}: adoptată entitatea nelegată cu codul „{cod}” "
                    + "(rest al unei rulări întrerupte) — nu se dublează.");
                entitate = candidat;
                nelegate.Remove(cod);
                deLegat.Add((cheie, entitate));
            }
            if (entitate == null) {
                entitate = os.CreateObject<T>();
                deLegat.Add((cheie, entitate));
            }
            aplica(entitate);
        }

        os.CommitChanges();
        foreach (var (cheie, e) in deLegat)
            Legaturi.Leaga(os, view, cheie, e.ID);
        os.CommitChanges();
        return (randuri.Count, deLegat.Count);
    }

    // Depozitele 1C → `Gestiune` (derivata fără câmpuri proprii). Se importă DOAR
    // elementele — grupurile sunt foldere de navigare, nu locuri de stoc. Cele
    // marcate la ștergere (30) intră totuși: documentele istorice ale anului
    // importat le referă; marcajul devine `Activ = false` (dispar din lookup-uri,
    // rămân legale ca latură a documentelor vechi).
    public static (int Procesate, int Noi) Depozite(IObjectSpace os,
            IReadOnlyList<FlaxDepozit> sursa, Action<string> avert) =>
        Upsert<Gestiune>(os, "Depozite", sursa.Where(d => d.EsteElement)
            .Select(d => (d.Id, d.Cod, (Action<Gestiune>)(g => {
                g.Cod = d.Cod;
                g.Denumire = d.Denumire ?? d.Cod;
                g.Activ = !d.Marcat;
            }))).ToList(), avert);

    // Casieriile → `ContPropriu` (decizia 31c: laturile trezoreriei). Contul
    // implicit se alege pe valută, nu se citește din 1C (casieriile n-au cont de
    // evidență în sursă): 5311 casa în lei, 5314 casa în valută.
    public static (int Procesate, int Noi) Casierii(IObjectSpace os,
            IReadOnlyList<FlaxCasierie> sursa, IReadOnlyDictionary<string, Guid> plan,
            Action<string> avert) =>
        Upsert<ContPropriu>(os, "Casierii", sursa
            .Select(c => (c.Id, c.Cod, (Action<ContPropriu>)(e => {
                e.Cod = c.Cod;
                e.Denumire = c.Denumire ?? c.Cod;
                e.EsteBanca = false;
                var simbol = EsteLei(c.Valuta) ? "5311" : "5314";
                if (plan.TryGetValue(simbol, out var id))
                    e.ContImplicitId = id;
                else
                    avert($"Casieria {c.Cod} „{c.Denumire}”: contul {simbol} lipsește din planul "
                        + "OMFP — rămâne fără ContImplicit (contarea PLT/INC va cere cont pe latură).");
            }))).ToList(), avert);

    // Conturile bancare PROPRII → `ContPropriu` cu `EsteBanca` și IBAN. Contul
    // implicit vine din `ContDeEvidenta` acolo unde 1C îl poartă (5 rânduri din
    // 46), altfel din valută (5121 lei / 5124 valută). Un cont de evidență
    // nemapabil sau SUMATOR nu se acceptă tăcut: se avertizează și se cade pe
    // valută — un ContPropriu fără cont ar da eroare abia la operarea plății.
    public static (int Procesate, int Noi) ConturiBancare(IObjectSpace os,
            IReadOnlyList<FlaxContBancar> sursa, IReadOnlyDictionary<string, Guid> plan,
            IReadOnlySet<string> sumatori, Func<string, string> mapeaza1C, Action<string> avert) =>
        Upsert<ContPropriu>(os, "ConturiBancare", sursa
            .Select(c => (c.Id, c.Cod, (Action<ContPropriu>)(e => {
                e.Cod = c.Cod;
                e.Denumire = Denumeste(c);
                e.EsteBanca = true;
                e.Iban = c.Iban;
                e.Activ = !c.Marcat;

                string simbol = null;
                if (c.ContDeEvidenta != null) {
                    simbol = mapeaza1C(c.ContDeEvidenta);
                    if (simbol == null)
                        avert($"Cont bancar {c.Cod}: contul de evidență 1C „{c.ContDeEvidenta}” nu se "
                            + "mapează pe planul OMFP — se cade pe contul de valută.");
                    else if (sumatori.Contains(simbol)) {
                        avert($"Cont bancar {c.Cod}: contul de evidență 1C „{c.ContDeEvidenta}” → {simbol}, "
                            + "care e SUMATOR în OMFP — se cade pe contul de valută.");
                        simbol = null;
                    }
                }
                simbol ??= EsteLei(c.Valuta) ? "5121" : "5124";
                if (plan.TryGetValue(simbol, out var id))
                    e.ContImplicitId = id;
                else
                    avert($"Cont bancar {c.Cod}: contul {simbol} lipsește din planul OMFP "
                        + "— rămâne fără ContImplicit.");
            }))).ToList(), avert);

    // Persoanele fizice → `Angajat` (titularii de avans/decont, decizia 31/32).
    // `Marca` = codul 1C: e singurul identificator stabil din sursă.
    // ATENȚIE: CNP-ul EXISTĂ în sursă dar NU are câmp în modelul Atlas — se
    // pierde la import. Adăugarea lui e pur aditivă (câmp pe `Angajat` +
    // migrație) și se face când apare cerința reală, nu preventiv.
    public static (int Procesate, int Noi) PersoaneFizice(IObjectSpace os,
            IReadOnlyList<FlaxPersoana> sursa, Action<string> avert) =>
        Upsert<Angajat>(os, "PersoaneFizice", sursa.Where(p => p.EsteElement)
            .Select(p => (p.Id, p.Cod, (Action<Angajat>)(a => {
                a.Cod = p.Cod;
                a.Marca = p.Cod;
                a.Denumire = p.Nume ?? p.Cod;
                a.Activ = !p.Marcat;
            }))).ToList(), avert);

    static bool EsteLei(string valuta) =>
        valuta == null || valuta.Equals("Lei", StringComparison.OrdinalIgnoreCase)
        || valuta.Equals("RON", StringComparison.OrdinalIgnoreCase);

    // Denumirea contului bancar: banca + IBAN e forma lizibilă și stabilă (nu
    // depinde de convenția de Description a 1C, care e „IBAN\Valuta la Bancă");
    // valuta intră doar când nu e lei, ca să distingă conturile aceleiași bănci
    // fără IBAN completat (2 rânduri în sursă).
    static string Denumeste(FlaxContBancar c) {
        var parti = new[] {
            c.Banca,
            EsteLei(c.Valuta) ? null : c.Valuta,
            c.Iban,
        }.Where(x => !string.IsNullOrWhiteSpace(x));
        var denumire = string.Join(" ", parti);
        return denumire.Length > 0 ? denumire : c.Denumire ?? c.Cod;
    }
}

// ======================= Nomenclatoarele MARI, la cerere =======================
//
// `Partenerii` (129k) și `Nomenclator` (312k) nu se importă în bloc: baza vie a
// anului 2025 atinge o fracțiune din ele, iar un catalog integral ar fi zgomot
// permanent în UI. Se materializează la prima referință și rămân în cache pe
// durata rulării.
//
// MECANICA DE COMMIT (importantă pentru pasul 3): helper-ul lucrează într-un
// ObjectSpace PROPRIU, nu în cel al apelantului. Motivul e că legătura trebuie
// scrisă DUPĂ commit-ul entității, iar un commit pe ObjectSpace-ul apelantului
// ar comite și documentul pe jumătate construit din jurul apelului. Consecința
// asumată: nomenclatorul creat NU se retrage dacă documentul care l-a cerut
// eșuează — un partener/produs orfan e inofensiv (nomenclator, nu registru), iar
// legătura garantează că a doua rulare îl reutilizează în loc să-l dubleze.
// Apelantul primește un Guid și îl folosește ca FK (deciziile 6/7: modelul
// poartă FK-uri explicite, deci nu are nevoie de entitatea atașată la OS-ul lui).
//
// Recuperarea după întrerupere: dacă rularea moare între commit-ul entității și
// cel al legăturii, entitatea rămâne nelegată. La reluare se caută după codul
// natural 1C și se re-leagă, în loc să se dubleze. ATENȚIE — codul 1C e unic la
// Parteneri (129.329/129.329) dar NU la Nomenclator (312.659 rânduri, 250.018
// coduri distincte): două poziții de catalog pot purta același cod. De aceea
// candidatul la recuperare trebuie să fie NELEGAT — o entitate deja legată de
// alt KeyField e un omonim legitim, nu un rest de rulare întreruptă.
//
// ======================================================================
// IDENTITATEA PRODUSULUI DE IMPORT = NOMENCLATOR 1C × SIMBOL DE CONT
// ======================================================================
// Amendament la 47d („Tipul produsului multi-cont = contul DOMINANT pe
// valoare"), scos de contradicția de registru diagnosticată înainte de 1C-d:
// deschiderea crea loturile per (document × nomenclator × SIMBOL) și le scria
// soldul în registrul simbolului, dar produsul primea UN singur Tip — cel
// dominant. Pentru cele 17 nomenclatoare ținute de 1C pe mai multe conturi de
// stoc, cele două reguli se contraziceau prin construcție: ieșirea căuta soldul
// în registrul Tipului produsului, marfa stătea în registrul lotului, iar
// cantitatea rămânea blocată (35 de loturi / 16 produse, ex. asamblarea
// SED00000002 cu 400 buc neconsumate).
//
// Rezolvarea NU e o a treia regulă în unealtă, ci identitatea corectă: în 1C
// identitatea de stoc E (nomenclator × cont) — exact ce dovedește subconto-ul
// `BalantaNivel3`, unde aceeași poziție de catalog ține solduri separate per
// cont. Modelul Atlas cere un `TipMaterial` per `Produs` (și motorul VALIDEAZĂ
// pe ASM/DSC/RLF/RDC că Tipul liniei = Tipul produsului lotului), deci
// traducerea fidelă e un `Produs` per pereche. Consecințe:
//  * fiecare geamăn are Tipul contului lui ⇒ registrul lotului = registrul
//    Tipului produsului, prin construcție, peste tot;
//  * decizia 47d rămâne NEATINSĂ (cheia de lot avea deja simbolul);
//  * invariantul modelului rămâne neatins (nicio modificare în Module);
//  * logica „contul dominant pe valoare" moare — nu mai are ce alege.
// Grupa FIFO a supapei de import (48a) devine per geamăn: netarea deschiderii a
// conservat sumele per (nomenclator × depozit), deci un deficit al unui geamăn
// NU se poate acoperi din celălalt. Nu se forțează cross-geamăn — dacă apare, e
// diferență raportată (§8.3), nu un fallback tăcut.
class ImportLaCerere {
    readonly IObjectSpaceProvider provider;
    readonly FlaxDb flax;
    readonly Action<string> avert;
    readonly Dictionary<string, Guid> parteneri;
    readonly Dictionary<string, Guid> produse;
    readonly Dictionary<string, Guid> tipuriMaterial;
    // Țintele deja legate (ambele view-uri): filtrul candidaților la recuperare.
    readonly HashSet<Guid> legate;
    // Referințele RESPINSE se memorează, altfel același produs lipsă ar avertiza
    // o dată per poziție (11.762 poziții de stoc pe ~3.000 produse în 2025 —
    // logul ar deveni inutilizabil). Cheia produsului include simbolul de cont:
    // același nomenclator poate fi cerut de pe alt cont, care ARE Tip în profil.
    readonly HashSet<string> respinse = [];

    // View-ul legăturilor produselor + formatul cheii compuse. Separatorul „|" e
    // stabil: hex-urile 1C sunt [0-9A-F]{32}, iar simbolurile OMFP sunt cifre —
    // niciunul nu-l poate conține, deci tăierea la primul „|" e neambiguă.
    // Cheile VECHI (hex simplu, dinaintea amendamentului) nu se migrează: baza de
    // import se reconstruiește cu `--recreeaza`; o cheie fără „|" se citește
    // oricum ca nomenclator, deci traducerea inversă rămâne corectă pe ele.
    public const string ViewProduse = "Nomenclator";

    public static string CheieProdus(string hexId, string simbolContOmfp) =>
        $"{hexId}|{simbolContOmfp}";

    // Traducerea înapoi la identitatea 1C (reconcilierea, §8.3): geamănul se taie
    // la nomenclator, fiindcă acolo compară sursa — `BalantaNivel3` agregat per
    // (nomenclator × depozit), peste conturi.
    public static string NomenclatorDinCheie(string cheie) =>
        cheie == null ? null : cheie.IndexOf('|') is var i && i >= 0 ? cheie[..i] : cheie;

    public int ParteneriNoi { get; private set; }
    public int ProduseNoi { get; private set; }
    public int Recuperate { get; private set; }
    public int ReferinteMoarte { get; private set; }
    public int ProduseFaraTip { get; private set; }

    public ImportLaCerere(IObjectSpaceProvider provider, FlaxDb flax, Action<string> avert) {
        this.provider = provider;
        this.flax = flax;
        this.avert = avert;
        using var os = provider.CreateObjectSpace();
        parteneri = Legaturi.Incarca(os, "Partenerii");
        produse = Legaturi.Incarca(os, ViewProduse);
        tipuriMaterial = os.GetObjectsQuery<TipMaterial>().ToDictionary(t => t.Cod, t => t.ID);
        legate = [.. parteneri.Values, .. produse.Values];
    }

    // Partenerii deja legați (rulare anterioară) își primesc câmpurile fiscale
    // o dată în rularea asta — nu se rescriu la fiecare referință.
    readonly HashSet<string> clasificati = [];

    public Guid? AsiguraPartener(string hexId) {
        if (string.IsNullOrEmpty(hexId))
            return null;
        if (parteneri.TryGetValue(hexId, out var existent)) {
            ReclasificaExistent(hexId, existent);
            return existent;
        }
        if (respinse.Contains($"P|{hexId}"))
            return null;

        var p = flax.PartenerDupaId(hexId);
        if (p == null) {
            respinse.Add($"P|{hexId}");
            ReferinteMoarte++;
            avert($"Partener 1C {hexId} nu există în flax.Partenerii — referință moartă, sărită.");
            return null;
        }
        var cod = p.Cod ?? hexId;
        var (id, nou) = Materializeaza<Partener>("Partenerii", hexId,
            os => os.GetObjectsQuery<Partener>().Where(x => x.Cod == cod).ToList(),
            e => {
                e.Cod = cod;
                e.Denumire = p.Denumire ?? cod;
                e.CodFiscal = p.CodUnic;
                e.RegistruComert = p.RegCom;
                AplicaClasificare(e, p);
                AplicaAdresa(e, flax.AdresaPartener(hexId));
            });
        parteneri[hexId] = id;
        clasificati.Add(hexId);
        if (nou)
            ParteneriNoi++;
        return id;
    }

    // Idempotența câmpurilor fiscale (felia D394): un partener materializat de o
    // rulare de dinaintea clasificării e legat, deci `Materializeaza` nu-l mai
    // atinge — câmpurile noi i se scriu aici, o dată per rulare, într-un OS
    // propriu (aceeași mecanică de commit ca materializarea). Cod/Denumire/
    // RegistruComert NU se rescriu — clasificarea nu e o re-materializare;
    // `CodFiscal` se atinge doar la PF cu CNP (D4-D1).
    void ReclasificaExistent(string hexId, Guid id) {
        if (!clasificati.Add(hexId))
            return;
        var p = flax.PartenerDupaId(hexId);
        if (p == null)
            return;
        using var os = provider.CreateObjectSpace();
        var e = os.GetObjectByKey<Partener>(id);
        if (e == null)
            return;
        AplicaClasificare(e, p);
        AplicaAdresa(e, flax.AdresaPartener(hexId));
        os.CommitChanges();
    }

    // ---------------- Clasificarea fiscală a partenerului (D4-D1) ----------------
    //
    // Ce spune sursa, verificat pe cei 129.329 de parteneri din `flax`:
    //  * `PersJurFiz` e completat la toți în afară de 12 ⇒ `TipPersoana` din sursă;
    //    unde tace, se DERIVĂ (CNP/CodUnic de 13 cifre ⇒ fizică).
    //  * `CNP` e completat doar la persoane fizice (17.862, din care 11.433 cu 13
    //    cifre curate); `CodUnic` la fizice e gol (70.983/90.715) ⇒ PF cu CNP
    //    poartă CNP-ul în `CodFiscal`, PJ păstrează `CodUnic` (neschimbat).
    //  * `PoliticaTVA` = „TVAlaEmitere" e VALOAREA IMPLICITĂ (118.772, inclusiv
    //    persoane fizice), deci NU e un semnal de înregistrare în scopuri de TVA;
    //    doar „TVAlaIncasare" (79) spune ceva: plătitor, în sistemul la încasare.
    //  * `DataLuariiInEvidentaTVA` e vidă la TOȚI (129.322 × 2001-01-01) — sursa
    //    tace ⇒ `InregistratTva` se DERIVĂ din prefixul „RO" al CUI-ului (22.409).
    //  * `Tara` e completată la 251 (restul null ⇒ RO); `CodAlfa2` din `Tari` e
    //    de obicei ISO, dar catalogul are și gunoi („NY", „TX", „NSW", „752") —
    //    codul ne-ISO se RAPORTEAZĂ, iar partenerul rămâne pe RO. 82 nerezidenți
    //    și 32 intracomunitari n-au țară deloc — tot raportate.
    //  * `NuIncludeInDec394` (281) și `Nerezident`/`Intracomunitar` n-au câmp în
    //    model (D4-D1 ține doar cele patru date ale clasificării D394): se
    //    NUMĂRĂ, ca inventar al sursei, nu se pierd tăcut.
    public int ParteneriClasificati { get; private set; }
    public int PfaInregistrate { get; private set; }
    public int ReclasificatiDinSursa { get; private set; }
    public int InregistratiDinRegistru { get; private set; }
    // Partenerii cu timbru ANAF pe care clasificarea din sursă NU i-a atins pe
    // axa TVA (canonicul bate evidența) și, separat, cei la care semnalul din
    // registru (TVA ≠ 0 pe achiziții) contrazice registrul ANAF de azi — se
    // RAPORTEAZĂ, nu se rescriu (statutul la data documentului = D4-r1).
    public int PastratiDinAnaf { get; private set; }
    public int RegistruContraAnaf { get; private set; }
    public int ParteneriLegati => parteneri.Count;
    // Candidații fazei `--anaf` (D15-D6): partenerii LEGAȚI de o fișă 1C — nu
    // toți partenerii bazei. Un partener creat de seed sau de mână nu vine din
    // import și nu e treaba conectorului să-l atingă.
    public IReadOnlyCollection<Guid> IdParteneriLegati => parteneri.Values;
    public int TipPersoanaDerivat { get; private set; }
    public int InregistratTvaDerivat { get; private set; }
    public int TvaLaIncasareDinSursa { get; private set; }
    public int TaraNerezolvata { get; private set; }
    public int NuIncludeInDec394 { get; private set; }
    public int NerezidentiSursa { get; private set; }

    static bool TreisprezeceCifre(string s) => s != null && s.Length == 13 && s.All(char.IsDigit);

    void AplicaClasificare(Partener e, FlaxPartener p) {
        ParteneriClasificati++;

        // Tip persoană: sursa, apoi derivarea.
        switch (p.PersJurFiz) {
            case "PersJur": e.TipPersoana = TipPersoana.Juridica; break;
            case "PersFiz": e.TipPersoana = TipPersoana.Fizica; break;
            default:
                TipPersoanaDerivat++;
                e.TipPersoana = TreisprezeceCifre(p.Cnp) || TreisprezeceCifre(p.CodUnic)
                    ? TipPersoana.Fizica : TipPersoana.Juridica;
                break;
        }

        // PF: identificatorul fiscal e CUI-ul RO când există (PFA/II ÎNREGISTRATĂ
        // în scopuri de TVA — fix 2 al review-ului D394: se declară pe tip 1 cu
        // CUI, nu pe tip 2 cu CNP), altfel CNP-ul (D4-D1). PJ = CodUnic, ca
        // înainte (setat de apelant, nu se atinge aici).
        if (e.TipPersoana == TipPersoana.Fizica) {
            if (p.CodUnic != null && p.CodUnic.Trim().StartsWith("RO", StringComparison.OrdinalIgnoreCase)) {
                e.CodFiscal = p.CodUnic;
                PfaInregistrate++;
            }
            else if (p.Cnp != null)
                e.CodFiscal = p.Cnp;
        }

        // Țara: ISO-2 din catalog; „UK" e grafia curentă a lui GB, restul ne-ISO
        // se raportează. Fără țară = RO (default-ul modelului), dar un nerezident
        // fără țară e o gaură a sursei, nu o certitudine.
        var iso = p.TaraIso?.Trim().ToUpperInvariant() switch {
            null or "" => null,
            "UK" => "GB",
            var c => c,
        };
        if (iso != null && iso.Length == 2 && iso.All(char.IsAsciiLetterUpper))
            e.Tara = iso;
        else {
            e.Tara = "RO";
            if (iso != null) {
                TaraNerezolvata++;
                avert($"Partener {p.Cod} ({p.Denumire}): țara 1C „{p.TaraIso}” nu e cod ISO-2 — lăsat pe RO.");
            }
            else if (p.Nerezident || p.Intracomunitar) {
                TaraNerezolvata++;
                avert($"Partener {p.Cod} ({p.Denumire}): {(p.Intracomunitar ? "intracomunitar" : "nerezident")} "
                    + "în 1C, dar fără țară — lăsat pe RO.");
            }
        }

        if (p.NuIncludeInDec394)
            NuIncludeInDec394++;
        if (p.Nerezident || p.Intracomunitar)
            NerezidentiSursa++;

        // Axa TVA: dacă partenerul poartă timbrul ANAF, registrul ANAF a vorbit
        // deja și e CANONICUL (D4-r1, D15-D3) — derivarea din prefixul „RO" și
        // politica 1C sunt evidență și nu au voie să-l răstoarne. Fără gardul
        // ăsta, `--reclasifica` de după `--anaf` (sau pasul final al rulării
        // următoare) ar de-înregistra înapoi partenerii pe care ANAF i-a corectat.
        // Se numără, ca raportul să spună cât din sursă a fost lăsat deoparte.
        if (e.DataSincronizareAnaf != null) {
            PastratiDinAnaf++;
            return;
        }

        // Înregistrarea în scopuri de TVA: singurul semnal al sursei e politica
        // „la încasare" (implică plătitor) sau o dată de luare în evidență
        // nevidă; altfel derivare din prefixul RO al CUI-ului.
        e.TvaLaIncasare = p.PoliticaTva == "TVAlaIncasare";
        if (e.TvaLaIncasare || p.DataTva != null) {
            e.InregistratTva = true;
            if (e.TvaLaIncasare)
                TvaLaIncasareDinSursa++;
        }
        else {
            InregistratTvaDerivat++;
            e.InregistratTva = p.CodUnic != null
                && p.CodUnic.StartsWith("RO", StringComparison.OrdinalIgnoreCase);
        }
    }

    // ---------------- Adresa structurată a partenerului (felia 15, D15-D6) ----------------
    //
    // Simetric cu `AplicaClasificare`, cu o singură lege în plus, și e cea care
    // contează: se scrie DOAR PE BLOC GOL. O adresă culeasă de om sau adusă din
    // registrul ANAF (`--anaf`) e mai bună decât una din 1C — 1C n-are decât ce
    // i-a introdus cineva acum ani, fără validare. „Bloc" înseamnă toate cele
    // șase câmpuri deodată: umplerea câmp-cu-câmp ar amesteca strada ANAF cu
    // localitatea 1C și ar produce o adresă care n-a existat nicăieri.
    //
    // Județul are DOUĂ chei în sursă, în ordinea asta:
    //  1. `CodJudet` — codul din CNP (un NUMĂR, deci fără ambiguitate); e
    //     completat pe 57.336 din 148.443 de rânduri;
    //  2. `Field3` — DENUMIREA, text liber („DAMBOVITA", „Judetul Bistrita -
    //     Nasaud"); `JudeteRo.DupaDenumire` o normalizează, dar tot o ghicire
    //     după formă rămâne.
    // Nerezolvat ⇒ nu se pune nimic (nicio ghicire, D15-D1) și denumirea brută
    // intră în `DetaliiAdresa`: informația SURSEI nu se pierde, doar nu se
    // preface în FK. Contorizat separat, ca să se vadă cât ne costă.
    public int AdresePreluate { get; private set; }
    public int FaraAdresaInSursa { get; private set; }
    // Blocul era deja completat (rulare anterioară, culegere de om, `--anaf`) ⇒
    // nu s-a atins. Contorul EXISTĂ ca raportul unei re-rulări să nu citească
    // „0 preluate" ca eșec: pe o bază deja importată zero e răspunsul CORECT, iar
    // fără cifra asta nu s-ar putea deosebi de „sursa n-are adrese".
    public int AdreseDejaCompletate { get; private set; }
    public int JudetDinCodCnp { get; private set; }
    public int JudetDinDenumire { get; private set; }
    public int JudetNerezolvat { get; private set; }
    public int AdreseTrunchiate { get; private set; }

    // Lungimile coloanelor, citite din MODEL prin reflecție — aceeași sursă ca
    // `SincronizareAnafService.Lungimi` și din același motiv: `[MaxLength]` de pe
    // `Partener` E lungimea SAF-T, iar o a doua listă scrisă cu mâna în conector
    // ar fi exact locul unde apare deriva. O adresă peste lungime nu se refuză
    // (importul n-are voie să pice pentru o stradă lungă) — se taie, cu contor.
    static readonly Dictionary<string, int> LungimiAdresa = typeof(Partener)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(pi => (pi.Name, Lungime: pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0))
        .Where(x => x.Lungime > 0)
        .ToDictionary(x => x.Name, x => x.Lungime, StringComparer.Ordinal);

    // Nomenclatorul de județe, citit O DATĂ pe rulare ca `cod ISO → ID`. Se
    // scrie doar FK-ul (`JudetId`), ca peste tot în conector (`ContImplicitId`,
    // `TipMaterialId`): un Guid n-are context, deci nu poate ajunge entitate a
    // altui ObjectSpace decât cel în care se comite. Navigația ar fi necesară
    // doar pentru gardianul XAF, care nu rulează aici (ușa non-secured).
    // 42 de rânduri — se încarcă la prima adresă, nu la construcția clasei:
    // o rulare care nu atinge niciun partener nu deschide un OS degeaba.
    Dictionary<string, Guid> judete;

    Guid? CautaJudet(string cod) {
        if (judete == null) {
            using var os = provider.CreateObjectSpace();
            judete = os.GetObjectsQuery<Judet>().Select(j => new { j.Cod, j.ID })
                .ToDictionary(j => j.Cod, j => j.ID, StringComparer.Ordinal);
        }
        return judete.TryGetValue(cod, out var id) ? id : null;
    }

    public void AplicaAdresa(Partener e, FlaxAdresa a) {
        // Un rând de adresă care n-are NICIO coloană structurată (doar `Present`,
        // ex. „Targoviste, Dambovita") nu e o adresă, e o notă: 1C are 148.443 de
        // rânduri de tip „Adresa", dar 141.641 cu localitate. Se numără la fel ca
        // absența — „n-avem adresă" e același fapt, indiferent cum arată rândul.
        if (a == null || (a.CodPostal == null && a.Localitate == null && a.Strada == null
                && a.Numar == null && a.Cladire == null
                && a.JudetDenumire == null && a.CodJudetCnp == null)) {
            FaraAdresaInSursa++;
            return;
        }

        // BLOC GOL: nimic cules și niciun județ. Verificat ÎNAINTE de orice
        // rezolvare de județ — pe un partener deja completat n-avem ce face cu
        // răspunsul, iar contoarele de județ ar număra o muncă neaplicată.
        if (e.Strada != null || e.Numar != null || e.DetaliiAdresa != null
                || e.Localitate != null || e.CodPostal != null || e.JudetId != null) {
            AdreseDejaCompletate++;
            return;
        }

        // Județul: codul din CNP întâi (cheie), denumirea pe urmă (formă).
        string codIso = null;
        if (a.CodJudetCnp != null)
            codIso = JudeteRo.DupaCodCnp(a.CodJudetCnp.Value);
        var dinCodCnp = codIso != null;
        codIso ??= JudetDinDenumire1C(a.JudetDenumire);

        var judet = codIso == null ? null : CautaJudet(codIso);
        if (judet != null) {
            e.JudetId = judet;
            if (dinCodCnp)
                JudetDinCodCnp++;
            else
                JudetDinDenumire++;
        }
        else if (a.JudetDenumire != null || a.CodJudetCnp != null)
            JudetNerezolvat++;

        // `DetaliiAdresa` adună ce n-are coloană proprie: bloc/scară/etaj/ap.
        // (SAF-T ține `Building` separat, modelul o pliază — D15-D1) și, când
        // județul n-a putut fi rezolvat, denumirea brută din sursă. Concatenare
        // simplă cu „, ": nu inventăm o gramatică de adresă.
        var detalii = DetaliiDinPrezentare(a);
        if (judet == null && a.JudetDenumire != null)
            detalii.Add(a.JudetDenumire);

        e.Strada = Taie(nameof(Partener.Strada), a.Strada);
        e.Numar = Taie(nameof(Partener.Numar), a.Numar);
        e.Localitate = Taie(nameof(Partener.Localitate), a.Localitate);
        e.CodPostal = Taie(nameof(Partener.CodPostal), a.CodPostal);
        e.DetaliiAdresa = Taie(nameof(Partener.DetaliiAdresa),
            detalii.Count == 0 ? null : string.Join(", ", detalii));
        AdresePreluate++;

        string Taie(string camp, string valoare) {
            if (valoare == null || !LungimiAdresa.TryGetValue(camp, out var maxim)
                    || valoare.Length <= maxim)
                return valoare;
            AdreseTrunchiate++;
            return valoare[..maxim];
        }
    }

    // Grafiile PROPRII sursei 1C pentru județ, peste normalizarea generală din
    // `JudeteRo.DupaDenumire` (care rămâne pură și nu știe de ele — „Sector 3"
    // dă null acolo, deliberat: sectorul nu e subdiviziune ISO):
    //  * „Sector 1"…„Sector 6", „SECT 1", „SECTOR1" ⇒ București (toate
    //    sectoarele sunt aceeași subdiviziune ISO, ca și CNP-ul 41–46);
    //  * prefixul „Jud." / „JUD " (Field3 = „Jud. Iasi", „JUD.BRASOV").
    // Nimic altceva — fără potrivire fuzzy: o denumire nerezolvată rămâne în
    // `DetaliiAdresa`, nu devine un județ ghicit.
    internal static string JudetDinDenumire1C(string denumire) {
        if (string.IsNullOrWhiteSpace(denumire))
            return null;
        var text = denumire.Trim();
        if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^SECT(OR)?\.?\s*[1-6]$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            return "RO-B";
        var cod = JudeteRo.DupaDenumire(text);
        if (cod != null)
            return cod;
        var faraPrefix = System.Text.RegularExpressions.Regex.Replace(text, @"^JUD\.?\s*", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return faraPrefix.Length == text.Length ? null : JudeteRo.DupaDenumire(faraPrefix);
    }

    // Detaliile de adresă: 1C ține etajul și apartamentul DOAR în `Present`
    // (forma concatenată „cod, județ, localitate, stradă, Nr, număr, bl. X,
    // sc. Y, et. Z, ap. W"), nu în coloanele numerotate — `Field8` are doar
    // blocul. Se iau segmentele cu prefixele bl./sc./et./ap. exact cum le-a
    // scris 1C; dacă `Present` nu le are, rămâne clădirea din `Field8`.
    internal static List<string> DetaliiDinPrezentare(FlaxAdresa a) {
        var detalii = new List<string>();
        if (a.Prezentare != null)
            foreach (var segment in a.Prezentare.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                if (System.Text.RegularExpressions.Regex.IsMatch(segment, @"^(bl|sc|et|ap)\.\s*\S",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    detalii.Add(segment);
        if (detalii.Count == 0 && a.Cladire != null)
            detalii.Add(a.Cladire);
        return detalii;
    }

    // ---------------- Reclasificarea TUTUROR partenerilor legați ----------------
    //
    // Fix 4 al review-ului advers D394 (constatarea cu cifre: 16.030 PJ din sursă
    // au CUI fără prefix „RO", iar derivarea din prefix îi făcea tip 2 ⇒
    // achiziții pe cartușul D, combinație pe care formularul o refuză). Doi pași,
    // în ordinea asta:
    //  1. SURSA: `AplicaClasificare` pe fiecare partener legat (nu doar pe cei
    //     referiți în rularea curentă — cosmetica 13 a review-ului), sărind pe
    //     cei clasificați deja în rularea asta (idempotent, aceeași sursă);
    //  2. REGISTRUL: partenerii care apar ca `PartenerId` pe rânduri `RegistruTva`
    //     cu `Sens = Achizitie` și `Tva ≠ 0` (A sau C cu TVA deductibilă) sunt
    //     ÎNREGISTRAȚI în scopuri de TVA — un furnizor care ne-a facturat TVA e
    //     înregistrat; evidența bate eticheta (34f). Contorizat separat.
    // Rulează ca pas final al importului normal ȘI ca mod propriu `--reclasifica`
    // (fără import de documente). Single-operator pe bază, ca toată unealta.
    // Canonicul (registrul ANAF al plătitorilor) rămâne restanță cu nume.
    public void ReclasificaToti() {
        // Adresele se citesc ÎN LOT, o interogare per tranșă de 1.000 (D15-D6):
        // pe cei ~20.000 de parteneri legați, un query per partener ar fi 20.000
        // de dus-întorsuri pentru date pe care sursa le dă din trei mișcări.
        var adrese = flax.AdreseParteneri(parteneri.Keys);
        foreach (var lot in parteneri.Chunk(500)) {
            using var os = provider.CreateObjectSpace();
            foreach (var (hexId, id) in lot) {
                if (clasificati.Contains(hexId))
                    continue;
                var e = os.GetObjectByKey<Partener>(id);
                if (e == null)
                    continue;
                var p = flax.PartenerDupaId(hexId);
                if (p == null)
                    continue;
                AplicaClasificare(e, p);
                AplicaAdresa(e, adrese.GetValueOrDefault(hexId));
                clasificati.Add(hexId);
                ReclasificatiDinSursa++;
            }
            os.CommitChanges();
        }
        using (var os = provider.CreateObjectSpace()) {
            var ids = os.GetObjectsQuery<RegistruTva>()
                .Where(r => r.Sens == SensTva.Achizitie && r.Tva != 0m && r.PartenerId != null)
                .Select(r => r.PartenerId.Value)
                .Distinct()
                .ToList();
            var neinregistrati = os.GetObjectsQuery<Partener>()
                .Where(p => ids.Contains(p.ID) && !p.InregistratTva)
                .ToList();
            // Un partener sincronizat cu ANAF care spune „neînregistrat" AZI, dar
            // ne-a facturat TVA în anul importat: canonicul rămâne ANAF (D15-D3);
            // contradicția e chiar cazul D4-r1 (statutul la data documentului) și
            // se numără, nu se ascunde.
            var deMarcat = neinregistrati.Where(p => p.DataSincronizareAnaf == null).ToList();
            RegistruContraAnaf = neinregistrati.Count - deMarcat.Count;
            foreach (var p in deMarcat)
                p.InregistratTva = true;
            InregistratiDinRegistru = deMarcat.Count;
            os.CommitChanges();
        }
    }

    // `simbolContOmfp` vine din contul pe care stă poziția (ex. 371.1 → „371"):
    // Clasă/Tip e echivalentul operațional al planului, iar Cod-ul Tipului E
    // simbolul de cont (decizia 26b). Un simbol fără Tip în profil e o GAURĂ DE
    // PROFIL, nu un produs de creat mut: se raportează și se întoarce null, iar
    // apelantul decide (decizia 21 — fiecare gaură = decizie explicită).
    //
    // Simbolul e ACUM parte din identitate (vezi nota clasei), nu doar sursa
    // Tipului: același nomenclator cerut de pe două conturi dă două produse.
    // Apelanții îl aleg deja coerent cu cheia lotului — simbolul LOTULUI acolo
    // unde linia vine cu lot (altfel produsul geamăn n-ar fi cel al lotului și
    // motorul ar refuza linia), contul rândului/secțiunii-sursă unde nu există
    // lot (intrări, plusuri, produse de asamblare).
    public Guid? AsiguraProdus(string hexId, string simbolContOmfp) {
        if (string.IsNullOrEmpty(hexId))
            return null;
        var cheie = CheieProdus(hexId, simbolContOmfp);
        if (produse.TryGetValue(cheie, out var existent))
            return existent;
        var cheieRespins = $"N|{cheie}";
        if (respinse.Contains(cheieRespins))
            return null;

        var n = flax.NomenclatorDupaId(hexId);
        if (n == null) {
            respinse.Add(cheieRespins);
            ReferinteMoarte++;
            avert($"Nomenclator 1C {hexId} nu există în flax.Nomenclator — referință moartă, sărită.");
            return null;
        }
        if (simbolContOmfp == null || !tipuriMaterial.TryGetValue(simbolContOmfp, out var tipId)) {
            respinse.Add(cheieRespins);
            ProduseFaraTip++;
            avert($"Nomenclator 1C {n.Cod} „{n.Denumire}”: profilul privat nu are TipMaterial cu codul "
                + $"„{simbolContOmfp ?? "(niciun cont)"}” — produsul NU se creează (gaură de profil).");
            return null;
        }
        // Cod-ul poartă simbolul, la TOATE produsele importate (nu doar la gemeni):
        // e ancora recuperării unei rulări întrerupte, iar un sufix pus abia când
        // apare al doilea geamăn ar însemna redenumirea primului — adică exact
        // ancora care trebuie să fie stabilă. Denumirea rămâne a nomenclatorului:
        // gemenii se disting prin Cod și Tip, nu prin nume.
        var cod = $"{n.Cod ?? hexId}/{simbolContOmfp}";
        var (id, nou) = Materializeaza<Produs>(ViewProduse, cheie,
            os => os.GetObjectsQuery<Produs>().Where(x => x.Cod == cod).ToList(),
            e => {
                e.Cod = cod;
                e.Denumire = n.Denumire ?? cod;
                e.UM = n.UM;
                e.TipMaterialId = tipId;
            });
        produse[cheie] = id;
        if (nou)
            ProduseNoi++;
        return id;
    }

    (Guid Id, bool Nou) Materializeaza<T>(string view, string cheie,
            Func<IObjectSpace, List<T>> cautaDupaCod, Action<T> aplica) where T : BaseObject {
        using var os = provider.CreateObjectSpace();
        // Candidat la recuperare = entitate cu același cod natural care nu e
        // legată de niciun KeyField 1C. O entitate legată e omonimul legitim al
        // altei poziții de catalog (codurile Nomenclator nu sunt unice) — nu se
        // atinge; una NElegată nu poate proveni decât dintr-o rulare întreruptă
        // între cele două commit-uri de mai jos.
        var recuperat = cautaDupaCod(os).FirstOrDefault(x => !legate.Contains(x.ID));
        if (recuperat != null) {
            Recuperate++;
            // Atributele se RE-aplică pe entitatea adoptată (fix review 1C-b):
            // candidatul nelegat poate fi restul unei rulări întrerupte pentru un
            // OMONIM (același Cod, alt KeyField) — fără re-aplicare, adopția ar
            // încrucișa tăcut Denumire/UM/TipMaterial între cele două poziții.
            aplica(recuperat);
            Legaturi.Leaga(os, view, cheie, recuperat.ID);
            os.CommitChanges();
            legate.Add(recuperat.ID);
            return (recuperat.ID, false);
        }
        var e = os.CreateObject<T>();
        aplica(e);
        os.CommitChanges();
        Legaturi.Leaga(os, view, cheie, e.ID);
        os.CommitChanges();
        legate.Add(e.ID);
        return (e.ID, true);
    }
}
