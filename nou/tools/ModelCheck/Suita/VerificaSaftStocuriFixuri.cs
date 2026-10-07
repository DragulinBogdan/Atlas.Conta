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

// ============ Felia 17: fixurile review-ului advers — D17-V6 ============
// Opt fixuri, pe ACEEAȘI scenă ca D17-V2/V3 (nu pe una fabricată alături: ce
// probează fixurile e chiar ce se vede în fișierul validat mai sus).
//
//  F1  `MovementReference` UNICĂ: `(codTip, Numar)` nu e o identitate — importul
//      aduce numărul sursei și conexele îl moștenesc —, deci coliziunile primesc
//      un discriminant `#n`, iar S4 numără referințele duplicate.
//  F2  cusătura S5: aceeași egalitate ca S1, cu Σ luată din LINIILE EMISE. S1
//      confruntă trei interogări pe registru între ele, deci nu vede fișierul.
//  F3  codul politicii RE-verificat în proiecție: un cod din afara
//      nomenclatorului scoate rândurile în `Neincluse`, nu le declară.
//  F4  `MovementPostingDate` omis când `DataOperare` cade în afara perioadei.
//  F5  rolul de terț al unui grup cu DOUĂ politici.
//  F6  intrările „0 bucăți, X lei" — declarate, cu cauza lor reală (45e).
//  F7  S3 spartă pe TIPUL documentului care pune diferența.
//  F8  `AnalysisTypeTable` GOL pe S (probat în D17-V3, pe fișier + DUK).
static class VerificaSaftStocuriFixuri {
    public static void Ruleaza(Suita s, IObjectSpace os, int an, int luna, DateOnly dataCreare, SaftDto saft,
            string marcaj, Gestiune mag1, Gestiune mag2, Produs produsA, Produs produsFaraCont,
            Lot lotA1, Lot lotF1, TipMaterial tip371, string idRaportor) {

        // ══════════ (a) Funcțiile PURE ══════════
        var faraDiscriminant = SaftReguli.MovementReference("DSC", "DSC-000123", null, false);
        var cuUnu = SaftReguli.MovementReference("DSC", "DSC-000123", null, false, 1);
        var cuDoi = SaftReguli.MovementReference("DSC", "DSC-000123", "30", true, 2);
        var cuDoisprezeceLung = SaftReguli.MovementReference("DSC", new string('9', 60), "30", true, 12);
        Console.WriteLine($"     MĂSURAT (D17-V6/F1 referință): fără discriminant „{faraDiscriminant.Referinta}”; "
            + $"#1 „{cuUnu.Referinta}”; #2 + cod + storno „{cuDoi.Referinta}”; număr lung + #12 "
            + $"„{cuDoisprezeceLung.Referinta}” ({cuDoisprezeceLung.Referinta.Length} caractere, "
            + $"trunchiat {cuDoisprezeceLung.Trunchiat}).");
        s.Check("D17-V6 (F1) `MovementReference` primește discriminantul `#n` ÎNAINTEA sufixelor de cod și de "
            + "storno — sufixele rămân coada recognoscibilă a referinței, iar două documente cu același număr "
            + "ies cu identități diferite. Trunchierea taie tot NUMĂRUL, nu discriminantul: el e chiar partea "
            + "care le deosebește",
            faraDiscriminant == ("DSC-DSC-000123", false)
            && cuUnu == ("DSC-DSC-000123#1", false)
            && cuDoi == ("DSC-DSC-000123#2/30/S", false)
            && cuDoisprezeceLung.Trunchiat
            && cuDoisprezeceLung.Referinta.Length == SaftReguli.LungimeMovementReference
            && cuDoisprezeceLung.Referinta.EndsWith("#12/30/S")
            && cuDoisprezeceLung.Referinta.StartsWith("DSC-"));

        // Proba NEGATIVĂ a cusăturii, pe un DTO fabricat: fără ea, „0 referințe
        // duplicate" ar putea să însemne „numărătoarea nu numără".
        var doua = new List<SaftMiscareStoc> {
            new() { MovementReference = "BTR-1" }, new() { MovementReference = "BTR-2" },
        };
        var trei = new List<SaftMiscareStoc> {
            new() { MovementReference = "BTR-1" }, new() { MovementReference = "BTR-1" },
            new() { MovementReference = "BTR-2" },
        };
        s.Check("D17-V6 (F1) `SaftProiectii.ReferinteDuplicate` NUMĂRĂ: două referințe distincte ⇒ 0, două "
            + "identice din trei ⇒ 1. Cusătura S4 e o afirmație despre FIȘIER, deci se probează pe un DTO, "
            + "fără nicio scenă",
            SaftProiectii.ReferinteDuplicate(doua) == 0 && SaftProiectii.ReferinteDuplicate(trei) == 1);

        // ══════════ (b) Fixurile care se citesc pe DTO-ul de bază ══════════
        var rez = saft.Rezumat;

        // ── F2: cusătura S5 ──────────────────────────────────────────────────────
        // Scena are DELIBERAT un produs fără cont de stoc: rândurile lui ies în
        // `Neincluse/FaraContStoc`, deci fișierul NU spune despre cele două intrări
        // ale lui ce spune registrul. S1 rămâne verde (registrul se închide pe sine)
        // — și exact asta era gaura: cusătura nu se uita la fișier.
        var emisePeCheie = saft.MiscariStoc.SelectMany(m => m.Linii)
            .GroupBy(l => (l.RepartitorId, l.LotId))
            .ToDictionary(g => g.Key, g => (Cant: g.Sum(l => l.Quantity), Val: g.Sum(l => l.BookValue)));
        var intrariRupte = saft.StocFizic.Where(e => {
            var emis = emisePeCheie.GetValueOrDefault((e.RepartitorId, e.LotId));
            return e.OpeningQuantity + emis.Cant != e.ClosingQuantity
                || e.OpeningValue + emis.Val != e.ClosingValue;
        }).ToList();
        Console.WriteLine($"     MĂSURAT (D17-V6/F2 S5): S1 {rez.StocIntrariDiferite}/{rez.StocIntrari} · "
            + $"S5 {rez.StocFizicVsMiscariDiferite}/{rez.StocIntrari} · `StocFizicBate` {rez.StocFizicBate}; "
            + $"intrările rupte: {string.Join(" | ", intrariRupte.Select(e => $"{e.WarehouseId}/{e.ProductCode} "
                + $"{e.OpeningQuantity:0.###}→{e.ClosingQuantity:0.###}"))}.");
        s.Check("D17-V6 (F2, pe cub) cusătura S5 pune FIȘIERUL de o parte a semnului egal: `Opening + Σ liniile EMISE == "
            + "Closing`, pe fiecare intrare, recalculat aici din liniile emise; pe cub nicio intrare nu se rupe, fiindcă "
            + "produsul fără cont își poartă contul istoric (SAF-B5)",
            rez.StocIntrariDiferite == 0 && rez.StocFizicVsMiscariDiferite == 0 && rez.StocFizicBate
            && intrariRupte.Count == 0 && saft.StocFizic.Count(e => e.LotId == lotF1.ID) == 1);

        // ── F4: `MovementPostingDate` în afara perioadei ─────────────────────────
        // Scena operează documentele ACUM (motorul pune `DataOperare = UtcNow`), iar
        // luna declarată e martie 2027 — deci toate mișcările cad pe cazul care pe
        // baza de import apare la fel: importul lui 12/2025 a rulat în 2026-08.
        var documenteCuMiscari = saft.MiscariStoc.Select(m => m.DocumentId).Distinct().Count();
        var avertPostare = saft.Avertismente
            .FirstOrDefault(a => a.Cod == "DataPostariiInAfaraPerioadei");
        Console.WriteLine($"     MĂSURAT (D17-V6/F4 postare): {saft.MiscariStoc.Count(m => m.MovementPostingDate != null)}"
            + $"/{saft.MiscariStoc.Count} mișcări cu `MovementPostingDate`; avertisment ×{avertPostare?.Numar ?? 0} "
            + $"peste {documenteCuMiscari} documente — ex. {avertPostare?.Exemple.FirstOrDefault()}");
        s.Check("D17-V6 (F4, pe cub, S3-D2) `MovementPostingDate` = data UTC a `Tranzactie.ScrisLa`, același reper ca "
            + "`SystemEntryDate` din GL (R2): prezentă pe fiecare mișcare chiar în afara perioadei (DUK o acceptă, D17-V3), "
            + "deci nu mai există nici omisiunea, nici avertismentul `DataPostariiInAfaraPerioadei`",
            documenteCuMiscari > 0
            && saft.MiscariStoc.All(m => m.MovementPostingDate != null)
            && avertPostare == null);

        // ── F7: S3 spartă pe tipul documentului ──────────────────────────────────
        foreach (var c in rez.StocPerCont)
            Console.WriteLine($"     MĂSURAT (D17-V6/F7 S3 {c.Cont}): diferență {c.Diferenta:N2} = "
                + string.Join(" + ", c.Componente.Select(x => $"{x.TipDocument} {x.Diferenta:N2}")));
        s.Check("D17-V6 (F7) fiecare cont din S3 își sparge diferența pe TIPUL documentului care o pune (rândurile "
            + "fără document = deschiderea): Σ componentelor == cifra contului, pe AMBELE laturi și pe diferență. "
            + "„371 diferă cu X” nu se poate acționa; „din care NTC atât și DSC atât” da",
            rez.StocPerCont.Count > 0
            && rez.StocPerCont.All(c => c.Componente.Sum(x => x.Diferenta) == c.Diferenta
                && c.Componente.Sum(x => x.StocFizic) == c.ClosingStocFizic
                && c.Componente.Sum(x => x.Balanta) == c.ClosingBalanta
                && c.Componente.All(x => x.Diferenta == x.StocFizic - x.Balanta))
            && rez.StocPerCont.Any(c => c.Componente.Count > 0));

        // ── F8: `AnalysisTypeTable` GOL ──────────────────────────────────────────
        s.Check("D17-V6 (F8) `AnalysisTypeTable` e GOL pe declarația de stocuri: S nu emite `Analysis` pe nicio "
            + "linie, deci tabela n-are ce declara — iar cele șapte interogări de dimensiuni pe registrul "
            + "contabil nu se mai fac. (Fișierul cu tabela goală trece DUK — probat în D17-V3.)",
            saft.TipuriAnaliza.Count == 0
            && saft.MiscariStoc.SelectMany(m => m.Linii).All(l => l.ProductCode != null));

        // ══════════ (c) Fixurile care cer scenă proprie ══════════
        // Toate adaugă artefacte cu MARCAJUL scenei, deci `Curata()` de la sfârșitul
        // lui D17-V2 le ia pe toate; politicile mutate se pun la loc AICI.
        var tip302 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
        var dDeschidere = new DateOnly(an - 1, 12, 31);

        // Produs pe o clasă NE-marfă (302 ⇒ clasa „M”): regulile de stoc private îl
        // trimit pe registrul GENERIC (`Magazie`), nu pe `Marfuri` — de aici un
        // document care atinge DOUĂ registre sub ACELAȘI cod, adică fix cazul lui F5.
        var produsMagazie = os.CreateObject<Produs>();
        produsMagazie.Cod = marcaj + "-MAG"; produsMagazie.Denumire = "Material SAF-T S (registrul generic)";
        produsMagazie.TipMaterial = tip302; produsMagazie.UM = "BUC";
        produsMagazie.UnitateMasura = os.FirstOrDefault<UnitateMasura>(u => u.Cod == "H87");
        produsMagazie.CodNc = "01012100";
        os.CommitChanges();

        Lot LotCuDeschidere(Produs produs, decimal pret, decimal cantitate, decimal valoare) {
            if (cantitate > 0m) {
                var factura = os.CreateObject<FacturaIntrare>(); factura.Data = dDeschidere;
                factura.Numar = marcaj + "-FCT-MAG";
                factura.Predator = os.FirstOrDefault<Partener>(p => p.Cod == marcaj + "-FURN");
                factura.Primitor = mag1;
                var linie = os.CreateObject<FacturaIntrareDetaliu>(); linie.Document = factura;
                linie.TipMaterial = produs.TipMaterial; linie.Cantitate = cantitate; linie.PretUnitar = pret;
                var primit = linie.CreeazaLot(os, produs, mag1);
                os.CommitChanges();
                var nir = MotorOperare.Opereaza(os, factura);
                if (nir != null) MotorOperare.Opereaza(os, nir);
                return primit;
            }
            throw new InvalidOperationException("LotCuDeschidere cere o cantitate pozitivă.");
        }
        var lotMagazie = LotCuDeschidere(produsMagazie, 4m, 100m, 400m);
        os.CommitChanges();

        // BTR care atinge AMBELE registre sub același cod (`80`): o linie de marfă
        // (⇒ `Marfuri`) și una de material (⇒ `Magazie`).
        var btrMixt = os.CreateObject<NotaTransfer>();
        btrMixt.Numar = marcaj + "-BTR-MIX"; btrMixt.Data = new DateOnly(an, luna, 22);
        btrMixt.Predator = mag1; btrMixt.Primitor = mag2; btrMixt.NumarPV = marcaj;
        var linMixtMarfa = os.CreateObject<DocumentDetaliu>();
        linMixtMarfa.Document = btrMixt; linMixtMarfa.TipMaterial = tip371;
        linMixtMarfa.Lot = lotA1; linMixtMarfa.Cantitate = 1m;
        var linMixtMaterial = os.CreateObject<DocumentDetaliu>();
        linMixtMaterial.Document = btrMixt; linMixtMaterial.TipMaterial = tip302;
        linMixtMaterial.Lot = lotMagazie; linMixtMaterial.Cantitate = 2m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, btrMixt);
        os.CommitChanges();

        // DOUĂ documente de același tip cu ACELAȘI număr — cazul real al importului
        // (1C aduce numărul sursei, conexele îl moștenesc). `Numar` e server-owned pe
        // BTR, dar ușa uneltei e cea NON-secured (44), exact ca a conectorului: se
        // scrie direct, cum scrie și importul.
        NotaTransfer BtrDuplicat(int zi, decimal cantitate) {
            var d = os.CreateObject<NotaTransfer>();
            d.Numar = marcaj + "-DUP"; d.Data = new DateOnly(an, luna, zi);
            d.Predator = mag1; d.Primitor = mag2; d.NumarPV = marcaj;
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = d; linie.TipMaterial = tip371; linie.Lot = lotA1; linie.Cantitate = cantitate;
            return d;
        }
        var dup1 = BtrDuplicat(23, 1m);
        var dup2 = BtrDuplicat(24, 2m);
        os.CommitChanges();
        MotorOperare.Opereaza(os, dup1);
        os.CommitChanges();
        MotorOperare.Opereaza(os, dup2);
        os.CommitChanges();

        var dupaScena = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);

        // ── F1 pe scenă ──────────────────────────────────────────────────────────
        var refDup = dupaScena.MiscariStoc.Where(m => m.DocumentId == dup1.ID || m.DocumentId == dup2.ID)
            .OrderBy(m => m.DocumentId).Select(m => m.MovementReference).ToList();
        var avertNumar = dupaScena.Avertismente
            .FirstOrDefault(a => a.Cod == "NumarDocumentDuplicat");
        var ordinePeId = new[] { dup1.ID, dup2.ID }.OrderBy(x => x).ToList();
        Console.WriteLine($"     MĂSURAT (D17-V6/F1 scenă): două BTR cu numărul „{dup1.Numar}” ⇒ "
            + $"[{string.Join(", ", refDup)}]; avertisment ×{avertNumar?.Numar ?? 0}; S4 referințe duplicate "
            + $"{dupaScena.Rezumat.ReferinteDuplicate}/{dupaScena.MiscariStoc.Count}, `ReferinteBat` "
            + $"{dupaScena.Rezumat.ReferinteBat}.");
        s.Check("D17-V6 (F1, pe cub, S3-D5) două documente de ACELAȘI tip cu ACELAȘI număr trec cu mișcările lor pe "
            + "rezerva `{TranzactieId:N}{cod}`: referințe DISTINCTE, fără forma lizibilă ambiguă, iar cusătura S4 numără "
            + "0 referințe duplicate",
            refDup.Count == 2 && refDup[0] != refDup[1]
            && refDup.All(r => !r.Contains(marcaj + "-DUP"))
            && ordinePeId.Count == 2
            && dupaScena.Rezumat.ReferinteDuplicate == 0 && dupaScena.Rezumat.ReferinteBat);

        s.Check("D17-V6 (F5) fără politici cu roluri diferite pe același cod, scena nu are refuzuri",
            dupaScena.Refuzuri.Count == 0);

        // ── F5: rolurile grupului ────────────────────────────────────────────────
        var tipBtrId = os.FirstOrDefault<TipDocument>(t => t.Cod == "BTR").ID;
        var polMarfuri = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .First(p => p.TipDocumentId == tipBtrId && p.TipStoc == TipStoc.Marfuri);
        var polMagazie = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .First(p => p.TipDocumentId == tipBtrId && p.TipStoc == TipStoc.Magazie);

        polMarfuri.RolTert = RolTertSaft.Furnizor;
        os.CommitChanges();
        var rolUnic = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var mixtUnic = rolUnic.MiscariStoc.SingleOrDefault(m => m.DocumentId == btrMixt.ID && !m.Storno);

        polMagazie.RolTert = RolTertSaft.Client;
        os.CommitChanges();
        var rolMixt = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var mixtDoua = rolMixt.MiscariStoc.SingleOrDefault(m => m.DocumentId == btrMixt.ID && !m.Storno);
        var avertRolMixt = rolMixt.Avertismente
            .FirstOrDefault(a => a.Cod == "RolTertMixt");

        polMarfuri.RolTert = RolTertSaft.Niciunul;
        polMagazie.RolTert = RolTertSaft.Niciunul;
        os.CommitChanges();

        Console.WriteLine($"     MĂSURAT (D17-V6/F5 roluri): un singur rol ne-`Niciunul` ⇒ mișcarea are "
            + $"{mixtUnic?.Linii.Count} linii, avertisment "
            + $"{rolUnic.Avertismente.Count(a => a.Cod == "RolTertMixt")}; DOUĂ roluri "
            + $"⇒ {mixtDoua?.Linii.Count} linii, avertisment ×{avertRolMixt?.Numar ?? 0} — "
            + $"ex. {avertRolMixt?.Exemple.FirstOrDefault()}");
        // 4 linii, nu 2: transferul scrie pe AMBELE picioare (−predator, +primitor)
        // pentru fiecare dintre cele două linii de document, deci grupul are patru
        // rânduri de registru — două pe `Marfuri` și două pe `Magazie`.
        s.Check("D17-V6 (F5, pe cub, S3-D2) rolul de terț e AL GRUPULUI: BTR-ul care atinge două categorii sub același "
            + "cod cu roluri diferite (Furnizor + Niciunul, apoi Furnizor + Client) e refuzat `SAFT_PROVENIENTA_AMBIGUA`, "
            + "nu ales determinist; un BTR pe o singură categorie, cu rolul cerut și fără partener, e `SAFT_TERT_LIPSA`",
            rolUnic.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzProvenienta && r.DocumentId == btrMixt.ID)
            && rolUnic.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzTertLipsa && r.DocumentId == dup1.ID)
            && rolMixt.Refuzuri.Any(r => r.Cod == SaftProiectii.RefuzProvenienta && r.DocumentId == btrMixt.ID
                && r.Mesaj.Contains("roluri de terț diferite")));

        // ── F3: codul politicii, RE-verificat în proiecție ───────────────────────
        // Se scrie pe ușa NON-secured (ca seed-ul și ca importul), deci gardianul nu
        // apucă să refuze — exact drumul pe care un cod greșit ajunge în bază.
        var codInitial = polMarfuri.CodMiscare;
        polMarfuri.CodMiscare = "999";
        os.CommitChanges();
        var cuCodStricat = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        var refuzCod = cuCodStricat.Refuzuri.Where(r => r.Cod == SaftProiectii.RefuzCodMiscare).ToList();
        polMarfuri.CodMiscare = codInitial;
        os.CommitChanges();
        var refacutCod = SaftProiectii.SaftStocuriPeCub(os, an, luna, dataCreare);
        Console.WriteLine($"     MĂSURAT (D17-V6/F3 cod „999”): {refuzCod.Count} refuzuri `SAFT_COD_MISCARE_NECUNOSCUT`; "
            + $"`MovementTypeTable` = [{string.Join(", ", cuCodStricat.TipuriMiscare.Select(t => t.Cod))}]; "
            + $"S2 bate {cuCodStricat.Rezumat.RegistruStocBate}; după restaurare "
            + $"{refacutCod.MiscariStoc.Count} mișcări (față de {cuCodStricat.MiscariStoc.Count}).");
        s.Check("D17-V6 (F3, pe cub, SAF-D4) un cod de mișcare care NU e în nomenclatorul D406 refuză fișierul "
            + "(`SAFT_COD_MISCARE_NECUNOSCUT`) — nu-l declară cu codul cules și nu-l pune în `MovementTypeTable`; "
            + "politica pusă la loc reface declarația",
            refuzCod.Count > 0 && refuzCod.All(r => r.Mesaj.Contains("999"))
            && !cuCodStricat.TipuriMiscare.Any(t => t.Cod == "999")
            && !cuCodStricat.MiscariStoc.Any(m => m.MovementType == "999")
            && refacutCod.MiscariStoc.Count == dupaScena.MiscariStoc.Count);

        // Gardianul: un cod de SPAȚII nu e nici cod valid, nici excludere deliberată.
        using (var osGard = s.Provider.CreateObjectSpace()) {
            var p = osGard.CreateObject<PoliticaMiscareSaft>();
            p.TipDocument = osGard.FirstOrDefault<TipDocument>(t => t.Cod == "BCS");
            p.TipStoc = TipStoc.Custodie;
            p.Semn = +1;
            p.CodMiscare = "   ";
            string mesajSpatii = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajSpatii = e.Message; }
            p.Motiv = "custodia nu se raportează încă";
            string mesajCuMotiv = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajCuMotiv = e.Message; }
            Console.WriteLine($"     MĂSURAT (D17-V6/F3 gardian): cod „   ” fără motiv → "
                + $"„{mesajSpatii ?? "<NU A ARUNCAT>"}”; același cod CU motiv → „{mesajCuMotiv ?? "tace"}”.");
            s.Check("D17-V6 (F3) gardianul citește codul cu `IsNullOrWhiteSpace`, nu `IsNullOrEmpty`: un cod de "
                + "SPAȚII trecea pe lângă AMBELE reguli — nu era verificat contra nomenclatorului (ramura cerea "
                + "„nenul”) și nu i se cerea motiv (ramura cerea „gol”). Acum e tratat ca absența codului, adică "
                + "drept excludere deliberată, deci cere motiv",
                mesajSpatii != null && mesajSpatii.Contains("motiv") && mesajCuMotiv == null);
            osGard.Rollback();
        }
    }
}
