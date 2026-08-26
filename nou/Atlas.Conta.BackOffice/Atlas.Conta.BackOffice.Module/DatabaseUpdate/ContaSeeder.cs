using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Profilul contabil = pachet de seed (decizia 29, materializată la P1):
// profil PER BAZĂ (aliniat cu bază-per-client — 35d), selectat din appsettings
// (`ProfilContabil`); fără profil în date, fără mixaj la runtime.
public enum ProfilContabil { Bugetar = 0, Privat = 1 }

// Nucleul NEUTRU al seed-ului (P1, design §5): ancorele TipDocument, perioadele
// fiscale, repartitorii minimali și MECANISMELE de derivare (Cod-Tip = simbol +
// tăierea segmentelor, 6xx=3xx) — tot ce nu depinde de planul de conturi.
// Conținutul dependent de plan (CSV, Clasă/Tip, politici, TipTva) trăiește în
// pachetele de profil: ProfilBugetar (CPLAN — conținutul de azi, mutat,
// nemodificat) și ProfilPrivat (OMFP 1802). Politicile se definesc pe
// funcționalitate (decizia 21) — legacy/1C sunt direcție, nu canon.
public static class ContaSeeder {
    // `conventie` = convenția de rotunjire a banilor (decizia 51c), dată DOAR la
    // prima seed-uire a bazei: după aceea e înghețată în rândul `SetareProfil`,
    // iar o valoare diferită e refuzată. Null = se păstrează ce are baza (sau
    // AwayFromZero la bază nouă — comportamentul de dinainte de 51c).
    public static void Seed(IObjectSpace os, ProfilContabil profil, MidpointRounding? conventie = null) {
        SeedTipuriDocument(os);
        SeedPerioadeFiscale(os);
        VerificaProfil(os, profil);
        var rotunjire = SeedSetareProfil(os, profil, conventie);
        // Procesul care tocmai a seed-uit baza rotunjește după regula ei —
        // fixată ÎNAINTE de pachetele de profil, ca nicio cale viitoare de seed
        // care ar rotunji bani să nu apuce să ruleze pe default.
        Scara.FixeazaConventia(rotunjire);
        // Nomenclatorul rândurilor D300 e al NUCLEULUI (D3-D1: formularul e al
        // legii, nu al profilului) și se COMITE înaintea pachetelor de profil —
        // maparea privată îl referă prin FK (30e).
        SeedRandD300(os);
        // Nomenclatorul de județe (felia 15, D15-D1) — tot al nucleului și tot
        // înaintea pachetelor de profil: e FK pe `Partener`, deci trebuie să
        // existe înainte de orice seed care ar culege o adresă.
        SeedJudete(os);
        // Nomenclatorul unităților de măsură (felia 16, D16-D2) — tot al
        // nucleului și tot înaintea pachetelor de profil: e FK pe `Produs`.
        SeedUnitatiMasura(os);
        // Rândul societății raportoare (felia 16, D16-D1): se CREEAZĂ gol dacă
        // lipsește, nu se rescrie niciodată. Referă `Judet` și `ContPropriu`,
        // dar numai dacă cineva le-a cules — rândul gol n-are FK-uri.
        SeedSocietate(os);
        os.CommitChanges();
        // Puntea de la `Produs.UM` (text liber) la FK-ul nou — DUPĂ commit-ul
        // nomenclatorului, fiindcă îl interoghează (30e).
        LeagaUnitatileProduselor(os);
        os.CommitChanges();
        if (profil == ProfilContabil.Bugetar)
            ProfilBugetar.Seed(os);
        else
            ProfilPrivat.Seed(os);
        VerificaD300(os, profil);
        VerificaD394(os, profil);
    }

    // Rândul de setare al bazei (decizia 51c). Gardian dublu: profilul (completează
    // `VerificaProfil`, care se uită doar la ancora planului — o bază fără conturi
    // trecea de el) și convenția de rotunjire, ÎNGHEȚATĂ după prima scriere.
    static MidpointRounding SeedSetareProfil(IObjectSpace os, ProfilContabil profil, MidpointRounding? conventie) {
        var setare = os.GetObjectsQuery<SetareProfil>().FirstOrDefault();
        if (setare == null) {
            setare = os.CreateObject<SetareProfil>();
            setare.Profil = profil;
            setare.RotunjireBani = conventie ?? MidpointRounding.AwayFromZero;
            os.CommitChanges();
            return setare.RotunjireBani;
        }
        if (setare.Profil != profil)
            throw new InvalidOperationException(
                $"Baza e seed-uită pe profilul {setare.Profil}, iar configurația cere {profil} — "
                + "profilul contabil e per bază (decizia 35d).");
        if (conventie != null && conventie != setare.RotunjireBani)
            throw new InvalidOperationException(
                $"Baza rotunjește banii cu {setare.RotunjireBani}, iar configurația cere {conventie}. "
                + "Convenția de rotunjire e ÎNGHEȚATĂ per bază (decizia 51c): schimbarea ei pe o bază "
                + "vie amestecă istoricul (jumătățile de ban deja postate au fost decise altfel).");
        return setare.RotunjireBani;
    }

    // Bootstrap-ul host-urilor care NU seed-uiesc (unelte pe bază existentă,
    // aplicația la pornire normală): convenția se citește o dată și se fixează.
    // Fără rând (bază pre-51c, încă ne-seed-uită) rămâne default-ul AwayFromZero.
    public static bool AplicaConventiaRotunjire(IObjectSpace os) {
        var setare = os.GetObjectsQuery<SetareProfil>().FirstOrDefault();
        if (setare == null)
            return false;
        Scara.FixeazaConventia(setare.RotunjireBani);
        return true;
    }

    // Profilul e per bază: o bază seed-uită cu un plan nu se re-seed-uiește cu
    // altul. Ancorele: 891.01.00 există doar în CPLAN (bugetar), 4426 doar în
    // OMFP (simboluri fără segmente).
    static void VerificaProfil(IObjectSpace os, ProfilContabil profil) {
        if (os.GetObjectsCount(typeof(Cont), null) == 0)
            return;
        var ancora = profil == ProfilContabil.Bugetar ? "891.01.00" : "4426";
        if (os.FirstOrDefault<Cont>(c => c.Simbol == ancora) == null)
            throw new InvalidOperationException(
                $"Baza are alt plan de conturi decât profilul configurat ({profil}) — profilul contabil e per bază (decizia 35d).");
    }

    // Decizia 20: nomenclatorul de tipuri oglindește clasele 1:1 — doar ancoră FK + UI.
    static void SeedTipuriDocument(IObjectSpace os) {
        (string Cod, string Denumire, string ClrType)[] tipuri = [
            ("FCT", "Factură intrare", nameof(FacturaIntrare)),
            ("FCL", "Factură ieșire", nameof(FacturaIesire)),
            ("NIR", "Notă de intrare-recepție", nameof(NIR)),
            ("BCS", "Bon de consum", nameof(BonConsum)),
            ("BTR", "Notă de transfer", nameof(NotaTransfer)),
            ("BPR", "Raport de producție", nameof(RaportProductie)),
            ("LDI", "Listă diferențe inventar", nameof(ListaDiferenteInventar)),
            ("DEC", "Decont", nameof(Decont)),
            ("PLT", "Plată", nameof(Plata)),
            ("INC", "Încasare", nameof(Incasare)),
            // Al 11-lea derivat (P2, decizia 37a): ancora e în nucleu pentru
            // AMBELE profiluri; la bugetar rămâne tip inert (fără politici), ca BPR.
            ("DSC", "Descărcare de gestiune", nameof(DescarcareGestiune)),
            // Al 12-lea derivat (FAZA 1C §5): nota contabilă — ușa de import
            // (decizia 9) și tip de culegere manuală, în AMBELE profiluri.
            ("NTC", "Notă contabilă", nameof(NotaContabila)),
            // Al 13-lea derivat (FAZA 1C §6): închiderea lunară de TVA — notă
            // contabilă GENERATĂ. Ancora e în nucleu pentru AMBELE profiluri; la
            // bugetar rămâne tip inert (fără PoliticaInchidereTva, fără
            // numerotare), ca DSC/BPR.
            ("ITV", "Închidere TVA", nameof(InchidereTva)),
            // Al 14-lea derivat (FAZA 1C §7): asamblarea (kitting n→m pe stoc).
            // Ancora e în nucleu pentru AMBELE profiluri; la bugetar rămâne tip
            // inert (fără politici), ca DSC/ITV/BPR.
            ("ASM", "Asamblare", nameof(Asamblare)),
            // Al 15-lea și al 16-lea derivat (FAZA 1C §7): retururile, pe
            // corespondența de STORNO (valori negative pe latura originală).
            // Ancorele sunt în nucleu pentru AMBELE profiluri; la bugetar rămân
            // tipuri inerte (fără politici), ca DSC/ITV/ASM/BPR.
            ("RLF", "Retur la furnizor", nameof(ReturFurnizor)),
            ("RDC", "Retur de la client", nameof(ReturClient)),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipDocument>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipDocument>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.ClrType = t.ClrType;
            }
        }
    }

    // Pivotul gardienilor din decizia 14; anul curent de lucru, deschis.
    static void SeedPerioadeFiscale(IObjectSpace os) {
        if (os.GetObjectsCount(typeof(PerioadaFiscala), null) > 0)
            return;
        for (int luna = 1; luna <= 12; luna++) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = 2026;
            p.Luna = luna;
            p.Inchisa = false;
        }
    }

    // Corpul formularului 300 (OPANAF nr. 174/2026, M.Of. 105/09.02.2026), în
    // NUCLEU pentru AMBELE profiluri (D3-D1): formularul e al legii, nu al
    // profilului contabil — bugetarul are aceleași 55 de poziții, doar fără
    // nicio mapare. 45 de rânduri numerotate + 10 sub-rânduri „din care";
    // `Ordine` = poziția în formular, singura sortare corectă (`Cod` e text:
    // alfabetic „10" ar veni înaintea lui „9").
    //
    // Idempotent pe `Cod`. Părinții și oglinzile se leagă din DICȚIONARUL
    // trecerii curente, nu printr-o a doua interogare: rândurile abia create nu
    // s-ar găsi la `FirstOrDefault` înainte de commit, iar o dependență de
    // ordinea de creare ar fi fost o capcană tăcută.
    //
    // `Cod` e SINGURA cheie; TOT restul se REscrie la fiecare trecere, inclusiv
    // pe rândurile preexistente (fix F6 al review-ului advers). Prima formă
    // scria câmpurile doar la creare, ceea ce transforma seed-ul în „insert dacă
    // lipsește" — iar un rând al LEGII care s-a schimbat (o denumire corectată,
    // o coloană adăugată de un ordin nou, un `Fel` reclasificat) ar fi rămas
    // pentru totdeauna în forma primei seed-uiri, pe TOATE bazele existente.
    // Nomenclatorul e `[ForbidCRUD]` tocmai fiindcă seed-ul e singura lui cale
    // de scriere; atunci seed-ul trebuie să fie și singura lui autoritate.
    // Rescrierea e inofensivă prin construcție: EF nu emite UPDATE pentru o
    // valoare identică, deci pe o bază la zi trecerea rămâne fără efect.
    internal static void SeedRandD300(IObjectSpace os) {
        const SectiuneD300 COL = SectiuneD300.Colectata, DED = SectiuneD300.Deductibila,
            REG = SectiuneD300.Regularizari;
        const FelRandD300 OP = FelRandD300.Operatiuni, TOT = FelRandD300.Total,
            OGL = FelRandD300.Oglinda, EXT = FelRandD300.Extern;

        // Cod · Denumire · Secțiune · are Bază · are TVA · Fel · Părinte · OglindaA
        (string Cod, string Denumire, SectiuneD300 Sectiune, bool Baza, bool Tva,
            FelRandD300 Fel, string Parinte, string Oglinda)[] randuri = [
            // ---- TVA COLECTATĂ: comerț intracomunitar și în afara UE (1–8) ----
            ("1", "Livrări intracomunitare de bunuri, scutite conform art. 294 alin. (2) lit. a) și d) din Codul fiscal", COL, true, false, OP, null, null),
            ("2", "Regularizări livrări intracomunitare de bunuri, scutite conform art. 294 alin. (2) lit. a) și d) din Codul fiscal", COL, true, false, OP, null, null),
            ("3", "Livrări de bunuri sau prestări de servicii pentru care locul livrării/prestării este în afara României, precum și livrări intracomunitare de bunuri scutite conform art. 294 alin. (2) lit. b) și c) din Codul fiscal, din care:", COL, true, false, OP, null, null),
            ("3.1", "Prestări de servicii intracomunitare care nu beneficiază de scutire în statul membru în care taxa este datorată", COL, true, false, OP, "3", null),
            ("4", "Regularizări privind prestările de servicii intracomunitare care nu beneficiază de scutire în statul membru în care taxa este datorată", COL, true, false, OP, null, null),
            ("5", "Achiziții intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă), din care:", COL, true, true, OP, null, null),
            ("5.1", "Achiziții intracomunitare pentru care cumpărătorul este obligat la plata TVA (taxare inversă), iar furnizorul este înregistrat în scopuri de TVA în statul membru din care a avut loc livrarea", COL, true, true, OP, "5", null),
            ("6", "Regularizări privind achizițiile intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă)", COL, true, true, OP, null, null),
            ("7", "Achiziții de bunuri, altele decât cele de la rd. 5 și 6, și achiziții de servicii pentru care beneficiarul din România este obligat la plata TVA (taxare inversă), din care:", COL, true, true, OP, null, null),
            ("7.1", "Achiziții de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă)", COL, true, true, OP, "7", null),
            ("8", "Regularizări privind achizițiile de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă)", COL, true, true, OP, null, null),
            // ---- TVA COLECTATĂ: livrări în interiorul țării și exporturi (9–16) ----
            ("9", "Livrări de bunuri și prestări de servicii, taxabile cu cota 21%", COL, true, true, OP, null, null),
            ("10", "Livrări de bunuri și prestări de servicii, taxabile cu cota 11%", COL, true, true, OP, null, null),
            ("11", "Livrări de bunuri taxabile cu cota 9% conform art. III din Legea nr. 141/2025", COL, true, true, OP, null, null),
            ("12", "Achiziții de bunuri și servicii supuse măsurilor de simplificare pentru care beneficiarul este obligat la plata TVA (taxare inversă), din care:", COL, true, true, OP, null, null),
            ("12.1", "Achiziții de bunuri și servicii, taxabile cu cota 21%", COL, true, true, OP, "12", null),
            ("12.2", "Achiziții de bunuri, taxabile cu cota 11%", COL, true, true, OP, "12", null),
            ("13", "Livrări de bunuri și prestări de servicii supuse măsurilor de simplificare (taxare inversă)", COL, true, false, OP, null, null),
            ("14", "Livrări de bunuri și prestări de servicii scutite cu drept de deducere, altele decât cele de la rd. 1-3", COL, true, false, OP, null, null),
            ("15", "Livrări de bunuri și prestări de servicii scutite fără drept de deducere", COL, true, false, OP, null, null),
            ("16", "Regularizări taxă colectată", COL, true, true, OP, null, null),
            // ---- TVA COLECTATĂ: vânzări la distanță / servicii electronice (17–18) ----
            ("17", "Vânzări intracomunitare de bunuri la distanță și prestări de servicii de telecomunicații, radiodifuziune, televiziune și electronice către persoane neimpozabile din alt stat membru, cu locul în România, conform art. 278^1 alin. (1) din Codul fiscal", COL, true, true, OP, null, null),
            ("18", "Regularizări privind vânzările intracomunitare de bunuri la distanță și prestările de servicii de telecomunicații, radiodifuziune, televiziune și electronice către persoane neimpozabile din alt stat membru, conform art. 278^1 alin. (1) din Codul fiscal", COL, true, true, OP, null, null),
            ("19", "TOTAL TAXĂ COLECTATĂ (sumă de la rd. 1 până la rd. 18, cu excepția celor de la rd. 3.1, 5.1, 7.1, 12.1, 12.2)", COL, true, true, TOT, null, null),
            // ---- TVA DEDUCTIBILĂ: oglinzile zonei de taxare inversă (20–23) ----
            ("20", "Achiziții intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă), din care:", DED, true, true, OGL, null, "5"),
            ("20.1", "Achiziții intracomunitare pentru care cumpărătorul este obligat la plata TVA (taxare inversă), iar furnizorul este înregistrat în scopuri de TVA în statul membru din care a avut loc livrarea", DED, true, true, OGL, "20", "5.1"),
            ("21", "Regularizări privind achizițiile intracomunitare de bunuri pentru care cumpărătorul este obligat la plata TVA (taxare inversă)", DED, true, true, OGL, null, "6"),
            ("22", "Achiziții de bunuri, altele decât cele de la rd. 20 și 21, și achiziții de servicii pentru care beneficiarul din România este obligat la plata TVA (taxare inversă), din care:", DED, true, true, OGL, null, "7"),
            ("22.1", "Achiziții de servicii intracomunitare pentru care beneficiarul este obligat la plata TVA (taxare inversă)", DED, true, true, OGL, "22", "7.1"),
            ("23", "Regularizări privind achizițiile de servicii intracomunitare pentru care beneficiarul din România este obligat la plata TVA (taxare inversă)", DED, true, true, OGL, null, "8"),
            // ---- TVA DEDUCTIBILĂ: achiziții în interiorul țării și importuri (24–29.1) ----
            ("24", "Achiziții de bunuri și servicii taxabile cu cota de 21%, altele decât cele de la rd. 27", DED, true, true, OP, null, null),
            ("25", "Achiziții de bunuri și servicii, taxabile cu cota de 11%", DED, true, true, OP, null, null),
            ("26", "Achiziții de bunuri și servicii supuse măsurilor de simplificare pentru care beneficiarul este obligat la plata TVA (taxare inversă), din care:", DED, true, true, OGL, null, "12"),
            ("26.1", "Achiziții de bunuri, taxabile cu cota 21%", DED, true, true, OGL, "26", "12.1"),
            ("26.2", "Achiziții de bunuri, taxabile cu cota 11%", DED, true, true, OGL, "26", "12.2"),
            ("27", "Compensația în cotă forfetară pentru achiziții de produse și servicii agricole de la furnizori care aplică regimul special pentru agricultori", DED, false, true, EXT, null, null),
            ("28", "Regularizări privind compensația în cotă forfetară", DED, false, true, EXT, null, null),
            ("29", "Achiziții de bunuri și servicii scutite de taxă sau neimpozabile, din care:", DED, true, false, OP, null, null),
            ("29.1", "Achiziții de servicii intracomunitare scutite de taxă", DED, true, false, OP, "29", null),
            // ---- TVA DEDUCTIBILĂ: totalurile și ajustările (30–35) ----
            ("30", "TOTAL TAXĂ DEDUCTIBILĂ (sumă de la rd. 20 până la rd. 28, cu excepția celor de la rd. 20.1, 22.1, 26.1, 26.2)", DED, true, true, TOT, null, null),
            ("31", "SUB-TOTAL TAXĂ DEDUSĂ conform art. 297 și art. 298 sau art. 300 și art. 298 din Codul fiscal și compensație în cotă forfetară", DED, false, true, TOT, null, null),
            ("32", "TVA efectiv restituită cumpărătorilor străini, inclusiv comisionul unităților autorizate", DED, false, true, EXT, null, null),
            ("33", "Regularizări taxă dedusă", DED, true, true, OP, null, null),
            ("34", "Ajustări conform pro-rata / ajustări de taxă", DED, false, true, EXT, null, null),
            ("35", "TOTAL TAXĂ DEDUSĂ (rd. 31 + rd. 32 + rd. 33 + rd. 34)", DED, false, true, TOT, null, null),
            // ---- REGULARIZĂRI conform art. 303 din Codul fiscal (36–45) ----
            ("36", "Suma negativă a TVA în perioada de raportare (rd. 35 - rd. 19)", REG, false, true, TOT, null, null),
            ("37", "Taxa de plată în perioada de raportare (rd. 19 - rd. 35)", REG, false, true, TOT, null, null),
            ("38", "Soldul TVA de plată din decontul perioadei fiscale precedente (rd. 44), neachitat până la data depunerii decontului de TVA", REG, false, true, EXT, null, null),
            ("39", "Diferențe de TVA de plată stabilite de organele fiscale prin decizie comunicată și neachitate până la data depunerii decontului de TVA", REG, false, true, EXT, null, null),
            ("40", "TVA de plată cumulat (rd. 37 + rd. 38 + rd. 39)", REG, false, true, TOT, null, null),
            ("41", "Soldul sumei negative a TVA reportate din perioada precedentă pentru care nu s-a solicitat rambursare (rd. 45 din decontul perioadei fiscale precedente)", REG, false, true, EXT, null, null),
            ("42", "Diferențe negative de TVA stabilite de organele de inspecție fiscală prin decizie comunicată până la data depunerii decontului de TVA", REG, false, true, EXT, null, null),
            ("43", "Suma negativă a TVA cumulate (rd. 36 + rd. 41 + rd. 42)", REG, false, true, TOT, null, null),
            ("44", "Sold TVA de plată la sfârșitul perioadei de raportare (rd. 40 - rd. 43)", REG, false, true, TOT, null, null),
            ("45", "Soldul sumei negative de TVA la sfârșitul perioadei de raportare (rd. 43 - rd. 40)", REG, false, true, TOT, null, null),
        ];

        var dupaCod = new Dictionary<string, RandD300>();
        for (int i = 0; i < randuri.Length; i++) {
            var r = randuri[i];
            var rand = os.FirstOrDefault<RandD300>(x => x.Cod == r.Cod);
            if (rand == null) {
                rand = os.CreateObject<RandD300>();
                rand.Cod = r.Cod;
            }
            rand.Denumire = r.Denumire;
            rand.Sectiune = r.Sectiune;
            rand.Ordine = i + 1;
            rand.AreBaza = r.Baza;
            rand.AreTva = r.Tva;
            rand.Fel = r.Fel;
            dupaCod[r.Cod] = rand;
        }
        // Legăturile se rescriu la fel de necondiționat ca restul — inclusiv
        // ȘTERGEREA uneia care nu mai e în tabel (`null` explicit): un rând care
        // a încetat să fie „din care" sau oglindă trebuie să și-o piardă, altfel
        // proiecția ar continua să adune într-un părinte care nu mai există în
        // formular. Ordinea (a doua trecere) rămâne obligatorie: ținta poate fi
        // un rând creat mai târziu în prima trecere.
        foreach (var r in randuri) {
            var rand = dupaCod[r.Cod];
            rand.Parinte = r.Parinte != null ? dupaCod[r.Parinte] : null;
            rand.OglindaA = r.Oglinda != null ? dupaCod[r.Oglinda] : null;
        }
    }

    // Județele (felia 15, D15-D1). Aceeași disciplină ca `SeedRandD300`: `Cod`
    // e cheia, restul se RESCRIE necondiționat la fiecare trecere — nomenclatorul
    // e `[ForbidCRUD]` tocmai fiindcă seed-ul e singura lui cale de scriere,
    // deci seed-ul trebuie să fie și singura lui autoritate (o denumire
    // corectată la un ordin nou ar fi rămas altfel înghețată în forma primei
    // seed-uiri, pe toate bazele existente). EF nu emite UPDATE pentru o valoare
    // identică, deci pe o bază la zi trecerea rămâne fără efect.
    //
    // Lista trăiește în `JudeteRo` (lângă conversii), nu aici: conectoarele au
    // nevoie de `DupaCodAuto`/`DupaCodCnp`/`DupaDenumire` ÎNAINTE de a atinge
    // baza, iar funcțiile pure se probează fără scenă.
    // Public: ModelCheck (alt assembly) probează rescrierea pe calea reală.
    public static void SeedJudete(IObjectSpace os) {
        foreach (var j in JudeteRo.Toate) {
            var judet = os.FirstOrDefault<Judet>(x => x.Cod == j.Cod);
            if (judet == null) {
                judet = os.CreateObject<Judet>();
                judet.Cod = j.Cod;
            }
            judet.Denumire = j.Denumire;
            judet.CodAuto = j.CodAuto;
            judet.CodCnp = j.CodCnp;
        }
    }

    // 41 de județe + municipiul București (ISO 3166-2:RO). Public: ModelCheck
    // (alt assembly) verifică aceeași cifră pe calea reală.
    public const int JudeteAsteptate = 42;

    // Unitățile de măsură UN/ECE (felia 16, D16-D2). Aceeași disciplină ca
    // `SeedJudete`: `Cod` e cheia, `Denumire` se REscrie necondiționat —
    // nomenclatorul e `[ForbidCRUD]`, deci seed-ul e singura lui autoritate, iar
    // o traducere corectată la o publicare nouă trebuie să ajungă pe bazele
    // existente. EF nu emite UPDATE pentru o valoare identică.
    //
    // 2.163 de rânduri, deci se citește tabela O DATĂ, în dicționar, în loc de
    // 2.163 de `FirstOrDefault` (fiecare = un round-trip; la seed-ul unei baze
    // noi asta ar fi însemnat minute în loc de secunde). `IgnoreQueryFilters`
    // NU se folosește: rândurile șterse logic rămân șterse, iar indexul unic e
    // filtrat pe `GCRecord = 0` tocmai ca re-crearea să fie posibilă.
    // Public: ModelCheck (alt assembly) probează rescrierea pe calea reală.
    public static void SeedUnitatiMasura(IObjectSpace os) {
        var existente = os.GetObjectsQuery<UnitateMasura>()
            .ToDictionary(u => u.Cod, StringComparer.Ordinal);
        foreach (var u in UnitatiMasuraUnEce.Toate) {
            if (!existente.TryGetValue(u.Cod, out var um)) {
                um = os.CreateObject<UnitateMasura>();
                um.Cod = u.Cod;
            }
            um.Denumire = u.Denumire;
        }
    }

    // UN/ECE Rec 20 + Rec 21, așa cum le publică ANAF în foaia `Unitati_masura`
    // (16.02.2026): 2.164 de rânduri cu cod, dintre care `B30` apare de DOUĂ ori
    // (o dată în corpul rec20 și o dată în blocul de modificări de la coadă) ⇒
    // 2.163 de coduri DISTINCTE. Public: ModelCheck verifică aceeași cifră pe
    // calea reală.
    public const int UnitatiMasuraAsteptate = 2163;

    // PUNTEA `Produs.UM` → `Produs.UnitateMasuraId` (felia 16, D16-D2).
    //
    // DE CE AICI și nu în migrație, deși felia o cerea acolo: la momentul
    // migrației nomenclatorul `UnitatiMasura` e GOL (migrația tocmai creează
    // tabela; seed-ul o umple pe urmă), deci orice `UPDATE ... FROM UnitatiMasura`
    // scris în migrație ar fi fost un no-op garantat — cod care pare să facă
    // ceva și nu face. Aici puntea rulează după ce nomenclatorul există, cu
    // ACELAȘI dicționar (`UnitatiMasuraRo.Rezolva`) pe care îl folosesc și
    // conectoarele, nu cu o transcriere a lui în SQL: două forme ale aceleiași
    // liste ar fi fost două liste. Intenția instrucțiunii — „niciun cod C# nu
    // rulează ÎN migrație" — rămâne respectată.
    //
    // Umple DOAR unde FK-ul e gol: o legătură pusă de om (sau corectată după o
    // rezolvare greșită) nu se rescrie niciodată. Ce nu se rezolvă rămâne gol —
    // NICIODATĂ o ghicire (produsul fără FK iese în fișier cu `H87` +
    // avertisment agregat, deci golul e vizibil, nu tăcut).
    //
    // Se lucrează pe dicționar, nu cu un `FirstOrDefault` per produs: pe baza de
    // import sunt zeci de mii de produse.
    static void LeagaUnitatileProduselor(IObjectSpace os) {
        var faraUnitate = os.GetObjectsQuery<Produs>()
            .Where(p => p.UnitateMasuraId == null && p.UM != null && p.UM != "")
            .ToList();
        if (faraUnitate.Count == 0)
            return;
        var unitati = os.GetObjectsQuery<UnitateMasura>()
            .ToDictionary(u => u.Cod, StringComparer.Ordinal);
        foreach (var produs in faraUnitate) {
            var cod = UnitatiMasuraRo.Rezolva(produs.UM);
            if (cod != null && unitati.TryGetValue(cod, out var unitate))
                produs.UnitateMasura = unitate;
        }
    }

    // Societatea raportoare (felia 16, D16-D1). SINGURUL seed care CREEAZĂ fără
    // să rescrie — și asta e regula, nu o scăpare: rândul e DATE ale clientului
    // (nume, CUI, adresă, contact, bază contabilă), culese de om, nu o listă a
    // legii. `--forceUpdate` pe o bază cu societatea completată n-are voie s-o
    // golească (riscul 12 al contractului). Ce face seed-ul e doar să existe
    // rândul, ca ecranul să aibă ce deschide.
    //
    // Default-urile (`Tara` RO, `BazaContabila` A) vin de pe tip, nu de aici:
    // un rând creat din UI trebuie să pornească la fel ca unul creat de seed.
    // Public: ModelCheck probează pe calea reală că re-seed-ul nu rescrie.
    public static void SeedSocietate(IObjectSpace os) {
        if (os.GetObjectsQuery<Societate>().Any())
            return;
        os.CreateObject<Societate>();
    }

    // 45 de rânduri numerotate + 10 sub-rânduri „din care" (OPANAF 174/2026).
    // Public: ModelCheck (alt assembly) verifică aceeași cifra pe calea reala.
    public const int RanduriD300Asteptate = 55;

    // Gardianul de profil al feliei D300 (D3-D8, în siajul lui 36c). Rulează
    // DUPĂ pachetele de profil, spre deosebire de `VerificaProfil`, care apără
    // ancora planului ÎNAINTE de a scrie ceva: aici obiectul verificat e chiar
    // rezultatul seed-ului, iar o verificare dinaintea lui ar fi fost vacuă.
    //
    // Partea de MAPARE e a profilului privat și trăiește în pachetul lui
    // (`ProfilPrivat.VerificaMapariD300`): nucleul n-are de unde ști ce coduri
    // de `TipTva` seed-uiește un profil, iar lista nemapatelor deliberate e
    // exact tabelul D3-D2, care e conținut de profil (29c).
    public static void VerificaD300(IObjectSpace os, ProfilContabil profil) {
        var randuri = os.GetObjectsQuery<RandD300>().ToList();
        if (randuri.Count != RanduriD300Asteptate)
            throw new InvalidOperationException(
                $"Nomenclatorul rândurilor D300 are {randuri.Count} rânduri, nu {RanduriD300Asteptate} "
                + "(OPANAF 174/2026: 45 de rânduri + 10 sub-rânduri).");
        if (randuri.Select(r => r.Ordine).Distinct().Count() != randuri.Count)
            throw new InvalidOperationException(
                "Rândurile D300 au poziții (`Ordine`) duplicate — ordinea formularului nu mai e definită.");
        foreach (var r in randuri) {
            // Sub-rândul „din care" se recunoaște după cod („12.1"): fără părinte
            // ar dispărea din agregarea rândului-mamă.
            if (r.Cod.Contains('.') && r.ParinteId == null)
                throw new InvalidOperationException($"Sub-rândul D300 {r.Cod} nu are părinte rezolvat.");
            if ((r.Fel == FelRandD300.Oglinda) != (r.OglindaAId != null))
                throw new InvalidOperationException(
                    $"Rândul D300 {r.Cod} are `Fel` = {r.Fel} și "
                    + (r.OglindaAId == null ? "nicio sursă de oglindă" : "totuși o sursă de oglindă") + ".");
        }
        var mapari = os.GetObjectsQuery<MapareD300>().ToList();
        if (profil == ProfilContabil.Bugetar) {
            // Bugetarul n-are `PoliticaTva`, deci `RegistruTva` îi rămâne gol:
            // o mapare acolo ar fi politică orfană, nu configurare.
            if (mapari.Count > 0)
                throw new InvalidOperationException(
                    $"Profilul bugetar are {mapari.Count} mapări D300, deși nu produce rânduri de registru fiscal.");
            return;
        }
        foreach (var m in mapari)
            if (m.Rand.Fel != FelRandD300.Operatiuni)
                throw new InvalidOperationException(
                    $"Maparea D300 {m.TipTva.Cod}/{m.Sens} țintește rd. {m.Rand.Cod}, de fel {m.Rand.Fel} — "
                    + "doar rândurile de operațiuni se alimentează din mapări (D3-D2).");
        VerificaMapariFaraAscendent(mapari, randuri);
        ProfilPrivat.VerificaMapariD300(os, mapari);
    }

    // Punte pentru ModelCheck: pachetele de profil rămân `internal` (nu sunt
    // API-ul modulului, ci conținut), dar re-seed-ul mapărilor D300 trebuie
    // probat pe FUNCȚIA REALĂ — cea pe care o cheamă `--updateDatabase` —, nu pe
    // o imitație scrisă în probă. Un singur seam, cu numele profilului în el.
    public static void SeedMapareD300Privat(IObjectSpace os) => ProfilPrivat.SeedMapareD300(os);

    // Gardianul D394 (felia 14, D4-D2), geamănul lui `VerificaD300`: bugetarul
    // n-are registru fiscal ⇒ 0 mapări; la privat ținta nu e niciodată AI/N
    // (AI se derivă, N n-are sursă) și fiecare tip seed-uit e mapat sau declarat
    // nemapat (jumătatea de profil, `ProfilPrivat.VerificaMapariD394`).
    public static void VerificaD394(IObjectSpace os, ProfilContabil profil) {
        var mapari = os.GetObjectsQuery<MapareD394>().ToList();
        if (profil == ProfilContabil.Bugetar) {
            if (mapari.Count > 0)
                throw new InvalidOperationException(
                    $"Profilul bugetar are {mapari.Count} mapări D394, deși nu produce rânduri de registru fiscal.");
            return;
        }
        foreach (var m in mapari)
            if (!MapareD394.TintaPermisa(m.Tip, m.Sens))
                throw new InvalidOperationException(
                    $"Maparea D394 {m.TipTva.Cod}/{m.Sens} țintește {m.Tip} — pe livrare se mapează doar L/V/LS, "
                    + "pe achiziție doar A/C/AS; AÎ se derivă din partener, iar N n-are sursă în registru (D4-D2).");
        ProfilPrivat.VerificaMapariD394(os, mapari);
    }

    public static void SeedMapareD394Privat(IObjectSpace os) => ProfilPrivat.SeedMapareD394(os);

    // Lista nemapatelor deliberate e parte din contract (D4-D2): proiecția și
    // ModelCheck o citesc prin nucleu (pachetul de profil rămâne internal).
    public static IReadOnlyCollection<(string TipTva, SensTva Sens, string Motiv)> NemapateD394Privat =>
        ProfilPrivat.NemapateD394;

    // DUBLA NUMĂRARE pe verticala „din care" (fix F4 al review-ului advers) —
    // riscul 1 al designului, rămas cu un singur gard din două.
    //
    // Proiecția adună rândul-părinte ca „mapările lui DIRECTE + Σ copiii", ceea
    // ce e corect cât timp o pereche `(TipTva, Sens)` nu cade pe AMÂNDOI. Dacă
    // TI21/achiziție e mapat și pe rd. 12.1, și pe rd. 12, cei 84 de lei intră
    // în rd. 12 de DOUĂ ori (o dată direct, o dată urcați din copil), iar de
    // acolo în totalul rd. 19 — un decont care se validează la ANAF și e greșit
    // cu suma dublată. Gardul nu poate sta în proiecție (acolo cifra e deja
    // făcută) și nici în forma „un rând nu primește două mapări ale aceleiași
    // perechi" (indexul unic al tripletei acoperă exact atâta): singura formă
    // corectă e pe LANȚUL de ascendenți, la orice adâncime.
    //
    // Ce NU e interzis: aceeași pereche pe două rânduri FRAȚI (TI19/achiziție pe
    // rd. 16 ȘI rd. 33) — acolo multiplicitatea e cerută de formular, cele două
    // sunt laturi diferite ale aceleiași operațiuni și nu se însumează niciodată
    // în același total.
    static void VerificaMapariFaraAscendent(
            IReadOnlyCollection<MapareD300> mapari, IReadOnlyCollection<RandD300> randuri) {
        var parinti = randuri.ToDictionary(r => r.ID, r => r.ParinteId);
        // Lanțul de ascendenți al unui rând, cu gardă de ciclu (aceeași
        // precauție ca proiecția: `Parinte` e o navigație, iar un ciclu
        // introdus din greșeală ar transforma verificarea într-o buclă infinită).
        HashSet<Guid> Ascendenti(Guid id) {
            var sus = new HashSet<Guid>();
            var curent = parinti.GetValueOrDefault(id);
            while (curent is Guid pid && sus.Add(pid))
                curent = parinti.GetValueOrDefault(pid);
            return sus;
        }
        foreach (var pereche in mapari.GroupBy(m => new { m.TipTvaId, m.Sens })) {
            var tinte = pereche.Select(m => m.RandId).Distinct().ToList();
            if (tinte.Count < 2)
                continue;
            foreach (var tinta in tinte) {
                var sus = Ascendenti(tinta);
                // `Cast<Guid?>` ca „negăsit" să fie `null`, nu `Guid.Empty`:
                // sentinela ar fi fost un al doilea înțeles al unei valori care
                // are deja unul (Guid-ul nescris).
                if (tinte.Where(sus.Contains).Cast<Guid?>().FirstOrDefault() is not Guid conflict)
                    continue;
                var copil = pereche.First(m => m.RandId == tinta);
                var parinte = pereche.First(m => m.RandId == conflict);
                throw new InvalidOperationException(
                    $"Maparea D300 {copil.TipTva.Cod}/{copil.Sens} țintește și rd. {copil.Rand.Cod}, și "
                    + $"rd. {parinte.Rand.Cod}, care îi e ascendent („din care”) — cifra ar intra de două ori "
                    + "în rândul-părinte și în totalul lui. Păstrați o singură mapare pe verticală.");
            }
        }
    }

    // Minimul pentru fluxurile e2e: două gestiuni, o unitate (loc de consum),
    // comisia de inventariere — identici în ambele profiluri.
    internal static void SeedRepartitoriMinimali(IObjectSpace os) {
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG1") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG1";
            g.Denumire = "Magazia centrală";
        }
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG2") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG2";
            g.Denumire = "Magazia secundară";
        }
        var sediu = os.FirstOrDefault<UnitateInterna>(x => x.Cod == "SEDIU");
        if (sediu == null) {
            sediu = os.CreateObject<UnitateInterna>();
            sediu.Cod = "SEDIU";
            sediu.Denumire = "Sediul central";
        }
        // Locul de consum e calitate transversală (decizia 16); sediul e primul
        // consumator — |= ca să nu strice calitățile adăugate din UI.
        sediu.Calitati |= CalitateRepartitor.LocConsum;

        var comisie = os.FirstOrDefault<UnitateInterna>(x => x.Cod == "COMISIE");
        if (comisie == null) {
            comisie = os.CreateObject<UnitateInterna>();
            comisie.Cod = "COMISIE";
            comisie.Denumire = "Comisia de inventariere";
        }
        comisie.Calitati |= CalitateRepartitor.Comisie;
    }

    // Tăierea segmentelor terminale spre sintetic (302.02.00.2 → 302.02.00 →
    // 302.02) — mecanismul comun al derivărilor din simbol; la OMFP simbolurile
    // n-au segmente, deci rămâne potrivirea exactă.
    internal static Guid? ContDinSimbol(IReadOnlyDictionary<string, Guid> conturi, string simbol) {
        while (simbol.Length > 0 && !conturi.ContainsKey(simbol))
            simbol = simbol.Contains('.') ? simbol[..simbol.LastIndexOf('.')] : "";
        return simbol.Length > 0 ? conturi[simbol] : null;
    }

    // Maparea Clasă/Tip → cont e date (decizia 4); Cod-ul Tipului E un simbol de
    // cont (10 §2), deci seed-ul o derivă: potrivire exactă, apoi tăierea
    // segmentelor terminale (detalierea sub sintetic aparține Tipului — decizia 10).
    internal static void SeedContImplicitTipMaterial(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        foreach (var tip in os.GetObjectsQuery<TipMaterial>().Where(t => t.ContImplicitId == null).ToList())
            tip.ContImplicitId = ContDinSimbol(conturi, tip.Cod);
    }

    // Contarea 6xx = 3xx per Clasă/Tip (echivalentul curat al celor 18 modele
    // legacy) — rând per TipMaterial cu Natura=Stoc (debitul diferă per Tip).
    // Debitul se DERIVĂ din simbolul contului de stoc: prima cifră 3→6
    // (301→601, 302.01→602.01), cu tăierea de segmente; `exceptii` acoperă
    // mapările care nu urmează schimbarea primei cifre (profilul privat:
    // 371→607, 345→711, 381→608 — decizia 29c). Incremental: tipurile fără
    // rând primesc regulă la fiecare updater; cele cu simbol non-3xx (bonuri
    // valorice 532/409) nu primesc — rând manual la nevoie (decizia 21).
    // Folosită de BCS (consum, fără filtru de semn), LDI (minus, SemnFiltru=-1),
    // DSC (costul descărcării) și RDC (costul care REVINE — `pastreazaSemn`:
    // corespondența de storno postează 607 = 371 cu valoarea negativă a liniei).
    internal static void SeedContare6xxDin3xx(IObjectSpace os, TipDocument tipDoc, int? semnFiltru,
        IReadOnlyDictionary<string, string> exceptii = null, bool pastreazaSemn = false) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var acoperite = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDoc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
        var tipuriStoc = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.Clasa.Natura == NaturaClasa.Stoc)
            .Select(t => new { t.ID, t.Cod }).ToList();
        foreach (var tip in tipuriStoc) {
            if (acoperite.Contains(tip.ID) || !tip.Cod.StartsWith('3'))
                continue;
            var simbol = exceptii?.GetValueOrDefault(tip.Cod) ?? ('6' + tip.Cod[1..]);
            var contDebit = ContDinSimbol(conturi, simbol);
            if (contDebit == null)
                continue;
            var regula = os.CreateObject<RegulaContare>();
            regula.TipDocument = tipDoc;
            regula.TipMaterialId = tip.ID;
            regula.SemnFiltru = semnFiltru;
            regula.PastreazaSemn = pastreazaSemn;
            regula.SursaContDebit = SursaCont.Explicit;
            regula.ContDebitId = contDebit;
            regula.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    // Derivarea de VÂNZARE pe FacturaIesire (P2, design §6) — mecanism în nucleu
    // (decizia 29c). Pe o linie de stoc regula generică FCL ar posta creditul pe
    // contul de STOC al Tipului (371); corecția: rând RegulaContare per TipMaterial
    // cu Natura=Stoc — debit `RepartitorPrimitor` (contul clientului, fallback
    // `fallbackDebit`, ca genericul), credit Explicit = contul de VENIT derivat din
    // `mapaVenit` (371→707, 345→701, 381→708…), altfel `fallbackVenit`. Potrivirea
    // exactă pe TipMaterial bate genericul în motor (26c), deci genericul rămâne
    // pentru servicii. Incremental, ca 6xx=3xx: tipurile deja acoperite se sar, iar
    // cele fără cont de venit rezolvabil se sar (același tratament ca `contDebit`
    // negăsit acolo). Costul descărcării (607/711 = 371/345) trăiește separat pe DSC.
    internal static void SeedContareVanzare(IObjectSpace os, TipDocument tipDoc, string fallbackDebit,
        IReadOnlyDictionary<string, string> mapaVenit, string fallbackVenit) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var acoperite = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDoc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
        var tipuriStoc = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.Clasa.Natura == NaturaClasa.Stoc)
            .Select(t => new { t.ID, t.Cod }).ToList();
        foreach (var tip in tipuriStoc) {
            if (acoperite.Contains(tip.ID))
                continue;
            var simbolVenit = mapaVenit.GetValueOrDefault(tip.Cod) ?? fallbackVenit;
            var contVenit = ContDinSimbol(conturi, simbolVenit);
            if (contVenit == null)
                continue;
            var regula = os.CreateObject<RegulaContare>();
            regula.TipDocument = tipDoc;
            regula.TipMaterialId = tip.ID;
            regula.SursaContDebit = SursaCont.RepartitorPrimitor;
            regula.ContDebitId = ContDinSimbol(conturi, fallbackDebit);
            regula.SursaContCredit = SursaCont.Explicit;
            regula.ContCreditId = contVenit;
        }
    }

    // Igienă (self-healing): o regulă cu sursă Explicit și fără cont debitor nu
    // poate rezolva niciodată — reziduu al evoluțiilor de schemă. Se șterge;
    // seed-urile incrementale o recreează corect.
    internal static void StergeReguliContareStricate(IObjectSpace os) {
        var stricate = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.SursaContDebit == SursaCont.Explicit && r.ContDebitId == null).ToList();
        if (stricate.Count > 0) {
            os.Delete(stricate);
            os.CommitChanges();
        }
    }

    // Garda de idempotență PER RÂND al unei reguli de contare: cheia unui rând e
    // (tip document × TipMaterial × filtru de natură) — exact discriminarea pe
    // care o folosește motorul la potrivire. Garda „există vreo regulă pe tipul
    // X" (folosită înainte) sare orice rând ADĂUGAT ulterior pe un tip deja
    // seed-uit; șablonul incremental e cel de la 6xx=3xx / vânzare.
    // Potrivirea celor două filtre NULLABLE se face în memorie (lista rândurilor
    // unui tip e mică): semantica SQL a lui `coloană = @parametru NULL` e prea
    // subtilă pentru o gardă de idempotență.
    internal static bool RegulaContareLipsa(IObjectSpace os, TipDocument tipDoc,
        Guid? tipMaterialId, NaturaClasa? naturaFiltru) =>
        !os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDoc.ID)
            .Select(r => new { r.TipMaterialId, r.NaturaFiltru })
            .ToList()
            .Any(r => r.TipMaterialId == tipMaterialId && r.NaturaFiltru == naturaFiltru);

    // Viramentul intern (F7-D6): Clasa/Tipul „VIR" + cele două reguli ale
    // perechii. `simbolTranzit` = contul de viramente interne al PROFILULUI
    // (581 în ambele planuri) — legat EXPLICIT, nu derivat din Cod (precedentul
    // punții S371, 52b): „VIR" nu e simbol de cont, deci derivarea nu-l atinge.
    //
    // Ieșirea (Plata): tranzit = cont propriu SURSĂ (581 = 5311);
    // intrarea (Incasare): cont propriu DESTINAȚIE = tranzit (5121 = 581).
    // Contul propriu se ia din latura lui, FĂRĂ fallback — un cont propriu fără
    // ContImplicit trebuie să pice zgomotos la operare (precedentul 31c).
    internal static void SeedContareVirament(IObjectSpace os,
        TipDocument plt, TipDocument inc, string simbolTranzit) {
        var clasa = os.FirstOrDefault<ClasaProdus>(c => c.Cod == "VIR");
        if (clasa == null) {
            clasa = os.CreateObject<ClasaProdus>();
            clasa.Cod = "VIR";
            clasa.Denumire = "Viramente interne";
            clasa.Natura = NaturaClasa.Virament;
        }
        var tip = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
        if (tip == null) {
            tip = os.CreateObject<TipMaterial>();
            tip.Cod = "VIR";
            tip.Denumire = "Virament intern";
            tip.Clasa = clasa;
        }
        if (tip.ContImplicitId == null)
            tip.ContImplicitId = os.FirstOrDefault<Cont>(c => c.Simbol == simbolTranzit)?.ID;

        if (RegulaContareLipsa(os, plt, tip.ID, null)) {
            var iesire = os.CreateObject<RegulaContare>();
            iesire.TipDocument = plt;
            iesire.TipMaterial = tip;
            iesire.SursaContDebit = SursaCont.TipMaterial;
            iesire.SursaContCredit = SursaCont.RepartitorPredator;
        }
        if (RegulaContareLipsa(os, inc, tip.ID, null)) {
            var intrare = os.CreateObject<RegulaContare>();
            intrare.TipDocument = inc;
            intrare.TipMaterial = tip;
            intrare.SursaContDebit = SursaCont.RepartitorPrimitor;
            intrare.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    internal static void SeedNumerotare(IObjectSpace os, string codTip, string serie) {
        if (os.FirstOrDefault<PoliticaNumerotare>(x => x.TipDocument.Cod == codTip) == null) {
            var numerotare = os.CreateObject<PoliticaNumerotare>();
            numerotare.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == codTip);
            numerotare.Serie = serie;
            numerotare.UrmatorulNumar = 1;
        }
    }
}
