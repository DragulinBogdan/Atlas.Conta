#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Declarația fluxului unui tip de document: pură, pe operandul închis, fără
/// <c>IObjectSpace</c>, entități sau ceas. Întoarce <c>null</c> DOAR după ce a
/// adăugat cel puțin un refuz.
/// </summary>
public interface IDeclarant {
    N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri);
}
