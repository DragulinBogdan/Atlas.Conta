# D394 — Declarația informativă 394, structura oficială 2026 (OPANAF 3769/2015, modificat prin OPANAF 2194/2025)

> **Actul de bază**: ORDINUL ANAF nr. 3.769/2015 din 23 decembrie 2015 privind
> declararea livrărilor/prestărilor și achizițiilor efectuate pe teritoriul național
> de persoanele înregistrate în scopuri de TVA și pentru aprobarea modelului și
> conținutului formularului (394), cod MFP 14.13.01.02/f — M.Of. nr. 11/07.01.2016.
> **Modificat prin**: OPANAF 1.105/2016 (art. 11), OPANAF 2.264/2016 (art. 10),
> OPANAF 2.328/2016 (art. 8 — termenul 30), **OPANAF 2.194/2025 din 11 septembrie
> 2025** (M.Of. nr. 852/17.09.2025) — înlocuiește integral **Anexa 1** (modelul
> formularului) și **Anexa 2** (instrucțiunile de completare).
> **Intrare în vigoare** (art. III din OPANAF 2194/2025): „se aplică începând cu
> operațiunile derulate de la data de **1.08.2025**".
> **Anexele 3 și 4** ale ordinului din 2015 (procedura de gestionare; caracteristici
> de tipărire) NU au fost modificate și nu privesc conținutul fișierului.
>
> **Nu există un ordin din 2026 pentru formularul 394.** Verificat pe
> legislatie.just.ro (lista actelor modificatoare, ultima consolidare 17.09.2025), pe
> pagina ANAF a formularului (ultimul pachet publicat = septembrie 2025) și prin
> căutări web la 2026-08-25. Spre deosebire de D300 (unde OPANAF 174/2026 a eliminat
> rândurile cotelor vechi), **D394 a fost extins în 2025 cu cotele 21%/11% fără să
> elimine cotele istorice** — vezi §1.
>
> **Schema XML în vigoare**: `d394_20250917.xml` (XSD, `targetNamespace =
> "mfp:anaf:dgti:d394:declaratie:v5"`, atribut `version="1.02"`), publicat 17.09.2025,
> cu anexa de validări `structD394_15092025.pdf` (marcaje de modificare `01.09.25`).
> Programul de asistență (PDF inteligent) curent: `D394_26092025.pdf`; soft J:
> `D394_17092025.zip`.
>
> **Sursă primară, integrală**: ordinul 2194/2025 cu ambele anexe (25 pag., text
> extras integral) + ordinul 3769/2015 (corpul, art. 1–12) + XSD-ul + anexa de
> validări au fost descărcate din `static.anaf.ro` și citite în întregime. Nimic
> din §2–§6 nu e reconstituit din surse secundare; ce nu s-a putut confirma e în §8.

---

## 1. Ce s-a schimbat prin OPANAF 2194/2025 (cotele 21%/11%)

Formularul 394 a fost adaptat prin **adăugarea** cotelor 21% și 11% la fiecare
defalcare pe cote, **păstrând** toate cotele istorice (24%, 20%, 19%, 9%, 5%).
Nota de subsol repetată pe toate cartușele spune: „TVA defalcată pe cote: 24%,
21%, 20%, 19%, 11%, 9%, 5%". Excepție: cartușul G (încasări AMEF) enumeră doar
„21%, 19%, 11%, 9%, 5%" — fără 24%/20% — deși schema XML `op2` păstrează și
câmpurile `baza20`/`TVA20`.

Elemente XML **adăugate** la 01.09.2025 (marcate `01.09.25` în anexa de validări):

| Zonă | Atribute noi | Observație |
|---|---|---|
| enumerarea `cota` (`Int_coteTVASType`, `Int_coteTVA2SType`) | valorile `21`, `11` | `0` rămâne permis doar pe tipurile fără TVA (§4.6) |
| `<informatii>` — I.4/I.5 | `tvaDed21`, `tvaDed11`, `tvaDedAI21`, `tvaDedAI11`, `tvaCol21`, `tvaCol11` | pe lângă 24/20/19/9/5 |
| `<informatii>` — I.3 (bife rambursare) | `achizitiiB21`, `achizitiiB11`, `achizitiiS21`, `achizitiiS11`, `BUN21`, `BUN11`, `Prest21`, `Prest11` | pe lângă variantele 24/20/19/9/5 |
| `<rezumat2>` (cartuș H + I.1) | apariție nouă cu `cota=21` / `cota=11` | structura rândului neschimbată |
| `<facturi>` (I.2.2 autofacturare) | `baza21`, `baza11`, `tva21`, `tva11` | |
| `<op2>` (cartuș G) | `baza21`, `TVA21`, `baza11`, `TVA11` (+ validările `baza21>=TVA21`, `baza11>=TVA11`) | |

**Nu s-a schimbat**: lista tipurilor de operațiune, tipurile de partener, structura
`op1`/`op11`, nomenclatoarele de bunuri cu taxare inversă (ultimul cod adăugat: 36
„Gaze naturale", v. A6.0.0/10.05.2022), lista CAEN de la I.7, regulile de
unicitate și sumele de control.

**Cota 9% tranzitorie** (locuințe, art. III din Legea 141/2025, până la 31.07.2026):
formularul n-are un rând distinct — 9% e pur și simplu una din cotele enumerate,
folosită ca atare pe `op1@cota`, `rezumat1@cota`, `rezumat2@cota`. Nu e nevoie de
nicio distincție „9% vechi / 9% tranzitoriu" în D394.

**Cotele istorice rămân operaționale**: o factură de storno emisă în 2026 pentru o
livrare din 2024 cu 19% se declară cu `cota=19`, cu bază și TVA negative (§5.3).
De aici decurge că generatorul are nevoie de **cota originală pe rândul de
storno** — aceeași cerință ca la D300 (decizia 68, „`Storno` în cheie").

---

## 2. Cine depune, perioada, termenul, corecția

Din Anexa 2 (instrucțiuni), pct. 1–5, și din corpul ordinului (art. 1, 7, 8):

**Cine** (pct. 1):
- **a)** persoanele înregistrate în scopuri de TVA conform art. 316 CF, pentru
  **livrările/prestările taxabile în România** la care persoana obligată la plata
  taxei este furnizorul (art. 307 alin. (1) sau (7)) **sau beneficiarul conform
  art. 331** (taxare inversă internă). Se declară orice operațiune taxabilă pentru
  care e emisă o factură, **inclusiv avansurile**, și operațiunile cu TVA la
  încasare. Declarația conține **facturile emise în perioada de raportare**,
  inclusiv cele cu mențiunea „taxare inversă" sau „TVA la încasare", **indiferent
  de data exigibilității**. Se declară în plus: autofacturile (I.2), valoarea
  totală a bonurilor fiscale (inclusiv cele care îndeplinesc condițiile unei
  facturi simplificate, cu sau fără CUI-ul beneficiarului), valoarea totală a
  documentelor emise pentru livrări fără obligația facturii sau a bonului fiscal;
- **b)** aceleași persoane, pentru **achizițiile taxabile cu locul în România**
  (art. 275/278), inclusiv cele cu beneficiar obligat la plată (art. 307 alin. (2),
  (3), (5), (6) și art. 331), indiferent de data exigibilității. **Nu se înscriu
  achizițiile intracomunitare care merg în D390.** Declarația conține **facturile
  primite în perioada de raportare**, plus borderourile de achiziții, filele din
  carnetele de comercializare a produselor agricole (achiziții de la persoane
  fizice), contractele cu persoane fizice și alte documente. Facturile simplificate
  și bonurile fiscale-facturi simplificate primite se declară **doar dacă au
  înscris CUI-ul beneficiarului**.

**Perioada** (art. 7; secțiunea 1 lit. a–c): perioada fiscală a decontului D300 —
`tip_D394 ∈ {L, T, S, A}`; `luna` = numărul perioadei (01–12 pentru lună, 03/06/09/12
pentru trimestru, 06/12 pentru semestru, 12 pentru an). Anexa de validări
adaugă cazul plătitorului trimestrial care face achiziții intracomunitare în
lunile 02/05/08/11: pentru acele luni se depune cu `tip_D394=T` cumulat (01+02,
04+05, 07+08, 10+11), iar următoarea perioadă devine lunară.

**Termen** (pct. 2; art. 8 modificat prin OPANAF 2328/2016): **până la data de 30
inclusiv a lunii următoare** perioadei de raportare, **inclusiv dacă nu au fost
operațiuni** (atunci `op_efectuate=0`). Pentru luna ianuarie: 28/29 februarie.

**Corecția** (pct. 3): omisiunile/erorile se corectează prin **redepunerea unei
declarații complete care o înlocuiește pe cea inițială** — nu există „rectificativă"
diferențială. **Nu se redepune** pentru facturile primite în altă perioadă decât
cea a emiterii lor de către furnizor (ele se declară în perioada primirii).
Design: D394 e un **snapshot integral rescriabil al perioadei**, opusul D300
(unde corecția merge doar în rândurile de regularizare ale perioadei curente).

**Formatul** (pct. 4–5): PDF cu XML atașat, semnat cu certificat calificat, prin
portalul e-România; alternativ format electronic + prima pagină tipărită la
registratură. Validarea cu programele de asistență ANAF.

---

## 3. Harta secțiunilor ↔ elementele XML

Rădăcina: `<declaratie394 xmlns="mfp:anaf:dgti:d394:declaratie:v5">`. Toate sumele
sunt `N(15)`, în **lei întregi** (`IntNeg15SType` = ±999.999.999.999.999 pentru
baze/TVA, `IntPoz15SType` ≥ 0 pentru numere de facturi/persoane). Ordinea
copiilor e fixă în XSD (`xs:sequence`): `informatii`, `rezumat1*`, `rezumat2*`,
`serieFacturi*`, `lista*`, `facturi*`, `op1*`, `op2*`.

| Cartuș tipărit | Element XML | Cardinalitate | Conținut |
|---|---|---|---|
| Antet: tip plătitor, perioada, sistem TVA, bifele „operațiuni?" / „afiliați?" | atribute pe `declaratie394` | 1 | §4.1 |
| A. Identificarea persoanei | atribute pe `declaratie394` | 1 | §4.1 |
| B. Reprezentant fiscal/legal/împuternicit | atribute `*R` pe `declaratie394` | 0–1 (`cifR` opțional) | §4.1 |
| C. Rezumat — parteneri înregistrați în RO (tip 1) | `rezumat1[@tip_partener=1]` + `detaliu` | 0–n (unic pe `tip_partener × cota`) | §4.2 |
| D. Rezumat — persoane neînregistrate (tip 2) | `rezumat1[@tip_partener=2]` + `detaliu` | 0–n (unic pe `tip_partener × cota × tip_N × document_N`) | §4.2 |
| E. Rezumat — nestabiliți în RO, stabiliți în UE (tip 3) | `rezumat1[@tip_partener=3]` | 0–n | §4.2 |
| F. Rezumat — nestabiliți în UE (tip 4) | `rezumat1[@tip_partener=4]` | 0–n | §4.2 |
| G. Încasări AMEF (Î1) / exceptate (Î2) | `op2` + `informatii@nr_BF_i1/incasari_i1/incasari_i2` + `rezumat2@*_incasari_*` | `op2`: 0–n (unic pe `tip_op2 × luna`) | §4.4 |
| H. Rezumat pe cote (suma de control) | `rezumat2` | 0–5 în XSD (vezi §8) | §4.3 |
| I.1 Facturi simplificate / bonuri cu CUI | atribute pe `rezumat2` | — | §4.3 |
| I.2 Plaja de facturi, emise, stornate, anulate, autofacturi, emise de beneficiari/terți | `serieFacturi` + `facturi` + `informatii@nrFacturi*` | 0–n | §4.5 |
| I.3 Natura operațiunilor din care provine soldul negativ cerut la rambursare | `informatii@solicit` + 40 de bife | 1 | §4.6 |
| I.4 / I.5 TVA la încasare — deductibilă la plată / colectată la încasare | `informatii@tvaDed*`, `tvaDedAI*`, `tvaCol*` | 1 | §4.7 |
| I.6 Regimuri speciale (agenții de turism; second-hand) | `informatii@incasari_ag…tva_antic` | 1 | §4.7 |
| I.7 Activități din lista ANAF (CAEN) | `lista` | 0–n (unic pe `caen × cota × operat`) | §4.8 |
| Semnătura / întocmit de / opțiunea de consultare | atribute `tip_intocmit`, `den_intocmit`, `cif_intocmit`, `calitate_intocmit`, `functie_intocmit`, `optiune`, `schimb_optiune` | 1 | §4.1 |
| Secțiunea 2 — listele operațiunilor (lit. C, D, E, F) | `op1` + `op11` | 0–n | §4.9 |
| Sume de control interne | `informatii@nrCui1..4`, `declaratie394@totalPlata_A` | 1 | §4.2, §4.1 |

---

## 4. Structura completă, câmp cu câmp

Legenda: **Tip** = tipul XSD (N = întreg, C(n) = șir de max n caractere); **Oblig.** =
`use="required"` în XSD (**DA**) sau condiționat (**cond.**, regula în coloana
Validări) sau opțional (**—**). Validările sunt cele din anexa `structD394_15092025`
(ERR = blocant; ATT = avertizare), completate cu restricțiile din XSD.

### 4.1 Antetul — atributele elementului `declaratie394`

| Atribut | Cartuș | Tip | Oblig. | Validări / semnificație |
|---|---|---|---|---|
| `luna` | perioada | N, 1–12 | DA | numărul perioadei (lună / ultima lună a trimestrului / 06 sau 12 la semestru / 12 la an) |
| `an` | perioada | N, 2016–2100 | DA | |
| `tip_D394` | tip plătitor | `L`/`T`/`S`/`A` | DA | ERR dacă `T` și luna ∉ {02,03,05,06,08,09,11,12}; `S` și luna ∉ {06,12}; `A` și luna ≠ 12 |
| `sistemTVA` | sistem TVA | 0/1 | DA | 0 = sistem normal; 1 = TVA la încasare. Condiționează `tvaDed*`/`tvaCol*` (§4.7) |
| `op_efectuate` | „Au fost efectuate operațiuni?" | 0/1 | DA | ERR dacă =1 și nu există date; ERR dacă =0 și există date; `rezumat2` se completează doar pentru =1 |
| `prsAfiliat` | „operațiuni cu persoane afiliate?" (art. 7 CF) | 0/1 | DA | bifă declarativă; nu are corespondent în liste |
| `cui` | A. cod de înregistrare TVA | token `[1-9]\d{1,9}` | DA | **fără prefixul RO** în XML (formularul tipărit îl arată); „Verificare cui" → ERR cui invalid/inexistent |
| `caen` | A. CAEN al activității preponderente | C(15) | DA | în anexa de validări C(4); lista `Str_coduriCaenSType` (≈600 valori, rev. 2 + coduri noi 8422/5122/9810/9820) e definită în XSD dar **nu e legată de atribut** (atributul e `Str15`) |
| `den` | A. denumire / nume și prenume | C(200) | DA | ERR denumire necompletată |
| `adresa` | A. domiciliul fiscal | C(1000) | DA | ERR adresă necompletată |
| `telefon` | A. | C(15) | DA (XSD) | |
| `fax` | A. | C(15) | — | |
| `mail` | A. | C(200) | — | |
| `totalPlata_A` | sumă de control | N(15) | DA | **= `nrCui1 + nrCui2 + nrCui3 + nrCui4` + Σ `rezumat2(bazaL + bazaA + bazaAI)`** — ERR „suma de control – mod de calcul eronat" |
| `cifR` | B. CIF/CNP reprezentant | token `[1-9]\d{12}` sau `[1-9]\d{1,9}` | — | ERR cui invalid |
| `denR` | B. denumire reprezentant | C(200) | DA (XSD) | |
| `functie_reprez` | B. calitatea reprezentantului | C(100) | DA (XSD) | |
| `adresaR` | B. adresa | C(1000) | DA (XSD) | |
| `telefonR`, `faxR` | B. | C(15) | — | |
| `mailR` | B. | C(200) | — | |
| `tip_intocmit` | „persoana care a întocmit" | 0/1 | DA | 0 = persoană juridică; 1 = persoană fizică |
| `den_intocmit` | denumire / nume și prenume întocmitor | C(75) | DA | |
| `cif_intocmit` | CUI / CNP-NIF întocmitor | N(13) | DA | dacă `tip_intocmit=1` trebuie să fie CNP/NIF; validare CIF |
| `calitate_intocmit` | calitatea (PJ) / altă calitate (PF) | C(75) | cond. | obligatoriu pentru `tip_intocmit=0`; pentru PF: cel puțin unul din `calitate_intocmit` / `functie_intocmit` |
| `functie_intocmit` | funcția în cadrul persoanei impozabile (PF) | C(75) | cond. | trebuie să fie null pentru `tip_intocmit=0` |
| `optiune` | acord de consultare a datelor de către parteneri (art. 11 alin. (3) lit. d) CPF) | 0/1 | DA | acordul dat cu DA e valabil pentru tot anul fiscal, fără posibilitatea revenirii |
| `schimb_optiune` | schimbarea opțiunii în cursul anului (NU → DA) | 1 sau absent | — | dacă =1 atunci `optiune=1` |

> Anexa de validări listează și `nume_declar`/`prenume_declar`/`functie_declar` (pozițiile
> 7–8). **XSD-ul nu are aceste atribute** — declarantul e purtat de `denR`/`functie_reprez`
> (elementul `<idReprezentant>` din anexa de validări a fost aplatizat în atribute
> ale rădăcinii). XSD-ul e autoritatea (§8, punctul 2).

### 4.2 Cartușele C, D, E, F — `rezumat1` (un rând per `tip_partener × cota`)

Tipurile de partener (`Int_tipPartenerSType`, verbatim din anexa de validări):

| `tip_partener` | Cartuș | Definiție |
|---|---|---|
| 1 | C | persoane impozabile înregistrate în scopuri de TVA în România |
| 2 | D | persoane neînregistrate în scopuri de TVA (inclusiv **persoane fizice**) |
| 3 | E | persoane nestabilite în România, stabilite în alt stat membru, neînregistrate și care nu sunt obligate să se înregistreze în scopuri de TVA în România |
| 4 | F | persoane nestabilite în România, neînregistrate și neobligate să se înregistreze, **nestabilite pe teritoriul UE** |

Fiecare `rezumat1` e **calculat** din `op1` (formula: Σ peste `op1` cu același
`tip`, `cota`, `tip_partener`). Obligativitatea fiecărui atribut depinde de
`tip_partener` și `cota` — „<>null" înseamnă „trebuie prezent (poate fi 0)",
„null" înseamnă „trebuie absent":

| Atribut | Tip | Formula | Prezent când |
|---|---|---|---|
| `tip_partener` | 1–4 | — | DA |
| `cota` | ∈ {0,5,9,19,20,24,21,11} | — | DA; `(tip_partener, cota)` unic; pentru `tip_partener=2`: `(tip_partener, cota, tip_N, document_N)` unic |
| `facturiL` | N(15) | Σ `op1@nrFact` pt. `tip=L` | `cota ≠ 0`; null pt. `cota=0` |
| `bazaL` | N(15) | Σ `op1@baza` pt. `tip=L` | idem |
| `tvaL` | N(15) | Σ `op1@tva` pt. `tip=L` | idem |
| `facturiLS` | N(15) | Σ `op1@nrFact` pt. `tip=LS` | `cota=0` și (`tip_partener≠2`, sau `tip_partener=2` și `document_N=1`); null altfel |
| `bazaLS` | N(15) | Σ `op1@baza` pt. `tip=LS` | idem |
| `facturiA` | N(15) | Σ `op1@nrFact` pt. `tip=A` | `tip_partener=1` și `cota≠0` |
| `bazaA` | N(15) | Σ `op1@baza` pt. `tip=A` | idem |
| `tvaA` | N(15) | Σ `op1@tva` pt. `tip=A` | idem |
| `facturiAI` | N(15) | Σ `op1@nrFact` pt. `tip=AI` | `tip_partener=1` și `cota≠0` |
| `bazaAI` | N(15) | Σ `op1@baza` pt. `tip=AI` | idem |
| `tvaAI` | N(15) | Σ `op1@tva` pt. `tip=AI` | idem |
| `facturiAS` | N(15) | Σ `op1@nrFact` pt. `tip=AS` | `tip_partener=1` și `cota=0` |
| `bazaAS` | N(15) | Σ `op1@baza` pt. `tip=AS` | idem |
| `facturiV` | N(15) | Σ `op1@nrFact` pt. `tip=V` | `tip_partener=1` și `cota=0` |
| `bazaV` | N(15) | Σ `op1@baza` pt. `tip=V` | idem — **V n-are coloană de TVA în XSD** (livrarea cu taxare inversă nu colectează) |
| `facturiC` | N(15) | Σ `op1@nrFact` pt. `tip=C` | `tip_partener ∈ {1,3,4}` și `cota≠0` |
| `bazaC` | N(15) | Σ `op1@baza` pt. `tip=C` | idem |
| `tvaC` | N(15) | Σ `op1@tva` pt. `tip=C` | idem — TVA autolichidată de beneficiar |
| `facturiN` | N(15) | Σ `op1@nrFact` pt. `tip=N` și `op1@tip_document=document_N` | `tip_partener=2` și `cota=0` |
| `document_N` | 1–5 | = `op1@tip_document` | idem: 1 facturi · 2 borderouri · 3 file carnet comercializare · 4 contracte · 5 alte documente |
| `bazaN` | N(15) | Σ `op1@baza` pt. `tip=N` (aceeași cheie) | idem |

> Anexa de validări descrie și `facturiASI`/`bazaASI` (achiziții regim special de la
> persoane cu TVA la încasare) și `tip_N`; **XSD-ul nu le are** (nici tipul de
> operațiune `ASI` nu e în enumerare, nici pe formularul tipărit). Autoritatea e XSD-ul.

**Sub-elementul `detaliu`** (0–n per `rezumat1`; formularul tipărit îl arată ca
„din care:" sub V și C în cartușul C și ca „Bunuri/Servicii" în cartușul D):

| Atribut | Tip | Formula | Prezent când |
|---|---|---|---|
| `bun` | `Int_nomenclatorBunuriSType` ∈ 21–36 | — | DA. Pt. `tip_partener=1`: 21 ≤ bun ≤ 31 sau bun = 36; pt. `tip_partener=2`: 21 ≤ bun ≤ 23 sau 32 ≤ bun ≤ 35 |
| `nrLivV` | N(15) | Σ `op11@nrFactPR` pt. `op1@tip=V`, `op11@codPR ↦ bun` | `tip_partener=1` și `cota=0` |
| `bazaLivV` | N(15) | Σ `op11@bazaPR` pt. `tip=V` | idem |
| `nrAchizC` | N(15) | Σ `op11@nrFactPR` pt. `op1@tip=C` | `tip_partener=1` și `cota≠0` |
| `bazaAchizC` | N(15) | Σ `op11@bazaPR` pt. `tip=C` | idem |
| `tvaAchizC` | N(15) | Σ `op11@tvaPR` pt. `tip=C` | idem |
| `nrN` | N(15) | Σ `op11@nrFactPR` pt. `op1@tip=N`, `tip_document=document_N` | `tip_partener=2` și `cota=0` |
| `valN` | N(15) | Σ `op11@bazaPR` pt. `tip=N` | idem |

Corespondența `op11@codPR → detaliu@bun`: codurile NC de cereale/plante tehnice
(`1001, 1002, 1003, 1004, 1005, 1201, 1205, 120600, 121291, 10086000, 120400`)
se **agregă** în `bun=21` („Cereale și plante tehnice" — folosit doar în
`rezumat1`); restul codurilor (22–36) sunt identice în ambele locuri.
Formula din anexă: „`op11(codPR) = bun` pt. `bun<>21`, sau `lung(op11(codPR))>2`
pt. `bun=21`".

**Nomenclatorul bunurilor** (Nomenclator cod produse, verbatim):

| Cod | Denumire | Valabil pt. `tip_partener` | Unde |
|---|---|---|---|
| 1001 | Grâu și meslin | 1, 2 | `op11@codPR` |
| 1002 | Secară | 1, 2 | `op11@codPR` |
| 1003 | Orz | 1, 2 | `op11@codPR` |
| 1004 | Ovăz | 1, 2 | `op11@codPR` |
| 1005 | Porumb | 1, 2 | `op11@codPR` |
| 1201 | Boabe de soia, chiar sfărâmate | 1, 2 | `op11@codPR` |
| 1205 | Semințe de rapiță sau de rapiță sălbatică, chiar sfărâmate | 1, 2 | `op11@codPR` |
| 120600 | Semințe de floarea-soarelui, chiar sfărâmate | 1, 2 | `op11@codPR` |
| 121291 | Sfeclă de zahăr | 1, 2 | `op11@codPR` |
| 10086000 | Triticale | 1, 2 | `op11@codPR` |
| 120400 | Semințe de in, chiar sfărâmate | 1, 2 | `op11@codPR` |
| 21 | Cereale și plante tehnice | 1, 2 | doar `detaliu@bun` (agregatul NC-urilor de mai sus) |
| 22 | Deșeuri feroase și neferoase | 1, 2 | ambele |
| 23 | Masă lemnoasă | 1, 2 | ambele |
| 24 | Certificate de emisii de gaze cu efect de seră | 1 | ambele |
| 25 | Energie electrică | 1 | ambele |
| 26 | Certificate verzi | 1 | ambele |
| 27 | Construcții / terenuri | 1 | ambele |
| 28 | Aur de investiții | 1 | ambele |
| 29 | Telefoane mobile | 1 | ambele |
| 30 | Microprocesoare | 1 | ambele |
| 31 | Console de jocuri, tablete PC și laptopuri | 1 | ambele |
| 32 | Terenuri | 2 | ambele |
| 33 | Construcții | 2 | ambele |
| 34 | Alte bunuri | 2 | ambele |
| 35 | Servicii | 2 | ambele |
| 36 | Gaze naturale | 1 (doar `tip ∈ {V, C}`, adăugat A6.0.0/10.05.2022) | `detaliu@bun` — **lipsește din `Str_listaCodPRSType`** (§8, punctul 4) |

**Numărul de persoane per cartuș** (`informatii`): `nrCui1` = COUNT(DISTINCT
`op1@cuiP`) pt. `tip_partener=1`; **`nrCui2` = numărul de înregistrări `op1`** pt.
`tip_partener=2` (nu distinct — persoanele fizice fără CNP n-au cheie); `nrCui3`,
`nrCui4` = COUNT(DISTINCT `cuiP`) pt. tipurile 3, 4. Toate `N(15)`, obligatorii,
„Atenționare dacă nu se respectă formula".

### 4.3 Cartușul H + I.1 — `rezumat2` (un rând per cotă)

`rezumat2@cota ∈ {5,9,19,20,24,21,11}` (fără 0). Se completează doar pentru
`op_efectuate=1`. XSD: `minOccurs=0 maxOccurs=5` (§8, punctul 3). Toate
atributele de sume sunt `use="required"` cu excepția celor `*_incasari_*`.

| Atribut | Cartuș | Formula / conținut | Oblig. |
|---|---|---|---|
| `cota` | — | cota rândului | — în XSD (dar semantic obligatoriu) |
| `nrFacturiL` | H | Σ `op1@nrFact` pt. `tip ∈ {L, V}` și `cota` | DA |
| `bazaL` | H | Σ `op1@baza` pt. `tip ∈ {L, V}` | DA |
| `tvaL` | H | Σ `op1@tva` pt. `tip ∈ {L, V}` | DA |
| `nrFacturiA` | H (A + C) | Σ `op1@nrFact` pt. `tip ∈ {A, C}` | DA |
| `bazaA` | H (A + C) | Σ `op1@baza` pt. `tip ∈ {A, C}` | DA |
| `tvaA` | H (A + C) | Σ `op1@tva` pt. `tip ∈ {A, C}` | DA |
| `nrFacturiAI` | H (AÎ) | Σ `op1@nrFact` pt. `tip=AI` | DA |
| `bazaAI` | H (AÎ) | Σ `op1@baza` pt. `tip=AI` | DA |
| `tvaAI` | H (AÎ) | Σ `op1@tva` pt. `tip=AI` | DA |
| `bazaFSLcod`, `TVAFSLcod` | I.1.1 | livrări cu **facturi simplificate emise care au înscris CUI-ul beneficiarului** | DA |
| `bazaFSL`, `TVAFSL` | I.1.2 | livrări cu facturi simplificate emise **fără CUI-ul beneficiarului și pentru care nu s-au emis bonuri fiscale** | DA |
| `bazaFSA`, `TVAFSA` | I.1.3 | achiziții cu facturi simplificate primite de la persoane cu **sistem normal**, cu CUI-ul beneficiarului înscris | DA |
| `bazaFSAI`, `TVAFSAI` | I.1.4 | idem, de la persoane cu **TVA la încasare** | DA |
| `bazaBFAI`, `TVABFAI` | I.1.5 | achiziții cu **bonuri fiscale-facturi simplificate** primite, cu CUI-ul beneficiarului înscris | DA |
| `baza_incasari_i1`, `tva_incasari_i1` | G (total pe cotă) | Σ `op2@baza{cota}` / `op2@TVA{cota}` pt. `tip_op2=I1` | cond.: obligatoriu pt. `cota ∉ {19, 24}` (anexă; text pre-2025, vezi §8 p. 6) |
| `baza_incasari_i2`, `tva_incasari_i2` | G (total pe cotă) | idem pt. `tip_op2=I2` | idem |
| `bazaL_PF`, `tvaL_PF` | (istoric) livrări L către persoane fizice ≤ 10.000 lei | **„Obligatoriu 0 începând cu 01.01.2017"** | DA |

> Cartușul H e „suma de control" a listelor: se recalculează integral din `op1`.
> **Livrările cu taxare inversă (V) intră în H la L, achizițiile cu taxare inversă
> (C) intră la A** — instrucțiunile la H pct. 4–5: „inclusiv cele pentru care se
> aplică taxarea inversă". Facturile simplificate nu intră în H (sunt separat în I.1).

### 4.4 Cartușul G — `op2` (încasări AMEF / activități exceptate, per lună)

| Atribut | Tip | Oblig. | Validări |
|---|---|---|---|
| `tip_op2` | `I1` / `I2` | DA | **I1** = încasări lunare prin AMEF, **cu excepția celor pentru care s-au emis facturi**, inclusiv bonurile fiscale-facturi simplificate cu sau fără CUI-ul beneficiarului; **I2** = încasări lunare din activități exceptate de la obligația AMEF (OUG 28/1999). `(tip_op2, luna)` unic |
| `luna` | 1–12 | DA | fiecare lună din perioada de raportare separat (la trimestru: 3 rânduri per tip) |
| `nrAMEF` | N(4) | cond. | `<>null și >0` pt. I1; null pt. I2. ATT dacă `nrBF/nrAMEF > 15000` sau `≤ 1` |
| `nrBF` | N(15) | cond. | idem I1/I2 — numărul bonurilor fiscale emise în lună |
| `total` | N(15) ≥ 0 | DA | total încasări (brut); `total ≥ Σ baze` (ERR); ATT dacă `total ≠ Σ baze + Σ TVA` (anexa spune „total <> baza20+baza19+baza9+baza5 atenționare") |
| `baza20`, `baza9`, `baza5`, `baza19`, `baza21`, `baza11` | N(15) ≥ 0 | DA | `baza{c} ≥ TVA{c}` pentru fiecare cotă; Σ baze > 0 |
| `TVA20`, `TVA9`, `TVA5`, `TVA19`, `TVA21`, `TVA11` | N(15) ≥ 0 | DA | |

Totalurile din `informatii`: `nr_BF_i1` = Σ `op2@nrBF` pt. I1; `incasari_i1` =
Σ `op2@total` pt. I1; `incasari_i2` = Σ `op2@total` pt. I2 (toate obligatorii).
**Nu există câmp pentru cota 24% în `op2`** (`IntPoz15SType` — valorile sunt
nenegative: stornările pe bon reduc baza lunii, nu apar cu semn).

### 4.5 Cartușul I.2 — `serieFacturi`, `facturi`, totalurile de facturi

**`serieFacturi`** (0–n):

| Atribut | Tip | Oblig. | Semnificație / validări |
|---|---|---|---|
| `tip` | 1–4 | DA | 1 = **plaja alocată** (I.2.1, prin decizie internă; se exclud facturile pentru intracomunitare, import/export și neimpozabile) · 2 = **emise** în perioadă din plaja alocată (I.2.2) · 3 = emise de **beneficiari** în numele persoanei (art. 319 alin. (18), I.2.3) · 4 = emise de **terți** în numele persoanei (art. 319 alin. (19), I.2.4) |
| `serieI` | C(20) | — | seria inițială (poate lipsi când facturile au doar număr; serii alfanumerice permise) |
| `nrI` | C(20) | DA | numărul inițial |
| `nrF` | C(20) | — | numărul final |
| `den` | C(100) | cond. | denumirea beneficiarului/terțului: `<>null` pt. `tip ∈ {3,4}`, null pt. {1,2} |
| `cui` | `CuiSType` | cond. | CUI-ul beneficiarului/terțului: aceeași regulă |

> Anexa de validări listează și `serieF` (seria finală); **XSD-ul n-are atributul**
> (§8, punctul 5). Formularul tipărit are „la seria … numărul …".

Reguli de coerență (anexă, pentru an ≥ 2017): dacă există `op1` cu `tip ∈ {L, LS}`
(sau `nrFacturiL_PF > 0` / `nrFacturiLS_PF > 0`) atunci există **cel puțin un
`serieFacturi` cu `tip=1` și unul cu `tip=2`**; dacă există `tip=2` există și
`tip=1`; `informatii@nrFacturi > 0` ⇔ există `tip=2`; `nrFacturi_benef > 0` ⇔
există `tip=3`; `nrFacturi_terti > 0` ⇔ există `tip=4`; dacă există `op1` cu
`tip ∈ {L, LS, V}` atunci `nrFacturi + nrFacturi_benef + nrFacturi_terti > 0`.

**`facturi`** (0–n) — facturile individuale declarate nominal:

| Atribut | Tip | Oblig. | Validări |
|---|---|---|---|
| `tip_factura` | 1–4 | DA | 1 = **stornată** („factura emisă a cărei valoare totală este negativă") · 2 = **anulată** („netransmisă beneficiarului, operațiunile nefiind înregistrate în contabilitate") · 3 = **autofacturare** · 4 = emisă de persoana impozabilă **în calitate de beneficiar, în numele furnizorilor** (art. 319 alin. (18)) |
| `serie` | C(20) | — | |
| `nr` | C(20) | DA | |
| `baza24`, `baza21`, `baza11`, `baza20`, `baza19`, `baza9`, `baza5` | N(15) | cond. | `<>null` pt. `tip_factura=3`; null pt. `tip_factura ∈ {1,2,4}` |
| `tva24`, `tva21`, `tva11`, `tva20`, `tva19`, `tva9`, `tva5` | N(15) | cond. | idem |

„Informațiile privind autofacturarea **nu vor fi declarate la alte rubrici** din
declarație" (pct. 2.2.3) — autofacturile nu apar în `op1`.

**Totalurile din `informatii`** (toate `N(15)`, obligatorii): `nrFacturi` (total
facturi emise în perioadă), `nrFacturi_benef`, `nrFacturi_terti`, `nrFacturiL_PF`,
`nrFacturiLS_PF`, `val_LS_PF` — ultimele trei „Obligatoriu 0 începând cu 01.01.2017"
(relicve ale plafonului de 10.000 lei/persoană fizică din 2016; mecanismul e mort,
câmpurile au rămas).

### 4.6 Cartușul I.3 — `solicit` și bifele naturii soldului negativ

`informatii@solicit` (0/1, obligatoriu): „se solicită rambursarea sumei negative
înregistrate în decontul de TVA aferent perioadei de raportare". Dacă `solicit=1`,
**toate** bifele de mai jos devin obligatorii (0/1); dacă `solicit=0` trebuie să
fie absente (ERR). Denumirile sunt cele din formular:

| Grup | Atribut | Bifa |
|---|---|---|
| Achiziții legate direct de bunuri imobile | `achizitiiPE` | parcuri eoliene |
| | `achizitiiCR` | construcții rezidențiale |
| | `achizitiiCB` | clădiri de birouri |
| | `achizitiiCI` | construcții industriale |
| | `achizitiiA` | altele |
| Achiziții de bunuri, cu excepția celor legate de imobile | `achizitiiB24` / `achizitiiB21` / `achizitiiB20` / `achizitiiB19` / `achizitiiB11` / `achizitiiB9` / `achizitiiB5` | cu cota 24% / 21% / 20% / 19% / 11% / 9% / 5% |
| Achiziții de servicii, cu excepția celor legate de imobile | `achizitiiS24` / `achizitiiS21` / `achizitiiS20` / `achizitiiS19` / `achizitiiS11` / `achizitiiS9` / `achizitiiS5` | cu cota 24% / 21% / 20% / 19% / 11% / 9% / 5% |
| | `importB` | importuri de bunuri |
| | `acINecorp` | achiziții imobilizări necorporale |
| Livrări | `livrariBI` | livrări de bunuri imobile |
| Livrări de bunuri, cu excepția imobilelor | `BUN24` / `BUN21` / `BUN20` / `BUN19` / `BUN11` / `BUN9` / `BUN5` | cu cota 24% / 21% / 20% / 19% / 11% / 9% / 5% |
| | `valoareScutit` | livrări de bunuri scutite de TVA |
| | `BunTI` | livrări de bunuri / prestări de servicii pentru care se aplică taxarea inversă |
| Prestări de servicii | `Prest24` / `Prest21` / `Prest20` / `Prest19` / `Prest11` / `Prest9` / `Prest5` | cu cota 24% / 21% / 20% / 19% / 11% / 9% / 5% |
| | `PrestScutit` | prestări de servicii scutite de TVA |
| | `LIntra` | livrări intracomunitare de bunuri |
| | `PrestIntra` | prestări intracomunitare de servicii |
| | `Export` | exporturi de bunuri |
| | `livINecorp` | livrări imobilizări necorporale |
| | `efectuat` | „Persoana impozabilă nu a efectuat livrări de bunuri/prestări de servicii în perioada de raportare" (anexă: 0 = nu a efectuat, 1 = a efectuat) |

### 4.7 Cartușele I.4, I.5, I.6 — TVA la încasare și regimurile speciale (atribute `informatii`)

Toate `N(15)`; cele șapte cote: `24, 21, 11, 20, 19, 9, 5`.

| Atribut | Cartuș | Conținut (verbatim din formular) | Oblig. |
|---|---|---|---|
| `tvaDedAI{cotă}` | **I.4.1** (sistem normal) și **I.5.3** (sistem la încasare) | TVA deductibilă aferentă facturilor **achitate** în perioada de raportare, indiferent de data primirii, de la persoane care aplică **sistemul de TVA la încasare**, per cotă | **DA** în XSD (toate cele 7) |
| `tvaCol{cotă}` | **I.5.1** | TVA colectată aferentă facturilor **încasate** în perioada de raportare, indiferent de data emiterii, de către persoana care aplică TVA la încasare | cond.: null pt. `sistemTVA=0`, `<>null` pt. `sistemTVA=1` |
| `tvaDed{cotă}` | **I.5.2** | TVA deductibilă aferentă facturilor **achitate** în perioadă, indiferent de data primirii, de la persoane care aplică **sistemul normal** | cond.: null pt. `sistemTVA=0`, `<>null` pt. `sistemTVA=1` |
| `incasari_ag`, `costuri_ag`, `marja_ag`, `tva_ag` | **I.6.1** agenții de turism | suma totală plătită de călător (fără taxă) · costurile agenției (inclusiv taxa) · marja de profit · TVA | — |
| `pret_vanzare`, `pret_cumparare`, `marja_antic`, `tva_antic` | **I.6.2** second-hand / opere de artă / colecție / antichități | preț de vânzare · preț de cumpărare · marja · TVA | — |

> `tvaDedAI*` e singurul set obligatoriu pentru **toți** depunătorii: cine aplică
> sistemul normal completează acolo I.4.1 (plățile către furnizori cu TVA la
> încasare), cine aplică TVA la încasare completează I.5.3. Anexa de validări scrie
> pentru `tvaDedAI*` „=null pt sistem_TVA=0" — contrazice XSD-ul (`required`) și
> formularul (I.4.1 există tocmai pentru sistemul normal); XSD-ul e autoritatea (§8, p. 7).

### 4.8 Cartușul I.7 — `lista` (activități din lista ANAF)

Un rând per `(caen, cota, operat)`, unic. Se completează **indiferent de CAEN-ul
preponderent** declarat la A, dacă activitatea a fost desfășurată în perioadă.

| Atribut | Tip | Oblig. | Valori |
|---|---|---|---|
| `caen` | `Int_listaCaenSType` | DA | **lista închisă**: 1071 Cofetărie și produse de patiserie · 4520 Spălătorie auto · 4730 Comerț cu amănuntul al carburanților · 47761 Comerț cu amănuntul al florilor, plantelor și semințelor · 47762 Comerț cu amănuntul al animalelor de companie și al hranei pentru acestea · 4932 Transporturi cu taxiuri · 55101 Hoteluri · 55102 Pensiuni turistice · 56103 Restaurante · 5630 Baruri și activități de servire a băuturilor · 812 Activități de curățenie · 9313 Activități ale centrelor de fitness · 9602 Activități de coafură și de înfrumusețare · 9603 Servicii de pompe funebre |
| `cota` | ∈ {5,9,19,20,24,21,11} | DA | |
| `operat` | 1/2 | DA | 1 = livrări bunuri · 2 = prestări servicii |
| `valoare` | N(15) | DA | valoarea livrărilor/prestărilor fără TVA |
| `tva` | N(15) | DA | |

> Codurile 47761/47762/55101/55102/56103 sunt **subdiviziuni proprii ANAF** (5
> cifre), nu CAEN standard — sursa (Anexa 2) le listează cu 4 cifre (4776, 5510,
> 5610) și textul diferit; XSD-ul cere cele 5 cifre. Un model care ține doar CAEN
> rev. 2/3 pe 4 cifre trebuie să aleagă manual subdiviziunea.

### 4.9 Secțiunea 2 — `op1` (o înregistrare per partener × tip × cotă) și `op11`

**Tipurile de operațiune** (`Str_listaTipOperatieSType`; definițiile verbatim din
Anexa 2, secțiunea a 2-a, pct. 2):

| `tip` | Definiție | Are TVA | `cota` |
|---|---|---|---|
| `L` | livrări de bunuri/prestări de servicii pentru care au fost emise facturi, **cu excepția facturilor simplificate** | da | ≠ 0 |
| `A` | achiziții pentru care au fost primite facturi, cu excepția facturilor simplificate (de la persoane cu sistem normal) | da | ≠ 0 |
| `AI` (tipărit **AÎ**) | achiziții pentru care au fost primite facturi **cu TVA la încasare** | da | ≠ 0 |
| `LS` | livrări pentru care au fost emise facturi de persoane care aplică **regimul special** (agenții de turism, second-hand, opere de artă, colecție, antichități) | nu (doar bază) | 0 |
| `AS` | achiziții pentru care au fost primite facturi de la persoane cu regimul special | nu | 0 |
| `V` | livrări pentru care se aplică **taxarea inversă** (art. 331) | nu (beneficiarul o autolichidează) | 0 |
| `C` | achiziții pentru care se aplică taxarea inversă | da (autolichidată) | ≠ 0 |
| `N` | achiziții de la **persoane neînregistrate** în scopuri de TVA, pentru care au fost primite facturi / borderouri de achiziții / file din carnetele de comercializare a produselor agricole / contracte | nu | 0 |

Regula de cotă: „valoarea 0 este permisă dacă și numai dacă `tip ∈ (LS, AS, N, V)`"
(anexa include și `ASI`, inexistent în XSD). Pentru `tip ∈ {L, A, AI, C}` cota e
una din {5, 9, 19, 20, 24, 21, 11}.

**Combinațiile permise partener × tip** (anexă, verbatim):
- `tip_partener=1` ⇒ `tip ≠ N` (deci L, A, AI, LS, AS, V, C);
- `tip_partener=2` ⇒ `tip ∈ {L, LS, N}`;
- `tip_partener ∈ {3, 4}` ⇒ `tip ∈ {L, LS, C}`.

| Atribut | Tip | Oblig. | Validări |
|---|---|---|---|
| `tip` | enumerare | DA | ERR „tip operațiune diferit de A,L,C,V,AI,LS,AS,N" |
| `tip_partener` | 1–4 | DA | |
| `cota` | ∈ {0,5,9,19,20,24,21,11} | DA | regula de mai sus |
| `cuiP` | C(50) | cond. | „Verificare validitate cuiP pt. `tip_partener ∈ (1,2)`" → ERR cuiP invalid/inexistent. Pt. tip 1: codul de înregistrare TVA (sau al reprezentantului fiscal desemnat). Pt. tip 2: CIF/CUI sau **CNP/NIF** (13 cifre) — obligatoriu când e colectat de la persoana fizică (facturi, contracte); **poate lipsi** (persoană fizică fără CNP, borderouri/file carnet). Pt. tip 3: cod valabil de TVA în statul membru. Pt. tip 4: cod din registrul companiilor din țara de rezidență. **Unicitate: `(cuiP, tip, cota)` unic** (ERR „combinația (cuiP, tip, cota_TVA) are apariții multiple") |
| `denP` | C(200) | DA | ERR denumire necompletată |
| `taraP` | `Str_listaTariSType` (ISO 3166-1 alfa-2, 249 valori) | cond. | obligatoriu pt. `tip_partener=2` și `cuiP=null`; null pt. `tip_partener≠2` sau `cuiP≠null` |
| `locP` | C(75) | cond. | localitate — obligatoriu pt. tip 2 fără `cuiP` și `taraP=RO` |
| `judP` | `Str_listaJudSType` (coduri SIRUTA de județ 01–40, 51, 52; 40 = București) | cond. | idem; null pt. `taraP≠RO` |
| `strP` | C(75) | cond. | idem |
| `nrP` | C(50) | cond. | idem |
| `blP` | C(50) | cond. | idem |
| `apP` | C(50) | cond. | idem |
| `detP` | C(100) | cond. | „alte detalii adresă" — se completează când nu există adresă din România sau când sunt alte elemente decât cele pretipărite |
| `tip_document` | 1–5 | cond. | obligatoriu pt. `tip_partener=2` și `tip=N`; altfel null. 1 facturi · 2 borderouri · 3 file carnet comercializare · 4 contracte · 5 alte documente |
| `nrFact` | N(15) ≥ 0 | DA | ERR nr. facturi necompletat; regula 1/0 pentru facturi multi-cotă (§5.2) |
| `baza` | N(15), cu semn | DA | ERR bază necompletată; negativă pentru stornări |
| `tva` | N(15), cu semn | cond. | obligatoriu pt. `tip ∈ {L, A, AI, C}`; null pt. {LS, AS, V, N} (anexa scrie „(L,V,C,A,AI)" — dar V n-are TVA în rezumate; XSD: opțional) |

> Anexa de validări menționează și `tip_N` (1 = bunuri, 2 = servicii) pe `op1`;
> **XSD-ul nu-l are**. Natura bunului pentru `N` e purtată de `op11@codPR` (32–35).

**`op11`** — defalcarea pe bunuri (0–24 apariții per `op1`; anexa scrie „0–24"):

| Atribut | Tip | Oblig. | Validări |
|---|---|---|---|
| `nrFactPR` | N(15) | DA | număr de facturi per bun; dacă o factură V/C conține mai multe bunuri de la lit. C: **1 la bunul cu baza cea mai mare (V) / cu TVA cea mai mare (C), 0 la restul** |
| `codPR` | `Str_listaCodPRSType` | DA | nomenclatorul §4.2 (per `tip_partener`); ERR cod necompletat; **unicitate `codPR` în cadrul `op1`** |
| `bazaPR` | N(15) | DA | |
| `tvaPR` | N(15) | cond. | null pt. `tip_partener=2` sau (`tip_partener=1` și `tip=V`); altfel `<>null` |

**Când e obligatorie secțiunea `op11`** (anexă, verbatim): „Pt. ((`tip ∈ (V, C)` și
`tip_partener=1`) sau (`tip=N` și (`lung(cuiP)=13` sau `cuiP=null`))) secțiunea
este obligatorie" — adică pentru **toate** operațiunile cu taxare inversă cu
parteneri români și pentru achizițiile **de la persoane fizice** (CNP sau fără cod).
Apare numai pentru `tip ∈ (L, A, V, C, AI)` la `tip_partener=1` și pentru `tip=N`
la `tip_partener=2` cu `cota=0`.

**Ce NU intră în `op1`**: facturile simplificate (→ `rezumat2`, I.1), bonurile
fiscale (→ `op2`, G), autofacturile (→ `facturi`, I.2), operațiunile din D390
(intracomunitare), exportul și importul (chiar cu certificat de amânare a plății
în vamă — cartușul F pct. 10), facturile emise de beneficiari/terți în numele
persoanei (doar numeric, I.2.3/2.4).

---

## 5. Regulile de agregare (din instrucțiuni; sinteză marcată ca atare)

### 5.1 Cheia de agregare

Secțiunea 2 cere „valorile totale înscrise în facturile emise/primite … **pentru
fiecare partener de tranzacție** pentru perioada de raportare, **defalcate pe
fiecare cotă de TVA**". Cheia rândului `op1` este deci:

> **(`tip_partener`, `cuiP` sau identitatea persoanei fizice, `tip`, `cota`)**, cu
> unicitatea verificată pe `(cuiP, tip, cota)`.

Pentru taxarea inversă, cheia coboară cu un nivel: `op11` per **categorie de
bunuri** (`codPR`) — „se completează separat față de celelalte operațiuni, pentru
fiecare cod de înregistrare în scopuri de TVA, categorie de bunuri/servicii supuse
taxării inverse și pentru fiecare cotă defalcată de TVA". Pentru cereale codul e
**NC-ul** (1001, 1002…), nu categoria 21.

Pentru `N` cheia include și **`tip_document`** (facturi vs. borderouri vs. file
carnet vs. contracte vs. alte documente) — `rezumat1` pentru `tip_partener=2` e
unic pe `(cota, tip_N, document_N)`.

Consecință (sinteză): un partener cu o factură cu două cote apare pe **două rânduri
`op1`**; același partener cu livrări (L) și achiziții (A) apare pe rânduri
distincte; `nrCui1` numără **persoanele distincte**, nu rândurile.

### 5.2 Numărul de facturi (`nrFact`)

Instrucțiuni, secțiunea a 2-a pct. 5, verbatim: „în situația în care în cuprinsul
unei facturi emise/primite există operațiuni cu cote de TVA diferite, la rubrica
«număr de facturi» se vor înscrie: **valoarea 1 în dreptul operațiunii cu valoarea
cea mai mare a TVA și valoarea 0 pentru restul operațiunilor**; dacă valoarea TVA
este aceeași pentru operațiunile respective, factura se va numerota o singură dată
în dreptul **cotei de TVA cea mai mare**." Aceeași regulă pentru factura de taxare
inversă cu mai multe categorii de bunuri (1 la categoria cu TVA cea mai mare la C;
anexa de validări spune „cu valoarea cea mai mare a **bazei**" la V — V n-are TVA).

Consecință: `nrFact` **nu e** COUNT(DISTINCT factură) per rând, ci o atribuire
**a fiecărei facturi la exact un rând** (cotă/bun). Σ `nrFact` pe toate rândurile
unui partener = numărul facturilor lui. Un generator are nevoie de agregarea **per
factură** înainte de agregarea per rând.

### 5.3 Stornările și facturile negative

- Instrucțiuni pct. 6–7: baza și TVA includ „**valoarea bazei impozabile aferentă
  facturilor de stornare**", iar „în cazul în care baza impozabilă este negativă
  valoarea totală a acesteia se înscrie cu semnul (−)". Deci stornările **se
  compensează în același rând** `(partener, tip, cotă)` cu facturile pozitive;
  rândul poate ieși negativ.
- Cota rândului e **cota facturii stornate**; un storno în 2026 al unei facturi
  cu 19% se declară cu `cota=19` (cotele istorice rămân în enumerare — §1).
- **Separat**, factura stornată se declară **nominal** la I.2.2 (`facturi` cu
  `tip_factura=1`, seria + numărul, fără sume). Definiție: „factura emisă … a cărei
  valoare totală este negativă" — criteriul e semnul totalului facturii, nu
  existența unei referințe la factura originală.
- `nrFact` pentru storno: instrucțiunile nu spun explicit; regula 1/0 se aplică
  mecanic (factura de storno e o factură). *(Interpretare proprie: se numără +1,
  nu −1.)*
- Facturile **anulate** (`tip_factura=2`) nu intră nicăieri în sume — sunt
  „netransmise beneficiarului, operațiunile nefiind înregistrate în contabilitate".
- Pe `op2` (bonuri) valorile sunt `IntPoz15SType` — stornările pe bon reduc baza
  lunii, dar rândul nu poate deveni negativ.

### 5.4 Persoanele fizice și persoanele neînregistrate (`tip_partener=2`)

- **Livrări (L)** către persoane fizice: se declară nominal cu **CNP** dacă persoana
  impozabilă îl colectează (facturi, contracte de prestări servicii/utilități/
  vânzări de bunuri mobile și imobile); în lipsa lui, **numele, prenumele și
  adresa** (țară, localitate, județ/sector, stradă, nr., bl., ap., alte detalii).
  **Nu există prag de valoare** în forma curentă — câmpurile `*_PF` ale plafonului
  de 10.000 lei sunt „obligatoriu 0 din 01.01.2017". Consecință: **fiecare factură
  către o persoană fizică cere identitatea ei** (CNP sau nume + adresă).
- **Achiziții (N)** de la persoane fizice: `op11` obligatoriu cu natura bunului
  (21–23, 32–35), `tip_document` obligatoriu; pentru borderouri/file carnet „se vor
  declara numele, prenumele și adresa persoanei fizice".
- Persoanele **juridice** neînregistrate în scopuri de TVA (neplătitori de TVA)
  sunt tot `tip_partener=2`, cu CUI-ul lor în `cuiP` (validat), fără adresă.
- Persoanele fizice **nu apar deloc în `op1`** când vânzarea s-a făcut pe bon
  fiscal (→ G) sau factură simplificată fără CUI (→ I.1.2).

### 5.5 Partenerii străini (`tip_partener` 3 și 4)

Se declară **numai** operațiunile taxabile cu locul în România (art. 275/278) care
**nu** intră în D390; la achiziții, numai cele cu beneficiar obligat la plata TVA
(art. 307 alin. (2), (3), (5), (6)) → `tip=C`. **Nu se declară** exportul și
importul. Identificatorul: cod valabil de TVA din statul membru (3) sau cod din
registrul companiilor din țara de rezidență (4); `taraP` **nu** se completează
pentru 3/4 (regula „null pt. `tip_partener≠2`") — țara e implicită în tipul de
partener, nu în date.

### 5.6 Bonurile fiscale și facturile simplificate

| Situație | Emitent (vânzător) | Primitor (cumpărător) |
|---|---|---|
| Bon fiscal obișnuit (fără CUI) | G / `op2` I1 (lunar, per cotă; nr. bonuri; nr. AMEF) | — (nu se declară) |
| Bon fiscal care îndeplinește condițiile unei facturi simplificate, **cu CUI-ul beneficiarului** | G / `op2` I1 („indiferent dacă au/nu au înscris codul") | I.1.5 `bazaBFAI`/`TVABFAI` |
| Bon fiscal pentru care s-a emis apoi **factură** | **exclus din G**; factura → `op1` | factura → `op1` |
| Factură simplificată emisă **cu CUI** | I.1.1 `bazaFSLcod`/`TVAFSLcod` | I.1.3 (furnizor sistem normal) / I.1.4 (furnizor TVA la încasare) |
| Factură simplificată emisă **fără CUI**, fără bon fiscal | I.1.2 `bazaFSL`/`TVAFSL` | — |
| Activitate exceptată de la AMEF (OUG 28/1999) | G / `op2` I2 (lunar); dacă s-au emis și facturi → cartușul operațiunii | — |

Pragul valoric al facturii simplificate (art. 319 alin. (12) CF) **nu apare** în
ordin — formularul nu-l validează; ce e „factură simplificată" e decizia
emitentului, D394 doar o clasifică.

### 5.7 Regularizări, avansuri, perioada facturilor

- **Avansurile** se declară ca orice factură (pct. 1 lit. a): „inclusiv pentru avansuri".
- Facturile se declară **în perioada emiterii (L) / primirii (A)**, „indiferent de
  data la care intervine exigibilitatea" — D394 e pe **data documentului**, nu pe
  exigibilitate (spre deosebire de D300). O factură primită cu întârziere se
  declară în perioada primirii, fără redepunerea perioadei emiterii.
- Nu există rânduri de regularizare; corecția = redepunere integrală (§2).
- Ajustările de bază (art. 287) se declară prin documentul care le materializează
  (factura de storno/regularizare), în perioada emiterii lui.

---

## 6. Maparea conceptelor formularului pe un model contabil

Ce trebuie să existe în model (per linie de registru de TVA, per document, per
partener) ca D394 să fie o proiecție pură, fără culegere manuală la generare:

| Concept D394 | Câmp XML | Ce cere de la model | Există în `RegistruTva`/nomenclatoare azi? (de verificat) |
|---|---|---|---|
| Tip partener (1–4) | `op1@tip_partener` | pe `Partener`: **înregistrat în scopuri de TVA în RO** (da/nu, cu valabilitate în timp — un partener poate deveni plătitor în cursul anului), **stabilit în RO / în alt SM / în afara UE**, **persoană fizică** | de verificat; probabil doar CUI + țară; lipsește statutul de plătitor de TVA cu istoric |
| CUI / CNP / cod străin | `op1@cuiP` | CUI fără prefix `RO`; CNP (13) pentru PF; cod TVA din SM pentru tip 3; cod registru pentru tip 4 | de verificat formatul (prefix RO) |
| Persoană fizică fără CNP | `op1@taraP…detP` | adresa structurată (țară ISO-2, localitate, județ cod SIRUTA 2 cifre, stradă, nr., bloc, ap., alte detalii) | lipsește probabil (satelitul partenerilor e amânat — 34g) |
| Tip operațiune (L/A/AI/LS/AS/V/C/N) | `op1@tip` | sensul (livrare/achiziție) + **regimul furnizorului** (normal / TVA la încasare / regim special marjă) + **taxarea inversă art. 331** + **partener neînregistrat** | sensul și TI există (`PoliticaTva.Directie`, 70a); regimul de TVA la încasare al **furnizorului** (AI) și regimul special (LS/AS) lipsesc probabil |
| Cotă | `op1@cota` | cota procentuală întreagă a `TipTva`; 0 pentru LS/AS/V/N; cota **originală** pe storno | `TipTva` (36a); storno în cheie (68) |
| Bază / TVA per (partener × tip × cotă) | `op1@baza/tva` | agregarea liniilor `RegistruTva` pe document → partener; sume în lei întregi (rotunjire la nivel de rând agregat) | atomul există (68); convenția de rotunjire = 51c |
| Număr de facturi cu regula 1/0 | `op1@nrFact` | agregare **per document** (cota cu TVA maxim), apoi per rând | de construit în proiecție |
| Categoria de bunuri cu taxare inversă + cod NC pentru cereale | `op11@codPR` | pe `Produs`/`TipMaterial` (sau pe linie): **codul NC** (8 cifre pentru cereale) sau categoria 22–36; pentru N: natura 21–23/32–35 | lipsește probabil; e o dimensiune nouă a nomenclatorului de produse |
| Tip document la N | `op1@tip_document` | tipul documentului de achiziție de la neînregistrați (factură / borderou / filă carnet / contract / alt document) | lipsește probabil; `TipDocument` de import ≠ această clasificare |
| Factură stornată / anulată (nominal) | `facturi@tip_factura=1/2` | seria + numărul facturii; semnul totalului; starea „anulată" (număr consumat, nefolosit) | numărul există (serie `FCL-`, 30b); „anulat" ≠ storno — de verificat dacă există stare distinctă |
| Autofactură | `facturi@tip_factura=3` + baze/TVA per cotă | tip de document „autofactură" cu defalcare pe cote, **exclus din `op1`** | lipsește (nu e în lista tipurilor din decizia 19/46) |
| Facturi emise de beneficiar în numele furnizorului / de terți | `facturi@tip_factura=4`, `serieFacturi@tip=3/4` | marcaj pe document + identitatea beneficiarului/terțului | lipsește |
| Plaja de facturi alocată | `serieFacturi@tip=1` | decizia internă: serie + interval de numere per an | `PoliticaNumerotare` (25f) — de verificat dacă are interval final |
| Facturi emise din plajă în perioadă | `serieFacturi@tip=2`, `informatii@nrFacturi` | min/max număr emis per serie în perioadă, COUNT | derivabil din documente |
| Încasări AMEF | `op2` | per lună: nr. AMEF, nr. bonuri, total încasări, bază/TVA per cotă; **bonurile cu factură ulterioară excluse** | lipsește (nu există bon fiscal ca document; „BF" e variantă de FacturaIntrare, decizia 19) |
| Facturi simplificate (emise / primite, cu/fără CUI) | `rezumat2@*FS*`, `*BFAI` | marcaj „simplificată" pe document + prezența CUI-ului beneficiarului | lipsește probabil |
| TVA la încasare: plăți/încasări în perioadă per cotă | `tvaDed*`, `tvaDedAI*`, `tvaCol*` | imperecherea plată↔factură (31d) × cota liniilor stinse × regimul furnizorului | imperecherea există; regimul furnizorului lipsește |
| Regim special marjă | `incasari_ag…tva_antic` | în afara scopului (regim special) | nu |
| Activități CAEN din lista I.7 | `lista` | CAEN-ul **activității** (nu al firmei) pe document/linie, cu subdiviziunile ANAF 5 cifre | lipsește; date de profil |
| Bifele I.3 (natura soldului negativ) | `informatii@achizitii*/BUN*/Prest*` | clasificarea achizițiilor/livrărilor perioadei pe bunuri/servicii/imobile/import/necorporale + cote — derivabilă din `Natura` clasei și cotă, dar „legate direct de bunuri imobile" cere un marcaj propriu | parțial derivabil |
| Persoane afiliate | `prsAfiliat` | flag pe `Partener` | lipsește probabil |
| Sistemul de TVA al depunătorului | `sistemTVA` | `SetareProfil` (52a) | de verificat |
| CAEN preponderent, reprezentant, întocmitor, opțiunea de consultare | antet | date de profil per bază, per an (opțiunea e anuală) | date de profil |

Concluzia de model (sinteză): D394 e o proiecție peste `RegistruTva` **doar pentru
coloanele de sume**; identitatea rândului cere trei lucruri care nu sunt în
registrul de TVA: **clasificarea partenerului** (tip 1–4, cu istoricul
înregistrării în scopuri de TVA), **regimul furnizorului** (AI/AS) și
**categoria de bunuri cu taxare inversă** (NC/categorie) pe produs. Plus două
familii de documente care nu există: **bonul fiscal** (G) și **autofactura** (I.2).

---

## 7. Validările din anexa `structD394_15092025` — sinteza

**Blocante (ERR)**:
- structurale: `luna`, `an ≥ 2016`, `tip_D394` vs. `luna`; `op_efectuate` vs.
  prezența datelor; `cui`/`cifR`/`cif_intocmit`/`cuiP` (validitate CIF; CNP când e
  persoană fizică); `denP`, `nrFact`, `baza` necompletate; `codPR` necompletat;
- unicitate: `(cuiP, tip, cota)` în `op1`; `codPR` în `op11`; `(tip_partener, cota)`
  în `rezumat1` (+ `tip_N`, `document_N` la tip 2); `(tip_op2, luna)` în `op2`;
  `(caen, cota, operat)` în `lista`;
- combinații: `tip_partener` × `tip` (§4.9); `cota=0` ⇔ `tip ∈ {LS,AS,N,V}`;
  prezența/absența câmpurilor condiționate (`tva`, `tip_document`, adresa PF,
  `tvaPR`, `op11`);
- sume: fiecare atribut din `rezumat1` și `rezumat2` = Σ din `op1`/`op11`/`op2`
  („ERR – mod calcul eronat"); `nr_BF_i1`, `incasari_i1/i2` = Σ `op2`;
  `totalPlata_A` (§4.1); `op2`: `total ≥ Σ baze`, `baza{c} ≥ TVA{c}`, Σ baze > 0;
- coerența I.2: existența `serieFacturi` tip 1/2 când există L/LS; `nrFacturi*`
  ⇔ existența secțiunilor tip 2/3/4;
- I.3: bifele obligatorii ⇔ `solicit=1`;
- `rezumat1` nedeclarat pentru un `(tip_partener, cota)` prezent în `op1`.

**Avertizări (ATT)**: `nrCui1..4` vs. COUNT(DISTINCT); `nrBF/nrAMEF > 15000` sau
`≤ 1`; `op2@total ≠ Σ baze + Σ TVA`.

**Ce NU validează schema** (sinteză): raportul TVA/bază per cotă pe `op1` (spre
deosebire de D300, nu există marje de cotă); corespondența cu D300; existența
partenerului în registrul ANAF e verificată doar ca format/validitate CIF, nu ca
statut de plătitor de TVA (deși clasificarea 1/2 depinde de el — riscul e la
depunător).

---

## 8. Surse, grad de încredere, incertitudini

### Surse

| # | Sursă | Ce acoperă | Încredere |
|---|---|---|---|
| 1 | `https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_2194_2025.pdf` — ORDIN nr. 2.194 din 11.09.2025, M.Of. 852/17.09.2025, cu Anexa 1 (formularul, facsimil) și Anexa 2 (instrucțiunile), 25 pag. | §1, §2, §3 (denumirile cartușelor), §4 (semnificațiile), §5 (regulile de agregare, verbatim) | **Oficial, primar.** Descărcat și citit integral (text extras cu `pdftotext -enc UTF-8`). |
| 2 | `https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_3769_2015.pdf` — ordinul de bază, corpul (art. 1–12) + anexele originale din 2015 | antetul (actul, art. 1, 7, 8), istoricul | **Oficial, primar.** Corpul citit integral; anexele 1–2 din 2015 sunt înlocuite de sursa 1 și nu au fost folosite. |
| 3 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/d394_20250917.xml` — **XSD-ul oficial** (namespace v5, version 1.02), 1.493 linii | §3 (ordinea elementelor, cardinalități), §4 (toate atributele, tipurile, `required`, enumerările), §8 (discrepanțele) | **Oficial, primar, autoritatea pentru structură.** Citit integral. |
| 4 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/structD394_15092025.pdf` — anexa de validări („Structura fișier XML pentru declarația 394"), cu istoricul modificărilor per câmp, 2.516 linii text | §4 (formulele, condițiile null/<>null, ERR/ATT), §7, nomenclatoarele | **Oficial, primar**, dar **documentul e un tabel întreținut incremental** (rânduri tăiate/adăugate suprapuse la extragere) și conține câmpuri absente din XSD; unde diferă, XSD-ul câștigă. |
| 5 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/structD394_10052022.pdf` — versiunea anterioară a anexei de validări | confirmarea că singurele diferențe 2022→2025 sunt marcajele `01.09.25` (cotele 21/11) | **Oficial, primar.** Folosit doar pentru diff. |
| 6 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/394.html` — pagina formularului | lista pachetelor publicate (ultimul: Soft A `D394_26092025.pdf`, Soft J `D394_17092025.zip`, XSD `d394_20250917.xml`, anexă `structD394_15092025.pdf`) | **Oficial.** Confirmă că nu există pachet 2026. |
| 7 | `https://legislatie.just.ro/Public/DetaliiDocument/174685` — Portal Legislativ, ORDIN 3769/2015 | lista actelor modificatoare (1105/2016, 2264/2016, 2328/2016, 2194/2025), data ultimei consolidări 17.09.2025 | **Oficial.** Conținutul nu a fost extras de aici. |
| 8 | `https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/D394_26092025.pdf` — programul de asistență (PDF inteligent, XFA) | — | **Oficial**, dar fără text extractibil; versiunea internă neconfirmată (punctul 1). |

Surse secundare consultate doar pentru orientare, **fără conținut preluat**:
pwc.ro (HTTP 403), ceccarbusinessmagazine.ro, startupcafe.ro, permisdeantreprenor.ro,
contabilul.manager.ro, portalcontabilitate.ro.

### Grad de încredere pe secțiune

| Secțiune | Încredere | Motiv |
|---|---|---|
| §1 — ce s-a schimbat | **Foarte ridicată** | marcajele `01.09.25` din sursa 4 + diff cu sursa 5 + art. III din sursa 1 |
| §2 — cine/când/corecție | **Foarte ridicată** | verbatim Anexa 2 pct. 1–5 + art. 7–8 |
| §3–§4 — structura, atributele, tipurile, obligativitatea | **Foarte ridicată** pentru XSD; **ridicată** pentru condițiile null/<>null (din sursa 4, tabel greu de extras) | criteriul mecanic al XSD-ului; condițiile au fost reconciliate rând cu rând cu semantica formularului |
| §4.2 — formulele `rezumat1` | **Ridicată** | textul formulelor e lizibil în sursa 4; condițiile de prezență pentru `facturiLS`/`bazaLS` la `tip_partener=2` sunt transcrise literal dar formularea sursei e ambiguă |
| §5 — agregarea | **Ridicată** | regulile 1/0, semnul stornării, PF, străinii — verbatim; interpretările proprii sunt marcate |
| §6 — maparea pe model | **Sinteză proprie** | coloana „există azi?" e o listă de verificat, nu o constatare din cod (agentul n-a citit modelul) |
| §7 — validările | **Ridicată** | din sursa 4 |

### Ce a rămas incert — 8 puncte deschise

1. **Identificatorul de versiune al programului de asistență** (ex. „A7.x.x") nu
   a putut fi citit din `D394_26092025.pdf` (XFA comprimat). Se știe cert: XSD
   `version="1.02"`, namespace `v5`, fișier `d394_20250917.xml`; ultima notă de
   versiune din anexa de validări e „Vers. A6.0.0 din 10.05.2022" (nota nu a fost
   actualizată la 01.09.2025). Pentru contract: **numele fișierului XSD + data**.
2. **Discrepanță anexă ↔ XSD la declarant**: anexa listează `nume_declar`,
   `prenume_declar`, `functie_declar` și un element `<idReprezentant>`; XSD-ul are
   doar atributele `denR`/`functie_reprez`/`adresaR`… pe rădăcină. Programul de
   asistență generează după XSD; anexa e depășită aici.
3. **`rezumat2` are `maxOccurs="5"` în XSD**, dar există 7 cote posibile. În
   practică o perioadă nu poate avea toate 7 (24/20 sunt moarte), dar o firmă cu
   19, 21, 9, 11 și 5 (storno vechi + tranzitoriu + cote curente) atinge exact 5.
   Un al șaselea rând (ex. 20% pe un storno foarte vechi) ar pica validarea XSD.
   Neconfirmat dacă validatorul DUK impune limita.
4. **Codul 36 (Gaze naturale)** e în `Int_nomenclatorBunuriSType` (`detaliu@bun`)
   și e anunțat „pt tip_partener=1 și tip în (V,C)" din A6.0.0, dar **lipsește din
   `Str_listaCodPRSType`** (`op11@codPR`). Cum `detaliu` se calculează din `op11`,
   nu e clar cum se declară efectiv o livrare/achiziție de gaze naturale cu taxare
   inversă în XML validat strict de XSD. Probabil o omisiune de XSD; de testat pe
   programul de asistență.
5. **`serieFacturi@serieF`** (seria finală a plajei) există în anexă și pe
   formular, dar nu în XSD — plaja se poate exprima doar cu `serieI` + `nrI`…`nrF`
   (o singură serie per rând). Plajele multi-serie = rânduri separate.
6. **Condiția „obligatoriu pt. cota ∉ {19, 24}"** pe `baza_incasari_i1/i2` din
   anexă e text din 2016 (când 19 era cota nouă) și nu a fost actualizată pentru
   21/11; în XSD atributele sunt opționale. Regula reală probabilă: prezente pentru
   cotele care apar în `op2`.
7. **`tvaDedAI*`**: anexa spune „=null pt sistem_TVA=0", XSD-ul le face
   `required` și formularul are I.4.1 tocmai pentru sistemul normal. XSD-ul e
   coerent cu formularul; anexa e greșită aici. La fel, anexa cere `tva` pe `op1`
   „obligatoriu pt tip în (L,V,C,A,AI)", dar V n-are TVA în `rezumat1`/XSD.
8. **Semantica exactă a `efectuat`** (bifa „Persoana impozabilă nu a efectuat
   livrări… în perioada de raportare"): anexa codifică `0 = nu a efectuat`,
   `1 = a efectuat` — inversul denumirii bifei. De verificat pe un XML generat de
   programul de asistență înainte de a-l considera contract.

Neverificat, deliberat în afara scopului: lista completă `Str_coduriCaenSType`
(≈600 coduri; oricum nu e legată de atributul `caen`), nomenclatorul complet de
țări (249 coduri ISO-2, în XSD), corespondența județelor (codurile 01–40, 51, 52 —
Nomenclator 1 din sursa 4).

### Fișiere sursă (local, gitignored: `anaf/d394/`)

- `OPANAF_2194_2025.pdf` + `OPANAF_2194_2025.txt` — ordinul din 2025 cu Anexa 1 (formularul) și Anexa 2 (instrucțiunile), text integral UTF-8
- `OPANAF_3769_2015.pdf` + `OPANAF_3769_2015.txt` — ordinul de bază (corp + anexele originale, înlocuite)
- `d394_20250917.xml` — **XSD-ul oficial în vigoare** (namespace v5, version 1.02)
- `structD394_15092025.pdf` + `structD394_15092025.txt` — anexa de validări (formule, restricții, nomenclatoare)
- `structD394_10052022.pdf` + `structD394_10052022.txt` — versiunea anterioară a anexei (pentru diff)
- `D394_26092025.pdf` — programul de asistență (PDF inteligent XFA; `D394_26092025.txt` conține doar placeholder-ul Adobe)
