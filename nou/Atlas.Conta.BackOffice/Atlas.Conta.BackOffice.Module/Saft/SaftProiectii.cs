using System.Globalization;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using N = Atlas.Conta.Nucleu;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Saft;

public static partial class SaftProiectii {

    public const string SoftwareCompanyName = "Atlas";
    public const string SoftwareID = "Atlas.Conta";
    public const string HeaderComment = "L";
    public const string HeaderCommentStocuri = "C";
    public const string AuditFileVersion = "2.0";
    public const string AuditFileCountry = "RO";
    public const string DefaultCurrencyCode = "RON";
    public const string MetodaEvaluare = "FIFO";
    public const string UnitateImplicita = "H87";
    public const string CodNcImplicit = "0";
    public const string LocalitateImplicita = "Nespecificat";

    public static readonly string[] TipuriVanzare = ["FCL", "RDC"];
    public static readonly string[] TipuriCumparare = ["FCT", "RLF"];
    public const string CodTipInchidereTva = "ITV";
    sealed class SoldTert {
        public decimal InitialDebit, InitialCredit, RulajDebit, RulajCredit;
        public decimal Net => InitialDebit - InitialCredit + RulajDebit - RulajCredit;
        public decimal NetInitial => InitialDebit - InitialCredit;
        public decimal Miscare => Math.Abs(RulajDebit) + Math.Abs(RulajCredit);
    }

    sealed class InfoPartener {
        public Guid Id;
        public string Denumire, CodFiscal, Cod, Tara;
        public TipPersoana TipPersoana;
        public bool InregistratTva, TvaLaIncasare;
        public string Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetCod;
        public string Id406;
        public FelIdSaft Fel;
    }

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
            Refuzuri = dto.Refuzuri,
            TipuriMiscare = dto.TipuriMiscare.Count,
            StocFizic = dto.StocFizic.Count,
            MiscariStoc = dto.MiscariStoc.Count,
            LiniiMiscare = dto.MiscariStoc.Sum(m => m.Linii.Count),
            Excluse = dto.Excluse,
            Rezumat = dto.Rezumat,
            Neincluse = AgregaNeincluse(dto.Neincluse),
            Avertismente = dto.Avertismente,
        };
    }

    /// <summary>
    /// `Neincluse` agregat per cauză (F20-D5) — funcție PURĂ pe lista plată,
    /// aceeași formă ca agregarea avertismentelor. Ordinea: `Numar` DESCRESCĂTOR
    /// (cauza cea mai grasă prima — e ordinea în care omul le citește), apoi
    /// `Cauza` ordinal; exemplele sunt PRIMELE din grup, adică primele în ordinea
    /// (deja deterministă) a listei plate. Totul e determinist: două cereri pe
    /// aceleași date dau exact același răspuns, până la ordinea exemplelor.
    /// </summary>
    public static List<NeinclusAgregat> AgregaNeincluse(IEnumerable<SaftNeinclus> neincluse) {
        ArgumentNullException.ThrowIfNull(neincluse);
        return neincluse
            .GroupBy(n => n.Cauza ?? "", StringComparer.Ordinal)
            .Select(g => new NeinclusAgregat {
                Cauza = g.Key,
                Numar = g.Count(),
                Randuri = g.Sum(n => n.Randuri),
                Suma = g.Sum(CifraNeinclus),
                Cantitate = g.Any(n => n.Cantitate != null) ? g.Sum(n => n.Cantitate ?? 0m) : null,
                Exemple = g.Take(NeinclusAgregat.MaximExemple).ToList(),
            })
            .OrderByDescending(a => a.Numar)
            .ThenBy(a => a.Cauza, StringComparer.Ordinal)
            .ToList();
    }

    static decimal CifraNeinclus(SaftNeinclus n) =>
        n.Valoare != null ? n.Valoare.Value
        : n.Baza != null ? Math.Abs(n.Baza.Value)
        : Math.Abs(n.Debit ?? 0m) + Math.Abs(n.Credit ?? 0m);

    sealed class RegulaMiscare {
        public Guid TipDocumentId;
        public string CodTipDocument;
        public TipStoc TipStoc;
        public int? Semn;
        public string Cod;
        public RolTertSaft Rol;
        public string Motiv;
    }

    /// <summary>
    /// Câte `MovementReference` se REPETĂ într-o listă de mișcări (cusătura S4,
    /// fixul F1). Funcție PURĂ și publică fiindcă e o afirmație despre FIȘIER,
    /// nu despre bază: proba negativă („două mișcări cu aceeași referință ⇒
    /// cusătura pică") se scrie pe un DTO fabricat, fără nicio scenă.
    /// </summary>
    public static int ReferinteDuplicate(IReadOnlyCollection<SaftMiscareStoc> miscari) =>
        miscari.Count - miscari.Select(m => m.MovementReference)
            .Distinct(StringComparer.Ordinal).Count();

    const string ComponentaDeschidere = "(deschidere)";

    static bool IdentitateTertValida(string id) {
        if (string.IsNullOrEmpty(id))
            return false;
        if (id == SaftReguli.TertNeaplicabil)
            return true;
        return id.Length > 2 && id[0] == '0' && id[1] is >= '0' and <= '6';
    }


    public static bool SeAplica(IObjectSpace os) => os.GetObjectsQuery<SetareProfil>()
        .Select(s => (ProfilContabil?)s.Profil).FirstOrDefault() != ProfilContabil.Bugetar;

    static string MotivNeaplicabil(IObjectSpace os, string modul) {
        if (SeAplica(os))
            return null;
        return $"SAF-T (D406{modul}) nu se aplică profilului bugetar: planul de conturi al instituțiilor "
            + "publice nu e printre cele 12 baze contabile (`TaxAccountingBasis`) ale schemei ANAF, deci "
            + "declarația n-are unde se valida.";
    }

    sealed class InfoSocietate {
        public string Denumire, CodFiscal, Tara;
        public bool InregistratTva, RaporteazaCnp;
        public string Strada, Numar, DetaliiAdresa, Localitate, CodPostal, JudetCod;
        public string ContactNume, ContactPrenume, Telefon, Email, Iban, ContBancarCod, BazaContabila;
    }

    static InfoSocietate CitesteSocietate(IObjectSpace os) {
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

    static Dictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)>
        CitesteConturi(IObjectSpace os) =>
        os.GetObjectsQuery<Cont>()
            .Select(c => new { c.ID, c.Simbol, c.Denumire, c.Functie, c.RolTert })
            .ToList()
            .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire, c.Functie, c.RolTert));

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
                OpeningDebitBalance = deschidere >= 0m ? deschidere : null,
                OpeningCreditBalance = deschidere < 0m ? -deschidere : null,
                ClosingDebitBalance = inchidere >= 0m ? inchidere : null,
                ClosingCreditBalance = inchidere < 0m ? -inchidere : null,
            });
        }
        return (rezultat, balanta);
    }

    static (List<SaftProdus> Produse, List<SaftUnitate> Unitati) ProduseSiUnitati(
        IObjectSpace os, IReadOnlyCollection<Guid> idsProduse, Action<CodAvertismentSaft, string> avert) {
        var listaProduse = idsProduse.ToList();
        var produse = os.GetObjectsQuery<Produs>()
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
                UOMToUOMBaseConversionFactor = 1m,
            });
        }
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

    static List<SaftTaxCode> TabelaTaxe(IReadOnlyDictionary<string, (decimal Cota, string Denumire)> coduri) =>
        coduri
            .Where(t => t.Key != SaftReguli.TaxCodeNefiscal)
            .OrderBy(t => t.Key, StringComparer.Ordinal)
            .Select(t => new SaftTaxCode {
                TaxType = SaftReguli.TaxTypeTva,
                TaxCode = t.Key,
                Description = t.Value.Denumire,
                TaxPercentage = t.Value.Cota,
                BaseRate = 1m,
                Country = AuditFileCountry,
            })
            .ToList();

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

    static Guid? PartenerulDocumentului(Guid? predatorId, Guid? primitorId,
        Dictionary<Guid, InfoPartener> parteneri) {
        var gasiti = new[] { predatorId, primitorId }
            .Where(id => id is Guid v && parteneri.ContainsKey(v))
            .Select(id => id.Value)
            .Distinct()
            .ToList();
        return gasiti.Count == 1 ? gasiti[0] : null;
    }

    static string VersiuneAssembly() {
        var versiune = typeof(SaftProiectii).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(SaftProiectii).Assembly.GetName().Version?.ToString()
            ?? "1.0";
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
              + "`TaxCode 000000`, adică „nerelevant fiscal”; completați maparea TVA SAF-T pentru versiune și secțiune.",
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
            "Solduri finale NEGATIVE pe (gestiune × lot). NU e o scăpare a gardianului de sold (25d păzește "
            + "cheia de stoc a MOTORULUI, care nu e încălcată): pe baza de import cauza e deriva de rotunjire "
            + "PER LOT, pe care contractul 1C o declară nereconciliabilă structural (45e/52) — valoarea "
            + "grupei se conservă, repartiția ei pe loturi nu. Se declară CA ATARE: registrul e sursa, iar o "
            + "ajustare la zero ar fi o cifră inventată.",
        CodAvertismentSaft.ReziduValoricFaraCantitate =>
            "Intrări de stoc fizic cu cantitate 0 la ambele capete și valoare nenulă („0 bucăți, X lei”) — "
            + "același rezidu al derivei per lot (45e), dar ALT fapt decât soldul negativ, deci altă cifră. "
            + "Se declară: `PhysicalStock` descrie patrimoniul, iar o valoare omisă ar face fișierul mai mic "
            + "decât balanța.",
        CodAvertismentSaft.NumarDocumentDuplicat =>
            "Perechi (tip × număr) purtate de MAI MULTE documente — importul aduce numărul sursei, iar conexele "
            + "îl moștenesc. `MovementReference` primește discriminantul `#1`…`#n` (ordinea `DocumentId`, "
            + "stabilă între rulări), ca identitatea mișcărilor din fișier să rămână unică.",
        CodAvertismentSaft.NumarFacturaDuplicat =>
            "Facturi din aceeași secțiune cu ACELAȘI `InvoiceNo` — aici NU se discriminează nimic: numărul e "
            + "cel real al facturii, iar un sufix inventat ar declara o factură care nu există. Faptul se "
            + "raportează ca să fie văzut înainte de depunere.",
        CodAvertismentSaft.DataPostariiInAfaraPerioadei =>
            "Documente a căror `DataOperare` cade în afara perioadei declarate (pe baza de import e ora "
            + "RULĂRII importului) — `MovementPostingDate` e opțional în schemă, deci se OMITE: o dată de "
            + "postare care contrazice antetul e mai rea decât absența ei.",
        CodAvertismentSaft.RolTertMixt =>
            "Mișcări al căror grup (document × storno × cod) atinge registre cu roluri de terț DIFERITE — "
            + "linia are un singur `CustomerID`/`SupplierID`, deci se ia rolul primei linii (determinist pe "
            + "`Id`). E o incoerență de POLITICĂ: același cod de mișcare ar trebui să aibă același rol.",
        CodAvertismentSaft.SoldPeTipStocNeraportat =>
            "Solduri pe registre de stoc pe care declarația NU le raportează (`Consum`, `Folosinta`, `Custodie`…): "
            + "niciun tip de document nu produce cod de mișcare pe ele, deci nu sunt patrimoniu în magazie. N-au "
            + "document, deci nu pot fi `Neincluse` — dar cifra rămâne vizibilă aici.",
        CodAvertismentSaft.MovementReferenceTrunchiat =>
            "Referințe de mișcare mai lungi de 35 de caractere — numărul documentului s-a tăiat de la ÎNCEPUT "
            + "(coada distinge, prefixul de serie se repetă). Identitatea rămâne unică prin sufixele de cod și de "
            + "storno, dar referința nu mai e numărul întreg.",
        CodAvertismentSaft.PlataAnalizaMixta =>
            "Linii de plată formate din postări cu analize diferite (centru de cost, proiect…) pe aceeași partidă: "
            + "`Analysis` se omite pe linia de plată, iar GL-ul păstrează analiza fiecărei postări.",
        CodAvertismentSaft.PlataPePartidaInitiala =>
            "Plăți alocate unei partide din soldul inițial: partida n-are număr de document, deci linia iese fără "
            + "`SourceDocumentID`.",
        _ => cod.ToString(),
    };
}
