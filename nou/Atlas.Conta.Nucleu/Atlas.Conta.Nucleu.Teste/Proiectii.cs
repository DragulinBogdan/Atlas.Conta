using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public sealed record Cheie(Guid? Unu = null, Guid? Doi = null, Latura? Latura = null);

public static class Proiectii {
    public static readonly (string Nume, Func<Postare, Cheie> Cheie)[] Toate = [
        ("cont", p => new Cheie(p.Coordonate.Cont)),
        ("cont+latura", p => new Cheie(p.Coordonate.Cont, null, p.Coordonate.Latura)),
        ("unitate", p => new Cheie(p.Coordonate.Unitate?.Id)),
        ("gestiune+produs", p => new Cheie(p.Coordonate.Gestiune, p.Coordonate.Produs)),
        ("partener", p => new Cheie(p.Coordonate.Partener)),
    ];

    public static void Egale(
        IReadOnlyDictionary<Cheie, Sold> stanga,
        IReadOnlyDictionary<Cheie, Sold> dreapta,
        string unde) {
        foreach (var cheie in stanga.Keys.Concat(dreapta.Keys).Distinct()) {
            var unul = stanga.TryGetValue(cheie, out var s) ? s : Sold.Zero;
            var altul = dreapta.TryGetValue(cheie, out var d) ? d : Sold.Zero;
            Assert.True(unul == altul, $"{unde}: pe {cheie} am {unul}, aștept {altul}");
        }
    }

    public static Dictionary<Cheie, Sold> Aduna(
        IReadOnlyDictionary<Cheie, Sold> stanga,
        IReadOnlyDictionary<Cheie, Sold> dreapta) {
        var suma = new Dictionary<Cheie, Sold>();
        foreach (var cheie in stanga.Keys.Concat(dreapta.Keys).Distinct())
            suma[cheie] = (stanga.TryGetValue(cheie, out var s) ? s : Sold.Zero)
                + (dreapta.TryGetValue(cheie, out var d) ? d : Sold.Zero);
        return suma;
    }
}
