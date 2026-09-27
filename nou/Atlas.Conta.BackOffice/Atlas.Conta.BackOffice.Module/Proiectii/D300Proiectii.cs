using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;


public sealed record ParametriD300(
    decimal SoldPlataPrecedent = 0m,
    decimal DiferentePlata = 0m,
    decimal SoldNegativPrecedent = 0m,
    decimal DiferenteNegative = 0m);

public sealed class D300Rand {
    public string Cod { get; set; }
    public string Denumire { get; set; }
    public string Sectiune { get; set; }
    public string Fel { get; set; }
    public int Nivel { get; set; }
    public int Ordine { get; set; }
    public decimal? Baza { get; set; }
    public decimal? Tva { get; set; }
    public int Randuri { get; set; }
    public string Surse { get; set; }
}

public sealed class D300Nemapat {
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public int Randuri { get; set; }
}

public sealed class D300Dto {
    public string VersiuneExportata { get; set; }
    public List<D300Rand> Randuri { get; set; } = [];
    public List<D300Nemapat> Nemapate { get; set; } = [];
    public List<string> Avertismente { get; set; } = [];
    public bool Rectificativa { get; set; }
    public bool PerioadaDeschisa { get; set; }
    public List<DecontTvaRand> DiferenteDeclarat { get; set; } = [];
}

public static class D300Proiectii {
    static readonly DateOnly PrimaPerioada2026 = new(2026, 1, 1);

    static readonly CultureInfo Ro = CultureInfo.GetCultureInfo("ro-RO");

    static readonly string[] OperanziRd19 =
        ["1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18"];
    static readonly string[] OperanziRd30 =
        ["20", "21", "22", "23", "24", "25", "26", "27", "28"];

    static readonly string[] CoduriFormule =
        ["19", "30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "40", "41", "42", "43", "44", "45"];

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

    public static D300Dto D300(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd,
        ParametriD300 externi) {
        externi ??= new ParametriD300();
        using var citire = Fiscale.DeschideCitirea(os);
        var rezultat = new D300Dto { VersiuneExportata = Fiscale.Versiune(os, FormularFiscal.D300, dataStart, dataEnd) };

        var agregate = Fiscale.IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd)
            .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota, r.DeImport, r.RegularizareD300 })
            .Select(g => new {
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Regim,
                g.Key.Cota, g.Key.DeImport, g.Key.RegularizareD300,
                Autocolectare = g.Sum(r => r.Autocolectare),
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            })
            .ToList();

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
        var dupaCod = noduri.Where(n => n.Cod != null)
            .GroupBy(n => n.Cod).ToDictionary(g => g.Key, g => g.First());

        foreach (var nod in noduri) {
            var vizitate = new HashSet<Guid> { nod.Id };
            var parinte = nod.ParinteId;
            while (parinte is Guid pid && dupaId.TryGetValue(pid, out var sus) && vizitate.Add(pid)) {
                nod.Nivel++;
                parinte = sus.ParinteId;
            }
        }

        var idsTip = agregate.Select(a => a.TipTvaId).Distinct().ToList();
        var etichete = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Cod, t.Denumire })
            .ToList()
            .ToDictionary(t => t.ID, t => (t.Cod, t.Denumire));

        var mapari = os.GetObjectsQuery<MapareD300>()
            .Select(m => new { m.TipTvaId, m.Sens, m.RandId, m.TipTva.Regim, m.TipTva.Cota, m.TipTva.DeImport })
            .ToList();

        var pierdutTva = new Dictionary<Guid, decimal>();
        var pierdutBaza = new Dictionary<Guid, decimal>();
        var nedeductibil = 0m;

        foreach (var a in agregate) {
            var eticheta = etichete.TryGetValue(a.TipTvaId, out var e) ? e : (Cod: null, Denumire: null);
            var tinte = mapari
                .Where(m => m.TipTvaId == a.TipTvaId && m.Sens == a.Sens
                    && m.Regim == a.Regim && m.Cota == a.Cota && m.DeImport == a.DeImport)
                .Select(m => m.RandId).Distinct()
                .Select(id => dupaId.GetValueOrDefault(id))
                .Where(n => n != null)
                .ToList();
            if (a.RegularizareD300) {
                var cod = a.Sens == SensTva.Livrare ? "16" : "33";
                tinte = dupaCod.TryGetValue(cod, out var regularizare) ? [regularizare] : [];
                if (a.Regim == RegimTva.TaxareInversa && a.Sens == SensTva.Achizitie
                        && dupaCod.TryGetValue("16", out var colectata)) tinte.Add(colectata);
            }
            else if (a.Regim == RegimTva.TaxareInversa && a.Sens == SensTva.Achizitie) {
                var iduri = tinte.Select(t => t.Id).ToHashSet();
                tinte.AddRange(noduri.Where(n => n.OglindaAId is Guid id && iduri.Contains(id)));
                tinte = tinte.DistinctBy(n => n.Id).ToList();
            }


            if (tinte.Count == 0) {
                rezultat.Nemapate.Add(new D300Nemapat {
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

            if (a.Regim == RegimTva.Capitalizat)
                nedeductibil += a.Tva
                    * tinte.Count(n => n.AreTva && OperanziRd30.Contains(n.Cod));

            foreach (var tinta in tinte) {
                if (tinta.AreBaza)
                    tinta.Baza += a.Baza;
                else if (a.Baza != 0m)
                    pierdutBaza[tinta.Id] = pierdutBaza.GetValueOrDefault(tinta.Id) + a.Baza;
                var taxa = a.Regim == RegimTva.TaxareInversa && a.Sens == SensTva.Achizitie
                    && tinta.Sectiune == SectiuneD300.Colectata ? a.Autocolectare : a.Tva;
                if (a.RegularizareD300 && a.Regim == RegimTva.Capitalizat) taxa = 0m;
                if (tinta.AreTva)
                    tinta.Tva += taxa;
                else if (a.Tva != 0m)
                    pierdutTva[tinta.Id] = pierdutTva.GetValueOrDefault(tinta.Id) + a.Tva;
                tinta.Randuri += a.Randuri;
                if (eticheta.Cod != null)
                    tinta.Surse.Add(eticheta.Cod);
            }
        }

        foreach (var nod in noduri.OrderByDescending(n => n.Nivel)) {
            if (nod.ParinteId is not Guid pid || !dupaId.TryGetValue(pid, out var parinte))
                continue;
            parinte.Baza += nod.Baza;
            parinte.Tva += nod.Tva;
            parinte.Randuri += nod.Randuri;
            foreach (var sursa in nod.Surse)
                parinte.Surse.Add(sursa);
        }

        var lipsa = CoduriFormule.Where(c => !dupaCod.ContainsKey(c)).ToList();
        if (lipsa.Count > 0) {
            rezultat.Avertismente.Add(
                $"Rândurile {string.Join(", ", lipsa)} nu s-au putut citi din nomenclatorul D300, deci "
                + "totalurile formularului nu s-au calculat — fie baza nu e seed-uită la zi, fie "
                + "utilizatorul curent n-are drept de citire pe nomenclator.");
        }
        else {
            decimal T(string cod) => dupaCod[cod].Tva;

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

            Aduna("19", OperanziRd19);
            Aduna("30", OperanziRd30);
            dupaCod["31"].Tva = T("30") - nedeductibil;
            dupaCod["35"].Tva = T("31") + T("32") + T("33") + T("34");
            dupaCod["36"].Tva = Math.Max(T("35") - T("19"), 0m);
            dupaCod["37"].Tva = Math.Max(T("19") - T("35"), 0m);
            dupaCod["40"].Tva = T("37") + T("38") + T("39");
            dupaCod["43"].Tva = T("36") + T("41") + T("42");
            dupaCod["44"].Tva = Math.Max(T("40") - T("43"), 0m);
            dupaCod["45"].Tva = Math.Max(T("43") - T("40"), 0m);
        }

        VerificaInegalitatileFormularului(rezultat, dupaCod);

        if (dataStart < PrimaPerioada2026)
            rezultat.Avertismente.Add(
                $"Perioada începe la {dataStart:dd.MM.yyyy}, înaintea anului 2026, dar formularul e cel în "
                + "vigoare (OPANAF 174/2026): cotele istorice apar pe rd. 16 și rd. 33, nu pe rândurile lor "
                + "de atunci. Cifrele sunt corecte; așezarea e a formularului de azi.");

        foreach (var (id, suma) in pierdutTva.OrderBy(p => dupaId[p.Key].Ordine))
            rezultat.Avertismente.Add(
                $"rd. {dupaId[id].Cod} a primit TVA {suma.ToString("N2", Ro)} pe care nu-l poate purta — "
                + "rândul n-are coloană de TVA în formular. Verificați maparea tipurilor de TVA.");
        foreach (var (id, suma) in pierdutBaza.OrderBy(p => dupaId[p.Key].Ordine))
            rezultat.Avertismente.Add(
                $"rd. {dupaId[id].Cod} a primit bază impozabilă {suma.ToString("N2", Ro)} pe care nu o poate "
                + "purta — rândul n-are coloană de valoare în formular. Verificați maparea tipurilor de TVA.");

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
        if (TvaProiectii.LunaExacta(dataStart, dataEnd) is (int anDeclarat, int lunaDeclarata))
            rezultat.PerioadaDeschisa = os.GetObjectsQuery<PerioadaFiscala>()
                .Any(p => p.An == anDeclarat && p.Luna == lunaDeclarata && !p.Inchisa);
        return rezultat;
    }

    static void VerificaInegalitatileFormularului(D300Dto rezultat, IReadOnlyDictionary<string, Nod> dupaCod) {
        Nod N(string cod) => dupaCod.GetValueOrDefault(cod);

        if (N("30") is { AreTva: true } rd30 && N("31") is { AreTva: true } rd31 && rd31.Tva > rd30.Tva)
            rezultat.Avertismente.Add(
                $"rd. 31 ({rd31.Tva.ToString("N2", Ro)}) depășește rd. 30 ({rd30.Tva.ToString("N2", Ro)}) — "
                + "validarea V_6 a formularului (taxa dedusă nu poate depăși taxa deductibilă) va refuza "
                + "depunerea. Cauza tipică: storno al unei achiziții fără drept de deducere într-o lună "
                + "ulterioară operării, care aduce în perioadă un nedeductibil negativ. Cifrele sunt cele "
                + "din registru; regularizarea se declară, nu se ajustează în decont.");

        foreach (var (parinte, copii) in new[] {
            ("12", new[] { "12.1", "12.2" }), ("26", new[] { "26.1", "26.2" })
        }) {
            if (N(parinte) is not { } sus)
                continue;
            var jos = copii.Select(N).Where(n => n != null).ToList();
            if (sus.AreBaza && jos.All(n => n.AreBaza) && sus.Baza < jos.Sum(n => n.Baza))
                rezultat.Avertismente.Add(Inegalitate(parinte, sus.Baza, copii, jos.Sum(n => n.Baza), "Valoare"));
            if (sus.AreTva && jos.All(n => n.AreTva) && sus.Tva < jos.Sum(n => n.Tva))
                rezultat.Avertismente.Add(Inegalitate(parinte, sus.Tva, copii, jos.Sum(n => n.Tva), "TVA"));
        }

        foreach (var oglinda in dupaCod.Values.Where(n => n.OglindaAId != null).OrderBy(n => n.Ordine)) {
            var sursa = dupaCod.Values.FirstOrDefault(n => n.Id == oglinda.OglindaAId.Value);
            if (sursa == null) {
                rezultat.Avertismente.Add(
                    $"rd. {oglinda.Cod} e rând-oglindă, dar rândul-sursă din care ar trebui copiat nu s-a "
                    + "putut citi din nomenclator — rândul rămâne zero, iar formularul va pica la validarea "
                    + "de egalitate.");
                continue;
            }
            if (oglinda.AreBaza && sursa.AreBaza && oglinda.Baza != sursa.Baza)
                rezultat.Avertismente.Add(Oglinda(oglinda.Cod, oglinda.Baza, sursa.Cod, sursa.Baza, "Valoare"));
            if (oglinda.AreTva && sursa.AreTva && oglinda.Tva != sursa.Tva)
                rezultat.Avertismente.Add(Oglinda(oglinda.Cod, oglinda.Tva, sursa.Cod, sursa.Tva, "TVA"));
        }
    }

    static string Inegalitate(string parinte, decimal sus, string[] copii, decimal jos, string coloana) =>
        $"rd. {parinte} ({sus.ToString("N2", Ro)}) e sub suma sub-rândurilor „din care” rd. "
        + $"{string.Join(" + rd. ", copii)} ({jos.ToString("N2", Ro)}), pe coloana {coloana} — validare "
        + "blocantă a formularului. Verificați legăturile de sub-rând din nomenclatorul D300.";

    static string Oglinda(string cod, decimal valoare, string codSursa, decimal sursa, string coloana) =>
        $"rd. {cod} ({valoare.ToString("N2", Ro)}) trebuie să fie EGAL cu rd. {codSursa} "
        + $"({sursa.ToString("N2", Ro)}) pe coloana {coloana} — validare blocantă a formularului "
        + "(zona deductibilă a taxării inverse e copia celei colectate).";
}
