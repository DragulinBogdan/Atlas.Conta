using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF.StateMachine;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;
// Numele DbSet-ului `RegistruContabil` umbrește tipul în interiorul contextului —
// alias pentru nameof-urile de mai jos (AutoInclude pe navigațiile plate).
using RegistruContabilEntitate = Atlas.Conta.BackOffice.Module.BusinessObjects.RegistruContabil;

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
        public DbSet<NirDetaliu> NIRDetalii { get; set; }
        public DbSet<BonConsum> BonuriConsum { get; set; }
        public DbSet<NotaTransfer> NoteTransfer { get; set; }
        public DbSet<ListaDiferenteInventar> ListeDiferenteInventar { get; set; }
        public DbSet<ListaDiferenteInventarDetaliu> ListeDiferenteInventarDetalii { get; set; }
        public DbSet<Decont> Deconturi { get; set; }
        public DbSet<DecontDetaliu> DecontDetalii { get; set; }
        public DbSet<Plata> Plati { get; set; }
        public DbSet<Incasare> Incasari { get; set; }
        public DbSet<DocumentTrezorerieDetaliu> TrezorerieDetalii { get; set; }
        public DbSet<RaportProductie> RapoarteProductie { get; set; }
        public DbSet<DescarcareGestiune> DescarcariGestiune { get; set; }
        public DbSet<DescarcareGestiuneDetaliu> DescarcariGestiuneDetalii { get; set; }
        public DbSet<NotaContabila> NoteContabile { get; set; }
        public DbSet<NotaContabilaDetaliu> NoteContabileDetalii { get; set; }
        // Al 13-lea derivat (FAZA 1C §6): închiderea lunară de TVA — notă
        // contabilă GENERATĂ (TPT pe două niveluri: Documente → NoteContabile →
        // InchideriTva); detaliul rămâne NotaContabilaDetaliu.
        public DbSet<InchidereTva> InchideriTva { get; set; }
        // Al 14-lea derivat (FAZA 1C §7): asamblarea/kitting n→m pe stoc
        // (BPR rămâne rezervat — decizia 19).
        public DbSet<Asamblare> Asamblari { get; set; }
        public DbSet<AsamblareDetaliu> AsamblariDetalii { get; set; }
        // Al 15-lea și al 16-lea derivat (FAZA 1C §7): retururile pe corespondența
        // de storno; ambele folosesc detaliul de BAZĂ (fără tabele de detaliu).
        public DbSet<ReturFurnizor> RetururiFurnizor { get; set; }
        public DbSet<ReturClient> RetururiClient { get; set; }
        public DbSet<Imperechere> Imperecheri { get; set; }

        // Registre + politici
        public DbSet<RegistruStoc> RegistruStoc { get; set; }
        public DbSet<RegistruContabil> RegistruContabil { get; set; }
        // Fără `AutoInclude` pe navigațiile lui — deliberat, spre deosebire de
        // `RegistruContabil` (41c). Acolo dimensiunile CHIAR se afișează pe
        // fiecare rând al grilei, deci lazy însemna N+1 per pagină plus
        // lazy-load pe OS disposed la render târziu. Aici view-ul XAF e o
        // suprafață de diagnostic — navigațiile se ascund din ListView
        // (`ContaUiBaseline`), iar consumatorii reali sunt proiecțiile, care
        // își fac join-urile explicit în `Select`. Nu există N+1 de prevenit.
        // Bonus: numele DbSet-ului poate coincide cu al clasei fără să ceară
        // alias-ul `using ...Entitate =` de care are nevoie `RegistruContabil`.
        public DbSet<RegistruTva> RegistruTva { get; set; }
        public DbSet<TipDocument> TipuriDocument { get; set; }
        public DbSet<RegulaStoc> ReguliStoc { get; set; }
        public DbSet<RegulaContare> ReguliContare { get; set; }
        public DbSet<PoliticaConex> PoliticiConex { get; set; }
        public DbSet<PoliticaNumerotare> PoliticiNumerotare { get; set; }
        public DbSet<PoliticaScadenta> PoliticiScadenta { get; set; }
        public DbSet<PoliticaValidare> PoliticiValidare { get; set; }
        public DbSet<PoliticaTva> PoliticiTva { get; set; }
        public DbSet<PoliticaInchidereTva> PoliticiInchidereTva { get; set; }
        // Setarea de profil a bazei (decizia 51c): un singur rând, scris de seed.
        public DbSet<SetareProfil> SetariProfil { get; set; }

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

            // Nivelul abstract intermediar al trezoreriei se declară EXPLICIT:
            // până la decizia 48b el intra în model doar fiindcă navigația
            // `Imperechere.DocumentTrezorerie` îl referea; odată relaxată la
            // `Document`, EF nu-l mai descoperea, iar Plata/Incasare ar fi
            // moștenit direct din Document — cu tabela `DocumentTrezorerie`
            // ștearsă și coloanele ei (TipInstrument/NumarExtras/DataExtras)
            // recreate goale pe frunze. Declarația ține schema neatinsă.
            modelBuilder.Entity<DocumentTrezorerie>();

            // F8-D6: latura pereche a viramentului — FK REAL self-referencing pe
            // nivelul abstract al trezoreriei. `WithMany()` fără colecție: sensul
            // invers e derivat prin query (`PerecheId`), nu materializat — o
            // colecție ar sugera „mai multe perechi", exact ce validarea refuză.
            // Restrict: piciorul arătat nu se șterge cât timp altcineva îl declară
            // pereche (ca `DescarcareGestiuneDetaliu.LinieSursa` mai jos).
            modelBuilder.Entity<DocumentTrezorerie>()
                .HasOne(d => d.LaturaPereche).WithMany().HasForeignKey(d => d.LaturaPerecheId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // DIM-3 (decizia 54c): maparea owned Dimensiuni a MURIT — registrul și
            // regula de contare poartă coloane plate ([Column] pe entități conservă
            // schema); `Dimensiuni` e value object ne-persistat al motorului.
            // Navigațiile dimensiunilor registrului se încarcă EAGER (motivul 41c,
            // neschimbat): grid-urile le afișează pe fiecare rând — lazy ar fi N+1
            // per rând plus lazy-load pe OS disposed la render târziu. Doar
            // registrul — regulile de contare rămân lazy (motorul citește scalari).
            foreach (var nav in new[] {
                         nameof(RegistruContabilEntitate.DebitRepartitor), nameof(RegistruContabilEntitate.DebitMaterial),
                         nameof(RegistruContabilEntitate.DebitCodFunctional), nameof(RegistruContabilEntitate.DebitCodEconomic),
                         nameof(RegistruContabilEntitate.DebitSursaFinantare), nameof(RegistruContabilEntitate.DebitUnitate),
                         nameof(RegistruContabilEntitate.DebitProiect), nameof(RegistruContabilEntitate.DebitCentruCost),
                         nameof(RegistruContabilEntitate.CreditRepartitor), nameof(RegistruContabilEntitate.CreditMaterial),
                         nameof(RegistruContabilEntitate.CreditCodFunctional), nameof(RegistruContabilEntitate.CreditCodEconomic),
                         nameof(RegistruContabilEntitate.CreditSursaFinantare), nameof(RegistruContabilEntitate.CreditUnitate),
                         nameof(RegistruContabilEntitate.CreditProiect), nameof(RegistruContabilEntitate.CreditCentruCost) })
                modelBuilder.Entity<RegistruContabilEntitate>().Navigation(nav).AutoInclude();

            modelBuilder.Entity<MigrareLegatura>()
                .HasIndex(m => new { m.Tabela, m.CheieLegacy }).IsUnique();

            AplicaScaraNumerica(modelBuilder);
        }

        // Scara fixă pe TOATE coloanele zecimale ale modelului (vezi `Scara`
        // pentru motiv: `numeric` fără scară moștenește scara împărțirii care a
        // produs valoarea, iar SUM-ul server-side peste ea depășește mantisa lui
        // `decimal`). Convenție centrală în locul a ~15 apeluri `HasPrecision`
        // împrăștiate: se aplică singură pe orice coloană nouă cu nume cunoscut.
        //
        // Gardianul (aruncă la construirea modelului) e jumătatea care contează:
        // un `decimal` cu nume nou nu poate ajunge în schemă fără scară — fail
        // zgomotos la pornire/migrare, nu drift descoperit peste luni într-un
        // OverflowException. Restrâns la tipurile PROPRII: tipurile DevExpress
        // mapate în context (audit, rapoarte, state machine) nu sunt ale noastre
        // de fixat, iar un `decimal` apărut la un upgrade de pachet n-are voie să
        // oprească aplicația.
        private static void AplicaScaraNumerica(ModelBuilder modelBuilder) {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
                if (entityType.ClrType?.Namespace?.StartsWith("Atlas.Conta.") != true)
                    continue;
                // Declarate, nu moștenite: sub TPT proprietatea bazei apare pe
                // fiecare derivată, dar aparține (și se configurează) o dată.
                foreach (var proprietate in entityType.GetDeclaredProperties()) {
                    var tip = Nullable.GetUnderlyingType(proprietate.ClrType) ?? proprietate.ClrType;
                    if (tip != typeof(decimal))
                        continue;
                    var scara = Scara.ScaraPentru(proprietate.Name)
                        ?? throw new InvalidOperationException(
                            $"Proprietatea zecimală {entityType.ClrType.Name}.{proprietate.Name} nu are scară " +
                            $"definită. Adaugă numele în Scara.ScaraPentru (bani / preț unitar / cantitate) — " +
                            $"o coloană `numeric` fără scară moștenește scara calculului care o produce și " +
                            $"sparge SUM-ul server-side.");
                    proprietate.SetPrecision(Scara.Precizie);
                    proprietate.SetScale(scara);
                }
            }
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
