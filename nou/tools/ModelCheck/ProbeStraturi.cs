using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
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

    // 104b: coaja refuză înaintea oricărui context, apoi își deschide singură contextul comenzii.
    public static void VerificaCoaja(Action<string, bool> check) {
        var comenzi = new (string Nume, Action<ComenziDocument> Ruleaza)[] {
            ("Opereaza", c => c.Opereaza(Guid.NewGuid())),
            ("AnuleazaOperarea", c => c.AnuleazaOperarea(Guid.NewGuid())),
            ("Storneaza", c => c.Storneaza(Guid.NewGuid(), new DateOnly(2026, 1, 31))),
            ("Corecteaza", c => c.Corecteaza(Guid.NewGuid(), new DateOnly(2026, 1, 31), MotivCorectie.EroareMateriala)),
            ("Valideaza", c => c.Valideaza(Guid.NewGuid())),
        };
        var refuzuri = new (string Nume, Func<Exception> Refuz)[] {
            ("404", () => new SubiectInvizibil()),
            ("403", () => new RefuzAcces(OperatieAcces.Modificare, typeof(Document))),
        };
        var abateri = new List<string>();
        foreach (var (nume, ruleaza) in comenzi) {
            foreach (var (cod, refuz) in refuzuri) {
                var fabrica = new FabricaNumarata();
                var prins = Prinde(() => ruleaza(new ComenziDocument(fabrica, new DreptRefuzat(refuz))));
                if (prins?.GetType() != refuz().GetType() || fabrica.Deschideri != 0)
                    abateri.Add($"{nume}/{cod}: {prins?.GetType().Name ?? "fără refuz"}, {fabrica.Deschideri} contexte");
            }
            var permisa = new FabricaNumarata();
            if (Prinde(() => ruleaza(new ComenziDocument(permisa, new DreptRefuzat(null)))) is not ContextDeschis
                    || permisa.Deschideri != 1)
                abateri.Add($"{nume}/permis: {permisa.Deschideri} contexte");
        }
        check("104b: coaja refuză 404/403 fără să deschidă contextul și, cu drept, își deschide singură "
            + "un context non-secured per comandă" + Abateri(abateri), abateri.Count == 0);

        var cuContext = typeof(ComenziDocument).GetMethods(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(m => m.GetParameters().Any(p => typeof(IObjectSpace).IsAssignableFrom(p.ParameterType)))
            .Select(m => m.Name).ToList();
        check("104b: comenzile cojii nu primesc `IObjectSpace` de la apelant" + Abateri(cuContext), cuContext.Count == 0);
    }

    static Exception Prinde(Action actiune) {
        try {
            actiune();
            return null;
        }
        catch (Exception ex) {
            return ex;
        }
    }

    sealed class DreptRefuzat(Func<Exception> refuz) : IDreptComanda {
        public void Cere(Guid documentId, ComandaDocument comanda) {
            if (refuz?.Invoke() is { } ex)
                throw ex;
        }
    }

    sealed class ContextDeschis : Exception;

    sealed class FabricaNumarata : INonSecuredObjectSpaceFactory {
        public int Deschideri { get; private set; }
        public IObjectSpace CreateNonSecuredObjectSpace(Type objectType) {
            Deschideri++;
            throw new ContextDeschis();
        }
    }

    static string Abateri(List<string> abateri) =>
        abateri.Count == 0 ? "" : " — abateri: " + string.Join(", ", abateri);
}
