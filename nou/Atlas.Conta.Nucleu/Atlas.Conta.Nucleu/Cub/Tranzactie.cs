namespace Atlas.Conta.Nucleu;

public sealed record Tranzactie(
    FelTranzactie Fel,
    DateOnly Data,
    Guid? Document,
    IReadOnlyList<Postare> Postari) {

    public bool Equals(Tranzactie? alta) =>
        alta is not null
        && Fel == alta.Fel
        && Data == alta.Data
        && Document == alta.Document
        && Secvente.Egale(Postari, alta.Postari);

    public override int GetHashCode() {
        var cod = new HashCode();
        cod.Add(Fel);
        cod.Add(Data);
        cod.Add(Document);
        Secvente.Adauga(ref cod, Postari);
        return cod.ToHashCode();
    }
}
