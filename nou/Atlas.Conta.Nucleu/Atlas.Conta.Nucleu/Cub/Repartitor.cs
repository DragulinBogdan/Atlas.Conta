namespace Atlas.Conta.Nucleu;

// Valorile numerice sunt ale `Declaratii.FelRepartitor` din Module.
public enum FelRepartitor {
    Partener = 1,
    Angajat = 2,
    Gestiune = 3,
    ContPropriu = 4,
    UnitateInterna = 5,
}

public readonly record struct Repartitor(Guid Id, FelRepartitor Fel) {
    public bool EsteExtern => Fel is FelRepartitor.Partener or FelRepartitor.Angajat;

    public bool EsteIntern => Fel is FelRepartitor.Gestiune or FelRepartitor.UnitateInterna;
}
