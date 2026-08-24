# Decizia 50 — Felia 1C-d — gate-ul fazei

- **Data**: 2026-07-27 (primul commit în jurnal)
- **Stare**: activă; (e)/(f) lotul de defecte închis de 52
- **Rezumat durabil**: `CLAUDE.md` §50
- **Docs**: docs/import/faza-1c-design.md

---

**Felia 1C-d — gate-ul fazei — executată; anul 2025 integral prin motor
(~205k documente/rulare, 1h40), contractele (1) sold per cont și (2)
închiderea de TVA VERZI 12/12, contractul (3) stoc verde 11/12 cu O
SINGURĂ cheie reziduală pe an (−0,69 lei din 66.553 chei, tranzitorie
august, diagnosticată la bănuț: 1C ține cost PER DEPOZIT pe același lot
și redistribuie valoarea la transfer conservând totalul; Atlas = preț
unic per lot, decizia 13 — diferență de model, nu defect). Forcing
function-ul 44.1 e trecut: 4423/4424 la cent cu 1C pe toate cele 12
luni. Tranșările:**
(a) **Lotul de robustețe pre-1C-d (49f) — executat în 5 pași**:
(1) ordinea în lună devine CRONOLOGICĂ pe timestamp-ul sursei (ordinea
pe tipuri = doar tiebreak; nicio ordine fixă nu putea fi corectă —
BTR↔DEZ nedecidabil) + `Disponibil` = sold simplu la dată + alias
`1C:LotAlias` pentru returul pe lot absent; (2) **amendament 47d:
identitatea produsului de import = (nomenclator 1C × simbol de cont)**
— „contul dominant" moare; gemenii (17) au fiecare Tipul contului lor,
registrul lotului = registrul Tipului produsului PRIN CONSTRUCȚIE
(invariantul motorului „Tip linie = Tip produs lot" din P2/1C-a rămâne
neatins; varianta „linia urmează lotul" ar fi picat pe el — regula de
oprire a agentului a prins-o); (3) D4: legat+Draft la reluare =
ștergere + REIMPORT complet (construcție-întâi, fallback re-operare pe
chei secundare; ITV se șterge înainte de regenerare) + D3:
`--deblocheaza <view>:<cheie>` (storno prin motor în pase repetate,
eliberarea cheilor de lot, legăturile șterse la final; artefact rămas
= exit 1) + persistarea `ProduseRealocate` în commit-ul documentului
(era 0 rânduri — se scria în OS-ul de planificare aruncat);
(4) D6: contractul devine MĂSURAT — registrul divergențelor
(`1C:Divergenta`, cheie sintetică autodescriptivă) înregistrat la locul
fiecărei fapte; (5) IncasareCard/ReevaluareMF pe calea tipizată (după
regenerarea view-urilor SkyConta; `TipuriFaraColoana` rămâne fallback).
(b) **Diagnoza rulării integrale a scos 5 familii pe populații ÎNCHISE**
(17+2+3+~35 documente), fixate în G1: poarta de idempotență VACUU
adevărată pe FCL fără secțiuni de venit (documentul dispărea FĂRĂ URMĂ
— nici skip, nici punte; include cesiuni de imobilizări de 1,14M),
gestiunea cerută pe FCT fără linii de stoc, cardul negativ „declarat
acoperit" fără postare. Sistemic: **„Acoperit" cere acoperitor**
(invariant în clasificator — rând fără corespondent real = punte cu
categorie sau eșec zgomotos), skip cu motiv OBLIGATORIU (188/an, 100%
itemizate), aritmetica de închidere a unităților ca Check permanent,
jurnal de contract per rulare (toate cheile, necap-uit).
(c) **G2 — justificarea măsurată a înlocuit euristicile**: contractul
(1) per cont prin EGALITATE cu registrul (plafonul de evaluare DOAR pe
conturile de stoc + oglinzile 6xx din politici; 401 justificat cândva
cu −144k prin toleranță — acum exclusiv prin egalitate); 371↔381 și
celelalte reclasificări = divergență înregistrată la vânzare (nu
reprezentare nouă); produsele născute de ASM intră prin PROVENIENȚĂ în
poarta „cost per lot rearanjat" (delta de evaluare e PER BUCATĂ și
călătorește cu marfa — sume fixe se învechesc, demonstrat aritmetic);
categorii noi măsurate: celula sursei cu valoare fără cantitate
(deschidere + intra-an, ambele sensuri), partea neprodusă a ASM scalat,
liniile FCT cu cantitate ≤0; pragul de rotunjire per cheie =
max(0,03; ε·√mișcări) plafonat 0,25 (convenția AwayFromZero măsurată:
net −4,18/an pe 3.628 rânduri midpoint; alarmă 3σ pe Σ algebrică).
(d) **Procedural**: schimbarea de FORMĂ a view-urilor la regenerare se
absoarbe prin intersecție (`AreColoana` — contractul declarat rămâne,
se citesc coloanele prezente, diferența se strigă); documentele
STORNATE fără legătură nu pică idempotența (reziduul legitim al
deblocării); rulările lungi NU ca task de fundal al harness-ului
(timeout 10 min le seceră tăcut) — proces detașat + monitor.
(e) **Review advers dedicat — 6 defecte de fond CONFIRMATE, documentate
ca lotul pre-1C-d-final** (nefixate — fără rulări de verificare la
închidere, decizia utilizatorului): D1 replanificarea unei unități
PARȚIAL importate corupe registrul divergențelor + dependenții se
importă pe FCL eșuat (fix: pattern-ul BCS — plan per cheie, nu per
document); D2 categoria „negativul sursei" e plafon de plauzibilitate
pe CANTITATE (un BCS pierdut pe produs cu negative intra-an poate
aluneca; fix: mărginire cu Δ-ul așteptat net de supapa 48a); D3 axa de
VALOARE e nemărginită pe produsele netate/realocate/ASM și contractul
(1) e circular pe ea (minim: raportarea mărimii mulțimii marcate);
D4 alarma de rotunjire să COMPARE deriva cu cifra calculabilă a
convenției, peste 2–3× = check; D5 rândurile orfane ale registrului NU
pică zgomotos (purjare la orice replanificare + UitaSursa pe legătura
orfană); D6 --sabotaj poate da fals-negativ (bucket-ul stale față de
plafonul din politici — 711 lipsește; contractele (2)/(3) lunare nu
sunt niciodată sabotate). Ținute la review: exceptarea Stornat,
aritmetica de închidere, „Acoperit cere acoperitor".
(f) **Rămase pentru 1C-d-final**: lotul (e) + hunk-ul BTR-cost
(înregistrarea redistribuirii de cost la transfer — comis, verificat
doar prin build + diagnostic pe sursă, NU prin rulare) + G3 (promovarea
celor 27 de TipMaterial ad-hoc — 6xx/408/409/419/447/473/5328/767/758x
— în seed-ul explicit ProfilPrivat, cu ModelCheck pe ambele profiluri)
+ rularea integrală de re-validare. Gate-ul de conținut e TRECUT;
fazele următoare (GATE XAF 44.2, pasul 5) se pot planifica.
