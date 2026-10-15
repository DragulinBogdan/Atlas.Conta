# DECL — declarațiile D300 și D394 în XML

- Tip: catalog
- Actualizat: 2026-10-15

Catalogul fixează fișierele de depus ale etapei E2, conform
[contractului E2](../../api/e2-depunere-xml-contract.md). Proba este
`ScenariiDecl` (`--scenarii DECL`), pe anul 2043. Fiecare fișier generat
trece întâi prin XSD-ul oficial, apoi prin DUKIntegrator cu validatorul
Soft J pin-uit (E2-D9). Un refuz așteptat e rezultat al scenariului: fișierul
lipsește, iar proiecția JSON poartă aceleași coduri în `RefuzuriXml`.

## Convenții și fixture

Societatea de probă: CUI 4221306, CAEN 4690, declarantul Popescu Ion,
administrator, cont la „Banca Test”, periodicitate lunară; scena restaurează
societatea la final. Furnizorul și clientul sunt înregistrați în scopuri de
TVA în România. Sumele din fișier sunt lei întregi, rotunjite pe rândul de
operațiuni, cu totalurile recalculate din rândurile rotunjite (E2-D3).

## D300

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-DECL-01 | Ianuarie, privat: FCT N21 100,40 (TVA 21,08), FCT TI21 300, FCL N21 1.000,50 (TVA 210,11), FCL N11 200. Proiecția în bani păstrează rd. 9 = 1.000,50 / 210,11. Fișierul: rd. 9 = 1.001 / 210, rd. 10 = 200 / 22, rd. 12.1 = 300 / 63, rd. 24 = 100 / 21, rd. 26.1 = 300 / 63; rd. 19 = 295, rd. 30 = rd. 31 = rd. 35 = 84, rd. 37 = rd. 40 = rd. 44 = 211, rd. 45 = 0; `bifa_interne` 1. Numărul atributelor R și suma lor sunt cele ale proiecției în lei; `totalPlata_A` e suma lor. XSD și DUK fără erori. | acceptat |
| SC-DECL-02 | Refuzuri, fără fișier și cu aceleași coduri în `RefuzuriXml`: CAEN lipsă → `DECLARATIE_ANTET_INCOMPLET`; societate trimestrială cu o lună cerută, perioadă de 15 zile → `DECLARATIE_PERIOADA_NESUPORTATA`; decembrie 2025 → același cod, formularul v12; februarie, privat, FCT N9 50 operată (fără rând în formular) → `DECLARATIE_DATE_NEINCLUSE`, cu tipul numit. | refuzat |
| SC-DECL-03 | Luna fără operațiuni (martie, ambele profiluri): fișier valid, `tip_decont` L, `luna` 3, `bifa_interne` 1, totalurile și `totalPlata_A` zero; `nr_evid` = 10 · 301 · 01 · 03/43 · scadența 25.04.43 · 0000 · cifra de control. Februarie, după stornarea FCT N9 în aceeași lună: nemapatul net zero nu mai refuză, fișierul e valid. | acceptat |
| SC-DECL-04 | Societate trimestrială, trimestrul I: `tip_decont` T, `luna` 3, rd. 9 = 1.001 din ianuarie, `nr_evid` pe obligația 302; XSD și DUK fără erori. | acceptat |
| SC-DECL-05 | Mutanți pe fișierul din SC-DECL-01: rd. 9 TVA 211 fără total → DUK (R66 pe rd. 19, R26 pe suma de control); CUI 4221307 → DUK („CUI invalid”); `tip_decont` X → XSD. | refuzat |
| SC-DECL-06 | Reexportul aceleiași perioade, cu aceleași date și același antet, e identic byte cu byte (ambele profiluri). | acceptat |

## D394

Plaja de probă: seria DCLT, 1–1.000, decizia la 01.01. Facturile emise
primesc numerele DCLT101, DCLT102…

| ID | Pași și așteptări numerice | Rezultat |
|---|---|---|
| SC-DECL-07 | Aprilie, privat: FCL N21 1.000,50 (DCLT101), FCL N11 200 (DCLT102), FCL TI21 500 pe un produs „deșeuri” (DCLT103), FCT N21 100,40, FCT TI21 300 pe un produs „deșeuri”. `op1`: L 21 = 1/1.001/210, L 11 = 1/200/22, V 0 = 1/500 cu `op11` 22 = 1/500, A 21 = 1/100/21, C 21 = 1/300/63 cu `op11` 22 = 1/300/63. `rezumat2`: cota 21 = L 1/1.001/210 și A+C 2/400/84; cota 11 = L 1/200/22. `rezumat1` are detaliul bunului 22 pe V (cota 0) și pe C (cota 21). `serieFacturi`: tip 1 DCLT 1–1.000, tip 2 DCLT 101–103; `nrFacturi` 3, `nrCui1` 2, `totalPlata_A` = 2 + 1.401 + 200 = 1.603. XSD v5 și DUK J8.0.2 fără erori și fără atenționări; reexportul e identic. | acceptat |
| SC-DECL-08 | Refuzuri D394, fără fișier: telefonul societății lipsă → `DECLARATIE_ANTET_INCOMPLET`; mai, FCL DCLT2000 (în afara plajei) → `DECLARATIE_DATE_NEINCLUSE` cu numărul facturii; iunie, FCL TI21 pe un produs fără categorie → `DECLARATIE_DATE_NEINCLUSE` cu `FaraOp11`. | refuzat |
| SC-DECL-09 | Luna fără operațiuni (martie, ambele profiluri): `op_efectuate` 0, `tip_D394` L, `luna` 3, `totalPlata_A` 0. Societate trimestrială, trimestrul III cu o FCL în iulie: `tip_D394` T, `luna` 9, `op_efectuate` 1. XSD și DUK fără erori. | acceptat |
| SC-DECL-10 | Mutanți pe fișierul din SC-DECL-07: baza `op1` L 21 + 1 fără rezumat → DUK; `totalPlata_A` + 1 → DUK; `codPR` 99 → XSD. | refuzat |

## Validare

Pasul 2, 2026-10-15: SC-DECL-07…10 verzi pe privat, SC-DECL-08 (antetul) și
09 (luna goală) și pe bugetar; DUK D394Validator J8.0.2 acceptă fișierele
fără atenționări. Validatorul cere `baza_incasari_i1`/`i2` și TVA-ul lor pe
fiecare `rezumat2` cu cotă diferită de 24; ies cu zero, fiindcă modelul nu
are încasări AMEF.

Pasul 1, 2026-10-15: `verifica.ps1 -Suita Scenarii -Tip DECL -Profil Ambele
-Sufix .ClaudeCVL` verde (`run-verificari/20261015-163901-958`). DUKIntegrator
cu D300Validator J12.0.1 acceptă fișierele SC-DECL-01, 03 și 04 fără
atenționări și respinge mutanții SC-DECL-05 pe regula așteptată.
