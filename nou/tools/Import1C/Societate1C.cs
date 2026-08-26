using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;

namespace Import1C;

// SOCIETATEA RAPORTOARE din 1C (felia 16, D16-D6).
//
// Ce umple: rândul unic `Societate` (D16-D1) — antetul fișierului SAF-T:
// denumire, CUI, registrul comerțului, adresa structurată, contactul, contul
// bancar. Sursele sunt cele pe care le citea și exportul vechi (§D.3):
// `flax.Organizatii` + `InfoRg_InformatiaDeContact` (adresă/telefon/e-mail, pe
// `Object_Organizatii_ID`) + `InfoRg_PersoaneResponsabileDinOrganizatia` +
// `ConturiBancare` proprii.
//
// REGULA, aceeași ca la adresa partenerului (72d): se scrie DOAR PE CÂMP GOL.
// Societatea e date ale clientului, culese de om pe un ecran (de aceea e
// nomenclator editabil, nu `SetareProfil`); o rulare de import n-are voie să
// rescrie sediul pe care l-a corectat contabilul fiindcă în 1C a rămas cel
// vechi. Ce e diferit se RAPORTEAZĂ, cu ambele valori — „gol se umple, diferit
// se raportează" e chiar formula lui 72d, iar aici canonicul e omul, nu sursa.
//
// O singură abatere de la „câmp cu câmp": ADRESA se scrie ca BLOC, exact ca la
// partener (`ImportLaCerere.AplicaAdresa`) și pentru același motiv — umplerea
// câmp-cu-câmp ar amesteca strada culeasă de om cu localitatea din 1C și ar
// produce o adresă care n-a existat nicăieri.
//
// Ce NU se ia din 1C: `BazaContabila` (e o alegere fiscală a societății, nu un
// fapt al catalogului) și `RaporteazaCnp` (politică de confidențialitate a
// bazei). Amândouă rămân pe default-urile tipului.
sealed class Societate1C {
    readonly IObjectSpaceProvider provider;
    readonly FlaxDb flax;
    readonly Action<string> avert;

    public Societate1C(IObjectSpaceProvider provider, FlaxDb flax, Action<string> avert) {
        this.provider = provider;
        this.flax = flax;
        this.avert = avert;
    }

    // Lungimile `[MaxLength]` ale lui `Societate`, citite din MODEL prin
    // reflecție — aceeași disciplină ca `AdresaSaft.Lungimi` (care le citește de
    // pe `Partener`) și pentru același motiv: coloana și fișierul trebuie să
    // spună același număr, iar o a doua listă scrisă cu mâna e locul în care
    // apare deriva. Nu se poate refolosi `AdresaSaft.Lungimi` ca atare: acolo
    // sunt lungimile lui `Partener`, iar `Societate` are câmpuri proprii
    // (`ContactNume`, `Telefon`, `Email`) pe care partenerul nu le are. Pe cele
    // ȘASE de adresă cele două dicționare sunt egale — faptul e probat de
    // D16-V1, nu presupus aici.
    static readonly IReadOnlyDictionary<string, int> Lungimi = typeof(Societate)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(pi => (pi.Name, Lungime: pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0))
        .Where(x => x.Lungime > 0)
        .ToDictionary(x => x.Name, x => x.Lungime, StringComparer.Ordinal);

    // Câmpurile umplute în rularea asta (nume → valoarea scrisă) și cele pe care
    // sursa le contrazice (nume → „în bază / în 1C"). Ambele liste intră în
    // raport: prima spune ce a făcut importul, a doua ce are omul de verificat.
    public List<string> CampuriUmplute { get; } = [];
    public List<string> Diferente { get; } = [];
    public int CampuriDejaCompletate { get; private set; }
    public int Trunchiate { get; private set; }
    public bool AdresaPreluata { get; private set; }
    public bool AdresaDejaCompletata { get; private set; }
    public bool FaraAdresaInSursa { get; private set; }
    public string JudetProvenienta { get; private set; } = "–";
    public bool InregistratTvaDerivat { get; private set; }

    /// <summary>
    /// Aplică organizația 1C pe rândul unic `Societate`. Întoarce `false` dacă
    /// sursa n-are exact o organizație — cazul e o DECIZIE („care societate
    /// raportează?"), nu o ordine de citire, deci unealta se oprește și spune
    /// care sunt (bug-ul §D.6 al exportului vechi: `select … from Organizatii`
    /// fără `where`, semnat cu una la întâmplare).
    /// </summary>
    public bool Executa() {
        var organizatii = flax.Organizatii();
        Console.WriteLine($"\n=== SOCIETATEA din 1C (flax.Organizatii: {organizatii.Count} rânduri) ===");
        foreach (var o in organizatii)
            Console.WriteLine($"    {o.Cod} · {o.DenumireCompleta ?? o.Denumire} · CUI {o.CodUnic ?? "–"} "
                + $"· RegCom {o.RegCom ?? "–"}");
        if (organizatii.Count != 1) {
            avert($"flax.Organizatii are {organizatii.Count} rânduri — societatea raportoare NU se poate "
                + "alege automat (exportul vechi o alegea la întâmplare, §D.6). Rândul `Societate` rămâne "
                + "neatins; alegerea e a operatorului.");
            return false;
        }

        var org = organizatii[0];
        var adresa = flax.AdresaOrganizatie(org.Id);
        var (telefon, email) = flax.ContactOrganizatie(org.Id);
        var conducator = flax.ConducatorulOrganizatiei(org.Id);
        Console.WriteLine($"    adresă: {(adresa == null ? "(niciuna în sursă)" : $"{adresa.JudetDenumire ?? "–"}, "
            + $"{adresa.Localitate ?? "–"}, {adresa.Strada ?? "–"} {adresa.Numar ?? "–"}")}; "
            + $"telefon {telefon ?? "–"}; e-mail {email ?? "–"}; "
            + $"conducător {conducator.Nume ?? "–"} ({conducator.Functie ?? "–"}).");

        using var os = provider.CreateObjectSpace();
        var e = os.GetObjectsQuery<Societate>().FirstOrDefault();
        if (e == null) {
            // Seed-ul creează rândul gol (`ContaSeeder.SeedSocietate`); dacă
            // lipsește, baza n-a fost seed-uită cu felia 16 — se creează aici,
            // ca unealta să nu ceară o rulare separată de updater.
            e = os.CreateObject<Societate>();
            avert("Rândul `Societate` lipsea (bază fără seed-ul feliei 16) — a fost creat de conector.");
        }

        // Identitatea fiscală. `InregistratTva` e bool — n-are „gol", deci nu
        // poate urma litera regulii 72d. Urmează SPIRITUL ei: se derivă o
        // singură dată, împreună cu CUI-ul, și numai când CUI-ul era gol (adică
        // rândul nu fusese încă identificat de nimeni). Pe o societate deja
        // completată, `InregistratTva` e alegerea omului și rămâne a lui.
        var cuiGol = string.IsNullOrWhiteSpace(e.CodFiscal);
        var inregistrat = org.CodUnic != null
            && org.CodUnic.TrimStart().StartsWith("RO", StringComparison.OrdinalIgnoreCase);
        Scrie(nameof(Societate.Denumire), e.Denumire, org.DenumireCompleta ?? org.Denumire,
            v => e.Denumire = v);
        // CUI-ul se normalizează cu ACEEAȘI funcție ca la partener
        // (`D394Proiectii.NormalizeazaCui`, o singură sursă — pasul 4b): taie
        // prefixul `RO` repetat și insensibil la caz. Societatea e rezidentă
        // („RO" hardcodat ar fi fost constanta pe care n-o mai găsește nimeni;
        // `Tara` e câmp și e default RO).
        Scrie(nameof(Societate.CodFiscal), e.CodFiscal,
            D394Proiectii.NormalizeazaCui(org.CodUnic, e.Tara, inregistrat), v => e.CodFiscal = v);
        if (cuiGol && !string.IsNullOrWhiteSpace(e.CodFiscal) && e.InregistratTva != inregistrat) {
            e.InregistratTva = inregistrat;
            InregistratTvaDerivat = true;
            CampuriUmplute.Add($"{nameof(Societate.InregistratTva)} = {inregistrat} "
                + $"(DERIVAT din prefixul RO al CUI-ului „{org.CodUnic}” — catalogul 1C n-are coloană de TVA)");
        }
        Scrie(nameof(Societate.RegistruComert), e.RegistruComert, org.RegCom, v => e.RegistruComert = v);

        // Contactul: numele conducătorului e „NUME PRENUME" în 1C (grafia
        // românească a catalogului de persoane fizice). Se rupe la PRIMUL spațiu
        // — primul token e numele de familie. E o convenție, nu un fapt al
        // sursei, deci se raportează ca atare; ce nu are spațiu intră întreg pe
        // `ContactNume` și `ContactPrenume` rămâne gol (nu se inventează).
        if (conducator.Nume != null) {
            var bucati = conducator.Nume.Split(' ', 2, StringSplitOptions.TrimEntries
                | StringSplitOptions.RemoveEmptyEntries);
            Scrie(nameof(Societate.ContactNume), e.ContactNume, bucati.ElementAtOrDefault(0),
                v => e.ContactNume = v);
            Scrie(nameof(Societate.ContactPrenume), e.ContactPrenume, bucati.ElementAtOrDefault(1),
                v => e.ContactPrenume = v);
        }
        Scrie(nameof(Societate.Telefon), e.Telefon, telefon, v => e.Telefon = v);
        Scrie(nameof(Societate.Email), e.Email, email, v => e.Email = v);

        AplicaAdresa(e, adresa);
        AplicaContBancar(os, e, org);

        os.CommitChanges();
        return true;
    }

    // Un câmp de text: gol ⇒ se umple (tăiat la lungimea din model); completat
    // și DIFERIT ⇒ se raportează; completat și identic ⇒ se numără tăcut.
    void Scrie(string camp, string existent, string dinSursa, Action<string> pune) {
        if (string.IsNullOrWhiteSpace(dinSursa))
            return;
        var valoare = dinSursa.Trim();
        if (Lungimi.TryGetValue(camp, out var maxim) && valoare.Length > maxim) {
            valoare = valoare[..maxim];
            Trunchiate++;
        }
        if (string.IsNullOrWhiteSpace(existent)) {
            pune(valoare);
            CampuriUmplute.Add($"{camp} = „{valoare}”");
            return;
        }
        CampuriDejaCompletate++;
        if (!string.Equals(existent.Trim(), valoare, StringComparison.Ordinal))
            Diferente.Add($"{camp}: în bază „{existent}” · în 1C „{valoare}”");
    }

    // Adresa, ca BLOC (vezi nota clasei). Județul urmează exact regulile
    // partenerului (D15-D6, refolosite ca atare): codul din CNP întâi — aici
    // lipsește pe organizații, dar cititorul e același —, denumirea pe urmă,
    // prin `JudeteRo` + grafiile proprii lui 1C; nerezolvat ⇒ nimic în FK și
    // denumirea brută în `DetaliiAdresa`.
    void AplicaAdresa(Societate e, FlaxAdresa a) {
        if (a == null || (a.CodPostal == null && a.Localitate == null && a.Strada == null
                && a.Numar == null && a.Cladire == null
                && a.JudetDenumire == null && a.CodJudetCnp == null)) {
            FaraAdresaInSursa = true;
            return;
        }
        if (e.Strada != null || e.Numar != null || e.DetaliiAdresa != null
                || e.Localitate != null || e.CodPostal != null || e.JudetId != null) {
            AdresaDejaCompletata = true;
            return;
        }

        string codIso = null;
        if (a.CodJudetCnp != null)
            codIso = JudeteRo.DupaCodCnp(a.CodJudetCnp.Value);
        var dinCodCnp = codIso != null;
        codIso ??= ImportLaCerere.JudetDinDenumire1C(a.JudetDenumire);

        // Județul e al adreselor din România, pe toate cele trei uși (72b);
        // societatea e rezidentă, dar regula se scrie o dată, nu se presupune.
        Guid? judet = null;
        if (codIso != null && string.Equals(e.Tara, "RO", StringComparison.Ordinal)) {
            using var osJudete = provider.CreateObjectSpace();
            judet = osJudete.GetObjectsQuery<Judet>().Where(j => j.Cod == codIso)
                .Select(j => (Guid?)j.ID).FirstOrDefault();
        }
        if (judet != null) {
            e.JudetId = judet;
            JudetProvenienta = dinCodCnp ? $"cod CNP ⇒ {codIso}" : $"denumire „{a.JudetDenumire}” ⇒ {codIso}";
        }
        else if (a.JudetDenumire != null || a.CodJudetCnp != null)
            JudetProvenienta = $"NEREZOLVAT („{a.JudetDenumire ?? a.CodJudetCnp?.ToString()}” "
                + "rămâne în DetaliiAdresa)";

        var detalii = ImportLaCerere.DetaliiDinPrezentare(a);
        if (judet == null && a.JudetDenumire != null)
            detalii.Add(a.JudetDenumire);

        e.Strada = Taie(nameof(Societate.Strada), a.Strada);
        e.Numar = Taie(nameof(Societate.Numar), a.Numar);
        e.Localitate = Taie(nameof(Societate.Localitate), a.Localitate);
        e.CodPostal = Taie(nameof(Societate.CodPostal), a.CodPostal);
        e.DetaliiAdresa = Taie(nameof(Societate.DetaliiAdresa),
            detalii.Count == 0 ? null : string.Join(", ", detalii));
        AdresaPreluata = true;
        CampuriUmplute.Add($"Adresa (bloc) = „{e.Strada} {e.Numar}, {e.Localitate}” "
            + $"[județ: {JudetProvenienta}]");

        string Taie(string camp, string valoare) {
            if (valoare == null || !Lungimi.TryGetValue(camp, out var maxim) || valoare.Length <= maxim)
                return valoare;
            Trunchiate++;
            return valoare[..maxim];
        }
    }

    // Contul bancar din `Header/Company/BankAccount`: FK către `ContPropriu`,
    // NU un IBAN copiat (conturile proprii sunt deja nomenclator, iar o a doua
    // copie s-ar desincroniza tăcut — nota lui `Societate.ContBancarId`).
    //
    // Alegerea: contul DECLARAT implicit în 1C (`ContBancarImplicit_ID`), fiindcă
    // e o decizie a omului; fără el, primul cont în lei cu IBAN, în ordinea
    // codului — și atunci se spune în raport că a fost o alegere a conectorului.
    void AplicaContBancar(IObjectSpace os, Societate e, FlaxOrganizatie org) {
        if (e.ContBancarId != null) {
            CampuriDejaCompletate++;
            return;
        }
        var legaturi = Legaturi.Incarca(os, "ConturiBancare");
        var conturi = flax.ConturiBancareProprii();
        var ales = org.ContBancarImplicitId != null
            ? conturi.FirstOrDefault(c => c.Id == org.ContBancarImplicitId)
            : null;
        var motiv = ales != null ? "contul implicit declarat în 1C" : null;
        ales ??= conturi.Where(c => !c.Marcat && EsteLei(c.Valuta) && !string.IsNullOrWhiteSpace(c.Iban))
            .OrderBy(c => c.Cod, StringComparer.Ordinal).FirstOrDefault();
        motiv ??= "primul cont ACTIV în lei cu IBAN (1C n-are cont implicit declarat)";
        if (ales == null) {
            avert("Societatea n-are cont bancar în 1C (niciun cont propriu activ în lei cu IBAN) — "
                + "`Header/BankAccount` va lipsi din fișier.");
            return;
        }
        if (!legaturi.TryGetValue(ales.Id, out var contPropriu)) {
            avert($"Contul bancar {ales.Cod} ({ales.Iban}) nu e legat de niciun `ContPropriu` — "
                + "nomenclatorul de conturi bancare n-a fost încă importat pe baza asta; "
                + "`Societate.ContBancar` rămâne gol.");
            return;
        }
        e.ContBancarId = contPropriu;
        CampuriUmplute.Add($"{nameof(Societate.ContBancarId)} = {ales.Cod} „{ales.Iban}” ({motiv})");
    }

    static bool EsteLei(string valuta) =>
        valuta == null || valuta.Equals("Lei", StringComparison.OrdinalIgnoreCase)
        || valuta.Equals("RON", StringComparison.OrdinalIgnoreCase);

    public void Raporteaza(string pas) {
        Console.WriteLine($"\n--- Societatea din 1C ({pas}) ---");
        Console.WriteLine($"SocietateCampuriUmplute {CampuriUmplute.Count,6}");
        foreach (var c in CampuriUmplute)
            Console.WriteLine($"        {c}");
        Console.WriteLine($"SocietateDiferente      {Diferente.Count,6} (câmp completat în bază ≠ 1C — "
            + "canonicul e omul, sursa se raportează)");
        foreach (var d in Diferente)
            Console.WriteLine($"        {d}");
        Console.WriteLine($"SocietateNeatinse       {CampuriDejaCompletate,6} câmpuri deja completate "
            + $"(idempotență); {Trunchiate} tăiate la lungimea SAF-T.");
        Console.WriteLine($"Adresa: {(AdresaPreluata ? "PRELUATĂ" : AdresaDejaCompletata ? "deja completată (neatinsă)"
            : FaraAdresaInSursa ? "fără adresă în sursă" : "neaplicată")}; județ: {JudetProvenienta}.");
    }
}
