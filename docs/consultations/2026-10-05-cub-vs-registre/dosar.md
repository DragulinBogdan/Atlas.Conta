# Dosarul de fapte (pentru runda 2)

- Commit: `a5df5fe`, branch `tr-d9-taierea`, 2026-10-05.
- Statut: hartă de dovezi, fără verdicte. Fiecare rând trimite la un fișier
  sau la o cifră numărată azi pe sursă.
- Autor: sesiunea Claude care a lucrat cu owner-ul la decizia 090 și la
  contractul TR-D9a. Autorul nu e neutru; analistul e rugat să raporteze ce
  omite sau înclină dosarul.
- Textele deciziilor și `CLAUDE.md` sunt scrise de autorii direcției
  actuale. Se citesc ca dovadă de intenție, nu ca probă.

## 1. Cronologia

| Data | Fapt | Unde |
|---|---|---|
| până la 2026-09-19 | sistemul rulează pe patru registre, motor pe `IObjectSpace` | `docs/decizii/001…089` |
| 2026-09-19 | sinteza câștig și pierdere, scrisă ÎNAINTE de implementare | `docs/nucleu/nucleu-bilant.md` |
| 2026-09-20 | decizia 090: cubul de postări | `docs/decizii/090-nucleu-cub-de-postari.md` |
| 2026-09-20 … 10-04 | feliile TR-D6a, D6b, D7a, D7b, D7c, D8 | `docs/nucleu/tr-d*-contract.md`, `docs/decizii/istoric-plan-de-lucru.md` |
| 2026-09-22 | decizia 091: proba supremă devine catalogul de scenarii | `docs/decizii/091-*.md`, `docs/nucleu/scenarii/` |
| 2026-10-04 | decizia 108: citirile sunt pe cub, registrele se ating prin listă nominală | `docs/decizii/108-*.md` |
| 2026-10-05 | contractul TR-D9a (tăierea) aprobat; pasul 1, inventarul, făcut; nimic tăiat | `docs/nucleu/tr-d9-taierea-contract.md`, `tr-d9-inventar.md` |

Între 2026-09-19 și azi sunt 175 de commit-uri pe ramura curentă.

## 2. Starea de azi: regim dual

- Tipurile cu `PosteazaInCub` scriu registrele și cubul în aceeași
  tranzacție de comandă. Pe privat sunt toate tipurile care declară; pe
  bugetar DSC, ITV, RDC, RLF și DVI sunt inerte.
- Citirile contabile, de stoc, de partide, fiscale și SAF-T vin din cub.
- Registrele se ating în producție numai prin lista nominală:
  `nou/tools/ModelCheck/ProbeCititoriRegistre.Lista.cs`. Are 75 de intrări:
  26 scriitor dual, 7 martor, 2 evidență XAF, 4 autorizare, 6 mapare,
  30 legătură.
- Ambele căi de scriere există și sunt probate. După TR-D9a rămâne una.

## 3. Forma celor două modele

Modulul: `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`,
prescurtat `M/`.

| Ce | Unde | Linii |
|---|---|---|
| `RegistruStoc`, `RegistruContabil`, `RegistruTva` | `M/BusinessObjects/Registre/Registre.cs` | 255 |
| `RegistruImobilizari` | `M/BusinessObjects/Registre/RegistruImobilizari.cs` | 59 |
| soldurile de perioadă | `M/BusinessObjects/Registre/SolduriPerioada.cs` | 99 |
| `Imperechere` | `M/BusinessObjects/Documente/Trezorerie.cs:652` | |
| `RegulaStoc`, `RegulaContare` | `M/BusinessObjects/Politici/Politici.cs:61`, `:83` | |
| `Postare` | `M/Cub/Postare.cs` | 65 |
| `Tranzactie` | `M/Cub/Tranzactie.cs` | 25 |
| designul cubului | `docs/nucleu/nucleu-cub-design.md` | |

## 4. Mărimea codului pe zone

Linii fizice, numărate azi, cu tot cu rânduri goale.

| Zona | Fișiere | Linii | Observație |
|---|---|---|---|
| `M/Motor/` | 25 | 6.824 | conține și cod care supraviețuiește tăierii |
| `M/Declaratii/` | 28 | 2.418 | declaranții per tip, operandul |
| `M/Cub/` cu `Citiri/` | 26 | 2.788 | materializarea și citirile |
| `nou/Atlas.Conta.Nucleu/` | 62 | 4.051 | motorul fără EF, cu testele lui |
| `M/Proiectii/` | 8 | 2.016 | |

Împărțirea lui `Motor/` pe membri care mor și membri care rămân este în
`docs/nucleu/tr-d9-inventar.md` §8. Cele mai mari fișiere: `GardianEditare.cs`
1.381, `MotorOperare.cs` 867, `SolduriService.cs` 457.

## 5. Unde stă fiecare caz de probă

| Caz | Calea registrelor | Calea cubului | Scenarii |
|---|---|---|---|
| 1. Factura de intrare | `M/Motor/MotorOperare.cs:63` (`CalculeazaSiValideaza`), `M/BusinessObjects/Documente/FacturaIntrare.cs`, regulile din `M/DatabaseUpdate/ProfilPrivat.cs` | `M/Declaratii/DeclarantFacturaIntrare.cs`, `Contractare.cs`, `Operand.cs` | `scenarii/FCT.md` |
| 2. NIR conex | `MotorOperare.cs`, `DocumenteGestiune.cs` | `DeclarantNir.cs`, `DeclarantNir.Diferenta.cs`, `M/Cub/ReceptiiConexe.cs` | `scenarii/NIR.md`; deciziile 098, 099; `tr-d8-nir-delta-contract.md` |
| 3. Asamblare | `M/BusinessObjects/Documente/Asamblare.cs`, `M/Motor/StocService.cs` | `DeclarantAsamblare.cs`, `M/Cub/Citiri/Transformare.cs` | `scenarii/ASM.md`; `tr-d7b-asm-transformare-contract.md` |
| 4. Nota contabilă | `M/BusinessObjects/Documente/NotaContabila.cs` | `DeclarantNotaContabila.cs` | `scenarii/NTC.md`; `nucleu-cub-design.md` §9 |
| 5. Storno | `MotorOperare.cs:637` (`Storneaza`), `M/Motor/CorectieService.cs` | `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Cub/Storno.cs` | cazurile de storno din fiecare fișier de tip; `scenarii/REGIM.md` |
| 6. Citiri grele | `M/Motor/SolduriService.cs`, `M/Proiectii/` | `M/Cub/Citiri/` | `scenarii/CITIRI.md`, `scenarii/SAFT.md` |
| 7. Custodie | neacoperită | refuzată explicit la LDI | decizia 093 (d); `scenarii/LDI.md` SC-LDI-14 |
| 8. ALOP | nescris | nescris | vezi §9 |

## 6. Măsurători

| Ce | Când | Unde |
|---|---|---|
| forma fizică a cubului contra registrelor, pe prototip, înaintea implementării | 2026-09-19 | `docs/nucleu/nucleu-fizica.md` |
| perf A/B pe aceeași bază, după implementare, pasul X-D5 | 2026-10-04 | `docs/nucleu/tr-d8-transversal-contract.md` |
| perf pe baza de import, tierul API | anterior cubului | `docs/api/p5-perf-masuratori.md` |
| coordonatele cubului contra rapoartelor reale | 2026-09-19 | `docs/nucleu/nucleu-coordonate-rapoarte.md` |

Regula proiectului: o cifră de perf se compară numai cu ea însăși, pe aceeași
bază. Dosarul nu a verificat dacă există o măsurătoare a registrelor refăcută
după implementarea cubului.

## 7. Ce șterge TR-D9a

- Contractul: `docs/nucleu/tr-d9-taierea-contract.md`, literele D9-D1…D15.
- Inventarul nominal: `docs/nucleu/tr-d9-inventar.md`, cu constatările
  I1…I8 încă nepreluate în contract.
- Anexa de probe: `docs/nucleu/tr-d9-inventar-modelcheck.md`.
- Tăierea e fără migrare de date; lanțul de migrații se resetează.
- După tăiere dispar regulile vechi ca termen de comparație executabil:
  planul din `MotorOperare`, oracolul normalizat din ModelCheck
  (`nou/tools/ModelCheck/Nucleu/CubDinRegistre.cs`) și cele patru grile XAF
  pe registre.

## 8. Restanțele

`docs/decizii/restante.md`, numărate azi după stare: 189 „după PoC",
30 „activă", 22 „migrare", 20 „cade la TR-D9". TR-D9b, unitățile, adaugă
comportament nou pe cub: reevaluarea, valuta pe partidă, costul vamal pe lot.

## 9. ALOP

- Amânat prin decizia 035: modulele bugetare se reiau după stabilizarea
  modelului pe privat (`docs/decizii/035-pivot-privat-first.md:24`).
- Owner-ul vrea ca ALOP să fie construit pe premisele și paradigma cazului
  general de pe privat.
- Implementarea veche: 30 de fișiere cu `alop` în nume sub `legacy/`.
  Inventarul din `db/inventar/` nu îl descrie.
- Coordonatele bugetare există deja pe `Postare` și pe ambele laturi din
  `RegistruContabil`: cod funcțional, cod economic, sursă de finanțare.

## 10. Ce nu conține dosarul

- Nicio comparație făcută de autor între căi.
- Nicio cifră despre timpul de dezvoltare per tip pe fiecare cale.
- Nicio schiță a opțiunii B; nu există cod pentru ea.
- Nicio părere a owner-ului în afara celor două rânduri din §9.
