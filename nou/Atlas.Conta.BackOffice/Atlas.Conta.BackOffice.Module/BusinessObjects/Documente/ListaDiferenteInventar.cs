using Atlas.Conta.BackOffice.Module.UI;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// LDI (05): plusuri și minusuri pe aceeași listă; singurul tip bidirecțional pe
// loturi — minusul descarcă un lot existent (ca un consum), plusul CREEAZĂ lot
// nou (ca un NIR manual) cu preț de evaluare cules. Predator = gestiunea
// inventariată (singura latură care mișcă stoc: +1 cu cantitate semnată);
// primitor = comisia de inventariere (calitatea Comisie — decizia 16).
[TipDetaliu(typeof(ListaDiferenteInventarDetaliu))]
public class ListaDiferenteInventar : Document {
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
        foreach (var d in Detalii.OfType<ListaDiferenteInventarDetaliu>()) {
            if (d.Directie is not (DirectieDiferenta.Plus or DirectieDiferenta.Minus))
                erori.Add("Fiecare linie de diferență poartă direcția (plus sau minus).");
            if (d.Cantitate == 0)
                erori.Add("Cantitatea diferenței nu poate fi zero.");
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
            else if (d.Directie == DirectieDiferenta.Minus && lot.LinieIntrareId == d.ID)
                erori.Add("Linia de minus descarcă un lot existent, nu unul creat de ea.");
        }
    }
}

public class ListaDiferenteInventarDetaliu : DocumentDetaliu, ILinieCuAtributeLot {
    // Direcția explicită (testul bazei §4) — se materializează în semnul
    // Cantitate-ii din bază la operare; UI-ul culege cantitatea pozitivă.
    public virtual DirectieDiferenta Directie { get; set; }
    // Prețul de evaluare cules la plus (lotul nou se naște cu el).
    public virtual decimal? PretEvaluare { get; set; }
    // Atribute de lot culese pe plus (inventar 05); motorul le copiază pe Lot.
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }
}
