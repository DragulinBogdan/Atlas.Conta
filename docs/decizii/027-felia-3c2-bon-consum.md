# Decizia 27 — Felia 3c-2 — BonConsum

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §27

---

**Felia 3c-2 — BonConsum — executată; validată e2e în ModelCheck (sold
deschidere → operare → gardieni → anulare directă → storno). Tranșările:**
(a) **Consumul alimentează DOUĂ registre simultan** (inventar 03, defa 65):
RegulaStoc −1 Magazie pe predator + +1 Consum pe primitor (generice,
Natura=Stoc) — consumul nu „dispare", rămâne pe responsabilul locului de
consum (obiecte în folosință / responsabilități).
(b) **Locul de consum = calitatea transversală `LocConsum`** (decizia 16),
nu clasă derivată: ValideazaOperare cere predator Gestiune și primitor
intern purtător al flag-ului; seed-ul pune flag-ul pe SEDIU (cu `|=`,
idempotent). Lotul NU se leagă de gestiunea predatoare prin validare —
locația curentă e soldul din registru, gardianul de sold refuză consumul
de unde lotul nu există (așa rămâne permis consumul loturilor transferate).
(c) **Contarea 6xx = 3xx per Clasă/Tip se DERIVĂ la seed** (echivalentul
curat al celor 18 modele legacy): rând RegulaContare per TipMaterial cu
Natura=Stoc și simbol 3xx — debit Explicit = simbolul cu prima cifră 3→6
(301→601, 302.01→602.01, 303.01→603) cu aceeași tăiere de segmente ca la
ContImplicit; credit = SursaCont.TipMaterial (contul de stoc al Tipului).
Seed incremental: tipurile fără rând primesc regulă la fiecare updater;
tipurile non-3xx (532/409 — bonuri valorice) nu contează pe BCS — rând
manual dacă apare nevoia reală (decizia 21).
(d) Valoare = preț lot × cantitate în PregatesteOperare (ca BTR — prețul
nu se culege); BCS e frunză în graful de dependențe ⇒ corecția directă
aproape întotdeauna permisă (03), reconfirmată de gardienii generici.
