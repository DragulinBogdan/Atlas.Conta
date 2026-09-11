# Decizia 19 — Lista țintă de tipuri de documente

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă

## Regula durabilă

**Tipurile de document = ierarhia de clase**: FacturaIntrare, FacturaIesire,
NIR, BonConsum, NotaTransfer, ListaDiferenteInventar, Decont, Plata,
Incasare (+ DescarcareGestiune 37; NotaContabila, InchidereTva, Asamblare,
ReturFurnizor, ReturClient 46). **RaportProductie rămâne REZERVAT**
(designul BPR se amână până la modulul de rețetar). Proforma exclusă; BF
(bon fiscal/avans) = variantă de FacturaIntrare, nu tip separat.

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
