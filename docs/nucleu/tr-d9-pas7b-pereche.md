# TR-D9a, pasul 7b — cheia de pereche pe postare (D9-A2)

Data: 2026-10-07. Execuție: main, direct. Contractul: amendamentul 1, D9-A2; D9-D15 rândul 7b; regula de oprire 10.
Nu schimbă nicio cifră de catalog; schimbă schema (coloana `Pereche` în `InitialCreate`, bazele recreate).

## Ce s-a scris

- **Nucleu.** `Postare.Pereche` (`int?`): ordinalul mișcării sau mutării în tranzacția ei. `Miscare.Postari` și
  `Mutare.Postari` cer ordinalul (≥ 1) și îl pun pe ambele postări; `Motor.Opereaza` numerotează mișcările
  1…m în `Operare` și mutările 1…k în `Transfer`, `Motor.Transfera` mutările 1…n. Transformările
  (`Transformare.Postari`) și deschiderea rămân fără ordinal.
- **Invariantul** (`Conservare.VerificaPerechile`, codul `PERECHE_INVALIDA`): în orice tranzacție care nu e
  deschidere, fiecare ordinal nenul are exact două postări, cu aceeași cauză, cantitățile opuse și, pe laturi
  opuse, aceeași valoare și aceeași valoare în valută (`Operare`), ori, pe aceeași latură, valorile opuse
  (`Transfer`); `Storno` acceptă oricare dintre forme. Ordinalul sub 1 și ordinalul pe deschidere sunt refuzuri.
- **Stornoul.** `Storno.Inverseaza` primește postările cu tranzacția lor sursă (`(Guid Sursa, Postare)`) și
  decalează ordinalele fiecărei surse cu maximul surselor dinaintea ei, în ordinea identificatorului; prima
  sursă păstrează ordinalele originalului. `Storno.Selecteaza` are forma cu sursă; formele plate rămân pentru
  o singură sursă. Materializarea trimite `TranzactieId` ca sursă; `DesfaceTransfer` moștenește ordinalele prin
  `with`.
- **Persistența.** `Postare.Pereche` (`integer NULL`) în `InitialCreate`, în snapshot și în `PostareVizual`
  (view pe `p.*`, entitatea cu proprietatea, STR-VIZUAL-1 cere paritatea). `Randuri.Citeste`/`Scrie` o poartă.
- **INV-CUB** (`Citiri.Invarianti.VerificaPerechi`): `CITIRE_PERECHE_INVALIDA` — pe (tranzacție, ordinal)
  nu sunt exact două postări cu același document, aceeași linie, cantități opuse și valorile potrivite
  laturilor felului; `CITIRE_PERECHE_LIPSA` — postare fără ordinal în afara deschiderii și a liniilor
  transformării (cele care au contrapondere virtuală pe aceeași cauză). Mutanții `PERECHE-RUPTA` (o postare a
  perechii fără ordinal) și `PERECHE-LIPSA` (ambele fără ordinal).
- **ModelCheck.** `STR-STORNO` reconstruiește stornoul din rândurile proprii cu tranzacția lor sursă;
  `STR-ROUNDTRIP` compară structural, deci cuprinde ordinalul. `N-r8` are `VerificaPerechi` ca martor permis
  (perechea transferului stă pe aceeași latură).

## Abaterea de la litera amendamentului, declarată

D9-A2 spune „postarea de storno poartă ordinalul originalului ei". Stornoul produsului e o singură tranzacție
peste mai multe surse (operarea, transferurile de stoc, cele de deschidere, atribuitele), iar ordinalele
surselor încep toate de la 1: ordinalul literal s-ar ciocni și invariantul ar refuza orice storno al unui
document cu împerechere. Forma implementată: ordinalul originalului plus decalajul sursei; pentru stornoul unei
singure surse (cazul fără transferuri) coincide cu litera. Perechile rămân întregi: două postări ale
stornoului au același ordinal exact când originalele lor erau pereche în aceeași sursă
(`StornoulPesteSurseDecaleazaPerechileSiLePastreazaIntregi`). Intră în decizia 110 ca precizare la D9-A2.

## Probele

| Probă | Rezultat |
|---|---|
| Nucleu | 190/190 (180 + 10: numerotarea mișcărilor/mutărilor și a transferului, ordinalul sub 1, perechea ruptă/dublată/sub 1/pe laturi nepotrivite/cu cauze diferite, deschiderea cu pereche, stornoul pe surse, stornoul unei singure surse, transformările fără ordinal) |
| `--probe-sursa` | 11/11 (`N-r8` cu martorul nou) |
| `has-pending-model-changes` | curat |
| `--dump-metadata` | fără diferență |
| Integrala privat | 4.795 OK / 0 FAIL pe `Atlas.Conta.ModelCheck.Privat.P7b` (creată de ModelCheck din `InitialCreate`) |
| Integrala bugetar | 3.516 OK / 0 FAIL pe `Atlas.Conta.BackOffice.P7b` (ef `database update` + updater Blazor) |
| Diferența față de pasul 7 (`compara.py`) | numai adaos: +4 pe fiecare profil (cei doi mutanți, pe ambele apariții); zero dispărute |

Log-urile: `run-nucleu/tr-d9a/pas7b/` (gitignored). Prima rulare privată a picat cu `CITIRE_PERECHE_INVALIDA`
pe perechile fără linie: `COUNT(DISTINCT "LinieId")` ignoră NULL; corectat cu `?? Guid.Empty`.

## Ce rămâne

- Cititorii nu folosesc cheia (D9-r4): contrapartida din fișa contului și conturile corespondente din
  registrul jurnal rămân cum sunt.
- Cheia nu acoperă transformările (D9-A9, pentru 110).
- Bazele de dezvoltare se recreează din `InitialCreate` (coloana nouă); clonele de import și de perf rămân pe
  schema veche până la pasul 8.
