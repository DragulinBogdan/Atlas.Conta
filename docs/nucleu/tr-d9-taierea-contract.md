# TR-D9a — tăierea registrelor și a regimului dual: contract pentru aprobare

- Data: 2026-10-05
- Stare: **aprobat de owner, 2026-10-05**, după închiderea review-ului
  advers Codex. D9-Q1…D9-Q5 sunt tranșate de owner, 2026-10-05 (A/A/A/A/A).
  Implementarea pornește cu pasul 1 din D9-D15, un commit per pas. Review-ul advers Codex al contractului la `63801a8`
  a adus șase observații, D9-RV1…D9-RV6
  (`comunicari/2026-10-05-1032-codex-claude-tr-d9a-contract-review.md`);
  reverificarea la `1fe9cb7` a adus încă trei, D9-RV7…D9-RV9
  (`comunicari/2026-10-05-1108-codex-claude-tr-d9a-reverificare.md`). Toate
  nouă sunt acceptate și aplicate aici, vezi „Amendamente după review-ul
  Codex". **Review-ul advers al contractului e închis la `75138d7`**
  (`comunicari/2026-10-05-1130-codex-claude-tr-d9a-contract-review-inchis.md`).
  Branch `tr-d9-taierea`, tăiat din `main` = `0bcd8b0` (după PR #21).
- **Amendamentul 1 (2026-10-05)**: hotărârile owner-ului după consultarea cub
  vs registre și după inventarul pasului 1, D9-A1…D9-A9, cu textul întreg în
  [`tr-d9-taierea-amendament-1.md`](tr-d9-taierea-amendament-1.md); aplicat
  aici, vezi „Amendamentul 1".
- Bază: 090 (a)(l)(m) și rândul TR-D9 din „Ordinea și regula de oprire";
  091 (g)(6), (h), (k); 108 (a) și „Ce rămâne deschis"; lista nominală X-D2
  (`nou/tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`); restanțele cu
  starea `cade la TR-D9` și `activă, TR-D9` din `docs/decizii/restante.md`.
- Închiderea: decizia **110**.

## De ce două felii (D9-Q1 = A)

TR-D9 din 090 este „unitățile și tăierea". Cele două jumătăți au naturi
opuse:

- **tăierea** scoate cod și nu adaugă comportament contabil. Cubul e deja
  singura sursă citită (108 a); rămâne de scos al doilea scriitor și tot ce
  există numai pentru el;
- **unitățile** adaugă comportament nou: reevaluarea cu `Atribuit`, stornoul
  după reevaluare (091 h, N-r5), reziduul RLF (T-r2), costul vamal pe lot
  (86-r1), valuta pe partidă (B-r6, 107-r4), `PoliticaEvaluare` (51e). Se
  scriu scenarii-întâi.

Făcute împreună, reevaluarea ar trebui scrisă și în registre sau regimul dual
ar trebui rupt la mijlocul feliei, iar review-ul n-ar putea separa „am scos"
de „am schimbat". Tranșat de owner: **TR-D9a = tăierea** (contractul de
față, închide 091 (g)(6)); **TR-D9b = unitățile** (contract propriu, după,
închide 091 (g)(3)). „Rotund" le cere pe amândouă.

## Ce spun sursele că înseamnă tăierea

| Sursa | Cerința | Litera de aici |
|---|---|---|
| 090 (a) | dispar cele patru registre, `Document.TotalStingere`, `TipStoc` ca cheie de registru, flag-ul de storno pe rând, `NumarNota`, `Lot.PretUnitar` ca fapt înghețat (ultimul amendat de D9-Q5: rămâne ca preț de intrare) | D9-D2, D9-D3, D9-D6, D9-D8 |
| 090 (l) | tăierea e greenfield, fără migrare de date; lanțul de migrații se resetează | D9-D6 |
| 090 (m) | din XAF mor cele patru grile pe registre; re-țintirea pe `Postare` e dezghețarea declarată | D9-D9 |
| 090 rândul TR-D9 | motorul vechi dispare; ModelCheck cu probele de formă rescrise; `--dump-integritate-tph` zero; `verifica:drift` zero sau driftul declarat | D9-D2, D9-D7, D9-D15 |
| 091 (g)(6) | citirile de bază din cub și registrele vechi tăiate, fără migrare | D9-D1 |
| 091 (c)(f) | oracolul normalizat nu mai e probă; Import1C compilează, nu se extinde | D9-D7, D9-D11 |
| 108 (a) | clasa 1 a listei nominale (scriitorii duali) și evidența XAF cad la TR-D9 | D9-D2, D9-D9 |
| 108, amendamentul `PartideCuRest` | criteriul de formă amânat la TR-D9, odată cu F27-r16 | D9-D10 |
| X-r2 | subiectul permisiunii pe cifrele citite din cub, după tăierea tipurilor de registru | D9-D9 |
| TR-r9 | inventarul rescrie/șterge al probelor de formă | D9-D7 |
| restanțele `cade la TR-D9` | mecanismul dispare cu registrele; nevoia rămasă se rejudecă pe cub | D9-D13 |

## Fapte măsurate (main `0bcd8b0`, 2026-10-05)

Cifrele sunt numărate pe sursă, nu estimate; inventarul nominal complet e
livrabilul pasului 1.

**Producție.**

- Lista nominală X-D2 are 75 de intrări: 26 scriitor dual, 7 martor, 2
  evidență XAF, 4 autorizare, 6 mapare, 30 legătură (`Imperechere`, rămâne).
  Scriitorii duali stau în 6 fișiere: `MotorOperare` (3), `CorectieService`
  (1), `GardianEditare` (1), `StocService` (7), `Fapte` + `DeclarantAsamblare`
  (3, absorbția ASM-B6), `Documente/Imobilizari.cs` (11).
- `MotorOperare.CalculeazaSiValideaza` construiește încă planul registrelor:
  mișcările de stoc pe `RegulaStoc`, notele pe `RegulaContare`, pasul TVA,
  `RegistruTvaService.Deriva`. Trei lucruri din el **nu** le face contractul
  cubului singur:
  1. scrie `Valoare` pe linia de ieșire din soldul REGISTRULUI
     (`StocService.AplicaValoareIesire`); declaranții evaluează separat, din
     soldul cubului (`N.Evaluare.Iesire`), deci linia și postarea au azi două
     surse;
  2. verifică dimensiunile obligatorii per cont pe notele registrului
     (`VerificaDimensiuniObligatorii`). Pe partea cubului, `VerificaLatura` e
     chemată numai de `ReceptiiConexe` și de `Materializare.Deschidere`;
     `Declaratii/`, `Contractare` și nucleul nu au gardul;
  3. fixează `Lot.PretUnitar` la operarea liniei care naște lotul
     (`MotorOperare.cs:349`, singurul scriitor de producție). Prețul are apoi
     două feluri de consumatori. Ca **fapt istoric**: `PregatesteOperare`
     recalculează prin `BazaLinie` → `Lot.ValoareLaPretulLotului` valoarea
     liniei de RLF și a costului RDC, pe care declarantul o ia apoi ca
     `ValoareDeclarata`; la fel linia NIR-ului conex pe lot străin
     (`DeclarantNir`). Ca **estimare**: aceeași `BazaLinie` pe BCS, BTR, DSC,
     LDI-minus și consumul ASM, valoare pe care operarea o înlocuiește;
     eticheta lotului din lookup-urile XAF; nouă DTO-uri (`LotPret`). Plus
     absorbția ASM-B6, care moare.
- `PosteazaInCub`: 16 apariții în 6 fișiere. Seed-ul are 20 de tipuri; BPR
  nu declară (rezervat, 19), iar DSC, ITV, RDC, RLF, DVI sunt inerte pe
  bugetar (fără politici, `PosteazaInCub = false`).
- `Document.TotalStingere`: scrisă de motor la operare, păzită de gardian,
  proiectată în 6 locuri de `ImperecheriProiectii.Antete`. Proiecția e moartă:
  rezultatul `DocumenteCuRest` își ia deja `Total` din postări
  (`ImperecheriProiectii.cs:141–156`), nu din antet.
- Hook-urile de stingere (`SensDeStins`, `PoateFiStins`, `CapacitateStingere`,
  `SursaStingeriiAutomate`): 8 apeluri în producție (`ImperechereService` 6,
  `NotaContabilaApply` 1, `ImperechereApply` 1). Nu citesc registre.
- `RegulaStoc` mai e consumată, în afara planului vechi, de LDI
  (`DeclarantDiferenteInventar`), DSC (`DescarcareService`, `FacturaIesire`),
  `Fapte` (operand) și „Explică". `TipStoc` trăiește și ca
  `Cont.CategorieStoc` și cheie a `PoliticaMiscareSaft`.
- `NumarNota`: coloană pe `RegistruContabil`; proiecțiile contabile de pe cub
  pun deja numărul documentului în câmpul cu acest nume.
- Autorizare: `RegistrulCitibil` pe trei controllere (ITV, AMO, Imobilizări)
  cere citirea pe tipul unui registru. `PerioadeController.TipuriInsumate` nu
  conține registre: enumeră `Document`, `InchidereTva`, `AmortizareLunara`,
  `Imperechere` și `PoliticaInchidere`.
- ASM: declarantul verifică azi ΣP = ΣR (produsele culese contra consumului
  evaluat de REGISTRE), clasifică grupurile pe P_g = R_g și adaugă produselor
  Δ = C − R, unde C e consumul evaluat pe cub (`DeclarantAsamblare.cs:78–108`).
  Comanda `DistribuieValoarea` își ia ținta din `AsamblareApply.PrezicSumaConsum`,
  care cheamă `MotorOperare.Valideaza` și citește apoi valorile liniilor de
  consum din ObjectSpace-ul temporar — adică valorile scrise de planul vechi.
- XAF: patru liste `[NavigationItem("Registre")]`, configurate în
  `ContaUiBaseline`. Blazor.Server nu referă registrele.

**ModelCheck** (71 de fișiere, 46.887 de linii).

- `Program.cs`: 33.241 de linii, 1.755 `Check(`. Referințele la registre
  sunt pe 508 linii; o parte sunt purje. Pe o delimitare euristică a
  unităților de nivel sus, 55 din 98 ating registrele, iar cele care ating
  registrele sau oracolul țin 27.996 de linii și 1.560 de `Check`. Cifra
  exactă e a inventarului.
- `Nucleu/` e oracolul registre → cub: `CubDinRegistre` (540), `Normalizari`
  (807), `ReconciliereCub` (577), `GatePeBaza` (633), `Comparabil` (187),
  `DiagnosticValoriStoc` (118), `ProbeCub` (328), `ProbeNucleu` (103).
- Scenele de catalog care folosesc registrul ca al doilea martor: 12 fișiere
  `Scenarii*.cs`, între 1 și 5 linii fiecare, plus `ScenaDocumente` (7),
  `AcoperireInvarianti` (7), `PerfCub.Evaluare` (4).

**Unelte și client.**

- Import1C: 57 de referințe la registre și la motorul vechi, în 10 fișiere
  (`ReconciliereLuna` 19, `Deschidere` 8, `Sabotaj` 8, `Alocare` 6, `Reluare`
  5). Migrare: 7. BackfillTva: 8 (unealta derivă și scrie `RegistruTva`).
- Client: `RegistruImobilizari.tsx`, `FisaCont.tsx`, `RegistruJurnal.tsx`,
  `D300.tsx`, `App.tsx` și `generated/api-types.ts`.

## D9-D1 — Perimetrul

**Intră**: scoaterea scriitorilor duali și a planului registrelor din motor
(D9-D2); valoarea liniei de ieșire dintr-o singură sursă (D9-D3); portarea
gardurilor planului vechi care nu sunt ale registrelor (D9-D4); scoaterea
regimului ca dată (D9-D5); schema, migrațiile și bazele (D9-D6); ModelCheck
fără oracol, cu probele de regulă re-țintite (D9-D7); `RegulaStoc` fără
rânduri moarte (D9-D8); evidența XAF și autorizarea (D9-D9); proiecțiile de
rest fără `TotalStingere` (D9-D10); uneltele (D9-D11); clientul și contractul
HTTP (D9-D12); verdictul restanțelor (D9-D13); documentația și invarianții
(D9-D14).

**Nu intră**: reevaluarea, `Atribuit` ca mecanism de produs, stornoul după
reevaluare, valuta, `PoliticaEvaluare`, costul vamal pe lot (TR-D9b);
re-cheierea `PoliticaMiscareSaft` pe cont (TR-r7); hostul fără XAF și async
(IM-r2, IM-r5); rafinarea blocajului scrierii (X-r1); migrarea 1C (091-r4);
orice cerință de produs nouă (090 m).

**Schimbările de comportament ale feliei** sunt o listă închisă. Orice
altceva schimbat e defect.

1. Valoarea scrisă pe LINIA de ieșire evaluată din sold nu mai vine din
   soldul registrului, ci din decizia contractului, deci devine egală cu
   postarea (D9-D3 a).
2. ASM: o culegere cu P = R ≠ C, acceptată azi prin absorbție, e refuzată
   până la redistribuire; după redistribuire, felul tranzacțiilor poate
   diferi de cel de azi (D9-D3, „ASM după tăiere").
3. Cifrele citite din cub cer un singur drept, citirea COMPLETĂ pe `Postare`:
   subiectul se unifică și verificarea e mai strictă decât cea pe tip, de azi
   (D9-D9).
4. Dacă pasul 1 arată că un tip inert pe profil se operează azi fără efect,
   refuzul lui (D9-D5). Pasul 1 a confirmat-o (I6).
5. Recepția liniei de stoc a facturii e păzită de gardul analizei
   obligatorii la operarea facturii; refuzul se mută de pe NIR-ul conex pe
   factură (D9-A3).
6. Regula de contare pe un tip al cărui declarant nu contează prin reguli e
   refuzată la editare (D9-A4).
7. Explicația persistată și API-ul ei rețin regulile de politică consumate,
   cu contorul rândului, în locul textului fix; formatul trece la versiunea 2
   (D9-A8).
8. Postarea de pe piciorul de terț al unui cont cu flag-ul `Repartitor`
   poartă partenerul și când contul nu urmărește partide; gardul analizei
   judecă numai postarea, fără latura documentului (D9-A10).

## D9-D2 — Un singur scriitor: coaja comenzii după tăiere

`MotorOperare` rămâne coaja comenzii (L2). Ordinea după tăiere:

1. gardurile de stare, dată și perioadă; blocajele (108 e), neschimbate;
2. `PregatesteOperare`;
3. validările care nu sunt ale registrelor: TVA cules la taxare inversă,
   `PoliticaValidare`, `ValideazaOperare`;
4. contractul (`Contractare`), cu refuzurile lui;
5. gardurile pe postările contractului: analiza obligatorie (D9-D4), soldul
   intermediar al loturilor, poziția fără fișă, recepțiile conexe;
6. materializarea în cub, cu explicația (108 c);
7. scrierile pe document: numărul, scadența, loturile născute, valoarea
   liniilor de ieșire (D9-D3), conexul, secundarul, starea;
8. împerecherea automată la operare.

`Valideaza` (dry-run) rulează 1–5 și întoarce numai refuzurile, ca azi
(`ComenziDocument.Valideaza` → `IReadOnlyList<string>`). Nu persistă nimic,
dar modifică obiectele din ObjectSpace-ul lui temporar (`PregatesteOperare`),
care se aruncă după apel. Nu ia blocajul scrierii (108 e): spune ce ar refuza
operarea pe faptele citite atunci, nu pe cele de la momentul comenzii.

Valorile lăsate de dry-run pe obiectele din memorie nu sunt rezultat: după
tăiere sunt estimări (D9-D3). Un apelant care le citește azi ca rezultat
trebuie mutat pe evaluarea contractului. Cunoscut: `PrezicSumaConsum` al ASM
(D9-D3); inventarul îi caută pe toți apelanții lui `MotorOperare.Valideaza`.

Un refuz din orice pas anulează întreaga tranzacție de comandă.

Dispar: `PlanOperare.Miscari`, `Note` și `RanduriTva`; `PotrivesteReguliStoc`;
pasul de note și pasul TVA din plan; `RegistruTvaService.Deriva` și rândul
`RandTva`; cele trei scrieri de registru din `Opereaza`, `AnuleazaOperarea`
și `Storneaza`; `IDocumentCuRegistruPropriu` cu cele 11 metode din
`Documente/Imobilizari.cs`; ramura `RegistruTva` din `CorectieService`;
absorbția ASM-B6 (`Fapte.SolduriLoturiRegistru`, câmpul din `Operand`, ramura
din `DeclarantAsamblare`); `StocService` cu cheia `(Lot, Repartitor, TipStoc)`,
în afară de ce pasul 1 dovedește că e regulă pură cu un consumator rămas.

Ce din serviciile vechi e regulă cu consumator viu rămâne; inventarul numește
membru cu membru ce rămâne din `RegistruTvaService`, `AmortizareService`,
`InchidereTvaService`, `DescarcareService` și `LoturiCulegereService`. Un
membru fără apelant după tăiere se șterge.

## D9-D3 — Valoarea liniei de stoc: două surse numite, niciuna registrul (D9-Q5 = A)

Intrarea care naște un lot își poartă valoarea culeasă și nu se atinge. O
linie care scoate stoc dintr-un lot existent, sau îl readuce pe el, își ia
valoarea dintr-una din două surse. Fiecare tip are una singură, numită de
declarant (108 c).

**(a) Evaluată din sold.** BCS, BTR, DSC, minusul LDI, consumul ASM. Valoarea
este decizia `ValoareIesire` a contractului: evaluarea pe soldul cubului, în
secvența liniilor, ultima ia restul (090 j). Coaja o scrie pe linie la
operare, în tranzacția comenzii. Azi o scrie planul vechi, din soldul
registrului; aici e schimbarea 1 din D9-D1.

Invariantul nou din `INV-CUB`, cu mutant. Pentru fiecare linie evaluată din
sold a unui document operat se alege **postarea de ieșire**: cea cauzată de
linie, pe lotul real al liniei, la capătul din care stocul pleacă. Nu se
însumează amândouă capetele și nu intră contraponderea de transformare.
Valoarea ei normalizată este:

- `Valoare`, când postarea e pe Credit (BCS, DSC, minusul LDI, consumul ASM
  în Operare);
- `−Valoare`, când ieșirea e pe Debit, într-un Transfer: gestiunea-sursă a
  BTR (un transfer 4/40 postează acolo Debit −40) și consumul ASM din
  Transfer.

Se cer două egalități, amândouă semnate: valoarea normalizată = valoarea
deciziei `ValoareIesire` a liniei; valoarea deciziei = valoarea liniei
înmulțită cu semnul convenției frunzei (−1 unde frunza ține ieșirea negativă
pe linie: consumul ASM, minusul LDI; +1 la BCS, BTR, DSC). Nu există `Abs`
global: un lot evaluat negativ (S-r7) dă o decizie negativă, iar egalitățile
rămân adevărate cu semn. Invariantul și mutantul se probează pe BCS, pe BTR
și pe un ASM cu Transfer, nu numai pe o ieșire pe Credit.

**(b) Declarată la prețul de intrare.** RLF, costul RDC, linia NIR-ului conex
pe lot străin. `PregatesteOperare` calculează valoarea liniei din cantitate ×
`Lot.PretUnitar`, iar declarantul o ia ca `ValoareDeclarata`. Nu se schimbă
nimic: nici calculul, nici sursa. Soldul curent nu poate ține locul prețului
de intrare — pe un lot golit raportul nu mai există (SC-RDC-07: recepție
10/100, ieșire 10/100, retur 2/20), iar pe un lot cu reziduu ar șterge
reziduul pe care T-r2 îl lasă explicit pentru TR-D9b (SC-RLF-05: ultima
bucată iese la 10,01, lotul rămâne 0/−0,01). NIR-minus rămâne pe
recepția-sursă.

**`Lot.PretUnitar` rămâne, ca preț de intrare** (tranșat de owner, D9-Q5).
E dată a lotului, scrisă o dată: de coajă, la operarea liniei care îl naște
(punctul 7 al cojii, D9-D2), sau de cel care deschide soldul (094). Servește sursa
(b), estimarea de draft a sursei (a), eticheta din lookup-uri și DTO-urile
`LotPret`. Nu evaluează nicio ieșire din sold: după scoaterea absorbției
ASM-B6, niciun declarant al sursei (a) nu-l citește. Amendează 090 (a) pe
acest punct; amendamentul se consemnează în 110. Ce valoare ia un retur pe un
lot de deschidere fără preț de intrare se măsoară la pasul 1 și se păstrează.

**Draftul și dry-run-ul.** Pe draft, valoarea unei linii din sursa (a)
rămâne estimarea de azi, cantitate × prețul de intrare; nu e promisiune.
Dry-run-ul validează cu aceleași reguli ca operarea, dar nu întoarce valori:
răspunsul lui rămâne lista de refuzuri și contractul HTTP nu se extinde
(D9-D12). Valoarea decisă se vede pe linie după operare și în explicația
tranzacției.

**Validările de frunză pe valoarea finală.** Azi `ValideazaOperare` rulează
după ce planul vechi a scris valoarea de golire; după tăiere ar vedea
estimarea. O validare care judecă valoarea finală (invariantul valoric ASM)
se mută după contract sau în declarant. Inventarul le numește pe toate.

**ASM după tăiere.** ASM-B6 a fost aprobată „numai în regimul dual" și
spune ce urmează: absorbția se scoate pentru operațiile noi, iar distribuirea
folosește calculul pur, disponibil înaintea gardului de balansare. Aici se
fixează:

- R dispare. Gardul de balansare devine **ΣP = ΣC**: produsele culese contra
  consumului evaluat pe cub. Dezechilibrul refuză atomic cu
  `ASAMBLARE_NEBALANSATA`, cu C, P și diferența;
- grupurile pe cont se clasifică pe **P_g = C_g**: grupul balansat intră în
  Transfer, restul în Operare. Forma ASM-B2 și regula n→m din ASM-B3 nu se
  schimbă: contraponderile cantitative, cel mult o Operare și un Transfer;
- nu mai există Δ. Produsul postează valoarea culeasă, consumul valoarea
  evaluată. Dispar decizia `AbsorbtieEvaluare` și refuzul
  `ASAMBLARE_DELTA_FARA_ANCORA`; `ASAMBLARE_PRODUS_NEPOZITIV` rămâne pe
  valoarea culeasă a produsului;
- **distribuirea citește evaluarea, nu liniile.** `DistribuieValoarea` își ia
  ținta din evaluarea consumurilor făcută de declarant pe operandul închis al
  draftului — același cod pe care `Declara` îl rulează înaintea gardului de
  balansare, nu o a doua formulă. Evaluarea e disponibilă și când contractul
  întreg refuză tocmai fiindcă produsul nu e încă balansat; o oprește numai
  un refuz al evaluării înseși (lot lipsă, stoc insuficient). Nu trece prin
  `MotorOperare.Valideaza`, nu persistă nimic și nu schimbă contractul HTTP;
- sursa valorii produsului (`PretEvaluare`) și regula distribuirii pe
  produse nu se ating; 75-r4 rămâne la TR-D9b.

Consecința e schimbare de comportament, nu numai de cod. SC-ASM-17: după
prima ieșire 1/3,33 din lotul 3/10, soldul cubului e 2/6,67; al doilea ASM
consumă 1 la 3,34 cu produsul cules la 3,33. Azi trece (P = R = 3,33, Δ =
+0,01 pe produs). După tăiere e refuzat până la redistribuire; după ea,
produsul e 3,34 și operarea trece. Fără o distribuire care citește evaluarea,
operatorul n-ar putea ieși din refuz: predicția ar da tot 3,33.

Rândurile de catalog atinse se rescriu nominal la pasul 2, înaintea codului.
Niciunul nu se reclasifică drept probă de formă:

| Rând | După tăiere |
|---|---|
| SC-ASM-17 | al doilea și al treilea ASM (culese 3,33 și 3,34 contra C = 3,34 și 3,33) sunt refuzate până la redistribuire; după ea produsele sunt 3,33; 3,34; 3,33 și sursa 0/0 |
| SC-ASM-18 | stornoul inversează exact 3,34; corecția cere produsul la valoarea evaluată, fără Δ |
| SC-ASM-19 | proba-capcană rulează prin comanda reală `DistribuieValoarea`: refuz înaintea redistribuirii, operare după, pe aceleași fapte |
| SC-ASM-20 | P_total 13,33 contra C 13,34: refuz. După redistribuire, așteptare derivată, nerulată: distribuirea proporțională a lui 13,34 dă 3,33 și 10,01 (ultimul ia restul), deci contra consumurilor 3,34 și 10 amândouă grupurile sunt nebalansate și intră într-o singură Operare, fără Transfer. Fixture-ul exact și măsurile complete se fixează la pasul 2 |
| SC-ASM-21 | grup numai-consum C = 3,34 și produs 3,33 pe alt cont: refuz; după redistribuire, Operare cu produsul 3,34 |
| SC-ASM-22 | ΣC = 9,98 + 1,02 = 11 = ΣP: trece fără redistribuire, produsele rămân 10,99 și 0,01; grupul numai-consum și grupul produselor intră în Operare. Dispare aserția acumulării Δ, rămân cifrele și acceptarea |
| SC-ASM-23 | nu mai e `ASAMBLARE_DELTA_FARA_ANCORA`: ΣC = 1,01 contra ΣP = 1 e `ASAMBLARE_NEBALANSATA`; după distribuirea lui 1,01, grupul numai-consum de 0,01 și grupul cu consum 1 și produs 1,01 intră în Operare. Limitarea duală dispare |
| SC-ASM-24 | fără obiect în forma cu Δ. Datele probei vechi (P = 10 contra C = 9,99 sau 9) dau acum refuz de DEZECHILIBRU și pot proba gardul P = C; refuzul pe produsul cules nepozitiv e alt motiv și are proba lui separată |
| SC-ASM-25 | lanțul rămâne numai pe cub: produsul 1/3,34 golit de BCS la 3,34, lot 0/0 |
| SC-ASM-26 | fără obiect în greenfield (102 b) |

Probele pure ale absorbției (`ProbeAsmOperand`, proprietățile nucleului pe
`AbsorbtieEvaluare`) se rescriu pe gardul P = C sau rămân fără obiect, rând
cu rând în inventar. Ținta 0/0 din ASM-B7 devine probă: trei BCS sau ASM de
câte 1 dintr-un lot 3/10 lasă lotul la 0/0, nu la 0/−0,01.

**Ce se închide.** T-r13 și T-r7: bazele se recreează (102 b), deci nu există
istoric divergent de tratat; SC-ASM-26 (stornoul unui original dual) rămâne
fără obiect și se marchează așa în catalog. Limita „golirea se decide la
momentul operării" (retro-ul lasă reziduu) rămâne, declarată; eliminarea ei e
reevaluare (TR-D9b).

**Probele**, scrise înaintea codului (pasul 2), cu cifra de azi și cu cea de
după, pe valorile LINIILOR, nu numai pe soldul final:

- sursa (a): un BCS cu două linii de câte 1 pe același lot 3/10 — 3,33 și
  3,34 pe linii și pe postări; dry-run-ul dinainte fără refuz și fără nimic
  persistat; ținta 0/0 de mai sus;
- ASM: aceeași culegere P = 3,33 / C = 3,34 refuzată atomic, `DistribuieValoarea`
  dă 3,34, operarea trece pe aceleași fapte; a doua distribuire nu schimbă
  nimic; predicția nu lasă nimic persistat; un caz cu grupuri pe conturi
  diferite, cu felurile și măsurile așteptate scrise înaintea implementării;
- sursa (b): SC-RDC-07, SC-RLF-05 și SC-RLF-13 își păstrează cifrele, pe
  linii și pe partidă, inclusiv după anulare și reoperare și prin corecție.

## D9-D4 — Gardurile planului vechi care supraviețuiesc

**Regula.** Fiecare refuz al planului vechi are, înaintea ștergerii, unul din
două verdicte scrise în inventar: *echivalent pe contract* (cod de refuz +
scenariu care îl probează) sau *al registrului, moare*. Niciun refuz nu
dispare fără rând în tabel.

Cunoscute înaintea inventarului:

| Refuzul de azi | Verdict |
|---|---|
| dimensiunile obligatorii per cont, pe nota de registru | trece pe postările contractului, cu regula de mai jos |
| contul debitor/creditor nu se poate rezolva | echivalent în `Contari.Rezolva`; de confirmat pe fiecare tip |
| linia intră în regulile de stoc fără lot | echivalent în declaranți (`LotLipsa`); de confirmat pe fiecare tip |
| tipul de TVA șters sau fără cont de TVA | de confirmat în `Declaratii/Fiscal` |
| soldul negativ pe cheia de registru | moare; soldul cubului e păzit de `Loturi.VerificaSoldIntermediar` |
| `NirRegimInactiv` | moare cu regimul |
| linia fără regulă de contare, sărită tăcut de planul vechi | deja refuz în declaranți (B-r10) |

**Gardul analizei obligatorii, portat.** Regula de azi
(`MotorOperare.VerificaLatura`) se păstrează întreagă:

- pe fiecare postare din domeniu se verifică flag-urile contului ei, cu
  maparea folosită deja de `Materializare.Deschidere`: `Repartitor` →
  partenerul sau gestiunea postării, `Material` → produsul, restul → analiza;
- **angajamentul ține loc de cod economic**: `CodEconomic` lipsește numai
  dacă lipsesc și codul, și angajamentul liniei. Gardul primește
  angajamentul de pe linia cauzală a postării (`LinieOperand.AngajamentId`,
  prin `LinieId`). `Materializare.Deschidere` trimite `null` fiindcă
  deschiderea n-are linie; modelul ei nu se copiază ca gard general;
- **domeniul** sunt postările contractului care țin locul notelor de azi:
  cele născute din contarea unei linii — regula de contare, postarea
  explicită, taxa — în `Carte = Contabil`. Nu intră cartea fiscală, mutările
  (BTR) și transformările (ASM, în amândouă felurile, cu contraponderile
  lor): planul vechi nu le face note, fiindcă BTR și ASM n-au reguli de
  contare în seed pe niciun profil, deci azi nu sunt păzite. Nu intră nici ce
  păzesc deja `ReceptiiConexe` și deschiderea. Un ASM de fel `Operare`
  postează pe conturi, dar n-a avut niciodată notă: păzirea lui ar fi refuz
  NOU, nu echivalent, și rămâne restanța D9-r1. Pasul 1 confirmă domeniul tip
  cu tip, contra notelor pe care planul vechi le verifică azi, și pe
  politicile editabile, nu numai pe seed: o regulă de contare adăugată de
  client pe un tip din afara domeniului se numește în inventar cu verdict;
- **recepția facturii (D9-A3)**: intră și postările recepției liniei de stoc
  făcute de factură (`DeclarantFacturaIntrare.Receptia`), deși nu se nasc
  dintr-o regulă de contare. Diferența postată de NIR-ul conex rămâne păzită
  de `ReceptiiConexe.VerificaAnaliza`;
- **repartitorul (D9-A10)**: gardul judecă numai capătul postării, `Partener`
  sau `Gestiune`. Piciorul intern poartă gestiunea, ca azi. Piciorul de terț
  al unui cont cu flag-ul `Repartitor` poartă partenerul, adică terțul
  capătului (la decont și la nota contabilă terțul e al liniei, nu latura
  externă a documentului), și când contul nu urmărește partide; partida
  rămâne legată de `UrmarestePartide`. Convenția pozițională a notei vechi
  (debit ← predator, credit ← primitor) nu se reproduce. B-D8 pct. 4 se
  amendează corespunzător. Până la pasul 6b gardul mai citește latura
  documentului.

Gardul se adaugă pe partea contractului **înaintea** tăierii (pasul 2), cât
timp planul vechi încă rulează: aceleași documente trebuie refuzate de
amândouă. Fiindcă gardul vechi îl poate masca pe cel nou pe toată durata
regimului dual, cel nou e o funcție chemabilă singură și are proba lui
directă, pe lângă proba prin comandă. Scena bugetară obligatorie: trece cu
angajament și fără cod economic; refuză atomic când lipsesc amândouă; trece
cu cod economic explicit.

## D9-D5 — `PosteazaInCub` dispare; tipul care nu declară

Coloana, alinierea din seed, `Materializare.AreTranzactii`, gardul
`POSTEAZA_IN_CUB_IREVERSIBIL` și ramurile `if (!tip.PosteazaInCub)` dispar.
108 (h) rămâne fără obiect; amendamentul se consemnează în 110. S-r6 se
închide.

Orice tip operabil declară. Operarea unui tip fără declarant (BPR) sau fără
politică pe profil (DSC, ITV, RDC, RLF, DVI pe bugetar) este refuz cu un
singur cod, pe toate ușile. Ce se întâmplă azi la operarea unui tip inert pe
bugetar se măsoară la pasul 1. Dacă azi trece, refuzul e schimbare de
comportament declarată: are scenariu cu comportamentul de azi și cu cel de
după, intră la pasul 6 și e a doua excepție a regulii de oprire.

Ramura `IMPERECHERE_FARA_EFECT` pentru „ambele documente trebuie să posteze
în cub" dispare; refuzul pe efect nul rămâne (108 i).

## D9-D6 — Schema, migrațiile, bazele

**Dispar**: tabelele `RegistruContabil`, `RegistruStoc`, `RegistruTva`,
`RegistruImobilizari`; coloanele `Document.TotalStingere`,
`TipDocument.PosteazaInCub`, `TipDocument.LaturaContPropriu` (T-r9).

**Rămân**: `Tranzactie` și `Postare`; `SoldPerioadaContabil` și
`SoldPerioadaStoc` (snapshot-urile cubului, 088, 103); `Imperechere`
(legătura, 101); `Lot.PretUnitar` (prețul de intrare, D9-D3); `RegulaStoc` și
`TipStoc` în forma din D9-D8.

Lanțul de migrații se resetează într-un singur `InitialCreate`, pe
precedentul C102 (102 e). SQL-ul cubului — partițiile, cheia compusă,
FK-urile per partiție, indecșii cititorilor, explicația — se pliază în el,
scris de mână (S-r4). `STR-SCHEMA` rămâne proba divergenței declarate dintre
snapshot-ul EF și bază. Bazele de dezvoltare se recreează (102 b); nu există
migrare de date.

Dacă nu cere mai mult decât scoaterea din model, `HCategory` iese din
`DbContext` în același reset (106-r5); altfel rămâne restanță.

## D9-D7 — ModelCheck: oracolul moare, probele de regulă se re-țintesc (TR-r9)

(a) **Oracolul dispare.** `Nucleu/CubDinRegistre`, `Normalizari`,
`Comparabil`, `ReconciliereCub`, `GatePeBaza`, `DiagnosticValoriStoc`,
modurile `--reconciliere-cub` și `--declaratie-pe-baza`, `STR-RECONCILIERE`,
acoperirile registru → cub din `INV-CUB` (contabilă, cantitativă de stoc, a
fișelor) și mutanții lor. Nu mai există al doilea scriitor de comparat.
108 (g) și (j) se amendează în 110. Invarianții interni ai cubului rămân:
echilibrul, conservarea transferului, explicația, taxa postată = taxa liniei
(109), plus cel nou din D9-D3.

(b) **Probele pe sursă.** Lista nominală X-D2 se reduce la clasa `Legatura`.
Se adaugă proba numelor interzise: în sursa C# și TypeScript din `nou/`, nicio
referință de cod la entitățile `RegistruContabil`, `RegistruStoc`,
`RegistruTva`, `RegistruImobilizari`, la membrii `PosteazaInCub` și
`TotalStingere`, la `IDocumentCuRegistruPropriu`, `StocService` și
`CubDinRegistre`. Proba e pe identificator întreg, ca X-D2, deci nu atinge
numele de rapoarte păstrate de D9-D12 (`RegistruImobilizariDto`, registrul-
jurnal) și nici serviciile care rămân cu alt nume (`RegistruTvaService`).
Excepțiile sunt o listă închisă: fișierul probei și codul generat. `docs/` nu
e în domeniu; istoricul rămâne acolo. Proba se activează la pasul 7, după
ultimul consumator portat și în același commit cu eliminarea declarațiilor;
înainte ar pica legitim. Proba cititorilor de `Postare` (091-r3) și a
blocajului scrierii rămân neschimbate.

(c) **Trei verdicte pe fiecare referință** din scenele `Program.cs` și
`Scenarii*.cs`, în inventarul pasului 1:

- *purjă* — linia dispare;
- *formă* — aserția moare: numărul de rânduri de registru, `NumarNota`,
  flag-ul `Storno`, cheia `TipStoc`, egalitatea registru = cub;
- *regulă* — aserția se re-țintește pe intrarea comună a cubului, cu aceeași
  cifră: `Citiri.Contabil`, `Loturi`, `Partide`, `Fiscale`, `Imobilizari`.

O aserție de regulă nu se șterge pe motiv că e „acoperită de catalog" fără
să fie numit rândul `SC-…` care o acoperă.

(d) **Re-țintirea se face sub regimul dual** (pasul 3), înaintea tăierii.
Cât timp registrele încă se scriu, aserția veche și cea re-țintită rulează pe
aceeași scenă și dau aceeași cifră; abia apoi cea veche se șterge. O aserție
care nu se poate re-ținti fără altă cifră e ori diferență declarată deja
(N-r3, T-D2.2, T-D4.2, D9-D3), ori defect: se raportează, nu se ajustează.

(e) **Contabilitatea probelor.** Numărul de `Check` înainte și după se
raportează pe scenă; fiecare scădere are rând în inventar.

## D9-D8 — `TipStoc` și `RegulaStoc` după tăiere

Cheia de registru `(Lot, Repartitor, TipStoc)` dispare. `TipStoc` rămâne
vocabular de politică: categoria contului de stoc și cheia
`PoliticaMiscareSaft`.

Un rând `RegulaStoc` fără consumator după tăiere iese din seed, pe ambele
profiluri, și din „Explică": politica nu descrie comportament pe care motorul
nu-l are (4). Consumatorii rămași se numesc în inventarul pasului 1. Dacă
după tăiere `RegulaStoc` nu mai are niciun consumator care să nu poată citi
structura din conturi, tabela dispare cu totul; altfel rămâne, redusă.

Re-cheierea `PoliticaMiscareSaft` pe `(TipDocument, Cont, Semn, Gestiune
virtuală)` din 090 (g) nu intră: rămâne TR-r7.

## D9-D9 — Evidența XAF și subiectul permisiunii (D9-Q2 = A, D9-Q3 = A)

Cele patru liste XAF ale registrelor dispar cu tipurile lor. Tranșat de
owner:

- le înlocuiește o singură listă XAF read-only pe `Postare`, în mod
  `ServerView` (85), sub „Registre", cu tranzacția, felul, documentul,
  contul, latura, partenerul, gestiunea, unitatea și cele trei măsuri; fără
  editare, fără detaliu editabil. E dezghețarea declarată de 090 (m);
- subiectul permisiunii devine dreptul de citire pe `Postare`, unic. Cele
  trei porți `RegistrulCitibil` îl cer în locul tipului de registru.
  Verificarea închiderii își păstrează toate drepturile din `TipuriInsumate`
  (documente, AMO, ITV, împerecheri, politica închiderii) și primește
  `Postare` pe lângă ele: constatările ei însumează postări. Dreptul pe
  `Postare` completează, nu înlocuiește drepturile pe celelalte date
  consumate;
- cifrele se însumează pe ușa de sistem, deci dreptul cerut e citirea
  COMPLETĂ: un rol cu criteriu de rând sau de membru pe `Postare` primește
  403 înaintea citirii, prin `Api.AccesComplet`, ca la explicație (108 d).
  E mai strict decât verificarea pe tip de azi și e schimbare declarată
  (D9-D1), împreună cu pierderea separării contabil / imobilizări.

`Postare` și `Tranzactie` devin tipuri vizibile pe ușa securizată XAF. Ce
urmează din asta se pin-uiește aici, fiindcă azi cubul nu apare în UI și nu
intră în metadata clientului (S-D1):

- gardianul le refuză scrierea pe orice ușă securizată, ca azi registrelor
  (14). Azi are cazuri nominale numai pentru registre și snapshot-uri, iar
  `Postare` e POCO în afara `EntitateConta`; o listă fără butoane nu ține
  loc de gard;
- nu se expun prin OData și nu intră în metadata clientului React;
- lista nu ocolește regula `Transfer` (108 b): e evidență brută, cu felul
  tranzacției ca coloană, nu raport pe cont. Nu are totaluri.

Probele, fiecare pe ce poate dovedi:

- **gardul**: pe un ObjectSpace SECURIZAT, cu administratorul, crearea,
  modificarea și ștergerea unei `Postare` și a unei `Tranzactie` sunt
  refuzate la commit și nu lasă nimic în bază — șase cazuri, în ModelCheck;
- **calea motorului**: aceeași comandă, prin ObjectSpace-ul de sistem, scrie
  — proba pozitivă, separată;
- **neexpunerea**: cererile HTTP pe OData și REST către cele două tipuri nu
  găsesc rută, iar tipurile nu apar în `$metadata` și în `api-types.ts`. Asta
  dovedește neexpunerea, nu gardul;
- **matricea de acces** (`refuzuri.ps1`, pe host viu): dreptul unic de
  citire pe cele trei porți și pe verificarea închiderii, plus restricția de
  rând și cea de membru pe `Postare`. Numărul de verificări crește cu rândurile noi; criteriul e zero
  FAIL de două ori la rând, nu un total fix;
- **lista în browser**, pe hostul real: paginare și filtrare în `ServerView`,
  rânduri din amândouă partițiile identificate corect, navigația spre
  tranzacție fără editare. Cheia e compusă numai în bază (S-r4); dacă asta
  rupe lista se află aici, nu din plan.

## D9-D10 — Proiecțiile de rest fără `TotalStingere` (F27-r16, F28-r1)

Sunt două lucruri separate.

**(a) Scoaterea proiecției moarte.** `ImperecheriProiectii.Antete` nu mai
proiectează `Document.TotalStingere`, iar coloana dispare. Rezultatul
`DocumenteCuRest` își ia deja totalul, restul și candidații din postări, deci
ruta, DTO-ul și cifrele nu se schimbă; proba e catalogul neschimbat.

**(b) Forma proiecției** (F27-r16, F28-r1): uniunea per tip,
`CandidatiPereche<T, TOpus>` și criteriul de formă amânat de owner la X-D5
(`PartideCuRest`: accesul la `Postare` nu depinde de istoric). Se judecă
aici: ori se închide — documentul deschizător al partidei se citește fără
parcurgerea tuturor postărilor de partidă —, ori owner-ul îl re-amână
explicit, cu cifra. Nu se închide prin tăcere. Orice schimbare a mulțimii de
candidați e schimbare de semantică și cere scenariu; (a) nu depinde de (b).

**Precizare (owner, 2026-10-06).** Scara de volum a arătat o creștere
pătratică a partidelor cu rest și a snapshot-ului de partide. Cauza nu e
criteriul de formă, ci îmbinarea soldurilor cu originile pe chei nulabile.
Ea se corectează în felie, la pasul 5c, înaintea tăierii, fără schimbare de
semantică și fără schimbare de schemă. Criteriul de formă propriu-zis rămâne
picat și e re-amânat explicit, cu cifra: 2,7 s de SQL pentru 488.733 de
partide cu rest la 5 milioane de postări
([`tr-d9-pas5c-imbinare-partide.md`](tr-d9-pas5c-imbinare-partide.md) §3).

Hook-urile de stingere nu citesc registre și rămân neatinse în felie
(D9-Q4 = A): `ImperechereService` și cele două adaptoare le cheamă ca azi.

## D9-D11 — Uneltele

- **Import1C** rămâne înghețat (091 f): compilează, nu se extinde. Citirile
  lui de registre trec pe intrările comune ale cubului; contractele 1 și 2
  ale reconcilierii (`ReconciliereLuna`) citesc cubul (109-r1). `Sabotaj`,
  `Alocare` și `Reluare` se portează numai cât cere compilarea; ce nu mai are
  sens fără registre se șterge, cu rând în inventar. O rulare pe ianuarie se
  face la închidere ca diagnostic, cu raportul comparat cu
  `run-verificari/r3-ian-final2/`; nu e gate (091 d).
- **Migrare**: se portează sau se șterge, după ce arată inventarul. Ștearsă
  la pasul 5 (2026-10-06): scria deschiderea numai în registre, fără nimic în
  cub, deci o bază deschisă de ea era deja refuzată de invarianții cubului.
- **BackfillTva** scrie `RegistruTva` și nu are obiect pe cub: se șterge.

## D9-D12 — Clientul React și contractul HTTP

Numele de raport rămân (`RegistruImobilizariDto`, registrul-jurnal): sunt
rapoarte, nu tabele. Din `api-types.ts` și din metadata ies numai tipurile
entităților de registru și câmpurile scoase (`posteazaInCub`,
`totalStingere`); driftul e declarat în contract la pasul 7, cu lista, și
`verifica:drift` trebuie să fie zero după regenerare. Clientul încetează să
citească aceste câmpuri la pasul 4, înaintea dispariției lor. Răspunsul de
validare nu se extinde: dry-run-ul întoarce refuzuri, nu valori (D9-D3).
Paginile React de detaliu rămân înghețate (104 d); listele și proiecțiile se
ating numai cât cere un câmp scos.

## D9-D13 — Verdictul restanțelor

Aprobat odată cu contractul; se scrie în `restante.md` la închidere.

| Id | Verdict |
|---|---|
| S-r6, T-r7, T-r13, IM-r1, IM-r3 | se închid prin tăiere (D9-D2, D9-D3, D9-D5) |
| 098-r3 | se închide numai cu proba: după stornoul sau anularea unui NIR acoperit cu consum (SC-X-01), o operație ulterioară pe același lot, refuzată azi de garda registrului, e acceptată; dispariția codului nu ajunge |
| 64h, 86-r13, F27-r11 | dimensiunea `Repartitor` a rândului de registru dispare, partenerul stă pe postare (090 c); se închid dacă pasul 1 confirmă că niciun cititor nu mai grupează pe laturile documentului, cu proba pe `Partener`, care vine din pasul 6b (D9-A10) |
| 75-r4 | NU se închide: privește `AsamblareDetaliu.PretEvaluare`, sursa valorii produsului (`round(q × preț)` ≠ Σ consum pe cantități mari: consum 100,00, produs 30.000 ⇒ 99,99), care supraviețuiește tăierii. Rămâne deschisă, cu destinația TR-D9b: valoarea produsului ca repartizare a consumului |
| TR-r9 | în felie (D9-D7) |
| F27-r16, F28-r1 | în felie (D9-D10) |
| X-r2 | în felie (D9-D9, D9-Q3) |
| 109-r1 | în felie (D9-D11) |
| T-r9 | în felie (D9-D6) |
| 106-r5 | parțial în felie (`HCategory`), restul rămâne |
| 76-r1, 76-r2, 76-r3, 86-r11, T-r5 | hook-urile de stingere: TR-D9b (D9-Q4 = A) |
| 104-r2 | `după PoC` (D9-Q4 = A) |
| N-r5, T-r2, 86-r1, B-r6, 107-r4, 51e | TR-D9b |
| 097-r2 | nu e a tăierii; rămâne activă, se tranșează în TR-D9b |
| 097-r3 | blocajul IMO rămâne; se rejudecă odată cu X-r1 (după PoC) |
| F27-r12 | snapshot-ul e al cubului; rămâne, `după PoC` |
| F26-r5 | se rejudecă pe cub; `după PoC` |
| T-r3 | regula rămâne (fără partener nu se inventează partidă); excluderea nominală din reconciliere dispare; limită declarată |
| B-r3 | se rejudecă la pasul 6: conexul nu mai are registre de servit |
| 63f | se judecă la pasul 1, pe cod |

Restanță nouă a feliei:

- **D9-r1** — gardul analizei obligatorii pe mutări și transformări (BTR,
  ASM): azi nepăzite, fiindcă planul vechi nu le face note; rămân nepăzite.
  `după PoC`.
- **D9-r2** — nu se deschide: regula de contare fără consumator e refuzată
  la editare, în felie (D9-A4).
- **D9-r3** — explicația de audit dincolo de nivelul 1: conținutul regulii
  la momentul operării (din jurnalul de audit sau din politici cu istoric
  propriu) și nomenclatoarele care decid contarea (D9-A8). `după PoC`.
- **D9-r4** — cititorii care folosesc cheia de pereche: contrapartida din
  fișa contului și conturile corespondente din registrul jurnal (D9-A2).
  `după PoC`.

Rândurile de catalog cu starea `amânat la TR-D9` se împart la fel: cele care
privesc registrele (limita duală din SC-X-01, garda din NIR, SC-ASM-19) se
închid aici; reevaluările și compensările (SC-X-05/06/07, SC-X-09, SC-X-13)
trec la TR-D9b.

## D9-D14 — Documentația și invarianții

În același commit cu pasul care schimbă comportamentul: `stare-curenta/`
pierde regimul dual, registrele și limitele lor; `limite-curente` pierde
limitele duale; `docs/invarianti.md` trece din „starea-țintă până la TR-D9"
în literă curentă (I, III, VI). Decizia 110 consemnează amendamentele la 108
(a, g, h, j) și la 090 (l). CLAUDE.md §Stare se rescrie ca stare, fără
rezumat de felie (091 l).

## D9-D15 — Pașii, verificarea, regula de oprire

Un commit per pas. Fiecare pas compilează toată soluția, cu uneltele
(Import1C, ProbeHttp; Migrare până la ștergerea ei, la pasul 5), și are
integrala verde pe ambele profiluri.
Nu există derogare tacită: un pas care nu poate fi verde singur se unește cu
vecinul lui, în contract, înaintea execuției.

Ordinea ține o singură regulă: **întâi se portează consumatorii, sub regimul
dual; simbolurile dispar la sfârșit, într-un pas atomic.**

| Pas | Ce | Schimbă comportament |
|---|---|---|
| 0 | contractul aprobat; review advers Codex al contractului | nu |
| 1 | inventarul nominal, în `docs/nucleu/tr-d9-inventar.md`: refuzurile planului vechi cu verdict și domeniul gardului analizei, tip cu tip, pe seed și pe politicile editabile (D9-D4); fiecare referință ModelCheck cu verdict (D9-D7 c); validările de frunză pe valoarea finală, apelanții lui `Valideaza` care citesc valori din OS-ul temporar și returul pe lot fără preț de intrare (D9-D2, D9-D3); consumatorii `RegulaStoc`; operarea tipului inert azi (D9-D5); membrii rămași ai serviciilor vechi (D9-D2); 63f | nu |
| 2 | sub dual: gardul analizei obligatorii pe postări, cu proba lui directă; scenariile D9-D3 pe valorile liniilor, cu cifra de azi și cu cea de după; rândurile ASM rescrise nominal, cu felurile și măsurile de după; scenariul 098-r3; scenariul tipului care nu declară; recepția facturii în gardul analizei, cu scena ei bugetară (D9-A3); scenariul regulii de contare fără consumator (D9-A4) | numai refuzuri echivalente cu cele de azi; schimbarea 5 |
| 3 | sub dual: re-țintirea aserțiilor de regulă pe cititorii cubului (D9-D7 d); clasa „repartitorul de pe latura notei" rămâne pasului 6b | nu |
| 4 | sub dual: consumatorii din produs — lista XAF pe `Postare` cu gardul ei, permisiunea unică pe cele trei porți și pe verificarea închiderii, proiecția moartă `TotalStingere` și forma proiecției de rest, clientul fără câmpurile care vor ieși (D9-D9, D9-D10, D9-D12). Listele vechi de registre rămân până la pasul 7. Precizările I3, I4 și I8 (D9-A6) | schimbarea 3: subiectul unic și citirea completă |
| 5 | sub dual: uneltele — Import1C și Migrare pe cititorii cubului, fără `StocService` și fără planul vechi; BackfillTva șters (D9-D11) | nu |
| 5b | probele dinaintea tăierii, fără cod de produs: ultima reconciliere registre ↔ cub arhivată, `PerfCub` în regim dual ca termen de comparație, scara de volum cu ruta securizată, raportată owner-ului (D9-A1) | nu |
| 5c | îmbinarea partidelor pe chei nulabile: cititorii de partide îmbină soldurile cu originile prin egalitate simplă, originea de document se recunoaște numai după identitate; scara re-măsurată pe aceeași bază (D9-D10 (b), precizarea din 2026-10-06). Făcut: [`tr-d9-pas5c-imbinare-partide.md`](tr-d9-pas5c-imbinare-partide.md) | nu |
| 6 | nimic nu mai scrie, nu mai citește și nu mai ramifică pe registre și pe regim: planul vechi, valoarea liniei din contract, ASM pe gardul P = C cu clasificarea pe C și distribuirea pe evaluarea pură, ramurile `PosteazaInCub` și refuzul tipului care nu declară, oracolul și martorii registru → cub cu mutanții lor (D9-D2, D9-D3, D9-D5, D9-D7 a). Refuzul regulii de contare fără consumator (D9-A4). Inversa fiscală născută finală, fără `ReatribuieInversaFiscala` (D9-A5). În același pas, lista nominală X-D2 și probele care asertează vechii scriitori se aduc la zi; nu rămâne o listă exactă învechită până la 7. Rămân numai declarațiile: entitățile, maparea, coloanele, listele XAF vechi, cazurile gardianului | schimbările 1, 2, 4 și 6 |
| 6b | după tăiere, fără oracol: repartitorul pe postare și gardul strict (D9-A10, mutat aici 2026-10-06). Forma minimală din 111 (h): un singur helper al capătului de terț, cu regula „terțul capătului"; gardul citește repartitorul capătului dintr-o singură funcție; aserțiile se scriu pe cititori, nu pe coloanele postării. Înaintea codului se măsoară și se scriu în catalog: postările care capătă coordonata, cititorii care grupează pe partener sau pe gestiune, invarianții care presupun partener numai cu partidă. Cele 17 aserții ale clasei „repartitorul de pe latura notei" se rescriu aici. Nu există diferență de declarat față de oracol: oracolul a murit la 6 | schimbarea 8 |
| 7 | eliminarea atomică a declarațiilor: entitățile de registru, coloanele scoase, maparea, listele XAF vechi, cazurile gardianului, `InitialCreate`, seed-ul fără rânduri moarte, recrearea bazelor, regenerarea metadatei și a tipurilor clientului, lista nominală redusă și activarea probei numelor interzise (D9-D6, D9-D7 b, D9-D8, D9-D12) | nu |
| 7b | cheia de pereche pe postare: ordinalul dat în nucleu, materializarea, coloana în `InitialCreate`, invariantul în `Conservare` și în `INV-CUB`; cititorii neschimbați (D9-A2) | nu |
| 7c | versiunea politicii în explicație: ipoteza, decizia `ContRezolvat`, DTO-ul, formatul 2, cu lista nominală a faptelor de politică scrisă înaintea codului (D9-A8) | schimbarea 7 |
| 8 | închiderea: integrala, nucleul, `--probe-sursa`, `refuzuri.ps1` pe host viu, `PerfCub`, probele din browser (operarea și lista pe `Postare`), rularea-diagnostic Import1C pe ianuarie, docs, decizia 110 cu limitele din D9-A9 și cu rezultatul scării de volum, restanțele, review advers Codex | nu |

După pasul 5, în afara motorului vechi, a martorilor lui și a declarațiilor,
nimic nu mai referă registrele; asta se verifică atunci cu lista nominală
X-D2, care trebuie să fi rămas numai cu clasele scriitor dual, martor,
mapare, legătură și cele două liste XAF vechi.

Pasul 1 citește tot `Program.cs`; e pasul pentru care se propune delegarea
(multiagent-delivery, un agent, cu verificarea main-ului pe eșantion).

**Regula de oprire.** Felia e închisă când sunt adevărate împreună:

1. niciun nume din lista interzisă (D9-D7 b) nu apare în `nou/`, probat de
   `--probe-sursa`;
2. integrala e verde pe ambele profiluri, pe baze recreate din
   `InitialCreate`; numărul de `Check` se reconciliază cu inventarul;
3. catalogul de scenarii e verde; cifrele și rezultatele lui sunt
   neschimbate în afara celor opt schimbări declarate în D9-D1, fiecare cu
   rândurile ei rescrise nominal înaintea codului;
4. fiecare refuz al planului vechi are rând în inventar, cu echivalentul lui
   probat direct sau cu verdictul „al registrului" (D9-D4);
5. nucleul trece integral; `refuzuri.ps1` trece de două ori la rând cu zero
   FAIL, cu rândurile noi din D9-D9; cele șase cazuri ale gardului pe
   `Postare` și `Tranzactie` și proba pozitivă a motorului sunt verzi;
   `verifica:drift` e zero după regenerare; `--dump-metadata` e la zi;
   `--dump-integritate-tph` e zero;
6. scara `PerfCub` își păstrează criteriile de formă; criteriul
   `PartideCuRest` e închis sau re-amânat explicit (D9-D10); costul comenzii
   fără registre se raportează A/B pe aceeași bază, cu termenul A măsurat la
   pasul 5b;
7. `stare-curenta/` și invarianții nu mai descriu regimul dual;
8. review-ul advers Codex e închis și decizia 110 e scrisă;
9. probele dinaintea tăierii sunt arhivate și scara de volum e raportată
   owner-ului înaintea pasului 6 (D9-A1);
10. invariantul perechii e verde în nucleu și în `INV-CUB`; nicio postare
    existentă nu ajunge modificată la commit (D9-A2, D9-A5);
11. probele versiunii politicii sunt verzi (D9-A8);
12. niciun document de catalog acceptat înaintea pasului 6b nu e refuzat de
    gardul strict; postările care au căpătat partenerul sunt numite în
    catalog (D9-A10).

**Oprire înaintea termenului** (se raportează owner-ului, nu se ocolește):
un refuz al planului vechi fără echivalent exprimabil pe contract; un cititor
de producție al registrelor în afara listei nominale; o aserție de regulă
care nu se re-țintește fără altă cifră nedeclarată; o cifră de catalog
schimbată în afara listei din D9-D1; un pas care nu poate avea integrala verde în
ordinea scrisă; lista pe `Postare` care nu funcționează pe hostul real.

## D9-Q — tranșările owner-ului (2026-10-05)

- **D9-Q1 — împărțirea = A.** TR-D9a tăierea, apoi TR-D9b unitățile, cu
  contracte separate. Respinsă: o singură felie TR-D9.
- **D9-Q2 — evidența XAF = A.** O listă read-only pe `Postare`. Respinse:
  nicio listă în XAF; liste separate pe domenii.
- **D9-Q3 — subiectul permisiunii = A.** Citirea pe `Postare`, unic.
  Respinsă: subiecte pe domeniu.
- **D9-Q4 — hook-urile care nu citesc registre = A.** Rămân în afara feliei:
  hook-urile de stingere trec la TR-D9b, 104-r2 trece la `după PoC`.
  Respinse: stingerea în TR-D9a; amândouă în TR-D9a.
- **D9-Q5 — prețul istoric al lotului = A** (după D9-RV2). `Lot.PretUnitar`
  rămâne, ca preț de intrare. Respinsă: coloana dispare și prețul se citește
  din faptul de origine al lotului, în cub.

## Amendamente după review-ul Codex (2026-10-05)

Review la `63801a8`:
`comunicari/2026-10-05-1032-codex-claude-tr-d9a-contract-review.md`. Toate
cele șase observații sunt verificate pe sursă și acceptate.

| Obs. | Ce era greșit | Corecția |
|---|---|---|
| D9-RV1 (P1) | pașii 4–7 nu puteau fi verzi fiecare: simbolurile dispăreau înaintea consumatorilor lor, iar interdicția textuală ar fi picat | D9-D15 rescris: consumatorii se portează sub dual (pașii 4–5), motorul încetează să scrie (6), declarațiile dispar atomic (7), cu proba numelor interzise activată acolo; D9-D7 (b) delimitează ce se interzice |
| D9-RV2 (P1) | „`ValoareDeclarata` din linie" nu păstra singură cifra: valoarea liniei de retur se calculează din `Lot.PretUnitar`, pe care contractul îl scotea | D9-D3 rescris pe două surse; `Lot.PretUnitar` rămâne preț de intrare (D9-Q5); probe pe valorile liniilor: SC-RDC-07, SC-RLF-05/13 |
| D9-RV3 (P2) | 75-r4 se închidea prin ștergerea coloanei de pe `Lot`, deși privește `PretEvaluare` al ASM | D9-D13: rămâne deschisă, spre TR-D9b |
| D9-RV4 (P2) | portarea gardului pierdea excepția de angajament și nu avea domeniu | D9-D4: regula întreagă, angajamentul de pe linia cauzală, domeniul numit, proba directă a gardului nou |
| D9-RV5 (P2) | dry-run-ul promitea valori fără contract de ieșire | D9-D2 și D9-D3: întoarce numai refuzuri; nu persistă, dar modifică OS-ul temporar; nu ia blocajul scrierii |
| D9-RV6 (P2) | ruta HTTP absentă nu proba gardianul pe tipurile noi | D9-D9: gardul probat pe ObjectSpace securizat, proba pozitivă pe cel de sistem, HTTP numai pentru neexpunere, matricea fără total fix, lista în browser pe hostul real |

Din constatările fără număr: faptul despre `TotalStingere` e nuanțat și
scoaterea proiecției moarte e separată de semantica proiecției (D9-D10);
098-r3 cere proba operației acceptate (D9-D13); refuzul tipului inert intră
în tabelul pașilor și în schimbările declarate (D9-D5, D9-D1).

Reverificarea la `1fe9cb7`:
`comunicari/2026-10-05-1108-codex-claude-tr-d9a-reverificare.md`. Trei
observații noi, verificate pe sursă și acceptate.

| Obs. | Ce era greșit | Corecția |
|---|---|---|
| D9-RV7 (P1) | „absorbția dispare, Δ-ul local rămâne, catalogul nu se schimbă" nu puteau fi adevărate împreună: SC-ASM-17 trece azi numai prin absorbție | D9-D3 „ASM după tăiere": gardul ΣP = ΣC, clasificarea pe C, fără Δ; schimbare de comportament declarată (D9-D1, 2); SC-ASM-17…26 rescrise nominal |
| D9-RV8 (P1) | `DistribuieValoarea` își ia ținta din valorile lăsate de dry-run pe linii; după tăiere ar fi prezis estimarea și operatorul n-ar fi putut ieși din refuz | distribuirea citește evaluarea consumurilor de la declarant, disponibilă înaintea gardului de balansare (ASM-B6, ultimul paragraf); apelanții lui `Valideaza` care citesc valori intră în inventar (D9-D2) |
| D9-RV9 (P2) | invariantul compara o magnitudine cu o sumă semnată: ieșirea prin Transfer e pe Debit, negativă | D9-D3 (a): postarea de ieșire aleasă, normalizată pe latură, două egalități semnate, fără `Abs`; probe pe BCS, BTR și ASM cu Transfer |

Închiderea review-ului la `75138d7`
(`comunicari/2026-10-05-1130-codex-claude-tr-d9a-contract-review-inchis.md`)
a confirmat tabelul SC-ASM și a adus precizări pentru rândurile 20, 22, 23 și
24, transcrise în tabel. Închiderea nu certifică implementarea.

Din răspunsurile la întrebări: pasul 6 aduce la zi lista nominală și probele
vechilor scriitori (D9-D15); gardul analizei nu se întinde la ASM și BTR,
care azi n-au note — restanța D9-r1 (D9-D4); citirea completă pe `Postare` e
consemnată ca schimbare mai strictă, iar `TipuriInsumate` își păstrează
drepturile și primește `Postare` pe lângă ele (D9-D9).

## Amendamentul 1 (2026-10-05)

Hotărât de owner punct cu punct, după consultarea cub vs registre
(`docs/consultations/2026-10-05-cub-vs-registre/`) și după inventarul pasului
1. Textul întreg, cu motivele și probele fiecărui punct:
[`tr-d9-taierea-amendament-1.md`](tr-d9-taierea-amendament-1.md). Unde cele
două texte diferă, câștigă amendamentul.

| Id | Ce aduce | Unde e aplicat |
|---|---|---|
| D9-A1 | probele dinaintea tăierii: reconcilierea arhivată, `PerfCub` dual, scara de volum ca raport. Precizat de owner la 2026-10-06: baza reconcilierii e `.Flax.R3f` | D9-D15, pasul 5b; regula de oprire 6 și 9; făcut: [`tr-d9-pas5b-probe.md`](tr-d9-pas5b-probe.md) |
| D9-A2 | cheia de pereche pe postare; transformările și deschiderile rămân fără pereche; cititorii neschimbați | D9-D15, pasul 7b; regula de oprire 10; D9-r4 |
| D9-A3 | I2: recepția facturii în gardul analizei obligatorii | D9-D1, schimbarea 5; D9-D4; pasul 2 |
| D9-A4 | I5: regula de contare fără consumator, refuzată la editare prin declarant | D9-D1, schimbarea 6; pașii 2 și 6; D9-r2 nu se deschide |
| D9-A5 | inversa fiscală se naște finală; `ReatribuieInversaFiscala` dispare | pasul 6; regula de oprire 10 |
| D9-A6 | precizările I1, I3, I4, I6, I7, I8 din inventar | D9-D2, D9-D3, D9-D4, D9-D5, D9-D6, D9-D10, cum le formulează amendamentul; pasul 4 |
| D9-A7 | poziția fără unitate rămâne cum e azi; limită consemnată | decizia 110; NG-r4 |
| D9-A8 | explicația reține regula care a decis și contorul ei; formatul 2 | D9-D1, schimbarea 7; pasul 7c; regula de oprire 11; D9-r3 |
| D9-A9 | limitele consemnate în decizia 110 | pasul 8 |

Registrele derivate (opțiunea B a consultării) nu se construiesc; se pot
deriva din cub ulterior. Explorarea nodului de transformare cu valoare, a
regimului declarat pentru poziția fără unitate și a semnului pe postare s-a
închis fără schimbare de model
(`docs/consultations/2026-10-05-nod-si-galeata/`).

## Amendamentul 2 (2026-10-06)

Hotărât de owner după constatările G1 și G2 ale pasului 2
([`tr-d9-pas2-probe.md`](tr-d9-pas2-probe.md), §1). Textul întreg:
[`tr-d9-taierea-amendament-2.md`](tr-d9-taierea-amendament-2.md). Unde cele
două texte diferă, câștigă amendamentul.

| Id | Ce aduce | Unde e aplicat |
|---|---|---|
| D9-A10 | G1: partenerul pe piciorul de terț al conturilor cu flag-ul `Repartitor`; gardul strict pe postare | D9-D1, schimbarea 8; D9-D4; D9-D15, pasul 6b (mutat din 2c la 2026-10-06, forma minimală din 111 h); regula de oprire 12; D9-D13 |
| D9-A11 | G2: materialul din regulă nu ajunge pe postare; limită acceptată | decizia 110 |

Respinsă: partenerul pe toate postările documentului cu terț. Partenerul și
pe conturile de cheltuieli și venituri e candidat pentru TR-D9b.

## Ce NU intră (amânări cu nume)

TR-D9b (N-r5, T-r2, 86-r1, B-r6, 107-r4, 51e, 097-r2 și hook-urile de
stingere cu 76-r1…r3, 86-r11, T-r5); 104-r2; TR-r7; X-r1; IM-r2; IM-r5; 091-r4 și rularea integrală 12/12;
104-r3, 104-r4; restanțele `după PoC`; NG-r2 și NG-r4, candidate pentru
TR-D9b; NG-r1, de probat separat; NG-r3, odată cu BPR.
