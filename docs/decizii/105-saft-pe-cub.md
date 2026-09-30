# 105 — SAF-T D406 pe cub: sursă unică, acces complet, certificare cu manifest

Data: 2026-09-30
Stare: activă; amendează 073 (delimitarea exportului L) și 074 (cheia și categoria politicii de mișcare S)
Docs: `docs/nucleu/tr-d8-saft-contract.md` (SAF-B1…B8, S0–S3, B8-R1…R4); `docs/nucleu/scenarii/SAFT.md`; `docs/nucleu/tr-d8-saft-ab.md`

**Starea verificării B8, 2026-09-30:** toate constatările sunt rezolvate.
Codex a aplicat corecția RV1.2 pe clasificatorul istoric `3a4372d`:
martori pe sursa reală pentru ambele ramuri de factură, 21 de mutanți
respinși, aceleași 111 diferențe clasificate. Integrala: **3.269 / 4.521 OK**.
Corecția este reverificată de Claude și comisă pe `tr-d8-saft-ab-rv1`.
Regula durabilă rămâne activă.
[Corecția și dovezile](../nucleu/tr-d8-saft-b8-rv12-corectie.md).

## Regula durabilă

**(a) Cubul este singura sursă a fișierelor L și S.** GL-ul, facturile,
plățile, mișcările și pozițiile se citesc din postările cubului și din
faptele fiscale, prin cititorii comuni `Cub/Citiri`. Ruta pe registre a fost
scoasă, iar diferențele ei față de cub sunt clasificate o singură dată, cu
martor numeric (raportul A/B final). Probele scrise de mână sunt proba
supremă (91).

**(b) Fără acces complet nu există fișier (SAF-D4).** Orice criteriu de rând
sau de membru pe tipurile citite de o ușă dă 403 `SAFT_ACCES_INCOMPLET`
înaintea proiecției, fără sume. Lista tipurilor citite se probează contra
SQL-ului emis. Refuzurile de domeniu (`SaftDto.Refuzuri`) apar în sumar și
dau 422 pe fișier, înaintea primului octet. Sumele nu se ajustează și nu se
inventează valori: maparea lipsă, contul sau categoria lipsă, politica
lipsă, codul necunoscut, rolul fără partener și soldul negativ ori rezidual
sunt refuzuri, nu avertismente.

**(c) Identitate și timp (S1).** Tranzacția GL e tranzacția cubului; linia e
postarea. `SystemEntryDate` și `MovementPostingDate` vin din
`Tranzactie.ScrisLa`. Factura e evenimentul cubului: `InvoiceDate` este data
documentului-rădăcină, inclusiv pentru 381. Corecția unei luni raportate e
381 (storno) + 384 (reemisă) cu același număr (B'). Luna închisă se
reexportă identic.

**(d) Plata (S2).** Ținta liniei de plată se află la capătul lunii operării.
Stornoul neagă liniile operării, iar legătura de după luna plății nu apare în
nicio declarație. Compensarea prin notă nu este plată (SAFT-r1).

**(e) Stocul (S3, SAF-D3 = C).** Categoria stocului vine din contul istoric
al postării (`Cont.CategorieStoc`, moștenită), nu din tipul de material
curent. Codul mișcării vine din politica pe tip × categorie × semn. NIR delta
e codul 10 cu cantitate semnată. ASM raportează producția la valoarea finală
(ΣP + ΣΔ = ΣC). Cheile sunt: WarehouseID = codul gestiunii, `StockAccountNo`
= lotul, `MovementReference` lizibilă sau rezerva `{TranzactieId:N}{cod}`.

**(f) Citirea e mărginită de fereastra deschisă (88e).** Opening-ul S citește
snapshot-ul. Reconcilierea stoc ↔ sold pe cont se sparge în „(sold
inițial)”, din snapshot, și componentele lunii pe tip de document (B8-Q3).
Numărul comenzilor SQL nu depinde de volum. Excepția numită: balanța
(SAFT-r4).

**(g) Certificarea cere manifest.** XSD v249 și DUK J2.2.18 fixate prin
SHA-256, fără `SĂRIT` și cu perioada din antet. Manifestul leagă fiecare
linie de postările ei `(Spatiu, ID)` și fiecare poziție de sursele ei.
Mutanții sunt respinși.

**(h) Bugetarul e neaplicabil** (TR-r7): ambele uși refuză cu 422.

## Context

Felia TR-D8 SAF-T a rulat în subfeliile S0 (validarea), S1 (GL și facturi),
S2 (Payments, comutarea L) și S3 (MovementOfGoods, PhysicalStock, comutarea
C), apoi gate-ul final B8. Contractul feliei conține toate tranșările
owner-ului (SAF-D1…D4, S1-D5 = B', S2-Q1/Q2, S3-Q1/Q2, B8-Q1…Q3) și
review-urile Codex, fiecare închis. Decizia adună numai regula; derivările,
cifrele și probele rămân în contract.

## Ce rămâne deschis

- SAFT-r1: documentul de compensare;
- SAFT-r2: mișcările numai valorice și `Cauza` în cheia politicii;
- SAFT-r3: codurile de diagnostic fără producător;
- SAFT-r4: balanța din snapshot;
- SAFT-r5: `ANALYZE` după inserarea masivă (migrare, import);
- FZ-r3: planul pe volum al alocării Payments;
- F26-r8: Assets;
- FZ-r7: reperul `TaxInformation`;
- TR-r7: bugetarul;
- pragul de ~43 ms al cererilor de 5–40 KB aparține proxy-ului de porturi Docker Desktop al
  mașinii de dezvoltare, nu produsului (B8-RV3-P); gate-ul transversal de perf măsoară fără el.

TR-D8 nu se închide integral: reconcilierea și auditul transversal, bugetul
de perf și inventarul complet T-r11 își păstrează gate-urile.
