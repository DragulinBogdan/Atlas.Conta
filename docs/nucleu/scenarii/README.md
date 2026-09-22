# Catalogul de scenarii — proba supremă a motorului (decizia 091)

**Actualizat: 2026-09-22.** Regula: `docs/decizii/091-scenarii-in-loc-de-import.md`,
literele (a)–(e). Un fișier per tip de document (`<COD>.md`), cu rândurile
de mai jos. Așteptarea se scrie de mână, din OMFP 1802 și din politica
profilului, cu cifre; nu se derivă din registre, din oracol sau din Flax.

## Forma unui rând

| Coloană | Conținut |
|---|---|
| Id | `SC-<COD>-<NN>`; transversalele `SC-X-<NN>` |
| Scenariu | documentul sau lanțul, cu datele care contează (cantități, prețuri, cote, perioade, unități numite) |
| Așteptare | postările pe cub (cont × latura × măsură × unitate) și citirile rezultate, cu cifre |
| Proba | numele check-ului ModelCheck (`SC-…`) sau al testului de nucleu |
| Proveniență | `regulă` (OMFP / politică), `recensământ Flax` (cu cifra găsită), `review` (Codex 2026-09-22 sau advers) |
| Stare | `scrisă` / `probată` / `refuzată de motor` (cu codul refuzului, ca așteptare) |

## Ciclul obligatoriu al fiecărui tip

Un tip nu e „pe cub" fără toate rândurile de mai jos, pe ambele profiluri
unde tipul există:

1. operare simplă, o linie;
2. operare cu mai multe linii pe unități diferite (loturi, partide, cote);
3. storno în aceeași perioadă;
4. storno peste graniță de perioadă (perioada fiscală re-ștampilată);
5. anulare (tranzacția dispare; unitățile născute dispar);
6. corecție în perioadă închisă: storno legat + document nou cu motiv;
7. stingere sau împerechere, unde tipul deschide partidă (transferul între
   partide, plafonul pe rest, desfacerea);
8. citirile după fiecare pas: sold pe unitate, fișă de cont, balanță la
   graniță, `Sold` reconstruit egal cu suma postărilor.

## Cazurile-limită obligatorii (unde tipul le poate produce)

- cantitate zero pe unitate ⇒ valoare zero (ultima ieșire ia restul);
- ieșire parțială cu rest, apoi golire, apoi re-intrare pe același produs;
- linie negativă (valoare sau cantitate) declarată semnată;
- lot născut „în roșu" (S-r7): refuz sau evaluare, ca așteptare explicită;
- avans + regularizare: două partide pe același document, soldul creanței e
  netul (S-D16);
- TVA pe mai multe cote cu rotunjire per document × cotă; taxare inversă
  (B-r4); regim `Capitalizat` (T-r8);
- valută cu curs (B-r6): `ValoareValuta` și `Valoare`, diferența de curs la
  stingere;
- document întârziat (`DataInregistrare` ≠ `Data`);
- storno de storno refuzat; storno al unui document cu dependenți refuzat;
- validare (dry-run) = operare pe același operand (S-r8, S-r11): același
  contract, aceleași refuzuri;
- două sesiuni reale pe același lot / aceeași partidă / operare simultană cu
  închiderea (F27-r8, S-r9) — condiție a lui „rotund", nu a pasului.

## Lanțurile transversale (`SC-X-*`)

Cele mai valoroase scenarii sunt cele pe care niciun import nu le exercită
și niciun tip nu le poartă singur:

| Id | Lanț | Ce probează |
|---|---|---|
| SC-X-01 | FCT cu recepție → BCS din lotul recepționat → storno FCT | refuzul stornoului cu dependenți; lotul cu ieșiri nu dispare |
| SC-X-02 | FCT → NIR conex operat → BCS | o singură recepție în cub (T-D9, `PoliticaConex`) |
| SC-X-03 | FCL cu avans (INC anterior) → transfer INC → FCL → RDC parțial | două partide, plafonul pe rest, creanța netă după retur |
| SC-X-04 | FCT → PLT parțială → PLT rest → storno PLT a doua | partida se redeschide exact cu suma stornată |
| SC-X-05 | recepție 100 × 10 → BCS 60 → corecție de preț +200 → storno BCS | 091 (h): inversare 600 + compensare 120 înapoi pe lot; datoria rămâne |
| SC-X-06 | ca SC-X-05, apoi storno corecție, apoi storno BCS | fără a doua compensare |
| SC-X-07 | recepție → două corecții succesive → storno BCS | compensarea cumulată pe proveniență |
| SC-X-08 | BTR între gestiuni → BCS din gestiunea primitoare → storno BTR | refuz: lotul mutat are ieșiri |
| SC-X-09 | ASM din două loturi → DSC din produsul rezultat → RLF pe unul din componente | costul produsului nu se re-propagă (granița declarată din design §11) |
| SC-X-10 | închidere de lună cu documente operate → document întârziat în luna închisă → corecție | perioada închisă ca graniță absolută; storno legat + document nou |
| SC-X-11 | `Deschidere` (contabil bloc + stoc pe lot + terți pe partidă) → PLT pe partida de deschidere → FCL nouă | partida fără document se stinge prin aceeași cheie ca una cu document |
| SC-X-12 | închidere de an: 121 → 1174, `Deschidere` a anului nou = `Sold` la 31.12 | F27-r4; snapshot-ul de referință egal cu suma postărilor |
| SC-X-13 | DVI legată la FCT → ajustarea costului pe lot cu taxa vamală → BCS din lot | 86-r1: `Atribuit` pe lot, costul ieșirii după ajustare |

## Tipurile și starea lor

| Cod | Tip | Pe cub din | Fișier | Stare catalog |
|---|---|---|---|---|
| BCS | bon de consum | felia 31 | `BCS.md` | de scris (probele `STR-*`/`NUC-*` existente se mapează pe rânduri) |
| FCT | factură intrare | felia 31 | `FCT.md` | de scris |
| PLT | plată | felia 31 | `PLT.md` | de scris |
| INC | încasare | felia 31 | `INC.md` | de scris |
| BTR | notă de transfer | pas 1 | `BTR.md` | de scris |
| FCL | factură ieșire | pas 2 | `FCL.md` | de scris |
| DSC | descărcare de gestiune | pas 2 (privat) | `DSC.md` | de scris |
| NTC | notă contabilă | pas 3 | `NTC.md` | se scrie cu pasul |
| ITV | închidere TVA | pas 3 | `ITV.md` | se scrie cu pasul |
| RDC | retur de la client | pas 4 | `RDC.md` | se scrie cu pasul |
| RLF | retur la furnizor | pas 4 | `RLF.md` | se scrie cu pasul |
| DVI | declarație vamală | pas 4 | `DVI.md` | se scrie cu pasul |
| ASM | asamblare | pas 5 | `ASM.md` | se scrie cu pasul |
| LDI | listă de inventar | pas 5 | `LDI.md` | se scrie cu pasul |
| NIR | recepție manuală | pas 5 | `NIR.md` | se scrie cu pasul |
| — | `Deschidere` | pas 6 | `DESCHIDERE.md` | se scrie cu pasul |
| PIF/CAS/AMO | imobilizări | TR-D9 | `IMO.md` | la TR-D9 |

Rularea pe un tip: `ModelCheck --scenarii <TIP>[,<TIP>…] [privat]` (091-r1);
scena unui rând se înregistrează cu tipul ei în `ScenelePeTip`
(`nou/tools/ModelCheck/Program.cs`), altfel filtrul nu o vede.

Tipurile deja pe cub primesc fișierul înaintea pasului 3: probele
`STR-*`/`NUC-*` existente se mapează pe rândurile ciclului, iar rândurile
lipsă (corecția în perioadă închisă, citirile) se scriu și se probează
atunci. Recensământul pe clonă (091-r2) se face o dată per tip, înaintea
scenariilor lui, și își lasă cifrele în coloana „Proveniență".
