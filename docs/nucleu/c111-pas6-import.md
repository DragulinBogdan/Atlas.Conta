# C111, pasul 6 — importul pe forma nouă

**Data:** 2026-10-10. **Stare:** importul și comparațiile lui sunt făcute;
`ScaraVolum.sql` și PerfCub sunt în
[`c111-pas6-scriere-si-volum.md`](c111-pas6-scriere-si-volum.md). Contractul:
[C111](c111-coordonate-contract.md), D9 și regulile de oprire 1, 2 și 7.

Urmele, neversionate: `run-verificari/c111-pas6/` (`import.C111P6/`,
`amprente/`, `saft/`, `fisa/`, cu rețetele `import.ps1` și `fisa-ab.py`).

## Rezumat

| Ce | Cifra |
|---|---|
| Import Flax, ianuarie 2025, pe forma nouă | 8 min 17 s; 16.597 documente, 18.770 tranzacții, 81.774 postări, 2.152 împerecheri, ca la pasul 0 |
| Reconcilierea importului | 10 verificări picate, ca la pasul 0; raportul sortat diferă numai la data din antet |
| `INV-CUB` pe baza importată | verde, după deschidere și pe baza integrală |
| Amprentele pasului 0 (balanța, stocul, partidele, D300, D394) | identice, octet cu octet |
| Amprenta analitică pe repartitor și pe contrapondere | identice cu așteptările pasului 1b |
| Amprenta analitică fiscală | 55 de rânduri diferite, toate din locul fiscal; fără loc în cheie, identică |
| Raportul SAF-T S | identic, în afara datei, a căilor și a timpilor |
| Fișa contului 401 pe ianuarie, înainte și prin pereche | 21.875 ms și 1.520 ms |

Binarul e Release, de la `b0e5343`, pe `Atlas.Conta.Import1C.Flax.C111P6`
(5446). Importul își raportează singur durata: 8 min 17 s acum, 7 min 27 s
la pasul 0, cu aceleași 469 de avertismente. Diferența de 50 s e între două
rulări unice, pe baze diferite, și nu e o măsurătoare a costului de scriere;
acela se ia cu PerfCub.

## Regula de oprire 1

Cele cinci secțiuni ale pasului 0 au același SHA-256 și același număr de
rânduri. Raportul de reconciliere sortat are două linii diferite, antetul cu
data rulării. Raportul SAF-T S are opt linii diferite: antetul, comanda
validatorului cu calea fișierului, timpii și calea fișierului.

## Regula de oprire 2

`analitic-repartitor` (6.289 de rânduri) și `analitic-contrapondere` (6) sunt
identice cu fișierele derivate la pasul 1b. S7 și S8 nu produc nicio
diferență pe această bază.

`analitic-fiscal` diferă pe 55 de rânduri, cum anunța pasul 1b: 25 de
rânduri de autocolectare, cu 722 de postări, stăteau pe locul „-” și sunt
acum pe 30 de rânduri cu loc numit. Pe forma nouă nicio postare cu fapt
fiscal nu mai e fără loc. Cu locul scos din cheie, cele două fișiere dau
aceleași sume și aceleași numărători pe cont × partener fiscal × rol × lună.

Baza n-are decont și folosește cele șase politici de TVA din seed, deci S10,
S12 și S13 nu apar, cum spunea pasul 1b.

## Cifra de timp a fișei (S9)

`ModelCheck --perf-fisa <bază> <simbol> <an> <lună> <director>` rulează
fișa produsului de cinci ori la cald și scrie SQL-ul ei. Pe contul 401, în
ianuarie:

| Forma | Rânduri | Cu contrapartidă | Mediana, prin produs |
|---|---|---|---|
| contul întreg | 7.012 | 5.676 | 1.955 ms |
| cont × repartitorul cel mai încărcat | 1.113 | 1.037 | 191 ms |

Comparația înainte și după e pe aceeași bază, cu SQL-ul produsului:
forma dinainte se obține din el punând la loc subinterogările pe sensul opus
al tranzacției (`fisa-ab.py`). Cinci rulări alternate, mediana, timpul de
execuție din `EXPLAIN ANALYZE`:

| Forma | Înainte | Prin pereche |
|---|---|---|
| contul întreg | 21.875 ms | 1.520 ms |
| cont × repartitor | 1.051 ms | 137 ms |

Pasul 0 măsurase SQL scris de mână pe baza de atunci: 21.152 și 2.342 ms,
respectiv 1.291 și 156 ms. Rândurile fără contrapartidă sunt cele fără
pereche, în principal deschiderea.

## Restul pasului 6

`ScaraVolum.sql`, PerfCub și costul scrierii cu cele două chei străine noi:
[`c111-pas6-scriere-si-volum.md`](c111-pas6-scriere-si-volum.md)
(2026-10-11). Sursa perechii din fișa contului s-a schimbat acolo; cifrele de
timp de mai sus sunt de dinaintea schimbării.
