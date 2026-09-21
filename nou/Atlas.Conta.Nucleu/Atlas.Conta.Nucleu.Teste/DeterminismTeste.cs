using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class DeterminismTeste {
    static readonly string[] RecordurileAsteptate = [
        nameof(Postare), nameof(Tranzactie), nameof(Coordonate), nameof(Contract), nameof(Declaratie),
        nameof(Miscare), nameof(Mutare), nameof(Capat), nameof(Unitate), nameof(Sold), nameof(Cauza),
        nameof(CodTva), nameof(Analiza), nameof(Refuz),
        nameof(Decizie), nameof(AlocareFifo), nameof(ValoareIesire), nameof(PartidaDeschisa),
        nameof(ContRezolvat),
        nameof(Ipoteza), nameof(SoldUnitateCitit), nameof(PerioadaDeschisa), nameof(VersiunePolitica),
    ];

    [Fact]
    public void DouaRulariPeAceeasiDeclaratieDauContracteEgale() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator);
            var intai = Motor.Opereaza(declaratie, new Rotunjire(MidpointRounding.AwayFromZero));
            var apoi = Motor.Opereaza(declaratie, new Rotunjire(MidpointRounding.ToEven));
            Assert.NotSame(intai, apoi);
            Assert.NotSame(intai.Tranzactii, apoi.Tranzactii);
            Assert.Equal(intai.Tranzactii, apoi.Tranzactii);
            Assert.Equal(intai, apoi);
            Assert.Equal(intai.GetHashCode(), apoi.GetHashCode());
        });

    [Fact]
    public void IpotezeleNuSchimbaTranzactia() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var declaratie = Gen.Declaratie(aleator);
            var alta = declaratie with {
                Ipoteze = [new VersiunePolitica("altă politică", Gen.Data(aleator))],
            };
            var unul = Motor.Opereaza(declaratie, new Rotunjire(MidpointRounding.AwayFromZero));
            var altul = Motor.Opereaza(alta, new Rotunjire(MidpointRounding.AwayFromZero));
            Assert.Equal(unul.Tranzactii, altul.Tranzactii);
            Assert.NotEqual(declaratie.Ipoteze, alta.Ipoteze);
            Assert.NotEqual(unul, altul);
        });

    [Fact]
    public void TranzactiaEEgalaStructural() =>
        Proprietate.Verifica(Proprietate.Cazuri, (aleator, _) => {
            var tranzactie = Gen.Operare(aleator);
            var copie = tranzactie with { Postari = tranzactie.Postari.ToList() };
            Assert.NotSame(tranzactie.Postari, copie.Postari);
            Assert.Equal(tranzactie, copie);
            Assert.Equal(tranzactie.GetHashCode(), copie.GetHashCode());
            var alta = tranzactie with {
                Postari = tranzactie.Postari.Skip(1).ToList(),
            };
            Assert.NotEqual(tranzactie, alta);
        });

    [Fact]
    public void RecordurilePubliceNAuSetterePubliceCareNuSuntInit() {
        var vinovati = new List<string>();
        var gasite = new List<string>();
        foreach (var tip in typeof(Postare).Assembly.GetExportedTypes()) {
            if (!ERecord(tip))
                continue;
            gasite.Add(tip.Name);
            foreach (var proprietate in tip.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                if (proprietate.SetMethod is { IsPublic: true } setter && !EInit(setter))
                    vinovati.Add($"{tip.Name}.{proprietate.Name}: setter public care nu e init");
            foreach (var camp in tip.GetFields(BindingFlags.Public | BindingFlags.Instance))
                if (!camp.IsInitOnly)
                    vinovati.Add($"{tip.Name}.{camp.Name}: câmp public mutabil");
        }
        Assert.True(vinovati.Count == 0, string.Join(" | ", vinovati));
        foreach (var nume in RecordurileAsteptate)
            Assert.Contains(nume, gasite);
    }

    [Fact]
    public void DecizieSiIpotezaSuntIerarhiiInchise() {
        foreach (var tip in new[] { typeof(Decizie), typeof(Ipoteza) }) {
            var constructori = tip
                .GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ToList();
            // CS8878 impune constructorul de copiere `protected` pe un record nesigilat: e singura ușă rămasă.
            var copiere = constructori.Where(c => EDeCopiere(tip, c)).ToList();
            Assert.All(copiere, c => Assert.True(c.IsFamily, $"{tip.Name}: constructorul de copiere nu e protected"));
            var deschisi = constructori
                .Where(c => !EDeCopiere(tip, c) && (c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
                .Select(c => $"{tip.Name}({string.Join(", ", c.GetParameters().Select(p => p.ParameterType.Name))})")
                .ToList();
            Assert.True(deschisi.Count == 0, string.Join(" | ", deschisi));
            Assert.Contains(constructori, c => !EDeCopiere(tip, c) && c.IsFamilyAndAssembly);
        }
    }

    [Fact]
    public void CazurileDecizieiSiAleIpotezeiSuntSigilate() {
        var cazuri = typeof(Postare).Assembly.GetExportedTypes()
            .Where(t => t != typeof(Decizie) && t != typeof(Ipoteza)
                && (typeof(Decizie).IsAssignableFrom(t) || typeof(Ipoteza).IsAssignableFrom(t)))
            .ToList();
        Assert.Equal(7, cazuri.Count);
        Assert.All(cazuri, t => Assert.True(t.IsSealed, $"{t.Name} nu e sigilat"));
    }

    static bool ERecord(Type tip) =>
        tip.GetMethod("<Clone>$", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) is not null;

    static bool EInit(MethodInfo setter) =>
        setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));

    static bool EDeCopiere(Type tip, ConstructorInfo constructor) =>
        constructor.GetParameters() is [{ } singur] && singur.ParameterType == tip;
}
