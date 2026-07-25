using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// Idempotența importului (precedentul Migrare, decizia 34b): corelarea
// sursă→țintă trăiește în `MigrareLegatura`, cheiată pe identitatea 1C (hex-ul
// KeyField) sub un nume de tabelă prefixat „1C:" — ca să nu se amestece cu
// legăturile pasului 4 (SQL Server legacy) dacă vreodată ajung în aceeași bază.
// Sursele fără id stabil (nomenclatoarele cu cod natural) fac upsert pe cod;
// deschiderea (DocumentId=null, decizia 25e) se rescrie integral la fiecare
// rulare — nu are nevoie de legături.
static class Legaturi {
    // Numele de tabelă convenite; view-ul 1C ca sufix.
    public static string Tabela(string view) => $"1C:{view}";

    public static Dictionary<string, Guid> Incarca(IObjectSpace os, string view) =>
        os.GetObjectsQuery<MigrareLegatura>()
            .Where(m => m.Tabela == Tabela(view))
            .ToDictionary(m => m.CheieLegacy, m => m.TintaId);

    public static void Leaga(IObjectSpace os, string view, string cheie, Guid tinta) {
        var l = os.CreateObject<MigrareLegatura>();
        l.Tabela = Tabela(view);
        l.CheieLegacy = cheie;
        l.TintaId = tinta;
    }
}
