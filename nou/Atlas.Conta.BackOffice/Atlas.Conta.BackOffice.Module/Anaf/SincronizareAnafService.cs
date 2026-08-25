using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Anaf;

// MERGE-UL cu registrul ANAF (felia 15, D15-D3): „gol se umple, diferit se
// raportează, canonicul bate".
//
// Cele trei regimuri, și de ce sunt trei:
//  · CANONIC (`InregistratTva`, `TvaLaIncasare`, `InactivFiscal`) — pe axa
//    „înregistrat în scopuri de TVA" registrul ANAF ESTE adevărul (D4-r1), nu
//    o evidență printre altele. Se scrie întotdeauna; ce era înainte iese ca
//    `Modificare`, ca să rămână urma.
//  · ETICHETĂ (`Denumire`, `RegistruComert`) — sunt ale contabilului. O valoare
//    culeasă și diferită NU se rescrie: iese ca `Diferenta`, ca omul să decidă.
//  · ADRESA — bloc, cu sursa aleasă (domiciliul fiscal dacă are localitate,
//    altfel sediul social), apoi câmp cu câmp după aceeași regulă.
//
// Peste toate, aceeași lege: un câmp GOL se umple (n-avem ce pierde), unul
// ne-gol și diferit se raportează, iar `suprascrie` transformă raportarea în
// scriere — dar tot cu `Modificare` care poartă valoarea VECHE (riscul 2 al
// contractului: un lot cu `suprascrie` e ireversibil, deci trebuie să lase
// listă, nu doar un contor).
//
// STATIC și PUR: `Aplica` nu face HTTP, nu comite și nu cunoaște `IObjectSpace`
// — de-aia D15-V2 o probează pe parteneri FABRICAȚI (`new Partener()`), fără
// scenă și fără rețea. Singura legătură cu baza, lookup-ul de județ, intră ca
// `Func<string, Judet>`. (`BaseObject` implementează `IObjectSpaceLink`, deci
// tentația era să citim `p.ObjectSpace` — pe o instanță fabricată e null, iar
// pe una reală ar fi legat o funcție pură de infrastructură.)
public static class SincronizareAnafService {
    // Lungimile coloanelor, citite din MODEL prin reflecție: `[MaxLength]` de pe
    // `Partener` e deja lungimea SAF-T (D15-D1, probat de D15-V1). O a doua
    // listă scrisă cu mâna aici ar fi exact locul unde apare deriva.
    static readonly Dictionary<string, int> Lungimi = typeof(Partener)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(pi => (pi.Name, Lungime: pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0))
        .Where(x => x.Lungime > 0)
        .ToDictionary(x => x.Name, x => x.Lungime, StringComparer.Ordinal);

    // ================= Interogabilitatea (D15-D3) =================

    // CUI-ul de trimis la ANAF, sau motivul pentru care partenerul nu e candidat.
    // Un singur loc pentru ambele: `Candidati` are nevoie de motiv, apelanții
    // simpli doar de cifră (`CuiInterogabil`).
    public static (long? Cui, string Motiv) Interogabilitate(Partener p) {
        if (p == null)
            return (null, "partener inexistent");
        if (!string.Equals(p.Tara, "RO", StringComparison.Ordinal))
            return (null, $"țara „{p.Tara}” — registrul `PlatitorTva` e al codurilor fiscale românești");
        var cifre = Cifre(p.CodFiscal);
        if (cifre == null)
            return (null, string.IsNullOrWhiteSpace(p.CodFiscal)
                ? "fără cod fiscal"
                : $"codul fiscal „{p.CodFiscal}” nu e un CUI (cifre)");
        // Clauza explicită din D15-D3: un CNP nu se interoghează. Plafonul de 10
        // cifre l-ar respinge oricum (CNP-ul are 13) — dar regula merită scrisă
        // ca regulă, nu lăsată să iasă din altă limită: motivul raportat
        // utilizatorului trebuie să spună „e CNP", nu „prea lung".
        if (p.TipPersoana == TipPersoana.Fizica && cifre.Length == 13)
            return (null, "cod numeric personal (persoană fizică fără CUI)");
        if (cifre.Length < 2 || cifre.Length > 10)
            return (null, $"codul fiscal are {cifre.Length} cifre (CUI-ul are între 2 și 10)");
        return (long.Parse(cifre), null);
    }

    public static long? CuiInterogabil(Partener p) => Interogabilitate(p).Cui;

    // Supraîncărcarea pe text: aceeași normalizare, folosită de probe și de
    // conectoare care au codul, nu partenerul.
    public static long? CuiInterogabil(string codFiscal) {
        var cifre = Cifre(codFiscal);
        return cifre != null && cifre.Length is >= 2 and <= 10 ? long.Parse(cifre) : null;
    }

    // Trim, MAJUSCULE, prefixul „RO" tăiat, spațiile ignorate („RO 123 45" ⇒
    // „12345" — riscul 7 al contractului), orice ALT caracter ne-cifră ⇒ null.
    // Nu „curățăm" litere: un cod fiscal cu litere nu e un CUI românesc, iar a-l
    // muta cu forța într-un număr ar interoga altă firmă.
    static string Cifre(string codFiscal) {
        var text = codFiscal?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(text))
            return null;
        if (text.StartsWith("RO", StringComparison.Ordinal))
            text = text[2..];
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text) {
            if (char.IsWhiteSpace(ch))
                continue;
            if (!char.IsAsciiDigit(ch))
                return null;
            sb.Append(ch);
        }
        return sb.Length == 0 ? null : sb.ToString();
    }

    // Filtrul din D15-D4/D15-D6: din ID-urile cerute ies candidații (parteneri
    // vii, români, cu CUI) și cei săriți, FIECARE CU MOTIV. Un ID care nu se
    // regăsește (inexistent sau șters logic — `GetObjectsQuery` filtrează
    // `GCRecord`) e tot un sărit, nu o excepție: un lot nu pică din cauza unui
    // rând.
    public static SelectieCandidati Candidati(IObjectSpace os, IEnumerable<Guid> ids) {
        var ceruti = (ids ?? []).Distinct().ToList();
        var gasiti = os.GetObjectsQuery<Partener>().Where(p => ceruti.Contains(p.ID)).ToList();
        var dupaId = gasiti.ToDictionary(p => p.ID);
        var candidati = new List<Partener>();
        var sarite = new List<Sarit>();
        foreach (var id in ceruti) {
            if (!dupaId.TryGetValue(id, out var p)) {
                sarite.Add(new Sarit(id, null, "partenerul nu există (sau e șters)"));
                continue;
            }
            var (cui, motiv) = Interogabilitate(p);
            if (cui == null)
                sarite.Add(new Sarit(id, Eticheta(p), motiv));
            else
                candidati.Add(p);
        }
        return new SelectieCandidati(candidati, sarite);
    }

    // ================= Merge-ul propriu-zis (D15-D3) =================

    // Partenerul NU se atinge, dar rezultatul spune de ce: `notFound` de la ANAF
    // (200 cu CUI-ul în `notFound` — se întâmplă și la coduri radiate, riscul 4)
    // nu e „nu e plătitor de TVA", e „nu figurează". `InregistratTva` rămâne ce
    // era, iar TIMBRUL nu se pune: n-a fost o sincronizare.
    public static RezultatSincronizare Negasit(Partener p, long cui) {
        var r = new RezultatSincronizare { PartenerId = p?.ID ?? Guid.Empty, Eticheta = Eticheta(p), Cui = cui, Gasit = false };
        r.Avertismente.Add($"CUI-ul {cui} nu figurează la ANAF — nicio valoare nu s-a schimbat "
            + "(un cod radiat răspunde la fel ca unul greșit).");
        return r;
    }

    public static RezultatSincronizare Aplica(Partener p, DateAnaf d, bool suprascrie, DateTime acum,
            Func<string, Judet> cautaJudet) {
        ArgumentNullException.ThrowIfNull(p);
        ArgumentNullException.ThrowIfNull(d);
        var r = new RezultatSincronizare {
            PartenerId = p.ID, Eticheta = Eticheta(p), Cui = d.Cui, Gasit = true,
        };

        // --- Canonic (D4-r1): registrul ANAF e adevărul pe axa TVA ---
        Canonic(r, nameof(Partener.InregistratTva), "înregistrat în scopuri de TVA",
            p.InregistratTva, d.ScpTva, v => p.InregistratTva = v);
        Canonic(r, nameof(Partener.TvaLaIncasare), "TVA la încasare",
            p.TvaLaIncasare, d.TvaLaIncasare, v => p.TvaLaIncasare = v);
        Canonic(r, nameof(Partener.InactivFiscal), "inactiv fiscal",
            p.InactivFiscal, d.Inactiv, v => p.InactivFiscal = v);

        // --- Etichete ---
        // `Denumire` NU se rescrie implicit peste o valoare culeasă (e eticheta
        // contabilului, tabelul din D15-D3). Peste una GOALĂ însă se scrie: „gol
        // se umple" e titlul deciziei, iar un partener fără nume n-are ce
        // pierde. Asimetria e deliberată și se citește exact așa în probe.
        Text(r, nameof(Partener.Denumire), p.Denumire, d.Denumire, v => p.Denumire = v, suprascrie);
        Text(r, nameof(Partener.RegistruComert), p.RegistruComert, d.NrRegCom, v => p.RegistruComert = v, suprascrie);

        // --- Adresa, ca BLOC cu sursă aleasă ---
        // Domiciliul fiscal e adresa de corespondență fiscală și e cea pe care o
        // cer SAF-T/e-Factura; sediul social e rezerva. Criteriul e `Localitate`
        // fiindcă e singurul câmp OBLIGATORIU al `AddressStructure`: o adresă
        // fără localitate nu e adresă, e un fragment.
        var adresa = AlegeAdresa(d);
        if (adresa == null)
            r.Avertismente.Add("ANAF n-a întors nicio adresă utilizabilă (fără localitate).");
        else {
            Text(r, nameof(Partener.Strada), p.Strada, adresa.Strada, v => p.Strada = v, suprascrie);
            Text(r, nameof(Partener.Numar), p.Numar, adresa.Numar, v => p.Numar = v, suprascrie);
            Text(r, nameof(Partener.DetaliiAdresa), p.DetaliiAdresa, adresa.Detalii, v => p.DetaliiAdresa = v, suprascrie);
            Text(r, nameof(Partener.Localitate), p.Localitate, adresa.Localitate, v => p.Localitate = v, suprascrie);
            // Codul poștal al adresei alese, cu `date_generale.codPostal` ca
            // rezervă (pe multe răspunsuri reale `dcod_Postal` e gol, iar cel
            // general e completat — CUI 14399840, apelul de la implementare).
            Text(r, nameof(Partener.CodPostal), p.CodPostal,
                Gol(adresa.CodPostal) ? d.CodPostal : adresa.CodPostal, v => p.CodPostal = v, suprascrie);
            Judetul(r, p, adresa, suprascrie, cautaJudet);
        }

        // TIMBRUL: pe ORICE răspuns „găsit", chiar fără nicio modificare — el
        // răspunde la „de când știm că datele astea sunt ale ANAF", nu la „ce
        // s-a schimbat". Nu intră în `Modificari`: e server-owned, nu o valoare
        // a partenerului pe care omul ar fi putut-o pune altfel.
        p.DataSincronizareAnaf = acum;
        return r;
    }

    // Sursa adresei: domiciliul fiscal dacă are localitate, altfel sediul social
    // (dacă are), altfel nimic.
    static AdresaAnaf AlegeAdresa(DateAnaf d) {
        if (d.DomiciliuFiscal != null && !Gol(d.DomiciliuFiscal.Localitate))
            return d.DomiciliuFiscal;
        if (d.SediuSocial != null && !Gol(d.SediuSocial.Localitate))
            return d.SediuSocial;
        return null;
    }

    static void Canonic(RezultatSincronizare r, string camp, string eticheta,
            bool vechi, bool? nou, Action<bool> scrie) {
        if (nou == null) {
            // Tăcerea ANAF nu e un „nu": blocul lipsă din răspuns lasă câmpul
            // neatins. Altfel un răspuns trunchiat ar de-înregistra în masă
            // parteneri plătitori de TVA — exact genul de scriere pe care
            // „canonic" o face periculoasă.
            r.Avertismente.Add($"ANAF n-a raportat „{eticheta}” — câmpul rămâne neschimbat.");
            return;
        }
        if (nou.Value == vechi)
            return;
        scrie(nou.Value);
        r.Modificari.Add(new ModificareCamp(camp, Da(vechi), Da(nou.Value)));
    }

    static void Text(RezultatSincronizare r, string camp, string cules, string anaf,
            Action<string> scrie, bool suprascrie) {
        if (Gol(anaf))
            // ANAF tace pe câmpul ăsta: nu ștergem ce avem. Un „" de la ANAF
            // înseamnă „nu am data asta", nu „valoarea corectă e goală" —
            // răspunsurile reale sunt pline de string-uri goale.
            return;
        var valoare = anaf.Trim();
        if (Lungimi.TryGetValue(camp, out var maxim) && valoare.Length > maxim) {
            r.Avertismente.Add($"{camp}: valoarea ANAF are {valoare.Length} caractere, coloana {maxim} "
                + $"(lungimea SAF-T) — tăiată la „{valoare[..maxim]}”.");
            valoare = valoare[..maxim];
        }
        if (Gol(cules)) {
            scrie(valoare);
            r.Modificari.Add(new ModificareCamp(camp, null, valoare));
            return;
        }
        // Egalitate ORDINALĂ după trim și pliere de spații — NU case-insensitive
        // și nici fără diacritice (D15-D3): „STR. AVRAM IANCU" vs. „Str. Avram
        // Iancu" e o diferență reală în felul în care ANAF își normalizează
        // adresele, și se raportează, nu se ascunde.
        if (Egal(cules, valoare))
            return;
        if (suprascrie) {
            scrie(valoare);
            r.Modificari.Add(new ModificareCamp(camp, cules, valoare));
        }
        else
            r.Diferente.Add(new DiferentaCamp(camp, cules, valoare));
    }

    static void Judetul(RezultatSincronizare r, Partener p, AdresaAnaf adresa, bool suprascrie,
            Func<string, Judet> cautaJudet) {
        if (Gol(adresa.CodJudetAuto))
            return;
        var cod = JudeteRo.DupaCodAuto(adresa.CodJudetAuto);
        if (cod == null) {
            r.Avertismente.Add($"Județ nerezolvat: ANAF a întors indicativul „{adresa.CodJudetAuto}”, "
                + "care nu e în lista ISO 3166-2:RO — câmpul rămâne neschimbat.");
            return;
        }
        // Gardul pereche din `GardianEditare.VerificaPartener` refuză un județ pe
        // o adresă din afara României. Candidat înseamnă `Tara == "RO"`, deci
        // cazul e teoretic — dar dacă țara s-a schimbat între selecție și
        // scriere, mai bine un avertisment decât o entitate pe care gardianul o
        // respinge la primul commit din UI.
        if (!string.Equals(p.Tara, "RO", StringComparison.Ordinal)) {
            r.Avertismente.Add($"Județul {cod} nu se scrie: partenerul are țara „{p.Tara}”, "
                + "iar județul e al adreselor din România.");
            return;
        }
        var judet = cautaJudet?.Invoke(cod);
        if (judet == null) {
            r.Avertismente.Add($"Județ nerezolvat: nomenclatorul nu are codul {cod} "
                + "(rulați seed-ul) — câmpul rămâne neschimbat.");
            return;
        }
        var codCules = p.Judet?.Cod;
        if (p.JudetId == null && p.Judet == null) {
            Scrie(judet);
            r.Modificari.Add(new ModificareCamp(nameof(Partener.Judet), null, cod));
            return;
        }
        if (p.JudetId == judet.ID || p.Judet?.ID == judet.ID)
            return;
        if (suprascrie) {
            Scrie(judet);
            r.Modificari.Add(new ModificareCamp(nameof(Partener.Judet), codCules ?? p.JudetId?.ToString(), cod));
        }
        else
            r.Diferente.Add(new DiferentaCamp(nameof(Partener.Judet), codCules ?? p.JudetId?.ToString(), cod));

        void Scrie(Judet j) {
            // Ambele capete: navigația (pe care o citește gardianul, fiindcă pe
            // un obiect nou scalarul se fixează abia la SaveChanges) și FK-ul
            // (pe care îl citesc proiecțiile).
            p.Judet = j;
            p.JudetId = j.ID;
        }
    }

    // ================= Orchestratorul =================

    // Ce consumă calea umană (acțiunea XAF, D15-D5), REST-ul (D15-D4) și
    // Import1C (D15-D6): candidați → interogare → `Aplica` → commit PER
    // PARTENER.
    //
    // De ce un commit per partener și nu unul pe tot lotul: un partener care
    // pică la commit (constraint, gardian pe altă cale) nu are voie să anuleze
    // munca celorlalți 499. La eșec se face `Rollback` (OS-ul se reîncarcă) și
    // partenerul iese în `Sarite` cu motivul — rezultatul lui NU rămâne în
    // `Rezultate`, fiindcă ar lista modificări care nu s-au scris nicăieri.
    //
    // `osNonSecured` — ușa non-secured (58c): `DataSincronizareAnaf` e
    // server-owned, iar `GardianEditare` o refuză pe orice cale securizată.
    // Apelantul o deschide și o închide; serviciul doar comite în ea.
    public static async Task<RezultatLot> SincronizeazaAsync(IObjectSpace osNonSecured,
            PlatitorTvaClient client, IEnumerable<Guid> ids, bool suprascrie, CancellationToken ct) {
        ArgumentNullException.ThrowIfNull(osNonSecured);
        ArgumentNullException.ThrowIfNull(client);
        var rezultat = new RezultatLot();
        var selectie = Candidati(osNonSecured, ids);
        rezultat.Sarite.AddRange(selectie.Sarite);
        if (selectie.Candidati.Count == 0)
            return rezultat;

        // CUI → partenerii care îl poartă: două fișe cu același cod fiscal sunt
        // un fapt al nomenclatoarelor reale (D4-r11), și amândouă trebuie
        // actualizate dintr-un singur răspuns.
        var perCui = new Dictionary<long, List<(Guid Id, string Eticheta)>>();
        foreach (var p in selectie.Candidati) {
            var cui = CuiInterogabil(p).Value;
            if (!perCui.TryGetValue(cui, out var lista))
                perCui[cui] = lista = [];
            lista.Add((p.ID, Eticheta(p)));
        }

        // Ziua interogării = ziua LOCALĂ (registrul e românesc și se citește „la
        // data de"); timbrul rămâne UTC, ca orice `DateTime` persistat.
        var raspuns = await client.Interogheaza([.. perCui.Keys], DateOnly.FromDateTime(DateTime.Today), ct)
            .ConfigureAwait(false);
        rezultat.Erori.AddRange(raspuns.Erori);

        var dateDupaCui = new Dictionary<long, DateAnaf>();
        foreach (var d in raspuns.Gasiti)
            dateDupaCui[d.Cui] = d;
        var negasite = raspuns.Negasiti.ToHashSet();

        // Cache de județe în ACELAȘI ObjectSpace în care se scrie partenerul —
        // altfel EF ar primi o entitate dintr-un alt context.
        var judete = new Dictionary<string, Judet>(StringComparer.Ordinal);
        Judet CautaJudet(string cod) {
            if (!judete.TryGetValue(cod, out var j))
                judete[cod] = j = osNonSecured.FirstOrDefault<Judet>(x => x.Cod == cod);
            return j;
        }

        var acum = DateTime.UtcNow;
        foreach (var (cui, parteneri) in perCui) {
            var gasit = dateDupaCui.TryGetValue(cui, out var date);
            if (!gasit && !negasite.Contains(cui)) {
                // CUI-ul n-a ajuns nici în `found`, nici în `notFound`: lotul lui
                // a eșuat. Partenerul e NEATINS — se spune, nu se tace.
                foreach (var (id, eticheta) in parteneri)
                    rezultat.Sarite.Add(new Sarit(id, eticheta,
                        $"lotul ANAF al CUI-ului {cui} a eșuat — partenerul e neatins"));
                continue;
            }
            foreach (var (id, eticheta) in parteneri) {
                // Re-citire prin ID: după un `Rollback` instanțele obținute
                // înainte sunt detașate.
                var p = osNonSecured.GetObjectByKey<Partener>(id);
                if (p == null) {
                    rezultat.Sarite.Add(new Sarit(id, eticheta, "partenerul a dispărut între selecție și scriere"));
                    continue;
                }
                var r = gasit ? Aplica(p, date, suprascrie, acum, CautaJudet) : Negasit(p, cui);
                try {
                    osNonSecured.CommitChanges();
                }
                catch (Exception ex) {
                    osNonSecured.Rollback();
                    rezultat.Sarite.Add(new Sarit(id, eticheta, $"salvarea a eșuat, nimic nu s-a scris: {ex.Message}"));
                    // Județele cache-uite aparțineau contextului de dinainte de
                    // Rollback — se aruncă, altfel următorul partener ar primi
                    // entități detașate.
                    judete.Clear();
                    continue;
                }
                rezultat.Rezultate.Add(r);
            }
        }
        return rezultat;
    }

    // ================= Mărunțișuri =================

    static bool Gol(string s) => string.IsNullOrWhiteSpace(s);

    static string Da(bool v) => v ? "da" : "nu";

    static string Eticheta(Partener p) => p == null ? null
        : !string.IsNullOrWhiteSpace(p.Denumire) ? p.Denumire
        : !string.IsNullOrWhiteSpace(p.Cod) ? p.Cod
        : p.ID.ToString();

    // Pliere de spații pentru comparație: „Str.  Avram   Iancu" == „Str. Avram
    // Iancu". Restul (majuscule, diacritice, punctuație) rămâne semnificativ.
    internal static bool Egal(string a, string b) => string.Equals(Pliaza(a), Pliaza(b), StringComparison.Ordinal);

    static string Pliaza(string s) {
        if (string.IsNullOrWhiteSpace(s))
            return "";
        var sb = new StringBuilder(s.Length);
        var spatiu = false;
        foreach (var ch in s.Trim()) {
            if (char.IsWhiteSpace(ch)) {
                spatiu = sb.Length > 0;
                continue;
            }
            if (spatiu) {
                sb.Append(' ');
                spatiu = false;
            }
            sb.Append(ch);
        }
        return sb.ToString();
    }
}

// ---------------- Rezultatele, ca DATE ----------------

// `Camp` poartă NUMELE proprietății (`Strada`, `Judet`, `InregistratTva`), nu o
// etichetă tradusă: e cheia pe care pasul 3 o duce în DTO și pe care clientul o
// leagă de captions-urile din `metadata.json` (42e — metadata leagă atributele,
// codul decide prezentarea).
public sealed record ModificareCamp(string Camp, string Vechi, string Nou);

public sealed record DiferentaCamp(string Camp, string Cules, string Anaf);

// Un partener necandidat sau neatins, cu motivul. `Eticheta` e denumirea (sau
// codul) — mesajul din UI n-are ce face cu un GUID.
public sealed record Sarit(Guid Id, string Eticheta, string Motiv);

public sealed record SelectieCandidati(IReadOnlyList<Partener> Candidati, IReadOnlyList<Sarit> Sarite);

public sealed class RezultatSincronizare {
    public Guid PartenerId { get; set; }
    public string Eticheta { get; set; }
    public long? Cui { get; set; }
    // `false` = CUI-ul e în `notFound` la ANAF: nimic scris, fără timbru.
    public bool Gasit { get; set; }
    public List<ModificareCamp> Modificari { get; } = [];
    public List<DiferentaCamp> Diferente { get; } = [];
    public List<string> Avertismente { get; } = [];
}

public sealed class RezultatLot {
    public List<RezultatSincronizare> Rezultate { get; } = [];
    public List<Sarit> Sarite { get; } = [];
    public List<EroareLotAnaf> Erori { get; } = [];
}
