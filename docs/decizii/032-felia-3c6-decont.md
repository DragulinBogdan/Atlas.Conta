# Decizia 32 — Felia 3c-6 — Decont

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §32

---

**Felia 3c-6 — Decont — executată; validată e2e în ModelCheck (avans →
decont cu postare explicită pe linie → imperecherea lanțului
avans↔decont↔regularizare → gardieni → anulare → storno). Tranșările:**
(a) **Postarea explicită pe linie = contract de interfață
`ILinieCuPostareExplicita`** (ContDebit/ContCredit/RepartitorDebit/
RepartitorCredit, toate opționale) — trăsătura PROPRIE a tipului (inventar
06, nuanța deciziei 15): motorul o consultă înaintea rezolvării declarative
(contul liniei bate SursaCont) și ca nivel maxim al coalesce-ului de
dimensiuni (repartitorul per latură), dar NUMAI tipurile care declară
interfața o au — nu devine mecanism generic (baza DocumentDetaliu nu
poartă câmpurile; azi doar DecontDetaliu).
(b) **Laturi: predator = Angajat (titular), primitor = unitate internă**
(UnitateInterna/Gestiune); fără reguli de stoc. Contare: un rând generic —
debit `SursaCont.TipMaterial` FĂRĂ fallback (Tip fără cont + linie fără
explicit = eroare clară la operare), credit `RepartitorPredator` cu
fallback 542.01.00 (acoperă angajatul fără ContImplicit).
(c) **Convenția 00 §5 devine default POLIMORF**:
`Document.RepartitorImplicitDebit()/Credit()` (debit←Predator,
credit←Primitor) — ultimul nivel de coalesce în motor; Decont mută
creditul pe TITULAR (soldul avansurilor 542 se ține per angajat, nu per
primitorul justificării).
(d) Cantitatea e pro-formă (legacy BUC/1): `PregatesteOperare` o
normalizează 0→1 și materializează Valoare = PretUnitar × Cantitate ×
(1+TVA). Clasificația bugetară per linie ca la FCT — hardcodată până la
validarea declarativă (3d, ca 29b). Lanțul avans→decont→regularizare =
imperecheri (31d), verificat e2e cu avansul stând pe ambele roluri.
Fără migrație: schema DecontDetaliu există din 3a.
