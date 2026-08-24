# Decizia 30 — Felia 3c-4 — FacturaIesire

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §30

---

**Felia 3c-4 — FacturaIesire — executată; validată e2e în ModelCheck
(laturi → refuz stoc → operare cu scadență default → debit particularizat
→ anulare directă → storno). Tranșările:**
(a) **FCL = pur creanță (411 = 7xx), NICIO regulă de stoc** (inventar 07:
în acest profil facturarea nu descarcă gestiune). Liniile cu Natura=Stoc
se REFUZĂ la validare — vânzarea de bunuri din stoc = document/reguli
proprii când apare nevoia; ca la FCT (29b), validarea hardcodată migrează
spre declarativ la 3d.
(b) **Contarea = un singur rând generic**: debit `SursaCont.PartenerPrimitor`
(ContImplicit al clientului — ex. 461 debitori; fallback 411.01.01),
credit `SursaCont.TipMaterial` FĂRĂ fallback (Tip fără cont = eroare clară
la operare). Contul de venit „ales pe linie" din legacy devine alegerea
Tipului: clasă nouă **VEN** (Natura=Serviciu) cu tipuri din planul bugetar
— 751.01.00 (prestări servicii), 750.02.00 (chirii), 751.04.00 (diverse);
la privat ar fi 704/706/708 — seed per profil (decizia 29).
(c) **`PoliticaScadenta` (tabelă nouă: TipDocumentId + ZileDefault)** —
formula de header legacy `DATA_SCADENTA = data+30` devine politică-date;
motorul o aplică generic pe `IDocumentCuScadenta` la operare, DOAR dacă
scadența nu a fost culeasă. FCT nu are rând seed (scadența furnizorului
se culege) — neatins.
(d) **Numerotare proprie `FCL-`** (serie fiscală — invers față de FCT).
TVA rămâne în `Valoare` (o singură valoare de postare — testul bazei 22a);
defalcarea 4427 TVA colectată = adăugare ulterioară aditivă (deployment
neplătitor, cota default 0). Dimensiunile urmează convenția 00 §5
nemodificată (debit←emitent, credit←client). Fără cerință de clasificație
bugetară (veniturile sunt exceptate de la angajament — 00 §10).
(e) **Fix seed fresh-install**: derivările care interoghează baza
(ContImplicit din simbol, 6xx=3xx) nu vedeau nomenclatoarele create în
același run — `Seed()` comite nomenclatoarele înainte de derivări.
