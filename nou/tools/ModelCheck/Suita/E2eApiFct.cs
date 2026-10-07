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

// ============ Scenariul e2e pasul 5 / felia 2: Api FCT + NIR (F2-D6) ============
// Același lanț ca blocul 3c de mai sus (FCT → NIR conex → registre), dar parcurs
// prin CONTRACTUL feliei: WriteDto → `FacturaIntrareApply.Aplica` → `Citeste` /
// `Lista` → dry-run → comenzile `ComenziDocument` → `NirApply`. Endpoint-urile din host
// sunt transport peste EXACT acest cod, deci ce e verde aici e verde și pe sârmă.
//
// Ce exersează în plus față de blocul 3c (și de felia BTR):
//   * LOTUL SE NAȘTE LA `Aplica`, din `ProdusId` — blocurile vechi îl creau cu
//     `CreeazaLot` MANUAL, pentru că pe calea lor nu există nici ViewController,
//     nici seam de culegere. Aici se probează chiar seam-ul (F2-D1);
//   * TVA-ul materializat LA CULEGERE (`Valoare`/`ValoareTva` înainte de operare)
//     + default-ul `TipTvaImplicit` + override-ul manual;
//   * dimensiunile frunzei culese din DTO și clonate pe NIR prin contract;
//   * numărul CULES (FCT n-are politică de numerotare — invers față de BTR/NIR);
//   * `Citeste.Copii` = link-ul UI spre NIR-ul generat.
static class E2eApiFct {
    public static void Ruleaza(Suita s) {
        const string MarcajApiFct = "E2E-API-FCT";

        void CurataApiFct(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFct)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
            foreach (var fct in os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Numar.StartsWith("E2E-AF")).ToList()) {
                foreach (var copil in os.GetObjectsQuery<Document>().Where(x => x.DocumentSursaId == fct.ID).ToList()) {
                    pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == copil.ID).ToList());
                    pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == copil.ID).ToList());
                    pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
                    pj.Adauga(copil);
                }
                pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == fct.ID).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == fct.ID).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == fct.ID).ToList());
                pj.Adauga(fct);
            }
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFct)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiFct)).ToList());
            pj.Adauga(os.GetObjectsQuery<Partener>().Where(p => p.Cod == "E2E-AFURN").ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-AFCE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiFct(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            // Profilul BUGETAR are DOAR regimuri Capitalizat (CAP21/CAP19/CAP11/CAP0) —
            // nu se inventează tipuri noi în seed pentru probe (decizia 21). Consecința e
            // notată la proba de override, singura care ar cere un regim cu TVA separat.
            var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
            var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = "E2E-AFURN";
            furnizor.Denumire = "Furnizor probă felia Api FCT";
            furnizor.CodFiscal = "RO12345678";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = "E2E-AFCE";
            codEc.Denumire = "Cod economic probă felia Api FCT";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajApiFct + "-A";
            produs.Denumire = "Produs A probă felia Api FCT";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            var produsB = os.CreateObject<Produs>();
            produsB.Cod = MarcajApiFct + "-B";
            produsB.Denumire = "Produs B probă felia Api FCT";
            produsB.UM = "BUC";
            produsB.TipMaterial = tipMateriale;
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunFct(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // --- Apply: creare din WriteDto (fără LotId/Valoare — server-owned) ---
            var write = new FacturaIntrareWriteDto {
                Numar = "E2E-AF1",
                Data = new DateOnly(2026, 3, 7),
                PredatorId = furnizor.ID,
                PrimitorId = mag1.ID,
                DataScadenta = new DateOnly(2026, 4, 6),
                NumarPV = "PV-AF1",
                DataPV = new DateOnly(2026, 3, 6),
                CodCpv = "03000000-1",
                Valuta = "RON",
                Curs = 1m,
                Linii = {
                    new FacturaIntrareLinieWriteDto {
                        TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                        Cantitate = 5m, PretUnitar = 10m, TipTvaId = cap19.ID,
                        LotFabricatie = "LOT-API", DataExpirare = new DateOnly(2027, 1, 1),
                        CodCpv = "03000000-1", CodEconomicId = codEc.ID
                    },
                    // Fără TipTva în payload ⇒ default-ul tipului de document (CAP21).
                    new FacturaIntrareLinieWriteDto {
                        TipMaterialId = tipServicii.ID,
                        Cantitate = 1m, PretUnitar = 100m, CodEconomicId = codEc.ID
                    }
                }
            };
            var idFct = FacturaIntrareApply.Aplica(os, null, write);
            var citit = FacturaIntrareApply.Citeste(os, idFct);
            s.Check("Apply FCT creare → header plat: NUMĂRUL CULES (FCT n-are politică), scadență/PV/valută + CodFiscal-ul furnizorului",
                citit != null && citit.Id == idFct && citit.Stare == "Draft"
                && citit.Numar == "E2E-AF1" && citit.Data == new DateOnly(2026, 3, 7)
                && citit.DataScadenta == new DateOnly(2026, 4, 6) && citit.NumarPV == "PV-AF1"
                && citit.DataPV == new DateOnly(2026, 3, 6) && citit.CodCpv == "03000000-1"
                && citit.Valuta == "RON" && citit.Curs == 1m
                && citit.PredatorId == furnizor.ID && citit.PredatorDenumire == furnizor.Denumire
                && citit.PredatorCodFiscal == "RO12345678"
                && citit.PrimitorId == mag1.ID && citit.PrimitorDenumire == mag1.Denumire
                && !citit.Autogenerat && citit.DocumentSursaId == null && citit.Copii.Count == 0
                && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

            var linieStoc = citit.Linii.Single(l => l.TipMaterialId == tipMateriale.ID);
            var linieServ = citit.Linii.Single(l => l.TipMaterialId == tipServicii.ID);
            var lotNascut = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc.Id);
            s.Check("PROBA FELIEI: lotul S-A NĂSCUT la Aplica din ProdusId (nu prin CreeazaLot manual) — nefinalizat, în gestiunea primitoare",
                lotNascut != null && lotNascut.ProdusId == produs.ID && lotNascut.GestiuneId == mag1.ID
                && lotNascut.Data == default && lotNascut.PretUnitar == 0m
                && linieStoc.LotId == lotNascut.ID
                && linieStoc.LotEticheta == lotNascut.Eticheta
                && linieStoc.LotEticheta.Contains("(în culegere)"));
            s.Check("Linia de serviciu NU naște lot (natura Clasei ≠ Stoc)",
                linieServ.LotId == null && linieServ.LotEticheta == null && linieServ.ProdusId == null);

            s.Check("TVA materializat LA CULEGERE (GATE 53c): stoc CAP19 → 59,5 brut; serviciu → 121; Total brut 180,5",
                linieStoc.Valoare == 59.5m && linieStoc.ValoareTva == 0m
                && linieServ.Valoare == 121m && linieServ.ValoareTva == 0m
                && citit.Total == 180.5m);
            s.Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (CAP21 pe FCT, seed bugetar); linia cu TipTva cules rămâne CAP19",
                linieServ.TipTvaId == cap21.ID && linieServ.TipTvaCod == "CAP21" && linieServ.TipTvaCota == 21m
                && linieStoc.TipTvaId == cap19.ID && linieStoc.TipTvaCod == "CAP19");
            s.Check("Linia poartă dimensiunile frunzei (Id + Cod) și atributele de lot, proiectate plat",
                linieStoc.CodEconomicId == codEc.ID && linieStoc.CodEconomicCod == "E2E-AFCE"
                && linieStoc.SursaFinantareId == null && linieStoc.SursaFinantareCod == null
                && linieStoc.CodFunctionalId == null && linieStoc.ProiectId == null
                && linieStoc.LotFabricatie == "LOT-API" && linieStoc.DataExpirare == new DateOnly(2027, 1, 1)
                && linieStoc.CodCpv == "03000000-1"
                && linieStoc.ProdusId == produs.ID && linieStoc.ProdusCod == produs.Cod
                && linieStoc.ProdusDenumire == produs.Denumire
                && linieStoc.Cantitate == 5m && linieStoc.PretUnitar == 10m);

            // --- Override-ul manual de ValoareTva: refuzat pe regimuri fără TVA separat ---
            // Review advers F2-D1/D7: la bugetar toate regimurile sunt Capitalizat
            // (TVA-ul stă în preț) — un override acceptat aici ar fi numărat TVA-ul de
            // două ori în Total și ar fi murit tăcut la operare. Semantica POZITIVĂ a
            // override-ului (păstrare + recalcul condiționat de declanșatori) se probează
            // în blocul privat, pe regimul Normal.
            write.Linii[0].Id = linieStoc.Id;
            write.Linii[0].TipTvaId = linieStoc.TipTvaId;
            write.Linii[1].Id = linieServ.Id;
            // ROUND-TRIP: clientul retrimite agregatul ÎNTREG, inclusiv TipTva-ul primit
            // la citire (pus de default). Pe o linie EXISTENTĂ absența lui nu e „n-am
            // apucat să-l trimit", ci golire deliberată — probată imediat mai jos.
            write.Linii[1].TipTvaId = linieServ.TipTvaId;
            write.Linii[1].ValoareTva = 3.33m;
            s.CheckRefuza("Override ValoareTva pe regim Capitalizat → refuz (F2-D1: regimul nu poartă TVA separat)",
                () => FacturaIntrareApply.Aplica(os, idFct, write));
            write.Linii[1].ValoareTva = -5m;
            s.CheckRefuza("Override ValoareTva NEGATIV → refuz (F2-D7)",
                () => FacturaIntrareApply.Aplica(os, idFct, write));
            write.Linii[1].ValoareTva = null;
            FacturaIntrareApply.Aplica(os, idFct, write);

            write.Linii[1].TipTvaId = null;
            FacturaIntrareApply.Aplica(os, idFct, write);
            s.Check("Pe linia EXISTENTĂ, TipTva absent din payload = GOLIRE deliberată (default-ul NU se re-aplică) → valoarea revine la net 100",
                FacturaIntrareApply.Citeste(os, idFct).Linii.Single(l => l.Id == linieServ.Id)
                    is { TipTvaId: null, Valoare: 100m, ValoareTva: 0m });
            write.Linii[1].TipTvaId = cap21.ID;
            FacturaIntrareApply.Aplica(os, idFct, write);

            // --- Reconcilierea colecției × ciclul de viață al lotului în culegere ---
            var idLotNascut = lotNascut.ID;
            write.Linii[0].ProdusId = produsB.ID;
            FacturaIntrareApply.Aplica(os, idFct, write);
            var lotSincronizat = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc.Id);
            s.Check("Reconciliere: produs reales pe linie → ACELAȘI lot, sincronizat (nu un al doilea lot pentru aceeași linie)",
                lotSincronizat != null && lotSincronizat.ID == idLotNascut && lotSincronizat.ProdusId == produsB.ID
                && os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == linieStoc.Id) == 1);

            write.Linii.RemoveAt(0);
            FacturaIntrareApply.Aplica(os, idFct, write);
            s.Check("Reconciliere: linia de stoc absentă din payload se ȘTERGE, iar lotul ei în culegere moare odată cu ea",
                !os.GetObjectsQuery<FacturaIntrareDetaliu>().Any(l => l.ID == linieStoc.Id)
                && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotNascut)
                && FacturaIntrareApply.Citeste(os, idFct).Linii.Count == 1);

            write.Linii.Insert(0, new FacturaIntrareLinieWriteDto {
                TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                Cantitate = 5m, PretUnitar = 10m, TipTvaId = cap19.ID,
                LotFabricatie = "LOT-API", DataExpirare = new DateOnly(2027, 1, 1),
                CodCpv = "03000000-1", CodEconomicId = codEc.ID
            });
            FacturaIntrareApply.Aplica(os, idFct, write);
            citit = FacturaIntrareApply.Citeste(os, idFct);
            var linieStoc2 = citit.Linii.Single(l => l.TipMaterialId == tipMateriale.ID);
            var lotNou = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc2.Id);
            s.Check("Linia de stoc re-adăugată (fără Id) e linie NOUĂ și își naște propriul lot",
                lotNou != null && lotNou.ID != idLotNascut && lotNou.ProdusId == produs.ID
                && linieStoc2.Id != linieStoc.Id && linieStoc2.LotId == lotNou.ID);
            write.Linii[0].Id = linieStoc2.Id;

            // --- Refuzuri de contract (mesaj de domeniu, nu excepție de infrastructură) ---
            s.CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                FacturaIntrareApply.Aplica(os, idFct, new FacturaIntrareWriteDto {
                    Numar = "E2E-AF1", Data = write.Data, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { new FacturaIntrareLinieWriteDto {
                        Id = Guid.NewGuid(), TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 1m } }
                }));
            s.CheckRefuza("Apply cu preț unitar în afara scării numeric(18,6) → refuz de domeniu al gardianului (104c), nu DbUpdateException", () => {
                using var osGard = s.Provider.CreateObjectSpace();
                new GardianEditare().OnObjectSpaceCreated(osGard);
                FacturaIntrareApply.Aplica(osGard, idFct, new FacturaIntrareWriteDto {
                    Numar = "E2E-AF1", Data = write.Data, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    Linii = { new FacturaIntrareLinieWriteDto {
                        TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
                });
            });
            s.CheckRefuza("Apply cu furnizor inexistent → refuz cu mesaj de domeniu (nu violare de FK)", () =>
                FacturaIntrareApply.Aplica(os, idFct, new FacturaIntrareWriteDto {
                    Numar = "E2E-AF1", Data = write.Data, PredatorId = Guid.NewGuid(), PrimitorId = mag1.ID
                }));
            FacturaIntrareApply.Aplica(os, idFct, write);
            s.Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact 2 linii",
                FacturaIntrareApply.Citeste(os, idFct).Linii.Count == 2);

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul FCT valid → listă goală", DryRunFct(idFct).Count == 0);

            var rezOperare = ComenziDocument.Sistem(os).Opereaza(idFct);
            s.Check("ComenziDocument.Opereaza pe FCT → Operat + ConexId (NIR-ul generat în aceeași tranzacție), cu mesaj pentru operator",
                rezOperare.StareNoua == StareDocument.Operat && rezOperare.ConexId != null
                && rezOperare.Mesaje.Count == 1);
            var idNir = rezOperare.ConexId.Value;

            var lotFinal = os.GetObjectByKey<Lot>(lotNou.ID);
            s.Check("Motorul FINALIZEAZĂ lotul născut la culegere: preț 11,9 (59,5/5), data facturii, atributele culese pe linie",
                lotFinal.PretUnitar == 11.9m && lotFinal.Data == new DateOnly(2026, 3, 7)
                && lotFinal.LotFabricatie == "LOT-API" && lotFinal.DataExpirare == new DateOnly(2027, 1, 1));

            citit = FacturaIntrareApply.Citeste(os, idFct);
            s.Check("Citeste după operare: numărul rămâne AL FURNIZORULUI (nicio serie consumată), affordances inversate",
                citit.Stare == "Operat" && citit.Numar == "E2E-AF1" && citit.DataOperare != null
                && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna);
            s.Check("Citeste.Copii → NIR-ul conex: codul ancorei TipDocument, starea ca text, marcajul Autogenerat",
                citit.Copii.Count == 1 && citit.Copii[0].Id == idNir && citit.Copii[0].Tip == "NIR"
                && citit.Copii[0].Stare == "Draft" && citit.Copii[0].Autogenerat
                && citit.Copii[0].Numar == null);

            var noteFctCub = CubScena.Note(os, idFct);
            var serviciuFctCub = noteFctCub.Where(p => p.Debit && p.Cont == tipServicii.ContImplicitId).ToList();
            s.Check("FCT postează DOAR linia de serviciu: 628 = 401, 121 (brut capitalizat)",
                serviciuFctCub.Sum(p => p.Valoare) == 121m
                && serviciuFctCub.Select(p => p.LinieId).Distinct().Count() == 1
                && noteFctCub.Where(p => p.Credit && p.LinieId == serviciuFctCub[0].LinieId).All(p => p.Cont == cont401.ID)
                && noteFctCub.Where(p => p.Credit && p.LinieId == serviciuFctCub[0].LinieId).Sum(p => p.Valoare) == 121m);

            // --- NIR: citirea conexului, apoi comanda pe el ---
            var nirDto = NirApply.Citeste(os, idNir);
            s.Check("NirApply.Citeste pe conex: header cu sursa ETICHETATĂ, fără număr (seria NIR se consumă la propria operare), editabil ca DRAFT (F5-D8b)",
                nirDto != null && nirDto.Stare == "Draft" && nirDto.Numar == null && nirDto.Autogenerat
                && nirDto.DocumentSursaId == idFct && nirDto.DocumentSursaNumar == "E2E-AF1"
                && nirDto.DocumentSursaTip == "FCT"
                && nirDto.PredatorDenumire == furnizor.Denumire && nirDto.PrimitorDenumire == mag1.Denumire
                && nirDto.Total == 59.5m
                // PoateEdita urmează STAREA (F5-D8b): felia 5 a adăugat calea de scriere
                // pe NIR, deci affordance-ul spune ce poate serverul (42e). Inclusiv pe
                // draftul AUTOGENERAT — conexul e proiectat să fie deschis în editare
                // (26d) pentru recepția parțială, iar gardul de lot străin (F5-D3) e ce
                // face editarea lui sigură. Până la felia 5 era fals prin construcție,
                // fiindcă tierul n-avea nicio cale de scriere (F2-D5).
                && nirDto.PoateEdita && nirDto.PoateOpera && !nirDto.PoateAnula && !nirDto.PoateStorna);
            s.Check("NIR-ul preia DOAR linia de stoc: lotul finalizat (eticheta nu mai spune „în culegere”), valoarea și dimensiunea clonată prin contract",
                nirDto.Linii.Count == 1 && nirDto.Linii[0].LotId == lotFinal.ID
                && nirDto.Linii[0].LotEticheta == lotFinal.Eticheta
                && !nirDto.Linii[0].LotEticheta.Contains("culegere")
                && nirDto.Linii[0].Cantitate == 5m && nirDto.Linii[0].Valoare == 59.5m
                && nirDto.Linii[0].TipTvaCod == "CAP19"
                && nirDto.Linii[0].CodEconomicId == codEc.ID && nirDto.Linii[0].CodEconomicCod == "E2E-AFCE");
            var listaNir = NirApply.Lista(os).Where(x => x.Id == idNir).ToList();
            s.Check("NirApply.Lista → un rând, cu Stare ca text (CASE în SQL), marcajul Autogenerat și Total din agregat",
                listaNir.Count == 1 && listaNir[0].Stare == "Draft" && listaNir[0].Autogenerat
                && listaNir[0].Total == 59.5m && listaNir[0].PrimitorDenumire == mag1.Denumire);

            var rezNir = ComenziDocument.Sistem(os).Opereaza(idNir);
            s.Check("ComenziDocument.Opereaza pe NIR → Operat, cu număr din politica proprie (seria NIR-), fără alt conex",
                rezNir.StareNoua == StareDocument.Operat && rezNir.ConexId == null
                && NirApply.Citeste(os, idNir).Numar?.StartsWith("NIR-") == true);
            var stocFctCub = CubScena.Stoc(os, idFct);
            s.Check("NIR → +5/+59,5 Magazie pe gestiunea primitoare, pe lotul născut la culegerea facturii",
                stocFctCub.Count == 1 && stocFctCub[0].Cont == tipMateriale.ContImplicitId && stocFctCub[0].Gestiune == mag1.ID
                && stocFctCub[0].Cantitate == 5m && stocFctCub[0].Semnata == 59.5m && stocFctCub[0].Unitate == lotFinal.ID);
            s.Check("NIR contează recepția: 302.01.00 = 401, 59,5",
                CubScena.Note(os, idFct).Nota(tipMateriale.ContImplicitId, cont401.ID, 59.5m));

            // --- NUC-FCT-API (B-D6, pas 5): declarantul pe documentul cules și operat prin ușa API ---
            ProbeNucleu.Proba(os, s.Check, "NUC-FCT-API", [os.GetObjectByKey<FacturaIntrare>(idFct)]);

            // --- Gardienii, prin contract ---
            s.CheckRefuza("Apply peste FCT Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => FacturaIntrareApply.Aplica(os, idFct, write));
            s.CheckRefuza("Sterge peste FCT Operat → același refuz de domeniu",
                () => FacturaIntrareApply.Sterge(os, idFct));
            s.CheckRefuza("Anularea FCT cu NIR operat → refuzată (gardianul grupului conex)",
                () => ComenziDocument.Sistem(os).AnuleazaOperarea(idFct));

            ComenziDocument.Sistem(os).AnuleazaOperarea(idNir);
            s.Check("NIR anulat: Draft, registrele proprii șterse — dar RĂMÂNE autogenerat",
                NirApply.Citeste(os, idNir) is { Stare: "Draft", Autogenerat: true }
                && CubScena.Sold(os, lotFinal.ID, mag1.ID) is { Cantitate: 5m, Valoare: 59.5m });
            ComenziDocument.Sistem(os).AnuleazaOperarea(idFct);
            s.Check("Anularea FCT ȘTERGE NIR-ul redevenit draft autogenerat (artefact al operării) — Copii se golește",
                NirApply.Citeste(os, idNir) == null
                && FacturaIntrareApply.Citeste(os, idFct) is { Stare: "Draft", Copii.Count: 0 });

            // --- Lista FCT ---
            var listaFct = FacturaIntrareApply.Lista(os).Where(x => x.Id == idFct).ToList();
            s.Check("Lista FCT → un rând, cu Stare ca text, furnizor/gestiune, scadență și Total BRUT din agregat",
                listaFct.Count == 1 && listaFct[0].Stare == "Draft" && listaFct[0].Numar == "E2E-AF1"
                && listaFct[0].PredatorDenumire == furnizor.Denumire
                && listaFct[0].PrimitorDenumire == mag1.Denumire
                && listaFct[0].DataScadenta == new DateOnly(2026, 4, 6) && listaFct[0].Total == 180.5m);
            s.Check("Lista FCT → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take)",
                FacturaIntrareApply.Lista(os).Where(x => x.Stare == "Draft")
                    .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

            // --- Sterge: lotul în culegere moare, lotul finalizat supraviețuiește ---
            var idFct2 = FacturaIntrareApply.Aplica(os, null, new FacturaIntrareWriteDto {
                Numar = "E2E-AF2", Data = new DateOnly(2026, 3, 8),
                PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                Linii = { new FacturaIntrareLinieWriteDto {
                    TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                    Cantitate = 2m, PretUnitar = 7m, CodEconomicId = codEc.ID } }
            });
            var idLotDraft = FacturaIntrareApply.Citeste(os, idFct2).Linii[0].LotId.Value;
            FacturaIntrareApply.Sterge(os, idFct2);
            s.Check("Sterge pe draft: documentul, liniile și LOTUL în culegere dispar împreună",
                FacturaIntrareApply.Citeste(os, idFct2) == null
                && !os.GetObjectsQuery<FacturaIntrareDetaliu>().Any(l => l.DocumentId == idFct2)
                && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraft));

            // Review advers F2-D4: lotul FINALIZAT al draftului anulat moare la Sterge
            // DOAR fără nicio urmă (anularea i-a șters rândurile de registru, nicio
            // linie vie nu-l mai referă) — altfel ar fi rămas pe viață în nomenclator.
            // Protecția GATE D1 (loturile cu istorie reală) ține prin gardele de
            // registre/referințe, nu prin refuzul global de dinainte.
            var idLotFinal = lotFinal.ID;
            FacturaIntrareApply.Sterge(os, idFct);
            s.Check("Sterge pe draftul anulat: lotul FINALIZAT rămas FĂRĂ URME (zero registre, zero referințe vii) moare cu documentul (F2-D4)",
                FacturaIntrareApply.Citeste(os, idFct) == null
                && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotFinal));

            CurataApiFct(os);
            s.Check("Curățenie finală felia Api FCT (fără reziduuri e2e)",
                !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-AF"))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiFct))
                && !os.GetObjectsQuery<Partener>().Any(p => p.Cod == "E2E-AFURN"));
        }
    }
}
