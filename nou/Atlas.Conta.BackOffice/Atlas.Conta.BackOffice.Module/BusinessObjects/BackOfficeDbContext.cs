using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.ExpressApp.EFCore.Updating;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF.StateMachine;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects {
    // Factory pentru design-time (dotnet ef migrations/database) — schema e
    // gestionată exclusiv prin migrații EF (update-ul automat XAF e dezactivat
    // în Startup). Connection string-ul oglindește appsettings.json.
    public class BackOfficeDesignTimeDbContextFactory : DesignTimeDbContextFactory<BackOfficeEFCoreDbContext> {
        protected override string ConnectionString =>
            "EFCoreProvider=Postgres;Host=localhost;Port=5444;Username=postgres;Password=postgres;Persist Security Info=True;Database=Atlas.Conta.BackOffice";
    }

    [TypesInfoInitializer(typeof(DbContextTypesInfoInitializer<BackOfficeEFCoreDbContext>))]
    public class BackOfficeEFCoreDbContext : DbContext {
        public BackOfficeEFCoreDbContext(DbContextOptions<BackOfficeEFCoreDbContext> options) : base(options) {
        }
        //public DbSet<ModuleInfo> ModulesInfo { get; set; }
        public DbSet<ModelDifference> ModelDifferences { get; set; }
        public DbSet<ModelDifferenceAspect> ModelDifferenceAspects { get; set; }
        public DbSet<PermissionPolicyRole> Roles { get; set; }
        public DbSet<Atlas.Conta.BackOffice.Module.BusinessObjects.ApplicationUser> Users { get; set; }
        public DbSet<Atlas.Conta.BackOffice.Module.BusinessObjects.ApplicationUserLoginInfo> UserLoginsInfo { get; set; }
        public DbSet<FileData> FileData { get; set; }
        public DbSet<ReportDataV2> ReportDataV2 { get; set; }
        public DbSet<StateMachine> StateMachines { get; set; }
        public DbSet<StateMachineState> StateMachineStates { get; set; }
        public DbSet<StateMachineTransition> StateMachineTransitions { get; set; }
        public DbSet<StateMachineAppearance> StateMachineAppearances { get; set; }
        public DbSet<DashboardData> DashboardData { get; set; }
        public DbSet<AuditDataItemPersistent> AuditData { get; set; }
        public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReferences { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<HCategory> HCategories { get; set; }

        // Nomenclatoare
        public DbSet<Repartitor> Repartitori { get; set; }
        public DbSet<Partener> Parteneri { get; set; }
        public DbSet<Angajat> Angajati { get; set; }
        public DbSet<Gestiune> Gestiuni { get; set; }
        public DbSet<UnitateInterna> UnitatiInterne { get; set; }
        public DbSet<ContPropriu> ConturiProprii { get; set; }
        public DbSet<ClasaProdus> ClaseProduse { get; set; }
        public DbSet<TipMaterial> TipuriMaterial { get; set; }
        public DbSet<Produs> Produse { get; set; }
        public DbSet<Lot> Loturi { get; set; }
        public DbSet<Cont> Conturi { get; set; }
        public DbSet<CodFunctional> CoduriFunctionale { get; set; }
        public DbSet<CodEconomic> CoduriEconomice { get; set; }
        public DbSet<SursaFinantare> SurseFinantare { get; set; }
        public DbSet<Proiect> Proiecte { get; set; }
        public DbSet<Unitate> Unitati { get; set; }
        public DbSet<Angajament> Angajamente { get; set; }
        public DbSet<PerioadaFiscala> PerioadeFiscale { get; set; }

        // Documente (TPT)
        public DbSet<Document> Documente { get; set; }
        public DbSet<DocumentDetaliu> DocumentDetalii { get; set; }
        public DbSet<FacturaIntrare> FacturiIntrare { get; set; }
        public DbSet<FacturaIntrareDetaliu> FacturiIntrareDetalii { get; set; }
        public DbSet<FacturaIesire> FacturiIesire { get; set; }
        public DbSet<FacturaIesireDetaliu> FacturiIesireDetalii { get; set; }
        public DbSet<NIR> NIRuri { get; set; }
        public DbSet<BonConsum> BonuriConsum { get; set; }
        public DbSet<NotaTransfer> NoteTransfer { get; set; }
        public DbSet<ListaDiferenteInventar> ListeDiferenteInventar { get; set; }
        public DbSet<ListaDiferenteInventarDetaliu> ListeDiferenteInventarDetalii { get; set; }
        public DbSet<Decont> Deconturi { get; set; }
        public DbSet<DecontDetaliu> DecontDetalii { get; set; }
        public DbSet<Plata> Plati { get; set; }
        public DbSet<Incasare> Incasari { get; set; }
        public DbSet<RaportProductie> RapoarteProductie { get; set; }
        public DbSet<Imperechere> Imperecheri { get; set; }

        // Registre + politici
        public DbSet<RegistruStoc> RegistruStoc { get; set; }
        public DbSet<RegistruContabil> RegistruContabil { get; set; }
        public DbSet<TipDocument> TipuriDocument { get; set; }
        public DbSet<RegulaStoc> ReguliStoc { get; set; }
        public DbSet<RegulaContare> ReguliContare { get; set; }
        public DbSet<PoliticaConex> PoliticiConex { get; set; }
        public DbSet<PoliticaNumerotare> PoliticiNumerotare { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseDeferredDeletion(this);
            modelBuilder.UseOptimisticLock();
            modelBuilder.SetOneToManyAssociationDeleteBehavior(DeleteBehavior.SetNull, DeleteBehavior.Cascade);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
            modelBuilder.Entity<Atlas.Conta.BackOffice.Module.BusinessObjects.ApplicationUserLoginInfo>(b => {
                b.HasIndex(nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.LoginProviderName), nameof(DevExpress.ExpressApp.Security.ISecurityUserLoginInfo.ProviderUserKey)).IsUnique();
            });
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.AuditItems)
                .WithOne(p => p.AuditedObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.OldItems)
                .WithOne(p => p.OldObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.NewItems)
                .WithOne(p => p.NewObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.UserItems)
                .WithOne(p => p.UserObject);
            modelBuilder.Entity<StateMachine>()
                .HasMany(t => t.States)
                .WithOne(t => t.StateMachine)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ModelDifference>()
                .HasMany(t => t.Aspects)
                .WithOne(t => t.Owner)
                .OnDelete(DeleteBehavior.Cascade);

            // Deciziile 1/3/16: TPT pe cele trei ierarhii.
            modelBuilder.Entity<Document>().UseTptMappingStrategy();
            modelBuilder.Entity<DocumentDetaliu>().UseTptMappingStrategy();
            modelBuilder.Entity<Repartitor>().UseTptMappingStrategy();

            modelBuilder.Entity<Document>()
                .HasMany(d => d.Detalii)
                .WithOne(d => d.Document)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Două relații distincte linie↔lot (altfel convenția le împerechează 1:1):
            // linia de ieșire REFERĂ un lot; lotul e CREAT de o linie de intrare.
            modelBuilder.Entity<DocumentDetaliu>()
                .HasOne(d => d.Lot).WithMany().HasForeignKey(d => d.LotId);
            modelBuilder.Entity<Lot>()
                .HasOne(l => l.LinieIntrare).WithMany().HasForeignKey(l => l.LinieIntrareId);

            // Owned type Dimensiuni (decizia 15) — pe linia de document, pe rândul
            // de registru contabil și pe cele trei seturi ale regulii de contare.
            // Navigația e REQUIRED: altfel, la table sharing cu toate coloanele
            // nullable, un rând complet null s-ar materializa ca Dimensiuni=null
            // (nu obiect gol) și ar sparge coalesce-ul motorului cu NRE.
            modelBuilder.Entity<DocumentDetaliu>().OwnsOne(d => d.Dimensiuni, ConfigureDimensiuni)
                .Navigation(d => d.Dimensiuni).IsRequired();
            modelBuilder.Entity<RegistruContabil>().OwnsOne(r => r.Dimensiuni, ConfigureDimensiuni)
                .Navigation(r => r.Dimensiuni).IsRequired();
            modelBuilder.Entity<RegulaContare>().OwnsOne(r => r.DimensiuniComun, ConfigureDimensiuni)
                .Navigation(r => r.DimensiuniComun).IsRequired();
            modelBuilder.Entity<RegulaContare>().OwnsOne(r => r.DimensiuniOverrideDebit, ConfigureDimensiuni)
                .Navigation(r => r.DimensiuniOverrideDebit).IsRequired();
            modelBuilder.Entity<RegulaContare>().OwnsOne(r => r.DimensiuniOverrideCredit, ConfigureDimensiuni)
                .Navigation(r => r.DimensiuniOverrideCredit).IsRequired();
        }

        private static void ConfigureDimensiuni<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder<T, Dimensiuni> b) where T : class {
            b.HasOne(d => d.Repartitor).WithMany().HasForeignKey(d => d.RepartitorId);
            b.HasOne(d => d.Material).WithMany().HasForeignKey(d => d.MaterialId);
            b.HasOne(d => d.CodFunctional).WithMany().HasForeignKey(d => d.CodFunctionalId);
            b.HasOne(d => d.CodEconomic).WithMany().HasForeignKey(d => d.CodEconomicId);
            b.HasOne(d => d.SursaFinantare).WithMany().HasForeignKey(d => d.SursaFinantareId);
            b.HasOne(d => d.Unitate).WithMany().HasForeignKey(d => d.UnitateId);
            b.HasOne(d => d.Proiect).WithMany().HasForeignKey(d => d.ProiectId);
            b.HasOne(d => d.CentruCost).WithMany().HasForeignKey(d => d.CentruCostId);
        }
    }

    public class BackOfficeAuditingDbContext : DbContext {
        public BackOfficeAuditingDbContext(DbContextOptions<BackOfficeAuditingDbContext> options) : base(options) {
        }
        public DbSet<AuditDataItemPersistent> AuditData { get; set; }
        public DbSet<AuditEFCoreWeakReference> AuditEFCoreWeakReferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseDeferredDeletion(this);
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.AuditItems)
                .WithOne(p => p.AuditedObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.OldItems)
                .WithOne(p => p.OldObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.NewItems)
                .WithOne(p => p.NewObject);
            modelBuilder.Entity<AuditEFCoreWeakReference>()
                .HasMany(p => p.UserItems)
                .WithOne(p => p.UserObject);
        }
    }
}
