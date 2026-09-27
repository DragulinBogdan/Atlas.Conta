# TR-D8 — intervale TVA și avertismente, R6

Data: 2026-09-26; revizie pentru review: 2026-09-27.
Stare: **implementat și verificat, review advers închis (2026-09-28)**. Regulile sunt 103(h) și
103(i). Owner-ul a aprobat M1(B) și M7 (§R6-B8) la 2026-09-27.
Surse: invarianții II/III/IV; deciziile 090(j), 103 și 104;
review-ul `2026-09-27-2207-claude-codex-r6-specificatie-review.md`;
SC-CIT-90…94 din `scenarii/CITIRI.md`.

## R6-B1 — intervalul și reperul

`TipTva.ValabilDeLa` / `ValabilPanaLa` sunt opționale, cu limite incluzive.
Null înseamnă capăt deschis. Un interval inversat se refuză la configurare;
ieșirea unui document din interval produce avertisment, nu refuz fiscal.
Nu deducem calendarul din cod, denumire sau procent la runtime și nu
inactivăm automat tipul când trece data de sfârșit.

Pentru faptul scris folosim exigibilitatea și calificarea înghețate în cub;
pentru draft, `DataExigibilitate ?? Data` și valorile pe care le-ar folosi
operarea, fără postări persistate. Intervalul comparat este cel curent.
Editarea cotei, regimului sau importului rămâne permisă; raportul distinge
calificarea istorică de cea curentă, fără recalcularea faptelor istorice.

## R6-B2 — taxa salvată și avertismentul aritmetic (M1)

**A, inclus în propunerea R6:** `TVA_TAXA_DIFERITA_DE_COTA` se emite la
operare și în raport. Înainte de B, L3 persista taxa calculată, iar operarea
păstra orice `ValoareTva` nenulă. Draftul salvat cu 100/19, după editarea
tipului la 21%, devenea fapt cu bază 100, cotă 21, taxă 19. Cu B, draftul
nemarcat preia taxa 21 la operare. Abaterea rămâne posibilă numai pentru
taxa culeasă, adică marcată. Raportul semnalează draftul marcat înainte de
operare. L3 oferă acțiunea explicită „Recalculează TVA la cotă”, comună
adaptorilor XAF și API, aplicabilă numai draftului și selecției explicite.

Calculul așteptat reutilizează regula existentă `Tva.PeDocument` (090j):
document × regim × cotă, repartizare pe linii și separat pe semne, cu
convenția de rotunjire a bazei de date. Nu introducem o a doua formulă
`Round(bază × cotă)` pe linie. Propunem prag diagnostic inclusiv de
**0,01 lei pe linie** față de taxa repartizată: abatere > 0,01 → avertisment.
Calificarea folosită este cea înghețată pentru operate, cea curentă pentru
drafturi. Schimbarea cotei din nomenclator produce separat impact
istoric/curent; un fapt corect 100/19 cu cotă înghețată 19 nu devine
aritmetic greșit după editarea la 21.

Comparația Bază/Taxă se aplică regimurilor care produc taxă: Normal și
TaxareInversa deductibilă. `Autocolectare` nu se adună încă o dată în taxă.
Capitalizat, scutit, neimpozabil și taxare inversă colectată nu primesc
abatere pentru absența unei taxe separate. Diagnosticul nu reconstruiește
TVA capitalizat dintr-o bază brută istorică. Intervalul și diferența de
calificare se verifică în continuare pe faptele disponibile.

`TolerantaTaxa` deja configurată în motor rămâne gardianul existent; R6
nu îl dezactivează și nu îi transformă refuzul în avertisment. Scenariile
cu taxă intenționat greșită și operare reușită folosesc toleranța neconfigurată.

**B, aprobat de owner 2026-09-27 (103i):** marcajul explicit
`DocumentDetaliu.TvaCules` (bool) înlocuiește proxy-ul „nenul = cules”.
A rămâne: marcajul decide ce taxă intră în fapt, iar avertismentul aritmetic
semnalează taxa culeasă care diferă de cotă.

- **Locul: baza.** Testul bazei (II) trece pe ambele condiții. Semantica
  „taxa liniei e dată de operator, nu calculată din cotă” e aceeași pe orice
  document `CuTva()`: FCT, FCL, DEC, DVI, RDC, RLF. Declarația o consumă
  direct la postare: `Fiscal.Valoarea` alege între taxa liniei și
  repartizarea nucleului (090j). `ValoareTva`, pe care o califică, stă deja
  pe bază. `LinieOperand` primește `TvaCules` alături de `ValoareTva`.
- **Zero nu se schimbă.** Marcajul acoperă numai taxa nenulă. Un 0 explicit
  pe un tip cu taxă rămâne refuzat, conform regulii C104 („0 nu e TVA
  cules”, `CulegereDocument.Mapata`). Lipsa taxei se alege prin tipul de
  TVA. Invariant: `TvaCules ⇒ ValoareTva ≠ 0`, cu CHECK în migrație.
- **Tranziții, numai în L3 (`CulegereDocument`), pe toate ușile:**
  - taxă nenulă introdusă de operator (editorul XAF sau `tvaCules` nenul
    în `Apply`) → `true`;
  - taxa adusă la 0 în ecran, baza mișcată (`IntrariBaza`, inclusiv
    `TipTva`) sau acțiunea „Recalculează TVA la cotă” → `false` și
    recalcul (`BazaSchimbata`);
  - linie nouă → `false`;
  - `ValoareTva = null` prin WriteDto, cu baza neschimbată, păstrează taxa
    și marcajul existente; revenirea la calcul se cere explicit;
  - clona conexului nu copiază taxa (`MotorOperare`, clona liniilor), deci
    rămâne `false`. Draftul de corecție copiază `ValoareTva` și `TvaCules`
    ale liniei sursă;
  - semnul aplicat de `PregatesteOperare` (RDC/RLF) nu schimbă marcajul.
- **Operarea.** `CalculeazaValori(pastreazaTvaCules: true)` păstrează taxa
  numai când `TvaCules`. Linia nemarcată primește la operare taxa
  repartizată pe cota curentă (`Fiscal.Valoarea`: `TvaCules ? ValoareTva :
  taxa.PerLinie`). Același criteriu înlocuiește testul `ValoareTva != 0`
  din gardul de taxă culeasă al `MotorOperare` (F13-D1/62f).
- **Inițializarea.** Migrația scrie `TvaCules = (ValoareTva <> 0)` pe liniile
  documentelor Draft. Păstrează comportamentul existent al fiecărui draft,
  fără să deducă intenția. Pe liniile operate, `false` nu are efect, fiindcă
  faptul e înghețat. Bazele de dezvoltare se recreează conform 102b.
- **Probe:** schimbarea cotei cu draft nemarcat și marcat (SC-CIT-91,
  SC-CIT-91b), editarea taxei, refuzul lui 0, recalculul explicit,
  corecția care păstrează marcajul, conexul fără marcaj, CHECK-ul refuzat
  de bază și driftul OpenAPI (`TvaCules` în `ReadDto` și în editorul XAF,
  doar în citire).

## R6-B3 — regularizarea avansului (T1, M4)

`TipMaterial.RegularizareAvans` este marca de politică, configurată numai
pe tip. Referința propusă devine **`LinieAvansId`**, opțională pe frunzele
FCT/FCL și expusă motorului prin contract de linie; nu intră pe baza
generică `DocumentDetaliu`. Aceasta precizează factura sursă din 103(h)
până la linia ei. Un singur FK autoreferențial pe tabela TPH și o singură
coloană fizică pentru proprietățile omoloage se probează în model.

Marca plus semnul negativ al bazei la **operare** înseamnă regularizare;
marca plus semnul pozitiv înseamnă avansul inițial, verificat normal pe
interval, fără cerință de referință. O linie nulă nu este regularizare.
Semnul inversei de storno nu reclasifică linia. FCT și FCL au aceeași regulă;
un retur nemarcat nu devine avans din cauza semnului sau a contului.

Lookup-ul XAF oferă liniile pozitive marcate, cu fapt fiscal activ, din
facturi operate ale aceluiași partener și sens. Referința este către alt
document. Se verifică și prin DTO/Apply; filtrul vizual nu ține loc de
validare. Faptele sursei se citesc exact prin `(DocumentId, LinieId)`.
O factură de avans cu 100/19 și 100/9 se regularizează prin două linii cu
referințe distincte. Dispare cazul de ambiguitate produs de referința numai
la antet; nu alegem sursa după cotă, valoare sau primul rezultat.

Cu sursă validă, cota/regimul se compară cu calificarea istorică a avansului,
iar verificarea intervalului folosește reperul avansului. Regularizarea
păstrează propriile repere D300/D394; referința nu mută suma în luna
avansului și nu schimbă taxa culeasă. Două facturi finale pot referi parțial
aceeași linie. Consumul/restul avansului și depășirea lui rămân în afara R6.

Fără referință: `TVA_AVANS_FARA_REFERINTA`. Sursă neoperată, de alt
partener/sens, nemarcată ori propria factură: `TVA_AVANS_REFERINTA_INVALIDA`.
Calificare diferită de sursa validă: `TVA_AVANS_CALIFICARE_DIFERITA`.
Acestea sunt avertismente fiscale, nu refuzuri de operare; nu recomandă
automat cota curentă. Regulile structurale/de acces ale culegerii rămân
valabile: FK inexistent/invizibil se tratează conform 80/104. Ștergerea
fizică a unei linii draft referite se refuză prin `NO ACTION` /
`ClientNoAction` → 422; nu există ștergere logică (104f/g).

## R6-B4 — retururi și reduceri (M2)

Pentru ajustările art. 287, reperul explicit este art. 282 alin. (9):
exigibilitate la evenimentul ajustării, regim și cotă ale operației de bază.
Aceasta justifică returul din septembrie la cota livrării din iulie;
art. 291 singur nu descrie complet cazul. Regula este reflectată și de
instrucțiunile D300 pentru ajustările la cotele vechi:
[OPANAF 174/2026, rândul 16](https://static.anaf.ro/static/10/Anaf/legislatie/OPANAF_174_2026.pdf).

R6 minim recunoaște RDC/RLF prin contractul declarației, iar reducerile
FCT/FCL nemarcate ca avans prin baza negativă a operării. Fără proveniență
fiscală rezolvată, emite `TVA_AJUSTARE_FARA_SURSA` **în locul** verdictului
de interval, indiferent dacă tipul ales este azi în interval. Mesajul spune
că nu poate valida cota originală, nu că ajustarea este greșită. Verificarea
aritmetică R6-B2 rămâne independentă. Excepția nu se aplică inverselor.

R6 nu promite că lotul RLF identifică automat factura fiscală: proveniența
stocului poate trece prin NIR, transfer sau deschidere. Extinderea va cere
lanț fiscal determinist, cu refuzul unei deducții arbitrare când lipsește
sursa sau există mai multe. Pentru RDC/reduceri trebuie contractată
referința fiscală explicită. Numele delimitării rămase în 103-r2 este
**„Proveniența fiscală a ajustărilor RDC/RLF/reduceri”**; nu o declarăm
livrată și nu alegem acum un câmp generic cu mai multe semantici.

## R6-B5 — implicite și straturi (M3, m1)

L3 rezolvă TVA implicit la `DataExigibilitate ?? Data`, inclusiv
`PoliticaTvaImplicit.ValabilDeLa`; candidații din afara intervalului sunt
excluși cu motiv în `Explica`. Filtrele de acces și `Activ` rămân valabile.
Lipsa unui candidat eligibil lasă alegerea explicită, cu explicație;
nu promitem că un tip istoric inactiv va fi propus. Schimbarea datei nu
înlocuiește în tăcere o alegere deja culeasă.

Regula intervalului și a cotei este o funcție pură, fără ObjectSpace sau
securitate XAF. L2 o consumă pe calificarea înghețată după operare, L3 pe
draft, proiecția pe citirile fiscale. Nucleul nu primește nomenclatoare și
nu se modifică pentru acest diagnostic. XAF și DTO/Apply sunt adaptoare
L3; paginile React de detaliu rămân înghețate (104d).

## R6-B6 — raport, compensare și securitate (M5, M6, m2, m3)

Raportul REST folosește ObjectSpace secured. Pentru operate, măsurile și
calificarea provin exclusiv din `Cub.Citiri.Fiscale`; pentru drafturi se
citește explicit agregatul. Perioada filtrată este a exigibilității, nu
implicit luna D300 sau data facturii. Rândurile arată document/linie,
Emise / Primite-Vamă, starea, codul, motivul și calificarea istorică/actuală.
Valorile sursei avansului apar numai dacă rândul și membrii sunt accesibili;
sursa invizibilă și cea inexistentă sunt indistincte pentru cititor.

L2 nu verifică securitatea sursei în contextul său de sistem. Mesajele
operării rămân `COD: text`, cu identificarea liniei proprii și formulare
minimală, fără număr, dată, cotă sau valori ale sursei. Linia proprie este
identificată prin poziția afișată, în aceeași ordine ca gardul F13-D1.
Afișarea mesajelor
nu transformă succesul comenzii în refuz. Raportul calculează diagnosticul
din datele permise; nu expune un rezultat îmbogățit în context non-secured.

Inversele se recunosc prin `InversaDinId`, nu se verifică independent și
nu produc un al doilea avertisment. Perechea original/inversă este
„compensat”, ascunsă implicit; istoricul se cere explicit. Compensarea se
rezolvă înaintea filtrării pe perioada originalului, inclusiv când inversa
este într-o altă lună. Raportul arată starea curentă, nu pretinde un sold
istoric la sfârșitul lunii selectate. Egalitatea sumelor unor fapte fără
legătură de inversare nu închide un avertisment.

O linie de avans stornată sau înlocuită prin corecție tehnică produce
`TVA_AVANS_SURSA_COMPENSATA` pe regularizarea care încă o referă. Corecția
creează linii noi; nu redirecționăm FK-ul automat. Remediul este alegerea
explicită a sursei corecte în draft sau prin calea de corecție a operatului.

Raportul se deschide după editarea tipului sau la cerere, filtrat pe tip și
perioadă; nu scanăm toate documentele la salvare sau la pornirea hostului.
Editarea calificării poate lăsa faptele noi fără mapare SAF-T, deoarece
cheia include calificarea. Raportul indică lipsa mapării potrivite prin
diagnosticul 103(f), fără să copieze automat maparea cotei vechi. Istoricul
mapat corect rămâne astfel. Diagnosticul mapării rulează numai când SAF-T
se aplică, după criteriul comun al proiecției SAF-T; profilul bugetar nu
primește acest avertisment. Remediile aparțin utilizatorului.

## R6-B7 — suprafețe și probe

| Zonă | Modificare propusă |
|---|---|
| Entități și EF | Interval TipTva, marcă TipMaterial, referință pe liniile FCT/FCL; migrație canonică și probe TPH/FK |
| L1/L2 | Context fiscal și funcție pură de diagnostic; Mesaje, fără rescrieri sau refuz fiscal nou |
| L3 | Implicite pe exigibilitate, recalcul explicit, validarea referinței; XAF și WriteDto/Apply |
| Raport | Proiecție secured, compensare, perioadă de exigibilitate, impact și avertismente distincte |
| UI | XAF pentru culegere; raport de citire în React sau XAF; fără calcul fiscal în TS |
| Contracte generate | Metadata/OpenAPI/types, fără drift |
| Probe | SC-CIT-90…94; integral pe ambele profiluri; HTTP secured, browser, model TPH și FK |

Acoperirea implementată și delimitările sunt actualizate în
`politici-si-fiscalitate` și `limite-curente`. Probe numerice pentru
rotunjire, taxare inversă și capitalizat completează scenariul SC-CIT-91.

## R6-B8 — alegerile owner-ului înaintea codului

Ambele alegeri sunt tranșate de owner la 2026-09-27 și înscrise în 103(i).

**M1(B): aprobat.** Marcajul `TvaCules` intră în R6 alături de A, cu
semantica din R6-B2.

**M7: aprobat.** Intervalele se scriu explicit în seed și se aliniază numai
pe rândurile `DinSeed`. Rândurile utilizatorului și regula `RefuzSeed` se
păstrează. Tabelul de mai jos este datele de implementat:

| Profil / tipuri existente | De la | Până la |
|---|---|---|
| Privat: N19, TI19; Bugetar: CAP19 | null | 2025-07-31 |
| Privat: N21, N11, TI21, NED21, IMP21, IMP11, IMPTI21, IMPTI11 | 2025-08-01 | null |
| Bugetar: CAP21, CAP11 | 2025-08-01 | null |
| Privat: N9 | null | 2026-07-31 |
| Privat: SDD, SFD, NIM, IMP; Bugetar: CAP0 | null | null |

Calendarul cotei standard/reduse și tranziția locuințelor se fundamentează
pe [Legea 141/2025, art. II și III](https://static.anaf.ro/static/10/Anaf/legislatie/L_141_2025.pdf).
Limita N9 nu verifică eligibilitatea unei locuințe și nu autorizează orice
operație la 9% până la acea dată. Capătul inferior null al cotelor istorice
nu pretinde validarea întregii istorii legislative. Mecanismul rămâne o
verificare a intervalului configurat, nu un motor al tuturor condițiilor TVA.

Implementarea urmează alegerile înscrise în 103(i).

## Verificarea implementării — 2026-09-28

- `ScenariiTvaIntervale` rulează prin `--scenarii FISCALE` / `CITIRI` pe
  ambele profiluri. Acoperă calificarea înghețată, taxa automată/culesă,
  exigibilitatea, compensarea între luni, avansul cu mai multe cote,
  corecția sursei fără redirecționare, retururile și repartizarea centului
  în cub. Modelul EF probează o singură coloană și un singur FK NO ACTION.
- Integrala ambelor profiluri: `run-verificari/20260927-235325-207`, exit 0.
  După completările de diagnostic și securitate: scenarii FISCALE pe ambele
  profiluri, `run-verificari/20260928-002153-343`, exit 0.
- HTTP secured: `nou/tools/ProbeHttp/tva-intervale.py`, clona `.CodexR6Http`;
  sursă ascunsă și membri fiscali `Valoare`/`CotaTva` ascunși, 401/404/403/422,
  interval inversat, recalcul explicit și ștergerea sursei referite refuzată.
  Matricea existentă `refuzuri.ps1`: 294/294 PASS.
- Browser React: perioadă, avertismente, calificări, comutatorul istoricului.
  Browser XAF: marcaj read-only, taxă manuală 19,50 păstrată la salvare,
  recalcul explicit 19,00 cu marcaj false, lookup cu sursa operată eligibilă
  și excluderea sursei draft. Client build și generare repetată fără drift.

Pentru FCT/FCL cu bază negativă, L3 permite și taxa manuală negativă:
regularizarea aprobată −100/−19 poate fi culeasă prin aceeași intrare.
RDC/RLF păstrează convenția culegerii pozitive și semnul aplicat la operare.
Proveniența fiscală RDC/RLF/reduceri și consumul/restul avansului nu sunt
implementate în această felie.

Corecturile review-ului din 2026-09-28 (M1–M3 și m2) sunt verificate:
FISCALE pe ambele profiluri, `run-verificari/20260928-005014-921`, exit 0;
HTTP cu tranziția PUT/null și operare, `run-verificari/r6/review-http.out.log`,
exit 0. În browser, recalculul pe FCT selectată produce 19 și se salvează;
acțiunea lipsește din NIR și din lista rădăcină `DocumentDetaliu_ListView`.
`git diff --check` trece. Integrala anterioară nu a fost reluată pentru
aceste corecturi locale. Observațiile m1/m3/m4 sunt în `limite-curente`.
