# C111, pasul 2 — forma nouă a postării

**Data:** 2026-10-10. **Stare:** implementat, verificat pe catalog și în
browser; rămân bazele de dezvoltare de recreat.
Contractul: [C111](c111-coordonate-contract.md), D2–D7 și D10.

Urmele, neversionate: `run-verificari/20261010-210057-804/` (integrala
finală, după review-ul Codex), `run-verificari/20261010-201708-710/`
(integrala dinaintea lui), `run-verificari/20261010-191728-066/`
(`--scenarii C111`).

## Ce s-a schimbat

- **Nucleul.** `Capat` și `Coordonate` poartă `Repartitor` (cu felul lui),
  `Contrapondere`, `PartenerFiscal` și `Loc`. `GestiuniVirtuale` a ieșit.
  `Conservare` ține invarianții 1–4.
- **Cubul.** `Postare` are coloanele `Repartitor`, `Contrapondere`,
  `PartenerFiscal` și `Loc`, cu chei străine pe `Repartitori` pe ambele
  partiții și cu constrângerile `CK_Postare_Contrapondere` și
  `CK_Postare_BlocFiscal`.
- **Scrierea.** Declaranții pun pe fiecare picior repartitorul al cărui sold
  îl mișcă (D5). Partenerul fiscal îl scrie numai `Fiscal.CuFapt`. Locul îl
  scriu `Fiscal.CuFapt`, din latura internă tipată a documentului, și
  `Terti.Capat`, când titularul unei partide ia locul repartitorului intern.
- **Recitirea.** `Randuri.Citeste` primește repartitorii citiți pe lot; un
  repartitor fără fel e refuz (`REPARTITOR_FARA_FEL`).
- **Citirile.** O singură axă analitică. `gestiuneId` a ieșit din balanță,
  fișă și soldul pe parteneri, pe proiecții și pe HTTP. „Sold parteneri” are
  un rând fără terț pe cont. Snapshot-ul contabil n-are `GestiuneId`.
- **Invarianții cubului.** `VerificaCoordonate`
  (`CITIRE_COORDONATE_INVALIDE`) e nou; inversa fiscală se compară și pe
  partener și pe loc.
- **Schema.** Lanțul e o singură migrație, `20261010151835_InitialCreate`,
  generată din model, cu tabela partiționată, funcția `cub_partida_id` și
  view-ul listei scrise în SQL.
- **Uneltele.** Poarta pasului 1 a ieșit, cu proba ei. `--amprente` citește
  secțiunile analitice din coloane. Import1C compilează pe forma nouă.

## Verificarea

| Probă | Rezultat |
|---|---|
| Suita nucleului | 199 din 199 |
| Integrala ModelCheck, bugetar | 3.631 OK, 0 FAIL |
| Integrala ModelCheck, privat | 5.035 OK, 0 FAIL |
| `--scenarii C111`, ambele profiluri | verde |
| Lista de postări în browser | coloanele noi, cu date |
| Drift OpenAPI | zero |
| Metadata clientului | la zi |

Integrala a rulat pe baze create din migrația nouă
(`Atlas.Conta.BackOffice.C111P2`, `Atlas.Conta.ModelCheck.Privat.C111P2`).
Față de pasul 1 (3.579 / 4.860) sunt 52, respectiv 175 de verificări în plus:
scenele C111 și mutanții coordonatelor. Ultimele șase pe fiecare profil vin
din review-ul Codex; bazele au fost recreate din migrația corectată.

Nicio așteptare de sumă pe cont nu s-a schimbat: literalii numerici scoși și
adăugați de diff-ul scenelor existente sunt aceiași, fișier cu fișier.

Lista de postări (S5) e probată în browser pe hostul Blazor, pe o bază
privată din seed (`Atlas.Conta.BackOffice.Privat.C111P2`, pe 5446), cu o
factură de intrare cu recepție și TVA și două note operate prin API. Sub
„Registre → Postări” lista arată `Repartitor`, `Partener fiscal`, `Loc` și
`Contrapondere`: 371 pe gestiune, cu partenerul fiscal și locul; 401 pe
furnizor, cu contraponderea „Terț” pe contracantitate; 4426 pe gestiune, cu
partenerul fiscal și locul; notele numai cu repartitorul. Rețeta,
neversionată: `run-verificari/c111-ui.ps1` și `c111-ui-date.py`.

## Așteptările schimbate

| Schimbarea | Scenele |
|---|---|
| S1 | SC-CIT-27 (230 pe loc, 11 pe furnizor, 241 în total, nimic fără repartitor); SC-CIT-11 și SC-CIT-16 (filtrul pe `repartitorId`); SC-CIT-15 (4426 pe loc, nu pe vamă); SC-DVI-16; NTC și ITV (latura fără repartitor explicit e pe locul notei); cheia atomului din snapshot |
| S2 | STR-FCL-AVANS-DOUA-PARTIDE; STR-FCT-DOUA-PARTIDE; SC-NIR-30 |
| S3, S5 | contraponderea „terț” la FCT, NIR, RLF, RDC și DSC; „inventar” și „consum” la LDI; „transformare” la ASM; STR-VIZUAL-3; STR-SCHEMA-3 și 4 |
| S4 | scrierea brută din D9-P4-I4-TVA-2 primește loc |
| S10, S12 | DEC (SC-DEC-04, 08, 09, 10, 14), FCT, FCL, RLF, RDC, DVI: repartitorul, partenerul fiscal și locul, pe fiecare picior cu fapt fiscal |
| S11, S13, S14 | nicio așteptare existentă schimbată; le țin scenele noi SC-C111-26, 28 și 29 |

Mutanții și scrierile brute din D9 își păstrează fiecare codul de refuz.

## Ce a apărut la implementare

Trei hotărâri ale owner-ului, toate în contract:

- **C111-Q12, S14.** PIF nu poate purta partenerul suportului fără fapt
  fiscal, deci poziția fără fișă îl pierde din cheie. Furnizorul se regăsește
  prin `SuportId`.
- **C111-Q13.** „Sold parteneri” numai cu terți nu se mai închidea cu balanța;
  are un rând fără terț pe cont.
- **C111-Q14.** Piciorul intern cu partidă și fără fapt fiscal își pierdea
  locul. `LocFiscal` a devenit `Loc` și se scrie și acolo. 111 (b) e amendată.

Două forme din catalog nu se pot atinge întocmai:

- **SC-C111-20.** Un storno cu doi originali cu parteneri fiscali diferiți nu
  e produs de declaranții de azi: stornoul inversează și postările atribuite
  altui document, dar împerecherea, singurul scriitor al atribuirii, produce
  transferuri de partide fără fapt fiscal. Scena probează stornoul peste
  operare și transfer, cu perechi suprapuse; forma cu parteneri diferiți
  rămâne neprobată.
- **SC-C111-18.** Felul necunoscut e probat numai la recitire; orice
  repartitor al unui document are fel.

## Ce nu e probat

- Bazele de dezvoltare, pe 5444 și pe 5446 fără sufix, nu sunt recreate; sunt
  pe lanțul vechi și nu corespund codului.
- Import1C compilează, dar n-a rulat pe forma nouă (pasul 6).
  `ScaraVolum.sql` și PerfCub sunt neatinse.
- Clientul React are încă `gestiuneId` printre filtrele din URL
  (`urlStare.ts`); serverul nu mai are parametrul (pasul 7).
- Martorele „azi e refuzat” de la SC-C111-24 și 29 sunt construite pe nucleu,
  nu rulate pe codul vechi.
- Primele trei ramuri ale lui `VerificaCoordonate` nu se pot aprinde cât
  există `CK_Postare_BlocFiscal`; le ține constrângerea.

## Review-ul Codex al implementării

Mesajul: `comunicari/2026-10-10-2038-codex-claude-c111-pas2-review.md`.
Dispozițiile sunt în contract, sub „Review-ul Codex al pasului 2”.

- **C111-P2-R1.** Constrângerea admitea `Loc` pe o postare fără fapt fiscal
  și fără unitate, fiindcă `"FelUnitate" = 2` dă nul. Comparația e acum
  totală; mutantul `COORD-LOC-FARA-UNITATE` o ține.
- **C111-P2-R2.** `INV-CUB` nu cerea forma contraponderii transformării pe
  postările cu pereche: o notă cu ambele postări marcate ieșea din balanță.
  `VerificaCoordonate` o cere pe orice postare marcată; mutantul
  `COORD-TRANSFORMARE-PE-PERECHE` o ține.
- **D10.** Nomenclatorul SAF-T nu reunește partenerul fiscal; textul
  contractului e adus la cod.
- **SC-C111-10.** Rândul fără terț e probat și cu o sumă fără niciun
  repartitor, dinamic și după închidere.

## Cum s-a lucrat

Nucleul, modulul, migrația și HTTP-ul le-a scris sesiunea principală.
ModelCheck (scenele existente, scenele noi, mutanții) l-a adus la zi un agent,
în trei livrări. Fiecare livrare a fost verificată pe diff și prin rerularea
integralei înainte de commit.
