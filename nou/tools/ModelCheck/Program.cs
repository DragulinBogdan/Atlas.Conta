using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;

// Validare model EF + (dacă baza există) verificare migrații/seed + scenariile
// end-to-end ale motorului de operare pe un IObjectSpace real — aceeași
// infrastructură XAF pe care o folosește și UI-ul (docs 113709).
//
// Parametrizat pe PROFIL (P1, design §7): implicit rulează suita bugetară pe
// baza aplicației (aceeași țintă ca appsettings.json); `ModelCheck privat`
// rulează blocul e2e privat pe o bază DEDICATĂ (profil-per-bază — 35d), pe
// care unealta o migrează și o seed-uiește singură (ContaSeeder, Privat).
// D10 — emitorul de metadata pentru clientul React. Rulează pe REFLECȚIE pură
// (fără bază, fără XafApplication) și iese imediat: `ModelCheck --dump-metadata
// [cale]`. Fără argument scrie la calea implicită documentată în MetadataDump.
{
    var indexDump = Array.FindIndex(args, a => a.Equals("--dump-metadata", StringComparison.OrdinalIgnoreCase));
    if (indexDump >= 0) {
        var caleDump = args.Length > indexDump + 1 && !args[indexDump + 1].StartsWith('-')
            ? Path.GetFullPath(args[indexDump + 1])
            : MetadataDump.CaleImplicita();
        MetadataDump.Scrie(caleDump);
        Console.WriteLine($"Metadata scrisă: {caleDump}");
        return;
    }
}

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

// D10 — disciplina migrațiilor aplicată codegen-ului (43d): canonic e artefactul
// COMIS, unealta doar verifică. Dacă `metadata.json` există și nu mai corespunde
// modelului (caption adăugat, enum extins, DefaultProperty mutat), rularea
// normală PICĂ — clientul nu are voie să se compileze pe captions fantomă.
{
    var caleMetadata = MetadataDump.CaleImplicita();
    var (exista, identic) = MetadataDump.VerificaDrift(caleMetadata);
    if (!exista)
        Console.WriteLine($"Metadata client: absentă ({caleMetadata}) — sări verificarea de drift.");
    else
        Check("Metadata clientului e la zi (altfel: rulați ModelCheck --dump-metadata)", identic);
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
            Rezumat();
            return;
        }
        var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
        var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
        Console.WriteLine($"Migrații aplicate: {applied.Count}; în așteptare: {pending.Count}"
            + (pending.Count > 0 ? $" ({string.Join(", ", pending)})" : ""));
        if (pending.Count > 0) {
            Console.WriteLine("Aplicați migrațiile înainte de scenariul e2e (dotnet ef database update).");
            Rezumat();
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

// Convenția de rotunjire a banilor = dată a bazei (decizia 51c): pe calea privată
// seed-ul tocmai a fixat-o, pe cea bugetară (bază deja seed-uită) se citește aici.
using (var osConventie = provider.CreateObjectSpace()) {
    var citita = ContaSeeder.AplicaConventiaRotunjire(osConventie);
    Console.WriteLine($"Convenție rotunjire bani: {Scara.ConventieBani}"
        + (citita ? " (din SetareProfil)" : " (implicit — baza nu are rând SetareProfil)"));
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

    // DIM-3: garda mapării PLATE — [Column] trebuie să conserve schema fostului
    // owned (round-trip insert/reread/update pe FK-ul plat al regulii de contare;
    // o nepotrivire de nume de coloană ar pica aici, nu în producție).
    var tipDoc = await ctx.TipuriDocument.FirstAsync();
    var repartitor = await ctx.Repartitori.FirstAsync();
    var proba = ctx.CreateProxy<RegulaContare>(); // XAF creează entitățile proxy — proba la fel
    proba.TipDocumentId = tipDoc.ID;
    proba.ComunRepartitorId = repartitor.ID;
    ctx.ReguliContare.Add(proba);
    await ctx.SaveChangesAsync();
    ctx.ChangeTracker.Clear();

    var recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
    Check("Coloana plată (DimensiuniComun_RepartitorId) → FK persistat și recitit", recitita.ComunRepartitorId == repartitor.ID);
    Check("Value object construit din coloanele plate", recitita.DimensiuniComun().RepartitorId == repartitor.ID);

    recitita.ComunRepartitorId = null;
    await ctx.SaveChangesAsync();
    ctx.ChangeTracker.Clear();
    recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
    Check("Update pe coloana plată → schimbare detectată", recitita.ComunRepartitorId == null);

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
            .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
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

        // G3 (decizia 50f): Tipurile create ad-hoc de importul 1C sunt acum seed
        // EXPLICIT — cod, clasă și cont implicit din profil, nu ghicite din prima
        // cifră a simbolului. Upsert pe Cod ⇒ bazele importate se corectează.
        (string Clasa, string[] Coduri)[] promovate = [
            ("TER", ["408", "4091", "4092", "419", "447", "473", "5328", "S371"]),
            ("C", ["6021", "6022", "6028", "604", "6422", "6458", "6581", "6651", "667"]),
            ("S", ["6051", "6052", "6053", "6231", "6232", "624", "627"]),
            ("VEN", ["767", "7581", "7588"]),
        ];
        var tipuriPromovate = promovate
            .SelectMany(g => g.Coduri.Select(cod => (g.Clasa, Cod: cod)))
            .Select(x => (x.Clasa, x.Cod, Tip: os.FirstOrDefault<TipMaterial>(t => t.Cod == x.Cod))).ToList();
        Check($"Seed G3: toate cele {tipuriPromovate.Count} Tipuri promovate există, în clasa declarată "
            + "(TER=terți/regularizări, C=cheltuieli, S=servicii, VEN=venituri)",
            tipuriPromovate.Count == 27
            && tipuriPromovate.All(x => x.Tip != null && x.Tip.Clasa?.Cod == x.Clasa));
        Check("Seed G3: clasa nouă TER e de natură Serviciu (paritate cu clasificarea ad-hoc — "
            + "regulile de contare se potrivesc pe natură)",
            os.FirstOrDefault<ClasaProdus>(c => c.Cod == "TER")?.Natura == NaturaClasa.Serviciu);
        Check("Seed G3: fiecare Tip promovat are cont implicit — cele 26 numerice pe simbolul lor, "
            + "puntea S371 explicit pe 371 (Cod-ul ei nu e simbol de cont)",
            tipuriPromovate.All(x => x.Tip?.ContImplicitId != null
                && os.GetObjectByKey<Cont>(x.Tip.ContImplicitId.Value).Simbol == (x.Cod == "S371" ? "371" : x.Cod)));
        Check("Seed G3: denumirile vin din planul OMFP, nu din import („1C: cont X”)",
            tipuriPromovate.All(x => x.Tip?.Denumire != null && !x.Tip.Denumire.StartsWith("1C:")));

        // Decizia 51c: convenția de rotunjire e dată de profil, un rând per bază.
        var setareProfil = os.GetObjectsQuery<SetareProfil>().ToList();
        Check("Seed 51c: un singur rând SetareProfil, pe profilul bazei, cu convenția de rotunjire "
            + "aplicată în Scara (Flax rămâne AwayFromZero)",
            setareProfil.Count == 1 && setareProfil[0].Profil == ProfilContabil.Privat
            && setareProfil[0].RotunjireBani == MidpointRounding.AwayFromZero
            && Scara.ConventieBani == MidpointRounding.AwayFromZero);

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
                && n.DimensiuniDebit().RepartitorId == furnizor.ID && n.DimensiuniCredit().RepartitorId == mag1.ID));

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
            os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID).Suma == 181.5m
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
            && noteDec.All(n => n.DimensiuniCredit().RepartitorId == angajat.ID));

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

        // --- Aceeași regulă pe FCL și DEC (36a uniformizat — decizia 48b) ---
        // Recalculul din cotă ar da 21,00; documentul real poartă 20,99, iar
        // rândul de TVA trebuie să posteze EXACT valoarea culeasă.
        var fclManual = os.CreateObject<FacturaIesire>();
        fclManual.Data = new DateOnly(2026, 3, 12);
        fclManual.Predator = sediu;
        fclManual.Primitor = client;
        var linieFclManual = os.CreateObject<FacturaIesireDetaliu>();
        linieFclManual.Document = fclManual;
        linieFclManual.TipMaterial = tip704;
        linieFclManual.Cantitate = 1m;
        linieFclManual.PretUnitar = 100m;
        linieFclManual.TipTva = n21;
        linieFclManual.ValoareTva = 20.99m; // TVA-ul de pe factura emisă (rotunjirea ei)
        os.CommitChanges();
        MotorOperare.Opereaza(os, fclManual);
        Check("FCL: ValoareTva culeasă (20,99) nu se suprascrie; 4111 = 4427 postează exact 20,99",
            linieFclManual.Valoare == 100m && linieFclManual.ValoareTva == 20.99m
            && Note(fclManual).Any(n => n.ContDebitId == cont4111.ID && n.ContCreditId == cont4427.ID
                && n.Valoare == 20.99m)
            && fclManual.Total == 120.99m);

        var decManual = os.CreateObject<Decont>();
        decManual.Data = new DateOnly(2026, 3, 13);
        decManual.Predator = angajat;
        decManual.Primitor = sediu;
        var linieDecManual = os.CreateObject<DecontDetaliu>();
        linieDecManual.Document = decManual;
        linieDecManual.TipMaterial = tip628;
        linieDecManual.PretUnitar = 100m;
        linieDecManual.TipTva = n21;
        linieDecManual.ValoareTva = 20.99m; // TVA-ul de pe bonul justificat
        os.CommitChanges();
        MotorOperare.Opereaza(os, decManual);
        Check("DEC: ValoareTva culeasă (20,99) nu se suprascrie; 4426 = 542 postează exact 20,99",
            linieDecManual.Valoare == 100m && linieDecManual.ValoareTva == 20.99m
            && Note(decManual).Any(n => n.ContDebitId == cont4426.ID && n.ContCreditId == cont542.ID
                && n.Valoare == 20.99m));

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
            .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
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
            noteDsc.All(n => n.DimensiuniDebit().RepartitorId == mag1.ID && n.DimensiuniCredit().RepartitorId == mag1.ID)
            && noteDsc.Where(n => n.ContDebitId == cont607.ID).All(n => n.DimensiuniDebit().MaterialId == produsA.ID)
            && notaPf.DimensiuniDebit().MaterialId == produsC.ID);
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
                notePrv.All(n => n.DimensiuniDebit().RepartitorId == sediu.ID
                    && n.DimensiuniCredit().RepartitorId == unitate.ID));
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

    // ============ Scenariul e2e: compensarea (NTC pe rolul de stingător) ============
    // Decizia 48b: Compensarea din 1C (869/an) e o notă contabilă operată care
    // STINGE — 401 = 4111 pe același partener stinge simultan datoria și creanța.
    // Acoperă: rolul de stingător declarat polimorf (facturile NU pot stinge),
    // plafonul PER CONTRAPARTIDĂ (nota dublă stinge de două ori), invariantul de
    // contrapartidă reformulat (repartitorii expliciți ai liniilor), refuzul pe
    // notă needitată/nepotrivită și gardianul de anulare cât există stingeri.
    {
        const string MarcajCmp = "E2E-CMP";

        void CurataCmp(IObjectSpace os) {
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajCmp)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            // Nota are laturi INTERNE (partenerul stă pe linii), deci nu e prinsă
            // de filtrul pe laturi — se adaugă prin numărul propriu.
            docs.AddRange(os.GetObjectsQuery<NotaContabila>().Where(d => d.Numar.StartsWith(MarcajCmp)));
            var docIds = docs.Select(d => d.ID).Distinct().ToList();
            os.Delete(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            os.Delete(docs);
            os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajCmp)).ToList());
            os.CommitChanges();
        }

        using (var os = provider.CreateObjectSpace()) {
            CurataCmp(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
            var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");

            Check("Seed privat: partenerul generic de retail CF (surogatul RVA — decizia 48b), fără ContImplicit propriu",
                os.FirstOrDefault<Partener>(p => p.Cod == "CF") is { Denumire: "CONSUMATOR FINAL", ContImplicitId: null });
            TipTva Tva(string cod) => os.FirstOrDefault<TipTva>(t => t.Cod == cod);
            Check("Seed privat: cotele istorice N19/TI19 (importul 1C aduce un an dinaintea Legii 141/2025), cu conturile 4426/4427 și SAF-T null",
                Tva("N19") is { Cota: 19m, Regim: RegimTva.Normal, CodSafTLivrare: null, CodSafTAchizitie: null }
                && Tva("N19").ContTvaDeductibilId != null && Tva("N19").ContTvaColectatId != null
                && Tva("TI19") is { Cota: 19m, Regim: RegimTva.TaxareInversa, CodSafTLivrare: null }
                && Tva("TI19").ContTvaDeductibilId != null);

            // Partenerul X e furnizor pe o factură și client pe alta — cazul real
            // al compensării; Y e martorul care nu apare pe notă.
            var partenerX = os.CreateObject<Partener>();
            partenerX.Cod = MarcajCmp + "-X";
            partenerX.Denumire = "Partener compensare X";
            var partenerY = os.CreateObject<Partener>();
            partenerY.Cod = MarcajCmp + "-Y";
            partenerY.Denumire = "Partener compensare Y";
            os.CommitChanges();

            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = MarcajCmp + "-FF";
            fct.Data = new DateOnly(2026, 5, 4);
            fct.Predator = partenerX;
            fct.Primitor = mag1;
            var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
            linieFct.Document = fct;
            linieFct.TipMaterial = tip628;
            linieFct.Cantitate = 1m;
            linieFct.PretUnitar = 100m;

            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = new DateOnly(2026, 5, 5);
            fcl.Predator = sediu;
            fcl.Primitor = partenerX;
            var linieFcl = os.CreateObject<FacturaIesireDetaliu>();
            linieFcl.Document = fcl;
            linieFcl.TipMaterial = tip704;
            linieFcl.Cantitate = 1m;
            linieFcl.PretUnitar = 100m;

            var fclY = os.CreateObject<FacturaIesire>();
            fclY.Data = new DateOnly(2026, 5, 5);
            fclY.Predator = sediu;
            fclY.Primitor = partenerY;
            var linieFclY = os.CreateObject<FacturaIesireDetaliu>();
            linieFclY.Document = fclY;
            linieFclY.TipMaterial = tip704;
            linieFclY.Cantitate = 1m;
            linieFclY.PretUnitar = 100m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            MotorOperare.Opereaza(os, fcl);
            MotorOperare.Opereaza(os, fclY);

            // Nota de compensare: laturi interne (partenerul stă pe LINIE, nu pe
            // latură — validarea NTC o cere), 401 = 4111 pe X, valoare parțială.
            NotaContabila NotaCompensare(decimal valoare, Repartitor repartitor) {
                var n = os.CreateObject<NotaContabila>();
                n.Numar = MarcajCmp + "-C" + valoare;
                n.Data = new DateOnly(2026, 5, 6);
                n.Predator = sediu;
                n.Primitor = sediu;
                var linie = os.CreateObject<NotaContabilaDetaliu>();
                linie.Document = n;
                linie.TipMaterial = tipTrz;
                linie.Descriere = "Compensare " + repartitor.Cod;
                linie.ContDebit = cont401;
                linie.ContCredit = cont4111;
                linie.RepartitorDebit = repartitor;
                linie.RepartitorCredit = repartitor;
                linie.Valoare = valoare;
                os.CommitChanges();
                return n;
            }

            var ntcDraft = NotaCompensare(60m, partenerX);
            CheckRefuza("Nota NEOPERATĂ nu stinge (invariantul „ambele operate” neatins)",
                () => ImperechereService.Imperecheaza(os, ntcDraft, fct, 10m));
            CheckRefuza("Factura NU e stingător — rolul e declarat de tip (CapacitateStingere), nu de FK",
                () => ImperechereService.Imperecheaza(os, fcl, fct, 10m));

            MotorOperare.Opereaza(os, ntcDraft);
            var ntc = ntcDraft;
            Check("Nota de compensare operată: 401 = 4111 pe X (60), fără stoc și fără TVA",
                ntc.Stare == StareDocument.Operat
                && os.GetObjectsQuery<RegistruContabil>().Count(r => r.DocumentId == ntc.ID && !r.Storno
                    && r.ContDebitId == cont401.ID && r.ContCreditId == cont4111.ID && r.Valoare == 60m) == 1);

            CheckRefuza("Contrapartida stinsă trebuie să apară pe liniile notei (factura lui Y nu se compensează cu nota lui X)",
                () => ImperechereService.Imperecheaza(os, ntc, fclY, 10m));

            ImperechereService.Imperecheaza(os, ntc, fct, 60m);
            ImperechereService.Imperecheaza(os, ntc, fcl, 60m);
            Check("Nota DUBLĂ stinge de două ori (60 pe datoria X + 60 pe creanța X); ambele facturi rămân cu 40",
                os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentStingatorId == ntc.ID) == 2
                && ImperechereService.Ramas(os, fct.ID) == 40m
                && ImperechereService.Ramas(os, fcl.ID) == 40m);

            CheckRefuza("Plafonul PER CONTRAPARTIDĂ e consumat (2 × 60): a treia stingere se refuză",
                () => {
                    var altaFcl = os.CreateObject<FacturaIesire>();
                    altaFcl.Data = new DateOnly(2026, 5, 7);
                    altaFcl.Predator = sediu;
                    altaFcl.Primitor = partenerX;
                    var l = os.CreateObject<FacturaIesireDetaliu>();
                    l.Document = altaFcl;
                    l.TipMaterial = tip704;
                    l.Cantitate = 1m;
                    l.PretUnitar = 50m;
                    os.CommitChanges();
                    MotorOperare.Opereaza(os, altaFcl);
                    ImperechereService.Imperecheaza(os, ntc, altaFcl, 10m);
                });

            CheckRefuza("Gardianul de anulare acoperă nota pe rolul de stingător (coloana DocumentStingator)",
                () => MotorOperare.AnuleazaOperarea(os, ntc));

            os.Delete(os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == ntc.ID).ToList());
            os.CommitChanges();
            MotorOperare.AnuleazaOperarea(os, ntc);
            Check("După ștergerea stingerilor nota se anulează normal (link fără registre proprii — 31d)",
                ntc.Stare == StareDocument.Draft
                && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == ntc.ID));

            CurataCmp(os);
            Check("Curățenie finală compensare (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajCmp))
                && !os.GetObjectsQuery<NotaContabila>().Any(d => d.Numar.StartsWith(MarcajCmp)));
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
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
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
            // Fix post-review: generarea „în urmă" ar închide a doua oară
            // soldurile deja închise cumulat de o lună ulterioară vie.
            CheckRefuza("Generare pentru o lună ANTERIOARĂ unei închideri vii → refuz zgomotos (închiderile sunt cronologice)",
                () => InchidereTvaService.Genereaza(os, 2026, 8, unitate.ID));

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
            os.CommitChanges();

            // Fix post-review (anti-stale): între generarea draftului și operare
            // mai intră un document de TVA în lună → soldurile nu mai sunt cele
            // din linii, operarea trebuie să REFUZE (altfel luna nu se închide
            // la 0 și regenerarea rămâne blocată de „închiderea vie").
            var fctOct2 = os.CreateObject<FacturaIntrare>();
            fctOct2.Numar = "E2E-ITV-FF3";
            fctOct2.Data = new DateOnly(2026, 10, 20);
            fctOct2.Predator = furnizor;
            fctOct2.Primitor = gestiune;
            var linieOct2 = os.CreateObject<FacturaIntrareDetaliu>();
            linieOct2.Document = fctOct2;
            linieOct2.TipMaterial = tip628;
            linieOct2.Cantitate = 1m;
            linieOct2.PretUnitar = 10m;
            linieOct2.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctOct2);
            CheckRefuza("Operarea unui draft STALE (soldurile s-au schimbat de la generare) → refuz, cu îndemnul de regenerare",
                () => MotorOperare.Opereaza(os, itvOctBis));
            Check("Draftul stale nu a lăsat rânduri-fantomă (33d)",
                !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == itvOctBis.ID));

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
            // Fix post-review: lotul produs de ALTĂ linie a aceluiași document
            // are prețul nefinalizat (0) la validare — consumul lui ar lăsa
            // valoare orfană în registrul de stoc, cu invariantul satisfăcut.
            RefuzAsm("Consum pe lotul PRODUS de altă linie a aceluiași document → refuz (lanțul de kitting = documente separate)", gestiune, gestiune, doc => {
                LinieAsm(doc, DirectieAsamblare.Consum, 2m, lotA);
                var produsNouLinie = LinieAsm(doc, DirectieAsamblare.Produs, 1m, pretEvaluare: 10m, produsNou: produsKit);
                LinieAsm(doc, DirectieAsamblare.Consum, 1m, produsNouLinie.Lot);
            });
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

    // ============ Scenariul e2e 1C-a: ReturFurnizor / ReturClient ============
    // Corespondența de STORNO (design §7, rezoluția spike-ului): liniile se culeg
    // POZITIVE, PregatesteOperare le semnează negativ, iar rândurile se postează
    // pe corespondența ORIGINALĂ cu valori negative — FĂRĂ flag-ul `Storno`
    // (ăla rămâne al meta-operației Storneaza: stornarea unui retur dă rânduri
    // POZITIVE cu Storno=true). Singura extensie de motor e
    // `RegulaContare.PastreazaSemn`. RDC = UN document cu linii pe două roluri
    // (venit fără lot / cost cu lotul original), cu `Total` = doar venitul.
    // Luna decembrie 2026 e nefolosită de celelalte blocuri.
    {
        const string MarcajRet = "E2E-RET";

        void CurataRet(IObjectSpace os) {
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajRet)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                os.Delete(doc);
            os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajRet)).ToList());
            os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajRet)).ToList());
            os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajRet)).ToList());
            os.CommitChanges();
        }

        using (var os = provider.CreateObjectSpace()) {
            CurataRet(os);

            var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");   // marfă (MF)
            var tip301 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "301");   // alt Tip de stoc (coerență)
            var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");   // venit din vânzarea mărfurilor
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var tipRlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
            var tipRdc = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            var cont4426 = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
            var cont4427 = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
            var cont371 = os.FirstOrDefault<Cont>(c => c.Simbol == "371");
            var cont607 = os.FirstOrDefault<Cont>(c => c.Simbol == "607");
            var cont707 = os.FirstOrDefault<Cont>(c => c.Simbol == "707");

            // --- Seed RLF/RDC privat ---
            var stocRlf = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRlf.ID).ToList();
            var stocRdc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipRdc.ID).ToList();
            Check("Seed RLF: ancoră + numerotare RLF-; stoc +1 pe PREDATOR (generic→Magazie, MF→Marfuri) — semnul liniei dă ieșirea",
                tipRlf != null && tipRlf.ClrType == nameof(ReturFurnizor)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRlf.ID)?.Serie == "RLF-"
                && stocRlf.Count == 2
                && stocRlf.All(r => r.Latura == LaturaDocument.Predator && r.Semn == +1)
                && stocRlf.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                && stocRlf.Any(r => r.TipStoc == TipStoc.Marfuri));
            Check("Seed RDC: ancoră + numerotare RDC-; stoc −1 pe PRIMITOR (marfa REVINE pe lotul original)",
                tipRdc != null && tipRdc.ClrType == nameof(ReturClient)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipRdc.ID)?.Serie == "RDC-"
                && stocRdc.Count == 2
                && stocRdc.All(r => r.Latura == LaturaDocument.Primitor && r.Semn == -1)
                && stocRdc.Any(r => r.ClasaId == null && r.TipStoc == TipStoc.Magazie)
                && stocRdc.Any(r => r.TipStoc == TipStoc.Marfuri));
            var contareRlf = os.GetObjectsQuery<RegulaContare>().Where(r => r.TipDocumentId == tipRlf.ID).ToList();
            Check("Seed RLF: UN rând generic Natura=Stoc cu PastreazaSemn — 3xx (Tipul) = furnizor (fallback 401)",
                contareRlf.Count == 1
                && contareRlf[0] is { NaturaFiltru: NaturaClasa.Stoc, PastreazaSemn: true, SemnFiltru: null,
                    SursaContDebit: SursaCont.TipMaterial, SursaContCredit: SursaCont.RepartitorPrimitor }
                && contareRlf[0].ContCreditId == cont401.ID && contareRlf[0].ContDebitId == null);
            var venitRdc = os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipDocumentId == tipRdc.ID && r.TipMaterialId == null).ToList();
            var costRdc = os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipDocumentId == tipRdc.ID && r.TipMaterialId == tip371.ID).ToList();
            Check("Seed RDC: rând generic de VENIT (Natura=Serviciu, PastreazaSemn) — client (fallback 4111) = contul Tipului, fără fallback",
                venitRdc.Count == 1
                && venitRdc[0] is { NaturaFiltru: NaturaClasa.Serviciu, PastreazaSemn: true, SemnFiltru: null,
                    SursaContDebit: SursaCont.RepartitorPredator, SursaContCredit: SursaCont.TipMaterial }
                && venitRdc[0].ContDebitId == cont4111.ID && venitRdc[0].ContCreditId == null);
            Check("Seed RDC: costul REVINE — 6xx = 3xx per TipMaterial cu excepțiile profilului (607=371), PastreazaSemn",
                costRdc.Count == 1
                && costRdc[0] is { PastreazaSemn: true, SemnFiltru: null,
                    SursaContDebit: SursaCont.Explicit, SursaContCredit: SursaCont.TipMaterial }
                && costRdc[0].ContDebitId == cont607.ID);
            var tvaRlf = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRlf.ID);
            var tvaRdc = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipRdc.ID);
            Check("Seed: PoliticaTva pe retururi (RLF deductibil/contrapartidă primitor 401; RDC colectat/contrapartidă predator 4111) + TipTva implicit N21",
                tvaRlf is { Directie: DirectieTva.Deductibil, SursaContrapartida: SursaCont.RepartitorPrimitor }
                && tvaRlf.ContrapartidaFallbackId == cont401.ID
                && tvaRdc is { Directie: DirectieTva.Colectat, SursaContrapartida: SursaCont.RepartitorPredator }
                && tvaRdc.ContrapartidaFallbackId == cont4111.ID
                && tipRlf.TipTvaImplicitId == n21.ID && tipRdc.TipTvaImplicitId == n21.ID);

            // --- Fixtures ---
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajRet + "-FURN";
            furnizor.Denumire = "Furnizor probă retur";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajRet + "-CLI";
            client.Denumire = "Client probă retur";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = MarcajRet + "-G";
            gestiune.Denumire = "Gestiune probă retur";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajRet + "-P";
            produs.Denumire = "Marfă probă retur";
            produs.UM = "BUC";
            produs.TipMaterial = tip371;
            os.CommitChanges();

            List<RegistruStoc> Stoc(Document doc) =>
                os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();
            List<RegistruContabil> Note(Document doc) =>
                os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID && !r.Storno).ToList();
            decimal SoldCont(Cont cont) =>
                (os.GetObjectsQuery<RegistruContabil>().Where(r => r.ContDebitId == cont.ID).Sum(r => (decimal?)r.Valoare) ?? 0m)
                - (os.GetObjectsQuery<RegistruContabil>().Where(r => r.ContCreditId == cont.ID).Sum(r => (decimal?)r.Valoare) ?? 0m);
            decimal SoldStoc(Lot lot, DateOnly? la = null) =>
                StocService.Sold(os, new CheieStoc(lot.ID, gestiune.ID, TipStoc.Marfuri), la);

            // --- Contextul: NIR manual, 10 buc × 10 lei ---
            var nir = os.CreateObject<NIR>();
            nir.Data = new DateOnly(2026, 12, 2);
            nir.Predator = furnizor; nir.Primitor = gestiune;
            var linNir = os.CreateObject<DocumentDetaliu>();
            linNir.Document = nir; linNir.TipMaterial = tip371; linNir.Cantitate = 10m; linNir.Valoare = 100m;
            var lot = linNir.CreeazaLot(os, produs, gestiune);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nir);
            Check("Precondiție retururi: lotul original pe Marfuri (10 buc × 10 lei)",
                lot.PretUnitar == 10m && SoldStoc(lot) == 10m);

            var sold4426Initial = SoldCont(cont4426);
            var sold4427Initial = SoldCont(cont4427);

            // --- RLF: 4 buc din lotul original, N21 (culegere POZITIVĂ) ---
            var dRlf = new DateOnly(2026, 12, 5);
            var rlf = os.CreateObject<ReturFurnizor>();
            rlf.Data = dRlf; rlf.Predator = gestiune; rlf.Primitor = furnizor;
            var linRlf = os.CreateObject<DocumentDetaliu>();
            linRlf.Document = rlf; linRlf.TipMaterial = tip371; linRlf.Lot = lot;
            linRlf.Cantitate = 4m; linRlf.TipTva = n21;
            os.CommitChanges();

            Check("RLF nu generează conex/secundar", MotorOperare.Opereaza(os, rlf) == null);
            Check("RLF → Operat + număr din politică (RLF-); linia culeasă pozitiv devine NEGATIVĂ (−4 / −40 / −8.4 TVA)",
                rlf.Stare == StareDocument.Operat && rlf.Numar?.StartsWith("RLF-") == true
                && linRlf.Cantitate == -4m && linRlf.Valoare == -40m && linRlf.ValoareTva == -8.4m);
            var stocRlfRand = Stoc(rlf);
            Check("RLF stoc: UN rând −4 / −40 pe Marfuri, în gestiunea predatoare (regula +1 × linia negativă)",
                stocRlfRand.Count == 1
                && stocRlfRand[0] is { Cantitate: -4m, Valoare: -40m, TipStoc: TipStoc.Marfuri }
                && stocRlfRand[0].RepartitorId == gestiune.ID
                && SoldStoc(lot) == 6m);
            var noteRlf = Note(rlf);
            Check("RLF note: 371 = 401 cu −40 și 4426 = 401 cu −8.4, pe corespondența ORIGINALĂ și FĂRĂ flag Storno",
                noteRlf.Count == 2
                && noteRlf.Any(r => r.ContDebitId == cont371.ID && r.ContCreditId == cont401.ID && r.Valoare == -40m)
                && noteRlf.Any(r => r.ContDebitId == cont4426.ID && r.ContCreditId == cont401.ID && r.Valoare == -8.4m)
                && noteRlf.All(r => !r.Storno && r.Data == dRlf));
            Check("RLF: Total = brutul negativ (−48.4) — datoria către furnizor scade",
                rlf.Total == -48.4m);
            Check("TVA deductibilă scade cu 8.4 (soldul 4426 după retur)",
                SoldCont(cont4426) - sold4426Initial == -8.4m);

            // --- Gardianul de sold: retur peste disponibil ---
            var pesteSold = os.CreateObject<ReturFurnizor>();
            pesteSold.Data = new DateOnly(2026, 12, 8);
            pesteSold.Predator = gestiune; pesteSold.Primitor = furnizor;
            var linPesteSold = os.CreateObject<DocumentDetaliu>();
            linPesteSold.Document = pesteSold; linPesteSold.TipMaterial = tip371; linPesteSold.Lot = lot;
            linPesteSold.Cantitate = 20m; linPesteSold.TipTva = n21;
            os.CommitChanges();
            CheckRefuza("Retur la furnizor peste sold (20 din 6) → refuzul gardianului de sold",
                () => MotorOperare.Opereaza(os, pesteSold));
            Check("Retur refuzat — fără rânduri-fantomă (33d)",
                !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == pesteSold.ID)
                && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == pesteSold.ID));
            os.Delete(pesteSold.Detalii.ToList());
            os.Delete(pesteSold);
            os.CommitChanges();

            // --- RDC: UN document, linie de venit (fără lot) + linie de cost (lotul original) ---
            var dRdc = new DateOnly(2026, 12, 10);
            var rdc = os.CreateObject<ReturClient>();
            rdc.Data = dRdc; rdc.Predator = client; rdc.Primitor = gestiune;
            var linVenit = os.CreateObject<DocumentDetaliu>();
            linVenit.Document = rdc; linVenit.TipMaterial = tip707; linVenit.Valoare = 100m; linVenit.TipTva = n21;
            var linCost = os.CreateObject<DocumentDetaliu>();
            linCost.Document = rdc; linCost.TipMaterial = tip371; linCost.Lot = lot; linCost.Cantitate = 3m;
            os.CommitChanges();

            MotorOperare.Opereaza(os, rdc);
            Check("RDC → Operat + număr RDC-; venitul semnat negativ (−100 / −21), costul negativ (−3 / −30), cantitatea de venit pro-formă pozitivă",
                rdc.Stare == StareDocument.Operat && rdc.Numar?.StartsWith("RDC-") == true
                && linVenit.Cantitate == 1m && linVenit.Valoare == -100m && linVenit.ValoareTva == -21m
                && linCost.Cantitate == -3m && linCost.Valoare == -30m && linCost.ValoareTva == 0m);
            var stocRdcRand = Stoc(rdc);
            Check("RDC stoc: UN rând +3 / +30 pe Marfuri, în gestiunea primitoare — marfa REVINE pe lotul original (regula −1 × linia negativă)",
                stocRdcRand.Count == 1
                && stocRdcRand[0] is { Cantitate: 3m, Valoare: 30m, TipStoc: TipStoc.Marfuri }
                && stocRdcRand[0].LotId == lot.ID && stocRdcRand[0].RepartitorId == gestiune.ID
                && SoldStoc(lot) == 9m);
            var noteRdc = Note(rdc);
            Check("RDC note: 4111 = 707 cu −100, 4111 = 4427 cu −21, 607 = 371 cu −30 — toate fără flag Storno",
                noteRdc.Count == 3
                && noteRdc.Any(r => r.ContDebitId == cont4111.ID && r.ContCreditId == cont707.ID && r.Valoare == -100m)
                && noteRdc.Any(r => r.ContDebitId == cont4111.ID && r.ContCreditId == cont4427.ID && r.Valoare == -21m)
                && noteRdc.Any(r => r.ContDebitId == cont607.ID && r.ContCreditId == cont371.ID && r.Valoare == -30m)
                && noteRdc.All(r => !r.Storno && r.Data == dRdc));
            Check("RDC: Total = DOAR liniile de venit (−121) — costul e mișcare internă venit↔stoc, nu creanță",
                rdc.Total == -121m
                && rdc.Detalii.Sum(d => d.Valoare + d.ValoareTva) == -151m); // totalul „naiv" al bazei
            // 4427 e cont de PASIV: rândul −21 stă pe CREDIT, deci soldul creditor
            // (−SoldCont, unde SoldCont = debit − credit) scade cu 21.
            Check("TVA colectată (sold CREDITOR) scade cu 21 după retur",
                -(SoldCont(cont4427) - sold4427Initial) == -21m);

            // --- Idempotența semnării: anulare → re-operare ---
            var numarRdc = rdc.Numar;
            MotorOperare.AnuleazaOperarea(os, rdc);
            Check("Anulare directă RDC (frunză) → Draft, registre goale, stocul revine la 6",
                rdc.Stare == StareDocument.Draft
                && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == rdc.ID)
                && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == rdc.ID)
                && SoldStoc(lot) == 6m);
            MotorOperare.Opereaza(os, rdc);
            Check("Re-operare → același număr și ACELEAȘI valori (semnarea e idempotentă prin Abs)",
                rdc.Stare == StareDocument.Operat && rdc.Numar == numarRdc
                && linVenit.Valoare == -100m && linVenit.ValoareTva == -21m
                && linCost.Cantitate == -3m && linCost.Valoare == -30m
                && rdc.Total == -121m && Note(rdc).Count == 3 && Stoc(rdc).Count == 1);

            // --- Refuzuri (obiecte throwaway, curățate imediat) ---
            void RefuzRet(string nume, Func<Document> fabrica) {
                var doc = fabrica();
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

            ReturClient RdcNou() {
                var doc = os.CreateObject<ReturClient>();
                doc.Data = new DateOnly(2026, 12, 12);
                doc.Predator = client; doc.Primitor = gestiune;
                return doc;
            }
            DocumentDetaliu LinieRdc(ReturClient doc, TipMaterial tip, decimal cantitate, decimal valoare, Lot lotLinie) {
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc; d.TipMaterial = tip; d.Cantitate = cantitate; d.Valoare = valoare;
                if (lotLinie != null)
                    d.Lot = lotLinie;
                return d;
            }

            RefuzRet("RDC: linie de venit (fără lot) cu Tip de STOC → refuz (venitul poartă natura Serviciu)", () => {
                var doc = RdcNou();
                LinieRdc(doc, tip371, 1m, 100m, null);
                return doc;
            });
            RefuzRet("RDC: linie de cost pe lotul creat de ea însăși → refuz (marfa revine pe lotul ORIGINAL)", () => {
                var doc = RdcNou();
                LinieRdc(doc, tip707, 1m, 100m, null);
                var d = LinieRdc(doc, tip371, 2m, 0m, null);
                d.CreeazaLot(os, produs, gestiune);
                return doc;
            });
            RefuzRet("RDC: linie de cost cu Tip incoerent cu produsul lotului → refuz", () => {
                var doc = RdcNou();
                LinieRdc(doc, tip707, 1m, 100m, null);
                LinieRdc(doc, tip301, 1m, 0m, lot); // lotul e al unui produs cu Tipul 371
                return doc;
            });
            RefuzRet("RLF: laturi inversate (predator partener / primitor gestiune) → refuz", () => {
                var doc = os.CreateObject<ReturFurnizor>();
                doc.Data = new DateOnly(2026, 12, 12);
                doc.Predator = furnizor; doc.Primitor = gestiune;
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = 1m;
                return doc;
            });
            RefuzRet("RLF: linie fără lot → refuz (returul descarcă lotul original)", () => {
                var doc = os.CreateObject<ReturFurnizor>();
                doc.Data = new DateOnly(2026, 12, 12);
                doc.Predator = gestiune; doc.Primitor = furnizor;
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc; d.TipMaterial = tip371; d.Cantitate = 1m;
                return doc;
            });
            // Fix post-review pas 4: un Tip de stoc creat ÎNTRE updater-e n-are
            // regulă derivată 607=3xx → fără refuz, stocul s-ar mișca fără notă
            // (exact defectul închis pe DSC la 38c). Lotul e „de deschidere"
            // (LinieIntrareId null, ca migrarea) ca să izoleze EXACT eroarea.
            var clasaMf = os.FirstOrDefault<ClasaProdus>(c => c.Cod == "MF");
            var tipNou = os.CreateObject<TipMaterial>();
            tipNou.Cod = MarcajRet + "-TIPX"; tipNou.Denumire = "Tip fără regulă (probă)"; tipNou.Clasa = clasaMf;
            var produsNou = os.CreateObject<Produs>();
            produsNou.Cod = MarcajRet + "-PRX"; produsNou.Denumire = "Produs Tip nou"; produsNou.UM = "BUC";
            produsNou.TipMaterial = tipNou;
            var lotNou = os.CreateObject<Lot>();
            lotNou.Produs = produsNou; lotNou.Gestiune = gestiune;
            lotNou.PretUnitar = 5m; lotNou.Data = new DateOnly(2026, 12, 1);
            os.CommitChanges();
            RefuzRet("RDC: linie de cost cu Tip FĂRĂ regulă de contare derivată → refuz (stocul nu se mișcă fără notă — 38c)", () => {
                var doc = RdcNou();
                LinieRdc(doc, tip707, 1m, 100m, null);
                LinieRdc(doc, tipNou, 1m, 0m, lotNou);
                return doc;
            });
            os.Delete(lotNou); os.Delete(produsNou); os.Delete(tipNou);
            os.CommitChanges();
            // Fix post-review pas 4: Capitalizat pe venitul stornat n-are sens
            // economic și ar compunda brutul la re-operare — refuz la validare.
            var ned21Ret = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
            RefuzRet("RDC: linie de venit cu regim Capitalizat (NED21) → refuz (semnarea ar compunda la re-operare)", () => {
                var doc = RdcNou();
                var d = LinieRdc(doc, tip707, 1m, 100m, null);
                d.TipTva = ned21Ret;
                return doc;
            });
            // Fix post-review: Capitalizat pe RLF ar umfla valoarea liniei peste
            // costul lotului (net × cotă) — identificarea specifică ruptă.
            RefuzRet("RLF: linie cu regim Capitalizat (NED21) → refuz (valoarea returului e costul lotului)", () => {
                var doc = os.CreateObject<ReturFurnizor>();
                doc.Data = new DateOnly(2026, 12, 12);
                doc.Predator = gestiune; doc.Primitor = furnizor;
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = doc; d.TipMaterial = tip371; d.Lot = lot; d.Cantitate = 1m; d.TipTva = ned21Ret;
                return doc;
            });

            // --- Storno pe RLF: rândurile inverse sunt POZITIVE cu Storno=true ---
            var dStorno = new DateOnly(2026, 12, 20);
            MotorOperare.Storneaza(os, rlf, dStorno);
            var toateStocRlf = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == rlf.ID).ToList();
            var toateNoteRlf = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == rlf.ID).ToList();
            Check("Storno pe RETUR: rândurile inverse sunt POZITIVE și poartă Storno=true (flag-ul rămâne al meta-operației)",
                rlf.Stare == StareDocument.Stornat
                && toateStocRlf.Count == 2 && toateNoteRlf.Count == 4
                && toateStocRlf.Single(r => r.Storno) is { Cantitate: 4m, Valoare: 40m, Data.Day: 20 }
                && toateNoteRlf.Count(r => r.Storno && r.Data == dStorno) == 2
                && toateNoteRlf.Any(r => r.Storno && r.ContDebitId == cont371.ID && r.ContCreditId == cont401.ID && r.Valoare == 40m)
                && toateNoteRlf.Any(r => r.Storno && r.ContDebitId == cont4426.ID && r.ContCreditId == cont401.ID && r.Valoare == 8.4m));
            Check("Stocul revine după stornarea returului (10 − 4 + 3 + 4 = 13); TVA deductibilă revine la valoarea inițială",
                SoldStoc(lot, dStorno) == 13m
                && SoldCont(cont4426) - sold4426Initial == 0m);

            CurataRet(os);
            Check("Curățenie finală retururi (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajRet))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajRet))
                && !os.GetObjectsQuery<ReturFurnizor>().Any()
                && !os.GetObjectsQuery<ReturClient>().Any());
        }
    }

    // ============ Felia Api FCT — semantica override-ului de TVA (privat) ============
    // Complementul blocului bugetar (review advers F2-D1): pe regimul Normal
    // override-ul e LEGITIM (36a — factura furnizorului bate rotunjirea), iar
    // recalculul e CONDIȚIONAT de declanșatorii din UI — un PUT care nu atinge
    // baza/TipTva nu pierde override-ul (clientul nu retrimite ValoareTva).
    {
        const string MarcajApiPrv = "E2E-APIPRV";
        using var os = provider.CreateObjectSpace();
        void CurataApiPrv(IObjectSpace o) {
            foreach (var d in o.GetObjectsQuery<FacturaIntrare>()
                    .Where(x => x.Numar.StartsWith(MarcajApiPrv)).ToList()) {
                o.Delete(o.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == d.ID).ToList());
                o.Delete(d);
            }
            foreach (var r in o.GetObjectsQuery<Repartitor>()
                    .Where(x => x.Cod.StartsWith(MarcajApiPrv)).ToList())
                o.Delete(r);
            o.CommitChanges();
        }
        CurataApiPrv(os);
        var n21Api = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var sddApi = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
        var tipServApi = os.GetObjectsQuery<TipMaterial>()
            .First(t => t.Clasa.Natura == NaturaClasa.Serviciu);
        var furnizorApi = os.CreateObject<Partener>();
        furnizorApi.Cod = MarcajApiPrv + "-F"; furnizorApi.Denumire = "Furnizor Api Privat";
        var gestiuneApi = os.CreateObject<Gestiune>();
        gestiuneApi.Cod = MarcajApiPrv + "-G"; gestiuneApi.Denumire = "Gestiune Api Privat";
        os.CommitChanges();

        var w = new FacturaIntrareWriteDto {
            Numar = MarcajApiPrv + "-1", Data = new DateOnly(2026, 3, 10),
            PredatorId = furnizorApi.ID, PrimitorId = gestiuneApi.ID,
            Linii = { new FacturaIntrareLinieWriteDto {
                TipMaterialId = tipServApi.ID, Cantitate = 1m, PretUnitar = 100m,
                TipTvaId = n21Api.ID } }
        };
        var idFctPrv = FacturaIntrareApply.Aplica(os, null, w);
        var liniePrv = FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0];
        Check("Api privat/N21: calculul la culegere — net 100 + TVA 21",
            liniePrv is { Valoare: 100m, ValoareTva: 21m });
        w.Linii[0].Id = liniePrv.Id;
        w.Linii[0].ValoareTva = 21.37m;
        FacturaIntrareApply.Aplica(os, idFctPrv, w);
        Check("Override pe regim Normal → acceptat, aplicat DUPĂ calcul (36a)",
            FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0].ValoareTva == 21.37m);
        w.Linii[0].ValoareTva = null;
        FacturaIntrareApply.Aplica(os, idFctPrv, w);
        Check("PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
            FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0].ValoareTva == 21.37m);
        w.Linii[0].PretUnitar = 200m;
        FacturaIntrareApply.Aplica(os, idFctPrv, w);
        Check("Schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
            FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0] is { Valoare: 200m, ValoareTva: 42m });
        if (sddApi != null) {
            w.Linii[0].TipTvaId = sddApi.ID;
            w.Linii[0].ValoareTva = 5m;
            CheckRefuza("Override pe regim Scutit (SDD) → refuz (F2-D1: regimul nu poartă TVA separat)",
                () => FacturaIntrareApply.Aplica(os, idFctPrv, w));
        }
        CurataApiPrv(os);
    }

    // ======== Felia Api DEC — semantica override-ului de TVA + 4426 = 542 (privat) ========
    // Complementul blocului bugetar `E2E-API-DEC` (F8-D13.1), pe același tipar ca
    // FCT: la bugetar toate regimurile sunt Capitalizat, deci acolo override-ul
    // are DOAR refuzuri; semantica POZITIVĂ (păstrare fără declanșator, cedare la
    // schimbarea bazei) cere un regim cu TVA separat și trăiește aici. Nu se
    // inventează tipuri de TVA în seedul bugetar pentru probe (decizia 21,
    // precedentul 56f).
    //
    // În plus față de FCT: DEC e singurul tip cu PoliticaTva pe latura
    // PREDATORULUI care e un ANGAJAT — rândul de TVA iese 4426 = 542 (bonul cu
    // TVA deductibil justificat pe decont), iar creditul cade pe fallback-ul 542
    // al regulii.
    {
        const string MarcajApiDecPrv = "E2E-APIDEC-PRV";
        using var os = provider.CreateObjectSpace();
        void CurataApiDecPrv(IObjectSpace o) {
            var repIds = o.GetObjectsQuery<Repartitor>()
                .Where(x => x.Cod.StartsWith(MarcajApiDecPrv)).Select(x => x.ID).ToList();
            var docs = o.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            o.Delete(o.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            o.Delete(o.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            o.Delete(docs);
            o.Delete(o.GetObjectsQuery<Repartitor>().Where(x => x.Cod.StartsWith(MarcajApiDecPrv)).ToList());
            o.CommitChanges();
        }
        CurataApiDecPrv(os);

        var n21Dec = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var sddDec = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
        var cont4426Dec = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
        var cont542Dec = os.FirstOrDefault<Cont>(c => c.Simbol == "542");
        // Tipul trebuie să aibă cont implicit: regula DEC rezolvă debitul din
        // `SursaCont.TipMaterial`, FĂRĂ fallback (32b).
        var tipCheltuiala = os.GetObjectsQuery<TipMaterial>()
            .First(t => t.Clasa.Natura == NaturaClasa.Serviciu && t.ContImplicitId != null);
        var titularPrv = os.CreateObject<Angajat>();
        titularPrv.Cod = MarcajApiDecPrv + "-ANG";
        titularPrv.Denumire = "Titular Api DEC Privat";
        var unitatePrv = os.CreateObject<UnitateInterna>();
        unitatePrv.Cod = MarcajApiDecPrv + "-U";
        unitatePrv.Denumire = "Unitate Api DEC Privat";
        os.CommitChanges();

        var wDec = new DecontWriteDto {
            Data = new DateOnly(2026, 3, 12),
            PredatorId = titularPrv.ID, PrimitorId = unitatePrv.ID,
            Linii = { new DecontLinieWriteDto {
                TipMaterialId = tipCheltuiala.ID, Descriere = "Bon justificat",
                Cantitate = 0m, PretUnitar = 100m, TipTvaId = n21Dec.ID } }
        };
        var idDecPrv = DecontApply.Aplica(os, null, wDec);
        var linieDecPrv = DecontApply.Citeste(os, idDecPrv).Linii[0];
        Check("Api DEC privat/N21: calculul la culegere — net 100 + TVA 21, cu cantitatea pro-forma 0 → 1 (F8-D2)",
            linieDecPrv is { Valoare: 100m, ValoareTva: 21m, Cantitate: 1m });
        wDec.Linii[0].Id = linieDecPrv.Id;
        wDec.Linii[0].ValoareTva = 21.37m;
        DecontApply.Aplica(os, idDecPrv, wDec);
        Check("Api DEC privat: override pe regim Normal → acceptat, aplicat DUPĂ calcul (36a — bonul bate rotunjirea)",
            DecontApply.Citeste(os, idDecPrv).Linii[0].ValoareTva == 21.37m);
        wDec.Linii[0].ValoareTva = null;
        DecontApply.Aplica(os, idDecPrv, wDec);
        Check("Api DEC privat: PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
            DecontApply.Citeste(os, idDecPrv).Linii[0].ValoareTva == 21.37m);
        wDec.Linii[0].PretUnitar = 200m;
        DecontApply.Aplica(os, idDecPrv, wDec);
        Check("Api DEC privat: schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
            DecontApply.Citeste(os, idDecPrv).Linii[0] is { Valoare: 200m, ValoareTva: 42m });
        if (sddDec != null) {
            wDec.Linii[0].TipTvaId = sddDec.ID;
            wDec.Linii[0].ValoareTva = 5m;
            CheckRefuza("Api DEC privat: override pe regim Scutit (SDD) → refuz (regimul nu poartă TVA separat)",
                () => DecontApply.Aplica(os, idDecPrv, wDec));
            wDec.Linii[0].TipTvaId = n21Dec.ID;
            wDec.Linii[0].ValoareTva = null;
            DecontApply.Aplica(os, idDecPrv, wDec);
        }
        OperareApi.Opereaza(os, idDecPrv);
        var notePrvDec = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId == idDecPrv && !r.Storno).ToList();
        Check("Api DEC privat operat: nota principală (cheltuiala = 542, 200 net) + rândul de TVA 4426 = 542 (42) — PoliticaTva pe latura predatorului, care e ANGAJATUL",
            notePrvDec.Count == 2
            && notePrvDec.Any(n => n.ContDebitId == tipCheltuiala.ContImplicitId
                && n.ContCreditId == cont542Dec.ID && n.Valoare == 200m)
            && notePrvDec.Any(n => n.ContDebitId == cont4426Dec.ID
                && n.ContCreditId == cont542Dec.ID && n.Valoare == 42m)
            && DecontApply.Citeste(os, idDecPrv) is { Total: 242m, PoateAnula: true });
        OperareApi.AnuleazaOperarea(os, idDecPrv);
        DecontApply.Sterge(os, idDecPrv);
        CurataApiDecPrv(os);
        Check("Curățenie finală felia Api DEC privat (fără reziduuri e2e)",
            DecontApply.Citeste(os, idDecPrv) == null
            && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiDecPrv)));
    }

    // ======= Felia Api FCL — culegere, TVA, operare (F4-D9, pasul 1 al feliei) =======
    // Fluxul de VÂNZARE parcurs prin CONTRACTUL feliei: WriteDto →
    // `FacturaIesireApply.Aplica` → `Citeste`/`Lista` → dry-run → `OperareApi`.
    // Endpoint-urile din host sunt transport peste EXACT acest cod, deci ce e verde
    // aici e verde și pe sârmă. Blocul trăiește în suita PRIVATĂ fiindcă vânzarea
    // din stoc e a profilului privat (la bugetar liniile de stoc pe FCL sunt
    // interzise declarativ — 30a — și DSC e tip inert).
    //
    // Ce exersează în plus față de felia FCT:
    //   * `Numar` SERVER-OWNED (serie fiscală „FCL-"): lipsește din WriteDto și se
    //     consumă abia la materializarea operării — exact invers față de FCT;
    //   * PINUL de lot CULES pe linie (`LotId` de bază) lângă produs („General!" +
    //     „Specific?", P2 §4) — pe FCT lotul era server-owned;
    //   * `GestiuneDescarcareId` pe header;
    //   * DSC-ul autogenerat apare în `Copii[]` — îl generează MOTORUL
    //     (`GenereazaSecundar` → `DescarcareService`), felia doar îl citește.
    {
        const string MarcajApiFcl = "E2E-API-FCL";
        using var os = provider.CreateObjectSpace();

        void CurataApiFcl(IObjectSpace o) {
            var repIds = o.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajApiFcl)).Select(r => r.ID).ToList();
            var docs = o.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            o.Delete(o.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            o.Delete(o.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            o.Delete(o.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            o.Delete(o.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Copiii autogenerați (DSC) întâi — DocumentSursa spre FCL.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                o.Delete(doc);
            // Rândurile de sold de DESCHIDERE n-au document (25e): se prind pe lot.
            var lotIds = o.GetObjectsQuery<Lot>()
                .Where(l => l.Produs.Cod.StartsWith(MarcajApiFcl)).Select(l => l.ID).ToList();
            o.Delete(o.GetObjectsQuery<RegistruStoc>().Where(r => lotIds.Contains(r.LotId)).ToList());
            o.Delete(o.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFcl)).ToList());
            o.Delete(o.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiFcl)).ToList());
            o.Delete(o.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiFcl)).ToList());
            o.Delete(o.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiFcl)).ToList());
            o.CommitChanges();
        }
        CurataApiFcl(os);

        var sediuFcl = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var tipMarfa = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");   // MF → Marfuri
        var tipServiciuFcl = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704"); // VEN
        var n21Fcl = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var sddFcl = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
        var cont4111Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
        var cont4427Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
        var cont707Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "707");

        var clientFcl = os.CreateObject<Partener>();
        clientFcl.Cod = MarcajApiFcl + "-CL";
        clientFcl.Denumire = "Client probă felia Api FCL";
        clientFcl.CodFiscal = "RO87654321";
        var gestiuneFcl = os.CreateObject<Gestiune>();
        gestiuneFcl.Cod = MarcajApiFcl + "-G";
        gestiuneFcl.Denumire = "Gestiune probă felia Api FCL";
        var codEcFcl = os.CreateObject<CodEconomic>();
        codEcFcl.Cod = MarcajApiFcl + "-CE";
        codEcFcl.Denumire = "Cod economic probă felia Api FCL";
        var produsFcl = os.CreateObject<Produs>();
        produsFcl.Cod = MarcajApiFcl + "-A";
        produsFcl.Denumire = "Marfă probă felia Api FCL";
        produsFcl.UM = "BUC";
        produsFcl.TipMaterial = tipMarfa;
        os.CommitChanges();

        // Sold de deschidere (decizia 25e — rând fără document sursă): două loturi
        // la prețuri diferite, în Marfuri (TipStoc-ul regulii DSC pentru clasa MF).
        Lot DeschidereFcl(decimal pret, decimal cantitate, DateOnly data) {
            var l = os.CreateObject<Lot>();
            l.Produs = produsFcl; l.Gestiune = gestiuneFcl; l.PretUnitar = pret; l.Data = data;
            var r = os.CreateObject<RegistruStoc>();
            r.Data = data; r.TipStoc = TipStoc.Marfuri; r.Lot = l; r.Repartitor = gestiuneFcl;
            r.Cantitate = cantitate; r.Valoare = cantitate * pret;
            return l;
        }
        var lotVechiFcl = DeschidereFcl(5m, 10m, new DateOnly(2026, 5, 2));
        var lotNouFcl = DeschidereFcl(6m, 10m, new DateOnly(2026, 5, 4));
        os.CommitChanges();

        // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
        // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
        IReadOnlyList<string> DryRunFcl(Guid docId) {
            using var osDry = provider.CreateObjectSpace();
            return OperareApi.Valideaza(osDry, docId);
        }

        // --- Apply: creare din WriteDto (fără Numar/Valoare — server-owned) ---
        var writeFcl = new FacturaIesireWriteDto {
            Data = new DateOnly(2026, 5, 10),
            PredatorId = sediuFcl.ID,
            PrimitorId = clientFcl.ID,
            GestiuneDescarcareId = gestiuneFcl.ID,
            Linii = {
                // Pin pe lotul NOU (mai scump): identificarea specifică bate FIFO.
                new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = lotNouFcl.ID,
                    Descriere = "Marfă cu pin", Cantitate = 3m, PretUnitar = 20m,
                    TipTvaId = n21Fcl.ID, CodEconomicId = codEcFcl.ID
                },
                new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID,
                    Descriere = "Marfă FIFO", Cantitate = 4m, PretUnitar = 20m,
                    TipTvaId = n21Fcl.ID
                },
                // Fără TipTva în payload ⇒ default-ul tipului de document (N21).
                new FacturaIesireLinieWriteDto {
                    TipMaterialId = tipServiciuFcl.ID,
                    Descriere = "Transport", Cantitate = 1m, PretUnitar = 100m
                }
            }
        };
        var idFcl = FacturaIesireApply.Aplica(os, null, writeFcl);
        var citFcl = FacturaIesireApply.Citeste(os, idFcl);
        FacturaIesireLinieReadDto LinieFclDto(string descriere) =>
            FacturaIesireApply.Citeste(os, idFcl).Linii.Single(l => l.Descriere == descriere);

        Check("Apply FCL creare → header plat: NUMĂRUL LIPSEȘTE (serie fiscală server-owned, invers față de FCT), "
            + "scadență neculeasă, gestiunea de descărcare și CodFiscal-ul clientului",
            citFcl != null && citFcl.Id == idFcl && citFcl.Stare == "Draft"
            && citFcl.Numar == null && citFcl.Data == new DateOnly(2026, 5, 10)
            && citFcl.DataScadenta == null && citFcl.DataOperare == null
            && citFcl.PredatorId == sediuFcl.ID && citFcl.PredatorDenumire == sediuFcl.Denumire
            && citFcl.PrimitorId == clientFcl.ID && citFcl.PrimitorDenumire == clientFcl.Denumire
            && citFcl.PrimitorCodFiscal == "RO87654321"
            && citFcl.GestiuneDescarcareId == gestiuneFcl.ID
            && citFcl.GestiuneDescarcareDenumire == gestiuneFcl.Denumire
            && !citFcl.Autogenerat && citFcl.DocumentSursaId == null && citFcl.Copii.Count == 0
            && citFcl.PoateEdita && citFcl.PoateOpera && !citFcl.PoateAnula && !citFcl.PoateStorna);

        var lPin = citFcl.Linii.Single(l => l.Descriere == "Marfă cu pin");
        var lFifo = citFcl.Linii.Single(l => l.Descriere == "Marfă FIFO");
        var lServ = citFcl.Linii.Single(l => l.Descriere == "Transport");
        Check("PROBA FELIEI: PINUL de lot e CULES (spre deosebire de FCT, unde lotul e server-owned) — "
            + "linia pin poartă lotul cu eticheta lui, linia FIFO rămâne fără lot",
            lPin.LotId == lotNouFcl.ID && lPin.LotEticheta == lotNouFcl.Eticheta
            && !lPin.LotEticheta.Contains("culegere")
            && lFifo.LotId == null && lFifo.LotEticheta == null);
        Check("Produsul („General!”) e pe liniile de stoc, cu cod și denumire proiectate plat; serviciul n-are produs",
            lPin.ProdusId == produsFcl.ID && lPin.ProdusCod == produsFcl.Cod
            && lPin.ProdusDenumire == produsFcl.Denumire
            && lFifo.ProdusId == produsFcl.ID && lServ.ProdusId == null && lServ.ProdusCod == null);
        Check("TVA materializat LA CULEGERE (GATE 53c): N21 → net + 21% separat (60/12,6; 80/16,8; 100/21); Total BRUT 290,4",
            lPin is { Valoare: 60m, ValoareTva: 12.6m, Cantitate: 3m, PretUnitar: 20m }
            && lFifo is { Valoare: 80m, ValoareTva: 16.8m }
            && lServ is { Valoare: 100m, ValoareTva: 21m }
            && citFcl.Total == 290.4m);
        Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (N21 pe FCL, seed privat)",
            lServ.TipTvaId == n21Fcl.ID && lServ.TipTvaCod == "N21" && lServ.TipTvaCota == 21m
            && lPin.TipTvaId == n21Fcl.ID);
        Check("Dimensiunea FRUNZEI (DIM-2: FCL are DOAR CodEconomic) — culeasă pe linia pin, absentă pe celelalte",
            lPin.CodEconomicId == codEcFcl.ID && lPin.CodEconomicCod == codEcFcl.Cod
            && lFifo.CodEconomicId == null && lFifo.CodEconomicCod == null);

        // --- Semantica override-ului de ValoareTva (aceleași reguli ca FCT) ---
        // ROUND-TRIP: clientul retrimite agregatul ÎNTREG, inclusiv TipTva-ul primit
        // la citire (pus de default) — absența lui pe o linie existentă ar fi golire.
        writeFcl.Linii[0].Id = lPin.Id;
        writeFcl.Linii[1].Id = lFifo.Id;
        writeFcl.Linii[2].Id = lServ.Id;
        writeFcl.Linii[2].TipTvaId = lServ.TipTvaId;
        writeFcl.Linii[2].ValoareTva = 20.5m;
        FacturaIesireApply.Aplica(os, idFcl, writeFcl);
        Check("Override ValoareTva pe regim Normal → acceptat, aplicat DUPĂ calcul (36a: documentul EMIS poartă rotunjirea)",
            LinieFclDto("Transport").ValoareTva == 20.5m);
        writeFcl.Linii[2].ValoareTva = null;
        FacturaIesireApply.Aplica(os, idFcl, writeFcl);
        Check("PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
            LinieFclDto("Transport").ValoareTva == 20.5m);
        writeFcl.Linii[2].PretUnitar = 200m;
        FacturaIesireApply.Aplica(os, idFcl, writeFcl);
        Check("Schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
            LinieFclDto("Transport") is { Valoare: 200m, ValoareTva: 42m });
        writeFcl.Linii[2].PretUnitar = 100m;
        FacturaIesireApply.Aplica(os, idFcl, writeFcl);

        writeFcl.Linii[2].ValoareTva = -5m;
        CheckRefuza("Override ValoareTva NEGATIV → refuz (F2-D7)",
            () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
        writeFcl.Linii[2].TipTvaId = sddFcl.ID;
        writeFcl.Linii[2].ValoareTva = 5m;
        CheckRefuza("Override pe regim Scutit (SDD) → refuz (regimul nu poartă TVA separat)",
            () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
        writeFcl.Linii[2].TipTvaId = n21Fcl.ID;
        writeFcl.Linii[2].ValoareTva = null;

        // --- Refuzuri de contract (mesaj de domeniu, nu excepție de infrastructură) ---
        FacturaIesireWriteDto PayloadFcl(FacturaIesireLinieWriteDto linie, Guid? gestiune = null) =>
            new() {
                Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiune ?? gestiuneFcl.ID,
                Linii = { linie }
            };
        CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
            FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 1m })));
        CheckRefuza("Apply cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
            FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 0.0000001m })));
        CheckRefuza("Apply cu pin pe lot inexistent → refuz cu mesaj de domeniu (nu violare de FK)", () =>
            FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = Guid.NewGuid(),
                Cantitate = 1m, PretUnitar = 10m })));
        CheckRefuza("Apply cu gestiune de descărcare inexistentă → refuz cu mesaj de domeniu", () =>
            FacturaIesireApply.Aplica(os, idFcl, PayloadFcl(new FacturaIesireLinieWriteDto {
                TipMaterialId = tipServiciuFcl.ID, Cantitate = 1m, PretUnitar = 1m }, Guid.NewGuid())));
        // M3 (F4 §Închidere): același Id de linie de DOUĂ ori în payload — a doua
        // apariție ar suprascrie tăcut prima; reconcilierea o refuză explicit.
        CheckRefuza("Apply cu același Id de linie repetat în payload → refuz (nu suprascriere tăcută)", () =>
            FacturaIesireApply.Aplica(os, idFcl, new FacturaIesireWriteDto {
                Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiuneFcl.ID,
                Linii = { writeFcl.Linii[0], writeFcl.Linii[0] }
            }));
        // M3: linie de tip BAZĂ pe draft (draft vechi pre-P2, «detaliu generic»).
        // Actualizarea ei prin Id se refuză („tip vechi"); absența ei din payload
        // o CURĂȚĂ — proba curățeniei e check-ul de reziduu de mai jos (3 linii).
        var linieBazaFcl = os.CreateObject<DocumentDetaliu>(); // NU FacturaIesireDetaliu
        linieBazaFcl.Document = os.GetObjectByKey<FacturaIesire>(idFcl);
        linieBazaFcl.TipMaterial = tipServiciuFcl; linieBazaFcl.Cantitate = 1m;
        os.CommitChanges();
        CheckRefuza("Apply cu Id de linie de tip BAZĂ (draft vechi) → refuz «tip vechi», nu adoptare tăcută", () =>
            FacturaIesireApply.Aplica(os, idFcl, new FacturaIesireWriteDto {
                Data = writeFcl.Data, PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
                GestiuneDescarcareId = gestiuneFcl.ID,
                Linii = { new FacturaIesireLinieWriteDto {
                    Id = linieBazaFcl.ID, TipMaterialId = tipServiciuFcl.ID,
                    Cantitate = 1m, PretUnitar = 1m } }
            }));
        FacturaIesireApply.Aplica(os, idFcl, writeFcl);
        citFcl = FacturaIesireApply.Citeste(os, idFcl);
        Check("Un Apply refuzat nu lasă reziduu, iar linia de tip BAZĂ absentă din payload e CURĂȚATĂ: "
            + "re-aplicarea payload-ului valid readuce agregatul la exact 3 linii, cu Total 290,4",
            citFcl.Linii.Count == 3 && citFcl.Total == 290.4m
            && citFcl.GestiuneDescarcareId == gestiuneFcl.ID);

        // --- Dry-run, apoi comanda ---
        Check("Dry-run (Valideaza) pe draftul FCL valid → listă goală", DryRunFcl(idFcl).Count == 0);
        Check("Dry-run-ul nu materializează nimic: documentul rămâne Draft, fără număr și fără registre",
            FacturaIesireApply.Citeste(os, idFcl) is { Stare: "Draft", Numar: null }
            && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idFcl));

        var rezFcl = OperareApi.Opereaza(os, idFcl);
        Check("OperareApi.Opereaza pe FCL → Operat + ConexId (descărcarea generată în aceeași tranzacție), cu mesaj pentru operator",
            rezFcl.StareNoua == StareDocument.Operat && rezFcl.ConexId != null
            && rezFcl.Mesaje.Count == 1);

        citFcl = FacturaIesireApply.Citeste(os, idFcl);
        Check("Citeste după operare: numărul vine ACUM din seria fiscală (FCL-), scadența din politică (+30), affordances inversate",
            citFcl.Stare == "Operat" && citFcl.Numar?.StartsWith("FCL-") == true
            && citFcl.DataOperare != null && citFcl.DataScadenta == citFcl.Data.AddDays(30)
            && !citFcl.PoateEdita && !citFcl.PoateOpera && citFcl.PoateAnula && citFcl.PoateStorna);
        Check("Citeste.Copii → DESCĂRCAREA conexă: codul ancorei TipDocument (DSC), draft autogenerat fără număr propriu",
            citFcl.Copii.Count == 1 && citFcl.Copii[0].Id == rezFcl.ConexId
            && citFcl.Copii[0].Tip == "DSC" && citFcl.Copii[0].Stare == "Draft"
            && citFcl.Copii[0].Autogenerat && citFcl.Copii[0].Numar == null);
        Check("Valorile culese supraviețuiesc operării (PregatesteOperare le rescrie din aceeași formulă)",
            LinieFclDto("Marfă cu pin") is { Valoare: 60m, ValoareTva: 12.6m }
            && citFcl.Total == 290.4m);

        var noteApiFcl = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idFcl).ToList();
        Check("Aceeași cale de postare ca în UI: 4111 = 707 pe marfă (140), 4111 = 704 pe serviciu (100), "
            + "4427 colectat per linie (50,4) — costul rămâne pe DSC",
            noteApiFcl.Count == 6
            && noteApiFcl.Where(n => n.ContCreditId == cont707Fcl.ID).Sum(n => n.Valoare) == 140m
            && noteApiFcl.Where(n => n.ContCreditId == tipServiciuFcl.ContImplicitId).Sum(n => n.Valoare) == 100m
            && noteApiFcl.Where(n => n.ContCreditId == cont4427Fcl.ID).Sum(n => n.Valoare) == 50.4m
            && noteApiFcl.All(n => n.ContDebitId == cont4111Fcl.ID));

        // --- Gardienii de scriere, prin contract ---
        CheckRefuza("Apply peste FCL Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
            () => FacturaIesireApply.Aplica(os, idFcl, writeFcl));
        CheckRefuza("Sterge peste FCL Operat → același refuz de domeniu",
            () => FacturaIesireApply.Sterge(os, idFcl));

        // --- Lista ---
        var listaFcl = FacturaIesireApply.Lista(os).Where(x => x.Id == idFcl).ToList();
        Check("Lista FCL → un rând, cu Stare ca text (CASE în SQL), emitent/client, scadență și Total BRUT din agregat",
            listaFcl.Count == 1 && listaFcl[0].Stare == "Operat"
            && listaFcl[0].Numar == citFcl.Numar
            && listaFcl[0].PredatorDenumire == sediuFcl.Denumire
            && listaFcl[0].PrimitorDenumire == clientFcl.Denumire
            && listaFcl[0].DataScadenta == citFcl.DataScadenta && listaFcl[0].Total == 290.4m);
        Check("Lista FCL → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take)",
            FacturaIesireApply.Lista(os).Where(x => x.Stare == "Operat")
                .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

        // --- Sterge pe draft (documentul + liniile; loturile REFERITE supraviețuiesc) ---
        var idFclDraft = FacturaIesireApply.Aplica(os, null, new FacturaIesireWriteDto {
            Data = new DateOnly(2026, 5, 12), PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
            GestiuneDescarcareId = gestiuneFcl.ID,
            Linii = { new FacturaIesireLinieWriteDto {
                TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID, LotId = lotVechiFcl.ID,
                Descriere = "De șters", Cantitate = 1m, PretUnitar = 10m, TipTvaId = n21Fcl.ID } }
        });
        FacturaIesireApply.Sterge(os, idFclDraft);
        Check("Sterge pe draft: documentul și liniile dispar, dar LOTUL pin rămâne (FCL îl referă, nu-l naște)",
            FacturaIesireApply.Citeste(os, idFclDraft) == null
            && !os.GetObjectsQuery<FacturaIesireDetaliu>().Any(l => l.DocumentId == idFclDraft)
            && os.GetObjectByKey<Lot>(lotVechiFcl.ID) != null);

        // ═══ Pasul 2 al feliei: descărcarea de gestiune prin API (F4-D2/D3/D4) ═══
        // Fluxul de VÂNZARE continuă de unde s-a oprit culegerea: descărcarea
        // conexă, generată de motor în tranzacția operării facturii, se CITEȘTE
        // prin felia ei (F4-D2: citire + comenzi, fără agregat de scriere), se
        // OPEREAZĂ prin comenzile generice, iar pozițiile fără stoc la facturare
        // (backorder) trec prin comanda manuală de generare (F4-D3) și prin
        // proiecția de rest (F4-D4).
        var cont607Fcl = os.FirstOrDefault<Cont>(c => c.Simbol == "607");

        var idDsc = citFcl.Copii.Single().Id;
        var citDsc = DscApply.Citeste(os, idDsc);
        Check("DscApply.Citeste → header: draft AUTOGENERAT legat de factura-sursă (cu numărul ei, pentru link-ul «Generat din»), "
            + "predator = GESTIUNEA de descărcare, primitor = clientul (laturile se ÎNLOCUIESC, nu se inversează)",
            citDsc != null && citDsc.Id == idDsc && citDsc.Stare == "Draft"
            && citDsc.Numar == null && citDsc.Data == new DateOnly(2026, 5, 10)
            && citDsc.Autogenerat && citDsc.DocumentSursaId == idFcl
            && citDsc.DocumentSursaNumar == citFcl.Numar && citDsc.DocumentSursaTip == "FCL"
            && citDsc.PredatorId == gestiuneFcl.ID && citDsc.PredatorDenumire == gestiuneFcl.Denumire
            && citDsc.PrimitorId == clientFcl.ID && citDsc.PrimitorDenumire == clientFcl.Denumire);
        Check("DSC prin API = CITIRE + comenzi (F4-D2): nicio affordance de editare, dar operarea e disponibilă pe draft",
            !citDsc.PoateEdita && citDsc.PoateOpera && !citDsc.PoateAnula && !citDsc.PoateStorna);

        var dPin = citDsc.Linii.Single(l => l.LinieSursaId == lPin.Id);
        var dFifo = citDsc.Linii.Single(l => l.LinieSursaId == lFifo.Id);
        Check("PROBA descărcării: PINUL respectat (3 buc din lotul pin, la 6 lei) și FIFO pe restul (4 buc din lotul VECHI, la 5) — "
            + "valoarea e COSTUL lotului, decuplat de prețul de vânzare (20)",
            citDsc.Linii.Count == 2
            && dPin.LotId == lotNouFcl.ID && dPin.Cantitate == 3m && dPin.Valoare == 18m
            && dFifo.LotId == lotVechiFcl.ID && dFifo.Cantitate == 4m && dFifo.Valoare == 20m
            && citDsc.Total == 38m);
        Check("Linia de descărcare: produsul vine PRIN LOT (frunza DSC n-are ProdusId), cu eticheta lotului; "
            + "linia de SERVICIU a facturii nu produce descărcare",
            dPin.ProdusId == produsFcl.ID && dPin.ProdusDenumire == produsFcl.Denumire
            && dPin.LotEticheta == lotNouFcl.Eticheta
            && dPin.TipMaterialId == tipMarfa.ID && dPin.TipMaterialCod == tipMarfa.Cod
            && citDsc.Linii.All(l => l.LinieSursaId != lServ.Id));
        Check("Dimensiunea frunzei (DIM-2) e CLONATĂ de pe linia FCL sursă: CodEconomic pe linia pin, absent pe cealaltă",
            dPin.CodEconomicId == codEcFcl.ID && dPin.CodEconomicCod == codEcFcl.Cod
            && dFifo.CodEconomicId == null && dFifo.CodEconomicCod == null);

        // --- Comenzile generice pe DSC (nimic nou în motor) ---
        Check("Dry-run pe descărcarea draft → listă goală (aceiași gardieni ca la operare)",
            DryRunFcl(idDsc).Count == 0);
        var rezDsc = OperareApi.Opereaza(os, idDsc);
        Check("OperareApi pe DSC → Operat, FĂRĂ conex (descărcarea e frunza lanțului conex)",
            rezDsc.StareNoua == StareDocument.Operat && rezDsc.ConexId == null && rezDsc.Mesaje.Count == 0);
        citDsc = DscApply.Citeste(os, idDsc);
        Check("Citeste după operare: numărul din seria proprie (DSC-), affordances inversate",
            citDsc.Stare == "Operat" && citDsc.Numar?.StartsWith("DSC-") == true
            && citDsc.DataOperare != null
            && !citDsc.PoateEdita && !citDsc.PoateOpera && citDsc.PoateAnula && citDsc.PoateStorna);

        var noteDscApi = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idDsc).ToList();
        Check("Costul se postează pe DSC, decuplat de vânzare: 607 = 371 la 38 (18+20), în timp ce factura a postat 140 pe 707",
            noteDscApi.Count == 2 && noteDscApi.Sum(n => n.Valoare) == 38m
            && noteDscApi.All(n => n.ContDebitId == cont607Fcl.ID
                && n.ContCreditId == tipMarfa.ContImplicitId));
        var stocDscApi = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idDsc).ToList();
        Check("Operarea DSC scoate marfa din gestiune: −3 pe lotul pin, −4 pe lotul vechi, Marfuri, pe gestiunea de descărcare",
            stocDscApi.Count == 2
            && stocDscApi.Single(r => r.LotId == lotNouFcl.ID).Cantitate == -3m
            && stocDscApi.Single(r => r.LotId == lotVechiFcl.ID).Cantitate == -4m
            && stocDscApi.All(r => r.TipStoc == TipStoc.Marfuri && r.RepartitorId == gestiuneFcl.ID));

        citFcl = FacturaIesireApply.Citeste(os, idFcl);
        Check("Affordance ONESTĂ pe grupul conex: cu descărcarea OPERATĂ, factura nu mai poate fi anulată/stornată",
            !citFcl.PoateAnula && !citFcl.PoateStorna
            && citFcl.Copii.Single() is { Tip: "DSC", Stare: "Operat" });
        var restFcl1 = FacturaIesireApply.RestNedescarcat(os, idFcl);
        Check("Factură acoperită integral: rest zero pe AMBELE linii de stoc (rândurile acoperite rămân în proiecție — "
            + "tabelul arată starea acoperirii, nu doar lipsa), iar PoateGeneraDescarcare e fals",
            restFcl1.Count == 2 && restFcl1.All(r => r.Rest == 0m && r.Acoperit == r.Cantitate)
            && !citFcl.PoateGeneraDescarcare);
        var genFaraRest = FacturaIesireApply.GenereazaDescarcare(os, idFcl, new DateOnly(2026, 5, 15));
        Check("GenereazaDescarcare pe o factură fără rest → DscId null (nimic de generat NU e eroare)",
            genFaraRest.DscId == null && genFaraRest.Resturi.Count == 0);

        // --- Backorder (F4-D3/D4): factura cere mai mult decât soldul disponibil ---
        // Rămas în gestiune după descărcarea de mai sus: 6 buc lot vechi + 7 buc
        // lot pin = 13. Factura cere 20 ⇒ 7 rămân backorder (venitul se postează
        // acum, costul la disponibilitate — fluxul-ancoră al magazinului, 37).
        var idFcl2 = FacturaIesireApply.Aplica(os, null, new FacturaIesireWriteDto {
            Data = new DateOnly(2026, 5, 20),
            PredatorId = sediuFcl.ID, PrimitorId = clientFcl.ID,
            GestiuneDescarcareId = gestiuneFcl.ID,
            Linii = { new FacturaIesireLinieWriteDto {
                TipMaterialId = tipMarfa.ID, ProdusId = produsFcl.ID,
                Descriere = "Comandă parțial acoperită", Cantitate = 20m, PretUnitar = 20m,
                TipTvaId = n21Fcl.ID } }
        });
        CheckRefuza("GenereazaDescarcare pe FCL DRAFT → refuz de DOMENIU (pe draft nu există încă acoperire de generat)",
            () => FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20)));

        var rezFcl2 = OperareApi.Opereaza(os, idFcl2);
        var idDsc2 = rezFcl2.ConexId.Value;
        var citDsc2 = DscApply.Citeste(os, idDsc2);
        Check("Backorder: descărcarea conexă alocă DOAR disponibilul (6 din lotul vechi + 7 din lotul pin = 13); "
            + "generatorul nu aruncă niciodată la lipsă de stoc",
            citDsc2.Linii.Count == 2 && citDsc2.Linii.Sum(l => l.Cantitate) == 13m
            && citDsc2.Linii.Single(l => l.LotId == lotVechiFcl.ID).Cantitate == 6m
            && citDsc2.Linii.Single(l => l.LotId == lotNouFcl.ID).Cantitate == 7m);

        var restFcl2 = FacturaIesireApply.RestNedescarcat(os, idFcl2);
        Check("RestNedescarcat (F4-D4): un rând per linie de stoc, cu produsul DENUMIT server-side — 20 cerute / 13 acoperite / 7 rest",
            restFcl2.Count == 1 && restFcl2[0].ProdusId == produsFcl.ID
            && restFcl2[0].ProdusDenumire == produsFcl.Denumire && restFcl2[0].LotId == null
            && restFcl2[0].Cantitate == 20m && restFcl2[0].Acoperit == 13m && restFcl2[0].Rest == 7m);
        var proiectieFcl2 = DescarcareService.RestNedescarcat(os, os.GetObjectByKey<FacturaIesire>(idFcl2));
        Check("Consistență (42c): DTO-ul de rest == proiecția `DescarcareService.RestNedescarcat` per linie",
            proiectieFcl2.Count == restFcl2.Count && proiectieFcl2.All(p => {
                var r = restFcl2.Single(x => x.LinieId == p.LinieId);
                return r.Cantitate == p.Cantitate && r.Acoperit == p.Acoperit
                    && r.Rest == p.RestNeacoperit && r.ProdusId == p.ProdusId && r.LotId == p.LotId;
            }));
        Check("PoateGeneraDescarcare (affordance server-side, F4-D4): adevărat pe factura operată, cu gestiune și rest > 0",
            FacturaIesireApply.Citeste(os, idFcl2).PoateGeneraDescarcare);

        // Descărcarea parțială se operează (draftul nu rezervă stoc — gardianul de
        // sold rămâne autoritatea), apoi comanda manuală se lovește de lipsă.
        OperareApi.Opereaza(os, idDsc2);
        var genFaraStoc = FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20));
        Check("GenereazaDescarcare fără sold disponibil → DscId null, dar restul se RAPORTEAZĂ (7 buc așteaptă marfă)",
            genFaraStoc.DscId == null && genFaraStoc.Resturi.Count == 1 && genFaraStoc.Resturi[0].Rest == 7m);

        // Suplimentarea stocului (în realitate: recepția FCT→NIR) — aceeași rețetă
        // de sold de deschidere ca la începutul blocului.
        var lotSuplFcl = DeschidereFcl(8m, 7m, new DateOnly(2026, 5, 18));
        os.CommitChanges();

        var genBackorder = FacturaIesireApply.GenereazaDescarcare(os, idFcl2, new DateOnly(2026, 5, 20));
        Check("Comanda manuală de generare (F4-D3), după suplimentarea stocului: al DOILEA DSC, exact restul, rest zero după el",
            genBackorder.DscId != null && genBackorder.Resturi.Count == 0);
        var citDsc3 = DscApply.Citeste(os, genBackorder.DscId.Value);
        Check("DSC₂ (backorder) — autogenerat pe factura-sursă, la data comenzii, cu linia-sursă păstrată și costul lotului nou (7 × 8)",
            citDsc3.Autogenerat && citDsc3.DocumentSursaId == idFcl2
            && citDsc3.Data == new DateOnly(2026, 5, 20) && citDsc3.Stare == "Draft"
            && citDsc3.Linii.Count == 1
            && citDsc3.Linii[0] is { Cantitate: 7m, Valoare: 56m }
            && citDsc3.Linii[0].LotId == lotSuplFcl.ID
            && citDsc3.Linii[0].LinieSursaId == restFcl2[0].LinieId);

        var citFcl2 = FacturaIesireApply.Citeste(os, idFcl2);
        Check("Grupul conex al facturii cu backorder: DOUĂ descărcări (una operată, una draft) — și PoateGeneraDescarcare a redevenit fals",
            citFcl2.Copii.Count == 2 && citFcl2.Copii.All(c => c.Tip == "DSC" && c.Autogenerat)
            && citFcl2.Copii.Count(c => c.Stare == "Operat") == 1
            && !citFcl2.PoateGeneraDescarcare
            && FacturaIesireApply.RestNedescarcat(os, idFcl2).Single().Rest == 0m);

        // --- Plafonul de acoperire per linie-sursă (review advers F4/D2) ---
        // Operat pe linia fcl2: 13 (DSC-ul conex); draftul de backorder (7) NU
        // intră în plafon — drafturile nu postează nimic, anti-dublarea lor e a
        // GENERATORULUI (RestNedescarcat le numără). Un DSC MANUAL cu încă 8 pe
        // aceeași linie-sursă ar duce materializarea la 21 din 20 facturate →
        // refuz zgomotos la operare (închide și dublura de generare concurentă:
        // primul document operat câștigă, al doilea pică aici).
        var dscManualFcl = os.CreateObject<DescarcareGestiune>();
        dscManualFcl.Data = new DateOnly(2026, 5, 21);
        dscManualFcl.PredatorId = gestiuneFcl.ID;
        dscManualFcl.PrimitorId = clientFcl.ID;
        dscManualFcl.DocumentSursa = os.GetObjectByKey<FacturaIesire>(idFcl2);
        var linieManualFcl = os.CreateObject<DescarcareGestiuneDetaliu>();
        linieManualFcl.Document = dscManualFcl;
        linieManualFcl.TipMaterialId = tipMarfa.ID;
        linieManualFcl.LotId = lotSuplFcl.ID;
        linieManualFcl.LinieSursaId = restFcl2[0].LinieId;
        linieManualFcl.Cantitate = 8m;
        os.CommitChanges();
        CheckRefuza("Plafonul de acoperire per linie-sursă (F4/D2): descărcarea MANUALĂ suprapusă "
            + "(8 peste cei 13 operați, din 20 facturate) → refuz la operare",
            () => OperareApi.Opereaza(os, dscManualFcl.ID));
        os.Delete(linieManualFcl);
        os.Delete(dscManualFcl);
        os.CommitChanges();

        // --- Lista DSC ---
        var listaDsc = DscApply.Lista(os).Where(x => x.Id == idDsc).ToList();
        Check("Lista DSC → un rând, cu Stare ca text (CASE în SQL), gestiune/client, marcajul «autogenerat» și costul din agregat",
            listaDsc.Count == 1 && listaDsc[0].Stare == "Operat" && listaDsc[0].Numar == citDsc.Numar
            && listaDsc[0].PredatorDenumire == gestiuneFcl.Denumire
            && listaDsc[0].PrimitorDenumire == clientFcl.Denumire
            && listaDsc[0].Autogenerat && listaDsc[0].Total == 38m);
        Check("Lista DSC → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take); "
            + "Citeste pe un id care nu e descărcare → null, nu excepție",
            DscApply.Lista(os).Where(x => x.Stare == "Operat")
                .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1
            && DscApply.Citeste(os, idFcl) == null);

        // --- Lanțul de anulare/storno pe grup ---
        OperareApi.AnuleazaOperarea(os, idDsc);
        Check("Anularea descărcării o readuce pe Draft, îi șterge rândurile de stoc și ELIBEREAZĂ factura "
            + "(PoateAnula/PoateStorna redevin adevărate — draftul continuă să acopere)",
            DscApply.Citeste(os, idDsc).Stare == "Draft"
            && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idDsc)
            && FacturaIesireApply.Citeste(os, idFcl) is { PoateAnula: true, PoateStorna: true }
            && FacturaIesireApply.RestNedescarcat(os, idFcl).All(r => r.Rest == 0m));
        var rezStornoDsc2 = OperareApi.Storneaza(os, idDsc2, new DateOnly(2026, 5, 25));
        Check("Storno pe descărcarea operată: Stornat — iar acoperirea se REDESCHIDE (stornatul nu acoperă, draftul da): "
            + "restul urcă de la 0 la 13 și PoateGeneraDescarcare redevine adevărat",
            rezStornoDsc2.StareNoua == StareDocument.Stornat
            && DscApply.Citeste(os, idDsc2) is { Stare: "Stornat", PoateAnula: false, PoateStorna: false }
            && FacturaIesireApply.RestNedescarcat(os, idFcl2).Single().Rest == 13m
            && FacturaIesireApply.Citeste(os, idFcl2).PoateGeneraDescarcare);

        CurataApiFcl(os);
        Check("Curățenie finală felia Api FCL + DSC (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiFcl))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiFcl))
            && !os.GetObjectsQuery<FacturaIesire>().Any(d => d.ID == idFcl || d.ID == idFcl2)
            && !os.GetObjectsQuery<DescarcareGestiune>().Any(d => d.ID == idDsc || d.ID == idDsc2));
    }

    // Felia 9 rulează pe AMBELE profiluri: proiecția e agnostică la plan (nu
    // cunoaște niciun simbol), dar tocmai de asta merită probată pe amândouă —
    // datele preexistente ale bazei diferă, iar invarianții globali (partidă
    // dublă, continuitate) se verifică peste ELE, nu doar peste scenariul propriu.
    VerificaBalanta();
    VerificaFisaJurnal();

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
    // BTR are politică de numerotare (BTR-), deci documentul ăsta e sonda pentru
    // GATE XAF D6: numărul se consumă în faza de MATERIALIZARE, după toți
    // gardienii. Înainte, `AsignaNumar` rula între validare și gardianul de sold —
    // un refuz lăsa numărul pe document ȘI incrementul pe politică în
    // ObjectSpace-ul VIU al apelantului (UI-ul rulează motorul în OS-ul
    // View-ului), iar orice Save ulterior le persista: gol în seria fiscală.
    var politicaBtr = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
    var numarInainte = politicaBtr.UrmatorulNumar;
    var insuficient = Transfer(mag2, mag1, 100m, new DateOnly(2026, 3, 10));
    CheckRefuza("Sold insuficient → operare refuzată", () => MotorOperare.Opereaza(os, insuficient));
    Check("Refuzul unui gardian NU consumă număr (D6): document fără Numar, politica neatinsă",
        string.IsNullOrWhiteSpace(insuficient.Numar) && politicaBtr.UrmatorulNumar == numarInainte);
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

// =============== Scenariul e2e pasul 5 / spike 1: felia BTR (D1/D8/D9) ===============
// Același obiect de studiu ca 3b (transferul), dar parcurs prin CONTRACTUL
// feliei, nu prin entități: WriteDto → `NotaTransferApply.Aplica` → `Citeste` /
// `Lista` → dry-run `OperareApi.Valideaza` → comenzile `OperareApi`. Endpoint-urile
// din host sunt transport peste EXACT acest cod (D1: DTO-uri + Apply în Module,
// fără ASP.NET), deci ce e verde aici e verde și pe sârmă — controllerul nu mai
// poate ascunde o regulă.
//
// Reciclează marcajele lui 3b (produs `E2E-PRB`, `NumarPV = "E2E"`), deci `Curata`
// de mai sus acoperă și reziduurile acestui bloc.
using (var os = provider.CreateObjectSpace()) {
    Curata(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var mag2 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
    var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajProdus;
    produs.Denumire = "Produs probă felia BTR";
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

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU, aruncat după apel: `Valideaza`
    // rulează `PregatesteOperare`, care SCRIE pe linii (contractul lui
    // MotorOperare.Valideaza). Pe calea vie, OS-ul ăsta e cel non-secured al
    // endpoint-ului `POST .../valideaza`.
    IReadOnlyList<string> DryRun(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }

    // --- Apply: creare din WriteDto (fără Stare/Numar/Valoare — server-owned) ---
    var write = new NotaTransferWriteDto {
        Data = new DateOnly(2026, 3, 5),
        PredatorId = mag1.ID,
        PrimitorId = mag2.ID,
        NumarPV = "E2E",
        DataPV = new DateOnly(2026, 3, 4),
        Linii = { new NotaTransferLinieWriteDto { TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 4m } }
    };
    var idBtr = NotaTransferApply.Aplica(os, null, write);
    var citit = NotaTransferApply.Citeste(os, idBtr);
    Check("Apply creare → header proiectat plat (laturi cu denumiri, PV, sursă) + affordances de Draft",
        citit != null && citit.Id == idBtr && citit.Stare == "Draft" && citit.Numar == null
        && citit.Data == new DateOnly(2026, 3, 5) && citit.DataPV == new DateOnly(2026, 3, 4)
        && citit.NumarPV == "E2E"
        && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
        && citit.PrimitorId == mag2.ID && citit.PrimitorDenumire == mag2.Denumire
        && !citit.Autogenerat && citit.DocumentSursaId == null
        && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);
    Check("Apply creare → o linie, cu eticheta lotului identică celei din model (oglinda lui Lot.Eticheta)",
        citit.Linii.Count == 1 && citit.Linii[0].LotId == lot.ID
        && citit.Linii[0].LotEticheta == lot.Eticheta
        && citit.Linii[0].TipMaterialCod == tipMaterial.Cod
        && citit.Linii[0].Cantitate == 4m);
    Check("Apply NU scrie `Valoare` pe linie (o materializează motorul la operare)",
        citit.Linii[0].Valoare == 0m && citit.Total == 0m);

    // --- Apply: reconcilierea colecției (update + insert, apoi delete) ---
    var idLinie = citit.Linii[0].Id;
    write.Linii[0].Id = idLinie;
    write.Linii[0].Cantitate = 3m;
    write.Linii.Add(new NotaTransferLinieWriteDto { TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 1m });
    NotaTransferApply.Aplica(os, idBtr, write);
    citit = NotaTransferApply.Citeste(os, idBtr);
    Check("Apply update → linia cu Id se actualizează, linia fără Id se adaugă",
        citit.Linii.Count == 2
        && citit.Linii.Single(l => l.Id == idLinie).Cantitate == 3m
        && citit.Linii.Sum(l => l.Cantitate) == 4m);

    write.Linii.RemoveAt(1);
    write.Linii[0].Cantitate = 4m;
    NotaTransferApply.Aplica(os, idBtr, write);
    citit = NotaTransferApply.Citeste(os, idBtr);
    Check("Apply → linia absentă din payload se ȘTERGE (reconciliere server-side, nu CRUD per linie)",
        citit.Linii.Count == 1 && citit.Linii[0].Id == idLinie && citit.Linii[0].Cantitate == 4m
        && os.GetObjectsQuery<DocumentDetaliu>().Count(l => l.DocumentId == idBtr) == 1);

    CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        NotaTransferApply.Aplica(os, idBtr, new NotaTransferWriteDto {
            Data = write.Data, PredatorId = mag1.ID, PrimitorId = mag2.ID, NumarPV = "E2E",
            Linii = { new NotaTransferLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipMaterial.ID, LotId = lot.ID, Cantitate = 1m } }
        }));

    // --- Dry-run: valid, apoi stricat deliberat ---
    var politicaBtrApi = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
    var numarInainteApi = politicaBtrApi.UrmatorulNumar;

    Check("Dry-run (Valideaza) pe draft valid → listă goală", DryRun(idBtr).Count == 0);

    write.PrimitorId = mag1.ID; // aceeași gestiune pe ambele laturi
    NotaTransferApply.Aplica(os, idBtr, write);
    var eroriDry = DryRun(idBtr);
    Check("Dry-run pe draft stricat → eroarea de domeniu a tipului, ca DATE",
        eroriDry.Count > 0 && eroriDry.Any(e => e.Contains("difere")));

    // Proba că dry-run-ul e chiar DRY: nimic materializat, nici măcar numărul
    // (care în `Opereaza` se consumă abia în faza de materializare — GATE XAF D6).
    using (var osVerif = provider.CreateObjectSpace()) {
        var docVerif = osVerif.GetObjectByKey<NotaTransfer>(idBtr);
        var polVerif = osVerif.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BTR");
        Check("Dry-run NU materializează nimic: zero rânduri de registru, număr neconsumat, stare Draft",
            !osVerif.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idBtr)
            && !osVerif.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idBtr)
            && string.IsNullOrWhiteSpace(docVerif.Numar)
            && docVerif.Stare == StareDocument.Draft
            && polVerif.UrmatorulNumar == numarInainteApi);
    }

    write.PrimitorId = mag2.ID;
    NotaTransferApply.Aplica(os, idBtr, write);
    Check("Dry-run pe draftul reparat → din nou listă goală", DryRun(idBtr).Count == 0);

    // --- Comenzile prin adaptor: rezultatul e DATE, nu entitate ---
    var rezultatOperare = OperareApi.Opereaza(os, idBtr);
    Check("OperareApi.Opereaza → OperareRezultat cu StareNoua=Operat, fără conex (BTR n-are politică)",
        rezultatOperare.DocumentId == idBtr && rezultatOperare.StareNoua == StareDocument.Operat
        && rezultatOperare.ConexId == null && rezultatOperare.Mesaje.Count == 0);
    Check("OperareRezultatDto → starea traversează sârma ca TEXT",
        OperareRezultatDto.Din(rezultatOperare).StareNoua == "Operat");

    citit = NotaTransferApply.Citeste(os, idBtr);
    Check("Citeste după operare → Numar din politică, Total și Valoare materializate de motor, affordances inversate",
        citit.Stare == "Operat" && citit.Numar?.StartsWith("BTR-") == true && citit.DataOperare != null
        && citit.Total == 40m && citit.Linii[0].Valoare == 40m
        && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna);

    CheckRefuza("Apply peste un document Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => NotaTransferApply.Aplica(os, idBtr, write));

    // --- Lista: proiecție IQueryable, traductibilă integral în SQL ---
    var randuriLista = NotaTransferApply.Lista(os).Where(x => x.Id == idBtr).ToList();
    Check("Lista → un rând, cu Stare ca text (CASE în SQL) și Total din agregatul liniilor",
        randuriLista.Count == 1 && randuriLista[0].Stare == "Operat" && randuriLista[0].Total == 40m
        && randuriLista[0].Numar == citit.Numar
        && randuriLista[0].PredatorDenumire == mag1.Denumire
        && randuriLista[0].PrimitorDenumire == mag2.Denumire);

    var idGol = NotaTransferApply.Aplica(os, null, new NotaTransferWriteDto {
        Data = new DateOnly(2026, 3, 6), PredatorId = mag1.ID, PrimitorId = mag2.ID, NumarPV = "E2E"
    });
    Check("Lista → draftul FĂRĂ linii apare cu Total 0 (LEFT JOIN pe agregat, nu subquery corelat)",
        NotaTransferApply.Lista(os).Any(x => x.Id == idGol && x.Total == 0m));
    Check("Lista → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: sort + take)",
        NotaTransferApply.Lista(os).Where(x => x.Stare == "Draft")
            .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

    // --- D9: proiecția de sold == StocService, per cheie ---
    var proiectie = StocProiectii.SoldStoc(os).Where(r => r.LotId == lot.ID).ToList();
    Check("Proiecția SoldStoc → exact cheile mișcate de scenariu (MAG1 6, MAG2 4), cu valoarea agregată",
        proiectie.Count == 2
        && proiectie.Single(r => r.RepartitorId == mag1.ID) is { Cantitate: 6m, Valoare: 60m, TipStoc: "Magazie" }
        && proiectie.Single(r => r.RepartitorId == mag2.ID) is { Cantitate: 4m, Valoare: 40m, TipStoc: "Magazie" });
    Check("Proiecția poartă etichetele plate ale lotului și ale gestiunii (fără navigație lazy per rând)",
        proiectie.All(r => r.ProdusCod == MarcajProdus && r.ProdusDenumire == produs.Denumire
            && r.ProdusUM == "BUC" && r.LotData == lot.Data && r.LotPretUnitar == 10m)
        && proiectie.Single(r => r.RepartitorId == mag1.ID).GestiuneDenumire == mag1.Denumire);
    Check("D9: proiecția == StocService.Sold pe FIECARE cheie (un al doilea adevăr ar fi un defect)",
        proiectie.All(r => r.Cantitate
            == StocService.Sold(os, new CheieStoc(r.LotId, r.RepartitorId, Enum.Parse<TipStoc>(r.TipStoc)))));

    // --- Anulare → re-operare → storno, tot prin adaptor ---
    var rezultatAnulare = OperareApi.AnuleazaOperarea(os, idBtr);
    Check("OperareApi.AnuleazaOperarea → Draft, registrele proprii șterse, affordances de Draft",
        rezultatAnulare.StareNoua == StareDocument.Draft
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idBtr)
        && NotaTransferApply.Citeste(os, idBtr).PoateEdita);
    OperareApi.Opereaza(os, idBtr);
    var rezultatStorno = OperareApi.Storneaza(os, idBtr, new DateOnly(2026, 7, 22));
    Check("OperareApi.Storneaza → Stornat + rânduri inverse la data cerută; nicio afordanță rămasă",
        rezultatStorno.StareNoua == StareDocument.Stornat
        && os.GetObjectsQuery<RegistruStoc>().Count(r => r.DocumentId == idBtr && r.Storno) == 2
        && os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idBtr && r.Storno)
            .All(r => r.Data == new DateOnly(2026, 7, 22))
        && NotaTransferApply.Citeste(os, idBtr) is
            { Stare: "Stornat", PoateEdita: false, PoateOpera: false, PoateAnula: false, PoateStorna: false });

    Curata(os);
    Check("Curățenie finală felia BTR (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajProdus)
        && !os.GetObjectsQuery<NotaTransfer>().Any(d => d.NumarPV == "E2E"));
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
    linieStoc.CodEconomicId = codEc.ID;
    linieServiciu.CodEconomicId = codEc.ID;
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
        noteFct[0].DimensiuniDebit().CodEconomicId == codEc.ID
        && noteFct[0].DimensiuniDebit().RepartitorId == furnizor.ID
        && noteFct[0].DimensiuniCredit().RepartitorId == mag1.ID);

    Check("Conex generat: NIR draft autogenerat, aceleași laturi",
        conex is NIR { Stare: StareDocument.Draft, Autogenerat: true }
        && conex.DocumentSursaId == fct.ID
        && conex.PredatorId == furnizor.ID && conex.PrimitorId == mag1.ID);
    Check("NIR-ul preia DOAR linia de stoc, cu lot, cantitate, valoare, dimensiuni",
        conex.Detalii.Count == 1 && conex.Detalii[0].TipMaterialId == tipMateriale.ID
        && conex.Detalii[0].LotId == lot.ID && conex.Detalii[0].Cantitate == 5m
        && conex.Detalii[0].Valoare == 59.5m && conex.Detalii[0].DimensiuniCulese().CodEconomicId == codEc.ID);

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
        noteNir[0].DimensiuniDebit().MaterialId == produs.ID
        && noteNir[0].DimensiuniCredit().MaterialId == produs.ID);
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
    linie2.SursaFinantareId = sursaFin.ID;
    linie2.CodFunctionalId = codFn.ID;
    linie2.ProiectId = proiect.ID;
    os.CommitChanges();
    Check("FCT doar cu servicii NU generează NIR", MotorOperare.Opereaza(os, fct2) == null);
    var nota404 = os.GetObjectsQuery<RegistruContabil>().Single(r => r.DocumentId == fct2.ID);
    Check("Creditul vine din ContImplicit al partenerului (404, nu fallback 401)",
        nota404.ContCreditId == cont404.ID);
    Check("Puntea angajamentului: E pe 404 satisfăcut fără cod economic; B/F/P rezolvate pe notă",
        nota404.DimensiuniCredit().CodEconomicId == null
        && nota404.DimensiuniCredit().SursaFinantareId == sursaFin.ID
        && nota404.DimensiuniCredit().CodFunctionalId == codFn.ID
        && nota404.DimensiuniCredit().ProiectId == proiect.ID);
    MotorOperare.Storneaza(os, fct2, new DateOnly(2026, 7, 22));

    CurataFct(os);
    Check("Curățenie finală FCT/NIR (fără reziduuri e2e)",
        !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-FF"))
        && !os.GetObjectsQuery<Partener>().Any(p => p.Cod.StartsWith("E2E-FURN")));
}

// ============ Scenariul e2e pasul 5 / felia 2: Api FCT + NIR (F2-D6) ============
// Același lanț ca blocul 3c de mai sus (FCT → NIR conex → registre), dar parcurs
// prin CONTRACTUL feliei: WriteDto → `FacturaIntrareApply.Aplica` → `Citeste` /
// `Lista` → dry-run → comenzile `OperareApi` → `NirApply`. Endpoint-urile din host
// sunt transport peste EXACT acest cod, deci ce e verde aici e verde și pe sârmă.
//
// Ce exersează în plus față de blocul 3c (și de felia BTR):
//   * LOTUL SE NAȘTE LA `Aplica`, din `ProdusId` — blocurile vechi îl creau cu
//     `CreeazaLot` MANUAL, pentru că pe calea lor nu există nici ViewController,
//     nici seam de culegere. Aici se probează chiar seam-ul (F2-D1);
//   * TVA-ul materializat LA CULEGERE (`Valoare`/`ValoareTva` înainte de operare)
//     + default-ul `TipTvaImplicit` + override-ul manual;
//   * dimensiunile frunzei culese din DTO și clonate pe NIR prin contract;
//   * numărul CULES (FCT n-are politică de numerotare — invers față de BTR/NIR);
//   * `Citeste.Copii` = link-ul UI spre NIR-ul generat.
const string MarcajApiFct = "E2E-API-FCT";

void CurataApiFct(IObjectSpace os) {
    var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFct)).Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)).ToList());
    foreach (var fct in os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Numar.StartsWith("E2E-AF")).ToList()) {
        foreach (var copil in os.GetObjectsQuery<Document>().Where(x => x.DocumentSursaId == fct.ID).ToList()) {
            os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == copil.ID).ToList());
            os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == copil.ID).ToList());
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
            os.Delete(copil);
        }
        os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == fct.ID).ToList());
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == fct.ID).ToList());
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == fct.ID).ToList());
        os.Delete(fct);
    }
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiFct)).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiFct)).ToList());
    os.Delete(os.GetObjectsQuery<Partener>().Where(p => p.Cod == "E2E-AFURN").ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-AFCE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiFct(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    // Profilul BUGETAR are DOAR regimuri Capitalizat (CAP21/CAP19/CAP11/CAP0) —
    // nu se inventează tipuri noi în seed pentru probe (decizia 21). Consecința e
    // notată la proba de override, singura care ar cere un regim cu TVA separat.
    var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
    var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");

    var furnizor = os.CreateObject<Partener>();
    furnizor.Cod = "E2E-AFURN";
    furnizor.Denumire = "Furnizor probă felia Api FCT";
    furnizor.CodFiscal = "RO12345678";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = "E2E-AFCE";
    codEc.Denumire = "Cod economic probă felia Api FCT";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajApiFct + "-A";
    produs.Denumire = "Produs A probă felia Api FCT";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    var produsB = os.CreateObject<Produs>();
    produsB.Cod = MarcajApiFct + "-B";
    produsB.Denumire = "Produs B probă felia Api FCT";
    produsB.UM = "BUC";
    produsB.TipMaterial = tipMateriale;
    os.CommitChanges();

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
    // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
    IReadOnlyList<string> DryRunFct(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }

    // --- Apply: creare din WriteDto (fără LotId/Valoare — server-owned) ---
    var write = new FacturaIntrareWriteDto {
        Numar = "E2E-AF1",
        Data = new DateOnly(2026, 3, 7),
        PredatorId = furnizor.ID,
        PrimitorId = mag1.ID,
        DataScadenta = new DateOnly(2026, 4, 6),
        NumarPV = "PV-AF1",
        DataPV = new DateOnly(2026, 3, 6),
        CodCpv = "03000000-1",
        Valuta = "RON",
        Curs = 1m,
        Linii = {
            new FacturaIntrareLinieWriteDto {
                TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                Cantitate = 5m, PretUnitar = 10m, TipTvaId = cap19.ID,
                LotFabricatie = "LOT-API", DataExpirare = new DateOnly(2027, 1, 1),
                CodCpv = "03000000-1", CodEconomicId = codEc.ID
            },
            // Fără TipTva în payload ⇒ default-ul tipului de document (CAP21).
            new FacturaIntrareLinieWriteDto {
                TipMaterialId = tipServicii.ID,
                Cantitate = 1m, PretUnitar = 100m, CodEconomicId = codEc.ID
            }
        }
    };
    var idFct = FacturaIntrareApply.Aplica(os, null, write);
    var citit = FacturaIntrareApply.Citeste(os, idFct);
    Check("Apply FCT creare → header plat: NUMĂRUL CULES (FCT n-are politică), scadență/PV/valută + CodFiscal-ul furnizorului",
        citit != null && citit.Id == idFct && citit.Stare == "Draft"
        && citit.Numar == "E2E-AF1" && citit.Data == new DateOnly(2026, 3, 7)
        && citit.DataScadenta == new DateOnly(2026, 4, 6) && citit.NumarPV == "PV-AF1"
        && citit.DataPV == new DateOnly(2026, 3, 6) && citit.CodCpv == "03000000-1"
        && citit.Valuta == "RON" && citit.Curs == 1m
        && citit.PredatorId == furnizor.ID && citit.PredatorDenumire == furnizor.Denumire
        && citit.PredatorCodFiscal == "RO12345678"
        && citit.PrimitorId == mag1.ID && citit.PrimitorDenumire == mag1.Denumire
        && !citit.Autogenerat && citit.DocumentSursaId == null && citit.Copii.Count == 0
        && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

    var linieStoc = citit.Linii.Single(l => l.TipMaterialId == tipMateriale.ID);
    var linieServ = citit.Linii.Single(l => l.TipMaterialId == tipServicii.ID);
    var lotNascut = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc.Id);
    Check("PROBA FELIEI: lotul S-A NĂSCUT la Aplica din ProdusId (nu prin CreeazaLot manual) — nefinalizat, în gestiunea primitoare",
        lotNascut != null && lotNascut.ProdusId == produs.ID && lotNascut.GestiuneId == mag1.ID
        && lotNascut.Data == default && lotNascut.PretUnitar == 0m
        && linieStoc.LotId == lotNascut.ID
        && linieStoc.LotEticheta == lotNascut.Eticheta
        && linieStoc.LotEticheta.Contains("(în culegere)"));
    Check("Linia de serviciu NU naște lot (natura Clasei ≠ Stoc)",
        linieServ.LotId == null && linieServ.LotEticheta == null && linieServ.ProdusId == null);

    Check("TVA materializat LA CULEGERE (GATE 53c): stoc CAP19 → 59,5 brut; serviciu → 121; Total brut 180,5",
        linieStoc.Valoare == 59.5m && linieStoc.ValoareTva == 0m
        && linieServ.Valoare == 121m && linieServ.ValoareTva == 0m
        && citit.Total == 180.5m);
    Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (CAP21 pe FCT, seed bugetar); linia cu TipTva cules rămâne CAP19",
        linieServ.TipTvaId == cap21.ID && linieServ.TipTvaCod == "CAP21" && linieServ.TipTvaCota == 21m
        && linieStoc.TipTvaId == cap19.ID && linieStoc.TipTvaCod == "CAP19");
    Check("Linia poartă dimensiunile frunzei (Id + Cod) și atributele de lot, proiectate plat",
        linieStoc.CodEconomicId == codEc.ID && linieStoc.CodEconomicCod == "E2E-AFCE"
        && linieStoc.SursaFinantareId == null && linieStoc.SursaFinantareCod == null
        && linieStoc.CodFunctionalId == null && linieStoc.ProiectId == null
        && linieStoc.LotFabricatie == "LOT-API" && linieStoc.DataExpirare == new DateOnly(2027, 1, 1)
        && linieStoc.CodCpv == "03000000-1"
        && linieStoc.ProdusId == produs.ID && linieStoc.ProdusCod == produs.Cod
        && linieStoc.ProdusDenumire == produs.Denumire
        && linieStoc.Cantitate == 5m && linieStoc.PretUnitar == 10m);

    // --- Override-ul manual de ValoareTva: refuzat pe regimuri fără TVA separat ---
    // Review advers F2-D1/D7: la bugetar toate regimurile sunt Capitalizat
    // (TVA-ul stă în preț) — un override acceptat aici ar fi numărat TVA-ul de
    // două ori în Total și ar fi murit tăcut la operare. Semantica POZITIVĂ a
    // override-ului (păstrare + recalcul condiționat de declanșatori) se probează
    // în blocul privat, pe regimul Normal.
    write.Linii[0].Id = linieStoc.Id;
    write.Linii[0].TipTvaId = linieStoc.TipTvaId;
    write.Linii[1].Id = linieServ.Id;
    // ROUND-TRIP: clientul retrimite agregatul ÎNTREG, inclusiv TipTva-ul primit
    // la citire (pus de default). Pe o linie EXISTENTĂ absența lui nu e „n-am
    // apucat să-l trimit", ci golire deliberată — probată imediat mai jos.
    write.Linii[1].TipTvaId = linieServ.TipTvaId;
    write.Linii[1].ValoareTva = 3.33m;
    CheckRefuza("Override ValoareTva pe regim Capitalizat → refuz (F2-D1: regimul nu poartă TVA separat)",
        () => FacturaIntrareApply.Aplica(os, idFct, write));
    write.Linii[1].ValoareTva = -5m;
    CheckRefuza("Override ValoareTva NEGATIV → refuz (F2-D7)",
        () => FacturaIntrareApply.Aplica(os, idFct, write));
    write.Linii[1].ValoareTva = null;
    FacturaIntrareApply.Aplica(os, idFct, write);

    write.Linii[1].TipTvaId = null;
    FacturaIntrareApply.Aplica(os, idFct, write);
    Check("Pe linia EXISTENTĂ, TipTva absent din payload = GOLIRE deliberată (default-ul NU se re-aplică) → valoarea revine la net 100",
        FacturaIntrareApply.Citeste(os, idFct).Linii.Single(l => l.Id == linieServ.Id)
            is { TipTvaId: null, Valoare: 100m, ValoareTva: 0m });
    write.Linii[1].TipTvaId = cap21.ID;
    FacturaIntrareApply.Aplica(os, idFct, write);

    // --- Reconcilierea colecției × ciclul de viață al lotului în culegere ---
    var idLotNascut = lotNascut.ID;
    write.Linii[0].ProdusId = produsB.ID;
    FacturaIntrareApply.Aplica(os, idFct, write);
    var lotSincronizat = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc.Id);
    Check("Reconciliere: produs reales pe linie → ACELAȘI lot, sincronizat (nu un al doilea lot pentru aceeași linie)",
        lotSincronizat != null && lotSincronizat.ID == idLotNascut && lotSincronizat.ProdusId == produsB.ID
        && os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == linieStoc.Id) == 1);

    write.Linii.RemoveAt(0);
    FacturaIntrareApply.Aplica(os, idFct, write);
    Check("Reconciliere: linia de stoc absentă din payload se ȘTERGE, iar lotul ei în culegere moare odată cu ea",
        !os.GetObjectsQuery<FacturaIntrareDetaliu>().Any(l => l.ID == linieStoc.Id)
        && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotNascut)
        && FacturaIntrareApply.Citeste(os, idFct).Linii.Count == 1);

    write.Linii.Insert(0, new FacturaIntrareLinieWriteDto {
        TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
        Cantitate = 5m, PretUnitar = 10m, TipTvaId = cap19.ID,
        LotFabricatie = "LOT-API", DataExpirare = new DateOnly(2027, 1, 1),
        CodCpv = "03000000-1", CodEconomicId = codEc.ID
    });
    FacturaIntrareApply.Aplica(os, idFct, write);
    citit = FacturaIntrareApply.Citeste(os, idFct);
    var linieStoc2 = citit.Linii.Single(l => l.TipMaterialId == tipMateriale.ID);
    var lotNou = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linieStoc2.Id);
    Check("Linia de stoc re-adăugată (fără Id) e linie NOUĂ și își naște propriul lot",
        lotNou != null && lotNou.ID != idLotNascut && lotNou.ProdusId == produs.ID
        && linieStoc2.Id != linieStoc.Id && linieStoc2.LotId == lotNou.ID);
    write.Linii[0].Id = linieStoc2.Id;

    // --- Refuzuri de contract (mesaj de domeniu, nu excepție de infrastructură) ---
    CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        FacturaIntrareApply.Aplica(os, idFct, new FacturaIntrareWriteDto {
            Numar = "E2E-AF1", Data = write.Data, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { new FacturaIntrareLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 1m } }
        }));
    CheckRefuza("Apply cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
        FacturaIntrareApply.Aplica(os, idFct, new FacturaIntrareWriteDto {
            Numar = "E2E-AF1", Data = write.Data, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { new FacturaIntrareLinieWriteDto {
                TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
        }));
    CheckRefuza("Apply cu furnizor inexistent → refuz cu mesaj de domeniu (nu violare de FK)", () =>
        FacturaIntrareApply.Aplica(os, idFct, new FacturaIntrareWriteDto {
            Numar = "E2E-AF1", Data = write.Data, PredatorId = Guid.NewGuid(), PrimitorId = mag1.ID
        }));
    FacturaIntrareApply.Aplica(os, idFct, write);
    Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact 2 linii",
        FacturaIntrareApply.Citeste(os, idFct).Linii.Count == 2);

    // --- Dry-run, apoi comanda ---
    Check("Dry-run (Valideaza) pe draftul FCT valid → listă goală", DryRunFct(idFct).Count == 0);

    var rezOperare = OperareApi.Opereaza(os, idFct);
    Check("OperareApi.Opereaza pe FCT → Operat + ConexId (NIR-ul generat în aceeași tranzacție), cu mesaj pentru operator",
        rezOperare.StareNoua == StareDocument.Operat && rezOperare.ConexId != null
        && rezOperare.Mesaje.Count == 1);
    var idNir = rezOperare.ConexId.Value;

    var lotFinal = os.GetObjectByKey<Lot>(lotNou.ID);
    Check("Motorul FINALIZEAZĂ lotul născut la culegere: preț 11,9 (59,5/5), data facturii, atributele culese pe linie",
        lotFinal.PretUnitar == 11.9m && lotFinal.Data == new DateOnly(2026, 3, 7)
        && lotFinal.LotFabricatie == "LOT-API" && lotFinal.DataExpirare == new DateOnly(2027, 1, 1));

    citit = FacturaIntrareApply.Citeste(os, idFct);
    Check("Citeste după operare: numărul rămâne AL FURNIZORULUI (nicio serie consumată), affordances inversate",
        citit.Stare == "Operat" && citit.Numar == "E2E-AF1" && citit.DataOperare != null
        && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna);
    Check("Citeste.Copii → NIR-ul conex: codul ancorei TipDocument, starea ca text, marcajul Autogenerat",
        citit.Copii.Count == 1 && citit.Copii[0].Id == idNir && citit.Copii[0].Tip == "NIR"
        && citit.Copii[0].Stare == "Draft" && citit.Copii[0].Autogenerat
        && citit.Copii[0].Numar == null);

    var noteFct = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idFct).ToList();
    Check("FCT postează DOAR linia de serviciu: 628 = 401, 121 (brut capitalizat)",
        noteFct.Count == 1 && noteFct[0].ContDebitId == tipServicii.ContImplicitId
        && noteFct[0].ContCreditId == cont401.ID && noteFct[0].Valoare == 121m);

    // --- NIR: citirea conexului, apoi comanda pe el ---
    var nirDto = NirApply.Citeste(os, idNir);
    Check("NirApply.Citeste pe conex: header cu sursa ETICHETATĂ, fără număr (seria NIR se consumă la propria operare), editabil ca DRAFT (F5-D8b)",
        nirDto != null && nirDto.Stare == "Draft" && nirDto.Numar == null && nirDto.Autogenerat
        && nirDto.DocumentSursaId == idFct && nirDto.DocumentSursaNumar == "E2E-AF1"
        && nirDto.DocumentSursaTip == "FCT"
        && nirDto.PredatorDenumire == furnizor.Denumire && nirDto.PrimitorDenumire == mag1.Denumire
        && nirDto.Total == 59.5m
        // PoateEdita urmează STAREA (F5-D8b): felia 5 a adăugat calea de scriere
        // pe NIR, deci affordance-ul spune ce poate serverul (42e). Inclusiv pe
        // draftul AUTOGENERAT — conexul e proiectat să fie deschis în editare
        // (26d) pentru recepția parțială, iar gardul de lot străin (F5-D3) e ce
        // face editarea lui sigură. Până la felia 5 era fals prin construcție,
        // fiindcă tierul n-avea nicio cale de scriere (F2-D5).
        && nirDto.PoateEdita && nirDto.PoateOpera && !nirDto.PoateAnula && !nirDto.PoateStorna);
    Check("NIR-ul preia DOAR linia de stoc: lotul finalizat (eticheta nu mai spune „în culegere”), valoarea și dimensiunea clonată prin contract",
        nirDto.Linii.Count == 1 && nirDto.Linii[0].LotId == lotFinal.ID
        && nirDto.Linii[0].LotEticheta == lotFinal.Eticheta
        && !nirDto.Linii[0].LotEticheta.Contains("culegere")
        && nirDto.Linii[0].Cantitate == 5m && nirDto.Linii[0].Valoare == 59.5m
        && nirDto.Linii[0].TipTvaCod == "CAP19"
        && nirDto.Linii[0].CodEconomicId == codEc.ID && nirDto.Linii[0].CodEconomicCod == "E2E-AFCE");
    var listaNir = NirApply.Lista(os).Where(x => x.Id == idNir).ToList();
    Check("NirApply.Lista → un rând, cu Stare ca text (CASE în SQL), marcajul Autogenerat și Total din agregat",
        listaNir.Count == 1 && listaNir[0].Stare == "Draft" && listaNir[0].Autogenerat
        && listaNir[0].Total == 59.5m && listaNir[0].PrimitorDenumire == mag1.Denumire);

    var rezNir = OperareApi.Opereaza(os, idNir);
    Check("OperareApi.Opereaza pe NIR → Operat, cu număr din politica proprie (seria NIR-), fără alt conex",
        rezNir.StareNoua == StareDocument.Operat && rezNir.ConexId == null
        && NirApply.Citeste(os, idNir).Numar?.StartsWith("NIR-") == true);
    var stocNir = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idNir).ToList();
    Check("NIR → +5/+59,5 Magazie pe gestiunea primitoare, pe lotul născut la culegerea facturii",
        stocNir.Count == 1 && stocNir[0].TipStoc == TipStoc.Magazie && stocNir[0].RepartitorId == mag1.ID
        && stocNir[0].Cantitate == 5m && stocNir[0].Valoare == 59.5m && stocNir[0].LotId == lotFinal.ID);
    var noteNir = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idNir).ToList();
    Check("NIR contează recepția: 302.01.00 = 401, 59,5",
        noteNir.Count == 1 && noteNir[0].ContDebitId == tipMateriale.ContImplicitId
        && noteNir[0].ContCreditId == cont401.ID && noteNir[0].Valoare == 59.5m);

    // --- Gardienii, prin contract ---
    CheckRefuza("Apply peste FCT Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => FacturaIntrareApply.Aplica(os, idFct, write));
    CheckRefuza("Sterge peste FCT Operat → același refuz de domeniu",
        () => FacturaIntrareApply.Sterge(os, idFct));
    CheckRefuza("Anularea FCT cu NIR operat → refuzată (gardianul grupului conex)",
        () => OperareApi.AnuleazaOperarea(os, idFct));

    OperareApi.AnuleazaOperarea(os, idNir);
    Check("NIR anulat: Draft, registrele proprii șterse — dar RĂMÂNE autogenerat",
        NirApply.Citeste(os, idNir) is { Stare: "Draft", Autogenerat: true }
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idNir));
    OperareApi.AnuleazaOperarea(os, idFct);
    Check("Anularea FCT ȘTERGE NIR-ul redevenit draft autogenerat (artefact al operării) — Copii se golește",
        NirApply.Citeste(os, idNir) == null
        && FacturaIntrareApply.Citeste(os, idFct) is { Stare: "Draft", Copii.Count: 0 });

    // --- Lista FCT ---
    var listaFct = FacturaIntrareApply.Lista(os).Where(x => x.Id == idFct).ToList();
    Check("Lista FCT → un rând, cu Stare ca text, furnizor/gestiune, scadență și Total BRUT din agregat",
        listaFct.Count == 1 && listaFct[0].Stare == "Draft" && listaFct[0].Numar == "E2E-AF1"
        && listaFct[0].PredatorDenumire == furnizor.Denumire
        && listaFct[0].PrimitorDenumire == mag1.Denumire
        && listaFct[0].DataScadenta == new DateOnly(2026, 4, 6) && listaFct[0].Total == 180.5m);
    Check("Lista FCT → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru + sort + take)",
        FacturaIntrareApply.Lista(os).Where(x => x.Stare == "Draft")
            .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

    // --- Sterge: lotul în culegere moare, lotul finalizat supraviețuiește ---
    var idFct2 = FacturaIntrareApply.Aplica(os, null, new FacturaIntrareWriteDto {
        Numar = "E2E-AF2", Data = new DateOnly(2026, 3, 8),
        PredatorId = furnizor.ID, PrimitorId = mag1.ID,
        Linii = { new FacturaIntrareLinieWriteDto {
            TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 2m, PretUnitar = 7m, CodEconomicId = codEc.ID } }
    });
    var idLotDraft = FacturaIntrareApply.Citeste(os, idFct2).Linii[0].LotId.Value;
    FacturaIntrareApply.Sterge(os, idFct2);
    Check("Sterge pe draft: documentul, liniile și LOTUL în culegere dispar împreună",
        FacturaIntrareApply.Citeste(os, idFct2) == null
        && !os.GetObjectsQuery<FacturaIntrareDetaliu>().Any(l => l.DocumentId == idFct2)
        && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraft));

    // Review advers F2-D4: lotul FINALIZAT al draftului anulat moare la Sterge
    // DOAR fără nicio urmă (anularea i-a șters rândurile de registru, nicio
    // linie vie nu-l mai referă) — altfel ar fi rămas pe viață în nomenclator.
    // Protecția GATE D1 (loturile cu istorie reală) ține prin gardele de
    // registre/referințe, nu prin refuzul global de dinainte.
    var idLotFinal = lotFinal.ID;
    FacturaIntrareApply.Sterge(os, idFct);
    Check("Sterge pe draftul anulat: lotul FINALIZAT rămas FĂRĂ URME (zero registre, zero referințe vii) moare cu documentul (F2-D4)",
        FacturaIntrareApply.Citeste(os, idFct) == null
        && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotFinal));

    CurataApiFct(os);
    Check("Curățenie finală felia Api FCT (fără reziduuri e2e)",
        !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-AF"))
        && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiFct))
        && !os.GetObjectsQuery<Partener>().Any(p => p.Cod == "E2E-AFURN"));
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
        note[0].DimensiuniDebit().RepartitorId == mag1.ID && note[0].DimensiuniCredit().RepartitorId == loc.ID);

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
    liniePlus.CodEconomicId = codEc.ID;
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
    foreach (var d in fcl.Detalii.OfType<FacturaIesireDetaliu>())
        d.CodEconomicId = codEc.ID;
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
        note.All(n => n.DimensiuniDebit().RepartitorId == sediu.ID
            && n.DimensiuniCredit().RepartitorId == client.ID));

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
    Linie(fcl2, tipServiciiVenit, 1m, 100m, cap19Fcl).CodEconomicId = codEc.ID;
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
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
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
    // Rândul GENERIC al tipului (fără TipMaterial, fără filtru de natură) — de la
    // F7-D6 fiecare tip de trezorerie are DOUĂ rânduri: genericul de mai jos și
    // cel per TipMaterial=VIR (viramentul intern). Căutarea „prima regulă a
    // tipului" ar fi devenit nedeterministă; proba rămâne despre generic.
    var regulaPlt = os.FirstOrDefault<RegulaContare>(
        r => r.TipDocument.Cod == "PLT" && r.TipMaterialId == null);
    Check("Seed PLT: debit RepartitorPrimitor (fallback 401), credit RepartitorPredator fără fallback",
        regulaPlt != null && regulaPlt.SursaContDebit == SursaCont.RepartitorPrimitor
        && regulaPlt.ContDebitId == cont401.ID
        && regulaPlt.SursaContCredit == SursaCont.RepartitorPredator && regulaPlt.ContCreditId == null);
    var regulaInc = os.FirstOrDefault<RegulaContare>(
        r => r.TipDocument.Cod == "INC" && r.TipMaterialId == null);
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
    linieStoc.CodEconomicId = codEc.ID;
    linieStoc.CreeazaLot(os, produs, mag1);
    var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
    linieServiciu.Document = fct;
    linieServiciu.TipMaterial = tipServicii;
    linieServiciu.Cantitate = 1m;
    linieServiciu.PretUnitar = 100m;
    linieServiciu.CodEconomicId = codEc.ID;
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
        && plataAuto.Detalii.All(d => d.DimensiuniCulese().CodEconomicId == codEc.ID && d.LotId == null));

    // --- Operarea plății: contare din laturi + imperecherea automată ---
    Check("Plata autogenerată nu generează alt conex", MotorOperare.Opereaza(os, plataAuto) == null);
    var notePlata = Note(plataAuto);
    Check("Plata contează per linie de defalcare: 401 = 770 (59,5 + 100)",
        notePlata.Count == 2 && notePlata.All(n => n.ContDebitId == cont401.ID && n.ContCreditId == cont770.ID)
        && notePlata.Sum(n => n.Valoare) == 159.5m);
    Check("Plata nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == plataAuto.ID));
    var impAuto = os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID);
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
        os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID).Suma == 159.5m);

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
    linieVenit.CodEconomicId = codEc.ID; // 751 cere E (3d)
    os.CommitChanges();
    MotorOperare.Opereaza(os, fcl);

    var inc = os.CreateObject<Incasare>();
    inc.Data = new DateOnly(2026, 3, 10);
    inc.Predator = casa; // intenționat greșit — plătitorul nu poate fi cont propriu
    inc.Primitor = casa;
    inc.TipInstrument = TipInstrumentPlata.Chitanta;
    var linieInc = os.CreateObject<DocumentTrezorerieDetaliu>();
    linieInc.Document = inc;
    linieInc.TipMaterial = tipTrz;
    CheckRefuza("Laturi greșite + linie fără valoare → refuz", () => MotorOperare.Opereaza(os, inc));
    inc.Predator = client;
    linieInc.Valoare = 119m;
    os.CommitChanges();
    // Casa (531) poartă defalcarea E — INC nu are politică de tip, dar contul
    // cere codul economic pe nota rezolvată (3d).
    CheckRefuza("Încasare fără cod economic (531 cere E) → refuz", () => MotorOperare.Opereaza(os, inc));
    linieInc.CodEconomicId = codEc.ID;
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
    var linieAvans = os.CreateObject<DocumentTrezorerieDetaliu>();
    linieAvans.Document = avans;
    linieAvans.TipMaterial = tipTrz;
    linieAvans.Valoare = 50m;
    os.CommitChanges();
    // 31f închis: obligativitatea clasificației pe liniile de plată = politică.
    CheckRefuza("Plată fără clasificație bugetară (politica PLT) → refuz",
        () => MotorOperare.Opereaza(os, avans));
    linieAvans.CodEconomicId = codEc.ID;
    os.CommitChanges();
    MotorOperare.Opereaza(os, avans);
    var noteAvans = Note(avans);
    Check("Avans operat cu număr din politică", avans.Numar?.StartsWith("PLT-") == true);
    Check("Contare avans: 542.01.00 (ContImplicit angajat) = 531.01.01 (casa), 50",
        noteAvans.Count == 1 && noteAvans[0].ContDebitId == cont542.ID
        && noteAvans[0].ContCreditId == cont531.ID && noteAvans[0].Valoare == 50m);
    Check("Nota avansului: repartitori din laturi (debit←casă, credit←angajat — 00 §5)",
        noteAvans[0].DimensiuniDebit().RepartitorId == casa.ID
        && noteAvans[0].DimensiuniCredit().RepartitorId == angajat.ID);

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

// ============ Felia Api Trz: PLT/INC prin nucleul generic (F3-D1/D5/D9) ============
// Marcaj E2E-API-TRZ. Blocul e2e de deasupra probează MOTORUL pe trezorerie;
// acesta probează CONTRACTUL API-ului peste el — aceleași obiecte de seed
// (CASA/TREZ, Tipul tehnic TRZ), dar cu documentele construite exclusiv din
// WriteDto și citite exclusiv prin proiecții plate. Ce exersează în plus față de
// feliile BTR/FCT:
//   * `Numar` SERVER-OWNED (PLT/INC au PoliticaNumerotare) — nici măcar nu e în
//     WriteDto; apare abia după operare, din serie. Invers față de FCT;
//   * `Valoare` CULEASĂ pe linie (trezoreria n-are `PregatesteOperare`);
//   * nucleul GENERIC pe `T : DocumentTrezorerie` — o singură implementare, două
//     rute, cu filtrarea pe tip făcută de TPT (`Citeste<Plata>` nu vede o
//     încasare);
//   * `TipInstrument` ca STRING pe sârmă, în ambele sensuri (round-trip, CASE în
//     listă, refuz de domeniu la valoare necunoscută);
//   * F3-D5: parametrii plății automate în DTO-urile FCT → plata autogenerată în
//     `Copii[]` → citită pe ruta ei → operată → imperecherea automată.
const string MarcajApiTrz = "E2E-ATRZ";

// Review F3-D5a: `TrezorerieApply.Lista` traduce `TipInstrument` în string cu un
// CASE care are ULTIMA ramură fallback („Chitanta") — un membru NOU de enum ar
// apărea tăcut ca „Chitanta" în grilă. Gardianul (analogul scării numerice): dacă
// enum-ul crește, testul pică zgomotos și cere actualizarea CASE-ului + a
// parse-ului `ApiEnum` + a etichetelor.
Check("Gardian F3-D5a: TipInstrumentPlata are exact membrii mapați în CASE-ul din Lista",
    Enum.GetNames<TipInstrumentPlata>().OrderBy(n => n)
        .SequenceEqual(new[] { "Cec", "Chitanta", "DispozitieCasa", "OrdinPlata" }));

void CurataApiTrz(IObjectSpace os) {
    // Toate documentele blocului ating cel puțin un repartitor marcat (inclusiv
    // plata autogenerată: TREZ → furnizorul marcat), deci marcajul de repartitor
    // e cheia de curățenie — ca la CurataTrz.
    var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiTrz)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
        os.Delete(doc);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiTrz)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajApiTrz + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiTrz(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
    var trezoreria = os.FirstOrDefault<ContPropriu>(c => c.Cod == "TREZ");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
    var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
    var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");

    var furnizor = os.CreateObject<Partener>();
    furnizor.Cod = MarcajApiTrz + "-FURN";
    furnizor.Denumire = "Furnizor probă felia Api Trz";
    var client = os.CreateObject<Partener>();
    client.Cod = MarcajApiTrz + "-CL";
    client.Denumire = "Client probă felia Api Trz";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajApiTrz + "-CE";
    codEc.Denumire = "Cod economic probă felia Api Trz";
    os.CommitChanges();

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
    // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
    IReadOnlyList<string> DryRunTrz(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }

    // ── (a) Plata culeasă manual ────────────────────────────────────────────
    var writePlt = new TrezorerieWriteDto {
        Data = new DateOnly(2026, 3, 14),
        PredatorId = casa.ID,
        PrimitorId = furnizor.ID,
        TipInstrument = "DispozitieCasa",
        NumarExtras = "EX-API-1",
        DataExtras = new DateOnly(2026, 3, 14),
        Linii = {
            // Tipul tehnic TRZ e DEFAULT DE CULEGERE în client (F3-D7), nu o
            // validare de server — de aceea vine prin payload ca oricare altul.
            new TrezorerieLinieWriteDto {
                TipMaterialId = tipTrz.ID, Valoare = 150m, CodEconomicId = codEc.ID
            }
        }
    };
    var idPlt = TrezorerieApply.Aplica<Plata>(os, null, writePlt);
    var plt = TrezorerieApply.Citeste<Plata>(os, idPlt);
    Check("Apply PLT creare → header plat; NUMĂRUL rămâne NULL — PLT are PoliticaNumerotare ⇒ server-owned (invers față de FCT, unde e cules)",
        plt != null && plt.Id == idPlt && plt.Stare == "Draft" && plt.Numar == null
        && plt.Data == new DateOnly(2026, 3, 14) && plt.DataOperare == null
        && plt.PredatorId == casa.ID && plt.PredatorDenumire == casa.Denumire
        && plt.PrimitorId == furnizor.ID && plt.PrimitorDenumire == furnizor.Denumire
        && plt.TipInstrument == "DispozitieCasa"
        && plt.NumarExtras == "EX-API-1" && plt.DataExtras == new DateOnly(2026, 3, 14)
        && plt.Total == 150m
        && !plt.Autogenerat && plt.DocumentSursaId == null && plt.DocumentSursaNumar == null
        && plt.DocumentSursaTip == null
        && plt.Copii.Count == 0
        && plt.PoateEdita && plt.PoateOpera && !plt.PoateAnula && !plt.PoateStorna);
    Check("Linia PLT: `Valoare` CULEASĂ (trezoreria n-are PregatesteOperare) + dimensiunea frunzei, proiectate plat",
        plt.Linii.Count == 1 && plt.Linii[0].TipMaterialId == tipTrz.ID
        && plt.Linii[0].TipMaterialCod == "TRZ" && plt.Linii[0].Valoare == 150m
        && plt.Linii[0].CodEconomicId == codEc.ID && plt.Linii[0].CodEconomicCod == codEc.Cod
        && plt.Linii[0].SursaFinantareId == null && plt.Linii[0].CodFunctionalId == null
        && plt.Linii[0].ProiectId == null && plt.Linii[0].AngajamentId == null);
    Check("Dry-run (Valideaza) pe draftul PLT valid → listă goală", DryRunTrz(idPlt).Count == 0);

    // Reconcilierea colecției + refuzurile de contract (pe calea de ACTUALIZARE,
    // ca un Apply refuzat să nu poată lăsa reziduu în ObjectSpace-ul viu).
    writePlt.Linii[0].Id = plt.Linii[0].Id;
    writePlt.Linii.Add(new TrezorerieLinieWriteDto {
        TipMaterialId = tipTrz.ID, Valoare = 20m, CodEconomicId = codEc.ID
    });
    TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt);
    Check("Reconciliere: linia nouă (fără Id) se adaugă, Total urcă la 170",
        TrezorerieApply.Citeste<Plata>(os, idPlt) is { Linii.Count: 2, Total: 170m });
    CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)",
        () => TrezorerieApply.Aplica<Plata>(os, idPlt, new TrezorerieWriteDto {
            Data = writePlt.Data, PredatorId = casa.ID, PrimitorId = furnizor.ID,
            Linii = { new TrezorerieLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipTrz.ID, Valoare = 1m } }
        }));
    CheckRefuza("Apply cu valoare în afara scării numeric(18,2) → refuz de domeniu, nu DbUpdateException",
        () => TrezorerieApply.Aplica<Plata>(os, idPlt, new TrezorerieWriteDto {
            Data = writePlt.Data, PredatorId = casa.ID, PrimitorId = furnizor.ID,
            Linii = { new TrezorerieLinieWriteDto {
                TipMaterialId = tipTrz.ID, Valoare = 1.005m } }
        }));
    writePlt.Linii.RemoveAt(1);
    TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt);
    Check("Reconciliere: linia absentă din payload se ȘTERGE; un Apply refuzat n-a lăsat reziduu",
        TrezorerieApply.Citeste<Plata>(os, idPlt) is { Linii.Count: 1, Total: 150m });

    // Laturile NU se verifică la scriere (draftul are voie să fie greșit) — le
    // refuză OPERAREA, prin hook-ul tipului. Proba pe un draft cu laturi inversate.
    var idPltInvers = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 3, 14),
        PredatorId = furnizor.ID, PrimitorId = casa.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 10m, CodEconomicId = codEc.ID } }
    });
    Check("TipInstrument absent din payload ⇒ OrdinPlata (default-ul convenției F3-D1)",
        TrezorerieApply.Citeste<Plata>(os, idPltInvers).TipInstrument == "OrdinPlata");
    var eroriInvers = DryRunTrz(idPltInvers);
    // Intenția probei rămâne aceeași (Apply acceptă laturile inversate, OPERAREA
    // le refuză), dar de la F7-D3 contul propriu e contrapartidă LEGALĂ pe PLT/
    // INC (viramentul intern) ⇒ „primitorul e casa" nu mai e greșeală în sine;
    // documentul rămâne refuzat pentru PREDATOR (un partener nu poate fi contul
    // din care se plătește). Cuplajul cu natura liniilor nu se aprinde: linia e
    // `TRZ`, iar predatorul-partener face documentul non-virament.
    Check("Apply acceptă laturile inversate (validarea lor e a OPERĂRII) — dry-run-ul o raportează ca DATE; de la F7-D3 rămâne DOAR refuzul predatorului (contul propriu e contrapartidă legală)",
        eroriInvers.Count == 1
        && eroriInvers.Any(e => e.Contains("Predatorul plății"))
        && !eroriInvers.Any(e => e.Contains("Primitorul plății")));
    TrezorerieApply.Sterge<Plata>(os, idPltInvers);
    Check("Sterge pe draft: documentul și liniile lui dispar împreună",
        TrezorerieApply.Citeste<Plata>(os, idPltInvers) == null
        && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idPltInvers));

    // ── Comanda pe plată: numărul din serie + contarea din laturi ───────────
    var rezPlt = OperareApi.Opereaza(os, idPlt);
    Check("OperareApi.Opereaza pe PLT → Operat, fără conex/secundar",
        rezPlt.StareNoua == StareDocument.Operat && rezPlt.ConexId == null && rezPlt.Mesaje.Count == 0);
    plt = TrezorerieApply.Citeste<Plata>(os, idPlt);
    Check("După operare numărul vine DIN SERIE (PLT-), nu din payload; affordances inversate",
        plt.Stare == "Operat" && plt.Numar?.StartsWith("PLT-") == true && plt.DataOperare != null
        && !plt.PoateEdita && !plt.PoateOpera && plt.PoateAnula && plt.PoateStorna);
    var notePlt = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idPlt).ToList();
    Check("PLT contează din laturi: 401.01.00 (fallback furnizor) = 531.01.01 (ContImplicit CASA), 150",
        notePlt.Count == 1 && notePlt[0].ContDebitId == cont401.ID
        && notePlt[0].ContCreditId == cont531.ID && notePlt[0].Valoare == 150m);
    Check("PLT nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idPlt));
    CheckRefuza("Apply peste PLT Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt));
    CheckRefuza("Sterge peste PLT Operat → același refuz de domeniu",
        () => TrezorerieApply.Sterge<Plata>(os, idPlt));

    var listaPlt = TrezorerieApply.Lista<Plata>(os).Where(x => x.Id == idPlt).ToList();
    Check("Lista PLT → Stare ȘI TipInstrument ca text (CASE în SQL), Autogenerat, Total din agregat",
        listaPlt.Count == 1 && listaPlt[0].Stare == "Operat"
        && listaPlt[0].TipInstrument == "DispozitieCasa" && !listaPlt[0].Autogenerat
        && listaPlt[0].Numar == plt.Numar && listaPlt[0].Total == 150m
        && listaPlt[0].PredatorDenumire == casa.Denumire
        && listaPlt[0].PrimitorDenumire == furnizor.Denumire);
    Check("Lista PLT → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru pe enum-ul textual + sort + take)",
        TrezorerieApply.Lista<Plata>(os).Where(x => x.TipInstrument == "DispozitieCasa")
            .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

    // ── (b) Încasarea: același nucleu, laturi oglindite ─────────────────────
    var idInc = TrezorerieApply.Aplica<Incasare>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 3, 15),
        PredatorId = client.ID, PrimitorId = casa.ID,
        TipInstrument = "Chitanta",
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 80m, CodEconomicId = codEc.ID } }
    });
    Check("Genericul filtrează pe TIP sub TPT: Citeste<Plata> pe un id de încasare → null (o rută nu adoptă documentele celeilalte)",
        TrezorerieApply.Citeste<Plata>(os, idInc) == null
        && TrezorerieApply.Citeste<Incasare>(os, idInc) != null
        && !TrezorerieApply.Lista<Plata>(os).Any(x => x.Id == idInc)
        && TrezorerieApply.Lista<Incasare>(os).Any(x => x.Id == idInc));
    OperareApi.Opereaza(os, idInc);
    var inc = TrezorerieApply.Citeste<Incasare>(os, idInc);
    var noteInc = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idInc).ToList();
    Check("Apply<Incasare> + operare: număr din seria proprie (INC-), contare oglindită 531.01.01 = 411.01.01 (fallback client), 80",
        inc.Stare == "Operat" && inc.Numar?.StartsWith("INC-") == true
        && inc.TipInstrument == "Chitanta" && inc.Total == 80m
        && noteInc.Count == 1 && noteInc[0].ContDebitId == cont531.ID
        && noteInc[0].ContCreditId == cont411.ID && noteInc[0].Valoare == 80m);

    // ── (c) Enum-ul pe sârmă: valoare necunoscută = refuz de domeniu ────────
    CheckRefuza("TipInstrument necunoscut → refuz cu valorile valide enumerate (nu conversie tăcută la 0, nu ArgumentException)",
        () => TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
            Data = new DateOnly(2026, 3, 14), PredatorId = casa.ID, PrimitorId = furnizor.ID,
            TipInstrument = "Bilet la ordin" }));
    CheckRefuza("TipInstrument dat ca NUMĂR („3”) → tot refuz: contractul e pe nume, nu pe ordinea membrilor",
        () => TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
            Data = new DateOnly(2026, 3, 14), PredatorId = casa.ID, PrimitorId = furnizor.ID,
            TipInstrument = "3" }));
    Check("Un Apply refuzat pe enum NU lasă document orfan în ObjectSpace (parse înaintea CreateObject)",
        os.GetObjectsQuery<Plata>().Count(p => p.PrimitorId == furnizor.ID) == 1);

    // ── (d) F3-D5: plata automată, cap-coadă prin API ───────────────────────
    var writeFct = new FacturaIntrareWriteDto {
        Numar = MarcajApiTrz + "-FF1",
        Data = new DateOnly(2026, 3, 16),
        PredatorId = furnizor.ID, PrimitorId = mag1.ID,
        // Grupul DECONT_*, ridicat din excluderea F2.
        GenereazaPlata = true,
        PlataContPropriuId = trezoreria.ID,
        PlataNumar = "OP-API-9",
        PlataData = new DateOnly(2026, 3, 17),
        PlataTipInstrument = "Cec",
        // Doar linii de SERVICIU ⇒ conexul NIR nu se generează (n-are linii
        // eligibile), deci singurul copil e SECUNDARUL — plata.
        Linii = { new FacturaIntrareLinieWriteDto {
            TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 100m,
            CodEconomicId = codEc.ID } }
    };
    var idFctPlata = FacturaIntrareApply.Aplica(os, null, writeFct);
    var fctCitit = FacturaIntrareApply.Citeste(os, idFctPlata);
    Check("F3-D5: parametrii plății automate fac ROUND-TRIP prin DTO-urile FCT (bifă, cont propriu + denumire, număr, dată, instrument ca STRING)",
        fctCitit.GenereazaPlata && fctCitit.PlataContPropriuId == trezoreria.ID
        && fctCitit.PlataContPropriuDenumire == trezoreria.Denumire
        && fctCitit.PlataNumar == "OP-API-9" && fctCitit.PlataData == new DateOnly(2026, 3, 17)
        && fctCitit.PlataTipInstrument == "Cec");
    CheckRefuza("Instrument de plată necunoscut pe FCT → același refuz (helper de parse COMUN cu trezoreria)",
        () => FacturaIntrareApply.Aplica(os, idFctPlata, new FacturaIntrareWriteDto {
            Numar = writeFct.Numar, Data = writeFct.Data,
            PredatorId = furnizor.ID, PrimitorId = mag1.ID, PlataTipInstrument = "OP" }));
    var idContPropriuInexistent = Guid.NewGuid();
    CheckRefuza("Cont propriu inexistent pe FCT → refuz cu mesaj de domeniu (nu violare de FK)",
        () => FacturaIntrareApply.Aplica(os, idFctPlata, new FacturaIntrareWriteDto {
            Numar = writeFct.Numar, Data = writeFct.Data,
            PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            GenereazaPlata = true, PlataContPropriuId = idContPropriuInexistent }));

    var rezFct = OperareApi.Opereaza(os, idFctPlata);
    Check("Operarea FCT (numai servicii ⇒ fără NIR conex) întoarce SECUNDARUL: draftul de plată",
        rezFct.StareNoua == StareDocument.Operat && rezFct.ConexId != null);
    var idPlataAuto = rezFct.ConexId.Value;
    fctCitit = FacturaIntrareApply.Citeste(os, idFctPlata);
    Check("Citeste.Copii → plata autogenerată: Tip „PLT” din ancora TipDocument, numărul CULES pe factură, Draft, Autogenerat",
        fctCitit.Copii.Count == 1 && fctCitit.Copii[0].Id == idPlataAuto
        && fctCitit.Copii[0].Tip == "PLT" && fctCitit.Copii[0].Numar == "OP-API-9"
        && fctCitit.Copii[0].Stare == "Draft" && fctCitit.Copii[0].Autogenerat);

    var plataAuto = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
    Check("Citeste<Plata> pe copil: header din grupul DECONT_* (TREZ→furnizor, Cec, data plății) + link înapoi la factură prin numărul ei",
        plataAuto != null && plataAuto.Autogenerat && plataAuto.Stare == "Draft"
        && plataAuto.Numar == "OP-API-9" && plataAuto.Data == new DateOnly(2026, 3, 17)
        && plataAuto.TipInstrument == "Cec"
        && plataAuto.PredatorId == trezoreria.ID && plataAuto.PrimitorId == furnizor.ID
        && plataAuto.DocumentSursaId == idFctPlata
        && plataAuto.DocumentSursaNumar == writeFct.Numar
        && plataAuto.DocumentSursaTip == "FCT"
        && plataAuto.Total == 121m);
    Check("Liniile plății autogenerate păstrează Tipul SURSEI (628, nu TRZ — F3-D7 e convenție de client), valoarea BRUTĂ și dimensiunea clonată",
        plataAuto.Linii.Count == 1 && plataAuto.Linii[0].TipMaterialId == tipServicii.ID
        && plataAuto.Linii[0].TipMaterialCod == "628.00.00"
        && plataAuto.Linii[0].Valoare == 121m
        && plataAuto.Linii[0].CodEconomicId == codEc.ID);

    OperareApi.Opereaza(os, idPlataAuto);
    var impAuto = os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == idPlataAuto).ToList();
    Check("Operarea plății autogenerate prin OperareApi → imperecherea automată pe BRUT (121), factura stinsă integral",
        impAuto.Count == 1 && impAuto[0].DocumentId == idFctPlata && impAuto[0].Suma == 121m
        && impAuto[0].Autogenerat && ImperechereService.Ramas(os, idFctPlata) == 0m);
    var plataOperata = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
    var notePlataAuto = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idPlataAuto).ToList();
    Check("Plata autogenerată operată: numărul rămâne CEL CULES pe factură (AsignaNumar onorează un număr existent — nu se consumă serie), contare 401 = 770",
        plataOperata.Stare == "Operat" && plataOperata.Numar == "OP-API-9"
        && notePlataAuto.Count == 1 && notePlataAuto[0].ContDebitId == cont401.ID
        && notePlataAuto[0].ContCreditId == cont770.ID && notePlataAuto[0].Valoare == 121m);
    Check("Affordance onestă pe FCT (F2-D5): copilul PLT operat blochează anularea/stornarea facturii",
        FacturaIntrareApply.Citeste(os, idFctPlata) is { PoateAnula: false, PoateStorna: false });

    // ── (e) F3-D2/F3-D3: affordances ONESTE + stingerile prin API ───────────
    // Perechea de mai jos era MARCAJUL limitei pasului 1 (affordance optimist
    // lângă refuzul motorului), scrisă ca să pice când intră fix-ul. Aici e
    // adusă la noul adevăr: DTO-ul spune același lucru ca gardianul.
    Check("F3-D2: affordance ONESTĂ — plata cu imperechere NU se mai anunță anulabilă (oglinda lui VerificaFaraImperecheri)",
        !plataOperata.PoateAnula && !plataOperata.PoateStorna);
    CheckRefuza("…iar motorul chiar refuză: anularea plății cu imperechere",
        () => OperareApi.AnuleazaOperarea(os, idPlataAuto));
    Check("F3-D2: numerele stingerii pe ReadDto-ul trezoreriei (Total/Asignat/Ramas din serviciu — TS nu le calculează)",
        plataOperata.Total == 121m && plataOperata.Asignat == 121m && plataOperata.Ramas == 0m);

    // Panoul de stingeri, citit din AMBELE capete ale ACELEIAȘI legături.
    var stingeriPlataAuto = ImperechereApply.Stingeri(os, idPlataAuto);
    Check("StingeriDto pe plata autogenerată: rolul de STINGĂTOR, celălalt document tipat „FCT” din ancoră, marcat Autogenerat",
        stingeriPlataAuto is { Total: 121m, Asignat: 121m, Ramas: 0m }
        && stingeriPlataAuto.Imperecheri.Count == 1
        && stingeriPlataAuto.Imperecheri[0].EsteStingator
        && stingeriPlataAuto.Imperecheri[0].CelalaltDocumentId == idFctPlata
        && stingeriPlataAuto.Imperecheri[0].CelalaltTip == "FCT"
        && stingeriPlataAuto.Imperecheri[0].CelalaltNumar == writeFct.Numar
        && stingeriPlataAuto.Imperecheri[0].Suma == 121m
        && stingeriPlataAuto.Imperecheri[0].Autogenerat);
    var stingeriFct = ImperechereApply.Stingeri(os, idFctPlata);
    Check("StingeriDto pe factură: ACELAȘI rând, cu rolul INVERSAT (EsteStingator false) și celălalt document tipat „PLT”",
        stingeriFct is { Total: 121m, Asignat: 121m, Ramas: 0m }
        && stingeriFct.Imperecheri.Count == 1
        && stingeriFct.Imperecheri[0].Id == stingeriPlataAuto.Imperecheri[0].Id
        && !stingeriFct.Imperecheri[0].EsteStingator
        && stingeriFct.Imperecheri[0].CelalaltDocumentId == idPlataAuto
        && stingeriFct.Imperecheri[0].CelalaltTip == "PLT"
        && stingeriFct.Imperecheri[0].CelalaltNumar == "OP-API-9");
    Check("Stingeri pe un id inexistent → null (nu excepție)",
        ImperechereApply.Stingeri(os, Guid.NewGuid()) == null);
    Check("F3-D4: documentul STINS INTEGRAL nu e candidat — filtrul Rest > 0 se aplică DUPĂ calcul, în SQL",
        !ImperecheriProiectii.DocumenteCuRest(os).Any(r => r.DocumentId == idFctPlata));

    // Ștergerea link-ului e LIBERĂ (31d) și deblochează anularea — exact fluxul
    // clientului: după DELETE, butonul Anulează redevine activ.
    ImperechereApply.Sterge(os, stingeriPlataAuto.Imperecheri[0].Id);
    var plataFaraLink = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
    Check("Sterge imperecherea → restul revine pe ambele documente ȘI affordance-ul se redeschide",
        plataFaraLink is { PoateAnula: true, PoateStorna: true, Asignat: 0m, Ramas: 121m }
        && ImperechereService.Ramas(os, idFctPlata) == 121m
        && ImperechereApply.Stingeri(os, idFctPlata).Imperecheri.Count == 0);
    CheckRefuza("Sterge pe o imperechere inexistentă → refuz de domeniu (nu NullReference)",
        () => ImperechereApply.Sterge(os, Guid.NewGuid()));

    // Creare prin API pe lanțul MANUAL: plata culeasă (150, casa → furnizor)
    // stinge parțial factura ACELUIAȘI furnizor (121).
    var creata = ImperechereApply.Creeaza(os, new ImperechereWriteDto {
        DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 100m });
    var panouPlt = ImperechereApply.Stingeri(os, idPlt);
    var panouFct = ImperechereApply.Stingeri(os, idFctPlata);
    Check("ImperechereApply.Creeaza → link ne-autogenerat; restul scade pe AMBELE părți (plata 50, factura 21)",
        creata.DocumentStingatorId == idPlt && creata.DocumentId == idFctPlata
        && creata.Suma == 100m && !creata.Autogenerat
        && panouPlt is { Total: 150m, Asignat: 100m, Ramas: 50m }
        && panouFct is { Total: 121m, Asignat: 100m, Ramas: 21m });
    Check("Rolurile în panou: plata e STINGĂTOR (celălalt FCT), factura e stinsă (celălalt PLT)",
        panouPlt.Imperecheri.Single() is { EsteStingator: true, CelalaltTip: "FCT", Autogenerat: false }
        && panouFct.Imperecheri.Single() is { EsteStingator: false, CelalaltTip: "PLT" });

    // Invarianții NU se rescriu în adaptor — refuzurile vin din
    // `ImperechereService.ValideazaCreare`, prin aceeași cale ca UI-ul.
    CheckRefuza("Creeaza peste restul stingibil al facturii (21 rămași, se cer 40) → refuz",
        () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 40m }));
    CheckRefuza("Creeaza fără contrapartidă comună (încasarea clientului × factura furnizorului) → refuz",
        () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idInc, DocumentId = idFctPlata, Suma = 10m }));
    CheckRefuza("Creeaza Plata↔Plata (același sens) → refuz",
        () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idPlt, DocumentId = idPlataAuto, Suma = 10m }));
    CheckRefuza("Creeaza cu document inexistent → mesaj de DOMENIU la graniță (traducerea cheie → entitate, abaterea de la 42b)",
        () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idPlt, DocumentId = Guid.NewGuid(), Suma = 10m }));
    CheckRefuza("Creeaza cu sumă în afara scării numeric(18,2) → refuz de domeniu, nu rotunjire tăcută",
        () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
            DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 1.005m }));
    Check("Un Creeaza refuzat n-a lăsat link fantomă (validarea precede CreateObject, în serviciu)",
        ImperechereApply.Stingeri(os, idFctPlata).Imperecheri.Count == 1);

    // ── (f) F3-D4: proiecția de rest, în oglindă cu serviciul ───────────────
    var cuRest = ImperecheriProiectii.DocumenteCuRest(os).ToList();
    Check("F3-D9: proiecția DocumenteCuRest == ImperechereService.Ramas pe FIECARE rând (un al doilea adevăr ar fi un defect)",
        cuRest.Count > 0 && cuRest.All(r => r.Rest == ImperechereService.Ramas(os, r.DocumentId)));
    var idsRdcOperate = os.GetObjectsQuery<ReturClient>()
        .Where(d => d.Stare == StareDocument.Operat).Select(d => d.ID).ToList();
    Check($"F3-D4: uniunea acoperă EXACT cele cinci tipuri concrete; RDC exclus deliberat (LiniiCreanta divergent) — {idsRdcOperate.Count} retururi operate în bază",
        cuRest.All(r => r.Tip is "FCT" or "FCL" or "PLT" or "INC" or "DEC")
        && !cuRest.Any(r => idsRdcOperate.Contains(r.DocumentId)));
    var randFct = cuRest.Single(r => r.DocumentId == idFctPlata);
    var randPlt = cuRest.Single(r => r.DocumentId == idPlt);
    Check("F3-D4: rândurile poartă tipul, contrapartida (latura partener, nu contul propriu) și cele trei numere",
        randFct is { Tip: "FCT", Total: 121m, Asignat: 100m, Rest: 21m }
        && randFct.ContrapartidaId == furnizor.ID && randFct.ContrapartidaDenumire == furnizor.Denumire
        && randPlt is { Tip: "PLT", Total: 150m, Asignat: 100m, Rest: 50m }
        && randPlt.ContrapartidaId == furnizor.ID);
    var cuRestFurnizor = ImperecheriProiectii.DocumenteCuRest(os, furnizor.ID).ToList();
    Check("F3-D4: filtrul pe contrapartidă dă candidații unui singur partener (factura + plățile furnizorului, fără încasarea clientului)",
        cuRestFurnizor.Count > 0 && cuRestFurnizor.All(r => r.ContrapartidaId == furnizor.ID)
        && cuRestFurnizor.Any(r => r.DocumentId == idFctPlata)
        && cuRestFurnizor.Any(r => r.DocumentId == idPlt)
        && !cuRestFurnizor.Any(r => r.DocumentId == idInc));
    Check("F3-D4: proiecția rămâne IQueryable — filtrarea/sortarea/paginarea se traduc în SQL peste uniune (sondă: filtru pe tip + sort + take)",
        ImperecheriProiectii.DocumenteCuRest(os).Where(r => r.Tip == "PLT")
            .OrderByDescending(r => r.Rest).Take(1).ToList().Count == 1);

    ImperechereApply.Sterge(os, creata.Id);
    Check("Sterge link-ul manual → restul revine integral pe ambele (150 / 121)",
        ImperechereService.Ramas(os, idPlt) == 150m
        && ImperechereService.Ramas(os, idFctPlata) == 121m);

    CurataApiTrz(os);
    Check("Curățenie finală felia Api Trz (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiTrz))
        && !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith(MarcajApiTrz)));
}

// ============ Scenariul e2e felia 7: viramentul intern (transferul 581) ============
// Contract p5-felia-vir, ancorele F7-D9. Transferul de bani între conturile
// proprii (casă ↔ bancă) — amânarea declarată la decizia 31f. Viramentul NU e un
// tip de document nou (F7-D1): e o PERECHE PLT+INC pe ACELEAȘI laturi (predator =
// contul sursă, primitor = contul destinație pe ambele picioare), fiindcă cele
// două picioare sunt confirmate de documente diferite, la date diferite (foaia de
// vărsământ azi, extrasul mâine). Contul 581 („viramente interne") ține diferența
// de timp și se închide singur când ambele picioare sunt operate.
//
// Fluxul probat, cap-coadă prin API: culegere prin `TrezorerieApply.Aplica<Plata>`
// → refuzurile cuplajului laturi↔natură prin dry-run → operare (581 = 5311, zero
// stoc, dimensiunea Repartitor = contul propriu AL PICIORULUI — F7-D5b) → latura
// pereche autogenerată în `Copii[]` → operarea ei (5121/770 = 581, ZERO
// imperecheri — F7-D5) → 581 închis la 0 → anulare, regenerare, storno pe ambele.
const string MarcajAvir = "E2E-AVIR";

void CurataAvir(IObjectSpace os) {
    // Toate documentele blocului ating cel puțin un repartitor marcat (inclusiv
    // latura pereche autogenerată: aceleași laturi ca sursa) — aceeași cheie de
    // curățenie ca la CurataApiTrz.
    var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAvir)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    // Copiii (latura pereche) înaintea părinților — FK-ul DocumentSursa.
    foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
        os.Delete(doc);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAvir)).ToList());
    // Regulile-fixtură ale probei de semn (F2) — înaintea Tipului lor, altfel
    // FK-ul le-ar ține în viață după o rulare întreruptă.
    var tipuriAvir = os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajAvir)).Select(t => t.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegulaContare>()
        .Where(r => r.TipMaterialId != null && tipuriAvir.Contains(r.TipMaterialId.Value)).ToList());
    // Tipul-fixtură (natura Virament, FĂRĂ regulă) se șterge DUPĂ linii.
    os.Delete(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajAvir)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajAvir + "-CE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataAvir(os);

    var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var cont581 = os.FirstOrDefault<Cont>(c => c.Simbol == "581");
    var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
    var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var ancoraPlt = os.FirstOrDefault<TipDocument>(t => t.Cod == "PLT");
    var ancoraInc = os.FirstOrDefault<TipDocument>(t => t.Cod == "INC");

    // ── (a) Seed-ul F7-D6: Clasa/Tipul VIR + cele două reguli ale perechii ──
    Check("Seed F7-D6: Clasa/Tipul „VIR” (Natura=Virament) cu contul de tranzit 581 legat EXPLICIT (derivările din simbol nu ating codul „VIR”)",
        tipVir != null && tipVir.Clasa.Cod == "VIR" && tipVir.Clasa.Natura == NaturaClasa.Virament
        && cont581 != null && tipVir.ContImplicitId == cont581.ID);
    var regulaPltVir = os.FirstOrDefault<RegulaContare>(
        r => r.TipDocumentId == ancoraPlt.ID && r.TipMaterialId == tipVir.ID);
    var regulaIncVir = os.FirstOrDefault<RegulaContare>(
        r => r.TipDocumentId == ancoraInc.ID && r.TipMaterialId == tipVir.ID);
    Check("Seed F7-D6: PLT×VIR = 581 (TipMaterial) / contul propriu PREDATOR fără fallback; INC×VIR oglindit (destinație / 581)",
        regulaPltVir != null && regulaPltVir.SursaContDebit == SursaCont.TipMaterial
        && regulaPltVir.SursaContCredit == SursaCont.RepartitorPredator && regulaPltVir.ContCreditId == null
        && regulaIncVir != null && regulaIncVir.SursaContDebit == SursaCont.RepartitorPrimitor
        && regulaIncVir.ContDebitId == null && regulaIncVir.SursaContCredit == SursaCont.TipMaterial);
    Check("Seed F7-D6: garda de idempotență e PER RÂND — rândul generic al fiecărui tip există exact o dată (nu s-a duplicat la re-seed, nici n-a înghițit rândul nou)",
        os.GetObjectsQuery<RegulaContare>().Count(r => r.TipDocumentId == ancoraPlt.ID && r.TipMaterialId == null) == 1
        && os.GetObjectsQuery<RegulaContare>().Count(r => r.TipDocumentId == ancoraInc.ID && r.TipMaterialId == null) == 1);
    Check("Seed F7-D6: viramentul NU primește reguli de STOC (Natura ≠ Stoc ⇒ motorul nu caută nimic)",
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "PLT" || r.TipDocument.Cod == "INC"));

    // Fixture: DOUĂ conturi proprii distincte (marcate — cheia de curățenie) +
    // un partener (contrapartida „greșită” a probelor de cuplaj) + un Tip de
    // natura Virament FĂRĂ rând de regulă, adică exact riscul pin-uit al feliei
    // („profil cu Tipul VIR seed-uit dar fără politica lui”). Fixtura înlocuiește
    // ștergerea temporară a regulii seed-uite: probează ACELAȘI gard, fără să
    // atingă seed-ul bazei.
    var casaVir = os.CreateObject<ContPropriu>();
    casaVir.Cod = MarcajAvir + "-CASA";
    casaVir.Denumire = "Casa probă virament";
    casaVir.ContImplicit = cont531;
    var bancaVir = os.CreateObject<ContPropriu>();
    bancaVir.Cod = MarcajAvir + "-BANCA";
    bancaVir.Denumire = "Banca probă virament";
    bancaVir.ContImplicit = cont770;
    bancaVir.EsteBanca = true;
    var partenerVir = os.CreateObject<Partener>();
    partenerVir.Cod = MarcajAvir + "-P";
    partenerVir.Denumire = "Partener probă virament";
    var codEcVir = os.CreateObject<CodEconomic>();
    codEcVir.Cod = MarcajAvir + "-CE";
    codEcVir.Denumire = "Cod economic probă virament";
    var tipVirFaraRegula = os.CreateObject<TipMaterial>();
    tipVirFaraRegula.Cod = MarcajAvir + "-VIR2";
    tipVirFaraRegula.Denumire = "Virament fără politică (fixtură)";
    tipVirFaraRegula.Clasa = tipVir.Clasa;
    tipVirFaraRegula.ContImplicit = cont581;
    os.CommitChanges();

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (`PregatesteOperare` SCRIE).
    IReadOnlyList<string> DryRunAvir(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }

    // ── (b) Ancora 1: viramentul cules prin API, cu affordance-ul de FORMĂ ──
    var writeVir = new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 10),
        PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
        TipInstrument = "DispozitieCasa",
        NumarExtras = "FV-AVIR-1", DataExtras = new DateOnly(2026, 4, 10),
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipVir.ID, Valoare = 500m, CodEconomicId = codEcVir.ID } }
    };
    var idVirPlt = TrezorerieApply.Aplica<Plata>(os, null, writeVir);
    var virPlt = TrezorerieApply.Citeste<Plata>(os, idVirPlt);
    Check("F7-D9 ancora 1: virament cules prin Apply<Plata> (CASA → BANCA, linie VIR) — WriteDto NEATINS, iar Citeste întoarce EsteVirament = true, calculat pe SERVER",
        virPlt is { EsteVirament: true, Stare: "Draft", Numar: null, Total: 500m, Autogenerat: false }
        && virPlt.PredatorId == casaVir.ID && virPlt.PrimitorId == bancaVir.ID
        && virPlt.Linii.Count == 1 && virPlt.Linii[0].TipMaterialCod == "VIR"
        && virPlt.Linii[0].Valoare == 500m && virPlt.Linii[0].CodEconomicId == codEcVir.ID);
    Check("F7-D9 ancora 1: dry-run pe viramentul valid → listă goală (cuplajul laturi↔natură se potrivește în ambele sensuri)",
        DryRunAvir(idVirPlt).Count == 0);

    // Martorul: aceeași casă, dar contrapartidă PARTENER și linie TRZ = plată
    // obișnuită. `EsteVirament` cere AMBELE laturi conturi proprii, nu doar una.
    var idPltNormala = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 10),
        PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
    });
    Check("F7-D7: plata OBIȘNUITĂ din același cont propriu rămâne EsteVirament = false, iar dry-run-ul ei e curat (nicio regresie de laturi)",
        TrezorerieApply.Citeste<Plata>(os, idPltNormala) is { EsteVirament: false }
        && DryRunAvir(idPltNormala).Count == 0);

    // ── (c) Ancora 2: cele patru refuzuri, prin dry-run ─────────────────────
    var politicaPlt = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT");
    var serieInainte = politicaPlt.UrmatorulNumar;

    var idRefuzTrz = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 11),
        PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
    });
    var eroriTrzPeContPropriu = DryRunAvir(idRefuzTrz);
    Check("F7-D9 ancora 2a: contrapartidă cont propriu + linie TRZ → refuz (fără cuplaj, linia ar cădea pe regula GENERICĂ a tipului și ar posta „destinație = sursă” pe FIECARE picior — dublă postare tăcută)",
        eroriTrzPeContPropriu.Count == 1
        && eroriTrzPeContPropriu[0].Contains("toate liniile trebuie să fie de virament"));

    var idRefuzVirPePartener = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 11),
        PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipVir.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
    });
    var eroriVirPePartener = DryRunAvir(idRefuzVirPePartener);
    Check("F7-D9 ancora 2b: linie VIR + contrapartidă partener → refuz (cuplajul e verificat în AMBELE sensuri; altfel 581 ar rămâne deschis pe veci)",
        eroriVirPePartener.Count == 1
        && eroriVirPePartener[0].Contains("cer contrapartidă cont propriu"));

    var idRefuzCatreSine = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 11),
        PredatorId = casaVir.ID, PrimitorId = casaVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipVir.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
    });
    var eroriCatreSine = DryRunAvir(idRefuzCatreSine);
    Check("F7-D9 ancora 2c: Predator == Primitor → refuz (581 = 581 pe același cont propriu, iar dimensiunea implicită n-ar mai distinge nimic)",
        eroriCatreSine.Count == 1
        && eroriCatreSine[0].Contains("același repartitor"));

    var idRefuzFaraRegula = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 11),
        PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipVirFaraRegula.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
    });
    var eroriFaraRegula = DryRunAvir(idRefuzFaraRegula);
    Check("F7-D9 ancora 2d: linie de natura Virament FĂRĂ regulă potrivită → refuz explicit (oglinda 38c: potrivirea ar cădea pe regula generică a tipului, fără niciun zgomot)",
        eroriFaraRegula.Count == 1
        && eroriFaraRegula[0].Contains("nu are regulă de contare potrivită"));

    var idsRefuz = new List<Guid> { idRefuzTrz, idRefuzVirPePartener, idRefuzCatreSine, idRefuzFaraRegula };
    Check("F7-D9 ancora 2: niciun refuz n-a lăsat rânduri-fantomă și n-a consumat serie (dry-run = calculează+validează, fără materializare — 33d)",
        idsRefuz.All(i => TrezorerieApply.Citeste<Plata>(os, i).Numar == null)
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId != null && idsRefuz.Contains(r.DocumentId.Value))
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId != null && idsRefuz.Contains(r.DocumentId.Value))
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar == serieInainte);
    foreach (var idRefuz in idsRefuz)
        TrezorerieApply.Sterge<Plata>(os, idRefuz);
    TrezorerieApply.Sterge<Plata>(os, idPltNormala);

    // ── Review advers F2: gardul trebuie să OGLINDEASCĂ matcher-ul motorului ──
    // Motorul filtrează ÎNTÂI pe `SemnFiltru` și abia din supraviețuitori alege
    // Tip exact → NaturaFiltru → generic. Pe trezorerie `Cantitate` = 0, deci
    // semnul e 0 și orice rând cu SemnFiltru ±1 iese din joc. `RegulaContare` e
    // dată EDITABILĂ în XAF: un rând de virament cu semn pus din greșeală ar
    // trece de un gard care ignoră semnul, iar motorul ar cădea pe regula
    // GENERICĂ a tipului — exact dubla postare tăcută pe care gardul o previne.
    var regulaSemn = os.CreateObject<RegulaContare>();
    regulaSemn.TipDocument = ancoraPlt;
    regulaSemn.TipMaterial = tipVirFaraRegula;
    regulaSemn.SursaContDebit = SursaCont.TipMaterial;
    regulaSemn.SursaContCredit = SursaCont.RepartitorPredator;
    regulaSemn.SemnFiltru = 1;
    os.CommitChanges();
    var idRefuzSemn = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 11),
        PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipVirFaraRegula.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
    });
    var eroriSemn = DryRunAvir(idRefuzSemn);
    Check("Review advers F2: rândul de virament cu `SemnFiltru = +1` NU e o potrivire (motorul îl exclude, semnul liniei de trezorerie e 0) ⇒ gardul refuză în continuare, în loc să lase documentul pe regula generică",
        eroriSemn.Count == 1 && eroriSemn[0].Contains("nu are regulă de contare potrivită"));
    TrezorerieApply.Sterge<Plata>(os, idRefuzSemn);
    os.Delete(regulaSemn);
    os.CommitChanges();

    // ── (d) Ancora 3: operarea piciorului de IEȘIRE ─────────────────────────
    var rezVirPlt = OperareApi.Opereaza(os, idVirPlt);
    var idVirInc = rezVirPlt.ConexId ?? Guid.Empty;
    var notePicior1 = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idVirPlt).ToList();
    Check("F7-D9 ancora 3: piciorul de ieșire postează 581 (tranzit) = 531.01.01 (contul propriu SURSĂ), 500 — un singur rând, ZERO stoc",
        notePicior1.Count == 1 && notePicior1[0].ContDebitId == cont581.ID
        && notePicior1[0].ContCreditId == cont531.ID && notePicior1[0].Valoare == 500m
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idVirPlt));
    Check("F7-D9 ancora 3 (F7-D5b): dimensiunea Repartitor = contul propriu AL PICIORULUI pe AMBELE laturi ale rândului — default-ul „debit←Predator/credit←Primitor” ar fi atribuit ieșirea contului DESTINAȚIE",
        notePicior1[0].DimensiuniDebit().RepartitorId == casaVir.ID
        && notePicior1[0].DimensiuniCredit().RepartitorId == casaVir.ID);

    // ── (e) Ancora 4: latura pereche ────────────────────────────────────────
    var virPltOperat = TrezorerieApply.Citeste<Plata>(os, idVirPlt);
    Check("F7-D9 ancora 4: latura pereche apare în Copii[] ca INC, Draft, Autogenerat, fără număr (seria se consumă la propria operare)",
        rezVirPlt.StareNoua == StareDocument.Operat && rezVirPlt.ConexId != null
        && virPltOperat.Numar?.StartsWith("PLT-") == true
        && virPltOperat.Copii.Count == 1 && virPltOperat.Copii[0].Id == idVirInc
        && virPltOperat.Copii[0].Tip == "INC" && virPltOperat.Copii[0].Stare == "Draft"
        && virPltOperat.Copii[0].Autogenerat && virPltOperat.Copii[0].Numar == null);
    var virInc = TrezorerieApply.Citeste<Incasare>(os, idVirInc);
    Check("F7-D9 ancora 4: perechea are laturile IDENTICE (NEinversate — direcția o poartă TIPUL), NumarExtras/DataExtras GOALE (fiecare picior are extrasul lui) și linia clonată cu dimensiuni",
        virInc is { EsteVirament: true, Autogenerat: true, Stare: "Draft", Numar: null,
            NumarExtras: null, DataExtras: null, Total: 500m }
        && virInc.PredatorId == casaVir.ID && virInc.PrimitorId == bancaVir.ID
        && virInc.Data == new DateOnly(2026, 4, 10) && virInc.TipInstrument == "DispozitieCasa"
        && virInc.DocumentSursaId == idVirPlt && virInc.DocumentSursaTip == "PLT"
        && virInc.Linii.Count == 1 && virInc.Linii[0].TipMaterialId == tipVir.ID
        && virInc.Linii[0].Valoare == 500m && virInc.Linii[0].CodEconomicId == codEcVir.ID);

    // Pivotul polimorf al lui F7-D5, direct pe contract: viramentul nu stinge și
    // nu poate fi stins; plata obișnuită își păstrează plafonul de dinainte.
    var idPltMartor = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 10),
        PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
    });
    Check("F7-D5: `CapacitateStingere` e NULL pe AMBELE picioare ale viramentului și rămâne plafonul obișnuit (o contrapartidă) pe plata către partener",
        os.GetObjectByKey<Plata>(idVirPlt).CapacitateStingere(os) == null
        && os.GetObjectByKey<Incasare>(idVirInc).CapacitateStingere(os) == null
        && os.GetObjectByKey<Plata>(idPltMartor).CapacitateStingere(os) is { Count: 1 });
    TrezorerieApply.Sterge<Plata>(os, idPltMartor);

    // ── (f) Ancora 5 + 8: operarea laturii pereche ──────────────────────────
    // Operatorul îi pune extrasul lui și o operează — perechea E editabilă prin
    // aceeași rută (`PoateEdita` = funcție de stare, F5-D8b).
    TrezorerieApply.Aplica<Incasare>(os, idVirInc, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 12),
        PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
        TipInstrument = "DispozitieCasa",
        NumarExtras = "EX-AVIR-2", DataExtras = new DateOnly(2026, 4, 12),
        Linii = { new TrezorerieLinieWriteDto {
            Id = virInc.Linii[0].Id, TipMaterialId = tipVir.ID,
            Valoare = 500m, CodEconomicId = codEcVir.ID } }
    });
    var rezVirInc = OperareApi.Opereaza(os, idVirInc);
    var notePicior2 = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idVirInc).ToList();
    Check("F7-D9 ancora 5: piciorul de INTRARE postează 770.00.00 (contul propriu DESTINAȚIE) = 581, la data lui; Repartitor = contul propriu al ACESTUI picior pe ambele laturi",
        rezVirInc.StareNoua == StareDocument.Operat
        && notePicior2.Count == 1 && notePicior2[0].ContDebitId == cont770.ID
        && notePicior2[0].ContCreditId == cont581.ID && notePicior2[0].Valoare == 500m
        && notePicior2[0].Data == new DateOnly(2026, 4, 12)
        && notePicior2[0].DimensiuniDebit().RepartitorId == bancaVir.ID
        && notePicior2[0].DimensiuniCredit().RepartitorId == bancaVir.ID
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idVirInc));
    Check("F7-D9 ancora 8: latura pereche OPERATĂ nu generează un al treilea document — gardul `Autogenerat` din `GenereazaSecundar` taie ping-pong-ul",
        rezVirInc.ConexId == null
        && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idVirInc));

    var idsPicioare = new List<Guid> { idVirPlt, idVirInc };
    var notePereche = os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && idsPicioare.Contains(r.DocumentId.Value))
        .Select(r => new { r.DocumentId, r.ContDebitId, r.ContCreditId, r.Valoare }).ToList();
    var sold581 = notePereche.Where(r => r.ContDebitId == cont581.ID).Sum(r => r.Valoare)
        - notePereche.Where(r => r.ContCreditId == cont581.ID).Sum(r => r.Valoare);
    Check("F7-D9 ancora 5: EXACT două rânduri de registru contabil pe toată perechea, ZERO rânduri de stoc, iar 581 se închide singur la 0 după ambele picioare",
        notePereche.Count == 2 && sold581 == 0m
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId != null && idsPicioare.Contains(r.DocumentId.Value)));
    Check("F7-D9 ancora 5: ZERO imperecheri create la operarea laturii pereche (F7-D5 — un link cu propria sursă ar fi blocat anularea/stornarea AMBELOR picioare)",
        !os.GetObjectsQuery<Imperechere>().Any(i => idsPicioare.Contains(i.DocumentStingatorId) || idsPicioare.Contains(i.DocumentId)));
    var virIncOperat = TrezorerieApply.Citeste<Incasare>(os, idVirInc);
    Check("F7-D9 ancora 5: affordance-ul rămâne onest — piciorul operat, fără imperecheri, se poate anula/storna; numărul vine din seria INC-",
        virIncOperat is { Stare: "Operat", Asignat: 0m, PoateAnula: true, PoateStorna: true, EsteVirament: true }
        && virIncOperat.Numar?.StartsWith("INC-") == true
        && virIncOperat.NumarExtras == "EX-AVIR-2");

    // ── Review advers F1: „nu poate fi STINS” — IMPUS, nu doar afirmat ──────
    // `CapacitateStingere` = null îl scoate din rolul de STINGĂTOR; rolul de
    // STINS e altul, și argumentul „contrapartida unei plăți normale nu apare pe
    // laturile lui” e FALS pentru `NotaContabila`: capacitățile ei sunt
    // repartitorii EXPLICIȚI ai liniilor (49a), deci pot cădea pe orice latură,
    // inclusiv pe conturile proprii ale unui picior de virament. O astfel de
    // stingere i-ar bloca definitiv anularea/stornarea, iar clientul ascunde
    // panoul de stingeri pe virament (F7-D8) ⇒ operatorul n-ar vedea DE CE.
    var unitateAvir = os.CreateObject<UnitateInterna>();
    unitateAvir.Cod = MarcajAvir + "-UI";
    unitateAvir.Denumire = "Unitate probă virament";
    os.CommitChanges();

    // Martorul de NONREGRESIE: plată OBIȘNUITĂ (contrapartidă partener) care
    // poartă ACELAȘI cont propriu pe latura ei — nota de mai jos trebuie să o
    // stingă normal. Gardul țintește viramentul, nu nota.
    var idPltStins = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
        Data = new DateOnly(2026, 4, 13),
        PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
        Linii = { new TrezorerieLinieWriteDto {
            TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
    });
    OperareApi.Opereaza(os, idPltStins);

    var ntcAvir = os.CreateObject<NotaContabila>();
    ntcAvir.Data = new DateOnly(2026, 4, 13);
    ntcAvir.Predator = unitateAvir;
    ntcAvir.Primitor = unitateAvir;
    var linieNtcAvir = os.CreateObject<NotaContabilaDetaliu>();
    linieNtcAvir.Document = ntcAvir;
    linieNtcAvir.TipMaterial = tipTrz;
    linieNtcAvir.Descriere = "Probă: repartitor explicit = contul propriu al viramentului";
    linieNtcAvir.ContDebit = cont581;
    linieNtcAvir.ContCredit = cont531;
    linieNtcAvir.RepartitorDebit = casaVir;
    linieNtcAvir.RepartitorCredit = casaVir;
    linieNtcAvir.CodEconomic = codEcVir;
    linieNtcAvir.Valoare = 200m;
    os.CommitChanges();
    MotorOperare.Opereaza(os, ntcAvir);
    Check("Review advers F1 (premisa): nota operată CHIAR poartă contul propriu al viramentului ca CONTRAPARTIDĂ stingibilă — invariantul de contrapartidă (31d) n-ar fi oprit-o",
        ntcAvir.Stare == StareDocument.Operat
        && ntcAvir.CapacitateStingere(os).ContainsKey(casaVir.ID));
    CheckRefuza("Review advers F1: nota NU poate stinge un picior de virament — rolul de STINS e polimorf (`PoateFiStins`), altfel piciorul rămânea blocat la anulare/storno fără explicație în UI",
        () => ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Plata>(idVirPlt), 50m));
    CheckRefuza("Review advers F1: nici celălalt picior (INC) nu se lasă stins — gardul e pe TIP, nu pe direcție",
        () => ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Incasare>(idVirInc), 50m));
    ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Plata>(idPltStins), 50m);
    Check("Review advers F1 (nonregresie): ACEEAȘI notă stinge normal plata obișnuită de pe același cont propriu — gardul țintește viramentul, nu mecanismul",
        ImperechereService.Ramas(os, idPltStins) == 20m
        && os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentStingatorId == ntcAvir.ID) == 1);

    // Corolarul F1: proiecția de REST chiar întorcea picioarele de virament când
    // e filtrată pe un cont propriu (PLT → contrapartida e primitorul, INC →
    // predatorul). Contractul (F7-D7) susținea că e imposibil structural; nu e.
    var restPeCasa = ImperecheriProiectii.DocumenteCuRest(os, casaVir.ID).ToList();
    var restPeBanca = ImperecheriProiectii.DocumenteCuRest(os, bancaVir.ID).ToList();
    var restPePartener = ImperecheriProiectii.DocumenteCuRest(os, partenerVir.ID).ToList();
    Check("Review advers F1 (corolar): `DocumenteCuRest` NU mai întoarce picioarele de virament pe niciunul dintre conturile proprii care le sunt contrapartidă (INC pe casă, PLT pe bancă)",
        !restPeCasa.Any(r => r.DocumentId == idVirInc)
        && !restPeBanca.Any(r => r.DocumentId == idVirPlt)
        // Proba NU e vacuă: ambele picioare îndeplinesc TOATE celelalte criterii
        // ale proiecției (Operat, Rest = totalul lor pe veci) — le scoate exclusiv
        // anti-join-ul nou.
        && ImperechereService.Ramas(os, idVirPlt) == 500m
        && ImperechereService.Ramas(os, idVirInc) == 500m);
    Check("Review advers F1 (corolar, nonregresie): anti-join-ul e ȚINTIT — plata obișnuită rămâne candidat pe contrapartida ei, cu restul calculat de proiecție egal cu al serviciului",
        restPePartener.Count(r => r.DocumentId == idPltStins) == 1
        && restPePartener.Single(r => r.DocumentId == idPltStins).Rest == ImperechereService.Ramas(os, idPltStins));

    // Legătura se desface înaintea scenariului de anulare/storno: gardianul de
    // imperecheri (31d) ar refuza anularea plății martor, iar curățenia finală
    // are nevoie de documente fără dependenți.
    os.Delete(os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == ntcAvir.ID).ToList());
    os.CommitChanges();
    MotorOperare.AnuleazaOperarea(os, ntcAvir);
    OperareApi.AnuleazaOperarea(os, idPltStins);

    // ── (g) Ancora 7: anulare, regenerare, storno ───────────────────────────
    OperareApi.AnuleazaOperarea(os, idVirInc);
    Check("F7-D9 ancora 7: anularea laturii pereche o readuce în Draft, fără rânduri proprii",
        TrezorerieApply.Citeste<Incasare>(os, idVirInc) is { Stare: "Draft" }
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idVirInc));
    OperareApi.AnuleazaOperarea(os, idVirPlt);
    Check("F7-D9 ancora 7: anularea SURSEI șterge draftul autogenerat (gardienii de grup existenți — artefact al operării, se regenerează la re-operare)",
        TrezorerieApply.Citeste<Incasare>(os, idVirInc) == null
        && TrezorerieApply.Citeste<Plata>(os, idVirPlt) is { Stare: "Draft", Copii.Count: 0 }
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idVirPlt));

    var rezReoperare = OperareApi.Opereaza(os, idVirPlt);
    var idVirInc2 = rezReoperare.ConexId ?? Guid.Empty;
    Check("Risc pin-uit (ping-pong): re-operarea sursei după anulare regenerează EXACT o latură pereche, nu una în plus",
        idVirInc2 != Guid.Empty && idVirInc2 != idVirInc
        && TrezorerieApply.Citeste<Plata>(os, idVirPlt).Copii.Count == 1);
    OperareApi.Opereaza(os, idVirInc2);
    // Piciorul-copil se stornează ÎNTÂI: gardianul de grup refuză stornarea
    // sursei cât timp copilul e Operat (nu și când e Stornat).
    OperareApi.Storneaza(os, idVirInc2, new DateOnly(2026, 4, 20));
    OperareApi.Storneaza(os, idVirPlt, new DateOnly(2026, 4, 20));
    var idsPicioare2 = new List<Guid> { idVirPlt, idVirInc2 };
    var noteStorno = os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && idsPicioare2.Contains(r.DocumentId.Value))
        .Select(r => new { r.ContDebitId, r.ContCreditId, r.Valoare, r.Storno, r.Data }).ToList();
    var sold581Storno = noteStorno.Where(r => r.ContDebitId == cont581.ID).Sum(r => r.Valoare)
        - noteStorno.Where(r => r.ContCreditId == cont581.ID).Sum(r => r.Valoare);
    Check("F7-D9 ancora 7: storno pe AMBELE picioare — câte un rând invers marcat Storno la data stornării, corespondența neschimbată, 581 rămâne închis la 0",
        TrezorerieApply.Citeste<Plata>(os, idVirPlt) is { Stare: "Stornat" }
        && TrezorerieApply.Citeste<Incasare>(os, idVirInc2) is { Stare: "Stornat" }
        && noteStorno.Count == 4 && noteStorno.Count(r => r.Storno) == 2
        && noteStorno.Where(r => r.Storno).All(r => r.Valoare == -500m && r.Data == new DateOnly(2026, 4, 20))
        && sold581Storno == 0m);

    CurataAvir(os);
    Check("Curățenie finală felia virament (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAvir))
        && !os.GetObjectsQuery<TipMaterial>().Any(t => t.Cod.StartsWith(MarcajAvir))
        && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajAvir)));
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
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
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
    var linieAvans = os.CreateObject<DocumentTrezorerieDetaliu>();
    linieAvans.Document = avans;
    linieAvans.TipMaterial = tipTrz;
    linieAvans.Valoare = 100m;
    linieAvans.CodEconomicId = codEc.ID; // politica PLT + defalcarea E (531/542)
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
    linieDeplasare.CodEconomicId = codEc.ID;
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
    linieProtocol.CodEconomicId = codEc.ID;
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
        notaDeplasare.DimensiuniDebit().RepartitorId == angajat.ID
        && notaDeplasare.DimensiuniDebit().CodEconomicId == codEc.ID
        && notaProtocol.DimensiuniDebit().RepartitorId == mag1.ID);
    Check("Dimensiuni credit: 542 pe TITULAR (default polimorf, nu primitorul SEDIU)",
        note.All(n => n.DimensiuniCredit().RepartitorId == angajat.ID));

    // --- Tip fără cont și fără postare explicită = refuz clar; fallback 542 ---
    var dec2 = os.CreateObject<Decont>();
    dec2.Data = new DateOnly(2026, 3, 9);
    dec2.Predator = angajat2; // fără ContImplicit — exersează fallback-ul regulii
    dec2.Primitor = sediu;
    var linieTehnica = os.CreateObject<DecontDetaliu>();
    linieTehnica.Document = dec2;
    linieTehnica.TipMaterial = tipTrz; // TRZ nu are ContImplicit
    linieTehnica.PretUnitar = 20m;
    linieTehnica.CodEconomicId = codEc.ID;
    os.CommitChanges();
    CheckRefuza("Tip fără cont implicit și linie fără cont explicit → refuz",
        () => MotorOperare.Opereaza(os, dec2));
    linieTehnica.ContDebit = cont623;
    os.CommitChanges();
    MotorOperare.Opereaza(os, dec2);
    Check("Angajat fără ContImplicit → creditul cade pe fallback-ul 542.01.00",
        Note(dec2).Single().ContCreditId == cont542.ID
        && Note(dec2).Single().DimensiuniCredit().RepartitorId == angajat2.ID);

    // --- Lanțul avans ↔ decont ↔ regularizare prin imperechere (31d) ---
    var impDecont = ImperechereService.Imperecheaza(os, avans, dec, 53.8m);
    Check("Imperechere avans↔decont: decontul stins integral, avansul cu rest 46,2",
        ImperechereService.Ramas(os, dec.ID) == 0m && ImperechereService.Ramas(os, avans.ID) == 46.2m);
    var regularizare = os.CreateObject<Incasare>();
    regularizare.Data = new DateOnly(2026, 3, 15);
    regularizare.Predator = angajat;
    regularizare.Primitor = casa;
    regularizare.TipInstrument = TipInstrumentPlata.DispozitieCasa;
    var linieReg = os.CreateObject<DocumentTrezorerieDetaliu>();
    linieReg.Document = regularizare;
    linieReg.TipMaterial = tipTrz;
    linieReg.Valoare = 46.2m;
    linieReg.CodEconomicId = codEc.ID; // casa (531) cere E
    os.CommitChanges();
    MotorOperare.Opereaza(os, regularizare);
    ImperechereService.Imperecheaza(os, regularizare, avans, 46.2m);
    Check("Regularizarea stinge restul: avansul asignat pe AMBELE roluri, rest 0",
        ImperechereService.Ramas(os, avans.ID) == 0m && ImperechereService.Ramas(os, regularizare.ID) == 0m);

    // F3-D3: panoul de stingeri pe documentul aflat pe AMBELE roluri — avansul
    // stinge decontul ȘI e stins de regularizare, deci `EsteStingator` diferă
    // între rândurile ACELUIAȘI panou (cazul pe care un DTO cu o singură
    // „coloană" de imperecheri l-ar fi ratat).
    var stingeriAvans = ImperechereApply.Stingeri(os, avans.ID);
    var randStinge = stingeriAvans.Imperecheri.Single(i => i.EsteStingator);
    var randStins = stingeriAvans.Imperecheri.Single(i => !i.EsteStingator);
    Check("StingeriDto pe avans: două rânduri cu roluri OPUSE (stinge DEC 53,8; e stins de INC 46,2), numerele din serviciu",
        stingeriAvans is { Total: 100m, Asignat: 100m, Ramas: 0m }
        && stingeriAvans.Imperecheri.Count == 2
        && randStinge is { CelalaltTip: "DEC", Suma: 53.8m } && randStinge.CelalaltDocumentId == dec.ID
        && randStins is { CelalaltTip: "INC", Suma: 46.2m } && randStins.CelalaltDocumentId == regularizare.ID);
    Check("…iar din capătul celălalt: panoul decontului vede avansul ca stingător de tip „PLT”, cu rest 0",
        ImperechereApply.Stingeri(os, dec.ID) is { Total: 53.8m, Ramas: 0m } panouDec
        && panouDec.Imperecheri.Single() is { EsteStingator: false, CelalaltTip: "PLT", Suma: 53.8m });

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
        randViramente.DimensiuniDebit().RepartitorId == mag1.ID
        && randStorno.DimensiuniDebit().RepartitorId == sediu.ID);
    Check("Dimensiuni credit: default polimorf (primitor) pe ambele linii",
        noteNtc.All(n => n.DimensiuniCredit().RepartitorId == unitate.ID));

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
    linieDefalcare.CodEconomicId = codEc.ID;
    os.CommitChanges();
    MotorOperare.Opereaza(os, ntcDefalcare);
    Check("628 cu cod economic cules pe linie → operare acceptată, dimensiunea pe latura contului",
        ntcDefalcare.Stare == StareDocument.Operat
        && Note(ntcDefalcare).Single().DimensiuniDebit().CodEconomicId == codEc.ID);

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

// ========== Scenariul e2e 1C-a: retururile la BUGETAR (tipuri inerte) ==========
// Ancorele RLF/RDC trăiesc în nucleu (ambele profiluri), politicile sunt DATE de
// profil: la bugetar nu există reguli/numerotare/TVA implicit, deci tipurile sunt
// inerte — ca DSC/ITV/ASM/BPR (decizia 29). Extensia de motor `PastreazaSemn` e
// aditivă și inertă la default false: nicio regulă existentă nu o poartă.
using (var os = provider.CreateObjectSpace()) {
    var tipRlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
    var tipRdc = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
    Check("Seed bugetar: ancorele TipDocument RLF/RDC există (nucleu), cu ClrType-urile claselor",
        tipRlf != null && tipRlf.ClrType == nameof(ReturFurnizor)
        && tipRdc != null && tipRdc.ClrType == nameof(ReturClient));
    bool Inert(TipDocument tip) =>
        !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tip.ID)
        && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tip.ID)
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tip.ID) == null
        && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tip.ID) == null
        && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tip.ID) == null
        && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tip.ID) == null
        && tip.TipTvaImplicitId == null;
    Check("Bugetar: RLF și RDC sunt tipuri INERTE — fără reguli de stoc/contare, numerotare, politici sau TVA implicit",
        Inert(tipRlf) && Inert(tipRdc));
    Check("Extensia PastreazaSemn e inertă la bugetar: nicio regulă de contare existentă nu o poartă",
        !os.GetObjectsQuery<RegulaContare>().Any(r => r.PastreazaSemn));
}

// ========= Scenariul e2e pasul 5 / felia 5: Api NIR scriere (F5-D8) =========
// RECEPȚIA FĂRĂ FACTURĂ, parcursă prin contractul feliei: WriteDto →
// `NirApply.Aplica` → `Citeste` → dry-run → `OperareApi.Opereaza` → registre.
// Fluxul n-a existat nicăieri până la felia asta (nici în XAF, nici prin API):
// `NirDetaliu` n-avea `ProdusId`, iar `CreeazaLot` n-avea niciun apelant din UI
// — exact golul de model pe care GATE-ul l-a închis pe FCT (53a).
//
// Ce exersează în plus față de blocul „Api FCT + NIR (F2-D6)":
//   * lotul se naște pe linia PROPRIE a NIR-ului (nu pe a facturii), din
//     `ProdusId`, la `Aplica` — seam-ul `LoturiCulegereService` generalizat (F5-D3);
//   * `Valoare = PretUnitar × Cantitate` materializată LA CULEGERE, cu formula
//     GEAMĂNĂ celei din `NIR.PregatesteOperare` (F5-D6a);
//   * TESTUL-ANCORĂ AL FELIEI (riscul 1 din contract): PUT pe NIR-ul CONEX —
//     cu produsul completat și cantitatea redusă — NU naște al doilea lot;
//   * refuzurile F5-D7/D7b, fiecare fără rânduri-fantomă (33d).
// Rulează pe profilul BUGETAR: NIR n-are `PoliticaTva` în niciun profil (F5-D5),
// deci nimic din felie nu cere profilul privat.
const string MarcajApiNir = "E2E-API-NIR";

void CurataApiNir(IObjectSpace os) {
    // Documentele probei se găsesc prin marfa lor: NIR-ul n-are număr cules
    // (seria e server-owned), deci ancora e produsul marcat — prin loturile lui
    // și prin `NirDetaliu.ProdusId`. Facturile probei se găsesc pe număr.
    var idsDoc = os.GetObjectsQuery<DocumentDetaliu>()
        .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiNir))
        .Select(d => d.DocumentId).ToList();
    idsDoc.AddRange(os.GetObjectsQuery<NirDetaliu>()
        .Where(d => d.Produs.Cod.StartsWith(MarcajApiNir))
        .Select(d => d.DocumentId).ToList());
    idsDoc.AddRange(os.GetObjectsQuery<FacturaIntrare>()
        .Where(d => d.Numar.StartsWith("E2E-ANF")).Select(d => d.ID).ToList());
    idsDoc = idsDoc.Distinct().ToList();
    // …plus copiii conecși (NIR-ul generat la operarea facturii).
    idsDoc.AddRange(os.GetObjectsQuery<Document>()
        .Where(d => d.DocumentSursaId != null && idsDoc.Contains(d.DocumentSursaId.Value))
        .Select(d => d.ID).ToList());
    idsDoc = idsDoc.Distinct().ToList();

    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
    os.CommitChanges();
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiNir)).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiNir)).ToList());
    os.Delete(os.GetObjectsQuery<Partener>().Where(p => p.Cod == "E2E-ANFURN").ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-ANCE").ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiNir(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

    var furnizor = os.CreateObject<Partener>();
    furnizor.Cod = "E2E-ANFURN";
    furnizor.Denumire = "Furnizor probă felia Api NIR";
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = "E2E-ANCE";
    codEc.Denumire = "Cod economic probă felia Api NIR";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajApiNir + "-A";
    produs.Denumire = "Produs A probă felia Api NIR";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    // Produs al ALTUI Tip — proba de coerență Tip-linie ↔ Produs (F5-D7).
    var produsStrain = os.CreateObject<Produs>();
    produsStrain.Cod = MarcajApiNir + "-S";
    produsStrain.Denumire = "Produs de alt Tip, probă felia Api NIR";
    produsStrain.UM = "BUC";
    produsStrain.TipMaterial = tipServicii;
    os.CommitChanges();

    var dataNir = new DateOnly(2026, 3, 12);

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
    // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
    IReadOnlyList<string> DryRunNir(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }

    // --- Apply: recepția MANUALĂ, din WriteDto (fără Numar/LotId/Valoare) ---
    var write = new NirWriteDto {
        Data = dataNir,
        PredatorId = furnizor.ID,
        PrimitorId = mag1.ID,
        Linii = {
            new NirLinieWriteDto {
                TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                Cantitate = 4m, PretUnitar = 12.5m,
                LotFabricatie = "LOT-NIR", DataExpirare = new DateOnly(2027, 6, 30),
                CodEconomicId = codEc.ID
            }
        }
    };
    var idNir = NirApply.Aplica(os, null, write);
    var citit = NirApply.Citeste(os, idNir);
    Check("Apply NIR manual → header plat, FĂRĂ număr (seria „NIR-” e server-owned, se consumă la operare — invers față de FCT)",
        citit != null && citit.Id == idNir && citit.Stare == "Draft" && citit.Numar == null
        && citit.Data == dataNir
        && citit.PredatorId == furnizor.ID && citit.PredatorDenumire == furnizor.Denumire
        && citit.PrimitorId == mag1.ID && citit.PrimitorDenumire == mag1.Denumire
        && !citit.Autogenerat && citit.DocumentSursaId == null
        && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

    var linie = citit.Linii.Single();
    var lotNascut = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == linie.Id);
    Check("PROBA FELIEI: lotul se naște pe linia PROPRIE a NIR-ului, din ProdusId — nefinalizat, în gestiunea PRIMITOARE (hook-ul GestiuneLoturiCulese)",
        lotNascut != null && lotNascut.ProdusId == produs.ID && lotNascut.GestiuneId == mag1.ID
        && lotNascut.Data == default && lotNascut.PretUnitar == 0m
        && linie.LotId == lotNascut.ID && !linie.LotStrain
        && linie.LotEticheta == lotNascut.Eticheta
        && linie.LotEticheta.Contains("(în culegere)"));
    Check("Valoarea materializată LA CULEGERE din prețul cules (F5-D6a): 4 × 12,5 = 50, Total 50 — nu 0 până la operare",
        linie.Valoare == 50m && linie.ValoareTva == 0m && citit.Total == 50m
        && linie.PretUnitar == 12.5m && linie.Cantitate == 4m);
    Check("Linia poartă produsul, atributele de lot și dimensiunea frunzei, proiectate plat",
        linie.ProdusId == produs.ID && linie.ProdusCod == produs.Cod
        && linie.ProdusDenumire == produs.Denumire
        && linie.LotFabricatie == "LOT-NIR" && linie.DataExpirare == new DateOnly(2027, 6, 30)
        && linie.CodEconomicId == codEc.ID && linie.CodEconomicCod == "E2E-ANCE"
        && linie.SursaFinantareId == null && linie.ProiectId == null
        && linie.TipTvaId == null);

    // --- Reconcilierea colecției (upsert pe Id) ---
    write.Linii[0].Id = linie.Id;
    write.Linii[0].Cantitate = 6m;
    NirApply.Aplica(os, idNir, write);
    citit = NirApply.Citeste(os, idNir);
    Check("Reconciliere pe Id: cantitatea schimbată → valoarea o urmează (6 × 12,5 = 75), ACELAȘI lot (nu un al doilea pentru aceeași linie)",
        citit.Linii.Single().Valoare == 75m && citit.Linii.Single().LotId == lotNascut.ID
        && os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == linie.Id) == 1);
    CheckRefuza("Apply NIR cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        NirApply.Aplica(os, idNir, new NirWriteDto {
            Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { new NirLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 1m } }
        }));
    CheckRefuza("Apply NIR cu același Id de linie de două ori → refuz (a doua apariție ar suprascrie tăcut prima)", () =>
        NirApply.Aplica(os, idNir, new NirWriteDto {
            Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { write.Linii[0], write.Linii[0] }
        }));
    CheckRefuza("Apply NIR cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
        NirApply.Aplica(os, idNir, new NirWriteDto {
            Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { new NirLinieWriteDto {
                TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
        }));
    NirApply.Aplica(os, idNir, write);
    Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact o linie",
        NirApply.Citeste(os, idNir).Linii.Count == 1);

    // --- Refuzurile de OPERARE (F5-D7/D7b), fiecare fără rânduri-fantomă ---
    void RefuzNir(string nume, NirLinieWriteDto linieProba) {
        var id = NirApply.Aplica(os, null, new NirWriteDto {
            Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
            Linii = { linieProba }
        });
        CheckRefuza(nume, () => OperareApi.Opereaza(os, id));
        Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
            !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == id)
            && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == id)
            && os.GetObjectByKey<NIR>(id).Stare == StareDocument.Draft);
        NirApply.Sterge(os, id);
    }

    RefuzNir("Linie de stoc FĂRĂ produs → refuz cu mesajul care spune CE SĂ FACĂ („alegeți produsul”, F5-D7)",
        new NirLinieWriteDto {
            TipMaterialId = tipMateriale.ID, Cantitate = 1m, PretUnitar = 10m, CodEconomicId = codEc.ID
        });
    RefuzNir("Produs de ALT Tip decât Tipul liniei → refuz (oglinda 53f: lotul ar ajunge în registrul altui Tip decât cel postat)",
        new NirLinieWriteDto {
            TipMaterialId = tipMateriale.ID, ProdusId = produsStrain.ID,
            Cantitate = 1m, PretUnitar = 10m, CodEconomicId = codEc.ID
        });
    RefuzNir("Preț unitar 0 pe linia care își NAȘTE lotul → refuz (F5-D7b: altfel lotul intră în stoc cu preț 0 și FIFO îl propagă în toate ieșirile)",
        new NirLinieWriteDto {
            TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 1m, PretUnitar = 0m, CodEconomicId = codEc.ID
        });

    // --- Dry-run, apoi comanda ---
    Check("Dry-run (Valideaza) pe draftul NIR manual valid → listă goală",
        DryRunNir(idNir).Count == 0);
    Check("Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și fără lot finalizat",
        os.GetObjectByKey<NIR>(idNir).Stare == StareDocument.Draft
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idNir)
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idNir)
        && os.GetObjectByKey<Lot>(lotNascut.ID).PretUnitar == 0m);

    var rezNir = OperareApi.Opereaza(os, idNir);
    Check("OperareApi.Opereaza pe NIR manual → Operat, cu număr din politica proprie (seria NIR-), fără conex; affordances inversate (nu mai e editabil)",
        rezNir.StareNoua == StareDocument.Operat && rezNir.ConexId == null
        && NirApply.Citeste(os, idNir) is { Numar: not null, PoateEdita: false, PoateOpera: false,
            PoateAnula: true, PoateStorna: true }
        && NirApply.Citeste(os, idNir).Numar.StartsWith("NIR-"));
    var lotFinalizat = os.GetObjectByKey<Lot>(lotNascut.ID);
    Check("Motorul FINALIZEAZĂ lotul născut pe linia NIR-ului: preț 12,5 (75/6), data documentului, atributele culese pe linie",
        lotFinalizat.PretUnitar == 12.5m && lotFinalizat.Data == dataNir
        && lotFinalizat.LotFabricatie == "LOT-NIR"
        && lotFinalizat.DataExpirare == new DateOnly(2027, 6, 30));
    var stocNirManual = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idNir).ToList();
    Check("NIR manual → +6/+75 Magazie pe gestiunea primitoare, pe lotul propriu",
        stocNirManual.Count == 1 && stocNirManual[0].TipStoc == TipStoc.Magazie
        && stocNirManual[0].RepartitorId == mag1.ID && stocNirManual[0].LotId == lotFinalizat.ID
        && stocNirManual[0].Cantitate == 6m && stocNirManual[0].Valoare == 75m);
    var noteNirManual = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == idNir).ToList();
    Check("NIR manual contează recepția ca oricare alta: 302.01.00 = 401, 75 (regula de oprire a feliei — registrele nu disting proveniența)",
        noteNirManual.Count == 1 && noteNirManual[0].ContDebitId == tipMateriale.ContImplicitId
        && noteNirManual[0].ContCreditId == cont401.ID && noteNirManual[0].Valoare == 75m);
    CheckRefuza("Apply peste NIR Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => NirApply.Aplica(os, idNir, write));
    CheckRefuza("Sterge peste NIR Operat → același refuz de domeniu",
        () => NirApply.Sterge(os, idNir));

    // --- TESTUL-ANCORĂ: PUT pe NIR-ul CONEX nu naște al doilea lot (F5-D3) ---
    // Marfa e deja recepționată pe lotul născut la culegerea FACTURII; un al
    // doilea lot ar dubla stocul invizibil pentru gardianul de sold (lotul nou
    // pornește de la zero, deci nicio verificare nu devine negativă).
    var idFct = FacturaIntrareApply.Aplica(os, null, new FacturaIntrareWriteDto {
        Numar = "E2E-ANF1", Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
        Linii = { new FacturaIntrareLinieWriteDto {
            TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 10m, PretUnitar = 5m, TipTvaId = cap19.ID, CodEconomicId = codEc.ID } }
    });
    var idNirConex = OperareApi.Opereaza(os, idFct).ConexId.Value;
    var conex = NirApply.Citeste(os, idNirConex);
    var lotFct = os.GetObjectByKey<Lot>(conex.Linii[0].LotId.Value);
    Check("NIR conex: DRAFT AUTOGENERAT deci EDITABIL (F5-D8b — recepția parțială e flux de producție), cu lot STRĂIN pe linie, fără produs și fără preț propriu",
        conex.PoateEdita && conex.Autogenerat
        && conex.Linii.Count == 1 && conex.Linii[0].LotStrain
        && conex.Linii[0].ProdusId == null && conex.Linii[0].PretUnitar == 0m
        && lotFct.LinieIntrareId != conex.Linii[0].Id && lotFct.PretUnitar == 5.95m);

    var loturiInainte = os.GetObjectsQuery<Lot>().Count(l => l.Produs.Cod.StartsWith(MarcajApiNir));
    // Cazul EXACT al riscului 1: PUT cu produsul COMPLETAT (clientul l-ar putea
    // trimite) și cantitatea redusă (recepție parțială — marfa primită e mai
    // puțină decât cea facturată).
    NirApply.Aplica(os, idNirConex, new NirWriteDto {
        Data = conex.Data, PredatorId = conex.PredatorId, PrimitorId = conex.PrimitorId,
        Linii = { new NirLinieWriteDto {
            Id = conex.Linii[0].Id, TipMaterialId = conex.Linii[0].TipMaterialId,
            ProdusId = produs.ID, Cantitate = 4m, PretUnitar = 99m,
            CodEconomicId = conex.Linii[0].CodEconomicId } }
    });
    var conexDupa = NirApply.Citeste(os, idNirConex);
    Check("TESTUL-ANCORĂ (riscul 1): PUT pe NIR-ul conex cu produs completat → NICIUN al doilea lot, linia referă tot lotul facturii",
        os.GetObjectsQuery<Lot>().Count(l => l.Produs.Cod.StartsWith(MarcajApiNir)) == loturiInainte
        && conexDupa.Linii[0].LotId == lotFct.ID && conexDupa.Linii[0].LotStrain
        && !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId == conexDupa.Linii[0].Id));
    Check("Recepția PARȚIALĂ pe conex: valoarea se recalculează din prețul LOTULUI (4 × 5,95 = 23,8), prețul cules pe linie e IGNORAT (F5-D6b)",
        conexDupa.Linii[0].Cantitate == 4m && conexDupa.Linii[0].Valoare == 23.8m
        && conexDupa.Total == 23.8m);
    Check("PUT-ul pe conex nu atinge TipTva-ul informativ clonat din factură (F5-D5: NIR-ul nu culege TVA)",
        conexDupa.Linii[0].TipTvaId == cap19.ID && conexDupa.Linii[0].TipTvaCod == "CAP19");
    // Review advers F3: „inert" trebuie să însemne GOLIT, nu doar nefolosit —
    // altfel produsul rămâne persistat pe linia conexă și îl citește validarea de
    // coerență Tip↔Produs, care poate face NIR-ul permanent ne-operabil printr-un
    // câmp pe care UI-ul îl afișează read-only.
    Check("Produsul trimis pe o linie cu lot STRĂIN e GOLIT, nu doar ignorat (review advers F3)",
        conexDupa.Linii[0].ProdusId == null);
    CheckRefuza("Sterge pe NIR-ul CONEX (draft autogenerat) → refuz: e artefactul operării facturii, poartă singura postare a datoriei (review advers F2)",
        () => NirApply.Sterge(os, idNirConex));
    Check("Refuzul de mai sus nu a atins documentul: conexul e viu, cu linia lui",
        NirApply.Citeste(os, idNirConex) is { } viu && viu.Linii.Count == 1);

    OperareApi.Opereaza(os, idNirConex);
    var stocConex = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idNirConex).ToList();
    Check("NIR conex operat după PUT: +4/+23,8 pe lotul facturii — gardul de preț (F5-D7b) NU atinge liniile cu lot străin, acolo prețul e al lotului",
        stocConex.Count == 1 && stocConex[0].LotId == lotFct.ID
        && stocConex[0].Cantitate == 4m && stocConex[0].Valoare == 23.8m);

    // --- Sterge: draftul manual și lotul lui mor împreună ---
    var idNir2 = NirApply.Aplica(os, null, new NirWriteDto {
        Data = dataNir, PredatorId = furnizor.ID, PrimitorId = mag1.ID,
        Linii = { new NirLinieWriteDto {
            TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 2m, PretUnitar = 8m, CodEconomicId = codEc.ID } }
    });
    var idLotDraft = NirApply.Citeste(os, idNir2).Linii[0].LotId.Value;
    NirApply.Sterge(os, idNir2);
    Check("Sterge pe draftul NIR manual: documentul, linia și LOTUL în culegere dispar împreună",
        NirApply.Citeste(os, idNir2) == null
        && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idNir2)
        && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraft));

    CurataApiNir(os);
    Check("Curățenie finală felia Api NIR (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiNir))
        && !os.GetObjectsQuery<Partener>().Any(p => p.Cod == "E2E-ANFURN")
        && !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-ANF"))
        && os.GetObjectByKey<NIR>(idNir) == null);
}

// ========= Scenariul e2e pasul 5 / felia 6: Api BCS scriere (F6-D11) =========
// Consumul cules manual, parcurs prin contractul feliei: WriteDto →
// `BonConsumApply.Aplica` → `Citeste`/`Lista` → dry-run → `OperareApi.Opereaza`
// → cele DOUĂ registre de stoc (−Magazie predator, +Consum primitor — 27a).
//
// Ce exersează în plus față de blocul e2e „3c: BonConsum" (care probează
// MOTORUL, construind documentele direct în ObjectSpace):
//   * culegerea prin AGREGAT — reconcilierea liniilor pe `Id`, refuzurile de
//     payload, ștergerea agregatului;
//   * `Valoare` materializată LA CULEGERE din prețul lotului (F6-D6), cu
//     golirea la 0 a liniei rămase fără lot — ce hook-ul de operare nu face;
//   * seria „BCS-" NECONSUMATĂ la un refuz de operare (F6-D4 + GATE D6);
//   * affordances oneste (F6-D7).
// Rulează pe profilul BUGETAR: BCS n-are `PoliticaTva` în niciun profil (F6-D5).
const string MarcajApiBcs = "E2E-API-BCS";

void CurataApiBcs(IObjectSpace os) {
    // Documentele probei se găsesc prin laturi (BCS n-are număr cules — seria e
    // server-owned), loturile și produsul prin marcaj.
    var idsDoc = os.GetObjectsQuery<BonConsum>()
        .Where(d => d.Predator.Cod.StartsWith(MarcajApiBcs) || d.Primitor.Cod.StartsWith(MarcajApiBcs))
        .Select(d => d.ID).ToList();
    idsDoc.AddRange(os.GetObjectsQuery<DocumentDetaliu>()
        .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiBcs))
        .Select(d => d.DocumentId).ToList());
    idsDoc = idsDoc.Distinct().ToList();

    var idsLot = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiBcs))
        .Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>()
        .Where(r => idsLot.Contains(r.LotId) || (r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value))).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
    os.CommitChanges();
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiBcs)).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiBcs)).ToList());
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiBcs)).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiBcs(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

    // Locul de consum e CALITATE transversală (27b), nu clasă: orice repartitor
    // intern o poate purta. Latura o validează motorul, nu tierul.
    var loc = os.CreateObject<UnitateInterna>();
    loc.Cod = MarcajApiBcs + "-LOC";
    loc.Denumire = "Loc de consum probă felia Api BCS";
    loc.Calitati = CalitateRepartitor.LocConsum;
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajApiBcs + "-A";
    produs.Denumire = "Produs A probă felia Api BCS";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    os.CommitChanges();

    var dataBcs = new DateOnly(2026, 3, 18);
    // Lotul de consumat + soldul lui de deschidere: BCS DESCARCĂ loturi, nu le
    // naște (liniile lui nu declară `ILinieCareNasteLot`).
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
    deschidere.Cantitate = 20m;
    deschidere.Valoare = 200m;
    os.CommitChanges();

    decimal SoldBcs(Repartitor r, TipStoc tipStoc) => StocService.Sold(os, new CheieStoc(lot.ID, r.ID, tipStoc));
    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
    // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
    IReadOnlyList<string> DryRunBcs(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }
    int SerieBcs() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "BCS").UrmatorulNumar;

    // --- Apply: consumul cules, fără Numar/Valoare în payload ---
    var writeBcs = new BcsWriteDto {
        Data = dataBcs,
        PredatorId = mag1.ID,
        PrimitorId = loc.ID,
        Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 4m } }
    };
    var idBcs = BonConsumApply.Aplica(os, null, writeBcs);
    var citit = BonConsumApply.Citeste(os, idBcs);
    Check("Apply BCS → header plat, FĂRĂ număr (seria „BCS-” e server-owned, se consumă la operare)",
        citit != null && citit.Id == idBcs && citit.Stare == "Draft" && citit.Numar == null
        && citit.Data == dataBcs
        && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
        && citit.PrimitorId == loc.ID && citit.PrimitorDenumire == loc.Denumire
        && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);
    Check("Valoarea consumului materializată LA CULEGERE din prețul LOTULUI (F6-D6): 4 × 10 = 40, Total 40 — nu 0 până la operare",
        citit.Linii.Single().Valoare == 40m && citit.Total == 40m
        && citit.Linii.Single().Cantitate == 4m
        && citit.Linii.Single().TipMaterialCod == "302.01.00"
        && citit.Linii.Single().LotId == lot.ID
        && citit.Linii.Single().LotEticheta == lot.Eticheta);
    var randBcs = BonConsumApply.Lista(os).Single(d => d.Id == idBcs);
    Check("Lista BCS: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
        randBcs.Stare == "Draft" && randBcs.Total == 40m && randBcs.Numar == null
        && randBcs.PredatorDenumire == mag1.Denumire && randBcs.PrimitorDenumire == loc.Denumire);

    // --- Reconcilierea colecției (upsert pe Id) + golirea valorii fără lot ---
    var idLinieBcs = citit.Linii.Single().Id;
    writeBcs.Linii[0].Id = idLinieBcs;
    writeBcs.Linii[0].Cantitate = 6m;
    BonConsumApply.Aplica(os, idBcs, writeBcs);
    Check("Reconciliere pe Id: cantitatea schimbată → valoarea o urmează (6 × 10 = 60)",
        BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);
    BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
        Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
        Linii = { new BcsLinieWriteDto { Id = idLinieBcs, TipMaterialId = tipMateriale.ID, Cantitate = 6m } }
    });
    Check("Lotul scos de pe linie → valoarea se GOLEȘTE la 0 (valoarea veche ar minți pe ecran — ce hook-ul de operare nu face)",
        BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 0m
        && BonConsumApply.Citeste(os, idBcs).Linii.Single().LotId == null);
    BonConsumApply.Aplica(os, idBcs, writeBcs);
    Check("Lotul repus → valoarea revine (6 × 10 = 60)",
        BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);

    CheckRefuza("Apply BCS cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
            Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
            Linii = { new BcsLinieWriteDto { Id = Guid.NewGuid(), TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
        }));
    CheckRefuza("Apply BCS cu același Id de linie de două ori → refuz (a doua apariție ar suprascrie tăcut prima)", () =>
        BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
            Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
            Linii = { writeBcs.Linii[0], writeBcs.Linii[0] }
        }));
    CheckRefuza("Apply BCS cu cantitate în afara scării numeric(18,3) → refuz de domeniu, nu DbUpdateException", () =>
        BonConsumApply.Aplica(os, idBcs, new BcsWriteDto {
            Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
            Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 0.0001m } }
        }));
    BonConsumApply.Aplica(os, idBcs, writeBcs);
    Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact o linie",
        BonConsumApply.Citeste(os, idBcs).Linii.Count == 1);

    // --- Refuzurile de OPERARE, fiecare fără rânduri-fantomă și fără serie consumată ---
    var serieInainteBcs = SerieBcs();
    void RefuzBcs(string nume, Guid predatorId, Guid primitorId, BcsLinieWriteDto linieProba) {
        var id = BonConsumApply.Aplica(os, null, new BcsWriteDto {
            Data = dataBcs, PredatorId = predatorId, PrimitorId = primitorId, Linii = { linieProba }
        });
        CheckRefuza(nume, () => OperareApi.Opereaza(os, id));
        Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
            !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == id)
            && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == id)
            && os.GetObjectByKey<BonConsum>(id).Stare == StareDocument.Draft
            && os.GetObjectByKey<BonConsum>(id).Numar == null);
        BonConsumApply.Sterge(os, id);
    }

    RefuzBcs("Laturi inversate (predator fără gestiune, primitor fără LocConsum) → refuz de domeniu la operare",
        loc.ID, mag1.ID,
        new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 1m });
    RefuzBcs("Linie de consum FĂRĂ lot → refuz („descărcarea e pe lot” — draftul avea voie să fie incomplet, operarea nu)",
        mag1.ID, loc.ID,
        new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, Cantitate = 1m });
    RefuzBcs("Cantitate ≤ 0 pe linia de consum → refuz",
        mag1.ID, loc.ID,
        new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 0m });
    RefuzBcs("Consum peste disponibil → refuz al gardianului de sold",
        mag1.ID, loc.ID,
        new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 999m });
    Check("Seria „BCS-” NU se consumă la refuz (F6-D4 + GATE D6: numărul se asignează abia la materializare)",
        SerieBcs() == serieInainteBcs);

    // --- Dry-run, apoi comanda ---
    Check("Dry-run (Valideaza) pe draftul BCS valid → listă goală", DryRunBcs(idBcs).Count == 0);
    Check("Dry-run-ul NU materializează nimic: documentul rămâne Draft, fără registre și fără număr",
        os.GetObjectByKey<BonConsum>(idBcs).Stare == StareDocument.Draft
        && os.GetObjectByKey<BonConsum>(idBcs).Numar == null
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idBcs));

    var rezBcs = OperareApi.Opereaza(os, idBcs);
    citit = BonConsumApply.Citeste(os, idBcs);
    Check("OperareApi.Opereaza pe BCS → Operat, cu număr din politica proprie (seria BCS-), fără conex; affordances inversate",
        rezBcs.StareNoua == StareDocument.Operat && rezBcs.ConexId == null
        && citit.Numar?.StartsWith("BCS-") == true && citit.DataOperare != null
        && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna
        && SerieBcs() == serieInainteBcs + 1);
    var stocBcs = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idBcs).ToList();
    Check("BCS operat → DOUĂ registre simultan: −6/−60 Magazie pe gestiune, +6/+60 Consum pe locul de consum (27a)",
        stocBcs.Count == 2
        && stocBcs.Any(r => r.TipStoc == TipStoc.Magazie && r.RepartitorId == mag1.ID
            && r.Cantitate == -6m && r.Valoare == -60m)
        && stocBcs.Any(r => r.TipStoc == TipStoc.Consum && r.RepartitorId == loc.ID
            && r.Cantitate == 6m && r.Valoare == 60m));
    Check("Solduri după operare: Magazie 14, Consum 6",
        SoldBcs(mag1, TipStoc.Magazie) == 14m && SoldBcs(loc, TipStoc.Consum) == 6m);
    Check("Valoarea culeasă e cea postată: hook-ul de operare rescrie aceeași formulă (geamăna F6-D6)",
        BonConsumApply.Citeste(os, idBcs).Linii.Single().Valoare == 60m);
    CheckRefuza("Apply peste BCS Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => BonConsumApply.Aplica(os, idBcs, writeBcs));
    CheckRefuza("Sterge peste BCS Operat → același refuz de domeniu",
        () => BonConsumApply.Sterge(os, idBcs));

    // --- Anulare (BCS e frunză în graful de dependențe — 27d) și storno ---
    Check("Anulare prin API → Draft + solduri revenite (Magazie 20, Consum 0)",
        OperareApi.AnuleazaOperarea(os, idBcs).StareNoua == StareDocument.Draft
        && SoldBcs(mag1, TipStoc.Magazie) == 20m && SoldBcs(loc, TipStoc.Consum) == 0m
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idBcs));
    OperareApi.Opereaza(os, idBcs);
    Check("Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete revenite",
        OperareApi.Storneaza(os, idBcs, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
        && os.GetObjectsQuery<RegistruStoc>().Count(r => r.DocumentId == idBcs) == 4
        && os.GetObjectsQuery<RegistruStoc>().Count(r => r.DocumentId == idBcs && r.Storno) == 2
        && SoldBcs(mag1, TipStoc.Magazie) == 20m && SoldBcs(loc, TipStoc.Consum) == 0m);

    // --- Sterge: draftul dispare cu tot cu linii, lotul NU (e al altcuiva) ---
    var idBcs2 = BonConsumApply.Aplica(os, null, new BcsWriteDto {
        Data = dataBcs, PredatorId = mag1.ID, PrimitorId = loc.ID,
        Linii = { new BcsLinieWriteDto { TipMaterialId = tipMateriale.ID, LotId = lot.ID, Cantitate = 1m } }
    });
    BonConsumApply.Sterge(os, idBcs2);
    Check("Sterge pe draftul BCS: documentul și liniile dispar, dar LOTUL rămâne (consumul nu naște loturi, îl descarcă)",
        BonConsumApply.Citeste(os, idBcs2) == null
        && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idBcs2)
        && os.GetObjectByKey<Lot>(lot.ID) != null);

    CurataApiBcs(os);
    Check("Curățenie finală felia Api BCS (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiBcs))
        && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiBcs))
        && os.GetObjectByKey<BonConsum>(idBcs) == null);
}

// ========= Scenariul e2e pasul 5 / felia 6: Api LDI scriere (F6-D11) =========
// Inventarierea culeasă manual, prin contractul feliei: WriteDto →
// `ListaDiferenteInventarApply.Aplica` → `Citeste`/`Lista` → dry-run →
// `OperareApi.Opereaza` → registre. Singurul tip BIDIRECȚIONAL: plusul NAȘTE
// lotul (ca o recepție manuală), minusul descarcă unul existent.
//
// TESTUL-ANCORĂ AL FELIEI (F6-D2): lotul plusului se naște în gestiunea
// INVENTARIATĂ — PREDATORUL, prin hook-ul `GestiuneLoturiCulese`. Default-ul
// bazei e primitorul, iar primitorul LDI e COMISIA (nu e `Gestiune`), deci fără
// override serviciul ar tăcea pentru totdeauna și mesajul „alegeți produsul" ar
// fi neîndeplinibil — exact golul pe care F5 l-a închis pe NIR.
// Rulează pe profilul BUGETAR: LDI n-are `PoliticaTva` în niciun profil (F6-D5).
const string MarcajApiLdi = "E2E-API-LDI";

void CurataApiLdi(IObjectSpace os) {
    var idsDoc = os.GetObjectsQuery<ListaDiferenteInventar>()
        .Where(d => d.Primitor.Cod.StartsWith(MarcajApiLdi))
        .Select(d => d.ID).ToList();
    idsDoc.AddRange(os.GetObjectsQuery<DocumentDetaliu>()
        .Where(d => d.Lot.Produs.Cod.StartsWith(MarcajApiLdi))
        .Select(d => d.DocumentId).ToList());
    idsDoc.AddRange(os.GetObjectsQuery<ListaDiferenteInventarDetaliu>()
        .Where(d => d.Produs.Cod.StartsWith(MarcajApiLdi))
        .Select(d => d.DocumentId).ToList());
    idsDoc = idsDoc.Distinct().ToList();

    var idsLot = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiLdi))
        .Select(l => l.ID).ToList();
    os.Delete(os.GetObjectsQuery<RegistruStoc>()
        .Where(r => idsLot.Contains(r.LotId) || (r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value))).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && idsDoc.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsDoc.Contains(d.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<Document>().Where(d => idsDoc.Contains(d.ID)).ToList());
    os.CommitChanges();
    os.Delete(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajApiLdi)).ToList());
    os.Delete(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajApiLdi)).ToList());
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiLdi)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiLdi)).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiLdi(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");

    // Comisia de inventariere e CALITATE transversală (28d), nu clasă — și, mai
    // ales, NU e `Gestiune`: exact motivul pentru care hook-ul de gestiune al
    // loturilor culese trebuie să arate spre PREDATOR.
    var comisie = os.CreateObject<UnitateInterna>();
    comisie.Cod = MarcajApiLdi + "-COM";
    comisie.Denumire = "Comisie de inventariere probă felia Api LDI";
    comisie.Calitati = CalitateRepartitor.Comisie;
    var codEc = os.CreateObject<CodEconomic>();
    codEc.Cod = MarcajApiLdi + "-CE";
    codEc.Denumire = "Cod economic probă felia Api LDI";
    var produs = os.CreateObject<Produs>();
    produs.Cod = MarcajApiLdi + "-A";
    produs.Denumire = "Produs A probă felia Api LDI";
    produs.UM = "BUC";
    produs.TipMaterial = tipMateriale;
    os.CommitChanges();

    var dataLdi = new DateOnly(2026, 3, 20);
    var lotVechi = os.CreateObject<Lot>();
    lotVechi.Produs = produs;
    lotVechi.PretUnitar = 10m;
    lotVechi.Gestiune = mag1;
    lotVechi.Data = new DateOnly(2026, 1, 10);
    var deschidereLdi = os.CreateObject<RegistruStoc>();
    deschidereLdi.Data = lotVechi.Data;
    deschidereLdi.TipStoc = TipStoc.Magazie;
    deschidereLdi.Lot = lotVechi;
    deschidereLdi.Repartitor = mag1;
    deschidereLdi.Cantitate = 10m;
    deschidereLdi.Valoare = 100m;
    os.CommitChanges();

    decimal SoldLdi(Lot l) => StocService.Sold(os, new CheieStoc(l.ID, mag1.ID, TipStoc.Magazie));
    IReadOnlyList<string> DryRunLdi(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }
    int SerieLdi() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "LDI").UrmatorulNumar;

    // --- Apply: lista bidirecțională, culeasă cu cantități POZITIVE ---
    var writeLdi = new LdiWriteDto {
        Data = dataLdi,
        PredatorId = mag1.ID,
        PrimitorId = comisie.ID,
        Linii = {
            new LdiLinieWriteDto {
                Directie = "Minus", TipMaterialId = tipMateriale.ID,
                LotId = lotVechi.ID, Cantitate = 2m
            },
            new LdiLinieWriteDto {
                Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                Cantitate = 3m, PretEvaluare = 7m,
                LotFabricatie = "LOT-PLUS", DataExpirare = new DateOnly(2027, 9, 30),
                CodEconomicId = codEc.ID
            }
        }
    };
    var idLdi = ListaDiferenteInventarApply.Aplica(os, null, writeLdi);
    var citit = ListaDiferenteInventarApply.Citeste(os, idLdi);
    Check("Apply LDI → header plat, FĂRĂ număr (seria „LDI-” e server-owned, se consumă la operare)",
        citit != null && citit.Id == idLdi && citit.Stare == "Draft" && citit.Numar == null
        && citit.Data == dataLdi
        && citit.PredatorId == mag1.ID && citit.PredatorDenumire == mag1.Denumire
        && citit.PrimitorId == comisie.ID && citit.PrimitorDenumire == comisie.Denumire
        && citit.Linii.Count == 2
        && citit.PoateEdita && citit.PoateOpera && !citit.PoateAnula && !citit.PoateStorna);

    var linieMinus = citit.Linii.Single(l => l.Directie == "Minus");
    var liniePlus = citit.Linii.Single(l => l.Directie == "Plus");
    var lotPlus = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == liniePlus.Id);
    Check("TESTUL-ANCORĂ (F6-D2): lotul PLUSULUI se naște pe linia proprie, din ProdusId, în gestiunea INVENTARIATĂ (PREDATORUL — hook-ul GestiuneLoturiCulese), nefinalizat",
        lotPlus != null && lotPlus.ProdusId == produs.ID && lotPlus.GestiuneId == mag1.ID
        && lotPlus.GestiuneId != comisie.ID
        && lotPlus.Data == default && lotPlus.PretUnitar == 0m
        && liniePlus.LotId == lotPlus.ID
        && liniePlus.LotEticheta == lotPlus.Eticheta
        && liniePlus.LotEticheta.Contains("(în culegere)"));
    Check("Minusul PINUIEȘTE un lot existent și rămâne NEATINS de serviciu (gardul de lot străin): niciun lot propriu pe linia de minus",
        linieMinus.LotId == lotVechi.ID
        && !os.GetObjectsQuery<Lot>().Any(l => l.LinieIntrareId == linieMinus.Id)
        && linieMinus.ProdusId == null && linieMinus.PretEvaluare == null);
    Check("Valoarea SEMNATĂ la culegere (F6-D6): minus −2 × 10 = −20, plus +3 × 7 = +21, Total = efectul NET (+1)",
        linieMinus.Valoare == -20m && liniePlus.Valoare == 21m && citit.Total == 1m);
    Check("Cantitatea rămâne POZITIVĂ până la operare (semnarea ei e a operării — 28a)",
        linieMinus.Cantitate == 2m && liniePlus.Cantitate == 3m);
    Check("Linia de plus poartă produsul, prețul de evaluare, atributele de lot și dimensiunea frunzei, proiectate plat",
        liniePlus.ProdusId == produs.ID && liniePlus.ProdusCod == produs.Cod
        && liniePlus.ProdusDenumire == produs.Denumire && liniePlus.PretEvaluare == 7m
        && liniePlus.LotFabricatie == "LOT-PLUS" && liniePlus.DataExpirare == new DateOnly(2027, 9, 30)
        && liniePlus.CodEconomicId == codEc.ID && liniePlus.CodEconomicCod == codEc.Cod);
    var randLdi = ListaDiferenteInventarApply.Lista(os).Single(d => d.Id == idLdi);
    Check("Lista LDI: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
        randLdi.Stare == "Draft" && randLdi.Total == 1m && randLdi.Numar == null
        && randLdi.PredatorDenumire == mag1.Denumire && randLdi.PrimitorDenumire == comisie.Denumire);

    // --- PUT repetat: lotul plusului NU se dublează (F6-D5: LotId din payload
    //     e ecoul ReadDto-ului, nu o intenție — pe plus se ignoră) ---
    writeLdi.Linii[0].Id = linieMinus.Id;
    writeLdi.Linii[1].Id = liniePlus.Id;
    writeLdi.Linii[1].LotId = liniePlus.LotId;   // exact ce ar retrimite clientul
    ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
    ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
    var dupaPut = ListaDiferenteInventarApply.Citeste(os, idLdi);
    Check("PUT repetat identic pe Plus → ACELAȘI lot, unul singur (round-trip-ul LotId nu re-leagă și nu dublează)",
        os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == liniePlus.Id) == 1
        && dupaPut.Linii.Single(l => l.Directie == "Plus").LotId == lotPlus.ID
        && dupaPut.Linii.Single(l => l.Directie == "Minus").LotId == lotVechi.ID);

    // --- Refuzurile de payload ---
    CheckRefuza("Apply LDI cu direcție necunoscută → refuz care ENUMERĂ valorile valide (parse pe NUME, la graniță)", () =>
        ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
            Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
            Linii = { new LdiLinieWriteDto { Directie = "Ambele", TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
        }));
    CheckRefuza("Apply LDI FĂRĂ direcție → același refuz (enum-ul n-are default valid — 28e; linia ar fi oricum ne-operabilă)", () =>
        ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
            Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
            Linii = { new LdiLinieWriteDto { TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
        }));
    CheckRefuza("Apply LDI cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        ListaDiferenteInventarApply.Aplica(os, idLdi, new LdiWriteDto {
            Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
            Linii = { new LdiLinieWriteDto {
                Id = Guid.NewGuid(), Directie = "Minus", TipMaterialId = tipMateriale.ID, Cantitate = 1m } }
        }));
    CheckRefuza("Apply LDI cu același Id de linie de două ori → refuz", () =>
        ListaDiferenteInventarApply.Aplica(os, idLdi, new LdiWriteDto {
            Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
            Linii = { writeLdi.Linii[0], writeLdi.Linii[0] }
        }));
    CheckRefuza("Apply LDI cu preț de evaluare în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
        ListaDiferenteInventarApply.Aplica(os, idLdi, new LdiWriteDto {
            Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
            Linii = { new LdiLinieWriteDto {
                Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
                Cantitate = 1m, PretEvaluare = 0.0000001m } }
        }));
    ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi);
    Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact două linii",
        ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Count == 2);

    // --- Comutarea de direcție Plus→Minus, pe un document propriu ---
    var idComut = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 5m, PretEvaluare = 4m, LotFabricatie = "LOT-COMUT",
            DataExpirare = new DateOnly(2028, 1, 31), CodEconomicId = codEc.ID } }
    });
    var linieComut = ListaDiferenteInventarApply.Citeste(os, idComut).Linii.Single();
    var lotComut = linieComut.LotId.Value;
    ListaDiferenteInventarApply.Aplica(os, idComut, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Id = linieComut.Id, Directie = "Minus", TipMaterialId = tipMateriale.ID,
            // Ce ar retrimite clientul după comutare: pinul nou + reziduul
            // câmpurilor de plus, pe care Apply are obligația să le GOLEASCĂ.
            LotId = lotVechi.ID, Cantitate = 5m, ProdusId = produs.ID, PretEvaluare = 4m,
            LotFabricatie = "LOT-COMUT", DataExpirare = new DateOnly(2028, 1, 31) } }
    });
    var dupaComut = ListaDiferenteInventarApply.Citeste(os, idComut).Linii.Single();
    Check("Comutare Plus→Minus prin PUT: lotul propriu NEFINALIZAT e ȘTERS (gardul NasteLot, F6-D3), pinul nou se aplică",
        !os.GetObjectsQuery<Lot>().Any(l => l.ID == lotComut)
        && dupaComut.Directie == "Minus" && dupaComut.LotId == lotVechi.ID);
    Check("Comutare Plus→Minus: câmpurile plusului sunt GOLITE, nu doar ignorate (F6-D3 — „inert devine adevărat”)",
        dupaComut.ProdusId == null && dupaComut.PretEvaluare == null
        && dupaComut.DataExpirare == null && dupaComut.LotFabricatie == null);
    Check("Comutare Plus→Minus: valoarea se re-materializează semnat, din prețul lotului PINUIT (−5 × 10 = −50)",
        dupaComut.Valoare == -50m);
    ListaDiferenteInventarApply.Sterge(os, idComut);
    Check("Sterge după comutare: documentul dispare, iar lotul PINUIT (al altcuiva) rămâne intact",
        ListaDiferenteInventarApply.Citeste(os, idComut) == null
        && os.GetObjectByKey<Lot>(lotVechi.ID) != null);

    // --- PUT care SCHIMBĂ PREDATORUL după nașterea lotului (review, gaura 1:
    //     jumătatea neexersată a testului-ancoră — gestiunea lotului îl urmează,
    //     ramura de sincronizare din LoturiCulegereService) ---
    var mag2Ldi = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG2");
    var idMutat = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 2m, PretEvaluare = 4m, CodEconomicId = codEc.ID } }
    });
    var linieMutata = ListaDiferenteInventarApply.Citeste(os, idMutat).Linii.Single();
    ListaDiferenteInventarApply.Aplica(os, idMutat, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag2Ldi.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Id = linieMutata.Id, Directie = "Plus", TipMaterialId = tipMateriale.ID,
            ProdusId = produs.ID, Cantitate = 2m, PretEvaluare = 4m, CodEconomicId = codEc.ID } }
    });
    Check("PUT care schimbă PREDATORUL după nașterea lotului → gestiunea lotului îl urmează (gaura 1 din review)",
        mag2Ldi != null
        && os.GetObjectByKey<Lot>(linieMutata.LotId.Value).GestiuneId == mag2Ldi.ID);
    ListaDiferenteInventarApply.Sterge(os, idMutat);

    // --- Self-healing pe linia „istorică": lot FINALIZAT + ProdusId null ---
    // `ProdusId` e coloană NOUĂ pe frunza LDI (migrația F6): pe liniile scrise
    // înainte de felie e null deși lotul e finalizat. Fără distincția din serviciu
    // (review GATE D1, replicat de F5), orice PUT le-ar ȘTERGE lotul — ID, dată
    // reală, preț, poziție FIFO — fără nicio eroare.
    var docIstoric = os.CreateObject<ListaDiferenteInventar>();
    docIstoric.Data = dataLdi;
    docIstoric.Predator = mag1;
    docIstoric.Primitor = comisie;
    var linieIstorica = os.CreateObject<ListaDiferenteInventarDetaliu>();
    linieIstorica.Document = docIstoric;
    linieIstorica.TipMaterial = tipMateriale;
    linieIstorica.Directie = DirectieDiferenta.Plus;
    linieIstorica.Cantitate = 2m;
    linieIstorica.PretEvaluare = 6m;
    linieIstorica.CodEconomicId = codEc.ID;
    os.CommitChanges();
    var lotIstoric = linieIstorica.CreeazaLot(os, produs, mag1);
    lotIstoric.PretUnitar = 6m;                 // FINALIZAT: a trecut prin motor
    lotIstoric.Data = new DateOnly(2026, 2, 1);
    linieIstorica.ProdusId = null;              // …dar coloana nouă e goală
    os.CommitChanges();
    var idIstoric = docIstoric.ID;
    var idLotIstoric = lotIstoric.ID;
    ListaDiferenteInventarApply.Aplica(os, idIstoric, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Id = linieIstorica.ID, Directie = "Plus", TipMaterialId = tipMateriale.ID,
            ProdusId = null, LotId = idLotIstoric, Cantitate = 2m, PretEvaluare = 6m,
            CodEconomicId = codEc.ID } }
    });
    var dupaIstoric = ListaDiferenteInventarApply.Citeste(os, idIstoric).Linii.Single();
    Check("Linia „istorică” (lot FINALIZAT, ProdusId null) → SELF-HEALING, nu ștergere: lotul supraviețuiește cu prețul și data lui, produsul se backfill-ează de pe lot",
        os.GetObjectByKey<Lot>(idLotIstoric) is { PretUnitar: 6m } lotViu
        && lotViu.Data == new DateOnly(2026, 2, 1)
        && dupaIstoric.ProdusId == produs.ID && dupaIstoric.LotId == idLotIstoric);
    ListaDiferenteInventarApply.Sterge(os, idIstoric);

    // --- Refuzurile de OPERARE, fără rânduri-fantomă și fără serie consumată ---
    var serieInainteLdi = SerieLdi();
    void RefuzLdi(string nume, Guid predatorId, Guid primitorId, LdiLinieWriteDto linieProba) {
        var id = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
            Data = dataLdi, PredatorId = predatorId, PrimitorId = primitorId, Linii = { linieProba }
        });
        CheckRefuza(nume, () => OperareApi.Opereaza(os, id));
        Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
            !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == id)
            && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == id)
            && os.GetObjectByKey<ListaDiferenteInventar>(id).Stare == StareDocument.Draft
            && os.GetObjectByKey<ListaDiferenteInventar>(id).Numar == null);
        ListaDiferenteInventarApply.Sterge(os, id);
    }

    RefuzLdi("Primitor fără calitatea Comisie → refuz („primitorul trebuie să fie comisia de inventariere”)",
        mag1.ID, mag1.ID,
        new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
            LotId = lotVechi.ID, Cantitate = 1m });
    RefuzLdi("Predator care nu e gestiune → refuz (predatorul e gestiunea inventariată)",
        comisie.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
            LotId = lotVechi.ID, Cantitate = 1m });
    RefuzLdi("Plus FĂRĂ produs → refuz cu mesajul care spune CE SĂ FACĂ („alegeți produsul”) — lotul nu s-a putut naște",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID,
            Cantitate = 1m, PretEvaluare = 5m, CodEconomicId = codEc.ID });
    RefuzLdi("Plus cu preț de evaluare 0 → refuz (28e: altfel lotul intră în stoc cu valoare zero și FIFO o propagă în toate ieșirile)",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 1m, CodEconomicId = codEc.ID });
    RefuzLdi("Plus fără cod economic (venitul 791 cere defalcarea E) → refuz al gardianului de dimensiuni obligatorii",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 1m, PretEvaluare = 5m });
    RefuzLdi("Minus FĂRĂ lot → refuz („linia de minus descarcă un lot existent”)",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID, Cantitate = 1m });
    RefuzLdi("Cantitate 0 pe linia de diferență → refuz",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
            LotId = lotVechi.ID, Cantitate = 0m });
    RefuzLdi("Minus peste disponibil → refuz al gardianului de sold",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Minus", TipMaterialId = tipMateriale.ID,
            LotId = lotVechi.ID, Cantitate = 999m });

    // Review advers F6-F2: coerența Tip↔Produs pe plusul care naște lot —
    // fără ea, lotul se năștea cu produs de marfă pe cont de materiale, iar
    // ieșirile legitime (care VALIDEAZĂ coerența) găseau stocul pe cheia greșită.
    var tipAltLdi = os.GetObjectsQuery<TipMaterial>().First(t => t.ID != tipMateriale.ID);
    var produsAltTip = os.CreateObject<Produs>();
    produsAltTip.Cod = MarcajApiLdi + "-B";
    produsAltTip.Denumire = "Produs B (alt Tip) probă felia Api LDI";
    produsAltTip.UM = "BUC";
    produsAltTip.TipMaterial = tipAltLdi;
    os.CommitChanges();
    RefuzLdi("Plus cu produs din ALT Tip decât Tipul liniei → refuz de coerență (review F6-F2: invariantul 50a se păzește la naștere, ca pe FCT/NIR/ASM)",
        mag1.ID, comisie.ID,
        new LdiLinieWriteDto { Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produsAltTip.ID,
            Cantitate = 1m, PretEvaluare = 5m, CodEconomicId = codEc.ID });

    // Review advers F6-F1 (oglinda gardului ASM, 46d): minusul care descarcă
    // lotul născut de o linie-FRATE ar intra cu preț nefinalizat (0) — gardianul
    // de sold ar trece (+3−2 ≥ 0 pe aceeași cheie, aceeași zi), iar consumul
    // restului la prețul finalizat ar lăsa valoare orfană pe cantitate 0.
    var idFrate = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 3m, PretEvaluare = 7m, CodEconomicId = codEc.ID } }
    });
    var citFrate = ListaDiferenteInventarApply.Citeste(os, idFrate).Linii.Single();
    ListaDiferenteInventarApply.Aplica(os, idFrate, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = {
            new LdiLinieWriteDto {
                Id = citFrate.Id, Directie = "Plus", TipMaterialId = tipMateriale.ID,
                ProdusId = produs.ID, Cantitate = 3m, PretEvaluare = 7m, CodEconomicId = codEc.ID },
            new LdiLinieWriteDto {
                Directie = "Minus", TipMaterialId = tipMateriale.ID,
                LotId = citFrate.LotId, Cantitate = 2m }
        }
    });
    CheckRefuza("Minus care descarcă lotul născut de linia-FRATE a aceluiași document → refuz (review F6-F1: prețul plusului nu există până la operare)",
        () => OperareApi.Opereaza(os, idFrate));
    Check("Refuzul F6-F1 — fără rânduri-fantomă (33d)",
        !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idFrate)
        && os.GetObjectByKey<ListaDiferenteInventar>(idFrate).Stare == StareDocument.Draft);
    ListaDiferenteInventarApply.Sterge(os, idFrate);

    Check("Seria „LDI-” NU se consumă la refuz (F6-D4 + GATE D6: numărul se asignează abia la materializare)",
        SerieLdi() == serieInainteLdi);

    // --- Dry-run, apoi comanda ---
    Check("Dry-run (Valideaza) pe draftul LDI valid → listă goală", DryRunLdi(idLdi).Count == 0);
    Check("Dry-run-ul NU materializează nimic: Draft, fără număr, fără registre, lotul plusului tot nefinalizat",
        os.GetObjectByKey<ListaDiferenteInventar>(idLdi).Stare == StareDocument.Draft
        && os.GetObjectByKey<ListaDiferenteInventar>(idLdi).Numar == null
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idLdi)
        && os.GetObjectByKey<Lot>(lotPlus.ID).PretUnitar == 0m);

    var rezLdi = OperareApi.Opereaza(os, idLdi);
    citit = ListaDiferenteInventarApply.Citeste(os, idLdi);
    Check("OperareApi.Opereaza pe LDI → Operat, cu număr din politica proprie (seria LDI-), fără conex; affordances inversate",
        rezLdi.StareNoua == StareDocument.Operat && rezLdi.ConexId == null
        && citit.Numar?.StartsWith("LDI-") == true && citit.DataOperare != null
        && !citit.PoateEdita && !citit.PoateOpera && citit.PoateAnula && citit.PoateStorna
        && SerieLdi() == serieInainteLdi + 1);
    Check("Operarea SEMNEAZĂ cantitatea (28a) — ReadDto o arată ca atare pe documentul (oricum) read-only: minus −2, plus +3",
        citit.Linii.Single(l => l.Directie == "Minus").Cantitate == -2m
        && citit.Linii.Single(l => l.Directie == "Plus").Cantitate == 3m
        && citit.Linii.Single(l => l.Directie == "Minus").Valoare == -20m
        && citit.Linii.Single(l => l.Directie == "Plus").Valoare == 21m);
    var lotPlusFinal = os.GetObjectByKey<Lot>(lotPlus.ID);
    Check("Motorul FINALIZEAZĂ lotul plusului: PretUnitar = PretEvaluare (7), data documentului, atributele culese pe linie",
        lotPlusFinal.PretUnitar == 7m && lotPlusFinal.Data == dataLdi
        && lotPlusFinal.LotFabricatie == "LOT-PLUS"
        && lotPlusFinal.DataExpirare == new DateOnly(2027, 9, 30));
    var stocLdi = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == idLdi).ToList();
    Check("LDI operat → 2 rânduri, ambele Magazie pe gestiunea INVENTARIATĂ: −2/−20 pe lotul vechi, +3/+21 pe lotul nou",
        stocLdi.Count == 2 && stocLdi.All(r => r.TipStoc == TipStoc.Magazie && r.RepartitorId == mag1.ID)
        && stocLdi.Any(r => r.LotId == lotVechi.ID && r.Cantitate == -2m && r.Valoare == -20m)
        && stocLdi.Any(r => r.LotId == lotPlus.ID && r.Cantitate == 3m && r.Valoare == 21m));
    Check("Solduri după operare: lot vechi 8, lot nou 3",
        SoldLdi(lotVechi) == 8m && SoldLdi(lotPlusFinal) == 3m);
    Check("Contare pe direcție (SemnFiltru): două note — minusul POZITIV (normalizat), plusul pe venitul de inventar",
        os.GetObjectsQuery<RegistruContabil>().Count(r => r.DocumentId == idLdi) == 2
        && os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idLdi && r.Valoare == 20m)
        && os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idLdi && r.Valoare == 21m));
    CheckRefuza("Apply peste LDI Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => ListaDiferenteInventarApply.Aplica(os, idLdi, writeLdi));
    CheckRefuza("Sterge peste LDI Operat → același refuz de domeniu",
        () => ListaDiferenteInventarApply.Sterge(os, idLdi));

    // --- Anulare directă (lotul plusului neatins de alții) și storno ---
    Check("Anulare prin API → Draft + solduri revenite (vechi 10, nou 0)",
        OperareApi.AnuleazaOperarea(os, idLdi).StareNoua == StareDocument.Draft
        && SoldLdi(lotVechi) == 10m && SoldLdi(lotPlusFinal) == 0m
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idLdi));
    OperareApi.Opereaza(os, idLdi);
    Check("Re-operare după anulare: semnul rămâne IDEMPOTENT (Math.Abs înainte de semnare, pe ambele căi)",
        ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Single(l => l.Directie == "Minus").Cantitate == -2m
        && ListaDiferenteInventarApply.Citeste(os, idLdi).Linii.Single(l => l.Directie == "Plus").Valoare == 21m);
    Check("Storno prin API → Stornat, 4 rânduri de stoc (2 + 2 inverse), solduri nete revenite",
        OperareApi.Storneaza(os, idLdi, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
        && os.GetObjectsQuery<RegistruStoc>().Count(r => r.DocumentId == idLdi) == 4
        && os.GetObjectsQuery<RegistruStoc>().Count(r => r.DocumentId == idLdi && r.Storno) == 2
        && SoldLdi(lotVechi) == 10m && SoldLdi(lotPlusFinal) == 0m);

    // --- Riscul 1 din contract: comutarea cu lotul propriu FINALIZAT ---
    // (operare → anulare → comutare pe Minus prin PUT): pinul ține, produsul se
    // golește pe toate căile (F6-M1), iar lotul finalizat SUPRAVIEȚUIEȘTE —
    // reziduu istoric asumat (F6-M3, documentat în contract §Închidere); la
    // ștergerea documentului, curățenia „fără urme” îl culege totuși.
    var idFinalizat = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 2m, PretEvaluare = 9m, CodEconomicId = codEc.ID } }
    });
    var linieFinalizata = ListaDiferenteInventarApply.Citeste(os, idFinalizat).Linii.Single();
    var idLotFinalizat = linieFinalizata.LotId.Value;
    OperareApi.Opereaza(os, idFinalizat);
    OperareApi.AnuleazaOperarea(os, idFinalizat);
    ListaDiferenteInventarApply.Aplica(os, idFinalizat, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Id = linieFinalizata.Id, Directie = "Minus", TipMaterialId = tipMateriale.ID,
            LotId = lotVechi.ID, Cantitate = 1m } }
    });
    var dupaFinalizat = ListaDiferenteInventarApply.Citeste(os, idFinalizat).Linii.Single();
    Check("Comutare cu lotul propriu FINALIZAT (operare→anulare→comutare): pinul ține, produsul golit, lotul finalizat SUPRAVIEȚUIEȘTE cu prețul lui (riscul 1 din contract)",
        dupaFinalizat.Directie == "Minus" && dupaFinalizat.LotId == lotVechi.ID
        && dupaFinalizat.ProdusId == null
        && os.GetObjectByKey<Lot>(idLotFinalizat) is { PretUnitar: 9m });
    ListaDiferenteInventarApply.Sterge(os, idFinalizat);
    Check("Sterge după comutarea cu lot finalizat: curățenia „fără urme” culege și reziduul (anularea i-a șters registrele, nicio linie vie nu-l referă)",
        os.GetObjectByKey<Lot>(idLotFinalizat) == null);

    // --- Sterge: draftul de plus și lotul lui în culegere mor împreună ---
    var idLdi2 = ListaDiferenteInventarApply.Aplica(os, null, new LdiWriteDto {
        Data = dataLdi, PredatorId = mag1.ID, PrimitorId = comisie.ID,
        Linii = { new LdiLinieWriteDto {
            Directie = "Plus", TipMaterialId = tipMateriale.ID, ProdusId = produs.ID,
            Cantitate = 1m, PretEvaluare = 3m, CodEconomicId = codEc.ID } }
    });
    var idLotDraftLdi = ListaDiferenteInventarApply.Citeste(os, idLdi2).Linii.Single().LotId.Value;
    ListaDiferenteInventarApply.Sterge(os, idLdi2);
    Check("Sterge pe draftul LDI: documentul, linia și LOTUL în culegere dispar împreună",
        ListaDiferenteInventarApply.Citeste(os, idLdi2) == null
        && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idLdi2)
        && !os.GetObjectsQuery<Lot>().Any(l => l.ID == idLotDraftLdi));

    CurataApiLdi(os);
    Check("Curățenie finală felia Api LDI (fără reziduuri e2e)",
        !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajApiLdi))
        && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiLdi))
        && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajApiLdi))
        && os.GetObjectByKey<ListaDiferenteInventar>(idLdi) == null);
}

// ===================== Felia Api DEC (F8-D13, blocul E2E-ADEC) =====================
// Fluxul-ancoră al decontului parcurs prin CONTRACTUL feliei: `DecontWriteDto` →
// `DecontApply.Aplica` → `Citeste`/`Lista` → dry-run → `OperareApi.Opereaza` →
// registre → imperecherea lanțului avans↔decont. Endpoint-urile din host sunt
// transport peste EXACT acest cod (blocul e2e 3c de mai sus probează MOTORUL pe
// obiecte construite direct — aici se probează CULEGEREA).
//
// Ce e PROPRIU tipului, față de toate feliile de până acum:
//   * POSTAREA EXPLICITĂ PE LINIE (32a): contul cules BATE `SursaCont`, iar
//     repartitorul cules e nivelul MAXIM al coalesce-ului de dimensiuni;
//   * CANTITATEA PRO-FORMA 0 → 1, de acum VIZIBILĂ la culegere (F8-D2), nu doar
//     în `PregatesteOperare`;
//   * `ILinieCuPretUnitar` (F8-D2) ⇒ `Valoare`/`ValoareTva` se materializează la
//     culegere prin ACELAȘI helper ca FCT/FCL.
// Rulează pe profilul BUGETAR (baza aplicației). NOTĂ de profil: acolo toate
// regimurile de TVA sunt Capitalizat, deci calea de override e doar REFUZ —
// semantica POZITIVĂ a override-ului (păstrare fără declanșator, cedare la
// schimbarea bazei) se probează în blocul privat, pe N21, exact ca la FCT
// (precedentul 56f — nu se inventează tipuri de TVA în seed pentru probe).
const string MarcajApiDec = "E2E-API-DEC";

void CurataApiDec(IObjectSpace os) {
    var repIds = os.GetObjectsQuery<Repartitor>()
        .Where(r => r.Cod.StartsWith(MarcajApiDec)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>()
        .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    os.Delete(docs);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiDec)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajApiDec)).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataApiDec(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
    var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
    var tipDeplasari = os.FirstOrDefault<TipMaterial>(t => t.Cod == "614.00.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
    var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");
    // Profilul bugetar: DOAR regimuri Capitalizat. CAP0 (cota 0) e singurul pe
    // care „Valoare = PretUnitar × Cantitate" se citește curat; CAP21 e
    // default-ul de tip (`TipTvaImplicit`) și capitalizează 21% în Valoare.
    var cap0 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP0");
    var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");

    var titular = os.CreateObject<Angajat>();
    titular.Cod = MarcajApiDec + "-ANG";
    titular.Denumire = "Titular probă felia Api DEC";
    titular.ContImplicit = cont542;
    var codEcDec = os.CreateObject<CodEconomic>();
    codEcDec.Cod = MarcajApiDec + "-CE";
    codEcDec.Denumire = "Cod economic probă felia Api DEC";
    os.CommitChanges();

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
    // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
    IReadOnlyList<string> DryRunDec(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }
    int SerieDec() => os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "DEC").UrmatorulNumar;
    List<RegistruContabil> NoteDec(Guid docId) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == docId && !r.Storno).ToList();

    var dataDec = new DateOnly(2026, 3, 12);

    // --- Apply: culegerea, cu cele două feluri de linie ale tipului ---
    var writeDec = new DecontWriteDto {
        Data = dataDec,
        PredatorId = titular.ID,
        PrimitorId = sediu.ID,
        NumarPV = "PV-DEC-1",
        DataPV = new DateOnly(2026, 3, 11),
        Linii = {
            // Linia „normală": contarea cade integral pe regulă (debit din contul
            // Tipului 614, credit pe titular). Cantitatea 0 = pro-forma.
            new DecontLinieWriteDto {
                TipMaterialId = tipDeplasari.ID, Descriere = "Transport delegație",
                Cantitate = 0m, PretUnitar = 30m, TipTvaId = cap0.ID,
                CodEconomicId = codEcDec.ID
            },
            // Linia cu POSTARE EXPLICITĂ: contul 623 bate Tipul 628, iar
            // repartitorul debitor cules (MAG1) bate default-ul polimorf
            // (debit←Predator = titularul). Fără TipTva în payload ⇒ default-ul
            // tipului de document (CAP21, seed bugetar).
            new DecontLinieWriteDto {
                TipMaterialId = tipServicii.ID, Descriere = "Protocol contractare",
                Cantitate = 2m, PretUnitar = 10m,
                ContDebitId = cont623.ID, RepartitorDebitId = mag1.ID,
                CodEconomicId = codEcDec.ID
            }
        }
    };
    var idDec = DecontApply.Aplica(os, null, writeDec);
    var citDec = DecontApply.Citeste(os, idDec);
    Check("Apply DEC → header plat, FĂRĂ număr (seria „DEC-” e server-owned, se consumă la operare) + PV",
        citDec != null && citDec.Id == idDec && citDec.Stare == "Draft" && citDec.Numar == null
        && citDec.Data == dataDec && citDec.NumarPV == "PV-DEC-1"
        && citDec.DataPV == new DateOnly(2026, 3, 11)
        && citDec.PredatorId == titular.ID && citDec.PredatorDenumire == titular.Denumire
        && citDec.PrimitorId == sediu.ID && citDec.PrimitorDenumire == sediu.Denumire
        && citDec.Linii.Count == 2
        && citDec.PoateEdita && citDec.PoateOpera && !citDec.PoateAnula && !citDec.PoateStorna);

    var linieDeplasare = citDec.Linii.Single(l => l.TipMaterialId == tipDeplasari.ID);
    var linieProtocol = citDec.Linii.Single(l => l.TipMaterialId == tipServicii.ID);
    Check("F8-D2: cantitatea PRO-FORMA 0 → 1 e VIZIBILĂ imediat după Aplica (nu abia la operare, „în spate”)",
        linieDeplasare.Cantitate == 1m);
    Check("F8-D2 (ILinieCuPretUnitar): Valoare = PretUnitar × Cantitate materializat LA CULEGERE — 30 (CAP0) și 24,2 (CAP21 capitalizează 21% în Valoare); Total brut 54,2",
        linieDeplasare.Valoare == 30m && linieDeplasare.ValoareTva == 0m
        && linieProtocol.Valoare == 24.2m && linieProtocol.ValoareTva == 0m
        && citDec.Total == 54.2m);
    Check("TipTvaImplicit s-a aplicat DOAR pe linia nouă fără TipTva în payload (CAP21 pe DEC, seed bugetar); linia cu TipTva cules rămâne CAP0",
        linieProtocol.TipTvaId == cap21.ID && linieProtocol.TipTvaCod == "CAP21"
        && linieProtocol.TipTvaCota == 21m
        && linieDeplasare.TipTvaId == cap0.ID && linieDeplasare.TipTvaCod == "CAP0");
    Check("Linia proiectează plat descrierea, prețul, dimensiunea frunzei ȘI postarea explicită (cont + repartitor, cu etichetele read-only)",
        linieDeplasare.Descriere == "Transport delegație" && linieDeplasare.PretUnitar == 30m
        && linieDeplasare.CodEconomicId == codEcDec.ID && linieDeplasare.CodEconomicCod == codEcDec.Cod
        && linieDeplasare.ContDebitId == null && linieDeplasare.ContDebitSimbol == null
        && linieDeplasare.RepartitorDebitId == null
        && linieProtocol.ContDebitId == cont623.ID && linieProtocol.ContDebitSimbol == "623.00.00"
        && linieProtocol.ContCreditId == null
        && linieProtocol.RepartitorDebitId == mag1.ID
        && linieProtocol.RepartitorDebitDenumire == mag1.Denumire
        && linieProtocol.RepartitorCreditId == null);
    var randDec = DecontApply.Lista(os).Single(d => d.Id == idDec);
    Check("Lista DEC: aceleași cifre ca agregatul (Total prin join pe agregat), stare tradusă în SQL",
        randDec.Stare == "Draft" && randDec.Total == 54.2m && randDec.Numar == null
        && randDec.PredatorDenumire == titular.Denumire && randDec.PrimitorDenumire == sediu.Denumire);

    // --- Override-ul manual de ValoareTva: refuzat pe regimuri fără TVA separat ---
    // (regula F2-D1/D7, o singură sursă — semantica pozitivă e în blocul privat.)
    writeDec.Linii[0].Id = linieDeplasare.Id;
    writeDec.Linii[0].TipTvaId = linieDeplasare.TipTvaId;
    writeDec.Linii[1].Id = linieProtocol.Id;
    writeDec.Linii[1].TipTvaId = linieProtocol.TipTvaId;   // round-trip-ul ReadDto
    writeDec.Linii[1].ValoareTva = 4.2m;
    CheckRefuza("Override ValoareTva pe regim Capitalizat → refuz (regimul nu poartă TVA separat)",
        () => DecontApply.Aplica(os, idDec, writeDec));
    writeDec.Linii[1].ValoareTva = -1m;
    CheckRefuza("Override ValoareTva NEGATIV → refuz",
        () => DecontApply.Aplica(os, idDec, writeDec));
    writeDec.Linii[1].ValoareTva = null;
    DecontApply.Aplica(os, idDec, writeDec);
    Check("Un Apply refuzat nu lasă reziduu: re-aplicarea payload-ului valid readuce agregatul la exact două linii, cu aceleași cifre",
        DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaRefuz && dupaRefuz.Linii.Count == 2);

    // Golirea deliberată a TipTva pe o linie EXISTENTĂ (default-ul NU se re-aplică).
    writeDec.Linii[1].TipTvaId = null;
    DecontApply.Aplica(os, idDec, writeDec);
    Check("Pe linia EXISTENTĂ, TipTva absent din payload = GOLIRE deliberată → valoarea revine la net 20",
        DecontApply.Citeste(os, idDec).Linii.Single(l => l.Id == linieProtocol.Id)
            is { TipTvaId: null, Valoare: 20m, ValoareTva: 0m });
    // Repunerea default-ului (explicit, prin payload) readuce agregatul la 54,2.
    writeDec.Linii[1].TipTvaId = cap21.ID;
    DecontApply.Aplica(os, idDec, writeDec);
    Check("Repunerea explicită a TipTva pe linia existentă redeclanșează calculul (24,2) — Total 54,2",
        DecontApply.Citeste(os, idDec) is { Total: 54.2m });

    // --- Refuzurile de payload (reconcilierea, probele M3/60d) ---
    CheckRefuza("Apply DEC cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)", () =>
        DecontApply.Aplica(os, idDec, new DecontWriteDto {
            Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
            Linii = { new DecontLinieWriteDto {
                Id = Guid.NewGuid(), TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 1m } }
        }));
    CheckRefuza("Apply DEC cu același Id de linie de două ori → refuz", () =>
        DecontApply.Aplica(os, idDec, new DecontWriteDto {
            Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
            Linii = { writeDec.Linii[0], writeDec.Linii[0] }
        }));
    CheckRefuza("Apply DEC cu preț unitar în afara scării numeric(18,6) → refuz de domeniu, nu DbUpdateException", () =>
        DecontApply.Aplica(os, idDec, new DecontWriteDto {
            Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
            Linii = { new DecontLinieWriteDto {
                TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 0.0000001m } }
        }));
    CheckRefuza("Apply DEC cu cont explicit inexistent → refuz de domeniu (rezolvarea pe navigație), nu violare de FK", () =>
        DecontApply.Aplica(os, idDec, new DecontWriteDto {
            Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
            Linii = { new DecontLinieWriteDto {
                TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 1m,
                ContDebitId = Guid.NewGuid() } }
        }));

    // Reconcilierea e cea care curăță reziduul unui Apply REFUZAT: un refuz de
    // DUPĂ `CreateObject` (scara, un FK inexistent) lasă linia în ObjectSpace-ul
    // VIU al apelantului — pe host OS-ul e per-cerere și moare cu ea, dar aici
    // trăiește mai departe, iar un commit ulterior ar persista-o. Următorul
    // payload valid o șterge, fiindcă nu e în el (același contract ca pe LDI).
    DecontApply.Aplica(os, idDec, writeDec);
    Check("Un Apply refuzat nu lasă reziduu în agregat: următorul payload valid readuce documentul la exact două linii (reconcilierea curăță liniile create înaintea refuzului)",
        DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaRefuzuri
        && dupaRefuzuri.Linii.Count == 2);

    // Linia de tip BAZĂ (decont istoric/importat) referită prin Id: citirea o
    // arată cu câmpurile frunzei NULE (as-cast pe TPT), reconcilierea o refuză
    // acționabil, iar absența ei din payload o ȘTERGE (proba M3/60d).
    var docIstoricDec = os.GetObjectByKey<Decont>(idDec);
    var linieBaza = os.CreateObject<DocumentDetaliu>();
    linieBaza.Document = docIstoricDec;
    linieBaza.TipMaterial = tipTrz;
    linieBaza.Cantitate = 1m;
    linieBaza.Valoare = 7m;
    os.CommitChanges();
    var idLinieBaza = linieBaza.ID;
    var citCuBaza = DecontApply.Citeste(os, idDec);
    Check("Citirea merge pe BAZA detaliului (as-cast la frunză): linia de tip BAZĂ APARE, cu câmpurile frunzei NULE",
        citCuBaza.Linii.Count == 3
        && citCuBaza.Linii.Single(l => l.Id == idLinieBaza)
            is { Descriere: null, PretUnitar: null, ContDebitId: null, Valoare: 7m });
    Check("…iar `Total` o numără la fel în agregat și în listă (definiția Document.Total, pe BAZA detaliului): 61,2",
        citCuBaza.Total == 61.2m
        && DecontApply.Lista(os).Single(d => d.Id == idDec).Total == 61.2m);
    CheckRefuza("Apply DEC cu Id-ul unei linii de tip BAZĂ → refuz acționabil („ștergeți-o și culegeți-o din nou”)", () =>
        DecontApply.Aplica(os, idDec, new DecontWriteDto {
            Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
            Linii = { new DecontLinieWriteDto {
                Id = idLinieBaza, TipMaterialId = tipTrz.ID, Cantitate = 1m, PretUnitar = 7m } }
        }));
    DecontApply.Aplica(os, idDec, writeDec);
    Check("Linia absentă din payload se ȘTERGE (reconciliere server-side): linia de bază dispare, agregatul revine la 54,2",
        !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == idLinieBaza)
        && DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaCuratenie
        && dupaCuratenie.Linii.Count == 2);

    // --- Refuzurile de OPERARE, fără rânduri-fantomă și fără serie consumată ---
    var serieInainteDec = SerieDec();
    void RefuzDec(string nume, Guid predatorId, Guid primitorId, DecontLinieWriteDto linieProba) {
        var id = DecontApply.Aplica(os, null, new DecontWriteDto {
            Data = dataDec, PredatorId = predatorId, PrimitorId = primitorId,
            Linii = { linieProba }
        });
        Check(nume + " — dry-run-ul îl vede (fără să atingă nimic)", DryRunDec(id).Count > 0);
        CheckRefuza(nume, () => OperareApi.Opereaza(os, id));
        Check(nume + " — fără rânduri-fantomă și fără număr consumat (33d + GATE D6)",
            !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == id)
            && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == id)
            && os.GetObjectByKey<Decont>(id).Stare == StareDocument.Draft
            && os.GetObjectByKey<Decont>(id).Numar == null);
        DecontApply.Sterge(os, id);
    }

    RefuzDec("Predator care nu e Angajat → refuz („predatorul decontului este titularul — un angajat”)",
        sediu.ID, sediu.ID,
        new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
            PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
    RefuzDec("Primitor care nu e unitate internă / gestiune → refuz",
        titular.ID, titular.ID,
        new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
            PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
    RefuzDec("Linie cu Valoare 0 (preț necules) → refuz („fiecare linie de decont poartă o valoare pozitivă”)",
        titular.ID, sediu.ID,
        new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
            PretUnitar = 0m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });
    RefuzDec("Linie fără clasificație bugetară (nici angajament, nici cod economic) → refuz al PoliticaValidare (33c, profil bugetar)",
        titular.ID, sediu.ID,
        new DecontLinieWriteDto { TipMaterialId = tipDeplasari.ID, Cantitate = 1m,
            PretUnitar = 10m, TipTvaId = cap0.ID });
    RefuzDec("Tip fără cont implicit și linie fără cont explicit → refuz clar (debitul nu se poate rezolva)",
        titular.ID, sediu.ID,
        new DecontLinieWriteDto { TipMaterialId = tipTrz.ID, Cantitate = 1m,
            PretUnitar = 10m, TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID });

    Check("Seria „DEC-” NU se consumă la refuz (F8-D3 + GATE D6: numărul se asignează abia la materializare)",
        SerieDec() == serieInainteDec);

    // --- Dry-run, apoi comanda ---
    Check("Dry-run (Valideaza) pe draftul DEC valid → listă goală", DryRunDec(idDec).Count == 0);
    Check("Dry-run-ul NU materializează nimic: Draft, fără număr, fără note",
        os.GetObjectByKey<Decont>(idDec).Stare == StareDocument.Draft
        && os.GetObjectByKey<Decont>(idDec).Numar == null
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId == idDec));

    var rezDec = OperareApi.Opereaza(os, idDec);
    citDec = DecontApply.Citeste(os, idDec);
    Check("OperareApi.Opereaza pe DEC → Operat, cu număr din politica proprie (seria DEC-), fără conex; affordances inversate",
        rezDec.StareNoua == StareDocument.Operat && rezDec.ConexId == null
        && citDec.Numar?.StartsWith("DEC-") == true && citDec.DataOperare != null
        && !citDec.PoateEdita && !citDec.PoateOpera && citDec.PoateAnula && citDec.PoateStorna
        && SerieDec() == serieInainteDec + 1);
    Check("Decontul nu mișcă stoc", !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId == idDec));

    var noteDec = NoteDec(idDec);
    var notaDeplasare = noteDec.Single(n => n.DetaliuId == linieDeplasare.Id);
    var notaProtocol = noteDec.Single(n => n.DetaliuId == linieProtocol.Id);
    Check("Contare (2 note, una per linie): debitul din contul Tipului (614) pe linia fără postare explicită, creditul 542 pe ambele",
        noteDec.Count == 2
        && notaDeplasare.ContDebitId == tipDeplasari.ContImplicitId
        && notaDeplasare.ContCreditId == cont542.ID && notaDeplasare.Valoare == 30m
        && notaProtocol.ContCreditId == cont542.ID);
    Check("ANCORA F8-D13.2: contul CULES pe linie (623) BATE rezolvarea declarativă (SursaCont.TipMaterial ar fi dat contul lui 628)",
        notaProtocol.ContDebitId == cont623.ID
        && tipServicii.ContImplicitId != null && notaProtocol.ContDebitId != tipServicii.ContImplicitId
        && notaProtocol.Valoare == 24.2m);
    Check("ANCORA F8-D13.2: repartitorul CULES (MAG1) e nivelul MAXIM al coalesce-ului de dimensiuni; pe linia fără el cade default-ul polimorf (debit←Predator = titularul)",
        notaProtocol.DimensiuniDebit().RepartitorId == mag1.ID
        && notaDeplasare.DimensiuniDebit().RepartitorId == titular.ID
        && notaDeplasare.DimensiuniDebit().CodEconomicId == codEcDec.ID);
    Check("Creditul (542) se dimensionează pe TITULAR pe AMBELE linii (default polimorf 32c, nu primitorul SEDIU)",
        noteDec.All(n => n.DimensiuniCredit().RepartitorId == titular.ID));
    CheckRefuza("Apply peste DEC Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
        () => DecontApply.Aplica(os, idDec, writeDec));
    CheckRefuza("Sterge peste DEC Operat → același refuz de domeniu",
        () => DecontApply.Sterge(os, idDec));

    // --- Lanțul avans → decont: imperecherea și affordance-ele ONESTE (57d) ---
    var avansDec = os.CreateObject<Plata>();
    avansDec.Data = new DateOnly(2026, 3, 2);
    avansDec.Predator = casa;
    avansDec.Primitor = titular;
    avansDec.TipInstrument = TipInstrumentPlata.DispozitieCasa;
    var linieAvansDec = os.CreateObject<DocumentTrezorerieDetaliu>();
    linieAvansDec.Document = avansDec;
    linieAvansDec.TipMaterial = tipTrz;
    linieAvansDec.Valoare = 100m;
    linieAvansDec.CodEconomicId = codEcDec.ID; // 531/542 cer defalcarea E
    os.CommitChanges();
    OperareApi.Opereaza(os, avansDec.ID);
    var impDec = ImperechereService.Imperecheaza(os, avansDec, os.GetObjectByKey<Decont>(idDec), 54.2m);
    citDec = DecontApply.Citeste(os, idDec);
    Check("ANCORA F8-D13.4: avansul (PLT pe titular) stinge decontul pe TOTALUL BRUT, iar affordance-ele devin ONESTE — PoateAnula/PoateStorna FALSE cât există imperecherea (57d)",
        ImperechereService.Ramas(os, idDec) == 0m
        && ImperechereService.Ramas(os, avansDec.ID) == 45.8m
        && citDec.Stare == "Operat" && !citDec.PoateAnula && !citDec.PoateStorna);
    CheckRefuza("…iar gardianul motorului confirmă: anularea decontului imperecheat = refuz",
        () => OperareApi.AnuleazaOperarea(os, idDec));
    os.Delete(impDec);
    os.CommitChanges();
    citDec = DecontApply.Citeste(os, idDec);
    Check("După ștergerea link-ului (31d: se șterge liber), affordance-ele revin",
        citDec.PoateAnula && citDec.PoateStorna);

    // --- Anulare, re-operare idempotentă, storno ---
    Check("Anulare prin API → Draft + notele șterse",
        OperareApi.AnuleazaOperarea(os, idDec).StareNoua == StareDocument.Draft
        && NoteDec(idDec).Count == 0);
    OperareApi.Opereaza(os, idDec);
    Check("Re-operare după anulare: cantitatea pro-forma rămâne 1, valorile rămân, numărul rămâne (idempotență)",
        DecontApply.Citeste(os, idDec) is { Total: 54.2m } dupaReoperare
        && dupaReoperare.Linii.Single(l => l.Id == linieDeplasare.Id).Cantitate == 1m
        && dupaReoperare.Numar?.StartsWith("DEC-") == true);
    Check("Storno prin API → Stornat, note inverse append-only (−30, −24,2) la data stornării",
        OperareApi.Storneaza(os, idDec, new DateOnly(2026, 7, 22)).StareNoua == StareDocument.Stornat
        && os.GetObjectsQuery<RegistruContabil>().Count(r => r.DocumentId == idDec) == 4
        && os.GetObjectsQuery<RegistruContabil>().Count(r => r.DocumentId == idDec && r.Storno
            && r.Data == new DateOnly(2026, 7, 22)
            && (r.Valoare == -30m || r.Valoare == -24.2m)) == 2);

    // --- Sterge pe un draft propriu ---
    var idDecSters = DecontApply.Aplica(os, null, new DecontWriteDto {
        Data = dataDec, PredatorId = titular.ID, PrimitorId = sediu.ID,
        Linii = { new DecontLinieWriteDto {
            TipMaterialId = tipDeplasari.ID, Cantitate = 1m, PretUnitar = 5m,
            TipTvaId = cap0.ID, CodEconomicId = codEcDec.ID } }
    });
    DecontApply.Sterge(os, idDecSters);
    Check("Sterge pe draftul DEC: documentul și liniile lui dispar împreună",
        DecontApply.Citeste(os, idDecSters) == null
        && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idDecSters));

    CurataApiDec(os);
    Check("Curățenie finală felia Api DEC (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiDec))
        && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajApiDec))
        && os.GetObjectByKey<Decont>(idDec) == null);
}

// ============ Felia 8, pasul 3: legătura de pereche prin API (E2E-APER) ======
// F8-D13, partea a doua. Exersează API-ul legăturii (`LaturaPerecheId` cules +
// `Pereche` derivată + endpoint-ul de candidați) PESTE mecanica pasului 1:
// suprimarea generării în AMBELE sensuri (F8-D7), cele șapte refuzuri ale
// validării (F8-D8, inclusiv amendamentul reciprocității), gardianul simetric
// de anulare/storno (F8-D9) și avertismentul CONSULTATIV (F8-D10) — care
// trebuie ȘI să apară când are ce spune, ȘI să tacă altfel: un mesaj care apare
// mereu e zgomot, unul care nu apare niciodată e mort.
const string MarcajAper = "E2E-APER";

void CurataAper(IObjectSpace os) {
    var repIds = os.GetObjectsQuery<Repartitor>()
        .Where(r => r.Cod.StartsWith(MarcajAper)).Select(r => r.ID).ToList();
    var docs = os.GetObjectsQuery<Document>()
        .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
    // Legăturile de pereche se RUP întâi: FK-ul e `Restrict` (F8-D6) și leagă
    // documentele între ele pe o axă pe care ordinea de ștergere n-o declară
    // nimeni (spre deosebire de `DocumentSursa`, unde copiii se cunosc).
    foreach (var t in docs.OfType<DocumentTrezorerie>().Where(t => t.LaturaPerecheId != null)) {
        t.LaturaPereche = null;
        t.LaturaPerecheId = null;
    }
    os.CommitChanges();
    var docIds = docs.Select(d => d.ID).ToList();
    os.Delete(os.GetObjectsQuery<Imperechere>()
        .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>()
        .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
    os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
    // Copiii (perechea autogenerată) înaintea părinților — FK-ul DocumentSursa.
    foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
        os.Delete(doc);
    os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAper)).ToList());
    os.Delete(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajAper)).ToList());
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataAper(os);

    var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
    var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
    var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
    var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");
    var cont581 = os.FirstOrDefault<Cont>(c => c.Simbol == "581");

    ContPropriu ContPropriuAper(string sufix, Cont contImplicit) {
        var cp = os.CreateObject<ContPropriu>();
        cp.Cod = MarcajAper + sufix;
        cp.Denumire = "Cont propriu probă pereche " + sufix;
        cp.ContImplicit = contImplicit;
        return cp;
    }
    var casa = ContPropriuAper("-CASA", cont531);
    var banca = ContPropriuAper("-BANCA", cont770);
    var banca2 = ContPropriuAper("-BANCA2", cont770);
    var partenerAper = os.CreateObject<Partener>();
    partenerAper.Cod = MarcajAper + "-P";
    partenerAper.Denumire = "Partener probă pereche";
    var codEcAper = os.CreateObject<CodEconomic>();
    codEcAper.Cod = MarcajAper + "-CE";
    codEcAper.Denumire = "Cod economic probă pereche";
    os.CommitChanges();

    // Un virament cules: laturi de conturi proprii + linie de natura Virament.
    TrezorerieWriteDto ScrieVir(DateOnly data, Repartitor pred, Repartitor prim,
        decimal valoare, Guid? pereche = null) => new() {
            Data = data, PredatorId = pred.ID, PrimitorId = prim.ID,
            TipInstrument = "DispozitieCasa", LaturaPerecheId = pereche,
            Linii = { new TrezorerieLinieWriteDto {
                TipMaterialId = tipVir.ID, Valoare = valoare, CodEconomicId = codEcAper.ID } }
        };
    // Martorul „nu e virament": contrapartidă obișnuită + Tipul tehnic TRZ.
    TrezorerieWriteDto ScrieNormal(DateOnly data, Repartitor pred, Repartitor prim,
        decimal valoare, Guid? pereche = null) => new() {
            Data = data, PredatorId = pred.ID, PrimitorId = prim.ID,
            LaturaPerecheId = pereche,
            Linii = { new TrezorerieLinieWriteDto {
                TipMaterialId = tipTrz.ID, Valoare = valoare, CodEconomicId = codEcAper.ID } }
        };

    // Dry-run-ul își cere ObjectSpace-ul PROPRIU (`PregatesteOperare` SCRIE).
    IReadOnlyList<string> DryRunAper(Guid docId) {
        using var osDry = provider.CreateObjectSpace();
        return OperareApi.Valideaza(osDry, docId);
    }
    // Refuzurile de gardian se citesc pe MESAJ, nu doar pe „a aruncat": F8-D9
    // cere ca mesajul SPECIFIC (latura pereche) să ajungă înaintea celui de grup
    // conex, iar ordinea aia e o alegere de cod, nu un accident.
    string MesajRefuz(Action actiune) {
        try { actiune(); return null; }
        catch (OperareException e) { return e.Message; }
    }
    decimal Sold581(params Guid[] docIds) {
        var note = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value))
            .Select(r => new { r.ContDebitId, r.ContCreditId, r.Valoare }).ToList();
        return note.Where(r => r.ContDebitId == cont581.ID).Sum(r => r.Valoare)
            - note.Where(r => r.ContCreditId == cont581.ID).Sum(r => r.Valoare);
    }

    // ── (A) Nonregresia F7 + `Pereche` simetrică + avertismentul ABSENT ──────
    var idA1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 4), casa, banca, 100m));
    var rezA1 = OperareApi.Opereaza(os, idA1);
    var idAc = rezA1.ConexId ?? Guid.Empty;
    Check("ANCORA F8-D13.5 (nonregresie F7): perechea AUTOGENERATĂ primește `LaturaPerecheId` = sursa — legătura o scrie motorul pe COPIL, singura parte care e Draft",
        idAc != Guid.Empty && os.GetObjectByKey<Incasare>(idAc).LaturaPerecheId == idA1);
    Check("ANCORA F8-D13.4 (ABSENT): la primul virament de pe aceste laturi nu există picioare operate compatibile ⇒ NICIUN avertisment consultativ (rămâne doar informarea de conex)",
        rezA1.Mesaje.Count == 1 && rezA1.Mesaje[0].Contains("documentul conex")
        && !rezA1.Mesaje.Any(m => m.Contains("picioare operate compatibile")));

    var citA1 = TrezorerieApply.Citeste<Plata>(os, idA1);
    var citAc = TrezorerieApply.Citeste<Incasare>(os, idAc);
    var numarA1 = citA1.Numar;
    Check("ANCORA F8-D13.6: `Pereche` e SIMETRICĂ — copilul o vede prin linkul PROPRIU, sursa DERIVAT (cine mă arată pe mine); tipul e rezolvat polimorf (CodTip), nu presupus din rută",
        citA1.LaturaPerecheId == null
        && citA1.Pereche != null && citA1.Pereche.Id == idAc && citA1.Pereche.Tip == "INC"
        && citA1.Pereche.Stare == "Draft" && citA1.Pereche.Numar == null
        && citAc.LaturaPerecheId == idA1
        && citAc.Pereche != null && citAc.Pereche.Id == idA1 && citAc.Pereche.Tip == "PLT"
        && citAc.Pereche.Stare == "Operat" && citAc.Pereche.Numar == numarA1
        && numarA1?.StartsWith("PLT-") == true);

    var rezAc = OperareApi.Opereaza(os, idAc);
    Check("ANCORA F8-D13.5 (nonregresie F7): latura pereche operată NU generează un al treilea document (gardul de recursie ține), 581 se închide la 0, ZERO imperecheri",
        rezAc.ConexId == null
        && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idAc)
        && Sold581(idA1, idAc) == 0m
        && !os.GetObjectsQuery<Imperechere>().Any(i =>
            i.DocumentStingatorId == idA1 || i.DocumentId == idA1
            || i.DocumentStingatorId == idAc || i.DocumentId == idAc));

    // ── (B) Gardianul F8-D9 pe perechea AUTOGENERATĂ: care mesaj iese ────────
    var mesajTintaAuto = MesajRefuz(() => OperareApi.AnuleazaOperarea(os, idA1));
    Check("ANCORA F8-D13.3: anularea ȚINTEI cu pointer-ul Operat = refuz, cu mesajul SPECIFIC de latură pereche — nu cel de grup conex, deși aici AMBII gardieni s-ar aplica (ordinea e fixată în cod)",
        mesajTintaAuto != null && mesajTintaAuto.Contains("latura pereche")
        && !mesajTintaAuto.Contains("conexe"));
    Check("…iar STORNAREA țintei primește exact același refuz (gardianul e pe ambele căi de corecție)",
        MesajRefuz(() => OperareApi.Storneaza(os, idA1, new DateOnly(2026, 5, 20))) is string m
        && m.Contains("latura pereche"));
    Check("ANCORA F8-D13.3: pointer-ul (piciorul care DECLARĂ legătura) se anulează LIBER — el e frunza, nimeni nu depinde de el",
        OperareApi.AnuleazaOperarea(os, idAc).StareNoua == StareDocument.Draft);
    Check("ANCORA F8-D13.3: după anularea pointer-ului, ținta se anulează; aici pointer-ul e ȘI copil autogenerat, deci dispare cu ea (artefact al operării)",
        OperareApi.AnuleazaOperarea(os, idA1).StareNoua == StareDocument.Draft
        && TrezorerieApply.Citeste<Incasare>(os, idAc) == null
        && TrezorerieApply.Citeste<Plata>(os, idA1) is { Stare: "Draft", Pereche: null });

    // ── (C1) SCENARIUL 64k CANONIC, capăt la capăt ──────────────────────────
    // Fix de criteriu (verificarea main-ului): piciorul de ieșire operat ȘI-A
    // GENERAT un draft care-l arată, iar operatorul îl IGNORĂ (nu-l șterge) și
    // culege manual al doilea picior. Pe criteriul „fără pereche" avertismentul
    // ar fi TĂCUT exact aici — draftul propriu excludea candidatul —, adică
    // jumătatea consultativă a lui F8-D10 era moartă pe scenariul pentru care a
    // fost scrisă. Criteriul corect e „fără pereche OPERATĂ": 581 se închide doar
    // când al doilea picior e operat, iar un draft e o intenție.
    var idAc2 = OperareApi.Opereaza(os, idA1).ConexId ?? Guid.Empty;
    var etichetaDraftBlocant = $"({TrezorerieApply.Citeste<Incasare>(os, idAc2).Data:dd.MM.yyyy})";
    var idC64 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 5), casa, banca, 100m));
    var rezC64 = OperareApi.Opereaza(os, idC64);
    Check("ANCORA 64k (PREZENT, draftul generat ÎNCĂ EXISTĂ): avertismentul APARE și numește ȘI piciorul candidat, ȘI draftul care blochează legarea — pe criteriul vechi („fără pereche”) aici era TĂCERE, exact pe scenariul canonic",
        rezC64.Mesaje.Any(m => m.Contains("picioare operate compatibile")
            && m.Contains(numarA1) && m.Contains("blocat de draftul")
            && m.Contains(etichetaDraftBlocant)));
    Check("ANCORA 64k: avertismentul dă ordinea EXECUTABILĂ (ștergeți draftul, apoi alegeți «latura pereche») — F8-D8 chiar refuză legarea cât timp draftul arată spre țintă, iar un sfat care nu se poate executa e mai rău decât tăcerea",
        rezC64.Mesaje.Any(m => m.Contains("ștergeți draftul generat și alegeți «latura pereche»")));
    Check("ANCORA 64k: candidatul blocat apare ȘI în endpoint, cu `PerecheDraftNumar` completat — clientul nu oferă o opțiune care pică la operare",
        TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null)
            .SingleOrDefault(c => c.Id == idA1) is { Stare: "Operat" } blocatDeDraft
        && blocatDeDraft.PerecheDraftNumar == etichetaDraftBlocant);
    Check("ANCORA F8-D13.4: avertismentul e CONSULTATIV, nu refuz — documentul e Operat, perechea s-a generat oricum (două viramente identice între aceleași conturi sunt legitime — 64k)",
        rezC64.StareNoua == StareDocument.Operat && rezC64.ConexId != null);
    // Refuzul F8-D8 punctul 5 pe un draft AUTOGENERAT: mesajul își spune cazul.
    var idC64Legat = TrezorerieApply.Aplica<Incasare>(os, null,
        ScrieVir(new DateOnly(2026, 5, 5), casa, banca, 100m, pereche: idA1));
    var eroriDraftAuto = DryRunAper(idC64Legat);
    Check("ANCORA F8-D13.2 (punctul 5, pointer AUTOGENERAT): refuzul NU mai spune „e deja perechea altui document” despre un draft pe care tocmai l-a născut sistemul — îl numește și dă remediul",
        eroriDraftAuto.Count == 1
        && eroriDraftAuto[0].Contains("latură pereche GENERATĂ automat")
        && eroriDraftAuto[0].Contains("ștergeți acel draft"));
    TrezorerieApply.Sterge<Incasare>(os, idC64Legat);
    OperareApi.AnuleazaOperarea(os, idC64);
    TrezorerieApply.Sterge<Incasare>(os, idC64);

    // ── (C2) Același flux DUPĂ ștergerea draftului blocant ──────────────────
    TrezorerieApply.Sterge<Incasare>(os, idAc2);
    Check("ANCORA 64k (celălalt capăt): după ștergerea draftului generat, candidatul apare cu `PerecheDraftNumar` NULL — adică „se poate lega direct”",
        TrezorerieApply.Citeste<Plata>(os, idA1) is { Stare: "Operat", Pereche: null }
        && TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null)
            .SingleOrDefault(c => c.Id == idA1) is { PerecheDraftNumar: null });
    var idC = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 6), casa, banca, 100m));
    var rezC = OperareApi.Opereaza(os, idC);
    Check("ANCORA F8-D13.4 (PREZENT, candidat liber): avertismentul îl numește FĂRĂ mențiunea de draft blocant — textul descrie starea reală, nu un șablon fix",
        rezC.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarA1)
            && !m.Contains("blocat de draftul")));
    OperareApi.AnuleazaOperarea(os, idC);
    TrezorerieApply.Sterge<Incasare>(os, idC);

    // ── (D) Legătura CULEASĂ suprimă generarea în AMBELE sensuri ─────────────
    var idD1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m));
    var idD2 = TrezorerieApply.Aplica<Incasare>(os, null,
        ScrieVir(new DateOnly(2026, 5, 9), casa, banca, 200m, pereche: idD1));
    var citD1 = TrezorerieApply.Citeste<Plata>(os, idD1);
    var citD2 = TrezorerieApply.Citeste<Incasare>(os, idD2);
    Check("ANCORA F8-D13.6: `LaturaPerecheId` e câmp CULES — `Aplica` îl scrie, iar `Citeste` întoarce `Pereche` COMPLETĂ pe AMBELE picioare (unul prin link propriu, celălalt derivat), pe două DRAFTURI",
        citD2.LaturaPerecheId == idD1 && citD2.Pereche != null && citD2.Pereche.Id == idD1
        && citD2.Pereche.Tip == "PLT" && citD2.Pereche.Stare == "Draft" && citD2.Pereche.Numar == null
        && citD1.LaturaPerecheId == null && citD1.Pereche != null && citD1.Pereche.Id == idD2
        && citD1.Pereche.Tip == "INC" && citD1.Pereche.Stare == "Draft");

    // Amendamentul F8-D8: reciprocitatea, pe chiar perechea de mai sus.
    TrezorerieApply.Aplica<Plata>(os, idD1,
        ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m, pereche: idD2));
    var eroriReciproc = DryRunAper(idD1);
    Check("ANCORA F8-D13.2 (amendament): legătura RECIPROCĂ A→B peste B→A = refuz — capcană cu ieșire zero (după operare fiecare l-ar bloca pe celălalt la anulare, iar linkul nu se mai poate șterge: nu mai sunt Draft)",
        eroriReciproc.Count == 1 && eroriReciproc[0].Contains("vă declară DEJA ca latură pereche"));
    TrezorerieApply.Aplica<Plata>(os, idD1, ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m));
    Check("…legătura se RETRAGE la fel de simplu cum s-a pus (e câmp cules): dry-run curat după golire",
        TrezorerieApply.Citeste<Plata>(os, idD1).LaturaPerecheId == null && DryRunAper(idD1).Count == 0);

    var rezD1 = OperareApi.Opereaza(os, idD1);
    Check("ANCORA F8-D13.1b: la operarea PRIMULUI picior — cel care NU poartă linkul — generarea se suprimă fiindcă CINEVA ÎL ARATĂ PE EL; fără jumătatea a doua a lui F8-D7 s-ar fi născut un al treilea document, adică gaura 64k mutată cu o zi mai devreme",
        rezD1.ConexId == null
        && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idD1)
        && !rezD1.Mesaje.Any(m => m.Contains("picioare operate compatibile")));
    var rezD2 = OperareApi.Opereaza(os, idD2);
    Check("ANCORA F8-D13.1a: piciorul cules CU link nu generează nimic la rândul lui; perechea declarată manual închide 581 la 0 cu EXACT două rânduri și ZERO imperecheri",
        rezD2.ConexId == null
        && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idD2)
        && os.GetObjectsQuery<RegistruContabil>().Count(r =>
            r.DocumentId == idD1 || r.DocumentId == idD2) == 2
        && Sold581(idD1, idD2) == 0m
        && !os.GetObjectsQuery<Imperechere>().Any(i =>
            i.DocumentStingatorId == idD1 || i.DocumentId == idD1
            || i.DocumentStingatorId == idD2 || i.DocumentId == idD2));

    // ── (E) Criteriul nou nu deschide poarta prea larg + gardianul F8-D9 ────
    // Aici perechea D e OPERATĂ pe ambele picioare: 581 s-a închis, deci niciunul
    // nu mai e candidat — spre deosebire de cel blocat doar de un DRAFT (C1).
    Check("ANCORA (limita criteriului nou): piciorul cu pereche OPERATĂ nu e candidat pe NICIUNA dintre rute — „fără pereche OPERATĂ” lărgește lista exact cu drafturile, nu cu perechile deja produse",
        !TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null).Any(c => c.Id == idD1)
        && !TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca.ID, null).Any(c => c.Id == idD2));
    var idE3 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 11), casa, banca, 50m));
    var rezE3 = OperareApi.Opereaza(os, idE3);
    Check("ANCORA (limita criteriului nou): nici avertismentul nu-l pomenește pe piciorul cu pereche OPERATĂ, deși îl pomenește pe cel liber — proba NU e vacuă (mesajul chiar apare)",
        rezE3.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarA1))
        && !rezE3.Mesaje.Any(m => m.Contains(TrezorerieApply.Citeste<Plata>(os, idD1).Numar)));
    OperareApi.AnuleazaOperarea(os, idE3);
    TrezorerieApply.Sterge<Incasare>(os, idE3);

    // Affordance ONESTĂ pe legătura manuală (afordanța găsită cu criteriul greșit):
    // gardianul F8-D9 refuză anularea țintei, dar `PoateAnula` nu-l oglindea —
    // grupul conex acoperă doar perechea AUTOGENERATĂ (care e și copil).
    Check("ANCORA (affordance onestă, F3-D2): `PoateAnula/PoateStorna` = FALSE pe ținta unei legături MANUALE cu pointer-ul Operat — până acum spuneau „da” despre un document pe care motorul îl refuză (grupul conex nu vede legătura declarată)",
        TrezorerieApply.Citeste<Plata>(os, idD1) is { Stare: "Operat", PoateAnula: false, PoateStorna: false }
        && TrezorerieApply.Citeste<Incasare>(os, idD2) is { PoateAnula: true, PoateStorna: true });

    // ── Gardianul F8-D9 pe legătura MANUALĂ (fără nicio relație de grup) ────
    var mesajTintaManuala = MesajRefuz(() => OperareApi.AnuleazaOperarea(os, idD1));
    Check("ANCORA F8-D13.3: ținta unei legături DECLARATE MANUAL n-are `DocumentSursa`, deci gardianul de grup conex n-are ce apăra — o apără exclusiv cel nou (F8-D9), altfel ținta s-ar re-opera și ar genera o pereche lângă cea deja operată",
        mesajTintaManuala != null && mesajTintaManuala.Contains("latura pereche")
        && !mesajTintaManuala.Contains("conexe"));
    OperareApi.AnuleazaOperarea(os, idD2);
    Check("…pointer-ul se anulează liber, iar DUPĂ el se anulează și ținta; legătura CULEASĂ supraviețuiește anulării (e a operatorului, nu artefact al operării — spre deosebire de draftul autogenerat)",
        OperareApi.AnuleazaOperarea(os, idD1).StareNoua == StareDocument.Draft
        && TrezorerieApply.Citeste<Incasare>(os, idD2) is { Stare: "Draft" } dupaAnulare
        && dupaAnulare.LaturaPerecheId == idD1);

    // ── (F) Cele șapte refuzuri ale validării legăturii (F8-D8), separat ─────
    var seriePltInainte = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar;
    var dataF = new DateOnly(2026, 5, 12);

    // Ținte-fixtură, fiecare pentru exact un refuz.
    var idTintaVir = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca, 10m));
    var idTintaNormala = TrezorerieApply.Aplica<Incasare>(os, null, ScrieNormal(dataF, partenerAper, casa, 10m));
    var idTintaAlteLaturi = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca2, 10m));
    var idTintaAratata = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca, 10m));
    var idPointerulEi = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAratata));

    var idFSelf = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(dataF, casa, banca, 10m));
    Check("ANCORA (fix review): `Aplica` refuză self-linkul PE LOC, cu mesajul MOTORULUI — apply-ul cunoaște `id`, iar acceptarea lui fabrica un draft pe care operatorul nu-l putea nici opera, nici șterge",
        MesajRefuz(() => TrezorerieApply.Aplica<Plata>(os, idFSelf,
            ScrieVir(dataF, casa, banca, 10m, pereche: idFSelf))) is string mSelfApply
        && mSelfApply.Contains("nu poate fi documentul însuși"));
    // Regula rămâne a MOTORULUI, nu a apply-ului: celelalte căi (UI-ul XAF, un
    // import) scriu câmpul direct, deci proba ei se face tot direct — altfel,
    // odată cu refuzul de la graniță, s-ar fi pierdut proba regulii de fond.
    var docSelf = os.GetObjectByKey<Plata>(idFSelf);
    docSelf.LaturaPereche = docSelf;
    os.CommitChanges();
    var eroriSelf = DryRunAper(idFSelf);
    Check("ANCORA F8-D13.2 (1/7 self-link): documentul care se arată PE SINE = refuz — și-ar suprima propria generare, apoi s-ar bloca singur la anulare (gardianul s-ar vedea pe el însuși)",
        eroriSelf.Count == 1 && eroriSelf[0].Contains("nu poate fi documentul însuși"));

    var idFNuEVirament = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieNormal(dataF, casa, partenerAper, 10m, pereche: idTintaVir));
    var eroriNuEVirament = DryRunAper(idFNuEVirament);
    Check("ANCORA F8-D13.2 (2/7 declarantul nu e virament): o plată obișnuită cu link = refuz — n-ar suprima nimic (nu generează oricum), dar ar BLOCA la anulare un document nevinovat prin gardianul F8-D9",
        eroriNuEVirament.Any(e => e.Contains("există doar la viramentul intern")));

    var idFTintaNuEVirament = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idTintaNormala));
    var eroriTintaNuEVirament = DryRunAper(idFTintaNuEVirament);
    Check("ANCORA F8-D13.2 (3/7 ținta nu e virament): predicatul e UNUL singur (`EsteVirament`) și se aplică în ambele capete ale legăturii",
        eroriTintaNuEVirament.Any(e => e.Contains("nu e un virament intern")));

    var idFTipGresit = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idA1));
    var eroriTipGresit = DryRunAper(idFTipGresit);
    Check("ANCORA F8-D13.2 (4/7 tip neopus): două PLĂȚI „pereche” = refuz prin CONTRACT (`TipLaturaPereche`, fără `is` pe tip în bază) — altfel ieșirea s-ar posta de două ori și 581 n-ar mai reveni la zero",
        eroriTipGresit.Count == 1 && eroriTipGresit[0].Contains("tipul opus"));

    var idFAlteLaturi = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAlteLaturi));
    var eroriAlteLaturi = DryRunAper(idFAlteLaturi);
    Check("ANCORA F8-D13.2 (5/7 alte laturi): picioarele stau pe ACELEAȘI conturi (F7-D1 — direcția o poartă tipul); altfel tranzitul 581 ar rămâne deschis pe amândouă",
        eroriAlteLaturi.Count == 1 && eroriAlteLaturi[0].Contains("alte laturi"));

    var idFTintaLegata = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idD2));
    var eroriTintaLegata = DryRunAper(idFTintaLegata);
    Check("ANCORA F8-D13.2 (6/7 ținta are DEJA link propriu spre un al treilea): un virament are exact două picioare",
        eroriTintaLegata.Count == 1 && eroriTintaLegata[0].Contains("deja declarat perechea altui document"));

    var idFTintaAratata = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAratata));
    var eroriTintaAratata = DryRunAper(idFTintaAratata);
    Check("ANCORA F8-D13.2 (7/7 ținta e ARĂTATĂ de altcineva): cealaltă jumătate a simetriei — două picioare de intrare declarate pereche ale aceleiași ieșiri s-ar opera amândouă și ar dubla postarea, tăcut",
        eroriTintaAratata.Count == 1 && eroriTintaAratata[0].Contains("deja arătat ca pereche de alt document"));

    var idsRefuzAper = new List<Guid> {
        idFSelf, idFNuEVirament, idFTintaNuEVirament, idFTipGresit,
        idFAlteLaturi, idFTintaLegata, idFTintaAratata
    };
    Check("ANCORA F8-D13.2: niciunul dintre cele șapte refuzuri n-a lăsat rânduri-fantomă și n-a consumat serie (dry-run = calculează+validează, fără materializare — 33d)",
        idsRefuzAper.All(i => TrezorerieApply.Citeste<Plata>(os, i).Numar == null)
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.DocumentId != null && idsRefuzAper.Contains(r.DocumentId.Value))
        && !os.GetObjectsQuery<RegistruStoc>().Any(r => r.DocumentId != null && idsRefuzAper.Contains(r.DocumentId.Value))
        && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar == seriePltInainte);

    // Ștergerea unui picior ARĂTAT de altul: refuz de DOMENIU, nu FK Restrict brut.
    var mesajStergere = MesajRefuz(() => TrezorerieApply.Sterge<Incasare>(os, idTintaAratata));
    Check("ANCORA F8-D13.6: ștergerea unui picior pe care ALTUL îl declară pereche = refuz de DOMENIU, cu remediul — fără pre-check ar fi ieșit ca violare de constraint (FK Restrict, tradusă generic — 60a): adevărată, dar fără ieșire",
        mesajStergere != null && mesajStergere.Contains("latura pereche")
        && mesajStergere.Contains("ștergeți întâi acea legătură"));

    // Cazul degenerat scos de probă: self-link-ul se arată PE SINE, deci pre-checkul
    // de mai sus îl prinde și pe el — iar FK-ul self-referențial cu `Restrict` e
    // verificat imediat, deci nici baza n-ar lăsa rândul să plece cu tot cu propria
    // referință. Fără mesaj propriu, operatorul rămâne cu un draft ne-operabil (F8-D8)
    // și neștergibil, informat că e „perechea lui însuși".
    var mesajSelf = MesajRefuz(() => TrezorerieApply.Sterge<Plata>(os, idFSelf));
    Check("ANCORA F8-D13.6 (caz degenerat): draftul care se arată PE SINE nu se poate nici opera (F8-D8), nici șterge (pre-check-ul de legătură se vede pe el însuși) — refuzul îi spune EXACT ieșirea: goliți câmpul, apoi ștergeți",
        mesajSelf != null && mesajSelf.Contains("se declară PE SINE")
        && mesajSelf.Contains("goliți întâi"));
    TrezorerieApply.Aplica<Plata>(os, idFSelf, ScrieVir(dataF, casa, banca, 10m));

    foreach (var idRefuz in idsRefuzAper)
        TrezorerieApply.Sterge<Plata>(os, idRefuz);
    TrezorerieApply.Sterge<Plata>(os, idPointerulEi);
    Check("…iar după ștergerea legăturilor care-l arătau, piciorul se șterge normal (refuzul era al legăturii, nu al documentului)",
        MesajRefuz(() => TrezorerieApply.Sterge<Incasare>(os, idTintaAratata)) == null
        && TrezorerieApply.Citeste<Incasare>(os, idTintaAratata) == null);
    TrezorerieApply.Sterge<Incasare>(os, idTintaVir);
    TrezorerieApply.Sterge<Incasare>(os, idTintaNormala);
    TrezorerieApply.Sterge<Incasare>(os, idTintaAlteLaturi);

    // ── (G) Endpoint-ul de candidați ────────────────────────────────────────
    // Starea la acest punct, pe laturile casa→banca: idA1 = PLT Operat FĂRĂ
    // pereche (candidat), idD1 = PLT Draft ARĂTAT de idD2 (nu), idD2 = INC Draft
    // cu link propriu (nu). Se adaugă un INC liber și o plată obișnuită.
    var idGLiber = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 14), casa, banca, 300m));
    var idGNormala = TrezorerieApply.Aplica<Plata>(os, null, ScrieNormal(new DateOnly(2026, 5, 14), casa, partenerAper, 55m));

    var candidatiPtPlata = TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca.ID, null);
    var candidatiPtIncasare = TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null);
    Check("ANCORA F8-D13.6: ruta PLT oferă picioare de tipul OPUS (INC) — inclusiv DRAFT-uri, fiindcă validarea legăturii nu cere stare —, cu `Total` și `Stare` calculate pe SERVER",
        candidatiPtPlata.Count == 1 && candidatiPtPlata[0].Id == idGLiber
        && candidatiPtPlata[0].Stare == "Draft" && candidatiPtPlata[0].Total == 300m
        && candidatiPtPlata[0].Data == new DateOnly(2026, 5, 14));
    Check("ANCORA F8-D13.6: ruta INC oferă piciorul de PLATĂ operat și liber (numărul și totalul lui, `PerecheDraftNumar` null)",
        candidatiPtIncasare.SingleOrDefault(c => c.Id == idA1) is { Stare: "Operat", Total: 100m, PerecheDraftNumar: null } liber
        && liber.Numar == numarA1);
    Check("ANCORA F8-D13.6: linkul PROPRIU rămâne excludere ABSOLUTĂ (idD2 nu se oferă — remediul e al legăturii lui, nu al nostru), dar cel ARĂTAT doar de un DRAFT e DIVULGAT, nu ascuns: apare cu draftul blocant numit, ca operatorul să știe ordinea",
        !candidatiPtPlata.Any(c => c.Id == idD2)
        && candidatiPtIncasare.SingleOrDefault(c => c.Id == idD1) is { Stare: "Draft" } blocat
        && blocat.PerecheDraftNumar == TrezorerieApply.Citeste<Incasare>(os, idD2).Numar);
    Check("ANCORA F8-D13.6: `exclusId` scoate documentul curent din propria listă (formularul întreabă înainte de a fi salvat, deci se poate întreba și despre sine)",
        !TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, idA1).Any(c => c.Id == idA1));
    Check("ANCORA F8-D13.6: laturile sunt filtrul PRIMAR, iar predicatul de virament NU se presupune — pe alte conturi lista e goală, iar o pereche de laturi ne-virament (casa→partener) nu oferă nimic deși EXISTĂ un document pe ea",
        TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca2.ID, null).Count == 0
        && TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, partenerAper.ID, null).Count == 0
        && TrezorerieApply.Citeste<Plata>(os, idGNormala) is { EsteVirament: false });
    var cablareGresita = false;
    try { TrezorerieApply.CandidatiPereche<Plata, Plata>(os, casa.ID, banca.ID, null); }
    catch (InvalidOperationException) { cablareGresita = true; }
    Check("ANCORA F8-D13.6: cusătura „tip opus” e VERIFICATĂ contra contractului domeniului (`TipLaturaPereche`) — o rută cablată greșit pică zgomotos, nu întoarce tăcut candidați de tipul greșit",
        cablareGresita);

    TrezorerieApply.Sterge<Incasare>(os, idGLiber);
    TrezorerieApply.Sterge<Plata>(os, idGNormala);

    // ── (H) Perechea STORNATĂ nu ține (fix review D1) ────────────────────────
    // Un picior stornat are registrele INVERSATE: 581 e redeschis, perechea NU
    // s-a produs. Felia avea DOUĂ criterii pentru aceeași întrebare — gardianul
    // de anulare/storno judeca `== Operat` (corect), iar citirea perechii și
    // filtrele de candidați numărau orice pointer ne-Draft (greșit). Consecința
    // pornea de la storno, unealta NORMALĂ de corecție când perioada e închisă
    // (decizia 14): piciorul rămas descoperit dispărea din listă și din
    // avertisment (gaura 64k, tăcută), iar legarea manuală era refuzată cu un
    // remediu IMPOSIBIL — „ștergeți acea legătură", pe un document stornat, care
    // nu se mai editează și nu se mai șterge.
    //
    // Laturi PROPRII secțiunii: listele de candidați se citesc pe (predator,
    // primitor), deci izolarea e condiția ca probele să fie deterministe.
    var casaH = ContPropriuAper("-CASA-H", cont531);
    var bancaH = ContPropriuAper("-BANCA-H", cont770);
    var casaI = ContPropriuAper("-CASA-I", cont531);
    var bancaI = ContPropriuAper("-BANCA-I", cont770);
    os.CommitChanges();

    var idH1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 16), casaH, bancaH, 400m));
    var idHc = OperareApi.Opereaza(os, idH1).ConexId ?? Guid.Empty;
    OperareApi.Opereaza(os, idHc);
    OperareApi.Storneaza(os, idHc, new DateOnly(2026, 5, 17));
    var numarH1 = TrezorerieApply.Citeste<Plata>(os, idH1).Numar;

    Check("ANCORA D1 (descriptiv vs decizional): pe sursa cu perechea STORNATĂ, `Pereche` o ARATĂ în continuare (o poți deschide), dar `PerecheActiva` = FALSE — 581 e din nou deschis, iar clientul ramifică pe boolean, nu pe stare",
        TrezorerieApply.Citeste<Plata>(os, idH1) is { Stare: "Operat", PerecheActiva: false } citH1
        && citH1.Pereche != null && citH1.Pereche.Id == idHc && citH1.Pereche.Stare == "Stornat");
    Check("ANCORA D1-A: sursa REDEVINE candidat pe endpoint după stornarea perechii (înainte, pointer-ul „nu e Draft” o scotea definitiv) — și fără mențiune de draft blocant, fiindcă nu există",
        TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casaH.ID, bancaH.ID, null)
            .SingleOrDefault(c => c.Id == idH1) is { Stare: "Operat", PerecheDraftNumar: null });

    // Legarea manuală de o țintă al cărei POINTER e stornat: PERMISĂ (remediul
    // imposibil a dispărut).
    var idH3 = TrezorerieApply.Aplica<Incasare>(os, null,
        ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idH1));
    Check("ANCORA D1: legarea manuală de o țintă arătată doar de un pointer STORNAT trece dry-run-ul — altfel refuzul era o fundătură (pointer-ul stornat nu se mai poate nici edita, nici șterge)",
        DryRunAper(idH3).Count == 0);
    // Aceeași regulă pe cealaltă jumătate a punctului 5: ținta are LINK PROPRIU,
    // dar spre un document stornat.
    var idHLegatDeStornat = TrezorerieApply.Aplica<Plata>(os, null,
        ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idHc));
    var idHSpreEa = TrezorerieApply.Aplica<Incasare>(os, null,
        ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idHLegatDeStornat));
    Check("ANCORA D1: nici linkul PROPRIU al țintei nu mai blochează dacă arată spre un document STORNAT — o singură noțiune („capătul stornat nu contează”), aplicată în ambele jumătăți ale punctului 5",
        DryRunAper(idHSpreEa).Count == 0);
    TrezorerieApply.Sterge<Incasare>(os, idHSpreEa);
    TrezorerieApply.Sterge<Plata>(os, idHLegatDeStornat);
    TrezorerieApply.Sterge<Incasare>(os, idH3);

    // Avertismentul: piciorul descoperit reintră în listă.
    var idH2 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m));
    var rezH2 = OperareApi.Opereaza(os, idH2);
    Check("ANCORA D1-A: un nou picior cules manual îl NUMEȘTE pe cel rămas descoperit în avertismentul consultativ — pe criteriul vechi era tăcere, deci gaura 64k se redeschidea tăcut după orice storno",
        rezH2.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarH1)));
    OperareApi.AnuleazaOperarea(os, idH2);
    var idH2Conex = os.GetObjectsQuery<DocumentTrezorerie>()
        .Where(x => x.DocumentSursaId == idH2).Select(x => (Guid?)x.ID).FirstOrDefault();
    Check("…iar anularea lui i-a șters draftul autogenerat (artefact al operării) — starea rămâne curată pentru proba următoare",
        idH2Conex == null);
    TrezorerieApply.Sterge<Incasare>(os, idH2);

    // D1-B: anulare + re-operare REGENEREAZĂ perechea.
    Check("ANCORA D1-B (gardianul rămâne pe Operat): ținta cu pointer STORNAT se anulează LIBER — gardianul apără registre, iar registrele pointer-ului sunt deja inversate",
        OperareApi.AnuleazaOperarea(os, idH1).StareNoua == StareDocument.Draft
        && os.GetObjectByKey<Incasare>(idHc) is { Stare: StareDocument.Stornat });
    var rezH1Reoperat = OperareApi.Opereaza(os, idH1);
    var idHc2 = rezH1Reoperat.ConexId ?? Guid.Empty;
    Check("ANCORA D1-B: re-operarea sursei REGENEREAZĂ latura pereche (suprimarea se uită la perechea ACTIVĂ, nu la orice pointer) — altfel indiciul din client prescria exact o operațiune care nu făcea nimic",
        idHc2 != Guid.Empty && os.GetObjectByKey<Incasare>(idHc2).LaturaPerecheId == idH1);
    OperareApi.Opereaza(os, idHc2);
    Check("ANCORA D1-B: perechea regenerată se operează (pointer-ul stornat nu mai blochează validarea) și 581 se închide la 0 peste TOATE cele trei documente — stornarea a golit contribuția piciorului anulat",
        TrezorerieApply.Citeste<Incasare>(os, idHc2) is { Stare: "Operat" }
        && Sold581(idH1, idHc, idHc2) == 0m
        && TrezorerieApply.Citeste<Plata>(os, idH1) is { PerecheActiva: true } dupaRegen
        && dupaRegen.Pereche.Id == idHc2);

    // ── (I) D2: linkul golit pe copilul autogenerat ─────────────────────────
    // `LaturaPerecheId` e câmp CULES, deci pe draftul autogenerat operatorul îl
    // poate goli cu un click. Datele rămân corecte (gardul `Autogenerat` ține),
    // dar o citire numai pe link ar declara „latura pereche lipsește, 581 rămâne
    // deschis" despre un document care o ARE — iar sfatul „culegeți manual
    // piciorul celălalt" ar produce chiar dubla postare pe care felia o închide.
    var idI1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 19), casaI, bancaI, 700m));
    var idIc = OperareApi.Opereaza(os, idI1).ConexId ?? Guid.Empty;
    Check("ANCORA D1 (PerecheActiva pe DRAFT): perechea abia generată e Draft — intenția celui de-al doilea picior — deci ACTIVĂ; `Stornat` e singura stare care nu contează",
        TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } inainteDeGolire
        && inainteDeGolire.Pereche.Stare == "Draft");
    TrezorerieApply.Aplica<Incasare>(os, idIc, ScrieVir(new DateOnly(2026, 5, 19), casaI, bancaI, 700m));
    Check("ANCORA D2: după golirea linkului pe copilul autogenerat, SURSA își vede perechea prin grupul conex (`DocumentSursaId` + `Autogenerat`) și o raportează ACTIVĂ — panoul nu mai poate spune „581 rămâne deschis” despre un document care are perechea",
        os.GetObjectByKey<Incasare>(idIc).LaturaPerecheId == null
        && TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } dupaGolire
        && dupaGolire.Pereche != null && dupaGolire.Pereche.Id == idIc);
    Check("ANCORA D2 (oglinda, adăugire peste literă — vezi raportul): și COPILUL își vede sursa, altfel aceeași minciună se citea de pe celălalt ecran — iar acolo sfatul „re-operați” e inert prin construcție (`Autogenerat`)",
        TrezorerieApply.Citeste<Incasare>(os, idIc) is { PerecheActiva: true } citCopil
        && citCopil.Pereche != null && citCopil.Pereche.Id == idI1);
    bool aGeneratDinNou;
    using (var osProba = provider.CreateObjectSpace()) {
        // ObjectSpace ARUNCAT, necomis: hook-ul CREEAZĂ documentul dacă decide
        // să genereze, iar proba e chiar despre absența lui.
        aGeneratDinNou = osProba.GetObjectByKey<Plata>(idI1).GenereazaSecundar(osProba) != null;
    }
    Check("ANCORA D2: suprimarea generării ține și cu linkul golit — sursa nu naște un al doilea copil (gardul de grup conex, nu doar cel de link)",
        !aGeneratDinNou);
    OperareApi.Opereaza(os, idIc);
    Check("ANCORA D1 (PerecheActiva pe OPERAT): perechea operată e activă, 581 se închide la 0 — golirea linkului a rămas o chestiune de AFIȘARE, datele n-au fost niciodată în pericol",
        TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } dupaOperare
        && dupaOperare.Pereche.Stare == "Operat"
        && Sold581(idI1, idIc) == 0m);

    CurataAper(os);
    Check("Curățenie finală felia pereche prin API (fără reziduuri e2e)",
        !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAper))
        && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajAper))
        && os.GetObjectByKey<Plata>(idA1) == null
        && os.GetObjectByKey<Incasare>(idD2) == null);
}

VerificaBalanta();
VerificaFisaJurnal();

Rezumat();

// ============ Felia 9 (raportare): balanța de verificare (R-D1…R-D4) ============
// Proiecția e un al DOILEA adevăr dacă nu e legată de primul (precedentul D9:
// `SoldStoc` == `StocService.Sold`). Aici primul adevăr e chiar registrul, citit
// naiv în memorie — plus invarianții care nu depind de scenariu (partidă dublă,
// continuitate), verificați peste TOATE rândurile bazei, nu doar peste ale
// noastre.
//
// Local function, apelată din AMBELE căi de profil (blocul privat iese cu
// `return` înainte de suita bugetară) — o singură definiție a probelor.
//
// Scenariul e ales ca fiecare risc pin-uit în contract să aibă un rând al lui:
// granițele de dată (exact `dataStart`, exact `dataEnd`, o zi după), storno
// căzând în perioadă, cont cu sold inițial și ZERO mișcare, cont cu mișcare care
// se netează la zero, același cont cu solduri de sensuri OPUSE pe doi repartitori
// (capcana R-D4), dimensiune pusă doar pe o LATURĂ. Toate rândurile au
// `DocumentId == null` — adică exact forma rândurilor de deschidere scrise de
// migrare (25e/34d), care nu trebuie să pice pe nicio navigație presupusă nenulă.
void VerificaBalanta() {
    const string MarcajBal = "E2E-BAL";
    using var os = provider.CreateObjectSpace();

    void CurataBal() {
        var conturiBal = os.GetObjectsQuery<Cont>()
            .Where(c => c.Simbol.StartsWith(MarcajBal)).Select(c => c.ID).ToList();
        os.Delete(os.GetObjectsQuery<RegistruContabil>()
            .Where(r => conturiBal.Contains(r.ContDebitId) || conturiBal.Contains(r.ContCreditId)).ToList());
        os.Delete(os.GetObjectsQuery<Cont>().Where(c => c.Simbol.StartsWith(MarcajBal)).ToList());
        os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajBal)).ToList());
        os.Delete(os.GetObjectsQuery<Proiect>().Where(p => p.Cod.StartsWith(MarcajBal)).ToList());
        os.CommitChanges();
        // Scena D4 ȘTERGE deliberat un `Cont` (soft delete), iar un obiect deja
        // șters nu mai iese din `GetObjectsQuery` — deci nici el, nici rândurile
        // lui de registru n-ar fi culese de curățenia de mai sus, iar reziduul ar
        // crește cu fiecare rulare. Purja e SQL direct, singurul mod de a atinge
        // rândurile de sub filtrul global `GCRecord`. Ordinea respectă FK-ul
        // (registrul referă conturile); rulează după commit, deci EF n-are nimic
        // în zbor.
        var db = ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)os).DbContext;
        db.Database.ExecuteSql($"DELETE FROM \"RegistruContabil\" WHERE \"NumarNota\" = {MarcajBal}");
        db.Database.ExecuteSql($"DELETE FROM \"Conturi\" WHERE \"Simbol\" LIKE {MarcajBal + "%"}");
    }
    CurataBal();

    Cont ContBal(string sufix) {
        var c = os.CreateObject<Cont>();
        c.Simbol = MarcajBal + sufix;
        c.Denumire = "Cont probă balanță " + sufix;
        return c;
    }
    UnitateInterna RepBal(string sufix) {
        var r = os.CreateObject<UnitateInterna>();
        r.Cod = MarcajBal + sufix;
        r.Denumire = "Repartitor probă balanță " + sufix;
        return r;
    }

    var c1 = ContBal("-1");   // solduri de ambele sensuri, pe doi repartitori
    var c2 = ContBal("-2");   // contrapartida
    var c3 = ContBal("-3");   // mișcare care se netează la ZERO
    var c4 = ContBal("-4");   // sold inițial și ZERO mișcare în perioadă
    var c5 = ContBal("-5");   // rânduri FĂRĂ repartitor lângă unul CU (review D3)
    var c6 = ContBal("-6");   // contul căruia îi dispare eticheta (review D4)
    var repA = RepBal("-A");
    var repB = RepBal("-B");
    var proiect = os.CreateObject<Proiect>();
    proiect.Cod = MarcajBal + "-P";
    proiect.Denumire = "Proiect probă balanță";

    void Nota(DateOnly data, Cont debit, Cont credit, decimal valoare,
        Repartitor repDebit, Repartitor repCredit, bool storno = false, Proiect proiectDebit = null) {
        var n = os.CreateObject<RegistruContabil>();
        n.Data = data;
        n.NumarNota = MarcajBal;
        n.ContDebit = debit;
        n.ContCredit = credit;
        n.Valoare = valoare;
        n.Storno = storno;
        n.DebitRepartitor = repDebit;
        n.CreditRepartitor = repCredit;
        n.DebitProiect = proiectDebit;
    }

    var ds = new DateOnly(2026, 4, 1);
    var de = new DateOnly(2026, 4, 30);

    Nota(new DateOnly(2026, 3, 15), c1, c2, 100m, repA, repA);                      // inițial
    Nota(new DateOnly(2026, 3, 15), c2, c1, 300m, repB, repB);                      // inițial, sens opus pe c1
    Nota(new DateOnly(2026, 3, 20), c4, c2, 70m, repA, repA);                       // c4: doar sold inițial
    Nota(ds, c1, c3, 50m, repA, repA, proiectDebit: proiect);                       // EXACT dataStart ⇒ rulaj
    Nota(de, c3, c1, 50m, repA, repA);                                              // EXACT dataEnd ⇒ inclus
    Nota(new DateOnly(2026, 4, 10), c2, c1, 40m, repA, repB, storno: true);         // storno ⇒ intră (R-D7)
    Nota(new DateOnly(2026, 5, 1), c1, c2, 999m, repA, repA);                       // după dataEnd ⇒ exclus
    // c5 (review D3): pe latura lui, DOUĂ rânduri fără repartitor (unul înainte de
    // perioadă, unul în ea) și unul CU — adică exact forma bazei de import
    // (deschiderea scrisă fără dimensiuni, 47c; 2025 fără dimensiuni culese pe
    // linie, DIM-2). Analitic ies două rânduri pe același cont: „fără repartitor"
    // și „repA".
    Nota(new DateOnly(2026, 3, 25), c5, c2, 200m, null, repA);                      // inițial, fără repartitor
    Nota(new DateOnly(2026, 4, 5), c5, c2, 30m, null, repA);                        // rulaj, fără repartitor
    Nota(new DateOnly(2026, 4, 6), c5, c2, 11m, repA, repA);                        // rulaj, CU repartitor
    Nota(new DateOnly(2026, 4, 12), c6, c2, 17m, repA, repA);                       // contul cu eticheta ștearsă
    os.CommitChanges();

    var sintetic = ContabilProiectii.Balanta(os, ds, de).ToList();
    BalantaRand Rand(Cont c) => sintetic.SingleOrDefault(r => r.ContId == c.ID);

    // ── 1. Partida dublă ────────────────────────────────────────────────────
    Check("R-D1: unpivot-ul păstrează partida dublă — Σ RulajDebit == Σ RulajCredit (și Σ Initial*) peste TOATĂ balanța bazei, nu doar peste scenariu",
        sintetic.Sum(r => r.RulajDebit) == sintetic.Sum(r => r.RulajCredit)
        && sintetic.Sum(r => r.InitialDebit) == sintetic.Sum(r => r.InitialCredit));

    // ── Scenariul pin-uit: granițe, storno, cazurile-limită ─────────────────
    Check("R-D3: granițele de dată — rândul de EXACT `dataStart` e RULAJ (nu sold inițial), cel de EXACT `dataEnd` e inclus, cel de a doua zi după `dataEnd` e exclus; rândul de STORNO intră ca orice rând (R-D7)",
        Rand(c1) is { InitialDebit: 100m, InitialCredit: 300m, RulajDebit: 50m, RulajCredit: 90m });
    Check("R-D4: netarea la nivelul cheii — c1 iese `SoldInițial C200` și `SoldFinal C240` (net = 100−300+50−90), cu sumele brute păstrate alături",
        Rand(c1) is { SoldInitialDebit: 0m, SoldInitialCredit: 200m, SoldFinalDebit: 0m, SoldFinalCredit: 240m });
    Check("Risc 4a: contul cu sold inițial și ZERO mișcare în perioadă APARE în balanță (asta pică forma naivă cu două agregări + join)",
        Rand(c4) is { InitialDebit: 70m, InitialCredit: 0m, SoldInitialDebit: 70m, RulajDebit: 0m, RulajCredit: 0m, SoldFinalDebit: 70m, SoldFinalCredit: 0m });
    Check("Risc 4b: contul a cărui mișcare se netează la ZERO apare cu rulaje nenule și sold 0 pe ambele coloane (nu dispare)",
        Rand(c3) is { InitialDebit: 0m, InitialCredit: 0m, RulajDebit: 50m, RulajCredit: 50m, SoldFinalDebit: 0m, SoldFinalCredit: 0m });
    Check("Risc 8: rândurile cu `DocumentId == null` (forma soldurilor de deschidere scrise de migrare) intră normal — proiecția nu atinge navigația `Document`",
        os.GetObjectsQuery<RegistruContabil>().Count(r => r.NumarNota == MarcajBal && r.DocumentId == null) == 11);

    // ── 2. Balanța == recomputare naivă în memorie, pe un eșantion ───────────
    // Primul adevăr = registrul citit rând cu rând și însumat în C#. Eșantionul:
    // conturile scenariului + primele câteva conturi REALE ale bazei (pe profilul
    // privat/bugetar sunt cele lăsate de celelalte blocuri e2e și de seed).
    var esantion = new List<Guid> { c1.ID, c2.ID, c3.ID, c4.ID };
    esantion.AddRange(os.GetObjectsQuery<RegistruContabil>()
        .Where(r => !esantion.Contains(r.ContDebitId))
        .Select(r => r.ContDebitId).Distinct().Take(5).ToList());
    var naivOk = true;
    foreach (var contId in esantion.Distinct()) {
        var randuri = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => (r.ContDebitId == contId || r.ContCreditId == contId) && r.Data <= de)
            .Select(r => new { r.Data, r.ContDebitId, r.ContCreditId, r.Valoare }).ToList();
        var iniD = randuri.Where(r => r.ContDebitId == contId && r.Data < ds).Sum(r => r.Valoare);
        var iniC = randuri.Where(r => r.ContCreditId == contId && r.Data < ds).Sum(r => r.Valoare);
        var rulD = randuri.Where(r => r.ContDebitId == contId && r.Data >= ds).Sum(r => r.Valoare);
        var rulC = randuri.Where(r => r.ContCreditId == contId && r.Data >= ds).Sum(r => r.Valoare);
        var netI = iniD - iniC;
        var netF = netI + rulD - rulC;
        var rand = sintetic.SingleOrDefault(r => r.ContId == contId);
        if (randuri.Count == 0) {
            naivOk &= rand == null;
            continue;
        }
        naivOk &= rand != null
            && rand.InitialDebit == iniD && rand.InitialCredit == iniC
            && rand.RulajDebit == rulD && rand.RulajCredit == rulC
            && rand.SoldInitialDebit == (netI > 0 ? netI : 0m)
            && rand.SoldInitialCredit == (netI < 0 ? -netI : 0m)
            && rand.SoldFinalDebit == (netF > 0 ? netF : 0m)
            && rand.SoldFinalCredit == (netF < 0 ? -netF : 0m);
    }
    Check($"Balanța == recomputarea NAIVĂ din registru, în memorie, pe un eșantion de {esantion.Distinct().Count()} conturi (toate cele 8 cifre per cont)",
        naivOk);

    // ── 3. Analitic ⇒ sintetic pe RULAJE (soldurile, deliberat, nu) ──────────
    var analitic = ContabilProiectii.Balanta(os, ds, de, analitic: true).ToList();
    var c1A = analitic.SingleOrDefault(r => r.ContId == c1.ID && r.RepartitorId == repA.ID);
    var c1B = analitic.SingleOrDefault(r => r.ContId == c1.ID && r.RepartitorId == repB.ID);
    Check("R-D4 (capcana): pe același cont, analiticul dă `D100` pe repartitorul A și `C340` pe B, în timp ce sinteticul netează la `C240` — ambele corecte, la niveluri diferite; de asta modul se CERE explicit, nu se deduce",
        c1A is { SoldFinalDebit: 100m, SoldFinalCredit: 0m }
        && c1B is { SoldFinalDebit: 0m, SoldFinalCredit: 340m }
        && Rand(c1) is { SoldFinalCredit: 240m });
    Check("Modul analitic poartă denumirea repartitorului din join-ul pe rezultatul agregat; cel sintetic lasă cheia goală pe TOATE rândurile",
        c1A.RepartitorDenumire == repA.Denumire && c1B.RepartitorDenumire == repB.Denumire
        && sintetic.All(r => r.RepartitorId == null && r.RepartitorDenumire == null));
    var rulajeAnalitic = analitic.GroupBy(r => r.ContId).ToDictionary(g => g.Key,
        g => (D: g.Sum(x => x.RulajDebit), C: g.Sum(x => x.RulajCredit),
              ID: g.Sum(x => x.InitialDebit), IC: g.Sum(x => x.InitialCredit)));
    Check("Rulajele (și sumele brute inițiale) SUNT aditive: însumate per cont, balanța analitică == cea sintetică, cont cu cont, pe toată baza",
        rulajeAnalitic.Count == sintetic.Count
        && sintetic.All(s => rulajeAnalitic.TryGetValue(s.ContId, out var a)
            && a.D == s.RulajDebit && a.C == s.RulajCredit
            && a.ID == s.InitialDebit && a.IC == s.InitialCredit));

    // ── Cusătura ANALITICĂ balanță ↔ fișă, inclusiv „fără repartitor" (D3) ──
    // Verificarea 5 din contract exista doar pe SINTETIC — adică exact pe modul în
    // care drill-down-ul nu putea greși. Pe rândul ANALITIC, fișa trebuie să se
    // închidă pe cifra RÂNDULUI, nu pe a contului; iar rândul „fără repartitor" e
    // cazul care n-avea cum: `Guid?` nu poate exprima „absent" (null = „fără
    // filtru"), deci drill-down-ul deschidea fișa NEfiltrată și se închidea pe
    // soldul SINTETIC. Santinela `repartitorNul` e a treia valoare.
    var c5Nul = analitic.SingleOrDefault(r => r.ContId == c5.ID && r.RepartitorId == null);
    var c5A = analitic.SingleOrDefault(r => r.ContId == c5.ID && r.RepartitorId == repA.ID);
    var fisaC5Nul = ContabilProiectii.FisaCont(os, c5.ID, ds, de, repartitorNul: true).ToList();
    var fisaC5A = ContabilProiectii.FisaCont(os, c5.ID, ds, de, repartitorId: repA.ID).ToList();
    var fisaC5Tot = ContabilProiectii.FisaCont(os, c5.ID, ds, de).ToList();
    Check("Cusătura pe rândul ANALITIC „fără repartitor” (review D3): santinela `repartitorNul` selectează exact rândurile cu dimensiunea ABSENTĂ pe latura lor — fișa lor se închide pe soldul RÂNDULUI (230, cu soldul inițial de 200 al aceleiași chei), nu pe cel sintetic al contului (241)",
        c5Nul is { InitialDebit: 200m, RulajDebit: 30m, SoldFinalDebit: 230m }
        && fisaC5Nul.Count == 1
        && fisaC5Nul[^1].SoldCurent == c5Nul.SoldFinalDebit - c5Nul.SoldFinalCredit
        && fisaC5Tot[^1].SoldCurent == 241m);
    Check("Aceeași cusătură pe rândul analitic CU repartitor, pe același cont: fișa filtrată se închide pe 11, iar cele două fișe analitice sunt DISJUNCTE și reconstituie împreună fișa contului (1 + 1 == 2 rânduri)",
        c5A is { InitialDebit: 0m, RulajDebit: 11m, SoldFinalDebit: 11m }
        && fisaC5A.Count == 1
        && fisaC5A[^1].SoldCurent == c5A.SoldFinalDebit - c5A.SoldFinalCredit
        && fisaC5Nul.Count + fisaC5A.Count == fisaC5Tot.Count
        && !fisaC5Nul.Select(r => r.Id).Intersect(fisaC5A.Select(r => r.Id)).Any());

    // ── 4. Continuitate: SoldInițial(N+1) == SoldFinal(N) ───────────────────
    var martie = ContabilProiectii.Balanta(os, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)).ToList();
    var netFinalMartie = martie.ToDictionary(r => r.ContId, r => r.SoldFinalDebit - r.SoldFinalCredit);
    Check("Continuitate: soldul inițial al lunii aprilie == soldul final al lunii martie, pe FIECARE cont al bazei (iar un cont care apare în martie nu poate lipsi din aprilie)",
        sintetic.All(r => (netFinalMartie.TryGetValue(r.ContId, out var net) ? net : 0m)
                == r.SoldInitialDebit - r.SoldInitialCredit)
        && martie.All(r => sintetic.Any(a => a.ContId == r.ContId)));

    // ── Filtrele de dimensiune: pre-agregare, pe LATURA corectă (risc 7) ─────
    var peProiect = ContabilProiectii.Balanta(os, ds, de, proiectId: proiect.ID).ToList();
    Check("Risc 7: filtrul de dimensiune se aplică pe atomi (înainte de `GROUP BY`) și pe LATURA LUI — proiectul e pus doar pe DEBITUL unui rând, deci apare doar contul debitor cu rulajul lui; contul creditor al ACELUIAȘI rând nu intră deloc",
        peProiect.Count == 1 && peProiect[0].ContId == c1.ID
        && peProiect[0] is { RulajDebit: 50m, RulajCredit: 0m, InitialDebit: 0m, InitialCredit: 0m });
    var peRepB = ContabilProiectii.Balanta(os, ds, de, repartitorId: repB.ID).ToList();
    Check("Filtrul pe Repartitor (dimensiune, nu cheie de grupare) taie tot ce nu-i aparține: c1 rămâne cu latura lui creditoare (300 inițial + 40 storno), c2 doar cu debitul inițial de 300",
        peRepB.Count == 2
        && peRepB.Single(r => r.ContId == c1.ID) is { InitialDebit: 0m, InitialCredit: 300m, RulajCredit: 40m, RulajDebit: 0m }
        && peRepB.Single(r => r.ContId == c2.ID) is { InitialDebit: 300m, InitialCredit: 0m, RulajDebit: 0m, RulajCredit: 0m });

    // ── Sonda de traducere: filtrare + sortare + paginare peste proiecție ────
    var sonda = ContabilProiectii.Balanta(os, ds, de).Where(r => r.ContSimbol.StartsWith(MarcajBal))
        .OrderByDescending(r => r.RulajCredit).Take(1).ToList();
    Check("Filtrarea/sortarea/paginarea se traduc în SQL PESTE proiecție (sondă: where + order + take → un singur rând, cel cu rulajul creditor maxim) — adică exact ce pune `DataSourceLoader` deasupra",
        sonda.Count == 1 && sonda[0].ContId == c1.ID && sonda[0].RulajCredit == 90m);
    Check("Perioada de o SINGURĂ zi: `dataStart == dataEnd` pe ziua unui rând — rândul e rulaj, iar tot ce e înainte devine sold inițial",
        ContabilProiectii.Balanta(os, de, de).SingleOrDefault(r => r.ContId == c3.ID)
            is { RulajDebit: 50m, RulajCredit: 0m, InitialCredit: 50m, SoldFinalDebit: 0m, SoldFinalCredit: 0m });

    // ══ REGRESIE: balanța prin `DataSourceLoader`, PAGINATĂ (review D2) ═══════
    //
    // Golul de acoperire care a permis defectul: TOATE verificările de mai sus
    // consumă `IQueryable`-ul direct (`.ToList()`) — balanța nu era încărcată prin
    // `DataSourceLoader` NICĂIERI, adică exact forma de punct orb care a produs și
    // defectul de ordine al feliei (vezi blocul omolog din fișă).
    //
    // Ce se rupea: fără ordine declarată, biblioteca își pune ordinea EI, iar pe
    // `BalantaRand` convenția EF nimerește `ContId` — cheie unică în modul
    // sintetic, dar REPETATĂ în cel analitic (cheia de grupare e `Cont ×
    // Repartitor`). `ORDER BY` pe cheie ne-unică sub `LIMIT/OFFSET` n-are ordine
    // garantată: un rând poate apărea pe două pagini sau pe niciuna. Postgres nu
    // randomizează, deci nu se manifesta la o rulare oarecare — dar garanția
    // lipsea, iar proba de mai jos o cere pe cea TARE: reuniunea paginilor ==
    // exact mulțimea dintr-o singură cerere, fără duplicate și fără rânduri sărite.
    //
    // Filtrul pe `ContSimbol` e chiar ce pune grila pe coloanele de ieșire
    // (legitim, R-D2) și ține scena mărginită la conturile blocului.
    List<BalantaRand> BalantaPrinLoader(bool cheieDubla, int skip, int take) {
        var optiuni = new DataSourceLoadOptionsBase {
            Skip = skip, Take = take,
            Filter = new object[] { "ContSimbol", "startswith", MarcajBal }
        };
        OrdineLista.AplicaOrdineImplicita(optiuni, ContabilProiectii.OrdineBalanta(cheieDubla));
        return DataSourceLoader.Load(ContabilProiectii.Balanta(os, ds, de, cheieDubla), optiuni)
            .data.Cast<BalantaRand>().ToList();
    }
    var paginareOk = true;
    var modAnaliticAreCheieRepetata = false;
    foreach (var cheieDubla in new[] { false, true }) {
        var totul = BalantaPrinLoader(cheieDubla, 0, 1000);
        string Cheie(BalantaRand r) => $"{r.ContId}|{r.RepartitorId}";
        // Premisa: în modul analitic cheia bibliotecii (`ContId`) chiar se repetă —
        // altfel proba n-ar avea dinți (c1 pe doi repartitori, c5 pe „null + repA").
        if (cheieDubla)
            modAnaliticAreCheieRepetata = totul.GroupBy(r => r.ContId).Any(g => g.Count() > 1);
        var pagini = new List<BalantaRand>();
        for (var skip = 0; skip < totul.Count; skip += 2)
            pagini.AddRange(BalantaPrinLoader(cheieDubla, skip, 2));
        paginareOk &= pagini.Count == totul.Count
            && pagini.Select(Cheie).Distinct().Count() == pagini.Count
            && pagini.Select(Cheie).OrderBy(k => k).SequenceEqual(totul.Select(Cheie).OrderBy(k => k))
            // …și ordinea declarată chiar ajunge la Postgres: paginile concatenate
            // reproduc secvența întreagă, rând cu rând.
            && pagini.Select(Cheie).SequenceEqual(totul.Select(Cheie));
    }
    Check("REGRESIE (review D2): prin `DataSourceLoader`, paginată din 2 în 2, balanța reproduce EXACT mulțimea unei singure cereri — fără duplicate și fără rânduri sărite — în AMBELE moduri; premisa (cheia bibliotecii, `ContId`, chiar se repetă în modul analitic) e verificată în aceeași trecere",
        paginareOk && modAnaliticAreCheieRepetata);

    // ══ D4: contului îi dispare ETICHETA, atomul rămâne ══════════════════════
    // Cu INNER JOIN linia DISPĂREA din balanță și `Σ RulajDebit != Σ RulajCredit`
    // în footer, fără nicio explicație — în timp ce fișa aceluiași cont mergea
    // perfect (ea joinează LEFT).
    //
    // Cum se ajunge în starea asta, măsurat aici, nu presupus: `os.Delete(cont)`
    // NU e calea — ștergerea prin ObjectSpace CASCADEAZĂ la rândurile de registru
    // (probat: după ea, contul rămâne fără niciun rând, deci atomul dispare cu
    // totul și partida dublă rămâne întreagă de la sine). Starea periculoasă e
    // aceea în care ATOMII SUPRAVIEȚUIESC etichetei: contul invizibil prin
    // SECURITATE — pe care ModelCheck, rulând pe un provider standalone
    // NEsecurizat, nu-l poate simula — și, cu aceeași formă exactă pentru
    // interogare, contul marcat șters direct în bază (import, migrare, script).
    // Scena o produce deci prin SQL: marcajul de ștergere pe `Cont`, rândurile de
    // registru neatinse. Din perspectiva interogării, cele două cazuri sunt
    // identice: join-ul pe etichetă nu găsește nimic.
    var c6Inainte = sintetic.SingleOrDefault(r => r.ContId == c6.ID);
    ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)os).DbContext.Database
        .ExecuteSql($"UPDATE \"Conturi\" SET \"GCRecord\" = 1 WHERE \"ID\" = {c6.ID}");
    var dupaStergereCont = ContabilProiectii.Balanta(os, ds, de).ToList();
    var c6Dupa = dupaStergereCont.SingleOrDefault(r => r.ContId == c6.ID);
    // Diagnostic la cerere (`MODELCHECK_D4=1`), ca `MODELCHECK_SQL` de la fișă:
    // scena de mai sus e singura din bloc care depinde de o mecanică ascunsă
    // (cascadarea ștergerii), deci merită să se poată inspecta fără a modifica cod.
    if (Environment.GetEnvironmentVariable("MODELCHECK_D4") == "1")
        Console.WriteLine($"[D4] inainte={c6Inainte?.ContSimbol ?? "<lipsa rand>"}/{c6Inainte?.RulajDebit} "
            + $"dupa={(c6Dupa == null ? "<lipsa rand>" : $"simbol={c6Dupa.ContSimbol ?? "<null>"} den={c6Dupa.ContDenumire ?? "<null>"} rulD={c6Dupa.RulajDebit} sfD={c6Dupa.SoldFinalDebit}")} "
            + $"count {sintetic.Count}->{dupaStergereCont.Count} "
            + $"partida {dupaStergereCont.Sum(r => r.RulajDebit)}/{dupaStergereCont.Sum(r => r.RulajCredit)} "
            + $"randuriRegistruC6={os.GetObjectsQuery<RegistruContabil>().Count(r => r.ContDebitId == c6.ID)}");
    Check("D4 (review advers): contului îi dispare ETICHETA (marcat șters în bază / invizibil prin securitate), dar atomul lui NU se pierde — rândul rămâne în balanță cu simbolul și denumirea goale și cu rulajul intact (17), numărul de rânduri e neschimbat, iar partida dublă rămâne întreagă peste TOATĂ balanța (cu INNER JOIN, linia dispărea tăcut și Σ debit != Σ credit)",
        c6Inainte is { ContSimbol: MarcajBal + "-6", RulajDebit: 17m }
        && c6Dupa is { ContSimbol: null, ContDenumire: null, RulajDebit: 17m, SoldFinalDebit: 17m }
        && dupaStergereCont.Count == sintetic.Count
        && dupaStergereCont.Sum(r => r.RulajDebit) == dupaStergereCont.Sum(r => r.RulajCredit));
    // …și fișa ACELUIAȘI cont (calea SQL brut) rămâne cusută pe aceeași cifră:
    // ea joinează LEFT de la început, deci cele două căi nu mai divergeau.
    Check("D4, cusătura: fișa contului fără etichetă se închide pe aceeași cifră ca rândul lui de balanță (17) — cele două căi nu divergeau doar pe join-ul de etichetă, iar acum nu divergează deloc",
        ContabilProiectii.FisaCont(os, c6.ID, ds, de).ToList() is { Count: 1 } fisaC6
        && fisaC6[^1].SoldCurent == c6Dupa.SoldFinalDebit - c6Dupa.SoldFinalCredit);

    CurataBal();
    Check("Curățenie finală felia balanță (fără reziduuri e2e)",
        !os.GetObjectsQuery<Cont>().Any(c => c.Simbol.StartsWith(MarcajBal))
        && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajBal))
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.NumarNota == MarcajBal));
}

// ====== Felia 9 (raportare): fișa de cont (R-D6/R-D8) + registrul-jurnal (R-D9) ======
// Verificarea 5 din contract e cea mai valoroasă a feliei: leagă calea SQL BRUT
// (fereastra fișei) de cea LINQ (balanța). Dacă cele două ar diverge, unul dintre
// rapoarte ar minți fără ca nimic să pice — exact defectul pe care o proiecție
// „al doilea adevăr" îl produce (precedentul D9: `SoldStoc` == `StocService.Sold`).
//
// Scenariul e ales ca fiecare risc pin-uit să aibă un rând: rând cu ACELAȘI cont
// pe ambele laturi (două rânduri de fișă, același `Id`), rând de storno, rânduri
// de deschidere (`DocumentId == null`), granițe de dată, rând legat de un
// document real (codul de tip, R-D8), filtrare peste proiecție (soldul curent
// rămâne al REGISTRULUI), paginare (pagina 2 continuă soldul), ștergere amânată
// (soft delete-ul scris de mână în SQL brut — dacă lipsește, fișa arată rânduri
// șterse și divergează TĂCUT de balanță).
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaBalanta`.
void VerificaFisaJurnal() {
    const string MarcajFsa = "E2E-FSA";
    // Marcaj propriu pentru blocul de regresie a ordinii: numărătorile fixate ale
    // scenariului de bază (4 note în perioadă, 6 în total) rămân neatinse.
    const string MarcajOrd = MarcajFsa + "-ORD";
    using var os = provider.CreateObjectSpace();

    // `StartsWith`, nu egalitate: blocul de regresie de mai jos folosește un marcaj
    // PROPRIU (`E2E-FSA-ORD`), ca rândurile lui să nu intre în numărătorile fixate
    // ale scenariului de bază.
    void CurataFsa() {
        os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.NumarNota.StartsWith(MarcajFsa)).ToList());
        os.Delete(os.GetObjectsQuery<NotaTransfer>().Where(d => d.Numar == MarcajFsa).ToList());
        os.Delete(os.GetObjectsQuery<Cont>().Where(c => c.Simbol.StartsWith(MarcajFsa)).ToList());
        os.Delete(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajFsa)).ToList());
        os.CommitChanges();
    }
    CurataFsa();

    Cont ContFsa(string sufix) {
        var c = os.CreateObject<Cont>();
        c.Simbol = MarcajFsa + sufix;
        c.Denumire = "Cont probă fișă " + sufix;
        return c;
    }
    var cF1 = ContFsa("-1");   // contul sub test
    var cF2 = ContFsa("-2");   // contrapartida
    var repF = os.CreateObject<UnitateInterna>();
    repF.Cod = MarcajFsa + "-R";
    repF.Denumire = "Repartitor probă fișă";
    // Document REAL, doar ca ținta linkului: codul de tip nu e o coloană sub TPT,
    // se rezolvă prin ancora `TipDocument` după numele clasei CLR (R-D8/60b).
    // Rămâne Draft — nu se operează nimic; rândurile de registru sunt scrise de
    // mână, exact ca în blocul de balanță.
    var doc = os.CreateObject<NotaTransfer>();
    doc.Numar = MarcajFsa;
    doc.Data = new DateOnly(2026, 4, 5);
    doc.Predator = repF;
    doc.Primitor = repF;

    void NotaFsa(DateOnly data, Cont debit, Cont credit, decimal valoare,
        bool storno = false, Document document = null, Repartitor repDebit = null) {
        var n = os.CreateObject<RegistruContabil>();
        n.Data = data;
        n.NumarNota = MarcajFsa;
        n.ContDebit = debit;
        n.ContCredit = credit;
        n.Valoare = valoare;
        n.Storno = storno;
        n.Document = document;
        n.DebitRepartitor = repDebit;
    }

    var ds = new DateOnly(2026, 4, 1);
    var de = new DateOnly(2026, 4, 30);

    NotaFsa(new DateOnly(2026, 3, 10), cF1, cF2, 100m, repDebit: repF);   // sold inițial: D100
    NotaFsa(ds, cF1, cF2, 60m);                                           // EXACT dataStart => rulaj
    NotaFsa(new DateOnly(2026, 4, 5), cF2, cF1, 25m, document: doc);      // legat de document
    NotaFsa(new DateOnly(2026, 4, 10), cF1, cF1, 40m);                    // ACELAȘI cont pe ambele laturi
    NotaFsa(new DateOnly(2026, 4, 15), cF2, cF1, 10m, storno: true);      // storno => intră (R-D7)
    NotaFsa(new DateOnly(2026, 5, 1), cF1, cF2, 999m);                    // după dataEnd => exclus
    os.CommitChanges();

    var fisa = ContabilProiectii.FisaCont(os, cF1.ID, ds, de).ToList();

    // -- Ordinea fixă + soldul curent cumulat (R-D6) --------------------------
    Check("R-D6: fișa iese în ordinea FIXĂ `Data, Id, Sens DESC`, iar soldul curent pornește de la soldul INIȚIAL (D100, din afara perioadei) și se cumulă rând cu rând: 160 → 135 → 175 → 135 → 125",
        fisa.Count == 5
        && fisa[0] is { Sens: "D", Debit: 60m, Credit: 0m, SoldCurent: 160m }
        && fisa[1] is { Sens: "C", Debit: 0m, Credit: 25m, SoldCurent: 135m }
        && fisa[2] is { Sens: "D", Debit: 40m, SoldCurent: 175m }
        && fisa[3] is { Sens: "C", Credit: 40m, SoldCurent: 135m }
        && fisa[4] is { Sens: "C", Credit: 10m, Storno: true, SoldCurent: 125m });
    Check("R-D1 pe fișă: rândul cu ACELAȘI cont pe ambele laturi produce DOUĂ rânduri de fișă cu același `Id` (debitul înaintea creditului) — deci cheia de grilă e perechea (`Id`, `Sens`), niciodată `Id` singur",
        fisa[2].Id == fisa[3].Id && fisa[2].ContrapartidaId == cF1.ID);
    Check("Contrapartida și repartitorul laturii vin din join-uri LEFT: simbolul contului opus pe fiecare rând, denumirea repartitorului doar unde dimensiunea e pusă (rândurile fără ea rămân goale, nu dispar)",
        fisa.All(r => r.ContrapartidaSimbol == (r.ContrapartidaId == cF1.ID ? cF1.Simbol : cF2.Simbol))
        && fisa.All(r => r.RepartitorDenumire == null));
    Check("Risc 8 pe fișă: rândurile fără document (forma soldurilor de deschidere — 25e/34d) trec normal prin join-ul LEFT pe `Documente`; doar rândul legat poartă numărul",
        fisa.Count(r => r.DocumentId == null) == 4
        && fisa.Single(r => r.DocumentId != null) is { DocumentNumar: MarcajFsa } legat
        && legat.DocumentId == doc.ID);

    // -- R-D8: codul de tip, în memorie, peste pagină -------------------------
    Check("R-D8: `DocumentTip` iese NULL din SQL (sub TPT nu e o coloană) — se completează abia în memorie, peste pagină",
        fisa.All(r => r.DocumentTip == null));
    ContabilProiectii.CompleteazaTipDocument(os, fisa);
    Check("R-D8 (după completare): rândul legat poartă „BTR”, rândurile fără document rămân goale (nu pică pe nicio navigație presupusă nenulă)",
        fisa.Single(r => r.DocumentId != null).DocumentTip == "BTR"
        && fisa.Where(r => r.DocumentId == null).All(r => r.DocumentTip == null));

    // -- VERIFICAREA 5: cusătura fișă (SQL brut) <-> balanță (LINQ) -----------
    var balantaFsa = ContabilProiectii.Balanta(os, ds, de).ToList();
    var randF1 = balantaFsa.Single(r => r.ContId == cF1.ID);
    Check("VERIFICAREA 5 (cusătura feliei): ultimul `SoldCurent` din fișă == `SoldFinalDebit − SoldFinalCredit` din balanță, pe același cont și aceeași perioadă — calea SQL BRUT cu fereastră și calea LINQ cu `GROUP BY` dau aceeași cifră (125)",
        fisa[^1].SoldCurent == randF1.SoldFinalDebit - randF1.SoldFinalCredit
        && fisa[^1].SoldCurent == 125m);
    Check("Cusătura, și pe soldul INIȚIAL: `SoldCurent` al primului rând minus efectul lui == `SoldInițial` din balanță (fereastra pornește exact de unde se oprește agregarea de dinainte de `dataStart`)",
        fisa[0].SoldCurent - (fisa[0].Debit - fisa[0].Credit)
            == randF1.SoldInitialDebit - randF1.SoldInitialCredit);

    // Aceeași cusătură, pe conturile REALE ale bazei (nu doar pe scenariu): dacă
    // undeva soft delete-ul, granițele de dată sau unpivot-ul ar diferi între cele
    // două căi, un cont oarecare al bazei o arată.
    var startLarg = new DateOnly(2000, 1, 1);
    var endLarg = new DateOnly(2100, 1, 1);
    var balantaTot = ContabilProiectii.Balanta(os, startLarg, endLarg).ToList();
    var esantionFisa = balantaTot.OrderByDescending(r => r.RulajDebit + r.RulajCredit).Take(5).ToList();
    var cusaturaOk = esantionFisa.Count > 0;
    foreach (var rand in esantionFisa) {
        var f = ContabilProiectii.FisaCont(os, rand.ContId, startLarg, endLarg).ToList();
        var net = rand.SoldFinalDebit - rand.SoldFinalCredit;
        cusaturaOk &= f.Count > 0 && f[^1].SoldCurent == net
            && f.Sum(x => x.Debit) == rand.RulajDebit && f.Sum(x => x.Credit) == rand.RulajCredit;
    }
    Check($"VERIFICAREA 5, pe date REALE: pe cele mai traficate {esantionFisa.Count} conturi ale bazei, ultimul `SoldCurent` din fișă == soldul final din balanță, iar Σ Debit/Σ Credit ale fișei == rulajele balanței",
        cusaturaOk);

    // -- Filtrarea rămâne permisă; soldul curent rămâne AL REGISTRULUI --------
    var fisaFiltrata = ContabilProiectii.FisaCont(os, cF1.ID, ds, de).Where(r => r.Sens == "C").ToList();
    Check("Filtrarea se așază PESTE proiecție (exact ce pune `DataSourceLoader` deasupra) și NU recalculează nimic: cele trei rânduri creditoare păstrează soldurile registrului (135, 135, 125), nu un cumul al submulțimii (25, 65, 75)",
        fisaFiltrata.Count == 3
        && fisaFiltrata.Select(r => r.SoldCurent).SequenceEqual(new[] { 135m, 135m, 125m }));

    // -- Paginarea: pagina 2 continuă soldul, iar LIMIT/OFFSET ajung în SQL ---
    var interogarePaginata = ContabilProiectii.FisaCont(os, cF1.ID, ds, de).Skip(2).Take(2);
    var sqlPaginat = interogarePaginata.ToQueryString();
    var paginat = interogarePaginata.ToList();
    var pusInSql = sqlPaginat.Contains("LIMIT") && sqlPaginat.Contains("OFFSET");
    // SQL-ul compus se tipărește la EȘEC (ca diagnoza să fie în același loc cu
    // verdictul) sau la cerere — `MODELCHECK_SQL=1` — ca proba să se poată reface
    // fără să modifici codul: e singura interogare a repo-ului scrisă de mână.
    if (!pusInSql || Environment.GetEnvironmentVariable("MODELCHECK_SQL") == "1")
        Console.WriteLine(sqlPaginat);
    Check("Paginarea se împinge în SQL (`LIMIT`/`OFFSET` peste `IQueryable`-ul din `SqlQuery<T>`), nu în memorie — riscul #1 al contractului",
        pusInSql);
    Check("Pagina 2 continuă soldul corect (175, 135) — fereastra se calculează peste TOATĂ perioada, nu peste pagină",
        paginat.Count == 2
        && paginat.Select(r => r.SoldCurent).SequenceEqual(new[] { 175m, 135m }));

    // ══ REGRESIE: ordinea prin `DataSourceLoader`, nu prin `.ToList()` ═════════
    //
    // Verificările de mai sus consumă `IQueryable`-ul DIRECT, unde `OrderBy`-ul
    // proiecției e singurul din joc. Calea de API trece însă prin
    // `DataSourceLoader`, care — când cererea n-are `sort=` și are paginare — își
    // pune PROPRIA ordine (`Id`-ul singur), cu un `OrderBy` ce ȘTERGE ordinea
    // proiecției în EF Core: `ORDER BY "Data", "Id", "Sens" DESC` ajungea la
    // Postgres ca `ORDER BY "Id"`. Mecanica, cu sursele citate:
    // `Proiectii/OrdineLista.cs`.
    //
    // De ce niciun check de dinainte nu-l prindea: în scenariile existente ordinea
    // de INSERARE coincide cu cea cronologică, iar cele două ordini dau atunci
    // aceeași secvență. Blocul ăsta o rupe DELIBERAT — și o rupe DETERMINIST:
    // Id-urile se dau explicit, în ordine inversă față de dată (rândul cu data cea
    // mai TÂRZIE primește cel mai mic Id), adică exact urma pe care o lasă operarea
    // retroactivă / corecțiile / reimportările. Pe Id-uri UUIDv7 „naturale" scena
    // n-ar fi reproductibilă: în aceeași milisecundă ordinea lor e aleatoare.
    var cF3 = ContFsa("-3");
    Guid IdOrd(int n) => Guid.Parse($"fa510000-0000-7000-8000-{n:D12}");
    void NotaOrd(int idSecvential, DateOnly data, Cont debit, Cont credit, decimal valoare) =>
        Atlas.DXF.EfCore.ObjectSpace.Extensions.ObjectSpaceExtensions.CreateObject<RegistruContabil>(
            os, IdOrd(idSecvential), n => {
                n.Data = data;
                n.NumarNota = MarcajOrd;
                n.ContDebit = debit;
                n.ContCredit = credit;
                n.Valoare = valoare;
            });
    // Id crescător ⇔ dată DESCRESCĂTOARE. Ultimul rând (Id-ul cel mai mic) are
    // ACELAȘI cont pe ambele laturi: două rânduri de fișă cu Id identic, deci
    // tiebreak-ul `Sens DESC` e și el sub test.
    NotaOrd(1, new DateOnly(2026, 4, 14), cF3, cF3, 5m);
    NotaOrd(2, new DateOnly(2026, 4, 12), cF3, cF2, 32m);
    NotaOrd(3, new DateOnly(2026, 4, 10), cF2, cF3, 16m);
    NotaOrd(4, new DateOnly(2026, 4, 8), cF3, cF2, 8m);
    NotaOrd(5, new DateOnly(2026, 4, 6), cF2, cF3, 4m);
    NotaOrd(6, new DateOnly(2026, 4, 4), cF3, cF2, 2m);
    NotaOrd(7, new DateOnly(2026, 4, 2), cF3, cF2, 1m);
    os.CommitChanges();

    // Exact calea controllerului: opțiunile de grilă + ordinea declarată a
    // proiecției, prin `DataSourceLoader`. Dacă seam-ul dispare (sau ordinea nu se
    // mai declară), biblioteca revine la `Id` și blocul PICĂ.
    List<FisaContRand> FisaPrinLoader(int skip, int take) {
        var optiuni = new DataSourceLoadOptionsBase { Skip = skip, Take = take };
        OrdineLista.AplicaOrdineImplicita(optiuni, ContabilProiectii.OrdineFisa());
        return DataSourceLoader.Load(ContabilProiectii.FisaCont(os, cF3.ID, ds, de), optiuni)
            .data.Cast<FisaContRand>().ToList();
    }
    // Invariantul REAL al fișei: soldul fiecărui rând == cumulul rândurilor de
    // dinaintea lui ÎN ORDINEA AFIȘATĂ (nu doar „ultimul sold e corect" — ăla iese
    // bun și dintr-o secvență amestecată).
    bool CumulOk(IReadOnlyList<FisaContRand> randuri, decimal soldInainte) {
        var acumulat = soldInainte;
        foreach (var r in randuri) {
            acumulat += r.Debit - r.Credit;
            if (acumulat != r.SoldCurent)
                return false;
        }
        return true;
    }

    // Întâi PREMISA, pe calea directă (neatinsă de bibliotecă, deci adevărată și
    // înainte, și după fix): scenariul chiar deosebește cele două ordini. Fără
    // asertarea asta, checkurile de mai jos ar putea trece pe o scenă fără dinți.
    var soldAsteptat = new[] { 1m, 3m, -1m, 7m, -9m, 23m, 28m, 23m };
    var ordDirect = ContabilProiectii.FisaCont(os, cF3.ID, ds, de).ToList();
    Check("Premisa regresiei: pe cele 7 note ordinea de INSERARE (`Id`) e exact INVERSUL celei cronologice — deci `ORDER BY Id` și `ORDER BY Data, Id` chiar dau secvențe diferite (verificat pe calea DIRECTĂ, cea pe care biblioteca n-o atinge)",
        ordDirect.Count == 8
        && ordDirect.Select(r => r.Id).SequenceEqual(ordDirect.Select(r => r.Id).OrderByDescending(i => i))
        && ordDirect.Select(r => r.SoldCurent).SequenceEqual(soldAsteptat));

    var ordTot = FisaPrinLoader(0, 100);
    Check("REGRESIE (defectul feliei 9): prin `DataSourceLoader`, fișa iese CRONOLOGIC (`Data, Id, Sens DESC`), nu în ordinea de inserare — iar soldul curent al fiecărui rând e cumulul rândurilor de dinaintea lui ÎN ORDINEA AFIȘATĂ (1, 3, −1, 7, −9, 23, 28, 23)",
        ordTot.Count == 8
        && ordTot.Select(r => r.Data).SequenceEqual(ordTot.Select(r => r.Data).OrderBy(d => d))
        && ordTot.Select(r => r.SoldCurent).SequenceEqual(soldAsteptat)
        && CumulOk(ordTot, 0m));
    Check("REGRESIE, pe tiebreak: rândul cu același cont pe ambele laturi iese „D” înaintea lui „C” (`Sens DESC`), cu același `Id` — a treia cheie a ordinii nu se pierde nici ea",
        ordTot[6] is { Sens: "D", Debit: 5m } && ordTot[7] is { Sens: "C", Credit: 5m }
        && ordTot[6].Id == ordTot[7].Id && ordTot[6].Id == IdOrd(1));

    var pag1 = FisaPrinLoader(0, 3);
    var pag2 = FisaPrinLoader(3, 3);
    var pag3 = FisaPrinLoader(6, 3);
    Check("REGRESIE, PESTE PAGINARE (acolo se manifestă defectul): cele trei pagini concatenate reproduc EXACT secvența completă, iar cumulul continuă peste granița dintre pagini — `LIMIT/OFFSET` taie chiar secvența pe care s-a cumulat fereastra",
        pag1.Concat(pag2).Concat(pag3).Select(r => r.SoldCurent).SequenceEqual(soldAsteptat)
        && CumulOk(pag1, 0m) && CumulOk(pag2, pag1[^1].SoldCurent) && CumulOk(pag3, pag2[^1].SoldCurent));

    // Aceeași boală pe jurnal, unde contractul (R-D9) promite ordine CRONOLOGICĂ
    // implicită: fără declarația de ordine, `DataSourceLoader` o înlocuiește tot cu
    // `Id`-ul, adică tot cu ordinea de inserare. Filtrul îl duce tot biblioteca —
    // deci se verifică și compunerea filtru + sortare.
    var optiuniJurnal = new DataSourceLoadOptionsBase {
        Take = 100,
        Filter = new object[] { "NumarNota", "=", MarcajOrd }
    };
    OrdineLista.AplicaOrdineImplicita(optiuniJurnal, ContabilProiectii.OrdineJurnal());
    var jurnalOrd = DataSourceLoader.Load(ContabilProiectii.RegistruJurnal(os, ds, de), optiuniJurnal)
        .data.Cast<JurnalRand>().ToList();
    Check("REGRESIE pe jurnal: ordinea implicită promisă de R-D9 e CRONOLOGICĂ și supraviețuiește încărcării prin `DataSourceLoader` (cele 7 note ale scenariului, 02 → 14 aprilie), nu ordinea de inserare",
        jurnalOrd.Count == 7
        && jurnalOrd.Select(r => r.Data).SequenceEqual(jurnalOrd.Select(r => r.Data).OrderBy(d => d))
        && jurnalOrd[0].Data == new DateOnly(2026, 4, 2) && jurnalOrd[^1].Data == new DateOnly(2026, 4, 14));

    // -- VERIFICAREA 6: jurnalul ---------------------------------------------
    var jurnal = ContabilProiectii.RegistruJurnal(os, ds, de).ToList();
    var sumaDirecta = os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.Data >= ds && r.Data <= de).Sum(r => r.Valoare);
    Check($"VERIFICAREA 6: Σ Valoare din registrul-jurnal == suma directă din registru pe interval ({sumaDirecta}), pe TOATE rândurile bazei — și numărul de rânduri e identic (jurnalul nu unpivotează)",
        jurnal.Sum(r => r.Valoare) == sumaDirecta
        && jurnal.Count == os.GetObjectsQuery<RegistruContabil>().Count(r => r.Data >= ds && r.Data <= de));
    var jurnalFsa = jurnal.Where(r => r.NumarNota == MarcajFsa).ToList();
    Check("R-D9: jurnalul listează rândurile BRUTE, nu atomii — cele 4 note ale scenariului din perioadă apar o SINGURĂ dată fiecare (inclusiv cea cu același cont pe ambele laturi, care în fișă produce două rânduri), cronologic",
        jurnalFsa.Count == 4
        && jurnalFsa.Select(r => r.Data).SequenceEqual(jurnalFsa.Select(r => r.Data).OrderBy(d => d))
        && jurnalFsa.Single(r => r.ContDebitId == cF1.ID && r.ContCreditId == cF1.ID).Valoare == 40m
        && jurnalFsa.All(r => r.ContDebitSimbol != null && r.ContCreditSimbol != null));
    ContabilProiectii.CompleteazaTipDocument(os, jurnalFsa);
    Check("R-D8 pe jurnal: aceeași completare partajată — rândul legat poartă „BTR” și numărul documentului, rândurile de deschidere rămân goale",
        jurnalFsa.Single(r => r.DocumentId != null) is { DocumentTip: "BTR", DocumentNumar: MarcajFsa }
        && jurnalFsa.Where(r => r.DocumentId == null).All(r => r.DocumentTip == null && r.DocumentNumar == null));
    Check("Jurnalul opțional pe perioadă: fără `dataStart`/`dataEnd` întoarce TOT registrul (inclusiv rândul de după `dataEnd`), fiindcă aici datele sunt filtre simple, nu granițe de agregare",
        ContabilProiectii.RegistruJurnal(os).Count(r => r.NumarNota == MarcajFsa) == 6);

    // -- Ștergerea amânată: scrisă DE MÂNĂ în SQL brut ------------------------
    // Calea LINQ primește `WHERE "GCRecord" = 0` din filtrul global XAF; SQL-ul
    // brut NU primește nimic automat. Dacă predicatul ar lipsi din fișă, rândul
    // șters de mai jos ar rămâne vizibil ACOLO și ar dispărea din balanță — adică
    // exact divergența tăcută pe care felia există s-o prevină.
    os.Delete(os.GetObjectsQuery<RegistruContabil>()
        .Single(r => r.NumarNota == MarcajFsa && r.Storno));
    os.CommitChanges();
    var fisaDupaStergere = ContabilProiectii.FisaCont(os, cF1.ID, ds, de).ToList();
    var balantaDupaStergere = ContabilProiectii.Balanta(os, ds, de).ToList().Single(r => r.ContId == cF1.ID);
    Check("Ștergerea AMÂNATĂ (`GCRecord`) se respectă și pe calea SQL brut: rândul șters iese din fișă (5 → 4 rânduri), soldul curent devine 135, iar balanța rămâne cusută pe aceeași cifră — predicatul e scris de mână, nu moștenit",
        fisaDupaStergere.Count == 4
        && fisaDupaStergere[^1].SoldCurent == 135m
        && balantaDupaStergere.SoldFinalDebit - balantaDupaStergere.SoldFinalCredit == 135m
        && ContabilProiectii.RegistruJurnal(os, ds, de).Count(r => r.NumarNota == MarcajFsa) == 3);

    // -- Perioada de o singură zi + filtrul de dimensiune pe fișă -------------
    Check("Perioada de o SINGURĂ zi pe fișă: `dataStart == dataEnd` pe ziua unui rând — se afișează doar rândurile acelei zile, dar soldul lor curent poartă tot ce a fost înainte (175, apoi 135)",
        ContabilProiectii.FisaCont(os, cF1.ID, new DateOnly(2026, 4, 10), new DateOnly(2026, 4, 10)).ToList()
            is { Count: 2 } oZi
        && oZi[0].SoldCurent == 175m && oZi[1].SoldCurent == 135m);
    Check("Filtrul de dimensiune e parametru de PROIECȚIE și se aplică înaintea ferestrei: filtrată pe repartitorul pus DOAR pe rândul de dinainte de perioadă, fișa lui aprilie iese goală, iar cea care începe în martie arată acel rând cu soldul 100 — filtrarea de grilă, în schimb, ar fi lăsat soldurile registrului neatinse",
        ContabilProiectii.FisaCont(os, cF1.ID, ds, de, repartitorId: repF.ID).ToList().Count == 0
        && ContabilProiectii.FisaCont(os, cF1.ID, new DateOnly(2026, 3, 1), de, repartitorId: repF.ID).ToList()
            is { Count: 1 } peRep
        && peRep[0].SoldCurent == 100m);

    CurataFsa();
    Check("Curățenie finală felia fișă + jurnal (fără reziduuri e2e)",
        !os.GetObjectsQuery<Cont>().Any(c => c.Simbol.StartsWith(MarcajFsa))
        && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajFsa))
        && !os.GetObjectsQuery<RegistruContabil>().Any(r => r.NumarNota.StartsWith(MarcajFsa))
        && !os.GetObjectsQuery<NotaTransfer>().Any(d => d.Numar == MarcajFsa));
}
