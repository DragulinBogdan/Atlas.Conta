namespace Atlas.Conta.BackOffice.Module.Api;

// Bucățile de proiecție PARTAJATE între felii — tot ce nu e al unui tip de
// document, dar nici nu poate trăi în SQL. Regula lor: nu ating baza, nu ating
// navigații; primesc câmpuri deja PROIECTATE PLAT și le compun în memorie.
internal static class ApiProiectii {
    // Oglinda lui `Lot.Eticheta` (care e [NotMapped], deci inaccesibil în SQL):
    // aceleași reguli, compuse după materializarea câmpurilor plate. Orice
    // schimbare acolo se reflectă aici — cusătura e documentată în ambele.
    // O SINGURĂ copie pentru toate feliile (BTR/FCT/NIR): eticheta lotului apare
    // pe orice linie care referă un lot, iar un al doilea adevăr de afișare ar
    // diverge tăcut de model.
    public static string EtichetaLot(string produs, DateOnly? data, decimal? pretUnitar) {
        if (data == null)
            return null;
        var denumire = produs ?? "(produs nedefinit)";
        var pret = pretUnitar ?? 0m;
        return data == default(DateOnly) && pret == 0m
            ? $"{denumire} (în culegere)"
            : $"{denumire} · {data:dd.MM.yyyy} · {pret:0.####}";
    }
}
