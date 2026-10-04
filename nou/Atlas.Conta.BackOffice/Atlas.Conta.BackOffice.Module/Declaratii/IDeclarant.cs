#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Declarația fluxului unui tip de document: pură, pe operandul închis, fără
/// <c>IObjectSpace</c>, entități sau ceas. Întoarce <c>null</c> DOAR după ce a
/// adăugat cel puțin un refuz sau a validat explicit o constatare fără mișcări.
/// </summary>
public interface IDeclarant {
    bool CereSoldRegistruPentruEvaluare => false;
    bool PermiteDeclaratieFaraMiscari(Operand operand) => false;
    /// <summary>Sursa din <see cref="SurseValoare"/> a ieșirilor pe lot valorizate de declarant; null când ieșirile se evaluează din sold.</summary>
    string? SursaValoareDeclarata => null;
    N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri);
}
