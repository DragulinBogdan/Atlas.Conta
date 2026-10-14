# Scopul Atlas.Conta

**Actualizat: 2026-10-14.**

Țintele și non-țintele de mai jos au fost propuse de agent și acceptate de
owner la 2026-10-14 (kitul de guvernanță, §3.9.9). De acum, o schimbare de
țintă, non-țintă sau structură se face prin decizie (§3.9.3).

## Problema și utilizatorii

Contabilul și gestionarul unei societăți private din România țin
contabilitatea și gestiunea pe OMFP 1802 și depun declarațiile fiscale
lunare. Aplicația veche (Delphi + SQL Server) le face cu un model generic
de documente configurat prin tabele, scump de întreținut. Atlas.Conta o
rescrie: privat-first, cu profilul bugetar ca pachet de seed peste același
motor (35).

## Ținte

- **T1 — Societatea privată își ține contabilitatea și gestiunea curentă în
  produs.** Documentele lunii se culeg, se operează și se corectează, iar
  luna se închide cu balanța echilibrată. Reușita: fiecare tip de document
  al profilului privat are ciclul obligatoriu din
  [catalogul de scenarii](nucleu/scenarii/README.md) verificat, iar o lună
  de probă se închide fără constatări blocante.
- **T2 — Declarațiile fiscale lunare ies din aceleași postări ca balanța,
  fără ajustări de mână.** Reușita: decontul și D300, D394 și SAF-T (L și S)
  trec validarea ANAF pe lunile de probă, iar SAF-T se reconciliază cu
  balanța pe aceeași lună (SC-SAFT-34, SC-SAFT-51).
- **T3 — Orice cifră se explică până la document și la regula care a
  decis-o.** Reușita: din balanță se coboară prin fișă și jurnal la
  postare, document și rândul de politică, fără reexecutarea politicii
  (SC-CIT-96…99).
- **T4 — Un client existent trece pe produs cu soldurile și partidele
  deschise, iar diferențele față de sursa veche se raportează.** Reușita:
  deschiderea pe lot și pe partidă reface balanța de la data trecerii
  (SC-DES-01…09), iar importul unei luni reale se reconciliază fără
  diferențe nedeclarate.
- **T5 — Profilul bugetar rulează pe același motor, ca pachet de seed.**
  Reușita: catalogul de scenarii e verde pe ambele profiluri, fără clasă de
  document și fără ramură de motor per profil.

## Non-ținte

- **NT1 — Migrarea istoricului de documente din legacy.** Istoricul rămâne
  în legacy; produsul primește soldurile de pornire (18).
- **NT2 — Culegerea documentelor în React.** Culegerea e în XAF; React face
  citiri și proiecții (104d).
- **NT3 — Tipuri de document descrise prin tabele de configurare.**
  Structura e cod, politica e date (4).
- **NT4 — Altă metodă de evaluare a stocului decât FIFO pe lot.** O singură
  metodă în motor (13, 51d).
- **NT5 — Mai mulți clienți în aceeași bază.** Baza aparține unui client
  (29).

Rândurile parcate din [catalog](functionalitati.md) care vin din limitele
curente (salariile, producția pe rețete, execuția bugetară completă,
valuta, comenzile de vânzare, e-Factura) rămân parcate; trecerea unuia
în non-ținte se face prin decizie.

## Structura

Ariile sunt paginile din `docs/docskit.json` (`arii`); descrierea a ce
există e în [starea curentă](stare-curenta/README.md).

| Arie | Responsabilitatea | Ținte | Stare |
|---|---|---|---|
| `domeniu-si-operare` | documentele, motorul de postare, stocul pe lot și partidele | T1, T4, T5 | există |
| `politici-si-fiscalitate` | configurarea profilului, TVA, închiderile, proiecțiile contabile, declarațiile și ANAF | T1, T2, T5 | există |
| `api-si-client` | ușile HTTP, securitatea, ecranele XAF și React, explicația deciziei | T1, T3 | există |
| `dezvoltare-si-validare` | persistența, seed-ul, verificările (ModelCheck, scenarii) și importul | T4, T5 | există |

## Etapa curentă

**E1 — Corecția valorii lotului.** Deschisă 2026-10-14, aliniată cu
contractul [TR-D9b-CV](nucleu/tr-d9b-corectii-valorice-contract.md).
Gata când: fiecare rând `E1` din catalog e `livrată`, adică TR-D9b-CV e
închis cu review-ul advers (pasul 5).
Funcționalitățile: rândurile `E1` din [catalog](functionalitati.md).
