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

// ============================ Scenariul e2e 3b ============================
// NotaTransfer end-to-end: sold de deschidere → operare (2 rânduri ±) →
// gardieni (sold intermediar, retroactiv, perioadă, dependență) → FIFO →
// anulare (corecție directă) → storno. Obiectele de test poartă marcajul E2E
// și se curăță la început (run eșuat anterior) și la sfârșit.
static class E2eNotaTransfer {
    public static void Ruleaza(Suita s) {
        using (var os = s.Provider.CreateObjectSpace()) {
            E2eCurata.Curata(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
            var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

            var produs = os.CreateObject<Produs>();
            produs.Cod = E2eCurata.MarcajProdus;
            produs.Denumire = "Produs probă e2e";
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;

            // Sold de deschidere (decizia 12): lot + rând de registru FĂRĂ document sursă.
            var lot = os.CreateObject<Lot>();
            lot.Produs = produs;
            lot.PretUnitar = 10m;
            lot.Gestiune = mag1;
            lot.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lot, 10m, 100m);
            os.CommitChanges();

            NotaTransfer Transfer(Gestiune dinspre, Gestiune spre, decimal cantitate, DateOnly data) {
                var doc = os.CreateObject<NotaTransfer>();
                doc.Data = data;
                doc.Predator = dinspre;
                doc.Primitor = spre;
                doc.NumarPV = "E2E";
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc;
                d.TipMaterial = tipMaterial;
                d.Lot = lot;
                d.Cantitate = cantitate;
                return doc;
            }
            decimal SoldCub(Gestiune g) => CubScena.Sold(os, lot.ID, g.ID).Cantitate;

            // --- Operare: 2 rânduri ±, valoarea din prețul lotului, numerotare ---
            var btr1 = Transfer(mag1, mag2, 4m, new DateOnly(2026, 3, 5));
            MotorOperare.Opereaza(os, btr1);
            s.Check("Operare → stare Operat + DataOperare", btr1.Stare == StareDocument.Operat && btr1.DataOperare != null);
            s.Check("Operare → număr asignat din politică", btr1.Numar?.StartsWith("BTR-") == true);

            var stocBtr1Cub = CubScena.Stoc(os, btr1.ID);
            s.Check("Operare → −4/−40 pe MAG1", stocBtr1Cub.Any(p =>
                p.Gestiune == mag1.ID && p.Cantitate == -4m && p.Semnata == -40m && p.Data == btr1.Data && !p.Storno));
            s.Check("Operare → +4/+40 pe MAG2", stocBtr1Cub.Any(p =>
                p.Gestiune == mag2.ID && p.Cantitate == 4m && p.Semnata == 40m && p.Data == btr1.Data && !p.Storno));
            s.Check("Operare → valoarea liniei = preț lot × cantitate", btr1.Detalii.Single().Valoare == 40m);

            s.Check("Solduri după transfer: MAG1=6, MAG2=4",
                SoldCub(mag1) == 6m && SoldCub(mag2) == 4m);

            s.CheckRefuza("Re-operarea unui document Operat e refuzată", () => MotorOperare.Opereaza(os, btr1));

            // --- Gardianul de sold: cerere peste disponibil ---
            // BTR are politică de numerotare (BTR-), deci documentul ăsta e sonda pentru
            // GATE XAF D6: numărul se consumă în faza de MATERIALIZARE, după toți
            // gardienii. Înainte, `AsignaNumar` rula între validare și gardianul de sold —
            // un refuz lăsa numărul pe document ȘI incrementul pe politică în
            // ObjectSpace-ul VIU al apelantului (UI-ul rulează motorul în OS-ul
            // View-ului), iar orice Save ulterior le persista: gol în seria fiscală.
            var politicaBtr = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
            var numarInainte = politicaBtr.UrmatorulNumar;
            var insuficient = Transfer(mag2, mag1, 100m, new DateOnly(2026, 3, 10));
            s.CheckRefuza("Sold insuficient → operare refuzată", () => MotorOperare.Opereaza(os, insuficient));
            s.Check("Refuzul unui gardian NU consumă număr (D6): document fără Numar, politica neatinsă",
                string.IsNullOrWhiteSpace(insuficient.Numar) && politicaBtr.UrmatorulNumar == numarInainte);
            os.Delete(insuficient.Detalii.ToList());
            os.Delete(insuficient);

            // --- Gardianul retroactiv: minus inserat în urmă rupe un prefix ulterior ---
            var retro = Transfer(mag1, mag2, 7m, new DateOnly(2026, 2, 1)); // feb: 3, mar: −1
            s.CheckRefuza("Operare retroactivă care duce soldul sub 0 → refuzată", () => MotorOperare.Opereaza(os, retro));
            os.Delete(retro.Detalii.ToList());
            os.Delete(retro);

            // --- Gardianul de perioadă ---
            var inAfara = Transfer(mag1, mag2, 1m, new DateOnly(2025, 12, 15));
            s.CheckRefuza("Perioadă nedefinită → refuz", () => MotorOperare.Opereaza(os, inAfara));
            // F27-D1: închiderea trece prin COMANDA motorului, iar lanțul cere P−1
            // închisă. Luna probată e deci o perioadă de SCENĂ dintr-un an liber (2029),
            // capăt de lanț prin absența precedentei — nu 06/2026 din seed, care ar fi
            // cerut închiderea lunilor 1–5.
            const int AnScenaPerioada = 2029;
            var scenaPerioada = os.CreateObject<PerioadaFiscala>();
            scenaPerioada.An = AnScenaPerioada;
            scenaPerioada.Luna = 6;
            os.CommitChanges();
            s.InchideLant(os, AnScenaPerioada, 6);
            inAfara.Data = new DateOnly(AnScenaPerioada, 6, 5);
            s.CheckRefuza("Perioadă închisă → refuz", () => MotorOperare.Opereaza(os, inAfara));
            s.RedeschideLant(os, AnScenaPerioada, 6);
            os.Delete(inAfara.Detalii.ToList());
            os.Delete(inAfara);
            os.CommitChanges();
            s.PurjaIstoricPerioade(os, AnScenaPerioada);
            new Purja(os).Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == AnScenaPerioada)).Executa();

            // --- Dependența pe loturi: BTR2 consumă din MAG2 ce a adus BTR1 ---
            var btr2 = Transfer(mag2, mag1, 4m, new DateOnly(2026, 4, 1));
            MotorOperare.Opereaza(os, btr2);
            s.Check("BTR2 operat (MAG2 golit)",
                SoldCub(mag2) == 0m && SoldCub(mag1) == 10m);
            s.CheckRefuza("Anularea BTR1 cu dependent (BTR2) → refuzată", () => MotorOperare.AnuleazaOperarea(os, btr1));
            s.CheckRefuza("Stornarea BTR1 cu dependent (BTR2) → refuzată", () =>
                MotorOperare.Storneaza(os, btr1, new DateOnly(2026, 7, 22)));

            // --- Corecția directă: anularea ultimului din lanț e permisă ---
            MotorOperare.AnuleazaOperarea(os, btr2);
            s.Check("Anulare BTR2 → Draft + rânduri șterse",
                btr2.Stare == StareDocument.Draft && btr2.DataOperare == null && CubScena.FaraStoc(os, btr2.ID));
            s.Check("Solduri revenite: MAG1=6, MAG2=4",
                SoldCub(mag1) == 6m && SoldCub(mag2) == 4m);
            MotorOperare.Opereaza(os, btr2);
            s.Check("Re-operare BTR2 după corecție", btr2.Stare == StareDocument.Operat);
            MotorOperare.AnuleazaOperarea(os, btr2);
            os.Delete(btr2.Detalii.ToList());
            os.Delete(btr2);
            os.CommitChanges();

            // --- Storno: rânduri inverse la data stornării, append-only ---
            MotorOperare.Storneaza(os, btr1, new DateOnly(2026, 7, 22));
            var stornoBtr1Cub = CubScena.Stoc(os, btr1.ID).Where(p => p.Storno).ToList();
            s.Check("Storno → stare Stornat", btr1.Stare == StareDocument.Stornat);
            s.Check("Storno → 4 rânduri (2 operare + 2 inverse marcate)",
                stornoBtr1Cub.Count > 0 && stornoBtr1Cub.All(p => p.Data == new DateOnly(2026, 7, 22)));
            s.Check("Storno → solduri nete: MAG1=10, MAG2=0",
                SoldCub(mag1) == 10m && SoldCub(mag2) == 0m);

            E2eCurata.Curata(os);
            s.Check("Curățenie finală (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod == E2eCurata.MarcajProdus));
        }
    }
}
