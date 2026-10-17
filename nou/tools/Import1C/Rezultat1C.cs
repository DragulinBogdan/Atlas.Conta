using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// 134 a
static class Rezultat1C {
    public const string View = "InchidereRezultat";
    public const string Categorie = "închiderea rezultatului: IRZ Atlas pe cheile lui față de rândurile 1C (134 a)";

    static List<(string Sursa, string Destinatie)> reguli;

    public static int RanduriSarite { get; private set; }
    public static int Inchideri { get; private set; }
    public static decimal Divergenta { get; private set; }

    static List<(string Sursa, string Destinatie)> Reguli(BuclaImport bucla) {
        if (reguli == null) {
            using var os = bucla.CreeazaObjectSpace();
            reguli = os.GetObjectsQuery<PoliticaInchidereConturi>()
                .Where(p => p.Moment == MomentInchidere.Lunar)
                .Select(p => new { Sursa = p.ContSursa.Simbol, Destinatie = p.ContDestinatie.Simbol })
                .ToList().Select(p => (p.Sursa, p.Destinatie)).ToList();
        }
        return reguli;
    }

    static bool EsteRandDeRezultat(BuclaImport bucla, FlaxRandNota r) {
        var debit = bucla.Catalog.Mapeaza(r.ContDebit);
        var credit = bucla.Catalog.Mapeaza(r.ContCredit);
        return debit != null && credit != null && Reguli(bucla).Any(p =>
            (debit == p.Destinatie && credit.StartsWith(p.Sursa, StringComparison.Ordinal))
            || (credit == p.Destinatie && debit.StartsWith(p.Sursa, StringComparison.Ordinal)));
    }

    public static IReadOnlyList<FlaxRandNota> FaraRezultat(BuclaImport bucla, IReadOnlyList<FlaxRandNota> randuri) {
        var ramase = randuri.Where(r => !EsteRandDeRezultat(bucla, r)).ToList();
        RanduriSarite += randuri.Count - ramase.Count;
        return ramase;
    }

    public static InchidereRezultat Genereaza(IObjectSpace os, BuclaImport bucla, ContextLuna ctx) {
        var irz = InchidereRezultatService.Genereaza(os, ctx.An, ctx.Luna, bucla.Catalog.SediuId);
        var atlas = irz == null ? []
            : irz.Detalii.OfType<NotaContabilaDetaliu>()
                .Select(l => (Debit: bucla.Catalog.Simbol(l.ContDebitId), Credit: bucla.Catalog.Simbol(l.ContCreditId), l.Valoare));
        var sursa = bucla.Flax.InchideriLuna(ctx.An, ctx.Luna)
            .SelectMany(h => bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [])
            .Where(r => EsteRandDeRezultat(bucla, r))
            .Select(r => (bucla.Catalog.Mapeaza(r.ContDebit), bucla.Catalog.Mapeaza(r.ContCredit), r.Suma));
        Divergenta += Imobilizari1C.Declara(bucla, RegistruDivergente.Sursa(View, $"{ctx.An:0000}-{ctx.Luna:00}"),
            Categorie, sursa, atlas);
        if (irz != null)
            Inchideri++;
        return irz;
    }

    public static void Raporteaza() {
        if (Inchideri + RanduriSarite == 0)
            return;
        Console.WriteLine($"  Rezultat: {Inchideri} închideri IRZ; {RanduriSarite} rânduri 6x/7x = 121 ale închiderii 1C "
            + $"lăsate IRZ-ului; divergențe |Σ| {Divergenta:N2}.");
    }
}
