# Checklist Detaliat de Implementare - Migrare Delphi → XAF

## FAZA 1: PREGĂTIRE ȘI SETUP (2-3 săptămâni)

### Săptămâna 1: Environment Setup

#### Setup Development Environment
- [ ] Instalare Visual Studio 2022 (sau mai recent)
- [ ] Instalare .NET 8 SDK
- [ ] Instalare DevExpress Universal 24.2+ (cu licență activă)
- [ ] Instalare SQL Server Management Studio
- [ ] Setup Git repository pentru proiect nou

#### Creare Proiect XAF
- [ ] Creare soluție nouă XAF:
  ```bash
  dotnet new xaf --name Contabilitate.XAF --framework net8.0
  ```
- [ ] Verificare structură proiect generată:
  - [ ] Contabilitate.XAF.Module (platform-independent)
  - [ ] Contabilitate.XAF.Blazor.Server (aplicație web)
  - [ ] Contabilitate.XAF.Win (optional - WinForms)

#### Configurare Connection String
- [ ] Update `appsettings.json` (Blazor):
  ```json
  {
    "ConnectionStrings": {
      "ConnectionString": "Server=10.20.0.218;Database=Contabilitate_XAF_2025;User Id=admin;Password=***;TrustServerCertificate=True;"
    }
  }
  ```
- [ ] Update `App.config` (WinForms dacă există)

#### Instalare Pachete NuGet Necesare
- [ ] DevExpress.ExpressApp (24.2.*)
- [ ] DevExpress.ExpressApp.EFCore (24.2.*)
- [ ] DevExpress.ExpressApp.Blazor (24.2.*)
- [ ] DevExpress.ExpressApp.Security.EFCore (24.2.*)
- [ ] DevExpress.ExpressApp.Validation.Blazor (24.2.*)
- [ ] DevExpress.ExpressApp.ReportsV2.Blazor (24.2.*)
- [ ] Microsoft.EntityFrameworkCore.SqlServer (8.0.*)
- [ ] Microsoft.EntityFrameworkCore.Tools (8.0.*)

### Săptămâna 2: Database Setup

#### Reverse Engineering Baza de Date
- [ ] Backup bază de date existentă (`contabilitate_Sotanga_2025`)
- [ ] Creare bază de date nouă pentru XAF (`Contabilitate_XAF_2025`)
- [ ] Reverse engineering tabele principale (CPLAN, CNOTE, REPARTITORI):
  ```bash
  dotnet ef dbcontext scaffold "Server=..." Microsoft.EntityFrameworkCore.SqlServer \
      --output-dir BusinessObjects/Generated \
      --tables CPLAN,CNOTE,CJURNALE,REPARTITORI \
      --context ContabilitateDbContext \
      --data-annotations
  ```
- [ ] Review cod generat
- [ ] Ajustare clase generate pentru a moșteni `BaseObject`

#### Configurare EF Core DbContext
- [ ] Creare `ContabilitateDbContext.cs`
- [ ] Configurare `OnModelCreating` cu mapări la tabele existente
- [ ] Creare fișier Migrations inițial:
  ```bash
  dotnet ef migrations add InitialCreate
  ```
- [ ] Verificare migration generat (nu aplica încă!)

### Săptămâna 3: Proof of Concept

#### POC: Modul Contabilitate Simplificat
- [ ] Creare entitate `ChartOfAccounts` (mapată la CPLAN)
- [ ] Creare entitate `AccountingEntry` simplificată (mapată la CNOTE)
- [ ] Test CRUD operations în XAF:
  - [ ] Creare cont nou
  - [ ] Modificare cont existent
  - [ ] Vizualizare listă conturi
  - [ ] Ștergere cont (soft delete)

#### Test UI Generat Automat
- [ ] Rulare aplicație Blazor
- [ ] Verificare List View pentru ChartOfAccounts
- [ ] Verificare Detail View pentru ChartOfAccounts
- [ ] Test filtrare și sortare în grid

#### Review & Ajustări
- [ ] Review cu echipa (dev + stakeholders)
- [ ] Colectare feedback
- [ ] Ajustare plan în funcție de feedback
- [ ] Decizie GO / NO-GO pentru continuare

---

## FAZA 2: MIGRARE ENTITĂȚI CORE (4-6 săptămâni)

### Săptămâna 4-5: Modul Contabilitate ⭐⭐⭐

#### Business Objects
- [ ] **ChartOfAccounts** (CPLAN):
  - [ ] Toate proprietățile mapate
  - [ ] Validări: AccountCode unique, required
  - [ ] Computed properties (BalanceAmount, etc.)
  - [ ] Business logic în OnSaving pentru propagare proprietăți de la Parent

- [ ] **AccountingEntry** (CNOTE):
  - [ ] Toate proprietățile mapate (70+ coloane!)
  - [ ] Associations cu ChartOfAccounts (Debit/Credit)
  - [ ] Associations cu CostCenter
  - [ ] Validări complexe (contabil balansat, conturi existente)
  - [ ] Status workflow (Draft → Posted → Deleted)

- [ ] **Journal** (CJURNALE):
  - [ ] Proprietăți de bază
  - [ ] Flag IsClosingJournal

- [ ] **CostCenter** (REPARTITORI):
  - [ ] Toate proprietățile (50+ coloane!)
  - [ ] Ierarhie (Parent-Children)
  - [ ] Validări (FiscalCode format, etc.)

#### Controllers & Actions
- [ ] **PostAccountingEntryController**:
  - [ ] SimpleAction "Contabilizează"
  - [ ] Validare înainte de postare
  - [ ] Update status la Posted
  - [ ] Mesaj confirmare

- [ ] **UnpostAccountingEntryController**:
  - [ ] SimpleAction "Anulează Contabilizare"
  - [ ] Verificare permisiuni
  - [ ] Revert status la Draft

- [ ] **AccountStatementController**:
  - [ ] PopupWindowShowAction "Fișă Cont"
  - [ ] Parametri: data_start, data_end
  - [ ] Integrare AccountStatementService

#### Services
- [ ] **AccountStatementService**:
  - [ ] Implementare migrare `sp_get_fisa_cont_new`
  - [ ] Metode:
    - `GetAccountStatement(accountCode, startDate, endDate, criteria)`
    - `CalculateInitialBalance(...)`
    - `GetAccountingEntries(...)`
    - `CalculateRunningBalance(...)`
  - [ ] Unit tests pentru fiecare metodă

#### Application Model Customization
- [ ] Layout pentru `ChartOfAccounts_DetailView`
- [ ] Layout pentru `AccountingEntry_DetailView` (tabs: General, Accounts, Budget)
- [ ] Column configuration pentru `ChartOfAccounts_ListView` (TreeListEditor)
- [ ] Column configuration pentru `AccountingEntry_ListView`
- [ ] Filters predefinite (Draft, Posted, Current Month)

#### Testing
- [ ] Test creare cont nou
- [ ] Test creare notă contabilă simplă (D = C)
- [ ] Test contabilizare notă
- [ ] Test generare fișă cont (comparație cu Delphi!)
- [ ] Test calcul sold
- [ ] Performance test liste mari (10k+ înregistrări)

### Săptămâna 6-7: Modul Buget ⭐⭐⭐

#### Business Objects
- [ ] **BudgetVersion** (BG_VERSIUNE)
- [ ] **BudgetAllocation** (BG_DESCHIDERE)
- [ ] **Commitment** (ALOP_ANGAJAMENTE)
- [ ] **CommitmentDecomposition** (ALOP_ANGAJAMENTE_DEFALCARE)
- [ ] **Liquidation** (ALOP_LICHIDARE)
- [ ] **LiquidationDecomposition** (ALOP_LICHIDARE_DEFALCARE)
- [ ] **PaymentOrder** (ALOP_ORDONANTARE)

#### Workflow ALOP
- [ ] **CommitmentController**:
  - [ ] Action "Validează Angajament"
  - [ ] Verificare disponibil bugetar (BudgetAvailabilityCalculator)
  - [ ] Generare notă contabilă (8066 / 8068)

- [ ] **LiquidationController**:
  - [ ] Action "Validează Lichidare"
  - [ ] Verificare referință la Commitment
  - [ ] Generare notă contabilă (401 / 8066)

- [ ] **PaymentOrderController**:
  - [ ] Action "Validează Ordonanțare"
  - [ ] Trigger plată în Casa/Banca

#### Services
- [ ] **BudgetAvailabilityCalculator**:
  - [ ] `CalculateAvailable(allocation)` → decimal
  - [ ] `HasSufficientBudget(commitment)` → bool
  - [ ] `GetCommittedAmount(allocation)` → decimal

- [ ] **BudgetAccountingService**:
  - [ ] `GenerateCommitmentEntry(commitment)`
  - [ ] `GenerateLiquidationEntry(liquidation)`
  - [ ] `GeneratePaymentOrderEntry(paymentOrder)`

#### Application Model
- [ ] Layout BudgetVersion (tabs: General, Allocations)
- [ ] Layout Commitment (tabs: General, Decomposition, Liquidations)
- [ ] Filters pentru BudgetAllocation (by FunctionalCode, EconomicCode)

#### Testing
- [ ] Test creare versiune buget
- [ ] Test alocare bugete pe clase bugetare
- [ ] Test creare angajament (cu verificare disponibil)
- [ ] Test creare lichidare (legată de angajament)
- [ ] Test workflow complet: Angajament → Lichidare → Ordonanțare
- [ ] Test generare note contabile automate
- [ ] Comparație calcul disponibil cu Delphi

### Săptămâna 8-9: Modul Casa/Banca ⭐⭐

#### Business Objects
- [ ] **BankAccount** (LISTA_CONTURI)
- [ ] **CashDocument** (BREGISTRU / CB_REGISTRU)
- [ ] **BankStatement** (extrase bancare importate)

#### Controllers & Actions
- [ ] **PaymentController**:
  - [ ] Action "Înregistrează Plată"
  - [ ] Generare notă contabilă (401 / 5121 sau 5311)

- [ ] **ReceiptController**:
  - [ ] Action "Înregistrează Încasare"
  - [ ] Generare notă contabilă (5121 / 411 sau 701)

- [ ] **BankStatementImportController**:
  - [ ] Action "Import Extras Bancar" (Excel/CSV)
  - [ ] Mapare automată pe note contabile existente (reconciliere)

#### Services
- [ ] **TreasuryAccountingService**:
  - [ ] `GeneratePaymentEntry(payment, order)`
  - [ ] `GenerateReceiptEntry(receipt)`

#### Testing
- [ ] Test creare cont bancar
- [ ] Test înregistrare plată
- [ ] Test înregistrare încasare
- [ ] Test import extras bancar
- [ ] Test reconciliere

### Săptămâna 10: Modul Gestiune ⭐⭐

#### Business Objects
- [ ] **Product** (GEST_GNMCL)
- [ ] **InventoryDocument** (GEST_DOCUM)
- [ ] **InventoryDocumentItem** (GEST_ITEMSI)
- [ ] **InventoryDocumentType** (GEST_TIP_DOCUM)

#### Controllers
- [ ] **InventoryDocumentController**:
  - [ ] Action "Validează Document"
  - [ ] Generare note contabile (NIR: 301/401, Consumuri: 6xx/301)

#### Services
- [ ] **InventoryAccountingService**:
  - [ ] `GenerateReceiptEntry(document)` - NIR
  - [ ] `GenerateIssueEntry(document)` - Bon Consum
  - [ ] `GenerateInvoiceEntry(document)` - Factură

- [ ] **InventoryValuationService**:
  - [ ] Calcul cost mediu ponderat (CMP)
  - [ ] FIFO / LIFO (optional)

#### Testing
- [ ] Test creare produs
- [ ] Test creare NIR
- [ ] Test calcul stoc
- [ ] Test generare note contabile pentru NIR
- [ ] Test generare note pentru consumuri

---

## FAZA 3: MIGRARE LOGICĂ BUSINESS & VALIDĂRI (3-4 săptămâni)

### Săptămâna 11-12: Controllers & Business Logic

#### Identificare Event Handlers Delphi
- [ ] Lista completă event handlers din Delphi:
  - [ ] AfterPost → OnSaved
  - [ ] BeforePost → OnSaving
  - [ ] OnNewRecord → ObjectSpace.ObjectCreated
  - [ ] OnValidate → RuleFromBoolProperty
  - [ ] OnCalcFields → Computed properties

#### Transcriere în XAF Controllers
- [ ] Pentru fiecare event handler Delphi major:
  - [ ] Identificare logică
  - [ ] Creare Controller XAF corespunzător
  - [ ] Implementare logică în C#
  - [ ] Unit testing

#### Exemple de Transcriere
- [ ] `TfrmNote.AfterPost` → `AccountingEntryController.ObjectSpace_ObjectSaved`
- [ ] `TfrmBalanta.OnCalcFields` → Computed properties în `ChartOfAccounts`
- [ ] `TfrmPlanConturi.OnValidate` → `[RuleFromBoolProperty]` în `ChartOfAccounts`

### Săptămâna 13: Validări Complexe

#### Business Rules
- [ ] **ChartOfAccounts**:
  - [ ] AccountCode unique și required
  - [ ] Sold inițial pe o singură parte (debit SAU credit)
  - [ ] Cont sintetic nu poate avea sold diferit de suma copiilor

- [ ] **AccountingEntry**:
  - [ ] Debit și Credit required
  - [ ] Conturi trebuie să existe în CPLAN
  - [ ] Suma > 0
  - [ ] Balanță echilibrată pentru note compuse
  - [ ] Journalul trebuie să existe

- [ ] **Commitment**:
  - [ ] Disponibil bugetar suficient
  - [ ] Beneficiar required
  - [ ] Sumă > 0
  - [ ] Contract obligatoriu pentru angajamente mari (ex: > 10,000 lei)

#### Implementare Validări
- [ ] Folosire `[RuleRequiredField]`
- [ ] Folosire `[RuleUniqueValue]`
- [ ] Folosire `[RuleFromBoolProperty]` pentru reguli complexe
- [ ] Folosire `[RuleRange]` pentru limite numerice
- [ ] Custom validări în Controllers

### Săptămâna 14: Actions Avansate

#### Implementare Actions Complexe
- [ ] **Imperechere Note** (matching invoices):
  - [ ] PopupWindowShowAction pentru selectare note
  - [ ] Calcul diferențe
  - [ ] Creare note de ajustare

- [ ] **Defalcare Note** (split entries):
  - [ ] PopupWindowShowAction pentru criterii defalcare
  - [ ] Generare note defalcate pe functional/economic

- [ ] **Inchidere Perioada**:
  - [ ] Verificare toate notele sunt contabilizate
  - [ ] Blocare perioada pentru modificări
  - [ ] Generare note închidere (balanță)

---

## FAZA 4: MIGRARE RAPOARTE (2-3 săptămâni)

### Săptămâna 15-16: DevExpress Reports

#### Setup Reports Module
- [ ] Activare ReportsV2 Module în XAF
- [ ] Creare folder `Reports` în proiect

#### Rapoarte Prioritare
- [ ] **Fișă Cont**:
  - [ ] Creare report template (.repx)
  - [ ] Data source: AccountStatementService
  - [ ] Layout: similar cu Delphi
  - [ ] Parametri: accountCode, startDate, endDate
  - [ ] Export: PDF, Excel

- [ ] **Balanță de Verificare**:
  - [ ] Creare report
  - [ ] Data source: ChartOfAccounts cu sume calculate
  - [ ] Grupare pe clase conturi
  - [ ] Subtotaluri și total general

- [ ] **Registru Jurnal**:
  - [ ] Lista note contabile
  - [ ] Filtrare pe jurnal și perioadă
  - [ ] Total debit = total credit

- [ ] **Situații Bugetare ALOP**:
  - [ ] Buget alocat vs. executat
  - [ ] Disponibil pe clase bugetare
  - [ ] Detalii angajamente/lichidări/plăți

- [ ] **Registru Casă**:
  - [ ] Listă încasări/plăți
  - [ ] Sold zilnic
  - [ ] Semnaturi

#### Report Actions în XAF
- [ ] Controller pentru fiecare raport:
  - [ ] SimpleAction sau PopupWindowShowAction
  - [ ] Parametrizare (dacă e cazul)
  - [ ] Afișare preview
  - [ ] Export direct (PDF/Excel)

#### Testing Rapoarte
- [ ] Test fiecare raport cu date reale
- [ ] Comparație output cu rapoarte Delphi (side-by-side)
- [ ] Verificare calcule (totaluri, subtotaluri)
- [ ] Test export PDF și Excel

### Săptămâna 17: Dashboards (Optional)

#### Dashboard Views
- [ ] **Dashboard Director**:
  - [ ] Total venituri/cheltuieli (an curent)
  - [ ] Execuție bugetară (%)
  - [ ] Grafice: venituri pe luni, cheltuieli pe categorii
  - [ ] Top 10 furnizori

- [ ] **Dashboard Contabil**:
  - [ ] Note necontabilizate (count)
  - [ ] Note eronate (cu erori de balanță)
  - [ ] Link rapid către Fișă Cont pentru conturi cu sold mare

---

## FAZA 5: MIGRARE UI & CUSTOMIZĂRI (3-4 săptămâni)

### Săptămâna 18-19: Layout & Appearance

#### Detail Views Layout
- [ ] **AccountingEntry_DetailView**:
  - [ ] Tab "Date Generale": EntryNumber, EntryDate, Journal, Description
  - [ ] Tab "Conturi": DebitAccount, CreditAccount, Amount
  - [ ] Tab "Clasificare": FunctionalCode, EconomicCode, OrgUnit, Project
  - [ ] Tab "Document Sursă": DocumentType, DocumentNumber, DocumentDate

- [ ] **ChartOfAccounts_DetailView**:
  - [ ] Grupare logică: Identificare, Solduri, Clasificare

- [ ] **Commitment_DetailView**:
  - [ ] Tab "General": CommitmentNumber, Date, Beneficiary
  - [ ] Tab "Clasificare": FunctionalCode, EconomicCode
  - [ ] Tab "Defalcare": Grid cu CommitmentDecompositions
  - [ ] Tab "Lichidări": Grid cu Liquidations

#### List Views Customization
- [ ] **ChartOfAccounts_ListView_Tree**:
  - [ ] TreeListEditor (ierarhic)
  - [ ] Coloane: AccountCode, Description, InitialBalance, CurrentBalance
  - [ ] Expandare automată nivel 1
  - [ ] Font bold pentru conturi sumatoare

- [ ] **AccountingEntry_ListView**:
  - [ ] GridListEditor
  - [ ] Coloane: EntryNumber, EntryDate, Journal, DebitAccount, CreditAccount, Amount
  - [ ] Conditional formatting: Draft (gray), Posted (black), Error (red)
  - [ ] Filters: Draft, Posted, Current Month

#### Conditional Appearance
- [ ] Conturi sumatoare → font bold, background gray
- [ ] Note draft → text gray
- [ ] Note cu erori → text red
- [ ] Disponibil bugetar negativ → background red

### Săptămâna 20: Navigation & Menu

#### Navigation Items
- [ ] **Contabilitate**:
  - [ ] Plan de Conturi
  - [ ] Note Contabile
  - [ ] Jurnale
  - [ ] Parteneri/Centre de Cost

- [ ] **Buget**:
  - [ ] Versiuni Buget
  - [ ] Alocări Bugetare
  - [ ] Angajamente
  - [ ] Lichidări
  - [ ] Ordonanțări

- [ ] **Gestiune**:
  - [ ] Produse
  - [ ] Documente Gestiune
  - [ ] Stocuri

- [ ] **Casa/Banca**:
  - [ ] Conturi Bancare
  - [ ] Registru Casa
  - [ ] Extrase Bancare

- [ ] **Rapoarte**:
  - [ ] Fișă Cont
  - [ ] Balanță
  - [ ] Registru Jurnal
  - [ ] Situații ALOP

#### Application Model
- [ ] Configurare NavigationItems în Model Editor
- [ ] Iconiță pentru fiecare item
- [ ] Grupare logică

### Săptămâna 21: Advanced UI Features

#### Master-Detail Views (Dashboard Views)
- [ ] **Plan Conturi + Note**:
  - [ ] Selectare cont în stânga → note ale contului în dreapta

- [ ] **Angajamente + Lichidări**:
  - [ ] Selectare angajament → lichidări aferente

#### Inline Actions în Grid
- [ ] Buton "Contabilizează" direct în grid note
- [ ] Buton "Validează" în grid angajamente

#### Tooltips & Hints
- [ ] Hint pentru sold cont (when hover)
- [ ] Tooltip pentru notă contabilă (detalii complete)

---

## FAZA 6: SECURITATE & PERMISIUNI (1-2 săptămâni)

### Săptămâna 22: Security System

#### Setup Security
- [ ] Activare SecurityStrategyComplex în Startup
- [ ] Configurare IdentityAuthenticationProvider

#### Definire Roluri
- [ ] **Administrator**:
  - [ ] Acces complet la toate modulele
  - [ ] Poate șterge note contabilizate
  - [ ] Poate anula închiderea perioadei

- [ ] **Contabil**:
  - [ ] Poate crea/modifica note Draft
  - [ ] Poate contabiliza note
  - [ ] NU poate șterge note Posted
  - [ ] Acces Read-Only la Buget

- [ ] **Contabil Șef**:
  - [ ] Toate drepturile Contabil
  - [ ] Poate șterge note Posted (cu justificare)
  - [ ] Poate închide perioada

- [ ] **Vizualizare**:
  - [ ] Read-Only la toate modulele
  - [ ] Poate genera rapoarte

#### Implementare Permisiuni
- [ ] Type permissions pentru fiecare entitate
- [ ] Member permissions pentru proprietăți sensibile (ex: Amount)
- [ ] Object-level permissions (ex: note proprii vs. note altora)

#### Audit Trail
- [ ] Activare AuditTrailModule
- [ ] Configurare audit pentru:
  - [ ] AccountingEntry
  - [ ] ChartOfAccounts
  - [ ] BudgetAllocation
  - [ ] Commitment

### Săptămâna 23: Testing Security

#### Test Scenarii
- [ ] Login ca Administrator → verificare acces complet
- [ ] Login ca Contabil → verificare restricții (nu poate șterge Posted)
- [ ] Login ca Vizualizare → verificare Read-Only
- [ ] Test audit trail (modificare cont → verificare istoric)

---

## FAZA 7: MIGRARE DATE & TESTING (2-3 săptămâni)

### Săptămâna 24: Migrare Date

#### Pregătire
- [ ] Backup complet bază de date existentă
- [ ] Creare script migrare date (dacă e cazul)
- [ ] Testare script pe copie bază de date

#### Strategie Migrare
**Opțiune A: Folosire Directă Bază Existentă**
- [ ] Mapare EF Core la tabele existente (deja făcut în Faza 1)
- [ ] Testare CRUD cu date reale
- [ ] Verificare integritate date

**Opțiune B: Migrare în Bază Nouă** (doar dacă Opțiunea A nu funcționează)
- [ ] Creare bază nouă cu EF Core Migrations
- [ ] Script SQL pentru migrare date:
  - [ ] CPLAN → ChartOfAccounts
  - [ ] CNOTE → AccountingEntry
  - [ ] REPARTITORI → CostCenter
  - [ ] Etc.
- [ ] Rulare script migrare
- [ ] Verificare completitudine (count records)
- [ ] Verificare calcule (solduri, totaluri)

#### Validare Date Migrated
- [ ] Comparație număr înregistrări (Delphi vs. XAF)
- [ ] Verificare solduri conturi (balanță)
- [ ] Verificare sume note contabile
- [ ] Test generare fișă cont → comparație cu Delphi

### Săptămâna 25-26: Testing Extensiv

#### Unit Tests
- [ ] AccountStatementService.CalculateInitialBalance
- [ ] AccountStatementService.GetAccountingEntries
- [ ] BudgetAvailabilityCalculator.CalculateAvailable
- [ ] InventoryValuationService.CalculateCMP

#### Integration Tests
- [ ] Test creare notă contabilă end-to-end
- [ ] Test workflow ALOP complet (Angajament → Plată)
- [ ] Test generare rapoarte cu date reale
- [ ] Test import extras bancar

#### UAT (User Acceptance Testing)
- [ ] Scenarii cu utilizatori finali:
  - [ ] Scenariul 1: Contabil introduce nota de la factura
  - [ ] Scenariul 2: Contabil-șef generează balanța lunii
  - [ ] Scenariul 3: Director buget creează angajament
  - [ ] Scenariul 4: Gestionar validează NIR
- [ ] Colectare feedback
- [ ] Prioritizare ajustări

#### Performance Testing
- [ ] Test încărcare liste mari (10k+ note)
- [ ] Test generare fișă cont pentru cont cu multe mișcări (5k+)
- [ ] Test generare balanță pentru an complet
- [ ] Optimizări query-uri lente (indecși, compiled queries)

#### Comparație Side-by-Side Delphi vs. XAF
- [ ] Fișă cont: aceleași rezultate?
- [ ] Balanță: aceleași solduri?
- [ ] Disponibil bugetar: aceleași calcule?
- [ ] Documentare diferențe (dacă există)

---

## FAZA 8: DEPLOYMENT & GO-LIVE (1-2 săptămâni)

### Săptămâna 27: Pregătire Deployment

#### Server Setup
- [ ] Instalare Windows Server 2019/2022 (sau Linux pentru Blazor)
- [ ] Instalare IIS 10+ (sau Kestrel standalone)
- [ ] Instalare .NET 8 Runtime (ASP.NET Core Runtime)
- [ ] Instalare SQL Server 2019/2022 (dacă e pe server nou)

#### Deployment Blazor Server
- [ ] Publicare aplicație:
  ```bash
  dotnet publish -c Release -o ./publish
  ```
- [ ] Creare Application Pool în IIS:
  - [ ] Name: ContabilitateXAF
  - [ ] .NET CLR Version: No Managed Code
  - [ ] Managed Pipeline Mode: Integrated
  - [ ] Identity: ApplicationPoolIdentity (sau cont dedicat)

- [ ] Creare Website/Application în IIS:
  - [ ] Physical path: calea către `./publish`
  - [ ] Binding: HTTPS (port 443) cu certificat SSL
  - [ ] Application Pool: ContabilitateXAF

- [ ] Configurare `appsettings.Production.json`:
  - [ ] Connection string către SQL Server producție
  - [ ] Logging level: Warning sau Error
  - [ ] Disable DetailedErrors în producție

#### Deployment WinForms (ClickOnce - optional)
- [ ] Configurare ClickOnce publishing în Visual Studio
- [ ] Publish location: `\\server\apps\Contabilitate\`
- [ ] Auto-increment version
- [ ] Test instalare de pe network share

#### Database Setup Producție
- [ ] Restore backup bază date existentă (sau migrare dacă e bază nouă)
- [ ] Rulare EF Core Migrations (dacă sunt modificări):
  ```bash
  dotnet ef database update --connection "Server=...;Database=Contabilitate_XAF_2025;..."
  ```
- [ ] Verificare integritate date
- [ ] Setup backup automat (daily full + hourly differential)

#### Security & Access
- [ ] Configurare Firewall (deschis port 443 pentru Blazor)
- [ ] Configurare SSL certificate (Let's Encrypt sau certificat intern)
- [ ] Creare utilizatori în XAF Security:
  - [ ] Administrator (admin@company.ro)
  - [ ] Utilizatori test pentru fiecare rol
- [ ] Test login de pe mașini client

### Săptămâna 28: Pilot & Go-Live

#### Faza Pilot (2-4 săptămâni)
- [ ] Selectare grup pilot (3-5 utilizatori)
- [ ] Training utilizatori pilot (2 sesiuni × 2 ore):
  - [ ] Sesiunea 1: Overview XAF, navigare, liste, filtre
  - [ ] Sesiunea 2: Workflow specific (note, balanță, rapoarte)

- [ ] Rulare paralelă Delphi + XAF:
  - [ ] Utilizatori introduc date în ambele sisteme
  - [ ] Comparație rezultate zilnic (balanță, fișă cont)
  - [ ] Documentare diferențe

- [ ] Colectare feedback zilnic:
  - [ ] Ce funcționează bine?
  - [ ] Ce lipsește sau e diferit față de Delphi?
  - [ ] Ce bugs sau erori au întâlnit?

- [ ] Ajustări rapide (hotfixes):
  - [ ] Fix bugs critice
  - [ ] Ajustări UI minor (labels, layout)
  - [ ] Adăugare shortcuts/validări cerute de utilizatori

#### Training Complet Utilizatori (1 săptămână înainte de Go-Live)
- [ ] Sesiuni training pentru toți utilizatorii:
  - [ ] Grup 1 (Contabili): 3 sesiuni × 3 ore
  - [ ] Grup 2 (Buget): 2 sesiuni × 2 ore
  - [ ] Grup 3 (Gestiune): 2 sesiuni × 2 ore
  - [ ] Grup 4 (Management): 1 sesiune × 1 oră (overview + rapoarte)

- [ ] Materiale training:
  - [ ] User manual (PDF/Wiki)
  - [ ] Video tutorials scurte (5-10 min/subiect)
  - [ ] Quick reference cards (cheat sheets)

#### Go-Live Day
- [ ] **Sâmbătă / Duminică** (weekend pentru timp de rezolvare probleme)
- [ ] **08:00**: Backup final bază Delphi
- [ ] **08:30**: Stop accesul la aplicația Delphi (read-only)
- [ ] **09:00**: Verificare finală date în XAF (sync dacă e nevoie)
- [ ] **10:00**: Activare aplicație XAF pentru toți utilizatorii
- [ ] **10:00-18:00**: Suport dedicat on-site (toată echipa de dev)
- [ ] **Luni 08:00**: Verificare funcționare normale, rezolvare probleme minore

#### Monitorizare Post Go-Live (2 săptămâni)
- [ ] Monitorizare daily logs (erori, warning-uri)
- [ ] Daily meeting cu utilizatori (15 min standup)
- [ ] Fix bugs critice în max 4 ore
- [ ] Fix bugs non-critice în max 48 ore
- [ ] Colectare feedback continuu

#### Decommissioning Delphi (după 1-2 luni)
- [ ] Backup final aplicație Delphi
- [ ] Arhivare cod sursă Delphi (Git repository)
- [ ] Păstrare aplicație Delphi Read-Only pentru 6 luni (backup/referință)
- [ ] Comunicare oficială: XAF este sistemul principal

---

## CHECKLIST GENERAL - MILESTONES

### Milestone 1: POC Reușit (Săptămâna 3)
- [ ] Aplicație XAF funcțională cu 2 entități (ChartOfAccounts, AccountingEntry)
- [ ] UI generat automat
- [ ] CRUD operations funcționale
- [ ] Decizie GO/NO-GO

### Milestone 2: Module Core Complete (Săptămâna 10)
- [ ] Modul Contabilitate 100% functional
- [ ] Modul Buget 100% functional
- [ ] Modul Casa/Banca 80% functional
- [ ] Modul Gestiune 80% functional

### Milestone 3: Rapoarte + UI (Săptămâna 21)
- [ ] Toate rapoartele prioritare funcționale
- [ ] UI customizat conform cerințelor
- [ ] Navigation configurată

### Milestone 4: Ready for Pilot (Săptămâna 26)
- [ ] Toate testele passed
- [ ] Date migrate și validate
- [ ] Training materials pregătite
- [ ] Server producție configurat

### Milestone 5: Go-Live (Săptămâna 28)
- [ ] Aplicație live în producție
- [ ] Toți utilizatorii au acces
- [ ] Monitoring activ

---

## RESURSE NECESARE

### Echipă
- **2 Developeri Senior .NET** (XAF + EF Core experience)
- **1 DBA** (SQL Server optimization)
- **1-2 QA Testers**
- **1 Business Analyst / PO** (bridge între dev și utilizatori)
- **1 System Administrator** (deployment, server setup)

### Software
- Visual Studio 2022 Professional/Enterprise (licențe)
- DevExpress Universal Subscription (licențe)
- SQL Server 2019/2022 Standard (licență server)

### Hardware (Server Producție)
- **CPU**: 4+ cores
- **RAM**: 16+ GB
- **Disk**: 500+ GB SSD (RAID 1 pentru redundanță)
- **Network**: 1 Gbps

### Timp Total Estimat
**20-28 săptămâni** (5-7 luni) cu echipă full-time

---

Acest checklist acoperă tot procesul de migrare de la setup inițial până la Go-Live. Poate fi folosit ca task tracker în Jira/Azure DevOps.
