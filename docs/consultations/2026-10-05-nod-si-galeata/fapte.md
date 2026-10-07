# Fapte pentru runda 2

Scris de sesiunea care a propus forma Y și regula Q. E părtinitor prin
construcție. Fiecare rând are sursa; verifică-l.

Prescurtări de cale: `N/` = `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`,
`M/` = `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`.

## 1. Ce e implementat azi

| Subiect | Forma de azi | Unde |
|---|---|---|
| Asamblarea | forma X: valoarea de la lot la lot pe grupuri de cont, contrapondere de cantitate cu valoarea zero în `GestiuniVirtuale.Transformare` | `docs/nucleu/tr-d7b-asm-transformare-contract.md` (ASM-B1…B7), `M/Declaratii/DeclarantAsamblare.cs`, `N/Motor/Transformare.cs` |
| Filtrul contraponderii | `Transformare.FaraContrapondere`, folosit de cititori | `M/Cub/Citiri/Transformare.cs:13`; apeluri în `Contabil.cs:34`, `Partide.cs:178` și `:198`, `Imobilizari.Conturi.cs:20`, `Invarianti.cs:96`, `nou/tools/ModelCheck/AcoperireInvarianti.cs:104` |
| Diferențele de inventar | forma Y deja: mișcare obișnuită între lot și capătul virtual `Inventar` sau `Consum`, contul capătului din `Contari.Rezolva`, fără compensare între linii | `M/Declaratii/DeclarantDiferenteInventar.cs:40-95`, `docs/nucleu/scenarii/LDI.md` (SC-LDI-03) |
| Mișcarea, mutarea | debit pe `La`, credit pe `DeLa`; mutarea cere același cont pe ambele capete | `N/Cub/Miscare.cs`, `N/Motor/Mutare.cs` |
| Conservarea | valoare pe carte, cantitate pe produs, transfer pe cont și latură, formele | `N/Conservare.cs` (unitatea nu e cerută în gestiune virtuală: linia 152) |
| Contul lotului | derivat din tipul de material al produsului, nu de pe linia documentului | `M/Motor/Fapte.cs:95-103` |
| Poziția fără partidă | regula P, partea de partide | `M/Cub/Citiri/Partide.cs:193-210` |
| Poziția fără lot | niciun gard | SC-NTC-14 în `docs/nucleu/scenarii/NTC.md` |
| Poziția fără fișă | gard de semn cronologic, pe coordonata completă, cu blocaj serial; conturile protejate deduse din trei surse | `M/Cub/Materializare.PozitieFaraFisa.cs`, SC-IMO-27, 28, 31 în `docs/nucleu/scenarii/IMO.md` |
| Dimensiunile obligatorii pe cont | steaguri pe cont; valoarea `Unitate` de acolo e unitatea organizatorică, nu lotul, partida sau fișa | `M/BusinessObjects/Comun/Enums.cs:125-135`, `M/Motor/MotorOperare.cs:511-533` |
| Notele fără unitate acceptate | 628 = 401 fără partener; 628 = 302 fără lot; 401 = 628 fără partener lângă o factură deschisă | SC-NTC-01, 14, 15 |

## 2. Ce spune contractul în lucru

- TR-D9a șterge registrele și regimul dual. Pasul 6 trece ASM pe gardul
  P = C, cu clasificarea pe C și distribuirea pe evaluarea pură:
  `docs/nucleu/tr-d9-taierea-contract.md:325-330` și `:665`.
- TR-D9 e împărțit în TR-D9a, tăierea, și TR-D9b, unitățile.
- Constatarea I2 din `docs/nucleu/tr-d9-inventar.md:23`: recepția facturii
  pe cub nu are gard de dimensiuni obligatorii.

## 3. Date reale

Sursa Flax, baza 1C originală, toate asamblările nemarcate:

| Măsura | Rezultat |
|---|---|
| Documente de asamblare | 10.198 |
| Linii pe contul 371.1 | 54.982 |
| Linii pe contul 302.8 | 1 |
| Documente cu mai mult de un cont | 1 |

Contractul ASM citează 367 din 1.228 de documente „cu mai multe conturi"
(`tr-d7b-asm-transformare-contract.md:31-36`). Acel recensământ număra contul
implicit al tipului de material al produsului, pe clona de import, nu contul
de pe linia sursei (`docs/nucleu/scenarii/recensamant-asm-ldi-nir.sql`).
Scenariul SC-ASM-04 își ia justificarea din acea cifră.

Owner-ul a hotărât că datele importate nu stabilesc regula.

## 4. Ce se propune

**Amendamentul 1, ASM pe același cont, forma Y.** Fiecare linie devine o
mișcare obișnuită între lot și nodul `Transformare`, pe contul lotului, cu
valoarea și cantitatea împreună. Gardul: nodul se închide pe cont, pe
document. Primitiva `Transformare`, clasificarea grupurilor și filtrul
contraponderii dispar. Documentul pe conturi diferite e refuzat și rămâne
pentru bonul de producție, rezervat prin decizia 019, cu contul nodului din
politică.

**Amendamentul 2, regula Q.** Regim declarat pe cont pentru poziția fără
unitate: închisă, deschisă, suport. Sămânța: deschisă pe stocuri și terți,
suport pe imobilizări și amortizări, nefiind editabil acolo. Numai nota și
deschiderea postează în găleată. Documentul tipat care nu numește unitatea
rămâne defect, ca invariant, nu ca politică. Închiderea unei găleți cu sold
refuză numai postările noi.

Poziția owner-ului: de acord cu amândouă. Poziția sesiunii care
orchestrează: de acord, cu gardul de semn păstrat ca regim suport și cu
amendamentul 1 pus în locul punctului ASM din pasul 6, nu după el.

## 5. Avertisment de circularitate

Catalogul de scenarii (`docs/nucleu/scenarii/`), testele nucleului și
ModelCheck au fost scrise sub forma X și regula P. Ele codifică forma, nu
numai regula contabilă. Un scenariu care ar pica în forma nouă nu e dovadă
împotriva ei. Unul care ar trece nu e dovadă pentru ea.
