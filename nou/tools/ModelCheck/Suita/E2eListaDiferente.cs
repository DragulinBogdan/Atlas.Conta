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

// ================= Scenariul e2e 3c: ListaDiferenteInventar =================
// Inventarierea: sold de deschidere → LDI cu minus (descarcă lotul existent)
// și plus (creează lot nou cu preț de evaluare) pe ACEEAȘI listă → un singur
// set de reguli de stoc (+1 predator, cantitatea semnată dă direcția) →
// contare pe direcție prin SemnFiltru (minus 6xx = 3xx pozitiv, plus
// 3xx = 791) → gardieni (laturi, direcție, sold) → anulare directă → storno.
static class E2eListaDiferente {
    public static void Ruleaza(Suita s) {
        const string MarcajLdi = "E2E-LDI-PRB";
        const string MarcajComisie = "E2E-LDI-COM";

        void CurataLdi(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajLdi).Select(l => l.ID).ToList();
            DeschidereScena.Curata(os, pj, loturi);
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
            foreach (var doc in os.GetObjectsQuery<ListaDiferenteInventar>()
                .Where(d => d.Primitor.Cod == MarcajComisie).ToList()) {
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
                pj.Adauga(doc);
            }
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajLdi).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajLdi).ToList());
            pj.Adauga(os.GetObjectsQuery<UnitateInterna>().Where(u => u.Cod == MarcajComisie).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajLdi + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataLdi(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var cont791 = os.FirstOrDefault<Cont>(c => c.Simbol == "791.00.00");

            // Politica derivată la seed: minusul per Tip cu filtru de semn, plusul generic.
            var regulaMinus = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == tipMaterial.ID);
            s.Check("Seed LDI: minus 302.01.00 → debit 602.01.00, SemnFiltru=-1",
                regulaMinus != null && regulaMinus.ContDebit?.Simbol == "602.01.00"
                && regulaMinus.SemnFiltru == -1 && regulaMinus.SursaContCredit == SursaCont.TipMaterial);
            var regulaPlus = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == null);
            s.Check("Seed LDI: plus generic → credit 791.00.00, SemnFiltru=+1",
                regulaPlus != null && regulaPlus.SemnFiltru == 1
                && regulaPlus.SursaContDebit == SursaCont.TipMaterial && regulaPlus.ContCreditId == cont791.ID);
            s.Check("Seed: comisia de inventariere poartă calitatea Comisie",
                os.FirstOrDefault<UnitateInterna>(u => u.Cod == "COMISIE")?.Calitati.HasFlag(CalitateRepartitor.Comisie) == true);

            var comisie = os.CreateObject<UnitateInterna>();
            comisie.Cod = MarcajComisie;
            comisie.Denumire = "Comisie probă e2e";
            comisie.Calitati = CalitateRepartitor.Comisie;
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajLdi + "-CE";
            codEc.Denumire = "Cod economic probă LDI";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajLdi;
            produs.Denumire = "Produs probă LDI";
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;
            var lotVechi = os.CreateObject<Lot>();
            lotVechi.Produs = produs;
            lotVechi.PretUnitar = 10m;
            lotVechi.Gestiune = mag1;
            lotVechi.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lotVechi, 10m, 100m);
            os.CommitChanges();

            decimal SoldCub(Lot lot) => CubScena.Sold(os, lot.ID, mag1.ID).Cantitate;

            // --- LDI bidirecțional: minus pe lotul existent + plus cu lot nou ---
            var ldi = os.CreateObject<ListaDiferenteInventar>();
            ldi.Data = new DateOnly(2026, 3, 5);
            ldi.Predator = mag1;
            ldi.Primitor = comisie;
            var linieMinus = os.CreateObject<ListaDiferenteInventarDetaliu>();
            linieMinus.Document = ldi;
            linieMinus.TipMaterial = tipMaterial;
            linieMinus.Directie = DirectieDiferenta.Minus;
            linieMinus.Lot = lotVechi;
            linieMinus.Cantitate = 2m; // UI-ul culege pozitiv; semnul îl pune operarea
            var liniePlus = os.CreateObject<ListaDiferenteInventarDetaliu>();
            liniePlus.Document = ldi;
            liniePlus.TipMaterial = tipMaterial;
            liniePlus.Directie = DirectieDiferenta.Plus;
            liniePlus.Cantitate = 3m;
            liniePlus.LotFabricatie = "LOT-P";

            // Validările proprii: lot pe plus + preț de evaluare + laturile.
            s.CheckRefuza("Plus fără lot creat / fără preț de evaluare → refuz", () => MotorOperare.Opereaza(os, ldi));
            var lotNou = liniePlus.CreeazaLot(os, produs, mag1);
            liniePlus.PretEvaluare = 7m;
            ldi.Primitor = mag1; // gestiune fără calitatea Comisie
            s.CheckRefuza("Primitor fără calitatea Comisie → refuz", () => MotorOperare.Opereaza(os, ldi));
            ldi.Primitor = comisie;
            os.CommitChanges();

            // Venitul plusului (791) poartă defalcarea E — cerută pe nota rezolvată (3d);
            // minusul (602 = 302, ambele S) nu cere nimic.
            s.CheckRefuza("Plus fără cod economic (791 cere E) → refuz", () => MotorOperare.Opereaza(os, ldi));
            liniePlus.CodEconomicId = codEc.ID;
            os.CommitChanges();

            // --- Operare: direcția materializată în semn, două rânduri ± pe predator ---
            s.Check("LDI nu generează conex", MotorOperare.Opereaza(os, ldi) == null);
            s.Check("Operare → stare Operat + număr din politică",
                ldi.Stare == StareDocument.Operat && ldi.Numar?.StartsWith("LDI-") == true);
            s.Check("Minus: direcția materializată în semn (−2 / −20)",
                linieMinus.Cantitate == -2m && linieMinus.Valoare == -20m);
            s.Check("Plus: cantitate pozitivă, valoarea din prețul de evaluare (+3 / +21)",
                liniePlus.Cantitate == 3m && liniePlus.Valoare == 21m);
            s.Check("Lot nou finalizat: preț 7, data documentului, atribute copiate",
                lotNou.PretUnitar == 7m && lotNou.Data == ldi.Data && lotNou.LotFabricatie == "LOT-P");
            var stocLdiCub = CubScena.Stoc(os, ldi.ID);
            s.Check("Operare → 2 rânduri de stoc, ambele Magazie pe gestiunea inventariată",
                stocLdiCub.Count == 2 && stocLdiCub.All(p => p.Cont == tipMaterial.ContImplicitId && p.Gestiune == mag1.ID));
            s.Check("Minus → −2/−20 pe lotul vechi", stocLdiCub.Any(p =>
                p.Unitate == lotVechi.ID && p.Cantitate == -2m && p.Semnata == -20m));
            s.Check("Plus → +3/+21 pe lotul nou", stocLdiCub.Any(p =>
                p.Unitate == lotNou.ID && p.Cantitate == 3m && p.Semnata == 21m));
            s.Check("Solduri: lot vechi 8, lot nou 3",
                SoldCub(lotVechi) == 8m && SoldCub(lotNou) == 3m);
            var noteLdiCub = CubScena.Note(os, ldi.ID);
            s.Check("Contare minus: 602.01.00 = 302.01.00, POZITIVĂ (normalizată cu semnul filtrului)",
                noteLdiCub.Nota(regulaMinus.ContDebitId, tipMaterial.ContImplicitId, 20m));
            s.Check("Contare plus: 302.01.00 = 791.00.00, 21",
                noteLdiCub.Nota(tipMaterial.ContImplicitId, cont791.ID, 21m));

            // --- Gardianul de sold: minus peste disponibil ---
            var pesteDisponibil = os.CreateObject<ListaDiferenteInventar>();
            pesteDisponibil.Data = new DateOnly(2026, 3, 10);
            pesteDisponibil.Predator = mag1;
            pesteDisponibil.Primitor = comisie;
            var lipsaMare = os.CreateObject<ListaDiferenteInventarDetaliu>();
            lipsaMare.Document = pesteDisponibil;
            lipsaMare.TipMaterial = tipMaterial;
            lipsaMare.Directie = DirectieDiferenta.Minus;
            lipsaMare.Lot = lotVechi;
            lipsaMare.Cantitate = 100m;
            s.CheckRefuza("Minus peste disponibil → refuz", () => MotorOperare.Opereaza(os, pesteDisponibil));
            os.Delete(pesteDisponibil.Detalii.ToList());
            os.Delete(pesteDisponibil);
            os.CommitChanges();

            // --- Anulare directă (lotul nou neatins de alții) + re-operare ---
            MotorOperare.AnuleazaOperarea(os, ldi);
            s.Check("Anulare LDI → Draft + solduri revenite (vechi 10, nou 0)",
                ldi.Stare == StareDocument.Draft && SoldCub(lotVechi) == 10m && SoldCub(lotNou) == 0m);
            MotorOperare.Opereaza(os, ldi);
            s.Check("Re-operare după corecție (semnul rămâne idempotent)",
                ldi.Stare == StareDocument.Operat && linieMinus.Cantitate == -2m && liniePlus.Cantitate == 3m);

            // --- Storno: inverse pe ambele direcții și pe note ---
            MotorOperare.Storneaza(os, ldi, new DateOnly(2026, 7, 22));
            var stornoLdiCub = CubScena.Note(os, ldi.ID).Where(p => p.Storno).ToList();
            s.Check("Storno LDI → 4 rânduri stoc (2 + 2 inverse) și notele inversate (−20, −21)",
                ldi.Stare == StareDocument.Stornat
                && stornoLdiCub.Nota(regulaMinus.ContDebitId, tipMaterial.ContImplicitId, -20m)
                && stornoLdiCub.Nota(tipMaterial.ContImplicitId, cont791.ID, -21m));
            s.Check("Storno → solduri nete: vechi 10, nou 0",
                SoldCub(lotVechi) == 10m && SoldCub(lotNou) == 0m);

            CurataLdi(os);
            s.Check("Curățenie finală LDI (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajLdi)
                && !os.GetObjectsQuery<UnitateInterna>().Any(u => u.Cod == MarcajComisie));
        }
    }
}
