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

// ================= Felia API RDC (F19, track 3) — E2E-API-RDC =================
// Returul de la client parcurs prin CONTRACTUL feliei: WriteDto →
// `ReturClientApply.Aplica` → `Citeste`/`Lista` → dry-run → `ComenziDocument`.
// Endpoint-urile din host sunt transport peste EXACT acest cod.
//
// Rulează DOAR pe profilul PRIVAT (F19-D14): politicile RDC există doar acolo.
//
// Ce exersează, în plus față de blocul de MOTOR (`E2E-RET`):
//   * RISCUL 5, pin-uit: rolul liniei e o PREZENȚĂ (`LotId`), nu un enum — un PUT
//     care mută o linie dintr-un rol în celălalt e REFUZ EXPLICIT, în ambele
//     sensuri, iar calea legitimă (ștergere + re-culegere) rămâne deschisă;
//   * F19-D7: linia de COST persistată cu `TipTvaId = null` — probat ÎN BAZĂ, pe
//     un ObjectSpace proaspăt, nu doar în ReadDto;
//   * F19-D9: `Total` == DOAR liniile de venit, `TotalCost` separat, pe agregat ȘI
//     pe listă;
//   * riscul 6: idempotența semnării prin calea API (anulare → PUT → re-operare).
static class VerificaApiRdc {
    public static void Ruleaza(Suita s) {
        const string Marcaj = "E2E-API-RDC";
        using var os = s.Provider.CreateObjectSpace();

        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tip301 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "301");   // alt Tip de stoc (coerență)
        var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");   // venit din vânzarea mărfurilor
        var tipRdc = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");   // Capitalizat
        var cont371 = os.FirstOrDefault<Cont>(c => c.Simbol == "371");
        var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
        var cont4427 = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
        var cont607 = os.FirstOrDefault<Cont>(c => c.Simbol == "607");
        var cont707 = os.FirstOrDefault<Cont>(c => c.Simbol == "707");

        var reguliRdc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRdc.ID).ToList();
        var tipStoc = (reguliRdc.FirstOrDefault(r => r.ClasaId == tip371.ClasaId)
            ?? reguliRdc.First(r => r.ClasaId == null)).TipStoc;

        void Curata() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var loturi = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => loturi.Contains(r.LotId) || (r.DocumentId != null && docIds.Contains(r.DocumentId.Value))).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => docIds.Contains(d.ID)).ToList()
                         .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }
        Curata();

        s.Check("Api RDC — precondiții de profil: ancora RDC cu seria „RDC-”, reguli de stoc −1 pe PRIMITOR (marfa "
            + "REVINE pe lotul original) și `PoliticaTva` COLECTAT cu `TipTvaImplicit = N21`",
            tipRdc != null && tipRdc.ClrType == nameof(ReturClient)
            && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRdc.ID)?.Serie == "RDC-"
            && reguliRdc.Count > 0 && reguliRdc.All(r => r.Latura == LaturaDocument.Primitor && r.Semn == -1)
            && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRdc.ID)?.Directie == DirectieTva.Colectat
            && tipRdc.TipTvaImplicitId == n21.ID
            && tip371 != null && tip301 != null && tip707 != null && ned21 != null);

        // ---------------- Scena ----------------
        var gest = os.CreateObject<Gestiune>();
        gest.Cod = Marcaj + "-G"; gest.Denumire = "Gestiune Api RDC";
        var client = os.CreateObject<Partener>();
        client.Cod = Marcaj + "-CL"; client.Denumire = "Client Api RDC"; client.CodFiscal = "RO44444449";
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-F"; furnizor.Denumire = "Furnizor Api RDC"; furnizor.CodFiscal = "RO44444450";
        var produsA = os.CreateObject<Produs>();
        produsA.Cod = Marcaj + "-A"; produsA.Denumire = "Marfă Api RDC"; produsA.UM = "BUC";
        produsA.TipMaterial = tip371;
        os.CommitChanges();

        var dataLot = new DateOnly(2026, 10, 1);
        var dataRdc = new DateOnly(2026, 10, 10);

        var nir = os.CreateObject<NIR>();
        nir.Data = dataLot; nir.Predator = furnizor; nir.Primitor = gest;
        var linNir = os.CreateObject<DocumentDetaliu>();
        linNir.Document = nir; linNir.TipMaterial = tip371; linNir.Cantitate = 10m; linNir.Valoare = 100m;
        var lot = linNir.CreeazaLot(os, produsA, gest);
        os.CommitChanges();
        MotorOperare.Opereaza(os, nir);
        os.CommitChanges();

        (decimal Cantitate, decimal Valoare) SoldCheieCub() => CubScena.SoldCheie(os, lot.ID, gest.ID);
        List<PostareScena> NoteCub(Guid docId) => CubScena.Note(os, docId).Where(p => !p.Storno).ToList();
        IReadOnlyList<string> DryRun(Guid docId) {
            using var osDry = s.Provider.CreateObjectSpace();
            return ComenziDocument.Sistem(osDry).Valideaza(docId);
        }
        int SerieRdc() {
            using var o = s.Provider.CreateObjectSpace();
            return o.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "RDC").UrmatorulNumar;
        }
        // Proba „ÎN BAZĂ", nu în ReadDto: ObjectSpace PROASPĂT, proiecție pe coloană.
        (Guid? TipTvaId, decimal ValoareTva, decimal Valoare, decimal Cantitate) InBaza(Guid linieId) {
            using var o = s.Provider.CreateObjectSpace();
            return o.GetObjectsQuery<DocumentDetaliu>().Where(x => x.ID == linieId)
                .Select(x => new ValueTuple<Guid?, decimal, decimal, decimal>(
                    x.TipTvaId, x.ValoareTva, x.Valoare, x.Cantitate))
                .Single();
        }

        s.Check("Api RDC premisă: lotul original de 10 × 10,00 (livrarea care se stornează) e pe stoc",
            lot.PretUnitar == 10m && SoldCheieCub() == (10m, 100m));

        // ═══════════ (A) Culegerea: DOUĂ roluri într-un singur document ═══════════
        // Linia de COST primește DELIBERAT `TipTvaId` în payload — exact ce face
        // calea de produs (implicitul tipului pus pe orice linie nouă). Apply trebuie
        // să-l ȘTEARGĂ, nu doar să-l ignore (F19-D7).
        var idRdc = ReturClientApply.Aplica(os, null, new RdcWriteDto {
            Data = dataRdc, PredatorId = client.ID, PrimitorId = gest.ID,
            Linii = {
                new RdcLinieWriteDto { TipMaterialId = tip707.ID, Valoare = 100m, TipTvaId = n21.ID },
                new RdcLinieWriteDto { TipMaterialId = tip371.ID, LotId = lot.ID, Cantitate = 3m,
                    TipTvaId = n21.ID, Valoare = 999m }
            }
        });
        var cit = ReturClientApply.Citeste(os, idRdc);
        var lVenit = cit.Linii.Single(l => l.LotId == null);
        var lCost = cit.Linii.Single(l => l.LotId != null);
        s.Check("Api RDC (F19-D8): culegerea e POZITIVĂ pe ambele roluri — venitul stornat 100,00 + TVA 21,00, costul "
            + "round(3 × 10,00) = 30,00 (prețul lotului, `Valoare` din payload IGNORATĂ pe rolul de marfă)",
            lVenit.Valoare == 100m && lVenit.ValoareTva == 21m && lVenit.TipTvaId == n21.ID
            && lCost.Valoare == 30m && lCost.Cantitate == 3m);
        var costInBaza = InBaza(lCost.Id);
        Console.WriteLine($"     MĂSURAT (Api RDC, F19-D7 — proba ÎN BAZĂ): linia de cost {lCost.Id} a fost culeasă cu "
            + $"`TipTvaId = {n21.Cod}` în payload; în baza de date are TipTvaId="
            + $"{costInBaza.TipTvaId?.ToString() ?? "null"}, ValoareTva={costInBaza.ValoareTva:N2}, "
            + $"Valoare={costInBaza.Valoare:N2}.");
        s.Check("ANCORA F19-D7 (probat ÎN BAZĂ, pe ObjectSpace proaspăt — nu doar în ReadDto): linia de COST își pierde "
            + "IDENTITATEA fiscală la culegere, nu doar valoarea — `TipTvaId = null` PERSISTAT, oglinda exactă a lui "
            + "`PregatesteOperare`. „Inert devine adevărat, nu doar afirmat”: `RegistruTva` scrie un rând pentru "
            + "ORICE linie cu `TipTvaId`, deci un implicit rămas aici ar intra în jurnal (și în D394) ca bază "
            + "impozabilă — inclusiv la BACKFILL, care recitește din model",
            costInBaza.TipTvaId == null && costInBaza.ValoareTva == 0m && costInBaza.Valoare == 30m
            && lCost.TipTvaId == null && lCost.ValoareTva == 0m && lCost.TipTvaCod == null);
        s.Check("ANCORA F19-D9: `Total` == DOAR liniile de VENIT (121,00 brut — oglinda lui `ReturClient.Total` virtual "
            + "și a lui `LiniiCreanta`), iar costul iese SEPARAT în `TotalCost` (30,00) — operatorul nu citește „121” "
            + "acolo unde documentul valorează „121 creanță + 30 cost”",
            cit.Total == 121m && cit.TotalCost == 30m
            && cit.Linii.Sum(l => l.Valoare + l.ValoareTva) == 151m);   // totalul „naiv” al bazei
        s.Check("Api RDC: header plat, FĂRĂ număr (seria „RDC-” e server-owned — F19-D6), cu denumirile ambelor laturi",
            cit.Numar == null && cit.Stare == "Draft" && cit.Data == dataRdc
            && cit.PredatorId == client.ID && cit.PredatorDenumire == client.Denumire
            && cit.PrimitorId == gest.ID && cit.PrimitorDenumire == gest.Denumire
            && cit.PoateEdita && cit.PoateOpera && !cit.PoateAnula && !cit.PoateStorna);
        var randLista = ReturClientApply.Lista(os).Single(d => d.Id == idRdc);
        s.Check("Api RDC: Lista poartă ACELEAȘI două cifre (două agregate condiționate într-o singură grupare) — grila "
            + "nu are voie să arate alt total decât detaliul",
            randLista.Total == 121m && randLista.TotalCost == 30m && randLista.Stare == "Draft"
            && randLista.Numar == null);

        RdcWriteDto Rescriere(RdcReadDto d) => new() {
            Data = d.Data, PredatorId = d.PredatorId, PrimitorId = d.PrimitorId,
            Linii = d.Linii.Select(l => new RdcLinieWriteDto {
                Id = l.Id, TipMaterialId = l.TipMaterialId, LotId = l.LotId,
                Cantitate = l.Cantitate, Valoare = l.Valoare,
                TipTvaId = l.TipTvaId, ValoareTva = l.ValoareTva
            }).ToList()
        };
        ReturClientApply.Aplica(os, idRdc, Rescriere(cit));
        s.Check("Api RDC: PUT de round-trip complet nu schimbă nimic (idempotent la nivel de agregat)",
            ReturClientApply.Citeste(os, idRdc) is { Total: 121m, TotalCost: 30m, Linii.Count: 2 });

        // ═══════════ (B) RISCUL 5: rolul liniei nu se schimbă prin PUT ═══════════
        var scoateLotul = Rescriere(ReturClientApply.Citeste(os, idRdc));
        scoateLotul.Linii.Single(l => l.Id == lCost.Id).LotId = null;
        s.CheckRefuza("ANCORA riscul 5 (COST → VENIT): un PUT care SCOATE `LotId` de pe o linie de cost e REFUZ "
            + "EXPLICIT, nu conversie tăcută — rolul e o PREZENȚĂ, iar o conversie ar trebui să fie COMPLETĂ (TVA, "
            + "natura Tipului, valoarea, cantitatea pro-formă); pe jumătate ar lăsa în document o linie care nu e "
            + "niciunul din cele două lucruri",
            () => ReturClientApply.Aplica(s.OsCuGardian(), idRdc, scoateLotul));
        var puneLotul = Rescriere(ReturClientApply.Citeste(os, idRdc));
        puneLotul.Linii.Single(l => l.Id == lVenit.Id).LotId = lot.ID;
        s.CheckRefuza("ANCORA riscul 5 (VENIT → COST): și sensul invers e refuzat — simetria contează, altfel „rolul "
            + "e imuabil” ar fi o regulă cu o singură direcție",
            () => ReturClientApply.Aplica(s.OsCuGardian(), idRdc, puneLotul));
        var dupaRefuz = ReturClientApply.Citeste(os, idRdc);
        Console.WriteLine($"     MĂSURAT (Api RDC, riscul 5): ambele PUT-uri de schimbare de rol au fost REFUZATE; "
            + $"documentul are în continuare {dupaRefuz.Linii.Count} linii "
            + $"[{string.Join(", ", dupaRefuz.Linii.Select(l => (l.LotId == null ? "venit" : "marfă") + $":{l.Valoare:N2}"))}], "
            + $"Total {dupaRefuz.Total:N2} / TotalCost {dupaRefuz.TotalCost:N2}.");
        s.Check("Api RDC (riscul 5): refuzurile n-au lăsat reziduu — documentul e neatins, cu ambele roluri intacte",
            dupaRefuz.Linii.Count == 2 && dupaRefuz.Total == 121m && dupaRefuz.TotalCost == 30m
            && dupaRefuz.Linii.Single(l => l.Id == lCost.Id).LotId == lot.ID
            && dupaRefuz.Linii.Single(l => l.Id == lVenit.Id).LotId == null
            && InBaza(lCost.Id).TipTvaId == null);
        // Calea LEGITIMĂ de schimbare de rol: agregatul o exprimă deja.
        var reculegere = Rescriere(dupaRefuz);
        reculegere.Linii.RemoveAll(l => l.Id == lCost.Id);
        reculegere.Linii.Add(new RdcLinieWriteDto { TipMaterialId = tip707.ID, Valoare = 5m, TipTvaId = n21.ID });
        ReturClientApply.Aplica(os, idRdc, reculegere);
        var citReculegere = ReturClientApply.Citeste(os, idRdc);
        s.Check("Api RDC (riscul 5): calea LEGITIMĂ rămâne deschisă și e cea pe care agregatul o exprimă deja — linia "
            + "absentă din payload se ȘTERGE, iar rolul dorit se culege ca linie NOUĂ (aici: costul devine un al "
            + "doilea venit de 5,00 + 1,05 TVA, deci `Total` 127,05 și `TotalCost` 0,00)",
            !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == lCost.Id)
            && citReculegere.Linii.Count == 2 && citReculegere.Total == 127.05m && citReculegere.TotalCost == 0m);
        // …și înapoi la scena de bază.
        var inapoi = Rescriere(citReculegere);
        inapoi.Linii.RemoveAll(l => l.TipMaterialId == tip707.ID && l.Valoare == 5m);
        inapoi.Linii.Add(new RdcLinieWriteDto { TipMaterialId = tip371.ID, LotId = lot.ID, Cantitate = 3m });
        ReturClientApply.Aplica(os, idRdc, inapoi);
        var citBaza = ReturClientApply.Citeste(os, idRdc);
        lCost = citBaza.Linii.Single(l => l.LotId != null);
        s.Check("Api RDC: scena revine la forma de bază (venit 121,00 + marfă 30,00), iar linia de cost NOUĂ primește "
            + "și ea `TipTvaId = null` — implicitul tipului nu se aplică pe rolul de marfă",
            citBaza.Total == 121m && citBaza.TotalCost == 30m && InBaza(lCost.Id).TipTvaId == null);

        // ═══════════ (C) Refuzurile de PAYLOAD ═══════════
        RdcWriteDto Payload(params RdcLinieWriteDto[] linii) => new() {
            Data = dataRdc, PredatorId = client.ID, PrimitorId = gest.ID, Linii = linii.ToList()
        };
        RdcLinieWriteDto LinieVenit() => new() { TipMaterialId = tip707.ID, Valoare = 10m, TipTvaId = n21.ID };
        s.CheckRefuza("Api RDC: Id de linie STRĂIN → refuz (agregatul nu adoptă linii din alt document)", () => {
            var l = LinieVenit(); l.Id = Guid.NewGuid();
            ReturClientApply.Aplica(os, idRdc, Payload(l));
        });
        s.CheckRefuza("Api RDC: același Id de linie de două ori → refuz", () => {
            var l = LinieVenit(); l.Id = lVenit.Id;
            ReturClientApply.Aplica(os, idRdc, Payload(l, l));
        });
        s.CheckRefuza("Api RDC: `TipMaterialId` absent din payload → refuz de DOMENIU, nu violare de FK NOT NULL", () => {
            var l = LinieVenit(); l.TipMaterialId = Guid.Empty;
            ReturClientApply.Aplica(os, idRdc, Payload(l));
        });
        s.CheckRefuza("Api RDC: lot inexistent pe o linie de marfă → refuz de domeniu", () =>
            ReturClientApply.Aplica(os, idRdc, Payload(new RdcLinieWriteDto {
                TipMaterialId = tip371.ID, LotId = Guid.NewGuid(), Cantitate = 1m
            })));
        s.CheckRefuza("Api RDC: valoare în afara scării numeric(18,2) → refuz de domeniu, nu DbUpdateException", () => {
            var l = LinieVenit(); l.Valoare = 10.001m;
            ReturClientApply.Aplica(s.OsCuGardian(), idRdc, Payload(l));
        });
        s.CheckRefuza("Api RDC: latură inexistentă în nomenclatorul de repartitori → refuz de domeniu, înaintea "
            + "oricărei modificări a header-ului", () =>
            ReturClientApply.Aplica(os, idRdc, new RdcWriteDto {
                Data = dataRdc, PredatorId = client.ID, PrimitorId = Guid.NewGuid(), Linii = { LinieVenit() }
            }));
        s.Check("Api RDC: …iar returul rămâne pe laturile lui și cu ambele linii (niciun refuz n-a rescris nimic)",
            ReturClientApply.Citeste(os, idRdc) is { Linii.Count: 2, Total: 121m, TotalCost: 30m });
        // Ca pe RLF: reziduul unui Apply refuzat DUPĂ `CreateObject` îl curăță
        // reconcilierea următorului payload al ACELUIAȘI document.
        ReturClientApply.Aplica(os, idRdc, Rescriere(ReturClientApply.Citeste(os, idRdc)));
        s.Check("Api RDC: un Apply refuzat nu lasă reziduu în agregat — următorul payload valid readuce documentul la "
            + "exact două linii, cu aceleași cifre pe ambele roluri",
            ReturClientApply.Citeste(os, idRdc) is { Linii.Count: 2, Total: 121m, TotalCost: 30m }
            && os.GetObjectsQuery<DocumentDetaliu>().Count(d => d.DocumentId == idRdc) == 2);

        // ═══════════ (D) Refuzurile TIPULUI, prin calea API ═══════════
        var serieInainte = SerieRdc();
        Guid IdNou(Guid predator, Guid primitor, params RdcLinieWriteDto[] linii) =>
            ReturClientApply.Aplica(os, null, new RdcWriteDto {
                Data = dataRdc, PredatorId = predator, PrimitorId = primitor, Linii = linii.ToList()
            });

        var idLaturi = IdNou(gest.ID, client.ID, LinieVenit());
        s.CheckRefuza("Api RDC: laturi INVERSATE (predator gestiune / primitor partener) → refuz al tipului",
            () => ComenziDocument.Sistem(os).Opereaza(idLaturi));
        s.Check("Api RDC: dry-run-ul spune ACELAȘI lucru înaintea comenzii (43b: autoritar e motorul)",
            DryRun(idLaturi).Any(e => e.Contains(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit)));
        s.Check("Api RDC: refuzul n-a lăsat rânduri-fantomă (33d)",
            CubScena.FaraStoc(os, idLaturi)
            && CubScena.FaraNote(os, idLaturi));
        ReturClientApply.Sterge(os, idLaturi);

        var idVenitStoc = IdNou(client.ID, gest.ID,
            new RdcLinieWriteDto { TipMaterialId = tip371.ID, Valoare = 100m, TipTvaId = n21.ID });
        s.CheckRefuza("Api RDC: linie de VENIT (fără lot) cu Tip de STOC → refuz al tipului (venitul poartă natura "
            + "Serviciu; marfa care revine se culege pe o linie cu lot)",
            () => ComenziDocument.Sistem(os).Opereaza(idVenitStoc));
        ReturClientApply.Sterge(os, idVenitStoc);

        var idTipIncoerent = IdNou(client.ID, gest.ID,
            LinieVenit(),
            new RdcLinieWriteDto { TipMaterialId = tip301.ID, LotId = lot.ID, Cantitate = 1m });
        s.CheckRefuza("Api RDC: linie de marfă cu Tip incoerent cu produsul lotului → refuz al tipului",
            () => ComenziDocument.Sistem(os).Opereaza(idTipIncoerent));
        ReturClientApply.Sterge(os, idTipIncoerent);

        var idCapitalizat = IdNou(client.ID, gest.ID,
            new RdcLinieWriteDto { TipMaterialId = tip707.ID, Valoare = 100m, TipTvaId = ned21.ID });
        s.CheckRefuza("Api RDC: venit cu regim `Capitalizat` (NED21) → refuz al MOTORULUI (43b) — semnarea ar compunda "
            + "brutul la re-operare",
            () => ComenziDocument.Sistem(os).Opereaza(idCapitalizat));
        ReturClientApply.Sterge(os, idCapitalizat);

        s.Check("Api RDC (F19-D6 + GATE D6): niciun refuz n-a consumat seria „RDC-”",
            SerieRdc() == serieInainte);

        // ═══════════ (E) Operarea ═══════════
        s.Check("Api RDC: dry-run-ul returului complet nu întoarce nicio eroare", DryRun(idRdc).Count == 0);
        ComenziDocument.Sistem(os).Opereaza(idRdc);
        var citOperat = ReturClientApply.Citeste(os, idRdc);
        var venitOperat = citOperat.Linii.Single(l => l.LotId == null);
        var costOperat = citOperat.Linii.Single(l => l.LotId != null);
        s.Check("Api RDC: operarea SEMNEAZĂ (venit −100,00 / −21,00 cu cantitatea pro-formă 1 pozitivă; cost −3 / "
            + "−30,00), consumă seria „RDC-”, iar `Total` devine −121,00 — tot DOAR venitul",
            citOperat.Stare == "Operat" && citOperat.Numar != null && citOperat.Numar.StartsWith("RDC-")
            && venitOperat.Cantitate == 1m && venitOperat.Valoare == -100m && venitOperat.ValoareTva == -21m
            && costOperat.Cantitate == -3m && costOperat.Valoare == -30m && costOperat.ValoareTva == 0m
            && citOperat.Total == -121m && citOperat.TotalCost == -30m);
        var stocRdcCub = CubScena.Stoc(os, idRdc);
        s.Check("Api RDC: stoc +3 / +30,00 pe LOTUL ORIGINAL, în gestiunea PRIMITOARE (regula −1 × linia negativă) — "
            + "soldul urcă de la 10 la 13",
            stocRdcCub.Count == 1
            && stocRdcCub[0].Unitate == lot.ID
            && SoldCheieCub() == (13m, 130m));
        var noteRdcCub = NoteCub(idRdc);
        s.Check("Api RDC: note pe corespondența ORIGINALĂ, toate negative — `4111 = 707` cu −100,00, `4111 = 4427` cu "
            + "−21,00 și `607 = 371` cu −30,00",
            noteRdcCub.Nota(cont4111.ID, cont707.ID, -100m)
            && noteRdcCub.Nota(cont4111.ID, cont4427.ID, -21m)
            && noteRdcCub.Nota(cont607.ID, cont371.ID, -30m)
            && noteRdcCub.All(p => p.Fel == N.FelTranzactie.Operare));
        var fiscalRdcCub = CubScena.Fapte(os, idRdc);
        s.Check("Api RDC (consecința lui F19-D7, măsurată pe REGISTRU): jurnalul de TVA are UN SINGUR rând, al liniei "
            + "de VENIT, cu baza −100,00 și taxa −21,00 — nu −130,00, cum ar fi ieșit dacă linia de cost și-ar fi "
            + "păstrat tipul de TVA de la culegere",
            fiscalRdcCub.Count == 1 && fiscalRdcCub[0].DetaliuId == venitOperat.Id
            && fiscalRdcCub[0] is { Baza: -100m, Tva: -21m, Sens: SensTva.Livrare });

        // ═══════════ (F) Riscul 6: idempotența semnării prin calea API ═══════════
        var numarRdc = citOperat.Numar;
        ComenziDocument.Sistem(os).AnuleazaOperarea(idRdc);
        var citAnulat = ReturClientApply.Citeste(os, idRdc);
        ReturClientApply.Aplica(os, idRdc, Rescriere(citAnulat));
        var citRenormalizat = ReturClientApply.Citeste(os, idRdc);
        Console.WriteLine($"     MĂSURAT (Api RDC, riscul 6): după anulare ReadDto dădea venit "
            + $"{citAnulat.Linii.Single(l => l.LotId == null).Valoare:N2} / cost "
            + $"{citAnulat.Linii.Single(l => l.LotId != null).Valoare:N2}; PUT-ul aceluiași ReadDto a readus "
            + $"documentul la Total {citRenormalizat.Total:N2} / TotalCost {citRenormalizat.TotalCost:N2}.");
        s.Check("RISCUL 6 (RDC): pe DRAFT bate Apply — round-trip-ul unui ReadDto SEMNAT readuce AMBELE roluri la forma "
            + "de CULEGERE (venit +100,00 / +21,00, cost +30,00), în loc să fie refuzat sau să compundă semnul",
            citAnulat.Stare == "Draft" && citAnulat.Total == -121m
            && citRenormalizat.Total == 121m && citRenormalizat.TotalCost == 30m
            && citRenormalizat.Linii.Single(l => l.LotId == null).Cantitate == 1m
            && citRenormalizat.Linii.Single(l => l.LotId != null).Cantitate == 3m
            && SoldCheieCub() == (10m, 100m));
        ComenziDocument.Sistem(os).Opereaza(idRdc);
        var citReoperat = ReturClientApply.Citeste(os, idRdc);
        s.Check("RISCUL 6 (RDC): re-operarea după anulare + PUT dă EXACT aceleași cifre și același număr",
            citReoperat.Numar == numarRdc && citReoperat.Total == -121m
            && citReoperat.Linii.Single(l => l.LotId != null).Valoare == -30m
            && SoldCheieCub() == (13m, 130m));

        // ═══════════ (G) Storno ═══════════
        s.Check("Api RDC: storno prin API → Stornat, rânduri INVERSE append-only (POZITIVE, cu flag `Storno`); stocul "
            + "revine la 10",
            ComenziDocument.Sistem(os).Storneaza(idRdc, new DateOnly(2026, 10, 20)).StareNoua == StareDocument.Stornat
            && CubScena.Stoc(os, idRdc).Count(p => p.Storno) == 1
            && CubScena.Note(os, idRdc).Count(p => p.Storno) == 6
            && SoldCheieCub() == (10m, 100m));
        s.CheckRefuza("Api RDC: PUT pe un document Stornat se refuză (pre-check de domeniu, înaintea gardianului)",
            () => ReturClientApply.Aplica(os, idRdc, Rescriere(ReturClientApply.Citeste(os, idRdc))));
        s.CheckRefuza("Api RDC: DELETE pe un document Stornat se refuză",
            () => ReturClientApply.Sterge(os, idRdc));

        Curata();
        s.Check("Api RDC: curățenie finală (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj))
            && os.GetObjectByKey<ReturClient>(idRdc) == null);
    }
}
