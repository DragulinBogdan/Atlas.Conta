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

public sealed class D394Dto {
    public List<D394Operatiune> Operatiuni { get; set; } = [];
    public List<D394Rezumat> Rezumat { get; set; } = [];
    public List<D394RezumatCota> RezumatCote { get; set; } = [];
    public List<D394Neinclus> Neincluse { get; set; } = [];
    public List<string> Avertismente { get; set; } = [];
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
    /// `tip_partener` (1–4) din identitatea fiscală a partenerului (D4-D1):
    /// PF ⇒ 2; RO înregistrat ⇒ 1; RO neînregistrat ⇒ 2; UE ⇒ 3; altfel 4.
    /// `Tara` goală se citește ca RO (default-ul nomenclatorului), nu ca 4.
    /// </summary>
    public static int TipPartener(TipPersoana tipPersoana, string tara, bool inregistratTva) {
        if (tipPersoana == TipPersoana.Fizica)
            return 2;
        var cod = Partener.NormalizeazaTara(tara);
        if (cod == "RO")
            return inregistratTva ? 1 : 2;
        return TariUe.Contine(cod) ? 3 : 4;
    }

    /// <summary>
    /// `cuiP`: `CodFiscal` normalizat — trim, majuscule, spațiile interioare
    /// scoase, iar prefixul `RO` tăiat DOAR pentru partenerii din RO (codurile
    /// străine rămân întregi: `DE123…` e chiar codul de TVA din statul membru).
    /// NULL pentru cod gol.
    /// </summary>
    public static string NormalizeazaCui(string codFiscal, string tara) {
        if (string.IsNullOrWhiteSpace(codFiscal))
            return null;
        var cui = new string(codFiscal.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (Partener.NormalizeazaTara(tara) == "RO" && cui.StartsWith("RO", StringComparison.Ordinal))
            cui = cui[2..];
        return cui.Length == 0 ? null : cui;
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
        public int TipPartener;
        public bool TvaLaIncasare, PersoanaFizica;
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
        public int Cota;
        public int NrFact, Randuri;
        public bool PersoanaFizica;
        public decimal Baza, Tva;
        public readonly SortedDictionary<string, string> Parteneri = new(StringComparer.Ordinal); // Denumire → Id
        public readonly HashSet<Guid> Documente = [];
        public readonly HashSet<(Guid, bool)> Facturi = []; // (Document × Storno) = o factură la ANAF
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
        var idsRep = agregate.Where(a => a.PartenerId != null).Select(a => a.PartenerId.Value).Distinct().ToList();
        var parteneri = os.GetObjectsQuery<Partener>()
            .Where(p => idsRep.Contains(p.ID))
            .Select(p => new { p.ID, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare })
            .ToList()
            .ToDictionary(p => p.ID, p => new InfoPartener {
                Id = p.ID,
                Denumire = p.Denumire,
                CuiP = NormalizeazaCui(p.CodFiscal, p.Tara),
                TipPartener = TipPartener(p.TipPersoana, p.Tara, p.InregistratTva),
                TvaLaIncasare = p.TvaLaIncasare,
                PersoanaFizica = p.TipPersoana == TipPersoana.Fizica
            });
        // Etichetele repartitorilor care NU sunt parteneri — pentru `Neincluse`.
        var idsNePartener = idsRep.Where(id => !parteneri.ContainsKey(id)).ToList();
        var repartitori = idsNePartener.Count == 0
            ? new Dictionary<Guid, string>()
            : os.GetObjectsQuery<Repartitor>()
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
        var clasificate = new List<(Guid DocumentId, bool Storno, InfoPartener Partener, TipOperatiuneD394 Tip, int Cota,
            decimal Baza, decimal Tva, int Randuri)>();
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
            clasificate.Add((a.DocumentId, a.Storno, partener, tip, cota, a.Baza, a.Tva, a.Randuri));
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

        // ── 4. `op1`: (tip_partener, cuiP, tip, cota) ───────────────────────
        var randuri = new Dictionary<(int, string, TipOperatiuneD394, int), Rand>();
        foreach (var c in clasificate) {
            var cheie = (c.Partener.TipPartener, c.Partener.CheieCui, c.Tip, c.Cota);
            if (!randuri.TryGetValue(cheie, out var rand))
                rand = randuri[cheie] = new Rand {
                    TipPartener = c.Partener.TipPartener, CheieCui = c.Partener.CheieCui,
                    CuiP = c.Partener.CuiP, Tip = c.Tip, Cota = c.Cota, PersoanaFizica = c.Partener.PersoanaFizica
                };
            rand.Baza += c.Baza;
            rand.Tva += c.Tva;
            rand.Randuri += c.Randuri;
            rand.Parteneri.TryAdd(c.Partener.Denumire ?? "", c.Partener.Id.ToString());
            rand.Documente.Add(c.DocumentId);
            if (rand.Facturi.Add((c.DocumentId, c.Storno))
                && castigatoare.Contains((c.DocumentId, c.Storno, c.Partener.CheieCui, c.Tip, c.Cota)))
                rand.NrFact++;
        }
        // Documentul se numără o dată per rând, dar câștigătorul lui e o COTĂ:
        // rândul cotei câștigătoare primește 1 și abia la prima apariție a
        // documentului — bucla de mai sus îl adaugă la `Documente` pe fiecare
        // rând atins și incrementează doar unde cheia e cea câștigătoare.
        // (Un document atinge un rând o singură dată după agregarea per cotă,
        // deci `Add` întoarce true exact o dată per (document, rând).)

        var ordonate = randuri.Values
            .OrderBy(r => r.TipPartener).ThenBy(r => r.CuiP ?? "￿", StringComparer.Ordinal)
            .ThenBy(r => r.Parteneri.Keys.First(), StringComparer.Ordinal)
            .ThenBy(r => r.Tip).ThenBy(r => r.Cota)
            .ToList();
        foreach (var r in ordonate) {
            var areTva = AreTva(r.Tip);
            rezultat.Operatiuni.Add(new D394Operatiune {
                TipPartener = r.TipPartener,
                CuiP = r.CuiP,
                // Denumirea: la parteneri UNIȚI pe același CUI, prima în ordine —
                // avertismentul de mai jos îi numește pe toți.
                Denumire = r.Parteneri.Keys.First(),
                Tip = r.Tip.ToString(),
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
        var av = rezultat.Avertismente;

        // Parteneri distincți uniți pe același CUI normalizat (riscul 2):
        // formularul cere unicitate pe `cuiP`, deci rândul e unul — dar
        // contabilul trebuie să știe CARE nomenclatoare s-au contopit.
        foreach (var g in randuri.Values.Where(r => r.CuiP != null && r.Parteneri.Count > 1)
                .GroupBy(r => (r.TipPartener, r.CuiP)).OrderBy(g => g.Key.TipPartener).ThenBy(g => g.Key.CuiP)) {
            var nume = g.SelectMany(r => r.Parteneri.Keys).Distinct().OrderBy(n => n, StringComparer.Ordinal);
            av.Add($"CUI {g.Key.CuiP}: partenerii {string.Join(", ", nume.Select(n => $"„{n}”"))} s-au unit pe "
                + "același cod fiscal normalizat — formularul cere un singur rând per CUI. Verificați dacă sunt "
                + "același partener scris de două ori sau două nomenclatoare cu cod greșit.");
        }
        // Tip 1 fără CUI / PF fără CNP: ANAF le respinge; noi nu ascundem cifra.
        foreach (var r in ordonate.Where(r => r.TipPartener == 1 && r.CuiP == null)
                .GroupBy(r => r.CheieCui).Select(g => g.First()))
            av.Add($"Partener „{r.Parteneri.Keys.First()}” înregistrat în scopuri de TVA (tip 1) fără cod fiscal — "
                + "rândurile lui se declară, dar formularul cere `cuiP` valid; completați codul în nomenclator.");
        // PF fără CNP valid: gol SAU alt format decât 13 cifre — în sursa reală
        // (Import1C, pasul 4a) mii de PF au CNP-ul în alt format, copiat ca atare
        // în `CodFiscal`; rândul se declară cu ce are, avertismentul spune ce-i
        // lipsește.
        foreach (var r in ordonate.Where(r => r.TipPartener == 2 && r.PersoanaFizica && !EsteCnp(r.CuiP))
                .GroupBy(r => r.CheieCui).Select(g => g.First()))
            av.Add($"Persoană fizică „{r.Parteneri.Keys.First()}” fără CNP valid "
                + $"({(r.CuiP == null ? "cod gol" : $"„{r.CuiP}” nu are 13 cifre")}) — formularul cere CNP-ul sau numele și "
                + "adresa structurată (D4-r2, neintrodusă în model); rândul se declară cu identificatorul existent.");
        // V cu TVA ≠ 0 (date pre-F13, 70d): coloana nu există — suma se strigă.
        foreach (var r in ordonate.Where(r => !AreTva(r.Tip) && r.Tva != 0m))
            av.Add($"Rândul {r.Tip} pentru „{r.Parteneri.Keys.First()}” (CUI {r.CuiP ?? "—"}) poartă în registru "
                + $"TVA {r.Tva.ToString("N2", Ro)}, dar tipul n-are coloană de TVA în formular — cifra e a datelor "
                + "de dinaintea regulii 70a (taxarea inversă pe livrare nu produce taxă) și rămâne în afara "
                + "declarației, nu se trunchiază tăcut.");
        // Cotă ne-întreagă: rândul iese cu cota trunchiată, nu se pierde.
        foreach (var cota in coteTrunchiate)
            av.Add($"Cota {cota.ToString("0.####", Ro)}% din registru nu e întreagă — formularul acceptă doar cote întregi; "
                + $"rândurile ei se declară pe cota {decimal.Truncate(cota).ToString("0", Ro)}%, cu sumele exacte.");
        // V/C fără detaliul `op11` (categoria de bunuri + cod NC) — D4-r5.
        foreach (var tip in new[] { TipOperatiuneD394.V, TipOperatiuneD394.C }) {
            var cu = ordonate.Where(r => r.Tip == tip).ToList();
            if (cu.Count == 0)
                continue;
            av.Add($"Operațiunile {tip} (bază {cu.Sum(r => r.Baza).ToString("N2", Ro)}, {cu.Count} "
                + $"{(cu.Count == 1 ? "rând" : "rânduri")}) cer în formular detaliul pe categorii de bunuri "
                + "(`op11`: categorie + cod NC), pe care modelul nu-l are (D4-r5) — rândurile `op1` se declară, "
                + "detaliul se completează manual.");
        }
        // Combinații partener × tip pe care formularul le refuză (§4.9):
        // achizițiile de la un neînregistrat (tip 2) ar fi `N` — fără sursă azi.
        foreach (var r in ordonate.Where(r =>
                (r.TipPartener == 2 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS))
                || (r.TipPartener is 3 or 4 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS or TipOperatiuneD394.C))))
            av.Add($"Rândul {r.Tip} pentru „{r.Parteneri.Keys.First()}” (tip partener {r.TipPartener}, bază "
                + $"{r.Baza.ToString("N2", Ro)}) e o combinație pe care formularul o refuză — pentru "
                + "neînregistrați achizițiile se declară ca N (D4-r3), pentru străini doar L/LS/C. Verificați "
                + "identitatea fiscală a partenerului.");

        return rezultat;
    }
}
