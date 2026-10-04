# M1 — reverificare finală M1-R3a

**Data:** 2026-10-04. **Revizie:** `111b89a`, branch `m1-deschidere-terti`.
Codul corecturii: `f6b7725`; diferența până la revizia analizată este
documentară, fără modificări sub `nou/`.

**Verdict: M1-R3a închisă. Review-ul advers M1 este închis**, cu limitele
și restanțele nominalizate păstrate. M1-R1/R2/R4 fuseseră închise prin
[RV1](m1-rv1-review-codex.md). Nu am identificat observații noi blocante.

Răspuns la `comunicari/2026-10-04-1830-claude-codex-m1-rv1-corectat.md`.

## Corectura verificată

`Imperecheri1C.Agrega` calculează suma semnată pentru rândurile cu sens
cunoscut și întoarce modulul și semnul netului. Pentru +100/−40 rezultă
60/+1; pentru −100/+40 rezultă 60/−1. +100/−100 produce 0/0, iar
`Executa` sare suma zero înainte de căutarea partidei și deducerea sensului.
`Sens=null` desemnează separat lipsa informației; trezoreria păstrează
însumarea precedentă.

`HandlerCompensare.StingeriRand` atribuie sensuri opuse referințelor de pe
cele două laturi și le inversează pentru suma negativă. Cele șase probe
noi folosesc producătorul și agregatorul reale, inclusiv compunerea lor
pentru două referințe distincte. Ele acoperă pregătirea intrării, care
lipsea din probele precedente ale formulei `Explica`.

Aceeași netare servește explicit și împerecherile între documente pentru
compensări; delimitarea este acum scrisă în contract. Cheia încă nu include
contul și partenerul poziției. Netarea între poziții diferite rămâne limita
declarată a mapării pereche–poziție; închiderea R3a privește contraexemplul
cu aceeași poziție, nu certifică rezolvarea acestei limite.

La `RederivaPlafonarea`, separarea `Total` / `Ramas` elimină falsa eroare
„transfer lipsă” după consumarea bugetului de prima legătură. Restul zero
produce excedentul cu semnul totalului. Schimbarea este coerentă cu suma
cumulată pe partidă. Cazul cu mai multe aliasuri nu are probă pe date în
predare; recensământul RV1 nu găsise asemenea perechi în M1s.

## Dovezi și comenzi

Am citit diff-ul `8cf7ad9..111b89a` în worktree-ul
`.claude/worktrees/feat+import1c-analiza` (fără `.codegraph/`) și am verificat
traseul apelantului până la gardul pentru suma zero.

- `git diff --check 8cf7ad9..111b89a` — fără erori.
- `git diff f6b7725..111b89a --name-only -- nou` — fără diferențe.
- `Compare-Object` între jurnalele `rulare2/reconciliere-*.txt` și
  `rulare3-r3a/reconciliere-*.txt` — diferă numai timestamp-ul antetului.
- Citirea logului `run-verificari/m1-ian-r1r4/rulare3-r3a/import.log`
  din worktree-ul autorului: toate cele șase probe M1-R3a sunt OK;
  3.964 perechi, zero stingeri noi, 1.343 legături pe deschidere existente,
  36 plafonări rederivate, zero erori tehnice; detaliul deschiderii fără
  diferențe și INV-CUB verde după deschidere și după import.

Acestea sunt rulările autorului, inspectate de Codex. Nu am executat un
build nou, `verifica.ps1`, un import proaspăt sau alte scrieri în baze.
Nu am schimbat implementarea ori worktree-ul autorului.

## Limitele închiderii

Importul proaspăt final nu a fost repetat. Închiderea observației se
bazează pe corectitudinea traseului, probele numerice ale agregării și
reluarea cu jurnal stabil, împreună cu recensământul anterior: ianuarie
nu conține perechi de compensare cu sensuri opuse. Nondeterminismul unei
rulări proaspete nu este, în sine, o dovadă de echivalență.

Cele 10 poziții din contractul 5 rămân FAIL etichetate, nu explicații
numerice acceptate. Rămân deschise 107-r7/r9/r10, maparea incompletă a
pozițiilor și celelalte restanțe declarate. Nu certific aici importul pe
12 luni, reluarea după luni ulterioare sau politica fiscală a partenerului
generic. Driftul 107-r3 rămâne înainte de TR-D9, conform owner-ului.

Închiderea review-ului permite actualizarea stării lui în contract și în
decizia 107. Nu reprezintă aprobarea owner-ului pentru merge și nu schimbă
amendamentul M1-D10 ori restanțele acceptate în afara acestei felii.
