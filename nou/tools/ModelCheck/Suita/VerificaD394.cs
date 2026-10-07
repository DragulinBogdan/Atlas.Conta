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

static class VerificaD394 {
    public static void Ruleaza(Suita s, bool cuTva) {
        const string MarcajD4 = "E2E-D394";
        using var os = s.Provider.CreateObjectSpace();

        var pStart = new DateOnly(2026, 8, 1);
        var pEnd = new DateOnly(2026, 8, 31);
        // Tăietura pentru storno (D4-V3): operarea în prima jumătate, stornarea în a doua.
        var mijloc = new DateOnly(2026, 8, 20);

        void CurataD4() {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajD4)).Select(r => r.ID).ToList();
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => d.Numar.StartsWith(MarcajD4)
                    || repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).Distinct().ToList();
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
                    .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajD4)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajD4)).ToList());
            // Tipul de TVA de probă (cota ne-întreagă) și maparea lui: artefacte de
            // scenă, deci purjă fizică — o mapare marcată ștearsă ar fi citită de
            // `VerificaD394` drept „nemapată de utilizator" (69b/F5).
            pj.Adauga(os.GetObjectsQuery<MapareD394>().Where(m => m.TipTva.Cod.StartsWith(MarcajD4)).ToList());
            pj.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod.StartsWith(MarcajD4)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajD4)).ToList());
            pj.Executa();
        }
        CurataD4();

        var reziduuPerioada = CubScena.FapteIntre(os, pStart, pEnd).Count;
        Console.WriteLine($"     MĂSURAT (premisa scenei D394): {reziduuPerioada} fapte fiscale preexistente în {pStart:MM.yyyy}.");
        s.Check("D4-V2 premisă: luna scenei e goală înainte de scenă — altfel cifrele exacte și cusăturile de mai jos "
            + "ar fi măsurate peste conținut străin",
            CubScena.FapteIntre(os, pStart, pEnd).Count == 0);

        // ---------------- Funcțiile nomenclatorului (D4-D1), fără bază ----------------
        s.Check("D4-V2 `TipPartener` — ÎNREGISTRAT BATE TOT (fixurile 2/3): PF înregistrată (PFA/II) ⇒ 1, nu 2; străin "
            + "înregistrat în RO (DE/GR/US cu cod RO) ⇒ 1, nu 3/4; apoi PF ⇒ 2; RO neînregistrat ⇒ 2; `Tara` goală/"
            + "null ⇒ RO (nu 4); UE (DE/GR) ⇒ 3; non-UE (US/GB) ⇒ 4",
            D394Proiectii.TipPartener(TipPersoana.Fizica, "RO", true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Fizica, "RO", false) == 2
            && D394Proiectii.TipPartener(TipPersoana.Fizica, "DE", false) == 2
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "DE", true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "GR", true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "US", true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "RO", true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "RO", false) == 2
            && D394Proiectii.TipPartener(TipPersoana.Juridica, null, true) == 1
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "", false) == 2
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "DE", false) == 3
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "US", false) == 4
            && D394Proiectii.TipPartener(TipPersoana.Juridica, "GB", false) == 4);
        s.Check("D4-V2 `NormalizeazaCui`: trim + majuscule + spații interioare scoase; prefixul RO tăiat pe RO SAU pe "
            + "înregistrat („ro 12 345 678” ⇒ „12345678”; DE înregistrat cu „RO999” ⇒ „999”), codul străin al "
            + "neînregistratului rămâne întreg („DE123456789”, „RO999” pe DE neînregistrat); gol/spații ⇒ null",
            D394Proiectii.NormalizeazaCui(" ro 12 345 678 ", "RO", true) == "12345678"
            && D394Proiectii.NormalizeazaCui("ro12345678", "RO", false) == "12345678"
            && D394Proiectii.NormalizeazaCui("12345678", null, false) == "12345678"
            && D394Proiectii.NormalizeazaCui("DE123456789", "DE", false) == "DE123456789"
            && D394Proiectii.NormalizeazaCui("RO999", "DE", false) == "RO999"
            && D394Proiectii.NormalizeazaCui("RO999", "DE", true) == "999"
            && D394Proiectii.NormalizeazaCui("  ", "RO", false) == null
            && D394Proiectii.NormalizeazaCui("-", "RO", false) == null
            && D394Proiectii.NormalizeazaCui(" ./ ", "RO", true) == null
            && D394Proiectii.NormalizeazaCui("RO-", "RO", true) == null
            && D394Proiectii.NormalizeazaCui(null, "RO", true) == null
            && D394Proiectii.NormalizeazaCui("RO", "RO", true) == null);
        s.Check("D4-V2 `NormalizeazaCui`: tăierea prefixului `RO` e REPETATĂ și insensibilă la caz — „RORo1853162” ⇒ "
            + "„1853162”, „ROro37472851” ⇒ „37472851” (singura cauză de respingere a fișierului SAF-T real la "
            + "validatorul ANAF: 2 parteneri din 5.536 cu prefixul pus de două ori, în grafii diferite). Se taie DOAR "
            + "acolo unde se tăia și înainte (RO sau înregistrat): codul străin al neînregistratului rămâne întreg, "
            + "iar un cod rămas gol după tăiere e tot null",
            D394Proiectii.NormalizeazaCui("RORo1853162", "RO", false) == "1853162"
            && D394Proiectii.NormalizeazaCui("ROro37472851", "RO", true) == "37472851"
            && D394Proiectii.NormalizeazaCui("RORO999", "DE", true) == "999"
            && D394Proiectii.NormalizeazaCui(" ro 12 345 678 ", "RO", true) == "12345678"
            && D394Proiectii.NormalizeazaCui("RORO1853162", "DE", false) == "RORO1853162"
            && D394Proiectii.NormalizeazaCui("RO999", "DE", false) == "RO999"
            && D394Proiectii.NormalizeazaCui("RORO", "RO", true) == null);

        // ---------------- Bugetarul: liste goale, și atât ----------------
        if (!cuTva) {
            var premisa = FctBugetaraOperata.Ruleaza(s, os, MarcajD4 + "-BUG", new DateOnly(2026, 8, 12));
            var anIntreg = D394Proiectii.D394(os, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
            Console.WriteLine($"     MĂSURAT (D4-V2 bugetar): documentul scenei {premisa.Numar} {premisa.Stare} pe {premisa.Data}, "
                + $"{premisa.Detalii.Count(l => l.TipTvaId != null)} linii cu TipTva; {CubScena.Fapte(os, premisa.ID).Count} fapte fiscale ale lui, "
                + $"{Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Fapte(os).Count()} în total.");
            s.Check("D4-V2 (bugetar): profilul neplătitor n-are `PoliticaTva` ⇒ fără fapte fiscale ⇒ proiecția întoarce "
                + "liste GOALE (operațiuni, rezumate, neincluse), zero avertismente și nrCui 0, pe un an în care scena a "
                + "OPERAT prin motor o FCT cu linie purtând TipTva (CAP21) — un neplătitor nu depune 394, iar proiecția nu "
                + "inventează nimic",
                premisa.Stare == StareDocument.Operat && premisa.Data.Year == 2026
                && premisa.Detalii.Any(l => l.TipTvaId != null)
                && CubScena.FaraFapte(os, premisa.ID) && CubScena.FaraFapte(os)
                && anIntreg.Operatiuni.Count == 0 && anIntreg.Rezumat.Count == 0 && anIntreg.RezumatCote.Count == 0
                && anIntreg.Neincluse.Count == 0 && anIntreg.Avertismente.Count == 0
                && anIntreg.NrCui1 + anIntreg.NrCui2 + anIntreg.NrCui3 + anIntreg.NrCui4 == 0
                && os.GetObjectsQuery<MapareD394>().Count() == 0);
            PurjaFctBugetara.Ruleaza(s, os, MarcajD4 + "-BUG");
            CurataD4();
            return;
        }

        // ---------------- Scena privată ----------------
        var mag = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");
        var tipCheltuiala = os.GetObjectsQuery<TipMaterial>()
            .First(t => t.Clasa.Natura == NaturaClasa.Serviciu && t.ContImplicitId != null);
        TipTva Tva(string cod) => os.FirstOrDefault<TipTva>(t => t.Cod == cod);
        var n21 = Tva("N21"); var n11 = Tva("N11"); var ti21 = Tva("TI21"); var ned21 = Tva("NED21"); var sdd = Tva("SDD");

        Partener Part(string sufix, string denumire, string cui, TipPersoana tipPersoana = TipPersoana.Juridica,
            string tara = "RO", bool inregistrat = true, bool laIncasare = false) {
            var p = os.CreateObject<Partener>();
            p.Cod = MarcajD4 + sufix; p.Denumire = denumire; p.CodFiscal = cui;
            p.TipPersoana = tipPersoana; p.Tara = tara; p.InregistratTva = inregistrat; p.TvaLaIncasare = laIncasare;
            return p;
        }
        // Cele patru tipuri de partener + PF, cu codurile fiscale din contract (D4-V2).
        var f1 = Part("-F1", "Furnizor tip 1 (RO…)", "RO12345678");
        var f1bis = Part("-F1B", "Furnizor tip 1 (același CUI, fără prefix)", "12345678");
        var f2 = Part("-F2", "Furnizor TVA la încasare", "RO22222222", laIncasare: true);
        var f0 = Part("-F0", "Furnizor tip 1 FĂRĂ cod fiscal", null);
        var c1 = Part("-C1", "Client tip 1", "RO33333333");
        var pf = Part("-PF", "Popescu Ion (CNP)", "1800101123456", TipPersoana.Fizica, inregistrat: false);
        var pf0 = Part("-PF0", "Ionescu Maria (fără CNP)", null, TipPersoana.Fizica, inregistrat: false);
        // CNP în ALT format decât 13 cifre (Import1C, pasul 4a: `CodFiscal` = CNP-ul copiat ca atare).
        var pf1 = Part("-PF1", "Georgescu Ana (CNP scurt)", "123", TipPersoana.Fizica, inregistrat: false);
        var de = Part("-DE", "Kunde GmbH", "DE123456789", tara: "DE", inregistrat: false);
        var us = Part("-US", "Buyer Inc", "US-999", tara: "US", inregistrat: false);
        // Fixurile 1–3 ale review-ului advers, pe scenă:
        //  • PFA ÎNREGISTRATĂ în scopuri de TVA (cod RO, persoană fizică) ⇒ tip 1, nu 2;
        //  • DE cu cod RO (înregistrare directă, art. 316) ⇒ tip 1 cu CUI-ul RO fără prefix;
        //  • scenariul BLOCANT: același CUI pe un nomenclator neînregistrat („1590120”) și pe unul
        //    înregistrat („RO1590120”) ⇒ UN singur rând, tip 1, nu două rânduri cu aceeași cheie XSD.
        var pfa = Part("-PFA", "Popescu Vasile PFA", "RO55555555", TipPersoana.Fizica, inregistrat: true);
        var deRo = Part("-DERO", "Kunde RO-reg GmbH", "RO66666666", tara: "DE", inregistrat: true);
        var g1 = Part("-G1", "Grup SRL (grafie fără prefix, neînregistrat)", "1590120", inregistrat: false);
        var g2 = Part("-G2", "Grup SRL (grafie cu prefix, înregistrat)", "RO1590120");
        var unitate = os.CreateObject<UnitateInterna>();
        unitate.Cod = MarcajD4 + "-UNIT"; unitate.Denumire = "Unitate probă D394";
        var gest = os.CreateObject<Gestiune>();
        gest.Cod = MarcajD4 + "-G"; gest.Denumire = "Gestiune probă D394";
        var angajat = os.CreateObject<Angajat>();
        angajat.Cod = MarcajD4 + "-ANG"; angajat.Denumire = "Titular decont probă D394";
        var produs = os.CreateObject<Produs>();
        produs.Cod = MarcajD4 + "-P"; produs.Denumire = "Marfă probă D394"; produs.UM = "BUC"; produs.TipMaterial = tip371;
        os.CommitChanges();

        FacturaIntrare Cumparare(string sufix, Partener furnizor, DateOnly data) {
            var f = os.CreateObject<FacturaIntrare>();
            f.Numar = MarcajD4 + sufix; f.Data = data; f.Predator = furnizor; f.Primitor = mag;
            return f;
        }
        FacturaIesire Vanzare(string sufix, Partener client, DateOnly data) {
            var f = os.CreateObject<FacturaIesire>();
            f.Numar = MarcajD4 + sufix; f.Data = data; f.Predator = unitate; f.Primitor = client;
            return f;
        }
        void LinieC(FacturaIntrare f, TipTva tva, decimal pret) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = f; d.TipMaterial = tip628; d.Cantitate = 1m; d.PretUnitar = pret; d.TipTva = tva;
        }
        FacturaIesireDetaliu LinieV(FacturaIesire f, TipTva tva, decimal pret) {
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = f; d.TipMaterial = tip704; d.Cantitate = 1m; d.PretUnitar = pret; d.TipTva = tva;
            return d;
        }
        var d5 = new DateOnly(2026, 8, 5);
        var d10 = new DateOnly(2026, 8, 10);

        // --- Achizițiile ---
        var fctA1 = Cumparare("-A1", f1, d5);
        LinieC(fctA1, n21, 1000m);    // A/21  1000 / 210
        LinieC(fctA1, n11, 500m);     // A/11   500 /  55   ⇒ nrFact 0 (TVA-ul maxim e pe 21)
        LinieC(fctA1, ned21, 100m);   // A/21 (capitalizat: brut 121 ⇒ 100 / 21) — se adună cu N21 pe același rând
        LinieC(fctA1, ti21, 400m);    // C/21   400 /  84   ⇒ al doilea tip pe aceeași factură: 1 + 1
        LinieC(fctA1, sdd, 70m);      // NEINCLUS: SDD nemapat deliberat
        var fctA2 = Cumparare("-A2", f1bis, d5);
        LinieC(fctA2, n21, 200m);     // A/21 pe ACELAȘI CUI normalizat ⇒ se unește cu F1, cu avertisment
        var fctAI = Cumparare("-AI", f2, d5);
        LinieC(fctAI, n21, 300m);     // AI/21 300 / 63 (furnizor cu TVA la încasare)
        var fctF0 = Cumparare("-A0", f0, d5);
        LinieC(fctF0, n21, 90m);      // A/21 90 / 18,9 — tip 1 fără CUI ⇒ avertisment, rândul rămâne
        var fctG1 = Cumparare("-G1", g1, d5);
        LinieC(fctG1, n21, 10m);      // A/21 10 / 2,1 pe „1590120” — nomenclatorul NEînregistrat
        var fctG2 = Cumparare("-G2", g2, d5);
        LinieC(fctG2, n21, 20m);      // A/21 20 / 4,2 pe „RO1590120” — nomenclatorul înregistrat ⇒ același rând, tip 1
        // NIR manual: lotul original pentru retururi (10 buc × 10 lei), fără TVA.
        var nir = os.CreateObject<NIR>();
        nir.Data = new DateOnly(2026, 8, 2); nir.Predator = f1; nir.Primitor = gest;
        var linNir = os.CreateObject<DocumentDetaliu>();
        linNir.Document = nir; linNir.TipMaterial = tip371; linNir.Cantitate = 10m; linNir.Valoare = 100m;
        var lot = linNir.CreeazaLot(os, produs, gest);
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctA1);
        MotorOperare.Opereaza(os, fctA2);
        MotorOperare.Opereaza(os, fctAI);
        MotorOperare.Opereaza(os, fctF0);
        MotorOperare.Opereaza(os, fctG1);
        MotorOperare.Opereaza(os, fctG2);
        MotorOperare.Opereaza(os, nir);
        os.CommitChanges();
        // RLF către F1: 2 buc din lot ⇒ A/21 −20 / −4,2 (sume negative, factură = 1).
        var rlf = os.CreateObject<ReturFurnizor>();
        rlf.Data = d10; rlf.Predator = gest; rlf.Primitor = f1;
        var linRlf = os.CreateObject<DocumentDetaliu>();
        linRlf.Document = rlf; linRlf.TipMaterial = tip371; linRlf.Lot = lot; linRlf.Cantitate = 2m; linRlf.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rlf);
        os.CommitChanges();

        // --- Livrările ---
        var fclL1 = Vanzare("-L1", c1, d5);
        LinieV(fclL1, n21, 2000m);    // L/21 2000 / 420 ⇒ 1
        LinieV(fclL1, n11, 800m);     // L/11  800 /  88 ⇒ 0
        // EGALITATE de TVA: 100 × 21% = 21,00 și 190,91 × 11% = 21,0001 ⇒ 21,00 ⇒ 1 pe cota MAI MARE (21).
        var fclEgal = Vanzare("-LE", c1, d5);
        LinieV(fclEgal, n21, 100m);
        LinieV(fclEgal, n11, 190.91m);
        // L + V pe aceeași factură ⇒ 1 + 1 (tipuri diferite se numără separat).
        var fclLV = Vanzare("-LV", c1, d5);
        LinieV(fclLV, n21, 500m);
        var linieTi = LinieV(fclLV, ti21, 700m);   // V/0, bază 700, TVA 0 (70a)
        // Operată în prima jumătate, STORNATĂ în a doua (D4-V3).
        var fclStorno = Vanzare("-LS", c1, d5);
        LinieV(fclStorno, n21, 400m);
        var fclPf = Vanzare("-PF", pf, d5);
        LinieV(fclPf, n21, 150m);     // tip 2 (PF cu CNP), L/21 150 / 31,5
        var fclPf0 = Vanzare("-PF0", pf0, d5);
        LinieV(fclPf0, n21, 60m);     // tip 2 (PF fără CNP) ⇒ avertisment
        var fclPf1 = Vanzare("-PF1", pf1, d5);
        LinieV(fclPf1, n21, 40m);     // tip 2 (CNP „123”), L/21 40 / 8,4 ⇒ avertisment, cuiP = „123”
        var fclDe = Vanzare("-DE", de, d5);
        LinieV(fclDe, n21, 250m);     // tip 3, L/21 250 / 52,5
        var fclUs = Vanzare("-US", us, d5);
        LinieV(fclUs, n21, 80m);      // tip 4, L/21 80 / 16,8
        var fclPfa = Vanzare("-PFA", pfa, d5);
        LinieV(fclPfa, n21, 25m);     // PFA înregistrată ⇒ tip 1, cuiP „55555555”, L/21 25 / 5,25
        var fclDeRo = Vanzare("-DERO", deRo, d5);
        LinieV(fclDeRo, n21, 15m);    // DE cu cod RO ⇒ tip 1, cuiP „66666666”, L/21 15 / 3,15
        // Cotă NE-ÎNTREAGĂ (5,5%): tip de TVA de probă, mapat pe L ⇒ rând pe cota 5, cu avertisment.
        var tvaC55 = os.CreateObject<TipTva>();
        tvaC55.Cod = MarcajD4 + "-C55"; tvaC55.Denumire = "Probă cotă 5,5%"; tvaC55.Cota = 5.5m; tvaC55.Regim = RegimTva.Normal;
        tvaC55.ContTvaDeductibilId = n21.ContTvaDeductibilId; tvaC55.ContTvaColectatId = n21.ContTvaColectatId;
        var mapC55 = os.CreateObject<MapareD394>();
        mapC55.TipTva = tvaC55; mapC55.Sens = SensTva.Livrare; mapC55.Tip = TipOperatiuneD394.L;
        var fclC55 = Vanzare("-C55", c1, d5);
        LinieV(fclC55, tvaC55, 100m); // L/5 (trunchiat) 100 / 5,5
        os.CommitChanges();
        foreach (var f in new[] { fclL1, fclEgal, fclLV, fclStorno, fclPf, fclPf0, fclPf1, fclDe, fclUs, fclC55, fclPfa, fclDeRo })
            MotorOperare.Opereaza(os, f);
        os.CommitChanges();
        MotorOperare.Storneaza(os, fclStorno, new DateOnly(2026, 8, 25));
        os.CommitChanges();
        // RDC de la C1: venit 100 (N21) + cost pe lotul original ⇒ L/21 −100 / −21, factură = 1.
        var rdc = os.CreateObject<ReturClient>();
        rdc.Data = d10; rdc.Predator = c1; rdc.Primitor = gest;
        var linVenit = os.CreateObject<DocumentDetaliu>();
        linVenit.Document = rdc; linVenit.TipMaterial = tip707; linVenit.Valoare = 100m; linVenit.TipTva = n21;
        var linCost = os.CreateObject<DocumentDetaliu>();
        linCost.Document = rdc; linCost.TipMaterial = tip371; linCost.Lot = lot; linCost.Cantitate = 1m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rdc);
        os.CommitChanges();
        // DEC: contrapartida e ANGAJATUL ⇒ `RepartitorNePartener` (riscul 6).
        var dec = os.CreateObject<Decont>();
        dec.Data = d10; dec.Predator = angajat; dec.Primitor = unitate;
        var linDec = os.CreateObject<DecontDetaliu>();
        linDec.Document = dec; linDec.TipMaterial = tipCheltuiala; linDec.Descriere = "Bon justificat";
        linDec.PretUnitar = 50m; linDec.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, dec);
        os.CommitChanges();

        // ══════════ D4-V2: fiecare rând, la cifră ══════════
        var d4 = D394Proiectii.D394(os, pStart, pEnd);
        Console.WriteLine($"     MĂSURAT (D4-V2, op1 pe {pStart:MM.yyyy}):");
        foreach (var o in d4.Operatiuni)
            Console.WriteLine($"         tip{o.TipPartener} {o.CuiP ?? "(fără CUI)",-14} {o.Tip,-2} cota {o.Cota,2}  "
                + $"nrFact {o.NrFact}  bază {o.Baza,10:N2}  TVA {(o.Tva is decimal t ? t.ToString("N2") : "—"),10}  "
                + $"n={o.Randuri} doc={o.Documente}  {o.Denumire}");
        foreach (var n in d4.Neincluse)
            Console.WriteLine($"         NEINCLUS {n.Cauza} {n.TipTvaCod}/{n.Sens} {n.Cota}% bază {n.Baza:N2} TVA {n.Tva:N2} "
                + $"n={n.Randuri} {n.RepartitorDenumire}");
        foreach (var a in d4.Avertismente)
            Console.WriteLine($"         AVERTISMENT {a.Cod} ×{a.Numar}{(a.Suma is decimal sa ? $" Σ {sa:N2}" : "")}: {string.Join(" | ", a.Exemple)}");
        D394Operatiune Op(int tipP, string cui, string tip, int cota) =>
            d4.Operatiuni.SingleOrDefault(o => o.TipPartener == tipP && o.CuiP == cui && o.Tip == tip && o.Cota == cota);
        D394Operatiune OpDen(string denumire, string tip, int cota) =>
            d4.Operatiuni.SingleOrDefault(o => o.Denumire == denumire && o.Tip == tip && o.Cota == cota);

        s.Check("D4-V2 tip 1 / A: F1 și F1-bis se UNESC pe CUI-ul normalizat „12345678” (prefixul RO tăiat): A/21 = "
            + "N21 1000 + NED21 100 (capitalizat, baza desfăcută) + F1-bis 200 + RLF −20 = 1280 / 268,8, pe 3 "
            + "documente și 4 rânduri de registru; A/11 = 500 / 55 dintr-un singur document",
            Op(1, "12345678", "A", 21) is { Baza: 1280m, Tva: 268.8m, Documente: 3, Randuri: 4 }
            && Op(1, "12345678", "A", 11) is { Baza: 500m, Tva: 55m, Documente: 1 });
        s.Check("D4-V2 tip 1 / C: taxarea inversă pe achiziție (TI21) = C/21 400 / 84 — TVA autolichidată, cu coloană; "
            + "AI: furnizorul cu `TvaLaIncasare` mută A în AI (300 / 63) fără nicio mapare",
            Op(1, "12345678", "C", 21) is { Baza: 400m, Tva: 84m }
            && Op(1, "22222222", "AI", 21) is { Baza: 300m, Tva: 63m, NrFact: 1 }
            && !d4.Operatiuni.Any(o => o.CuiP == "22222222" && o.Tip == "A"));
        s.Check("D4-V2 tip 1 / L și V la clientul C1 („33333333”): L/21 = 2000 + 100 + 500 + (400 − 400 storno) − 100 "
            + "RDC = 2500 / 525 pe 5 documente; L/11 = 800 + 190,91 = 990,91 / 109; V/0 = 700 cu `Tva` NULL "
            + "(fără coloană) și `TvaNedeclarat` 0 (70a: livrarea în taxare inversă n-are taxă)",
            Op(1, "33333333", "L", 21) is { Baza: 2500m, Tva: 525m, Documente: 5 }
            && Op(1, "33333333", "L", 11) is { Baza: 990.91m, Tva: 109m, Documente: 2 }
            && Op(1, "33333333", "V", 0) is { Baza: 700m, Tva: null, TvaNedeclarat: 0m, NrFact: 1, Documente: 1 });
        s.Check("D4-V2 tipurile 2/3/4: PF cu CNP ⇒ tip 2, cuiP = CNP-ul (13 cifre); PF fără CNP ⇒ tip 2, cuiP null, rând "
            + "PROPRIU (nu se unește cu cealaltă PF); DE ⇒ tip 3, codul ÎNTREG „DE123456789”; US ⇒ tip 4, "
            + "„US-999” intact",
            Op(2, "1800101123456", "L", 21) is { Baza: 150m, Tva: 31.5m, NrFact: 1 }
            && OpDen("Ionescu Maria (fără CNP)", "L", 21) is { TipPartener: 2, CuiP: null, Baza: 60m, Tva: 12.6m }
            && Op(2, "123", "L", 21) is { Baza: 40m, Tva: 8.4m, NrFact: 1 }
            && Op(3, "DE123456789", "L", 21) is { Baza: 250m, Tva: 52.5m }
            && Op(4, "US-999", "L", 21) is { Baza: 80m, Tva: 16.8m });
        s.Check("D4-V2 tip 1 fără cod fiscal: rândul se EMITE (A/21 90 / 18,9, cuiP null) — cifra nu se ascunde; cota "
            + "5,5% iese pe rândul cotei 5 cu sumele exacte (100 / 5,5)",
            OpDen("Furnizor tip 1 FĂRĂ cod fiscal", "A", 21) is { TipPartener: 1, CuiP: null, Baza: 90m, Tva: 18.9m }
            && Op(1, "33333333", "L", 5) is { Baza: 100m, Tva: 5.5m, NrFact: 1 });
        s.Check("D4-V2 forma: enum-urile pleacă STRING (`Tip` ȘI `Sens`), rândurile ordonate pe tipul de partener, fără "
            + "dubluri pe cheia XSD (cuiP, tip, cota) pe TOT `Operatiuni` — tipul de partener NU mai e o axă a cheii",
            d4.Operatiuni.All(o => o.Tip is "L" or "A" or "AI" or "V" or "C")
            && d4.Operatiuni.All(o => o.Sens is "Livrare" or "Achizitie")
            && d4.Operatiuni.Select(o => o.TipPartener).SequenceEqual(d4.Operatiuni.Select(o => o.TipPartener).OrderBy(t => t))
            && d4.Operatiuni.Where(o => o.CuiP != null).Select(o => (o.CuiP, o.Tip, o.Cota)).Distinct().Count()
                == d4.Operatiuni.Count(o => o.CuiP != null));
        s.Check("D4-V2 (fixurile 2/3) PFA înregistrată ⇒ tip 1 cu cuiP „55555555” (L/21 25 / 5,25), NU pe cartușul D; DE cu cod RO "
            + "⇒ tip 1 cu cuiP „66666666” (prefixul tăiat deși țara e DE), NU tip 3 cu „RO66666666”",
            Op(1, "55555555", "L", 21) is { Baza: 25m, Tva: 5.25m, NrFact: 1 }
            && Op(1, "66666666", "L", 21) is { Baza: 15m, Tva: 3.15m, NrFact: 1 }
            && !d4.Operatiuni.Any(o => o.TipPartener == 2 && o.CuiP == "55555555")
            && !d4.Operatiuni.Any(o => o.CuiP == "RO66666666" || (o.CuiP == "66666666" && o.TipPartener != 1)));
        s.Check("D4-V2 (fix 1, scenariul BLOCANT) același CUI pe „1590120” (neînregistrat) și „RO1590120” (înregistrat) ⇒ UN "
            + "singur rând op1, tip 1, A/21 = 10 + 20 = 30 / 6,3 pe 2 documente — niciun rând tip 2 cu aceeași cheie XSD; "
            + "avertismentul `CuiUnit` numește AMBELE nomenclatoare cu tipul lor propriu",
            Op(1, "1590120", "A", 21) is { Baza: 30m, Tva: 6.3m, Documente: 2, NrFact: 2 }
            && d4.Operatiuni.Count(o => o.CuiP == "1590120") == 1
            && d4.Avertismente.SingleOrDefault(a => a.Cod == "CuiUnit") is { } cuiUnit
            && cuiUnit.Exemple.Any(e => e.StartsWith("CUI 1590120 (rând tip 1)") && e.Contains("grafie fără prefix, neînregistrat)” (tip 2)")
                && e.Contains("grafie cu prefix, înregistrat)” (tip 1)")));

        // ══════════ D4-V3: nrFact — regula 1/0 per document ══════════
        s.Check("D4-V3 nrFact: F1 A/21 = 3 (A1 + A2 + RLF — și A1 numără pe 21, nu pe 11, fiindcă 231 > 55) și A/11 = 0; "
            + "C1 L/21 = 6 (L1, egalitatea pe cota mai mare, LV, factura stornată + factura ei de STORNO, RDC) și L/11 = 0 (L1: 88 < 420; "
            + "egalitatea: 21 = 21 ⇒ cota 21)",
            Op(1, "12345678", "A", 21).NrFact == 3 && Op(1, "12345678", "A", 11).NrFact == 0
            && Op(1, "33333333", "L", 21).NrFact == 6 && Op(1, "33333333", "L", 11).NrFact == 0);
        s.Check("D4-V3 nrFact: L + V pe aceeași factură se numără SEPARAT per tip (1 + 1); C pe factura A1 la fel (A: 1, "
            + "C: 1); Σ nrFact pe rândurile lui F1 = numărul facturilor lui (A1, A2, RLF pe A + A1 încă o dată pe C = 3 + 1)",
            Op(1, "33333333", "V", 0).NrFact == 1 && Op(1, "12345678", "C", 21).NrFact == 1
            && d4.Operatiuni.Where(o => o.CuiP == "12345678").Sum(o => o.NrFact) == 4);
        // Tăietura de ZILE nu mai taie: declarația filtrează pe perioada de
        // DECLARARE, care e LUNA (F27-D5), deci ambele jumătăți ale lui august
        // întorc luna întreagă. Faptul că stornoul e un fapt distinct se citește din
        // registru (rândurile lui sunt în a doua jumătate, cu semn) și din
        // nrFact 6 peste Documente 5 — numărătoarea e (Document × Storno).
        var jum1 = D394Proiectii.D394(os, pStart, mijloc);
        var jum2 = D394Proiectii.D394(os, mijloc.AddDays(1), pEnd);
        var s1 = jum1.Operatiuni.Single(o => o.CuiP == "33333333" && o.Tip == "L" && o.Cota == 21);
        var s2 = jum2.Operatiuni.SingleOrDefault(o => o.CuiP == "33333333" && o.Tip == "L" && o.Cota == 21);
        var stornoCub = Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Fapte(os).ToList()
            .Where(f => f.Storno && f.Data > mijloc && f.Data <= pEnd && f.PartenerId == c1.ID
                && f.Sens == SensTva.Livrare && f.Cota == 21m).ToList();
        Console.WriteLine($"     MĂSURAT (D4-V3 storno): 1–20.08 L/21 C1 {s1.Baza:N2}/{s1.Tva:N2} nrFact {s1.NrFact}; "
            + $"21–31.08 {s2?.Baza:N2}/{s2?.Tva:N2} nrFact {s2?.NrFact}; luna întreagă {Op(1, "33333333", "L", 21).Baza:N2} nrFact {Op(1, "33333333", "L", 21).NrFact}; "
            + $"cub 21–31.08: {stornoCub.Count} fapte, Σ {stornoCub.Sum(f => f.Baza):N2}/{stornoCub.Sum(f => f.Tva):N2}.");
        s.Check("D4-V3 storno: rândurile inverse ale lui C1 stau în a doua jumătate a lunii, cu sume NEGATIVE "
            + "(−400 / −84) și cu perioada de declarare a stornării; pe luna întreagă ele se netează cu operarea pe "
            + "același DOCUMENT (Documente 5, 2500 / 525) dar sunt DOUĂ FACTURI la ANAF — nrFact 6, fiindcă unitatea "
            + "de numărare e (Document × Storno), nu DocumentId. O fereastră de ZILE nu mai taie luna: declarația "
            + "filtrează pe perioada de DECLARARE, care e luna întreagă (F27-D5), deci ambele jumătăți întorc aceleași "
            + "cifre ca luna",
            stornoCub.Count == 1
            && stornoCub[0].Baza == -400m && stornoCub[0].Tva == -84m
            && stornoCub[0].PerioadaAn == pEnd.Year && stornoCub[0].PerioadaLuna == pEnd.Month
            && Op(1, "33333333", "L", 21) is { Baza: 2500m, Tva: 525m, NrFact: 6, Documente: 5 }
            && s1 is { Baza: 2500m, Tva: 525m, NrFact: 6, Documente: 5 }
            && s2 is { Baza: 2500m, Tva: 525m, NrFact: 6, Documente: 5 }
            && jum1.Operatiuni.Count == d4.Operatiuni.Count
            && jum2.Operatiuni.Count == d4.Operatiuni.Count);

        // ══════════ D4-V4: nimic nu se pierde — cusătura cu registrul ══════════
        // Sensul rândului `op1` e DATA din DTO (fix 5), nu deducerea din tip; coerența tip↔sens o garantează
        // gardul `TintaPermisa(tip, sens)` și se MĂSOARĂ aici pe fiecare rând.
        decimal OpBaza(SensTva s) => d4.Operatiuni.Where(o => o.Sens == s.ToString()).Sum(o => o.Baza);
        decimal OpTva(SensTva s) => d4.Operatiuni.Where(o => o.Sens == s.ToString()).Sum(o => (o.Tva ?? 0m) + o.TvaNedeclarat);
        decimal NeBaza(SensTva s) => d4.Neincluse.Where(n => n.Sens == s.ToString()).Sum(n => n.Baza);
        decimal NeTva(SensTva s) => d4.Neincluse.Where(n => n.Sens == s.ToString()).Sum(n => n.Tva);
        var brutCub = CubScena.FapteIntre(os, pStart, pEnd);
        decimal RegBazaCub(SensTva s) => brutCub.Where(f => f.Sens == s).Sum(f => f.Baza);
        decimal RegTvaCub(SensTva s) => brutCub.Where(f => f.Sens == s).Sum(f => f.Tva);
        Console.WriteLine($"     MĂSURAT (D4-V4): cub {brutCub.Count} fapte; achiziție Σ {RegBazaCub(SensTva.Achizitie):N2}/{RegTvaCub(SensTva.Achizitie):N2} "
            + $"= op1 {OpBaza(SensTva.Achizitie):N2}/{OpTva(SensTva.Achizitie):N2} + neincluse {NeBaza(SensTva.Achizitie):N2}/{NeTva(SensTva.Achizitie):N2}; "
            + $"livrare Σ {RegBazaCub(SensTva.Livrare):N2}/{RegTvaCub(SensTva.Livrare):N2} = {OpBaza(SensTva.Livrare):N2}/{OpTva(SensTva.Livrare):N2} + {NeBaza(SensTva.Livrare):N2}/{NeTva(SensTva.Livrare):N2}.");
        s.Check("D4-V4 (D4-D4) — NIMIC nu se pierde: Σ `Operatiuni` + Σ `Neincluse` == Σ faptelor fiscale din cub pe perioadă, PER SENS "
            + "(`Sens` din DTO), pe AMBELE coloane; și fiecare rând are tipul coerent cu sensul (L/V pe livrare, A/AI/C pe achiziție)",
            new[] { SensTva.Achizitie, SensTva.Livrare }.All(s =>
                OpBaza(s) + NeBaza(s) == RegBazaCub(s) && OpTva(s) + NeTva(s) == RegTvaCub(s))
            && d4.Operatiuni.All(o => (o.Sens == "Livrare") == (o.Tip is "L" or "V" or "LS"))
            && brutCub.Count > 0);
        s.Check("D4-V4 `Neincluse` cu cauza: DEC ⇒ `RepartitorNePartener` (angajatul, numit — 50 / 10,5 pe achiziție); SDD ⇒ "
            + "`TipTvaNemapat` (70 / 0); niciun `FaraPartener` pe scenă; partiția e completă (fiecare grup e ori "
            + "așezat, ori raportat)",
            d4.Neincluse.Count == 2
            && d4.Neincluse.Single(n => n.Cauza == "RepartitorNePartener") is
                { Sens: "Achizitie", TipTvaCod: "N21", Baza: 50m, Tva: 10.5m, Randuri: 1, RepartitorDenumire: "Titular decont probă D394" }
            && d4.Neincluse.Single(n => n.Cauza == "TipTvaNemapat") is
                { Sens: "Achizitie", TipTvaCod: "SDD", Baza: 70m, Tva: 0m, Cota: 0m, Randuri: 1, RepartitorId: null });

        // ══════════ D4-V5: cusătura cu D300 — două proiecții, același registru ══════════
        var d3 = D300Proiectii.D300(os, pStart, pEnd, null);
        D300Rand R(string cod) => d3.Randuri.Single(r => r.Cod == cod);
        decimal SumaOp(string tip, int cota, Func<D394Operatiune, decimal> col) =>
            d4.Operatiuni.Where(o => o.Tip == tip && o.Cota == cota).Sum(col);
        var neinclusDec = d4.Neincluse.Single(n => n.Cauza == "RepartitorNePartener");
        var neinclusSdd = d4.Neincluse.Single(n => n.Cauza == "TipTvaNemapat");
        Console.WriteLine($"     MĂSURAT (D4-V5): rd. 9 {R("9").Baza:N2}/{R("9").Tva:N2} vs Σ L/21 {SumaOp("L", 21, o => o.Baza):N2}/{SumaOp("L", 21, o => o.Tva ?? 0m):N2}; "
            + $"rd. 24 {R("24").Baza:N2}/{R("24").Tva:N2} vs Σ A+AI/21 {SumaOp("A", 21, o => o.Baza) + SumaOp("AI", 21, o => o.Baza):N2} + DEC {neinclusDec.Baza:N2}; "
            + $"rd. 12.1 {R("12.1").Baza:N2} vs C {SumaOp("C", 21, o => o.Baza):N2}; rd. 13 {R("13").Baza:N2} vs V {SumaOp("V", 0, o => o.Baza):N2}; rd. 29 {R("29").Baza:N2} vs SDD {neinclusSdd.Baza:N2}.");
        s.Check("D4-V5 cusătura cu D300 pe aceeași perioadă: rd. 9 (N21/livrare) == Σ L la 21% (bază ȘI TVA); rd. 12.1 == "
            + "Σ C la 21%; rd. 13 == Σ V (bază); rd. 10 == Σ L la 11%",
            R("9").Baza == SumaOp("L", 21, o => o.Baza) && R("9").Tva == SumaOp("L", 21, o => o.Tva ?? 0m)
            && R("12.1").Baza == SumaOp("C", 21, o => o.Baza) && R("12.1").Tva == SumaOp("C", 21, o => o.Tva ?? 0m)
            && R("13").Baza == SumaOp("V", 0, o => o.Baza)
            && R("10").Baza == SumaOp("L", 11, o => o.Baza) && R("10").Tva == SumaOp("L", 11, o => o.Tva ?? 0m)
            && R("9").Baza > 0m && R("12.1").Baza > 0m && R("13").Baza > 0m);
        s.Check("D4-V5 diferența legitimă e EXACT `Neincluse`: rd. 24 (N21 + NED21 pe achiziție) == Σ A + Σ AI la 21% + "
            + "decontul angajatului (`RepartitorNePartener`, pe ambele coloane); rd. 29 (scutite) == SDD-ul "
            + "`TipTvaNemapat` — ce D394 nu declară, D300 arată, și cifra e aceeași",
            R("24").Baza == SumaOp("A", 21, o => o.Baza) + SumaOp("AI", 21, o => o.Baza) + neinclusDec.Baza
            && R("24").Tva == SumaOp("A", 21, o => o.Tva ?? 0m) + SumaOp("AI", 21, o => o.Tva ?? 0m) + neinclusDec.Tva
            && R("29").Baza == neinclusSdd.Baza
            // …iar tipul de TVA de probă (5,5%) e nemapat în D300 și mapat în D394: singura pereche care apare doar într-una.
            && d3.Nemapate.Single().TipTvaCod == tvaC55.Cod && d3.Nemapate.Single().Baza == 100m);

        // ══════════ D4-V6: rezumatele, recalculate independent din op1 ══════════
        var rez = d4.Rezumat;
        bool RezumatCorect(D394Rezumat r) {
            var g = d4.Operatiuni.Where(o => o.TipPartener == r.TipPartener && o.Cota == r.Cota).ToList();
            int F(string t) => g.Where(o => o.Tip == t).Sum(o => o.NrFact);
            decimal B(string t) => g.Where(o => o.Tip == t).Sum(o => o.Baza);
            decimal T(string t) => g.Where(o => o.Tip == t).Sum(o => o.Tva ?? 0m);
            bool Col(int? f, decimal? b, decimal? t, bool prezent, string tip, bool cuTvaCol = true) =>
                prezent
                    ? f == F(tip) && b == B(tip) && (!cuTvaCol || t == T(tip))
                    : f == null && b == null && t == null;
            var cota0 = r.Cota == 0;
            return Col(r.FacturiL, r.BazaL, r.TvaL, !cota0, "L")
                && Col(r.FacturiLS, r.BazaLS, null, cota0, "LS", cuTvaCol: false)
                && Col(r.FacturiA, r.BazaA, r.TvaA, r.TipPartener == 1 && !cota0, "A")
                && Col(r.FacturiAI, r.BazaAI, r.TvaAI, r.TipPartener == 1 && !cota0, "AI")
                && Col(r.FacturiAS, r.BazaAS, null, r.TipPartener == 1 && cota0, "AS", cuTvaCol: false)
                && Col(r.FacturiV, r.BazaV, null, r.TipPartener == 1 && cota0, "V", cuTvaCol: false)
                && Col(r.FacturiC, r.BazaC, r.TvaC, r.TipPartener is 1 or 3 or 4 && !cota0, "C")
                && (r.TipPartener == 2 && cota0 ? r.FacturiN == 0 && r.BazaN == 0m : r.FacturiN == null && r.BazaN == null);
        }
        Console.WriteLine($"     MĂSURAT (D4-V6): rezumat1 {rez.Count} rânduri "
            + $"[{string.Join(", ", rez.Select(r => $"tip{r.TipPartener}/{r.Cota}"))}]; nrCui {d4.NrCui1}/{d4.NrCui2}/{d4.NrCui3}/{d4.NrCui4}; "
            + $"rezumat2 [{string.Join(", ", d4.RezumatCote.Select(c => $"{c.Cota}%: L {c.NrFacturiL}/{c.BazaL:N2}/{c.TvaL:N2}, A+C {c.NrFacturiA}/{c.BazaA:N2}/{c.TvaA:N2}, AI {c.NrFacturiAI}/{c.BazaAI:N2}"))}].");
        s.Check("D4-V6 rezumat1: un rând per (tip_partener, cotă) prezentă în op1 — tip1/0 (V), tip1/5, tip1/11, tip1/21, "
            + "tip2/21, tip3/21, tip4/21 — fiecare RECALCULAT independent din op1, cu NULL exact unde XSD-ul cere "
            + "absent (A/AI/C absente pe tipul 2, C prezent pe 3/4, V/AS doar pe tip1 × cota 0)",
            rez.Select(r => (r.TipPartener, r.Cota)).SequenceEqual([(1, 0), (1, 5), (1, 11), (1, 21), (2, 21), (3, 21), (4, 21)])
            && rez.All(RezumatCorect)
            && rez.Single(r => r is { TipPartener: 1, Cota: 0 }) is { FacturiV: 1, BazaV: 700m, FacturiA: null, BazaL: null, FacturiAS: 0 }
            && rez.Single(r => r is { TipPartener: 2, Cota: 21 }) is { FacturiL: 3, BazaL: 250m, TvaL: 52.5m, FacturiA: null, FacturiC: null, FacturiN: null }
            && rez.Single(r => r is { TipPartener: 3, Cota: 21 }) is { FacturiC: 0, BazaC: 0m, FacturiA: null }
            && rez.Single(r => r is { TipPartener: 1, Cota: 21 }) is { FacturiA: 6, BazaA: 1400m, FacturiAI: 1, BazaAI: 300m, TvaAI: 63m, FacturiC: 1, BazaC: 400m });
        s.Check("D4-V6 nrCui: tip 1 = 7 persoane DISTINCTE (F1 unit cu F1-bis, F2, F0 fără cod, C1, PFA, DE-RO, Grup unit pe "
            + "1590120); tip 2 = 3 ÎNREGISTRĂRI (PF cu CNP, fără CNP, cu CNP scurt); tip 3 = 1; tip 4 = 1",
            d4.NrCui1 == 7 && d4.NrCui2 == 3 && d4.NrCui3 == 1 && d4.NrCui4 == 1);
        s.Check("D4-V6 rezumat2 (cartușul H) per cotă ≠ 0: 21% ⇒ L = Σ L/21 pe toate tipurile de partener, A = Σ A + C, "
            + "AI separat; 11% și 5% cu L; fără rând pentru cota 0",
            d4.RezumatCote.Select(c => c.Cota).SequenceEqual([5, 11, 21])
            && d4.RezumatCote.Single(c => c.Cota == 21) is { BazaL: 3120m, TvaL: 655.2m, NrFacturiL: 13,
                BazaA: 1800m, TvaA: 378m, NrFacturiA: 7, BazaAI: 300m, NrFacturiAI: 1 }
            && d4.RezumatCote.Single(c => c.Cota == 11) is { BazaL: 990.91m, TvaL: 109m, NrFacturiL: 0, BazaA: 500m, NrFacturiA: 0 }
            && d4.RezumatCote.Single(c => c.Cota == 5) is { BazaL: 100m, TvaL: 5.5m, NrFacturiL: 1 });

        // ══════════ D4-V7: avertismentele — AGREGATE per cauză, cu exemple nominale ══════════
        var av = d4.Avertismente;
        D394Avertisment Av(string cod) => av.SingleOrDefault(a => a.Cod == cod);
        s.Check("D4-V7 avertismente per CAUZĂ (fix 7): un rând per cod — `Tip1FaraCui` (F0, numit, suma 90); `PfFaraCnp` ×2 "
            + "(gol ȘI „123”, numite; cea cu CNP de 13 cifre NU); `FaraOp11` cu ambele tipuri ca exemple (V 700, C 400, Σ 1100); "
            + "`CotaNeintreaga` (5,5 ⇒ 5); `CuiUnit` ×2 (12345678 cu F1 + F1-bis numite; 1590120); NIMIC altceva — fără "
            + "`TvaPeTipFaraColoana` pe date post-F13, fără `CombinatieRefuzata`, fără `ClasificariDiferite`, fără `PartenerInactiv`",
            Av("Tip1FaraCui") is { Numar: 1, Suma: 90m } t1 && t1.Exemple.Single().Contains("Furnizor tip 1 FĂRĂ cod fiscal")
            && Av("PfFaraCnp") is { Numar: 2 } pf2
                && pf2.Exemple.Any(e => e.Contains("Ionescu Maria") && e.Contains("cod gol"))
                && pf2.Exemple.Any(e => e.Contains("Georgescu Ana") && e.Contains("„123” nu are 13 cifre"))
                && !pf2.Exemple.Any(e => e.Contains("Popescu Ion"))
            && Av("FaraOp11") is { Numar: 2, Suma: 1100m } op11
                && op11.Exemple.Any(e => e.StartsWith("V: bază 700,00")) && op11.Exemple.Any(e => e.StartsWith("C: bază 400,00"))
            && Av("CotaNeintreaga") is { Numar: 1, Suma: null } cn && cn.Exemple.Single() == "5,5% ⇒ 5%"
            && Av("CuiUnit") is { Numar: 2, Suma: null } cu
                && cu.Exemple.Any(e => e.StartsWith("CUI 12345678") && e.Contains("Furnizor tip 1 (RO…)") && e.Contains("fără prefix"))
                && cu.Exemple.Any(e => e.StartsWith("CUI 1590120"))
            && av.All(a => !string.IsNullOrWhiteSpace(a.Mesaj) && a.Exemple.Count > 0 && a.Exemple.Count <= 5 && a.Numar >= a.Exemple.Count)
            && av.Select(a => a.Cod).Distinct().Count() == av.Count
            && av.Count == 5);

        // ══════════ Fix 6: partenerul INACTIV se declară (104f) ══════════
        // Facturile unui partener inactivat sunt documente operate și se declară;
        // proiecția emite `PartenerInactiv`, nu `RepartitorNePartener`; cusătura
        // cu registrul ține.
        var fSters = Part("-FS", "Furnizor inactiv", "RO44444444");
        os.CommitChanges();
        var fctSters = Cumparare("-AS", fSters, d10);
        LinieC(fctSters, n21, 30m);   // A/21 30 / 6,3
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctSters);
        os.CommitChanges();
        fSters.Activ = false;
        os.CommitChanges();
        var d4s = D394Proiectii.D394(os, pStart, pEnd);
        var opSters = d4s.Operatiuni.SingleOrDefault(o => o.CuiP == "44444444" && o.Tip == "A" && o.Cota == 21);
        Console.WriteLine($"     MĂSURAT (fix 6): partener inactiv ⇒ op1 {(opSters == null ? "<lipsă>" : $"tip{opSters.TipPartener} {opSters.Baza:N2}/{opSters.Tva:N2} „{opSters.Denumire}”")}; "
            + $"neincluse {d4s.Neincluse.Count}; avertismente [{string.Join(", ", d4s.Avertismente.Select(a => $"{a.Cod}×{a.Numar}"))}]; "
            + $"partener activ: {fSters.Activ}.");
        var brutS = CubScena.FapteIntre(os, pStart, pEnd);
        bool CusaturaS(SensTva sens) =>
            d4s.Operatiuni.Where(o => o.Sens == sens.ToString()).Sum(o => o.Baza) + d4s.Neincluse.Where(n => n.Sens == sens.ToString()).Sum(n => n.Baza)
                == brutS.Where(f => f.Sens == sens).Sum(f => f.Baza)
            && d4s.Operatiuni.Where(o => o.Sens == sens.ToString()).Sum(o => (o.Tva ?? 0m) + o.TvaNedeclarat) + d4s.Neincluse.Where(n => n.Sens == sens.ToString()).Sum(n => n.Tva)
                == brutS.Where(f => f.Sens == sens).Sum(f => f.Tva);
        s.Check("Fix 6 (review advers, 104f): partenerul INACTIV rămâne în op1 pe tip 1 "
            + "cu numele și CUI-ul lui (A/21 30 / 6,3), NU cade în `Neincluse/RepartitorNePartener` (cauza rămâne doar "
            + "decontul angajatului — tot 2 neincluse); avertisment `PartenerInactiv` ×1 cu numele; cusătura cu registrul "
            + "ține pe ambele sensuri și coloane",
            os.GetObjectsQuery<Partener>().Any(p => p.ID == fSters.ID && !p.Activ)
            && opSters is { TipPartener: 1, Baza: 30m, Tva: 6.3m, NrFact: 1, Denumire: "Furnizor inactiv" }
            && d4s.Neincluse.Count == 2 && d4s.Neincluse.Count(n => n.Cauza == "RepartitorNePartener") == 1
            && d4s.Avertismente.SingleOrDefault(a => a.Cod == "PartenerInactiv") is { Numar: 1 } psCub
                && psCub.Exemple.Single().Contains("Furnizor inactiv") && psCub.Exemple.Single().Contains("44444444")
            && d4s.Avertismente.Count == 6
            && CusaturaS(SensTva.Achizitie) && CusaturaS(SensTva.Livrare));

        // ══════════ D4-r14 (felia 16): `FaraOp11` tace acolo unde codul NC EXISTĂ ══════════
        // Jumătate din `op11` (codul NC) a intrat în model odată cu `Produs.CodNc`
        // (D16-D2), deci avertismentul se restrânge la LINIILE fără produs codificat.
        // Proba: o achiziție în taxare inversă pe un produs CU cod NC — baza ei intră
        // normal în `op1` (rândul C urcă), dar NU se mai adaugă la avertisment.
        var produsNc = os.CreateObject<Produs>();
        produsNc.Cod = MarcajD4 + "-PNC"; produsNc.Denumire = "Serviciu codificat NC";
        produsNc.TipMaterial = tip628; produsNc.CodNc = "87654321"; produsNc.UM = "BUC";
        os.CommitChanges();
        var fctNc = Cumparare("-CNC", f1, d10);
        var linNc = os.CreateObject<FacturaIntrareDetaliu>();
        linNc.Document = fctNc; linNc.TipMaterial = tip628; linNc.Cantitate = 1m;
        linNc.PretUnitar = 200m; linNc.TipTva = ti21; linNc.Produs = produsNc;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctNc);
        os.CommitChanges();
        var cuNc = D394Proiectii.D394(os, pStart, pEnd);
        var op11Nou = cuNc.Avertismente.SingleOrDefault(a => a.Cod == "FaraOp11");
        var cNou = cuNc.Operatiuni.Single(o => o.CuiP == "12345678" && o.Tip == "C" && o.Cota == 21);
        Console.WriteLine($"     MĂSURAT (D4-r14): C urcă la {cNou.Baza:N2}, iar `FaraOp11` rămâne "
            + $"Σ {op11Nou?.Suma:N2} [{string.Join(" | ", op11Nou?.Exemple ?? [])}].");
        s.Check("D4-r14 `FaraOp11` se restrânge la liniile FĂRĂ cod NC: achiziția în taxare inversă pe un produs cu "
            + "`CodNc` urcă rândul C de la 400 la 600, dar avertismentul rămâne pe 400 (V 700 + C 400 = 1.100) — "
            + "jumătatea de detaliu pe care modelul o are nu se mai reclamă; categoria de bunuri rămâne restanța D4-r5",
            cNou.Baza == 600m
            && op11Nou is { Numar: 2, Suma: 1100m }
            && op11Nou.Exemple.Any(e => e.StartsWith("V: bază 700,00") && e.Contains("fără cod NC"))
            && op11Nou.Exemple.Any(e => e.StartsWith("C: bază 400,00"))
            && op11Nou.Mesaj.Contains("cod NC"));

        // ---------------- Curățenie ----------------
        CurataD4();
        s.Check("Curățenie finală felia D394 (fără reziduuri e2e: repartitori, documente, lot, produs, tipul de TVA de probă "
            + "și maparea lui — politica de profil rămâne la 13 mapări)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajD4))
            && !os.GetObjectsQuery<Document>().Any(d => d.Numar.StartsWith(MarcajD4))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajD4))
            && !os.GetObjectsQuery<TipTva>().Any(t => t.Cod.StartsWith(MarcajD4))
            && CubScena.FapteIntre(os, pStart, pEnd).Count == 0
            && os.GetObjectsQuery<MapareD394>().Count() == 13);
    }
}
