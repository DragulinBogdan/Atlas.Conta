using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF.StateMachine;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
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
        // Istoricul închiderilor/redeschiderilor de perioadă (F27-D1), append-only.
        public DbSet<InchiderePerioada> InchideriPerioade { get; set; }
        public DbSet<TipTva> TipuriTva { get; set; }
        // Nomenclatorul de județe (felia 15, D15-D1): ISO 3166-2:RO, seed-uit tot
        // în NUCLEU (împărțirea administrativă nu ține de planul de conturi).
        public DbSet<Judet> Judete { get; set; }
        // Nomenclatorul unităților de măsură (felia 16, D16-D2): UN/ECE Rec 20+21,
        // tot al nucleului — unitățile nu țin de planul de conturi.
        public DbSet<UnitateMasura> UnitatiMasura { get; set; }
        // Societatea raportoare (felia 16, D16-D1): UN SINGUR rând per bază
        // (gardianul îl apără; nu există „index unic pe nimic").
        public DbSet<Societate> Societati { get; set; }
        // Nomenclatorul rândurilor decontului de TVA (felia 12, D3-D1) — corpul
        // formularului 300 din OPANAF 174/2026, seed-uit în NUCLEU (e lege, nu profil).
        public DbSet<RandD300> RanduriD300 { get; set; }

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
        // Declarația vamală de import (86a), cu detaliul
        // de BAZĂ (fără tabelă de detaliu) și cu legătura n→m spre facturi.
        public DbSet<Dvi> Dvi { get; set; }
        public DbSet<DviFactura> DviFacturi { get; set; }
        // Cele trei derivate ale imobilizărilor (F26-D5/D6/D7).
        public DbSet<PunereInFunctiune> PuneriInFunctiune { get; set; }
        public DbSet<PunereInFunctiuneDetaliu> PuneriInFunctiuneDetalii { get; set; }
        public DbSet<IesireImobilizare> IesiriImobilizari { get; set; }
        public DbSet<IesireImobilizareDetaliu> IesiriImobilizariDetalii { get; set; }
        public DbSet<AmortizareLunara> AmortizariLunare { get; set; }
        public DbSet<AmortizareLunaraDetaliu> AmortizariLunareDetalii { get; set; }
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
        public DbSet<RegistruImobilizari> RegistruImobilizari { get; set; }
        public DbSet<SoldPerioadaContabil> SolduriPerioadaContabil { get; set; }
        public DbSet<SoldPerioadaStoc> SolduriPerioadaStoc { get; set; }
        public DbSet<Imobilizare> Imobilizari { get; set; }
        public DbSet<ClasificareImobilizari> ClasificariImobilizari { get; set; }
        public DbSet<PoliticaAmortizare> PoliticiAmortizare { get; set; }
        public DbSet<RegulaDeductibilitate> ReguliDeductibilitate { get; set; }
        public DbSet<TipDocument> TipuriDocument { get; set; }
        public DbSet<RegulaStoc> ReguliStoc { get; set; }
        public DbSet<RegulaContare> ReguliContare { get; set; }
        public DbSet<PoliticaConex> PoliticiConex { get; set; }
        public DbSet<PoliticaNumerotare> PoliticiNumerotare { get; set; }
        public DbSet<PoliticaScadenta> PoliticiScadenta { get; set; }
        public DbSet<PoliticaValidare> PoliticiValidare { get; set; }
        public DbSet<PoliticaTva> PoliticiTva { get; set; }
        public DbSet<PoliticaInchidereTva> PoliticiInchidereTva { get; set; }
        // Politica de așezare pe decont (D3-D2): (TipTva × Sens) → rând, n rânduri.
        public DbSet<MapareD300> MapariD300 { get; set; }
        // Politica D394 (D4-D2): (TipTva × Sens) → tip de operațiune, UNA per pereche.
        public DbSet<MapareD394> MapariD394 { get; set; }
        // Politica SAF-T S (felia 17, D17-D1): (TipDocument × TipStoc × Semn?) →
        // cod de mișcare + rolul terțului; cod null = excludere deliberată.
        public DbSet<PoliticaMiscareSaft> PoliticiMiscareSaft { get; set; }
        // Implicitul de TVA la culegere (felia 23, F23-D2): (TipDocument ×
        // ClasaFiscalaPartener? × ValabilDeLa?) → TipTva. Purtătorul de REGIM al
        // rezolvării din `ImpliciteService`; cota vine de pe produs.
        public DbSet<PoliticaTvaImplicit> PoliticiTvaImplicit { get; set; }
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

            // DVI-D3: legătura n→m declarație ↔ facturi de import. Perechea e
            // identitatea rândului (o factură o dată pe aceeași declarație),
            // filtrată pe `GCRecord = 0` ca toate unicitățile pe tipuri cu
            // ștergere amânată (60a) — dezlegarea și relegarea aceleiași facturi
            // pe un draft e flux normal. FK-uri `Restrict`: legătura nu dispare
            // tăcut nici pe capătul declarației, nici pe cel al facturii.
            modelBuilder.Entity<DviFactura>()
                .HasIndex(f => new { f.DviId, f.FacturaId }).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            modelBuilder.Entity<DviFactura>()
                .HasOne(f => f.Dvi).WithMany(d => d.Facturi).HasForeignKey(f => f.DviId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DviFactura>()
                .HasOne(f => f.Factura).WithMany().HasForeignKey(f => f.FacturaId)
                .OnDelete(DeleteBehavior.Restrict);

            // F27-D1: perioada e verigă de lanț, deci `(An, Luna)` e IDENTITATE, nu
            // o coincidență. Filtrat pe rândurile vii ca toate unicitățile de pe
            // tipuri cu ștergere amânată (60a) — altfel o perioadă ștearsă ar
            // bloca recrearea aceleiași luni. Fără index, `VerificaDeschisa`
            // (`FirstOrDefault`) ar fi ales nedeterminist între două rânduri.
            modelBuilder.Entity<PerioadaFiscala>()
                .HasIndex(p => new { p.An, p.Luna }).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            // Istoricul nu dispare cu perioada (Restrict) și nu e agregat al ei:
            // e registrul închiderilor, nu o colecție de culegere.
            modelBuilder.Entity<InchiderePerioada>(b => {
                b.HasOne(i => i.Perioada).WithMany(p => p.Istoric).HasForeignKey(i => i.PerioadaId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(i => i.PerioadaId);
            });

            // F27-D3: snapshot-urile perioadelor de referință. Cheia completă e
            // UNICĂ per perioadă, cu `NULLS NOT DISTINCT` (Postgres 15+) —
            // dimensiunile sunt nullable, iar semantica cerută e „aceleași
            // dimensiuni lipsă = aceeași cheie", nu „fiecare NULL e altceva".
            // Fără filtru pe `GCRecord`: rândurile se șterg FIZIC (nu e
            // nomenclator, e proiecție rescrisă la fiecare închidere).
            // FK-uri `Restrict` și fără `AutoInclude`: consumatorii agregă, nu
            // afișează — spre deosebire de `RegistruContabil` (41c).
            modelBuilder.Entity<SoldPerioadaContabil>(b => {
                b.HasIndex(s => new {
                    s.An, s.Luna, s.ContId, s.RepartitorId, s.MaterialId, s.CodFunctionalId,
                    s.CodEconomicId, s.SursaFinantareId, s.UnitateId, s.ProiectId, s.CentruCostId
                }).IsUnique().AreNullsDistinct(false);
                b.HasIndex(s => new { s.An, s.Luna });
                b.HasOne(s => s.Cont).WithMany().HasForeignKey(s => s.ContId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Repartitor).WithMany().HasForeignKey(s => s.RepartitorId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Material).WithMany().HasForeignKey(s => s.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.CodFunctional).WithMany().HasForeignKey(s => s.CodFunctionalId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.CodEconomic).WithMany().HasForeignKey(s => s.CodEconomicId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.SursaFinantare).WithMany().HasForeignKey(s => s.SursaFinantareId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Unitate).WithMany().HasForeignKey(s => s.UnitateId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Proiect).WithMany().HasForeignKey(s => s.ProiectId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.CentruCost).WithMany().HasForeignKey(s => s.CentruCostId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<SoldPerioadaStoc>(b => {
                b.HasIndex(s => new { s.An, s.Luna, s.LotId, s.RepartitorId, s.TipStoc }).IsUnique();
                b.HasIndex(s => new { s.An, s.Luna });
                b.HasOne(s => s.Lot).WithMany().HasForeignKey(s => s.LotId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(s => s.Repartitor).WithMany().HasForeignKey(s => s.RepartitorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Rulajele unei luni se citesc pe `Data` (spike B.4: 70,6 → 37,9 ms
            // pe contabil, 16,2 → 14,1 pe stoc, plan de index scan în loc de
            // parallel seq scan). Filtrat pe rândurile vii, ca toate citirile.
            modelBuilder.Entity<RegistruContabil>()
                .HasIndex(r => r.Data).HasFilter("\"GCRecord\" = 0");
            modelBuilder.Entity<RegistruStoc>()
                .HasIndex(r => r.Data).HasFilter("\"GCRecord\" = 0");

            // FK-uri `Restrict`: convenția globală `SetNull`/`Cascade` ar goli tăcut fișa sau linia-sursă (F26-D1/D2/D5).
            modelBuilder.Entity<Imobilizare>(b => {
                b.HasOne(f => f.TipMaterial).WithMany().HasForeignKey(f => f.TipMaterialId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(f => f.Clasificare).WithMany().HasForeignKey(f => f.ClasificareId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(f => f.Loc).WithMany().HasForeignKey(f => f.LocId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(f => f.CentruCost).WithMany().HasForeignKey(f => f.CentruCostId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(f => f.CodEconomic).WithMany().HasForeignKey(f => f.CodEconomicId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(f => f.Responsabil).WithMany().HasForeignKey(f => f.ResponsabilId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<RegistruImobilizari>(b => {
                b.HasOne(r => r.Imobilizare).WithMany().HasForeignKey(r => r.ImobilizareId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(r => r.Repartitor).WithMany().HasForeignKey(r => r.RepartitorId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(r => r.Document).WithMany().HasForeignKey(r => r.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(r => r.Detaliu).WithMany().HasForeignKey(r => r.DetaliuId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<PunereInFunctiuneDetaliu>(b => {
                b.HasOne(d => d.Imobilizare).WithMany().HasForeignKey(d => d.ImobilizareId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(d => d.LinieSursa).WithMany().HasForeignKey(d => d.LinieSursaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<IesireImobilizareDetaliu>()
                .HasOne(d => d.Imobilizare).WithMany().HasForeignKey(d => d.ImobilizareId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<AmortizareLunaraDetaliu>()
                .HasOne(d => d.Imobilizare).WithMany().HasForeignKey(d => d.ImobilizareId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<PoliticaAmortizare>(b => {
                b.HasOne(p => p.TipMaterial).WithMany().HasForeignKey(p => p.TipMaterialId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.ContAmortizare).WithMany().HasForeignKey(p => p.ContAmortizareId)
                    .OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.ContCheltuialaAmortizare).WithMany()
                    .HasForeignKey(p => p.ContCheltuialaAmortizareId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(p => p.ContCheltuialaCedare).WithMany()
                    .HasForeignKey(p => p.ContCheltuialaCedareId).OnDelete(DeleteBehavior.Restrict);
            });

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

            // D15-D1: `Cod` („RO-CJ") e cheia de idempotență a seed-ului și
            // identitatea de raportare (SAF-T `Region`) — unic. Filtrat pe
            // `GCRecord = 0` ca toate unicitățile pe tipuri cu ștergere amânată
            // (60a): rândul șters rămâne fizic în tabelă, iar un index NEfiltrat
            // ar bloca pentru totdeauna re-seed-ul aceluiași județ.
            modelBuilder.Entity<Judet>().HasIndex(j => j.Cod).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            // Județul unui partener nu se poate șterge din nomenclator cât timp
            // e referit; convenția globală SetNull ar fi golit tăcut adresa —
            // exact genul de pierdere pe care SAF-T o descoperă la validare.
            modelBuilder.Entity<Partener>()
                .HasOne(p => p.Judet).WithMany().HasForeignKey(p => p.JudetId)
                .OnDelete(DeleteBehavior.Restrict);

            // D16-D2: `Cod` („H87") e cheia de idempotență a seed-ului și
            // identitatea de raportare (`UOMStandard`) — unic, filtrat pe
            // `GCRecord = 0` din exact motivul lui `Judet.Cod` de mai sus.
            modelBuilder.Entity<UnitateMasura>().HasIndex(u => u.Cod).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            // Unitatea unui produs nu se șterge din nomenclator cât e referită
            // (`[ForbidCRUD]` o face oricum imposibilă din UI); convenția globală
            // SetNull ar fi golit tăcut câmpul, iar produsul ar fi ieșit în fișier
            // cu `H87` „implicit" fără ca nimeni să fi decis asta.
            modelBuilder.Entity<Produs>()
                .HasOne(p => p.UnitateMasura).WithMany().HasForeignKey(p => p.UnitateMasuraId)
                .OnDelete(DeleteBehavior.Restrict);

            // D16-D1: cele două FK-uri ale societății, ambele Restrict din același
            // motiv ca `Partener.Judet` — județul e `AuditFileRegion` din `Header`,
            // iar contul bancar e `BankAccount`; o golire tăcută a oricăruia s-ar
            // descoperi la validarea fișierului, nu la ștergere.
            modelBuilder.Entity<Societate>()
                .HasOne(s => s.Judet).WithMany().HasForeignKey(s => s.JudetId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Societate>()
                .HasOne(s => s.ContBancar).WithMany().HasForeignKey(s => s.ContBancarId)
                .OnDelete(DeleteBehavior.Restrict);

            // D3-D1: `Cod` e cheia de idempotență a seed-ului și identitatea de
            // raportare a rândului („12.1") — unic. Cele două self-FK-uri sunt
            // Restrict (ca `DocumentTrezorerie.LaturaPereche` mai sus): un rând nu se
            // șterge cât timp altul îl declară părinte sau sursă de oglindă, iar
            // convenția globală SetNull ar fi golit tăcut structura formularului.
            // `WithMany()` fără colecție: sensurile inverse („copiii", „oglinzile
            // mele") se derivă prin query în proiecție, nu se materializează.
            // FILTRAT pe `GCRecord = 0` (fix F5 al review-ului advers), ca toate
            // unicitățile pe tipuri cu ȘTERGERE AMÂNATĂ (60a): rândul șters din
            // UI rămâne fizic în tabelă, deci un index unic NEfiltrat l-ar face
            // să blocheze pentru totdeauna recrearea aceleiași chei — o ștergere
            // reversibilă prin construcție ar fi devenit definitivă printr-un
            // efect colateral al schemei, cu un `23505` brut în loc de un mesaj.
            modelBuilder.Entity<RandD300>().HasIndex(r => r.Cod).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            modelBuilder.Entity<RandD300>()
                .HasOne(r => r.Parinte).WithMany().HasForeignKey(r => r.ParinteId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<RandD300>()
                .HasOne(r => r.OglindaA).WithMany().HasForeignKey(r => r.OglindaAId)
                .OnDelete(DeleteBehavior.Restrict);
            // D3-D2: tripleta e identitatea mapării — aceeași pereche poate cădea pe
            // mai multe rânduri (TI19 pe achiziție: 16 ȘI 33), dar de două ori pe
            // ACELAȘI rând ar dubla cifra la proiecție.
            //
            // Filtrat pe `GCRecord = 0` din același motiv ca `RandD300` de mai sus,
            // dar aici cazul e cel REAL: maparea e politică editabilă (decizia 4),
            // deci ștergerea ei din XAF e un flux normal, iar reintroducerea aceleiași
            // triplete după o ștergere e chiar remediul unei greșeli de culegere.
            modelBuilder.Entity<MapareD300>()
                .HasIndex(m => new { m.TipTvaId, m.Sens, m.RandId }).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            // D4-D2: spre deosebire de D300, aici PERECHEA e identitatea — un grup
            // de registru are un singur tip de operațiune în 394. Filtrat pe
            // `GCRecord = 0` din același motiv (ștergerea logică e flux normal).
            modelBuilder.Entity<MapareD394>()
                .HasIndex(m => new { m.TipTvaId, m.Sens }).IsUnique()
                .HasFilter("\"GCRecord\" = 0");

            // D17-D1: tripleta `(TipDocument, TipStoc, Semn)` e identitatea
            // politicii de mișcare SAF-T — un rând de registru se potrivește pe
            // exact una, altfel codul de mișcare ar depinde de ordinea rândurilor.
            // Filtrat pe `GCRecord = 0` din același motiv ca la D300/D394:
            // politica e date editabile (decizia 4), deci ștergerea e flux normal,
            // iar reintroducerea aceleiași chei e chiar remediul unei greșeli.
            modelBuilder.Entity<PoliticaMiscareSaft>()
                .HasIndex(p => new { p.TipDocumentId, p.TipStoc, p.Semn }).IsUnique()
                .HasFilter("\"GCRecord\" = 0");
            // A DOUA jumătate a unicității, cerută de semantica lui NULL în SQL:
            // în Postgres `NULL <> NULL`, deci indexul de mai sus NU oprește două
            // rânduri „orice semn" pe aceeași pereche (tip, TipStoc) — exact
            // dublura care ar face potrivirea nedeterministă. Indexul parțial de
            // aici o prinde: unic pe pereche, PRINTRE rândurile cu `Semn` null.
            modelBuilder.Entity<PoliticaMiscareSaft>()
                .HasIndex(p => new { p.TipDocumentId, p.TipStoc }).IsUnique()
                .HasFilter("\"Semn\" IS NULL AND \"GCRecord\" = 0");

            AplicaUnicitatiPolitici(modelBuilder);

            // F23-D3 — `Activ` e jumătatea de MIGRAȚIE a tipului viu: coloana se
            // adaugă cu `DEFAULT true`, deci rândurile EXISTENTE rămân vii (un
            // nomenclator care s-ar stinge în întregime la un `database update`
            // ar goli tăcut toate lookup-urile de culegere). Perechea ei e
            // inițializatorul `= true` de pe proprietate, pentru rândurile noi.
            modelBuilder.Entity<TipTva>().Property(t => t.Activ).HasDefaultValue(true);

            AplicaScaraNumerica(modelBuilder);
            AplicaColoanaCautare(modelBuilder);
            AplicaFunctiaFaraDiacritice(modelBuilder);
        }

        // UNICITATEA POLITICILOR ȘI A CODURILOR DE NOMENCLATOR (felia 23,
        // F23-D3). Până acum, nouă politici per tip de document și cinci coduri
        // de nomenclator erau chei DOAR prin convenție: seed-ul le trata ca
        // atare, motorul le citea cu `FirstOrDefault`, iar comentariul din
        // `Cautare` afirma deja că indexurile există. Un al doilea rând pe
        // aceeași cheie — creabil din XAF sau, de la felia asta, prin OData —
        // făcea motorul NEDETERMINIST și TĂCUT: nu pică nimic, doar postează
        // uneori altfel.
        //
        // Trei reguli comune, toate deja precedent în fișier:
        //   * FILTRAT pe `"GCRecord" = 0` (60a): rândul șters logic rămâne fizic
        //     în tabelă, iar un index nefiltrat i-ar bloca definitiv recrearea —
        //     tocmai remediul unei greșeli de culegere;
        //   * `NULLS NOT DISTINCT` (`AreNullsDistinct(false)`) unde cheia are
        //     coloane nullable. În Postgres `NULL <> NULL`, deci fără asta două
        //     rânduri „orice clasă / orice semn / regulă generică" pe aceeași
        //     cheie ar trece nestingherite — exact dublura pe care indexul
        //     există s-o oprească. Perechea de indexuri parțiale din 74a
        //     (`PoliticaMiscareSaft`) a fost forma de dinaintea lui Postgres 15;
        //     rămâne acolo, nu se rescrie într-o felie care nu e a ei.
        //   * NIMIC nu se maschează: cheile de mai jos au fost măsurate pe cele
        //     șase baze de dev înaintea migrației (0 grupuri duble). Un dublu
        //     real ar fi fost regulă de oprire, nu un `DISTINCT ON` în migrație
        //     (77k).
        //
        // Ce NU intră, declarat: `Repartitor.Cod`. Spațiul de coduri e PARTAJAT
        // pe TPT între parteneri, gestiuni, angajați și conturi proprii, iar
        // bazele de import au coliziuni legitime între familii — restanță cu
        // nume, nu o unicitate impusă pe tăcute.
        private static void AplicaUnicitatiPolitici(ModelBuilder modelBuilder) {
            const string viu = "\"GCRecord\" = 0";

            // (1) Politicile cu UN rând per tip de document — cheia e FK-ul,
            // fără nullable, deci indexul simplu ajunge.
            modelBuilder.Entity<PoliticaTva>()
                .HasIndex(p => p.TipDocumentId).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaConex>()
                .HasIndex(p => p.TipDocumentSursaId).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaScadenta>()
                .HasIndex(p => p.TipDocumentId).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaValidare>()
                .HasIndex(p => p.TipDocumentId).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaNumerotare>()
                .HasIndex(p => p.TipDocumentId).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaInchidereTva>()
                .HasIndex(p => p.TipDocumentId).IsUnique().HasFilter(viu);

            // (2) Regulile de alimentare — cheia lor e cheia de POTRIVIRE a
            // motorului, cu nullable-uri pe trepte (`ClasaId`, `TipMaterialId`,
            // `NaturaFiltru`, `SemnFiltru`). Aici `NULLS NOT DISTINCT` e chiar
            // conținutul regulii: două reguli generice pe același tip ar fi
            // ales-o pe prima întoarsă de bază.
            modelBuilder.Entity<RegulaStoc>()
                .HasIndex(r => new { r.TipDocumentId, r.Latura, r.ClasaId }).IsUnique()
                .AreNullsDistinct(false).HasFilter(viu);
            modelBuilder.Entity<RegulaContare>()
                .HasIndex(r => new { r.TipDocumentId, r.TipMaterialId, r.NaturaFiltru, r.SemnFiltru })
                .IsUnique().AreNullsDistinct(false).HasFilter(viu);

            // (3) Implicitul de TVA (F23-D2): două dintre cele trei coloane ale
            // cheii sunt nullable („orice clasă", „dintotdeauna").
            modelBuilder.Entity<PoliticaTvaImplicit>()
                .HasIndex(p => new { p.TipDocumentId, p.ClasaFiscala, p.ValabilDeLa }).IsUnique()
                .AreNullsDistinct(false).HasFilter(viu);

            // (4) Codurile de nomenclator pe care seed-ul le folosește DEJA ca
            // chei de idempotență (`os.FirstOrDefault<T>(x => x.Cod == …)`).
            // `TipDocument` are două: `Cod` e ancora de politică, `ClrType` e
            // ancora de motor (`GasesteTipDocument`) — un al doilea rând pe
            // oricare dintre ele ar rupe rezoluția de tip.
            modelBuilder.Entity<TipDocument>()
                .HasIndex(t => t.Cod).IsUnique().HasFilter(viu);
            modelBuilder.Entity<TipDocument>()
                .HasIndex(t => t.ClrType).IsUnique().HasFilter(viu);
            modelBuilder.Entity<TipTva>()
                .HasIndex(t => t.Cod).IsUnique().HasFilter(viu);
            modelBuilder.Entity<Cont>()
                .HasIndex(c => c.Simbol).IsUnique().HasFilter(viu);
            modelBuilder.Entity<ClasaProdus>()
                .HasIndex(c => c.Cod).IsUnique().HasFilter(viu);
            modelBuilder.Entity<TipMaterial>()
                .HasIndex(t => t.Cod).IsUnique().HasFilter(viu);

            // (5) Identitatea fișei, cheia de seed a catalogului, un rând de politică per tip (F26-D4).
            modelBuilder.Entity<Imobilizare>()
                .HasIndex(f => f.NumarInventar).IsUnique().HasFilter(viu);
            modelBuilder.Entity<ClasificareImobilizari>()
                .HasIndex(c => c.Cod).IsUnique().HasFilter(viu);
            modelBuilder.Entity<PoliticaAmortizare>()
                .HasIndex(p => p.TipMaterialId).IsUnique().HasFilter(viu);
        }

        // Căutarea fără diacritice pe PROIECȚII (decizia 78): `Cautare.
        // FaraDiacritice` devine funcție de query, tradusă pe EXACT fragmentul
        // SQL al coloanei generate (`Cautare.FragmentSql` — o singură
        // ortografie a lui `translate(lower(…))`). Consumatorul e
        // `CautareFiltru` (rescrierea predicatelor de string ale
        // `DataSourceLoader`); pe sursele materializate în memorie rulează
        // corpul C# — LINQ-to-Objects nu se uită la traducere.
        //
        // De ce nu `unaccent()` și nu colație: aceleași motive ca la coloana
        // generată (antetul din `Cautare.cs`) — al doilea normalizator ar putea
        // divergea de tabelul De/La, iar proba SQL == C# n-ar mai acoperi tot.
        private static void AplicaFunctiaFaraDiacritice(ModelBuilder modelBuilder) {
            // `text` e tipul real al coloanelor de string pe Postgres; literalii
            // De/La ies prin generatorul standard de literal (apostrof dublat),
            // identic cu felul în care migrația scrie expresia coloanei.
            var text = new StringTypeMapping("text", System.Data.DbType.String);
            modelBuilder
                .HasDbFunction(typeof(Cautare).GetMethod(nameof(Cautare.FaraDiacritice), [typeof(string)])!)
                .HasTranslation(argumente => new SqlFunctionExpression("translate",
                    [
                        new SqlFunctionExpression("lower", [argumente[0]],
                            nullable: true, argumentsPropagateNullability: [true], typeof(string), text),
                        new SqlConstantExpression(Cautare.De, text),
                        new SqlConstantExpression(Cautare.La, text),
                    ],
                    nullable: true, argumentsPropagateNullability: [true, false, false], typeof(string), text));
        }

        // Căutarea fără diacritice (felia 20, F20-D1): coloană GENERATĂ STORED
        // pe fiecare nomenclator care declară `ICuCautare`. Configurare
        // GENERICĂ — o buclă, nu 14 blocuri copiate: prezența interfeței e
        // toată declarația, iar SQL-ul vine dintr-o singură sursă (`Cautare.
        // ExpresieSql`), aceeași care ajunge în migrație și în oracolul din
        // ModelCheck.
        //
        // Trei reguli, toate deduse din model (nicio listă de tipuri aici):
        //   * coloana aparține ENTITĂȚII EF care declară proprietatea, nu clasei
        //     CLR: sub TPT o singură coloană pe `Repartitor` acoperă Partener/
        //     Gestiune/Angajat/UnitateInterna/ContPropriu; o bază CLR nemapată
        //     (`Dimensiune`) lasă coloana pe fiecare derivată.
        //   * numele coloanei de cod se citește din entitate: `Cod`, altfel
        //     `Simbol` (planul de conturi), altfel doar denumirea.
        //   * `Denumire` e obligatorie — un nomenclator care ar declara
        //     interfața fără ea ar produce o coloană tăcut inutilă, deci e
        //     eroare la construirea modelului (ca gardianul scării).
        //
        // 77-r2 — pe ACEEAȘI declarație stă și obligativitatea: un nomenclator
        // care se caută după (cod, denumire) le are pe amândouă. NOT NULL +
        // CHECK `btrim(...) <> ''` (NOT NULL singur lasă să treacă `''` și
        // `'   '`) pe coloana de cod și pe `Denumire`, în tabelul care le
        // DECLARĂ (sub TPT: `Repartitori`, o dată pentru toate frunzele). Ușa
        // de sistem (Import1C, seed, motor) e apărată aici, de schemă; ușa
        // secured primește mesajul de domeniu din `GardianEditare` înaintea
        // bazei, iar violarea de constraint — dacă totuși ajunge — iese tot
        // 422, tradusă (39a/60a, `CheckTemplate` cu numele regulii).
        private static void AplicaColoanaCautare(ModelBuilder modelBuilder) {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()) {
                var clr = entityType.ClrType;
                if (clr == null || !typeof(ICuCautare).IsAssignableFrom(clr))
                    continue;
                if (entityType.FindProperty(Cautare.NumeColoana)?.DeclaringType != entityType)
                    continue;
                if (clr.GetProperty(Cautare.NumeDenumire) == null)
                    throw new InvalidOperationException(
                        $"{clr.Name} declară ICuCautare dar n-are `Denumire` — coloana generată " +
                        "n-ar avea ce normaliza.");
                var coloanaCod = Cautare.NumeCod(clr);
                var entitate = modelBuilder.Entity(clr);
                entitate.Property(Cautare.NumeColoana)
                    .HasComputedColumnSql(Cautare.ExpresieSql(coloanaCod, Cautare.NumeDenumire), stored: true)
                    .ValueGeneratedOnAddOrUpdate();

                var tabel = entityType.GetTableName() ?? clr.Name;
                foreach (var coloana in new[] { coloanaCod, Cautare.NumeDenumire }) {
                    if (coloana == null)
                        continue;
                    entitate.Property(coloana).IsRequired();
                    entitate.ToTable(t => t.HasCheckConstraint(
                        Cautare.NumeRegulaNeGol(tabel, coloana), Cautare.SqlNeGol(coloana)));
                }
            }
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
