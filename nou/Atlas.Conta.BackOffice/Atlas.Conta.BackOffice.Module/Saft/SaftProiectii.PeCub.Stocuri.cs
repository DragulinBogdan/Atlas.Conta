using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Saft;

public static partial class SaftProiectii {
    public const string RefuzCategorieLipsa = "SAFT_CATEGORIE_LIPSA";
    public const string RefuzCategorieNeacoperita = "SAFT_CATEGORIE_NEACOPERITA";
    public const string RefuzMiscareFaraPolitica = "SAFT_MISCARE_FARA_POLITICA";
    public const string RefuzMiscareValorica = "SAFT_MISCARE_VALORICA";
    public const string RefuzCodMiscare = "SAFT_COD_MISCARE_NECUNOSCUT";
    public const string RefuzTertLipsa = "SAFT_TERT_LIPSA";
    public const string RefuzSoldNegativ = "SAFT_SOLD_NEGATIV";
    public const string RefuzReziduValoric = "SAFT_REZIDU_VALORIC";
    public const string RefuzCheie = "SAFT_CHEIE_NEINJECTIVA";

    /// <summary>D406 S din cub (TR-D8 S3): PhysicalStock și MovementOfGoods pe postările pe lot.</summary>
    public static SaftDto SaftStocuriPeCub(IObjectSpace os, int an, int luna, DateOnly? dataCreare = null) {
        using var citire = Fiscale.DeschideCitirea(os, cereIzolare: true);
        var dto = new SaftDto {
            An = an, Luna = luna,
            DataStart = new DateOnly(an, luna, 1),
            DataEnd = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna)),
        };
        dto.Neaplicabil = MotivNeaplicabil(os, " S");
        if (dto.Neaplicabil == null)
            new StocuriPeCub(os, dto, dataCreare).Ruleaza();
        return dto;
    }

    sealed class StocuriPeCub(IObjectSpace os, SaftDto dto, DateOnly? dataCreare) {
        readonly record struct Pozitie(Guid Lot, Guid Cont, Guid Produs, Guid Gestiune);

        sealed class PostareLot {
            public Guid Id, TranzactieId, Lot, Cont, Produs, Gestiune;
            public Guid? DocumentId, LinieId;
            public N.FelTranzactie Fel;
            public N.Spatiu Spatiu;
            public DateOnly Data, Deschisa;
            public DateTime ScrisLa;
            public decimal Cantitate, Valoare;
            public Pozitie Pozitie => new(Lot, Cont, Produs, Gestiune);
        }

        sealed class Linie {
            public Guid TranzactieId;
            public Guid? DocumentId, LinieId;
            public N.FelTranzactie Fel;
            public DateOnly Data, Deschisa;
            public DateTime ScrisLa;
            public Pozitie Pozitie;
            public decimal Cantitate, Valoare;
            public List<SaftSursa> Postari = [];
            public RegulaMiscare Regula;
        }

        sealed record DocInfo(string Numar, Guid PredatorId, Guid PrimitorId, Guid? SursaId, bool Autogenerat);

        readonly DateOnly start = dto.DataStart, end = dto.DataEnd;
        readonly Dictionary<CodAvertismentSaft, List<(string Exemplu, decimal? Suma)>> avertismente = [];
        readonly CategoriiStoc categorii = new(os);
        readonly HashSet<Guid> conturiFaraCategorie = [];
        readonly HashSet<(Guid, TipStoc)> conturiNeacoperite = [];

        Dictionary<Guid, (string Simbol, string Denumire, string Functie, RolTertCont RolTert)> conturi;
        Dictionary<Guid, string> codPerDocument;
        Dictionary<Guid, DocInfo> documente;
        Dictionary<Guid, int> pozitieLinie;
        Dictionary<Guid, string> warehouse;
        Dictionary<Guid, InfoPartener> parteneri;
        string idSocietate, ownerId;

        void Avert(CodAvertismentSaft cod, string exemplu, decimal? suma = null) {
            if (!avertismente.TryGetValue(cod, out var lista)) lista = avertismente[cod] = [];
            lista.Add((exemplu, suma));
        }

        void Refuza(string cod, string mesaj, Guid? documentId = null, Guid? tranzactieId = null) =>
            dto.Refuzuri.Add(new SaftRefuz { Cod = cod, Mesaj = mesaj, DocumentId = documentId, TranzactieId = tranzactieId });

        string SimbolCont(Guid cont) => conturi.TryGetValue(cont, out var c) ? c.Simbol : cont.ToString();
        string Eticheta(Guid? document) => document is Guid d && documente.TryGetValue(d, out var doc)
            ? $"{codPerDocument.GetValueOrDefault(d)} {doc.Numar}" : JurnalDeschidere;

        TipStoc? Categorie(Guid cont) {
            var categorie = categorii.Rezolva(cont);
            dto.CategoriiStoc[SimbolCont(cont)] = categorie?.ToString() ?? "-";
            if (categorie == null) conturiFaraCategorie.Add(cont);
            else if (CategoriiStoc.Rol(categorie.Value) == RolCategorieStoc.Neacoperita)
                conturiNeacoperite.Add((cont, categorie.Value));
            return categorie;
        }

        bool Raportabila(Guid cont) => Categorie(cont) is TipStoc c && CategoriiStoc.Rol(c) == RolCategorieStoc.Raportabila;

        public void Ruleaza() {
            var soc = CitesteSocietate(os);
            VerificaSocietate(soc, (cod, exemplu) => Avert(cod, exemplu));
            idSocietate = SaftReguli.IdSocietate(soc?.CodFiscal, soc?.Tara);
            ownerId = SaftReguli.OwnerIdRaportor(soc?.CodFiscal, soc?.Tara);
            dto.Header = Antet(soc, dto.An, dto.Luna, dataCreare, HeaderCommentStocuri);

            conturi = CitesteConturi(os);
            var (conturiSaft, balanta) = ConturiSiSolduri(os, start, end, conturi, (cod, exemplu) => Avert(cod, exemplu));
            dto.Conturi = conturiSaft;

            var luna = Loturi.Postari(os).Where(p => p.Data >= start && p.Data <= end)
                .Select(p => new PostareLot {
                    Id = p.ID, Spatiu = p.Spatiu, TranzactieId = p.TranzactieId, DocumentId = p.Tranzactie.DocumentId, LinieId = p.LinieId,
                    Fel = p.Tranzactie.Fel, Data = p.Data, ScrisLa = p.Tranzactie.ScrisLa,
                    Lot = p.Unitate.Value, Cont = p.Cont, Produs = p.Produs.Value, Gestiune = p.Gestiune.Value,
                    Deschisa = p.UnitateDeschisa ?? p.Data, Cantitate = p.Cantitate,
                    Valoare = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare,
                }).ToList();
            // S2-D5: accesul complet se verifică înaintea proiecției, deci cumulul poate porni din snapshot.
            var initiale = Loturi.Cumulate(os, CitireCumul.Integrala, start.AddDays(-1)).ToList();

            Metadate(luna, initiale);
            var politici = Politici();
            var linii = Linii(luna.Where(p => p.Fel != N.FelTranzactie.Deschidere), politici);
            var pozitii = Pozitii(luna, initiale, linii);
            Miscari(linii);
            CategoriiRefuzate();
            Chei();

            var (produse, unitati) = ProduseSiUnitati(os,
                pozitii.Select(e => e.ProdusId).Concat(dto.MiscariStoc.SelectMany(m => m.Linii).Select(l => l.ProdusId))
                    .Distinct().ToList(), (cod, exemplu) => Avert(cod, exemplu));
            dto.Produse = produse;
            dto.Unitati = unitati;
            var produsPeId = produse.ToDictionary(p => p.ProdusId);
            foreach (var e in dto.StocFizic) {
                var p = produsPeId[e.ProdusId];
                e.ProductCode = p.ProductCode;
                e.UomPhysicalStock = p.UOMBase;
                e.StockAccountCommodityCode = p.ProductCommodityCode;
            }
            foreach (var l in dto.MiscariStoc.SelectMany(m => m.Linii)) {
                var p = produsPeId[l.ProdusId];
                l.ProductCode = p.ProductCode;
                l.UnitOfMeasure = p.UOMBase;
            }
            ChePozitii();
            dto.Taxe = TabelaTaxe(CoduriTva());
            dto.TipuriAnaliza = [];
            Rezumat(luna, linii, balanta);

            dto.Avertismente = avertismente.OrderBy(a => a.Key).Select(a => new SaftAvertisment {
                Cod = a.Key.ToString(), Mesaj = Mesaj(a.Key), Numar = a.Value.Count,
                Suma = a.Value.Any(x => x.Suma != null) ? a.Value.Sum(x => x.Suma ?? 0m) : null,
                Exemple = a.Value.Take(5).Select(x => x.Exemplu).ToList(),
            }).ToList();
        }

        Dictionary<string, (decimal Cota, string Denumire)> CoduriTva() {
            var tipuri = os.GetObjectsQuery<TipTva>().Select(t => new { t.ID, t.Denumire }).ToList()
                .ToDictionary(t => t.ID, t => t.Denumire);
            var coduri = new Dictionary<string, (decimal Cota, string Denumire)>(StringComparer.Ordinal);
            var mapari = new MapariFiscale(os);
            foreach (var fapt in Fiscale.Fapte(os).Where(f => f.Data >= start && f.Data <= end).ToList()) {
                var mapare = mapari.Pentru(fapt, SectiuneTvaSaft.Facturi);
                if (mapare?.TaxType == SaftReguli.TaxTypeTva)
                    coduri.TryAdd(mapare.TaxCode, (fapt.Cota, tipuri.GetValueOrDefault(fapt.TipTvaId)));
            }
            return coduri;
        }

        void Metadate(List<PostareLot> luna, List<SoldLot> initiale) {
            var ids = luna.Where(p => p.DocumentId != null).Select(p => p.DocumentId.Value).Distinct().ToList();
            codPerDocument = ApiProiectii.CoduriTip(os, ids);
            documente = os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID))
                .Select(d => new { d.ID, d.Numar, d.PredatorId, d.PrimitorId, d.DocumentSursaId, d.Autogenerat }).ToList()
                .ToDictionary(d => d.ID, d => new DocInfo(d.Numar, d.PredatorId, d.PrimitorId, d.DocumentSursaId, d.Autogenerat));
            var idsSursa = documente.Values.Where(d => d.Autogenerat && d.SursaId != null).Select(d => d.SursaId.Value)
                .Where(id => !documente.ContainsKey(id)).Distinct().ToList();
            foreach (var d in os.GetObjectsQuery<Document>().Where(d => idsSursa.Contains(d.ID))
                         .Select(d => new { d.ID, d.Numar, d.PredatorId, d.PrimitorId }).ToList())
                documente[d.ID] = new DocInfo(d.Numar, d.PredatorId, d.PrimitorId, null, false);

            var idsLinie = luna.Where(p => p.LinieId != null).Select(p => p.LinieId.Value).Distinct().ToList();
            pozitieLinie = os.GetObjectsQuery<DocumentDetaliu>().Where(d => idsLinie.Contains(d.ID))
                .Select(d => new { d.ID, d.Pozitie }).ToList().ToDictionary(d => d.ID, d => d.Pozitie);

            var idsGestiune = luna.Select(p => p.Gestiune).Concat(initiale.Select(s => s.GestiuneId)).Distinct().ToList();
            warehouse = os.GetObjectsQuery<Repartitor>().Where(r => idsGestiune.Contains(r.ID))
                .Select(r => new { r.ID, r.Cod }).ToList().ToDictionary(r => r.ID, r => r.Cod?.Trim());

            var idsRep = documente.Values.SelectMany(d => new[] { d.PredatorId, d.PrimitorId }).Distinct().ToList();
            var raporteazaCnp = CitesteSocietate(os)?.RaporteazaCnp ?? false;
            parteneri = os.GetObjectsQuery<Partener>().Where(p => idsRep.Contains(p.ID))
                .Select(p => new { p.ID, p.Cod, p.Denumire, p.CodFiscal, p.TipPersoana, p.Tara, p.InregistratTva, p.TvaLaIncasare })
                .ToList()
                .ToDictionary(p => p.ID, p => new InfoPartener {
                    Id = p.ID, Cod = p.Cod, Denumire = p.Denumire, CodFiscal = p.CodFiscal, TipPersoana = p.TipPersoana,
                    Tara = p.Tara, InregistratTva = p.InregistratTva, TvaLaIncasare = p.TvaLaIncasare,
                    Id406 = SaftReguli.IdPartener(p.TipPersoana, p.Tara, p.InregistratTva, p.CodFiscal, p.Cod, p.ID, raporteazaCnp).Id,
                });
        }

        List<RegulaMiscare> Politici() {
            var tipuri = os.GetObjectsQuery<TipDocument>().Select(t => new { t.ID, t.Cod }).ToList()
                .ToDictionary(t => t.ID, t => t.Cod);
            return os.GetObjectsQuery<PoliticaMiscareSaft>()
                .Select(p => new { p.TipDocumentId, p.TipStoc, p.Semn, p.CodMiscare, p.RolTert, p.Motiv }).ToList()
                .Select(p => new RegulaMiscare {
                    TipDocumentId = p.TipDocumentId, TipStoc = p.TipStoc, Semn = p.Semn,
                    Cod = string.IsNullOrWhiteSpace(p.CodMiscare) ? null : p.CodMiscare.Trim(),
                    Rol = p.RolTert, Motiv = p.Motiv, CodTipDocument = tipuri.GetValueOrDefault(p.TipDocumentId),
                }).ToList();
        }

        List<Linie> Linii(IEnumerable<PostareLot> miscari, List<RegulaMiscare> politici) {
            var linii = new List<Linie>();
            foreach (var g in miscari.GroupBy(p => (p.TranzactieId, p.LinieId, p.Pozitie))) {
                var prima = g.First();
                var linie = new Linie {
                    TranzactieId = prima.TranzactieId, DocumentId = prima.DocumentId, LinieId = prima.LinieId,
                    Fel = prima.Fel, Data = prima.Data, ScrisLa = prima.ScrisLa, Deschisa = g.Min(p => p.Deschisa),
                    Pozitie = prima.Pozitie, Cantitate = g.Sum(p => p.Cantitate), Valoare = g.Sum(p => p.Valoare),
                    Postari = g.Select(p => new SaftSursa { Spatiu = p.Spatiu, Id = p.Id }).OrderBy(x => x.Id).ToList(),
                };
                if (linie.Cantitate == 0m && linie.Valoare == 0m) continue;
                linii.Add(linie);
                var eticheta = Eticheta(linie.DocumentId);
                if (linie.Cantitate == 0m) {
                    Refuza(RefuzMiscareValorica, $"{eticheta}: linia pe lotul {linie.Pozitie.Lot}, contul "
                        + $"{SimbolCont(linie.Pozitie.Cont)} are cantitatea 0 și valoarea {linie.Valoare} (SAFT-r2).",
                        linie.DocumentId, linie.TranzactieId);
                    continue;
                }
                if (Categorie(linie.Pozitie.Cont) is not TipStoc categorie
                        || CategoriiStoc.Rol(categorie) == RolCategorieStoc.Neacoperita)
                    continue;
                var semn = Math.Sign(linie.Cantitate) * (linie.Fel == N.FelTranzactie.Storno ? -1 : 1);
                var tip = linie.DocumentId is Guid d ? codPerDocument.GetValueOrDefault(d) : null;
                var candidati = politici.Where(p => p.CodTipDocument == tip && p.TipStoc == categorie).ToList();
                var regula = candidati.FirstOrDefault(p => p.Semn == semn) ?? candidati.FirstOrDefault(p => p.Semn == null);
                if (regula == null) {
                    Refuza(RefuzMiscareFaraPolitica, $"{eticheta}: nicio politică de mișcare SAF-T pentru "
                        + $"{tip ?? "(fără tip)"}/{categorie}/{semn:+0;-0}.", linie.DocumentId, linie.TranzactieId);
                    continue;
                }
                if (regula.Cod != null && !SaftReguli.EsteCodMiscare(regula.Cod)) {
                    Refuza(RefuzCodMiscare, $"{eticheta}: politica {tip}/{categorie} are codul „{regula.Cod}”, "
                        + "care nu e în nomenclatorul D406.", linie.DocumentId, linie.TranzactieId);
                    continue;
                }
                linie.Regula = regula;
            }
            return linii;
        }

        List<SaftStocFizic> Pozitii(List<PostareLot> luna, List<SoldLot> initiale, List<Linie> linii) {
            var deschidere = new Dictionary<Pozitie, (decimal Q, decimal V)>();
            void Aduna(Dictionary<Pozitie, (decimal Q, decimal V)> tinta, Pozitie p, decimal q, decimal v) {
                var x = tinta.GetValueOrDefault(p);
                tinta[p] = (x.Q + q, x.V + v);
            }
            foreach (var s in initiale)
                Aduna(deschidere, new Pozitie(s.LotId, s.ContId, s.ProdusId, s.GestiuneId), s.Cantitate, s.Valoare);
            foreach (var p in luna.Where(p => p.Fel == N.FelTranzactie.Deschidere))
                Aduna(deschidere, p.Pozitie, p.Cantitate, p.Valoare);
            var rulaj = new Dictionary<Pozitie, (decimal Q, decimal V)>();
            foreach (var l in linii) Aduna(rulaj, l.Pozitie, l.Cantitate, l.Valoare);
            var intrari = new Dictionary<Pozitie, (decimal Q, decimal V)>();
            foreach (var l in linii.Where(l => l.Cantitate > 0m)) Aduna(intrari, l.Pozitie, l.Cantitate, l.Valoare);

            var rezultat = new List<SaftStocFizic>();
            foreach (var p in deschidere.Keys.Concat(rulaj.Keys).Distinct()) {
                var (qi, vi) = deschidere.GetValueOrDefault(p);
                var (qr, vr) = rulaj.GetValueOrDefault(p);
                if (qi == 0m && vi == 0m && !rulaj.ContainsKey(p) || !Raportabila(p.Cont))
                    continue;
                var (qf, vf) = (qi + qr, vi + vr);
                var eticheta = $"lotul {p.Lot} pe {SimbolCont(p.Cont)} în gestiunea {warehouse.GetValueOrDefault(p.Gestiune) ?? p.Gestiune.ToString()}";
                foreach (var (q, v, capat) in new[] { (qi, vi, "inițial"), (qf, vf, "final") }) {
                    if (q < 0m || v < 0m)
                        Refuza(RefuzSoldNegativ, $"{eticheta}: sold {capat} {q}/{v}.");
                    else if (q == 0m && v != 0m)
                        Refuza(RefuzReziduValoric, $"{eticheta}: sold {capat} cu cantitate 0 și valoare {v}.");
                }
                var intrare = intrari.GetValueOrDefault(p);
                var pret = qf != 0m ? vf / qf : qi != 0m ? vi / qi : intrare.Q != 0m ? intrare.V / intrare.Q : 0m;
                rezultat.Add(new SaftStocFizic {
                    RepartitorId = p.Gestiune, LotId = p.Lot, ContId = p.Cont, ProdusId = p.Produs,
                    WarehouseId = warehouse.GetValueOrDefault(p.Gestiune),
                    StockAccountNo = p.Lot.ToString("N"),
                    ProductType = SaftReguli.ProductTypeDinCont(SimbolCont(p.Cont)),
                    OwnerId = ownerId, UomConversionFactor = 1m,
                    UnitPrice = Math.Round(pret, 2, MidpointRounding.AwayFromZero),
                    OpeningQuantity = qi, OpeningValue = vi, ClosingQuantity = qf, ClosingValue = vf,
                    StockCharacteristic = SaftReguli.StockCharacteristic.Cheie,
                    StockCharacteristicValue = SaftReguli.StockCharacteristic.Valoare,
                });
            }
            dto.StocFizic = rezultat.OrderBy(e => e.WarehouseId ?? "", StringComparer.Ordinal)
                .ThenBy(e => e.ProdusId).ThenBy(e => e.LotId).ThenBy(e => e.ContId).ToList();
            return dto.StocFizic;
        }

        void Miscari(List<Linie> linii) {
            var excluse = new Dictionary<RegulaMiscare, SaftExclus>();
            var coduri = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var l in linii.Where(l => l.Regula is { Cod: null })) {
                if (!excluse.TryGetValue(l.Regula, out var x))
                    x = excluse[l.Regula] = new SaftExclus {
                        TipDocument = l.Regula.CodTipDocument, TipStoc = l.Regula.TipStoc.ToString(),
                        Semn = l.Regula.Semn, Motiv = l.Regula.Motiv,
                    };
                x.Numar++;
                x.Cantitate += l.Cantitate;
                x.Valoare += l.Valoare;
            }
            dto.Excluse = excluse.Values.OrderBy(x => x.TipDocument ?? "", StringComparer.Ordinal)
                .ThenBy(x => x.TipStoc, StringComparer.Ordinal).ThenBy(x => x.Semn ?? 0).ToList();

            foreach (var ev in linii.Where(l => l.Regula is { Cod: not null }).GroupBy(l => l.TranzactieId)
                         .OrderBy(g => g.First().Data).ThenBy(g => g.First().ScrisLa).ThenBy(g => g.Key)) {
                var prima = ev.First();
                var peCod = ev.GroupBy(l => l.Regula.Cod).OrderBy(g => g.Key.Length).ThenBy(g => g.Key, StringComparer.Ordinal).ToList();
                var docId = prima.DocumentId;
                var doc = docId is Guid d ? documente.GetValueOrDefault(d) : null;
                var tip = docId is Guid t ? codPerDocument.GetValueOrDefault(t) : null;
                foreach (var g in peCod) {
                    var roluri = g.Select(l => l.Regula.Rol).Distinct().ToList();
                    if (roluri.Count > 1) {
                        Refuza(RefuzProvenienta, $"{Eticheta(docId)}: mișcarea {g.Key} cere roluri de terț diferite "
                            + $"({string.Join(", ", roluri)}).", docId, ev.Key);
                        continue;
                    }
                    var (customer, supplier) = Terti(roluri[0], doc, docId, ev.Key);
                    var miscare = new SaftMiscareStoc {
                        DocumentId = docId ?? Guid.Empty, TranzactieId = ev.Key,
                        Storno = prima.Fel == N.FelTranzactie.Storno,
                        MovementReference = Referinta(tip, doc?.Numar, prima.Fel, peCod.Count > 1 ? g.Key : null),
                        MovementDate = prima.Data,
                        MovementPostingDate = DataSistem(prima.ScrisLa),
                        MovementType = g.Key, DocumentType = tip, DocumentNumber = doc?.Numar,
                    };
                    var pozitie = 0;
                    foreach (var l in g.OrderBy(l => l.LinieId is Guid li ? pozitieLinie.GetValueOrDefault(li) : int.MaxValue)
                                 .ThenBy(l => l.Deschisa).ThenBy(l => l.Pozitie.Lot)
                                 .ThenBy(l => warehouse.GetValueOrDefault(l.Pozitie.Gestiune) ?? "", StringComparer.Ordinal)) {
                        var gestiune = warehouse.GetValueOrDefault(l.Pozitie.Gestiune);
                        miscare.Linii.Add(new SaftLinieMiscareStoc {
                            DetaliuId = l.LinieId, LineNumber = ++pozitie,
                            AccountId = SaftReguli.SimbolSaft(SimbolCont(l.Pozitie.Cont)),
                            CustomerId = customer, SupplierId = supplier,
                            ProdusId = l.Pozitie.Produs, LotId = l.Pozitie.Lot, RepartitorId = l.Pozitie.Gestiune,
                            ContId = l.Pozitie.Cont, Postari = l.Postari,
                            ShipToWarehouseId = l.Cantitate > 0m ? gestiune : null,
                            ShipFromWarehouseId = l.Cantitate < 0m ? gestiune : null,
                            StockAccountNo = l.Pozitie.Lot.ToString("N"),
                            Quantity = l.Cantitate, UomConversionFactor = 1m, BookValue = l.Valoare,
                            MovementSubType = g.Key,
                        });
                    }
                    coduri.TryAdd(g.Key, SaftReguli.CoduriMiscare[g.Key]);
                    dto.MiscariStoc.Add(miscare);
                }
            }

            var inGl = dto.MiscariStoc.Where(m => m.TranzactieId != null).Select(m => m.TranzactieId.Value).Distinct().ToList();
            var tranzactiiGl = Contabil.Postari(os).Where(p => inGl.Contains(p.TranzactieId))
                .Select(p => p.TranzactieId).Distinct().ToList().ToHashSet();
            foreach (var m in dto.MiscariStoc)
                m.TransactionId = m.TranzactieId is Guid tx && tranzactiiGl.Contains(tx) ? tx.ToString() : null;

            dto.TipuriMiscare = coduri.OrderBy(c => c.Key.Length).ThenBy(c => c.Key, StringComparer.Ordinal)
                .Select(c => new SaftTipMiscare { Cod = c.Key, Descriere = c.Value }).ToList();
            var liniiEmise = dto.MiscariStoc.SelectMany(m => m.Linii).ToList();
            dto.NumberOfMovementLines = liniiEmise.Count;
            dto.TotalQuantityReceived = liniiEmise.Where(l => l.Quantity > 0m).Sum(l => l.Quantity);
            dto.TotalQuantityIssued = -liniiEmise.Where(l => l.Quantity < 0m).Sum(l => l.Quantity);
        }

        (string Customer, string Supplier) Terti(RolTertSaft rol, DocInfo doc, Guid? docId, Guid tranzactie) {
            if (rol == RolTertSaft.Niciunul)
                return SaftReguli.TertiLinieStoc(rol, null, idSocietate);
            var partener = doc == null ? null : PartenerulDocumentului(doc.PredatorId, doc.PrimitorId, parteneri);
            if (partener == null && doc is { Autogenerat: true, SursaId: Guid sursa } && documente.TryGetValue(sursa, out var s))
                partener = PartenerulDocumentului(s.PredatorId, s.PrimitorId, parteneri);
            if (partener is not Guid p) {
                Refuza(RefuzTertLipsa, $"{Eticheta(docId)}: politica cere rolul {rol}, dar documentul nu are partener.",
                    docId, tranzactie);
                return SaftReguli.TertiLinieStoc(RolTertSaft.Niciunul, null, idSocietate);
            }
            return SaftReguli.TertiLinieStoc(rol, parteneri[p].Id406, idSocietate);
        }

        static string Referinta(string tip, string numar, N.FelTranzactie fel, string cod) =>
            string.IsNullOrWhiteSpace(tip) || string.IsNullOrWhiteSpace(numar) ? null
                : $"{tip.Trim()}-{numar.Trim()}"
                    + (fel == N.FelTranzactie.Transfer ? "/T" : fel == N.FelTranzactie.Storno ? "/S" : "")
                    + (cod == null ? "" : "/" + cod);

        // S3-D5: forma lizibilă când e unică și încape; altfel tranzacția + codul, disjunctă prin lipsa lui „-”.
        void Chei() {
            var numereComune = dto.MiscariStoc.GroupBy(m => (m.DocumentType, m.DocumentNumber))
                .Where(g => g.Select(m => m.DocumentId).Distinct().Count() > 1).SelectMany(g => g).ToHashSet();
            var lizibile = dto.MiscariStoc.Where(m => !numereComune.Contains(m)
                    && m.MovementReference is { Length: <= SaftReguli.LungimeMovementReference })
                .GroupBy(m => m.MovementReference, StringComparer.Ordinal).Where(g => g.Count() == 1)
                .Select(g => g.Single()).ToHashSet();
            foreach (var m in dto.MiscariStoc.Where(m => !lizibile.Contains(m)))
                m.MovementReference = $"{m.TranzactieId:N}{m.MovementType}";

            var gestiuni = dto.StocFizic.Select(e => e.RepartitorId)
                .Concat(dto.MiscariStoc.SelectMany(m => m.Linii).Select(l => l.RepartitorId)).Distinct();
            foreach (var g in gestiuni.GroupBy(id => warehouse.GetValueOrDefault(id) ?? "", StringComparer.Ordinal)) {
                if (g.Key.Length is 0 or > SaftReguli.LungimeMovementReference)
                    Refuza(RefuzCheie, $"Gestiunea {string.Join(", ", g)} are codul „{g.Key}”: WarehouseID cere 1–35 caractere.");
                else if (g.Count() > 1)
                    Refuza(RefuzCheie, $"WarehouseID „{g.Key}” aparține la {g.Count()} gestiuni.");
            }
        }

        void ChePozitii() {
            foreach (var g in dto.StocFizic.GroupBy(e => (e.WarehouseId, e.ProductCode, e.StockAccountNo, e.ProductType))
                         .Where(g => g.Count() > 1))
                Refuza(RefuzCheie, $"Poziția ({g.Key.WarehouseId}, {g.Key.ProductCode}, {g.Key.StockAccountNo}, "
                    + $"{g.Key.ProductType}) acoperă {g.Count()} conturi ale aceluiași lot.");
        }

        void CategoriiRefuzate() {
            foreach (var cont in conturiFaraCategorie.OrderBy(c => SimbolCont(c), StringComparer.Ordinal))
                Refuza(RefuzCategorieLipsa, $"Contul {SimbolCont(cont)} poartă loturi, dar nici el, nici părinții lui "
                    + "n-au categorie de stoc (Plan de conturi → Categorie stoc).");
            foreach (var (cont, categorie) in conturiNeacoperite.OrderBy(c => SimbolCont(c.Item1), StringComparer.Ordinal))
                Refuza(RefuzCategorieNeacoperita, $"Contul {SimbolCont(cont)} are categoria {categorie}, "
                    + "neacoperită de declarația S.");
        }

        void Rezumat(List<PostareLot> luna, List<Linie> linii, List<BalantaRand> balanta) {
            var emise = dto.MiscariStoc.SelectMany(m => m.Linii).ToList();
            var emisePePozitie = emise.GroupBy(l => new Pozitie(l.LotId, l.ContId, l.ProdusId, l.RepartitorId))
                .ToDictionary(g => g.Key, g => (Q: g.Sum(l => l.Quantity), V: g.Sum(l => l.BookValue)));
            var diferite = dto.StocFizic.Count(e => {
                var x = emisePePozitie.GetValueOrDefault(new Pozitie(e.LotId, e.ContId, e.ProdusId, e.RepartitorId));
                return e.OpeningQuantity + x.Q != e.ClosingQuantity || e.OpeningValue + x.V != e.ClosingValue;
            });
            var raportabile = linii.Where(l => Raportabila(l.Pozitie.Cont)).ToList();
            // S3-RV1: și contul raportabil cu sold sau rulaj contabil fără nicio poziție fizică.
            var conturiGl = balanta.Where(b => b.InitialDebit != 0m || b.InitialCredit != 0m
                    || b.RulajDebit != 0m || b.RulajCredit != 0m)
                .Where(b => categorii.Rezolva(b.ContId) is TipStoc c && CategoriiStoc.Rol(c) == RolCategorieStoc.Raportabila)
                .Select(b => b.ContId);
            var perCont = dto.StocFizic.Select(e => e.ContId).Concat(conturiGl).Distinct().Select(cont => {
                var inchidereGl = balanta.Where(x => x.ContId == cont)
                    .Sum(x => x.InitialDebit - x.InitialCredit + x.RulajDebit - x.RulajCredit);
                var stoc = dto.StocFizic.Where(e => e.ContId == cont).Sum(e => e.ClosingValue);
                return new SaftDiferentaCont {
                    Cont = SaftReguli.ProductTypeDinCont(SimbolCont(cont)), ContId = cont,
                    ClosingStocFizic = stoc, ClosingBalanta = inchidereGl, Diferenta = stoc - inchidereGl,
                };
            }).OrderBy(c => c.Cont, StringComparer.Ordinal).ToList();
            Componente(perCont);

            var coduriProdus = dto.Produse.Select(p => p.ProductCode).ToHashSet(StringComparer.Ordinal);
            var referite = dto.StocFizic.Select(e => e.ProductCode).Concat(emise.Select(l => l.ProductCode))
                .Distinct(StringComparer.Ordinal).ToList();
            var coduriDeclarate = dto.TipuriMiscare.Select(t => t.Cod).ToHashSet(StringComparer.Ordinal);
            var coduriReferite = dto.MiscariStoc.Select(m => m.MovementType).Concat(emise.Select(l => l.MovementSubType))
                .Distinct(StringComparer.Ordinal).ToList();
            var miscari = luna.Where(p => p.Fel != N.FelTranzactie.Deschidere).ToList();
            dto.Rezumat = new SaftRezumat {
                NumarMiscari = dto.MiscariStoc.Count, NumarLiniiMiscare = emise.Count, NumarStocFizic = dto.StocFizic.Count,
                NumarTipuriMiscare = dto.TipuriMiscare.Count, RanduriRegistruStoc = miscari.Count, NumarProduse = dto.Produse.Count,
                StocIntrari = dto.StocFizic.Count, StocIntrariDiferite = diferite, StocFizicVsMiscariDiferite = diferite,
                StocOpeningCantitate = dto.StocFizic.Sum(e => e.OpeningQuantity),
                StocOpeningValoare = dto.StocFizic.Sum(e => e.OpeningValue),
                StocMiscariCantitate = raportabile.Sum(l => l.Cantitate), StocMiscariValoare = raportabile.Sum(l => l.Valoare),
                StocClosingCantitate = dto.StocFizic.Sum(e => e.ClosingQuantity),
                StocClosingValoare = dto.StocFizic.Sum(e => e.ClosingValue),
                StocEmiseCantitate = emise.Sum(l => l.Quantity), StocEmiseValoare = emise.Sum(l => l.BookValue),
                StocFizicBate = diferite == 0,
                MiscariCantitate = emise.Sum(l => l.Quantity), MiscariValoare = emise.Sum(l => l.BookValue),
                ExcluseCantitate = dto.Excluse.Sum(x => x.Cantitate), ExcluseValoare = dto.Excluse.Sum(x => x.Valoare),
                RegistruStocCantitate = miscari.Sum(p => p.Cantitate), RegistruStocValoare = miscari.Sum(p => p.Valoare),
                StocPerCont = perCont,
                ClosingStocFizic = perCont.Sum(c => c.ClosingStocFizic), ClosingBalantaStoc = perCont.Sum(c => c.ClosingBalanta),
                ConturiStocVerificate = perCont.Count, ConturiStocDiferite = perCont.Count(c => c.Diferenta != 0m),
                ProduseReferite = referite.Count, ProduseLipsa = referite.Count(c => !coduriProdus.Contains(c)),
                CoduriMiscareFolosite = coduriReferite.Count, CoduriMiscareLipsa = coduriReferite.Count(c => !coduriDeclarate.Contains(c)),
                IdentitatiTertInvalide = emise.SelectMany(l => new[] { l.CustomerId, l.SupplierId })
                    .Concat(dto.StocFizic.Select(e => e.OwnerId)).Count(id => !IdentitateTertValida(id)),
                ReferinteDuplicate = ReferinteDuplicate(dto.MiscariStoc),
            };
            dto.Rezumat.RegistruStocBate = dto.Rezumat.MiscariCantitate + dto.Rezumat.ExcluseCantitate == dto.Rezumat.RegistruStocCantitate
                && dto.Rezumat.MiscariValoare + dto.Rezumat.ExcluseValoare == dto.Rezumat.RegistruStocValoare;
            dto.Rezumat.ReferinteBat = dto.Rezumat.ReferinteDuplicate == 0;
        }

        // S3-D7b: diferența pe cont se sparge pe tipul documentului, din postările pe lot și din GL.
        void Componente(List<SaftDiferentaCont> perCont) {
            if (perCont.Count == 0) return;
            var ids = perCont.Select(c => c.ContId).ToList();
            var stoc = Loturi.Postari(os).Where(p => p.Data <= end && ids.Contains(p.Cont))
                .GroupBy(p => new { p.Cont, p.Tranzactie.DocumentId })
                .Select(g => new { g.Key.Cont, g.Key.DocumentId,
                    Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) }).ToList();
            var gl = Contabil.Postari(os).Where(p => p.Data <= end && ids.Contains(p.Cont))
                .GroupBy(p => new { p.Cont, p.DocumentId })
                .Select(g => new { g.Key.Cont, g.Key.DocumentId,
                    Valoare = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) }).ToList();
            var coduri = ApiProiectii.CoduriTip(os, stoc.Select(x => x.DocumentId).Concat(gl.Select(x => x.DocumentId))
                .OfType<Guid>().Distinct().ToList());
            string Tip(Guid? d) => d is Guid id ? coduri.GetValueOrDefault(id) ?? "(tip necunoscut)" : ComponentaDeschidere;
            var acumulat = new Dictionary<(Guid Cont, string Tip), (decimal Stoc, decimal Gl)>();
            foreach (var x in stoc) {
                var k = (x.Cont, Tip(x.DocumentId));
                var v = acumulat.GetValueOrDefault(k);
                acumulat[k] = (v.Stoc + x.Valoare, v.Gl);
            }
            foreach (var x in gl) {
                var k = (x.Cont, Tip(x.DocumentId));
                var v = acumulat.GetValueOrDefault(k);
                acumulat[k] = (v.Stoc, v.Gl + x.Valoare);
            }
            foreach (var c in perCont)
                c.Componente = acumulat.Where(a => a.Key.Cont == c.ContId && (a.Value.Stoc != 0m || a.Value.Gl != 0m))
                    .Select(a => new SaftComponentaCont {
                        TipDocument = a.Key.Tip, StocFizic = a.Value.Stoc, Balanta = a.Value.Gl,
                        Diferenta = a.Value.Stoc - a.Value.Gl,
                    })
                    .OrderByDescending(x => Math.Abs(x.Diferenta)).ThenBy(x => x.TipDocument, StringComparer.Ordinal).ToList();
        }
    }
}
