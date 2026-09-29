using System.Globalization;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Saft;

public static partial class SaftProiectii {
    public const string JurnalDeschidere = "DESCHIDERE";
    public const string RefuzProvenienta = "SAFT_PROVENIENTA_AMBIGUA";
    public const string RefuzSursa = "SAFT_SURSA_INCOMPLETA";
    public const string RefuzMapare = "SAFT_MAPARE_LIPSA";
    public const string RefuzCorectie = "SAFT_CORECTIE_INCOMPLETA";

    /// <summary>D406 L din cub (TR-D8 S1/S2): nomenclatoare, GL, facturi și plăți.</summary>
    public static SaftDto SaftPeCub(IObjectSpace os, int an, int luna, DateOnly? dataCreare = null) {
        using var citire = Fiscale.DeschideCitirea(os, cereIzolare: true);
        var dto = new SaftDto {
            An = an, Luna = luna,
            DataStart = new DateOnly(an, luna, 1),
            DataEnd = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna)),
        };
        dto.Neaplicabil = MotivNeaplicabil(os, "");
        if (dto.Neaplicabil == null)
            new ExportPeCub(os, dto, dataCreare).Ruleaza();
        return dto;
    }

    /// <summary>`SystemEntryDate`: data UTC a timbrului `Tranzactie.ScrisLa` (S1-R3), fără fusul mașinii care exportă.</summary>
    public static DateOnly DataSistem(DateTime scrisLa) => DateOnly.FromDateTime(
        scrisLa.Kind == DateTimeKind.Local ? scrisLa.ToUniversalTime() : scrisLa);

    sealed partial class ExportPeCub(IObjectSpace os, SaftDto dto, DateOnly? dataCreare) {
        sealed record DocInfo(Guid Id, string Numar, DateOnly Data, Guid? CorecteazaId);
        sealed record LinieInfo(int Pozitie, decimal Cantitate, Guid TipMaterialId, Guid? LotId);

        readonly DateOnly start = dto.DataStart, end = dto.DataEnd;
        readonly Dictionary<CodAvertismentSaft, List<(string Exemplu, decimal? Suma)>> avertismente = [];
        readonly Dictionary<string, (decimal Cota, string Denumire)> codTvaFolosit = new(StringComparer.Ordinal);
        readonly HashSet<Guid> adreseIncomplete = [], produseFolosite = [], tertFaraPartener = [];
        readonly List<(RolTertCont Rol, Guid PartenerId, string AccountID)> tertiReferiti = [];
        readonly MapariFiscale mapari = new(os);

        string idSocietate;
        bool raporteazaCnp;
        Dictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)> conturi;
        Dictionary<Guid, InfoPartener> parteneri;
        Dictionary<Guid, string> codPerDocument, descrieri;
        Dictionary<string, string> tipuriDocument;
        Dictionary<Guid, DocInfo> documente;
        Dictionary<Guid, LinieInfo> linii;
        Dictionary<Guid, Guid> produsPeLinie;
        Dictionary<Guid, decimal> pretPeLinie;
        Dictionary<Guid, string> tipuriMaterial;
        Dictionary<Guid, (string Cod, string Denumire)> tipuriTva;
        Dictionary<(Guid Tranzactie, Guid Linie), List<FaptFiscal>> fapte;
        EtichetePerioada etichete;

        void Avert(CodAvertismentSaft cod, string exemplu, decimal? suma = null) {
            if (!avertismente.TryGetValue(cod, out var lista)) lista = avertismente[cod] = [];
            lista.Add((exemplu, suma));
        }

        void Refuza(string cod, string mesaj, Guid? documentId = null, Guid? tranzactieId = null) =>
            dto.Refuzuri.Add(new SaftRefuz { Cod = cod, Mesaj = mesaj, DocumentId = documentId, TranzactieId = tranzactieId });

        RolTertCont Rol(Guid cont) => conturi.TryGetValue(cont, out var c) ? c.RolTert : RolTertCont.Niciunul;
        string Simbol(Guid cont) => SaftReguli.SimbolSaft(conturi.TryGetValue(cont, out var c) ? c.Simbol : null);
        string CodTip(Guid? document) => document is Guid d ? codPerDocument.GetValueOrDefault(d) : null;

        string Eticheta(Guid? document) {
            var doc = document is Guid d ? documente.GetValueOrDefault(d) : null;
            var cod = CodTip(document);
            return doc == null ? JurnalDeschidere : $"{cod} {doc.Numar} din {doc.Data:dd.MM.yyyy}";
        }

        public void Ruleaza() {
            var soc = CitesteSocietate(os);
            VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));
            idSocietate = soc == null ? null : SaftReguli.IdSocietate(soc.CodFiscal, soc.Tara);
            raporteazaCnp = soc?.RaporteazaCnp ?? false;
            dto.Header = Antet(soc, dto.An, dto.Luna, dataCreare, HeaderComment);

            conturi = CitesteConturi(os);
            var (conturiSaft, balanta) = ConturiSiSolduri(os, start, end, conturi, (cod, exemplu) => Avert(cod, exemplu));
            dto.Conturi = conturiSaft;

            var jurnal = Contabil.Jurnal(os, start, end).ToList();
            fapte = Fiscale.Fapte(os).Where(f => f.Data >= start && f.Data <= end).ToList()
                .GroupBy(f => (f.TranzactieId, f.DetaliuId)).ToDictionary(g => g.Key, g => g.ToList());
            tipuriTva = os.GetObjectsQuery<TipTva>().Select(t => new { t.ID, t.Cod, t.Denumire }).ToList()
                .ToDictionary(t => t.ID, t => (t.Cod, t.Denumire));
            Documente(jurnal);
            var plati = PregatestePlati(jurnal);
            var agregateTert = Parteneri(jurnal, plati.Values.Select(p => p.Extern).OfType<Guid>());

            Gl(jurnal);
            CorectiiIncomplete(jurnal);
            dto.FacturiEmise = Facturi(jurnal, vanzare: true);
            dto.FacturiPrimite = Facturi(jurnal, vanzare: false);
            Plati(jurnal, plati);
            Terti(agregateTert);

            var (produse, unitati) = ProduseSiUnitati(os, produseFolosite.ToList(), (cod, exemplu) => Avert(cod, exemplu));
            dto.Produse = produse;
            dto.Unitati = unitati;
            var produsPeId = produse.ToDictionary(p => p.ProdusId);
            foreach (var l in dto.FacturiEmise.Concat(dto.FacturiPrimite).SelectMany(f => f.Linii)) {
                if (l.ProductCode == null || !Guid.TryParse(l.ProductCode, out var id) || !produsPeId.TryGetValue(id, out var p))
                    continue;
                l.ProductCode = p.ProductCode;
                l.ProductDescription = p.Description;
                l.InvoiceUOM = p.UOMBase;
            }
            dto.Taxe = TabelaTaxe(codTvaFolosit);
            dto.TipuriAnaliza = etichete.Lista();
            Rezumat(balanta);

            dto.Avertismente = avertismente.OrderBy(a => a.Key).Select(a => new SaftAvertisment {
                Cod = a.Key.ToString(), Mesaj = Mesaj(a.Key), Numar = a.Value.Count,
                Suma = a.Value.Any(x => x.Suma != null) ? a.Value.Sum(x => x.Suma ?? 0m) : null,
                Exemple = a.Value.Take(5).Select(x => x.Exemplu).ToList(),
            }).ToList();
            dto.Neincluse = dto.Neincluse.OrderBy(n => n.Cauza, StringComparer.Ordinal)
                .ThenBy(n => n.Sectiune ?? "", StringComparer.Ordinal)
                .ThenBy(n => n.ContSimbol ?? "", StringComparer.Ordinal).ToList();
        }

        void Documente(List<PostareJurnal> jurnal) {
            var ids = jurnal.Where(p => p.DocumentId != null).Select(p => p.DocumentId.Value).Distinct().ToList();
            codPerDocument = ApiProiectii.CoduriTip(os, ids);
            tipuriDocument = os.GetObjectsQuery<TipDocument>().Select(t => new { t.Cod, t.Denumire }).ToList()
                .GroupBy(t => t.Cod, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.First().Denumire, StringComparer.Ordinal);
            documente = os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID))
                .Select(d => new DocInfo(d.ID, d.Numar, d.Data, d.CorecteazaId)).ToList()
                .ToDictionary(d => d.Id);

            var idsLinie = jurnal.Where(p => p.LinieId != null).Select(p => p.LinieId.Value).Distinct().ToList();
            linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsLinie.Contains(d.ID))
                .Select(d => new { d.ID, d.Pozitie, d.Cantitate, d.TipMaterialId, d.LotId }).ToList()
                .ToDictionary(d => d.ID, d => new LinieInfo(d.Pozitie, d.Cantitate, d.TipMaterialId, d.LotId));
            produsPeLinie = [];
            pretPeLinie = [];
            foreach (var x in os.GetObjectsQuery<FacturaIntrareDetaliu>().Where(d => idsLinie.Contains(d.ID))
                         .Select(d => new { d.ID, d.PretUnitar, d.ProdusId }).ToList()) {
                pretPeLinie[x.ID] = x.PretUnitar;
                if (x.ProdusId is Guid p) produsPeLinie[x.ID] = p;
            }
            foreach (var x in os.GetObjectsQuery<FacturaIesireDetaliu>().Where(d => idsLinie.Contains(d.ID))
                         .Select(d => new { d.ID, d.PretUnitar, d.ProdusId }).ToList()) {
                pretPeLinie[x.ID] = x.PretUnitar;
                if (x.ProdusId is Guid p) produsPeLinie[x.ID] = p;
            }
            var idsLot = linii.Values.Where(l => l.LotId != null).Select(l => l.LotId.Value).Distinct().ToList();
            var produsPeLot = os.GetObjectsQuery<Lot>().Where(l => idsLot.Contains(l.ID))
                .Select(l => new { l.ID, l.ProdusId }).ToList().ToDictionary(l => l.ID, l => l.ProdusId);
            foreach (var (id, l) in linii)
                if (!produsPeLinie.ContainsKey(id) && l.LotId is Guid lot && produsPeLot.TryGetValue(lot, out var p))
                    produsPeLinie[id] = p;
            tipuriMaterial = os.GetObjectsQuery<TipMaterial>().Select(t => new { t.ID, t.Denumire }).ToList()
                .ToDictionary(t => t.ID, t => t.Denumire);

            descrieri = [];
            foreach (var x in os.GetObjectsQuery<NotaContabilaDetaliu>().Where(d => idsLinie.Contains(d.ID))
                         .Select(d => new { d.ID, d.Descriere }).ToList())
                if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;
            foreach (var x in os.GetObjectsQuery<DecontDetaliu>().Where(d => idsLinie.Contains(d.ID))
                         .Select(d => new { d.ID, d.Descriere }).ToList())
                if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;
            foreach (var x in os.GetObjectsQuery<FacturaIesireDetaliu>().Where(d => idsLinie.Contains(d.ID))
                         .Select(d => new { d.ID, d.Descriere }).ToList())
                if (!string.IsNullOrWhiteSpace(x.Descriere)) descrieri[x.ID] = x.Descriere;
        }

        sealed record AgregatTert(Guid ContId, Guid? RepartitorId,
            decimal InitialDebit, decimal InitialCredit, decimal RulajDebit, decimal RulajCredit);

        List<AgregatTert> Parteneri(List<PostareJurnal> jurnal, IEnumerable<Guid> suplimentari) {
            var conturiCuRol = conturi.Where(c => c.Value.RolTert != RolTertCont.Niciunul).Select(c => c.Key).ToList();
            var agregate = ContabilProiectii.Atomi(os)
                .Where(r => r.Data <= end && conturiCuRol.Contains(r.ContId))
                .GroupBy(r => new { r.ContId, r.RepartitorId })
                .Select(g => new {
                    g.Key.ContId, g.Key.RepartitorId,
                    InitialDebit = g.Sum(r => r.Data < start ? r.Debit : 0m),
                    InitialCredit = g.Sum(r => r.Data < start ? r.Credit : 0m),
                    RulajDebit = g.Sum(r => r.Data >= start ? r.Debit : 0m),
                    RulajCredit = g.Sum(r => r.Data >= start ? r.Credit : 0m),
                }).ToList()
                .Select(a => new AgregatTert(a.ContId, a.RepartitorId, a.InitialDebit, a.InitialCredit, a.RulajDebit, a.RulajCredit))
                .ToList();

            var ids = jurnal.Where(p => p.Partener != null).Select(p => p.Partener.Value)
                .Concat(agregate.Where(a => a.RepartitorId != null).Select(a => a.RepartitorId.Value))
                .Concat(jurnal.Where(p => p.CentruCost != null).Select(p => p.CentruCost.Value))
                .Concat(suplimentari)
                .Distinct().ToList();
            parteneri = os.GetObjectsQuery<Partener>().Where(p => ids.Contains(p.ID))
                .Select(p => new {
                    p.ID, p.Cod, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare,
                    p.Strada, p.Numar, p.DetaliiAdresa, p.Localitate, p.CodPostal, JudetCod = p.Judet.Cod,
                }).ToList()
                .ToDictionary(p => p.ID, p => {
                    var identitate = SaftReguli.IdPartener(
                        p.TipPersoana, p.Tara, p.InregistratTva, p.CodFiscal, p.Cod, p.ID, raporteazaCnp);
                    return new InfoPartener {
                        Id = p.ID, Cod = p.Cod, Denumire = p.Denumire, CodFiscal = p.CodFiscal,
                        TipPersoana = p.TipPersoana, Tara = p.Tara,
                        InregistratTva = p.InregistratTva, TvaLaIncasare = p.TvaLaIncasare,
                        Strada = p.Strada, Numar = p.Numar, DetaliiAdresa = p.DetaliiAdresa,
                        Localitate = p.Localitate, CodPostal = p.CodPostal, JudetCod = p.JudetCod,
                        Id406 = identitate.Id, Fel = identitate.Fel,
                    };
                });
            var repartitori = os.GetObjectsQuery<Repartitor>().Where(r => ids.Contains(r.ID))
                .Select(r => new { r.ID, r.Cod, r.Denumire }).ToList()
                .ToDictionary(r => r.ID, r => (r.Cod, r.Denumire));
            etichete = new EtichetePerioada(os, repartitori);
            return agregate;
        }

        SaftAdresa AdresaPartener(InfoPartener p) {
            var tara = SaftReguli.CodTaraSaft(p.Tara);
            var oras = p.Localitate;
            if (string.IsNullOrWhiteSpace(oras)) {
                oras = LocalitateImplicita;
                if (adreseIncomplete.Add(p.Id))
                    Avert(CodAvertismentSaft.AdresaIncompleta,
                        $"„{p.Denumire}” n-are localitate — `City` e obligatoriu în `AddressStructure`, deci iese "
                        + $"„{LocalitateImplicita}”.");
            }
            return new SaftAdresa {
                StreetName = p.Strada, Number = p.Numar, AdditionalAddressDetail = p.DetaliiAdresa,
                City = oras, PostalCode = p.CodPostal, Region = tara == "RO" ? p.JudetCod : null, Country = tara,
            };
        }

        List<SaftAnaliza> Analiza(PostareJurnal p) =>
            Analiza(p.CentruCost, p.Proiect, p.UnitateOrganizatorica, p.SursaFinantare, p.CodFunctional, p.CodEconomic);

        List<SaftAnaliza> Analiza(Guid? cc, Guid? proiect, Guid? unitate, Guid? sf, Guid? cf, Guid? ce) {
            var lista = new List<SaftAnaliza>();
            void Adauga(string tip, Guid? id) {
                if (id is Guid v) lista.Add(new SaftAnaliza { AnalysisType = tip, AnalysisID = etichete.Inregistreaza(tip, v) });
            }
            Adauga("CC", cc);
            Adauga("P", proiect);
            Adauga("U", unitate);
            Adauga("SF", sf);
            Adauga("CF", cf);
            Adauga("CE", ce);
            return lista;
        }

        FaptFiscal FaptulPostarii(PostareJurnal p) {
            if (p.LinieId is not Guid linie || !fapte.TryGetValue((p.TranzactieId, linie), out var candidati))
                return null;
            var potrivite = candidati.Where(f => f.TipTvaId == p.TipTvaId && (int)f.Sens == (int?)p.SensTva).ToList();
            return potrivite.Count == 1 ? potrivite[0] : null;
        }

        static SaftTaxInfo Nefiscal() => new() {
            TaxType = SaftReguli.TaxTypeNefiscal, TaxCode = SaftReguli.TaxCodeNefiscal, TaxAmount = 0m,
        };

        SaftTaxInfo Taxa(FaptFiscal fapt, SectiuneTvaSaft sectiune, N.RolTva rol, decimal suma, Guid? document, Guid tranzactie) {
            var mapare = mapari.Pentru(fapt, sectiune, rol);
            if (mapare == null) {
                var tip = tipuriTva.GetValueOrDefault(fapt.TipTvaId);
                Refuza(RefuzMapare, $"{Eticheta(document)}: mapare {MapariFiscale.Versiune}/{sectiune} absentă pentru "
                    + $"{tip.Cod}, {fapt.Cota}%, {fapt.Sens}, {rol}.", document, tranzactie);
                return Nefiscal();
            }
            if (mapare.TaxType == SaftReguli.TaxTypeTva)
                codTvaFolosit.TryAdd(mapare.TaxCode, (fapt.Cota, tipuriTva.GetValueOrDefault(fapt.TipTvaId).Denumire));
            return new SaftTaxInfo {
                TaxType = mapare.TaxType, TaxCode = mapare.TaxCode,
                TaxPercentage = fapt.Cota, TaxBase = fapt.Baza, TaxAmount = suma,
            };
        }

        (string Customer, string Supplier) Identitati(PostareJurnal p) {
            var rol = Rol(p.Cont);
            if (rol == RolTertCont.Niciunul)
                return (idSocietate, idSocietate);
            if (p.Partener is not Guid pid || !parteneri.TryGetValue(pid, out var partener)) {
                if (tertFaraPartener.Add(p.Id))
                    Avert(CodAvertismentSaft.TertFaraPartener,
                        $"{Eticheta(p.DocumentId)}: postarea pe contul {Simbol(p.Cont)} n-are partener — "
                        + "`CustomerID`/`SupplierID` ies cu codul societății.", p.Valoare);
                return (idSocietate, idSocietate);
            }
            tertiReferiti.Add((rol, pid, Simbol(p.Cont)));
            return rol == RolTertCont.Client ? (partener.Id406, idSocietate) : (idSocietate, partener.Id406);
        }

        void Gl(List<PostareJurnal> jurnal) {
            var jurnale = new Dictionary<string, SaftJurnal>(StringComparer.Ordinal);
            foreach (var tx in jurnal.GroupBy(p => p.TranzactieId)
                         .OrderBy(g => g.First().DataTranzactie)
                         .ThenBy(g => g.First().DocumentId is Guid d && documente.TryGetValue(d, out var doc) ? doc.Numar ?? "" : "",
                             StringComparer.Ordinal)
                         .ThenBy(g => g.Key)) {
                var prima = tx.First();
                var cod = prima.DocumentId == null ? JurnalDeschidere : CodTip(prima.DocumentId) ?? "?";
                if (!jurnale.TryGetValue(cod, out var j))
                    j = jurnale[cod] = new SaftJurnal {
                        JournalID = cod, Type = cod,
                        Description = tipuriDocument.GetValueOrDefault(cod) ?? (cod == JurnalDeschidere ? "Deschidere" : cod),
                    };
                var debit = tx.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare);
                var credit = tx.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare);
                if (debit != credit)
                    Refuza(RefuzProvenienta, $"{Eticheta(prima.DocumentId)}: tranzacția {tx.Key} nu e echilibrată "
                        + $"(D {debit}, C {credit}).", prima.DocumentId, tx.Key);
                var doc = prima.DocumentId is Guid id ? documente.GetValueOrDefault(id) : null;
                var descriere = $"{tipuriDocument.GetValueOrDefault(cod) ?? cod} {doc?.Numar}".Trim()
                    + (prima.Fel == N.FelTranzactie.Storno ? " (storno)" : "");
                var tranzactie = new SaftTranzactie {
                    DocumentId = prima.DocumentId ?? Guid.Empty,
                    TransactionID = tx.Key.ToString(),
                    Period = dto.Luna, PeriodYear = dto.An,
                    TransactionDate = prima.DataTranzactie,
                    GLPostingDate = prima.DataTranzactie,
                    SystemEntryDate = DataSistem(prima.ScrisLa),
                    Description = descriere,
                    CustomerID = PartenerulTranzactiei(tx, RolTertCont.Client) ?? idSocietate,
                    SupplierID = PartenerulTranzactiei(tx, RolTertCont.Furnizor) ?? idSocietate,
                };
                var pozitie = 0;
                foreach (var p in tx.OrderBy(p => p.Spatiu).ThenBy(p => p.Id)) {
                    var (customer, supplier) = Identitati(p);
                    tranzactie.Linii.Add(new SaftLinieTranzactie {
                        RandRegistruId = p.Id,
                        Spatiu = p.Spatiu,
                        DetaliuId = p.LinieId,
                        RecordID = (++pozitie).ToString(CultureInfo.InvariantCulture),
                        AccountID = Simbol(p.Cont),
                        CustomerID = customer, SupplierID = supplier,
                        Description = (p.LinieId is Guid l ? descrieri.GetValueOrDefault(l) : null) ?? descriere,
                        DebitCreditIndicator = p.Latura == N.Latura.Debit ? "D" : "C",
                        Amount = p.Valoare, CurrencyCode = DefaultCurrencyCode, CurrencyAmount = p.Valoare,
                        Analiza = Analiza(p),
                        TaxInformation = TaxaGl(p, cod),
                    });
                }
                j.Tranzactii.Add(tranzactie);
            }
            dto.Jurnale = jurnale.Values.OrderBy(j => j.JournalID, StringComparer.Ordinal).ToList();
        }

        string PartenerulTranzactiei(IEnumerable<PostareJurnal> tx, RolTertCont rol) {
            var ids = tx.Where(p => Rol(p.Cont) == rol && p.Partener != null).Select(p => p.Partener.Value)
                .Where(parteneri.ContainsKey).Distinct().ToList();
            return ids.Count == 1 ? parteneri[ids[0]].Id406 : null;
        }

        SaftTaxInfo TaxaGl(PostareJurnal p, string codTip) {
            if (codTip == CodTipInchidereTva) {
                codTvaFolosit.TryAdd(SaftReguli.TaxCodeInchidereTva, (0m, "TVA — note contabile"));
                return new() { TaxType = SaftReguli.TaxTypeTva, TaxCode = SaftReguli.TaxCodeInchidereTva, TaxAmount = 0m };
            }
            if (p.TipTvaId == null || p.RolTva is not (N.RolTva.Taxa or N.RolTva.Autocolectare))
                return Nefiscal();
            if (FaptulPostarii(p) is not { } fapt) {
                Refuza(RefuzProvenienta, $"{Eticheta(p.DocumentId)}: postarea fiscală {p.Spatiu}/{p.Id} nu are un fapt "
                    + "fiscal unic pe tranzacția și linia ei.", p.DocumentId, p.TranzactieId);
                return Nefiscal();
            }
            return Taxa(fapt, SectiuneTvaSaft.GeneralLedger, p.RolTva.Value, p.Valoare, p.DocumentId, p.TranzactieId);
        }

        void CorectiiIncomplete(List<PostareJurnal> jurnal) {
            var stornate = jurnal.Where(p => p.Fel == N.FelTranzactie.Storno && p.DocumentId != null)
                .Select(p => p.DocumentId.Value).Distinct().ToList();
            foreach (var d in os.GetObjectsQuery<Document>()
                         .Where(d => d.CorecteazaId != null && stornate.Contains(d.CorecteazaId.Value) && d.Stare == StareDocument.Draft)
                         .Select(d => new { d.ID, d.CorecteazaId }).ToList())
                Refuza(RefuzCorectie, $"{Eticheta(d.CorecteazaId)}: stornoul corecției e operat, înlocuitorul e încă Draft.",
                    d.CorecteazaId);
        }

        List<SaftFactura> Facturi(List<PostareJurnal> jurnal, bool vanzare) {
            var tipuri = vanzare ? TipuriVanzare : TipuriCumparare;
            var lista = new List<SaftFactura>();
            foreach (var tx in jurnal
                         .Where(p => p.Fel is N.FelTranzactie.Operare or N.FelTranzactie.Storno && tipuri.Contains(CodTip(p.DocumentId)))
                         .GroupBy(p => p.TranzactieId)
                         .OrderBy(g => g.First().DataTranzactie)
                         .ThenBy(g => documente[g.First().DocumentId.Value].Numar ?? "", StringComparer.Ordinal)
                         .ThenBy(g => g.Key))
                if (Factura(tx.Key, tx.ToList(), vanzare) is { } factura)
                    lista.Add(factura);
            foreach (var coliziune in lista.GroupBy(f => (f.InvoiceNo ?? "", f.InvoiceType))
                         .Where(g => g.Count() > 1).OrderBy(g => g.Key.Item1, StringComparer.Ordinal))
                Avert(CodAvertismentSaft.NumarFacturaDuplicat,
                    $"{(vanzare ? "SalesInvoices" : "PurchaseInvoices")} „{coliziune.Key.Item1}” ({coliziune.Key.InvoiceType}): "
                    + $"{coliziune.Count()} evenimente cu același număr și tip.");
            return lista;
        }

        SaftFactura Factura(Guid tranzactieId, List<PostareJurnal> tx, bool vanzare) {
            var prima = tx[0];
            var docId = prima.DocumentId.Value;
            var doc = documente[docId];
            var eticheta = Eticheta(docId);
            var rol = vanzare ? RolTertCont.Client : RolTertCont.Furnizor;
            var laturaTert = vanzare ? N.Latura.Debit : N.Latura.Credit;
            decimal Comercial(PostareJurnal p) => p.Latura == laturaTert ? p.Valoare : -p.Valoare;
            decimal Contrapartida(PostareJurnal p) => -Comercial(p);
            bool EsteTert(PostareJurnal p) => Rol(p.Cont) == rol;

            var terti = tx.Where(EsteTert).ToList();
            if (terti.Count == 0) {
                if (fapte.Keys.Any(k => k.Tranzactie == tranzactieId))
                    Refuza(RefuzSursa, $"{eticheta}: fapte fiscale fără niciun cont cu rol de {rol} pe eveniment.", docId, tranzactieId);
                return null;
            }
            var conturiTert = terti.Select(p => p.Cont).Distinct().ToList();
            var parteneriTert = terti.Select(p => p.Partener).Distinct().ToList();
            if (conturiTert.Count != 1 || parteneriTert.Count != 1) {
                Refuza(RefuzProvenienta, $"{eticheta}: {conturiTert.Count} conturi și {parteneriTert.Count} parteneri de terț "
                    + "pe eveniment; factura cere câte unul.", docId, tranzactieId);
                return null;
            }
            if (parteneriTert[0] is not Guid pid || !parteneri.TryGetValue(pid, out var partener)) {
                Refuza(RefuzSursa, $"{eticheta}: postările de terț n-au un partener declarabil.", docId, tranzactieId);
                return null;
            }

            var brut = terti.Sum(Comercial);
            var factura = new SaftFactura {
                DocumentId = docId,
                Storno = prima.Fel == N.FelTranzactie.Storno,
                DocumentTip = CodTip(docId),
                InvoiceNo = doc.Numar,
                InvoiceType = SaftReguli.InvoiceTypeEveniment(prima.Fel == N.FelTranzactie.Storno, doc.CorecteazaId != null, brut),
                SelfBillingIndicator = "0",
                GLPostingDate = prima.DataTranzactie,
                TransactionID = tranzactieId.ToString(),
                AccountID = Simbol(conturiTert[0]),
                PartenerID = partener.Id406, PartenerCheie = partener.Id, PartenerDenumire = partener.Denumire,
                BillingAddress = AdresaPartener(partener),
                GrossTotal = brut,
            };
            tertiReferiti.Add((rol, partener.Id, factura.AccountID));

            var dateDocument = new HashSet<DateOnly>();
            var conservat = 0m;
            var liniiFactura = new List<(int Pozitie, SaftLinieFactura Linie)>();
            foreach (var grup in tx.Where(p => p.LinieId != null).GroupBy(p => p.LinieId.Value)) {
                var linieId = grup.Key;
                if (!linii.TryGetValue(linieId, out var info)) {
                    Refuza(RefuzSursa, $"{eticheta}: linia {linieId} a cubului nu există în document.", docId, tranzactieId);
                    continue;
                }
                var subLinii = new List<(Guid Cont, decimal Net, SaftTaxInfo Taxa, DateOnly? Exigibil, PostareJurnal Sursa)>();
                if (fapte.TryGetValue((tranzactieId, linieId), out var fapteLinie)) {
                    var baze = grup.Where(p => p.RolTva == N.RolTva.Baza).ToList();
                    var conturiBaza = baze.Select(p => p.Cont).Distinct().ToList();
                    if (fapteLinie.Count != 1 || conturiBaza.Count != 1) {
                        Refuza(RefuzProvenienta, $"{eticheta}: linia {info.Pozitie} are {fapteLinie.Count} fapte fiscale și "
                            + $"{conturiBaza.Count} conturi de bază; factura cere câte unul.", docId, tranzactieId);
                        continue;
                    }
                    var fapt = fapteLinie[0];
                    dateDocument.Add(fapt.DataDocument);
                    conservat += fapt.Baza + fapt.Tva - fapt.Autocolectare;
                    subLinii.Add((conturiBaza[0], fapt.Baza,
                        Taxa(fapt, SectiuneTvaSaft.Facturi, N.RolTva.Taxa, fapt.Tva, docId, tranzactieId),
                        fapt.DataExigibilitate, baze[0]));
                } else {
                    var tertLinie = grup.Where(EsteTert).ToList();
                    if (tertLinie.Count == 0)
                        continue;
                    var net = tertLinie.Sum(Comercial);
                    var contrapartide = grup.Where(p => !EsteTert(p)).ToList();
                    if (contrapartide.Select(p => p.Cont).Distinct().Count() != 1 || contrapartide.Sum(Contrapartida) != net) {
                        Refuza(RefuzProvenienta, $"{eticheta}: linia nefiscală {info.Pozitie} are net {net} fără o contrapartidă "
                            + "unică și egală.", docId, tranzactieId);
                        continue;
                    }
                    conservat += net;
                    subLinii.Add((contrapartide[0].Cont, net, Nefiscal(), null, contrapartide[0]));
                }
                var cantitate = Math.Abs(info.Cantitate);
                if (cantitate == 0m) {
                    Refuza(RefuzSursa, $"{eticheta}: linia {info.Pozitie} are cantitatea comercială 0.", docId, tranzactieId);
                    continue;
                }
                produsPeLinie.TryGetValue(linieId, out var produs);
                if (produs != Guid.Empty) produseFolosite.Add(produs);
                foreach (var s in subLinii) {
                    var pret = pretPeLinie.TryGetValue(linieId, out var pu) && pu != 0m && subLinii.Count == 1
                        ? pu : Scara.RotunjestePret(Math.Abs(s.Net) / cantitate);
                    liniiFactura.Add((info.Pozitie, new SaftLinieFactura {
                        DetaliuId = linieId,
                        AccountID = Simbol(s.Cont),
                        ProductCode = produs == Guid.Empty ? null : produs.ToString(),
                        Quantity = cantitate,
                        UnitPrice = pret,
                        TaxPointDate = s.Exigibil ?? default,
                        Description = descrieri.GetValueOrDefault(linieId)
                            ?? tipuriMaterial.GetValueOrDefault(info.TipMaterialId) ?? doc.Numar,
                        InvoiceLineAmount = s.Net,
                        DebitCreditIndicator = vanzare ? "C" : "D",
                        Analiza = Analiza(s.Sursa),
                        TaxInformation = s.Taxa,
                    }));
                }
            }

            if (dateDocument.Count > 1) {
                Refuza(RefuzProvenienta, $"{eticheta}: faptele fiscale au {dateDocument.Count} date de document diferite.",
                    docId, tranzactieId);
                return null;
            }
            if (conservat != brut) {
                Refuza(RefuzProvenienta, $"{eticheta}: brutul comercial {brut} diferă de Σ(net + taxă − autocolectare) = "
                    + $"{conservat} pe liniile facturii.", docId, tranzactieId);
                return null;
            }
            factura.InvoiceDate = dateDocument.Count == 1 ? dateDocument.First() : doc.Data;
            var pozitie = 0;
            foreach (var (_, l) in liniiFactura.OrderBy(x => x.Pozitie)) {
                l.LineNumber = ++pozitie;
                if (l.TaxPointDate == default) l.TaxPointDate = factura.InvoiceDate;
                factura.Linii.Add(l);
            }
            factura.NetTotal = factura.Linii.Sum(l => l.InvoiceLineAmount);
            factura.TaxInformationTotals = factura.Linii
                .GroupBy(l => (l.TaxInformation.TaxType, l.TaxInformation.TaxCode))
                .Select(g => new SaftTaxInfo {
                    TaxType = g.Key.TaxType, TaxCode = g.Key.TaxCode,
                    TaxBase = g.Sum(l => l.TaxInformation.TaxBase ?? 0m),
                    TaxAmount = g.Sum(l => l.TaxInformation.TaxAmount),
                })
                .OrderBy(t => t.TaxCode, StringComparer.Ordinal).ToList();
            return factura;
        }

        void Terti(List<AgregatTert> agregate) {
            var solduri = new Dictionary<(Guid Partener, RolTertCont Rol, Guid Cont), SoldTert>();
            var neincluse = new Dictionary<(CauzaNeincludere, Guid, Guid?), SaftNeinclus>();
            foreach (var a in agregate) {
                var rol = Rol(a.ContId);
                if (a.RepartitorId is not Guid rep || !parteneri.ContainsKey(rep)) {
                    var cauza = a.RepartitorId != null ? CauzaNeincludere.RepartitorNePartener : CauzaNeincludere.FaraPartener;
                    var cheieN = (cauza, a.ContId, a.RepartitorId);
                    if (!neincluse.TryGetValue(cheieN, out var n))
                        n = neincluse[cheieN] = new SaftNeinclus {
                            Cauza = cauza.ToString(),
                            Sectiune = rol == RolTertCont.Client ? "Customers" : "Suppliers",
                            ContId = a.ContId,
                            ContSimbol = conturi.TryGetValue(a.ContId, out var ci) ? ci.Simbol : null,
                            RepartitorId = a.RepartitorId,
                            Debit = 0m, Credit = 0m,
                        };
                    n.Debit += a.InitialDebit + a.RulajDebit;
                    n.Credit += a.InitialCredit + a.RulajCredit;
                    n.Randuri++;
                    continue;
                }
                var cheie = (rep, rol, a.ContId);
                if (!solduri.TryGetValue(cheie, out var sold)) sold = solduri[cheie] = new SoldTert();
                sold.InitialDebit += a.InitialDebit; sold.RulajDebit += a.RulajDebit;
                sold.InitialCredit += a.InitialCredit; sold.RulajCredit += a.RulajCredit;
            }
            dto.Neincluse.AddRange(neincluse.Values.Where(n => n.Debit != 0m || n.Credit != 0m));

            var terti = new Dictionary<(RolTertCont Rol, string Id), SaftTert>();
            foreach (var g in solduri.GroupBy(s => (s.Key.Partener, s.Key.Rol)).OrderBy(g => g.Key.Rol).ThenBy(g => g.Key.Partener)) {
                var p = parteneri[g.Key.Partener];
                var deschidere = g.Sum(x => x.Value.NetInitial);
                var inchidere = g.Sum(x => x.Value.Net);
                if (deschidere == 0m && inchidere == 0m && g.Sum(x => x.Value.Miscare) == 0m)
                    continue;
                var contPrincipal = g.OrderByDescending(x => x.Value.Miscare)
                    .ThenByDescending(x => Math.Abs(x.Value.Net))
                    .ThenBy(x => conturi.TryGetValue(x.Key.Cont, out var c) ? c.Simbol : "", StringComparer.Ordinal)
                    .First().Key.Cont;
                var tert = Tert(p, Simbol(contPrincipal), deschidere, inchidere);
                if (terti.TryGetValue((g.Key.Rol, tert.Id), out var existent)) {
                    Avert(CodAvertismentSaft.PartenerDublat,
                        $"{(g.Key.Rol == RolTertCont.Client ? "Clientul" : "Furnizorul")} „{p.Denumire}” are același "
                        + $"identificator SAF-T ({tert.Id}) ca „{existent.Name}” — se declară o singură intrare, cu "
                        + "soldurile cumulate.");
                    CumuleazaSolduri(existent, tert);
                    continue;
                }
                terti[(g.Key.Rol, tert.Id)] = tert;
                if (p.Fel == FelIdSaft.CodIntern && !string.IsNullOrWhiteSpace(p.CodFiscal)
                        && (p.InregistratTva || Partener.NormalizeazaTara(p.Tara) == "RO"))
                    Avert(CodAvertismentSaft.PartenerFaraCuiValid,
                        $"„{p.Denumire}” e român sau înregistrat în scopuri de TVA, dar codul fiscal "
                        + $"(„{p.CodFiscal}”) nu trece cifra de control — se declară cu prefixul 04 "
                        + "(cod intern), nu 00.");
            }
            foreach (var (rol, partenerId, accountId) in tertiReferiti) {
                var p = parteneri[partenerId];
                terti.TryAdd((rol, p.Id406), Tert(p, accountId, 0m, 0m));
            }
            dto.Clienti = terti.Where(t => t.Key.Rol == RolTertCont.Client).Select(t => t.Value)
                .OrderBy(t => t.Id, StringComparer.Ordinal).ToList();
            dto.Furnizori = terti.Where(t => t.Key.Rol == RolTertCont.Furnizor).Select(t => t.Value)
                .OrderBy(t => t.Id, StringComparer.Ordinal).ToList();
        }

        SaftTert Tert(InfoPartener p, string accountId, decimal deschidere, decimal inchidere) => new() {
            PartenerId = p.Id, Id = p.Id406, RegistrationNumber = p.Id406, Name = p.Denumire,
            Address = AdresaPartener(p),
            TaxRegistrationNumber = p.Fel == FelIdSaft.CuiRoman ? p.Id406[2..] : null,
            TaxType = SaftReguli.RegimFiscalPartener(p.Fel, p.InregistratTva, p.TvaLaIncasare),
            AccountID = accountId,
            OpeningDebitBalance = deschidere >= 0m ? deschidere : null,
            OpeningCreditBalance = deschidere < 0m ? -deschidere : null,
            ClosingDebitBalance = inchidere >= 0m ? inchidere : null,
            ClosingCreditBalance = inchidere < 0m ? -inchidere : null,
            FelId = p.Fel.ToString(),
        };

        void Rezumat(List<BalantaRand> balanta) {
            var liniiGl = dto.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).ToList();
            var totalDebit = liniiGl.Where(l => l.DebitCreditIndicator == "D").Sum(l => l.Amount);
            var totalCredit = liniiGl.Where(l => l.DebitCreditIndicator == "C").Sum(l => l.Amount);
            if (totalDebit != totalCredit)
                Refuza(RefuzProvenienta, $"GL-ul lunii nu e echilibrat (D {totalDebit}, C {totalCredit}).");
            decimal Inchidere(IEnumerable<SaftTert> terti) =>
                terti.Sum(t => (t.ClosingDebitBalance ?? 0m) - (t.ClosingCreditBalance ?? 0m));
            decimal InchidereGla(RolTertCont rol) => dto.Conturi.Where(c => Rol(c.ContId) == rol)
                .Sum(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m));
            dto.Rezumat = new SaftRezumat {
                Tranzactii = dto.Jurnale.Sum(j => j.Tranzactii.Count),
                LiniiGl = liniiGl.Count,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                TvaGl = liniiGl.Where(l => l.TaxInformation.TaxType == SaftReguli.TaxTypeTva).Sum(l => l.TaxInformation.TaxAmount),
                BazaFacturiAchizitie = dto.FacturiPrimite.SelectMany(f => f.Linii).Sum(l => l.TaxInformation.TaxBase ?? 0m),
                BazaFacturiLivrare = dto.FacturiEmise.SelectMany(f => f.Linii).Sum(l => l.TaxInformation.TaxBase ?? 0m),
                ClosingGla = dto.Conturi.Sum(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m)),
                ClosingBalanta = balanta.Sum(b => b.InitialDebit - b.InitialCredit + b.RulajDebit - b.RulajCredit),
                ConturiVerificate = dto.Conturi.Count,
                ClosingClienti = Inchidere(dto.Clienti),
                ClosingGlaClienti = InchidereGla(RolTertCont.Client),
                ClosingFurnizori = Inchidere(dto.Furnizori),
                ClosingGlaFurnizori = InchidereGla(RolTertCont.Furnizor),
                NeincluseClienti = dto.Neincluse.Where(n => n.Sectiune == "Customers").Sum(n => (n.Debit ?? 0m) - (n.Credit ?? 0m)),
                NeincluseFurnizori = dto.Neincluse.Where(n => n.Sectiune == "Suppliers").Sum(n => (n.Debit ?? 0m) - (n.Credit ?? 0m)),
                NetTotalEmise = dto.FacturiEmise.Sum(f => f.NetTotal),
                NetTotalPrimite = dto.FacturiPrimite.Sum(f => f.NetTotal),
                GrossTotalEmise = dto.FacturiEmise.Sum(f => f.GrossTotal),
                GrossTotalPrimite = dto.FacturiPrimite.Sum(f => f.GrossTotal),
                NumarClienti = dto.Clienti.Count,
                NumarFurnizori = dto.Furnizori.Count,
                NumarFacturiEmise = dto.FacturiEmise.Count,
                NumarFacturiPrimite = dto.FacturiPrimite.Count,
                NumarProduse = dto.Produse.Count,
            };
        }
    }
}
