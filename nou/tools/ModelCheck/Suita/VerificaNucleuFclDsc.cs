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

// ====== Felia 32, pasul 2: FCL ∪ DSC pe cub (T-D4) ======
// FCL e pur creanță: venitul și taxa COLECTATĂ per linie, partidă pe fiecare cont cu
// rol de terț atins de linii (4111 și, pe regularizarea de avans, 419), zero postări
// de stoc. DSC duce costul și stocul: lotul iese din gestiunea predatoare evaluat pe
// raportul curent, iar capătul de cost stă pe gestiunea virtuală `Client` — marfa a
// părăsit patrimoniul, deci gestiunea lui e a terțului (T-D4.1 pe oracol).
static class VerificaNucleuFclDsc {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajNucFclDsc = "E2E-NUC-FCLDSC";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";

        void CurataNucFclDsc(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucFclDsc)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            var idsLot = os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucFclDsc)).Select(l => l.ID).ToList();
            var deschideri = os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => p.DocumentId == null && p.Unitate != null && idsLot.Contains(p.Unitate.Value))
                .Select(p => p.TranzactieId).Distinct().ToList();
            pj.AdaugaCheie<Atlas.Conta.BackOffice.Module.Cub.Postare>(os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                .Where(p => deschideri.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
            pj.AdaugaCheie<Atlas.Conta.BackOffice.Module.Cub.Tranzactie>(deschideri);
            ProbeCub.Purjeaza(pj, os, docIds);                                             // S-D8
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Descărcările autogenerate înaintea facturilor care le-au născut.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucFclDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(x => x.Cod.StartsWith(MarcajNucFclDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(MarcajNucFclDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(MarcajNucFclDsc)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucFclDsc)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataNucFclDsc(os);
        try {
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
        var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "704" : "751.01.00"));
        var tipMarfa = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        Cont ContSimbolFclDsc(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
        var contCreanta = ContSimbolFclDsc(privat ? "4111" : "411.01.01");
        var cont4427 = ContSimbolFclDsc("4427");
        var cont419 = ContSimbolFclDsc("419");
        var cont607 = ContSimbolFclDsc("607");
        var cont707 = ContSimbolFclDsc("707");
        s.Check($"NUC-FCLDSC-{eticheta} scenă: profilul are contul de creanță al FCL, iar rolul de terț pe el "
            + $"e al PROFILULUI SAF-T — aici {(privat ? "Client" : "Niciunul")}; urmărirea partidei este separată",
            contCreanta != null && tipVenit?.ContImplicitId != null
            && (contCreanta.RolTert == RolTertCont.Client) == privat);

        // Emitentul e al SCENEI, nu `SEDIU`: fiecare document al ei are atunci un
        // repartitor marcat pe o latură, deci purja îl găsește (inclusiv facturile de refuz).
        var emitent = os.CreateObject<UnitateInterna>();
        emitent.Cod = MarcajNucFclDsc + "-EM";
        emitent.Denumire = "Emitent probă felia 32";
        var client = os.CreateObject<Partener>();
        client.Cod = MarcajNucFclDsc + "-CL";
        client.Denumire = "Client probă felia 32";
        CodEconomic codEc = null;
        if (!privat) {
            // Conturile de venit bugetare cer clasificația E pe notă (3d).
            codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajNucFclDsc + "-CE";
            codEc.Denumire = "Clasificație de venit probă felia 32";
        }
        TipMaterial tipAvans = null;
        if (privat) {
            tipAvans = os.CreateObject<TipMaterial>();
            tipAvans.Cod = MarcajNucFclDsc + "-AV";
            tipAvans.Denumire = "Regularizare avans client (419)";
            tipAvans.Clasa = tipVenit.Clasa;
            tipAvans.ContImplicit = cont419;
        }
        os.CommitChanges();

        FacturaIesire Factura(DateOnly data, Gestiune descarcare = null) {
            var doc = os.CreateObject<FacturaIesire>();
            doc.Data = data;
            doc.Predator = emitent;
            doc.Primitor = client;
            doc.GestiuneDescarcare = descarcare;
            return doc;
        }
        FacturaIesireDetaliu Linie(FacturaIesire doc, TipMaterial tip, decimal cantitate, decimal pret,
                TipTva tva = null, Produs produs = null) {
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = doc;
            d.TipMaterial = tip;
            d.Cantitate = cantitate;
            d.PretUnitar = pret;
            d.TipTva = tva;
            d.Produs = produs;
            d.CodEconomicId = codEc?.ID;
            return d;
        }
        Guid Partida(Document doc, Cont cont) =>
            N.Unitate.DeschidePartida(cont.ID, client.ID, doc.ID, doc.DataInregistrare).Id;

        var fclVenit = Factura(new DateOnly(2026, 3, 5));
        Linie(fclVenit, tipVenit, 1m, 200m, privat ? n21 : null);
        var fclStorno = Factura(new DateOnly(2026, 3, 6));
        Linie(fclStorno, tipVenit, 1m, 90m, privat ? n21 : null);
        var fclAnulat = Factura(new DateOnly(2026, 3, 7));
        Linie(fclAnulat, tipVenit, 1m, 70m, privat ? n21 : null);
        os.CommitChanges();
        var dataStornoFcl = new DateOnly(2026, 7, 22);

        {
            // --- STR-FCL-VENIT-TVA: creanța cu partidă, venitul și taxa pe gestiunea emitentului ---
            MotorOperare.Opereaza(os, fclVenit);
            var postariVenit = ProbeCub.Postari(os, fclVenit.ID);
            var partidaVenit = Partida(fclVenit, contCreanta);
            var creanta = postariVenit.Where(p => p.Cont == contCreanta.ID).ToList();
            var venitul = postariVenit.SingleOrDefault(p => p.Cont == tipVenit.ContImplicitId);
            s.Check($"STR-FCL-VENIT-TVA ({eticheta}): EXACT o tranzacție `Operare`, cu {(privat ? 4 : 2)} postări "
                + $"({(privat ? "net + taxă pe ambele laturi" : "doar netul — bugetarul e neplătitor")}), "
                + "zero postări de stoc",
                ProbeCub.Tranzactii(os, fclVenit.ID) is [{ Fel: N.FelTranzactie.Operare }]
                && postariVenit.Count == (privat ? 4 : 2)
                && postariVenit.All(p => p.Spatiu != N.Spatiu.Stoc && p.Cantitate == 0m));
            s.Check($"STR-FCL-VENIT-TVA ({eticheta}): creanța e DEBIT pe {contCreanta.Simbol}, fără gestiune, "
                + "pe partida documentului, cu partenerul = clientul",
                creanta.Count == (privat ? 2 : 1)
                && creanta.All(p => p.Latura == N.Latura.Debit && p.Gestiune == null
                    && p.Unitate == partidaVenit
                    && p.Partener == client.ID)
                && creanta.Sum(p => p.Valoare) == (privat ? 242m : 200m));
            s.Check($"STR-FCL-VENIT-TVA ({eticheta}): venitul e CREDIT 200 pe gestiunea emitentului"
                + (privat ? ", cu codul de TVA `Baza`/`Livrare`, perioada declarării și partenerul" : ", fără cod de TVA"),
                venitul is { Latura: N.Latura.Credit, Valoare: 200m } && venitul.Gestiune == emitent.ID
                && venitul.Unitate == null
                && (privat
                    ? venitul.TipTvaId == n21.ID && venitul.SensTva == N.SensTva.Livrare
                        && venitul.RolTva == N.RolTva.Baza && venitul.PerioadaDeclarare != null
                        && venitul.Partener == client.ID
                    : venitul.TipTvaId == null && venitul.PerioadaDeclarare == null));
            if (privat) {
                var taxa = postariVenit.Single(p => p.Cont == cont4427.ID);
                s.Check($"STR-FCL-VENIT-TVA ({eticheta}): taxa COLECTATĂ e CREDIT 42 pe 4427, pe gestiunea "
                    + "emitentului, cu rolul `Taxa` și partenerul; soldul partidei de creanță = 242",
                    taxa is { Latura: N.Latura.Credit, Valoare: 42m } && taxa.Gestiune == emitent.ID
                    && taxa.RolTva == N.RolTva.Taxa && taxa.SensTva == N.SensTva.Livrare
                    && taxa.Partener == client.ID
                    && ProbeCub.SoldPartida(os, partidaVenit) == 242m);
            }
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCL-{eticheta}", fclVenit);

            // --- STR-FCL-STORNO / STR-FCL-ANULARE: pe facturi fără împerecheri ---
            MotorOperare.Opereaza(os, fclStorno);
            MotorOperare.Storneaza(os, fclStorno, dataStornoFcl);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-FCL-{eticheta}", fclStorno, dataStornoFcl);
            MotorOperare.Opereaza(os, fclAnulat);
            s.Check($"STR-FCL-ANULARE ({eticheta}) premisă: documentul are tranzacție și postări în cub",
                ProbeCub.Tranzactii(os, fclAnulat.ID).Count == 1
                && ProbeCub.Postari(os, fclAnulat.ID).Count > 0);
            MotorOperare.AnuleazaOperarea(os, fclAnulat);
            ProbeCub.FaraRanduri(os, s.Check,
                $"STR-FCL-ANULARE ({eticheta}): anularea operării șterge FIZIC tranzacția și postările ei, "
                + "simetric cu registrele",
                fclAnulat.ID);

            // --- STR-FCL-REFUZ: laturile, în dry-run, fără nimic scris ---
            var tipFcl = os.FirstOrDefault<TipDocument>(t => t.ClrType == nameof(FacturaIesire));
            var fclPredatorGresit = Factura(new DateOnly(2026, 3, 8));
            fclPredatorGresit.Predator = client;
            Linie(fclPredatorGresit, tipVenit, 1m, 10m, privat ? n21 : null);
            var fclPrimitorGresit = Factura(new DateOnly(2026, 3, 8));
            fclPrimitorGresit.Primitor = mag1;
            Linie(fclPrimitorGresit, tipVenit, 1m, 10m, privat ? n21 : null);
            os.CommitChanges();
            var refuzPredator = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, fclPredatorGresit, tipFcl);
            var refuzPrimitor = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, fclPrimitorGresit, tipFcl);
            s.Check($"STR-FCL-REFUZ ({eticheta}): predatorul partener dă `PREDATOR_NEPOTRIVIT`, primitorul "
                + "gestiune dă `PRIMITOR_NEPOTRIVIT`, iar dry-run-ul nu scrie nimic în cub",
                refuzPredator.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit, StringComparison.Ordinal))
                && refuzPrimitor.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PrimitorNepotrivit, StringComparison.Ordinal))
                && ProbeCub.Tranzactii(os, fclPredatorGresit.ID).Count == 0
                && ProbeCub.Tranzactii(os, fclPrimitorGresit.ID).Count == 0);

            // Bugetarul n-are DSC (tip inert), n-are partide pe creanță și n-are TVA:
            // restul scenei e al profilului privat.
            if (!privat) {
                CurataNucFclDsc(os);
                s.Check($"NUC-FCLDSC-{eticheta} — curățenie finală (fără reziduuri de scenă)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNucFclDsc)));
                return;
            }

            // --- STR-FCL-AVANS-DOUA-PARTIDE: 4111 și 419, amândouă cu `RolTert` ---
            var fclAvans = Factura(new DateOnly(2026, 3, 9));
            Linie(fclAvans, tipVenit, 1m, 200m, n21);
            Linie(fclAvans, tipAvans, 1m, -100m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclAvans);
            var postariAvans = ProbeCub.Postari(os, fclAvans.ID);
            var partidaAvansCreanta = Partida(fclAvans, contCreanta);
            var partidaAvans419 = Partida(fclAvans, cont419);
            var avansul = postariAvans.SingleOrDefault(p => p.Cont == cont419.ID);
            s.Check($"STR-FCL-AVANS-DOUA-PARTIDE ({eticheta}): documentul deschide DOUĂ partide (090d) — pe "
                + "creanță și pe 419 —, iar regularizarea e `C 419 -100` cu partidă, partener și gestiunea "
                + "emitentului",
                postariAvans.Select(p => p.Unitate).OfType<Guid>().Distinct().Count() == 2
                && avansul is { Latura: N.Latura.Credit, Valoare: -100m }
                && avansul.Unitate == partidaAvans419 && avansul.Partener == client.ID
                && avansul.Gestiune == emitent.ID);
            s.Check($"STR-FCL-AVANS-DOUA-PARTIDE ({eticheta}): partida de creanță ține netul, taxa ȘI debitul "
                + "regularizării (200 + 42 − 100 = 142), iar partida 419 ține doar creditul ei",
                ProbeCub.SoldPartida(os, partidaAvansCreanta) == 142m
                && ProbeCub.SoldPartida(os, partidaAvans419) == 100m);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCL-AVANS-{eticheta}", fclAvans);

            // --- STR-FCL-NEGATIV: valorile declarate SEMNATE, pe aceleași laturi ---
            var fclNegativ = Factura(new DateOnly(2026, 3, 10));
            Linie(fclNegativ, tipVenit, 1m, -50m, n21);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclNegativ);
            var postariNegativ = ProbeCub.Postari(os, fclNegativ.ID);
            s.Check($"STR-FCL-NEGATIV ({eticheta}): prețul unitar negativ se declară CA ATARE — `D 4111 -50`, "
                + "`C 704 -50`, `D 4111 -10,5`, `C 4427 -10,5` —, iar soldul partidei e −60,5",
                postariNegativ.Count == 4
                && postariNegativ.Where(p => p.Cont == contCreanta.ID)
                    .All(p => p.Latura == N.Latura.Debit && p.Valoare < 0m)
                && postariNegativ.Single(p => p.Cont == tipVenit.ContImplicitId) is
                    { Latura: N.Latura.Credit, Valoare: -50m }
                && postariNegativ.Single(p => p.Cont == cont4427.ID) is
                    { Latura: N.Latura.Credit, Valoare: -10.5m }
                && ProbeCub.SoldPartida(os, Partida(fclNegativ, contCreanta)) == -60.5m);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCL-NEGATIV-{eticheta}", fclNegativ);

            // --- STR-FCL-INC-TRANSFER: împerecherea de după operare mută partida (S-D13) ---
            var inc = os.CreateObject<Incasare>();
            inc.Data = new DateOnly(2026, 3, 11);
            inc.Predator = client;
            inc.Primitor = banca;
            var linieInc = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieInc.Document = inc;
            linieInc.TipMaterial = tipTrz;
            linieInc.Valoare = 242m;
            var inc2 = os.CreateObject<Incasare>();
            inc2.Data = new DateOnly(2026, 3, 11);
            inc2.Predator = client;
            inc2.Primitor = banca;
            var linieInc2 = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieInc2.Document = inc2;
            linieInc2.TipMaterial = tipTrz;
            linieInc2.Valoare = 142m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, inc);
            MotorOperare.Opereaza(os, inc2);
            var dataStingerii = new DateOnly(2026, 3, 11);
            var imp = ImperechereService.Imperecheaza(os, inc, fclVenit, 242m, null, dataStingerii);
            var transferInc = ProbeCub.Transferuri(os, inc.ID);
            var postariTransfer = transferInc.Count == 1
                ? ProbeCub.Postari(os, inc.ID).Where(p => p.TranzactieId == transferInc[0].ID).ToList()
                : [];
            s.Check($"STR-FCL-INC-TRANSFER ({eticheta}): încasarea primește o tranzacție `Transfer` cu 2 postări "
                + "pe contul de creanță, pe ACEEAȘI latură — de pe partida ei proprie pe partida facturii "
                + "—, datată cu `Imperechere.Data`; soldul partidei facturii devine 0",
                transferInc.Count == 1 && transferInc[0].Data == imp.Data
                && postariTransfer.Count == 2
                && postariTransfer.All(p => p.Cont == contCreanta.ID && p.Partener == client.ID)
                && postariTransfer.Select(p => p.Latura).Distinct().Count() == 1
                && postariTransfer.Any(p => p.Unitate == partidaVenit)
                && postariTransfer.Any(p => p.Unitate == Partida(inc, contCreanta))
                && ProbeCub.SoldPartida(os, partidaVenit) == 0m);
            ImperechereService.Imperecheaza(os, inc2, fclAvans, 142m, null, dataStingerii);
            s.Check($"STR-FCL-INC-TRANSFER ({eticheta}): pe factura cu avans plafonul e RESTUL partidei de "
                + "referință (142, nu 242) — partida de creanță se golește, iar partida 419 rămâne neatinsă",
                ProbeCub.SoldPartida(os, partidaAvansCreanta) == 0m
                && ProbeCub.SoldPartida(os, partidaAvans419) == 100m
                && ProbeCub.Transferuri(os, inc2.ID).Count == 1);
            ImperechereService.Desfa(os, imp.ID, new DateOnly(2026, 3, 12));
            s.Check($"STR-FCL-INC-TRANSFER ({eticheta}): desfacerea scrie transferul INVERS — partida facturii "
                + "revine la 242, iar încasarea are acum DOUĂ tranzacții `Transfer`",
                ProbeCub.SoldPartida(os, partidaVenit) == 242m
                && ProbeCub.Transferuri(os, inc.ID).Count == 2);

            // --- DSC: loturi cu raport diferit de prețul înghețat ---
            Produs CreeazaProdusDsc(string sufix) {
                var p = os.CreateObject<Produs>();
                p.Cod = MarcajNucFclDsc + sufix;
                p.Denumire = "Produs probă felia 32" + sufix;
                p.UM = "BUC";
                p.TipMaterial = tipMarfa;
                return p;
            }
            Lot Lotul(string sufix, decimal pretUnitar) {
                var lotNou = os.CreateObject<Lot>();
                lotNou.Produs = CreeazaProdusDsc(sufix);
                lotNou.PretUnitar = pretUnitar;
                lotNou.Gestiune = mag1;
                lotNou.Data = new DateOnly(2026, 1, 10);
                return lotNou;
            }
            // (A) prețul înghețat = raportul curent. (B) prețul rămas la 10, raportul 15.
            var lotA = Lotul("-A", 15m);
            var lotB = Lotul("-B", 10m);
            var lotC = Lotul("-C", 15m);
            var lotD = Lotul("-D", 15m);
            os.CommitChanges();
            using (var tx = TranzactieComanda.Incepe(os)) {
                var contStoc = tipMarfa.ContImplicitId.Value;
                var contrapartida = os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891")).ID;
                Atlas.Conta.BackOffice.Module.Cub.Materializare.Deschide(os, new(2026, 1, 20),
                    [new(contStoc, N.Latura.Debit, 900, true), new(contrapartida, N.Latura.Credit, 900)],
                    [new(contStoc, lotA.ID, mag1.ID, 20, 300), new(contStoc, lotB.ID, mag1.ID, 20, 300),
                     new(contStoc, lotC.ID, mag1.ID, 10, 150), new(contStoc, lotD.ID, mag1.ID, 10, 150)], []);
                os.CommitChanges(); tx.Commit();
            }

            // --- STR-DSC-COST-CLIENT: costul pe gestiunea virtuală `Client` ---
            var fclStoc = Factura(new DateOnly(2026, 4, 10), mag1);
            Linie(fclStoc, tipMarfa, 4m, 30m, n21, lotA.Produs);
            os.CommitChanges();
            var dsc = (DescarcareGestiune)MotorOperare.Opereaza(os, fclStoc);
            MotorOperare.Opereaza(os, dsc);
            var postariDsc = ProbeCub.Postari(os, dsc.ID);
            var iesirea = postariDsc.SingleOrDefault(p => p.Spatiu == N.Spatiu.Stoc);
            var costul = postariDsc.SingleOrDefault(p => p.Cont == cont607.ID);
            s.Check($"STR-DSC-COST-CLIENT ({eticheta}): descărcarea scrie EXACT o `Operare` cu 2 postări — "
                + "lotul iese pe contul de stoc al gestiunii predatoare, costul intră pe 607 —, la valoarea "
                + "raportului curent (60)",
                ProbeCub.Tranzactii(os, dsc.ID) is [{ Fel: N.FelTranzactie.Operare }]
                && postariDsc.Count == 2
                && iesirea is { Latura: N.Latura.Credit, Valoare: 60m, Cantitate: -4m }
                && iesirea.Unitate == lotA.ID && iesirea.Gestiune == mag1.ID);
            s.Check($"STR-DSC-COST-CLIENT ({eticheta}): capătul de cost poartă `+4` pe gestiunea VIRTUALĂ a "
                + "clientului (090g), fără unitate — marfa a părăsit patrimoniul —, cu produsul lotului și cu "
                + "terțul de pe primitor (T-D13 g); descărcarea n-are partidă (607/371 n-au rol de terț), iar "
                + "ieșirea de stoc n-are partener",
                costul is { Latura: N.Latura.Debit, Valoare: 60m, Cantitate: 4m }
                && costul.Gestiune == N.GestiuniVirtuale.Client && costul.Unitate == null
                && costul.Produs == lotA.Produs.ID
                && costul.Partener == client.ID
                && iesirea.Partener == null);
            s.Check($"STR-DSC-COST-CLIENT ({eticheta}): factura are în cub DOAR venitul — `4111 = 707` (120) și "
                + "taxa (25,2) —, zero postări de stoc; `Sold` pe lot din postările descărcării = "
                + $"{ProbeCub.SoldUnitate(os, dsc.ID, lotA.ID)}",
                ProbeCub.Postari(os, fclStoc.ID) is { Count: 4 } aleFacturii
                && aleFacturii.All(p => p.Spatiu != N.Spatiu.Stoc)
                && aleFacturii.Single(p => p.Cont == cont707.ID).Valoare == 120m);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCL-STOC-{eticheta}", fclStoc);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-DSC-{eticheta}", dsc);

            // --- STR-DSC-GOLIRE (= STR-DSC-FARA-SURSA): manuală, fără sursă, golind lotul ---
            DescarcareGestiune Descarcare(DateOnly data, Lot lot, decimal cantitate, Repartitor predator = null) {
                var doc = os.CreateObject<DescarcareGestiune>();
                doc.Data = data;
                doc.Predator = predator ?? mag1;
                doc.Primitor = client;
                var d = os.CreateObject<DescarcareGestiuneDetaliu>();
                d.Document = doc;
                d.TipMaterial = tipMarfa;
                d.Lot = lot;
                d.Cantitate = cantitate;
                return doc;
            }
            var dscGolire = Descarcare(new DateOnly(2026, 4, 11), lotB, 20m);
            var dscStorno = Descarcare(new DateOnly(2026, 4, 12), lotC, 10m);
            var dscAnulat = Descarcare(new DateOnly(2026, 4, 13), lotD, 10m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, dscGolire);
            var postariGolire = ProbeCub.Postari(os, dscGolire.ID);
            s.Check($"STR-DSC-GOLIRE ({eticheta}): descărcarea MANUALĂ (fără `DocumentSursa`) are aceeași formă "
                + "ca cea autogenerată, iar linia care golește lotul duce tot restul valoric (300, pe când "
                + $"cantitatea x prețul înghețat {lotB.PretUnitar} ar fi dat 200)",
                !dscGolire.Autogenerat && dscGolire.DocumentSursaId == null
                && postariGolire.Count == 2 && postariGolire.All(p => p.Valoare == 300m)
                && postariGolire.Single(p => p.Spatiu == N.Spatiu.Stoc).Gestiune == mag1.ID
                && postariGolire.Single(p => p.Cont == cont607.ID).Gestiune == N.GestiuniVirtuale.Client);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-DSC-GOLIRE-{eticheta}", dscGolire);

            // --- STR-DSC-STORNO / STR-DSC-ANULARE ---
            MotorOperare.Opereaza(os, dscStorno);
            MotorOperare.Storneaza(os, dscStorno, dataStornoFcl);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-DSC-{eticheta}", dscStorno, dataStornoFcl);
            MotorOperare.Opereaza(os, dscAnulat);
            s.Check($"STR-DSC-ANULARE ({eticheta}) premisă: documentul are tranzacție și postări în cub",
                ProbeCub.Tranzactii(os, dscAnulat.ID).Count == 1
                && ProbeCub.Postari(os, dscAnulat.ID).Count == 2);
            MotorOperare.AnuleazaOperarea(os, dscAnulat);
            ProbeCub.FaraRanduri(os, s.Check,
                $"STR-DSC-ANULARE ({eticheta}): anularea operării șterge FIZIC tranzacția și postările ei",
                dscAnulat.ID);

            // --- STR-DSC-REFUZ ---
            var tipDsc = os.FirstOrDefault<TipDocument>(t => t.ClrType == nameof(DescarcareGestiune));
            var dscFaraLot = Descarcare(new DateOnly(2026, 4, 14), lotA, 1m);
            var dscPredatorGresit = Descarcare(new DateOnly(2026, 4, 14), lotA, 1m, client);
            os.CommitChanges();
            dscFaraLot.Detalii.Single().Lot = null;
            os.CommitChanges();
            var refuzFaraLot = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, dscFaraLot, tipDsc);
            var refuzPredatorDsc = Atlas.Conta.BackOffice.Module.Cub.Materializare.Refuzuri(os, dscPredatorGresit, tipDsc);
            s.Check($"STR-DSC-REFUZ ({eticheta}): linia fără lot dă `LOT_LIPSA`, predatorul partener dă "
                + "`PREDATOR_NEPOTRIVIT`, iar dry-run-ul nu scrie nimic în cub",
                refuzFaraLot.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.LotLipsa, StringComparison.Ordinal))
                && refuzPredatorDsc.Any(m => m.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit, StringComparison.Ordinal))
                && ProbeCub.Tranzactii(os, dscFaraLot.ID).Count == 0
                && ProbeCub.Tranzactii(os, dscPredatorGresit.ID).Count == 0);
        }

        CurataNucFclDsc(os);
        s.Check($"NUC-FCLDSC-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNucFclDsc))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajNucFclDsc))
            && !os.GetObjectsQuery<TipMaterial>().Any(t => t.Cod.StartsWith(MarcajNucFclDsc)));
        } finally {
            using var curatenie = s.Provider.CreateObjectSpace();
            CurataNucFclDsc(curatenie);
        }
    }
}
