# 103 — Fapte fiscale istorice și perioade distincte de declarare

Data: 2026-09-25
Stare: activă; amendează 088(g/h) și DVI-B3
Docs: `docs/nucleu/tr-d8-citiri-contract.md`, §D8-B8; `docs/nucleu/scenarii/CITIRI.md`, SC-CIT-79…87

## Regula durabilă

**(a)** Cubul păstrează calificarea fiscală istorică: regim, cotă, import,
identitatea documentului fiscal și atribuiri distincte D300/D394. Sumele
fiscale sunt valorile stocate, cu semn; cartea și contul sunt proveniență.
Inversa copiază calificarea originală. Nomenclatorul curent oferă etichete.

**(b)** Data documentului, faptul generator/exigibilitatea când diferă,
primirea la achiziție și înregistrarea sunt repere distincte. Data primirii
se propune din data înregistrării, este editabilă în draft și se îngheață la
operare. D394 folosește primirea/emisia, D300 perioada exercitării obligației.

**(c)** Eroarea de evidență, factura distinctă de corecție și eroarea materială
a formularului sunt cazuri distincte. Pentru factura înregistrată 100/21,
corectată la 80/16,80 după declarare, D300 păstrează perioada declarată și
duce diferența −20/−4,20 la regularizarea curentă. D394 înlocuiește perioada
inițială cu 80/16,80 și o singură factură. Inversa tehnică nu este factură.
Factura nouă de reducere −20/−4,20 aparține propriei perioade de emitere/primire.
Exemplul nu definește procedura ANAF pentru erorile materiale ale formularului.

**(d)** Închiderea internă nu dovedește depunerea unei declarații. Depunerea
se confirmă explicit pe formular/perioadă, cu versiunea exportată și momentul
confirmării. Reperul nu păstrează un registru paralel de sume fiscale.
Versiunea este amprenta deterministă a faptelor perioadei din citirea comună,
calificată prin formular și perioadă, emisă împreună cu exportul. Raportul
și amprenta văd același snapshot. Confirmarea o recalculează sub blocaj
exclusiv și refuză `DEPUNERE_VERSIUNE_DEPASITA` dacă faptele diferă.
Eticheta liberă nu este versiune. Exportul JSON disponibil nu este XML ANAF;
amprenta certifică faptele cubului, nu parametrii externi/manuali ai formularului.

**(e)** Achiziția cu taxare inversă are o Bază, o Taxă și o Autocolectare.
Autocolectarea se atașează contrapărții contabile colectate și păstrează
Sens=Achiziție. Unicitatea este cel mult o postare per rol și fapt economic;
rolul suplimentar este permis numai la autolichidare. D300 nu mai dublează
obligația prin oglindirea taxei. D394 numără o achiziție.

**(f)** Cititorii fiscali trec prin `Cub.Citiri.Fiscale`, pe ambele cărți.
Identitatea atomului este `(Spatiu, PostareId)`; gruparea păstrează calificările
istorice și proveniența corecției. Nu se introduce snapshot fiscal cumulativ.
Codificarea SAF-T este o mapare versionată la margine pe secțiune, calificare
istorică și versiune de export. O mapare absentă produce diagnostic.

**(g)** Implementarea și validarea urmează D8-B8 și SC-CIT-79…87; aprobarea
nu ține loc de probă. Compatibilizarea bazelor de dezvoltare rămâne interzisă
de 102. TVA la încasare, pro-rata și alte regimuri necontractate rămân în afară.

**(h)** Intervalele opționale `ValabilDeLa`/`ValabilPanaLa` pe tipul TVA
se compară cu exigibilitatea, nu cu data documentului. Ieșirea din interval,
schimbarea retroactivă a intervalului și editarea cotei/regimului/importului
unui tip folosit produc avertismente și raport de impact; nu refuză operarea,
nu stornază și nu reoperează automat. Faptele își păstrează calificarea
istorică. Utilizatorul alege remediul: document fiscal nou pentru emise,
clarificare cu furnizorul/vama pentru primite, corecție tehnică pentru
eroarea proprie de evidență.

Regularizarea avansului se recunoaște prin marcă de politică, niciodată
prin simbolul contului. Referința opțională a liniei la factura de avans
permite compararea cu cota și exigibilitatea istorice ale avansului.
Lipsa referinței produce avertisment, fără refuz. Regula este alegerea
owner-ului din 2026-09-26, transmisă în review-ul 1018 și reluată în cererea
de continuare; implementarea R6 și corecturile review-ului sunt verificate la 2026-09-28; contractul este
`docs/nucleu/tr-d8-tva-intervale-contract.md`.

**(i)** Taxa liniei are un marcaj explicit, `TvaCules`, în locul proxy-ului
„nenul = cules”. Taxa nemarcată e calculată și se recalculează la operare pe
cota curentă. Taxa marcată e a operatorului, se păstrează și e comparată
aritmetic cu cota. Marcajul stă pe baza liniei, iar tranzițiile lui sunt
numai în L3. Zeroul explicit rămâne refuzat pe un tip cu taxă.

Seed-ul profilurilor poartă intervalele declarate ale tipurilor TVA
(Legea 141/2025: 19% până la 31.07.2025; 21% și 11% de la 01.08.2025;
9% pentru locuințe până la 31.07.2026). Seed-ul le aliniază numai pe
rândurile `DinSeed` și nu inactivează automat tipurile.

Ambele reguli sunt alegerile owner-ului din 2026-09-27 (M1(B) și M7).
Detaliile, tabelul complet și probele sunt în contractul R6 (R6-B2, R6-B8).

## Context și tranșare

Portarea cititorilor a identificat trei pierderi de informație: cota era
recitită din nomenclator, o singură perioadă servea două declarații cu repere
diferite, iar contrapartea taxării inverse nu avea fapt fiscal propriu.
Contractul D8-B8 documentează cercetarea, alternativele și exemplele numerice.
Owner-ul a aprobat explicit toate trei recomandările: **F1=A, F2=A, F3=A**.

088(g/h) rămâne istoric al regulii înlocuite; referințele sale la prima
închidere ca dovadă a declarării sunt înlocuite de (d). DVI-B3 păstrează baza
distinctă și unicitatea Bază/Taxă, completată cu Autocolectare conform (e).
088(g/h) nu mai are parametru de politică: `DeclarareIntarziata` este
eliminat din model, seed și contractele generate; atribuirea urmează (b–d).

## Restanțe

- **103-r1** — închisă 2026-09-26: D8-B8 implementată; integral 3.182/4.214
  OK, Nucleu 180/180, HTTP, browser și A/B pe aceeași bază. Dovezi și limite
  în `docs/nucleu/tr-d8-review-codex.md`. Review R1–R5 corectat, integral
  3.207/4.237 OK și HTTP/browser reluate; predare fără commit. Restul TR-D8 rămâne deschis.
- **103-r2** — R6 implementat și verificat, review advers închis (2026-09-28):
  intervale TVA, raport de impact, regularizarea avansului prin linia sursă,
  marcajul `TvaCules` și intervalele din seed, conform (h) și (i).
  Rămâne activă proveniența fiscală a ajustărilor RDC/RLF/reduceri (R6-B4);
  consumul/restul avansului nu intră în R6.
- **103-r3** — activă, la următoarea atingere a validării L3: semnele
  incompatibile ale bazei și taxei culese pe FCT/FCL sunt încă admise,
  cu avertisment aritmetic. Rămân de stabilit și probat refuzul de domeniu
  și identificarea liniei de factură fără proxy-ul `ILinieCuAvans`.
  Nu blochează închiderea R6 și nu introduce acum un refuz nou.
