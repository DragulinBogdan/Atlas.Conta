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

Rândurile se scriu la pasul 2 al contractului, odată cu `op1`, `op11`,
rezumatele și plaja de facturi (E2-D5…D7).

## Validare

Pasul 1, 2026-10-15: `verifica.ps1 -Suita Scenarii -Tip DECL -Profil Ambele
-Sufix .ClaudeCVL` verde (`run-verificari/20261015-163901-958`). DUKIntegrator
cu D300Validator J12.0.1 acceptă fișierele SC-DECL-01, 03 și 04 fără
atenționări și respinge mutanții SC-DECL-05 pe regula așteptată.
