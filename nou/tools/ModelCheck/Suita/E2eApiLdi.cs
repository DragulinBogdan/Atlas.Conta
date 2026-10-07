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

// ========= Scenariul e2e pasul 5 / felia 6: Api LDI scriere (F6-D11) =========
// Inventarierea culeasă manual, prin contractul feliei: WriteDto →
// `ListaDiferenteInventarApply.Aplica` → `Citeste`/`Lista` → dry-run →
// `ComenziDocument.Opereaza` → registre. Singurul tip BIDIRECȚIONAL: plusul NAȘTE
// lotul (ca o recepție manuală), minusul descarcă unul existent.
//
// TESTUL-ANCORĂ AL FELIEI (F6-D2): lotul plusului se naște în gestiunea
// INVENTARIATĂ — PREDATORUL, prin hook-ul `GestiuneLoturiCulese`. Default-ul
// bazei e primitorul, iar primitorul LDI e COMISIA (nu e `Gestiune`), deci fără
// override serviciul ar tăcea pentru totdeauna și mesajul „alegeți produsul" ar
// fi neîndeplinibil — exact golul pe care F5 l-a închis pe NIR.
// Rulează pe profilul BUGETAR: LDI n-are `PoliticaTva` în niciun profil (F6-D5).
static class E2eApiLdi {
    public static void Ruleaza(Suita s) {
        const string MarcajApiLdi = "E2E-API-LDI";

        void CurataApiLdi(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var idsDoc = os.GetObjectsQuery<ListaDiferenteInventar>()
                .Where(d => d.Primitor.Cod.StartsWith(MarcajApiLdi))
                .Select(d => d.ID).ToList();
            idsDoc.AddRange(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiLdi))
                .Select(d => d.DocumentId).ToList());
            idsDoc.AddRange(os.GetObjectsQuery<ListaDiferenteInventarDetaliu>()
                .Where(d => d.Produs.Cod.StartsWith(MarcajApiLdi))
                .Select(d => d.DocumentId).ToList());
            idsDoc = idsDoc.Distinct().ToList();

            var idsLot = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiLdi))
                .Select(l => l.ID).ToList();
            DeschidereScena.Curata(os, pj, idsLot);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiLdi)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiLdi)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiLdi)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiLdi)).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiLdi(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

            // Comisia de inventariere e CALITATE transversală (28d), nu clasă — și, mai
            // ales, NU e `Gestiune`: exact motivul pentru care hook-ul de gestiune al
            // loturilor culese trebuie să arate spre PREDATOR.
            var comisie = os.CreateObject<UnitateInterna>();
            comisie.Cod = MarcajApiLdi + "-COM";
            comisie.Denumire = "Comisie de inventariere probă felia Api LDI";
            comisie.Calitati = CalitateRepartitor.Comisie;
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajApiLdi + "-CE";
            codEc.Denumire = "Cod economic probă felia Api LDI";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajApiLdi + "-A";
            produs.Denumire = "Produs A probă felia Api LDI";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            os.CommitChanges();

            var dataLdi = new DateOnly(2026, 3, 20);
            var lotVechi = os.CreateObject<Lot>();
            lotVechi.Produs = produs;
            lotVechi.PretUnitar = 10m;
            lotVechi.Gestiune = mag1;
            lotVechi.Data = new DateOnly(2026, 1, 10);
            DeschidereScena.Scrie(os, lotVechi, 10m, 100m);
            os.CommitChanges();

            IReadOnlyList<string> DryRunLdi(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }
            int SerieLdi() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "LDI").UrmatorulNumar;

            // --- Apply: lista bidirecțională, culeasă cu cantități POZITIVE ---
            var writeLdi = new LdiWriteDto {
                Data = dataLdi,
                PredatorId = mag1.ID,
                PrimitorId = comisie.ID,
                Linii = {
                    new LdiLinieWriteDto {
                        Directie = "Minus", TipMaterialId = tipMateriale.ID,
                        LotId = lotVechi.ID, Cantitate = 2m
                    },
                    new LdiLinieWriteDto {
                        Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                        Cantitate = 3m, PretEvaluare = 7m,
                        LotFabricatie = "LOT-PLUS", DataExpirare = new DateOnly(2027, 9, 30),
                        CodEconomicId = codEc.ID
                    }
                }
            };
            var idLdi = ListaDiferenteInventarApply.Aplica(os, null, writeLdi);
            var citit = ListaDiferenteInventarApply.Citeste(os, idLdi);
            s.Check("Apply LDI → header plat, FĂRĂ număr (seria „LDI-” e server-owned, se consumă la operare)",
                citit != null && citit.Id == idLdi && citit.Stare == "Draft" && citit.Numar == null
                && citit.Data == dataLdi
                && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
                && citit.PrimitorId == comisie.ID && citit.PrimitorDenumire == comisie.Denumire
                && citit.Linii.Count == 2
                && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

            var linieMinus = citit.Linii.Single(l => l.Directie == "Minus");
            var liniePlus = citit.Linii.Single(l => l.Directie == "Plus");
            var lotPlus = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == liniePlus.Id);
            s.Check("TESTUL-ANCORĂ (F6-D2): lotul PLUSULUI se naște pe linia proprie, din ProdusId, în gestiunea INVENTARIATĂ (PREDATORUL — hook-ul GestiuneLoturiCulese), nefinalizat",
                lotPlus != null && lotPlus.ProdusId == produs.ID && lotPlus.GestiuneId == mag1.ID
                && lotPlus.GestiuneId != comisie.ID
                && lotPlus.Data == default && lotPlus.PretUnitar == 0m
                && liniePlus.LotId == lotPlus.ID
                && liniePlus.LotEticheta == lotPlus.Eticheta
                && liniePlus.LotEticheta.Contains("(în culegere)"));
            s.Check("Minusul PINUIEȘTE un lot existent și rămâne NEATINS de serviciu (gardul de lot străin): niciun lot propriu pe linia de minus",
                linieMinus.LotId == lotVechi.ID
                && !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId == linieMinus.Id)
                && linieMinus.ProdusId == null && linieMinus.PretEvaluare == null);
            s.Check("Valoarea SEMNATĂ la culegere (F6-D6): minus −2 × 10 = −20, plus +3 × 7 = +21, Total = efectul NET (+1)",
                linieMinus.Valoare == -20m && liniePlus.Valoare == 21m && citit.Total == 1m);
            s.Check("Cantitatea rămâne POZITIVĂ până la operare (semnarea ei e a operării — 28a)",
                linieMinus.Cantitate == 2m && liniePlus.Cantitate == 3m);
            s.Check("Linia de plus poartă produsul, prețul de evaluare, atributele de lot și dimensiunea frunzei, proiectate plat",
                liniePlus.ProdusId == produs.ID && liniePlus.ProdusCod == produs.Cod
                && liniePlus.ProdusDenumire == produs.Denumire && liniePlus.PretEvaluare == 7m
                && liniePlus.LotFabricatie == "LOT-PLUS" && liniePlus.DataExpirare == new DateOnly(2027, 9, 30)
                && liniePlus.CodEconomicId == codEc.ID && liniePlus.CodEconomicCod == codEc.Cod);
            var randLdi = ListaDiferenteInventarApply.Lista(os).Single(d => d.Id == idLdi);
            s.Check("Lista LDI: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
                randLdi.Stare == "Draft" && randLdi.Total == 1m && randLdi.Numar == null
                && randLdi.PredatorDenumire == mag1.Denumire && randLdi.PrimitorDenumire == comisie.Denumire);

            // --- PUT repetat: lotul plusului NU se dublează (F6-D5: LotId din payload
            //     e ecoul ReadDto-ului, nu o intenție — pe plus se ignoră) ---
            writeLdi.Linii[0].Id = linieMinus.Id;
            writeLdi.Linii[1].Id = liniePlus.Id;
            writeLdi.Linii[1].LotId = liniePlus.LotId;   // exact ce ar retrimite clientul
            ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
            ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
            var dupaPut = ListaDiferenteInventarApply.Citeste(os, idLdi);
            s.Check("PUT repetat identic pe Plus → ACELAȘI lot, unul singur (round-trip-ul LotId nu re-leagă și nu dublează)",
                os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == liniePlus.Id) == 1
                && dupaPut.Linii.Single(l => l.Directie == "Plus").LotId == lotPlus.ID
                && dupaPut.Linii.Single(l => l.Directie == "Minus").LotId == lotVechi.ID);

            // --- Refuzurile de payload ---
            s.CheckRefuza("Apply LDI cu direcție necunoscută → refuz care ENUMERĂ valorile valide (parse pe NUME, la graniță)", () =>
                ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                    Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                    Linii = { new LdiLinieWriteDto { Directie = "Ambele", TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
                }));
            s.CheckRefuza("Apply LDI FĂRĂ direcție → același refuz (enum-ul n-are default valid — 28e; linia ar fi oricum ne-operabilă)", () =>
                ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                    Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                    Linii = { new LdiLinieWriteDto { TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
                }));
            s.CheckRefuza("Apply LDI cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                ListaDiferenteInventarApply.Aplica(os, idLdi, new LdiWriteDto {
                    Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                    Linii = { new LdiLinieWriteDto {
                        Id = Guid.NewGuid(), Directie = "Minus", TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
                }));
            s.CheckRefuza("Apply LDI cu același Id de linie de două ori → refuz", () =>
                ListaDiferenteInventarApply.Aplica(os, idLdi, new LdiWriteDto {
                    Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                    Linii = { writeLdi.Linii[0], writeLdi.Linii[0] }
                }));
            s.CheckRefuza("Apply LDI cu preț de evaluare în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
                ListaDiferenteInventarApply.Aplica(s.OsCuGardian(), idLdi, new LdiWriteDto {
                    Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                    Linii = { new LdiLinieWriteDto {
                        Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                        Cantitate = 1m, PretEvaluare = 0.0000001m } }
                }));
            ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
            s.Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact două linii",
                ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Count == 2);

            // --- Comutarea de direcție Plus→Minus, pe un document propriu ---
            var idComut = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 5m, PretEvaluare = 4m, LotFabricatie = "LOT-COMUT",
                    DataExpirare = new DateOnly(2028, 1, 31), CodEconomicId = codEc.ID } }
            });
            var linieComut = ListaDiferenteInventarApply.Citeste(os, idComut).Linii.Single();
            var lotComut = linieComut.LotId.Value;
            ListaDiferenteInventarApply.Aplica(os, idComut, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Id = linieComut.Id, Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    // Ce ar retrimite clientul după comutare: pinul nou + reziduul
                    // câmpurilor de plus, pe care Apply are obligația să le GOLEASCĂ.
                    LotId = lotVechi.ID, Cantitate = 5m, ProdusId = produs.ID, PretEvaluare = 4m,
                    LotFabricatie = "LOT-COMUT", DataExpirare = new DateOnly(2028, 1, 31) } }
            });
            var dupaComut = ListaDiferenteInventarApply.Citeste(os, idComut).Linii.Single();
            s.Check("Comutare Plus→Minus prin PUT: lotul propriu NEFINALIZAT e ȘTERS (gardul NasteLot, F6-D3), pinul nou se aplică",
                !os.GetObjectsQuery<Lot>().Any(l => l.ID == lotComut)
                && dupaComut.Directie == "Minus" && dupaComut.LotId == lotVechi.ID);
            s.Check("Comutare Plus→Minus: câmpurile plusului sunt GOLITE, nu doar ignorate (F6-D3 — „inert devine adevărat”)",
                dupaComut.ProdusId == null && dupaComut.PretEvaluare == null
                && dupaComut.DataExpirare == null && dupaComut.LotFabricatie == null);
            s.Check("Comutare Plus→Minus: valoarea se re-materializează semnat, din prețul lotului PINUIT (−5 × 10 = −50)",
                dupaComut.Valoare == -50m);
            ListaDiferenteInventarApply.Sterge(os, idComut);
            s.Check("Sterge după comutare: documentul dispare, iar lotul PINUIT (al altcuiva) rămâne intact",
                ListaDiferenteInventarApply.Citeste(os, idComut) == null
                && os.GetObjectByKey<Lot>(lotVechi.ID) != null);

            // --- PUT care SCHIMBĂ PREDATORUL după nașterea lotului (review, gaura 1:
            //     jumătatea neexersată a testului-ancoră — gestiunea lotului îl urmează,
            //     ramura de sincronizare din LoturiCulegereService) ---
            var mag2Ldi = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
            var idMutat = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 2m, PretEvaluare = 4m, CodEconomicId = codEc.ID } }
            });
            var linieMutata = ListaDiferenteInventarApply.Citeste(os, idMutat).Linii.Single();
            ListaDiferenteInventarApply.Aplica(os, idMutat, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag2Ldi.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Id = linieMutata.Id, Directie = "Plus", TipMaterialId = tipMateriale.ID,
                    ProdusId = produs.ID, Cantitate = 2m, PretEvaluare = 4m, CodEconomicId = codEc.ID } }
            });
            s.Check("PUT care schimbă PREDATORUL după nașterea lotului → gestiunea lotului îl urmează (gaura 1 din review)",
                mag2Ldi != null
                && os.GetObjectByKey<Lot>(linieMutata.LotId.Value).GestiuneId == mag2Ldi.ID);
            ListaDiferenteInventarApply.Sterge(os, idMutat);

            // --- Self-healing pe linia „istorică": lot FINALIZAT + ProdusId null ---
            // `ProdusId` e coloană NOUĂ pe frunza LDI (migrația F6): pe liniile scrise
            // înainte de felie e null deși lotul e finalizat. Fără distincția din serviciu
            // (review GATE D1, replicat de F5), orice PUT le-ar ȘTERGE lotul — ID, dată
            // reală, preț, poziție FIFO — fără nicio eroare.
            var docIstoric = os.CreateObject<ListaDiferenteInventar>();
            docIstoric.Data = dataLdi;
            docIstoric.Predator = mag1;
            docIstoric.Primitor = comisie;
            var linieIstorica = os.CreateObject<ListaDiferenteInventarDetaliu>();
            linieIstorica.Document = docIstoric;
            linieIstorica.TipMaterial = tipMateriale;
            linieIstorica.Directie = DirectieDiferenta.Plus;
            linieIstorica.Cantitate = 2m;
            linieIstorica.PretEvaluare = 6m;
            linieIstorica.CodEconomicId = codEc.ID;
            os.CommitChanges();
            var lotIstoric = linieIstorica.CreeazaLot(os, produs, mag1);
            lotIstoric.PretUnitar = 6m;                 // FINALIZAT: a trecut prin motor
            lotIstoric.Data = new DateOnly(2026, 2, 1);
            linieIstorica.ProdusId = null;              // …dar coloana nouă e goală
            os.CommitChanges();
            var idIstoric = docIstoric.ID;
            var idLotIstoric = lotIstoric.ID;
            ListaDiferenteInventarApply.Aplica(os, idIstoric, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Id = linieIstorica.ID, Directie = "Plus", TipMaterialId = tipMateriale.ID,
                    ProdusId = null, LotId = idLotIstoric, Cantitate = 2m, PretEvaluare = 6m,
                    CodEconomicId = codEc.ID } }
            });
            var dupaIstoric = ListaDiferenteInventarApply.Citeste(os, idIstoric).Linii.Single();
            s.Check("Linia „istorică” (lot FINALIZAT, ProdusId null) → SELF-HEALING, nu ștergere: lotul supraviețuiește cu prețul și data lui, produsul se backfill-ează de pe lot",
                os.GetObjectByKey<Lot>(idLotIstoric) is { PretUnitar: 6m } lotViu
                && lotViu.Data == new DateOnly(2026, 2, 1)
                && dupaIstoric.ProdusId == produs.ID && dupaIstoric.LotId == idLotIstoric);
            ListaDiferenteInventarApply.Sterge(os, idIstoric);

            // --- Refuzurile de OPERARE, fără rânduri-fantomă și fără serie consumată ---
            var serieInainteLdi = SerieLdi();
            void RefuzLdi(string nume, Guid predatorId, Guid primitorId, LdiLinieWriteDto linieProba) {
                var id = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                    Data = dataLdi, PredatorId = predatorId, PrimitorId = primitorId, Linii = { linieProba }
                });
                s.CheckRefuza(nume, () => ComenziDocument.Sistem(os).Opereaza(id));
                s.Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                    CubScena.FaraStoc(os, id)
                    && CubScena.FaraNote(os, id)
                    && os.GetObjectByKey<ListaDiferenteInventar>(id).Stare == StareDocument.Draft
                    && os.GetObjectByKey<ListaDiferenteInventar>(id).Numar == null);
                ListaDiferenteInventarApply.Sterge(os, id);
            }

            RefuzLdi("Primitor fără calitatea Comisie → refuz („primitorul trebuie să fie comisia de inventariere”)",
                mag1.ID, mag1.ID,
                new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    LotId = lotVechi.ID, Cantitate = 1m });
            RefuzLdi("Predator care nu e gestiune → refuz (predatorul e gestiunea inventariată)",
                comisie.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    LotId = lotVechi.ID, Cantitate = 1m });
            RefuzLdi("Plus FĂRĂ produs → refuz cu mesajul care spune CE SĂ FACĂ („alegeți produsul”) — lotul nu s-a putut naște",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID,
                    Cantitate = 1m, PretEvaluare = 5m, CodEconomicId = codEc.ID });
            RefuzLdi("Plus cu preț de evaluare 0 → refuz (28e: altfel lotul intră în stoc cu valoare zero și FIFO o propagă în toate ieșirile)",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 1m, CodEconomicId = codEc.ID });
            RefuzLdi("Plus fără cod economic (venitul 791 cere defalcarea E) → refuz al gardianului de dimensiuni obligatorii",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 1m, PretEvaluare = 5m });
            RefuzLdi("Minus FĂRĂ lot → refuz („linia de minus descarcă un lot existent”)",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID, Cantitate = 1m });
            RefuzLdi("Cantitate 0 pe linia de diferență → refuz",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    LotId = lotVechi.ID, Cantitate = 0m });
            RefuzLdi("Minus peste disponibil → refuz al gardianului de sold",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    LotId = lotVechi.ID, Cantitate = 999m });

            // Review advers F6-F2: coerența Tip↔Produs pe plusul care naște lot —
            // fără ea, lotul se năștea cu produs de marfă pe cont de materiale, iar
            // ieșirile legitime (care VALIDEAZĂ coerența) găseau stocul pe cheia greșită.
            var tipAltLdi = os.GetObjectsQuery<TipMaterial>().First(t => t.ID != tipMateriale.ID);
            var produsAltTip = os.CreateObject<Produs>();
            produsAltTip.Cod = MarcajApiLdi + "-B";
            produsAltTip.Denumire = "Produs B (alt Tip) probă felia Api LDI";
            produsAltTip.UM = "BUC";
            produsAltTip.TipMaterial = tipAltLdi;
            os.CommitChanges();
            RefuzLdi("Plus cu produs din ALT Tip decât Tipul liniei → refuz de coerență (review F6-F2: invariantul 50a se păzește la naștere, ca pe FCT/NIR/ASM)",
                mag1.ID, comisie.ID,
                new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produsAltTip.ID,
                    Cantitate = 1m, PretEvaluare = 5m, CodEconomicId = codEc.ID });

            // Review advers F6-F1 (oglinda gardului ASM, 46d): minusul care descarcă
            // lotul născut de o linie-FRATE ar intra cu preț nefinalizat (0) — gardianul
            // de sold ar trece (+3−2 ≥ 0 pe aceeași cheie, aceeași zi), iar consumul
            // restului la prețul finalizat ar lăsa valoare orfană pe cantitate 0.
            var idFrate = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 3m, PretEvaluare = 7m, CodEconomicId = codEc.ID } }
            });
            var citFrate = ListaDiferenteInventarApply.Citeste(os, idFrate).Linii.Single();
            ListaDiferenteInventarApply.Aplica(os, idFrate, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = {
                    new LdiLinieWriteDto {
                        Id = citFrate.Id, Directie = "Plus", TipMaterialId = tipMateriale.ID,
                        ProdusId = produs.ID, Cantitate = 3m, PretEvaluare = 7m, CodEconomicId = codEc.ID },
                    new LdiLinieWriteDto {
                        Directie = "Minus", TipMaterialId = tipMateriale.ID,
                        LotId = citFrate.LotId, Cantitate = 2m }
                }
            });
            s.CheckRefuza("Minus care descarcă lotul născut de linia-FRATE a aceluiași document → refuz (review F6-F1: prețul plusului nu există până la operare)",
                () => ComenziDocument.Sistem(os).Opereaza(idFrate));
            s.Check("Refuzul F6-F1 — fără rânduri-fantomă (33d)",
                CubScena.FaraStoc(os, idFrate)
                && os.GetObjectByKey<ListaDiferenteInventar>(idFrate).Stare == StareDocument.Draft);
            ListaDiferenteInventarApply.Sterge(os, idFrate);

            s.Check("Seria „LDI-” NU se consumă la refuz (F6-D4 + GATE D6: numărul se asignează abia la materializare)",
                SerieLdi() == serieInainteLdi);

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul LDI valid → listă goală", DryRunLdi(idLdi).Count == 0);
            s.Check("Dry-run-ul NU materializează nimic: Draft, fără număr, fără registre, lotul plusului tot nefinalizat",
                os.GetObjectByKey<ListaDiferenteInventar>(idLdi).Stare == StareDocument.Draft
                && os.GetObjectByKey<ListaDiferenteInventar>(idLdi).Numar == null
                && CubScena.FaraStoc(os, idLdi)
                && os.GetObjectByKey<Lot>(lotPlus.ID).PretUnitar == 0m);

            var rezLdi = ComenziDocument.Sistem(os).Opereaza(idLdi);
            citit = ListaDiferenteInventarApply.Citeste(os, idLdi);
            s.Check("ComenziDocument.Opereaza pe LDI → Operat, cu număr din politica proprie (seria LDI-), fără conex; affordances inversate",
                rezLdi.StareNoua == StareDocument.Operat && rezLdi.ConexId == null
                && citit.Numar?.StartsWith("LDI-") == true && citit.DataOperare != null
                && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna
                && SerieLdi() == serieInainteLdi + 1);
            s.Check("Operarea SEMNEAZĂ cantitatea (28a) — ReadDto o arată ca atare pe documentul (oricum) read-only: minus −2, plus +3",
                citit.Linii.Single(l => l.Directie == "Minus").Cantitate == -2m
                && citit.Linii.Single(l => l.Directie == "Plus").Cantitate == 3m
                && citit.Linii.Single(l => l.Directie == "Minus").Valoare == -20m
                && citit.Linii.Single(l => l.Directie == "Plus").Valoare == 21m);
            var lotPlusFinal = os.GetObjectByKey<Lot>(lotPlus.ID);
            s.Check("Motorul FINALIZEAZĂ lotul plusului: PretUnitar = PretEvaluare (7), data documentului, atributele culese pe linie",
                lotPlusFinal.PretUnitar == 7m && lotPlusFinal.Data == dataLdi
                && lotPlusFinal.LotFabricatie == "LOT-PLUS"
                && lotPlusFinal.DataExpirare == new DateOnly(2027, 9, 30));
            decimal SoldLdiCub(Lot l) => CubScena.Sold(os, l.ID, mag1.ID).Cantitate;
            var stocLdiApiCub = CubScena.Stoc(os, idLdi);
            s.Check("LDI operat → 2 rânduri, ambele Magazie pe gestiunea INVENTARIATĂ: −2/−20 pe lotul vechi, +3/+21 pe lotul nou",
                stocLdiApiCub.Count == 2 && stocLdiApiCub.All(p => p.Cont == tipMateriale.ContImplicitId && p.Gestiune == mag1.ID)
                && stocLdiApiCub.Any(p => p.Unitate == lotVechi.ID && p.Cantitate == -2m && p.Semnata == -20m)
                && stocLdiApiCub.Any(p => p.Unitate == lotPlus.ID && p.Cantitate == 3m && p.Semnata == 21m));
            s.Check("Solduri după operare: lot vechi 8, lot nou 3",
                SoldLdiCub(lotVechi) == 8m && SoldLdiCub(lotPlusFinal) == 3m);
            var noteLdiApiCub = CubScena.Note(os, idLdi);
            s.Check("Contare pe direcție (SemnFiltru): două note — minusul POZITIV (normalizat), plusul pe venitul de inventar",
                noteLdiApiCub.Any(p => p.Debit && p.Valoare == 20m) && noteLdiApiCub.Any(p => p.Credit && p.Valoare == 20m)
                && noteLdiApiCub.Any(p => p.Debit && p.Valoare == 21m) && noteLdiApiCub.Any(p => p.Credit && p.Valoare == 21m));
            s.CheckRefuza("Apply peste LDI Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi));
            s.CheckRefuza("Sterge peste LDI Operat → același refuz de domeniu",
                () => ListaDiferenteInventarApply.Sterge(os, idLdi));

            // --- Anulare directă (lotul plusului neatins de alții) și storno ---
            s.Check("Anulare prin API → Draft + solduri revenite (vechi 10, nou 0)",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idLdi).StareNoua == StareDocument.Draft
                && SoldLdiCub(lotVechi) == 10m && SoldLdiCub(lotPlusFinal) == 0m
                && CubScena.FaraStoc(os, idLdi));
            ComenziDocument.Sistem(os).Opereaza(idLdi);
            s.Check("Re-operare după anulare: semnul rămâne IDEMPOTENT (Math.Abs înainte de semnare, pe ambele căi)",
                ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Single(l => l.Directie == "Minus").Cantitate == -2m
                && ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Single(l => l.Directie == "Plus").Valoare == 21m);
            s.Check("Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete revenite",
                ComenziDocument.Sistem(os).Storneaza(idLdi, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
                && CubScena.Stoc(os, idLdi).Count == 4
                && CubScena.Stoc(os, idLdi).Count(p => p.Storno) == 2
                && SoldLdiCub(lotVechi) == 10m && SoldLdiCub(lotPlusFinal) == 0m);

            // --- Riscul 1 din contract: comutarea cu lotul propriu FINALIZAT ---
            // (operare → anulare → comutare pe Minus prin PUT): pinul ține, produsul se
            // golește pe toate căile (F6-M1), iar lotul finalizat SUPRAVIEȚUIEȘTE —
            // reziduu istoric asumat (F6-M3, documentat în contract §Închidere); la
            // ștergerea documentului, curățenia „fără urme” îl culege totuși.
            var idFinalizat = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 2m, PretEvaluare = 9m, CodEconomicId = codEc.ID } }
            });
            var linieFinalizata = ListaDiferenteInventarApply.Citeste(os, idFinalizat).Linii.Single();
            var idLotFinalizat = linieFinalizata.LotId.Value;
            ComenziDocument.Sistem(os).Opereaza(idFinalizat);
            ComenziDocument.Sistem(os).AnuleazaOperarea(idFinalizat);
            ListaDiferenteInventarApply.Aplica(os, idFinalizat, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Id = linieFinalizata.Id, Directie = "Minus", TipMaterialId = tipMateriale.ID,
                    LotId = lotVechi.ID, Cantitate = 1m } }
            });
            var dupaFinalizat = ListaDiferenteInventarApply.Citeste(os, idFinalizat).Linii.Single();
            s.Check("Comutare cu lotul propriu FINALIZAT (operare→anulare→comutare): pinul ține, produsul golit, lotul finalizat SUPRAVIEȚUIEȘTE cu prețul lui (riscul 1 din contract)",
                dupaFinalizat.Directie == "Minus" && dupaFinalizat.LotId == lotVechi.ID
                && dupaFinalizat.ProdusId == null
                && os.GetObjectByKey<Lot>(idLotFinalizat) is { PretUnitar: 9m });
            ListaDiferenteInventarApply.Sterge(os, idFinalizat);
            s.Check("Sterge după comutarea cu lot finalizat: curățenia „fără urme” culege și reziduul (anularea i-a șters registrele, nicio linie vie nu-l referă)",
                os.GetObjectByKey<Lot>(idLotFinalizat) == null);

            // --- Sterge: draftul de plus și lotul lui în culegere mor împreună ---
            var idLdi2 = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
                Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
                Linii = { new LdiLinieWriteDto {
                    Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 1m, PretEvaluare = 3m, CodEconomicId = codEc.ID } }
            });
            var idLotDraftLdi = ListaDiferenteInventarApply.Citeste(os, idLdi2).Linii.Single().LotId.Value;
            ListaDiferenteInventarApply.Sterge(os, idLdi2);
            s.Check("Sterge pe draftul LDI: documentul, linia și LOTUL în culegere dispar împreună",
                ListaDiferenteInventarApply.Citeste(os, idLdi2) == null
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idLdi2)
                && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraftLdi));

            CurataApiLdi(os);
            s.Check("Curățenie finală felia Api LDI (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiLdi))
                && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiLdi))
                && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajApiLdi))
                && os.GetObjectByKey<ListaDiferenteInventar>(idLdi) == null);
        }
    }
}
