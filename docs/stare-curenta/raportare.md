# Raportare

**Actualizat: 2026-10-15.** [Index](README.md)

Proiecțiile contabile și de stoc, soldul pe parteneri și explicația deciziei.

## Explicația deciziei (X-D4, 2026-10-03)

Contractul acceptat își persistă deciziile și ipotezele: de ce a costat
ieșirea atât, din ce sold, ce partide a stins.

- **Unde**: `Tranzactie.Explicatie` (`jsonb`), pe prima tranzacție scrisă a
  contractului — `Operare` când există, altfel `Transfer` (BTR, ASM fără
  schimb de cont). A doua tranzacție a aceluiași contract o referă prin
  `ExplicatieDinId`. Se scrie în același `INSERT` cu rândul și nu se mai
  modifică. `Storno`, `Deschidere`, transferul împerecherii, desfacerea și
  stingerea de deschidere nu au explicație; baza refuză prin
  `CK_Tranzactie_Explicatie` și `CK_Tranzactie_ExplicatieDin`.
- **Ce**: `Cub.Explicatie` — versiunea schemei (`v` = 2), numele
  declarantului, `JumatatiDeBan`, deciziile și ipotezele contractului în
  ordinea declarației, cu linia și unitatea lor. Un cititor refuză o versiune,
  o decizie sau o ipoteză pe care nu o cunoaște; versiunea 1 nu are cititor.
- **Regula care a decis** (D9-A8): pentru fiecare rând de politică din care
  declarantul a luat un fapt folosit la o postare, ipoteza
  `VersiunePolitica(Fel, Rand, Versiune)` — clasa rândului, identificatorul
  și contorul lui la operare, una pe rând, fără dubluri
  (`Declaratii/PoliticiConsumate`). Se rețin: regula de contare câștigătoare
  a fiecărei linii (numită și pe `ContRezolvat.Regula`), regula de stoc a
  LDI, politica de TVA când declarația are fapt fiscal, politica de
  diferență a NIR-ului conex și politica
  de amortizare a fișelor atinse. Nu se rețin nomenclatoarele care decid
  (tipul de material, tipul de TVA, steagurile contului) și nici conținutul
  regulii la operare (D9-r3). Un declarant care nu consumă reguli (ASM, BTR,
  NTC) nu scrie ipoteza. Lista nominală, declarant cu declarant:
  `docs/nucleu/tr-d9-pas7c-versiunea-politicii.md`.
- **Ieșirea pe lot** are exact o decizie de valoare: `ValoareIesire` când
  valoarea vine din soldul citit (BCS, DSC, BTR, LDI minus, consumul ASM),
  `ValoareDeclarata` când o dă altă sursă — linia documentului (RLF și RDC, la
  valoarea culeasă) sau recepția facturii (NIR-minus). Sursele sunt
  `Declaratii.SurseValoare`. Decizia poartă cantitatea și valoarea pozitive;
  postarea-sursă a unui transfer le are negative. Mecanismul e al
  declarantului, nu al explicației: `IDeclarant.SursaValoareDeclarata` e
  `null` pentru declaranții care evaluează din sold și numește sursa pentru
  cei trei care declară valoarea (NIR, RLF, RDC). Niciun declarant nu le
  amestecă.
- **Soldul citit** (`SoldUnitateCitit`) e soldul net al unității la data
  documentului, fără documentul curent, o dată per unitate. Soldul dinaintea
  fiecărei ieșiri următoare de pe aceeași unitate se derivă din deciziile
  anterioare (`Explicatie.IesiriEvaluate`).
- **Stingerea FIFO** are câte o `AlocareFifo` per linie și partidă stinsă,
  iar partida stinsă are soldul ei citit între ipoteze; partida proprie a
  documentului are `PartidaDeschisa`.
- **Stornoul** se explică prin original: `Cub.Citiri.Explicatii.PeTranzactie`
  urmează `InversaDin` și întoarce explicațiile purtătorilor de origine.
  **Anularea operării** șterge explicația odată cu tranzacția.
- **Invariantul** (`Explicatii.VerificaAcoperire`, în `INV-CUB`):
  `CITIRE_EXPLICATIE_LIPSA` (o tranzacție `Operare` sau o ieșire pe lot fără
  explicație), `…_REFERINTA` (referință spre o tranzacție fără explicație sau
  a altui document), `…_STORNO` (inversa pe lot nu oglindește originalul),
  `…_IESIRE` (ieșirile postate și deciziile de valoare nu corespund una la
  una pe linie, unitate, cantitate și valoare), `…_EVALUARE` (valoarea unei
  `ValoareIesire` nu rezultă din soldul persistat, cu rotunjirea bazei),
  `…_STINGERE` (alocare fără postare, postare pe partida altui document
  fără alocare, partidă stinsă fără sold citit sau cu alocări peste soldul
  citit), `…_MECANISM` (explicația numește alt declarant decât cel al
  documentului, poartă o valoare declarată la un declarant care evaluează
  din sold, sau o valoare evaluată ori altă sursă la unul care declară).
  Declarantul se ia din document, nu din explicație.
- **Transferul persistat conservă pe cont** (`Invarianti.VerificaTransferuri`,
  `CITIRE_TRANSFER_NECONSERVAT`, în `INV-CUB`): în orice tranzacție `Transfer`
  valoarea se anulează pe (cont, latură) și cantitatea pe (cont, produs). E
  regula de la contractare (090f), reverificată pe ce s-a scris. Acoperă
  contul capătului de destinație, pe care nici acoperirea cantitativă a
  stocului, nici reconcilierea (a) nu îl văd.
- **Perechea persistată e cea de la contractare** (`Invarianti.VerificaPerechi`,
  în `INV-CUB`): `CITIRE_PERECHE_INVALIDA` — pe (tranzacție, ordinal) nu stau
  exact două postări cu același document, aceeași linie, aceeași dată,
  cantități opuse și valorile potrivite laturilor felului; `CITIRE_PERECHE_LIPSA` — postare fără
  ordinal în afara deschiderii și a liniilor transformării. (D9-A2)
- **Ordinalul numără postările tranzacției** (`Invarianti.VerificaOrdinale`,
  în `INV-CUB`): `CITIRE_ORDINAL_INVALID` — postările unei tranzacții nu sunt
  numărate de la 1, fără goluri și fără dubluri. (117)

Dry-run-ul nu persistă nimic. O declarație pe care nucleul nu o poate
construi (`ArgumentException`) sau un declarant care nu întoarce nici
declarație, nici refuz dau refuzul `DECLARATIE_INVALIDA`, pe dry-run și pe
operare, în aceeași formă ca orice refuz al declarației (S-r11).

## Identitățile cititorilor (C111-D10)

Cititorii din `Cub/Citiri` expun fiecare identitate separat; nimeni nu
alege între două coloane.

| Identitatea | Domeniul | Coloana |
|---|---|---|
| titularul partidei | postarea cu `FelUnitate.Partida` (`Citiri/Partide`) | `Repartitor` |
| terțul capătului | postarea contabilă a unui document (`Citiri/Plati`) | `Repartitor`, când e extern |
| partenerul fiscal | postarea cu fapt fiscal (`Citiri/Fiscale`) | `PartenerFiscal` |
| locul | postarea cu fapt fiscal sau capătul intern cu partidă; îl citesc lista postărilor și `INV-CUB`, nicio citire de sold | `Loc` |
| terțul comercial | postarea pe un cont cu `RolTert` (`Contabil.Jurnal`, `PostareJurnal.TertComercial`) | `Repartitor`, când e extern |
| locul lotului | postarea cu lot (`Citiri/Loturi`) | `Repartitor` |
| repartitorul intern fără lot | locul fișei, poziția fără fișă (`Citiri/Imobilizari`) | `Repartitor` |
| contraponderea | postarea marcată | `Contrapondere` |
| axa analitică | atomul contabil (`Contabil.Analitice`) | `Repartitor` |

„Extern” se judecă într-un singur loc, `Contabil.Externi`: repartitorii de
fel partener sau angajat.

Regula terțului comercial în SAF-T (S11) și cea a partenerului fiscal în
D394 (S13) sunt în [politici-si-fiscalitate](politici.md),
§„SAF-T” și §„D300 și D394”.

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (6; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 42 (amendată de 104, 106), 66, 67, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 104 (amendată de 106), 117.
<!-- /gen:decizii -->
