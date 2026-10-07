using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>Gardienii retragerii unui document operat care se citesc fără a executa comanda (106k): primul motiv, în ordinea motorului, sau null. Fără blocaje.</summary>
public sealed record GardieniRetragere(string Anulare, string Stornare) {
    public static GardieniRetragere Citeste(IObjectSpace os, Document doc) {
        var legaturi = MotorOperare.MotivLaturaPerecheOperata(os, doc) ?? MotorOperare.MotivConexeOperate(os, doc);
        if (legaturi != null)
            return new(FiscalitateService.MotivAnulare(os, doc)
                ?? GardianPerioada.MotivInchisa(os, doc.DataInregistrare) ?? legaturi, legaturi);

        var propriu = doc as IDocumentCuEfecteProprii;
        var imperecheri = ImperechereService.Motive(os, doc);
        var dependentiProprii = propriu?.MotivDependenti(os);
        var receptii = Cub.ReceptiiConexe.MotivDependenti(os, doc);
        var nominalizari = new Lazy<Cub.Materializare.Nominalizari>(() => Cub.Materializare.CitesteNominalizari(os, doc));

        var anulare = FiscalitateService.MotivAnulare(os, doc)
            ?? GardianPerioada.MotivInchisa(os, doc.DataInregistrare)
            ?? imperecheri.Anulare
            ?? MotorOperare.MotivLoturiFolosite(os, doc)
            ?? dependentiProprii
            ?? receptii
            ?? nominalizari.Value.Motiv(doc.DataInregistrare)
            ?? Cub.Materializare.MotivStingereDeschidere(os, doc);

        // Data stornării o alege comanda: aici intră numai ce refuză la orice dată.
        // Stornoul desface singur legăturile vii din perioadele închise, deci partenerii lor nu se numără la nominalizări.
        var stornare = imperecheri.Stornare
            ?? dependentiProprii
            ?? propriu?.MotivPerioadaStornarii(os)
            ?? receptii
            ?? nominalizari.Value.Motiv(DateOnly.MaxValue, imperecheri.Parteneri);

        return new(anulare, stornare);
    }
}
