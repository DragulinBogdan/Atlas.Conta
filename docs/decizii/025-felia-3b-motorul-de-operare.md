# Decizia 25 — Felia 3b

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §25

---

**Felia 3b — executată; motorul de operare în `Module/Motor/`
(`MotorOperare`, `StocService`, `DimensiuniResolver`, `GardianPerioada`),
validat end-to-end pe NotaTransfer în ModelCheck (28 verificări, pe
`EFCoreObjectSpaceProvider` standalone — docs 113709). Tranșările:**
(a) **RegistruContabil poartă dimensiuni PER LATURĂ**:
`DimensiuniDebit`/`DimensiuniCredit` (două owned) înlocuiesc
`RepartitorDebit/Credit` + setul unic — regula are Override per latură,
deci rezultatul rezolvat diferă (echivalentul tripletelor dim_d/dim_c din
CNOTE). Coalesce per latură: linie → override(latură) → comun → default
header (debit←Predator, credit←Primitor — 00 §5).
(b) **Hooks polimorfe pe bază**, consumate doar de motor:
`PregatesteOperare(os)` (derivata materializează `Valoare` — BTR/BCS:
preț lot × cantitate) și `ValideazaOperare(os, erori)`. Lucrează pe
FK-uri + IObjectSpace, NU pe navigații (contextul apelant nu garantează
lazy loading); motorul preîncarcă Clasa/Natura per Tip la fel.
(c) **Loturile nu se nasc în motor** (baza nu are ProdusId — testul
bazei): linia de intrare creează Lotul la culegere (`Lot.LinieIntrare`),
motorul îl FINALIZEAZĂ la operare (PretUnitar = Valoare/Cantitate, Data).
Se exersează la 3c/NIR.
(d) **Gardieni**: perioadă lipsă = închisă; sold intermediar = prefix-sum
pe zile per cheie (Lot × Repartitor × TipStoc) ≥ 0 — acoperă și operarea
retroactivă; anularea (corecția directă) simulează eliminarea rândurilor
proprii + cere ca loturile create să nu fie atinse de alte documente;
storno = rânduri inverse (flag `Storno`) la data stornării, cu aceeași
verificare din acea dată încolo; grup conex: gardian conservator (refuz
cât timp există copii `DocumentSursaId` operați) până la mecanismul
complet din 3c.
(e) **`RegistruStoc/Contabil.DocumentId` e nullable**: null = rând de
sold de deschidere scris de migrare (decizia 12), fără document sursă.
(f) Numerotarea se asignă la operare din `PoliticaNumerotare` (seed BTR).
UI: `DocumentOperareController` (Operează/Anulează/Stornează) doar
deleagă la motor — aceeași cale o va folosi tierul Web API (pasul 5).
Limitare asumată, documentată în cod: verificarea de sold și commit-ul nu
sunt serializate între utilizatori concurenți (back-office cu operator
unic); la nevoie: advisory lock Postgres per cheie de stoc, aditiv.
