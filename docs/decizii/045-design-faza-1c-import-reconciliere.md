# Decizia 45 — Design FAZA 1C — conector de import + reconciliere pe an fiscal complet

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §45
- **Docs**: docs/import/faza-1c-design.md

---

**Design FAZA 1C — conector de import + reconciliere pe an fiscal
complet — FIXAT** (`docs/import/faza-1c-design.md`, toate tranșările
confirmate 25.07.2026; implementarea = feliile 1C-a…1C-d din design §11).
Sursa inventariată pe date (baza `flax`, view-urile SkyConta în
`EServicesFlx` schema `[flax]`): 2025 = an complet cu închidere de lună pe
12 luni (4427=4426, 4427=4423, plata 4423=512), politică FIFO + TVA la
EMITERE (TVA la încasare rămâne amânat — 36f), loturile VII ca subconto
(produs × doc achiziție × depozit; `BalantaNivel3` = stoc per lot cu
cantitate+valoare). Tranșările:
(a) **Sursa = view-urile SkyConta** (contract de coloane, nu dependență de
cod); OneCvProvider rămâne proiectului de import generic (35a). Igienă:
log curat la generare, LTRIM, corecție an rezidual, agregare valute,
filtre pe Period.
(b) **Se importă anul 2025 OPERAT PRIN MOTOR** (nu copiat în registre):
deschidere 01.01.2025 (solduri contra 891 + Lot per poziție BalantaNivel3;
loturile negative — artefactul returului-ca-lot — netate și raportate),
apoi documentele cronologic, idempotent prin MigrareLegatura
(`Tabela="1C:…"`). Bază dedicată profil Privat, migrată+seed-uită de
unealtă (calea ModelCheck.Privat). Scop = reconciliere-harness, nu go-live
(12/18 neatinse). ~130k documente — rulare lungă cu resume.
(c) **Patru tipuri noi de PRODUS + unul de mecanism**: `NotaContabila`
(NTC — importul de note din decizia 9, linii `ILinieCuPostareExplicita`,
fără stoc/contare, și tip de culegere manuală); `InchidereTva` (ITV) —
draft generat de `InchidereTvaService.Genereaza(perioada)` (precedent
DescarcareService 37b) cu 4427=4426 + excedent→4423/4424, conturile în
`PoliticaInchidereTva` per profil (bugetar = tip inert); `Asamblare`
(ASM) — kitting n→m pe marfă, `Directie` pe detaliu (28a), consum din
loturi existente + produs cu lot nou și valoare explicită, invariant
Σ produse = Σ consumuri, FĂRĂ contare la marfă→marfă (371=371 = zgomot,
raționamentul 23c); **BPR rămâne rezervat** (19 neatinsă — semantica de
producție cu rețetar nu se forțează); `ReturFurnizor`/`ReturClient`
(RLF/RDC) — cerință de produs: storno achiziție/vânzare pe LOTUL ORIGINAL
(RLF: stoc −1 + 401=3xx + TVA deductibilă stornată; RDC: +1 pe lotul
existent + venit storno + costul revine); reprezentarea storno și
spargerea venit/cost = spike în felia de implementare.
(d) **Maparea**: FCT+NIR conex, FCL+DSC (descărcarea NU se re-pichează:
liniile DSC din rândurile 607=371 ale 1C cu lotul ca PIN — 37d), BTR, BCS,
LDI±, PLT/INC (extrasul per rând; stingerile din subconto → Imperechere),
Compensare → NTC+Imperechere, retail (RaportVanzariAmanunt) și avizele =
surogate FCL+DSC / DSC+NTC; Operatia/Salarii/MF/închiderea 121 → NTC;
rândurile de TVA ale închiderii 1C se SAR — le generează ITV, reconcilierea
le compară (forcing function-ul TVA-ului structural P1). Regula: stocul nu
se mișcă fără registru de stoc.
(e) **Contractul de reconciliere trăiește în conector** (34f; ModelCheck
neatins): (1) SOLD per cont sintetic OMFP per lună = Balanta 1C normalizată
(rulajele doar informativ — reprezentarea storno 1C cu minus pe aceeași
latură le face nereconciliabile structural); (2) 4423/4424 per lună
generate de ITV = sumele 1C; (3) stoc per produs×gestiune lunar
(cantitate+valoare) = BalantaNivel3 agregat (per-LOT nu e țintă — loturile
negative); (4) 891→0. Toleranță 0.005; diferențele sursei se raportează.
(f) **Conectorul = `nou/tools/Import1C`** (consolă ca Migrare, `FlaxDb`
pe view-uri, SqlClient raw); refolosește pattern-urile Migrare prin
copiere/adaptare, FĂRĂ bibliotecă comună prematură; diferența de fond:
apelează MotorOperare (precedent ModelCheck pe provider standalone).
Profilul privat se completează PE PARCURS (TipTva istoric 19%, Clasă/Tip
marfă, partener retail generic, politicile tipurilor noi) — fiecare gaură
= decizie explicită (21/35b), nu transcriere din 1C.
