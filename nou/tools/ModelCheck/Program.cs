using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

// Validare model EF + (dacă baza există) verificare migrații/seed + scenariile
// end-to-end ale motorului de operare (felia 3b: NotaTransfer; felia 3c:
// FacturaIntrare→NIR conex) pe un IObjectSpace real — aceeași infrastructură
// XAF pe care o folosește și UI-ul (docs 113709).
// Aceeași țintă ca appsettings.json: Postgres localhost:5444.
const string ConnectionString =
    "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.BackOffice";

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

var opts = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql(ConnectionString)
    .UseChangeTrackingProxies()
    .Options;

using (var ctx = new BackOfficeEFCoreDbContext(opts)) {
    Console.WriteLine($"Model OK: {ctx.Model.GetEntityTypes().Count()} entity types");

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

    Console.WriteLine($"TipuriDocument:  {await ctx.TipuriDocument.CountAsync()}");
    Console.WriteLine($"ClaseProduse:    {await ctx.ClaseProduse.CountAsync()}");
    Console.WriteLine($"TipuriMaterial:  {await ctx.TipuriMaterial.CountAsync()}");
    Console.WriteLine($"Conturi:         {await ctx.Conturi.CountAsync()} (din care cu defalcare: {await ctx.Conturi.CountAsync(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna)})");
    Console.WriteLine($"Repartitori:     {await ctx.Repartitori.CountAsync()}");
    Console.WriteLine($"PerioadeFiscale: {await ctx.PerioadeFiscale.CountAsync()}");
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

// ============================ Scenariul e2e 3b ============================
// NotaTransfer end-to-end: sold de deschidere → operare (2 rânduri ±) →
// gardieni (sold intermediar, retroactiv, perioadă, dependență) → FIFO →
// anulare (corecție directă) → storno. Obiectele de test poartă marcajul E2E
// și se curăță la început (run eșuat anterior) și la sfârșit.
const string MarcajProdus = "E2E-PRB";

using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(ConnectionString)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies());

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
    os.CommitChanges();
}

using (var os = provider.CreateObjectSpace()) {
    CurataFct(os);

    var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
    var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
    var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
    var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
    var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404.01.00");

    // Maparea Clasă/Tip → cont derivată de seed din simboluri (decizia 4).
    Check("Seed: Tip 302.01.00 → cont 302.01.00 (potrivire exactă)",
        tipMateriale.ContImplicitId != null
        && os.GetObjectByKey<Cont>(tipMateriale.ContImplicitId.Value).Simbol == "302.01.00");
    Check("Seed: Tip 628.00.00 → cont 628.* (tăierea segmentelor)",
        tipServicii.ContImplicitId != null
        && os.GetObjectByKey<Cont>(tipServicii.ContImplicitId.Value).Simbol.StartsWith("628"));

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
    linieStoc.CotaTva = 19m;
    linieStoc.LotFabricatie = "LOT-A";
    linieStoc.DataExpirare = new DateOnly(2027, 1, 1);
    var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
    linieServiciu.Document = fct;
    linieServiciu.TipMaterial = tipServicii;
    linieServiciu.Cantitate = 1m;
    linieServiciu.PretUnitar = 100m;
    linieServiciu.CotaTva = 0m;

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
    linie2.Dimensiuni.CodEconomicId = codEc.ID;
    os.CommitChanges();
    Check("FCT doar cu servicii NU generează NIR", MotorOperare.Opereaza(os, fct2) == null);
    Check("Creditul vine din ContImplicit al partenerului (404, nu fallback 401)",
        os.GetObjectsQuery<RegistruContabil>().Single(r => r.DocumentId == fct2.ID).ContCreditId == cont404.ID);
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

    var client = os.CreateObject<Partener>();
    client.Cod = MarcajFcl + "-CL1";
    client.Denumire = "Client probă e2e";
    os.CommitChanges();

    FacturaIesireDetaliu Linie(FacturaIesire doc, TipMaterial tip, decimal cantitate, decimal pret, decimal cota) {
        var d = os.CreateObject<FacturaIesireDetaliu>();
        d.Document = doc;
        d.TipMaterial = tip;
        d.Cantitate = cantitate;
        d.PretUnitar = pret;
        d.CotaTva = cota;
        return d;
    }
    List<RegistruContabil> Note(Document doc) =>
        os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();

    // --- Validările laturilor + refuzul liniilor de stoc ---
    var fcl = os.CreateObject<FacturaIesire>();
    fcl.Data = new DateOnly(2026, 3, 5);
    fcl.Predator = client; // inversat intenționat
    fcl.Primitor = sediu;
    Linie(fcl, tipServiciiVenit, 2m, 100m, 0m).Descriere = "Servicii refacturate";
    CheckRefuza("Laturi inversate (predator partener / primitor intern) → refuz",
        () => MotorOperare.Opereaza(os, fcl));
    fcl.Predator = sediu;
    fcl.Primitor = client;

    var linieStoc = Linie(fcl, tipStocMat, 1m, 5m, 0m);
    CheckRefuza("Linie de stoc pe factura de ieșire → refuz (nu descarcă gestiune)",
        () => MotorOperare.Opereaza(os, fcl));
    os.Delete(linieStoc);
    Linie(fcl, tipChirii, 1m, 50m, 0m).Descriere = "Chirie spațiu";
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
    Linie(fcl2, tipServiciiVenit, 1m, 100m, 19m);
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
    linieStoc.CotaTva = 19m;
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
    linieProtocol.CotaTva = 19m;
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

Console.WriteLine(esecuri == 0 ? "\nToate verificările au trecut." : $"\n{esecuri} verificări EȘUATE.");
Environment.ExitCode = esecuri == 0 ? 0 : 1;
