# 92. Identitatea partidei include partenerul

- **Data**: 2026-09-23
- **Stare**: activă; amendează N-D6 din contractul TR-D6a și precizează 090(d)
- **Docs**: `docs/nucleu/scenarii/NTC.md` (SC-NTC-13), `docs/nucleu/tr-d6a-nucleu-pur-contract.md`, `docs/nucleu/tr-d7b-tipuri-ramase-contract.md`

## Regula durabilă

(a) Partida nouă are ID determinist din `(document deschizător, cont,
partener)`. Parteneri diferiți pe același document și cont deschid partide
diferite. Data nu intră în identitate. Codificarea este SHA-256 peste cele
trei `Guid.ToByteArray()` concatenate în această ordine; primii 16 octeți
devin Guid, cu nibble-ul de versiune din octetul 7 pus pe 8.

(b) Postările deja persistate rămân nemodificate. La nominalizarea unei
surse sau la transfer se folosește identitatea persistată, inclusiv cheia
veche `(document, cont)`, recunoscută în adaptorul cubului. Compatibilitatea
nu permite cumularea a două identități sau a doi parteneri sub aceeași
partidă. Nu se reoperează documente prin politici pentru a le schimba cheia.

(c) Scenariul obligatoriu este nota cu două datorii de 100 pe 401, pentru
furnizori diferiți: două partide de −100; stingerea uneia nu o atinge pe
cealaltă. Testele nucleului verifică determinismul, independența față de
dată și separarea după fiecare componentă a cheii. Compatibilitatea cu
identitatea veche se probează separat în adaptor, fără mutații pe Flax.

## Context și tranșare

N-D6 și testul său excludeau explicit partenerul din identitate. NTC poate
conține parteneri diferiți pe același cont. Recensământul read-only Flax din
2026-09-23 a găsit 78 combinații document–cont de acest fel, maximum 82
parteneri; contraexemplul minim nu depinde de datele importate.

Owner-ul a ales explicit „Extindem cheia cu partenerul”. Varianta de refuz
temporar al acestor note nu este adoptată. Direcția cubului și regulile
FIFO nu se schimbă. Nu este necesară o migrație care rescrie istoricul.

## Restanțe

Nu introduce o felie separată: implementarea și probele intră în pasul 3
NTC/ITV. Concurența și cititorii comuni rămân în obligațiile 091 existente.
