using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Motor;

public static class FiscalitateService {
    const long CheieDepuneri = 103_300_394;

    public static void BlocheazaScrierea(IObjectSpace os, Document doc) {
        if (doc is IDocumentFiscal)
            ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw(
                "SELECT pg_advisory_xact_lock_shared({0})", CheieDepuneri);
    }

    public static void VerificaAnularea(IObjectSpace os, Document doc) {
        if (doc is not IDocumentFiscal) return;
        var depuneri = os.GetObjectsQuery<DepunereDeclaratie>();
        if (os.GetObjectsQuery<Cub.Postare>().Any(p => p.DocumentId == doc.ID && p.TipTvaId != null
                && depuneri.Any(d => d.Formular == FormularFiscal.D300 && d.Perioada == p.PerioadaDeclarare
                    || d.Formular == FormularFiscal.D394 && d.Perioada == p.PerioadaD394)))
            throw new OperareException("TVA_DEJA_DECLARATA: faptele declarate se corectează prin inversă, nu se șterg prin anularea operării.");
    }

    public sealed record Atribuire(int PerioadaD300, N.ReperFiscal Reper);

    public static int Luna(DateOnly data) => data.Year * 100 + data.Month;

    public static Atribuire Atribuie(IObjectSpace os, Document doc, DirectieTva directie) {
        var inregistrare = doc.DataInregistrare == default ? doc.Data : doc.DataInregistrare;
        if (doc.CorecteazaId is Guid original && doc.MotivCorectie == MotivCorectie.EroareMateriala
                && Reper(os, original, N.FelTranzactie.Storno) is { } atribuire) {
            if (atribuire.Reper.DataInregistrare != inregistrare)
                throw new OperareException("TVA_CORECTIE_DATA: corecția păstrează data inversei deja înregistrate.");
            return atribuire;
        }
        var exigibilitate = (doc as IDocumentFiscal)?.DataExigibilitate ?? doc.Data;
        DateOnly? primire = directie == DirectieTva.Deductibil
            ? (doc as IDocumentFiscalPrimit)?.DataPrimire ?? inregistrare : null;
        if (primire > inregistrare || exigibilitate > inregistrare)
            throw new OperareException("TVA_DATE: primirea și exigibilitatea nu pot urma înregistrării fiscale.");
        var perioada = directie == DirectieTva.Deductibil ? Luna(inregistrare) : Luna(exigibilitate);
        var regularizare = perioada < Luna(inregistrare)
            && EsteDepusa(os, FormularFiscal.D300, perioada);
        return new(regularizare ? Luna(inregistrare) : perioada,
            new(doc.ID, doc.Data, exigibilitate, primire, inregistrare,
                Luna(primire ?? doc.Data), regularizare));
    }

    public static Atribuire Original(IObjectSpace os, Guid document) => Reper(os, document, N.FelTranzactie.Operare);

    static Atribuire Reper(IObjectSpace os, Guid document, N.FelTranzactie fel) {
        var repere = os.GetObjectsQuery<Cub.Postare>()
            .Where(p => p.DocumentId == document && p.TipTvaId != null && p.Tranzactie.Fel == fel)
            .Select(p => new { p.PerioadaDeclarare, p.DocumentFiscalId, p.DataDocument, p.DataExigibilitate,
                p.DataPrimire, p.DataInregistrare, p.PerioadaD394, p.RegularizareD300 })
            .Distinct().ToArray();
        if (repere.Length == 0) return null;
        if (repere.Length != 1)
            throw new OperareException("TVA_REPER: documentul are atribuiri fiscale incompatibile.");
        var r = repere[0];
        return new(r.PerioadaDeclarare!.Value, new(r.DocumentFiscalId!.Value,
            r.DataDocument!.Value, r.DataExigibilitate!.Value, r.DataPrimire,
            r.DataInregistrare!.Value, r.PerioadaD394!.Value, r.RegularizareD300));
    }

    public static Atribuire Corectie(IObjectSpace os, Atribuire original, DateOnly data) {
        var regularizare = EsteDepusa(os, FormularFiscal.D300, original.PerioadaD300);
        return new(regularizare ? Luna(data) : original.PerioadaD300,
            original.Reper with { DataInregistrare = data,
                RegularizareD300 = regularizare || original.Reper.RegularizareD300 });
    }

    public static bool EsteDepusa(IObjectSpace os, FormularFiscal formular, int perioada) =>
        os.GetObjectsQuery<DepunereDeclaratie>().Any(d => d.Formular == formular && d.Perioada == perioada);

    public static Guid ConfirmaDepunerea(IObjectSpace os, FormularFiscal formular, int an, int luna,
            string versiuneExportata, string utilizator) {
        if (!Enum.IsDefined(formular) || an < 1900 || an > 9999 || luna < 1 || luna > 12
                || string.IsNullOrWhiteSpace(versiuneExportata))
            throw new OperareException("DEPUNERE_DATE: formularul, perioada și versiunea exportată sunt obligatorii.");
        var perioada = an * 100 + luna;
        using var tx = TranzactieComanda.Incepe(os);
        ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw(
            "SELECT pg_advisory_xact_lock({0})", CheieDepuneri);
        var existenta = os.GetObjectsQuery<DepunereDeclaratie>().FirstOrDefault(d =>
            d.Formular == formular && d.Perioada == perioada && d.VersiuneExportata == versiuneExportata);
        if (existenta != null) return existenta.ID;
        var drafturi = os.GetObjectsQuery<Document>().Where(d => d.Stare == StareDocument.Draft
            && d.CorecteazaId != null && d.MotivCorectie == MotivCorectie.EroareMateriala);
        if (os.GetObjectsQuery<Cub.Postare>().Any(p => p.InversaTehnica
                && (formular == FormularFiscal.D300 ? p.PerioadaDeclarare == perioada : p.PerioadaD394 == perioada)
                && drafturi.Any(d => d.CorecteazaId == p.DocumentId)))
            throw new OperareException("DEPUNERE_CORECTIE_DRAFT: finalizați corecția începută înainte de confirmarea depunerii.");
        var inceput = new DateOnly(an, luna, 1);
        if (versiuneExportata != Cub.Citiri.Fiscale.Versiune(os, formular, inceput, inceput.AddMonths(1).AddDays(-1)))
            throw new OperareException("DEPUNERE_VERSIUNE_DEPASITA: faptele perioadei diferă de export; exportați din nou înainte de confirmare.");
        var depunere = os.CreateObject<DepunereDeclaratie>();
        depunere.Formular = formular;
        depunere.Perioada = perioada;
        depunere.VersiuneExportata = versiuneExportata;
        depunere.ConfirmataLa = DateTime.UtcNow;
        depunere.ConfirmataDe = utilizator;
        os.CommitChanges();
        tx.Commit();
        return depunere.ID;
    }
}
