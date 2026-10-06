#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Declarația fluxului unui tip de document: pură, pe operandul închis, fără
/// <c>IObjectSpace</c>, entități sau ceas. Întoarce <c>null</c> DOAR după ce a
/// adăugat cel puțin un refuz sau a validat explicit o constatare fără mișcări.
/// </summary>
public enum PoliticaProfil { Niciuna, Contare, Tva, InchidereTva }

public interface IDeclarant {
    /// <summary>Declarantul contează liniile prin <c>RegulaContare</c> (D9-A4).</summary>
    bool ConteazaPrinReguli => false;
    /// <summary>Politica de profil fără de care tipul nu declară (D9-D5).</summary>
    PoliticaProfil PoliticaCeruta => PoliticaProfil.Niciuna;
    bool PermiteDeclaratieFaraMiscari(Operand operand) => false;
    /// <summary>Sursa din <see cref="SurseValoare"/> a ieșirilor pe lot valorizate de declarant; null când ieșirile se evaluează din sold.</summary>
    string? SursaValoareDeclarata => null;
    N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri);
}
