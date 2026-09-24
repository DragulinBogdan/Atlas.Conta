namespace Atlas.Conta.Nucleu;

public enum RolTransformare { Consum = 1, Produs = 2 }

public sealed record Transformare(
    Capat Real,
    RolTransformare Rol,
    FelTranzactie Fel,
    decimal Cantitate,
    decimal Valoare,
    Cauza Cauza) {

    public static (Postare Reala, Postare Contrapondere) Postari(Transformare linie, DateOnly data) {
        ArgumentNullException.ThrowIfNull(linie);
        var real = linie.Real;
        if (real is null || real.Unitate is not { Fel: FelUnitate.Lot } lot
                || real.Gestiune is null || GestiuniVirtuale.Este(real.Gestiune)
                || real.Produs is null || real.Produs != lot.Produs || real.Cont != lot.Cont
                || real.Carte != Carte.Contabil || real.Partener != null || lot.Partener != null
                || real.CodTva != null || real.PerioadaDeclarare != null || real.Valuta != null)
            throw new ArgumentException("Transformarea cere un lot pe gestiune reală, în cartea contabilă, fără terț, TVA sau valută.", nameof(linie));
        if (linie.Rol is not (RolTransformare.Consum or RolTransformare.Produs)
                || linie.Fel is not (FelTranzactie.Operare or FelTranzactie.Transfer)
                || linie.Cantitate <= 0m || linie.Valoare < 0m)
            throw new ArgumentException("Linia transformării cere rol, fel, cantitate pozitivă și valoare nenegativă.", nameof(linie));
        var consum = linie.Rol == RolTransformare.Consum;
        var latura = consum && linie.Fel == FelTranzactie.Operare ? Latura.Credit : Latura.Debit;
        var cantitate = consum ? -linie.Cantitate : linie.Cantitate;
        var valoare = consum && linie.Fel == FelTranzactie.Transfer ? -linie.Valoare : linie.Valoare;
        var virtuala = linie.Real with { Gestiune = GestiuniVirtuale.Transformare, Unitate = null };
        return (new Postare(linie.Real.Pe(latura, data), cantitate, 0m, valoare, linie.Cauza),
            new Postare(virtuala.Pe(latura, data), -cantitate, 0m, 0m, linie.Cauza));
    }
}
