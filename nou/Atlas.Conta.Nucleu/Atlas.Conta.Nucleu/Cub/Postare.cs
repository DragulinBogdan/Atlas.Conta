namespace Atlas.Conta.Nucleu;

public sealed record Postare(
    Coordonate Coordonate,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza,
    Guid? Atribuit = null) {

    // `with` ocolește constructorul; scara se apără și în `init` (N-D2).
    readonly decimal cantitate = LaScara(Cantitate, Scara.Cantitate, nameof(Cantitate));
    readonly decimal valoareValuta = LaScara(ValoareValuta, Scara.Bani, nameof(ValoareValuta));
    readonly decimal valoare = LaScara(Valoare, Scara.Bani, nameof(Valoare));

    public decimal Cantitate {
        get => cantitate;
        init => cantitate = LaScara(value, Scara.Cantitate, nameof(Cantitate));
    }

    public decimal ValoareValuta {
        get => valoareValuta;
        init => valoareValuta = LaScara(value, Scara.Bani, nameof(ValoareValuta));
    }

    public decimal Valoare {
        get => valoare;
        init => valoare = LaScara(value, Scara.Bani, nameof(Valoare));
    }

    static decimal LaScara(decimal valoare, int scara, string nume) =>
        Scara.EsteLa(valoare, scara)
            ? valoare
            : throw new ArgumentException($"{nume} = {valoare} are mai multe zecimale decât scara {scara}.", nume);
}
