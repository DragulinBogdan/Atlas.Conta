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

// ═══ Felia 32, pasul 2b — laturile documentului ca structură (T-D13) ═══
static class VerificaLaturi {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajLat = "E2E-NUC-LAT";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";
        const string CodPredator = Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit;
        const string CodPrimitor = Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PrimitorNepotrivit;

        void CurataLat(IObjectSpace os) {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajLat)).Select(r => r.ID).ToList();
            var idsLot = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajLat)).Select(l => l.ID).ToList();
            var idsDoc = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)
                    || d.Detalii.Any(x => x.LotId != null && idsLot.Contains(x.LotId.Value)))
                .Select(d => d.ID).ToList();
            ProbeCub.Purjeaza(pj, os, idsDoc);
            DeschidereScena.Curata(os, pj, idsLot);
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => idsLot.Contains(r.LotId)
                    || (r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value))).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => idsDoc.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>()
                .Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajLat)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(x => x.Cod.StartsWith(MarcajLat)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajLat)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataLat(os);

        // --- STR-LATURI-CONTRACT: fiecare tip de document din seed declară `Laturi()` ---
        // Documentele de probă îl respectă prin ușa entității, pe care fiecare scenă o
        // trece la operare; pe date reale, recensământul clonei Flax (contract, T-D13).
        var tipuri = os.GetObjectsQuery<TipDocument>().Select(t => new { t.Cod, t.ClrType }).ToList();
        var assembly = typeof(Document).Assembly;
        var faraContract = new List<string>();
        foreach (var t in tipuri) {
            var clasa = assembly.GetType($"{typeof(Document).Namespace}.{t.ClrType}");
            if (clasa is null || Activator.CreateInstance(clasa) is not Document instanta) {
                faraContract.Add(t.Cod);
                continue;
            }
            var contract = instanta.Laturi();
            if (contract is null
                    || contract.Predator.Permisa == Atlas.Conta.BackOffice.Module.Declaratii.Parte.Niciuna
                    || contract.Primitor.Permisa == Atlas.Conta.BackOffice.Module.Declaratii.Parte.Niciuna)
                faraContract.Add(t.Cod);
        }
        s.Check($"STR-LATURI-CONTRACT ({eticheta}): toate cele {tipuri.Count} tipuri de document din seed declară "
            + "`Laturi()` cu părți nevide pe ambele laturi (lipsă: "
            + (faraContract.Count == 0 ? "niciunul" : string.Join(", ", faraContract)) + ")",
            tipuri.Count > 0 && faraContract.Count == 0);

        // --- STR-LATURI-REFUZ: latura de partea greșită, pe ambele uși, cu același cod ---
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
        var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.01.00"));
        var tipBcs = os.FirstOrDefault<TipDocument>(t => t.ClrType == nameof(BonConsum));
        var tert = os.CreateObject<Partener>();
        tert.Cod = MarcajLat + "-TERT";
        tert.Denumire = "Partener probă laturi";
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = MarcajLat + "-LOC";
        loc.Denumire = "Loc de consum probă laturi";
        loc.Calitati = CalitateRepartitor.LocConsum;
        var produs = os.CreateObject<Produs>();
        produs.Cod = MarcajLat + "-P";
        produs.Denumire = "Produs probă laturi";
        produs.UM = "BUC";
        produs.TipMaterial = tipMaterial;
        var lot = os.CreateObject<Lot>();
        lot.Produs = produs;
        lot.PretUnitar = 10m;
        lot.Gestiune = mag1;
        lot.Data = new DateOnly(2026, 1, 10);
        DeschidereScena.Scrie(os, lot, 10m, 100m);
        BonConsum Bcs(Repartitor predator, Repartitor primitor) {
            var doc = os.CreateObject<BonConsum>();
            doc.Data = new DateOnly(2026, 3, 5);
            doc.Predator = predator;
            doc.Primitor = primitor;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc;
            d.TipMaterial = tipMaterial;
            d.Lot = lot;
            d.Cantitate = 1m;
            return doc;
        }
        var bcsPredatorExtern = Bcs(tert, loc);
        var bcsPrimitorFaraCalitate = Bcs(mag1, mag2);
        var bcsCorect = Bcs(mag1, loc);
        var decPredatorIntern = os.CreateObject<Decont>();
        decPredatorIntern.Data = new DateOnly(2026, 3, 5);
        decPredatorIntern.Predator = mag1;
        decPredatorIntern.Primitor = loc;
        os.CommitChanges();

        // Aceeași funcție pe ambele uși: mesajul declarației = linia din refuzul de operare.
        static bool AceeasiLinie(IReadOnlyList<string> declaratie, string operare, string cod) =>
            declaratie.Count(m => m.StartsWith(cod + ":", StringComparison.Ordinal)) == 1
            && operare != null
            && operare.Split('\n').Contains(declaratie.Single(m => m.StartsWith(cod + ":", StringComparison.Ordinal)));

        var declPredator = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, bcsPredatorExtern, tipBcs);
        var operPredator = s.Refuz(() => MotorOperare.Opereaza(os, bcsPredatorExtern));
        s.Check($"STR-LATURI-REFUZ ({eticheta}): BCS cu predator PARTENER e refuzat pe ușa declarației cu "
            + "`PREDATOR_NEPOTRIVIT` (înaintea declarantului: singurul refuz) și pe ușa entității cu ACEEAȘI linie; "
            + "nimic scris, documentul rămâne Draft",
            declPredator.Count == 1 && AceeasiLinie(declPredator, operPredator, CodPredator)
            && ProbeCub.Tranzactii(os, bcsPredatorExtern.ID).Count == 0
            && bcsPredatorExtern.Stare == StareDocument.Draft);

        var declPrimitor = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, bcsPrimitorFaraCalitate, tipBcs);
        var operPrimitor = s.Refuz(() => MotorOperare.Opereaza(os, bcsPrimitorFaraCalitate));
        s.Check($"STR-LATURI-REFUZ ({eticheta}): BCS cu primitor gestiune FĂRĂ calitatea LocConsum e refuzat pe "
            + "ambele uși cu `PRIMITOR_NEPOTRIVIT`, mesajul numește calitatea lipsă; nimic scris",
            declPrimitor.Count == 1 && AceeasiLinie(declPrimitor, operPrimitor, CodPrimitor)
            && declPrimitor[0].Contains("fără calitatea LocConsum", StringComparison.Ordinal)
            && ProbeCub.Tranzactii(os, bcsPrimitorFaraCalitate.ID).Count == 0
            && bcsPrimitorFaraCalitate.Stare == StareDocument.Draft);

        var operDec = s.Refuz(() => MotorOperare.Opereaza(os, decPredatorIntern));
        s.Check($"STR-LATURI-REFUZ ({eticheta}): DEC cu predator gestiune e refuzat pe ușa "
            + "entității cu `PREDATOR_NEPOTRIVIT` — contractul e al clasei, nu al declarantului",
            operDec != null && operDec.Split('\n').Any(l => l.StartsWith(CodPredator + ":", StringComparison.Ordinal))
            && decPredatorIntern.Stare == StareDocument.Draft);

        var declCorect = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, bcsCorect, tipBcs);
        s.Check($"STR-LATURI-REFUZ ({eticheta}): BCS cu laturile pe partea permisă (gestiune → loc de consum) "
            + "nu are niciun refuz de latură pe ușa declarației",
            !declCorect.Any(m => m.StartsWith(CodPredator + ":", StringComparison.Ordinal)
                || m.StartsWith(CodPrimitor + ":", StringComparison.Ordinal)));

        CurataLat(os);
        s.Check($"NUC-LAT-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajLat))
            && !os.GetObjectsQuery<Produs>().Any(x => x.Cod.StartsWith(MarcajLat)));
    }
}
