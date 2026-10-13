# TR-D9b, pasul 1 — inventarul originii partidei

Tip: analiza
Data: 2026-10-12
Alimentează: contractul TR-D9b, D9B-D1…D7

Inventar read-only pe `96ccc5acbb4048d7ba41f60924863d631e83daca`.
CodeGraph a precedat căutările de apeluri și completarea intervalelor
omise. Nu s-au citit baze și nu s-au executat probe de produs.
[Contractul](tr-d9b-contract.md) spune ținta; această pagină spune starea
găsită și riscurile care justifică probele.

În tabel, `Module/` înseamnă
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`, iar `Nucleu/`
înseamnă `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`.

## 1. Identitate, producători și materializare

| Loc | Fapt verificat | Consecință pentru pas |
|---|---|---|
| `Nucleu/Cub/Unitate.cs:27–31,43–52` | ID determinist document/cont/partener; referința inițială are discriminator propriu; data nu intră în hash | se păstrează identitatea; nu se generează alt ID în tabela originii |
| `Module/Declaratii/Partide.cs:15,24,38` | helper-ele disting partida proprie, sursa și nominalizarea | originea stingătorului nu se atribuie partidei sursei |
| `Module/Declaratii/DeclarantTrezorerie.cs:82–122` | propria se construiește per cont/titular; `proprieDeclarata` este un singur boolean pentru declarație | lista deciziilor nu este inventarul sigur al tuturor partidelor finale; SC-D9B-03 |
| `Module/Declaratii/DeclarantNotaContabila.cs:61–91` | FIFO/pin pe latură; ce rămâne deschide partida proprie; deduplicare pe ID | păstrarea regulilor 115, inclusiv excedentul |
| `Module/Cub/Transferuri.cs:45` | transferul poate construi partida proprie lipsă din setul primit | materializarea originilor trebuie să acopere și transferul |
| `Module/Cub/Materializare.Deschidere.cs:106–109,142–146` | fiecare intrare inițială produce unitate și postare; apoi trece prin `Scrie` | referința inițială trebuie transmisă explicit; nu se poate recupera din hash |
| `Module/Cub/Materializare.cs:40–59,343–358` | postările finale ale contractului trec prin singurul `Scrie`; acesta setează tranzacția și ordinalele | punct central de materializare pe set, în tranzacția apelantului |
| `Module/Cub/Randuri.cs:119–132` | unitatea citită se recompune din postare, cont, titular/produs și data de deschidere | primul pachet nu cere mutarea tuturor atributelor unității în tabelă |

Recensământul apelurilor `Partide.Proprie`/`Numeste` în producție include
declarantul FCT, FCL, NIR și diferența NIR, RLF, RDC, DVI, DEC, NTC și
trezoreria PLT/INC. Se verifică pe postările finale ale fiecărei căi,
nu doar pe factură/plată. Import1C calculează ID-uri în `HandlereNote` și
`DeschidereCub`, dar scrierea trece prin comenzile/materializarea curente;
conectorul nu devine un al doilea scriitor de origine.

## 2. Cititori și ordine

| Loc | Fapt verificat | Consecință |
|---|---|---|
| `Module/Cub/Citiri/Partide.cs:252–262` | `Origini` și `OriginiDocument` caută postările proprii prin ID calculat și `Distinct`; prima include deschiderea | ținta F27-r16/F28-r1 este încă prezentă după C111 |
| același fișier, `125–162,214` | `Proprii`, `PropriiAle`, `CuOrigine`, `MiscariPePartidele`, `NominalizataLibera` depind de origine | mutarea centrală acoperă și calculele operaționale, nu doar lista |
| același fișier, `264–292` | soldurile folosesc mișcări/snapshot; `Deschideri` citește minimul ordinalului fără document | proiecția originii nu preia soldul; ordinalul se păstrează exact |
| `Module/Cub/Citiri/CumulPerioade.cs:27–43` | `Vizibila` citește mișcări; `Integrala` poate folosi snapshot | nu se servește snapshot integral unui rol restrâns |
| `Module/Proiectii/ImperecheriProiectii.cs:136–156` | resturile îmbină originea cu document, cont și repartitor în ObjectSpace-ul primit | noua origine nu justifică ocolirea securității |

Apeluri suplimentare de portat/verificat: `Motor/SolduriService.cs:122`
(solduri cu origine), `Motor/ImperechereService.cs:137` și
`Cub/Materializare.cs:163` (țintele împerecherii), `Cub/Citiri/Plati.cs:109`
(originea plăților), `Motor/Fapte.cs:187` (ordinalul pentru FIFO).
`tools/ModelCheck/ScaraVolum.cs:144,175` numără originile și trebuie să
certifice popularea proiecției în fixture-ul de volum.

Amendamentul care fixează ordinea curentă este **117**, nu forma inițială
116: ordinalul persistat departajează deschiderile, nu ID-ul postării.
SC-NTC-09/13/23…26 și SC-CIT-112/113 rămân martori numerici independenți.

## 3. Retragere, storno și securitate

`Materializare.Anuleaza` (`Module/Cub/Materializare.cs:246–264`) verifică
dependenții, apoi șterge postările și tranzacțiile `Operare`/`Transfer`
ale documentului. `DependentiPartide` (`304–327`) citește efectele altor
documente, iar `MotivPartide` (`298–302`) judecă netul pe evoluția temporală.
De aici rezultă riscul unui FK global pe unitate: postări străine cu net
zero pot rămâne după o retragere permisă. SC-D9B-12 trebuie să confirme
montajul prin comenzile reale; posibilitatea este dedusă din gard, nu
raportată ca probă executată.

Stornoul (`83–132`) păstrează originalul și scrie inversa; desfacerea
transferului (`217–230`) scrie alt transfer cu măsuri inverse. Niciuna nu
elimină suportul inițial. Stingerea unei partide inițiale are gard distinct:
`MotivStingereDeschidere` (`289–293`) refuză retragerea și cere storno.
Catalogul DESCHIDERE, SC-DES-13, afirmă aceeași regulă.

Securitatea actuală filtrează și originea prin `Partide.Postari(os)`, nu
doar soldul final. Un join direct cu o tabelă citibilă integral ar schimba
acest comportament. D9B-D5 cere păstrarea vizibilității suportului propriu
și probe HTTP. Protecția scrierii trebuie extinsă la proiecție după modelul
verificat în `ScenariiConsumatori.Gard` (`127–161`): create/update/delete
refuzate pe ușa securizată, inclusiv pentru administrator.

## 4. Delimitarea constatărilor

Nu s-a demonstrat un defect de sold. Booleanul din trezorerie este o
limită a folosirii explicației drept inventar de persistare, nu dovadă că
lipsește o postare. Referințele istorice din cataloage la regimul dual și
la lipsa partidelor bugetare sunt texte vechi; PLT are și amendamentul
explicit din 2026-09-25 care activează probele pe ambele profiluri. Nu se
folosesc paragrafele istorice drept țintă și nu se rescriu aici în masă.

Inventarul acoperă căile nominale ale originii. Inventarul complet al
impactului redenumirilor, configurarea EF/securitate, forma SQL și pragurile
de performanță se verifică în pașii lor; nu sunt declarate implementate.
Forma de persistare este alegerea propusă în contract, nu o clasă existentă.
