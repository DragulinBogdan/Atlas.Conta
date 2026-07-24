using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

// Validare model EF + (dacă baza există) verificare migrații/seed + scenariile
// end-to-end ale motorului de operare pe un IObjectSpace real — aceeași
// infrastructură XAF pe care o folosește și UI-ul (docs 113709).
//
// Parametrizat pe PROFIL (P1, design §7): implicit rulează suita bugetară pe
// baza aplicației (aceeași țintă ca appsettings.json); `ModelCheck privat`
// rulează blocul e2e privat pe o bază DEDICATĂ (profil-per-bază — 35d), pe
// care unealta o migrează și o seed-uiește singură (ContaSeeder, Privat).
var profil = args.Any(a => a.Contains("privat", StringComparison.OrdinalIgnoreCase))
    ? ProfilContabil.Privat : ProfilContabil.Bugetar;
var connectionString = "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database="
    + (profil == ProfilContabil.Privat ? "Atlas.Conta.ModelCheck.Privat" : "Atlas.Conta.BackOffice");

var esecuri = 0;
void Check(string nume, bool ok) {
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {nume}");
    if (!ok)
        esecuri++;
}
void CheckRefuza(string nume, Action actiune) {
    try {
        actiune();
        Check(nume, false);
    }
    catch (OperareException e) {
        Console.WriteLine($"OK   {nume} — „{e.Message.Split('\n')[0]}”");
    }
}
void Rezumat() {
    Console.WriteLine(esecuri == 0 ? "\nToate verificările au trecut." : $"\n{esecuri} verificări EȘUATE.");
    Environment.ExitCode = esecuri == 0 ? 0 : 1;
}

var opts = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql(connectionString)
    .UseChangeTrackingProxies()
    .Options;

using (var ctx = new BackOfficeEFCoreDbContext(opts)) {
    Console.WriteLine($"Model OK: {ctx.Model.GetEntityTypes().Count()} entity types; profil: {profil}");

    if (profil == ProfilContabil.Privat) {
        // Baza privată aparține uneltei: se creează/migrează aici.
        await ctx.Database.MigrateAsync();
    }
    else {
        if (!await ctx.Database.CanConnectAsync()) {
            Console.WriteLine("Baza nu există încă — doar validare de model.");
            return;
        }
        var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
        var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
        Console.WriteLine($"Migrații aplicate: {applied.Count}; în așteptare: {pending.Count}"
            + (pending.Count > 0 ? $" ({string.Join(", ", pending)})" : ""));
        if (pending.Count > 0) {
            Console.WriteLine("Aplicați migrațiile înainte de scenariul e2e (dotnet ef database update).");
            return;
        }
    }
}

using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(connectionString)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies());

// Seed-ul profilului privat: exact calea updater-ului (ContaSeeder), pe
// ObjectSpace standalone; idempotent la rulări repetate.
if (profil == ProfilContabil.Privat) {
    using var osSeed = provider.CreateObjectSpace();
    ContaSeeder.Seed(osSeed, ProfilContabil.Privat);
}

using (var ctx = new BackOfficeEFCoreDbContext(opts)) {
    Console.WriteLine($"TipuriDocument:  {await ctx.TipuriDocument.CountAsync()}");
    Console.WriteLine($"ClaseProduse:    {await ctx.ClaseProduse.CountAsync()}");
    Console.WriteLine($"TipuriMaterial:  {await ctx.TipuriMaterial.CountAsync()}");
    Console.WriteLine($"Conturi:         {await ctx.Conturi.CountAsync()} (din care cu defalcare: {await ctx.Conturi.CountAsync(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna)})");
    Console.WriteLine($"Repartitori:     {await ctx.Repartitori.CountAsync()}");
    Console.WriteLine($"PerioadeFiscale: {await ctx.PerioadeFiscale.CountAsync()}");
    Console.WriteLine($"TipuriTva:       {await ctx.TipuriTva.CountAsync()}");
    Console.WriteLine($"ReguliStoc:      {await ctx.ReguliStoc.CountAsync()}");
    Console.WriteLine($"ReguliContare:   {await ctx.ReguliContare.CountAsync()}");

    // Garda pentru limitarea owned + table sharing: cu navigația REQUIRED, un rând
    // cu toate dimensiunile null trebuie să materializeze obiect gol, nu null.
    var tipDoc = await ctx.TipuriDocument.FirstAsync();
    var repartitor = await ctx.Repartitori.FirstAsync();
    var proba = ctx.CreateProxy<RegulaContare>(); // owner-ul TREBUIE proxy (ca în XAF)
    proba.TipDocumentId = tipDoc.ID;
    proba.DimensiuniComun = new Dimensiuni { RepartitorId = repartitor.ID };
    ctx.ReguliContare.Add(proba);
    await ctx.SaveChangesAsync();
    ctx.ChangeTracker.Clear();

    var recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
    Check("Owned insert → FK persistat", recitita.DimensiuniComun.RepartitorId == repartitor.ID);
    Check("Owned all-null → instanță, nu null", recitita.DimensiuniOverrideDebit != null);

    recitita.DimensiuniComun.RepartitorId = null;
    await ctx.SaveChangesAsync();
    ctx.ChangeTracker.Clear();
    recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
    Check("Owned update → schimbare detectată", recitita.DimensiuniComun.RepartitorId == null);

    ctx.ReguliContare.Remove(recitita);
    await ctx.SaveChangesAsync();
}

// ========================= Scenariul e2e P1: profil privat =========================
// TVA structural pe baza privată (OMFP 1802): FCT cu linie stoc + linie serviciu
// (NIR net, 4426 pe factură — inclusiv pentru linia de stoc fără regulă
// principală, 401 brut, imperecherea plății automate pe brut) → FCL cu 4427 →
// DEC cu 4426 = 542 → taxare inversă (4426 = 4427) → capitalizat (nedeductibil)
// → ValoareTva culeasă manual păstrată → storno cu rânduri TVA inverse.
if (profil == ProfilContabil.Privat) {
    const string MarcajPrv = "E2E-PRV";

    void CurataPrv(IObjectSpace os) {
        var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajPrv)).Select(r => r.ID).ToList();
        var docs = os.GetObjectsQuery<Document>()
            .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
        var docIds = docs.Select(d => d.ID).ToList();
        os.Delete(os.GetObjectsQuery<Imperechere>()
            .Where(i => docIds.Contains(i.DocumentTrezorerieId) || docIds.Contains(i.DocumentId)).ToList());
        os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
        foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
            os.Delete(doc);
        os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajPrv).ToList());
        os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajPrv).ToList());
        os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajPrv)).ToList());
        os.CommitChanges();
    }

    using (var os = provider.CreateObjectSpace()) {
        CurataPrv(os);

        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
        var tip302 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tip345 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "345");
        var tip381 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "381");
        var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var ti21 = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
        var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
        Cont ContSimbol(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
        var cont401 = ContSimbol("401");
        var cont4111 = ContSimbol("4111");
        var cont4426 = ContSimbol("4426");
        var cont4427 = ContSimbol("4427");
        var cont542 = ContSimbol("542");
        var cont5121 = ContSimbol("5121");

        // --- Seed-ul profilului privat ---
        Check("Seed: N21 = Normal 21%, conturile 4426/4427 (+4428 rezervat) și codurile SAF-T ca date",
            n21 != null && n21.Regim == RegimTva.Normal && n21.Cota == 21m
            && n21.ContTvaDeductibilId == cont4426.ID && n21.ContTvaColectatId == cont4427.ID
            && n21.ContTvaNeexigibilId != null
            && n21.CodSafTLivrare == "310344" && n21.CodSafTAchizitie == "301104");
        Check("Seed: derivarea contului implicit pe simboluri OMFP (302 → 302, exact)",
            tip302?.ContImplicitId != null
            && os.GetObjectByKey<Cont>(tip302.ContImplicitId.Value).Simbol == "302");
        Cont DebitBcs(TipMaterial tip) {
            var r = os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "BCS" && x.TipMaterialId == tip.ID);
            return r?.ContDebitId == null ? null : os.GetObjectByKey<Cont>(r.ContDebitId.Value);
        }
        Check("Seed BCS: 302→602 (mecanic) + excepțiile profilului 371→607, 345→711, 381→608",
            DebitBcs(tip302)?.Simbol == "602" && DebitBcs(tip371)?.Simbol == "607"
            && DebitBcs(tip345)?.Simbol == "711" && DebitBcs(tip381)?.Simbol == "608");
        var plusLdi = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == null);
        Check("Seed LDI: plusul de inventar → credit 7588 (nu 791 — decizia 29c)",
            plusLdi != null && plusLdi.SemnFiltru == +1
            && plusLdi.ContCredit?.Simbol == "7588");
        PoliticaTva Tva(string cod) => os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == cod);
        Check("Seed: PoliticaTva — FCT/DEC deduc (401/542), FCL colectează (4111); NIR/PLT/INC fără rând",
            Tva("FCT")?.Directie == DirectieTva.Deductibil && Tva("FCT").SursaContrapartida == SursaCont.RepartitorPredator
            && Tva("FCT").ContrapartidaFallback?.Simbol == "401"
            && Tva("DEC")?.Directie == DirectieTva.Deductibil && Tva("DEC").ContrapartidaFallback?.Simbol == "542"
            && Tva("FCL")?.Directie == DirectieTva.Colectat && Tva("FCL").SursaContrapartida == SursaCont.RepartitorPrimitor
            && Tva("FCL").ContrapartidaFallback?.Simbol == "4111"
            && Tva("NIR") == null && Tva("PLT") == null && Tva("INC") == null);
        Check("Seed: profilul de validare privat — fără clasificație bugetară; FCL NU mai interzice stocul (P2, descărcarea de gestiune preia vânzarea din stoc)",
            os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCT") == null
            && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL")?.NaturaInterzisa != NaturaClasa.Stoc);
        Check("Seed: plan OMFP fără defalcări obligatorii (pornesc goale — design §5)",
            !os.GetObjectsQuery<Cont>().Any(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna));

        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = MarcajPrv + "-FURN";
        furnizor.Denumire = "Furnizor probă privat";
        var client = os.CreateObject<Partener>();
        client.Cod = MarcajPrv + "-CL";
        client.Denumire = "Client probă privat";
        var angajat = os.CreateObject<Angajat>();
        angajat.Cod = MarcajPrv + "-ANG";
        angajat.Denumire = "Titular probă privat"; // fără ContImplicit — fallback 542
        var produs = os.CreateObject<Produs>();
        produs.Cod = MarcajPrv;
        produs.Denumire = "Produs probă privat";
        produs.UM = "BUC";
        produs.TipMaterial = tip302;
        os.CommitChanges();

        List<RegistruContabil> Note(Document doc) =>
            os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

        // --- FCT: linie stoc + linie serviciu, N21, plata automată pe brut ---
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = "E2E-PRV-FF1";
        fct.Data = new DateOnly(2026, 3, 3);
        fct.Predator = furnizor;
        fct.Primitor = mag1;
        fct.GenereazaPlata = true;
        fct.PlataContPropriu = banca;
        fct.PlataNumar = "OP-P1";
        fct.PlataData = new DateOnly(2026, 3, 4);
        var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
        linieStoc.Document = fct;
        linieStoc.TipMaterial = tip302;
        linieStoc.Cantitate = 5m;
        linieStoc.PretUnitar = 10m;
        linieStoc.TipTva = n21;
        var lot = linieStoc.CreeazaLot(os, produs, mag1);
        var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
        linieServiciu.Document = fct;
        linieServiciu.TipMaterial = tip628;
        linieServiciu.Cantitate = 1m;
        linieServiciu.PretUnitar = 100m;
        linieServiciu.TipTva = n21;
        os.CommitChanges();

        var conex = MotorOperare.Opereaza(os, fct);
        Check("FCT privat: lanțul de valori NET + TVA separat (50/10,5 și 100/21)",
            linieStoc.Valoare == 50m && linieStoc.ValoareTva == 10.5m
            && linieServiciu.Valoare == 100m && linieServiciu.ValoareTva == 21m);
        Check("Lot finalizat la NET: preț 10 (fără TVA capitalizat)",
            lot.PretUnitar == 10m);
        Check("Total factură = BRUT (181,5) — imperecherea stinge brutul",
            fct.Total == 181.5m);
        var noteFct = Note(fct);
        Check("FCT: 3 note — serviciul net (628 = 401, 100) + câte un rând 4426 per linie",
            noteFct.Count == 3
            && noteFct.Any(n => n.ContDebitId == tip628.ContImplicitId && n.ContCreditId == cont401.ID && n.Valoare == 100m)
            && noteFct.Count(n => n.ContDebitId == cont4426.ID && n.ContCreditId == cont401.ID) == 2);
        Check("Rândul 4426 al liniei de STOC există deși linia nu are regulă principală (netul e pe NIR)",
            noteFct.Any(n => n.DetaliuId == linieStoc.ID && n.ContDebitId == cont4426.ID && n.Valoare == 10.5m));
        Check("Rândul 4426 al serviciului: 21; dimensiunile din default-ul polimorf al header-ului",
            noteFct.Any(n => n.DetaliuId == linieServiciu.ID && n.ContDebitId == cont4426.ID && n.Valoare == 21m
                && n.DimensiuniDebit.RepartitorId == furnizor.ID && n.DimensiuniCredit.RepartitorId == mag1.ID));

        // --- NIR conex: netul, fără TVA ---
        Check("Conex: NIR draft cu linia de stoc la NET, TipTva clonat ca informație, ValoareTva 0",
            conex is NIR { Stare: StareDocument.Draft } && conex.Detalii.Count == 1
            && conex.Detalii[0].Valoare == 50m && conex.Detalii[0].ValoareTva == 0m
            && conex.Detalii[0].TipTvaId == n21.ID);
        MotorOperare.Opereaza(os, conex);
        var noteNir = Note(conex);
        var stocNir = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == conex.ID).ToList();
        Check("NIR: +5/50 în stoc (evaluare la net) și o singură notă 302 = 401, 50 — fără rânduri TVA",
            stocNir.Count == 1 && stocNir[0].Cantitate == 5m && stocNir[0].Valoare == 50m
            && noteNir.Count == 1 && noteNir[0].ContDebitId == tip302.ContImplicitId
            && noteNir[0].ContCreditId == cont401.ID && noteNir[0].Valoare == 50m);

        // --- Plata automată: defalcarea pe BRUT + imperecherea integrală ---
        var plataAuto = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fct.ID);
        Check("Plata autogenerată: liniile clonează defalcarea BRUTĂ (60,5 + 121 = 181,5), fără TipTva",
            plataAuto.Detalii.Count == 2 && plataAuto.Detalii.Sum(d => d.Valoare) == 181.5m
            && plataAuto.Detalii.All(d => d.TipTvaId == null && d.ValoareTva == 0m));
        MotorOperare.Opereaza(os, plataAuto);
        var notePlata = Note(plataAuto);
        Check("Plata contează 401 = 5121 (banca) pe brut",
            notePlata.Count == 2 && notePlata.All(n => n.ContDebitId == cont401.ID && n.ContCreditId == cont5121.ID)
            && notePlata.Sum(n => n.Valoare) == 181.5m);
        Check("Imperecherea automată stinge BRUTUL facturii (181,5; rest 0)",
            os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentTrezorerieId == plataAuto.ID).Suma == 181.5m
            && ImperechereService.Ramas(os, fct.ID) == 0m);

        // --- FCL: 4427 colectat ---
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Data = new DateOnly(2026, 3, 6);
        fcl.Predator = sediu;
        fcl.Primitor = client;
        var linieVenit = os.CreateObject<FacturaIesireDetaliu>();
        linieVenit.Document = fcl;
        linieVenit.TipMaterial = tip704;
        linieVenit.Cantitate = 1m;
        linieVenit.PretUnitar = 200m;
        linieVenit.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fcl);
        var noteFcl = Note(fcl);
        Check("FCL: 4111 = 704 net (200) + 4111 = 4427 (42); scadența default +30; total brut 242",
            noteFcl.Count == 2
            && noteFcl.Any(n => n.ContDebitId == cont4111.ID && n.ContCreditId == tip704.ContImplicitId && n.Valoare == 200m)
            && noteFcl.Any(n => n.ContDebitId == cont4111.ID && n.ContCreditId == cont4427.ID && n.Valoare == 42m)
            && fcl.DataScadenta == fcl.Data.AddDays(30) && fcl.Total == 242m);

        // --- DEC: 4426 = 542 pe titular ---
        var dec = os.CreateObject<Decont>();
        dec.Data = new DateOnly(2026, 3, 8);
        dec.Predator = angajat;
        dec.Primitor = sediu;
        var linieDec = os.CreateObject<DecontDetaliu>();
        linieDec.Document = dec;
        linieDec.TipMaterial = tip628;
        linieDec.PretUnitar = 30m; // cantitatea pro-formă 0 → 1
        linieDec.TipTva = n21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, dec);
        var noteDec = Note(dec);
        Check("DEC: cheltuiala net (628 = 542, 30) + TVA justificat (4426 = 542, 6,3), creditul pe TITULAR",
            noteDec.Count == 2
            && noteDec.Any(n => n.ContDebitId == tip628.ContImplicitId && n.ContCreditId == cont542.ID && n.Valoare == 30m)
            && noteDec.Any(n => n.ContDebitId == cont4426.ID && n.ContCreditId == cont542.ID && n.Valoare == 6.3m)
            && noteDec.All(n => n.DimensiuniCredit.RepartitorId == angajat.ID));

        // --- Taxare inversă: 4426 = 4427, apoi storno cu rândurile TVA inverse ---
        var fctTi = os.CreateObject<FacturaIntrare>();
        fctTi.Numar = "E2E-PRV-FF2";
        fctTi.Data = new DateOnly(2026, 3, 9);
        fctTi.Predator = furnizor;
        fctTi.Primitor = mag1;
        var linieTi = os.CreateObject<FacturaIntrareDetaliu>();
        linieTi.Document = fctTi;
        linieTi.TipMaterial = tip628;
        linieTi.Cantitate = 1m;
        linieTi.PretUnitar = 100m;
        linieTi.TipTva = ti21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctTi);
        var noteTi = Note(fctTi);
        Check("Taxare inversă: serviciul net (628 = 401, 100) + autolichidare 4426 = 4427 (21), total facturii NET",
            noteTi.Count == 2
            && noteTi.Any(n => n.ContDebitId == tip628.ContImplicitId && n.ContCreditId == cont401.ID && n.Valoare == 100m)
            && noteTi.Any(n => n.ContDebitId == cont4426.ID && n.ContCreditId == cont4427.ID && n.Valoare == 21m)
            && fctTi.Total == 121m);
        MotorOperare.Storneaza(os, fctTi, new DateOnly(2026, 7, 23));
        var stornoTi = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId == fctTi.ID && r.Storno).ToList();
        Check("Storno cu TVA: rândurile inverse includ și rândul 4426 = 4427 (−100, −21)",
            stornoTi.Count == 2 && stornoTi.Any(r => r.Valoare == -100m)
            && stornoTi.Any(r => r.Valoare == -21m && r.ContDebitId == cont4426.ID));

        // --- Capitalizat (nedeductibil): comportamentul bugetar, ca date ---
        var fctNed = os.CreateObject<FacturaIntrare>();
        fctNed.Numar = "E2E-PRV-FF3";
        fctNed.Data = new DateOnly(2026, 3, 10);
        fctNed.Predator = furnizor;
        fctNed.Primitor = mag1;
        var linieNed = os.CreateObject<FacturaIntrareDetaliu>();
        linieNed.Document = fctNed;
        linieNed.TipMaterial = tip628;
        linieNed.Cantitate = 1m;
        linieNed.PretUnitar = 100m;
        linieNed.TipTva = ned21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctNed);
        Check("Capitalizat (NED21): Valoare = brut 121, ValoareTva 0, o singură notă 628 = 401 — fără rând 4426",
            linieNed.Valoare == 121m && linieNed.ValoareTva == 0m
            && Note(fctNed).Count == 1 && Note(fctNed).Single().Valoare == 121m
            && !Note(fctNed).Any(n => n.ContDebitId == cont4426.ID));

        // --- ValoareTva culeasă pe FCT bate rotunjirea noastră (design §3) ---
        var fctManual = os.CreateObject<FacturaIntrare>();
        fctManual.Numar = "E2E-PRV-FF4";
        fctManual.Data = new DateOnly(2026, 3, 11);
        fctManual.Predator = furnizor;
        fctManual.Primitor = mag1;
        var linieManual = os.CreateObject<FacturaIntrareDetaliu>();
        linieManual.Document = fctManual;
        linieManual.TipMaterial = tip628;
        linieManual.Cantitate = 1m;
        linieManual.PretUnitar = 100m;
        linieManual.TipTva = n21;
        linieManual.ValoareTva = 20.9m; // TVA-ul de pe factura furnizorului
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctManual);
        Check("ValoareTva culeasă manual (20,9) nu se suprascrie la operare; rândul 4426 o postează",
            linieManual.ValoareTva == 20.9m
            && Note(fctManual).Any(n => n.ContDebitId == cont4426.ID && n.Valoare == 20.9m));

        CurataPrv(os);
        Check("Curățenie finală privat (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajPrv))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajPrv));
    }

    // ================= Scenariul e2e P2: descărcarea de gestiune (DSC) =================
    // Magazinul online (design §2): FCL dictată de site (preț decuplat de cost,
    // poziții fără stoc la facturare), identificare cu prioritate pe lot. Loturile
    // vin din NIR-uri manuale la DATE DIFERITE — FIFO determinist pe Lot.Data (nu
    // tie-break pe Lot.ID/Guid). FCL cu FIFO + pin (contenția pe același produs) +
    // serviciu + poziție indisponibilă → DSC conex spart pe loturi la GENERARE
    // (pin întâi) → operare DSC (cost 6xx=3xx ≠ vânzarea 7xx) → backorder
    // (Genereaza direct = acțiunea manuală) → gardieni de grup + storno care
    // redeschide restul → anularea cu draft îl șterge → default TipTva de culegere.
    const string MarcajDsc = "E2E-DSC";

    void CurataDsc(IObjectSpace os) {
        var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDsc)).Select(r => r.ID).ToList();
        var docs = os.GetObjectsQuery<Document>()
            .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
        var docIds = docs.Select(d => d.ID).ToList();
        os.Delete(os.GetObjectsQuery<Imperechere>()
            .Where(i => docIds.Contains(i.DocumentTrezorerieId) || docIds.Contains(i.DocumentId)).ToList());
        os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
        // Copiii autogenerați (DSC conex, fără Numar) întâi — DocumentSursa spre FCL.
        foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
            os.Delete(doc);
        os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajDsc)).ToList());
        os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajDsc)).ToList());
        // Tipul creat în testul de defect 1 (nu e curățat prin markerul de doc).
        os.Delete(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajDsc)).ToList());
        os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDsc)).ToList());
        os.CommitChanges();
    }

    using (var os = provider.CreateObjectSpace()) {
        CurataDsc(os);

        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
        var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371"); // marfă
        var tip345 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "345"); // produs finit
        var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704"); // serviciu (VEN)
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var sdd = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
        Cont ContSimbol(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
        var cont4111 = ContSimbol("4111");
        var cont4427 = ContSimbol("4427");
        var cont607 = ContSimbol("607");
        var cont711 = ContSimbol("711");
        var cont707 = ContSimbol("707");
        var cont701 = ContSimbol("701");

        // --- Seed P2 privat ---
        var reguliStocDsc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocument.Cod == "DSC").ToList();
        Check("Seed DSC: −1 pe predator; generic→Magazie, MF→Marfuri (oglindește NIR/LDI privat)",
            reguliStocDsc.Count == 2 && reguliStocDsc.All(r => r.Latura == LaturaDocument.Predator && r.Semn == -1)
            && reguliStocDsc.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
            && reguliStocDsc.Any(r => r.TipStoc == TipStoc.Marfuri));
        RegulaContare CostDsc(TipMaterial tip) => os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "DSC" && r.TipMaterialId == tip.ID);
        Check("Seed DSC cost: 371→607=371, 345→711=345 (excepțiile profilului; credit = contul de stoc al Tipului)",
            CostDsc(tip371)?.ContDebit?.Simbol == "607" && CostDsc(tip371).SursaContCredit == SursaCont.TipMaterial
            && CostDsc(tip345)?.ContDebit?.Simbol == "711" && CostDsc(tip345).SursaContCredit == SursaCont.TipMaterial);
        RegulaContare VanzareFcl(TipMaterial tip) => os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "FCL" && r.TipMaterialId == tip.ID);
        Check("Seed FCL vânzare: 371→707, 345→701 (debit RepartitorPrimitor/4111, credit VENITUL — nu contul de stoc)",
            VanzareFcl(tip371)?.ContCredit?.Simbol == "707" && VanzareFcl(tip371).SursaContDebit == SursaCont.RepartitorPrimitor
            && VanzareFcl(tip371).ContDebit?.Simbol == "4111"
            && VanzareFcl(tip345)?.ContCredit?.Simbol == "701");
        Check("Seed FCL: rândul NaturaInterzisa=Stoc șters la privat (descărcarea preia vânzarea din stoc — 37e)",
            os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL") == null);
        Check("Seed DSC: numerotare DSC-; ancora fără TipTvaImplicit; N21 default pe FCT/FCL/DEC, NIR/DSC null (37f)",
            os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "DSC")?.Serie == "DSC-"
            && os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC")?.TipTvaImplicitId == null
            && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT")?.TipTvaImplicitId == n21.ID
            && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL")?.TipTvaImplicitId == n21.ID
            && os.FirstOrDefault<TipDocument>(t => t.Cod == "DEC")?.TipTvaImplicitId == n21.ID
            && os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR")?.TipTvaImplicitId == null);

        // --- Setup: furnizor, client, produse A/B (marfă 371), C (produs finit 345) ---
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = MarcajDsc + "-FURN";
        furnizor.Denumire = "Furnizor probă DSC";
        var client = os.CreateObject<Partener>();
        client.Cod = MarcajDsc + "-CL";
        client.Denumire = "Client probă DSC"; // fără ContImplicit → creanța pe fallback 4111
        Produs CreeazaProdus(string sufix, TipMaterial tip) {
            var p = os.CreateObject<Produs>();
            p.Cod = MarcajDsc + sufix;
            p.Denumire = "Produs probă DSC" + sufix;
            p.UM = "BUC";
            p.TipMaterial = tip;
            return p;
        }
        var produsA = CreeazaProdus("-A", tip371); // marfă
        var produsB = CreeazaProdus("-B", tip371); // marfă fără stoc inițial
        var produsC = CreeazaProdus("-C", tip345); // produs finit
        os.CommitChanges();

        List<RegistruContabil> Note(Document doc) =>
            os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

        // --- NIR-uri manuale la DATE diferite (FIFO determinist pe Lot.Data) ---
        var d1 = new DateOnly(2026, 4, 1);
        var d2 = new DateOnly(2026, 4, 5);
        var nir1 = os.CreateObject<NIR>();
        nir1.Data = d1; nir1.Predator = furnizor; nir1.Primitor = mag1;
        var linA1 = os.CreateObject<DocumentDetaliu>();
        linA1.Document = nir1; linA1.TipMaterial = tip371; linA1.Cantitate = 10m; linA1.Valoare = 50m;
        var lotA1 = linA1.CreeazaLot(os, produsA, mag1); // 5 lei/buc
        var linC1 = os.CreateObject<DocumentDetaliu>();
        linC1.Document = nir1; linC1.TipMaterial = tip345; linC1.Cantitate = 5m; linC1.Valoare = 40m;
        var lotC1 = linC1.CreeazaLot(os, produsC, mag1); // 8 lei/buc
        os.CommitChanges();
        MotorOperare.Opereaza(os, nir1);

        var nir2 = os.CreateObject<NIR>();
        nir2.Data = d2; nir2.Predator = furnizor; nir2.Primitor = mag1;
        var linA2 = os.CreateObject<DocumentDetaliu>();
        linA2.Document = nir2; linA2.TipMaterial = tip371; linA2.Cantitate = 10m; linA2.Valoare = 60m;
        var lotA2 = linA2.CreeazaLot(os, produsA, mag1); // 6 lei/buc (mai scump, mai nou)
        os.CommitChanges();
        MotorOperare.Opereaza(os, nir2);
        Check("Loturi de marfă finalizate la NET (A1=5, A2=6/buc, în Marfuri) + C1=8 (Magazie)",
            lotA1.PretUnitar == 5m && lotA2.PretUnitar == 6m && lotC1.PretUnitar == 8m
            && StocService.Sold(os, new CheieStoc(lotA1.ID, mag1.ID, TipStoc.Marfuri)) == 10m
            && StocService.Sold(os, new CheieStoc(lotA2.ID, mag1.ID, TipStoc.Marfuri)) == 10m
            && StocService.Sold(os, new CheieStoc(lotC1.ID, mag1.ID, TipStoc.Magazie)) == 5m);

        // --- Refuzuri prealabile ale culegerii FCL (General!+Specific?, §4) ---
        void RefuzFcl(string nume, Gestiune gest, Produs prod, Lot pin) {
            var f = os.CreateObject<FacturaIesire>();
            f.Data = new DateOnly(2026, 4, 8); f.Predator = sediu; f.Primitor = client;
            f.GestiuneDescarcare = gest;
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = f; d.TipMaterial = tip371; d.Cantitate = 1m; d.PretUnitar = 10m; d.TipTva = n21;
            d.Produs = prod;
            if (pin != null) d.Lot = pin;
            os.CommitChanges();
            CheckRefuza(nume, () => MotorOperare.Opereaza(os, f));
            os.Delete(f.Detalii.ToList());
            os.Delete(f);
            os.CommitChanges();
        }
        RefuzFcl("Linie de stoc fără ProdusId → refuz (identitatea liniei = produsul)", mag1, null, null);
        RefuzFcl("FCL cu linii de stoc fără GestiuneDescarcare → refuz", null, produsA, null);
        RefuzFcl("Pin pe lot care NU aparține produsului liniei → refuz", mag1, produsA, lotC1);
        RefuzFcl("Pin pe lot fără sold în gestiunea de descărcare (MAG2 goală) → refuz «întâi transfer (BTR)»", mag2, produsA, lotA1);

        // --- FCL: L1 FIFO(A,12), L2 pin(A,A2,5), L3 serviciu, L4 indisponibil(B,7), L5(C,3) ---
        var d3 = new DateOnly(2026, 4, 10);
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Data = d3; fcl.Predator = sediu; fcl.Primitor = client;
        fcl.GestiuneDescarcare = mag1;
        FacturaIesireDetaliu LinieFcl(TipMaterial tip, Produs prod, decimal cant, decimal pret, Lot pin = null) {
            var d = os.CreateObject<FacturaIesireDetaliu>();
            d.Document = fcl; d.TipMaterial = tip; d.Produs = prod; d.Cantitate = cant; d.PretUnitar = pret; d.TipTva = n21;
            if (pin != null) d.Lot = pin;
            return d;
        }
        var lFclA = LinieFcl(tip371, produsA, 12m, 10m);           // L1 FIFO → net 120
        var lFclApin = LinieFcl(tip371, produsA, 5m, 10m, lotA2);  // L2 pin A2 → net 50
        var lFclServ = LinieFcl(tip704, null, 1m, 100m);           // L3 serviciu → net 100
        var lFclB = LinieFcl(tip371, produsB, 7m, 9m);             // L4 indisponibil → net 63
        var lFclC = LinieFcl(tip345, produsC, 3m, 20m);            // L5 produs finit → net 60
        os.CommitChanges();

        var dscDraft = MotorOperare.Opereaza(os, fcl);

        // Notele FCL: venitul se postează ACUM (preț de vânzare), inclusiv pentru
        // poziția indisponibilă B; costul vine separat pe DSC.
        var noteFcl = Note(fcl);
        RegistruContabil Venit(FacturaIesireDetaliu l) => noteFcl.Single(n => n.DetaliuId == l.ID && n.ContCreditId != cont4427.ID);
        Check("FCL note: 4111=707 net pe marfă L1/L2/L4 (venitul se postează ACUM, inclusiv fără stoc)",
            Venit(lFclA).ContDebitId == cont4111.ID && Venit(lFclA).ContCreditId == cont707.ID && Venit(lFclA).Valoare == 120m
            && Venit(lFclApin).ContCreditId == cont707.ID && Venit(lFclApin).Valoare == 50m
            && Venit(lFclB).ContCreditId == cont707.ID && Venit(lFclB).Valoare == 63m);
        Check("FCL note: 4111=701 pe produsul finit L5 (60), 4111=704 pe serviciul L3 (100)",
            Venit(lFclC).ContCreditId == cont701.ID && Venit(lFclC).Valoare == 60m
            && Venit(lFclServ).ContCreditId == tip704.ContImplicitId && Venit(lFclServ).Valoare == 100m);
        Check("FCL: 4427 colectat per linie (N21); 10 note total (5 venit + 5 TVA)",
            noteFcl.Count == 10 && noteFcl.Count(n => n.ContDebitId == cont4111.ID && n.ContCreditId == cont4427.ID) == 5);

        // Opereaza întoarce DSC-ul draft (secundar); spargerea pe loturi la GENERARE.
        var dsc = (DescarcareGestiune)dscDraft;
        var dd = dsc.Detalii.OfType<DescarcareGestiuneDetaliu>().ToList();
        Check("Conex DSC: draft autogenerat, DocumentSursa=FCL, predator=gestiune, primitor=client",
            dsc.Stare == StareDocument.Draft && dsc.Autogenerat && dsc.DocumentSursaId == fcl.ID
            && dsc.PredatorId == mag1.ID && dsc.PrimitorId == client.ID);
        var linL2 = dd.Where(x => x.LinieSursaId == lFclApin.ID).ToList();
        Check("Spargere PIN ÎNTÂI: L2 → 1 rând lot A2, 5 buc, cost 30",
            linL2.Count == 1 && linL2[0].LotId == lotA2.ID && linL2[0].Cantitate == 5m && linL2[0].Valoare == 30m);
        Check("Spargere FIFO: L1 → A1 10 buc (50) + A2 2 buc (12, din 10−5 rămase după pin)",
            dd.Count(x => x.LinieSursaId == lFclA.ID) == 2
            && dd.Any(x => x.LinieSursaId == lFclA.ID && x.LotId == lotA1.ID && x.Cantitate == 10m && x.Valoare == 50m)
            && dd.Any(x => x.LinieSursaId == lFclA.ID && x.LotId == lotA2.ID && x.Cantitate == 2m && x.Valoare == 12m));
        var linL5 = dd.Where(x => x.LinieSursaId == lFclC.ID).ToList();
        Check("Spargere: L5 → C1 3 buc (24); L4 indisponibil → nicio linie; total 4 rânduri, cost 116",
            linL5.Count == 1 && linL5[0].LotId == lotC1.ID && linL5[0].Cantitate == 3m && linL5[0].Valoare == 24m
            && !dd.Any(x => x.LinieSursaId == lFclB.ID) && dd.Count == 4 && dd.Sum(x => x.Valoare) == 116m);

        var resturi = DescarcareService.RestNedescarcat(os, fcl);
        Check("RestNedescarcat (cusătura §2.2): L4 rest 7, celelalte 0",
            resturi.Single(x => x.LinieId == lFclB.ID).RestNeacoperit == 7m
            && resturi.Where(x => x.LinieId != lFclB.ID).All(x => x.RestNeacoperit == 0m));

        // --- Operare DSC: cost 6xx=3xx (≠ vânzarea), −stoc pe gestiune, dim ambele laturi pe gestiune ---
        Check("DSC nu generează conex", MotorOperare.Opereaza(os, dsc) == null);
        Check("DSC operat cu număr din politică", dsc.Stare == StareDocument.Operat && dsc.Numar?.StartsWith("DSC-") == true);
        var noteDsc = Note(dsc);
        Check("Cost marfă: 607 = 371 la COST 92 (30+50+12) — decuplat de vânzarea 707 (233)",
            noteDsc.Where(n => n.ContDebitId == cont607.ID).Sum(n => n.Valoare) == 92m
            && noteDsc.Where(n => n.ContDebitId == cont607.ID).All(n => n.ContCreditId == tip371.ContImplicitId)
            && noteFcl.Where(n => n.ContCreditId == cont707.ID).Sum(n => n.Valoare) == 233m);
        var notaPf = noteDsc.Single(n => n.ContDebitId == cont711.ID);
        Check("Cost produs finit: 711 = 345, 24; exact 4 note pe DSC",
            notaPf.ContCreditId == tip345.ContImplicitId && notaPf.Valoare == 24m && noteDsc.Count == 4);
        Check("DSC dimensiuni: AMBELE laturi Repartitor=gestiunea (override polimorf), Material din lot",
            noteDsc.All(n => n.DimensiuniDebit.RepartitorId == mag1.ID && n.DimensiuniCredit.RepartitorId == mag1.ID)
            && noteDsc.Where(n => n.ContDebitId == cont607.ID).All(n => n.DimensiuniDebit.MaterialId == produsA.ID)
            && notaPf.DimensiuniDebit.MaterialId == produsC.ID);
        var stocDsc = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == dsc.ID).ToList();
        Check("DSC stoc: −10 A1, −7 A2 (Marfuri), −3 C1 (Magazie), toate pe gestiune",
            stocDsc.Where(r => r.LotId == lotA1.ID).Sum(r => r.Cantitate) == -10m
            && stocDsc.Where(r => r.LotId == lotA2.ID).Sum(r => r.Cantitate) == -7m
            && stocDsc.Where(r => r.LotId == lotC1.ID).Sum(r => r.Cantitate) == -3m
            && stocDsc.Where(r => r.LotId == lotA1.ID || r.LotId == lotA2.ID).All(r => r.TipStoc == TipStoc.Marfuri)
            && stocDsc.Single(r => r.LotId == lotC1.ID).TipStoc == TipStoc.Magazie
            && stocDsc.All(r => r.RepartitorId == mag1.ID));

        // --- Backorder (§5): recepția produsului B, apoi Genereaza direct (acțiunea manuală) ---
        var d4 = new DateOnly(2026, 4, 15);
        var nir3 = os.CreateObject<NIR>();
        nir3.Data = d4; nir3.Predator = furnizor; nir3.Primitor = mag1;
        var linB1 = os.CreateObject<DocumentDetaliu>();
        linB1.Document = nir3; linB1.TipMaterial = tip371; linB1.Cantitate = 7m; linB1.Valoare = 28m;
        var lotB1 = linB1.CreeazaLot(os, produsB, mag1); // 4 lei/buc
        os.CommitChanges();
        MotorOperare.Opereaza(os, nir3);

        var dsc2 = DescarcareService.Genereaza(os, fcl, d4);
        os.CommitChanges();
        var db2 = dsc2?.Detalii.OfType<DescarcareGestiuneDetaliu>().SingleOrDefault();
        Check("Backorder: DSC₂ autogenerat DOAR pe linia B (7 buc, cost 28), DocumentSursa=FCL",
            dsc2 != null && dsc2.Autogenerat && dsc2.DocumentSursaId == fcl.ID && dsc2.Detalii.Count == 1
            && db2 != null && db2.LinieSursaId == lFclB.ID && db2.LotId == lotB1.ID && db2.Cantitate == 7m && db2.Valoare == 28m);
        Check("A doua apelare Genereaza → null (acoperirea completă; draftul contează)",
            DescarcareService.Genereaza(os, fcl, d4) == null);
        MotorOperare.Opereaza(os, dsc2);
        var n2 = Note(dsc2).Single();
        Check("DSC₂ operat: 607 = 371 la costul B (28)",
            n2.ContDebitId == cont607.ID && n2.ContCreditId == tip371.ContImplicitId && n2.Valoare == 28m);

        // --- Gardieni de grup + storno care redeschide restul ---
        var d5 = new DateOnly(2026, 4, 20);
        CheckRefuza("Anularea FCL cu DSC operat → refuz", () => MotorOperare.AnuleazaOperarea(os, fcl));
        CheckRefuza("Stornarea FCL cu DSC operat → refuz", () => MotorOperare.Storneaza(os, fcl, d5));
        MotorOperare.Storneaza(os, dsc2, d5);
        Check("Storno DSC₂ → Stornat, B1 revenit în stoc",
            dsc2.Stare == StareDocument.Stornat
            && StocService.Sold(os, new CheieStoc(lotB1.ID, mag1.ID, TipStoc.Marfuri), d5) == 7m);
        var dsc3 = DescarcareService.Genereaza(os, fcl, d5);
        var db3 = dsc3?.Detalii.OfType<DescarcareGestiuneDetaliu>().SingleOrDefault();
        Check("Stornat NU acoperă: Genereaza redeschide restul L4 (DSC₃, B1 7 buc)",
            dsc3 != null && dsc3.Detalii.Count == 1 && db3 != null
            && db3.LinieSursaId == lFclB.ID && db3.LotId == lotB1.ID && db3.Cantitate == 7m);
        os.Delete(dsc3.Detalii.ToList());
        os.Delete(dsc3);
        os.CommitChanges();

        // --- Anularea cu draft: operarea unei FCL noi generează un DSC draft; anularea îl șterge ---
        var d6 = new DateOnly(2026, 4, 25);
        var fclMini = os.CreateObject<FacturaIesire>();
        fclMini.Data = d6; fclMini.Predator = sediu; fclMini.Primitor = client;
        fclMini.GestiuneDescarcare = mag1;
        var lMini = os.CreateObject<FacturaIesireDetaliu>();
        lMini.Document = fclMini; lMini.TipMaterial = tip371; lMini.Produs = produsA;
        lMini.Cantitate = 2m; lMini.PretUnitar = 10m; lMini.TipTva = n21;
        os.CommitChanges();
        var dscMini = MotorOperare.Opereaza(os, fclMini);
        Check("FCL₂ operată generează un DSC draft autogenerat",
            dscMini is DescarcareGestiune { Stare: StareDocument.Draft, Autogenerat: true }
            && os.GetObjectsQuery<DescarcareGestiune>().Any(x => x.DocumentSursaId == fclMini.ID));
        MotorOperare.AnuleazaOperarea(os, fclMini);
        Check("Anularea FCL₂ ȘTERGE DSC-ul draft autogenerat (gardianul existent); FCL₂ pe Draft",
            fclMini.Stare == StareDocument.Draft
            && !os.GetObjectsQuery<DescarcareGestiune>().Any(x => x.DocumentSursaId == fclMini.ID));

        // --- Datoria P1: default TipTva de CULEGERE (N21); culegerea explicită bate ---
        var fclTva = os.CreateObject<FacturaIesire>();
        fclTva.Data = d6; fclTva.Predator = sediu; fclTva.Primitor = client;
        var lNoTva = os.CreateObject<FacturaIesireDetaliu>();
        lNoTva.Document = fclTva; lNoTva.TipMaterial = tip704; lNoTva.Cantitate = 1m; lNoTva.PretUnitar = 10m;
        var lExplicit = os.CreateObject<FacturaIesireDetaliu>();
        lExplicit.Document = fclTva; lExplicit.TipMaterial = tip704; lExplicit.Cantitate = 1m; lExplicit.PretUnitar = 10m; lExplicit.TipTva = sdd;
        TvaService.AplicaTipTvaImplicit(os, fclTva, lNoTva);
        TvaService.AplicaTipTvaImplicit(os, fclTva, lExplicit);
        Check("Default TipTva: linia fără TipTva primește N21; linia cu SDD explicit rămâne neatinsă",
            lNoTva.TipTvaId == n21.ID && lExplicit.TipTvaId == sdd.ID);
        os.CommitChanges();

        // --- Defecte găsite la review advers (P2): validări noi de integritate ---
        // Fiecare pe obiecte throwaway, șterse imediat (tipul nou nu e prins de
        // markerul de doc al CurataDsc — se șterge explicit).

        // Defect 1: Tip de stoc creat DUPĂ seed → fără regulă de contare derivată.
        var tipNou = os.CreateObject<TipMaterial>();
        tipNou.Cod = MarcajDsc + "-TIPNOU";
        tipNou.Denumire = "Tip de stoc nou (post-seed, fără reguli)";
        tipNou.Clasa = tip371.Clasa; // MF (Natura=Stoc) — dar fără rând de vânzare/cost
        var produsNou = CreeazaProdus("-N", tipNou);
        os.CommitChanges();

        var fclDef1 = os.CreateObject<FacturaIesire>();
        fclDef1.Data = d6; fclDef1.Predator = sediu; fclDef1.Primitor = client; fclDef1.GestiuneDescarcare = mag1;
        var lFclDef1 = os.CreateObject<FacturaIesireDetaliu>();
        lFclDef1.Document = fclDef1; lFclDef1.TipMaterial = tipNou; lFclDef1.Produs = produsNou;
        lFclDef1.Cantitate = 1m; lFclDef1.PretUnitar = 10m; lFclDef1.TipTva = n21;
        os.CommitChanges();
        CheckRefuza("Defect 1 (FCL): linie de stoc cu Tip nou fără regulă de vânzare → refuz",
            () => MotorOperare.Opereaza(os, fclDef1));
        os.Delete(fclDef1.Detalii.ToList());
        os.Delete(fclDef1);
        os.CommitChanges();

        // Simetric pe DSC manual: Tip fără regulă de cost (ar mișca stoc fără notă).
        var lotNou = os.CreateObject<Lot>();
        lotNou.Produs = produsNou; lotNou.Gestiune = mag1; lotNou.PretUnitar = 5m; lotNou.Data = d1;
        var dscDef1 = os.CreateObject<DescarcareGestiune>();
        dscDef1.Data = d6; dscDef1.Predator = mag1; dscDef1.Primitor = client;
        var lDscDef1 = os.CreateObject<DescarcareGestiuneDetaliu>();
        lDscDef1.Document = dscDef1; lDscDef1.TipMaterial = tipNou; lDscDef1.Lot = lotNou; lDscDef1.Cantitate = 1m;
        os.CommitChanges();
        CheckRefuza("Defect 1 (DSC manual): linie cu Tip fără regulă de cost → refuz",
            () => MotorOperare.Opereaza(os, dscDef1));
        os.Delete(dscDef1.Detalii.ToList());
        os.Delete(dscDef1);
        os.Delete(lotNou);
        os.Delete(produsNou);
        os.Delete(tipNou);
        os.CommitChanges();

        // Defect 4: produs de ALT Tip decât Tipul liniei (linie tip371 × produsC/345).
        RefuzFcl("Defect 4 (FCL): linie de stoc cu produs de alt Tip decât Tipul liniei → refuz", mag1, produsC, null);

        // Defect 7: linie de BAZĂ DocumentDetaliu (ne-derivată) pe FCL ar ocoli General!+Specific?.
        var fclDef7 = os.CreateObject<FacturaIesire>();
        fclDef7.Data = d6; fclDef7.Predator = sediu; fclDef7.Primitor = client;
        var lDef7 = os.CreateObject<DocumentDetaliu>(); // NU FacturaIesireDetaliu
        lDef7.Document = fclDef7; lDef7.TipMaterial = tip704; lDef7.Cantitate = 1m;
        os.CommitChanges();
        CheckRefuza("Defect 7 (FCL): linie de bază DocumentDetaliu (ne-derivată, «detaliu generic») → refuz",
            () => MotorOperare.Opereaza(os, fclDef7));
        os.Delete(fclDef7.Detalii.ToList());
        os.Delete(fclDef7);
        os.CommitChanges();

        // Defect 2: DSC manual (fără DocumentSursa) cu LinieSursaId spre o linie a
        // FCL-ului viu → refuz; ȘI RestNedescarcat filtrează pe DocumentSursa, deci
        // draftul STRĂIN nu otrăvește acoperirea L4 (rest 7 neschimbat) cât există.
        var poison = os.CreateObject<DescarcareGestiune>();
        poison.Data = d6; poison.Predator = mag1; poison.Primitor = client; // fără DocumentSursa
        var lPoison = os.CreateObject<DescarcareGestiuneDetaliu>();
        lPoison.Document = poison; lPoison.TipMaterial = tip371; lPoison.Lot = lotA1;
        lPoison.Cantitate = 7m; lPoison.LinieSursaId = lFclB.ID;
        os.CommitChanges();
        Check("Defect 2: draftul străin (alt DocumentSursa) NU otrăvește acoperirea — L4 rest rămâne 7",
            DescarcareService.RestNedescarcat(os, fcl).Single(x => x.LinieId == lFclB.ID).RestNeacoperit == 7m);
        CheckRefuza("Defect 2: DSC manual (fără DocumentSursa) cu LinieSursaId → refuz",
            () => MotorOperare.Opereaza(os, poison));
        os.Delete(poison.Detalii.ToList());
        os.Delete(poison);
        os.CommitChanges();

        CurataDsc(os);
        Check("Curățenie finală DSC (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajDsc))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajDsc)));
    }

    // ================= Scenariul e2e 1C-a: NotaContabila (privat) =================
    // Nota e NEUTRĂ față de profil (fără plan hardcodat, fără politici în afara
    // numerotării): aceleași mecanisme, conturi OMFP. La privat niciun cont nu
    // cere dimensiuni, deci rămâne nucleul — postare explicită, valoare CA ATARE
    // (inclusiv negativă), storno.
    {
        const string MarcajNtcPrv = "E2E-NTP";

        void CurataNtcPrv(IObjectSpace os) {
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtcPrv)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            os.Delete(docs);
            os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtcPrv)).ToList());
            os.CommitChanges();
        }

        using (var os = provider.CreateObjectSpace()) {
            CurataNtcPrv(os);

            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var cont605 = os.FirstOrDefault<Cont>(c => c.Simbol == "605");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            var cont471 = os.FirstOrDefault<Cont>(c => c.Simbol == "471");

            var tipNtc = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");
            Check("Seed NTC privat: ancoră + numerotare NTC-, fără reguli de stoc/contare și fără PoliticaTva",
                tipNtc != null
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC")?.Serie == "NTC-"
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipNtc.ID)
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipNtc.ID)
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipNtc.ID) == null);

            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = MarcajNtcPrv + "-UI";
            unitate.Denumire = "Unitate probă notă privat";
            os.CommitChanges();

            var ntc = os.CreateObject<NotaContabila>();
            ntc.Data = new DateOnly(2026, 4, 6);
            ntc.Predator = sediu;
            ntc.Primitor = unitate;
            var linieUtilitati = os.CreateObject<NotaContabilaDetaliu>();
            linieUtilitati.Document = ntc;
            linieUtilitati.TipMaterial = tipTrz;
            linieUtilitati.Descriere = "Utilități de regularizat";
            linieUtilitati.ContDebit = cont605;
            linieUtilitati.ContCredit = cont401;
            linieUtilitati.Valoare = 300m;
            var linieMinus = os.CreateObject<NotaContabilaDetaliu>();
            linieMinus.Document = ntc;
            linieMinus.TipMaterial = tipTrz;
            linieMinus.Descriere = "Reportare în avans (minus)";
            linieMinus.ContDebit = cont471;
            linieMinus.ContCredit = cont605;
            linieMinus.Valoare = -50m;
            os.CommitChanges();

            MotorOperare.Opereaza(os, ntc);
            Check("Privat: operare → Operat + număr NTC-",
                ntc.Stare == StareDocument.Operat && ntc.Numar?.StartsWith("NTC-") == true);
            var notePrv = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == ntc.ID && !r.Storno).ToList();
            Check("Privat: exact 2 rânduri, conturile OMFP explicite (605 = 401; 471 = 605)", notePrv.Count == 2
                && notePrv.Any(n => n.ContDebitId == cont605.ID && n.ContCreditId == cont401.ID)
                && notePrv.Any(n => n.ContDebitId == cont471.ID && n.ContCreditId == cont605.ID));
            Check("Privat: valorile CA ATARE (300 / −50)",
                notePrv.Any(n => n.Valoare == 300m) && notePrv.Any(n => n.Valoare == -50m));
            Check("Privat: dimensiunile din default-ul polimorf (debit←predator, credit←primitor)",
                notePrv.All(n => n.DimensiuniDebit.RepartitorId == sediu.ID
                    && n.DimensiuniCredit.RepartitorId == unitate.ID));
            Check("Privat: nota nu postează TVA (fără TipTva pe linii) și nu mișcă stoc",
                notePrv.All(n => n.DetaliuId != null)
                && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == ntc.ID));

            MotorOperare.Storneaza(os, ntc, new DateOnly(2026, 7, 22));
            var toatePrv = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == ntc.ID).ToList();
            Check("Privat: storno → rânduri inverse (−300 / +50) la data stornării",
                ntc.Stare == StareDocument.Stornat && toatePrv.Count == 4
                && toatePrv.Count(r => r.Storno && r.Data == new DateOnly(2026, 7, 22)
                    && (r.Valoare == -300m || r.Valoare == 50m)) == 2);

            CurataNtcPrv(os);
            Check("Curățenie finală notă contabilă privat (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNtcPrv)));
        }
    }

    // ================= Scenariul e2e 1C-a: InchidereTva (privat) =================
    // Închiderea lunară de TVA (design FAZA 1C §6) — forcing function-ul TVA-ului
    // structural din P1: ITV E o notă contabilă GENERATĂ din soldurile registrului
    // (InchidereTvaService), nu culeasă. Acoperă: transferul 4427=4426 pe minim +
    // excedentul de plată (septembrie), idempotența pe draft existent, excedentul
    // de recuperat 4424=4426 (octombrie — cumulativul dovedește că septembrie s-a
    // închis la 0), regenerarea DUPĂ storno.
    // Lunile 9/10 sunt NEFOLOSITE de celelalte blocuri (care lucrează în 3–7 și se
    // curăță după ele) — precondiția de sold 0 se verifică explicit.
    {
        const string MarcajItv = "E2E-ITV";

        void CurataItv(IObjectSpace os) {
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajItv)).Select(r => r.ID).ToList();
            // Documentele după repartitorii marcați prind și ITV-urile: laturile
            // lor sunt unitatea marcată (predator = primitor).
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            os.Delete(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentTrezorerieId) || docIds.Contains(i.DocumentId)).ToList());
            os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                os.Delete(doc);
            os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajItv)).ToList());
            os.CommitChanges();
        }

        using (var os = provider.CreateObjectSpace()) {
            CurataItv(os);

            Cont ContItv(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
            var cont4423 = ContItv("4423");
            var cont4424 = ContItv("4424");
            var cont4426 = ContItv("4426");
            var cont4427 = ContItv("4427");
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
            var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");

            var tipItv = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
            var politicaItv = os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocument.Cod == "ITV");
            Check("Seed ITV privat: ancoră + numerotare ITV- + conturile închiderii ca DATE (4426/4427/4423/4424), fără reguli de stoc/contare",
                tipItv != null && tipItv.ClrType == nameof(InchidereTva)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "ITV")?.Serie == "ITV-"
                && politicaItv != null
                && politicaItv.ContDeductibilaId == cont4426.ID && politicaItv.ContColectataId == cont4427.ID
                && politicaItv.ContDePlataId == cont4423.ID && politicaItv.ContDeRecuperatId == cont4424.ID
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipItv.ID)
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipItv.ID));

            // Soldurile CUMULATE, exact ca în serviciu (4426 activ, 4427 pasiv).
            decimal SoldDebitor(Cont cont, DateOnly la) =>
                (os.GetObjectsQuery<RegistruContabil>().Where(r => r.Data <= la && r.ContDebitId == cont.ID).Sum(r => (decimal?)r.Valoare) ?? 0m)
                - (os.GetObjectsQuery<RegistruContabil>().Where(r => r.Data <= la && r.ContCreditId == cont.ID).Sum(r => (decimal?)r.Valoare) ?? 0m);
            decimal SoldCreditor(Cont cont, DateOnly la) => -SoldDebitor(cont, la);

            var finalSep = new DateOnly(2026, 9, 30);
            var finalOct = new DateOnly(2026, 10, 31);
            Check("Precondiție: soldurile cumulate 4426/4427 la 30.09.2026 sunt 0 (blocurile anterioare s-au curățat)",
                SoldDebitor(cont4426, finalSep) == 0m && SoldCreditor(cont4427, finalSep) == 0m);

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajItv + "-FURN";
            furnizor.Denumire = "Furnizor probă închidere TVA";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajItv + "-CL";
            client.Denumire = "Client probă închidere TVA";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = MarcajItv + "-MAG";
            gestiune.Denumire = "Gestiune probă închidere TVA";
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = MarcajItv + "-UI";
            unitate.Denumire = "Unitate probă închidere TVA"; // laturile ITV
            os.CommitChanges();

            List<RegistruContabil> NoteItv(Document doc) =>
                os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

            // --- Septembrie: 4426 = 21 (FCT servicii 100) și 4427 = 42 (FCL 200) ---
            var fctSep = os.CreateObject<FacturaIntrare>();
            fctSep.Numar = "E2E-ITV-FF1";
            fctSep.Data = new DateOnly(2026, 9, 10);
            fctSep.Predator = furnizor;
            fctSep.Primitor = gestiune;
            var linieServiciuSep = os.CreateObject<FacturaIntrareDetaliu>();
            linieServiciuSep.Document = fctSep;
            linieServiciuSep.TipMaterial = tip628;
            linieServiciuSep.Cantitate = 1m;
            linieServiciuSep.PretUnitar = 100m;
            linieServiciuSep.TipTva = n21;

            var fclSep = os.CreateObject<FacturaIesire>();
            fclSep.Data = new DateOnly(2026, 9, 12);
            fclSep.Predator = unitate;
            fclSep.Primitor = client;
            var linieVenitSep = os.CreateObject<FacturaIesireDetaliu>();
            linieVenitSep.Document = fclSep;
            linieVenitSep.TipMaterial = tip704;
            linieVenitSep.Cantitate = 1m;
            linieVenitSep.PretUnitar = 200m;
            linieVenitSep.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctSep);
            MotorOperare.Opereaza(os, fclSep);
            Check("Septembrie: TVA-ul lunii în registru — 4426 deductibilă 21, 4427 colectată 42",
                SoldDebitor(cont4426, finalSep) == 21m && SoldCreditor(cont4427, finalSep) == 42m);

            var itvSep = InchidereTvaService.Genereaza(os, 2026, 9, unitate.ID);
            Check("Generare septembrie: draft ITV la ultima zi a lunii, laturi = unitatea internă, NEautogenerat (document lunar de primă clasă)",
                itvSep != null && itvSep.Stare == StareDocument.Draft && itvSep.Data == finalSep
                && itvSep.PredatorId == unitate.ID && itvSep.PrimitorId == unitate.ID && !itvSep.Autogenerat);
            var liniiSep = itvSep.Detalii.OfType<NotaContabilaDetaliu>().ToList();
            Check("Generare septembrie: EXACT 2 linii — transferul 4427 = 4426 pe minim (21) și TVA de plată 4427 = 4423 (21); fără rând de recuperat",
                itvSep.Detalii.Count == 2 && liniiSep.Count == 2
                && liniiSep.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4426.ID && l.Valoare == 21m)
                && liniiSep.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4423.ID && l.Valoare == 21m));
            os.CommitChanges();
            Check("Idempotență: a doua chemare pe aceeași lună → null (draftul existent ține locul)",
                InchidereTvaService.Genereaza(os, 2026, 9, unitate.ID) == null);

            MotorOperare.Opereaza(os, itvSep);
            Check("Operare septembrie: Operat + număr din politică (ITV-), rândurile contabile = liniile",
                itvSep.Stare == StareDocument.Operat && itvSep.Numar?.StartsWith("ITV-") == true
                && NoteItv(itvSep).Count == 2 && NoteItv(itvSep).All(n => n.DetaliuId != null));
            Check("După închidere: 4423 TVA de plată = 21, iar 4426/4427 rămân la 0 pe 30.09",
                SoldCreditor(cont4423, finalSep) == 21m
                && SoldDebitor(cont4426, finalSep) == 0m && SoldCreditor(cont4427, finalSep) == 0m);

            // --- Octombrie: doar achiziție (4426 = 10,5) → excedent de recuperat ---
            var fctOct = os.CreateObject<FacturaIntrare>();
            fctOct.Numar = "E2E-ITV-FF2";
            fctOct.Data = new DateOnly(2026, 10, 5);
            fctOct.Predator = furnizor;
            fctOct.Primitor = gestiune;
            var linieServiciuOct = os.CreateObject<FacturaIntrareDetaliu>();
            linieServiciuOct.Document = fctOct;
            linieServiciuOct.TipMaterial = tip628;
            linieServiciuOct.Cantitate = 1m;
            linieServiciuOct.PretUnitar = 50m;
            linieServiciuOct.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctOct);

            var itvOct = InchidereTvaService.Genereaza(os, 2026, 10, unitate.ID);
            var liniiOct = itvOct?.Detalii.OfType<NotaContabilaDetaliu>().ToList() ?? [];
            Check("Generare octombrie: O SINGURĂ linie — TVA de recuperat 4424 = 4426 (10,5); soldul cumulat dovedește că septembrie s-a închis la 0",
                itvOct != null && itvOct.Data == finalOct && itvOct.Detalii.Count == 1
                && liniiOct.Single().ContDebitId == cont4424.ID
                && liniiOct.Single().ContCreditId == cont4426.ID && liniiOct.Single().Valoare == 10.5m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, itvOct);
            Check("Operare octombrie: 4424 TVA de recuperat = 10,5 și 4426 revine la 0",
                itvOct.Stare == StareDocument.Operat
                && SoldDebitor(cont4424, finalOct) == 10.5m && SoldDebitor(cont4426, finalOct) == 0m);

            // --- Storno pe ULTIMA închidere → regenerarea lunii e permisă ---
            // (stornarea septembrie ar strica soldurile lui octombrie deja operat).
            MotorOperare.Storneaza(os, itvOct, finalOct);
            Check("Storno octombrie: rânduri inverse la data stornării, soldul de recuperat revine în 4426",
                itvOct.Stare == StareDocument.Stornat
                && SoldDebitor(cont4424, finalOct) == 0m && SoldDebitor(cont4426, finalOct) == 10.5m);
            var itvOctBis = InchidereTvaService.Genereaza(os, 2026, 10, unitate.ID);
            var liniiOctBis = itvOctBis?.Detalii.OfType<NotaContabilaDetaliu>().ToList() ?? [];
            Check("Regenerare după storno: draft NOU cu aceeași linie 4424 = 4426 (10,5) — guard-ul de idempotență exclude Stornatul",
                itvOctBis != null && itvOctBis.ID != itvOct.ID && itvOctBis.Detalii.Count == 1
                && liniiOctBis.Single().ContDebitId == cont4424.ID && liniiOctBis.Single().Valoare == 10.5m);
            os.Delete(itvOctBis.Detalii.ToList());
            os.Delete(itvOctBis);
            os.CommitChanges();

            CurataItv(os);
            Check("Curățenie finală închidere TVA (fără reziduuri e2e; soldurile de TVA revin la 0)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajItv))
                && !os.GetObjectsQuery<InchidereTva>().Any()
                && SoldDebitor(cont4426, finalOct) == 0m && SoldCreditor(cont4427, finalOct) == 0m);
        }
    }

    // ================= Scenariul e2e 1C-a: Asamblare (ASM) =================
    // Kitting n→m pe stoc (design FAZA 1C §7): consumurile descarcă loturi
    // EXISTENTE (preț lot × cantitate, pattern BCS), liniile de produs CREEAZĂ
    // lot cu valoare explicită (PretEvaluare, ca LDI-plus), iar invariantul
    // Σ produse = Σ consumuri ține valoarea în patrimoniu. Direcția explicită se
    // materializează în SEMN (mecanismul LDI 28a) peste UN SINGUR set de reguli
    // de stoc (+1 pe predator). Stocul se mișcă FĂRĂ notă contabilă (23c:
    // marfă→marfă la sintetic = zgomot) — verificat explicit.
    // Luna noiembrie 2026 e nefolosită de celelalte blocuri.
    {
        const string MarcajAsm = "E2E-ASM";

        void CurataAsm(IObjectSpace os) {
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAsm)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                os.Delete(doc);
            os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajAsm)).ToList());
            os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajAsm)).ToList());
            os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAsm)).ToList());
            os.CommitChanges();
        }

        using (var os = provider.CreateObjectSpace()) {
            CurataAsm(os);

            var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371"); // marfă (MF)
            var tipAsm = os.FirstOrDefault<TipDocument>(t => t.Cod == "ASM");

            // --- Seed ASM privat ---
            var reguliStocAsm = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipAsm.ID).ToList();
            Check("Seed ASM: ancoră TipDocument + numerotare ASM-; UN SINGUR set de reguli de stoc (+1 pe predator; generic→Magazie, MF→Marfuri)",
                tipAsm != null && tipAsm.ClrType == nameof(Asamblare)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipAsm.ID)?.Serie == "ASM-"
                && reguliStocAsm.Count == 2
                && reguliStocAsm.All(r => r.Latura == LaturaDocument.Predator && r.Semn == +1)
                && reguliStocAsm.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                && reguliStocAsm.Any(r => r.TipStoc == TipStoc.Marfuri));
            Check("Seed ASM: FĂRĂ reguli de contare (marfă→marfă la sintetic = zgomot — 23c) și fără politici de TVA/scadență/validare",
                !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipAsm.ID)
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipAsm.ID) == null
                && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipAsm.ID) == null
                && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipAsm.ID) == null
                && os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipAsm.ID) == null);

            // --- Fixtures: furnizor, gestiunea de lucru, 2 componente + kitul ---
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajAsm + "-FURN";
            furnizor.Denumire = "Furnizor probă asamblare";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = MarcajAsm + "-G";
            gestiune.Denumire = "Gestiune probă asamblare";
            Produs ProdusAsm(string sufix) {
                var p = os.CreateObject<Produs>();
                p.Cod = MarcajAsm + sufix;
                p.Denumire = "Produs probă asamblare" + sufix;
                p.UM = "BUC";
                p.TipMaterial = tip371; // kitting pe marfă (ca la flax)
                return p;
            }
            var produsA = ProdusAsm("-A");
            var produsB = ProdusAsm("-B");
            var produsKit = ProdusAsm("-KIT");
            os.CommitChanges();

            List<RegistruStoc> Stoc(Document doc) =>
                os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

            // --- NIR manual: loturile componentelor (10 × 5 lei, 4 × 10 lei) ---
            var dNir = new DateOnly(2026, 11, 2);
            var nir = os.CreateObject<NIR>();
            nir.Data = dNir; nir.Predator = furnizor; nir.Primitor = gestiune;
            var linNirA = os.CreateObject<DocumentDetaliu>();
            linNirA.Document = nir; linNirA.TipMaterial = tip371; linNirA.Cantitate = 10m; linNirA.Valoare = 50m;
            var lotA = linNirA.CreeazaLot(os, produsA, gestiune);
            var linNirB = os.CreateObject<DocumentDetaliu>();
            linNirB.Document = nir; linNirB.TipMaterial = tip371; linNirB.Cantitate = 4m; linNirB.Valoare = 40m;
            var lotB = linNirB.CreeazaLot(os, produsB, gestiune);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir);
            Check("Precondiție ASM: loturile componentelor pe Marfuri (A 10 buc × 5, B 4 buc × 10)",
                lotA.PretUnitar == 5m && lotB.PretUnitar == 10m
                && StocService.Sold(os, new CheieStoc(lotA.ID, gestiune.ID, TipStoc.Marfuri)) == 10m
                && StocService.Sold(os, new CheieStoc(lotB.ID, gestiune.ID, TipStoc.Marfuri)) == 4m);

            // Fabrica de linii: rolul + cantitatea POZITIVĂ culese, lotul fie
            // existent (consum), fie născut pe linie (produs).
            AsamblareDetaliu LinieAsm(Asamblare doc, DirectieAsamblare directie, decimal cantitate,
                Lot lot = null, decimal? pretEvaluare = null, Produs produsNou = null, Gestiune gestiuneLot = null) {
                var d = os.CreateObject<AsamblareDetaliu>();
                d.Document = doc;
                d.TipMaterial = tip371;
                d.Directie = directie;
                d.Cantitate = cantitate;
                d.PretEvaluare = pretEvaluare;
                if (lot != null)
                    d.Lot = lot;
                if (produsNou != null)
                    d.CreeazaLot(os, produsNou, gestiuneLot ?? gestiune);
                return d;
            }

            // --- Asamblarea: 6 × A (30) + 2 × B (20) → 2 kituri × 25 (50) ---
            var dAsm = new DateOnly(2026, 11, 5);
            var asm = os.CreateObject<Asamblare>();
            asm.Data = dAsm; asm.Predator = gestiune; asm.Primitor = gestiune; // aceeași gestiune
            var consumA = LinieAsm(asm, DirectieAsamblare.Consum, 6m, lotA);
            var consumB = LinieAsm(asm, DirectieAsamblare.Consum, 2m, lotB);
            var produsKitLinie = LinieAsm(asm, DirectieAsamblare.Produs, 2m, pretEvaluare: 25m, produsNou: produsKit);
            var lotKit = produsKitLinie.Lot;
            os.CommitChanges();

            Check("Asamblarea nu generează conex/secundar", MotorOperare.Opereaza(os, asm) == null);
            Check("Operare → Operat + număr din politică (ASM-)",
                asm.Stare == StareDocument.Operat && asm.Numar?.StartsWith("ASM-") == true);
            Check("Semnarea direcției: consumurile devin negative (−6/−30, −2/−20), produsul rămâne pozitiv (+2/+50)",
                consumA.Cantitate == -6m && consumA.Valoare == -30m
                && consumB.Cantitate == -2m && consumB.Valoare == -20m
                && produsKitLinie.Cantitate == 2m && produsKitLinie.Valoare == 50m);
            var stocAsm = Stoc(asm);
            Check("Stoc: EXACT 3 rânduri (−6/−30 A, −2/−20 B, +2/+50 kit) pe Marfuri, în gestiunea de lucru",
                stocAsm.Count == 3
                && stocAsm.Single(r => r.LotId == lotA.ID) is { Cantitate: -6m, Valoare: -30m }
                && stocAsm.Single(r => r.LotId == lotB.ID) is { Cantitate: -2m, Valoare: -20m }
                && stocAsm.Single(r => r.LotId == lotKit.ID) is { Cantitate: 2m, Valoare: 50m }
                && stocAsm.All(r => r.TipStoc == TipStoc.Marfuri && r.RepartitorId == gestiune.ID));
            Check("ZERO rânduri contabile pe ASM (stocul se mișcă fără notă — marfă→marfă e zgomot, 23c)",
                !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == asm.ID));
            Check("Lotul kitului finalizat de motor: PretUnitar = valoarea alocată / cantitate (25), data documentului",
                lotKit.PretUnitar == 25m && lotKit.Data == dAsm
                && StocService.Sold(os, new CheieStoc(lotKit.ID, gestiune.ID, TipStoc.Marfuri)) == 2m
                && StocService.Sold(os, new CheieStoc(lotA.ID, gestiune.ID, TipStoc.Marfuri)) == 4m
                && StocService.Sold(os, new CheieStoc(lotB.ID, gestiune.ID, TipStoc.Marfuri)) == 2m);

            // --- Refuzuri (fiecare pe obiecte throwaway, curățate imediat) ---
            void RefuzAsm(string nume, Repartitor predator, Repartitor primitor, Action<Asamblare> linii) {
                var doc = os.CreateObject<Asamblare>();
                doc.Data = new DateOnly(2026, 11, 8);
                doc.Predator = predator;
                doc.Primitor = primitor;
                linii(doc);
                os.CommitChanges();
                CheckRefuza(nume, () => MotorOperare.Opereaza(os, doc));
                Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                    !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == doc.ID)
                    && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == doc.ID));
                var idsLinii = doc.Detalii.Select(d => d.ID).ToList();
                os.Delete(doc.Detalii.ToList());
                os.Delete(os.GetObjectsQuery<Lot>()
                    .Where(l => l.LinieIntrareId != null && idsLinii.Contains(l.LinieIntrareId.Value)).ToList());
                os.Delete(doc);
                os.CommitChanges();
            }

            RefuzAsm("Invariantul valoric picat (2 kituri × 30 = 60 ≠ 50 consumate) → refuz", gestiune, gestiune, doc => {
                LinieAsm(doc, DirectieAsamblare.Consum, 6m, lotA);
                LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotB);
                LinieAsm(doc, DirectieAsamblare.Produs, 2m, pretEvaluare: 30m, produsNou: produsKit);
            });
            RefuzAsm("Consum pe lotul creat de linia proprie → refuz", gestiune, gestiune, doc =>
                LinieAsm(doc, DirectieAsamblare.Consum, 1m, produsNou: produsKit));
            RefuzAsm("Produs pe lot STRĂIN (refolosit, nu născut pe linie) → refuz", gestiune, gestiune, doc => {
                LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotB);
                LinieAsm(doc, DirectieAsamblare.Produs, 2m, lotA, pretEvaluare: 10m);
            });
            RefuzAsm("Direcție nesetată (default-ul enum-ului nu e valid) → refuz", gestiune, gestiune, doc => {
                var d = LinieAsm(doc, DirectieAsamblare.Consum, 1m, lotA);
                d.Directie = default; // linie culeasă fără rol
            });
            RefuzAsm("Linie de produs fără preț de evaluare → refuz", gestiune, gestiune, doc =>
                LinieAsm(doc, DirectieAsamblare.Produs, 2m, produsNou: produsKit));
            RefuzAsm("Consum peste sold (A are 4 buc) → refuzul gardianului de sold", gestiune, gestiune, doc => {
                LinieAsm(doc, DirectieAsamblare.Consum, 100m, lotA);
                LinieAsm(doc, DirectieAsamblare.Produs, 2m, pretEvaluare: 250m, produsNou: produsKit);
            });
            RefuzAsm("Latură care nu e gestiune (predator = furnizor) → refuz", furnizor, gestiune, doc => {
                LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotA);
                LinieAsm(doc, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsKit);
            });
            RefuzAsm("Linie de bază DocumentDetaliu («detaliu generic») pe asamblare → refuz", gestiune, gestiune, doc => {
                var d = os.CreateObject<DocumentDetaliu>(); // NU AsamblareDetaliu
                d.Document = doc;
                d.TipMaterial = tip371;
                d.Lot = lotA;
                d.Cantitate = 1m;
            });

            // --- Dezasamblarea = ACELAȘI tip (1 consum → n produse) ---
            var dDez = new DateOnly(2026, 11, 10);
            var dez = os.CreateObject<Asamblare>();
            dez.Data = dDez; dez.Predator = gestiune; dez.Primitor = gestiune;
            var consumKit = LinieAsm(dez, DirectieAsamblare.Consum, 1m, lotKit);           // −25
            var produsA2 = LinieAsm(dez, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsA);  // +10
            var produsB2 = LinieAsm(dez, DirectieAsamblare.Produs, 1m, pretEvaluare: 15m, produsNou: produsB);  // +15
            var lotA2 = produsA2.Lot;
            var lotB2 = produsB2.Lot;
            os.CommitChanges();
            MotorOperare.Opereaza(os, dez);
            Check("Dezasamblare (1 kit → 2 componente): Σ produse (10+15) = Σ consum (25); loturi noi la prețurile alocate",
                dez.Stare == StareDocument.Operat && consumKit.Valoare == -25m
                && lotA2.PretUnitar == 10m && lotB2.PretUnitar == 15m);
            Check("Stoc după dezasamblare: kit 1 rămas, loturile componente noi cu 1 buc fiecare",
                StocService.Sold(os, new CheieStoc(lotKit.ID, gestiune.ID, TipStoc.Marfuri)) == 1m
                && StocService.Sold(os, new CheieStoc(lotA2.ID, gestiune.ID, TipStoc.Marfuri)) == 1m
                && StocService.Sold(os, new CheieStoc(lotB2.ID, gestiune.ID, TipStoc.Marfuri)) == 1m
                && Stoc(dez).Count == 3);

            // --- Corecție directă → re-operare → storno ---
            var numarDez = dez.Numar;
            MotorOperare.AnuleazaOperarea(os, dez);
            Check("Anulare directă (dezasamblarea e frunză) → Draft + registre goale",
                dez.Stare == StareDocument.Draft
                && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == dez.ID)
                && StocService.Sold(os, new CheieStoc(lotKit.ID, gestiune.ID, TipStoc.Marfuri)) == 2m);
            MotorOperare.Opereaza(os, dez);
            Check("Re-operare → același număr, aceleași 3 rânduri (semnarea e idempotentă)",
                dez.Stare == StareDocument.Operat && dez.Numar == numarDez
                && Stoc(dez).Count == 3 && consumKit.Cantitate == -1m && consumKit.Valoare == -25m);
            var dStorno = new DateOnly(2026, 11, 20);
            MotorOperare.Storneaza(os, dez, dStorno);
            var toateDez = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == dez.ID).ToList();
            Check("Storno → 3 rânduri inverse (Storno=true) la data stornării; soldurile revin (kit 2, componentele noi 0)",
                dez.Stare == StareDocument.Stornat && toateDez.Count == 6
                && toateDez.Count(r => r.Storno && r.Data == dStorno) == 3
                && toateDez.Where(r => r.Storno).Sum(r => r.Cantitate) == -1m
                && StocService.Sold(os, new CheieStoc(lotKit.ID, gestiune.ID, TipStoc.Marfuri), dStorno) == 2m
                && StocService.Sold(os, new CheieStoc(lotA2.ID, gestiune.ID, TipStoc.Marfuri), dStorno) == 0m);

            CurataAsm(os);
            Check("Curățenie finală asamblare (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAsm))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajAsm))
                && !os.GetObjectsQuery<Asamblare>().Any());
        }
    }

    Rezumat();
    return;
}

// ============================ Scenariul e2e 3b ============================
// NotaTransfer end-to-end: sold de deschidere → operare (2 rânduri ±) →
// gardieni (sold intermediar, retroactiv, perioadă, dependență) → FIFO →
// anulare (corecție directă) → storno. Obiectele de test poartă marcajul E2E
// și se curăță la început (run eșuat anterior) și la sfârșit.
const string MarcajProdus = "E2E-PRB";

void Curata(IObjectSpace os) {
    var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajProdus).Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
    var docs = os.GetObjectsQuery<NotaTransfer>().Where(d => d.NumarPV == "E2E").ToList();
    foreach (var doc in docs) {
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
    }
    os.Delete(docs);
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajProdus).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajProdus).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    Curata(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
    var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajProdus;
    produs.Denumire = "Produs probă e2e";
    produs.UM = "BUC";
    produs.TipMaterial = tipMaterial;

    // Sold de deschidere (decizia 12): lot + rând de registru FĂRĂ document sursă.
    var lot = os.CreateObject<Lot>();
    lot.Produs = produs;
    lot.PretUnitar = 10m;
    lot.Gestiune = mag1;
    lot.Data = new DateOnly(2026, 1, 10);
    var deschidere = os.CreateObject<RegistruStoc>();
    deschidere.Data = lot.Data;
    deschidere.TipStoc = TipStoc.Magazie;
    deschidere.Lot = lot;
    deschidere.Repartitor = mag1;
    deschidere.Cantitate = 10m;
    deschidere.Valoare = 100m;
    os.CommitChanges();

    NotaTransfer Transfer(Gestiune dinspre, Gestiune spre, decimal cantitate, DateOnly data) {
        var doc = os.CreateObject<NotaTransfer>();
        doc.Data = data;
        doc.Predator = dinspre;
        doc.Primitor = spre;
        doc.NumarPV = "E2E";
        var d = os.CreateObject<DocumentDetaliu>();
        d.Document = doc;
        d.TipMaterial = tipMaterial;
        d.Lot = lot;
        d.Cantitate = cantitate;
        return doc;
    }
    decimal Sold(Gestiune g) => StocService.Sold(os, new CheieStoc(lot.ID, g.ID, TipStoc.Magazie));
    List<RegistruStoc> RanduriStoc(Document doc) =>
        os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID).ToList();

    // --- Operare: 2 rânduri ±, valoarea din prețul lotului, numerotare ---
    var btr1 = Transfer(mag1, mag2, 4m, new DateOnly(2026, 3, 5));
    MotorOperare.Opereaza(os, btr1);
    var randuri = RanduriStoc(btr1);
    Check("Operare → stare Operat + DataOperare", btr1.Stare == StareDocument.Operat && btr1.DataOperare != null);
    Check("Operare → număr asignat din politică", btr1.Numar?.StartsWith("BTR-") == true);
    Check("Operare → exact 2 rânduri de stoc", randuri.Count == 2);
    Check("Operare → −4/−40 pe MAG1", randuri.Any(r =>
        r.RepartitorId == mag1.ID && r.Cantitate == -4m && r.Valoare == -40m && r.Data == btr1.Data && !r.Storno));
    Check("Operare → +4/+40 pe MAG2", randuri.Any(r =>
        r.RepartitorId == mag2.ID && r.Cantitate == 4m && r.Valoare == 40m && r.Data == btr1.Data && !r.Storno));
    Check("Operare → valoarea liniei = preț lot × cantitate", btr1.Detalii.Single().Valoare == 40m);
    Check("Operare → fără rânduri contabile (23c)",
        !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == btr1.ID));
    Check("Solduri după transfer: MAG1=6, MAG2=4", Sold(mag1) == 6m && Sold(mag2) == 4m);

    CheckRefuza("Re-operarea unui document Operat e refuzată", () => MotorOperare.Opereaza(os, btr1));

    // --- Gardianul de sold: cerere peste disponibil ---
    var insuficient = Transfer(mag2, mag1, 100m, new DateOnly(2026, 3, 10));
    CheckRefuza("Sold insuficient → operare refuzată", () => MotorOperare.Opereaza(os, insuficient));
    os.Delete(insuficient.Detalii.ToList());
    os.Delete(insuficient);

    // --- Gardianul retroactiv: minus inserat în urmă rupe un prefix ulterior ---
    var retro = Transfer(mag1, mag2, 7m, new DateOnly(2026, 2, 1)); // feb: 3, mar: −1
    CheckRefuza("Operare retroactivă care duce soldul sub 0 → refuzată", () => MotorOperare.Opereaza(os, retro));
    os.Delete(retro.Detalii.ToList());
    os.Delete(retro);

    // --- Gardianul de perioadă ---
    var inAfara = Transfer(mag1, mag2, 1m, new DateOnly(2025, 12, 15));
    CheckRefuza("Perioadă nedefinită → refuz", () => MotorOperare.Opereaza(os, inAfara));
    var iunie = os.FirstOrDefault<PerioadaFiscala>(p => p.An == 2026 && p.Luna == 6);
    iunie.Inchisa = true;
    inAfara.Data = new DateOnly(2026, 6, 5);
    CheckRefuza("Perioadă închisă → refuz", () => MotorOperare.Opereaza(os, inAfara));
    iunie.Inchisa = false;
    os.Delete(inAfara.Detalii.ToList());
    os.Delete(inAfara);
    os.CommitChanges();

    // --- FIFO ---
    var alocari = StocService.AlocaFifo(os, produs.ID, mag1.ID, TipStoc.Magazie, new DateOnly(2026, 7, 1), 3m);
    Check("FIFO → o alocare pe lotul disponibil", alocari.Count == 1 && alocari[0].Lot.ID == lot.ID && alocari[0].Cantitate == 3m);
    CheckRefuza("FIFO peste disponibil → refuz", () =>
        StocService.AlocaFifo(os, produs.ID, mag1.ID, TipStoc.Magazie, new DateOnly(2026, 7, 1), 50m));

    // --- Dependența pe loturi: BTR2 consumă din MAG2 ce a adus BTR1 ---
    var btr2 = Transfer(mag2, mag1, 4m, new DateOnly(2026, 4, 1));
    MotorOperare.Opereaza(os, btr2);
    Check("BTR2 operat (MAG2 golit)", Sold(mag2) == 0m && Sold(mag1) == 10m);
    CheckRefuza("Anularea BTR1 cu dependent (BTR2) → refuzată", () => MotorOperare.AnuleazaOperarea(os, btr1));
    CheckRefuza("Stornarea BTR1 cu dependent (BTR2) → refuzată", () =>
        MotorOperare.Storneaza(os, btr1, new DateOnly(2026, 7, 22)));

    // --- Corecția directă: anularea ultimului din lanț e permisă ---
    MotorOperare.AnuleazaOperarea(os, btr2);
    Check("Anulare BTR2 → Draft + rânduri șterse",
        btr2.Stare == StareDocument.Draft && btr2.DataOperare == null && RanduriStoc(btr2).Count == 0);
    Check("Solduri revenite: MAG1=6, MAG2=4", Sold(mag1) == 6m && Sold(mag2) == 4m);
    MotorOperare.Opereaza(os, btr2);
    Check("Re-operare BTR2 după corecție", btr2.Stare == StareDocument.Operat);
    MotorOperare.AnuleazaOperarea(os, btr2);
    os.Delete(btr2.Detalii.ToList());
    os.Delete(btr2);
    os.CommitChanges();

    // --- Storno: rânduri inverse la data stornării, append-only ---
    MotorOperare.Storneaza(os, btr1, new DateOnly(2026, 7, 22));
    var toate = RanduriStoc(btr1);
    Check("Storno → stare Stornat", btr1.Stare == StareDocument.Stornat);
    Check("Storno → 4 rânduri (2 operare + 2 inverse marcate)",
        toate.Count == 4 && toate.Count(r => r.Storno) == 2
        && toate.Where(r => r.Storno).All(r => r.Data == new DateOnly(2026, 7, 22)));
    Check("Storno → solduri nete: MAG1=10, MAG2=0", Sold(mag1) == 10m && Sold(mag2) == 0m);

    Curata(os);
    Check("Curățenie finală (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajProdus));
}

// ========================= Scenariul e2e 3c: FCT → NIR =========================
// Lanțul de cumpărare: factura cu linie de stoc (lot creat la culegere) + linie
// de serviciu → operare (postează DOAR serviciul, finalizează lotul, generează
// NIR conex cu liniile de stoc) → operare NIR (+1 stoc, contează recepția) →
// gardienii grupului conex (anulare/storno pe părinte) → surse de cont
// (TipMaterial / ContImplicit partener, fallback 401/404).
const string MarcajFct = "E2E-FCT-PRB";

void CurataFct(IObjectSpace os) {
    var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajFct).Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
    foreach (var fct in os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Numar.StartsWith("E2E-FF")).ToList()) {
        foreach (var copil in os.GetObjectsQuery<Document>().Where(x => x.DocumentSursaId == fct.ID).ToList()) {
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == copil.ID).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
            os.Delete(copil);
        }
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == fct.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == fct.ID).ToList());
        os.Delete(fct);
    }
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajFct).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajFct).ToList());
    os.Delete(os.GetObjectsQuery<Partener>().Where(p => p.Cod.StartsWith("E2E-FURN")).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-CE").ToList());
    os.Delete(os.GetObjectsQuery<SursaFinantare>().Where(c => c.Cod == "E2E-SF").ToList());
    os.Delete(os.GetObjectsQuery<CodFunctional>().Where(c => c.Cod == "E2E-CF").ToList());
    os.Delete(os.GetObjectsQuery<Proiect>().Where(c => c.Cod == "E2E-PR").ToList());
    os.Delete(os.GetObjectsQuery<Angajament>().Where(c => c.Cod == "E2E-ANG").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataFct(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404.01.00");
    // P1: cota nu se mai culege pe linie — regimul Capitalizat (neplătitor)
    // vine din nomenclatorul TipTva al profilului; 19% e rândul istoric.
    var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

    Check("Seed P1 (bugetar): TipTva Capitalizat ca date, fără conturi de TVA și fără PoliticaTva",
        cap19 != null && cap19.Regim == RegimTva.Capitalizat && cap19.Cota == 19m
        && cap19.ContTvaDeductibilId == null
        && !os.GetObjectsQuery<PoliticaTva>().Any());

    // Maparea Clasă/Tip → cont derivată de seed din simboluri (decizia 4).
    Check("Seed: Tip 302.01.00 → cont 302.01.00 (potrivire exactă)",
        tipMateriale.ContImplicitId != null
        && os.GetObjectByKey<Cont>(tipMateriale.ContImplicitId.Value).Simbol == "302.01.00");
    Check("Seed: Tip 628.00.00 → cont 628.* (tăierea segmentelor)",
        tipServicii.ContImplicitId != null
        && os.GetObjectByKey<Cont>(tipServicii.ContImplicitId.Value).Simbol.StartsWith("628"));

    // Politicile de validare per tip (3d): profilul bugetar cere clasificație
    // pe documentele de angajare/plată; INC rămâne fără rând (venituri —
    // defalcarea E a conturilor de trezorerie acoperă nivelul de cont).
    bool CereClasificatie(string cod) =>
        os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == cod)?.CereClasificatieBugetara == true;
    Check("Seed 3d: FCT/DEC/PLT cer clasificație bugetară; INC nu",
        CereClasificatie("FCT") && CereClasificatie("DEC") && CereClasificatie("PLT") && !CereClasificatie("INC"));

    // P2 (design §7): la bugetar ancora DSC există (nucleu, ca BPR) dar e INERTĂ —
    // fără politici, deci hook-ul GenereazaSecundar nu descarcă nimic; FCL rămâne
    // pur creanță cu natura Stoc interzisă. Datoria P1 (37f): default CAP21.
    Check("Seed bugetar: ancora DSC există și e inertă (0 RegulaStoc, 0 RegulaContare pe DSC)",
        os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC") != null
        && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DSC")
        && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "DSC"));
    var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");
    Check("Seed bugetar: TipTvaImplicit CAP21 pe FCT/FCL/DEC; NIR/DSC null",
        os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT")?.TipTvaImplicitId == cap21.ID
        && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL")?.TipTvaImplicitId == cap21.ID
        && os.FirstOrDefault<TipDocument>(t => t.Cod == "DEC")?.TipTvaImplicitId == cap21.ID
        && os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR")?.TipTvaImplicitId == null
        && os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC")?.TipTvaImplicitId == null);

    var furnizor = os.CreateObject<Partener>();
    furnizor.Cod = "E2E-FURN";
    furnizor.Denumire = "Furnizor probă e2e";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = "E2E-CE";
    codEc.Denumire = "Cod economic probă e2e";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajFct;
    produs.Denumire = "Produs probă FCT";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    os.CommitChanges();

    var fct = os.CreateObject<FacturaIntrare>();
    fct.Data = new DateOnly(2026, 3, 3);
    fct.Predator = furnizor;
    fct.Primitor = mag1;
    var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
    linieStoc.Document = fct;
    linieStoc.TipMaterial = tipMateriale;
    linieStoc.Cantitate = 5m;
    linieStoc.PretUnitar = 10m;
    linieStoc.TipTva = cap19;
    linieStoc.LotFabricatie = "LOT-A";
    linieStoc.DataExpirare = new DateOnly(2027, 1, 1);
    var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
    linieServiciu.Document = fct;
    linieServiciu.TipMaterial = tipServicii;
    linieServiciu.Cantitate = 1m;
    linieServiciu.PretUnitar = 100m;

    // Validările proprii FCT: număr furnizor, clasificație bugetară, lot pe stoc.
    CheckRefuza("FCT fără număr/clasificație/lot → refuz", () => MotorOperare.Opereaza(os, fct));
    fct.Numar = "E2E-FF1";
    linieStoc.Dimensiuni.CodEconomicId = codEc.ID;
    linieServiciu.Dimensiuni.CodEconomicId = codEc.ID;
    var lot = linieStoc.CreeazaLot(os, produs, mag1);
    os.CommitChanges();

    // --- Operare FCT: postează serviciul, finalizează lotul, generează NIR ---
    var conex = MotorOperare.Opereaza(os, fct);
    Check("FCT operată; lanțul de valori materializat (59,5 / 100)",
        fct.Stare == StareDocument.Operat && linieStoc.Valoare == 59.5m && linieServiciu.Valoare == 100m);
    Check("Lot finalizat: preț 11,9 (cu TVA capitalizat) + atribute copiate",
        lot.PretUnitar == 11.9m && lot.Data == fct.Data
        && lot.LotFabricatie == "LOT-A" && lot.DataExpirare == new DateOnly(2027, 1, 1));
    Check("FCT nu mișcă stoc (intrarea o face NIR-ul)",
        !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == fct.ID));
    var noteFct = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == fct.ID).ToList();
    Check("FCT contează DOAR linia de serviciu: 628 = 401, 100",
        noteFct.Count == 1 && noteFct[0].ContDebitId == tipServicii.ContImplicitId
        && noteFct[0].ContCreditId == cont401.ID && noteFct[0].Valoare == 100m);
    Check("Nota FCT: dimensiuni rezolvate (cod economic + repartitori laturi)",
        noteFct[0].DimensiuniDebit.CodEconomicId == codEc.ID
        && noteFct[0].DimensiuniDebit.RepartitorId == furnizor.ID
        && noteFct[0].DimensiuniCredit.RepartitorId == mag1.ID);

    Check("Conex generat: NIR draft autogenerat, aceleași laturi",
        conex is NIR { Stare: StareDocument.Draft, Autogenerat: true }
        && conex.DocumentSursaId == fct.ID
        && conex.PredatorId == furnizor.ID && conex.PrimitorId == mag1.ID);
    Check("NIR-ul preia DOAR linia de stoc, cu lot, cantitate, valoare, dimensiuni",
        conex.Detalii.Count == 1 && conex.Detalii[0].TipMaterialId == tipMateriale.ID
        && conex.Detalii[0].LotId == lot.ID && conex.Detalii[0].Cantitate == 5m
        && conex.Detalii[0].Valoare == 59.5m && conex.Detalii[0].Dimensiuni.CodEconomicId == codEc.ID);

    // --- Operare NIR: singurul +1 al lanțului + contarea recepției ---
    var nir = (NIR)conex;
    Check("NIR-ul nu generează alt conex", MotorOperare.Opereaza(os, nir) == null);
    Check("NIR operat cu număr din politică", nir.Stare == StareDocument.Operat && nir.Numar?.StartsWith("NIR-") == true);
    var stocNir = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == nir.ID).ToList();
    Check("NIR → +5/+59,5 Magazie pe gestiunea primitoare",
        stocNir.Count == 1 && stocNir[0].TipStoc == TipStoc.Magazie && stocNir[0].RepartitorId == mag1.ID
        && stocNir[0].Cantitate == 5m && stocNir[0].Valoare == 59.5m && stocNir[0].LotId == lot.ID);
    var noteNir = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == nir.ID).ToList();
    Check("NIR contează recepția: 302.01.00 = 401, 59,5",
        noteNir.Count == 1 && noteNir[0].ContDebitId == tipMateriale.ContImplicitId
        && noteNir[0].ContCreditId == cont401.ID && noteNir[0].Valoare == 59.5m);
    Check("Nota NIR: Materialul implicit din lot (produsul) pe ambele laturi (3d)",
        noteNir[0].DimensiuniDebit.MaterialId == produs.ID
        && noteNir[0].DimensiuniCredit.MaterialId == produs.ID);
    Check("Sold lot după recepție: 5 pe MAG1",
        StocService.Sold(os, new CheieStoc(lot.ID, mag1.ID, TipStoc.Magazie)) == 5m);

    // --- Grupul conex la anulare/storno ---
    CheckRefuza("Anularea FCT cu NIR operat → refuzată", () => MotorOperare.AnuleazaOperarea(os, fct));
    MotorOperare.AnuleazaOperarea(os, nir);
    Check("NIR anulat (corecție directă): Draft, fără rânduri",
        nir.Stare == StareDocument.Draft && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == nir.ID));
    MotorOperare.AnuleazaOperarea(os, fct);
    Check("Anularea FCT șterge NIR-ul draft autogenerat",
        fct.Stare == StareDocument.Draft && !os.GetObjectsQuery<NIR>().Any(x => x.DocumentSursaId == fct.ID));

    // --- Re-operare + storno pe tot lanțul ---
    var conex2 = MotorOperare.Opereaza(os, fct);
    Check("Re-operarea FCT generează un NIR draft proaspăt", conex2 is NIR { Stare: StareDocument.Draft });
    MotorOperare.Opereaza(os, conex2);
    CheckRefuza("Stornarea FCT cu NIR operat → refuzată", () =>
        MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22)));
    MotorOperare.Storneaza(os, conex2, new DateOnly(2026, 7, 22));
    Check("Storno NIR → sold lot 0",
        StocService.Sold(os, new CheieStoc(lot.ID, mag1.ID, TipStoc.Magazie)) == 0m);
    MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22));
    var noteFinale = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == fct.ID).ToList();
    Check("Storno FCT → nota serviciului inversată, append-only",
        fct.Stare == StareDocument.Stornat && noteFinale.Count == 2
        && noteFinale.Single(r => r.Storno).Valoare == -100m);

    // --- Sursa de cont RepartitorPredator: ContImplicit bate fallback-ul 401 ---
    var furnizorImobilizari = os.CreateObject<Partener>();
    furnizorImobilizari.Cod = "E2E-FURN2";
    furnizorImobilizari.Denumire = "Furnizor cu cont propriu";
    furnizorImobilizari.ContImplicit = cont404;
    var fct2 = os.CreateObject<FacturaIntrare>();
    fct2.Numar = "E2E-FF2";
    fct2.Data = new DateOnly(2026, 3, 4);
    fct2.Predator = furnizorImobilizari;
    fct2.Primitor = mag1;
    var linie2 = os.CreateObject<FacturaIntrareDetaliu>();
    linie2.Document = fct2;
    linie2.TipMaterial = tipServicii;
    linie2.Cantitate = 1m;
    linie2.PretUnitar = 200m;
    // Clasificația prin ANGAJAMENT (nu cod economic): satisface și politica de
    // tip, și — prin puntea din 3d — defalcarea E a contului 404.
    var angajament = os.CreateObject<Angajament>();
    angajament.Cod = "E2E-ANG";
    angajament.Denumire = "Angajament probă e2e";
    linie2.Angajament = angajament;
    os.CommitChanges();

    // 404 poartă defalcarea BFEPR — fără Sursă de finanțare / Cod funcțional /
    // Proiect pe dimensiunile REZOLVATE, operarea se refuză (3d).
    CheckRefuza("404 (BFEPR): Sursă de finanțare/Cod funcțional/Proiect lipsă → refuz",
        () => MotorOperare.Opereaza(os, fct2));
    var sursaFin = os.CreateObject<SursaFinantare>();
    sursaFin.Cod = "E2E-SF";
    sursaFin.Denumire = "Sursă de finanțare probă e2e";
    var codFn = os.CreateObject<CodFunctional>();
    codFn.Cod = "E2E-CF";
    codFn.Denumire = "Cod funcțional probă e2e";
    var proiect = os.CreateObject<Proiect>();
    proiect.Cod = "E2E-PR";
    proiect.Denumire = "Proiect probă e2e";
    os.CommitChanges();
    linie2.Dimensiuni.SursaFinantareId = sursaFin.ID;
    linie2.Dimensiuni.CodFunctionalId = codFn.ID;
    linie2.Dimensiuni.ProiectId = proiect.ID;
    os.CommitChanges();
    Check("FCT doar cu servicii NU generează NIR", MotorOperare.Opereaza(os, fct2) == null);
    var nota404 = os.GetObjectsQuery<RegistruContabil>().Single(r => r.DocumentId == fct2.ID);
    Check("Creditul vine din ContImplicit al partenerului (404, nu fallback 401)",
        nota404.ContCreditId == cont404.ID);
    Check("Puntea angajamentului: E pe 404 satisfăcut fără cod economic; B/F/P rezolvate pe notă",
        nota404.DimensiuniCredit.CodEconomicId == null
        && nota404.DimensiuniCredit.SursaFinantareId == sursaFin.ID
        && nota404.DimensiuniCredit.CodFunctionalId == codFn.ID
        && nota404.DimensiuniCredit.ProiectId == proiect.ID);
    MotorOperare.Storneaza(os, fct2, new DateOnly(2026, 7, 22));

    CurataFct(os);
    Check("Curățenie finală FCT/NIR (fără reziduuri e2e)",
        !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-FF"))
        && !os.GetObjectsQuery<Partener>().Any(p => p.Cod.StartsWith("E2E-FURN")));
}

// ======================== Scenariul e2e 3c: BonConsum ========================
// Consumul: sold de deschidere → operare (−Magazie pe gestiune, +Consum pe
// locul de consum — DOUĂ registre simultan) → contarea 6xx = 3xx din politica
// derivată la seed → gardieni (laturi, sold) → frunză în graful de dependențe:
// anulare directă permisă → storno.
const string MarcajBcs = "E2E-BCS-PRB";
const string MarcajLoc = "E2E-BCS-LOC";

void CurataBcs(IObjectSpace os) {
    var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajBcs).Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
    foreach (var doc in os.GetObjectsQuery<BonConsum>()
        .Where(d => d.Predator.Cod == MarcajLoc || d.Primitor.Cod == MarcajLoc).ToList()) {
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
        os.Delete(doc);
    }
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajBcs).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajBcs).ToList());
    os.Delete(os.GetObjectsQuery<UnitateInterna>().Where(u => u.Cod == MarcajLoc).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataBcs(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipOI = os.FirstOrDefault<TipMaterial>(t => t.Cod == "303.01.00");

    // Politica de contare derivată la seed: 3→6 pe simbol + tăierea segmentelor.
    var regulaMat = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "BCS" && r.TipMaterialId == tipMaterial.ID);
    Check("Seed BCS: 302.01.00 → debit 602.01.00 (potrivire exactă)",
        regulaMat != null && regulaMat.ContDebit?.Simbol == "602.01.00"
        && regulaMat.SursaContCredit == SursaCont.TipMaterial);
    var regulaOI = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "BCS" && r.TipMaterialId == tipOI.ID);
    Check("Seed BCS: 303.01.00 → debit 603 (tăierea segmentelor spre sintetic)",
        regulaOI != null && regulaOI.ContDebit?.Simbol == "603");

    var loc = os.CreateObject<UnitateInterna>();
    loc.Cod = MarcajLoc;
    loc.Denumire = "Loc de consum probă e2e";
    loc.Calitati = CalitateRepartitor.LocConsum;
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajBcs;
    produs.Denumire = "Produs probă BCS";
    produs.UM = "BUC";
    produs.TipMaterial = tipMaterial;
    var lot = os.CreateObject<Lot>();
    lot.Produs = produs;
    lot.PretUnitar = 10m;
    lot.Gestiune = mag1;
    lot.Data = new DateOnly(2026, 1, 10);
    var deschidere = os.CreateObject<RegistruStoc>();
    deschidere.Data = lot.Data;
    deschidere.TipStoc = TipStoc.Magazie;
    deschidere.Lot = lot;
    deschidere.Repartitor = mag1;
    deschidere.Cantitate = 10m;
    deschidere.Valoare = 100m;
    os.CommitChanges();

    BonConsum Consum(Repartitor dinspre, Repartitor spre, decimal cantitate, DateOnly data) {
        var doc = os.CreateObject<BonConsum>();
        doc.Data = data;
        doc.Predator = dinspre;
        doc.Primitor = spre;
        var d = os.CreateObject<DocumentDetaliu>();
        d.Document = doc;
        d.TipMaterial = tipMaterial;
        d.Lot = lot;
        d.Cantitate = cantitate;
        return doc;
    }
    decimal Sold(Repartitor r, TipStoc tipStoc) => StocService.Sold(os, new CheieStoc(lot.ID, r.ID, tipStoc));

    // --- Validările laturilor: predator gestiune, primitor cu calitatea LocConsum ---
    var gresit = Consum(loc, mag1, 1m, new DateOnly(2026, 3, 5));
    CheckRefuza("Laturi greșite (predator non-gestiune, primitor fără LocConsum) → refuz",
        () => MotorOperare.Opereaza(os, gresit));
    os.Delete(gresit.Detalii.ToList());
    os.Delete(gresit);

    // --- Operare: două registre simultan + contarea 602 = 302 ---
    var bcs1 = Consum(mag1, loc, 4m, new DateOnly(2026, 3, 5));
    Check("BCS nu generează conex", MotorOperare.Opereaza(os, bcs1) == null);
    Check("Operare → stare Operat + număr din politică",
        bcs1.Stare == StareDocument.Operat && bcs1.Numar?.StartsWith("BCS-") == true);
    Check("Operare → valoarea liniei = preț lot × cantitate", bcs1.Detalii.Single().Valoare == 40m);
    var randuri = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == bcs1.ID).ToList();
    Check("Operare → exact 2 rânduri de stoc (două registre simultan)", randuri.Count == 2);
    Check("Operare → −4/−40 Magazie pe MAG1", randuri.Any(r =>
        r.TipStoc == TipStoc.Magazie && r.RepartitorId == mag1.ID && r.Cantitate == -4m && r.Valoare == -40m));
    Check("Operare → +4/+40 Consum pe locul de consum", randuri.Any(r =>
        r.TipStoc == TipStoc.Consum && r.RepartitorId == loc.ID && r.Cantitate == 4m && r.Valoare == 40m));
    Check("Solduri: Magazie MAG1=6, Consum loc=4",
        Sold(mag1, TipStoc.Magazie) == 6m && Sold(loc, TipStoc.Consum) == 4m);
    var note = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == bcs1.ID).ToList();
    Check("Contare: 602.01.00 = 302.01.00 (creditul din contul Tipului), 40",
        note.Count == 1 && note[0].ContDebitId == regulaMat.ContDebitId
        && note[0].ContCreditId == tipMaterial.ContImplicitId && note[0].Valoare == 40m);
    Check("Nota BCS: repartitori din laturi (debit←predator, credit←primitor — 00 §5)",
        note[0].DimensiuniDebit.RepartitorId == mag1.ID && note[0].DimensiuniCredit.RepartitorId == loc.ID);

    // --- Gardianul de sold: consum peste disponibil ---
    var pesteDisponibil = Consum(mag1, loc, 100m, new DateOnly(2026, 3, 10));
    CheckRefuza("Consum peste disponibil → refuz", () => MotorOperare.Opereaza(os, pesteDisponibil));
    os.Delete(pesteDisponibil.Detalii.ToList());
    os.Delete(pesteDisponibil);
    os.CommitChanges();

    // --- Frunză în graful de dependențe (03): corecția directă merge oricând ---
    MotorOperare.AnuleazaOperarea(os, bcs1);
    Check("Anulare BCS → Draft + solduri revenite (Magazie 10, Consum 0)",
        bcs1.Stare == StareDocument.Draft
        && Sold(mag1, TipStoc.Magazie) == 10m && Sold(loc, TipStoc.Consum) == 0m);
    MotorOperare.Opereaza(os, bcs1);

    // --- Storno: inverse pe AMBELE registre la data stornării ---
    MotorOperare.Storneaza(os, bcs1, new DateOnly(2026, 7, 22));
    var toate = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == bcs1.ID).ToList();
    Check("Storno BCS → 4 rânduri stoc (2 + 2 inverse) și nota inversată",
        bcs1.Stare == StareDocument.Stornat
        && toate.Count == 4 && toate.Count(r => r.Storno) == 2
        && os.GetObjectsQuery<RegistruContabil>()
            .Single(r => r.DocumentId == bcs1.ID && r.Storno).Valoare == -40m);
    Check("Storno → solduri nete: Magazie 10, Consum 0",
        Sold(mag1, TipStoc.Magazie) == 10m && Sold(loc, TipStoc.Consum) == 0m);

    CurataBcs(os);
    Check("Curățenie finală BCS (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajBcs)
        && !os.GetObjectsQuery<UnitateInterna>().Any(u => u.Cod == MarcajLoc));
}

// ================= Scenariul e2e 3c: ListaDiferenteInventar =================
// Inventarierea: sold de deschidere → LDI cu minus (descarcă lotul existent)
// și plus (creează lot nou cu preț de evaluare) pe ACEEAȘI listă → un singur
// set de reguli de stoc (+1 predator, cantitatea semnată dă direcția) →
// contare pe direcție prin SemnFiltru (minus 6xx = 3xx pozitiv, plus
// 3xx = 791) → gardieni (laturi, direcție, sold) → anulare directă → storno.
const string MarcajLdi = "E2E-LDI-PRB";
const string MarcajComisie = "E2E-LDI-COM";

void CurataLdi(IObjectSpace os) {
    var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajLdi).Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
    foreach (var doc in os.GetObjectsQuery<ListaDiferenteInventar>()
        .Where(d => d.Primitor.Cod == MarcajComisie).ToList()) {
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
        os.Delete(doc);
    }
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajLdi).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajLdi).ToList());
    os.Delete(os.GetObjectsQuery<UnitateInterna>().Where(u => u.Cod == MarcajComisie).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajLdi + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataLdi(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var cont791 = os.FirstOrDefault<Cont>(c => c.Simbol == "791.00.00");

    // Politica derivată la seed: minusul per Tip cu filtru de semn, plusul generic.
    var regulaMinus = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == tipMaterial.ID);
    Check("Seed LDI: minus 302.01.00 → debit 602.01.00, SemnFiltru=-1",
        regulaMinus != null && regulaMinus.ContDebit?.Simbol == "602.01.00"
        && regulaMinus.SemnFiltru == -1 && regulaMinus.SursaContCredit == SursaCont.TipMaterial);
    var regulaPlus = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == null);
    Check("Seed LDI: plus generic → credit 791.00.00, SemnFiltru=+1",
        regulaPlus != null && regulaPlus.SemnFiltru == 1
        && regulaPlus.SursaContDebit == SursaCont.TipMaterial && regulaPlus.ContCreditId == cont791.ID);
    Check("Seed: comisia de inventariere poartă calitatea Comisie",
        os.FirstOrDefault<UnitateInterna>(u => u.Cod == "COMISIE")?.Calitati.HasFlag(CalitateRepartitor.Comisie) == true);

    var comisie = os.CreateObject<UnitateInterna>();
    comisie.Cod = MarcajComisie;
    comisie.Denumire = "Comisie probă e2e";
    comisie.Calitati = CalitateRepartitor.Comisie;
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajLdi + "-CE";
    codEc.Denumire = "Cod economic probă LDI";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajLdi;
    produs.Denumire = "Produs probă LDI";
    produs.UM = "BUC";
    produs.TipMaterial = tipMaterial;
    var lotVechi = os.CreateObject<Lot>();
    lotVechi.Produs = produs;
    lotVechi.PretUnitar = 10m;
    lotVechi.Gestiune = mag1;
    lotVechi.Data = new DateOnly(2026, 1, 10);
    var deschidere = os.CreateObject<RegistruStoc>();
    deschidere.Data = lotVechi.Data;
    deschidere.TipStoc = TipStoc.Magazie;
    deschidere.Lot = lotVechi;
    deschidere.Repartitor = mag1;
    deschidere.Cantitate = 10m;
    deschidere.Valoare = 100m;
    os.CommitChanges();

    decimal Sold(Lot lot) => StocService.Sold(os, new CheieStoc(lot.ID, mag1.ID, TipStoc.Magazie));

    // --- LDI bidirecțional: minus pe lotul existent + plus cu lot nou ---
    var ldi = os.CreateObject<ListaDiferenteInventar>();
    ldi.Data = new DateOnly(2026, 3, 5);
    ldi.Predator = mag1;
    ldi.Primitor = comisie;
    var linieMinus = os.CreateObject<ListaDiferenteInventarDetaliu>();
    linieMinus.Document = ldi;
    linieMinus.TipMaterial = tipMaterial;
    linieMinus.Directie = DirectieDiferenta.Minus;
    linieMinus.Lot = lotVechi;
    linieMinus.Cantitate = 2m; // UI-ul culege pozitiv; semnul îl pune operarea
    var liniePlus = os.CreateObject<ListaDiferenteInventarDetaliu>();
    liniePlus.Document = ldi;
    liniePlus.TipMaterial = tipMaterial;
    liniePlus.Directie = DirectieDiferenta.Plus;
    liniePlus.Cantitate = 3m;
    liniePlus.LotFabricatie = "LOT-P";

    // Validările proprii: lot pe plus + preț de evaluare + laturile.
    CheckRefuza("Plus fără lot creat / fără preț de evaluare → refuz", () => MotorOperare.Opereaza(os, ldi));
    var lotNou = liniePlus.CreeazaLot(os, produs, mag1);
    liniePlus.PretEvaluare = 7m;
    ldi.Primitor = mag1; // gestiune fără calitatea Comisie
    CheckRefuza("Primitor fără calitatea Comisie → refuz", () => MotorOperare.Opereaza(os, ldi));
    ldi.Primitor = comisie;
    os.CommitChanges();

    // Venitul plusului (791) poartă defalcarea E — cerută pe nota rezolvată (3d);
    // minusul (602 = 302, ambele S) nu cere nimic.
    CheckRefuza("Plus fără cod economic (791 cere E) → refuz", () => MotorOperare.Opereaza(os, ldi));
    liniePlus.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();

    // --- Operare: direcția materializată în semn, două rânduri ± pe predator ---
    Check("LDI nu generează conex", MotorOperare.Opereaza(os, ldi) == null);
    Check("Operare → stare Operat + număr din politică",
        ldi.Stare == StareDocument.Operat && ldi.Numar?.StartsWith("LDI-") == true);
    Check("Minus: direcția materializată în semn (−2 / −20)",
        linieMinus.Cantitate == -2m && linieMinus.Valoare == -20m);
    Check("Plus: cantitate pozitivă, valoarea din prețul de evaluare (+3 / +21)",
        liniePlus.Cantitate == 3m && liniePlus.Valoare == 21m);
    Check("Lot nou finalizat: preț 7, data documentului, atribute copiate",
        lotNou.PretUnitar == 7m && lotNou.Data == ldi.Data && lotNou.LotFabricatie == "LOT-P");
    var randuri = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == ldi.ID).ToList();
    Check("Operare → 2 rânduri de stoc, ambele Magazie pe gestiunea inventariată",
        randuri.Count == 2 && randuri.All(r => r.TipStoc == TipStoc.Magazie && r.RepartitorId == mag1.ID));
    Check("Minus → −2/−20 pe lotul vechi", randuri.Any(r =>
        r.LotId == lotVechi.ID && r.Cantitate == -2m && r.Valoare == -20m));
    Check("Plus → +3/+21 pe lotul nou", randuri.Any(r =>
        r.LotId == lotNou.ID && r.Cantitate == 3m && r.Valoare == 21m));
    Check("Solduri: lot vechi 8, lot nou 3", Sold(lotVechi) == 8m && Sold(lotNou) == 3m);
    var note = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == ldi.ID).ToList();
    Check("Contare minus: 602.01.00 = 302.01.00, POZITIVĂ (normalizată cu semnul filtrului)",
        note.Any(n => n.ContDebitId == regulaMinus.ContDebitId
            && n.ContCreditId == tipMaterial.ContImplicitId && n.Valoare == 20m));
    Check("Contare plus: 302.01.00 = 791.00.00, 21",
        note.Any(n => n.ContDebitId == tipMaterial.ContImplicitId
            && n.ContCreditId == cont791.ID && n.Valoare == 21m));
    Check("Exact 2 note (una per direcție)", note.Count == 2);

    // --- Gardianul de sold: minus peste disponibil ---
    var pesteDisponibil = os.CreateObject<ListaDiferenteInventar>();
    pesteDisponibil.Data = new DateOnly(2026, 3, 10);
    pesteDisponibil.Predator = mag1;
    pesteDisponibil.Primitor = comisie;
    var lipsaMare = os.CreateObject<ListaDiferenteInventarDetaliu>();
    lipsaMare.Document = pesteDisponibil;
    lipsaMare.TipMaterial = tipMaterial;
    lipsaMare.Directie = DirectieDiferenta.Minus;
    lipsaMare.Lot = lotVechi;
    lipsaMare.Cantitate = 100m;
    CheckRefuza("Minus peste disponibil → refuz", () => MotorOperare.Opereaza(os, pesteDisponibil));
    os.Delete(pesteDisponibil.Detalii.ToList());
    os.Delete(pesteDisponibil);
    os.CommitChanges();

    // --- Anulare directă (lotul nou neatins de alții) + re-operare ---
    MotorOperare.AnuleazaOperarea(os, ldi);
    Check("Anulare LDI → Draft + solduri revenite (vechi 10, nou 0)",
        ldi.Stare == StareDocument.Draft && Sold(lotVechi) == 10m && Sold(lotNou) == 0m);
    MotorOperare.Opereaza(os, ldi);
    Check("Re-operare după corecție (semnul rămâne idempotent)",
        ldi.Stare == StareDocument.Operat && linieMinus.Cantitate == -2m && liniePlus.Cantitate == 3m);

    // --- Storno: inverse pe ambele direcții și pe note ---
    MotorOperare.Storneaza(os, ldi, new DateOnly(2026, 7, 22));
    var toate = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == ldi.ID).ToList();
    Check("Storno LDI → 4 rânduri stoc (2 + 2 inverse) și notele inversate (−20, −21)",
        ldi.Stare == StareDocument.Stornat && toate.Count == 4 && toate.Count(r => r.Storno) == 2
        && os.GetObjectsQuery<RegistruContabil>().Count(r => r.DocumentId == ldi.ID && r.Storno
            && (r.Valoare == -20m || r.Valoare == -21m)) == 2);
    Check("Storno → solduri nete: vechi 10, nou 0", Sold(lotVechi) == 10m && Sold(lotNou) == 0m);

    CurataLdi(os);
    Check("Curățenie finală LDI (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajLdi)
        && !os.GetObjectsQuery<UnitateInterna>().Any(u => u.Cod == MarcajComisie));
}

// ===================== Scenariul e2e 3c: FacturaIesire =====================
// Facturarea: pur creanță (411 = 7xx), fără registru de stoc → numerotare
// proprie din politică (serie fiscală) → scadență default +30 din politică
// (fără să suprascrie scadența culeasă) → contare per linie cu creditul din
// contul de venit al Tipului și debitul particularizat prin ContImplicit al
// clientului (461) → validări laturi + refuz linii de stoc → anulare directă
// → storno.
const string MarcajFcl = "E2E-FCL";

void CurataFcl(IObjectSpace os) {
    foreach (var doc in os.GetObjectsQuery<FacturaIesire>()
        .Where(d => d.Primitor.Cod.StartsWith(MarcajFcl) || d.Predator.Cod.StartsWith(MarcajFcl)).ToList()) {
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).ToList());
        os.Delete(doc);
    }
    os.Delete(os.GetObjectsQuery<Partener>().Where(p => p.Cod.StartsWith(MarcajFcl)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajFcl + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataFcl(os);

    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var tipServiciiVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "751.01.00");
    var tipChirii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "750.02.00");
    var tipStocMat = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
    var cont461 = os.FirstOrDefault<Cont>(c => c.Simbol == "461.01.01");
    var cap19Fcl = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

    // Politica derivată la seed: tipurile de venit cu contul din simbol.
    Check("Seed: Tip 751.01.00 (clasa VEN) → cont 751.01.00",
        tipServiciiVenit?.ContImplicitId != null
        && os.GetObjectByKey<Cont>(tipServiciiVenit.ContImplicitId.Value).Simbol == "751.01.00");
    var regulaFcl = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "FCL");
    Check("Seed FCL: debit RepartitorPrimitor (fallback 411.01.01), credit TipMaterial",
        regulaFcl != null && regulaFcl.SursaContDebit == SursaCont.RepartitorPrimitor
        && regulaFcl.ContDebitId == cont411.ID && regulaFcl.SursaContCredit == SursaCont.TipMaterial);
    Check("Seed FCL: fără reguli de stoc (pur creanță)",
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "FCL"));
    var politicaScadenta = os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocument.Cod == "FCL");
    Check("Seed FCL: politică de scadență +30", politicaScadenta?.ZileDefault == 30);
    Check("Seed 3d: FCL interzice natura Stoc (PoliticaValidare, fostul hardcode 30a)",
        os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL")?.NaturaInterzisa == NaturaClasa.Stoc);

    var client = os.CreateObject<Partener>();
    client.Cod = MarcajFcl + "-CL1";
    client.Denumire = "Client probă e2e";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajFcl + "-CE";
    codEc.Denumire = "Clasificație de venit probă e2e";
    os.CommitChanges();

    FacturaIesireDetaliu Linie(FacturaIesire doc, TipMaterial tip, decimal cantitate, decimal pret, TipTva tipTva = null) {
        var d = os.CreateObject<FacturaIesireDetaliu>();
        d.Document = doc;
        d.TipMaterial = tip;
        d.Cantitate = cantitate;
        d.PretUnitar = pret;
        d.TipTva = tipTva;
        return d;
    }
    List<RegistruContabil> Note(Document doc) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();

    // --- Validările laturilor + refuzul liniilor de stoc ---
    var fcl = os.CreateObject<FacturaIesire>();
    fcl.Data = new DateOnly(2026, 3, 5);
    fcl.Predator = client; // inversat intenționat
    fcl.Primitor = sediu;
    Linie(fcl, tipServiciiVenit, 2m, 100m).Descriere = "Servicii refacturate";
    CheckRefuza("Laturi inversate (predator partener / primitor intern) → refuz",
        () => MotorOperare.Opereaza(os, fcl));
    fcl.Predator = sediu;
    fcl.Primitor = client;

    var linieStoc = Linie(fcl, tipStocMat, 1m, 5m);
    CheckRefuza("Linie de stoc pe factura de ieșire → refuz (nu descarcă gestiune)",
        () => MotorOperare.Opereaza(os, fcl));
    os.Delete(linieStoc);
    Linie(fcl, tipChirii, 1m, 50m).Descriere = "Chirie spațiu";
    os.CommitChanges();

    // Conturile de venit (751/750) poartă defalcarea E — clasificația de venit
    // e cerută la nivel de CONT (3d), nu de tip (FCL nu are rând de politică).
    CheckRefuza("Venituri fără cod economic (751/750 cer E) → refuz",
        () => MotorOperare.Opereaza(os, fcl));
    foreach (var d in fcl.Detalii)
        d.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();

    // --- Operare: serie fiscală + scadență default + o notă per linie ---
    Check("FCL nu generează conex", MotorOperare.Opereaza(os, fcl) == null);
    Check("Operare → stare Operat + număr din seria fiscală",
        fcl.Stare == StareDocument.Operat && fcl.Numar?.StartsWith("FCL-") == true);
    Check("Scadența default din politică: data + 30",
        fcl.DataScadenta == fcl.Data.AddDays(30));
    Check("FCL nu mișcă stoc",
        !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == fcl.ID));
    var note = Note(fcl);
    Check("Contare servicii: 411.01.01 = 751.01.00, 200",
        note.Any(n => n.ContDebitId == cont411.ID
            && n.ContCreditId == tipServiciiVenit.ContImplicitId && n.Valoare == 200m));
    Check("Contare chirie: 411.01.01 = 750.02.00, 50",
        note.Any(n => n.ContDebitId == cont411.ID
            && n.ContCreditId == tipChirii.ContImplicitId && n.Valoare == 50m));
    Check("Exact 2 note (una per linie)", note.Count == 2);
    Check("Nota FCL: repartitori din laturi (debit←emitent, credit←client — 00 §5)",
        note.All(n => n.DimensiuniDebit.RepartitorId == sediu.ID
            && n.DimensiuniCredit.RepartitorId == client.ID));

    // --- Debit particularizat (461) + scadență culeasă + TVA în valoare ---
    var clientDebitor = os.CreateObject<Partener>();
    clientDebitor.Cod = MarcajFcl + "-CL2";
    clientDebitor.Denumire = "Debitor cu cont propriu";
    clientDebitor.ContImplicit = cont461;
    var fcl2 = os.CreateObject<FacturaIesire>();
    fcl2.Data = new DateOnly(2026, 3, 6);
    fcl2.Predator = sediu;
    fcl2.Primitor = clientDebitor;
    fcl2.DataScadenta = new DateOnly(2026, 12, 31); // culeasă manual
    Linie(fcl2, tipServiciiVenit, 1m, 100m, cap19Fcl).Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    MotorOperare.Opereaza(os, fcl2);
    Check("Debitul din ContImplicit al clientului (461, nu fallback 411)",
        Note(fcl2).Single().ContDebitId == cont461.ID);
    Check("Valoarea postată include TVA (o singură Valoare pe linie: 119)",
        Note(fcl2).Single().Valoare == 119m);
    Check("Scadența culeasă manual nu se suprascrie",
        fcl2.DataScadenta == new DateOnly(2026, 12, 31));
    MotorOperare.Storneaza(os, fcl2, new DateOnly(2026, 7, 22));

    // --- Fără stoc = fără dependenți: anulare directă mereu permisă → storno ---
    MotorOperare.AnuleazaOperarea(os, fcl);
    Check("Anulare FCL → Draft + notele șterse",
        fcl.Stare == StareDocument.Draft && Note(fcl).Count == 0);
    MotorOperare.Opereaza(os, fcl);
    Check("Re-operare după corecție (numărul asignat rămâne)",
        fcl.Stare == StareDocument.Operat && fcl.Numar?.StartsWith("FCL-") == true);
    MotorOperare.Storneaza(os, fcl, new DateOnly(2026, 7, 22));
    var noteFinale = Note(fcl);
    Check("Storno FCL → note inverse append-only (−200, −50) la data stornării",
        fcl.Stare == StareDocument.Stornat && noteFinale.Count == 4
        && noteFinale.Count(r => r.Storno && (r.Valoare == -200m || r.Valoare == -50m)
            && r.Data == new DateOnly(2026, 7, 22)) == 2);

    CurataFcl(os);
    Check("Curățenie finală FCL (fără reziduuri e2e)",
        !os.GetObjectsQuery<Partener>().Any(p => p.Cod.StartsWith(MarcajFcl)));
}

// ============== Scenariul e2e 3c: Plata/Incasare + Imperechere ==============
// Trezoreria (decizia 31): FCT cu grupul DECONT_* → draft Plata autogenerat cu
// liniile-defalcare → operare (contare per latură din ContImplicit pe
// Repartitor: 401 = 770) + imperecherea automată → gardianul de imperecheri la
// anulare/storno → încasare manuală (411 fallback → casă) + imperechere
// manuală cu invarianții (stare, contrapartidă, rest) → avans către angajat
// (542 din ContImplicit) → storno după ștergerea stingerii.
const string MarcajTrz = "E2E-TRZ";

void CurataTrz(IObjectSpace os) {
    var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajTrz)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentTrezorerieId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
        os.Delete(doc);
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajTrz).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajTrz).ToList());
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajTrz)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajTrz + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataTrz(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "751.01.00");
    var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
    var trezoreria = os.FirstOrDefault<ContPropriu>(c => c.Cod == "TREZ");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
    var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
    var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
    var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");

    // Politicile trezoreriei din seed (decizia 31).
    Check("Seed: conturi proprii CASA→531.01.01, TREZ→770.00.00 (bancă)",
        casa?.ContImplicitId == cont531.ID && trezoreria?.ContImplicitId == cont770.ID && trezoreria.EsteBanca);
    Check("Seed: Tipul tehnic TRZ (defalcare) fără cont implicit",
        tipTrz != null && tipTrz.ContImplicitId == null && tipTrz.Clasa.Natura == NaturaClasa.Tehnica);
    var regulaPlt = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "PLT");
    Check("Seed PLT: debit RepartitorPrimitor (fallback 401), credit RepartitorPredator fără fallback",
        regulaPlt != null && regulaPlt.SursaContDebit == SursaCont.RepartitorPrimitor
        && regulaPlt.ContDebitId == cont401.ID
        && regulaPlt.SursaContCredit == SursaCont.RepartitorPredator && regulaPlt.ContCreditId == null);
    var regulaInc = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "INC");
    Check("Seed INC: debit RepartitorPrimitor fără fallback, credit RepartitorPredator (fallback 411)",
        regulaInc != null && regulaInc.SursaContDebit == SursaCont.RepartitorPrimitor
        && regulaInc.ContDebitId == null && regulaInc.ContCreditId == cont411.ID);
    Check("Seed: fără reguli de stoc pe PLT/INC (pur contabile)",
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "PLT" || r.TipDocument.Cod == "INC"));

    var furnizor = os.CreateObject<Partener>();
    furnizor.Cod = MarcajTrz + "-FURN";
    furnizor.Denumire = "Furnizor probă trezorerie";
    var client = os.CreateObject<Partener>();
    client.Cod = MarcajTrz + "-CL";
    client.Denumire = "Client probă trezorerie";
    var angajat = os.CreateObject<Angajat>();
    angajat.Cod = MarcajTrz + "-ANG";
    angajat.Denumire = "Angajat probă trezorerie";
    angajat.ContImplicit = cont542; // avansurile de trezorerie ale titularului
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajTrz + "-CE";
    codEc.Denumire = "Cod economic probă trezorerie";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajTrz;
    produs.Denumire = "Produs probă trezorerie";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    os.CommitChanges();

    List<RegistruContabil> Note(Document doc) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

    // --- FCT cu plata automată (00 §7): DECONT_* → draft Plata + defalcare ---
    var fct = os.CreateObject<FacturaIntrare>();
    fct.Numar = "E2E-TF1";
    fct.Data = new DateOnly(2026, 3, 3);
    fct.Predator = furnizor;
    fct.Primitor = mag1;
    fct.GenereazaPlata = true;
    var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
    linieStoc.Document = fct;
    linieStoc.TipMaterial = tipMateriale;
    linieStoc.Cantitate = 5m;
    linieStoc.PretUnitar = 10m;
    linieStoc.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
    linieStoc.Dimensiuni.CodEconomicId = codEc.ID;
    linieStoc.CreeazaLot(os, produs, mag1);
    var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
    linieServiciu.Document = fct;
    linieServiciu.TipMaterial = tipServicii;
    linieServiciu.Cantitate = 1m;
    linieServiciu.PretUnitar = 100m;
    linieServiciu.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();

    CheckRefuza("GenereazaPlata fără cont propriu cules → refuz", () => MotorOperare.Opereaza(os, fct));
    fct.PlataContPropriu = trezoreria;
    fct.PlataNumar = "OP-77";
    fct.PlataData = new DateOnly(2026, 3, 4);
    fct.PlataTipInstrument = TipInstrumentPlata.Cec;
    os.CommitChanges();

    var conex = MotorOperare.Opereaza(os, fct);
    Check("Operarea FCT întoarce conexul NIR; plata e al doilea copil autogenerat", conex is NIR);
    var plataAuto = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fct.ID);
    Check("Plata draft: header din grupul DECONT_* (TREZ → furnizor, OP-77, CEC, data plății)",
        plataAuto.Stare == StareDocument.Draft && plataAuto.Autogenerat
        && plataAuto.PredatorId == trezoreria.ID && plataAuto.PrimitorId == furnizor.ID
        && plataAuto.Numar == "OP-77" && plataAuto.TipInstrument == TipInstrumentPlata.Cec
        && plataAuto.Data == new DateOnly(2026, 3, 4));
    Check("Plata draft: liniile clonează defalcarea facturii (2 linii, 159,5, dimensiuni, fără lot)",
        plataAuto.Detalii.Count == 2 && plataAuto.Detalii.Sum(d => d.Valoare) == 159.5m
        && plataAuto.Detalii.All(d => d.Dimensiuni.CodEconomicId == codEc.ID && d.LotId == null));

    // --- Operarea plății: contare din laturi + imperecherea automată ---
    Check("Plata autogenerată nu generează alt conex", MotorOperare.Opereaza(os, plataAuto) == null);
    var notePlata = Note(plataAuto);
    Check("Plata contează per linie de defalcare: 401 = 770 (59,5 + 100)",
        notePlata.Count == 2 && notePlata.All(n => n.ContDebitId == cont401.ID && n.ContCreditId == cont770.ID)
        && notePlata.Sum(n => n.Valoare) == 159.5m);
    Check("Plata nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == plataAuto.ID));
    var impAuto = os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentTrezorerieId == plataAuto.ID);
    Check("Imperecherea automată: FCT stinsă integral (159,5, Autogenerat)",
        impAuto.DocumentId == fct.ID && impAuto.Suma == 159.5m && impAuto.Autogenerat
        && ImperechereService.Ramas(os, fct.ID) == 0m && ImperechereService.Ramas(os, plataAuto.ID) == 0m);

    // --- Gardianul de imperecheri: corecția cere întâi ștergerea stingerii ---
    CheckRefuza("Anularea plății cu imperechere → refuz", () => MotorOperare.AnuleazaOperarea(os, plataAuto));
    CheckRefuza("Stornarea FCT cu plata operată → refuz", () =>
        MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22)));
    os.Delete(impAuto);
    os.CommitChanges();
    MotorOperare.AnuleazaOperarea(os, plataAuto);
    Check("După ștergerea imperecherii, anularea plății merge (Draft, notele șterse)",
        plataAuto.Stare == StareDocument.Draft && Note(plataAuto).Count == 0);
    MotorOperare.Opereaza(os, plataAuto);
    Check("Re-operarea plății re-creează imperecherea automată",
        os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentTrezorerieId == plataAuto.ID).Suma == 159.5m);

    // --- Încasare manuală + imperechere manuală cu FCL (invarianții stingerii) ---
    var fcl = os.CreateObject<FacturaIesire>();
    fcl.Data = new DateOnly(2026, 3, 6);
    fcl.Predator = sediu;
    fcl.Primitor = client;
    var linieVenit = os.CreateObject<FacturaIesireDetaliu>();
    linieVenit.Document = fcl;
    linieVenit.TipMaterial = tipVenit;
    linieVenit.Cantitate = 1m;
    linieVenit.PretUnitar = 119m;
    linieVenit.Dimensiuni.CodEconomicId = codEc.ID; // 751 cere E (3d)
    os.CommitChanges();
    MotorOperare.Opereaza(os, fcl);

    var inc = os.CreateObject<Incasare>();
    inc.Data = new DateOnly(2026, 3, 10);
    inc.Predator = casa; // intenționat greșit — plătitorul nu poate fi cont propriu
    inc.Primitor = casa;
    inc.TipInstrument = TipInstrumentPlata.Chitanta;
    var linieInc = os.CreateObject<DocumentDetaliu>();
    linieInc.Document = inc;
    linieInc.TipMaterial = tipTrz;
    CheckRefuza("Laturi greșite + linie fără valoare → refuz", () => MotorOperare.Opereaza(os, inc));
    inc.Predator = client;
    linieInc.Valoare = 119m;
    os.CommitChanges();
    // Casa (531) poartă defalcarea E — INC nu are politică de tip, dar contul
    // cere codul economic pe nota rezolvată (3d).
    CheckRefuza("Încasare fără cod economic (531 cere E) → refuz", () => MotorOperare.Opereaza(os, inc));
    linieInc.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    Check("Încasarea nu generează conex", MotorOperare.Opereaza(os, inc) == null);
    Check("Încasare operată cu număr din politică", inc.Numar?.StartsWith("INC-") == true);
    var noteInc = Note(inc);
    Check("Contare încasare: 531.01.01 (casa) = 411.01.01 (fallback client), 119",
        noteInc.Count == 1 && noteInc[0].ContDebitId == cont531.ID
        && noteInc[0].ContCreditId == cont411.ID && noteInc[0].Valoare == 119m);

    var fclDraft = os.CreateObject<FacturaIesire>();
    fclDraft.Data = new DateOnly(2026, 3, 11);
    fclDraft.Predator = sediu;
    fclDraft.Primitor = client;
    CheckRefuza("Imperechere cu document neoperat → refuz", () =>
        ImperechereService.Imperecheaza(os, inc, fclDraft, 1m));
    CheckRefuza("Imperechere fără contrapartidă comună (încasarea clientului × factura furnizorului) → refuz",
        () => ImperechereService.Imperecheaza(os, inc, fct, 1m));
    CheckRefuza("Imperechere peste restul neasignat → refuz", () =>
        ImperechereService.Imperecheaza(os, inc, fcl, 200m));
    var impManual = ImperechereService.Imperecheaza(os, inc, fcl, 119m);
    Check("Imperechere manuală: FCL stinsă integral, resturile 0",
        !impManual.Autogenerat && ImperechereService.Ramas(os, fcl.ID) == 0m
        && ImperechereService.Ramas(os, inc.ID) == 0m);
    CheckRefuza("A doua stingere pe aceeași încasare (rest 0) → refuz", () =>
        ImperechereService.Imperecheaza(os, inc, fcl, 1m));

    // --- Avansul către angajat: 542 din ContImplicit bate fallback-ul 401 ---
    var avans = os.CreateObject<Plata>();
    avans.Data = new DateOnly(2026, 3, 12);
    avans.Predator = casa;
    avans.Primitor = angajat;
    avans.TipInstrument = TipInstrumentPlata.DispozitieCasa;
    var linieAvans = os.CreateObject<DocumentDetaliu>();
    linieAvans.Document = avans;
    linieAvans.TipMaterial = tipTrz;
    linieAvans.Valoare = 50m;
    os.CommitChanges();
    // 31f închis: obligativitatea clasificației pe liniile de plată = politică.
    CheckRefuza("Plată fără clasificație bugetară (politica PLT) → refuz",
        () => MotorOperare.Opereaza(os, avans));
    linieAvans.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    MotorOperare.Opereaza(os, avans);
    var noteAvans = Note(avans);
    Check("Avans operat cu număr din politică", avans.Numar?.StartsWith("PLT-") == true);
    Check("Contare avans: 542.01.00 (ContImplicit angajat) = 531.01.01 (casa), 50",
        noteAvans.Count == 1 && noteAvans[0].ContDebitId == cont542.ID
        && noteAvans[0].ContCreditId == cont531.ID && noteAvans[0].Valoare == 50m);
    Check("Nota avansului: repartitori din laturi (debit←casă, credit←angajat — 00 §5)",
        noteAvans[0].DimensiuniDebit.RepartitorId == casa.ID
        && noteAvans[0].DimensiuniCredit.RepartitorId == angajat.ID);

    // --- Storno: refuzat cât există stingerea, curat după ștergerea ei ---
    CheckRefuza("Stornarea încasării cu imperechere → refuz", () =>
        MotorOperare.Storneaza(os, inc, new DateOnly(2026, 7, 22)));
    os.Delete(impManual);
    os.CommitChanges();
    MotorOperare.Storneaza(os, inc, new DateOnly(2026, 7, 22));
    var toateNoteInc = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == inc.ID).ToList();
    Check("Storno încasare → nota inversată append-only (−119) la data stornării",
        inc.Stare == StareDocument.Stornat && toateNoteInc.Count == 2
        && toateNoteInc.Single(r => r.Storno).Valoare == -119m);

    CurataTrz(os);
    Check("Curățenie finală trezorerie (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajTrz))
        && !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajTrz));
}

// ===================== Scenariul e2e 3c: Decont =====================
// Justificarea avansurilor (inventar 06): avans (Plata către angajat) → decont
// cu debit din contul Tipului + postare explicită pe linie (cont 623 +
// repartitor de cost) → creditul 542 dimensionat pe TITULAR (default polimorf,
// nu convenția credit←Primitor) → imperecherea lanțului avans↔decont↔
// regularizare → Tip fără cont și fără explicit = refuz clar → fallback 542
// la angajatul fără ContImplicit → gardianul de imperecheri → anulare → storno.
const string MarcajDec = "E2E-DEC";

void CurataDec(IObjectSpace os) {
    var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDec)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentTrezorerieId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    os.Delete(docs);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajDec)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajDec + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataDec(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
    var tipDeplasari = os.FirstOrDefault<TipMaterial>(t => t.Cod == "614.00.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
    var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");

    // Politicile din seed (inventar 06).
    var regulaDec = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "DEC");
    Check("Seed DEC: debit TipMaterial fără fallback, credit RepartitorPredator (fallback 542.01.00)",
        regulaDec != null && regulaDec.SursaContDebit == SursaCont.TipMaterial && regulaDec.ContDebitId == null
        && regulaDec.SursaContCredit == SursaCont.RepartitorPredator && regulaDec.ContCreditId == cont542.ID);
    Check("Seed DEC: fără reguli de stoc (doar registru contabil)",
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DEC"));

    var angajat = os.CreateObject<Angajat>();
    angajat.Cod = MarcajDec + "-ANG";
    angajat.Denumire = "Titular probă decont";
    angajat.ContImplicit = cont542;
    var angajat2 = os.CreateObject<Angajat>();
    angajat2.Cod = MarcajDec + "-ANG2";
    angajat2.Denumire = "Titular fără cont implicit";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajDec + "-CE";
    codEc.Denumire = "Cod economic probă decont";
    os.CommitChanges();

    List<RegistruContabil> Note(Document doc) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

    // --- Avansul: Plata casa → angajat (542 = 531, din 3c-5) ---
    var avans = os.CreateObject<Plata>();
    avans.Data = new DateOnly(2026, 3, 3);
    avans.Predator = casa;
    avans.Primitor = angajat;
    avans.TipInstrument = TipInstrumentPlata.DispozitieCasa;
    var linieAvans = os.CreateObject<DocumentDetaliu>();
    linieAvans.Document = avans;
    linieAvans.TipMaterial = tipTrz;
    linieAvans.Valoare = 100m;
    linieAvans.Dimensiuni.CodEconomicId = codEc.ID; // politica PLT + defalcarea E (531/542)
    os.CommitChanges();
    MotorOperare.Opereaza(os, avans);
    Check("Avansul operat (100, casa → angajat)", avans.Stare == StareDocument.Operat);

    // --- Decontul: validările proprii, apoi operarea ---
    var dec = os.CreateObject<Decont>();
    dec.Data = new DateOnly(2026, 3, 8);
    dec.Predator = sediu; // intenționat greșit — titularul e angajat
    dec.Primitor = angajat;
    var linieDeplasare = os.CreateObject<DecontDetaliu>();
    linieDeplasare.Document = dec;
    linieDeplasare.TipMaterial = tipDeplasari;
    linieDeplasare.Descriere = "Transport delegație";
    CheckRefuza("Laturi greșite + valoare 0 + fără clasificație → refuz",
        () => MotorOperare.Opereaza(os, dec));
    dec.Predator = angajat;
    dec.Primitor = sediu;
    linieDeplasare.PretUnitar = 30m; // cantitatea rămâne 0 — pro-forma → 1
    linieDeplasare.Dimensiuni.CodEconomicId = codEc.ID;
    // Postarea explicită pe linie (trăsătura DEC): cont + repartitor de cost.
    var linieProtocol = os.CreateObject<DecontDetaliu>();
    linieProtocol.Document = dec;
    linieProtocol.TipMaterial = tipServicii;
    linieProtocol.Descriere = "Protocol contractare";
    linieProtocol.ContDebit = cont623;
    linieProtocol.RepartitorDebit = mag1;
    linieProtocol.PretUnitar = 10m;
    linieProtocol.Cantitate = 2m;
    linieProtocol.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
    linieProtocol.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();

    Check("Decontul nu generează conex", MotorOperare.Opereaza(os, dec) == null);
    Check("Operare → stare Operat + număr din politică",
        dec.Stare == StareDocument.Operat && dec.Numar?.StartsWith("DEC-") == true);
    Check("Cantitatea pro-forma (0 → 1) și lanțul de valori (30 / 23,8)",
        linieDeplasare.Cantitate == 1m && linieDeplasare.Valoare == 30m
        && linieProtocol.Valoare == 23.8m);
    Check("Decontul nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == dec.ID));
    var note = Note(dec);
    Check("Contare deplasare: debit din contul Tipului (614) = 542, 30",
        note.Any(n => n.ContDebitId == tipDeplasari.ContImplicitId
            && n.ContCreditId == cont542.ID && n.Valoare == 30m));
    Check("Contare protocol: debitul EXPLICIT al liniei (623 bate Tipul 628) = 542, 23,8",
        note.Any(n => n.ContDebitId == cont623.ID && n.ContCreditId == cont542.ID && n.Valoare == 23.8m));
    Check("Exact 2 note (una per linie)", note.Count == 2);
    var notaDeplasare = note.Single(n => n.Valoare == 30m);
    var notaProtocol = note.Single(n => n.Valoare == 23.8m);
    Check("Dimensiuni debit: default←titular la deplasare, repartitorul EXPLICIT (MAG1) la protocol",
        notaDeplasare.DimensiuniDebit.RepartitorId == angajat.ID
        && notaDeplasare.DimensiuniDebit.CodEconomicId == codEc.ID
        && notaProtocol.DimensiuniDebit.RepartitorId == mag1.ID);
    Check("Dimensiuni credit: 542 pe TITULAR (default polimorf, nu primitorul SEDIU)",
        note.All(n => n.DimensiuniCredit.RepartitorId == angajat.ID));

    // --- Tip fără cont și fără postare explicită = refuz clar; fallback 542 ---
    var dec2 = os.CreateObject<Decont>();
    dec2.Data = new DateOnly(2026, 3, 9);
    dec2.Predator = angajat2; // fără ContImplicit — exersează fallback-ul regulii
    dec2.Primitor = sediu;
    var linieTehnica = os.CreateObject<DecontDetaliu>();
    linieTehnica.Document = dec2;
    linieTehnica.TipMaterial = tipTrz; // TRZ nu are ContImplicit
    linieTehnica.PretUnitar = 20m;
    linieTehnica.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    CheckRefuza("Tip fără cont implicit și linie fără cont explicit → refuz",
        () => MotorOperare.Opereaza(os, dec2));
    linieTehnica.ContDebit = cont623;
    os.CommitChanges();
    MotorOperare.Opereaza(os, dec2);
    Check("Angajat fără ContImplicit → creditul cade pe fallback-ul 542.01.00",
        Note(dec2).Single().ContCreditId == cont542.ID
        && Note(dec2).Single().DimensiuniCredit.RepartitorId == angajat2.ID);

    // --- Lanțul avans ↔ decont ↔ regularizare prin imperechere (31d) ---
    var impDecont = ImperechereService.Imperecheaza(os, avans, dec, 53.8m);
    Check("Imperechere avans↔decont: decontul stins integral, avansul cu rest 46,2",
        ImperechereService.Ramas(os, dec.ID) == 0m && ImperechereService.Ramas(os, avans.ID) == 46.2m);
    var regularizare = os.CreateObject<Incasare>();
    regularizare.Data = new DateOnly(2026, 3, 15);
    regularizare.Predator = angajat;
    regularizare.Primitor = casa;
    regularizare.TipInstrument = TipInstrumentPlata.DispozitieCasa;
    var linieReg = os.CreateObject<DocumentDetaliu>();
    linieReg.Document = regularizare;
    linieReg.TipMaterial = tipTrz;
    linieReg.Valoare = 46.2m;
    linieReg.Dimensiuni.CodEconomicId = codEc.ID; // casa (531) cere E
    os.CommitChanges();
    MotorOperare.Opereaza(os, regularizare);
    ImperechereService.Imperecheaza(os, regularizare, avans, 46.2m);
    Check("Regularizarea stinge restul: avansul asignat pe AMBELE roluri, rest 0",
        ImperechereService.Ramas(os, avans.ID) == 0m && ImperechereService.Ramas(os, regularizare.ID) == 0m);

    // --- Gardianul de imperecheri + corecția directă + storno ---
    CheckRefuza("Anularea decontului cu imperechere → refuz", () => MotorOperare.AnuleazaOperarea(os, dec));
    os.Delete(impDecont);
    os.CommitChanges();
    MotorOperare.AnuleazaOperarea(os, dec);
    Check("Anulare decont → Draft + notele șterse", dec.Stare == StareDocument.Draft && Note(dec).Count == 0);
    MotorOperare.Opereaza(os, dec);
    Check("Re-operare idempotentă (cantitatea pro-forma rămâne 1, numărul rămâne)",
        dec.Stare == StareDocument.Operat && linieDeplasare.Cantitate == 1m
        && dec.Numar?.StartsWith("DEC-") == true);
    MotorOperare.Storneaza(os, dec, new DateOnly(2026, 7, 22));
    var toateNoteDec = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == dec.ID).ToList();
    Check("Storno decont → note inverse append-only (−30, −23,8) la data stornării",
        dec.Stare == StareDocument.Stornat && toateNoteDec.Count == 4
        && toateNoteDec.Count(r => r.Storno && (r.Valoare == -30m || r.Valoare == -23.8m)
            && r.Data == new DateOnly(2026, 7, 22)) == 2);

    CurataDec(os);
    Check("Curățenie finală decont (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajDec)));
}

// ===================== Scenariul e2e 1C-a: NotaContabila =====================
// Nota contabilă (design FAZA 1C §5): tipul FĂRĂ nicio regulă de contare —
// postarea explicită COMPLETĂ a liniei bate ABSENȚA regulii (mecanismul 32a
// extins în motor). Acoperă: valoarea CA ATARE (inclusiv negativă — nota storno
// de import), repartitorul explicit vs. default-ul polimorf, laturile interne,
// gardianul generic de dimensiuni obligatorii per cont (628 cere E), anularea
// directă, re-operarea și storno-ul.
const string MarcajNtc = "E2E-NTC";

void CurataNtc(IObjectSpace os) {
    var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtc)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    os.Delete(docs);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtc)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajNtc + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataNtc(os);

    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    // Conturi CPLAN: 581.x și 623 fără defalcare obligatorie; 628 cere E.
    var cont581a = os.FirstOrDefault<Cont>(c => c.Simbol == "581.01.01");
    var cont581b = os.FirstOrDefault<Cont>(c => c.Simbol == "581.01.02");
    var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");
    var cont628 = os.FirstOrDefault<Cont>(c => c.Simbol == "628.00.00");

    var tipNtc = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");
    Check("Seed NTC: ancora TipDocument + numerotare NTC-, FĂRĂ reguli de stoc/contare și fără politici de TVA/scadență/validare",
        tipNtc != null && tipNtc.ClrType == nameof(NotaContabila)
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC")?.Serie == "NTC-"
        && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipNtc.ID)
        && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipNtc.ID)
        && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipNtc.ID) == null
        && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipNtc.ID) == null
        && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipNtc.ID) == null);
    Check("Precondiție de plan: 581.01.01/581.01.02/623.00.00 fără defalcare, 628.00.00 cere cod economic",
        cont581a.DimensiuniObligatorii == DimensiuneFlags.Niciuna
        && cont581b.DimensiuniObligatorii == DimensiuneFlags.Niciuna
        && cont623.DimensiuniObligatorii == DimensiuneFlags.Niciuna
        && cont628.DimensiuniObligatorii.HasFlag(DimensiuneFlags.CodEconomic));

    // Laturile notei = repartitori INTERNI (convenția tipului); partenerul de
    // probă există doar pentru refuzul de latură.
    var unitate = os.CreateObject<UnitateInterna>();
    unitate.Cod = MarcajNtc + "-UI";
    unitate.Denumire = "Unitate probă notă contabilă";
    var partener = os.CreateObject<Partener>();
    partener.Cod = MarcajNtc + "-PART";
    partener.Denumire = "Partener probă notă contabilă";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajNtc + "-CE";
    codEc.Denumire = "Cod economic probă notă contabilă";
    os.CommitChanges();

    List<RegistruContabil> Note(Document doc) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();

    // --- Nota: două linii, a doua NEGATIVĂ (storno de notă din import) ---
    var ntc = os.CreateObject<NotaContabila>();
    ntc.Data = new DateOnly(2026, 4, 6);
    ntc.Predator = sediu;
    ntc.Primitor = unitate;
    var linieViramente = os.CreateObject<NotaContabilaDetaliu>();
    linieViramente.Document = ntc;
    linieViramente.TipMaterial = tipTrz;
    linieViramente.Descriere = "Virament intern";
    linieViramente.ContDebit = cont581a;
    linieViramente.ContCredit = cont581b;
    linieViramente.Valoare = 100m;
    linieViramente.RepartitorDebit = mag1; // postare explicită și pe repartitor
    var linieStorno = os.CreateObject<NotaContabilaDetaliu>();
    linieStorno.Document = ntc;
    linieStorno.TipMaterial = tipTrz;
    linieStorno.Descriere = "Corecție cu minus (notă storno)";
    linieStorno.ContDebit = cont623;
    linieStorno.ContCredit = cont581a;
    linieStorno.Valoare = -40m;
    os.CommitChanges();

    Check("Nota contabilă nu generează conex/secundar", MotorOperare.Opereaza(os, ntc) == null);
    Check("Operare → stare Operat + număr din politică (NTC-)",
        ntc.Stare == StareDocument.Operat && ntc.Numar?.StartsWith("NTC-") == true);
    Check("Nota nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == ntc.ID));
    var noteNtc = Note(ntc);
    Check("Exact 2 rânduri contabile (una per linie), fără regulă de contare", noteNtc.Count == 2);
    var randViramente = noteNtc.SingleOrDefault(n => n.DetaliuId == linieViramente.ID);
    var randStorno = noteNtc.SingleOrDefault(n => n.DetaliuId == linieStorno.ID);
    Check("Conturile = cele EXPLICITE ale liniilor (581.01.01 = 581.01.02; 623 = 581.01.01)",
        randViramente?.ContDebitId == cont581a.ID && randViramente.ContCreditId == cont581b.ID
        && randStorno?.ContDebitId == cont623.ID && randStorno.ContCreditId == cont581a.ID);
    Check("Valorile se postează CA ATARE, inclusiv negativa (100 / −40), fără flag de storno",
        randViramente.Valoare == 100m && randStorno.Valoare == -40m
        && !randViramente.Storno && !randStorno.Storno);
    Check("Dimensiuni debit: repartitorul EXPLICIT (MAG1) pe prima linie, default polimorf (predator SEDIU) pe a doua",
        randViramente.DimensiuniDebit.RepartitorId == mag1.ID
        && randStorno.DimensiuniDebit.RepartitorId == sediu.ID);
    Check("Dimensiuni credit: default polimorf (primitor) pe ambele linii",
        noteNtc.All(n => n.DimensiuniCredit.RepartitorId == unitate.ID));

    // --- Refuzurile: invarianții tipului + gardianul generic de dimensiuni ---
    void RefuzNtc(string nume, Repartitor predator, Repartitor primitor, Action<NotaContabilaDetaliu> configurare) {
        var doc = os.CreateObject<NotaContabila>();
        doc.Data = new DateOnly(2026, 4, 7);
        doc.Predator = predator;
        doc.Primitor = primitor;
        var linie = os.CreateObject<NotaContabilaDetaliu>();
        linie.Document = doc;
        linie.TipMaterial = tipTrz;
        linie.ContDebit = cont581a;
        linie.ContCredit = cont581b;
        linie.Valoare = 10m;
        configurare(linie);
        os.CommitChanges();
        CheckRefuza(nume, () => MotorOperare.Opereaza(os, doc));
        Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
            !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == doc.ID));
        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    RefuzNtc("Linie fără cont creditor → refuz", sediu, unitate, l => l.ContCredit = null);
    RefuzNtc("Linie cu valoare 0 → refuz", sediu, unitate, l => l.Valoare = 0m);
    RefuzNtc("Latură cu Partener (nota are laturi interne) → refuz", sediu, partener, _ => { });
    RefuzNtc("Cont cu defalcare obligatorie (628 cere E) fără cod economic → refuzul gardianului generic",
        sediu, unitate, l => l.ContDebit = cont628);

    // Linia de BAZĂ ar fi sărită mut de motor (n-are postare explicită) — refuz
    // explicit, ca pe DSC/FCL (38c).
    var ntcBaza = os.CreateObject<NotaContabila>();
    ntcBaza.Data = new DateOnly(2026, 4, 7);
    ntcBaza.Predator = sediu;
    ntcBaza.Primitor = unitate;
    var linieBaza = os.CreateObject<DocumentDetaliu>(); // NU NotaContabilaDetaliu
    linieBaza.Document = ntcBaza;
    linieBaza.TipMaterial = tipTrz;
    linieBaza.Valoare = 10m;
    os.CommitChanges();
    CheckRefuza("Linie de bază DocumentDetaliu («detaliu generic») pe notă → refuz",
        () => MotorOperare.Opereaza(os, ntcBaza));
    os.Delete(ntcBaza.Detalii.ToList());
    os.Delete(ntcBaza);
    os.CommitChanges();

    // Același cont 628, cu codul economic cules pe linie → gardianul e satisfăcut.
    var ntcDefalcare = os.CreateObject<NotaContabila>();
    ntcDefalcare.Data = new DateOnly(2026, 4, 8);
    ntcDefalcare.Predator = sediu;
    ntcDefalcare.Primitor = unitate;
    var linieDefalcare = os.CreateObject<NotaContabilaDetaliu>();
    linieDefalcare.Document = ntcDefalcare;
    linieDefalcare.TipMaterial = tipTrz;
    linieDefalcare.ContDebit = cont628;
    linieDefalcare.ContCredit = cont581a;
    linieDefalcare.Valoare = 55m;
    linieDefalcare.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    MotorOperare.Opereaza(os, ntcDefalcare);
    Check("628 cu cod economic cules pe linie → operare acceptată, dimensiunea pe latura contului",
        ntcDefalcare.Stare == StareDocument.Operat
        && Note(ntcDefalcare).Single().DimensiuniDebit.CodEconomicId == codEc.ID);

    // --- Corecție directă → re-operare → storno ---
    var numarNtc = ntc.Numar;
    MotorOperare.AnuleazaOperarea(os, ntc);
    Check("Anulare directă → Draft + registrele goale",
        ntc.Stare == StareDocument.Draft
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == ntc.ID));
    MotorOperare.Opereaza(os, ntc);
    Check("Re-operare → același număr, aceleași 2 rânduri (100 / −40)",
        ntc.Stare == StareDocument.Operat && ntc.Numar == numarNtc
        && Note(ntc).Count == 2 && Note(ntc).Sum(n => n.Valoare) == 60m);
    MotorOperare.Storneaza(os, ntc, new DateOnly(2026, 7, 22));
    var toateNoteNtc = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == ntc.ID).ToList();
    Check("Storno → rânduri inverse append-only la data stornării (−100 / +40, nota negativă devine pozitivă)",
        ntc.Stare == StareDocument.Stornat && toateNoteNtc.Count == 4
        && toateNoteNtc.Count(r => r.Storno && r.Data == new DateOnly(2026, 7, 22)
            && (r.Valoare == -100m || r.Valoare == 40m)) == 2);

    CurataNtc(os);
    Check("Curățenie finală notă contabilă (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNtc)));
}

// ============ Scenariul e2e 1C-a: InchidereTva la BUGETAR (tip inert) ============
// Dovada agnosticismului: ancora ITV există în nucleu pentru ambele profiluri, dar
// conturile închiderii sunt DATE de profil (PoliticaInchidereTva) — fără rând,
// generatorul întoarce null și tipul rămâne inert, ca DSC/BPR (decizia 29).
using (var os = provider.CreateObjectSpace()) {
    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var tipItv = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
    Check("Seed bugetar: ancora TipDocument ITV există (nucleu), FĂRĂ politică de închidere și fără numerotare",
        tipItv != null && tipItv.ClrType == nameof(InchidereTva)
        && os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocumentId == tipItv.ID) == null
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipItv.ID) == null);
    Check("Generatorul la bugetar → null (tip inert: conturile închiderii nu sunt hardcodate în motor)",
        InchidereTvaService.Genereaza(os, 2026, 9, sediu.ID) == null);
    Check("Bugetar: niciun document ITV creat de apelul de mai sus",
        !os.GetObjectsQuery<InchidereTva>().Any());
}

// ============== Scenariul e2e 1C-a: Asamblare la BUGETAR (tip inert) ==============
// Ancora ASM trăiește în nucleu (ambele profiluri), dar politicile sunt DATE de
// profil: la bugetar nu există reguli de stoc/contare și nici numerotare, deci
// tipul e inert — ca DSC/ITV/BPR (decizia 29).
using (var os = provider.CreateObjectSpace()) {
    var tipAsm = os.FirstOrDefault<TipDocument>(t => t.Cod == "ASM");
    Check("Seed bugetar: ancora TipDocument ASM există (nucleu), cu ClrType-ul clasei",
        tipAsm != null && tipAsm.ClrType == nameof(Asamblare));
    Check("Bugetar: ASM e tip INERT — fără reguli de stoc/contare, fără numerotare și fără politici de TVA/scadență/validare",
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipAsm.ID)
        && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipAsm.ID)
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipAsm.ID) == null
        && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipAsm.ID) == null
        && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipAsm.ID) == null
        && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipAsm.ID) == null);
}

Rezumat();
