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

// ===================== Scenariul e2e 3c: FacturaIesire =====================
// Facturarea: pur creanță (411 = 7xx), fără registru de stoc → numerotare
// proprie din politică (serie fiscală) → scadență default +30 din politică
// (fără să suprascrie scadența culeasă) → contare per linie cu creditul din
// contul de venit al Tipului și debitul particularizat prin ContImplicit al
// clientului (461) → validări laturi + refuz linii de stoc → anulare directă
// → storno.
static class E2eFacturaIesire {
    public static void Ruleaza(Suita s) {
        const string MarcajFcl = "E2E-FCL";

        void CurataFcl(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            foreach (var doc in os.GetObjectsQuery<FacturaIesire>()
                .Where(d => d.Primitor.Cod.StartsWith(MarcajFcl) || d.Predator.Cod.StartsWith(MarcajFcl)).ToList()) {
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
                pj.Adauga(doc);
            }
            pj.Adauga(os.GetObjectsQuery<Partener>().Where(p => p.Cod.StartsWith(MarcajFcl)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajFcl + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataFcl(os);

            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipServiciiVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "751.01.00");
            var tipChirii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "750.02.00");
            var tipStocMat = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
            var cont461 = os.FirstOrDefault<Cont>(c => c.Simbol == "461.01.01");
            var cap19Fcl = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

            // Politica derivată la seed: tipurile de venit cu contul din simbol.
            s.Check("Seed: Tip 751.01.00 (clasa VEN) → cont 751.01.00",
                tipServiciiVenit?.ContImplicitId != null
                && os.GetObjectByKey<Cont>(tipServiciiVenit.ContImplicitId.Value).Simbol == "751.01.00");
            var regulaFcl = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "FCL");
            s.Check("Seed FCL: debit RepartitorPrimitor (fallback 411.01.01), credit TipMaterial",
                regulaFcl != null && regulaFcl.SursaContDebit == SursaCont.RepartitorPrimitor
                && regulaFcl.ContDebitId == cont411.ID && regulaFcl.SursaContCredit == SursaCont.TipMaterial);
            s.Check("Seed FCL: fără reguli de stoc (pur creanță)",
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "FCL"));
            var politicaScadenta = os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocument.Cod == "FCL");
            s.Check("Seed FCL: politică de scadență +30", politicaScadenta?.ZileDefault == 30);
            s.Check("Seed 3d: FCL interzice natura Stoc (PoliticaValidare, fostul hardcode 30a)",
                os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL")?.NaturaInterzisa == NaturaClasa.Stoc);

            var client = os.CreateObject<Partener>();
            client.Cod = MarcajFcl + "-CL1";
            client.Denumire = "Client probă e2e";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajFcl + "-CE";
            codEc.Denumire = "Clasificație de venit probă e2e";
            os.CommitChanges();

            FacturaIesireDetaliu Linie(FacturaIesire doc, TipMaterial tip, decimal cantitate, decimal pret, TipTva tipTva = null) {
                var d = os.CreateObject<FacturaIesireDetaliu>();
                d.Document = doc;
                d.TipMaterial = tip;
                d.Cantitate = cantitate;
                d.PretUnitar = pret;
                d.TipTva = tipTva;
                return d;
            }

            // --- Validările laturilor + refuzul liniilor de stoc ---
            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = new DateOnly(2026, 3, 5);
            fcl.Predator = client; // inversat intenționat
            fcl.Primitor = sediu;
            Linie(fcl, tipServiciiVenit, 2m, 100m).Descriere = "Servicii refacturate";
            s.CheckRefuza("Laturi inversate (predator partener / primitor intern) → refuz",
                () => MotorOperare.Opereaza(os, fcl));
            fcl.Predator = sediu;
            fcl.Primitor = client;

            var linieStoc = Linie(fcl, tipStocMat, 1m, 5m);
            s.CheckRefuza("Linie de stoc pe factura de ieșire → refuz (nu descarcă gestiune)",
                () => MotorOperare.Opereaza(os, fcl));
            os.Delete(linieStoc);
            Linie(fcl, tipChirii, 1m, 50m).Descriere = "Chirie spațiu";
            os.CommitChanges();

            // Conturile de venit (751/750) poartă defalcarea E — clasificația de venit
            // e cerută la nivel de CONT (3d), nu de tip (FCL nu are rând de politică).
            s.CheckRefuza("Venituri fără cod economic (751/750 cer E) → refuz",
                () => MotorOperare.Opereaza(os, fcl));
            foreach (var d in fcl.Detalii.OfType<FacturaIesireDetaliu>())
                d.CodEconomicId = codEc.ID;
            os.CommitChanges();

            // --- Operare: serie fiscală + scadență default + o notă per linie ---
            s.Check("FCL nu generează conex", MotorOperare.Opereaza(os, fcl) == null);
            s.Check("Operare → stare Operat + număr din seria fiscală",
                fcl.Stare == StareDocument.Operat && fcl.Numar?.StartsWith("FCL-") == true);
            s.Check("Scadența default din politică: data + 30",
                fcl.DataScadenta == fcl.Data.AddDays(30));
            s.Check("FCL nu mișcă stoc",
                CubScena.FaraStoc(os, fcl.ID));
            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID);
            var noteCub = NoteCub(fcl);
            s.Check("Contare servicii: 411.01.01 = 751.01.00, 200",
                noteCub.Nota(cont411.ID, tipServiciiVenit.ContImplicitId, 200m));
            s.Check("Contare chirie: 411.01.01 = 750.02.00, 50",
                noteCub.Nota(cont411.ID, tipChirii.ContImplicitId, 50m));
            s.Check("D9-A10 FCL: debitul 411 poartă clientul (partida lui), creditul de venit gestiunea emitentă SEDIU",
                noteCub.Where(p => p.Cont == cont411.ID).All(p => p.Repartitor == client.ID)
                && noteCub.Where(p => p.Credit).All(p => p.Gestiune == sediu.ID));

            // --- Debit particularizat (461) + scadență culeasă + TVA în valoare ---
            var clientDebitor = os.CreateObject<Partener>();
            clientDebitor.Cod = MarcajFcl + "-CL2";
            clientDebitor.Denumire = "Debitor cu cont propriu";
            clientDebitor.ContImplicit = cont461;
            var fcl2 = os.CreateObject<FacturaIesire>();
            fcl2.Data = new DateOnly(2026, 3, 6);
            fcl2.Predator = sediu;
            fcl2.Primitor = clientDebitor;
            fcl2.DataScadenta = new DateOnly(2026, 12, 31); // culeasă manual
            Linie(fcl2, tipServiciiVenit, 1m, 100m, cap19Fcl).CodEconomicId = codEc.ID;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl2);
            var debitFcl2Cub = NoteCub(fcl2).Where(p => p.Debit).ToList();
            s.Check("Debitul din ContImplicit al clientului (461, nu fallback 411)",
                debitFcl2Cub.Count > 0 && debitFcl2Cub.All(p => p.Cont == cont461.ID));
            s.Check("Valoarea postată include TVA (o singură Valoare pe linie: 119)",
                debitFcl2Cub.Sum(p => p.Valoare) == 119m);
            s.Check("Scadența culeasă manual nu se suprascrie",
                fcl2.DataScadenta == new DateOnly(2026, 12, 31));
            MotorOperare.Storneaza(os, fcl2, new DateOnly(2026, 7, 22));

            // --- Fără stoc = fără dependenți: anulare directă mereu permisă → storno ---
            MotorOperare.AnuleazaOperarea(os, fcl);
            s.Check("Anulare FCL → Draft + notele șterse",
                fcl.Stare == StareDocument.Draft && CubScena.FaraNote(os, fcl.ID));
            MotorOperare.Opereaza(os, fcl);
            s.Check("Re-operare după corecție (numărul asignat rămâne)",
                fcl.Stare == StareDocument.Operat && fcl.Numar?.StartsWith("FCL-") == true);
            MotorOperare.Storneaza(os, fcl, new DateOnly(2026, 7, 22));
            var stornoFclCub = NoteCub(fcl).Where(p => p.Storno && p.Data == new DateOnly(2026, 7, 22)).ToList();
            s.Check("Storno FCL → note inverse append-only (−200, −50) la data stornării",
                fcl.Stare == StareDocument.Stornat
                && stornoFclCub.Count(p => p.Debit && (p.Valoare == -200m || p.Valoare == -50m)) == 2
                && stornoFclCub.Count(p => p.Credit && (p.Valoare == -200m || p.Valoare == -50m)) == 2);

            CurataFcl(os);
            s.Check("Curățenie finală FCL (fără reziduuri e2e)",
                !os.GetObjectsQuery<Partener>().Any(p => p.Cod.StartsWith(MarcajFcl)));
        }
    }
}
