using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class ImoFixture {
    public static Document OpereazaCuSuport(IObjectSpace os, PunereInFunctiune pif) {
        CreeazaSuport(os, pif);
        return MotorOperare.Opereaza(os, pif);
    }

    public static void CreeazaSuport(IObjectSpace os, PunereInFunctiune pif) {
        var linii = pif.Detalii.OfType<PunereInFunctiuneDetaliu>()
            .Where(l => l.LinieSursaId == null && l.Fel != FelLiniePif.Revizuire && l.Valoare > 0m).ToList();
        if (linii.Count == 0) return;
        var marcaj = "IMO-SUPORT-" + pif.ID;
        if (os.GetObjectsQuery<NotaContabila>().Any(n => n.Numar == marcaj && n.Stare == StareDocument.Operat)) return;
        var cont = os.GetObjectsQuery<Cont>().First(c => c.Simbol == "1012" || c.Simbol == "117.00.00").ID;
        var trz = os.GetObjectsQuery<TipMaterial>().Single(t => t.Cod == "TRZ").ID;
        var n = os.CreateObject<NotaContabila>(); n.Numar = marcaj;
        n.Data = pif.Data; n.DataInregistrare = pif.DataInregistrare;
        n.PredatorId = pif.PredatorId; n.PrimitorId = pif.PrimitorId;
        foreach (var l in linii) {
            var activ = os.GetObjectByKey<TipMaterial>(l.TipMaterialId).ContImplicitId.Value;
            var fisa = os.GetObjectByKey<Imobilizare>(l.ImobilizareId);
            void Nota(Guid debit, Guid credit, decimal valoare) {
                var d = os.CreateObject<NotaContabilaDetaliu>(); d.Document = n; d.TipMaterialId = trz;
                d.ContDebitId = debit; d.ContCreditId = credit; d.Valoare = valoare;
                d.RepartitorDebitId = pif.PrimitorId; d.RepartitorCreditId = pif.PrimitorId;
                d.CodEconomicId = fisa?.CodEconomicId ?? os.GetObjectsQuery<CodEconomic>().Select(c => (Guid?)c.ID).FirstOrDefault();
            }
            Nota(activ, cont, l.Valoare);
            if (l.AmortizareInitiala != 0m) {
                var amortizare = os.GetObjectsQuery<PoliticaAmortizare>().Single(p => p.TipMaterialId == l.TipMaterialId).ContAmortizareId.Value;
                Nota(cont, amortizare, l.AmortizareInitiala);
            }
        }
        os.CommitChanges(); MotorOperare.Opereaza(os, n);
    }
}
