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

// ===================== Felia Api DEC (F8-D13, blocul E2E-ADEC) =====================
// Fluxul-ancoră al decontului parcurs prin CONTRACTUL feliei: `DecontWriteDto` →
// `DecontApply.Aplica` → `Citeste`/`Lista` → dry-run → `ComenziDocument.Opereaza` →
// registre → imperecherea lanțului avans↔decont. Endpoint-urile din host sunt
// transport peste EXACT acest cod (blocul e2e 3c de mai sus probează MOTORUL pe
// obiecte construite direct — aici se probează CULEGEREA).
//
// Ce e PROPRIU tipului, față de toate feliile de până acum:
//   * POSTAREA EXPLICITĂ PE LINIE (32a): contul cules BATE `SursaCont`, iar
//     repartitorul cules e nivelul MAXIM al coalesce-ului de dimensiuni;
//   * CANTITATEA PRO-FORMA 0 → 1, de acum VIZIBILĂ la culegere (F8-D2), nu doar
//     în `PregatesteOperare`;
//   * `ILinieCuPretUnitar` (F8-D2) ⇒ `Valoare`/`ValoareTva` se materializează la
//     culegere prin ACELAȘI helper ca FCT/FCL.
// Rulează pe profilul BUGETAR (baza aplicației). NOTĂ de profil: acolo toate
// regimurile de TVA sunt Capitalizat, deci calea de override e doar REFUZ —
// semantica POZITIVĂ a override-ului (păstrare fără declanșator, cedare la
// schimbarea bazei) se probează în blocul privat, pe N21, exact ca la FCT
// (precedentul 56f — nu se inventează tipuri de TVA în seed pentru probe).
static class E2eApiDec {
    public static void Ruleaza(Suita s) {
        const string MarcajApiDec = "E2E-API-DEC";

        void CurataApiDec(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajApiDec)).Select(r => r.ID).ToList();
            // Scena șterge deconturi și pe calea API (`DecontApply.Sterge` ⇒ `os.Delete` ⇒
            // ștergere AMÂNATĂ, 60a). Documentele acelea NU se prind pe laturi — laturile
            // decontului de probă sunt repartitori de SEED (angajat/unitate), nu marcați —
            // dar liniile lor poartă codul economic al scenei, care e marcat. Fără firul
            // ăsta rândurile rămâneau în tabelă (invizibile, dar prezente) și blocau purja
            // codului economic pe FK — măsurat pe profilul bugetar la prima rulare cu
            // interceptorul (F13-D2).
            var codIds = os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(MarcajApiDec)).Select(c => c.ID).ToList();
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).Select(d => d.ID)
                .Concat(os.GetObjectsQuery<DecontDetaliu>()
                    .Where(l => l.CodEconomicId != null && codIds.Contains(l.CodEconomicId.Value))
                    .Select(l => l.DocumentId))
                .Distinct().ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)));
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)));
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => docIds.Contains(d.ID)));
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiDec)));
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiDec)));
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiDec(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
            var tipDeplasari = os.FirstOrDefault<TipMaterial>(t => t.Cod == "614.00.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
            var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");
            // Profilul bugetar: DOAR regimuri Capitalizat. CAP0 (cota 0) e singurul pe
            // care „Valoare = PretUnitar × Cantitate" se citește curat; CAP21 e
            // default-ul de tip (`TipTvaImplicit`) și capitalizează 21% în Valoare.
            var cap0 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP0");
            var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");

            var titular = os.CreateObject<Angajat>();
            titular.Cod = MarcajApiDec + "-ANG";
            titular.Denumire = "Titular probă felia Api DEC";
            titular.ContImplicit = cont542;
            var codEcDec = os.CreateObject<CodEconomic>();
            codEcDec.Cod = MarcajApiDec + "-CE";
            codEcDec.Denumire = "Cod economic probă felia Api DEC";
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunDec(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }
            int SerieDec() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "DEC").UrmatorulNumar;
            List<PostareScena> NoteDecCub(Guid docId) => CubScena.Note(os, docId).Where(p => !p.Storno).ToList();

            var dataDec = new DateOnly(2026, 3, 12);

            // --- Apply: culegerea, cu cele două feluri de linie ale tipului ---
            var writeDec = new DecontWriteDto {
                Data = dataDec,
                PredatorId = titular.ID,
                PrimitorId = sediu.ID,
                NumarPV = "PV-DEC-1",
                DataPV = new DateOnly(2026, 3, 11),
                Linii = {
                    // Linia „normală": contarea cade integral pe regulă (debit din contul
                    // Tipului 614, credit pe titular). Cantitatea 0 = pro-forma.
                    new DecontLinieWriteDto {
                        TipMaterialId = tipDeplasari.ID, Descriere = "Transport delegație",
                        Cantitate = 0m, PretUnitar = 30m, TipTvaId = cap0.ID,
                        CodEconomicId = codEcDec.ID
                    },
                    // Linia cu POSTARE EXPLICITĂ: contul 623 bate Tipul 628, iar
                    // repartitorul debitor cules (MAG1) bate default-ul polimorf
                    // (debit←Predator = titularul). Fără TipTva în payload ⇒ default-ul
                    // tipului de document (CAP21, seed bugetar).
                    new DecontLinieWriteDto {
                        TipMaterialId = tipServicii.ID, Descriere = "Protocol contractare",
                        Cantitate = 2m, PretUnitar = 10m,
                        ContDebitId = cont623.ID, RepartitorDebitId = mag1.ID,
                        CodEconomicId = codEcDec.ID
                    }
                }
            };
            var idDec = DecontApply.Aplica(os, null, writeDec);
            var citDec = DecontApply.Citeste(os, idDec);
            s.Check("Apply DEC → header plat, FĂRĂ număr (seria „DEC-” e server-owned, se consumă la operare) + PV",
                citDec != null && citDec.Id == idDec && citDec.Stare == "Draft" && citDec.Numar == null
                && citDec.Data == dataDec && citDec.NumarPV == "PV-DEC-1"
                && citDec.DataPV == new DateOnly(2026, 3, 11)
                && citDec.PredatorId == titular.ID && citDec.PredatorDenumire == titular.Denumire
                && citDec.PrimitorId == sediu.ID && citDec.PrimitorDenumire == sediu.Denumire
                && citDec.Linii.Count == 2
                && citDec.PoateEdita && citDec.PoateOpera && !citDec.PoateAnula && !citDec.PoateStorna);

            var linieDeplasare = citDec.Linii.Single(l => l.TipMaterialId == tipDeplasari.ID);
            var linieProtocol = citDec.Linii.Single(l => l.TipMaterialId == tipServicii.ID);
            s.Check("F8-D2: cantitatea PRO-FORMA 0 → 1 e VIZIBILĂ imediat după Aplica (nu abia la operare, „în spate”)",
                linieDeplasare.Cantitate == 1m);
            s.Check("F8-D2 (ILinieCuPretUnitar): Valoare = PretUnitar × Cantitate materializat LA CULEGERE — 30 (CAP0) și 24,2 (CAP21 capitalizează 21% în Valoare); Total brut 54,2",
                linieDeplasare.Valoare == 30m && linieDeplasare.ValoareTva == 0m
                && linieProtocol.Valoare == 24.2m && linieProtocol.ValoareTva == 0m
                && citDec.Total == 54.2m);
            s.Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (CAP21 pe DEC, seed bugetar); linia cu TipTva cules rămâne CAP0",
                linieProtocol.TipTvaId == cap21.ID && linieProtocol.TipTvaCod == "CAP21"
                && linieProtocol.TipTvaCota == 21m
                && linieDeplasare.TipTvaId == cap0.ID && linieDeplasare.TipTvaCod == "CAP0");
            s.Check("Linia proiectează plat descrierea, prețul, dimensiunea frunzei ȘI postarea explicită (cont + repartitor, cu etichetele read-only)",
                linieDeplasare.Descriere == "Transport delegație" && linieDeplasare.PretUnitar == 30m
                && linieDeplasare.CodEconomicId == codEcDec.ID && linieDeplasare.CodEconomicCod == codEcDec.Cod
                && linieDeplasare.ContDebitId == null && linieDeplasare.ContDebitSimbol == null
                && linieDeplasare.RepartitorDebitId == null
                && linieProtocol.ContDebitId == cont623.ID && linieProtocol.ContDebitSimbol == "623.00.00"
                && linieProtocol.ContCreditId == null
                && linieProtocol.RepartitorDebitId == mag1.ID
                && linieProtocol.RepartitorDebitDenumire == mag1.Denumire
                && linieProtocol.RepartitorCreditId == null);
            var randDec = DecontApply.Lista(os).Single(d => d.Id == idDec);
            s.Check("Lista DEC: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
                randDec.Stare == "Draft" && randDec.Total == 54.2m && randDec.Numar == null
                && randDec.PredatorDenumire == titular.Denumire && randDec.PrimitorDenumire == sediu.Denumire);

            // --- Override-ul manual de ValoareTva: refuzat pe regimuri fără TVA separat ---
            // (regula F2-D1/D7, o singură sursă — semantica pozitivă e în blocul privat.)
            writeDec.Linii[0].Id = linieDeplasare.Id;
            writeDec.Linii[0].TipTvaId = linieDeplasare.TipTvaId;
            writeDec.Linii[1].Id = linieProtocol.Id;
            writeDec.Linii[1].TipTvaId = linieProtocol.TipTvaId;   // round-trip-ul ReadDto
            writeDec.Linii[1].ValoareTva = 4.2m;
            s.CheckRefuza("Override ValoareTva pe regim Capitalizat → refuz (regimul nu poartă TVA separat)",
                () => DecontApply.Aplica(os, idDec, writeDec));
            writeDec.Linii[1].ValoareTva = -1m;
            s.CheckRefuza("Override ValoareTva NEGATIV → refuz",
                () => DecontApply.Aplica(os, idDec, writeDec));
            writeDec.Linii[1].ValoareTva = null;
            DecontApply.Aplica(os, idDec, writeDec);
            s.Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact două linii, cu aceleași cifre",
                DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaRefuz && dupaRefuz.Linii.Count == 2);

            // Golirea deliberată a TipTva pe o linie EXISTENTĂ (default-ul NU se re-aplică).
            writeDec.Linii[1].TipTvaId = null;
            DecontApply.Aplica(os, idDec, writeDec);
            s.Check("Pe linia EXISTENTĂ, TipTva absent din payload = GOLIRE deliberată → valoarea revine la net 20",
                DecontApply.Citeste(os, idDec).Linii.Single(l => l.Id == linieProtocol.Id)
                    is { TipTvaId: null, Valoare: 20m, ValoareTva: 0m });
            // Repunerea default-ului (explicit, prin payload) readuce agregatul la 54,2.
            writeDec.Linii[1].TipTvaId = cap21.ID;
            DecontApply.Aplica(os, idDec, writeDec);
            s.Check("Repunerea explicită a TipTva pe linia existentă redeclanșează calculul (24,2) — Total 54,2",
                DecontApply.Citeste(os, idDec) is { Total: 54.2m });

            // --- Refuzurile de payload (reconcilierea, probele M3/60d) ---
            s.CheckRefuza("Apply DEC cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
                DecontApply.Aplica(os, idDec, new DecontWriteDto {
                    Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                    Linii = { new DecontLinieWriteDto {
                        Id = Guid.NewGuid(), TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 1m } }
                }));
            s.CheckRefuza("Apply DEC cu același Id de linie de două ori → refuz", () =>
                DecontApply.Aplica(os, idDec, new DecontWriteDto {
                    Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                    Linii = { writeDec.Linii[0], writeDec.Linii[0] }
                }));
            s.CheckRefuza("Apply DEC cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
                DecontApply.Aplica(s.OsCuGardian(), idDec, new DecontWriteDto {
                    Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                    Linii = { new DecontLinieWriteDto {
                        TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
                }));
            s.CheckRefuza("Apply DEC cu cont explicit inexistent → refuz de domeniu (rezolvarea pe navigație), nu violare de FK", () =>
                DecontApply.Aplica(os, idDec, new DecontWriteDto {
                    Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                    Linii = { new DecontLinieWriteDto {
                        TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 1m,
                        ContDebitId = Guid.NewGuid() } }
                }));

            // Reconcilierea e cea care curăță reziduul unui Apply REFUZAT: un refuz de
            // DUPĂ `CreateObject` (scara, un FK inexistent) lasă linia în ObjectSpace-ul
            // VIU al apelantului — pe host OS-ul e per-cerere și moare cu ea, dar aici
            // trăiește mai departe, iar un commit ulterior ar persista-o. Următorul
            // payload valid o șterge, fiindcă nu e în el (același contract ca pe LDI).
            DecontApply.Aplica(os, idDec, writeDec);
            s.Check("Un Apply refuzat nu lasă reziduu în agregat: următorul payload valid readuce documentul la exact două linii (reconcilierea curăță liniile create înaintea refuzului)",
                DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaRefuzuri
                && dupaRefuzuri.Linii.Count == 2);

            // Linia de tip BAZĂ (decont istoric/importat) referită prin Id: citirea o
            // arată cu câmpurile frunzei NULE (as-cast), reconcilierea o refuză
            // acționabil, iar absența ei din payload o ȘTERGE (proba M3/60d).
            var docIstoricDec = os.GetObjectByKey<Decont>(idDec);
            var linieBaza = os.CreateObject<DocumentDetaliu>();
            linieBaza.Document = docIstoricDec;
            linieBaza.TipMaterial = tipTrz;
            linieBaza.Cantitate = 1m;
            linieBaza.Valoare = 7m;
            os.CommitChanges();
            var idLinieBaza = linieBaza.ID;
            var citCuBaza = DecontApply.Citeste(os, idDec);
            s.Check("Citirea merge pe BAZA detaliului (as-cast la frunză): linia de tip BAZĂ APARE, cu câmpurile frunzei NULE",
                citCuBaza.Linii.Count == 3
                && citCuBaza.Linii.Single(l => l.Id == idLinieBaza)
                    is { Descriere: null, PretUnitar: null, ContDebitId: null, Valoare: 7m });
            s.Check("…iar `Total` o numără la fel în agregat și în listă (definiția Document.Total, pe BAZA detaliului): 61,2",
                citCuBaza.Total == 61.2m
                && DecontApply.Lista(os).Single(d => d.Id == idDec).Total == 61.2m);
            s.CheckRefuza("Apply DEC cu Id-ul unei linii de tip BAZĂ → refuz acționabil („ștergeți-o și culegeți-o din nou”)", () =>
                DecontApply.Aplica(os, idDec, new DecontWriteDto {
                    Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                    Linii = { new DecontLinieWriteDto {
                        Id = idLinieBaza, TipMaterialId = tipTrz.ID, Cantitate = 1m, PretUnitar = 7m } }
                }));
            DecontApply.Aplica(os, idDec, writeDec);
            s.Check("Linia absentă din payload se ȘTERGE (reconciliere server-side): linia de bază dispare, agregatul revine la 54,2",
                !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == idLinieBaza)
                && DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaCuratenie
                && dupaCuratenie.Linii.Count == 2);

            // --- Refuzurile de OPERARE, fără rânduri-fantomă și fără serie consumată ---
            var serieInainteDec = SerieDec();
            void RefuzDec(string nume, Guid predatorId, Guid primitorId, DecontLinieWriteDto linieProba) {
                var id = DecontApply.Aplica(os, null, new DecontWriteDto {
                    Data = dataDec, PredatorId = predatorId, PrimitorId = primitorId,
                    Linii = { linieProba }
                });
                s.Check(nume + " — dry-run-ul îl vede (fără să atingă nimic)", DryRunDec(id).Count > 0);
                s.CheckRefuza(nume, () => ComenziDocument.Sistem(os).Opereaza(id));
                s.Check(nume + " — fără rânduri-fantomă și fără număr consumat (33d + GATE D6)",
                    CubScena.FaraNote(os, id)
                    && CubScena.FaraStoc(os, id)
                    && os.GetObjectByKey<Decont>(id).Stare == StareDocument.Draft
                    && os.GetObjectByKey<Decont>(id).Numar == null);
                DecontApply.Sterge(os, id);
            }

            RefuzDec("Predator care nu e Angajat → refuz („predatorul decontului este titularul — un angajat”)",
                sediu.ID, sediu.ID,
                new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
                    PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
            RefuzDec("Primitor care nu e unitate internă / gestiune → refuz",
                titular.ID, titular.ID,
                new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
                    PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
            RefuzDec("Linie cu Valoare 0 (preț necules) → refuz („fiecare linie de decont poartă o valoare pozitivă”)",
                titular.ID, sediu.ID,
                new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
                    PretUnitar = 0m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
            RefuzDec("Linie fără clasificație bugetară (nici angajament, nici cod economic) → refuz al PoliticaValidare (33c, profil bugetar)",
                titular.ID, sediu.ID,
                new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
                    PretUnitar = 10m, TipTvaId = cap0.ID });
            RefuzDec("Tip fără cont implicit și linie fără cont explicit → refuz clar (debitul nu se poate rezolva)",
                titular.ID, sediu.ID,
                new DecontLinieWriteDto { TipMaterialId = tipTrz.ID, Cantitate = 1m,
                    PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });

            s.Check("Seria „DEC-” NU se consumă la refuz (F8-D3 + GATE D6: numărul se asignează abia la materializare)",
                SerieDec() == serieInainteDec);

            // --- Dry-run, apoi comanda ---
            s.Check("Dry-run (Valideaza) pe draftul DEC valid → listă goală", DryRunDec(idDec).Count == 0);
            s.Check("Dry-run-ul NU materializează nimic: Draft, fără număr, fără note",
                os.GetObjectByKey<Decont>(idDec).Stare == StareDocument.Draft
                && os.GetObjectByKey<Decont>(idDec).Numar == null
                && CubScena.FaraNote(os, idDec));

            var rezDec = ComenziDocument.Sistem(os).Opereaza(idDec);
            citDec = DecontApply.Citeste(os, idDec);
            s.Check("ComenziDocument.Opereaza pe DEC → Operat, cu număr din politica proprie (seria DEC-), fără conex; affordances inversate",
                rezDec.StareNoua == StareDocument.Operat && rezDec.ConexId == null
                && citDec.Numar?.StartsWith("DEC-") == true && citDec.DataOperare != null
                && !citDec.PoateEdita && !citDec.PoateOpera && citDec.PoateAnula && citDec.PoateStorna
                && SerieDec() == serieInainteDec + 1);
            s.Check("Decontul nu mișcă stoc",
                CubScena.FaraStoc(os, idDec));

            var noteDecCub = NoteDecCub(idDec);
            s.Check("Contare (2 note, una per linie): debitul din contul Tipului (614) pe linia fără postare explicită, creditul 542 pe ambele",
                noteDecCub.Nota(tipDeplasari.ContImplicitId, cont542.ID, 30m, linieDeplasare.Id)
                && noteDecCub.Any(p => p.Credit && p.LinieId == linieProtocol.Id)
                && noteDecCub.Where(p => p.Credit).All(p => p.Cont == cont542.ID));
            var debitProtocolCub = noteDecCub.Where(p => p.Debit && p.LinieId == linieProtocol.Id).ToList();
            s.Check("ANCORA F8-D13.2: contul CULES pe linie (623) BATE rezolvarea declarativă (SursaCont.TipMaterial ar fi dat contul lui 628)",
                debitProtocolCub.Count > 0 && debitProtocolCub.All(p => p.Cont == cont623.ID)
                && tipServicii.ContImplicitId != null && cont623.ID != tipServicii.ContImplicitId
                && debitProtocolCub.Sum(p => p.Valoare) == 24.2m);

            s.Check("ANCORA F8-D13.2: repartitorul CULES (MAG1) e nivelul MAXIM al coalesce-ului de dimensiuni; pe linia fără el cade default-ul polimorf (debit←Predator = titularul) [cub]",
                noteDecCub.Any(p => p.Debit && p.LinieId == linieDeplasare.Id)
                && noteDecCub.Where(p => p.Debit && p.LinieId == linieDeplasare.Id).All(p => p.CodEconomic == codEcDec.ID));
            s.Check("D9-A10 Api DEC: debitul protocolului poartă gestiunea culeasă MAG1, al deplasării titularul; creditul 542 titularul pe ambele",
                debitProtocolCub.All(p => p.Gestiune == mag1.ID)
                && noteDecCub.Where(p => p.Debit && p.LinieId == linieDeplasare.Id).All(p => p.Repartitor == titular.ID)
                && noteDecCub.Where(p => p.Credit).All(p => p.Repartitor == titular.ID));

            s.CheckRefuza("Apply peste DEC Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => DecontApply.Aplica(os, idDec, writeDec));
            s.CheckRefuza("Sterge peste DEC Operat → același refuz de domeniu",
                () => DecontApply.Sterge(os, idDec));

            // --- Lanțul avans → decont: imperecherea și affordance-ele ONESTE (57d) ---
            var avansDec = os.CreateObject<Plata>();
            avansDec.Data = new DateOnly(2026, 3, 2);
            avansDec.Predator = casa;
            avansDec.Primitor = titular;
            avansDec.TipInstrument = TipInstrumentPlata.DispozitieCasa;
            var linieAvansDec = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieAvansDec.Document = avansDec;
            linieAvansDec.TipMaterial = tipTrz;
            linieAvansDec.Valoare = 100m;
            linieAvansDec.CodEconomicId = codEcDec.ID; // 531/542 cer defalcarea E
            os.CommitChanges();
            ComenziDocument.Sistem(os).Opereaza(avansDec.ID);
            var impDec = ImperechereService.Imperecheaza(os, avansDec, os.GetObjectByKey<Decont>(idDec), 54.2m);
            citDec = DecontApply.Citeste(os, idDec);
            s.Check("ANCORA F8-D13.4: avansul (PLT pe titular) stinge decontul pe TOTALUL BRUT, iar affordance-ele devin ONESTE — PoateAnula/PoateStorna FALSE cât există imperecherea (57d)",
                ImperechereService.Ramas(os, idDec) == 0m
                && ImperechereService.Ramas(os, avansDec.ID) == 45.8m
                && citDec.Stare == "Operat" && !citDec.PoateAnula && !citDec.PoateStorna);
            s.CheckRefuza("…iar gardianul motorului confirmă: anularea decontului imperecheat = refuz",
                () => ComenziDocument.Sistem(os).AnuleazaOperarea(idDec));
            ImperechereService.Sterge(os, impDec.ID);
            citDec = DecontApply.Citeste(os, idDec);
            s.Check("După ștergerea link-ului (31d: se șterge liber), affordance-ele revin",
                citDec.PoateAnula && citDec.PoateStorna);

            // --- Anulare, re-operare idempotentă, storno ---
            s.Check("Anulare prin API → Draft + notele șterse",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idDec).StareNoua == StareDocument.Draft
                && CubScena.FaraNote(os, idDec));
            ComenziDocument.Sistem(os).Opereaza(idDec);
            s.Check("Re-operare după anulare: cantitatea pro-forma rămâne 1, valorile rămân, numărul rămâne (idempotență)",
                DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaReoperare
                && dupaReoperare.Linii.Single(l => l.Id == linieDeplasare.Id).Cantitate == 1m
                && dupaReoperare.Numar?.StartsWith("DEC-") == true);
            s.Check("Storno prin API → Stornat, note inverse append-only (−30, −24,2) la data stornării",
                ComenziDocument.Sistem(os).Storneaza(idDec, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
                && CubScena.Note(os, idDec).Where(p => p.Storno).All(p => p.Data == new DateOnly(2026, 7, 22))
                && CubScena.Note(os, idDec).Where(p => p.Storno && p.Debit && p.LinieId == linieDeplasare.Id).Sum(p => p.Valoare) == -30m
                && CubScena.Note(os, idDec).Where(p => p.Storno && p.Debit && p.LinieId == linieProtocol.Id).Sum(p => p.Valoare) == -24.2m);

            // --- Sterge pe un draft propriu ---
            var idDecSters = DecontApply.Aplica(os, null, new DecontWriteDto {
                Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
                Linii = { new DecontLinieWriteDto {
                    TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 5m,
                    TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID } }
            });
            DecontApply.Sterge(os, idDecSters);
            s.Check("Sterge pe draftul DEC: documentul și liniile lui dispar împreună",
                DecontApply.Citeste(os, idDecSters) == null
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idDecSters));

            CurataApiDec(os);
            s.Check("Curățenie finală felia Api DEC (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiDec))
                && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajApiDec))
                && os.GetObjectByKey<Decont>(idDec) == null);
        }
    }
}
