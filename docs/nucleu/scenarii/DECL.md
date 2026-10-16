# DECL — declarațiile D300 și D394 în XML

- Tip: catalog
- Actualizat: 2026-10-16

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
| SC-DECL-11 | Octombrie: FCL N21 100 (DCLT108); noiembrie: FCL N21 50 (DCLT109) și stornoul facturii din octombrie. Fișierul pe noiembrie: `nrFacturi` 1, `serieFacturi` tip 2 = 109–109; XSD și DUK fără erori și fără atenționări. Stornoul nu e factură emisă (122 f). | acceptat |
| SC-DECL-10 | Mutanți pe fișierul din SC-DECL-07: baza `op1` L 21 + 1 fără rezumat → DUK; `totalPlata_A` + 1 → DUK; `codPR` 99 → XSD. | refuzat |
| SC-DECL-12 | Bunurile unui rând își împart rândul rotunjit (122 b), ambele profiluri: cinci bunuri de 0,60 pe un rând de 3 lei → 1, 1, 1, 0, 0 (regula veche dădea −1, 1, 1, 1, 1); 2,60 și −2,40 pe un rând de 0 lei → 2 și −2; 500 de rânduri aleatoare (sămânța 122): lei întregi, suma egală cu rândul, semnul păstrat, abatere sub 2 lei. | acceptat |
| SC-DECL-13 | Decembrie, privat: FCL N21 100,40 către client și FCL N21 200,40 către furnizor (amândoi tip 1). `DiferenteRotunjire` = [L 21%, 2 rânduri, bază −1, TVA 0]: fișierul are 100 + 200 = 300 de lei, totalul rotunjit o dată 301. XSD și DUK fără erori și fără atenționări. | acceptat |
| SC-DECL-14 | Fișierul confirmat (122 i), privat: D394 decembrie cu persoane afiliate se reface din `Parametri` cu aceeași amprentă; alți parametri → `DEPUNERE_FISIER_DIFERIT`; telefonul societății schimbat după export → aceeași amprentă (antetul e în parametri); amprenta fără parametri → `DEPUNERE_DATE`; confirmarea păstrează amprenta și parametrii; aceeași cerere e idempotentă. D300 ianuarie cu sold precedent 12,34, temei 2, pro-rata 50 și `bifaInterne` 0 se reface cu aceeași amprentă. | acceptat |
| SC-DECL-16 | Mapările D300 și D394 sunt coerente (123 a, b), privat. Gardianul de seed refuză: `SDD`/livrare → AI; `SDD`/achiziție → L; `NED21`/livrare exclusă fără motiv; `N21`/livrare → V (D300 o pune pe rd. 9, care permite L, LS); `IMP21`/achiziție → A (importul se mapează numai ca excludere). Excluderea cu motiv trece. Gardianul de editare refuză la salvare D394 `N21`/livrare → V, D300 `N21`/livrare → rd. 13 (D394 e L, rd. 13 permite V), excluderea fără motiv și `IMPTI21`/achiziție → C; o excludere cu alt motiv se salvează. Cele 30 de mapări ale profilului rămân după fiecare probă. | refuzat |
| SC-DECL-17 | August, privat: FCL N21 50 și FCL SFD 100 (scutită). Fișierul D394 se generează fără refuzuri, XSD v5 și DUK fără erori și fără atenționări; SFD 100 e în `Excluse`, cu motivul, iar `Neincluse` e gol (123 a). În punte, SFD/livrare: D300 100, exclus 100, declarat 0. | acceptat |
| SC-DECL-18 | FCT N21 200 înregistrată la 02.09 și primită la 28.08, privat. Puntea din august: D300 0, doar în perioada D394 200/42, declarat 200, diferență −200, documentul numit. Puntea din septembrie: D300 200, doar în perioada D300 200/42, declarat 0, diferență 200. Pe aprilie, august și septembrie, fiecare rând se închide (D300 − doar D300 + doar D394 = declarat + exclus + neinclus, bază și TVA), iar declaratul, exclusul și neinclusul din punte sunt sumele `Operatiuni`, `Excluse` și `Neincluse` (123 c). | acceptat |
| SC-DECL-15 | Corectarea confirmării (122 i), privat, D394 decembrie: reconfirmarea aceleiași versiuni fără fișier înlocuiește confirmarea cu fișier (aceasta rămâne retrasă, cea nouă o numește în `Inlocuieste`); reconfirmarea cu fișier atașează fișierul ulterior; retragerea fără motiv → `DEPUNERE_DATE`; retragerea cu motiv scoate luna din „depusă”; a doua retragere → `DEPUNERE_NECONFIRMATA`. Garda D300 e în SC-CIT-84. | acceptat |

## Validare

Decizia 123, 2026-10-16: SC-DECL-17 și SC-DECL-18 verzi la `--scenarii DECL privat`
(`run-verificari/d123/decl-privat.log`); SC-DECL-16 rulează în integrală (`VerificaD394Seed`). Integrala e verde pe ambele profiluri (privat 5599, bugetar 4091, fără FAIL; a doua rulare privată pe aceeași bază, tot verde), în `run-verificari/d123/`. Calea reală, pe o bază nouă din seed (`run-verificari/d123/ui-porneste.ps1`): HTTP D394 august cu SFD în `Excluse` și puntea cu D123-FCT-1; ecranul React D394 cu panoul de excluse și puntea; în XAF, N21/livrare → V refuzat la salvare cu mesajul rândului 9, iar baza rămâne neschimbată.

Discuția deciziei 122, 2026-10-15: SC-DECL-12…14 verzi pe ambele profiluri
(`run-verificari/20261015-200648-030`); calea HTTP a confirmării cu fișier
(D300 și D394, mai 2026, baza E2P4) e în `run-verificari/e2-122-depunere.ps1`.

Pasul 5, 2026-10-15, după review-ul advers: harta rând → atribut e verificată
pe o transcriere separată din `d300-structura-2026.md` §5, iar fiecare fișier
cere DUK fără erori și fără atenționări. SC-DECL-11 probează stornoul în altă
lună. `--scenarii DECL,CVL` verde pe ambele profiluri.

Pasul 2, 2026-10-15: SC-DECL-07…10 verzi pe privat, SC-DECL-08 (antetul) și
09 (luna goală) și pe bugetar; DUK D394Validator J8.0.2 acceptă fișierele
fără atenționări. Validatorul cere `baza_incasari_i1`/`i2` și TVA-ul lor pe
fiecare `rezumat2` cu cotă diferită de 24; ies cu zero, fiindcă modelul nu
are încasări AMEF.

Pasul 1, 2026-10-15: `verifica.ps1 -Suita Scenarii -Tip DECL -Profil Ambele
-Sufix .ClaudeCVL` verde (`run-verificari/20261015-163901-958`). DUKIntegrator
cu D300Validator J12.0.1 acceptă fișierele SC-DECL-01, 03 și 04 fără
atenționări și respinge mutanții SC-DECL-05 pe regula așteptată.
