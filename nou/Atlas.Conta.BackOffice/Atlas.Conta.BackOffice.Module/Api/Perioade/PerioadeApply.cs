using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Perioade;

// Ambalarea comenzilor de perioadă (F27-D1/D2), tiparul `PoliticiApply`: zero
// reguli proprii — lanțul, verificarea și istoricul sunt ale lui
// `PerioadaService`, iar controllerul rămâne transport.
public static class PerioadeApply {
    public static PerioadaDto[] Lant(IObjectSpace os) =>
        PerioadaService.Lant(os).Select(Din).ToArray();

    public static ConstatareInchidereDto[] Verifica(IObjectSpace os, int an, int luna) =>
        PerioadaService.Verifica(os, an, luna)
            .Select(c => new ConstatareInchidereDto(c.Cheie, c.Fel, c.Severitate.ToString(), c.Text,
                c.ObiectId, c.ObiectEticheta))
            .ToArray();

    public static InchiderePerioadaRezultatDto Inchide(IObjectSpace os, int an, int luna,
            InchidePerioadaRequestDto cerere, Guid? deId, string de) =>
        Din(os, PerioadaService.Inchide(os, an, luna, cerere?.Acceptate ?? [], deId, de));

    public static InchiderePerioadaRezultatDto Redeschide(IObjectSpace os, int an, int luna,
            RedeschidePerioadaRequestDto cerere, Guid? deId, string de) =>
        Din(os, PerioadaService.Redeschide(os, an, luna, cerere?.Motiv, deId, de));

    static InchiderePerioadaRezultatDto Din(IObjectSpace os, InchiderePerioada rand) =>
        new(Din(os.GetObjectByKey<PerioadaFiscala>(rand.PerioadaId)), rand.Fel.ToString(), rand.La, rand.De);

    static PerioadaDto Din(PerioadaFiscala p) =>
        new(p.An, p.Luna, p.Inchisa, p.InchisaLa, p.InchisaPrimaOara);
}
