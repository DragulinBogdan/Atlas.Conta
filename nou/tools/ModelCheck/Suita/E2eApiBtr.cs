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

// =============== Scenariul e2e pasul 5 / spike 1: felia BTR (D1/D8/D9) ===============
// Același obiect de studiu ca 3b (transferul), dar parcurs prin CONTRACTUL
// feliei, nu prin entități: WriteDto → `NotaTransferApply.Aplica` → `Citeste` /
// `Lista` → dry-run `ComenziDocument.Valideaza` → comenzile `ComenziDocument`. Endpoint-urile
// din host sunt transport peste EXACT acest cod (D1: DTO-uri + Apply în Module,
// fără ASP.NET), deci ce e verde aici e verde și pe sârmă — controllerul nu mai
// poate ascunde o regulă.
//
// Reciclează marcajele lui 3b (produs `E2E-PRB`, `NumarPV = "E2E"`), deci `Curata`
// de mai sus acoperă și reziduurile acestui bloc.
static class E2eApiBtr {
    public static void Ruleaza(Suita s) {
        using (var os = s.Provider.CreateObjectSpace()) {
            E2eCurata.Curata(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
            var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

            var produs = os.CreateObject<Produs>();
            produs.Cod = E2eCurata.MarcajProdus;
            produs.Denumire = "Produs probă felia BTR";
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;

            var lot = os.CreateObject<Lot>();
            lot.Produs = produs;
            lot.PretUnitar = 10m;
            lot.Gestiune = mag1;
            lot.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lot, 10m, 100m);
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU, aruncat după apel: `Valideaza`
            // rulează `PregatesteOperare`, care SCRIE pe linii (contractul lui
            // MotorOperare.Valideaza). Pe calea vie, OS-ul ăsta e cel non-secured al
            // endpoint-ului `POST .../valideaza`.
            IReadOnlyList<string> DryRun(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // --- Apply: creare din WriteDto (fără Stare/Numar/Valoare — server-owned) ---
            var write = new NotaTransferWriteDto {
                Data = new DateOnly(2026, 3, 5),
                PredatorId = mag1.ID,
                PrimitorId = mag2.ID,
                NumarPV = "E2E",
                DataPV = new DateOnly(2026, 3, 4),
                Linii = { new NotaTransferLinieWriteDto { TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 4m } }
            };
            var idBtr = NotaTransferApply.Aplica(os, null, write);
            var citit = NotaTransferApply.Citeste(os, idBtr);
            s.Check("Apply creare → header proiectat plat (laturi cu denumiri, PV, sursă) + affordances de Draft",
                citit != null && citit.Id == idBtr && citit.Stare == "Draft" && citit.Numar == null
                && citit.Data == new DateOnly(2026, 3, 5) && citit.DataPV == new DateOnly(2026, 3, 4)
                && citit.NumarPV == "E2E"
                && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
                && citit.PrimitorId == mag2.ID && citit.PrimitorDenumire == mag2.Denumire
                && !citit.Autogenerat && citit.DocumentSursaId == null
                && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);
            s.Check("Apply creare → o linie, cu eticheta lotului identică celei din model (oglinda lui Lot.Eticheta)",
                citit.Linii.Count == 1 && citit.Linii[0].LotId == lot.ID
                && citit.Linii[0].LotEticheta == lot.Eticheta
                && citit.Linii[0].TipMaterialCod == tipMaterial.Cod
                && citit.Linii[0].Cantitate == 4m);
            s.Check("104c: Apply materializează `Valoare` la culegere prin formula tipului (prețul lotului × cantitate), ca operarea",
                citit.Linii[0].Valoare == Scara.RotunjesteBani(4m * lot.PretUnitar) && citit.Total == citit.Linii[0].Valoare);

            // --- Apply: reconcilierea colecției (update + insert, apoi delete) ---
            var idLinie = citit.Linii[0].Id;
            write.Linii[0].Id = idLinie;
            write.Linii[0].Cantitate = 3m;
            write.Linii.Add(new NotaTransferLinieWriteDto { TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 1m });
            NotaTransferApply.Aplica(os, idBtr, write);
            citit = NotaTransferApply.Citeste(os, idBtr);
            s.Check("Apply update → linia cu Id se actualizează, linia fără Id se adaugă",
                citit.Linii.Count == 2
                && citit.Linii.Single(l => l.Id == idLinie).Cantitate == 3m
                && citit.Linii.Sum(l => l.Cantitate) == 4m);

            write.Linii.RemoveAt(1);
            write.Linii[0].Cantitate = 4m;
            NotaTransferApply.Aplica(os, idBtr, write);
            citit = NotaTransferApply.Citeste(os, idBtr);
            s.Check("Apply → linia absentă din payload se ȘTERGE (reconciliere server-side, nu CRUD per linie)",
                citit.Linii.Count == 1 && citit.Linii[0].Id == idLinie && citit.Linii[0].Cantitate == 4m
                && os.GetObjectsQuery<DocumentDetaliu>().Count(l => l.DocumentId == idBtr) == 1);

            s.CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                NotaTransferApply.Aplica(os, idBtr, new NotaTransferWriteDto {
                    Data = write.Data, PredatorId = mag1.ID, PrimitorId = mag2.ID, NumarPV = "E2E",
                    Linii = { new NotaTransferLinieWriteDto {
                        Id = Guid.NewGuid(), TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 1m } }
                }));

            // --- Dry-run: valid, apoi stricat deliberat ---
            var politicaBtrApi = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
            var numarInainteApi = politicaBtrApi.UrmatorulNumar;

            s.Check("Dry-run (Valideaza) pe draft valid → listă goală", DryRun(idBtr).Count == 0);

            write.PrimitorId = mag1.ID; // aceeași gestiune pe ambele laturi
            NotaTransferApply.Aplica(os, idBtr, write);
            var eroriDry = DryRun(idBtr);
            s.Check("Dry-run pe draft stricat → eroarea de domeniu a tipului, ca DATE",
                eroriDry.Count > 0 && eroriDry.Any(e => e.Contains("difere")));

            // Proba că dry-run-ul e chiar DRY: nimic materializat, nici măcar numărul
            // (care în `Opereaza` se consumă abia în faza de materializare — GATE XAF D6).
            using (var osVerif = s.Provider.CreateObjectSpace()) {
                var docVerif = osVerif.GetObjectByKey<NotaTransfer>(idBtr);
                var polVerif = osVerif.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
                s.Check("Dry-run NU materializează nimic: zero rânduri de registru, număr neconsumat, stare Draft",
                    CubScena.FaraStoc(osVerif, idBtr)
                    && CubScena.FaraNote(osVerif, idBtr)
                    && string.IsNullOrWhiteSpace(docVerif.Numar)
                    && docVerif.Stare == StareDocument.Draft
                    && polVerif.UrmatorulNumar == numarInainteApi);
            }

            write.PrimitorId = mag2.ID;
            NotaTransferApply.Aplica(os, idBtr, write);
            s.Check("Dry-run pe draftul reparat → din nou listă goală", DryRun(idBtr).Count == 0);

            // --- Comenzile prin adaptor: rezultatul e DATE, nu entitate ---
            var rezultatOperare = ComenziDocument.Sistem(os).Opereaza(idBtr);
            s.Check("ComenziDocument.Opereaza → OperareRezultat cu StareNoua=Operat, fără conex (BTR n-are politică)",
                rezultatOperare.DocumentId == idBtr && rezultatOperare.StareNoua == StareDocument.Operat
                && rezultatOperare.ConexId == null && rezultatOperare.Mesaje.Count == 0);
            s.Check("OperareRezultatDto → starea traversează sârma ca TEXT",
                OperareRezultatDto.Din(rezultatOperare).StareNoua == "Operat");

            citit = NotaTransferApply.Citeste(os, idBtr);
            s.Check("Citeste după operare → Numar din politică, Total și Valoare materializate de motor, affordances inversate",
                citit.Stare == "Operat" && citit.Numar?.StartsWith("BTR-") == true && citit.DataOperare != null
                && citit.Total == 40m && citit.Linii[0].Valoare == 40m
                && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna);

            s.CheckRefuza("Apply peste un document Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => NotaTransferApply.Aplica(os, idBtr, write));

            // --- Lista: proiecție IQueryable, traductibilă integral în SQL ---
            var randuriLista = NotaTransferApply.Lista(os).Where(x => x.Id == idBtr).ToList();
            s.Check("Lista → un rând, cu Stare ca text (CASE în SQL) și Total din agregatul liniilor",
                randuriLista.Count == 1 && randuriLista[0].Stare == "Operat" && randuriLista[0].Total == 40m
                && randuriLista[0].Numar == citit.Numar
                && randuriLista[0].PredatorDenumire == mag1.Denumire
                && randuriLista[0].PrimitorDenumire == mag2.Denumire);

            var idGol = NotaTransferApply.Aplica(os, null, new NotaTransferWriteDto {
                Data = new DateOnly(2026, 3, 6), PredatorId = mag1.ID, PrimitorId = mag2.ID, NumarPV = "E2E"
            });
            s.Check("Lista → draftul FĂRĂ linii apare cu Total 0 (LEFT JOIN pe agregat, nu subquery corelat)",
                NotaTransferApply.Lista(os).Any(x => x.Id == idGol && x.Total == 0m));
            s.Check("Lista → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: sort + take)",
                NotaTransferApply.Lista(os).Where(x => x.Stare == "Draft")
                    .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

            // --- D9: proiecția de sold == StocService, per cheie ---
            var proiectie = StocProiectii.SoldStoc(os).Where(r => r.LotId == lot.ID).ToList();
            s.Check("Proiecția SoldStoc → exact cheile mișcate de scenariu (MAG1 6, MAG2 4), cu valoarea agregată",
                proiectie.Count == 2
                && proiectie.Single(r => r.RepartitorId == mag1.ID) is { Cantitate: 6m, Valoare: 60m }
                && proiectie.Single(r => r.RepartitorId == mag2.ID) is { Cantitate: 4m, Valoare: 40m });
            s.Check("Proiecția poartă etichetele plate ale lotului și ale gestiunii (fără navigație lazy per rând)",
                proiectie.All(r => r.ProdusCod == E2eCurata.MarcajProdus && r.ProdusDenumire == produs.Denumire
                    && r.ProdusUM == "BUC" && r.LotData == lot.Data && r.LotPretUnitar == 10m)
                && proiectie.Single(r => r.RepartitorId == mag1.ID).GestiuneDenumire == mag1.Denumire);
            s.Check("D9: proiecția == cititorul operațional pe cheia completă",
                proiectie.All(r => r.Cantitate == Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Solduri(os)
                    .Single(s => s.LotId == r.LotId && s.ContId == r.ContId && s.ProdusId == r.ProdusId
                        && s.GestiuneId == r.RepartitorId).Cantitate));

            // --- Anulare → re-operare → storno, tot prin adaptor ---
            var rezultatAnulare = ComenziDocument.Sistem(os).AnuleazaOperarea(idBtr);
            s.Check("ComenziDocument.AnuleazaOperarea → Draft, registrele proprii șterse, affordances de Draft",
                rezultatAnulare.StareNoua == StareDocument.Draft
                && CubScena.FaraStoc(os, idBtr)
                && NotaTransferApply.Citeste(os, idBtr).PoateEdita);
            ComenziDocument.Sistem(os).Opereaza(idBtr);
            var rezultatStorno = ComenziDocument.Sistem(os).Storneaza(idBtr, new DateOnly(2026, 7, 22));
            s.Check("ComenziDocument.Storneaza → Stornat + rânduri inverse la data cerută; nicio afordanță rămasă",
                rezultatStorno.StareNoua == StareDocument.Stornat
                && CubScena.Stoc(os, idBtr).Where(p => p.Storno).ToList() is { Count: > 0 } stornoApiBtr
                && stornoApiBtr.All(p => p.Data == new DateOnly(2026, 7, 22))
                && NotaTransferApply.Citeste(os, idBtr) is
                    { Stare: "Stornat", PoateEdita: false, PoateOpera: false, PoateAnula: false, PoateStorna: false });

            E2eCurata.Curata(os);
            s.Check("Curățenie finală felia BTR (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod == E2eCurata.MarcajProdus)
                && !os.GetObjectsQuery<NotaTransfer>().Any(d => d.NumarPV == "E2E"));
        }
    }
}
