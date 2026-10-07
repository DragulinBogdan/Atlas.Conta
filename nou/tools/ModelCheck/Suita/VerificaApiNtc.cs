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

// ================= Felia API NTC (F19, track 1) — E2E-API-NTC =================
// Nota contabilă parcursă prin CONTRACTUL feliei: WriteDto →
// `NotaContabilaApply.Aplica` → `Citeste`/`Lista` → `Candidati` → dry-run →
// `ComenziDocument`. Endpoint-urile din host sunt transport peste EXACT acest cod,
// deci ce e verde aici e verde și pe sârmă.
//
// Rulează pe AMBELE profiluri (F19-D14): NTC are `PoliticaNumerotare` în
// amândouă, fiindcă nota e NEUTRĂ față de profil — fără stoc, fără contare, fără
// TVA, fără scadență, fără validare. Singurul lucru care diferă sunt SIMBOLURILE
// conturilor scenei, luate din profil, nu presupuse.
//
// Ce exersează, în plus față de blocul de MOTOR (`E2E-NTC`, care probează
// postarea explicită și refuzurile pe calea XAF):
//   * `Valoare` CULEASĂ, inclusiv NEGATIVĂ, supraviețuiește round-trip-ului
//     Write → Read → Write (F19-D8: nu există `MaterializeazaValori` care s-o
//     rescrie);
//   * reconcilierea agregatului: Id străin, Id repetat, linie de tip BAZĂ, FK
//     inexistent, scara banilor — fiecare refuz de DOMENIU, fără reziduu;
//   * seria „NTC-" NEconsumată la refuz (F19-D6 + GATE D6);
//   * `candidati` (F19-D10): plafonul per contrapartidă IDENTIC cu
//     `CapacitateStingere`, „asignat" identic cu `ImperechereService.AsignatFataDe`
//     și rândurile identice cu `DocumenteCuRest` filtrat pe acea contrapartidă —
//     cusătură MĂSURATĂ (se probează că `Disponibil` e exact suma maximă pe care
//     serviciul o acceptă și că un ban peste ea se refuză);
//   * NTC ABSENT din `DocumenteCuRest` (nota n-are semantică de „rest").
static class VerificaApiNtc {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-API-NTC";
        using var os = s.Provider.CreateObjectSpace();

        // Conturile scenei, per PROFIL (motorul nu cunoaște niciun simbol — 29;
        // scena, care e date, îl cunoaște pe al profilului ei).
        var contFurnizor = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "401" : "401.01.00"));
        var contClient = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "4111" : "411.01.01"));
        var contTranzit = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "581" : "581.01.01"));
        var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
        var tipNtc = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");

        void Curata() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            // Laturile notei sunt unitatea internă MARCATĂ a scenei, deci filtrul pe
            // laturi prinde și notele, nu doar documentele de trezorerie.
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>()
                .Where(d => docIds.Contains(d.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }
        Curata();

        s.Check("Api NTC — precondiții de profil: ancora NTC cu seria „NTC-”, conturile scenei și NICIO regulă de contare "
            + "(postarea explicită a liniei postează în ABSENȚA regulii — riscul 1 al feliei, trăsătura tipului)",
            tipNtc != null && tipNtc.ClrType == nameof(NotaContabila)
            && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC")?.Serie == "NTC-"
            && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipNtc.ID)
            && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipNtc.ID)
            && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipNtc.ID) == null
            && contFurnizor != null && contClient != null && contTranzit != null
            && casa != null && tipTrz != null);

        // ---------------- Scena ----------------
        // Laturile notei: o unitate internă MARCATĂ (nota le cere interne).
        var unitate = os.CreateObject<UnitateInterna>();
        unitate.Cod = Marcaj + "-U";
        unitate.Denumire = "Unitate probă felia Api NTC";
        // Partenerul pe care se face compensarea (contrapartida de pe LINIE) + un al
        // doilea, ca panoul să aibă DOUĂ contrapartide — exact cazul pentru care
        // `DocumenteCuRest(contrapartidaId)` nu ajunge (F19-D10).
        var partenerX = os.CreateObject<Partener>();
        partenerX.Cod = Marcaj + "-X";
        partenerX.Denumire = "Partener Api NTC X";
        var partenerY = os.CreateObject<Partener>();
        partenerY.Cod = Marcaj + "-Y";
        partenerY.Denumire = "Partener Api NTC Y";
        var codEc = os.CreateObject<CodEconomic>();
        codEc.Cod = Marcaj + "-CE";
        codEc.Denumire = "Cod economic probă Api NTC";
        os.CommitChanges();

        // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
        // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
        IReadOnlyList<string> DryRunNtc(Guid docId) {
            using var osDry = s.Provider.CreateObjectSpace();
            return ComenziDocument.Sistem(osDry).Valideaza(docId);
        }
        int SerieNtc() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC").UrmatorulNumar;

        List<PostareScena> NoteNtcCub(Guid docId) => CubScena.Note(os, docId).Where(p => !p.Storno).ToList();

        var dataNtc = new DateOnly(2026, 4, 20);

        // --- Apply: culegerea, cu cele trei feluri de linie ale scenei ---
        var write = new NtcWriteDto {
            Data = dataNtc,
            PredatorId = unitate.ID,
            PrimitorId = unitate.ID,
            Linii = {
                // Compensarea propriu-zisă: furnizor = client pe ACELAȘI partener.
                // Plafonul lui X devine 2 × 60 (o dată pe debit, o dată pe credit).
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Descriere = "Compensare X",
                    ContDebitId = contClient.ID, ContCreditId = contFurnizor.ID,
                    RepartitorDebitId = partenerX.ID, RepartitorCreditId = partenerX.ID,
                    CodEconomicId = codEc.ID, Valoare = 60m
                },
                // O singură latură cu contrapartidă ⇒ plafonul lui Y e 25.
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Descriere = "Preluare datorie Y",
                    ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerY.ID,
                    CodEconomicId = codEc.ID, Valoare = 25m
                },
                // Linia NEGATIVĂ (nota storno, F19-D8): fără repartitori pe linie,
                // deci nu atinge niciun plafon — dimensiunile cad pe default-ul
                // polimorf al header-ului (32c).
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Descriere = "Corecție cu minus",
                    ContDebitId = contTranzit.ID, ContCreditId = contClient.ID,
                    CodEconomicId = codEc.ID, Valoare = -10m
                }
            }
        };
        var idNtc = NotaContabilaApply.Aplica(os, null, write);
        var cit = NotaContabilaApply.Citeste(os, idNtc);
        s.Check("Api NTC: Apply → header plat, FĂRĂ număr (seria „NTC-” e server-owned, se consumă la operare), "
            + "cu denumirile ambelor laturi interne",
            cit != null && cit.Id == idNtc && cit.Stare == "Draft" && cit.Numar == null
            && cit.Data == dataNtc
            && cit.PredatorId == unitate.ID && cit.PredatorDenumire == unitate.Denumire
            && cit.PrimitorId == unitate.ID && cit.PrimitorDenumire == unitate.Denumire
            && cit.Linii.Count == 3
            && cit.PoateEdita && cit.PoateOpera && !cit.PoateAnula && !cit.PoateStorna);

        var lCompensare = cit.Linii.Single(l => l.Descriere == "Compensare X");
        var lPreluare = cit.Linii.Single(l => l.Descriere == "Preluare datorie Y");
        var lMinus = cit.Linii.Single(l => l.Descriere == "Corecție cu minus");
        s.Check("Api NTC (F19-D8): `Valoare` e CULEASĂ ca atare — inclusiv NEGATIVĂ (nota storno); nu există lanț de valori "
            + "care s-o rescrie, iar `Total` = Σ liniilor (60 + 25 − 10 = 75)",
            lCompensare.Valoare == 60m && lPreluare.Valoare == 25m && lMinus.Valoare == -10m
            && cit.Total == 75m);
        s.Check("Api NTC: linia proiectează plat postarea EXPLICITĂ (cont = simbol + denumire, repartitor per latură) "
            + "și dimensiunea frunzei; laturile necompletate ies null, nu 0",
            lCompensare.ContDebitId == contClient.ID && lCompensare.ContDebitSimbol == contClient.Simbol
            && lCompensare.ContDebitDenumire == contClient.Denumire
            && lCompensare.ContCreditId == contFurnizor.ID && lCompensare.ContCreditSimbol == contFurnizor.Simbol
            && lCompensare.RepartitorDebitId == partenerX.ID
            && lCompensare.RepartitorDebitDenumire == partenerX.Denumire
            && lCompensare.RepartitorCreditId == partenerX.ID
            && lPreluare.RepartitorCreditId == null && lPreluare.RepartitorCreditDenumire == null
            && lMinus.RepartitorDebitId == null && lMinus.RepartitorCreditId == null
            && lCompensare.CodEconomicId == codEc.ID && lCompensare.CodEconomicCod == codEc.Cod
            && lCompensare.TipMaterialId == tipTrz.ID && lCompensare.TipMaterialCod == tipTrz.Cod);
        var randLista = NotaContabilaApply.Lista(os).Single(d => d.Id == idNtc);
        s.Check("Api NTC: Lista dă aceleași cifre ca agregatul (Total prin join pe agregat), starea tradusă în SQL",
            randLista.Stare == "Draft" && randLista.Total == 75m && randLista.Numar == null
            && randLista.PredatorDenumire == unitate.Denumire
            && randLista.PrimitorDenumire == unitate.Denumire);

        // Round-trip complet: ReadDto → WriteDto → Apply, valorile (negativa inclusă)
        // rămân neschimbate și nu apar/dispar linii.
        var rescriere = new NtcWriteDto {
            Data = cit.Data, PredatorId = cit.PredatorId, PrimitorId = cit.PrimitorId,
            Linii = cit.Linii.Select(l => new NtcLinieWriteDto {
                Id = l.Id, TipMaterialId = l.TipMaterialId, Descriere = l.Descriere,
                ContDebitId = l.ContDebitId, ContCreditId = l.ContCreditId,
                RepartitorDebitId = l.RepartitorDebitId, RepartitorCreditId = l.RepartitorCreditId,
                CodEconomicId = l.CodEconomicId, Valoare = l.Valoare
            }).ToList()
        };
        NotaContabilaApply.Aplica(os, idNtc, rescriere);
        s.Check("Api NTC: round-trip Read → Write → Apply nu schimbă nimic (negativul supraviețuiește; "
            + "nicio linie nu apare și nu dispare)",
            NotaContabilaApply.Citeste(os, idNtc) is { Total: 75m } dupaRoundTrip
            && dupaRoundTrip.Linii.Count == 3
            && dupaRoundTrip.Linii.Single(l => l.Id == lMinus.Id).Valoare == -10m);

        // --- Refuzurile de PAYLOAD (reconcilierea) ---
        NtcWriteDto Payload(params NtcLinieWriteDto[] linii) => new() {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID, Linii = linii.ToList()
        };
        NtcLinieWriteDto LinieValida() => new() {
            TipMaterialId = tipTrz.ID, Descriere = "probă",
            ContDebitId = contTranzit.ID, ContCreditId = contClient.ID,
            CodEconomicId = codEc.ID, Valoare = 5m
        };

        s.CheckRefuza("Api NTC: Id de linie STRĂIN → refuz (agregatul nu adoptă linii din alt document)", () => {
            var l = LinieValida(); l.Id = Guid.NewGuid();
            NotaContabilaApply.Aplica(os, idNtc, Payload(l));
        });
        s.CheckRefuza("Api NTC: același Id de linie de două ori → refuz", () => {
            var l = new NtcLinieWriteDto {
                Id = lCompensare.Id, TipMaterialId = tipTrz.ID,
                ContDebitId = contClient.ID, ContCreditId = contFurnizor.ID, Valoare = 60m
            };
            NotaContabilaApply.Aplica(os, idNtc, Payload(l, l));
        });
        s.CheckRefuza("Api NTC: `TipMaterialId` absent din payload → refuz de DOMENIU („Tipul (contul/clasa) … "
            + "nu există sau nu e vizibil(ă)…”, fraza unică F22-D6), nu violare de FK NOT NULL "
            + "(singurul câmp fără rol pe notă, dar obligatoriu pe BAZĂ)", () => {
            var l = LinieValida(); l.TipMaterialId = Guid.Empty;
            NotaContabilaApply.Aplica(os, idNtc, Payload(l));
        });
        s.CheckRefuza("Api NTC: cont explicit inexistent → refuz de domeniu (rezolvarea pe navigație)", () => {
            var l = LinieValida(); l.ContDebitId = Guid.NewGuid();
            NotaContabilaApply.Aplica(os, idNtc, Payload(l));
        });
        s.CheckRefuza("Api NTC: repartitor de linie inexistent → refuz de domeniu", () => {
            var l = LinieValida(); l.RepartitorCreditId = Guid.NewGuid();
            NotaContabilaApply.Aplica(os, idNtc, Payload(l));
        });
        s.CheckRefuza("Api NTC: valoare în afara scării numeric(18,2) → refuz de domeniu, nu DbUpdateException", () => {
            var l = LinieValida(); l.Valoare = 5.001m;
            NotaContabilaApply.Aplica(s.OsCuGardian(), idNtc, Payload(l));
        });
        // Pe documentul EXISTENT, nu pe unul nou: refuzul cade ÎNAINTE de orice
        // atingere a header-ului, deci nota rămâne exact cum era. (Pe calea de
        // CREARE același refuz vine după `CreateObject`, adică lasă în
        // ObjectSpace-ul apelantului un document fără latură — pe host OS-ul e
        // per-cerere și moare cu ea, aici ar fi persistat de următorul commit.
        // Contractul e al apelantului, documentat în antetul lui `Aplica`.)
        s.CheckRefuza("Api NTC: latură inexistentă în nomenclatorul de repartitori → refuz de domeniu, înaintea "
            + "oricărei modificări a header-ului", () =>
            NotaContabilaApply.Aplica(os, idNtc, new NtcWriteDto {
                Data = dataNtc, PredatorId = Guid.NewGuid(), PrimitorId = unitate.ID,
                Linii = { LinieValida() }
            }));
        s.Check("Api NTC: …iar nota rămâne pe laturile ei (refuzul n-a rescris nimic)",
            NotaContabilaApply.Citeste(os, idNtc) is { PredatorId: var pId } && pId == unitate.ID);

        NotaContabilaApply.Aplica(os, idNtc, rescriere);
        s.Check("Api NTC: un Apply refuzat nu lasă reziduu în agregat — următorul payload valid readuce documentul la "
            + "exact trei linii, cu aceleași cifre (reconcilierea curăță liniile create înaintea refuzului)",
            NotaContabilaApply.Citeste(os, idNtc) is { Total: 75m } dupaRefuzuri
            && dupaRefuzuri.Linii.Count == 3);

        // --- Linia de tip BAZĂ (notă istorică/importată) ---
        var docIstoric = os.GetObjectByKey<NotaContabila>(idNtc);
        var linieBaza = os.CreateObject<DocumentDetaliu>();   // NU NotaContabilaDetaliu
        linieBaza.Document = docIstoric;
        linieBaza.TipMaterial = tipTrz;
        linieBaza.Valoare = 7m;
        os.CommitChanges();
        var idLinieBaza = linieBaza.ID;
        var citCuBaza = NotaContabilaApply.Citeste(os, idNtc);
        s.Check("Api NTC: citirea merge pe BAZA detaliului (as-cast la frunză) — linia de tip BAZĂ APARE, cu câmpurile "
            + "frunzei NULE, iar `Total` o numără la fel în agregat și în listă (82)",
            citCuBaza.Linii.Count == 4
            && citCuBaza.Linii.Single(l => l.Id == idLinieBaza)
                is { Descriere: null, ContDebitId: null, ContCreditId: null, CodEconomicId: null, Valoare: 7m }
            && citCuBaza.Total == 82m
            && NotaContabilaApply.Lista(os).Single(d => d.Id == idNtc).Total == 82m);
        s.CheckRefuza("Api NTC: Id-ul unei linii de tip BAZĂ în payload → refuz acționabil („ștergeți-o și culegeți-o din nou”)",
            () => {
                var l = LinieValida(); l.Id = idLinieBaza;
                NotaContabilaApply.Aplica(os, idNtc, Payload(l));
            });
        s.CheckRefuza("Api NTC: motorul refuză o notă cu linie de tip BAZĂ (n-are postare explicită — ar fi sărită mut)",
            () => ComenziDocument.Sistem(os).Opereaza(idNtc));
        NotaContabilaApply.Aplica(os, idNtc, rescriere);
        s.Check("Api NTC: linia absentă din payload se ȘTERGE (reconciliere server-side) — linia de bază dispare, "
            + "agregatul revine la 75",
            !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == idLinieBaza)
            && NotaContabilaApply.Citeste(os, idNtc) is { Total: 75m } dupaCuratenie
            && dupaCuratenie.Linii.Count == 3);

        // --- Refuzurile de OPERARE: fără rânduri-fantomă, fără serie consumată ---
        var serieInainte = SerieNtc();
        void RefuzOperare(string nume, Guid predatorId, Guid primitorId, NtcLinieWriteDto linie) {
            var id = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
                Data = dataNtc, PredatorId = predatorId, PrimitorId = primitorId, Linii = { linie }
            });
            s.Check(nume + " — dry-run-ul îl vede (fără să atingă nimic)", DryRunNtc(id).Count > 0);
            s.CheckRefuza(nume, () => ComenziDocument.Sistem(os).Opereaza(id));
            s.Check(nume + " — fără rânduri-fantomă și fără număr consumat (33d + GATE D6)",
                CubScena.FaraNote(os, id)
                && CubScena.FaraStoc(os, id)
                && os.GetObjectByKey<NotaContabila>(id).Stare == StareDocument.Draft
                && os.GetObjectByKey<NotaContabila>(id).Numar == null);
            NotaContabilaApply.Sterge(os, id);
        }

        RefuzOperare("Api NTC: latură `Partener` (nota are laturi INTERNE — partenerul stă pe contul liniei) → refuz",
            unitate.ID, partenerX.ID, LinieValida());
        var faraCont = LinieValida(); faraCont.ContCreditId = null;
        RefuzOperare("Api NTC: linie fără cont creditor → refuz (nota n-are reguli de contare care s-o salveze)",
            unitate.ID, unitate.ID, faraCont);
        var valoareZero = LinieValida(); valoareZero.Valoare = 0m;
        RefuzOperare("Api NTC: linie cu valoare 0 → refuz al TIPULUI (felia NU duplică regula — negativul rămâne permis)",
            unitate.ID, unitate.ID, valoareZero);
        var faraDefalcare = LinieValida();
        faraDefalcare.ContDebitId = contClient.ID;
        faraDefalcare.ContCreditId = contTranzit.ID;
        faraDefalcare.CodEconomicId = null;
        if (contClient.DimensiuniObligatorii.HasFlag(DimensiuneFlags.CodEconomic))
            RefuzOperare("Api NTC: cont cu defalcare obligatorie fără cod economic pe linie → refuzul gardianului "
                + "generic (33a: postarea explicită NU scutește linia de `DimensiuniObligatorii`)",
                unitate.ID, unitate.ID, faraDefalcare);
        else
            s.Check("Api NTC: profilul nu cere defalcare pe contul de furnizor — gardianul `DimensiuniObligatorii` "
                + "se probează pe profilul care o cere (bugetar)", true);

        s.Check("Api NTC: seria „NTC-” NU se consumă la refuz (F19-D6 + GATE D6: numărul se asignează abia la materializare)",
            SerieNtc() == serieInainte);

        // --- Dry-run, apoi comanda ---
        s.Check("Api NTC: dry-run (Valideaza) pe draftul valid → listă goală", DryRunNtc(idNtc).Count == 0);
        s.Check("Api NTC: dry-run-ul NU materializează nimic (Draft, fără număr, fără note)",
            os.GetObjectByKey<NotaContabila>(idNtc).Stare == StareDocument.Draft
            && os.GetObjectByKey<NotaContabila>(idNtc).Numar == null
            && CubScena.FaraNote(os, idNtc));

        var rez = ComenziDocument.Sistem(os).Opereaza(idNtc);
        cit = NotaContabilaApply.Citeste(os, idNtc);
        s.Check("Api NTC: Opereaza → Operat, cu număr din seria proprie (NTC-), fără conex și fără secundar; "
            + "affordances inversate",
            rez.StareNoua == StareDocument.Operat && rez.ConexId == null
            && cit.Numar?.StartsWith("NTC-") == true && cit.DataOperare != null
            && !cit.PoateEdita && !cit.PoateOpera && cit.PoateAnula && cit.PoateStorna
            && SerieNtc() == serieInainte + 1);
        s.Check("Api NTC: nota nu mișcă stoc și nu scrie jurnal de TVA (fără `PoliticaTva` în niciun profil — F19-D7)",
            CubScena.FaraStoc(os, idNtc)
            && CubScena.FaraFapte(os, idNtc));

        var noteCub = NoteNtcCub(idNtc);
        s.Check("ANCORA F19-D14 (NTC): operarea prin API postează pe POSTAREA EXPLICITĂ a liniei — câte un rând per linie, "
            + "cu conturile CULESE, în absența oricărei `RegulaContare` pe tip",
            noteCub.Nota(contClient.ID, contFurnizor.ID, null, lCompensare.Id)
            && noteCub.Nota(contClient.ID, contTranzit.ID, null, lPreluare.Id)
            && noteCub.Nota(contTranzit.ID, contClient.ID, null, lMinus.Id));
        s.Check("Api NTC: valorile se postează CA ATARE, inclusiv negativa (60 / 25 / −10), fără flag de storno (46a)",
            noteCub.Count(p => p.LinieId == lCompensare.Id && p.Valoare == 60m) == 2
            && noteCub.Count(p => p.LinieId == lPreluare.Id && p.Valoare == 25m) == 2
            && noteCub.Count(p => p.LinieId == lMinus.Id && p.Valoare == -10m) == 2
            && noteCub.All(p => p.Fel == N.FelTranzactie.Operare));

        s.Check("Api NTC: dimensiunile — repartitorul EXPLICIT al liniei e nivelul MAXIM al coalesce-ului, iar pe latura "
            + "necompletată cade default-ul polimorf al header-ului (32c) [cub]",
            noteCub.Any(p => p.Debit && p.LinieId == lCompensare.Id)
            && noteCub.Where(p => p.Debit && p.LinieId == lCompensare.Id).All(p => p.CodEconomic == codEc.ID));
        s.Check("D9-A10 Api NTC: repartitorul cules al liniei pe capătul lui (X / X; Y / implicitul primitor); linia fără el ia implicitul antetului",
            noteCub.Where(p => p.LinieId == lCompensare.Id).All(p => p.Repartitor == partenerX.ID)
            && noteCub.Single(p => p.Debit && p.LinieId == lPreluare.Id).Repartitor == partenerY.ID
            && noteCub.Single(p => p.Credit && p.LinieId == lPreluare.Id).Gestiune == unitate.ID
            && noteCub.Single(p => p.Debit && p.LinieId == lMinus.Id).Gestiune == unitate.ID);
        s.CheckRefuza("Api NTC: Apply peste o notă OPERATĂ → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
            () => NotaContabilaApply.Aplica(os, idNtc, rescriere));
        s.CheckRefuza("Api NTC: Sterge peste o notă OPERATĂ → același refuz de domeniu",
            () => NotaContabilaApply.Sterge(os, idNtc));

        // ---------------- Panoul de compensare (F19-D10) ----------------
        // Documentele de stins: două pe X (ca plafonul să aibă unde se consuma) și
        // unul pe Y. Trezoreria e scena NEUTRĂ față de profil (TRZ + contare din
        // laturi, cu fallback pe furnizor/client); compensarea „reală” FCT ↔ FCL e
        // deja probată de blocul de motor `E2E-CMP`.
        Document TrezorerieOperata(bool incasare, Repartitor partener, decimal valoare, DateOnly data) {
            DocumentTrezorerie doc = incasare ? os.CreateObject<Incasare>() : os.CreateObject<Plata>();
            doc.Data = data;
            doc.Predator = incasare ? partener : casa;
            doc.Primitor = incasare ? casa : partener;
            doc.TipInstrument = TipInstrumentPlata.Chitanta;
            var linie = os.CreateObject<DocumentTrezorerieDetaliu>();
            linie.Document = doc;
            linie.TipMaterial = tipTrz;
            linie.Valoare = valoare;
            linie.CodEconomicId = codEc.ID;   // conturile 5xx/401/411 bugetare cer defalcarea E
            os.CommitChanges();
            ComenziDocument.Sistem(os).Opereaza(doc.ID);
            return doc;
        }
        var incX1 = TrezorerieOperata(incasare: true, partenerX, 100m, new DateOnly(2026, 4, 10));
        var incX2 = TrezorerieOperata(incasare: true, partenerX, 100m, new DateOnly(2026, 4, 11));
        // Perechea de SENS a scenei (F19-D16): pe X există și o CREANȚĂ (plata =
        // avans dat), pe Y și o DATORIE (încasarea = avans primit), ca fiecare
        // jumătate de plafon să aibă candidați proprii și ca proba „aceeași notă
        // stinge 60 de datorie + 60 de creanță" să existe pe date, nu în text.
        var pltX = TrezorerieOperata(incasare: false, partenerX, 90m, new DateOnly(2026, 4, 12));
        var pltY = TrezorerieOperata(incasare: false, partenerY, 50m, new DateOnly(2026, 4, 12));
        var incY = TrezorerieOperata(incasare: true, partenerY, 30m, new DateOnly(2026, 4, 13));

        var capacitati = os.GetObjectByKey<NotaContabila>(idNtc).CapacitateStingere(os);
        s.Check("ANCORA F19-D16: plafonul notei e DEFALCAT PE SENS — X are 60 pe `Datorie` (repartitorul de pe DEBIT: "
            + "nota debitează 401, deci stinge ce datorăm) ȘI 60 pe `Creanta` (cel de pe CREDIT); Y doar 25 pe datorie. "
            + "Tavanul per contrapartidă rămâne 120 pe X — ce s-a schimbat e că fiecare jumătate se consumă pe latura ei",
            capacitati.Count == 2
            && capacitati[partenerX.ID] == new PlafonStingere(60m, 60m)
            && capacitati[partenerY.ID] == new PlafonStingere(25m, 0m));

        var cand = NotaContabilaApply.Candidati(os, idNtc);
        s.Check("ANCORA F19-D10 + D16: `candidati` întoarce O INTRARE PER (CONTRAPARTIDĂ × SENS) de pe linii (exact ce nu "
            + "putea `DocumenteCuRest`, care filtrează pe UNA singură), cu eticheta repartitorului — X pe amândouă "
            + "jumătățile, Y doar pe cea de datorie",
            cand != null && cand.Stare == "Operat" && cand.PoateStinge
            && cand.Contrapartide.Count == 3
            && cand.Contrapartide.Any(c => c.RepartitorId == partenerX.ID && c.Sens == "Datorie"
                && c.RepartitorCod == partenerX.Cod)
            && cand.Contrapartide.Any(c => c.RepartitorId == partenerX.ID && c.Sens == "Creanta")
            && cand.Contrapartide.Any(c => c.RepartitorId == partenerY.ID && c.Sens == "Datorie"));
        var candXd = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Datorie");
        var candXc = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Creanta");
        var candY = cand.Contrapartide.Single(c => c.RepartitorId == partenerY.ID);
        s.Check("ANCORA F19-D10 (cusătură MĂSURATĂ, nu afirmată): plafonul per (contrapartidă × sens) din `candidati` e "
            + "IDENTIC cu `NotaContabila.CapacitateStingere` — X 60 datorie / 60 creanță, Y 25 datorie",
            cand.Contrapartide.All(c => capacitati[c.RepartitorId][Enum.Parse<SensStingere>(c.Sens)] == c.Capacitate)
            && candXd.Capacitate == 60m && candXc.Capacitate == 60m && candY.Capacitate == 25m
            && candXd.Asignat == 0m && candXd.Disponibil == 60m);

        List<DocumentCuRestRand> RestPentru(Guid contrapartidaId, SensStingere sens) =>
            ImperecheriProiectii.DocumenteCuRest(os, contrapartidaId, sens)
                .OrderBy(r => r.Data).ThenBy(r => r.Numar).ToList();
        bool Aceleasi(List<DocumentCuRestRand> a, List<DocumentCuRestRand> b) =>
            a.Count == b.Count && a.Zip(b).All(p =>
                p.First.DocumentId == p.Second.DocumentId && p.First.Tip == p.Second.Tip
                && p.First.ContrapartidaId == p.Second.ContrapartidaId && p.First.Sens == p.Second.Sens
                && p.First.Total == p.Second.Total && p.First.Asignat == p.Second.Asignat
                && p.First.Rest == p.Second.Rest);
        s.Check("ANCORA F19-D10 + D16: rândurile unei jumătăți sunt EXACT `DocumenteCuRest` filtrat pe (contrapartidă × "
            + "sens) — încasările lui X (avansuri primite = datorii) sub jumătatea de DEBIT, plata către X (avans dat = "
            + "creanță) sub cea de CREDIT; niciun document nu apare sub ambele, iar documentele lui Y nu apar la X",
            Aceleasi(candXd.Candidati, RestPentru(partenerX.ID, SensStingere.Datorie))
            && Aceleasi(candXc.Candidati, RestPentru(partenerX.ID, SensStingere.Creanta))
            && Aceleasi(candY.Candidati, RestPentru(partenerY.ID, SensStingere.Datorie))
            && candXd.Candidati.Select(r => r.DocumentId).OrderBy(x => x)
                .SequenceEqual(new[] { incX1.ID, incX2.ID }.OrderBy(x => x))
            && candXc.Candidati.Single().DocumentId == pltX.ID
            && candY.Candidati.Single().DocumentId == incY.ID
            && !candY.Candidati.Any(r => r.DocumentId == pltY.ID)
            && !candXd.MaiSunt && !candXc.MaiSunt && !candY.MaiSunt);
        var totRest = ImperecheriProiectii.DocumenteCuRest(os).ToList();
        s.Check("F19-D16 (42c): literalul `Sens` al proiecției == `Document.SensDeStins` pe FIECARE rând — proiecția "
            + "dublează un calcul al motorului (funcție de TIP, netraductibilă în SQL), deci se MĂSOARĂ contra lui",
            totRest.Count > 0
            && totRest.All(r => r.Sens == os.GetObjectByKey<Document>(r.DocumentId).SensDeStins(os).ToString()));
        s.Check("F19-D10: NTC NU se adaugă în `DocumenteCuRest` (nota n-are semantică de „rest”: Σ liniilor ei nu e nici "
            + "creanță, nici datorie) — deci nu se poate propune pe ea însăși",
            !totRest.Any(r => r.DocumentId == idNtc));

        // ═══ A doua axă a lui F19-D16: CARE contrapartidă se taxează ═══
        // Ipoteza contractului (citită din cod la pasul 2, NEMĂSURATĂ atunci): regula
        // veche alegea „primul key din `capacitati` care se potrivește cu o latură a
        // documentului stins", iar cheile se umplu debit-întâi, în ordinea liniilor
        // ⇒ un document care poartă DOUĂ dintre contrapartidele notei pe cele două
        // laturi ale lui era taxat pe plafonul altui grup decât cel sub care panoul
        // l-a afișat. Scena: nota poartă CASA (60) și partenerul X (5); încasarea
        // `incX1` are predator X și primitor CASA, deci le poartă pe amândouă.
        var idAxa2 = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = {
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = casa.ID, CodEconomicId = codEc.ID, Valoare = 60m },
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerX.ID, CodEconomicId = codEc.ID, Valoare = 5m }
            }
        });
        ComenziDocument.Sistem(os).Opereaza(idAxa2);
        var notaAxa2 = os.GetObjectByKey<NotaContabila>(idAxa2);
        var capAxa2 = notaAxa2.CapacitateStingere(os);
        var cheiAxa2 = capAxa2.Keys.ToList();
        s.Check("MĂSURARE F19-D16 (a doua axă) — PREMISA: nota poartă DOUĂ contrapartide, în ordinea liniilor "
            + "(debit-întâi): casa 60,00 pe datorie și partenerul X 5,00 pe datorie; iar încasarea `incX1` le poartă pe "
            + "AMÂNDOUĂ (predator = X, primitor = casa). Configurația e realizabilă: `CapacitateStingere` ia ORICE "
            + "repartitor de pe linie, nu doar parteneri",
            cheiAxa2.Count == 2 && cheiAxa2[0] == casa.ID && cheiAxa2[1] == partenerX.ID
            && capAxa2[casa.ID].Datorie == 60m && capAxa2[partenerX.ID].Datorie == 5m
            && incX1.PredatorId == partenerX.ID && incX1.PrimitorId == casa.ID);
        // Regula ȘTEARSĂ din `ValideazaCreare`, re-executată aici pe aceeași scenă și
        // pe același dicționar: e o funcție deterministă de (ordinea cheilor ×
        // laturile documentului), ambele asertate mai sus.
        Guid primulKeyVechi = Guid.Empty;
        foreach (var candidat in capAxa2.Keys)
            if (candidat == incX1.PredatorId || candidat == incX1.PrimitorId) {
                primulKeyVechi = candidat;
                break;
            }
        Console.WriteLine("     MĂSURAT (F19-D16, a doua axă) — IPOTEZA CONFIRMATĂ: regula veche („primul key care se "
            + "potrivește cu o latură”) alege CASA, cu plafon 60,00, deși panoul afișează încasarea sub partenerul X, "
            + "cu disponibil 5,00. O stingere de 50,00 ar fi trecut pe plafonul altui grup decât cel afișat.");
        s.Check("MĂSURAT (F19-D16, a doua axă): regula veche alegea CASA (primul key, plafon 60,00), NU contrapartida sub "
            + "care panoul chiar afișează documentul (X, plafon 5,00) — `DocumenteCuRest` pune încasarea pe predatorul "
            + "ei (X) și pe nimeni altcineva",
            primulKeyVechi == casa.ID && capAxa2[casa.ID].Datorie == 60m && capAxa2[partenerX.ID].Datorie == 5m
            && ImperecheriProiectii.DocumenteCuRest(os, partenerX.ID, SensStingere.Datorie)
                .Any(r => r.DocumentId == incX1.ID)
            && !ImperecheriProiectii.DocumenteCuRest(os, casa.ID).Any(r => r.DocumentId == incX1.ID));
        s.CheckRefuza("F19-D16 (a doua axă, ÎNCHISĂ): documentul stins poartă DOUĂ dintre contrapartidele notei, pe laturi "
            + "diferite ⇒ deducția REFUZĂ zgomotos în loc să aleagă „primul key”",
            () => ImperechereService.Imperecheaza(os, notaAxa2, os.GetObjectByKey<Document>(incX1.ID), 50m));
        s.CheckRefuza("F19-D16 (a doua axă): cu contrapartida CERUTĂ explicit (X — grupul sub care panoul l-a afișat) se "
            + "aplică plafonul LUI: 50,00 > 5,00 se refuză",
            () => ImperechereService.Imperecheaza(os, notaAxa2, os.GetObjectByKey<Document>(incX1.ID), 50m,
                partenerX.ID));
        s.CheckRefuza("F19-D16 (a doua axă): aceeași ambiguitate pe calea REST (`ImperechereApply.Creeaza`) — refuz de "
            + "domeniu, nu o alegere tăcută",
            () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                DocumentStingatorId = idAxa2, DocumentId = incX1.ID, Suma = 5m }));
        var impAxa2 = ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idAxa2, DocumentId = incX1.ID, Suma = 5m, ContrapartidaId = partenerX.ID });
        s.Check("F19-D16 (a doua axă): exact plafonul lui X (5,00) se acceptă pe contrapartida cerută; casa rămâne cu "
            + "55,00 disponibil — `AsignatFataDe` numără o stingere contra ORICĂREI contrapartide de pe documentul "
            + "stins (semantică preexistentă, conservatoare: nu deschide plafon)",
            impAxa2.Suma == 5m
            && ImperechereService.AsignatFataDe(os, idAxa2, partenerX.ID, SensStingere.Datorie) == 5m
            && capAxa2[casa.ID].Datorie - ImperechereService.AsignatFataDe(os, idAxa2, casa.ID, SensStingere.Datorie)
                == 60m);
        foreach (var idStingere in os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == idAxa2).Select(i => i.ID).ToArray())
            ImperechereService.Sterge(os, idStingere);
        s.Check("F19-D16 (a doua axă): scena revine la zero — încasarea își recapătă restul întreg (100,00)",
            ImperechereService.Ramas(os, incX1.ID) == 95m);
        ComenziDocument.Sistem(os).AnuleazaOperarea(idAxa2);

        // ═══ Proba de fond a lui F19-D16: 60 nu mai stinge 120 de aceeași natură ═══
        // Defectul măsurat la pasul 2 al feliei: nota `401 = 4111` de 60,00 pe X avea
        // plafon 120,00 și îl consuma INTEGRAL pe două ÎNCASĂRI — documente de
        // aceeași natură (80,00 + 40,00). Tavanul de 120 era corect (60 pe datorie +
        // 60 pe creanță = cele două stingeri legitime); lipsea regula că fiecare
        // jumătate se consumă pe latura ei.
        s.CheckRefuza("PROBA F19-D16: 80,00 pe o încasare (= datorie) depășește jumătatea de DEBIT a notei (60,00), deși "
            + "plafonul TOTAL al contrapartidei e tot 120,00 — înainte de F19-D16 trecea",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
                os.GetObjectByKey<Document>(incX1.ID), 80m));
        ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
            os.GetObjectByKey<Document>(incX1.ID), 60m);
        cand = NotaContabilaApply.Candidati(os, idNtc);
        candXd = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Datorie");
        candXc = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Creanta");
        s.Check("ANCORA F19-D14 (NTC): stingerea prin serviciu scade `Rest`-ul documentului stins (100 → 40) ȘI se vede "
            + "în panou ca `Asignat` PE JUMĂTATEA EI — cifra vine din `ImperechereService.AsignatFataDe(cp, sens)`, "
            + "funcția pe care o cheamă și `ValideazaCreare` (o singură formulă a plafonului); jumătatea de creanță "
            + "rămâne INTACTĂ",
            ImperechereService.Ramas(os, incX1.ID) == 40m
            && candXd.Asignat == 60m
            && candXd.Asignat == ImperechereService.AsignatFataDe(os, idNtc, partenerX.ID, SensStingere.Datorie)
            && candXd.Disponibil == 0m
            && candXc.Asignat == 0m && candXc.Disponibil == 60m
            && candXd.Candidati.Count == 0);
        s.CheckRefuza("PROBA F19-D16 (defectul de la pasul 2, ÎNCHIS): jumătatea de datorie e consumată — a doua încasare "
            + "nu mai primește niciun ban. Înainte lua încă 40,00 și o compensare de 60,00 stingea 120,00 pe aceeași "
            + "latură economică",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
                os.GetObjectByKey<Document>(incX2.ID), 0.01m));
        s.CheckRefuza("ANCORA F19-D10 (riscul 2, capătul „panoul nu promite mai mult decât acceptă serviciul”): un ban "
            + "PESTE `Disponibil` al jumătății de creanță (60,01) → refuz al `ValideazaCreare`",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
                os.GetObjectByKey<Document>(pltX.ID), 60.01m));
        ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
            os.GetObjectByKey<Document>(pltX.ID), 60m);
        cand = NotaContabilaApply.Candidati(os, idNtc);
        candXd = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Datorie");
        candXc = cand.Contrapartide.Single(c => c.RepartitorId == partenerX.ID && c.Sens == "Creanta");
        s.Check("PROBA F19-D16 (cazul LEGITIM rămâne): ACEEAȘI notă de 60,00 stinge 60,00 de DATORIE (încasarea) + 60,00 "
            + "de CREANȚĂ (plata) — 120,00 în total, exact tavanul, dar fiecare jumătate pe latura ei; ambele "
            + "`Disponibil` ajung la 0",
            os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentStingatorId == idNtc) == 2
            && os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == idNtc).Sum(i => i.Suma) == 120m
            && candXd.Disponibil == 0m && candXc.Disponibil == 0m
            && ImperechereService.Ramas(os, pltX.ID) == 30m);
        s.CheckRefuza("Api NTC: plafonul consumat integral pe AMBELE jumătăți → orice altă stingere pe aceeași "
            + "contrapartidă se refuză",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtc),
                os.GetObjectByKey<Document>(incX2.ID), 0.01m));
        cit = NotaContabilaApply.Citeste(os, idNtc);
        s.Check("Api NTC: affordance ONEST pe stingeri — `PoateAnula`/`PoateStorna` FALSE cât există imperecheri (57d), "
            + "iar gardianul motorului confirmă",
            !cit.PoateAnula && !cit.PoateStorna);
        s.CheckRefuza("Api NTC: anularea unei note care STINGE se refuză (gardianul acoperă coloana DocumentStingator)",
            () => ComenziDocument.Sistem(os).AnuleazaOperarea(idNtc));
        foreach (var idStingere in os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == idNtc).Select(i => i.ID).ToArray())
            ImperechereService.Sterge(os, idStingere);
        cit = NotaContabilaApply.Citeste(os, idNtc);
        s.Check("Api NTC: după ștergerea link-urilor (31d: se șterg liber) affordance-ele revin, iar panoul arată din nou "
            + "plafonul întreg — pe FIECARE jumătate (60 datorie + 60 creanță)",
            cit.PoateAnula && cit.PoateStorna
            && NotaContabilaApply.Candidati(os, idNtc).Contrapartide
                .Where(c => c.RepartitorId == partenerX.ID)
                .All(c => c is { Asignat: 0m, Disponibil: 60m })
            && NotaContabilaApply.Candidati(os, idNtc).Contrapartide
                .Count(c => c.RepartitorId == partenerX.ID) == 2);

        // ═══ Review O2, MĂSURAT (restanță DECLARATĂ, nefixată) ═══
        // `AsignatFataDe` adaugă `caStins` — cât s-a stins PE stingătorul însuși —
        // la AMBELE sensuri, necondiționat. Pe un plafon cu două jumătăți asta
        // consumă de două ori. Review-ul l-a considerat inaccesibil pe motiv că
        // „laturile notei sunt unități interne, deci nota nu poate fi stinsă de
        // nimic". Trezoreria chiar nu ajunge acolo — `Plata` cere primitor
        // partener/angajat/cont propriu, deci o unitate internă nu poate fi
        // contrapartida ei (probat: operarea unei plăți către `unitate` e REFUZATĂ).
        // Dar o ALTĂ NOTĂ ajunge: `CapacitateStingere` ia ORICE repartitor de pe
        // linie, inclusiv unitatea internă (aceeași proprietate pe care se sprijină
        // și măsurarea „a doua axă" de mai sus).
        // Verdictul „nu e atacabil" RĂMÂNE, dar din alt motiv: efectul e
        // CONSERVATOR — închide plafon (60,00 disponibil în loc de 90,00), nu
        // deschide. Se măsoară ca să nu mai fie o afirmație; fixul e restanță.
        s.CheckRefuza("Review O2 (premisa corectată): trezoreria NU poate stinge o notă — `Plata` cere primitor "
            + "partener/angajat/cont propriu, deci o unitate internă nu e niciodată contrapartida ei",
            () => TrezorerieOperata(incasare: false, unitate, 30m, new DateOnly(2026, 4, 14)));
        var idStingeNota = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = { new NtcLinieWriteDto {
                TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                RepartitorDebitId = unitate.ID, CodEconomicId = codEc.ID, Valoare = 30m } }
        });
        ComenziDocument.Sistem(os).Opereaza(idStingeNota);
        s.CheckRefuza("101: nota cu repartitor intern nu consumă partidele partenerului", () =>
            ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idStingeNota),
                os.GetObjectByKey<Document>(idNtc), 30m));
        s.Check("101: refuzul păstrează ambele disponibiluri fără dublă scădere",
            ImperechereService.AsignatFataDe(os, idNtc, partenerX.ID, SensStingere.Datorie) == 0m
            && ImperechereService.AsignatFataDe(os, idNtc, partenerX.ID, SensStingere.Creanta) == 0m);

        // ═══ F19-D16 (F2): schimbarea de comportament a trezoreriei, DECLARATĂ ═══
        // Contractul pin-uise inițial „pentru documentele cu o SINGURĂ contrapartidă
        // (toată trezoreria + DEC) comportamentul rămâne IDENTIC". Nu rămâne, iar
        // review-ul a găsit-o pe HTTP fiindcă suita n-avea niciun check pe perechile
        // acelea: plafonul trezoreriei e pe UN SINGUR sens (`SensPropriu().Opus()`),
        // deci perechea cu sensuri nepotrivite cade pe `perechi.Count == 0` — deși
        // ambele documente au o singură contrapartidă, iar regula veche o accepta pe
        // plafonul total. Verdictul e CORECT contabil (a credita 401 nu stinge o
        // factură de furnizor); vechea permisivitate era o gaură din aceeași
        // familie. Cazul real pe Flax: PLT `SED00000097-1` → FCL `FLAX000161444`,
        // 700,00, 23.04.2025 — SINGURA imperechere PLT→FCL din cele 46.056 ale
        // importului, fără niciun gard mai vechi care s-o fi oprit (`Plata↔Plata` e
        // refuzată separat; `Plata↔FacturaIesire` nu era).
        // Aici se măsoară MECANISMUL; perechile ca atare (ce trece / ce se refuză,
        // pe documente reale cu aceeași contrapartidă) sunt în blocul `E2E-CMP`.
        var pltProba = os.GetObjectByKey<Plata>(pltX.ID);
        var fclProba = os.CreateObject<FacturaIesire>();
        s.Check("F19-D16 (F2, mecanismul schimbării declarate): `Plata.CapacitateStingere` are TOT plafonul pe "
            + "`Datorie` și 0,00 pe `Creanta`, iar `FacturaIesire.SensDeStins` = `Creanta` — deci orice PLT→FCL cade "
            + "pe refuzul „n-are capacitate pe sensul cerut”, deși e trezorerie cu o SINGURĂ contrapartidă",
            pltProba.CapacitateStingere(os)[partenerX.ID] == new PlafonStingere(90m, 0m)
            && fclProba.SensDeStins(os) == SensStingere.Creanta
            && pltProba.SensDeStins(os) == SensStingere.Creanta);
        os.Delete(fclProba);
        os.CommitChanges();
        // ═══ F19-D16 corectată de review (F1): plafonul e MIȘCAREA NETĂ ═══
        // Prima versiune suma `Σ |Valoare|` per (repartitor × sens) și răsturna
        // sensul per LINIE negativă — deci o pereche +v/−v pe ACEEAȘI latură a
        // aceluiași repartitor producea (Datorie v, Creanta v) deși mișcarea NETĂ pe
        // partener era ZERO: exact defectul "o compensare de 60 stinge 120", mutat
        // de pe axa laturilor pe axa semnelor. Forma e dominantă pe Flax 2025: 899
        // din 1.478 de chei (repartitor × latură) cu net 0,00 și brut > 0 — 10,5
        // mil. lei de capacitate fantomă, cu candidați reali dedesubt.
        // Formula de acum: `|Σ semnat|` per (repartitor × LATURĂ), semnul netului
        // alege sensul, iar cheia cu net 0 nu intră deloc în dicționar.
        var idNtcNetZero = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = {
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerX.ID, CodEconomicId = codEc.ID, Valoare = 40m },
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerX.ID, CodEconomicId = codEc.ID, Valoare = -40m }
            }
        });
        ComenziDocument.Sistem(os).Opereaza(idNtcNetZero);
        var capNetZero = os.GetObjectByKey<NotaContabila>(idNtcNetZero).CapacitateStingere(os);
        var netPeX = os.GetObjectsQuery<NotaContabilaDetaliu>()
            .Where(d => d.DocumentId == idNtcNetZero && d.RepartitorDebitId == partenerX.ID)
            .Sum(d => d.Valoare);
        var brutPeX = os.GetObjectsQuery<NotaContabilaDetaliu>()
            .Where(d => d.DocumentId == idNtcNetZero && d.RepartitorDebitId == partenerX.ID)
            .Sum(d => Math.Abs(d.Valoare));
        Console.WriteLine($"     ANCORA F19-D16 (F1) — nota cu perechea +40/-40 pe DEBITUL lui X: brut {brutPeX:N2}, "
            + $"miscare neta {netPeX:N2} => plafon Datorie "
            + $"{capNetZero.GetValueOrDefault(partenerX.ID).Datorie:N2} / Creanta "
            + $"{capNetZero.GetValueOrDefault(partenerX.ID).Creanta:N2}");
        s.Check("ANCORA F19-D16 (F1, proba de fond): nota cu BRUT 80,00 și mișcare NETĂ 0,00 pe X are plafon ZERO pe "
            + "AMBELE sensuri — X nici măcar nu apare printre contrapartidele notei (cheia cu net 0 nu intră în "
            + "dicționar). Prima versiune a lui F19-D16 dădea „Datorie 40,00 / Creanța 40,00”",
            brutPeX == 80m && netPeX == 0m
            && !capNetZero.ContainsKey(partenerX.ID)
            && capNetZero.GetValueOrDefault(partenerX.ID) == new PlafonStingere(0m, 0m));
        var candNetZero = NotaContabilaApply.Candidati(os, idNtcNetZero);
        s.Check("ANCORA F19-D16 (F1): panoul `candidati` nu mai PROPUNE nicio jumătate fantomă — zero grupuri pe X, deși "
            + "documentele lui X sunt acolo, cu rest; operatorul nu mai vede butoane „Stinge” pentru o notă care n-a "
            + "mișcat nimic pe partener",
            candNetZero.PoateStinge && !candNetZero.Contrapartide.Any(c => c.RepartitorId == partenerX.ID));
        s.CheckRefuza("ANCORA F19-D16 (F1, cifra): serviciul REFUZĂ acum stingerea unei încasări a lui X — înainte "
            + "accepta 40,00 pe jumătatea de datorie…",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtcNetZero),
                os.GetObjectByKey<Document>(incX2.ID), 40m));
        s.CheckRefuza("ANCORA F19-D16 (F1, cifra): …și 40,00 pe cea de creanță (plata lui X). Cele 80,00 pe care o notă "
            + "cu mișcare 0,00 pe X le stingea nu se mai împart în două jumătăți, ci dispar",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idNtcNetZero),
                os.GetObjectByKey<Document>(pltX.ID), 40m));
        s.Check("ANCORA F19-D16 (F1): nicio imperechere n-a fost creată, deci resturile documentelor lui X rămân "
            + "NEATINSE (100,00 pe a doua încasare, 90,00 pe plată)",
            !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == idNtcNetZero)
            && ImperechereService.Ramas(os, incX2.ID) == 100m
            && ImperechereService.Ramas(os, pltX.ID) == 90m);

        // Netarea nu MUTĂ tratarea liniei negative, o SUBSUMEAZĂ: nu mai există
        // răsturnare per LINIE (`Math.Abs` pe fiecare valoare), doar semnul NETULUI.
        var idNtcNetNegativ = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = {
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerY.ID, CodEconomicId = codEc.ID, Valoare = -40m },
                new NtcLinieWriteDto {
                    TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                    RepartitorDebitId = partenerY.ID, CodEconomicId = codEc.ID, Valoare = -40m }
            }
        });
        ComenziDocument.Sistem(os).Opereaza(idNtcNetNegativ);
        s.Check("ANCORA F19-D16 (F1): două linii de −40,00 pe DEBITUL lui Y dau net −80,00, deci plafon 80,00 pe "
            + "`Creanta` — un debit de −80 e economic un credit de 80. Latura o răstoarnă semnul NETULUI, nu semnul "
            + "fiecărei linii: tratarea liniei negative e o consecință a netării, nu un caz special",
            os.GetObjectByKey<NotaContabila>(idNtcNetNegativ).CapacitateStingere(os)[partenerY.ID]
                == new PlafonStingere(0m, 80m));
        s.Check("ANCORA F19-D16 (F1): …iar panoul urmează plafonul — un singur grup pe Y, pe `Creanta`",
            NotaContabilaApply.Candidati(os, idNtcNetNegativ).Contrapartide
                .Single(c => c.RepartitorId == partenerY.ID) is { Sens: "Creanta", Capacitate: 80m });

        // Panoul pe un DRAFT: nota nu stinge încă, deci nu întoarce NIMIC de stins
        // (review M3). Capacitățile EXISTĂ (sunt ale liniilor, care sunt culese pe
        // draft), dar o afordanță nu contrazice datele pe care le însoțește: un
        // panou complet de plafoane și candidați lângă `PoateStinge: false` ar fi
        // „panoul promite mai mult decât acceptă serviciul" (riscul 2), pe axa
        // STĂRII. `Stare`/`PoateStinge` rămân — ele spun DE CE e gol.
        var idDraft = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = { new NtcLinieWriteDto {
                TipMaterialId = tipTrz.ID, ContDebitId = contClient.ID, ContCreditId = contTranzit.ID,
                RepartitorDebitId = partenerY.ID, CodEconomicId = codEc.ID, Valoare = 12m } }
        });
        s.Check("Api NTC (review M3): `candidati` pe un DRAFT — `PoateStinge` false ȘI lista de contrapartide GOALĂ, "
            + "deși capacitățile liniilor există (12,00 pe Y): panoul nu promite plafoane pe care serviciul le refuză "
            + "din primul invariant al stingerii (ambele documente operate)",
            NotaContabilaApply.Candidati(os, idDraft) is { Stare: "Draft", PoateStinge: false } candDraft
            && candDraft.Contrapartide.Count == 0
            && os.GetObjectByKey<NotaContabila>(idDraft).CapacitateStingere(os)[partenerY.ID]
                == new PlafonStingere(12m, 0m));
        s.CheckRefuza("Api NTC: și serviciul refuză stingerea de pe un draft (nota NEOPERATĂ nu stinge)",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idDraft),
                os.GetObjectByKey<Document>(pltY.ID), 5m));
        NotaContabilaApply.Sterge(os, idDraft);
        s.Check("Api NTC: Sterge pe draft — documentul și liniile lui dispar împreună",
            NotaContabilaApply.Citeste(os, idDraft) == null
            && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idDraft));

        // Nota FĂRĂ repartitori pe linii nu stinge nimic (dicționar gol → refuz):
        // panoul o spune, iar serviciul o confirmă.
        var idFaraContrapartida = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
            Data = dataNtc, PredatorId = unitate.ID, PrimitorId = unitate.ID,
            Linii = { new NtcLinieWriteDto {
                TipMaterialId = tipTrz.ID, ContDebitId = contTranzit.ID, ContCreditId = contClient.ID,
                CodEconomicId = codEc.ID, Valoare = 9m } }
        });
        ComenziDocument.Sistem(os).Opereaza(idFaraContrapartida);
        s.Check("Api NTC: nota fără repartitori pe linii n-are nicio contrapartidă — panoul întoarce lista GOALĂ "
            + "(48b: fără contrapartide explicite nota nu stinge nimic)",
            NotaContabilaApply.Candidati(os, idFaraContrapartida) is { PoateStinge: true } candGol
            && candGol.Contrapartide.Count == 0);
        s.CheckRefuza("Api NTC: …iar serviciul refuză explicit („documentul care stinge nu poartă nicio contrapartidă”)",
            () => ImperechereService.Imperecheaza(os, os.GetObjectByKey<NotaContabila>(idFaraContrapartida),
                os.GetObjectByKey<Document>(pltY.ID), 5m));

        // --- Anulare, re-operare, storno ---
        // FIFO-ul pe cub leagă notele ulterioare de partidele primei note,
        // inclusiv nota cu total zero, ale cărei linii ating partide diferite.
        if (privat) {
            using var osRefuz = s.Provider.CreateObjectSpace();
            var refuzDependenti = false;
            try { ComenziDocument.Sistem(osRefuz).AnuleazaOperarea(idNtc); }
            catch (OperareException e) { refuzDependenti = e.Message.Contains("PARTIDA_CU_DEPENDENTI"); }
            s.Check("Api NTC: anularea sursei refuzată cât timp notele FIFO depind de ea", refuzDependenti);
        }
        foreach (var dependent in new[] { idNtcNetNegativ, idNtcNetZero }) {
            using var osDependent = s.Provider.CreateObjectSpace();
            ComenziDocument.Sistem(osDependent).AnuleazaOperarea(dependent);
        }
        var numarNtc = cit.Numar;
        s.Check("Api NTC: anulare prin API → Draft + notele șterse",
            ComenziDocument.Sistem(os).AnuleazaOperarea(idNtc).StareNoua == StareDocument.Draft
            && CubScena.FaraNote(os, idNtc));
        ComenziDocument.Sistem(os).Opereaza(idNtc);
        s.Check("Api NTC: re-operare după anulare — același număr, aceleași trei rânduri (valorile nu se re-semnează)",
            NotaContabilaApply.Citeste(os, idNtc) is { Total: 75m } dupaReoperareCub
            && dupaReoperareCub.Numar == numarNtc
            && NoteNtcCub(idNtc).Where(p => p.Debit).Sum(p => p.Valoare) == 75m
            && NoteNtcCub(idNtc).Where(p => p.Credit).Sum(p => p.Valoare) == 75m);
        s.Check("Api NTC: storno prin API → Stornat, rânduri inverse append-only la data stornării (−60 / −25 / +10 — "
            + "negativa devine pozitivă)",
            ComenziDocument.Sistem(os).Storneaza(idNtc, new DateOnly(2026, 7, 23)).StareNoua == StareDocument.Stornat
            && CubScena.Note(os, idNtc).Count(p => p.Storno && p.Debit
                && p.Data == new DateOnly(2026, 7, 23)
                && (p.Valoare == -60m || p.Valoare == -25m || p.Valoare == 10m)) == 3
            && CubScena.Note(os, idNtc).Count(p => p.Storno && p.Credit
                && p.Data == new DateOnly(2026, 7, 23)
                && (p.Valoare == -60m || p.Valoare == -25m || p.Valoare == 10m)) == 3);

        Curata();
        s.Check("Api NTC: curățenie finală (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(Marcaj))
            && os.GetObjectByKey<NotaContabila>(idNtc) == null);
    }
}
