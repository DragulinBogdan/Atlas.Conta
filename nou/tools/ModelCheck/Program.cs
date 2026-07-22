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

    // --- Sursa de cont PartenerPredator: ContImplicit bate fallback-ul 401 ---
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

Console.WriteLine(esecuri == 0 ? "\nToate verificările au trecut." : $"\n{esecuri} verificări EȘUATE.");
Environment.ExitCode = esecuri == 0 ? 0 : 1;
