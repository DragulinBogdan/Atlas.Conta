# DIM-2 — Inventarul dimensiunilor culese per tip (pasul 1, decizia 54e)

> Metodă: probe, nu ghicit. Sursele: (1) scenariile ModelCheck (singura
> „culegere" reală de azi), (2) handler-ele Import1C (anul 2025 pe date reale),
> (3) validările + politicile seed din Module, (4) UI-ul XAF actual,
> (5) uneltele Migrare/seed. Data: 2026-08-07.

## 1. Faptele scoase de probe

**F1. Import1C nu atinge owned-ul deloc.** Zero referințe `Dimensiuni` în
`nou/tools/Import1C`. Anul 2025 integral (187k documente, contract 4×12 verde)
a trecut prin motor FĂRĂ nicio dimensiune culeasă pe linie. Nevoile de
dimensiuni ale importului sunt acoperite de: postarea explicită pe NTC
(`RepartitorDebitId/CreditId` — FK-uri proprii ale `NotaContabilaDetaliu`,
deja pe frunză), Materialul din lot (rezolvat de motor, 33b) și default-ul
polimorf de header.

**F2. UI-ul actual nu culege NICIO dimensiune.** Owned-ul se afișează
read-only ca ToString (40e/41c). Culegerea de dimensiuni pe linie există azi
DOAR în codul ModelCheck (scenariile bugetare). DIM-2 nu „mută" culegerea din
UI — o face posibilă pentru prima dată (FK-uri normale = lookup standard).

**F3. Nicio regulă de contare seed nu poartă override-uri de dimensiuni.**
`DimensiuniComun/OverrideDebit/OverrideCredit` nu apar în niciun seed
(`DatabaseUpdate/`) — nivelul „regulă" al coalesce-ului e mecanism viu, dar
date moarte. (Rămâne neatins acum; devine plat la DIM-3.)

**F4. Migrare scrie dimensiuni doar pe REGISTRU** (rândurile de deschidere,
`DimensiuniDebit/Credit` — 34d). Partea de registru e teritoriul DIM-3;
DIM-2 nu atinge unealta Migrare.

**F5. `DimensiuniObligatorii` (gardianul 33a)**: bugetar = din CPLAN_DEFALCARE
(R/M/F/E/B/P); privat = gol. Gardianul verifică seturile REZOLVATE — nu cere
ca dimensiunea să vină de pe linie, dar linia e singura sursă pentru ce nu dau
lotul (M), header-ul (R) sau angajamentul (puntea E).

**F6. Probele ModelCheck, linie cu linie** (`tools/ModelCheck/Program.cs`):

| Locul | Tipul liniei | Dimensiuni setate |
|---|---|---|
| 2217–2218, 2906, 2913 | `FacturaIntrareDetaliu` | CodEconomic |
| 2328–2330 | `FacturaIntrareDetaliu` | SursaFinantare, CodFunctional, Proiect (404/BFEPR) |
| 2247 | linia NIR conex | primește CodEconomic prin CLONĂ din FCT |
| 2933 | liniile Plata autogenerată | primesc CodEconomic prin CLONĂ din FCT |
| 2745, 2778, 2970 | `FacturaIesireDetaliu` | CodEconomic (751 cere E) |
| 2586 | `ListaDiferenteInventarDetaliu` (plus) | CodEconomic (791 cere E) |
| 3146, 3157, 3192 | `DecontDetaliu` | CodEconomic |
| 3029, 3123, 3127 | linia Plata (azi detaliu de BAZĂ) | CodEconomic (531/542 cer E) |
| 2989, 3216 | linia Incasare (azi detaliu de BAZĂ) | CodEconomic |
| 3409 | `NotaContabilaDetaliu` | CodEconomic (defalcarea E pe trezorerie) |

**F7. Repartitor și Material nu se culeg pe linie NICĂIERI.** Repartitor =
default polimorf de header + postarea explicită (DEC/NTC au deja FK-urile
`RepartitorDebit/Credit` proprii). Material = din lotul liniei (33b).
La frunze NU se pun câmpuri pentru ele.

**F8. Unitate și CentruCost nu apar în NICIO probă de culegere pe linie.**
(Unitate există doar pe rândurile de registru scrise de Migrare — F4.)
Nu primesc câmp pe nicio frunză; rămân în value object + registru. Adăugarea
ulterioară e pur aditivă.

## 2. Tabelul rezultat: tip → dimensiuni culese pe frunză

Reuniunea per TIP (nu per profil — invariant IV.3). „(clonă)" = tipul nu
culege el, dar PRIMEȘTE prin `PreiaDimensiuni` de la sursă, deci frunza
trebuie să poarte câmpurile.

| Tip | Detaliu azi | Dimensiuni pe frunză | Sursa nevoii |
|---|---|---|---|
| FacturaIntrare (FCT) | `FacturaIntrareDetaliu` | **CodEconomic, SursaFinantare, CodFunctional, Proiect** | culegere directă (F6) |
| NIR | detaliu de BAZĂ | **aceleași 4** (clonă din FCT) | clona conexă copiază liniile de stoc |
| Plata (PLT) | detaliu de BAZĂ | **aceleași 4** (culege E + clonă din FCT) | plata autogenerată clonează defalcarea FCT |
| Incasare (INC) | detaliu de BAZĂ | **aceleași 4** (culege E; simetria PLT) | culegere directă; vezi Î2 |
| FacturaIesire (FCL) | `FacturaIesireDetaliu` | **CodEconomic** | culegere directă |
| DescarcareGestiune (DSC) | `DescarcareGestiuneDetaliu` | **CodEconomic** (clonă din FCL) | `DescarcareService` copiază dimensiunile liniei sursă |
| ListaDiferenteInventar (LDI) | `ListaDiferenteInventarDetaliu` | **CodEconomic** | culegere directă (plus) |
| Decont (DEC) | `DecontDetaliu` | **CodEconomic** (+ postarea explicită existentă) | culegere directă |
| NotaContabila (NTC) + ITV | `NotaContabilaDetaliu` | **CodEconomic** (+ postarea explicită existentă) | culegere directă |
| NotaTransfer (BTR) | bază | **nimic** | nicio probă |
| BonConsum (BCS) | bază | **nimic** | nicio probă |
| Asamblare (ASM) | `AsamblareDetaliu` | **nimic** | nicio probă |
| ReturFurnizor/ReturClient | bază | **nimic** | nicio probă |
| RaportProductie (BPR) | rezervat (19) | **nimic** | clasa rezervată |

## 3. Consecințe de schemă (propunerea de implementare)

1. **Frunze NOI**: `NirDetaliu` și un detaliu pentru trezorerie — propunere:
   **`DocumentTrezorerieDetaliu` UNIC** pe baza comună `DocumentTrezorerie`
   (PLT+INC au aceeași semantică de linie: defalcarea sumei — 31a), în loc de
   două clase-gemene. Vezi Î1.
2. **FK-uri noi pe frunzele existente** conform tabelului (perechi
   scalar+navigație, ca orice FK XAF; lookup standard).
3. **Override-uri** `DimensiuniCulese()`/`PreiaDimensiuni()` per frunză —
   exact forma din DIM-1, implementarea de bază devine goală (`new()` / no-op).
4. **Migrația**: coloane noi pe tabelele frunzelor (TPT) + UPDATE de mutare
   din `DocumentDetalii.Dimensiuni_*` (join pe ID) + DROP pe cele 8+8 coloane
   `Dimensiuni_*` de pe bază. Datele reale afectate sunt minime (F1/F2 — doar
   bazele de ModelCheck și eventuale culegeri manuale bugetare), dar mutarea
   se scrie complet, nu „pe încredere".
5. **Culegerea din tipuri** (`BTR/BCS/ASM/RLF/RDC`) nu se schimbă — moștenesc
   implementarea goală de pe bază.
6. **Neatinse în DIM-2**: registrul (`DimensiuniDebit/Credit`), `RegulaContare`
   (cele 3 seturi), unealta Migrare, maparea owned din DbContext pentru ele —
   toate la DIM-3. Import1C: doar recompilare (F1).

## 4. Întrebările deschise — TRANȘATE (validate de utilizator, 2026-08-07)

- **Î1 → `DocumentTrezorerieDetaliu` UNIC** pe baza `DocumentTrezorerie`
  (PLT+INC): semantica liniei e identică (defalcarea sumei — 31a), clona
  plății automate scrie în același tip.
- **Î2 → toate cele 4 câmpuri pe trezorerie, cu nuanța de profil**: la
  bugetar („public") sunt necesare — obligativitatea o dau `PoliticaValidare`
  + `DimensiuniObligatorii` (date seed, există deja); la privat rămân
  OPȚIONALE (nicio validare hardcodată în clasă — decizia 54d: schema per
  tip, politică + vizibilitate per profil; vizibilitatea la DIM-4).
- **Î3 → culegibile pe NIR manual**, același layout ca FCT — altfel NIR-ul
  manual nu poate satisface defalcarea conturilor 3xx la bugetari.

## 5. Planul pașilor de implementare (modul: main-ul spune, utilizatorul face)

1. **Frunzele noi + FK-urile** (doar model; fără migrație, fără override-uri —
   owned-ul rămâne sursa de adevăr până la pasul 2): `NirDetaliu`,
   `DocumentTrezorerieDetaliu`, FK-urile pe frunzele existente, `[TipDetaliu]`
   pe NIR/PLT/INC, DbSet-urile. Build verde, comportament neschimbat.
2. **Bascularea**: migrația EF (coloane pe frunze + UPDATE mutare din
   `Dimensiuni_*` + DROP pe bază), scoaterea owned-ului `Dimensiuni` de pe
   `DocumentDetaliu` (proprietate + `OwnsOneRequired`), override-urile
   `DimensiuniCulese`/`PreiaDimensiuni` pe frunze, baza devine goală
   (`new()` / no-op). Pierdere de date: zero în practică (F1/F2 — nicio bază
   reală nu are dimensiuni culese pe linii; doar bazele ModelCheck, care se
   reconstruiesc).
3. **Clonările creează frunza declarată**: `GenereazaConex`
   (MotorOperare:433) și plata autogenerată (FacturaIntrare:78) instanțiază
   azi `DocumentDetaliu` de BAZĂ — trec pe tipul din `[TipDetaliu]` al
   documentului țintă (sursa unică a declarației, mecanismul 40a).
4. **Refactor ModelCheck**: setterii `.Dimensiuni.X =` → FK-urile frunzelor;
   liniile PLT/INC din scenarii → `DocumentTrezorerieDetaliu`; asserțiile pe
   clone. Import1C: doar recompilare (F1).
5. **Verificarea**: build tot + ModelCheck ambele profiluri (inclusiv
   verificarea de drift migrații/model). Smoke UI rămâne la DIM-4.
