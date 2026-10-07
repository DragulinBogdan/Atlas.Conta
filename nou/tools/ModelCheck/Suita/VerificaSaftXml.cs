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

// ============ Felia 16, pas 3: fișierul XML + oracolul DUK — D16-V3 ============
// Ce probează: (a) `SaftXml.Scrie` produce un document care se citește înapoi cu
// structura cerută (rădăcină, namespace de PRODUCȚIE, cele 12 secțiuni ale
// modului L, `xs:choice`-urile respectate, ramura `Name` absentă); (b)
// VALIDATORUL OFICIAL îl acceptă — singura probă care contează pentru un fișier
// care pleacă la ANAF; (c) riscurile 5–9 ale contractului, MĂSURATE pe fișiere
// derivate din același DTO, cu verdictul brut al validatorului scris în rezumat.
//
// Riscurile 6 (`ExchangeRate` absent pe RON), 7 (segmentarea `1/1`), 8
// (`ProductCommodityCode = 0`) și 9 (diacriticele) sunt TOATE prezente în
// fișierul scenei: dacă el trece, ele sunt măsurate acolo, nu în derivate — un
// fișier „cu diacritice" ar fi fost o copie a celui de bază. Derivatele acoperă
// exact ce baza NU conține: analiticul de 7 cifre, baza contabilă `ONGE` și un
// identificator cu caracter special.
static class VerificaSaftXml {
    public static void Ruleaza(Suita s, SaftDto saft, int an, int luna, double msProiectie) {
        var director = Duk.DirectorTemporar();
        var prefix = $"saft-scena-{an}-{luna:00}";

        string Scrie(SaftDto dto, string nume) {
            var cale = Path.Combine(director, $"{nume}.xml");
            var cronometru = Stopwatch.StartNew();
            using (var fisier = File.Create(cale))
                SaftXml.Scrie(dto, fisier);
            if (nume == prefix)
                Console.WriteLine($"     MĂSURAT (D16-V3 timp): proiecția {msProiectie:N0} ms, scrierea "
                    + $"{cronometru.Elapsed.TotalMilliseconds:N0} ms, {new FileInfo(cale).Length:N0} octeți → {cale}");
            return cale;
        }

        // ═══ Fixul F7: fără CUI, fișierul nu pleacă deloc ═══
        // `Header.Company.RegistrationNumber` e identitatea DECLARANTULUI; fără el
        // fișierul e o declarație anonimă. Refuzul e AL SCRIITORULUI, ca și cel
        // pentru `Neaplicabil` — apelantul REST îl traduce în 422 (`SaftController`),
        // dar decizia nu e a ecranului. Se probează AICI fiindcă în ModelCheck nu
        // există HTTP; matricea de status a ușii rămâne măsurată în V4.
        var cuiInitial = saft.Header.RegistrationNumber;
        string mesajFaraCui = null;
        saft.Header.RegistrationNumber = null;
        try {
            using var flux = new MemoryStream();
            SaftXml.Scrie(saft, flux);
        }
        catch (InvalidOperationException e) {
            mesajFaraCui = e.Message;
        }
        finally {
            saft.Header.RegistrationNumber = cuiInitial;
        }
        Console.WriteLine($"     MĂSURAT (D16-V3/F7 fără CUI): `SaftXml.Scrie` → "
            + $"„{mesajFaraCui ?? "<A SCRIS FIȘIERUL>"}”.");
        s.Check("D16-V3 (fixul F7) `SaftXml.Scrie` REFUZĂ o declarație fără codul fiscal al societății raportoare, cu "
            + "mesaj de domeniu — un XML anonim servit cu status 200 ar fi cel mai prost răspuns posibil: pare "
            + "succes și nu se poate depune",
            mesajFaraCui != null && mesajFaraCui.Contains("codul fiscal al societății raportoare")
            && mesajFaraCui.Contains(nameof(SaftHeader.RegistrationNumber))
            && saft.Header.RegistrationNumber == cuiInitial);

        var caleXml = Scrie(saft, prefix);
        var doc = XDocument.Load(caleXml);
        XNamespace ns = SaftXml.SpatiuNume;
        List<XElement> Toate(string nume) => doc.Descendants(ns + nume).ToList();

        // ---------------- Structura, citită ÎNAPOI ----------------
        string[] sectiuni = [
            "Header", "GeneralLedgerAccounts", "Customers", "Suppliers", "TaxTable", "UOMTable",
            "AnalysisTypeTable", "Products", "GeneralLedgerEntries", "SalesInvoices", "PurchaseInvoices", "Payments",
        ];
        var lipsa = sectiuni.Where(s => !doc.Descendants(ns + s).Any()).ToList();
        var liniiGl = Toate("TransactionLine");
        var liniiCuAmbele = liniiGl.Count(l =>
            l.Element(ns + "DebitAmount") != null && l.Element(ns + "CreditAmount") != null);
        var numeInInfo = Toate("CustomerInfo").Concat(Toate("SupplierInfo"))
            .Count(i => i.Element(ns + "Name") != null);
        Console.WriteLine($"     MĂSURAT (D16-V3 structură): rădăcina „{doc.Root?.Name.LocalName}” în „{doc.Root?.Name.NamespaceName}”; "
            + $"secțiuni lipsă [{string.Join(", ", lipsa)}]; {liniiGl.Count} linii de GL, "
            + $"{Toate("Invoice").Count} facturi, {Toate("Payment").Count} plăți; "
            + $"NumberOfEntries GL = {Toate("GeneralLedgerEntries").First().Element(ns + "NumberOfEntries")?.Value}.");
        s.Check("D16-V3 fișierul se citește înapoi cu forma cerută: rădăcina `AuditFile` în namespace-ul de PRODUCȚIE "
            + "(`…d406…`, nu `…d406t…` — greșeala prototipului 1C), cele 12 secțiuni ale modului L prezente, "
            + "`NumberOfEntries` = numărul de tranzacții, `xs:choice`-ul debit/credit respectat pe FIECARE linie și "
            + "nicio ramură `Name` în `CustomerInfo`/`SupplierInfo` (validatorul o respinge explicit)",
            doc.Root?.Name == ns + "AuditFile"
            && doc.Root.Name.NamespaceName == "mfp:anaf:dgti:d406:declaratie:v1"
            && lipsa.Count == 0
            && Toate("GeneralLedgerEntries").First().Element(ns + "NumberOfEntries")?.Value
                == saft.Rezumat.Tranzactii.ToString()
            && liniiGl.Count == saft.Rezumat.LiniiGl && liniiCuAmbele == 0 && numeInInfo == 0
            && Toate("TotalSegmentsInsequence").Single().Value == "1"
            && Toate("SegmentIndex").Single().Value == "1"
            && Toate("Invoice").Count == saft.FacturiEmise.Count + saft.FacturiPrimite.Count);
        // Secțiunile modurilor S/A se emit GOALE (§A.5), nu se omit.
        // Felia 17 a făcut din `SaftXml` un scriitor pentru AMBELE declarații; proba
        // că L n-a mișcat e chiar aici: aceleași patru secțiuni GOALE, plus
        // `PhysicalStock` ABSENTĂ (singura secțiune S opțională — un tag gol acolo ar
        // fi invalid, fiindcă `PhysicalStockEntry` e obligatoriu înăuntru).
        s.Check("D16-V3 secțiunile pe care lunarul nu le raportează (`MovementTypeTable`, `Owners`, `Assets`, "
            + "`MovementOfGoods`) se emit ca tag deschis/închis — schema le cere prezente, nota metodologică le "
            + "cere goale; `PhysicalStock` (`minOccurs=0`) lipsește cu totul",
            new[] { "MovementTypeTable", "Owners", "Assets", "MovementOfGoods" }
                .All(s => Toate(s).Count == 1 && !Toate(s).Single().HasElements)
            && Toate("PhysicalStock").Count == 0);
        // Cealaltă jumătate a aceleiași regresii: pe LUNAR, cele patru secțiuni cu
        // totaluri le păstrează. Felia 17 le golește pe modulul `C` (profilul
        // validatorului le refuză acolo) — proba de aici ține gardul pe `L`, unde
        // ele sunt chiar conținutul declarației.
        s.Check("D16-V3 lunarul își păstrează totalurile de secțiune (`NumberOfEntries`/`TotalDebit`/`TotalCredit` "
            + "pe `GeneralLedgerEntries`, `SalesInvoices`, `PurchaseInvoices`, `Payments`) — golirea lor e a "
            + "modulului `C`, nu a scriitorului",
            new[] { "GeneralLedgerEntries", "SalesInvoices", "PurchaseInvoices", "Payments" }
                .All(s => Toate(s).Single().Elements(ns + "NumberOfEntries").Count() == 1
                    && Toate(s).Single().Elements(ns + "TotalDebit").Count() == 1
                    && Toate(s).Single().Elements(ns + "TotalCredit").Count() == 1));

        // ---------------- Oracolul: validatorul oficial ----------------
        var rezultat = Duk.Valideaza(caleXml);
        Console.WriteLine($"     MĂSURAT (D16-V3 DUK): {rezultat.Rezumat}");
        Console.WriteLine($"         COMANDA {rezultat.Comanda}");
        foreach (var e in rezultat.Erori.Take(30))
            Console.WriteLine($"         EROARE DUK: {e}");
        foreach (var a in rezultat.Avertismente.Take(30))
            Console.WriteLine($"         ATENȚIONARE DUK: {a}");
        s.Check("D16-V3 VALIDATORUL OFICIAL (DUKIntegrator, kit local) acceptă fișierul scenei — cu diacritice "
                + "(riscul 9), fără `ExchangeRate` pe RON (riscul 6), cu `ProductCommodityCode = 0` (riscul 8) și cu "
                + "segmentarea `1/1` (riscul 7): patru riscuri pin-uite, măsurate pe același fișier",
                rezultat.Valid);

        // ---------------- Riscurile care cer fișiere DERIVATE ----------------
        // Fiecare derivată schimbă UN lucru în DTO, se scrie și se validează; verdictul
        // se RAPORTEAZĂ (nu e o probă de trecut/picat: e o măsurătoare care intră în
        // §Închidere al contractului).
        void Masoara(string nume, string intrebare, Action modifica, Action refa) {
            modifica();
            try {
                var cale = Scrie(saft, $"{prefix}-{nume}");
                var r = Duk.Valideaza(cale);
                Console.WriteLine($"     MĂSURAT (D16-V3 {nume}): {intrebare} → {r.Rezumat}");
                foreach (var e in r.Erori.Take(5))
                    Console.WriteLine($"         EROARE DUK: {e}");
            }
            finally {
                refa();
            }
        }

        if (rezultat.Disponibil) {
            // Riscul 5: analiticul fără puncte poate depăși „6 cifre" (J2.2.5).
            var contProba = new SaftCont {
                AccountID = "3020200", AccountDescription = "Probă analitic de 7 cifre",
                AccountType = "Activ", OpeningDebitBalance = 0m, ClosingDebitBalance = 0m,
            };
            Masoara("cont-7-cifre", "`AccountID` = 3020200 (analitic de 7 cifre, sinteticul 302 există în plan)",
                () => saft.Conturi.Add(contProba), () => saft.Conturi.Remove(contProba));

            // Baza contabilă `ONGE` a fost adăugată în 05.06.2025; XSD-ul din 2023
            // n-o are. Dacă validatorul o refuză, kitul local e în urma schemei.
            var bazaInitiala = saft.Header.TaxAccountingBasis;
            Masoara("baza-ONGE", "`TaxAccountingBasis` = ONGE (adăugat în schema din 05.06.2025)",
                () => saft.Header.TaxAccountingBasis = "ONGE",
                () => saft.Header.TaxAccountingBasis = bazaInitiala);

            // Prefixul `04` cere un cod „fără caractere speciale" (regex-ul
            // `[^A-Za-z0-9]` din validator) — cratima ar trebui respinsă.
            var tert = saft.Clienti.FirstOrDefault(c => c.Id.StartsWith("04", StringComparison.Ordinal))
                ?? saft.Clienti.FirstOrDefault();
            if (tert != null) {
                var idInitial = tert.Id;
                var registruInitial = tert.RegistrationNumber;
                Masoara("id-cu-cratima", $"`CustomerID` = 04PROBA-1 în loc de {idInitial} (caracter special)",
                    () => { tert.Id = "04PROBA-1"; tert.RegistrationNumber = "04PROBA-1"; },
                    () => { tert.Id = idInitial; tert.RegistrationNumber = registruInitial; });
            }

            // ═══ Fixul F10: grafia Greciei — `EL` (VIES) vs `GR` (ISO 3166-1) ═══
            // Exemplele ANAF din §B.4 scriu `01EL123456789`, dar `Country` din
            // `AddressStructure` e ISO 3166-1, unde Grecia e `GR`. `SaftReguli` produce
            // azi prefixul din `Partener.Tara` ca atare, deci ambele grafii pot ieși —
            // și niciuna nu e „aleasă", ceea ce e chiar problema. Se măsoară pe FIȘIER,
            // cu oracolul, pe un FURNIZOR de probă adăugat în master files: nereferit
            // de nicio factură, deci verdictul e despre identificator, nu despre o
            // referință orfană. Trei variante, ca să se distingă „prefixul e greșit"
            // de „prefixul nu se potrivește cu țara adresei".
            foreach (var (nume, idGrec, taraAdresa) in new[] {
                         ("grec-EL-tara-GR", "01EL123456789", "GR"),
                         ("grec-GR-tara-GR", "01GR123456789", "GR"),
                         ("grec-EL-tara-EL", "01EL123456789", "EL"),
                     }) {
                var grec = new SaftTert {
                    Id = idGrec,
                    RegistrationNumber = idGrec,
                    Name = "Proba Grecia EPE",
                    Address = new SaftAdresa {
                        StreetName = "Ermou", Number = "1", City = "Athina",
                        PostalCode = "10563", Country = taraAdresa,
                    },
                    AccountID = "401",
                    OpeningCreditBalance = 0m,
                    ClosingCreditBalance = 0m,
                    FelId = nameof(FelIdSaft.UeInregistrat),
                };
                Masoara(nume, $"`SupplierID` = {idGrec} cu `Country` = {taraAdresa} (grafia "
                        + $"{(idGrec.Contains("EL") ? "VIES" : "ISO 3166-1")} a Greciei)",
                    () => saft.Furnizori.Add(grec), () => saft.Furnizori.Remove(grec));
            }
        }
    }
}
