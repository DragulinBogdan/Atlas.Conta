using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Decizia 14: perioada închisă = graniță absolută. O perioadă NEDEFINITĂ e
// tratată ca închisă — exercițiul de lucru se deschide explicit prin seed/UI.
public static class GardianPerioada {
    public static void VerificaDeschisa(IObjectSpace os, DateOnly data) {
        var perioada = os.FirstOrDefault<PerioadaFiscala>(p => p.An == data.Year && p.Luna == data.Month);
        if (perioada == null)
            throw new OperareException($"Perioada {data.Month:00}/{data.Year} nu e definită — e tratată ca închisă.");
        if (perioada.Inchisa)
            throw new OperareException($"Perioada {data.Month:00}/{data.Year} e închisă.");
    }
}
