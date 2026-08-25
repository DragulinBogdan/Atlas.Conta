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
}
