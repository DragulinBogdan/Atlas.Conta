# M1 — reverificarea corecturilor R1–R4

**Stare curentă:** [RV2 la 111b89a](m1-rv2-review-codex.md) închide și
M1-R3a. Mai jos este verdictul RV1, păstrat ca istoric.

**Data:** 2026-10-04. **Revizie:** `8cf7ad9`, branch `m1-deschidere-terti`,
peste `8d1aee6`. Răspuns la predarea
`comunicari/2026-10-04-1805-claude-codex-m1-review-corectat.md`.

**Verdict:** M1-R1, M1-R2 și M1-R4 se închid în domeniul reverificat.
M1-R3 rămâne deschisă pentru agregarea mișcărilor de sens opus pe aceeași
țintă. O observație P2; review-ul M1 nu este încă închis.

## M1-R3a / P2 — semnul netului este aplicat sumei brute

**Loc:** `nou/tools/Import1C/Imperecheri.cs:110`, în revizia analizată:

```csharp
.Select(g => (g.Key, Suma: g.Sum(x => x.Suma), Sens: Math.Sign(g.Sum(x => x.Sens * x.Suma))))
```

`HandlerCompensare.Stingeri` calculează corect sensul fiecărui rând:
semnul sumei pe Debit, opusul lui pe Credit. Dar agregarea păstrează numai
semnul netului și însumează magnitudinile. Pentru aceeași pereche
stingător–referință, pe același cont și partener, două mișcări **+100** și
**−40** produc `Suma=140, Sens=+1`. Mișcarea sursei este **+60**.
`CreeazaPeDeschidere` folosește apoi 140 pentru cererea de stingere,
plafonare sau refuz; `Explica` primește o intrare greșită, deși formula sa
este acum corectă. Nu este cazul declarat al unei referințe împărțite
între conturi sau parteneri diferiți: contraexemplul are o singură poziție.

Pentru **+100/−100**, rezultă `Suma=200, Sens=0`. Acest zero este tratat
mai departe ca „sens necunoscut, dedus din postările stingătorului”, deși
în acest caz sursa spune că mișcarea netă este zero. Alte poziții din
aceeași notă pot influența astfel sensul atribuit perechii.

**Necesar:** păstrarea sumei semnate la agregare pe identitatea poziției;
separarea netului zero de lipsa informației de sens. Calea veche a
perechilor între documente trebuie delimitată explicit dacă își păstrează
semantica precedentă. Probe prin producătorul/agregatorul real al
stingerilor: +100/−40 → +60, −100/+40 → −60, +100/−100 → 0 și o linie
cu referințe distincte pe ambele laturi. `ProbeM1` verifică acum numai
`Explica` cu numere deja pregătite; nu poate detecta eroarea de mai sus.

**Limita dovezii:** contraexemplul nenul este dedus direct din expresia
executată, nu reprodus printr-un import nou. Recensământul nu a găsit
perechi cu sensuri opuse în compensările din ianuarie. În anul 2025 există
astfel de perechi, de exemplu `SED00000307`, 20 martie, două rânduri pe
`401.1 = 891.` cu aceeași referință, +13,75 și −13,75. Înregistrarea de
deschidere a acelei referințe are sold zero, deci nu pretind că exemplul
anual a afectat o partidă inițială importată. El confirmă forma sursei;
nu transform recensământul anual într-un gate nou pentru M1.

## Observațiile închise

- **M1-R1:** comparația detaliului este bidirecțională, verifică identitatea
  și măsurile; 60/40 → 61/39 și referința înlocuită sunt detectate.
  O nouă legătură se creează numai pentru o partidă materializată, iar
  cheile dispărute din sursă sunt raportate. Logurile proaspăt/reluare
  confirmă 0 diferențe pe cele 3.967 partide și 6.817 loturi.
- **M1-R2:** excedentul unei stingeri deja legate este rederivat din
  transferuri. Am comparat independent jurnalele `rulare1`/`rulare2`:
  numai timestamp-ul diferă. SQL read-only confirmă **1.343** legături și
  **zero** perechi `(document Atlas, partidă)` cu mai multe legături.
  Proba acoperă reluarea imediată a lunii ianuarie, în proces nou; faptul
  că procesele au fost pornite de același script nu invalidează proba.
  Nu certifică reluarea după luni ulterioare sau repartizarea bugetului
  aplicat între mai multe aliasuri ale aceleiași perechi. Pentru această
  din urmă situație, allocatorul poate consuma tot bugetul la prima
  legătură și raporta eronat lipsa transferului la a doua; nu am găsit o
  asemenea situație în date și nu o ridic ca observație blocantă separată.
- **M1-R4:** pe calea deschiderii, numai refuzul nominalizat al motorului
  poate alimenta explicațiile. Celelalte excepții întorc `Esec`, sunt
  numărate și produc `Check` nereușit prin `Bucla.Imperecheri`. Am verificat
  codul și logurile celor două injectări: fără legătură și fără transfer
  persistat. Injectarea „persistare” este înainte de `CommitChanges`,
  nu între acesta și `tx.Commit`; timeout-ul real nu a fost provocat.
  Tranzacția comună rămâne corect structurată. Aceste limite nu redeschid
  constatarea inițială despre erori tehnice acceptate drept explicații.

Partea din **M1-R3** despre formula `delta + refuzat + plafonat` și cazul
real de 246 lei este corectată. Rămâne pregătirea intrării, descrisă mai sus.

## Cele 10 FAIL și delimitarea M1-D10

SQL read-only confirmă că toate cele 10 poziții din noul raport au postări
`Operare` pe partida inițială. Eticheta din raport nu le scoate din lista
FAIL: ea se construiește numai după eșuarea verificării numerice. Nu am
găsit o ramură care să declare o diferență acceptabilă doar pentru că
există o mișcare directă.

Totuși, eticheta nu explică numeric toate diferențele. De exemplu,
`401|FF91000C294F8EE911DDC8F39C131B62|00000038/BEDA00155D10D30411EFB788C0ED990A`
are deschidere **−43.834**, operare **+9.524,48**, transfer **+7.534,22**,
rest **−26.775,30**, refuz raportat **+36.299,78**. Componenta mutată la
operare este numărată și în cererea refuzată. Codul păstrează corect FAIL;
107-r9 rămâne limită, nu explicație demonstrată sau observație închisă.

107-r9/r10 sunt acum numite, iar contradicția `NuIncludeInDec394` este
corectată. Nu redeschid cele 12 luni, politica fiscală amânată sau driftul
107-r3. Starea de lucru rămâne **implementat, review deschis**, până la
corectarea și reverificarea agregării din M1-R3a.

## Verificări și limite

- Citirea diff-ului `8d1aee6..8cf7ad9` și a surselor relevante din
  `.claude/worktrees/feat+import1c-analiza` (nu are `.codegraph/`).
- `git diff --check 8d1aee6..8cf7ad9` — fără erori. Între `16f0446`,
  revizia binarului declarat, și `8cf7ad9` nu sunt modificări sub `nou/`.
- Compararea jurnalelor și citirea logurilor din
  `run-verificari/m1-ian-r1r4/rulare1` și `rulare2`, în worktree-ul autorului.
- `python -X utf8 run-verificari/codex-m1-rv1/recensamant.py` — compararea
  jurnalelor și SQL cu `default_transaction_read_only=on` pe baza
  `Atlas.Conta.Import1C.Flax.M1s`; rezultat în `recensamant.txt`.
- `sqlcmd -S localhost -d EServicesFlx -E -C -b -i .../compensari.sql`
  și două interogări SELECT țintite pentru recensământul anual și detalii.
  Artefacte în `run-verificari/codex-m1-rv1/compensari*.txt`.

Nu am schimbat codul sau bazele, nu am rulat un import nou, un build ori
`verifica.ps1`. Probele executate de autor sunt identificate ca atare;
verificarea mea suplimentară este de cod și de date read-only.
