using System.Globalization;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Saft;

// D406 (SAF-T), modul de raportare **L**, ca PROIECȚIE peste registre — felia 16,
// D16-D4. Al treilea formular pe același tipar (D300 → D394 → D406): formularul e
// al legii, maparea e politică, iar ce cere formularul și modelul nu are se
// RAPORTEAZĂ (avertisment agregat / `Neincluse`), niciodată nu se inventează.
//
// ═══ Sursele, o singură trecere ═══
//   • `RegistruContabil` — GL, soldurile conturilor și ale terților;
//   • `RegistruTva`      — `TaxInformation` pe linii de GL și pe facturi;
//   • nomenclatoarele    — etichete, prin dicționare (LEFT JOIN în memorie: „un
//                          rând nu iese din raport fiindcă i-a murit eticheta");
//   • `Societate` + funcțiile pure din `SaftReguli` — identitățile.
//
// ═══ Ce NU face ═══
// Nu scrie XML (pasul 3), nu persistă declarația (35c), nu paginează (un formular
// nu se paginează), nu rotunjește (bani exacți — 71g), nu filtrează `Storno`
// (registrul e append-only, iar suma lui algebrică e adevărul).
//
// ═══ Partenerul se citește de pe RÂND, nu de pe latură (constatarea 64h) ═══
// Contractul feliei presupunea „partenerul e dimensiunea `Repartitor` A LATURII cu
// contul de terț". Pe modelul de azi asta e FALS, și e fapt măsurat, nu opinie:
// convenția 00 §5 (`RepartitorImplicitDebit` = Predator, `…Credit` = Primitor)
// pune pe fiecare latură CONTRAPARTIDA ei, nu titularul contului. Concret, pe o
// factură de intrare 628 = 401: latura de DEBIT (628, cont fără rol) primește
// FURNIZORUL, iar latura de CREDIT (401, contul de furnizor) primește GESTIUNEA.
// Citit strict pe latură, `Suppliers` ar fi ieșit gol, iar fiecare factură de
// intrare ar fi aterizat în `Neincluse` cu `RepartitorNePartener`.
//
// Regula implementată păstrează intactă partea care e a LEGII (riscul 1: „rolul e
// al CONTULUI, nu al laturii") și o completează cu singura lectură pe care datele
// o susțin: **rolul vine de la contul laturii, partenerul de pe RÂND** — latura
// lui dacă e un `Partener`, altfel cealaltă. Rândul de registru poartă oricum
// ambele capete ale notei, deci nu se ghicește nimic: se citește ce e scris.
// Decizia rămâne deschisă la nivel de MODEL (64h: „dimensiunea Repartitor pune
// contrapartida, nu repartitorul contului"); dacă vreodată convenția se schimbă,
// `PartenerulRandului` de mai jos e singurul loc de atins.
public static class SaftProiectii {

    // Constantele de cod ale antetului (D16-D4).
    public const string SoftwareCompanyName = "Atlas";
    public const string SoftwareID = "Atlas.Conta";
    public const string HeaderComment = "L";
    public const string AuditFileVersion = "2.0";
    public const string AuditFileCountry = "RO";
    public const string DefaultCurrencyCode = "RON";
    public const string MetodaEvaluare = "FIFO";
    // Unitatea de rezervă a nomenclatorului ANAF (exportul legacy o punea pe toate
    // liniile de servicii): „bucată".
    public const string UnitateImplicita = "H87";
    // `ProductCommodityCode` fără cod NC — valoarea pe care exportul legacy a
    // depus-o în producție (riscul 8 al contractului, de măsurat cu DUK în V3).
    public const string CodNcImplicit = "0";
    public const string LocalitateImplicita = "Nespecificat";

    // Tipurile de document care produc facturi/plăți în SAF-T. Codurile sunt ale
    // seed-ului de NUCLEU (`ContaSeeder.SeedTipuriDocument`), identice pe ambele
    // profiluri — ancore de tip, nu simboluri de cont (decizia 29 rămâne întreagă).
    public static readonly string[] TipuriVanzare = ["FCL", "RDC"];
    public static readonly string[] TipuriCumparare = ["FCT", "RLF"];
    public static readonly string[] TipuriRetur = ["RLF", "RDC"];
    public static readonly string[] TipuriPlata = ["PLT", "INC"];
    public const string CodTipInchidereTva = "ITV";
    const string CodTipReturClient = "RDC";
    const string CodTipPlata = "PLT";

    // Rândul de registru contabil, PLAT — tipul pe care îl materializează
    // proiecția (nu entitatea: `IgnoreAutoIncludes` + `Select` = o singură
    // interogare, fără cele 16 navigații de dimensiuni ale registrului, 41c).
    sealed class RandGl {
        public Guid Id { get; set; }
        public DateOnly Data { get; set; }
        public string NumarNota { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? DetaliuId { get; set; }
        public Guid ContDebitId { get; set; }
        public Guid ContCreditId { get; set; }
        public decimal Valoare { get; set; }
        public bool Storno { get; set; }

        public Guid? DebitRepartitorId { get; set; }
        public Guid? DebitCodFunctionalId { get; set; }
        public Guid? DebitCodEconomicId { get; set; }
        public Guid? DebitSursaFinantareId { get; set; }
        public Guid? DebitUnitateId { get; set; }
        public Guid? DebitProiectId { get; set; }
        public Guid? DebitCentruCostId { get; set; }

        public Guid? CreditRepartitorId { get; set; }
        public Guid? CreditCodFunctionalId { get; set; }
        public Guid? CreditCodEconomicId { get; set; }
        public Guid? CreditSursaFinantareId { get; set; }
        public Guid? CreditUnitateId { get; set; }
        public Guid? CreditProiectId { get; set; }
        public Guid? CreditCentruCostId { get; set; }
    }

    // Acumulatorul unui sold de terț: (partener × rol × cont).
    sealed class SoldTert {
        public decimal InitialDebit, InitialCredit, RulajDebit, RulajCredit;
        public decimal Net => InitialDebit - InitialCredit + RulajDebit - RulajCredit;
        public decimal NetInitial => InitialDebit - InitialCredit;
        public decimal Miscare => Math.Abs(RulajDebit) + Math.Abs(RulajCredit);
    }

    // Identitatea fiscală a unui partener, citită PLAT de pe frunză (ca la D394).
    sealed class InfoPartener {
        public Guid Id;
        public string Denumire, CodFiscal, Cod, Tara;
        public TipPersoana TipPersoana;
        public bool InregistratTva, TvaLaIncasare;
        public string Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetCod;
        public string Id406;
        public FelIdSaft Fel;
    }

    // Faptul fiscal al unei linii, în forma în care îl consumă proiecția.
    readonly record struct FaptTva(Guid TipTvaId, SensTva Sens, RegimTva Regim, decimal Cota, decimal Baza, decimal Tva);

    /// <summary>
    /// Declarația D406 (modul L) pe o lună. `dataCreare` există DOAR pentru
    /// determinismul probelor (`AuditFileDateCreated`); în producție e ziua de azi.
    /// </summary>
    public static SaftDto Saft(IObjectSpace os, int an, int luna, DateOnly? dataCreare = null) {
        var rezultat = new SaftDto {
            An = an,
            Luna = luna,
            DataStart = new DateOnly(an, luna, 1),
            DataEnd = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna)),
        };

        // ── 0. Profilul: SAF-T e NEAPLICABIL la bugetar (D16-D5) ─────────────
        // Planul de conturi al instituțiilor publice nu e printre cele 12
        // `TaxAccountingBasis` ale schemei, deci fișierul n-are unde se valida.
        // Refuzul vine ÎNAINTEA oricărei interogări pe registre: un DTO gol cu
        // motiv, nu un fișier gol semnat cu CUI-ul cuiva.
        var setare = os.GetObjectsQuery<SetareProfil>().Select(s => new { s.Profil }).FirstOrDefault();
        if (setare != null && setare.Profil == ProfilContabil.Bugetar) {
            rezultat.Neaplicabil =
                "SAF-T (D406) nu se aplică profilului bugetar: planul de conturi al instituțiilor publice nu e "
                + "printre cele 12 baze contabile (`TaxAccountingBasis`) ale schemei ANAF, deci declarația n-are "
                + "unde se valida.";
            return rezultat;
        }

        var dataStart = rezultat.DataStart;
        var dataEnd = rezultat.DataEnd;

        // Avertismentele se strâng per cauză și se emit AGREGAT la final (fixul 7
        // al review-ului D394): un rând per cod, cu numărul, suma și ≤ 5 exemple.
        var avertismente = new Dictionary<CodAvertismentSaft, List<(string Exemplu, decimal? Suma)>>();
        void Avert(CodAvertismentSaft cod, string exemplu, decimal? suma = null) {
            if (!avertismente.TryGetValue(cod, out var lista))
                lista = avertismente[cod] = [];
            lista.Add((exemplu, suma));
        }
        var neincluse = new List<SaftNeinclus>();

        // `AddressStructure` (5.1) a unui partener. Avertismentul e per PARTENER,
        // nu per apariție: adresa aceluiași client se scrie și în master files, și
        // pe fiecare factură a lui — numărate, cele patru copii ar fi spus „patru
        // adrese incomplete" despre una singură.
        var adreseIncomplete = new HashSet<Guid>();
        SaftAdresa AdresaPartener(InfoPartener p) {
            var taraPartener = Partener.NormalizeazaTara(p.Tara);
            var oras = p.Localitate;
            if (string.IsNullOrWhiteSpace(oras)) {
                oras = LocalitateImplicita;
                if (adreseIncomplete.Add(p.Id))
                    Avert(CodAvertismentSaft.AdresaIncompleta,
                        $"„{p.Denumire}” n-are localitate — `City` e obligatoriu în `AddressStructure`, deci iese "
                        + $"„{LocalitateImplicita}”.");
            }
            return new SaftAdresa {
                StreetName = p.Strada,
                Number = p.Numar,
                AdditionalAddressDetail = p.DetaliiAdresa,
                City = oras,
                PostalCode = p.CodPostal,
                // `Region` NUMAI pe RO (regula validatorului, 72b).
                Region = taraPartener == "RO" ? p.JudetCod : null,
                Country = taraPartener,
            };
        }

        // ── 1. Societatea raportoare (antetul + latura liberă a fiecărei linii) ──
        var soc = os.GetObjectsQuery<Societate>()
            .Select(s => new {
                s.ID, s.Denumire, s.CodFiscal, s.InregistratTva, s.Tara,
                s.Strada, s.Numar, s.DetaliiAdresa, s.Localitate, s.CodPostal,
                JudetCod = s.Judet.Cod,
                s.ContactNume, s.ContactPrenume, s.Telefon, s.Email,
                Iban = s.ContBancar.Iban, ContBancarCod = s.ContBancar.Cod,
                s.BazaContabila, s.RaporteazaCnp
            })
            .FirstOrDefault();

        void LipsaSocietate(string camp) =>
            Avert(CodAvertismentSaft.SocietateIncompleta,
                $"`Societate.{camp}` e gol — fișierul nu trece validarea fără el "
                + "(completați „Configurare → Societate”).");

        if (soc == null) {
            LipsaSocietate("(rândul lipsește)");
        }
        else {
            if (string.IsNullOrWhiteSpace(soc.CodFiscal))
                LipsaSocietate(nameof(Societate.CodFiscal));
            else if (!SaftReguli.CuiValid(soc.CodFiscal))
                Avert(CodAvertismentSaft.SocietateIncompleta,
                    $"`Societate.CodFiscal` („{soc.CodFiscal}”) nu trece cifra de control a CUI-ului — "
                    + "`RegistrationNumber` iese cum e cules, dar validatorul îl va refuza.");
            if (string.IsNullOrWhiteSpace(soc.Denumire)) LipsaSocietate(nameof(Societate.Denumire));
            if (string.IsNullOrWhiteSpace(soc.Localitate)) LipsaSocietate(nameof(Societate.Localitate));
            if (string.IsNullOrWhiteSpace(soc.JudetCod)) LipsaSocietate(nameof(Societate.Judet));
            if (string.IsNullOrWhiteSpace(soc.ContactNume)) LipsaSocietate(nameof(Societate.ContactNume));
            if (string.IsNullOrWhiteSpace(soc.Telefon)) LipsaSocietate(nameof(Societate.Telefon));
            if (string.IsNullOrWhiteSpace(soc.Iban)) LipsaSocietate(nameof(Societate.ContBancar));
        }

        var idSocietate = soc == null ? null : SaftReguli.IdSocietate(soc.CodFiscal, soc.Tara);
        var raporteazaCnp = soc?.RaporteazaCnp ?? false;
        var taraSocietate = Partener.NormalizeazaTara(soc?.Tara);

        rezultat.Header = new SaftHeader {
            AuditFileVersion = AuditFileVersion,
            AuditFileCountry = AuditFileCountry,
            AuditFileRegion = soc?.JudetCod,
            AuditFileDateCreated = dataCreare ?? DateOnly.FromDateTime(DateTime.Today),
            SoftwareCompanyName = SoftwareCompanyName,
            SoftwareID = SoftwareID,
            SoftwareVersion = VersiuneAssembly(),
            DefaultCurrencyCode = DefaultCurrencyCode,
            HeaderComment = HeaderComment,
            SegmentIndex = "1",
            TotalSegmentsInSequence = "1",
            TaxAccountingBasis = soc?.BazaContabila ?? Societate.BazaContabilaImplicita,
            RegistrationNumber = soc == null
                ? null
                : SaftReguli.RegistrationNumberSocietate(soc.CodFiscal, soc.Tara, soc.InregistratTva),
            Name = soc?.Denumire,
            Address = new SaftAdresa {
                StreetName = soc?.Strada,
                Number = soc?.Numar,
                AdditionalAddressDetail = soc?.DetaliiAdresa,
                City = string.IsNullOrWhiteSpace(soc?.Localitate) ? LocalitateImplicita : soc.Localitate,
                PostalCode = soc?.CodPostal,
                Region = taraSocietate == "RO" ? soc?.JudetCod : null,
                Country = taraSocietate,
            },
            ContactFirstName = soc?.ContactPrenume,
            ContactLastName = soc?.ContactNume,
            Telephone = soc?.Telefon,
            Email = soc?.Email,
            IBANNumber = soc?.Iban,
            BankAccountNumber = soc?.Iban ?? soc?.ContBancarCod,
            PeriodStart = luna,
            PeriodStartYear = an,
            PeriodEnd = luna,
            PeriodEndYear = an,
        };

        // ── 2. Nomenclatoarele-etichetă (dicționare, LEFT JOIN în memorie) ───
        var conturi = os.GetObjectsQuery<Cont>()
            .Select(c => new { c.ID, c.Simbol, c.Denumire, c.Functie, c.RolTert })
            .ToList()
            .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire, c.Functie, c.RolTert));
        RolTertCont Rol(Guid contId) =>
            conturi.TryGetValue(contId, out var c) ? c.RolTert : RolTertCont.Niciunul;
        string Simbol(Guid contId) =>
            SaftReguli.SimbolSaft(conturi.TryGetValue(contId, out var c) ? c.Simbol : null);

        var tipuriTva = os.GetObjectsQuery<TipTva>()
            .Select(t => new {
                t.ID, t.Cod, t.Denumire, t.Cota, t.Regim,
                t.CodSafTLivrare, t.CodSafTAchizitie,
                t.ContTvaDeductibilId, t.ContTvaColectatId, t.ContTvaNeexigibilId
            })
            .ToList();
        var tipTvaDupaId = tipuriTva.ToDictionary(t => t.ID);
        var conturiTva = tipuriTva
            .SelectMany(t => new[] { t.ContTvaDeductibilId, t.ContTvaColectatId, t.ContTvaNeexigibilId })
            .Where(id => id != null).Select(id => id.Value).ToHashSet();

        var tipuriDocument = os.GetObjectsQuery<TipDocument>()
            .Select(t => new { t.Cod, t.Denumire })
            .ToList()
            .GroupBy(t => t.Cod, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Denumire, StringComparer.Ordinal);

        var tipuriMaterial = os.GetObjectsQuery<TipMaterial>()
            .Select(t => new { t.ID, t.Denumire })
            .ToList()
            .ToDictionary(t => t.ID, t => t.Denumire);

        // ── 3. Soldurile conturilor (GeneralLedgerAccounts) ──────────────────
        // Aceeași agregare ca balanța de verificare (BP-D2: agregarea frunzelor NU
        // se rescrie) — o a doua ar diverge tăcut de prima.
        var balanta = ContabilProiectii.Balanta(os, dataStart, dataEnd, analitic: false).ToList();
        foreach (var b in balanta
                     .OrderBy(x => x.ContSimbol ?? "", StringComparer.Ordinal).ThenBy(x => x.ContId)) {
            var deschidere = b.InitialDebit - b.InitialCredit;
            var inchidere = deschidere + b.RulajDebit - b.RulajCredit;
            if (deschidere == 0m && inchidere == 0m && b.RulajDebit == 0m && b.RulajCredit == 0m)
                continue;
            conturi.TryGetValue(b.ContId, out var info);
            if (!SaftReguli.FunctieCunoscuta(info.Functie))
                Avert(CodAvertismentSaft.TipContNecunoscut,
                    $"Contul {info.Simbol ?? b.ContSimbol ?? b.ContId.ToString()} are funcția "
                    + $"„{info.Functie ?? "(goală)"}” — `AccountType` iese „Bifunctional”.");
            rezultat.Conturi.Add(new SaftCont {
                ContId = b.ContId,
                AccountID = SaftReguli.SimbolSaft(info.Simbol ?? b.ContSimbol),
                AccountDescription = info.Denumire ?? b.ContDenumire,
                AccountType = SaftReguli.TipCont(info.Functie),
                // `xs:choice` (nota [1]): debit XOR credit; 0 se declară pe debit.
                OpeningDebitBalance = deschidere >= 0m ? deschidere : null,
                OpeningCreditBalance = deschidere < 0m ? -deschidere : null,
                ClosingDebitBalance = inchidere >= 0m ? inchidere : null,
                ClosingCreditBalance = inchidere < 0m ? -inchidere : null,
            });
        }

        // ── 4. Rândurile de registru ale perioadei (GL) ──────────────────────
        // `DocumentId != null`: rândurile de DESCHIDERE (25e/34d) sunt solduri, nu
        // tranzacții — ele intră în `Opening*`, nu în `GeneralLedgerEntries`.
        var randuri = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd && r.DocumentId != null)
            .Select(r => new RandGl {
                Id = r.ID, Data = r.Data, NumarNota = r.NumarNota,
                DocumentId = r.DocumentId.Value, DetaliuId = r.DetaliuId,
                ContDebitId = r.ContDebitId, ContCreditId = r.ContCreditId,
                Valoare = r.Valoare, Storno = r.Storno,
                DebitRepartitorId = r.DebitRepartitorId, DebitCodFunctionalId = r.DebitCodFunctionalId,
                DebitCodEconomicId = r.DebitCodEconomicId, DebitSursaFinantareId = r.DebitSursaFinantareId,
                DebitUnitateId = r.DebitUnitateId, DebitProiectId = r.DebitProiectId,
                DebitCentruCostId = r.DebitCentruCostId,
                CreditRepartitorId = r.CreditRepartitorId, CreditCodFunctionalId = r.CreditCodFunctionalId,
                CreditCodEconomicId = r.CreditCodEconomicId, CreditSursaFinantareId = r.CreditSursaFinantareId,
                CreditUnitateId = r.CreditUnitateId, CreditProiectId = r.CreditProiectId,
                CreditCentruCostId = r.CreditCentruCostId,
            })
            .ToList();

        // ── 5. Rândurile fiscale ale perioadei ───────────────────────────────
        var randuriTva = os.GetObjectsQuery<RegistruTva>()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd)
            .Select(r => new {
                r.ID, r.DocumentId, r.DetaliuId, r.Sens, r.TipTvaId, r.Regim, r.Cota, r.Baza, r.Tva, r.Storno
            })
            .ToList();
        // Cheia (Detaliu × Storno): motorul scrie un rând per linie și per
        // meta-operație (stornoul e o a doua trecere peste aceleași linii).
        var tvaPeDetaliu = new Dictionary<(Guid, bool), FaptTva>();
        foreach (var t in randuriTva)
            tvaPeDetaliu.TryAdd((t.DetaliuId, t.Storno),
                new FaptTva(t.TipTvaId, t.Sens, t.Regim, t.Cota, t.Baza, t.Tva));

        // Mulțimea documentelor: rândurile contabile ALE PERIOADEI, plus cele
        // fiscale — un document care n-a postat nimic contabil (toate liniile fără
        // regulă) tot are fapte fiscale, iar cusătura le cere numărate.
        var idsDocumente = randuri.Select(r => r.DocumentId)
            .Concat(randuriTva.Select(t => t.DocumentId))
            .Distinct().ToList();
        // Tipul documentului POLIMORF, într-un singur query (60b) — sub TPT nu
        // există discriminator, iar ancora se caută după numele clasei CLR.
        var codPerDocument = ApiProiectii.CoduriTip(os, idsDocumente);
        string CodTip(Guid documentId) => codPerDocument.GetValueOrDefault(documentId);

        var documente = os.GetObjectsQuery<Document>()
            .Where(d => idsDocumente.Contains(d.ID))
            .Select(d => new { d.ID, d.Numar, d.Data, d.DataOperare, d.PredatorId, d.PrimitorId })
            .ToList()
            .ToDictionary(d => d.ID);

        // ── 6. Repartitorii care apar oriunde (parteneri + restul) ───────────
        var conturiCuRol = conturi.Where(c => c.Value.RolTert != RolTertCont.Niciunul)
            .Select(c => c.Key).ToList();

        // Agregatul soldurilor de terți: PROPRIU, nu `Balanta(analitic: true)`.
        // Motivul e în antetul clasei — cheia balanței analitice e (cont ×
        // dimensiunea ACELEIAȘI laturi), iar partenerul stă pe latura cealaltă. O
        // singură interogare grupată pe perechile de conturi × perechile de
        // repartitori: cardinalitatea e a nomenclatoarelor, nu a registrului.
        var agregateTert = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes()
            .Where(r => r.Data <= dataEnd
                && (conturiCuRol.Contains(r.ContDebitId) || conturiCuRol.Contains(r.ContCreditId)))
            .GroupBy(r => new { r.ContDebitId, r.ContCreditId, r.DebitRepartitorId, r.CreditRepartitorId })
            .Select(g => new {
                g.Key.ContDebitId, g.Key.ContCreditId, g.Key.DebitRepartitorId, g.Key.CreditRepartitorId,
                Initial = g.Sum(r => r.Data < dataStart ? r.Valoare : 0m),
                Rulaj = g.Sum(r => r.Data >= dataStart ? r.Valoare : 0m),
            })
            .ToList();

        var idsRepartitor = new HashSet<Guid>();
        void AdaugaRep(Guid? id) { if (id is Guid v) idsRepartitor.Add(v); }
        foreach (var r in randuri) {
            AdaugaRep(r.DebitRepartitorId);
            AdaugaRep(r.CreditRepartitorId);
            AdaugaRep(r.DebitCentruCostId);
            AdaugaRep(r.CreditCentruCostId);
        }
        foreach (var a in agregateTert) {
            AdaugaRep(a.DebitRepartitorId);
            AdaugaRep(a.CreditRepartitorId);
        }
        foreach (var d in documente.Values) {
            idsRepartitor.Add(d.PredatorId);
            idsRepartitor.Add(d.PrimitorId);
        }
        var listaRep = idsRepartitor.ToList();

        // `IgnoreQueryFilters` (aceeași lecție ca la D394): partenerul ȘTERS logic
        // din nomenclator se DECLARĂ — documentele lui sunt operate, iar fișierul
        // nu depinde de viața nomenclatorului.
        var parteneri = os.GetObjectsQuery<Partener>().IgnoreQueryFilters()
            .Where(p => listaRep.Contains(p.ID))
            .Select(p => new {
                p.ID, p.Cod, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare,
                p.Strada, p.Numar, p.DetaliiAdresa, p.Localitate, p.CodPostal, JudetCod = p.Judet.Cod
            })
            .ToList()
            .ToDictionary(p => p.ID, p => new InfoPartener {
                Id = p.ID, Cod = p.Cod, Denumire = p.Denumire, CodFiscal = p.CodFiscal,
                TipPersoana = p.TipPersoana, Tara = p.Tara,
                InregistratTva = p.InregistratTva, TvaLaIncasare = p.TvaLaIncasare,
                Strada = p.Strada, Numar = p.Numar, DetaliiAdresa = p.DetaliiAdresa,
                Localitate = p.Localitate, CodPostal = p.CodPostal, JudetCod = p.JudetCod,
            });
        foreach (var p in parteneri.Values) {
            var identitate = SaftReguli.IdPartener(
                p.TipPersoana, p.Tara, p.InregistratTva, p.CodFiscal, p.Cod, p.Id, raporteazaCnp);
            p.Id406 = identitate.Id;
            p.Fel = identitate.Fel;
        }

        var repartitori = os.GetObjectsQuery<Repartitor>().IgnoreQueryFilters()
            .Where(r => listaRep.Contains(r.ID))
            .Select(r => new { r.ID, r.Cod, r.Denumire })
            .ToList()
            .ToDictionary(r => r.ID, r => (r.Cod, r.Denumire));
        var idsAngajat = os.GetObjectsQuery<Angajat>().IgnoreQueryFilters()
            .Where(a => listaRep.Contains(a.ID)).Select(a => a.ID).ToList().ToHashSet();
        var idsContPropriu = os.GetObjectsQuery<ContPropriu>().IgnoreQueryFilters()
            .Where(c => listaRep.Contains(c.ID)).Select(c => c.ID).ToList().ToHashSet();

        string DenumireRep(Guid? id) =>
            id is Guid v && repartitori.TryGetValue(v, out var r) ? r.Denumire : null;

        // Partenerul unui RÂND (vezi antetul clasei): latura proprie dacă e
        // `Partener`, altfel cealaltă. `null` = rândul n-are niciun partener pe el.
        (Guid? Id, bool ExistaRepartitor) PartenerulRandului(Guid? repLatura, Guid? repCealalta) {
            if (repLatura is Guid a && parteneri.ContainsKey(a))
                return (a, true);
            if (repCealalta is Guid b && parteneri.ContainsKey(b))
                return (b, true);
            return (null, repLatura != null || repCealalta != null);
        }

        // ── 7. Customers / Suppliers ─────────────────────────────────────────
        var solduri = new Dictionary<(Guid Partener, RolTertCont Rol, Guid Cont), SoldTert>();
        var neincluseTert = new Dictionary<(CauzaNeincludere, Guid, Guid?), SaftNeinclus>();

        void AcumuleazaTert(Guid contId, RolTertCont rol, Guid? repLatura, Guid? repCealalta,
            decimal initial, decimal rulaj, bool debit) {
            var (partenerId, existaRep) = PartenerulRandului(repLatura, repCealalta);
            if (partenerId == null) {
                var cauza = existaRep ? CauzaNeincludere.RepartitorNePartener : CauzaNeincludere.FaraPartener;
                var repId = repLatura ?? repCealalta;
                var cheieN = (cauza, contId, repId);
                if (!neincluseTert.TryGetValue(cheieN, out var n))
                    n = neincluseTert[cheieN] = new SaftNeinclus {
                        Cauza = cauza.ToString(),
                        Sectiune = rol == RolTertCont.Client ? "Customers" : "Suppliers",
                        ContId = contId,
                        ContSimbol = conturi.TryGetValue(contId, out var ci) ? ci.Simbol : null,
                        RepartitorId = repId,
                        RepartitorDenumire = DenumireRep(repId),
                        Debit = 0m, Credit = 0m,
                    };
                if (debit) n.Debit += initial + rulaj;
                else n.Credit += initial + rulaj;
                n.Randuri++;
                return;
            }
            var cheie = (partenerId.Value, rol, contId);
            if (!solduri.TryGetValue(cheie, out var sold))
                sold = solduri[cheie] = new SoldTert();
            if (debit) { sold.InitialDebit += initial; sold.RulajDebit += rulaj; }
            else { sold.InitialCredit += initial; sold.RulajCredit += rulaj; }
        }

        foreach (var a in agregateTert) {
            var rolD = Rol(a.ContDebitId);
            if (rolD != RolTertCont.Niciunul)
                AcumuleazaTert(a.ContDebitId, rolD, a.DebitRepartitorId, a.CreditRepartitorId,
                    a.Initial, a.Rulaj, debit: true);
            var rolC = Rol(a.ContCreditId);
            if (rolC != RolTertCont.Niciunul)
                AcumuleazaTert(a.ContCreditId, rolC, a.CreditRepartitorId, a.DebitRepartitorId,
                    a.Initial, a.Rulaj, debit: false);
        }
        neincluse.AddRange(neincluseTert.Values.Where(n => n.Debit != 0m || n.Credit != 0m));

        // Un rând per (partener × rol); conturile rolului se însumează, iar
        // `AccountID` e cel cu cea mai mare mișcare (contract D16-D4).
        var terti = new Dictionary<(RolTertCont Rol, string Id), SaftTert>();
        foreach (var g in solduri.GroupBy(s => (s.Key.Partener, s.Key.Rol))
                     .OrderBy(g => g.Key.Rol).ThenBy(g => g.Key.Partener)) {
            var p = parteneri[g.Key.Partener];
            var deschidere = g.Sum(x => x.Value.NetInitial);
            var inchidere = g.Sum(x => x.Value.Net);
            var miscare = g.Sum(x => x.Value.Miscare);
            if (deschidere == 0m && inchidere == 0m && miscare == 0m)
                continue;
            var contPrincipal = g
                .OrderByDescending(x => x.Value.Miscare)
                .ThenByDescending(x => Math.Abs(x.Value.Net))
                .ThenBy(x => conturi.TryGetValue(x.Key.Cont, out var c) ? c.Simbol : "", StringComparer.Ordinal)
                .First().Key.Cont;

            var tert = new SaftTert {
                PartenerId = p.Id,
                Id = p.Id406,
                RegistrationNumber = p.Id406,
                Name = p.Denumire,
                Address = AdresaPartener(p),
                TaxRegistrationNumber = p.Fel == FelIdSaft.CuiRoman ? p.Id406[2..] : null,
                TaxType = SaftReguli.RegimFiscalPartener(p.Fel, p.InregistratTva, p.TvaLaIncasare),
                AccountID = Simbol(contPrincipal),
                OpeningDebitBalance = deschidere >= 0m ? deschidere : null,
                OpeningCreditBalance = deschidere < 0m ? -deschidere : null,
                ClosingDebitBalance = inchidere >= 0m ? inchidere : null,
                ClosingCreditBalance = inchidere < 0m ? -inchidere : null,
                FelId = p.Fel.ToString(),
            };
            // Riscul 2 al contractului: același identificator pe două nomenclatoare
            // ⇒ cheie duplicată în master files. Se păstrează O intrare, cu
            // soldurile CUMULATE (altfel fișierul ar pierde cifre), și se strigă.
            var cheie = (g.Key.Rol, tert.Id);
            if (terti.TryGetValue(cheie, out var existent)) {
                Avert(CodAvertismentSaft.PartenerDublat,
                    $"{(g.Key.Rol == RolTertCont.Client ? "Clientul" : "Furnizorul")} „{p.Denumire}” are același "
                    + $"identificator SAF-T ({tert.Id}) ca „{existent.Name}” — se declară o singură intrare, cu "
                    + "soldurile cumulate.");
                CumuleazaSolduri(existent, tert);
                continue;
            }
            terti[cheie] = tert;
            if (p.Fel == FelIdSaft.CodIntern
                    && (p.InregistratTva || Partener.NormalizeazaTara(p.Tara) == "RO"))
                Avert(CodAvertismentSaft.PartenerFaraCuiValid,
                    $"„{p.Denumire}” e român sau înregistrat în scopuri de TVA, dar codul fiscal "
                    + $"(„{p.CodFiscal ?? "gol"}”) nu trece cifra de control — se declară cu prefixul 04 "
                    + "(cod intern), nu 00.");
        }
        // Partenerii REFERIȚI de facturi și plăți, strânși pe parcurs: master files
        // trebuie să-i declare pe toți, chiar dacă soldul lor e zero (vezi
        // `AsiguraTert` de mai jos). Listele se așază abia după secțiunile
        // documentelor, din același motiv.
        var tertiReferiti = new List<(RolTertCont Rol, Guid PartenerId, string AccountID)>();

        // ── 8. Etichetele dimensiunilor (AnalysisTypeTable + Analysis) ───────
        var dimensiuni = new Dictionary<string, Dictionary<Guid, (string Cod, string Denumire)>>(StringComparer.Ordinal) {
            ["CF"] = os.GetObjectsQuery<CodFunctional>().Select(x => new { x.ID, x.Cod, x.Denumire })
                .ToList().ToDictionary(x => x.ID, x => (x.Cod, x.Denumire)),
            ["CE"] = os.GetObjectsQuery<CodEconomic>().Select(x => new { x.ID, x.Cod, x.Denumire })
                .ToList().ToDictionary(x => x.ID, x => (x.Cod, x.Denumire)),
            ["SF"] = os.GetObjectsQuery<SursaFinantare>().Select(x => new { x.ID, x.Cod, x.Denumire })
                .ToList().ToDictionary(x => x.ID, x => (x.Cod, x.Denumire)),
            ["U"] = os.GetObjectsQuery<Unitate>().Select(x => new { x.ID, x.Cod, x.Denumire })
                .ToList().ToDictionary(x => x.ID, x => (x.Cod, x.Denumire)),
            ["P"] = os.GetObjectsQuery<Proiect>().Select(x => new { x.ID, x.Cod, x.Denumire })
                .ToList().ToDictionary(x => x.ID, x => (x.Cod, x.Denumire)),
            // Centrul de cost e o CALITATE de repartitor (decizia 16), nu un
            // nomenclator propriu — etichetele vin din același dicționar.
            ["CC"] = repartitori.ToDictionary(x => x.Key, x => (x.Value.Cod, x.Value.Denumire)),
        };
        var descriereTip = new Dictionary<string, string>(StringComparer.Ordinal) {
            ["CF"] = "Cod funcțional", ["CE"] = "Cod economic", ["SF"] = "Sursă de finanțare",
            ["U"] = "Unitate", ["P"] = "Proiect", ["CC"] = "Centru de cost",
        };
        var tipuriAnalizaFolosite = new Dictionary<(string, string), SaftTipAnaliza>();

        List<SaftAnaliza> Analiza(bool debit, RandGl r) {
            var lista = new List<SaftAnaliza>();
            void Adauga(string tip, Guid? id) {
                if (id is not Guid v)
                    return;
                var etichete = dimensiuni[tip];
                etichete.TryGetValue(v, out var eticheta);
                var cod = string.IsNullOrWhiteSpace(eticheta.Cod) ? v.ToString("N") : eticheta.Cod;
                lista.Add(new SaftAnaliza { AnalysisType = tip, AnalysisID = cod });
                tipuriAnalizaFolosite.TryAdd((tip, cod), new SaftTipAnaliza {
                    AnalysisType = tip, AnalysisTypeDescription = descriereTip[tip],
                    AnalysisID = cod, AnalysisIDDescription = eticheta.Denumire ?? cod,
                });
            }
            Adauga("CC", debit ? r.DebitCentruCostId : r.CreditCentruCostId);
            Adauga("P", debit ? r.DebitProiectId : r.CreditProiectId);
            Adauga("U", debit ? r.DebitUnitateId : r.CreditUnitateId);
            Adauga("SF", debit ? r.DebitSursaFinantareId : r.CreditSursaFinantareId);
            Adauga("CF", debit ? r.DebitCodFunctionalId : r.CreditCodFunctionalId);
            Adauga("CE", debit ? r.DebitCodEconomicId : r.CreditCodEconomicId);
            return lista;
        }

        // ── 9. Descrierile liniilor-sursă (un query per FRUNZĂ, nu per rând) ──
        var idsDetaliu = randuri.Where(r => r.DetaliuId != null)
            .Select(r => r.DetaliuId.Value).Distinct().ToList();
        var descrieri = new Dictionary<Guid, string>();
        foreach (var x in os.GetObjectsQuery<NotaContabilaDetaliu>()
                     .Where(d => idsDetaliu.Contains(d.ID)).Select(d => new { d.ID, d.Descriere }).ToList())
            if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;
        foreach (var x in os.GetObjectsQuery<DecontDetaliu>()
                     .Where(d => idsDetaliu.Contains(d.ID)).Select(d => new { d.ID, d.Descriere }).ToList())
            if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;
        foreach (var x in os.GetObjectsQuery<FacturaIesireDetaliu>()
                     .Where(d => idsDetaliu.Contains(d.ID)).Select(d => new { d.ID, d.Descriere }).ToList())
            if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;

        // ── 10. `TaxInformation` pe rândul de GL ─────────────────────────────
        var codTvaFolosit = new Dictionary<string, (decimal Cota, string Denumire)>(StringComparer.Ordinal);
        var tvaFaraCod = new HashSet<Guid>();
        var tvaGl = 0m;
        var tvaFaraCodSaft = 0m;

        SaftTaxInfo Nefiscal() => new() {
            TaxType = SaftReguli.TaxTypeNefiscal, TaxCode = SaftReguli.TaxCodeNefiscal, TaxAmount = 0m
        };

        SaftTaxInfo TaxaRandului(RandGl r, string codTipDocument) {
            // ITV (riscul 4): rândurile închiderii lunare ating 4426/4427 fără să
            // fie operațiuni taxabile (n-au rând în `RegistruTva`) — codul lor e
            // `TVA_NoteContabile`, identificat prin TIPUL documentului, nu prin
            // simbol (decizia 29: niciun simbol de cont în cod).
            if (codTipDocument == CodTipInchidereTva) {
                codTvaFolosit.TryAdd(SaftReguli.TaxCodeInchidereTva,
                    (0m, "TVA — note contabile (închiderea lunară)"));
                return new SaftTaxInfo {
                    TaxType = SaftReguli.TaxTypeTva, TaxCode = SaftReguli.TaxCodeInchidereTva, TaxAmount = 0m
                };
            }
            if (r.DetaliuId is Guid det && tvaPeDetaliu.TryGetValue((det, r.Storno), out var fapt)
                    && tipTvaDupaId.TryGetValue(fapt.TipTvaId, out var tip)) {
                var atingeExigibil =
                    tip.ContTvaDeductibilId == r.ContDebitId || tip.ContTvaDeductibilId == r.ContCreditId
                    || tip.ContTvaColectatId == r.ContDebitId || tip.ContTvaColectatId == r.ContCreditId;
                // Riscul 3, DECIS: `ContTvaNeexigibil` (4428) NU e taxă exigibilă —
                // e poziția de așteptare a mecanismului „TVA la încasare" (36f,
                // rezervat). Un cod de taxă pe ea ar declara exigibilă o sumă care
                // nu e datorată încă, deci rândul rămâne `000/000000`.
                if (atingeExigibil) {
                    var cod = fapt.Sens == SensTva.Livrare ? tip.CodSafTLivrare : tip.CodSafTAchizitie;
                    if (string.IsNullOrWhiteSpace(cod)) {
                        // Cifra nu se pierde: rândul iese `000/000000`, dar taxa lui
                        // se numără separat în rezumat (cusătura de TVA).
                        tvaFaraCodSaft += fapt.Tva;
                        if (tvaFaraCod.Add(tip.ID))
                            Avert(CodAvertismentSaft.TipTvaFaraCodSaft,
                                $"Tipul de TVA „{tip.Cod}” ({tip.Denumire}) n-are cod SAF-T pe "
                                + $"{(fapt.Sens == SensTva.Livrare ? "livrare" : "achiziție")} — rândurile lui ies "
                                + "cu `000/000000`.", fapt.Tva);
                        return Nefiscal();
                    }
                    tvaGl += fapt.Tva;
                    codTvaFolosit.TryAdd(cod, (tip.Cota, tip.Denumire));
                    return new SaftTaxInfo {
                        TaxType = SaftReguli.TaxTypeTva, TaxCode = cod,
                        TaxPercentage = fapt.Cota, TaxBase = fapt.Baza, TaxAmount = fapt.Tva
                    };
                }
            }
            return Nefiscal();
        }

        // ── 11. GeneralLedgerEntries ─────────────────────────────────────────
        var tertFaraPartener = new HashSet<Guid>();
        (string Customer, string Supplier) IdentitatiLatura(RandGl r, bool debit) {
            var contId = debit ? r.ContDebitId : r.ContCreditId;
            var rol = Rol(contId);
            if (rol == RolTertCont.Niciunul)
                return (idSocietate, idSocietate);
            var (partenerId, _) = PartenerulRandului(
                debit ? r.DebitRepartitorId : r.CreditRepartitorId,
                debit ? r.CreditRepartitorId : r.DebitRepartitorId);
            if (partenerId is not Guid pid) {
                if (tertFaraPartener.Add(r.Id))
                    Avert(CodAvertismentSaft.TertFaraPartener,
                        $"Rândul de registru din {r.Data:dd.MM.yyyy} pe contul "
                        + $"{(conturi.TryGetValue(contId, out var c) ? c.Simbol : contId.ToString())} n-are niciun "
                        + "partener pe laturi — `CustomerID`/`SupplierID` ies cu codul societății.", r.Valoare);
                return (idSocietate, idSocietate);
            }
            var idP = parteneri[pid].Id406;
            return rol == RolTertCont.Client ? (idP, idSocietate) : (idSocietate, idP);
        }

        var randuriPeDocument = randuri.GroupBy(r => r.DocumentId).ToDictionary(g => g.Key, g => g.ToList());
        var randuriPeId = randuri.ToDictionary(r => r.Id);
        var jurnale = new Dictionary<string, SaftJurnal>(StringComparer.Ordinal);
        decimal totalDebit = 0m, totalCredit = 0m;
        var numarLinii = 0;

        foreach (var docId in randuriPeDocument.Keys
                     .OrderBy(id => documente.TryGetValue(id, out var d) ? d.Data : DateOnly.MinValue)
                     .ThenBy(id => documente.TryGetValue(id, out var d) ? d.Numar ?? "" : "", StringComparer.Ordinal)
                     .ThenBy(id => id)) {
            var doc = documente.GetValueOrDefault(docId);
            var cod = CodTip(docId) ?? "?";
            if (!jurnale.TryGetValue(cod, out var jurnal))
                jurnal = jurnale[cod] = new SaftJurnal {
                    JournalID = cod, Type = cod,
                    Description = tipuriDocument.GetValueOrDefault(cod) ?? cod,
                };

            var randuriDoc = randuriPeDocument[docId];
            var descriereDoc = $"{tipuriDocument.GetValueOrDefault(cod) ?? cod} {doc?.Numar}".Trim();
            var tranzactie = new SaftTranzactie {
                DocumentId = docId,
                TransactionID = docId.ToString(),
                Period = luna,
                PeriodYear = an,
                TransactionDate = doc?.Data ?? dataStart,
                Description = descriereDoc,
                SystemEntryDate = doc?.DataOperare is DateTime dt
                    ? DateOnly.FromDateTime(dt) : doc?.Data ?? dataStart,
                GLPostingDate = doc?.Data ?? dataStart,
                CustomerID = idSocietate,
                SupplierID = idSocietate,
            };
            // La nivel de TRANZACȚIE: dacă documentul are UN singur partener pe
            // laturi, el; altfel societatea pe amândouă (GL.19/GL.20 COM).
            if (PartenerulDocumentului(doc?.PredatorId, doc?.PrimitorId, parteneri) is Guid pdoc) {
                var rolDoc = RolulDocumentului(randuriDoc, Rol);
                if (rolDoc == RolTertCont.Client)
                    tranzactie.CustomerID = parteneri[pdoc].Id406;
                else if (rolDoc == RolTertCont.Furnizor)
                    tranzactie.SupplierID = parteneri[pdoc].Id406;
            }

            var pozitie = 0;
            foreach (var r in randuriDoc
                         .OrderBy(x => x.Data)
                         .ThenBy(x => x.NumarNota ?? "", StringComparer.Ordinal)
                         .ThenBy(x => x.Id)) {
                var taxa = TaxaRandului(r, cod);
                var descriereLinie = r.DetaliuId is Guid det ? descrieri.GetValueOrDefault(det) : null;
                foreach (var debit in new[] { true, false }) {
                    var (customer, supplier) = IdentitatiLatura(r, debit);
                    pozitie++;
                    tranzactie.Linii.Add(new SaftLinieTranzactie {
                        RandRegistruId = r.Id,
                        DetaliuId = r.DetaliuId,
                        RecordID = pozitie.ToString(CultureInfo.InvariantCulture),
                        AccountID = Simbol(debit ? r.ContDebitId : r.ContCreditId),
                        CustomerID = customer,
                        SupplierID = supplier,
                        Description = descriereLinie ?? descriereDoc,
                        DebitCreditIndicator = debit ? "D" : "C",
                        Amount = r.Valoare,
                        CurrencyCode = DefaultCurrencyCode,
                        CurrencyAmount = r.Valoare,
                        Analiza = Analiza(debit, r),
                        // `TaxInformation` e obligatoriu pe FIECARE linie (S.TI.1):
                        // aceeași informație pe ambele laturi ale notei — cusătura de
                        // TVA se măsoară o dată per RÂND, nu per linie (vezi `tvaGl`).
                        TaxInformation = new SaftTaxInfo {
                            TaxType = taxa.TaxType, TaxCode = taxa.TaxCode,
                            TaxPercentage = taxa.TaxPercentage, TaxBase = taxa.TaxBase,
                            TaxAmount = taxa.TaxAmount
                        },
                    });
                    numarLinii++;
                }
                totalDebit += r.Valoare;
                totalCredit += r.Valoare;
            }
            jurnal.Tranzactii.Add(tranzactie);
        }
        rezultat.Jurnale = jurnale.Values.OrderBy(j => j.JournalID, StringComparer.Ordinal).ToList();

        // ── 12. Liniile documentelor de factură și de plată ──────────────────
        var idsFacturiVanzare = idsDocumente.Where(id => TipuriVanzare.Contains(CodTip(id))).ToList();
        var idsFacturiCumparare = idsDocumente.Where(id => TipuriCumparare.Contains(CodTip(id))).ToList();
        var idsPlati = idsDocumente.Where(id => TipuriPlata.Contains(CodTip(id))).ToList();
        var idsCuLinii = idsFacturiVanzare.Concat(idsFacturiCumparare).Concat(idsPlati).ToList();

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => idsCuLinii.Contains(d.DocumentId))
            .Select(d => new {
                d.ID, d.DocumentId, d.TipMaterialId, d.LotId, d.Cantitate, d.Valoare, d.TipTvaId, d.ValoareTva
            })
            .ToList();
        var liniiPeDocument = linii.GroupBy(l => l.DocumentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.ID).ToList());
        var idsLinie = linii.Select(l => l.ID).ToList();

        var pretUnitar = new Dictionary<Guid, decimal>();
        var produsPeLinie = new Dictionary<Guid, Guid>();
        foreach (var x in os.GetObjectsQuery<FacturaIntrareDetaliu>().Where(d => idsLinie.Contains(d.ID))
                     .Select(d => new { d.ID, d.PretUnitar, d.ProdusId }).ToList()) {
            pretUnitar[x.ID] = x.PretUnitar;
            if (x.ProdusId is Guid p) produsPeLinie[x.ID] = p;
        }
        foreach (var x in os.GetObjectsQuery<FacturaIesireDetaliu>().Where(d => idsLinie.Contains(d.ID))
                     .Select(d => new { d.ID, d.PretUnitar, d.ProdusId }).ToList()) {
            pretUnitar[x.ID] = x.PretUnitar;
            if (x.ProdusId is Guid p) produsPeLinie[x.ID] = p;
        }
        var idsLot = linii.Where(l => l.LotId != null).Select(l => l.LotId.Value).Distinct().ToList();
        var produsPeLot = os.GetObjectsQuery<Lot>().IgnoreQueryFilters()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId }).ToList()
            .ToDictionary(l => l.ID, l => l.ProdusId);

        // Valuta facturii de intrare — multi-valuta e explicit în afara feliei
        // (34g deschis), deci e AVERTISMENT, nu conversie.
        var valutaFct = os.GetObjectsQuery<FacturaIntrare>()
            .Where(f => idsFacturiCumparare.Contains(f.ID))
            .Select(f => new { f.ID, f.Valuta }).ToList()
            .ToDictionary(f => f.ID, f => f.Valuta);

        // Contrapartida liniilor de STOC ale facturii de intrare, prin NIR-ul
        // CONEX (amendamentul D16-D4). Recepția contează pe NIR (26a), deci linia
        // facturii n-are rând contabil propriu — dar are o urmă MATERIALIZATĂ:
        // linia naște lotul (`Lot.LinieIntrareId`), NIR-ul conex îl recepționează
        // (`RegistruStoc.LotId` → `DetaliuId` al liniei de NIR), iar rândul
        // contabil al acelei linii poartă contul de stoc pe DEBIT. Contul NU se
        // inventează (nici din `TipMaterial.ContImplicit`): se citește din ce s-a
        // scris efectiv. Fără rând de recepție pe lot ⇒ rămâne `Neincluse`.
        //
        // Pe SETURI, nu per linie: trei interogări (loturile născute de liniile
        // lunii, recepțiile lor, rândurile contabile ale liniilor de recepție).
        //
        // `Nullable.Value` NU se dereferențiază într-o proiecție pe tip ANONIM
        // peste o ușă securizată (măsurat, V4): pentru un utilizator FĂRĂ drept de
        // citire pe tip, compilatorul de securitate DevExpress rescrie arborele,
        // iar funcletizer-ul EF ajunge să evalueze argumentele lui `new { … }` pe
        // rând — `LinieIntrareId.Value` pe un `null` a ieșit
        // `InvalidOperationException: Nullable object must have a value`, adică
        // 500 acolo unde contractul cere 200 cu liste goale. Gardul `!= null` din
        // `Where` nu ajută: el se evaluează cu scurt-circuit, argumentele lui
        // `new` nu. Deci nullable-ul trece ca atare, iar despachetarea se face în
        // memorie, după `ToList()`.
        var loturiNascute = os.GetObjectsQuery<Lot>().IgnoreQueryFilters()
            .Where(l => l.LinieIntrareId != null && idsLinie.Contains(l.LinieIntrareId.Value))
            .Select(l => new { l.ID, l.LinieIntrareId })
            .ToList()
            .Select(l => new { l.ID, LinieId = l.LinieIntrareId.Value })
            .ToList();
        var idsLotNascut = loturiNascute.Select(l => l.ID).ToList();
        // Cantitate POZITIVĂ și nestornată = recepție; ieșirile aceluiași lot
        // (DSC, RLF, BCS) poartă contul de descărcare, nu pe cel de stoc.
        var receptii = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => idsLotNascut.Contains(r.LotId) && r.DetaliuId != null
                && r.Cantitate > 0m && !r.Storno)
            .Select(r => new { r.LotId, r.DetaliuId, r.Data })
            .ToList()
            .Select(r => new { r.LotId, DetaliuId = r.DetaliuId.Value, r.Data })
            .ToList();
        var idsDetaliuReceptie = receptii.Select(r => r.DetaliuId).Distinct().ToList();
        var randuriReceptie = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes()
            .Where(r => r.DetaliuId != null && idsDetaliuReceptie.Contains(r.DetaliuId.Value) && !r.Storno)
            .Select(r => new { r.ID, r.DetaliuId, r.ContDebitId })
            .ToList()
            .Select(r => new { r.ID, DetaliuId = r.DetaliuId.Value, r.ContDebitId })
            .ToList();
        var receptiePeDetaliu = randuriReceptie
            .GroupBy(r => r.DetaliuId)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.ID).First());
        var receptiePeLinie = loturiNascute
            .Select(l => new {
                l.LinieId,
                Rand = receptii
                    .Where(r => r.LotId == l.ID && receptiePeDetaliu.ContainsKey(r.DetaliuId))
                    .OrderBy(r => r.Data).ThenBy(r => r.DetaliuId)
                    .Select(r => receptiePeDetaliu[r.DetaliuId])
                    .FirstOrDefault(),
            })
            .Where(x => x.Rand != null)
            .GroupBy(x => x.LinieId)
            .ToDictionary(g => g.Key, g => (g.First().Rand.ID, g.First().Rand.ContDebitId));

        var produseFolosite = new HashSet<Guid>();
        decimal bazaFacturiAchizitie = 0m, bazaFacturiLivrare = 0m;
        decimal bazaNeincluseAchizitie = 0m, bazaNeincluseLivrare = 0m;

        // Cifrele fiscale rămase în afara fișierului — cusătura D16-D4 le cere
        // numărate, nu doar numite.
        void Neinclus(CauzaNeincludere cauza, Guid docId, bool storno, Guid? detaliuId, string sectiune) {
            decimal baza = 0m, tva = 0m;
            var sens = SensTva.Achizitie;
            var idsCautate = detaliuId is Guid d
                ? [d]
                : liniiPeDocument.GetValueOrDefault(docId)?.Select(l => l.ID).ToList() ?? [];
            foreach (var id in idsCautate)
                if (tvaPeDetaliu.TryGetValue((id, storno), out var fapt)) {
                    baza += fapt.Baza;
                    tva += fapt.Tva;
                    sens = fapt.Sens;
                }
            if (sens == SensTva.Achizitie) bazaNeincluseAchizitie += baza;
            else bazaNeincluseLivrare += baza;
            var doc = documente.GetValueOrDefault(docId);
            neincluse.Add(new SaftNeinclus {
                Cauza = cauza.ToString(),
                Sectiune = sectiune,
                Sens = sens.ToString(),
                DocumentId = docId,
                DocumentNumar = doc?.Numar,
                DocumentTip = CodTip(docId),
                DetaliuId = detaliuId,
                Baza = baza,
                Tva = tva,
                Randuri = 1,
            });
        }

        List<SaftFactura> Facturi(List<Guid> idsDoc, RolTertCont rolAsteptat, string sectiune) {
            var lista = new List<SaftFactura>();
            foreach (var docId in idsDoc
                         .OrderBy(id => documente.TryGetValue(id, out var d) ? d.Data : DateOnly.MinValue)
                         .ThenBy(id => documente.TryGetValue(id, out var d) ? d.Numar ?? "" : "", StringComparer.Ordinal)
                         .ThenBy(id => id)) {
                var doc = documente.GetValueOrDefault(docId);
                var cod = CodTip(docId);
                var esteRetur = TipuriRetur.Contains(cod);
                var randuriDoc = randuriPeDocument.GetValueOrDefault(docId) ?? [];
                var liniiDoc = liniiPeDocument.GetValueOrDefault(docId) ?? [];

                // Jumătățile documentului: operarea și (dacă există) stornarea. Fără
                // rânduri CONTABILE nu se poate emite nimic — dar faptele fiscale
                // există, deci jumătățile se citesc din registrul de TVA.
                var jumatati = randuriDoc.Count > 0
                    ? randuriDoc.Select(r => r.Storno).Distinct().OrderBy(s => s).ToList()
                    : liniiDoc.SelectMany(l => new[] { false, true })
                        .Where(s => liniiDoc.Any(l => tvaPeDetaliu.ContainsKey((l.ID, s))))
                        .Distinct().OrderBy(s => s).ToList();

                foreach (var storno in jumatati) {
                    var randuriJumatate = randuriDoc.Where(r => r.Storno == storno).ToList();
                    var semn = storno ? -1m : 1m;

                    // `Invoice.AccountID` (M): contul de terț de pe rândurile
                    // documentului. Fără el factura nu se poate emite.
                    var contTert = randuriJumatate
                        .SelectMany(r => new[] { r.ContDebitId, r.ContCreditId })
                        .Where(c => Rol(c) == rolAsteptat)
                        .GroupBy(c => c)
                        .OrderByDescending(gr => gr.Count())
                        .Select(gr => (Guid?)gr.Key)
                        .FirstOrDefault();
                    if (contTert == null) {
                        Avert(CodAvertismentSaft.ContFaraRolPeFactura,
                            $"{cod} {doc?.Numar} din {doc?.Data:dd.MM.yyyy} n-are pe rândurile lui niciun cont cu "
                            + $"rol de {(rolAsteptat == RolTertCont.Client ? "client" : "furnizor")} "
                            + "(`Cont.RolTert`) — factura nu se poate emite.");
                        Neinclus(CauzaNeincludere.ContFaraRol, docId, storno, null, sectiune);
                        continue;
                    }

                    // Partenerul: de pe RÂNDUL care poartă contul de terț (vezi
                    // antetul clasei); dacă rândul tace, laturile documentului.
                    Guid? partenerId = null;
                    foreach (var r in randuriJumatate) {
                        if (r.ContDebitId == contTert.Value)
                            partenerId = PartenerulRandului(r.DebitRepartitorId, r.CreditRepartitorId).Id;
                        else if (r.ContCreditId == contTert.Value)
                            partenerId = PartenerulRandului(r.CreditRepartitorId, r.DebitRepartitorId).Id;
                        if (partenerId != null)
                            break;
                    }
                    partenerId ??= PartenerulDocumentului(doc?.PredatorId, doc?.PrimitorId, parteneri);
                    if (partenerId is not Guid pid) {
                        Neinclus(CauzaNeincludere.DocumentFaraPartener, docId, storno, null, sectiune);
                        continue;
                    }
                    var p = parteneri[pid];

                    var factura = new SaftFactura {
                        DocumentId = docId,
                        Storno = storno,
                        DocumentTip = cod,
                        InvoiceNo = doc?.Numar,
                        InvoiceDate = doc?.Data ?? dataStart,
                        InvoiceType = SaftReguli.InvoiceType(storno, esteRetur),
                        SelfBillingIndicator = "0",
                        AccountID = Simbol(contTert.Value),
                        PartenerID = p.Id406,
                        PartenerCheie = p.Id,
                        PartenerDenumire = p.Denumire,
                        BillingAddress = AdresaPartener(p),
                    };
                    tertiReferiti.Add((rolAsteptat, p.Id, factura.AccountID));
                    if (valutaFct.TryGetValue(docId, out var valuta)
                            && !string.IsNullOrWhiteSpace(valuta)
                            && !string.Equals(valuta, DefaultCurrencyCode, StringComparison.OrdinalIgnoreCase))
                        Avert(CodAvertismentSaft.FacturaInValuta,
                            $"{cod} {doc?.Numar} e în {valuta} — fișierul declară totul în RON "
                            + "(`CurrencyAmount` = `Amount`), fără curs.");

                    var pozitie = 0;
                    foreach (var l in liniiDoc) {
                        // Linia de COST a returului de la client (68): fără `TipTva`,
                        // cu lot — mișcare internă venit↔stoc, NU linie de factură.
                        // Rămâne în GL, deci nu e o pierdere.
                        if (cod == CodTipReturClient && l.LotId != null && l.TipTvaId == null)
                            continue;

                        var randuriLinie = randuriJumatate.Where(r => r.DetaliuId == l.ID).ToList();
                        var randContrapartida = randuriLinie.FirstOrDefault(r =>
                            (r.ContDebitId != contTert.Value && !conturiTva.Contains(r.ContDebitId))
                            || (r.ContCreditId != contTert.Value && !conturiTva.Contains(r.ContCreditId)));
                        Guid contLinie;
                        bool contrapartidaDebit;
                        // Rândul din care se citesc DIMENSIUNILE liniei de factură:
                        // al contrapartidei, sau — pe stocul facturii de intrare —
                        // al recepției de pe NIR-ul conex.
                        RandGl randDimensiuni;
                        if (randContrapartida != null) {
                            contrapartidaDebit = randContrapartida.ContDebitId != contTert.Value
                                && !conturiTva.Contains(randContrapartida.ContDebitId);
                            contLinie = contrapartidaDebit
                                ? randContrapartida.ContDebitId : randContrapartida.ContCreditId;
                            randDimensiuni = randContrapartida;
                        }
                        else if (receptiePeLinie.TryGetValue(l.ID, out var receptie)) {
                            // Linia de STOC a facturii de intrare: contul vine din
                            // realitatea materializată a conexului (vezi mai sus),
                            // pe DEBITUL rândului de recepție.
                            contLinie = receptie.ContDebitId;
                            contrapartidaDebit = true;
                            randDimensiuni = randuriPeId.GetValueOrDefault(receptie.ID);
                        }
                        else {
                            // Fără contrapartidă ȘI fără recepție pe lot,
                            // `InvoiceLine.AccountID` (M) n-are sursă — și NU se
                            // inventează (D16-D4). Cazul real: linia de stoc a unei
                            // facturi al cărei NIR conex n-a fost încă operat.
                            Avert(CodAvertismentSaft.LinieFaraContrapartida,
                                $"{cod} {doc?.Numar}: o linie n-are cont contrapartidă în registrul contabil și "
                                + "nici recepție pe lotul născut de ea (NIR-ul conex nu e operat) — "
                                + "linia nu intră în fișier.", semn * l.Valoare);
                            Neinclus(CauzaNeincludere.FaraContrapartida, docId, storno, l.ID, sectiune);
                            continue;
                        }

                        var produsId = produsPeLinie.TryGetValue(l.ID, out var pl)
                            ? pl
                            : l.LotId is Guid lot && produsPeLot.TryGetValue(lot, out var pr) ? pr : (Guid?)null;
                        if (produsId is Guid pidProdus)
                            produseFolosite.Add(pidProdus);

                        var valoare = semn * l.Valoare;
                        var cantitate = Math.Abs(l.Cantitate);
                        var pret = pretUnitar.TryGetValue(l.ID, out var pu) && pu != 0m
                            ? pu
                            : cantitate == 0m
                                ? Math.Abs(valoare)
                                : Scara.RotunjesteBani(Math.Abs(valoare) / cantitate);
                        if (cantitate == 0m)
                            cantitate = 1m;

                        var taxa = Nefiscal();
                        if (tvaPeDetaliu.TryGetValue((l.ID, storno), out var fapt)
                                && tipTvaDupaId.TryGetValue(fapt.TipTvaId, out var tip)) {
                            var codTaxa = fapt.Sens == SensTva.Livrare ? tip.CodSafTLivrare : tip.CodSafTAchizitie;
                            if (!string.IsNullOrWhiteSpace(codTaxa)) {
                                taxa = new SaftTaxInfo {
                                    TaxType = SaftReguli.TaxTypeTva, TaxCode = codTaxa,
                                    TaxPercentage = fapt.Cota, TaxBase = fapt.Baza, TaxAmount = fapt.Tva
                                };
                                codTvaFolosit.TryAdd(codTaxa, (tip.Cota, tip.Denumire));
                            }
                            if (fapt.Sens == SensTva.Achizitie) bazaFacturiAchizitie += fapt.Baza;
                            else bazaFacturiLivrare += fapt.Baza;
                        }

                        pozitie++;
                        factura.Linii.Add(new SaftLinieFactura {
                            DetaliuId = l.ID,
                            LineNumber = pozitie,
                            AccountID = Simbol(contLinie),
                            // Se completează cu codul real după ce se citesc
                            // produsele (mai jos) — aici doar identitatea.
                            ProductCode = produsId?.ToString(),
                            Quantity = cantitate,
                            UnitPrice = pret,
                            TaxPointDate = doc?.Data ?? dataStart,
                            Description = descrieri.GetValueOrDefault(l.ID)
                                ?? tipuriMaterial.GetValueOrDefault(l.TipMaterialId)
                                ?? factura.InvoiceNo,
                            InvoiceLineAmount = valoare,
                            // S.I.47: semnul stă pe SUME, indicatorul rămâne al
                            // direcției documentului (`C` vânzare / `D` cumpărare).
                            DebitCreditIndicator = rolAsteptat == RolTertCont.Client ? "C" : "D",
                            Analiza = randDimensiuni == null ? [] : Analiza(contrapartidaDebit, randDimensiuni),
                            TaxInformation = taxa,
                        });
                        factura.NetTotal += valoare;
                        factura.GrossTotal += valoare + semn * l.ValoareTva;
                    }

                    factura.TaxInformationTotals = factura.Linii
                        .Where(x => x.TaxInformation != null)
                        .GroupBy(x => (x.TaxInformation.TaxType, x.TaxInformation.TaxCode))
                        .Select(gr => new SaftTaxInfo {
                            TaxType = gr.Key.TaxType, TaxCode = gr.Key.TaxCode,
                            TaxBase = gr.Sum(x => x.TaxInformation.TaxBase ?? 0m),
                            TaxAmount = gr.Sum(x => x.TaxInformation.TaxAmount),
                        })
                        .OrderBy(x => x.TaxCode, StringComparer.Ordinal)
                        .ToList();
                    lista.Add(factura);
                }
            }
            return lista;
        }

        rezultat.FacturiEmise = Facturi(idsFacturiVanzare, RolTertCont.Client, "SalesInvoices");
        rezultat.FacturiPrimite = Facturi(idsFacturiCumparare, RolTertCont.Furnizor, "PurchaseInvoices");

        // ── 13. Payments ─────────────────────────────────────────────────────
        var trezorerie = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(d => idsPlati.Contains(d.ID))
            .Select(d => new { d.ID, d.TipInstrument }).ToList()
            .ToDictionary(d => d.ID, d => d.TipInstrument);
        var stingeri = os.GetObjectsQuery<Imperechere>()
            .Where(i => idsPlati.Contains(i.DocumentStingatorId))
            .Select(i => new { i.DocumentStingatorId, i.DocumentId }).ToList()
            .GroupBy(i => i.DocumentStingatorId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.DocumentId).Distinct().ToList());
        var idsStinse = stingeri.SelectMany(s => s.Value).Distinct().ToList();
        var numereStinse = os.GetObjectsQuery<Document>()
            .Where(d => idsStinse.Contains(d.ID))
            .Select(d => new { d.ID, d.Numar }).ToList()
            .ToDictionary(d => d.ID, d => d.Numar);

        foreach (var docId in idsPlati
                     .OrderBy(id => documente.TryGetValue(id, out var d) ? d.Data : DateOnly.MinValue)
                     .ThenBy(id => documente.TryGetValue(id, out var d) ? d.Numar ?? "" : "", StringComparer.Ordinal)
                     .ThenBy(id => id)) {
            var doc = documente.GetValueOrDefault(docId);
            var cod = CodTip(docId);
            var estePlata = cod == CodTipPlata;
            var contrapartidaId = estePlata ? doc?.PrimitorId : doc?.PredatorId;
            var celalaltId = estePlata ? doc?.PredatorId : doc?.PrimitorId;
            // Viramentul intern (F7-D1): ambele laturi conturi proprii — banii nu
            // părăsesc patrimoniul, deci nu e o plată către un terț. Rămâne DOAR în
            // GL, prin construcție (nu e o pierdere, deci nu e `Neinclus`).
            if (contrapartidaId is Guid cid && celalaltId is Guid oid
                    && idsContPropriu.Contains(cid) && idsContPropriu.Contains(oid))
                continue;

            string customer = idSocietate, supplier = idSocietate;
            var randuriDocPlata = randuriPeDocument.GetValueOrDefault(docId) ?? [];
            var contTertPlata = randuriDocPlata
                .SelectMany(r => new[] { r.ContDebitId, r.ContCreditId })
                .FirstOrDefault(c => Rol(c) != RolTertCont.Niciunul);
            if (contrapartidaId is Guid cpid && parteneri.TryGetValue(cpid, out var partenerPlata)) {
                // Rolul îl dă contul de terț al plății; fallback pe direcția tipului.
                var rolPlata = contTertPlata == Guid.Empty
                    ? (estePlata ? RolTertCont.Furnizor : RolTertCont.Client)
                    : Rol(contTertPlata);
                if (rolPlata == RolTertCont.Client) customer = partenerPlata.Id406;
                else supplier = partenerPlata.Id406;
                tertiReferiti.Add((rolPlata, partenerPlata.Id,
                    contTertPlata == Guid.Empty ? null : Simbol(contTertPlata)));
            }
            else if (contrapartidaId is Guid aid && idsAngajat.Contains(aid)) {
                Avert(CodAvertismentSaft.PlataCatreAngajat,
                    $"{cod} {doc?.Numar} din {doc?.Data:dd.MM.yyyy} are ca partener un ANGAJAT "
                    + $"(„{DenumireRep(aid)}”) — SAF-T n-are identitate de angajat, deci "
                    + "`CustomerID`/`SupplierID` ies cu codul societății.");
            }
            else {
                Neinclus(CauzaNeincludere.DocumentFaraPartener, docId, storno: false, null, "Payments");
                continue;
            }

            var (metoda, mecanism) = SaftReguli.MetodaPlata(trezorerie.GetValueOrDefault(docId));
            var plata = new SaftPlata {
                DocumentId = docId,
                DocumentTip = cod,
                PaymentRefNo = doc?.Numar,
                TransactionDate = doc?.Data ?? dataStart,
                PaymentMethod = metoda,
                PaymentMechanism = mecanism,
                Description = $"{tipuriDocument.GetValueOrDefault(cod) ?? cod} {doc?.Numar}".Trim(),
            };
            var stinse = stingeri.GetValueOrDefault(docId) ?? [];
            var sourceDocumentId = stinse.Count == 1 ? numereStinse.GetValueOrDefault(stinse[0]) : null;

            var pozitiePlata = 0;
            foreach (var l in liniiPeDocument.GetValueOrDefault(docId) ?? []) {
                var randPlata = randuriDocPlata.FirstOrDefault(r => r.DetaliuId == l.ID);
                Guid? contLinie = null;
                var debitLatura = estePlata;
                if (randPlata != null) {
                    var cuRol = new[] { (randPlata.ContDebitId, true), (randPlata.ContCreditId, false) }
                        .FirstOrDefault(x => Rol(x.Item1) != RolTertCont.Niciunul);
                    if (cuRol.Item1 != Guid.Empty) {
                        contLinie = cuRol.Item1;
                        debitLatura = cuRol.Item2;
                    }
                    else {
                        contLinie = estePlata ? randPlata.ContDebitId : randPlata.ContCreditId;
                    }
                }
                pozitiePlata++;
                plata.Linii.Add(new SaftLiniePlata {
                    DetaliuId = l.ID,
                    LineNumber = pozitiePlata,
                    SourceDocumentID = sourceDocumentId,
                    AccountID = contLinie is Guid cl ? Simbol(cl) : null,
                    CustomerID = customer,
                    SupplierID = supplier,
                    Description = descrieri.GetValueOrDefault(l.ID) ?? plata.Description,
                    DebitCreditIndicator = estePlata ? "D" : "C",
                    PaymentLineAmount = l.Valoare + l.ValoareTva,
                    Analiza = randPlata == null ? [] : Analiza(debitLatura, randPlata),
                    TaxInformation = Nefiscal(),
                });
                plata.GrossTotal += l.Valoare + l.ValoareTva;
            }
            rezultat.Plati.Add(plata);
        }

        // ── 13b. Master files: partenerul REFERIT se declară, chiar cu sold zero ──
        // Cusătura „fiecare `CustomerID`/`SupplierID` de pe facturi și plăți există
        // în `Customers`/`Suppliers`" nu se ține din solduri: un partener a cărui
        // singură activitate a fost o factură OPERATĂ și STORNATĂ în aceeași lună
        // are rulaj net zero pe contul lui de terț, deci nu apare în agregat — dar
        // fișierul îl referă de două ori (factura 380 și factura 381 de storno).
        // Un identificator referit și nedeclarat e o eroare de validare, iar o
        // intrare cu solduri zero e adevărul.
        foreach (var (rol, partenerId, accountId) in tertiReferiti) {
            var p = parteneri[partenerId];
            if (terti.ContainsKey((rol, p.Id406)))
                continue;
            terti[(rol, p.Id406)] = new SaftTert {
                PartenerId = p.Id,
                Id = p.Id406,
                RegistrationNumber = p.Id406,
                Name = p.Denumire,
                Address = AdresaPartener(p),
                TaxRegistrationNumber = p.Fel == FelIdSaft.CuiRoman ? p.Id406[2..] : null,
                TaxType = SaftReguli.RegimFiscalPartener(p.Fel, p.InregistratTva, p.TvaLaIncasare),
                AccountID = accountId,
                OpeningDebitBalance = 0m,
                ClosingDebitBalance = 0m,
                FelId = p.Fel.ToString(),
            };
        }
        rezultat.Clienti = terti.Where(t => t.Key.Rol == RolTertCont.Client).Select(t => t.Value)
            .OrderBy(t => t.Id, StringComparer.Ordinal).ToList();
        rezultat.Furnizori = terti.Where(t => t.Key.Rol == RolTertCont.Furnizor).Select(t => t.Value)
            .OrderBy(t => t.Id, StringComparer.Ordinal).ToList();

        // ── 14. Products + UOMTable ──────────────────────────────────────────
        var listaProduse = produseFolosite.ToList();
        var produse = os.GetObjectsQuery<Produs>().IgnoreQueryFilters()
            .Where(p => listaProduse.Contains(p.ID))
            .Select(p => new {
                p.ID, p.Cod, p.Denumire, p.CodNc, p.UM,
                CodUm = p.UnitateMasura.Cod, DenumireUm = p.UnitateMasura.Denumire,
                Natura = (NaturaClasa?)p.TipMaterial.Clasa.Natura
            })
            .ToList();
        var unitatiFolosite = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in produse.OrderBy(x => x.Cod ?? "", StringComparer.Ordinal).ThenBy(x => x.ID)) {
            var codNc = p.CodNc;
            if (string.IsNullOrWhiteSpace(codNc)) {
                codNc = CodNcImplicit;
                Avert(CodAvertismentSaft.FaraCodNc,
                    $"Produsul „{p.Denumire}” ({p.Cod}) n-are cod NC — `ProductCommodityCode` iese „{CodNcImplicit}”.");
            }
            var codUm = p.CodUm;
            var denumireUm = p.DenumireUm;
            if (string.IsNullOrWhiteSpace(codUm)) {
                codUm = UnitateImplicita;
                denumireUm = null;
                Avert(CodAvertismentSaft.FaraUnitateMasura,
                    $"Produsul „{p.Denumire}” ({p.Cod}) n-are unitate de măsură UN/ECE "
                    + $"(UM liberă: „{p.UM ?? "gol"}”) — `UOMBase` iese „{UnitateImplicita}”.");
            }
            if (!unitatiFolosite.TryGetValue(codUm, out var existenta) || existenta == null)
                unitatiFolosite[codUm] = denumireUm;
            rezultat.Produse.Add(new SaftProdus {
                ProdusId = p.ID,
                ProductCode = p.Cod ?? p.ID.ToString(),
                GoodsServicesID = p.Natura == NaturaClasa.Stoc ? "01" : "02",
                Description = p.Denumire,
                ProductCommodityCode = codNc,
                ValuationMethod = MetodaEvaluare,
                UOMBase = codUm,
                UOMStandard = codUm,
                // SEM: „If UOMBase = UOMStandard, UOMToUOMBaseConversionFactor = 1".
                UOMToUOMBaseConversionFactor = 1m,
            });
        }
        // Denumirile lipsă (unitatea de rezervă) se completează din nomenclator.
        var coduriFaraDenumire = unitatiFolosite.Where(u => u.Value == null).Select(u => u.Key).ToList();
        foreach (var u in os.GetObjectsQuery<UnitateMasura>()
                     .Where(x => coduriFaraDenumire.Contains(x.Cod))
                     .Select(x => new { x.Cod, x.Denumire }).ToList())
            unitatiFolosite[u.Cod] = u.Denumire;
        rezultat.Unitati = unitatiFolosite.OrderBy(u => u.Key, StringComparer.Ordinal)
            .Select(u => new SaftUnitate { UnitOfMeasure = u.Key, Description = u.Value ?? u.Key })
            .ToList();

        // `ProductCode`/`InvoiceUOM` pe liniile de factură: identitatea pusă mai sus
        // se înlocuiește cu datele reale ale produsului.
        var codProdus = produse.ToDictionary(p => p.ID, p => p.Cod ?? p.ID.ToString());
        var denumireProdus = produse.ToDictionary(p => p.ID, p => p.Denumire);
        var umProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.UOMBase);
        foreach (var f in rezultat.FacturiEmise.Concat(rezultat.FacturiPrimite))
            foreach (var l in f.Linii) {
                if (l.ProductCode == null || !Guid.TryParse(l.ProductCode, out var pidLinie))
                    continue;
                l.ProductCode = codProdus.GetValueOrDefault(pidLinie);
                l.ProductDescription = denumireProdus.GetValueOrDefault(pidLinie);
                l.InvoiceUOM = umProdus.GetValueOrDefault(pidLinie);
            }

        // ── 15. TaxTable + AnalysisTypeTable ─────────────────────────────────
        rezultat.Taxe = codTvaFolosit
            .Where(t => t.Key != SaftReguli.TaxCodeNefiscal)
            .OrderBy(t => t.Key, StringComparer.Ordinal)
            .Select(t => new SaftTaxCode {
                TaxType = SaftReguli.TaxTypeTva,
                TaxCode = t.Key,
                Description = t.Value.Denumire,
                TaxPercentage = t.Value.Cota,
                // `SAFBaseRate` e restricționat [0,0000–1,0000] — „integral
                // deductibil" se scrie `1`, nu `100` (nota din descriere).
                BaseRate = 1m,
                Country = AuditFileCountry,
            })
            .ToList();
        rezultat.TipuriAnaliza = tipuriAnalizaFolosite.Values
            .OrderBy(a => a.AnalysisType, StringComparer.Ordinal)
            .ThenBy(a => a.AnalysisID, StringComparer.Ordinal)
            .ToList();

        // ── 16. Cusăturile (D16-D4) ──────────────────────────────────────────
        bool EsteFactura(Guid documentId) {
            var c = CodTip(documentId);
            return TipuriVanzare.Contains(c) || TipuriCumparare.Contains(c);
        }
        rezultat.Rezumat = new SaftRezumat {
            Tranzactii = rezultat.Jurnale.Sum(j => j.Tranzactii.Count),
            LiniiGl = numarLinii,
            RanduriRegistru = randuri.Count,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            ValoareRegistruContabil = randuri.Sum(r => r.Valoare),
            TvaGl = tvaGl,
            TvaRegistru = randuriTva.Sum(t => t.Tva),
            TvaCapitalizat = randuriTva.Where(t => t.Regim == RegimTva.Capitalizat).Sum(t => t.Tva),
            TvaFaraCodSaft = tvaFaraCodSaft,
            BazaFacturiAchizitie = bazaFacturiAchizitie,
            BazaFacturiLivrare = bazaFacturiLivrare,
            BazaNeincluseAchizitie = bazaNeincluseAchizitie,
            BazaNeincluseLivrare = bazaNeincluseLivrare,
            BazaRegistruAchizitie = randuriTva
                .Where(t => EsteFactura(t.DocumentId) && t.Sens == SensTva.Achizitie).Sum(t => t.Baza),
            BazaRegistruLivrare = randuriTva
                .Where(t => EsteFactura(t.DocumentId) && t.Sens == SensTva.Livrare).Sum(t => t.Baza),
            ClosingGla = rezultat.Conturi.Sum(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m)),
            ClosingBalanta = balanta.Sum(b => b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit),
            NetTotalEmise = rezultat.FacturiEmise.Sum(f => f.NetTotal),
            NetTotalPrimite = rezultat.FacturiPrimite.Sum(f => f.NetTotal),
            GrossTotalEmise = rezultat.FacturiEmise.Sum(f => f.GrossTotal),
            GrossTotalPrimite = rezultat.FacturiPrimite.Sum(f => f.GrossTotal),
            TotalPlati = rezultat.Plati.Sum(p => p.GrossTotal),
            NumarClienti = rezultat.Clienti.Count,
            NumarFurnizori = rezultat.Furnizori.Count,
            NumarFacturiEmise = rezultat.FacturiEmise.Count,
            NumarFacturiPrimite = rezultat.FacturiPrimite.Count,
            NumarPlati = rezultat.Plati.Count,
            NumarProduse = rezultat.Produse.Count,
        };

        // ── 17. Avertismentele, AGREGATE per cauză ───────────────────────────
        rezultat.Avertismente = avertismente
            .OrderBy(a => a.Key)
            .Select(a => new SaftAvertisment {
                Cod = a.Key.ToString(),
                Mesaj = Mesaj(a.Key),
                Numar = a.Value.Count,
                Suma = a.Value.Any(x => x.Suma != null) ? a.Value.Sum(x => x.Suma ?? 0m) : null,
                Exemple = a.Value.Take(5).Select(x => x.Exemplu).ToList(),
            })
            .ToList();

        rezultat.Neincluse = neincluse
            .OrderBy(n => n.Cauza, StringComparer.Ordinal)
            .ThenBy(n => n.Sectiune ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.DocumentNumar ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.ContSimbol ?? "", StringComparer.Ordinal)
            .ToList();
        return rezultat;
    }

    // ── helper-e pure ───────────────────────────────────────────────────────

    // Riscul 2: doi parteneri cu același identificator ⇒ O intrare, cu soldurile
    // CUMULATE (o intrare care pierde cifre ar fi mai rea decât cheia duplicată).
    static void CumuleazaSolduri(SaftTert tinta, SaftTert sursa) {
        var netInitial = (tinta.OpeningDebitBalance ?? 0m) - (tinta.OpeningCreditBalance ?? 0m)
            + (sursa.OpeningDebitBalance ?? 0m) - (sursa.OpeningCreditBalance ?? 0m);
        var net = (tinta.ClosingDebitBalance ?? 0m) - (tinta.ClosingCreditBalance ?? 0m)
            + (sursa.ClosingDebitBalance ?? 0m) - (sursa.ClosingCreditBalance ?? 0m);
        tinta.OpeningDebitBalance = netInitial >= 0m ? netInitial : null;
        tinta.OpeningCreditBalance = netInitial < 0m ? -netInitial : null;
        tinta.ClosingDebitBalance = net >= 0m ? net : null;
        tinta.ClosingCreditBalance = net < 0m ? -net : null;
    }

    // Partenerul „al documentului": exact unul pe laturi ⇒ el; zero sau doi ⇒ null
    // (latura liberă rămâne societatea — GL.19/GL.20 COM).
    static Guid? PartenerulDocumentului(Guid? predatorId, Guid? primitorId,
        Dictionary<Guid, InfoPartener> parteneri) {
        var gasiti = new[] { predatorId, primitorId }
            .Where(id => id is Guid v && parteneri.ContainsKey(v))
            .Select(id => id.Value)
            .Distinct()
            .ToList();
        return gasiti.Count == 1 ? gasiti[0] : null;
    }

    // Rolul „al documentului": primul rol de terț întâlnit pe rândurile lui.
    static RolTertCont RolulDocumentului(IEnumerable<RandGl> randuri, Func<Guid, RolTertCont> rol) {
        foreach (var r in randuri) {
            var rd = rol(r.ContDebitId);
            if (rd != RolTertCont.Niciunul)
                return rd;
            var rc = rol(r.ContCreditId);
            if (rc != RolTertCont.Niciunul)
                return rc;
        }
        return RolTertCont.Niciunul;
    }

    static string VersiuneAssembly() {
        var versiune = typeof(SaftProiectii).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(SaftProiectii).Assembly.GetName().Version?.ToString()
            ?? "1.0";
        // `SoftwareVersion` e `SAFshorttextType` (max 18).
        return versiune.Length <= 18 ? versiune : versiune[..18];
    }

    static string Mesaj(CodAvertismentSaft cod) => cod switch {
        CodAvertismentSaft.FaraCodNc =>
            "Produse fără cod NC (`ProductCommodityCode` e obligatoriu în `Product`) — se declară cu valoarea „0”, "
            + "cea folosită de exportul legacy în producție; completați `Produs.CodNc` unde legea îl cere.",
        CodAvertismentSaft.FaraUnitateMasura =>
            "Produse fără unitate de măsură din nomenclatorul UN/ECE — `UOMBase`/`UOMStandard` ies „H87” (bucată); "
            + "legați `Produs.UnitateMasura` (conectorul 1C o rezolvă din grafia românească).",
        CodAvertismentSaft.AdresaIncompleta =>
            "Parteneri fără localitate — `City` e obligatoriu în `AddressStructure`; se declară „Nespecificat”, "
            + "ceea ce trece validarea sintactică, dar adresa rămâne falsă.",
        CodAvertismentSaft.TipTvaFaraCodSaft =>
            "Tipuri de TVA fără cod SAF-T pe direcția folosită — rândurile lor ies cu `TaxType 000` / "
            + "`TaxCode 000000`, adică „nerelevant fiscal”; completați `TipTva.CodSafT*`.",
        CodAvertismentSaft.TipContNecunoscut =>
            "Conturi cu `Functie` diferită de D/C/B — `AccountType` iese „Bifunctional”, valoarea care nu minte "
            + "despre sensul soldului.",
        CodAvertismentSaft.PlataCatreAngajat =>
            "Plăți/încasări a căror contrapartidă e un angajat — SAF-T n-are identitate de angajat, deci ambele "
            + "identificatoare ies cu codul societății raportoare (convenția GL.19/GL.20).",
        CodAvertismentSaft.FacturaInValuta =>
            "Facturi în altă valută decât RON — fișierul declară totul în moneda antetului (34g: valutele rămân "
            + "amânate), deci cursul și suma în valută nu se raportează.",
        CodAvertismentSaft.SocietateIncompleta =>
            "Antetul societății raportoare e incomplet — fișierul se generează, dar validatorul îl respinge: "
            + "completați „Configurare → Societate”.",
        CodAvertismentSaft.ContFaraRolPeFactura =>
            "Documente de factură fără niciun cont cu rol de terț pe rândurile lor — `Invoice.AccountID` e "
            + "obligatoriu, deci factura nu se emite (vezi `Neincluse`); verificați `Cont.RolTert` din plan.",
        CodAvertismentSaft.PartenerDublat =>
            "Parteneri distincți cu același identificator SAF-T — master files cer o cheie unică, deci se declară "
            + "o singură intrare cu soldurile cumulate; verificați nomenclatorul.",
        CodAvertismentSaft.PartenerFaraCuiValid =>
            "Parteneri români sau înregistrați în scopuri de TVA al căror cod fiscal nu trece cifra de control — "
            + "identificatorul lor iese cu prefixul `04` (cod intern), fiindcă `00` cere un CUI valid.",
        CodAvertismentSaft.LinieFaraContrapartida =>
            "Linii de factură fără cont contrapartidă în registrul contabil — cazul liniilor de STOC ale facturii "
            + "de intrare, a căror recepție contează pe NIR-ul conex (26a). `InvoiceLine.AccountID` e obligatoriu "
            + "și nu se inventează, deci liniile ies în `Neincluse`.",
        CodAvertismentSaft.TertFaraPartener =>
            "Rânduri de registru pe conturi de terți fără niciun partener pe laturi — `CustomerID`/`SupplierID` "
            + "ies cu codul societății, iar soldul lor nu ajunge în `Customers`/`Suppliers`.",
        _ => cod.ToString(),
    };
}

