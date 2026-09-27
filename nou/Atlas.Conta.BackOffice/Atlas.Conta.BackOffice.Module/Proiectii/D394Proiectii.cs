using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;



public sealed class D394Operatiune {
    public int TipPartener { get; set; }
    public string CuiP { get; set; }
    public string Denumire { get; set; }
    public string Tip { get; set; }
    public string Sens { get; set; }
    public int Cota { get; set; }
    public int NrFact { get; set; }
    public decimal Baza { get; set; }
    public decimal? Tva { get; set; }
    public decimal TvaNedeclarat { get; set; }
    public int Randuri { get; set; }
    public int Documente { get; set; }
}

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
    public int? FacturiN { get; set; }
    public decimal? BazaN { get; set; }
}

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

public sealed class D394Neinclus {
    public string Cauza { get; set; }
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public decimal Cota { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public int Randuri { get; set; }
}

public sealed class D394Avertisment {
    public string Cod { get; set; }
    public string Mesaj { get; set; }
    public int Numar { get; set; }
    public decimal? Suma { get; set; }
    public List<string> Exemple { get; set; } = [];
}

public sealed class D394Dto {
    public string VersiuneExportata { get; set; }
    public bool Rectificativa { get; set; }
    public bool PerioadaDeschisa { get; set; }
    public List<DecontTvaRand> DiferenteDeclarat { get; set; } = [];
    public List<D394Operatiune> Operatiuni { get; set; } = [];
    public List<D394Rezumat> Rezumat { get; set; } = [];
    public List<D394RezumatCota> RezumatCote { get; set; } = [];
    public List<D394Neinclus> Neincluse { get; set; } = [];
    public List<D394Avertisment> Avertismente { get; set; } = [];
    public int NrCui1 { get; set; }
    public int NrCui2 { get; set; }
    public int NrCui3 { get; set; }
    public int NrCui4 { get; set; }
}

public static class D394Proiectii {
    static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");


    public static int TipPartener(TipPersoana tipPersoana, string tara, bool inregistratTva) =>
        (int)ClasaFiscala.APartenerului(tipPersoana, tara, inregistratTva);

    public static string NormalizeazaCui(string codFiscal, string tara, bool inregistratTva) {
        if (string.IsNullOrWhiteSpace(codFiscal))
            return null;
        var cui = new string(codFiscal.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (inregistratTva || Partener.NormalizeazaTara(tara) == "RO")
            while (cui.StartsWith("RO", StringComparison.Ordinal))
                cui = cui[2..];
        return cui.Any(char.IsLetterOrDigit) ? cui : null;
    }

    public static bool EsteCnp(string cui) => cui != null && cui.Length == 13 && cui.All(char.IsDigit);

    static bool AreTva(TipOperatiuneD394 tip) =>
        tip is TipOperatiuneD394.L or TipOperatiuneD394.A or TipOperatiuneD394.AI or TipOperatiuneD394.C;

    static (int Cota, bool Trunchiat) CotaDeclarata(TipOperatiuneD394 tip, decimal cotaSnapshot) {
        if (!AreTva(tip))
            return (0, false);
        var intreg = (int)decimal.Truncate(cotaSnapshot);
        return (intreg, intreg != cotaSnapshot);
    }

    sealed class InfoPartener {
        public Guid Id;
        public string Denumire, CuiP;
        public int TipPropriu, TipPartener;
        public bool TvaLaIncasare, PersoanaFizica, Inactiv;
        public string CheieCui => CuiP ?? "#" + Id.ToString("N");
    }

    sealed class Rand {
        public int TipPartener;
        public string CheieCui, CuiP;
        public TipOperatiuneD394 Tip;
        public SensTva Sens;
        public int Cota;
        public int NrFact, Randuri;
        public decimal Baza, Tva;
        public readonly Dictionary<Guid, InfoPartener> Parteneri = [];
        public readonly HashSet<Guid> Documente = [];
        public readonly HashSet<(Guid, bool)> Facturi = []; // (Document × Storno) = o factură la ANAF
        public IEnumerable<string> Nume => Parteneri.Values.Select(p => p.Denumire ?? "").Distinct().OrderBy(n => n, StringComparer.Ordinal);
        public string Denumire => Nume.First();
        public bool PersoanaFizica => Parteneri.Values.Any(p => p.PersoanaFizica);
    }

    public static D394Dto D394(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd) {
        using var citire = Fiscale.DeschideCitirea(os);
        var rezultat = new D394Dto { VersiuneExportata = Fiscale.Versiune(os, FormularFiscal.D394, dataStart, dataEnd) };

        var agregate = Fiscale.IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd, FormularFiscal.D394)
            .GroupBy(r => new { DocumentId = r.DocumentFiscalId, Storno = r.Storno && !r.InversaTehnica, r.PartenerId, r.Sens, r.TipTvaId, r.Cota, r.Regim, r.DeImport })
            .Select(g => new {
                g.Key.DocumentId,
                g.Key.Storno,
                g.Key.PartenerId,
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Cota, g.Key.Regim, g.Key.DeImport,
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            })
            .ToList();

        var idsRep = agregate.Where(a => a.PartenerId != null).Select(a => a.PartenerId.Value).Distinct().ToList();
        var parteneri = os.GetObjectsQuery<Partener>()
            .Where(p => idsRep.Contains(p.ID))
            .Select(p => new { p.ID, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare, p.Activ })
            .ToList()
            .ToDictionary(p => p.ID, p => new InfoPartener {
                Id = p.ID,
                Denumire = p.Denumire,
                CuiP = NormalizeazaCui(p.CodFiscal, p.Tara, p.InregistratTva),
                TipPropriu = TipPartener(p.TipPersoana, p.Tara, p.InregistratTva),
                TipPartener = TipPartener(p.TipPersoana, p.Tara, p.InregistratTva),
                TvaLaIncasare = p.TvaLaIncasare,
                PersoanaFizica = p.TipPersoana == TipPersoana.Fizica,
                Inactiv = !p.Activ
            });
        var clasificariDiferite = new List<(string CuiP, List<InfoPartener> Parteneri, int Tip)>();
        foreach (var g in parteneri.Values.Where(p => p.CuiP != null).GroupBy(p => p.CuiP)) {
            var lista = g.OrderBy(p => p.Id).ToList();
            var tip = lista.Any(p => p.TipPropriu == 1) ? 1 : lista[0].TipPropriu;
            if (tip != 1 && lista.Any(p => p.TipPropriu != tip))
                clasificariDiferite.Add((g.Key, lista, tip));
            foreach (var p in lista)
                p.TipPartener = tip;
        }
        var idsNePartener = idsRep.Where(id => !parteneri.ContainsKey(id)).ToList();
        var repartitori = idsNePartener.Count == 0
            ? new Dictionary<Guid, string>()
            : os.GetObjectsQuery<Repartitor>()
                .Where(r => idsNePartener.Contains(r.ID))
                .Select(r => new { r.ID, r.Denumire })
                .ToList()
                .ToDictionary(r => r.ID, r => r.Denumire);

        var idsTip = agregate.Select(a => a.TipTvaId).Distinct().ToList();
        var etichete = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Cod, t.Denumire })
            .ToList()
            .ToDictionary(t => t.ID, t => (t.Cod, t.Denumire));

        var mapari = os.GetObjectsQuery<MapareD394>()
            .Select(m => new { m.TipTvaId, m.Sens, m.Tip })
            .ToList()
            .GroupBy(m => (m.TipTvaId, m.Sens))
            .ToDictionary(g => g.Key, g => g.First().Tip);

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

        var clasificate = new List<(Guid DocumentId, bool Storno, InfoPartener Partener, TipOperatiuneD394 Tip, SensTva Sens,
            int Cota, decimal Baza, decimal Tva, int Randuri)>();
        var coteTrunchiate = new SortedSet<decimal>();

        foreach (var a in agregate.Where(a => a.Baza != 0m || a.Tva != 0m)) {
            if (a.DeImport) {
                Neinclus(CauzaNeincludere.TipTvaNemapat, a.Sens, a.TipTvaId, a.Cota,
                    a.PartenerId, a.Baza, a.Tva, a.Randuri);
                continue;
            }
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
            if (tip == TipOperatiuneD394.A && partener.TvaLaIncasare)
                tip = TipOperatiuneD394.AI;
            var (cota, trunchiat) = CotaDeclarata(tip, a.Cota);
            if (trunchiat)
                coteTrunchiate.Add(a.Cota);
            clasificate.Add((a.DocumentId, a.Storno, partener, tip, a.Sens, cota, a.Baza, a.Tva, a.Randuri));
        }

        var castigatoare = new HashSet<(Guid, bool, string, TipOperatiuneD394, int)>();
        foreach (var g in clasificate.GroupBy(c => (c.DocumentId, c.Storno, c.Partener.CheieCui, c.Tip))) {
            var peCota = g.GroupBy(c => c.Cota)
                .Select(x => (Cota: x.Key, TvaAbs: Math.Abs(x.Sum(c => c.Tva))))
                .OrderByDescending(x => x.TvaAbs).ThenByDescending(x => x.Cota)
                .First();
            castigatoare.Add((g.Key.DocumentId, g.Key.Storno, g.Key.CheieCui, g.Key.Tip, peCota.Cota));
        }

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

        foreach (var g in ordonate.GroupBy(r => (r.TipPartener, r.Cota)).OrderBy(g => g.Key.TipPartener).ThenBy(g => g.Key.Cota)) {
            var (tp, cota) = g.Key;
            int F(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.NrFact);
            decimal B(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.Baza);
            decimal T(TipOperatiuneD394 t) => g.Where(r => r.Tip == t).Sum(r => r.Tva);
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
        rezultat.NrCui1 = ordonate.Where(r => r.TipPartener == 1).Select(r => r.CheieCui).Distinct().Count();
        rezultat.NrCui2 = ordonate.Count(r => r.TipPartener == 2);
        rezultat.NrCui3 = ordonate.Where(r => r.TipPartener == 3).Select(r => r.CheieCui).Distinct().Count();
        rezultat.NrCui4 = ordonate.Where(r => r.TipPartener == 4).Select(r => r.CheieCui).Distinct().Count();

        rezultat.Neincluse = neincluse.Values
            .OrderBy(n => n.Cauza).ThenBy(n => n.Sens).ThenBy(n => n.TipTvaCod ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.RepartitorDenumire ?? "", StringComparer.Ordinal)
            .ToList();

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
        var cuiCuRanduri = ordonate.Where(r => r.CuiP != null).Select(r => r.CuiP).ToHashSet();
        Avert(CodAvertismentD394.ClasificariDiferite,
            "Același CUI apare pe parteneri cu clasificări diferite (țară/tip persoană) și niciunul înregistrat în "
            + "scopuri de TVA — rândul a luat tipul primului partener; verificați identitatea fiscală.",
            clasificariDiferite.Where(c => cuiCuRanduri.Contains(c.CuiP)).OrderBy(c => c.CuiP, StringComparer.Ordinal)
                .Select(c => ($"CUI {c.CuiP}: parteneri cu clasificări diferite ({Nume(c.Parteneri)}) — rândul pe tip {c.Tip}", (decimal?)null))
                .ToList());
        Avert(CodAvertismentD394.Tip1FaraCui,
            "Partener înregistrat în scopuri de TVA (tip 1) fără cod fiscal — rândurile lui se declară, dar formularul "
            + "cere `cuiP` valid; completați codul în nomenclator.",
            ordonate.Where(r => r.TipPartener == 1 && r.CuiP == null).GroupBy(r => r.CheieCui)
                .Select(g => ($"„{g.First().Denumire}”: bază {N2(g.Sum(r => r.Baza))}", (decimal?)g.Sum(r => r.Baza))).ToList());
        Avert(CodAvertismentD394.PfFaraCnp,
            "Persoană fizică fără CNP valid (cod gol sau alt format decât 13 cifre) — formularul cere CNP-ul sau numele "
            + "și adresa structurată (D4-r2, neintrodusă în model); rândul se declară cu identificatorul existent.",
            ordonate.Where(r => r.TipPartener == 2 && r.PersoanaFizica && !EsteCnp(r.CuiP)).GroupBy(r => r.CheieCui)
                .Select(g => ($"„{g.First().Denumire}” ({(g.First().CuiP == null ? "cod gol" : $"„{g.First().CuiP}” nu are 13 cifre")}): bază {N2(g.Sum(r => r.Baza))}",
                    (decimal?)g.Sum(r => r.Baza))).ToList());
        Avert(CodAvertismentD394.TvaPeTipFaraColoana,
            "Rânduri pe un tip FĂRĂ coloană de TVA în formular (V/LS/AS) poartă TVA în registru — cifra e a datelor de "
            + "dinaintea regulii 70a (taxarea inversă pe livrare nu produce taxă) și rămâne în afara declarației, nu se "
            + "trunchiază tăcut (`TvaNedeclarat` pe rând).",
            ordonate.Where(r => !AreTva(r.Tip) && r.Tva != 0m)
                .Select(r => ($"{r.Tip} „{r.Denumire}” (CUI {r.CuiP ?? "—"}): TVA {N2(r.Tva)}", (decimal?)r.Tva)).ToList());
        Avert(CodAvertismentD394.CotaNeintreaga,
            "Cote din registru care nu sunt întregi — formularul acceptă doar cote întregi; rândurile lor se declară pe "
            + "cota trunchiată, cu sumele exacte.",
            coteTrunchiate.Select(c => ($"{c.ToString("0.####", Ro)}% ⇒ {decimal.Truncate(c).ToString("0", Ro)}%", (decimal?)null)).ToList());
        var perechiVc = mapari
            .Where(m => m.Value is TipOperatiuneD394.V or TipOperatiuneD394.C)
            .Select(m => m.Key).ToHashSet();
        var idsDetaliuVc = new List<(TipOperatiuneD394 Tip, Guid DetaliuId, decimal Baza)>();
        if (perechiVc.Count > 0)
            idsDetaliuVc = Fiscale.IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd, FormularFiscal.D394)
                .Select(r => new { r.DetaliuId, r.TipTvaId, r.Sens, r.Baza })
                .ToList()
                .Where(r => perechiVc.Contains((r.TipTvaId, r.Sens)))
                .Select(r => (Tip: mapari[(r.TipTvaId, r.Sens)], r.DetaliuId, r.Baza))
                .ToList();
        var codNcPeLinie = new HashSet<Guid>();
        if (idsDetaliuVc.Count > 0) {
            var ids = idsDetaliuVc.Select(x => x.DetaliuId).Distinct().ToList();
            var idsLot = os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.ID))
                .Select(d => new { d.ID, d.LotId }).ToList();
            var loturi = idsLot.Where(x => x.LotId != null).Select(x => x.LotId.Value).Distinct().ToList();
            var ncPeLot = os.GetObjectsQuery<Lot>()
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
        Avert(CodAvertismentD394.CombinatieRefuzata,
            "Combinații partener × tip de operațiune pe care formularul le refuză — pentru neînregistrați achizițiile "
            + "se declară ca N (D4-r3), pentru străini doar L/LS/C. Verificați identitatea fiscală a partenerului "
            + "(Import1C: `--reclasifica`).",
            ordonate.Where(r =>
                    (r.TipPartener == 2 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS))
                    || (r.TipPartener is 3 or 4 && r.Tip is not (TipOperatiuneD394.L or TipOperatiuneD394.LS or TipOperatiuneD394.C)))
                .Select(r => ($"{r.Tip} „{r.Denumire}” (tip partener {r.TipPartener}, CUI {r.CuiP ?? "—"}): bază {N2(r.Baza)}", (decimal?)r.Baza))
                .ToList());
        Avert(CodAvertismentD394.PartenerInactiv,
            "Partener inactiv în nomenclator cu documente operate în perioadă — rândurile lui se declară (declarația nu "
            + "depinde de viața nomenclatorului), iar identitatea lui fiscală se corectează după reactivare.",
            ordonate.SelectMany(r => r.Parteneri.Values).Where(p => p.Inactiv).DistinctBy(p => p.Id).OrderBy(p => p.Id)
                .Select(p => ($"„{p.Denumire}” (CUI {p.CuiP ?? "—"})", (decimal?)null)).ToList());

        if (TvaProiectii.LunaExacta(dataStart, dataEnd) is (int anDeclarat, int lunaDeclarata)) {
            var rectificativa = TvaProiectii.Rectificativa(os, anDeclarat, lunaDeclarata);
            rezultat.Rectificativa = rectificativa.EsteRectificativa;
            rezultat.PerioadaDeschisa = rectificativa.PerioadaDeschisa;
            rezultat.DiferenteDeclarat = rectificativa.Agregat;
        }
        return rezultat;
    }
}
