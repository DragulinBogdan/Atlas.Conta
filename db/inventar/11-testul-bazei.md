# Testul bazei (pasul 2 din plan) — câmpurile `Document` / `DocumentDetaliu`

Intrare: specificațiile 00–10 + deciziile CLAUDE.md. Criteriul (decizia 2),
aplicat per câmp:

> Apare într-o formulă de stoc sau regulă contabilă → **bază**.
> Apare doar pe ecranul unui tip de document → **derivată**.

Două completări de criteriu, necesare pe teren (nu modifică decizia, o
operaționalizează):

- **Câmpuri structurale/de ciclu de viață** (identitate, stare, legături conex)
  nu apar în formule, dar sunt consumate de MOTOARE transversale (operare,
  anulare pe grup conex, gardianul de perioadă — deciziile 14/17). Le tratăm ca
  bază pe același temei: motorul consumă doar clasa de bază.
- **Câmpuri legacy care mor** (înlocuite de entități sau de registre) se
  marchează explicit — nu intră în nicio clasă.

## 1. Verdictele — header (`Document`)

| Câmp legacy | Verdict | Justificare (unde apare în reguli) |
|---|---|---|
| `ID_PREDATOR` / `ID_PRIMITOR` | **BAZĂ** | Motorul de stoc alege latura afectată prin `PREDATOR=1/2` (00 §4); motorul de note le folosește ca `repartitorDebit/Credit` (00 §5). Ambele motoare consumă exclusiv aceste două FK-uri de pe header. |
| `NR_DOCUM`, `DATA_DOCUM` | **BAZĂ** | Identitate + `DATA_DOCUM` e pivotul gardienilor: perioadă fiscală închisă (00 §8), stoc „la dată", data rândurilor de registru (decizia 14). |
| `STARE` (+ `DATA_STERGERE`, `ID_UTILIZATOR_STERGERE`) | **BAZĂ** (transformat) | Devine ciclul de viață `Stare: Draft→Operat→Stornat` + audit (decizia 14). Toate query-urile legacy de stoc/note filtrează `STARE=1` — în modelul nou filtrarea o preiau registrele, dar starea rămâne pe bază (motorul de operare o consumă). |
| `ID_INITIAL`, `ID_DOCUMENT_CONEX`, `ID_TRANZACTIE`, `AUTOGENERAT` | **BAZĂ** (comprimat) | Anularea operează pe TOT grupul conex (00 §8) — motor transversal ⇒ legătura trebuie vizibilă pe bază. Se comprimă în: `DocumentSursaId` (FK nullable self, sursă→generat, decizia 17) + `Autogenerat`. Rădăcina/grupul se derivă din lanț; nu persistăm trei id-uri redundante. |
| `ID_MODIFICARE` | **MOARE** | Lanțul de versiuni e înlocuit de registre append-only + storno (decizia 14, 00 §2). |
| `TOTALDOC` | **BAZĂ, calculat** | Consumat de imperechere (`totaldoc`/`asignat`/`ramas`, 09 §2) și de plata automată. Devine proprietate calculată `Total = Σ Detalii.Valoare` — funcționează pe bază DOAR pentru că `Valoare` e în baza detaliului (§3). Diferența hardcodată per tip (tip 12 pe `VALOARE_LIVRARE`, restul pe `VALOARE_RECEPTIE_TVA` — 00 §10) dispare prin unificarea `Valoare`. |
| `TOTALTVA` (= Σ valoare × curs, „nu e TVA" — 00 §10) | **MOARE** | Denumire înșelătoare, nefolosit de vreo regulă reală; echivalentul valutar e treabă de raportare pe derivată. |
| `DATA_SCADENTA` | **derivată, prin interfață** | Nu apare în nicio regulă de stoc/notă. E trăsătura tipurilor de creanță/obligație: `FacturaIntrare` (scadența furnizorului) și `FacturaIesire` (formulă politică `data+30` — 07). Imperecherea lucrează pe totaluri, nu pe scadență. TRANȘAT (proprietar): interfață `IDocumentCuScadenta` pe ambele Facturi — contract ferm, nu dublare liberă. |
| `NR_NOTA` / `DATA_NOTA` | **MUTAT pe registru** | Numărul notei contabile aparține rândurilor de registru contabil generate la operare, nu documentului. Hack-ul „utilizator 402 ⇒ NR_NOTA='luna/MAG'" (00 §10) moare. |
| `DESCRIERE_CURS`→NR_PV, `DATA_EMITERE`→DATA_PV, `DECONT_DATA`/`DATA_CHITANTA`→DATA_PV | **derivată** | Refolosiri confirmate (00 §11). Doar ecran (FCT, BTR, DEC). Devin câmpuri proprii, corect denumite (`NumarPV`, `DataPV`) pe derivatele care le au. |
| `COD_CPV` (header) | **derivată** | Doar ecran FCT — atribut de achiziție. |
| Grup `DECONT_*` (8 câmpuri) | **derivată, persistat** | Parametrii generării plății automate (00 §7). TRANȘAT (proprietar): câmpuri PERSISTATE pe `FacturaIntrare` (nu dialog non-persistent) — parametrii generării documentului conex `Plata` rămân pe factură ca evidență a intenției. |
| Valută / curs (FCT externe) | **derivată** | Nicio regulă de stoc/notă nu consumă cursul (registrele sunt în RON). `FacturaIntrare` poartă valuta/cursul pentru afișare + calculul valorilor RON la culegere. |
| `NR_EXTRAS`/`DATA_EXTRAS`, `TIPDOC` plată, `TRANSFER`/`COD_CBT` | **derivată** | Doar pe `Plata`/`Incasare` (09). Contul propriu (`CONT_CSP`) NU e câmp: e predatorul/primitorul tipizat (`ContPropriu` din familia Repartitori). |
| `NR_DOC_CONEX`/`DATA_DOC_CONEX` | **derivată / parametri** | Numărul NIR-ului cules anticipat pe factură (01) — parametru de generare conex, nu structură. |

### `Document` (bază) — lista finală

```
Document (abstract, TPT)
  Numar            string      — identitate; politica de numerotare = date (seed per tip)
  Data             DateOnly    — pivotul perioadei fiscale și al registrelor
  PredatorId       FK Repartitor
  PrimitorId       FK Repartitor
  Stare            enum Draft | Operat | Stornat
  DataOperare      DateTime?   — momentul generării rândurilor de registru
  DocumentSursaId  FK Document?  — legătura conex sursă→generat (decizia 17)
  Autogenerat      bool
  Total            calculat = Σ Detalii.Valoare  (nu se persistă)
  Detalii          colecția de DocumentDetaliu
```

Nullabilitatea predator/primitor: ambele obligatorii pe bază — toate cele 10
tipuri au două laturi (la `Plata`/`Incasare` laturile sunt `ContPropriu` ↔
`Partener`/`Angajat`). Îngustarea tipului de repartitor permis = validare pe
derivată, nu schemă.

## 2. Verdictele — poziții (`DocumentDetaliu`)

| Câmp legacy | Verdict | Justificare |
|---|---|---|
| `ID_GEST_TIP_MATERIAL` (Tipul) | **BAZĂ** | Cheia motorului de note: toate modelele de contare sunt per `(tip document, ID_GEST_TIP_MATERIAL)` (00 §5). Tipul aparține unei Clase (10 §2) ⇒ un singur FK dă ambele chei: Clasa (regulile de stoc se agață de ea) se derivă din Tip. Nu persistăm separat `ID_GEST_TIP_PRODUSE` pe linie. |
| `CODMAT` (lotul) | **BAZĂ** (nullable) | Motorul de stoc lucrează pe lot: `stock = semn × cantitate` per codmat (00 §4), dependența la anulare/corecție e pe loturi (decizia 14), evaluarea e prețul lotului (decizia 13). Nullable: tipurile fără stoc (Decont, FacturaIesire, Plata) nu referă lot; obligativitatea per tip = validare. |
| `CANTITATE` | **BAZĂ** | Formula de stoc este literal `semn × cantitate` (00 §4). Rămâne SEMNATĂ pe bază (vezi §4, LDI). |
| `VALOARE_RECEPTIE_TVA` / `VALOARE_LIVRARE` | **BAZĂ, unificat ca `Valoare`** | Tensiunea (1) — vezi §3. |
| `PRET_UNITAR` | **derivată** | Input al lanțului pe tipurile de intrare; pe ieșiri e DISPLAY al prețului de lot (03: „readonly de facto"). Nicio regulă nu-l consumă direct — motorul ia prețul din `Lot`, iar la crearea lotului îl derivă din `Valoare / Cantitate`. |
| `COTA_TVA`, `PRET_RECEPTIE`, `PRET_RECEPTIE_TVA`, `TVA_RECEPTIE`, `VALOARE_RECEPTIE` | **derivată** | Intermediarele lanțului — vezi §3. |
| `PRET_LIVRARE`, `VALOARE_LIVRARE_VALUTA` | **derivată / zgomot** | Familia LIVRARE există vizibil doar pe FacturaIesire (07); `VALOARE_LIVRARE_VALUTA` e zgomot de config confirmat (03, 04). |
| `COD_FUNCTIONAL`, `COD_ECONOMIC` (+ proiect, unitate, sursă) | **BAZĂ, în `Dimensiuni`** | Tensiunea (2) — vezi §5. Modelele de note le preiau de pe linie (00 §5) ⇒ regulă contabilă ⇒ bază. |
| `ID_ANGAJAMENTE_DEFALCARE` | **BAZĂ (nullable) — CONFIRMAT** | Apare în modelele de note (00 §5: „preluate de pe linie") și în validarea de clasificație („angajament SAU cod economic" — 01). NU e în lista de 8 dimensiuni a deciziei 15 ⇒ FK nullable `AngajamentId` pe baza detaliului, LÂNGĂ `Dimensiuni`. Confirmat (proprietar): mecanismul angajamentelor e el însuși head + detaliu + self-reference, cu 4 tipuri (legal / buget anual / multianual…) — modulul se proiectează separat, „după ce devine rotund"; până atunci FK-ul nullable e suficient și stabil. |
| `DETALII_ANGAJAMENT` | **MOARE** | Readonly calculat — descrierea angajamentului; devine afișare din FK. |
| `ContD`/`ContC`/`RepD`/`RepC` | **derivată** | Decizia 15: nu se preiau ca override generic. `Decont`: toate patru = trăsătura PROPRIE a tipului (postare explicită pe linie, 06). `FacturaIntrare`/`FacturaIesire`: `ContC` obligatoriu pe linie era fluxul principal legacy (01, 07). TRANȘAT (proprietar): contul se rezolvă prin POLITICĂ — per tip partener (`REPARTITORI_TIPURI.CONT`: 401/404/411) și/sau per Clasă-Tip; câmp explicit pe linia Factura* poate apărea ULTERIOR ca fallback, pur aditiv. Nu intră în modelul inițial. |
| `DENMAT` | **derivată** | Pe tipurile cu produs e denormalizare (moare — vine din `Produs`); pe `Decont`/`FacturaIesire` e descrierea liberă a liniei ⇒ câmp `Descriere` pe acele derivate. |
| `UM` | **afară din linie** | Nicio regulă n-o consumă; aparține `Produs`-ului (catalog). Pe `Decont` era pro-forma (`'BUC'`/`'1'` — 06) — moare. |
| `ID_GEST_SUMATOR` (selector produs) | **înlocuit de mecanism** | Selecția pe produs cu spargere auto pe loturi = picking-ul auto-FIFO din decizia 13 (există deja în legacy ca `spSpargeCodSumCulegere`). `ProdusId` ca FK persistat pe linie NU e necesar în bază (derivabil din `Lot.ProdusId`); pe liniile fără lot nu are sens. Selectorul e UI. |
| `DATA_EXPIRARE`, `LOT_FABRICATIE` | **pe `Lot`** | Atribute ale lotului (nomenclator, 10 §3), nu ale liniei. Se culeg la intrare dar se persistă pe entitatea `Lot` creată de linie. |
| `PRODUS` ('M'/'S'), `TIPMAT`, `TIP_MATERIAL` | **MOR** | Mecanica tabelei late: flag-ul M/S dispare (Produs/Lot sunt entități), denormalizările de Clasă/Tip dispar (FK-ul e sursa). Refolosirea `TIPMAT`=„LOT" pe NIR (00 §11) e acoperită de entitatea `Lot`. |
| `CANTITATE_SUPLIMENTARA`, `CONVERSIE_UM` | **afară** | UM duală — configurată dar nefolosită vizibil (01); zgomot REQUIRED+invizibil pe BCS/BTR (03, 04). Dacă apare nevoia reală: atribut de `Produs`, aditiv. |
| `STOCK_BEFORE`/`STOCK_AFTER` | **nepersistat** | Feedback de sold la culegere (04) — devine interogare pe registru la editare, nu coloană. |
| `COD_CPV`, `CATEGORIE_GRUPARE` (linie) | **derivată** | Atribute de achiziție, doar ecran FCT (01). |
| `SEMN_CANTITATE=0` („linie nomenclator", 00 §10) | **MOARE** | Linie fără mișcare = hack; în modelul nou o linie de document operat mișcă registre sau nu există. |

### `DocumentDetaliu` (bază) — lista finală

```
DocumentDetaliu (bază; de confirmat la inventarul XAF dacă derivatele
                 de detaliu sunt necesare deloc — decizia 3)
  DocumentId     FK Document
  TipMaterialId  FK TipMaterial   — cheia contării; Clasa (cheia stocului) = TipMaterial.Clasa
  LotId          FK Lot?          — cheia stocului; null pe tipurile fără stoc
  Cantitate      decimal          — semnată; formula de registru = semn(politică) × Cantitate
  Valoare        decimal          — valoarea finală de postare (vezi §3)
  AngajamentId   FK Angajament?   — confirmat; modulul angajamente se proiectează separat
  Dimensiuni     owned Dimensiuni — toate componentele nullable (vezi §5)
```

## 3. Tensiunea (1): lanțul de valori — tranșat

**Verdict: lanțul se taie la capăt. Doar capătul (`Valoare`) intră în bază;
tot ce e în amonte e derivată.**

Argumentul, pe criteriu:

- Ce consumă efectiv motoarele: stocul valoric = `semn × VALOARE_RECEPTIE_TVA`
  (00 §4), formula de notă = `gest_itemsi.Valoare_receptie_tva` în aproape
  toate modelele (00 §5), imperecherea = totaluri de document (09 §2).
  **Un singur număr per linie.** Niciun model de notă și nicio regulă de stoc
  nu consumă `PRET_UNITAR`, `COTA_TVA`, `TVA_RECEPTIE` sau vreun intermediar.
- Lanțul de DERIVARE a acelui număr diferă per tip: FCT/NIR/DEC — lanțul cu TVA
  (precedența 1–5); BTR — `PRET_RECEPTIE × CANTITATE` FĂRĂ TVA (04);
  BCS — prețul lotului, neculese (03); FacturaIesire — familia LIVRARE (07);
  LDI — preț de evaluare cules la plusuri (05). Exact profilul „doar pe
  ecranul unui tip" ⇒ derivată.
- Unificarea rezolvă și hack-ul `TOTALDOC` per tip (00 §10): `Total = Σ Valoare`
  devine uniform pe bază, iar `VALOARE_RECEPTIE` recaptionat „VALOARE" pe
  FacturaIesire (dubla familie pe aceeași linie, 07) dispare.
- Crearea lotului nu cere preț în bază: prețul fix al lotului =
  `Valoare / Cantitate` de pe linia de intrare (echivalent `PRET_RECEPTIE_TVA`),
  derivabil de motor din bază.

Semantica `Valoare`: **valoarea cu care linia intră în registre** (stoc valoric
+ note). Pentru instituția publică include TVA-ul nedeductibil capitalizat
(00 §4); pe FacturaIesire e valoarea de livrare. Numele NU codifică
TVA/recepție/livrare — asta e treaba lanțului din derivată.

Risc asumat (explicit): dacă vreodată o regulă contabilă va avea nevoie de
TVA separat (4426/4427 la un client plătitor de TVA), TVA-ul nu e în bază.
Adăugarea unui `ValoareTva` nullable în bază e pur aditivă la acel moment;
nu plătim acum pentru o nevoie pe care niciun deployment n-o are.

## 4. LDI și semnul cantității

`Cantitate` rămâne **semnată în bază** — e chiar limbajul motorului
(`semn × cantitate`), iar LDI e dovada că semnul liniei poartă informație
(direcția diferenței, 05). Direcția explicită cerută de 05 („nu semn implicit")
se rezolvă pe DERIVATĂ: `ListaDiferenteInventarDetaliu` expune
`Directie: Plus|Minus` (+ validare cantitate pozitivă la culegere) și
materializează semnul în `Cantitate` din bază. Celelalte derivate validează
`Cantitate > 0`. Astfel filtrul de politică `SEMN_ITEMS` (mort peste tot în
afară de LDI) nu mai există ca mecanism: politica LDI dă semnul +1, linia
poartă semnul real.

## 5. Tensiunea (2): granița bază/derivată la dimensiuni — tranșat

**Verdict: owned type-ul `Dimensiuni` intră INTEGRAL pe baza detaliului, cu
toate componentele nullable. Granița nu e în schemă — e în validare.**

Argumentul:

- Motorul de note consumă dimensiunile GENERIC: coalesce linie → regulă
  (`Comun`/`OverrideDebit`/`OverrideCredit`) → header (decizia 15), iar
  modelele legacy le preiau de pe linie indiferent de tip (00 §5). Un membru
  al owned type-ului consumat de o funcție generică a motorului = regulă
  contabilă ⇒ bază. Splitarea owned type-ului per derivată ar face funcția de
  rezolvare ne-generică — exact ce interzice decizia 1.
- Ce diferă REAL între tipuri nu e schema, ci care dimensiuni se culeg și care
  sunt obligatorii: clasificația bugetară obligatorie pe FCT, exceptarea
  FacturaIesire de la angajament (00 §10), dimensiunile de venit vs cheltuială.
  Astea sunt exact „date de validare: dimensiuni obligatorii per cont"
  (decizia 15) + metadata UI per tip (decizia 8) — politici, nu coloane.
- Cazul BREG_P confirmă: defalcarea bugetară a plății (09) devine linii de
  `Plata` cu `Dimensiuni` + `Valoare` — funcționează DOAR dacă dimensiunile
  sunt pe baza detaliului, altfel Plata ar avea nevoie de propriul mecanism.

Delimitări care rămân în afara owned type-ului:

- **`AngajamentId`** — referință la execuția bugetară, nu dimensiune de notă
  (nu e în cele 8 din decizia 15); FK separat nullable pe bază (§2), confirmat.
- **Componenta `Repartitor` din `Dimensiuni`** vs `Predator`/`Primitor` de pe
  header: headerul dă repartitorii D/C impliciți ai notei (00 §5); componenta
  de pe linie e override punctual. Ordinea de coalesce (linie → regulă →
  header) se fixează la modelul motorului.
- **Defalcarea multi-sursă pe linie** (cofinanțare 85/15, 00 §12): NU intră în
  baza detaliului acum. Mecanismul propriu de defalcare (seturi de `Dimensiuni`
  cu pondere) se proiectează odată cu `SursaFinantare` (decizia 21) și e
  aditiv — o entitate copil a liniei, nu coloane.

## 6. Ce NU e în bază — sumar per derivată (doar câmpuri proprii)

| Derivată | Câmpuri proprii (peste bază) |
|---|---|
| `FacturaIntrare` | DataScadenta (`IDocumentCuScadenta`), NumarPV/DataPV (`IDocumentCuPV`), CodCpv, valută/curs, TethysId (import), parametrii generare NIR/Plata (câmpuri persistate — tranșat) |
| `FacturaIesire` | DataScadenta (`IDocumentCuScadenta`, default politică +30), serie/numerotare fiscală proprie; linia: Descriere, lanț LIVRARE, CotaTva; contul de venit = politică (tranșat) |
| `NIR` | (aproape goală — structura e în bază; numărul din parametrii conexului) |
| `BonConsum` | (goală structural; gestiune→loc de consum = predator/primitor) |
| `NotaTransfer` | NumarPV/DataPV (`IDocumentCuPV`); linia: (goală — lot+cantitate din bază) |
| `ListaDiferenteInventar` | comisie (calitate pe repartitor, de confirmat); linia: Directie, preț de evaluare la plus |
| `Decont` | NumarPV/DataPV (`IDocumentCuPV`); linia: Descriere, ContDebit/ContCredit/RepDebit/RepCredit (trăsătură proprie, 06) |
| `Plata`/`Incasare` | TipInstrument (OP/CEC/DCP/chitanță), NrExtras/DataExtras, referință transfer |
| `RaportProductie` | REZERVAT (decizia 19) — pattern-ul (linii −1 pe loturi existente + linii +1 care creează loturi cu preț dat) e deja acoperit de bază via LDI |

Observația pentru decizia 3 (derivate pe DETALII): după test, diferențele de
detaliu care justifică schemă proprie sunt DOAR: lanțul de valori per familie
(Factura*), `Directie`+preț evaluare (LDI), `Descriere`+conturi (Decont,
FacturaIesire). NIR/BonConsum/NotaTransfer au detaliul = baza pură + validare.
Se confirmă intuiția deciziei 3: derivatele de detaliu există, dar nu pentru
toate tipurile — de decis la modelul XAF dacă TPT parțial sau bază + câteva
derivate.

## 7. Tranșările discuției (2026-07-22, proprietar) — TOATE ÎNCHISE

1. **`AngajamentId`**: rămâne FK NULLABLE pe baza detaliului. Mecanismul
   angajamentelor (head + detaliu + self-reference, 4 tipuri: legal / buget
   anual / multianual…) se proiectează ca modul separat, „după ce devine
   rotund" — FK-ul nullable e forma stabilă până atunci.
2. **Contul pe linie la Factura***: se rezolvă prin POLITICĂ (per tip partener
   și/sau per Clasă-Tip); câmpul explicit pe linie poate apărea ulterior ca
   FALLBACK, pur aditiv. Nu intră în modelul inițial.
3. **`DECONT_*`**: câmpuri PERSISTATE pe `FacturaIntrare` (nu dialog
   non-persistent).
4. **`DataScadenta`** și **`NumarPV`/`DataPV`**: INTERFEȚE
   (`IDocumentCuScadenta` pe Factura*; `IDocumentCuPV` pe FacturaIntrare,
   NotaTransfer, Decont) — contract mai ferm decât dublarea liberă.

Pasul 2 e închis; urmează pasul 3 — modelul XAF (bază + derivate TPT,
validare declarativă, tabelele de politică).
