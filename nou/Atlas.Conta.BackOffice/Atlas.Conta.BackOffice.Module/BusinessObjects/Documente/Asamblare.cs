using Atlas.Conta.BackOffice.Module.UI;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// ASM (design FAZA 1C §7): asamblarea/kitting-ul — n consumuri → m produse, tot
// pe stoc, într-o gestiune. Tip SEPARAT: BPR (RaportProductie) rămâne REZERVAT
// (decizia 19 neatinsă — producția cu rețetar și chei de distribuție e alt
// subiect; forțarea semanticii ar polua clasa rezervată).
//
// Mecanica e a LDI-ului bidirecțional (28a): direcția e explicită pe linie și se
// materializează în SEMN la operare (consum −, produs +), iar regulile de stoc
// sunt un singur set +1 pe predator — semnul liniei dă direcția. Consumurile
// descarcă loturi EXISTENTE (valoare = preț lot × cantitate, pattern BCS/BTR);
// liniile de produs CREEAZĂ lotul la culegere (CreeazaLot), cu valoare explicită
// (`PretEvaluare`, ca plusul de inventar) — motorul îi fixează prețul la operare.
//
// Invariantul valoric (Σ produse = Σ consumuri) ține valoarea în patrimoniu:
// asamblarea nu creează și nu distruge valoare, doar o mută între loturi.
// Alocarea AUTOMATĂ pe rețetar rămâne amânată (BPR) — valorile se culeg sau vin
// din import.
//
// FĂRĂ reguli de contare: la plan sintetic marfă→marfă (371=371) e zgomot
// (raționamentul 23c, ca la NotaTransfer); mișcarea reală trăiește în registrul
// de stoc, pe loturi. Producția reală (345=711) primește reguli când apare
// cerința — aditiv, ca date.
//
// Dezasamblarea e ACELAȘI tip (1 consum → n produse), fără flag: rolul liniilor
// e tot ce diferă.
[TipDetaliu(typeof(AsamblareDetaliu))]
public class Asamblare : Document {
    // Toleranța invariantului = toleranța de reconciliere a fazei (design §9).
    const decimal Toleranta = 0.005m;

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<AsamblareDetaliu>()) {
            // Idempotent prin Abs (re-operarea după anulare nu dublează semnul).
            if (d.Directie == DirectieAsamblare.Consum) {
                d.Cantitate = -Math.Abs(d.Cantitate);
                d.Valoare = d.LotId != null
                    ? d.Cantitate * os.GetObjectByKey<Lot>(d.LotId.Value).PretUnitar
                    : 0m;
            }
            else if (d.Directie == DirectieAsamblare.Produs) {
                d.Cantitate = Math.Abs(d.Cantitate);
                d.Valoare = d.Cantitate * (d.PretEvaluare ?? 0m);
            }
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        // Asamblarea trăiește într-o gestiune (regulile de stoc lucrează pe
        // predator); laturile POT fi identice — nu se validează egalitatea.
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune)
            erori.Add("Predatorul asamblării este gestiunea în care se lucrează.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul asamblării este o gestiune (de regulă aceeași cu predatorul).");

        // Coerența Tip-linie ↔ produsul lotului, proiecție server-side (fără
        // navigații lazy în enumerare — 25b, ca pe DSC).
        var idsLot = Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var tipPerLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.Produs.TipMaterialId })
            .ToDictionary(l => l.ID, l => l.TipMaterialId);
        // Consumul unui lot produs de ACELAȘI document ar intra cu preț
        // nefinalizat (0) — invariantul ar fi satisfiabil cu valoare orfană în
        // registrul de stoc (review advers 1C-a): lanțul de kitting se face în
        // documente separate, în ordinea operării.
        var idsLiniiProprii = Detalii.Select(d => d.ID).ToHashSet();

        foreach (var d in Detalii) {
            if (d is not AsamblareDetaliu linie) {
                erori.Add("Linia asamblării trebuie culeasă ca linie de asamblare, nu ca detaliu generic.");
                continue;
            }
            if (linie.Directie is not (DirectieAsamblare.Consum or DirectieAsamblare.Produs)) {
                erori.Add("Fiecare linie de asamblare poartă rolul ei (consum sau produs).");
                continue;
            }
            if (linie.Cantitate == 0)
                erori.Add("Cantitatea liniei de asamblare nu poate fi zero.");
            if (linie.LotId == null) {
                erori.Add(linie.Directie == DirectieAsamblare.Produs
                    ? "Linia de produs își creează lotul la culegere (alegeți produsul)."
                    : "Linia de consum descarcă un lot existent.");
                continue;
            }
            var lot = os.GetObjectByKey<Lot>(linie.LotId.Value);
            if (linie.Directie == DirectieAsamblare.Produs) {
                if (lot.LinieIntrareId != linie.ID)
                    erori.Add("Lotul unei linii de produs se naște pe linia însăși, nu se refolosește.");
                if (lot.GestiuneId != PredatorId)
                    erori.Add("Lotul produsului aparține gestiunii în care se asamblează.");
                if ((linie.PretEvaluare ?? 0m) <= 0m)
                    erori.Add("Linia de produs cere preț de evaluare pozitiv (valoarea nu se derivă din rețetar).");
            }
            else if (lot.LinieIntrareId != null && idsLiniiProprii.Contains(lot.LinieIntrareId.Value))
                erori.Add("Linia de consum descarcă un lot existent, nu unul creat de acest document (lanțul de kitting = documente separate, operate în ordine).");
            if (tipPerLot.TryGetValue(linie.LotId.Value, out var tipLot) && tipLot != null && tipLot != linie.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei.");
        }

        // Invariantul alocării (§7): valoarea produsă = valoarea consumată.
        // Rulează DUPĂ semnare (motorul cheamă PregatesteOperare înaintea
        // validării), deci consumurile sunt negative și suma trebuie să dea 0.
        var linii = Detalii.OfType<AsamblareDetaliu>().ToList();
        var sumaProduse = linii.Where(d => d.Directie == DirectieAsamblare.Produs).Sum(d => d.Valoare);
        var sumaConsumuri = linii.Where(d => d.Directie == DirectieAsamblare.Consum).Sum(d => d.Valoare);
        if (Math.Abs(sumaProduse + sumaConsumuri) > Toleranta)
            erori.Add($"Valoarea produsă ({sumaProduse}) trebuie să fie egală cu valoarea consumată ({-sumaConsumuri}) — asamblarea nu creează și nu distruge valoare.");
    }
}

public class AsamblareDetaliu : DocumentDetaliu, ILinieCuAtributeLot {
    // Rolul explicit al liniei — se materializează în semnul Cantitate-ii din
    // bază la operare; UI-ul culege cantitatea pozitivă (ca LDI).
    public virtual DirectieAsamblare Directie { get; set; }
    // Valoarea alocată liniei de produs (lotul nou se naște cu ea); pe consum
    // nu se folosește — valoarea vine din prețul lotului descărcat.
    public virtual decimal? PretEvaluare { get; set; }
    // Atributele lotului produs, copiate de motor la finalizare.
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }
}
