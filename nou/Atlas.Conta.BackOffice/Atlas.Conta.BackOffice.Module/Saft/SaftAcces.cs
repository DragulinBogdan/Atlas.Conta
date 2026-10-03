using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace Atlas.Conta.BackOffice.Module.Saft;

/// <summary>
/// Accesul complet cerut de D406 L pe cub (SAF-D4, S2-D5): niciun criteriu de rând sau de membru pe tipurile
/// citite de <see cref="SaftProiectii.SaftPeCub"/>, inclusiv pe toate tipurile mapate în aceleași tabele.
/// </summary>
public static class SaftAcces {
    public const string Refuz = "SAFT_ACCES_INCOMPLET";

    /// <summary>Tipurile ale căror tabele le citește exportul L pe cub; proba SC-SAFT-36 le compară cu SQL-ul emis.</summary>
    public static readonly Type[] Citite = [
        typeof(Cub.Postare), typeof(Cub.Tranzactie), typeof(Document), typeof(DocumentDetaliu), typeof(Cont),
        typeof(Repartitor), typeof(TipDocument), typeof(TipTva), typeof(Societate), typeof(Produs), typeof(Lot),
        typeof(UnitateMasura), typeof(TipMaterial), typeof(CodFunctional), typeof(CodEconomic), typeof(SursaFinantare),
        typeof(Unitate), typeof(Proiect), typeof(Judet), typeof(ClasaProdus), typeof(MapareTvaSaft), typeof(SetareProfil),
    ];

    /// <summary>Tipurile citite de exportul S pe cub (S3-D6); proba SC-SAFT-48 le compară cu SQL-ul emis.</summary>
    public static readonly Type[] CititeStocuri = [
        typeof(Cub.Postare), typeof(Cub.Tranzactie), typeof(Document), typeof(DocumentDetaliu), typeof(Cont),
        typeof(Repartitor), typeof(TipDocument), typeof(TipTva), typeof(Societate), typeof(Produs),
        typeof(UnitateMasura), typeof(TipMaterial), typeof(Judet), typeof(ClasaProdus), typeof(MapareTvaSaft),
        typeof(SetareProfil), typeof(PoliticaMiscareSaft), typeof(PerioadaFiscala), typeof(SoldPerioadaStoc),
    ];

    public static IReadOnlySet<string> Tabele(IObjectSpace os, IEnumerable<Type> citite = null) =>
        AccesComplet.Tabele(os, citite ?? Citite);

    public static List<string> Lipsuri(IObjectSpace os, ISelectDataSecurityProvider securitate, IEnumerable<Type> citite = null) =>
        AccesComplet.Lipsuri(os, securitate, citite ?? Citite);
}
