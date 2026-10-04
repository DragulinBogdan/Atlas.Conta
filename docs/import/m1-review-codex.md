# M1 — review advers Codex

**Stare curentă:** [reverificarea finală la 111b89a](m1-rv2-review-codex.md)
închide M1-R1–R4, inclusiv M1-R3a, cu limitele documentate.
Mai jos este review-ul inițial.

**Data:** 2026-10-04. **Verdict:** modificări necesare; M1-D10 nu este încă
reverificat ca îndeplinit. Patru observații: două P1 și două P2.

Review cerut prin `comunicari/2026-10-04-1605-claude-codex-m1-review.md`.
Ținta este `m1-deschidere-terti`, **8d1aee6**, peste main **ac372fc**,
citită în `.claude/worktrees/feat+import1c-analiza/`. Referințele de cod și
numerele de linie de mai jos sunt din acea revizie, nu din main.
Contract: `docs/import/m1-deschidere-terti-contract.md`; decizie: 107.
Nu am modificat implementarea, ramura autorului sau bazele.

## Observații

### M1-R1 — P1: reluarea nu verifică detaliul deschiderii contra sursei

**Loc:** `nou/tools/Import1C/DeschidereCub.cs:195–234`, în special 217–234.

Ramura cu deschidere existentă citește numai contul, latura, valoarea și
felul unității. Compară sumele brute per cont/latură și numărul de partide
și loturi. Nu compară valoarea fiecărei partide cu intrarea curentă, nici
mulțimea completă a identităților din sursă cu cea deja materializată.

Contraexemplu: două partide creditoare pe același cont, cu aceleași chei,
au în cub 60 și 40, iar sursa reluată are 61 și 39. Numărul rămâne 2,
controlul Credit rămâne 100, identificatorii legăturilor nu se schimbă.
Toate comparațiile noi ale deschiderii acceptă intrarea. Registrul bloc
rămâne identic, deci `CITIRE_DESCHIDERE_INCOMPLETA` nu poate identifica
redistribuirea. INV-CUB verifică baza, nu noul detaliu al sursei.

Mai mult, dacă sursa înlocuiește o referință cu alta păstrând totalul și
numărul, bucla legăturilor creează ID-ul noii partide fără a verifica
existența sa în tranzacția de deschidere; legătura veche rămâne.
Continuarea poate căuta o partidă care nu a fost materializată.

**Necesar:** verificare bidirecțională a identităților și măsurilor
deschiderii, înainte de a declara reluarea validă sau a completa legături.
Probe: redistribuire 60/40 → 61/39, referință înlocuită cu număr constant,
reluare identică. Aceasta întărește verificarea cerută de M1-D2/D9;
nu cere rescrierea cubului existent.

### M1-R2 — P2: explicațiile plafonărilor dispar la reluarea procesului

**Loc:** `Imperecheri.cs:58–59,136–138,288–302`;
`ReconciliereLuna.cs:294–307`.

`PlafonatPePartida` există numai în memorie. După succes, legătura
`1C:StingereDeschidere` persistă numai partida țintă. La o nouă rulare,
ramura `ContainsKey` sare perechea înainte de a reconstitui excedentul.
Contractul 5 citește astfel zero pentru o plafonare deja aplicată.

Exemplu real din raportul rebazat:
`401|9414D067E5E9285611E626F22A8845EC|00000038/BEDA00155D10D30411EFB84F2EC39EE0`.
Deschidere Credit **2.463,86**, cub final **0**, sursă **+0,89**;
raportul o acceptă prin plafonarea **0,89**. Cu legătura existentă și
dicționarul gol, Δ **−0,89** nu mai are explicația din prima rulare.
Există **30** poziții acceptate prin plafonare în raportul predat.

Acest defect afectează reconcilierea idempotentă, chiar dacă stingerea
însăși nu se dublează. `--continua` înseamnă continuarea peste luni picate;
nu persistă memoria între două procese.

**Necesar:** explicații persistate sau rederivate determinist din fapte,
cu luna și sensul lor. Probă în două procese, aceeași sursă și bază:
plafonare urmată de reluare, raport de reconciliere identic. Nu am executat
un al doilea import complet pe baza autorului; constatarea rezultă din
traseul de reluare și exemplul numeric existent.

### M1-R3 — P2: contractul 5 pierde sensul refuzurilor și nu însumează explicațiile

**Loc:** `Imperecheri.cs:285,315`; `ReconciliereLuna.cs:294–307`.

Refuzurile sunt acumulate ca sume pozitive și primesc ulterior semnul
soldului inițial. Acesta nu este neapărat sensul mișcării refuzate:
M1-D7 admite explicit și cazul cu semn inversat.

Caz real:
`4111|8FEF0040F4D1BD8611DC37663B36B968|00000075/BEDA00155D10D30411EF73E95BE16352`.
BalantaNivel3 confirmă sold inițial **−201,67**, rulaj Credit **246,00**,
sold februarie **−447,67**. Nota sursă din 8 ianuarie este
`512.1 = 411.1`, **246,00**. Cubul păstrează **−201,67**, iar pass2
înregistrează refuz **246,00**. Diferența este exact **+246,00**;
formula încearcă **−246,00** și declară FAIL. Mișcarea mărește soldul
creditor; refuzul ei nu poate fi explicat folosind semnul unei stingeri
normale. Nu este nevoie de schimbarea motorului pentru a raporta corect
această divergență de sens.

Separat, ramurile verifică `sarit` și `plafonat` fiecare singur, nu suma
lor. Contraexemplu numeric: deschidere debitoare **100**, cerere **110**
plafonată cu **10**, apoi încă **20** refuzat deoarece partida este stinsă.
Cub **0**, sursă **−30**, Δ **30**. Nici comparația cu 20, nici cea cu 10
nu explică rezultatul, deși explicațiile cumulate îl acoperă exact.

**Necesar:** delte semnate după mișcare și cumulate pe partidă/perioadă;
probe pentru ambele sensuri și pentru refuz + plafonare pe aceeași partidă.
Corectura semnului plafonării din 0276920 nu acoperă aceste două cazuri.

### M1-R4 — P1: o eroare tehnică poate deveni diferență explicată și contract verde

**Loc:** `Imperecheri.cs:304–325`, în special înscrierea necondiționată în
`SaritPePartida` la 315; consumator: `ReconciliereLuna.cs:298–301`.

`catch (Exception)` tratează inclusiv erorile de tranzacție/persistență
și `SCRIERE_OCUPATA` ca sumă refuzată pe partidă. Eticheta „alt motiv”
apare în avertisment, dar reconcilierea nu o consultă și acceptă suma
drept explicație de business. Nu este incrementat eșecul de import al
documentului.

Contraexemplu de traseu: partidă inițială creditoare **100**, plată
operată corect **100**, sursă finală **0**. Un timeout al blocajului din
`TranzactieComanda.Incepe` împiedică transferul. Catch-ul lasă cubul la
**−100** și adaugă `sarit=100`; contractul 5 verifică
`−100 − (−1 × 100) = 0` și îl declară explicat. INV-CUB și contractele
contabile nu detectează lipsa unui transfer care nu a fost scris.

**Necesar:** numai refuzurile de business nominalizate pot explica
reconcilierea. Erorile tehnice și motivele necunoscute trebuie să producă
eșec distinct, fără a alimenta explicația acceptată. Probă de eroare la
începutul tranzacției și de eroare la persistare, cu absența efectelor și
verdict nereușit. Acesta este un contraexemplu dedus din cod; nu am injectat
timeout-uri în baza predată.

## Triajul celor 11 poziții din rularea rebazată

Am citit baza `Atlas.Conta.Import1C.Flax.M1r` în tranzacții read-only și,
țintit, `EServicesFlx`. Postările celor nouă poziții de mai jos au
`Tranzactie.Fel=Operare`, nu `Transfer` din noul `CreeazaPeDeschidere`.
Originea este demonstrată; atribuirea integrală unui defect de motor sau
ordinii FIFO ar cere probe independente suplimentare și nu o certific aici.

| Poziție — sfârșitul referinței / partener pentru fără document | Δ cub − sursă | Originea mișcărilor în cub |
|---|---:|---|
| 401 / `...C100331B8AFE` | +2.615,73 | Compensare `...DCBB8077595A`, operare +4.288,55; refuz pass2 1.672,82 |
| 4111 / `...B3A1D1437938` | +745,42 | Compensare `...DCBB80775967`, operare −4,02; refuz pass2 749,44 |
| 4111 / `...B372A5C06D49` | −520,00 | Compensare `...D3F4A4D6C8E5`, operare −520,00 |
| 401 fără document / `FF91000C294F8EE911DDC8F39C131B62` | −483,18 | Operatia `...DC85202B0CE1`, operare −286,81; sursa finală +483,18 |
| 4111 / `...E005B187FE49` | −405,60 | Compensare `...D3F4A4D6C8E5`, operare −405,60 |
| 4111 / `...20963E13D89C` | −399,00 | Compensare `...D3F4A4D6C8E5`, operare −399,00 |
| 401 / `...AD4E82D1143A` | −60,00 | Operatia `...E79DA08C5235`, operare −50 și −10 |
| 4111 fără document / `9417D067E5E9285611E817AD27AD8805` | −0,16 | Operatia `...DB0CE695F2D6`, operare +698,84 pe sold inițial −699 |
| 4111 / `...9891CB795D4E` | +0,16 | Aceeași Operatia, operare +0,16; sursa păstrează −0,16 pe această referință |

Celelalte două:

- **+246,00**, referință `...73E95BE16352`: fără mișcare în cub după
  deschidere; divergența de sens este explicată la M1-R3.
- **+5.117,60**, referință `...D7680B187BD6`: fără mișcare în cub după
  deschidere. Sursa are −117,60 pe Debit din compensarea
  `...D3F4A4D6C8E5` și +5.000 pe Credit din
  `RaportDeVanzariCuAmanunt/BEDA00155D10D30411EFD4E970DFE1D9`, linia 12.
  În Atlas, copilul `#inc12` există și postează 5.000 pe o partidă proprie,
  fără legătură `StingereDeschidere`. `Imperecheri1C.Executa` enumeră
  Extras, Plata, Incasare, Card și Compensare; nu enumeră încasările inline
  ale retailului. Este o limită de acoperire a fluxului inline, de numit în
  restanța aferentă; nu dovedește defectul noului transfer. Refuzul de
  117,60 nu explică singur întreaga diferență.

Prin urmare, cele 11 nu trebuie transferate în bloc la „ordine” sau „motor”.
Nici scăderea 42 → 11 nu certifică de una singură formula reconcilierii.

## Celelalte puncte cerute și regula de oprire

- **Ordinea și brutețea deschiderii:** loturile sunt pregătite înainte de
  cub, iar blocurile se scriu după el; controalele terților sunt pe laturi.
  Nu am identificat un defect nou în această ordine pe rularea proaspătă.
  Probe ale autorului: 3.967 partide, 6.817 loturi, 10.828 postări,
  INV-CUB după deschidere și după import verde. Limita reluării este R1.
- **Atomicitatea stingerii + legăturii:** `Materializare.Imperecheaza`,
  `Legaturi.Leaga`, `os.CommitChanges` și `tx.Commit` sunt în aceeași
  tranzacție explicită. `CommitChanges` nu încheie tranzacția exterioară.
  Nu văd fereastră de commit parțial între cele două persistări. Nu am
  simulat oprirea procesului în acea fereastră.
- **Blocajul global:** apelurile de materializare folosesc
  `TranzactieComanda.Incepe`; `CereScriere` asigură din nou blocajul.
  Citirea preliminară a restului este în afară, dar motorul reverifică
  disponibilul sub blocaj. Nu am găsit o cale nouă care scrie cubul fără
  blocaj. Tratarea timeout-ului rămâne R4.
- **Generic / fiscal:** nu extind review-ul la 107-r5. Există însă o
  contradicție documentară de corectat: M1-D5 promite `NuIncludeInDec394`,
  dar `AsiguraPartenerGeneric` scrie doar cod și denumire; nomenclatorul
  importatorului spune explicit că modelul nu are acel câmp. Decizia
  107-r5 lasă excluderea ca decizie de produs. Contractul trebuie să
  distingă ținta de comportamentul implementat; nu este probată aici o
  includere fiscală nouă și nu cer implementarea unei politici amânate.
- **Comentariile scurtate:** nu am găsit o pierdere independentă de
  contract la reducerea lor la referințe de decizie. Lipsesc probele de
  reluare/refuz pentru cazurile R1–R4, nu comentarii narative mai lungi.

Amendamentul owner-ului permite amânarea celor 12 luni și nu îl redeschid.
Este rezonabil ca driftul nominalizat al contractelor 1/2 să fie separat,
cu 107-r3 înainte de TR-D9, și ca limitele inline/FIFO rămase să fie
raportate numeric. Totuși, „verde pe mecanism” nu este demonstrat cât timp
verificarea poate explica erori tehnice și își schimbă explicațiile la
reluare. Propun păstrarea stării **implementat, review deschis**, corectarea
R1–R4, apoi proba de ianuarie + reluare. Nu transform interpretarea autorului
a M1-D10 într-o aprobare a owner-ului.

## Verificări executate și limite

- CodeGraph pentru traseele existente de deschidere, materializare,
  citire și tranzacție; citirea diff-ului și a surselor din worktree-ul
  țintă (care nu are `.codegraph/`).
- `git diff --name-only main..m1-deschidere-terti` și
  `git diff --check main..m1-deschidere-terti` — fără erori de whitespace;
  codul schimbat este limitat la Import1C. Diff-ul `c855406..8d1aee6` pe
  Import1C cuprinde comentarii; probele autorului rămân relevante pentru
  comportamentul reviziei citite.
- Citirea `run-verificari/m1-ian-rebazat/import.log` și
  `reconciliere-20261004-152854.txt` din worktree-ul țintă. Acestea sunt
  rulări ale autorului, nu un import repetat de Codex.
- `python -X utf8 run-verificari/codex-m1-review/recensamant.py` — SQL
  read-only pe baza M1r; rezultat în `recensamant.txt` din același director.
- `sqlcmd -S localhost -d EServicesFlx -E -C -b -Q <SELECT țintit>` —
  recensământ read-only al celor două referințe. Interogările sunt
  păstrate în `run-verificari/codex-m1-review/flax.sql`; rezultatele în
  `flax-sens.txt` și `flax-miscari.txt`. Prima încercare în sandbox nu a
  putut folosi autentificarea Windows; interogările au reușit cu escalare.

Nu am rulat `verifica.ps1`, un build nou, un import complet sau fault
injection: nu am schimbat codul, motorul ori scenariile. Contraexemplele
R1/R4 și cel combinat din R3 sunt analiză de traseu, nu probe executate prin
ModelCheck. R2 folosește un caz real și analiza ramurii de reluare. Triajul
de mai sus este verificat prin citirea faptelor persistate. Validarea
corecturilor trebuie să adauge probe executabile prin fluxul de verificare
al proiectului și să păstreze sufixe de baze proprii.
