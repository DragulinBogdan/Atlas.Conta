# E2 — depunerea fiscală: D300 și D394 în XML, SAF-T pe luna cu corecție valorică

- Tip: contract
- Data: 2026-10-15
- Trigger: owner, deschiderea etapei E2 după merge-ul TR-D9b-CV (2026-10-15)
- Livrează: F-TVA-01, F-SAFT-03 (etapa E2 din [scop](../scop.md))
- Arii: tva, saft, nomenclatoare, api, ecrane, verificare
- Decizii: 035 (c), 051 (c), 069, 071, 080, 103 (d), 104 (d), 119
- Stare: deschis

Owner-ul a aprobat la 2026-10-15 antetul pe societate (E2-D4), categoria
de bunuri pe produs (E2-D6) și includerea SAF-T pe luna cu corecție, cu
pasul de cercetare 3a înaintea codului (E2-D10).

Ținta T2 cere ca decontul, D394 și SAF-T să treacă validarea ANAF pe lunile
de probă. Azi D300 și D394 există numai ca proiecții JSON (decizia 69, 71):
cifrele sunt verificate, dar fișierul de depus nu există (69-r6, D4-r13).
SAF-T refuză luna în care o corecție valorică e operată sau stornată (CV-D9,
CV-r1). Contractul fixează forma celor două fișiere noi, sursa lor, datele
de antet care lipsesc din model, oracolul extern și reprezentarea SAF-T a
corecției.

Structura formularelor e cercetată din surse primare în
[d300-structura-2026.md](d300-structura-2026.md) și
[d394-structura-2026.md](d394-structura-2026.md). Schemele oficiale și
anexele de validare sunt descărcate local în `anaf/d300/` și `anaf/d394/`
(gitignored), la versiunile din E2-D1.

## Scop

1. Fișierul XML D300 pe schema v12, pentru perioadele din 2026.
2. Fișierul XML D394 pe schema v5 (1.02), pentru operațiunile de la
   2025-08-01.
3. Datele de antet ale declarantului, editabile o singură dată pe societate.
4. Ușile HTTP și butonul de descărcare din ecranele React D300 și D394,
   cu refuzurile înaintea primului byte.
5. SAF-T (D406 L și S) pe luna cu corecție valorică pe lot, odată cu
   SAFT-r2 (mișcarea de stoc numai valorică).
6. Oracolul: XSD-ul oficial în proces și DUKIntegrator cu validatorii
   oficiali, pe kit pin-uit.

## Ce NU intră

- Transmiterea la ANAF (SPV, semnătura electronică); confirmarea depunerii
  rămâne evidența utilizatorului (103d).
- Formularele anterioare: D300 v10 și v11 (2025), D394 înainte de
  2025-08-01. Versionarea generală a formularului (69-r1) rămâne restanță;
  E2 introduce numai registrul celor două versiuni curente.
- Perioadele semestriale și anuale. Lunarul și trimestrialul intră.
- Secțiunile pe care modelul nu le are ca sursă: D300 rândurile `Extern`
  (rămân parametri, ca azi), prorata și ajustările (F-TVA-02), TVA la
  încasare (F-TVA-03), D394 cartușul G (AMEF), I.4–I.7 și regimurile
  speciale. Ele se emit goale sau cu zero, conform schemei, și se listează
  în limite (E2-D5).
- e-Factura (F-ANAF-03) și exportul D406 pentru imobilizări sau trimestru.

## D-uri pin-uite

### E2-D1 — formularele, versiunile și perioadele

| Formular | Schema | Namespace | Perioade | Validator oficial |
|---|---|---|---|---|
| D300 | `d300_v12_11022026.xml` (XSD 1.02) | `mfp:anaf:dgti:d300:declaratie:v12` | de la 2026-01 | `D300Validator_11022026_2.zip` (Soft J) |
| D394 | `d394_20250917.xml` (XSD 1.02) | `mfp:anaf:dgti:d394:declaratie:v5` | de la 2025-08 | `D394_17092025.zip` (Soft J) |

Registrul e cod, nu date: structura formularului e a legii (principiul 4).
Fiecare rând are versiunea, data de la care se aplică și harta rând →
element XML; o versiune nouă de formular e un rând nou, nu o ramură în
serializator.

Perioada declarată e o lună sau un trimestru calendaristic întreg, după
periodicitatea societății (E2-D4). Altă perioadă, o perioadă dinaintea
versiunii suportate sau o periodicitate care nu corespunde societății dau
422 `DECLARATIE_PERIOADA_NESUPORTATA`, cu motivul. `tip_decont` și
`tip_D394` sunt `L` sau `T`, iar `luna` e ultima lună a perioadei.

### E2-D2 — sursa: XML-ul serializează proiecția existentă

Fișierul nu recitește cubul. Serializatorul primește `D300Dto` sau
`D394Dto`, calculat pe același snapshot tranzacțional, plus antetul și
parametrii cererii. Amprenta `VersiuneExportata` e aceeași ca la JSON pentru
aceeași perioadă; răspunsul XML o poartă în antetul HTTP
`X-Versiune-Exportata`, iar confirmarea depunerii o folosește neschimbată
(103d). Antetul și parametrii nu intră în amprentă: amprenta certifică
faptele, nu fișierul (limita existentă din `tva.md`). Același snapshot,
același antet și aceiași parametri dau un fișier identic byte cu byte.

### E2-D3 — rotunjirea la leu

Sumele din ambele formulare sunt lei întregi. Regula e legală, nu de profil:
fracțiunile sub 50 de bani se neglijează, iar cele de la 50 de bani în sus
se întregesc la un leu (`MidpointRounding.AwayFromZero` pe valoarea cu
semn). Rotunjirea se face pe cea mai fină unitate emisă, iar totalurile se
recalculează din valorile rotunjite:

- D300: fiecare rând de operațiuni, separat bază și TVA; oglinzile și
  totalurile (19, 30, 31, 35…45) ies din aceleași formule ale proiecției,
  aplicate pe rândurile rotunjite. Formula rămâne una singură: proiecția
  primește modul „lei” și nu are o a doua implementare a totalurilor.
- D394: fiecare `op1` și `op11`; `rezumat1`, `rezumat2`, `nrCui*` și
  `totalPlata_A` sunt sume peste valorile rotunjite emise.

Diferența dintre suma în lei și suma în bani a cubului nu e eroare și nu se
raportează pe rând; DTO-ul JSON rămâne în bani.

### E2-D4 — antetul declarantului (owner, 2026-10-15)

Datele stabile ale societății intră în `Societate`, nomenclator cules în XAF:

| Câmp nou | Folosit de | Notă |
|---|---|---|
| `Caen` | D300 `caen`, D394 `caen` | cod CAEN rev. 3, validat pe lista din XSD-ul D300 |
| `PeriodicitateTva` (`Lunar`, `Trimestrial`) | `tip_decont`, `tip_D394`, E2-D1 | implicit `Lunar` |
| `DeclarantNume`, `DeclarantPrenume`, `DeclarantFunctie`, `DeclarantAdresa` | D300 `nume_declar`, `prenume_declar`, `functie_declar`; D394 `denR`, `functie_reprez`, `adresaR` | reprezentantul legal care semnează; adresa goală ia adresa societății |
| `IntocmitDe` (persoană juridică sau fizică), `IntocmitDenumire`, `IntocmitCif`, `IntocmitCalitate` | D394 `tip_intocmit`, `den_intocmit`, `cif_intocmit`, `calitate_intocmit`/`functie_intocmit` | implicit societatea însăși |
| `OptiuneConsultareD394` | D394 `optiune` | opțiunea anuală a societății |

Restul antetului există: denumirea, CUI-ul (fără prefixul `RO`), adresa,
telefonul, e-mailul, contul bancar (IBAN) și banca din contul propriu.

Datele care țin de perioadă sunt parametrii cererii, cu implicite: D300
`temei` (0), `solicit_ramb` (`N`), `pro_rata` (0), `depusReprezentant` (0),
cele patru bife de taxare inversă internă (`N`; din `op11` acolo unde
D394 are categoria de bunuri), parametrii `Extern` de azi; D394
`prsAfiliat` (0) și `schimb_optiune`. `bifa_interne` se propune din date
(1 dacă rândurile 1–8 sunt nule) și rămâne parametru. `nr_evid` se
calculează din regula anexei: codul obligației, perioada, scadența (ziua
25 a lunii următoare perioadei) și cifra de control.

Un câmp obligatoriu lipsă dă 422 `DECLARATIE_ANTET_INCOMPLET`, cu lista
câmpurilor, înaintea primului byte. Nu se emite valoare de rezervă: SAF-T
avertizează și completează orașul, declarația nu.

### E2-D5 — completitudinea: ce blochează fișierul

Fișierul se refuză cu 422 `DECLARATIE_DATE_NEINCLUSE` cât proiecția are sume
pe care formularul nu le-ar conține:

- D300: oricare `Nemapate` cu bază sau TVA nenule;
- D394: oricare `Neincluse` și avertismentele care ar da eroare la validator
  (`Tip1FaraCui`, `PfFaraCnp`, `TvaPeTipFaraColoana`, `CotaNeintreaga`,
  `FaraOp11`, `CombinatieRefuzata`). Lista finală se fixează la pasul 2,
  pe validator, fiecare cod cu un mutant care îl probează.

Avertismentele de atenționare ale formularului (marjele de cotă din D300,
`CuiUnit`, `ClasificariDiferite`, `PartenerInactiv`) nu blochează și se
arată lângă buton. Secțiunile fără sursă din „Ce NU intră” se emit goale și
apar ca limită permanentă a exportului, în ecran și în `limite-curente.md`.
O societate cu asemenea operațiuni nu poate folosi exportul ca declarație
completă; produsul spune asta, nu o ascunde.

### E2-D6 — categoria de bunuri pentru taxarea inversă internă (owner, 2026-10-15)

`op11` cere, pe livrările și achizițiile cu taxare inversă internă
(tipurile `V`/`C`), categoria de bunuri (`codPR`, lista
`Str_listaCodPRSType` din XSD) și codul NC al produsului (D4-r5).
`Produs` are deja `CodNc` și primește câmpul `CategorieTaxareInversa`, cu
lista schemei ca enum. Linia V/C pe un produs fără categorie sau fără cod NC
e refuzată la export (`FaraOp11` devine blocant, E2-D5). Categoria nu se
deduce din tipul de material sau din cont.

### E2-D7 — D394 I.2, plaja de facturi (de fixat la pasul 2)

`serieFacturi` și `facturi` descriu plajele alocate și facturile emise,
anulate sau stornate (D4-r7). Sursa propusă e `PoliticaNumerotare` a
facturii de vânzare (seria) și facturile FCL operate în perioadă (primul și
ultimul număr pe serie). Pasul 2 citește instrucțiunile și XSD-ul pe
fiecare atribut, înaintea codului. Dacă modelul nu are sursa pentru un
atribut obligatoriu, regula de oprire se aplică, nu se inventează plaja.

### E2-D8 — ușile și ecranele

- `GET api/proiectii/d300/xml` și `GET api/proiectii/d394/xml`, cu aceiași
  parametri ca proiecția JSON plus parametrii din E2-D4. Accesul e cel al
  proiecției fiscale (citire pe `Postare`); refuzurile au ordinea
  401 → 400 → 404 → 403 → 422 și corpul `EroriDto` (80).
- Fișierul e `application/xml; charset=utf-8`, cu numele
  `D300_<CUI>_<AAAA><LL>.xml` și `D394_<CUI>_<AAAA><LL>.xml`.
- DTO-ul JSON primește `RefuzuriXml` (codurile E2-D4/D5 pe perioada
  cerută), ca ecranul să arate refuzul înaintea descărcării și să țină
  butonul inactiv, ca la SAF-T.
- Ecranele React D300 și D394 primesc butonul „Descarcă XML” și parametrii
  de perioadă din E2-D4. Ele sunt proiecții, nu pagini de detaliu înghețate
  (104d). Câmpurile noi din `Societate` și `Produs` se culeg în XAF.

### E2-D9 — oracolul

Două porți, ambele pe kit pin-uit, ca la D406 (`ManifestD406`):

1. XSD-ul oficial, validat în proces (`System.Xml.Schema`), cu SHA-256-ul
   fișierului în manifest.
2. DUKIntegrator cu validatorii Soft J din E2-D1, în același `dist` cu kitul
   D406, cu SHA-256 pe fiecare jar. Fișierul de erori trebuie să conțină
   literal `ok`; atenționările se listează și nu blochează. Kit absent sau
   nepotrivit manifestului înseamnă respingere, nu trecere.

Proba e un catalog nou de scenarii, `DECL` (`SC-DECL-NN`), scris la pasul 1,
rulat prin `verifica.ps1 -Suita Scenarii -Tip DECL` pe ambele profiluri:

- lunile din scenariile fiscale existente (2026-05…07), o lună fără
  operațiuni și un trimestru;
- fiecare valoare din XML recalculată independent din DTO după E2-D3;
- mutanți: un rând modificat, un total rupt, un CUI invalid, un `codPR`
  lipsă; fiecare trebuie respins de poarta la care se așteaptă;
- reexportul aceleiași perioade dă același fișier.

Pe profilul bugetar proba rulează pe datele de TVA ale profilului, dacă
există, altfel pe calea fără operațiuni; nu există ramură de profil.

### E2-D10 — SAF-T pe luna cu corecție valorică (owner, 2026-10-15: cercetare, apoi alegere)

Luna cu corecție rămâne refuzată până la închiderea pasului 3a. Pasul 3a e
documentar și se probează cu validatorul D406 existent, pe fișiere-mutant:

- **Factura corectivă** (`PurchaseInvoices`): linia de corecție a FCT, cu
  cantitatea comercială 0 și valoarea ei, pe tipul facturii primite
  (`380` sau `384` din `Nom_Tipuri_facturi`). Se verifică dacă validatorul
  primește linia cu cantitate 0. Cantitatea fictivă (1) e exclusă de owner
  (CV-D9).
- **Mișcarea de stoc numai valorică** (`MovementOfGoods`, SAFT-r2): numai
  partea alocată stocului rămas, cu codul 90 („Cheltuieli ulterioare
  incluse în valoarea de intrare”) pentru corecția pozitivă și 60
  („Reduceri comerciale primite”) pentru cea negativă; alternativa e
  100/101 („Diferențe de preț”). Partea compensată pe consum rămâne numai
  în GL, sub jurnalul FCT (CV-D9).
- **Storno-ul**: perechile de compensare se separă de liniile comerciale,
  ca factura stornată să nu poarte compensarea ca linie de factură.

Owner-ul alege codurile la închiderea pasului 3a; pasul 3b le implementează
și ridică refuzurile `SAFT_SURSA_INCOMPLETA`, `SAFT_PROVENIENTA_AMBIGUA` și
`SAFT_MISCARE_VALORICA` numai pentru formele acoperite. Rândurile SC-CVL-32
și SC-SAFT corespunzătoare trec din refuz în export verificat.

## Restanțe

E2 preia 69-r6 (XML D300), D4-r13 (XML D394), D4-r5 (`op11`, avertismentul `FaraOp11`), CV-r1
și SAFT-r2; parțial D4-r7 (I.2, după E2-D7). 69-r1 rămâne deschisă pentru
formularele anterioare. Restanțele noi ale etapei se numesc `E2-rN`.

## Pași

| Pas | Conținut | Probă | Stare |
|---|---|---|---|
| 0 | contractul, scopul E2, catalogul funcționalităților, sursele ANAF descărcate | docskit verde; owner pe E2-D4, E2-D6, E2-D10 | închis 2026-10-15 |
| 1 | `Societate` (E2-D4) cu migrația și XAF; registrul formularelor; D300 XML cu rotunjirea (E2-D3); kitul D300 pin-uit; SC-DECL pentru D300 | ModelCheck ambele profiluri; XSD + DUK pe lunile de probă | închis 2026-10-15 |
| 2 | D394 XML; `Produs.CategorieTaxareInversa` (E2-D6); I.2 (E2-D7); lista blocantă (E2-D5); kitul D394 | idem, cu mutanții D394 | |
| 3a | SAF-T pe luna cu corecție: cercetarea formei pe validatorul D406 | fișiere-mutant DUK; hotărârea owner-ului | |
| 3b | implementarea formei alese; refuzurile ridicate pentru formele acoperite | SC-CVL-32, SC-SAFT noi, DUK D406 L și S | |
| 4 | ușile HTTP, `RefuzuriXml`, ecranele React, openapi | `refuzuri.ps1` pe host viu; browser | |
| 5 | regresia integrală, review advers, închiderea | integrala ambele profiluri; nucleu; HTTP; docskit | |

## Regula de oprire

Lucrul se oprește și revine la owner când:

1. validatorul oficial cere o valoare pe care modelul nu o are și singura
   cale de trecere ar fi o valoare inventată (plajă de facturi, categorie
   de bunuri, cantitate fictivă);
2. totalurile recalculate din rândurile rotunjite nu trec validarea
   formularului, deci E2-D3 e greșit;
3. o schimbare de schemă depășește câmpurile din E2-D4 și E2-D6;
4. validatorul D406 respinge orice formă a corecției fără cantitate
   fictivă (pasul 3a);
5. un pas consumă de două ori estimarea lui fără probă verde.

## Review advers și ce rămâne deschis

La închidere, un agent nou, numai citire, verifică fiecare D-uri contra
codului și a fișierelor validate; constatările se repară înaintea PR-ului.
Un review Codex se propune owner-ului într-o linie, la închidere.

## Verificarea pasului 1 — 2026-10-15

- `verifica.ps1 -Suita Scenarii -Tip DECL -Profil Ambele -Sufix .ClaudeCVL`:
  verde, `run-verificari/20261015-163901-958`. Fișierele SC-DECL-01, 03 și 04
  trec XSD-ul v12 și DUKIntegrator cu D300Validator J12.0.1, fără
  atenționări. Validatorul recalculează totalurile și `totalPlata_A` și dă
  aceleași cifre ca fișierul.
- Mutanții SC-DECL-05 pică pe regula așteptată: rd. 9 TVA + 1 pe R66 (rd. 19)
  și pe R26 (suma de control), CUI-ul cu cifra de control greșită pe „CUI
  invalid”, `tip_decont` X pe XSD.
- `verifica.ps1 -Suita Integral -Profil Ambele -Sufix .ClaudeCVL`: verde,
  `run-verificari/20261015-164226-472` (4.083 de verificări pe bugetar,
  5.536 pe privat, zero eșecuri).
- `verifica.ps1 -Suita Nucleu`: 252 de teste verzi.
- `pnpm gen:openapi` și `pnpm gen:types`: driftul e `D300Dto.RefuzuriXml`,
  `RefuzDeclaratie` și câmpurile noi ale `Societate`; `tsc -b` verde.
- `totalPlata_A` e suma atributelor R; anexa numerotează câmpurile 27…124 pe
  numerotarea istorică, iar validatorul confirmă regula.
- XSD-ul D394 și validatorul D394 J8.0.2 sunt pin-uite; folosirea lor e a
  pasului 2.
