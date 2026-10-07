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

// ============ Felia 18, pas 2: D18-D2 — ieșirea care GOLEȘTE cheia preia restul (D18-V2) ============
// Lotul de 3 bucăți intrat cu 30,02 (3 × 10,005 rotunjit la bani) are prețul
// 10,006667; trei ieșiri de câte 1 la `preț × cantitate` ar scoate 10,01 + 10,01
// + 10,01 = 30,03 și ar lăsa lotul cu 0 bucăți și −0,01 lei (reziduul 74g).
// Regula D18-D2: ieșirea care golește cheia ia tot soldul valoric rămas — a
// treia iese cu 10,00, lotul ajunge exact la 0/0,00. Pe AMBELE profiluri
// (BCS/BTR/LDI există pe amândouă); DSC, RLF și ASM doar pe privat.
static class VerificaValoareIesire {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-D18";
        using var os = s.Provider.CreateObjectSpace();
        var codTip = privat ? "371" : "302.01.00";
        var tipMat = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTip);
        var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var n21 = privat ? os.FirstOrDefault<TipTva>(t => t.Cod == "N21") : null;

        void Curata() {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).Distinct().ToList();
            var loturi = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => ids.Contains(i.DocumentId) || ids.Contains(i.DocumentStingatorId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
                         .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(e => e.Cod == Marcaj + "-CE").ToList());
            pj.Executa();
        }
        Curata();

        // ---------------- Scena ----------------
        var g1 = os.CreateObject<Gestiune>();
        g1.Cod = Marcaj + "-G1"; g1.Denumire = "Gestiune D18 sursă";
        var g2 = os.CreateObject<Gestiune>();
        g2.Cod = Marcaj + "-G2"; g2.Denumire = "Gestiune D18 destinație";
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = Marcaj + "-LOC"; loc.Denumire = "Loc de consum D18"; loc.Calitati = CalitateRepartitor.LocConsum;
        var comisie = os.CreateObject<UnitateInterna>();
        comisie.Cod = Marcaj + "-COM"; comisie.Denumire = "Comisie D18"; comisie.Calitati = CalitateRepartitor.Comisie;
        Produs Prod(string sufix) {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + sufix; p.Denumire = "Produs D18" + sufix; p.UM = "BUC"; p.TipMaterial = tipMat;
            return p;
        }
        var produs = Prod("-P");
        var dataLot = new DateOnly(2026, 1, 15);
        // Lot cu preț „strâmb": valoarea de intrare e rotunjită la bani, prețul la
        // 6 zecimale (26e) — exact cum îl lasă motorul după un NIR de 3 × 10,005.
        Lot LotNou(Produs p, Gestiune g, decimal cantitate, decimal valoare) {
            var furnizor = os.FirstOrDefault<Partener>(r => r.Cod == Marcaj + "-FURN-STOC");
            if (furnizor == null) {
                furnizor = os.CreateObject<Partener>(); furnizor.Cod = Marcaj + "-FURN-STOC";
                furnizor.Denumire = furnizor.Cod;
            }
            var economic = os.FirstOrDefault<CodEconomic>(e => e.Cod == Marcaj + "-CE");
            if (!privat && economic == null) {
                economic = os.CreateObject<CodEconomic>(); economic.Cod = Marcaj + "-CE";
                economic.Denumire = economic.Cod;
            }
            var factura = os.CreateObject<FacturaIntrare>(); factura.Data = dataLot;
            factura.Numar = Marcaj + "-FCT-" + Guid.NewGuid().ToString("N");
            factura.Predator = furnizor; factura.Primitor = g;
            var linie = os.CreateObject<FacturaIntrareDetaliu>(); linie.Document = factura;
            linie.TipMaterial = tipMat; linie.Cantitate = cantitate;
            linie.PretUnitar = Scara.RotunjestePret(valoare / cantitate); linie.CodEconomicId = economic?.ID;
            var lot = linie.CreeazaLot(os, p, g);
            os.CommitChanges();
            var nir = MotorOperare.Opereaza(os, factura);
            if (nir != null) MotorOperare.Opereaza(os, nir);
            return lot;
        }
        var lotA = LotNou(produs, g1, 3m, 30.02m);
        var lotB = LotNou(produs, g1, 3m, 30.02m);
        var lotE = LotNou(produs, g1, 3m, 30.02m);
        os.CommitChanges();

        (decimal Cantitate, decimal Valoare) SoldCheieCub(Lot lot, Repartitor r) => CubScena.SoldCheie(os, lot.ID, r.ID);
        BonConsum Bcs(Lot lot, DateOnly data, params decimal[] cantitati) {
            var doc = os.CreateObject<BonConsum>();
            doc.Data = data; doc.Predator = g1; doc.Primitor = loc;
            foreach (var q in cantitati) {
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc; d.TipMaterial = tipMat; d.Lot = lot; d.Cantitate = q;
            }
            os.CommitChanges();
            return doc;
        }
        // Lotul „cu rest": după două ieșiri de câte 1 (10,01 + 10,01) rămâne 1 bucată
        // și 10,00 lei, iar `preț × 1` ar scoate 10,01. (O ieșire de 3 dintr-odată nu
        // ar arăta nimic: 3 × 10,006667 = 30,020001 se rotunjește tot la 30,02 —
        // reziduul se naște din ACUMULAREA rotunjirilor, nu dintr-o singură ieșire.)
        Lot LotCuRest(Produs p) {
            var lot = LotNou(p, g1, 3m, 30.02m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, Bcs(lot, new DateOnly(2026, 5, 1), 1m));
            MotorOperare.Opereaza(os, Bcs(lot, new DateOnly(2026, 5, 2), 1m));
            os.CommitChanges();
            return lot;
        }
        s.Check("D18-V2 premisă: lotul de 3 × 30,02 are prețul 10,006667 (26e), `preț × 1` rotunjit la bani dă 10,01, iar "
            + "`preț × 3` dă tot 30,02 — reziduul e al ACUMULĂRII rotunjirilor",
            lotA.PretUnitar == 10.006667m && Scara.RotunjesteBani(1m * lotA.PretUnitar) == 10.01m
            && Scara.RotunjesteBani(3m * lotA.PretUnitar) == 30.02m);

        // ---------------- (a) 1 + 1 + 1: primele două la preț, a treia ia restul ----------------
        var bcs1 = Bcs(lotA, new DateOnly(2026, 5, 1), 1m);
        MotorOperare.Opereaza(os, bcs1);
        var bcs2 = Bcs(lotA, new DateOnly(2026, 5, 2), 1m);
        MotorOperare.Opereaza(os, bcs2);
        os.CommitChanges();
        var soldDupa2 = SoldCheieCub(lotA, g1);
        Console.WriteLine($"     MĂSURAT (D18-V2 a): BCS1 {bcs1.Detalii.Single().Valoare:N2}, BCS2 {bcs2.Detalii.Single().Valoare:N2}; "
            + $"lotul după două ieșiri: {soldDupa2.Cantitate:0.###} / {soldDupa2.Valoare:N2}.");
        s.Check("D18-V2 (a) ieșirile care NU golesc rămân la `preț × cantitate`: 10,01 și 10,01, lotul rămâne cu 1 bucată "
            + "și 10,00 lei (30,02 − 20,02)",
            bcs1.Detalii.Single().Valoare == 10.01m && bcs2.Detalii.Single().Valoare == 10.01m
            && soldDupa2 == (1m, 10.00m));

        var bcs3 = Bcs(lotA, new DateOnly(2026, 5, 3), 1m);
        var dryRun = MotorOperare.Valideaza(os, bcs3);
        var valoareDryRun = bcs3.Detalii.Single().Valoare;
        MotorOperare.Opereaza(os, bcs3);
        os.CommitChanges();
        var randuri3 = CubScena.Stoc(os, bcs3.ID);
        var soldDupa3 = SoldCheieCub(lotA, g1);
        var nota3Cub = CubScena.Note(os, bcs3.ID);
        var dto3 = BonConsumApply.Citeste(os, bcs3.ID);
        Console.WriteLine($"     MĂSURAT (D18-V2 a, golire): dry-run {valoareDryRun:N2} ({dryRun.Count} erori), operat "
            + $"{bcs3.Detalii.Single().Valoare:N2}; rânduri {string.Join(" | ", randuri3.Select(r => $"{r.Cantitate:0.###}/{r.Semnata:N2}"))}; "
            + $"lot {soldDupa3.Cantitate:0.###}/{soldDupa3.Valoare:N2}; nota {nota3Cub.Where(p => p.Debit).Sum(p => p.Valoare):N2}; ReadDto {dto3.Linii.Single().Valoare:N2}.");
        s.Check("D18-V2 (a) a treia ieșire GOLEȘTE lotul: valoarea liniei = 10,00 (soldul valoric rămas, nu 10,01), lotul "
            + "ajunge exact la 0 / 0,00, iar rândul de consum poartă aceiași 10,00 (restul se MUTĂ, nu se pierde)",
            bcs3.Detalii.Single().Valoare == 10.00m && soldDupa3 == (0m, 0m)
            && randuri3.Any(r => r.Gestiune == g1.ID && r.Cantitate == -1m && r.Semnata == -10.00m)
            && randuri3.Any(r => r.Gestiune == loc.ID && r.Cantitate == 1m && r.Semnata == 10.00m));
        s.Check("D18-V2 (a) dry-run == operare: `Valideaza` trece prin ACELAȘI calcul (33d) și lasă pe linie 10,00, "
            + "exact ce materializează `Opereaza`",
            dryRun.Count == 0 && valoareDryRun == 10.01m && bcs3.Detalii.Single().Valoare == 10.00m);
        s.Check("D18-V2 (a) contarea consumului (6xx = 3xx) postează valoarea materializată, 10,00 — cenții de rotunjire "
            + "ai intrării rămân pe cheltuială, contul de stoc ajunge exact la 0 pe lot",
            nota3Cub.Count == 2 && nota3Cub.All(p => p.Valoare == 10.00m)
            && nota3Cub.Count(p => p.Debit) == 1 && nota3Cub.Count(p => p.Credit) == 1);
        s.Check("D18-V2 (a) ReadDto după operare arată valoarea MATERIALIZATĂ (10,00), nu previzualizarea de la culegere "
            + "(10,01) — afișarea nu minte",
            dto3.Linii.Single().Valoare == 10.00m && dto3.Total == 10.00m);

        // Anulare + re-operare: rândurile șterse nu se numără de două ori, valoarea e aceeași.
        MotorOperare.AnuleazaOperarea(os, bcs3);
        var soldAnulat = SoldCheieCub(lotA, g1);
        MotorOperare.Opereaza(os, bcs3);
        os.CommitChanges();
        var randuriReoperat = CubScena.Stoc(os, bcs3.ID);
        Console.WriteLine($"     MĂSURAT (D18-V2 a, anulare): după anulare lot {soldAnulat.Cantitate:0.###}/{soldAnulat.Valoare:N2}; "
            + $"re-operat {bcs3.Detalii.Single().Valoare:N2}, {randuriReoperat.Count} rânduri.");
        s.Check("D18-V2 (a) anulare + re-operare: soldul revine la 1 / 10,00, re-operarea scrie din nou 10,00 și exact "
            + "2 rânduri (rândurile șterse ale aceluiași document nu intră în sold)",
            soldAnulat == (1m, 10.00m) && bcs3.Detalii.Single().Valoare == 10.00m
            && randuriReoperat.Count == 2 && SoldCheieCub(lotA, g1) == (0m, 0m));

        // Storno: rânduri inverse IDENTICE (valoarea absorbită se întoarce ca atare).
        MotorOperare.Storneaza(os, bcs3, new DateOnly(2026, 5, 20));
        os.CommitChanges();
        var originaleCub = CubScena.Stoc(os, bcs3.ID).Where(p => !p.Storno).ToList();
        var inverseCub = CubScena.Stoc(os, bcs3.ID).Where(p => p.Storno).ToList();
        s.Check("D18-V2 (a) stornoul liniei absorbante = rânduri inverse identice (−(−10,00) pe magazie, −10,00 pe consum), "
            + "lotul revine la 1 / 10,00",
            originaleCub.Count == 2 && inverseCub.Count == 2
            && originaleCub.All(o => inverseCub.Any(i => i.Unitate == o.Unitate && i.Gestiune == o.Gestiune
                && i.Cont == o.Cont && i.Cantitate == -o.Cantitate && i.Semnata == -o.Semnata))
            && SoldCheieCub(lotA, g1) == (1m, 10.00m));

        // ---------------- (b) două linii pe aceeași cheie într-un document ----------------
        var bcsTrei = Bcs(lotB, new DateOnly(2026, 5, 4), 1m, 1m, 1m);
        MotorOperare.Opereaza(os, bcsTrei);
        os.CommitChanges();
        var liniiTrei = bcsTrei.Detalii.Select(d => d.Valoare).ToList();
        Console.WriteLine($"     MĂSURAT (D18-V2 b): liniile 1 + 1 + 1 pe același lot ⇒ {string.Join(" + ", liniiTrei.Select(v => v.ToString("N2")))}.");
        s.Check("D18-V2 (b) trei linii pe aceeași cheie în ACELAȘI document se acumulează în ordinea liniilor: 10,01, 10,01, "
            + "iar a treia vede soldul de după primele două și ia restul 10,00 — lotul 0 / 0,00",
            liniiTrei.Count == 3 && liniiTrei.Count(v => v == 10.01m) == 2 && liniiTrei.Count(v => v == 10.00m) == 1
            && SoldCheieCub(lotB, g1) == (0m, 0m));

        // ---------------- (c) BTR care golește sursa: destinația poartă restul ----------------
        var lotC = LotCuRest(produs);
        var btr = os.CreateObject<NotaTransfer>();
        btr.Data = new DateOnly(2026, 5, 5); btr.Predator = g1; btr.Primitor = g2; btr.NumarPV = Marcaj;
        var linBtr = os.CreateObject<DocumentDetaliu>();
        linBtr.Document = btr; linBtr.TipMaterial = tipMat; linBtr.Lot = lotC; linBtr.Cantitate = 1m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, btr);
        os.CommitChanges();
        var randuriBtr = CubScena.Stoc(os, btr.ID);
        Console.WriteLine($"     MĂSURAT (D18-V2 c): BTR 1 buc (rest 10,00, preț × 1 = 10,01) ⇒ linia {linBtr.Valoare:N2}; "
            + $"{string.Join(" | ", randuriBtr.Select(r => $"{r.Cantitate:0.###}/{r.Semnata:N2}"))}; "
            + $"sursă {SoldCheieCub(lotC, g1).Valoare:N2}, destinație {SoldCheieCub(lotC, g2).Valoare:N2}.");
        s.Check("D18-V2 (c) BTR care golește sursa: valoarea liniei e comună ambelor laturi, deci restul se MUTĂ — "
            + "sursa −1 / −10,00 (exact 0 după), destinația +1 / +10,00 — nu 10,01 (S1 pe ambele gestiuni: cantitate ȘI valoare)",
            linBtr.Valoare == 10.00m
            && SoldCheieCub(lotC, g1) == (0m, 0m)
            && SoldCheieCub(lotC, g2) == (1m, 10.00m));

        // ---------------- (d) LDI Minus care golește ----------------
        var lotD = LotCuRest(produs);
        var ldi = os.CreateObject<ListaDiferenteInventar>();
        ldi.Data = new DateOnly(2026, 5, 6); ldi.Predator = g1; ldi.Primitor = comisie;
        var linLdi = os.CreateObject<ListaDiferenteInventarDetaliu>();
        linLdi.Document = ldi; linLdi.TipMaterial = tipMat; linLdi.Directie = DirectieDiferenta.Minus;
        linLdi.Lot = lotD; linLdi.Cantitate = 1m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, ldi);
        os.CommitChanges();
        Console.WriteLine($"     MĂSURAT (D18-V2 d): LDI− 1 buc (rest 10,00) ⇒ linia {linLdi.Valoare:N2}, lot {SoldCheieCub(lotD, g1).Valoare:N2}.");
        s.Check("D18-V2 (d) minusul de inventar care golește lotul ia restul CU SEMNUL liniei (convenția LDI: "
            + "valoarea poartă semnul cantității; regula +1 pe predator o scrie ca atare): −10,00, nu −10,01 — lotul 0 / 0,00",
            linLdi.Valoare == -10.00m && SoldCheieCub(lotD, g1) == (0m, 0m));

        // ---------------- (e) ieșire care NU golește: neschimbat ----------------
        var bcsPartial = Bcs(lotE, new DateOnly(2026, 5, 7), 1m);
        MotorOperare.Opereaza(os, bcsPartial);
        os.CommitChanges();
        s.Check("D18-V2 (e) ieșirea care NU golește cheia rămâne exact ca înainte de D2: 10,01 = `RotunjesteBani(1 × "
            + "10,006667)`, lotul 2 / 20,01",
            bcsPartial.Detalii.Single().Valoare == 10.01m && SoldCheieCub(lotE, g1) == (2m, 20.01m));

        // ---------------- (r) RETRO — limita regulii, ca FAPT documentat (review F1) ----------------
        // Lot 2 × 20,01 (preț 10,005). BCS din 10.05 −1 operat PRIMUL: vede 2 ⇒ nu
        // golește ⇒ `round(1 × preț)`, rest 1. BCS RETRO din 05.05 −1 operat DUPĂ:
        // prefix-ul ≤ 05.05 nu vede rândul din 10.05 ⇒ tot 2 ⇒ nu golește ⇒
        // `round(1 × preț)`. Cheia rămâne 0 / (20,01 − 2 × round): golirea e decisă
        // LA OPERARE pe registrul EXISTENT, retro-ul lasă reziduu — vizibil în SAF-T
        // S (`ReziduValoricFaraCantitate`) și clasificat de oracol „re-deschisă retro".
        var produsRetro = Prod("-RETRO");
        var lotRetro = LotNou(produsRetro, g1, 2m, 20.01m);
        os.CommitChanges();
        var unuRetro = Scara.RotunjesteBani(1m * lotRetro.PretUnitar);
        var bcsTarziu = Bcs(lotRetro, new DateOnly(2026, 5, 10), 1m);
        MotorOperare.Opereaza(os, bcsTarziu);
        os.CommitChanges();
        var bcsRetro = Bcs(lotRetro, new DateOnly(2026, 5, 5), 1m);
        MotorOperare.Opereaza(os, bcsRetro);
        os.CommitChanges();
        var soldRetro = SoldCheieCub(lotRetro, g1);
        var soldRetroCub = soldRetro;
        Console.WriteLine($"     MĂSURAT (D18-V2 r): preț {lotRetro.PretUnitar:0.######}, round(1 × preț) {unuRetro:N2}; "
            + $"BCS 10.05 {bcsTarziu.Detalii.Single().Valoare:N2} (operat primul), BCS retro 05.05 {bcsRetro.Detalii.Single().Valoare:N2}; "
            + $"cheia {soldRetro.Cantitate:0.###} / {soldRetro.Valoare:N2}.");
        s.Check("D18-V2 (r) LIMITA regulii (F1), ca fapt: golirea e decisă la operare pe registrul existent — documentul "
            + "retro nu re-decide linia din 10.05 și nu vede rândul ei; ambele ies la `round(1 × preț)`, cheia rămâne "
            + "0 bucăți cu reziduu ≠ 0 (nu se corectează tăcut)",
            bcsTarziu.Detalii.Single().Valoare == unuRetro && bcsRetro.Detalii.Single().Valoare == unuRetro
            && soldRetro.Cantitate == 0m && soldRetro.Valoare == 20.01m - 2 * unuRetro && soldRetro.Valoare != 0m);
        if (privat) {
            // Vizibil în S: în luna în care lotul e 0 la AMBELE capete (iunie), intrarea
            // se declară cu codul ei de avertisment — reziduul retro nu e ascuns.
            var sIunie = SaftProiectii.SaftStocuriPeCub(os, 2026, 6);
            var refuzRetro = sIunie.Refuzuri.Where(r => (r.Cod == SaftProiectii.RefuzReziduValoric || r.Cod == SaftProiectii.RefuzSoldNegativ)
                && r.Mesaj.Contains(lotRetro.ID.ToString())).ToList();
            var intrareRetro = sIunie.StocFizic.SingleOrDefault(e => e.LotId == lotRetro.ID);
            Console.WriteLine($"     MĂSURAT (D18-V2 r, S 06/2026): intrarea {intrareRetro?.OpeningQuantity:0.###}→{intrareRetro?.ClosingQuantity:0.###} "
                + $"@ {intrareRetro?.OpeningValue:N2}→{intrareRetro?.ClosingValue:N2}; refuzuri {string.Join(", ", refuzRetro.Select(r => r.Cod))}.");
            s.Check("D18-V2 (r, pe cub, S3-D4) SAF-T S nu ascunde reziduul retro: intrarea 0→0 bucăți cu valoarea reziduului "
                + "la ambele capete e în fișier, iar reziduul refuză fișierul pe lotul lui (−0,01 ⇒ `SAFT_SOLD_NEGATIV`; un "
                + "rezidu pozitiv ar fi `SAFT_REZIDU_VALORIC`), nu e doar avertisment",
                intrareRetro != null && intrareRetro.OpeningQuantity == 0m && intrareRetro.ClosingQuantity == 0m
                && intrareRetro.ClosingValue == soldRetroCub.Valoare
                && refuzRetro.Count > 0);
        }

        Guid? lotFiscalId = null;
        if (privat) {
            // ---------------- (f) DSC generat de FCL care golește lotul ----------------
            var client = os.CreateObject<Partener>();
            client.Cod = Marcaj + "-CL"; client.Denumire = "Client D18"; client.CodFiscal = "22222229";
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-FURN"; furnizor.Denumire = "Furnizor D18"; furnizor.CodFiscal = "RO33333338";
            var produsF = Prod("-PF");
            var produsKit = Prod("-KIT");
            os.CommitChanges();
            var lotF = LotCuRest(produsF);
            var lotG = LotCuRest(produs);
            var lotH = LotCuRest(produs);

            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = new DateOnly(2026, 5, 8); fcl.Predator = sediu; fcl.Primitor = client; fcl.GestiuneDescarcare = g1;
            var linFcl = os.CreateObject<FacturaIesireDetaliu>();
            linFcl.Document = fcl; linFcl.TipMaterial = tipMat; linFcl.Produs = produsF;
            linFcl.Cantitate = 1m; linFcl.PretUnitar = 50m; linFcl.TipTva = n21;
            os.CommitChanges();
            var dsc = MotorOperare.Opereaza(os, fcl);
            os.CommitChanges();
            var previzualizare = dsc?.Detalii.Single().Valoare;
            if (dsc != null) {
                MotorOperare.Opereaza(os, dsc);
                os.CommitChanges();
            }
            var dtoDsc = dsc == null ? null : DscApply.Citeste(os, dsc.ID);
            Console.WriteLine($"     MĂSURAT (D18-V2 f): DSC generat cu {previzualizare:N2} (previzualizarea 37b), operat "
                + $"{dsc?.Detalii.Single().Valoare:N2}, ReadDto {dtoDsc?.Linii.Single().Valoare:N2}, "
                + $"lot {SoldCheieCub(lotF, g1).Valoare:N2}.");
            s.Check("D18-V2 (f) DSC-ul conex al FCL-ului care golește lotul: generatorul lasă previzualizarea `preț × "
                + "cantitate` (10,01 — 37b), operarea materializează restul 10,00, ReadDto îl arată, lotul 0 / 0,00",
                dsc is DescarcareGestiune { Stare: StareDocument.Operat } && previzualizare == 10.01m
                && dsc.Detalii.Single().Valoare == 10.00m && dtoDsc.Linii.Single().Valoare == 10.00m
                && SoldCheieCub(lotF, g1) == (0m, 0m));

            // ---------------- (g) RLF care golește lotul: ieșire FISCALĂ, nu de evaluare (review F5) ----------------
            // Suma returului e a notei de credit a furnizorului (`q × preț`), nu a
            // lotului: RLF declară `IDocumentCuIesireFiscala`, motorul sare regula
            // de golire, reziduul de cenți rămâne pe lot (declarat în S), NU pe 401.
            var rlf = os.CreateObject<ReturFurnizor>();
            rlf.Data = new DateOnly(2026, 5, 9); rlf.Predator = g1; rlf.Primitor = furnizor;
            var linRlf = os.CreateObject<DocumentDetaliu>();
            linRlf.Document = rlf; linRlf.TipMaterial = tipMat; linRlf.Lot = lotG; linRlf.Cantitate = 1m; linRlf.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, rlf);
            os.CommitChanges();
            var noteRlf = CubScena.Note(os, rlf.ID);
            var tvaAsteptat = Scara.RotunjesteBani(10.01m * n21.Cota / 100m);
            Console.WriteLine($"     MĂSURAT (D18-V2 g): RLF 1 buc (rest 10,00, preț × 1 = 10,01) ⇒ linia {linRlf.Valoare:N2}, TVA {linRlf.ValoareTva:N2} "
                + $"(cota {n21.Cota}% ⇒ {tvaAsteptat:N2}); note "
                + $"{string.Join(" | ", noteRlf.Select(n => $"{(n.Debit ? "D" : "C")} {n.Valoare:N2}"))}; "
                + $"lot {SoldCheieCub(lotG, g1).Cantitate:0.###}/{SoldCheieCub(lotG, g1).Valoare:N2}.");
            var cont401Cub = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            s.Check("D18-V2 (g) returul la furnizor care golește lotul rămâne la `preț × cantitate` = −10,01 (suma hârtiei "
                + "furnizorului, `IDocumentCuIesireFiscala`), stornarea 3xx = 401 postează −10,01, iar lotul rămâne 0 / −0,01 — "
                + "reziduul e al lotului (declarat), nu al lui 401",
                rlf.Declarant()?.SursaValoareDeclarata == Atlas.Conta.BackOffice.Module.Declaratii.SurseValoare.Linie
                && linRlf.Valoare == -10.01m
                && SoldCheieCub(lotG, g1) == (0m, -0.01m)
                && CubScena.Note(os, rlf.ID).Any(p => p.Credit && p.Valoare == -10.01m && p.Cont == cont401Cub.ID));
            s.Check("D18-V2 (g) TVA-ul returului e coerent cu baza lui: `ValoareTva` = −round(10,01 × cota) — baza TVA și "
                + "valoarea liniei sunt aceeași cifră (F5 original: cu absorbția, baza rămânea 10,01 iar valoarea devenea 10,00)",
                linRlf.ValoareTva == -tvaAsteptat && tvaAsteptat > 0m);
            lotFiscalId = lotG.ID;

            // ---------------- (h) ASM: consumul preia restul; produsul CULES trebuie să-l egaleze ----------------
            var asm = os.CreateObject<Asamblare>();
            asm.Data = new DateOnly(2026, 5, 10); asm.Predator = g1; asm.Primitor = g1;
            var consum = os.CreateObject<AsamblareDetaliu>();
            consum.Document = asm; consum.TipMaterial = tipMat; consum.Directie = DirectieAsamblare.Consum;
            consum.Lot = lotH; consum.Cantitate = 1m;
            var produsAsm = os.CreateObject<AsamblareDetaliu>();
            produsAsm.Document = asm; produsAsm.TipMaterial = tipMat; produsAsm.Directie = DirectieAsamblare.Produs;
            produsAsm.Cantitate = 1m; produsAsm.PretEvaluare = 10.01m; // = preț × 1, evaluarea „naivă"
            var lotKit = produsAsm.CreeazaLot(os, produsKit, g1);
            os.CommitChanges();
            s.CheckRefuza("D18-V2 (h) ASM cu produsul evaluat NAIV la preț × 1 = 10,01 iar consumul GOLEȘTE lotul (ia 10,00): "
                + "invariantul |Σproduse − Σconsumuri| ≤ 0,005 SEMNALEAZĂ (0,01 > 0,005) — nicio latură nu se ajustează tăcut",
                () => MotorOperare.Opereaza(os, asm));
            var refuzAsm = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, asm).Refuzuri
                .FirstOrDefault(r => r.Cod == Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.AsamblareNebalansata);
            var consumEvaluat = refuzAsm?.Mesaj is { } mesajAsm && mesajAsm.StartsWith("Consum C=")
                ? decimal.Parse(mesajAsm["Consum C=".Length..mesajAsm.IndexOf(';')]) : (decimal?)null;
            s.Check("D18-V2 (h) la refuz consumul a fost deja evaluat la rest (−10,00): valoarea finală există înainte de "
                + "validare (33d), deci mesajul invariantului spune cifra REALĂ a consumului",
                consum.Valoare == -10.01m && consumEvaluat == 10.00m);
            produsAsm.PretEvaluare = 10.00m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, asm);
            os.CommitChanges();
            Console.WriteLine($"     MĂSURAT (D18-V2 h): consum {consum.Valoare:N2}, produs {produsAsm.Valoare:N2}, "
                + $"lotul kit {lotKit.PretUnitar:0.######}; lot consumat {SoldCheieCub(lotH, g1).Valoare:N2}.");
            s.Check("D18-V2 (h) cu produsul cules la valoarea consumului (10,00) asamblarea trece: consumul −10,00 golește "
                + "lotul la 0 / 0,00, kitul se naște cu 10,00 — valoarea nu se creează și nu se distruge (46d)",
                asm.Stare == StareDocument.Operat && consum.Valoare == -10.00m && produsAsm.Valoare == 10.00m
                && lotKit.PretUnitar == 10.00m && SoldCheieCub(lotH, g1) == (0m, 0m));
        }

        // ---------------- Nimic nu rămâne „0 bucăți, X lei" pe scenă ----------------
        var loturiScena = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
        var cheiCub = CubScena.Chei(os, loturiScena);
        var reziduuriCub = cheiCub.Where(c => c.Cantitate == 0m && c.Valoare != 0m).Select(c => c.LotId).ToList();
        // Reziduurile AȘTEPTATE, fiecare cu numele lui: retro (limita F1) și, pe
        // privat, returul fiscal (F5). Orice altă cheie golită cu valoare = defect.
        var reziduuriAsteptate = new List<Guid> { lotRetro.ID };
        if (lotFiscalId is { } fiscalId)
            reziduuriAsteptate.Add(fiscalId);
        Console.WriteLine($"     MĂSURAT (D18-V2 rezidu): {cheiCub.Count} chei pe scenă, {cheiCub.Count(c => c.Cantitate == 0m)} golite, "
            + $"{reziduuriCub.Count} cu valoare fără cantitate (așteptate {reziduuriAsteptate.Count}: retro"
            + (privat ? " + RLF fiscal" : "") + ").");
        s.Check("D18-V2 `ReziduValoricFaraCantitate` pe cheile golite de documente ale scenei = EXACT cele două limite declarate "
            + "(retro F1; pe privat și returul fiscal F5) — pe nicio altă cheie golită nu rămâne valoare (reziduul 74g nu se "
            + "mai naște din evaluare)",
            reziduuriCub.Count == reziduuriAsteptate.Count && reziduuriAsteptate.All(reziduuriCub.Contains)
            && cheiCub.Count(c => c.Cantitate == 0m) >= (privat ? 7 : 4));

        Curata();
        s.Check("D18-V2 curățenie (fără reziduuri: repartitori, documente, loturi, produse)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj)));
    }
}
