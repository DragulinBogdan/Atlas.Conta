using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

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

    // Formula fixată în design §3, cu SENSUL adăugat de F13-D1:
    //   Capitalizat:            Valoare = net × (1 + Cota/100); ValoareTva = 0
    //   Normal:                 Valoare = net;                  ValoareTva = net × Cota/100
    //   TaxareInversa × Deductibil (achiziție): ca Normal — beneficiarul
    //                           autolichidează (4426 = 4427).
    //   TaxareInversa × Colectat (livrare):     Valoare = net;  ValoareTva = 0
    //   Scutit / Neimpozabil / TipTva null:     Valoare = net;  ValoareTva = 0
    //
    // F13-D1 (Cod fiscal art. 331): taxarea inversă NU e proprietatea liniei
    // singure, ci a perechii (regim × latura documentului). Furnizorul emite
    // factura FĂRĂ TVA, cu mențiunea „taxare inversă"; taxa o declară și o
    // deduce beneficiarul. Formula de dinainte era per REGIM și inventa pe
    // livrare o taxă pe care nimeni n-o datorează (D300 o ocolea printr-o
    // excepție, iar Import1C o compensa la sursă).
    //
    // Sensul vine din `PoliticaTva.Directie` a TIPULUI de document (36b),
    // politică-dată existentă — nu un câmp nou pe `TipTva` și nu un hook pe
    // frunză: un tip fără `PoliticaTva` nu postează TVA oricum, deci `directie`
    // null ⇒ comportamentul dinainte (nu se pierde nimic). Parametrul e
    // EXPLICIT, fără default: un apelant nou trebuie să se întrebe pe ce latură
    // se află, nu să moștenească tăcut achiziția.
    //
    // `pastreazaTvaCules` (FCT/FCL/DEC — regula 36a uniformizată prin decizia
    // 48b): un ValoareTva nenul CULES nu se suprascrie la operare. Documentul
    // real (factura furnizorului, factura emisă, bonul justificat) poartă
    // rotunjirea lui; recalculul din cotă ar diferi pe bani mărunți și ar rupe
    // atât reconcilierea de import, cât și e-Factura (36f). Pe TI × Colectat NU
    // se aplică — acolo nu există taxă de păstrat, iar un `ValoareTva` cules e
    // REFUZAT la operare (`VerificaTvaCulesTaxareInversa`), nu înghițit tăcut.
    public static void CalculeazaValori(DocumentDetaliu d, decimal net,
        IReadOnlyDictionary<Guid, InfoTva> tipuri, DirectieTva? directie,
        bool pastreazaTvaCules = false) {
        var info = d.TipTvaId != null ? tipuri.GetValueOrDefault(d.TipTvaId.Value) : default;
        switch (info.Regim) {
            case RegimTva.Capitalizat:
                d.Valoare = net * (1 + info.Cota / 100m);
                d.ValoareTva = 0m;
                break;
            // F13-D1: pe LIVRARE taxarea inversă se poartă ca un regim fără taxă.
            case RegimTva.TaxareInversa when directie == DirectieTva.Colectat:
                d.Valoare = net;
                d.ValoareTva = 0m;
                break;
            case RegimTva.Normal:
            case RegimTva.TaxareInversa:
                d.Valoare = net;
                if (!(pastreazaTvaCules && d.ValoareTva != 0m))
                    d.ValoareTva = net * info.Cota / 100m;
                break;
            default: // Scutit / Neimpozabil / fără TipTva
                d.Valoare = net;
                d.ValoareTva = 0m;
                break;
        }
        // Cele două valori de POSTARE se rotunjesc la bani (`Scara`) — inclusiv
        // TVA-ul cules pe calea `pastreazaTvaCules`, ca instanța din memorie să
        // fie exact ce ajunge în `numeric(18,2)`. `net` rămâne nerotunjit până
        // aici: rotunjirea se face o singură dată, pe rezultat.
        d.Valoare = Scara.RotunjesteBani(d.Valoare);
        d.ValoareTva = Scara.RotunjesteBani(d.ValoareTva);
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
