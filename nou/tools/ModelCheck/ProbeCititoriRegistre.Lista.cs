namespace Atlas.Conta.BackOffice.ModelCheck;

// Lista nominală X-D2: maparea, cazurile gardianului, evidența XAF, autorizarea și legătura.
static partial class ProbeCititoriRegistre {
    const string M = SursaProductie.Modul;
    const string W = SursaProductie.WebApi;
    const string Cele4 = "RegistruContabil|RegistruStoc|RegistruTva|RegistruImobilizari";
    const string Motor3 = "RegistruContabil|RegistruStoc|RegistruTva";

    // Rezultate netipizate derivate dintr-o tabelă din listă: apelanții lor o citesc.
    static readonly Sursa[] Purtatori = [
        new("Partide", "NominalizataLibera", true, ["Imperechere"]),
    ];

    static readonly Permisa[] Permise = [
        // ── maparea EF ──
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.OnModelCreating", Cele4 + "|Imperechere", Clasa.Mapare, "maparea EF"),
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.RegistruContabil", "RegistruContabil", Clasa.Mapare, "DbSet"),
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.RegistruStoc", "RegistruStoc", Clasa.Mapare, "DbSet"),
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.RegistruTva", "RegistruTva", Clasa.Mapare, "DbSet"),
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.RegistruImobilizari", "RegistruImobilizari", Clasa.Mapare, "DbSet"),
        new(M + "BusinessObjects/BackOfficeDbContext.cs", "BackOfficeEFCoreDbContext.Imperecheri", "Imperechere", Clasa.Mapare, "DbSet"),

        // ── cazurile gardianului: refuză scrierea registrelor pe uși securizate (14); cad cu entitățile ──
        new(M + "Motor/GardianEditare.cs", "GardianEditare.Verifica", Cele4, Clasa.Gardian, "refuză scrierea registrelor pe uși securizate (14)"),

        // ── suprafețele de evidență XAF (rămân până la pasul 7) ──
        new(M + "UI/ContaUiBaseline.cs", "ContaUiBaseline.AscundeFkuriBrute", Motor3 + "|Imperechere", Clasa.Evidenta, "listele XAF ale registrelor"),
        new(M + "UI/ContaUiBaseline.cs", "ContaUiBaseline.Imobilizari", "RegistruImobilizari", Clasa.Evidenta, "lista XAF a registrului de imobilizări"),

        // ── autorizarea verificării închiderii (80e) ──
        new(W + "API/Conta/PerioadeController.cs", "PerioadeController.TipuriInsumate", "Imperechere", Clasa.Autorizare, "tipurile însumate de verificarea închiderii (80e)"),

        // ── `Imperechere` e legătura explicită, nu registru: rămâne și după TR-D9 ──
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Imperecheaza", "Imperechere", Clasa.Legatura, "comanda de împerechere"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Desfa", "Imperechere", Clasa.Legatura, "comanda de desfacere"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.CreeazaInvers", "Imperechere", Clasa.Legatura, "legătura inversă"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.InverseazaLaStorno", "Imperechere", Clasa.Legatura, "desfacerea legăturilor la storno"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Motive", "Imperechere", Clasa.Legatura, "regimul citește legăturile fără blocaj (31d, 088j, 106k)"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Toate", "Imperechere", Clasa.Legatura, "legăturile documentului, pe ambele roluri"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.MotivVii", "Imperechere", Clasa.Legatura, "stornoul refuză legătura vie din perioada deschisă"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Vii", "Imperechere", Clasa.Legatura, "legăturile neinversate ale documentului"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Sterge", "Imperechere", Clasa.Legatura, "ștergerea în perioada deschisă"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.DesfaceEfect", "Imperechere", Clasa.Legatura, "compensarea transferului legăturii"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Creeaza", "Imperechere", Clasa.Legatura, "scrie legătura"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.ValideazaCreare", "Imperechere", Clasa.Legatura, "capacitatea perechii include nominalizarea liberă (101)"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.Verifica", "Imperechere", Clasa.Legatura, "gardianul legăturii"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.VerificaImperechere", "Imperechere", Clasa.Legatura, "regulile legăturii la Committing"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.AreRandInvers", "Imperechere", Clasa.Legatura, "legătura deja desfăcută"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.EsteFixupDeStergere", "Imperechere", Clasa.Legatura, "ștergerea perechii original–invers"),
        new(M + "Motor/MotorOperare.cs", "MotorOperare.MotivImperecheri", "Imperechere", Clasa.Legatura, "anularea cere document fără legături (31d)"),
        new(M + "Cub/Citiri/Partide.cs", "Partide.NominalizataLibera", "Imperechere", Clasa.Legatura, "nominalizarea neacoperită de legături (101)"),
        new(M + "Cub/Materializare.cs", "Materializare.Imperecheaza", "Imperechere", Clasa.Legatura, "transferul scade nominalizarea liberă (101)"),
        new(M + "Api/Trz/ImperechereApply.cs", "ImperechereApply.Din", "Imperechere", Clasa.Legatura, "DTO-ul legăturii"),
        new(M + "Api/Trz/ImperechereApply.cs", "ImperechereApply.Sterge", "Imperechere", Clasa.Legatura, "adaptorul comenzii de ștergere"),
        new(M + "Api/Trz/ImperechereApply.cs", "ImperechereApply.Stingeri", "Imperechere", Clasa.Legatura, "rândurile panoului; restul vine din `Partide`"),
        new(M + "Controllers/ImperechereController.cs", "ImperechereController..ctor", "Imperechere", Clasa.Legatura, "acțiunile XAF ale legăturii"),
        new(M + "Controllers/ImperechereController.cs", "ImperechereController.AplicaCapabilitati", "Imperechere", Clasa.Legatura, "afordanțele XAF"),
        new(M + "Controllers/ImperechereController.cs", "ImperechereController.Imperecheaza_Execute", "Imperechere", Clasa.Legatura, "adaptorul XAF al împerecherii"),
        new(M + "Controllers/ImperechereController.cs", "ImperechereController.Desfa_Execute", "Imperechere", Clasa.Legatura, "adaptorul XAF al desfacerii"),
        new(M + "Controllers/ImperechereController.cs", "ImperechereController.Sterge_Execute", "Imperechere", Clasa.Legatura, "adaptorul XAF al ștergerii"),
        new(W + "API/Conta/ImperecheriController.cs", "ImperecheriController.Post", "Imperechere", Clasa.Legatura, "ușa HTTP a împerecherii"),
        new(W + "API/Conta/ImperecheriController.cs", "ImperecheriController.Delete", "Imperechere", Clasa.Legatura, "ușa HTTP a ștergerii"),
        new(W + "API/Conta/ImperecheriController.cs", "ImperecheriController.Desfa", "Imperechere", Clasa.Legatura, "ușa HTTP a desfacerii"),
    ];
}
