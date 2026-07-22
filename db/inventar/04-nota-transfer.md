# Inventar legacy — BTR (Bon de transfer) → `NotaTransfer`

Identitate legacy: tip 9, cod `BTR`; combo activ defa 33 (intern → intern).
Fără date în 2026. Rama: direcție, nu canon (decizia 21).

## Funcționalitate

Mută stoc între două gestiuni interne, fără efect valoric de cheltuială:
**−1 predator, +1 primitor, ACELAȘI tip stoc (1 — magazie)**. Lotul își
schimbă gestiunea, prețul rămâne al lotului.

## Contare

1 singur rând în `GEST_DEFA_NOTA_CONT` (defa 33) — transfer 3xx=3xx pe
analitici de gestiune sau deloc, după politica contabilă. Funcțional: transferul
poate să nu genereze note (aceleași conturi sintetice) sau să miște doar
analiticul de repartitor — de definit curat la seed; în legacy defalcarea pe
gestiune se obținea din dimensiunea Repartitor a notei, nu din conturi diferite.

## Câmpuri vii pe poziție (defa 33)

Selecție lot (`CODMAT`/`ID_GEST_SUMATOR` vizibile), Clasă/Tip, UM, `CANTITATE`,
`STOCK_AFTER` (vizibil — feedback de sold la culegere), `PRET_UNITAR` (al
lotului), `VALOARE_RECEPTIE_TVA = PRET_RECEPTIE × CANTITATE` (formulă FĂRĂ TVA
aici — transferul nu re-aplică TVA), dimensiuni bugetare, `ContD`/`ContC`
vizibile (override disponibil, neobligatoriu).
Zgomot: `CANTITATE_SUPLIMENTARA`, `VALOARE_LIVRARE_VALUTA` REQUIRED+invizibile.

Header viu: `NR_NOTA`/`DATA_NOTA` + refolosirile `DESCRIERE_CURS`→NR_PV,
`DATA_EMITERE`→DATA_PV (același pattern PV ca la FCT).

## Mapare spre modelul nou

- **`NotaTransfer`**: gestiune sursă → gestiune destinație; detaliu = `Lot` +
  cantitate. Cel mai simplu tip din familie — candidatul ideal pentru primul
  vertical slice al motorului de registre (un registru, două rânduri ±).
- Dependență: transferul CREEAZĂ disponibilitate în gestiunea destinație ⇒
  consumurile ulterioare din destinație depind de el (graful pe loturi din
  decizia 14 acoperă natural cazul).
