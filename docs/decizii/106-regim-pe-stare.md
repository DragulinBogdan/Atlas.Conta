# 106 — Regimul pe stare: o singură sursă a editabilității și a comenzilor disponibile

Data: 2026-09-30
Stare: activă; amendează 042(e) (affordance-urile pe resursă au acum o sursă comună) și precizează 104(c); (h) adăugat 2026-10-01 (axa 2 făcută, închide 106-r4); (i) adăugat 2026-10-04 (delimitarea față de 108)
Docs: `docs/nucleu/scenarii/REGIM.md`; `design/format-xaf-documente.md`; `docs/stare-curenta/domeniu-si-operare.md`, `api-si-client.md`, `dezvoltare-si-validare.md`

## Regula durabilă

**(a) Formatul unitar al ecranului de document e definit de regimul pe stare.**
Antetul și liniile sunt declarate o dată pe ierarhie (baseline), tipul
contribuie doar ce are în plus. Ce definește ecranul e regimul: în starea
curentă, ce se editează și ce comenzi sunt disponibile. Regimul e o valoare
calculată, nu o convenție de scriere.

**(b) Regimul e onest și are o singură sursă, în coaja comenzii (L2).**
`Api/RegimDocument.Calculeaza` întoarce, pentru un document, editabilitatea
și, pentru fiecare comandă din vocabular, motivul indisponibilității sau
`null`. Motivele sunt cele ale gardienilor motorului, citite prin aceleași
predicate (`MotorOperare.Motiv*`, `GardianPerioada.MotivInchisa`), nu
reformulate: Draft → Operează, Validează, Șterge; Operat → Anulează dacă
perioada înregistrării e deschisă și nu există latură pereche operată,
conexe operate ori împerecheri; Stornează și Corectează pe aceleași
dependențe, fără perioadă (ea se verifică pe data cerută, la comandă);
Stornat → nimic. Editabil ⇔ Draft. O affordance care ar spune „se poate” pe
un document pe care motorul îl refuză e defect.

**(c) Tipul contribuie prin hook polimorf, nu prin `switch`.**
`Document.ContribuieRegim(os, regim)` decide comenzile proprii tipului, în
vocabularul închis `RegimDocument.ComenziCunoscute` (Sterge, Regenereaza,
Distribuie, Stinge, GenereazaDescarcarea, plus `ComandaDocument`). O comandă
nouă intră întâi în vocabular, apoi în hook-ul tipului; regimul refuză
numele necunoscute.

**(d) Adaptorii randează regimul, nu îl re-derivă.** `Api/*Apply` proiectează
`Poate*` din regim; XAF are un singur gardian (`RegimDocumentController` și
gemenii lui pe linii) care pune `View.AllowEdit/New/Delete` și
`Action.Enabled` cu cheia „Regim”, potrivind acțiunea cu comanda prin sufixul
ID-ului (`Document.Opereaza` → `Opereaza`). Motivul indisponibilității ajunge
în tooltip-ul acțiunii. Controllerele de comenzi rămân executanți: fără
`Enabled["Stare"]`, fără `AllowEdit["Stare"]`. Excepția declarată:
`DscApply.PoateEdita = false` e fapt al tierului API (F4-D2, nu există cale
de scriere), nu regulă de domeniu.

**(e) Proba.** ModelCheck probează structural că fiecare comandă din
toolbar-ul DetailView-ului unui `Document` numește o comandă a regimului,
că nicio affordance din `Api/` nu se calculează din `StareDocument` și că
niciun controller nu poartă cheia „Stare”. Catalogul `REGIM` (SC-REGIM-01…07)
scrie de mână regimul pe Draft, Operat, împerechere, conex operat, perioadă
închisă și Stornat, și verifică egalitatea ReadDto ↔ regim.

**(f) Costul asumat.** Regimul onest plătește interogările dependenților la
fiecare citire (API) și la fiecare re-evaluare pe DetailView (XAF:
activare, `CurrentObjectChanged`, `Committed`, `Reloaded`). Sunt trei `Any`
mărginite pe ID plus o citire de perioadă fără blocare; se acceptă în
schimbul dispariției refuzului-surpriză.

**(g) Ce nu intră.** StateMachine-ul DevExpress nu se folosește pentru
regim: tranziția e comandă a motorului cu registre, nu schimbare de câmp.
Modulele externe (Llamachant, Reactive.XAF) nu intră: regulile per tip sunt
structură, deci cod, nu date. Axele 1 și 2 ale formatului (antet, coloanele
liniilor pe roluri) rămân în baseline, cu restanțele numite.

**(h) Axa 2: liniile se declară pe roluri, o dată pe ierarhie (2026-10-01,
închide 106-r4; precizează (g)).** Vocabularul rolurilor liniei (Directie,
Identitate, Provenienta, Unitate, Cantitate, Pret, Tva, Valori, AtributeLot,
Conturi, Parametri) e declarat o dată pe `DocumentDetaliu`, în ordinea de
culegere; un tip spune doar ce roluri poartă și ce nu poartă. Baza deține
ordinea, tipul deține conținutul; un rol al bazei nepurtat de tip se ascunde,
un membru fără rol vine la coadă. Rezultatul motorului (`Valoare` unde
`BazaLinie` o calculează și nu e intrare; `Lot` unde îl naște mecanismul) e
`ReadOnly` printr-o singură declarație, pe grilă și pe dialogul liniei
deopotrivă. Primitiva stă în Atlas.DXF (`Columns`/`Slot`/`Drop`, `ReadOnly`,
26.1.4.10), nu în Conta: e structură de view, fără noțiune de document.
Excepțiile pe view rămân view-scoped și câștigă. Proba: `ProbeLinii`
(106h-1…3) pe modelul real. Limita numită: tipurile de pe grila generică nu
blochează `Valoare` (106-r6). Vocabularul e structură (cod), deci o ordine
nouă = o linie schimbată pe bază; un tip nu poate reordona sloturile.

**(i) Ce promite regimul (2026-10-04; precizează (b) după 108).** Regimul
răspunde pentru gardienii care se pot citi fără a executa comanda: starea,
dependenții și perioada. Refuzurile care depind de valori (sold, validări,
contractul declarației) și așteptarea blocajului de scriere
(`SCRIERE_OCUPATA`, 108f) apar numai la comandă și nu fac regimul neonest.
Citirea regimului nu ia blocajul scrierii (108e). Predicatul împerecherilor
are un singur loc, `MotorOperare.MotivImperecheri`, numit în lista nominală
a registrelor (108a) ca legătură.

## Context

Sesiune de arhitectură cu owner-ul, 2026-09-30, pornită de la `design/*-xaf.md`
(2026-08-06, starea de la GATE XAF): „format unitar pentru documente în XAF,
cu primitivele XAF; ce folosim din modulele native, interne (Atlas.DXF) și
externe; unde intră HCategory / ITreeNode”.

Constatări din cod:

- Antetul ierarhiei e deja o singură declarație (`ContaUiBaseline`,
  `ForHierarchy<Document>().Layout()`); 4 tipuri au grup propriu. Comutarea
  liniilor tipizate e un atribut plus un updater. Comenzile, editarea pe Draft,
  numerotarea și recalculul TVA sunt controllere generice.
- Divergența per tip: grilele de linii scrise de mână pe indici, blocajele
  Lot/Valoare copiate în trei locuri, regulile per tip împrăștiate (metode
  virtuale, interfețe marker, `switch` în `NormalizariTip`, Appearance pe
  frunze), iar **regulile pe stare erau cod în cinci controllere**, fără
  primitivă.
- Regimul avea două surse, amândouă în L4: XAF (`Stare == Draft/Operat` în
  `DocumentEditareController`, `DocumentDetaliiEditareController`,
  `DocumentOperareController`, `FacturaIesireDescarcareController`) și API
  (18 fișiere `Apply`, cu 17× `PoateOpera = Draft`, 13× `PoateAnula = Operat
  && faraImperecheri`, 3× și `faraCopiiOperati`, TRZ și cu latura pereche).
  API-ul era mai onest decât XAF; niciunul nu vedea perioada închisă.
- Module DevExpress înregistrate fără utilizare în domeniu: StateMachine,
  ViewVariants, HCategory (template). `DxTreeListEditor` e în pachetul Blazor
  de bază, pe `ITreeNode` sau pe date plate (KeyFieldName/ParentKeyFieldName),
  doar Client/Queryable, fără Server (conflict cu implicitul 85a).
- Atlas.DXF (raport de explorare, 2026-09-30): fluent-ul are layout autoritar
  pe tip cu merge pe ierarhie, coloane, `RequiredNav`, `ServerMode`, navigație,
  dialoguri, wizard; nu are noțiune de document, nici reguli pe stare, iar un
  grup n-are număr de coloane sau direcție.

Tranșările owner-ului:

- axa 3 (regulile pe stare) definește formatul, înaintea axelor 1 și 2;
- regimul onest, cu costul interogărilor și cu obligația de a arăta motivul,
  în locul regimului ieftin doar pe stare cu refuz la comandă;
- scrierea pe worktree separat (`c106-regim-stare`), fără suprapunere cu
  felia SAF-T.

HCategory și ITreeNode: documentele nu sunt ierarhie de date, ci clasificare
plată pe `TipDocument`; „seturile de documente” sunt navigație. Ierarhiile
reale sunt `Cont` (`ParinteId`), `RandD300` (`Parinte`) și clasificarea
imobilizărilor (prefix de cod), toate afișate plat. Locul natural al tree
list-ului e planul de conturi și D300, prin modul plat Key/ParentKey, fără
`HCategory` (moștenește `BaseObject`, contra 104e) și fără `ITreeNode`
(cere `IBindingList`, nimic în plus pe EF Core). Rămâne restanță (106-r3).

## Ce rămâne deschis

- 106-r1: închisă 2026-10-04. Probat în browser pe hostul Blazor, pe o
  notă contabilă Draft și pe una Operată: elementul de sub cursor al fiecărei
  acțiuni dezactivate poartă motivul în `title`, inclusiv pe acțiunea cu
  parametru (Stornează), unde butonul are `pointer-events: none` și titlul
  stă pe containerul lui; tooltip-ul propriu revine pe acțiunea disponibilă.
  Proba a arătat că ștergerea standard XAF rămânea disponibilă pe documentul
  operat; e legată acum de comanda `Sterge`. Lista de documente nu aplică
  regimul pe selecție (refuzul vine de la gardian, la salvare).
- 106-r2: comenzile proprii ale tipurilor (Regenereaza, Distribuie, Stinge)
  au acțiuni doar în React; în XAF nu există încă acțiuni pentru ele.
- 106-r3: planul de conturi și D300 pe `DxTreeListEditor` în mod plat, cu
  view opt-out de la `Server` (85a).
- 106-r4: închisă prin (h), 2026-10-01.
- 106-r6: BCS/BTR/RLF calculează `Valoare`, dar stau pe grila generică
  și pe dialogul `DocumentDetaliu_DetailView`, comun cu DVI (care o culege);
  blocajul cere detaliu propriu.
- 106-r5: StateMachine și ViewVariants ies din `Startup.cs` și din
  `RequiredModuleTypes` (tabele și noduri de model fără utilizare); `HCategory`
  iese din `DbContext`.
