using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public sealed class PostarePlata {
    public Guid Id { get; set; }
    public N.Spatiu Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public N.FelTranzactie Fel { get; set; }
    public DateOnly DataTranzactie { get; set; }
    public Guid DocumentId { get; set; }
    public Guid? LinieId { get; set; }
    public Guid Cont { get; set; }
    public N.Latura Latura { get; set; }
    public decimal Valoare { get; set; }
    public Guid? Partener { get; set; }
    public Guid? Unitate { get; set; }
    public Guid? Valuta { get; set; }
    public Guid? InversaDinId { get; set; }
    public Guid? CodFunctional { get; set; }
    public Guid? CodEconomic { get; set; }
    public Guid? SursaFinantare { get; set; }
    public Guid? UnitateOrganizatorica { get; set; }
    public Guid? Proiect { get; set; }
    public Guid? CentruCost { get; set; }
}

public sealed record SursaPlata(N.Spatiu Spatiu, Guid Id);

public enum TintaPlata { Document, PartidaInitiala, Rest, FaraPartida }

/// <summary>Partea unei plăți pe (cont, partener) care a ajuns la aceeași țintă, la capătul lunii operării.</summary>
public sealed class AlocarePlata {
    public Guid Cont { get; init; }
    public Guid? Partener { get; init; }
    public N.Latura Latura { get; init; }
    public TintaPlata Tinta { get; init; }
    public Guid? Unitate { get; init; }
    public Guid? DocumentTinta { get; init; }
    public decimal Suma { get; set; }
    public List<PostarePlata> Contributii { get; } = [];
    public List<SursaPlata> Transferuri { get; } = [];
}

public sealed class AlocariPlata {
    public Guid DocumentId { get; init; }
    public Guid OperareId { get; set; }
    public DateOnly Capat { get; set; }
    public List<PostarePlata> Operare { get; } = [];
    public List<AlocarePlata> Linii { get; } = [];
    public List<string> Erori { get; } = [];
}

/// <summary>
/// Alocarea plăților din cub (TR-D8 S2-D3): nominalizarea din `Operare`, apoi transferurile pe partida
/// proprie datate până la capătul lunii operării; restul rămâne pe partida proprie.
/// </summary>
public static class Plati {
    public static IQueryable<PostarePlata> Postari(IObjectSpace os) =>
        Contabil.Postari(os).Where(p => p.DocumentId != null).Select(p => new PostarePlata {
            Id = p.ID, Spatiu = p.Spatiu, TranzactieId = p.TranzactieId, Fel = p.Tranzactie.Fel,
            DataTranzactie = p.Tranzactie.Data, DocumentId = p.DocumentId.Value, LinieId = p.LinieId,
            Cont = p.Cont, Latura = p.Latura, Valoare = p.Valoare, Partener = p.Partener,
            Unitate = p.FelUnitate == N.FelUnitate.Partida ? p.Unitate : null, Valuta = p.Valuta, InversaDinId = p.InversaDinId,
            CodFunctional = p.CodFunctional, CodEconomic = p.CodEconomic, SursaFinantare = p.SursaFinantare,
            UnitateOrganizatorica = p.UnitateOrganizatorica, Proiect = p.Proiect, CentruCost = p.CentruCost,
        });

    /// <summary>Alocarea fiecărui document, pe postările latura contrapartidei primite pentru el.</summary>
    public static Dictionary<Guid, AlocariPlata> Alocari(IObjectSpace os, IReadOnlyDictionary<Guid, N.Latura> laturaContrapartidei) {
        var rezultat = laturaContrapartidei.Keys.ToDictionary(d => d, d => new AlocariPlata { DocumentId = d });
        if (rezultat.Count == 0)
            return rezultat;
        var documente = rezultat.Keys.ToList();
        foreach (var p in Postari(os).Where(p => documente.Contains(p.DocumentId) && p.Fel == N.FelTranzactie.Operare)
                     .ToList().Where(p => p.Latura == laturaContrapartidei[p.DocumentId]))
            rezultat[p.DocumentId].Operare.Add(p);

        var proprii = new Dictionary<Guid, (Guid Document, Guid Cont, Guid Partener)>();
        foreach (var a in rezultat.Values) {
            var tranzactii = a.Operare.Select(p => p.TranzactieId).Distinct().ToList();
            if (tranzactii.Count != 1) {
                a.Erori.Add($"{tranzactii.Count} tranzacții Operare cu postări de contrapartidă");
                continue;
            }
            a.OperareId = tranzactii[0];
            var data = a.Operare[0].DataTranzactie;
            a.Capat = new DateOnly(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month));
            foreach (var p in a.Operare.Where(p => p.Unitate != null && p.Partener != null))
                proprii.TryAdd(Partide.Identitate(a.DocumentId, p.Cont, p.Partener.Value), (a.DocumentId, p.Cont, p.Partener.Value));
        }

        var idsProprii = proprii.Keys.ToList();
        var capMaxim = rezultat.Values.Where(a => a.Erori.Count == 0).Select(a => a.Capat).DefaultIfEmpty().Max();
        var tranzactiiTransfer = idsProprii.Count == 0 ? [] : Partide.Postari(os)
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && idsProprii.Contains(p.Unitate.Value) && p.Data <= capMaxim)
            .Select(p => p.TranzactieId).Distinct().ToList();
        var transferuri = tranzactiiTransfer.Count == 0 ? [] : Partide.Postari(os)
            .Where(p => tranzactiiTransfer.Contains(p.TranzactieId))
            .Select(p => new { p.ID, p.Spatiu, p.TranzactieId, p.Data, p.Cont, p.Partener, p.Latura, p.Valoare, Unitate = p.Unitate.Value })
            .ToList().GroupBy(p => p.TranzactieId).ToDictionary(g => g.Key, g => g.ToList());

        var unitati = rezultat.Values.SelectMany(a => a.Operare).Where(p => p.Unitate != null).Select(p => p.Unitate.Value)
            .Concat(transferuri.Values.SelectMany(t => t).Select(p => p.Unitate)).Distinct().ToList();
        var origini = unitati.Count == 0 ? [] : Partide.Origini(os).Where(o => unitati.Contains(o.UnitateId))
            .Select(o => new { o.UnitateId, o.DocumentId }).ToList()
            .GroupBy(o => o.UnitateId).ToDictionary(g => g.Key, g => g.Select(o => o.DocumentId).Distinct().ToList());

        AlocarePlata Linie(AlocariPlata a, Guid cont, Guid? partener, N.Latura latura, TintaPlata tinta, Guid? unitate, Guid? document) {
            var linie = a.Linii.FirstOrDefault(l => l.Cont == cont && l.Partener == partener && l.Tinta == tinta && l.Unitate == unitate);
            if (linie == null)
                a.Linii.Add(linie = new AlocarePlata {
                    Cont = cont, Partener = partener, Latura = latura, Tinta = tinta, Unitate = unitate, DocumentTinta = document,
                });
            return linie;
        }

        AlocarePlata Tinta(AlocariPlata a, Guid cont, Guid partener, N.Latura latura, Guid unitate) {
            if (!origini.TryGetValue(unitate, out var o) || o.Count != 1) {
                a.Erori.Add($"partida {unitate} nu are o origine unică");
                return null;
            }
            return o[0] is Guid document
                ? Linie(a, cont, partener, latura, TintaPlata.Document, unitate, document)
                : Linie(a, cont, partener, latura, TintaPlata.PartidaInitiala, unitate, null);
        }

        foreach (var a in rezultat.Values.Where(a => a.Erori.Count == 0)) {
            foreach (var p in a.Operare) {
                if (p.Unitate is not Guid u || p.Partener is not Guid partener) {
                    var fara = Linie(a, p.Cont, p.Partener, p.Latura, TintaPlata.FaraPartida, null, null);
                    fara.Suma += p.Valoare;
                    fara.Contributii.Add(p);
                    continue;
                }
                var proprie = Partide.Identitate(a.DocumentId, p.Cont, partener);
                var linie = u == proprie
                    ? Linie(a, p.Cont, partener, p.Latura, TintaPlata.Rest, u, null)
                    : Tinta(a, p.Cont, partener, p.Latura, u);
                if (linie == null) continue;
                linie.Suma += p.Valoare;
                linie.Contributii.Add(p);
            }
            foreach (var rest in a.Linii.Where(l => l.Tinta == TintaPlata.Rest).ToList()) {
                foreach (var (id, perechi) in transferuri) {
                    var pe = perechi.Where(p => p.Unitate == rest.Unitate).ToList();
                    if (pe.Count == 0 || pe[0].Data > a.Capat) continue;
                    var alta = perechi.Where(p => p.Unitate != rest.Unitate).ToList();
                    if (perechi.Count != 2 || pe.Count != 1 || alta.Count != 1 || alta[0].Cont != pe[0].Cont
                            || alta[0].Partener != pe[0].Partener || alta[0].Latura != pe[0].Latura
                            || alta[0].Valoare != -pe[0].Valoare) {
                        a.Erori.Add($"transferul {id} nu este o pereche pe partida proprie");
                        continue;
                    }
                    var efect = pe[0].Latura == rest.Latura ? pe[0].Valoare : -pe[0].Valoare;
                    var tinta = Tinta(a, rest.Cont, rest.Partener.Value, rest.Latura, alta[0].Unitate);
                    if (tinta == null) continue;
                    tinta.Suma -= efect;
                    rest.Suma += efect;
                    tinta.Contributii.AddRange(rest.Contributii.Where(c => !tinta.Contributii.Contains(c)));
                    foreach (var p in perechi) {
                        var sursa = new SursaPlata(p.Spatiu, p.ID);
                        tinta.Transferuri.Add(sursa);
                        rest.Transferuri.Add(sursa);
                    }
                }
            }
            if (a.Linii.Any(l => l.Suma < 0m))
                a.Erori.Add("o alocare sau restul are semn opus plății");
            a.Linii.RemoveAll(l => l.Suma == 0m && l.Tinta != TintaPlata.FaraPartida);
        }
        return rezultat;
    }
}
