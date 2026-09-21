namespace Atlas.Conta.Nucleu;

public static class Scara {
    public const int Precizie = 18;

    public const int Bani = 2;
    public const int Pret = 6;
    public const int Cantitate = 3;
    public const int Procent = 4;

    public static bool EsteLa(decimal valoare, int scara) => decimal.Round(valoare, scara) == valoare;
}
