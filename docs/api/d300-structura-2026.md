# D300 — Decont de TVA, structura oficială 2026 (OPANAF 174/2026)

> **Actul**: ORDINUL ANAF nr. 174/2026 din 5 februarie 2026 pentru aprobarea
> modelului şi conţinutului formularului (300) „Decont de taxă pe valoarea
> adăugată", publicat în M.Of. nr. 105/09.02.2026.
> **Anexa 1** = modelul formularului · **Anexa 2** = instrucţiunile de completare ·
> **Anexa 3** = caracteristici de editare.
> **Intrare în vigoare** (art. 6): se utilizează începând cu declararea obligaţiilor
> fiscale aferente **primei perioade fiscale din anul 2026**, indiferent dacă
> perioada fiscală e luna, trimestrul, semestrul sau anul.
>
> **Sursă primară, integrală**: textul ordinului + ambele anexe au fost descărcate
> şi citite în întregime de pe ANAF
> (`https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_174_2026.pdf`, 15 pag.),
> completate cu schema XML oficială `structura_D300_v12.0.0_10022026.pdf` (28 pag.).
> **Nimic din acest document nu e reconstituit din surse secundare** — vezi §6.

---

## 1. Ce s-a schimbat faţă de formularul 2025

Formularul are **45 de rânduri numerotate** (1–45), plus sub-rândurile 3.1, 5.1,
7.1, 12.1, 12.2, 20.1, 22.1, 26.1, 26.2, 29.1 — deci **55 de poziţii numerotate**
în corpul decontului, plus secţiunile informative A/A1/B/B1 şi casetele de facturi.

Modificarea faţă de modelul valabil august–decembrie 2025 (OPANAF care implementa
Legea 141/2025) este **exclusiv eliminarea rândurilor de cote vechi**, structura
rămânând altfel identică:

| Eliminat în 2026 | Ce conţinea | Element XML dispărut |
|---|---|---|
| rd. 9.1 | Livrări taxabile cu **19%** | `R69_1` / `R69_2` |
| rd. 10.1 | Livrări taxabile cu **9%** (altele decât cele de la rd. 11) | `R70_1` / `R70_2` |
| rd. 11.1 | Livrări taxabile cu **5%** | `R71_1` / `R71_2` |
| rd. 12.3 | Achiziţii taxare inversă art. 331, cota **19%** | `R12_3_1` / `R12_3_2` |
| rd. 12.4 | Achiziţii taxare inversă art. 331, cota **9%** | `R72_1` / `R72_2` |
| rd. 12.5 | Achiziţii taxare inversă art. 331, cota **5%** | `R73_1` / `R73_2` |
| rd. 24.1 | Achiziţii taxabile cu **19%** | `R74_1` / `R74_2` |
| rd. 25.1 | Achiziţii taxabile cu **9%** | `R75_1` / `R75_2` |
| rd. 26 (vechi) | Achiziţii taxabile cu **5%** | `R24_1` / `R24_2` |
| rd. 27.4 | Achiziţii simplificare, cota **9%** | `R76_1` / `R76_2` |
| rd. 27.5 | Achiziţii simplificare, cota **5%** | `R77_1` / `R77_2` |
| rd. 14.1, 14.2 | Livrări scutite cu drept, art. 294 alin. (5) lit. a)–d) | `R67_1`, `R68_1` |

Consecinţă: **numerotarea rândurilor s-a comprimat**. Un rând care în 2025 purta
numărul N poartă acum, tipic, N−2 în zona deductibilă. Vezi maparea completă în §5
— e capcana principală la portarea unui generator D300 existent.

**Cotele rămase în formular**: 21% (standard), 11% (redusă), **9% doar tranzitoriu**
pe rd. 11 — livrări de bunuri conform art. III din Legea nr. 141/2025 (livrarea de
locuinţe către persoane fizice, aplicabil până la 31.07.2026 în condiţiile legii).
Cotele 24/20/19/9/5% **nu mai au rânduri proprii**; ele intră exclusiv prin
rândurile de regularizare 16 (colectată) şi 33 (dedusă) — vezi §4.

---

## 2. Tabelul complet al rândurilor

Legenda coloanelor: **B** = „Valoare" (baza de impozitare) · **T** = „TVA".
Secţiuni: **COL** = TVA colectată · **DED** = TVA deductibilă ·
**REG** = Regularizări conform art. 303 · **INF** = informativ.

### 2.1 Secţiunea „TAXA PE VALOAREA ADĂUGATĂ COLECTATĂ" (rd. 1–19)

Sub-secţiunea **COMERŢ INTRACOMUNITAR ŞI ÎN AFARA UE**:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| 1 | Livrări intracomunitare de bunuri, scutite conform art. 294 alin. (2) lit. a) şi d) din Codul fiscal | B | COL | — |
| 2 | Regularizări livrări intracomunitare scutite conform art. 294 alin. (2) lit. a) şi d) din Codul fiscal | B | COL | — |
| 3 | Livrări de bunuri sau prestări de servicii pentru care locul livrării/locul prestării este în afara României (în UE sau în afara UE), precum şi livrări intracomunitare de bunuri, scutite conform art. 294 alin. (2) lit. b) şi c) din Codul fiscal, **din care:** | B | COL | — |
| 3.1 | Prestări de servicii intracomunitare care nu beneficiază de scutire în statul membru în care taxa este datorată | B | COL | — |
| 4 | Regularizări privind prestările de servicii intracomunitare care nu beneficiază de scutire în statul membru în care taxa este datorată | B | COL | — |
| 5 | Achiziţii intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă), **din care:** | B+T | COL | — |
| 5.1 | Achiziţii intracomunitare pentru care cumpărătorul este obligat la plata TVA (taxare inversă), iar furnizorul este înregistrat în scopuri de TVA în statul membru din care a avut loc livrarea intracomunitară | B+T | COL | — |
| 6 | Regularizări privind achiziţiile intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă) | B+T | COL | — |
| 7 | Achiziţii de bunuri, altele decât cele de la rd. 5 şi 6, şi achiziţii de servicii pentru care beneficiarul din România este obligat la plata TVA (taxare inversă), **din care:** | B+T | COL | — |
| 7.1 | Achiziţii de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă) | B+T | COL | — |
| 8 | Regularizări privind achiziţiile de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă) | B+T | COL | — |

Sub-secţiunea **LIVRĂRI DE BUNURI/PRESTĂRI DE SERVICII ÎN INTERIORUL ŢĂRII ŞI EXPORTURI**:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| 9 | Livrări de bunuri şi prestări de servicii, taxabile cu **cota 21%** | B+T | COL | — |
| 10 | Livrări de bunuri şi prestări de servicii, taxabile cu **cota 11%** | B+T | COL | — |
| 11 | Livrări de bunuri taxabile cu **cota 9%** conform art. III din Legea nr. 141/2025 | B+T | COL | — |
| 12 | Achiziţii de bunuri şi servicii supuse măsurilor de simplificare pentru care beneficiarul este obligat la plata TVA (taxare inversă), **din care:** | B+T | COL | rd. 12 ≥ rd. 12.1 + rd. 12.2 (pe ambele coloane) |
| 12.1 | Achiziţii de bunuri şi servicii, taxabile cu **cota 21%** | B+T | COL | — |
| 12.2 | Achiziţii de bunuri, taxabile cu **cota 11%** | B+T | COL | — |
| 13 | Livrări de bunuri şi prestări de servicii supuse măsurilor de simplificare (taxare inversă) | B | COL | — |
| 14 | Livrări de bunuri şi prestări de servicii **scutite cu drept de deducere**, altele decât cele de la rd. 1–3 | B | COL | — |
| 15 | Livrări de bunuri şi prestări de servicii **scutite fără drept de deducere** | B | COL | — |
| 16 | Regularizări taxă colectată | B+T | COL | — |

Zona **vânzări la distanţă / servicii electronice** (casetă proprie, cu coloane Valoare + TVA):

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| 17 | Vânzări intracomunitare de bunuri la distanţă şi prestări de servicii de telecomunicaţii, de radiodifuziune şi televiziune, precum şi servicii furnizate pe cale electronică către persoane neimpozabile dintr-un alt stat membru, pentru care locul livrării/prestării este în România, conform art. 278^1 alin. (1) din Codul fiscal | B+T | COL | — |
| 18 | Regularizări privind vânzările intracomunitare de bunuri la distanţă şi prestările de servicii de telecomunicaţii, de radiodifuziune şi televiziune, precum şi servicii furnizate pe cale electronică către persoane neimpozabile dintr-un alt stat membru, conform art. 278^1 alin. (1) din Codul fiscal | B+T | COL | — |

**Totalul secţiunii**:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| **19** | **TOTAL TAXĂ COLECTATĂ** (sumă de la rd. 1 până la rd. 18, cu excepţia celor de la rd. 3.1, 5.1, 7.1, 12.1, 12.2) | B+T | COL | **col. B** = rd. 1+2+3+4+5+6+7+8+9+10+11+12+13+14+15+16+17+18<br>**col. T** = rd. 5+6+7+8+9+10+11+12+16+17+18 |

> Formula pe coloana TVA rezultă mecanic: se însumează aceleaşi rânduri, dar numai
> cele care au coloana TVA. Rândurile 1–4, 13, 14, 15 (doar bază) nu contribuie.

### 2.2 Secţiunea „TAXA PE VALOAREA ADĂUGATĂ DEDUCTIBILĂ" (rd. 20–35)

Sub-secţiunea **ACHIZIŢII INTRACOMUNITARE DE BUNURI ŞI ALTE ACHIZIŢII DE BUNURI ŞI SERVICII IMPOZABILE ÎN ROMÂNIA** — rânduri-oglindă ale zonei colectate:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă / oglindă |
|---|---|---|---|---|
| 20 | Achiziţii intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă), **din care:** | B+T | DED | **= rd. 5** (ambele coloane, validare blocantă) |
| 20.1 | Achiziţii intracomunitare pentru care cumpărătorul este obligat la plata TVA (taxare inversă), iar furnizorul este înregistrat în scopuri de TVA în statul membru din care a avut loc livrarea | B+T | DED | **= rd. 5.1** |
| 21 | Regularizări privind achiziţiile intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă) | B+T | DED | **= rd. 6** |
| 22 | Achiziţii de bunuri, altele decât cele de la rd. 20 şi 21, şi achiziţii de servicii pentru care beneficiarul din România este obligat la plata TVA (taxare inversă), **din care:** | B+T | DED | **= rd. 7** |
| 22.1 | Achiziţii de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă) | B+T | DED | **= rd. 7.1** |
| 23 | Regularizări privind achiziţii de servicii intracomunitare pentru care beneficiarul din România este obligat la plata TVA (taxare inversă) | B+T | DED | **= rd. 8** |

Sub-secţiunea **ACHIZIŢII DE BUNURI/SERVICII ÎN INTERIORUL ŢĂRII ŞI IMPORTURI, ACHIZIŢII INTRACOMUNITARE SCUTITE SAU NEIMPOZABILE**:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă / oglindă |
|---|---|---|---|---|
| 24 | Achiziţii de bunuri şi servicii taxabile cu **cota de 21%**, altele decât cele de la rd. 27 | B+T | DED | — |
| 25 | Achiziţii de bunuri şi servicii, taxabile cu **cota de 11%** | B+T | DED | — |
| 26 | Achiziţii de bunuri şi servicii supuse măsurilor de simplificare pentru care beneficiarul este obligat la plata TVA (taxare inversă), **din care:** | B+T | DED | **= rd. 12**; rd. 26 ≥ rd. 26.1 + rd. 26.2 |
| 26.1 | Achiziţii de bunuri, taxabile cu **cota 21%** | B+T | DED | **= rd. 12.1** |
| 26.2 | Achiziţii de bunuri, taxabile cu **cota 11%** | B+T | DED | **= rd. 12.2** |
| 27 | Compensaţia în cotă forfetară pentru achiziţii de produse şi servicii agricole de la furnizori care aplică regimul special pentru agricultori | **T** | DED | doar coloana TVA |
| 28 | Regularizări privind compensaţia în cotă forfetară | **T** | DED | doar coloana TVA |
| 29 | Achiziţii de bunuri şi servicii scutite de taxă sau neimpozabile, **din care:** | B | DED | doar bază |
| 29.1 | Achiziţii de servicii intracomunitare scutite de taxă | B | DED | doar bază |

Totalurile şi ajustările:

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| **30** | **TOTAL TAXĂ DEDUCTIBILĂ** (sumă de la rd. 20 până la rd. 28, cu excepţia celor de la rd. 20.1, 22.1, 26.1, 26.2) | B+T | DED | **col. B** = rd. 20+21+22+23+24+25+26<br>**col. T** = rd. 20+21+22+23+24+25+26+27+28 |
| **31** | SUB-TOTAL TAXĂ DEDUSĂ CONFORM ART. 297 ŞI ART. 298 SAU ART. 300 ŞI ART. 298 DIN CODUL FISCAL ŞI COMPENSAŢIE ÎN COTĂ FORFETARĂ | **T** | DED | **rd. 31 ≤ rd. 30** (validare blocantă) |
| 32 | TVA efectiv restituită cumpărătorilor străini, inclusiv comisionul unităţilor autorizate | **T** | DED | — |
| 33 | Regularizări taxă dedusă | B+T | DED | — |
| 34 | Ajustări conform pro-rata / ajustări de taxă | **T** | DED | poate fi **±** |
| **35** | **TOTAL TAXĂ DEDUSĂ** | **T** | DED | **rd. 31 + rd. 32 + rd. 33 + rd. 34** |

> Atenţie: rd. 29 (scutite/neimpozabile) **nu** intră în totalul rd. 30 — e pur
> informativ. Şi rd. 30 nu e „taxa dedusă", ci „taxa deductibilă"; deducerea
> efectivă e rd. 31, care poate fi strict mai mică (§4).

### 2.3 Secţiunea „REGULARIZĂRI CONFORM ART. 303 DIN CODUL FISCAL" (rd. 36–45)

Toate rândurile au **doar coloana TVA**.

| Rd. | Denumire oficială | Col. | Secţ. | Formulă |
|---|---|---|---|---|
| **36** | Suma negativă a TVA în perioada de raportare (rd. 35 − rd. 19) | T | REG | **MAX(rd. 35 − rd. 19, 0)** |
| **37** | Taxa de plată în perioada de raportare (rd. 19 − rd. 35) | T | REG | **MAX(rd. 19 − rd. 35, 0)** |
| 38 | Soldul TVA de plată din decontul perioadei fiscale precedente (rd. 44 din decontul perioadei fiscale precedente) neachitate până la data depunerii decontului de TVA | T | REG | **= 0 dacă rd. 41 > 0** |
| 39 | Diferenţe de TVA de plată stabilite de organele fiscale prin decizie comunicată şi neachitate până la data depunerii decontului de TVA | T | REG | — |
| **40** | TVA de plată cumulat | T | REG | **rd. 37 + rd. 38 + rd. 39** |
| 41 | Soldul sumei negative a TVA reportate din perioada precedentă pentru care nu s-a solicitat rambursare (rd. 45 din decontul perioadei fiscale precedente) | T | REG | **= 0 dacă rd. 38 > 0** |
| 42 | Diferenţe negative de TVA stabilite de organele de inspecţie fiscală prin decizie comunicată până la data depunerii decontului de TVA | T | REG | — |
| **43** | Suma negativă a TVA cumulate | T | REG | **rd. 36 + rd. 41 + rd. 42** |
| **44** | Sold TVA de plată la sfârşitul perioadei de raportare | T | REG | **MAX(rd. 40 − rd. 43, 0)** |
| **45** | Soldul sumei negative de TVA la sfârşitul perioadei de raportare | T | REG | **MAX(rd. 43 − rd. 40, 0)** |

> 36/37 şi 44/45 sunt perechi mutual exclusive (MAX cu 0): cel mult unul din
> fiecare pereche e nenul. La fel 38/41, prin restricţia încrucişată.
> Bucla între deconturi: **rd. 44 → rd. 38 al perioadei următoare**;
> **rd. 45 → rd. 41 al perioadei următoare** (dacă nu s-a cerut rambursarea).

### 2.4 Secţiuni informative şi casete (INF)

| Poziţie | Conţinut | Câmpuri |
|---|---|---|
| Antet | Perioada de raportare (lună/trimestru/semestru/an) + Anul | `luna`, `an`, `tip_decont` ∈ {L,T,S,A} |
| Antet | Bifă „Declaraţie depusă după anularea rezervei verificării ulterioare" + temeiul legal | `temei` |
| Antet | Bifă „Declaraţie depusă potrivit art. 90 alin. (4) din Codul de procedură fiscală" + CIF succesor | `cuiSuccesor` |
| Antet | Bifă „Decont consolidat de TVA, depus de reprezentantul grupului fiscal unic" (art. 269 alin. (9)) | `depusReprezentant` |
| Antet | Bifă „decont simplificat" — numai operaţiuni în interiorul ţării (art. 5 din ordin) | `bifa_interne` |
| Antet | Date de identificare: CIF, denumire, domiciliu fiscal, telefon/fax/e-mail, bancă, cont | — |
| Antet | **PRO RATA DE DEDUCERE %** (art. 300) | 5 poziţii |
| Subsol | Cod CAEN al activităţii preponderente efectiv desfăşurate în perioada de raportare | `caen`, listă închisă de valori (rev2+rev3) |
| Subsol | 4 întrebări Da/Nu — operaţiuni cu taxare inversă art. 331 a căror exigibilitate intervine în perioada de raportare: cereale şi plante tehnice · telefoane mobile · dispozitive cu circuite integrate · console de jocuri, tablete PC şi laptopuri | — |
| Subsol | „Solicitaţi rambursarea soldului sumei negative de TVA?" (bifa care decide dacă rd. 45 se reportează la rd. 41) | — |
| Casetă | **Facturi emise după inspecţia fiscală**, art. 330 alin. (3) | nr. facturi, total bază, total TVA |
| Casetă | **Facturi primite după inspecţia fiscală**, pct. 108 alin. (6) din Norme | nr. facturi, total bază, total TVA |
| Casetă | **Facturi emise după reînregistrarea în scopuri de TVA** pentru operaţiuni din perioada cu cod anulat, art. 11 alin. (6) şi (8) | nr. facturi, total bază, total TVA |
| **A** | Livrări de bunuri şi prestări de servicii realizate a căror TVA aferentă a rămas **neexigibilă**, existentă în sold la sfârşitul perioadei de raportare, ca urmare a aplicării **sistemului TVA la încasare**, **din care:** | Valoare + TVA |
| **A1** | …realizate în ultimele 6 luni / 2 trimestre calendaristice | Valoare + TVA; A ≥ A1 |
| **B** | Achiziţii de bunuri şi servicii realizate pentru care **nu s-a exercitat dreptul de deducere** a TVA aferentă, existentă în sold la sfârşitul perioadei de raportare, ca urmare a aplicării **art. 297 alin. (2) şi (3)**, **din care:** | Valoare + TVA |
| **B1** | …realizate în ultimele 6 luni / 2 trimestre calendaristice | Valoare + TVA; B ≥ B1 |
| Casetă | Valoarea totală, fără TVA, a operaţiunilor de la art. 278^1 alin. (1) lit. b) — vânzări intracomunitare la distanţă şi servicii TRE către neimpozabili din alte SM: **Total an precedent** / **An curent (inclusiv perioada de raportare)** | cumulat din bazele rd. 17 şi 18 |

---

## 3. Ce operaţiuni intră în fiecare rând de operaţiuni

Redat fidel după Anexa 2 (instrucţiunile de completare). Sursa datelor este,
pentru aproape toate rândurile, **jurnalul de vânzări / jurnalul de cumpărări**
(definite la nota de subsol: „orice jurnale, registre, evidenţe sau alte documente
similare" întocmite conform art. 321 CF şi pct. 101 din Norme) — un detaliu direct
relevant pentru proiecţia peste `RegistruTva`.

Criteriul temporal general: **exigibilitatea taxei intervine în perioada de
raportare**. Excepţiile explicite sunt notate mai jos.

### TVA colectată

**Rd. 1** — baza de impozitare pentru livrările intracomunitare de bunuri scutite
conform art. 294 alin. (2) lit. a) şi d), **şi** livrările intracomunitare cu cod T
efectuate în cadrul unei operaţiuni triunghiulare de cumpărătorul revânzător
(art. 276 alin. (5)); **inclusiv avansurile parţiale** facturate pentru aceste
livrări scutite; **plus** ajustările de bază art. 287 a căror exigibilitate cade în
perioadă (art. 282 alin. (9)).

**Rd. 2** — regularizări ulterioare datorate unor evenimente care modifică datele
declarate la rd. 1 **în altă perioadă**: modificarea preţului din alte cauze decât
art. 287, nedeclararea din eroare a livrării intracomunitare în perioada
exigibilităţii etc.

**Rd. 3** — baza pentru livrările/prestările **neimpozabile în România** fiindcă
locul livrării/prestării nu e în România (art. 275 şi 278), **plus** livrările
intracomunitare scutite conform art. 294 alin. (2) lit. b) şi c); inclusiv
avansurile parţiale pentru acele livrări intracomunitare scutite; plus ajustările
art. 287; plus **orice alte regularizări ulterioare**, *cu excepţia* celor privind
serviciile intracomunitare, care merg la rd. 4.

**Rd. 3.1** (din care) — baza pentru serviciile cu locul stabilit după art. 278
alin. (2), **altele decât cele scutite** în statul membru unde sunt impozabile,
prestate către persoane impozabile nestabilite în România dar stabilite în UE;
plus ajustările art. 287.

**Rd. 4** — regularizări ulterioare care modifică datele declarate la rd. **3.1**
în altă perioadă (aceleaşi tipuri de evenimente ca la rd. 2).

**Rd. 5** — bază + TVA pentru **achiziţiile intracomunitare de bunuri taxabile în
România**, plus baza pentru achiziţiile efectuate de beneficiarul unei livrări
ulterioare într-o operaţiune triunghiulară, pentru care acesta e obligat la plata
taxei (art. 307 alin. (4)); inclusiv **avansurile parţiale** plătite; plus
ajustările art. 287.

**Rd. 5.1** (din care) — achiziţiile intracomunitare la care furnizorul e
înregistrat în scopuri de TVA în statul membru de plecare. *(Sub-rând informativ,
exclus din totalul rd. 19.)*

**Rd. 6** — achiziţiile intracomunitare a căror exigibilitate a intervenit în altă
perioadă dar nu au fost declarate, plus regularizările achiziţiilor declarate
anterior. Instrucţiunile enumeră limitativ tipurile:
- modificarea preţului bunurilor declarate ca AIC pe bază de **autofactură**, când
  factura primită ulterior are alt preţ (cu excepţia cazului când modificarea vine
  dintr-un eveniment art. 287 din perioada de raportare);
- declararea AIC pe autofactură, iar la primirea facturii într-o perioadă
  ulterioară se constată că exigibilitatea era într-o perioadă anterioară;
- modificarea bazei şi a taxei ca urmare a **modificării cursului valutar** de
  referinţă, din neconcordanţe între data primirii facturii şi data declarării AIC;
- orice alte evenimente care modifică datele declarate iniţial, **cu excepţia
  ajustărilor art. 287**, când au loc într-o altă perioadă decât cea a
  exigibilităţii.

**Rd. 7** — bază + TVA pentru achiziţiile de bunuri şi servicii pentru care
**beneficiarul din România e obligat la plata TVA** conform art. 307 alin. (2)–(6),
**şi pentru importurile** cărora li se aplică art. 326 alin. (4) şi (5) (taxare
inversă la import / amânarea plăţii în vamă); plus ajustările art. 287 şi orice
alte regularizări ulterioare, *cu excepţia* celor privind serviciile
intracomunitare (→ rd. 8).

**Rd. 7.1** (din care) — achiziţiile de **servicii intracomunitare** cu beneficiar
obligat la plata TVA conform art. 307 alin. (2); plus ajustările art. 287.

**Rd. 8** — regularizări ulterioare care modifică datele declarate la rd. **7.1**
în altă perioadă.

**Rd. 9** — **cota 21%**. Bază + TVA colectată pentru livrările/prestările taxabile
cu 21%, **inclusiv livrările şi prestările asimilate**; plus baza şi TVA pentru
**operaţiunile supuse regimurilor speciale**, determinate pe baza situaţiilor de
calcul întocmite în acest scop; plus ajustările art. 287 (art. 282 alin. (9) şi
(10)) **dacă au fost generate de operaţiuni cu cota 21%**.

**Rd. 10** — **cota 11%**. Idem rd. 9, pentru livrările/prestările taxabile cu 11%,
inclusiv cele asimilate; plus ajustările art. 287 generate de operaţiuni cu 11%.
*(Textul instrucţiunilor nu repetă aici menţiunea regimurilor speciale.)*

**Rd. 11** — **cota 9% tranzitorie**. Bază + TVA pentru **livrările de bunuri**
taxabile cu 9% conform **art. III din Legea nr. 141/2025**, începând cu 1 august
2025; plus ajustările art. 287 generate de operaţiuni cu această cotă de 9%.

**Rd. 12** — bază + taxă **colectată** pentru achiziţiile de bunuri şi servicii ale
beneficiarilor care aplică **măsurile de simplificare de la art. 331** (taxare
inversă internă); plus ajustările art. 287 generate de operaţiuni cu 21% sau 11%.

**Rd. 12.1 / 12.2** (din care) — defalcarea rd. 12 pe cote: **12.1 = 21%** (bunuri
şi servicii), **12.2 = 11%** (numai bunuri). *(Sub-rânduri, excluse din rd. 19.)*

**Rd. 13** — baza pentru **livrările/prestările efectuate** de furnizorul/prestatorul
care aplică măsurile de simplificare art. 331 — cealaltă faţă a taxării inverse
interne, fără TVA colectată. Include operaţiuni a căror exigibilitate intervine în
perioada de raportare **sau în perioade fiscale anterioare**; plus ajustările
art. 287.

**Rd. 14** — baza pentru livrările/prestările **scutite cu drept de deducere**, cu
exigibilitate în perioada de raportare **sau în perioade anterioare**:
- art. 294 alin. (1) (exporturi şi asimilate), art. 295 (regimuri suspensive/antrepozit),
  art. 296 (intermediari);
- operaţiuni scutite conform **art. 292 alin. (2) lit. a) pct. 1–5 şi lit. b)**
  (financiar-bancare, asigurări) **când clientul e stabilit în afara UE** sau când
  operaţiunile sunt în legătură directă cu bunuri care vor fi exportate, precum şi
  operaţiunile intermediarilor care acţionează în numele şi contul altei persoane
  când intervin în astfel de operaţiuni;
- art. 294 alin. (5) (misiuni diplomatice, forţe armate, organisme internaţionale);
- operaţiuni scutite cu drept de deducere **potrivit altor dispoziţii legale**.

**Rd. 15** — baza pentru livrările **scutite fără drept de deducere**, prevăzute la
art. 292 sau potrivit altor dispoziţii legale.

**Rd. 16 — Regularizări taxă colectată.** Rândul-tampon al cotelor istorice.
Conţine:
- sumele rezultate din **corectarea informaţiilor de la rd. 9, 10, 11 şi 12 din
  deconturile anterioare**;
- **ajustările art. 287** cu exigibilitate în perioada de raportare (art. 282
  alin. (9) şi (10)) **generate de operaţiuni cu cota 24%, 20%, 19%, 9% sau 5%**;
- operaţiuni cu exigibilitate în perioada de raportare **dar pentru care se aplică
  cota 24%, 20%, 19%, 9% sau 5%**;
- sumele din **regularizările art. 291 alin. (6)** datorate modificării cotelor de TVA;
- orice alte sume din regularizări prevăzute de legislaţie datorate unor evenimente,
  **cu excepţia celor de la art. 287**, care modifică datele declarate iniţial
  (ex.: nedeclararea din eroare a operaţiunii în perioada exigibilităţii).

**Rd. 17** — bază + TVA colectată pentru **vânzările intracomunitare de bunuri la
distanţă** (art. 266 alin. (1) pct. 35) şi pentru serviciile de telecomunicaţii,
radiodifuziune, TV şi cele furnizate pe cale electronică, când bunurile sunt
expediate în alt stat membru / beneficiarul e persoană neimpozabilă din alt stat
membru, **iar locul livrării/prestării se consideră a fi în România** conform
art. 278^1 alin. (1); inclusiv avansurile parţiale.
Condiţia de loc: valoarea totală fără TVA a acestor operaţiuni **nu depăşeşte în
anul curent 46.337 lei** şi nu a depăşit-o nici în anul precedent, iar
furnizorul/prestatorul **nu a optat** ca locul să fie la beneficiar (art. 275
alin. (2) sau art. 278 alin. (5) lit. h)).

**Rd. 18** — regularizări ulterioare care modifică datele declarate la rd. 17 în
altă perioadă.

### TVA deductibilă

**Rd. 20, 20.1, 21, 22, 22.1, 23** — instrucţiunile spun literal „se înscriu
**aceleaşi informaţii** declarate la rândul 5 / 5.1 / 6 / 7 / 7.1 / 8". Sunt
rânduri-oglindă, validate ca egalitate strictă în schema XML (V_7…V_18). Pentru un
generator: **nu se recalculează, se copiază**.

**Rd. 24** — **cota 21%**. Bază + TVA deductibilă pentru achiziţiile **din ţară** de
bunuri şi servicii taxabile cu 21%, **altele decât cele înscrise la rd. 27**
(compensaţia forfetară agricolă); **plus baza şi taxa aferentă importurilor care NU
se încadrează în art. 326 alin. (4) şi (5)** (adică importurile cu TVA plătită
efectiv în vamă — cele cu taxare inversă la import stau la rd. 22); plus ajustările
art. 287 generate de operaţiuni cu 21%; **plus** achiziţiile taxabile cu 21%
efectuate de persoana impozabilă **beneficiară a unui regim special** în condiţiile
art. 314 alin. (12), art. 315 alin. (14) sau art. 315^2 alin. (24) (OSS/IOSS).

**Rd. 25** — **cota 11%**. Simetric rd. 24: achiziţii din ţară taxabile cu 11%,
taxa aferentă importurilor din afara art. 326 alin. (4)–(5), ajustările art. 287
pentru cota 11%, şi achiziţiile beneficiarilor de regim special.

> Observaţie de structură: **nu există rând de achiziţii cu cota 9% tranzitorie**
> în zona deductibilă. Simetria rd. 11 (colectat 9%) nu are pereche deductibilă —
> rd. 25.1, care exista în forma august–decembrie 2025, a fost eliminat.
> Vezi §6, punctul deschis (1).

**Rd. 26** — bază + taxă **deductibilă** pentru achiziţiile cu **măsuri de
simplificare art. 331**; plus ajustările art. 287 pentru cotele 21% sau 11%.
Instrucţiunile adaugă explicit: „Se înscriu aceleaşi informaţii declarate la
rândul 12." **Rd. 26.1 = rd. 12.1** (21%), **rd. 26.2 = rd. 12.2** (11%).

**Rd. 27** — sumele reprezentând **compensaţia în cotă forfetară achitată** pentru
achiziţii de produse şi servicii agricole de la furnizori care aplică regimul
special pentru agricultori, **în aceleaşi limite şi condiţii aplicabile pentru
deducerea TVA** (art. 297–301). Numai coloana TVA.

**Rd. 28** — sumele din **corectarea** compensaţiei în cotă forfetară înscrise la
rd. 27. Numai coloana TVA.

**Rd. 29** — contravaloarea (numai bază, fără TVA) a:
- achiziţiilor de bunuri şi servicii a căror livrare/prestare a fost **scutită de
  taxă, neimpozabilă sau supusă unui regim special de taxă**;
- achiziţiilor din alte state membre **pentru care nu se datorează TVA în România**;
- **achiziţiilor intracomunitare scutite de taxă sau neimpozabile în România**;
- achiziţiilor de **servicii intracomunitare scutite** de TVA;
- **importurilor scutite** de TVA.

> „Nu este obligatorie înscrierea în decont a sumelor care nu sunt incluse în baza
> impozabilă." — clauză utilă: rd. 29 nu trebuie să fie o oglindă exhaustivă a
> tuturor cheltuielilor fără TVA.

**Rd. 29.1** (din care) — achiziţiile de **servicii intracomunitare scutite** de TVA.

**Rd. 31 — SUB-TOTAL TAXĂ DEDUSĂ.** Rândul unde se decide efectiv cât se deduce.
Conţine TVA **efectiv dedusă** pentru achiziţiile de la rd. 20–26, plus compensaţia
forfetară de la rd. 27 şi 28:
- **persoane cu drept integral de deducere** (art. 297 şi 298): tot, minus taxa
  pentru care **nu se permite** exercitarea dreptului de deducere conform art. 298;
- **persoane cu regim mixt** (art. 298 şi 300), în funcţie de destinaţia achiziţiilor:
  - taxa aferentă achiziţiilor destinate operaţiunilor **cu** drept de deducere
    (mai puţin cele interzise de art. 298);
  - taxa dedusă **conform pro rata** pentru achiziţiile cu destinaţie mixtă (mai
    puţin cele interzise de art. 298);
  - taxa aferentă achiziţiilor destinate **operaţiunilor fără drept de deducere
    NU se preia în acest rând**;
  - compensaţia forfetară dedusă potrivit art. 315^1 alin. (17), plus regularizările
    ei.

Restricţia explicită din instrucţiuni: „Totalul de la rândul 31 poate fi **mai mic
sau egal** cu totalul de la rândurile 20–28, cu excepţia celor de la rândurile
20.1, 22.1, 26.1 şi 26.2" — adică **rd. 31 ≤ rd. 30**, validat blocant (V_6).

**Rd. 32** — TVA **efectiv restituită** în baza art. 294 alin. (1) lit. b)
cumpărătorilor persoane fizice **nestabilite în UE** (tax-free), de către
persoanele impozabile autorizate, **inclusiv comisionul** perceput pentru
activitatea de restituire.

**Rd. 33 — Regularizări taxă dedusă.** Simetricul rd. 16 pe partea deductibilă:
- sumele din **corectarea taxei deduse** (art. 297 şi 298, respectiv 298 şi 300
  pentru regim mixt) aferentă operaţiunilor înscrise la **rd. 24, 25, 26 şi 27 din
  deconturile anterioare**;
- **ajustările art. 287** cu exigibilitate ulterioară datei de **1 ianuarie 2017**,
  generate de operaţiuni cu cota **24%, 20%, 19%, 9% sau 5%** (art. 282 alin. (9) şi (10));
- operaţiuni cu exigibilitate în perioada de raportare **dar taxate cu 24/20/19/9/5%**;
- sumele din **regularizările art. 291 alin. (6)** datorate modificării cotelor;
- orice alte sume din regularizări legale datorate unor evenimente, **cu excepţia
  celor de la art. 287 şi art. 315^1**, care modifică datele declarate iniţial
  (ex.: nedeclararea din eroare).

**Rd. 34 — Ajustări conform pro-rata / ajustări de taxă.** Conţine:
- diferenţele rezultate din **ajustarea anuală pe bază de pro rata definitivă**
  (art. 300);
- diferenţele din **regularizarea erorilor** constatate ulterior în calculul pro
  rata definitive (pct. 70 alin. (5) din Norme);
- diferenţele rezultate din **ajustarea taxei deductibile**, **cu semnul plus sau
  minus**, după caz (ajustările de bunuri de capital / schimbare de destinaţie).

### Regularizări art. 303

**Rd. 38** — se preia suma de la **rd. 44 din decontul precedent**, minus sumele
achitate până la data depunerii. Excepţii explicite: **nu se preia** taxa cumulată
de plată pentru care s-a aprobat o **înlesnire la plată**, respectiv cea cu care
organul fiscal s-a înscris la **masa credală** (Legea 85/2014). La **fuziune**,
succesorul preia soldul taxei de plată; la **divizare**, proporţional cu cotele
alocate.

**Rd. 39** — diferenţa de TVA de plată stabilită prin decizie comunicată, **nestinsă**
la data depunerii. **Nu se preiau** sumele care la data depunerii **nu sunt
considerate restante** (art. 157 alin. (2) CPF); ele intră în decontul perioadei în
care devin restante.

**Rd. 41** — se preia suma de la **rd. 45 din decontul precedent**, pentru care
**nu s-a solicitat rambursarea** (prin bifa corespunzătoare din decontul anterior).
Excepţie: soldul negativ din decontul anterior datei deschiderii **procedurii
insolvenţei** nu se preia — debitorul e obligat să ceară rambursarea prin
corectarea decontului perioadei anterioare. Fuziune/divizare: se preia analog rd. 38.

**Rd. 42** — diferenţa negativă stabilită de inspecţia fiscală prin decizie
comunicată până la data depunerii. **Nu se preiau** diferenţele din decizii a căror
executare a fost **suspendată de instanţă**, pe durata suspendării; ele intră în
decontul perioadei în care a încetat suspendarea.

### Ce NU se înscrie în decont (secţiunea „ATENŢIE!" din instrucţiuni)

- TVA din **facturile de executare silită** emise de persoanele abilitate prin lege
  să vândă bunurile supuse executării silite;
- taxa cumulată de plată pentru care a fost aprobată o **înlesnire la plată**;
- taxa cumulată de plată cu care organul fiscal **s-a înscris la masa credală**
  (Legea 85/2014);
- soldul sumei negative din decontul aferent perioadei **anterioare deschiderii
  procedurii insolvenţei**.

Şi, transversal: **„Nu se admit întocmirea şi depunerea de deconturi rectificative
pentru corectarea datelor din deconturile anterioare."** Corecţia se face exclusiv
prin rândurile de regularizare din decontul curent (16, 33 şi cele specifice
2/4/6/8/18/21/23/28), sau prin procedura separată de corectare a erorilor materiale
(OPANAF 3604/2015). **Aceasta e regula care determină designul: D300 e o proiecţie
a perioadei curente, nu un snapshot rescriabil retroactiv.**

---

## 4. Notă cerută: operaţiuni fără TVA deductibilă; stornări şi ajustări de bază

### 4.1 Unde apar operaţiunile fără drept de deducere / TVA nedeductibilă

Nu există un rând „TVA nedeductibilă". Nedeductibilul se manifestă în **patru
locuri distincte**, cu semantici diferite:

1. **Diferenţa rd. 30 − rd. 31.** Achiziţia taxabilă intră **integral** (bază + TVA)
   în rd. 24/25/26 şi deci în totalul rd. 30 — indiferent dacă TVA e deductibilă sau
   nu. Ce nu se deduce pur şi simplu **nu se preia** în rd. 31. Diferenţa
   rd. 30 − rd. 31 **este** TVA nededusă a perioadei: taxa interzisă de art. 298
   (ex. băuturi alcoolice şi produse din tutun; vehicule cu limitarea de 50%), plus
   partea nedeductibilă la pro rata, plus taxa aferentă achiziţiilor destinate
   exclusiv operaţiunilor fără drept de deducere. Formularul **nu cere defalcarea**
   acestei diferenţe pe cauze.
2. **Rd. 29** — achiziţiile **fără TVA din start**: scutite, neimpozabile, regim
   special, AIC scutite, importuri scutite. Doar bază, fără coloană TVA. A nu se
   confunda cu (1): aici nu există TVA de dedus, în (1) există şi nu se deduce.
3. **Rd. 15** — pe partea de ieşiri: livrările **scutite fără drept de deducere**
   (art. 292). Ele nu produc TVA colectată, dar sunt numitorul pro ratei şi cauza
   nedeductibilităţii din (1).
4. **Secţiunea informativă B / B1** — achiziţii pentru care **nu s-a exercitat încă**
   dreptul de deducere conform art. 297 alin. (2) şi (3) (TVA neexigibilă la
   cumpărare, tipic contrapartida sistemului TVA la încasare), rămase în sold la
   sfârşitul perioadei şi care **vor deveni deductibile ulterior**. Este o amânare,
   nu o pierdere — spre deosebire de (1).

Simetric, pe partea colectată, **secţiunea A / A1** ţine TVA rămasă **neexigibilă**
la vânzare din aplicarea **sistemului TVA la încasare** (art. 282), în sold la
sfârşitul perioadei. A şi B sunt **solduri cumulate** (includ operaţiuni din
perioade anterioare), nu fluxuri ale perioadei — singurele poziţii din decont cu
această semantică, alături de rd. 38/39/41/42.

### 4.2 Unde se raportează stornările şi ajustările de bază

Regula de partaj a instrucţiunilor este **pe cauză juridică**, nu pe semn:

| Situaţie | Unde intră |
|---|---|
| **Ajustări art. 287** (anulare totală/parţială, refuz de calitate/preţ/cantitate, reduceri de preţ ulterioare, contravaloare neîncasată — faliment) generate de operaţiuni cu **cotele curente** (21%, 11%, 9% tranzitoriu) | **În rândul operaţiunii originale**, pe cotă: rd. 9 / 10 / 11 (colectat), rd. 24 / 25 (dedus), rd. 1 / 3 / 3.1 / 5 / 7 / 12 / 13 / 14 / 17 pentru categoriile respective — **în perioada în care intervine exigibilitatea ajustării** (art. 282 alin. (9) şi (10)) |
| **Ajustări art. 287** generate de operaţiuni cu **cote istorice** (24%, 20%, 19%, 9%, 5%) | **rd. 16** (colectat) / **rd. 33** (dedus) |
| Operaţiuni cu exigibilitate acum, dar **taxate cu o cotă istorică** | **rd. 16** / **rd. 33** |
| **Regularizări art. 291 alin. (6)** — datorate modificării cotelor de TVA (tipic: facturarea la o cotă, faptul generator la alta, în jurul lui 01.08.2025) | **rd. 16** / **rd. 33** |
| Corectarea unei erori de declarare (nedeclarare din eroare, preţ modificat din **alte** cauze decât art. 287) pe **operaţiuni interne** | **rd. 16** / **rd. 33** |
| Idem, pe **livrări intracomunitare scutite** | **rd. 2** |
| Idem, pe **servicii intracomunitare prestate** (rd. 3.1) | **rd. 4** |
| Idem, pe **achiziţii intracomunitare de bunuri** | **rd. 6** (şi oglinda **rd. 21**) |
| Idem, pe **servicii intracomunitare primite** (rd. 7.1) | **rd. 8** (şi oglinda **rd. 23**) |
| Idem, pe **vânzări la distanţă / servicii electronice** (rd. 17) | **rd. 18** |
| Corectarea **compensaţiei forfetare agricole** | **rd. 28** |
| **Ajustarea pro rata definitivă** anuală + erori în calculul pro rata + **ajustări de taxă deductibilă** (bunuri de capital, schimbare de destinaţie) | **rd. 34**, cu semnul **plus sau minus** |

Consecinţe pentru implementare, care merită subliniate:

- **Semnul.** Rândurile de regularizare (2, 4, 6, 8, 16, 18, 28, 33) şi rd. 34
  admit valori negative — sunt corecţii, nu fluxuri. Rândurile de operaţiuni admit
  şi ele valori negative când ajustarea art. 287 depăşeşte operaţiunile perioadei.
  Validările de cotă din schema XML sunt formulate ca inegalităţi `Round(20%*bază)
  ≤ TVA ≤ Round(22%*bază)` — se comportă corect pe negative doar dacă ambele
  componente au acelaşi semn, ceea ce impune ca **baza şi TVA ale unei stornări să
  fie ambele negative**.
- **Ruta unei stornări depinde de cota documentului stornat, nu de data stornării.**
  Un storno în 2026 al unei facturi din 2024 cu 19% merge la rd. 16, nu la rd. 9.
  Asta cere ca sursa (`RegistruTva`) să păstreze cota originală pe rândul de storno
  — nu doar sensul şi valoarea. Se leagă direct de decizia 68 („`Storno` în cheie")
  şi de 46a („storno = valori NEGATIVE pe corespondenţa ORIGINALĂ").
- **Pragul temporal pentru rd. 33** e explicit: ajustările art. 287 cu exigibilitate
  **ulterioară datei de 1 ianuarie 2017**. Anterioare — instrucţiunile tac.

---

## 5. Maparea XML (pentru generatorul de fişier)

Schema: `xmlns="mfp:anaf:dgti:d300:declaratie:v12"`, `xsi:schemaLocation="… D300.xsd"`,
`UniversalCode = D300_A10.0.0`. Toate câmpurile de sume sunt `N(15)`, în **lei
întregi**.

**Capcana centrală**: numele elementelor XML au fost îngheţate la o numerotare
istorică şi **nu urmăresc numerele de rând tipărite**. Sufixul `_1` = coloana
Valoare/bază, `_2` = coloana TVA.

| Rd. tipărit 2026 | Element bază (col.1) | Element TVA (col.2) |
|---|---|---|
| 1 | `R1_1` | — |
| 2 | `R2_1` | — |
| 3 | `R3_1` | — |
| 3.1 | `R3_1_1` | — |
| 4 | `R4_1` | — |
| 5 | `R5_1` | `R5_2` |
| 5.1 | `R5_1_1` | `R5_1_2` |
| 6 | `R6_1` | `R6_2` |
| 7 | `R7_1` | `R7_2` |
| 7.1 | `R7_1_1` | `R7_1_2` |
| 8 | `R8_1` | `R8_2` |
| 9 (21%) | `R9_1` | `R9_2` |
| 10 (11%) | `R10_1` | `R10_2` |
| 11 (9% tranz.) | `R11_1` | `R11_2` |
| 12 | `R12_1` | `R12_2` |
| 12.1 (21%) | `R12_1_1` | `R12_1_2` |
| 12.2 (11%) | `R12_2_1` | `R12_2_2` |
| 13 | `R13_1` | — |
| 14 | `R14_1` | — |
| 15 | `R15_1` | — |
| 16 | `R16_1` | `R16_2` |
| 17 | `R64_1` | `R64_2` |
| 18 | `R65_1` | `R65_2` |
| **19** (TOTAL colectat) | `R17_1` | `R17_2` |
| 20 | `R18_1` | `R18_2` |
| 20.1 | `R18_1_1` | `R18_1_2` |
| 21 | `R19_1` | `R19_2` |
| 22 | `R20_1` | `R20_2` |
| 22.1 | `R20_1_1` | `R20_1_2` |
| 23 | `R21_1` | `R21_2` |
| 24 (21%) | `R22_1` | `R22_2` |
| 25 (11%) | `R23_1` | `R23_2` |
| 26 | `R25_1` | `R25_2` |
| 26.1 (21%) | `R25_1_1` | `R25_1_2` |
| 26.2 (11%) | `R25_2_1` | `R25_2_2` |
| 27 (compensaţie forfetară) | — | `R43_2` |
| 28 (regularizări compensaţie) | — | `R44_2` |
| 29 | `R26_1` | — |
| 29.1 | `R26_1_1` | — |
| **30** (TOTAL deductibil) | `R27_1` | `R27_2` |
| 31 (SUB-TOTAL dedus) | — | `R28_2` |
| 32 | — | `R29_2` |
| 33 | `R30_1` | `R30_2` |
| 34 | — | `R31_2` |
| **35** (TOTAL dedus) | — | `R32_2` |
| **36** | — | `R33_2` |
| **37** | — | `R34_2` |
| 38 | — | `R35_2` |
| 39 | — | `R36_2` |
| **40** | — | `R37_2` |
| 41 | — | `R38_2` |
| 42 | — | `R39_2` |
| **43** | — | `R40_2` |
| **44** | — | `R41_2` |
| **45** | — | `R42_2` |

Câmpuri non-rând: `luna`, `an`, `tip_decont`, `depusReprezentant`, `bifa_interne`,
`temei`, `cuiSuccesor`, `nume_declar`, `prenume_declar`, `functie_declar`, `cui`,
`den`, `adresa`, `telefon`, `fax`, `mail`, `banca`, `cont`, `caen`, `totalPlata_A`
(sumă de control), `nr_facturi`/`baza`/`tva` (emise după inspecţie),
`nr_facturi_primite`/`baza_primite`/`tva_primite`, `nr_fact_emise`/`total_baza`/`total_tva`
(după reînregistrare), `valoare_a`/`tva_a`/`valoare_a1`/`tva_a1`,
`valoare_b`/`tva_b`/`valoare_b1`/`tva_b1`, `total_precedent`, `total_curent`.

### Validări blocante (ERR) şi de avertizare (ATT)

**Egalităţi oglindă (ERR)** — V_7…V_24:
`rd. 20 = rd. 5` · `rd. 20.1 = rd. 5.1` · `rd. 21 = rd. 6` · `rd. 22 = rd. 7` ·
`rd. 22.1 = rd. 7.1` · `rd. 23 = rd. 8` · `rd. 26 = rd. 12` · `rd. 26.1 = rd. 12.1` ·
`rd. 26.2 = rd. 12.2` — pe **ambele** coloane.

**Ierarhie (ERR)**: `rd. 12 ≥ rd. 12.1 + rd. 12.2` · `rd. 26 ≥ rd. 26.1 + rd. 26.2` ·
`rd. 31 ≤ rd. 30` (col. TVA).

**Totaluri (ERR)**: rd. 19, 30, 35, 36, 37, 40, 43, 44, 45 — calculate, obligatorii.

**Decont simplificat (ERR, V_1)**: dacă `bifa_interne = 1` (numai operaţiuni în
interiorul ţării, art. 5 din ordin), atunci rd. **1, 2, 3, 3.1, 4, 5, 5.1, 6, 7,
7.1, 8** şi oglinzile lor **20, 20.1, 21, 22, 22.1, 23** trebuie să fie **nule**
(plus `R26_1_1`, adică rd. 29.1).

**Marje de cotă (ATT — avertizare, nu blocaj)**, cu toleranţă ±1 punct procentual:

| Rd. | Interval acceptat pentru TVA |
|---|---|
| 9, 12.1, 24 | `Round(20% × bază)` … `Round(22% × bază)` |
| 10, 12.2, 25, 26.2 | `Round(10% × bază)` … `Round(12% × bază)` |
| 11 | `Round(8% × bază)` … `Round(10% × bază)` |
| 26.1 | `Round(20% × bază)` … `Round(21% × bază)` ← **vezi §6, punct deschis (2)** |

`Round(...)` e la **0 zecimale** — decontul se depune în lei întregi. Rotunjirea
de la nivelul documentului către lei întregi la nivel de rând de decont e o
decizie proprie (decizia 51c: convenţia de rotunjire e dată de profil şi
îngheţată per bază).

---

## 6. Surse, grad de încredere şi ce a rămas incert

### Surse

| # | Sursă | Ce acoperă | Încredere |
|---|---|---|---|
| 1 | `https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_174_2026.pdf` — ORDINUL nr. 174/2026 din 5 februarie 2026, text integral + Anexele 1, 2, 3 (15 pag.) | §2 (structura rândurilor, denumiri oficiale, formule tipărite), §3 (instrucţiuni de completare, integral), §4 | **Oficial, primar.** Descărcat şi citit integral. Denumirile rândurilor din §2 sunt transcrise verbatim din Anexa 1; conţinutul din §3 e parafrazat fidel din Anexa 2, cu citate acolo unde formularea contează. |
| 2 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/structura_D300_v12.0.0_10022026.pdf` — Structura fişierului XML pentru declaraţia 300, v12.0.0 din **10.02.2026** (28 pag.) | §1 (lista rândurilor eliminate), §2 (maparea coloanelor B/T per rând), §5 (nume elemente, validări, marje de cotă) | **Oficial, primar.** Versiunea e datată la o zi după publicarea ordinului în M.Of. şi conţine explicit modificările marcate `01.01.26`. |
| 3 | `https://static.anaf.ro/static/3/Cluj/20260220114126_cj_d300_20feb2026.pdf` — comunicat DGRFP Cluj-Napoca, nr. CJR_DEC_3226/19.02.2026 | Confirmarea datei M.Of. (105/09.02.2026), termenele de depunere, cota 9% pentru locuinţe până la 31.07.2026 | **Oficial, secundar** (comunicat de presă ANAF, nu act normativ). Folosit doar ca confirmare. |
| 4 | `https://legislatie.just.ro/Public/DetaliiDocument/307258` — Portal Legislativ, ORDIN 174 05/02/2026 | Existenţa şi identificarea actului | **Oficial**; conţinutul nu a fost extras de aici (sursa 1 l-a acoperit integral). |

Surse secundare identificate dar **neutilizate** pentru conţinut (infotva.manager.ro,
universuljuridic.ro, startupcafe.ro, smarttax.ro, permisdeantreprenor.ro,
evz.ro): sursele primare au acoperit totul. `universuljuridic.ro` a răspuns HTTP 403.

**Nicio afirmaţie din §2–§5 nu provine dintr-o sursă secundară.**

### Grad de încredere pe secţiune

| Secţiune | Încredere | Motiv |
|---|---|---|
| §2 — numerotarea, denumirile, secţiunile | **Foarte ridicată** | Verbatim din Anexa 1. |
| §2 — maparea coloanelor Bază/TVA | **Ridicată** | Derivată din prezenţa/absenţa elementelor `_1`/`_2` în schema XML oficială — criteriu mecanic, nu interpretare vizuală a PDF-ului. |
| §2 — formulele de total | **Foarte ridicată** | Dublu confirmate: textul tipărit din Anexa 1 + formulele din schema XML. |
| §3 — conţinutul rândurilor | **Foarte ridicată** | Parafrază fidelă a Anexei 2, citită integral. |
| §4.1 — unde apare nedeductibilul | **Ridicată** | Sinteză proprie peste textul oficial al rd. 30/31/29/15 şi al secţiunii B; concluzia „rd. 30 − rd. 31 = TVA nededusă" e derivată logic, nu citată ca atare. |
| §4.2 — ruta stornărilor | **Ridicată** | Partajul art. 287 vs. non-287 e explicit în instrucţiuni pentru fiecare rând. Observaţiile despre semn şi implementare sunt inferenţe proprii, marcate ca atare. |
| §5 — maparea XML | **Ridicată, cu o rezervă** | Extrasă din tabelul oficial; documentul afişează istoric numerele de rând (vechi tăiate, noi adăugate) şi extragerea a cerut interpretare. Recomand verificarea pe un XML generat de programul de asistenţă ANAF înainte de a o considera contract. |

### Ce a rămas incert — 3 puncte deschise

1. **Nu există rând de achiziţii cu cota 9% tranzitorie.** Formularul are rd. 11
   pentru livrări cu 9% (art. III din Legea 141/2025), dar zona deductibilă nu are
   pereche — rd. 25.1 (`R75_*`, „Achiziţii de bunuri, taxabile cu cota 9%") a fost
   eliminat de la 01.01.2026. Instrucţiunile nu spun unde declară cumpărătorul o
   achiziţie de locuinţă cu 9% în 2026. Lecturi posibile: (a) intenţionat — cota 9%
   tranzitorie vizează livrări către **persoane fizice**, care nu depun D300, deci
   cazul nu apare; (b) se declară la rd. 24 sau 25. **Interpretarea (a) e cea mai
   probabilă şi coerentă cu art. III**, dar nu e confirmată de un text oficial.
2. **Marja de cotă la rd. 26.1 pare o eroare în documentaţia ANAF.** Limita
   superioară e `Round(21% × bază)`, în timp ce rândurile omoloage (9, 12.1, 24)
   au `Round(22% × bază)`. Fiind o validare **ATT** (avertizare), nu blochează
   depunerea. Nu am găsit erată.
3. **Aspectul vizual exact al casetei rd. 17–18** (dacă are antet de coloane
   propriu, separat de restul secţiunii colectate) — extragerea text a PDF-ului
   sugerează o casetă cu „Valoare / TVA" propriu, dar nu am validat-o pe formularul
   randat. Fără impact asupra datelor: coloanele şi elementele XML sunt certe.

Neverificat, deliberat în afara scopului: lista completă a codurilor CAEN acceptate
(există în sursa 2, ~400 valori) şi structura `totalPlata_A` (suma de control,
poziţii 1–23, formulă documentată în sursa 2).

### Fişiere sursă (local, gitignored: `anaf/d300/`)

- `OPANAF_174_2026.pdf` + `opanaf174.txt` — ordinul, text integral extras
- `structura_D300_v12.pdf` + `structura_D300_v12.txt` — schema XML oficială v12.0.0
