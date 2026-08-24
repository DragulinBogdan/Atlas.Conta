# Decizia 49 — Felia 1C-c — documentele 2025 prin motor

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §49
- **Docs**: docs/import/faza-1c-design.md

---

**Felia 1C-c — documentele 2025 prin motor — executată; ianuarie complet
prin motor (15.235 documente, 0 eșecuri, ~8,5 min), contractele (1) sold
per cont și (2) închiderea de TVA VERZI pe ianuarie (ITV = exact cifrele
1C — forcing function-ul 44.1 a trecut); contractul (3) stoc cu 112
nepotriviri diagnosticate pe familii (ASM-consum + bani mărunți), gate-ul
integral rămâne 1C-d. Tranșările:**
(a) **Module (pas 0)**: regula 36a uniformizată pe FCT/FCL/DEC
(`pastreazaTvaCules`); **rolul de stingător în imperechere e POLIMORF**
— `Document.CapacitateStingere(os)` → contrapartide cu plafon PER
CONTRAPARTIDĂ (trezoreria = una singură cu totalul, identic numeric cu
31d; NotaContabila = repartitorii expliciți ai liniilor, numărați per
latură — compensarea 401=4111 de X stinge X datorie ȘI X creanță);
migrația ImperechereStingator (FK relaxat la Document, filtrarea în
validare; `DocumentTrezorerie` declarat explicit în model — capcana TPT);
seed privat N19/TI19 (istoric, fără coduri SAF-T) + partener CF.
(b) **Pre-flight enforcing + dicționar-LISTĂ** (48c executat):
`mapari-conturi.csv` comentat (triajul real = 7 decizii, nu ~130;
capcana 6351→6584 nu 635); măturile (coduri, Recorder-e cu volume,
acoperirea Posted, view-uri stale) pică rularea zgomotos. Cititorii pe
LUNĂ (registrul per document ar fi costat ~2,5h/an; pe lună ~10s/an).
(c) **Arhitectura buclei**: idempotență per document (legătura
`1C:<view>` în ACELAȘI commit cu draftul; legat+Draft→re-operare; copiii
autogenerați reluați), ITV prin aceeași cale `ImportaDocument`
(regenerabil la storno — doc al uneltei), imperecherile = trecerea 2 cu
legături proprii, supapa 48a cu disponibil pe linia de timp, **rețeta
NTC-punte cu WHITELIST** (rândul 1C inexprimabil în documentul tipizat se
transcrie pe categorii explicite: punte de stoc via 7588, punte de
cheltuială, reclasificarea BTR cu alias de lot, avans/aviz/imobilizări,
extras-alte; orice rând fără rost declarat = eșec zgomotos, NU catch-all).
(d) **Faptele sursei care au dictat mecanica**: sumele secțiunilor în
VALUTA documentului (conversie round(Suma×Curs,2) per linie); view-urile
SkyConta STALE (cotele 21% + tipul nou IncasareCard/_Document7633 —
citit RAW, TypeRef=numărul tabelei; regenerarea view-urilor = precondiție
1C-d verificată de pre-flight); documentul stins al extrasului e în
SUBCONTO 3, nu DocBaza (ordinele nu postează — nu se importă); FCL-ul de
import poartă DOAR linii de venit (contul 1C exact), stocul integral pe
DSC manual pin-uit (auto-DSC-ul P2 s-ar fi declanșat altfel); criteriul
de skip = „antet Posted care NU postează" (TipOperatiune e etichetă
implicită, NU consignație); facturile „doar 408" (marfa intrată pe aviz)
= linii ne-stoc pe Tipul 408 (decizia a-2 — documentul rămâne țintă de
imperechere); RVA = FCL(CF)+DSC+INC-uri; 891 = hub tehnic viu, comparat
ca orice cont; 1C intră pe MINUS pe celule de stoc și intra-an —
categoria justificată „negativ de sursă" mărginită de cifra sursei,
citită din pozițiile brute per lot.
(e) **Fix de MODEL scos de import: scara numerică** (`Comun/Scara.cs`):
bani numeric(18,2), prețuri (18,6), cantități (18,3), aplicate PE NUME cu
GARDIAN la construirea modelului (decimal nou nemapat = throw); rotunjire
la materializare în motor (rădăcina: finalizarea lotului propaga scara
împărțirii; SUM server-side depășea mantisa — OverflowException în
ImperechereService.Total). Migrația ScaraNumerica alter-only; bazele de
import pre-fix se reconstruiesc (--recreeaza), nu se migrează în loc.
(f) **Review advers**: D1 fixat (stingerile compensării pe AMBELE laturi
— 104 linii/an), D2 fixat (ITV nu se generează pe lună cu eșecuri;
stornat→regenerare), D5 fixat (puntea declară Simbolul de postare, nu
Cod-ul geamănului „S371"); **lotul pre-1C-d documentat**: D3 (fereastra
punte→document poate bloca definitiv; deblocare țintită), D4 (draft
eșuat = alocare învechită; replanificare), D6 (îngustarea bucket-ului
justificat — un BCS pierdut pe perechea 6xx/3xx poate azi trece
neobservat), O1/O4/O5 + diagnozele ASM-consum (ancoră SED00000002/28.01)
și RLF 28,09.
(g) Unealta e single-operator pe bază (race pe delete+rewrite la
deschidere = dubluri exacte, auto-vindecabile la re-rulare); măsurat:
luna ~8,5 min ⇒ anul estimat sub 2h — fără optimizări (§3 respectat).
