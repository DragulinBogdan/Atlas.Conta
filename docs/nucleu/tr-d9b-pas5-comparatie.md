# TR-D9b, pasul 5 — comparația A/B și review-ul advers

- Tip: analiza
- Data: 2026-10-12
- Contract: [TR-D9b](tr-d9b-contract.md), D9B-D7
- Alimentează: acceptarea primului pachet al originii partidei
- Stare: verificat și reverificat de Claude, cu o corecție de produs; comis la cererea owner-ului, 2026-10-13

## Protocol și proveniență

Comparația folosește exclusiv baza privată
`Atlas.Conta.ModelCheck.Privat.CodexD9bB`, Postgres nativ pe 5446.
Referința istorică `.CodexD9bA` este păstrată. Scena nouă este produsă prin
documentele și scriitorul B; amplificarea sintetică pornește din fotografia
acelor fapte și origini. Nu se reconstruiesc origini lipsă dintr-o bază A.

Cele patru trepte sunt 1, 10, 100 și 629, până la 5.001.179 postări. Copierea
păstrează conturile, multiplică documentele și titularii, recalculează
identitățile deterministe, inclusiv referințele partidelor inițiale, și
continuă ordinalele deschiderii. Invariantul de origine verifică fiecare
treaptă. Limita montajului din pasul 3 rămâne: copiile nu sunt documente
operaționale complete, fiindcă liniile fiscale și detaliile nu sunt
multiplicate. Nu se rulează invariantul global al explicațiilor pe ele.

`D9bCititorA` păstrează cititorul anterior numai în ModelCheck. SQL-ul lui
de domeniu trebuie să coincidă cu captura înghețată din
`nou/tools/ModelCheck/Referinte/D9bA/`; diferența oprește proba. Ambele
variante folosesc LINQ/EF, aceleași DTO-uri, aceeași materializare și
aceleași date. Pe ușa securizată, autentificarea și citirile de permisiuni
sunt cele curente, cu utilizatorul real al bazei B.

Se măsoară separat origine/sold/rest × integral/pagina 50 × sistem/admin
securizat. Ușa de sistem este cea a hosturilor: ObjectSpace-ul nesecurizat
al furnizorului securizat (`CreateNonsecuredObjectSpace`), nu furnizorul
simplu al harness-ului. Fiecare serie are o încălzire și cinci măsurări; se compară
medianele. Ordinea seriilor A/B se inversează între pagini și trepte.
Hash-ul rezultatului se calculează în afara cronometrării, dar include
toate câmpurile DTO, etichetele, ID-urile și nulurile. SQL-urile și planurile
`EXPLAIN (ANALYZE, BUFFERS)` sunt păstrate; manifestele au numărul și
amprenta faptelor, schema, binarele, configurația și dimensiunile originii.

Scriitorul A este compilat într-un worktree separat din aceeași fotografie,
cu `Cub/Materializare.cs`, `Cub/Materializare.Deschidere.cs` și
`Cub/Citiri/Partide.cs` restaurate din
`96ccc5acbb4048d7ba41f60924863d631e83daca`, aplicând doar redenumirile CLR
ale pasului 2. Schema și modelul B sunt păstrate pentru compatibilitate.
Nu apare comutator A/B în produs. Proveniența celor trei surse este în
`run-verificari/d9b-writer-a/writer-a-source.json`.

Comenzile A/B folosesc aceleași două note cu 2/51 partide pe baza B.
Fiecare rundă operează prin comanda reală, verifică 200/5100 și retrage în
`finally`; următoarea rundă pornește din Draft fără postări proprii.
`scriere-mediu.json` păstrează hash-urile binarelor și amprentele integrale
ale postărilor/originilor înainte și după lot. Sinteza cere aceleași ID-uri
de document și `FapteDupaA = FapteInainteB`, plus pragul scrierii și o
singură interogare de origine pentru fiecare comandă B.

## Rezultate A/B

Cifrele sunt ale rulării de reverificare, pe o bază B nouă; prima rulare
este descrisă la [reverificare](#reverificarea-claude).

**48/48 perechi egale și toate pragurile de citire verzi**, în
`run-verificari/20261012-201658-993/`. Toate cele 576 de execuții au rezultat
stabil în propria serie; planurile cerute au fost obținute. Amprenta
postărilor la treapta maximă este `9dd92bbeefb84b8855c26ac704356e2c`,
neschimbată după citiri și după comenzile de scriere.

Rest integral, ușa de sistem, mediane în milisecunde:

| Postări | A | B |
|---|---:|---:|
| 7.951 | 34,78 | 4,59 |
| 79.510 | 138,70 | 39,26 |
| 795.100 | 1.217,26 | 513,10 |
| 5.001.179 | 11.952,96 | 3.664,04 |

La maxim, reducerea este **69,35%**, iar pragul este 9.562,37 ms. Ambele
variante livrează 488.733 rânduri. Planul normal trece de la două scanări
Postare, circa 6,13 milioane rânduri atinse și 188.227 buffers, la o scanare,
circa 3,60 milioane și 92.128 buffers. Alocările pentru DTO-ul integral rămân
aproape egale: 269,88/269,87 milioane octeți; schimbarea reduce descoperirea
în SQL, nu dimensiunea rezultatului.

Restul citirilor la maxim, mediane în milisecunde:

| Citire | Sistem A | Sistem B | Admin securizat A | Admin securizat B |
|---|---:|---:|---:|---:|
| Origine integrală | 6.828,85 | 512,17 | 6.798,77 | 2.185,14 |
| Origine pagina 50 | 6.249,34 | 0,48 | 6.216,32 | 3,49 |
| Sold integral | 1.433,69 | 1.355,55 | 1.431,33 | 1.201,40 |
| Sold pagina 50 | 1,91 | 1,31 | 4,77 | 3,33 |
| Rest integral | 11.952,96 | 3.664,04 | 11.824,26 | 10.737,79 |
| Rest pagina 50 | 11.962,38 | 3,98 | 11.818,37 | 5,11 |

Originea B de sistem are zero scanări Postare; sistemul folosește o comandă
SQL, adminul securizat șase, inclusiv încărcarea permisiunilor. Tabela
conține 651.644 origini: 60.710.912 octeți tabelă și 95.305.728 indecși,
împreună 148,79 MiB. Citirea securizată păstrează costul verificării
suportului vizibil; câștigul ei nu este cel al ușii de sistem: restul
integral al adminului securizat scade cu 9,19%.

Restul paginat B depinde de planul ales: 3,98/5,11 ms aici, 466,91/498,30 ms
în prima rulare, pe aceeași formă de date. Cifra nu este o garanție; pragul
ei rămâne cel general, față de A.

Scrierea este măsurată în
`run-verificari/d9b-writer-a/run-verificari/20261012-204810-751/` pentru A și
`run-verificari/20261012-205013-264/` pentru B:

| Partide | A ms | B ms | Prag ms | Comenzi SQL A/B | Citiri origine B |
|---|---:|---:|---:|---:|---:|
| 2 | 18,3989 | 21,5931 | 28,3989 | 32/33 | 1 |
| 51 | 68,0272 | 63,5928 | 78,2313 | 32/33 | 1 |

Ambele trec. Pragul este `max(1,15 × A, A + 10 ms)`; pragul și calculul
automat nu au fost schimbate după rezultat. Mediana din cinci comenzi nu
desparte variantele la 51 partide: A măsoară 55,10–68,47 ms, B 58,29–80,86 ms,
iar în prima rulare A era 64,01 și B 73,57, cu marja de 0,45 ms. La 2 partide
B rămâne cu circa 3 ms peste A în ambele rulări, costul citirii de origine.
Nu se afirmă nici câștig, nici rezervă măsurată la scriere.

Sinteza automată este verde în `run-verificari/20261012-205201-474/`:
aceleași documente, bază, schemă și fapte între A/B, restaurare după lot,
același binar de produs B la citire/scriere. Rezumatul cu duratele și
amprentele este versionat în
[artefact](../../nou/tools/ModelCheck/Referinte/D9bAB/comparatie.json).
Build-urile au zero erori; compilarea completă A raportează nouă
avertismente în surse existente, iar compilarea harness-ului B șase.

## Review advers

Review independent Codex, în worktree separat, prin inspecția surselor,
contractului și probelor. Nu este reverificarea Claude a owner-ului.
Reviewer-ul nu a executat baze sau build-uri concurente cu benchmark-ul.
Raportul brut este `run-verificari/d9b-review/review-d9b.md`.

**D9B-R1, P2 — origine fără suport, mutant absent.** Mutanții lipsă și
origine străină aprindeau prima condiție a invariantului; a doua putea fi
eliminată fără să-i facă să treacă. Corecția adaugă
`ORIGINE-FARA-SUPORT`: origine cu ID determinist și FK-uri valide, pe un
Draft fără postări. Tranzacția mutantului se anulează. Reviewer-ul a
acceptat corecția de sursă. Integrala finală probează explicit mutantul pe
ambele profiluri, cu `ORIGINE_PARTIDA_INCONSISTENTA`.

**D9B-R2, P2 — proba restrictivă reintroducea SQL-ul vechi cu ramură de
nul.** Integrala intermediară `20261012-160751-906` a trecut scenariile și
mutanții bugetari, dar a refuzat `RAMURA-NUL` în apelul nou la
`D9bCititorA.PartideCuRest`. XAF maschează câmpurile Postare prin `CASE`,
iar îmbinarea veche între două surse adaugă ramura de nul pe ContId.
Alinierea filtrului Furnizor nu a rezolvat forma (rularea țintită
`20261012-161231-928`, păstrată ca eșec).

Proba revizuită compară întregul set al originilor A/B sub același rol,
ordonat după UnitateId. O a doua factură 70 pentru același titular rămâne
vizibilă: exact originea ei trebuie să existe în ambele seturi, iar
factura 100 ascunsă trebuie să lipsească. Resturile 40/70 și documentul nul
al facturii ascunse rămân așteptări independente pe B; HTTP din pasul 4
verifică ruta reală. Nu se mai execută îmbinarea completă veche sub rolul
restrictiv; comparația integrală a restului A/B la volum acoperă ușa de
sistem și adminul securizat. Reviewer-ul a acceptat această delimitare:
se compară direct sursa schimbată, cu un martor pozitiv și unul ascuns.
SQL-ul A înghețat, gardianul și lista excepțiilor nu se modifică.

Nu s-a identificat alt defect funcțional concret în inspecția scriitorului,
ciclului de viață, schemei sau securității. Harness-ul citirilor și
abordarea scrierii A/B au fost revizuite separat. Reviewer-ul a recalculat
independent cele 48 mediane, egalitățile, pragurile și hash-urile celor trei
surse restaurate pentru A.

**Verdict final: D9B-R1 și D9B-R2 închise, fără constatări blocante rămase.**
Reviewer-ul a verificat sursa finală și logurile ambelor profiluri din
integrala de mai jos. Verdictul privește inspecția independentă și
recalculul artefactelor; nu pretinde o rerulare independentă pe baze.

## Reverificarea Claude

**D9B-R3 — ușa de sistem a hosturilor lua ramura securizată.** Cititorul
recunoștea ușa de sistem după tipul ObjectSpace-ului. Hosturile o deschid
prin `CreateNonsecuredObjectSpace`, care întoarce același tip ca ușa
securizată, cu securitatea oprită pe context. Pe host, originea de sistem
era deci citită cu `EXISTS` peste `Postare`, contrar D9B-D5. Funcțional nu
ascundea nimic: pe un context fără securitate filtrul nu elimină rânduri.
Prima comparație (`run-verificari/20261012-153203-041/`) măsura seria de
sistem pe furnizorul simplu al harness-ului, pe care hosturile nu îl
folosesc; cifrele ei de sistem nu descriau produsul și sunt înlocuite.

Corecția: `Vizibilitate.Securizat` decide după starea securității pe
context și este folosit și de scrierile globale ale soldurilor, care aveau
deja predicatul corect. SC-D9B-10 cere acum, pe ușa de sistem a hostului
dintr-o sesiune restrictivă, SQL de origine fără `Postare` și originea
ascunsă rolului. Proba este roșie pe predicatul vechi
(`run-verificari/20261012-200939-286/`, păstrată ca eșec) și verde pe cel
nou. Seria de sistem a comparației rulează pe aceeași ușă.

Cu acordul owner-ului, filtrul de eligibilitate al scriitorului folosește
un delegat compilat o dată, din aceleași expresii ca `Partide.Postari`;
înainte, expresia se compila la fiecare comandă. Efectul nu se desparte de
zgomot în măsurarea scrierii.

Comparația, scrierea A/B și sinteza au fost rerulate integral pe o bază B
nouă; baza primei rulări este păstrată ca `.CodexD9bB.v1`. Scriitorul A este
cel din worktree-ul primei rulări, neschimbat.

## Validare finală

`verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bOrigine`
este verde în `run-verificari/20261012-201158-086/rezultat.json`:
**3.759 bugetar / 5.161 privat OK, zero FAIL**, cu același binar ca
rularea de comparație și scrierea B. Ambele loguri conțin
mutantul `INV-CUB-ORIGINE-FARA-SUPORT`, egalitatea A/B sub rol restrictiv
cu originea vizibilă 70 și cea ascunsă 100, proba ușii de sistem a
hostului, resturile numerice, curățenia și `RAMURA-NUL` verzi. Integrala
dinaintea reverificării, `run-verificari/20261012-161454-010/`, avea
3.758 / 5.160. Eșecurile intermediare rămân păstrate.

Proba HTTP a fost rerulată după corecție, pe host viu:
**327/327 PASS** și SC-D9B-10 verde, `run-verificari/20261012-205322-837/`.
Nucleul nu a fost atins; 199/199 la reverificare,
`run-verificari/20261012-183432-746/`. Infrastructura rămâne proba
pasului 4. Driftul OpenAPI este zero pe o bază migrată; baza de dezvoltare
fără sufix nu are migrația și se recreează (102b) înaintea hosturilor.

`docskit gen`, `docskit check`, `docskit metrics --check` și
`git diff --check` sunt verzi. Avertismentele documentare existente rămân;
linia de metrici nu a fost modificată. Ieșirile sunt păstrate în
`run-verificari/d9b-comparatie/docskit-check.log` și `docskit-metrics.log`.

## Reproducere și limite

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bComparatie -Profil Privat -Sufix .CodexD9bB
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bScriere -Profil Privat -Sufix .CodexD9bB
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita D9bRaport -Profil Privat -Sufix .CodexD9bB -Citiri <rulare-citiri> -ScriereA <rulare-scriitor-A> -ScriereB <rulare-scriitor-B>
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bOrigine
```

Prima comandă cere o bază B nouă, fără postări sau schema `scara`;
baza existentă se redenumește ori se șterge înainte.
Scrierea se rulează secvențial din cele două checkout-uri, cu același
sufix și aceleași date. Adaptorul local folosește pachetele offline și
condițiile descrise în pasul 4; nu modifică configurația versionată.
Pachetul a fost comis la cererea owner-ului, la 2026-10-13, fără altă rulare.
Acest pas privește pachetul originii; nu închide întreaga TR-D9b și nu
introduce registrele bugetare 111(f).
