# 79. Pasul 5, felia 21 — ITV (închiderea de TVA) prin API și client: comandă cu cauză + ecran de rezultat

- **Data**: 2026-09-01
- **Stare**: activă (b amendată de 80e: cifrele motorului cer și dreptul de citire pe `RegistruContabil`; c amendată de 80b: comenzile NTC pe un id ITV nu mai sunt permise — 404 pe toată ușa NTC; 79-r6 închisă de 80; 79-r1 închisă la 2026-09-02 — acțiunea XAF „Generează închiderea", vezi restanța)
- **Docs**: `docs/api/p5-felia-itv-contract.md` (F21-D1…D12 + §Închidere cu cifrele HTTP), `nou/.../Module/Motor/InchidereTvaService.cs`, `nou/.../Module/Api/Itv/`, `nou/.../WebApi/API/Conta/ItvController.cs`, `nou/Atlas.Conta.Client/src/felii/itv/`; probele în `nou/tools/ModelCheck/Program.cs` (blocul `E2E-API-ITV`)

## Context

ITV era ultimul tip de document fără ușă HTTP și fără ecran, și deliberat
altfel (76a, F19-D1): nu e un agregat cules, e rezultatul unui serviciu
(`InchidereTvaService`, 46c). Singurul apelant viu era consola Import1C; un
contabil nu putea genera o închidere nici din React, nici din XAF (acțiunea
XAF a rămas „aditivă" din 1C-a). Două constatări ale explorării au fixat
forma feliei:

- **Serviciul colapsa trei cauze de „nimic de generat" pe același `null`**
  (profil inert / închidere vie / solduri zero), iar Import1C le compensa cu
  un string scris de apelant (`motivFaraDraft`). Un ecran nu poate spune DE
  CE n-a generat dintr-un `null`.
- **ITV era vizibil și scriibil prin felia NTC**: sub TPT
  `GetObjectsQuery<NotaContabila>()` întoarce și `InchidereTva`, deci un
  draft ITV apărea în `GET api/ntc`, se deschidea la `/ntc/{id}` și se putea
  rescrie prin `PUT api/ntc/{id}` (reconcilierea acceptă orice linii;
  gardianul anti-stale l-ar fi prins abia la operare).

Pe deasupra, comanda de generare n-are subiect: `ComandaAutorizata<T>(id)`
(55b/72e) cere o instanță, iar aici instanța e PRODUSĂ — asimetria numită de
76-r4, ajunsă acum la o ușă reală.

## Decizia

**(a) Serviciul întoarce REZULTAT cu cauză; `Genereaza` rămâne pentru
apelanții vechi.** `MotivNegenerare { ProfilInert, InchidereVie, FaraSold,
NeCronologica, DraftAnterior, PerioadaInchisa }` (ultimele două din review,
mai jos; în `BusinessObjects/Comun/Enums.cs`, cu `[XafDisplayName]`
— dump-ul de metadata ia DOAR spațiul `BusinessObjects`, deci un enum lângă
serviciu n-ar fi ajuns niciodată în client), `RezultatInchidere(Document,
Motiv, Sold4426?, Sold4427?, InchidereVieId?, Linii)`, `LiniiInchidere`.
`CalculeazaLinii(sold4426, sold4427)` e **SINGURA aritmetică** a celor trei
linii (rotunjirea intră în ea) — generatorul o consumă, deci previzualizarea
nu e o copie a calculului, e același calcul (42c). `Analizeaza` e singura
ordonare a gardienilor (profil → închidere vie → cronologie înainte →
draft anterior neoperat → perioada fiscală → solduri → TRZ; primele trei din
46c, următoarele două din review); raportul (`Previzualizeaza`, nu scrie) și
comanda (`Incearca`, nu comite) diferă printr-un singur bit: **cronologia și
perioada sunt motiv la raport și refuz zgomotos la comandă** (46c rămâne).
`LiniiPotrivescSoldurile` e SINGURUL criteriu anti-stale — al gardianului
din `ValideazaOperare` și al lui `Stale` din DTO. Soldurile se citesc o
dată și însoțesc orice verdict de sub gardul de profil (pe `ProfilInert` sunt
`null`, nu 0 — acolo nu există conturi, un zero ar fi o minciună
liniștitoare). Gard nou de ARGUMENT, înaintea tuturor: `unitateId` care nu e
`UnitateInterna` ⇒ `OperareException` la generare (oglinda gardului din
`NotaContabila.ValideazaOperare`, adusă acolo unde cererea greșită se
formulează, nu abia la operare). `Genereaza` = `Incearca(...).Document` —
Import1C și probele de motor neatinse.

**(b) Gate-ul de generare e pe TIP; cifrele motorului nu se filtrează tăcut.**
`PoateCrea(Type, os)` = `IsGrantedExtensions.CanCreate(tip, os)` pe un OS
securizat, ÎNAINTE de ușa non-secured pe care serviciul își scrie draftul
(58c). Trei rute răspund cu cifre ale motorului (soldurile 4426/4427 la data
închiderii, `Stale`, liniile care s-ar genera) și rulează pe ușa NON-SECURED
după un verdict de acces luat pe cea securizată: `GET {id}` prin
`AutorizeazaCitire<T>` (instanță: invizibil 404 / vizibil fără drept 403),
`previzualizare` prin `PoateCiti` pe tip, `genereaza` prin `PoateCrea` pe tip.
Motivul e cel de la 73g: soldurile însumate peste rândurile de registru pe
care permisiunile le ascund ar fi o cifră FALSĂ prezentată ca „nu e nimic de
închis", nu una goală. Listele rămân pe ușa securizată (lista goală e un
răspuns adevărat). Măsurat pe HTTP: `User` primește 403 pe `previzualizare`
și pe `genereaza` fără să apară vreun draft, 404 pe `GET {id}` (invizibil —
familia 72-r10, notată, nu normalizată).

**(c) ITV iese din felia NTC.** `NotaContabilaApply.Lista`/`Citeste`/
`Candidati` filtrează `!(d is InchidereTva)` (EF traduce `is` pe TPT prin
frunză, verificat pe `ToQueryString`); `Aplica` (PUT) și `Sterge` refuză cu
mesaj de domeniu (422). `is` e la GRANIȚĂ, în Apply, nu în motor — regula
„motorul nu cunoaște frunzele" nu e atinsă. Comenzile `opereaza`/`anuleaza`/
`storneaza` ale NTC pe un id ITV rămân permise: sunt `OperareApi` pe
`Document`, agnostic la tip, cu același rezultat ca pe ruta ITV.

**(d) Rutele: comandă + rezultat, zero schimbări de schemă, zero seturi OData
noi.** `api/itv`: lista (ordine implicită DECLARATĂ `Data` desc + `Id` desc —
după o regenerare draftul lunii vechi ar sări în capul listei pe ordinea de
inserare), `GET {id}` (`ItvReadDto`: liniile + `Transfer/DePlata/DeRecuperat`
identificate prin CONTURILE POLITICII, `Sold*Curent` prin aceeași `Solduri` ca
gardianul, `Stale` doar pe Draft prin criteriul gardianului, `null` fără
politică completă, affordances 57d
inclusiv `PoateRegenera`), `previzualizare?an&luna` (validare explicită,
toate erorile deodată, 400 `EroriDto`), `POST genereaza {An, Luna, UnitateId}`
(`[Range]` pe an ȘI lună — `An = 0` dădea 500 din `DateOnly`; **200 și când
`Motiv != null`**: e un raport, ca `DscId = null` la backorder și lotul ANAF;
422 doar pe cronologie / TRZ lipsă / unitate ne-internă), `{id}/regenereaza`
(doar Draft; gate de instanță ȘI `PoateCrea` — produce un document nou;
`Incearca(…, inlocuieste: id)` ÎNAINTE de ștergere, o singură tranzacție:
refuzul lasă draftul intact, „luna nu mai are ce închide" e legitim și iese
ca raport cu draftul vechi șters), `DELETE {id}` (Draft, ușa secured),
`opereaza`/`anuleaza`/`storneaza`/`valideaza` ca la NTC. Unitatea internă e
**parametru cules** (modelul n-are „unitatea societății"; `Societate` nu
poartă FK spre `UnitateInterna`), precompletată în client doar când setul are
exact un rând.

**(e) Clientul: previzualizarea e ecranul de rezultat ÎNAINTE de faptă.**
`/itv` = bara de generare (an/lună `<select>` nativ în URL, ca la SAF-T;
unitatea prin `SelectBox` pe setul OData `UnitateInterna` citit ÎNTREG —
F21-D4 cere NUMĂRUL rândurilor, iar `Lookup` e legat de formular prin
`useCamp`, tiparul fișei de cont) + previzualizarea lunii (soldurile și cele
trei linii, sau motivul prin `labelEnum('MotivNegenerare')` cu link către
închiderea blocantă) + grila (fără înălțime fixă: blocul de deasupra e
variabil); „Generează" activ doar pe `Motiv == null` și unitate aleasă; 200
fără document = raport pe ecran; refuzul se arată doar cât e al cererii de pe
ecran (tiparul SAF-T). `/itv/:id` pe `DocumentShell` fără formular: antet
read-only, liniile, panoul soldurilor la data închiderii, atenție DOAR pe
`Stale === true`, comenzile standard + Regenerează/Șterge cu
`ConfirmareInline`, storno cu `cereData.implicit = Data` (46f, indiciul în
subsol). `rutaTip('ITV')` — rândurile ITV din fișă și registrul-jurnal devin
link. Smoke pe Privat 11/11, consolă curată, zero 4xx/5xx; `Stale === true`
probat doar în ModelCheck (ar fi cerut o scriere în afara ciclului).
Convenții păstrate deliberat, nu decise aici: „Verifică" rămâne activ pe
orice stare (ca la NTC — dry-run legitim, mesajul e al serverului); unitatea
aleasă moare la remontarea listei (nu e în URL, F21-D4).

**(f) Probele.** ModelCheck: blocul `E2E-API-ITV` (26 probe la livrare + 22
din review, privat; final 944 / 900, 0 FAIL) pe
lunile 11/12 2026 — ocupate de scenele ASM/RET care rulează DUPĂ, deci blocul
stă imediat după `E2E-ITV`, cu precondiție MĂSURATĂ al cărei nume spune ce
rupe o reordonare, și purjă fizică cu zero închideri rămase; cusăturile:
previzualizare == liniile generate, `Stale` din DTO == refuzul gardianului
(`OperareApi.Valideaza`), regenerarea ⇒ draft NOU cu vechiul șters logic,
cele patru uși NTC închise (cu control pe nota adevărată), bugetar ⇒
`ProfilInert` cu solduri `null`. HTTP (Privat, Admin/User): 31 de probe în
§Închidere al contractului, toate cu codurile pin-uite; timpi la cald
38–112 ms (59: nimic de optimizat).

## Review advers (agent separat, scenarii concrete; fix-urile aplicate de main)

**Două defecte de FOND, aceeași rădăcină — cronologia trăia doar la
generare și doar într-un sens:**

- **F1.** Draft OCT neoperat → se generează NOV (nimic nu-l oprea: „vie"
  se uita doar la luna cerută, „cronologic" doar înainte) → se operează NOV
  → se operează OCT: trece, cu `Stale == false` și „Verifică" declarând că
  e în regulă ⇒ 4423 dublat, 4426/4427 cu reziduu negativ mascat de
  `Max(0, …)` din `Solduri`. Gaura era pre-existentă (46c), dar Import1C
  generează strict cronologic și operează imediat; felia a livrat butoanele
  care o ating. **Fix, în ambele sensuri și pe ambele uși**: `Analizeaza`
  2c — un Draft neoperat pe o lună ANTERIOARĂ blochează generarea
  (`DraftAnterior`: motiv la raport, refuz la comandă); `InchidereTva.
  ValideazaOperare` — o închidere OPERATĂ ulterioară refuză OPERAREA (ușa prin
  care trece orice operare: XAF, API, consolă); un Draft ulterior nu
  blochează (devine stale și îl refuză gardianul anti-stale).
- **F2.** `Regenereaza` ștergea și COMITEA, apoi `Incearca` putea arunca
  (cronologie, unitate ștearsă) ⇒ 422 cu draftul pierdut definitiv, pe o
  acțiune al cărei text promite că îl reface; clientul nu invalida pe
  `catch`, deci rămânea pe documentul șters cu comenzile active. **Fix**:
  `Incearca(…, inlocuieste: id)` — draftul de față nu contează ca închidere
  vie, refuzul vine ÎNAINTEA oricărui `Delete`, ștergerea și nașterea sunt o
  singură tranzacție (idempotența nu mai depinde de filtrul ștergerii
  amânate); clientul reîncarcă și pe refuz.

**Medii, fixate:** M2 perioada fiscală închisă — `PerioadaInchisa` la raport
/ refuz la comandă (un draft acolo nu s-ar fi putut opera, dar ar fi blocat
cronologic lunile dinainte); M4 `Stale` era mai STRICT decât gardianul (trei
egalități pe linii vs două de sumă) și `false` pe politică incompletă —
`LiniiPotrivescSoldurile` e acum singurul criteriu, `null` fără politică
completă; M5 `regenereaza` produce un document nou dar era autorizat doar pe
`CanWrite` al celui vechi — cere și `PoateCrea`; M6 previzualizarea afirma
simbolurile 4426/4427/4423/4424 din cod — vin acum din politică prin DTO
(`Simbol*`), iar panoul documentului nu mai numește conturi; M7
`previzualizare` fără `Domeniu` (ancoră lipsă ⇒ 400 text/plain) — învelită;
M8 două probe care numeau mai mult decât exercitau (+ precondiția blocului
prinde acum și mutarea după `E2E-ASM`); M9 Import1C `CereRepartitor` pe baza
`Repartitor` — sub TPT `Cod` e unic per frunză, iar de la 79a un partener
„SEDIU" ar fi picat luna la ITV: SEDIU/COMISIE se cer pe `UnitateInterna`.
M1 (unitatea nu schimbă cifra — închiderea e a societății) = un rând de text
pe ecran. **Declarate, nu fixate:** M3 → 79-r5; scurgerea de informație a
previzualizării → 79-r6; „Verifică" pe ne-Draft → 79-r7. Cosmetice lăsate:
`?an=abc` în URL lasă `<select>` fără opțiune; `invalidateQueries(['itv'])`
nu atinge grila DevExtreme (comentariul corectat); două click-uri rapide pe
o comandă trimit două cereri (a doua ia 422).

## Ce rămâne deschis (restanțe cu nume)

- **79-r1** acțiunea XAF „Generează închiderea" (aditivă; 44/53) — un
  contabil pe Blazor tot nu poate genera. **ÎNCHISĂ la 2026-09-02**:
  `Module/Controllers/InchidereTvaGenerareController.cs` — `PopupWindowShowAction`
  pe `InchidereTva_ListView`, parametrii (an, lună, unitate internă) pe un
  obiect NON-PERSISTENT (`GenerareInchidereTvaParametri`, `[DomainComponent]`,
  exportat în `Module.cs`; lookup-ul de unitate prin
  `NonPersistentObjectSpace.PopulateAdditionalObjectSpaces` LOCAL dialogului,
  nu un hook global pe `ObjectSpaceCreated`); default = luna după ultima
  închidere vie, unitatea precompletată doar la exact un rând (F21-D4);
  gate-ul de pe API (F21-D3 + 80b: `CanCreate` ȘI `CanWrite` pe TIP, pe OS-ul
  securizat, ÎNAINTE de ușa non-secured) cu fraza `Refuzuri.FaraDrept`;
  comanda = `InchidereTvaApply.Genereaza` pe ușa non-secured (aceeași ca REST
  și ModelCheck); „nu s-a generat" = toast informativ cu motivul din
  `[XafDisplayName]` + soldurile + închiderea blocantă (79d), refuzul de
  domeniu = `UserFriendlyException`, draftul născut se deschide într-un TAB
  nou (`TargetWindow.NewWindow` — aplicația e MDI; `Default` cât dialogul e
  deschis devine `NewModalWindow`, `Current` înlocuia view-ul tab-ului fără
  să-i reconstruiască toolbar-ul — probate în browser, explicate pe sursa
  `BlazorMdiShowViewStrategy`). Smoke în browser (Privat): Admin — fără
  unitate ⇒ „Alegeți unitatea internă", 01/2026 ⇒ „Luna n-are sold" cu
  solduri, 10/2026 ⇒ draft cu 2 linii deschis în tab cu Operează/Stornează,
  09/2026 cu draft ulterior ⇒ refuz de cronologie, 12/2025 ⇒ „Luna are deja
  o închidere"; Cititor — dialogul se deschide, „Generează" ⇒ „Nu aveți
  dreptul de a crea „Inchidere Tva”", niciun draft scris. Ce NU face,
  deliberat: previzualizare în dialog, regenerare (pe Blazor = ștergere +
  generare), butonul rămâne VIZIBIL fără drept (refuzul explicit bate gardul
  care tace, 62f). Capcană: `[ModelDefault("EditMask", "0")]` pe `int` arată
  `0` în Blazor — masca întreagă fără separator e `"d"`.
- **79-r2** `PoliticaInchidereTva` pe OData ReadOnly + ecran React (familia
  77-r3).
- **79-r3** închiderea perioadei fiscale din client (rămâne XAF/seed, 53i);
  ITV nu o atinge (46c).
- **79-r4** mesajul `[Range]` de pe `genereaza` e în engleză
  (`DataAnnotations` default) lângă cele românești ale rutei de
  previzualizare — 70-r5 confirmată pe încă o rută.
- **79-r5** storno-ul unei închideri la o dată din ALTĂ lună: rândurile
  inverse cad în luna stornării, soldurile lunii închise rămân 0 și
  previzualizarea ei raportează `FaraSold` — cauza greșită. Ecranul propune
  data corectă (46f), serverul acceptă orice dată (GATE XAF D10); un motiv
  care să numească închiderea stornată e aditiv.
- **79-r6** cine are drept de citire pe `InchidereTva` vede prin
  previzualizare soldurile de TVA ale întregii societăți fără drept pe
  `RegistruContabil` — consecința asumată a lui (b); intră în discuția 77-r8.
- **79-r7** „Verifică" rămâne activ pe Operat/Stornat și arată refuzul
  dry-run-ului ca eroare — convenția NTC, transversală tuturor ecranelor.
- 36f (TVA la încasare, 4428, prorata) rămân amânate; cusătura D300 rd. 36/37
  (69f) neschimbată; Import1C neatins.
