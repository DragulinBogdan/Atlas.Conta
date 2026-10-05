# Runda 3: critica încrucișată

## Contextul

Doi analiști independenți au răspuns aceleiași întrebări despre acest repo,
la commit-ul `a5df5fe`: înaintea ștergerii registrelor (contractul TR-D9a,
`docs/nucleu/tr-d9-taierea-contract.md`), ce s-a câștigat și ce s-a pierdut
cu cubul de postări și care opțiune rămâne.

- **A.** Numai cubul. Orice citire e o sumă pe cub sau pe soldurile lui.
- **B.** Cubul e autoritatea, registrele sunt proiecție materializată din el
  și calea principală de citire a rapoartelor.
- **C.** Numai registrele. Motorul le scrie direct.
- **D.** Regimul dual de azi, ca termen de referință.

Criteriile: coerența modelului, simplitatea implementării, simplitatea
reprezentării unui tip de document, portița notelor contabile punctuale, loc
pentru optimizări de viteză, explicabilitatea la audit, reversibilitatea.

Tu ești unul dintre cei doi. Raportul tău de runda 2 este
`docs/consultare/raport-PROPRIU.md`. Raportul celuilalt analist este
`docs/consultare/raport-CELALALT.md`. A lucrat pe același repo, același dosar
(`docs/dosar.md`) și aceleași sarcini, fără să te vadă.

## Reguli de lucru

- Numai citire: fără modificări, fără build, fără ModelCheck, fără rulări pe
  baze. Ai voie cu citire de fișiere, căutare și `git log`.
- Fiecare afirmație de fapt poartă `fișier:linie`. Ce nu poți ancora
  marchezi `[judecată]`.
- Documentele din repo sunt scrise de autorii direcției actuale. Proba e
  codul, catalogul de scenarii și măsurătorile.

## Sarcinile

1. Numește cele mai slabe trei afirmații din raportul lui. Verifică-le pe
   cod și spune ce ai găsit, cu `fișier:linie`.
2. Numește ce a văzut el și ți-a scăpat ție.
3. Pentru fiecare dezacord de verdict dintre voi, spune ce dovadă l-ar
   tranșa și dacă ea se poate obține fără prototip.
4. Spune dacă îți schimbi recomandarea pentru TR-D9a. Dacă da, din ce cauză.
   Dă lista ta finală de amendamente, în ordinea priorității.
5. Scrie într-un paragraf ce i-ai spune owner-ului dacă ar avea un singur
   minut.

Nu căuta consensul. Un dezacord rămas, cu dovada care l-ar tranșa, e un
rezultat bun. Cel mult 1.200 de cuvinte, în română.
