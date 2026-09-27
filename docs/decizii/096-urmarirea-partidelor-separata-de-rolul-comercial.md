# 96. Urmărirea partidelor este separată de rolul comercial

- Data: 2026-09-24
- Stare: aprobată de owner, activă; (b) amendat de 100 pentru cele trei conturi comerciale bugetare; (d) și 096-r1 depășite de 102
- Docs: `docs/nucleu/tr-d7c-dec-contract.md` DEC-B2; 090(d), 092, 095.

## Regula durabilă

(a) `Cont.UrmarestePartide` stabilește dacă motorul urmărește restul pe
unități de partidă. `Cont.RolTert` rămâne clasificarea Client/Furnizor
folosită de SAF-T. Proprietățile sunt independente.

(b) Seed-ul păstrează urmărirea conturilor comerciale deja acoperite și o
activează pe 542 privat, respectiv 542.01.00/542.02.00 bugetar. Simbolurile
stau exclusiv în politica profilului. Conturile manuale pot activa
urmărirea fără să primească un rol comercial.

(c) Angajatul este parte externă patrimoniului în contractul laturilor.
Partida DEC îl urmărește pe titular; nu îl transformă în Furnizor și nu
introduce implicit furnizorul de pe bon ca altă identitate în decont.
Identitatea partidei rămâne document/cont/partener (092).

(d) Migrația inițializează noul atribut pentru conturile cu rol comercial
existent, inclusiv cele manuale. Nu rescrie postări istorice fără unitate.
Lipsurile istorice se raportează înaintea activării cititorilor; seed-ul
nu ține loc de rematerializare a istoriei.

## Context și alegere

542 avea RolTert=Niciunul în ambele profiluri. DEC 121 și PLT 50 lăsau
sold 71, dar fără partidă pe cub. Proprietatea comercială nu putea fi
extinsă la urmărirea angajatului fără a schimba semantica SAF-T.
Owner-ul a aprobat explicit `Cont.UrmarestePartide`.

## Restanțe

Implementarea intră în DEC, urmărită prin 095-r1. Tratarea istoriei fără
unitate la activarea cititorilor este urmărită prin 096-r1 și explicită în
contractul TR-D8.
