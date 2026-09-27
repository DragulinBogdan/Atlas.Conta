using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using System.Runtime.CompilerServices;

namespace Atlas.Conta.BackOffice.Module.Culegere;

/// <summary>
/// L3: precompletarea și recalculul draftului. Adaptorul XAF și `Api/*Apply` o cheamă
/// la aceleași momente; validarea culegerii e a gardianului de commit (104c).
/// </summary>
public static class CulegereDocument {
    static readonly ConditionalWeakTable<IObjectSpace, object> inCalcul = new();
    /// <summary>Starea bazei unei linii existente, înaintea mapării ei.</summary>
    public readonly record struct Amprenta(decimal? Baza, Guid? TipTvaId);

    /// <summary>Documentul nou intră în evidență azi, dacă apelantul nu i-a dat data.</summary>
    public static void Nou(Document doc) {
        if (doc.Data != default)
            return;
        doc.Data = DateOnly.FromDateTime(DateTime.Today);
        doc.DataInregistrare = doc.Data;
    }

    /// <summary>Data înregistrării urmează data documentului cât timp erau egale (F27-D4).</summary>
    public static void DataMutata(Document doc, DateOnly veche) {
        if (veche == doc.DataInregistrare)
            doc.DataInregistrare = doc.Data;
    }

    /// <summary>
    /// O proprietate a liniei s-a schimbat în ecran: produsul precompletează, baza recalculează,
    /// iar TVA-ul adus la 0 revine la cotă (0 nu e TVA cules).
    /// </summary>
    public static void LinieSchimbata(IObjectSpace os, Document doc, DocumentDetaliu linie, string proprietate) {
        if (doc.Stare != StareDocument.Draft || inCalcul.TryGetValue(os, out _))
            return;
        if (proprietate is nameof(FacturaIntrareDetaliu.Produs) or nameof(FacturaIntrareDetaliu.ProdusId))
            ProdusAles(os, doc, linie);
        else if (doc.IntrariBaza().Contains(proprietate)
                || proprietate == nameof(DocumentDetaliu.ValoareTva) && linie.ValoareTva == 0m && doc.CuTva())
            BazaSchimbata(os, doc, linie);
        else if (proprietate == nameof(DocumentDetaliu.ValoareTva) && doc.CuTva())
            linie.TvaCules = linie.ValoareTva != 0m;
    }

    public static void ProdusAles(IObjectSpace os, Document doc, DocumentDetaliu linie) {
        PrecompleteazaTipMaterial(os, linie);
        if (linie.TipTvaId == null && AplicaTipTvaImplicit(os, doc, linie))
            BazaSchimbata(os, doc, linie);
    }

    /// <summary>Baza s-a mișcat: valorile se recalculează, iar TVA-ul cules pe baza veche se pierde.</summary>
    public static void BazaSchimbata(IObjectSpace os, Document doc, DocumentDetaliu linie) {
        linie.TvaCules = false;
        Calculeaza(os, doc, [linie], false);
    }

    public static void RecalculeazaTva(IObjectSpace os, Document doc, IEnumerable<DocumentDetaliu> linii) {
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException("TVA se recalculează numai pe draft.");
        var selectate = linii.DistinctBy(l => l.ID).ToList();
        if (!doc.CuTva() || selectate.Count == 0 || selectate.Any(l => l.DocumentId != doc.ID))
            throw new OperareException("Selectați explicit liniile documentului cu TVA.");
        foreach (var linie in selectate)
            linie.TvaCules = false;
        Calculeaza(os, doc, selectate, false);
    }

    static void Calculeaza(IObjectSpace os, Document doc, List<DocumentDetaliu> linii, bool pastreaza) {
        inCalcul.Add(os, new object());
        try { doc.CalculeazaValori(os, linii, pastreazaTvaCules: pastreaza); }
        finally { inCalcul.Remove(os); }
    }

    /// <summary>Null pe linia nouă.</summary>
    public static Amprenta? Urmareste(IObjectSpace os, Document doc, DocumentDetaliu linie) =>
        os.IsNewObject(linie) ? null : new Amprenta(doc.BazaLinie(os, linie), linie.TipTvaId);

    /// <summary>
    /// Linia tocmai mapată de adaptorul API: precompletările, recalculul când baza s-a mișcat
    /// față de <paramref name="inainte"/>, apoi TVA-ul cules explicit (36a). Un 0 explicit nu e
    /// TVA cules: e acceptat numai dacă și cota dă 0; lipsa taxei se alege prin tipul de TVA.
    /// </summary>
    public static void Mapata(IObjectSpace os, Document doc, DocumentDetaliu linie, Amprenta? inainte, decimal? tvaCules) {
        VerificaScara(os, doc, [linie]);
        PrecompleteazaTipMaterial(os, linie);
        if (inainte == null && linie.TipTvaId == null)
            AplicaTipTvaImplicit(os, doc, linie);
        if (inainte != new Amprenta(doc.BazaLinie(os, linie), linie.TipTvaId) || tvaCules == 0m && doc.CuTva())
            BazaSchimbata(os, doc, linie);
        if (tvaCules is not decimal valoare)
            return;
        if (valoare == 0m && doc.CuTva()) {
            if (linie.ValoareTva != 0m)
                throw new OperareException($"Valoarea TVA 0 contrazice tipul de TVA al liniei, a cărui cotă dă "
                    + $"{linie.ValoareTva:N2}. Lăsați valoarea goală pentru calculul din cotă sau alegeți un tip "
                    + "de TVA fără taxă (scutit, neimpozabil).");
            return;
        }
        if (valoare < 0m && doc.SemnulEAlOperarii())
            valoare = -valoare;
        if (RefuzTvaCules(os, doc, linie, valoare) is string refuz)
            throw new OperareException(refuz);
        linie.ValoareTva = valoare;
        linie.TvaCules = valoare != 0m;
    }

    /// <summary>
    /// Înaintea commit-ului culegerii, pe orice ușă: fiecare document Draft atins
    /// își primește normalizarea tipului, precompletările rămase, loturile și valorile.
    /// </summary>
    public static void InainteDeSalvare(IObjectSpace os) {
        foreach (var doc in DocumenteAtinse(os))
            Normalizeaza(os, doc);
        LoturiCulegereService.CurataOrfane(os);
    }

    public static void Normalizeaza(IObjectSpace os, Document doc) {
        NormalizariTip.Aplica(os, doc);
        var linii = doc.Detalii.Where(l => !os.IsObjectToDelete(l)).ToList();
        foreach (var linie in linii.Where(os.IsNewObject)) {
            PrecompleteazaTipMaterial(os, linie);
            if (linie.TipTvaId == null)
                AplicaTipTvaImplicit(os, doc, linie);
        }
        VerificaScara(os, doc, linii);
        VerificaTvaCules(os, doc, linii);
        LoturiCulegereService.Sincronizeaza(os, doc);
        Calculeaza(os, doc, linii, true);
    }

    // 49e: recalculul rotunjește valoarea culeasă (RDC, DVI), deci scara se verifică înaintea lui.
    static void VerificaScara(IObjectSpace os, Document doc, List<DocumentDetaliu> linii) {
        var erori = new List<string>();
        GardianEditare.VerificaScara(os, doc, erori);
        foreach (var linie in linii)
            GardianEditare.VerificaScara(os, linie, erori);
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori.Distinct()));
    }

    // 36a, F13-D1: TVA-ul cules are sens numai pe regimurile care îl postează separat. Se verifică
    // înaintea recalculului, care l-ar șterge în tăcere.
    static void VerificaTvaCules(IObjectSpace os, Document doc, List<DocumentDetaliu> linii) {
        var erori = linii.Where(l => l.TvaCules)
            .Select(l => RefuzTvaCules(os, doc, l, doc.SemnulEAlOperarii() ? Math.Abs(l.ValoareTva) : l.ValoareTva))
            .Where(r => r != null).Distinct().ToList();
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));
    }

    static string RefuzTvaCules(IObjectSpace os, Document doc, DocumentDetaliu linie, decimal valoare) {
        if (!doc.CuTva() || valoare == 0m)
            return null;
        if (valoare < 0m && !(linie is ILinieCuAvans && doc.BazaLinie(os, linie) < 0m))
            return "Valoarea TVA negativă cere o linie de factură cu bază negativă.";
        if (decimal.Round(valoare, Scara.Bani) != valoare)
            return $"Valoarea TVA acceptă cel mult {Scara.Bani} zecimale.";
        var regim = linie.TipTvaId is Guid id ? os.GetObjectByKey<TipTva>(id)?.Regim : null;
        if (regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
            return "Valoarea TVA se completează manual doar pe un tip de TVA cu regim "
                + "Normal sau Taxare inversă — regimul liniei nu poartă TVA separat.";
        if (regim == RegimTva.TaxareInversa && TvaService.DirectiePentru(os, doc) == DirectieTva.Colectat)
            return $"Taxarea inversă pe livrare nu poartă TVA; linia are TVA {valoare:N2} — lăsați valoarea goală.";
        return null;
    }

    static List<Document> DocumenteAtinse(IObjectSpace os) =>
        os.ModifiedObjects.Cast<object>()
            .Select(o => o switch {
                Document d => d,
                DocumentDetaliu l => l.Document
                    ?? (l.DocumentId != Guid.Empty ? os.GetObjectByKey<Document>(l.DocumentId) : null),
                _ => null,
            })
            .Where(d => d != null && !os.IsObjectToDelete(d) && d.Stare == StareDocument.Draft)
            .Distinct()
            .ToList();

    // Se scrie și navigația: editorul de lookup din ecran nu vede FK-ul singur.
    static void PrecompleteazaTipMaterial(IObjectSpace os, DocumentDetaliu linie) {
        if (linie.TipMaterialId != Guid.Empty || linie.ProdusCules() is not Guid produsId)
            return;
        var tipId = os.GetObjectsQuery<Produs>()
            .Where(p => p.ID == produsId)
            .Select(p => p.TipMaterialId)
            .FirstOrDefault();
        if (tipId is Guid id && os.GetObjectByKey<TipMaterial>(id) is TipMaterial tip) {
            linie.TipMaterial = tip;
            linie.TipMaterialId = id;
        }
    }

    // 103h: implicitul folosește exigibilitatea, fără schimbarea alegerii existente.
    static bool AplicaTipTvaImplicit(IObjectSpace os, Document doc, DocumentDetaliu linie) {
        if (!doc.CuTva())
            return false;
        var tipDoc = MotorOperare.GasesteTipDocument(os, doc);
        var rezultat = ImpliciteService.TipTva(os, tipDoc.ID,
            ImpliciteService.PartenerulDocumentului(os, doc), linie.ProdusCules(),
            (doc as IDocumentFiscal)?.DataExigibilitate ?? doc.Data);
        if (rezultat.TipTvaId is not Guid id)
            return false;
        linie.TipTva = os.GetObjectByKey<TipTva>(id);
        linie.TipTvaId = id;
        return true;
    }
}
