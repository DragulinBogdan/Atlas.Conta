# Decizia 53 — GATE XAF

- **Data**: 2026-07-31 (primul commit în jurnal)
- **Stare**: activă; (d) CORECTATĂ 2026-08-24 — layout-ul e `.Layout(...)` în ContaUiBaseline (vezi corecția din text); (e) AuditTrail reactivat la DIM-3
- **Rezumat durabil**: `CLAUDE.md` §53
- **Docs**: docs/gate-xaf-contract.md, docs/api/lista-react.md

---

**GATE XAF — executat; gate TRECUT** (contractul feliei:
`docs/gate-xaf-contract.md`, 15 decizii pin-uite D1–D15). Criteriul 44.2
(„un contabil tolerant le operează zilnic", explicit NU product-grade) e
îndeplinit pe FCT: culegere completă → Save → OPERARE, validat în browser pe
**clona bazei de import** (187k documente, 129k parteneri, 312k produse;
originalul, harness-ul de reconciliere, neatins) cu registrele corecte
(628=401 1.000, 4426=401 210). Tranșările:
(a) **Golul de flux al FCT era de MODEL, nu de finisaj**: `CreeazaLot`
(25c/26e) nu avea NICIUN apelant din UI, iar `FacturaIntrareDetaliu` n-avea
`ProdusId` — validarea „liniile de stoc își creează lotul la culegere
(alegeți produsul)" era neîndeplinibilă, iar ocolirea (Lot ales din
Nomenclatoare) rupea TVA-ul de cost (două prețuri independente). Fix:
`ProdusId` pe DERIVATĂ (testul bazei ține — baza nu-l poartă) + seam de
culegere în `Committing` (`FacturaIntrareLoturiController`): naște,
sincronizează (produs/gestiune) și curăță lotul propriu; `Lot` devine
read-only pe FCT (bypass-ul divergent se închide), coerența Tip↔Produs =
oglinda 38c.
(b) **`AsignaNumar` mutat în faza de materializare** — alinierea cu propriul
principiu 33d: asignat înaintea gardienilor, un refuz lăsa numărul consumat
și `UrmatorulNumar` incrementat în OS-ul viu (UI-ul rulează motorul în OS-ul
View-ului), iar un Save ulterior le persista = gol în seria fiscală. Check
ModelCheck dedicat, probat prin sabotaj. Idem `AplicaScadenta` (review D8).
(c) **Calculul la culegere refolosește ACELAȘI helper**
(`TvaService.CalculeazaLaCulegere`): operatorul culegea financiar orb —
`Valoare`/`ValoareTva`/`Total` rămâneau 0 până la operare. Semantica diferă
într-un punct, deliberat: la culegere baza s-a schimbat ⇒ TVA-ul se
recalculează; regula 36a (TVA-ul cules bate rotunjirea) rămâne a OPERĂRII.
`Valoare` devine read-only în UI (e rezultat — `PregatesteOperare` o
rescrie); `ValoareTva` rămâne editabilă.
(d) **Layout-ul DetailView NU se poate face cu EntityFluent**: `.Section()`/
`.Group()` doar ADAUGĂ item-uri lipsă, iar la momentul customizer-ului
layout-ul auto-generat conține deja toți membrii ⇒ grupurile ar ieși goale.
Mecanismul corect e cel NATIV: `[DetailViewLayout(grup, index)]` pe
proprietăți + updater propriu DOAR pentru etichetele grupurilor
(`LayoutDocumenteUpdater`). Captions RO pe proprietăți
(`[XafDisplayName]`) ⇒ identice în ListView și DetailView.
**[CORECȚIE 2026-08-24]** Mecanismul de mai sus a fost ÎNLOCUIT pe
01.08.2026 (Atlas.DXF 26.1.3.9): layout-ul documentelor e declarativ, în
`ContaUiBaseline` cu `.Layout(...)` (API autoritar aplicat de
`UiLayoutUpdater`); `[DetailViewLayout]` + `LayoutDocumenteUpdater` au
murit. Compunerea e bază-întâi (grupurile derivatei nested în containerul
`Antet` al bazei); `.Section()/.Group()/.Tabs()` rămân aditive; membrii
nedeclarați ajung în `Unplaced`. Forma curentă e în CLAUDE.md 53d.
(e) **AuditTrail EF Core e incompatibil cu owned types** — descoperit la
smoke: `AuditTrailService.GetKeyAsObject` citește PK-ul prin reflecție CLR,
owned au PK SHADOW ⇒ NRE la ORICE SaveChanges care atinge o linie de
document (apelul precede filtrarea pe tip ⇒ neconfigurabil). Dezactivat
modulul ȘI hook-ul (`WithAuditedDbContext` → context simplu); tabelele rămân
în schemă. Istoricul contabil nu depinde de el (registre append-only,
corecția = anulare/storno — decizia 14).
(f) **Review advers: 8 defecte, toate fixate**, cel critic fiind ștergerea
silențioasă de loturi ISTORICE: `ProdusId` e coloană nouă, deci pe cele
34.289 de linii importate e null deși lotul e finalizat — ramura „produsul
a fost golit" le ștergea (ID, dată reală, preț, poziție FIFO) la orice
commit, inclusiv cel pe care `Opereaza` îl face necondiționat. Fix:
distincția lot FINALIZAT (trecut prin motor) vs născut la culegere +
self-healing. Restul: cache de controller care traversa view-urile (blocaj
pe FCT / serie ocolită pe FCL — instanțele de ViewController se REFOLOSESC),
`Total` fără refresh, `Eticheta` nemapată în full-text search (excepție EF),
curățenia no-op pe ListView (EF nu marchează dependenții la cascadă DB),
N+1 pe eticheta de lot în registrul de stoc.
(g) **Lista React deschisă** (`docs/api/lista-react.md`, artefactul cerut de
44.2): itemii structurali (ObjectSpace sincron, dialoguri, feedback),
restanțele moștenite (multi-tab staleness, SmartLookup revertat,
localizarea shell-ului) + cei găsiți în felie (sumar de grilă — model
WinForms-only; `Total` în liste = N+1; `Total` pe DetailView fără
notificare; AuditTrail × owned).
(h) **Corecții de stare** față de deciziile anterioare, verificate în cod:
**SmartLookup e REVERTAT** la lookup standard din commit `98ce1d0` (decizia
40d descria starea de dinaintea revert-ului); **FK-urile brute pe
NIR/BTR/BCS/PLT/INC erau deja rezolvate** de
`ForHierarchy<Document>().HideForeignKeys()` (nota 40e inexactă); Atlas.DXF
e **pinnat 26.1.3.7**, nu flotant (contra 41e).
(i) Rămase, ne-blocante: culegerea de produs pe NIR manual / LDI+ / ASM
(mecanismul D2 e extensibil, wiring-ul n-a intrat în felie); localizarea
shell-ului; perioadele fiscale ≠ 2026 se adaugă manual; `Data` pe
documentele conexe rămâne a sursei.
