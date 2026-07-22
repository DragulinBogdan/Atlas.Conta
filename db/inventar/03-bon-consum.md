# Inventar legacy — BCS (Bon de consum) → `BonConsum`

Identitate legacy: tip 17, cod `BCS`; combo activ defa 65 (intern → intern,
gestiune → loc de consum). Fără date în 2026 (config activ; folosit istoric).
Rama: direcție, nu canon (decizia 21).

## Funcționalitate

Consumul scoate din magazie și înregistrează cheltuiala pe locul de consum,
cu descărcare pe loturi (codmat).

## Reguli de stoc (defa 65) — dublu-fațetat

| Latura | Tip stoc | Semn |
|---|---|---|
| predator (gestiunea) | 1 — Stoc magazie | **−1** |
| primitor (locul de consum) | 2 — Stoc consum | **+1** |

Consumul nu „dispare": se mută într-un registru de consum pe primitor —
util pentru obiecte date în folosință / responsabilități. Pattern de reținut
pentru registrele noi: un document poate alimenta DOUĂ registre simultan.

## Contare

18 modele (defa 65): `6xx per Clasă/Tip = 3xx corespondent` (601=301,
6021=3021, …, 602.09=302.09), valoare `VALOARE_RECEPTIE_TVA` (= valoarea de
lot, cu TVA — vezi 00 §4), repartitori = predator/primitor, dimensiuni de pe
linie. Fără override-uri de cont vizibile (contare integral din politică).

## Câmpuri vii pe poziție (defa 65)

Selecție lot/sumator (`ID_GEST_SUMATOR` vizibil — consumul alege pe sumator și
sistemul sparge pe loturi, sau direct pe codmat după `tipStock`), Clasă/Tip,
UM, `CANTITATE`, `PRET_UNITAR` (readonly de facto — prețul lotului),
`VALOARE_RECEPTIE_TVA` calculat, dimensiuni bugetare
(`ID_ANGAJAMENTE_DEFALCARE`, `COD_FUNCTIONAL`, `COD_ECONOMIC`).
Zgomot de config confirmat: `CANTITATE_SUPLIMENTARA` și
`VALOARE_LIVRARE_VALUTA` marcate REQUIRED dar invizibile — se ignoră.

Header viu: doar `NR_NOTA`/`DATA_NOTA` peste structural.

## Validare funcțională esențială

Stoc insuficient pe lot: `TCVUnit` verifică `STOCK_BEFORE`/`STOCK_AFTER` și
`AcceptStockNegativ` (parametru) — în modelul nou: verificarea de sold pe lot
la operare + regula soldului intermediar la corecții (decizia 14).

## Mapare spre modelul nou

- **`BonConsum`**: header slim (gestiune → loc de consum/centru de cost);
  detaliu = referință `Lot` + cantitate; valoarea vine din lot (nu se culege).
- Consumul e **frunză** în graful de dependențe (nimic nu depinde de el) ⇒
  corecție directă aproape întotdeauna permisă (00 §8).
- Politici seed: regulile celor două registre (−magazie/+consum) + maparea
  6xx/3xx per Clasă/Tip — de definit curat pe planul de conturi actual.
