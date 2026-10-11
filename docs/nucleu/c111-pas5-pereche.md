# C111, pasul 5 — contrapartida prin pereche (S9)

- Tip: analiza
- Data: 2026-10-10
- Alimentează: 111 (raport de pas al contractului C111)

**Data:** 2026-10-10. **Stare:** implementat și verificat pe catalog și pe
HTTP; cifra de timp e luată la pasul 6 ([`c111-pas6-import.md`](c111-pas6-import.md)).
Sursa perechii s-a schimbat la pasul 6: `Contabil.Perechi`, mărginită de
interval, în locul lui `Contabil.Postari`
([`c111-pas6-scriere-si-volum.md`](c111-pas6-scriere-si-volum.md)), iar
îmbinarea perechii din registrul jurnal e scrisă în SQL de la pasul 7
([`c111-pas7-client-si-inchidere.md`](c111-pas7-client-si-inchidere.md)). Contractul:
[C111](c111-coordonate-contract.md), S9 și C111-Q2; decizia 111 (d). Închide
D9-r4.

Urmele, neversionate: `run-verificari/20261010-221107-389/` (integrala),
`run-verificari/c111-s9-http.py` (proba HTTP).

## Ce s-a schimbat

- **Fișa contului.** Contrapartida unui rând e contul celeilalte postări a
  perechii (`Postare.Pereche`), cu identificator și simbol. Înainte era lista
  conturilor de sens opus din toată tranzacția, cu identificator numai când
  lista avea un singur cont.
- **Registrul jurnal.** `JurnalRand` are `ContrapartidaId` și
  `ContrapartidaSimbol`, citite la fel.
- **Rândul fără pereche** (deschiderea) n-are contrapartidă.
- `PostareAnalitica` expune `Pereche`. Îmbinarea din registrul jurnal e scrisă
  fără ramură de nul (proba `RAMURA-NUL`).

## Verificarea

| Probă | Rezultat |
|---|---|
| Integrala ModelCheck, bugetar | 3.660 OK, 0 FAIL |
| Integrala ModelCheck, privat | 5.062 OK, 0 FAIL |
| Fișa contului 401, pe HTTP | 100 ↔ 371, 21 ↔ 4426 |
| Registrul jurnal, pe HTTP | fiecare rând cu contul din perechea lui |
| OpenAPI și tipurile clientului | regenerate: două câmpuri noi pe `JurnalRand` |

Proba HTTP: WebApi pe o bază privată din seed, cu o factură de intrare cu
recepție și TVA (371 = 401 de 100, 4426 = 401 de 21) și două note. Înainte de
S9 ambele rânduri ale fișei lui 401 ar fi arătat „371, 4426”, fără
identificator.

## Așteptările schimbate

SC-CIT-21: fișa stocului dă pe fiecare rând contul din perechea lui, în locul
listei comune; se adaugă aceeași probă pe registrul jurnal.

## Ce nu e probat

- **Cifra de timp.** Contractul o cere față de pasul 0, pe aceeași bază. Baza
  pasului 0 e pe schema veche, deci măsurarea cere un import pe forma nouă;
  se face la pasul 6. Pasul 0 a măsurat SQL scris de mână: pe Flax, contul
  întreg pe o lună, 21.152 ms pe forma veche și 2.342 ms prin pereche.
- Clientul React nu arată încă coloana nouă a registrului jurnal (pasul 7).
- Pe storno cu mai multe surse ordinalele sunt decalate; contrapartida
  inversei nu are o probă anume.
