# DVI — baza vamală distinctă în cartea fiscală

2026-09-23. **Direcție aprobată de owner:** baza distinctă în `Carte=Fiscal`,
în același cub, cu echilibrare proprie și fără efect asupra balanței
contabile sau partidelor. **Forma concretă de mai jos este pregătită pentru
review, înainte de implementare.** DVI nu este încă activat pe cub.

Completează T-D8 din `tr-d7b-tipuri-ramase-contract.md` și B-D8 pct.5 din
`tr-d6b-declaratia-fluxului-contract.md`. Execută 090(a/k) și amendamentul091;
nu schimbă reprezentarea fiscală a FCT/FCL/retururilor. T-r10 se închide
numai după implementare și probe, nu prin aprobarea direcției.

## DVI-B1 — Faptul care trebuie păstrat

Pe DVI, `DocumentDetaliu.Valoare` este baza vamală culeasă, iar
`ValoareTva` este taxa culeasă sau calculată (086). Baza100 cu taxa21
produce numai nota contabilă4426=446:21. Baza nu se poate recupera exact
din taxă/cotă:100 și100,01 pot produce aceeași taxă21 după rotunjire.

Cubul trebuie să păstreze independent ambele cifre, fără citirea
valorilor din document pentru reconstruirea jurnalului fiscal. Identitatea
documentului, MRN-ul și celelalte atribute descriptive se citesc în
continuare prin `Cauza`, conform B-D8 pct.5.

## DVI-B2 — Cele patru postări, fără conturi noi

Nota de taxă folosește politica existentă. Mișcarea de bază se echilibrează
pe **același cont de TVA deductibil** în cartea fiscală. Contul se rezolvă
din `TipTvaFapt.ContTvaDeductibilId`;4426 este exemplul profilului, nu o
constantă în declarant. Nu se adaugă un cont fictiv în plan sau un GUID
fără nomenclator.

Exemplu: bază100, tipIMP21, taxa21, contrapartidă446:

| Carte | Latură | Cont | Valoare | CodTva | Partener | Unitate |
|---|---|---|---:|---|---|---|
| Contabil | Debit | 4426 | 21 | IMP21 / Achiziție / Taxă | predatorul DVI | null |
| Contabil | Credit | 446 | 21 | null | numai dacă se deschide partidă | numai dacă446 are RolTert |
| Fiscal | Debit | 4426 | 100 | IMP21 / Achiziție / Bază | predatorul DVI | null |
| Fiscal | Credit | 4426 | 100 | null | null | null |

Creditul fiscal echilibrează măsura de bază. Nu este datorie, TVA colectat,
venit sau bază negativă în jurnal; nu poartă `CodTva`. Cele două postări
fiscale au sold net zero inclusiv pe contul4426. Rulajele lor de100 sunt
vizibile la inspectarea cărții fiscale și nu reprezintă rulaje de taxă.

Ambele postări fiscale au aceeași `Cauza(document,linie)`, aceeași dată de
înregistrare, `Gestiune` internă a DVI și `Analiza` liniei. Cantitatea și
valoarea în valută sunt0; `Produs`, `Unitate`, `Valuta`, `Atribuit` sunt
null. Numai debitul fiscal are perioada declarării și partenerul fiscal;
creditul fiscal are și `PerioadaDeclarare=null`. Ambele sunt în
`Spatiu.Contabil`: spațiul este derivat din unitate, iar cartea fiscală
nu este o partiție nouă.

O linie obișnuită produce două mișcări, patru postări, într-o singură
tranzacție `Operare`. Fiecare carte se balansează separat. Mișcarea bazei
este `Operare`, nu `Transfer`: înregistrează un fapt fiscal nou.

**Consecința alegerii contului:** rolul Bază, cartea și codul fiscal sunt
semantica măsurii100. Un raport care numește orice rulaj4426 „TVA” fără
carte/rol este incorect. Alegerea evită o nouă familie de conturi tehnice,
cu prețul acestei discipline explicite de citire.

## DVI-B3 — Variante și reguli de unicitate

- Contrapartidă401 prin comisionar: taxa21 deschide partida proprie
  `(document,401,partener)` cu rest−21. Baza100 nu trece prin
  `Partide.Numeste`/`CuPartida`; ambele capete fiscale rămân fără unitate.
- 446 cu RolTert: aceeași regulă, numai taxa deschide partida. În seed-ul
  privat actual446 nu are RolTert; nu se schimbă seed-ul contului pentru
  a forța o partidă în scenă. `PoateFiStins=false` rămâne T-r5/86-r11.
- Taxare inversăIMPTI21: contabil4426=4427:21, baza fiscală100 în aceeași
  pereche ca mai sus. Creditul4427 rămâne fără fapt colectat; B-r4 nu se
  rezolvă în această felie.
- Bază0,01 cu taxa calculată0: numai cele două postări fiscale de0,01;
  există tranzacție operată, nu există postări de taxă sau partidă.
- Taxa culeasă21,03 la baza100 se păstrează după politica existentă de
  toleranță; baza rămâne100. Taxa nu se copiază și în `Carte=Fiscal`.

Per `(tranzacție,linie,tipTVA,sens)` există exact un fapt Bază și cel mult
un fapt Taxă. La DVI baza este în cartea fiscală, taxa în cea contabilă.
La celelalte tipuri, faptul de bază deja atașat netului rămâne acolo:
nu se adaugă o copie fiscală. Aceasta completează B-D8 pct.5 pentru
documentul fără net, fără o normalizare care ascunde valori lipsă.

## DVI-B4 — Contractul citirii

| Citire | Selecție și măsură |
|---|---|
| Balanță/rulaje/fișă contabilă | `Carte=Contabil`; regulile existente pentru felul tranzacției rămân |
| Rest/FIFO/stingeri | `Carte=Contabil`, unitate Partidă; baza nu are unitate |
| Jurnal/decont TVA | toate cărțile, numai `CodTva != null`; grupare pe sens, tip, rol, perioadă și dimensiunile cerute; sumă din `Valoare` semnată |
| Conservare | separat pe fiecare `Carte`, pentru toate postările inclusiv echilibrarea |
| Audit | toate cărțile; păstrează cauza, latura, rolul, perioada și măsurile |

În jurnalul fiscal nu se aplică semnul debit−credit: valoarea este deja
semnată, iar sensul Achiziție/Livrare și rolul Bază/Taxă sunt coordonate.
Altfel vânzările creditate și retururile ar avea semne greșite.

Schimbare concretă identificată: `ReconciliereCub.Contabile` agregă astăzi
toate cărțile; va cere `Carte=Contabil`. `Fiscale` și `Storno` trebuie să
citească ambele cărți, numai faptele cu cod, iar `Balanta` rămâne per carte.
Se verifică și cititorii de partide și helper-ele scenelor; un filtru pe
`Spatiu.Contabil` singur nu selectează cartea contabilă.

Portarea rapoartelor de producție rămâne TR-D8. Aici se probează proiecția
numerică direct din postări, fără citirea cifrelor din document sau
preluarea așteptărilor din registre. Comparația suplimentară cu registrele
nu înlocuiește aceste așteptări.

## DVI-B5 — Ciclu de viață și date

Anularea în perioada deschisă elimină atomic ambele mișcări; reoperarea
reproduce aceleași fapte o singură dată. Stornoul inversează toate cele
patru postări cu mecanismul comun, fără schimbarea laturii/cărții și fără
atingerea originalelor. Taxa0 inversează cele două postări de bază.

Storno comercial în februarie al DVI din ianuarie:−100/−21 fiscal în
februarie,100/21 rămân în ianuarie. Corecție pentru eroare materială în
februarie la baza80: invers−100/−21 și nou80/16,80 declarate în ianuarie;
net fiscal ianuarie80/16,80, postări contabile ale corecției în februarie.
Ambele roluri folosesc aceeași politică a perioadei declarării (088).

Document întârziat, fără corecție: dacă ianuarie este încă deschis,
baza și taxa se declară în ianuarie; dacă ianuarie este închis, politica
deductibilă a DVI le declară în februarie. Toate postările sunt
datate la înregistrarea din februarie. Catalogul separă aceste fixture-uri
(SC07/18); regula nu se deduce doar din `Data != DataInregistrare`.

Gardul partidelor cu dependenți rămâne comun: dacă taxa a deschis o
partidă consumată prin NTC, anularea/stornarea refuză atomic înainte de
inversare. Baza fiscală nu creează sau eliberează dependenți de partidă.
Legăturile `DviFactura` sunt evidență; nu nominalizează partida FCT și
nu schimbă baza declarată. Protecția FCT legate,86-r2, rămâne distinctă.

## DVI-B6 — Date de intrare și suprafața implementării

`DeclarantDvi` rămâne pur și lucrează numai cu operandul închis. Faptul
`TipTvaFapt` primește `DeImport`, proiectat în interogarea pe set deja
existentă din `Fapte.TipuriTva`; azi câmpul lipsește din fapt, deși există
pe entitate. Nu se verifică importul după prefixul codului. Nu se adaugă
interogări per linie sau coloane persistente în cub.

Declarantul validează liniile, baza pozitivă, tipul de import cu cotă
pozitivă, politica Deductibil și conturile rezolvabile. Folosește calculul
de taxă și `Fiscal.Impozitul` comune. Noul helper al bazei, dacă se extrage,
stă în `Declaratii/Fiscal.cs` și este apelat explicit de DVI, fără
`switch` după tip în motor. Nu se creează RegulaContare de net pentru DVI.
MRN-ul și starea facturilor legate păstrează validarea actuală a entității;
acoperirea celor două uși se notează distinct în catalog.

Suprafață prevăzută: declarantul și override-ul DVI, helper fiscal,
`TipTvaFapt`/maparea sa, coduri stabile de refuz, seed privat,
`ScenariiDvi` și înregistrarea scenei, helper de comparație care include
`Carte`, curățenia `DviFactura`, filtrele cititorilor/gate-ului atinse.
`Conservare`, `Miscare`, `Postare`, `Storno` au deja forma necesară și nu
cer modificarea modelului pur sau a schemei EF pentru această propunere.

## DVI-B7 — Probe obligatorii înainte de activare

Catalogul numeric este `scenarii/DVI.md`, SC-DVI-01…18. În plus față de
ciclul complet: bazele100 și100,01 cu aceeași taxă21 rămân distincte;
taxa culeasă21,03 nu schimbă baza100; baza0,01 cu taxă0 supraviețuiește;
partida401 rămâne−21; multicota păstrează100/21 și50/5,50.

Se compară exact toate coordonatele postărilor, inclusiv `Carte` și
creditul de echilibrare fără cod. Se asertează rulajele contabile21/21,
nu doar soldul net care ar ascunde100 debit+100 credit în plus. Se
verifică invarianta per carte, unicitatea faptelor fiscale, lipsa
unităților pe perechea bazei și proiecția fiscală înainte/după storno.

Gate: `verifica.ps1 -Suita Scenarii -Tip DVI -Profil Ambele -Sufix .CodexBCS`,
apoi `-Suita Integral -Profil Ambele -Sufix .CodexBCS` și suita Nucleu;
secvențial, cu manifest, zero FAIL și curățenie pe marcaj. Pe bugetar se
probează explicit profilul inert. Zero DVI pe Flax rămâne consemnat;
Import1C nu se rulează ca probă supremă (091).

## Limite și punct de review

Nu intră DVI pe loturi/SC-X-13, reevaluarea, faptul colectat B-r4,
stingerea directă DVI sau portarea generală a rapoartelor. Nu se schimbă
declaranții deja validați pentru a muta toate bazele în cartea fiscală.

**Punctul concret de review este DVI-B2:** contul TVA deductibil drept
ancoră pentru debitul și creditul fiscal egale, cu rol Bază numai pe
debit. Restul disciplinei rezultă din separarea pe cărți aleasă de owner.
Implementarea nu a început; acest contract nu afirmă probe deja trecute.
