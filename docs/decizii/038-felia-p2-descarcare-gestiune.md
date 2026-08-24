# Decizia 38 — Felia P2 — DescarcareGestiune

- **Data**: 2026-07-23 (primul commit în jurnal)
- **Stare**: activă; (e) backlog-ul UI închis de 39–40
- **Rezumat durabil**: `CLAUDE.md` §38
- **Docs**: docs/privat/p2-descarcare-design.md

---

**Felia P2 — DescarcareGestiune — executată; validată e2e în ModelCheck
(privat 69 verificări, idempotent la rulări repetate; bugetar 180) +
review advers dedicat cu fix-urile aplicate. Tranșările de implementare
peste designul fixat (37):**
(a) **`AlocaFifoTolerant` = nucleul pickingului** (alocă disponibilul,
întoarce restul); `AlocaFifo` devine wrapper care aruncă la rest —
comportament identic pentru apelanții existenți. `Genereaza` și
`RestNedescarcat` sunt PUBLICE pe `DescarcareService` — puncte de intrare
ale motorului (controller azi, Web API la pasul 5); `RestNedescarcat` e
cusătura interogabilă a fluxului de comenzi (design §2.2).
(b) **Contenția intra-draft — rafinare de design**: două linii FCL pot
concura pe același lot într-o generare, iar `Sold()` nu vede alocările
necomise ⇒ generatorul procesează ÎNTÂI liniile pin-uite (identificarea
specifică bate FIFO), apoi produs+FIFO, cu mapă `dejaAlocat` per lot
scăzută din solduri. Generatorul NU aruncă niciodată la lipsă de stoc
(restul = backorder); refuzul „lot explicit fără sold în gestiune — întâi
BTR" trăiește în `FacturaIesire.ValideazaOperare`, înainte de
materializare (33d — hook-ul `GenereazaSecundar` rulează după ea).
(c) **Review-ul advers a întărit integritatea (validări, nu design):**
linie de stoc FCL / linie DSC fără regulă de contare per-Tip = refuz
explicit (30b — un Tip creat între updater-e nu mai postează
greșit/silențios prin genericul FCL, nici nu mai mișcă stoc fără notă pe
DSC); coerența Tip-linie ↔ Produs/Lot validată pe ambele tipuri;
`LinieSursa` validată ca linie a facturii-sursă a documentului, iar
acoperirea din `RestNedescarcat` filtrată pe `DocumentSursa == fcl` (un
DSC străin nu poate otrăvi backorder-ul altei facturi); liniile FCL/DSC
trebuie să fie de tipul derivat (linia de bază ar ocoli General!+
Specific?); gardian NOU generic de editare (`DocumentEditareController`):
documentele ne-Draft sunt read-only la nivel de DetailView — câmpurile
FCL sunt load-bearing și după operare.
(d) **UI**: acțiunea „Generează descărcarea" = ParametrizedAction cu dată
(default azi), doar pe FCL Operat, deleagă la serviciu în ObjectSpace
propriu și deschide draftul în editare; restul nedescărcat se raportează
în mesaj pe AMBELE căi (operare + acțiune). `DefaultTipTvaController`
generic pe `NewObjectViewController.ObjectCreated` (masterul din
`NestedFrame.ViewItem` — sub EF Core back-reference-ul liniei NU e
inițializat pre-commit, docs DevExpress 402990/112912); seed
`TipTvaImplicit` N21/CAP21 setat doar unde e null.
(e) **Smoke UI XAF (37f) — parțial, cu constatări**: aplicația pornește
curat cu modelul P2; DSC apare ca al 11-lea tip creabil; câmpul
GestiuneDescarcare + acțiunea (data precompletată azi, disabled pe Draft)
și coloanele TipTva/ValoareTva vizibile pe FCL. Constatări PRE-EXISTENTE,
rămase la faza polish XAF: butonul New al colecției Detalii creează tipul
de BAZĂ (culegerea Descriere/PretUnitar/ProdusId și smoke-ul end-to-end
al default-ului TVA blocate până la ListView-uri tipizate per derivată;
validarea (c) refuză între timp liniile generice la operare); commit-ul
master-ului la New pe linie cu laturile necompletate → FK violation brut
(lipsesc validările de culegere pe laturi). Tot la polish, din review:
traducerea FK Restrict (ștergerea unei linii referite de un DSC) în mesaj
prietenos; mecanismul generic „tipul de detaliu declarat per document".
