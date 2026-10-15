# E2 — depunerea fiscală: D300 și D394 în XML, SAF-T pe luna cu corecție valorică

- Tip: contract
- Data: 2026-10-15
- Trigger: owner, deschiderea etapei E2 după merge-ul TR-D9b-CV (2026-10-15)
- Livrează: F-TVA-01, F-SAFT-03 (etapa E2 din [scop](../scop.md))
- Arii: tva, saft, nomenclatoare, api, ecrane, verificare
- Decizii: 035 (c), 051 (c), 069, 071, 080, 103 (d), 104 (d), 119, 122
- Stare: închis 2026-10-15

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
(103d). Amprenta și confirmarea există numai pe lună; trimestrul se exportă
fără amprentă (E2-r1). Antetul și parametrii nu intră în amprentă: amprenta certifică
faptele, nu fișierul. Fișierul are amprenta lui (SHA-256, antetul
`X-Sha256-Fisier`) și parametrii din care se reface (`X-Parametri-Fisier`);
confirmarea care le primește reface fișierul și îl păstrează numai când
amprenta coincide (122 i). Același snapshot, același antet și aceiași
parametri dau un fișier identic byte cu byte.

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
- D394: fiecare `op1`; `op11` își împarte `op1` rotunjit prin
  `Repartizare.Hamilton` la scara leului, separat pe semne (122 b);
  `rezumat1`, `rezumat2`, `nrCui*` și `totalPlata_A` sunt sume peste
  valorile rotunjite emise.

Diferența dintre suma în lei și suma în bani a cubului nu e eroare și nu se
raportează pe rând; DTO-ul JSON rămâne în bani. Diferența dintre rotunjirea pe
partener (D394) și rotunjirea o dată a totalului (D300) se arată pe tip ×
cotă în `D394Dto.DiferenteRotunjire`, ca informare (122 b).

### E2-D4 — antetul declarantului (owner, 2026-10-15)

Datele stabile ale societății intră în `Societate`, nomenclator cules în XAF:

| Câmp nou | Folosit de | Notă |
|---|---|---|
| `Caen` | D300 `caen`, D394 `caen` | patru cifre; lista CAEN din XSD o verifică validatorul ANAF (E2-r6) |
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

Un câmp obligatoriu lipsă sau peste lungimea schemei (telefonul 15, banca 50)
dă 422 `DECLARATIE_ANTET_INCOMPLET`, cu lista câmpurilor, înaintea primului
byte. Nu se emite valoare de rezervă: SAF-T
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
`Str_listaCodPRSType` din XSD). Pentru cereale și plante tehnice codul e
chiar poziția NC (1001…10086000), agregată în rezumat la bunul 21; restul
sunt categoriile 22–35. `Produs` primește câmpul `CategorieTaxareInversa`, cu
lista schemei ca enum (D4-r5). Linia V/C pe un produs fără categorie e
refuzată la export (`FaraOp11` devine blocant, E2-D5). Categoria nu se
deduce din tipul de material, din cont sau din codul NC.

### E2-D7 — D394 I.2, plaja de facturi (owner, 2026-10-15)

`serieFacturi` tip 1 e plaja alocată prin decizie internă, iar tip 2 sunt
facturile emise în perioadă din plajă (D4-r7). Plaja e un nomenclator nou,
`PlajaFacturi`: seria (poate lipsi), numărul inițial, numărul final și data
deciziei, cu mai multe plaje pe serie și istoric. Două plaje active ale
aceleiași serii nu se suprapun. Pentru o perioadă se declară plajele active
cu decizia până la sfârșitul ei.

Factura emisă e documentul cu număr propriu și fapte de livrare în perioadă (122 f). Numărul
ei se leagă de plajă prin prefix și cifrele finale: prefixul e seria, cifrele
cad în interval. Tip 2 poartă, pe fiecare plajă, primul și ultimul număr
emis; `nrFacturi` e numărul facturilor emise. O factură emisă care nu cade în
nicio plajă activă blochează fișierul (`DECLARATIE_DATE_NEINCLUSE`, cu
numărul ei). Facturile anulate și autofacturile (`facturi`) nu au sursă în
model și rămân limită.

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
   literal `ok`; în catalog, fișierele de scenariu trec și fără atenționări
   (N4), iar mutanții sunt respinși la poarta așteptată. În produs,
   atenționările formularului nu opresc fișierul (E2-D5). Kit absent sau
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

Owner-ul a ales la 2026-10-15, după pasul 3a: factura urmează regula comună a
tipului, ca orice FCT (380; 381 pe storno sau pe total negativ), cu linia de corecție la cantitate 0 și informație fiscală nefiscală
(`000`/`000000`); mișcarea de stoc are codul 90 pentru corecția pozitivă și
60 pentru cea negativă. Pasul 3b le implementează
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
| 2 | D394 XML; `Produs.CategorieTaxareInversa` (E2-D6); I.2 (E2-D7); lista blocantă (E2-D5); kitul D394 | idem, cu mutanții D394 | închis 2026-10-15 |
| 3a | SAF-T pe luna cu corecție: cercetarea formei pe validatorul D406 | fișiere-mutant DUK; hotărârea owner-ului | închis 2026-10-15 |
| 3b | implementarea formei alese; refuzurile ridicate pentru formele acoperite | SC-CVL-32, DUK D406 L și S | închis 2026-10-15 |
| 4 | ușile HTTP, `RefuzuriXml`, ecranele React, openapi | `refuzuri.ps1` pe host viu; browser | închis 2026-10-15 |
| 5 | regresia integrală, review advers, închiderea | integrala ambele profiluri; nucleu; HTTP; docskit | închis 2026-10-15 |

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

Review-ul de închidere (agent nou, numai citire, 2026-10-15) a găsit 2 MAJOR,
4 MEDIU și 4 MINOR. Toate sunt tratate:

| # | Grad | Constatare | Rezolvare |
|---|---|---|---|
| M1 | MAJOR | stornoul comercial era numărat ca factură emisă: `nrFacturi` și plaja tip 2 greșite, refuz fals pe plaja dezactivată | facturile emise exclud stornoul (122 f); SC-DECL-11; anulatele și autofacturile rămân E2-r4 |
| M2 | MAJOR | un rol fără citire pe `Document`, `PlajaFacturi` sau `Societate` pierdea I.2 tăcut sau primea 422 în loc de 403 | ușile cer citirea acestor tipuri (403); factura emisă invizibilă dă refuz |
| D1 | MEDIU | trimestrul n-are amprentă, deci nici confirmare | limită scrisă (E2-r1); antetul `X-Versiune-Exportata` lipsește când amprenta e nulă |
| D2 | MEDIU | telefonul (15) și banca (50) peste schemă treceau; CAEN-ul nu era verificat pe listă, deși E2-D4 o spunea | lungimile intră în refuzul de antet; textul E2-D4 corectat, lista rămâne E2-r6 |
| D3 | MEDIU | sub-linia cotei din SAF-T se aplica pe orice linie fiscală | restrânsă la loturile cu o corecție de valoare |
| D4 | MEDIU | limitele spuneau „fără ușă HTTP” după pasul 4 și sugerau o detecție a trimestrului cumulat | textul corectat; E2-r2 |
| N1 | MINOR | gardianul plajelor și potrivirea normalizau seria diferit; o plajă dezactivată în același commit era citită activă | `PlajaFacturi.SerieNormalizata`, comună; gardianul citește întâi obiectele modificate |
| N2 | MINOR | `RefuzuriXml` din JSON (bani) și din XML (lei) puteau diferi pe `TvaPeTipFaraColoana` | avertismentul se judecă pe TVA-ul dinaintea rotunjirii |
| N3 | MINOR | `Neincluse` D394 cu net zero nu refuza | orice rând neinclus refuză |
| N4 | MINOR | oracolul: harta comparată cu ea însăși, atenționările DUK neasertate, corecția negativă neprobată | harta transcrisă separat în probă; „fără atenționări” asertat; diminuarea în SC-CVL-32 (381, cod 60) |

Restanțele noi sunt E2-r1…E2-r6 (decizia 122). Un review Codex se propune
owner-ului într-o linie, la merge.

## Discuția deciziei 122 — 2026-10-15

Owner-ul a trecut prin decizie după pasul 5 și a hotărât:

| Punct | Hotărârea | Unde |
|---|---|---|
| factura emisă | definită prin numărul propriu; stornoul rămâne afară cât modelul nu-i dă număr (E2-r4) | 122 f, E2-D7 |
| D394 față de D300 | diferența de rotunjire pe tip × cotă se afișează pe ecranul D394, fără refuz | 122 b, `D394Dto.DiferenteRotunjire` |
| restul pe bunuri | regula de repartizare a nucleului (`Repartizare.Hamilton`), nu restul pe bunul cel mai mare, care putea întoarce semnul unui bun | 122 b, SC-DECL-12 |
| fișierul confirmat | amprenta și parametrii fișierului se păstrează la confirmare, în E2; serverul reface fișierul și refuză altă amprentă | 122 i, migrația `FisierDepunere`, SC-DECL-14 |
| refuzul pe date invizibile | 403 pe tipul fără citire, 422 pe factura ascunsă pe obiect | 122 c |
| oracolul | „fără atenționări” e regula catalogului, nu a exportului | 122 h, E2-D9 |
| antetul fișierului | intră în parametri, ca fișierul depus să rămână reproductibil după schimbarea datelor societății | 122 i |
| confirmarea greșită | reconfirmarea aceleiași versiuni înlocuiește confirmarea activă; retragerea cu motiv, refuzată pe D300 cât regularizările se sprijină pe ea | 122 i, migrația `RetragereDepunere`, SC-DECL-15, SC-CIT-84 |

Verificare: `--scenarii DECL,FISCALE` verde pe ambele profiluri
(`run-verificari/20261015-211428-473`); calea HTTP pe baza E2P4
(`run-verificari/e2-122-depunere.ps1`): pe D300 și D394, amprenta din antet
e SHA-256 al corpului, amprenta fără parametri, parametrii stricați și
retragerea fără motiv dau 400, confirmarea după schimbarea telefonului
societății trece, confirmarea păstrează amprenta și parametrii, reconfirmarea
fără fișier înlocuiește confirmarea, retragerea scoate luna din „confirmată”,
iar reconfirmarea cu fișier o readuce. În browser (clientul pe baza E2P4):
după „Descarcă XML” ecranul arată începutul amprentei fișierului; pe D300 mai
2026 butonul de retragere stă inactiv fără motiv, retragerea cu motiv lasă
luna neconfirmată, iar descărcarea și confirmarea XML o confirmă din nou.
Integrala ModelCheck e verde pe ambele profiluri (`run-verificari/20261015-211740-457`).
Tabelul diferențelor de rotunjire e probat pe DTO (SC-DECL-13); luna din
E2P4 nu are diferențe, deci tabelul nu s-a randat în browser.

La 2026-10-16 owner-ul a aprobat decizia 122 și D-urile rămase de confirmat
(E2-D1, E2-D2, E2-D3, E2-D5, E2-D8, E2-D9), așa cum sunt implementate.

## Verificarea pasului 5 — 2026-10-15

- Înaintea review-ului: `verifica.ps1 -Suita Integral -Profil Ambele -Sufix
  .ClaudeCVL` verde, `run-verificari/20261015-181028-346` (4.088 bugetar,
  5.566 privat).
- După reparații: `--scenarii DECL,CVL` verde pe ambele profiluri; integrala
  verde în `run-verificari/20261015-182816-530` (4.088 bugetar, 5.573
  privat); `refuzuri.ps1` 334/334 PASS pe o bază nouă din seed; calea de
  succes reluată (200 pe ambele uși, amprenta egală cu JSON-ul); nucleu
  252/252; docskit `check` și `metrics --check` verzi.

## Verificarea pasului 4 — 2026-10-15

- `run-verificari/e2-pas4-http.ps1` (bază privată nouă din seed pe 5446,
  WebApi pe 5089): `refuzuri.ps1` 334/334 PASS. Probele noi: 400 fără
  perioadă și pe `temei` invalid, 403 pentru `User`, 422
  `DECLARATIE_ANTET_INCOMPLET` pentru Admin pe societatea goală, pe ambele uși.
- Calea de succes (`run-verificari/e2-pas4-succes.ps1`, antetul completat prin
  SQL): ambele uși dau 200 `application/xml; charset=utf-8`, cu numele
  `D300_4221306_202605.xml` / `D394_4221306_202605.xml`, iar
  `X-Versiune-Exportata` e egal cu `VersiuneExportata` din JSON. Fișierele
  descărcate trec DUK (`ok`) pe D300 și D394.
- Browser (client Vite pe aceeași bază): butonul „Descarcă XML” produce
  fișierul cu numele serverului; bifa „Temei” ajunge în fișier ca
  `temei="2"`; cu CAEN-ul lipsă, D394 arată panoul
  `DECLARATIE_ANTET_INCOMPLET` și butonul inactiv. Textele vechi de sub grile
  („fișierul XML e altă unealtă”) sunt înlocuite.
- Proba a găsit terminatorul de linie al sistemului (CRLF pe Windows) în
  fișier; `XmlDeclaratie` scrie acum LF pe orice platformă.
- `pnpm gen:openapi` și `gen:types`: driftul e cele două uși noi; `tsc -b`
  verde.

## Verificarea pasului 3b — 2026-10-15

- `PoliticaMiscareSaft` primește cauza în cheie (migrația `CauzaMiscareSaft`,
  ambele indexuri unice). Seed-ul privat adaugă FCT × Magazie/Mărfuri ×
  ±1 → 90/60 și FCT × Consum × ±1 exclus cu motiv: 29 de rânduri.
- Exportul L emite linia de corecție pe fiecare cont de contrapartidă, cu
  cantitate 0 și `000`/`000000`; cota corecției din RLF devine a doua linie
  nefiscală. Exportul S dă liniei de corecție codul politicii pe cauză.
- SC-CVL-32, `--scenarii CVL` pe privat: toate verificările verzi; L și S pe
  ianuarie, februarie și martie 2042 trec XSD-ul și DUK J2.2.18, fără
  atenționări. Prima rulare a arătat două goluri, reparate în pas: partea pe
  consum n-avea politică (`SAFT_MISCARE_FARA_POLITICA` pe FCT/Consum) și RLF-ul
  pe lot corectat nu-și închidea brutul (−262 față de −222, cota de 40).
- Restul liniilor numai valorice (NIR cu diferență, TVA capitalizat, DVI)
  rămân refuz: SAFT-r2 se restrânge, nu se închide.
- Integrala: bugetar verde în `run-verificari/20261015-175042-623` (4.088 de
  verificări). Pe privat, prima rulare a picat pe patru verificări vechi
  scrise pe cheia fără cauză (23 de politici, oracolul SC-SAFT-49). Oracolul
  citește acum politica comercială, iar numărul e 29. A doua rulare e verde
  în `run-verificari/20261015-175746-911` (5.566 de verificări).

## Cercetarea pasului 3a — 2026-10-15

Fișierele-mutant pornesc de la exporturile valide ale scenelor SAFT
(L august 2026, S martie 2027) și trec prin DUKIntegrator AnLunaUI J2.2.18,
kitul pin-uit al D406. Scriptul și rezultatele sunt în `run-verificari/e2-3a/`.

| Mutant | Rezultat |
|---|---|
| L: linia de corecție 371, cantitate 0, preț 0, valoare 200, fără `TaxInformation`, factura 380 sau 384 | respins: `TaxInformation` e obligatoriu pe linie |
| L: aceeași linie fără `Quantity` | respins: `Quantity` e obligatoriu |
| L: cantitate 0, `TaxInformation` nefiscal (`000`/`000000`, TVA 0), factura 380 | acceptat, fără atenționări |
| L: același, factura 384 | acceptat, fără atenționări |
| S: mișcare cu cantitate 0 și valoare ±200 pe lot, codurile 90, 60, 100 sau 101 (în `MovementTypeTable`), poziția de stoc cu valoarea finală ajustată | acceptat pe toate patru |
| S: aceeași mișcare fără `Quantity` | respins: `Quantity` e obligatoriu |

Concluzii: cantitatea fictivă nu e necesară (regula de oprire 4 nu se
aplică). Linia de corecție poartă cantitatea 0 și informația fiscală
nefiscală, fiindcă corecția din E1 nu are TVA (CV-D1). Validatorul nu
deosebește 380 de 384 și nici codurile de mișcare între ele: alegerea e de
semantică, nu de validare.

## Verificarea pasului 2 — 2026-10-15

- `verifica.ps1 -Suita Scenarii -Tip DECL`: SC-DECL-07…10 verzi pe privat;
  SC-DECL-08 și 09 și pe bugetar. Fișierele D394 trec XSD-ul v5 și
  DUKIntegrator cu D394Validator J8.0.2, fără atenționări; mutanții
  SC-DECL-10 pică la poarta așteptată.
- Validatorul a cerut `baza_incasari_i1`/`i2` și TVA-ul lor pe fiecare
  `rezumat2` cu cotă diferită de 24 (regulile R105.1…R108.1); ies cu zero.
- Integrala: bugetar verde în `run-verificari/20261015-170404-118` (4.088 de
  verificări). Pe privat, prima rulare a picat pe verificarea veche D4-r14:
  `FaraOp11` excludea linia cu cod NC. Verificarea cere acum categoria de
  bunuri (E2-D6). A doua rulare e verde în `run-verificari/20261015-171050-379`
  (5.557 de verificări).
- `pnpm gen:openapi`, `gen:types` și `--dump-metadata`: driftul e
  `PlajaFacturi`, `Produs.CategorieTaxareInversa`, `D394Dto.SeriiFacturi`,
  `NrFacturi`, `RefuzuriXml` și `D394Operatiune.Bunuri`; `tsc -b` verde.

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
