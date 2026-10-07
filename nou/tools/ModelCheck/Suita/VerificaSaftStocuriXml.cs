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

// ============ Felia 17, pas 3: fișierul S + oracolul DUK — D17-V3 ============
// Ce probează: (a) `SaftXml.Scrie` produce, din ACELAȘI DTO și cu ACELAȘI
// scriitor ca lunarul, un fișier de STOCURI care se citește înapoi cu structura
// XSD-ului (ordinea copiilor pe cele trei structuri noi, secțiunile L goale,
// `PhysicalStock` prezent); (b) VALIDATORUL OFICIAL îl acceptă; (c) riscurile
// 1–5 ale contractului, fiecare MĂSURAT cu DUK pe un fișier derivat care
// schimbă UN singur lucru, cu verdictul brut scris în rezumat.
//
// Riscurile 3 (`StockCharacteristic = "0"`), 4 (`StockAccountNo` = Guid de 36) și
// 5 (`ProductType` = simbol de cont) sunt TOATE în fișierul de bază: dacă el
// trece, ele sunt măsurate acolo. Derivatele există ca să separe „a trecut" de
// „nu e verificat": un `ProductType` care NU e cont din plan, o cheie de
// caracteristică din nomenclatorul accizelor, o referință peste limita de 35.
static class VerificaSaftStocuriXml {
    public static void Ruleaza(Suita s, SaftDto saft, int an, int luna, double msProiectie) {
        var director = Duk.DirectorTemporar();
        var prefix = $"saft-s-scena-{an}-{luna:00}";

        string Scrie(SaftDto dto, string nume) {
            var cale = Path.Combine(director, $"{nume}.xml");
            var cronometru = Stopwatch.StartNew();
            using (var fisier = File.Create(cale))
                SaftXml.Scrie(dto, fisier);
            if (nume == prefix)
                Console.WriteLine($"     MĂSURAT (D17-V3 timp): proiecția {msProiectie:N0} ms, scrierea "
                    + $"{cronometru.Elapsed.TotalMilliseconds:N0} ms, {new FileInfo(cale).Length:N0} octeți → {cale}");
            return cale;
        }

        var caleXml = Scrie(saft, prefix);
        var doc = XDocument.Load(caleXml);
        XNamespace ns = SaftXml.SpatiuNume;
        List<XElement> Toate(string nume) => doc.Descendants(ns + nume).ToList();
        XElement Unul(string nume) => doc.Descendants(ns + nume).FirstOrDefault();
        string Text(XElement e, string nume) => e?.Element(ns + nume)?.Value;

        // ---------------- Ordinea copiilor = a XSD-ului ----------------
        // Nu „conține elementele", ci „în ordinea schemei, fără nimic străin":
        // `xs:sequence` e o ordine OBLIGATORIE, iar opționalele pot lipsi — deci
        // secvența reală trebuie să fie o SUBSECVENȚĂ a celei din schemă. Un element
        // pus greșit (ex. `UnitPrice` înaintea perechii UM/factor) pică aici, nu în
        // mesajul opac al validatorului.
        bool SubsecventaA(XElement parinte, string[] ordineXsd, out string explicatie) {
            var toti = parinte == null ? [] : parinte.Elements().Select(e => e.Name.LocalName).ToArray();
            // Repetițiile consecutive sunt `maxOccurs="unbounded"` (13 `StockMovement`,
            // n `StockMovementLine`) — ele nu spun nimic despre ORDINE, deci se
            // colapsează înainte de comparație; ce rămâne trebuie să fie o
            // subsecvență strictă a `xs:sequence`-ului.
            var real = toti.Where((n, k) => k == 0 || toti[k - 1] != n).ToArray();
            var strain = real.Where(n => !ordineXsd.Contains(n)).ToList();
            var i = 0;
            var inOrdine = true;
            foreach (var nume in real) {
                var poz = Array.IndexOf(ordineXsd, nume, i);
                if (poz < 0) { inOrdine = false; break; }
                i = poz + 1;
            }
            explicatie = $"[{string.Join(", ", real)}]"
                + (strain.Count > 0 ? $" — STRĂINE: {string.Join(", ", strain)}" : "")
                + (inOrdine ? "" : " — ORDINE GREȘITĂ");
            return strain.Count == 0 && inOrdine;
        }

        string[] ordineStocFizic = [
            "WarehouseID", "LocationID", "ProductCode", "StockAccountNo", "ProductType", "ProductStatus",
            "StockAccountCommodityCode", "OwnerID", "UOMPhysicalStock", "UOMToUOMBaseConversionFactor",
            "UnitPrice", "OpeningStockQuantity", "OpeningStockValue", "ClosingStockQuantity",
            "ClosingStockValue", "StockCharacteristics",
        ];
        string[] ordineMiscare = [
            "MovementReference", "MovementDate", "MovementPostingDate", "MovementPostingTime", "TaxPointDate",
            "MovementType", "SourceID", "SystemID", "DocumentReference", "StockMovementLine",
        ];
        string[] ordineLinie = [
            "LineNumber", "AccountID", "TransactionID", "CustomerID", "SupplierID", "ShipTo", "ShipFrom",
            "ProductCode", "StockAccountNo", "Quantity", "UnitOfMeasure",
            "UOMToUOMPhysicalStockConversionFactor", "BookValue", "MovementSubType", "MovementComments",
            "TaxInformation",
        ];
        string[] ordineMasterFiles = [
            "GeneralLedgerAccounts", "Taxonomies", "Customers", "Suppliers", "TaxTable", "UOMTable",
            "AnalysisTypeTable", "MovementTypeTable", "Products", "PhysicalStock", "Owners", "Assets",
        ];
        string[] ordineSourceDocuments = [
            "SalesInvoices", "PurchaseInvoices", "Payments", "MovementOfGoods", "AssetTransactions",
        ];
        string[] ordineMovementOfGoods = [
            "NumberOfMovementLines", "TotalQuantityReceived", "TotalQuantityIssued", "StockMovement",
        ];

        var intrariStoc = Toate("PhysicalStockEntry");
        var miscari = Toate("StockMovement");
        var liniiMiscare = Toate("StockMovementLine");
        var miscareaCuMulteLinii = miscari.OrderByDescending(m => m.Elements(ns + "StockMovementLine").Count())
            .FirstOrDefault();
        var intrareaCuLot = intrariStoc.FirstOrDefault(e => e.Element(ns + "StockAccountNo") != null)
            ?? intrariStoc.FirstOrDefault();
        var liniaCuLot = liniiMiscare.FirstOrDefault(l => l.Element(ns + "StockAccountNo") != null)
            ?? liniiMiscare.FirstOrDefault();
        var ordini = new List<(string Ce, bool Ok, string Explicatie)>();
        foreach (var (ce, el, ordine) in new (string, XElement, string[])[] {
                     ("MasterFiles", Unul("MasterFiles"), ordineMasterFiles),
                     ("SourceDocuments", Unul("SourceDocuments"), ordineSourceDocuments),
                     ("MovementOfGoods", Unul("MovementOfGoods"), ordineMovementOfGoods),
                     ("PhysicalStockEntry", intrareaCuLot, ordineStocFizic),
                     ("StockMovement", miscareaCuMulteLinii, ordineMiscare),
                     ("StockMovementLine", liniaCuLot, ordineLinie),
                 }) {
            var ok = SubsecventaA(el, ordine, out var explicatie);
            ordini.Add((ce, ok, explicatie));
            Console.WriteLine($"     MĂSURAT (D17-V3 ordine/{ce}): {explicatie}");
        }

        var cantitatiNegative = liniiMiscare
            .Count(l => decimal.Parse(l.Element(ns + "Quantity")!.Value,
                System.Globalization.CultureInfo.InvariantCulture) < 0m);
        var loturiPeLinii = liniiMiscare.Count(l => l.Element(ns + "StockAccountNo") != null);
        var loturiPeStoc = intrariStoc.Count(e => e.Element(ns + "StockAccountNo") != null);
        var lungimeMaximaLot = intrariStoc.Concat(liniiMiscare)
            .Select(e => e.Element(ns + "StockAccountNo")?.Value?.Length ?? 0).DefaultIfEmpty(0).Max();
        var lungimeMaximaReferinta = miscari
            .Select(m => m.Element(ns + "MovementReference")?.Value?.Length ?? 0).DefaultIfEmpty(0).Max();
        Console.WriteLine($"     MĂSURAT (D17-V3 structură): `HeaderComment` = „{Text(Unul("Header"), "HeaderComment")}”; "
            + $"{Toate("MovementTypeTableEntry").Count} tipuri de mișcare, {intrariStoc.Count} intrări de stoc fizic, "
            + $"{miscari.Count} mișcări / {liniiMiscare.Count} linii "
            + $"(`NumberOfMovementLines` = {Text(Unul("MovementOfGoods"), "NumberOfMovementLines")}, "
            + $"recepționat {Text(Unul("MovementOfGoods"), "TotalQuantityReceived")} / "
            + $"eliberat {Text(Unul("MovementOfGoods"), "TotalQuantityIssued")}); "
            + $"{cantitatiNegative} cantități NEGATIVE; `StockAccountNo` pe {loturiPeStoc}/{intrariStoc.Count} intrări "
            + $"și {loturiPeLinii}/{liniiMiscare.Count} linii, lungime max {lungimeMaximaLot}; "
            + $"`MovementReference` max {lungimeMaximaReferinta} caractere.");

        s.Check("D17-V3 fișierul S se citește înapoi cu forma cerută: `HeaderComment = C` (SINGURUL lucru care-l face "
            + "declarație de stocuri), cele trei secțiuni noi pline cu exact câte rânduri are DTO-ul, totalurile "
            + "lui `MovementOfGoods` scrise de server, iar ORDINEA copiilor pe fiecare structură nouă e o "
            + "subsecvență a `xs:sequence`-ului din XSD — inclusiv cele două perechi UM/factor, care în schemă "
            + "sunt `xs:sequence`-uri INTERIOARE, nu elemente libere",
            doc.Root?.Name == ns + "AuditFile"
            && Text(Unul("Header"), "HeaderComment") == "C"
            && Toate("MovementTypeTableEntry").Count == saft.TipuriMiscare.Count
            && Toate("PhysicalStock").Count == 1 && intrariStoc.Count == saft.StocFizic.Count
            && miscari.Count == saft.MiscariStoc.Count
            && liniiMiscare.Count == saft.NumberOfMovementLines
            && Text(Unul("MovementOfGoods"), "NumberOfMovementLines") == saft.NumberOfMovementLines.ToString()
            && ordini.All(o => o.Ok)
            && loturiPeStoc == saft.StocFizic.Count(e => e.StockAccountNo != null)
            && loturiPeLinii == saft.MiscariStoc.Sum(m => m.Linii.Count(l => l.StockAccountNo != null))
            && cantitatiNegative == saft.MiscariStoc.Sum(m => m.Linii.Count(l => l.Quantity < 0m))
            && cantitatiNegative > 0);

        // Secțiunile pe care declarația de STOCURI nu le raportează: TOATE ies tag
        // deschis/închis (§A.5), inclusiv cele patru care pe lunar poartă totaluri.
        // Regula NU e o deducție din XSD (unde `NumberOfEntries` e `minOccurs=0`,
        // deci un 0 ar fi fost legal), ci un fapt MĂSURAT: prima versiune a
        // scriitorului le-a scris cu `NumberOfEntries = 0` și validatorul a respins
        // fișierul cu „elementul 'NumberOfEntries' a depasit numarul maxim de
        // aparitii (0)" — profilul `C` le pune maxOccurs pe 0. Riscul 2 al
        // contractului, închis: nu doar „tolerate", ci OBLIGATORIU goale.
        var goale = new[] {
            "Customers", "Suppliers", "Owners", "Assets",
            "GeneralLedgerEntries", "SalesInvoices", "PurchaseInvoices", "Payments",
            // `AnalysisTypeTable` intră în listă de la fixul F8: tabela e
            // nomenclatorul valorilor pe care fișierul le FOLOSEȘTE, iar S nu emite
            // `Analysis` pe nicio linie — o listă pe care nimic n-o referă e zgomot
            // în fișier și șapte interogări pe registrul contabil în proiecție.
            "AnalysisTypeTable",
        }.Select(n => (Nume: n, Copii: Unul(n)?.Elements().Count() ?? -1)).ToList();
        Console.WriteLine($"     MĂSURAT (D17-V3 secțiuni L): [{string.Join(", ",
            goale.Select(g => $"{g.Nume}:{g.Copii}"))}] copii.");
        s.Check("D17-V3 (riscul 2 + fixul F8) secțiunile pe care declarația de STOCURI nu le raportează sunt "
            + "PREZENTE și COMPLET goale — nici măcar `NumberOfEntries`/`TotalDebit`/`TotalCredit` la zero, "
            + "fiindcă profilul `C` al validatorului le refuză („a depasit numarul maxim de aparitii (0)”): un "
            + "total la zero descrie rânduri care aici nu există, iar §A.5 se aplică literal. `AnalysisTypeTable` "
            + "e printre ele: fără `Analysis` pe linii, tabela n-are ce declara",
            goale.All(g => g.Copii == 0) && saft.TipuriAnaliza.Count == 0);

        // ---------------- Oracolul: validatorul oficial ----------------
        var rezultat = Duk.Valideaza(caleXml);
        Console.WriteLine($"     MĂSURAT (D17-V3 DUK): {rezultat.Rezumat}");
        Console.WriteLine($"         COMANDA {rezultat.Comanda}");
        foreach (var e in rezultat.Erori.Take(30))
            Console.WriteLine($"         EROARE DUK: {e}");
        foreach (var a in rezultat.Avertismente.Take(30))
            Console.WriteLine($"         ATENȚIONARE DUK: {a}");
        s.Check("D17-V3 VALIDATORUL OFICIAL (DUKIntegrator, kit local) acceptă fișierul de STOCURI al scenei — "
                + "cu `HeaderComment = C`, cu cantități NEGATIVE pe ieșiri (riscul 1), cu secțiunile lunarului "
                + "goale (riscul 2), cu `StockCharacteristic = 0` (riscul 3), cu `StockAccountNo` = Guid de 36 "
                + "caractere (riscul 4) și cu `ProductType` = simbol de cont (riscul 5)",
                rezultat.Valid);

        // ---------------- Riscurile care cer fișiere DERIVATE ----------------
        // Fiecare derivată schimbă UN lucru, se scrie și se validează; verdictul se
        // RAPORTEAZĂ (nu e o probă de trecut/picat: e o măsurătoare care intră în
        // §Închidere al contractului).
        DukRezultat Masoara(string nume, string intrebare, Action modifica, Action refa) {
            modifica();
            try {
                var cale = Scrie(saft, $"{prefix}-{nume}");
                var r = Duk.Valideaza(cale);
                Console.WriteLine($"     MĂSURAT (D17-V3/{nume}): {intrebare} → {r.Rezumat}");
                foreach (var e in r.Erori.Take(8))
                    Console.WriteLine($"         EROARE DUK: {e}");
                foreach (var a in r.Avertismente.Take(8))
                    Console.WriteLine($"         ATENȚIONARE DUK: {a}");
                return r;
            }
            finally {
                refa();
            }
        }

        // ═══ `PhysicalStock` gol pe „C": fișierul NU se mai scrie (D17-D4) ═══
        // Pasul 3 a MĂSURAT că profilul „la cerere" cere secțiunea PREZENTĂ
        // („elementul 'PhysicalStock' ar fi trebuit sa apara de minimum 1 ori"), deși
        // XSD-ul o are `minOccurs=0`, iar un tag gol pică schema (`PhysicalStockEntry`
        // e obligatoriu înăuntru). Nu există deci formă validă a unei declarații S
        // fără nicio intrare de stoc fizic — ceea ce înseamnă că fișierul nu se poate
        // DEPUNE, nu că se depune gol. Proba de la pasul 3 („omisă ⇒ trece") a fost
        // înlocuită cu asta: scriitorul REFUZĂ, exact ca pe `Neaplicabil`, iar
        // `GET api/proiectii/saft/stocuri/xml` îl traduce în 422 cât timp răspunsul e
        // încă schimbabil. Nu cere DUK — e o probă de contract al scriitorului, deci
        // stă ÎNAINTEA ieșirii pe kit lipsă.
        var stocInitial = saft.StocFizic;
        string mesajFaraStoc = null;
        saft.StocFizic = [];
        try {
            using var fluxFaraStoc = new MemoryStream();
            SaftXml.Scrie(saft, fluxFaraStoc);
        }
        catch (InvalidOperationException ex) {
            mesajFaraStoc = ex.Message;
        }
        finally {
            saft.StocFizic = stocInitial;
        }
        Console.WriteLine($"     MĂSURAT (D17-V3 fără stoc fizic): `SaftXml.Scrie` → "
            + $"„{mesajFaraStoc ?? "<A SCRIS FIȘIERUL>"}”.");
        s.Check("D17-V3 `SaftXml.Scrie` REFUZĂ o declarație S (`HeaderComment = C`) cu `StocFizic` GOL, cu mesaj de "
            + "domeniu — `PhysicalStock` e obligatoriu prezent pe profilul „la cerere” (măsurat la pasul 3), deci "
            + "fișierul n-ar fi depozabil; un XML scris cu status 200 ar părea succes",
            mesajFaraStoc != null && mesajFaraStoc.Contains("stoc fizic", StringComparison.OrdinalIgnoreCase)
            && mesajFaraStoc.Contains("PhysicalStock", StringComparison.Ordinal));

        if (!rezultat.Disponibil)
            return;

        // ═══ Riscul 1: `Quantity` NEGATIV pe ieșiri ═══
        // Fișierul de bază îl conține deja; derivata e ANTIDOTUL propus de contract
        // (|q|, semnul doar pe storno, direcția din codul de mișcare). Se măsoară ca
        // să existe cifra ambelor variante — decizia dacă proiecția se schimbă e a
        // arhitectului, nu a probei.
        var cantitatiInitiale = saft.MiscariStoc
            .SelectMany(m => m.Linii.Select(l => (Miscare: m, Linie: l, Q: l.Quantity))).ToList();
        var (primiteInitial, eliberateInitial) = (saft.TotalQuantityReceived, saft.TotalQuantityIssued);
        Masoara("cantitati-absolute",
            "riscul 1 — `Quantity` = |cantitate| (semnul DOAR pe jumătatea de storno), direcția rămasă în "
            + "`MovementType`",
            () => {
                foreach (var (m, l, q) in cantitatiInitiale)
                    l.Quantity = m.Storno ? -Math.Abs(q) : Math.Abs(q);
                saft.TotalQuantityReceived = saft.MiscariStoc.SelectMany(m => m.Linii)
                    .Where(l => l.Quantity > 0m).Sum(l => l.Quantity);
                saft.TotalQuantityIssued = Math.Abs(saft.MiscariStoc.SelectMany(m => m.Linii)
                    .Where(l => l.Quantity < 0m).Sum(l => l.Quantity));
            },
            () => {
                foreach (var (_, l, q) in cantitatiInitiale)
                    l.Quantity = q;
                saft.TotalQuantityReceived = primiteInitial;
                saft.TotalQuantityIssued = eliberateInitial;
            });

        // ═══ Riscul 2, INVERS: secțiunile lunarului cu totalurile la zero ═══
        // Fișierul de bază le scrie goale fiindcă așa cere profilul `C`. Aici se
        // măsoară forma din care a ieșit regula — aceleași trei elemente pe care
        // lunarul le scrie mereu, cu 0 — ca „obligatoriu gol" să fie o CIFRĂ, nu o
        // convingere. Post-procesare de XML: e o întrebare despre FORMĂ, nu o a doua
        // cale de scriere.
        var caleCuTotaluri = Path.Combine(director, $"{prefix}-sectiuni-L-cu-totaluri.xml");
        var docTotaluri = XDocument.Load(caleXml);
        foreach (var nume in new[] { "GeneralLedgerEntries", "SalesInvoices", "PurchaseInvoices", "Payments" })
            docTotaluri.Descendants(ns + nume).FirstOrDefault()?.Add(
                new XElement(ns + "NumberOfEntries", "0"),
                new XElement(ns + "TotalDebit", "0.00"),
                new XElement(ns + "TotalCredit", "0.00"));
        docTotaluri.Save(caleCuTotaluri);
        var rTotaluri = Duk.Valideaza(caleCuTotaluri);
        Console.WriteLine($"     MĂSURAT (D17-V3/sectiuni-L-cu-totaluri): riscul 2 — `GeneralLedgerEntries`/"
            + $"`SalesInvoices`/`PurchaseInvoices`/`Payments` cu `NumberOfEntries = 0` și totalurile 0,00 "
            + $"→ {rTotaluri.Rezumat}");
        foreach (var e in rTotaluri.Erori.Take(8))
            Console.WriteLine($"         EROARE DUK: {e}");

        // ═══ Riscul 3: `StockCharacteristic` — cheia e text liber sau nomenclator? ═══
        var intrareProba = saft.StocFizic[0];
        var (cheieInitiala, valoareInitiala) = (intrareProba.StockCharacteristic, intrareProba.StockCharacteristicValue);
        Masoara("caracteristica-acciza",
            $"riscul 3 — `StockCharacteristic` = „blue_35”/„35” în loc de („{cheieInitiala}”,„{valoareInitiala}”): "
            + "cheia din nomenclatorul accizelor",
            () => { intrareProba.StockCharacteristic = "blue_35"; intrareProba.StockCharacteristicValue = "35"; },
            () => {
                intrareProba.StockCharacteristic = cheieInitiala;
                intrareProba.StockCharacteristicValue = valoareInitiala;
            });

        // ═══ Riscul 4: lungimile ═══
        // `StockAccountNo` = Guid de 36 într-un câmp de 70 e deja în bază. Ce nu e:
        // marginea lui `MovementReference` (35) — exact limita pe care proiecția o
        // impune tăind numărul. Două derivate: la limită și cu UN caracter peste.
        var miscareProba = saft.MiscariStoc[0];
        var referintaInitiala = miscareProba.MovementReference;
        foreach (var (nume, lungime) in new[] { ("referinta-35", 35), ("referinta-36", 36) })
            Masoara(nume, $"riscul 4 — `MovementReference` de {lungime} caractere "
                    + $"(`SAFmiddle1textType` = max {SaftReguli.LungimeMovementReference})",
                () => miscareProba.MovementReference = new string('R', lungime),
                () => miscareProba.MovementReference = referintaInitiala);

        // ═══ Riscul 5: `ProductType` = simbol de cont ═══
        // Întrebarea nu e „trece 371?" (trece, e în bază), ci „e verificat contra
        // planului de conturi, ca `AccountID`?". Un simbol care NU există în plan
        // separă cele două răspunsuri.
        var tipInitial = intrareProba.ProductType;
        Masoara("product-type-fara-cont",
            $"riscul 5 — `ProductType` = „999999” (simbol care NU e în planul de conturi) în loc de "
            + $"„{tipInitial}”: e validat ca `AccountID` sau e text liber?",
            () => intrareProba.ProductType = "999999",
            () => intrareProba.ProductType = tipInitial);

    }
}
