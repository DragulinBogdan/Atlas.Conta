# Coordonatele cubului contra rapoartelor reale (pasul 1 din §11)

Stare: **PROBAT (2026-09-19), cu review advers aplicat** — amendamente la
§2, §3, §4, §6 și §7 ale designului (`nucleu-cub-design.md`). Proba: patru
inventare ale implementării curente, cu `fișier:linie` pe fiecare câmp de
ieșire — `run-nucleu/coordonate/01-contabil.md` (balanță plată/pliată, fișa
contului, registrul jurnal, sold cont × repartitor, documente cu rest),
`02-tva.md` (jurnale, decont, rectificativă, D300, D394), `03-saft.md` (D406 L
și C, toate secțiunile emise), `04-unitati.md` (stoc, loturi, imobilizări,
partide). Întrebarea pusă fiecărui câmp: e sumă pe cub, atribut al unei
coordonate, atribut al documentului prin `Cauza`, decizie de politică sau
funcție de randare peste sume? Review-ul advers (agent separat, 4 MAJOR,
5 MEDIU, 3 MINOR) e integrat mai jos; unde a schimbat o concluzie, e spus.

## 1. Rezultatul

**„Minimal" se ține. „Reconstruibil" se ține numai cu patru amendamente
structurale** pe care prima versiune a sintezei le presupunea nescrise:

1. postarea contabilă poartă **latura** (debit/credit) separat de semn —
   stornoul de azi e „în roșu" (aceleași conturi, valoare negativă,
   `Motor/MotorOperare.cs:669-681`), iar rulajele brute ale balanței, ale
   fișei și `TotalDebit`/`TotalCredit` din SAF-T sunt cifre de raport;
2. **stornoul e o tranzacție distinctă** pe același document (fel
   `Operare | Storno`, cu data și `ScrisLa` ale ei) — nu e coordonată a
   postării, dar e cheie de grupare în jurnalul TVA, D394, jumătățile de
   factură/plată și MovementOfGoods;
3. **taxa se postează per linie**: SAF-T cere `TaxAmount` obligatoriu pe
   fiecare `InvoiceLine` și pe linia de GL; documentul × cotă rămâne unitatea
   de DECIZIE și de rotunjire, repartizată pe linii cu primitiva din §6;
4. **partenerul stă pe postarea de terț**: convenția de azi pune
   contrapartida pe fiecare latură (`SaftProiectii.cs:29-46`), iar un cub de
   postări atomice n-are „cealaltă latură".

Două coordonate ale designului (`Valuta`, legătura `Atribuit`) **nu pot fi
probate de niciun raport real**: implementarea curentă n-are nici partidă în
valută, nici reevaluare. Două coloane ale registrului imobilizărilor
(`DeductibilCumulat`, `Luni`) sunt azi sume de registru și în design NU sunt
sume pe cub (§5.4, §5.5) — sunt limita declarată a lui „reconstruibil": vin
din contractele înghețate ale documentelor de impozit și din liniile de AMO.

Tot ce citesc rapoartele de azi din DOCUMENT se împarte în două: **cifre**,
care în cub devin sume pe `Cauza` (sumele facturilor și plăților din SAF-T,
`TotalStingere`, `Lot.PretUnitar`), și **atribute** (număr, dată fizică, tip,
descriere, instrument de plată, cantitatea și prețul facturat pe linie), care
rămân ale documentului și se citesc prin `Cauza`. Cubul reconstruiește prima
categorie; a doua nu e a lui.

## 2. Coordonatele, cu proba de minimalitate

Fiecare coordonată are raportul care se rupe fără ea; ce nu are un asemenea
raport e marcat.

| Coordonată | Spațiu | Se rupe fără ea | Azi (registre) |
|---|---|---|---|
| `Cont` | contabil | balanță, fișă, jurnal, GLA, GLE | `ContDebitId`/`ContCreditId` |
| `Latura` (D / C) | contabil | rulajele brute ale balanței și fișei, `TotalDebit`/`TotalCredit` SAF-T, `DebitCreditIndicator` — cu storno în roșu semnul nu le dă | latura rândului-pereche |
| `Data` (a înregistrării) | toate | granițele de perioadă, snapshot, S1/S5 | `Data` pe contabil, stoc, imobilizări (`= DataInregistrare`); pe fiscal `Data` e cea FIZICĂ (`MotorOperare.cs:379`) — vezi §4 |
| `Partener` (inclusiv angajat) | contabil, terți, fiscal | Customers/Suppliers, D394, jurnale, sold partener, `CustomerID`/`SupplierID` pe linia de GL | `Repartitor` pe latură; `RegistruTva.PartenerId` |
| `Gestiune` — reală (depozit, casă/bancă, loc) sau virtuală (Consum, Folosință, Gratuit, ProducțieNeterminată, Furnizor, Client) | stoc, contabil | sold stoc, PhysicalStock, MovementOfGoods; locul fișei la data faptului | `Repartitor` (Gestiune/ContPropriu/UnitateInterna) + `TipStoc` |
| `Produs` | stoc, contabil | Products/PhysicalStock/MovementOfGoods, op11 D394, evaluarea la cost mediu, dimensiunea Material | `Lot.ProdusId`; `DebitMaterialId` |
| `Unitate` (lot / partidă / fișă) | stoc, terți, imobilizări | sold stoc pe lot, FIFO, `StockAccountNo`, documente cu rest, registrul imobilizărilor | `LotId`; `Document.TotalStingere`; `ImobilizareId` |
| `CodTva` = (tip@versiune × sens × rol Bază/Taxă) | fiscal | jurnale, decont, D300, D394, TaxInformation, TaxTable | `RegistruTva` (`TipTvaId`, `Sens`, `Regim`, `Cota`, coloanele `Baza`/`Tva`) |
| `PerioadaDeclarare` | fiscal | jurnale, D300, D394, SAF-T (alt reper decât `Data`) | `PerioadaAn`/`PerioadaLuna` |
| `Carte` (Contabil / Fiscal) | imobilizări, rezultat fiscal | registrul imobilizărilor (brut și amortizare fiscală); D101 | `ValoareFiscala`/`AmortizareFiscala` ca coloane |
| `Analiză` ×6: CodFunctional, CodEconomic, SursaFinantare, UnitateOrganizatorică, Proiect, CentruCost | contabil | SAF-T `Analysis` (CC, P, U, SF, CF, CE), filtrele balanței, snapshot | 6 din cele 8 coloane plate ale fiecărei laturi (12 din 16) |
| `Valuta` (+ măsura `ValoareValuta`) | terți, trezorerie | **niciun raport azi** (C0 din `04-unitati.md`); partida în valută și 665/765 doar în sondajul designului | `FacturaIntrare.Valuta` informativ |
| `Cauza` = (document, linie, tranzacție) | toate | fișă/jurnal (număr, tip), toate `SourceDocuments`, jurnalul TVA per linie, `NrFact`, rectificativa; tranzacția = jumătățile de storno de azi | `DocumentId` + `DetaliuId` + `Storno` pe toate registrele |
| `Atribuit` | toate | **niciun raport azi** (reevaluarea nu există); stornoul după reevaluare, doar în sondaj | — |

Măsurile rămân trei (`Cantitate`, `ValoareValuta`, `Valoare`). Cele două
coloane ale registrului fiscal (`Baza`, `Tva`) devin `Valoare` pe rolul din
`CodTva`; cele cinci ale registrului de imobilizări devin `Valoare` pe
`Carte × Cont` (2xx pentru brut, 28xx pentru amortizare), fără
`AmortizareDeductibila` (§5.4).

`Latura` e coordonată doar pe spațiul contabil: conservarea devine
`Σ Debit = Σ Credit` per tranzacție, soldul contului `Σ D − Σ C`, iar stornoul
în roșu rămâne legal și distinct de o postare pe latura opusă. Pe stoc semnul
ajunge (intrare/ieșire), cum e și azi.

**Ce NU e coordonată** (și azi e coloană de registru):

- `TipStoc` — Magazie/Mărfuri = `Gestiune` reală + `Cont` al laturii de valoare
  (371 vs 3xx, decis de politică din produs); Consum/Folosință/Gratuit/
  ProducțieNeterminată = gestiuni virtuale; Custodie = cantitate pe cont în
  afara bilanțului (803x), cu valoare zero — distinctă de stocul propriu prin
  `Cont`, nu prin gestiune (azi n-are rânduri vii). Politica de mișcare SAF-T
  și mulțimea „gestiunilor de patrimoniu" din PhysicalStock se re-cheiază pe
  `(tip document × Cont × semn × rol terț)` în loc de `TipStoc` (§5.8).
- `Storno` pe postare — e felul TRANZACȚIEI (§1.2). Niciun raport nu-l
  FILTREAZĂ; cele care îl grupează grupează pe tranzacție.
- `NumarNota`, perechea debit/credit, cele 16 coloane de dimensiuni per latură —
  postarea are un singur cont, o latură și un singur set de coordonate; GLE
  emite `TransactionLine` = postare.
- `ClrType`, `Regim`, `Cota` pe rând — tipul e al documentului (prin `Cauza`),
  regimul și cota sunt ale versiunii de `TipTva` pe care o numește coordonata.
  Decontul care azi scoate două rânduri când cota s-a schimbat în perioadă iese
  identic: două versiuni, două coordonate. Consecință de declarat: azi
  `TaxTable.TaxPercentage` vine din nomenclatorul CURENT, iar
  `TaxInformation.TaxPercentage` din snapshotul rândului
  (`SaftProiectii.cs:2885` vs `:669`); sub `tip@versiune` ambele ies din
  versiune, deci fișierul emis se schimbă acolo unde cota s-a editat.

## 3. Reconstruibilitatea, raport cu raport

Notație: `Σ[filtru; grup]` = suma măsurii `Valoare` (sau `Cantitate`) pe cub.

| Raport | Pe cub | Randare (ne-aditiv, peste sume) | Prin `Cauza`/`Unitate` (atribut) |
|---|---|---|---|
| Balanță plată | `Σ[Data<start; Cont(,Partener), Latura]` + `Σ[start≤Data≤end; …]` | netarea `max(D−C,0)` la nivelul grupului; sintetic/analitic = alt grup | — |
| Balanță pe plan | frunzele balanței plate | cumul brut în sus, netare per nod | — |
| Fișa contului | postările cu `Cont=X`, fereastră cumulativă pe `Data`, debit/credit din `Latura` | sold curent = fereastră; contrapartida = celelalte postări ale tranzacției (n-are unic pe tranzacție multi-picior) | număr, tip document |
| Registrul jurnal | tranzacțiile perioadei, postările lor | — | număr, tip |
| Sold cont × partener | `Σ[Data≤t; Cont, Partener, Latura]` | netare | — |
| Documente cu rest | `Σ[Data≤t; Unitate=partidă] ≠ 0` | sensul = semnul soldului × funcția contului | număr, dată fizică; partenerul = coordonata unității |
| Jurnal cumpărări/vânzări | `Σ[PerioadaDeclarare; Cauza.document, Cauza.tranzacție, CodTva]`, Bază/Taxă = rolul | — | număr, tip, DATA FIZICĂ (coloana `Data` a jurnalului e `MIN(doc.Data)`, `TvaProiectii.cs:225`); CUI din nomenclator |
| Decont schelet | `Σ[PerioadaDeclarare; CodTva]` | — | — |
| Rectificativă | postările cu `Tranzactie.ScrisLa > InchisaPrimaOara` în perioadă | — | număr, dată fizică |
| D300 | `Σ[PerioadaDeclarare; CodTva]` × `MapareD300` | totaluri, oglinzi, rd. 31 (scădere pe apariție), `max(…,0)` | — |
| D394 | `Σ[PerioadaDeclarare; Partener, CodTva, Cauza.tranzacție]` + `count distinct Cauza.document` | tip partener re-decis pe CUI, `NrFact` (argmax pe cotă), normalizarea CUI | op11 doar pentru avertisment (produsul liniei) |
| SAF-T GLA | = balanța sintetică | despicare pe semn | — |
| SAF-T Customers/Suppliers | `Σ[Cont.RolTert≠0; Partener, Cont, Latura]` — CERE partenerul pe postarea de terț (§5.4) | `AccountID` = argmax mișcare; cumul pe identificator dublat | — |
| SAF-T TaxTable / Products / UOM / Analysis | mulțimile de coordonate FOLOSITE în perioadă | — | — |
| SAF-T PhysicalStock | `Σ[Data; Gestiune reală, Unitate=lot]` pe cantitate și valoare | `UnitPrice = ΣValoare/ΣCantitate` | atributele lotului (produs, UM) |
| SAF-T GLE | tranzacția = (document, tranzacție); linia = postarea, D/C din `Latura`; `CustomerID`/`SupplierID` = `Partener` al postării; `TaxInformation` = postările cu `CodTva` ale aceleiași linii | — | număr, `DataOperare`, descrierea liniei |
| SAF-T SalesInvoices / PurchaseInvoices | per `Cauza.linie`: contul = postarea non-terț a liniei; `InvoiceLineAmount` = `Σ[Cauza=linie; rol Bază]`; taxa = `Σ[Cauza=linie; rol Taxă]` (postată per linie, §1.3) | totalurile facturii | număr, dată; `Quantity` și `UnitPrice` de pe TOATE liniile (`SaftProiectii.cs:1158-1164`); descriere. **Limită**: linia de stoc a facturii de intrare nu postează — contul și analiza ei vin de pe linia de recepție a NIR-ului conex (§5.6) |
| SAF-T Payments | per `Cauza.linie`: `Σ[Cauza=linie; Cont terț]` | — | număr, `TipInstrument`; `SourceDocumentID` = `Cauza.document` al unității (partida) pe care postează linia |
| SAF-T MovementOfGoods | postările de cantitate ale tranzacției; codul de mișcare din politică pe `(tip document × Cont × semn × rol terț)` | spargerea pe cod | număr, `DataOperare` |
| Sold stoc / loturi | `Σ[Data≤t; Unitate=lot, Gestiune]` | cost = `ΣValoare/ΣCantitate` (azi `Lot.PretUnitar` înghețat) | atributele lotului |
| FIFO | loturile cu `ΣCantitate>0` în (produs, gestiune), ordonate pe prima postare | — | — |
| Fișa mijlocului fix | `Σ[Data≤t; Unitate=fișă, Carte, Cont]` | net = brut − amortizare | parametrii (metodă, durată) = ultimul eveniment (§5.3); `DataPunereInFunctiune`/`DataIesire` = data FIZICĂ a PIF/CAS (§4); `DeductibilCumulat`, `Luni` (§5.4, §5.5 — nu sunt sume pe cub) |
| Registrul imobilizărilor | idem, pe toate fișele cu postare de PIF | totaluri | stare = are PIF fără CAS; datele fizice prin `Cauza` |
| Previzualizare AMO | contract al motorului, nu raport | — | — |

Coloanele ne-aditive sunt funcții de randare peste sume sau numărători pe
cub — inclusiv `NrFact` (numărătoare de `Cauza` per cheie) și `AccountID` al
partenerului (argmax peste sume per cont). Excepțiile sunt cele două din §1
(`DeductibilCumulat`, `Luni`). Snapshot-urile de azi (trei tabele cu trei
forme) sunt același `Σ[Data≤graniță; toate coordonatele]`, pe spațiu.

## 4. Ce se citește prin `Cauza` și de ce e legitim

Regula: **cubul ține faptele contabile; documentul ține identitatea și
descrierea faptelor**. Ce citesc rapoartele prin `Cauza`, din cele patru
inventare:

- identitate: `Numar`, `Data` (fizică — reperul jurnalelor de TVA, al
  rectificativei, al documentelor cu rest, al fișei de imobilizare:
  `DataPunereInFunctiune`/`DataIesire` se scriu din `Document.Data`,
  `Imobilizari.cs:212-214`, și sunt criteriul de eligibilitate al AMO), tipul,
  `DataOperare` (`SystemEntryDate`, `MovementPostingDate`), lanțul
  `DocumentSursa`/`LinieSursa` (conex);
- tranzacția: felul (operare / storno) și `ScrisLa` — singurul filtru al
  rectificativei (`TvaProiectii.cs:409-410`);
- descriere: `Descriere` a liniei, `TipInstrument` al plății;
- atribute de linie fără fapt în cub: `Cantitate` și `PretUnitar` facturate
  (SAF-T le citește de pe orice linie de factură; cantitatea de stoc e postată
  de linia NIR-ului, nu de a facturii), `Luni` ale liniei de AMO (§5.5);
- parametrii fișei (metodă, durată, categorie, utilizare) = atributele
  ultimului document-eveniment pe unitate, ordonat pe `Data` (§5.3).

Ce citesc rapoartele de azi din document și **în cub devine sumă** (deci
dispare ca citire de document): `DocumentDetaliu.Valoare`/`ValoareTva`
(sumele facturilor și plăților din SAF-T), `TotalStingere` + `Imperecheri` +
`PartidaDeschisa` (partida ca unitate), `Lot.PretUnitar` (raportul unității),
`Imobilizare.Stare` (are PIF fără CAS), soldurile de terți scanate pe tot
istoricul în SAF-T (același snapshot ca balanța).

Ce se citește de niciunde, la fiecare cerere, și rămâne așa: cusăturile SAF-T
(egalități între secțiuni), avertismentele, clasificarea partenerului pe CUI.
Sunt verificări și nomenclator, nu fapte.

## 5. Constatări care amendează designul

1. **§2 se completează** cu `Latura` (spațiul contabil), cu cele șase
   coordonate de analiză (set închis, în schemă, nu EAV — SAF-T le numește cu
   litere fixe), cu `Data` a înregistrării ca coordonată explicită și cu forma
   compusă a lui `CodTva`. `Gestiune` include casa/banca (`ContPropriu` de
   azi) și locul imobilizării — registrul de imobilizări poartă locul la data
   faptului și niciun raport nu-l citește, iar registrul afișează locul curent
   de pe fișă: în cub locul e coordonată a postării și transferul între locuri
   e postare de cantitate 1.
2. **§7: stornoul e o tranzacție distinctă pe același document** (fel
   `Operare | Storno`, data și `ScrisLa` proprii), iar `Cauza` e
   `(document, linie, tranzacție)` — la granularitatea LINIEI obligatoriu
   (SAF-T SourceDocuments, jurnalul TVA per linie, D394 `op11`). Consecințe de
   declarat la decizie: `InvoiceNo` dublat pe factura stornată în aceeași lună
   (azi doar strigat), discriminantul din `MovementReference`, „două facturi"
   la ANAF în D394.
3. **§6: taxa se postează per linie.** Documentul × cotă rămâne unitatea de
   decizie și de rotunjire; cota de linie iese din `Repartizeaza(taxa
   documentului, ponderi = bazele liniilor)`, deci Σ pe document e exactă prin
   construcție și nimic nu se reconciliază. Fără asta `InvoiceLine.TaxAmount`
   (obligatoriu) și `TaxInformation` de pe linia de GL nu se pot emite.
4. **§3: rezolvarea coordonatelor scrie partenerul pe postarea de terț**
   (contul cu `RolTert`), nu „contrapartida pe fiecare latură". E singura
   formă în care Customers/Suppliers, `CustomerID`/`SupplierID` pe linia de
   GL și soldul pe partener sunt sume. Postarea de cheltuială/venit poate
   purta și ea partenerul (util pe analitice), dar nu e obligată.
5. **Deductibilul e azi persistat, în design e citire.** Registrul
   imobilizărilor cere `DeductibilCumulat` la o dată; sub design cifra vine din
   contractele înghețate ale documentelor de impozit (perioadele declarate)
   plus citirea cu politică pe perioada deschisă. Coloana rămâne, sursa se
   schimbă; e limită declarată a lui „reconstruibil din cub" (§1). Nimic din
   D406 nu o cere (SAF-T nu emite `Assets` azi). La fel **`Luni`** (lunile
   amortizate, cu recuperare): decizia liniei de AMO, prin `Cauza`, sau
   numărul de tranzacții lunare pe unitate când nu există recuperare.
6. **Limita liniei de stoc a facturii de intrare**: linia FCT nu postează
   nimic pe stoc și nici pe cheltuială; NIR-ul conex postează recepția. SAF-T
   cere pe linia facturii contul și analiza acelei recepții
   (`SaftProiectii.cs:890-949`, `:1111-1117`). În cub lanțul e
   `linia FCT → LinieSursa (atribut de document) → Cauza liniei NIR → postare`
   sau `Unitate=lot`; nu e sumă pe `Cauza=linie FCT`. Alternativa e ca linia
   FCT să posteze ea recepția (NIR-ul dispare ca sursă de postări) — decizie de
   modelare a documentelor, nu a cubului; se tranșează la pasul 3 din §11.
7. **Parametrii unității, versionați pe evenimente**: fișa își citește metoda
   și durata prin coalesce înapoi peste rândurile-eveniment nestornate
   (`AmortizareService.cs:114`). În cub asta e „ultimul document-eveniment pe
   unitate", nu o agregare — dar cere ca evenimentul fără postare (o revizuire
   de durată fără valoare) să existe totuși ca tranzacție pe unitate (postare
   de valoare zero) sau ca atribut versionat al unității. Recomandare: postare
   cu valoare zero, ca reevaluarea din §5 al designului (aceeași primitivă).
8. **`TipStoc` dispare, politicile lui se re-cheiază**: `PoliticaMiscareSaft`
   (`Politici.cs:609-637`, cheia `TipDocument × TipStoc × Semn? + RolTert`) și
   mulțimea gestiunilor de patrimoniu devin `(tip document × Cont × semn × rol
   terț)`; Custodie se separă prin cont extrabilanțier.
9. **Perioada de declarare e decizie la scriere, niciodată rescrisă.** Azi
   `CorectieService.cs:97-105` rescrie `PerioadaAn/Luna` pe rânduri deja scrise
   (motivul „eroare materială"). În cub, motivul corecției alege perioada
   tranzacției de storno la momentul postării; rândul nu se mai atinge.
10. **`Sens` și `Tip` ale partidei** nu mai sunt literali per tip de document
    (`ImperecheriProiectii.cs:120-169`): sensul e semnul soldului unității pe
    contul ei; tipul e al `Cauza`. Uniunea pe șase ramuri dispare. Lanțul
    avans ↔ decont ↔ regularizare (un document pe ambele roluri) rămâne de
    probat pe cifre la scrierea deciziei: în cub e un flux între două partide
    ale aceluiași partener.
11. **Cantitatea în afara spațiului stocului nu se postează.** Linia de
    factură n-are cantitate în cub (o are linia de NIR/DSC); SAF-T o ia din
    document. Regula previne tentația de a „posta" cantități facturate ca să
    le avem în raport.
12. **Nume**: `Unitate` (lot/partidă/fișă) se ciocnește cu dimensiunea
    bugetară `Unitate` (organizatorică). Cea bugetară devine
    `UnitateOrganizatorica`.

## 6. Ce nu poate proba pasul acesta

- `Valuta`/`ValoareValuta` și `Atribuit`: nicio implementare, deci niciun
  raport. Rămân pe sondajele numerice ale designului (partida în valută,
  stornoul după reevaluare). Se probează la primul raport real care le cere
  (partide în valută cu 665/765; DVI pe loturi).
- Registrul de evidență fiscală / D101 și e-Factura n-au fost inventariate
  (D101 nu e implementat; e-Factura e emitere, nu raport de cub).
- Fișa de magazie nu există azi ca raport; pe cub e fișa contului cu
  `Unitate=lot` în loc de `Cont` — aceeași fereastră cumulativă.
- Custodie n-are rânduri vii în nicio bază; re-cheierea din §5.8 e pe hârtie.
- Cifrele de volum (câte postări per document, câte coordonate nenule pe rând)
  sunt ale pasului 2 din §11 (fizica), nu ale acestuia.
