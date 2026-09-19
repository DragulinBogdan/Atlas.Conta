# Inventar: rapoartele de TVA (jurnale, decont, D300, D394, rectificativă)

Pasul 1 al designului `docs/nucleu/nucleu-cub-design.md`. Doar INVENTAR: ce
citește implementarea curentă, cu `fișier:linie`. Fără propuneri.

Căile sunt relative la
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/` (prefix `M/`) și
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/` (prefix `W/`).

## Sursa comună: `RegistruTva`

Toate cele cinci rapoarte citesc ACELAȘI registru —
`M/BusinessObjects/Registre/Registre.cs:203-256`:

| coloană | linie | scrisă de motor din |
|---|---|---|
| `Data` | 207 | `doc.Data` (`M/Motor/MotorOperare.cs:379`); pe storno `dataStorno` (`:683`) |
| `PerioadaAn`/`PerioadaLuna` | 212, 214 | `RegistruTvaService.PerioadaDeclarare` (`:380-383`); pe storno luna stornării (`:686-687`) |
| `ScrisLa` | 219 | `DateTime.UtcNow` la operare (`:376,384`) / la storno (`:680,688`) |
| `Sens` | 221 | `PoliticaTva.Directie` (`M/Motor/RegistruTvaService.cs:107`) |
| `DocumentId`/`DetaliuId` | 223, 225 | documentul și linia (`:385-386`) |
| `PartenerId` | 235 | `doc.PredatorId`/`doc.PrimitorId` după `PoliticaTva.SursaContrapartida` (`M/Motor/RegistruTvaService.cs:114-118`) |
| `TipTvaId` | 239 | `DocumentDetaliu.TipTvaId` (`M/BusinessObjects/Documente/Document.cs:368`) |
| `Regim`, `Cota` | 243, 245 | SNAPSHOT din `TipTva` la operare (`M/Motor/RegistruTvaService.cs:127-130`) |
| `Baza`, `Tva` | 249, 251 | `RegistruTvaService.Cifre` din `DocumentDetaliu.Valoare`/`ValoareTva` (`:146,160-192`) |
| `Storno` | 255 | rândul invers, cu sume negate (`M/Motor/MotorOperare.cs:696-698`) |

Rândul se scrie DOAR dacă tipul documentului are `PoliticaTva` ȘI linia are
`TipTvaId` (`M/Motor/RegistruTvaService.cs:101-103,120-122,132-134`).
Registrul n-are rânduri de deschidere (`DocumentId`/`DetaliuId` NENULE).

Excepție de la „append-only" pe care o fac rapoartele posibilă:
`M/Motor/CorectieService.cs:97-105` REESCRIE `PerioadaAn`/`PerioadaLuna` pe
rândurile de storno deja scrise, când corecția are motivul „eroare materială".

---

## 1. Jurnalul de cumpărări / jurnalul de vânzări

### 1.1 Identitate
Un singur raport parametrizat pe `Sens`: „jurnal de cumpărări" = `Achizitie`,
„jurnal de vânzări" = `Livrare`.
- Proiecție: `M/Proiectii/TvaProiectii.cs:172-283` (rândul: `:32-77`; ordinea: `:298-307`).
- Filtru de perioadă partajat: `TvaProiectii.IntreLuni` (`:145-157`).
- Ușă: REST `GET /api/proiectii/jurnal-tva` (`W/API/Conta/TvaControllere.cs:20-64`),
  paginat prin `DataSourceLoader`. Fără ușă XAF și fără OData (registrul are
  ListView XAF read-only, dar nu jurnalul).
- Client: `nou/Atlas.Conta.Client/src/felii/tva/JurnalTva.tsx:42`.

### 1.2 Granul rândului
`(DocumentId × TipTvaId × PartenerId × Regim × Cota × Storno × PerioadaAn ×
PerioadaLuna)` — `M/Proiectii/TvaProiectii.cs:211-215`. Contractul e „per
(Document × TipTva)"; restul cheii e funcțional determinat, cu DOUĂ excepții
reale: `Storno` (operarea și stornarea sunt două fapte fiscale) și perechea de
perioadă.

### 1.3 Coloanele

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `DocumentId` | `RegistruTva.DocumentId` | registru | `TvaProiectii.cs:217,252` |
| `DocumentNumar` | `Document.Numar` prin LEFT JOIN | document | `TvaProiectii.cs:244-245,253`; `Document.cs:93` |
| `DocumentTip` | `TipDocument.Cod` pe `Document.ClrType`, completat ÎN MEMORIE peste pagină | document + nomenclator | `TvaControllere.cs:62`; `Proiectii/ContabilProiectii.cs:1094-1102`; `Api/CititorTipDocument.cs:13-43` |
| `Data` | `MIN(RegistruTva.Data)` pe grup | registru | `TvaProiectii.cs:225` |
| `PerioadaAn`/`PerioadaLuna` | `RegistruTva.Perioada*` | registru | `TvaProiectii.cs:223-224` |
| `PartenerId` | `RegistruTva.PartenerId` | registru | `TvaProiectii.cs:219,259` |
| `PartenerDenumire` | `Repartitor.Denumire` LEFT JOIN | nomenclator | `TvaProiectii.cs:246-248,260` |
| `PartenerCodFiscal` | `(p as Partener).CodFiscal` | nomenclator | `TvaProiectii.cs:261`; `Repartitori.cs:56` |
| `TipTvaId` | `RegistruTva.TipTvaId` | registru | `TvaProiectii.cs:218,262` |
| `TipTvaCod`/`TipTvaDenumire` | `TipTva.Cod`/`.Denumire` LEFT JOIN | nomenclator | `TvaProiectii.cs:249-250,263-264`; `TipTva.cs:22-23` |
| `Regim` | `RegistruTva.Regim` (snapshot) → string prin `CASE` | registru | `TvaProiectii.cs:272-276` |
| `Cota` | `RegistruTva.Cota` (snapshot) | registru | `TvaProiectii.cs:277` |
| `CodSafT` | `TipTva.CodSafTAchizitie`/`CodSafTLivrare`, ales pe `sens` | nomenclator | `TvaProiectii.cs:233,278`; `TipTva.cs:61-62` |
| `Baza`, `Tva` | `SUM(RegistruTva.Baza/.Tva)` | registru | `TvaProiectii.cs:226-227` |
| `Storno` | `RegistruTva.Storno` | registru | `TvaProiectii.cs:222,281` |

### 1.4 Filtrele și cheile de agregare
- `Sens` OBLIGATORIU, refuz 400 în controller (`TvaControllere.cs:41-45`).
- Perioada: pe `PerioadaAn*100+PerioadaLuna`, NU pe `Data` și nu pe
  `DataInregistrare` (`TvaProiectii.cs:145-157`). Ambele capete opționale,
  reduse la luna lor.
- Filtrarea se face pe RÂNDURI, înaintea grupării (`:175-176`) — de aceea rândul
  de storno cade în luna stornării.
- Nu filtrează `Stare` a documentului (registrul conține doar fapte operate) și
  NU filtrează `Storno`: suma algebrică e adevărul.
- Sumează `Baza` și `Tva`; `Data` e `MIN`.

### 1.5 Ce citește din DOCUMENT și nu din registru
- `Document.Numar` (`TvaProiectii.cs:253`).
- `Document.ClrType` → `TipDocument.Cod`, în al doilea query, pe pagină
  (`CititorTipDocument.cs:17-21,33-37`).
- Indirect, prin motor: `Data`, `DataInregistrare`, `PredatorId`/`PrimitorId` și
  `Valoare`/`ValoareTva` ale liniei au intrat în registru la operare.

### 1.6 Cazuri speciale
- Toate cele trei join-uri de etichetă sunt LEFT: o etichetă dispărută nu scoate
  rândul din raport (`:235-250`).
- `CodSafT` e DIRECȚIONAL și se rezolvă la CITIRE, cu nomenclatorul de azi
  (`:230-233,278`).
- Lanțul `RegimTva → string` e scris inline de trei ori (jurnal `:272-276`,
  decont `:358-362`, rectificativă `:428-432`) ca să se traducă în `CASE`.
- Ordinea implicită e declarată și TOTALĂ (`Data, DocumentId, TipTvaId, Storno`)
  pentru corectitudinea paginării (`:298-307`).

---

## 2. Decontul „schelet" (`DecontTva`)

### 2.1 Identitate
`M/Proiectii/TvaProiectii.cs:323-372` (rândul `:80-93`, ordinea `:379-384`);
ușă REST `GET /api/proiectii/decont-tva` (`W/API/Conta/TvaControllere.cs:72-95`),
paginat; client `DecontTva.tsx:30`.

### 2.2 Granul rândului
`(Sens × TipTvaId × Regim × Cota)` — `:336`. Cheia e `TipTva`, NU (Regim×Cota):
`SDD`/`SFD` au același regim și aceeași cotă 0, dar coduri ANAF și rânduri D300
diferite (`:311-319`).

### 2.3 Tabelul coloanelor

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `Sens` | `RegistruTva.Sens` → string | registru | `TvaProiectii.cs:338,353` |
| `TipTvaId` | `RegistruTva.TipTvaId` | registru | `:339,354` |
| `TipTvaCod`/`TipTvaDenumire` | `TipTva` LEFT JOIN | nomenclator | `:350-351,355-356` |
| `Regim`, `Cota` | snapshot de pe rând | registru | `:340-341,358-363` |
| `CodSafT` | `TipTva.CodSafT*` ales pe `Sens` per grup | nomenclator | `:366-367` |
| `Randuri` | `COUNT(*)` rânduri de registru | calcul | `:342,368` |
| `Baza`, `Tva` | `SUM` | registru | `:343-344` |

### 2.4 Filtrele și cheile de agregare
Aceleași două capete opționale, tot pe perioada de DECLARARE (`:326`). Fără
`Sens` obligatoriu (rândurile îl poartă). Perioada e opțională deliberat
(`TvaControllere.cs:84-87`).

### 2.5 Ce citește din DOCUMENT și nu din registru
Nimic. E singurul raport de TVA fără nicio atingere de document.

### 2.6 Cazuri speciale
- O `Cota` editată în nomenclator între două luni ale perioadei produce DOUĂ
  rânduri pe același `TipTva`, nu o cifră etichetată aleator (`:328-334`).
- `Randuri` e urma spre granularitatea SAF-T, nu o cifră de declarație (`:88-90`).

---

## 3. Conținutul de rectificativă

### 3.1 Identitate
`M/Proiectii/TvaProiectii.cs:398-471` (rânduri `:97-113`, antet `:116-131`);
ușă REST `GET /api/proiectii/rectificativa-tva?an&luna`
(`W/API/Conta/RectificativaTvaController.cs:22-57`), fără paginare, cu gate
dublu (404 pe luna inexistentă sau invizibilă `:49-51`, 403 pe registru
`:53-56`). Consumat ȘI din interiorul D300 (`D300Proiectii.cs:494-499`) și D394
(`D394Proiectii.cs:701-706`).

### 3.2 Granul rândului
Două secțiuni:
- `Randuri`: un rând de REGISTRU (linie de document), fără agregare (`:414-440`).
- `Agregat`: `(Sens × TipTvaId × Regim × Cota)` — cheia decontului (`:443-448`).

### 3.3 Tabelul coloanelor

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `An`/`Luna` | parametri ai cererii | calcul | `:399` |
| `InchisaPrimaOara` | `PerioadaFiscala.InchisaPrimaOara` | perioada | `:400-404`; `PerioadaFiscala.cs:34` |
| `PerioadaDeschisa` | `PerioadaFiscala.Inchisa == false` | perioada | `:405`; `PerioadaFiscala.cs:25` |
| `EsteRectificativa` | `Randuri.Count > 0` | calcul | `:441` |
| `DocumentId` | registru | registru | `:420` |
| `DocumentNumar` | `Document.Numar` LEFT JOIN | document | `:415-416,421` |
| `DocumentData` | `Document.Data` LEFT JOIN | document | `:422`; `Document.cs:95` |
| `Data` | `RegistruTva.Data` | registru | `:423` |
| `Sens`, `TipTva*`, `Regim`, `Cota`, `Baza`, `Tva`, `Storno` | registru + LEFT JOIN de etichetă | registru / nomenclator | `:424-436` |
| `ScrisLa` | `RegistruTva.ScrisLa` | registru | `:437` |
| `Agregat.*` | ca `DecontTvaRand` (§2) | registru / nomenclator | `:443-467` |

### 3.4 Filtrele și cheile de agregare
`PerioadaAn == an && PerioadaLuna == luna && ScrisLa > InchisaPrimaOara`
(`:409-410`). Perioada niciodată închisă ⇒ raport gol, fără reper (`:406-407`).
Ordonare `ScrisLa, DocumentId, TipTvaId` (`:439`).

### 3.5 Ce citește din DOCUMENT și nu din registru
`Document.Numar` și `Document.Data` (`:421-422`).

### 3.6 Cazuri speciale
- Rectificativa e DERIVATĂ din compararea a două timestamp-uri, nu e un flag
  (`:386-395`).
- Redeschiderea NU șterge `InchisaPrimaOara`, deci reperul original rămâne.
- Subiectul are înțeles doar pe o lună calendaristică EXACTĂ — `LunaExacta`
  (`:476-480`); pe un interval multi-lună D300/D394 raportează `false` cu listă
  goală.

---

## 4. Decontul de TVA — formularul 300

### 4.1 Identitate
- Proiecție: `M/Proiectii/D300Proiectii.cs:161-501`; DTO-uri `:39-107`;
  validările formularului `:520-578`.
- Nomenclatorul rândurilor: `M/BusinessObjects/Nomenclatoare/RandD300.cs:42-73`
  (55 de poziții, `[ForbidCRUD]`, venite din seed).
- Politica de așezare: `MapareD300`
  (`M/BusinessObjects/Politici/Politici.cs:389-434`).
- Ușă: REST `GET /api/proiectii/d300` (`W/API/Conta/D300Controller.cs:26-82`),
  FĂRĂ paginare, cu perioadă obligatorie; client `D300.tsx:67`.

### 4.2 Granul rândului
Rândul de IEȘIRE = o poziție a formularului (`RandD300`), adică
`cod × (coloana Bază, coloana TVA)`. Intern se trece prin granul
`(Sens × TipTvaId × Regim × Cota)` (`:185-196`).
Secțiuni cu gran propriu:
- `Randuri`: poziție de formular (55) — `:476-490`.
- `Nemapate`: `(Sens × TipTva × Regim × Cota)` — `:270-284`.
- `DiferenteDeclarat`: cheia decontului, din rectificativă — `:494-499`.

### 4.3 Tabelul coloanelor

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `Cod`, `Denumire`, `Ordine` | `RandD300.Cod`/`.Denumire`/`.Ordine` | politica | `:201-212,479-484`; `RandD300.cs:45,46,50` |
| `Sectiune` | `RandD300.Sectiune` | politica | `:481`; `RandD300.cs:47` |
| `Fel` | `RandD300.Fel` | politica | `:482`; `RandD300.cs:57` |
| `Nivel` | adâncimea lanțului `ParinteId`, cu gardă de ciclu | calcul | `:222-229`; `RandD300.cs:62` |
| `Baza` (Fel=Operatiuni) | `Σ RegistruTva.Baza` pe grupurile mapate pe rând | registru + politica | `:193,312-313`; mapări `:240-242,261-266` |
| `Tva` (Fel=Operatiuni) | `Σ RegistruTva.Tva`, idem | registru + politica | `:194,316-317` |
| `Baza`/`Tva` (Fel=Total) | formula legii, scrisă în cod | calcul | `:403-443` |
| `Baza`/`Tva` (Fel=Oglinda) | copie din rândul `RandD300.OglindaA` | politica + calcul | `:363-372`; `RandD300.cs:70` |
| `Tva` (Fel=Extern, rd. 38/39/41/42) | parametri ai cererii | politica (parametru) | `:39-43,415-418`; `D300Controller.cs:57-58` |
| `Baza`/`Tva` (rd. 27/28/32/34) | rămân 0 — fără sursă în model | calcul | `:419-422` |
| coloană absentă în formular | `null`, NU 0 | politica | `:485-486`; `RandD300.cs:55-56` |
| `Randuri` | `Σ COUNT` rânduri de registru; 0 pe total/oglindă/extern | calcul | `:192,330,487` |
| `Surse` | codurile `TipTva` distincte care au alimentat rândul | nomenclator | `:233-238,331-332,488` |
| `Nemapate.*` | grupul de registru + etichetă LEFT | registru / nomenclator | `:270-284` |
| `Rectificativa`, `PerioadaDeschisa`, `DiferenteDeclarat` | vezi §3 | perioada / registru | `:494-499` |
| `Avertismente` | liste construite în cod | calcul | `:391-394,454-471,520-578` |

### 4.4 Filtrele și cheile de agregare
- Perioada de DECLARARE, prin `IntreLuni` (`:185`); ambele capete obligatorii
  (`D300Controller.cs:44-47`).
- `Storno` NU intră nici în filtru, nici în cheie: la D300 adevărul e chiar suma
  ALGEBRICĂ (`:172-175`).
- Cheia de agregare include `Regim` (îl consumă rd. 31) și `Cota` (snapshot,
  pentru `D300Nemapat.Cota`) — `:176-184`.
- O pereche `(TipTva, Sens)` poate cădea pe MAI MULTE rânduri; suma peste
  rânduri depășește deliberat suma registrului (`:244-250`).
- Totalurile se calculează peste TOATE rândurile — de aici lipsa paginării
  (`:11-16`; `D300Controller.cs:11-18`).

### 4.5 Ce citește din DOCUMENT și nu din registru
NIMIC. D300 nu atinge `Document` sau `DocumentDetaliu` pe nicio ramură.
Singurele surse ne-registru sunt nomenclatorul `RandD300`, politica
`MapareD300`, etichetele `TipTva`, `PerioadaFiscala` (prin rectificativă) și cei
patru parametri externi ai cererii.

### 4.6 Cazuri speciale
- **Ordinea celor cinci pași e load-bearing**: mapări → părinți „din care" →
  oglinzi → externi → totaluri (`:18-26`).
- **Gardul contra dublei numărări**: sub-rândurile „din care" lipsesc din
  `OperanziRd19`/`OperanziRd30` (`:127-133`).
- **Rd. 31 = rd. 30 − nedeductibil**, unde nedeductibilul e `Σ Tva` al
  grupurilor cu `Regim = Capitalizat`, numărat PE APARIȚIE în rd. 30 (ținte cu
  coloană TVA care sunt operanzi ai rd. 30), nu pe grup (`:288-307,433`).
  Ne-aditiv: aceeași cifră de registru se poate scădea de 0, 1 sau 2 ori.
- **Oglinzile** copiază din sursă DUPĂ ce sursa și-a strâns copiii; `Randuri` se
  zeroizează, `Surse` se moștenește (`:352-372`).
- **Singurele trunchieri**: `max(…,0)` pe rd. 36/37/44/45 (`:438-443`). Rândurile
  de operațiuni pot ieși NET NEGATIVE și rămân așa.
- **Cifra care nu încape** pe o coloană absentă nu se înghite: se ține deoparte
  și iese ca avertisment (`:310-315,328-329,464-471`).
- **Validările blocante ale formularului** (V_6 „rd. 31 ≤ rd. 30", ierarhia „din
  care", egalitățile-oglindă) rulează pe cifrele NOASTRE și ies ca AVERTISMENT,
  nu 422 (`:520-578`).
- **Lipsa oricărui rând-formulă** oprește TOATE totalurile, cu un avertisment
  care nu acuză nici seed-ul, nici securitatea (`:379-395`).
- **O singură versiune a formularului** (OPANAF 174/2026); perioadele anterioare
  primesc avertisment (`:114,454-458`).
- Nu rotunjește nimic — cifrele vin deja rotunjite la bani din registru (`:30-32`).

---

## 5. Declarația informativă 394

### 5.1 Identitate
- Proiecție: `M/Proiectii/D394Proiectii.cs:283-708`; DTO-uri `:39-179`.
- Politica de tip de operațiune: `MapareD394`
  (`M/BusinessObjects/Politici/Politici.cs:525-568`, cu gardul `TintaPermisa`
  `:564-568`).
- Clasificarea fiscală a partenerului:
  `M/BusinessObjects/Comun/ClasaFiscala.cs:22-31`.
- Ușă: REST `GET /api/proiectii/d394` (`W/API/Conta/D394Controller.cs:23-51`),
  fără paginare, perioadă obligatorie; client `D394.tsx:60`.

### 5.2 Granul rândului (patru secțiuni)
- `Operatiuni` (`op1`): `(CheieCui × Tip × Sens × Cota)` — `:442-448`.
  `CheieCui` = CUI normalizat, sau identitatea partenerului când n-are cod
  (`:257`). `TipPartener` e FUNCȚIE de cheie, nu axă (`:437-441`).
- `Rezumat` (`rezumat1`): `(TipPartener × Cota)` — `:490`.
- `RezumatCote` (`rezumat2`): `Cota ≠ 0` — `:527`.
- `Neincluse`: `(Cauză × Sens × TipTva × Cota × Repartitor)` — `:371-374`.
- Intern se trece prin granul de agregare
  `(DocumentId × Storno × PartenerId × Sens × TipTvaId × Cota)` — `:293-306`.

### 5.3 Tabelul coloanelor (`op1`)

| câmp | sursă | fel | fișier:linie |
|---|---|---|---|
| `TipPartener` | `ClasaFiscala.APartenerului(TipPersoana, Tara, InregistratTva)`, apoi re-decis PE CUI peste nomenclatoare | nomenclator + calcul | `:195-196,326-327,336-344`; `Repartitori.cs:65,77,94` |
| `CuiP` | `Partener.CodFiscal` normalizat (trim, majuscule, `RO` tăiat repetat, cod fără alfanumerice ⇒ null) | nomenclator + calcul | `:214-225,325`; `Repartitori.cs:56` |
| `Denumire` | `Partener.Denumire`; prima în ordine la parteneri uniți pe CUI | nomenclator | `:274-275,476` |
| `Tip` | `MapareD394.(TipTva × Sens) → Tip`, cu `A → AI` dacă `Partener.TvaLaIncasare` | politica + nomenclator | `:364-368,408-415`; `Repartitori.cs:99` |
| `Sens` | `RegistruTva.Sens` | registru | `:298,419,478` |
| `Cota` | `int(trunc(RegistruTva.Cota))`; 0 pentru V/LS/AS/N | registru + calcul | `:236-241,416` |
| `NrFact` | 1 pe cota cu `Σ Tva` absolut maxim per `(Document × Storno × CheieCui × Tip)`, 0 pe restul | calcul | `:422-435,455-457` |
| `Baza` | `Σ RegistruTva.Baza` | registru | `:303,450` |
| `Tva` | `Σ RegistruTva.Tva`, sau `null` pe tipurile fără coloană (V/LS/AS/N) | registru | `:304,451,482` |
| `TvaNedeclarat` | `Σ Tva` când tipul n-are coloană | calcul | `:483` |
| `Randuri` | `COUNT` rânduri de registru | calcul | `:302,452` |
| `Documente` | `COUNT DISTINCT DocumentId` | registru | `:454,485` |

`Rezumat`/`RezumatCote` sunt agregări pure peste `op1` (`:492-494,528-530`), cu
reguli de PREZENȚĂ per `(tip_partener, cotă)` scrise în cod (`:496-501`);
`FacturiN`/`BazaN` sunt un 0 adevărat, fără sursă (`:522-524`).
`NrCui1..4`: persoane distincte pe tip 1/3/4, ÎNREGISTRĂRI pe tip 2 (`:545-548`).

### 5.4 Filtrele și cheile de agregare
- Perioada de DECLARARE, prin `IntreLuni` (`:293`); ambele capete obligatorii
  (`D394Controller.cs:35-43`).
- `Storno` NU se filtrează, dar INTRĂ în cheie: la ANAF factura și factura de
  storno sunt DOUĂ facturi, iar la noi stau pe același `DocumentId` (`:288-292`).
- Partenerii se citesc cu `IgnoreQueryFilters()` — facturile unui partener șters
  logic se declară (`:318,349,654`).
- Nu rotunjește la leu (`:23-24`); nu paginează.

### 5.5 Ce citește din DOCUMENT și nu din registru
- `RegistruTva.DetaliuId` → `DocumentDetaliu.LotId` → `Lot.Produs.CodNc`
  (`:651-660`; `ProdusLot.cs:57`).
- `FacturaIntrareDetaliu.Produs.CodNc` și `FacturaIesireDetaliu.Produs.CodNc` —
  citite direct de pe FRUNZELE de linie (`:661-666`).
  Ambele DOAR pentru avertismentul `FaraOp11`, nu pentru cifre.
- `DocumentId` intră în cheia de agregare, în `NrFact` și în `Documente`, dar ca
  identitate, nu ca citire de câmp.

### 5.6 Cazuri speciale
- **`AI` e DERIVAT în cod**, nu mapat: `A` × furnizor cu `TvaLaIncasare`
  (`:412-415`); `MapareD394` REFUZĂ țintele `AI` și `N`
  (`Politici.cs:520-523,564-568`).
- **Tipul de partener se re-decide pe CUI**, peste nomenclatoare: „înregistrat
  bate tot"; fără niciun înregistrat, tipul primului pe `Id` + avertisment
  (`:332-344,587-595`).
- **`NrFact` e ne-aditiv**: regula 1/0 per document, cu departajare pe TVA
  absolut maxim și, la egalitate, cota mai mare (`:422-435`). `Facturi` e memoria
  „am văzut factura asta pe rândul ăsta", ca incrementul să nu vină de două ori
  când o factură are două `TipTva` pe aceeași cotă (`:459-462`).
- **Normalizarea CUI taie prefixul `RO` REPETAT** (`RORo1853162 ⇒ 1853162`) —
  cauza reală a unei respingeri SAF-T la ANAF (`:205-212,219-220`); un „cod" fără
  alfanumerice devine null, altfel toți s-ar uni pe o cheie (`:221-224`).
- **Cotele ne-întregi** se trunchiază pe rând și se strigă (`:236-241,619-623`).
- **Nimic nu se pierde**: fiecare grup ajunge ori în `Operatiuni`, ori în
  `Neincluse` cu cauza lui; Σ pe ambele == Σ registrului (`:29-33,399-420`).
  Cauzele: `FaraPartener` (`:401`), `RepartitorNePartener` (`:405`),
  `TipTvaNemapat` (`:409`).
- **Nouă avertismente agregate** (max. 5 exemple fiecare): `CuiUnit` (`:582`),
  `ClasificariDiferite` (`:590`), `Tip1FaraCui` (`:597`), `PfFaraCnp` (`:606`,
  CNP = exact 13 cifre `:228`), `TvaPeTipFaraColoana` (`:613`), `CotaNeintreaga`
  (`:620`), `FaraOp11` (`:668`), `CombinatieRefuzata` (`:680`), `PartenerSters`
  (`:691`).
- **`rezumat2` unește V la L și C la A**, dar V are cota declarată 0 și cartușul
  H n-are rând de cota 0 ⇒ V rămâne doar în `op1`/`rezumat1` (`:107-111,533-538`).

---

## 6. Anexă: `InchidereTvaService` (generatorul ITV)

Nu e raport, dar e în lot și e singurul consumator de TVA care NU citește
`RegistruTva`. `M/Motor/InchidereTvaService.cs`:
- Conturile: `PoliticaInchidereTva` (4426/4427/4423/4424) — `:180-184`;
  `Politici.cs:330-355`. Fără setul complet ⇒ `ProfilInert`.
- Soldurile: `SolduriService.AtomiCumulati` — snapshot de perioadă de referință
  + rulaje de după (`:268-280`). Deci **snapshot + registru CONTABIL**, nu
  registrul de TVA.
- Liniile: `Transfer = min(4426, 4427)`, `DePlata = 4427 − transfer`,
  `DeRecuperat = 4426 − transfer`, rotunjite la bani (`:58-64`).
- Gardienii, în ordine: profil → închidere vie în lună → cronologie ulterioară →
  draft anterior → perioadă închisă (`GardianPerioada.VerificaDeschisa`) →
  solduri zero (`:169-256`).
- `Math.Max(0, …)` pe ambele solduri: un sold pe sensul „greșit" nu se închide
  (`:278-279`).
- ITV nu produce rânduri de `RegistruTva` (n-are `PoliticaTva`) — nota lui mișcă
  4426/4427 fără a fi operațiune taxabilă (`M/Motor/RegistruTvaService.cs:93-95`).
