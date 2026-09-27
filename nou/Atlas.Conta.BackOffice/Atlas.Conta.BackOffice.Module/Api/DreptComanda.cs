using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>Dreptul operatorului pe comanda cerută; aruncă <see cref="SubiectInvizibil"/> (404) sau <see cref="RefuzAcces"/> (403).</summary>
public interface IDreptComanda {
    void Cere(Guid documentId, ComandaDocument comanda);
}

public static class DreptComanda {
    /// <summary>Operatorul de sistem al uneltelor standalone (ModelCheck, import): fără securitate XAF.</summary>
    public static readonly IDreptComanda Sistem = new DreptSistem();

    sealed class DreptSistem : IDreptComanda {
        public void Cere(Guid documentId, ComandaDocument comanda) { }
    }
}

/// <summary>Dreptul din securitatea XAF a utilizatorului autentificat, pe documentul rezolvat prin ușa securizată.</summary>
public sealed class DreptComandaXaf : IDreptComanda {
    readonly IObjectSpaceFactory fabricaSecurizata;
    readonly ISecurityStrategyBase securitate;
    readonly Type usa;
    readonly Func<object, bool> peUsaAsta;

    /// <param name="usa">Tipul ușii: un document de alt tip e invizibil pe ea.</param>
    /// <param name="peUsaAsta">Restricția ușii peste tip; documentul care nu o trece e invizibil.</param>
    public DreptComandaXaf(IObjectSpaceFactory fabricaSecurizata, ISecurityStrategyBase securitate,
            Type usa = null, Func<object, bool> peUsaAsta = null) {
        this.fabricaSecurizata = fabricaSecurizata ?? throw new ArgumentNullException(nameof(fabricaSecurizata));
        this.securitate = securitate;
        this.usa = usa ?? typeof(Document);
        this.peUsaAsta = peUsaAsta;
    }

    public void Cere(Guid documentId, ComandaDocument comanda) {
        using var os = fabricaSecurizata.CreateObjectSpace(usa);
        var document = RandDupaCheie.Oricare(os, usa, documentId) as Document;
        if (document == null || !usa.IsInstanceOfType(document) || peUsaAsta?.Invoke(document) == false)
            throw new SubiectInvizibil();
        var tip = MotorOperare.ClasaReala(document);
        if (securitate is not IRequestSecurityStrategy cerinte || !cerinte.CanWrite(os, (object)document))
            throw new RefuzAcces(OperatieAcces.Modificare, tip);
        if (comanda != ComandaDocument.Corecteaza)
            return;
        if (!cerinte.CanCreate(tip, os) || !cerinte.CanWrite(tip, os))
            throw new RefuzAcces(OperatieAcces.Creare, tip);
    }
}
