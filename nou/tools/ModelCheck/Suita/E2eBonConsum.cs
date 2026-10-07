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

// ======================== Scenariul e2e 3c: BonConsum ========================
// Consumul: sold de deschidere → operare (−Magazie pe gestiune, +Consum pe
// locul de consum — DOUĂ registre simultan) → contarea 6xx = 3xx din politica
// derivată la seed → gardieni (laturi, sold) → frunză în graful de dependențe:
// anulare directă permisă → storno.
static class E2eBonConsum {
    public static void Ruleaza(Suita s) {
        const string MarcajBcs = "E2E-BCS-PRB";
        const string MarcajLoc = "E2E-BCS-LOC";

        void CurataBcs(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajBcs).Select(l => l.ID).ToList();
            DeschidereScena.Curata(os, pj, loturi);
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
            foreach (var doc in os.GetObjectsQuery<BonConsum>()
                .Where(d => d.Predator.Cod == MarcajLoc || d.Primitor.Cod == MarcajLoc).ToList()) {
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
                pj.Adauga(doc);
            }
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajBcs).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajBcs).ToList());
            pj.Adauga(os.GetObjectsQuery<UnitateInterna>().Where(u => u.Cod == MarcajLoc).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataBcs(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var tipOI = os.FirstOrDefault<TipMaterial>(t => t.Cod == "303.01.00");

            // Politica de contare derivată la seed: 3→6 pe simbol + tăierea segmentelor.
            var regulaMat = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "BCS" && r.TipMaterialId == tipMaterial.ID);
            s.Check("Seed BCS: 302.01.00 → debit 602.01.00 (potrivire exactă)",
                regulaMat != null && regulaMat.ContDebit?.Simbol == "602.01.00"
                && regulaMat.SursaContCredit == SursaCont.TipMaterial);
            var regulaOI = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "BCS" && r.TipMaterialId == tipOI.ID);
            s.Check("Seed BCS: 303.01.00 → debit 603 (tăierea segmentelor spre sintetic)",
                regulaOI != null && regulaOI.ContDebit?.Simbol == "603");

            var loc = os.CreateObject<UnitateInterna>();
            loc.Cod = MarcajLoc;
            loc.Denumire = "Loc de consum probă e2e";
            loc.Calitati = CalitateRepartitor.LocConsum;
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajBcs;
            produs.Denumire = "Produs probă BCS";
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;
            var lot = os.CreateObject<Lot>();
            lot.Produs = produs;
            lot.PretUnitar = 10m;
            lot.Gestiune = mag1;
            lot.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lot, 10m, 100m);
            os.CommitChanges();

            BonConsum Consum(Repartitor dinspre, Repartitor spre, decimal cantitate, DateOnly data) {
                var doc = os.CreateObject<BonConsum>();
                doc.Data = data;
                doc.Predator = dinspre;
                doc.Primitor = spre;
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc;
                d.TipMaterial = tipMaterial;
                d.Lot = lot;
                d.Cantitate = cantitate;
                return doc;
            }

            // --- Validările laturilor: predator gestiune, primitor cu calitatea LocConsum ---
            var gresit = Consum(loc, mag1, 1m, new DateOnly(2026, 3, 5));
            s.CheckRefuza("Laturi greșite (predator non-gestiune, primitor fără LocConsum) → refuz",
                () => MotorOperare.Opereaza(os, gresit));
            os.Delete(gresit.Detalii.ToList());
            os.Delete(gresit);

            // --- Operare: două registre simultan + contarea 602 = 302 ---
            var bcs1 = Consum(mag1, loc, 4m, new DateOnly(2026, 3, 5));
            s.Check("BCS nu generează conex", MotorOperare.Opereaza(os, bcs1) == null);
            s.Check("Operare → stare Operat + număr din politică",
                bcs1.Stare == StareDocument.Operat && bcs1.Numar?.StartsWith("BCS-") == true);
            s.Check("Operare → valoarea liniei = preț lot × cantitate", bcs1.Detalii.Single().Valoare == 40m);
            var stocBcs1Cub = CubScena.Stoc(os, bcs1.ID);
            s.Check("Operare → −4/−40 Magazie pe MAG1", stocBcs1Cub.Any(p =>
                p.Cont == tipMaterial.ContImplicitId && p.Gestiune == mag1.ID && p.Cantitate == -4m && p.Semnata == -40m));
            s.Check("Operare → +4/+40 Consum pe locul de consum", stocBcs1Cub.Any(p =>
                p.Cont == regulaMat.ContDebitId && p.Gestiune == loc.ID && p.Cantitate == 4m && p.Semnata == 40m));
            decimal SoldCub(Repartitor r, Guid? cont) => CubScena.Sold(os, lot.ID, r.ID, cont).Cantitate;
            s.Check("Solduri: Magazie MAG1=6, Consum loc=4",
                SoldCub(mag1, tipMaterial.ContImplicitId) == 6m && SoldCub(loc, regulaMat.ContDebitId) == 4m);
            var noteBcsCub = CubScena.Note(os, bcs1.ID);
            s.Check("Contare: 602.01.00 = 302.01.00 (creditul din contul Tipului), 40",
                noteBcsCub.Count == 2
                && noteBcsCub.Nota(regulaMat.ContDebitId, tipMaterial.ContImplicitId, 40m));
            s.Check("D9-A10 BCS: creditul de stoc poartă gestiunea predatoare MAG1, debitul de consum locul primitor",
                noteBcsCub.Single(p => p.Credit).Gestiune == mag1.ID && noteBcsCub.Single(p => p.Debit).Gestiune == loc.ID);

            // --- NUC-BCS (B-D4, pas 3): declarantul frunzei ---
            ProbeNucleu.Proba(os, s.Check, "NUC-BCS", [bcs1]);

            // --- Gardianul de sold: consum peste disponibil ---
            var pesteDisponibil = Consum(mag1, loc, 100m, new DateOnly(2026, 3, 10));
            s.CheckRefuza("Consum peste disponibil → refuz", () => MotorOperare.Opereaza(os, pesteDisponibil));
            os.Delete(pesteDisponibil.Detalii.ToList());
            os.Delete(pesteDisponibil);
            os.CommitChanges();

            // --- Frunză în graful de dependențe (03): corecția directă merge oricând ---
            MotorOperare.AnuleazaOperarea(os, bcs1);
            s.Check("Anulare BCS → Draft + solduri revenite (Magazie 10, Consum 0)",
                bcs1.Stare == StareDocument.Draft
                && SoldCub(mag1, tipMaterial.ContImplicitId) == 10m && SoldCub(loc, regulaMat.ContDebitId) == 0m);
            MotorOperare.Opereaza(os, bcs1);

            // --- Storno: inverse pe AMBELE registre la data stornării ---
            MotorOperare.Storneaza(os, bcs1, new DateOnly(2026, 7, 22));
            s.Check("Storno BCS → 4 rânduri stoc (2 + 2 inverse) și nota inversată",
                bcs1.Stare == StareDocument.Stornat
                && CubScena.Note(os, bcs1.ID).Where(p => p.Storno).ToList()
                    .Nota(regulaMat.ContDebitId, tipMaterial.ContImplicitId, -40m));
            s.Check("Storno → solduri nete: Magazie 10, Consum 0",
                SoldCub(mag1, tipMaterial.ContImplicitId) == 10m && SoldCub(loc, regulaMat.ContDebitId) == 0m);

            CurataBcs(os);
            s.Check("Curățenie finală BCS (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajBcs)
                && !os.GetObjectsQuery<UnitateInterna>().Any(u => u.Cod == MarcajLoc));
        }
    }
}
