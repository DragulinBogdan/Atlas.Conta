using Atlas.Conta.BackOffice.Module.BusinessObjects;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Gardul analizei obligatorii chemat singur, fără planul registrelor care îl poate masca (D9-D4).</summary>
static class ProbeGardAnaliza {
    public static void Ruleaza(Action<string, bool> check) {
        var document = Guid.NewGuid(); var linie = Guid.NewGuid();
        var cheltuiala = Guid.NewGuid(); var furnizor = Guid.NewGuid(); var liber = Guid.NewGuid();
        var optional = Guid.NewGuid(); var mandat = Guid.NewGuid();
        var economic = Guid.NewGuid();
        var gestiune = new N.Repartitor(Guid.NewGuid(), N.FelRepartitor.Gestiune);
        var partener = new N.Repartitor(Guid.NewGuid(), N.FelRepartitor.Partener);
        var angajat = new N.Repartitor(Guid.NewGuid(), N.FelRepartitor.Angajat);
        var propriu = new N.Repartitor(Guid.NewGuid(), N.FelRepartitor.ContPropriu);
        var conturi = new Dictionary<Guid, C.GardAnaliza.ContFapt> {
            [cheltuiala] = new("628", DimensiuneFlags.CodEconomic),
            [furnizor] = new("401", DimensiuneFlags.CodEconomic, FeluriRepartitor.Partener),
            [liber] = new("5311", DimensiuneFlags.Niciuna),
            [optional] = new("803", DimensiuneFlags.Niciuna, FeluriRepartitor.Partener | FeluriRepartitor.Niciunul),
            [mandat] = new("552", DimensiuneFlags.Niciuna, FeluriRepartitor.ContPropriu),
        };
        N.Miscare Miscare(Guid debit, Guid credit, N.Analiza analiza, N.Carte carte = N.Carte.Contabil,
                N.Repartitor? peDebit = null, N.Repartitor? peCredit = null, Guid? produs = null,
                N.Contrapondere? contrapondereCredit = null) => new(
            new N.Capat { Cont = credit, Carte = carte, Analiza = analiza, Repartitor = peCredit, Produs = produs,
                Contrapondere = contrapondereCredit },
            new N.Capat { Cont = debit, Carte = carte, Analiza = analiza, Repartitor = peDebit, Produs = produs },
            0m, 0m, 100m, new N.Cauza(document, linie));
        IReadOnlyList<string> Lipsuri(N.Miscare miscare, Guid? angajament,
                IReadOnlyDictionary<Guid, C.GardAnaliza.ContFapt> pe = null) => C.GardAnaliza.Lipsuri([miscare], pe ?? conturi,
            new Dictionary<Guid, C.GardAnaliza.LinieFapt> { [linie] = new("Servicii", angajament) });
        var cuCod = N.Analiza.Fara with { CodEconomic = economic };
        void Verifica(string nume, bool rezultat) => check("GARD-ANALIZA: " + nume, rezultat);

        Verifica("trece cu angajament și fără cod economic",
            Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, peCredit: partener), Guid.NewGuid()).Count == 0);
        var amandoua = Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, peCredit: partener), null);
        Verifica("refuză când lipsesc și codul economic, și angajamentul, câte un rând pe latură — "
            + string.Join(" | ", amandoua),
            amandoua.SequenceEqual([
                "Contul 628 (debit, linia cu Servicii) cere: Cod economic.",
                "Contul 401 (credit, linia cu Servicii) cere: Cod economic."]));
        Verifica("trece cu cod economic explicit",
            Lipsuri(Miscare(cheltuiala, furnizor, cuCod, peCredit: partener), null).Count == 0);
        Verifica("cartea fiscală nu intră în domeniu",
            Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, N.Carte.Fiscal), null).Count == 0);
        Verifica("contul fără analiză obligatorie nu cere nimic",
            Lipsuri(Miscare(liber, liber, N.Analiza.Fara), null).Count == 0);

        var faraRepartitor = Lipsuri(Miscare(cheltuiala, furnizor, cuCod), null);
        Verifica("D9-A10: repartitorul e al capătului; fără el gardul refuză — "
            + string.Join(" | ", faraRepartitor),
            faraRepartitor.SequenceEqual(["Contul 401 (credit, linia cu Servicii) cere: Repartitor."])
            && Lipsuri(Miscare(cheltuiala, furnizor, cuCod, peCredit: partener), null).Count == 0);

        var altFel = Lipsuri(Miscare(furnizor, cheltuiala, cuCod, peDebit: gestiune), null);
        Verifica("C111-D4 (S6): repartitorul de alt fel decât cele declarate e refuzat — " + string.Join(" | ", altFel),
            altFel.SequenceEqual(["Contul 401 (debit, linia cu Servicii) cere: Repartitor de felul Partener (primit: Gestiune)."]));
        var peOptional = Lipsuri(Miscare(optional, liber, N.Analiza.Fara, peDebit: gestiune), null);
        Verifica("C111-D4: mulțimea cu „niciunul” admite lipsa și felul declarat, nu alt fel — " + string.Join(" | ", peOptional),
            Lipsuri(Miscare(optional, liber, N.Analiza.Fara), null).Count == 0
            && Lipsuri(Miscare(optional, liber, N.Analiza.Fara, peDebit: partener), null).Count == 0
            && peOptional.SequenceEqual(["Contul 803 (debit, linia cu Servicii) cere: Repartitor de felul Partener (primit: Gestiune)."]));
        var peMandat = Lipsuri(Miscare(mandat, liber, N.Analiza.Fara, peDebit: partener), null);
        Verifica("C111-D4: contul care declară numai feluri e păzit; contul propriu trece, terțul și lipsa nu — "
            + string.Join(" | ", peMandat),
            Lipsuri(Miscare(mandat, liber, N.Analiza.Fara, peDebit: propriu), null).Count == 0
            && peMandat.SequenceEqual(["Contul 552 (debit, linia cu Servicii) cere: Repartitor de felul ContPropriu (primit: Partener)."])
            && Lipsuri(Miscare(mandat, liber, N.Analiza.Fara), null)
                .SequenceEqual(["Contul 552 (debit, linia cu Servicii) cere: Repartitor."]));
        var dublu = new Dictionary<Guid, C.GardAnaliza.ContFapt> {
            [furnizor] = new("462", DimensiuneFlags.Niciuna, FeluriRepartitor.Partener | FeluriRepartitor.Angajat),
        };
        var peDublu = Lipsuri(Miscare(furnizor, liber, N.Analiza.Fara, peDebit: gestiune), null, pe: dublu);
        Verifica("C111-D4: două feluri declarate le admit pe amândouă — " + string.Join(" | ", peDublu),
            Lipsuri(Miscare(furnizor, liber, N.Analiza.Fara, peDebit: partener), null, pe: dublu).Count == 0
            && Lipsuri(Miscare(furnizor, liber, N.Analiza.Fara, peDebit: angajat), null, pe: dublu).Count == 0
            && peDublu.SequenceEqual(["Contul 462 (debit, linia cu Servicii) cere: Repartitor de felul Partener sau Angajat (primit: Gestiune)."]));

        var marcataFaraCod = Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, contrapondereCredit: N.Contrapondere.Tert), Guid.NewGuid());
        var marcata = Lipsuri(Miscare(liber, furnizor, N.Analiza.Fara, contrapondereCredit: N.Contrapondere.Tert), null);
        Verifica("C111-D4: contraponderea e scutită numai de prezența repartitorului — "
            + string.Join(" | ", marcata),
            marcataFaraCod.Count == 0
            && marcata.SequenceEqual(["Contul 401 (credit, linia cu Servicii) cere: Cod economic."])
            && Lipsuri(Miscare(liber, furnizor, cuCod, contrapondereCredit: N.Contrapondere.Tert), null).Count == 0);
        var marcataAltFel = Lipsuri(Miscare(liber, furnizor, cuCod, peCredit: gestiune, contrapondereCredit: N.Contrapondere.Tert), null);
        Verifica("C111-D4: pe contrapondere, felul unui repartitor prezent se judecă — " + string.Join(" | ", marcataAltFel),
            marcataAltFel.SequenceEqual(["Contul 401 (credit, linia cu Servicii) cere: Repartitor de felul Partener (primit: Gestiune)."])
            && Lipsuri(Miscare(liber, furnizor, cuCod, peCredit: partener, contrapondereCredit: N.Contrapondere.Tert), null).Count == 0);

        var axe = new (DimensiuneFlags Flag, string Nume, N.Analiza Cu)[] {
            (DimensiuneFlags.CodFunctional, "Cod funcțional", N.Analiza.Fara with { CodFunctional = Guid.NewGuid() }),
            (DimensiuneFlags.SursaFinantare, "Sursă de finanțare", N.Analiza.Fara with { SursaFinantare = Guid.NewGuid() }),
            (DimensiuneFlags.Unitate, "Unitate", N.Analiza.Fara with { UnitateOrganizatorica = Guid.NewGuid() }),
            (DimensiuneFlags.Proiect, "Proiect", N.Analiza.Fara with { Proiect = Guid.NewGuid() }),
            (DimensiuneFlags.CentruCost, "Centru de cost", N.Analiza.Fara with { CentruCost = Guid.NewGuid() }),
        };
        Verifica("fiecare axă de analiză se cere pe numele ei și e satisfăcută de valoarea ei", axe.All(axa => {
            var pe = new Dictionary<Guid, C.GardAnaliza.ContFapt> { [cheltuiala] = new("628", axa.Flag) };
            return Lipsuri(Miscare(cheltuiala, liber, N.Analiza.Fara), Guid.NewGuid(), pe: pe)
                    .SequenceEqual([$"Contul 628 (debit, linia cu Servicii) cere: {axa.Nume}."])
                && Lipsuri(Miscare(cheltuiala, liber, axa.Cu), null, pe: pe).Count == 0;
        }));
        var material = new Dictionary<Guid, C.GardAnaliza.ContFapt> { [cheltuiala] = new("302", DimensiuneFlags.Material) };
        Verifica("materialul e produsul capătului",
            Lipsuri(Miscare(cheltuiala, liber, N.Analiza.Fara), null, pe: material)
                .SequenceEqual(["Contul 302 (debit, linia cu Servicii) cere: Material."])
            && Lipsuri(Miscare(cheltuiala, liber, N.Analiza.Fara, produs: Guid.NewGuid()), null, pe: material).Count == 0);

        var dublate = C.GardAnaliza.Lipsuri(
            [Miscare(cheltuiala, liber, N.Analiza.Fara), Miscare(cheltuiala, liber, N.Analiza.Fara)], conturi,
            new Dictionary<Guid, C.GardAnaliza.LinieFapt> { [linie] = new("Servicii", null) });
        Verifica("două mișcări ale aceleiași linii pe același cont dau un singur rând", dublate.Count == 1);
    }
}
