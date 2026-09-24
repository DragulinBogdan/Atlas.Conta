# 99. Cauza diferenței dintre evidență și constatarea fizică

- **Data**: 2026-09-24
- **Stare**: aprobată de owner, activă; concretizează 098(a) și 098-r1.
- **Docs**: `docs/nucleu/scenarii/NIR.md`, `docs/nucleu/scenarii/LDI.md`,
  098, 093, T-D13 (felul permis per latură pe tip), 088 (motivul decide efectul).

## Regula durabilă

(a) **Cauza diferenței este o noțiune generală a domeniului, nu a NIR.**
Ea clasifică diferența dintre ce spune evidența (factura, scripticul,
transferul) și ce s-a constatat fizic. Valorile, pe direcție:

- minus: `Perisabilitate` (în limitele normale), `Imputabila`,
  `Neimputabila`, `PeDrum` (proprietate transferată, bun în circulație),
  `InClarificare`;
- plus: `Plus`.

(b) **Structura în cod, efectul în politică.** Cauza este un enum pe linia
de diferență. Setul permis se declară per tip, pe tiparul T-D13: NIR
permite toate valorile, LDI nu permite `PeDrum` și `InClarificare`.
Contul (și, ulterior, efectul fiscal) vine din politică pe
tip × cauză × clasa materialului; niciun simbol de cont în motor.

(c) **`Imputabila` cere partenerul pe linie**: persoana căreia i se impută
diferența (furnizor, transportator, salariat). Partida se deschide pe el.

(d) **Avansul nu este o cauză de diferență.** Factura primită înaintea
livrării, fără transferul proprietății, este o proprietate a facturii
(linie de avans), nu a recepției. O FCT de avans nu postează recepție.

(e) **Subsetul implementat acum este al NIR (098a).** Delta minus are
implicit `InClarificare`, delta plus `Plus`. Politica seed-ului privat:
`InClarificare` → 473, `Plus` → 408, `PeDrum` → 32x pe clasa materialului
(301→321, 302→322, 303→323, 371→327, 381→328), `Imputabila` → 461,
`Perisabilitate` și `Neimputabila` → 6xx pe clasa materialului. Pe bugetar,
conturile se confirmă pe planul OMFP 1917 înaintea seed-ului (098-r1).

(f) **Schimbarea lotului pe conex se refuză.** Lotul e născut de linia FCT;
un lot greșit se corectează pe factură, nu prin NIR (argumentul lui (d)).
Delta nu inventează 473/408 pentru o simplă reidentificare de lot.

(g) **O singură recepție activă per FCT, cumulativă.** Conexul activ descrie
totalul constatat. Sosirea ulterioară (inclusiv `PeDrum`) este corecția
conexului: stornoul inversează delta anterioară, documentul nou o
recalculează pe cumul. Corecția păstrează proveniența absorbției față de
sursă, independent de `Autogenerat`.

(h) **Pe conexul acoperit liniile sursei nu se șterg.** Cantitatea poate fi
zero și poartă cauza; relaxarea e locală conexului, NIR-ul manual rămâne
strict. O singură cauză pe linie; două cauze pentru aceeași lipsă se
refuză în subsetul acesta.

## Context

La tranșarea 098(a), delta NIR avea nevoie de contrapartidă pe cauză:
marfa pe drum, lipsa imputabilă, perisabilitatea și cauza încă
necunoscută au conturi diferite. Aceeași clasificare lipsește la LDI,
unde orice minus e tratat azi ca consum: lipsa imputabilă salariatului și
perisabilitatea în limite nu se pot exprima. Un câmp numai pentru NIR ar fi
produs ulterior o a doua clasificare aproape identică pentru inventar.
Owner-ul a ales noțiunea generală, cu implementarea incrementală.

## Restanțe

- 099-r1: LDI adoptă cauza diferenței (minus imputabil pe partener,
  perisabilitate, neimputabilă; plus).
- 099-r2: efectele fiscale per cauză (ajustarea TVA, deductibilitatea).
- 099-r3: BTR cu lipsă la primire, pe aceeași clasificare.
