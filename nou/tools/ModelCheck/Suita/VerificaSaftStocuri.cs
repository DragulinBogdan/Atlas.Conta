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

// ============ Felia 17, pas 2: regulile + proiecția S — D17-V2 ============
// Ce probează: (a) funcțiile PURE ale legii pentru stocuri (`SaftReguli`,
// D17-D2), fără bază — deci identic pe ambele profiluri; (b) proiecția
// `SaftProiectii.SaftStocuriPeCub` pe o scenă privată care atinge FIECARE tip de
// document care mișcă stoc (NIR ← FCT, BTR, BCS, LDI ±, FCL + DSC, ASM, RLF,
// RDC), cu deschidere pe loturi × gestiuni, cu DOUĂ stornouri (unul în aceeași
// lună, unul peste lună) și cu un produs FĂRĂ cont de stoc, cu CUSĂTURILE
// S1–S4; (c) `Neaplicabil` pe profilul bugetar.
//
// Luna scenei e MARTIE 2027 — și asta e o alegere, nu o preferință: fișierul S
// declară `PhysicalStock` pe TOATĂ baza, iar `MovementOfGoods` pe luna cerută.
// Perioadele fiscale seed-uite acoperă doar 2026, deci 2027 e o lună în care
// NICIUN alt scenariu al suitei n-a scris vreun document — cifrele de mai jos
// sunt ale scenei, nu ale reziduului. Perioadele 02 și 03/2027 le creează scena
// și le purjează la final.
static class VerificaSaftStocuri {
    public static void Ruleaza(Suita suita, bool privat) {
        const string Marcaj = "E2E-SAFT-S";
        using var os = suita.Provider.CreateObjectSpace();

        const int an = 2027, luna = 3;
        var pStart = new DateOnly(an, luna, 1);
        var pEnd = new DateOnly(an, luna, 31);
        var dataCreare = new DateOnly(2027, 4, 5);

        // ══════════ D17-V2 (a): funcțiile pure ale legii ══════════
        var terti = SaftReguli.TertiLinieStoc(RolTertSaft.Client, "0012345674", "0011111111");
        var tertiFurnizor = SaftReguli.TertiLinieStoc(RolTertSaft.Furnizor, "0012345674", "0011111111");
        var tertiIntern = SaftReguli.TertiLinieStoc(RolTertSaft.Niciunul, "0012345674", "0011111111");
        var tertiFaraPartener = SaftReguli.TertiLinieStoc(RolTertSaft.Client, null, "0011111111");
        suita.Check("D17-V2 `TertiLinieStoc` — convenția S (xlsx SD.MG.21/22) e ALTA decât a lui L: livrarea pune "
            + "`CustomerID` = partenerul și `SupplierID` = „0”, achiziția simetric, iar mișcarea internă pune "
            + "AMBELE = raportorul (regula validatorului: nu pot fi ambele „0”). Rol CERUT dar partener LIPSĂ ⇒ "
            + "tot raportorul pe ambele — apelantul pune avertismentul, funcția nu inventează un identificator",
            terti == ("0012345674", "0") && tertiFurnizor == ("0", "0012345674")
            && tertiIntern == ("0011111111", "0011111111")
            && tertiFaraPartener == ("0011111111", "0011111111")
            && SaftReguli.TertiLinieStoc(RolTertSaft.Niciunul, null, null) == (null, null)
            && SaftReguli.TertNeaplicabil == "0");
        suita.Check("D17-V2 `OwnerIdRaportor` = `00`+CUI, ADICĂ identificatorul de partener al raportorului "
            + "(`IdSocietate`) — un nume nou pentru aceeași regulă, nu o a doua implementare; `Owners` rămâne "
            + "gol cât timp tot stocul e al lui (ghid p. 36)",
            SaftReguli.OwnerIdRaportor("RO12345674", "RO") == "0012345674"
            && SaftReguli.OwnerIdRaportor("RO12345674", "RO") == SaftReguli.IdSocietate("RO12345674", "RO")
            && SaftReguli.OwnerIdRaportor(null, "RO") == null);
        suita.Check("D17-V2 `ProductTypeDinCont` — simbolul contului de stoc, fără puncte, tăiat la 18 "
            + "(`SAFshorttextType`); fără cont ⇒ „0”, valoarea de rezervă a stocului fizic (pe MIȘCĂRI aceeași "
            + "gaură scoate linia din fișier, `AccountID` nu se inventează); `StockCharacteristic` = („0”,„0”) "
            + "cât timp modelul n-are axa accizelor",
            SaftReguli.ProductTypeDinCont("371") == "371"
            && SaftReguli.ProductTypeDinCont("302.01.00") == "3020100"
            && SaftReguli.ProductTypeDinCont(null) == "0" && SaftReguli.ProductTypeDinCont("  ") == "0"
            && SaftReguli.ProductTypeDinCont("1234567890.1234567890").Length == 18
            && SaftReguli.StockCharacteristic == ("0", "0"));
        var refSimpla = SaftReguli.MovementReference("BCS", "BCS-000123", null, false);
        var refSpartă = SaftReguli.MovementReference("ASM", "ASM-000045", "70", false);
        var refStorno = SaftReguli.MovementReference("LDI", "LDI-000007", "110", true);
        var refLunga = SaftReguli.MovementReference("FCL", new string('9', 60), "30", true);
        suita.Check("D17-V2 `MovementReference` ≤ 35: `{CodTip}-{Numar}`, `/{cod}` DOAR când documentul se sparge "
            + "(ASM ⇒ două mișcări), `/S` pe jumătatea de storno — sufixele sunt cele care fac referința UNICĂ "
            + "per (Document × Storno × cod). Peste limită se taie NUMĂRUL de la ÎNCEPUT (coada distinge, "
            + "prefixul de serie se repetă) și se RAPORTEAZĂ; funcția nu aruncă niciodată",
            refSimpla == ("BCS-BCS-000123", false)
            && refSpartă == ("ASM-ASM-000045/70", false)
            && refStorno == ("LDI-LDI-000007/110/S", false)
            && refLunga.Trunchiat && refLunga.Referinta.Length == 35
            && refLunga.Referinta.StartsWith("FCL-") && refLunga.Referinta.EndsWith("/30/S")
            && SaftReguli.LungimeMovementReference == 35);

        // ══════════ D17-V2 (c): bugetarul — `Neaplicabil`, fără nicio interogare ══════════
        if (!privat) {
            var gol = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
            Console.WriteLine($"     MĂSURAT (D17-V2 bugetar): „{gol.Neaplicabil}”.");
            suita.Check("D17-V2 (bugetar) declarația de STOCURI e la fel de neaplicabilă ca lunarul (73c): planul "
                + "instituțiilor publice nu e printre cele 12 `TaxAccountingBasis`, iar profilul n-are nicio "
                + "politică de mișcare. DTO GOL cu motiv, oprit ÎNAINTE de orice interogare pe registre",
                gol.Neaplicabil != null && gol.Neaplicabil.Contains("bugetar") && gol.Neaplicabil.Contains("D406 S")
                && gol.Header == null && gol.MiscariStoc.Count == 0 && gol.StocFizic.Count == 0
                && gol.TipuriMiscare.Count == 0 && gol.Excluse.Count == 0 && gol.Neincluse.Count == 0
                && gol.Avertismente.Count == 0 && gol.Conturi.Count == 0
                && gol.Rezumat.NumarMiscari == 0 && gol.Rezumat.RanduriRegistruStoc == 0);
            VerificaSaftJson.Ruleaza(suita, gol, "stocuri bugetar, Neaplicabil");
            return;
        }

        // ══════════ D17-V2 (b): scena privată ══════════
        var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");
        var clasaMf = os.FirstOrDefault<ClasaProdus>(c => c.Cod == "MF");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var judetCj = os.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
        var umBucata = os.FirstOrDefault<UnitateMasura>(u => u.Cod == "H87");

        var societate = os.GetObjectsQuery<Societate>().First();
        var socInainte = (societate.Denumire, societate.CodFiscal, societate.InregistratTva, societate.Tara,
            societate.Strada, societate.Numar, societate.Localitate, societate.CodPostal, societate.JudetId,
            societate.ContactNume, societate.ContactPrenume, societate.Telefon, societate.Email,
            societate.ContBancarId, societate.RaporteazaCnp);
        var ibanInainte = banca.Iban;

        void Curata() {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => d.Numar.StartsWith(Marcaj)
                    || repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).Distinct().ToList();
            var loturi = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
            DeschidereScena.Curata(os, pj, loturi);
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => ids.Contains(i.DocumentId) || ids.Contains(i.DocumentStingatorId)).ToList());
            // Rândurile de DESCHIDERE n-au document: se prind pe lot.
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
                         .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).ToList());
            var tipuriScena = os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)).Select(t => t.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.DinSeed && r.TipMaterialId != null && tipuriScena.Contains(r.TipMaterialId.Value)));
            pj.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => tipuriScena.Contains(t.ID)));
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            // Perioadele 2027 sunt artefact de scenă (seed-ul acoperă doar 2026).
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.An == an).ToList());
            pj.Executa();
        }
        void RestaureazaSocietatea() {
            var s = os.GetObjectsQuery<Societate>().First();
            (s.Denumire, s.CodFiscal, s.InregistratTva, s.Tara, s.Strada, s.Numar, s.Localitate, s.CodPostal,
                s.JudetId, s.ContactNume, s.ContactPrenume, s.Telefon, s.Email, s.ContBancarId, s.RaporteazaCnp) = socInainte;
            os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA").Iban = ibanInainte;
            os.CommitChanges();
        }
        Curata();

        suita.Check("D17-V2 premisă: luna scenei e goală de mișcări de stoc înainte de scenă (2027 e în afara "
            + "perioadelor fiscale seed-uite, deci niciun alt scenariu al suitei n-a scris acolo) — altfel "
            + "cifrele exacte de mai jos ar fi măsurate peste conținut străin",
            CubScena.MiscariIntre(os, pStart, pEnd, cuDocument: true) == 0);

        // ---------------- Perioadele fiscale ale scenei ----------------
        foreach (var l in new[] { 2, 3 }) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = an; p.Luna = l; p.Inchisa = false;
        }

        // ---------------- Societatea raportoare ----------------
        banca.Iban = "RO49AAAA1B31007593840000";
        societate.Denumire = "Atlas Probă SAF-T S SRL";
        societate.CodFiscal = "12345674";
        societate.InregistratTva = true;
        societate.Tara = "RO";
        societate.Strada = "Str. Stocului";
        societate.Numar = "3";
        societate.Localitate = "Cluj-Napoca";
        societate.CodPostal = "400000";
        societate.Judet = judetCj;
        societate.ContactNume = "Popescu";
        societate.ContactPrenume = "Ion";
        societate.Telefon = "0264000000";
        societate.Email = "probe@atlas.test";
        societate.ContBancar = banca;
        societate.RaporteazaCnp = false;
        os.CommitChanges();
        var idRaportor = SaftReguli.IdSocietate(societate.CodFiscal, societate.Tara);

        // ---------------- Nomenclatoarele scenei ----------------
        Partener Part(string sufix, string denumire, string cui) {
            var p = os.CreateObject<Partener>();
            p.Cod = Marcaj + sufix; p.Denumire = denumire; p.CodFiscal = cui;
            p.TipPersoana = TipPersoana.Juridica; p.Tara = "RO"; p.InregistratTva = true;
            p.Strada = "Str. Partenerului"; p.Numar = "2"; p.Localitate = "Cluj-Napoca";
            p.CodPostal = "400001"; p.Judet = judetCj;
            return p;
        }
        var furnizor = Part("-FURN", "Furnizor SAF-T S SRL", "RO33333338");
        var client = Part("-CL", "Client SAF-T S SRL", "22222229");

        var mag1 = os.CreateObject<Gestiune>();
        mag1.Cod = Marcaj + "-MAG1"; mag1.Denumire = "Magazia 1 SAF-T S";
        var mag2 = os.CreateObject<Gestiune>();
        mag2.Cod = Marcaj + "-MAG2"; mag2.Denumire = "Magazia 2 SAF-T S";
        var locConsum = os.CreateObject<UnitateInterna>();
        locConsum.Cod = Marcaj + "-LOC"; locConsum.Denumire = "Loc de consum SAF-T S";
        locConsum.Calitati = CalitateRepartitor.LocConsum;
        var comisie = os.CreateObject<UnitateInterna>();
        comisie.Cod = Marcaj + "-COM"; comisie.Denumire = "Comisie de inventar SAF-T S";
        comisie.Calitati = CalitateRepartitor.Comisie;
        var codEconomic = os.CreateObject<CodEconomic>();
        codEconomic.Cod = Marcaj + "-CE"; codEconomic.Denumire = "Cod economic de probă SAF-T S";

        // D17: contul temporar permite operarea; lipsa lui ulterioară probează raportul incomplet.
        var tipFaraCont = os.CreateObject<TipMaterial>();
        tipFaraCont.Cod = Marcaj + "-TIP"; tipFaraCont.Denumire = "Tip fără cont de stoc SAF-T S";
        tipFaraCont.Clasa = clasaMf; tipFaraCont.ContImplicit = null;

        Produs Prod(string sufix, string denumire, string codNc, UnitateMasura um, string umText, TipMaterial tip) {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + sufix; p.Denumire = denumire; p.UM = umText;
            p.TipMaterial = tip; p.CodNc = codNc; p.UnitateMasura = um;
            return p;
        }
        // Codul NC e REAL (`01012100`): validatorul îl caută în nomenclatorul NC8.
        var produsA = Prod("-A", "Marfă SAF-T S A", "01012100", umBucata, "BUC", tip371);
        var produsB = Prod("-B", "Marfă SAF-T S B", null, null, "navete", tip371);
        var produsKit = Prod("-KIT", "Kit SAF-T S", "01012100", umBucata, "BUC", tip371);
        var produsFaraCont = Prod("-FC", "Marfă fără cont de stoc", "01012100", umBucata, "BUC", tipFaraCont);
        // Produs din NOMENCLATOR care nu se mișcă și n-are stoc: `Products` declară
        // doar ce apare în fișier, iar absența lui e chiar proba.
        var produsNefolosit = Prod("-NEF", "Marfă nefolosită SAF-T S", "01012100", umBucata, "BUC", tip371);
        os.CommitChanges();

        var pozitiiInitiale = new List<(Lot Lot, Guid Gestiune, decimal Cantitate, decimal Valoare)>();
        Lot Deschidere(Produs produs, Gestiune gestiuneLot, decimal pret, DateOnly data,
            params (Gestiune Gestiune, decimal Cantitate)[] solduri) {
            var lot = os.CreateObject<Lot>();
            lot.Produs = produs; lot.PretUnitar = pret; lot.Gestiune = gestiuneLot; lot.Data = data;
            foreach (var sold in solduri)
                pozitiiInitiale.Add((lot, sold.Gestiune.ID, sold.Cantitate, sold.Cantitate * pret));
            return lot;
        }
        var dDeschidere = new DateOnly(an - 1, 12, 31);
        // Lotul A1 stă în AMBELE gestiuni (deschiderea nu e legată de `Lot.Gestiune`):
        // exact cazul care cere `StockAccountNo` în magazia a doua.
        var lotA1 = Deschidere(produsA, mag1, 10m, dDeschidere, (mag1, 100m), (mag2, 10m));
        var lotA2 = Deschidere(produsA, mag2, 12m, dDeschidere, (mag2, 50m));
        var lotB1 = Deschidere(produsB, mag1, 20m, dDeschidere, (mag1, 40m));
        var lotF1 = Deschidere(produsFaraCont, mag1, 5m, dDeschidere, (mag1, 30m));
        tipFaraCont.ContImplicitId = tip371.ContImplicitId;
        DeschidereScena.Scrie(os, dDeschidere, pozitiiInitiale.ToArray());
        tipFaraCont.ContImplicitId = null;
        os.CommitChanges();

        // ---------------- Luna ANTERIOARĂ: un BTR care se stornează în luna curentă ----------------
        var btrAnterior = os.CreateObject<NotaTransfer>();
        btrAnterior.Numar = Marcaj + "-BTR-FEB"; btrAnterior.Data = new DateOnly(an, 2, 10);
        btrAnterior.Predator = mag1; btrAnterior.Primitor = mag2; btrAnterior.NumarPV = Marcaj;
        var linBtrAnterior = os.CreateObject<DocumentDetaliu>();
        linBtrAnterior.Document = btrAnterior; linBtrAnterior.TipMaterial = tip371;
        linBtrAnterior.Lot = lotA1; linBtrAnterior.Cantitate = 2m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, btrAnterior);
        os.CommitChanges();

        // ---------------- Documentele lunii ----------------
        // FCT (03) ⇒ NIR conex: recepția contează pe NIR (26a), deci mișcarea de stoc
        // e a NIR-ului, iar partenerul lui vine de pe laturile clonate din factură.
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = Marcaj + "-FCT"; fct.Data = new DateOnly(an, luna, 3);
        fct.Predator = furnizor; fct.Primitor = mag1;
        var linFct = os.CreateObject<FacturaIntrareDetaliu>();
        linFct.Document = fct; linFct.TipMaterial = tip371; linFct.Produs = produsA;
        linFct.Cantitate = 10m; linFct.PretUnitar = 30m; linFct.TipTva = n21;
        var lotFct = linFct.CreeazaLot(os, produsA, mag1);
        os.CommitChanges();
        var nirConex = MotorOperare.Opereaza(os, fct);
        os.CommitChanges();
        if (nirConex != null) {
            MotorOperare.Opereaza(os, nirConex);
            os.CommitChanges();
        }
        suita.Check("D17-V2 premisă de scenă: FCT a generat NIR-ul conex OPERAT, iar lotul mărfii s-a finalizat la "
            + "30 lei/buc (mișcarea de stoc e a NIR-ului, nu a facturii — 26a)",
            nirConex is NIR { Stare: StareDocument.Operat } && lotFct.PretUnitar == 30m);

        // BTR (05): transfer intern, fără terț.
        var btr = os.CreateObject<NotaTransfer>();
        btr.Numar = Marcaj + "-BTR"; btr.Data = new DateOnly(an, luna, 5);
        btr.Predator = mag1; btr.Primitor = mag2; btr.NumarPV = Marcaj;
        var linBtr = os.CreateObject<DocumentDetaliu>();
        linBtr.Document = btr; linBtr.TipMaterial = tip371; linBtr.Lot = lotA1; linBtr.Cantitate = 5m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, btr);
        os.CommitChanges();

        // BCS (06): −Marfuri (cod 70) + `Consum` (EXCLUS deliberat).
        var bcs = os.CreateObject<BonConsum>();
        bcs.Numar = Marcaj + "-BCS"; bcs.Data = new DateOnly(an, luna, 6);
        bcs.Predator = mag1; bcs.Primitor = locConsum;
        var linBcs = os.CreateObject<DocumentDetaliu>();
        linBcs.Document = bcs; linBcs.TipMaterial = tip371; linBcs.Lot = lotB1; linBcs.Cantitate = 4m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, bcs);
        os.CommitChanges();

        // LDI (08): plus (110) + minus (120) — un document, DOUĂ coduri.
        var ldi = os.CreateObject<ListaDiferenteInventar>();
        ldi.Numar = Marcaj + "-LDI"; ldi.Data = new DateOnly(an, luna, 8);
        ldi.Predator = mag1; ldi.Primitor = comisie;
        var linLdiMinus = os.CreateObject<ListaDiferenteInventarDetaliu>();
        linLdiMinus.Document = ldi; linLdiMinus.TipMaterial = tip371;
        linLdiMinus.Directie = DirectieDiferenta.Minus; linLdiMinus.Lot = lotB1; linLdiMinus.Cantitate = 2m;
        var linLdiPlus = os.CreateObject<ListaDiferenteInventarDetaliu>();
        linLdiPlus.Document = ldi; linLdiPlus.TipMaterial = tip371;
        linLdiPlus.Directie = DirectieDiferenta.Plus; linLdiPlus.Cantitate = 3m;
        linLdiPlus.PretEvaluare = 7m; linLdiPlus.CodEconomic = codEconomic;
        var lotLdiPlus = linLdiPlus.CreeazaLot(os, produsA, mag1);
        os.CommitChanges();
        MotorOperare.Opereaza(os, ldi);
        os.CommitChanges();

        // FCL (10) ⇒ DSC conex: ieșirea din patrimoniu e a descărcării (37a).
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Data = new DateOnly(an, luna, 10);
        fcl.Predator = sediu; fcl.Primitor = client; fcl.GestiuneDescarcare = mag1;
        var linFcl = os.CreateObject<FacturaIesireDetaliu>();
        linFcl.Document = fcl; linFcl.TipMaterial = tip371; linFcl.Produs = produsA;
        linFcl.Cantitate = 3m; linFcl.PretUnitar = 50m; linFcl.TipTva = n21;
        os.CommitChanges();
        var dsc = MotorOperare.Opereaza(os, fcl);
        os.CommitChanges();
        if (dsc != null) {
            MotorOperare.Opereaza(os, dsc);
            os.CommitChanges();
        }

        // D17: configurația fără cont este degradată după operare; ASM-B4 refuză operarea fără cont.
        AsamblareDetaliu LinieAsm(Asamblare doc, DirectieAsamblare directie, decimal cantitate,
            TipMaterial tip, Lot lot = null, decimal? pretEvaluare = null, Produs produsNou = null) {
            var d = os.CreateObject<AsamblareDetaliu>();
            d.Document = doc; d.TipMaterial = tip; d.Directie = directie; d.Cantitate = cantitate;
            d.PretEvaluare = pretEvaluare;
            if (lot != null) d.Lot = lot;
            if (produsNou != null) d.CreeazaLot(os, produsNou, mag1);
            return d;
        }
        var asm = os.CreateObject<Asamblare>();
        asm.Numar = Marcaj + "-ASM"; asm.Data = new DateOnly(an, luna, 12);
        asm.Predator = mag1; asm.Primitor = mag1;
        LinieAsm(asm, DirectieAsamblare.Consum, 4m, tip371, lotA1);
        LinieAsm(asm, DirectieAsamblare.Consum, 3m, tipFaraCont, lotF1);
        var linAsmProdus = LinieAsm(asm, DirectieAsamblare.Produs, 2m, tip371,
            pretEvaluare: 27.5m, produsNou: produsKit);
        var lotKit = linAsmProdus.Lot;
        os.CommitChanges();
        var refuzFaraCont = false;
        using (var proba = suita.Provider.CreateObjectSpace()) {
            try { ComenziDocument.Sistem(proba).Opereaza(asm.ID); }
            catch (OperareException e) { refuzFaraCont = e.Message.Contains("CONT_STOC_LIPSA"); }
        }
        suita.Check("D17/ASM: lipsa contului refuză atomic operarea; degradarea nomenclatorului se probează după operare",
            refuzFaraCont
            && !os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>().Any(p => p.DocumentId == asm.ID));
        tipFaraCont.ContImplicitId = tip371.ContImplicitId;
        os.CommitChanges();
        MotorOperare.Opereaza(os, asm);
        tipFaraCont.ContImplicitId = null;
        os.CommitChanges();

        // RLF (14): retur la furnizor — cod 50, rol Furnizor.
        var rlf = os.CreateObject<ReturFurnizor>();
        rlf.Data = new DateOnly(an, luna, 14);
        rlf.Predator = mag1; rlf.Primitor = furnizor;
        var linRlf = os.CreateObject<DocumentDetaliu>();
        linRlf.Document = rlf; linRlf.TipMaterial = tip371; linRlf.Lot = lotB1;
        linRlf.Cantitate = 2m; linRlf.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rlf);
        os.CommitChanges();

        // RDC (16): retur de la client — cod 40, rol Client; costul revine pe lotul
        // ORIGINAL, venitul e linie de factură (fără stoc).
        var rdc = os.CreateObject<ReturClient>();
        rdc.Data = new DateOnly(an, luna, 16);
        rdc.Predator = client; rdc.Primitor = mag1;
        var linRdcVenit = os.CreateObject<DocumentDetaliu>();
        linRdcVenit.Document = rdc; linRdcVenit.TipMaterial = tip707;
        linRdcVenit.Valoare = 50m; linRdcVenit.TipTva = n21;
        var linRdcCost = os.CreateObject<DocumentDetaliu>();
        linRdcCost.Document = rdc; linRdcCost.TipMaterial = tip371;
        linRdcCost.Lot = lotA1; linRdcCost.Cantitate = 1m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rdc);
        os.CommitChanges();


        // Cele DOUĂ stornouri: unul peste lună (documentul e din februarie, rândurile
        // inverse cad în martie) și unul în ACEEAȘI lună.
        MotorOperare.Storneaza(os, btrAnterior, new DateOnly(an, luna, 20));
        os.CommitChanges();
        MotorOperare.Storneaza(os, ldi, new DateOnly(an, luna, 25));
        os.CommitChanges();

        // ══════════ Proiecția ══════════
        var cronometru = Stopwatch.StartNew();
        var saft = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var msProiectie = cronometru.Elapsed.TotalMilliseconds;
        var rez = saft.Rezumat;
        Console.WriteLine($"     MĂSURAT (D17-V2, {luna:00}.{an}): {rez.NumarMiscari} mișcări / "
            + $"{rez.NumarLiniiMiscare} linii peste {rez.RanduriRegistruStoc} rânduri de registru; "
            + $"{rez.NumarStocFizic} intrări de stoc fizic; {rez.NumarTipuriMiscare} tipuri de mișcare "
            + $"[{string.Join(", ", saft.TipuriMiscare.Select(t => t.Cod))}]; {saft.Produse.Count} produse, "
            + $"{saft.Unitati.Count} UM, {saft.Conturi.Count} conturi, {saft.Taxe.Count} coduri de taxă, "
            + $"{saft.TipuriAnaliza.Count} tipuri de analiză; recepționat {saft.TotalQuantityReceived:0.###} / "
            + $"eliberat {saft.TotalQuantityIssued:0.###}; proiecția {msProiectie:N0} ms.");
        Console.WriteLine($"     MĂSURAT (D17-V2 cusături): S1 {rez.StocIntrariDiferite}/{rez.StocIntrari} intrări "
            + $"diferite, S5 (vs FIȘIER) {rez.StocFizicVsMiscariDiferite}/{rez.StocIntrari} — emise "
            + $"{rez.StocEmiseCantitate:0.###}/{rez.StocEmiseValoare:N2}; "
            + $"(deschidere {rez.StocOpeningCantitate:0.###}/{rez.StocOpeningValoare:N2} + mișcări "
            + $"{rez.StocMiscariCantitate:0.###}/{rez.StocMiscariValoare:N2} = închidere "
            + $"{rez.StocClosingCantitate:0.###}/{rez.StocClosingValoare:N2}); S2 mișcări {rez.MiscariValoare:N2} + "
            + $"excluse {rez.ExcluseValoare:N2} + neincluse {rez.NeincluseStocValoare:N2} vs registru "
            + $"{rez.RegistruStocValoare:N2} (cantitate {rez.MiscariCantitate:0.###} + {rez.ExcluseCantitate:0.###} + "
            + $"{rez.NeincluseStocCantitate:0.###} vs {rez.RegistruStocCantitate:0.###}); S3 "
            + $"{rez.ConturiStocDiferite}/{rez.ConturiStocVerificate} conturi diferite (stoc "
            + $"{rez.ClosingStocFizic:N2} vs balanță {rez.ClosingBalantaStoc:N2}); S4 produse "
            + $"{rez.ProduseLipsa}/{rez.ProduseReferite}, coduri {rez.CoduriMiscareLipsa}/"
            + $"{rez.CoduriMiscareFolosite}, identități invalide {rez.IdentitatiTertInvalide}.");
        foreach (var c in rez.StocPerCont)
            Console.WriteLine($"         S3 CONT {c.Cont}: stoc {c.ClosingStocFizic:N2} vs balanță "
                + $"{c.ClosingBalanta:N2} ⇒ diferență {c.Diferenta:N2}");
        foreach (var x in saft.Excluse)
            Console.WriteLine($"         EXCLUS {x.TipDocument}/{x.TipStoc}/{x.Semn?.ToString() ?? "±"} ×{x.Numar} "
                + $"{x.Cantitate:0.###}/{x.Valoare:N2}: {x.Motiv}");
        foreach (var n in saft.Neincluse)
            Console.WriteLine($"         NEINCLUS {n.Cauza} [{n.Sectiune}] {n.DocumentTip} {n.DocumentNumar} "
                + $"{n.ProdusCod} {n.TipStoc}/{n.Semn?.ToString() ?? "±"} ×{n.Randuri} "
                + $"{n.Cantitate:0.###}/{n.Valoare:N2}");
        foreach (var a in saft.Avertismente)
            Console.WriteLine($"         AVERTISMENT {a.Cod} ×{a.Numar}{(a.Suma is decimal s ? $" Σ {s:N2}" : "")}: "
                + $"{string.Join(" | ", a.Exemple)}");

        // ---------------- D17-V1: politica acoperă mișcările pe care cubul le produce (D9-D8) ----------------
        {
            var primaZi = new DateOnly(an, luna, 1);
            var ultimaZi = primaZi.AddMonths(1).AddDays(-1);
            var postariStoc = os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => p.Spatiu == N.Spatiu.Stoc && p.DocumentId != null && p.Data >= primaZi && p.Data <= ultimaZi)
                .Select(p => new { DocumentId = p.DocumentId.Value, p.Cont }).ToList();
            var coduri = CititorTipDocument.Coduri(os, postariStoc.Select(p => p.DocumentId).Distinct().ToList());
            var categorii = new CategoriiStoc(os);
            var produse = postariStoc
                .Select(p => (Tip: coduri.GetValueOrDefault(p.DocumentId), Categorie: categorii.Rezolva(p.Cont)))
                .Where(p => p.Categorie is TipStoc c && CategoriiStoc.Rol(c) != RolCategorieStoc.Neacoperita)
                .Select(p => (p.Tip, TipStoc: p.Categorie.Value)).Distinct().ToList();
            var politici = os.GetObjectsQuery<PoliticaMiscareSaft>().Select(p => new { Tip = p.TipDocument.Cod, p.TipStoc })
                .ToList().Select(p => (p.Tip, p.TipStoc)).Distinct().ToList();
            var neacoperite = produse.Except(politici).ToList();
            var tipuriStoc = new[] { "FCT", "BTR", "BCS", "LDI", "DSC", "ASM", "RLF", "RDC" };
            var tipuriScena = produse.Select(p => p.Tip).Distinct().ToList();
            string Lista(IEnumerable<(string Tip, TipStoc TipStoc)> x) =>
                string.Join(", ", x.OrderBy(p => p.Tip, StringComparer.Ordinal).ThenBy(p => p.TipStoc).Select(p => $"{p.Tip}/{p.TipStoc}"));
            Console.WriteLine($"     MĂSURAT (D17-V1/acoperire pe cub): {produse.Count} perechi (tip × categorie) produse de "
                + $"postările de stoc ale lunii [{Lista(produse)}]; fără politică [{Lista(neacoperite)}]; politici fără "
                + $"mișcare în scenă [{Lista(politici.Except(produse))}].");
            suita.Check("D17-V1 (privat) politica acoperă FIECARE pereche (tip × categorie de stoc) pe care cubul o produce "
                + "pe cele opt tipuri ale scenei (FCT, BTR, BCS, LDI, DSC, ASM, RLF, RDC; NIR-ul conex egal nu mișcă, SAF-B5)",
                neacoperite.Count == 0 && tipuriStoc.All(tipuriScena.Contains));
        }

        // ---------------- Serializarea JSON + sumarul ----------------
        VerificaSaftJson.Ruleaza(suita, saft, $"stocuri privat, {luna:00}.{an}");
        var sumar = SaftProiectii.Sumar(saft);
        suita.Check("D17-V2 `Sumar` numără și secțiunile S: mișcări, linii, stoc fizic, tipuri de mișcare — iar "
            + "`Excluse` trece ÎNTREG (e mărginit de numărul de POLITICI, nu de volumul registrului) și e "
            + "chiar partea pe care omul o citește înainte de a depune",
            sumar.MiscariStoc == saft.MiscariStoc.Count
            && sumar.LiniiMiscare == saft.MiscariStoc.Sum(m => m.Linii.Count)
            && sumar.StocFizic == saft.StocFizic.Count && sumar.TipuriMiscare == saft.TipuriMiscare.Count
            && ReferenceEquals(sumar.Excluse, saft.Excluse)
            && sumar.MiscariStoc == rez.NumarMiscari && sumar.LiniiMiscare == rez.NumarLiniiMiscare
            && sumar.StocFizic == rez.NumarStocFizic && sumar.TipuriMiscare == rez.NumarTipuriMiscare);

        // ---------------- F20-D5 (S): agregatul se închide pe cusătura S2 ----------------
        // Cusătura generică (Σ agregat == Σ lista plată) e în `VerificaSaftJson`,
        // rulată pentru AMBELE module. Aici e partea care ține DOAR de S și e chiar
        // motivul definiției semnate a lui `Suma`: cifrele agregatului sunt aceiași
        // termeni pe care S2 îi pune în ecuație — deci ecranul care citește sumarul
        // și cusătura care apără fișierul vorbesc despre aceleași bani.
        suita.Check("F20-D5 (S, pe cub) S nu mai are `Neincluse`: golurile de politică, cont sau categorie refuză fișierul "
            + "(S3-D6, SAF-D4), deci agregatul e gol și termenul `Neincluse` al cusăturii S2 e zero",
            sumar.Neincluse.Count == 0 && saft.Neincluse.Count == 0
            && rez.NeincluseStocValoare == 0m && rez.NeincluseStocCantitate == 0m);

        // ---------------- F20-D5: `ContId` pe S3 (drill-down la fișă) ----------------
        // S3 spune „371 diferă cu X"; ca omul să poată DESCHIDE contul, rândul are
        // nevoie de GUID, nu doar de simbol. Oracolul: contul cu acel id are, după
        // aceeași normalizare pe care o folosește gruparea, chiar simbolul rândului.
        {
            var simboluriConturi = os.GetObjectsQuery<Cont>()
                .Select(c => new { c.ID, c.Simbol }).ToList()
                .ToDictionary(c => c.ID, c => c.Simbol);
            var reale = rez.StocPerCont.Where(c => c.Cont != SaftReguli.ProductTypeImplicit).ToList();
            foreach (var c in rez.StocPerCont)
                Console.WriteLine($"     MĂSURAT (F20-D5 S3): cont „{c.Cont}” ⇒ ContId {c.ContId} = "
                    + $"„{simboluriConturi.GetValueOrDefault(c.ContId) ?? "<niciun cont>"}”");
            suita.Check("F20-D5 `SaftDiferentaCont.ContId` = GUID-ul contului din spatele simbolului: nenul pe fiecare "
                + "rând al cărui simbol e un cont real, iar contul cu acel id are — după ACEEAȘI normalizare "
                + "`ProductTypeDinCont` folosită la grupare — exact simbolul rândului; niciun cont inventat pe "
                + "„0” (`ProductTypeImplicit`, produsul fără cont de stoc), unde rămâne `Guid.Empty` (73e)",
                reale.Count > 0
                && reale.All(c => c.ContId != Guid.Empty
                    && simboluriConturi.TryGetValue(c.ContId, out var simbol)
                    && SaftReguli.ProductTypeDinCont(simbol) == c.Cont)
                && rez.StocPerCont.All(c => c.Cont != SaftReguli.ProductTypeImplicit
                    || c.ContId == Guid.Empty));
        }

        // ---------------- D18-V1: trecerea UNICĂ peste istoric == recalcularea naivă ----------------
        {
            var d18 = OracolStocFizic.Compara(os, saft, pStart, pEnd);
            Console.WriteLine($"     MĂSURAT (D18-V1 pe cub): {d18}");
            suita.Check("D18-V1 (pe cub) Opening/Closing al fiecărei intrări `PhysicalStock` == suma naivă a postărilor cubului pe "
                + "(gestiune, lot, cont), cu deschiderea din lună în Opening (SC-SAFT-45); cheile așteptate — domeniul din postări "
                + "și categoria contului, nu din fișier — coincid cu cele din fișier în ambele sensuri (B8-RV2)", d18.Ok);
        }

        // ---------------- Antetul ----------------
        suita.Check("D17-V2 antet: `HeaderComment = C` („la cerere”) — SINGURUL lucru care face din fișier o "
            + "declarație de STOCURI (nu există „D406S”); restul antetului e identic cu al lunarului, din "
            + "aceeași funcție comună",
            saft.Header != null && saft.Header.HeaderComment == "C"
            && saft.Header.AuditFileVersion == "2.0" && saft.Header.AuditFileCountry == "RO"
            && saft.Header.RegistrationNumber == "RO12345674"
            && saft.Header.PeriodStart == luna && saft.Header.PeriodStartYear == an
            && saft.Header.AuditFileDateCreated == dataCreare
            && saft.Header.Address.City == "Cluj-Napoca" && saft.Header.Address.Region == "RO-CJ");

        // ---------------- Codul fiecărui tip de document ----------------
        List<SaftMiscareStoc> Miscari(Document doc, bool storno = false) => saft.MiscariStoc
            .Where(m => m.DocumentId == doc.ID && m.Storno == storno).ToList();
        string CodMiscare(Document doc) => string.Join("+", Miscari(doc).Select(m => m.MovementType)
            .OrderBy(c => c, StringComparer.Ordinal));
        var mFct = Miscari(fct).SingleOrDefault();
        var mBtr = Miscari(btr).SingleOrDefault();
        var mBcs = Miscari(bcs).SingleOrDefault();
        var mDsc = Miscari(dsc).SingleOrDefault();
        var mRlf = Miscari(rlf).SingleOrDefault();
        var mRdc = Miscari(rdc).SingleOrDefault();
        Console.WriteLine($"     MĂSURAT (D17-V2 coduri): NIR {CodMiscare(nirConex)}, BTR {CodMiscare(btr)}, "
            + $"BCS {CodMiscare(bcs)}, LDI {CodMiscare(ldi)}, DSC {CodMiscare(dsc)}, ASM {CodMiscare(asm)}, "
            + $"RLF {CodMiscare(rlf)}, RDC {CodMiscare(rdc)}, FCT {CodMiscare(fct)}.");
        suita.Check("D17-V2 fiecare tip de document primește codul PE CARE I-L DĂ POLITICA, nu unul din cod: FCT ⇒ 10 "
            + "(achiziție; NIR-ul conex egal nu mișcă, SAF-B5), BTR ⇒ 80 (transfer intern), BCS ⇒ 70 (consum), LDI ⇒ "
            + "110 + 120 (plus și minus de inventar), DSC ⇒ 30 (vânzare), ASM ⇒ 20 + 70 (producție consumând), RLF ⇒ "
            + "50 (retur la furnizor), RDC ⇒ 40 (retur de la client)",
            CodMiscare(fct) == "10" && CodMiscare(nirConex) == "" && CodMiscare(btr) == "80" && CodMiscare(bcs) == "70"
            && CodMiscare(ldi) == "110+120" && CodMiscare(dsc) == "30" && CodMiscare(asm) == "20+70"
            && CodMiscare(rlf) == "50" && CodMiscare(rdc) == "40");

        // ---------------- ASM: documentul care se SPARGE ----------------
        var miscariAsm = Miscari(asm).OrderBy(m => m.MovementType, StringComparer.Ordinal).ToList();
        var asm20 = miscariAsm.FirstOrDefault(m => m.MovementType == "20");
        var asm70 = miscariAsm.FirstOrDefault(m => m.MovementType == "70");
        Console.WriteLine($"     MĂSURAT (D17-V2 ASM): {miscariAsm.Count} mișcări — "
            + $"{string.Join(" | ", miscariAsm.Select(m => $"{m.MovementReference} ×{m.Linii.Count} "
                + $"{m.Linii.Sum(l => l.Quantity):0.###}/{m.Linii.Sum(l => l.BookValue):N2}"))}.");
        suita.Check("D17-V2 ASM: UN document, DOUĂ mișcări — unitatea e `(Document × Storno × cod)`, iar referința "
            + "capătă `/T` (Transfer) și sufixul de cod (`/T/20`, `/T/70`, S3-D5) exact fiindcă documentul se sparge; "
            + "produsul iese pozitiv (+2 / 55), consumul negativ (−7 / −55): linia produsului al cărui tip n-are azi "
            + "cont de stoc iese pe contul istoric al postării, 371 (SAF-B5)",
            miscariAsm.Count == 2
            && asm20 != null && asm20.MovementReference.EndsWith("/T/20") && asm20.Linii.Count == 1
            && asm20.Linii[0].Quantity == 2m && asm20.Linii[0].BookValue == 55m
            && asm70 != null && asm70.MovementReference.EndsWith("/T/70") && asm70.Linii.Count == 2
            && asm70.Linii.Sum(l => l.Quantity) == -7m && asm70.Linii.Sum(l => l.BookValue) == -55m
            && asm70.Linii.Single(l => l.LotId == lotF1.ID) is { Quantity: -3m, BookValue: -15m, AccountId: "371" }
            && asm20.MovementReference != asm70.MovementReference
            && asm20.Linii.All(l => l.MovementSubType == "20") && asm70.Linii.All(l => l.MovementSubType == "70"));

        // ---------------- Stornourile: mișcare PROPRIE, cantitate inversă ----------------
        var stornoLdi = Miscari(ldi, storno: true).OrderBy(m => m.MovementType, StringComparer.Ordinal).ToList();
        var stornoBtr = Miscari(btrAnterior, storno: true).SingleOrDefault();
        var ldiPlus = Miscari(ldi).Single(m => m.MovementType == "110");
        var ldiMinus = Miscari(ldi).Single(m => m.MovementType == "120");
        Console.WriteLine($"     MĂSURAT (D17-V2 storno): LDI în aceeași lună ⇒ "
            + $"{string.Join(" | ", stornoLdi.Select(m => $"{m.MovementReference} {m.MovementDate:dd.MM} "
                + $"{m.Linii.Sum(l => l.Quantity):0.###}"))}; BTR din {btrAnterior.Data:MM.yyyy} stornat ⇒ "
            + $"{stornoBtr?.MovementReference} {stornoBtr?.MovementDate:dd.MM} "
            + $"{stornoBtr?.Linii.Sum(l => l.Quantity):0.###} pe {stornoBtr?.Linii.Count} linii.");
        suita.Check("D17-V2 stornoul e o MIȘCARE PROPRIE, cu sufixul `/S` (înaintea codului, S3-D5) și cu cantitatea inversă față de original — "
            + "și își păstrează CODUL operației inițiale (un storno de plus de inventar e tot `110`). Potrivirea "
            + "politicii se face pe semnul REGULII, nu pe cel al rândului: citit pe semnul brut, stornoul "
            + "n-ar mai fi găsit nicio politică și ar fi ieșit în `Neincluse`",
            stornoLdi.Count == 2
            && stornoLdi.All(m => m.MovementReference.EndsWith($"/S/{m.MovementType}") && m.MovementDate == new DateOnly(an, luna, 25))
            && stornoLdi.Single(m => m.MovementType == "110").Linii.Sum(l => l.Quantity)
                == -ldiPlus.Linii.Sum(l => l.Quantity)
            && stornoLdi.Single(m => m.MovementType == "120").Linii.Sum(l => l.Quantity)
                == -ldiMinus.Linii.Sum(l => l.Quantity)
            && stornoLdi.All(m => m.MovementReference != Miscari(ldi)
                .Single(o => o.MovementType == m.MovementType).MovementReference));
        suita.Check("D17-V2 stornoul PESTE LUNĂ: documentul e din februarie (rândurile lui originale sunt în "
            + "`Opening`), iar jumătatea de storno cade în martie ca mișcare a lunii — `MovementDate` e data "
            + "STORNĂRII, nu a documentului, iar `DocumentNumber` rămâne al documentului stornat",
            stornoBtr != null && stornoBtr.MovementType == "80" && stornoBtr.Linii.Count == 2
            && stornoBtr.MovementDate == new DateOnly(an, luna, 20)
            && stornoBtr.MovementReference.EndsWith("/S")
            && stornoBtr.DocumentNumber == btrAnterior.Numar
            && stornoBtr.Linii.Sum(l => l.Quantity) == 0m
            && stornoBtr.Linii.Any(l => l.RepartitorId == mag1.ID && l.Quantity == 2m)
            && stornoBtr.Linii.Any(l => l.RepartitorId == mag2.ID && l.Quantity == -2m)
            && !saft.MiscariStoc.Any(m => m.DocumentId == btrAnterior.ID && !m.Storno));

        // ---------------- `Excluse`: decizia, cu motivul ei ----------------
        var exclusConsum = saft.Excluse.SingleOrDefault(x => x.TipDocument == "BCS");
        Console.WriteLine($"     MĂSURAT (D17-V2 Excluse): {saft.Excluse.Count} rânduri — "
            + $"{exclusConsum?.TipStoc}/{exclusConsum?.Semn} ×{exclusConsum?.Numar} "
            + $"{exclusConsum?.Cantitate:0.###}/{exclusConsum?.Valoare:N2}.");
        suita.Check("D17-V2 `Excluse` = excluderea DELIBERATĂ, agregată per politică și cu motivul scris de om: "
            + "rândul `+Consum` al bonului de consum (4 buc × 20 lei) nu e stoc în magazie (27a), deci nu se "
            + "declară — dar cifra lui rămâne vizibilă. Deosebirea de `Neincluse` (o gaură) e de FOND",
            saft.Excluse.Count == 1 && exclusConsum != null
            && exclusConsum.TipStoc == nameof(TipStoc.Consum) && exclusConsum.Semn == 1
            && exclusConsum.Numar == 1 && exclusConsum.Cantitate == 4m && exclusConsum.Valoare == 80m
            && !string.IsNullOrWhiteSpace(exclusConsum.Motiv)
            && !saft.MiscariStoc.Any(m => m.DocumentId == bcs.ID && m.Linii.Count != 1));

        // ---------------- `Neincluse`: produsul fără cont de stoc ----------------
        var neinclusFaraCont = saft.Neincluse
            .SingleOrDefault(n => n.Cauza == "FaraContStoc");
        Console.WriteLine($"     MĂSURAT (D17-V2 Neincluse): {saft.Neincluse.Count} rânduri — "
            + $"{neinclusFaraCont?.Cauza} pe „{neinclusFaraCont?.ProdusCod}” ×{neinclusFaraCont?.Randuri} "
            + $"{neinclusFaraCont?.Cantitate:0.###}/{neinclusFaraCont?.Valoare:N2}; "
            + $"stoc fizic al produsului fără cont: "
            + $"{saft.StocFizic.Count(e => e.ProdusId == produsFaraCont.ID)} intrări cu `ProductType` "
            + $"„{saft.StocFizic.FirstOrDefault(e => e.ProdusId == produsFaraCont.ID)?.ProductType}”.");
        suita.Check("D17-V2 (SAF-B5, înlocuiește `Neincluse/FaraContStoc`) produsul al cărui tip n-are azi cont de stoc: "
            + "postările lui poartă contul istoric, deci stocul fizic și consumul ASM ies pe 371, fără `Neincluse` și "
            + "fără `ProdusFaraContStoc`; operarea unui document nou pe el e refuzată (D17/ASM, `CONT_STOC_LIPSA`)",
            saft.Neincluse.Count == 0
            && saft.StocFizic.Where(e => e.ProdusId == produsFaraCont.ID).ToList() is [{ ProductType: "371", OpeningQuantity: 30m, ClosingQuantity: 27m }]
            && !saft.Avertismente.Any(a => a.Cod == "ProdusFaraContStoc"));

        // ---------------- Terții pe linia de mișcare ----------------
        var idFurnizor = SaftReguli.IdPartener(furnizor, societate).Id;
        var idClient = SaftReguli.IdPartener(client, societate).Id;
        Console.WriteLine($"     MĂSURAT (D17-V2 terți): FCT ({mFct?.Linii[0].CustomerId}, "
            + $"{mFct?.Linii[0].SupplierId}); DSC ({mDsc?.Linii[0].CustomerId}, {mDsc?.Linii[0].SupplierId}); "
            + $"RLF ({mRlf?.Linii[0].CustomerId}, {mRlf?.Linii[0].SupplierId}); RDC "
            + $"({mRdc?.Linii[0].CustomerId}, {mRdc?.Linii[0].SupplierId}); BTR "
            + $"({mBtr?.Linii[0].CustomerId}, {mBtr?.Linii[0].SupplierId}); raportor {idRaportor}.");
        suita.Check("D17-V2 terții pe linia de mișcare, convenția S: achiziția (FCT, SAF-B5) și returul la furnizor (RLF) ⇒ "
            + "`(0, furnizor)`; vânzarea (DSC) și returul de la client (RDC) ⇒ `(client, 0)`; mișcarea internă "
            + "(BTR, BCS, LDI, ASM) ⇒ raportorul pe AMBELE — și niciodată ambele „0”, regula pe care "
            + "validatorul chiar o impune",
            mFct != null && mFct.Linii.All(l => l.CustomerId == "0" && l.SupplierId == idFurnizor)
            && mRlf.Linii.All(l => l.CustomerId == "0" && l.SupplierId == idFurnizor)
            && mDsc.Linii.All(l => l.CustomerId == idClient && l.SupplierId == "0")
            && mRdc.Linii.All(l => l.CustomerId == idClient && l.SupplierId == "0")
            && mBtr.Linii.All(l => l.CustomerId == idRaportor && l.SupplierId == idRaportor)
            && mBcs.Linii.All(l => l.CustomerId == idRaportor && l.SupplierId == idRaportor)
            && Miscari(asm).SelectMany(m => m.Linii)
                .All(l => l.CustomerId == idRaportor && l.SupplierId == idRaportor)
            && saft.MiscariStoc.SelectMany(m => m.Linii).All(l => l.CustomerId != "0" || l.SupplierId != "0"));
        suita.Check("D17-V2 partenerul unui CONEX se citește de pe laturile lui (NIR-ul clonat din FCT poartă "
            + "furnizorul, DSC-ul generat din FCL poartă clientul); `Owners` rămâne gol, iar `OwnerID` de pe "
            + "fiecare intrare de stoc fizic e raportorul — bunurile sunt proprii (ghid p. 36)",
            nirConex.Autogenerat && nirConex.DocumentSursaId == fct.ID
            && dsc.Autogenerat && dsc.DocumentSursaId == fcl.ID
            && saft.StocFizic.All(e => e.OwnerId == idRaportor));

        // ---------------- `PhysicalStock`: per lot × gestiune, cu `StockAccountNo` ----------------
        SaftStocFizic Intrare(Lot lot, Gestiune g) =>
            saft.StocFizic.SingleOrDefault(e => e.LotId == lot.ID && e.RepartitorId == g.ID);
        var eA1Mag1 = Intrare(lotA1, mag1);
        var eA1Mag2 = Intrare(lotA1, mag2);
        var eA2Mag2 = Intrare(lotA2, mag2);
        var eB1Mag1 = Intrare(lotB1, mag1);
        var eKit = Intrare(lotKit, mag1);
        var ePlus = Intrare(lotLdiPlus, mag1);
        var aleMele = saft.StocFizic.Where(e => e.WarehouseId == mag1.Cod || e.WarehouseId == mag2.Cod).ToList();
        Console.WriteLine($"     MĂSURAT (D17-V2 stoc fizic): {aleMele.Count} intrări ale scenei din "
            + $"{saft.StocFizic.Count} — {string.Join(" | ", aleMele.Select(e => $"{e.WarehouseId}/{e.ProductCode}"
                + $" {e.OpeningQuantity:0.###}→{e.ClosingQuantity:0.###} @{e.UnitPrice:0.00}"
                + $"{(e.StockAccountNo == null ? "" : " #lot")}"))}.");
        suita.Check("D17-V2 `PhysicalStock`: o intrare per (gestiune × lot) — lotul E „prețul unitar aplicabil” al "
            + "ghidului (p. 36), iar identificarea specifică (decizia 13) face granularitatea exactă. "
            + "Deschiderea vine din rândurile fără document (25e), închiderea din tot ce e ≤ ultima zi; "
            + "`UnitPrice` = prețul lotului, `UOMToUOMBaseConversionFactor` = 1, `StockCharacteristics` = 0/0",
            aleMele.Count == 8
            && eA1Mag1 is { OpeningQuantity: 98m, ClosingQuantity: 89m, UnitPrice: 10m }
            && eA1Mag2 is { OpeningQuantity: 12m, ClosingQuantity: 15m }
            && eA2Mag2 is { OpeningQuantity: 50m, ClosingQuantity: 50m, UnitPrice: 12m }
            && eB1Mag1 is { OpeningQuantity: 40m, ClosingQuantity: 34m, UnitPrice: 20m }
            && eKit is { OpeningQuantity: 0m, ClosingQuantity: 2m, UnitPrice: 27.5m }
            && aleMele.All(e => e.UomConversionFactor == 1m
                && e.StockCharacteristic == "0" && e.StockCharacteristicValue == "0"));
        suita.Check("D17-V2 intrarea care s-a NĂSCUT și a MURIT în lună (plusul de inventar stornat: deschidere 0, "
            + "închidere 0) rămâne în fișier fiindcă are MIȘCARE — altfel cele două mișcări ale ei (`110` și "
            + "`110/S`) ar fi referit un stoc care nu apare nicăieri",
            ePlus is { OpeningQuantity: 0m, ClosingQuantity: 0m }
            && saft.MiscariStoc.Count(m => m.Linii.Any(l => l.LotId == lotLdiPlus.ID)) == 2);
        suita.Check("D17-V2 `StockAccountNo` = identificatorul lotului pe FIECARE intrare și linie (S3-D5): cheia poziției e "
            + "(gestiune, produs, lot, tip), iar unicitatea ei o apără garda de injectivitate, nu numărul de loturi",
            saft.StocFizic.All(e => e.StockAccountNo == e.LotId.ToString("N"))
            && saft.MiscariStoc.SelectMany(m => m.Linii).All(l => l.StockAccountNo == l.LotId.ToString("N")));

        // ---------------- Cusăturile S1–S4 ----------------
        suita.Check("D17-V2 cusătura S1 (stoc fizic vs REGISTRU): pe FIECARE intrare, `Opening + Σ rândurile lunii == "
            + "Closing`, pe cantitate ȘI pe valoare — trei interogări independente (deschiderea cumulată, "
            + "rândurile lunii, închiderea cumulată) care trebuie să se închidă una pe alta. Validatorul ANAF nu "
            + "verifică aritmetica asta NICĂIERI, deci ea e a noastră sau a nimănui. "
            + "(`StocFizicBate` poartă de la D17-V6 ȘI cusătura S5 — pe scena asta ea pică DELIBERAT, pe "
            + "produsul fără cont de stoc; verdictul lui S1 singur e `StocIntrariDiferite`)",
            rez.StocIntrariDiferite == 0 && rez.StocIntrari > 0
            && rez.StocOpeningCantitate + rez.StocMiscariCantitate == rez.StocClosingCantitate
            && rez.StocOpeningValoare + rez.StocMiscariValoare == rez.StocClosingValoare
            && rez.StocIntrari == saft.StocFizic.Count);
        suita.Check("D17-V2 cusătura S2 (nimic nu se pierde; pe cub `Neincluse` = 0, golurile refuză): `Σ mișcări + Σ Excluse + Σ Neincluse == Σ postărilor de stoc` "
            + "pe documentele lunii, pe TOATE `TipStoc`-urile — inclusiv `Consum`, care nu e raportat. Fără "
            + "termenul ăsta egalitatea s-ar fi măsurat pe sine (registrul restrâns la ce intră în fișier)",
            rez.RegistruStocBate
            && rez.MiscariCantitate + rez.ExcluseCantitate + rez.NeincluseStocCantitate == rez.RegistruStocCantitate
            && rez.MiscariValoare + rez.ExcluseValoare + rez.NeincluseStocValoare == rez.RegistruStocValoare
            && rez.RanduriRegistruStoc > 0 && rez.ExcluseValoare != 0m && rez.NeincluseStocValoare == 0m);
        suita.Check("D17-V2 cusătura S3 (stoc vs. contabilitate) e MĂSURATĂ ȘI RAPORTATĂ per cont, nu blocantă: "
            + "registrul contabil poate purta 3xx și din note contabile ori deschideri fără lot, iar diferența "
            + "e un fapt de citit. Lista e per `ProductType`, iar Σ ei e chiar `ClosingStocFizic`",
            rez.ConturiStocVerificate == rez.StocPerCont.Count && rez.StocPerCont.Count > 0
            && rez.ClosingStocFizic == rez.StocPerCont.Sum(c => c.ClosingStocFizic)
            && rez.ClosingStocFizic == saft.StocFizic.Sum(e => e.ClosingValue)
            && rez.ClosingBalantaStoc == rez.StocPerCont.Sum(c => c.ClosingBalanta)
            && rez.StocPerCont.All(c => c.Diferenta == c.ClosingStocFizic - c.ClosingBalanta)
            && rez.ConturiStocDiferite == rez.StocPerCont.Count(c => c.Diferenta != 0m));
        suita.Check("D17-V2 cusătura S4 (integritatea referințelor): fiecare `ProductCode` de pe mișcări și din stocul "
            + "fizic e în `Products`, fiecare cod de mișcare (tip ȘI subtip) e în `MovementTypeTable`, iar "
            + "fiecare identificator de terț are format valid — `0` sau prefix `00`–`06`",
            rez.ReferinteBat && rez.ProduseLipsa == 0 && rez.CoduriMiscareLipsa == 0
            && rez.IdentitatiTertInvalide == 0 && rez.ProduseReferite > 0 && rez.CoduriMiscareFolosite > 0);
        suita.Check("D17-V2 totalurile lui `MovementOfGoods` sunt ale SERVERULUI (42c): numărul de linii și "
            + "cantitățile primite/eliberate se calculează în proiecție, nu în client; `MovementTypeTable` "
            + "declară EXACT codurile folosite, cu descrierea din nomenclatorul legii (sursă unică)",
            saft.NumberOfMovementLines == saft.MiscariStoc.Sum(m => m.Linii.Count)
            && saft.TotalQuantityReceived == saft.MiscariStoc.SelectMany(m => m.Linii)
                .Where(l => l.Quantity > 0m).Sum(l => l.Quantity)
            && saft.TotalQuantityIssued == Math.Abs(saft.MiscariStoc.SelectMany(m => m.Linii)
                .Where(l => l.Quantity < 0m).Sum(l => l.Quantity))
            && saft.TipuriMiscare.All(t => SaftReguli.CoduriMiscare[t.Cod] == t.Descriere)
            && saft.TipuriMiscare.Select(t => t.Cod).Distinct(StringComparer.Ordinal).Count()
                == saft.TipuriMiscare.Count);

        // ---------------- Master files: produsele DECLARATE, nu nomenclatorul ----------------
        var produsSaftB = saft.Produse.SingleOrDefault(p => p.ProdusId == produsB.ID);
        Console.WriteLine($"     MĂSURAT (D17-V2 produse): {saft.Produse.Count} declarate din "
            + $"{os.GetObjectsQuery<Produs>().Count()} în nomenclator (nefolositul „{produsNefolosit.Cod}” "
            + $"{(saft.Produse.Any(p => p.ProdusId == produsNefolosit.ID) ? "APARE" : "lipsește")}); "
            + $"B: NC „{produsSaftB?.ProductCommodityCode}”, UM „{produsSaftB?.UOMBase}”, "
            + $"metodă {produsSaftB?.ValuationMethod}.");
        suita.Check("D17-V2 `Products` = DOAR produsele care apar în mișcări sau în stocul fizic (nu tot "
            + "nomenclatorul: produsul care n-a mișcat și n-are stoc LIPSEȘTE), prin ACEEAȘI funcție ca "
            + "lunarul: codul NC lipsă ⇒ „0” + avertisment, UM lipsă ⇒ „H87” + avertisment, "
            + "`ValuationMethod` FIFO (51e)",
            !saft.Produse.Any(p => p.ProdusId == produsNefolosit.ID)
            && saft.Produse.Count < os.GetObjectsQuery<Produs>().Count()
            && produsSaftB is { ProductCommodityCode: "0", UOMBase: "H87", ValuationMethod: "FIFO" }
            && saft.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.FaraCodNc))
            && saft.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.FaraUnitateMasura))
            && saft.Unitati.Any(u => u.UnitOfMeasure == "H87")
            && saft.MiscariStoc.SelectMany(m => m.Linii).All(l => l.UnitOfMeasure != null
                && l.ProductCode != null && l.UomConversionFactor == 1m));

        // ---------------- Probele care cer o politică ALTFEL (mutate și puse la loc) ----------------
        // Pe modelul de azi NICIUN tip cu rol de terț nu poate rămâne fără partener
        // (NIR cere un `Partener` predator prin propria validare, DSC/RLF/RDC îl au pe
        // laturi), iar toate perechile (tip × registru) au politică. Cele două
        // ramuri rămân totuși drumuri reale ale proiecției — deci se probează
        // schimbând POLITICA, nu proiecția, și punând-o la loc.
        var tipBtr = os.FirstOrDefault<TipDocument>(t => t.Cod == "BTR").ID;
        var politicaBtr = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .First(p => p.TipDocumentId == tipBtr && p.TipStoc == TipStoc.Marfuri);
        politicaBtr.RolTert = RolTertSaft.Furnizor;
        os.CommitChanges();
        var cuRol = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var btrCuRol = cuRol.MiscariStoc.SingleOrDefault(m => m.DocumentId == btr.ID && !m.Storno);
        Console.WriteLine($"     MĂSURAT (D17-V2 rol fără partener): BTR ⇒ ({btrCuRol?.Linii[0].CustomerId}, "
            + $"{btrCuRol?.Linii[0].SupplierId}); avertisment "
            + $"{cuRol.Avertismente.FirstOrDefault(a => a.Cod == "TertLipsaPeMiscare")?.Numar ?? 0}.");
        suita.Check("D17-V2 (pe cub, SAF-D4) rol cerut + partener LIPSĂ ⇒ refuz `SAFT_TERT_LIPSA` pe document, nu raportorul "
            + "tăcut și nu identificator inventat",
            cuRol.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzTertLipsa && r.DocumentId == btr.ID));

        politicaBtr.RolTert = RolTertSaft.Niciunul;
        politicaBtr.TipStoc = TipStoc.Custodie;
        os.CommitChanges();
        var faraPolitica = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var refuzPolitica = faraPolitica.Refuzuri.Where(r => r.Cod == SaftProiectii.RefuzMiscareFaraPolitica).ToList();
        Console.WriteLine($"     MĂSURAT (D17-V2 fără politică): {refuzPolitica.Count} refuzuri `SAFT_MISCARE_FARA_POLITICA`.");
        suita.Check("D17-V2 (pe cub, SAF-D4) mișcare FĂRĂ nicio politică ⇒ refuz `SAFT_MISCARE_FARA_POLITICA` pe BTR, nu "
            + "`Neincluse`: o gaură de profil oprește fișierul",
            refuzPolitica.Count > 0 && refuzPolitica.All(r => r.DocumentId is Guid d
                && os.GetObjectByKey<Document>(d) is NotaTransfer));
        politicaBtr.TipStoc = TipStoc.Marfuri;
        os.CommitChanges();
        var refacut = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        suita.Check("D17-V2 politica pusă la loc ⇒ declarația REDEVINE identică (mișcări, linii, stoc fizic, "
            + "`Neincluse`): proiecția e o funcție a datelor, nu a ordinii în care s-au citit",
            refacut.MiscariStoc.Count == saft.MiscariStoc.Count
            && refacut.NumberOfMovementLines == saft.NumberOfMovementLines
            && refacut.StocFizic.Count == saft.StocFizic.Count
            && refacut.Neincluse.Count == saft.Neincluse.Count
            && refacut.Rezumat.RegistruStocValoare == rez.RegistruStocValoare
            && refacut.Rezumat.StocIntrariDiferite == rez.StocIntrariDiferite
            && refacut.Rezumat.StocFizicVsMiscariDiferite == rez.StocFizicVsMiscariDiferite
            && refacut.Rezumat.RegistruStocBate);

        // ---------------- Fișierul S + oracolul DUK (D17-V3) ----------------
        // Se rulează AICI, cât scena e încă pe bază, pe DTO-ul probat mai sus:
        // fișierul validat de ANAF e cel al scenei, nu unul fabricat pentru ocazie.
        VerificaSaftStocuriXml.Ruleaza(suita, saft, an, luna, msProiectie);

        // ---------------- Fixurile review-ului advers (D17-V6) ----------------
        // Tot pe scena asta, tot cât e pe bază: fixurile schimbă exact ce se vede în
        // fișier și în cusături, deci proba lor trebuie să fie aceeași scenă, nu una
        // fabricată alături.
        VerificaSaftStocuriFixuri.Ruleaza(suita, os, an, luna, dataCreare, saft, Marcaj,
            mag1, mag2, produsA, produsFaraCont, lotA1, lotF1, tip371, idRaportor);

        // ---------------- Curățenie ----------------
        RestaureazaSocietatea();
        Curata();
        var reziduuri = new List<string>();
        void FaraReziduu(string ce, bool gol) { if (!gol) reziduuri.Add(ce); }
        FaraReziduu("repartitori", !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj)));
        FaraReziduu("produse", !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj)));
        FaraReziduu("tipuri de material", !os.GetObjectsQuery<TipMaterial>().Any(t => t.Cod.StartsWith(Marcaj)));
        FaraReziduu("coduri economice", !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(Marcaj)));
        FaraReziduu("documente", !os.GetObjectsQuery<Document>().Any(d => d.Numar != null && d.Numar.StartsWith(Marcaj)));
        FaraReziduu("perioade fiscale 2027", !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == an));
        FaraReziduu("rânduri de stoc în lună", CubScena.MiscariIntre(os, pStart, pEnd) == 0);
        FaraReziduu("antetul societății",
            os.GetObjectsQuery<Societate>().First().CodFiscal == socInainte.CodFiscal);
        // Politica mutată de probe trebuie să fie EXACT cum a găsit-o scena: cele 23
        // de rânduri seed-uite, cu BTR/Marfuri/orice-semn → 80, fără rol.
        var btrDupa = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .Where(p => p.TipDocument.Cod == "BTR" && p.TipStoc == TipStoc.Marfuri)
            .Select(p => new { p.Semn, p.CodMiscare, p.RolTert }).ToList();
        FaraReziduu("politica BTR/Marfuri",
            os.GetObjectsQuery<PoliticaMiscareSaft>().Count() == 23
            && btrDupa.Count == 1 && btrDupa[0].Semn == null && btrDupa[0].CodMiscare == "80"
            && btrDupa[0].RolTert == RolTertSaft.Niciunul);
        Console.WriteLine($"     MĂSURAT (D17-V2 curățenie): {(reziduuri.Count == 0 ? "niciun reziduu"
            : "REZIDUU pe " + string.Join(", ", reziduuri))}.");
        suita.Check("Curățenie finală felia SAF-T S (fără reziduuri e2e: repartitori, documente, loturi, produse, "
            + "tipul de material de probă, perioadele fiscale 2027; antetul societății și POLITICA mutată de "
            + "probe puse la loc — o politică rămasă stricată ar fi mutat cifrele feliei următoare)",
            reziduuri.Count == 0);
    }
}
