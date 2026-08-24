# Decizia 62 — Pasul 5, felia 5 — NIR scriere

- **Data**: 2026-08-12 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §62
- **Docs**: docs/api/p5-felia-nir-contract.md

---

**Pasul 5, felia 5 — NIR scriere (recepția culeasă manual) — executată**
(contract + închidere: `docs/api/p5-felia-nir-contract.md`, F5-D1…D9 +
amendamentele D7b/D8b; flux-ancoră în browser pe clona bazei de import,
review advers cu 2 defecte de fond fixate). Ridică excluderea declarată la
felia 2 (F2-D3) și închide restanța de model 53i. Deblochează **recepția
fără factură** (marfă pe aviz, factura vine ulterior) — cale care nu exista
NICĂIERI: aceeași gaură de model pe care GATE-ul a închis-o pe FCT (53a),
doar că pe NIR. Tranșările:
(a) **`NirDetaliu` capătă `ProdusId`+`PretUnitar`+atributele de lot**
(migrația `NirCulegereLot`, strict aditivă), iar produsul intră printr-un
contract NOU, `ILinieCareNasteLot`, implementat de FCT și NIR.
**Interdicția e load-bearing**: `FacturaIesireDetaliu`/
`DescarcareGestiuneDetaliu` au și ele `ProdusId`, dar cu semantică OPUSĂ
(pin spre un lot EXISTENT — 37d); dacă ar declara interfața, fiecare
culegere de FCL ar naște loturi fantomă. Numele spune INTENȚIA, nu forma.
Gestiunea lotului nou = hook polimorf `Document.GestiuneLoturiCulese`
(default primitorul, adevărat pe FCT și NIR; ancoră pentru LDI+/ASM).
(b) **`LoturiCulegereService` generalizat** pe `(Document,
ILinieCareNasteLot)` — mutare de TIP, nu de semantică (cele 4 fix-uri de
review ale feliei 2 și ale GATE-ului păstrate verbatim), cu un **gard NOU**:
linia care referă un lot STRĂIN rămâne NEATINSĂ. Fără el, un PUT pe NIR-ul
conex cu produs completat ar naște un al doilea lot pentru marfă deja
recepționată, iar gardianul de sold nu-l poate prinde (lotul nou pornește
de la zero). Un singur seam pentru toate căile: controllerul XAF devine
`DocumenteLoturiCulegereController` pe `Document` (lecția 58c).
(c) **Valoarea: două cazuri, o formulă fiecare, identică la culegere și la
operare** — lot propriu = preț cules × cantitate (fără ramura asta un NIR
manual s-ar opera cu valoare 0, deci și lotul cu preț 0), lot străin =
prețul lotului (recepția PARȚIALĂ pe conexul generat e flux de producție).
`NirApply.MaterializeazaValori` rulează ABIA după `Sincronizeaza` — formula
depinde de lotul tocmai născut — și iterează doar pe frunză: liniile de tip
BAZĂ ale NIR-urilor istorice/importate nu se ating (clasa de defect GATE D1).
(d) **Contractul de scriere, inversat față de FCT pe `Numar`**: NIR ARE
PoliticaNumerotare, deci seria e server-owned (ca BTR/FCL/TRZ). NIR **nu
culege TVA** (n-are `PoliticaTva` în niciun profil — un TVA cules aici ar fi
cifră moartă, iar la sosirea facturii dublă postare); `TipTvaId` de pe clona
conexă rămâne informativ. `LotId` server-owned pe ambele cazuri.
(e) **Amendamentul F5-D7b**: linia care își NAȘTE lotul cere preț pozitiv —
altfel recepția culeasă fără preț se operează tăcut și naște un lot cu
valoare zero, propagat de FIFO în toate ieșirile ulterioare (precedentul:
plusul de inventar, 28e). **F5-D8b**: `PoateEdita` redevine funcție de stare
inclusiv pe draftul AUTOGENERAT — F2-D5 îl ținea fals fiindcă tierul n-avea
cale de scriere; din felia asta are, deci `false` devenise el însuși
minciuna.
(f) **Review advers — două defecte de fond, ambele deschise DE felie deși
niciunul nu era în `NirApply`, și niciunul atins de proba fericită**:
(1) *capcana lotului străin în XAF* — F5-D9 a făcut din ecranul de NIR o
cale VIE de culegere, dar `ContaUiBaseline` n-avea oglinda blocului FCT,
deci `Lot` rămânea EDITABIL lângă produs și preț: alegând din nomenclator
lotul unei recepții anterioare, serviciul tăcea (gardul (b), corect), iar
operarea adăuga +cantitate pe un lot DEJA în stoc, evaluat la prețul LUI —
marfă re-recepționată, invizibilă pentru gardianul de sold. **Lecția
generală: un gard care tace devine capcană exact pe calea unde există și
alternativa corectă.** (2) *ștergerea conexului* — `Sterge` refuza doar
starea, nu și `Autogenerat`: pe o factură cu linii exclusiv de stoc, factura
postează DOAR rândul de TVA (26a), deci ștergerea NIR-ului conex lăsa o
factură OPERATĂ fără datorie și fără marfă în registre. F5-D8b tranșase
EDITAREA artefactului — **anihilarea e altă decizie și nu se moștenește din
ea**. Minore fixate: produsul de pe linia cu lot străin se GOLEȘTE („inert"
devine adevărat, nu doar afirmat în comentarii — persistat, îl citea
validarea Tip↔Produs și putea face NIR-ul permanent ne-operabil);
`StergeConexeDraftAutogenerate` curăță loturile orfane ale liniilor manuale
adăugate pe conex.
(g) Rămase ne-blocante: ecranul XAF de NIR rămâne funcțional, nu
product-grade (`RecalculValoriCulegere` rămâne FCT/FCL-only, cu limitarea
documentată în cod); `AngajamentId` fără lookup în editor (ca la FCT);
`window.confirm` la ștergere, inconsecvent cu confirmarea inline din F3 —
item pentru `lista-react.md`.
