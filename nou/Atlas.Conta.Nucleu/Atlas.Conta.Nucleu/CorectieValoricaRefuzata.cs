namespace Atlas.Conta.Nucleu;

public enum MotivCorectieValorica { LotNeeligibil, ValoareInsuficienta, Cronologie, Dependenti, AnInchis, Provenienta }

/// <summary>Refuzul mecanismului de corecție valorică; codul de refuz îl dă declarantul.</summary>
public sealed class CorectieValoricaRefuzata(MotivCorectieValorica motiv, string mesaj) : Exception(mesaj) {
    public MotivCorectieValorica Motiv { get; } = motiv;
}
