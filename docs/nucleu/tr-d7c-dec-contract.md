# DEC — declarație completă înaintea TR-D8

- Data: 2026-09-24
- Stare: implementare autorizată prin 095; DEC-B2 aprobată explicit prin 096.
- Surse: 090(d/j), 091, 092, 095; mecanismul existent DEC și politicile
  profilurilor; invarianții II/III/IV.

## DEC-B1 — Contabil și fiscal

DEC justifică cheltuiala titularului. Rezolvarea conturilor urmează regula
tipului cu prioritatea conturilor explicite de pe linie; repartitorul
explicit are prioritate față de cel implicit. Valoarea trebuie să fie
pozitivă, cantitatea zero la culegere devine 1 prin pregătirea existentă.
Nu produce mișcări de stoc. Exemple în politica privată: bază 100 și
taxă 21 → D cheltuială 100, D 4426 21, C 542 121. Capitalizat 121 →
D cheltuială 100 cu fapt Bază și D cheltuială 21 cu fapt Taxă, C 542 121.
Bugetarul păstrează brutul calculat prin politica lui, fără fapte fiscale
cât timp nu are PoliticaTva. Taxarea inversă folosește conturile politicii.

Contrapartida taxei rămâne a politicii TVA, nu contul explicit de net;
proba cu credit explicit diferit verifică separat cele două sume. Cota,
rotunjirea și perioada de declarare folosesc mecanismele fiscale comune.
Partenerii expliciți diferiți pe același cont rămân separați conform 092.

## DEC-B2 — Partida titularului: contradicția de model găsită

`Cont.RolTert` are Niciunul/Client/Furnizor și este rol comercial SAF-T.
Seed-ul exclude intenționat decontările necomerciale; 542 și analiticele
sale au Niciunul pe ambele profiluri. Cubul îl folosește însă și drept
condiție pentru deschiderea partidei. Astfel, DEC 121 și PLT 50 pe 542
produc sold contabil 71, dar zero partide: cititorul „documente cu rest”
portat pe unitate ar pierde decontul, deși stingerea există în produs.

**Aprobat prin 096:** separăm `Cont.UrmarestePartide` (bool, politică
operațională) de `Cont.RolTert` (rol comercial SAF-T). Motorul, deschiderea
și cititorii de partide consumă primul. SAF-T păstrează rolul comercial.

- Migrația inițializează UrmarestePartide=true pentru conturile existente
  cu Client/Furnizor, păstrând comportamentul lor.
- Seed-ul activează urmărirea pentru conturile comerciale deja acoperite
  și pentru conturile operative 542 ale fiecărui profil; niciun simbol
  în motor. Conturile manuale păstrează configurarea explicită.
- Alte decontări (de exemplu 461/462) nu sunt activate implicit în această
  felie; politica le poate activa fără release.
- Titularul este Angajat, rămâne identificat ca atare; nu devine Furnizor
  și nu apare în Suppliers numai pentru că îi urmărim restul.
- Istoricul postărilor fără unitate nu se rescrie prin simpla migrație.
  Activarea cititorilor pe o bază cu asemenea istoric cere raport și
  tratare explicită, ca celelalte lipsuri din regimul dual.

Alternativă: extindem RolTert cu `Decontare` și filtrăm strict
Client/Furnizor în toți cititorii SAF-T. Necesită mai puțină schemă,
dar păstrează în aceeași proprietate două responsabilități diferite.
Nu folosim Furnizor fictiv pentru a face proba DEC să treacă.

## DEC-B3 — Probe și graniță

Catalog numeric înainte de cod: `scenarii/DEC.md`. Ciclul complet pe
ambele profiluri: simplu, linii multiple, explicit, TVA aplicabilă,
anulare, storno aceeași lună/peste perioadă, corecție, PLT/INC și
desfacerea stingerii, refuzuri atomice și perf 2/51. Conturile și
titularii se aleg ca date, fără comutatoare DEC în nucleu.
Citirile de producție rămân TR-D8; scenariile verifică independent cubul.

## Închidere implementare — 2026-09-24

Declarant DEC și 096 implementate. Catalogul SC-DEC-01…17 este verificat;
refuzurile de configurare sunt probate pe operand, restul pe comenzile reale.
Integral: 2.413 / 3.503 OK, zero FAIL, manifestul și limitele în `scenarii/DEC.md`.
Istoricul fără unitate rămâne 096-r1, iar cititorii de producție rămân TR-D8.
Review-ul advers este cerut separat; nu este declarat efectuat aici.
