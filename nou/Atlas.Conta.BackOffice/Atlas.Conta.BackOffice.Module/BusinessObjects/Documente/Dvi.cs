using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// DVI (decizia 86): declarația vamală de import. Liniile stau pe
// `DocumentDetaliu` de bază (precedentul NIR/BCS) — `Valoare` = valoarea în
// vamă, `ValoareTva` = taxa declarată; nimic nu postează valoarea, doar taxa.
[TipDetaliu(typeof(DocumentDetaliu))]
[XafDisplayName("Declarație vamală de import")]
public class Dvi : Document {
    // DVI-D4: taxa în vamă se plătește ca orice taxă (precedentul ITV/4423),
    // nu prin imperechere — soldul lui 446 pe biroul vamal e ce rămâne de plătit.
    public override bool PoateFiStins(DevExpress.ExpressApp.IObjectSpace os) => false;

    // Creditul notei de TVA e contul de taxă al PREDATORULUI (446 al biroului
    // vamal), deci dimensiunea Repartitor îl urmărește pe el, nu unitatea
    // primitoare — precedentul `Decont`/`DescarcareGestiune` (00 §5).
    public override Guid RepartitorImplicitCredit(DevExpress.ExpressApp.IObjectSpace os) => PredatorId;

    [DevExpress.ExpressApp.DC.Aggregated]
    [XafDisplayName("Facturi de import")]
    public virtual ObservableCollection<DviFactura> Facturi { get; set; } = new();

    // Cele două cifre ale declarației, în locul lui `Total (brut)` (ascuns pe
    // view-urile DVI prin baseline): brutul ar aduna baza cu taxa, iar suma n-ar
    // fi nici valoarea vămuită, nici ce se datorează.
    [NotMapped]
    [XafDisplayName("Valoare în vamă")]
    [VisibleInListView(false), VisibleInLookupListView(false)]
    public virtual decimal Baza => Detalii.Sum(d => d.Valoare);

    [NotMapped]
    [XafDisplayName("TVA în vamă")]
    [VisibleInListView(false), VisibleInLookupListView(false)]
    public virtual decimal Taxa => Detalii.Sum(d => d.ValoareTva);

    // 48b: baza e CULEASĂ (valoarea în vamă, nu preț × cantitate), taxa culeasă
    // se păstrează, iar una lăsată la 0 se calculează din cotă.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        var directie = Motor.TvaService.DirectiePentru(os, this);
        foreach (var d in Detalii)
            Motor.TvaService.CalculeazaValori(d, d.Valoare, tipuri, directie, pastreazaTvaCules: true);
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (string.IsNullOrWhiteSpace(Numar))
            erori.Add("Declarația vamală poartă numărul ei (MRN) — se completează la culegere.");
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul declarației vamale trebuie să fie un partener "
                + "(biroul vamal, sau comisionarul care a plătit taxa în vamă).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not UnitateInterna)
            erori.Add("Primitorul declarației vamale trebuie să fie o unitate internă.");

        var idsTipTva = Detalii.Where(d => d.TipTvaId != null)
            .Select(d => d.TipTvaId.Value).Distinct().ToList();
        var tipuri = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTipTva.Contains(t.ID))
            .Select(t => new { t.ID, t.Cod, t.DeImport, t.Cota })
            .ToList()
            .ToDictionary(t => t.ID, t => (t.Cod, t.DeImport, t.Cota));
        foreach (var d in Detalii) {
            if (d.Valoare <= 0)
                erori.Add("Valoarea în vamă a fiecărei linii trebuie să fie pozitivă.");
            if (d.TipTvaId == null || !tipuri.TryGetValue(d.TipTvaId.Value, out var tip)) {
                erori.Add("Fiecare linie a declarației vamale poartă un tip de TVA de import.");
                continue;
            }
            if (!tip.DeImport)
                erori.Add($"Tipul de TVA {tip.Cod} nu e de import — liniile declarației vamale "
                    + "poartă tipurile de import ale profilului.");
            else if (tip.Cota <= 0)
                erori.Add($"TVA-ul în vamă are cotă, iar tipul {tip.Cod} are cota {tip.Cota} — "
                    + "alegeți tipul de import cu cota declarației.");
        }

        var id = ID;
        var idsFacturi = os.GetObjectsQuery<DviFactura>()
            .Where(f => f.DviId == id).Select(f => f.FacturaId).ToList();
        if (idsFacturi.Count == 0)
            return;
        var neoperate = os.GetObjectsQuery<FacturaIntrare>()
            .Where(f => idsFacturi.Contains(f.ID) && f.Stare != StareDocument.Operat)
            .Select(f => new { f.Numar, f.Stare })
            .ToList();
        foreach (var f in neoperate)
            erori.Add($"Factura de import {f.Numar} legată la declarație e în starea „{f.Stare}”, "
                + "nu Operat — dezlegați-o sau operați-o înainte de a opera declarația.");
    }
}

// DVI-D3: legătura n→m declarație ↔ facturi de import, pe forma `Imperechere`.
// E EVIDENȚĂ, nu sursa cifrelor: baza și taxa sunt cele declarate în vamă.
[XafDisplayName("Factură de import legată")]
public class DviFactura : BaseObject, IVerificabilLaCommit {
    public virtual Guid DviId { get; set; }
    [XafDisplayName("Declarație vamală")]
    public virtual Dvi Dvi { get; set; }
    public virtual Guid FacturaId { get; set; }
    [XafDisplayName("Factură de import")]
    public virtual FacturaIntrare Factura { get; set; }

    // DVI-D3: creare/ștergere doar cât declarația e Draft, fără editare, factura
    // `Operat` la creare, perechea unică cu mesaj de domeniu (nu `23505`).
    public void Verifica(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        var sters = os.IsObjectToDelete(this) || os.IsDeletedObject(this);
        if (!sters && !os.IsNewObject(this)) {
            erori.Add("Legătura dintre declarația vamală și factură nu se editează — "
                + "ștergeți-o și creați-o din nou.");
            return;
        }
        var dvi = Dvi ?? (DviId != Guid.Empty ? os.GetObjectByKey<Dvi>(DviId) : null);
        if (dvi == null) {
            erori.Add("Legătura cere declarația vamală.");
            return;
        }
        if (dvi.Stare != StareDocument.Draft) {
            erori.Add($"Facturile de import se leagă și se dezleagă doar cât declarația vamală "
                + $"e Draft (starea „{dvi.Stare}”) — anulați operarea sau stornați-o.");
            return;
        }
        if (sters)
            return;
        var factura = Factura ?? (FacturaId != Guid.Empty ? os.GetObjectByKey<FacturaIntrare>(FacturaId) : null);
        if (factura == null) {
            erori.Add("Legătura cere factura de intrare.");
            return;
        }
        if (factura.Stare != StareDocument.Operat)
            erori.Add($"Factura de import {factura.Numar} e în starea „{factura.Stare}” — "
                + "la declarația vamală se leagă doar facturi operate.");
        var id = ID;
        var dviId = dvi.ID;
        var facturaId = factura.ID;
        // Interogarea nu vede rândurile NOI ale aceluiași commit (grila XAF).
        var dublura = os.GetObjectsQuery<DviFactura>().Any(f => f.ID != id && f.DviId == dviId && f.FacturaId == facturaId)
            || os.ModifiedObjects.OfType<DviFactura>().Any(f => !ReferenceEquals(f, this)
                && os.IsNewObject(f) && !os.IsObjectToDelete(f)
                && (f.Dvi?.ID ?? f.DviId) == dviId && (f.Factura?.ID ?? f.FacturaId) == facturaId);
        if (dublura)
            erori.Add($"Factura de import {factura.Numar} e deja legată la această declarație vamală.");
    }
}
