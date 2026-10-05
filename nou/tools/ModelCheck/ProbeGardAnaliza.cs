using Atlas.Conta.BackOffice.Module.BusinessObjects;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Gardul analizei obligatorii chemat singur, fără planul registrelor care îl poate masca (D9-D4).</summary>
static class ProbeGardAnaliza {
    public static void Ruleaza(Action<string, bool> check) {
        var document = Guid.NewGuid(); var linie = Guid.NewGuid();
        var cheltuiala = Guid.NewGuid(); var furnizor = Guid.NewGuid(); var liber = Guid.NewGuid();
        var economic = Guid.NewGuid(); var gestiune = Guid.NewGuid(); var partener = Guid.NewGuid();
        var conturi = new Dictionary<Guid, C.GardAnaliza.ContFapt> {
            [cheltuiala] = new("628", DimensiuneFlags.CodEconomic),
            [furnizor] = new("401", DimensiuneFlags.Repartitor | DimensiuneFlags.CodEconomic),
            [liber] = new("5311", DimensiuneFlags.Niciuna),
        };
        N.Miscare Miscare(Guid debit, Guid credit, N.Analiza analiza, N.Carte carte = N.Carte.Contabil,
                Guid? peGestiune = null, Guid? pePartener = null, Guid? produs = null) => new(
            new N.Capat { Cont = credit, Carte = carte, Analiza = analiza, Partener = pePartener, Produs = produs },
            new N.Capat { Cont = debit, Carte = carte, Analiza = analiza, Gestiune = peGestiune, Produs = produs },
            0m, 0m, 100m, new N.Cauza(document, linie));
        IReadOnlyList<string> Lipsuri(N.Miscare miscare, Guid? angajament, Guid? alDocumentului = null,
                IReadOnlyDictionary<Guid, C.GardAnaliza.ContFapt> pe = null) => C.GardAnaliza.Lipsuri([miscare], pe ?? conturi,
            new Dictionary<Guid, C.GardAnaliza.LinieFapt> { [linie] = new("Servicii", angajament) },
            alDocumentului, alDocumentului);
        var cuCod = N.Analiza.Fara with { CodEconomic = economic };
        void Verifica(string nume, bool rezultat) => check("GARD-ANALIZA: " + nume, rezultat);

        Verifica("trece cu angajament și fără cod economic",
            Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, pePartener: partener), Guid.NewGuid()).Count == 0);
        var amandoua = Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, pePartener: partener), null);
        Verifica("refuză când lipsesc și codul economic, și angajamentul, câte un rând pe latură — "
            + string.Join(" | ", amandoua),
            amandoua.SequenceEqual([
                "Contul 628 (debit, linia cu Servicii) cere: Cod economic.",
                "Contul 401 (credit, linia cu Servicii) cere: Cod economic."]));
        Verifica("trece cu cod economic explicit",
            Lipsuri(Miscare(cheltuiala, furnizor, cuCod, pePartener: partener), null).Count == 0);
        Verifica("cartea fiscală nu intră în domeniu",
            Lipsuri(Miscare(cheltuiala, furnizor, N.Analiza.Fara, N.Carte.Fiscal), null).Count == 0);
        Verifica("contul fără analiză obligatorie nu cere nimic",
            Lipsuri(Miscare(liber, liber, N.Analiza.Fara), null).Count == 0);

        var faraRepartitor = Lipsuri(Miscare(cheltuiala, furnizor, cuCod), null);
        Verifica("repartitorul lipsește numai când nu-l poartă nici capătul, nici documentul — "
            + string.Join(" | ", faraRepartitor),
            faraRepartitor.SequenceEqual(["Contul 401 (credit, linia cu Servicii) cere: Repartitor."])
            && Lipsuri(Miscare(cheltuiala, furnizor, cuCod), null, alDocumentului: partener).Count == 0
            && Lipsuri(Miscare(cheltuiala, furnizor, cuCod, pePartener: partener), null).Count == 0
            && Lipsuri(Miscare(furnizor, cheltuiala, cuCod, peGestiune: gestiune), null).Count == 0);

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
