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

// ====== Felia 30, pasul 5: declarantul FCT pe scenă privată ======
// Scenele FCT existente probează un regim per document; aici stau cazurile pe care
// B-D6 le pinuiește și care nu există nicăieri altundeva: PATRU regimuri pe același
// document (cu recepția TR-D3 între ele), factura de imobilizare (404 pentru net,
// 401 pentru taxă — două conturi de terț, deci două partide) și N-r4, singurul caz
// din pilot în care taxa decisă pe DOCUMENT diferă de suma taxelor per linie.
//
// Ca la N-r3 (pasul 3), diferența N-r4 se CONSEMNEAZĂ numeric, nu se normalizează:
// lista B-D8 e închisă (B-D10 (b)).
static class VerificaNucleuFct {
    public static void Ruleaza(Suita s, bool privat) {
        const string MarcajNucFct = "E2E-NUC-FCT";
        var eticheta = privat ? "PRIVAT" : "BUGETAR";

        void CurataNucFct(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucFct)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)
                    || d.Numar.StartsWith(MarcajNucFct)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            ProbeCub.Purjeaza(pj, os, docIds);                                             // S-D8
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Conexele (NIR) înaintea părinților.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            os.CommitChanges();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajNucFct)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(x => x.Cod.StartsWith(MarcajNucFct)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imobilizare>()
                .Where(f => f.NumarInventar.StartsWith(MarcajNucFct)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajNucFct)).ToList());
            pj.Executa();
        }

        using var os = s.Provider.CreateObjectSpace();
        CurataNucFct(os);

        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var tipStoc = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
        var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tipImobilizare = os.FirstOrDefault<TipMaterial>(t => t.Cod == "2131");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var sfd = os.FirstOrDefault<TipTva>(t => t.Cod == "SFD");
        var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
        Cont ContSimbolFct(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
        var cont401 = ContSimbolFct("401");
        var cont404 = ContSimbolFct("404");
        var cont4426 = ContSimbolFct("4426");
        s.Check($"NUC-FCT-{eticheta} scenă: profilul are cele patru regimuri (N21 normal, SFD scutit, NED21 "
            + "capitalizat) și două conturi de terț cu rol (401 furnizor, 404 furnizor de imobilizări)",
            n21?.Regim == RegimTva.Normal && sfd?.Regim == RegimTva.Scutit
            && ned21?.Regim == RegimTva.Capitalizat
            && cont401?.RolTert == RolTertCont.Furnizor && cont404?.RolTert == RolTertCont.Furnizor
            && tipImobilizare?.Clasa?.Natura == NaturaClasa.Imobilizare);

        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = MarcajNucFct + "-FURN";
        furnizor.Denumire = "Furnizor probă felia 30 (FCT)";
        var produs = os.CreateObject<Produs>();
        produs.Cod = MarcajNucFct + "-P";
        produs.Denumire = "Produs probă felia 30 (FCT)";
        produs.UM = "BUC";
        produs.TipMaterial = tipStoc;
        os.CommitChanges();

        FacturaIntrare Factura(string sufix, DateOnly data) {
            var doc = os.CreateObject<FacturaIntrare>();
            doc.Numar = MarcajNucFct + sufix;
            doc.Data = data;
            doc.Predator = furnizor;
            doc.Primitor = mag1;
            return doc;
        }
        FacturaIntrareDetaliu Linie(FacturaIntrare doc, TipMaterial tip, decimal cantitate, decimal pret, TipTva tva) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = doc;
            d.TipMaterial = tip;
            d.Cantitate = cantitate;
            d.PretUnitar = pret;
            d.TipTva = tva;
            return d;
        }

        // --- (1) PATRU regimuri pe același document, cu recepția TR-D3 între ele ---
        var fct = Factura("-F1", new DateOnly(2026, 3, 3));
        var linieStoc = Linie(fct, tipStoc, 5m, 10m, n21);
        var linieServiciu = Linie(fct, tipServicii, 1m, 100m, n21);
        var linieScutita = Linie(fct, tipServicii, 1m, 70m, sfd);
        // Brutul capitalizat iese fix 100, ca desfacerea jurnalului să fie 82,64 + 17,36.
        var linieCapitalizata = Linie(fct, tipServicii, 1m, 82.644628m, ned21);
        var lot = linieStoc.CreeazaLot(os, produs, mag1);
        os.CommitChanges();
        var nir = MotorOperare.Opereaza(os, fct);
        MotorOperare.Opereaza(os, nir);
        s.Check($"NUC-FCT-P4-1 ({eticheta}) scenă: patru regimuri pe același document — 50/10,5 (stoc, normal), "
            + "100/21 (serviciu, normal), 70/0 (scutit), 100 brut (capitalizat)",
            linieStoc.Valoare == 50m && linieStoc.ValoareTva == 10.5m
            && linieServiciu.Valoare == 100m && linieServiciu.ValoareTva == 21m
            && linieScutita.Valoare == 70m && linieScutita.ValoareTva == 0m
            && linieCapitalizata.Valoare == 100m && linieCapitalizata.ValoareTva == 0m
            && nir is NIR && lot != null);
        var fiscaleP4 = CubScena.Fapte(os, fct.ID).Where(f => !f.Storno).ToList();
        s.Check($"NUC-FCT-P4-2 ({eticheta}): patru rânduri fiscale — capitalizatul desface brutul (82,64 + 17,36), "
            + "scutitul are bază fără taxă, iar linia de stoc are rând deși netul ei e pe recepție",
            fiscaleP4.Count == 4
            && fiscaleP4.Any(f => f.DetaliuId == linieCapitalizata.ID && f.Baza == 82.64m && f.Tva == 17.36m)
            && fiscaleP4.Any(f => f.DetaliuId == linieScutita.ID && f.Baza == 70m && f.Tva == 0m)
            && fiscaleP4.Any(f => f.DetaliuId == linieStoc.ID && f.Baza == 50m && f.Tva == 10.5m));
        ProbeNucleu.Proba(os, s.Check, "NUC-FCT-P4", [fct]);
        var contractP4 = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fct);
        var partidaP4 = N.Unitate.DeschidePartida(cont401.ID, furnizor.ID, fct.ID, fct.DataInregistrare).Id;
        s.Check($"NUC-FCT-P4-3 ({eticheta}): O SINGURĂ partidă pe 401 (090h) poartă netul, taxa și recepția; "
            + "capătul virtual al recepției e singura postare cu gestiune structurală",
            contractP4.EsteAcceptat
            && contractP4.Decizii.OfType<N.PartidaDeschisa>().Single().Unitate.Id == partidaP4
            && contractP4.Tranzactii.SelectMany(t => t.Postari).Count(p => p.Coordonate.Unitate?.Id == partidaP4) == 7
            && contractP4.Tranzactii.SelectMany(t => t.Postari).Count(p => N.GestiuniVirtuale.Este(p.Coordonate.Gestiune)) == 1);

        // --- (2) Imobilizarea: net pe 404, taxă pe 401 — două partide pe același document ---
        var fctImo = Factura("-F2", new DateOnly(2026, 3, 4));
        var linieImo = Linie(fctImo, tipImobilizare, 1m, 500m, n21);
        os.CommitChanges();
        s.Check($"NUC-FCT-IMO-1 ({eticheta}) scenă: factura de imobilizare n-are conex (filtrul e natura Stoc)",
            MotorOperare.Opereaza(os, fctImo) == null && linieImo.Valoare == 500m && linieImo.ValoareTva == 105m);
        var noteImoCub = CubScena.Note(os, fctImo.ID);
        s.Check($"NUC-FCT-IMO-2 ({eticheta}): netul pe 404 (fallback-ul regulii de natură), taxa pe 401 "
            + "(contrapartida politicii de TVA) — două conturi de terț pe același document",
            noteImoCub.Nota(tipImobilizare.ContImplicitId, cont404.ID, 500m)
            && noteImoCub.Nota(cont4426.ID, cont401.ID, 105m));
        ProbeNucleu.Proba(os, s.Check, "NUC-FCT-IMO", [fctImo]);
        var contractImo = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fctImo);
        s.Check($"NUC-FCT-IMO-3 ({eticheta}): DOUĂ partide deschise de aceeași factură, una per cont de terț (090h)",
            contractImo.EsteAcceptat
            && contractImo.Decizii.OfType<N.PartidaDeschisa>().Select(d => d.Unitate.Cont).OrderBy(c => c)
                .SequenceEqual(new[] { cont401.ID, cont404.ID }.OrderBy(c => c)));

        // --- (2b) MAJOR-1: plata autogenerată de o factură cu DOUĂ conturi de terț ---
        // 101: legătura automată confirmă nominalizarea efectivă, 105 pe 401.
        // Restul facturii stă pe 404, iar diferența plății pe propria partidă 401.
        var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
        var fctPlata = Factura("-F4", new DateOnly(2026, 3, 6));
        fctPlata.GenereazaPlata = true;
        fctPlata.PlataContPropriu = casa;
        fctPlata.PlataNumar = MarcajNucFct + "-OP";
        fctPlata.PlataData = new DateOnly(2026, 3, 12);
        Linie(fctPlata, tipImobilizare, 1m, 500m, n21);
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctPlata);
        var plataImo = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fctPlata.ID);
        MotorOperare.Opereaza(os, plataImo);
        var impImo = os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataImo.ID);
        s.Check($"NUC-PLT-IMO-1 ({eticheta}) scenă: factura de imobilizare cu plată generată — brutul 605 "
            + "se împarte pe 404 (500) și 401 (105); legătura automată confirmă numai 105 pe contul comun (101)",
            fctPlata.Total == 605m && plataImo.Detalii.Single().Valoare == 605m && impImo.Suma == 105m
            && ImperechereService.Ramas(os, fctPlata.ID) == 500m && ImperechereService.Ramas(os, plataImo.ID) == 500m);

        var contractPlataImo = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, plataImo);
        var partidaFctImo = N.Unitate.DeschidePartida(
            cont401.ID, furnizor.ID, fctPlata.ID, fctPlata.DataInregistrare).Id;
        var partidaProprieImo = N.Unitate.DeschidePartida(
            cont401.ID, furnizor.ID, plataImo.ID, plataImo.DataInregistrare).Id;
        s.Check($"NUC-PLT-IMO-2 ({eticheta}): declarantul nominalizează DOAR 105 pe partida facturii "
            + "(cât ține ea pe 401) și lasă 500 pe partida proprie a plății; ipoteza consemnează soldul "
            + "REAL al partidei (105 credit), nu restul documentului (605) — MAJOR-1",
            contractPlataImo.EsteAcceptat
            && contractPlataImo.Tranzactii.SelectMany(t => t.Postari)
                .Any(x => x.Coordonate.Unitate?.Id == partidaFctImo && x.Valoare == 105m)
            && contractPlataImo.Tranzactii.SelectMany(t => t.Postari)
                .Any(x => x.Coordonate.Unitate?.Id == partidaProprieImo && x.Valoare == 500m)
            && contractPlataImo.Decizii.OfType<N.AlocareFifo>().Single().Masura == 105m
            && contractPlataImo.Ipoteze.OfType<N.SoldUnitateCitit>().Single() is { } cititImo
            && cititImo.Unitate.Id == partidaFctImo && cititImo.Sold.Credit == 105m);
        ProbeNucleu.Proba(os, s.Check, $"NUC-PLT-IMO-{eticheta}", [plataImo]);

        // --- (2c) 109: taxa nemarcată se decide la pregătire, iar declarația postează taxa liniei ---
        var fctCulese = Factura("-F5", new DateOnly(2026, 3, 7));
        var culeasa = Linie(fctCulese, tipServicii, 1m, 100m, n21);
        var lasata = Linie(fctCulese, tipServicii, 1m, 50m, n21);
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctCulese);
        s.Check($"NUC-FCT-CULESE-1 ({eticheta}) scenă: două linii la ACEEAȘI cotă — 100/21 și 50/10,50",
            culeasa.Valoare == 100m && culeasa.ValoareTva == 21m
            && lasata.Valoare == 50m && lasata.ValoareTva == 10.5m);
        using (var osCulese = s.Provider.CreateObjectSpace()) {
            var alDoilea = osCulese.GetObjectByKey<FacturaIntrare>(fctCulese.ID);
            alDoilea.Detalii.Single(d => d.ID == lasata.ID).ValoareTva = 0m;
            alDoilea.PregatesteOperare(osCulese);
            var contractCulese = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(
                osCulese, alDoilea);
            var taxePeLinie = contractCulese.Tranzactii.SelectMany(t => t.Postari)
                .Where(x => x.Coordonate.Cont == cont4426.ID)
                .ToDictionary(x => x.Cauza.Linie, x => x.Valoare);
            foreach (var refuz in contractCulese.Refuzuri)
                Console.WriteLine($"       refuz {refuz.Cod}: {refuz.Mesaj}");
            s.Check($"NUC-FCT-CULESE-2 ({eticheta}): linia lăsată la zero primește la pregătire repartizarea "
                + "documentului (10,50), iar declarația postează taxa fiecărei linii (21 și 10,50)",
                contractCulese.EsteAcceptat
                && taxePeLinie.Count == 2
                && taxePeLinie.GetValueOrDefault(culeasa.ID) == 21m
                && taxePeLinie.GetValueOrDefault(lasata.ID) == 10.5m);
        }

        // --- (3) N-r4, 109: taxa se decide pe DOCUMENT × cotă și ajunge aceeași pe linie, în registre și în cub ---
        var fctR4 = Factura("-F3", new DateOnly(2026, 3, 5));
        foreach (var _ in Enumerable.Range(0, 3))
            Linie(fctR4, tipServicii, 1m, 0.01m, n21);
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctR4);
        var taxaVeche = fctR4.Detalii.Sum(d => d.ValoareTva);
        s.Check($"NUC-FCT-N-R4-1 ({eticheta}): trei linii de 0,01 la 21% — taxa documentului (0,0063 → 0,01) stă pe o "
            + $"singură linie, iar registrele o poartă la fel: X = {taxaVeche}",
            CubScena.Note(os, fctR4.ID).Where(p => p.Debit && p.Cont == cont4426.ID).Sum(p => p.Valoare) == 0.01m
            && fctR4.Detalii.All(d => d.Valoare == 0.01m) && fctR4.Detalii.Sum(d => d.ValoareTva) == 0.01m
            && fctR4.Detalii.Count(d => d.ValoareTva == 0.01m) == 1
            && CubScena.Fapte(os, fctR4.ID) is { Count: 3 } fiscaleR4Cub
            && fiscaleR4Cub.Sum(f => f.Tva) == 0.01m);

        var contractR4 = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fctR4);
        var taxaNoua = contractR4.Tranzactii.SelectMany(t => t.Postari)
            .Where(p => p.Coordonate.Cont == cont4426.ID).Sum(p => p.Valoare);
        s.Check($"NUC-FCT-N-R4-2 ({eticheta}): cubul postează taxa liniei — Y = {taxaNoua}, "
            + $"Δ = Y − X = {taxaNoua - taxaVeche}, pe o singură linie (Hamilton)",
            contractR4.EsteAcceptat && taxaNoua == 0.01m && taxaNoua == taxaVeche
            && contractR4.Tranzactii.SelectMany(t => t.Postari).Count(p => p.Coordonate.Cont == cont4426.ID) == 1);

        // --- Felia 31 (TR-D7a), S-D8: cubul PERSISTAT pe FCT ---
        // (a) factura cu RECEPȚIE: partiția Stoc și capătul virtual N-D4.
        var fctCub = Factura("-F6", new DateOnly(2026, 3, 9));
        var linieCubStoc = Linie(fctCub, tipStoc, 4m, 25m, n21);
        Linie(fctCub, tipServicii, 1m, 60m, n21);
        linieCubStoc.CreeazaLot(os, produs, mag1);
        os.CommitChanges();
        {
            var nirCub = MotorOperare.Opereaza(os, fctCub);

            var faraDeclarant = os.CreateObject<RaportProductie>();
            faraDeclarant.Numar = MarcajNucFct + "-CONFIG";
            faraDeclarant.Data = fctCub.Data; faraDeclarant.Predator = mag1; faraDeclarant.Primitor = mag1;
            var linieConfig = os.CreateObject<DocumentDetaliu>();
            linieConfig.Document = faraDeclarant; linieConfig.TipMaterial = tipServicii;
            linieConfig.Cantitate = 1; linieConfig.Valoare = 100;
            os.CommitChanges();
            {
                string mesajConfig = null;
                using (var osConfig = s.Provider.CreateObjectSpace())
                    try { ComenziDocument.Sistem(osConfig).Opereaza(faraDeclarant.ID); }
                    catch (OperareException e) { mesajConfig = e.Message; }
                using var osDupaConfig = s.Provider.CreateObjectSpace();
                s.Check($"STR-CONFIG ({eticheta}): un tip FĂRĂ declarant (BPR) refuză operarea "
                    + $"ca eroare de configurare — „{mesajConfig?.Split('\n')[0]}” — și nu scrie nimic (S-D3)",
                    mesajConfig != null
                    && mesajConfig.Contains(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.TipFaraDeclaratie)
                    && osDupaConfig.GetObjectByKey<Document>(faraDeclarant.ID).Stare == StareDocument.Draft);
                ProbeCub.FaraRanduri(osDupaConfig, s.Check,
                    $"STR-CONFIG ({eticheta}): zero rânduri în cub după refuzul de configurare", faraDeclarant.ID);
            }

            MotorOperare.Opereaza(os, nirCub);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCT-CUB-{eticheta}", fctCub);
            var peStoc = ProbeCub.Postari(os, fctCub.ID, N.FelTranzactie.Operare)
                .Where(p => p.Spatiu == N.Spatiu.Stoc).ToList();
            var virtuale = ProbeCub.Postari(os, fctCub.ID, N.FelTranzactie.Operare)
                .Where(p => N.GestiuniVirtuale.Este(p.Gestiune)).ToList();
            s.Check($"STR-OPERARE {eticheta} (FCT/recepție): recepția scrie pe partiția Stoc (o postare, pe lotul "
                + "născut de linie), iar capătul virtual N-D4 stă pe partiția Contabil, cu gestiunea structurală",
                peStoc.Count == 1 && peStoc[0].Cantitate == 4m && peStoc[0].Unitate != null
                && virtuale.Count == 1 && virtuale[0].Spatiu == N.Spatiu.Contabil
                && virtuale[0].Gestiune == N.GestiuniVirtuale.Furnizor && virtuale[0].Cantitate == -4m);
        }

        // (b) STR-STORNO pe o factură de SERVICII (fără conex operat), unde postările poartă reper fiscal.
        var fctStorno = Factura("-F7", new DateOnly(2026, 3, 10));
        Linie(fctStorno, tipServicii, 1m, 90m, n21);
        os.CommitChanges();
        var dataStornoFct = new DateOnly(2026, 7, 22);
        {
            MotorOperare.Opereaza(os, fctStorno);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCT-STORNO-{eticheta}", fctStorno);
            MotorOperare.Storneaza(os, fctStorno, dataStornoFct);
            ProbeCub.ProbaStorno(os, s.Check, $"NUC-FCT-{eticheta}", fctStorno, dataStornoFct);
        }

        // (c) STR-REFUZ / STR-VALIDEAZA: gardul de toleranță al declarantului e mai STRICT decât
        //     motorul vechi (B-r1) — exact documentul pe care motorul vechi îl operează.
        var fctRefuz = Factura("-F8", new DateOnly(2026, 3, 11));
        var linieRefuz = Linie(fctRefuz, tipServicii, 1m, 100m, n21);
        linieRefuz.ValoareTva = 21.5m;
        Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fctRefuz, linieRefuz, nameof(DocumentDetaliu.ValoareTva));
        os.CommitChanges();
        {
            MotorOperare.Opereaza(os, fctRefuz);
            s.Check($"STR-REFUZ ({eticheta}) premisă: motorul VECHI operează factura cu TVA cules 21,50 pe o bază "
                + "de 100 la 21% (abatere 0,50, peste toleranța de 0,01 × liniile cotei)",
                fctRefuz.Stare == StareDocument.Operat && linieRefuz.ValoareTva == 21.5m);
            MotorOperare.AnuleazaOperarea(os, fctRefuz);
        }

        // S-D15: gardul nu mai vine din seed — scena îl pune LOCAL pe politica tipului.
        using (ProbeCub.CuToleranta(os, fctRefuz, 0.01m)) {
            string mesajRefuz = null;
            using (var osRefuz = s.Provider.CreateObjectSpace())
                try { ComenziDocument.Sistem(osRefuz).Opereaza(fctRefuz.ID); }
                catch (OperareException e) { mesajRefuz = e.Message; }
            using var osDupaRefuz = s.Provider.CreateObjectSpace();
            s.Check($"STR-REFUZ ({eticheta}): refuzul declarației e refuzul operației — „{mesajRefuz?.Split('\n')[0]}”; "
                + "documentul rămâne Draft, iar tranzacția comenzii nu lasă niciun rând în registre",
                mesajRefuz != null && mesajRefuz.Contains(N.Coduri.TvaInAfaraTolerantei)
                && osDupaRefuz.GetObjectByKey<Document>(fctRefuz.ID).Stare == StareDocument.Draft);
            ProbeCub.FaraRanduri(osDupaRefuz, s.Check,
                $"STR-REFUZ ({eticheta}): zero rânduri în cub după refuz", fctRefuz.ID);

            using var osDry = s.Provider.CreateObjectSpace();
            var eroriDry = ComenziDocument.Sistem(osDry).Valideaza(fctRefuz.ID);
            s.Check($"STR-VALIDEAZA ({eticheta}): dry-run-ul pe tipul migrat arată refuzul declarației "
                + $"([{string.Join("; ", eroriDry)}]), fără să scrie ceva",
                eroriDry.Any(e => e.Contains(N.Coduri.TvaInAfaraTolerantei)));
        }

        // (d) STR-FCT-NEGATIV (S-D14): linia „în roșu" pe aceeași factură (retur/discount),
        //     pe care motorul vechi o operează — semnul trece prin postări, nenormalizat.
        var fctNegativ = Factura("-F9", new DateOnly(2026, 3, 13));
        Linie(fctNegativ, tipServicii, 1m, 100m, n21);
        var linieRosie = Linie(fctNegativ, tipServicii, 1m, -40m, n21);
        os.CommitChanges();
        {
            MotorOperare.Opereaza(os, fctNegativ);
            s.Check($"STR-FCT-NEGATIV ({eticheta}) premisă: linia negativă e culeasă ca atare (−40 net, −8,40 "
                + "taxă) și motorul vechi operează documentul",
                linieRosie.Valoare == -40m && linieRosie.ValoareTva == -8.4m
                && fctNegativ.Stare == StareDocument.Operat);
            ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCT-NEGATIV-{eticheta}", fctNegativ);
            var aleRosii = ProbeCub.Postari(os, fctNegativ.ID, N.FelTranzactie.Operare);
            s.Check($"STR-FCT-NEGATIV ({eticheta}): semnul liniei trece NEnormalizat în postări (net −40 și taxă "
                + "−8,40, câte două capete) și tranzacția rămâne balansată: Σ D = Σ C",
                aleRosii.Count(p => p.Valoare == -40m) == 2
                && aleRosii.Count(p => p.Valoare == -8.4m) == 2
                && aleRosii.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare)
                    == aleRosii.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare));
        }

        // (e) STR-FCT-DOUA-PARTIDE (S-D16): linia ne-stoc pe TipMaterial-ul contului 408,
        //     cum o naște reclasificarea importului (`HandlerFactura.cs:143-160`) — DOUĂ
        //     conturi cu `RolTert` pe aceeași linie (B-r7). Numai pe profilul care le are.
        var tip408 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "408");
        var cont408 = ContSimbolFct("408");
        if (tip408 != null && cont408 != null && cont408.RolTert != RolTertCont.Niciunul) {
            var fctDoua = Factura("-FA", new DateOnly(2026, 3, 14));
            Linie(fctDoua, tip408, 1m, 100m, n21);
            os.CommitChanges();
            {
                MotorOperare.Opereaza(os, fctDoua);
                s.Check($"STR-FCT-DOUA-PARTIDE ({eticheta}) premisă: linia postează 408 = 401, două conturi cu "
                    + "`RolTert` pe ACELAȘI rând (B-r7)",
                    CubScena.Note(os, fctDoua.ID).Nota(cont408.ID, cont401.ID, 100m));
                ProbeCub.ProbaOperare(os, s.Check, $"NUC-FCT-DOUA-PARTIDE-{eticheta}", fctDoua);
                var partida408 = N.Unitate.DeschidePartida(
                    cont408.ID, furnizor.ID, fctDoua.ID, fctDoua.DataInregistrare).Id;
                var partida401 = N.Unitate.DeschidePartida(
                    cont401.ID, furnizor.ID, fctDoua.ID, fctDoua.DataInregistrare).Id;
                var aleDouaPartide = ProbeCub.Postari(os, fctDoua.ID, N.FelTranzactie.Operare);
                s.Check($"STR-FCT-DOUA-PARTIDE ({eticheta}): AMBELE conturi cu `RolTert` ale liniei poartă "
                    + "partida partenerului pe contul LOR (S-D16), nu doar piciorul de terț",
                    aleDouaPartide.Any(p => p.Cont == cont408.ID && p.Unitate == partida408
                        && p.Partener == furnizor.ID)
                    && aleDouaPartide.Any(p => p.Cont == cont401.ID && p.Unitate == partida401
                        && p.Partener == furnizor.ID));
            }
        }

        CurataNucFct(os);
        s.Check($"NUC-FCT-{eticheta} — curățenie finală (fără reziduuri de scenă)",
            !os.GetObjectsQuery<Produs>().Any(x => x.Cod.StartsWith(MarcajNucFct))
            && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNucFct)));
    }
}
