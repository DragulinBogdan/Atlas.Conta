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

    // Formula fixată în design §3:
    //   Capitalizat:            Valoare = net × (1 + Cota/100); ValoareTva = 0
    //   Normal / TaxareInversa: Valoare = net;                  ValoareTva = net × Cota/100
    //   Scutit / Neimpozabil / TipTva null: Valoare = net;      ValoareTva = 0
    // `pastreazaTvaCules` (doar FCT): factura furnizorului bate rotunjirea
    // noastră — un ValoareTva nenul cules manual nu se suprascrie la operare.
    public static void CalculeazaValori(DocumentDetaliu d, decimal net,
        IReadOnlyDictionary<Guid, InfoTva> tipuri, bool pastreazaTvaCules = false) {
        var info = d.TipTvaId != null ? tipuri.GetValueOrDefault(d.TipTvaId.Value) : default;
        switch (info.Regim) {
            case RegimTva.Capitalizat:
                d.Valoare = net * (1 + info.Cota / 100m);
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
    }

    // Datoria P1 (design §8): default TipTva per tip de document, aplicat la
    // CULEGERE (nu în motor) — TipDocument.TipTvaImplicit. No-op dacă linia are
    // deja un TipTva cules (culegerea explicită bate default-ul). Apelantul:
    // controllerul XAF de creare a liniei (pasul 5); ModelCheck o exersează direct.
    public static void AplicaTipTvaImplicit(IObjectSpace os, Document doc, DocumentDetaliu linie) {
        if (linie.TipTvaId != null)
            return;
        var tip = MotorOperare.GasesteTipDocument(os, doc);
        if (tip.TipTvaImplicitId != null)
            linie.TipTvaId = tip.TipTvaImplicitId;
    }
}
