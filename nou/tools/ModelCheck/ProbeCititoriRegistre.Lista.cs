namespace Atlas.Conta.BackOffice.ModelCheck;

// Lista nominală X-D2. Clasele 1–3 sunt ale contractului (scriitor dual, martor, evidență XAF);
// `Mapare`, `Autorizare` și `Legatura` le-a cerut prima rulare.
static partial class ProbeCititoriRegistre {
    const string M = SursaProductie.Modul;
    const string W = SursaProductie.WebApi;
    const string Cele4 = "RegistruContabil|RegistruStoc|RegistruTva|RegistruImobilizari";
    const string Motor3 = "RegistruContabil|RegistruStoc|RegistruTva";
    const string AbsorbtieAsm = "ASM-B6";

    // Rezultate netipizate derivate din registru: apelanții lor citesc registrul.
    static readonly Sursa[] Purtatori = [
        new("StocService", "SolduriLaData", true, ["RegistruStoc"]),
        new("StocService", "Sold", true, ["RegistruStoc"]),
        new("StocService", "AlocaFifoTolerant", true, ["RegistruStoc"]),
        new("StocService", "AlocaFifo", true, ["RegistruStoc"]),
        new("Fapte", "SolduriLoturiRegistru", true, ["RegistruStoc"]),
        new("Operand", "SolduriLoturiRegistru", false, ["RegistruStoc"]),
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

        // ── 1. scriitorii regimului dual (cad la TR-D9) ──
        new(M + "Motor/MotorOperare.cs", "MotorOperare.Opereaza", Motor3, Clasa.ScriitorDual, "scrie rândurile operării"),
        new(M + "Motor/MotorOperare.cs", "MotorOperare.AnuleazaOperarea", Motor3, Clasa.ScriitorDual, "șterge rândurile documentului anulat și reverifică soldul"),
        new(M + "Motor/MotorOperare.cs", "MotorOperare.Storneaza", Motor3, Clasa.ScriitorDual, "scrie inversele rândurilor documentului"),
        new(M + "Motor/CorectieService.cs", "CorectieService.Corecteaza", "RegistruTva", Clasa.ScriitorDual, "reatribuie perioada inversei fiscale"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.Verifica", Cele4, Clasa.ScriitorDual, "refuză scrierea registrelor pe uși securizate (14)"),
        new(M + "Motor/StocService.cs", "StocService.MiscariRegistru", "RegistruStoc", Clasa.ScriitorDual, "rădăcina citirilor planului registrelor"),
        new(M + "Motor/StocService.cs", "StocService.SolduriLaData", "RegistruStoc", Clasa.ScriitorDual, "soldul pe cheie pentru valoarea ieșirii din registru"),
        new(M + "Motor/StocService.cs", "StocService.AplicaValoareIesire", "RegistruStoc", Clasa.ScriitorDual, "regula golirii pe rândul de registru (D18-D2)"),
        new(M + "Motor/StocService.cs", "StocService.VerificaSoldIntermediar", "RegistruStoc", Clasa.ScriitorDual, "garda de sold a planului registrelor"),
        new(M + "Motor/StocService.cs", "StocService.Sold", "RegistruStoc", Clasa.ScriitorDual, "fără apelant de producție; oracol al probelor"),
        new(M + "Motor/StocService.cs", "StocService.AlocaFifoTolerant", "RegistruStoc", Clasa.ScriitorDual, "fără apelant de producție; oracol al probelor"),
        new(M + "Motor/StocService.cs", "StocService.AlocaFifo", "RegistruStoc", Clasa.ScriitorDual, "fără apelant de producție; oracol al probelor"),
        new(M + "Motor/Fapte.cs", "Fapte.SolduriLoturiRegistru", "RegistruStoc", Clasa.ScriitorDual, AbsorbtieAsm + ": soldul registrului pentru R"),
        new(M + "Motor/Fapte.cs", "Fapte.Operand", "RegistruStoc", Clasa.ScriitorDual, AbsorbtieAsm + ": operandul poartă soldul registrului numai la cererea declarantului"),
        new(M + "Declaratii/DeclarantAsamblare.cs", "DeclarantAsamblare.Declara", "RegistruStoc", Clasa.ScriitorDual, AbsorbtieAsm + ": absorbția Δ față de registru"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "PunereInFunctiune.MaterializeazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie fișa PIF"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "PunereInFunctiune.EliminaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "șterge rândurile la anulare"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "PunereInFunctiune.StorneazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie inversele"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "PunereInFunctiune.RanduriProprii", "RegistruImobilizari", Clasa.ScriitorDual, "rândurile proprii ale documentului"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "PunereInFunctiune.Inverseaza", "RegistruImobilizari", Clasa.ScriitorDual, "inversa unui rând"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "IesireImobilizare.MaterializeazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie ieșirea CAS"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "IesireImobilizare.EliminaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "șterge rândurile la anulare"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "IesireImobilizare.StorneazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie inversele"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "AmortizareLunara.MaterializeazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie amortizarea lunii"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "AmortizareLunara.EliminaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "șterge rândurile la anulare"),
        new(M + "BusinessObjects/Documente/Imobilizari.cs", "AmortizareLunara.StorneazaRegistrul", "RegistruImobilizari", Clasa.ScriitorDual, "scrie inversele"),

        // ── 2. martori și diagnostice ──
        new(M + "Cub/Citiri/Invarianti.cs", "Invarianti.Verifica", "RegistruContabil", Clasa.Martor, "INV-CUB: acoperirea contabilă a cubului"),
        new(M + "Cub/Citiri/Imobilizari.cs", "Imobilizari.VerificaAcoperire", "RegistruImobilizari", Clasa.Martor, "acoperirea fișelor (097-r1)"),
        new(M + "Cub/Citiri/Loturi.cs", "Loturi.VerificaAcoperire", "RegistruStoc", Clasa.Martor, "INV-CUB: acoperirea cantitativă a stocului pe grup"),
        new(M + "Cub/Materializare.Deschidere.cs", "Materializare.Deschide", "RegistruStoc", Clasa.Martor, "refuză lotul de deschidere care are deja mișcări"),
        new(M + "Motor/LoturiCulegereService.cs", "LoturiLiniiSterse.Curata", "RegistruStoc", Clasa.Martor, "urma lotului înaintea ștergerii lui"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.VerificaTipTva", "RegistruTva", Clasa.Martor, "referința care oprește ștergerea tipului de TVA"),
        new(M + "BusinessObjects/Nomenclatoare/Imobilizari.cs", "Imobilizare.Verifica", "RegistruImobilizari", Clasa.Martor, "referința care oprește ștergerea fișei"),

        // ── 3. suprafețele de evidență XAF (rămân până la TR-D9) ──
        new(M + "UI/ContaUiBaseline.cs", "ContaUiBaseline.AscundeFkuriBrute", Motor3 + "|Imperechere", Clasa.Evidenta, "listele XAF ale registrelor"),
        new(M + "UI/ContaUiBaseline.cs", "ContaUiBaseline.Imobilizari", "RegistruImobilizari", Clasa.Evidenta, "lista XAF a registrului de imobilizări"),

        // ── cheia de autorizare a cifrelor (F22-D5) ──
        new(W + "API/Conta/ItvController.cs", "ItvController.RegistrulCitibil", "RegistruContabil", Clasa.Autorizare, "dreptul de citire cerut de cifrele închiderii de TVA"),
        new(W + "API/Conta/AmoController.cs", "AmoController.RegistrulCitibil", "RegistruImobilizari", Clasa.Autorizare, "dreptul de citire cerut de cifrele amortizării"),
        new(W + "API/Conta/ImobilizariController.cs", "ImobilizariController.RegistrulCitibil", "RegistruImobilizari", Clasa.Autorizare, "dreptul de citire cerut de fișă și registru"),
        new(W + "API/Conta/PerioadeController.cs", "PerioadeController.TipuriInsumate", "Imperechere", Clasa.Autorizare, "tipurile însumate de verificarea închiderii (80e)"),

        // ── `Imperechere` e legătura explicită, nu registru: rămâne și după TR-D9 ──
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Imperecheaza", "Imperechere", Clasa.Legatura, "comanda de împerechere"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Desfa", "Imperechere", Clasa.Legatura, "comanda de desfacere"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.CreeazaInvers", "Imperechere", Clasa.Legatura, "legătura inversă"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.InverseazaLaStorno", "Imperechere", Clasa.Legatura, "desfacerea legăturilor la storno"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Sterge", "Imperechere", Clasa.Legatura, "ștergerea în perioada deschisă"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.DesfaceEfect", "Imperechere", Clasa.Legatura, "compensarea transferului legăturii"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.Creeaza", "Imperechere", Clasa.Legatura, "scrie legătura"),
        new(M + "Motor/ImperechereService.cs", "ImperechereService.ValideazaCreare", "Imperechere", Clasa.Legatura, "capacitatea perechii include nominalizarea liberă (101)"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.Verifica", "Imperechere", Clasa.Legatura, "gardianul legăturii"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.VerificaImperechere", "Imperechere", Clasa.Legatura, "regulile legăturii la Committing"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.AreRandInvers", "Imperechere", Clasa.Legatura, "legătura deja desfăcută"),
        new(M + "Motor/GardianEditare.cs", "GardianEditare.EsteFixupDeStergere", "Imperechere", Clasa.Legatura, "ștergerea perechii original–invers"),
        new(M + "Motor/MotorOperare.cs", "MotorOperare.VerificaFaraImperecheri", "Imperechere", Clasa.Legatura, "anularea și stornoul cer document fără legături (31d)"),
        new(M + "Cub/Citiri/Partide.cs", "Partide.NominalizataLibera", "Imperechere", Clasa.Legatura, "nominalizarea neacoperită de legături (101)"),
        new(M + "Cub/Materializare.cs", "Materializare.Imperecheaza", "Imperechere", Clasa.Legatura, "transferul scade nominalizarea liberă (101)"),
        new(M + "Api/ApiProiectii.cs", "ApiProiectii.AreImperecheri", "Imperechere", Clasa.Legatura, "afordanța de anulare: documentul poartă legături"),
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
