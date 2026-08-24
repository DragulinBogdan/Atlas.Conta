# Decizia 37 — Design P2 — descărcarea de gestiune la FacturaIesire

- **Data**: 2026-07-23 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §37
- **Docs**: docs/privat/p2-descarcare-design.md

---

**Design P2 — descărcarea de gestiune la FacturaIesire — FIXAT**
(`docs/privat/p2-descarcare-design.md`, toate cele 6 tranșări confirmate
23.07.2026; implementarea urmează). Fluxul-ancoră acoperit (nu copiat —
decizia 21): magazin online — FCL dictată de site (preț decuplat de cost,
marjă posibil negativă), poziții fără stoc la facturare (venit acum, cost
la disponibilitate), identificare cu prioritate pe lot. Tranșările:
(a) **Tip nou `DescarcareGestiune` (DSC, al 11-lea derivat)**; detaliu
derivat cu `LinieSursaId?` (FK real spre DocumentDetaliu — acoperirea per
linie FCL). Laturi gestiune→client, AMBELE dimensiuni pe gestiune
(`RepartitorImplicitCredit()`→Predator, ca Decont 32c). Valoare = cost
(preț lot × cantitate, pattern BTR/BCS); fără TVA pe DSC (4427 rămâne
integral pe FCL). Stoc −1 predator (Magazie/MF→Marfuri); contare
`SeedContare6xxDin3xx` cu excepțiile profilului (607=371, 711=345).
Reutilizarea BonConsum respinsă (+Consum pe primitor ≠ ieșire din
patrimoniu). Ancora TipDocument DSC în nucleu; la bugetar fără politici —
tip inert.
(b) **Spargerea pe loturi la GENERARE** (draftul concret e condiția
override-ului manual — decizia 13; operarea nu creează linii nicăieri în
motor); `AlocaFifo` variantă TOLERANTĂ (alocă disponibilul, întoarce
restul); gardianul de sold rămâne autoritatea la operare (alocarea
învechită refuză zgomotos). TipStoc-ul căutării se citește din RegulaStoc
al DSC-ului (fără hardcode).
(c) **Generator = `DescarcareService` în motor, NU clona PoliticaConex**
(valorile diferă, liniile se sparg 1→N, predatorul se înlocuiește):
declanșat prin `GenereazaSecundar` pe FCL (precedent 31e) + acțiune
„Generează descărcarea" pe FCL Operat pentru REST (backorder, după
recepția FCT→NIR; data se culege, default azi). Fără tabelă
PoliticaDescarcare (nimic de configurat; aditivă la nevoie). Restul
neacoperit NU intră pe DSC — se raportează și rămâne interogabil per
linie (cusătura fluxului de comenzi, pasul 5). Acoperirea = Σ linii DSC
Draft+Operat pe LinieSursaId (draftul contează — anti-dublare; Stornat
nu). Gardienii de grup existenți acoperă tot (draft șters la anulare,
operat blochează).
(d) **Culegerea FCL — General! + Specific?**: `ProdusId?` pe derivată,
OBLIGATORIU prin validare pe liniile de stoc (identitatea liniei =
produsul; schema rămâne nullable — aceeași derivată poartă serviciile);
`LotId` de bază = rafinarea specifică OPȚIONALĂ, validată ca aparținând
produsului, prioritară la picking și FĂRĂ fallback FIFO pe restul ei
(pinul e intenția magazinului; deblocarea = scoaterea pinului).
`GestiuneDescarcareId?` pe header — o gestiune per factură la P2; lot
explicit fără sold în ea = refuz (întâi BTR).
(e) **Derivarea de VÂNZARE pe FCL** (regula generică ar posta 4111=371):
rând RegulaContare per TipMaterial de stoc — debit RepartitorPrimitor
(fallback 4111), credit Explicit din mapă 371→707, 345→701, 381→708,
fallback 708; incremental la updater, editabil ca date. Seed-ul privat
ȘTERGE rândul NaturaInterzisa=Stoc de pe FCL (pas explicit — există în
bazele P1); la bugetar RĂMÂNE (30a) — diferența de profil e în date,
hook-ul no-op natural.
(f) **Datoriile P1 intră în felie**: `TipTvaImplicitId?` pe TipDocument
(default de CULEGERE, nu de motor — un rând PoliticaTva doar-pentru-
default ar activa pasul TVA; seed N21 privat / CAP21 bugetar pe
FCT/FCL/DEC) + verificarea culegerii TipTva/ValoareTva și a noilor câmpuri
în UI XAF (smoke test manual în contractul feliei).
(g) Amânate, documentate (design §10): comenzile (sales order/PO) și
importul FCL din site (pasul 5), regenerarea automată la recepția NIR,
multi-gestiune per factură, amănuntul la preț de vânzare (371/378/4428 —
non-goal: evaluarea rămâne identificare specifică la cost net), rezervarea
de stoc (gardianul de sold ajunge single-operator).
