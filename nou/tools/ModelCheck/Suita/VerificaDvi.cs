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

// ---------------------------------------------------------------------------
// Felia 25 (DVI-D8) — declarația vamală de import
// ---------------------------------------------------------------------------
// Cusătura probată: tipul `Dvi` (cu `Motor/*` NEATINS) → politica de TVA a
// profilului → postările contabile și fiscale ale cubului → D300 (rd. 24 direct, rd. 7
// cu oglinda 22), jurnalul de cumpărări, D394 (nedeclarat deliberat) și codul
// SAF-T al importului (DVI-r7).
//
// Luna de lucru e FEBRUARIE 2026: niciun alt bloc privat nu scrie acolo. Cifrele
// formularului se citesc ca DELTĂ (înainte/după operare), ca proba să nu depindă
// de ce a mai lăsat baza — oglinda rd. 22 = rd. 7 rămâne absolută, fiind o
// egalitate a formularului.
static class VerificaDvi {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-DVI";

        // ── Bugetar: ancora în nucleu, tipul INERT, nomenclatorul fără import ────
        if (!privat) {
            using var osB = s.Provider.CreateObjectSpace();
            var tipB = osB.FirstOrDefault<TipDocument>(t => t.Cod == "DVI");
            var deImportB = osB.GetObjectsQuery<TipTva>().Count(t => t.DeImport);
            Console.WriteLine($"     MĂSURAT (DVI-V0/bugetar): ancoră DVI = "
                + $"{(tipB == null ? "<lipsă>" : tipB.ClrType)}; {deImportB} tipuri de TVA `DeImport`.");
            s.Check("DVI-V0 (bugetar) ancora TipDocument DVI există în NUCLEU cu ClrType-ul clasei, dar tipul e "
                + "INERT — nicio politică (TVA/implicit/numerotare/scadență/validare/conex), nicio regulă de "
                + "stoc sau contare — și niciun `TipTva` de import: codurile 301204/300604 sunt ale profilului "
                + "privat (DVI-D2), ca DSC/ITV/ASM/BPR",
                tipB != null && tipB.ClrType == nameof(Dvi)
                && osB.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipB.ID) == null
                && osB.FirstOrDefault<PoliticaTvaImplicit>(p => p.TipDocumentId == tipB.ID) == null
                && osB.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipB.ID) == null
                && osB.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipB.ID) == null
                && osB.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipB.ID) == null
                && osB.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipB.ID) == null
                && !osB.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipB.ID)
                && !osB.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipB.ID)
                && deImportB == 0);
            return;
        }

        // ── Curățenia scenei (purjă FIZICĂ — F13-D2) ─────────────────────────────
        // Purjă și scena blocului `E2E-API-DVI` (aceeași lună): un crash acolo nu
        // are voie să otrăvească rularea următoare a blocului ăsta.
        void CurataDvi(IObjectSpace os) {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj) || r.Cod.StartsWith("E2E-API-DVI")).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<DviFactura>()
                .Where(f => docIds.Contains(f.DviId) || docIds.Contains(f.FacturaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj) || r.Cod.StartsWith("E2E-API-DVI")).ToList());
            pj.Executa();
        }

        // CALEA REALĂ: dispecerul generic `IVerificabilLaCommit` din
        // `GardianEditare.Verifica` (DVI-D3). Nu se cheamă `DviFactura.Verifica`
        // direct — proba trebuie să ateste că regula e ARMATĂ pe ușa securizată,
        // nu doar că există corpul ei.
        static string RefuzGardian(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }
        var febStart = new DateOnly(2026, 2, 1);
        var febEnd = new DateOnly(2026, 2, 28);
        var dataDvi = new DateOnly(2026, 2, 16);
        Guid idFct, idDvi, idVama, idUnitate, idContPropriu, idTipTrz, idDviDraft, idFctDraft;

        using (var os = s.Provider.CreateObjectSpace())
            CurataDvi(os);
        using (var os = s.Provider.CreateObjectSpace())
            s.Check("DVI — precondiție: FEBRUARIE 2026 e liberă după purjă (nicio declarație, niciun document în lună); "
                + "dacă pică, un alt bloc a lăsat scena în luna asta și cifrele de mai jos ar fi peste conținut străin",
                !os.GetObjectsQuery<Dvi>().Any()
                && !os.GetObjectsQuery<Document>().Any(d => d.Data >= febStart && d.Data <= febEnd));

        using (var os = s.Provider.CreateObjectSpace()) {
            // ---- Seed-ul profilului (DVI-D2) ----
            var tipDvi = os.FirstOrDefault<TipDocument>(t => t.Cod == "DVI");
            var politicaTva = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDvi.ID);
            var cont446 = os.FirstOrDefault<Cont>(c => c.Simbol == "446");
            var cont4426 = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
            var cont4427 = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
            TipTva Tva(string cod) => os.FirstOrDefault<TipTva>(t => t.Cod == cod);
            var imp21 = Tva("IMP21");
            var imp11 = Tva("IMP11");
            var impTi21 = Tva("IMPTI21");
            var impTi11 = Tva("IMPTI11");
            var imp = Tva("IMP");
            Console.WriteLine($"     MĂSURAT (DVI-V1/privat): PoliticaTva DVI = "
                + $"{politicaTva?.Directie.ToString() ?? "<lipsă>"}/{politicaTva?.SursaContrapartida.ToString() ?? "-"}, "
                + $"fallback {os.GetObjectByKey<Cont>(politicaTva?.ContrapartidaFallbackId ?? Guid.Empty)?.Simbol ?? "<null>"}; "
                + $"IMP21 {imp21?.Cota}/{imp21?.Regim}/SAF-T {imp21?.CodSafTAchizitie}; "
                + $"IMP11 SAF-T {imp11?.CodSafTAchizitie}; IMPTI21 {impTi21?.Regim}/SAF-T {impTi21?.CodSafTAchizitie}; "
                + $"IMPTI11 SAF-T {impTi11?.CodSafTAchizitie}; ancora tipului = "
                + $"{os.GetObjectByKey<TipTva>(tipDvi.TipTvaImplicitId ?? Guid.Empty)?.Cod ?? "<null>"}.");
            s.Check("DVI-V1 (privat) seed-ul DVI: ancoră cu ClrType-ul clasei, politică de TVA deductibilă contra "
                + "contului implicit al PREDATORULUI cu fallback 446, ancoră de tip IMP21 și implicit generic "
                + "(orice clasă fiscală) tot IMP21 — și NICIO regulă de contare/stoc, nicio numerotare, scadență "
                + "sau politică de conex: declarația nu postează valoarea liniei, doar taxa",
                tipDvi != null && tipDvi.ClrType == nameof(Dvi)
                && politicaTva != null && politicaTva.Directie == DirectieTva.Deductibil
                && politicaTva.SursaContrapartida == SursaCont.RepartitorPredator
                && politicaTva.ContrapartidaFallbackId == cont446.ID
                && tipDvi.TipTvaImplicitId == imp21.ID
                && os.GetObjectsQuery<PoliticaTvaImplicit>().Count(p => p.TipDocumentId == tipDvi.ID
                    && p.ClasaFiscala == null && p.TipTvaId == imp21.ID) == 1
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipDvi.ID)
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipDvi.ID)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipDvi.ID) == null
                && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipDvi.ID) == null
                && os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipDvi.ID) == null);

            s.Check("DVI-V2 (privat) cele patru tipuri de import ale nomenclatorului SAF-T: 21%/11% cu taxa PLĂTITĂ "
                + "în vamă (301204/301205, regim Normal) și 21%/11% cu amânarea plății (300604/300605, taxare "
                + "inversă); toate deductibile pe 4426, toate `DeImport`, niciunul cu cod de livrare — importul e "
                + "exclusiv de achiziție. `IMP` (factura furnizorului extern) rămâne cotă 0 și capătă doar "
                + "`DeImport`, ca lookup-ul de culegere să-l vadă, iar `ValideazaOperare` să-l refuze pe cotă",
                imp21 != null && imp21.Cota == 21m && imp21.Regim == RegimTva.Normal
                && imp21.CodSafTAchizitie == "301204" && imp21.CodSafTLivrare == null
                && imp11 != null && imp11.Cota == 11m && imp11.Regim == RegimTva.Normal
                && imp11.CodSafTAchizitie == "301205"
                && impTi21 != null && impTi21.Cota == 21m && impTi21.Regim == RegimTva.TaxareInversa
                && impTi21.CodSafTAchizitie == "300604"
                && impTi11 != null && impTi11.Cota == 11m && impTi11.Regim == RegimTva.TaxareInversa
                && impTi11.CodSafTAchizitie == "300605"
                && new[] { imp21, imp11, impTi21, impTi11 }.All(t => t.DeImport
                    && t.ContTvaDeductibilId == cont4426.ID && t.ContTvaColectatId == cont4427.ID)
                && imp != null && imp.DeImport && imp.Cota == 0m);

            // ---- Scena ----
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-EXT";
            furnizor.Denumire = "Furnizor extra-UE probă DVI";
            furnizor.Tara = "MA";
            var vama = os.CreateObject<Partener>();
            vama.Cod = Marcaj + "-VAMA";
            vama.Denumire = "Biroul vamal probă DVI";
            vama.Tara = "RO";
            vama.ContImplicit = cont446;
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-MAG";
            gestiune.Denumire = "Gestiune probă DVI";
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-UI";
            unitate.Denumire = "Unitate probă DVI";
            os.CommitChanges();
            idVama = vama.ID;
            idUnitate = unitate.ID;
            idContPropriu = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA").ID;
            idTipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ").ID;
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");

            // Factura furnizorului extern: `IMP`, deci fără taxă pe linie (83g).
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + "-FF1";
            fct.Data = new DateOnly(2026, 2, 10);
            fct.Predator = furnizor;
            fct.Primitor = gestiune;
            var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
            linieFct.Document = fct;
            linieFct.TipMaterial = tip628;
            linieFct.Cantitate = 1m;
            linieFct.PretUnitar = 1400m;
            linieFct.TipTva = imp;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            idFct = fct.ID;
            s.Check("DVI-V3 (privat) PREMISA: factura furnizorului extern e operată cu `IMP` — cotă 0, deci linia "
                + "nu poartă taxă și rândul ei fiscal are TVA zero; TVA-ul datorat în vamă n-are de unde intra "
                + "în evidență decât prin declarație (83g)",
                fct.Stare == StareDocument.Operat
                && CubScena.Fapte(os, fct.ID).Count(f => f.Tva == 0m) == 1);

            // ---- Cifrele formularului ÎNAINTE de declarație (delta) ----
            var d300Inainte = D300Proiectii.D300(os, febStart, febEnd, null);
            decimal Baza0(string cod) => d300Inainte.Randuri.Single(r => r.Cod == cod).Baza ?? 0m;
            decimal Tva0(string cod) => d300Inainte.Randuri.Single(r => r.Cod == cod).Tva ?? 0m;
            var (baza24, tva24) = (Baza0("24"), Tva0("24"));
            var (baza7, tva7) = (Baza0("7"), Tva0("7"));

            // ---- Declarația vamală ----
            var dvi = os.CreateObject<Dvi>();
            dvi.Numar = "26ROBV" + Marcaj;
            dvi.Data = dataDvi;
            dvi.Predator = vama;
            dvi.Primitor = unitate;
            var linieNormal = os.CreateObject<DocumentDetaliu>();
            linieNormal.Document = dvi;
            linieNormal.TipMaterial = tip628;
            linieNormal.TipTva = imp21;
            linieNormal.Valoare = 1000m;
            linieNormal.ValoareTva = 210m;
            var linieTi = os.CreateObject<DocumentDetaliu>();
            linieTi.Document = dvi;
            linieTi.TipMaterial = tip628;
            linieTi.TipTva = impTi21;
            linieTi.Valoare = 500m;
            // TVA-ul lăsat la 0: îl calculează `TvaService` la operare (48b).
            var legatura = os.CreateObject<DviFactura>();
            legatura.Dvi = dvi;
            legatura.Factura = fct;
            os.CommitChanges();
            idDvi = dvi.ID;

            MotorOperare.Opereaza(os, dvi);
            var noteCub = CubScena.Note(os, idDvi);
            Console.WriteLine($"     MĂSURAT (DVI-V4/privat): {noteCub.Count} postări contabile; "
                + $"{CubScena.Fapte(os, idDvi).Count} fapte fiscale; {CubScena.Stoc(os, idDvi).Count} mișcări de stoc.");
            s.Check("DVI-V4 (privat) planul declarației, cu `Motor/*` NEATINS: ZERO mișcări de stoc, ZERO note pe "
                + "`Valoare` (liniile n-au regulă de contare și nici conturi explicite, deci motorul le sare) și "
                + "EXACT două note de TVA — 4426 = 446 de 210 pe linia cu taxa plătită în vamă (contrapartida e "
                + "contul implicit al biroului vamal), 4426 = 4427 de 105 pe cea cu amânarea plății (autolichidare, "
                + "sold zero). Taxa liniei TI a fost CALCULATĂ din cotă la operare, nu culeasă",
                dvi.Stare == StareDocument.Operat
                && CubScena.FaraStoc(os, idDvi)
                && noteCub.Nota(cont4426.ID, cont446.ID, 210m)
                && noteCub.Nota(cont4426.ID, cont4427.ID, 105m)
                && linieTi.ValoareTva == 105m);
            s.Check("D9-A10 DVI: creditul 446 al declarației rămâne fără repartitor — 446 nu urmărește partide și nu cere repartitor (forma îngustă)",
                noteCub.Where(p => p.Credit && p.Cont == cont446.ID).ToList() is { Count: 1 } credit446
                && credit446[0].Repartitor == null && credit446[0].Partener == null);
            var randuriTvaCub = CubScena.Fapte(os, idDvi);
            s.Check("DVI-V5 (privat) jurnalul fiscal: două rânduri de ACHIZIȚIE (direcția vine din `PoliticaTva`), "
                + "amândouă pe contrapartida declarată de politică — biroul vamal, nu furnizorul extern —, cu "
                + "bazele și taxele declarației (1000/210 și 500/105) și cu regimul ca SNAPSHOT pe rând",
                randuriTvaCub.Count == 2
                && randuriTvaCub.All(f => f.Sens == SensTva.Achizitie && f.PartenerId == idVama && !f.Storno)
                && randuriTvaCub.Any(f => f.Regim == RegimTva.Normal && f.Baza == 1000m && f.Tva == 210m)
                && randuriTvaCub.Any(f => f.Regim == RegimTva.TaxareInversa && f.Baza == 500m && f.Tva == 105m));

            // ---- D300: rd. 24 direct, rd. 7 cu oglinda 22 ----
            var d300 = D300Proiectii.D300(os, febStart, febEnd, null);
            D300Rand R(string cod) => d300.Randuri.Single(r => r.Cod == cod);
            Console.WriteLine($"     MĂSURAT (DVI-V6/privat): rd. 24 {Baza0("24")}/{Tva0("24")} → "
                + $"{R("24").Baza}/{R("24").Tva}; rd. 7 {baza7}/{tva7} → {R("7").Baza}/{R("7").Tva}; "
                + $"rd. 22 {R("22").Baza}/{R("22").Tva}; surse rd. 24 „{R("24").Surse}”, rd. 7 „{R("7").Surse}”; "
                + $"{d300.Nemapate.Count} operațiuni nemapate, {d300.Avertismente.Count} avertismente.");
            s.Check("DVI-V6 (privat) decontul: taxa PLĂTITĂ în vamă intră pe rd. 24 (achiziții taxabile 21%, unde "
                + "legea pune și importurile care nu se încadrează la art. 326 alin. (4)–(5)) cu 1000/210, "
                + "amânarea plății pe rd. 7 cu 500/105, iar rd. 22 OGLINDEȘTE rd. 7 — deducerea taxării inverse "
                + "e copia colectării, pusă de proiecție din `RandD300.OglindaA`, nu de o a doua mapare",
                R("24").Baza - baza24 == 1000m && R("24").Tva - tva24 == 210m
                && R("7").Baza - baza7 == 500m && R("7").Tva - tva7 == 105m
                && R("22").Baza == R("7").Baza && R("22").Tva == R("7").Tva
                && R("24").Surse.Contains("IMP21") && R("7").Surse.Contains("IMPTI21")
                && d300.Avertismente.Count == 0);

            // ---- Jurnalul de cumpărări ----
            var jurnal = TvaProiectii.JurnalTva(os, SensTva.Achizitie, febStart, febEnd).ToList()
                .Where(j => j.DocumentId == idDvi).ToList();
            s.Check("DVI-V7 (privat) jurnalul de cumpărări arată declarația cu un rând per tip de TVA, pe "
                + "contrapartida ei, cu codurile SAF-T direcționale ale importului (301204 / 300604)",
                jurnal.Count == 2
                && jurnal.All(j => j.PartenerId == idVama && j.DocumentNumar == dvi.Numar)
                && jurnal.Any(j => j.TipTvaCod == "IMP21" && j.CodSafT == "301204" && j.Baza == 1000m && j.Tva == 210m)
                && jurnal.Any(j => j.TipTvaCod == "IMPTI21" && j.CodSafT == "300604" && j.Baza == 500m && j.Tva == 105m));

            // ---- D394: declarația vamală NU se declară ----
            var d394 = D394Proiectii.D394(os, febStart, febEnd);
            var neincluseDvi = d394.Neincluse
                .Where(n => n.TipTvaCod == "IMP21" || n.TipTvaCod == "IMPTI21").ToList();
            Console.WriteLine($"     MĂSURAT (DVI-V8/privat): {d394.Operatiuni.Count} rânduri op1, din care "
                + $"{d394.Operatiuni.Count(o => o.Denumire == vama.Denumire)} pe biroul vamal; neincluse pe tipuri "
                + $"de import: {string.Join(", ", neincluseDvi.Select(n => $"{n.TipTvaCod}/{n.Cauza} {n.Baza}/{n.Tva}"))}.");
            s.Check("DVI-V8 (privat) declarația vamală NU intră în 394: partenerul extern nu se declară, iar biroul "
                + "vamal nu e o operațiune de raportat — cifrele nu DISPAR însă, ci apar în panoul `Neincluse` cu "
                + "cauza `TipTvaNemapat`, exact disciplina golurilor de profil (decizia 21)",
                !d394.Operatiuni.Any(o => o.Denumire == vama.Denumire)
                && neincluseDvi.Count == 2
                && neincluseDvi.All(n => n.Cauza == "TipTvaNemapat")
                && neincluseDvi.Sum(n => n.Baza) == 1500m && neincluseDvi.Sum(n => n.Tva) == 315m);

            // ---- DVI-r7: codul SAF-T al importului ----
            var saft = SaftProiectii.SaftPeCub(os, 2026, 2);
            if (saft.Neaplicabil != null)
                Console.WriteLine($"     SKIP (DVI-r7): D406 nu se aplică bazei — {saft.Neaplicabil}.");
            else {
                var liniiDvi = saft.Jurnale.SelectMany(j => j.Tranzactii)
                    .Where(t => t.DocumentId == idDvi).SelectMany(t => t.Linii).ToList();
                var coduri = liniiDvi.Where(l => l.TaxInformation != null)
                    .Select(l => l.TaxInformation.TaxCode).Distinct().OrderBy(c => c, StringComparer.Ordinal).ToList();
                Console.WriteLine($"     MĂSURAT (DVI-r7/privat): {liniiDvi.Count} linii de GL ale declarației, "
                    + $"coduri de taxă {string.Join(", ", coduri)}; tabela de taxe conține "
                    + $"{string.Join(", ", saft.Taxe.Select(t => t.TaxCode).Where(c => c.StartsWith("3012") || c.StartsWith("3006")))}.");
                s.Check("DVI-r7 (privat) proiecția D406 iterează faptele fiscale fără filtru de tip de document, deci "
                    + "rândul declarației iese cu codul ei de taxă (301204 pe linia plătită în vamă, 300604 pe cea "
                    + "cu amânarea plății) și amândouă intră în `TaxTable` — restanța se închide în felie, nu se "
                    + "amână",
                    coduri.Contains("301204") && coduri.Contains("300604")
                    && saft.Taxe.Any(t => t.TaxCode == "301204") && saft.Taxe.Any(t => t.TaxCode == "300604"));
            }
        }

        // ---- Gardianul legăturii, PE CALEA REALĂ (DVI-D3) ----
        // Fiecare refuz se măsoară pe un ObjectSpace al lui, ca `ModifiedObjects` să
        // conțină EXACT obiectul sub test — altfel mesajele s-ar cumula.
        string RefuzLegaturaNoua(Guid dviId, Guid facturaId) {
            using var os = s.Provider.CreateObjectSpace();
            var leg = os.CreateObject<DviFactura>();
            leg.Dvi = os.GetObjectByKey<Dvi>(dviId);
            leg.Factura = os.GetObjectByKey<FacturaIntrare>(facturaId);
            var refuz = RefuzGardian(os);
            os.Rollback();
            return refuz;
        }

        string refuzOperat, refuzStergere;
        refuzOperat = RefuzLegaturaNoua(idDvi, idFct);
        using (var os = s.Provider.CreateObjectSpace()) {
            os.Delete(os.GetObjectsQuery<DviFactura>().ToList().First(f => f.DviId == idDvi));
            refuzStergere = RefuzGardian(os);
            os.Rollback();
        }
        Console.WriteLine($"     MĂSURAT (DVI-V9/privat): legătură NOUĂ pe declarație operată → "
            + $"„{refuzOperat?.Split('\n')[0] ?? "<acceptată>"}”; ȘTERGERE pe declarație operată → "
            + $"„{refuzStergere?.Split('\n')[0] ?? "<acceptată>"}”.");
        s.Check("DVI-V9 (privat) legăturile sunt înghețate cu documentul, iar regula e ARMATĂ pe ușa securizată: "
            + "`GardianEditare.Verifica` cheamă `IVerificabilLaCommit` înaintea switch-ului, deci nici crearea, "
            + "nici ȘTERGEREA unei legături nu mai trec cât declarația e Operat — gardul vede și `Delete` "
            + "(fără `EsteSters`), altfel o factură s-ar putea dezlega de pe un document cu registre scrise",
            refuzOperat != null && refuzOperat.Contains("Draft")
            && refuzStergere != null && refuzStergere.Contains("Draft"));

        using (var os = s.Provider.CreateObjectSpace()) {
            var unitate = os.GetObjectByKey<UnitateInterna>(idUnitate);
            var vama = os.GetObjectByKey<Partener>(idVama);
            var imp21 = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP21");
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");

            var draft = os.CreateObject<Dvi>();
            draft.Numar = "26ROBV" + Marcaj + "-B";
            draft.Data = dataDvi;
            draft.Predator = vama;
            draft.Primitor = unitate;
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = draft;
            linie.TipMaterial = tip628;
            linie.TipTva = imp21;
            linie.Valoare = 200m;
            linie.ValoareTva = 42m;
            os.CommitChanges();
            idDviDraft = draft.ID;

            // Factură DRAFT — nu se leagă.
            var fctDraft = os.CreateObject<FacturaIntrare>();
            fctDraft.Numar = Marcaj + "-FF2";
            fctDraft.Data = new DateOnly(2026, 2, 11);
            fctDraft.Predator = os.GetObjectsQuery<Partener>().ToList().First(p => p.Cod == Marcaj + "-EXT");
            fctDraft.Primitor = os.GetObjectsQuery<Gestiune>().ToList().First(g => g.Cod == Marcaj + "-MAG");
            var linieDraft = os.CreateObject<FacturaIntrareDetaliu>();
            linieDraft.Document = fctDraft;
            linieDraft.TipMaterial = tip628;
            linieDraft.Cantitate = 1m;
            linieDraft.PretUnitar = 50m;
            os.CommitChanges();
            idFctDraft = fctDraft.ID;
        }

        var refuzFacturaDraft = RefuzLegaturaNoua(idDviDraft, idFctDraft);
        string acceptata, refuzDubla, refuzEditare;
        using (var os = s.Provider.CreateObjectSpace()) {
            var leg = os.CreateObject<DviFactura>();
            leg.Dvi = os.GetObjectByKey<Dvi>(idDviDraft);
            leg.Factura = os.GetObjectByKey<FacturaIntrare>(idFct);
            acceptata = RefuzGardian(os);
            os.CommitChanges();
        }
        refuzDubla = RefuzLegaturaNoua(idDviDraft, idFct);
        // Aceeași pereche de DOUĂ ori în ACELAȘI commit (grila XAF nested, un singur
        // Save): interogarea nu vede rândurile noi — regula trebuie să vadă
        // `ModifiedObjects`.
        string refuzDublaAcelasiCommit;
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft2 = os.CreateObject<Dvi>();
            draft2.Numar = Marcaj + "-MRN-D2";
            draft2.Data = dataDvi;
            draft2.Predator = os.GetObjectByKey<Repartitor>(idVama);
            draft2.Primitor = os.GetObjectByKey<Repartitor>(idUnitate);
            var fct = os.GetObjectByKey<FacturaIntrare>(idFct);
            var l1 = os.CreateObject<DviFactura>();
            l1.Dvi = draft2;
            l1.Factura = fct;
            var l2 = os.CreateObject<DviFactura>();
            l2.Dvi = draft2;
            l2.Factura = fct;
            refuzDublaAcelasiCommit = RefuzGardian(os);
        }
        Console.WriteLine($"     MĂSURAT (DVI-V10b/privat): aceeași factură de două ori în același commit → „{refuzDublaAcelasiCommit?.Split('\n')[0]}”");
        s.Check("DVI-V10b (privat) unicitatea perechii vede și rândurile NOI ale aceluiași commit (grila XAF cu un singur Save): "
            + "refuz de gardian cu numele facturii, nu 23505 din bază",
            refuzDublaAcelasiCommit != null && refuzDublaAcelasiCommit.Contains("e deja legată"));
        using (var os = s.Provider.CreateObjectSpace()) {
            var editata = os.GetObjectsQuery<DviFactura>().ToList().First(f => f.DviId == idDviDraft);
            editata.FacturaId = idFctDraft;
            refuzEditare = RefuzGardian(os);
            os.Rollback();
        }
        Console.WriteLine($"     MĂSURAT (DVI-V10/privat): factură Draft → „{refuzFacturaDraft?.Split('\n')[0]}”; "
            + $"aceeași factură de două ori → „{refuzDubla?.Split('\n')[0]}”; editare → "
            + $"„{refuzEditare?.Split('\n')[0]}”; legătura validă → {acceptata ?? "acceptată"}.");
        s.Check("DVI-V10 (privat) regulile legăturii pe un draft, tot prin `GardianEditare.Verifica`: factura "
            + "trebuie să fie OPERATĂ, aceeași factură nu se leagă de două ori la aceeași declarație (refuzul e "
            + "de DOMENIU, cu numărul facturii în mesaj, nu un `23505` din index) și legătura nu se editează — "
            + "se șterge și se recreează, ca `Imperechere`",
            refuzFacturaDraft != null && refuzFacturaDraft.Contains("facturi operate")
            && acceptata == null
            && refuzDubla != null && refuzDubla.Contains("deja legată")
            && refuzEditare != null && refuzEditare.Contains("nu se editează"));

        // ---- Refuzurile de OPERARE (DVI-D4) ----
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.GetObjectsQuery<Dvi>().ToList().First(d => d.Numar.EndsWith("-B"));
            var imp = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP");
            var numar = draft.Numar;
            draft.Numar = null;
            s.CheckRefuza("DVI-V11 (privat) declarația fără MRN nu se operează — numărul e al declarației vamale, "
                + "se culege (fără politică de numerotare, ca la FCT)",
                () => MotorOperare.Opereaza(os, draft));
            draft.Numar = numar;
            draft.Detalii.First().TipTva = imp;
            s.CheckRefuza("DVI-V12 (privat) o linie cu `IMP` (cotă 0) nu se operează: TVA-ul în vamă are cotă, iar "
                + "tipul facturii de import n-o poartă — refuzul e pe COTĂ și pe `DeImport`, nu pe un cod "
                + "hardcodat",
                () => MotorOperare.Opereaza(os, draft));
            draft.Detalii.First().TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            s.CheckRefuza("DVI-V13 (privat) o linie cu un tip de TVA intern (N21) nu se operează: declarația poartă "
                + "doar tipuri `DeImport`",
                () => MotorOperare.Opereaza(os, draft));
            draft.Detalii.First().TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP21");
            draft.Predator = os.GetObjectByKey<UnitateInterna>(idUnitate);
            s.CheckRefuza("DVI-V14 (privat) predatorul declarației e un PARTENER (biroul vamal sau comisionarul "
                + "care a plătit taxa), nu un repartitor intern — altfel n-ar exista cui se datorează taxa",
                () => MotorOperare.Opereaza(os, draft));
            os.Rollback();
        }

        // ---- Plata TAXEI (DVI-D4 amendat) și stornoul ----
        // DVI nu e document stins: TVA-ul în vamă se plătește ca orice taxă
        // (precedentul ITV/4423), iar ce rămâne de plătit e SOLDUL lui 446 pe biroul
        // vamal, nu un „rest" al documentului.
        using (var os = s.Provider.CreateObjectSpace()) {
            var dvi = os.GetObjectByKey<Dvi>(idDvi);
            var cont446 = os.FirstOrDefault<Cont>(c => c.Simbol == "446");
            var contPropriu = os.GetObjectByKey<ContPropriu>(idContPropriu);


            var plata = os.CreateObject<Plata>();
            plata.Numar = Marcaj + "-PLT";
            plata.Data = new DateOnly(2026, 2, 20);
            plata.PredatorId = idContPropriu;
            plata.PrimitorId = idVama;
            plata.TipInstrument = TipInstrumentPlata.OrdinPlata;
            var linieP = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieP.Document = plata;
            linieP.TipMaterialId = idTipTrz;
            linieP.Valoare = 210m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, plata);
            var imperecheriPlata = os.GetObjectsQuery<Imperechere>()
                .Count(i => i.DocumentStingatorId == plata.ID || i.DocumentId == plata.ID);
            var fisaVama = ContabilProiectii
                .FisaCont(os, cont446.ID, febStart, febEnd, repartitorId: idVama).ToList();
            Console.WriteLine($"     MĂSURAT (DVI-V15/privat): plata taxei — {CubScena.Note(os, plata.ID).Count} postări"
                + $"; {imperecheriPlata} imperecheri; 446 pe declarație + plată: net "
                + $"{CubScena.SoldCont(os, cont446.ID, null, new[] { idDvi, plata.ID })}; fișa lui 446 filtrată pe "
                + $"biroul vamal: {fisaVama.Count} rând(uri), ultim sold {(fisaVama.Count == 0 ? "-" : fisaVama[^1].SoldCurent.ToString())} "
                + "(446 nu urmărește partide; coordonata Partener rămâne absentă în cub).");
            var notaPlataCub = CubScena.Note(os, plata.ID);
            s.Check("DVI-V15 (privat) TVA-ul în vamă se plătește ca orice TAXĂ, nu prin imperechere (DVI-D4 "
                + "amendat): plata către biroul vamal postează 446 = contul propriu, motorul NU creează nicio "
                + "imperechere, iar contul 446 se închide la ZERO peste declarație + plată, inclusiv în fișa "
                + "citită din cub. Filtrul Partener nu recuperează vechea dimensiune Repartitor: "
                + "446 nu urmărește partide și nici DVI, nici plata nu nominalizează partenerul [cub]",
                notaPlataCub.Count == 2
                && notaPlataCub.Nota(cont446.ID, contPropriu.ContImplicitId, 210m)
                && imperecheriPlata == 0
                && CubScena.SoldCont(os, cont446.ID, null, new[] { idDvi, plata.ID }) == 0m
                && fisaVama.Count == 0
                && ContabilProiectii.FisaCont(os, cont446.ID, febStart, febEnd).ToList() is { Count: 2 } fisaTaxaCub
                && fisaTaxaCub[^1].SoldCurent == 0m);

            // DVI nu apare printre documentele cu rest — `DocumenteCuRest` e o uniune
            // per tip concret (FCT/FCL/PLT/INC/DEC), iar ramura DVI ar cere o regulă
            // de rest polimorfă (DVI-r11).
            var cuRest = ImperecheriProiectii.DocumenteCuRest(os).ToList();
            var cuRestVama = ImperecheriProiectii.DocumenteCuRest(os, idVama).ToList();
            Console.WriteLine($"     MĂSURAT (DVI-V15b/privat): `DocumenteCuRest` — {cuRest.Count(r => r.DocumentId == idDvi)} "
                + $"rânduri ale declarației în lista generală, {cuRestVama.Count(r => r.DocumentId == idDvi)} "
                + $"filtrat pe biroul vamal ({cuRestVama.Count} rânduri în total pe contrapartida asta).");
            s.Check("DVI-V15b (privat) declarația NU apare în `DocumenteCuRest`, nici filtrat pe biroul vamal: "
                + "panoul de stingeri n-o propune, deci nu promite o operațiune pe care serverul ar refuza-o",
                !cuRest.Any(r => r.DocumentId == idDvi) && !cuRestVama.Any(r => r.DocumentId == idDvi));

            var refuzImperechere = s.Refuz(() => ImperechereService.Imperecheaza(os, plata, dvi, 210m));
            Console.WriteLine($"     MĂSURAT (DVI-V15c/privat): imperechere plată → declarație: "
                + $"„{refuzImperechere ?? "<ACCEPTATĂ>"}”.");
            s.Check("DVI-V15c (privat) `ImperechereService` REFUZĂ declarația pe rolul de document STINS, cu refuz "
                + "de DOMENIU (nu excepție brută): `Dvi.PoateFiStins` e false — hook-ul polimorf prin care tipul "
                + "își declară că nu închide nicio datorie —, deci o stingere greșită se oprește la validare, "
                + "nu la cifra restului",
                refuzImperechere != null
                && os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentId == idDvi) == 0);

            MotorOperare.Storneaza(os, dvi, new DateOnly(2026, 2, 25));
            var stornoCub = CubScena.Note(os, idDvi).Where(p => p.Storno).ToList();
            var tvaStornoCub = CubScena.Fapte(os, idDvi).Where(f => f.Storno).ToList();
            s.Check("DVI-V16 (privat) stornoul declarației inversează AMBELE registre la data stornării — două note "
                + "cu valoare negativă și două rânduri fiscale de storno; registrele rămân append-only",
                dvi.Stare == StareDocument.Stornat
                && stornoCub.Count == 4 && stornoCub.All(p => p.Valoare < 0m && p.Data == new DateOnly(2026, 2, 25))
                && tvaStornoCub.Count == 2
                && tvaStornoCub.Sum(f => f.Baza) == -1500m && tvaStornoCub.Sum(f => f.Tva) == -315m);

            // DVI-r2: anularea facturii legate NU se refuză — cifrele declarației nu
            // derivă din factură.
            var fct = os.GetObjectByKey<FacturaIntrare>(idFct);
            MotorOperare.AnuleazaOperarea(os, fct);
            s.Check("DVI-V17 (privat, DVI-r2) anularea unei facturi legate la o declarație NU se refuză: legătura e "
                + "EVIDENȚĂ, nu sursa cifrelor — baza și taxa sunt cele declarate în vamă. Ecranul arată starea "
                + "facturii; o regulă de dependență ar cere un hook nou pe `Document` (restanță)",
                fct.Stare == StareDocument.Draft
                && os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi) == 1);
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataDvi(os);
        using (var os = s.Provider.CreateObjectSpace())
            s.Check("DVI (privat) scena nu lasă urme: partenerii, documentele, legăturile și registrele lor sunt "
                + "purjate FIZIC",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
                && !os.GetObjectsQuery<Dvi>().Any()
                && !os.GetObjectsQuery<DviFactura>().Any()
                && CubScena.FapteIntre(os, febStart, febEnd).Count == 0);
    }
}
