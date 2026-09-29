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
    public static readonly string[] TipuriRetur = ["RLF", "RDC"];
    public static readonly string[] TipuriPlata = ["PLT", "INC"];
    public const string CodTipInchidereTva = "ITV";
    const string CodTipReturClient = "RDC";
    const string CodTipPlata = "PLT";

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

        rezultat.Neaplicabil = MotivNeaplicabil(os, "");
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
                Region = taraPartener == "RO" ? p.JudetCod : null,
                Country = taraPartener,
            };
        }

        var soc = CitesteSocietate(os);
        VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));

        var idSocietate = soc == null ? null : SaftReguli.IdSocietate(soc.CodFiscal, soc.Tara);
        var raporteazaCnp = soc?.RaporteazaCnp ?? false;

        rezultat.Header = Antet(soc, an, luna, dataCreare, HeaderComment);

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

        var (conturiSaft, balanta) = ConturiSiSolduri(
            os, dataStart, dataEnd, conturi, (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Conturi = conturiSaft;

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

        var randuriTva = Fiscale.Fapte(os).Where(f => f.Data >= dataStart && f.Data <= dataEnd).ToList();
        var tvaPeDetaliu = randuriTva.ToDictionary(t => (t.DetaliuId, t.Storno));
        var mapariFiscale = new MapariFiscale(os);
        var postariTaxa = Fiscale.Postari(os)
            .Where(p => p.Data >= dataStart && p.Data <= dataEnd && p.Carte == N.Carte.Contabil
                && (p.RolTva == N.RolTva.Taxa || p.RolTva == N.RolTva.Autocolectare))
            .Select(p => new { p.DocumentId, p.LinieId, Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno,
                p.Cont, p.Latura, p.RolTva }).ToList()
            .ToDictionary(p => (p.DocumentId, p.LinieId, p.Storno, p.Cont, p.Latura), p => p.RolTva.Value);

        var idsDocumente = randuri.Select(r => r.DocumentId)
            .Concat(randuriTva.Select(t => t.DocumentId))
            .Distinct().ToList();
        var codPerDocument = ApiProiectii.CoduriTip(os, idsDocumente);
        string CodTip(Guid documentId) => codPerDocument.GetValueOrDefault(documentId);

        var documente = os.GetObjectsQuery<Document>()
            .Where(d => idsDocumente.Contains(d.ID))
            .Select(d => new { d.ID, d.Numar, d.Data, d.DataOperare, d.PredatorId, d.PrimitorId })
            .ToList()
            .ToDictionary(d => d.ID);

        var conturiCuRol = conturi.Where(c => c.Value.RolTert != RolTertCont.Niciunul)
            .Select(c => c.Key).ToList();

        var agregateTert = ContabilProiectii.Atomi(os)
            .Where(r => r.Data <= dataEnd && conturiCuRol.Contains(r.ContId))
            .GroupBy(r => new { r.ContId, r.RepartitorId })
            .Select(g => new {
                g.Key.ContId, g.Key.RepartitorId,
                InitialDebit = g.Sum(r => r.Data < dataStart ? r.Debit : 0m),
                InitialCredit = g.Sum(r => r.Data < dataStart ? r.Credit : 0m),
                RulajDebit = g.Sum(r => r.Data >= dataStart ? r.Debit : 0m),
                RulajCredit = g.Sum(r => r.Data >= dataStart ? r.Credit : 0m),
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
        foreach (var a in agregateTert) AdaugaRep(a.RepartitorId);
        foreach (var d in documente.Values) {
            idsRepartitor.Add(d.PredatorId);
            idsRepartitor.Add(d.PrimitorId);
        }
        var listaRep = idsRepartitor.ToList();

        var parteneri = os.GetObjectsQuery<Partener>()
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

        var repartitori = os.GetObjectsQuery<Repartitor>()
            .Where(r => listaRep.Contains(r.ID))
            .Select(r => new { r.ID, r.Cod, r.Denumire })
            .ToList()
            .ToDictionary(r => r.ID, r => (r.Cod, r.Denumire));
        var idsAngajat = os.GetObjectsQuery<Angajat>()
            .Where(a => listaRep.Contains(a.ID)).Select(a => a.ID).ToList().ToHashSet();
        var idsContPropriu = os.GetObjectsQuery<ContPropriu>()
            .Where(c => listaRep.Contains(c.ID)).Select(c => c.ID).ToList().ToHashSet();

        string DenumireRep(Guid? id) =>
            id is Guid v && repartitori.TryGetValue(v, out var r) ? r.Denumire : null;

        (Guid? Id, bool ExistaRepartitor) PartenerulRandului(Guid? repLatura, Guid? repCealalta) {
            if (repLatura is Guid a && parteneri.ContainsKey(a))
                return (a, true);
            if (repCealalta is Guid b && parteneri.ContainsKey(b))
                return (b, true);
            return (null, repLatura != null || repCealalta != null);
        }

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
            AcumuleazaTert(a.ContId, Rol(a.ContId), a.RepartitorId, null,
                a.InitialDebit, a.RulajDebit, debit: true);
            AcumuleazaTert(a.ContId, Rol(a.ContId), a.RepartitorId, null,
                a.InitialCredit, a.RulajCredit, debit: false);
        }
        neincluse.AddRange(neincluseTert.Values.Where(n => n.Debit != 0m || n.Credit != 0m));

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
                    && !string.IsNullOrWhiteSpace(p.CodFiscal)
                    && (p.InregistratTva || Partener.NormalizeazaTara(p.Tara) == "RO"))
                Avert(CodAvertismentSaft.PartenerFaraCuiValid,
                    $"„{p.Denumire}” e român sau înregistrat în scopuri de TVA, dar codul fiscal "
                    + $"(„{p.CodFiscal}”) nu trece cifra de control — se declară cu prefixul 04 "
                    + "(cod intern), nu 00.");
        }
        var tertiReferiti = new List<(RolTertCont Rol, Guid PartenerId, string AccountID)>();

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

        var codTvaFolosit = new Dictionary<string, (decimal Cota, string Denumire)>(StringComparer.Ordinal);
        var tvaFaraCod = new Dictionary<Guid, (string Cod, string Denumire, SensTva Sens, decimal Suma, int Randuri)>();
        var tvaGl = 0m;
        var tvaFaraCodSaft = 0m;
        var taxeInGl = new HashSet<(Guid, bool)>();

        SaftTaxInfo Nefiscal() => new() {
            TaxType = SaftReguli.TaxTypeNefiscal, TaxCode = SaftReguli.TaxCodeNefiscal, TaxAmount = 0m
        };

        SaftTaxInfo TaxaRandului(RandGl r, string codTipDocument, bool debit) {
            if (codTipDocument == CodTipInchidereTva) {
                codTvaFolosit.TryAdd(SaftReguli.TaxCodeInchidereTva, (0m, "TVA — note contabile"));
                return new() { TaxType = SaftReguli.TaxTypeTva,
                    TaxCode = SaftReguli.TaxCodeInchidereTva, TaxAmount = 0m };
            }
            var cont = debit ? r.ContDebitId : r.ContCreditId;
            var latura = debit ? N.Latura.Debit : N.Latura.Credit;
            if (r.DetaliuId is not Guid det || !tvaPeDetaliu.TryGetValue((det, r.Storno), out var fapt)
                    || !postariTaxa.TryGetValue((r.DocumentId, r.DetaliuId, r.Storno, cont, latura), out var rol))
                return Nefiscal();
            var mapare = mapariFiscale.Pentru(fapt, SectiuneTvaSaft.GeneralLedger, rol);
            var suma = rol == N.RolTva.Autocolectare ? fapt.Autocolectare : fapt.Tva;
            var tip = tipTvaDupaId.GetValueOrDefault(fapt.TipTvaId);
            if (mapare == null) {
                if (rol == N.RolTva.Taxa) tvaFaraCodSaft += suma;
                var acumulat = tvaFaraCod.GetValueOrDefault(fapt.TipTvaId);
                tvaFaraCod[fapt.TipTvaId] = (tip?.Cod, tip?.Denumire, fapt.Sens,
                    acumulat.Suma + suma, acumulat.Randuri + 1);
                return Nefiscal();
            }
            if (rol == N.RolTva.Taxa && taxeInGl.Add((det, r.Storno))) tvaGl += suma;
            if (mapare.TaxType == SaftReguli.TaxTypeTva)
                codTvaFolosit.TryAdd(mapare.TaxCode, (fapt.Cota, tip?.Denumire));
            return new() { TaxType = mapare.TaxType, TaxCode = mapare.TaxCode,
                TaxPercentage = fapt.Cota, TaxBase = fapt.Baza, TaxAmount = suma };
        }

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
                var descriereLinie = r.DetaliuId is Guid det ? descrieri.GetValueOrDefault(det) : null;
                foreach (var debit in new[] { true, false }) {
                    var taxa = TaxaRandului(r, cod, debit);
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
                        TaxInformation = new SaftTaxInfo {
                            TaxType = taxa.TaxType, TaxCode = taxa.TaxCode,
                            TaxPercentage = taxa.TaxPercentage, TaxBase = taxa.TaxBase,
                            TaxAmount = taxa.TaxAmount
                        },
                    });
                    numarLinii++;
                    if (debit) totalDebit += r.Valoare;
                    else totalCredit += r.Valoare;
                }
            }
            jurnal.Tranzactii.Add(tranzactie);
        }
        rezultat.Jurnale = jurnale.Values.OrderBy(j => j.JournalID, StringComparer.Ordinal).ToList();

        foreach (var t in tvaFaraCod.Values.OrderBy(t => t.Cod, StringComparer.Ordinal))
            Avert(CodAvertismentSaft.TipTvaFaraCodSaft,
                $"Tipul de TVA „{t.Cod}” ({t.Denumire}) n-are cod SAF-T pe "
                + $"{(t.Sens == SensTva.Livrare ? "livrare" : "achiziție")} — cele {t.Randuri} rânduri ale lui "
                + "ies cu `000/000000`.", t.Suma);

        var idsFacturiVanzare = idsDocumente.Where(id => TipuriVanzare.Contains(CodTip(id))).ToList();
        var idsFacturiCumparare = idsDocumente.Where(id => TipuriCumparare.Contains(CodTip(id))).ToList();
        var idsPlati = idsDocumente.Where(id => TipuriPlata.Contains(CodTip(id))).ToList();
        var idsFacturi = idsFacturiVanzare.Concat(idsFacturiCumparare).ToList();
        var idsCuLinii = idsFacturi.Concat(idsPlati).ToList();

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
        var produsPeLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId }).ToList()
            .ToDictionary(l => l.ID, l => l.ProdusId);

        var valutaFct = os.GetObjectsQuery<FacturaIntrare>()
            .Where(f => idsFacturiCumparare.Contains(f.ID))
            .Select(f => new { f.ID, f.Valuta }).ToList()
            .ToDictionary(f => f.ID, f => f.Valuta);

        var loturiNascute = os.GetObjectsQuery<Lot>()
            .Where(l => l.LinieIntrareId != null && idsLinie.Contains(l.LinieIntrareId.Value))
            .Select(l => new { l.ID, l.LinieIntrareId })
            .ToList()
            .Select(l => new { l.ID, LinieId = l.LinieIntrareId.Value })
            .ToList();
        var idsLotNascut = loturiNascute.Select(l => l.ID).ToList();
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

                var jumatati = randuriDoc.Count > 0
                    ? randuriDoc.Select(r => r.Storno).Distinct().OrderBy(s => s).ToList()
                    : liniiDoc.SelectMany(l => new[] { false, true })
                        .Where(s => liniiDoc.Any(l => tvaPeDetaliu.ContainsKey((l.ID, s))))
                        .Distinct().OrderBy(s => s).ToList();

                foreach (var storno in jumatati) {
                    var randuriJumatate = randuriDoc.Where(r => r.Storno == storno).ToList();
                    var semn = storno ? -1m : 1m;
                    var dataJumatate = randuriJumatate.Count > 0
                        ? randuriJumatate.Min(r => r.Data)
                        : doc?.Data ?? dataStart;

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
                        if (cod == CodTipReturClient && l.LotId != null && l.TipTvaId == null)
                            continue;

                        var randuriLinie = randuriJumatate.Where(r => r.DetaliuId == l.ID).ToList();
                        var randContrapartida = randuriLinie.FirstOrDefault(r =>
                            (r.ContDebitId != contTert.Value && !conturiTva.Contains(r.ContDebitId))
                            || (r.ContCreditId != contTert.Value && !conturiTva.Contains(r.ContCreditId)));
                        Guid contLinie;
                        bool contrapartidaDebit;
                        RandGl randDimensiuni;
                        if (randContrapartida != null) {
                            contrapartidaDebit = randContrapartida.ContDebitId != contTert.Value
                                && !conturiTva.Contains(randContrapartida.ContDebitId);
                            contLinie = contrapartidaDebit
                                ? randContrapartida.ContDebitId : randContrapartida.ContCreditId;
                            randDimensiuni = randContrapartida;
                        }
                        else if (receptiePeLinie.TryGetValue(l.ID, out var receptie)) {
                            contLinie = receptie.ContDebitId;
                            contrapartidaDebit = true;
                            randDimensiuni = randuriPeId.GetValueOrDefault(receptie.ID);
                        }
                        else {
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

                        var areFapt = tvaPeDetaliu.TryGetValue((l.ID, storno), out var fapt);
                        var tip = areFapt ? tipTvaDupaId.GetValueOrDefault(fapt.TipTvaId) : null;
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
                            var mapare = mapariFiscale.Pentru(fapt, SectiuneTvaSaft.Facturi);
                            if (mapare != null) {
                                taxa = new SaftTaxInfo {
                                    TaxType = mapare.TaxType, TaxCode = mapare.TaxCode,
                                    TaxPercentage = fapt.Cota, TaxBase = fapt.Baza, TaxAmount = fapt.Tva
                                };
                                if (mapare.TaxType == SaftReguli.TaxTypeTva)
                                    codTvaFolosit.TryAdd(mapare.TaxCode, (fapt.Cota, tip?.Denumire));
                            } else {
                                Avert(CodAvertismentSaft.TipTvaFaraCodSaft,
                                    $"Mapare {MapariFiscale.Versiune}/Facturi absentă: {tip?.Cod}, {fapt.Cota}%, {fapt.Sens}.", fapt.Tva);
                            }
                            if (fapt.Sens == SensTva.Achizitie) bazaFacturiAchizitie += fapt.Baza;
                            else bazaFacturiLivrare += fapt.Baza;
                        }

                        pozitie++;
                        factura.Linii.Add(new SaftLinieFactura {
                            DetaliuId = l.ID,
                            LineNumber = pozitie,
                            AccountID = Simbol(contLinie),
                            ProductCode = produsId?.ToString(),
                            Quantity = cantitate,
                            UnitPrice = pret,
                            TaxPointDate = dataJumatate,
                            Description = descrieri.GetValueOrDefault(l.ID)
                                ?? tipuriMaterial.GetValueOrDefault(l.TipMaterialId)
                                ?? factura.InvoiceNo,
                            InvoiceLineAmount = valoare,
                            DebitCreditIndicator = rolAsteptat == RolTertCont.Client ? "C" : "D",
                            Analiza = randDimensiuni == null ? [] : Analiza(contrapartidaDebit, randDimensiuni),
                            TaxInformation = taxa,
                        });
                        factura.NetTotal += valoare;
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

        foreach (var (sectiune, lista) in new[] {
                     ("SalesInvoices", rezultat.FacturiEmise), ("PurchaseInvoices", rezultat.FacturiPrimite) })
            foreach (var coliziune in lista.GroupBy(f => f.InvoiceNo ?? "", StringComparer.Ordinal)
                         .Where(gr => gr.Count() > 1)
                         .OrderBy(gr => gr.Key, StringComparer.Ordinal))
                Avert(CodAvertismentSaft.NumarFacturaDuplicat,
                    $"{sectiune} „{coliziune.Key}”: {coliziune.Count()} facturi cu ACELAȘI `InvoiceNo` "
                    + $"({string.Join(", ", coliziune.Take(3).Select(f => f.DocumentTip + (f.Storno ? " (storno)" : "")))}).");

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
            if (contrapartidaId is Guid cid && celalaltId is Guid oid
                    && idsContPropriu.Contains(cid) && idsContPropriu.Contains(oid))
                continue;

            var randuriDocPlata = randuriPeDocument.GetValueOrDefault(docId) ?? [];
            var contTertPlata = randuriDocPlata
                .SelectMany(r => new[] { r.ContDebitId, r.ContCreditId })
                .FirstOrDefault(c => Rol(c) != RolTertCont.Niciunul);

            string customer = idSocietate, supplier = idSocietate;
            if (contrapartidaId is Guid cpid && parteneri.TryGetValue(cpid, out var partenerPlata)) {
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

        var (produseSaft, unitatiSaft) = ProduseSiUnitati(
            os, produseFolosite.ToList(), (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Produse = produseSaft;
        rezultat.Unitati = unitatiSaft;

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

        rezultat.Taxe = TabelaTaxe(codTvaFolosit);
        rezultat.TipuriAnaliza = etichete.Lista();

        bool EsteFactura(Guid documentId) {
            var c = CodTip(documentId);
            return TipuriVanzare.Contains(c) || TipuriCumparare.Contains(c);
        }

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
            TvaCapitalizat = randuriTva.Where(t => t.Regim == RegimTva.Capitalizat
                && !taxeInGl.Contains((t.DetaliuId, t.Storno))).Sum(t => t.Tva),
            TvaFaraCodSaft = tvaFaraCodSaft,
            BazaFacturiAchizitie = bazaFacturiAchizitie,
            BazaFacturiLivrare = bazaFacturiLivrare,
            BazaNeincluseAchizitie = bazaNeincluseAchizitie,
            BazaNeincluseLivrare = bazaNeincluseLivrare,
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

    public static SaftDto SaftStocuri(IObjectSpace os, int an, int luna, DateOnly? dataCreare = null) {
        var rezultat = new SaftDto {
            An = an,
            Luna = luna,
            DataStart = new DateOnly(an, luna, 1),
            DataEnd = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna)),
        };

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

        var soc = CitesteSocietate(os);
        VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));
        var idSocietate = SaftReguli.IdSocietate(soc?.CodFiscal, soc?.Tara);
        var ownerId = SaftReguli.OwnerIdRaportor(soc?.CodFiscal, soc?.Tara);
        rezultat.Header = Antet(soc, an, luna, dataCreare, HeaderCommentStocuri);

        var conturi = CitesteConturi(os);
        var (conturiSaft, balanta) = ConturiSiSolduri(
            os, dataStart, dataEnd, conturi, (cod, exemplu) => Avert(cod, exemplu));
        rezultat.Conturi = conturiSaft;

        var tipuriDocument = os.GetObjectsQuery<TipDocument>()
            .Select(t => new { t.ID, t.Cod, t.ClrType }).ToList();
        var idTipPeCod = tipuriDocument
            .Where(t => t.Cod != null)
            .GroupBy(t => t.Cod, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().ID, StringComparer.Ordinal);

        var politici = os.GetObjectsQuery<PoliticaMiscareSaft>()
            .Select(p => new { p.TipDocumentId, p.TipStoc, p.Semn, p.CodMiscare, p.RolTert, p.Motiv })
            .ToList()
            .Select(p => new RegulaMiscare {
                TipDocumentId = p.TipDocumentId, TipStoc = p.TipStoc, Semn = p.Semn,
                Cod = string.IsNullOrWhiteSpace(p.CodMiscare) ? null : p.CodMiscare.Trim(),
                Rol = p.RolTert, Motiv = p.Motiv,
                CodTipDocument = tipuriDocument.FirstOrDefault(t => t.ID == p.TipDocumentId)?.Cod,
            })
            .ToList();
        var politiciExacte = politici.Where(p => p.Semn != null)
            .ToDictionary(p => (p.TipDocumentId, p.TipStoc, p.Semn.Value));
        var politiciGenerice = politici.Where(p => p.Semn == null)
            .ToDictionary(p => (p.TipDocumentId, p.TipStoc));
        RegulaMiscare Potriveste(Guid tipId, TipStoc tipStoc, int semnRegula) =>
            semnRegula != 0 && politiciExacte.TryGetValue((tipId, tipStoc, semnRegula), out var exact)
                ? exact
                : politiciGenerice.GetValueOrDefault((tipId, tipStoc));

        var raportate = politici.Where(p => p.Cod != null).Select(p => p.TipStoc).Distinct().ToList();
        var raportateSet = raportate.ToHashSet();

        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
            .Where(r => r.Data >= dataStart && r.Data <= dataEnd && r.DocumentId != null)
            .Select(r => new RandStoc {
                Id = r.ID, Data = r.Data, TipStoc = r.TipStoc, LotId = r.LotId,
                RepartitorId = r.RepartitorId, Cantitate = r.Cantitate, Valoare = r.Valoare,
                Storno = r.Storno, DocumentId = r.DocumentId.Value, DetaliuId = r.DetaliuId,
            })
            .ToList();

        var agregat = AgregatStoc(os, dataStart, dataEnd);
        var deschideri = SoldPeCheie(agregat, raportateSet, a => (a.CantitateInitiala, a.ValoareInitiala),
            a => a.RanduriInitiale > 0);
        var inchideri = SoldPeCheie(agregat, raportateSet, a => (a.CantitateInitiala + a.CantitateRulaj,
            a.ValoareInitiala + a.ValoareRulaj), _ => true);

        foreach (var g in agregat
                     .Where(a => !raportateSet.Contains(a.TipStoc))
                     .GroupBy(a => a.TipStoc)
                     .Select(g => new {
                         TipStoc = g.Key,
                         Cantitate = g.Sum(a => a.CantitateInitiala + a.CantitateRulaj),
                         Valoare = g.Sum(a => a.ValoareInitiala + a.ValoareRulaj),
                         Randuri = g.Sum(a => a.Randuri),
                     })
                     .OrderBy(x => x.TipStoc)) {
            if (g.Cantitate == 0m && g.Valoare == 0m)
                continue;
            Avert(CodAvertismentSaft.SoldPeTipStocNeraportat,
                $"`{g.TipStoc}`: {g.Randuri} rânduri de registru, sold {g.Cantitate:0.###} / {g.Valoare:0.00} lei "
                + "la sfârșitul perioadei — niciun tip de document nu produce cod de mișcare pe registrul ăsta, "
                + "deci `PhysicalStock` nu-l declară.", g.Valoare);
        }

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

        var idsRepartitor = new HashSet<Guid>();
        foreach (var d in documente.Values) { idsRepartitor.Add(d.PredatorId); idsRepartitor.Add(d.PrimitorId); }
        foreach (var s in surse.Values) { idsRepartitor.Add(s.PredatorId); idsRepartitor.Add(s.PrimitorId); }
        foreach (var r in randuriStoc) idsRepartitor.Add(r.RepartitorId);
        foreach (var a in deschideri) idsRepartitor.Add(a.RepartitorId);
        foreach (var a in inchideri) idsRepartitor.Add(a.RepartitorId);
        var listaRep = idsRepartitor.ToList();

        var repartitori = os.GetObjectsQuery<Repartitor>()
            .Where(r => listaRep.Contains(r.ID))
            .Select(r => new { r.ID, r.Cod, r.Denumire })
            .ToList()
            .ToDictionary(r => r.ID, r => (r.Cod, r.Denumire));
        var parteneri = os.GetObjectsQuery<Partener>()
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

        var idsLot = randuriStoc.Select(r => r.LotId)
            .Concat(deschideri.Select(a => a.LotId))
            .Concat(inchideri.Select(a => a.LotId))
            .Distinct().ToList();
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId, l.PretUnitar })
            .ToList()
            .ToDictionary(l => l.ID, l => (l.ProdusId, l.PretUnitar));
        var idsProdus = loturi.Values.Select(l => l.ProdusId).Distinct().ToList();
        var produseCont = os.GetObjectsQuery<Produs>()
            .Where(p => idsProdus.Contains(p.ID))
            .Select(p => new { p.ID, p.Cod, p.Denumire, ContSimbol = p.TipMaterial.ContImplicit.Simbol })
            .ToList()
            .ToDictionary(p => p.ID, p => (p.Cod, p.Denumire, p.ContSimbol));

        string SimbolStoc(Guid produsId) =>
            produseCont.TryGetValue(produsId, out var p) ? SaftReguli.SimbolSaft(p.ContSimbol) : null;
        string NumeProdus(Guid produsId) =>
            produseCont.TryGetValue(produsId, out var p) ? (p.Denumire ?? p.Cod ?? produsId.ToString()) : produsId.ToString();

        var deschidereCheie = deschideri.ToDictionary(a => (a.RepartitorId, a.LotId), a => (a.Cantitate, a.Valoare));
        var inchidereCheie = inchideri.ToDictionary(a => (a.RepartitorId, a.LotId), a => (a.Cantitate, a.Valoare));
        var cuMiscare = randuriStoc.Where(r => raportateSet.Contains(r.TipStoc))
            .Select(r => (r.RepartitorId, r.LotId)).ToHashSet();
        var chei = deschidereCheie.Keys.Concat(inchidereCheie.Keys).Concat(cuMiscare).Distinct().ToList();

        var produseFolosite = new HashSet<Guid>();
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
                    + "(deriva de rotunjire PER LOT a importului, 45e/52; gardianul 25d păzește altă cheie).",
                    inchidere.Valoare);
            if (deschidere.Cantitate == 0m && inchidere.Cantitate == 0m
                    && (deschidere.Valoare != 0m || inchidere.Valoare != 0m))
                Avert(CodAvertismentSaft.ReziduValoricFaraCantitate,
                    $"„{NumeProdus(lot.ProdusId)}” în {EtichetaRepartitor(repartitori, cheie.Item1)}: cantitate 0 "
                    + $"la ambele capete, valoare {deschidere.Valoare:0.00} → {inchidere.Valoare:0.00} lei.",
                    Math.Abs(inchidere.Valoare != 0m ? inchidere.Valoare : deschidere.Valoare));
            produseFolosite.Add(lot.ProdusId);
            stocFizic.Add(new SaftStocFizic {
                RepartitorId = cheie.Item1,
                LotId = cheie.Item2,
                ProdusId = lot.ProdusId,
                WarehouseId = EtichetaRepartitor(repartitori, cheie.Item1),
                ProductType = SaftReguli.ProductTypeDinCont(simbol),
                OwnerId = ownerId,
                UomConversionFactor = 1m,
                UnitPrice = Math.Round(lot.PretUnitar, 2, MidpointRounding.AwayFromZero),
                OpeningQuantity = deschidere.Cantitate,
                OpeningValue = deschidere.Valoare,
                ClosingQuantity = inchidere.Cantitate,
                ClosingValue = inchidere.Valoare,
                StockCharacteristic = SaftReguli.StockCharacteristic.Cheie,
                StockCharacteristicValue = SaftReguli.StockCharacteristic.Valoare,
            });
        }
        var loturiPerProdusGestiune = stocFizic
            .GroupBy(e => (e.RepartitorId, e.ProdusId))
            .ToDictionary(g => g.Key, g => g.Select(e => e.LotId).Distinct().Count());
        bool CereStockAccountNo(Guid repartitorId, Guid produsId) =>
            loturiPerProdusGestiune.GetValueOrDefault((repartitorId, produsId)) > 1;
        foreach (var e in stocFizic)
            if (CereStockAccountNo(e.RepartitorId, e.ProdusId))
                e.StockAccountNo = e.LotId.ToString();

        var emise = new List<(RandStoc Rand, RegulaMiscare Regula)>();
        var excluse = new Dictionary<(Guid, TipStoc, int?), SaftExclus>();
        var faraPolitica = new Dictionary<(string, TipStoc, int), SaftNeinclus>();
        var codNecunoscut = new Dictionary<(string, TipStoc, string), SaftNeinclus>();
        foreach (var r in randuriStoc) {
            var codTip = codPerDocument.GetValueOrDefault(r.DocumentId);
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
            if (!SaftReguli.EsteCodMiscare(regula.Cod)) {
                var cheie = (codTip ?? "(tip necunoscut)", r.TipStoc, regula.Cod);
                if (!codNecunoscut.TryGetValue(cheie, out var n)) {
                    documente.TryGetValue(r.DocumentId, out var d);
                    n = codNecunoscut[cheie] = new SaftNeinclus {
                        Cauza = nameof(CauzaNeincludere.CodMiscareNecunoscut),
                        Sectiune = "MovementOfGoods",
                        DocumentId = r.DocumentId,
                        DocumentNumar = d?.Numar,
                        DocumentTip = cheie.Item1,
                        TipStoc = r.TipStoc.ToString(),
                        Semn = regula.Semn,
                        CodMiscare = regula.Cod,
                        Cantitate = 0m, Valoare = 0m,
                    };
                }
                n.Cantitate += r.Cantitate;
                n.Valoare += r.Valoare;
                n.Randuri++;
                continue;
            }
            emise.Add((r, regula));
        }

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

        string NumarEfectiv(Guid documentId) {
            var n = documente.TryGetValue(documentId, out var d) ? d.Numar : null;
            return string.IsNullOrWhiteSpace(n) ? documentId.ToString("N") : n;
        }

        var discriminantPerDocument = new Dictionary<Guid, int>();
        foreach (var coliziune in emise.Select(x => x.Rand.DocumentId).Distinct()
                     .GroupBy(id => (Tip: codPerDocument.GetValueOrDefault(id) ?? "", Numar: NumarEfectiv(id)))
                     .Where(gr => gr.Count() > 1)
                     .OrderBy(gr => gr.Key.Tip, StringComparer.Ordinal)
                     .ThenBy(gr => gr.Key.Numar, StringComparer.Ordinal)) {
            var ordonate = coliziune.OrderBy(id => id).ToList();
            for (var i = 0; i < ordonate.Count; i++)
                discriminantPerDocument[ordonate[i]] = i + 1;
            Avert(CodAvertismentSaft.NumarDocumentDuplicat,
                $"{coliziune.Key.Tip} „{coliziune.Key.Numar}”: {ordonate.Count} documente cu ACELAȘI număr — "
                + $"`MovementReference` primește discriminantul `#1`…`#{ordonate.Count}`.");
        }

        var faraContStoc = new Dictionary<Guid, SaftNeinclus>();
        var tertLipsaStrigat = new HashSet<Guid>();
        var rolMixtStrigat = new HashSet<Guid>();
        var postareStrigata = new HashSet<Guid>();
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
            var numar = NumarEfectiv(g.Key.DocumentId);
            var seSparge = coduriPerDocument.GetValueOrDefault((g.Key.DocumentId, g.Key.Storno)) > 1;
            var (referinta, trunchiat) = SaftReguli.MovementReference(
                codTip, numar, seSparge ? g.Key.Cod : null, g.Key.Storno,
                discriminantPerDocument.TryGetValue(g.Key.DocumentId, out var ordinal) ? ordinal : null);
            if (trunchiat) {
                referinteTrunchiate++;
                Avert(CodAvertismentSaft.MovementReferenceTrunchiat,
                    $"{codTip} {numar} ⇒ `MovementReference` „{referinta}” "
                    + $"(max {SaftReguli.LungimeMovementReference} caractere).");
            }

            var roluriDistincte = g.Select(x => x.Regula.Rol).Where(r => r != RolTertSaft.Niciunul)
                .Distinct().ToList();
            var rolul = roluriDistincte.Count == 1
                ? roluriDistincte[0]
                : g.OrderBy(x => x.Rand.Id).First().Regula.Rol;
            if (roluriDistincte.Count > 1 && rolMixtStrigat.Add(g.Key.DocumentId))
                Avert(CodAvertismentSaft.RolTertMixt,
                    $"{codTip} {numar} (cod {g.Key.Cod}): politicile registrelor atinse cer roluri diferite "
                    + $"[{string.Join(", ", roluriDistincte.OrderBy(r => r.ToString(), StringComparer.Ordinal))}] "
                    + $"— mișcarea are un singur `CustomerID`/`SupplierID`, deci iese cu „{rolul}”.");

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
                    Quantity = r.Cantitate,
                    UomConversionFactor = 1m,
                    BookValue = r.Valoare,
                    MovementSubType = g.Key.Cod,
                });
            }
            if (linii.Count == 0)
                continue;
            coduriFolosite.TryAdd(g.Key.Cod, SaftReguli.CoduriMiscare[g.Key.Cod]);

            DateOnly? dataPostarii = null;
            if (doc?.DataOperare is DateTime op) {
                var zi = DateOnly.FromDateTime(op);
                if (zi >= dataStart && zi <= dataEnd)
                    dataPostarii = zi;
                else if (postareStrigata.Add(g.Key.DocumentId))
                    Avert(CodAvertismentSaft.DataPostariiInAfaraPerioadei,
                        $"{codTip} {numar}: `DataOperare` {zi:yyyy-MM-dd} e în afara perioadei "
                        + $"{dataStart:yyyy-MM-dd}…{dataEnd:yyyy-MM-dd} — `MovementPostingDate` se omite.");
            }

            rezultat.MiscariStoc.Add(new SaftMiscareStoc {
                DocumentId = g.Key.DocumentId,
                Storno = g.Key.Storno,
                MovementReference = referinta,
                MovementDate = g.Min(x => x.Rand.Data),
                MovementPostingDate = dataPostarii,
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
        neincluse.AddRange(codNecunoscut.Values);
        neincluse.AddRange(faraContStoc.Values);

        rezultat.NumberOfMovementLines = rezultat.MiscariStoc.Sum(m => m.Linii.Count);
        rezultat.TotalQuantityReceived = rezultat.MiscariStoc
            .SelectMany(m => m.Linii).Where(l => l.Quantity > 0m).Sum(l => l.Quantity);
        rezultat.TotalQuantityIssued = Math.Abs(rezultat.MiscariStoc
            .SelectMany(m => m.Linii).Where(l => l.Quantity < 0m).Sum(l => l.Quantity));

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

        var tipuriTva = os.GetObjectsQuery<TipTva>()
            .Select(t => new { t.ID, t.Denumire, t.Cota, t.CodSafTLivrare, t.CodSafTAchizitie })
            .ToList()
            .ToDictionary(t => t.ID);
        var codTvaFolosit = new Dictionary<string, (decimal Cota, string Denumire)>(StringComparer.Ordinal);
        var mapariFiscale = new MapariFiscale(os);
        foreach (var fapt in Fiscale.Fapte(os).Where(f => f.Data >= dataStart && f.Data <= dataEnd).ToList()) {
            var mapare = mapariFiscale.Pentru(fapt, SectiuneTvaSaft.Facturi);
            if (mapare?.TaxType == SaftReguli.TaxTypeTva)
                codTvaFolosit.TryAdd(mapare.TaxCode, (fapt.Cota, tipuriTva.GetValueOrDefault(fapt.TipTvaId)?.Denumire));
            else if (mapare == null)
                Avert(CodAvertismentSaft.TipTvaFaraCodSaft,
                    $"Mapare {MapariFiscale.Versiune}/Facturi absentă: {fapt.TipTvaId}, {fapt.Cota}%, {fapt.Sens}.", fapt.Tva);
        }
        rezultat.Taxe = TabelaTaxe(codTvaFolosit);

        rezultat.TipuriAnaliza = [];


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

        var emisePeCheie = new Dictionary<(Guid, Guid), (decimal Cantitate, decimal Valoare)>();
        foreach (var l in rezultat.MiscariStoc.SelectMany(m => m.Linii)) {
            var cheie = (l.RepartitorId, l.LotId);
            var acumulat = emisePeCheie.GetValueOrDefault(cheie);
            emisePeCheie[cheie] = (acumulat.Cantitate + l.Quantity, acumulat.Valoare + l.BookValue);
        }
        var intrariVsMiscari = 0;
        var sumaEmise = (Cantitate: 0m, Valoare: 0m);
        foreach (var e in rezultat.StocFizic) {
            var emis = emisePeCheie.GetValueOrDefault((e.RepartitorId, e.LotId));
            sumaEmise = (sumaEmise.Cantitate + emis.Cantitate, sumaEmise.Valoare + emis.Valoare);
            if (e.OpeningQuantity + emis.Cantitate != e.ClosingQuantity
                    || e.OpeningValue + emis.Valoare != e.ClosingValue)
                intrariVsMiscari++;
        }

        var balantaPeSimbol = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var b in balanta) {
            var simbol = SaftReguli.ProductTypeDinCont(b.ContSimbol);
            balantaPeSimbol[simbol] = balantaPeSimbol.GetValueOrDefault(simbol)
                + b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit;
        }
        var contIdPerSimbol = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var pereche in conturi
                .Where(c => !string.IsNullOrWhiteSpace(c.Value.Simbol))
                .OrderBy(c => c.Value.Simbol.Length)
                .ThenBy(c => c.Value.Simbol, StringComparer.Ordinal))
            contIdPerSimbol.TryAdd(SaftReguli.ProductTypeDinCont(pereche.Value.Simbol), pereche.Key);

        var perCont = rezultat.StocFizic
            .GroupBy(e => e.ProductType, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => {
                var stoc = g.Sum(e => e.ClosingValue);
                var dinBalanta = balantaPeSimbol.GetValueOrDefault(g.Key);
                return new SaftDiferentaCont {
                    Cont = g.Key,
                    ContId = contIdPerSimbol.GetValueOrDefault(g.Key),
                    ClosingStocFizic = stoc,
                    ClosingBalanta = dinBalanta,
                    Diferenta = stoc - dinBalanta,
                };
            })
            .ToList();
        ComponenteS3(os, perCont, raportate, dataEnd, conturi);

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

        var referinteDuplicate = ReferinteDuplicate(rezultat.MiscariStoc);

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
            StocFizicVsMiscariDiferite = intrariVsMiscari,
            StocEmiseCantitate = sumaEmise.Cantitate,
            StocEmiseValoare = sumaEmise.Valoare,
            StocFizicBate = intrariDiferite == 0 && intrariVsMiscari == 0,

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
            ReferinteDuplicate = referinteDuplicate,
            ReferinteBat = referiteProdus.All(coduriProdus.Contains)
                && coduriReferite.All(coduriDeclarate.Contains)
                && identitatiInvalide == 0
                && referinteDuplicate == 0,
        };

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

    sealed class RegulaMiscare {
        public Guid TipDocumentId;
        public string CodTipDocument;
        public TipStoc TipStoc;
        public int? Semn;
        public string Cod;
        public RolTertSaft Rol;
        public string Motiv;
    }

    sealed class SoldStocRand {
        public Guid RepartitorId { get; set; }
        public Guid LotId { get; set; }
        public decimal Cantitate { get; set; }
        public decimal Valoare { get; set; }
    }

    sealed class AgregatStocRand {
        public Guid RepartitorId { get; set; }
        public Guid LotId { get; set; }
        public TipStoc TipStoc { get; set; }
        public decimal CantitateInitiala { get; set; }
        public decimal ValoareInitiala { get; set; }
        public decimal CantitateRulaj { get; set; }
        public decimal ValoareRulaj { get; set; }
        public int RanduriInitiale { get; set; }
        public int Randuri { get; set; }
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

    static void ComponenteS3(
            IObjectSpace os, List<SaftDiferentaCont> perCont, List<TipStoc> raportate, DateOnly dataEnd,
            IReadOnlyDictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)> conturi) {
        if (perCont.Count == 0)
            return;
        var tinte = perCont.Select(c => c.Cont).ToHashSet(StringComparer.Ordinal);
        var simbolPerContId = new Dictionary<Guid, string>();
        foreach (var (id, info) in conturi) {
            var simbol = SaftReguli.ProductTypeDinCont(info.Simbol);
            if (tinte.Contains(simbol))
                simbolPerContId[id] = simbol;
        }
        var contIdsTinta = simbolPerContId.Keys.ToList();

        var agregatStoc = os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
            .Where(r => r.Data <= dataEnd && raportate.Contains(r.TipStoc))
            .Select(r => new {
                Simbol = r.Lot.Produs.TipMaterial.ContImplicit.Simbol, r.DocumentId, r.Valoare
            })
            .GroupBy(x => new { x.Simbol, x.DocumentId })
            .Select(g => new { g.Key.Simbol, g.Key.DocumentId, Valoare = g.Sum(x => x.Valoare) })
            .ToList();
        var agregatGl = Cub.Citiri.Contabil.Postari(os)
            .Where(r => r.Data <= dataEnd && contIdsTinta.Contains(r.Cont))
            .GroupBy(r => new { r.Cont, r.DocumentId })
            .Select(g => new {
                g.Key.Cont, g.Key.DocumentId,
                Valoare = g.Sum(r => r.Latura == Nucleu.Latura.Debit ? r.Valoare : -r.Valoare)
            })
            .ToList();
        var codPerDocument = ApiProiectii.CoduriTip(os, agregatStoc.Select(x => x.DocumentId)
            .Concat(agregatGl.Select(x => x.DocumentId))
            .Where(id => id != null).Select(id => id.Value).Distinct().ToList());

        var acumulator = new Dictionary<(string Cont, string Tip), (decimal Stoc, decimal Balanta)>();
        void Aduna(string cont, Guid? documentId, decimal stoc, decimal balanta) {
            var tip = documentId is Guid id
                ? codPerDocument.GetValueOrDefault(id) ?? "(tip necunoscut)"
                : ComponentaDeschidere;
            var cheie = (cont, tip);
            var vechi = acumulator.GetValueOrDefault(cheie);
            acumulator[cheie] = (vechi.Stoc + stoc, vechi.Balanta + balanta);
        }
        foreach (var x in agregatStoc) {
            var simbol = SaftReguli.ProductTypeDinCont(x.Simbol);
            if (tinte.Contains(simbol))
                Aduna(simbol, x.DocumentId, x.Valoare, 0m);
        }
        foreach (var x in agregatGl) {
            if (simbolPerContId.TryGetValue(x.Cont, out var simbol))
                Aduna(simbol, x.DocumentId, 0m, x.Valoare);
        }

        foreach (var cont in perCont)
            cont.Componente = acumulator
                .Where(a => a.Key.Cont == cont.Cont && (a.Value.Stoc != 0m || a.Value.Balanta != 0m))
                .Select(a => new SaftComponentaCont {
                    TipDocument = a.Key.Tip,
                    StocFizic = a.Value.Stoc,
                    Balanta = a.Value.Balanta,
                    Diferenta = a.Value.Stoc - a.Value.Balanta,
                })
                .OrderByDescending(c => Math.Abs(c.Diferenta))
                .ThenBy(c => c.TipDocument, StringComparer.Ordinal)
                .ToList();
    }

    static List<AgregatStocRand> AgregatStoc(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd) =>
        os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes()
            .Where(r => r.Data <= dataEnd)
            .GroupBy(r => new { r.RepartitorId, r.LotId, r.TipStoc })
            .Select(g => new AgregatStocRand {
                RepartitorId = g.Key.RepartitorId,
                LotId = g.Key.LotId,
                TipStoc = g.Key.TipStoc,
                CantitateInitiala = g.Sum(r => r.Data < dataStart ? r.Cantitate : 0m),
                ValoareInitiala = g.Sum(r => r.Data < dataStart ? r.Valoare : 0m),
                CantitateRulaj = g.Sum(r => r.Data >= dataStart ? r.Cantitate : 0m),
                ValoareRulaj = g.Sum(r => r.Data >= dataStart ? r.Valoare : 0m),
                RanduriInitiale = g.Sum(r => r.Data < dataStart ? 1 : 0),
                Randuri = g.Count(),
            })
            .ToList();

    static List<SoldStocRand> SoldPeCheie(
            List<AgregatStocRand> agregat, HashSet<TipStoc> raportate,
            Func<AgregatStocRand, (decimal Cantitate, decimal Valoare)> sold,
            Func<AgregatStocRand, bool> exista) =>
        agregat
            .Where(a => raportate.Contains(a.TipStoc) && exista(a))
            .GroupBy(a => (a.RepartitorId, a.LotId))
            .Select(g => new SoldStocRand {
                RepartitorId = g.Key.RepartitorId,
                LotId = g.Key.LotId,
                Cantitate = g.Sum(a => sold(a).Cantitate),
                Valoare = g.Sum(a => sold(a).Valoare),
            })
            .ToList();

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
