# 84. Pasul 5, felia 24 — politicile: seed-ul aliniază, rolul `Configurator`, cele șapte ecrane, potrivirea ca funcții pure și „Explică"

- **Data**: 2026-09-11
- **Stare**: activă (execută decizia 83 a–j; închide 81-r8, 81k pe ecranele de politică și „Explică"; amendează 38c/64 — gardurile de nivel minim al contării devin contract declarat pe clasa documentului; amendează 83j — ștergerea unui `TipTva` referit se refuză ca dezactivarea; avans declarat pe IM-D2/IM-D4)
- **Docs**: `docs/api/p5-felia24-politici-explica-contract.md` (F24-D1…D7 + §Închidere), `docs/decizii/083-seed-provenienta-goluri-configurator.md`, `nou/.../Module/DatabaseUpdate/ContaSeeder.cs` (`Aliniaza`, `RaportSeed.cs`), `nou/.../Module/BusinessObjects/Politici/TipuriConfigurabile.cs`, `nou/.../Module/DatabaseUpdate/Updater.cs` (`SeedRolConfigurator`), `nou/.../Module/Motor/Potrivire.cs`, `Fapte.cs`, `nou/.../Module/Api/Politici/ExplicaApply.cs`, `ExplicaDtos.cs`, `nou/.../WebApi/API/Conta/PoliticiController.cs` (`explica`), `nou/Atlas.Conta.Client/src/felii/politici/` (șapte ecrane noi, `Explica.tsx`, `GrilaPolitica.tsx`), `nou/tools/ProbeHttp/refuzuri.ps1`, `nou/tools/ModelCheck/Program.cs` (F24-V/R/P/E)

## Context

Decizia 83 a tranșat trei întrebări (re-seed-ul pe `DinSeed`, golurile privat
NIM/IMP, rolul `Configurator`) și a lăsat execuția feliei 24, împreună cu
restul lui 81k: ecranele de politică fără editor (`RegulaContare`,
`RegulaStoc`, `PoliticaTva`, `PoliticaConex`, `PoliticaValidare`,
`MapareD300`, `MapareD394`) și explicabilitatea configurației. Contractul
feliei a fixat două track-uri independente (A = configurația, B = „Explică")
și forma faptelor ca avans pe contractul de izolare a motorului (IM-D2/IM-D4).

Felia s-a livrat pe `multiagent-delivery`: două track-uri în paralel, pe
worktree-uri și baze ModelCheck separate (`MODELCHECK_BAZA_SUFIX`), un agent
per pas, verificare independentă și commit per pas, review advers la
închidere, cu fix-urile aplicate înaintea probei finale.

## Tranșări

**(a) Seed-ul ALINIAZĂ (83a–d executate).** `ContaSeeder.Aliniaza<T>(os,
cheie, potrivire, seteaza)` e singura formă a lui „găsit == null ⇒ creează"
pe cele 17 tipuri `ICuProvenienta`: caută VIU pe cheia indexului unic (81c),
apoi cu `IgnoreQueryFilters` (rândul șters logic rămâne șters și se spune);
creează cu timbru sau, pe rândul `DinSeed`, aplică `seteaza` peste un
snapshot al proprietăților SCALARE mapate și tipărește fiecare diferență
`tip / cheie / câmp: vechi → nou`; rândul manual nu se atinge. Cheia se
citește din modelul design-time (indexurile unice), iar un `seteaza` care o
atinge pe un rând existent aruncă (83b); un ObjectSpace care nu e
`EFCoreObjectSpace` aruncă, nu tace. `Seed` întoarce `RaportSeed` (contoare
per tabel create / corectate / manuale / șterse + lista corecțiilor), tipărit
la final. Site-urile care gardau „orice regulă pe tipul X" trec pe cheia
COMPLETĂ (`RegulaStoc` pe (tip, latură, clasă); `RegulaContare` pe (tip,
material, natură, semn) — `RegulaContareLipsa`, care ignora semnul, a murit);
derivatele (`SeedContare6xxDin3xx`, `SeedContareVanzare`) recalculează cheia
`DinSeed`, sar cheia acoperită manual și NU creează un al doilea rând când
rândul seed stă pe altă cheie (ar dubla potrivirea motorului). Câmpuri ținute
deliberat în afara alinierii: `TipTva.Activ` (al lui `SeedTipuriTvaInactive`),
`PoliticaNumerotare.UrmatorulNumar` (stare de runtime — se scrie doar pe rândul
nou), `TipMaterial.ContImplicitId` (derivarea umple doar goluri). Devieri de
comportament asumate: `Cont.Functie`/`RolTert`/`Denumire`/`Parinte`,
`TipMaterial` promovate din import, ancora `TipDocument.TipTvaImplicit`,
`PoliticaConex`/`PoliticaValidare` se aliniază DOAR pe timbru aprins (înainte
se rescriau necondiționat sau doar la creare); curățenia istorică a rândului
`PoliticaValidare` FCL din P1 șterge doar rândul `DinSeed` (review M2).

**(b) Golurile privat (83f–g executate).** `TipTva` `IMP` („Achiziție din
import — TVA prin DVI", 0 %, `Neimpozabil`, activ) **fără cod SAF-T**:
nomenclatorul ANAF (16.02.2026) n-are cod pentru factura furnizorului extern
fără TVA; `301204` (rd. 24) / `300604` (simplificare) sunt ale DVI-ului
(83-r3, 83-r4); `308302` (rd. 29) ar fi contrazis „nemapat pe D300".
`NIM.CodSafTAchizitie = 308302` („achiziții scutite de taxă sau
neimpozabile", exact rândul 29 pe care seed-ul mapează deja NIM×A): 83f pune
NIM pe achiziție, iar un implicit fără cod SAF-T ar fi fost o gaură creată de
noi. `ImpliciteTva` 6 → 10 (`FCT`/`RLF` × `NeinregistratRo` → NIM, × `ExtraUe`
→ IMP); `IMP` nemapat deliberat pe D300 („baza și taxa se declară din DVI" /
„tip de achiziție") și pe D394 (partener extra-UE) pe ambele sensuri. Bugetar:
zero rânduri.

**(c) O singură listă, un singur rol (83h–i executate).**
`Politici.TipuriConfigurabile` = lista EXPLICITĂ a celor 17 tipuri
(reflecția e a probei, în ambele sensuri); o consumă `PoliticiApply.TipuriCitite`
(∪ `Partener`/`Produs`), `VerificareProfilService` (buclă cu etichete per tip —
tip fără etichetă pică zgomotos) și rolul. `Updater.SeedRolConfigurator(os)`:
`ReadOnlyAllByDefault` + Create/Write/Delete pe fiecare tip din listă,
permisiunile REAPLICATE la fiecare seed (rolul e al release-ului, nu al
bazei); fără Deny suplimentar (simetric cu `Cititori`); **rolul e de
producție** — seed-uit în afara blocului `#if !RELEASE` — iar userul dev
`Configurator` (fără parolă) rămâne înăuntru, ca `Cititor`. Al patrulea
oracol în `refuzuri.ps1`: scrie politicile (201/204/200), primește aceleași
422 ale gardianului ca `Admin` (inclusiv `Cod` pe ancoră — are Write pe
`TipDocument`, refuzul e al domeniului), 403 pe documente, comenzi,
`Partener`, `Societate`.

**(d) Cele șapte ecrane (F24-D4).** Peste `GrilaPolitica`, care capătă modul
`formular` (grupurile sunt ale ECRANULUI ⇒ `Editing mode="popup"`; coloanele
ascunse intră în formular doar listate explicit — capcană DevExtreme cu
sursa în cod) și `laRandNou`. `RegulaContare` = popup cu „Potrivire / Conturi
/ Dimensiuni comune / Override debit / Override credit", cele 24 de FK-uri
doar în formular; **81-r8 închisă**: rândul nou propune VIZIBIL
`SursaContDebit = SursaContCredit = Explicit` (și `SursaContrapartida` pe
`PoliticaTva`), refuzul `Explicit ⇒ cont` rămâne al gardianului și apare în
popup, fără 500. Rândul nou propune și valori pentru enum-urile fără membru 0
(`Latura`, `TipStoc`, `Directie`, `Sens`, `Tip`) — afordanță; gardul e (j).
Lookup-ul `RandD300` filtrat pe `Operatiuni` (69b, afordanță); căutarea
lookup-urilor de coloană pe `Cautare` prin `editorOptions.searchExpr` (77a).
`Unitate*` rămân read-only (setul OData n-are controller; zero seturi noi,
76a). Semnul regulii de stoc NU se propune după latură (seed-ul are ambele
semne pe Predator).

**(e) `Motor/Potrivire.cs` (F24-D5).** Faptele = `record struct`-uri plate
(`RegulaContareFapt`, `RegulaStocFapt`, `LinieFapt`, `LaturiFapt`,
`PoliticaConexFapt`, `PoliticaTvaImplicitFapt`, `TipTvaFapt`); funcțiile
`Contare` (SemnFiltru ÎNAINTEA nivelurilor; Tip exact > Natură > generic;
câștigătorul = PRIMUL de la nivelul cel mai înalt, în ordinea listei —
ordinea bazei, fără ORDER BY, ca înainte; candidații cu `MotivEliminare`),
`Stoc` (per latură; Clasă exactă bate genericul; genericul DOAR pe
`Natura = Stoc`), `Cont` (`RezolvaCont` mutat, cu `SursaRezolvata`), `Conex`,
`TvaImplicit` (clasamentul din `ImpliciteService`, cu textele; rândul
câștigător inactiv sare treapta). `Fapte` = singura ortografie entitate → fapt
(`Natura`/`ClasaId` null = Tip necunoscut — echivalent cu `(NaturaClasa)0` de
înainte: nivelul de natură cere explicit `NaturaFiltru != null`). Motorul
consumă `Potrivire` (`RezolvaCont` a murit, `PotrivesteReguliStoc` e wrapper,
conexul prin `Conex`, `ImpliciteService` = doar citirile). Oglinzile au
murit: `GetObjectsQuery<RegulaContare>` = 0 în `BusinessObjects/Documente`;
`DescarcareService.TipStocPentruClasa` a murit — divergența declarată (cădea
pe genericul de stoc fără gardul `Natura = Stoc`) avea expunere reală zero
(`RestNedescarcat` și `liniiStoc` filtrează deja pe Stoc) și e fixată de proba
pură F24-P4.

**(f) Gardul de nivel minim al contării = contract DECLARAT pe clasa
documentului (amendează forma lui 38c/64; review F2).** `[GardContare(natura?,
nivelMinim, mesaj)]` pe `FacturaIesire` (Stoc ⇒ exact), `DescarcareGestiune`
(toate ⇒ exact), `ReturClient` (Stoc ⇒ exact), trezorerie (Virament ⇒ cel
puțin natura). Se aplică O SINGURĂ dată, generic, în `Document.ValideazaOperare`
prin `Potrivire.Contare(...).Nivel < nivelMinim` — deci gardul capătă TOATE
axele potrivirii (și `SemnFiltru`, pe care cele trei oglinzi 38c n-o aveau —
64) — iar explicația citește ACELAȘI atribut. Textele refuzurilor sunt
neschimbate.

**(g) `GET api/politici/explica` (F24-D6).** Explică CONFIGURAȚIA pe o linie
ipotetică (tip, tip de material, semn, dată, laturi, partener, produs), fără
document: blocuri `Contare` (câștigător, nivel, candidați cu motiv, conturile
rezolvate cu SURSA fiecăruia, `PostareExplicita` pe NTC și pe DEC — linia
explicită bate regula punctual — și `Rezerve[]`: ce ar refuza motorul deși
regula s-a potrivit — gardul (f) și `PoliticaValidare.NaturaInterzisa`),
`Stoc[]`, `Tva`, `Conex`, `Implicit` (verdictul întreg al culegerii), rândurile
`Validare`/`Scadenta`/`Numerotare`; fiecare rând poartă `DinSeed`, fiecare bloc
`Concluzie` formată pe SERVER (cu rezerve: „Regula câștigătoare ar posta X = Y,
dar operarea ar fi refuzată: …"), inclusiv concluziile de absență. Enum-urile
verdictului stau în `BusinessObjects/Comun/Enums.cs` cu `[XafDisplayName]`
(pe sârmă ca string, etichete prin `metadata.json`). Ordinea pe sârmă (80a):
400 `EroriDto` (tip necunoscut, semn ∉ {−1, +1}, guid malformat, `tipMaterial`
lipsă) → 403 (gate `CanRead` pe `TipuriConfigurabile` ∪ TipMaterial /
Repartitor / Partener / Produs, pe ușa securizată) → 422 (referință
inexistentă sau invizibilă, fraza unică 80f, prin `Any` pe nomenclator —
`GetObjectByKey` castează sub TPT) → 200 pe ușa NON-SECURED. Proba de
consistență (42c): conturile explicației == rândul din `RegistruContabil`
după `Opereaza` pe un document real echivalent.

**(h) Panoul „Explică" (F24-D7).** `/politici/explica`, starea = URL;
interogarea pleacă singură cu tip + tip de material; opt carduri cu
`Concluzie`-a serverului, câștigătorul, candidații eliminați cu motivul
etichetat, marcajul seed/manual, `Rezerve` ca atenție; zero calcul și zero
frază de verdict în TS. `GrilaPolitica.explica` = URL-ul panoului pentru
rândul FOCALIZAT (11 ecrane; `ReguliContare` trimite și materialul și
semnul; D300/D394 fără buton — cheia lor nu are tip de document); „Vezi
politica" înapoi spre grilă, fără focalizare pe rând.

**(i) Un enum fără membru definit se refuză pe toate ușile (review F1).**
`GardianEditare` verifică generic, pe orice `ICuProvenienta` modificat, că
fiecare proprietate enum (ne-`[Flags]`) poartă un membru definit — altfel
OData scria tăcut `0` (`Directie = 0` cădea pe COLECTAT, `Latura = 0` scria
stocul pe primitor, `TipStoc = 0` deschidea un registru inexistent).
Convenția „default invalid" din `Enums.cs` are acum gardul ei.

**(j) Ștergerea unui `TipTva` referit se refuză ca dezactivarea (review M3;
amendează 83j).** Un `DELETE` pe un tip referit de implicite, ancoră, parteneri,
produse, linii de document sau `RegistruTva` iese 422 — altfel un click de
`Configurator` lăsa liniile neoperabile, implicitele căzând tăcut pe treapta
următoare și seed-ul următor ne-rulabil (mapările D300/D394 aruncă pe tipul
lipsă). Alternativa corectă (dezactivarea) era refuzată, iar cea distructivă
trecea (62f).

**(k) Rândul de probă al lui `Admin` din `refuzuri.ps1`** ocupa cheia de seed
`FCT × ExtraUe` (fără dată) și, șters logic la final, împiedica implicitul
`IMP` la re-seed (83j). Rândul de probă stă pe o dată proprie; fantomele au
fost purjate fizic pe baza host-ului.

## Review advers și probe

| Scenariu | Verdict |
|---|---|
| Re-seed-ul corectează un rând al clientului | doar cu timbru aprins; probat: editat (timbru stins) ⇒ neatins (F24-V2), corectat pe ușa de sistem ⇒ readus (F24-V1), șters ⇒ rămâne șters (F24-V3) |
| Helper-ul schimbă o cheie | imposibil prin construcție + gard din indexurile unice (F24-V5) |
| Două rânduri seed pe aceeași cheie | al doilea pică pe indexul unic la commit (eroare de bază, zgomotoasă; fără probă dedicată — 84-r9) |
| `Configurator` scrie un document printr-un FK de politică | niciun tip configurabil nu referă documente/registre; 403 pe REST măsurat; deep insert OData neprobat (84-r11) |
| `Configurator` dezactivează / șterge `TipTva` folosit | gardianul refuză ambele (j) |
| `Potrivire` diverge de motor | motorul o consumă; ModelCheck identic + Import1C integral identic |
| `TipStocPentruClasa` fără gardul `Natura = Stoc` | expunere zero, fixat de F24-P4 |
| `explica` dezvăluie un `TipMaterial` invizibil | 422 înainte de calcul (măsurat pe HTTP) |
| Concluzia contrazice motorul (postare explicită DEC/NTC, garduri 38c/VIR, `NaturaInterzisa`) | `PostareExplicita` + `Rezerve` (f, g); F24-E5/E8 |
| Enum 0 scris prin OData | (i), probat pe ModelCheck și HTTP |
| `IMP` pe FCL | nemapat cu motiv, gardianul nu-l interzice |

Cifre (verificate independent de main la fiecare pas): ModelCheck privat
978 → 1016 OK, bugetar 927 → 946 OK, 0 FAIL (probele F24-V1…V9, R1…R3,
P1…P8, E1…E8, G1…G3); `refuzuri.ps1` 42 → 117 probe, 117 PASS (Admin /
Cititor / User / Configurator); seed pe Flax: prima trecere `11 create, 1
corectate`, a doua `0 / 0`; **Import1C integral** pe binarul pasului 1
(`run-f24a`, 2h24 sub încărcare): CONTRACT ÎNDEPLINIT, 932 avertismente,
raport de reconciliere IDENTIC cu baseline-ul F18 (467 linii, doar antetul
cu data diferă); pe codul închis al feliei (`run-f24`, 1h51): CONTRACT ÎNDEPLINIT, 932
avertismente, raport IDENTIC cu baseline-ul F18 — proba supremă a lui 84a/e/f.
Commit-urile: `9e836e4` (sufix ModelCheck), `5da99e7` (pas 1), `1129df2`
(pas 2), `48fc5ef` (merge pas 4, `5d15cb0`), `1519447` (pas 5), `5cc3ea8`
(merge pas 3), `7139918` (pas 6), `cf607e8` (fix-urile review-ului).

## Ce rămâne deschis

- **84-r1** `SDD`/`SFD` fără cod SAF-T de achiziție (nu sunt implicite de
  achiziție azi; devine relevant când un implicit le pune pe achiziție, cum a
  făcut 83f cu NIM).
- **84-r2** `N9.CodSafTLivrare = 310310` (rd. 10.1 în nomenclator) în timp ce
  seed-ul mapează N9 pe rd. 11 (nomenclatorul are `310357`/`310358` pentru cota
  9 % conform art. III din Legea 141/2025) — preexistent, de tranșat odată cu
  D300/SAF-T.
- **84-r3** Raportul de profil n-are categoria „rând de seed lipsă" (șters de
  utilizator): seed-ul îl raportează în consolă, iar lipsa unui `TipTva`
  referit de mapări aruncă din altă funcție; recrearea la runtime rămâne 83-r1.
- **84-r4** Întoarcerea din panou în grilă nu focalizează rândul; `Unitate`
  are `EntitySet` fără controller OData (câmpurile `Unitate*` sunt read-only în
  formularul regulii de contare).
- **84-r5** Captions: `SursaCont` și mai mulți membri ai politicilor n-au
  `[XafDisplayName]` (ecranele poartă caption-ul în cod; familia 70-r5).
- **84-r6** `explica`: gate-ul de citire e pe TIP, nu pe obiect (un rol viitor
  cu criteriu de obiect pe `Cont` ar vedea simboluri prin explicație — aceeași
  limită ca `verificare`, 80e); `tip` se rezolvă înaintea gate-ului (400 pe cod
  inexistent pentru orice rol — oracol pe ancore, ca `api/implicite`).
- **84-r7** Rolul `Configurator` e al release-ului: permisiunile se reaplică la
  fiecare seed; pe RELEASE există rolul, nu și un user (adminul îl atribuie);
  navigația XAF rămâne 83-r5.
- **84-r8** `Cont.DimensiuniObligatorii` (bugetar) intră acum în aliniere pe
  rândurile `DinSeed` — primul câmp editabil de client aliniat pe o bază vie;
  expunerea reală e a lui 81-r3 (măsurat 0 corecții).
- **84-r9** Ordinea candidaților la egalitate de nivel = ordinea bazei (fără
  ORDER BY), ca înainte; unicitatea o dă indexul (81c), nu potrivirea; „două
  rânduri seed pe aceeași cheie" pică pe index, fără probă de seed.
- **84-r10** = F24-r1: `Import1C/Catalog.IncarcaContare` pe `Potrivire`
  (predicția ar acoperi orice tipar de regulă), cu baseline-ul ca probă.
- **84-r11** Neprobate: deep insert OData pentru `Configurator`; smoke vizual
  XAF pe gardurile rescrise (acoperite de ModelCheck prin aceleași hook-uri).
- **84-r12** Golul valoric `Stoc` pe LDI nu depinde de semn (regula e +1 pe
  predator; semnul se materializează din `Directie`, 28a) — observație pentru
  cine citește panoul, nu defect.
