using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// LDI (05): plusuri și minusuri pe aceeași listă; singurul tip bidirecțional pe
// loturi — minusul descarcă un lot existent (ca un consum), plusul CREEAZĂ lot
// nou (ca un NIR manual) cu preț de evaluare cules. Predator = gestiunea
// inventariată (singura latură care mișcă stoc: +1 cu cantitate semnată);
// primitor = comisia de inventariere (calitatea Comisie — decizia 16).
[TipDetaliu(typeof(ListaDiferenteInventarDetaliu))]
public class ListaDiferenteInventar : Document {
    // F6-D2: lotul plusului se naște în gestiunea INVENTARIATĂ — predatorul
    // (28d), nu primitorul (default-ul bazei). Primitorul LDI e comisia de
    // inventariere, care nu e `Gestiune`, deci fără override-ul ăsta serviciul
    // de culegere ar tăcea pentru totdeauna (skip grațios pe gestiune lipsă).
    public override Gestiune GestiuneLoturiCulese(DevExpress.ExpressApp.IObjectSpace os) =>
        PredatorId != Guid.Empty ? os.GetObjectByKey<Repartitor>(PredatorId) as Gestiune : null;

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<ListaDiferenteInventarDetaliu>()) {
            // Direcția explicită se materializează în semn — UI-ul culege
            // cantitatea pozitivă, limbajul motorului e semn × cantitate.
            var semn = d.Directie == DirectieDiferenta.Minus ? -1 : +1;
            d.Cantitate = Math.Abs(d.Cantitate) * semn;
            // Valoarea poartă același semn ca și cantitatea (registrele o iau
            // ca atare); minusul se evaluează la prețul lotului descărcat,
            // plusul la prețul de evaluare cules (lotul nou se naște cu el).
            if (d.Directie == DirectieDiferenta.Minus && d.LotId != null)
                d.Valoare = Scara.RotunjesteBani(d.Cantitate * os.GetObjectByKey<Lot>(d.LotId.Value).PretUnitar);
            else if (d.Directie == DirectieDiferenta.Plus)
                d.Valoare = Scara.RotunjesteBani(d.Cantitate * (d.PretEvaluare ?? 0m));
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune)
            erori.Add("Predatorul listei de diferențe este gestiunea inventariată.");
        // Comisia e calitate transversală, nu clasă (decizia 16) — orice
        // repartitor intern o poate purta.
        var primitor = os.GetObjectByKey<Repartitor>(PrimitorId);
        if (primitor is Partener || !primitor.Calitati.HasFlag(CalitateRepartitor.Comisie))
            erori.Add("Primitorul trebuie să fie comisia de inventariere (calitatea Comisie).");
        // Review advers F6-F1 (oglinda gardului ASM, 46d): minusul care descarcă
        // lotul născut de o linie-FRATE ar intra cu preț nefinalizat (0) —
        // gardianul de sold ar trece (aceeași cheie, aceeași zi), iar consumul
        // restului la prețul finalizat ar lăsa valoare orfană pe cantitate 0.
        var idsLiniiProprii = Detalii.Select(x => x.ID).ToHashSet();
        // Review advers F6-F2: coerența Tip↔Produs pe plusul care naște lot —
        // invariantul 50a („Tip linie = Tip produs lot") se păzește la NAȘTERE,
        // ca pe toate intrările (FCT/NIR/ASM); proiecție, disciplina 25b.
        var idsProdus = Detalii.OfType<ILinieCareNasteLot>()
            .Where(x => x.ProdusId != null).Select(x => x.ProdusId.Value).Distinct().ToList();
        var tipPerProdus = idsProdus.Count == 0
            ? new Dictionary<Guid, Guid?>()
            : os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => (Guid?)p.TipMaterialId);
        foreach (var d in Detalii.OfType<ListaDiferenteInventarDetaliu>()) {
            if (d.Directie is not (DirectieDiferenta.Plus or DirectieDiferenta.Minus))
                erori.Add("Fiecare linie de diferență poartă direcția (plus sau minus).");
            if (d.Cantitate == 0)
                erori.Add("Cantitatea diferenței nu poate fi zero.");
            if (d.ProdusId != null && tipPerProdus.TryGetValue(d.ProdusId.Value, out var tipProdus)
                    && tipProdus != null && tipProdus != d.TipMaterialId)
                erori.Add("Produsul liniei aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
            if (d.LotId == null) {
                erori.Add(d.Directie == DirectieDiferenta.Plus
                    ? "Linia de plus își creează lotul la culegere (alegeți produsul)."
                    : "Linia de minus descarcă un lot existent.");
                continue;
            }
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            if (d.Directie == DirectieDiferenta.Plus) {
                if (lot.LinieIntrareId != d.ID)
                    erori.Add("Lotul unei linii de plus se naște pe linia însăși, nu se refolosește.");
                if (lot.GestiuneId != PredatorId)
                    erori.Add("Lotul plusului aparține gestiunii inventariate.");
                if ((d.PretEvaluare ?? 0m) <= 0m)
                    erori.Add("Plusul de inventar cere preț de evaluare pozitiv.");
            }
            else if (d.Directie == DirectieDiferenta.Minus && lot.LinieIntrareId != null
                    && idsLiniiProprii.Contains(lot.LinieIntrareId.Value))
                erori.Add("Linia de minus descarcă un lot existent, nu unul creat de acest document (lotul plusului nu are preț până la operare).");
        }
    }
}

// F6-D2 (închide restanța 53i pe LDI+): plusul de inventar e o INTRARE culeasă
// manual — marfa găsită în plus la inventariere nu vine de nicăieri, deci linia
// își naște lotul, exact ca recepția fără factură (F5 pe NIR). Produsul e
// mecanismul: `LoturiCulegereService` îl transformă în lot, în gestiunea
// inventariată (hook-ul de mai sus). Pe MINUS câmpurile astea sunt inerte —
// gardul `NasteLot` le scoate din joc, iar culegerea le golește (F6-D3).
[Appearance("LDI_Linie_Plus_FaraLot", AppearanceItemType.ViewItem, "Directie = 'Plus'",
    TargetItems = nameof(Lot), Enabled = false)]
[Appearance("LDI_Linie_Minus_FaraCulegere", AppearanceItemType.ViewItem, "Directie = 'Minus'",
    TargetItems = nameof(Produs) + ";" + nameof(PretEvaluare) + ";" + nameof(DataExpirare)
        + ";" + nameof(LotFabricatie), Enabled = false)]
public class ListaDiferenteInventarDetaliu : DocumentDetaliu, ILinieCuAtributeLot, ILinieCareNasteLot {
    // Direcția explicită (testul bazei §4) — se materializează în semnul
    // Cantitate-ii din bază la operare; UI-ul culege cantitatea pozitivă.
    [XafDisplayName("Direcție")]
    public virtual DirectieDiferenta Directie { get; set; }

    // Identitatea liniei de PLUS — oglinda lui `NirDetaliu.ProdusId` (F5-D1).
    // Nullable în schemă (aceeași frunză poartă și liniile de minus, unde marfa
    // e a lotului descărcat); obligatoriu pe plus, prin validarea de operare
    // („Linia de plus își creează lotul la culegere (alegeți produsul)").
    public virtual Guid? ProdusId { get; set; }
    // Catalog de produse (potențial mare).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Produs")]
    public virtual Produs Produs { get; set; }

    // F6-D3: doar plusul naște. Direcția nesetată (enum-ul n-are default valid —
    // 28e) cade tot pe `false`: o linie fără direcție culeasă nu inventează marfă.
    // Flag de MECANISM, nu câmp: nu se mapează (n-ar avea backing field oricum,
    // dar atributul o spune explicit) și nu se arată — altfel ar ieși coloană în
    // grila liniei.
    [NotMapped, Browsable(false)]
    public bool NasteLot => Directie == DirectieDiferenta.Plus;

    // Prețul de evaluare cules la plus (lotul nou se naște cu el).
    [XafDisplayName("Preț evaluare")]
    public virtual decimal? PretEvaluare { get; set; }
    // Atribute de lot culese pe plus (inventar 05); motorul le copiază pe Lot.
    [XafDisplayName("Dată expirare")]
    public virtual DateOnly? DataExpirare { get; set; }
    [XafDisplayName("Lot fabricație")]
    public virtual string LotFabricatie { get; set; }

    // DIM-2 (decizia 54c, inventar §2): plusul de inventar (791/7588) cere E.
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() { CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
