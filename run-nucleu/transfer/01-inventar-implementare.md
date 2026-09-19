# Inventarul implementării curente, pe responsabilități

Explorarea 1 a pasului 3 (transferul). Clasificare, nu propunere: nimic de aici
nu compară cu cubul și nimic nu judecă. Toate trimiterile sunt `fișier:linie` pe
`p5-f28-tph` la 2026-09-19. Rădăcina implicită a căilor scurte:
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`.

Ce n-am putut completa din cod e spus explicit în §D (rândurile de seed
generate în buclă) și §G (numărul de probe per FUNCȚIE de suită).

---

## A. Motorul (`Motor/*.cs`, 21 fișiere, 6.249 linii)

`fel`: `regulă` = calcul care ar putea fi pur; `mecanism` = scrie/citește forma
registrelor de azi; `gardian` = admisibilitate; `adaptor` = entitate → fapt plat,
cheie → rând, tranzacție.

| serviciu / clasă | responsabilitate | linii | citește (tabele) | scrie (tabele) | `IObjectSpace` | fel |
|---|---|---|---|---|---|---|
| `MotorOperare` (`Motor/MotorOperare.cs:20`) | operarea / anularea / stornarea unui document; singura cale de scriere a registrelor | 814 | `PerioadeFiscale`, `TipuriDocument`, `TipuriMaterial`, `ReguliStoc`, `ReguliContare`, `PoliticaTva`, `PoliticaValidare`, `PoliticaConex`, `PoliticaNumerotare`, `PoliticaScadenta`, `TipuriTva`, `Conturi`, `Loturi`, `Repartitori`, cele 3 registre | `RegistruStoc`, `RegistruContabil`, `RegistruTva`, `Loturi` (preț/dată), `Documente` (Stare/Numar/DataOperare/TotalStingere), `Documente`+`DocumenteDetalii` (conex/secundar), `PoliticaNumerotare.UrmatorulNumar` | **da** (peste tot) | mecanism (+ gardian în fazele 1–2) |
| `GardianEditare` (`Motor/GardianEditare.cs:61`) | gardianul transversal de scriere pe `Committing`-ul ObjectSpace-urilor SECURED | 1.312 | tot ce verifică (documente, politici, nomenclatoare, perioade) | `ICuProvenienta.DinSeed = false` (`:862`) — singura scriere | **da** | gardian |
| `SolduriService` (`Motor/SolduriService.cs:59`) | snapshot-urile de perioadă + citirea cumulată (snapshot + rulaje) | 529 | `RegistruContabil`, `RegistruStoc`, `Documente`, `Imperecheri`, `PerioadeFiscale` | `SolduriPerioadaContabil`, `SolduriPerioadaStoc`, `PartideDeschise` (SQL brut) | **da** (cere `EFCoreObjectSpace`, `:517`) | mecanism |
| `AmortizareService` (`Motor/AmortizareService.cs:65`) | situația fișei din registru + aritmetica lunii + generatorul AMO | 460 | `RegistruImobilizari`, `Imobilizari`, `PoliticaAmortizare`, `RegulaDeductibilitate`, `PerioadeFiscale`, `AmortizariLunare` | `AmortizareLunara` + liniile ei (draft), `:222-243` | **da** | regulă + mecanism (vezi mai jos) |
| `PerioadaService` (`Motor/PerioadaService.cs:12`) | lanțul perioadelor: verificare → închidere → redeschidere | 374 | `PerioadeFiscale`, `Documente`, `InchideriTva`, `AmortizariLunare`, `PoliticaInchidere`, `PartideDeschise` | `PerioadeFiscale` (Inchisa/InchisaLa/InchisaPrimaOara), `InchideriPerioada` | **da** (`FOR UPDATE` brut, `:338`) | gardian + mecanism |
| `ImperechereService` (`Motor/ImperechereService.cs:25`) | stingerea: plafon per contrapartidă × sens, rândul invers, stingerea automată | 371 | `Documente`, `Imperecheri`, `PerioadeFiscale` | `Imperecheri` | **da** | regulă + mecanism |
| `LoturiCulegereService` (`Motor/LoturiCulegereService.cs:42`) | nașterea / sincronizarea / curățenia loturilor la CULEGERE | 312 | `Loturi`, `DocumenteDetalii`, `TipuriMaterial`, `RegistruStoc` | `Loturi`, `DocumenteDetalii.LotId` | **da** (folosește `ModifiedObjects`, `GetObjectsToDelete`) | mecanism |
| `StocService` (`Motor/StocService.cs:18`) | valoarea ieșirii care golește, gardianul de sold ≥ 0, FIFO | 299 | `RegistruStoc` (prin `SolduriService`), `Loturi`, `Repartitori` | `DocumenteDetalii.Valoare` (`:119`) | **da** (cu excepția a două funcții pure) | regulă + gardian |
| `InchidereTvaService` (`Motor/InchidereTvaService.cs:33`) | decontul lunar de TVA: analiză → 3 linii → draft ITV | 281 | `PoliticaInchidereTva`, `InchideriTva`, `PerioadeFiscale`, `TipuriMaterial`, atomii contabili | `InchidereTva` + `NotaContabilaDetaliu` (draft) | **da** | regulă + mecanism |
| `VerificareProfilService` (`Motor/VerificareProfilService.cs:38`) | raportul „unde s-a abătut baza de la profilul livrat” | 277 | toate tabelele `ICuProvenienta` + țintele lor | nimic | **da** | gardian (doar raportează) |
| `Potrivire` (`Motor/Potrivire.cs:67`) | potrivirea politicilor pe o linie: contare, stoc, cont, conex, implicit TVA | 244 | — (fapte plate) | — | **nu** | **regulă (pură)** |
| `CorectieService` (`Motor/CorectieService.cs:306`) | corecția = storno legat + draft nou copiat prin metadata EF | 160 | `Documente`, `DocumenteDetalii`, `Loturi`, `RegistruTva` | documentul nou + liniile + lotul renăscut; `RegistruTva.Perioada*` pe rândurile de storno (`:382-386`) | **da** (cere `EFCoreObjectSpace`, `:432`) | mecanism |
| `RegistruTvaService` (`Motor/RegistruTvaService.cs:196`) | derivarea faptelor fiscale + perioada de declarare | 193 | `PoliticaTva`, `TipuriTva`, `PerioadeFiscale`, `RegistruTva` | nimic (întoarce `RandTva[]`) | **da** | regulă (descriere, nu entitate) |
| `TvaService` (`Motor/TvaService.cs:10`) | formula Valoare/ValoareTva pe linie; gardul taxării inverse pe livrare | 171 | `TipuriTva`, `PoliticaTva`, `TipuriDocument` | `DocumentDetaliu.Valoare/ValoareTva` | **da** (fără `CalculeazaValori`) | regulă |
| `ImpliciteService` (`Motor/ImpliciteService.cs:16`) | implicitul de TVA la culegere (citirile) + „cine e partenerul documentului” | 105 | `Parteneri`, `Produse`, `TipuriDocument`, `PoliticaTvaImplicit`, `TipuriTva` | nimic | **da** | adaptor (citiri) |
| `DescarcareService` (`Motor/DescarcareService.cs:14`) | restul nedescărcat al FCL + generatorul draftului DSC | 145 | `DescarcariGestiuneDetalii`, `TipuriMaterial`, `ReguliStoc`, `Loturi`, `RegistruStoc` | `DescarcareGestiune` + liniile ei (draft) | **da** | regulă + mecanism |
| `Fapte` (`Motor/Fapte.cs:8`) | **singura** ortografie entitate → fapt plat | 69 | `ReguliContare`, `ReguliStoc`, `PoliticaConex`, `PoliticaTvaImplicit`, `TipuriTva`, `TipuriMaterial` | nimic | **da** | **adaptor** |
| `RandDupaCheie` (`Motor/RandDupaCheie.cs:11`) | rândul după cheie pe rădăcina ierarhiei EF, cu tipul verificat după (89) | 53 | orice tabelă | nimic | **da** | adaptor |
| `GardianPerioada` (`Motor/GardianPerioada.cs:10`) | perioada deschisă, cu `FOR SHARE` pe rândul lunii | 34 | `PerioadeFiscale` (SQL brut) | nimic | **da** (cere `EFCoreObjectSpace`) | gardian |
| `DimensiuniResolver` (`Motor/DimensiuniResolver.cs:7`) | coalesce pe cele 8 componente, prima sursă nenulă câștigă | 26 | — | — | **nu** | **regulă (pură)** |
| `TranzactieComanda` (`Motor/TranzactieComanda.cs:9`) | deschide tranzacția comenzii pe `DbContext`-ul OS-ului | 20 | — | — | **da** (cere `EFCoreObjectSpace`) | adaptor |

Tipurile-valoare din `Motor/` care nu sunt servicii: `OperareException`
(`MotorOperare.cs:8`), `CheieStoc`/`MiscareStoc`/`SoldStoc` (`StocService.cs:7-16`),
faptele plate (`Potrivire.cs:14-36`), `RandRegistru`/`SituatieImobilizare`/
`BazaAmortizare`/`RegulaSnapshot`/`LinieIesire`/`LinieAmortizare`
(`AmortizareService.cs:9-62`), `RandReconstructie`/`RaportReconstructie`
(`SolduriService.cs:13-20`).

### A.1 `MotorOperare` — fazele

**Faza 1+2, `CalculeazaSiValideaza` (`:62-300`)** — nu scrie nimic în registre;
`Valideaza` (`:49-60`, dry-run) o cheamă și prinde `OperareException`:

| pas | linii | ce face |
|---|---|---|
| gard de stare | `:63-64` | numai `Draft` |
| normalizare `DataInregistrare` | `:65-69` | `default` ⇒ `Data`; refuz dacă e anterioară lui `Data` |
| gard de perioadă | `:70` | `GardianPerioada.VerificaDeschisa(DataInregistrare)` |
| captura TVA-ului cules | `:78-80` | înainte de pregătire (pregătirea îl zerorizează) |
| hook `PregatesteOperare` | `:82` | derivata materializează `Valoare`/`ValoareTva` pe linii |
| ancora tipului | `:83` | `GasesteTipDocument` pe numele CLR |
| preîncărcarea claselor | `:87` | `Fapte.ClaseTip` (proiecție) |
| gardul taxării inverse | `:100` | `TvaService.VerificaTvaCulesTaxareInversa` |
| validarea declarativă | `:101` (def. `:435-448`) | `PoliticaValidare`: clasificație bugetară, natură interzisă |
| valoarea ieșirii care golește | `:114-115` | `Fapte.ReguliStoc` + potrivire TOLERANTĂ → `StocService.AplicaValoareIesire` |
| hook `ValideazaOperare` | `:117` | invarianții tipului; `:118-119` aruncă tot ce s-a cumulat |
| mișcările de stoc + gardianul de sold | `:125-126` | potrivire STRICTĂ → `StocService.VerificaSoldIntermediar` |
| notele contabile | `:137-211` | reguli de contare, postarea explicită (`ILinieCuPostareExplicita`), rezolvarea conturilor (`Potrivire.Cont`), dimensiunile per latură, normalizarea semnului |
| pasul TVA contabil | `:220-282` | `PoliticaTva` → 4426/4427 per linie, cu ramura de taxare inversă `:244-256` |
| faptele fiscale | `:290` | `RegistruTvaService.Deriva` (descriere, nu entități) |
| dimensiunile obligatorii | `:295` (def. `:482-528`) | flag-urile `DimensiuniObligatorii` ale contului, per latură |

**Faza 3, materializarea (`Opereaza`, `:303-429`)**, în ordine:

| ce scrie | linii |
|---|---|
| `Numar` din `PoliticaNumerotare` (consumă seria ABIA aici) | `:321`, def. `:790-802` |
| `DataScadenta` din `PoliticaScadenta` | `:326`, def. `:807-813` |
| finalizarea loturilor născute de linii (`PretUnitar = Valoare/Cantitate`, `Data`, atribute) | `:333-350` |
| **`RegistruStoc`** — `Data`, `TipStoc`, `LotId`, `RepartitorId`, `Cantitate = Semn × cant`, `Valoare = Semn × val`, `Document`, `Detaliu` | `:352-362` |
| **`RegistruContabil`** — `Data`, `ContDebitId`, `ContCreditId`, `Valoare`, cele 2×8 dimensiuni, `Document`, `Detaliu` | `:364-374` |
| **`RegistruTva`** — `Data` = `doc.Data` (nu `DataInregistrare`), `PerioadaAn/Luna` din `PerioadaDeclarare`, `ScrisLa`, `Sens`, `PartenerId`, `TipTvaId`, `Regim`, `Cota`, `Baza`, `Tva` | `:376-394` |
| `Document.TotalStingere` din `LiniiCreanta` | `:396-397` |
| **registrul propriu al tipului** (`IDocumentCuRegistruPropriu.MaterializeazaRegistrul`) | `:400-401` |
| documentul **conex** (clonă filtrată pe natură, `PoliticaConex`) | `:406-409`, def. `GenereazaConex` `:534-573` |
| documentul **secundar** (hook `GenereazaSecundar` al frunzei) | `:414-418` |
| `Stare = Operat`, `DataOperare` | `:420-421` |
| stingerea automată (`ImperechereService.CreeazaAutomataLaOperare`) | `:425` |
| **un singur** `CommitChanges` | `:427` |

**Conexul vs secundarul**: conexul e MECANISM de motor (politica `PoliticaConex`
decide ținta, inversarea laturilor și filtrul de natură; clona copiază
`TipMaterialId`, `LotId`, `Cantitate`, `Valoare`, `TipTvaId` — dar NU `ValoareTva`
— `AngajamentId` și dimensiunile, `:558-571`; liniile se nasc pe frunza declarată
prin `[TipDetaliu]`, `:556`). Secundarul e HOOK de frunză (`GenereazaSecundar`),
construit din câmpuri culese; motorul doar îl marchează `Autogenerat` +
`DocumentSursa`.

**Stornoul „în roșu" (`Storneaza`, `:628-705`)** — nu la liniile pe care le indică
spec-ul (`:669-681`), ci:

- gardieni: stare `Operat` `:629-630`, data ≥ `DataInregistrare` `:631-632`,
  perioada stornării deschisă `:633`, fără latură pereche operată `:634`, fără
  conexe operate `:635`, `ImperechereService.InverseazaLaStorno` `:636`, ștergerea
  conexelor draft `:637`;
- delta de stoc verificată pe soldul ≥ 0 de la data stornării `:643-646`;
- **`RegistruStoc` invers**: `Cantitate = -r.Cantitate`, `Valoare = -r.Valoare`,
  `Storno = true`, `Data = dataStorno` — `:648-659`;
- **`RegistruContabil` invers**: `Valoare = -r.Valoare`, ACELEAȘI conturi și
  dimensiuni, `Storno = true` — `:660-672` (asta e „roșul");
- **`RegistruTva` invers**: `Baza = -r.Baza`, `Tva = -r.Tva`, identitatea fiscală
  COPIATĂ (nu re-derivată), perioada = luna stornării — `:681-699`;
- `IDocumentCuRegistruPropriu.StorneazaRegistrul` `:700-701`;
- `Stare = Stornat` `:703`.

**Anularea (`AnuleazaOperarea`, `:578-622`)**: corecție directă, permisă doar fără
dependenți — latură pereche `:582`, conexe operate `:583`, imperecheri `:584`;
șterge conexele draft `:585`; simulează eliminarea rândurilor de stoc `:596-602`;
refuză dacă loturile create au fost atinse de altcineva `:606-611`; șterge fizic
cele trei registre `:613-615` + registrul propriu `:616-617`; revine la `Draft`,
golește `DataOperare` și `TotalStingere` `:618-620`.

### A.2 Regula vs mecanismul, pe serviciile cerute

| serviciu | REGULA (intrări → ieșiri) | mecanismul din jur |
|---|---|---|
| `ImperechereService` | `ValideazaCreare` (`:189-312`): (stingător, stins, sumă, contrapartidă?, dată) → refuz sau (contrapartidă, sens) ales; plafonul = `CapacitateStingere[cp][sens] − AsignatFataDe(cp, sens)` | `Total`/`Asignat`/`Ramas` (`:30-51`) citesc `Documente.TotalStingere` și `Imperecheri`; `Creeaza`/`CreeazaInvers`/`Desfa`/`Imperecheaza` scriu rânduri + tranzacție; `CreeazaAutomataLaOperare` (`:144-161`); `AsignatFataDe` (`:327-362`) **materializează polimorf documentele stinse** ca să le ceară hook-ul `SensDeStins` |
| `DescarcareService` | alocarea: (resturi per linie FCL, gestiune, dată) → `(LinieId, TipMaterialId, LotId, Cantitate)[]`, cu PIN-urile înaintea FIFO (`:89-112`) | `RestNedescarcat` (`:22-50`, proiecții server-side); citirea regulilor de stoc DSC (`:67-70`); crearea draftului + liniilor (`:125-142`) |
| `StocService` | **pure**: `ValoareGolire(soldInainte, cantitateIesita)` (`:41-42`) și `VerificaGoliri(randuriCheie, pretLot)` (`:166-202`, oracolul din registru) | `SolduriLaData` (`:57-69`), `AplicaValoareIesire` (`:103-125`, SCRIE pe linii), `VerificaSoldIntermediar` (`:216-250`), `Sold` (`:204`), `AlocaFifoTolerant`/`AlocaFifo` (`:259-298`) |
| `LoturiCulegereService` | decizia per linie: (produs cules, natura clasei, `NasteLot`, lot propriu finalizat?) → creează / sincronizează / rupe referința / șterge (`:78-180`) | totul e mecanism XAF: `ModifiedObjects` (`:253`), `GetObjectsToDelete` (`:193`, `:269`), `IsObjectToDelete`, ștergerea orfanilor (`LoturiLiniiSterse`, `:265-312`) |
| `AmortizareService` | **pure**: `CotaLunara(BazaAmortizare)` (`:124-133`) + `Liniara`/`Degresiva` (`:135-161`), `Deductibil(fiscal, categorie, exclusiv, reguli, data)` (`:164-183`), `Situatie(IEnumerable<RandRegistru>, laData)` (`:76-94`), `LuniDeRecuperat` (`:403-415`), `Cifra` (`:423-456`) | `Randuri` (citirea registrului, `:96-111`), `Calcul` (`:332-399`, citește fișe + politici + reguli), `Analizeaza` (`:261-327`, gardienii de cronologie/perioadă), `Incearca` (`:210-245`, creează documentul) |
| `PerioadaService` | `Verifica(os, an, luna)` → listă de `ConstatareInchidere` (`:34-63`); severitatea vine din `PoliticaInchidere` (`:71-121`) — dar CITEȘTE baza, nu e pură | `Blocheaza` (`FOR UPDATE`, `:338-351`), `Inchide`/`Redeschide` (`:246-332`): re-verifică în tranzacție, materializează/elimină snapshot-uri, scrie `InchiderePerioada` |
| `SolduriService` | `Referinte` (`:75-89`: ultima închisă + fiecare decembrie închis) și `Referinta(panaLa)` (`:207-215`) sunt regulile; restul e SQL | `Materializeaza`/`ScrieContabil`/`ScrieStoc`/`MaterializeazaPartide` (SQL brut generat, `:92-140`, `:302-380`), `AtomiCumulati`/`MiscariCumulate` (citirea hibridă snapshot+rulaje, `:226-298`), `Reconstruieste` (`:176-202`) |
| `InchidereTvaService` | `CalculeazaLinii(sold4426, sold4427)` (`:58-64`) și `LiniiPotrivescSoldurile` (`:72-73`) — **pure**; `Analizeaza` (`:169-256`) e ordinea unică a gardienilor | `Solduri` (`:268-280`, prin `SolduriService.AtomiCumulati`), `Incearca` (`:88-141`, creează ITV + 3 linii) |
| `CorectieService` | nicio regulă pură: `ExcluseDocument`/`ExcluseLinie`/`ExcluseLot` (`:308-330`) sunt liste, restul e copiere | `Corecteaza` (`:335-396`): tranzacție, storno, `Copiaza` prin metadata EF (`:420-430`), `RenasteLotul` (`:405-413`), rescrierea perioadei fiscale a rândurilor de storno |

---

## B. Hook-urile documentului

Toate hook-urile virtuale de pe `Document` (`BusinessObjects/Documente/Document.cs`)
și pe `DocumentDetaliu`, plus interfețele consumate de motor. Coloana „OS" =
primește `IObjectSpace`.

| hook | semnătură | OS | cine îl cheamă | override-uri | ce decid (exemple) |
|---|---|---|---|---|---|
| `PregatesteOperare` | `void (IObjectSpace)` `Document.cs:196` | **da** | `MotorOperare.cs:82` | 13 | lanțul de valori al liniei: `NIR` recalculează din prețul lotului sau din prețul cules (`DocumenteGestiune.cs:43-51`); `FacturaIntrare` aplică `TvaService.CalculeazaValori` cu `pastreazaTvaCules` (`FacturaIntrare.cs:61-75`); `ReturClient` semnează negativ și ȘTERGE `TipTvaId` pe liniile de cost (`Retururi.cs:134-181`) |
| `ValideazaOperare` | `void (IObjectSpace, ICollection<string>)` `Document.cs:296` | **da** | `MotorOperare.cs:117` | 17 | `NIR` cere predator `Partener` / primitor `Gestiune`, lot per linie, preț pozitiv pe lotul propriu (`DocumenteGestiune.cs:53-124`); `Dvi` cere tip de TVA `DeImport` cu cotă și facturi legate OPERATE (`Dvi.cs:51-96`); baza însăși verifică `GardContare` (`Document.cs:313-324`) |
| `LiniiCreanta` | `IQueryable<DocumentDetaliu> (IQueryable<DocumentDetaliu>)` `Document.cs:188` | nu | `MotorOperare.cs:397` (scrierea lui `TotalStingere`) | 1 | `ReturClient` păstrează doar liniile de venit (`Retururi.cs:190-191`) |
| `Total` | `decimal { get; }` `[NotMapped]` `Document.cs:181` | nu | UI / DTO-uri; geamănul server-side e `LiniiCreanta` | 1 | `ReturClient` (`Retururi.cs:184-186`) |
| `RepartitorImplicitDebit` | `Guid (IObjectSpace)` `Document.cs:205` | **da** | `MotorOperare.cs:194` | 1 | `DocumentTrezorerie` alege după tipul repartitorului de pe latură (`Trezorerie.cs:214-215`) |
| `RepartitorImplicitCredit` | `Guid (IObjectSpace)` `Document.cs:206` | **da** | `MotorOperare.cs:198` | 4 | `Decont` mută creditul pe titular (`Decont.cs:29`); `DescarcareGestiune` (`DescarcareGestiune.cs:19`); `Dvi` (`Dvi.cs:23`); `DocumentTrezorerie` (`Trezorerie.cs:216`) |
| `GestiuneLoturiCulese` | `Gestiune (IObjectSpace)` `Document.cs:219` | **da** | `LoturiCulegereService.cs:76` | 2 | `ListaDiferenteInventar` ia gestiunea inventariată (`ListaDiferenteInventar.cs:22-23`); `Asamblare` (`Asamblare.cs:53-54`) |
| `CapacitateStingere` | `IReadOnlyDictionary<Guid, PlafonStingere> (IObjectSpace)` `Document.cs:237` | **da** | `ImperechereService.cs:219` | 2 | `NotaContabila` = repartitorii expliciți ai liniilor, per sens (`NotaContabila.cs:57-86`); `DocumentTrezorerie` = o contrapartidă, plafon = totalul (`Trezorerie.cs:171-185`) |
| `SursaStingeriiAutomate` | `Guid? (IObjectSpace)` `Document.cs:242` | **da** | `ImperechereService.cs:145` | 1 | `DocumentTrezorerie` (`Trezorerie.cs:201-202`) |
| `PoateFiStins` | `bool (IObjectSpace)` `Document.cs:254` | **da** | `ImperechereService.cs:152`, `:232` | 5 | `Dvi` false (`Dvi.cs:18`); `PunereInFunctiune`/`IesireImobilizare`/`AmortizareLunara` false (`Imobilizari.cs:22`, `:364`, `:547`); `DocumentTrezorerie` = `!EsteVirament` (`Trezorerie.cs:197`) |
| `SensDeStins` | `SensStingere? (IObjectSpace)` `Document.cs:271` | **da** | `ImperechereService.cs:265`, `:354` | 7 | `FacturaIntrare`/`NIR`/`Decont`/`ReturClient` ⇒ `Datorie`; `FacturaIesire`/`ReturFurnizor` ⇒ `Creanta`; `DocumentTrezorerie` ⇒ `SensPropriu()` (`Trezorerie.cs:187`) |
| `GenereazaSecundar` | `Document (IObjectSpace)` `Document.cs:279` | **da** | `MotorOperare.cs:414` | 3 | `FacturaIntrare` naște `Plata` din grupul DECONT_* (`FacturaIntrare.cs:76-103`); `FacturaIesire` cheamă `DescarcareService.Genereaza` (`FacturaIesire.cs:51-57`); `DocumentTrezorerie` naște latura pereche a viramentului (`Trezorerie.cs:235-286`) |
| `MesajeDupaOperare` | `IReadOnlyList<string> (IObjectSpace)` `Document.cs:290` | **da** | `Api/OperareApi.cs:58` (DUPĂ commit) | 2 | `DocumentTrezorerie` (`Trezorerie.cs:444`); `IesireImobilizare` (`Imobilizari.cs:483`) |
| `DocumentDetaliu.DimensiuniCulese` | `Dimensiuni ()` `Document.cs:384` | nu | `MotorOperare.cs:190`, `:274`; `DescarcareService.cs:140`; `GenereazaConex` | 11 | fiecare frunză care culege dimensiuni: `FacturaIntrareDetaliu` 4 componente (`FacturaIntrare.cs:220-223`), `DocumentTrezorerieDetaliu` 4 (`Trezorerie.cs:631-634`), restul doar `CodEconomicId` |
| `DocumentDetaliu.PreiaDimensiuni` | `void (Dimensiuni)` `Document.cs:388` | nu | `MotorOperare.cs:570`; `DescarcareService.cs:140`; `FacturaIntrare.cs:360` | 11 | perechea de scriere a celor de mai sus |
| `DocumentDetaliu.ProdusCules` | `Guid? ()` `Document.cs:400` | nu | `TvaService.cs:167` | 5 | `FacturaIntrareDetaliu`, `NirDetaliu`, `FacturaIesireDetaliu`, `ListaDiferenteInventarDetaliu`, `AsamblareDetaliu` |

Metodele NEvirtuale consumate de motor: `DocumentDetaliu.CreeazaLot(os, produs, gestiune)`
(`Document.cs:406-413`, chemată din `LoturiCulegereService.cs:163`).

### B.1 Interfețele consumate de motor

| interfață | declarată la | consumată la | implementatori |
|---|---|---|---|
| `IVerificabilLaCommit` | `Comun/Interfete.cs:125-127` | `GardianEditare.cs:206-207` (dispecerul, fără gard de ștergere) | `DviFactura` (`Dvi.cs:101`, `:111`), `PoliticaAmortizare` (`Politici/Politici.cs:165`) |
| `IDocumentCuRegistruPropriu` | `Comun/Interfete.cs:130-137` (3 metode, toate cu OS) | `MotorOperare.cs:400-401` (materializare), `:616-617` (anulare), `:700-701` (storno) | `PunereInFunctiune` (`Imobilizari.cs:14`, corpuri la `:187`, `:218`, `:227`), `IesireImobilizare` (`:359`, `:426`, `:450`, `:457`), `AmortizareLunara` (`:546`, `:616`, `:632`, `:638`) |
| `IDocumentCuScadenta` | `Comun/Interfete.cs:6-8` | `MotorOperare.cs:808` (`AplicaScadenta`) | `FacturaIntrare`, `FacturaIesire` |
| `IDocumentCuIesireFiscala` | `Comun/Interfete.cs:118` (marker) | `StocService.cs:105` (sare regula D18-D2) | `ReturFurnizor` (`Retururi.cs:29`) |
| `IDocumentCuPostareExplicita` | `Comun/Interfete.cs:84` (marker) | `MotorOperare.cs:167` (postarea fără regulă) | `NotaContabila` (+ `InchidereTva`), `IesireImobilizare`, `AmortizareLunara` |
| `ILinieCuPostareExplicita` | `Comun/Interfete.cs:71-76` | `MotorOperare.cs:157`, `:172`, `:177`, `:192`, `:196` | `DecontDetaliu`, `NotaContabilaDetaliu`, `IesireImobilizareDetaliu`, `AmortizareLunaraDetaliu` |
| `ILinieCareNasteLot` | `Comun/Interfete.cs:55-64` (cu `NasteLot => true`) | `LoturiCulegereService.cs:58`, `:108` | `FacturaIntrareDetaliu`, `NirDetaliu`, `ListaDiferenteInventarDetaliu`, `AsamblareDetaliu` |
| `ILinieCuAtributeLot` | `Comun/Interfete.cs:18-21` | `MotorOperare.cs:346-349` (finalizarea lotului) | `FacturaIntrareDetaliu`, `NirDetaliu`, `ListaDiferenteInventarDetaliu`, `AsamblareDetaliu` |
| `ILinieCuPretUnitar` | `Comun/Interfete.cs:37-39` | UI/Apply (culegere), nu motorul | `FacturaIntrareDetaliu`, `FacturaIesireDetaliu`, `DecontDetaliu` |
| `GardContareAttribute` | `Comun/Interfete.cs:93-106` (contract pe CLASĂ) | `MotorOperare.cs:787` → `Document.cs:313-324` | `FacturaIesire`, `DescarcareGestiune`, `ReturClient`, `Plata`, `Incasare` |
| `ICuProvenienta` | `Comun/ICuProvenienta.cs:162-164` | `GardianEditare.cs:193-202` | 20 tipuri (`Politici/TipuriConfigurabile.cs:10-17`) |
| `ICuCautare` | `Comun/Cautare.cs` | `GardianEditare.cs:189-190` | nomenclatoarele căutabile |

**Hook-urile care cer instanța, nu doar date**: toate cele 13 de pe `Document`
primesc `IObjectSpace` și îl folosesc pentru `GetObjectByKey` pe repartitori,
loturi și politici; `CapacitateStingere`, `SensDeStins` și `PoateFiStins` sunt
chemate de `ImperechereService.AsignatFataDe` pe documente MATERIALIZATE polimorf
(`ImperechereService.cs:340-343`) — singurul loc din motor care încarcă documente
întregi ca să le întrebe tipul.

---

## C. Documentele tipate (20 de frunze concrete)

Ancorele `TipDocument` (cod → clasă CLR) sunt în `DatabaseUpdate/ContaSeeder.cs:278-316`;
discriminatorul TPH `ClrType` e mapat în `BusinessObjects/BackOfficeDbContext.cs:212`
(`Document`), `:216` (`DocumentDetaliu`), `:220` (`Repartitor`).

| tip (ClrType) | fișier:linie | câmpuri proprii de culegere (header) | tipul de detaliu + câmpurile lui proprii | ce postează azi | conex generat | hook-uri suprascrise |
|---|---|---|---|---|---|---|
| **FCT** `FacturaIntrare` | `Documente/FacturaIntrare.cs:279` | `DataScadenta`, `NumarPV`, `DataPV`, `CodCpv`, `TethysId`, `Valuta`, `Curs`, `GenereazaPlata`, `PlataContPropriuId`, `PlataNumar`, `PlataData`, `PlataTipInstrument`, `GenereazaChitanta`, `ChitantaNumar`, `ChitantaData` | `FacturaIntrareDetaliu` (`:425`): `PretUnitar`, `CodCpv`, `ProdusId`, `DataExpirare`, `LotFabricatie`, `CodEconomicId`, `SursaFinantareId`, `CodFunctionalId`, `ProiectId` | **contabil** (liniile NON-stoc + rândurile 4426), **TVA** (registru), **terți** (401) — **nu stoc** | **NIR** (`PoliticaConex`, filtru natură = Stoc) | `SensDeStins`, `PregatesteOperare`, `GenereazaSecundar`, `ValideazaOperare` |
| **NIR** `NIR` | `Documente/DocumenteGestiune.cs:17` | — | `NirDetaliu` (`:137`): `ProdusId`, `PretUnitar`, `DataExpirare`, `LotFabricatie`, `CodEconomicId`, `SursaFinantareId`, `CodFunctionalId`, `ProiectId` | **stoc** (+1 primitor), **contabil** (3xx = 401) | — | `SensDeStins`, `PregatesteOperare`, `ValideazaOperare` |
| **BCS** `BonConsum` | `Documente/DocumenteGestiune.cs:201` | — | `DocumentDetaliu` (baza) | **stoc** (−Magazie predator, +Consum primitor), **contabil** (6xx = 3xx) | — | `PregatesteOperare`, `ValideazaOperare` |
| **BTR** `NotaTransfer` | `Documente/DocumenteGestiune.cs:229` | `NumarPV`, `DataPV` | `DocumentDetaliu` (baza) | **stoc** (−predator / +primitor); contabil: fără regulă (23c) | — | `PregatesteOperare`, `ValideazaOperare` |
| **BPR** `RaportProductie` | `Documente/RaportProductie.cs:6` | — (clasă goală, rezervată — 19) | `DocumentDetaliu` | nimic | — | niciunul |
| **LDI** `ListaDiferenteInventar` | `Documente/ListaDiferenteInventar.cs:17` | — | `ListaDiferenteInventarDetaliu` (`:108`): `Directie`, `ProdusId`, `PretEvaluare`, `DataExpirare`, `LotFabricatie`, `CodEconomicId` | **stoc** (± după `Directie`), **contabil** | — | `GestiuneLoturiCulese`, `PregatesteOperare`, `ValideazaOperare` |
| **DEC** `Decont` | `Documente/Decont.cs:14` | `NumarPV`, `DataPV` | `DecontDetaliu` (`:74`): `Descriere`, `PretUnitar`, `ContDebitId`, `ContCreditId`, `RepartitorDebitId`, `RepartitorCreditId`, `CodEconomicId` | **contabil** (postare explicită pe linie), **TVA** (4426 = 542), **terți** (542) | — | `SensDeStins`, `RepartitorImplicitCredit`, `PregatesteOperare`, `ValideazaOperare` |
| **PLT** `Plata` | `Documente/Trezorerie.cs:561` | moștenite din `DocumentTrezorerie` (`:15`): `TipInstrument`, `NumarExtras`, `DataExtras`, `LaturaPerecheId` | `DocumentTrezorerieDetaliu` (`:614`): `CodEconomicId`, `SursaFinantareId`, `CodFunctionalId`, `ProiectId` | **contabil**, **terți** (stinge datorii) | latura pereche `Incasare` la virament (secundar) | `GetContrapartidaId`, `SensPropriu`, `CreeazaPereche`, `TipLaturaPereche`, `ValideazaOperare` (+ cele de pe bază) |
| **INC** `Incasare` | `Documente/Trezorerie.cs:588` | idem | `DocumentTrezorerieDetaliu` | **contabil**, **terți** (stinge creanțe) | latura pereche `Plata` | idem |
| **DSC** `DescarcareGestiune` | `Documente/DescarcareGestiune.cs:15` | — | `DescarcareGestiuneDetaliu` (`:113`): `LinieSursaId`, `CodEconomicId` | **stoc** (−1 predator = gestiunea), **contabil** (6xx = 3xx) | — | `RepartitorImplicitCredit`, `PregatesteOperare`, `ValideazaOperare` |
| **FCL** `FacturaIesire` | `Documente/FacturaIesire.cs:17` | `DataScadenta`, `GestiuneDescarcareId` | `FacturaIesireDetaliu` (`:148`): `Descriere`, `PretUnitar`, `ProdusId`, `CodEconomicId` | **contabil** (venit), **TVA** (4427), **terți** (4111) — **nu stoc** | **DSC** ca SECUNDAR (`DescarcareService`), nu prin `PoliticaConex` | `SensDeStins`, `PregatesteOperare`, `GenereazaSecundar`, `ValideazaOperare` |
| **NTC** `NotaContabila` | `Documente/NotaContabila.cs:22` | — | `NotaContabilaDetaliu` (`:114`): `Descriere`, `ContDebitId`, `ContCreditId`, `RepartitorDebitId`, `RepartitorCreditId`, `CodEconomicId` | **contabil** (postare explicită completă, fără `RegulaContare`) | — | `CapacitateStingere`, `ValideazaOperare` |
| **ITV** `InchidereTva` | `Documente/InchidereTva.cs:26` (derivă din `NotaContabila`) | — | `NotaContabilaDetaliu` | **contabil** (4426/4427/4423/4424) | — | `ValideazaOperare` (anti-stale pe solduri) |
| **ASM** `Asamblare` | `Documente/Asamblare.cs:43` | — | `AsamblareDetaliu` (`:176`): `Directie`, `ProdusId`, `PretEvaluare`, `DataExpirare`, `LotFabricatie` | **stoc** (consum − / produs +), **contabil** | — | `GestiuneLoturiCulese`, `PregatesteOperare`, `ValideazaOperare` |
| **RLF** `ReturFurnizor` | `Documente/Retururi.cs:29` | — (detaliul de BAZĂ) | `DocumentDetaliu` | **stoc** (−q pe lotul original), **contabil** (3xx = 401 cu −V), **TVA** (4426 = 401 cu −TVA) | — | `SensDeStins`, `PregatesteOperare`, `ValideazaOperare`; declară `IDocumentCuIesireFiscala` |
| **RDC** `ReturClient` | `Documente/Retururi.cs:124` | — (detaliul de BAZĂ) | `DocumentDetaliu` | **stoc** (+ marfa revine), **contabil** (venit −V + cost), **TVA** (4111 = 4427 cu −TVA) | — | `SensDeStins`, `PregatesteOperare`, `Total`, `LiniiCreanta`, `ValideazaOperare` |
| **DVI** `Dvi` | `Documente/Dvi.cs:15` | colecția `Facturi` (`ObservableCollection<DviFactura>`, `:27`), calculate `Baza` (`:35`) și `Taxa` (`:40`) | `DocumentDetaliu` (baza, `[TipDetaliu(typeof(DocumentDetaliu))]` `:13`) | **contabil** (4426 = 446/401), **TVA** (registru, tipuri `DeImport`) | — | `PoateFiStins` (false), `RepartitorImplicitCredit`, `PregatesteOperare`, `ValideazaOperare` |
| **PIF** `PunereInFunctiune` | `Documente/Imobilizari.cs:14` | — | `PunereInFunctiuneDetaliu` (`:314`): `ImobilizareId`, `Fel`, `LinieSursaId`, `ValoareFiscala`, `AmortizareInitiala`, `AmortizareFiscalaInitiala`, `LuniAmortizateInitial`, `Metoda`, `DurataLuni`, `ValoareReziduala`, `MetodaFiscala`, `DurataFiscalaLuni`, `CategorieFiscala`, `UtilizareExclusiva` | **contabil** + **imobilizări** (`RegistruImobilizari`, `Fel = Intrare/Modernizare/Revizuire/Reevaluare`) | — | `PoateFiStins` (false), `PregatesteOperare`, `ValideazaOperare`; `IDocumentCuRegistruPropriu` |
| **CAS** `IesireImobilizare` | `Documente/Imobilizari.cs:359` | `Cauza` (`CauzaIesire`) | `IesireImobilizareDetaliu` (`:507`): `ImobilizareId`, `Fel`, `ContDebitId`, `ContCreditId`, `RepartitorDebitId`, `RepartitorCreditId`, `CodEconomicId` | **contabil** (postare explicită: cumulat + valoare rămasă) + **imobilizări** (`Fel = Iesire`, cifre NEGATE din situație) | — | `PoateFiStins`, `ValideazaOperare`, `MesajeDupaOperare`; `IDocumentCuPostareExplicita`, `IDocumentCuRegistruPropriu` |
| **AMO** `AmortizareLunara` | `Documente/Imobilizari.cs:546` | — | `AmortizareLunaraDetaliu` (`:665`): `ImobilizareId`, `ValoareFiscala`, `ValoareDeductibila`, `Luni`, `ContDebitId`, `ContCreditId`, `RepartitorDebitId`, `RepartitorCreditId`, `CentruCostId`, `CodEconomicId` | **contabil** (6811 = 281x, postare explicită) + **imobilizări** (`Fel = Amortizare`, cele trei cifre) | — | `PoateFiStins`, `ValideazaOperare`; `IDocumentCuPostareExplicita`, `IDocumentCuRegistruPropriu` |

### C.1 Cazurile cerute explicit

**FCT ↔ NIR conex.** Linia FCT postează NETUL doar pe liniile FĂRĂ regulă de stoc
(servicii); pe liniile de stoc `RegulaContare` lipsește deliberat, deci motorul
le sare (`MotorOperare.cs:167-169`) — netul lor ajunge pe NIR. TVA-ul se postează
însă pe FACTURĂ, pentru TOATE liniile, fiindcă pasul TVA e independent de
potrivirea regulii principale (`MotorOperare.cs:213-282`). Conexul se generează
în `MotorOperare.GenereazaConex` (`:534-573`) după `PoliticaConex`
(`Fapte.Conex`, `Motor/Fapte.cs:26-34`): clona copiază `LotId` (lotul e născut de
linia FACTURII, deci pe NIR e „lot străin" — `LoturiCulegereService.cs:84-96`) și
`TipTvaId` ca informație, dar **nu** `ValoareTva` (`MotorOperare.cs:565-568`).
`NIR.PregatesteOperare` (`DocumenteGestiune.cs:43-51`) recalculează valoarea din
`lot.PretUnitar` exact pentru că lotul e străin.

**DVI.** Agregat cu legătură n→m spre facturi prin `DviFactura`
(`Dvi.cs:101-...`, `IVerificabilLaCommit` cu `Verifica` la `:111`). Baza e CULEASĂ
(valoarea în vamă, nu preț × cantitate) — `PregatesteOperare` cheamă
`CalculeazaValori(d, d.Valoare, …, pastreazaTvaCules: true)` (`Dvi.cs:44-49`).
Legătura e EVIDENȚĂ, nu sursa cifrelor; operarea refuză dacă o factură legată nu
e `Operat` (`Dvi.cs:84-96`). `PoateFiStins` = false.

**Trezoreria.** Bază abstractă `DocumentTrezorerie` (`Trezorerie.cs:15`) cu două
frunze concrete, `Plata` (`:561`) și `Incasare` (`:588`). Stingerea: rolul de
stingător prin `CapacitateStingere` (o contrapartidă, plafon = totalul, `:171`),
sensul prin `SensPropriu()` (`Plata` ⇒ `Creanta`, `Incasare` ⇒ `Datorie`, `:567`,
`:593`), stingerea automată prin `SursaStingeriiAutomate` (`:201`). Viramentul
intern: `LaturaPerecheId` (`:29`) + `CreeazaPereche`/`TipLaturaPereche`
(`:227`, `:569`, `:595`); un virament nu poate fi stins (`PoateFiStins`, `:197`).
Două documente de același sens nu se imperechează (`ImperechereService.cs:209-210`
— singurul `is` pe frunze de DOCUMENT din motor; celelalte referințe la frunze din
`Motor/` sunt pe ierarhia `Repartitor` — `is not UnitateInterna` la
`AmortizareService.cs:213` și `InchidereTvaService.cs:95` — și două interogări
tipate pe frunze de document în `PerioadaService.cs:204` și `:207`
(`FacturaIntrare`/`FacturaIesire`, constatarea `RestScadent`)).

**AMO / PIF / CAS** scriu al PATRULEA registru, `RegistruImobilizari`, prin
`IDocumentCuRegistruPropriu` — motorul nu le cunoaște, doar dispecerizează:
- PIF `MaterializeazaRegistrul` (`Imobilizari.cs:187-215`): un rând per linie, cu
  valorile inițiale ȘI parametrii (metodă, durată, reziduală, categorie fiscală);
  mută fișa în `InFunctiune`;
- CAS (`:426-448`): un rând per FIȘĂ (grupat), cu cifrele NEGATE din
  `AmortizareService.Situatie`; mută fișa în `Iesita`;
- AMO (`:616-630`): un rând per linie, cu cele trei cifre (contabil / fiscal /
  deductibil) și `Luni`;
- toate trei refuză anularea/stornarea dacă există fapte ulterioare pe fișă
  (`VerificaFaraFapteUlterioare`) și stornoul în afara lunii documentului
  (`VerificaLunaStornarii`).

---

## D. Politicile (date)

Toate în `BusinessObjects/Politici/Politici.cs` dacă nu se spune altfel. Cele 20
de tipuri „ale configurației" sunt enumerate explicit în
`BusinessObjects/Politici/TipuriConfigurabile.cs:10-17` (toate `ICuProvenienta`).

| tabelă de politică | fișier:linie | cheia | ce parametrizează | cine o consumă | versionată pe perioadă? | rânduri pe seed privat |
|---|---|---|---|---|---|---|
| `TipDocument` (ancoră) | `:21` | `Cod` / `ClrType` | legătura clasă CLR ↔ cod; `TipTvaImplicit` | `MotorOperare.cs:767-772`, toate politicile prin FK | nu | **20** (log ModelCheck privat) |
| `RegulaStoc` | `:52` | `TipDocument × Latura × Clasa?` | `TipStoc` + `Semn` per latură | `Fapte.cs:18` → `Potrivire.cs:160` → `MotorOperare.cs:114`, `:125`; `DescarcareService.cs:68` | nu | **19** |
| `RegulaContare` | `:74` | `TipDocument × TipMaterial? × NaturaFiltru? × SemnFiltru?` | conturile D/C (sursă declarativă + fallback), `PastreazaSemn`, 3×8 dimensiuni (`Comun`/`OverrideDebit`/`OverrideCredit`) | `Fapte.cs:11` → `Potrivire.cs:79` → `MotorOperare.cs:137-211`; `Document.cs:317` (gardul de contare) | nu | **58** |
| `PoliticaConex` | `:241` | `TipDocumentSursa` | ținta, `InverseazaLaturi`, `NaturaFiltru` | `Fapte.cs:26` → `MotorOperare.cs:407` | nu | 1 (FCT → NIR, `ProfilPrivat.cs:976`) |
| `PoliticaScadenta` | `:262` | `TipDocument` | `ZileDefault` | `MotorOperare.cs:810` | nu | 1 (FCL, `ProfilPrivat.cs:1041`) |
| `PoliticaValidare` | `:279` | `TipDocument` | `CereClasificatieBugetara`, `NaturaInterzisa` | `MotorOperare.cs:438` | nu | **0** — declarat explicit: privatul n-are niciun rând (`ProfilPrivat.cs:1078-1092`) |
| `PoliticaTva` | `:302` | `TipDocument` | `Directie`, `SursaContrapartida` + fallback, `DeclarareIntarziata` | `MotorOperare.cs:220`; `RegistruTvaService.cs:272`; `TvaService.cs:116` | nu | **7** (FCT, DEC, FCL, RLF, RDC, DVI — `ProfilPrivat.cs:460-475`; ITV n-are) |
| `PoliticaInchidereTva` | `:330` | `TipDocument` (= ITV) | 4426/4427/4423/4424 | `InchidereTvaService.cs:181` | nu | 1 (`ProfilPrivat.cs:1158`) |
| `PoliticaNumerotare` | `:358` | `TipDocument` | `Serie`, `UrmatorulNumar`, `Format` | `MotorOperare.cs:793`; `GardianEditare.cs:509` | nu | **15** apeluri `SeedNumerotare` (`ProfilPrivat.cs`) |
| `MapareD300` | `:389` | `TipTva × Sens` (N rânduri per pereche) | rândul din decontul D300 | `Proiectii/D300Proiectii.cs`; `GardianEditare.cs:1194` | nu | **22** (tabelul `MapariD300`, `ProfilPrivat.cs`) |
| `MapareD394` | `:525` | `TipTva × Sens` (unic) | `TipOperatiuneD394` | `Proiectii/D394Proiectii.cs`; `GardianEditare.cs:1208` | nu | **13** (tabelul `MapariD394`) |
| `PoliticaMiscareSaft` | `:602` | `TipDocument × TipStoc × Semn?` | `CodMiscare` (D406), `RolTert`, `Motiv` la excludere | `Saft/SaftProiectii.cs`; `GardianEditare.cs:800` | nu | **21** (tabelul `PoliticiMiscareSaft`) |
| `PoliticaTvaImplicit` | `:674` | `TipDocument × ClasaFiscala? × ValabilDeLa?` | `TipTva` implicit la culegere | `Fapte.cs:39` → `Potrivire.cs:209` → `ImpliciteService.cs:73` | **DA** (`ValabilDeLa`) | **11** (tabelul `ImpliciteTva`) |
| `PoliticaAmortizare` | `:709` | `TipMaterial` | `ContAmortizare`, `ContCheltuialaAmortizare`, `ContCheltuialaCedare` | `AmortizareService.cs:355`, `:189` | nu | **7** perechi (`ProfilPrivat.cs:1183-1188`) |
| `RegulaDeductibilitate` | `:748` | `Categorie × DeLa` (+ `DoarNeexclusiv`, `Fel`) | plafon lunar / procent de deductibilitate fiscală | `AmortizareService.cs:362` → `Deductibil` `:164` | **DA** (`DeLa`/`PanaLa`) | **3** (`ProfilPrivat.cs:1191-1205`) |
| `PoliticaInchidere` | `:775` | `Fel` (`FelConstatareInchidere`) | severitatea constatării (`Blocant`/`Avertisment`/`Ignorat`) | `PerioadaService.cs:72` | nu | **4** (`ProfilPrivat.cs:1170-1175`) |
| `SetareProfil` | `Politici/SetareProfil.cs:19` | rând unic | `Profil`, `RotunjireBani` (înghețată per bază) | `ContaSeeder.AplicaConventiaRotunjire`; `Comun/Scara.cs` | nu (înghețată) | 1 |

**Ce NU e tabelă proprie**: rotunjirea (o coloană pe `SetareProfil` + clasa de cod
`Comun/Scara.cs`); dimensiunile obligatorii per cont (coloana
`Cont.DimensiuniObligatorii`, consumată la `MotorOperare.cs:491`); conturile
implicite ale tipurilor și repartitorilor (`TipMaterial.ContImplicitId`,
`Repartitor.ContImplicitId` — citite prin `Fapte.ClaseTip`/`Fapte.Laturi`);
catalogul HG 2139/2004 (`Nomenclatoare/Imobilizari.cs`, nomenclator, nu politică);
rândurile D300 (`Nomenclatoare/RandD300.cs`, nomenclator al formularului).

**Versionate pe perioadă**: doar `PoliticaTvaImplicit` (`ValabilDeLa`) și
`RegulaDeductibilitate` (`DeLa`/`PanaLa`). **Neversionate**: toate celelalte 15 —
o schimbare de politică se aplică de la următoarea operare, iar registrele scrise
rămân neatinse.

**Ce n-am putut număra static**: rândurile de `RegulaContare` și `RegulaStoc` per
tip de document se generează în bucle peste liste locale ale fiecărui
`SeedPolitici*` din `ProfilPrivat.cs` (plus helperele `SeedContare6xxDin3xx`,
`SeedContareVanzare`, `SeedContareVirament` din `ContaSeeder.cs:895-1020`), deci
numărul lor nu se citește dintr-un tabel; cifrele din tabel (19 și 58) vin din
antetul ultimei rulări ModelCheck pe profilul privat
(`run-f28/ui-tip/privat.log`), nu din cod. La fel `Conturi: 644`, `TipuriTva: 15`,
`TipuriMaterial: 62`, `ClaseProduse: 14`, `Repartitori: 7`, `PerioadeFiscale: 12`.

---

## E. Gardienii

### E.1 `GardianEditare` (`Motor/GardianEditare.cs`, 1.312 linii)

Seam: `IObjectSpaceCustomizer` → `Committing` pe ObjectSpace-urile **SECURED**
(`:78-96`). Ordinea: dreptul, apoi domeniul.

| literă | regulă | ce citește | fișier:linie (dispecer / corp) |
|---|---|---|---|
| **pasul 0** | `Create`/`Write`/`Delete` prin `IRequestSecurityStrategy`, pe fiecare obiect modificat | tracker (`ModifiedObjects`) + strategia de securitate | `:129-166` |
| **(a)** | documentul se culege cât e `Draft`; `Stare`, `DataOperare`, `Autogenerat`, `DocumentSursa`, `TotalStingere`, `Numar` (când tipul are numerotare), legătura de corecție sunt ale motorului | tracker (`OriginalValues`) + bază (`PoliticaNumerotare`) | `:253-254` / `:387-433`; liniile `:256-257` / `:480-501` |
| **(b)** | registrele și snapshot-urile (`RegistruStoc/Contabil/Tva/Imobilizari`, `SoldPerioada*`, `PartidaDeschisa`) se scriu doar de motor | tipul obiectului | `:215-237` |
| **(c)** | imperecherea: `New` validat prin `ImperechereService.ValideazaCreare`, `Edit` refuzat, `Delete` liber doar în perioadă deschisă și fără rând invers | tracker + bază | `:259-260` / `:520-575` |
| **(d)** | `Cont.Parinte` nu poate închide un ciclu (limită 64 niveluri) | bază (`GetObjectByKey<Cont>`) | `:264-265` / `:608-639` |
| **(e)** | partenerul: `Tara` ISO-2, județ doar pe RO, `DataSincronizareAnaf` și `InactivFiscal` server-owned | tracker (`OriginalValues`) | `:270-271` / `:641-695` |
| **(f)** | societatea: un singur rând, județ doar pe RO, `BazaContabila` din lista ANAF | bază (`Count()`) + `ModifiedObjects` | `:275-276` / `:700-749` |
| **(g)** | `Produs.CodNc` are exact 8 cifre sau e gol | obiectul | `:280-281` / `:783-788` |
| **(h)** | `PoliticaMiscareSaft`: cod din nomenclatorul D406, motiv obligatoriu la excludere, semn ∈ {−1, null, +1} | obiectul + `SaftReguli` | `:285-286` / `:800-834` |
| **(i)** | orice `ICuCautare` are `Cod`/`Simbol` și `Denumire` nevide | obiectul (reflecție prin `Cautare`) | `:189-190` / `:759-773` |
| **(j)** | proveniența: `DinSeed = true` refuzat pe rând NOU, stins idempotent la orice editare | tracker (`IsNewObject`) | `:193-194` / `:851-863` |
| **(k)** | orice proprietate de enum (non-`[Flags]`) are o valoare definită | obiectul (reflecție, cache `:865-881`) | `:201` / `:883-894` |
| **(l)** | `IVerificabilLaCommit.Verifica` — invarianții pe care entitatea îi poartă singură (fără gard de ștergere) | entitatea | `:206-207` |
| **(m)** | istoricul închiderilor (`InchiderePerioada`) se scrie doar de motor | tipul | `:241-247` |
| **(n)** | perioada fiscală: `An`/`Luna` imuabile, starea e a motorului, ștergerea refuzată dacă e închisă sau are istoric | tracker + bază | `:250-251` / `:339-371` |
| **(o)** | FK-ul spre un tip NE-rădăcină al unei ierarhii TPH are ținta de tipul cerut; cele 9 FK-uri se descoperă generic din metadata EF | tracker întâi (`RandDupaCheie.Oricare`), apoi baza | `:209-210` / `:915-934`, `FkSpreFrunze` `:900-913` |

Plus invarianții politicilor deschise pe OData (F23-D5), fără literă, dispecerizați
în `switch`-ul `:292-327`: `TipDocument` (`:944`), `TipTva` (`:974`),
`PoliticaTvaImplicit` (`:1046`), `PoliticaTva` (`:1072`), `RegulaContare` (`:1080`),
`RegulaStoc` (`:1108`), `PoliticaNumerotare` (`:1130`), `PoliticaScadenta` (`:1152`),
`PoliticaInchidereTva` (`:1162`), `PoliticaInchidere` (`:1179`), `MapareD300`
(`:1194`), `MapareD394` (`:1208`). Ajutoare transversale: `EsteSters` (`:1266`),
`Originale` (`:1279`), `PerioadaDeschisa` (`:586`).

### E.2 Ceilalți trei

- **`GardianPerioada`** (`Motor/GardianPerioada.cs:10`, 34 linii): `VerificaDeschisa(os, data)`
  — `SELECT "Inchisa" … FOR SHARE` pe rândul lunii; perioada NEDEFINITĂ e tratată
  ca închisă. Șase apelanți: `MotorOperare.cs:70`, `:581`, `:633`;
  `ImperechereService.cs:67`, `:80`, `:133`; plus `InchidereTvaService.cs:244`,
  `AmortizareService.cs:317`, `GardianEditare.cs:588`.
- **`VerificareProfilService`** (`Motor/VerificareProfilService.cs:38`, 277 linii):
  raportul „ce s-a abătut de la profilul livrat" — rânduri manuale, referințe
  șterse, tipuri inactive referite, ancore lipsă, goluri de mapare
  (`ContaSeeder.GoluriMapari`). Nu refuză și nu repară nimic.
- **`VerificaTipDocument`** (`Motor/GardianEditare.cs:944-967`): ancora oglindește
  clasele 1:1 — creare și ștergere refuzate, `Cod` și `ClrType` imuabile; rămân
  editabile `Denumire` și `TipTvaImplicit`.

---

## F. Registrele și proiecțiile

### F.1 Coloanele

**`RegistruStoc`** (`BusinessObjects/Registre/Registre.cs:21`): `Data` `DateOnly`,
`TipStoc` enum, `LotId` `Guid` + nav, `RepartitorId` `Guid` + nav, `Cantitate`
`decimal` (semnată), `Valoare` `decimal` (semnată), `Storno` `bool`, `DocumentId`
`Guid?` (null = deschidere de migrare), `DetaliuId` `Guid?`.

**`RegistruContabil`** (`:43`): `Data` `DateOnly`, `NumarNota` `string`,
`ContDebitId`/`ContCreditId` `Guid` + nav, `Valoare` `decimal`, **2 × 8 dimensiuni
plate** (`Debit*`/`Credit*`: `RepartitorId`, `MaterialId`, `CodFunctionalId`,
`CodEconomicId`, `SursaFinantareId`, `UnitateId`, `ProiectId`, `CentruCostId`,
toate `Guid?`, mapate pe coloanele `DimensiuniDebit_*`/`DimensiuniCredit_*`,
`:61-125`), `Storno` `bool`, `DocumentId` `Guid?`, `DetaliuId` `Guid?`. Puntea spre
value object: `DimensiuniDebit()`/`DimensiuniCredit()`/`AplicaDimensiuni*`
(`:130-153`).

**`RegistruTva`** (`:203`): `Data` `DateOnly` (data faptului), `PerioadaAn` `int`,
`PerioadaLuna` `int`, `ScrisLa` `DateTime`, `Sens` `SensTva`, `DocumentId` `Guid`
(NENUL), `DetaliuId` `Guid` (NENUL), `PartenerId` `Guid?` → `Repartitor`,
`TipTvaId` `Guid`, `Regim` `RegimTva` (snapshot), `Cota` `decimal` (snapshot),
`Baza` `decimal`, `Tva` `decimal`, `Storno` `bool`.

**`RegistruImobilizari`** (`Registre/RegistruImobilizari.cs:370`): `Data` `DateOnly`,
`ImobilizareId` `Guid` + nav, `Fel` `FelMiscareImobilizare`, `Valoare`,
`ValoareFiscala`, `Amortizare`, `AmortizareFiscala`, `AmortizareDeductibila`
(toate `decimal`), `Luni` `int`, parametrii nullable doar pe evenimente (`Metoda`,
`DurataLuni`, `ValoareReziduala`, `MetodaFiscala`, `DurataFiscalaLuni`,
`CategorieFiscala`, `UtilizareExclusiva`), `RepartitorId` `Guid` (locul la data
faptului), `DocumentId` `Guid` (NENUL), `DetaliuId` `Guid` (NENUL), `Storno` `bool`.

**`SoldPerioadaContabil`** (`Registre/SolduriPerioada.cs:269`): `An`, `Luna` `int`;
`ContId` `Guid`; cele 8 dimensiuni `Guid?`; `Debit`, `Credit` `decimal` (cumulate,
separat pe laturi). **`SoldPerioadaStoc`** (`:318`): `An`, `Luna`, `LotId`,
`RepartitorId`, `TipStoc`, `Cantitate`, `Valoare`. **`PartidaDeschisa`** (`:344`):
`An`, `Luna`, `DocumentId`, `Rest` `decimal`. Cheile integral zero se omit din
toate trei.

**`Imperechere`** (`Documente/Trezorerie.cs:654`): `DocumentStingatorId` `Guid` +
nav (tip `Document`, nu `DocumentTrezorerie`), `DocumentId` `Guid` + nav, `Suma`
`decimal` (algebrică), `Data` `DateOnly`, `InverseazaId` `Guid?` + nav,
`Autogenerat` `bool`. Nu e document (fără ciclu Draft/Operat).

### F.2 Cine SCRIE

| registru | scriitori |
|---|---|
| `RegistruStoc` | `MotorOperare.cs:353` (operare), `:649` (storno invers), `:613` (ștergere la anulare); `tools/Import1C/Deschidere.cs:497` și `tools/Migrare/Program.cs:421` (deschidere, `DocumentId = null`) |
| `RegistruContabil` | `MotorOperare.cs:365`, `:661`, `:614`; `tools/Import1C/Deschidere.cs:104`; `tools/Migrare/Program.cs:372`, `:382` |
| `RegistruTva` | `MotorOperare.cs:378`, `:682`, `:615`; `tools/BackfillTva/Backfill.cs:252`; `CorectieService.cs:382-386` (rescrie DOAR `PerioadaAn/Luna` pe rândurile de storno) |
| `RegistruImobilizari` | exclusiv prin `IDocumentCuRegistruPropriu`: `Imobilizari.cs:187` (PIF), `:426` (CAS), `:616` (AMO), plus inversele din `Inverseaza` și ștergerile din `EliminaRegistrul` |
| `SoldPerioadaContabil` / `SoldPerioadaStoc` | `SolduriService.ScrieContabil` (`:302`) / `ScrieStoc` (`:321`), SQL brut; șterse de `Elimina` (`:151`) |
| `PartidaDeschisa` | `SolduriService.MaterializeazaPartide` (`:107`), SQL brut |
| `Imperecheri` | `ImperechereService.Creeaza` (`:164`), `CreeazaInvers` (`:89`) |

### F.3 Cine CITEȘTE (numărul de fișiere care referă tipul)

| registru | `Proiectii/` | `Saft/` | `Api/` (Module) | WebApi controllers | total „raport" |
|---|---|---|---|---|---|
| `RegistruStoc` | 0 | 2 | 0 | 1 (`SoldStocController`) | **3** |
| `RegistruContabil` | 2 (`ContabilProiectii`, `TvaProiectii`) | 2 | 0 | 6 (`Balanta`, `FisaCont`, `Itv`, `RegistruJurnal`, `Saft`, `SoldPartener`) | **10** |
| `RegistruTva` | 3 (`D300`, `D394`, `Tva`) | 3 | 1 (`Rdc/ReturClientApply`) | 4 (`D300`, `D394`, `RectificativaTva`, `TvaControllere`) | **11** |
| `RegistruImobilizari` | 0 | 0 | 2 (`Imo/ImobilizariApply`, `Imo/ImobilizariDtos`) | 2 (`Amo`, `Imobilizari`) | **4** |
| `SoldPerioada*` | 0 | 0 | 0 | 0 | **0** (se citesc DOAR prin `SolduriService`) |
| `PartidaDeschisa` | 1 (`ImperecheriProiectii`) | 0 | 0 | 0 | **1** |
| `Imperecheri` | 1 | 1 | 2 (`ApiProiectii`, `Trz/*`) | 2 (`Imperecheri`, `Perioade`) | **6** |

### F.4 Ce citește din registre ALTCEVA decât rapoartele

- **Motorul**: `StocService` (sold intermediar, FIFO, valoarea golirii) prin
  `SolduriService.MiscariCumulate`; `SolduriService` (snapshot + reconstrucție);
  `MotorOperare` la anulare/storno (`:587-592`, `:639-641`); `AmortizareService`
  (`:96-111`, situația fișei); `RegistruTvaService.PerioadaOriginalului` (`:243`);
  `InchidereTvaService.Solduri` (`:268`, prin atomii cumulați);
  `CorectieService` (`:382`).
- **Culegerea**: `LoturiCulegereService.cs:291` — un lot cu rânduri de registru nu
  se mai șterge.
- **Gardienii**: `GardianEditare` nu citește registrele, doar refuză scrierea lor;
  `GardianEditare.cs:983` citește `RegistruTva` ca să refuze ștergerea unui `TipTva`.
- **Unelte**: `Import1C` (`Deschidere`, `Reconciliere`, `ReconciliereLuna`,
  `Reluare`, `Sabotaj`, `Diagnostic`, `Alocare`, `Saft1C`, `Program`),
  `ModelCheck` (`Program.cs`, `Purja.cs`), `BackfillTva`, `Migrare`.
- **UI**: `UI/ContaUiBaseline.cs` (doar layout/coloane, `ServerView` pe registre).

---

## G. ModelCheck (`nou/tools/ModelCheck`)

**Organizare** (8 fișiere, 32.087 linii):

| fișier | linii | rol |
|---|---|---|
| `Program.cs` | 31.290 | suita întreagă: top-level statements + ~90 de funcții locale `Verifica*`/`Curata*` |
| `MetadataDump.cs` | 201 | `--dump-metadata` (reflecție pură, fără bază) — contractul de metadata al clientului React |
| `Duk.cs` | 190 | validarea XSD/DUK a fișierelor SAF-T |
| `IntegritateTph.cs` | 139 | probele F28-I/J generate din metadata EF; `--dump-integritate-tph <cale>` scrie SQL-ul pentru bazele de import |
| `Purja.cs` | 136 | ștergere FIZICĂ pentru curățenia scenelor |
| `ModelAplicatie.cs` | 52 | modelul XAF al hostului Blazor (probele D85) |
| `ScanareGetObjectByKey.cs` | 45 | plasa F28-N (scanare de sursă) |
| `NumaratorSql.cs` | 34 | interceptor EF care numără instrucțiunile SQL |

**Ce pornește**: NU un host web. Un `EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>`
direct (`Program.cs:212-225`), cu `UseChangeTrackingProxies`, `UseLazyLoadingProxies`,
`UseDeferredDeletion` și interceptorul de numărare. Baza: `Atlas.Conta.BackOffice`
(bugetar, implicit) sau `Atlas.Conta.ModelCheck.Privat` (argumentul `privat`), port
5444, plus sufixul `MODELCHECK_BAZA_SUFIX` pentru rulări paralele (`:87-91`). Pe
profilul privat unealta migrează și seed-uiește singură (`:263-266`). Probele D85
compilează și încarcă modelul aplicației Blazor (`ModelAplicatie.cs`).

**Structura rulării privat**: bloc e2e privat (`:599-4120`, scenele pe marcaje
`E2E-*`, cu `Curata*` la început și la sfârșit) → apoi ~40 de apeluri
`Verifica*(privat: true)` (`:4126-4197`) → `Rezumat()`.

**Probe per familie, profilul privat** (din `run-f28/ui-tip/privat.log`, ultima
rulare: **1450 OK / 0 eșecuri**), grupate după prefixul numelui probei:

| familie (prefix) | probe | ce probează | formă sau regulă |
|---|---|---|---|
| `Api*`/`E2E-API-*` (scene de tier API) | 212 | ușile `*Apply`/`OperareApi` per tip de document: culegere → validare → operare → anulare → storno | **regulă** (refuzuri, stări, cifre postate) |
| `D16-V*` | 71 | modelul SAF-T + societatea raportoare | mixt (formă de DTO + regulă) |
| `IMO-V*` | 67 | imobilizările pe scenă (PIF/CAS/AMO, registrul propriu) | **formă** (coloanele `RegistruImobilizari`) + regulă |
| `D17-V*` | 58 | mișcările și stocurile SAF-T (D406 S) | **formă** (citește `RegistruStoc` per rând) |
| `F27-R*` | 53 | review advers felia 27 (perioade, solduri, partide) | mixt |
| `F21-D*` | 46 | ecranul ITV (previzualizare, motive, stale) | **regulă** |
| `D3-V*` | 41 | D300 (seed + proiecție) | **regulă** (Σ per rând) |
| `D4-V*` | 38 | D394 | **regulă** |
| `SOL-C*`/`SOL-V*` | 53 | snapshot-urile de perioadă și reconstrucția | **formă** (`SolduriPerioada*`, `PartideDeschise`) |
| `ANCORA D*` | 35 | latura pereche a viramentului, grupul conex | regulă |
| `D18-V*` | 30 | valoarea ieșirii care golește (D18-D2) + oracolul din registru | **formă** (citește `RegistruStoc` rând cu rând) |
| `F23-V*` | 30 | implicitele de culegere + gardianul politicilor | regulă |
| `Seed*` | 29 | conținutul seed-ului privat (tipuri de TVA, conturi, politici) | regulă |
| `D15-V*` | 29 | adresele partenerilor + sincronizarea ANAF | regulă |
| `PAR-V*` | 25 | `TotalStingere`, partidele deschise, imperecherea datată | **formă** (`PartidaDeschisa`) |
| `API-IMO-V*` | 23 | ușile `api/pif|cas|amo|imobilizari` | regulă |
| `ACC-V*` | 22 | constatările de închidere și acceptarea pe cheie | regulă |
| `DVI-V*` | 20 | declarația vamală pe scenă | regulă |
| `COR-V*` | 20 | corecția (storno legat + document nou) | regulă |
| `F19-D*` | 20 | plafonul de stingere per contrapartidă × sens | regulă |
| `PDT-V*` | 19 | perioada de declarare pe registrul fiscal | **formă** (`RegistruTva.PerioadaAn/Luna`) |
| `PER-V*`/`PER-C*` | 23 | lanțul perioadelor, comanda de închidere, cursa | regulă |
| `DIR-V*` | 18 | `DataInregistrare` ca reper al registrelor | **formă** |
| `Cautare*` | 18 | oracolul căutării fără diacritice (coloană generată vs C#) | formă (schemă) |
| `IMO-R*` | 16 | review advers felia 26 | regulă |
| `F13-D*` | 15 | taxarea inversă (F13-D1) | regulă |
| `F28-*` | 15 | TPH: discriminatorul ca dată, gardianul (o), integritatea FK/coloane | **formă** (schema TPH) |
| `F24-E*` | 12 | „Explică" == potrivirea motorului | regulă |
| `F24-V*` | 11 | rolul `Configurator` | regulă |
| `F24-P*` | 10 | `Potrivire` ca funcții PURE | **regulă** (cea mai independentă de formă) |
| `R-D*` | 10 | balanța, fișa de cont, registrul-jurnal | **regulă** (Σ, partidă dublă) |
| `AMO-V*` | 9 | aritmetica amortizării | **regulă** (pură) |
| `JT-D*` | 8 | cusătura `RegistruTva` ↔ `RegistruContabil` | **formă** (există al treilea registru) |
| `D85-*` | 7 | modul de acces al ListView-urilor XAF | formă (model XAF) |
| `BP-D*` | 4 | balanța pliată | regulă |
| restul (scene e2e cu nume în proză: `Curățenie*`, `Storno*`, `Consum*`, `Linie*`, `Override*`, `RLF:`, `RDC:`, `DSC*`, `Defect*`, `REGRESIE*`, `PROBA*`, `Risc*`, `VERIFICAREA*`, …) | ≈368 | scenariile end-to-end ale motorului pe date create în test | mixt, majoritar **regulă** (cifre de postare, refuzuri) |

**Formă vs regulă** — care ar cădea la schimbarea formei registrelor:
- **Probează FORMA** (cad dacă registrele își schimbă forma): `D17-V*` (58),
  `D18-V*` (30), `SOL-*` (53), `PAR-V*` (25), `PDT-V*` (19), `DIR-V*` (18),
  `IMO-V*` parțial (67), `JT-D*` (8), `F28-*` (15), `Cautare*` (18) — plus
  fragmentele din scenele e2e care numără rânduri de registru.
- **Probează REGULA** (rămân valabile pe orice formă): `F24-P*` (10, funcții pure),
  `AMO-V*` (9), `R-D*` (10, Σ și partidă dublă), `D3-V*`/`D4-V*` (79, cifre de
  declarație), `F21-D*` (46), `ACC-V*` (22), `COR-V*` (20), `F19-D*` (20),
  `F23-V*` (30), `Api*` (212, refuzuri și stări) — și toate probele `CheckRefuza`.

**Ce n-am putut determina din cod**: numărul exact de probe per FUNCȚIE
`Verifica*` — numele probelor nu conțin numele funcției, iar `Program.cs` are
31.290 de linii cu funcții locale imbricate; gruparea de mai sus e pe prefixul
numelui probei din log, nu pe funcția care o emite. Cifrele sunt EXACTE ca sumă
(1450), dar maparea prefix → funcție e aproximativă acolo unde o funcție emite
probe cu mai multe prefixe (ex. `VerificaImobilizari` emite și `IMO-V*`, și probe
cu nume în proză).

---

## H. Import1C (`nou/tools/Import1C`, 32 de fișiere, 16.017 linii)

**Bucla** (`Bucla.cs`, 1.016 linii + `Reluare.cs`, 382):
`ContextLuna` (`Bucla.cs:20`) → handlerele per tip 1C (`HandlerTip`, `:37`; lista
`Handlere.Toate`, `:74`) COLECTEAZĂ unități de import (`ctx.Planifica`), pe care
bucla le execută în **ordinea cronologiei sursei** (`UnitateImport.Moment` = ora
antetului 1C, `:54`). Ordinea lunii: documente → imperecheri → închiderea de TVA
→ reconciliere. Unitatea de idempotență e documentul (legătura `1C:<view>/<cheieHex>`
scrisă în ACELAȘI commit cu draftul, `:833-846`); unitatea de contract e luna.

**Cum operează**: `ImportaDocument(view, cheieHex, construiesteDraft, …)`
(`Bucla.cs:692`) → `Executa` → `Opereaza(IObjectSpace os, Document doc, string view, string cheieHex)`
(`Bucla.cs:903-931`), care cheamă **`MotorOperare.Opereaza(os, curent)`**
(`:910`) în buclă peste lanțul de copii autogenerați (conexul NIR, plata
secundară), maximum 5 pași. **Nu** trece prin `OperareApi` și nu deschide
tranzacție proprie: `MotorOperare.Opereaza` își comite singur. Gardianul de
editare NU e activ (cale standalone, fără `IObjectSpaceCustomizerService` —
`GardianEditare.cs:52-55`). Fereastra contractului 4 se măsoară prin
`Scara.MidpointBani` în jurul apelului (`:907`, `:929`).

**Raportul de reconciliere** (`reconciliere-yyyyMMdd-HHmmss.txt`, scris de
`JurnalContract.cs:20`; logica în `ReconciliereLuna.cs`, 1.334 linii +
`Reconciliere.cs`, 312 pentru deschidere). Patru contracte **per lună**, la
sfârșitul ei:

1. **sold per cont sintetic OMFP** — Atlas vs balanța 1C mapată; fiecare cont cu
   Δ ≠ 0 trebuie explicat EXACT de registrul divergențelor (`Divergente.cs`,
   438 linii), altfel e eșec, oricât de mic;
2. **închiderea de TVA** — rândurile generate de ITV-ul Atlas vs sumele închiderii
   1C, per corespondență (`4427 = 4423`, `4427 = 4426`);
3. **stoc per produs × gestiune** (cantitate + valoare) vs `BalantaNivel3`
   agregat; **cantitatea e STRICTĂ** și e discriminantul;
4. **deriva reziduurilor de rotunjire** ≤ plafonul calculat din contorul motorului
   (`Scara.MidpointBani`) și convenția bazei, cu praguri statistice
   (`PragRotunjireCheie`, `PragRotunjireSistematica`, `ReconciliereLuna.cs:76-98`).

**De unde vin cifrele**:
- **așteptate (1C)**: view-urile SkyConta din schema `[flax]` a bazei
  `EServicesFlx`, prin `FlaxDb.cs` (636 linii) și `FlaxDocumente.cs` (1.053) —
  contract de coloane, re-citite la reconciliere, nu reținute din import;
- **obținute (Atlas)**: baza se **RECITEȘTE integral din Postgres** prin proiecții
  proprii — `RegistruContabil` (`ContDebitId`/`ContCreditId`/`Valoare`/`Data`,
  unpivotate pe laturi) pentru contractul 1, `RegistruTva` pentru 2, `RegistruStoc`
  (`LotId`→produs, `RepartitorId`, `Cantitate`, `Valoare`, `Data`) pentru 3.
  Explicit: nimic din structurile în memorie ale importului nu intră în
  reconciliere (`ReconciliereLuna.cs:10-16`).

**Ce ar cere de la un motor nou ca să producă ACELAȘI raport**: (a) o cale de
comandă cu semnătura `(ObjectSpace-echivalent, document) → document copil` care
comite singură și întoarce lanțul de autogenerate; (b) un contor de decizii de
rotunjire la jumătatea de ban, expus ca `Scara.MidpointBani`; (c) o citire
re-derivabilă din persistență a celor trei agregate — sold per cont × lună, bază
și taxă per corespondență de închidere, cantitate și valoare per (produs ×
gestiune) × lună — fără să treacă prin structurile importului; (d) idempotență per
document cu o cheie externă scrisă în același commit cu documentul; (e) ștergerea
integrală a unui draft cu tot ce a născut (`Drafturi.Sterge`, folosită la reluare).

**Dimensiune**: cele mai mari fișiere — `ReconciliereLuna.cs` 1.334,
`Program.cs` 1.147, `HandlereStoc.cs` 1.058, `FlaxDocumente.cs` 1.053,
`Bucla.cs` 1.016, `Nomenclatoare.cs` 940, `HandlereVanzare.cs` 841,
`HandlereRetur.cs` 682, `Deschidere.cs` 643, `FlaxDb.cs` 636.

---

## I. Ușile (API, XAF)

**Adaptorul unic**: `Api/OperareApi.cs:43` — comandă prin ID, entitățile nu trec
granița. `Opereaza` (`:47-61`), `AnuleazaOperarea` (`:63-69`), `Storneaza`
(`:71-77`), `Corecteaza` (`:81-86`), `Valideaza` (dry-run, `:92-95`). Fiecare
comandă deschide `TranzactieComanda.Incepe(os)` ca blocarea de perioadă să țină
peste `CommitChanges`.

**Apelanții direcți ai motorului** (`MotorOperare.*`):

| apelant | fișier:linie | ce cheamă |
|---|---|---|
| `OperareApi` | `Api/OperareApi.cs:50`, `:66`, `:74`, `:94` | `Opereaza`, `AnuleazaOperarea`, `Storneaza`, `Valideaza` |
| `CorectieService` | `Motor/CorectieService.cs:74` | `Storneaza` |
| `AsamblareApply` | `Api/Asm/AsamblareApply.cs:490` | `Valideaza` (predicția pe OS aruncat) |
| `Import1C` | `tools/Import1C/Bucla.cs:910` | `Opereaza` |
| `Import1C` (reluare) | `tools/Import1C/Reluare.cs:344` | `Storneaza` |
| `ModelCheck` | `tools/ModelCheck/Program.cs` (numeroase) | toate |

**Apelanții lui `OperareApi`** — o singură formă, repetată:
- **XAF**: `Controllers/DocumentOperareController.cs:37` (`Opereaza`), `:77`
  (`AnuleazaOperarea`), `:98` (`Storneaza`) — un singur controller pentru toate
  tipurile.
- **REST**: 18 controllere, fiecare cu aceleași patru acțiuni
  (`opereaza`/`anuleaza`/`storneaza`/`valideaza`), în
  `Atlas.Conta.BackOffice.WebApi/API/Conta/`: `Amo` (`:111`, `:118`, `:126`, `:135`),
  `Asm` (`:85`…), `Bcs` (`:84`…), `Cas` (`:79`…), `Decont` (`:82`…), `Dsc` (`:46`…),
  `Dvi` (`:119`…), `FacturaIntrare` (`:97`…), `Fcl` (`:95`…), `Itv` (`:210`…),
  `Ldi` (`:85`…), `Nir` (`:92`…), `NotaTransfer` (`:94`…), `Ntc` (`:124`…),
  `Pif` (`:107`…), `Rdc` (`:87`…), `Rlf` (`:85`…), `TrezorerieControllers` (`:130`…).

**Ce primește clientul din operare**: `OperareRezultat`
(`Api/OperareApi.cs:16-20`) = `{ DocumentId, StareNoua, ConexId, Mesaje[] }` —
**nu planul**. Planul (`PlanOperare`, `MotorOperare.cs:26-36`) e `sealed class`
PRIVATĂ și nu iese niciodată din motor. Refuzurile ies ca `OperareException`
(`UserFriendlyException`) cu liniile cumulate pe `\n`, traduse de tierul REST în
`EroriDto.Din(...)`; dry-run-ul (`valideaza`) întoarce chiar lista de refuzuri
(listă goală = trece). Corecția întoarce `CorectieRezultat`
(`Api/OperareApi.cs:25-29`) = `{ OriginalId, CorectieId, StareOriginal, TipCod }`.

**„Explică"** stă în `Api/Politici/ExplicaApply.cs` (`ExplicaApply.Explica`,
`:51`): consumă ACELEAȘI funcții pure ca motorul (`Potrivire.Contare`,
`Potrivire.Stoc`, `Potrivire.Cont`, `Potrivire.Conex`,
`ImpliciteService.Explica` la `:74`) și le împachetează în `ExplicatieDto` cu
blocuri per axă (`ExplicaContareDto` `:174`, `ExplicaStocDto` `:226`,
`ExplicaTvaDto` `:253`, `ExplicaConexDto` `:273`, `ExplicaImplicitDto` `:297`,
`ExplicaScadentaDto`/`ExplicaNumerotareDto` `:82-86`, `ExplicaValidareDto` `:161`).
Ușa securizată e `CereVizibile` (`:39`), execuția pe OS non-secured (`:48-51`).
