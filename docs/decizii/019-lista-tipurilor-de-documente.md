# Decizia 19 — Lista țintă de tipuri de documente

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §19

---

**Lista țintă de tipuri de documente (10 derivate):** FacturaIntrare,
FacturaIesire (clase separate, nu comasate), NIR, BonConsum, NotaTransfer,
RaportProductie (n materii prime → m produse; loturile produse se
evaluează prin alocarea valorii consumurilor — istoric Otelinox/PPUP:
preț produs manual din rețetar cu chei de distribuție; designul BPR se
AMÂNĂ până la modulul de rețetar, clasa rămâne rezervată în ierarhie),
ListaDiferenteInventar, Decont, Plata, Incasare.
Proforma (FPR) exclusă. BF (bon fiscal / avans) = variantă de
FacturaIntrare (același flux, conex NIR), NU tip separat (confirmat).
