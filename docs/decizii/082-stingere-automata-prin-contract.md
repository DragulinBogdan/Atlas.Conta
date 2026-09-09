# 82 — Stingerea automată prin contract de document

- **Data**: 2026-09-08
- **Stare**: activă
- **Docs**: `docs/invarianti.md` (II), `Module/BusinessObjects/Documente/Document.cs`, `Module/BusinessObjects/Documente/Trezorerie.cs`, `Module/Motor/ImperechereService.cs`, `Module/Motor/MotorOperare.cs`, `nou/tools/ModelCheck/Program.cs`

## Context

`MotorOperare.Opereaza` condiționa împerecherea automată prin
`doc is DocumentTrezorerie`. Regula contrazicea invariantul II: motorul
cunoaște mecanismele și contractele bazei, nu tipurile care participă.
Capacitatea de stingere există deja ca hook, dar nu înseamnă participare
automată: nota contabilă poate stinge manual fără să-și stingă sursa la operare.

## Tranșări

**(a)** `Document.SursaStingeriiAutomate(IObjectSpace)` declară sursa prin
`Guid?`; implicit `null`. Metoda nu scrie. `DocumentTrezorerie` participă
doar când este autogenerat, are sursă și `CapacitateStingere` nenulă.
Viramentele interne rămân excluse prin capacitatea existentă.

**(b)** `ImperechereService.CreeazaAutomataLaOperare` consumă contractul,
verifică `PoateFiStins` pe sursă și calculează minimul dintre disponibilul
documentului și restul sursei. Disponibilul păstrează formula existentă:
`Detalii.Sum(Valoare + ValoareTva) - Asignat`, cu liniile din ObjectSpace,
inclusiv modificările necomise. Sursa folosește `Ramas`. Suma pozitivă intră
prin `Creeaza`, cu validările și rotunjirea existente, `autogenerat: true`.

**(c)** Motorul cheamă serviciul după materializarea registrelor și asignarea
stării `Operat`/`DataOperare`, înainte de unicul său commit. Serviciul nu
comite și nu prinde refuzurile. `MesajeDupaOperare` rămâne informare după
commit. Schema, DTO-urile și comportamentul de stingere nu se schimbă.

## Review

- Participarea explicită nu înscrie automat orice document cu capacitate de
  stingere și sursă; NTC păstrează comportamentul manual.
- Ambele roluri rămân verificate: copilul recules ca trezorerie obișnuită
  poate avea încă un virament ca sursă; sursa trebuie să permită stingerea.
- Un refuz după materializare nu comite nimic. Instanțele din ObjectSpace
  pot fi modificate în memorie, ca înainte; apelantul trebuie să abandoneze
  ObjectSpace-ul comenzii refuzate. Nu se promite rollback în memorie.
- Calculul local al disponibilului nu este înlocuit cu o citire SQL a
  liniilor persistate; proba 40 persistat → 30 necomis apără această graniță.

## Verificări

ModelCheck păstrează probele de plată automată integrală, reoperare și
virament fără împerechere. Probele 82 adaugă NTC cu sursă și capacitate dar
fără stingere automată, plată neautogenerată cu sursă, valoare necomisă,
plafonare la rest parțial, rest zero și refuz de contrapartidă fără operare,
număr, registre sau împerechere persistate (recitire într-un ObjectSpace nou).

Închidere (2026-09-09): build ModelCheck și WebApi reușit; ModelCheck bugetar
**926 OK / 0 FAIL**, privat **977 OK / 0 FAIL**, ambele cu exit code 0.
`pnpm verifica:drift` a trecut, fără schimbări în artefactele generate;
verificarea metadata este inclusă în ModelCheck. Șapte probe noi pentru 82.

La rularea privată, `TEMP`/`TMP` au fost direcționate într-un director de
lucru scriibil: exportul SAF-T scrie fișiere și rulează DUK. O întrerupere a
scenei SAF-T înainte de curățenie lasă date care influențează scenele D300/
D394 la reluare; rularea finală de mai sus a pornit după curățarea reziduurilor
propriei probe din baza dedicată `Atlas.Conta.ModelCheck.Privat`.

## Ce rămâne deschis

Nicio extensie funcțională nouă. Concurența operării și alte dependențe ale
motorului de tipuri rămân în afara acestei mutări restrânse.
