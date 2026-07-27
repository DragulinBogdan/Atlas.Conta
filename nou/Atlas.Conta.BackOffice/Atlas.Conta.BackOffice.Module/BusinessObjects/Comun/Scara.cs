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

    // CONVENȚIA DE ROTUNJIRE (decizia 51c) — dată de PROFIL, nu constantă de cod.
    // Reziduul de reconciliere e `sum(round)` vs `round(sum)` (motorul rotunjește
    // per rând de registru, sursa poartă valoarea întreagă a lotului): convenția
    // nu-l face să dispară, alege doar SENSUL în care se acumulează. AwayFromZero
    // împinge toate jumătățile de ban în același sens (deriva măsurată pe importul
    // 1C: −4,18 lei/an pe 3.628 rânduri midpoint); ToEven (bancară) le compensează.
    //
    // Default AwayFromZero = comportamentul de dinainte de decizia 51c, deci
    // orice bază neatinsă rămâne numeric identică. Se fixează O DATĂ la pornirea
    // fiecărui host, din rândul `SetareProfil` al bazei (vezi `FixeazaConventia`)
    // — static, nu parametru prin semnăturile motorului: e o proprietate a bazei
    // în care rulează procesul, nu a operației.
    static MidpointRounding conventieBani = MidpointRounding.AwayFromZero;
    static bool conventieFixata;

    public static MidpointRounding ConventieBani => conventieBani;

    // Idempotentă cu aceeași valoare (mai mulți bootstrap-uri în același proces:
    // seed + citire), zgomotoasă la valoare diferită — un proces care ar rotunji
    // în două feluri e o bază amestecată în devenire.
    public static void FixeazaConventia(MidpointRounding conventie) {
        if (conventieFixata && conventie != conventieBani)
            throw new InvalidOperationException(
                $"Convenția de rotunjire a fost deja fixată pe {conventieBani}; nu se poate schimba pe "
                + $"{conventie} în același proces. Convenția e ÎNGHEȚATĂ per bază (decizia 51c) — "
                + "o bază vie nu-și schimbă regula de rotunjire fără migrare de istoric.");
        conventieBani = conventie;
        conventieFixata = true;
    }

    // Câte valori au căzut EXACT pe jumătatea de ban (acolo unde convenția chiar
    // decide). Contorul e materia primă a alarmei de rotunjire din reconciliere:
    // deriva așteptată a convenției e calculabilă din el (n × 0,005 în cel mai
    // rău caz), deci o derivă mult peste ea nu mai e rotunjire, ci defect.
    static long midpointBani;
    public static long MidpointBani => Interlocked.Read(ref midpointBani);

    public static decimal RotunjesteBani(decimal v) {
        // Aritmetică decimal, nu double: restul față de banul întreg e exact.
        if (Math.Abs(v) % 0.01m == 0.005m)
            Interlocked.Increment(ref midpointBani);
        return Math.Round(v, Bani, conventieBani);
    }

    // Prețul unitar NU urmează convenția: `SetareProfil.RotunjireBani` e a scării
    // BANI (valorile postate — decizia 51c); prețul e identificare pe lot, nu
    // postare, iar o bază pe ToEven nu are de ce să-și schimbe tăcut și
    // EVALUAREA (preț de lot la 6 zecimale), doar sensul jumătăților de ban.
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
