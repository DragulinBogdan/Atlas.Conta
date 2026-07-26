using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// LOTUL DE ROBUSTEȚE PRE-1C-d, pasul 4: REGISTRUL DIVERGENȚELOR CUNOSCUTE.
//
// Defectul D6 (49f) are o singură rădăcină: unealta ȘTIE, în momentul în care o
// produce, fiecare diferență dintre ce spune sursa și ce scrie în Atlas — dar o
// uită imediat, iar contractul lunar o redescoperă mai târziu ca pe o anomalie
// anonimă, pe care o clasifică prin EURISTICI („încape în negativul sursei",
// „e sub plafonul cumulat al netării"). Euristica minte în ambele direcții: un
// document de stoc pierdut cu totul trece neobservat (perechea 6xx/3xx are suma
// zero, deci „bucket-ul se închide"), iar o linie aruncată legitim rămâne
// nejustificată fiindcă artefactul sursei care a cauzat-o nu se mai vede la
// finele lunii (celula negativă tranzitorie a lui S24 Ultra).
//
// Registrul mută justificarea de la EURISTICĂ la MĂSURĂTOARE: la locul fiecărui
// avertisment existent se înregistrează CE anume nu s-a putut face — pe ce
// produs, în ce gestiune, ce cantitate, ce valoare are în sursă și pe ce pereche
// de conturi. Contractul (3) nu mai întreabă „ar putea diferența asta să vină de
// la un artefact al sursei?", ci „este diferența EGALĂ cu suma liniilor pe care
// le-am aruncat chiar eu pe cheia asta?". O diferență fără înregistrare
// corespondentă e eșec, oricât de plauzibilă ar arăta.
//
// PERSISTENȚA (cerința de determinism, D): verdictul trebuie să fie identic
// între rularea care SCRIE și cea care doar recitește. Divergențele se produc în
// PLANIFICARE, iar planificarea nu se mai face pentru documentele deja importate
// — deci un registru ținut doar în memorie ar fi gol la reluare și ar declara
// nejustificate exact diferențele pe care rularea anterioară le-a produs
// deliberat. Se persistă, ca tot restul evidenței importului, în
// `MigrareLegatura` cu tabela „1C:Divergenta" (chei sintetice, `TintaId` gol —
// rândurile nu trimit la o entitate, sunt fapte despre o lună). Nicio schimbare
// de model.
//
// IDEMPOTENȚA: cheia rândului conține ȘI valorile, iar indexul din memorie e
// ținut pe IDENTITATE (lună × sursă × categorie × cheie de stoc × pereche de
// conturi). La replanificarea unui document (D4: draft șters și reimportat)
// rândurile lui vechi se dau înapoi întregi și se rescriu — nu se adună peste
// ele. Un document șters și NEreimportat își lasă rândurile în urmă; asta nu
// poate ascunde nimic, fiindcă justificarea e o EGALITATE: o înregistrare rămasă
// fără fapt face diferența să NU mai bată, deci pică zgomotos.

// Efectul unei divergențe asupra unei chei a contractului (3). O linie aruncată
// poate atinge DOUĂ chei (transferul: marfa rămâne la expeditor ȘI nu ajunge la
// destinatar), deci se dau ca listă. Semnul e mereu „cât are Atlas ÎN PLUS față
// de sursă" pe cheia aia — aceeași convenție ca Δ-ul contractului.
sealed record EfectStoc(string ProdusHex, string DepozitHex, decimal Cantitate, decimal Valoare);

// O înregistrare a registrului: un fapt măsurat, atribuit lunii în care s-a
// întâmplat. `ValoareNepostata` = partea din rândul 1C pe care Atlas NU o
// postează deloc, cu semnul sursei (0 când puntea a transcris-o — atunci
// contabilitatea nu divergează, doar stocul). `ContCredit` poate fi null: o
// punte dezechilibrată lasă un rest per CONT, nu per pereche.
sealed record Divergenta(int An, int Luna, string Sursa, string Categorie,
    string ProdusHex, string DepozitHex, decimal Cantitate, decimal Valoare,
    string ContDebit, string ContCredit, decimal ValoareNepostata) {

    // Identitatea sub care se agregă (mai multe rânduri ale aceluiași document pe
    // aceeași cheie se adună într-o singură înregistrare — proveniența rămâne la
    // nivel de document, ca raportul să nu explodeze).
    public string Identitate => string.Join('\u0001',
        An, Luna, Sursa, Categorie, ProdusHex ?? "", DepozitHex ?? "",
        ContDebit ?? "", ContCredit ?? "");

    public Divergenta Aduna(Divergenta alta) => this with {
        Cantitate = Cantitate + alta.Cantitate,
        Valoare = Valoare + alta.Valoare,
        ValoareNepostata = ValoareNepostata + alta.ValoareNepostata,
    };
}

sealed class RegistruDivergente {
    public const string View = "Divergenta";
    const string Versiune = "D1";
    const char Sep = '|';

    // Starea COMISĂ (încărcată din bază + confirmată în rularea asta).
    readonly Dictionary<string, Divergenta> toate = new(StringComparer.Ordinal);
    readonly Dictionary<string, string> cheiPersistate = new(StringComparer.Ordinal);
    readonly Dictionary<string, HashSet<string>> peSursa = new(StringComparer.Ordinal);

    // Ce s-a DECIS în planificarea unității curente, dar încă nu s-a scris:
    // planificarea rulează într-un ObjectSpace care se aruncă (e o interogare de
    // solduri), exact ca marcajele supapei de alocare — evidența se scrie în
    // ObjectSpace-ul care se comite, al documentului.
    readonly Dictionary<string, Divergenta> inAsteptare = new(StringComparer.Ordinal);

    // Sursele ale căror rânduri au fost deja rescrise în rularea asta: la primul
    // flush al unei surse rândurile ei vechi se dau înapoi (replanificare), la
    // următoarele se adună (un document care se materializează în mai multe
    // commit-uri — bonul de consum spart pe gestiuni).
    readonly HashSet<string> surseScrise = new(StringComparer.Ordinal);

    // Mutațiile pregătite în ObjectSpace-ul apelantului, aplicate în memorie abia
    // după commit (`Confirma`): un commit picat n-are voie să lase registrul
    // „scris" doar în RAM. `Divergenta == null` = rând care doar pleacă.
    List<(string Identitate, Divergenta Divergenta, string CheieNoua)> tranzactie;

    public int Inregistrari => toate.Count;
    public int Abandonate { get; private set; }

    // ======================= Încărcare =======================

    public void Incarca(IObjectSpace os, Action<string> avert) {
        foreach (var cheie in Legaturi.Incarca(os, View).Keys) {
            var d = Decodeaza(cheie);
            if (d == null) {
                avert($"Registrul divergențelor: rândul „{cheie}” nu se poate citi (format necunoscut) "
                    + "— ignorat; contractul lunii lui va raporta diferența ca nejustificată.");
                continue;
            }
            Adopta(d.Identitate, d, cheie);
        }
    }

    void Adopta(string identitate, Divergenta d, string cheie) {
        toate[identitate] = d;
        cheiPersistate[identitate] = cheie;
        if (!peSursa.TryGetValue(d.Sursa, out var set))
            peSursa[d.Sursa] = set = new HashSet<string>(StringComparer.Ordinal);
        set.Add(identitate);
    }

    void Uita(string identitate) {
        if (toate.Remove(identitate, out var d) && peSursa.TryGetValue(d.Sursa, out var set))
            set.Remove(identitate);
        cheiPersistate.Remove(identitate);
    }

    // Cheia de SURSĂ a unei chei de import: artefactele unui document poartă
    // sufixele handlerului („#punte", „#dsc@<depozit>", „@<depozit>"), dar
    // registrul le adună pe toate sub documentul care le-a produs — replanificarea
    // lui dă înapoi tot ce a înregistrat.
    public static string Sursa(string view, string cheie) =>
        $"{view}/{(cheie.IndexOfAny(['#', '@']) is var i && i > 0 ? cheie[..i] : cheie)}";

    // Replanificarea unui document (D4: draft șters și reimportat): rândurile lui
    // pleacă ÎNAINTE ca planificarea nouă să-și scrie ale ei. Fără asta, un
    // document care nu mai produce nicio divergență și-ar lăsa înregistrările în
    // urmă, iar contractul ar „explica" o diferență care nu mai există (ar pica
    // zgomotos — dar degeaba).
    public void UitaSursa(IObjectSpace os, string sursa) {
        var identitati = (peSursa.GetValueOrDefault(sursa) ?? []).ToList();
        if (identitati.Count == 0)
            return;
        var chei = identitati.Select(i => cheiPersistate[i]).ToList();
        var tabela = Legaturi.Tabela(View);
        os.Delete(os.GetObjectsQuery<MigrareLegatura>()
            .Where(m => m.Tabela == tabela && chei.Contains(m.CheieLegacy)).ToList());
        os.CommitChanges();
        foreach (var identitate in identitati)
            Uita(identitate);
    }

    // ======================= Înregistrarea =======================

    // `sursa` e cheia documentului-SURSĂ („<view>/<cheieHex>"), nu a artefactului
    // Atlas: la replanificare tot ce a produs documentul se dă înapoi împreună.
    public void Inregistreaza(int an, int luna, string sursa, string categorie,
            IEnumerable<EfectStoc> stoc = null,
            string contDebit = null, string contCredit = null, decimal valoareNepostata = 0m) {
        // Scara de scriere: valorile vin din împărțiri pro-rata (`Suma × rest /
        // cantitate`) și moștenesc zeci de zecimale. Cheia rândului le conține, deci
        // fără rotunjire ar fi lungă și, mai rău, ar putea diferi între rulări pe
        // ultimul bit. 6 zecimale la cantitate (toleranța contractului e 0,0005) și
        // 4 la bani (toleranța e 0,005) sunt cu ordine de mărime sub ce se compară.
        var efecte = stoc?
            .Select(e => e with {
                Cantitate = Math.Round(e.Cantitate, 6),
                Valoare = Math.Round(e.Valoare, 4),
            })
            .Where(e => e.Cantitate != 0m || e.Valoare != 0m)
            .ToList() ?? [];
        valoareNepostata = Math.Round(valoareNepostata, 4);
        if (efecte.Count == 0 && valoareNepostata == 0m)
            return;
        if (efecte.Count == 0) {
            Adauga(new Divergenta(an, luna, sursa, Curat(categorie), null, null, 0m, 0m,
                contDebit, contCredit, valoareNepostata));
            return;
        }
        // Efectul CONTABIL al unei linii aruncate e unul singur, oricâte chei de
        // stoc atinge: se pune pe primul efect, ca suma per cont să nu se dubleze.
        var contabilPus = false;
        foreach (var e in efecte) {
            Adauga(new Divergenta(an, luna, sursa, Curat(categorie),
                e.ProdusHex, e.DepozitHex, e.Cantitate, e.Valoare,
                contabilPus ? null : contDebit, contabilPus ? null : contCredit,
                contabilPus ? 0m : valoareNepostata));
            contabilPus = true;
        }
    }

    void Adauga(Divergenta d) {
        var identitate = d.Identitate;
        inAsteptare[identitate] = inAsteptare.TryGetValue(identitate, out var deja)
            ? deja.Aduna(d) : d;
    }

    // Categoriile sunt etichete de raport scrise în cod; separatorul n-are ce
    // căuta în ele, dar cheia trebuie să rămână parsabilă orice s-ar întâmpla.
    static string Curat(string text) =>
        text?.Replace(Sep, '/').Replace('\u0001', ' ') ?? "";

    // ======================= Persistența =======================

    // Se scrie în ObjectSpace-ul DOCUMENTULUI, înaintea commit-ului lui (același
    // punct cu `AlocareIesire.Persista`).
    public void Persista(IObjectSpace os) {
        tranzactie = null;
        if (inAsteptare.Count == 0)
            return;

        // Primul flush al unei surse în rularea asta = replanificare: rândurile ei
        // vechi pleacă (mai puțin cele reproduse identic — le lăsăm pe loc, ca să
        // nu ștergem și să reinserăm aceeași cheie în același commit).
        var purjate = new HashSet<string>(StringComparer.Ordinal);
        var deSters = new List<string>();
        foreach (var sursa in inAsteptare.Values.Select(d => d.Sursa).Distinct(StringComparer.Ordinal)) {
            if (!surseScrise.Add(sursa))
                continue;
            foreach (var identitate in (peSursa.GetValueOrDefault(sursa) ?? []).ToList()) {
                purjate.Add(identitate);
                if (inAsteptare.TryGetValue(identitate, out var noua) && Codeaza(noua) == cheiPersistate[identitate])
                    continue;
                deSters.Add(cheiPersistate[identitate]);
            }
        }

        var mutatii = new List<(string, Divergenta, string)>();
        foreach (var identitate in purjate)
            if (!inAsteptare.ContainsKey(identitate))
                mutatii.Add((identitate, null, null));

        foreach (var (identitate, pending) in inAsteptare) {
            // Sursă deja scrisă în rularea asta și NEpurjată acum (document spart pe
            // mai multe commit-uri): se adună peste ce s-a scris.
            var final = !purjate.Contains(identitate) && toate.TryGetValue(identitate, out var deja)
                ? deja.Aduna(pending)
                : pending;
            var cheieNoua = CodeazaVerificat(final);
            var cheieVeche = cheiPersistate.GetValueOrDefault(identitate);
            if (cheieVeche == cheieNoua)
                continue;
            if (cheieVeche != null && !deSters.Contains(cheieVeche))
                deSters.Add(cheieVeche);
            mutatii.Add((identitate, final, cheieNoua));
        }

        if (deSters.Count > 0) {
            var tabela = Legaturi.Tabela(View);
            os.Delete(os.GetObjectsQuery<MigrareLegatura>()
                .Where(m => m.Tabela == tabela && deSters.Contains(m.CheieLegacy)).ToList());
        }
        foreach (var (_, d, cheieNoua) in mutatii)
            if (d != null)
                Legaturi.Leaga(os, View, cheieNoua, Guid.Empty);
        tranzactie = mutatii;
    }

    // Divergențele unei unități de import care N-A MATERIALIZAT NIMIC: linia
    // aruncată INTEGRAL (transferul pe care 1C îl face pe minus, vânzarea fără
    // niciun lot acoperit) lasă documentul fără linii, deci fără commit — iar
    // fără commit înregistrările ar fi abandonate. Efectul e pervers: exact
    // liniile pierdute CEL MAI GRAV ar fi singurele neînregistrate, iar
    // contractul le-ar raporta ca nejustificate. Măsurat pe M187 × REZERVARI
    // MAGAZIN: transferul din 13.01 13:51 aruncat integral, marfa rămasă la
    // expeditor, nicio urmă în registru.
    //
    // Se scriu deci în ObjectSpace PROPRIU, la finalul unității — dar numai dacă
    // unitatea s-a executat până la capăt: o unitate care a aruncat o excepție
    // n-a produs faptele pe care înregistrările le descriu, iar acolo abandonul
    // rămâne răspunsul corect (`RenuntaLaNepersistate`).
    public void PersistaRamase(IObjectSpaceProvider provider) {
        if (inAsteptare.Count == 0)
            return;
        using var os = provider.CreateObjectSpace();
        Persista(os);
        os.CommitChanges();
        Confirma();
    }

    public void Confirma() {
        if (tranzactie != null)
            foreach (var (identitate, d, cheieNoua) in tranzactie) {
                Uita(identitate);
                if (d != null)
                    Adopta(identitate, d, cheieNoua);
            }
        tranzactie = null;
        inAsteptare.Clear();
    }

    public void RenuntaLaNepersistate() {
        Abandonate += inAsteptare.Count;
        inAsteptare.Clear();
        tranzactie = null;
    }

    // ======================= Interogarea contractului =======================

    // Divergențele care contează pentru soldul CUMULAT la finele lunii: toate
    // lunile de până acum inclusiv. Purtarea înainte (§12.4) iese de la sine —
    // registrul e cumulativ prin construcție, nu prin memoria rulării.
    public List<Divergenta> PanaLa(int an, int luna) =>
        toate.Values.Where(d => d.An < an || (d.An == an && d.Luna <= luna)).ToList();

    // ======================= Codificarea =======================

    // Registrul e determinist doar dacă rândul scris se citește ÎNAPOI identic —
    // cheia sintetică îi poartă și valorile. Verificarea costă microsecunde și
    // prinde pe loc orice separator strecurat într-o etichetă sau orice scară de
    // decimal care nu supraviețuiește formatării; un rând care nu se recitește ar
    // dispărea tăcut la reluare, adică exact defectul pe care registrul îl repară.
    static string CodeazaVerificat(Divergenta d) {
        var cheie = Codeaza(d);
        if (Decodeaza(cheie) != d)
            throw new InvalidOperationException(
                $"Registrul divergențelor: rândul „{cheie}” nu se citește înapoi identic — "
                + "cheia sintetică e coruptă, iar verdictul contractului n-ar mai fi determinist.");
        return cheie;
    }

    static string Codeaza(Divergenta d) => string.Join(Sep,
        Versiune,
        d.An.ToString(CultureInfo.InvariantCulture),
        d.Luna.ToString(CultureInfo.InvariantCulture),
        d.Sursa ?? "", d.Categorie ?? "", d.ProdusHex ?? "", d.DepozitHex ?? "",
        d.Cantitate.ToString(CultureInfo.InvariantCulture),
        d.Valoare.ToString(CultureInfo.InvariantCulture),
        d.ContDebit ?? "", d.ContCredit ?? "",
        d.ValoareNepostata.ToString(CultureInfo.InvariantCulture));

    static Divergenta Decodeaza(string cheie) {
        var p = cheie.Split(Sep);
        if (p.Length != 12 || p[0] != Versiune)
            return null;
        if (!int.TryParse(p[1], CultureInfo.InvariantCulture, out var an)
                || !int.TryParse(p[2], CultureInfo.InvariantCulture, out var luna)
                || !decimal.TryParse(p[7], CultureInfo.InvariantCulture, out var cantitate)
                || !decimal.TryParse(p[8], CultureInfo.InvariantCulture, out var valoare)
                || !decimal.TryParse(p[11], CultureInfo.InvariantCulture, out var nepostata))
            return null;
        return new Divergenta(an, luna, p[3], p[4],
            Gol(p[5]), Gol(p[6]), cantitate, valoare, Gol(p[9]), Gol(p[10]), nepostata);
    }

    static string Gol(string text) => text.Length == 0 ? null : text;
}
