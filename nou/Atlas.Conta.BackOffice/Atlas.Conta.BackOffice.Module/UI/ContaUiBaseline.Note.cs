using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.DXF.Core.Views.Discovery;
using Atlas.DXF.Core.Views.Fluent;

namespace Atlas.Conta.BackOffice.Module.UI;

public sealed partial class ContaUiBaseline {
    // Nota contabilă: linia E postarea, deci perechea de conturi o identifică (FAZA 1C §5).
    static void NotaContabila(UiBaselineRegistry registry) {
        registry.For<NotaContabilaDetaliu>()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.Descriere, d => d.ContDebit, d => d.ContCredit,
                    d => d.RepartitorDebit, d => d.RepartitorCredit)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva))
            .ListView(nameof(NotaContabilaDetaliu) + ListView, Culegere);

        // E3-D3: liniile închiderii sunt generate, deci grila nu se culege (85c, F26-D12).
        registry.For<InchidereRezultatDetaliu>()
            .HideForeignKeys()
            .Columns(c => c
                .Slot(Rol.Identitate, d => d.ContDebit, d => d.ContCredit,
                    d => d.RepartitorDebit, d => d.RepartitorCredit, d => d.SursaPeDebit)
                .Drop(Rol.Unitate).Drop(Rol.Cantitate).Drop(Rol.Tva))
            .ListView(nameof(InchidereRezultatDetaliu) + ListView, ReadOnly);
    }
}
