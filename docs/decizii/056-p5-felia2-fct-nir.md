# Decizia 56 — Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API

- **Data**: 2026-08-08 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §56
- **Docs**: docs/api/p5-felia-fct-contract.md

---

**Pasul 5, felia 2 — FacturaIntrare + conex-NIR prin API — executată**
(contract + închidere: `docs/api/p5-felia-fct-contract.md`, F2-D1…D9;
flux-ancoră complet în browser pe baza Privat, review advers fără defecte
de fond, fix-urile aplicate). Tranșările care rămân adevărate:
(a) **`LoturiCulegereService`** (extracția F2-D1, mutare verbatim probată
cu diff): nașterea/sincronizarea/curățenia loturilor la culegere e serviciu
pe `IObjectSpace` pur, apelat din controllerele XAF (adaptori) ȘI din
`FacturaIntrareApply` — pe tierul API nu rulează niciun ViewController,
extracția era condiția de funcționare. Rafinări din review: Tipul mutat pe
ne-stoc rupe referința la lotul FINALIZAT (nu-l șterge); lotul finalizat
rămas FĂRĂ URME (zero registre, zero referințe vii) moare la Sterge.
(b) **Apply-ul FCT reconstituie culegerea integral**: `Numar` cules (FCT =
numărul furnizorului — diferență de contract față de BTR), TipTva implicit
doar pe linii NOI (pe existente absența = golire deliberată — clientul face
round-trip la TipTvaId), `CalculeazaLaCulegere` CONDIȚIONAT de
declanșatorii din UI (baza/TipTva schimbate — altfel override-ul de
ValoareTva murea la orice PUT), override acceptat DOAR pe regimuri cu TVA
separat și niciodată negativ; limitele semanticii (override 0 moare la
operare — 36a; renunțarea cere declanșator) documentate în DTO, fix de
fond = flag `TvaSuprascris` aditiv. Affordances oneste (`PoateAnula/Storna`
încorporează copiii operați; `Copii[]` în ReadDto = grupul conex).
(c) **NIR prin API = citire + comenzi** (scope confirmat; POST/PUT +
`ProdusId` pe NirDetaliu = felie viitoare); citirea liniilor pe BAZA
detaliului cu frunza prin as-cast (TPT LEFT JOIN) — istoricul pre-DIM-2
nu iese cu linii goale. OData extins: Partener/Produs CRUD (nomenclatoare
vii), TipTva/dimensiunile bugetare ReadOnly (politici).
(d) **Driftul openapi ÎNCHIS ca datorie** (M2 din spike): generare OFFLINE
cu `swagger tofile` pe assembly (fără host/Postgres — HostedService-urile
nu pornesc; byte-identic cu dump-ul live), `pnpm gen:openapi` +
`verifica:drift`.
(e) **Trei bug-uri de NUCLEU client** găsite la smoke, fixate în
vocabular: cheile OData sunt OBIECTE `Guid` DevExtreme (comparațiile se
normalizează cu `String()`; valoarea widget-ului rămâne nativă — `toJSON`
o serializează corect); `laSelectie` = NOTIFICARE, felia aplică derivatele
cu update FUNCȚIONAL pe propria stare; **`onValueChanged` propagă DOAR
schimbările cu `e.event`** — schimbarea programatică re-raporta prin
closure-ul vechi al contextului și ștergea câmpurile abia scrise. Regula
generală a vocabularului: formularul e sursa de adevăr, widget-ul
raportează exclusiv acțiunile omului.
(f) ModelCheck: blocul Api FCT (bugetar — inclusiv „lotul se naște la
Aplica din ProdusId", refuzurile de override) + bloc privat NOU de
semantică TVA pe N21 (retenție fără declanșatori / cedare la schimbarea
bazei / refuz pe Scutit); verzi ambele profiluri.
