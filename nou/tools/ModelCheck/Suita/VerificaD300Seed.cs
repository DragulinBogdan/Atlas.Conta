using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ================ Felia 12 (D300): seed-ul formularului — D3-V1 ================
// Nomenclatorul rândurilor și politica de așezare, măsurate ca DATE în bază, nu
// ca intenție: seed-ul le scrie, `ContaSeeder.VerificaD300` le apără la scriere
// (aruncă), iar aici se verifică ce a rămas — inclusiv pe o bază care a fost
// seed-uită de o versiune anterioară a uneltei.
//
// Partea de rânduri e comună ambelor profiluri (formularul e al legii, nu al
// profilului contabil); partea de mapare e a profilului privat — bugetarul n-are
// `PoliticaTva`, deci nici rânduri de registru fiscal de așezat pe formular.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaRegistruTva`.
static class VerificaD300Seed {
    public static void Ruleaza(Suita s, bool privat) {
        using var os = s.Provider.CreateObjectSpace();
        var randuri = os.GetObjectsQuery<RandD300>().ToList();
        var dupaCod = randuri.Where(r => r.Cod != null).ToDictionary(r => r.Cod);
        var dupaId = randuri.ToDictionary(r => r.ID);

        s.Check("D3-V1 rândurile D300: cele 55 de poziții ale formularului (OPANAF 174/2026 — 45 de rânduri + 10 sub-rânduri)",
            randuri.Count == ContaSeeder.RanduriD300Asteptate && dupaCod.Count == randuri.Count);
        // `Ordine` e singura sortare corectă a formularului: `Cod` e TEXT, iar
        // alfabetic rd. 10 ar veni înaintea lui rd. 9, iar 12.1 nu s-ar așeza nicăieri.
        s.Check("D3-V1 ordinea formularului: `Ordine` acoperă exact 1..55, fără duplicate",
            randuri.Select(r => r.Ordine).OrderBy(o => o).SequenceEqual(Enumerable.Range(1, randuri.Count)));

        // Rândurile-cheie: cele patru totaluri pe care le calculează proiecția, un
        // sub-rând de operațiuni (12.1 — ținta mapării TI21) și oglinda lui (26.1).
        s.Check("D3-V1 felul rândurilor-cheie: 19/30/31/35 = Total, 12.1 = Operațiuni, 26.1 = Oglindă",
            dupaCod["19"].Fel == FelRandD300.Total
            && dupaCod["30"].Fel == FelRandD300.Total
            && dupaCod["31"].Fel == FelRandD300.Total
            && dupaCod["35"].Fel == FelRandD300.Total
            && dupaCod["12.1"].Fel == FelRandD300.Operatiuni
            && dupaCod["26.1"].Fel == FelRandD300.Oglinda);
        // Numărătoarea pe fel: 28 rânduri de operațiuni, 10 totaluri, 9 oglinzi,
        // 8 externe (D3-D1). O reclasificare tăcută ar muta cifre între mecanisme.
        s.Check("D3-V1 repartiția pe fel: 28 Operațiuni + 10 Total + 9 Oglindă + 8 Extern = 55",
            randuri.Count(r => r.Fel == FelRandD300.Operatiuni) == 28
            && randuri.Count(r => r.Fel == FelRandD300.Total) == 10
            && randuri.Count(r => r.Fel == FelRandD300.Oglinda) == 9
            && randuri.Count(r => r.Fel == FelRandD300.Extern) == 8);
        // Secțiunile, pe pozițiile din formular: 1..19 colectată, 20..35 deductibilă,
        // 36..45 regularizări (cu sub-rândurile în secțiunea părintelui).
        s.Check("D3-V1 secțiunile: pozițiile 1-24 colectată, 25-45 deductibilă, 46-55 regularizări",
            randuri.All(r => r.Sectiune == (r.Ordine <= 24 ? SectiuneD300.Colectata
                : r.Ordine <= 45 ? SectiuneD300.Deductibila : SectiuneD300.Regularizari)));

        // Coloanele care CHIAR există pe rând (D3-D3: unde nu există, proiecția lasă
        // null, nu 0 — un ecran care afișează 0,00 într-o casetă inexistentă minte).
        s.Check("D3-V1 coloanele absente: rd. 13/14/15/29/29.1 n-au TVA, rd. 27/28/31/32/34/35 și 36-45 n-au bază",
            new[] { "13", "14", "15", "29", "29.1" }.All(c => dupaCod[c].AreBaza && !dupaCod[c].AreTva)
            && new[] { "27", "28", "31", "32", "34", "35", "36", "40", "43", "45" }
                .All(c => !dupaCod[c].AreBaza && dupaCod[c].AreTva));

        // ── FORMULARUL ÎNTREG, poziție cu poziție (fix F6 al review-ului advers) ──
        //
        // Verificările de mai sus sunt pe EȘANTIOANE și pe numărătoare: „55 de
        // rânduri", „28 de operațiuni", „rd. 13/14/15 n-au TVA". Toate ar fi trecut
        // și cu rd. 17 clasificat greșit ca `Extern`, sau cu rd. 25 fără coloană de
        // bază — cifra totală rămâne 55, repartiția pe fel rămâne 28/10/9/8 dacă
        // greșeala e o PERMUTARE. Iar seed-ul e singura cale de scriere a
        // nomenclatorului (`[ForbidCRUD]`), deci o poziție greșită n-are nicio altă
        // sită între ea și decontul depus.
        //
        // Tabelul de mai jos e TRANSCRIS din §2 al `d300-structura-2026.md` (adică
        // din ordin), nu importat din seed: o probă care citește tabelul pe care-l
        // verifică probează doar că fișierul se citește pe sine. Fiecare linie:
        // ordinea în formular · cod · secțiune · are bază · are TVA · fel.
        (int Ordine, string Cod, SectiuneD300 Sectiune, bool Baza, bool Tva, FelRandD300 Fel)[] asteptat = [
            // COLECTATĂ — comerț intracomunitar și în afara UE (rd. 1-8): doar bază
            // pe livrări/prestări (1-4), bază ȘI TVA pe taxarea inversă (5-8).
            (1, "1", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (2, "2", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (3, "3", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (4, "3.1", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (5, "4", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (6, "5", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (7, "5.1", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (8, "6", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (9, "7", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (10, "7.1", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (11, "8", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            // COLECTATĂ — interiorul țării și exporturi (rd. 9-16): cotele în vigoare
            // (9/10/11), taxarea inversă internă (12 + cele două sub-rânduri), apoi
            // cele trei rânduri fără coloană de TVA (13 simplificare, 14 scutit cu
            // drept, 15 scutit fără drept) și regularizările (16).
            (12, "9", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (13, "10", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (14, "11", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (15, "12", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (16, "12.1", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (17, "12.2", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (18, "13", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (19, "14", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (20, "15", SectiuneD300.Colectata, true, false, FelRandD300.Operatiuni),
            (21, "16", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            // COLECTATĂ — vânzări la distanță / servicii electronice (17-18) + total.
            (22, "17", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (23, "18", SectiuneD300.Colectata, true, true, FelRandD300.Operatiuni),
            (24, "19", SectiuneD300.Colectata, true, true, FelRandD300.Total),
            // DEDUCTIBILĂ — oglinzile zonei de taxare inversă (20-23).
            (25, "20", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (26, "20.1", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (27, "21", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (28, "22", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (29, "22.1", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (30, "23", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            // DEDUCTIBILĂ — achiziții interne și importuri (24-29.1). Rd. 27/28
            // (compensația agricultorilor) au DOAR coloana TVA; rd. 29/29.1
            // (scutite/neimpozabile) DOAR bază — asimetria e a formularului.
            (31, "24", SectiuneD300.Deductibila, true, true, FelRandD300.Operatiuni),
            (32, "25", SectiuneD300.Deductibila, true, true, FelRandD300.Operatiuni),
            (33, "26", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (34, "26.1", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (35, "26.2", SectiuneD300.Deductibila, true, true, FelRandD300.Oglinda),
            (36, "27", SectiuneD300.Deductibila, false, true, FelRandD300.Extern),
            (37, "28", SectiuneD300.Deductibila, false, true, FelRandD300.Extern),
            (38, "29", SectiuneD300.Deductibila, true, false, FelRandD300.Operatiuni),
            (39, "29.1", SectiuneD300.Deductibila, true, false, FelRandD300.Operatiuni),
            // DEDUCTIBILĂ — totalurile și ajustările (30-35). Rd. 30 e ultimul cu
            // coloană de bază; de la rd. 31 în jos formularul e doar TVA.
            (40, "30", SectiuneD300.Deductibila, true, true, FelRandD300.Total),
            (41, "31", SectiuneD300.Deductibila, false, true, FelRandD300.Total),
            (42, "32", SectiuneD300.Deductibila, false, true, FelRandD300.Extern),
            (43, "33", SectiuneD300.Deductibila, true, true, FelRandD300.Operatiuni),
            (44, "34", SectiuneD300.Deductibila, false, true, FelRandD300.Extern),
            (45, "35", SectiuneD300.Deductibila, false, true, FelRandD300.Total),
            // REGULARIZĂRI art. 303 (36-45): toate doar TVA; externii sunt exact
            // cei patru pe care modelul nu-i are (38/39 de plată, 41/42 negative).
            (46, "36", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
            (47, "37", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
            (48, "38", SectiuneD300.Regularizari, false, true, FelRandD300.Extern),
            (49, "39", SectiuneD300.Regularizari, false, true, FelRandD300.Extern),
            (50, "40", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
            (51, "41", SectiuneD300.Regularizari, false, true, FelRandD300.Extern),
            (52, "42", SectiuneD300.Regularizari, false, true, FelRandD300.Extern),
            (53, "43", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
            (54, "44", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
            (55, "45", SectiuneD300.Regularizari, false, true, FelRandD300.Total),
        ];
        var abateri = asteptat
            .Where(a => !dupaCod.TryGetValue(a.Cod, out var r)
                || (r.Ordine, r.Sectiune, r.AreBaza, r.AreTva, r.Fel)
                    != (a.Ordine, a.Sectiune, a.Baza, a.Tva, a.Fel))
            .Select(a => a.Cod).ToList();
        if (abateri.Count > 0)
            Console.WriteLine($"     MĂSURAT (D3-V1, abateri de la §2): {string.Join(", ", abateri.Select(c =>
                dupaCod.TryGetValue(c, out var r)
                    ? $"rd. {c} → ordine {r.Ordine}, {r.Sectiune}, bază {r.AreBaza}, TVA {r.AreTva}, {r.Fel}"
                    : $"rd. {c} → LIPSEȘTE"))}");
        s.Check("D3-V1 (F6) formularul poziție cu poziție: toate cele 55 de rânduri au `Ordine`, `Sectiune`, perechea "
            + "(AreBaza, AreTva) și `Fel` EXACT cum le scrie ordinul (§2), verificate contra unui tabel transcris "
            + "în probă — nu prin eșantion, fiindcă o permutare trece de orice numărătoare",
            abateri.Count == 0 && asteptat.Length == ContaSeeder.RanduriD300Asteptate);
        // Denumirile nu se transcriu (sunt fraze de ordin, de zeci de cuvinte), dar
        // absența lor s-ar vedea ca o casetă goală pe formular.
        s.Check("D3-V1 (F6) fiecare rând are denumirea din ordin, negoală",
            randuri.All(r => !string.IsNullOrWhiteSpace(r.Denumire)));

        // Sub-rândurile „din care": părintele e prefixul codului (12.1 → 12).
        var subRanduri = randuri.Where(r => r.Cod.Contains('.')).ToList();
        s.Check("D3-V1 sub-rândurile: cele 10 poziții „din care” au părintele rezolvat, exact pe prefixul codului",
            subRanduri.Count == 10
            && subRanduri.All(r => r.ParinteId != null
                && dupaId[r.ParinteId.Value].Cod == r.Cod[..r.Cod.IndexOf('.')])
            && randuri.Where(r => !r.Cod.Contains('.')).All(r => r.ParinteId == null));

        // Oglinzile: zona deductibilă 20…23/26/26.1/26.2 e copia exactă a zonei
        // colectate 5…8/12/12.1/12.2 (taxarea inversă se colectează și se deduce în
        // aceeași perioadă — formularul cere egalitatea ca validare blocantă).
        (string Oglinda, string Sursa)[] oglinzi = [
            ("20", "5"), ("20.1", "5.1"), ("21", "6"), ("22", "7"), ("22.1", "7.1"),
            ("23", "8"), ("26", "12"), ("26.1", "12.1"), ("26.2", "12.2"),
        ];
        s.Check("D3-V1 oglinzile: cele 9 rânduri deductibile trimit fiecare la sursa lui colectată, și doar ele au sursă",
            oglinzi.All(o => dupaCod[o.Oglinda].Fel == FelRandD300.Oglinda
                && dupaCod[o.Oglinda].OglindaAId is Guid id && dupaId[id].Cod == o.Sursa)
            && randuri.All(r => (r.Fel == FelRandD300.Oglinda) == (r.OglindaAId != null)));

        // ---------------- Politica de așezare (D3-D2) ----------------
        var mapari = os.GetObjectsQuery<MapareD300>().ToList();
        if (!privat) {
            // Bugetarul n-are `PoliticaTva` ⇒ `RegistruTva` gol ⇒ o mapare acolo ar
            // fi politică orfană, nu configurare (D3-V7, partea de seed).
            s.Check("D3-V1 (bugetar) profilul n-are nicio mapare D300 — nu produce rânduri de registru fiscal",
                mapari.Count == 0);
            return;
        }

        string[] Randuri(string codTip, SensTva sens) => mapari
            .Where(m => m.TipTva.Cod == codTip && m.Sens == sens)
            .Select(m => m.Rand.Cod).OrderBy(c => c, StringComparer.Ordinal).ToArray();

        s.Check("D3-V1 (privat) tabelul D3-D2 e seed-uit integral: 22 mapări, toate către rânduri de operațiuni",
            mapari.Count == 22 && mapari.All(m => m.Rand.Fel == FelRandD300.Operatiuni));
        s.Check("D3-V1 (privat) cotele în vigoare: N21 → 9/24, N11 → 10/25, N9 doar pe livrare → 11",
            Randuri("N21", SensTva.Livrare).SequenceEqual(["9"])
            && Randuri("N21", SensTva.Achizitie).SequenceEqual(["24"])
            && Randuri("N11", SensTva.Livrare).SequenceEqual(["10"])
            && Randuri("N11", SensTva.Achizitie).SequenceEqual(["25"])
            && Randuri("N9", SensTva.Livrare).SequenceEqual(["11"])
            && Randuri("N9", SensTva.Achizitie).Length == 0);
        // TI21 pe achiziție are O SINGURĂ mapare (12.1); rd. 26.1 e OGLINDĂ, o pune
        // proiecția — a doua mapare ar fi dublat cifra, nu ar fi completat-o.
        s.Check("D3-V1 (privat) taxarea inversă 21%: livrare → 13, achiziție → 12.1 SINGUR (26.1 e oglindă, vine din cod)",
            Randuri("TI21", SensTva.Livrare).SequenceEqual(["13"])
            && Randuri("TI21", SensTva.Achizitie).SequenceEqual(["12.1"]));
        // Cazul „n rânduri" care a cerut nomenclatorul în loc de o coloană pe TipTva:
        // cotele istorice n-au rânduri proprii în forma 2026, deci taxarea inversă de
        // 19% se declară prin regularizări pe AMBELE laturi, din aceeași achiziție.
        s.Check("D3-V1 (privat) cotele istorice: N19 → 16/33, iar TI19 pe achiziție cade pe DOUĂ rânduri (16 ȘI 33)",
            Randuri("N19", SensTva.Livrare).SequenceEqual(["16"])
            && Randuri("N19", SensTva.Achizitie).SequenceEqual(["33"])
            && Randuri("TI19", SensTva.Livrare).SequenceEqual(["13"])
            && Randuri("TI19", SensTva.Achizitie).SequenceEqual(["16", "33"]));
        // NED21 intră în rd. 24 (deci în totalul 30 — taxă DEDUCTIBILĂ), dar
        // nedeductibilul se scade la rd. 31 pe `Regim = Capitalizat`, în proiecție:
        // aici NU e o mapare lipsă, ci exact maparea din tabel.
        s.Check("D3-V1 (privat) scutirile și nedeductibilul: SDD → 14/29, SFD → 15/29, NIM doar achiziție → 29, NED21 doar achiziție → 24",
            Randuri("SDD", SensTva.Livrare).SequenceEqual(["14"])
            && Randuri("SDD", SensTva.Achizitie).SequenceEqual(["29"])
            && Randuri("SFD", SensTva.Livrare).SequenceEqual(["15"])
            && Randuri("SFD", SensTva.Achizitie).SequenceEqual(["29"])
            && Randuri("NIM", SensTva.Livrare).Length == 0
            && Randuri("NIM", SensTva.Achizitie).SequenceEqual(["29"])
            && Randuri("NED21", SensTva.Livrare).Length == 0
            && Randuri("NED21", SensTva.Achizitie).SequenceEqual(["24"]));
        // Ce NU e mapat e mapat DELIBERAT: cele trei perechi din D3-D2 sunt singurele
        // găuri admise printre tipurile seed-uite (gardianul din seeder aruncă altfel).
        s.Check("D3-V1 (privat) perechile (TipTva × Sens) nemapate sunt EXACT cele declarate deliberat în seed, "
            + "fiecare cu motivul ei (N9/Achiziție, NED21/Livrare, NIM/Livrare, IMP pe ambele sensuri — 83g: baza "
            + "și taxa importului se declară din DVI — și tipurile de import pe livrare). Mulțimea se citește din "
            + "LISTA seed-ului, nu se scrie în probă",
            os.GetObjectsQuery<TipTva>().ToList()
                .SelectMany(t => new[] { SensTva.Achizitie, SensTva.Livrare }.Select(s => (t.Cod, Sens: s)))
                .Count(p => Randuri(p.Cod, p.Sens).Length == 0)
                == ContaSeeder.NemapateD300Privat.Count);

        // ══════ F4: gardul de ASCENDENT — dubla numărare pe verticala „din care" ══════
        //
        // Riscul 1 al designului avea un singur gard din două. Indexul unic pe
        // tripletă oprește „aceeași pereche de două ori pe ACELAȘI rând"; nimic nu
        // oprea „aceeași pereche pe rd. 12.1 ȘI pe rd. 12", iar acolo proiecția
        // adună copilul în părinte, deci cifra intră de două ori în rd. 12 și de
        // acolo în totalul rd. 19. Un decont care trece toate validările ANAF și e
        // greșit cu suma dublată e exact forma de defect pe care felia trebuie s-o
        // facă imposibilă.
        //
        // Proba stă lângă contract și se șterge după ea (precedentul mapării de
        // probă din D3-V3), și trece prin FUNCȚIA REALĂ a seed-ului, nu printr-o
        // reimplementare a regulii.
        //
        // Fiecare fază pe ObjectSpace-ul EI: scrierea, verificarea și curățenia sunt
        // trei tranzacții diferite, exact ca în viață (culegerea din UI, apoi
        // `--updateDatabase`). Un singur OS ar fi lăsat identity map-ul să răspundă
        // în locul bazei după ștergerea prin SQL brut.
        string RuleazaGardianul() {
            using var osGard = s.Provider.CreateObjectSpace();
            try {
                ContaSeeder.VerificaD300(osGard, ProfilContabil.Privat);
                return null;
            }
            catch (InvalidOperationException e) { return e.Message; }
        }
        int MapariVii() {
            using var osNum = s.Provider.CreateObjectSpace();
            return osNum.GetObjectsQuery<MapareD300>().Count();
        }
        void SqlBrut(FormattableString sql) {
            using var osSql = s.Provider.CreateObjectSpace();
            ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)osSql).DbContext.Database.ExecuteSql(sql);
        }
        int RefuzuriMapareD300() {
            using var osNum = s.Provider.CreateObjectSpace();
            return osNum.GetObjectsQuery<RefuzSeed>().Count(r => r.Tip == nameof(MapareD300));
        }
        bool ExistaFizic(Guid id) {
            using var osSql = s.Provider.CreateObjectSpace();
            return ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)osSql).DbContext.Database
                .SqlQuery<int>($"SELECT count(*)::int AS \"Value\" FROM \"MapariD300\" WHERE \"ID\" = {id}").Single() > 0;
        }
        {
            Guid idProba;
            using (var osScrie = s.Provider.CreateObjectSpace()) {
                var ti21Seed = osScrie.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
                var rand12 = osScrie.FirstOrDefault<RandD300>(r => r.Cod == "12");
                // Premisa: TI21/achiziție e deja mapat pe rd. 12.1, copilul lui rd. 12.
                var proba = osScrie.CreateObject<MapareD300>();
                proba.TipTva = ti21Seed;
                proba.Sens = SensTva.Achizitie;
                proba.Rand = rand12;
                osScrie.CommitChanges();
                idProba = proba.ID;
            }
            var mesajGard = RuleazaGardianul();
            // SQL brut, nu prin gardian: proba n-are voie să lase un refuz de seed (104i).
            SqlBrut($"DELETE FROM \"MapariD300\" WHERE \"ID\" = {idProba}");
            var mesajDupa = RuleazaGardianul();
            Console.WriteLine($"     MĂSURAT (D3-V1/F4): gardian pe (TI21, Achiziție, rd. 12) lângă rd. 12.1 → "
                + $"„{mesajGard ?? "<NU A ARUNCAT>"}”; după curățare → „{mesajDupa ?? "tace"}”.");
            s.Check("D3-V1 (F4) o pereche (TipTva × Sens) nu poate ținti și un rând, și un ASCENDENT al lui: "
                + "(TI21, Achiziție) pe rd. 12 lângă rd. 12.1 existent e REFUZAT de `ContaSeeder.VerificaD300` "
                + "cu ambele rânduri numite în mesaj — altfel cei 84 de lei ar fi intrat de două ori în rd. 12 "
                + "și în totalul rd. 19",
                mesajGard != null && mesajGard.Contains("rd. 12.1") && mesajGard.Contains("rd. 12,")
                && mesajGard.Contains("ascendent")
                // …iar gardul TACE de îndată ce proba dispare: nu e un refuz permanent
                // al mapării legitime pe rd. 12.1.
                && mesajDupa == null && MapariVii() == 22);
        }

        // ══════ F5 (104i): ștergerea unei mapări e o decizie, nu o gaură ══════
        //
        // Maparea e politică editabilă (decizia 4): ștergerea din XAF trece prin
        // gardian, care lasă `RefuzSeed`. Proba măsoară pe funcțiile REALE că
        // re-seed-ul n-o recreează și că `VerificaMapariD300` n-o citește ca gaură,
        // apoi restaurează prin ștergerea refuzului.
        {
            Guid idSters;
            using (var osTinta = s.Provider.CreateObjectSpace()) {
                var sfd = osTinta.FirstOrDefault<TipTva>(t => t.Cod == "SFD");
                var rand29 = osTinta.FirstOrDefault<RandD300>(r => r.Cod == "29");
                var tinta = osTinta.FirstOrDefault<MapareD300>(m =>
                    m.TipTvaId == sfd.ID && m.Sens == SensTva.Achizitie && m.RandId == rand29.ID);
                idSters = tinta.ID;
                new GardianEditare().OnObjectSpaceCreated(osTinta);
                osTinta.Delete(tinta);
                osTinta.CommitChanges();
            }
            var existaDupaDelete = ExistaFizic(idSters);
            var refuzuri = RefuzuriMapareD300();
            var dupaStergere = MapariVii();

            // Re-seed pe calea reală — aceeași funcție pe care o cheamă profilul.
            using (var osSeed = s.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedMapareD300Privat(osSeed);
                osSeed.CommitChanges();
            }
            var dupaReseed = MapariVii();
            var mesajVerificare = RuleazaGardianul();

            // Restaurare: refuzul se șterge, iar re-seed-ul readuce maparea.
            SqlBrut($"DELETE FROM \"RefuzuriSeed\" WHERE \"Tip\" = 'MapareD300'");
            using (var osSeed = s.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedMapareD300Privat(osSeed);
                osSeed.CommitChanges();
            }
            var dupaRestaurare = MapariVii();
            var mesajFinal = RuleazaGardianul();
            Console.WriteLine($"     MĂSURAT (D3-V1/F5): mapări vii 22 → {dupaStergere} după ștergere "
                + $"(fizic: {existaDupaDelete}, refuzuri: {refuzuri}) → {dupaReseed} după re-seed → "
                + $"{dupaRestaurare} după restaurare; gardian după re-seed: „{mesajVerificare ?? "tace"}”.");
            s.Check("D3-V1 (F5, 104i) ștergerea unei mapări din XAF e FIZICĂ și lasă un `RefuzSeed`; re-seed-ul "
                + "NU o recreează (21 mapări vii, nu 22 — decizia utilizatorului bate tabelul de profil), iar "
                + "gardianul o citește ca „nemapată de utilizator”, nu ca gaură de profil. După ștergerea "
                + "refuzului, re-seed-ul o readuce la 22",
                !existaDupaDelete && refuzuri == 1
                && dupaStergere == 21 && dupaReseed == 21 && mesajVerificare == null
                && dupaRestaurare == 22 && mesajFinal == null);
        }

    }
}
