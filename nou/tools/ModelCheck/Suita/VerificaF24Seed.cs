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

// ═══════════ Felia 24, pasul 1 — seed-ul care ALINIAZĂ (decizia 83) ═══════════
// Timbrul e proprietate: rândul `DinSeed` e al seed-ului și se aliniază la cod
// (V1), rândul editat pe ușa securizată e al clientului (V2), rândul șters
// rămâne șters (V3), cheia acoperită manual oprește derivarea (V4), o schimbare
// de cheie e refuzată (V5), iar a doua trecere nu mai are ce corecta (V6).
// Toate rulează pe FUNCȚIA REALĂ (`ContaSeeder.Seed`), nu pe o imitație, și își
// lasă baza exact cum au găsit-o.
static class VerificaF24Seed {
    public static void Ruleaza(Suita s, bool privat) {
        var profilF24 = privat ? ProfilContabil.Privat : ProfilContabil.Bugetar;
        var etichetaF24 = privat ? "privat" : "bugetar";

        RaportSeed Reseed() {
            using var osS = s.Provider.CreateObjectSpace();
            var r = ContaSeeder.Seed(osS, profilF24);
            osS.CommitChanges();
            return r;
        }

        // ---- F24-V1: rândul de seed modificat pe ușa de sistem se readuce ----
        int zileInitial;
        Guid idScadenta;
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectsQuery<PoliticaScadenta>().ToList()
                .First(p => p.TipDocument.Cod == "FCL");
            idScadenta = rand.ID;
            zileInitial = rand.ZileDefault;
            rand.ZileDefault = zileInitial + 15;
            os.CommitChanges();
        }
        var raportV1 = Reseed();
        int zileDupa;
        bool timbruV1;
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectByKey<PoliticaScadenta>(idScadenta);
            zileDupa = rand.ZileDefault;
            timbruV1 = rand.DinSeed;
        }
        var corectieV1 = raportV1.Corectii.FirstOrDefault(c => c.Tip == nameof(PoliticaScadenta)
            && c.Cheie == "FCL" && c.Camp == nameof(PoliticaScadenta.ZileDefault));
        Console.WriteLine($"     MĂSURAT (F24-V1/{etichetaF24}): {zileInitial} → {zileInitial + 15} pe ușa de "
            + $"sistem, re-seed ⇒ {zileDupa} zile, `DinSeed` = {timbruV1}; contor `PoliticaScadenta`: "
            + $"{raportV1.Pentru(nameof(PoliticaScadenta)).Corectate} corectate; corecția raportată: "
            + $"„{corectieV1?.Camp}: {corectieV1?.Vechi} → {corectieV1?.Nou}”.");
        s.Check($"F24-V1 ({etichetaF24}) re-seed-ul CORECTEAZĂ rândul `DinSeed` divergent (83a): scadența FCL dusă "
            + $"la {zileInitial + 15} de zile pe ușa de sistem revine la {zileInitial}, timbrul rămâne aprins, "
            + "iar corecția apare în raport ca `câmp: vechi → nou` — auditul re-seed-ului e consola, fiindcă ușa "
            + "de sistem nu trece prin AuditTrail (83d)",
            zileDupa == zileInitial && timbruV1
            && raportV1.Pentru(nameof(PoliticaScadenta)).Corectate >= 1
            && corectieV1 != null && corectieV1.Vechi == (zileInitial + 15).ToString()
            && corectieV1.Nou == zileInitial.ToString());

        // ---- F24-V2: rândul EDITAT pe ușa securizată rămâne al clientului ----
        // Gardianul stinge timbrul la orice scriere securizată (F23-D4); proba
        // reproduce EFECTUL lui, nu ușa (securitatea se măsoară pe HTTP — 80i).
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectByKey<PoliticaScadenta>(idScadenta);
            rand.ZileDefault = zileInitial + 15;
            rand.DinSeed = false;
            os.CommitChanges();
        }
        var raportV2 = Reseed();
        int zileV2;
        bool timbruV2;
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectByKey<PoliticaScadenta>(idScadenta);
            zileV2 = rand.ZileDefault;
            timbruV2 = rand.DinSeed;
        }
        Console.WriteLine($"     MĂSURAT (F24-V2/{etichetaF24}): rând editat (timbru stins) ⇒ {zileV2} zile, "
            + $"`DinSeed` = {timbruV2}; contoare `PoliticaScadenta`: "
            + $"{raportV2.Pentru(nameof(PoliticaScadenta)).Manuale} manuale, "
            + $"{raportV2.Pentru(nameof(PoliticaScadenta)).Corectate} corectate.");
        s.Check($"F24-V2 ({etichetaF24}) rândul cu timbrul STINS e al clientului și NU se atinge (83a): re-seed-ul "
            + $"îl lasă la {zileInitial + 15} de zile, îl numără la „manuale pe cheie de seed” și nu raportează "
            + "nicio corecție pe tabelul lui — altfel primul `--forceUpdate` ar desface tăcut o decizie luată "
            + "deliberat",
            zileV2 == zileInitial + 15 && !timbruV2
            && raportV2.Pentru(nameof(PoliticaScadenta)).Manuale >= 1
            && raportV2.Pentru(nameof(PoliticaScadenta)).Corectate == 0);
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectByKey<PoliticaScadenta>(idScadenta);
            rand.ZileDefault = zileInitial;
            rand.DinSeed = true;
            os.CommitChanges();
        }

        // ---- F24-V3: rândul ȘTERS rămâne șters (104i: prin refuzul lăsat de gardian) ----
        using (var os = s.Provider.CreateObjectSpace()) {
            new GardianEditare().OnObjectSpaceCreated(os);
            os.Delete(os.GetObjectByKey<PoliticaScadenta>(idScadenta));
            os.CommitChanges();
        }
        var raportV3 = Reseed();
        int viiDupaStergere;
        using (var os = s.Provider.CreateObjectSpace())
            viiDupaStergere = os.GetObjectsQuery<PoliticaScadenta>().ToList()
                .Count(p => p.TipDocument.Cod == "FCL");
        // Restaurarea: refuzul se purjează, apoi seed-ul recreează rândul.
        using (var os = s.Provider.CreateObjectSpace())
            new Purja(os).Adauga(os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nameof(PoliticaScadenta))).Executa();
        var raportV3b = Reseed();
        int viiDupaPurja;
        bool timbruV3;
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.GetObjectsQuery<PoliticaScadenta>().ToList()
                .FirstOrDefault(p => p.TipDocument.Cod == "FCL");
            viiDupaPurja = rand == null ? 0 : 1;
            timbruV3 = rand != null && rand.DinSeed && rand.ZileDefault == zileInitial;
        }
        Console.WriteLine($"     MĂSURAT (F24-V3/{etichetaF24}): după ștergere ⇒ {viiDupaStergere} "
            + $"rânduri vii, contor `șterse` = {raportV3.Pentru(nameof(PoliticaScadenta)).Sterse}; după purja "
            + $"refuzului ⇒ {viiDupaPurja} rânduri, create = {raportV3b.Pentru(nameof(PoliticaScadenta)).Create}.");
        s.Check($"F24-V3 ({etichetaF24}) rândul ȘTERS de utilizator NU se recreează nici sub 83a (decizia 4 "
            + "rămâne, 104i): re-seed-ul îl numără la „șterse” și o SPUNE în consolă; după purja refuzului, "
            + "seed-ul îl recreează cu timbru — baza rămâne exact cum a găsit-o proba",
            viiDupaStergere == 0 && raportV3.Pentru(nameof(PoliticaScadenta)).Sterse >= 1
            && viiDupaPurja == 1 && timbruV3
            && raportV3b.Pentru(nameof(PoliticaScadenta)).Create >= 1);

        // ---- F24-V3 (privat): cheia zecimală citită din bază (numeric(18,4)) = cheia seed-ului ----
        if (privat) {
            (string Versiune, SectiuneTvaSaft Sectiune, Guid TipTvaId, RegimTva Regim, decimal Cota, bool DeImport,
                SensTva Sens, Atlas.Conta.Nucleu.RolTva Rol) cheieSaft;
            string cotaCitita, refuzCota;
            using (var os = s.Provider.CreateObjectSpace()) {
                var rand = os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.Cota == 21m && m.DinSeed)
                    .OrderBy(m => m.ID).First();
                cheieSaft = (rand.Versiune, rand.Sectiune, rand.TipTvaId, rand.Regim, rand.Cota, rand.DeImport,
                    rand.Sens, rand.Rol);
                cotaCitita = rand.Cota.ToString(System.Globalization.CultureInfo.InvariantCulture);
                new GardianEditare().OnObjectSpaceCreated(os);
                os.Delete(rand);
                os.CommitChanges();
            }
            int SaftVii() {
                using var os = s.Provider.CreateObjectSpace();
                return os.GetObjectsQuery<MapareTvaSaft>().Count(m => m.Versiune == cheieSaft.Versiune
                    && m.Sectiune == cheieSaft.Sectiune && m.TipTvaId == cheieSaft.TipTvaId && m.Regim == cheieSaft.Regim
                    && m.Cota == cheieSaft.Cota && m.DeImport == cheieSaft.DeImport && m.Sens == cheieSaft.Sens
                    && m.Rol == cheieSaft.Rol);
            }
            using (var os = s.Provider.CreateObjectSpace())
                refuzCota = os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nameof(MapareTvaSaft))
                    .Select(r => r.Cheie).FirstOrDefault();
            var raportSaft = Reseed();
            var saftDupaReseed = SaftVii();
            using (var os = s.Provider.CreateObjectSpace())
                new Purja(os).Adauga(os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nameof(MapareTvaSaft))).Executa();
            var raportSaftB = Reseed();
            var saftDupaPurja = SaftVii();
            Console.WriteLine($"     MĂSURAT (F24-V3/cheie zecimală): cota citită „{cotaCitita}”, refuz {refuzCota}; "
                + $"după re-seed ⇒ {saftDupaReseed} rânduri (șterse = {raportSaft.Pentru(nameof(MapareTvaSaft)).Sterse}); "
                + $"după purja refuzului ⇒ {saftDupaPurja} (create = {raportSaftB.Pentru(nameof(MapareTvaSaft)).Create}).");
            s.Check("F24-V3 (privat, 104i) cheia refuzului e canonică pe zecimale: maparea SAF-T cu cota 21 citită din "
                + "bază ca 21.0000 și candidatul seed-ului cu 21 dau ACEEAȘI cheie — re-seed-ul real (`ContaSeeder.Seed`) "
                + "nu o recreează; după purja refuzului o recreează",
                cotaCitita == "21.0000" && refuzCota != null && refuzCota.Contains("\"Cota\":\"21\"")
                && saftDupaReseed == 0 && raportSaft.Pentru(nameof(MapareTvaSaft)).Sterse == 1
                && saftDupaPurja == 1 && raportSaftB.Pentru(nameof(MapareTvaSaft)).Create == 1);
        }

        // ---- F24-V4: cheia derivată acoperită MANUAL oprește derivarea (privat) ----
        if (privat) {
            Guid bcsId, tipMfId;
            using (var os = s.Provider.CreateObjectSpace()) {
                bcsId = os.FirstOrDefault<TipDocument>(t => t.Cod == "BCS").ID;
                tipMfId = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371").ID;
            }
            // Rândul derivat 6xx=3xx al lui BCS pe Tipul 371 (excepția 607) dispare
            // FIZIC, ca locul lui să fie liber pentru unul manual pe aceeași cheie.
            using (var os = s.Provider.CreateObjectSpace())
                new Purja(os).Adauga(os.GetObjectsQuery<RegulaContare>()
                    .Where(r => r.TipDocumentId == bcsId && r.TipMaterialId == tipMfId)).Executa();
            using (var os = s.Provider.CreateObjectSpace()) {
                var manuala = os.CreateObject<RegulaContare>();
                manuala.TipDocumentId = bcsId;
                manuala.TipMaterialId = tipMfId;
                manuala.SursaContDebit = SursaCont.TipMaterial;
                manuala.SursaContCredit = SursaCont.TipMaterial;
                os.CommitChanges();
            }
            Reseed();
            int randuriPeTip, randuriToate;
            bool manualaNeatinsa;
            using (var os = s.Provider.CreateObjectSpace()) {
                var peTip = os.GetObjectsQuery<RegulaContare>()
                    .Where(r => r.TipDocumentId == bcsId && r.TipMaterialId == tipMfId).ToList();
                randuriPeTip = peTip.Count;
                manualaNeatinsa = peTip.Count == 1 && !peTip[0].DinSeed
                    && peTip[0].SursaContDebit == SursaCont.TipMaterial;
                randuriToate = os.GetObjectsQuery<RegulaContare>()
                    .Count(r => r.TipDocumentId == bcsId && r.TipMaterialId == tipMfId);
            }
            Console.WriteLine($"     MĂSURAT (F24-V4): BCS×371 acoperit MANUAL ⇒ {randuriPeTip} rând viu "
                + $"(neatins: {manualaNeatinsa}), {randuriToate} rânduri în tabelă cu tot cu cele șterse.");
            s.Check("F24-V4 cheia acoperită MANUAL oprește derivarea (83c): rândul 6xx=3xx al lui BCS pe Tipul 371 "
                + "e al clientului, deci seed-ul nu-l rescrie și nici nu adaugă unul al lui lângă — două rânduri "
                + "pe aceeași cheie ar fi făcut potrivirea motorului nedeterministă",
                randuriPeTip == 1 && manualaNeatinsa && randuriToate == 1);
            // Restaurare: rândul manual dispare fizic, seed-ul îl reface pe al lui.
            using (var os = s.Provider.CreateObjectSpace())
                new Purja(os).Adauga(os.GetObjectsQuery<RegulaContare>()
                    .Where(r => r.TipDocumentId == bcsId && r.TipMaterialId == tipMfId)).Executa();
            Reseed();
            bool refacut;
            using (var os = s.Provider.CreateObjectSpace())
                refacut = os.GetObjectsQuery<RegulaContare>().ToList()
                    .Count(r => r.TipDocumentId == bcsId && r.TipMaterialId == tipMfId && r.DinSeed) == 1;
            s.Check("F24-V4 restaurare: după purja rândului manual, derivarea îl reface pe al seed-ului — scena "
                + "nu lasă profilul schimbat în urma ei", refacut);
        }

        // ---- F24-V5: seed-ul nu schimbă CHEIA ----
        var codAncora = privat ? "N21" : "CAP21";
        string mesajCheie = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            try {
                ContaSeeder.Aliniaza<TipTva>(os, codAncora, t => t.Cod == codAncora,
                    t => t.Cod = codAncora + "X");
            }
            catch (InvalidOperationException e) {
                mesajCheie = e.Message;
            }
            os.Rollback();
        }
        bool ancoraIntacta;
        using (var os = s.Provider.CreateObjectSpace())
            ancoraIntacta = os.FirstOrDefault<TipTva>(t => t.Cod == codAncora) != null
                && os.FirstOrDefault<TipTva>(t => t.Cod == codAncora + "X") == null;
        Console.WriteLine($"     MĂSURAT (F24-V5/{etichetaF24}): `Cod` {codAncora} → {codAncora}X ⇒ "
            + $"„{mesajCheie ?? "<NU A ARUNCAT>"}”; ancora intactă: {ancoraIntacta}.");
        s.Check($"F24-V5 ({etichetaF24}) cheia NU se schimbă prin seed (83b): un `seteaza` care atinge o coloană "
            + "de index unic pică zgomotos, cu numele câmpului și cele două valori — o redenumire de cod e pas "
            + "de MIGRAȚIE de date, niciodată upsert tăcut",
            mesajCheie != null && mesajCheie.Contains("cheia") && mesajCheie.Contains(nameof(TipTva.Cod))
            && ancoraIntacta);

        // ---- F24-V6: a doua trecere nu mai are ce corecta ----
        Reseed();
        var raportV6 = Reseed();
        Console.WriteLine($"     MĂSURAT (F24-V6/{etichetaF24}): a doua trecere ⇒ {raportV6.TotalCreate} create, "
            + $"{raportV6.TotalCorectate} corectate, {raportV6.Corectii.Count} corecții listate"
            + (raportV6.Corectii.Count > 0
                ? " (" + string.Join("; ", raportV6.Corectii.Take(5)
                    .Select(c => $"{c.Tip}/{c.Cheie}/{c.Camp}: {c.Vechi} → {c.Nou}")) + ")"
                : "") + ".");
        s.Check($"F24-V6 ({etichetaF24}) IDEMPOTENȚĂ: pe o bază deja aliniată a doua trecere a seed-ului nu "
            + "creează și nu corectează NIMIC. E proba că `seteaza` nu atinge câmpuri din afara contractului "
            + "(`Activ` pe `TipTva`, contorul din `PoliticaNumerotare`) și că diff-ul nu vede fantome (scări "
            + "zecimale, coloane generate, FK-uri)",
            raportV6.TotalCreate == 0 && raportV6.TotalCorectate == 0 && raportV6.Corectii.Count == 0);

        // ---- F24-V9: curățenia FCL din `ProfilPrivat` respectă timbrul (M2) ----
        if (privat) {
            Guid idValidare;
            using (var os = s.Provider.CreateObjectSpace()) {
                var manuala = os.CreateObject<PoliticaValidare>();
                manuala.TipDocument = os.GetObjectsQuery<TipDocument>().ToList().First(t => t.Cod == "FCL");
                manuala.NaturaInterzisa = NaturaClasa.Stoc;
                os.CommitChanges();
                idValidare = manuala.ID;
            }
            var raportV9 = Reseed();
            bool traieste, timbruStins;
            NaturaClasa? naturaDupa;
            using (var os = s.Provider.CreateObjectSpace()) {
                var rand = os.GetObjectByKey<PoliticaValidare>(idValidare);
                traieste = rand != null;
                timbruStins = rand is { DinSeed: false };
                naturaDupa = rand?.NaturaInterzisa;
            }
            Console.WriteLine($"     MĂSURAT (F24-V9/privat): rândul manual `PoliticaValidare` FCL "
                + $"(natura interzisă {naturaDupa?.ToString() ?? "<golită>"}) după re-seed: "
                + $"{(traieste ? "viu" : "ȘTERS")}, `DinSeed` = {!timbruStins}; "
                + $"{raportV9.Pentru(nameof(PoliticaValidare)).Sterse} retrageri raportate.");
            s.Check("F24-V9 (privat) curățenia istorică a lui `ProfilPrivat` (rândul P1 „FCL nu poartă stoc”) se "
                + "uită la TIMBRU: un rând identic creat de client din ecranul de validare — configurație "
                + "legitimă („vindem doar servicii”) — supraviețuiește lui `--forceUpdate`. Înainte dispărea "
                + "tăcut, contra lui 83a, iar ecranul feliei 24 tocmai deschisese ușa care îl face creabil",
                traieste && timbruStins && naturaDupa == NaturaClasa.Stoc);
            using (var os = s.Provider.CreateObjectSpace())
                new Purja(os).Adauga(os.GetObjectsQuery<PoliticaValidare>()
                    .Where(p => p.ID == idValidare)).Executa();
        }

        // ---- F24-V8: listele profilului ----
        using (var os = s.Provider.CreateObjectSpace()) {
            var goluri = ContaSeeder.GoluriMapari(os, profilF24);
            var active = os.GetObjectsQuery<TipTva>().Count(t => t.Activ);
            var deSeed = os.GetObjectsQuery<TipTva>().Count(t => t.DinSeed);
            var implicite = os.GetObjectsQuery<PoliticaTvaImplicit>().Count();
            if (!privat) {
                Console.WriteLine($"     MĂSURAT (F24-V8/bugetar): {deSeed} tipuri TVA de seed, {active} active, "
                    + $"{implicite} implicite, {goluri.Count} goluri de mapare.");
                s.Check("F24-V8 (bugetar) profilul nu primește NICIUN rând nou din 83f/g (golurile sunt ale "
                    + "profilului privat): patru cote capitalizate, trei active, zero implicite, zero goluri",
                    deSeed == 4 && active == 3 && implicite == 0 && goluri.Count == 0);
                return;
            }
            var imp = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP");
            var nim = os.FirstOrDefault<TipTva>(t => t.Cod == "NIM");
            var nemapateImp = ContaSeeder.NemapateD300Privat.Count(n => n.TipTva == "IMP");
            var nemapateImp394 = ContaSeeder.NemapateD394Privat.Count(n => n.TipTva == "IMP");
            Console.WriteLine($"     MĂSURAT (F24-V8/privat): {deSeed} tipuri TVA de seed, {active} active; "
                + $"IMP = {(imp == null ? "<lipsă>" : $"cotă {imp.Cota}, regim {imp.Regim}, activ {imp.Activ}, "
                    + $"SAF-T achiziție {imp.CodSafTAchizitie ?? "<null>"}")}; "
                + $"NIM SAF-T achiziție = {nim?.CodSafTAchizitie ?? "<null>"}; "
                + $"{ContaSeeder.ImpliciteTvaPrivat.Count} implicite în tabel, {implicite} în bază; "
                + $"IMP nemapat deliberat: {nemapateImp} pe D300, {nemapateImp394} pe D394; "
                + $"{goluri.Count} goluri de mapare.");
            s.Check("F24-V8 (privat) tipul `IMP` e rând de nomenclator sub un regim EXISTENT (83g): cotă 0, regim "
                + "Neimpozabil, activ, fără cod SAF-T — nomenclatorul ANAF n-are cod pentru factura furnizorului "
                + "extern fără TVA, iar 301204/300604 sunt ale DVI-ului (83-r3). `NIM` capătă 308302, fiindcă "
                + "83f îl pune pe achiziție, iar un implicit fără cod ar fi o gaură făcută de noi",
                imp != null && imp.Cota == 0m && imp.Regim == RegimTva.Neimpozabil && imp.Activ
                && imp.CodSafTAchizitie == null && imp.CodSafTLivrare == null
                && nim != null && nim.CodSafTAchizitie == "308302"
                && deSeed == 15);
            s.Check("F24-V8 (privat) golul se închide cu RÂND și cu MOTIV: unsprezece implicite de politică, iar `IMP` e "
                + "declarat nemapat deliberat pe AMBELE sensuri și în D300 („din DVI”), și în D394 („partener "
                + "extra-UE nu se declară”) — gardienii de profil rămân verzi prin liste, nu prin excepție în cod",
                ContaSeeder.ImpliciteTvaPrivat.Count == 11 && implicite == 11
                && nemapateImp == 2 && nemapateImp394 == 2 && goluri.Count == 0);
        }
    }
}
