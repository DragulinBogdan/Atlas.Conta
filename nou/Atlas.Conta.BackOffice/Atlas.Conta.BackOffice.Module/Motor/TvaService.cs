using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Motor;

// P1 (design §3): calculul TVA e helper comun, apelat din PregatesteOperare al
// derivatelor purtătoare de TVA (FacturaIntrare/FacturaIesire/Decont) —
// „RecalculeazaValoare unificat". Lucrează pe FK-uri + IObjectSpace (25b):
// tipurile de TVA se preîncarcă, nu se ating navigațiile.
public static class TvaService {
    public readonly record struct InfoTva(RegimTva Regim, decimal Cota);

    public static Dictionary<Guid, InfoTva> IncarcaTipuri(IObjectSpace os, IEnumerable<DocumentDetaliu> detalii) {
        var ids = detalii.Where(d => d.TipTvaId != null).Select(d => d.TipTvaId.Value).Distinct().ToList();
        return os.GetObjectsQuery<TipTva>()
            .Where(t => ids.Contains(t.ID))
            .Select(t => new { t.ID, t.Regim, t.Cota })
            .ToDictionary(t => t.ID, t => new InfoTva(t.Regim, t.Cota));
    }

    /// <summary>Valoarea liniei datorată terțului: taxa autolichidată (TaxareInversa) rămâne în afara decontării.</summary>
    public static decimal DatoratTertului(DocumentDetaliu d, IReadOnlyDictionary<Guid, InfoTva> tipuri) =>
        d.Valoare + (d.TipTvaId is Guid id && tipuri.TryGetValue(id, out var tip) && tip.Regim == RegimTva.TaxareInversa
            ? 0m : d.ValoareTva);

    /// <summary>
    /// Valoarea liniei și, pe tipul fără politică de TVA, taxa ei; cu politică, taxa nemarcată e a
    /// repartizării pe document (109a). Fiecare câmp se scrie o singură dată, rotunjit (109g).
    /// </summary>
    public static void CalculeazaValori(DocumentDetaliu d, decimal net,
        IReadOnlyDictionary<Guid, InfoTva> tipuri, DirectieTva? directie,
        bool pastreazaTvaCules = false) {
        var info = d.TipTvaId != null ? tipuri.GetValueOrDefault(d.TipTvaId.Value) : default;
        // F13-D1: pe livrare taxarea inversă se poartă ca un regim fără taxă.
        var poartaTaxa = info.Regim == RegimTva.Normal
            || info.Regim == RegimTva.TaxareInversa && directie != DirectieTva.Colectat;
        var valoare = info.Regim == RegimTva.Capitalizat ? net * (1 + info.Cota / 100m) : net;
        var taxa = !poartaTaxa ? 0m
            : directie == null && !(pastreazaTvaCules && d.TvaCules) ? net * info.Cota / 100m
            : d.ValoareTva;
        d.Valoare = Scara.RotunjesteBani(valoare);
        d.ValoareTva = Scara.RotunjesteBani(taxa);
    }

    /// <summary>
    /// Taxa nemarcată a documentului: decisă pe document × cotă din valorile liniilor și repartizată
    /// pe linii, în ordinea operării (109a). Fără politică de TVA rămâne taxa de linie.
    /// </summary>
    public static void RepartizeazaTaxa(IObjectSpace os, IEnumerable<DocumentDetaliu> linii, ContextTva tva) {
        if (tva.Directie is not DirectieTva directie)
            return;
        var db = (BackOfficeEFCoreDbContext)((EFCoreObjectSpace)os).DbContext;
        var fiscale = db.InOrdineaPozitiilor(linii.Where(l => l.TipTvaId is Guid id && tva.Tipuri.ContainsKey(id)))
            .Select(l => (Linie: l, Cheie: Guid.NewGuid(), Tip: tva.Tipuri[l.TipTvaId.Value]))
            .ToList();
        if (fiscale.All(f => f.Linie.TvaCules))
            return;
        var taxa = N.Tva.PeDocument(
            [.. fiscale.Select(f => new N.LinieTva(f.Cheie, f.Linie.Valoare, (N.RegimTva)(int)f.Tip.Regim, f.Tip.Cota))],
            (N.DirectieTva)(int)directie, new N.Rotunjire(Scara.ConventieBani));
        foreach (var f in fiscale.Where(f => !f.Linie.TvaCules))
            f.Linie.ValoareTva = taxa.PerLinie[f.Cheie];
    }

    // Latura fiscală a tipului de document (36b): `Deductibil` = achiziție,
    // `Colectat` = livrare, `null` = tipul nu e eveniment de TVA în profilul
    // ăsta (nicio `PoliticaTva`) — caz în care motorul nu postează TVA oricum.
    // O interogare peste politică; în bucle se cheamă o dată per document.
    public static DirectieTva? DirectiePentru(IObjectSpace os, Document doc) =>
        DirectiePentruTip(os, MotorOperare.GasesteTipDocument(os, doc).ID);

    public static DirectieTva? DirectiePentruTip(IObjectSpace os, Guid tipDocumentId) =>
        os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDocumentId)?.Directie;

    // F13-D1, partea care STRIGĂ (62f: „un gard care tace devine capcană"):
    // pe un document de LIVRARE, o linie cu regim de taxare inversă și
    // `ValoareTva` CULES nenul e un refuz de domeniu, nu o valoare de aruncat.
    // `CalculeazaValori` o aduce la 0 — corect fiscal, dar dacă operatorul a
    // scris o sumă acolo, ori linia are alt regim decât crede el, ori suma e
    // greșită; ambele merită spuse.
    //
    // Se cheamă din motor (`MotorOperare.CalculeazaSiValideaza`), UN singur loc
    // prin care trec și calea XAF, și cea de API: valorile culese se capturează
    // ÎNAINTE de `PregatesteOperare` (care le zerorizează), iar tipul de TVA se
    // citește DUPĂ (pregătirea poate să-l golească deliberat — RDC își șterge
    // identitatea fiscală pe liniile de cost, felia 11).
    internal static void VerificaTvaCulesTaxareInversa(IObjectSpace os, TipDocument tipDoc,
        IReadOnlyList<(DocumentDetaliu Linie, int Pozitie, decimal TvaCules)> culese,
        ICollection<string> erori) {
        if (DirectiePentruTip(os, tipDoc.ID) != DirectieTva.Colectat)
            return;
        var candidate = culese.Where(c => c.TvaCules != 0m && c.Linie.TipTvaId != null).ToList();
        if (candidate.Count == 0)
            return;
        var tipuri = IncarcaTipuri(os, candidate.Select(c => c.Linie));
        foreach (var c in candidate)
            if (tipuri.GetValueOrDefault(c.Linie.TipTvaId.Value).Regim == RegimTva.TaxareInversa)
                erori.Add("Taxarea inversă pe livrare nu poartă TVA; "
                    // „poartă", nu „are cules": pe o factură operată ÎNAINTE de F13
                    // valoarea a pus-o vechiul motor, nu operatorul — de la gard nu se
                    // distinge, iar mesajul nu trebuie să acuze (review F13, defect 2).
                    + $"linia {c.Pozitie} poartă TVA {c.TvaCules:N2} — goliți-o înainte de operare.");
    }
}

/// <summary>Tipurile de TVA ale liniilor și latura fiscală a documentului, citite o dată per calcul.</summary>
public sealed record ContextTva(IReadOnlyDictionary<Guid, TvaService.InfoTva> Tipuri, DirectieTva? Directie);
