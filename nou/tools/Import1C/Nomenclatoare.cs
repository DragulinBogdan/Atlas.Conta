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
            IReadOnlyList<(string Cheie, Action<T> Aplica)> randuri, Action<string> avert)
            where T : BaseObject {
        var tabela = Legaturi.Tabela(view);
        var legaturi = Legaturi.Incarca(os, view);
        var deLegat = new List<(string Cheie, T Entitate)>();

        foreach (var (cheie, aplica) in randuri) {
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
            .Select(d => (d.Id, (Action<Gestiune>)(g => {
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
            .Select(c => (c.Id, (Action<ContPropriu>)(e => {
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
            .Select(c => (c.Id, (Action<ContPropriu>)(e => {
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
            .Select(p => (p.Id, (Action<Angajat>)(a => {
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
        produse = Legaturi.Incarca(os, "Nomenclator");
        tipuriMaterial = os.GetObjectsQuery<TipMaterial>().ToDictionary(t => t.Cod, t => t.ID);
        legate = [.. parteneri.Values, .. produse.Values];
    }

    public Guid? AsiguraPartener(string hexId) {
        if (string.IsNullOrEmpty(hexId))
            return null;
        if (parteneri.TryGetValue(hexId, out var existent))
            return existent;
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
            });
        parteneri[hexId] = id;
        if (nou)
            ParteneriNoi++;
        return id;
    }

    // `simbolContOmfp` vine din contul pe care stă poziția (ex. 371.1 → „371"):
    // Clasă/Tip e echivalentul operațional al planului, iar Cod-ul Tipului E
    // simbolul de cont (decizia 26b). Un simbol fără Tip în profil e o GAURĂ DE
    // PROFIL, nu un produs de creat mut: se raportează și se întoarce null, iar
    // apelantul decide (decizia 21 — fiecare gaură = decizie explicită).
    public Guid? AsiguraProdus(string hexId, string simbolContOmfp) {
        if (string.IsNullOrEmpty(hexId))
            return null;
        if (produse.TryGetValue(hexId, out var existent))
            return existent;
        var cheieRespins = $"N|{hexId}|{simbolContOmfp}";
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
        var cod = n.Cod ?? hexId;
        var (id, nou) = Materializeaza<Produs>("Nomenclator", hexId,
            os => os.GetObjectsQuery<Produs>().Where(x => x.Cod == cod).ToList(),
            e => {
                e.Cod = cod;
                e.Denumire = n.Denumire ?? cod;
                e.UM = n.UM;
                e.TipMaterialId = tipId;
            });
        produse[hexId] = id;
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
