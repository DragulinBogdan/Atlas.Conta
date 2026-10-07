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

// ============ Felia 30, pasul 3: declarantul BCS pe scenă proprie ============
// Scena e2e „3c: BonConsum" e BUGETARĂ (blocul privat se încheie cu `return`),
// deci proba declarantului pe AMBELE profiluri cere o scenă proprie. Al doilea
// lot al ei MĂSOARĂ N-r3: raportul valoric al unei chei de stoc se poate
// depărta de `Lot.PretUnitar` (înghețat la prima intrare), iar motorul vechi
// valorizează cu prețul, nucleul cu raportul curent.
//
// Mecanismul celei de-a doua intrări: rând de DESCHIDERE pe aceeași cheie de
// stoc — același mecanism cu care scenele își deschid stocul, și singurul
// disponibil azi: documentele nu pot da două prețuri pe același lot (lotul E
// prețul, 13 — NIR-ul pe lot străin reia `Lot.PretUnitar`, LDI plus naște lot
// nou, corecția renaște lotul, DVI nu postează valoare pe stoc).
static class VerificaNucleuBcs {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajNucBcs = "E2E-NUC-BCS";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";

        void CurataNucBcs(IObjectSpace os) {
            var pj = new Purja(os);
            var idsLot = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucBcs)).Select(l => l.ID).ToList();
            var idsDoc = os.GetObjectsQuery<BonConsum>()
                .Where(d => d.Predator.Cod.StartsWith(MarcajNucBcs) || d.Primitor.Cod.StartsWith(MarcajNucBcs))
                .Select(d => d.ID).ToList();
            var deschideri = os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => p.DocumentId == null && p.Unitate != null && idsLot.Contains(p.Unitate.Value))
                .Select(p => p.TranzactieId).Distinct().ToList();
            pj.AdaugaCheie<Atlas.Conta.BackOffice.Module.Cub.Postare>(os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => deschideri.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
            pj.AdaugaCheie<Atlas.Conta.BackOffice.Module.Cub.Tranzactie>(deschideri);
            ProbeCub.Purjeaza(pj, os, idsDoc);                                             // S-D8
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => idsLot.Contains(r.LotId)
                    || (r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value))).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => idsDoc.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucBcs)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(MarcajNucBcs)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucBcs)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataNucBcs(os);
        try {
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.01.00"));
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = MarcajNucBcs + "-LOC";
        loc.Denumire = "Loc de consum probă felia 30";
        loc.Calitati = CalitateRepartitor.LocConsum;

        Lot Lotul(string sufix, decimal pretUnitar) {
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajNucBcs + sufix;
            produs.Denumire = "Produs probă felia 30" + sufix;
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;
            var lotNou = os.CreateObject<Lot>();
            lotNou.Produs = produs;
            lotNou.PretUnitar = pretUnitar;
            lotNou.Gestiune = mag1;
            lotNou.Data = new DateOnly(2026, 1, 10);
            return lotNou;
        }
        BonConsum Consum(Lot lot, decimal cantitate, DateOnly data) {
            var doc = os.CreateObject<BonConsum>();
            doc.Data = data;
            doc.Predator = mag1;
            doc.Primitor = loc;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc;
            d.TipMaterial = tipMaterial;
            d.Lot = lot;
            d.Cantitate = cantitate;
            return doc;
        }

        // (A) lotul necorectat: raportul curent E prețul înghețat.
        var lotCurat = Lotul("-A", 10m);
        // (B) lotul corectat (N-r3): soldul 20 / 300 cu prețul înghețat la 10.
        var lotCorectat = Lotul("-B", 10m);
        var lotCub = Lotul("-E", 10m);
        os.CommitChanges();
        using (var tx = TranzactieComanda.Incepe(os)) {
            var contStoc = tipMaterial.ContImplicitId.Value;
            var contrapartida = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
            Atlas.Conta.BackOffice.Module.Cub.Materializare.Deschide(os, new(2026, 1, 20),
                [new(contStoc, N.Latura.Debit, 500, true), new(contrapartida, N.Latura.Credit, 500)],
                [new(contStoc, lotCurat.ID, mag1.ID, 10, 100), new(contStoc, lotCorectat.ID, mag1.ID, 20, 300),
                 new(contStoc, lotCub.ID, mag1.ID, 10, 100)], []);
            os.CommitChanges(); tx.Commit();
        }

        var bcs = Consum(lotCurat, 4m, new DateOnly(2026, 3, 5));
        os.CommitChanges();
        MotorOperare.Opereaza(os, bcs);
        s.Check($"NUC-BCS-{eticheta} scenă: BCS operat pe lot necorectat — linia la 40 (4 × 10), "
            + "două rânduri de stoc și o notă",
            bcs.Detalii.Single().Valoare == 40m);
        ProbeNucleu.Proba(os, s.Check, $"NUC-BCS-{eticheta}", [bcs]);

        // --- N-r3 MĂSURAT: prețul înghețat contra raportului curent ---
        var soldCorectat = CubScena.Sold(os, lotCorectat.ID, mag1.ID, null, new DateOnly(2026, 3, 6));
        s.Check($"NUC-BCS-N-R3-1 ({eticheta}): cheia de stoc are 20 buc / 300 lei (raport 15), dar "
            + $"`Lot.PretUnitar` a rămas înghețat la {lotCorectat.PretUnitar} — prețul ≠ raportul",
            soldCorectat.Cantitate == 20m && soldCorectat.Valoare == 300m && lotCorectat.PretUnitar == 10m);

        var bcsR3 = Consum(lotCorectat, 5m, new DateOnly(2026, 3, 6));
        MotorOperare.Opereaza(os, bcsR3);
        var valoareVeche = bcsR3.Detalii.Single().Valoare;
        s.Check($"NUC-BCS-N-R3-2 ({eticheta}): motorul vechi valorizează cu prețul înghețat — X = {valoareVeche} "
            + "(5 × 10), aceeași cifră în nota contabilă",
            valoareVeche == 75m
            && CubScena.Note(os, bcsR3.ID).Where(p => p.Debit).Sum(p => p.Valoare) == 75m);

        var contractR3 = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, bcsR3);
        var valoareNoua = contractR3.Decizii.OfType<N.ValoareIesire>().Single().Valoare;
        Console.WriteLine($"     MĂSURAT (N-r3/{eticheta}): lot cu două intrări (10 × 10 lei + 10 × 20 lei) = "
            + $"20 buc / 300 lei, `Lot.PretUnitar` = {lotCorectat.PretUnitar}; consum de 5 buc → "
            + $"motorul vechi X = {valoareVeche}, nucleul Y = {valoareNoua}, "
            + $"Δ = Y − X = {valoareNoua - valoareVeche}.");
        s.Check($"NUC-BCS-N-R3-3 ({eticheta}): nucleul evaluează pe raportul CURENT — Y = {valoareNoua} "
            + $"(5 × 300/20), deci Δ = Y − X = {valoareNoua - valoareVeche}",
            contractR3.EsteAcceptat && valoareNoua == 75m && valoareNoua == valoareVeche);
        s.Check($"NUC-BCS-N-R3-5 ({eticheta}): tranzacția declarantului se conservă pe cifra nouă "
            + "(valoarea nu se pierde între capete)",
            contractR3.Tranzactii.SelectMany(N.Conservare.Verifica).Count() == 0);

        // --- NUC-BCS-REFUZURI (MINOR-1): refuzul e al LINIEI, nu al documentului ---
        // Două linii pe două loturi fără stoc: `Evaluare.Iesire` aruncă pe fiecare, iar
        // declarantul le adună pe amândouă în loc să se oprească la prima (B-D2).
        var lotGol1 = Lotul("-C", 10m);
        var lotGol2 = Lotul("-D", 10m);
        os.CommitChanges();
        var bcsRefuzat = Consum(lotGol1, 1m, new DateOnly(2026, 3, 7));
        var aDoua = os.CreateObject<DocumentDetaliu>();
        aDoua.Document = bcsRefuzat;
        aDoua.TipMaterial = tipMaterial;
        aDoua.Lot = lotGol2;
        aDoua.Cantitate = 2m;
        os.CommitChanges();
        var contractRefuzat = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, bcsRefuzat);
        s.Check($"NUC-BCS-REFUZURI ({eticheta}): două linii pe loturi fără stoc ⇒ DOUĂ refuzuri "
            + "STOC_INSUFICIENT, câte unul pe linia lui (nu primul și atât)",
            !contractRefuzat.EsteAcceptat
            && contractRefuzat.Refuzuri.Count == 2
            && contractRefuzat.Refuzuri.All(r => r.Cod == N.Coduri.StocInsuficient)
            && contractRefuzat.Refuzuri.Select(r => r.Linie).Distinct().Count() == 2
            && contractRefuzat.Refuzuri.All(r => bcsRefuzat.Detalii.Any(d => d.ID == r.Linie)));

        // --- Felia 31 (TR-D7a), S-D8: cubul PERSISTAT pe BCS ---
        var bcsCub = Consum(lotCub, 3m, new DateOnly(2026, 3, 8));
        var bcsAnulat = Consum(lotCub, 2m, new DateOnly(2026, 3, 9));
        os.CommitChanges();
        var dataStornoBcs = new DateOnly(2026, 7, 22);
        {
            MotorOperare.Opereaza(os, bcsCub);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-BCS-{eticheta}", bcsCub);

            MotorOperare.Storneaza(os, bcsCub, dataStornoBcs);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-BCS-{eticheta}", bcsCub, dataStornoBcs);

            MotorOperare.Opereaza(os, bcsAnulat);
            s.Check($"STR-ANULARE ({eticheta}) premisă: documentul are tranzacție și postări în cub după operare",
                ProbeCub.Tranzactii(os, bcsAnulat.ID).Count == 1 && ProbeCub.Postari(os, bcsAnulat.ID).Count > 0);
            MotorOperare.AnuleazaOperarea(os, bcsAnulat);
            ProbeCub.FaraRanduri(os, s.Check,
                $"STR-ANULARE ({eticheta}): anularea operării șterge FIZIC tranzacția `Operare` și postările ei, "
                + "simetric cu registrele",
                bcsAnulat.ID);
        }

        CurataNucBcs(os);
        s.Check($"NUC-BCS-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajNucBcs))
            && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNucBcs)));
        }
        finally { using var curatare = s.Provider.CreateObjectSpace(); CurataNucBcs(curatare); }

    }
}
