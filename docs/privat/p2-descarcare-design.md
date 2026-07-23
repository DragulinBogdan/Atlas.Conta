# P2 — Descărcarea de gestiune la FacturaIesire (design)

Stare: **FIXAT (23.07.2026) — toate cele 6 tranșări din §11 confirmate;
implementarea urmează (decizia 37 în CLAUDE.md).** Contextul: decizia 35e
(candidatul fixat: document CONEX de descărcare, simetricul FCT→NIR) + decizia
36 (TVA structural — 4427 pe FCL e deja rezolvat). Zonă sensibilă (atinge
mecanismul conex și lanțul de valori) → design înainte de schemă.

## 1. Fapte pe care stă designul (cod la zi)

- **FCL azi (30, sub P1)**: pur creanță — un rând generic `4111 = 7xx` la net
  (credit `SursaCont.TipMaterial` → contul Tipului VEN) + `4111 = 4427` per
  linie prin `PoliticaTva`. Liniile cu Natura=Stoc sunt REFUZATE prin
  `PoliticaValidare.NaturaInterzisa` (rând seed privat, marcat „până la P2").
  `FacturaIesireDetaliu`: Descriere + PretUnitar — **nu poartă produs**; baza
  poartă `LotId?` nefolosit pe FCL.
- **Conexul actual (26d)** e clonă 1:1 a liniilor sursă filtrate pe natură
  (`GenereazaConex` în motor, declanșat de `PoliticaConex`), cu aceleași
  loturi/valori/dimensiuni. **Secundarul (31e)** e hook de tip
  (`GenereazaSecundar`) — construit din date culese pe derivată. Ambele produc
  copii ai grupului conex (`DocumentSursa` + `Autogenerat`): draft șters la
  anularea sursei, operat → blochează anularea/stornarea sursei.
- **`StocService.AlocaFifo` (3b)**: sparge (produs × gestiune × TipStoc × dată
  × cantitate) pe loturi în ordinea vechimii; ARUNCĂ la stoc insuficient.
  Gardianul de sold intermediar re-verifică oricum orice mișcare la operare.
- **Derivările de cost există în seed-ul privat (29c/36c)**:
  `SeedContare6xxDin3xx` cu excepțiile 371→607, 345→711, 381→608 — folosite azi
  de BCS și LDI-minus. Registrele de stoc privat: generic→Magazie, MF→Marfuri.
- **Evaluarea la privat e la NET** (36b): lotul primește preț net; costul
  descărcării nu are componentă de TVA.

## 2. Fluxul țintă (magazinul online) și ce CERE de la model

Prima implementare privată: firmă cu magazin online. Decizia 21 se aplică —
fluxul se ACOPERĂ funcțional, nu se transcrie:

1. **FCL e dictată de site** (prețul cel mai mic din site) și poate conține
   poziții care NU sunt în stoc la data facturării. ⇒ prețul de vânzare e
   cules și complet decuplat de cost; marja poate fi negativă — nicio validare
   preț-contra-cost. ⇒ descărcarea NU poate fi condiție de operare a FCL:
   venitul se postează acum, costul când există stocul.
2. **Comanda dinspre magazin** (sales order) și **PO-urile spre furnizori**
   pentru pozițiile lipsă = documente de comandă, FĂRĂ registre — nu intră în
   P2 (vin cu integrarea API, pasul 5). Ce cere de la P2: **restul nedescărcat
   per linie FCL trebuie să fie interogabil** (cusătura pe care se va așeza
   fluxul de comenzi) și **descărcarea trebuie să poată veni mai târziu**,
   după recepția pe lanțul FCT→NIR.
3. **Identificarea cu prioritate pe poziția specifică din lot**: când comanda
   specifică exact poziția, descărcarea folosește ACEL lot (identificarea
   specifică din decizia 13, first-class); altfel produs + auto-FIFO.

## 3. Tipul nou: `DescarcareGestiune` (DSC)

Al 11-lea derivat (aditiv peste lista din decizia 19; BPR rămâne rezervat).
Reutilizarea BonConsum e respinsă: consumul alimentează +Consum pe primitor
(rămâne pe responsabil — 27a); vânzarea IESE din patrimoniu — doar −1, fără
registru pereche.

- **Laturi**: predator = **gestiunea de descărcare**, primitor = **clientul**
  (partenerul de pe FCL) — marfa pleacă fizic la client. Dimensiunile ambelor
  laturi rămân însă pe gestiune: `RepartitorImplicitCredit() => PredatorId`
  (override polimorf, precedentul Decont 32c) — soldul 371/345 se ține per
  gestiune; clientul trăiește pe rândurile FCL și pe link-ul `DocumentSursa`.
- **Detaliu derivat** `DescarcareGestiuneDetaliu : DocumentDetaliu` cu
  `LinieSursaId?` (FK real spre `DocumentDetaliu` — cross-document, fără ciclu
  de inserție): trasabilitatea acoperirii per linie FCL. Restul = baza pură
  (lot, cantitate, valoare).
- **Valoare = cost**: `PregatesteOperare` materializează
  `Valoare = PretUnitar lot × Cantitate` (pattern-ul BTR/BCS — prețul nu se
  culege). `TipTvaId` null, `ValoareTva` 0 — TVA-ul vânzării e integral pe FCL
  (P1); fără rând `PoliticaTva` pe DSC.
- **Stoc**: RegulaStoc −1 pe predator, generic→Magazie + MF→Marfuri (aceeași
  mapare ca NIR/LDI privat). Contare: `SeedContare6xxDin3xx(dsc, null,
  exceptii)` — 607=371, 711=345, 608=381, 60x=30x per TipMaterial, mecanismul
  existent neschimbat.
- **Validări proprii**: predator Gestiune, lot per linie, cantitate > 0.
  DSC-ul cules MANUAL (fără sursă) rămâne legal — document normal de ieșire
  din gestiune. Numerotare seed `DSC-`.

## 4. Identificarea pe linia FCL: lot prioritar, produs + FIFO altfel

- `FacturaIesireDetaliu` primește **`ProdusId?`** (FK Produs). Testul
  apartenenței (decizia 2): produsul NU apare în formule de stoc sau reguli
  contabile (stocul lucrează pe Lot) — e cheia pickingului la culegere ⇒
  derivată, nu bază.
- **General! + Specific?**: identitatea liniei de stoc e PRODUSUL (poziția din
  site ↔ produs) — `ProdusId` e obligatoriu pe liniile cu Natura=Stoc prin
  VALIDARE (schema rămâne nullable: aceeași derivată poartă și liniile de
  servicii, fără produs). `LotId` de pe BAZĂ = rafinarea specifică OPȚIONALĂ:
  când comanda specifică poziția exactă, linia poartă și lotul, iar
  generatorul îl onorează cu prioritate (nu face FIFO peste el); lotul setat
  trebuie să aparțină produsului liniei (validare). Restul de descărcat se
  calculează astfel întotdeauna pe produs — inclusiv după epuizarea lotului
  pinuit. Efect secundar corect: dimensiunea Material se rezolvă din lot (33b)
  și pe rândurile FCL.
- `FacturaIesire` primește **`GestiuneDescarcareId?`** (FK Gestiune) —
  obligatorie doar când există linii de stoc. P2 = o singură gestiune per
  factură (magazinul online are un depozit); lotul explicit fără sold în ea =
  refuz cu mesaj (întâi transfer BTR). Gruparea multi-gestiune = extensie
  aditivă a generatorului, documentată, neimplementată.

## 5. Generatorul: spargerea pe loturi la GENERARE, serviciu propriu în motor

**Tranșarea principală (întrebarea 1). Spargerea se face la GENERARE**, nu la
operarea descărcării:

- Draftul trebuie să fie CONCRET ca override-ul manual de lot (decizia 13,
  first-class) să aibă pe ce opera — un draft „produs × cantitate" ar amâna
  conținutul real până la operare, unde utilizatorul nu mai poate interveni.
- Operarea nu creează/sparge linii nicăieri în motor (`PregatesteOperare` doar
  materializează valori) — spargerea la operare ar introduce un mecanism nou
  în hot-path pentru un câștig fals de prospețime: gardianul de sold
  re-verifică ORICUM la operare; alocarea învechită refuză zgomotos, iar
  utilizatorul re-alege sau regenerează.
- Consecvent cu lanțul FCT→NIR: și acolo draftul conex poartă loturile
  concrete de la generare.

**Mecanism: serviciu propriu (`DescarcareService.Genereaza`), NU clona
`PoliticaConex`.** Clona nu se potrivește pe niciuna din cele trei axe:
valorile diferă (cost din lot, nu prețul de vânzare), liniile se sparg 1→N pe
loturi, predatorul se ÎNLOCUIEȘTE cu gestiunea (nu se inversează). Declanșare
pe ambele căi, același cod:

- **La operarea FCL**: hook `GenereazaSecundar` pe `FacturaIesire`
  (precedentul Plata automată 31e — secundarul se construiește din date culese
  pe derivată: gestiunea, loturile/produsele liniilor). Motorul îl marchează
  `Autogenerat + DocumentSursa` și `Opereaza` îl întoarce spre editare —
  mecanica existentă, zero schimbare în motor.
- **Acțiune „Generează descărcarea" pe FCL Operat** (controller, ca
  Operează/Stornează): re-rulează generatorul pentru RESTUL nedescărcat —
  calea backorder-ului (marfa a sosit între timp pe FCT→NIR). Documentul
  generat e tot copil al grupului conex (`DocumentSursa + Autogenerat`);
  data lui se culege (default azi) — descărcarea târzie nu poate precede
  stocul, gardianul de sold acoperă natural.

**Algoritmul** (per linie FCL cu Natura=Stoc):

```
rest = Cantitate − Σ cantități pe liniile DSC cu LinieSursaId = linia,
       din documente Draft sau Operat (Stornat nu acoperă; draftul
       contează — altfel a doua generare ar dubla alocarea)
dacă rest ≤ 0 → linia e acoperită, nimic
lot explicit pe linie → alocă min(rest, sold lot în gestiune la dată)
                        DOAR din acel lot (prioritatea identificării
                        specifice); FĂRĂ fallback pe FIFO pentru rest —
                        pinul e intenția magazinului; deblocarea = se scoate
                        pinul de pe linie (produsul rămâne, FIFO preia)
altfel (doar produs)  → AlocaFifo TOLERANT: alocă ce există, întoarce restul
                        (variantă nouă — cea existentă aruncă la insuficient)
TipStoc-ul căutării = din RegulaStoc-ul DSC-ului pe clasa liniei
                      (specifica bate generica, ca în motor — fără hardcode)
```

Rezultatul: un draft DSC cu o linie per (linie FCL × lot) — `LinieSursa`,
lot, cantitatea alocată, dimensiunile clonate de pe linia FCL; fără nicio
alocare → nu se generează nimic. **Restul neacoperit NU intră pe DSC** — se
raportează (mesajul operării / UI pe FCL) și rămâne interogabil per linie
(cusătura §2.2). Fără regenerare automată la recepție — acțiunea manuală
ajunge la P2; declanșarea din NIR = aditivă, când fluxul de comenzi o ceară.

Gardienii de grup existenți acoperă totul fără modificări: anularea FCL șterge
drafturile DSC autogenerate și refuză cât există DSC operat; stornarea FCL
cere întâi stornarea DSC-urilor.

## 6. Contarea FCL pe liniile de stoc: derivarea de VÂNZARE

Regula generică FCL de azi (credit `SursaCont.TipMaterial`) ar posta pe o
linie de stoc `4111 = 371` — contul Tipului e contul de STOC, nu de venit.
Corecția, cu mecanismul existent (TipMaterial exact bate genericul — 26c):

- **`SeedContareVanzare` (nou, în nucleu — mecanism, per decizia 29c)**: rând
  `RegulaContare` per TipMaterial cu Natura=Stoc pe FCL — debit
  `RepartitorPrimitor` (fallback 4111, ca genericul), credit Explicit = contul
  de venit derivat din mapă: **371→707, 345→701, 381→708, fallback 708**
  („Venituri din activități diverse" — niciun tip de stoc nu rămâne fără venit;
  nepotrivirile fine, ex. 703 produse reziduale, se editează ca DATE).
  Incremental la updater, ca 6xx=3xx.
- Regula generică rămâne neschimbată pentru servicii (VEN); separarea preț de
  vânzare / cost e completă: FCL = 4111 = 7xx net + 4111 = 4427 (preț site);
  DSC = 607/711 = 371/345 (cost din loturi). Niciun canal între ele.

## 7. Politici și seed per profil

- **Privat**: seed-ul ȘTERGE rândul `PoliticaValidare` FCL cu
  `NaturaInterzisa=Stoc` (pas explicit de updater — rândul există în bazele
  seed-uite la P1) și adaugă: derivarea de vânzare (§6), politicile DSC
  (RegulaStoc, 6xx=3xx cu excepțiile, numerotarea).
- **Bugetar**: ancora `TipDocument` DSC există (nucleul o seed-uiește pentru
  ambele profiluri, ca BPR), dar FĂRĂ politici → tip inert; rândul
  `NaturaInterzisa=Stoc` pe FCL RĂMÂNE (30a: în profilul bugetar facturarea nu
  descarcă gestiune) — hook-ul `GenereazaSecundar` nu găsește niciodată linii
  de stoc, no-op natural. Diferența de profil iese din DATE, nu din cod.
- Fără tabelă `PoliticaDescarcare` la P2: ținta (FCL→DSC) și filtrul (natura
  Stoc) sunt funcționale, fixate de clasă — nu există încă nimic de
  configurat. Dacă apare (gestiune implicită per bază, praguri), tabela se
  adaugă aditiv.

## 8. Datoriile mici din P1 (aditive, în aceeași felie)

- **Default TipTva per tip de document**: `TipDocument.TipTvaImplicitId?`
  (FK nullable pe ancoră). NU pe `PoliticaTva` — bugetarul n-are rânduri
  PoliticaTva (fără postare) dar are nevoie de default (CAP21), iar un rând
  PoliticaTva doar-pentru-default ar activa pasul TVA din motor. Default de
  CULEGERE, nu de motor: se aplică la crearea liniei (controller XAF; viitorul
  API îl citește la fel); linia culeasă explicit nu se atinge. Seed: privat
  FCT/FCL/DEC → N21; bugetar FCT/FCL/DEC → CAP21.
- **Verificarea culegerii TipTva/ValoareTva în UI XAF**: punct în contractul
  feliei (P1 a validat doar prin ModelCheck) — smoke test manual pe
  FCT/FCL/DEC + culegerea noilor câmpuri (ProdusId, GestiuneDescarcare,
  override de lot pe draftul DSC).

## 9. Validare (contractul feliei — ModelCheck privat)

Bloc e2e nou pe baza privată (suita bugetară rămâne verde nemodificată):

1. NIR manual: marfă în 2 loturi (prețuri diferite) + produs finit.
2. FCL cu: linie marfă acoperind ambele loturi (FIFO), linie cu LOT EXPLICIT
   (prioritatea identificării specifice — sare peste FIFO), linie serviciu,
   linie marfă INDISPONIBILĂ (produs fără stoc) → operare: 4111 = 707/704 net
   + 4427 per linie; DSC draft autogenerat cu spargerea corectă pe loturi;
   restul liniei indisponibile raportat, nu pe DSC.
3. Operare DSC: 607 = 371 la cost (≠ prețul de vânzare), −stoc pe gestiune;
   dimensiunile ambelor laturi pe gestiune, Material din lot.
4. Backorder: NIR nou pe produsul lipsă → acțiunea „Generează descărcarea" pe
   FCL operat → al doilea DSC doar pe rest; a doua apelare → nimic (idempotent
   pe acoperire, draftul contează).
5. Gardieni: anularea FCL refuzată cu DSC operat; anularea cu DSC draft îl
   șterge; storno DSC → acoperirea scade, restul redevine generabil.
6. Produs finit: 711 = 345 la descărcare, 701 la vânzare.
7. Default TipTva: linia nouă pe FCL primește N21, culegerea explicită bate.

## 10. Amânate, documentate

- **Comenzile** (sales order din magazin, PO spre furnizori) și importul FCL
  din site — documente fără registre + API, la pasul 5; P2 lasă cusătura:
  restul per linie FCL interogabil, identificarea produs/lot pe linie.
- **Regenerarea automată a descărcării la recepția NIR** — aditivă peste
  acțiunea manuală, odată cu fluxul de comenzi.
- **Descărcarea multi-gestiune per factură** (gruparea alocărilor pe gestiuni,
  un DSC per gestiune) — extensie a generatorului.
- **Evidența cu amănuntul la preț de vânzare (371/378/4428)** — NU intră:
  evaluarea rămâne identificare specifică la cost net (deciziile 13/36b);
  dacă apare clientul cu amănuntul clasic, e profil de politici nou, nu
  schimbare de motor.
- **Rezervarea de stoc** (alocarea din draft nu rezervă; gardianul de sold
  decide la operare) — suficient single-operator (limitarea 25 asumată);
  rezervarea = mecanism viitor odată cu multi-operator/API.

## 11. Tranșări deschise (de confirmat înainte de implementare)

1. Tip nou `DescarcareGestiune` (DSC, al 11-lea derivat) cu detaliu derivat
   purtător de `LinieSursaId` (FK real spre DocumentDetaliu); laturi
   gestiune→client cu ambele dimensiuni pe gestiune — recomandat: da.
2. Spargerea pe loturi la GENERARE (draft concret, override-abil), cu
   `AlocaFifo` variantă tolerantă (alocă disponibilul, întoarce restul);
   gardianul de sold rămâne autoritatea la operare — recomandat: da.
3. Generator = `DescarcareService` în motor, declanșat prin hook
   `GenereazaSecundar` la operarea FCL + acțiune manuală „Generează
   descărcarea" pe rest (backorder); FĂRĂ tabelă PoliticaDescarcare la P2 —
   recomandat: da.
4. Culegerea FCL — General! + Specific?: `ProdusId?` pe detaliu (derivată),
   OBLIGATORIU prin validare pe liniile de stoc (identitatea liniei =
   produsul); `LotId` de bază = rafinarea specifică opțională, prioritară la
   picking, validată ca aparținând produsului; `GestiuneDescarcareId?` pe
   header — recomandat: da.
5. Derivarea de vânzare pe FCL (rând per TipMaterial de stoc: 371→707,
   345→701, fallback 708, ca date incrementale) + ștergerea
   `NaturaInterzisa=Stoc` din seed-ul privat (rămâne la bugetar) —
   recomandat: da.
6. Datoria P1: `TipTvaImplicitId?` pe `TipDocument`, aplicat la CULEGERE (nu
   în motor); seed N21 privat / CAP21 bugetar pe FCT/FCL/DEC — recomandat: da.
