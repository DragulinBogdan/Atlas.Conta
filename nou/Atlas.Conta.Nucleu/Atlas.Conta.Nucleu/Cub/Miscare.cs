namespace Atlas.Conta.Nucleu;

public sealed record Miscare(
    Capat DeLa,
    Capat La,
    decimal Cantitate,
    decimal ValoareValuta,
    decimal Valoare,
    Cauza Cauza) {

    public static (Postare Debit, Postare Credit) Postari(Miscare miscare, DateOnly data) => (
        new Postare(
            miscare.La.Pe(Latura.Debit, data),
            miscare.Cantitate,
            miscare.ValoareValuta,
            miscare.Valoare,
            miscare.Cauza),
        new Postare(
            miscare.DeLa.Pe(Latura.Credit, data),
            -miscare.Cantitate,
            miscare.ValoareValuta,
            miscare.Valoare,
            miscare.Cauza));
}
