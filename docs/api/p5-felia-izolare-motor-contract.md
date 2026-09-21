# Izolarea motorului de `IObjectSpace` — plan independent de ORM, contracte economice de citire (contract)

Data: 2026-09-10. Stare: **DEPĂȘITĂ de decizia 90** (2026-09-20,
`docs/decizii/090-nucleu-cub-de-postari.md` §„Raportul cu contractul IM”):
pașii 0a–0c și 1–7 NU se mai execută ca felie proprie; ce s-a livrat deja
(precedentele pure, `SolduriService`, `RandDupaCheie`, `TranzactieComanda`,
`CititorTipDocument`) se preia de TR-D6a/b și TR-D7; IM-r1…r7 sunt
re-evaluate acolo. Textul rămâne ca istoric. Pleacă din analiza de arhitectură din
2026-09-09 (inventarul cuplajului + propunerea sintetizată din consultul
extern GPT-6, cu corecturile de context). Decizia rezultată e 90, pe
forma nouă, nu pe aceasta.

> **Amendament 2026-09-18 (felia 28, decizia 89).** Contractul a fost scris pe
> maparea TPT; felia 28 a trecut cele trei ierarhii pe TPH cu discriminatorul
> mapat `ClrType`. Consecințele pentru felia asta:
> - **IM-D10** e amendat: „ZERO schimbare de schemă” a devenit „schimbările de
>   schemă intră doar prin decizie proprie, reanalizată la momentul ei”;
>   cifrele de ModelCheck de la deschidere sunt cele de la închiderea feliei 28.
> - **IM-D4 „Relații” și „Politici”** se scriu și se MĂSOARĂ pe maparea finală:
>   o interogare pe `Document` nu mai plătește 20 de LEFT JOIN-uri + `CASE`,
>   iar „tipul documentului (ancora)” se citește ca dată
>   (`Api/CititorTipDocument`: proiecția `{ID, ClrType}`, `Clasa` =
>   discriminator → tip concret, cache static), fără materializare polimorfă.
>   Adaptorul peste `IObjectSpace` reutilizează cititorul; nu inventează un
>   al doilea.
> - **Regula de citire a coloanelor de frunză** (89b) se aplică și
>   adaptorului: o coloană de frunză se citește doar pe o mulțime restrânsă
>   pe tip sau prin `is ? :`; `as`/cast pe frunză nu filtrează pe tip.
> - **Căutarea după cheie** (89i): adaptorul și contractele de citire caută un
>   rând al unui tip ne-rădăcină prin `Motor/RandDupaCheie` (pe rădăcina
>   ierarhiei, cu tipul verificat după), niciodată prin
>   `GetObjectByKey<Frunza>`. Identity map-ul EF e per rădăcină, iar F28-N
>   scanează sursa.
> - **Tabelul „Scop”, rândul `GenereazaConex`**: rezoluția
>   `TipDocument.ClrType → Type` există deja o dată, în
>   `CititorTipDocument.Clasa`; IM-D9 o poate consuma în loc de
>   `Assembly.GetTypes()` repetat.
> - Cifrele din „Scop” sunt de la 2026-09-10; re-măsurarea pe codul de după
>   feliile 24–28 și consecințele ei pe decizii, pași și criteriul de
>   prioritate stau în §„Reanaliza 2026-09-18”.

## Scop

Motorul (`Module/Motor/`, 13 servicii, ~3.950 de linii) lucrează pe
`IObjectSpace` și pe entitățile urmărite de ORM. Cuplajul real, măsurat:

| Locul | Ce folosește | Volum |
|---|---|---|
| `Motor/*` | `GetObjectsQuery` 87, `GetObjectByKey` 15, `CreateObject` 13, `Delete` 8, `CommitChanges` 4 | mecanic |
| Hook-urile polimorfe de pe `Document` | 12 metode virtuale primesc `IObjectSpace` (`PregatesteOperare`, `ValideazaOperare`, `RepartitorImplicit*`, `CapacitateStingere`, `SursaStingeriiAutomate`, `GenereazaSecundar`, `MesajeDupaOperare`…) | contractul cu frunzele |
| Frunzele (11 fișiere) | `GetObjectByKey` 44, `GetObjectsQuery` 34, `CreateObject` 6 | culegerea, nu motorul |
| `GardianEditare` | `ModifiedObjects`, `IsNewObject`, `IsObjectToDelete` | change-tracking XAF — altă natură |
| `PlanOperare` | referă `DocumentDetaliu`, `RegulaStoc`, entități vii | planul nu e independent |
| `GenereazaConex` | `typeof(Document).Assembly.GetTypes()` pe `TipDocument.ClrType` | rezoluție de tip în assembly-ul Module |

Ce cumpără izolarea, **declarat** (motivul feliei, nu un scenariu viitor):

1. **Dry-run fără scriere.** `Valideaza` scrie azi `Valoare`/`Cantitate` pe
   linii („ATENȚIE (contract de apelant)" din `MotorOperare.cs`), deci cere
   OS de unică folosință; ASM `distribuie-valoarea` are „al doilea OS
   obligatoriu" (76c). Un plan cu valori calculate în afara entităților
   închide ambele.
2. **Explicabilitatea.** Planul spune ce regulă a produs nota, ce lot a fost
   ales și de unde vine dimensiunea. Felia 24 („Explică", 81k) CERE exact
   extragerea potrivirii din `MotorOperare` ca funcție pură — aceeași lucrare.
3. **Async la margini.** I/O-ul se concentrează în `Incarca` și
   `Materializeaza`; partea pură nu se atinge. Limitarea structurală a
   ObjectSpace-ului sincron (motivația migrării pe React) primește o ieșire
   fără a scoate XAF din host.
4. **Testabilitate pe fază**, fără provider in-memory (gardienii depind de
   semantică SQL: prefix-sum, `translate`, SQL brut în fișă). ModelCheck pe
   Postgres rămâne oracolul; probele pure îl completează.

Ce NU e motivul: documentele custom per client. Structura de documente nu se
redeschide (51a), clasele per profil sunt respinse (54d). Dacă produsul cere
vreodată tipuri per client, e decizie proprie, testată întâi contra
invarianților, iar blocajul ei real e `DbContext`-ul monolitic + migrațiile
canonice (23a), nu `IObjectSpace`.

## Reanaliza 2026-09-18 (după feliile 24–28)

Măsurat cu grep/wc pe `HEAD` (`p5-f28-tph`), aceleași categorii ca în §Scop:

| Locul | 2026-09-10 | 2026-09-18 |
|---|---|---|
| `Motor/*` | 13 fișiere, ~3.950 linii; `GetObjectsQuery` 87, `GetObjectByKey` 15, `CreateObject` 13, `Delete` 8, `CommitChanges` 4 | 21 fișiere, 6.249 linii; 111 / 23 / 23 / 8 / **8** (`MotorOperare` 3, `PerioadaService` 2, `ImperechereService` 2, `CorectieService` 1) |
| Hook-uri cu `IObjectSpace` | 12 pe `Document` | 11 pe `Document` (`PregatesteOperare` 13 override-uri, `ValideazaOperare` 20, `SensDeStins` 7, `PoateFiStins` 5…) + 4 pe interfețe noi: `IVerificabilLaCommit.Verifica` (86e), `IDocumentCuRegistruPropriu.Materializeaza/Elimina/StorneazaRegistrul` (87c) = **15** |
| Frunzele | 11 fișiere; `GetObjectByKey` 44, `GetObjectsQuery` 34, `CreateObject` 6 | 15 fișiere, 3.698 linii; 51 / 43 / 10; `RandDupaCheie` 5 (doar `Dvi`, `Trezorerie`) |
| `PlanOperare` | referă entități vii | neschimbat: clasă `sealed` internă lui `MotorOperare`; `Miscari`/`Note` țin `DocumentDetaliu` + `MiscareStoc` |
| `Valideaza` scrie pe linii | da („ATENȚIE”) | da: `Cantitate`/`Valoare` la 358–369, 563–564, 654–666 |
| `GenereazaConex` | `Assembly.GetTypes()` | neschimbat; `CititorTipDocument.Clasa` (89f) există și NU e consumat |
| `GardianEditare` | change-tracking | 1.312 linii; `ModifiedObjects` 7, `IsNewObject` 11, `IsObjectToDelete` 2; regula (o) pe tracker (89e) |
| Pure (0 `IObjectSpace`) | — | `Potrivire.cs` (244), `DimensiuniResolver.cs` (26); `Fapte.cs` NU e pur — e adaptorul entitate → fapt (84e), rămâne așa |

Citirea cifrelor: cuplajul a crescut cu ~58 % în opt zile prin felii fără
legătură cu IM (imobilizări, perioade/solduri, corecție, TPH). Niciuna n-a
contrazis contractul; fiecare l-a lungit: patru servicii noi amestecă citirea
cu scrierea în aceeași metodă (`AmortizareService.Genereaza`,
`PerioadaService.Inchide/Redeschide`, `CorectieService`, `ImperechereService`),
commit-urile din motor s-au dublat, suprafața de hook-uri a crescut cu trei.

### Ce s-a livrat deja din contract, în afara feliei (precedente, nu implementare)

- **Faza pură are precedente în casă**: `Potrivire.cs` (84e), `DimensiuniResolver`,
  `AmortizareService.CotaLunara` (87g), `InchidereTvaService.CalculeazaLinii`,
  `PerioadaService.Verifica` (constatări pe date încărcate). IM nu inventează
  forma „citire în lot → calcul pur → materializare”; o generalizează la
  `MotorOperare`.
- **IM-D4 „Stoc” are primul adaptor**: `SolduriService` (88e) — întrebări
  economice (chei + dată → solduri), scriere prin SQL brut. E adaptorul
  SOLDURILOR DE REFERINȚĂ, nu al loturilor: prefix-sum-ul pe (Lot × Repartitor ×
  TipStoc) din fereastra afectată (25d) rămâne partea grea, neatinsă.
- **IM-D4 „Politici”**: potrivirea consumă fapte plate (`record struct`, F24-D5).
  84-r10 (`Import1C/Catalog.IncarcaContare`) rămâne a doua definiție a
  potrivirii; pasul 2 o închide.
- **IM-D9**: rezoluția `ClrType → Type` e centralizată în
  `CititorTipDocument.Clasa` (cache static, 89f). `GenereazaConex` n-o consumă:
  schimbare de trei linii, fără felie.
- **IM-D3, jumătate**: tranzacția e DEJA a apelantului (`Motor/TranzactieComanda`,
  F27-D1 — o deschid `OperareApi` (3×), `AmoApply`, `InchidereTvaApply`,
  `PerioadeApply`, `CorectieService`, `ImperechereService`); commit-ul e încă al
  motorului (8 apeluri), înrolat în tranzacția apelantului. Mutarea lui la
  pasul 4 e mecanică: tranzacția stă deja unde trebuie să ajungă commit-ul, iar
  secvența conex/secundar (42b) se păstrează — apelantul înlănțuie pe valoarea
  întoarsă de `Opereaza`. Apelanții direcți ai motorului care preiau commit-ul:
  `OperareApi`, `AsamblareApply`, `CorectieService`, `ImperechereService`,
  `Import1C/Bucla` + `Reluare`, `ModelCheck`.
- **IM-D8, precondiție nouă**: identity map-ul EF e per RĂDĂCINĂ în orice
  strategie de moștenire (89i); orice adaptor caută prin `RandDupaCheie`,
  niciodată `GetObjectByKey<Frunza>` (F28-N scanează sursa).

### Ce contractul nu cunoștea și acoperă de acum

- **D4 „Persistență” + D6 — `IDocumentCuRegistruPropriu` (87c)**: al patrulea
  registru (`RegistruImobilizari`), trei metode care primesc `os`, apelate de
  `MotorOperare` prin trei dispecere. Sunt punct de extensie al MATERIALIZĂRII,
  nu al calculului: primesc portul de persistență, nu `ContextOperare`.
  Contractul de persistență scrie patru registre, nu trei.
- **D7 — `IVerificabilLaCommit` (86e) și regula (o) (89e)**: `IVerificabilLaCommit`
  e al gardianului, nu al motorului: regula de pe entitate primește lista de
  schimbări + întrebările de citire, în aceeași mișcare cu D7. Regula (o)
  depinde de tracker (ținta unui FK spre frunză poate fi un obiect NOU din
  același commit — F28-K), deci „lista de schimbări” din D7 conține obiectele
  noi ca ținte de rezolvare, cu `RandDupaCheie` drept fallback pe citire. E
  cazul greu concret al lui D7, deja probat (F28-D/K). F28-r4 (dezproxarea în
  patru copii) se închide la aceeași atingere.
- **D3 — `GardianPerioada` (88l)**: citirea `FOR SHARE` a verigii e I/O în faza
  de validare. Sub IM se mută în `Incarca` ca fapt („perioada de la
  `DataInregistrare` e deschisă și blocată”), ținut de tranzacția apelantului;
  faza 2 rămâne pură.
- **D4 „Politici”, lista extinsă**: `PoliticaAmortizare`, `RegulaDeductibilitate`,
  `ClasificareImobilizari` (87d), `PoliticaInchidere`, `PoliticaTva.DeclarareIntarziata`
  (88b).
- **D4 „Relații”**: perioada e LANȚ cu `DataInregistrare` reper (88a/c);
  împerecherea e fapt datat cu invariant de ordine — `Data` ≥ înregistrarea
  ambelor documente (88j). Spike-ul D5 (perechea 581 cu imperecherea automată)
  primește și regula de dată, nu doar efectul nesalvat.
- **D6, cazul care rezistă proiecției (89f)**: hook-ul care are nevoie de
  INSTANȚĂ — `SensDeStins`, chemat de `ImperechereService.AsignatFataDe` pe
  contrapartide (76-r3). Contractul „Relații” dă restul per contrapartidă în
  lot; sensul rămâne al frunzei, deci contrapartida se materializează și sub
  IM. 76-r3 se închide doar dacă sensul devine fapt scris la operare — decizie
  proprie, în afara IM.
- **Suprafața de hook-uri pentru pasul 3**: 11 pe `Document` + cele 3 ale
  registrului propriu (pe port); `IVerificabilLaCommit` trece la pasul 6.

### IM-D10, corectat

- Import1C: IDENTIC pe CONȚINUT SORTAT (standardul feliilor 27–28, 89h;
  `dezvoltare-si-validare.md`), nu „byte-cu-byte”. Textul lui D10 și al
  pasului 4 e corectat mai jos.
- Gate nou: după orice atingere a frunzelor sau a FK-urilor spre frunze, SQL-ul
  din `--dump-integritate-tph` pe baza de import întoarce zero rânduri (89e/h).
- **Perf: `valideaza`/dry-run NU are cifră** în `p5-perf-masuratori.md` (grep:
  0). Pragul „perf ≤” al lui D10 nu e verificabil azi pe exact calea pe care
  felia o schimbă ⇒ pasul 0a o măsoară înainte de orice schimbare (A/B pe
  aceeași bază, 59). Cifrele actuale de reper (F28): operarea FCT cu 49 de
  linii 230/199 ms, fișa 4111 61/55 ms, `CoduriTip` 23 ms.

### Criteriul de prioritate, azi

Niciunul din cele trei semnale nu e real: „Explică” s-a consumat în varianta
pe CONFIGURAȚIE (84g: câștigător, candidați, surse, rezerve pe o linie
ipotetică), nu pe planul unui document — lacuna e declarată în 84 §Scop
(„rezolvarea motorului se vede DOAR prin dry-run `valideaza`”); dry-run-ul
n-are cifră; niciun host fără XAF. Deciziile 86 și 87 au trecut explicit
peste IM pe criteriul ăsta.

Ce s-a schimbat fără a fi semnal, dar mută costul:

1. **Felia s-a ieftinit** prin 88/89: taxa de join pe citirile pe bază a
   dispărut, tipul se citește ca dată, tranzacția e deja la apelant, faza pură
   are precedente, „Stoc” are un adaptor.
2. **Felia se scumpește cu fiecare felie de motor**: în opt zile +24
   `GetObjectsQuery`, +4 commit-uri, +3 hook-uri, +4 servicii amestecate.
   „Tech-debt cu prag”: pragul a fost atins prin recurență, nu printr-un
   semnal de produs.

Propunere, de decis de owner: **al patrulea semnal**, măsurabil — o felie nouă
care ADAUGĂ un serviciu în `Motor/*` cu citirea și scrierea în aceeași metodă
declanșează IM înaintea ei; până atunci, orice serviciu nou de motor se scrie
de la început pe forma precedentelor (citire în lot → calcul pur →
materializare), ca să nu mărească ce IM are de mutat.

### Pașii, replanificați

- **0a (nou)** — cifra de dry-run: `valideaza` și `opereaza` pe HTTP, A/B pe
  aceeași bază; devine pragul lui D10.
- **0b** — spike-ul D5, neschimbat, plus regula de dată 88j pe perechea 581.
- **0c (nou, gratuit, înaintea feliei)** — `GenereazaConex` pe
  `CititorTipDocument.Clasa` (D9): commit separat, ModelCheck pe ambele
  profiluri, fără Import1C.
- **1** — planul POCO, neschimbat.
- **2** — contractele: politici (lista extinsă) → stoc (loturi + prefix-sum;
  `SolduriService` rămâne adaptorul soldurilor de referință) → relații (lanț
  de perioade, împerecheri datate) → persistență (patru registre,
  `IDocumentCuRegistruPropriu` pe port). Închide 84-r10.
- **3** — hook-urile: 11 + 3 pe port; `IVerificabilLaCommit` NU aici.
- **4** — materializarea și commit-ul la apelant: ieftinit (tranzacția e deja
  acolo); lista apelanților direcți de mai sus. Oprire: `refuzuri.ps1`
  294/294, Import1C identic pe conținut sortat, integritate-tph zero.
- **5** — explicația, neschimbată; 84-r6 și 86-r8 rămân separate.
- **6** — `GardianEditare` pur + adaptor, acum cu regula (o), `IVerificabilLaCommit`
  și F28-r4; rămâne OPȚIONAL, decis la pasul 4.
- **7** — review advers + docs; IM-r1…r7 intră în `restante.md` odată cu
  decizia de închidere (azi stau doar aici).

## Testul contra invarianților

- **II** (motorul nu cunoaște frunzele): întărit — contractul cu frunzele
  devine explicit (hook-urile primesc contextul, nu un OS cu tot modelul).
  Interzis: `IFacturaIntrare` consumat de motor. Nivelul „specific per tip"
  (contract + comportament al tipului, consumat de frunză) rămâne AMÂNAT.
- **III** (registrele scrise doar de motor): neatins — materializarea rămâne
  a motorului; planul nu e o cale ocolitoare de scriere.
- **IV** (politica = date): contractul de citire a politicilor le întoarce
  materializate, nu le mută în cod; tabelele rămân editabile fără release.
- **VI** (evaluarea = motorul): neatins; golirea valorică (75a) și FIFO-ul
  rămân în faza pură.
- 33d (calculează → validează → materializează): păstrat literal; planul
  este exact granița dintre fazele 2 și 3.
- 42b (motorul în OS non-secured PROPRIU, secvență nu cuib): păstrat — portul
  nu comite; commit-ul e al apelantului (`OperareApi`, controllerul XAF,
  Import1C).

## Deciziile

### IM-D1 — Un singur motor; XAF și API sunt căi de acces, nu implementări

Nu există „motor pentru XAF" și „motor pentru API". Portul de persistență
stă SUB securitatea, auditul, ștergerea amânată și gardianul XAF, nu în locul
lor. Scoaterea XAF din WebApi NU intră (precondițiile ei — permisiuni pe
tip/membru, pasul zero 80c, audit, `GCRecord` — sunt ale XAF-ului).

### IM-D2 — Planul independent de ORM

`PlanOperare` devine obiect simplu, imuabil după calcul, cu **identificatori
și valori**, fără referințe la entități urmărite:

- valorile calculate ale liniilor (`Valoare`, `Cantitate` semnată,
  `ValoareTva`, golirea valorică 75a);
- notele contabile și mișcările de stoc, complet rezolvate (conturi finale,
  dimensiuni pline per latură, 25/15);
- faptele TVA (`RegistruTvaService.RandTva`, deja în plan);
- finalizările de lot (25c) și cererile de documente conexe/secundare/stingeri
  automate (26d, 31e, 82) ca DATE, nu ca obiecte create;
- tranziția de stare, numărul și scadența de asignat (53b, 30c);
- **explicația**: per notă regula care a produs-o, per mișcare lotul ales și
  motivul, per dimensiune sursa din coalesce (25).

Previzualizarea (`Valideaza`) și comanda (`Opereaza`) rulează ACELAȘI calcul.
Previzualizarea nu scrie pe entități. Planul NU autorizează salvarea lui
ulterioară: comanda recalculează (stocul, politicile, stingerile se pot
schimba între cele două momente).

### IM-D3 — Fazele și cine comite

`Incarca(context, doc) → Calculeaza(fapte) → Valideaza(fapte, plan) →
Materializeaza(persistenta, plan) → efectele de după (conex, secundar,
stingerea automată) → commit al APELANTULUI`. Fazele 2–3 sunt PURE (fără I/O,
fără ORM). Anularea și stornoul urmează același tipar (plan de anulare / plan
de storno). `CommitChanges` dispare din `Motor/*` (azi 4 apeluri).

### IM-D4 — Contractele de citire sunt întrebări economice, nu `IQueryable`

Un port generic (interogare + creare + ștergere) e RESPINS: ar fi
`IObjectSpace` sub alt nume și ar păstra cuplajul cu modelul. Contractele,
puține, grupate pe mecanism, întorc rezultate materializate în LOT pentru
toate liniile documentului (fără o interogare per linie):

| Contract | Ce răspunde |
|---|---|
| Politici | tipul documentului (ancora), `PoliticaTva/Conex/Scadenta/Validare/Numerotare/InchidereTva`, `RegulaStoc`/`RegulaContare` potrivite pe tip, clasele/tipurile de material ale liniilor, `TipTva` |
| Stoc | loturile referite (preț, gestiune, dată, `LinieIntrareId`), soldul intermediar per (Lot × Repartitor × TipStoc) la orice dată din fereastra afectată (25d — NU „sold la dată", ci prefix-sum), fără a încărca întregul registru |
| Relații | dependenții (copii operați, storno-uri), imperecherile și restul (`Asignat`/`Ramas`), perioada fiscală |
| Persistență | aplică efectele planului (rânduri de registru, loturi finalizate, documente generate, imperecheri, tranziția de stare, numărul) și NIMIC altceva |

Primul adaptor e peste `IObjectSpace` (singurul din felie). `IQueryable`,
entitățile EF și navigațiile leneșe rămân în adaptor.

### IM-D5 — Registrul de lucru: efectele nesalvate ale aceleiași comenzi

Corectitudinea de azi vine parțial din identity map-ul unui singur
ObjectSpace: FIFO-ul pe loturi peste liniile aceluiași document, conexul care
vede liniile sursei, perechea 581 cu imperecherea automată (82c: „înainte de
commit"), DSC-ul cu spargerea pe loturi (37b). Într-un plan materializat de
adaptor, a doua alocare trebuie să vadă efectul primei **explicit**: planul se
construiește secvențial peste un „registru de lucru" (overlay în memorie peste
răspunsurile contractelor de citire), nu peste starea ORM-ului. Asta e partea
grea a feliei și primește SPIKE înaintea oricărei interfețe (pasul 0).
Concurența între operatori rămâne parcată (25f); advisory lock-ul e aditiv.

### IM-D6 — Frunzele primesc contextul, nu `IObjectSpace`; nivelul specific rămâne amânat

Cele 12 hook-uri de pe `Document` (și `ILinie*`) primesc contractele de
citire (un `ContextOperare`), nu un OS. Contractul comun al bazei rămâne
**clasele de bază** `Document`/`DocumentDetaliu` (sunt cod, satisfac II);
interfețele `IDocument`/`ILinie` peste ele NU intră (EF cere tipuri concrete
în interogări; costul apare doar dacă există un al doilea model). Nivelul
„specific per tip" (`IFacturaIntrare` + comportamentul facturii ca serviciu,
consumat de frunză) = amânare cu nume, condiționată de decizia „documente
custom".

### IM-D7 — `GardianEditare`: reguli pure + adaptor de tracking

Regulile (proveniență, ancore, unicitate, read-only post-Draft, ciclu pe
`Cont.Parinte`, etc.) devin funcții pe o listă de „schimbări" (obiect, fel:
nou/modificat/șters, valorile originale); adaptorul XAF le colectează din
`ModifiedObjects` în `Committing`. A doua implementare (interceptor
`SaveChanges`) NU intră — apare doar cu un host fără XAF. Pasul zero de
securitate (80c) rămâne în adaptor.

### IM-D8 — Adaptorul EF direct: amânat, cu precondiții numite

Un adaptor peste `DbContext` fără XAF cere: permisiuni pe tip/membru, ștergere
logică (`GCRecord`), gardian, audit, `CreateProxy`/`AutoInclude` (41c). Fără un
consumator cu nume (worker, host fără XAF, async măsurat pe WebApi) nu se
construiește. Contractele din D4 trebuie să-l facă POSIBIL, nu să-l livreze.

### IM-D9 — Ancora `TipDocument.ClrType` și rezoluția conexului rămân

Decizia 20 (un singur nomenclator de tipuri, și e codul) nu se atinge.
Identificatorul semantic stabil + registrul explicit de comportament/fabrică
(propunerea externă) devin decizie proprie DOAR când apar assembly-uri de
extensie; până atunci `GenereazaConex` își caută clasa unde o caută azi, dar
prin contractul de persistență (D4), nu prin `os.CreateObject(tipClr)` în
motor.

### IM-D10 — Regula de oprire a feliei

Felia e închisă când, pe codul final:

- ModelCheck e verde pe AMBELE profiluri cu TOATE probele existente
  neschimbate în text (cifrele ≥ cele de la deschidere: bugetar 1294 /
  privat 1450, la închiderea feliei 28 — amendament 89; cifrele inițiale
  ale contractului erau privat 978 / bugetar 927) plus probele pure noi;
- raportul de reconciliere Import1C pe Flax e IDENTIC pe conținut sortat cu
  baseline-ul (standardul feliilor 27–28, 89h; textul inițial: „byte-cu-byte”;
  precedentul DIM-4; 12 luni / 0 FAIL, DUK ok) și SQL-ul din
  `--dump-integritate-tph` pe baza de import întoarce zero rânduri;
- `refuzuri.ps1` 294/294 (nicio schimbare de comportament pe securitate; cifra inițială a contractului era 80/80, actualizată la închiderea feliei 28);
- perf pe HTTP ≤ cifrele din `p5-perf-masuratori.md` (59), inclusiv cifra
  de dry-run măsurată la pasul 0a — niciun contract de citire nu introduce o
  interogare per linie;
- schimbările de schemă intră doar prin decizie proprie, reanalizată la
  momentul ei (amendament 89; textul inițial: „ZERO schimbare de schemă”);
  ZERO schimbare de sârmă în DTO-urile existente; explicația din plan intră
  ADITIV în răspunsul lui `valideaza`;
- `openapi.json`/`api-types.ts` regenerate fără drift, `metadata.json` la zi.

## Pașii (un agent per pas, regulă de oprire per pas, verificare independentă)

0. **Spike registrul de lucru** (D5), pe scenariul cel mai greu: FCL cu DSC
   pin + FIFO peste mai multe linii pe același lot, plus perechea 581 cu
   imperecherea automată. Livrabil: o notă în `docs/` cu forma overlay-ului
   și dovada că ModelCheck rămâne identic pe scena FCL/DSC/VIR cu overlay-ul
   în loc de identity map. Oprire: dacă overlay-ul cere mai mult decât planul
   secvențial + răspunsurile în lot, felia se REPROIECTEAZĂ înainte de pasul 1.
1. **Planul POCO** (D2, fără explicație încă): `PlanOperare` cu Id-uri +
   valori; `Valideaza` nu mai scrie pe entități (comentariul „ATENȚIE" din
   `MotorOperare.cs` dispare, 76c pierde „al doilea OS"). Oprire: ModelCheck
   identic; proba nouă „dry-run-ul lasă entitățile neatinse" pe fiecare tip.
2. **Contractele de citire** (D4), mecanism cu mecanism: politici → stoc →
   relații, cu adaptorul peste `IObjectSpace`; `GetObjectsQuery` dispare din
   `Motor/*` în afara adaptorului. Oprire: ModelCheck identic; perf pe HTTP
   măsurată după fiecare mecanism (fără interogare per linie).
3. **Hook-urile pe context** (D6): cele 12 hook-uri + `ILinie*` primesc
   `ContextOperare`; frunzele nu mai referă `IObjectSpace`. Oprire: ModelCheck
   identic; `grep IObjectSpace BusinessObjects/Documente` = 0 în afara
   culegerii (`CreeazaLot`, `LoturiCulegereService` rămân pe seam-ul lor, 56).
4. **Materializarea și comenzile** (D3): `Materializeaza` peste contractul de
   persistență; anulare, storno, stingerea automată, conex/secundar ca efecte
   ale planului; `CommitChanges` iese din `Motor/*`. Oprire: ModelCheck
   identic; Import1C identic pe conținut sortat; `refuzuri.ps1` 294/294
   (cifra inițială 80/80).
5. **Explicația** (D2, aditiv): planul poartă regula/lotul/sursa dimensiunii;
   `valideaza` le expune în DTO (aditiv); clientul le arată pe ASM (76h) și
   pe panourile de stingere. Oprire: drift zero, smoke în browser.
6. **`GardianEditare` pur + adaptor** (D7) — OPȚIONAL în felie, decis la
   pasul 4 pe cifra de efort rămasă.
7. **Review advers + docs**: scenariile din D5 (efect nesalvat văzut de a
   doua alocare; plan stale între previzualizare și comandă; refuz după
   materializare fără urme; storno pe plan), decizia NNN (cu „Regula
   durabilă"), README-ul jurnalului, `restante.md`, CLAUDE.md §Stare,
   stare-curenta (domeniu-si-operare: fazele, contractele), istoricul.

## Ce NU intră (amânări cu nume, textul aici)

- **IM-r1** nivelul specific per tip (`IFacturaIntrare` + comportament ca
  serviciu) — condiționat de decizia „documente custom" (51a/54d rămân).
- **IM-r2** adaptorul EF direct și hostul fără XAF (D8).
- **IM-r3** `IDocument`/`ILinie` peste clasele de bază (D6).
- **IM-r4** identificatorul semantic al tipului + registrul de fabrici (D9).
- **IM-r5** async efectiv în host-uri: felia face I/O-ul izolabil; trecerea
  la `async` în `OperareApi`/controllere e felie proprie, cu cifră (59).
- **IM-r6** concurența între operatori (25f) — overlay-ul din D5 nu o rezolvă
  și nu pretinde.
- **IM-r7** extensia per client a modelului (`DbContext` de extensie sau
  configurații din alt assembly + migrații per client) — spike separat,
  înaintea oricărei decizii de produs.

## Criteriul de prioritate

Felia e MARE (schimbă granița domeniu/persistență) și nu livrează funcție
nouă vizibilă în afară de explicație. Se pornește când cel puțin unul din
semnalele următoare e real, nu presupus:

1. felia 24 „Explică" e cerută — atunci pașii 1–2 sunt PRECONDIȚIA ei, nu o
   lucrare paralelă;
2. throughput-ul WebApi pe operare/dry-run devine cifră (59) și cere async;
3. al doilea host/consumator al motorului fără XAF capătă nume (worker,
   conector care nu vrea bootstrap-ul XAF).

Până atunci rămâne „Următorul pas" în CLAUDE.md cu contractul ăsta ca punct
de reluare. Alternativa „portul generic ca schelă" e respinsă explicit
(tech-debt care devine canonic prin vechime).
