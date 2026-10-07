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

// ========= Scenariul e2e pasul 5 / felia 5: Api NIR scriere (F5-D8) =========
// RECEPȚIA FĂRĂ FACTURĂ, parcursă prin contractul feliei: WriteDto →
// `NirApply.Aplica` → `Citeste` → dry-run → `ComenziDocument.Opereaza` → registre.
// Fluxul n-a existat nicăieri până la felia asta (nici în XAF, nici prin API):
// `NirDetaliu` n-avea `ProdusId`, iar `CreeazaLot` n-avea niciun apelant din UI
// — exact golul de model pe care GATE-ul l-a închis pe FCT (53a).
//
// Ce exersează în plus față de blocul „Api FCT + NIR (F2-D6)":
//   * lotul se naște pe linia PROPRIE a NIR-ului (nu pe a facturii), din
//     `ProdusId`, la `Aplica` — seam-ul `LoturiCulegereService` generalizat (F5-D3);
//   * `Valoare = PretUnitar × Cantitate` materializată LA CULEGERE, cu formula
//     GEAMĂNĂ celei din `NIR.PregatesteOperare` (F5-D6a);
//   * TESTUL-ANCORĂ AL FELIEI (riscul 1 din contract): PUT pe NIR-ul CONEX —
//     cu produsul completat și cantitatea redusă — NU naște al doilea lot;
//   * refuzurile F5-D7/D7b, fiecare fără rânduri-fantomă (33d).
// Rulează pe profilul BUGETAR: NIR n-are `PoliticaTva` în niciun profil (F5-D5),
// deci nimic din felie nu cere profilul privat.
static class E2eApiNir {
    public static void Ruleaza(Suita s) {
        const string MarcajApiNir = "E2E-API-NIR";

        void CurataApiNir(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            // Documentele probei se găsesc prin marfa lor: NIR-ul n-are număr cules
            // (seria e server-owned), deci ancora e produsul marcat — prin loturile lui
            // și prin `NirDetaliu.ProdusId`. Facturile probei se găsesc pe număr.
            var idsDoc = os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiNir))
                .Select(d => d.DocumentId).ToList();
            idsDoc.AddRange(os.GetObjectsQuery<NirDetaliu>()
                .Where(d => d.Produs.Cod.StartsWith(MarcajApiNir))
                .Select(d => d.DocumentId).ToList());
            idsDoc.AddRange(os.GetObjectsQuery<FacturaIntrare>()
                .Where(d => d.Numar.StartsWith("E2E-ANF")).Select(d => d.ID).ToList());
            idsDoc = idsDoc.Distinct().ToList();
            // …plus copiii conecși (NIR-ul generat la operarea facturii).
            idsDoc.AddRange(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsDoc.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList());
            idsDoc = idsDoc.Distinct().ToList();

            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiNir)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiNir)).ToList());
            pj.Adauga(os.GetObjectsQuery<Partener>().Where(p => p.Cod == "E2E-ANFURN").ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-ANCE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiNir(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = "E2E-ANFURN";
            furnizor.Denumire = "Furnizor probă felia Api NIR";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = "E2E-ANCE";
            codEc.Denumire = "Cod economic probă felia Api NIR";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajApiNir + "-A";
            produs.Denumire = "Produs A probă felia Api NIR";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            // Produs al ALTUI Tip — proba de coerență Tip-linie ↔ Produs (F5-D7).
            var produsStrain = os.CreateObject<Produs>();
            produsStrain.Cod = MarcajApiNir + "-S";
            produsStrain.Denumire = "Produs de alt Tip, probă felia Api NIR";
            produsStrain.UM = "BUC";
            produsStrain.TipMaterial = tipServicii;
            os.CommitChanges();

            var dataNir = new DateOnly(2026, 3, 12);

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunNir(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // --- Apply: recepția MANUALĂ, din WriteDto (fără Numar/LotId/Valoare) ---
            var write = new NirWriteDto {
                Data = dataNir,
                PredatorId = furnizor.ID,
                PrimitorId = mag1.ID,
                Linii = {
                    new NirLinieWriteDto {
                        TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                        Cantitate = 4m, PretUnitar = 12.5m,
                        LotFabricatie = "LOT-NIR", DataExpirare = new DateOnly(2027, 6, 30),
                        CodEconomicId = codEc.ID
                    }
                }
            };
            var idNir = NirApply.Aplica(os, null, write);
            var citit = NirApply.Citeste(os, idNir);
            s.Check("Apply NIR manual → header plat, FĂRĂ număr (seria „NIR-” e server-owned, se consumă la operare — invers față de FCT)",
                citit != null && citit.Id == idNir && citit.Stare == "Draft" && citit.Numar == null
                && citit.Data == dataNir
                && citit.PredatorId == furnizor.ID && citit.PredatorDenumire == furnizor.Denumire
                && citit.PrimitorId == mag1.ID && citit.PrimitorDenumire == mag1.Denumire
                && !citit.Autogenerat && citit.DocumentSursaId == null
                && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

            var linie = citit.Linii.Single();
            var lotNascut = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linie.Id);
            s.Check("PROBA FELIEI: lotul se naște pe linia PROPRIE a NIR-ului, din ProdusId — nefinalizat, în gestiunea PRIMITOARE (hook-ul GestiuneLoturiCulese)",
                lotNascut != null && lotNascut.ProdusId == produs.ID && lotNascut.GestiuneId == mag1.ID
                && lotNascut.Data == default && lotNascut.PretUnitar == 0m
                && linie.LotId == lotNascut.ID && !linie.LotStrain
                && linie.LotEticheta == lotNascut.Eticheta
                && linie.LotEticheta.Contains("(în culegere)"));
            s.Check("Valoarea materializată LA CULEGERE din prețul cules (F5-D6a): 4 × 12,5 = 50, Total 50 — nu 0 până la operare",
                linie.Valoare == 50m && linie.ValoareTva == 0m && citit.Total == 50m
                && linie.PretUnitar == 12.5m && linie.Cantitate == 4m);
            s.Check("Linia poartă produsul, atributele de lot și dimensiunea frunzei, proiectate plat",
                linie.ProdusId == produs.ID && linie.ProdusCod == produs.Cod
                && linie.ProdusDenumire == produs.Denumire
                && linie.LotFabricatie == "LOT-NIR" && linie.DataExpirare == new DateOnly(2027, 6, 30)
                && linie.CodEconomicId == codEc.ID && linie.CodEconomicCod == "E2E-ANCE"
                && linie.SursaFinantareId == null && linie.ProiectId == null
                && linie.TipTvaId == null);

            // --- Reconcilierea colecției (upsert pe Id) ---
            write.Linii[0].Id = linie.Id;
            write.Linii[0].Cantitate = 6m;
            NirApply.Aplica(os, idNir, write);
            citit = NirApply.Citeste(os, idNir);
            s.Check("Reconciliere pe Id: cantitatea schimbată → valoarea o urmează (6 × 12,5 = 75), ACELAȘI lot (nu un al doilea pentru aceeași linie)",
                citit.Linii.Single().Valoare == 75m && citit.Linii.Single().LotId == lotNascut.ID
                && os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == linie.Id) == 1);
            s.CheckRefuza("Apply NIR cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                NirApply.Aplica(os, idNir, new NirWriteDto {
                    Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { new NirLinieWriteDto {
                        Id = Guid.NewGuid(), TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 1m } }
                }));
            s.CheckRefuza("Apply NIR cu același Id de linie de două ori → refuz (a doua apariție ar suprascrie tăcut prima)", () =>
                NirApply.Aplica(os, idNir, new NirWriteDto {
                    Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { write.Linii[0], write.Linii[0] }
                }));
            s.CheckRefuza("Apply NIR cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
                NirApply.Aplica(s.OsCuGardian(), idNir, new NirWriteDto {
                    Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { new NirLinieWriteDto {
                        TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
                }));
            NirApply.Aplica(os, idNir, write);
            s.Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact o linie",
                NirApply.Citeste(os, idNir).Linii.Count == 1);

            // --- Refuzurile de OPERARE (F5-D7/D7b), fiecare fără rânduri-fantomă ---
            void RefuzNir(string nume, NirLinieWriteDto linieProba) {
                var id = NirApply.Aplica(os, null, new NirWriteDto {
                    Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { linieProba }
                });
                s.CheckRefuza(nume, () => ComenziDocument.Sistem(os).Opereaza(id));
                s.Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                    CubScena.FaraStoc(os, id)
                    && CubScena.FaraNote(os, id)
                    && os.GetObjectByKey<NIR>(id).Stare == StareDocument.Draft);
                NirApply.Sterge(os, id);
            }

            RefuzNir("Linie de stoc FĂRĂ produs → refuz cu mesajul care spune CE SĂ FACĂ („alegeți produsul”, F5-D7)",
                new NirLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 10m, CodEconomicId = codEc.ID
                });
            RefuzNir("Produs de ALT Tip decât Tipul liniei → refuz (oglinda 53f: lotul ar ajunge în registrul altui Tip decât cel postat)",
                new NirLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, ProdusId = produsStrain.ID,
                    Cantitate = 1m, PretUnitar = 10m, CodEconomicId = codEc.ID
                });
            RefuzNir("Preț unitar 0 pe linia care își NAȘTE lotul → refuz (F5-D7b: altfel lotul intră în stoc cu preț 0 și FIFO îl propagă în toate ieșirile)",
                new NirLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 1m, PretUnitar = 0m, CodEconomicId = codEc.ID
                });

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul NIR manual valid → listă goală",
                DryRunNir(idNir).Count == 0);
            s.Check("Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și fără lot finalizat",
                os.GetObjectByKey<NIR>(idNir).Stare == StareDocument.Draft
                && CubScena.FaraStoc(os, idNir)
                && CubScena.FaraNote(os, idNir)
                && os.GetObjectByKey<Lot>(lotNascut.ID).PretUnitar == 0m);

            var rezNir = ComenziDocument.Sistem(os).Opereaza(idNir);
            s.Check("ComenziDocument.Opereaza pe NIR manual → Operat, cu număr din politica proprie (seria NIR-), fără conex; affordances inversate (nu mai e editabil)",
                rezNir.StareNoua == StareDocument.Operat && rezNir.ConexId == null
                && NirApply.Citeste(os, idNir) is { Numar: not null, PoateEdita: false, PoateOpera: false,
                    PoateAnula: true, PoateStorna: true }
                && NirApply.Citeste(os, idNir).Numar.StartsWith("NIR-"));
            var lotFinalizat = os.GetObjectByKey<Lot>(lotNascut.ID);
            s.Check("Motorul FINALIZEAZĂ lotul născut pe linia NIR-ului: preț 12,5 (75/6), data documentului, atributele culese pe linie",
                lotFinalizat.PretUnitar == 12.5m && lotFinalizat.Data == dataNir
                && lotFinalizat.LotFabricatie == "LOT-NIR"
                && lotFinalizat.DataExpirare == new DateOnly(2027, 6, 30));
            var stocNirManualCub = CubScena.Stoc(os, idNir);
            s.Check("NIR manual → +6/+75 Magazie pe gestiunea primitoare, pe lotul propriu",
                stocNirManualCub.Count == 1 && stocNirManualCub[0].Cont == tipMateriale.ContImplicitId
                && stocNirManualCub[0].Gestiune == mag1.ID && stocNirManualCub[0].Unitate == lotFinalizat.ID
                && stocNirManualCub[0].Cantitate == 6m && stocNirManualCub[0].Semnata == 75m);
            s.Check("NIR manual contează recepția ca oricare alta: 302.01.00 = 401, 75 (regula de oprire a feliei — registrele nu disting proveniența)",
                CubScena.Note(os, idNir).Nota(tipMateriale.ContImplicitId, cont401.ID, 75m));
            s.CheckRefuza("Apply peste NIR Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => NirApply.Aplica(os, idNir, write));
            s.CheckRefuza("Sterge peste NIR Operat → același refuz de domeniu",
                () => NirApply.Sterge(os, idNir));

            // --- TESTUL-ANCORĂ: PUT pe NIR-ul CONEX nu naște al doilea lot (F5-D3) ---
            // Marfa e deja recepționată pe lotul născut la culegerea FACTURII; un al
            // doilea lot ar dubla stocul invizibil pentru gardianul de sold (lotul nou
            // pornește de la zero, deci nicio verificare nu devine negativă).
            var idFct = FacturaIntrareApply.Aplica(os, null, new FacturaIntrareWriteDto {
                Numar = "E2E-ANF1", Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                Linii = { new FacturaIntrareLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 10m, PretUnitar = 5m, TipTvaId = cap19.ID, CodEconomicId = codEc.ID } }
            });
            var idNirConex = ComenziDocument.Sistem(os).Opereaza(idFct).ConexId.Value;
            var conex = NirApply.Citeste(os, idNirConex);
            var lotFct = os.GetObjectByKey<Lot>(conex.Linii[0].LotId.Value);
            s.Check("NIR conex: DRAFT AUTOGENERAT deci EDITABIL (F5-D8b — recepția parțială e flux de producție), cu lot STRĂIN pe linie, fără produs și fără preț propriu",
                conex.PoateEdita && conex.Autogenerat
                && conex.Linii.Count == 1 && conex.Linii[0].LotStrain
                && conex.Linii[0].ProdusId == null && conex.Linii[0].PretUnitar == 0m
                && lotFct.LinieIntrareId != conex.Linii[0].Id && lotFct.PretUnitar == 5.95m);

            var loturiInainte = os.GetObjectsQuery<Lot>().Count(l => l.Produs.Cod.StartsWith(MarcajApiNir));
            // Cazul EXACT al riscului 1: PUT cu produsul COMPLETAT (clientul l-ar putea
            // trimite) și cantitatea redusă (recepție parțială — marfa primită e mai
            // puțină decât cea facturată).
            NirApply.Aplica(os, idNirConex, new NirWriteDto {
                Data = conex.Data, PredatorId = conex.PredatorId, PrimitorId = conex.PrimitorId,
                Linii = { new NirLinieWriteDto {
                    Id = conex.Linii[0].Id, TipMaterialId = conex.Linii[0].TipMaterialId,
                    ProdusId = produs.ID, Cantitate = 4m, PretUnitar = 99m,
                    CodEconomicId = conex.Linii[0].CodEconomicId } }
            });
            var conexDupa = NirApply.Citeste(os, idNirConex);
            s.Check("TESTUL-ANCORĂ (riscul 1): PUT pe NIR-ul conex cu produs completat → NICIUN al doilea lot, linia referă tot lotul facturii",
                os.GetObjectsQuery<Lot>().Count(l => l.Produs.Cod.StartsWith(MarcajApiNir)) == loturiInainte
                && conexDupa.Linii[0].LotId == lotFct.ID && conexDupa.Linii[0].LotStrain
                && !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId == conexDupa.Linii[0].Id));
            s.Check("Recepția PARȚIALĂ pe conex: valoarea se recalculează din prețul LOTULUI (4 × 5,95 = 23,8), prețul cules pe linie e IGNORAT (F5-D6b)",
                conexDupa.Linii[0].Cantitate == 4m && conexDupa.Linii[0].Valoare == 23.8m
                && conexDupa.Total == 23.8m);
            s.Check("PUT-ul pe conex nu atinge TipTva-ul informativ clonat din factură (F5-D5: NIR-ul nu culege TVA)",
                conexDupa.Linii[0].TipTvaId == cap19.ID && conexDupa.Linii[0].TipTvaCod == "CAP19");
            // Review advers F3: „inert" trebuie să însemne GOLIT, nu doar nefolosit —
            // altfel produsul rămâne persistat pe linia conexă și îl citește validarea de
            // coerență Tip↔Produs, care poate face NIR-ul permanent ne-operabil printr-un
            // câmp pe care UI-ul îl afișează read-only.
            s.Check("Produsul trimis pe o linie cu lot STRĂIN e GOLIT, nu doar ignorat (review advers F3)",
                conexDupa.Linii[0].ProdusId == null);
            s.CheckRefuza("Sterge pe NIR-ul CONEX (draft autogenerat) → refuz: e artefactul operării facturii, poartă singura postare a datoriei (review advers F2)",
                () => NirApply.Sterge(os, idNirConex));
            s.Check("Refuzul de mai sus nu a atins documentul: conexul e viu, cu linia lui",
                NirApply.Citeste(os, idNirConex) is { } viu && viu.Linii.Count == 1);

            ComenziDocument.Sistem(os).Opereaza(idNirConex);
            var stocConexCub = CubScena.Stoc(os, idFct, idNirConex);
            s.Check("NIR conex operat după PUT: +4/+23,8 pe lotul facturii — gardul de preț (F5-D7b) NU atinge liniile cu lot străin, acolo prețul e al lotului",
                stocConexCub.Count > 0 && stocConexCub.All(p => p.Unitate == lotFct.ID)
                && stocConexCub.Sum(p => p.Cantitate) == 4m && stocConexCub.Sum(p => p.Semnata) == 23.8m);

            // --- Sterge: draftul manual și lotul lui mor împreună ---
            var idNir2 = NirApply.Aplica(os, null, new NirWriteDto {
                Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                Linii = { new NirLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 2m, PretUnitar = 8m, CodEconomicId = codEc.ID } }
            });
            var idLotDraft = NirApply.Citeste(os, idNir2).Linii[0].LotId.Value;
            NirApply.Sterge(os, idNir2);
            s.Check("Sterge pe draftul NIR manual: documentul, linia și LOTUL în culegere dispar împreună",
                NirApply.Citeste(os, idNir2) == null
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idNir2)
                && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraft));

            CurataApiNir(os);
            s.Check("Curățenie finală felia Api NIR (fără reziduuri e2e)",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiNir))
                && !os.GetObjectsQuery<Partener>().Any(p => p.Cod == "E2E-ANFURN")
                && !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-ANF"))
                && os.GetObjectByKey<NIR>(idNir) == null);
        }
    }
}
