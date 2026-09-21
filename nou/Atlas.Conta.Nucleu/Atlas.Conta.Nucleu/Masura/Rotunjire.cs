namespace Atlas.Conta.Nucleu;

public sealed class Rotunjire(MidpointRounding conventie) {
    int jumatatiDeBan;

    public MidpointRounding Conventie => conventie;

    public int JumatatiDeBan => jumatatiDeBan;

    public decimal Bani(decimal valoare) {
        if (Math.Abs(valoare) % 0.01m == 0.005m)
            jumatatiDeBan++;
        return Math.Round(valoare, Scara.Bani, conventie);
    }

    // Prețul unitar e identificare pe lot, nu postare: convenția de profil e a scării BANI (N-D5).
    public decimal Pret(decimal valoare) => Math.Round(valoare, Scara.Pret, MidpointRounding.AwayFromZero);

    public decimal Cantitate(decimal valoare) => Math.Round(valoare, Scara.Cantitate, conventie);
}
