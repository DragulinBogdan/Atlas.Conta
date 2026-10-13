# D9b — rezultatul A/B pe baza B

`comparatie.json` păstrează cele 48 perechi, cinci durate și mediana fiecărei
serii, cardinalitățile/amprentele DTO, alocările, comenzile, scanările din
planuri, schema, configurația, amprentele binarelor și ale faptelor,
dimensiunile originii și comenzile cu 2/51 partide.

Datele au fost produse prin scriitorul B și amplificate din fotografia
scenei reale. Ambii cititori au folosit aceeași bază B. Seria de sistem
rulează pe ușa nesecurizată a furnizorului securizat, cea a hosturilor. SQL-ul A este
verificat față de captura din `../D9bA/`; cititorul vechi există numai în
harness. SQL-urile B și planurile brute sunt în directoarele rulării.

Scriitorul A a fost reconstruit separat din fotografia curentă, restaurând
trei fișiere de produs din `96ccc5ac`, doar cu redenumirile CLR aplicate.
Lista și SHA-256-urile surselor sunt în `ProvenientaScriitorA`. Modelul și
migrația B au rămas prezente. Scrierile au folosit aceleași note pe aceeași
bază; amprentele postărilor și originilor coincid înainte/după și între A/B.

Toate pragurile sunt verzi în această rulare, a doua, făcută la
reverificare pe o bază B nouă. Pentru 51 partide B are 63,5928 ms, limita
78,2313 ms; în prima rulare avea 73,5688 față de 74,0146. Mediana din cinci
comenzi nu desparte variantele: rezultatul nu dovedește o rezervă de
performanță la scriere.

Reproducere, comenzile și limitele montajului sunt în
[raportul pasului 5](../../../../../docs/nucleu/tr-d9b-pas5-comparatie.md).
Artefactul complet local este produs prin `verifica.ps1 -Suita D9bRaport`;
JSON-ul versionat rezumă probele, fără a înlocui logurile sau planurile:
pe serie păstrează cele cinci durate calde, mediana lor, comenzile,
cardinalul, amprenta, mediana alocărilor și planul ultimei execuții.
