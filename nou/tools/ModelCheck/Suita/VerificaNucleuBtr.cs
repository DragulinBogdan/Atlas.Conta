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

// ====== Felia 32, pasul 1: BTR pe cub — felul mixt `Operare`/`Transfer` (T-D2) ======
// Pe modelul de azi BTR produce NUMAI `Transfer`: cheia de stoc e (Lot, Repartitor,
// TipStoc), deci lotul își schimbă gestiunea și contul rămâne al lui pe ambele capete
// (T-D2.1). Ramura `Operare` a formei mixte e probată de proprietățile nucleului
// (`MotorTeste.DeclaratiaCuAmbeleListeDaOperareaApoiTransferul`) și, pe scenă, de ASM.
static class VerificaNucleuBtr {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajNucBtr = "E2E-NUC-BTR";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";

        void CurataNucBtr(IObjectSpace os) {
            var pj = new Purja(os);
            var idsLot = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucBtr)).Select(l => l.ID).ToList();
            var idsDoc = os.GetObjectsQuery<NotaTransfer>()
                .Where(d => d.NumarPV == MarcajNucBtr).Select(d => d.ID).ToList();
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
            pj.Adauga(os.GetObjectsQuery<Document>()
                .Where(d => idsDoc.Contains(d.ID)).ToList());
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucBtr)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(MarcajNucBtr)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataNucBtr(os);
        try {
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
        var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.01.00"));

        Lot Lotul(string sufix, decimal pretUnitar) {
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajNucBtr + sufix;
            produs.Denumire = "Produs probă felia 32" + sufix;
            produs.UM = "BUC";
            produs.TipMaterial = tipMaterial;
            var lotNou = os.CreateObject<Lot>();
            lotNou.Produs = produs;
            lotNou.PretUnitar = pretUnitar;
            lotNou.Gestiune = mag1;
            lotNou.Data = new DateOnly(2026, 1, 10);
            return lotNou;
        }
        NotaTransfer Btr(Lot lot, decimal cantitate, DateOnly data, Repartitor dinspre, Repartitor spre) {
            var doc = os.CreateObject<NotaTransfer>();
            doc.Data = data;
            doc.NumarPV = MarcajNucBtr;
            doc.Predator = dinspre;
            doc.Primitor = spre;
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc;
            d.TipMaterial = tipMaterial;
            d.Lot = lot;
            d.Cantitate = cantitate;
            return doc;
        }

        var lotA = Lotul("-A", 10m);
        var lotB = Lotul("-B", 10m);
        var lotC = Lotul("-C", 10m);
        os.CommitChanges();
        using (var tx = TranzactieComanda.Incepe(os)) {
            var contStoc = tipMaterial.ContImplicitId.Value;
            var contrapartida = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
            Atlas.Conta.BackOffice.Module.Cub.Materializare.Deschide(os, new(2026, 1, 20),
                [new(contStoc, N.Latura.Debit, 450, true), new(contrapartida, N.Latura.Credit, 450)],
                [new(contStoc, lotA.ID, mag1.ID, 10, 100), new(contStoc, lotB.ID, mag1.ID, 20, 300),
                 new(contStoc, lotC.ID, mag1.ID, 5, 50)], []);
            os.CommitChanges(); tx.Commit();
        }
        var btr = Btr(lotA, 4m, new DateOnly(2026, 3, 5), mag1, mag2);
        var btrGolire = Btr(lotB, 20m, new DateOnly(2026, 3, 6), mag1, mag2);
        var btrAnulat = Btr(lotC, 2m, new DateOnly(2026, 3, 7), mag1, mag2);
        os.CommitChanges();
        var dataStornoBtr = new DateOnly(2026, 7, 22);

        {
            // --- STR-BTR-ACELASI-CONT: o singură tranzacție, de fel `Transfer` ---
            MotorOperare.Opereaza(os, btr);
            var tranzactii = ProbeCub.Tranzactii(os, btr.ID);
            var postari = ProbeCub.Postari(os, btr.ID);
            var contStoc = tipMaterial.ContImplicitId;
            s.Check($"STR-BTR-ACELASI-CONT ({eticheta}): BTR MAG1 -> MAG2 pe un lot cu sold scrie EXACT o "
                + "tranzacție a documentului, de fel `Transfer`, cu 2 postări per linie, ambele pe `Debit`",
                tranzactii.Count == 1 && tranzactii[0].Fel == N.FelTranzactie.Transfer
                && postari.Count == 2 * btr.Detalii.Count
                && postari.All(p => p.Latura == N.Latura.Debit && p.Cont == contStoc));
            s.Check($"STR-BTR-ACELASI-CONT ({eticheta}): suma valorilor per (cont, latură) = 0 — transferul iese "
                + "din rapoartele pe cont (090f); cantitatea e -4 pe MAG1 și +4 pe MAG2",
                postari.GroupBy(p => (p.Cont, p.Latura)).All(g => g.Sum(p => p.Valoare) == 0m)
                && postari.Single(p => p.Gestiune == mag1.ID) is { Cantitate: -4m, Valoare: -40m }
                && postari.Single(p => p.Gestiune == mag2.ID) is { Cantitate: 4m, Valoare: 40m });
            s.Check($"STR-BTR-ACELASI-CONT ({eticheta}): semnalul de gestiune NU se pierde — fiecare capăt "
                + "poartă `Gestiune` și `Unitate` (lotul), pe ACELAȘI lot (T-D2.1)",
                postari.All(p => p.Gestiune != null && p.Unitate == lotA.ID && p.Produs == lotA.ProdusId)
                && postari.Select(p => p.Gestiune).Distinct().Count() == 2);
            s.Check($"STR-BTR-ACELASI-CONT ({eticheta}): `Sold` din cub pe lot rămâne NESCHIMBAT — transferul "
                + "mută unitatea între gestiuni, nu-i schimbă soldul",
                ProbeCub.SoldUnitate(os, btr.ID, lotA.ID) == N.Sold.Zero);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-BTR-{eticheta}", btr);

            // --- STR-BTR-GOLIRE: valoarea mutată e restul valoric, nu cantitate x preț ---
            MotorOperare.Opereaza(os, btrGolire);
            var postariGolire = ProbeCub.Postari(os, btrGolire.ID);
            s.Check($"STR-BTR-GOLIRE ({eticheta}): linia care golește lotul mută tot restul valoric (300, pe când "
                + $"cantitatea x prețul înghețat {lotB.PretUnitar} ar fi dat 200), identic cu `RegistruStoc`",
                postariGolire.Count == 2
                && postariGolire.Single(p => p.Gestiune == mag2.ID).Valoare == 300m);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-BTR-GOLIRE-{eticheta}", btrGolire);

            // --- STR-BTR-STORNO: o tranzacție `Storno` peste postările transferului ---
            MotorOperare.Storneaza(os, btr, dataStornoBtr);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-BTR-{eticheta}", btr, dataStornoBtr);

            // --- STR-BTR-ANULARE: anularea șterge FIZIC transferul ---
            MotorOperare.Opereaza(os, btrAnulat);
            s.Check($"STR-BTR-ANULARE ({eticheta}) premisă: documentul are tranzacție și postări în cub",
                ProbeCub.Tranzactii(os, btrAnulat.ID).Count == 1
                && ProbeCub.Postari(os, btrAnulat.ID).Count == 2);
            MotorOperare.AnuleazaOperarea(os, btrAnulat);
            ProbeCub.FaraRanduri(os, s.Check,
                $"STR-BTR-ANULARE ({eticheta}): anularea operării șterge FIZIC tranzacția `Transfer` și "
                + "postările ei, simetric cu registrele",
                btrAnulat.ID);

            // --- STR-BTR-REFUZ: refuzul declarației, fără nimic scris ---
            var tipBtr = os.FirstOrDefault<TipDocument>(t => t.ClrType == nameof(NotaTransfer));
            var btrIdentice = Btr(lotA, 1m, new DateOnly(2026, 3, 8), mag1, mag1);
            var btrFaraLot = Btr(lotA, 1m, new DateOnly(2026, 3, 8), mag1, mag2);
            os.CommitChanges();
            btrFaraLot.Detalii.Single().Lot = null;
            os.CommitChanges();
            var refuzIdentice = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, btrIdentice, tipBtr);
            var refuzFaraLot = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, btrFaraLot, tipBtr);
            s.Check($"STR-BTR-REFUZ ({eticheta}): predator = primitor da `GESTIUNI_IDENTICE`, linia fără lot da "
                + "`LOT_LIPSA`, iar dry-run-ul nu scrie nimic în cub",
                refuzIdentice.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.GestiuniIdentice, StringComparison.Ordinal))
                && refuzFaraLot.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.LotLipsa, StringComparison.Ordinal))
                && ProbeCub.Tranzactii(os, btrIdentice.ID).Count == 0
                && ProbeCub.Tranzactii(os, btrFaraLot.ID).Count == 0);
        }

        CurataNucBtr(os);
        s.Check($"NUC-BTR-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajNucBtr))
            && !os.GetObjectsQuery<NotaTransfer>().Any(d => d.NumarPV == MarcajNucBtr));
        } finally {
            using var curatenie = s.Provider.CreateObjectSpace();
            CurataNucBtr(curatenie);
        }
    }
}
