# Inventar legacy — BPR (Raport de producție) → `RaportProductie` [AMÂNAT]

Identitate legacy: tip 19, cod `BPR`; combo defa 73 (intern → intern),
dezactivat (`STARE=0` pe tip), zero documente. Rama: direcție, nu canon
(decizia 21).

## Statut (confirmat proprietar, decizia 19 amendată)

Folosit istoric la Otelinox + modulul PPUP. Semantica: intră n materii prime,
ies 1–m produse; **valoarea produselor rezultate era introdusă MANUAL ca preț
de produs, din rețetar (cu chei de distribuție)** — nu exista alocare automată
a costului consumurilor în afara rețetarului.

**Designul se amână până la modulul de rețetar.** În faza 1:

- Clasa `RaportProductie` rămâne rezervată în ierarhie (slot, fără
  implementare completă).
- Modelul de registre trebuie doar să NU blocheze pattern-ul: un document cu
  linii de consum (−1, loturi existente) și linii de produs (+1, **creează
  loturi cu preț dat**) — pattern deja acoperit de LDI (plusul creează lot cu
  preț cules), deci nicio cerință structurală nouă.

## Reguli de stoc existente (defa 73, doar ca direcție)

−1 predator pe stoc magazie (consum materii prime); intrarea produselor era
configurată prin tipul de stoc 7 („Stoc marfă neterminată") pe alte combo-uri.

## De reținut la reluare

- Cheile de distribuție din rețetar = politica de alocare n→m (rămân date).
- `GEST_DEFA_ITEMSI_TIP_MATERIAL` (487 rânduri) filtra ce materiale pot apărea
  pe fiecare combo — relevant pentru separarea liniilor de consum vs produs.
