using Atlas.DXF.EfCore.Owned;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF.StateMachine;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<TipTva> TipuriTva { get; set; }

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
        public DbSet<DescarcareGestiune> DescarcariGestiune { get; set; }
        public DbSet<DescarcareGestiuneDetaliu> DescarcariGestiuneDetalii { get; set; }
        public DbSet<NotaContabila> NoteContabile { get; set; }
        public DbSet<NotaContabilaDetaliu> NoteContabileDetalii { get; set; }
        // Al 13-lea derivat (FAZA 1C §6): închiderea lunară de TVA — notă
        // contabilă GENERATĂ (TPT pe două niveluri: Documente → NoteContabile →
        // InchideriTva); detaliul rămâne NotaContabilaDetaliu.
        public DbSet<InchidereTva> InchideriTva { get; set; }
        public DbSet<Imperechere> Imperecheri { get; set; }

        // Registre + politici
        public DbSet<RegistruStoc> RegistruStoc { get; set; }
        public DbSet<RegistruContabil> RegistruContabil { get; set; }
        public DbSet<TipDocument> TipuriDocument { get; set; }
        public DbSet<RegulaStoc> ReguliStoc { get; set; }
        public DbSet<RegulaContare> ReguliContare { get; set; }
        public DbSet<PoliticaConex> PoliticiConex { get; set; }
        public DbSet<PoliticaNumerotare> PoliticiNumerotare { get; set; }
        public DbSet<PoliticaScadenta> PoliticiScadenta { get; set; }
        public DbSet<PoliticaValidare> PoliticiValidare { get; set; }
        public DbSet<PoliticaTva> PoliticiTva { get; set; }
        public DbSet<PoliticaInchidereTva> PoliticiInchidereTva { get; set; }

        // Infrastructura migrării (pasul 4): corelare legacy → nou.
        public DbSet<MigrareLegatura> MigrareLegaturi { get; set; }

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

            // Linia REFERĂ un lot prin FK real; sensul invers (lotul e CREAT de o
            // linie de intrare) e doar coloana Lot.LinieIntrareId, fără FK — vezi
            // comentariul din Lot (ciclu de inserție altfel).
            modelBuilder.Entity<DocumentDetaliu>()
                .HasOne(d => d.Lot).WithMany().HasForeignKey(d => d.LotId);

            // Trasabilitatea acoperirii per linie FCL (design P2 §3): linia DSC
            // referă linia FCL sursă printr-un FK real cross-document. Restrict —
            // linia sursă nu se șterge cât e referită de o descărcare (ca sensul
            // linie→lot de mai sus, care nu cascadează pe ștergerea lotului).
            modelBuilder.Entity<DescarcareGestiuneDetaliu>()
                .HasOne(d => d.LinieSursa).WithMany().HasForeignKey(d => d.LinieSursaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Owned type Dimensiuni (decizia 15) — pe linia de document, pe rândul
            // de registru contabil și pe cele trei seturi ale regulii de contare.
            // OwnsOneRequired (Atlas.DXF): navigație REQUIRED, altfel la table sharing
            // un rând complet null s-ar materializa ca Dimensiuni=null (nu obiect gol)
            // și ar sparge coalesce-ul motorului cu NRE.
            modelBuilder.Entity<DocumentDetaliu>().OwnsOneRequired(d => d.Dimensiuni, ConfigureDimensiuni);
            // Pe registru navigațiile interne se încarcă EAGER (AutoInclude):
            // grid-urile și Dimensiuni.ToString le citesc pe fiecare rând — lazy
            // ar însemna N+1 per instanță de owned (identity map-ul nu ajută,
            // fiecare rând are instanța lui) plus lazy-load pe OS disposed la
            // render târziu. Doar aici — liniile de document și regulile de
            // contare rămân lazy (motorul le interoghează în hot-path fără să
            // afișeze etichete).
            modelBuilder.Entity<RegistruContabil>().OwnsOneRequired(r => r.DimensiuniDebit, ConfigureDimensiuniEager);
            modelBuilder.Entity<RegistruContabil>().OwnsOneRequired(r => r.DimensiuniCredit, ConfigureDimensiuniEager);
            modelBuilder.Entity<RegulaContare>().OwnsOneRequired(r => r.DimensiuniComun, ConfigureDimensiuni);
            modelBuilder.Entity<RegulaContare>().OwnsOneRequired(r => r.DimensiuniOverrideDebit, ConfigureDimensiuni);
            modelBuilder.Entity<RegulaContare>().OwnsOneRequired(r => r.DimensiuniOverrideCredit, ConfigureDimensiuni);

            modelBuilder.Entity<MigrareLegatura>()
                .HasIndex(m => new { m.Tabela, m.CheieLegacy }).IsUnique();
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

        private static void ConfigureDimensiuniEager<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.OwnedNavigationBuilder<T, Dimensiuni> b) where T : class {
            ConfigureDimensiuni(b);
            b.Navigation(d => d.Repartitor).AutoInclude();
            b.Navigation(d => d.Material).AutoInclude();
            b.Navigation(d => d.CodFunctional).AutoInclude();
            b.Navigation(d => d.CodEconomic).AutoInclude();
            b.Navigation(d => d.SursaFinantare).AutoInclude();
            b.Navigation(d => d.Unitate).AutoInclude();
            b.Navigation(d => d.Proiect).AutoInclude();
            b.Navigation(d => d.CentruCost).AutoInclude();
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
