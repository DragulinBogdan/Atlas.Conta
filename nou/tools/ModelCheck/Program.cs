using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;

// Validare model EF + (dacă baza există) verificare migrații/seed.
// Aceeași țintă ca appsettings.json: Postgres localhost:5444.
var opts = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
    .UseNpgsql("Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.BackOffice")
    .UseChangeTrackingProxies()
    .Options;

using var ctx = new BackOfficeEFCoreDbContext(opts);
Console.WriteLine($"Model OK: {ctx.Model.GetEntityTypes().Count()} entity types");

if (!await ctx.Database.CanConnectAsync()) {
    Console.WriteLine("Baza nu există încă — doar validare de model.");
    return;
}

var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
var applied = (await ctx.Database.GetAppliedMigrationsAsync()).ToList();
Console.WriteLine($"Migrații aplicate: {applied.Count}; în așteptare: {pending.Count}"
    + (pending.Count > 0 ? $" ({string.Join(", ", pending)})" : ""));

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
var regula = await ctx.ReguliContare.FirstOrDefaultAsync();
if (regula != null)
    Console.WriteLine($"Owned all-null → DimensiuniComun {(regula.DimensiuniComun == null ? "NULL (BUG!)" : "instanță OK")}");
