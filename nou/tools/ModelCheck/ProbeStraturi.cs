using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp.DC;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Probele structurale ale deciziei 104 (h), pe modelul design-time.
static class ProbeStraturi {
    public static void Verifica(DbContext ctx, Action<string, bool> check) {
        var model = ctx.GetService<IDesignTimeModel>().Model;
        var domeniu = model.GetEntityTypes()
            .Where(e => e.ClrType.Namespace == typeof(EntitateConta).Namespace).ToList();

        var peBaseObject = domeniu
            .Where(e => typeof(DevExpress.Persistent.BaseImpl.EF.BaseObject).IsAssignableFrom(e.ClrType)
                && !typeof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo).IsAssignableFrom(e.ClrType)
                && !typeof(DevExpress.ExpressApp.Security.ISecurityUser).IsAssignableFrom(e.ClrType))
            .Select(e => e.ClrType.Name).ToList();
        check("104e: tipurile de domeniu derivă din `EntitateConta`, nu din `BaseObject`-ul DevExpress (securitatea rămâne)"
            + Abateri(peBaseObject), peBaseObject.Count == 0);

        var cuGcRecord = model.GetEntityTypes().Where(e => e.FindProperty("GCRecord") != null)
            .Select(e => e.ClrType.Name).ToList();
        check("104f: niciun tip din model nu are proprietatea `GCRecord`, iar modelul n-are marcajul de ștergere amânată"
            + Abateri(cuGcRecord), cuGcRecord.Count == 0 && model.FindAnnotation("IDeferredDeletion") == null);

        var cascade = domeniu.SelectMany(e => e.GetDeclaredForeignKeys())
            .Where(fk => typeof(EntitateConta).IsAssignableFrom(fk.DeclaringEntityType.ClrType))
            .Where(fk => (fk.DeleteBehavior == DeleteBehavior.Cascade) != BackOfficeEFCoreDbContext.EsteCompozitie(fk)
                || fk.DeleteBehavior is not (DeleteBehavior.Cascade or DeleteBehavior.ClientNoAction))
            .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name}.{fk.DependentToPrincipal?.Name ?? fk.Properties[0].Name}={fk.DeleteBehavior}")
            .ToList();
        var compozitii = domeniu.SelectMany(e => e.GetDeclaredForeignKeys()).Count(BackOfficeEFCoreDbContext.EsteCompozitie);
        check($"104g: `Cascade` numai pe compoziții (`[Aggregated]`, {compozitii}), refuz din bază (`ClientNoAction`) pe restul FK-urilor de domeniu"
            + Abateri(cascade), cascade.Count == 0 && compozitii > 0);

        var registreCuBlocare = domeniu.Where(e => typeof(RandRegistru).IsAssignableFrom(e.ClrType))
            .Where(e => e.GetProperties().Any(p => p.IsConcurrencyToken))
            .Select(e => e.ClrType.Name).ToList();
        check("104e: `RandRegistru` n-are câmp de blocare optimistă" + Abateri(registreCuBlocare),
            registreCuBlocare.Count == 0);

        var editabileFaraBlocare = domeniu.Where(e => typeof(Editabila).IsAssignableFrom(e.ClrType))
            .Where(e => e.FindProperty(nameof(Editabila.OptimisticLockField)) is not { IsConcurrencyToken: true })
            .Select(e => e.ClrType.Name).ToList();
        check("104e: fiecare `Editabila` are `OptimisticLockField` ca jeton de concurență" + Abateri(editabileFaraBlocare),
            editabileFaraBlocare.Count == 0);

        var faraCheie = Politici.TipuriConfigurabile
            .Where(t => !model.FindEntityType(t)!.GetIndexes().Any(i => i.IsUnique))
            .Select(t => t.Name).ToList();
        check("104i: fiecare tip configurabil are index unic — cheia refuzului de seed" + Abateri(faraCheie),
            faraCheie.Count == 0);
    }

    static string Abateri(List<string> abateri) =>
        abateri.Count == 0 ? "" : " — abateri: " + string.Join(", ", abateri);
}
