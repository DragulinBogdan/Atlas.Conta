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

// ====== Felia 30, pasul 4: declarantul de trezorerie pe scenă privată ======
// Scenele PLT/INC existente (e2e 3c, felia Api Trz, felia 7) sunt BUGETARE, iar
// pe bugetar niciun cont n-are `RolTert` (`SeedRolTert` e privat, D16-V1): tot
// miezul lui B-D5 — partida, partenerul pe piciorul de terț, nominalizarea
// TR-D2a — rămânea probat într-un singur document (plata din P1). Scena de aici
// îl probează pe toate cele patru forme, pe profilul care are partide.
//
// Cazul SPLIT (plata mai mare decât restul sursei) e singurul din pilot în care
// declarantul și oracolul NU coincid: pin-ul 3 cere ca partea nealocată să fie o
// a doua mișcare, iar o mișcare are DOUĂ capete, deci se sparge și piciorul de
// bani; `TrD2NominalizeazaPrinImperechere` sparge doar postările cu partidă.
// Diferența se CONSEMNEAZĂ aici (ca N-r3 la pasul 3), nu se normalizează — B-D10 (b).
static class VerificaNucleuTrezorerie {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajNucTrz = "E2E-NUC-TRZ";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";

        void CurataNucTrz(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucTrz)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            ProbeCub.Purjeaza(pj, os, docIds);                                             // S-D8
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Copiii (plata autogenerată, latura pereche) înaintea părinților.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucTrz)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(x => x.Cod.StartsWith(MarcajNucTrz)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucTrz)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataNucTrz(os);
        try {
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
        var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
        var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
        var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "628" : "628.00.00"));
        var tipStocTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.00.00"));
        var tipImobilizareTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "2131" : "213.01.00"));
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "401" : "401.01.00"));
        var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "404" : "404.01.00"));
        var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "4111" : "411.01.01"));
        s.Check($"NUC-TRZ-{eticheta} scenă: profilul are conturi cu `RolTert` (401 furnizor, 4111 client) — "
            + "fără ele partida nici nu se deschide (B-D8 pct. 10)",
            cont401?.RolTert == RolTertCont.Furnizor && cont4111?.RolTert == RolTertCont.Client);

        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = MarcajNucTrz + "-FURN";
        furnizor.Denumire = "Furnizor probă felia 30";
        var client = os.CreateObject<Partener>();
        client.Cod = MarcajNucTrz + "-CL";
        client.Denumire = "Client probă felia 30";
        var produsTrz = os.CreateObject<Produs>();
        produsTrz.Cod = MarcajNucTrz + "-P";
        produsTrz.Denumire = "Produs probă felia 31 (TRZ)";
        produsTrz.UM = "BUC";
        produsTrz.TipMaterial = tipStocTrz;
        var casaVir = os.CreateObject<ContPropriu>();
        casaVir.Cod = MarcajNucTrz + "-CASA";
        casaVir.Denumire = "Casa probă felia 30";
        casaVir.ContImplicit = casa.ContImplicit;
        var bancaVir = os.CreateObject<ContPropriu>();
        bancaVir.Cod = MarcajNucTrz + "-BANCA";
        bancaVir.Denumire = "Banca probă felia 30";
        bancaVir.ContImplicit = banca.ContImplicit;
        bancaVir.EsteBanca = true;
        os.CommitChanges();

        T Trezorerie<T>(Repartitor predator, Repartitor primitor, TipMaterial tip, decimal valoare, DateOnly data)
                where T : DocumentTrezorerie {
            var doc = os.CreateObject<T>();
            doc.Data = data;
            doc.Predator = predator;
            doc.Primitor = primitor;
            var d = os.CreateObject<DocumentTrezorerieDetaliu>();
            d.Document = doc;
            d.TipMaterial = tip;
            d.Valoare = valoare;
            os.CommitChanges();
            return doc;
        }
        Guid PartidaDin(Document doc, Cont cont, Repartitor partener) =>
            N.Unitate.DeschidePartida(cont.ID, partener.ID, doc.ID, doc.DataInregistrare).Id;
        List<Guid?> UnitatiPe(N.Contract contract, Cont cont) =>
            [.. contract.Tranzactii.SelectMany(t => t.Postari)
                .Where(p => p.Coordonate.Cont == cont.ID).Select(p => p.Coordonate.Unitate?.Id)];

        // --- (1) plata manuală către furnizor: partida PROPRIE pe 401 ---
        var plt = Trezorerie<Plata>(casa, furnizor, tipTrz, 60m, new DateOnly(2026, 3, 5));
        MotorOperare.Opereaza(os, plt);
        s.Check($"NUC-PLT-{eticheta} scenă: plată manuală 60 către furnizor — o notă 401 = 5311, fără stingere",
            CubScena.Note(os, plt.ID).Nota(cont401.ID, casa.ContImplicitId, 60m)
            && !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == plt.ID));
        ProbeNucleu.Proba(os, s.Check, $"NUC-PLT-{eticheta}", [plt]);
        var contractPlt = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, plt);
        s.Check($"NUC-PLT-{eticheta}: fără document-sursă, postarea de terț numește partida PROPRIE a plății "
            + "pe 401, iar piciorul de bani (5311) rămâne fără unitate",
            contractPlt.EsteAcceptat
            && UnitatiPe(contractPlt, cont401) is [var unitatePlt] && unitatePlt == PartidaDin(plt, cont401, furnizor)
            && contractPlt.Decizii.OfType<N.PartidaDeschisa>().Count() == 1
            && contractPlt.Tranzactii.SelectMany(t => t.Postari).Count(p => p.Coordonate.Unitate == null) == 1);

        // --- (2) încasare de la client: partida proprie pe 4111 ---
        var inc = Trezorerie<Incasare>(client, casa, tipTrz, 80m, new DateOnly(2026, 3, 5));
        MotorOperare.Opereaza(os, inc);
        s.Check($"NUC-INC-{eticheta} scenă: încasare manuală 80 de la client — o notă 5311 = 4111",
            CubScena.Note(os, inc.ID).Nota(casa.ContImplicitId, cont4111.ID, 80m));
        ProbeNucleu.Proba(os, s.Check, $"NUC-INC-{eticheta}", [inc]);
        var contractInc = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, inc);
        s.Check($"NUC-INC-{eticheta}: oglinda — partida proprie stă pe piciorul de CREDIT (4111), fără ca "
            + "declarantul să știe că e încasare (latura o dă `RolTert`-ul contului rezolvat)",
            contractInc.EsteAcceptat
            && UnitatiPe(contractInc, cont4111) is [var unitateInc] && unitateInc == PartidaDin(inc, cont4111, client)
            && contractInc.Tranzactii.SelectMany(t => t.Postari).Single(p => p.Coordonate.Unitate != null).Coordonate.Latura == N.Latura.Credit);

        // --- (3) SPLIT: plata autogenerată e mai mare decât restul facturii ---
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = MarcajNucTrz + "-F1";
        fct.Data = new DateOnly(2026, 3, 3);
        fct.Predator = furnizor;
        fct.Primitor = mag1;
        fct.GenereazaPlata = true;
        fct.PlataContPropriu = casa;
        fct.PlataNumar = MarcajNucTrz + "-OP";
        fct.PlataData = new DateOnly(2026, 3, 10);
        var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
        linieServiciu.Document = fct;
        linieServiciu.TipMaterial = tipServicii;
        linieServiciu.Cantitate = 1m;
        linieServiciu.PretUnitar = 100m;
        linieServiciu.TipTva = n21;
        os.CommitChanges();
        // `Opereaza` întoarce conexul SAU secundarul: fără linie de stoc nu e NIR, deci
        // ce iese e draftul plății autogenerate.
        var secundar = MotorOperare.Opereaza(os, fct);
        var plataAuto = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fct.ID);
        s.Check($"NUC-PLT-SPLIT ({eticheta}) scenă: FCT numai de servicii ⇒ fără NIR conex, iar operarea "
            + "întoarce SECUNDARUL (draftul plății); brutul facturii = 121",
            secundar is Plata && secundar.ID == plataAuto.ID && fct.Total == 121m
            && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == fct.ID && d.ID != plataAuto.ID));

        // --- NUC-FCT-SERV (B-D6, pas 5): factura FĂRĂ linie de stoc — niciun capăt virtual,
        //     deci nici cantitate; netul și taxa cad pe aceeași partidă de 401 ---
        ProbeNucleu.Proba(os, s.Check, $"NUC-FCT-SERV-{eticheta}", [fct]);

        var plataPartiala = Trezorerie<Plata>(casa, furnizor, tipTrz, 60m, new DateOnly(2026, 3, 5));
        MotorOperare.Opereaza(os, plataPartiala);
        ImperechereService.Imperecheaza(os, plataPartiala, fct, 60m, data: new DateOnly(2026, 3, 20));
        var restInainte = ImperechereService.Ramas(os, fct.ID);
        s.Check($"NUC-PLT-SPLIT-1 ({eticheta}): restul facturii (61) e MAI MIC decât plata autogenerată (121) — "
            + "cazul în care o linie se împarte între partida stinsă și partida proprie",
            restInainte == 61m && plataAuto.Detalii.Single().Valoare == 121m);

        MotorOperare.Opereaza(os, plataAuto);
        var impAuto = os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID);
        s.Check($"NUC-PLT-SPLIT-2 ({eticheta}): motorul vechi se plafonează la restul sursei — împerecherea "
            + $"automată e {impAuto.Suma}, nu 121, iar nota rămâne UNA de 121",
            impAuto.Suma == 61m);

        var contractSplit = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, plataAuto);
        var partidaFct = PartidaDin(fct, cont401, furnizor);
        var partidaProprie = PartidaDin(plataAuto, cont401, furnizor);
        s.Check($"NUC-PLT-SPLIT-3 ({eticheta}): declarantul împarte linia — 61 pe partida FACTURII (nominalizare "
            + "TR-D2a) + 60 pe partida PROPRIE a plății, în ordinea liniilor; ipoteza consemnează soldul REAL "
            + "al partidei (121 credit pe 401), iar plafonul nominalizării e restul documentului (61) — MAJOR-1",
            contractSplit.EsteAcceptat
            && contractSplit.Tranzactii.SelectMany(t => t.Postari).Any(p => p.Coordonate.Unitate?.Id == partidaFct && p.Valoare == 61m)
            && contractSplit.Tranzactii.SelectMany(t => t.Postari).Any(p => p.Coordonate.Unitate?.Id == partidaProprie && p.Valoare == 60m)
            && contractSplit.Decizii.OfType<N.AlocareFifo>().Single().Masura == 61m
            && contractSplit.Ipoteze.OfType<N.SoldUnitateCitit>().Single() is { } citit
            && citit.Sold.Credit == 121m && citit.Unitate.Id == partidaFct);

        ProbeNucleu.Proba(os, s.Check, $"NUC-PLT-SPLIT", [plataAuto]);

        s.Check($"NUC-PLT-SPLIT-5 ({eticheta}): declarația se conservă oricum — suma celor două bucăți e "
            + "valoarea liniei, iar `Conservare.Verifica` e gol",
            contractSplit.Tranzactii.SelectMany(N.Conservare.Verifica).Count() == 0
            && contractSplit.Tranzactii.SelectMany(t => t.Postari).Where(p => p.Coordonate.Cont == cont401.ID).Sum(p => p.Valoare) == 121m);

        // --- (4) viramentul intern pe profilul cu partide ---
        var virPlt = Trezorerie<Plata>(casaVir, bancaVir, tipVir, 500m, new DateOnly(2026, 3, 11));
        var virInc = MotorOperare.Opereaza(os, virPlt);
        s.Check($"NUC-PLT-VIR-{eticheta} scenă: piciorul de ieșire postează 581 = 5311 și naște latura pereche",
            virInc is Incasare { Stare: StareDocument.Draft });
        ProbeNucleu.Proba(os, s.Check, $"NUC-PLT-VIR-{eticheta}", [virPlt]);
        MotorOperare.Opereaza(os, virInc);
        ProbeNucleu.Proba(os, s.Check, $"NUC-INC-VIR-{eticheta}", [virInc]);
        var contractVir = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, virPlt);
        s.Check($"NUC-PLT-VIR-{eticheta}: la virament ambele capete poartă contul propriu AL PICIORULUI ca "
            + "gestiune și NICIUNUL o partidă (581/5311 n-au `RolTert`) — `GetContPropriuId` reprodus din "
            + "sursa declarată a regulii, fără `is` pe tipul documentului",
            contractVir.EsteAcceptat
            && contractVir.Tranzactii.SelectMany(t => t.Postari).All(p => p.Coordonate.Gestiune == casaVir.ID
                && p.Coordonate.Unitate == null && p.Coordonate.Partener == null));

        // --- Felia 31 (TR-D7a), S-D8: cubul PERSISTAT pe PLT/INC ---
        var pltCub = Trezorerie<Plata>(casa, furnizor, tipTrz, 40m, new DateOnly(2026, 3, 13));
        var incCub = Trezorerie<Incasare>(client, casa, tipTrz, 30m, new DateOnly(2026, 3, 13));
        var dataStornoTrz = new DateOnly(2026, 7, 22);
        {
            MotorOperare.Opereaza(os, pltCub);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-PLT-{eticheta}", pltCub);
            MotorOperare.Opereaza(os, incCub);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-INC-{eticheta}", incCub);
            MotorOperare.Storneaza(os, pltCub, dataStornoTrz);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-PLT-{eticheta}", pltCub, dataStornoTrz);
        }

        // ===== Felia 31 (TR-D7a) pasul 5, S-D13: imperecherea ca tranzactie `Transfer` =====
        s.Check($"STR-TRANSFER-6 ({eticheta}): împerecherea AUTOMATă la operare (plata autogenerată din "
            + "FCT) NU produce transfer — nominalizarea e deja în `Operare` (TR-D2a)",
            ProbeCub.Transferuri(os, plataAuto.ID).Count == 0
            && ProbeCub.Tranzactii(os, plataAuto.ID).Count(t => t.Fel == N.FelTranzactie.Operare) == 1);

        var pltT = Trezorerie<Plata>(casa, furnizor, tipTrz, 100m, new DateOnly(2026, 3, 14));
        MotorOperare.Opereaza(os, pltT);
        var partidaProprieT = PartidaDin(pltT, cont401, furnizor);
        s.Check($"STR-TRANSFER-1 ({eticheta}): plata manuală (fără sursă) operată pe tip migrat scrie EXACT "
            + "o `Operare` cu partida PROPRIE pe 401, de 100",
            ProbeCub.Tranzactii(os, pltT.ID) is [{ Fel: N.FelTranzactie.Operare }]
            && ProbeCub.Postari(os, pltT.ID).Count(p => p.Unitate == partidaProprieT) == 1
            && ProbeCub.SoldPartida(os, partidaProprieT) == 100m);

        var fctT = os.CreateObject<FacturaIntrare>();
        fctT.Numar = MarcajNucTrz + "-F2";
        fctT.Data = new DateOnly(2026, 3, 4);
        fctT.Predator = furnizor;
        fctT.Primitor = mag1;
        var linieT = os.CreateObject<FacturaIntrareDetaliu>();
        linieT.Document = fctT;
        linieT.TipMaterial = tipServicii;
        linieT.Cantitate = 1m;
        linieT.PretUnitar = 200m;
        linieT.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctT);
        var partidaFctT = PartidaDin(fctT, cont401, furnizor);
        s.Check($"STR-TRANSFER-2 ({eticheta}) scenă: factura de 242 e operată și migrată — partida ei pe 401 "
            + "ține 242 pe credit, plata 100 pe debit",
            ProbeCub.SoldPartida(os, partidaFctT) == -242m && fctT.Total == 242m);

        // Data împerecherii e în ALTĂ LUNĂ decât a documentelor: transferul se datează
        // după FAPTUL de stingere, nu după `max(DataInregistrare)` (MAJOR-B).
        var impT = ImperechereService.Imperecheaza(os, pltT, fctT, 100m, data: new DateOnly(2026, 4, 10));
        var transferT = ProbeCub.Transferuri(os, pltT.ID);
        var postariT = transferT.Count == 1
            ? ProbeCub.Postari(os, pltT.ID).Where(p => p.TranzactieId == transferT[0].ID).ToList()
            : [];
        s.Check($"STR-TRANSFER-2 ({eticheta}): `ImperechereService.Creeaza` după operare scrie EXACT o "
            + "`Tranzactie(Transfer)` pe STINGĂTOR, cu două postări pe 401 (−100 de pe partida proprie, "
            + "+100 pe a facturii), conservată pe (cont, latură)",
            transferT.Count == 1
            && transferT[0].Data == impT.Data
            && postariT.All(p => p.Data == impT.Data)
            && postariT.Count == 2
            && postariT.All(p => p.Cont == cont401.ID && p.Latura == N.Latura.Debit && p.Partener == furnizor.ID)
            && postariT.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) == 0m
            && postariT.Single(p => p.Valoare < 0m).Unitate == partidaProprieT
            && postariT.Single(p => p.Valoare > 0m).Unitate == partidaFctT);
        s.Check($"STR-TRANSFER-2 ({eticheta}): `Sold` per partidă din cub — partida facturii e stinsă cu 100 "
            + "(−142), partida proprie a plății rămâne 0",
            ProbeCub.SoldPartida(os, partidaFctT) == -142m
            && ProbeCub.SoldPartida(os, partidaProprieT) == 0m);


        var refuzPlafon = s.Refuz(() => ImperechereService.Imperecheaza(
            os, pltT, fctT, 1m, data: new DateOnly(2026, 4, 10)));
        s.Check($"STR-TRANSFER-2 ({eticheta}): a doua stingere de pe aceeași plată e refuzată — plafonul "
            + $"documentului o oprește înaintea partidei ({refuzPlafon?.Split('\n')[0]})",
            refuzPlafon != null && ProbeCub.Transferuri(os, pltT.ID).Count == 1);

        var inversT = ImperechereService.Desfa(os, impT.ID, new DateOnly(2026, 4, 11));
        var dupaDesfacere = ProbeCub.Transferuri(os, pltT.ID);
        s.Check($"STR-TRANSFER-4 ({eticheta}): `Desfa` scrie transferul INVERS (a treia tranzacție), datat la "
            + "ziua rândului INVERS de împerechere (MAJOR-B), iar Σ per partidă revine: factura la −242, "
            + "partida proprie la 100",
            dupaDesfacere.Count == 2
            && dupaDesfacere.Single(t => t.ID != transferT[0].ID).Data == inversT.Data
            && inversT.Data == new DateOnly(2026, 4, 11)
            && ProbeCub.SoldPartida(os, partidaFctT) == -242m
            && ProbeCub.SoldPartida(os, partidaProprieT) == 100m);

        var impT2 = ImperechereService.Imperecheaza(os, pltT, fctT, 100m, data: new DateOnly(2026, 4, 12));
        s.Check($"STR-TRANSFER-5 ({eticheta}): re-împerecherea scrie al patrulea transfer, partida facturii "
            + "e iar stinsă cu 100",
            ProbeCub.Transferuri(os, pltT.ID).Count == 3
            && ProbeCub.SoldPartida(os, partidaFctT) == -142m);
        var refuzStorno = s.Refuz(() => MotorOperare.Storneaza(os, fctT, new DateOnly(2026, 4, 13)));
        s.Check($"STR-TRANSFER-5 ({eticheta}): stornarea stinsului cu împerechere VIE într-o perioadă DESCHISă "
            + "e refuzată de gardianul F27-D8 — cubul rămâne neatins; inversul la storno "
            + "(`InverseazaLaStorno` → `CreeazaInvers`) e calea din perioada îNCHISĂ, aceeași cu `Desfa`",
            refuzStorno != null
            && ProbeCub.Transferuri(os, pltT.ID).Count == 3
            && ProbeCub.Tranzactii(os, fctT.ID).All(t => t.Fel == N.FelTranzactie.Operare));
        ImperechereService.Desfa(os, impT2.ID, new DateOnly(2026, 4, 14));
        MotorOperare.Storneaza(os, fctT, new DateOnly(2026, 4, 15));
        s.Check($"STR-TRANSFER-5 ({eticheta}): după desfacere, stornarea facturii trece — factura are `Storno` "
            + "în cub, iar cele patru transferuri ale plății se anulează două câte două (Σ partidă = 0)",
            ProbeCub.Tranzactii(os, fctT.ID).Count(t => t.Fel == N.FelTranzactie.Storno) == 1
            && ProbeCub.SoldPartida(os, partidaFctT) == 0m
            && ProbeCub.SoldPartida(os, partidaProprieT) == 100m);

        // --- STR-TRANSFER-2 re-tăiat (MAJOR-A): FCT cu linie de STOC ---
        // Recepția stă pe NIR-ul CONEX în registre și pe partida facturii în cub (TR-D3):
        // plafonul stingerii e brutul INTEGRAL, nu doar taxa + serviciile.
        var fctS = os.CreateObject<FacturaIntrare>();
        fctS.Numar = MarcajNucTrz + "-F3";
        fctS.Data = new DateOnly(2026, 3, 6);
        fctS.Predator = furnizor;
        fctS.Primitor = mag1;
        var linieStocS = os.CreateObject<FacturaIntrareDetaliu>();
        linieStocS.Document = fctS;
        linieStocS.TipMaterial = tipStocTrz;
        linieStocS.Cantitate = 5m;
        linieStocS.PretUnitar = 10m;
        linieStocS.TipTva = n21;
        var linieServiciuS = os.CreateObject<FacturaIntrareDetaliu>();
        linieServiciuS.Document = fctS;
        linieServiciuS.TipMaterial = tipServicii;
        linieServiciuS.Cantitate = 1m;
        linieServiciuS.PretUnitar = 200m;
        linieServiciuS.TipTva = n21;
        linieStocS.CreeazaLot(os, produsTrz, mag1);
        os.CommitChanges();
        var nirS = MotorOperare.Opereaza(os, fctS);
        MotorOperare.Opereaza(os, nirS);
        var partidaFctS = PartidaDin(fctS, cont401, furnizor);
        s.Check($"STR-TRANSFER-2 ({eticheta}) scenă: factura de 302,5 (50 stoc + 200 servicii + 52,5 TVA) — în "
            + "REGISTRE 401 se împarte între factură (252,5) și NIR-ul conex (50), în CUB partida ei ține "
            + "brutul INTEGRAL (recepția e a facturii, TR-D3)",
            nirS is NIR { Stare: StareDocument.Operat } && fctS.Total == 302.5m
            && ProbeCub.SoldPartida(os, partidaFctS) == -302.5m);

        var pltS = Trezorerie<Plata>(casa, furnizor, tipTrz, 302.5m, new DateOnly(2026, 3, 16));
        MotorOperare.Opereaza(os, pltS);
        var partidaProprieS = PartidaDin(pltS, cont401, furnizor);
        var impS = ImperechereService.Imperecheaza(os, pltS, fctS, 302.5m, data: new DateOnly(2026, 4, 16));
        var transferS = ProbeCub.Transferuri(os, pltS.ID);
        s.Check($"STR-TRANSFER-2 ({eticheta}): plata pe brutul INTEGRAL mută 302,5 dintr-o singură bucată — "
            + "partida facturii e stinsă la 0, iar partida proprie a plății rămâne 0 (MAJOR-A: plafonul e "
            + "soldul cu conexul absorbit, nu doar rândurile proprii ale facturii)",
            transferS.Count == 1 && transferS[0].Data == impS.Data
            && ProbeCub.Postari(os, pltS.ID).Count(p => p.TranzactieId == transferS[0].ID
                && p.Unitate == partidaFctS && p.Valoare == 302.5m) == 1
            && ProbeCub.SoldPartida(os, partidaFctS) == 0m
            && ProbeCub.SoldPartida(os, partidaProprieS) == 0m);

        // --- STR-TRANSFER-7 (MEDIU-3): plafonul e RESTUL partidei, nu soldul ei întreg ---
        // Factura de imobilizare ține netul pe 404 și taxa pe 401, deci totalul de stins
        // (605) e mai mare decât ce ține partida de referință (105): a doua stingere
        // trece de plafonul DOCUMENTULUI, dar nu și de restul partidei.
        var fct7 = os.CreateObject<FacturaIntrare>();
        fct7.Numar = MarcajNucTrz + "-F7";
        fct7.Data = new DateOnly(2026, 3, 7);
        fct7.Predator = furnizor;
        fct7.Primitor = mag1;
        var linie7 = os.CreateObject<FacturaIntrareDetaliu>();
        linie7.Document = fct7;
        linie7.TipMaterial = tipImobilizareTrz;
        linie7.Cantitate = 1m;
        linie7.PretUnitar = 500m;
        linie7.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fct7);
        var partidaFct7 = PartidaDin(fct7, cont401, furnizor);
        s.Check($"STR-TRANSFER-7 ({eticheta}) scenă: factura de imobilizare ține 500 pe 404 și doar taxa (105) "
            + "pe 401, iar restul stingibil al DOCUMENTULUI e 605",
            ProbeCub.SoldPartida(os, partidaFct7) == -105m
            && ImperechereService.Ramas(os, fct7.ID) == 605m
            && cont404 != null && ProbeCub.SoldPartida(
                os, PartidaDin(fct7, cont404, furnizor)) == -500m);

        var plt7a = Trezorerie<Plata>(casa, furnizor, tipTrz, 60m, new DateOnly(2026, 3, 18));
        MotorOperare.Opereaza(os, plt7a);
        ImperechereService.Imperecheaza(os, plt7a, fct7, 60m, data: new DateOnly(2026, 4, 18));
        s.Check($"STR-TRANSFER-7 ({eticheta}): prima stingere (60) intră întreagă — partida de 105 mai ține 45",
            ProbeCub.Transferuri(os, plt7a.ID).Count == 1
            && ProbeCub.SoldPartida(os, partidaFct7) == -45m);

        var plt7b = Trezorerie<Plata>(casa, furnizor, tipTrz, 200m, new DateOnly(2026, 3, 19));
        MotorOperare.Opereaza(os, plt7b);
        var partidaProprie7b = PartidaDin(plt7b, cont401, furnizor);
        s.CheckRefuza($"STR-TRANSFER-7/101 ({eticheta}): cererea 200 peste partida disponibilă 45 este refuzată integral",
            () => ImperechereService.Imperecheaza(os, plt7b, fct7, 200m, data: new DateOnly(2026, 4, 19)));
        s.Check($"STR-TRANSFER-7/101 ({eticheta}): refuzul nu scrie legătură sau transfer",
            ProbeCub.Transferuri(os, plt7b.ID).Count == 0
            && !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == plt7b.ID)
            && ProbeCub.SoldPartida(os, partidaFct7) == -45m && ProbeCub.SoldPartida(os, partidaProprie7b) == 200m);
        ImperechereService.Imperecheaza(os, plt7b, fct7, 45m, data: new DateOnly(2026, 4, 19));
        var transfer7b = ProbeCub.Transferuri(os, plt7b.ID);
        s.Check($"STR-TRANSFER-7 ({eticheta}): cererea explicită 45 stinge exact RESTUL partidei; "
            + "factura ajunge la 0 pe 401, iar plata păstrează 155 pe partida proprie (101)",
            transfer7b.Count == 1
            && ProbeCub.Postari(os, plt7b.ID).Count(p => p.TranzactieId == transfer7b[0].ID
                && p.Unitate == partidaFct7 && p.Valoare == 45m) == 1
            && ProbeCub.SoldPartida(os, partidaFct7) == 0m
            && ProbeCub.SoldPartida(os, partidaProprie7b) == 155m);

        // B-r2: latura pe care tipul cere contul propriu
        var pltInvers = os.CreateObject<Plata>();
        pltInvers.Data = new DateOnly(2026, 3, 15);
        pltInvers.Predator = furnizor;
        pltInvers.Primitor = casa;
        var linieInvers = os.CreateObject<DocumentTrezorerieDetaliu>();
        linieInvers.Document = pltInvers;
        linieInvers.TipMaterial = tipTrz;
        linieInvers.Valoare = 10m;
        os.CommitChanges();
        var contractInvers = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, pltInvers);
        s.Refuz(() => MotorOperare.Opereaza(os, pltInvers));
        s.Check($"STR-LATURA ({eticheta}): T-D13 — plata cu laturile inversate (partenerul PREDATOR) e "
            + "refuzată de contractul laturilor cu `PREDATOR_NEPOTRIVIT`, înaintea declarantului și ca singur "
            + "refuz (contul propriu e permis pe primitorul plății: viramentul); nimic scris în cub",
            !contractInvers.EsteAcceptat
            && contractInvers.Refuzuri is [{ Cod: Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit }]
            && ProbeCub.Tranzactii(os, pltInvers.ID).Count == 0
            && pltInvers.Stare == StareDocument.Draft);


        } finally {
            using var curatenie = s.Provider.CreateObjectSpace();
            CurataNucTrz(curatenie);
        }
        s.Check($"NUC-TRZ-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNucTrz))
            && !os.GetObjectsQuery<Document>().Any(d => d.Numar != null && d.Numar.StartsWith(MarcajNucTrz)));
    }
}
