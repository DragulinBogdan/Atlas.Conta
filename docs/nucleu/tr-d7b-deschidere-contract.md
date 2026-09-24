# TR-D7b — Deschiderea generică

- Data: 2026-09-24
- Stare: aprobat de owner 2026-09-24; implementat și verificat pe ambele
  profiluri; review advers solicitat separat. Decizia 094.
- Surse: T-D7 din `tr-d7b-tipuri-ramase-contract.md`, 090(d/g), 091(f/i),
  `docs/invarianti.md` I și III. Conectorul 1C rămâne înghețat (091-f).

## DES-B1 — Mecanismul deja aprobat

`Cub.Materializare.Deschide` primește solduri inițiale și detalierea lor,
într-o singură tranzacție `Fel=Deschidere`, fără document. Nu scrie direct
conectorul, nu există document fictiv. Data este comună postărilor.
Terții au partener și partidă pe cont, partener și referință de deschidere
stabilă, inclusiv per factură când sursa o oferă (090-d, 092).
Stocul are lot, produs, gestiune reală și cantitate.
Identitățile se dau generic, fără tipuri sau referințe 1C în motor.

## DES-B2 — Detalierea înlocuiește soldul bloc (aprobat)

T-D7 cere câte două postări pentru rândul contabil (cont contra 891),
apoi o postare valorică suplimentară pentru fiecare rând de stoc.
Tot T-D7 cere în probe balanță pe fiecare carte. În cod, `Conservare`
exceptează deja `Deschidere` de la egalitatea valorică și cantitativă
(C6); aceasta permite tehnic forma, dar nu rezolvă dublarea soldului.

Contraexemplu independent: sold contabil D 302 = 100 și stoc pe același
cont: lot A 4/40, lot B 3/60. Forma literală aditivă scrie D 302 100,
C 891 100, D 302/A 40, D 302/B 60. Cititorul contabil pe reuniunea
partițiilor vede D 302 **200**, deși soldul inițial este **100**;
debitul total este 200, creditul 100. Excluderea partiției Stoc din
balanță nu este o soluție: ar exclude și recepțiile normale unificate.
Diferențele istorice descrise în TR-r12 sunt dovezi de migrare,
nu o așteptare pentru un client greenfield (091-a/c/f).

**Regula aprobată: detalierea înlocuiește, nu suplimentează.**

- Soldul contabil total este controlul pe `(Carte, Cont, Latura)`.
- Pe conturile detaliate, postările sunt chiar pozițiile de lot sau
  partidă. Totalul se verifică exact la ban; nu se mai scrie rândul bloc.
- Exemplul scrie D 302/A 40, D 302/B 60 și C 891 100:
  cont 302 = 100, lot A = 4/40, lot B = 3/60, D=C=100.
- Exemplu terți: C 401 = 100 se detaliază în partidele F1 60 și F2 40,
  cu D 891 100. Nu apare și o a treia postare C 401 100 fără partidă.
- Ancora vine ca identitate de cont din intrare/politică; motorul nu
  hardcodează simbolul 891. Dacă intrarea cuprinde deja corespondențele
  contra ancorei, nu o dublează.
- Un cont nedetaliat păstrează rândul contabil. Un cont cerut ca detaliat
  trebuie acoperit integral; nu se completează implicit un rest fără lot
  ori fără partener. Exemplu total 100, loturi 99: refuz atomic, diferența
  1 raportată cu cheia. Conectorul viitor va rezolva explicit divergența.
- Deschiderea de stoc gratuit (q pozitiv, valoare 0) păstrează cantitatea.
  Soldurile creditoare folosesc latura Credit și valoare nenegativă.
- Nu se schimbă excepția generică C6 a nucleului; constructorul acestui
  flux validează propria intrare și echilibrarea valorică pe carte.

Aceasta amendează forma aditivă din T-D7. Owner-ul a aprobat explicit
detalierea și refuzul diferențelor la 2026-09-24.

## DES-B3 — Probe obligatorii

1. Exemplul 302/891 de mai sus, citit pe cont și pe cele două loturi.
2. 401: două partide ale aceluiași partener și o partidă a altui partener;
   identități distincte și sume exacte. PLT ulterioară stinge numai partida
   indicată, fără document de deschidere fictiv.
3. Repetarea deschiderii refuzată, inclusiv două apeluri înainte de commit;
   rollback după refuz lasă baza neschimbată.
4. Solduri creditoare, stoc gratuit, cont/lot/partener absent, dată greșită,
   detaliere incompletă sau excedentară; refuz înaintea oricărei scrieri.
5. Citire la ziua anterioară, la deschidere și după PLT; conservarea
   originalului la storno-ul plății și refuzul stingerii peste rest.
6. Round-trip `Randuri` pentru postările fără document; unicitate în bază;
   citire pe set, fără interogări per poziție.

Catalogul numeric este `scenarii/DESCHIDERE.md`.
TR-D8 poate fi inventariat între timp; portarea balanței nu ascunde
contradicția printr-un filtru special pentru deschidere.

## DES-B4 — Intrarea și granița comenzii

Intrarea cuprinde soldurile de control, inclusiv ancora contabilă aleasă
de apelant; motorul nu adaugă o ancoră implicită. Cheia soldului este
Carte/Cont/Latura, cu marcaj explicit pentru detaliere obligatorie.
Conturile cu RolTert cer întotdeauna partide; restul pot fi solduri bloc.
Loturile cer gestiune reală, produs cu cont implicit corespunzător, data
lotului cel mult data deschiderii și cantitate pozitivă; valoarea poate
fi zero. Referința stabilă a partidei nu este FK spre Document.
Datele unităților de partidă sunt data deschiderii; identitatea include
contul, partenerul și referința, conform 092.

Materializarea participă la tranzacția apelantului, fără commit propriu;
cere tranzacție explicită și perioadă deschisă. Refuzul nu adaugă entități
de cub în ObjectSpace. Un index unic filtrat permite cel mult o Deschidere
în bază; apelurile repetate înainte de commit sunt refuzate și în memorie.
Stingerea unei partide de deschidere participă la aceeași graniță de
comandă, cu blocare și verificarea restului la data cerută.

## Implementare și verificare

`Materializare.Deschidere.cs` materializează intrarea; overload-ul din
`Materializare.StingereDeschidere.cs` mută valoarea de pe partida proprie
a stingătorului pe partida inițială. Scrierea dezactivează temporar lazy
loading în EF și restaurează starea lui: 2/51 loturi cer 6/6 interogări,
nu o citire de Tranzactie pentru fiecare postare nouă.

Migrația canonică `20260923225546_DeschidereUnica` adaugă indexul unic
filtrat pe Fel=Deschidere. Verificată pe cele două baze `.CodexBCS`;
proba cu două sesiuni reale confirmă unicitatea la commit.

Integral: **2.353 bugetar / 3.402 privat**, zero FAIL, exit 0, build fără
avertismente, `run-verificari/20260924-021417-392/rezultat.json`.
Comenzile și încercările intermediare: `scenarii/DESCHIDERE.md`.
Cititorii de producție, consumul stocului inițial prin FIFO și ușile
UI/HTTP nu sunt declarate implementate de acest mecanism. 1C rămâne
înghețat; nu s-a schimbat conectorul și nu s-a rulat importul.
