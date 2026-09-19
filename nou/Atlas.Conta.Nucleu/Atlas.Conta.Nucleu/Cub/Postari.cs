namespace Atlas.Conta.Nucleu;

public static class Postari {
    // 090g: spațiul e funcție de unitate, nu câmp stocat.
    public static Spatiu Spatiu(this Postare postare) =>
        postare.Coordonate.Unitate?.Fel == FelUnitate.Lot ? Nucleu.Spatiu.Stoc : Nucleu.Spatiu.Contabil;
}
