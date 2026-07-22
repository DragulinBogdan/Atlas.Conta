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

// Curățarea Clasă/Tip la seed (inventar 10 §2): separă clasele purtătoare de
// stoc de cele tehnice (TVA/Diferențe) și de naturile fără stoc.
public enum NaturaClasa {
    Stoc = 1,
    Serviciu = 2,
    Cheltuiala = 3,
    Imobilizare = 4,
    Tehnica = 5,
}

public enum TipInstrumentPlata { OrdinPlata = 1, Cec = 2, DispozitieCasa = 3, Chitanta = 4 }

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
