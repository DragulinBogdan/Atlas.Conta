using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// DECLARAȚIA INFORMATIVĂ 394 (OPANAF 3769/2015, modificat prin OPANAF 2194/2025)
// ca PROIECȚIE peste `RegistruTva` (felia 14, D4-D3). Aceleași cifre ca în D300,
// așezate altfel: **per partener** (tip de partener × CUI × tip de operațiune ×
// cotă), cu rezumatele formularului și regula lui proprie de numărare a
// facturilor.
//
// ═══ Trei surse, o singură trecere ═══
//   • sumele    — proiecție PURĂ peste registru (Σ `Baza`, Σ `Tva`, snapshot);
//   • tipul de operațiune (L/A/V/C/LS/AS) — POLITICĂ `MapareD394`
//     `(TipTva × Sens) → tip` (D4-D2), cu `AI` DERIVAT în cod din flag-ul
//     furnizorului (`TvaLaIncasare`);
//   • tipul de partener (1–4) și `cuiP` — FUNCȚIE a nomenclatorului `Partener`
//     (D4-D1), în cod: definiția e a legii, nu variază per client.
//
// ═══ Ce NU face ═══
// Nu produce XML-ul și nu persistă declarația (35c). Nu rotunjește la leu —
// livrează bani exacți, ca să se coasă la cent cu registrul și cu D300 (D4-D3
// pasul 6). Nu filtrează `Storno` (68): stornoul poartă `TipTva`-ul și cota
// ORIGINALĂ cu sume negative — exact cerința formularului (§5.3). Nu paginează:
// un formular nu se paginează (fără `DataSourceLoader`, ca D300).
//
// ═══ Nimic nu se pierde (D4-D4) ═══
// Fiecare grup de registru ajunge ori în `Operatiuni`, ori în `Neincluse` cu
// cauza lui — Σ pe ambele == Σ registrului, per sens, pe ambele coloane. Ce
// formularul cere și modelul nu are (op11, N, bonuri…) iese ca AVERTISMENT cu
// suma, nu ca 0 tăcut (D4-D5).

// `CauzaNeincludere` stă în `BusinessObjects/Comun/Enums.cs` — doar enum-urile de
// acolo ajung în `metadata.json` (etichetele clientului).

// Un rând `op1` al secțiunii 2 — PLAT prin construcție (deciziile 6/7).
public sealed class D394Operatiune {
    // 1–4 (cartușele C/D/E/F) — derivat din `Partener` (D4-D1).
    public int TipPartener { get; set; }
    // `CodFiscal` normalizat (trim, majuscule, prefixul RO tăiat DOAR pe RO).
    // NULL când partenerul n-are cod (tip 1 fără CUI, PF fără CNP) — rândul
    // se emite cu avertisment, nu se ascunde.
    public string CuiP { get; set; }
    public string Denumire { get; set; }
    // Enum-urile pleacă STRING (57a).
    public string Tip { get; set; }
    // Sensul rândului de registru din care vine rândul (fix 5 al review-ului):
    // cusătura per sens se face pe DATA asta, nu pe deducerea din `Tip` —
    // gardul `MapareD394.TintaPermisa(tip, sens)` garantează coerența, iar
    // proba o MĂSOARĂ pe ce a ieșit.
    public string Sens { get; set; }
    // Cota DECLARATĂ: `int(Cota)` din snapshot pentru L/A/AI/C, 0 pentru
    // V/LS/AS/N (regula formularului — §4.9).
    public int Cota { get; set; }
    // Regula 1/0 per document (§5.2): Σ `NrFact` pe rândurile unui partener ==
    // numărul facturilor lui, nu `count distinct` per rând.
    public int NrFact { get; set; }
    public decimal Baza { get; set; }
    // NULL = tipul n-are coloană de TVA în XSD (V, LS, AS, N) — niciodată 0 în
    // locul lui null (aceeași regulă ca la D300).
    public decimal? Tva { get; set; }
    // TVA-ul de registru pe un tip FĂRĂ coloană (V cu TVA ≠ 0 pe date pre-F13):
    // nu încape în `Tva` (care e null) și nu se înghite — iese în avertisment
    // ȘI stă aici, ca Σ `Operatiuni` + Σ `Neincluse` să rămână Σ registrului
    // (D4-D4). 0 pe orice rând sănătos.
    public decimal TvaNedeclarat { get; set; }
    // Rânduri de REGISTRU și documente DISTINCTE în spatele cifrei. `Documente`
    // numără documente; `NrFact` numără FACTURI la ANAF — stornoul unui document
    // e o factură de storno, deci un document operat și stornat în aceeași
    // perioadă dă `Documente = 1`, `NrFact = 2`, sume net 0.
    public int Randuri { get; set; }
    public int Documente { get; set; }
}

// Un rând `rezumat1` (cartușele C/D/E/F) per `(tip_partener, cota)`, calculat
// din `op1`. Câmpurile absente per tip de partener / cotă (§4.2) sunt NULL, nu
// 0: XSD-ul cere absența lor, iar un ecran care afișează „0" acolo minte.
public sealed class D394Rezumat {
    public int TipPartener { get; set; }
    public int Cota { get; set; }
    public int? FacturiL { get; set; }
    public decimal? BazaL { get; set; }
    public decimal? TvaL { get; set; }
    public int? FacturiLS { get; set; }
    public decimal? BazaLS { get; set; }
    public int? FacturiA { get; set; }
    public decimal? BazaA { get; set; }
    public decimal? TvaA { get; set; }
    public int? FacturiAI { get; set; }
    public decimal? BazaAI { get; set; }
    public decimal? TvaAI { get; set; }
    public int? FacturiAS { get; set; }
    public decimal? BazaAS { get; set; }
    public int? FacturiV { get; set; }
    public decimal? BazaV { get; set; }
    public int? FacturiC { get; set; }
    public decimal? BazaC { get; set; }
    public decimal? TvaC { get; set; }
    // `N` n-are sursă în registru (D4-r3): prezent (0) unde XSD-ul îl cere
    // (tip 2 × cota 0), null altfel.
    public int? FacturiN { get; set; }
    public decimal? BazaN { get; set; }
}

// Un rând `rezumat2` (cartușul H) per cotă ≠ 0: sumele de control ale
// coloanelor pe care le avem. Formularul cere ca V să intre la L și C la A
// („inclusiv cele pentru care se aplică taxarea inversă" — §4.3); V are însă
// cota declarată 0, iar cartușul H n-are rând pentru cota 0 — deci V rămâne
// numai în `op1`/`rezumat1`, iar C intră la A pe cota lui.
public sealed class D394RezumatCota {
    public int Cota { get; set; }
    public int NrFacturiL { get; set; }
    public decimal BazaL { get; set; }
    public decimal TvaL { get; set; }
    public int NrFacturiA { get; set; }
    public decimal BazaA { get; set; }
    public decimal TvaA { get; set; }
    public int NrFacturiAI { get; set; }
    public decimal BazaAI { get; set; }
    public decimal TvaAI { get; set; }
}

// Un grup de registru care NU intră în declarație, cu cauza și cifrele lui
// (D4-D4): parte din contract, nu log — un gard care tace devine capcană (62f).
public sealed class D394Neinclus {
    public string Cauza { get; set; }
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    // LEFT join: un `TipTva` șters logic lasă eticheta goală, nu scoate cifra.
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    // Snapshot-ul de pe rând — cifra care a intrat în calcul, nu cea de azi.
    public decimal Cota { get; set; }
    // Repartitorul contrapartidei, când există (`RepartitorNePartener`): ecranul
    // spune „decontul angajatului X", nu „un rând fără partener".
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public int Randuri { get; set; }
}

// Un avertisment AGREGAT per cauză (D4-D5, fix 7 al review-ului advers): pe
// baza reală un an ar produce mii de string-uri cu aceeași cauză (fiecare PF
// cu CNP în alt format, fiecare combinație refuzată) și semnalul util s-ar
// îneca. Deci un rând per cauză: descrierea o dată, numărul de cazuri, suma
// (unde are sens) și cel mult 5 exemple nominale — ecranul le pliază.
public sealed class D394Avertisment {
    // `CodAvertismentD394` ca string (57a); eticheta din metadata.
    public string Cod { get; set; }
    public string Mesaj { get; set; }
    public int Numar { get; set; }
    // NULL unde cauza n-are sumă (CUI-uri unite, cote ne-întregi).
    public decimal? Suma { get; set; }
    public List<string> Exemple { get; set; } = [];
}

public sealed class D394Dto {
    public List<D394Operatiune> Operatiuni { get; set; } = [];
    public List<D394Rezumat> Rezumat { get; set; } = [];
    public List<D394RezumatCota> RezumatCote { get; set; } = [];
    public List<D394Neinclus> Neincluse { get; set; } = [];
    public List<D394Avertisment> Avertismente { get; set; } = [];
    // `informatii@nrCui1..4`: parteneri DISTINCȚI per tip de partener (§4.2).
    // Pentru tip 2 formularul numără ÎNREGISTRĂRILE `op1` (persoanele fizice
    // fără CNP n-au cheie) — deci `NrCui2` = numărul rândurilor de tip 2.
    public int NrCui1 { get; set; }
    public int NrCui2 { get; set; }
    public int NrCui3 { get; set; }
    public int NrCui4 { get; set; }
}

public static class D394Proiectii {
    static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");

    // ── D4-D1: funcțiile nomenclatorului, publice și reutilizabile ──────────

    /// <summary>
    /// `tip_partener` (1–4) din identitatea fiscală a partenerului (D4-D1, cu
    /// fixurile 2/3 ale review-ului advers): **ÎNREGISTRAT BATE TOT** —
    /// înregistrat în scopuri de TVA în România ⇒ 1, indiferent de felul
    /// persoanei (PFA/II înregistrate) sau de țară (străin cu cod RO, art.
    /// 316); apoi PF ⇒ 2; RO neînregistrat ⇒ 2; UE ⇒ 3; altfel 4. §4.2: tip 1 =
    /// „persoane impozabile înregistrate în scopuri de TVA în România", tip 3 =
    /// „neînregistrate și care nu sunt obligate să se înregistreze".
    /// `Tara` goală se citește ca RO (default-ul nomenclatorului), nu ca 4.
    /// </summary>
    public static int TipPartener(TipPersoana tipPersoana, string tara, bool inregistratTva) {
        if (inregistratTva)
            return 1;
        if (tipPersoana == TipPersoana.Fizica)
            return 2;
        var cod = Partener.NormalizeazaTara(tara);
        if (cod == "RO")
            return 2;
        return TariUe.Contine(cod) ? 3 : 4;
    }

    /// <summary>
    /// `cuiP`: `CodFiscal` normalizat — trim, majuscule, spațiile interioare
    /// scoase, iar prefixul `RO` tăiat pentru partenerii din RO SAU înregistrați
    /// în scopuri de TVA în RO (străinul cu cod RO se declară pe tip 1 cu CUI-ul
    /// românesc fără prefix); codurile străine ale neînregistraților rămân
    /// întregi (`DE123…` e chiar codul de TVA din statul membru). NULL pentru
    /// cod gol.
    /// </summary>
    public static string NormalizeazaCui(string codFiscal, string tara, bool inregistratTva) {
        if (string.IsNullOrWhiteSpace(codFiscal))
            return null;
        var cui = new string(codFiscal.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if ((inregistratTva || Partener.NormalizeazaTara(tara) == "RO") && cui.StartsWith("RO", StringComparison.Ordinal))
            cui = cui[2..];
        // Un „cod" fără nicio literă/cifră („-", „./", „--") e un cod LIPSĂ scris
        // altfel (3.597 PF în sursa 1C au „-"): dacă ar trece, toți s-ar uni pe
        // aceeași cheie într-un singur rând op1 — capcană măsurată la smoke.
        return cui.Any(char.IsLetterOrDigit) ? cui : null;
    }

    /// <summary>CNP/NIF = exact 13 cifre (§4.9). Orice altceva pe o PF = identificator lipsă/invalid.</summary>
    public static bool EsteCnp(string cui) => cui != null && cui.Length == 13 && cui.All(char.IsDigit);

    // Tipurile cu coloană de TVA în XSD (§4.9): L, A, AI, C. V/LS/AS/N doar bază.
    static bool AreTva(TipOperatiuneD394 tip) =>
        tip is TipOperatiuneD394.L or TipOperatiuneD394.A or TipOperatiuneD394.AI or TipOperatiuneD394.C;

    // Cota declarată: 0 pentru V/LS/AS/N (regula formularului), `int(Cota)`
    // altfel. `Trunchiat` = cota snapshot nu era întreagă (avertisment).
    static (int Cota, bool Trunchiat) CotaDeclarata(TipOperatiuneD394 tip, decimal cotaSnapshot) {
        if (!AreTva(tip))
            return (0, false);
        var intreg = (int)decimal.Truncate(cotaSnapshot);
        return (intreg, intreg != cotaSnapshot);
    }

    // Identitatea fiscală a unui partener, citită PLAT de pe frunză.
    sealed class InfoPartener {
        public Guid Id;
        public string Denumire, CuiP;
        // `TipPropriu` = clasificarea NOMENCLATORULUI acestui partener;
        // `TipPartener` = tipul RÂNDULUI, decis pe CUI (fix 1 al review-ului):
        // un CUI nu poate fi simultan înregistrat și neînregistrat, deci dacă
        // vreun partener cu CUI-ul X e tip 1, X e tip 1 pentru toți; altfel,
        // clasificări diferite pe același CUI ⇒ tipul primului (pe Id) + avertisment.
        public int TipPropriu, TipPartener;
        public bool TvaLaIncasare, PersoanaFizica, Sters;
        // Cheia rândului: CUI-ul normalizat; fără cod, IDENTITATEA partenerului —
        // două PF fără CNP nu se contopesc într-un rând fiindcă amândouă au
        // codul gol.
        public string CheieCui => CuiP ?? "#" + Id.ToString("N");
    }

    // Acumulatorul unui rând `op1`.
    sealed class Rand {
        public int TipPartener;
        public string CheieCui, CuiP;
        public TipOperatiuneD394 Tip;
        public SensTva Sens;
        public int Cota;
        public int NrFact, Randuri;
        public decimal Baza, Tva;
        // Cheiat pe Id (fix 8 al review-ului): două nomenclatoare cu același CUI
        // ȘI aceeași denumire sunt tot două nomenclatoare unite.
        public readonly Dictionary<Guid, InfoPartener> Parteneri = [];
        public readonly HashSet<Guid> Documente = [];
        public readonly HashSet<(Guid, bool)> Facturi = []; // (Document × Storno) = o factură la ANAF
        public IEnumerable<string> Nume => Parteneri.Values.Select(p => p.Denumire ?? "").Distinct().OrderBy(n => n, StringComparer.Ordinal);
        public string Denumire => Nume.First();
        public bool PersoanaFizica => Parteneri.Values.Any(p => p.PersoanaFizica);
    }

    /// <summary>
    /// Declarația 394 pe o perioadă (ambele capete INCLUSIVE, pe `Data`
    /// rândului de registru — data stornării pentru storno, 25d).
    /// </summary>
    public static D394Dto D394(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd) {
        var rezultat = new D394Dto();

        // ── 1. Agregatul de registru (D4-D3 pasul 1) ────────────────────────
        // Cheia coboară la DOCUMENT fiindcă regula nrFact (§5.2) se decide per
        // factură, nu per rând; `Cota` e snapshot și rămâne în cheie din același
        // motiv ca la D300. `Storno` nu se filtrează (suma algebrică e adevărul, iar
        // stornoul cade pe cota originală) dar INTRĂ în cheie: la ANAF factura și
        // factura de storno sunt DOUĂ facturi, iar la noi stornoul stă pe același
        // `DocumentId` (25d) — unitatea de numărare nrFact e (Document × Storno).
        var agregate = os.GetObjectsQuery<RegistruTva>()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd)
            .GroupBy(r => new { r.DocumentId, r.Storno, r.PartenerId, r.Sens, r.TipTvaId, r.Cota })
            .Select(g => new {
                g.Key.DocumentId,
                g.Key.Storno,
                g.Key.PartenerId,
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Cota,
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            })
            .ToList();

        // ── 2. Clasificarea (D4-D3 pasul 2) ─────────────────────────────────
        // Partenerul: join pe FRUNZA `Partener` (TPT), nu cast pe navigația lazy
        // `Repartitor` (riscul 6). Un `PartenerId` care nu se regăsește aici e
        // un repartitor de alt fel (Angajatul de pe DEC) ⇒ `Neincluse`.
        // `IgnoreQueryFilters` (fix 6 al review-ului): facturile unui partener
        // ȘTERS logic din nomenclator se declară — sunt documente operate;
        // declarația nu depinde de viața nomenclatorului. Rândul lui iese cu
        // avertisment `PartenerSters`; cauza `RepartitorNePartener` rămâne doar
        // pentru repartitorii care chiar nu sunt parteneri (Angajatul de pe DEC).
        var idsRep = agregate.Where(a => a.PartenerId != null).Select(a => a.PartenerId.Value).Distinct().ToList();
        var parteneri = os.GetObjectsQuery<Partener>().IgnoreQueryFilters()
            .Where(p => idsRep.Contains(p.ID))
            .Select(p => new { p.ID, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare, p.GCRecord })
            .ToList()
            .ToDictionary(p => p.ID, p => new InfoPartener {
                Id = p.ID,
                Denumire = p.Denumire,
                CuiP = NormalizeazaCui(p.CodFiscal, p.Tara, p.InregistratTva),
                TipPropriu = TipPartener(p.TipPersoana, p.Tara, p.InregistratTva),
                TipPartener = TipPartener(p.TipPersoana, p.Tara, p.InregistratTva),
                TvaLaIncasare = p.TvaLaIncasare,
                PersoanaFizica = p.TipPersoana == TipPersoana.Fizica,
                Sters = p.GCRecord != 0
            });
        // Tipul rândului se decide PE CUI, peste nomenclatoare (fix 1): „înregistrat
        // bate tot"; fără niciun înregistrat, clasificări diferite ⇒ tipul primului
        // (ordonat pe Id) și avertisment. Rândurile fără CUI rămân pe identitatea
        // partenerului, cu tipul lui propriu.
        var clasificariDiferite = new List<(string CuiP, List<InfoPartener> Parteneri, int Tip)>();
        foreach (var g in parteneri.Values.Where(p => p.CuiP != null).GroupBy(p => p.CuiP)) {
            var lista = g.OrderBy(p => p.Id).ToList();
            var tip = lista.Any(p => p.TipPropriu == 1) ? 1 : lista[0].TipPropriu;
            if (tip != 1 && lista.Any(p => p.TipPropriu != tip))
                clasificariDiferite.Add((g.Key, lista, tip));
            foreach (var p in lista)
                p.TipPartener = tip;
        }
        // Etichetele repartitorilor care NU sunt parteneri — pentru `Neincluse`.
        var idsNePartener = idsRep.Where(id => !parteneri.ContainsKey(id)).ToList();
        var repartitori = idsNePartener.Count == 0
            ? new Dictionary<Guid, string>()
            : os.GetObjectsQuery<Repartitor>().IgnoreQueryFilters()
                .Where(r => idsNePartener.Contains(r.ID))
                .Select(r => new { r.ID, r.Denumire })
                .ToList()
                .ToDictionary(r => r.ID, r => r.Denumire);

        // Etichetele tipurilor de TVA, LEFT (ca la D300).
        var idsTip = agregate.Select(a => a.TipTvaId).Distinct().ToList();
        var etichete = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Cod, t.Denumire })
            .ToList()
            .ToDictionary(t => t.ID, t => (t.Cod, t.Denumire));

        // Politica: o singură mapare per pereche (index unic filtrat).
        var mapari = os.GetObjectsQuery<MapareD394>()
            .Select(m => new { m.TipTvaId, m.Sens, m.Tip })
            .ToList()
            .GroupBy(m => (m.TipTvaId, m.Sens))
            .ToDictionary(g => g.Key, g => g.First().Tip);

        // Grupurile neincluse se strâng pe (cauză, sens, tip, cotă, repartitor).
        var neincluse = new Dictionary<(CauzaNeincludere, SensTva, Guid, decimal, Guid?), D394Neinclus>();
        void Neinclus(CauzaNeincludere cauza, SensTva sens, Guid tipTvaId, decimal cota, Guid? repId,
            decimal baza, decimal tva, int randuri) {
            var cheie = (cauza, sens, tipTvaId, cota, repId);
            if (!neincluse.TryGetValue(cheie, out var n)) {
                var eticheta = etichete.TryGetValue(tipTvaId, out var e) ? e : (Cod: null, Denumire: null);
                n = neincluse[cheie] = new D394Neinclus {
                    Cauza = cauza.ToString(),
                    Sens = sens.ToString(),
                    TipTvaId = tipTvaId,
                    TipTvaCod = eticheta.Cod,
                    TipTvaDenumire = eticheta.Denumire,
                    Cota = cota,
                    RepartitorId = repId,
                    RepartitorDenumire = repId is Guid rid ? repartitori.GetValueOrDefault(rid) : null
                };
            }
            n.Baza += baza;
            n.Tva += tva;
            n.Randuri += randuri;
        }

        // Grupul clasificat, încă la granularitatea DOCUMENTULUI — pasul 3 are
        // nevoie de el așa.
        var clasificate = new List<(Guid DocumentId, bool Storno, InfoPartener Partener, TipOperatiuneD394 Tip, SensTva Sens,
            int Cota, decimal Baza, decimal Tva, int Randuri)>();
        var coteTrunchiate = new SortedSet<decimal>();

        foreach (var a in agregate) {
            if (a.PartenerId is not Guid pid) {
                Neinclus(CauzaNeincludere.FaraPartener, a.Sens, a.TipTvaId, a.Cota, null, a.Baza, a.Tva, a.Randuri);
                continue;
            }
            if (!parteneri.TryGetValue(pid, out var partener)) {
                Neinclus(CauzaNeincludere.RepartitorNePartener, a.Sens, a.TipTvaId, a.Cota, pid, a.Baza, a.Tva, a.Randuri);
                continue;
            }
            if (!mapari.TryGetValue((a.TipTvaId, a.Sens), out var tip)) {
                Neinclus(CauzaNeincludere.TipTvaNemapat, a.Sens, a.TipTvaId, a.Cota, null, a.Baza, a.Tva, a.Randuri);
                continue;
            }
            // D4-D2: `A` devine `AI` la furnizorul în sistemul TVA la încasare.
            // Doar A: C (taxarea inversă) n-are variantă „la încasare".
            if (tip == TipOperatiuneD394.A && partener.TvaLaIncasare)
                tip = TipOperatiuneD394.AI;
            var (cota, trunchiat) = CotaDeclarata(tip, a.Cota);
            if (trunchiat)
                coteTrunchiate.Add(a.Cota);
            clasificate.Add((a.DocumentId, a.Storno, partener, tip, a.Sens, cota, a.Baza, a.Tva, a.Randuri));
        }

        // ── 3. Numărul de facturi, per document (§5.2) ──────────────────────
        // Un document se numără 1 pe cota cu TVA-ul (absolut) maxim și 0 pe
        // celelalte; la egalitate, cota mai mare. Per (document × partener ×
        // tip): documentul cu L și V se numără separat pe fiecare tip (1 + 1).
        // Stornoul e o FACTURĂ proprie (§5.3): cheia e (Document × Storno) —
        // operarea și stornarea în aceeași perioadă dau net 0 cu nrFact 2.
        var castigatoare = new HashSet<(Guid, bool, string, TipOperatiuneD394, int)>();
        foreach (var g in clasificate.GroupBy(c => (c.DocumentId, c.Storno, c.Partener.CheieCui, c.Tip))) {
            var peCota = g.GroupBy(c => c.Cota)
                .Select(x => (Cota: x.Key, TvaAbs: Math.Abs(x.Sum(c => c.Tva))))
                .OrderByDescending(x => x.TvaAbs).ThenByDescending(x => x.Cota)
                .First();
            castigatoare.Add((g.Key.DocumentId, g.Key.Storno, g.Key.CheieCui, g.Key.Tip, peCota.Cota));
        }

        // ── 4. `op1`: (cuiP, tip, cota) — tipul de partener e FUNCȚIE de cheie ──
        // `TipPartener` e decis pe CUI (mai sus), deci nu mai e o axă a cheii:
        // unicitatea `(cuiP, tip, cota)` cerută de XSD e garantată prin
        // construcție. `Sens` intră în cheie doar ca o mapare incoerentă (dacă ar
        // scăpa de gard) să nu contopească achiziții cu livrări.
        var randuri = new Dictionary<(string, TipOperatiuneD394, SensTva, int), Rand>();
        foreach (var c in clasificate) {
            var cheie = (c.Partener.CheieCui, c.Tip, c.Sens, c.Cota);
            if (!randuri.TryGetValue(cheie, out var rand))
                rand = randuri[cheie] = new Rand {
                    TipPartener = c.Partener.TipPartener, CheieCui = c.Partener.CheieCui,
                    CuiP = c.Partener.CuiP, Tip = c.Tip, Sens = c.Sens, Cota = c.Cota
                };
            rand.Baza += c.Baza;
            rand.Tva += c.Tva;
            rand.Randuri += c.Randuri;
            rand.Parteneri.TryAdd(c.Partener.Id, c.Partener);
            rand.Documente.Add(c.DocumentId);
            if (rand.Facturi.Add((c.DocumentId, c.Storno))
                && castigatoare.Contains((c.DocumentId, c.Storno, c.Partener.CheieCui, c.Tip, c.Cota)))
                rand.NrFact++;
        }
        // `Facturi` e memoria „am văzut factura asta pe rândul ăsta": o factură
        // atinge același rând prin mai multe grupuri când are două `TipTva` pe
        // aceeași cotă (N21 + NED21), iar incrementul vine o singură dată, doar
        // unde cota rândului e cea câștigătoare.

        var ordonate = randuri.Values
            .OrderBy(r => r.TipPartener).ThenBy(r => r.CuiP ?? "￿", StringComparer.Ordinal)
            .ThenBy(r => r.Denumire, StringComparer.Ordinal)
            .ThenBy(r => r.Tip).ThenBy(r => r.Cota)
            .ToList();
        foreach (var r in ordonate) {
            var areTva = AreTva(r.Tip);
            rezultat.Operatiuni.Add(new D394Operatiune {
                TipPartener = r.TipPartener,
                CuiP = r.CuiP,
                // Denumirea: la parteneri UNIȚI pe același CUI, prima în ordine —
                // avertismentul de mai jos îi numește pe toți.
                Denumire = r.Denumire,
                Tip = r.Tip.ToString(),
                Sens = r.Sens.ToString(),
                Cota = r.Cota,
                NrFact = r.NrFact,
                Baza = r.Baza,
                Tva = areTva ? r.Tva : null,
                TvaNedeclarat = areTva ? 0m : r.Tva,
                Randuri = r.Randuri,
                Documente = r.Documente.Count
            });
        }

        // ── 5. `rezumat1` per (tip_partener, cota) și `rezumat2` per cotă ───
        foreach (var g in ordonate.GroupBy(r => (r.TipPartener, r.Cota)).OrderBy(g => g.Key.TipPartener).ThenBy(g => g.Key.Cota)) {
            var (tp, cota) = g.Key;
            int F(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.NrFact);
            decimal B(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.Baza);
            decimal T(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.Tva);
            // Regulile de prezență din §4.2: null = XSD-ul cere absent.
            var l = cota != 0;
            var ls = cota == 0;
            var a = tp == 1 && cota != 0;
            var asV = tp == 1 && cota == 0;
            var c = tp is 1 or 3 or 4 && cota != 0;
            var n = tp == 2 && cota == 0;
            rezultat.Rezumat.Add(new D394Rezumat {
                TipPartener = tp, Cota = cota,
                FacturiL = l ? F(TipOperatiuneD394.L) : null,
                BazaL = l ? B(TipOperatiuneD394.L) : null,
                TvaL = l ? T(TipOperatiuneD394.L) : null,
                FacturiLS = ls ? F(TipOperatiuneD394.LS) : null,
                BazaLS = ls ? B(TipOperatiuneD394.LS) : null,
                FacturiA = a ? F(TipOperatiuneD394.A) : null,
                BazaA = a ? B(TipOperatiuneD394.A) : null,
                TvaA = a ? T(TipOperatiuneD394.A) : null,
                FacturiAI = a ? F(TipOperatiuneD394.AI) : null,
                BazaAI = a ? B(TipOperatiuneD394.AI) : null,
                TvaAI = a ? T(TipOperatiuneD394.AI) : null,
                FacturiAS = asV ? F(TipOperatiuneD394.AS) : null,
                BazaAS = asV ? B(TipOperatiuneD394.AS) : null,
                FacturiV = asV ? F(TipOperatiuneD394.V) : null,
                BazaV = asV ? B(TipOperatiuneD394.V) : null,
                FacturiC = c ? F(TipOperatiuneD394.C) : null,
                BazaC = c ? B(TipOperatiuneD394.C) : null,
                TvaC = c ? T(TipOperatiuneD394.C) : null,
                // `N` n-are sursă (D4-r3): un ZERO adevărat acolo unde XSD-ul îl cere.
                FacturiN = n ? 0 : null,
                BazaN = n ? 0m : null,
            });
        }
        foreach (var g in ordonate.Where(r => r.Cota != 0).GroupBy(r => r.Cota).OrderBy(g => g.Key)) {
            int F(params TipOperatiuneD394[] t) => g.Where(r => t.Contains(r.Tip)).Sum(r => r.NrFact);
            decimal B(params TipOperatiuneD394[] t) => g.Where(r => t.Contains(r.Tip)).Sum(r => r.Baza);
            decimal T(params TipOperatiuneD394[] t) => g.Where(r => t.Contains(r.Tip)).Sum(r => r.Tva);
            rezultat.RezumatCote.Add(new D394RezumatCota {
                Cota = g.Key,
                NrFacturiL = F(TipOperatiuneD394.L, TipOperatiuneD394.V),
                BazaL = B(TipOperatiuneD394.L, TipOperatiuneD394.V),
                TvaL = T(TipOperatiuneD394.L),
                NrFacturiA = F(TipOperatiuneD394.A, TipOperatiuneD394.C),
                BazaA = B(TipOperatiuneD394.A, TipOperatiuneD394.C),
                TvaA = T(TipOperatiuneD394.A, TipOperatiuneD394.C),
                NrFacturiAI = F(TipOperatiuneD394.AI),
                BazaAI = B(TipOperatiuneD394.AI),
                TvaAI = T(TipOperatiuneD394.AI),
            });
        }
        // `nrCui`: persoane DISTINCTE (tip 1/3/4), înregistrări (tip 2) — §4.2.
        rezultat.NrCui1 = ordonate.Where(r => r.TipPartener == 1).Select(r => r.CheieCui).Distinct().Count();
        rezultat.NrCui2 = ordonate.Count(r => r.TipPartener == 2);
        rezultat.NrCui3 = ordonate.Where(r => r.TipPartener == 3).Select(r => r.CheieCui).Distinct().Count();
        rezultat.NrCui4 = ordonate.Where(r => r.TipPartener == 4).Select(r => r.CheieCui).Distinct().Count();

        rezultat.Neincluse = neincluse.Values
            .OrderBy(n => n.Cauza).ThenBy(n => n.Sens).ThenBy(n => n.TipTvaCod ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.RepartitorDenumire ?? "", StringComparer.Ordinal)
            .ToList();

        // ── 6. Avertismentele (D4-D5): se RAPORTEAZĂ, nu se inventează ──────
        // AGREGATE per cauză (fix 7): un rând per cod, cu numărul de cazuri, suma
        // (unde are sens) și cel mult 5 exemple nominale.
        void Avert(CodAvertismentD394 cod, string mesaj, IReadOnlyList<(string Exemplu, decimal? Suma)> cazuri) {
            if (cazuri.Count == 0)
                return;
            rezultat.Avertismente.Add(new D394Avertisment {
                Cod = cod.ToString(),
                Mesaj = mesaj,
                Numar = cazuri.Count,
                Suma = cazuri.Any(c => c.Suma != null) ? cazuri.Sum(c => c.Suma ?? 0m) : null,
                Exemple = cazuri.Take(5).Select(c => c.Exemplu).ToList()
            });
        }
        string N2(decimal v) => v.ToString("N2", Ro);
        string Nume(IEnumerable<InfoPartener> parts) =>
            string.Join(", ", parts.Select(p => $"„{p.Denumire}” (tip {p.TipPropriu})"));

        // Parteneri distincți uniți pe același CUI normalizat (riscul 2), peste
        // TIPURI (fix 1): formularul cere unicitate pe `cuiP`, deci rândul e unul
        // — dar contabilul trebuie să știe CARE nomenclatoare s-au contopit.
        var peCui = ordonate.Where(r => r.CuiP != null).GroupBy(r => r.CuiP)
            .Select(g => (CuiP: g.Key, Parteneri: g.SelectMany(r => r.Parteneri.Values).DistinctBy(p => p.Id).OrderBy(p => p.Id).ToList(),
                Tip: g.First().TipPartener))
            .Where(g => g.Parteneri.Count > 1)
            .OrderBy(g => g.CuiP, StringComparer.Ordinal)
            .ToList();
        Avert(CodAvertismentD394.CuiUnit,
            "Parteneri distincți s-au unit pe același cod fiscal normalizat — formularul cere un singur rând per CUI, "
            + "iar tipul de partener al rândului e cel al partenerului înregistrat în scopuri de TVA (dacă există). "
            + "Verificați dacă sunt același partener scris de două ori sau două nomenclatoare cu cod greșit.",
            peCui.Select(g => ($"CUI {g.CuiP} (rând tip {g.Tip}): {Nume(g.Parteneri)}", (decimal?)null)).ToList());
        // Același CUI cu clasificări diferite și niciun înregistrat: rândul a luat
        // tipul primului — decizia se strigă, nu se ia tăcut.
        var cuiCuRanduri = ordonate.Where(r => r.CuiP != null).Select(r => r.CuiP).ToHashSet();
        Avert(CodAvertismentD394.ClasificariDiferite,
            "Același CUI apare pe parteneri cu clasificări diferite (țară/tip persoană) și niciunul înregistrat în "
            + "scopuri de TVA — rândul a luat tipul primului partener; verificați identitatea fiscală.",
            clasificariDiferite.Where(c => cuiCuRanduri.Contains(c.CuiP)).OrderBy(c => c.CuiP, StringComparer.Ordinal)
                .Select(c => ($"CUI {c.CuiP}: parteneri cu clasificări diferite ({Nume(c.Parteneri)}) — rândul pe tip {c.Tip}", (decimal?)null))
                .ToList());
        // Tip 1 fără CUI / PF fără CNP: ANAF le respinge; noi nu ascundem cifra.
        Avert(CodAvertismentD394.Tip1FaraCui,
            "Partener înregistrat în scopuri de TVA (tip 1) fără cod fiscal — rândurile lui se declară, dar formularul "
            + "cere `cuiP` valid; completați codul în nomenclator.",
            ordonate.Where(r => r.TipPartener == 1 && r.CuiP == null).GroupBy(r => r.CheieCui)
                .Select(g => ($"„{g.First().Denumire}”: bază {N2(g.Sum(r => r.Baza))}", (decimal?)g.Sum(r => r.Baza))).ToList());
        // PF fără CNP valid: gol SAU alt format decât 13 cifre — în sursa reală
        // (Import1C, pasul 4a) mii de PF au CNP-ul în alt format, copiat ca atare
        // în `CodFiscal`; rândul se declară cu ce are, avertismentul spune ce-i
        // lipsește.
        Avert(CodAvertismentD394.PfFaraCnp,
            "Persoană fizică fără CNP valid (cod gol sau alt format decât 13 cifre) — formularul cere CNP-ul sau numele "
            + "și adresa structurată (D4-r2, neintrodusă în model); rândul se declară cu identificatorul existent.",
            ordonate.Where(r => r.TipPartener == 2 && r.PersoanaFizica && !EsteCnp(r.CuiP)).GroupBy(r => r.CheieCui)
                .Select(g => ($"„{g.First().Denumire}” ({(g.First().CuiP == null ? "cod gol" : $"„{g.First().CuiP}” nu are 13 cifre")}): bază {N2(g.Sum(r => r.Baza))}",
                    (decimal?)g.Sum(r => r.Baza))).ToList());
        // V cu TVA ≠ 0 (date pre-F13, 70d): coloana nu există — suma se strigă.
        Avert(CodAvertismentD394.TvaPeTipFaraColoana,
            "Rânduri pe un tip FĂRĂ coloană de TVA în formular (V/LS/AS) poartă TVA în registru — cifra e a datelor de "
            + "dinaintea regulii 70a (taxarea inversă pe livrare nu produce taxă) și rămâne în afara declarației, nu se "
            + "trunchiază tăcut (`TvaNedeclarat` pe rând).",
            ordonate.Where(r => !AreTva(r.Tip) && r.Tva != 0m)
                .Select(r => ($"{r.Tip} „{r.Denumire}” (CUI {r.CuiP ?? "—"}): TVA {N2(r.Tva)}", (decimal?)r.Tva)).ToList());
        // Cotă ne-întreagă: rândul iese cu cota trunchiată, nu se pierde.
        Avert(CodAvertismentD394.CotaNeintreaga,
            "Cote din registru care nu sunt întregi — formularul acceptă doar cote întregi; rândurile lor se declară pe "
            + "cota trunchiată, cu sumele exacte.",
            coteTrunchiate.Select(c => ($"{c.ToString("0.####", Ro)}% ⇒ {decimal.Truncate(c).ToString("0", Ro)}%", (decimal?)null)).ToList());
        // V/C fără detaliul `op11` (categoria de bunuri + cod NC) — D4-r5,
        // RESTRÂNS de D4-r14 (felia 16): jumătate din detaliu — codul NC — există
        // acum pe `Produs` (D16-D2), deci avertismentul nu mai e despre TOATE
        // rândurile de taxare inversă, ci doar despre LINIILE care chiar n-au
        // codul. O factură de taxare inversă cu produse codificate NC nu mai
        // strigă degeaba; una cu servicii sau cu produse necodificate, da.
        //
        // Se citește la nivel de LINIE (`DetaliuId`), nu de rând `op1`: `op11` e
        // detaliul pe categorii de bunuri al facturii, iar codul NC stă pe produsul
        // liniei. Ce rămâne deschis (restanța D4-r5 propriu-zisă) e CATEGORIA de
        // bunuri și structura `op11` însăși — vezi mesajul.
        var perechiVc = mapari
            .Where(m => m.Value is TipOperatiuneD394.V or TipOperatiuneD394.C)
            .Select(m => m.Key).ToHashSet();
        var idsDetaliuVc = new List<(TipOperatiuneD394 Tip, Guid DetaliuId, decimal Baza)>();
        if (perechiVc.Count > 0)
            idsDetaliuVc = os.GetObjectsQuery<RegistruTva>()
                .Where(r => r.Data >= dataStart && r.Data <= dataEnd)
                .Select(r => new { r.DetaliuId, r.TipTvaId, r.Sens, r.Baza })
                .ToList()
                .Where(r => perechiVc.Contains((r.TipTvaId, r.Sens)))
                .Select(r => (Tip: mapari[(r.TipTvaId, r.Sens)], r.DetaliuId, r.Baza))
                .ToList();
        var codNcPeLinie = new HashSet<Guid>();
        if (idsDetaliuVc.Count > 0) {
            var ids = idsDetaliuVc.Select(x => x.DetaliuId).Distinct().ToList();
            // Produsul liniei: pe frunzele de factură e explicit, pe restul se
            // citește prin lot (`Lot.Produs`). Un query per sursă, nu per linie.
            var idsLot = os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.ID))
                .Select(d => new { d.ID, d.LotId }).ToList();
            var loturi = idsLot.Where(x => x.LotId != null).Select(x => x.LotId.Value).Distinct().ToList();
            var ncPeLot = os.GetObjectsQuery<Lot>().IgnoreQueryFilters()
                .Where(l => loturi.Contains(l.ID))
                .Select(l => new { l.ID, l.Produs.CodNc }).ToList()
                .ToDictionary(l => l.ID, l => l.CodNc);
            foreach (var x in idsLot)
                if (x.LotId is Guid lot && ncPeLot.TryGetValue(lot, out var nc) && !string.IsNullOrWhiteSpace(nc))
                    codNcPeLinie.Add(x.ID);
            foreach (var x in os.GetObjectsQuery<FacturaIntrareDetaliu>().Where(d => ids.Contains(d.ID))
                         .Select(d => new { d.ID, CodNc = d.Produs.CodNc }).ToList())
                if (!string.IsNullOrWhiteSpace(x.CodNc)) codNcPeLinie.Add(x.ID);
            foreach (var x in os.GetObjectsQuery<FacturaIesireDetaliu>().Where(d => ids.Contains(d.ID))
                         .Select(d => new { d.ID, CodNc = d.Produs.CodNc }).ToList())
                if (!string.IsNullOrWhiteSpace(x.CodNc)) codNcPeLinie.Add(x.ID);
        }
        Avert(CodAvertismentD394.FaraOp11,
            "Operațiunile V și C cer în formular detaliul pe categorii de bunuri (`op11`: categorie + cod NC). "
            + "Codul NC există pe `Produs` (felia 16), deci se strigă DOAR liniile care n-au produs codificat; "
            + "categoria de bunuri și structura `op11` rămân de completat manual (D4-r5).",
            idsDetaliuVc.Where(x => !codNcPeLinie.Contains(x.DetaliuId))
                .GroupBy(x => x.Tip)
                .OrderBy(g => g.Key)
                .Select(g => ($"{g.Key}: bază {N2(g.Sum(x => x.Baza))} ({g.Count()} "
                        + $"{(g.Count() == 1 ? "linie" : "linii")} fără cod NC)", (decimal?)g.Sum(x => x.Baza)))
                .ToList());
        // Combinații partener × tip pe care formularul le refuză (§4.9):
        // achizițiile de la un neînregistrat (tip 2) ar fi `N` — fără sursă azi.
        Avert(CodAvertismentD394.CombinatieRefuzata,
            "Combinații partener × tip de operațiune pe care formularul le refuză — pentru neînregistrați achizițiile "
            + "se declară ca N (D4-r3), pentru străini doar L/LS/C. Verificați identitatea fiscală a partenerului "
            + "(Import1C: `--reclasifica`).",
            ordonate.Where(r =>
                    (r.TipPartener == 2 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS))
                    || (r.TipPartener is 3 or 4 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS or TipOperatiuneD394.C)))
                .Select(r => ($"{r.Tip} „{r.Denumire}” (tip partener {r.TipPartener}, CUI {r.CuiP ?? "—"}): bază {N2(r.Baza)}", (decimal?)r.Baza))
                .ToList());
        // Partener șters logic din nomenclator (fix 6): rândul se declară, dar
        // nomenclatorul nu-l mai arată — contabilul trebuie să știe.
        Avert(CodAvertismentD394.PartenerSters,
            "Partener șters din nomenclator cu documente operate în perioadă — rândurile lui se declară (declarația nu "
            + "depinde de viața nomenclatorului), dar identitatea lui fiscală nu se mai poate corecta decât după "
            + "restaurare.",
            ordonate.SelectMany(r => r.Parteneri.Values).Where(p => p.Sters).DistinctBy(p => p.Id).OrderBy(p => p.Id)
                .Select(p => ($"„{p.Denumire}” (CUI {p.CuiP ?? "—"})", (decimal?)null)).ToList());

        return rezultat;
    }
}
