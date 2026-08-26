using System.Globalization;
using System.Text;
using System.Xml;

namespace Atlas.Conta.BackOffice.Module.Saft;

// FIȘIERUL D406 — scriitor STREAMING peste `SaftDto` (felia 16, D16-D5).
//
// ═══ Ce e și ce nu e ═══
// Singura lui răspundere e FORMA: numele, ordinea și tipul elementelor. Nicio
// decizie de conținut nu se ia aici — cifrele, identificatorii, codurile de taxă
// și avertismentele sunt ale proiecției (`SaftProiectii`), iar ce nu se poate
// declara a ieșit deja în `Neincluse`. Consecința practică: dacă validatorul
// respinge fișierul, se știe imediat dacă e o problemă de SCRIERE (aici) sau de
// MODEL (acolo).
//
// ═══ Ordinea elementelor = a SCHEMEI, nu a DTO-ului ═══
// Sursa ordinii e XSD-ul oficial (`Ro_SAFT_Schema_v247_20230306.xsd`, 2.4.7 —
// singurul XSD care există ca fișier; kitul DUK nu-l livrează, validarea e
// compilată în `D406Validator.jar`). Fiecare `xs:sequence` de acolo e o ordine
// OBLIGATORIE, iar cele șapte `xs:choice` (structura §C.4) sunt exclusive:
//   • `OpeningDebitBalance` XOR `OpeningCreditBalance` (idem `Closing*`);
//   • `DebitAmount` XOR `CreditAmount` pe linia de GL;
//   • `TaxPercentage` XOR `FlatTaxRate` în `TaxCodeDetails`;
//   • `CustomerInfo` XOR `SupplierInfo` pe factură, fiecare cu
//     `CustomerID`/`SupplierID` XOR `Name` — iar ramura `Name` e RESPINSĂ
//     explicit de validator („Va rugam sa folositi CustomerID!"), deci nu se
//     emite niciodată;
//   • `IBANNumber` XOR `BankAccountNumber` (+ nume + SortCode) în contul bancar;
//   • perechea de date XOR perechea de perioade în `SelectionCriteria`.
// Absența unui element opțional se scrie prin OMISIUNE, nu prin `<X/>` sau `0`:
// `decimal?`/`null` din DTO înseamnă „nu e cazul", iar `0` ar fi o afirmație.
//
// Secțiunile pe care modul L nu le raportează (`MovementTypeTable`, `Owners`,
// `Assets`, `MovementOfGoods`) NU sunt opționale în schemă — se emit ca tag
// deschis/închis, exact cum cere nota metodologică (§A.5: „în cazul în care nu
// este nimic de raportat se va transmite doar tag-urile de început și de sfârșit
// ale sub-secțiunii").
//
// ═══ Detaliile care nu se văd din tabelul de câmpuri ═══
//   • `TaxAmount` NU e un număr, e o `AmountStructure` (Amount + CurrencyCode +
//     CurrencyAmount) — la fel `DebitAmount`/`CreditAmount`/`InvoiceLineAmount`/
//     `PaymentLineAmount`.
//   • `ExchangeRate` e opțional și NU se emite: totul e în moneda antetului
//     (`CurrencyAmount == Amount`). Validatorul verifică `Amount ==
//     round(CurrencyAmount × ExchangeRate)` — dacă tratează cursul lipsă ca 0,
//     fișierul pică; e riscul 6 al contractului, MĂSURAT cu DUK în V3.
//   • `PaymentMechanism` nu stă pe `Payment`, ci în `PaymentSettlement` — de
//     aceea perechea metodă/mecanism se sparge în două locuri.
//   • Diacriticele rămân INTACTE: XML-ul e UTF-8, iar exportul legacy le tăia
//     fără motiv documentat (riscul 9, măsurat în V3).
//   • Zecimalele: `InvariantCulture`, punct, fără separator de mii. Banii cu
//     EXACT 2 zecimale (`SAFmonetaryType` are `fractionDigits 2`, deci o a treia
//     ar fi invalidă — inclusiv pe `UnitPrice`, care în model are 6), cantitățile
//     cu până la 6, cota fără zerouri inutile.
public static class SaftXml {

    /// <summary>Namespace-ul de PRODUCȚIE (cel de test e `…d406t…`).</summary>
    public const string SpatiuNume = "mfp:anaf:dgti:d406:declaratie:v1";

    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Scrie declarația în flux. `Neaplicabil` (profilul bugetar) ⇒
    /// `InvalidOperationException` cu motivul proiecției: un fișier gol semnat cu
    /// CUI-ul cuiva ar fi o declarație falsă, nu o listă goală (apelantul REST îl
    /// traduce în 422).
    /// </summary>
    public static void Scrie(SaftDto dto, Stream iesire) {
        ArgumentNullException.ThrowIfNull(dto);
        ArgumentNullException.ThrowIfNull(iesire);
        if (dto.Neaplicabil != null)
            throw new InvalidOperationException(dto.Neaplicabil);
        if (dto.Header == null)
            throw new InvalidOperationException(
                "Declarația D406 n-are antet — proiecția n-a rulat sau societatea raportoare lipsește.");

        var setari = new XmlWriterSettings {
            // Fără BOM: kitul acceptă ambele, iar absența lui e alegerea care nu
            // are nevoie de justificare.
            Encoding = new UTF8Encoding(false),
            Indent = true,
            IndentChars = "  ",
            // Fluxul e al apelantului (răspuns HTTP sau fișier) — scriitorul nu-l
            // închide.
            CloseOutput = false,
        };
        using var w = XmlWriter.Create(iesire, setari);
        var moneda = dto.Header.DefaultCurrencyCode ?? SaftProiectii.DefaultCurrencyCode;

        // ── helper-e de scriere ─────────────────────────────────────────────
        void Start(string nume) => w.WriteStartElement(nume, SpatiuNume);
        void Stop() => w.WriteEndElement();
        // Opțional: absent ⇒ NU se emite.
        void El(string nume, string valoare) {
            if (!string.IsNullOrEmpty(valoare))
                w.WriteElementString(nume, SpatiuNume, valoare);
        }
        // Obligatoriu în schemă: se emite chiar gol (proiecția a strigat deja
        // lipsa printr-un avertisment — fișierul nu inventează o valoare).
        void ElCerut(string nume, string valoare) =>
            w.WriteElementString(nume, SpatiuNume, valoare ?? "");
        void ElBani(string nume, decimal? valoare) {
            if (valoare is decimal v)
                w.WriteElementString(nume, SpatiuNume, Bani(v));
        }
        void ElData(string nume, DateOnly valoare) =>
            w.WriteElementString(nume, SpatiuNume, valoare.ToString("yyyy-MM-dd", Inv));
        void ElInt(string nume, int valoare) =>
            w.WriteElementString(nume, SpatiuNume, valoare.ToString(Inv));
        // `AmountStructure` (5.2) — fără `ExchangeRate` (vezi antetul).
        void Suma(string nume, decimal valoare) {
            Start(nume);
            ElCerut("Amount", Bani(valoare));
            ElCerut("CurrencyCode", moneda);
            ElCerut("CurrencyAmount", Bani(valoare));
            Stop();
        }
        // `AddressStructure` (5.1) — `City` și `Country` sunt obligatorii.
        void Adresa(string nume, SaftAdresa a) {
            Start(nume);
            El("StreetName", a?.StreetName);
            El("Number", a?.Number);
            El("AdditionalAddressDetail", a?.AdditionalAddressDetail);
            ElCerut("City", a?.City);
            El("PostalCode", a?.PostalCode);
            El("Region", a?.Region);
            ElCerut("Country", a?.Country);
            Stop();
        }
        // `AnalysisStructure` (5.3).
        void Analize(List<SaftAnaliza> analize) {
            foreach (var a in analize ?? []) {
                Start("Analysis");
                ElCerut("AnalysisType", a.AnalysisType);
                ElCerut("AnalysisID", a.AnalysisID);
                Stop();
            }
        }
        // `TaxInformationStructure` (5.15) — obligatoriu pe FIECARE linie. Numele
        // elementului e PARAMETRU fiindcă aceeași structură se cheamă
        // `TaxInformation` pe linie și `TaxInformationTotals` în totalurile
        // documentului (măsurat cu DUK: „elementul 'TaxInformation' nu poate sa
        // apara in descendeta caii …/InvoiceDocumentTotals").
        void Taxa(SaftTaxInfo t, string nume = "TaxInformation") {
            var taxa = t ?? new SaftTaxInfo {
                TaxType = SaftReguli.TaxTypeNefiscal, TaxCode = SaftReguli.TaxCodeNefiscal
            };
            Start(nume);
            ElCerut("TaxType", taxa.TaxType);
            ElCerut("TaxCode", taxa.TaxCode);
            if (taxa.TaxPercentage is decimal cota)
                ElCerut("TaxPercentage", Cota(cota));
            ElBani("TaxBase", taxa.TaxBase);
            Suma("TaxAmount", taxa.TaxAmount);
            Stop();
        }
        // Soldurile: `xs:choice` pe fiecare pereche (nota [1]).
        void Solduri(decimal? debitDeschidere, decimal? creditDeschidere,
            decimal? debitInchidere, decimal? creditInchidere) {
            if (creditDeschidere is decimal cd)
                ElCerut("OpeningCreditBalance", Bani(cd));
            else
                ElCerut("OpeningDebitBalance", Bani(debitDeschidere ?? 0m));
            if (creditInchidere is decimal ci)
                ElCerut("ClosingCreditBalance", Bani(ci));
            else
                ElCerut("ClosingDebitBalance", Bani(debitInchidere ?? 0m));
        }

        w.WriteStartDocument();
        Start("AuditFile");

        // ── 1. Header (S.H.1–13 + H.2/H.3) ──────────────────────────────────
        var h = dto.Header;
        Start("Header");
        ElCerut("AuditFileVersion", h.AuditFileVersion);
        ElCerut("AuditFileCountry", h.AuditFileCountry);
        El("AuditFileRegion", h.AuditFileRegion);
        ElData("AuditFileDateCreated", h.AuditFileDateCreated);
        ElCerut("SoftwareCompanyName", h.SoftwareCompanyName);
        ElCerut("SoftwareID", h.SoftwareID);
        ElCerut("SoftwareVersion", h.SoftwareVersion);
        Start("Company");
        ElCerut("RegistrationNumber", h.RegistrationNumber);
        ElCerut("Name", h.Name);
        Adresa("Address", h.Address);
        // `ContactHeaderStructure` (5.7): persoana și telefonul sunt obligatorii.
        Start("Contact");
        Start("ContactPerson");
        ElCerut("FirstName", h.ContactFirstName);
        ElCerut("LastName", h.ContactLastName);
        Stop();
        ElCerut("Telephone", h.Telephone);
        El("Email", h.Email);
        Stop();
        // `BankAccountStructure` (5.4) e un `xs:choice`: IBAN-ul EXCLUDE numărul
        // de cont intern, nu-l însoțește.
        Start("BankAccount");
        if (!string.IsNullOrEmpty(h.IBANNumber))
            ElCerut("IBANNumber", h.IBANNumber);
        else
            ElCerut("BankAccountNumber", h.BankAccountNumber);
        Stop();
        Stop(); // Company
        ElCerut("DefaultCurrencyCode", moneda);
        Start("SelectionCriteria");
        ElInt("PeriodStart", h.PeriodStart);
        ElInt("PeriodStartYear", h.PeriodStartYear);
        ElInt("PeriodEnd", h.PeriodEnd);
        ElInt("PeriodEndYear", h.PeriodEndYear);
        Stop();
        ElCerut("HeaderComment", h.HeaderComment);
        ElCerut("SegmentIndex", h.SegmentIndex);
        // Grafia e a XSD-ului (`s` mic), nu a mesajului de eroare al
        // validatorului — cea care contează la parsare (structura §F.23).
        ElCerut("TotalSegmentsInsequence", h.TotalSegmentsInSequence);
        ElCerut("TaxAccountingBasis", h.TaxAccountingBasis);
        Stop(); // Header

        // ── 2. MasterFiles ──────────────────────────────────────────────────
        Start("MasterFiles");

        Start("GeneralLedgerAccounts");
        foreach (var c in dto.Conturi) {
            Start("Account");
            ElCerut("AccountID", c.AccountID);
            ElCerut("AccountDescription", c.AccountDescription);
            ElCerut("AccountType", c.AccountType);
            Solduri(c.OpeningDebitBalance, c.OpeningCreditBalance,
                c.ClosingDebitBalance, c.ClosingCreditBalance);
            Stop();
        }
        Stop();

        ScrieTerti("Customers", "Customer", "CustomerID", dto.Clienti);
        ScrieTerti("Suppliers", "Supplier", "SupplierID", dto.Furnizori);

        // `TaxTable`: un singur `TaxTableEntry` (TVA), cu un `TaxCodeDetails` per
        // cod folosit — gruparea e a tipului de impozit, nu a codului.
        Start("TaxTable");
        if (dto.Taxe.Count > 0) {
            Start("TaxTableEntry");
            ElCerut("TaxType", dto.Taxe[0].TaxType);
            ElCerut("Description", "Taxa pe valoarea adăugată");
            foreach (var t in dto.Taxe) {
                Start("TaxCodeDetails");
                ElCerut("TaxCode", t.TaxCode);
                El("Description", t.Description);
                ElCerut("TaxPercentage", Cota(t.TaxPercentage));
                ElCerut("BaseRate", t.BaseRate.ToString("0.0000", Inv));
                ElCerut("Country", t.Country);
                Stop();
            }
            Stop();
        }
        Stop();

        Start("UOMTable");
        foreach (var u in dto.Unitati) {
            Start("UOMTableEntry");
            ElCerut("UnitOfMeasure", u.UnitOfMeasure);
            ElCerut("Description", u.Description);
            Stop();
        }
        Stop();

        Start("AnalysisTypeTable");
        foreach (var a in dto.TipuriAnaliza) {
            Start("AnalysisTypeTableEntry");
            ElCerut("AnalysisType", a.AnalysisType);
            ElCerut("AnalysisTypeDescription", a.AnalysisTypeDescription);
            ElCerut("AnalysisID", a.AnalysisID);
            ElCerut("AnalysisIDDescription", a.AnalysisIDDescription);
            Stop();
        }
        Stop();

        // Modul S (stocuri), nu L: tag deschis/închis (§A.5).
        Start("MovementTypeTable");
        Stop();

        Start("Products");
        foreach (var p in dto.Produse) {
            Start("Product");
            ElCerut("ProductCode", p.ProductCode);
            El("GoodsServicesID", p.GoodsServicesID);
            ElCerut("Description", p.Description);
            ElCerut("ProductCommodityCode", p.ProductCommodityCode);
            El("ValuationMethod", p.ValuationMethod);
            ElCerut("UOMBase", p.UOMBase);
            ElCerut("UOMStandard", p.UOMStandard);
            ElCerut("UOMToUOMBaseConversionFactor", Factor(p.UOMToUOMBaseConversionFactor));
            Stop();
        }
        Stop();

        // Modul S / A: goale în lunar.
        Start("Owners");
        Stop();
        Start("Assets");
        Stop();
        Stop(); // MasterFiles

        // ── 3. GeneralLedgerEntries ─────────────────────────────────────────
        Start("GeneralLedgerEntries");
        ElInt("NumberOfEntries", dto.Rezumat.Tranzactii);
        ElCerut("TotalDebit", Bani(dto.Rezumat.TotalDebit));
        ElCerut("TotalCredit", Bani(dto.Rezumat.TotalCredit));
        foreach (var j in dto.Jurnale) {
            Start("Journal");
            ElCerut("JournalID", j.JournalID);
            ElCerut("Description", j.Description);
            ElCerut("Type", j.Type);
            foreach (var t in j.Tranzactii) {
                Start("Transaction");
                ElCerut("TransactionID", t.TransactionID);
                ElInt("Period", t.Period);
                ElInt("PeriodYear", t.PeriodYear);
                ElData("TransactionDate", t.TransactionDate);
                ElCerut("Description", t.Description);
                ElData("SystemEntryDate", t.SystemEntryDate);
                ElData("GLPostingDate", t.GLPostingDate);
                ElCerut("CustomerID", t.CustomerID);
                ElCerut("SupplierID", t.SupplierID);
                foreach (var l in t.Linii) {
                    Start("TransactionLine");
                    ElCerut("RecordID", l.RecordID);
                    ElCerut("AccountID", l.AccountID);
                    Analize(l.Analiza);
                    ElCerut("CustomerID", l.CustomerID);
                    ElCerut("SupplierID", l.SupplierID);
                    ElCerut("Description", l.Description);
                    // `xs:choice` (nota [4]): latura decide elementul, semnul
                    // rămâne pe sumă („storno în negru").
                    Suma(l.DebitCreditIndicator == "D" ? "DebitAmount" : "CreditAmount", l.Amount);
                    Taxa(l.TaxInformation);
                    Stop();
                }
                Stop();
            }
            Stop();
        }
        Stop();

        // ── 4. SourceDocuments ──────────────────────────────────────────────
        Start("SourceDocuments");
        ScrieFacturi("SalesInvoices", dto.FacturiEmise, vanzare: true);
        ScrieFacturi("PurchaseInvoices", dto.FacturiPrimite, vanzare: false);

        Start("Payments");
        ElInt("NumberOfEntries", dto.Plati.Count);
        ElCerut("TotalDebit", Bani(dto.Plati.Where(p => p.Linii.Any(l => l.DebitCreditIndicator == "D"))
            .Sum(p => p.Linii.Where(l => l.DebitCreditIndicator == "D").Sum(l => l.PaymentLineAmount))));
        ElCerut("TotalCredit", Bani(dto.Plati.Where(p => p.Linii.Any(l => l.DebitCreditIndicator == "C"))
            .Sum(p => p.Linii.Where(l => l.DebitCreditIndicator == "C").Sum(l => l.PaymentLineAmount))));
        foreach (var p in dto.Plati) {
            Start("Payment");
            ElCerut("PaymentRefNo", p.PaymentRefNo);
            ElInt("Period", dto.Luna);
            ElInt("PeriodYear", dto.An);
            ElData("TransactionDate", p.TransactionDate);
            ElCerut("PaymentMethod", p.PaymentMethod);
            ElCerut("Description", p.Description);
            foreach (var l in p.Linii) {
                Start("PaymentLine");
                ElCerut("LineNumber", l.LineNumber.ToString(Inv));
                El("SourceDocumentID", l.SourceDocumentID);
                ElCerut("AccountID", l.AccountID);
                Analize(l.Analiza);
                ElCerut("CustomerID", l.CustomerID);
                ElCerut("SupplierID", l.SupplierID);
                El("Description", l.Description);
                ElCerut("DebitCreditIndicator", l.DebitCreditIndicator);
                Suma("PaymentLineAmount", l.PaymentLineAmount);
                Taxa(l.TaxInformation);
                Stop();
            }
            // `PaymentMechanism` trăiește AICI, nu pe `Payment` — perechea
            // metodă/mecanism pe care validatorul o impune se scrie în două
            // locuri diferite ale aceleiași plăți.
            if (!string.IsNullOrEmpty(p.PaymentMechanism)) {
                Start("PaymentSettlement");
                ElCerut("PaymentMechanism", p.PaymentMechanism);
                Stop();
            }
            Start("PaymentDocumentTotals");
            ElCerut("GrossTotal", Bani(p.GrossTotal));
            Stop();
            Stop();
        }
        Stop();

        // Modul S: gol în lunar.
        Start("MovementOfGoods");
        Stop();
        Stop(); // SourceDocuments

        Stop(); // AuditFile
        w.WriteEndDocument();
        w.Flush();

        // ── secțiunile compuse ──────────────────────────────────────────────
        void ScrieTerti(string colectie, string element, string numeId, List<SaftTert> lista) {
            Start(colectie);
            foreach (var t in lista) {
                Start(element);
                // `CompanyStructure` (5.6) — identitatea completă a terțului.
                Start("CompanyStructure");
                ElCerut("RegistrationNumber", t.RegistrationNumber);
                ElCerut("Name", t.Name);
                Adresa("Address", t.Address);
                if (!string.IsNullOrEmpty(t.TaxRegistrationNumber)) {
                    Start("TaxRegistration");
                    ElCerut("TaxRegistrationNumber", t.TaxRegistrationNumber);
                    El("TaxType", t.TaxType);
                    Stop();
                }
                Stop();
                ElCerut(numeId, t.Id);
                ElCerut("AccountID", t.AccountID);
                Solduri(t.OpeningDebitBalance, t.OpeningCreditBalance,
                    t.ClosingDebitBalance, t.ClosingCreditBalance);
                Stop();
            }
            Stop();
        }

        void ScrieFacturi(string colectie, List<SaftFactura> lista, bool vanzare) {
            Start(colectie);
            ElInt("NumberOfEntries", lista.Count);
            // Totalurile secțiunii, pe convenția liniilor: vânzarea e pe credit,
            // cumpărarea pe debit (schema le cere, fără să enunțe vreo regulă de
            // egalitate — §C.2).
            ElCerut("TotalDebit", Bani(vanzare ? 0m : lista.Sum(f => f.NetTotal)));
            ElCerut("TotalCredit", Bani(vanzare ? lista.Sum(f => f.NetTotal) : 0m));
            foreach (var f in lista) {
                Start("Invoice");
                ElCerut("InvoiceNo", f.InvoiceNo);
                // `xs:choice` (nota [5]): vânzarea declară clientul, cumpărarea
                // furnizorul; ramura `Name` e interzisă de validator.
                Start(vanzare ? "CustomerInfo" : "SupplierInfo");
                ElCerut(vanzare ? "CustomerID" : "SupplierID", f.PartenerID);
                Adresa("BillingAddress", f.BillingAddress);
                Stop();
                ElCerut("AccountID", f.AccountID);
                ElInt("Period", dto.Luna);
                ElInt("PeriodYear", dto.An);
                ElData("InvoiceDate", f.InvoiceDate);
                ElCerut("InvoiceType", f.InvoiceType);
                ElCerut("SelfBillingIndicator", f.SelfBillingIndicator);
                foreach (var l in f.Linii) {
                    Start("InvoiceLine");
                    ElCerut("LineNumber", l.LineNumber.ToString(Inv));
                    ElCerut("AccountID", l.AccountID);
                    Analize(l.Analiza);
                    El("ProductCode", l.ProductCode);
                    El("ProductDescription", l.ProductDescription);
                    ElCerut("Quantity", Cantitate(l.Quantity));
                    El("InvoiceUOM", l.InvoiceUOM);
                    ElCerut("UnitPrice", Bani(l.UnitPrice));
                    ElData("TaxPointDate", l.TaxPointDate);
                    ElCerut("Description", l.Description);
                    Suma("InvoiceLineAmount", l.InvoiceLineAmount);
                    ElCerut("DebitCreditIndicator", l.DebitCreditIndicator);
                    Taxa(l.TaxInformation);
                    Stop();
                }
                Start("InvoiceDocumentTotals");
                foreach (var t in f.TaxInformationTotals)
                    Taxa(t, "TaxInformationTotals");
                ElCerut("NetTotal", Bani(f.NetTotal));
                ElCerut("GrossTotal", Bani(f.GrossTotal));
                Stop();
                Stop();
            }
            Stop();
        }
    }

    // `SAFmonetaryType`: `fractionDigits 2` — o a treia zecimală ar fi invalidă,
    // deci rotunjirea la fișier e a SCHEMEI, nu o alegere a noastră (banii din
    // model sunt oricum la 2; prețul unitar, care are 6, se rotunjește AICI).
    static string Bani(decimal valoare) =>
        Math.Round(valoare, 2, MidpointRounding.AwayFromZero).ToString("0.00", Inv);

    // `SAFquantityType`: până la 6 zecimale, fără zerouri inutile.
    static string Cantitate(decimal valoare) =>
        Math.Round(valoare, 6, MidpointRounding.AwayFromZero).ToString("0.######", Inv);

    // `TaxPercentage` e `xs:decimal` liber: se scrie cota așa cum e (21, 9,5).
    static string Cota(decimal valoare) => valoare.ToString("0.####", Inv);

    static string Factor(decimal valoare) => valoare.ToString("0.######", Inv);
}
