# Pas 3 (transferul), explorarea 3: stocul în cub — rândul unificat cu 3xx, `TipStoc`, sink-urile, linia FCT contra NIR

Scop: faptele (cod + cifre) pentru trei tranșări de modelare ale nucleului
nou: (1) dacă postarea de stoc (cantitate) și postarea contabilă pe 3xx
(valoare) sunt UNA sau două postări; (2) dacă `TipStoc` e derivabil din
`(Cont, Gestiune reală/virtuală)` și cum se re-cheiază politicile lui;
(3) dacă linia facturii de intrare (FCT) postează ea recepția sau rămâne
NIR-ul. NU tranșezi tu; livrezi fapte.

Citește întâi: `docs/nucleu/nucleu-cub-design.md` §2–§5;
`docs/nucleu/nucleu-coordonate-rapoarte.md` §2 („Ce NU e coordonată”), §5.6,
§5.8, §5.11; `docs/nucleu/nucleu-fizica.md` §2.5 (ultimul paragraf) și FZ-r5;
`run-nucleu/fizica/02-semantica-mapare.md`; `run-nucleu/fizica/pas1/01-schema.sql`.
CodeGraph există (`.codegraph/`): `codegraph explore` înainte de grep.

Mediu: Postgres 18.4 în docker `contapal-postgres-1`:
`docker exec contapal-postgres-1 psql -U postgres -d Atlas.Conta.Nucleu.Fizica.x1 -Atc "..."`;
scripturi prin `docker cp` + `-f`. Schema `public` = registrele de azi
(`RegistruStoc`, `RegistruContabil`, `Documente`, `DocumentDetalii`, `Loturi`,
`Produse`, `Repartitori`, `PlanConturi`, politicile), `f2."Postare"` = cubul.
Ai voie DOAR schema `tr3` pentru ajutătoare; nimic în `public`/`f2`/`cub`;
Flax nu se atinge; `.x10` nu se folosește. SQL-urile în
`run-nucleu/transfer/03-sql/`, ieșirile brute în `03-out/`.

Scrie în `run-nucleu/transfer/03-stoc-sinkuri.md`:

## A. Din cod (fișier:linie pe fiecare afirmație)
1. `TipStoc`: enum-ul (valorile), TOATE locurile care îl citesc sau îl scriu
   (grep integral în `nou/`: motor, politici, proiecții, SAF-T, API, client
   React — listă cu fișier:linie și ce decid pe el).
2. Politica de stoc: tabela/tabelele (`PoliticaStoc`? semn, filtru, TipStoc
   per tip document × rol), `PoliticaContare` pentru 3xx/371/6xx/7xx,
   `PoliticaMiscareSaft` (cheia, rândurile din seed privat), mulțimea
   „gestiunilor de patrimoniu” a PhysicalStock. Cum se decide azi contul de
   stoc (371 vs 3xx) — din produs (Clasă/Tip), din gestiune, din politică?
3. Fluxul unei linii de stoc prin motor: `PregatesteOperare` → `StocService`
   → `DescarcareService`/`LoturiCulegereService` → rândul de `RegistruStoc`
   ȘI rândul de `RegistruContabil` pereche: unde se scriu, cu ce chei
   comune (`DetaliuId`, `LotId`, `Material`), când rândul de stoc N-ARE
   pereche contabilă (NotaTransfer, deschideri, custodie, alt caz) și când
   rândul contabil pe 3xx N-ARE rând de stoc (există?). Reevaluarea/corecția
   de preț azi (există? DVI pe loturi).
4. Sink-urile: consumul (DSC → 6xx), folosința, gratuitul, producția
   neterminată: ce `Repartitor`/`TipStoc` primesc azi, ce cont; asamblarea
   (ASM) — intrare + ieșire în același document; inventarul (plus/minus).
5. FCT contra NIR: ce poartă linia FCT (`FacturaIntrareDetaliu`: cantitate,
   preț, produs, `LinieSursa`?), ce poartă linia NIR, cum se generează
   conexul (cine din cine: FCT → NIR sau NIR → FCT; politica de conex),
   ce postează fiecare (stoc, 3xx, 401, 4426), FCT de servicii (fără stoc),
   DVI și legătura n→m cu facturi. SAF-T: ce citește de pe linia FCT contra
   linia NIR (`SaftProiectii.cs:890-949`, `:1111-1117`, `:1158-1164`).

## B. Cifrele, pe Fizica.x1 (SQL + ieșire brută la fiecare)
1. `RegistruStoc` pe (tip document, `TipStoc`, semn): număr de rânduri,
   Σ cantitate, Σ valoare.
2. Perechea stoc ↔ contabil: pentru fiecare rând de stoc, există un rând
   contabil cu același `DetaliuId` (sau altă cheie — spui care) și cu
   `Material`/lot potrivit? Categorii: 1:1 cu aceeași valoare / 1:1 cu
   valoare diferită (cât diferă, de ce — TVA? rotunjire?) / fără pereche
   (pe tip) / 1:n. Invers: rânduri contabile pe conturi de stoc (3xx, 371,
   381…) fără rând de stoc — pe tip și cont.
3. `TipStoc` contra `(Cont al laturii de valoare, Repartitor)`: tabel de
   contingență — pentru fiecare `TipStoc`, ce combinații (Cont, tip de
   repartitor, repartitor) apar și câte rânduri. Întrebarea de răspuns cu
   cifră: există două valori de `TipStoc` cu ACEEAȘI (Cont, Repartitor)?
   (dacă da, `TipStoc` nu e derivabil — spune pe ce rânduri).
4. `PoliticaMiscareSaft`: cheia de azi contra `(tip document × Cont × semn ×
   rol terț)` — pe rândurile REALE de stoc, cheia nouă e injectivă spre
   codul de mișcare? (câte coliziuni, care).
5. FCT/NIR: câte linii FCT au `LinieSursa` spre NIR (sau invers), câte FCT
   n-au NIR (servicii), câte NIR n-au FCT; pe perechi: cantitatea și prețul
   liniei FCT contra liniei NIR (egale? diferă — cât, câte); câte FCT au
   >1 NIR și câte NIR agregă >1 FCT.
6. Custodie / 803x: rânduri vii? (cifra).
7. Economia reală a unificării: numărul de postări de stoc care AR fi
   aceeași postare cu 3xx (perechea 1:1 din B2) și numărul celor care rămân
   separate; procent din cub.

## C. Ce n-ai putut proba
Scurt.

Reguli:
- Cifre din `count(*)`, nu estimări; fiecare cu SQL-ul salvat.
- Fără propuneri, fără „ar trebui”. Faptele se raportează și când contrazic
  designul (ex. dacă `TipStoc` NU e derivabil).
- Nu atingi `nou/`, nu rulezi build-uri, nu scrii în alte baze.
- Regulă de oprire: A (5 puncte) + B (7 puncte) + C. Ce nu se poate obține
  se declară, nu se aproximează.
- Răspunsul final către coordonator: max 15 linii — cifrele-cheie (B2, B3,
  B5, B7), surprizele, ce n-ai putut proba.
