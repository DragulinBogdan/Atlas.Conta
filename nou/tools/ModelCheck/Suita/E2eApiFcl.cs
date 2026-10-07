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

// ======= Felia Api FCL — culegere, TVA, operare (F4-D9, pasul 1 al feliei) =======
// Fluxul de VÂNZARE parcurs prin CONTRACTUL feliei: WriteDto →
// `FacturaIesireApply.Aplica` → `Citeste`/`Lista` → dry-run → `ComenziDocument`.
// Endpoint-urile din host sunt transport peste EXACT acest cod, deci ce e verde
// aici e verde și pe sârmă. Blocul trăiește în suita PRIVATĂ fiindcă vânzarea
// din stoc e a profilului privat (la bugetar liniile de stoc pe FCL sunt
// interzise declarativ — 30a — și DSC e tip inert).
//
// Ce exersează în plus față de felia FCT:
//   * `Numar` SERVER-OWNED (serie fiscală „FCL-"): lipsește din WriteDto și se
//     consumă abia la materializarea operării — exact invers față de FCT;
//   * PINUL de lot CULES pe linie (`LotId` de bază) lângă produs („General!" +
//     „Specific?", P2 §4) — pe FCT lotul era server-owned;
//   * `GestiuneDescarcareId` pe header;
//   * DSC-ul autogenerat apare în `Copii[]` — îl generează MOTORUL
//     (`GenereazaSecundar` → `DescarcareService`), felia doar îl citește.
static class E2eApiFcl {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajApiFcl = "E2E-API-FCL";
            using var os = s.Provider.CreateObjectSpace();

            void CurataApiFcl(IObjectSpace o) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(o);
                var repIds = o.GetObjectsQuery<Repartitor>()
                    .Where(r => r.Cod.StartsWith(MarcajApiFcl)).Select(r => r.ID).ToList();
                var docs = o.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(o.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
                pj.Adauga(o.GetObjectsQuery<RegistruStoc>()
                    .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(o.GetObjectsQuery<RegistruContabil>()
                    .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(o.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                // Copiii autogenerați (DSC) întâi — DocumentSursa spre FCL.
                foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                    pj.Adauga(doc);
                // Rândurile de sold de DESCHIDERE n-au document (25e): se prind pe lot.
                var lotIds = o.GetObjectsQuery<Lot>()
                    .Where(l => l.Produs.Cod.StartsWith(MarcajApiFcl)).Select(l => l.ID).ToList();
                DeschidereScena.Curata(o, pj, lotIds);
                pj.Adauga(o.GetObjectsQuery<RegistruStoc>().Where(r => lotIds.Contains(r.LotId)).ToList());
                pj.Adauga(o.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFcl)).ToList());
                pj.Adauga(o.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiFcl)).ToList());
                pj.Adauga(o.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiFcl)).ToList());
                pj.Adauga(o.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiFcl)).ToList());
                pj.Executa();
            }
            CurataApiFcl(os);

            var sediuFcl = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipMarfa = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");   // MF → Marfuri
            var tipServiciuFcl = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704"); // VEN
            var n21Fcl = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var sddFcl = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            var cont4111Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            var cont4427Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
            var cont707Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "707");

            var clientFcl = os.CreateObject<Partener>();
            clientFcl.Cod = MarcajApiFcl + "-CL";
            clientFcl.Denumire = "Client probă felia Api FCL";
            clientFcl.CodFiscal = "RO87654321";
            var gestiuneFcl = os.CreateObject<Gestiune>();
            gestiuneFcl.Cod = MarcajApiFcl + "-G";
            gestiuneFcl.Denumire = "Gestiune probă felia Api FCL";
            var codEcFcl = os.CreateObject<CodEconomic>();
            codEcFcl.Cod = MarcajApiFcl + "-CE";
            codEcFcl.Denumire = "Cod economic probă felia Api FCL";
            var produsFcl = os.CreateObject<Produs>();
            produsFcl.Cod = MarcajApiFcl + "-A";
            produsFcl.Denumire = "Marfă probă felia Api FCL";
            produsFcl.UM = "BUC";
            produsFcl.TipMaterial = tipMarfa;
            os.CommitChanges();

            Lot LotFcl(decimal pret, DateOnly data) {
                var l = os.CreateObject<Lot>();
                l.Produs = produsFcl; l.Gestiune = gestiuneFcl; l.PretUnitar = pret; l.Data = data;
                return l;
            }
            var lotVechiFcl = LotFcl(5m, new DateOnly(2026, 5, 2));
            var lotNouFcl = LotFcl(6m, new DateOnly(2026, 5, 4));
            DeschidereScena.Scrie(os, lotNouFcl.Data,
                (lotVechiFcl, gestiuneFcl.ID, 10m, 50m), (lotNouFcl, gestiuneFcl.ID, 10m, 60m));
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunFcl(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // --- Apply: creare din WriteDto (fără Numar/Valoare — server-owned) ---
            var writeFcl = new FacturaIesireWriteDto {
                Data = new DateOnly(2026, 5, 10),
                PredatorId = sediuFcl.ID,
                PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiuneFcl.ID,
                Linii = {
                    // Pin pe lotul NOU (mai scump): identificarea specifică bate FIFO.
                    new FacturaIesireLinieWriteDto {
                        TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = lotNouFcl.ID,
                        Descriere = "Marfă cu pin", Cantitate = 3m, PretUnitar = 20m,
                        TipTvaId = n21Fcl.ID, CodEconomicId = codEcFcl.ID
                    },
                    new FacturaIesireLinieWriteDto {
                        TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID,
                        Descriere = "Marfă FIFO", Cantitate = 4m, PretUnitar = 20m,
                        TipTvaId = n21Fcl.ID
                    },
                    // Fără TipTva în payload ⇒ default-ul tipului de document (N21).
                    new FacturaIesireLinieWriteDto {
                        TipMaterialId = tipServiciuFcl.ID,
                        Descriere = "Transport", Cantitate = 1m, PretUnitar = 100m
                    }
                }
            };
            var idFcl = FacturaIesireApply.Aplica(os, null, writeFcl);
            var citFcl = FacturaIesireApply.Citeste(os, idFcl);
            FacturaIesireLinieReadDto LinieFclDto(string descriere) =>
                FacturaIesireApply.Citeste(os, idFcl).Linii.Single(l => l.Descriere == descriere);

            s.Check("Apply FCL creare → header plat: NUMĂRUL LIPSEȘTE (serie fiscală server-owned, invers față de FCT), "
                + "scadență neculeasă, gestiunea de descărcare și CodFiscal-ul clientului",
                citFcl != null && citFcl.Id == idFcl && citFcl.Stare == "Draft"
                && citFcl.Numar == null && citFcl.Data == new DateOnly(2026, 5, 10)
                && citFcl.DataScadenta == null && citFcl.DataOperare == null
                && citFcl.PredatorId == sediuFcl.ID && citFcl.PredatorDenumire == sediuFcl.Denumire
                && citFcl.PrimitorId == clientFcl.ID && citFcl.PrimitorDenumire == clientFcl.Denumire
                && citFcl.PrimitorCodFiscal == "RO87654321"
                && citFcl.GestiuneDescarcareId == gestiuneFcl.ID
                && citFcl.GestiuneDescarcareDenumire == gestiuneFcl.Denumire
                && !citFcl.Autogenerat && citFcl.DocumentSursaId == null && citFcl.Copii.Count == 0
                && citFcl.PoateEdita && citFcl.PoateOpera && !citFcl.PoateAnula && !citFcl.PoateStorna);

            var lPin = citFcl.Linii.Single(l => l.Descriere == "Marfă cu pin");
            var lFifo = citFcl.Linii.Single(l => l.Descriere == "Marfă FIFO");
            var lServ = citFcl.Linii.Single(l => l.Descriere == "Transport");
            s.Check("PROBA FELIEI: PINUL de lot e CULES (spre deosebire de FCT, unde lotul e server-owned) — "
                + "linia pin poartă lotul cu eticheta lui, linia FIFO rămâne fără lot",
                lPin.LotId == lotNouFcl.ID && lPin.LotEticheta == lotNouFcl.Eticheta
                && !lPin.LotEticheta.Contains("culegere")
                && lFifo.LotId == null && lFifo.LotEticheta == null);
            s.Check("Produsul („General!”) e pe liniile de stoc, cu cod și denumire proiectate plat; serviciul n-are produs",
                lPin.ProdusId == produsFcl.ID && lPin.ProdusCod == produsFcl.Cod
                && lPin.ProdusDenumire == produsFcl.Denumire
                && lFifo.ProdusId == produsFcl.ID && lServ.ProdusId == null && lServ.ProdusCod == null);
            s.Check("TVA materializat LA CULEGERE (GATE 53c): N21 → net + 21% separat (60/12,6; 80/16,8; 100/21); Total BRUT 290,4",
                lPin is { Valoare: 60m, ValoareTva: 12.6m, Cantitate: 3m, PretUnitar: 20m }
                && lFifo is { Valoare: 80m, ValoareTva: 16.8m }
                && lServ is { Valoare: 100m, ValoareTva: 21m }
                && citFcl.Total == 290.4m);
            s.Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (N21 pe FCL, seed privat)",
                lServ.TipTvaId == n21Fcl.ID && lServ.TipTvaCod == "N21" && lServ.TipTvaCota == 21m
                && lPin.TipTvaId == n21Fcl.ID);
            s.Check("Dimensiunea FRUNZEI (DIM-2: FCL are DOAR CodEconomic) — culeasă pe linia pin, absentă pe celelalte",
                lPin.CodEconomicId == codEcFcl.ID && lPin.CodEconomicCod == codEcFcl.Cod
                && lFifo.CodEconomicId == null && lFifo.CodEconomicCod == null);

            // --- Semantica override-ului de ValoareTva (aceleași reguli ca FCT) ---
            // ROUND-TRIP: clientul retrimite agregatul ÎNTREG, inclusiv TipTva-ul primit
            // la citire (pus de default) — absența lui pe o linie existentă ar fi golire.
            writeFcl.Linii[0].Id = lPin.Id;
            writeFcl.Linii[1].Id = lFifo.Id;
            writeFcl.Linii[2].Id = lServ.Id;
            writeFcl.Linii[2].TipTvaId = lServ.TipTvaId;
            writeFcl.Linii[2].ValoareTva = 20.5m;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            s.Check("Override ValoareTva pe regim Normal → acceptat, aplicat DUPĂ calcul (36a: documentul EMIS poartă rotunjirea)",
                LinieFclDto("Transport").ValoareTva == 20.5m);
            writeFcl.Linii[2].ValoareTva = null;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            s.Check("PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
                LinieFclDto("Transport").ValoareTva == 20.5m);
            writeFcl.Linii[2].PretUnitar = 200m;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            s.Check("Schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
                LinieFclDto("Transport") is { Valoare: 200m, ValoareTva: 42m });
            writeFcl.Linii[2].PretUnitar = 100m;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            // F13-D1 (review, defect 1): pe LIVRARE o linie în taxare inversă cu TVA
            // cules e refuzată la PUT, nu abia la operare — draftul nu minte în ReadDto.
            var tipTvaServ = writeFcl.Linii[2].TipTvaId;
            writeFcl.Linii[2].TipTvaId = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21").ID;
            writeFcl.Linii[2].ValoareTva = 63m;
            s.CheckRefuza("F13-D1 PUT FCL cu linie TI21 + ValoareTva 63 → refuz la salvare («Taxarea inversă pe livrare nu poartă TVA»)",
                () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
            writeFcl.Linii[2].ValoareTva = null;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            s.Check("F13-D1 PUT FCL cu linie TI21 FĂRĂ TVA → acceptat, ValoareTva 0 de la culegere (Colectat × TI)",
                LinieFclDto("Transport").ValoareTva == 0m);
            writeFcl.Linii[2].TipTvaId = tipTvaServ;
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);

            writeFcl.Linii[2].ValoareTva = -5m;
            s.CheckRefuza("Override ValoareTva NEGATIV → refuz (F2-D7)",
                () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
            writeFcl.Linii[2].TipTvaId = sddFcl.ID;
            writeFcl.Linii[2].ValoareTva = 5m;
            s.CheckRefuza("Override pe regim Scutit (SDD) → refuz (regimul nu poartă TVA separat)",
                () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
            writeFcl.Linii[2].TipTvaId = n21Fcl.ID;
            writeFcl.Linii[2].ValoareTva = null;

            // --- Refuzuri de contract (mesaj de domeniu, nu excepție de infrastructură) ---
            FacturaIesireWriteDto PayloadFcl(FacturaIesireLinieWriteDto linie, Guid? gestiune = null) =>
                new() {
                    Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                    GestiuneDescarcareId = gestiune ?? gestiuneFcl.ID,
                    Linii = { linie }
                };
            s.CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                    Id = Guid.NewGuid(), TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 1m })));
            s.CheckRefuza("Apply cu preț unitar în afara scării numeric(18,6) → refuz de domeniu al gardianului (104c), nu DbUpdateException", () => {
                using var osGard = s.Provider.CreateObjectSpace();
                new GardianEditare().OnObjectSpaceCreated(osGard);
                FacturaIesireApply.Aplica(osGard, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 0.0000001m }));
            });
            s.CheckRefuza("Apply cu pin pe lot inexistent → refuz cu mesaj de domeniu (nu violare de FK)", () =>
                FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = Guid.NewGuid(),
                    Cantitate = 1m, PretUnitar = 10m })));
            s.CheckRefuza("Apply cu gestiune de descărcare inexistentă → refuz cu mesaj de domeniu", () =>
                FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 1m }, Guid.NewGuid())));
            // M3 (F4 §Închidere): același Id de linie de DOUĂ ori în payload — a doua
            // apariție ar suprascrie tăcut prima; reconcilierea o refuză explicit.
            s.CheckRefuza("Apply cu același Id de linie repetat în payload → refuz (nu suprascriere tăcută)", () =>
                FacturaIesireApply.Aplica(os, idFcl, new FacturaIesireWriteDto {
                    Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                    GestiuneDescarcareId = gestiuneFcl.ID,
                    Linii = { writeFcl.Linii[0], writeFcl.Linii[0] }
                }));
            // M3: linie de tip BAZĂ pe draft (draft vechi pre-P2, «detaliu generic»).
            // Actualizarea ei prin Id se refuză („tip vechi"); absența ei din payload
            // o CURĂȚĂ — proba curățeniei e check-ul de reziduu de mai jos (3 linii).
            var linieBazaFcl = os.CreateObject<DocumentDetaliu>(); // NU FacturaIesireDetaliu
            linieBazaFcl.Document = os.GetObjectByKey<FacturaIesire>(idFcl);
            linieBazaFcl.TipMaterial = tipServiciuFcl; linieBazaFcl.Cantitate = 1m;
            os.CommitChanges();
            s.CheckRefuza("Apply cu Id de linie de tip BAZĂ (draft vechi) → refuz «tip vechi», nu adoptare tăcută", () =>
                FacturaIesireApply.Aplica(os, idFcl, new FacturaIesireWriteDto {
                    Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                    GestiuneDescarcareId = gestiuneFcl.ID,
                    Linii = { new FacturaIesireLinieWriteDto {
                        Id = linieBazaFcl.ID, TipMaterialId = tipServiciuFcl.ID,
                        Cantitate = 1m, PretUnitar = 1m } }
                }));
            FacturaIesireApply.Aplica(os, idFcl, writeFcl);
            citFcl = FacturaIesireApply.Citeste(os, idFcl);
            s.Check("Un Apply refuzat nu lasă reziduu, iar linia de tip BAZĂ absentă din payload e CURĂȚATĂ: "
                + "re-aplicarea payload-ului valid readuce agregatul la exact 3 linii, cu Total 290,4",
                citFcl.Linii.Count == 3 && citFcl.Total == 290.4m
                && citFcl.GestiuneDescarcareId == gestiuneFcl.ID);

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul FCL valid → listă goală", DryRunFcl(idFcl).Count == 0);
            s.Check("Dry-run-ul nu materializează nimic: documentul rămâne Draft, fără număr și fără registre",
                FacturaIesireApply.Citeste(os, idFcl) is { Stare: "Draft", Numar: null }
                && CubScena.FaraNote(os, idFcl));

            var rezFcl = ComenziDocument.Sistem(os).Opereaza(idFcl);
            s.Check("ComenziDocument.Opereaza pe FCL → Operat + ConexId (descărcarea generată în aceeași tranzacție), cu mesaj pentru operator",
                rezFcl.StareNoua == StareDocument.Operat && rezFcl.ConexId != null
                && rezFcl.Mesaje.Count == 1);

            citFcl = FacturaIesireApply.Citeste(os, idFcl);
            s.Check("Citeste după operare: numărul vine ACUM din seria fiscală (FCL-), scadența din politică (+30), affordances inversate",
                citFcl.Stare == "Operat" && citFcl.Numar?.StartsWith("FCL-") == true
                && citFcl.DataOperare != null && citFcl.DataScadenta == citFcl.Data.AddDays(30)
                && !citFcl.PoateEdita && !citFcl.PoateOpera && citFcl.PoateAnula && citFcl.PoateStorna);
            s.Check("Citeste.Copii → DESCĂRCAREA conexă: codul ancorei TipDocument (DSC), draft autogenerat fără număr propriu",
                citFcl.Copii.Count == 1 && citFcl.Copii[0].Id == rezFcl.ConexId
                && citFcl.Copii[0].Tip == "DSC" && citFcl.Copii[0].Stare == "Draft"
                && citFcl.Copii[0].Autogenerat && citFcl.Copii[0].Numar == null);
            s.Check("Valorile culese supraviețuiesc operării (PregatesteOperare le rescrie din aceeași formulă)",
                LinieFclDto("Marfă cu pin") is { Valoare: 60m, ValoareTva: 12.6m }
                && citFcl.Total == 290.4m);

            var noteApiFclCub = CubScena.Note(os, idFcl);
            s.Check("Aceeași cale de postare ca în UI: 4111 = 707 pe marfă (140), 4111 = 704 pe serviciu (100), "
                + "4427 colectat per linie (50,4) — costul rămâne pe DSC",
                noteApiFclCub.Rulaj(cont707Fcl.ID, N.Latura.Credit) == 140m
                && noteApiFclCub.Rulaj(tipServiciuFcl.ContImplicitId, N.Latura.Credit) == 100m
                && noteApiFclCub.Rulaj(cont4427Fcl.ID, N.Latura.Credit) == 50.4m
                && noteApiFclCub.Where(p => p.Debit).All(p => p.Cont == cont4111Fcl.ID));

            // --- Gardienii de scriere, prin contract ---
            s.CheckRefuza("Apply peste FCL Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
            s.CheckRefuza("Sterge peste FCL Operat → același refuz de domeniu",
                () => FacturaIesireApply.Sterge(os, idFcl));

            // --- Lista ---
            var listaFcl = FacturaIesireApply.Lista(os).Where(x => x.Id == idFcl).ToList();
            s.Check("Lista FCL → un rând, cu Stare ca text (CASE în SQL), emitent/client, scadență și Total BRUT din agregat",
                listaFcl.Count == 1 && listaFcl[0].Stare == "Operat"
                && listaFcl[0].Numar == citFcl.Numar
                && listaFcl[0].PredatorDenumire == sediuFcl.Denumire
                && listaFcl[0].PrimitorDenumire == clientFcl.Denumire
                && listaFcl[0].DataScadenta == citFcl.DataScadenta && listaFcl[0].Total == 290.4m);
            s.Check("Lista FCL → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take)",
                FacturaIesireApply.Lista(os).Where(x => x.Stare == "Operat")
                    .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

            // --- Sterge pe draft (documentul + liniile; loturile REFERITE supraviețuiesc) ---
            var idFclDraft = FacturaIesireApply.Aplica(os, null, new FacturaIesireWriteDto {
                Data = new DateOnly(2026, 5, 12), PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiuneFcl.ID,
                Linii = { new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = lotVechiFcl.ID,
                    Descriere = "De șters", Cantitate = 1m, PretUnitar = 10m, TipTvaId = n21Fcl.ID } }
            });
            FacturaIesireApply.Sterge(os, idFclDraft);
            s.Check("Sterge pe draft: documentul și liniile dispar, dar LOTUL pin rămâne (FCL îl referă, nu-l naște)",
                FacturaIesireApply.Citeste(os, idFclDraft) == null
                && !os.GetObjectsQuery<FacturaIesireDetaliu>().Any(l => l.DocumentId == idFclDraft)
                && os.GetObjectByKey<Lot>(lotVechiFcl.ID) != null);

            // ═══ Pasul 2 al feliei: descărcarea de gestiune prin API (F4-D2/D3/D4) ═══
            // Fluxul de VÂNZARE continuă de unde s-a oprit culegerea: descărcarea
            // conexă, generată de motor în tranzacția operării facturii, se CITEȘTE
            // prin felia ei (F4-D2: citire + comenzi, fără agregat de scriere), se
            // OPEREAZĂ prin comenzile generice, iar pozițiile fără stoc la facturare
            // (backorder) trec prin comanda manuală de generare (F4-D3) și prin
            // proiecția de rest (F4-D4).
            var cont607Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "607");

            var idDsc = citFcl.Copii.Single().Id;
            var citDsc = DscApply.Citeste(os, idDsc);
            s.Check("DscApply.Citeste → header: draft AUTOGENERAT legat de factura-sursă (cu numărul ei, pentru link-ul «Generat din»), "
                + "predator = GESTIUNEA de descărcare, primitor = clientul (laturile se ÎNLOCUIESC, nu se inversează)",
                citDsc != null && citDsc.Id == idDsc && citDsc.Stare == "Draft"
                && citDsc.Numar == null && citDsc.Data == new DateOnly(2026, 5, 10)
                && citDsc.Autogenerat && citDsc.DocumentSursaId == idFcl
                && citDsc.DocumentSursaNumar == citFcl.Numar && citDsc.DocumentSursaTip == "FCL"
                && citDsc.PredatorId == gestiuneFcl.ID && citDsc.PredatorDenumire == gestiuneFcl.Denumire
                && citDsc.PrimitorId == clientFcl.ID && citDsc.PrimitorDenumire == clientFcl.Denumire);
            s.Check("DSC prin API = CITIRE + comenzi (F4-D2): nicio affordance de editare, dar operarea e disponibilă pe draft",
                !citDsc.PoateEdita && citDsc.PoateOpera && !citDsc.PoateAnula && !citDsc.PoateStorna);

            var dPin = citDsc.Linii.Single(l => l.LinieSursaId == lPin.Id);
            var dFifo = citDsc.Linii.Single(l => l.LinieSursaId == lFifo.Id);
            s.Check("PROBA descărcării: PINUL respectat (3 buc din lotul pin, la 6 lei) și FIFO pe restul (4 buc din lotul VECHI, la 5) — "
                + "valoarea e COSTUL lotului, decuplat de prețul de vânzare (20)",
                citDsc.Linii.Count == 2
                && dPin.LotId == lotNouFcl.ID && dPin.Cantitate == 3m && dPin.Valoare == 18m
                && dFifo.LotId == lotVechiFcl.ID && dFifo.Cantitate == 4m && dFifo.Valoare == 20m
                && citDsc.Total == 38m);
            s.Check("Linia de descărcare: produsul vine PRIN LOT (frunza DSC n-are ProdusId), cu eticheta lotului; "
                + "linia de SERVICIU a facturii nu produce descărcare",
                dPin.ProdusId == produsFcl.ID && dPin.ProdusDenumire == produsFcl.Denumire
                && dPin.LotEticheta == lotNouFcl.Eticheta
                && dPin.TipMaterialId == tipMarfa.ID && dPin.TipMaterialCod == tipMarfa.Cod
                && citDsc.Linii.All(l => l.LinieSursaId != lServ.Id));
            s.Check("Dimensiunea frunzei (DIM-2) e CLONATĂ de pe linia FCL sursă: CodEconomic pe linia pin, absent pe cealaltă",
                dPin.CodEconomicId == codEcFcl.ID && dPin.CodEconomicCod == codEcFcl.Cod
                && dFifo.CodEconomicId == null && dFifo.CodEconomicCod == null);

            // --- Comenzile generice pe DSC (nimic nou în motor) ---
            s.Check("Dry-run pe descărcarea draft → listă goală (aceiași gardieni ca la operare)",
                DryRunFcl(idDsc).Count == 0);
            var rezDsc = ComenziDocument.Sistem(os).Opereaza(idDsc);
            s.Check("ComenziDocument pe DSC → Operat, FĂRĂ conex (descărcarea e frunza lanțului conex)",
                rezDsc.StareNoua == StareDocument.Operat && rezDsc.ConexId == null && rezDsc.Mesaje.Count == 0);
            citDsc = DscApply.Citeste(os, idDsc);
            s.Check("Citeste după operare: numărul din seria proprie (DSC-), affordances inversate",
                citDsc.Stare == "Operat" && citDsc.Numar?.StartsWith("DSC-") == true
                && citDsc.DataOperare != null
                && !citDsc.PoateEdita && !citDsc.PoateOpera && citDsc.PoateAnula && citDsc.PoateStorna);

            var noteDscApiCub = CubScena.Note(os, idDsc);
            s.Check("Costul se postează pe DSC, decuplat de vânzare: 607 = 371 la 38 (18+20), în timp ce factura a postat 140 pe 707",
                noteDscApiCub.Rulaj(cont607Fcl.ID, N.Latura.Debit) == 38m
                && noteDscApiCub.Rulaj(tipMarfa.ContImplicitId, N.Latura.Credit) == 38m
                && noteDscApiCub.All(p => p.Debit ? p.Cont == cont607Fcl.ID : p.Cont == tipMarfa.ContImplicitId));
            var stocDscApiCub = CubScena.Stoc(os, idDsc);
            s.Check("Operarea DSC scoate marfa din gestiune: −3 pe lotul pin, −4 pe lotul vechi, Marfuri, pe gestiunea de descărcare",
                stocDscApiCub.Count == 2
                && stocDscApiCub.Single(p => p.Unitate == lotNouFcl.ID).Cantitate == -3m
                && stocDscApiCub.Single(p => p.Unitate == lotVechiFcl.ID).Cantitate == -4m
                && stocDscApiCub.All(p => p.Cont == tipMarfa.ContImplicitId && p.Gestiune == gestiuneFcl.ID));

            citFcl = FacturaIesireApply.Citeste(os, idFcl);
            s.Check("Affordance ONESTĂ pe grupul conex: cu descărcarea OPERATĂ, factura nu mai poate fi anulată/stornată",
                !citFcl.PoateAnula && !citFcl.PoateStorna
                && citFcl.Copii.Single() is { Tip: "DSC", Stare: "Operat" });
            var restFcl1 = FacturaIesireApply.RestNedescarcat(os, idFcl);
            s.Check("Factură acoperită integral: rest zero pe AMBELE linii de stoc (rândurile acoperite rămân în proiecție — "
                + "tabelul arată starea acoperirii, nu doar lipsa), iar PoateGeneraDescarcare e fals",
                restFcl1.Count == 2 && restFcl1.All(r => r.Rest == 0m && r.Acoperit == r.Cantitate)
                && !citFcl.PoateGeneraDescarcare);
            var genFaraRest = FacturaIesireApply.GenereazaDescarcare(os, idFcl, new DateOnly(2026, 5, 15));
            s.Check("GenereazaDescarcare pe o factură fără rest → DscId null (nimic de generat NU e eroare)",
                genFaraRest.DscId == null && genFaraRest.Resturi.Count == 0);

            // --- Backorder (F4-D3/D4): factura cere mai mult decât soldul disponibil ---
            // Rămas în gestiune după descărcarea de mai sus: 6 buc lot vechi + 7 buc
            // lot pin = 13. Factura cere 20 ⇒ 7 rămân backorder (venitul se postează
            // acum, costul la disponibilitate — fluxul-ancoră al magazinului, 37).
            var idFcl2 = FacturaIesireApply.Aplica(os, null, new FacturaIesireWriteDto {
                Data = new DateOnly(2026, 5, 20),
                PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiuneFcl.ID,
                Linii = { new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID,
                    Descriere = "Comandă parțial acoperită", Cantitate = 20m, PretUnitar = 20m,
                    TipTvaId = n21Fcl.ID } }
            });
            s.CheckRefuza("GenereazaDescarcare pe FCL DRAFT → refuz de DOMENIU (pe draft nu există încă acoperire de generat)",
                () => FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20)));

            var rezFcl2 = ComenziDocument.Sistem(os).Opereaza(idFcl2);
            var idDsc2 = rezFcl2.ConexId.Value;
            var citDsc2 = DscApply.Citeste(os, idDsc2);
            s.Check("Backorder: descărcarea conexă alocă DOAR disponibilul (6 din lotul vechi + 7 din lotul pin = 13); "
                + "generatorul nu aruncă niciodată la lipsă de stoc",
                citDsc2.Linii.Count == 2 && citDsc2.Linii.Sum(l => l.Cantitate) == 13m
                && citDsc2.Linii.Single(l => l.LotId == lotVechiFcl.ID).Cantitate == 6m
                && citDsc2.Linii.Single(l => l.LotId == lotNouFcl.ID).Cantitate == 7m);

            var restFcl2 = FacturaIesireApply.RestNedescarcat(os, idFcl2);
            s.Check("RestNedescarcat (F4-D4): un rând per linie de stoc, cu produsul DENUMIT server-side — 20 cerute / 13 acoperite / 7 rest",
                restFcl2.Count == 1 && restFcl2[0].ProdusId == produsFcl.ID
                && restFcl2[0].ProdusDenumire == produsFcl.Denumire && restFcl2[0].LotId == null
                && restFcl2[0].Cantitate == 20m && restFcl2[0].Acoperit == 13m && restFcl2[0].Rest == 7m);
            var proiectieFcl2 = DescarcareService.RestNedescarcat(os, os.GetObjectByKey<FacturaIesire>(idFcl2));
            s.Check("Consistență (42c): DTO-ul de rest == proiecția `DescarcareService.RestNedescarcat` per linie",
                proiectieFcl2.Count == restFcl2.Count && proiectieFcl2.All(p => {
                    var r = restFcl2.Single(x => x.LinieId == p.LinieId);
                    return r.Cantitate == p.Cantitate && r.Acoperit == p.Acoperit
                        && r.Rest == p.RestNeacoperit && r.ProdusId == p.ProdusId && r.LotId == p.LotId;
                }));
            s.Check("PoateGeneraDescarcare (affordance server-side, F4-D4): adevărat pe factura operată, cu gestiune și rest > 0",
                FacturaIesireApply.Citeste(os, idFcl2).PoateGeneraDescarcare);

            // Descărcarea parțială se operează (draftul nu rezervă stoc — gardianul de
            // sold rămâne autoritatea), apoi comanda manuală se lovește de lipsă.
            ComenziDocument.Sistem(os).Opereaza(idDsc2);
            var genFaraStoc = FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20));
            s.Check("GenereazaDescarcare fără sold disponibil → DscId null, dar restul se RAPORTEAZĂ (7 buc așteaptă marfă)",
                genFaraStoc.DscId == null && genFaraStoc.Resturi.Count == 1 && genFaraStoc.Resturi[0].Rest == 7m);

            var furnizorSupl = os.CreateObject<Partener>();
            furnizorSupl.Cod = MarcajApiFcl + "-FURN"; furnizorSupl.Denumire = furnizorSupl.Cod;
            var facturaSupl = os.CreateObject<FacturaIntrare>();
            facturaSupl.Data = new DateOnly(2026, 5, 18); facturaSupl.Numar = MarcajApiFcl + "-FCT";
            facturaSupl.Predator = furnizorSupl; facturaSupl.Primitor = gestiuneFcl;
            var linieSupl = os.CreateObject<FacturaIntrareDetaliu>(); linieSupl.Document = facturaSupl;
            linieSupl.TipMaterial = tipMarfa; linieSupl.Cantitate = 7m; linieSupl.PretUnitar = 8m;
            var lotSuplFcl = linieSupl.CreeazaLot(os, produsFcl, gestiuneFcl);
            os.CommitChanges();
            var nirSupl = MotorOperare.Opereaza(os, facturaSupl);
            if (nirSupl != null) MotorOperare.Opereaza(os, nirSupl);

            var genBackorder = FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20));
            s.Check("Comanda manuală de generare (F4-D3), după suplimentarea stocului: al DOILEA DSC, exact restul, rest zero după el",
                genBackorder.DscId != null && genBackorder.Resturi.Count == 0);
            var citDsc3 = DscApply.Citeste(os, genBackorder.DscId.Value);
            s.Check("DSC₂ (backorder) — autogenerat pe factura-sursă, la data comenzii, cu linia-sursă păstrată și costul lotului nou (7 × 8)",
                citDsc3.Autogenerat && citDsc3.DocumentSursaId == idFcl2
                && citDsc3.Data == new DateOnly(2026, 5, 20) && citDsc3.Stare == "Draft"
                && citDsc3.Linii.Count == 1
                && citDsc3.Linii[0] is { Cantitate: 7m, Valoare: 56m }
                && citDsc3.Linii[0].LotId == lotSuplFcl.ID
                && citDsc3.Linii[0].LinieSursaId == restFcl2[0].LinieId);

            var citFcl2 = FacturaIesireApply.Citeste(os, idFcl2);
            s.Check("Grupul conex al facturii cu backorder: DOUĂ descărcări (una operată, una draft) — și PoateGeneraDescarcare a redevenit fals",
                citFcl2.Copii.Count == 2 && citFcl2.Copii.All(c => c.Tip == "DSC" && c.Autogenerat)
                && citFcl2.Copii.Count(c => c.Stare == "Operat") == 1
                && !citFcl2.PoateGeneraDescarcare
                && FacturaIesireApply.RestNedescarcat(os, idFcl2).Single().Rest == 0m);

            // --- Plafonul de acoperire per linie-sursă (review advers F4/D2) ---
            // Operat pe linia fcl2: 13 (DSC-ul conex); draftul de backorder (7) NU
            // intră în plafon — drafturile nu postează nimic, anti-dublarea lor e a
            // GENERATORULUI (RestNedescarcat le numără). Un DSC MANUAL cu încă 8 pe
            // aceeași linie-sursă ar duce materializarea la 21 din 20 facturate →
            // refuz zgomotos la operare (închide și dublura de generare concurentă:
            // primul document operat câștigă, al doilea pică aici).
            var dscManualFcl = os.CreateObject<DescarcareGestiune>();
            dscManualFcl.Data = new DateOnly(2026, 5, 21);
            dscManualFcl.PredatorId = gestiuneFcl.ID;
            dscManualFcl.PrimitorId = clientFcl.ID;
            dscManualFcl.DocumentSursa = os.GetObjectByKey<FacturaIesire>(idFcl2);
            var linieManualFcl = os.CreateObject<DescarcareGestiuneDetaliu>();
            linieManualFcl.Document = dscManualFcl;
            linieManualFcl.TipMaterialId = tipMarfa.ID;
            linieManualFcl.LotId = lotSuplFcl.ID;
            linieManualFcl.LinieSursaId = restFcl2[0].LinieId;
            linieManualFcl.Cantitate = 8m;
            os.CommitChanges();
            s.CheckRefuza("Plafonul de acoperire per linie-sursă (F4/D2): descărcarea MANUALĂ suprapusă "
                + "(8 peste cei 13 operați, din 20 facturate) → refuz la operare",
                () => ComenziDocument.Sistem(os).Opereaza(dscManualFcl.ID));
            os.Delete(linieManualFcl);
            os.Delete(dscManualFcl);
            os.CommitChanges();

            // --- Lista DSC ---
            var listaDsc = DscApply.Lista(os).Where(x => x.Id == idDsc).ToList();
            s.Check("Lista DSC → un rând, cu Stare ca text (CASE în SQL), gestiune/client, marcajul «autogenerat» și costul din agregat",
                listaDsc.Count == 1 && listaDsc[0].Stare == "Operat" && listaDsc[0].Numar == citDsc.Numar
                && listaDsc[0].PredatorDenumire == gestiuneFcl.Denumire
                && listaDsc[0].PrimitorDenumire == clientFcl.Denumire
                && listaDsc[0].Autogenerat && listaDsc[0].Total == 38m);
            s.Check("Lista DSC → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take); "
                + "Citeste pe un id care nu e descărcare → null, nu excepție",
                DscApply.Lista(os).Where(x => x.Stare == "Operat")
                    .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1
                && DscApply.Citeste(os, idFcl) == null);

            // --- Lanțul de anulare/storno pe grup ---
            ComenziDocument.Sistem(os).AnuleazaOperarea(idDsc);
            s.Check("Anularea descărcării o readuce pe Draft, îi șterge rândurile de stoc și ELIBEREAZĂ factura "
                + "(PoateAnula/PoateStorna redevin adevărate — draftul continuă să acopere)",
                DscApply.Citeste(os, idDsc).Stare == "Draft"
                && CubScena.FaraStoc(os, idDsc)
                && FacturaIesireApply.Citeste(os, idFcl) is { PoateAnula: true, PoateStorna: true }
                && FacturaIesireApply.RestNedescarcat(os, idFcl).All(r => r.Rest == 0m));
            var rezStornoDsc2 = ComenziDocument.Sistem(os).Storneaza(idDsc2, new DateOnly(2026, 5, 25));
            s.Check("Storno pe descărcarea operată: Stornat — iar acoperirea se REDESCHIDE (stornatul nu acoperă, draftul da): "
                + "restul urcă de la 0 la 13 și PoateGeneraDescarcare redevine adevărat",
                rezStornoDsc2.StareNoua == StareDocument.Stornat
                && DscApply.Citeste(os, idDsc2) is { Stare: "Stornat", PoateAnula: false, PoateStorna: false }
                && FacturaIesireApply.RestNedescarcat(os, idFcl2).Single().Rest == 13m
                && FacturaIesireApply.Citeste(os, idFcl2).PoateGeneraDescarcare);

            CurataApiFcl(os);
            s.Check("Curățenie finală felia Api FCL + DSC (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiFcl))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiFcl))
                && !os.GetObjectsQuery<FacturaIesire>().Any(d => d.ID == idFcl || d.ID == idFcl2)
                && !os.GetObjectsQuery<DescarcareGestiune>().Any(d => d.ID == idDsc || d.ID == idDsc2));
        }
    }
}
