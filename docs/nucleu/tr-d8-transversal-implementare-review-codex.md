# TR-D8 transversal — review advers al implementării

Data: 2026-10-04. Revizie: `2b57aa5844da1391243cd00f79c9e8e0c0a18f65`,
branch `tr-d8-transversal`. Referință: [contractul](tr-d8-transversal-contract.md),
inclusiv amendamentele din „Execuție”, și [decizia 108](../decizii/108-gate-transversal-tr-d8.md).

**Verdict: review deschis, patru observații P2.** Integrala originală este verde:
3.387 verificări bugetar / 4.597 privat, zero FAIL. Problemele de mai jos sunt
lacune ale verificărilor care certifică felia; nu am observat că fluxurile
normale ar produce automat datele corupte injectate în probe.

## X-RI1 — P2: explicația poate pierde dovada evaluării și rămâne acceptată

Loc: `Module/Cub/Citiri/Explicatii.cs:98–119`
([sursa](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Explicatii.cs)).

Invariantul normalizează `ValoareIesire` și `ValoareDeclarata` în aceeași
structură, apoi recalculează numai deciziile care au rămas `ValoareIesire`.
Astfel, mecanismul pretins de explicație decide chiar obligația de a-i
verifica dovada.

Reprodus pe ambele profiluri, în BCS-ul SC-CIT-96: cele trei decizii de
evaluare au fost înlocuite cu `ValoareDeclarata`, aceleași cantități/valori,
sursa fictivă `receptie-falsa`; cele două `SoldUnitateCitit` au fost eliminate.
`Invarianti.Verifica` acceptă. Explicația nu mai poate demonstra evaluarea
succesivă 0,33 / 50 / 0,34, deși X-D4(d)(1) și amendamentul pasului 2,
punctul 4, cer dovada evaluării din sold.

Al doilea caz măsurat: NTC stinge 10 din partida facturii; eliminarea unicului
`SoldUnitateCitit`, păstrând `AlocareFifo`, este acceptată pe ambele profiluri.
Ramura FIFO compară doar suma alocată cu postările. Cititorul permite acum
`StingereExplicata.SoldCitit = null`, deși explicația cerută include soldul citit.

De închis: verificarea mecanismului și a ipotezelor obligatorii nu trebuie
să poată fi ocolită prin redenumirea deciziei. Adăugați mutanți pentru
substituția evaluat → declarat și pentru soldul FIFO lipsă/alterat, cu
refuz stabil la `INV-CUB`. Nu propun recitirea soldului curent în locul
soldului istoric folosit de comandă.

## X-RI2 — P2: contul destinației BTR scapă și verificării compensatoare invocate

Locuri: `Module/Cub/Citiri/Loturi.cs:114–138`
([sursa](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/Citiri/Loturi.cs));
`ModelCheck/Nucleu/ReconciliereCub.cs:302–305`
([sursa](../../nou/tools/ModelCheck/Nucleu/ReconciliereCub.cs)).

Amendamentul X-D7 elimină explicit contul din cheia cantitativă și spune că
valoarea pe cont rămâne la reconcilierea (a). Însă (a) selectează `Fel = 1`
(`Operare`), excluzând transferul BTR. Auditul explicației verifică numai
capătul cu cantitate negativă; echilibrul tranzacției este global, fără cont.

Reprodus: în BTR-ul de 2 buc / 25 lei din SC-CIT-96 am schimbat **numai**
contul postării pozitive de destinație, din 302 în 628 pe privat, respectiv
302.01.00 în 628.00.00 pe bugetar. Cantitatea, valoarea, lotul, gestiunea,
registrele și explicația au rămas identice. Rezultat pe ambele profiluri:
`INV-CUB` acceptă, iar `ReconciliereCub.Ruleaza` pe document întoarce **0 Δ**.
Stocul destinației este astfel atribuit altui cont fără să cadă aceste probe.
Nota despre (f) vacuă este explicită în log; (f) privește partidele și nu
poate acoperi acest transfer de lot.

De închis: păstrați cheia cantitativă amendată, dar acoperiți separat
coordonatele valorice ale destinației transferului, cu mutantul de mai sus.
Nu cer derivarea contului istoric din nomenclatorul actual și nici includerea
transferurilor în rapoartele contabile care le exclud prin contract. Afirmația
că (a) compensează lipsa contului nu este valabilă pentru BTR.

## X-RI3 — P2: eșecul EXPLAIN devine o probă verde cu zero accesări

Locuri: `ModelCheck/PerfCub.Masuri.cs:283–286,243–248`
([sursa](../../nou/tools/ModelCheck/PerfCub.Masuri.cs));
`PerfCub.Evaluare.cs:191–200`
([sursa](../../nou/tools/ModelCheck/PerfCub.Evaluare.cs)).

`Planuri` prinde orice excepție din EXPLAIN, scrie text în artefact și
continuă. Nu propagă eșecul în `Masura.Eroare` și nu raportează câte comenzi
au rămas fără plan. Dacă toate planurile eșuează, statisticile sunt zero,
iar criteriul de independență față de istoric acceptă acele zerouri.

Probă executată: injectarea `SELECT 1 / 0 FROM "Postare" LIMIT 1` în lista
dată lui `Planuri` produce artefactul „EXPLAIN respins” și întoarce toate
statisticile zero. Aceste statistici, introduse într-o matrice sintetică
m = 0/6/12, k = 1/4, sunt acceptate de evaluatorul real `Evalueaza`, inclusiv
criteriul de acces la `Postare`. Proba s-a executat pe ambele profiluri.
**Aceasta probează tratarea erorii; nu este o nouă măsurare de performanță**
și nu afirmă că planurile predate de Claude au eșuat.

De închis: orice plan obligatoriu respins trebuie să invalideze măsurarea;
urmăriți comenzile eligibile versus planurile obținute. Un zero legitim
de accesări nu se confundă cu absența probei. Adăugați proba de eroare.
Metoda aprobată `enable_seqscan = off` și pragul de buffere nu sunt contestate.

## X-RI4 — P2: controlul numeric D300 verifică valorile altui cititor

Loc: `ModelCheck/PerfCub.Masuri.cs:131–138`
([sursa](../../nou/tools/ModelCheck/PerfCub.Masuri.cs));
`PerfCub.Evaluare.cs:45,57–61`
([sursa](../../nou/tools/ModelCheck/PerfCub.Evaluare.cs)).

Operația măsurată apelează `D300Proiectii.D300`, dar valorile comparate cu
așteptările sunt citite separat prin `TvaProiectii.DecontTva`. Din D300 se
verifică doar numărul de nemapate; `Randuri.Count` este cardinalitate
raportată, nu valoare asertată. D300 poate întoarce sume greșite sau rânduri
goale, cu nemapate zero, iar controlul numeric să rămână verde dacă DecontTva
este corect. În plus, timpul și SQL-ul atribuite D300 includ a doua citire.

Constatare statică, fără mutant executat pentru D300. De închis: comparați
bazele/taxele din **rezultatul D300** cu constantele independente ale scenei;
o modificare numai a valorilor D300 trebuie să pice controlul. Dacă se dorește
și măsurarea DecontTva, declarați-o separat. Este cerința X-D5(c), nu o
solicitare de a schimba regula fiscală.

## Verificări și artefacte

Toate comenzile au pornit din rădăcina repository-ului, prin wrapper:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip CITIRI -Profil Ambele -Sufix .CodexXReview
```

- Integrala pe surse nemodificate: `run-verificari/20261004-084147-266/`,
  manifest exit 0; 3.387 / 4.597 OK, zero FAIL.
- Prima probă cu mutanți: `run-verificari/20261004-085114-017/`.
- Proba completată cu reconcilierea BTR și eroarea EXPLAIN:
  `run-verificari/20261004-085421-092/`, exit 0. Supraviețuirea mutanților
  este scrisă explicit ca `REVIEW-ADVERS ... MUTANT SUPRAVIETUIT`;
  exit 0 al fixture-ului nu înseamnă că auditul îi refuză.
- Reproduceri locale: `run-verificari/codex-x-review/repro.patch` și
  `PerfCub.ReviewProbe.cs`. Pentru reluare pe aceeași revizie, aplicați patch-ul
  pe `ScenariiExplicatii.cs`, copiați fișierul suplimentar în `nou/tools/ModelCheck/`
  și rulați CITIRI prin wrapper pe clone proprii. Probele SQL fac rollback;
  documentele fixture-ului se curăță prin scena existentă.
- Bazele proprii: `Atlas.Conta.BackOffice.CodexXReview` și
  `Atlas.Conta.ModelCheck.Privat.CodexXReview`, clone din bazele `.ClaudeX2`.
  Nicio modificare în bazele sursă. Codul temporar al probelor a fost retras.
- După retragerea instrumentării: CITIRI, ambele profiluri, recompilat și
  rulat verde în `run-verificari/20261004-085641-715/`, exit 0; nouă scene
  per profil, zero postări reziduale. Binarul local nu mai conține probele
  temporare.

## Limite și observații care nu se redeschid

Nu am identificat o cale reală de producție care să intre în scriere cu o
tranzacție străină și să ocolească blocajul; comportamentul `Asigura(IObjectSpace)`
singur nu este raportat ca defect demonstrat. Nici un cititor suplimentar de
registru care să scape scanării nu a fost confirmat. Limita sintactică a
probei rămâne cea declarată.

Timeout-ul `SCRIERE_OCUPATA`, amânarea `PartideCuRest`/F27-r16, metoda din
plan și întrebarea X-r3 despre capătul de consum BCS sunt delimitări deja
declarate/aprobate, nu observații noi. Review-ul contractului X-RV1…X-RV7
rămâne închis; X-RI1…X-RI4 privesc implementarea și probele ei.

Am inspectat codul autorizării explicației și probele HTTP, dar nu am reluat
hostul HTTP, matricea de securitate, driftul OpenAPI sau scara completă în
container. Rezultatele lor din predare nu sunt prezentate ca rulări proprii.
Nu am schimbat codul de producție și nu am făcut commit.
