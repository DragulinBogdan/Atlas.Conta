using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Saft;

public static partial class SaftProiectii {
    sealed partial class ExportPeCub {
        enum FelExtern { Virament, Partener, Angajat, Altul }

        sealed record DocPlata(Guid Id, TipInstrumentPlata Instrument, N.Latura? Contrapartida, Guid? Extern, FelExtern Fel);

        Dictionary<Guid, DocPlata> PregatestePlati(List<PostareJurnal> jurnal) {
            var ids = jurnal.Where(p => p.DocumentId != null && p.Fel is N.FelTranzactie.Operare or N.FelTranzactie.Storno)
                .Select(p => p.DocumentId.Value).Distinct().ToList();
            var docs = os.GetObjectsQuery<DocumentTrezorerie>().Where(d => ids.Contains(d.ID))
                .Select(d => new { d.ID, d.TipInstrument, d.PredatorId, d.PrimitorId, d.ClrType }).ToList();
            if (docs.Count == 0)
                return [];
            var laturi = docs.SelectMany(d => new[] { d.PredatorId, d.PrimitorId }).Distinct().ToList();
            var proprii = os.GetObjectsQuery<ContPropriu>().Where(c => laturi.Contains(c.ID)).Select(c => c.ID).ToList().ToHashSet();
            var angajati = os.GetObjectsQuery<Angajat>().Where(c => laturi.Contains(c.ID)).Select(c => c.ID).ToList().ToHashSet();
            var externi = os.GetObjectsQuery<Partener>().Where(c => laturi.Contains(c.ID)).Select(c => c.ID).ToList().ToHashSet();
            var clase = docs.Select(d => d.ClrType).Distinct().ToList();
            var laturaPropriu = os.GetObjectsQuery<TipDocument>().Where(t => clase.Contains(t.ClrType))
                .Select(t => new { t.ClrType, t.LaturaContPropriu }).ToList()
                .GroupBy(t => t.ClrType, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.Select(t => t.LaturaContPropriu).Distinct().ToList(), StringComparer.Ordinal);
            return docs.ToDictionary(d => d.ID, d => {
                var propriu = laturaPropriu.TryGetValue(d.ClrType, out var l) && l.Count == 1 ? l[0] : null;
                Guid? extern_ = propriu switch {
                    LaturaDocument.Predator => d.PrimitorId,
                    LaturaDocument.Primitor => d.PredatorId,
                    _ => null,
                };
                var fel = proprii.Contains(d.PredatorId) && proprii.Contains(d.PrimitorId) ? FelExtern.Virament
                    : extern_ is Guid e && externi.Contains(e) ? FelExtern.Partener
                    : extern_ is Guid a && angajati.Contains(a) ? FelExtern.Angajat
                    : FelExtern.Altul;
                N.Latura? contrapartida = propriu switch {
                    LaturaDocument.Predator => N.Latura.Debit,
                    LaturaDocument.Primitor => N.Latura.Credit,
                    _ => null,
                };
                return new DocPlata(d.ID, d.TipInstrument, contrapartida, extern_, fel);
            });
        }

        void Plati(List<PostareJurnal> jurnal, Dictionary<Guid, DocPlata> plati) {
            var evenimente = jurnal
                .Where(p => p.DocumentId is Guid d && plati.TryGetValue(d, out var info) && info.Fel != FelExtern.Virament
                    && p.Fel is N.FelTranzactie.Operare or N.FelTranzactie.Storno)
                .Select(p => (p.TranzactieId, Document: p.DocumentId.Value, p.Fel, p.DataTranzactie)).Distinct()
                .OrderBy(e => e.DataTranzactie)
                .ThenBy(e => documente[e.Document].Numar ?? "", StringComparer.Ordinal)
                .ThenBy(e => e.TranzactieId).ToList();
            if (evenimente.Count == 0)
                return;
            foreach (var e in evenimente.Where(e => plati[e.Document].Contrapartida == null))
                Refuza(RefuzSursa, $"{Eticheta(e.Document)}: tipul documentului nu declară latura contului propriu.",
                    e.Document, e.TranzactieId);
            var contra = plati.Values.Where(p => p.Fel != FelExtern.Virament && p.Contrapartida != null)
                .Where(p => evenimente.Any(e => e.Document == p.Id))
                .ToDictionary(p => p.Id, p => p.Contrapartida.Value);
            var alocari = Cub.Citiri.Plati.Alocari(os, contra);
            var idsEvenimente = evenimente.Select(e => e.TranzactieId).ToList();
            var postari = Cub.Citiri.Plati.Postari(os).Where(p => idsEvenimente.Contains(p.TranzactieId)).ToList()
                .GroupBy(p => p.TranzactieId).ToDictionary(g => g.Key, g => g.ToList());
            var idsTinte = alocari.Values.SelectMany(a => a.Linii).Select(l => l.DocumentTinta).OfType<Guid>().Distinct().ToList();
            var tinte = os.GetObjectsQuery<Document>().Where(d => idsTinte.Contains(d.ID))
                .Select(d => new { d.ID, d.Numar, d.Data }).ToList().ToDictionary(d => d.ID);

            foreach (var e in evenimente.Where(e => contra.ContainsKey(e.Document))) {
                var info = plati[e.Document];
                var a = alocari[e.Document];
                var storno = e.Fel == N.FelTranzactie.Storno;
                var semn = storno ? -1m : 1m;
                var eticheta = Eticheta(e.Document);
                var proprii = postari.GetValueOrDefault(e.TranzactieId, []).Where(p => p.Latura == info.Contrapartida).ToList();
                if (info.Fel == FelExtern.Altul) {
                    NeinclusPlata(CauzaNeincludere.DocumentFaraPartener, e.Document, info.Extern, proprii);
                    continue;
                }
                if (a.Erori.Count > 0) {
                    Refuza(RefuzProvenienta, $"{eticheta}: alocarea plății nu se derivă din cub ({string.Join("; ", a.Erori)}).",
                        e.Document, e.TranzactieId);
                    continue;
                }
                if (a.Operare.Any(p => p.Valuta != null) || fapte.Keys.Any(k => k.Tranzactie == e.TranzactieId)) {
                    Refuza(RefuzSursa, $"{eticheta}: plata are valută sau fapte fiscale, pe care Payments nu le acoperă.",
                        e.Document, e.TranzactieId);
                    continue;
                }
                if (a.Operare.Select(p => p.Partener).OfType<Guid>().Distinct().Count() > 1) {
                    Refuza(RefuzProvenienta, $"{eticheta}: plata are mai mulți parteneri pe contrapartidă.", e.Document, e.TranzactieId);
                    continue;
                }
                if (info.Fel == FelExtern.Partener && a.Linii.All(l => Rol(l.Cont) == RolTertCont.Niciunul)) {
                    Avert(CodAvertismentSaft.PlataFaraContTert,
                        $"{eticheta} n-are pe contrapartidă niciun cont cu rol de terț (`Cont.RolTert`).",
                        proprii.Sum(p => p.Valoare));
                    NeinclusPlata(CauzaNeincludere.ContFaraRol, e.Document, info.Extern, proprii);
                    continue;
                }
                var conservata = proprii.GroupBy(p => (p.Cont, p.Latura))
                    .Select(g => (g.Key, Suma: g.Sum(p => p.Valoare)))
                    .Concat(a.Linii.GroupBy(l => (l.Cont, l.Latura)).Select(g => (g.Key, Suma: -semn * g.Sum(l => l.Suma))))
                    .GroupBy(x => x.Key).All(g => g.Sum(x => x.Suma) == 0m);
                if (!conservata) {
                    Refuza(RefuzProvenienta, $"{eticheta}: suma evenimentului nu este suma liniilor de plată pe cont și latură.",
                        e.Document, e.TranzactieId);
                    continue;
                }
                if (info.Fel == FelExtern.Angajat)
                    Avert(CodAvertismentSaft.PlataCatreAngajat,
                        $"{eticheta} are ca partener un angajat — `CustomerID`/`SupplierID` ies cu codul societății.");

                var doc = documente[e.Document];
                var (metoda, mecanism) = SaftReguli.MetodaPlata(info.Instrument);
                var cod = CodTip(e.Document);
                var descriere = $"{tipuriDocument.GetValueOrDefault(cod) ?? cod} {doc.Numar}".Trim() + (storno ? " (storno)" : "");
                var plata = new SaftPlata {
                    DocumentId = e.Document, Storno = storno, DocumentTip = cod,
                    PaymentRefNo = doc.Numar, TransactionID = e.TranzactieId.ToString(),
                    TransactionDate = e.DataTranzactie,
                    PaymentMethod = metoda, PaymentMechanism = mecanism, Description = descriere,
                };
                var pozitie = 0;
                foreach (var l in a.Linii
                             .OrderBy(l => l.Tinta)
                             .ThenBy(l => l.DocumentTinta is Guid t ? tinte[t].Data : DateOnly.MinValue)
                             .ThenBy(l => l.DocumentTinta is Guid t ? tinte[t].Numar ?? "" : "", StringComparer.Ordinal)
                             .ThenBy(l => l.DocumentTinta)
                             .ThenBy(l => Simbol(l.Cont), StringComparer.Ordinal)
                             .ThenBy(l => l.Unitate)) {
                    if (l.Tinta == TintaPlata.PartidaInitiala)
                        Avert(CodAvertismentSaft.PlataPePartidaInitiala, $"{eticheta}: {l.Suma * semn} pe o partidă inițială.");
                    var (customer, supplier) = info.Fel == FelExtern.Angajat
                        ? (idSocietate, idSocietate)
                        : IdentitatiPlata(l.Cont, l.Partener ?? info.Extern, eticheta);
                    var chei = l.Contributii.Select(p => (p.CentruCost, p.Proiect, p.UnitateOrganizatorica,
                        p.SursaFinantare, p.CodFunctional, p.CodEconomic)).Distinct().ToList();
                    if (chei.Count > 1)
                        Avert(CodAvertismentSaft.PlataAnalizaMixta, $"{eticheta}: linia pe contul {Simbol(l.Cont)}.");
                    var linii = l.Contributii.Select(p => p.LinieId).Distinct().ToList();
                    var contributii = l.Contributii.Select(p => p.Id).ToHashSet();
                    var surse = (storno
                            ? proprii.Where(p => p.InversaDinId is Guid o && contributii.Contains(o)).Select(p => new SursaPlata(p.Spatiu, p.Id))
                            : l.Contributii.Select(p => new SursaPlata(p.Spatiu, p.Id)))
                        .Concat(l.Transferuri).Distinct()
                        .Select(s => new SaftSursa { Spatiu = s.Spatiu, Id = s.Id }).ToList();
                    var suma = semn * l.Suma;
                    plata.Linii.Add(new SaftLiniePlata {
                        DetaliuId = linii.Count == 1 ? linii[0] ?? Guid.Empty : Guid.Empty,
                        LineNumber = ++pozitie,
                        SourceDocumentID = l.Tinta == TintaPlata.Document ? tinte[l.DocumentTinta.Value].Numar : null,
                        AccountID = Simbol(l.Cont),
                        CustomerID = customer, SupplierID = supplier,
                        Description = (linii.Count == 1 && linii[0] is Guid d ? descrieri.GetValueOrDefault(d) : null) ?? descriere,
                        DebitCreditIndicator = l.Latura == N.Latura.Debit ? "D" : "C",
                        PaymentLineAmount = suma,
                        Analiza = chei.Count == 1
                            ? Analiza(chei[0].CentruCost, chei[0].Proiect, chei[0].UnitateOrganizatorica,
                                chei[0].SursaFinantare, chei[0].CodFunctional, chei[0].CodEconomic)
                            : [],
                        TaxInformation = Nefiscal(),
                        TintaDocumentId = l.DocumentTinta,
                        Surse = surse,
                    });
                    plata.GrossTotal += suma;
                }
                dto.Plati.Add(plata);
            }
        }

        (string Customer, string Supplier) IdentitatiPlata(Guid cont, Guid? partener, string eticheta) {
            var rol = Rol(cont);
            if (rol == RolTertCont.Niciunul)
                return (idSocietate, idSocietate);
            if (partener is not Guid pid || !parteneri.TryGetValue(pid, out var p)) {
                Avert(CodAvertismentSaft.TertFaraPartener,
                    $"{eticheta}: linia de plată pe contul {Simbol(cont)} n-are partener — `CustomerID`/`SupplierID` ies cu codul societății.");
                return (idSocietate, idSocietate);
            }
            tertiReferiti.Add((rol, pid, Simbol(cont)));
            return rol == RolTertCont.Client ? (p.Id406, idSocietate) : (idSocietate, p.Id406);
        }

        void NeinclusPlata(CauzaNeincludere cauza, Guid document, Guid? repartitor, List<PostarePlata> postari) =>
            dto.Neincluse.Add(new SaftNeinclus {
                Cauza = cauza.ToString(), Sectiune = "Payments",
                DocumentId = document, DocumentNumar = documente[document].Numar, DocumentTip = CodTip(document),
                RepartitorId = repartitor,
                Debit = postari.Where(p => p.Latura == N.Latura.Debit).Sum(p => p.Valoare),
                Credit = postari.Where(p => p.Latura == N.Latura.Credit).Sum(p => p.Valoare),
                Randuri = postari.Count,
            });
    }
}
