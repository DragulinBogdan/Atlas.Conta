namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// SCARA NUMERICĂ A MODELULUI — o singură definiție, consumată în două locuri:
// configurarea EF (`BackOfficeEFCoreDbContext.AplicaScaraNumerica` → coloane
// `numeric(18,s)`) și motorul (rotunjirea la materializare).
//
// De ce: `numeric` FĂRĂ scară în Postgres păstrează scara valorii scrise, iar
// valorile materializate de motor moșteneau scara ÎMPĂRȚIRII care le-a produs
// (`Lot.PretUnitar = Valoare / Cantitate` → 20+ zecimale, apoi
// `Valoare = PretUnitar × Cantitate` le duce mai departe). Consecința nu e
// cosmetică: un `SUM` server-side peste astfel de valori (ex.
// `ImperechereService.Total`) depășește mantisa lui `decimal` și aruncă
// `OverflowException` la materializare — la importul 1C pe ianuarie 2025 s-au
// pierdut astfel 8 imperecheri. Orice consumator viitor care adună bani în SQL
// (proiecțiile tierului API — decizia 42c) lovește același perete.
//
// Cele trei scări:
//   * BANI (2) — valorile POSTATE: ce intră în registre și în stingeri. Leul
//     are doi bani; o valoare de postare cu mai multe zecimale nu are înțeles
//     contabil, doar reziduu de calcul.
//   * PRET (6) — prețurile UNITARE: identificarea specifică pe lot (decizia 13)
//     cere preț fin, altfel `Valoare = Preț × Cantitate` nu se mai întoarce la
//     valoarea de intrare. Șase zecimale țin round-tripul în toleranța 0,005 pe
//     cantitățile reale (verificat pe sursa 1C: prețurile ei încap în 6).
//   * CANTITATE (3) — verificat pe sursa 1C: nicio cantitate din 2025 (documente
//     + `BalantaNivel3`) nu depășește 3 zecimale, deci fixarea nu mișcă date.
//
// Coloană nouă de tip `decimal`: îi dai un NUME din vocabularul de mai jos sau
// extinzi `ScaraPentru`; gardianul din DbContext refuză modelul altfel — scara
// nu se mai poate pierde tăcut.
public static class Scara {
    public const int Precizie = 18;

    public const int Bani = 2;
    public const int Pret = 6;
    public const int Cantitate = 3;
    public const int Procent = 4;

    // AwayFromZero (rotunjirea „comercială") — nu bancherească: e ce fac și
    // sursele de date (1C, facturile furnizorilor) și ce așteaptă reconcilierea.
    public static decimal RotunjesteBani(decimal v) => Math.Round(v, Bani, MidpointRounding.AwayFromZero);
    public static decimal RotunjestePret(decimal v) => Math.Round(v, Pret, MidpointRounding.AwayFromZero);

    // Convenția e pe NUMELE proprietății, nu pe o listă de coloane: numele astea
    // sunt vocabular stabil al modelului (fiecare derivată nouă aduce alt
    // `PretUnitar`/`PretEvaluare`), iar o listă explicită s-ar dezactualiza tăcut
    // la următorul tip de document. `null` = nume necunoscut → eroare la
    // construirea modelului, cu numele proprietății în mesaj.
    public static int? ScaraPentru(string numeProprietate) => numeProprietate switch {
        // Bani postați: valoarea de postare a rândului principal, a doua valoare
        // de postare (TVA) și suma stinsă de o imperechere.
        "Valoare" or "ValoareTva" or "Suma" => Bani,
        // Prețuri unitare + cursul valutar (BNR dă 4 zecimale, 6 lasă loc).
        "PretUnitar" or "PretEvaluare" or "Curs" => Pret,
        "Cantitate" => Cantitate,
        // Cota de TVA (21, 19, 11, 9, 0) — procent, nu bani.
        "Cota" => Procent,
        _ => null
    };
}
