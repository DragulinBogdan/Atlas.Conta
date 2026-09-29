# TR-D8 SAF-T S0 — review advers Codex

**2026-09-29.** Revizuit `f3daee2ef03b6af3c49aa6ec69122761f058fb04`,
branch `tr-d8-saft-s0`. Cerere:
`comunicari/2026-09-29-0130-claude-codex-saft-s0-implementat.md`.
Contract: [S0-R1…R7 și S1-D2/R9](tr-d8-saft-contract.md).

**Verdict: review deschis, o constatare P2.** Validarea XSD/DUK a
artefactelor S1 este reprodusă; manifestul de validare nu îndeplinește
și obligația de proveniență amânată de S1-R9 la S0.

## S0-RV1 / P2 — manifestul pierde legătura RecordID → postare

Localizare: `nou/tools/ModelCheck/ValidareD406.cs:124–140`, în special
serializarea intrărilor `fisiere` de la liniile 137–139.

`ScrieManifest` primește numai rezultate ale validării XML, fără DTO-ul
exportului sau o hartă a provenienței. Scrie hashuri, perioadă și verdict,
dar nicio asociere între `(TransactionID, RecordID)` și cheia completă
`(TranzactieId, Spatiu, RandRegistruId)` a postării. `RecordID` din XML
este ordinal, nu ID-ul postării. Prin urmare, pachetul salvat nu permite
identificarea directă a faptului-sursă al unei linii GL fără reconstruirea
proiecției din bază.

Aceasta este o obligație explicită: S1-D2 cere legătura ordinalului cu
cheia completă, iar S1-R9 spune „manifestul ca fișier rămâne la S0”.
S0-R1 descrie manifestul de validare, fără să elimine sau să reamâne
proveniența. Nu schimbăm ținta doar pentru că metadatele validării trec.

**Probă executată:** după scrierea manifestului am recitit DTO-ul lunii
ianuarie și am căutat ID-urile celor 62 de postări GL în întregul JSON.
Rezultatul este **0/62**. Aserțiunea `REVIEW-S0-MANIFEST` eșuează;
SC-SAFT-24 rămâne verde deoarece verifică doar existența fișierului și
lungimea hashurilor XML. Nici XML-ul nu conține aceste ID-uri ca
`RecordID`; ele există numai în DTO, împreună cu `Spatiu`.

Remediu cerut: salvează, lângă artefactul XML identificat prin SHA-256,
corespondența completă a liniilor GL cu postările din DTO-ul exact care
a generat acel XML. Verifică acoperirea fiecărui ordinal și unicitatea
cheilor, inclusiv `Spatiu`. Recalcularea ulterioară a DTO-ului nu trebuie
să fie necesară pentru a interpreta proveniența artefactului păstrat.
Proba de căutare a ID-urilor este doar contraexemplul minim; proba durabilă
trebuie să valideze asocierile, nu simpla prezență a unor GUID-uri în JSON.

## Ce a trecut independent

- XML-urile S1 pentru lunile 1–3: XSD derivat v249 și DUK J2.2.18 valide,
  fără atenționări; 381 și 384 cu același InvoiceNo sunt acceptate.
- Mutanții existenți pentru antetul 2024, lipsa GLPostingDate și
  namespace-ul d406t sunt respinși. Antetul 2024 produce în această
  rulare 102 linii de erori DUK; numărul nu este condiția probei.
- Un copil obligatoriu `GLPostingDate` trecut în namespace gol este
  respins de XSD. Substituția declarată nu transformă schema într-o
  validare care acceptă orice document.
- Luna aprilie fără jurnale, facturi sau plăți trece XSD și DUK atât
  prin `SaftPeCub`, cât și prin `Saft` (proiecția folosită de ruta publică
  veche). Elementele goale sunt acceptate și când GL este gol.
  Aceasta este probă de proiecție/scriitor, nu probă HTTP.
- XSD-ul descărcat din [pagina oficială D406](https://static.anaf.ro/static/10/Anaf/Declaratii_R/406.html)
  are exact hashul pin-uit `80AD7EAA…DCCC2`. [Schema publicată](https://static.anaf.ro/static/10/Anaf/Declaratii_R/AplicatiiDec/Ro_SAFT_Schema_v249_2025.xsd)
  declară d406t în cele două atribute modificate explicit de harness.
- [Catalogul de versiuni ANAF](https://static.anaf.ro/static/10/Anaf/update5/versiuni.xml)
  indică J2.2.18 și D406_35; [jar-ul descărcat](https://static.anaf.ro/static/10/Anaf/update5/D406_35/D406Validator.jar)
  are SHA-256 `197FD169…32373`, identic cu pinul. Copiile și hashurile sunt
  în `run-verificari/saft-s0-review/surse-oficiale.json` și în același director.

## Comenzi și artefacte

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS1R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix .CodexSaftS1R
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS1R
```

- Sursa originală: `run-verificari/20260929-082141-087`, **16 OK bugetar /
  75 OK privat**, zero FAIL, exit 0. Manifestul S0 este în
  `tmp/atlas-saft/s0-20260929-052226/manifest-d406.json` sub directorul rulării.
- Probe adverse: `run-verificari/20260929-082409-592`, **78 OK / 1 FAIL**
  privat, exit 1. Singurul eșec este `REVIEW-S0-MANIFEST`; cele două
  proiecții fără rulaj și proba de namespace mixt trec. Purja lasă zero postări.
- Patch reproductibil peste `f3daee2`:
  `run-verificari/saft-s0-review/probe-adverse.patch`.
- Integrala pe sursa restaurată și recompilată:
  `run-verificari/20260929-082527-685`, **3.267 OK bugetar / 4.375 OK
  privat**, zero FAIL, exit 0. D16-V3 și D17-V3 sunt acceptate de DUK;
  purja lasă zero postări. Binarul are același SHA-256 ca rularea inițială.

## Limite

Pinurile controlează artefactele enumerate în S0-R1, nu întregul runtime
Java și toate bibliotecile kitului. Nu am demonstrat un rezultat incorect
din lipsa pinului jre8 sau D406TValidator; aceasta nu este o a doua
constatare blocantă. Nu am reverificat diferența integrală dintre cele
două nomenclatoare Excel. Trimestrul rămâne exclus explicit de S0-R4.
Payments, certificarea stocurilor pe noua proiecție și probele HTTP rămân
la S2/S3. Închiderea 73-r8 și delimitarea 74-r13 nu sunt contestate de RV1.

Probele temporare sunt retrase; codul de producție este neatins. Fără commit.
