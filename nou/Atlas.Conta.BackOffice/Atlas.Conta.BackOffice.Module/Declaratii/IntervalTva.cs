namespace Atlas.Conta.BackOffice.Module.Declaratii;

public static class IntervalTva {
    public static bool Contine(DateOnly data, DateOnly? deLa, DateOnly? panaLa) =>
        (deLa == null || data >= deLa) && (panaLa == null || data <= panaLa);
}
