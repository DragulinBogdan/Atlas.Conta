# Bilanțul noii forme: ce câștigăm, ce pierdem (sinteza pașilor 1–3, pentru TR-D5)

Stare: **SINTEZĂ (2026-09-19)** peste `nucleu-cub-design.md`,
`nucleu-coordonate-rapoarte.md`, `nucleu-fizica.md`, `nucleu-transfer.md`.
Nicio cifră nouă; fiecare rând citează pasul care a măsurat-o. Termenul de
comparație e **forma de azi** (4 registre + 3 snapshot-uri + `PartideDeschise`
+ `Imperecheri` + `TotalStingere`, motor pe `IObjectSpace`) **împreună cu
evoluția ei planificată** (contractul IM), nu forma de azi înghețată.
Scopul: owner-ul decide TR-D5 pe un tabel, nu pe patru docs.

## 1. Bilanțul într-un tabel

| Dimensiune | Verdict | Cifra care îl susține | Sursa |
|---|---|---|---|
| Corectitudinea modelului (partide, stoc ↔ contabil, partener) | **câștig mare** | 87.478 „partide” fără datornic; 14.341 FCT cu rest negativ; Δ 3xx 585 k azi INVIZIBIL; balanța „pe partener” e pe sediu în 96 % din 4111 | transfer TR-D1, TR-D4; fizica §3 |
| Un mecanism în loc de patru (storno, închidere, snapshot, sold) | **câștig** | 4 registre + 3 snapshot-uri + `PartideDeschise` → `Postare` + `Sold`; 24 FK → 9; 24 indexi → 7 | fizica §2.1, transfer §3.3 |
| Izolarea motorului (ținta IM) | **câștig** | 15 hook-uri × 20 frunze devin declarație pe operand închis; cuplajul pe forma veche creștea cu 58 % în 8 zile | IM (reanalizat 2026-09-18), transfer §3.2 |
| Scrierea unui document | **câștig, dar nu din cub** | 1,7 ms contra 5,3; cu FK echivalente 4,28 ≈ F0 — câștigul e din jumătate de FK-uri | fizica §2.1 (b) |
| Citirile lunii (TVA, D394, partener, FIFO) | **câștig** | jurnal 23 → 7 ms; D394 18 → 8; fișa × partener 719 → 33 | fizica §2.1 |
| Citirile integrale de istoric (fișă, terți SAF-T, GLE) | **pierdere** | fișa 4111 de la `Sold` 348 ms contra F0 cu referință 248 (+40 %; integrală 1.017 → 1.757, FZ-r2 măsurată); terți SAF-T 600 → 959 (+60 %); TaxInformation 17 → 137 | fizica §2.4, §3.1 |
| Balanța pe snapshot | **pierdere mică** | F0 cu referință 57 ms bate F2+S 99 ms; `Sold` e 1,7–2,1× snapshot-ul de azi | fizica §2.3 |
| Disc | **pierdere** | 1,5–1,8× (11,56 M postări contra 6,72 M rânduri; 3,7 GB contra 2,1 la ×10) | fizica §2.4 |
| Coordonate noi (Carte, Valuta, Analiză ×6, Atribuit) | **câștig de capacitate** | D101, partide în valută, reevaluarea — azi ar cere al cincilea registru | coordonate §2 |
| Semantică vizibilă utilizatorului | **pierdere de continuitate** | „Documente cu rest” 63.336 → 66.658; FCL cu avans 104,02 → 16,61; SAF-T se schimbă în 4 locuri | transfer TR-D1, coordonate §2 |
| Disciplina `Fel = Transfer` | **cost permanent** | uitată într-un raport pe cont: fișa 4111 +38 %, jurnal +21,7 % | transfer TR-D2 |
| Volumul de rescriere | **cost mare, o dată** | ≈3.450 linii motor + 6.384 linii proiecții + ≈293 probe de formă + 4 grile XAF + Import1C | transfer §3.2, TR-r9 |
| Forma declarației fluxului (TR-D6b) | **necunoscuta principală** | înlocuiește 30 override-uri + 11 hook-uri; NESCRISĂ | transfer §4 |
| Ce nu s-a putut proba | **risc, nu pierdere** | valută, `Atribuit`, storno pe cub, imobilizări, împerecherea datată real, coordonatele pe ani reali | fizica §2.5, transfer §6 |
| Produsul pe durata tranziției | **cost de oportunitate** | XAF și React înghețate pe TR-D5…D10 (6 pași, fiecare felie cu contract) | design (stare) |

Citirea scurtă: **noua formă se plătește prin corectitudine și prin
unicitatea mecanismului, nu prin viteză și nu prin disc.** Cine o alege
pentru performanță se înșală pe cifre; cine o respinge pentru disc și fișă
plătește în continuare cele patru defecte de model de mai jos.

## 2. Ce câștigăm

### 2.1 Defecte de model care devin IMPOSIBILE structural (nu „reparate”)

Acestea sunt câștigul care nu se poate obține prin IM sau prin cârpirea
formei de azi, fiindcă forma le produce:

1. **Partida ca document.** 87.478 de „partide” F0 (515,7 M) stau pe
   documente care nu postează nimic pe un cont de terț (NotaTransfer,
   DescarcareGestiune); 2.036 FCL + 25 FCT au un `TotalStingere` care nu e
   sumă pe niciun cont; 14.341 FCT au rest NEGATIV (−45,5 M) fiindcă datoria
   e ruptă pe FCT/NIR; 1.085 FCT cu taxare inversă n-au unde primi 1.179
   stingeri (25,6 M). Sub partida = unitate deschisă de postarea pe terț
   (TR-D1) niciuna din cele patru nu se poate scrie.
2. **Două registre nereconciliate pentru același fapt.** Registrul de stoc
   și cel contabil diferă azi cu +585.404,66 pe 3xx (371 +404 k) și nimeni
   nu vede, pentru că nu există nicio probă care să le compare. Sub postarea
   unică (TR-D4, 1:1 cu valoare egală pe 100 % din perechile existente)
   diferența nu are unde să existe. Cine are dreptate se află abia la TR-D7
   (TR-r12) — dar azi nu se știe nici că întrebarea există.
3. **„Partener” care e repartitorul laturii.** Pe 4111, 138.414 din 144.239
   de rânduri au ca repartitor sediul, nu clientul (1.985 chei cont ×
   repartitor contra 20.190 cont × partener). Balanța analitică, soldul pe
   partener și Customers/Suppliers din SAF-T grupează de zece ori mai grosier
   decât cred. Partenerul pe postarea de terț (amendamentul 4 al pasului 1) e
   singura formă în care aceste rapoarte sunt sume.
4. **Rânduri rescrise după scriere.** `CorectieService` rescrie
   `PerioadaAn/Luna` pe rânduri fiscale deja scrise; `Lot.PretUnitar` e cifră
   înghețată lângă un registru care spune altceva după corecție. În cub
   perioada e decizie la scriere, prețul e raportul unității; niciun fapt nu
   se atinge.

La acestea se adaugă două proprietăți pe care forma de azi nu le poate
oferi: **taxa per linie exactă prin construcție** (`Repartizeaza`; SAF-T cere
`TaxAmount` obligatoriu pe fiecare linie, azi imposibil de emis fără reziduu)
și **cantitate zero pe unitate ⇒ valoare zero** (fără valoare orfană; ține
doar cu evaluare la scriere serializată).

### 2.2 Un mecanism în loc de patru

Închiderea, stornoul, snapshot-ul și soldul se scriu azi de patru ori, cu
patru forme (`RegistruContabil`/`Stoc`/`Tva`/`Imobilizari`), și încă o dată
pentru partide (`TotalStingere` + `Imperecheri` + `PartideDeschise`). Noua
formă are `Postare` + `Tranzactie` + `Sold`. Dispar: 4 registre, 3 tabele de
snapshot, `PartideDeschise`, `Imperecheri`, `TotalStingere`, `TipStoc`,
flag-ul `Storno` pe rând, `NumarNota`, 16 coloane de dimensiuni per latură,
`PoliticaConex` și mecanismul clonei, 4 hook-uri de stingere,
`IDocumentCuRegistruPropriu`, cele 6 ramuri din `ImperecheriProiectii`,
puntea 891 a importului. Pe `RegistruContabil` singur: 24 de indexi (70 MB la
55 MB de tabelă, unul per FK) contra 4 per uz + 3 de identitate; 24 de
triggere FK (82 % din timpul de inserție) contra 9.

### 2.3 Motorul pur — ținta IM, obținută altfel

IM voia faza pură, contractele de citire și commit-ul la apelant pe forma de
azi. Măsurat pe 2026-09-18, cuplajul crescuse cu 58 % în opt zile: 15
hook-uri cu `IObjectSpace` (`PregatesteOperare` 13 override-uri,
`ValideazaOperare` 20, `SensDeStins` 7…), 111 `GetObjectsQuery` în `Motor/*`,
frunzele cu 51 `GetObjectByKey`. Pe forma veche izolarea e o luptă continuă
cu fiecare frunză nouă. Pe forma nouă hook-urile nu se izolează, DISPAR:
documentul declară fluxul pe operand închis, motorul întoarce
`Contract { Postari, Decizii, Ipoteze }`. Validarea = operarea fără reținere
(o computație, două ieșiri); simularea nu mai poate minți fiindcă ipotezele
sunt în contract. Ce IM livrase (`Potrivire`, `DimensiuniResolver`,
`SolduriService` ca adaptor, `RandDupaCheie`, tranzacția la apelant) se
preia integral.

### 2.4 Capacități pe care forma de azi nu le are

`Carte` (Contabil | Fiscal) deschide D101 și registrul de evidență fiscală
fără al cincilea registru; `Valuta` + `ValoareValuta` deschid partida în
valută cu 665/765; `Atribuit` face stornoul corect după o reevaluare (DVI pe
loturi, corecția de preț, transportul pe lot); cele 6 coordonate de analiză
sunt structură, nu 12 din 16 coloane plate per latură. Imobilizările pierd al
patrulea registru: fișa e unitate, AMO postează în două cărți prin același
motor, `MaterializeazaRegistrul`/`Elimina`/`Storneaza` dispar.

### 2.5 Citirile unde cubul bate registrele

| Interogare | F0 | F2 | Sursa câștigului |
|---|---|---|---|
| Jurnal cumpărări, o lună | 23 ms | 7,4 | `Index Only Scan` pe `(PerioadaDeclarare, CodTva)`; azi `Seq Scan` cu predicat aritmetic |
| D394, o lună | 18 | 7,5 | idem |
| Fișa 4111 × partener | 719 | 33 | partenerul e coordonată, nu repartitor de latură |
| FIFO produs × gestiune | 1,0 | 0,8 | FIFO în SQL, nu N+1 în C# |
| Scrierea, 49 linii | 5,3 | 1,7 | 9 FK în loc de 24 |
| Cub contra cub, cu `Sold` | — | 4–9× | balanță 424 → 109, partide 567 → 63 |

## 3. Ce pierdem

### 3.1 Viteza pe citirile integrale de istoric

Singurele interogări unde F2 e CLAR mai lentă decât F0 la ×10, toate pe
istoric complet: fișa 4111 1.017 → 1.757 ms; terții SAF-T 600 → 959;
`TaxInformation` GLE 17 → 137; citirea unui document 0,3 → 0,7; GLE 3 → 275
(forma interogării, evitabilă). Mecanismul: 11,56 M postări contra 6,72 M
rânduri (contabilul și fiscalul se dublează prin `Latura` și rol), iar pe
fișă F0 intră pe doi indexi de cont, F2 pe `Data` și filtrează. Remediul
(fișa de la `Sold` + postările de după graniță, FZ-r2) e MĂSURAT (2026-09-19):
F2 1.726 → 348 ms la ×10, cu contrapartidă 2.255 → 940, soldul final identic;
costul nu mai depinde de lungimea istoricului. Dar F0 are același remediu
deja în cod (ramura de referință a lui `FisaCont`) și face 248 ms: pierderea
reală pe fișă e **+40 %, nu +73 %**, iar sursa ei e granul lui `Sold`
(126.263 rânduri pe 4111 la o graniță contra 1.591 în snapshot-ul de azi;
FZ-r1). Partiționarea pe an nu mai e necesară pentru fișă (F3+S 328 ↔ F2+S
348); FZ-r6 rămâne doar pentru PhysicalStock.

### 3.2 Snapshot-ul de azi e mai grosier și mai rapid pe raportul lui

`Sold` pe toate coordonatele are 321.199 rânduri pe Contabil la 31.12.2025
contra 184.780 în `SolduriPerioadaContabil` (1,7×; 2,1× la ×10). Pe balanța
sintetică F0 cu referință face 57 ms, F2+S 99 ms. Cifrele +S de la ×10 sunt
LIMITE INFERIOARE (granița ținută artificial constantă, FZ-r4). Dacă un al
doilea read model mai grosier merită e întrebare deschisă (FZ-r1).

### 3.3 Discul

1,5–1,8× discul registrelor de azi: 3.732 MB contra 2.105 MB la ×10 (cu
`Sold` pe 10 granițe 4.965 MB). Lățimea rândului e egală (182 B contra 189);
sursa e numărul de postări. Acceptat și declarat de fizica §4.6; unificarea
stocului cu 3xx (TR-D4) recuperează 8,63 % din cub, nu mai mult.

### 3.4 Continuitatea semantică, vizibilă utilizatorului și la ANAF

Fiecare e consecință DECLARATĂ a unei TR-D, dar e o schimbare pe care cineva
o va vedea:

- „Documente cu rest” își schimbă definiția: 63.336 → 66.658 partide; o FCL
  cu avans arată 16,61 (compensat în partidă), nu 104,02 (brutul de azi);
  un document cu două conturi de terț apare de două ori; documentele fără
  postare de terț dispar din listă.
- SAF-T: `InvoiceNo` dublat pe factura stornată în aceeași lună; „două
  facturi” în D394 la storno; `MovementOfGoods`/`MovementReference` se mută
  de pe NIR pe FCT; `TaxTable.TaxPercentage` iese din versiune, deci
  fișierul se schimbă acolo unde cota a fost editată.
- Balanța: soldul 3xx se mută cu +585 k (constatare, TR-r12) — până la
  tranșarea cu 1C, o cifră din balanță e alta decât ieri.
- Rulajul brut per partidă nu mai e sumă (TR-r8); fișa partidei se randează
  ca fereastră.
- Împerecherea devine operație (document `Împerechere`), nu legătură;
  invariantul I se amendează.

### 3.5 Disciplina permanentă `Fel = Transfer`

Tranzacțiile de transfer (împerecheri ulterioare, transfer între gestiuni pe
același cont) au Σ per (Cont, Latura) = 0 și trebuie EXCLUSE din orice raport
pe cont. Pe Flax ar fi 88.896 de postări: un raport care uită filtrul dă fișa
4111 +38 %, 401 +46 %, jurnalul +21,7 %. Costul nu e de implementare, e de
disciplină: orice interogare nouă pe cont poartă regula; singura apărare e
proba (DUK cu sumă negativă în `Storno`/`Transfer`, TR-D7). Pe forma de azi
convenția nu există fiindcă împerecherea nu postează.

### 3.6 Volumul rescrierii, o dată

| Ce | Cât | Cum |
|---|---|---|
| Motorul (`MotorOperare`, `StocService`, `LoturiCulegere`, `Descarcare`, `Imperechere`, `Solduri`, `Amortizare`, `RegistruTva`, `Tva`, `Corectie`) | ≈3.450 linii | rescris pe operand închis |
| Proiecțiile (`Proiectii/`, `Saft/`, `Api/*Proiectii`) | 6.384 linii | forma din coordonate §3; 8/13 deja probate identice |
| Probele de FORMĂ din ModelCheck (D17, D18, SOL, PAR, PDT, DIR, IMO parțial, JT, F28) | ≈293 din 1.450 | rescrise pe `Postare` sau șterse cu forma (TR-r9); ≈1.150 de probe de REGULĂ rămân |
| Grilele XAF pe registre | 4 | re-țintite la TR-D9 (singura dezghețare) |
| Import1C | ~15 citiri de `TipStoc`, `Deschidere`, `Bucla.Opereaza` | cu puncte de atingere numite |
| Hook-urile frunzelor | 30 override-uri + 11 hook-uri | înlocuite de declarația fluxului — forma NESCRISĂ (TR-D6b) |

Ce NU se rescrie (și e cea mai mare parte): cele 20 de frunze și culegerea,
17 tabele de politică, `Potrivire`/`DimensiuniResolver`/`Fapte`/
`ImpliciteService`, lanțul de perioade (88), `GardianEditare` (a), (d)–(o),
ușile REST/OData, DTO-urile, securitatea, seed-ul, nomenclatoarele.

### 3.7 Necunoscutele (risc, nu pierdere măsurată)

Fără date pe Flax, deci fără probă: valuta și partidele în valută,
`Atribuit`, stornoul pe cub (0 rânduri), imobilizările (registru gol),
decontul cu angajatul, avansul de furnizor, compensarea 401 = 4111, lanțul
avans → factură → regularizare, corecția legată, DVI, profilul bugetar,
custodia, recepția pe aviz. Data împerecherii e artefact de import (toate
2026-09-18), deci **toate cifrele de partidă sunt „la infinit” și proba
supremă Import1C e oarbă pe partide până la TR-r6**. Creșterea reală a
coordonatelor pe zece ani (deci mărimea lui `Sold`) e nemăsurată; costul FK
la scară, `VACUUM` sub scrieri reale și concurența cu operarea serializată la
fel (FZ-r8).

### 3.8 Produsul stă pe loc

XAF și React sunt înghețate pe durata TR-D5…D10: șase pași, fiecare felie cu
contract, review advers și Import1C ca gate. Nicio funcție nouă până la
tăiere. Restanțele cu cerere de produs (F26-r1/r8/r9/r13, F27-r11/r13/r1,
84-r5, 86-r11, 86-r13, 80-r1) așteaptă. Contractul IM (analiza din
2026-09-09 și 2026-09-18) e cost scufundat mic: ce livrase se preia, pașii
0a–0c și 1–7 nu se mai execută.

## 4. Ce NU e nici câștig, nici pierdere (ca să nu fie numărat greșit)

- **Re-proiecția**: o regulă schimbată nu re-proiectează istoricul. E așa și
  azi (registre append-only, 33d); pierderea e față de legacy, nu față de
  forma curentă.
- **Deschiderea de terți fără partener** (65,5 % din sold, 87 % pe 4111): e
  defect al conectorului 1C, nu al vreunei forme. Noua formă doar REFUZĂ să-l
  ascundă (o partidă nu se poate deschide fără partener) — devine cerință
  pentru 1C (TR-r6), nu pierdere.
- **Postgres**: partiționarea LIST, FK per partiție și `Index Only Scan` sunt
  specifice; singurul provider configurat e Postgres, deci nu e cost de
  portabilitate.
- **Scrierea**: cu FK echivalente cubul e la fel de scump ca F0 (4,28 contra
  4,63–5,47 ms). Câștigul real e al deciziei „9 FK în loc de 24”, care s-ar
  putea lua parțial și pe forma de azi.

## 5. Ce ar trebui să cântărească decizia TR-D5

1. Câștigul e **de corectitudine** (§2.1) și **de unicitate a mecanismului**
   (§2.2); e câștig de capacitate (§2.4) doar dacă D101, valuta și
   reevaluarea sunt cerințe reale de produs — altfel sunt structură plătită
   în disc.
2. Pierderea măsurată e **disc 1,5–1,8× și fișa de la `Sold` +40 % față de
   F0 cu referință** (348 contra 248 ms la ×10; FZ-r2 măsurată, fișa
   integrală de +73 % nu mai e cifra relevantă). Ce a rămas e granul lui
   `Sold` (FZ-r1): dacă un al doilea read model mai grosier merită, se
   decide pe cifră, nu înainte de TR-D6a.
3. Costul necunoscut e **declarația fluxului** (TR-D6b): dacă pilotul pe
   BCS/PLT/FCT nu produce o formă mai simplă decât cele 41 de hook-uri, noua
   formă a mutat cuplajul, nu l-a desființat. E singurul pas care poate
   întoarce decizia; de aceea vine înaintea strangler-ului.
4. Costul sigur e **timpul**: șase felii cu produsul înghețat. Strangler-ul
   per tip ca dată (TR-D7) e ce face costul reversibil pas cu pas — cubul se
   scrie LÂNGĂ registre, iar un tip nemigrat rămâne pe forma veche.
