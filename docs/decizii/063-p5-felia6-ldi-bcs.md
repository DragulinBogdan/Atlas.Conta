# Decizia 63 — Pasul 5, felia 6 — LDI + BCS prin API și client

- **Data**: 2026-08-13 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §63
- **Docs**: docs/api/p5-felia-ldi-bcs-contract.md

---

**Pasul 5, felia 6 — LDI + BCS prin API și client — executată** (contract
+ închidere: `docs/api/p5-felia-ldi-bcs-contract.md`, F6-D1…D12;
flux-ancoră complet în browser pe baza Privat, review advers cu 2 defecte
de fond fixate). Închide restanța 53i pe LDI+ (culegerea produsului care
naște lotul de plus). Tranșările:
(a) **Modelul LDI**: `ProdusId` + `ILinieCareNasteLot` pe frunză, membru
NOU de contract `NasteLot` (default interface member `=> true`; LDI:
`Directie == Plus` — direcția nesetată nu inventează marfă), consumat de
`LoturiCulegereService` ca gard de direcție poziționat ÎNAINTEA gardului
de lot străin și a self-healing-ului (linia care refuză nașterea își
pierde lotul nefinalizat ȘI produsul — F6-M1, toate căile ajung la
aceeași stare); override `GestiuneLoturiCulese` → PREDATORUL (gestiunea
inventariată, 28d — primitorul e comisia, nu o `Gestiune`; fără override
serviciul tăcea); migrația `LdiCulegereLot` strict aditivă; `[Appearance]`
pe direcție + captions RO + `Valoare` read-only în baseline.
(b) **API**: `BonConsumApply` (șablonul BTR + `MaterializeazaValori`
geamănă — valoarea consumului vizibilă la culegere, lot scos ⇒ 0) și
`ListaDiferenteInventarApply` (șablonul NIR: frunză prin as-cast la
citire, `Directie` STRING cu parse pe nume — pe liniile EXISTENTE tipul
se judecă înaintea parse-ului (F6-M4); `LotId` aplicat DOAR pe Minus, pe
Plus ignorat — round-trip-ul ReadDto; câmpurile plusului GOLITE persistat
pe Minus; valoarea SEMNATĂ la culegere, Total = efect net); `Numar`
server-owned pe ambele; fără TVA (nu au PoliticaTva); affordances
NIR-style. 88 verificări ModelCheck noi pe suita bugetară.
(c) **Review advers — 2 defecte de FOND**: F1 — minusul putea descărca
lotul născut de linia-FRATE a ACELUIAȘI document la preț nefinalizat 0
(gardianul de sold trecea pe aceeași cheie/zi; valoare orfană pe
cantitate 0) ⇒ gardul ASM 46d replicat: minus pe lot creat de acest
document = refuz; F2 — LDI nu avea validarea de coerență Tip↔Produs deși
devenea intrare care naște lot (invariantul 50a rupt la naștere) ⇒
replicată de pe NIR. Limitări asumate: lotul FINALIZAT rămâne orfan după
comutarea operare→anulare→comutare (M3 — sold 0, inofensiv; curățenia
„fără urme" îl culege la ștergerea documentului); pinul XAF cules
înaintea direcției se scoate doar comutând dus-întors (M5, UX).
(d) **Client**: felia `bcs` = clona BTR cu etichete culese; felia `ldi` =
editor cu COMUTATOR de direcție (câmpurile direcției inactive NU se
randează; comutarea golește client-side câmpurile celeilalte direcții,
comparație PRE-update — inclusiv pinul, F6-M2); `etichetaLot`/`ziLocala`
extrase în `nucleu/lot.ts` (a 3-a/a 4-a utilizare; BTR+FCL migrate — BTR
își schimbă formatul etichetei); `CodEconomic` culegibil pe AMBELE
direcții (contarea minusului 6xx=3xx poate cere E la bugetar).
(e) **Smoke browser** (WebApi+client, baza Privat): BCS-548 operat
(5×0,65=3,25 la culegere; −Magazie gestiune + +Consum SEDIU; 6028=3028);
LDI-19 operat (lotul plusului născut la salvare „(în culegere)" în
gestiunea PREDATORULUI, finalizat de motor 0,70/data documentului;
minusul semnat; note 3028=7588 privat + 6028=3028 pozitiv normalizat);
gardianul de sold cu mesaj de domeniu prietenos pe lotul fără sold —
validarea empirică a F6-D8 (lookup nefiltrat, autoritatea = motorul).
Capcană de MEDIU: migrația se aplică și pe bazele de dev suplimentare
(`--connection ...Database=Atlas.Conta.BackOffice.Privat`) — altfel
orice query TPT pe detalii dă 500.
(f) Rămase: DEC = felie proprie (F6-D12: `Cont` în OData, lookup
`Angajament`, aderarea `ILinieCuPretUnitar`); filtrarea laturilor
interne pe `Calitati`; retrofit `MaterializeazaValori` pe BTR;
rezolvarea lazy a display-ului lookup-urilor pe linia existentă
(moștenit BTR) — itemi în contract §Închidere / lista-react.
