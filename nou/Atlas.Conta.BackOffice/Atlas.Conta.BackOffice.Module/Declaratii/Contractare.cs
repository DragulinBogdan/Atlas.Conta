#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Driverul declarației: citește operandul, cheamă declarantul frunzei și dă
/// nucleului declarația. Nu materializează nimic.
/// </summary>
public static class Contractare {
    public static N.Contract Contracteaza(IObjectSpace os, Document doc) {
        ArgumentNullException.ThrowIfNull(doc);
        var declarant = doc.Declarant
            ?? throw new InvalidOperationException(
                $"Documentul {doc.ID} e de un tip care nu declară încă — driverul nu se cheamă pe el.");
        var operand = Motor.Fapte.Operand(os, doc);
        var rotunjire = new N.Rotunjire(Scara.ConventieBani);
        var refuzuri = new List<N.Refuz>();
        N.Declaratie? declaratie = null;
        try {
            declaratie = declarant.Declara(operand, rotunjire, refuzuri);
        }
        catch (N.RefuzException e) {
            refuzuri.Add(e.Refuz);
        }
        if (refuzuri.Count > 0)
            return N.Contract.Refuza(refuzuri, [], [], rotunjire.JumatatiDeBan);
        return declaratie is null
            ? throw new InvalidOperationException(
                $"Declarantul {declarant.GetType().Name} a întors null fără niciun refuz.")
            : N.Motor.Opereaza(declaratie, rotunjire);
    }
}
