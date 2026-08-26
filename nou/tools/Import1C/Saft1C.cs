using System.Diagnostics;
using System.Text;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;

namespace Import1C;

// `--saft <an> <lună>` — declarația D406 a unei luni REALE, generată pe baza de
// import și trecută prin validatorul oficial (felia 16, D16-D6, proba V5).
//
// DE CE în conector și nu doar în ModelCheck: suita rulează pe o SCENĂ (câteva
// zeci de documente construite ca să acopere ramurile). Fișierul de producție
// se măsoară pe anul importat din 1C — 13.966 de tranzacții, 3.950 de clienți —
// iar întrebările care contează la scara asta (trece DUK? cât durează? cât
// ocupă? câte `Neincluse`?) n-au răspuns pe scenă.
//
// DE CE oracolul e ACELAȘI `Duk.cs`: fișierul e legat în proiect din
// `tools/ModelCheck` (`<Compile Include="../ModelCheck/Duk.cs">`), nu copiat.
// O copie ar fi însemnat două locuri în care se corectează mecanica jar-ului
// (`-d` care agață procesul, prefixul `!` fără de care atenționările se pierd),
// iar cele două l-ar fi rulat diferit exact când contează. Nu s-a mutat în
// `Module`: pornirea unui proces java nu e treaba unui assembly de domeniu, pe
// care îl încarcă și host-ul web.
//
// Ușa e cea NON-SECURED a uneltei (44: conectoarele n-au nevoie de tierul API),
// deci proiecția vede tot — spre deosebire de REST, unde `User` primește sumar
// gol și 403 pe fișier (D16-D5).
static class Saft1C {

    public static int Executa(IObjectSpaceProvider provider, int an, int luna, string director) {
        var cronometru = Stopwatch.StartNew();
        Console.WriteLine($"\n=== SAF-T (D406) {luna:00}/{an} — proiecție, fișier, validator ===");

        SaftDto dto;
        using (var os = provider.CreateObjectSpace())
            dto = SaftProiectii.Saft(os, an, luna);
        var durataProiectie = cronometru.Elapsed;

        // Bugetarul n-are bază contabilă printre cele 12 ale ANAF ⇒ declarația
        // e NEAPLICABILĂ, nu goală (D16-D5). Se spune și se iese cu cod ≠ 0:
        // într-un lanț de comenzi, „n-am scris fișierul" nu are voie să treacă
        // drept succes.
        if (dto.Neaplicabil != null) {
            Console.Error.WriteLine($"SAF-T neaplicabil pe baza asta: {dto.Neaplicabil}");
            return 1;
        }

        var caleXml = Path.Combine(director, $"saft-{an}-{luna:00}.xml");
        cronometru.Restart();
        using (var fisier = File.Create(caleXml))
            SaftXml.Scrie(dto, fisier);
        var durataScriere = cronometru.Elapsed;
        var dimensiune = new FileInfo(caleXml).Length;

        cronometru.Restart();
        var duk = Duk.Valideaza(caleXml, an, luna);
        var durataDuk = cronometru.Elapsed;

        var caleRaport = Path.Combine(director, $"saft-{an}-{luna:00}-raport.txt");
        var raport = Compune(dto, duk, caleXml, dimensiune,
            durataProiectie, durataScriere, durataDuk);
        File.WriteAllText(caleRaport, raport, new UTF8Encoding(false));
        Console.Write(raport);
        Console.WriteLine($"\nFișierul: {caleXml} ({dimensiune / 1024.0 / 1024.0:N1} MiB)");
        Console.WriteLine($"Raportul: {caleRaport}");

        // Codul de ieșire: 0 dacă validatorul a spus `ok`. Un fișier RESPINS e
        // eșecul rulării, nu o observație — iar dacă oracolul lipsește
        // (`Disponibil = false`), rularea NU se declară verde pe o probă care
        // n-a avut loc (SĂRIT ≠ trecut, nota lui `Duk`).
        return duk.Disponibil && duk.Valid ? 0 : 1;
    }

    static string Compune(SaftDto dto, DukRezultat duk, string caleXml, long dimensiune,
            TimeSpan proiectie, TimeSpan scriere, TimeSpan validare) {
        var s = new StringBuilder();
        var rez = dto.Rezumat;
        s.AppendLine($"SAF-T (D406, modul L) {dto.Luna:00}/{dto.An} — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        s.AppendLine($"Perioada: {dto.DataStart:yyyy-MM-dd} … {dto.DataEnd:yyyy-MM-dd}");
        s.AppendLine($"Raportor: {dto.Header?.Name ?? "(fără denumire)"} · "
            + $"{dto.Header?.RegistrationNumber ?? "(fără CUI)"} · bază {dto.Header?.TaxAccountingBasis ?? "–"} · "
            + $"regiune {dto.Header?.AuditFileRegion ?? "–"}");
        s.AppendLine();

        s.AppendLine("--- Secțiuni (număr de intrări) ---");
        s.AppendLine($"  GeneralLedgerAccounts   {dto.Conturi.Count,10}");
        s.AppendLine($"  Customers               {dto.Clienti.Count,10}");
        s.AppendLine($"  Suppliers               {dto.Furnizori.Count,10}");
        s.AppendLine($"  TaxTable                {dto.Taxe.Count,10}");
        s.AppendLine($"  UOMTable                {dto.Unitati.Count,10}");
        s.AppendLine($"  AnalysisTypeTable       {dto.TipuriAnaliza.Count,10}");
        s.AppendLine($"  Products                {dto.Produse.Count,10}");
        s.AppendLine($"  Journals                {dto.Jurnale.Count,10}");
        s.AppendLine($"  Transactions            {rez.Tranzactii,10}   (linii GL {rez.LiniiGl})");
        s.AppendLine($"  SalesInvoices           {dto.FacturiEmise.Count,10}   "
            + $"(net {rez.NetTotalEmise:N2}, brut {rez.GrossTotalEmise:N2})");
        s.AppendLine($"  PurchaseInvoices        {dto.FacturiPrimite.Count,10}   "
            + $"(net {rez.NetTotalPrimite:N2}, brut {rez.GrossTotalPrimite:N2})");
        s.AppendLine($"  Payments                {dto.Plati.Count,10}   (Σ {rez.TotalPlati:N2}, din care "
            + $"{dto.Plati.Count(p => p.Storno)} de storno)");
        s.AppendLine($"  … facturi de storno     {dto.FacturiEmise.Concat(dto.FacturiPrimite).Count(f => f.Storno),10}");
        s.AppendLine();

        // Cusăturile D16-D4, cu STARE: fiecare e o egalitate la cent, iar
        // raportul spune verdictul, nu doar cifrele — un fișier care trece DUK
        // și pierde bani între registre e tot un fișier greșit (validatorul nu
        // verifică semantica fiscală, §5 al contractului).
        s.AppendLine("--- Cusăturile (D16-D4), la cent ---");
        Cusatura(s, "1 partidă dublă: Σ debit == Σ credit == Σ RegistruContabil",
            rez.TotalDebit == rez.TotalCredit && rez.TotalDebit == rez.ValoareRegistruContabil,
            $"D {rez.TotalDebit:N2} · C {rez.TotalCredit:N2} · registru {rez.ValoareRegistruContabil:N2}");
        Cusatura(s, "2 TVA: Σ TaxAmount GL + capitalizat + fără cod SAF-T == Σ RegistruTva.Tva",
            rez.TvaGl + rez.TvaCapitalizat + rez.TvaFaraCodSaft == rez.TvaRegistru,
            $"GL {rez.TvaGl:N2} + capitalizat {rez.TvaCapitalizat:N2} + fără cod {rez.TvaFaraCodSaft:N2} "
                + $"= {rez.TvaGl + rez.TvaCapitalizat + rez.TvaFaraCodSaft:N2} vs registru {rez.TvaRegistru:N2}");
        Cusatura(s, "3a facturi (achiziție): Σ bază linii + Σ bază neincluse == Σ RegistruTva.Baza (TOATE tipurile)",
            rez.BazaFacturiAchizitie + rez.BazaNeincluseAchizitie == rez.BazaRegistruAchizitie,
            $"{rez.BazaFacturiAchizitie:N2} + {rez.BazaNeincluseAchizitie:N2} "
                + $"= {rez.BazaFacturiAchizitie + rez.BazaNeincluseAchizitie:N2} vs {rez.BazaRegistruAchizitie:N2}");
        Cusatura(s, "3b facturi (livrare): idem",
            rez.BazaFacturiLivrare + rez.BazaNeincluseLivrare == rez.BazaRegistruLivrare,
            $"{rez.BazaFacturiLivrare:N2} + {rez.BazaNeincluseLivrare:N2} "
                + $"= {rez.BazaFacturiLivrare + rez.BazaNeincluseLivrare:N2} vs {rez.BazaRegistruLivrare:N2}");
        // Cusătura 4 e PER CONT (fixul F3 al review-ului): suma netă se anulează
        // pe erori de semn opus, deci ea singură nu probează nimic. Cifrele nete
        // rămân afișate — sunt utile la citire —, dar verdictul e al conturilor.
        Cusatura(s, "4 solduri PER CONT: closing (semnat) == balanța contului, pe fiecare cont din GLA",
            rez.ConturiDiferite == 0 && rez.ConturiVerificate > 0,
            $"{rez.ConturiVerificate} conturi verificate, {rez.ConturiDiferite} diferite; "
                + $"Σ|closing| {rez.SumaAbsolutaClosing:N2}; net GLA {rez.ClosingGla:N2} vs "
                + $"balanță {rez.ClosingBalanta:N2}");
        Cusatura(s, "6a terți (clienți): Σ Closing din Customers + Σ Neincluse[Customers] == Σ Closing GLA pe "
                + "conturile cu RolTert = Client",
            rez.ClosingClienti + rez.NeincluseClienti == rez.ClosingGlaClienti,
            $"{rez.ClosingClienti:N2} + {rez.NeincluseClienti:N2} "
                + $"= {rez.ClosingClienti + rez.NeincluseClienti:N2} vs GLA {rez.ClosingGlaClienti:N2}");
        Cusatura(s, "6b terți (furnizori): idem, pe conturile cu RolTert = Furnizor",
            rez.ClosingFurnizori + rez.NeincluseFurnizori == rez.ClosingGlaFurnizori,
            $"{rez.ClosingFurnizori:N2} + {rez.NeincluseFurnizori:N2} "
                + $"= {rez.ClosingFurnizori + rez.NeincluseFurnizori:N2} vs GLA {rez.ClosingGlaFurnizori:N2}");
        // Cusătura „master files" se recalculează AICI, pe listele fișierului:
        // e singura care nu încape într-o cifră de rezumat, dar e exact cea care
        // ar produce un fișier care referă un partener nedeclarat.
        var idClienti = dto.Clienti.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var idFurnizori = dto.Furnizori.Select(c => c.Id).ToHashSet(StringComparer.Ordinal);
        var facturiOrfane = dto.FacturiEmise.Count(f => !idClienti.Contains(f.PartenerID))
            + dto.FacturiPrimite.Count(f => !idFurnizori.Contains(f.PartenerID));
        var platiOrfane = dto.Plati.SelectMany(p => p.Linii)
            .Count(l => !idClienti.Contains(l.CustomerID) && !idFurnizori.Contains(l.SupplierID));
        Cusatura(s, "5 master files: fiecare CustomerID/SupplierID de pe facturi există în Customers/Suppliers",
            facturiOrfane == 0,
            $"{facturiOrfane} facturi cu partener nedeclarat; {platiOrfane} linii de plată fără niciun "
                + "identificator în cele două liste (societatea proprie e legitimă acolo)");
        s.AppendLine();

        s.AppendLine($"--- Neincluse: {dto.Neincluse.Count} intrări, GRUPATE per cauză ---");
        if (dto.Neincluse.Count == 0)
            s.AppendLine("  (niciuna — tot ce e în registre a intrat în fișier)");
        foreach (var g in dto.Neincluse.GroupBy(n => $"{n.Cauza} [{n.Sectiune}]")
                     .OrderByDescending(g => g.Count())) {
            var baza = g.Sum(n => n.Baza ?? 0m);
            var tva = g.Sum(n => n.Tva ?? 0m);
            var debit = g.Sum(n => n.Debit ?? 0m);
            var credit = g.Sum(n => n.Credit ?? 0m);
            s.AppendLine($"  {g.Count(),8} × {g.Key}  bază {baza:N2} · TVA {tva:N2} · D {debit:N2} · C {credit:N2}");
            foreach (var n in g.Take(3))
                s.AppendLine($"           ex. {n.DocumentTip} {n.DocumentNumar} {n.ContSimbol} "
                    + $"{n.RepartitorDenumire}");
        }
        s.AppendLine();

        s.AppendLine($"--- Avertismente: {dto.Avertismente.Count} coduri ---");
        if (dto.Avertismente.Count == 0)
            s.AppendLine("  (niciunul)");
        foreach (var a in dto.Avertismente.OrderByDescending(a => a.Numar)) {
            s.AppendLine($"  {a.Numar,8} × {a.Cod}{(a.Suma is decimal suma ? $"  Σ {suma:N2}" : "")}: {a.Mesaj}");
            foreach (var ex in a.Exemple)
                s.AppendLine($"           ex. {ex}");
        }
        s.AppendLine();

        s.AppendLine($"--- Validatorul oficial (DUKIntegrator): {duk.Rezumat} ---");
        s.AppendLine($"  comanda: {duk.Comanda ?? "(n-a rulat)"}");
        if (!duk.Disponibil)
            s.AppendLine($"  SĂRIT: {duk.Motiv} — proba de validitate NU s-a făcut.");
        // Erorile se GRUPEAZĂ pe tipul mesajului: un fișier de producție respins
        // dă zeci de mii de linii pe una-două cauze, iar lista brută n-are cap.
        // Cheia = mesajul fără cifrele care variază de la un rând la altul.
        foreach (var g in duk.Erori.GroupBy(TipMesaj).OrderByDescending(g => g.Count())) {
            s.AppendLine($"  {g.Count(),8} × {g.Key}");
            foreach (var e in g.Take(3))
                s.AppendLine($"           {e}");
        }
        foreach (var g in duk.Avertismente.GroupBy(TipMesaj).OrderByDescending(g => g.Count())) {
            s.AppendLine($"  {g.Count(),8} ! {g.Key}");
            foreach (var a in g.Take(3))
                s.AppendLine($"           {a}");
        }
        s.AppendLine();

        s.AppendLine("--- Timpi și dimensiune ---");
        s.AppendLine($"  proiecție {proiectie.TotalSeconds:N1} s · scriere XML {scriere.TotalSeconds:N1} s · "
            + $"validator {validare.TotalSeconds:N1} s");
        s.AppendLine($"  fișier {caleXml} — {dimensiune:N0} octeți ({dimensiune / 1024.0 / 1024.0:N1} MiB)");
        return s.ToString();
    }

    static void Cusatura(StringBuilder s, string nume, bool ok, string cifre) =>
        s.AppendLine($"  [{(ok ? "OK  " : "PICĂ")}] {nume}\n           {cifre}");

    // Tipul unui mesaj al validatorului = mesajul cu cifrele, ghilimelele și
    // codurile înlocuite prin `#`. Fără normalizarea asta, „linia 12345" și
    // „linia 12346" ar fi două cauze diferite, iar gruparea n-ar grupa nimic.
    static string TipMesaj(string mesaj) {
        var s = new StringBuilder(mesaj.Length);
        var cifra = false;
        foreach (var ch in mesaj) {
            if (char.IsAsciiDigit(ch)) {
                if (!cifra)
                    s.Append('#');
                cifra = true;
                continue;
            }
            cifra = false;
            s.Append(ch);
        }
        return s.ToString();
    }
}
