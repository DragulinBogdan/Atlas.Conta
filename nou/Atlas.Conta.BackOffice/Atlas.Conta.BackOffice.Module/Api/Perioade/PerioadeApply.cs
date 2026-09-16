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

    // Raportul se ia pe starea dinaintea rescrierii, iar rescrierea e în ACEEAȘI
    // tranzacție: un eșec la jumătate n-are voie să lase referințe pe jumătate
    // reconstruite (F27-D1/D3).
    public static ReconstructieRezultatDto Reconstruieste(IObjectSpace os) {
        using var tx = TranzactieComanda.Incepe(os);
        var raport = SolduriService.Reconstruieste(os);
        tx.Commit();
        return new ReconstructieRezultatDto(raport.Referinte
            .Select(r => new ReconstructieReferintaDto(r.An, r.Luna,
                r.ContabilExistente, r.ContabilRecalculate, r.ContabilDiferite,
                r.StocExistente, r.StocRecalculate, r.StocDiferite,
                r.DiferentaDebit, r.DiferentaCredit, r.DiferentaCantitate, r.DiferentaValoare))
            .ToArray());
    }

    static InchiderePerioadaRezultatDto Din(IObjectSpace os, InchiderePerioada rand) =>
        new(Din(os.GetObjectByKey<PerioadaFiscala>(rand.PerioadaId)), rand.Fel.ToString(), rand.La, rand.De);

    static PerioadaDto Din(PerioadaFiscala p) =>
        new(p.An, p.Luna, p.Inchisa, p.InchisaLa, p.InchisaPrimaOara);
}
