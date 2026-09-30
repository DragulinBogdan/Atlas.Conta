using System.Globalization;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SAF-B8 D2: comparația semantică ruta veche ↔ cub pe toate secțiunile unui fișier,
// cu clasele de diferență și martorii lor numerici.
static class SaftAb {
    public sealed record Rand(string Sectiune, object Cheie, string Eticheta, string Vechi, string Nou,
        IReadOnlyList<object> V, IReadOnlyList<object> N);

    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    static string B(decimal v) => v.ToString("0.00####", Inv);
    static string B(decimal? v) => v is { } x ? B(x) : "-";

    public static List<Rand> Compara(SaftDto vechi, SaftDto nou, Func<Guid, string> eticheta) {
        var randuri = new List<Rand>();
        void Sectiune<TK, TI>(string nume, Func<SaftDto, IEnumerable<TI>> elemente, Func<TI, TK> cheie,
                Func<IReadOnlyList<TI>, string> valoare, Func<TK, string> text) where TK : notnull where TI : notnull {
            var v = elemente(vechi).GroupBy(cheie).ToDictionary(g => g.Key, g => (IReadOnlyList<TI>)g.ToList());
            var n = elemente(nou).GroupBy(cheie).ToDictionary(g => g.Key, g => (IReadOnlyList<TI>)g.ToList());
            foreach (var k in v.Keys.Union(n.Keys)) {
                var a = v.TryGetValue(k, out var lv) ? valoare(lv) : "absent";
                var b = n.TryGetValue(k, out var ln) ? valoare(ln) : "absent";
                if (a != b)
                    randuri.Add(new(nume, k, text(k), a, b, lv?.Cast<object>().ToList() ?? [], ln?.Cast<object>().ToList() ?? []));
            }
        }
        static string Toate<T>(IEnumerable<T> x, Func<T, string> f) => string.Join(" + ", x.Select(f).Order(StringComparer.Ordinal));
        string Doc(Guid id, bool storno) => (id == Guid.Empty ? "Deschidere" : eticheta(id)) + (storno ? " storno" : "");

        Sectiune("Conturi", d => d.Conturi, c => c.AccountID, l => Toate(l, c =>
            $"{B(c.OpeningDebitBalance)}/{B(c.OpeningCreditBalance)} → {B(c.ClosingDebitBalance)}/{B(c.ClosingCreditBalance)}"), k => k);
        Sectiune("Terți", d => d.Clienti.Select(t => ("C", t)).Concat(d.Furnizori.Select(t => ("F", t))),
            x => $"{x.Item1} {x.t.Id} {x.t.AccountID}", l => Toate(l, x =>
                $"{B(x.t.OpeningDebitBalance)}/{B(x.t.OpeningCreditBalance)} → {B(x.t.ClosingDebitBalance)}/{B(x.t.ClosingCreditBalance)}"), k => k);
        Sectiune("GL", d => d.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii.Select(l => (t.DocumentId, l))),
            x => (x.DocumentId, x.l.AccountID, x.l.DebitCreditIndicator), l => B(l.Sum(x => x.l.Amount)),
            k => $"{Doc(k.DocumentId, false)} {k.AccountID} {k.DebitCreditIndicator}");
        Sectiune("Facturi", d => d.FacturiEmise.Concat(d.FacturiPrimite), f => (f.DocumentId, f.Storno), l => Toate(l, Factura),
            k => Doc(k.DocumentId, k.Storno));
        Sectiune("Plăți", d => d.Plati, p => (p.DocumentId, p.Storno), l => Toate(l, p => $"{B(p.GrossTotal)} ["
            + string.Join("; ", p.Linii.Select(x => $"{x.SourceDocumentID} {x.AccountID} {x.DebitCreditIndicator} {B(x.PaymentLineAmount)}")
                .Order(StringComparer.Ordinal)) + "]"), k => Doc(k.DocumentId, k.Storno));
        Sectiune("Mișcări", d => d.MiscariStoc.SelectMany(m => m.Linii.Select(l => (m, l))),
            x => (x.m.DocumentId, x.m.Storno, x.l.LotId, x.l.RepartitorId),
            l => $"{string.Join(",", l.Select(x => x.l.MovementSubType).Distinct().Order(StringComparer.Ordinal))} "
                + $"{B(l.Sum(x => x.l.Quantity))}/{B(l.Sum(x => x.l.BookValue))}",
            k => $"{Doc(k.DocumentId, k.Storno)} lot {eticheta(k.LotId)} gest {eticheta(k.RepartitorId)}");
        // StockAccountNo / WarehouseID sunt forma cheii (S3-D5), verificată prin XSD/DUK și unicitate, nu prin A/B (SAF-B5).
        Sectiune("Stoc", d => d.StocFizic, p => (p.LotId, p.RepartitorId), l => Toate(l, p =>
            $"{p.ProductType} {B(p.OpeningQuantity)}/{B(p.OpeningValue)} → {B(p.ClosingQuantity)}/{B(p.ClosingValue)}"),
            k => $"lot {eticheta(k.LotId)} gest {eticheta(k.RepartitorId)}");
        Sectiune("Diagnostic", d => d.Refuzuri.Select(r => "refuz " + r.Cod)
            .Concat(d.Avertismente.Select(a => "avertisment " + a.Cod))
            .Concat(d.Neincluse.Select(n => "neinclus " + n.Cauza)), x => x, l => l.Count.ToString(Inv), k => k);
        return randuri;
    }

    static string Factura(SaftFactura f) =>
        $"{f.InvoiceType} {f.InvoiceNo} {f.InvoiceDate:yyyy-MM-dd} {f.AccountID} {B(f.NetTotal)}/{B(f.GrossTotal)} ["
        + string.Join("; ", f.Linii.Select(l => $"{l.AccountID} {B(l.Quantity)}×{B(l.InvoiceLineAmount)} {l.TaxInformation?.TaxCode} {B(l.TaxInformation?.TaxAmount ?? 0)}")
            .Order(StringComparer.Ordinal)) + "]";

    public static string Eticheta(IObjectSpace os, Guid id) =>
        os.GetObjectByKey<Document>(id) is { } d ? $"{d.GetType().Name.Replace("Proxy", "")} {d.Numar}"
        : os.GetObjectByKey<Repartitor>(id) is { } r ? r.Cod
        : os.GetObjectByKey<Lot>(id) is { } l ? $"{l.Eticheta} #{id.ToString("N")[^4..]}"
        : id.ToString("N")[^8..];

    // Raportul: SAFT_AB_RAPORT = fișierul Markdown în care se adaugă rândurile.
    public static void Scrie(string scena, bool privat, int an, int luna, IEnumerable<(Rand Rand, string Clasa)> randuri) {
        var cale = Environment.GetEnvironmentVariable("SAFT_AB_RAPORT");
        if (string.IsNullOrEmpty(cale)) return;
        static string E(string s) => s.Replace("|", "\\|");
        var profil = privat ? "privat" : "bugetar";
        var linii = randuri.Select(x =>
                $"| {scena} | {profil} | {an}-{luna:00} | {x.Rand.Sectiune} | {E(x.Rand.Eticheta)} | {E(x.Rand.Vechi)} | {E(x.Rand.Nou)} | {x.Clasa ?? "**NECLASIFICAT**"} |")
            .DefaultIfEmpty($"| {scena} | {profil} | {an}-{luna:00} | — | fără diferențe | | | |");
        File.AppendAllLines(cale, linii);
    }

    // Rulează clasificatorul pe lunile scenei, în ordine; o diferență neclasificată sau o pereche deschisă respinge.
    static (List<(int Luna, List<(Rand Rand, string Clasa)> Randuri)> Luni, List<string> Perechi) Evalueaza(
            IObjectSpace os, int an, IReadOnlyList<(int Luna, SaftDto V, SaftDto N)> luni) {
        var clasificator = new Clasificator();
        var rezultat = new List<(int, List<(Rand, string)>)>();
        foreach (var (luna, v, n) in luni) {
            clasificator.LunaNoua(v, n);
            rezultat.Add((luna, Compara(v, n, g => Eticheta(os, g)).Select(r => (r, clasificator.Clasa(os, an, luna, r))).ToList()));
        }
        return (rezultat, clasificator.PerechiNeinchise(os));
    }

    static SaftDto Cloneaza(SaftDto d) =>
        System.Text.Json.JsonSerializer.Deserialize<SaftDto>(System.Text.Json.JsonSerializer.Serialize(d));

    // Fiecare mutant atinge o măsură din exportul pe cub; întoarce false dacă luna n-are ținta lui.
    static readonly (string Nume, Func<SaftDto, bool> Muta)[] Mutanti = [
        ("brutul unei plăți +1", d => d.Plati.FirstOrDefault() is { } p && Muta(() => { p.GrossTotal += 1; p.Linii[0].PaymentLineAmount += 1; })),
        ("o linie GL +1", d => d.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).FirstOrDefault() is { } l && Muta(() => l.Amount += 1)),
        ("data unei facturi +1 zi", d => d.FacturiEmise.Concat(d.FacturiPrimite).FirstOrDefault() is { } f && Muta(() => f.InvoiceDate = f.InvoiceDate.AddDays(1))),
        ("valoarea unei linii de mișcare +1", d => d.MiscariStoc.SelectMany(m => m.Linii).FirstOrDefault() is { } l && Muta(() => l.BookValue += 1)),
        ("ClosingValue al unei poziții +1", d => d.StocFizic.FirstOrDefault() is { } p && Muta(() => p.ClosingValue += 1)),
    ];
    static bool Muta(Action a) { a(); return true; }

    // SAF-B8 D2: raportul, verificarea clasificării și mutanții pe prima lună a scenei.
    public static void Ruleaza(IObjectSpace os, string scena, string id, bool privat, int an,
            IReadOnlyList<(int Luna, SaftDto V, SaftDto N)> luni, Action<string, bool> verifica) {
        var (rezultat, perechi) = Evalueaza(os, an, luni);
        foreach (var (luna, randuri) in rezultat) {
            Scrie(scena, privat, an, luna, randuri);
            foreach (var x in randuri.Where(x => x.Clasa == null))
                Console.WriteLine($"     A/B NECLASIFICAT luna {luna}: {x.Rand.Sectiune} {x.Rand.Eticheta}: {x.Rand.Vechi} → {x.Rand.Nou}");
            verifica($"{id}: A/B final luna {luna}: {randuri.Count} diferențe, toate clasificate", randuri.All(x => x.Clasa != null));
        }
        foreach (var p in perechi) Console.WriteLine($"     A/B PERECHE NEÎNCHISĂ: {p}");
        verifica($"{id}: A/B final — perechile document ↔ conex egale cumulat (sau conexul Draft fără registru)", perechi.Count == 0);
        if (luni.Count == 0) return;
        var respinsi = new List<string>();
        var aplicati = new List<string>();
        foreach (var (nume, muta) in Mutanti) {
            var mutant = Cloneaza(luni[0].N);
            if (!muta(mutant)) continue;
            aplicati.Add(nume);
            var (r, p) = Evalueaza(os, an, [(luni[0].Luna, luni[0].V, mutant), .. luni.Skip(1)]);
            if (r.Any(x => x.Randuri.Any(y => y.Clasa == null)) || p.Count > 0) respinsi.Add(nume);
        }
        Console.WriteLine($"     MĂSURAT (A/B mutanți luna {luni[0].Luna}): aplicați [{string.Join("; ", aplicati)}], "
            + $"nerespinși [{string.Join("; ", aplicati.Except(respinsi))}]");
        verifica($"{id}: A/B final — mutanții pe luna {luni[0].Luna} sunt respinși ({string.Join("; ", aplicati)})",
            aplicati.Count > 0 && respinsi.Count == aplicati.Count);
    }

    // Clasele admise (SAF-B5, S1-R, S2-R, S3-D8, S3-RV4); fiecare cere martorul ei numeric.
    public sealed class Clasificator {
        SaftDto vechi, nou;
        bool facturaNumaiPeCub, liniiStocPeCub, contIstoric;
        readonly HashSet<Guid> explicate = [];
        readonly Dictionary<(Guid Sursa, Guid Conex), Dictionary<string, (decimal Vechi, decimal Nou)>> perechi = [];

        public void LunaNoua(SaftDto v, SaftDto n) {
            (vechi, nou) = (v, n);
            facturaNumaiPeCub = liniiStocPeCub = contIstoric = false;
        }

        public string Clasa(IObjectSpace os, int an, int luna, Rand r) => r.Sectiune switch {
            "GL" => Gl(os, an, luna, r),
            "Facturi" => Factura(os, r),
            "Plăți" => Plata(r),
            "Mișcări" => Miscare(os, r),
            "Stoc" => Stoc(os, an, luna, r),
            "Diagnostic" => Diagnostic(r),
            _ => null,
        };

        // Perechea document ↔ conex (FCT ↔ NIR): fiecare rută o poartă pe alt document, cumulat pe scenă egal.
        (Guid Sursa, Guid Conex)? Pereche(IObjectSpace os, Guid doc) {
            var d = os.GetObjectByKey<Document>(doc);
            if (d == null) return null;
            if (d.Autogenerat && d.DocumentSursaId is { } s) return (s, doc);
            var conex = os.GetObjectsQuery<Document>().Where(x => x.Autogenerat && x.DocumentSursaId == doc).Select(x => x.ID).ToList();
            return conex.Count == 1 ? (doc, conex[0]) : null;
        }

        void Acumuleaza((Guid Sursa, Guid Conex) pereche, string subcheie, decimal v, decimal n) {
            if (!perechi.TryGetValue(pereche, out var d)) perechi[pereche] = d = [];
            var x = d.GetValueOrDefault(subcheie);
            d[subcheie] = (x.Vechi + v, x.Nou + n);
            explicate.Add(pereche.Sursa); explicate.Add(pereche.Conex);
        }

        string Gl(IObjectSpace os, int an, int luna, Rand r) {
            var (doc, cont, dc) = ((Guid, string, string))r.Cheie;
            static decimal S(IReadOnlyList<object> l) => l.Cast<(Guid, SaftLinieTranzactie l)>().Sum(x => x.l.Amount);
            if (doc == Guid.Empty && r.V.Count == 0) {
                var inceput = new DateOnly(an, luna, 1);
                var latura = dc == "D" ? N.Latura.Debit : N.Latura.Credit;
                var conturi = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToList()
                    .Where(c => SaftReguli.SimbolSaft(c.Simbol) == cont).Select(c => c.ID).ToList();
                var cub = os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                        && p.Carte == N.Carte.Contabil && p.Latura == latura && conturi.Contains(p.Cont)
                        && p.Data >= inceput && p.Data <= inceput.AddMonths(1).AddDays(-1))
                    .Sum(p => p.Valoare);
                return cub == S(r.N) ? "094/SC-SAFT-45: jurnalul DESCHIDERE din cub; ruta veche nu vedea deschiderea generică" : null;
            }
            if (Pereche(os, doc) is not { } p) return null;
            Acumuleaza(p, $"GL {cont} {dc}", S(r.V), S(r.N));
            return "SAF-B5: recepția stă pe documentul-sursă în cub, pe conex în registrul vechi (martor: perechea egală cumulat)";
        }

        string Factura(IObjectSpace os, Rand r) {
            var (doc, _) = ((Guid, bool))r.Cheie;
            var pereche = Pereche(os, doc) is { } pp && pp.Sursa == doc ? pp : ((Guid Sursa, Guid Conex)?)null;
            if (r.V.Count == 0 && r.N.Count == 1 && pereche is { } p
                && (perechi.ContainsKey(p) || RegistruPe(os, p.Conex)) && r.N[0] is SaftFactura f
                && f.InvoiceDate == DataRadacina(os, doc) && f.NetTotal == f.Linii.Sum(l => l.InvoiceLineAmount)
                && f.GrossTotal == f.NetTotal + f.Linii.Sum(l => l.TaxInformation?.TaxAmount ?? 0)) {
                facturaNumaiPeCub = true;
                explicate.Add(doc);
                return "SAF-B5: factura fără rând propriu în registrul vechi (recepția și baza pe conex); cubul o emite";
            }
            if (r.V.Count != 1 || r.N.Count != 1) return null;
            var v = (SaftFactura)r.V[0];
            var n = (SaftFactura)r.N[0];
            if (pereche != null && v.InvoiceType == n.InvoiceType && v.InvoiceDate == n.InvoiceDate) {
                var extra = n.Linii.ToList();
                foreach (var l in v.Linii.Select(Linie)) {
                    var i = extra.FindIndex(x => Linie(x) == l);
                    if (i < 0) { extra = null; break; }
                    extra.RemoveAt(i);
                }
                if (extra is { Count: > 0 } && n.NetTotal - v.NetTotal == extra.Sum(x => x.InvoiceLineAmount)
                    && n.GrossTotal - v.GrossTotal == extra.Sum(x => x.InvoiceLineAmount + (x.TaxInformation?.TaxAmount ?? 0))) {
                    liniiStocPeCub = true;
                    explicate.Add(doc);
                    return "SAF-B5: linia de stoc a facturii are contul recepției în cub; ruta veche o lăsa în Neincluse (LinieFaraContrapartida)";
                }
            }
            var tip = v.InvoiceType == n.InvoiceType ? null
                : v.InvoiceType == "380" && n.InvoiceType == "384" && os.GetObjectByKey<Document>(doc)?.CorecteazaId != null ? "S1-D5 (B')" : "?";
            if (tip == "?") return null;
            var data = v.InvoiceDate == n.InvoiceDate ? null : n.InvoiceDate == DataRadacina(os, doc) ? "S1-R4" : "?";
            if (data == "?") return null;
            var brut = v.GrossTotal == n.GrossTotal ? null
                : n.GrossTotal == n.NetTotal && v.GrossTotal - n.GrossTotal == n.Linii.Sum(l => l.TaxInformation?.TaxAmount ?? 0)
                    && TaxareInversa(os, doc) ? "S1-D4 (TI: brutul fără autocolectare)" : "?";
            if (brut == "?") return null;
            var restul = new SaftFactura {
                InvoiceType = n.InvoiceType, InvoiceNo = v.InvoiceNo, InvoiceDate = n.InvoiceDate, AccountID = v.AccountID,
                NetTotal = v.NetTotal, GrossTotal = n.GrossTotal, Linii = v.Linii,
            };
            return SaftAb.Factura(restul) == SaftAb.Factura(n)
                ? string.Join(" + ", new[] { tip, data, brut }.Where(x => x != null))
                : null;
        }

        static string Linie(SaftLinieFactura l) =>
            $"{l.AccountID} {B(l.Quantity)}×{B(l.InvoiceLineAmount)} {l.TaxInformation?.TaxCode} {B(l.TaxInformation?.TaxAmount ?? 0)}";

        static bool RegistruPe(IObjectSpace os, Guid doc) =>
            os.GetObjectsQuery<RegistruContabil>().Any(x => x.DocumentId == doc) || os.GetObjectsQuery<RegistruStoc>().Any(x => x.DocumentId == doc);

        static DateOnly DataRadacina(IObjectSpace os, Guid doc) {
            var d = os.GetObjectByKey<Document>(doc);
            while (d.CorecteazaId is { } c) d = os.GetObjectByKey<Document>(c);
            return d.Data;
        }

        static bool TaxareInversa(IObjectSpace os, Guid doc) {
            var tipuri = os.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == doc && l.TipTvaId != null)
                .Select(l => l.TipTvaId.Value).ToList();
            return tipuri.Count > 0 && os.GetObjectsQuery<TipTva>().Where(t => tipuri.Contains(t.ID)).All(t => t.Cod.StartsWith("TI"));
        }

        static string Plata(Rand r) {
            if (r.V.Count != 1 || r.N.Count != 1) return null;
            var v = (SaftPlata)r.V[0];
            var n = (SaftPlata)r.N[0];
            static string Conturi(SaftPlata p) => string.Join(",", p.Linii.Select(l => $"{l.AccountID} {l.DebitCreditIndicator}").Distinct().Order());
            return v.GrossTotal == n.GrossTotal && v.Linii.Sum(l => l.PaymentLineAmount) == n.Linii.Sum(l => l.PaymentLineAmount)
                && Conturi(v) == Conturi(n)
                ? "S2-D2/D3: alocarea pe partide din cub față de atribuirea întregii plăți (brut și Σ linii conservate)"
                : null;
        }

        string Miscare(IObjectSpace os, Rand r) {
            var (doc, _, lot, gest) = ((Guid, bool, Guid, Guid))r.Cheie;
            var liniiN = r.N.Cast<(SaftMiscareStoc m, SaftLinieMiscareStoc l)>().Select(x => x.l).ToList();
            static (decimal Q, decimal V) S(IReadOnlyList<object> l) =>
                (l.Cast<(SaftMiscareStoc m, SaftLinieMiscareStoc l)>().Sum(x => x.l.Quantity),
                 l.Cast<(SaftMiscareStoc m, SaftLinieMiscareStoc l)>().Sum(x => x.l.BookValue));
            var (v, n) = (S(r.V), S(r.N));
            string clasa = null;
            if (Pereche(os, doc) is { } p) {
                Acumuleaza(p, $"lot {lot} gest {gest} Q", v.Q, n.Q);
                Acumuleaza(p, $"lot {lot} gest {gest} V", v.V, n.V);
                clasa = doc == p.Conex && r.V.Count > 0 && r.N.Count > 0
                    ? "S3-R1: NIR delta față de recepția integrală pe NIR în registru (martor: perechea egală cumulat)"
                    : "SAF-B5: recepția pe documentul-sursă în cub, pe conex în registru (martor: perechea egală cumulat)";
            }
            else if (os.GetObjectByKey<Document>(doc) is Asamblare && v.Q == n.Q && Math.Abs(v.V - n.V) <= 0.01m
                && os.GetObjectsQuery<C.Postare>().Where(x => x.DocumentId == doc && x.Unitate != null && x.Cantitate != 0)
                    .GroupBy(x => x.Tranzactie.ID).All(g => g.Sum(x => x.Latura == N.Latura.Debit ? x.Valoare : -x.Valoare) == 0))
                clasa = "S3-R2: Δ ASM — valoarea FIFO a cubului, ΣP + ΣΔ = ΣC pe tranzacție";
            else if (r.V.Count == 0 && liniiN.Count > 0 && liniiN.All(l => ProdusFaraCont(l.ProdusId) && ContulPostarii(os, doc, l)))
                clasa = ContIstoric();
            if (clasa != null) explicate.Add(doc);
            return clasa;
        }

        bool ProdusFaraCont(Guid produs) => vechi.Neincluse.Any(x => x.Cauza == "FaraContStoc" && x.ProdusId == produs)
            || vechi.StocFizic.Any(x => x.ProdusId == produs && x.ProductType == "0");

        static bool ContulPostarii(IObjectSpace os, Guid doc, SaftLinieMiscareStoc l) {
            var conturi = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Unitate == l.LotId).Select(p => p.Cont).Distinct().ToList();
            return os.GetObjectsQuery<Cont>().Where(c => conturi.Contains(c.ID)).Select(c => c.Simbol).ToList()
                .Any(sim => SaftReguli.SimbolSaft(sim) == l.AccountId);
        }

        string ContIstoric() {
            contIstoric = true;
            return "SAF-B5: contul istoric al postării față de contul curent al tipului de material (fără cont azi)";
        }

        // Poziția pe sursă: nou − vechi = Σ pe document (cub − registru), iar fiecare document cu diferență e explicat.
        string Stoc(IObjectSpace os, int an, int luna, Rand r) {
            var (lot, gest) = ((Guid, Guid))r.Cheie;
            static (decimal OQ, decimal OV, decimal CQ, decimal CV, string Tip) S(IReadOnlyList<object> l) =>
                (l.Cast<SaftStocFizic>().Sum(p => p.OpeningQuantity), l.Cast<SaftStocFizic>().Sum(p => p.OpeningValue),
                 l.Cast<SaftStocFizic>().Sum(p => p.ClosingQuantity), l.Cast<SaftStocFizic>().Sum(p => p.ClosingValue),
                 string.Join(",", l.Cast<SaftStocFizic>().Select(p => p.ProductType).Distinct()));
            var (v, n) = (S(r.V), S(r.N));
            var inceput = new DateOnly(an, luna, 1);
            var capat = inceput.AddMonths(1).AddDays(-1);
            var cub = os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == lot && p.Gestiune == gest && p.Carte == N.Carte.Contabil && p.Data <= capat)
                .Select(p => new { Doc = p.DocumentId ?? Guid.Empty, p.Data, Deschidere = p.Tranzactie.Fel == N.FelTranzactie.Deschidere,
                    p.Cantitate, V = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare }).ToList();
            var reg = os.GetObjectsQuery<RegistruStoc>().Where(x => x.LotId == lot && x.RepartitorId == gest && x.Data <= capat)
                .Select(x => new { Doc = x.DocumentId ?? Guid.Empty, x.Data, x.Cantitate, x.Valoare }).ToList();
            var diferente = cub.Select(x => (x.Doc, x.Data, x.Deschidere, Q: x.Cantitate, V: x.V))
                .Concat(reg.Select(x => (x.Doc, x.Data, Deschidere: false, Q: -x.Cantitate, V: -x.Valoare))).ToList();
            decimal Q(bool deschidere) => diferente.Where(x => x.Data < inceput || deschidere && x.Deschidere).Sum(x => x.Q);
            decimal V(bool deschidere) => diferente.Where(x => x.Data < inceput || deschidere && x.Deschidere).Sum(x => x.V);
            if (n.OQ - v.OQ != Q(true) || n.OV - v.OV != V(true)
                || n.CQ - v.CQ != diferente.Sum(x => x.Q) || n.CV - v.CV != diferente.Sum(x => x.V)) return null;
            var peDocument = diferente.GroupBy(x => x.Doc).Where(g => g.Sum(x => x.Q) != 0 || g.Sum(x => x.V) != 0).Select(g => g.Key).ToList();
            var artefacte = peDocument.Where(d => d != Guid.Empty && !explicate.Contains(d)
                && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == d) && os.GetObjectsQuery<RegistruStoc>().Any(x => x.DocumentId == d)).ToList();
            if (peDocument.Any(d => d != Guid.Empty && !explicate.Contains(d) && !artefacte.Contains(d))) return null;
            var clase = new List<string>();
            if (r.V.Count > 0 && r.N.Count > 0 && v.Tip != n.Tip) {
                if (v.Tip != "0" || cub.Count == 0) return null;
                clase.Add(ContIstoric());
            }
            if (peDocument.Contains(Guid.Empty)) clase.Add("094: deschiderea generică există numai în cub (fără registru)");
            if (artefacte.Count > 0) clase.Add("artefact de probă: document operat nemigrat, numai în registru (imposibil în producție, ProbeCub.Nemigrat)");
            if (peDocument.Any(explicate.Contains)) clase.Add("Σ al diferențelor clasificate pe documente");
            return clase.Count > 0 ? "poziția pe sursă (cub − registru pe document): " + string.Join(" + ", clase) : null;
        }

        string Diagnostic(Rand r) => (string)r.Cheie switch {
            "avertisment NumarFacturaDuplicat" when r.N.Count == 0 => "S1-D5 (B'): 381/384 poartă numărul facturii, fără avertisment",
            "neinclus TipFaraSectiuneFacturi" when r.N.Count == 0 => "S2-R4: baza fără factură (DVI, decont) în cusături, nu Neincluse",
            "avertisment DataPostariiInAfaraPerioadei" when r.N.Count == 0 => "S1-R: datele din tranzacția cubului, nu din DataOperare",
            "avertisment SoldPeTipStocNeraportat" when r.N.Count == 0 => "S3-D1: categoria pe cont; soldul nestoc nu e avertisment (F27-r10)",
            "avertisment PlataAnalizaMixta" when r.V.Count == 0 => "S2-D2 (SC-SAFT-33)",
            "avertisment PlataPePartidaInitiala" when r.V.Count == 0 => "S2-D3 (SC-SAFT-30)",
            "avertisment FaraCodNc" or "avertisment FaraUnitateMasura" when r.V.Count == 0 && facturaNumaiPeCub =>
                "SAF-B5: produsul facturii emise numai de cub intră în MasterFiles cu avertismentele lui",
            "avertisment TipTvaFaraCodSaft" when r.N.Count == 0 && nou.Refuzuri.Any(x => x.Cod == SaftProiectii.RefuzMapare) =>
                "S1-D4: maparea TVA lipsă refuză fișierul (avertismentul vechi devenit refuz)",
            "refuz " + SaftProiectii.RefuzMapare when r.V.Count == 0 && vechi.Avertismente.Any(x => x.Cod == "TipTvaFaraCodSaft") =>
                "S1-D4: maparea TVA lipsă refuză fișierul (avertismentul vechi devenit refuz)",
            "avertisment LinieFaraContrapartida" or "neinclus FaraContrapartida" when r.N.Count == 0 && liniiStocPeCub =>
                "SAF-B5: linia de stoc a facturii are contul recepției în cub",
            "avertisment ProdusFaraContStoc" or "neinclus FaraContStoc" when r.N.Count == 0 && contIstoric =>
                "SAF-B5: contul istoric al postării; produsul fără cont azi nu mai iese din fișier",
            _ => null,
        };

        // La capătul scenei: fiecare pereche folosită e egală cumulat, sau conexul e Draft fără registru (recepția numai în cub).
        public List<string> PerechiNeinchise(IObjectSpace os) => perechi
            .Where(p => p.Value.Any(x => x.Value.Vechi != x.Value.Nou)
                && !(os.GetObjectByKey<Document>(p.Key.Conex)?.Stare == StareDocument.Draft && !RegistruPe(os, p.Key.Conex)))
            .Select(p => $"{Eticheta(os, p.Key.Sursa)} ↔ {Eticheta(os, p.Key.Conex)}: "
                + string.Join("; ", p.Value.Where(x => x.Value.Vechi != x.Value.Nou).Select(x => $"{x.Key} {x.Value.Vechi} → {x.Value.Nou}")))
            .ToList();
    }
}
