using Atlas.Conta.BackOffice.Module.Anaf;

namespace Atlas.Conta.BackOffice.Module.Api.Parteneri;

// Felia 15 (D15-D4): contractul de SÂRMĂ al comenzii de sincronizare ANAF.
//
// De ce DTO peste rezultatele din `Module/Anaf` și nu ele direct — același motiv
// ca la `OperareRezultatDto` (ApiDtos.cs): tipurile de domeniu sunt ale
// serviciului și au voie să se miște (liste mutabile, `IReadOnlyList`, câmpuri
// adăugate pentru Import1C). Contractul pe care îl consumă TypeScript-ul e o
// proiecție DELIBERATĂ, cu tablouri imutabile și fără nimic din interiorul
// serviciului. Traducerea se face aici, o dată.
//
// `Camp` rămâne NUMELE proprietății (`Strada`, `Judet`, `InregistratTva`), nu o
// etichetă tradusă: clientul îl leagă de captions-urile din `metadata.json`
// (42e — metadata leagă atributele, codul decide prezentarea).

public sealed record ModificareCampDto(string Camp, string Vechi, string Nou) {
    public static ModificareCampDto Din(ModificareCamp m) => new(m.Camp, m.Vechi, m.Nou);
}

public sealed record DiferentaCampDto(string Camp, string Cules, string Anaf) {
    public static DiferentaCampDto Din(DiferentaCamp d) => new(d.Camp, d.Cules, d.Anaf);
}

// Un partener care NU a fost interogat/scris, cu motivul: necandidat (țară,
// CNP, fără CUI), inexistent, fără drept de scriere, lot ANAF eșuat, commit
// picat. Pe lot e un RÂND, nu o eroare — o cerere pentru 500 de parteneri nu
// pică fiindcă unul e persoană fizică.
public sealed record PartenerSaritDto(Guid Id, string Eticheta, string Motiv) {
    public static PartenerSaritDto Din(Sarit s) => new(s.Id, s.Eticheta, s.Motiv);
}

// Un lot ANAF eșuat, ca dată. `Tranzitorie` e cea care decide statusul pe
// endpoint-ul de un singur partener (503 vs. 422) și e informația de care are
// nevoie clientul ca să știe dacă are rost să reîncerce.
public sealed record EroareAnafDto(long[] Lot, string Mesaj, bool Tranzitorie) {
    public static EroareAnafDto Din(EroareLotAnaf e) => new([.. e.Lot], e.Mesaj, e.Tranzitorie);
}

// Rezultatul pentru UN partener: ce s-a scris (`Modificari`, cu valoarea VECHE —
// riscul 2 al contractului), ce s-a raportat fără să se scrie (`Diferente`) și
// ce n-a mers de tot (`Avertismente`). `Gasit = false` = CUI-ul e în `notFound`
// la ANAF: nimic scris, fără timbru.
public sealed record SincronizareAnafDto(
    Guid PartenerId,
    string Eticheta,
    long? Cui,
    bool Gasit,
    ModificareCampDto[] Modificari,
    DiferentaCampDto[] Diferente,
    string[] Avertismente) {

    public static SincronizareAnafDto Din(RezultatSincronizare r) => new(
        r.PartenerId, r.Eticheta, r.Cui, r.Gasit,
        [.. r.Modificari.Select(ModificareCampDto.Din)],
        [.. r.Diferente.Select(DiferentaCampDto.Din)],
        [.. r.Avertismente]);
}

// Corpul comenzii de LOT. Clasă cu setter-e (nu record): e ce leagă model
// binding-ul, iar un corp lipsă/malformat trebuie să iasă prin
// `InvalidModelStateResponseFactory` ca `EroriDto`, nu ca excepție de
// deserializare (70f).
public sealed class SincronizareAnafCerereDto {
    public Guid[] Ids { get; set; }

    // Explicit, cu default `false`: suprascrierea rescrie `Denumire`/adresa peste
    // valorile culese și e ireversibilă pe un lot de 500 (riscul 2). Cine o vrea
    // o cere pe nume.
    public bool Suprascrie { get; set; }
}

public sealed record SincronizareAnafLotDto(
    SincronizareAnafDto[] Rezultate,
    PartenerSaritDto[] Sarite,
    EroareAnafDto[] Erori) {

    // `sariteInPlus` = refuzurile gate-ului (inexistent / fără drept de scriere),
    // decise ÎNAINTE ca serviciul să vadă lotul. Se concatenează aici ca lista
    // `Sarite` să fie una singură: pentru client „n-a fost atins, uite de ce" e
    // același lucru, indiferent dacă motivul e securitatea sau nomenclatorul.
    public static SincronizareAnafLotDto Din(RezultatLot r, IEnumerable<PartenerSaritDto> sariteInPlus = null) => new(
        [.. r.Rezultate.Select(SincronizareAnafDto.Din)],
        [.. (sariteInPlus ?? []).Concat(r.Sarite.Select(PartenerSaritDto.Din))],
        [.. r.Erori.Select(EroareAnafDto.Din)]);
}
