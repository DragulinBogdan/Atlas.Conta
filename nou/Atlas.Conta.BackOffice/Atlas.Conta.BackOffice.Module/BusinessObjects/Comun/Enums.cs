using DevExpress.ExpressApp.DC;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

public enum StareDocument { Draft = 0, Operat = 1, Stornat = 2 }

public enum LaturaDocument { Predator = 1, Primitor = 2 }

// Schema registrelor = cod (decizia 14); tipurile legacy de stoc devin valori.
public enum TipStoc {
    Magazie = 1,
    Consum = 2,
    Folosinta = 3,
    Custodie = 4,
    Marfuri = 5,
    Gratuit = 6,
    ProductieNeterminata = 7,
}

public enum DirectieDiferenta { Plus = 1, Minus = 2 }

// Rolul liniei pe Asamblare (FAZA 1C §7): kitting n→m pe stoc — consumurile
// descarcă loturi existente, produsele nasc loturi noi. Fără valoarea 0:
// default-ul invalid e protecția la linii culese fără rol (ca DirectieDiferenta).
public enum DirectieAsamblare { Consum = 1, Produs = 2 }

// Curățarea Clasă/Tip la seed (inventar 10 §2): separă clasele purtătoare de
// stoc de cele tehnice (TVA/Diferențe) și de naturile fără stoc.
public enum NaturaClasa {
    Stoc = 1,
    Serviciu = 2,
    Cheltuiala = 3,
    Imobilizare = 4,
    Tehnica = 5,
    // F7-D2 (viramentul intern, transferul 581): „linia mută bani în interiorul
    // patrimoniului, între două conturi proprii" — nici stoc, nici serviciu,
    // nici cheltuială. NU se refolosește `Tehnica`: acolo stă exact `TRZ`
    // (linia obișnuită de trezorerie), iar discriminarea de care depinde
    // cuplajul laturi↔natură și contarea 581 ar dispărea. Contul de tranzit
    // rămâne DATĂ (TipMaterial.ContImplicit per profil) — motorul nu cunoaște
    // niciun simbol (decizia 29).
    Virament = 6,
}

// `[XafDisplayName]` pe MEMBRI (review F3-D7): XAF îl citește prin
// `EnumDescriptor`, iar dump-ul de metadata (`MetadataDump.LabeluriEnum` prin
// `CaptionHelper`) îl emite — deci și UI-ul XAF, și clientul React arată
// eticheta frumoasă, dintr-o singură sursă.
public enum TipInstrumentPlata {
    [XafDisplayName("Ordin de plată")] OrdinPlata = 1,
    [XafDisplayName("Cec")] Cec = 2,
    [XafDisplayName("Dispoziție de casă")] DispozitieCasa = 3,
    [XafDisplayName("Chitanță")] Chitanta = 4,
}

// Regimul fiscal al unui TipTva (P1, design §2). `NeexigibilLaIncasare` NU
// intră la P1 — se adaugă aditiv odată cu mecanismul (4428 + transfer la
// imperechere); nomenclatorul rezervă doar contul.
public enum RegimTva {
    // Deductibil/colectat după direcția documentului (PoliticaTva).
    Normal = 1,
    // TVA intră în `Valoare`, nu se postează separat — comportamentul
    // profilului bugetar neplătitor și al achizițiilor fără drept de deducere.
    Capitalizat = 2,
    // Autolichidare (Cod fiscal art. 331), DOAR pe latura beneficiarului: pe
    // achiziție (PoliticaTva.Directie = Deductibil) un rând 4426 = 4427 pe
    // valoarea TVA, sold zero; pe livrare (Colectat) furnizorul emite FĂRĂ TVA
    // — nicio taxă, niciun rând (F13-D1).
    TaxareInversa = 3,
    Scutit = 4,
    Neimpozabil = 5,
}

// Direcția postării TVA per tip de document (PoliticaTva): FCT/DEC deduc,
// FCL colectează. E și SENSUL taxării inverse (F13-D1): pe `Deductibil`
// autolichidare 4426 = 4427, pe `Colectat` nicio taxă și niciun rând.
public enum DirectieTva { Deductibil = 1, Colectat = 2 }

// Latura jurnalului de TVA (JT-D1): cumpărări sau vânzări. Sensul NU e o a doua
// axă de configurare — se derivă din `PoliticaTva.Directie` a tipului de
// document (Deductibil → Achiziție, Colectat → Livrare), acolo unde profilul a
// declarat deja că tipul e un eveniment de TVA. Pe rândul de `RegistruTva` e
// SNAPSHOT, ca `Regim` și `Cota` (JT-D3): politica e dată editabilă, iar
// jurnalul unei perioade declarate nu are voie să-și schimbe latura fiindcă
// cineva a rescris politica anul următor. Fără valoarea 0 — convenția locală:
// default invalid = protecție la rânduri scrise fără sens (ca DirectieDiferenta
// și DirectieAsamblare).
public enum SensTva {
    [XafDisplayName("Achiziție")] Achizitie = 1,
    [XafDisplayName("Livrare")] Livrare = 2,
}

// De unde își ia o latură a regulii de contare contul (testul bazei §7.2:
// „contul se rezolvă prin POLITICĂ — per tip partener și/sau per Clasă-Tip").
// Contul explicit al regulii rămâne fallback când sursa nu rezolvă.
public enum SursaCont {
    Explicit = 0,
    TipMaterial = 1,         // TipMaterial.ContImplicit al liniei (302x/303x/6xx…)
    RepartitorPredator = 2,  // Repartitor.ContImplicit de pe latura predator (401/404, 5xx/770…)
    RepartitorPrimitor = 3,  // idem, latura primitor (411 — FacturaIesire; 401/542 — Plata)
}

// Calități transversale pe repartitori (decizia 16) — roluri, nu clase derivate.
[Flags]
public enum CalitateRepartitor {
    Niciuna = 0,
    Gestionar = 1,
    Comisie = 2,
    Departament = 4,
    CentruCost = 8,
    LocConsum = 16,
    Delegat = 32,
    Cursant = 64,
}

// Flag-urile de defalcare din plan (R/M/E/B/F/P + sursa) devin date de validare:
// dimensiuni obligatorii per cont (decizia 15).
[Flags]
public enum DimensiuneFlags {
    Niciuna = 0,
    Repartitor = 1,
    Material = 2,
    CodFunctional = 4,
    CodEconomic = 8,
    SursaFinantare = 16,
    Unitate = 32,
    Proiect = 64,
    CentruCost = 128,
}

// Secțiunea formularului 300 (OPANAF 174/2026) în care stă un rând. Ordinea
// valorilor e ordinea din formular; fără valoarea 0 — convenția locală a
// fișierului (default invalid = protecție la rânduri scrise fără sens).
public enum SectiuneD300 {
    [XafDisplayName("TVA colectată")] Colectata = 1,
    [XafDisplayName("TVA deductibilă")] Deductibila = 2,
    [XafDisplayName("Regularizări")] Regularizari = 3,
}

// Cum se alimentează un rând al decontului (D3-D1) — singura axă după care
// proiecția decide de unde vine cifra:
//   Operatiuni — primește mapări `(TipTva × Sens)` din registru;
//   Total      — calculat în cod, din formula legii (19, 30, 31, 35…45);
//   Oglinda    — copiat din rândul-sursă (`OglindaA`: 20…23, 26, 26.1, 26.2);
//   Extern     — fără sursă în model (agricultori, restituiri, pro-rata,
//                soldurile perioadei precedente): parametru sau 0.
// O mapare care țintește altceva decât `Operatiuni` e REFUZATĂ (D3-D2) — altfel
// cifra ar fi suprascrisă tăcut de formulă/oglindă la prima proiecție.
public enum FelRandD300 {
    [XafDisplayName("Operațiuni")] Operatiuni = 1,
    [XafDisplayName("Total")] Total = 2,
    [XafDisplayName("Oglindă")] Oglinda = 3,
    [XafDisplayName("Extern")] Extern = 4,
}

// Rolul de TERȚ al unui cont din plan (felia 16, D16-D3): pe ce conturi „stă"
// un client și pe ce conturi un furnizor. SAF-T are DOUĂ liste separate
// (`Customers` / `Suppliers`) și cere pe fiecare rând de registru `CustomerID`
// SAU `SupplierID` — rolul e al CONTULUI, nu al laturii (riscul 1 al
// contractului: partenerul de pe DEBITUL unui 401, la plata datoriei, iese
// `SupplierID`).
//
// DE CE date, și nu simboluri în cod: motorul și proiecțiile nu cunosc niciun
// simbol de cont (decizia 29). Care sunt conturile de clienți e o proprietate a
// PLANULUI, deci se seed-uiește per profil — la privat OMFP 1802 (411*, 401*…),
// la bugetar nimic (SAF-T îi e neaplicabil).
//
// `Niciunul` = 0, valoarea implicită: un cont nou nu e terț până nu se spune.
// Aici default-ul e chiar răspunsul corect pentru marea majoritate a planului,
// spre deosebire de enum-urile fără valoarea 0 (`DirectieDiferenta`), unde
// default-ul ar fi fost o linie fără rost.
public enum RolTertCont {
    [XafDisplayName("Niciunul")] Niciunul = 0,
    [XafDisplayName("Client")] Client = 1,
    [XafDisplayName("Furnizor")] Furnizor = 2,
}

// Cine e terțul unei MIȘCĂRI de stoc în SAF-T S (felia 17, D17-D1) — și DE CE nu
// se refolosește `RolTertCont` de mai sus, deși are exact aceleași trei valori.
//
// Convenția secțiunii S e ALTA decât a lui L (xlsx SD.MG.21/22, verbatim):
// livrarea către client pune `CustomerID` = partenerul și `SupplierID` = „0";
// achiziția, simetric; mișcarea internă pune AMBELE = identitatea raportorului.
// La L, latura liberă e raportorul, nu „0" — deci un enum comun ar fi ascuns
// două reguli diferite sub un singur nume și ar fi invitat refolosirea greșită.
//
// Iar SURSA e alta: la L rolul e al CONTULUI (`Cont.RolTert`, un fapt al
// planului); aici e al TIPULUI de document × registrului atins (NIR aduce de la
// furnizor, DSC dă către client, BTR nu iese din patrimoniu) — un fapt al
// politicii, cules pe `PoliticaMiscareSaft`.
//
// `Niciunul` = 0, valoarea implicită: mișcarea e internă până nu se spune
// altceva — răspunsul corect pentru majoritatea tipurilor (BTR/BCS/LDI/ASM).
public enum RolTertSaft {
    [XafDisplayName("Niciunul (mișcare internă)")] Niciunul = 0,
    [XafDisplayName("Client")] Client = 1,
    [XafDisplayName("Furnizor")] Furnizor = 2,
}

// Felul persoanei partenerului (felia 14, D4-D1): singura axă de pe nomenclator
// pe care D394 o cere EXPLICIT (PF ⇒ `tip_partener = 2`, CUI-ul e CNP). Restul
// clasificării 1–4 se DERIVĂ din `Tara` + `InregistratTva` (`D394Proiectii.TipPartener`).
public enum TipPersoana {
    [XafDisplayName("Persoană juridică")] Juridica = 1,
    [XafDisplayName("Persoană fizică")] Fizica = 2,
}

// Tipul de operațiune din secțiunea 2 a D394 (`Int_tipOpSType`, stabil din
// 2016 — de aceea enum, nu nomenclator ca rândurile D300; D4-D2). Denumirile
// sunt cele din formular (§4.9 al structurii). Politica `MapareD394` poate
// ținti doar L/A/V/C/LS/AS: `AI` se DERIVĂ (A × furnizor cu TVA la încasare),
// iar `N` n-are sursă în registru azi (D4-r3).
public enum TipOperatiuneD394 {
    [XafDisplayName("L — livrări de bunuri/prestări de servicii")] L = 1,
    [XafDisplayName("A — achiziții (sistem normal)")] A = 2,
    [XafDisplayName("AÎ — achiziții cu TVA la încasare")] AI = 3,
    [XafDisplayName("LS — livrări în regim special")] LS = 4,
    [XafDisplayName("AS — achiziții în regim special")] AS = 5,
    [XafDisplayName("V — livrări cu taxare inversă")] V = 6,
    [XafDisplayName("C — achiziții cu taxare inversă")] C = 7,
    [XafDisplayName("N — achiziții de la neînregistrați")] N = 8,
}

// Cauza pentru care un grup de registru fiscal nu are unde să cadă în D394 —
// parte din contract (`Neincluse`, D4-D4), cu etichetă pentru ecran.
public enum CauzaNeincludere {
    // `RegistruTva.PartenerId` e null: `SursaContrapartida` Explicit/TipMaterial.
    [XafDisplayName("Fără partener pe rândul de registru")] FaraPartener = 1,
    // Contrapartida e un `Repartitor` care nu e `Partener` (Angajatul de pe DEC).
    [XafDisplayName("Contrapartida nu e partener (angajat/intern)")] RepartitorNePartener = 2,
    // Perechea `(TipTva × Sens)` n-are mapare — scutitele deliberate (SDD/SFD/
    // NIM) sau un `TipTva` propriu al clientului, încă nemapat.
    [XafDisplayName("Tip de TVA fără mapare D394")] TipTvaNemapat = 3,

    // ── Felia 16 (SAF-T, D16-D4): cauzele proprii fișierului D406 ───────────
    // Enum-ul e PARTAJAT deliberat cu D394 (același vocabular de „ce nu intră și
    // de ce"): primele trei cauze sunt aceleași fapte, citite de două formulare.
    // Documentul n-are niciun cont cu `RolTert` pe rândurile lui (nici pe cele
    // ale conexelor lui autogenerate), deci `Invoice.AccountID` (M) — sau
    // `AccountID` al terțului referit de o plată — n-are sursă. Nici factura,
    // nici plata nu se emit: un identificator gol face fișierul invalid.
    [XafDisplayName("Document fără cont de terț")] ContFaraRol = 4,
    // Linia de factură n-are contrapartidă în registrul contabil: singurele ei
    // rânduri sunt cel de TVA și cel al contului de terț (cazul liniilor de STOC
    // ale FCT — recepția contează pe NIR, 26a). `InvoiceLine.AccountID` e
    // obligatoriu și NU se inventează.
    [XafDisplayName("Linie de factură fără cont contrapartidă")] FaraContrapartida = 5,
    // Latura de partener a documentului nu e un `Partener` (nomenclator de alt
    // fel) — factura/plata n-are `CustomerInfo`/`SupplierInfo`.
    [XafDisplayName("Documentul n-are partener pe laturi")] DocumentFaraPartener = 6,
    // Faptul fiscal aparține unui tip care NU are secțiune de facturi în D406
    // (DEC, NTC, bonurile fiscale…): baza lui e în `RegistruTva` și în GL, dar nu
    // într-un `Invoice`. Fără cauza asta, cusătura 3 s-ar fi putut ține doar
    // restrângând registrul la tipurile de factură — adică măsurându-se pe sine.
    [XafDisplayName("Tip de document fără secțiune de facturi")] TipFaraSectiuneFacturi = 7,

    // ── Felia 17 (SAF-T S, D17-D3): cauzele proprii declarației de stocuri ──
    // Rândul de registru de stoc n-are NICIO politică de mișcare pe cheia lui
    // (tip × TipStoc × semn), deci `MovementType` n-are sursă. Se deosebește
    // DELIBERAT de excludere: un rând cu politică FĂRĂ cod e o decizie luată
    // (iese în `Excluse`, cu motivul ei), unul fără politică e o gaură de profil
    // — „nimic nu se pierde” cere ca cele două să nu arate la fel.
    [XafDisplayName("Rând de stoc fără cod de mișcare")] FaraCodMiscare = 8,
    // Produsul mișcat n-are cont de stoc (`TipMaterial.ContImplicit`), iar
    // `MovementLine.AccountID` e obligatoriu. Un cont inventat e interzis (73e),
    // deci linia iese din fișier și intră aici, cu cantitatea și valoarea ei.
    [XafDisplayName("Produs fără cont de stoc")] FaraContStoc = 9,
    // Politica are un cod de mișcare care NU e în nomenclatorul D406 (`999`,
    // spații, un cod scos dintr-o versiune veche a listei). Gardianul îl refuză
    // la culegere, dar seed-ul și conectoarele scriu pe ușa non-secured, iar
    // validatorul respinge fișierul ÎNTREG pe o valoare din afara listei — deci
    // proiecția RE-verifică și scoate rândurile afară, în loc să le declare cu
    // un cod inventat. `MovementTypeTable` nu-l vede niciodată.
    [XafDisplayName("Cod de mișcare necunoscut în politică")] CodMiscareNecunoscut = 10,
}

// Cauza unui avertisment SAF-T (D16-D4) — aceeași formă agregată ca la D394
// (`{Cod, Mesaj, Numar, Suma, Exemple[≤5]}`, fixul 7 al review-ului advers): pe o
// lună reală un cod ar produce mii de rânduri identice, iar semnalul util s-ar
// îneca. Pe sârmă string (57a), eticheta din metadata.
//
// Regula de fond a listei: fiecare cod e un loc în care FORMULARUL cere ceva ce
// modelul nu are, iar fișierul iese cu valoarea de rezervă declarată de ANAF (0,
// `H87`, `000000`, „Nespecificat") — niciodată cu o valoare inventată și
// niciodată tăcut.
public enum CodAvertismentSaft {
    [XafDisplayName("Produs fără cod NC")] FaraCodNc = 1,
    [XafDisplayName("Produs fără unitate de măsură UN/ECE")] FaraUnitateMasura = 2,
    [XafDisplayName("Adresă incompletă (localitatea e obligatorie)")] AdresaIncompleta = 3,
    [XafDisplayName("Tip de TVA fără cod SAF-T")] TipTvaFaraCodSaft = 4,
    [XafDisplayName("Cont cu funcție necunoscută")] TipContNecunoscut = 5,
    [XafDisplayName("Plată către un angajat (fără identitate de partener)")] PlataCatreAngajat = 6,
    [XafDisplayName("Factură în valută")] FacturaInValuta = 7,
    [XafDisplayName("Antetul societății e incomplet")] SocietateIncompleta = 8,
    [XafDisplayName("Factură fără cont de terț")] ContFaraRolPeFactura = 9,
    [XafDisplayName("Parteneri cu același identificator SAF-T")] PartenerDublat = 10,
    [XafDisplayName("Partener fără cod fiscal valid")] PartenerFaraCuiValid = 11,
    [XafDisplayName("Linie de factură fără cont contrapartidă")] LinieFaraContrapartida = 12,
    [XafDisplayName("Rând de registru fără partener pe cont de terț")] TertFaraPartener = 13,
    // Plata către un PARTENER ale cărei rânduri n-ating niciun cont cu `RolTert`
    // (462 „Creditori diverși", 461, un cont de decontare oarecare): terțul n-ar
    // avea ce `AccountID` să declare în master files, iar un `<AccountID/>` gol
    // face fișierul invalid. Plata iese în `Neincluse`, nu cu un cont inventat.
    [XafDisplayName("Plată fără cont de terț pe rânduri")] PlataFaraContTert = 14,

    // ── Felia 17 (SAF-T S, D17-D3) ──────────────────────────────────────────
    // Politica cere un rol de terț (NIR ⇒ furnizor, DSC ⇒ client), dar
    // documentul n-are niciun `Partener` pe laturi — nici pe ale lui, nici pe
    // ale documentului-sursă când e autogenerat. Ambele identificatoare ies cu
    // ale raportorului (mișcare internă), ceea ce e onest, nu inventat.
    [XafDisplayName("Mișcare cu rol de terț, fără partener")] TertLipsaPeMiscare = 15,
    // `PhysicalStock.ProductType` cere contul de stoc al produsului; lipsă ⇒ `0`.
    // Pe MIȘCĂRI aceeași gaură e mai gravă (`AccountID` e obligatoriu), deci
    // acolo iese `Neincluse/FaraContStoc`, nu avertisment.
    [XafDisplayName("Produs fără cont de stoc")] ProdusFaraContStoc = 16,
    // Sold final negativ pe (gestiune × lot). NU e „gardianul de sold a dormit"
    // (25d păzește soldul pe cheia de stoc a MOTORULUI, iar el nu e încălcat):
    // pe baza de import cauza e deriva de rotunjire PER LOT, pe care contractul
    // 1C o declară nereconciliabilă structural (45e/52). Registrul e sursa —
    // se declară CA ATARE și se strigă, nu se ajustează la zero.
    [XafDisplayName("Sold de stoc negativ")] SoldNegativ = 17,
    // Sold de deschidere pe un `TipStoc` fără nicio politică cu cod (Custodie,
    // Gratuit, ProductieNeterminata): `PhysicalStock` nu-l declară. N-are
    // document, deci nu poate fi `Neincluse` — dar nici nu are voie să dispară.
    [XafDisplayName("Sold pe tip de stoc neraportat")] SoldPeTipStocNeraportat = 18,
    // `MovementReference` (max 35) n-a încăput cu sufixele ei: numărul s-a tăiat
    // de la început. Identitatea rămâne unică prin sufixe, dar e scurtată.
    [XafDisplayName("Referință de mișcare trunchiată")] MovementReferenceTrunchiat = 19,

    // ── Felia 17, fixurile review-ului advers (D17-V6) ──────────────────────
    // DOUĂ documente de același tip cu ACELAȘI număr (importul 1C aduce numărul
    // sursei, iar conexele îl moștenesc): `(codTip, Numar)` nu e o identitate,
    // deci `MovementReference` primește un discriminant (`#1`, `#2`) ca să
    // rămână unică în fișier. Cifra se strigă — numerele duplicate sunt un fapt
    // al nomenclatorului, nu al generatorului.
    [XafDisplayName("Număr de document duplicat (referință discriminată)")] NumarDocumentDuplicat = 20,
    // Același fapt pe LUNAR: două facturi din aceeași secțiune cu același
    // `InvoiceNo`. Aici NU se discriminează nimic — `InvoiceNo` e numărul REAL
    // al facturii și nu are voie să fie inventat —, dar cifra se declară.
    [XafDisplayName("Număr de factură duplicat")] NumarFacturaDuplicat = 21,
    // `MovementPostingDate` = `DataOperare`, care e ora RULĂRII (importul din
    // 1C a operat 12/2025 în 2026-08). O dată de postare în afara perioadei
    // declarate e o contradicție în fișier, deci elementul (opțional) se OMITE
    // și se strigă, în loc să se scrie o dată care contrazice antetul.
    [XafDisplayName("Data postării în afara perioadei (omisă)")] DataPostariiInAfaraPerioadei = 22,
    // Grupul `(Document × Storno × Cod)` are DOUĂ roluri de terț distincte pe
    // rândurile lui (politici diferite per `TipStoc`): fișierul are un singur
    // `CustomerID`/`SupplierID` per linie, iar rolul se ia deterministic de pe
    // prima linie. Politica ar trebui să fie coerentă pe același cod.
    [XafDisplayName("Roluri de terț mixte pe aceeași mișcare")] RolTertMixt = 23,
    // Intrare de stoc fizic cu cantitate 0 la ambele capete și valoare nenulă
    // la vreunul — reziduul valoric al derivei de rotunjire per lot (45e).
    // Se DECLARĂ ca atare (registrul e sursa), separat de `SoldNegativ`:
    // e alt fapt, cu altă cauză și cu altă cifră.
    [XafDisplayName("Rezidu valoric fără cantitate")] ReziduValoricFaraCantitate = 24,
}

// Cauza unui avertisment D394 (D4-D5, fix 7 al review-ului advers): avertismentele
// ies AGREGATE per cauză (`{Cod, Mesaj, Numar, Suma, Exemple[≤5]}`), nu ca un
// string per partener — pe baza reală un an ar produce mii de rânduri cu aceeași
// cauză și semnalul util s-ar îneca. Pe sârmă string (57a), eticheta din metadata.
public enum CodAvertismentD394 {
    [XafDisplayName("Parteneri uniți pe același CUI")] CuiUnit = 1,
    [XafDisplayName("Același CUI cu clasificări diferite")] ClasificariDiferite = 2,
    [XafDisplayName("Înregistrat în scopuri de TVA fără cod fiscal")] Tip1FaraCui = 3,
    [XafDisplayName("Persoană fizică fără CNP valid")] PfFaraCnp = 4,
    [XafDisplayName("TVA pe un tip fără coloană de TVA")] TvaPeTipFaraColoana = 5,
    [XafDisplayName("Cotă ne-întreagă")] CotaNeintreaga = 6,
    [XafDisplayName("V/C fără detaliul pe categorii de bunuri (op11)")] FaraOp11 = 7,
    [XafDisplayName("Combinație partener × tip refuzată de formular")] CombinatieRefuzata = 8,
    [XafDisplayName("Partener șters din nomenclator")] PartenerSters = 9,
}

// De ce n-a generat închiderea lunară de TVA (F21-D2). `InchidereTvaService`
// colapsa cele trei cauze de „nimic de generat" pe un singur `null`, iar
// apelantul le compensa cu un string pasat de el (`motivFaraDraft` în Import1C)
// — adică ecranul nu putea spune DE CE. Cauzele sunt distincte pentru operator:
//   ProfilInert   — profilul n-are `PoliticaInchidereTva` completă (bugetarul);
//                   nici conturi, deci nici solduri de arătat;
//   InchidereVie  — luna are deja o închidere Draft sau Operată (stornatul nu
//                   contează — regenerarea e permisă natural);
//   FaraSold      — ambele solduri de TVA sunt 0: luna n-are ce închide;
//   NeCronologica — o închidere vie ULTERIOARĂ a închis deja cumulat și luna
//                   asta. E motiv DOAR în `Previzualizeaza`, care e raport;
//                   `Incearca` rămâne refuz zgomotos (`OperareException`, 46c).
//
// Enum-ul stă AICI, nu lângă serviciu, fiindcă dump-ul de metadata (deci
// eticheta din clientul React) ia doar tipurile din spațiul `BusinessObjects`
// (`MetadataDump.EsteRelevant`). Pe sârmă pleacă numele membrului (57a).
public enum MotivNegenerare {
    [XafDisplayName("Profilul nu are conturile închiderii")] ProfilInert = 1,
    [XafDisplayName("Luna are deja o închidere")] InchidereVie = 2,
    [XafDisplayName("Luna n-are sold de TVA de închis")] FaraSold = 3,
    [XafDisplayName("Există o închidere pentru o lună ulterioară")] NeCronologica = 4,
}
