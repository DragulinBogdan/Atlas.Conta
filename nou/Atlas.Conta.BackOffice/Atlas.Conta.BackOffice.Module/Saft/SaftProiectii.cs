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
    // Modulul S (felia 17): tipul declarației vine EXCLUSIV de aici — validatorul
    // citește `AUDIT_FILE_TYPE.ON_DEMAND = "C"`; un „D406S" nu există.
    public const string HeaderCommentStocuri = "C";
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
    /// Sumarul declarației: antetul, cât are fiecare secțiune, cusăturile,
    /// `Neincluse` și avertismentele — TOT ce citește omul înainte de a depune,
    /// FĂRĂ listele care fac fișierul (D16-D5, amendamentul pasului 4).
    /// <para>
    /// Funcție PURĂ pe DTO: nicio interogare, niciun `IObjectSpace`. Deci nu
    /// poate diverge de fișier — sumarul și XML-ul se scriu din același obiect,
    /// iar un contor greșit ar însemna o listă greșită, nu o a doua numărătoare.
    /// Contoarele se calculează din LISTE, nu se copiază din `Rezumat`: acolo
    /// unde `Rezumat` are aceeași cifră (`NumarClienti`, `Tranzactii`…), egalitatea
    /// e o cusătură probată în ModelCheck, nu o presupunere.
    /// </para>
    /// </summary>
    public static SaftSumarDto Sumar(SaftDto dto) {
        ArgumentNullException.ThrowIfNull(dto);
        return new SaftSumarDto {
            Neaplicabil = dto.Neaplicabil,
            An = dto.An,
            Luna = dto.Luna,
            DataStart = dto.DataStart,
            DataEnd = dto.DataEnd,
            Header = dto.Header,
            Conturi = dto.Conturi.Count,
            Clienti = dto.Clienti.Count,
            Furnizori = dto.Furnizori.Count,
            CoduriTaxa = dto.Taxe.Count,
            Unitati = dto.Unitati.Count,
            Produse = dto.Produse.Count,
            TipuriAnaliza = dto.TipuriAnaliza.Count,
            Jurnale = dto.Jurnale.Count,
            Tranzactii = dto.Jurnale.Sum(j => j.Tranzactii.Count),
            LiniiGl = dto.Jurnale.Sum(j => j.Tranzactii.Sum(t => t.Linii.Count)),
            FacturiEmise = dto.FacturiEmise.Count,
            FacturiPrimite = dto.FacturiPrimite.Count,
            Plati = dto.Plati.Count,
            // Modulul S (felia 17): aceeași regulă — contoarele din LISTE, iar
            // acolo unde `Rezumat` are deja cifra, egalitatea e o cusătură.
            TipuriMiscare = dto.TipuriMiscare.Count,
            StocFizic = dto.StocFizic.Count,
            MiscariStoc = dto.MiscariStoc.Count,
            LiniiMiscare = dto.MiscariStoc.Sum(m => m.Linii.Count),
            Excluse = dto.Excluse,
            Rezumat = dto.Rezumat,
            Neincluse = dto.Neincluse,
            Avertismente = dto.Avertismente,
        };
    }

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
        rezultat.Neaplicabil = MotivNeaplicabil(os, "");
        if (rezultat.Neaplicabil != null)
            return rezultat;

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
            var taraPartener = SaftReguli.CodTaraSaft(p.Tara);
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
        var soc = CitesteSocietate(os);
        VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));

        var idSocietate = soc == null ? null : SaftReguli.IdSocietate(soc.CodFiscal, soc.Tara);
        var raporteazaCnp = soc?.RaporteazaCnp ?? false;

        rezultat.Header = Antet(soc, an, luna, dataCreare, HeaderComment);

        // ── 2. Nomenclatoarele-etichetă (dicționare, LEFT JOIN în memorie) ───
        var conturi = CitesteConturi(os);
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
        var (conturiSaft, balanta) = ConturiSiSolduri(
            os, dataStart, dataEnd, conturi, (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Conturi = conturiSaft;

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
        //
        // ═══ Regula pe rândul cu DOI parteneri (fixul F2, pin-uire) ═══
        // Pe o notă de compensare `401 = 4111` cu partenerul A pe debit și B pe
        // credit, AMBELE conturi au rol de terț și AMBELE laturi au un partener.
        // Regula NU e „primul găsit" și nu e „partenerul documentului": fiecare
        // latură își ia PARTENERUL PROPRIU (`repLatura`), iar rolul îl dă CONTUL
        // acelei laturi. Deci A, de pe debitul lui 401 (cont de furnizor), iese
        // FURNIZOR, iar B, de pe creditul lui 4111 (cont de client), iese CLIENT
        // — chiar dacă intuiția „debitul lui 401 stinge o datorie, deci e
        // furnizorul de pe factură" ar fi dat același rezultat din alt motiv.
        // Căderea pe `repCealalta` e REZERVA pentru cazul (majoritar, 64h) în
        // care latura contului de terț poartă contrapartida, nu titularul: ea se
        // aplică doar când latura proprie NU e un `Partener`.
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
            // Fixul C1 al review-ului: avertismentul e despre un cod fiscal care
            // NU TRECE cifra de control, deci cere un cod fiscal. O persoană
            // fizică fără cod (retail, `CodFiscal` gol) nu „pică validarea" — ea
            // n-are ce valida, iar `04`+cod intern e chiar răspunsul corect al
            // §B.4 pentru ea. Lipsa codului rămâne vizibilă prin `FelId`.
            if (p.Fel == FelIdSaft.CodIntern
                    && !string.IsNullOrWhiteSpace(p.CodFiscal)
                    && (p.InregistratTva || Partener.NormalizeazaTara(p.Tara) == "RO"))
                Avert(CodAvertismentSaft.PartenerFaraCuiValid,
                    $"„{p.Denumire}” e român sau înregistrat în scopuri de TVA, dar codul fiscal "
                    + $"(„{p.CodFiscal}”) nu trece cifra de control — se declară cu prefixul 04 "
                    + "(cod intern), nu 00.");
        }
        // Partenerii REFERIȚI de facturi și plăți, strânși pe parcurs: master files
        // trebuie să-i declare pe toți, chiar dacă soldul lor e zero (vezi
        // `AsiguraTert` de mai jos). Listele se așază abia după secțiunile
        // documentelor, din același motiv.
        var tertiReferiti = new List<(RolTertCont Rol, Guid PartenerId, string AccountID)>();

        // ── 8. Etichetele dimensiunilor (AnalysisTypeTable + Analysis) ───────
        var etichete = new EtichetePerioada(os, repartitori);

        List<SaftAnaliza> Analiza(bool debit, RandGl r) {
            var lista = new List<SaftAnaliza>();
            void Adauga(string tip, Guid? id) {
                if (id is not Guid v)
                    return;
                lista.Add(new SaftAnaliza { AnalysisType = tip, AnalysisID = etichete.Inregistreaza(tip, v) });
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
        // Fixul C1: avertismentul „tip de TVA fără cod SAF-T" e UNUL per tip (nu
        // per rând — pe o lună reală ar fi mii), dar SUMA lui trebuie să fie a
        // TUTUROR rândurilor tipului, nu a primului întâlnit. Deci se acumulează
        // aici și se emite AGREGAT după §11, când s-a terminat de citit GL-ul.
        var tvaFaraCod = new Dictionary<Guid, (string Cod, string Denumire, SensTva Sens, decimal Suma, int Randuri)>();
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
                        var acumulat = tvaFaraCod.GetValueOrDefault(tip.ID);
                        tvaFaraCod[tip.ID] = (tip.Cod, tip.Denumire, fapt.Sens,
                            acumulat.Suma + fapt.Tva, acumulat.Randuri + 1);
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
            // Fixul L3: data tranzacției e a RÂNDURILOR ei, nu a documentului. Un
            // document operat în luna trecută și STORNAT în luna asta apare aici
            // doar cu rândurile lui de storno (motorul le scrie la `dataStorno`);
            // `doc.Data` ar fi pus pe el o dată din afara perioadei declarate,
            // adică o tranzacție care contrazice `Period`. Pentru documentele
            // operate și stornate în aceeași lună, minimul E `doc.Data`, deci
            // nimic nu se schimbă.
            var dataDoc = randuriDoc.Count > 0 ? randuriDoc.Min(r => r.Data) : doc?.Data ?? dataStart;
            var tranzactie = new SaftTranzactie {
                DocumentId = docId,
                TransactionID = docId.ToString(),
                Period = luna,
                PeriodYear = an,
                TransactionDate = dataDoc,
                Description = descriereDoc,
                SystemEntryDate = doc?.DataOperare is DateTime dt
                    ? DateOnly.FromDateTime(dt) : dataDoc,
                GLPostingDate = dataDoc,
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
                    // Fixul F3, cusătura 1: totalurile se numără din LINIILE
                    // EMISE, fiecare pe latura ei — `DebitAmount` la `D`,
                    // `CreditAmount` la `C`. Varianta dinainte (`+= r.Valoare` de
                    // două ori, în afara buclei de laturi) le făcea egale prin
                    // construcție, deci cusătura „Σ debit == Σ credit" se
                    // verifica pe sine și n-ar fi prins niciodată o latură
                    // pierdută la scriere.
                    if (debit) totalDebit += r.Valoare;
                    else totalCredit += r.Valoare;
                }
            }
            jurnal.Tranzactii.Add(tranzactie);
        }
        rezultat.Jurnale = jurnale.Values.OrderBy(j => j.JournalID, StringComparer.Ordinal).ToList();

        // Fixul C1: avertismentele „tip de TVA fără cod SAF-T", acum că GL-ul s-a
        // terminat — unul per TIP, cu Σ pe toate rândurile lui.
        foreach (var t in tvaFaraCod.Values.OrderBy(t => t.Cod, StringComparer.Ordinal))
            Avert(CodAvertismentSaft.TipTvaFaraCodSaft,
                $"Tipul de TVA „{t.Cod}” ({t.Denumire}) n-are cod SAF-T pe "
                + $"{(t.Sens == SensTva.Livrare ? "livrare" : "achiziție")} — cele {t.Randuri} rânduri ale lui "
                + "ies cu `000/000000`.", t.Suma);

        // ── 12. Liniile documentelor de factură și de plată ──────────────────
        var idsFacturiVanzare = idsDocumente.Where(id => TipuriVanzare.Contains(CodTip(id))).ToList();
        var idsFacturiCumparare = idsDocumente.Where(id => TipuriCumparare.Contains(CodTip(id))).ToList();
        var idsPlati = idsDocumente.Where(id => TipuriPlata.Contains(CodTip(id))).ToList();
        var idsFacturi = idsFacturiVanzare.Concat(idsFacturiCumparare).ToList();
        var idsCuLinii = idsFacturi.Concat(idsPlati).ToList();

        // ═══ Fixul L1: contul de terț al facturii poate sta pe CONEXUL ei ═══
        // Pe o factură de intrare cu TOATE liniile pe stoc (achiziție intra-
        // comunitară, taxare inversă, scutită), singurele rânduri contabile ale
        // FACTURII sunt `4426 = 4427` — recepția, cu tot cu 401-ul, contează pe
        // NIR-ul conex (26a). Căutat doar pe rândurile facturii, `Invoice.AccountID`
        // (M) n-avea sursă și factura cădea în `Neincluse/ContFaraRol`: 84 de
        // facturi reale pe o singură lună a bazei de import, cu bază și TVA cu tot.
        //
        // Contul NU se inventează (nici din `Repartitor.ContImplicit`): se citește
        // din ce s-a scris efectiv pe conexul autogenerat — același principiu ca
        // `receptiePeLinie` de mai jos, aplicat la nivel de DOCUMENT în loc de
        // linie. Pe SETURI: două interogări pentru toate facturile lunii.
        //
        // Fără filtru pe `Storno`: simbolul contului de terț e același pe ambele
        // jumătăți ale conexului, iar jumătatea de storno a facturii-sursă poate
        // exista fără ca cea a conexului să existe (sau invers).
        var conexe = os.GetObjectsQuery<Document>()
            .Where(d => d.Autogenerat && d.DocumentSursaId != null
                && idsFacturi.Contains(d.DocumentSursaId.Value))
            .Select(d => new { d.ID, d.DocumentSursaId })
            .ToList()
            .Select(d => new { d.ID, SursaId = d.DocumentSursaId.Value })
            .ToList();
        var idsConex = conexe.Select(c => c.ID).ToList();
        var randuriPeConex = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes()
            .Where(r => r.DocumentId != null && idsConex.Contains(r.DocumentId.Value))
            .Select(r => new { r.DocumentId, r.ContDebitId, r.ContCreditId })
            .ToList()
            .Select(r => new { DocumentId = r.DocumentId.Value, r.ContDebitId, r.ContCreditId })
            .GroupBy(r => r.DocumentId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var conturiConexePeFactura = new Dictionary<Guid, List<Guid>>();
        foreach (var c in conexe.OrderBy(c => c.ID)) {
            if (!randuriPeConex.TryGetValue(c.ID, out var randuriConex))
                continue;
            if (!conturiConexePeFactura.TryGetValue(c.SursaId, out var lista))
                lista = conturiConexePeFactura[c.SursaId] = [];
            foreach (var r in randuriConex) {
                lista.Add(r.ContDebitId);
                lista.Add(r.ContCreditId);
            }
        }

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
                    // Fixul L3: data JUMĂTĂȚII, nu a documentului. Stornoul e o
                    // factură proprie (`381`), iar motorul îi scrie rândurile la
                    // `dataStorno` — o factură de storno emisă în luna asta n-are
                    // voie să poarte data facturii originale, care poate fi în
                    // altă lună (sau chiar în alt an) decât perioada declarată.
                    var dataJumatate = randuriJumatate.Count > 0
                        ? randuriJumatate.Min(r => r.Data)
                        : doc?.Data ?? dataStart;

                    // `Invoice.AccountID` (M): contul de terț de pe rândurile
                    // documentului — sau, dacă factura n-are niciunul, de pe
                    // rândurile CONEXULUI ei autogenerat (fixul L1, vezi §12).
                    // Fără el factura nu se poate emite.
                    var contTert = randuriJumatate
                        .SelectMany(r => new[] { r.ContDebitId, r.ContCreditId })
                        .Where(c => Rol(c) == rolAsteptat)
                        .GroupBy(c => c)
                        .OrderByDescending(gr => gr.Count())
                        .Select(gr => (Guid?)gr.Key)
                        .FirstOrDefault();
                    if (contTert == null && conturiConexePeFactura.TryGetValue(docId, out var conturiConex))
                        contTert = conturiConex
                            .Where(c => Rol(c) == rolAsteptat)
                            .GroupBy(c => c)
                            .OrderByDescending(gr => gr.Count())
                            .Select(gr => (Guid?)gr.Key)
                            .FirstOrDefault();
                    if (contTert == null) {
                        Avert(CodAvertismentSaft.ContFaraRolPeFactura,
                            $"{cod} {doc?.Numar} din {doc?.Data:dd.MM.yyyy} n-are niciun cont cu "
                            + $"rol de {(rolAsteptat == RolTertCont.Client ? "client" : "furnizor")} "
                            + "(`Cont.RolTert`) nici pe rândurile lui, nici pe cele ale conexelor autogenerate "
                            + "— factura nu se poate emite.");
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
                        InvoiceDate = dataJumatate,
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

                        // Faptul fiscal se citește ÎNAINTE de valori: pe regimul
                        // `Capitalizat` el e singura sursă a NETULUI (fixul L2).
                        var areFapt = tvaPeDetaliu.TryGetValue((l.ID, storno), out var fapt)
                            && tipTvaDupaId.ContainsKey(fapt.TipTvaId);
                        var tip = areFapt ? tipTvaDupaId[fapt.TipTvaId] : null;
                        // ═══ Fixul L2: `Capitalizat` — `Valoare` de pe linie e BRUTĂ ═══
                        // Pe NED21 (achiziție fără drept de deducere) `TvaService`
                        // pune TVA-ul ÎN cost: `DocumentDetaliu.Valoare` = brut,
                        // `ValoareTva` = 0. `RegistruTva` desface înapoi baza
                        // (`RegistruTvaService.Cifre`), deci `fapt.Baza + fapt.Tva
                        // == Valoare` EXACT. `InvoiceLineAmount` e o valoare NETĂ
                        // (are `TaxInformation` lângă ea, cu baza și taxa), deci
                        // linia trebuie să iasă cu `fapt.Baza`: altfel factura
                        // declara brutul ca net, iar `NetTotal` era umflat cu
                        // TVA-ul nedeductibil. `fapt.Baza`/`fapt.Tva` sunt DEJA
                        // semnate (rândul de storno le poartă negative), deci
                        // `semn` nu se mai aplică peste ele.
                        var capitalizat = areFapt && fapt.Regim == RegimTva.Capitalizat;
                        var valoare = capitalizat ? fapt.Baza : semn * l.Valoare;
                        var cantitate = Math.Abs(l.Cantitate);
                        var pret = pretUnitar.TryGetValue(l.ID, out var pu) && pu != 0m
                            ? pu
                            : cantitate == 0m
                                ? Math.Abs(valoare)
                                : Scara.RotunjesteBani(Math.Abs(valoare) / cantitate);
                        if (cantitate == 0m)
                            cantitate = 1m;

                        var taxa = Nefiscal();
                        if (areFapt) {
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
                            TaxPointDate = dataJumatate,
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
                        // Brutul: net + taxa liniei. Pe `Capitalizat`, `ValoareTva`
                        // e 0 pe linie (TVA-ul e în cost), iar taxa reală e a
                        // faptului fiscal — deci brutul se reface din el, ca
                        // `NetTotal + TVA == GrossTotal` să rămână adevărat (fixul L2).
                        factura.GrossTotal += valoare + (capitalizat ? fapt.Tva : semn * l.ValoareTva);
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

            var randuriDocPlata = randuriPeDocument.GetValueOrDefault(docId) ?? [];
            var contTertPlata = randuriDocPlata
                .SelectMany(r => new[] { r.ContDebitId, r.ContCreditId })
                .FirstOrDefault(c => Rol(c) != RolTertCont.Niciunul);

            string customer = idSocietate, supplier = idSocietate;
            if (contrapartidaId is Guid cpid && parteneri.TryGetValue(cpid, out var partenerPlata)) {
                // ═══ Fixul F6: terțul REFERIT are nevoie de un `AccountID` ═══
                // `Customer`/`Supplier` din master files cere `AccountID` (M).
                // Dacă rândurile plății n-ating niciun cont cu `RolTert` (plata
                // pe 462 „Creditori diverși", pe un cont de decontare oarecare),
                // intrarea de terț ar fi ieșit cu `<AccountID/>` gol — adică un
                // fișier invalid, respins de validator pe o cauză care n-are
                // nicio legătură cu plata. Contul NU se inventează: plata iese
                // în `Neincluse`, cu cauză și cu avertisment. Rândurile ei rămân
                // în GL, cu societatea pe ambele identificatoare (nu e o pierdere
                // contabilă, e o absență din secțiunea `Payments`).
                if (contTertPlata == Guid.Empty || Simbol(contTertPlata) == null) {
                    Avert(CodAvertismentSaft.PlataFaraContTert,
                        $"{cod} {doc?.Numar} din {doc?.Data:dd.MM.yyyy} către „{partenerPlata.Denumire}” n-are pe "
                        + "rândurile ei niciun cont cu rol de terț (`Cont.RolTert`) — `Customer`/`Supplier` "
                        + "n-ar avea ce `AccountID` să declare, deci plata nu intră în `Payments`.");
                    Neinclus(CauzaNeincludere.ContFaraRol, docId, storno: false, null, "Payments");
                    continue;
                }
                var rolPlata = Rol(contTertPlata);
                if (rolPlata == RolTertCont.Client) customer = partenerPlata.Id406;
                else supplier = partenerPlata.Id406;
                tertiReferiti.Add((rolPlata, partenerPlata.Id, Simbol(contTertPlata)));
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
            var stinse = stingeri.GetValueOrDefault(docId) ?? [];
            var sourceDocumentId = stinse.Count == 1 ? numereStinse.GetValueOrDefault(stinse[0]) : null;
            var liniiPlata = liniiPeDocument.GetValueOrDefault(docId) ?? [];

            // ═══ Fixul F1: plata STORNATĂ e o plată proprie, cu semnul ei ═══
            // Aceeași unitate ca la facturi (Document × Storno): motorul scrie
            // rândurile inverse la `dataStorno`, deci o plată operată și stornată
            // în aceeași lună are DOUĂ jumătăți. Fără spargere, secțiunea
            // `Payments` o declara o singură dată, POZITIV — adică declara ca
            // încasată o sumă care fusese anulată, iar `TotalDebit`/`TotalCredit`
            // ale secțiunii nu mai băteau cu GL-ul.
            var jumatatiPlata = randuriDocPlata.Count > 0
                ? randuriDocPlata.Select(r => r.Storno).Distinct().OrderBy(s => s).ToList()
                : [false];
            foreach (var stornoPlata in jumatatiPlata) {
                var randuriJumatatePlata = randuriDocPlata.Where(r => r.Storno == stornoPlata).ToList();
                var semnPlata = stornoPlata ? -1m : 1m;
                var descriereBaza = $"{tipuriDocument.GetValueOrDefault(cod) ?? cod} {doc?.Numar}".Trim();
                var plata = new SaftPlata {
                    DocumentId = docId,
                    Storno = stornoPlata,
                    DocumentTip = cod,
                    PaymentRefNo = doc?.Numar,
                    // Aceeași regulă ca la facturi (fixul L3): data e a rândurilor
                    // jumătății, nu a documentului.
                    TransactionDate = randuriJumatatePlata.Count > 0
                        ? randuriJumatatePlata.Min(r => r.Data)
                        : doc?.Data ?? dataStart,
                    PaymentMethod = metoda,
                    PaymentMechanism = mecanism,
                    Description = stornoPlata ? $"{descriereBaza} (storno)" : descriereBaza,
                };

                var pozitiePlata = 0;
                foreach (var l in liniiPlata) {
                    var randPlata = randuriJumatatePlata.FirstOrDefault(r => r.DetaliuId == l.ID);
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
                        AccountID = contLinie is Guid cl ? Simbol(cl) : Simbol(contTertPlata),
                        CustomerID = customer,
                        SupplierID = supplier,
                        Description = descrieri.GetValueOrDefault(l.ID) ?? plata.Description,
                        // Indicatorul rămâne al DIRECȚIEI documentului, semnul stă
                        // pe sumă — exact convenția „storno în negru" a schemei,
                        // aceeași ca pe liniile de factură (S.I.47).
                        DebitCreditIndicator = estePlata ? "D" : "C",
                        PaymentLineAmount = semnPlata * (l.Valoare + l.ValoareTva),
                        Analiza = randPlata == null ? [] : Analiza(debitLatura, randPlata),
                        TaxInformation = Nefiscal(),
                    });
                    plata.GrossTotal += semnPlata * (l.Valoare + l.ValoareTva);
                }
                rezultat.Plati.Add(plata);
            }
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
        var (produseSaft, unitatiSaft) = ProduseSiUnitati(
            os, produseFolosite.ToList(), (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Produse = produseSaft;
        rezultat.Unitati = unitatiSaft;

        // `ProductCode`/`InvoiceUOM` pe liniile de factură: identitatea pusă mai sus
        // se înlocuiește cu datele reale ale produsului.
        var codProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.ProductCode);
        var denumireProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.Description);
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
        rezultat.Taxe = TabelaTaxe(codTvaFolosit);
        rezultat.TipuriAnaliza = etichete.Lista();

        // ── 16. Cusăturile (D16-D4) ──────────────────────────────────────────
        bool EsteFactura(Guid documentId) {
            var c = CodTip(documentId);
            return TipuriVanzare.Contains(c) || TipuriCumparare.Contains(c);
        }

        // ═══ Fixul F5: faptele fiscale ale tipurilor FĂRĂ secțiune de facturi ═══
        // Un decont (DEC), o notă contabilă (NTC) sau un bon fiscal poartă TVA în
        // `RegistruTva`, dar D406 n-are unde le pune: `SalesInvoices` și
        // `PurchaseInvoices` sunt secțiuni de FACTURI, iar tipurile astea nu emit
        // una. Cifrele lor sunt în GL (rândul de TVA are cod de taxă), dar nu
        // într-un `Invoice` — și până acum nu erau nicăieri în contract.
        //
        // Consecința: cusătura 3 se măsura pe un registru RESTRÂNS la tipurile de
        // factură, adică pe exact mulțimea care intra în fișier — o egalitate
        // care se verifica pe sine. Acum registrul se citește ÎNTREG per sens, iar
        // ce nu e factură iese numit în `Neincluse`.
        foreach (var g in randuriTva
                     .Where(t => !EsteFactura(t.DocumentId))
                     .GroupBy(t => (t.DocumentId, t.Storno, t.Sens))
                     .OrderBy(g => documente.TryGetValue(g.Key.DocumentId, out var d) ? d.Data : DateOnly.MinValue)
                     .ThenBy(g => g.Key.DocumentId)
                     .ThenBy(g => g.Key.Storno)) {
            var bazaTip = g.Sum(t => t.Baza);
            var tvaTip = g.Sum(t => t.Tva);
            if (bazaTip == 0m && tvaTip == 0m)
                continue;
            if (g.Key.Sens == SensTva.Achizitie) bazaNeincluseAchizitie += bazaTip;
            else bazaNeincluseLivrare += bazaTip;
            var docTip = documente.GetValueOrDefault(g.Key.DocumentId);
            neincluse.Add(new SaftNeinclus {
                Cauza = nameof(CauzaNeincludere.TipFaraSectiuneFacturi),
                Sectiune = "SourceDocuments",
                Sens = g.Key.Sens.ToString(),
                DocumentId = g.Key.DocumentId,
                DocumentNumar = docTip?.Numar,
                DocumentTip = CodTip(g.Key.DocumentId),
                Baza = bazaTip,
                Tva = tvaTip,
                Randuri = g.Count(),
            });
        }

        // ═══ Fixul F3, cusătura 4: soldurile se compară PER CONT ═══
        var inchidereBalanta = new Dictionary<Guid, decimal>();
        foreach (var b in balanta)
            inchidereBalanta[b.ContId] = inchidereBalanta.GetValueOrDefault(b.ContId)
                + b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit;
        var conturiDiferite = 0;
        var sumaAbsolutaClosing = 0m;
        foreach (var c in rezultat.Conturi) {
            var inchidereCont = (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m);
            sumaAbsolutaClosing += Math.Abs(inchidereCont);
            if (!inchidereBalanta.TryGetValue(c.ContId, out var dinBalanta) || dinBalanta != inchidereCont)
                conturiDiferite++;
        }

        // ═══ Fixul F4: cusătura terților ═══
        // Ce declară master files (Σ `Closing` pe `Customers`) plus ce n-a putut
        // fi declarat (Σ soldurilor din `Neincluse[Customers]`) trebuie să fie
        // exact soldul conturilor de client din GLA. Aceeași ecuație pentru
        // furnizori. Fără ea, un partener care iese din agregat (repartitor care
        // nu e partener, rând fără nicio latură de partener) dispărea din fișier
        // fără ca vreo cifră să scadă undeva.
        decimal ClosingTerti(List<SaftTert> lista) =>
            lista.Sum(t => (t.ClosingDebitBalance ?? 0m) - (t.ClosingCreditBalance ?? 0m));
        decimal NeincluseTerti(string sectiune) => neincluse
            .Where(n => n.Sectiune == sectiune)
            .Sum(n => (n.Debit ?? 0m) - (n.Credit ?? 0m));
        decimal ClosingGlaRol(RolTertCont rol) => rezultat.Conturi
            .Where(c => Rol(c.ContId) == rol)
            .Sum(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m));

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
            // Fixul F5: TOATE tipurile, nu doar cele de factură — ce n-are
            // secțiune de facturi e numit în `Neincluse`, nu scos din numitor.
            BazaRegistruAchizitie = randuriTva.Where(t => t.Sens == SensTva.Achizitie).Sum(t => t.Baza),
            BazaRegistruLivrare = randuriTva.Where(t => t.Sens == SensTva.Livrare).Sum(t => t.Baza),
            ClosingGla = rezultat.Conturi.Sum(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m)),
            ClosingBalanta = balanta.Sum(b => b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit),
            ConturiVerificate = rezultat.Conturi.Count,
            ConturiDiferite = conturiDiferite,
            SumaAbsolutaClosing = sumaAbsolutaClosing,
            ClosingClienti = ClosingTerti(rezultat.Clienti),
            NeincluseClienti = NeincluseTerti("Customers"),
            ClosingGlaClienti = ClosingGlaRol(RolTertCont.Client),
            ClosingFurnizori = ClosingTerti(rezultat.Furnizori),
            NeincluseFurnizori = NeincluseTerti("Suppliers"),
            ClosingGlaFurnizori = ClosingGlaRol(RolTertCont.Furnizor),
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

    // ═══════════ D406, modulul S (stocuri) — felia 17, D17-D3 ════════════════
    //
    // ACELAȘI `SaftDto`, ALT set de secțiuni: tipul declarației vine EXCLUSIV din
    // `Header.HeaderComment` („C" = la cerere), nu dintr-un „D406S" care nu
    // există. Sursa e `RegistruStoc` — invariantul I: fișierul S e o PROIECȚIE a
    // registrului, niciun calcul nou de stoc, doar agregare.
    //
    // ═══ Ce e politică și ce e cod ═══
    // Tipul mișcării (`10` achiziție, `70` consum, `80` transfer…) e o funcție a
    // TIPULUI de document și a REGISTRULUI atins — deci politică per profil
    // (`PoliticaMiscareSaft`, D17-D1). Ce e al legii — formatul identității,
    // convenția `0`/raportor pe laturi, granularitatea per preț unitar — rămâne
    // în `SaftReguli`. Motorul și proiecția nu cunosc niciun cod hardcodat.
    //
    // ═══ Semnul pe care se potrivește politica NU e semnul rândului ═══
    // Registrul e append-only, iar stornoul scrie rândurile INVERSE la data
    // stornării (25d). Citit pe semnul brut, rândul de storno al unui bon de
    // consum (−Magazie devenit +Magazie) n-ar mai găsi nicio politică și ar ieși
    // în `Neincluse`. Politica oglindește `RegulaStoc`, deci potrivirea se face pe
    // semnul REGULII: `(Storno ? −1 : +1) × sign(Cantitate)`. Codul rămâne al
    // operației originale (un storno de plus de inventar e tot `110`), iar
    // cantitatea rămâne cea din registru — negativă, adică inversul originalului.
    //
    // ═══ Ce nu intră, în DOUĂ liste diferite ═══
    // `Excluse` = politică FĂRĂ cod, cu motiv scris de om (o alegere: `+Consum`
    // de pe BCS nu e stoc în magazie). `Neincluse` = gaură (nicio politică pe
    // cheie, sau produs fără cont de stoc). Un singur sac le-ar fi confundat, iar
    // „nimic nu se pierde" (invariantul V) cere ca ele să nu arate la fel.
    public static SaftDto SaftStocuri(IObjectSpace os, int an, int luna, DateOnly? dataCreare = null) {
        var rezultat = new SaftDto {
            An = an,
            Luna = luna,
            DataStart = new DateOnly(an, luna, 1),
            DataEnd = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna)),
        };

        // ── 0. Profilul: și S e NEAPLICABIL la bugetar (73c) ─────────────────
        rezultat.Neaplicabil = MotivNeaplicabil(os, " S");
        if (rezultat.Neaplicabil != null)
            return rezultat;

        var dataStart = rezultat.DataStart;
        var dataEnd = rezultat.DataEnd;

        var avertismente = new Dictionary<CodAvertismentSaft, List<(string Exemplu, decimal? Suma)>>();
        void Avert(CodAvertismentSaft cod, string exemplu, decimal? suma = null) {
            if (!avertismente.TryGetValue(cod, out var lista))
                lista = avertismente[cod] = [];
            lista.Add((exemplu, suma));
        }
        var neincluse = new List<SaftNeinclus>();

        // ── 1. Societatea raportoare (antet + `OwnerID` + laturile interne) ──
        var soc = CitesteSocietate(os);
        VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));
        var idSocietate = SaftReguli.IdSocietate(soc?.CodFiscal, soc?.Tara);
        // `Owners` rămâne GOL: tot stocul e al raportorului (ghid p. 36). Terții
        // cu `8038` n-au sursă în model azi — restanță cu nume, nu invenție.
        var ownerId = SaftReguli.OwnerIdRaportor(soc?.CodFiscal, soc?.Tara);
        rezultat.Header = Antet(soc, an, luna, dataCreare, HeaderCommentStocuri);

        // ── 2–3. Planul de conturi + `GeneralLedgerAccounts` (comune cu L) ───
        var conturi = CitesteConturi(os);
        var (conturiSaft, balanta) = ConturiSiSolduri(
            os, dataStart, dataEnd, conturi, (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Conturi = conturiSaft;

        // ── 4. Politica de mișcare: `(TipDocument × TipStoc × Semn?) → cod` ──
        var tipuriDocument = os.GetObjectsQuery<TipDocument>()
            .Select(t => new { t.ID, t.Cod }).ToList();
        var idTipPeCod = tipuriDocument
            .Where(t => t.Cod != null)
            .GroupBy(t => t.Cod, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().ID, StringComparer.Ordinal);

        var politici = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .Select(p => new { p.TipDocumentId, p.TipStoc, p.Semn, p.CodMiscare, p.RolTert, p.Motiv })
            .ToList()
            .Select(p => new RegulaMiscare {
                TipDocumentId = p.TipDocumentId, TipStoc = p.TipStoc, Semn = p.Semn,
                Cod = p.CodMiscare, Rol = p.RolTert, Motiv = p.Motiv,
                CodTipDocument = tipuriDocument.FirstOrDefault(t => t.ID == p.TipDocumentId)?.Cod,
            })
            .ToList();
        // Unicitatea e a bazei (două indexuri filtrate, D17-D1) — dicționarele o
        // presupun, nu o reverifică.
        var politiciExacte = politici.Where(p => p.Semn != null)
            .ToDictionary(p => (p.TipDocumentId, p.TipStoc, p.Semn.Value));
        var politiciGenerice = politici.Where(p => p.Semn == null)
            .ToDictionary(p => (p.TipDocumentId, p.TipStoc));
        RegulaMiscare Potriveste(Guid tipId, TipStoc tipStoc, int semnRegula) =>
            semnRegula != 0 && politiciExacte.TryGetValue((tipId, tipStoc, semnRegula), out var exact)
                ? exact
                : politiciGenerice.GetValueOrDefault((tipId, tipStoc));

        // `TipStoc`-urile RAPORTATE = cele care apar în politici CU COD. Soldurile
        // de `Consum`/`Folosinta` nu sunt patrimoniu în magazie, deci
        // `PhysicalStock` nu le declară.
        var raportate = politici.Where(p => p.Cod != null).Select(p => p.TipStoc).Distinct().ToList();
        var raportateSet = raportate.ToHashSet();

        // ── 5. Rândurile de registru ale lunii (TOATE `TipStoc`: numitorul S2) ──
        // `DocumentId != null`: rândurile de DESCHIDERE (25e/34d) sunt sold, nu
        // mișcare — ele intră în `Opening`, nu în `MovementOfGoods`.
        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd && r.DocumentId != null)
            .Select(r => new RandStoc {
                Id = r.ID, Data = r.Data, TipStoc = r.TipStoc, LotId = r.LotId,
                RepartitorId = r.RepartitorId, Cantitate = r.Cantitate, Valoare = r.Valoare,
                Storno = r.Storno, DocumentId = r.DocumentId.Value, DetaliuId = r.DetaliuId,
            })
            .ToList();

        // ── 6. Soldurile: DOUĂ interogări GRUPATE, nu una per lot ────────────
        // Cardinalitatea rezultatului e a stocului (gestiune × lot), nu a
        // registrului: pe o lună reală registrul are zeci de mii de rânduri, iar
        // un `SoldStoc` per intrare ar fi fost N interogări.
        var deschideri = AgregatStoc(os, raportate, dataStart.AddDays(-1));
        var inchideri = AgregatStoc(os, raportate, dataEnd);

        // Soldurile pe `TipStoc`-uri pe care declarația NU le raportează: n-au
        // document, deci nu pot fi `Neincluse` — dar nici n-au voie să dispară.
        foreach (var g in os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
                     .Where(r => r.Data <= dataEnd && !raportate.Contains(r.TipStoc))
                     .GroupBy(r => r.TipStoc)
                     .Select(g => new {
                         TipStoc = g.Key,
                         Cantitate = g.Sum(r => r.Cantitate),
                         Valoare = g.Sum(r => r.Valoare),
                         Randuri = g.Count(),
                     })
                     .ToList()
                     .OrderBy(x => x.TipStoc)) {
            if (g.Cantitate == 0m && g.Valoare == 0m)
                continue;
            Avert(CodAvertismentSaft.SoldPeTipStocNeraportat,
                $"`{g.TipStoc}`: {g.Randuri} rânduri de registru, sold {g.Cantitate:0.###} / {g.Valoare:0.00} lei "
                + "la sfârșitul perioadei — niciun tip de document nu produce cod de mișcare pe registrul ăsta, "
                + "deci `PhysicalStock` nu-l declară.", g.Valoare);
        }

        // ── 7. Documentele mișcărilor (tipul POLIMORF, într-un query — 60b) ──
        var idsDocumente = randuriStoc.Select(r => r.DocumentId).Distinct().ToList();
        var codPerDocument = ApiProiectii.CoduriTip(os, idsDocumente);
        var documente = os.GetObjectsQuery<Document>()
            .Where(d => idsDocumente.Contains(d.ID))
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataOperare, d.PredatorId, d.PrimitorId,
                d.DocumentSursaId, d.Autogenerat
            })
            .ToList()
            .ToDictionary(d => d.ID);
        // Partenerul unui conex (NIR ← FCT, DSC ← FCL) e pe SURSĂ: NIR-ul are
        // gestiunea pe ambele laturi, factura are furnizorul. Un NIR MANUAL n-are
        // sursă — și atunci raportorul pe ambele laturi + avertisment, nu refuz.
        var idsSursa = documente.Values
            .Where(d => d.Autogenerat && d.DocumentSursaId != null)
            .Select(d => d.DocumentSursaId.Value).Distinct().ToList();
        var surse = idsSursa.Count == 0
            ? []
            : os.GetObjectsQuery<Document>()
                .Where(d => idsSursa.Contains(d.ID))
                .Select(d => new { d.ID, d.PredatorId, d.PrimitorId })
                .ToList()
                .ToDictionary(d => d.ID, d => (d.PredatorId, d.PrimitorId));

        // ── 8. Repartitorii: laturile documentelor, gestiunile, centrele de cost ──
        IQueryable<RegistruContabil> Gl() => os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd);
        var idsCentruCost = Gl().Select(r => r.DebitCentruCostId)
            .Concat(Gl().Select(r => r.CreditCentruCostId)).Distinct().ToList();

        var idsRepartitor = new HashSet<Guid>();
        void AdaugaRep(Guid? id) { if (id is Guid v) idsRepartitor.Add(v); }
        foreach (var d in documente.Values) { idsRepartitor.Add(d.PredatorId); idsRepartitor.Add(d.PrimitorId); }
        foreach (var s in surse.Values) { idsRepartitor.Add(s.PredatorId); idsRepartitor.Add(s.PrimitorId); }
        foreach (var r in randuriStoc) idsRepartitor.Add(r.RepartitorId);
        foreach (var a in deschideri) idsRepartitor.Add(a.RepartitorId);
        foreach (var a in inchideri) idsRepartitor.Add(a.RepartitorId);
        foreach (var id in idsCentruCost) AdaugaRep(id);
        var listaRep = idsRepartitor.ToList();

        var repartitori = os.GetObjectsQuery<Repartitor>().IgnoreQueryFilters()
            .Where(r => listaRep.Contains(r.ID))
            .Select(r => new { r.ID, r.Cod, r.Denumire })
            .ToList()
            .ToDictionary(r => r.ID, r => (r.Cod, r.Denumire));
        // Ca la D394/L: partenerul ȘTERS logic se declară — documentele lui sunt
        // operate, iar fișierul nu depinde de viața nomenclatorului.
        var parteneri = os.GetObjectsQuery<Partener>().IgnoreQueryFilters()
            .Where(p => listaRep.Contains(p.ID))
            .Select(p => new {
                p.ID, p.Cod, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare
            })
            .ToList()
            .ToDictionary(p => p.ID, p => new InfoPartener {
                Id = p.ID, Cod = p.Cod, Denumire = p.Denumire, CodFiscal = p.CodFiscal,
                TipPersoana = p.TipPersoana, Tara = p.Tara,
                InregistratTva = p.InregistratTva, TvaLaIncasare = p.TvaLaIncasare,
            });
        var raporteazaCnp = soc?.RaporteazaCnp ?? false;
        foreach (var p in parteneri.Values) {
            var identitate = SaftReguli.IdPartener(
                p.TipPersoana, p.Tara, p.InregistratTva, p.CodFiscal, p.Cod, p.Id, raporteazaCnp);
            p.Id406 = identitate.Id;
            p.Fel = identitate.Fel;
        }

        // ── 9. Loturile și produsele atinse (mișcări + stoc fizic) ───────────
        var idsLot = randuriStoc.Select(r => r.LotId)
            .Concat(deschideri.Select(a => a.LotId))
            .Concat(inchideri.Select(a => a.LotId))
            .Distinct().ToList();
        var loturi = os.GetObjectsQuery<Lot>().IgnoreQueryFilters()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId, l.PretUnitar })
            .ToList()
            .ToDictionary(l => l.ID, l => (l.ProdusId, l.PretUnitar));
        var idsProdus = loturi.Values.Select(l => l.ProdusId).Distinct().ToList();
        // Contul de stoc al produsului: `TipMaterial.ContImplicit` (26b — maparea
        // e DATE). `ProductType` pe stocul fizic, `AccountID` pe linia de mișcare.
        var produseCont = os.GetObjectsQuery<Produs>().IgnoreQueryFilters()
            .Where(p => idsProdus.Contains(p.ID))
            .Select(p => new { p.ID, p.Cod, p.Denumire, ContSimbol = p.TipMaterial.ContImplicit.Simbol })
            .ToList()
            .ToDictionary(p => p.ID, p => (p.Cod, p.Denumire, p.ContSimbol));

        string SimbolStoc(Guid produsId) =>
            produseCont.TryGetValue(produsId, out var p) ? SaftReguli.SimbolSaft(p.ContSimbol) : null;
        string NumeProdus(Guid produsId) =>
            produseCont.TryGetValue(produsId, out var p) ? (p.Denumire ?? p.Cod ?? produsId.ToString()) : produsId.ToString();

        // ── 10. `PhysicalStock` — o intrare per (gestiune × lot) ─────────────
        var deschidereCheie = deschideri.ToDictionary(a => (a.RepartitorId, a.LotId), a => (a.Cantitate, a.Valoare));
        var inchidereCheie = inchideri.ToDictionary(a => (a.RepartitorId, a.LotId), a => (a.Cantitate, a.Valoare));
        // Prezența unei mișcări în lună ține intrarea în fișier chiar dacă
        // deschiderea și închiderea sunt zero (a intrat și a ieșit tot în lună).
        var cuMiscare = randuriStoc.Where(r => raportateSet.Contains(r.TipStoc))
            .Select(r => (r.RepartitorId, r.LotId)).ToHashSet();
        var chei = deschidereCheie.Keys.Concat(inchidereCheie.Keys).Concat(cuMiscare).Distinct().ToList();

        var produseFolosite = new HashSet<Guid>();
        // Avertismentul e per PRODUS, nu per apariție (aceeași lecție ca la
        // adresele incomplete din L): același produs are o intrare de stoc în
        // fiecare gestiune, iar numărate, ele ar fi spus „patru produse fără cont”
        // despre unul singur.
        var faraContStrigat = new HashSet<Guid>();
        var stocFizic = new List<SaftStocFizic>();
        foreach (var cheie in chei) {
            var deschidere = deschidereCheie.GetValueOrDefault(cheie);
            var inchidere = inchidereCheie.GetValueOrDefault(cheie);
            if (deschidere.Cantitate == 0m && deschidere.Valoare == 0m
                    && inchidere.Cantitate == 0m && inchidere.Valoare == 0m
                    && !cuMiscare.Contains(cheie))
                continue;
            if (!loturi.TryGetValue(cheie.Item2, out var lot))
                continue;
            var simbol = SimbolStoc(lot.ProdusId);
            if (string.IsNullOrEmpty(simbol) && faraContStrigat.Add(lot.ProdusId))
                Avert(CodAvertismentSaft.ProdusFaraContStoc,
                    $"Produsul „{NumeProdus(lot.ProdusId)}” n-are cont de stoc (`TipMaterial.ContImplicit`) — "
                    + $"`ProductType` iese „{SaftReguli.ProductTypeImplicit}”.");
            if (inchidere.Cantitate < 0m || inchidere.Valoare < 0m)
                Avert(CodAvertismentSaft.SoldNegativ,
                    $"„{NumeProdus(lot.ProdusId)}” în {EtichetaRepartitor(repartitori, cheie.Item1)}: sold final "
                    + $"{inchidere.Cantitate:0.###} / {inchidere.Valoare:0.00} lei — se declară CA ATARE "
                    + "(gardianul de sold, 25d, n-ar trebui să-l lase să existe).", inchidere.Valoare);
            produseFolosite.Add(lot.ProdusId);
            stocFizic.Add(new SaftStocFizic {
                RepartitorId = cheie.Item1,
                LotId = cheie.Item2,
                ProdusId = lot.ProdusId,
                WarehouseId = EtichetaRepartitor(repartitori, cheie.Item1),
                ProductType = SaftReguli.ProductTypeDinCont(simbol),
                OwnerId = ownerId,
                UomConversionFactor = 1m,
                // 73-r11: `UnitPrice` la 2 zecimale, deși lotul poartă 6 (Scara).
                UnitPrice = Math.Round(lot.PretUnitar, 2, MidpointRounding.AwayFromZero),
                OpeningQuantity = deschidere.Cantitate,
                OpeningValue = deschidere.Valoare,
                ClosingQuantity = inchidere.Cantitate,
                ClosingValue = inchidere.Valoare,
                StockCharacteristic = SaftReguli.StockCharacteristic.Cheie,
                StockCharacteristicValue = SaftReguli.StockCharacteristic.Valoare,
            });
        }
        // `StockAccountNo` = identificatorul lotului, DOAR când produsul are mai
        // multe intrări în ACEEAȘI gestiune (ghid p. 36): cu un singur lot, câmpul
        // n-ar distinge nimic, iar schema îl lasă opțional.
        var loturiPerProdusGestiune = stocFizic
            .GroupBy(e => (e.RepartitorId, e.ProdusId))
            .ToDictionary(g => g.Key, g => g.Select(e => e.LotId).Distinct().Count());
        bool CereStockAccountNo(Guid repartitorId, Guid produsId) =>
            loturiPerProdusGestiune.GetValueOrDefault((repartitorId, produsId)) > 1;
        foreach (var e in stocFizic)
            if (CereStockAccountNo(e.RepartitorId, e.ProdusId))
                e.StockAccountNo = e.LotId.ToString();

        // ── 11. `MovementOfGoods` — potrivirea pe politică, apoi gruparea ────
        var emise = new List<(RandStoc Rand, RegulaMiscare Regula)>();
        var excluse = new Dictionary<(Guid, TipStoc, int?), SaftExclus>();
        var faraPolitica = new Dictionary<(string, TipStoc, int), SaftNeinclus>();
        foreach (var r in randuriStoc) {
            var codTip = codPerDocument.GetValueOrDefault(r.DocumentId);
            // Semnul REGULII, nu al rândului (vezi antetul metodei).
            var semn = (r.Storno ? -1 : 1) * Math.Sign(r.Cantitate != 0m ? r.Cantitate : r.Valoare);
            var regula = codTip != null && idTipPeCod.TryGetValue(codTip, out var tipId)
                ? Potriveste(tipId, r.TipStoc, semn)
                : null;
            if (regula == null) {
                var cheie = (codTip ?? "(tip necunoscut)", r.TipStoc, semn);
                if (!faraPolitica.TryGetValue(cheie, out var n)) {
                    documente.TryGetValue(r.DocumentId, out var d);
                    n = faraPolitica[cheie] = new SaftNeinclus {
                        Cauza = nameof(CauzaNeincludere.FaraCodMiscare),
                        Sectiune = "MovementOfGoods",
                        DocumentId = r.DocumentId,
                        DocumentNumar = d?.Numar,
                        DocumentTip = cheie.Item1,
                        TipStoc = r.TipStoc.ToString(),
                        Semn = semn,
                        Cantitate = 0m, Valoare = 0m,
                    };
                }
                n.Cantitate += r.Cantitate;
                n.Valoare += r.Valoare;
                n.Randuri++;
                continue;
            }
            if (regula.Cod == null) {
                var cheie = (regula.TipDocumentId, regula.TipStoc, regula.Semn);
                if (!excluse.TryGetValue(cheie, out var x))
                    x = excluse[cheie] = new SaftExclus {
                        TipDocument = regula.CodTipDocument,
                        TipStoc = regula.TipStoc.ToString(),
                        Semn = regula.Semn,
                        Motiv = regula.Motiv,
                    };
                x.Numar++;
                x.Cantitate += r.Cantitate;
                x.Valoare += r.Valoare;
                continue;
            }
            emise.Add((r, regula));
        }

        // Un document se SPARGE când poartă mai multe coduri pe aceeași jumătate
        // (ASM: `20` pe produs, `70` pe consumuri) — abia atunci referința are
        // nevoie de sufixul de cod ca să rămână unică.
        var coduriPerDocument = emise
            .GroupBy(x => (x.Rand.DocumentId, x.Rand.Storno))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Regula.Cod).Distinct().Count());

        Guid? PartenerulMiscarii(Guid documentId) {
            if (!documente.TryGetValue(documentId, out var d))
                return null;
            var propriu = PartenerulDocumentului(d.PredatorId, d.PrimitorId, parteneri);
            if (propriu != null)
                return propriu;
            if (d.Autogenerat && d.DocumentSursaId is Guid sursaId && surse.TryGetValue(sursaId, out var s))
                return PartenerulDocumentului(s.PredatorId, s.PrimitorId, parteneri);
            return null;
        }

        var faraContStoc = new Dictionary<Guid, SaftNeinclus>();
        var tertLipsaStrigat = new HashSet<Guid>();
        var referinteTrunchiate = 0;
        var coduriFolosite = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var g in emise
                     .GroupBy(x => (x.Rand.DocumentId, x.Rand.Storno, x.Regula.Cod))
                     .OrderBy(g => g.Min(x => x.Rand.Data))
                     .ThenBy(g => g.Key.DocumentId)
                     .ThenBy(g => g.Key.Storno)
                     .ThenBy(g => g.Key.Cod, StringComparer.Ordinal)) {
            documente.TryGetValue(g.Key.DocumentId, out var doc);
            var codTip = codPerDocument.GetValueOrDefault(g.Key.DocumentId);
            var numar = doc?.Numar;
            if (string.IsNullOrWhiteSpace(numar))
                numar = g.Key.DocumentId.ToString("N");
            var seSparge = coduriPerDocument.GetValueOrDefault((g.Key.DocumentId, g.Key.Storno)) > 1;
            var (referinta, trunchiat) = SaftReguli.MovementReference(
                codTip, numar, seSparge ? g.Key.Cod : null, g.Key.Storno);
            if (trunchiat) {
                referinteTrunchiate++;
                Avert(CodAvertismentSaft.MovementReferenceTrunchiat,
                    $"{codTip} {numar} ⇒ `MovementReference` „{referinta}” "
                    + $"(max {SaftReguli.LungimeMovementReference} caractere).");
            }

            var rolul = g.First().Regula.Rol;
            var partenerId = PartenerulMiscarii(g.Key.DocumentId);
            string idPartener = null;
            if (partenerId is Guid pid && parteneri.TryGetValue(pid, out var infoPartener))
                idPartener = infoPartener.Id406;
            if (rolul != RolTertSaft.Niciunul && idPartener == null
                    && tertLipsaStrigat.Add(g.Key.DocumentId))
                Avert(CodAvertismentSaft.TertLipsaPeMiscare,
                    $"{codTip} {numar}: politica cere rolul „{rolul}”, dar documentul n-are niciun partener pe "
                    + "laturi (nici pe ale sursei) — `CustomerID`/`SupplierID` ies cu identitatea raportorului.");
            var (customerId, supplierId) = SaftReguli.TertiLinieStoc(rolul, idPartener, idSocietate);

            var linii = new List<SaftLinieMiscareStoc>();
            var pozitie = 0;
            foreach (var (r, _) in g.OrderBy(x => x.Rand.Id)) {
                var produsId = loturi.TryGetValue(r.LotId, out var lot) ? lot.ProdusId : Guid.Empty;
                var simbol = produsId == Guid.Empty ? null : SimbolStoc(produsId);
                if (string.IsNullOrEmpty(simbol)) {
                    // `MovementLine.AccountID` e obligatoriu, iar un cont inventat
                    // e interzis (73e): linia iese din fișier, cu cifrele ei.
                    if (!faraContStoc.TryGetValue(produsId, out var n))
                        n = faraContStoc[produsId] = new SaftNeinclus {
                            Cauza = nameof(CauzaNeincludere.FaraContStoc),
                            Sectiune = "MovementOfGoods",
                            DocumentId = g.Key.DocumentId,
                            DocumentNumar = numar,
                            DocumentTip = codTip,
                            ProdusId = produsId == Guid.Empty ? null : produsId,
                            ProdusCod = produsId == Guid.Empty ? null
                                : produseCont.GetValueOrDefault(produsId).Cod,
                            TipStoc = r.TipStoc.ToString(),
                            Cantitate = 0m, Valoare = 0m,
                        };
                    n.Cantitate += r.Cantitate;
                    n.Valoare += r.Valoare;
                    n.Randuri++;
                    continue;
                }
                produseFolosite.Add(produsId);
                linii.Add(new SaftLinieMiscareStoc {
                    RandRegistruId = r.Id,
                    DetaliuId = r.DetaliuId,
                    LineNumber = ++pozitie,
                    AccountId = simbol,
                    CustomerId = customerId,
                    SupplierId = supplierId,
                    ProdusId = produsId,
                    LotId = r.LotId,
                    RepartitorId = r.RepartitorId,
                    StockAccountNo = CereStockAccountNo(r.RepartitorId, produsId) ? r.LotId.ToString() : null,
                    // SEMNATĂ ca în registru: intrare +, ieșire −, stornoul inversat
                    // (riscul 1 al contractului, măsurat cu DUK în V3).
                    Quantity = r.Cantitate,
                    UomConversionFactor = 1m,
                    BookValue = r.Valoare,
                    // Același nomenclator pentru tip și subtip: modelul n-are o a
                    // doua axă, iar un subtip inventat ar fi zgomot.
                    MovementSubType = g.Key.Cod,
                });
            }
            if (linii.Count == 0)
                continue;
            coduriFolosite.TryAdd(g.Key.Cod, SaftReguli.CoduriMiscare.GetValueOrDefault(g.Key.Cod, g.Key.Cod));
            rezultat.MiscariStoc.Add(new SaftMiscareStoc {
                DocumentId = g.Key.DocumentId,
                Storno = g.Key.Storno,
                MovementReference = referinta,
                MovementDate = g.Min(x => x.Rand.Data),
                MovementPostingDate = doc?.DataOperare is DateTime op ? DateOnly.FromDateTime(op) : null,
                MovementType = g.Key.Cod,
                DocumentType = codTip,
                DocumentNumber = doc?.Numar,
                TransactionId = g.Key.DocumentId.ToString(),
                Linii = linii,
            });
        }

        rezultat.StocFizic = stocFizic
            .OrderBy(e => e.WarehouseId ?? "", StringComparer.Ordinal)
            .ThenBy(e => e.ProdusId)
            .ThenBy(e => e.LotId)
            .ToList();
        rezultat.TipuriMiscare = coduriFolosite
            .OrderBy(c => c.Key.Length).ThenBy(c => c.Key, StringComparer.Ordinal)
            .Select(c => new SaftTipMiscare { Cod = c.Key, Descriere = c.Value })
            .ToList();
        rezultat.Excluse = excluse.Values
            .OrderBy(x => x.TipDocument ?? "", StringComparer.Ordinal)
            .ThenBy(x => x.TipStoc ?? "", StringComparer.Ordinal)
            .ThenBy(x => x.Semn ?? 0)
            .ToList();
        neincluse.AddRange(faraPolitica.Values);
        neincluse.AddRange(faraContStoc.Values);

        rezultat.NumberOfMovementLines = rezultat.MiscariStoc.Sum(m => m.Linii.Count);
        rezultat.TotalQuantityReceived = rezultat.MiscariStoc
            .SelectMany(m => m.Linii).Where(l => l.Quantity > 0m).Sum(l => l.Quantity);
        rezultat.TotalQuantityIssued = Math.Abs(rezultat.MiscariStoc
            .SelectMany(m => m.Linii).Where(l => l.Quantity < 0m).Sum(l => l.Quantity));

        // ── 12. `Products` + `UOMTable` (comune cu L) ────────────────────────
        var (produseSaft, unitatiSaft) = ProduseSiUnitati(
            os, produseFolosite.ToList(), (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Produse = produseSaft;
        rezultat.Unitati = unitatiSaft;
        var codProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.ProductCode);
        var umProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.UOMBase);
        var ncProdus = rezultat.Produse.ToDictionary(p => p.ProdusId, p => p.ProductCommodityCode);
        foreach (var e in rezultat.StocFizic) {
            e.ProductCode = codProdus.GetValueOrDefault(e.ProdusId) ?? e.ProdusId.ToString();
            e.UomPhysicalStock = umProdus.GetValueOrDefault(e.ProdusId) ?? UnitateImplicita;
            e.StockAccountCommodityCode = ncProdus.GetValueOrDefault(e.ProdusId) ?? CodNcImplicit;
        }
        foreach (var l in rezultat.MiscariStoc.SelectMany(m => m.Linii)) {
            l.ProductCode = codProdus.GetValueOrDefault(l.ProdusId) ?? l.ProdusId.ToString();
            l.UnitOfMeasure = umProdus.GetValueOrDefault(l.ProdusId) ?? UnitateImplicita;
        }

        // ── 13. `TaxTable` + `AnalysisTypeTable` (perioada, ca la L) ─────────
        // S nu emite `TaxInformation` și nici `Analysis` pe linii, dar secțiunile
        // sunt așteptate pentru `HeaderComment = C` (validator + ghid p. 42):
        // codurile de taxă FOLOSITE în perioadă și dimensiunile ei.
        var tipuriTva = os.GetObjectsQuery<TipTva>()
            .Select(t => new { t.ID, t.Denumire, t.Cota, t.CodSafTLivrare, t.CodSafTAchizitie })
            .ToList()
            .ToDictionary(t => t.ID);
        var codTvaFolosit = new Dictionary<string, (decimal Cota, string Denumire)>(StringComparer.Ordinal);
        foreach (var p in os.GetObjectsQuery<RegistruTva>()
                     .Where(r => r.Data >= dataStart && r.Data <= dataEnd)
                     .Select(r => new { r.TipTvaId, r.Sens })
                     .Distinct()
                     .ToList()) {
            if (!tipuriTva.TryGetValue(p.TipTvaId, out var t))
                continue;
            var cod = p.Sens == SensTva.Livrare ? t.CodSafTLivrare : t.CodSafTAchizitie;
            if (!string.IsNullOrWhiteSpace(cod))
                codTvaFolosit.TryAdd(cod, (t.Cota, t.Denumire));
        }
        rezultat.Taxe = TabelaTaxe(codTvaFolosit);

        var etichete = new EtichetePerioada(os, repartitori);
        void Axa(string tip,
            System.Linq.Expressions.Expression<Func<RegistruContabil, Guid?>> peDebit,
            System.Linq.Expressions.Expression<Func<RegistruContabil, Guid?>> peCredit) {
            foreach (var id in Gl().Select(peDebit).Concat(Gl().Select(peCredit)).Distinct().ToList())
                if (id is Guid v)
                    etichete.Inregistreaza(tip, v);
        }
        Axa("CC", r => r.DebitCentruCostId, r => r.CreditCentruCostId);
        Axa("P", r => r.DebitProiectId, r => r.CreditProiectId);
        Axa("U", r => r.DebitUnitateId, r => r.CreditUnitateId);
        Axa("SF", r => r.DebitSursaFinantareId, r => r.CreditSursaFinantareId);
        Axa("CF", r => r.DebitCodFunctionalId, r => r.CreditCodFunctionalId);
        Axa("CE", r => r.DebitCodEconomicId, r => r.CreditCodEconomicId);
        rezultat.TipuriAnaliza = etichete.Lista();

        // ── 14. Cusăturile S1–S4 ─────────────────────────────────────────────
        // Validatorul ANAF nu face NICIO aritmetică pe stocuri: Opening + intrări
        // − ieșiri = Closing nu e verificat nicăieri, iar totalurile n-au regulă.
        // Deci cusăturile de aici sunt singura garanție că fișierul spune adevărul.

        // (S1) Per intrare: `Opening + Σ mișcările lunii == Closing`, pe TipStoc-urile
        // RAPORTATE. Σ se ia din REGISTRU (nu din liniile emise): rândurile care
        // n-au intrat în fișier sunt numite în `Neincluse`, iar S2 e cusătura lor
        // — S1 verifică aritmetica celor trei interogări (deschidere, lună,
        // închidere), care sunt independente una de alta.
        var miscariPeCheie = new Dictionary<(Guid, Guid), (decimal Cantitate, decimal Valoare)>();
        foreach (var r in randuriStoc.Where(r => raportateSet.Contains(r.TipStoc))) {
            var cheie = (r.RepartitorId, r.LotId);
            var acumulat = miscariPeCheie.GetValueOrDefault(cheie);
            miscariPeCheie[cheie] = (acumulat.Cantitate + r.Cantitate, acumulat.Valoare + r.Valoare);
        }
        var intrariDiferite = 0;
        foreach (var e in rezultat.StocFizic) {
            var miscare = miscariPeCheie.GetValueOrDefault((e.RepartitorId, e.LotId));
            if (e.OpeningQuantity + miscare.Cantitate != e.ClosingQuantity
                    || e.OpeningValue + miscare.Valoare != e.ClosingValue)
                intrariDiferite++;
        }
        var sumaMiscariRaportate = miscariPeCheie.Values
            .Aggregate((Cantitate: 0m, Valoare: 0m),
                (acc, x) => (acc.Cantitate + x.Cantitate, acc.Valoare + x.Valoare));

        // (S3) Σ `ClosingStockValue` per cont de stoc vs. `Balanta` pe același
        // cont — RAPORTATĂ, nu blocantă (registrul contabil poate purta 3xx și din
        // note contabile ori deschideri fără lot).
        var balantaPeSimbol = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var b in balanta) {
            var simbol = SaftReguli.ProductTypeDinCont(b.ContSimbol);
            balantaPeSimbol[simbol] = balantaPeSimbol.GetValueOrDefault(simbol)
                + b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit;
        }
        var perCont = rezultat.StocFizic
            .GroupBy(e => e.ProductType, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => {
                var stoc = g.Sum(e => e.ClosingValue);
                var dinBalanta = balantaPeSimbol.GetValueOrDefault(g.Key);
                return new SaftDiferentaCont {
                    Cont = g.Key,
                    ClosingStocFizic = stoc,
                    ClosingBalanta = dinBalanta,
                    Diferenta = stoc - dinBalanta,
                };
            })
            .ToList();

        // (S4) Integritatea referințelor din fișier.
        var coduriProdus = rezultat.Produse.Select(p => p.ProductCode).ToHashSet(StringComparer.Ordinal);
        var referiteProdus = rezultat.StocFizic.Select(e => e.ProductCode)
            .Concat(rezultat.MiscariStoc.SelectMany(m => m.Linii).Select(l => l.ProductCode))
            .Where(c => c != null).Distinct(StringComparer.Ordinal).ToList();
        var coduriDeclarate = rezultat.TipuriMiscare.Select(t => t.Cod).ToHashSet(StringComparer.Ordinal);
        var coduriReferite = rezultat.MiscariStoc.Select(m => m.MovementType)
            .Concat(rezultat.MiscariStoc.SelectMany(m => m.Linii).Select(l => l.MovementSubType))
            .Where(c => c != null).Distinct(StringComparer.Ordinal).ToList();
        var identitatiInvalide = rezultat.MiscariStoc.SelectMany(m => m.Linii)
            .SelectMany(l => new[] { l.CustomerId, l.SupplierId })
            .Concat(rezultat.StocFizic.Select(e => e.OwnerId))
            .Count(id => !IdentitateTertValida(id));

        var miscariCantitate = rezultat.MiscariStoc.SelectMany(m => m.Linii).Sum(l => l.Quantity);
        var miscariValoare = rezultat.MiscariStoc.SelectMany(m => m.Linii).Sum(l => l.BookValue);
        var excluseCantitate = rezultat.Excluse.Sum(x => x.Cantitate);
        var excluseValoare = rezultat.Excluse.Sum(x => x.Valoare);
        var neincluseCantitate = neincluse.Sum(n => n.Cantitate ?? 0m);
        var neincluseValoare = neincluse.Sum(n => n.Valoare ?? 0m);
        var registruCantitate = randuriStoc.Sum(r => r.Cantitate);
        var registruValoare = randuriStoc.Sum(r => r.Valoare);

        rezultat.Rezumat = new SaftRezumat {
            NumarMiscari = rezultat.MiscariStoc.Count,
            NumarLiniiMiscare = rezultat.NumberOfMovementLines,
            NumarStocFizic = rezultat.StocFizic.Count,
            NumarTipuriMiscare = rezultat.TipuriMiscare.Count,
            RanduriRegistruStoc = randuriStoc.Count,
            NumarProduse = rezultat.Produse.Count,

            StocIntrari = rezultat.StocFizic.Count,
            StocIntrariDiferite = intrariDiferite,
            StocOpeningCantitate = rezultat.StocFizic.Sum(e => e.OpeningQuantity),
            StocOpeningValoare = rezultat.StocFizic.Sum(e => e.OpeningValue),
            StocMiscariCantitate = sumaMiscariRaportate.Cantitate,
            StocMiscariValoare = sumaMiscariRaportate.Valoare,
            StocClosingCantitate = rezultat.StocFizic.Sum(e => e.ClosingQuantity),
            StocClosingValoare = rezultat.StocFizic.Sum(e => e.ClosingValue),
            StocFizicBate = intrariDiferite == 0,

            MiscariCantitate = miscariCantitate,
            MiscariValoare = miscariValoare,
            ExcluseCantitate = excluseCantitate,
            ExcluseValoare = excluseValoare,
            NeincluseStocCantitate = neincluseCantitate,
            NeincluseStocValoare = neincluseValoare,
            RegistruStocCantitate = registruCantitate,
            RegistruStocValoare = registruValoare,
            RegistruStocBate =
                miscariCantitate + excluseCantitate + neincluseCantitate == registruCantitate
                && miscariValoare + excluseValoare + neincluseValoare == registruValoare,

            StocPerCont = perCont,
            ClosingStocFizic = perCont.Sum(c => c.ClosingStocFizic),
            ClosingBalantaStoc = perCont.Sum(c => c.ClosingBalanta),
            ConturiStocVerificate = perCont.Count,
            ConturiStocDiferite = perCont.Count(c => c.Diferenta != 0m),

            ProduseReferite = referiteProdus.Count,
            ProduseLipsa = referiteProdus.Count(c => !coduriProdus.Contains(c)),
            CoduriMiscareFolosite = coduriReferite.Count,
            CoduriMiscareLipsa = coduriReferite.Count(c => !coduriDeclarate.Contains(c)),
            IdentitatiTertInvalide = identitatiInvalide,
            ReferinteBat = referiteProdus.All(coduriProdus.Contains)
                && coduriReferite.All(coduriDeclarate.Contains)
                && identitatiInvalide == 0,
        };

        // ── 15. Avertismentele, AGREGATE per cauză ───────────────────────────
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
            .ThenBy(n => n.DocumentTip ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.TipStoc ?? "", StringComparer.Ordinal)
            .ThenBy(n => n.ProdusCod ?? "", StringComparer.Ordinal)
            .ToList();
        return rezultat;
    }

    // Rândul de `RegistruStoc`, PLAT (aceeași rațiune ca `RandGl`: `Select` +
    // `IgnoreAutoIncludes` = o singură interogare, fără navigațiile registrului).
    sealed class RandStoc {
        public Guid Id { get; set; }
        public DateOnly Data { get; set; }
        public TipStoc TipStoc { get; set; }
        public Guid LotId { get; set; }
        public Guid RepartitorId { get; set; }
        public decimal Cantitate { get; set; }
        public decimal Valoare { get; set; }
        public bool Storno { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? DetaliuId { get; set; }
    }

    // Un rând de `PoliticaMiscareSaft`, citit plat + codul tipului de document
    // (identitatea pe care o citește omul în `Excluse`).
    sealed class RegulaMiscare {
        public Guid TipDocumentId;
        public string CodTipDocument;
        public TipStoc TipStoc;
        public int? Semn;
        public string Cod;
        public RolTertSaft Rol;
        public string Motiv;
    }

    // Soldul cumulat până la o dată, GRUPAT pe (gestiune × lot), pe
    // `TipStoc`-urile raportate. Rândurile de deschidere (`DocumentId null`) intră
    // — ele SUNT sold.
    sealed class AgregatStocRand {
        public Guid RepartitorId { get; set; }
        public Guid LotId { get; set; }
        public decimal Cantitate { get; set; }
        public decimal Valoare { get; set; }
    }

    static List<AgregatStocRand> AgregatStoc(IObjectSpace os, List<TipStoc> tipuri, DateOnly pana) =>
        tipuri.Count == 0
            ? []
            : os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
                .Where(r => r.Data <= pana && tipuri.Contains(r.TipStoc))
                .GroupBy(r => new { r.RepartitorId, r.LotId })
                .Select(g => new AgregatStocRand {
                    RepartitorId = g.Key.RepartitorId,
                    LotId = g.Key.LotId,
                    Cantitate = g.Sum(r => r.Cantitate),
                    Valoare = g.Sum(r => r.Valoare),
                })
                .ToList();

    // `WarehouseID` (`SAFmiddle1textType`, max 35): codul gestiunii, iar acolo
    // unde nomenclatorul n-are cod, denumirea tăiată — niciodată un Guid.
    static string EtichetaRepartitor(
        IReadOnlyDictionary<Guid, (string Cod, string Denumire)> repartitori, Guid id) {
        if (!repartitori.TryGetValue(id, out var r))
            return id.ToString("N")[..32];
        if (!string.IsNullOrWhiteSpace(r.Cod))
            return r.Cod.Length <= 35 ? r.Cod : r.Cod[..35];
        if (!string.IsNullOrWhiteSpace(r.Denumire))
            return r.Denumire.Length <= 35 ? r.Denumire : r.Denumire[..35];
        return id.ToString("N")[..32];
    }

    // Formatul identității de terț pe o linie de stoc: fie literalul `0` (latura
    // neaplicabilă), fie un identificator `00`–`06` cu ceva după prefix.
    static bool IdentitateTertValida(string id) {
        if (string.IsNullOrEmpty(id))
            return false;
        if (id == SaftReguli.TertNeaplicabil)
            return true;
        return id.Length > 2 && id[0] == '0' && id[1] is >= '0' and <= '6';
    }

    // ══ Bucățile COMUNE celor două module (L și S) ═══════════════════════════
    //
    // Antetul, planul de conturi, produsele, unitățile de măsură, tabela de taxe
    // și etichetele dimensiunilor sunt ALE FIȘIERULUI, nu ale modulului: aceleași
    // secțiuni, aceleași reguli, aceleași avertismente. Extrase aici (felia 17,
    // pasul 2) ca SaftStocuri să le refolosească — o a doua copie ar fi divergit
    // tăcut, exact ca a doua agregare a balanței de care ne ferim în §3.

    // Motivul pentru care declarația NU se aplică bazei — sau `null`. Se citește
    // ÎNAINTEA oricărei interogări pe registre. `modul` = sufixul din mesaj („" la
    // lunar, „ S" la stocuri): textul rămâne identic cu cel din felia 16.
    static string MotivNeaplicabil(IObjectSpace os, string modul) {
        var setare = os.GetObjectsQuery<SetareProfil>().Select(s => new { s.Profil }).FirstOrDefault();
        if (setare == null || setare.Profil != ProfilContabil.Bugetar)
            return null;
        return $"SAF-T (D406{modul}) nu se aplică profilului bugetar: planul de conturi al instituțiilor "
            + "publice nu e printre cele 12 baze contabile (`TaxAccountingBasis`) ale schemei ANAF, deci "
            + "declarația n-are unde se valida.";
    }

    // Societatea raportoare, citită PLAT (o singură interogare, fără navigații).
    sealed class InfoSocietate {
        public string Denumire, CodFiscal, Tara;
        public bool InregistratTva, RaporteazaCnp;
        public string Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetCod;
        public string ContactNume, ContactPrenume, Telefon, Email, Iban, ContBancarCod, BazaContabila;
    }

    static InfoSocietate CitesteSocietate(IObjectSpace os) {
        // Proiecție anonimă în SQL, apoi mapare în MEMORIE: `InfoSocietate` are
        // câmpuri, iar un `MemberInit` pe câmpuri e exact felul de expresie pe care
        // traducătorul EF n-are de ce s-o știe.
        var s = os.GetObjectsQuery<Societate>()
            .Select(x => new {
                x.Denumire, x.CodFiscal, x.InregistratTva, x.Tara,
                x.Strada, x.Numar, x.DetaliiAdresa, x.Localitate, x.CodPostal,
                JudetCod = x.Judet.Cod,
                x.ContactNume, x.ContactPrenume, x.Telefon, x.Email,
                Iban = x.ContBancar.Iban, ContBancarCod = x.ContBancar.Cod,
                x.BazaContabila, x.RaporteazaCnp
            })
            .FirstOrDefault();
        return s == null ? null : new InfoSocietate {
            Denumire = s.Denumire, CodFiscal = s.CodFiscal, InregistratTva = s.InregistratTva, Tara = s.Tara,
            Strada = s.Strada, Numar = s.Numar, DetaliiAdresa = s.DetaliiAdresa,
            Localitate = s.Localitate, CodPostal = s.CodPostal, JudetCod = s.JudetCod,
            ContactNume = s.ContactNume, ContactPrenume = s.ContactPrenume,
            Telefon = s.Telefon, Email = s.Email,
            Iban = s.Iban, ContBancarCod = s.ContBancarCod,
            BazaContabila = s.BazaContabila, RaporteazaCnp = s.RaporteazaCnp,
        };
    }

    // Câmpurile fără de care fișierul nu trece validarea — avertisment, nu refuz:
    // declarația se generează, iar omul vede exact ce-i lipsește.
    static void VerificaSocietate(InfoSocietate soc, Action<CodAvertismentSaft, string> avert) {
        void Lipsa(string camp) =>
            avert(CodAvertismentSaft.SocietateIncompleta,
                $"`Societate.{camp}` e gol — fișierul nu trece validarea fără el "
                + "(completați „Configurare → Societate”).");

        if (soc == null) {
            Lipsa("(rândul lipsește)");
            return;
        }
        if (string.IsNullOrWhiteSpace(soc.CodFiscal))
            Lipsa(nameof(Societate.CodFiscal));
        else if (!SaftReguli.CuiValid(soc.CodFiscal))
            avert(CodAvertismentSaft.SocietateIncompleta,
                $"`Societate.CodFiscal` („{soc.CodFiscal}”) nu trece cifra de control a CUI-ului — "
                + "`RegistrationNumber` iese cum e cules, dar validatorul îl va refuza.");
        if (string.IsNullOrWhiteSpace(soc.Denumire)) Lipsa(nameof(Societate.Denumire));
        if (string.IsNullOrWhiteSpace(soc.Localitate)) Lipsa(nameof(Societate.Localitate));
        if (string.IsNullOrWhiteSpace(soc.JudetCod)) Lipsa(nameof(Societate.Judet));
        if (string.IsNullOrWhiteSpace(soc.ContactNume)) Lipsa(nameof(Societate.ContactNume));
        if (string.IsNullOrWhiteSpace(soc.Telefon)) Lipsa(nameof(Societate.Telefon));
        if (string.IsNullOrWhiteSpace(soc.Iban)) Lipsa(nameof(Societate.ContBancar));
    }

    // `Header` + `Company` — identic pe cele două module, cu o singură diferență:
    // `HeaderComment` („L" = lunar, „C" = la cerere/stocuri). Tipul declarației
    // vine EXCLUSIV de acolo (validatorul: `AUDIT_FILE_TYPE.ON_DEMAND = "C"`).
    static SaftHeader Antet(InfoSocietate soc, int an, int luna, DateOnly? dataCreare, string headerComment) {
        var taraSocietate = SaftReguli.CodTaraSaft(soc?.Tara);
        return new SaftHeader {
            AuditFileVersion = AuditFileVersion,
            AuditFileCountry = AuditFileCountry,
            AuditFileRegion = soc?.JudetCod,
            AuditFileDateCreated = dataCreare ?? DateOnly.FromDateTime(DateTime.Today),
            SoftwareCompanyName = SoftwareCompanyName,
            SoftwareID = SoftwareID,
            SoftwareVersion = VersiuneAssembly(),
            DefaultCurrencyCode = DefaultCurrencyCode,
            HeaderComment = headerComment,
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
    }

    // Planul de conturi ca dicționar de etichete (LEFT JOIN în memorie). Tuplu, nu
    // clasă: `TryGetValue(…, out var info)` pe un rând absent trebuie să dea
    // câmpuri goale, nu `null` de dereferențiat.
    static Dictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)>
        CitesteConturi(IObjectSpace os) =>
        os.GetObjectsQuery<Cont>()
            .Select(c => new { c.ID, c.Simbol, c.Denumire, c.Functie, c.RolTert })
            .ToList()
            .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire, c.Functie, c.RolTert));

    // `GeneralLedgerAccounts` + balanța din care s-a calculat (apelantul o refolosește
    // pentru cusăturile de solduri: L pe cusătura 4, S pe S3).
    static (List<SaftCont> Conturi, List<BalantaRand> Balanta) ConturiSiSolduri(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd,
        Dictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)> conturi,
        Action<CodAvertismentSaft, string> avert) {
        var balanta = ContabilProiectii.Balanta(os, dataStart, dataEnd, analitic: false).ToList();
        var rezultat = new List<SaftCont>();
        foreach (var b in balanta
                     .OrderBy(x => x.ContSimbol ?? "", StringComparer.Ordinal).ThenBy(x => x.ContId)) {
            var deschidere = b.InitialDebit - b.InitialCredit;
            var inchidere = deschidere + b.RulajDebit - b.RulajCredit;
            if (deschidere == 0m && inchidere == 0m && b.RulajDebit == 0m && b.RulajCredit == 0m)
                continue;
            conturi.TryGetValue(b.ContId, out var info);
            if (!SaftReguli.FunctieCunoscuta(info.Functie))
                avert(CodAvertismentSaft.TipContNecunoscut,
                    $"Contul {info.Simbol ?? b.ContSimbol ?? b.ContId.ToString()} are funcția "
                    + $"„{info.Functie ?? "(goală)"}” — `AccountType` iese „Bifunctional”.");
            rezultat.Add(new SaftCont {
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
        return (rezultat, balanta);
    }

    // `Products` + `UOMTable` — DOAR produsele referite de fișier (nu tot
    // nomenclatorul), cu valorile de rezervă ale ANAF-ului acolo unde modelul tace.
    static (List<SaftProdus> Produse, List<SaftUnitate> Unitati) ProduseSiUnitati(
        IObjectSpace os, IReadOnlyCollection<Guid> idsProduse, Action<CodAvertismentSaft, string> avert) {
        var listaProduse = idsProduse.ToList();
        var produse = os.GetObjectsQuery<Produs>().IgnoreQueryFilters()
            .Where(p => listaProduse.Contains(p.ID))
            .Select(p => new {
                p.ID, p.Cod, p.Denumire, p.CodNc, p.UM,
                CodUm = p.UnitateMasura.Cod, DenumireUm = p.UnitateMasura.Denumire,
                Natura = (NaturaClasa?)p.TipMaterial.Clasa.Natura
            })
            .ToList();
        var rezultat = new List<SaftProdus>();
        var unitatiFolosite = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in produse.OrderBy(x => x.Cod ?? "", StringComparer.Ordinal).ThenBy(x => x.ID)) {
            var codNc = p.CodNc;
            if (string.IsNullOrWhiteSpace(codNc)) {
                codNc = CodNcImplicit;
                avert(CodAvertismentSaft.FaraCodNc,
                    $"Produsul „{p.Denumire}” ({p.Cod}) n-are cod NC — `ProductCommodityCode` iese „{CodNcImplicit}”.");
            }
            var codUm = p.CodUm;
            var denumireUm = p.DenumireUm;
            if (string.IsNullOrWhiteSpace(codUm)) {
                codUm = UnitateImplicita;
                denumireUm = null;
                avert(CodAvertismentSaft.FaraUnitateMasura,
                    $"Produsul „{p.Denumire}” ({p.Cod}) n-are unitate de măsură UN/ECE "
                    + $"(UM liberă: „{p.UM ?? "gol"}”) — `UOMBase` iese „{UnitateImplicita}”.");
            }
            if (!unitatiFolosite.TryGetValue(codUm, out var existenta) || existenta == null)
                unitatiFolosite[codUm] = denumireUm;
            rezultat.Add(new SaftProdus {
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
        var unitati = unitatiFolosite.OrderBy(u => u.Key, StringComparer.Ordinal)
            .Select(u => new SaftUnitate { UnitOfMeasure = u.Key, Description = u.Value ?? u.Key })
            .ToList();
        return (rezultat, unitati);
    }

    // `TaxTable` — un rând per cod SAF-T FOLOSIT (`000000` nu se declară).
    static List<SaftTaxCode> TabelaTaxe(IReadOnlyDictionary<string, (decimal Cota, string Denumire)> coduri) =>
        coduri
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

    // Etichetele dimensiunilor + `AnalysisTypeTable`. Obiect, nu funcție: tabela
    // declară doar tipurile FOLOSITE, deci înregistrarea și listarea trebuie să
    // împartă aceeași stare — la L folosirea vine din liniile de GL emise, la S
    // din dimensiunile perioadei (S nu emite `Analysis` pe linii).
    sealed class EtichetePerioada {
        static readonly Dictionary<string, string> Descrieri = new(StringComparer.Ordinal) {
            ["CF"] = "Cod funcțional", ["CE"] = "Cod economic", ["SF"] = "Sursă de finanțare",
            ["U"] = "Unitate", ["P"] = "Proiect", ["CC"] = "Centru de cost",
        };
        readonly Dictionary<string, Dictionary<Guid, (string Cod, string Denumire)>> dimensiuni;
        readonly Dictionary<(string, string), SaftTipAnaliza> folosite = [];

        public EtichetePerioada(IObjectSpace os, Dictionary<Guid, (string Cod, string Denumire)> repartitori) {
            dimensiuni = new Dictionary<string, Dictionary<Guid, (string Cod, string Denumire)>>(StringComparer.Ordinal) {
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
        }

        /// <summary>Marchează dimensiunea ca FOLOSITĂ și întoarce `AnalysisID`-ul ei.</summary>
        public string Inregistreaza(string tip, Guid id) {
            dimensiuni[tip].TryGetValue(id, out var eticheta);
            var cod = string.IsNullOrWhiteSpace(eticheta.Cod) ? id.ToString("N") : eticheta.Cod;
            folosite.TryAdd((tip, cod), new SaftTipAnaliza {
                AnalysisType = tip, AnalysisTypeDescription = Descrieri[tip],
                AnalysisID = cod, AnalysisIDDescription = eticheta.Denumire ?? cod,
            });
            return cod;
        }

        public List<SaftTipAnaliza> Lista() => folosite.Values
            .OrderBy(a => a.AnalysisType, StringComparer.Ordinal)
            .ThenBy(a => a.AnalysisID, StringComparer.Ordinal)
            .ToList();
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
        CodAvertismentSaft.PlataFaraContTert =>
            "Plăți/încasări către un PARTENER ale căror rânduri n-ating niciun cont cu `RolTert` (462, 461, un cont "
            + "de decontare oarecare) — `Customer`/`Supplier` cere `AccountID`, iar un element gol face fișierul "
            + "invalid, deci plata iese în `Neincluse`; puneți rolul pe contul folosit sau plătiți pe contul de terț.",
        CodAvertismentSaft.TertFaraPartener =>
            "Rânduri de registru pe conturi de terți fără niciun partener pe laturi — `CustomerID`/`SupplierID` "
            + "ies cu codul societății, iar soldul lor nu ajunge în `Customers`/`Suppliers`.",
        CodAvertismentSaft.TertLipsaPeMiscare =>
            "Mișcări de stoc a căror politică cere un rol de terț (NIR ⇒ furnizor, DSC ⇒ client), dar al căror "
            + "document n-are niciun partener pe laturi — nici pe ale documentului-sursă. Ambele identificatoare "
            + "ies cu ale societății raportoare, adică „mișcare internă”; cazul tipic e NIR-ul MANUAL, fără "
            + "factură-sursă.",
        CodAvertismentSaft.ProdusFaraContStoc =>
            "Produse fără cont de stoc (`TipMaterial.ContImplicit`) — `PhysicalStock.ProductType` iese „0”. Pe "
            + "MIȘCĂRI aceeași gaură scoate linia din fișier (`Neincluse/FaraContStoc`): acolo `AccountID` e "
            + "obligatoriu, iar un cont inventat e interzis (73e).",
        CodAvertismentSaft.SoldNegativ =>
            "Solduri finale NEGATIVE pe (gestiune × lot) — gardianul de sold (25d) n-ar trebui să le lase să "
            + "existe. Se declară CA ATARE: registrul e sursa, iar o ajustare la zero ar fi o cifră inventată.",
        CodAvertismentSaft.SoldPeTipStocNeraportat =>
            "Solduri pe registre de stoc pe care declarația NU le raportează (`Consum`, `Folosinta`, `Custodie`…): "
            + "niciun tip de document nu produce cod de mișcare pe ele, deci nu sunt patrimoniu în magazie. N-au "
            + "document, deci nu pot fi `Neincluse` — dar cifra rămâne vizibilă aici.",
        CodAvertismentSaft.MovementReferenceTrunchiat =>
            "Referințe de mișcare mai lungi de 35 de caractere — numărul documentului s-a tăiat de la ÎNCEPUT "
            + "(coada distinge, prefixul de serie se repetă). Identitatea rămâne unică prin sufixele de cod și de "
            + "storno, dar referința nu mai e numărul întreg.",
        _ => cod.ToString(),
    };
}

