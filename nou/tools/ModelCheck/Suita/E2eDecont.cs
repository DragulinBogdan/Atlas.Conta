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

// ===================== Scenariul e2e 3c: Decont =====================
// Justificarea avansurilor (inventar 06): avans (Plata către angajat) → decont
// cu debit din contul Tipului + postare explicită pe linie (cont 623 +
// repartitor de cost) → creditul 542 dimensionat pe TITULAR (default polimorf,
// nu convenția credit←Primitor) → imperecherea lanțului avans↔decont↔
// regularizare → Tip fără cont și fără explicit = refuz clar → fallback 542
// la angajatul fără ContImplicit → gardianul de imperecheri → anulare → storno.
static class E2eDecont {
    public static void Ruleaza(Suita s) {
        const string MarcajDec = "E2E-DEC";

        void CurataDec(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDec)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            pj.Adauga(docs);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDec)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajDec + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDec(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
            var tipDeplasari = os.FirstOrDefault<TipMaterial>(t => t.Cod == "614.00.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
            var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");

            // Politicile din seed (inventar 06).
            var regulaDec = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "DEC");
            s.Check("Seed DEC: debit TipMaterial fără fallback, credit RepartitorPredator (fallback 542.01.00)",
                regulaDec != null && regulaDec.SursaContDebit == SursaCont.TipMaterial && regulaDec.ContDebitId == null
                && regulaDec.SursaContCredit == SursaCont.RepartitorPredator && regulaDec.ContCreditId == cont542.ID);
            s.Check("Seed DEC: fără reguli de stoc (doar registru contabil)",
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DEC"));

            var angajat = os.CreateObject<Angajat>();
            angajat.Cod = MarcajDec + "-ANG";
            angajat.Denumire = "Titular probă decont";
            angajat.ContImplicit = cont542;
            var angajat2 = os.CreateObject<Angajat>();
            angajat2.Cod = MarcajDec + "-ANG2";
            angajat2.Denumire = "Titular fără cont implicit";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajDec + "-CE";
            codEc.Denumire = "Cod economic probă decont";
            os.CommitChanges();

            // --- Avansul: Plata casa → angajat (542 = 531, din 3c-5) ---
            var avans = os.CreateObject<Plata>();
            avans.Data = new DateOnly(2026, 3, 3);
            avans.Predator = casa;
            avans.Primitor = angajat;
            avans.TipInstrument = TipInstrumentPlata.DispozitieCasa;
            var linieAvans = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieAvans.Document = avans;
            linieAvans.TipMaterial = tipTrz;
            linieAvans.Valoare = 100m;
            linieAvans.CodEconomicId = codEc.ID; // politica PLT + defalcarea E (531/542)
            os.CommitChanges();
            MotorOperare.Opereaza(os, avans);
            s.Check("Avansul operat (100, casa → angajat)", avans.Stare == StareDocument.Operat);

            // --- Decontul: validările proprii, apoi operarea ---
            var dec = os.CreateObject<Decont>();
            dec.Data = new DateOnly(2026, 3, 8);
            dec.Predator = sediu; // intenționat greșit — titularul e angajat
            dec.Primitor = angajat;
            var linieDeplasare = os.CreateObject<DecontDetaliu>();
            linieDeplasare.Document = dec;
            linieDeplasare.TipMaterial = tipDeplasari;
            linieDeplasare.Descriere = "Transport delegație";
            s.CheckRefuza("Laturi greșite + valoare 0 + fără clasificație → refuz",
                () => MotorOperare.Opereaza(os, dec));
            dec.Predator = angajat;
            dec.Primitor = sediu;
            linieDeplasare.PretUnitar = 30m; // cantitatea rămâne 0 — pro-forma → 1
            linieDeplasare.CodEconomicId = codEc.ID;
            // Postarea explicită pe linie (trăsătura DEC): cont + repartitor de cost.
            var linieProtocol = os.CreateObject<DecontDetaliu>();
            linieProtocol.Document = dec;
            linieProtocol.TipMaterial = tipServicii;
            linieProtocol.Descriere = "Protocol contractare";
            linieProtocol.ContDebit = cont623;
            linieProtocol.RepartitorDebit = mag1;
            linieProtocol.PretUnitar = 10m;
            linieProtocol.Cantitate = 2m;
            linieProtocol.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
            linieProtocol.CodEconomicId = codEc.ID;
            os.CommitChanges();

            s.Check("Decontul nu generează conex", MotorOperare.Opereaza(os, dec) == null);
            s.Check("Operare → stare Operat + număr din politică",
                dec.Stare == StareDocument.Operat && dec.Numar?.StartsWith("DEC-") == true);
            s.Check("Cantitatea pro-forma (0 → 1) și lanțul de valori (30 / 23,8)",
                linieDeplasare.Cantitate == 1m && linieDeplasare.Valoare == 30m
                && linieProtocol.Valoare == 23.8m);
            s.Check("Decontul nu mișcă stoc",
                CubScena.FaraStoc(os, dec.ID));
            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();
            var noteCub = NoteCub(dec);
            s.Check("Contare deplasare: debit din contul Tipului (614) = 542, 30",
                noteCub.Nota(tipDeplasari.ContImplicitId, cont542.ID, 30m));
            var protocolCub = noteCub.Where(p => p.LinieId == linieProtocol.ID).ToList();
            s.Check("Contare protocol: debitul EXPLICIT al liniei (623 bate Tipul 628) = 542, 23,8",
                protocolCub.All(p => p.Debit ? p.Cont == cont623.ID : p.Cont == cont542.ID)
                && protocolCub.Rulaj(cont623.ID, N.Latura.Debit) == 23.8m
                && protocolCub.Rulaj(cont542.ID, N.Latura.Credit) == 23.8m);

            s.Check("Dimensiuni debit: default←titular la deplasare, repartitorul EXPLICIT (MAG1) la protocol [cub]",
                noteCub.Any(p => p.LinieId == linieDeplasare.ID && p.Debit)
                && noteCub.Where(p => p.LinieId == linieDeplasare.ID && p.Debit).All(p => p.CodEconomic == codEc.ID));
            s.Check("D9-A10 DEC: debitul deplasării poartă titularul (implicit), al protocolului gestiunea culeasă MAG1; creditul 542 titularul pe ambele",
                noteCub.Where(p => p.LinieId == linieDeplasare.ID && p.Debit).All(p => p.Repartitor == angajat.ID)
                && protocolCub.Where(p => p.Debit).All(p => p.Gestiune == mag1.ID)
                && noteCub.Where(p => p.Credit).All(p => p.Repartitor == angajat.ID));

            // --- Tip fără cont și fără postare explicită = refuz clar; fallback 542 ---
            var dec2 = os.CreateObject<Decont>();
            dec2.Data = new DateOnly(2026, 3, 9);
            dec2.Predator = angajat2; // fără ContImplicit — exersează fallback-ul regulii
            dec2.Primitor = sediu;
            var linieTehnica = os.CreateObject<DecontDetaliu>();
            linieTehnica.Document = dec2;
            linieTehnica.TipMaterial = tipTrz; // TRZ nu are ContImplicit
            linieTehnica.PretUnitar = 20m;
            linieTehnica.CodEconomicId = codEc.ID;
            os.CommitChanges();
            s.CheckRefuza("Tip fără cont implicit și linie fără cont explicit → refuz",
                () => MotorOperare.Opereaza(os, dec2));
            linieTehnica.ContDebit = cont623;
            os.CommitChanges();
            MotorOperare.Opereaza(os, dec2);

            var creditDec2Cub = NoteCub(dec2).Where(p => p.Credit).ToList();
            s.Check("Angajat fără ContImplicit → creditul cade pe fallback-ul 542.01.00 [cub]",
                creditDec2Cub.Count > 0 && creditDec2Cub.All(p => p.Cont == cont542.ID));
            s.Check("D9-A10 DEC: creditul 542 de fallback poartă titularul fără cont implicit",
                creditDec2Cub.All(p => p.Repartitor == angajat2.ID));

            // --- Lanțul avans ↔ decont ↔ regularizare prin imperechere (31d) ---
            var impDecont = ImperechereService.Imperecheaza(os, avans, dec, 53.8m);
            s.Check("Imperechere avans↔decont: decontul stins integral, avansul cu rest 46,2",
                ImperechereService.Ramas(os, dec.ID) == 0m && ImperechereService.Ramas(os, avans.ID) == 46.2m);
            var regularizare = os.CreateObject<Incasare>();
            regularizare.Data = new DateOnly(2026, 3, 15);
            regularizare.Predator = angajat;
            regularizare.Primitor = casa;
            regularizare.TipInstrument = TipInstrumentPlata.DispozitieCasa;
            var linieReg = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieReg.Document = regularizare;
            linieReg.TipMaterial = tipTrz;
            linieReg.Valoare = 46.2m;
            linieReg.CodEconomicId = codEc.ID; // casa (531) cere E
            os.CommitChanges();
            MotorOperare.Opereaza(os, regularizare);
            ImperechereService.Imperecheaza(os, regularizare, avans, 46.2m);
            s.Check("Regularizarea stinge restul: avansul asignat pe AMBELE roluri, rest 0",
                ImperechereService.Ramas(os, avans.ID) == 0m && ImperechereService.Ramas(os, regularizare.ID) == 0m);

            // F3-D3: panoul de stingeri pe documentul aflat pe AMBELE roluri — avansul
            // stinge decontul ȘI e stins de regularizare, deci `EsteStingator` diferă
            // între rândurile ACELUIAȘI panou (cazul pe care un DTO cu o singură
            // „coloană" de imperecheri l-ar fi ratat).
            var stingeriAvans = ImperechereApply.Stingeri(os, avans.ID);
            var randStinge = stingeriAvans.Imperecheri.Single(i => i.EsteStingator);
            var randStins = stingeriAvans.Imperecheri.Single(i => !i.EsteStingator);
            s.Check("StingeriDto pe avans: două rânduri cu roluri OPUSE (stinge DEC 53,8; e stins de INC 46,2), numerele din serviciu",
                stingeriAvans is { Total: 100m, Asignat: 100m, Ramas: 0m }
                && stingeriAvans.Imperecheri.Count == 2
                && randStinge is { CelalaltTip: "DEC", Suma: 53.8m } && randStinge.CelalaltDocumentId == dec.ID
                && randStins is { CelalaltTip: "INC", Suma: 46.2m } && randStins.CelalaltDocumentId == regularizare.ID);
            s.Check("…iar din capătul celălalt: panoul decontului vede avansul ca stingător de tip „PLT”, cu rest 0",
                ImperechereApply.Stingeri(os, dec.ID) is { Total: 53.8m, Ramas: 0m } panouDec
                && panouDec.Imperecheri.Single() is { EsteStingator: false, CelalaltTip: "PLT", Suma: 53.8m });

            // --- Gardianul de imperecheri + corecția directă + storno ---
            s.CheckRefuza("Anularea decontului cu imperechere → refuz", () => MotorOperare.AnuleazaOperarea(os, dec));
            ImperechereService.Sterge(os, impDecont.ID);
            MotorOperare.AnuleazaOperarea(os, dec);
            s.Check("Anulare decont → Draft + notele șterse",
                dec.Stare == StareDocument.Draft && CubScena.FaraNote(os, dec.ID));
            MotorOperare.Opereaza(os, dec);
            s.Check("Re-operare idempotentă (cantitatea pro-forma rămâne 1, numărul rămâne)",
                dec.Stare == StareDocument.Operat && linieDeplasare.Cantitate == 1m
                && dec.Numar?.StartsWith("DEC-") == true);
            MotorOperare.Storneaza(os, dec, new DateOnly(2026, 7, 22));
            var stornoDecCub = CubScena.Note(os, dec.ID).Where(p => p.Storno).ToList();
            s.Check("Storno decont → note inverse append-only (−30, −23,8) la data stornării",
                dec.Stare == StareDocument.Stornat
                && stornoDecCub.Count > 0 && stornoDecCub.All(p => p.Data == new DateOnly(2026, 7, 22))
                && stornoDecCub.Where(p => p.Debit && p.LinieId == linieDeplasare.ID).Sum(p => p.Valoare) == -30m
                && stornoDecCub.Where(p => p.Debit && p.LinieId == linieProtocol.ID).Sum(p => p.Valoare) == -23.8m);

            CurataDec(os);
            s.Check("Curățenie finală decont (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajDec)));
        }
    }
}
