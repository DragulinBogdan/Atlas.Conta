using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

// F27-D6 — corecția unui document operat: storno-ul originalului la o dată din
// perioada deschisă + un DRAFT nou cu aceeași culegere, legat de original și
// motivat. Nu e o a doua cale de stornare: `MotorOperare.Storneaza` rămâne
// neatins, cu toți gardienii lui (dependenți, împerecheri, sold, perioadă).
//
// Ce copiază: TOATE proprietățile scalare și FK-urile MAPATE ale lanțului TPT,
// prin metadata EF — nu o listă scrisă de mână pe 15 tipuri, care ar fi tăcut
// incompletă la primul câmp nou al unei frunze. Ce NU copiază e o listă mică și
// justificabilă: identitatea (`ID`), ce stăpânește motorul (`Stare`,
// `DataOperare`, `Autogenerat`, `DocumentSursaId`, `TotalStingere`), datele proprii corecției
// (`DataInregistrare`, `CorecteazaId`, `MotivCorectie`) și câmpurile de
// infrastructură ale lui `BaseObject`.
//
// `Numar` și `Data` se PĂSTREAZĂ: documentul fizic e același (aceeași factură a
// furnizorului, același număr de serie), doar evidența lui se reface. Seria nu
// se consumă din nou — `AsignaNumar` onorează un număr deja completat.
public static class CorectieService {
    // Proprietățile care NU se copiază pe documentul nou.
    static readonly HashSet<string> ExcluseDocument = new(StringComparer.Ordinal) {
        nameof(Document.ID), nameof(Document.Stare), nameof(Document.DataOperare),
        nameof(Document.DataInregistrare), nameof(Document.DocumentSursaId),
        nameof(Document.Autogenerat), nameof(Document.CorecteazaId), nameof(Document.MotivCorectie),
        nameof(Document.TotalStingere),
        GcRecord, LockField,
    };

    // Liniile: identitatea și gazda. `LotId` se copiază de aici (linia care
    // CONSUMĂ un lot îl păstrează) și se rescrie mai jos doar pe linia care l-a
    // NĂSCUT.
    static readonly HashSet<string> ExcluseLinie = new(StringComparer.Ordinal) {
        nameof(DocumentDetaliu.ID), nameof(DocumentDetaliu.DocumentId), GcRecord, LockField,
    };

    // Lotul renăscut: identitatea, linia-mamă și cele două câmpuri pe care le
    // scrie MOTORUL la operare (26e) — noul lot pornește nefinalizat, ca orice
    // lot născut la culegere.
    static readonly HashSet<string> ExcluseLot = new(StringComparer.Ordinal) {
        nameof(Lot.ID), nameof(Lot.LinieIntrareId), nameof(Lot.Data), nameof(Lot.PretUnitar),
        GcRecord, LockField,
    };

    const string GcRecord = "GCRecord";
    const string LockField = "OptimisticLockField";

    public static (Document Storno, Document Corectie) Corecteaza(IObjectSpace os, Guid documentId,
            DateOnly dataCorectie, MotivCorectie motiv) {
        using var tx = TranzactieComanda.Incepe(os);
        var original = Rezolva.Cere<Document>(os, documentId, "Documentul");
        // Ordinea celor două refuzuri: cel SPECIFIC întâi. Un original corectat e
        // deja `Stornat`, deci verificarea de stare l-ar fi înghițit cu mesajul
        // generic, iar operatorul n-ar fi aflat CINE îl corectează.
        var deja = os.GetObjectsQuery<Document>()
            .Where(d => d.CorecteazaId == documentId)
            .Select(d => new { d.Numar, d.Data })
            .FirstOrDefault();
        if (deja != null)
            throw new OperareException(
                $"Documentul {Eticheta(original.Numar, original.Data)} e deja corectat de "
                + $"{Eticheta(deja.Numar, deja.Data)} — legătura de corecție e 1:1.");
        if (original.Stare != StareDocument.Operat)
            throw new OperareException("Se corectează doar un document operat.");

        // Storno-ul, cu toți gardienii lui (perioada corecției deschisă, data
        // ≥ data înregistrării, fără dependenți, fără împerecheri).
        MotorOperare.Storneaza(os, original, dataCorectie);

        var db = DbContext(os);
        var corectie = (Document)os.CreateObject(db.Entry(original).Metadata.ClrType);
        Copiaza(db, original, corectie, ExcluseDocument);
        corectie.DataInregistrare = dataCorectie;
        corectie.CorecteazaId = original.ID;
        corectie.MotivCorectie = motiv;
        corectie.Stare = StareDocument.Draft;

        foreach (var linie in os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => d.DocumentId == documentId).ToList()) {
            var copie = (DocumentDetaliu)os.CreateObject(db.Entry(linie).Metadata.ClrType);
            Copiaza(db, linie, copie, ExcluseLinie);
            copie.Document = corectie;
            RenasteLotul(os, db, linie, copie);
        }

        // Efectul FISCAL al motivului (F27-D6): eroarea materială aparține
        // perioadei originalului, deci rândurile inverse tocmai scrise de storno
        // se declară acolo, nu în perioada stornării (JT-D5 rămâne regula pentru
        // faptul nou). Rândurile documentului NOU primesc aceeași perioadă la
        // operarea lui, prin `RegistruTvaService.PerioadaDeclarare`.
        var alOriginalului = motiv == MotivCorectie.EroareMateriala
            ? RegistruTvaService.PerioadaOriginalului(os, documentId)
            : null;
        if (alOriginalului is { } perioada)
            foreach (var rand in os.GetObjectsQuery<RegistruTva>()
                    .Where(r => r.DocumentId == documentId && r.Storno).ToList()) {
                rand.PerioadaAn = perioada.An;
                rand.PerioadaLuna = perioada.Luna;
            }

        var erori = new List<string>();
        GardianEditare.VerificaLegaturaCorectiei(os, corectie, erori);
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));

        os.CommitChanges();
        tx.Commit();
        return (original, corectie);
    }

    // Regula lotului, inversă celei prin care motorul îl recunoaște
    // (`Lot.LinieIntrareId == linie.ID` la finalizare, `MotorOperare.Opereaza`):
    // linia care a NĂSCUT lotul primește unul PROPRIU, nou, nefinalizat — marfa
    // recepționată o dată nu se poate recepționa a doua oară pe același lot, iar
    // lotul vechi rămâne cu sold zero prin rândurile de storno. Linia care doar
    // CONSUMĂ un lot (ieșirile, pinul de picking) îl păstrează prin `LotId`,
    // copiat ca orice alt FK.
    static void RenasteLotul(IObjectSpace os, DbContext db, DocumentDetaliu linie, DocumentDetaliu copie) {
        var lot = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linie.ID);
        if (lot == null)
            return;
        var lotNou = os.CreateObject<Lot>();
        Copiaza(db, lot, lotNou, ExcluseLot);
        lotNou.LinieIntrareId = copie.ID;
        copie.Lot = lotNou;
    }

    // Copierea GENERICĂ prin metadata EF: `GetProperties()` dă, pe TPT, toate
    // proprietățile mapate ale lanțului (bază + derivate), inclusiv FK-urile.
    // Navigațiile nu sunt proprietăți, deci nu se ating: relațiile se refac din
    // FK-urile copiate. Proprietățile-umbră n-au CLR-corespondent și n-au ce
    // purta din culegere.
    static void Copiaza(DbContext db, object sursa, object tinta, HashSet<string> excluse) {
        var intrareSursa = db.Entry(sursa);
        var intrareTinta = db.Entry(tinta);
        foreach (var proprietate in intrareSursa.Metadata.GetProperties()) {
            if (proprietate.IsShadowProperty() || proprietate.IsPrimaryKey()
                    || excluse.Contains(proprietate.Name))
                continue;
            intrareTinta.Property(proprietate.Name).CurrentValue =
                intrareSursa.Property(proprietate.Name).CurrentValue;
        }
    }

    static DbContext DbContext(IObjectSpace os) =>
        os is EFCoreObjectSpace efCore
            ? efCore.DbContext
            : throw new InvalidOperationException(
                $"Corecția cere un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`, "
                + "deci culegerea nu poate fi copiată prin metadata modelului.");

    static string Eticheta(string numar, DateOnly data) =>
        string.IsNullOrWhiteSpace(numar) ? $"({data:dd.MM.yyyy})" : numar;
}
