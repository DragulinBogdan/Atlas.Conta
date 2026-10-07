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

// ============ Felia 12 (D300): proiecția decontului — D3-V2…V7 ============
// CUSĂTURA pe care o probează: registrul fiscal (felia 11) → rândurile
// formularului 300, prin politica de așezare `(TipTva × Sens) → RandD300`.
// Adică exact locul unde trei mecanisme diferite trebuie să dea o singură cifră:
// mapările (DATE, editabile), agregarea părinților „din care" și a oglinzilor
// (STRUCTURĂ, din nomenclator) și formulele de total (COD, din lege).
//
// DE CE merită o scenă proprie și nu se lipește de `E2E-JTVA`: scena jurnalelor
// e construită să demonstreze granularitatea per document (patru regimuri pe o
// factură, un storno peste graniță de lună), iar decontul are nevoie de exact
// invers — o mulțime CONTROLATĂ de operațiuni, câte una pe fiecare `TipTva`, cu
// sume distincte, într-o lună pe care nimeni altcineva n-o atinge. Cusătura
// D3-D5 (rd. 37 == nota ITV a lunii) e chiar imposibil de măsurat altfel:
// soldurile pe care le închide ITV-ul sunt CUMULATE, deci orice reziduu al
// altei scene ar fi făcut egalitatea falsă dintr-un motiv fără nicio legătură
// cu decontul.
//
// Scena stă pe DOUĂ luni, și asta e o decizie, nu o comoditate:
//   • MAI — numai operațiuni MAPATE. E luna pe care o închide ITV-ul, iar o
//     operațiune nemapată care POSTEAZĂ (achiziția cu cota tranzitorie de 9%,
//     care alimentează 4426 dar n-are rând în forma 2026) ar rupe egalitatea
//     rd. 37 == ITV — corect, dar dintr-un motiv care e chiar constatarea, nu
//     defectul: ce nu încape în formular rămâne în contabilitate.
//   • IUNIE — cele două perechi nemapate deliberat (N9/Achiziție, NIM/Livrare)
//     și perechea operare+storno. Peste ele se citește perioada COMBINATĂ, unde
//     `Nemapate` are ce arăta.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaD300Seed`.
static class VerificaD300 {
    public static void Ruleaza(Suita s, bool cuTva) {
        const string MarcajD3 = "E2E-D300";
        using var os = s.Provider.CreateObjectSpace();

        // Perioada COMBINATĂ (mai + iunie) — pe ea se citesc D3-V2/V3/V5.
        var pStart = new DateOnly(2026, 5, 1);
        var pEnd = new DateOnly(2026, 6, 30);
        // Luna ITV-ului (D3-V4) și luna nemapatelor + a stornării (D3-V6).
        var maiStart = new DateOnly(2026, 5, 1);
        var maiEnd = new DateOnly(2026, 5, 31);
        var iunieStart = new DateOnly(2026, 6, 1);
        var iunieEnd = new DateOnly(2026, 6, 30);

        void CurataD3() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajD3)).Select(r => r.ID).ToList();
            // Documentele se prind pe DOUĂ căi: numărul marcat (FCT/FCL, unde
            // numărul e cules sau pre-completat) ȘI laturile marcate — închiderea de
            // TVA are numărul server-owned, iar singura ei urmă e unitatea internă.
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => d.Numar.StartsWith(MarcajD3)
                    || repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).Distinct().ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruTva>().Where(r => ids.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
                    .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajD3)).ToList());
            // Maparea de probă a avertismentului (vezi D3-V3), dacă o rulare
            // anterioară a murit între creare și ștergere: altfel ar rămâne politică
            // fantomă care mută TVA-ul lui N21 pe rd. 14 pentru toată lumea.
            //
            // TRIPLETA COMPLETĂ, nu „rd. 14 pe livrare": rândul 14 ARE o mapare
            // legitimă de profil (SDD → 14), iar o curățenie pe rând ar fi șters
            // POLITICA, nu proba. Măsurat, nu presupus — exact asta s-a întâmplat la
            // prima rulare: SDD-livrare a apărut în `Nemapate`, rd. 14 a rămas gol,
            // iar rd. 19 a ieșit cu 150 de lei mai mic. O curățenie de scenă n-are
            // voie să atingă seed-ul; când marcajul nu se poate pune pe obiect
            // (`MapareD300` n-are cod), identitatea LUI e singurul filtru corect.
            var randProba = os.FirstOrDefault<RandD300>(r => r.Cod == "14");
            var tipProba = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            if (randProba != null && tipProba != null)
                pj.Adauga(os.GetObjectsQuery<MapareD300>()
                    .Where(m => m.RandId == randProba.ID && m.TipTvaId == tipProba.ID
                        && m.Sens == SensTva.Livrare).ToList());
            pj.Executa();
        }
        CurataD3();

        // ---------------- Premisele MĂSURATE ale scenei ----------------
        // Fără ele, două dintre verificări ar fi vacue sau false dintr-un motiv
        // străin: o lună care mai conține rânduri fiscale ale altcuiva strică
        // cifrele exacte ale lui D3-V2, iar un sold de TVA rămas deschis dinainte
        // intră în ITV (care închide CUMULATIV) și rupe D3-V4.
        var reziduuPerioada = CubScena.FapteIntre(os, pStart, pEnd).Count;

        var politicaItv = os.GetObjectsQuery<PoliticaInchidereTva>().FirstOrDefault();
        // Conturile ca DATE (decizia 29): niciun simbol în probă, la fel ca în motor.
        Guid? contDeductibila = politicaItv?.ContDeductibilaId, contColectata = politicaItv?.ContColectataId,
            contDePlata = politicaItv?.ContDePlataId, contDeRecuperat = politicaItv?.ContDeRecuperatId;
        decimal SoldNetCub(Guid? contId, DateOnly panaLa) => contId == null ? 0m : CubScena.SoldCont(os, contId.Value, panaLa);
        var sold4426Initial = SoldNetCub(contDeductibila, maiEnd);
        var sold4427Initial = SoldNetCub(contColectata, maiEnd);
        // O închidere VIE ulterioară lunii scenei ar face `Genereaza` să refuze
        // cronologic — nu e defectul nostru, dar e premisa noastră.
        var inchideriUlterioare = os.GetObjectsQuery<InchidereTva>()
            .Count(d => d.Data > maiEnd && d.Stare != StareDocument.Stornat);
        Console.WriteLine($"     MĂSURAT (premisele scenei D300): {reziduuPerioada} rânduri fiscale preexistente în "
            + $"{pStart:MM.yyyy}-{pEnd:MM.yyyy}; sold deductibilă {sold4426Initial:N2} / colectată "
            + $"{sold4427Initial:N2} la {maiEnd:dd.MM.yyyy}; {inchideriUlterioare} închideri vii ulterioare.");
        s.Check("D3-V2/V4 premisă: perioada scenei e goală înainte de scenă (niciun rând fiscal străin, niciun sold de "
            + "TVA deschis din alte luni, nicio închidere vie ulterioară) — altfel cifrele exacte de mai jos și "
            + "egalitatea cu nota ITV ar fi măsurate peste conținut străin",
            CubScena.FapteIntre(os, pStart, pEnd).Count == 0
            && SoldNetCub(contDeductibila, maiEnd) == 0m && SoldNetCub(contColectata, maiEnd) == 0m
            && inchideriUlterioare == 0);

        // ---------------- Bugetarul: D3-V7, și atât ----------------
        if (!cuTva) {
            var gol = D300Proiectii.D300(os, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null);
            s.Check("D3-V7 (bugetar): profilul neplătitor n-are `PoliticaTva` ⇒ `RegistruTva` gol ⇒ formularul se "
                + "întoarce ÎNTREG (cele 55 de poziții, în ordinea legii) cu toate cifrele zero, `Nemapate` gol și "
                + "zero avertismente — decontul unui neplătitor e un formular gol, nu o listă goală",
                gol.Randuri.Count == ContaSeeder.RanduriD300Asteptate
                && gol.Randuri.Select(r => r.Ordine).SequenceEqual(Enumerable.Range(1, gol.Randuri.Count))
                && gol.Randuri.All(r => (r.Baza ?? 0m) == 0m && (r.Tva ?? 0m) == 0m)
                && gol.Randuri.All(r => r.Randuri == 0 && r.Surse == null)
                && gol.Nemapate.Count == 0 && gol.Avertismente.Count == 0
                && os.GetObjectsQuery<MapareD300>().Count() == 0);
            // Coloanele absente rămân NULL și pe formularul gol: „0,00" într-o casetă
            // inexistentă minte la fel de tare pe un decont gol ca pe unul plin.
            s.Check("D3-V7 (bugetar): coloanele care nu există în formular ies `null`, nu 0 — rd. 13/14/15/29 fără TVA, "
                + "rd. 31/35/36-45 fără bază",
                gol.Randuri.Where(r => new[] { "13", "14", "15", "29", "29.1" }.Contains(r.Cod))
                    .All(r => r.Baza == 0m && r.Tva == null)
                && gol.Randuri.Where(r => new[] { "31", "35", "36", "37", "40", "43", "44", "45" }.Contains(r.Cod))
                    .All(r => r.Baza == null && r.Tva == 0m));
            CurataD3();
            return;
        }

        // ---------------- Scena privată ----------------
        var mag = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
        TipTva Tva(string cod) => os.FirstOrDefault<TipTva>(t => t.Cod == cod);
        var n21 = Tva("N21"); var n11 = Tva("N11"); var n9 = Tva("N9");
        var ti21 = Tva("TI21"); var n19 = Tva("N19"); var ti19 = Tva("TI19");
        var ned21 = Tva("NED21"); var sdd = Tva("SDD"); var sfd = Tva("SFD"); var nim = Tva("NIM");

        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = MarcajD3 + "-FURN";
        furnizor.Denumire = "Furnizor probă D300";
        var client = os.CreateObject<Partener>();
        client.Cod = MarcajD3 + "-CLIENT";
        client.Denumire = "Client probă D300";
        // Unitatea internă e MARCATĂ deliberat: e singura latură a notei de închidere
        // (predator = primitor), deci singurul fir pe care curățenia o poate prinde —
        // numărul ei e server-owned.
        var unitate = os.CreateObject<UnitateInterna>();
        unitate.Cod = MarcajD3 + "-UNIT";
        unitate.Denumire = "Unitate probă D300";
        os.CommitChanges();

        FacturaIntrare Cumparare(string sufix, DateOnly data) {
            var f = os.CreateObject<FacturaIntrare>();
            f.Numar = MarcajD3 + sufix;
            f.Data = data;
            f.Predator = furnizor;
            f.Primitor = mag;
            return f;
        }
        FacturaIesire Vanzare(string sufix, DateOnly data) {
            var f = os.CreateObject<FacturaIesire>();
            // Numărul se pre-completează: FCL are politică de numerotare, iar
            // `AsignaNumar` onorează numărul cules (55a) — cu seria fiscală,
            // curățenia (care caută marcajul în `Numar`) l-ar fi lăsat în urmă.
            f.Numar = MarcajD3 + sufix;
            f.Data = data;
            f.Predator = unitate;
            f.Primitor = client;
            return f;
        }
        void LinieC(FacturaIntrare f, TipTva tva, decimal pret) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = f; d.TipMaterial = tip628; d.Cantitate = 1m; d.PretUnitar = pret; d.TipTva = tva;
        }
        void LinieV(FacturaIesire f, TipTva tva, decimal pret) {
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = f; d.TipMaterial = tip704; d.Cantitate = 1m; d.PretUnitar = pret; d.TipTva = tva;
        }

        // --- MAI: numai operațiuni MAPATE, câte una pe fiecare pereche a tabelului
        //     D3-D2, cu sume distincte ca fiecare rând să fie recunoscut la cifră ---
        var fctMai = Cumparare("-C1", new DateOnly(2026, 5, 10));
        LinieC(fctMai, n21, 1000m);    // → rd. 24     (1000 / 210)
        LinieC(fctMai, n11, 500m);     // → rd. 25     ( 500 /  55)
        LinieC(fctMai, ti21, 400m);    // → rd. 12.1   ( 400 /  84), oglindă 26.1
        LinieC(fctMai, n19, 300m);     // → rd. 33     ( 300 /  57)
        LinieC(fctMai, ti19, 200m);    // → rd. 16 ȘI rd. 33 (200 / 38) — cazul „n rânduri"
        LinieC(fctMai, ned21, 100m);   // → rd. 24, capitalizat: brut 121 ⇒ (100 / 21)
        LinieC(fctMai, sdd, 70m);      // → rd. 29     (  70 /   —)
        LinieC(fctMai, sfd, 60m);      // → rd. 29     (  60 /   —)
        LinieC(fctMai, nim, 50m);      // → rd. 29     (  50 /   —)
        var fclMai = Vanzare("-V1", new DateOnly(2026, 5, 12));
        LinieV(fclMai, n21, 2000m);    // → rd. 9      (2000 / 420)
        LinieV(fclMai, n11, 800m);     // → rd. 10     ( 800 /  88)
        LinieV(fclMai, n9, 600m);      // → rd. 11     ( 600 /  54)
        LinieV(fclMai, n19, 400m);     // → rd. 16     ( 400 /  76)
        LinieV(fclMai, sdd, 150m);     // → rd. 14     ( 150 /   —)
        LinieV(fclMai, sfd, 120m);     // → rd. 15     ( 120 /   —)
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctMai);
        MotorOperare.Opereaza(os, fclMai);
        os.CommitChanges();

        // ══════════ D3-V4 (D3-D5): rezultatul decontului == nota ITV a lunii ══════════
        // Generată ÎNAINTE ca luna iunie să existe — nu că ar conta (`Solduri`
        // cumulează până la 31.05), ci ca ordinea probei să fie cea a realității:
        // contabilul închide luna, apoi depune decontul ei.
        var maiDoar = D300Proiectii.D300(os, maiStart, maiEnd, null);
        D300Rand RandMai(string cod) => maiDoar.Randuri.Single(r => r.Cod == cod);
        var itv = InchidereTvaService.Genereaza(os, 2026, 5, unitate.ID);
        var liniiItv = itv?.Detalii.OfType<NotaContabilaDetaliu>().ToList() ?? [];
        // Liniile se identifică prin CONTURILE POLITICII, nu prin simbol (29): proba
        // unui motor agnostic la plan n-are voie să fie ea însăși legată de plan.
        var linieDePlata = liniiItv.SingleOrDefault(l =>
            l.ContDebitId == contColectata && l.ContCreditId == contDePlata);
        var linieDeRecuperat = liniiItv.SingleOrDefault(l =>
            l.ContDebitId == contDeRecuperat && l.ContCreditId == contDeductibila);
        var linieTransfer = liniiItv.SingleOrDefault(l =>
            l.ContDebitId == contColectata && l.ContCreditId == contDeductibila);
        os.CommitChanges();
        Console.WriteLine($"     MĂSURAT (D3-V4): ITV mai — transfer {linieTransfer?.Valoare ?? 0m:N2}, "
            + $"de plată {linieDePlata?.Valoare ?? 0m:N2}, de recuperat {linieDeRecuperat?.Valoare ?? 0m:N2}; "
            + $"D300 rd. 19 TVA {RandMai("19").Tva:N2}, rd. 35 {RandMai("35").Tva:N2}, "
            + $"rd. 36 {RandMai("36").Tva:N2}, rd. 37 {RandMai("37").Tva:N2}.");
        s.Check("D3-V4 (D3-D5) — CUSĂTURA cu registrul contabil: pe luna scenei, rd. 37 (taxa de plată) == valoarea "
            + "liniei de TVA de plată a notei ITV, iar rd. 36 (suma negativă) == linia de TVA de recuperat, ambele "
            + "citite prin conturile POLITICII, nu prin simbol. Taxarea inversă se anulează în amândouă (4426=4427 "
            + "la ITV, rd. 12.1 vs rd. 26.1 în decont) și nedeductibilul lipsește din amândouă (nepostat contabil, "
            + "exclus din rd. 31): două derivări complet independente, aceeași cifră",
            itv != null && itv.Data == maiEnd
            && RandMai("37").Tva == (linieDePlata?.Valoare ?? 0m)
            && RandMai("36").Tva == (linieDeRecuperat?.Valoare ?? 0m)
            // Anti-vacuitate: pe zero de ambele părți egalitatea ar fi trecut și cu
            // derivarea complet greșită.
            && RandMai("37").Tva > 0m && RandMai("36").Tva == 0m
            // …iar cele două jumătăți ale ITV-ului sunt chiar totalurile decontului:
            // 4427 închis == rd. 19 col. TVA, 4426 închis == rd. 35.
            && (linieTransfer?.Valoare ?? 0m) + (linieDePlata?.Valoare ?? 0m) == RandMai("19").Tva
            && (linieTransfer?.Valoare ?? 0m) + (linieDeRecuperat?.Valoare ?? 0m) == RandMai("35").Tva);

        // --- IUNIE: nemapatele deliberate + perechea operare/storno ---
        var fctIunie = Cumparare("-C2", new DateOnly(2026, 6, 10));
        LinieC(fctIunie, n9, 90m);     // NEMAPAT: formularul 2026 n-are achiziție cu 9%
        var fclIunie = Vanzare("-V2", new DateOnly(2026, 6, 11));
        LinieV(fclIunie, nim, 110m);   // NEMAPAT: în afara sferei TVA, nu se declară
        // TAXARE INVERSĂ PE LIVRARE, acum cu regula F13-D1 în motor: rândul de
        // registru poartă baza și TVA ZERO, fiindcă furnizorul emite fără taxă
        // (art. 331). Rd. 13 („livrări supuse măsurilor de simplificare") are o
        // singură coloană, de bază — și exact atât primește. Deliberat în IUNIE, nu
        // în mai: luna ITV-ului e a cusăturii D3-V4, iar o mișcare de TVA în plus
        // acolo ar fi mutat o probă în alta.
        //
        // Înainte de F13 aici se pierdeau tăcut 63,00 lei de taxă inventată de
        // motor, iar proiecția avea o excepție anume ca să nu strige la fiecare
        // livrare cu taxare inversă (fostul „fix F2"). Excepția a dispărut odată cu
        // cauza; proba de mai jos măsoară că nu mai e nimic de ocolit.
        LinieV(fclIunie, ti21, 300m);  // → rd. 13, bază 300; TVA 0 (F13-D1)
        var fclStorno = Vanzare("-V3", new DateOnly(2026, 6, 15));
        LinieV(fclStorno, n19, 1000m); // → rd. 16, apoi stornat în ACEEAȘI lună
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctIunie);
        MotorOperare.Opereaza(os, fclIunie);
        MotorOperare.Opereaza(os, fclStorno);
        os.CommitChanges();
        MotorOperare.Storneaza(os, fclStorno, new DateOnly(2026, 6, 20));
        os.CommitChanges();

        // ══════════ D3-V2: fiecare rând, la cifră ══════════
        var d3 = D300Proiectii.D300(os, pStart, pEnd, null);
        D300Rand R(string cod) => d3.Randuri.Single(r => r.Cod == cod);
        // Formularul, așa cum a ieșit — tipărit înaintea verificărilor, ca un FAIL
        // să vină cu cifrele lui, nu doar cu numele lui.
        Console.WriteLine($"     MĂSURAT (D3-V2, formularul pe {pStart:MM.yyyy}-{pEnd:MM.yyyy}):");
        foreach (var r in d3.Randuri.Where(r => (r.Baza ?? 0m) != 0m || (r.Tva ?? 0m) != 0m))
            Console.WriteLine($"         rd. {r.Cod,-5} {r.Fel,-11} bază {r.Baza,12:N2}  TVA {r.Tva,12:N2}  "
                + $"n={r.Randuri}  [{r.Surse}]");
        foreach (var n in d3.Nemapate)
            Console.WriteLine($"         NEMAPAT {n.TipTvaCod ?? "(fără cod)"}/{n.Sens} {n.Regim} {n.Cota}% "
                + $"bază {n.Baza:N2} TVA {n.Tva:N2} n={n.Randuri}");
        foreach (var a in d3.Avertismente)
            Console.WriteLine($"         AVERTISMENT: {a}");
        s.Check("D3-V2 forma răspunsului: formularul întreg (55 de poziții), în ordinea `Ordine`, cu sub-rândurile la "
            + "nivelul 1 și felul fiecărui rând pe sârmă ca STRING (57a)",
            d3.Randuri.Count == ContaSeeder.RanduriD300Asteptate
            && d3.Randuri.Select(r => r.Ordine).SequenceEqual(Enumerable.Range(1, d3.Randuri.Count))
            && d3.Randuri.Where(r => r.Cod.Contains('.')).All(r => r.Nivel == 1)
            && d3.Randuri.Where(r => !r.Cod.Contains('.')).All(r => r.Nivel == 0)
            && R("12.1").Fel == "Operatiuni" && R("26.1").Fel == "Oglinda"
            && R("19").Fel == "Total" && R("38").Fel == "Extern"
            && R("9").Sectiune == "Colectata" && R("24").Sectiune == "Deductibila"
            && R("37").Sectiune == "Regularizari");
        s.Check("D3-V2 cotele în vigoare, col. Valoare + col. TVA: rd. 9 = 2000/420 (N21 livrare), rd. 10 = 800/88 "
            + "(N11), rd. 11 = 600/54 (N9 — singurul rând al cotei tranzitorii, DOAR pe livrare)",
            R("9").Baza == 2000m && R("9").Tva == 420m && R("9").Surse == "N21"
            && R("10").Baza == 800m && R("10").Tva == 88m
            && R("11").Baza == 600m && R("11").Tva == 54m);
        s.Check("D3-V2 taxarea inversă internă, cele PATRU poziții ale aceleiași operațiuni: rd. 12.1 = 400/84 din "
            + "mapare, rd. 12 = 12.1 + 12.2 din agregarea „din care”, iar rd. 26/26.1 sunt OGLINZILE lor — copiate "
            + "din cod, nu mapate (a doua mapare ar fi dublat cifra, nu ar fi completat-o)",
            R("12.1").Baza == 400m && R("12.1").Tva == 84m
            && R("12").Baza == 400m && R("12").Tva == 84m
            && R("12.2").Baza == 0m && R("12.2").Tva == 0m
            && R("26").Baza == R("12").Baza && R("26").Tva == R("12").Tva
            && R("26.1").Baza == R("12.1").Baza && R("26.1").Tva == R("12.1").Tva
            // Oglinda nu ADAUGĂ rânduri de registru — ele sunt numărate pe sursă —
            // dar moștenește identitatea care a produs cifra.
            && R("26.1").Randuri == 0 && R("26.1").Surse == "TI21"
            && R("12.1").Randuri == 1);
        s.Check("D3-V2 cotele ISTORICE, cazul care a cerut nomenclatorul în loc de o coloană: rd. 16 = 600/114 "
            + "(N19 livrare 400/76 + TI19 achiziție 200/38) și rd. 33 = 500/95 (N19 achiziție 300/57 + ACELAȘI "
            + "TI19 200/38) — o pereche (TipTva × Sens) așezată pe DOUĂ rânduri deodată",
            R("16").Baza == 600m && R("16").Tva == 114m
            && R("33").Baza == 500m && R("33").Tva == 95m
            && R("16").Surse == "N19, TI19" && R("33").Surse == "N19, TI19");
        s.Check("D3-V2 rândurile FĂRĂ coloană de TVA: rd. 14 = 150 (SDD livrare), rd. 15 = 120 (SFD livrare), "
            + "rd. 29 = 180 (SDD 70 + SFD 60 + NIM 50 pe achiziție) — cu `Tva` NULL, nu 0",
            R("14").Baza == 150m && R("14").Tva == null
            && R("15").Baza == 120m && R("15").Tva == null
            && R("29").Baza == 180m && R("29").Tva == null && R("29").Randuri == 3
            && R("29").Surse == "NIM, SDD, SFD");
        s.Check("D3-V2 nedeductibilul (§4.1): NED21 intră în rd. 24 alături de N21 (1100/231 = 1000/210 + 100/21) și "
            + "deci în rd. 30 — dar rd. 31 îl SCADE: 370 − 21 = 349. Scăderea se face pe `Regim`-ul SNAPSHOT al "
            + "grupului, nu pe cel al nomenclatorului de azi",
            R("24").Baza == 1100m && R("24").Tva == 231m && R("24").Surse == "N21, NED21"
            && R("25").Baza == 500m && R("25").Tva == 55m
            && R("30").Tva == 370m && R("31").Tva == 349m
            && R("31").Tva == R("30").Tva - 21m);
        s.Check("D3-V2 nemapatele, cu cifrele lor (D3-D4): exact două perechi — N9 pe ACHIZIȚIE (formularul 2026 "
            + "n-are rând de achiziție cu 9%) și NIM pe LIVRARE (în afara sferei) — cu regim și cotă SNAPSHOT, "
            + "etichetele join-uite la citire și numărul de rânduri de registru din spate",
            d3.Nemapate.Count == 2
            && d3.Nemapate.Single(n => n.TipTvaCod == "N9") is
                { Sens: "Achizitie", Regim: "Normal", Cota: 9m, Baza: 90m, Tva: 8.1m, Randuri: 1 }
            && d3.Nemapate.Single(n => n.TipTvaCod == "NIM") is
                { Sens: "Livrare", Regim: "Neimpozabil", Cota: 0m, Baza: 110m, Tva: 0m, Randuri: 1 }
            && d3.Nemapate.All(n => n.TipTvaDenumire != null));

        // ══════════ D3-V3 (D3-D4): nimic nu se pierde ══════════
        // Identitatea se scrie la nivel de GRUP, fiindcă la nivel de rând nu poate fi
        // o simplă egalitate: o pereche așezată pe două rânduri (TI19/achiziție →
        // rd. 16 ȘI rd. 33) apare de două ori în suma rândurilor, și e CORECT — sunt
        // două laturi ale aceleiași operațiuni, cerute amândouă de formular. Deci:
        //
        //   Σ rânduri cu mapări DIRECTE + Σ nemapate − Σ multiplicitate + Σ pierdut
        //     == Σ registru pe perioadă,  pe AMBELE coloane.
        //
        // „Multiplicitatea" și „pierdutul" se recalculează aici din agregatul brut și
        // din tabelul de mapare citit din bază — independent de proiecție, altfel
        // proba ar fi doar proiecția comparată cu ea însăși.
        var mapariD3 = os.GetObjectsQuery<MapareD300>().ToList()
            .Select(m => new { m.TipTvaId, m.Sens, m.Rand.Cod, m.Rand.AreBaza, m.Rand.AreTva }).ToList();
        var coduriDirecte = mapariD3.Select(m => m.Cod).Distinct().ToHashSet();
        var sumaRanduriBaza = d3.Randuri.Where(r => coduriDirecte.Contains(r.Cod)).Sum(r => r.Baza ?? 0m);
        var sumaRanduriTva = d3.Randuri.Where(r => coduriDirecte.Contains(r.Cod)).Sum(r => r.Tva ?? 0m);
        var sumaNemapateBaza = d3.Nemapate.Sum(n => n.Baza);
        var sumaNemapateTva = d3.Nemapate.Sum(n => n.Tva);
        var brutCub = CubScena.FapteIntre(os, pStart, pEnd);
        var grupuriCub = brutCub
            .GroupBy(f => new { f.Sens, f.TipTvaId, f.Regim, f.Cota })
            .Select(g => new { g.Key.Sens, g.Key.TipTvaId, Baza = g.Sum(x => x.Baza), Tva = g.Sum(x => x.Tva) })
            .ToList();
        decimal multiplicitateBazaCub = 0m, multiplicitateTvaCub = 0m, pierdutBazaCub = 0m, pierdutTvaCub = 0m;
        var grupuriNemapateCub = 0;
        foreach (var g in grupuriCub) {
            var tinte = mapariD3.Where(m => m.TipTvaId == g.TipTvaId && m.Sens == g.Sens).ToList();
            if (tinte.Count == 0) {
                grupuriNemapateCub++;
                continue;
            }
            var cuBaza = tinte.Count(t => t.AreBaza);
            var cuTvaTinta = tinte.Count(t => t.AreTva);
            multiplicitateBazaCub += Math.Max(cuBaza - 1, 0) * g.Baza;
            multiplicitateTvaCub += Math.Max(cuTvaTinta - 1, 0) * g.Tva;
            if (cuBaza == 0) pierdutBazaCub += g.Baza;
            if (cuTvaTinta == 0) pierdutTvaCub += g.Tva;
        }
        Console.WriteLine($"     MĂSURAT (D3-V3): cub {brutCub.Count} fapte / {grupuriCub.Count} grupuri "
            + $"({grupuriNemapateCub} nemapate); Σ bază {brutCub.Sum(f => f.Baza):N2}, Σ TVA {brutCub.Sum(f => f.Tva):N2}; "
            + $"multiplicitate {multiplicitateBazaCub:N2}/{multiplicitateTvaCub:N2}; "
            + $"pierdut pe coloane absente {pierdutBazaCub:N2}/{pierdutTvaCub:N2}.");
        s.Check("D3-V3 (D3-D4) — NIMIC nu se pierde: Σ rândurilor cu mapări directe + Σ nemapate, minus "
            + "multiplicitatea așezării pe mai multe rânduri, plus ce n-a încăput pe coloane absente, == Σ "
            + "registrului pe perioadă, pe AMBELE coloane. Baza nu pierde nimic, multiplicitatea e exact "
            + "TI19/achiziție (numărat o dată la rd. 16 și o dată la rd. 33), iar pierderea pe coloana TVA e "
            + "ZERO — de la F13-D1 nu mai există taxă calculată pe livrarea în taxare inversă (vezi mai jos)",
            sumaRanduriBaza + sumaNemapateBaza - multiplicitateBazaCub + pierdutBazaCub == brutCub.Sum(f => f.Baza)
            && sumaRanduriTva + sumaNemapateTva - multiplicitateTvaCub + pierdutTvaCub == brutCub.Sum(f => f.Tva)
            && pierdutBazaCub == 0m && pierdutTvaCub == 0m
            && multiplicitateBazaCub == 200m && multiplicitateTvaCub == 38m
            && grupuriNemapateCub == d3.Nemapate.Count && grupuriNemapateCub == 2
            && d3.Avertismente.Count == 0);

        // ══════ F13-D1: pe rd. 13 nu mai e nimic de ocolit ══════
        //
        // Locul unde stătea excepția „F2". Cât timp `TvaService` calcula taxa pe
        // REGIM, indiferent de latură, o livrare în taxare inversă producea un TVA
        // pe care formularul nu-l cere (rd. 13, „livrări supuse măsurilor de
        // simplificare", are o singură coloană, de bază) — iar proiecția îl ocolea
        // deliberat, ca să nu strige la fiecare astfel de livrare. Excepția era o
        // cârpă peste un defect de MOTOR, marcată ca restanța D3-r4.
        //
        // F13-D1 a rezolvat cauza: TI × Colectat ⇒ `ValoareTva = 0` și niciun rând
        // contabil, deci rândul de registru intră în proiecție cu taxă zero.
        // Proba e discriminantă: rd. 13 poartă baza întreagă cu `Tva` NULL (coloana
        // chiar nu există pe rând), NU există avertisment — dar acum fiindcă n-a mai
        // rămas nicio cifră pe dinafară (`pierdutTva == 0` mai sus, recalculat
        // independent de proiecție), nu fiindcă proiecția tace. Un TVA care n-ar
        // încăpea pe o coloană absentă redevine ce trebuie să fie: defect de mapare,
        // raportat — vezi proba de mai jos, pe rd. 14.
        var randTi = d3.Randuri.Single(r => r.Cod == "13");
        var registruTiCub = brutCub.Where(f => f.Regim == RegimTva.TaxareInversa && f.Sens == SensTva.Livrare).ToList();
        s.Check("D3-V2/V3 (F13-D1) taxarea inversă pe LIVRARE: rândul de registru poartă bază 300,00 și TVA 0,00 "
            + "(motorul nu mai inventează taxa furnizorului), rd. 13 primește baza cu `Tva` NULL — coloana nu "
            + "există pe rând — și NU se pierde nicio cifră pe drum, deci niciun avertisment; excepția din "
            + "`D300Proiectii` a dispărut odată cu cauza ei",
            registruTiCub.Count == 1 && registruTiCub[0].Baza == 300m && registruTiCub[0].Tva == 0m
            && randTi is { Baza: 300m, Tva: null, Randuri: 1 } && randTi.Surse == "TI21"
            && !d3.Avertismente.Any(a => a.StartsWith("rd. 13"))
            && d3.Avertismente.Count == 0);

        // ── Pierderea TĂCUTĂ, provocată deliberat (riscul 4 din design) ──
        // O mapare greșită (N21 pe rd. 14, care n-are coloană de TVA) e exact forma
        // de defect pe care felia trebuie s-o facă IMPOSIBIL de ratat: decontul ar fi
        // ieșit mai mic decât registrul, fără nicio urmă. Proba trăiește lângă
        // contract și se șterge după ea.
        var rand14 = os.FirstOrDefault<RandD300>(r => r.Cod == "14");
        var mapareProba = os.CreateObject<MapareD300>();
        mapareProba.TipTva = n21;
        mapareProba.Sens = SensTva.Livrare;
        mapareProba.Rand = rand14;
        os.CommitChanges();
        var cuProba = D300Proiectii.D300(os, pStart, pEnd, null);
        var avertismentProba = cuProba.Avertismente
            .FirstOrDefault(a => a.StartsWith("rd. 14 a primit TVA"));
        // Purjă FIZICĂ (F13-D2): obiectul probei e proiecția, nu ștergerea.
        new Purja(os).Adauga(mapareProba).Executa();
        var dupaProba = D300Proiectii.D300(os, pStart, pEnd, null);
        s.Check("D3-V3, riscul 4 — un gard care tace devine capcană (62f): mapat pe rd. 14 (rând FĂRĂ coloană de "
            + "TVA), N21-livrare își pierde cei 420,00 lei de taxă. Proiecția NU-i strecoară nicăieri și NU tace: "
            + "iese un avertisment cu rândul și suma, iar baza se adună normal (2150 = 150 + 2000). După ștergerea "
            + "mapării de probă, formularul revine EXACT la cifrele dinainte",
            avertismentProba != null && avertismentProba.Contains("420,00")
            && cuProba.Randuri.Single(r => r.Cod == "14") is { Baza: 2150m, Tva: null }
            && dupaProba.Avertismente.Count == 0
            && dupaProba.Randuri.Single(r => r.Cod == "14").Baza == 150m
            && dupaProba.Randuri.Select(r => (r.Cod, r.Baza, r.Tva))
                .SequenceEqual(d3.Randuri.Select(r => (r.Cod, r.Baza, r.Tva))));

        // ══════════ D3-V5: formulele, recalculate independent ══════════
        // Externii NENULI, ca rd. 40/43/44/45 să nu fie probate pe zero — și cu
        // rd. 41 = 0, fiindcă perechea 38/41 e mutual exclusivă (refuzul e la
        // controller, probat HTTP; aici se verifică doar că proiecția pune fiecare
        // parametru pe rândul lui).
        var externi = new ParametriD300(SoldPlataPrecedent: 100m, DiferentePlata: 7m,
            SoldNegativPrecedent: 0m, DiferenteNegative: 3m);
        var cuExterni = D300Proiectii.D300(os, pStart, pEnd, externi);
        D300Rand E(string cod) => cuExterni.Randuri.Single(r => r.Cod == cod);
        decimal T(string cod) => E(cod).Tva ?? 0m;
        decimal B(string cod) => E(cod).Baza ?? 0m;
        // Operanzii, exact cum îi numește ordinul — rescriși AICI, nu importați din
        // proiecție: o probă care refolosește lista pe care o verifică nu probează
        // nimic. Sub-rândurile „din care" lipsesc din amândouă (sunt în părinți).
        var op19 = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18" };
        var op30 = new[] { "20", "21", "22", "23", "24", "25", "26", "27", "28" };
        s.Check("D3-V5 totalurile de secțiune, recalculate independent: rd. 19 = Σ rd. 1-18 fără sub-rânduri (pe "
            + "fiecare coloană, numai operanzii care CHIAR au coloana), rd. 30 = Σ rd. 20-28 — rd. 29 rămâne "
            + "informativ și NU intră, iar 12.1/12.2/26.1/26.2 nu se adună peste părinții lor (dubla numărare)",
            B("19") == op19.Where(c => E(c).Baza != null).Sum(B)
            && T("19") == op19.Where(c => E(c).Tva != null).Sum(T)
            && B("30") == op30.Where(c => E(c).Baza != null).Sum(B)
            && T("30") == op30.Where(c => E(c).Tva != null).Sum(T)
            && T("19") == 760m && T("30") == 370m && B("30") == 2000m);
        s.Check("D3-V5 rd. 31 (§4.1): taxa DEDUSĂ = taxa DEDUCTIBILĂ minus TVA-ul fără drept de deducere așezat în "
            + "secțiunea deductibilă — și rămâne sub rd. 30, cum cere validarea blocantă a formularului",
            T("31") == T("30") - 21m && T("31") <= T("30") && T("31") == 349m);
        s.Check("D3-V5 rd. 35 = rd. 31 + 32 + 33 + 34, iar perechea 36/37 e mutual exclusivă prin `max(…, 0)`: "
            + "exact una dintre ele e nenulă",
            T("35") == T("31") + T("32") + T("33") + T("34")
            && T("36") == Math.Max(T("35") - T("19"), 0m)
            && T("37") == Math.Max(T("19") - T("35"), 0m)
            && (T("36") == 0m) != (T("37") == 0m));
        s.Check("D3-V5 regularizările art. 303, cu externii pe rândurile lor: rd. 38/39/41/42 poartă EXACT parametrii "
            + "primiți, rd. 40 = 37 + 38 + 39, rd. 43 = 36 + 41 + 42, iar 44/45 sunt a doua pereche exclusivă",
            T("38") == 100m && T("39") == 7m && T("41") == 0m && T("42") == 3m
            && T("40") == T("37") + T("38") + T("39")
            && T("43") == T("36") + T("41") + T("42")
            && T("44") == Math.Max(T("40") - T("43"), 0m)
            && T("45") == Math.Max(T("43") - T("40"), 0m)
            && T("44") > 0m && T("45") == 0m);
        s.Check("D3-V5 rândurile fără sursă în model rămân prezente și zero (27/28 agricultori, 32 restituiri, "
            + "34 pro-rata — 36f): un formular din care lipsesc rânduri nu mai e formularul",
            new[] { "27", "28", "32", "34" }.All(c => E(c).Fel == "Extern" && E(c).Tva == 0m && E(c).Baza == null)
            // …iar externii NU se strecoară în cifrele operațiunilor: rândurile
            // rămân identice cu proiecția fără parametri.
            && cuExterni.Randuri.Where(r => r.Fel == "Operatiuni")
                .Select(r => (r.Cod, r.Baza, r.Tva))
                .SequenceEqual(d3.Randuri.Where(r => r.Fel == "Operatiuni").Select(r => (r.Cod, r.Baza, r.Tva))));

        // ══════ F3: rd. 31 se scade PE APARIȚIE în rd. 30, nu pe grup ══════
        //
        // Prima formă a scăderii întreba „a ajuns grupul în secțiunea Deductibila?".
        // Criteriul suna corect și greșea pe două căi OPUSE, pe care cele două probe
        // de mai jos le izolează una câte una — cu maparea de probă creată și ștearsă
        // în scenă, ca cea din D3-V3, ca politica de profil să rămână neatinsă.
        //
        // Ambele probe folosesc NED21 (singurul `Capitalizat` din scenă, cu 21,00 lei
        // de TVA nedeductibil pe achiziția de 100). Un `TipTva` nou, fără rânduri de
        // registru, ar fi făcut probele VACUE: nimic de scăzut, egalitate trivială.
        MapareD300 ProbaNed(string codRand) {
            var m = os.CreateObject<MapareD300>();
            m.TipTva = ned21;
            m.Sens = SensTva.Achizitie;
            m.Rand = os.FirstOrDefault<RandD300>(r => r.Cod == codRand);
            os.CommitChanges();
            return m;
        }
        // Ca mai sus: artefact de scenă ⇒ purjă fizică, ca politica de profil să
        // rămână neatinsă ȘI ca tabela să nu adune mapări marcate șterse (F13-D2).
        void StergeProba(MapareD300 m) => new Purja(os).Adauga(m).Executa();

        // (a) Țintă în secțiunea Deductibila, dar care NU e operand al rd. 30:
        //     rd. 33 „Regularizări taxă dedusă". Cifra n-a intrat niciodată în
        //     rd. 30, deci nu are ce să iasă din rd. 31. Criteriul „pe secțiune" ar
        //     fi scăzut-o a doua oară (42 în loc de 21) și ar fi coborât taxa dedusă
        //     sub adevăr, adică ar fi declarat mai puțin TVA de recuperat.
        var probaRd33 = ProbaNed("33");
        var cu33 = D300Proiectii.D300(os, pStart, pEnd, null);
        D300Rand A(string cod) => cu33.Randuri.Single(r => r.Cod == cod);
        StergeProba(probaRd33);
        Console.WriteLine($"     MĂSURAT (F3a): cu NED21 mapat în plus pe rd. 33 — rd. 30 {A("30").Tva:N2}, "
            + $"rd. 31 {A("31").Tva:N2}, rd. 33 {A("33").Tva:N2}, rd. 35 {A("35").Tva:N2}.");
        s.Check("D3-V5 (F3a) o țintă din secțiunea Deductibilă care NU e operand al rd. 30 (rd. 33 — regularizări "
            + "taxă dedusă) nu produce nicio scădere în plus: rd. 30 rămâne 370,00, rd. 31 rămâne 349,00 = 370 − "
            + "21, iar cei 21,00 se văd în rd. 33 (95 → 116) și de acolo în rd. 35",
            A("30").Tva == 370m && A("31").Tva == 349m
            && A("33").Baza == 600m && A("33").Tva == 116m
            && A("35").Tva == A("31").Tva + A("33").Tva);

        // (b) Două ținte care sunt AMÂNDOUĂ operanzi ai rd. 30 (rd. 24 și rd. 25):
        //     cifra intră de două ori în rd. 30, deci trebuie scoasă de două ori din
        //     rd. 31. Criteriul „o dată per grup" scădea 21 în loc de 42 și lăsa
        //     rd. 31 la 370 — TVA dedus pe o taxă fără drept de deducere, adică
        //     exact ce interzice §4.1.
        var probaRd25 = ProbaNed("25");
        var cu25 = D300Proiectii.D300(os, pStart, pEnd, null);
        D300Rand C(string cod) => cu25.Randuri.Single(r => r.Cod == cod);
        StergeProba(probaRd25);
        Console.WriteLine($"     MĂSURAT (F3b): cu NED21 mapat în plus pe rd. 25 — rd. 24 {C("24").Tva:N2}, "
            + $"rd. 25 {C("25").Tva:N2}, rd. 30 {C("30").Tva:N2}, rd. 31 {C("31").Tva:N2}.");
        s.Check("D3-V5 (F3b) o pereche `Capitalizat` așezată pe DOUĂ rânduri operanzi ai rd. 30 se scade de DOUĂ "
            + "ori: rd. 25 urcă la 76,00 (55 + 21), rd. 30 la 391,00, iar rd. 31 rămâne 349,00 = 391 − 2×21. "
            + "Scăderea urmărește câte apariții a avut cifra în rd. 30, nu câte grupuri au fost",
            C("25").Baza == 600m && C("25").Tva == 76m
            && C("30").Tva == 391m && C("31").Tva == 391m - 2 * 21m && C("31").Tva == 349m);

        // …și după ambele probe formularul revine EXACT la cifrele dinainte.
        var dupaProbeNed = D300Proiectii.D300(os, pStart, pEnd, null);
        s.Check("D3-V5 (F3) după ștergerea celor două mapări de probă, formularul revine bit cu bit la cifrele "
            + "profilului — politica de seed a rămas neatinsă (22 mapări vii)",
            dupaProbeNed.Randuri.Select(r => (r.Cod, r.Baza, r.Tva))
                .SequenceEqual(d3.Randuri.Select(r => (r.Cod, r.Baza, r.Tva)))
            && dupaProbeNed.Avertismente.Count == 0
            && os.GetObjectsQuery<MapareD300>().Count() == 22);

        // ══════════ D3-V6: storno în aceeași perioadă ══════════
        var iunie = D300Proiectii.D300(os, iunieStart, iunieEnd, null);
        D300Rand I(string cod) => iunie.Randuri.Single(r => r.Cod == cod);
        s.Check("D3-V6 storno în ACEEAȘI perioadă: operarea și stornarea unei livrări N19 de 1000/190 se netează pe "
            + "rd. 16 la 0 — dar rândul NU dispare, iar `Randuri` = 2 arată că în spate stau două fapte fiscale "
            + "distincte. Storno-ul cade pe rândul cotei ISTORICE prin `TipTvaId`-ul lui snapshot, nu prin vreun "
            + "mecanism propriu: proiecția nu știe că e storno",
            I("16").Baza == 0m && I("16").Tva == 0m && I("16").Randuri == 2 && I("16").Surse == "N19"
            // …iar peste perioada combinată netarea nu schimbă nimic: mai rămâne mai.
            && R("16").Baza == 600m && R("16").Tva == 114m
            // Iunie n-are decât nemapatele și perechea stornată ⇒ toate totalurile 0.
            && I("19").Tva == 0m && I("35").Tva == 0m && I("36").Tva == 0m && I("37").Tva == 0m
            && iunie.Nemapate.Count == 2);

        // ══════ F1: validările BLOCANTE ale formularului, ca avertismente ══════
        //
        // A TREIA lună a scenei, iulie, există pentru un singur fapt: stornarea
        // facturii de MAI, adică a celei care poartă NED21. Un storno într-o lună
        // ULTERIOARĂ operării aduce în perioada lui o achiziție fără drept de
        // deducere cu semn NEGATIV — deci un „nedeductibil" negativ, iar scăderea
        // de la rd. 31 devine adunare: rd. 31 IESE PESTE rd. 30, ceea ce validarea
        // V_6 a formularului refuză categoric.
        //
        // Cifrele sunt CORECTE (taxa dedusă chiar crește: se anulează o
        // nedeductibilitate declarată luna trecută), dar formularul nu are cum să le
        // exprime. Deci: NU se trunchiază — se STRIGĂ. Fără avertisment, contabilul
        // ar fi aflat de la DUKIntegrator, cu un cod și fără nicio cauză.
        //
        // Fără ITV pe iulie, deliberat: cusătura D3-D5 e a lunii mai, iar o închidere
        // în plus ar fi amestecat două probe.
        MotorOperare.Storneaza(os, fctMai, new DateOnly(2026, 7, 15));
        os.CommitChanges();
        var iulie = D300Proiectii.D300(os, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31), null);
        D300Rand J(string cod) => iulie.Randuri.Single(r => r.Cod == cod);
        // Mai și iunie se RECITESC după storno (rândurile lui poartă data stornării,
        // 25d — deci n-au ce căuta în lunile dinainte; proba o măsoară, n-o presupune).
        var maiDupaStorno = D300Proiectii.D300(os, maiStart, maiEnd, null);
        var iunieDupaStorno = D300Proiectii.D300(os, iunieStart, iunieEnd, null);
        const string PrefixV6 = "rd. 31 (";
        Console.WriteLine($"     MĂSURAT (F1, iulie — storno al facturii de mai): rd. 24 {J("24").Tva:N2}, "
            + $"rd. 30 {J("30").Tva:N2}, rd. 31 {J("31").Tva:N2}; avertismente: "
            + $"{(iulie.Avertismente.Count == 0 ? "niciunul" : string.Join(" | ", iulie.Avertismente))}");
        s.Check("D3-V5 (F1) validarea blocantă V_6 (rd. 31 ≤ rd. 30) e MĂSURATĂ pe cifrele noastre: stornarea în "
            + "iulie a facturii de mai aduce nedeductibilul cu semn negativ (rd. 24 = −231,00), deci rd. 31 "
            + "(−349,00) DEPĂȘEȘTE rd. 30 (−370,00). Proiecția nu trunchiază și nu normalizează — scoate un "
            + "avertisment care numește ambele cifre, validarea și cauza tipică",
            J("24").Baza == -1100m && J("24").Tva == -231m
            && J("30").Tva == -370m && J("31").Tva == -349m && J("31").Tva > J("30").Tva
            && iulie.Avertismente.SingleOrDefault(a => a.StartsWith(PrefixV6)) is { } v6
            && v6.Contains("V_6") && v6.Contains("-349,00") && v6.Contains("-370,00")
            && v6.Contains("storno"));
        s.Check("D3-V5 (F1) avertismentul V_6 e SPECIFIC lunii care rupe inegalitatea: pe mai (rd. 31 = 349 ≤ "
            + "rd. 30 = 370) și pe iunie (ambele 0) NU apare — un gard care ar striga mereu s-ar ignora la fel "
            + "de repede ca unul care tace",
            !maiDupaStorno.Avertismente.Any(a => a.StartsWith(PrefixV6))
            && !iunieDupaStorno.Avertismente.Any(a => a.StartsWith(PrefixV6))
            && maiDupaStorno.Randuri.Single(r => r.Cod == "31").Tva == 349m
            && maiDupaStorno.Randuri.Single(r => r.Cod == "30").Tva == 370m
            // …și niciuna dintre cele trei luni nu rupe vreo altă inegalitate a
            // formularului: ierarhia „din care" și oglinzile sunt egalități prin
            // construcție, iar proba le măsoară pe rezultat, nu pe intenția codului.
            && new[] { iulie, maiDupaStorno, iunieDupaStorno }
                .All(d => !d.Avertismente.Any(a => a.Contains("sub suma sub-rândurilor")
                    || a.Contains("trebuie să fie EGAL"))));

        // ---------------- Curățenie ----------------
        var idItv = itv.ID;
        CurataD3();
        s.Check("Curățenie finală felia D300 (fără reziduuri e2e — inclusiv nota de închidere, al cărei singur fir "
            + "e unitatea internă marcată)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajD3))
            && !os.GetObjectsQuery<Document>().Any(d => d.Numar.StartsWith(MarcajD3))
            && !os.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItv)
            && CubScena.FapteIntre(os, pStart, new DateOnly(2026, 7, 31)).Count == 0
            && os.GetObjectsQuery<MapareD300>().Count() == 22);
    }
}
