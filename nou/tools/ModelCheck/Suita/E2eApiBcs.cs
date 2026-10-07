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

// ========= Scenariul e2e pasul 5 / felia 6: Api BCS scriere (F6-D11) =========
// Consumul cules manual, parcurs prin contractul feliei: WriteDto →
// `BonConsumApply.Aplica` → `Citeste`/`Lista` → dry-run → `ComenziDocument.Opereaza`
// → cele DOUĂ registre de stoc (−Magazie predator, +Consum primitor — 27a).
//
// Ce exersează în plus față de blocul e2e „3c: BonConsum" (care probează
// MOTORUL, construind documentele direct în ObjectSpace):
//   * culegerea prin AGREGAT — reconcilierea liniilor pe `Id`, refuzurile de
//     payload, ștergerea agregatului;
//   * `Valoare` materializată LA CULEGERE din prețul lotului (F6-D6), cu
//     golirea la 0 a liniei rămase fără lot — ce hook-ul de operare nu face;
//   * seria „BCS-" NECONSUMATĂ la un refuz de operare (F6-D4 + GATE D6);
//   * affordances oneste (F6-D7).
// Rulează pe profilul BUGETAR: BCS n-are `PoliticaTva` în niciun profil (F6-D5).
static class E2eApiBcs {
    public static void Ruleaza(Suita s) {
        const string MarcajApiBcs = "E2E-API-BCS";

        void CurataApiBcs(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            // Documentele probei se găsesc prin laturi (BCS n-are număr cules — seria e
            // server-owned), loturile și produsul prin marcaj.
            var idsDoc = os.GetObjectsQuery<BonConsum>()
                .Where(d => d.Predator.Cod.StartsWith(MarcajApiBcs) || d.Primitor.Cod.StartsWith(MarcajApiBcs))
                .Select(d => d.ID).ToList();
            idsDoc.AddRange(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiBcs))
                .Select(d => d.DocumentId).ToList());
            idsDoc = idsDoc.Distinct().ToList();

            var idsLot = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiBcs))
                .Select(l => l.ID).ToList();
            DeschidereScena.Curata(os, pj, idsLot);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiBcs)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiBcs)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiBcs)).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiBcs(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

            // Locul de consum e CALITATE transversală (27b), nu clasă: orice repartitor
            // intern o poate purta. Latura o validează motorul, nu tierul.
            var loc = os.CreateObject<UnitateInterna>();
            loc.Cod = MarcajApiBcs + "-LOC";
            loc.Denumire = "Loc de consum probă felia Api BCS";
            loc.Calitati = CalitateRepartitor.LocConsum;
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajApiBcs + "-A";
            produs.Denumire = "Produs A probă felia Api BCS";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            os.CommitChanges();

            var dataBcs = new DateOnly(2026, 3, 18);
            // Lotul de consumat + soldul lui de deschidere: BCS DESCARCĂ loturi, nu le
            // naște (liniile lui nu declară `ILinieCareNasteLot`).
            var lot = os.CreateObject<Lot>();
            lot.Produs = produs;
            lot.PretUnitar = 10m;
            lot.Gestiune = mag1;
            lot.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lot, 20m, 200m);
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunBcs(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }
            int SerieBcs() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BCS").UrmatorulNumar;

            // --- Apply: consumul cules, fără Numar/Valoare în payload ---
            var writeBcs = new BcsWriteDto {
                Data = dataBcs,
                PredatorId = mag1.ID,
                PrimitorId = loc.ID,
                Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 4m } }
            };
            var idBcs = BonConsumApply.Aplica(os, null, writeBcs);
            var citit = BonConsumApply.Citeste(os, idBcs);
            s.Check("Apply BCS → header plat, FĂRĂ număr (seria „BCS-” e server-owned, se consumă la operare)",
                citit != null && citit.Id == idBcs && citit.Stare == "Draft" && citit.Numar == null
                && citit.Data == dataBcs
                && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
                && citit.PrimitorId == loc.ID && citit.PrimitorDenumire == loc.Denumire
                && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);
            s.Check("Valoarea consumului materializată LA CULEGERE din prețul LOTULUI (F6-D6): 4 × 10 = 40, Total 40 — nu 0 până la operare",
                citit.Linii.Single().Valoare == 40m && citit.Total == 40m
                && citit.Linii.Single().Cantitate == 4m
                && citit.Linii.Single().TipMaterialCod == "302.01.00"
                && citit.Linii.Single().LotId == lot.ID
                && citit.Linii.Single().LotEticheta == lot.Eticheta);
            var randBcs = BonConsumApply.Lista(os).Single(d => d.Id == idBcs);
            s.Check("Lista BCS: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
                randBcs.Stare == "Draft" && randBcs.Total == 40m && randBcs.Numar == null
                && randBcs.PredatorDenumire == mag1.Denumire && randBcs.PrimitorDenumire == loc.Denumire);

            // --- Reconcilierea colecției (upsert pe Id) + golirea valorii fără lot ---
            var idLinieBcs = citit.Linii.Single().Id;
            writeBcs.Linii[0].Id = idLinieBcs;
            writeBcs.Linii[0].Cantitate = 6m;
            BonConsumApply.Aplica(os, idBcs, writeBcs);
            s.Check("Reconciliere pe Id: cantitatea schimbată → valoarea o urmează (6 × 10 = 60)",
                BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);
            BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
                Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
                Linii = { new BcsLinieWriteDto { Id = idLinieBcs, TipMaterialId = tipMateriale.ID, Cantitate = 6m } }
            });
            s.Check("Lotul scos de pe linie → valoarea se GOLEȘTE la 0 (valoarea veche ar minți pe ecran — ce hook-ul de operare nu face)",
                BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 0m
                && BonConsumApply.Citeste(os, idBcs).Linii.Single().LotId == null);
            BonConsumApply.Aplica(os, idBcs, writeBcs);
            s.Check("Lotul repus → valoarea revine (6 × 10 = 60)",
                BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);

            s.CheckRefuza("Apply BCS cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
                    Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
                    Linii = { new BcsLinieWriteDto { Id = Guid.NewGuid(), TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
                }));
            s.CheckRefuza("Apply BCS cu același Id de linie de două ori → refuz (a doua apariție ar suprascrie tăcut prima)", () =>
                BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
                    Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
                    Linii = { writeBcs.Linii[0], writeBcs.Linii[0] }
                }));
            s.CheckRefuza("Apply BCS cu cantitate în afara scării numeric(18,3) → refuz de domeniu, nu DbUpdateException", () =>
                BonConsumApply.Aplica(s.OsCuGardian(), idBcs, new BcsWriteDto {
                    Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
                    Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 0.0001m } }
                }));
            BonConsumApply.Aplica(os, idBcs, writeBcs);
            s.Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact o linie",
                BonConsumApply.Citeste(os, idBcs).Linii.Count == 1);

            // --- Refuzurile de OPERARE, fiecare fără rânduri-fantomă și fără serie consumată ---
            var serieInainteBcs = SerieBcs();
            void RefuzBcs(string nume, Guid predatorId, Guid primitorId, BcsLinieWriteDto linieProba) {
                var id = BonConsumApply.Aplica(os, null, new BcsWriteDto {
                    Data = dataBcs, PredatorId = predatorId, PrimitorId = primitorId, Linii = { linieProba }
                });
                s.CheckRefuza(nume, () => ComenziDocument.Sistem(os).Opereaza(id));
                s.Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                    CubScena.FaraStoc(os, id)
                    && CubScena.FaraNote(os, id)
                    && os.GetObjectByKey<BonConsum>(id).Stare == StareDocument.Draft
                    && os.GetObjectByKey<BonConsum>(id).Numar == null);
                BonConsumApply.Sterge(os, id);
            }

            RefuzBcs("Laturi inversate (predator fără gestiune, primitor fără LocConsum) → refuz de domeniu la operare",
                loc.ID, mag1.ID,
                new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 1m });
            RefuzBcs("Linie de consum FĂRĂ lot → refuz („descărcarea e pe lot” — draftul avea voie să fie incomplet, operarea nu)",
                mag1.ID, loc.ID,
                new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, Cantitate = 1m });
            RefuzBcs("Cantitate ≤ 0 pe linia de consum → refuz",
                mag1.ID, loc.ID,
                new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 0m });
            RefuzBcs("Consum peste disponibil → refuz al gardianului de sold",
                mag1.ID, loc.ID,
                new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 999m });
            s.Check("Seria „BCS-” NU se consumă la refuz (F6-D4 + GATE D6: numărul se asignează abia la materializare)",
                SerieBcs() == serieInainteBcs);

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul BCS valid → listă goală", DryRunBcs(idBcs).Count == 0);
            s.Check("Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și fără număr",
                os.GetObjectByKey<BonConsum>(idBcs).Stare == StareDocument.Draft
                && os.GetObjectByKey<BonConsum>(idBcs).Numar == null
                && CubScena.FaraStoc(os, idBcs));

            var rezBcs = ComenziDocument.Sistem(os).Opereaza(idBcs);
            citit = BonConsumApply.Citeste(os, idBcs);
            s.Check("ComenziDocument.Opereaza pe BCS → Operat, cu număr din politica proprie (seria BCS-), fără conex; affordances inversate",
                rezBcs.StareNoua == StareDocument.Operat && rezBcs.ConexId == null
                && citit.Numar?.StartsWith("BCS-") == true && citit.DataOperare != null
                && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna
                && SerieBcs() == serieInainteBcs + 1);
            decimal SoldBcsCub(Repartitor r, Guid? cont = null) => CubScena.Sold(os, lot.ID, r.ID, cont).Cantitate;
            var stocBcsCub = CubScena.Stoc(os, idBcs);
            s.Check("BCS operat → DOUĂ registre simultan: −6/−60 Magazie pe gestiune, +6/+60 Consum pe locul de consum (27a)",
                stocBcsCub.Count == 2
                && stocBcsCub.Any(p => p.Cont == tipMateriale.ContImplicitId && p.Gestiune == mag1.ID
                    && p.Cantitate == -6m && p.Semnata == -60m)
                && stocBcsCub.Any(p => p.Cont != tipMateriale.ContImplicitId && p.Gestiune == loc.ID
                    && p.Cantitate == 6m && p.Semnata == 60m));
            s.Check("Solduri după operare: Magazie 14, Consum 6",
                SoldBcsCub(mag1, tipMateriale.ContImplicitId) == 14m && SoldBcsCub(loc) == 6m);
            s.Check("Valoarea culeasă e cea postată: hook-ul de operare rescrie aceeași formulă (geamăna F6-D6)",
                BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);

            // --- NUC-BCS-API (B-D4, pas 3): declarantul pe documentul operat prin ușa API ---
            ProbeNucleu.Proba(os, s.Check, "NUC-BCS-API", [os.GetObjectByKey<BonConsum>(idBcs)]);
            s.CheckRefuza("Apply peste BCS Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => BonConsumApply.Aplica(os, idBcs, writeBcs));
            s.CheckRefuza("Sterge peste BCS Operat → același refuz de domeniu",
                () => BonConsumApply.Sterge(os, idBcs));

            // --- Anulare (BCS e frunză în graful de dependențe — 27d) și storno ---
            s.Check("Anulare prin API → Draft + solduri revenite (Magazie 20, Consum 0)",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idBcs).StareNoua == StareDocument.Draft
                && SoldBcsCub(mag1, tipMateriale.ContImplicitId) == 20m && SoldBcsCub(loc) == 0m
                && CubScena.FaraStoc(os, idBcs));
            ComenziDocument.Sistem(os).Opereaza(idBcs);
            s.Check("Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete revenite",
                ComenziDocument.Sistem(os).Storneaza(idBcs, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
                && CubScena.Stoc(os, idBcs).Count == 4
                && CubScena.Stoc(os, idBcs).Count(p => p.Storno) == 2
                && SoldBcsCub(mag1, tipMateriale.ContImplicitId) == 20m && SoldBcsCub(loc) == 0m);

            // --- Sterge: draftul dispare cu tot cu linii, lotul NU (e al altcuiva) ---
            var idBcs2 = BonConsumApply.Aplica(os, null, new BcsWriteDto {
                Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
                Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 1m } }
            });
            BonConsumApply.Sterge(os, idBcs2);
            s.Check("Sterge pe draftul BCS: documentul și liniile dispar, dar LOTUL rămâne (consumul nu naște loturi, îl descarcă)",
                BonConsumApply.Citeste(os, idBcs2) == null
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idBcs2)
                && os.GetObjectByKey<Lot>(lot.ID) != null);

            CurataApiBcs(os);
            s.Check("Curățenie finală felia Api BCS (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiBcs))
                && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiBcs))
                && os.GetObjectByKey<BonConsum>(idBcs) == null);
        }
    }
}
