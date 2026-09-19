namespace Atlas.Conta.Nucleu.Teste;

public static class Proprietate {
    public const int Cazuri = 500;

    public static void Verifica(int cazuri, Action<Random, int> caz, int samanta = Gen.Samanta) {
        for (var i = 0; i < cazuri; i++) {
            var aleator = new Random(samanta + i);
            try {
                caz(aleator, i);
            } catch (Exception eroare) {
                throw new InvalidOperationException(
                    $"cazul {i} din {cazuri} (sămânța {samanta + i}, bază {samanta}): {eroare.Message}",
                    eroare);
            }
        }
    }
}
