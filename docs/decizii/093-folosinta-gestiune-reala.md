# 93. Folosința păstrează gestiunea reală și lotul

- **Data**: 2026-09-24
- **Stare**: activă, aprobată de owner; amendează 090(g) și TR-D7b T-D9 numai pentru Folosință; Custodie rămâne explicit neacoperită.
- **Docs**: `docs/nucleu/tr-d7b-ldi-contract.md` (LDI-B3), `docs/nucleu/scenarii/LDI.md`.

## Regula durabilă

(a) **Coordonatele Folosință sunt gestiunea reală, lotul și contul
politicii.** Contul din seed-ul bugetar este 303.02.00, fără hardcodare în
motor. Recepția, transferul, consumul și inventarierea folosesc aceeași
identitate a stocului. Nu se introduce o gestiune virtuală Folosință sau
o axă suplimentară și nu se schimbă semantica axelor Analiza.

(b) **În regimul dual, politica registrelor este coerentă pe lanț.** Pentru
clasa OF, BTR folosește Folosinta pe ambele laturi, BCS pe sursă, NIR și LDI
pe gestiunea stocului. Destinația consumului păstrează comportamentul BCS
curent. Conturile rămân date de politică.

(c) **Istoricul nu se rescrie.** Stornoul inversează postările și cheile
originale; operarea documentului nou/corecției folosește politica curentă.
Nu se caută sold prin fallback la alt TipStoc. Distribuția istorică pe
lot × gestiune × TipStoc și document se raportează distinct de comparația
valorică, care ar putea ascunde o distribuție greșită prin însumare.

(d) **Custodie rămâne refuzată explicit la LDI.** Existența mai multor
conturi 803x nu alege contul politicii și nu stabilește relația dintre
postarea cantitativă fără valoare și nota valorică de inventar. T-r6 și
TR-r7 rămân active pentru domeniul rămas și injectivitatea SAF-T.

## Context și tranșare

FCT scrie deja recepția în gestiunea reală. Mutarea numai a LDI pe o
constantă virtuală ar lăsa stocul real intact și ar produce un stoc virtual
negativ. Mutarea întregului lanț pe aceeași constantă ar suprapune MAG1 și
MAG2 la transferul aceluiași lot. Owner-ul a aprobat păstrarea gestiunii
reale, după alegerea tratării întregului lanț special și excluderea Custodie.

Invarianții I–IV sunt păstrați: operații prin documente, motor generic,
istoric append-only, coordonate cu semantică unică și politică în date.
Proba numerică, ciclurile și limitele implementării stau în catalogul LDI.
Cititorii comuni rămân TR-D8; eliminarea registrelor și propria evaluare
din cub rămân TR-D9. Nu se redeschide decizia cubului.
