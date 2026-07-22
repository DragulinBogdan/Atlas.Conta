# Inventar legacy — Motorul de postare (transversal)

Surse: `spGestValideazaDocument`, `spGestCreateDocConex`, `spGestAnuleazaDocum`,
`spGestNotaCulegere`, `SP_GETSTOCK`, `vStockAll`, `fnStockCodMat/Sum`,
`legacy/Gestiune/TCVUnit.pas`. Extrasele brute: `db/export/`.

> **Rama inventarului (decizia 21):** config-ul legacy a fost făcut fără
> verificări și e stratificat pe generații (ex: `GEST_DEFA_NOTA_CONT` are
> seturi de 141 de rânduri pe combo-uri moarte 261/265). Ce e documentat aici
> e DIRECȚIE și evidență de funcționalitate, NU sursă canonică de seed.
> Politicile noi se definesc curat per cerință; legacy-ul arbitrează doar
> „ce trebuia să facă sistemul".

## 1. Ciclu de viață și staging

Legacy are deja separarea Draft/Operat, implementată prin **tabele fizice separate**:

- Culegerea scrie în `CULGEST_DOCUM` / `CULGEST_ITEMSI` (draft).
- „Validarea" (`spGestValideazaDocument`) copiază în `GEST_DOCUM` / `GEST_ITEMSI`
  prin SQL dinamic construit din `syscolumns` (intersecția coloanelor — tolerant
  la diferențe de schemă între clienți), apoi **șterge draftul**.
- Anularea = soft delete: `STARE=0` + `DATA_STERGERE` + `ID_UTILIZATOR_STERGERE`.
  Toate query-urile de stoc/note filtrează `STARE=1`.

**→ modelul nou:** decizia 14 (Draft→Operat→Stornat pe aceeași entitate; registrele
persistate înlocuiesc rolul lui `STARE=1` ca filtru implicit).

## 2. Identificatorii de legătură pe GEST_DOCUM

Setați la postare (self-reference dacă nu există):

| Câmp | Semantică |
|---|---|
| `ID_INITIAL` | documentul sursă din care a fost generat (conex) |
| `ID_DOCUMENT_CONEX` | rădăcina grupului de documente legate (FCT + NIR-ul ei) |
| `ID_TRANZACTIE` | id în cadrul grupului |
| `ID_MODIFICARE` | lanț de versiuni la modificare |

Anularea operează pe **tot grupul conex** (vezi §8).

**→ modelul nou:** relație tipizată `DocumentConex` (sursă→generat) + grup de
tranzacție; lanțul de modificare dispare (istoricul îl țin registrele + storno).

## 3. Limbajul de formule (config → cod)

- **Pe poziții** (`GEST_DEFA_DOCUM_ITEMSI.FORMULA_CALCUL`): expresii SQL peste
  coloanele-surori ale liniei, cu ordine explicită `PRECEDENTA`.
  Lanț tipic: `PRET_RECEPTIE = PRET_UNITAR` → `PRET_RECEPTIE_TVA` →
  `TVA_RECEPTIE` → `VALOARE_RECEPTIE` → `VALOARE_RECEPTIE_TVA`.
  Evaluate client-side în Delphi (`FEvaluator`) în timpul culegerii.
- **Pe header** (`GEST_DEFA_DOCUM_DOCUMENT.FORMULA_CALCUL`): fie `SUM(coloană)`
  peste poziții (footer + agregat), fie expresii simple. Re-evaluate la postare
  prin UPDATE dinamic (`[camp] = (SELECT TOP 1 <formula> FROM gest_itemsi ...)`).
- Config-ul enumeră TOATE coloanele fizice (66 pe itemsi) per combo, cu
  `VISIBLE`/`REQUIRED`/`READONLY`; câmpurile „vii" per tip = vizibile sau cu
  formulă sau referite de reguli.
- Valori implicite ca formule constante: `'M'`, `'BUC'`, `'1'`.

**→ modelul nou:** proprietăți calculate C# pe clasele derivate (ordinea o dă
compilatorul; `PRECEDENTA` moare); agregatele header = proprietăți peste colecția
de linii; obligativitatea → validare declarativă (decizia 4).

## 4. Motorul de stoc

Regula: `GEST_DEFA_STOC_TIP_PRODUSE(id_gest_defa_docum, PREDATOR, id_gest_tip_produse) →
(id_gest_tip_stoc, SEMN, SEMN_ITEMS)`:

- `PREDATOR` = 1/2: pe ce latură a documentului se aplică (repartitorul afectat).
- `SEMN` = ±1 aplicat pe cantitate; `SEMN_ITEMS` = filtru pe semnul cantității
  liniei (`sign(cantitate) = semn_items`); în practică regulile sunt dublate
  pentru −1 și +1 (filtru efectiv inexistent).
- `id_gest_tip_stoc` → `GEST_TIP_STOC` (magazie/consum/custodie/folosință/
  mărfuri/gratuit...), cu nivel și grupă.
- ATENȚIE: tabela are dublă cheiere istorică — rânduri vechi pe
  `ID_GEST_TIP_DOCUM` cu `id_gest_defa_docum NULL` (122 rânduri) și rânduri noi
  pe defa (240). `vStockAll` joinează DOAR pe defa ⇒ rândurile pe tip par moarte.
  De verificat înainte de seed (LDI le are duplicate pe ambele chei).

Evaluarea: `vStockAll` = view care join-ează fiecare linie postată cu regula și
emite `stock = semn × cantitate`, `stockValoric = semn × valoare_receptie_tva`
(fallback `cantitate × pret_receptie_tva` din nomenclator). **Stocul e evaluat
cu TVA** (instituție publică, TVA nedeductibil capitalizat).

Stocul „la dată" (`fnStockCodMat/Sum`) se calculează **invers**: totalul până azi
minus mișcările de după data cerută (`SP_GETSTOCK` #stockLaZi).

Granularitate după parametrul de societate `tipStock`:
- `'M'` (aici): direct pe `codmat` (lot).
- `'S'`: culegere pe sumator, spartă automat pe codmat-uri la postare
  (`spSpargeCodSumCulegere`) — auto-picking de loturi există deja.

Lotul: la postare, `GEST_GNMCL` (rândul-lot) e ștampilat cu
`id_document_intrare`, `data_cod`, `GestIntrare`; conexul NIR adaugă
`id_document_receptie`.

**→ modelul nou:** deciziile 13–14. Regula (semn × latură × tip stoc × filtru
tip produs) rămâne date; `vStockAll` devine registru persistat scris la operare;
`SP_GETSTOCK`+15 variante și inversarea „la dată" dispar (interogare directă pe
registru).

## 5. Motorul de note contabile

Două generații de config:

1. `GEST_DEFA_NOTA_CONT` (veche): `(defa, tip_material) → cont_debitor/cont_creditor`,
   FORMULA/CONDITIE aproape nefolosite. Azi servește conturi implicite
   (ex. `SP_GETSTOCK`, plata automată).
2. `gest_defa_docum_nota_model` (curentă, 181 rânduri): per
   `(defa, id_gest_tip_material)`, **fragmente de expresii SQL ca date**, splice-uite
   în SQL dinamic de `spGestNotaCulegere`:
   - `formula_nota` = valoarea (ex. `gest_itemsi.Valoare_receptie_tva`)
   - `ContDebit/ContCredit` = `isnull(gest_itemsi.ContD, '<cont implicit>')` —
     override-ul de linie e integrat în regulă
   - `repartitorDebit/Credit` = `gest_docum.id_predator` / `id_primitor`
   - dimensiuni: `cod_functional`, `cod_economic`, `id_angajamente_defalcare`
     preluate de pe linie
   - **fallback pe conex**: dacă documentul nu are modele proprii, se folosesc
     modelele combo-ului conex (aceeași direcție predator/primitor).

Cheia de mapare contabilă e `ID_GEST_TIP_MATERIAL` = exact planul Clasă/Tip
(decizia 4: rămâne date). Nota finală trăiește în `CNOTE` cu tripletele
`dim/dim_d/dim_c` (decizia 15 le înlocuiește).

**→ modelul nou:** tabelă de politică: tip document × Clasă/Tip → cont D/C +
`Dimensiuni` (Comun/OverrideDebit/OverrideCredit); motorul C# rezolvă și scrie
rândurile de registru contabil. Fragmentele SQL dispar.

## 6. Documentul conex

`spGestCreateDocConex(@sursa, @defa_țintă)`:
1. Setează `ID_DOCUMENT_CONEX` pe sursă (rădăcina grupului).
2. Clonează header-ul; `TIP_DESCARCARE` ≠ 0 ⇒ swap predator/primitor;
   numerotare automată (`fnGestNextNumarDocum`) sau `NR_DOC_CONEX` cules pe sursă;
   `AUTOGENERAT=1`, `ID_INITIAL = sursă`.
3. Clonează DOAR pozițiile cu tip material permis de țintă
   (`GEST_ITEMSI_TIP_MATERIAL` per defa țintă) — filtru de conținut.
4. Re-evaluează formulele header ale țintei; ștampilează lotul cu
   `id_document_receptie`.
5. Delphi deschide documentul generat în editare (`btnValidareConex`).

Config: `GEST_DEFA_DOCUM.ID_DOCUMENT_CONEX` (FCT→NIR=4, BF→NIR).

**→ modelul nou:** decizia 17 — generare tipizată (FacturaIntrare → NIR) cu link
persistat; filtrul pe tip material devine politică per pereche conexă.

## 7. Plata automată (decont)

La postare, dacă `DECONT_GENERATE=1` pe header (câmpuri `DECONT_*` culese):
`BREGISTRU` (înregistrarea de plată) + `BREG_P` (defalcare bugetară) +
`GEST_DECONTARI` (m2m document↔plată, `AUTOGENERAT=1`) +
`GEST_DEFALCARE_DECONTARI` (defalcare **pe poziții**, cu `VALOARE_RECEPTIE_TVA`).
Contul plății: primul `CONT_CREDITOR LIKE '4%'` din `GEST_DEFA_NOTA_CONT`.

**→ modelul nou:** decizia 17 — Plata devine document; generarea automată devine
caz de „document conex" (FacturaIntrare → Plata), imperecherea entitate proprie
cu sume la nivel de document (defalcarea pe poziții: de confirmat dacă e necesară).

## 8. Anularea

`spGestAnuleazaDocum` — gardieni, în ordine:
1. **Dependență**: pentru facturi/NIR, dacă codmat-urile documentului apar în
   documente ulterioare NE-factură/NIR (`den_docum LIKE '%factura%'` /
   `'%N%I%R%'` — euristică hardcodată pe DENUMIRE!) ⇒ refuz.
2. **Perioadă fiscală închisă** (`PERIOADE_FISCALE.inchisa=1`) ⇒ refuz.
3. În afara exercițiului definit ⇒ refuz.

Apoi: soft delete pe **tot grupul conex** + ștergerea plăților autogenerate
(BREGISTRU/BREG_P/GEST_DECONTARI/defalcări).

**→ modelul nou:** decizia 14 — aceeași schemă, dar dependența devine exactă
(rânduri de registru pe loturile produse) în loc de euristică pe denumire, iar
anularea grupului scrie storno în registre, nu filtru STARE.

## 9. Validări / aprobări

- `GEST_TEMPLATE_VALIDARI` per defa: `TIP_VALIDARE`, `ID_FUNCTIUNE` (rol),
  `ZILE_GRATIE`, `PRIORITATE` — definesc cine confirmă documentul.
- `spGestIntrodValidari` creează instanțe în `GEST_VALIDARI_DOCUM` la postare
  (11.089 rânduri = cozi de confirmare).
- Separat: `SP_GEST_DOCUM_CAMPURI_LIPSA` — validare de completitudine.

**→ modelul nou:** workflow de aprobare — de decis dacă intră în faza 1 sau
rămâne la nivel de stare simplă (de discutat la modelul nou).

## 10. Reguli hardcodate găsite (de tratat explicit la seed)

| Loc | Regula |
|---|---|
| `spGestValideazaDocument` | tip 12 (FCT IESIRE) exceptat de la obligativitatea angajamentului |
| idem | tip 2 (FCT): interzis cantitate negativă pe `produs='M'` |
| idem | `TOTALDOC`: tip 12 folosește `VALOARE_LIVRARE`, restul `VALOARE_RECEPTIE_TVA` |
| idem | utilizator 402 ⇒ `NR_NOTA = 'luna/MAG'` (hack per utilizator) |
| idem | `TOTALTVA = Σ VALOARE_RECEPTIE_TVA × curs` — denumire înșelătoare (nu e TVA) |
| `spGestAnuleazaDocum` | clasificarea FCT/NIR prin `LIKE` pe denumire |
| `SP_GETSTOCK` | propagare `COD_FUNCTIONAL`/unitate/proiect din ultima folosire a codmat-ului |
| `TCVUnit.pas` | `SEMN_CANTITATE=0` pe linie ⇒ „nomenclator" (linie fără mișcare) |

## 11. Refolosiri semantice de câmpuri (confirmate)

| Câmp fizic | Pe tip | Semantică reală |
|---|---|---|
| `DESCRIERE_CURS` (header) | FCT | „NR_PV" (număr proces verbal) |
| `DATA_EMITERE` (header) | FCT | „Data PV" |
| `DECONT_DATA`, `DATA_CHITANTA` | FCT | recaptionate „DATA_PV" |
| `TIPMAT` (items) | NIR | „LOT/Clasa Material" — dublu rol |

(se completează pe măsură ce acoperim restul tipurilor)

## 12. Defalcarea procentuală (`*_procent`) — hack cu nevoie reală

`culgest_itemsi_procent` → `gest_itemsi_procent` (copiate la postare „pentru
istoric"): defalcare procentuală a liniei **de la angajament în jos**
(procent, sumă, cantitate, cod funcțional/economic, proiect, unitate per
felie). Confirmat (proprietar): hack pentru lipsa dimensiunii Sursă de
finanțare. Nevoia reală din spate: **cofinanțare multi-sursă pe aceeași linie**
(ex. 85% UE + 15% național). În modelul nou NU se preia mecanismul; se
proiectează defalcare explicită pe seturi de `Dimensiuni` cu pondere, odată cu
dimensiunea SursaFinantare (decizia 21).

## 13. Întrebări deschise

1. FCT (140 modele note) vs NIR (23 modele, fallback conex): care postează
   efectiv 3xx=401 în producție. Relevanța scade sub decizia 21 (seed pe
   funcționalitate) — rămâne de definit CURAT unde postăm în modelul nou:
   recomandare — pe NIR (recepția), factura poartă doar obligația 401/408.
2. Rândurile de reguli stoc cheiate pe `ID_GEST_TIP_DOCum` (fără defa) și
   combo-ul fantomă 258: zgomot confirmat de decizia 21 — se ignoră la seed.
3. `GEST_DEFALCARE_DECONTARI` (imperechere pe poziții): necesară în modelul nou
   sau suficient la nivel de document?
4. Override-urile de postare pe linie (`ContD/ContC/RepD/RepC`): pe FCT `ContC`
   e obligatoriu, pe DEC toate patru sunt vizibile (decontul e cvasi-notă
   manuală). Nuanțează decizia 15: nu se preiau ca mecanism GENERIC de
   override, dar Decont are nevoie de specificare explicită de postare pe
   linie ca trăsătură PROPRIE a tipului.
