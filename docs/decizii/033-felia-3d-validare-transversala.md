# Decizia 33 — Felia 3d — validarea transversală

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §33

---

**Felia 3d — validarea transversală — executată; validată e2e în ModelCheck
(177 verificări: refuz per cont pe 404/BFEPR, puntea angajamentului,
politici per tip pe FCT/DEC/PLT/FCL, venituri/trezorerie cu E). Tranșările:**
(a) **Dimensiunile obligatorii per cont = gardian generic în motor**
(decizia 15 închisă): flag-urile `Cont.DimensiuniObligatorii` se verifică
pe seturile REZOLVATE ale rândului contabil, per latură (debitul contra
DimensiuniDebit, creditul contra DimensiuniCredit); toate lipsurile se
raportează împreună, cu simbolul contului și linia. **Puntea E**: până la
modulul de angajamente, `AngajamentId` pe linie satisface cerința de
CodEconomic (clasificația trăiește în angajament; când modulul apare,
rezolvarea va materializa CodEconomic din angajament și puntea moare).
(b) **Dimensiunea Material se rezolvă implicit din lot** (`Lot.Produs` →
MaterialId, ambele laturi, ultimul nivel de coalesce alături de
repartitorul implicit polimorf) — închide nota „de confirmat la motor";
în CPLAN-ul bugetar niciun cont nu cere azi M, dar mecanismul e viu.
(c) **`PoliticaValidare` (tabelă nouă): obligativitățile per tip ca profil
de validare** (decizia 29) — `CereClasificatieBugetara` (angajament SAU cod
economic per linie) și `NaturaInterzisa`. Seed bugetar: FCT/DEC/PLT cer
clasificație (29b/32d închise; 31f închis — INC nu primește rând: veniturile
n-au angajamente, iar defalcarea E a conturilor de trezorerie cere oricum
codul economic la nivel de cont); FCL interzice natura Stoc (30a închisă).
Hardcode-urile echivalente din FacturaIntrare/Decont/FacturaIesire au fost
șterse. Motorul aplică politica generic, înaintea hook-ului
`ValideazaOperare` al tipului, cu erorile cumulate în aceeași listă.
(d) **`Opereaza` restructurat pe calculează-validează-materializează**:
rândurile contabile se calculează (regulă, conturi, dimensiuni) și se
validează ÎNAINTE de orice `CreateObject` de registru (ca mișcările de
stoc din 3b) — un refuz al oricărui gardian (sold, cont nerezolvabil,
dimensiuni lipsă) nu lasă rânduri-fantomă în ObjectSpace-ul apelantului;
finalizarea loturilor s-a mutat tot înaintea materializării.
(e) **Invarianții imperecherii erau deja transversali** (31d, în
`ImperechereService` + gardianul de anulare/storno din motor) — nimic nou;
re-confirmați e2e. Consecință de date asumată: activarea flag-urilor CPLAN
cere acum cod economic pe veniturile facturate (751/750 — E), plusul de
inventar (791 — E) și liniile de trezorerie (531/542/770 — E) — cerințe
reale ale profilului bugetar, editabile ca date. Felia 3d ÎNCHISĂ.
