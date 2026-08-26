using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// LUNGIMILE `AddressStructure` din SAF-T, citite din MODEL (felia 15 D15-D1,
// mutate aici la felia 16 / D16-D1).
//
// De ce reflecție și nu o listă scrisă cu mâna: `[MaxLength]` de pe `Partener`
// E deja lungimea SAF-T — coloana și fișierul trebuie să spună același număr.
// O a doua listă ar fi exact locul în care apare deriva (o coloană lărgită într-o
// migrație, o tăiere care rămâne la vechea cifră și un fișier respins la
// validare, luni mai târziu).
//
// De ce a URCAT din `SincronizareAnafService` (unde s-a născut la R6) în
// `Comun`: la felia 16 are TREI consumatori care nu se cunosc între ei —
// serviciul de sincronizare ANAF (taie adresele venite din registru),
// conectorul 1C (taie adresele venite din `InfoRg_InformatiaDeContact`) și
// proba D16-V1, care verifică faptul de fond: `Societate` poartă ACELEAȘI
// lungimi ca `Partener`, fiindcă amândouă ies prin același `AddressStructure`
// (raportorul în `Header`, partenerul în `Customer`/`Supplier`).
// `SincronizareAnafService.Lungimi` rămâne ca alias, ca să nu se rupă apelanții.
//
// `Partener` rămâne ORIGINEA cifrelor (nu `Societate`): el e cel căruia i le-a
// dat felia 15, iar proba de model ține cele două tipuri egale.
public static class AdresaSaft {
    // Nume de proprietate → lungimea maximă, pentru TOATE proprietățile
    // `[MaxLength]` ale lui `Partener` (nu doar cele de adresă: apelanții taie
    // și `Denumire`/`CodFiscal` cu aceeași sursă).
    public static readonly IReadOnlyDictionary<string, int> Lungimi = typeof(Partener)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(pi => (pi.Name, Lungime: pi.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? 0))
        .Where(x => x.Lungime > 0)
        .ToDictionary(x => x.Name, x => x.Lungime, StringComparer.Ordinal);

    // Cele ȘASE câmpuri ale blocului de adresă, în ordinea în care le cere
    // `AddressStructure` și în care le culege un om. Lista e cheia probei D16-V1
    // („`Societate` are aceleași `MaxLength` ca `Partener`") și a oricărei
    // viitoare a treia entități cu adresă — un câmp adăugat aici e un câmp pe
    // care proba îl cere pe amândouă.
    public static readonly IReadOnlyList<string> CampuriAdresa = [
        nameof(Partener.Strada),
        nameof(Partener.Numar),
        nameof(Partener.DetaliiAdresa),
        nameof(Partener.Localitate),
        nameof(Partener.CodPostal),
        nameof(Partener.JudetId),
    ];
}
