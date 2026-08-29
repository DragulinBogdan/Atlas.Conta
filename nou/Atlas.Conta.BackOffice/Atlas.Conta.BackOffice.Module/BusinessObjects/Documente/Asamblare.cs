using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

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
// D18-D2: consumul care GOLEȘTE un lot ia tot soldul valoric rămas (motorul o
// face înainte de validare, deci invariantul se verifică pe valoarea FINALĂ a
// consumului). Produsele rămân la valoarea CULEASĂ (`PretEvaluare`): dacă
// operatorul le-a evaluat la `preț lot × cantitate` iar lotul consumat integral
// purta un rezidu de cenți, invariantul SEMNALEAZĂ (refuz cu ambele sume) — nu
// se ajustează tăcut nicio latură; cine culege alege valoarea produsului egală
// cu consumul (importul o face: `Aloca` prezice exact valoarea motorului).
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

    // F19-D3 (închide 53i pe ASM): lotul liniei de PRODUS se naște în gestiunea
    // în care se asamblează — PREDATORUL (46d: regulile de stoc lucrează pe
    // predator), nu primitorul (default-ul bazei). Laturile POT diferi („de
    // regulă aceeași", nu obligatoriu), iar `ValideazaOperare` cere explicit
    // `lot.GestiuneId == PredatorId` — fără override-ul ăsta lotul s-ar naște în
    // gestiunea greșită și operarea ar refuza, fără nicio cale de reparare din UI.
    public override Gestiune GestiuneLoturiCulese(DevExpress.ExpressApp.IObjectSpace os) =>
        PredatorId != Guid.Empty ? os.GetObjectByKey<Repartitor>(PredatorId) as Gestiune : null;

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.OfType<AsamblareDetaliu>()) {
            // Idempotent prin Abs (re-operarea după anulare nu dublează semnul).
            if (d.Directie == DirectieAsamblare.Consum) {
                d.Cantitate = -Math.Abs(d.Cantitate);
                d.Valoare = d.LotId != null
                    ? Scara.RotunjesteBani(d.Cantitate * os.GetObjectByKey<Lot>(d.LotId.Value).PretUnitar)
                    : 0m;
            }
            else if (d.Directie == DirectieAsamblare.Produs) {
                d.Cantitate = Math.Abs(d.Cantitate);
                d.Valoare = Scara.RotunjesteBani(d.Cantitate * (d.PretEvaluare ?? 0m));
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
        // Coerența Tip-linie ↔ PRODUSUL cules, gardul pe care îl au toate
        // tipurile care nasc loturi (LDI F6-F2, FCT, FCL, NIR) și care lipsea de
        // pe ASM (review M2). ASM o acoperea doar TRANZITIV, prin lotul deja
        // născut — iar mesajul de acolo trimite spre `Lot`, care pe linia de
        // produs e server-owned și READ-ONLY (`ASM_Linie_Produs_FaraLot`, plus
        // `LotId` ignorat deliberat în `AsamblareApply`): un refuz care arată
        // spre un câmp pe care operatorul nu-l poate atinge din niciun ecran.
        var idsProdus = Detalii.OfType<AsamblareDetaliu>()
            .Where(d => d.ProdusId != null).Select(d => d.ProdusId.Value).Distinct().ToList();
        var tipPerProdus = idsProdus.Count == 0
            ? new Dictionary<Guid, Guid?>()
            : os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => (Guid?)p.TipMaterialId);
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
            if (linie.ProdusId != null && tipPerProdus.TryGetValue(linie.ProdusId.Value, out var tipProdus)
                    && tipProdus != null && tipProdus != linie.TipMaterialId)
                erori.Add("Produsul liniei aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
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
            // Gardul de LOT rămâne al CONSUMULUI, unde lotul e chiar câmpul pe
            // care operatorul îl alege. Pe produs lotul e derivat din produs,
            // deci acolo vorbește gardul de mai sus — care numește un câmp
            // editabil (62f: un refuz care arată spre un câmp read-only e tot o
            // capcană, doar că zgomotoasă).
            if (linie.Directie != DirectieAsamblare.Produs
                    && tipPerLot.TryGetValue(linie.LotId.Value, out var tipLot)
                    && tipLot != null && tipLot != linie.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei — "
                    + "corectați Tipul sau lotul.");
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

// F19-D3 (închide restanța 53i pe ASM, oglinda exactă a lui F6-D2 pe LDI):
// linia de PRODUS a asamblării e o INTRARE de stoc — marfa nouă nu vine de
// nicăieri, deci linia își naște lotul, ca plusul de inventar. Produsul e
// mecanismul: `LoturiCulegereService` îl transformă în lot, în gestiunea în care
// se asamblează (hook-ul de mai sus). Pe CONSUM câmpurile astea sunt inerte —
// gardul `NasteLot` le scoate din joc, iar culegerea le golește (F6-D3).
// Interdicția din `Interfete.cs` („nu se declară pe ieșiri") rămâne neatinsă:
// linia de produs e intrare, linia de consum e cea care descarcă un lot existent.
[Appearance("ASM_Linie_Produs_FaraLot", AppearanceItemType.ViewItem, "Directie = 'Produs'",
    TargetItems = nameof(Lot), Enabled = false)]
[Appearance("ASM_Linie_Consum_FaraCulegere", AppearanceItemType.ViewItem, "Directie = 'Consum'",
    TargetItems = nameof(Produs) + ";" + nameof(PretEvaluare) + ";" + nameof(DataExpirare)
        + ";" + nameof(LotFabricatie), Enabled = false)]
public class AsamblareDetaliu : DocumentDetaliu, ILinieCuAtributeLot, ILinieCareNasteLot {
    // Rolul explicit al liniei — se materializează în semnul Cantitate-ii din
    // bază la operare; UI-ul culege cantitatea pozitivă (ca LDI).
    [XafDisplayName("Direcție")]
    public virtual DirectieAsamblare Directie { get; set; }

    // Identitatea liniei de PRODUS — oglinda lui `ListaDiferenteInventarDetaliu.
    // ProdusId` (F6-D2). Nullable în schemă (aceeași frunză poartă și liniile de
    // consum, unde marfa e a lotului descărcat); obligatoriu pe produs, prin
    // validarea de operare („Linia de produs își creează lotul la culegere
    // (alegeți produsul)").
    public virtual Guid? ProdusId { get; set; }
    // Catalog de produse (potențial mare).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Produs")]
    public virtual Produs Produs { get; set; }

    // F6-D3 aplicat pe ASM: doar produsul naște. Consumul descarcă un lot
    // EXISTENT, iar un produs rămas cules pe el ar naște lot-artefact pe draft.
    // Direcția nesetată (enum-ul n-are membru 0 — `Consum = 1`, `Produs = 2`, ca
    // `DirectieDiferenta`) cade tot pe `false`: o linie fără rol cules nu
    // inventează marfă. Flag de MECANISM, nu câmp: nu se mapează (n-ar avea
    // backing field oricum, dar atributul o spune explicit) și nu se arată —
    // altfel ar ieși coloană în grila liniei.
    [NotMapped, Browsable(false)]
    public bool NasteLot => Directie == DirectieAsamblare.Produs;

    // Valoarea alocată liniei de produs (lotul nou se naște cu ea); pe consum
    // nu se folosește — valoarea vine din prețul lotului descărcat.
    [XafDisplayName("Preț evaluare")]
    public virtual decimal? PretEvaluare { get; set; }
    // Atributele lotului produs, copiate de motor la finalizare.
    [XafDisplayName("Dată expirare")]
    public virtual DateOnly? DataExpirare { get; set; }
    [XafDisplayName("Lot fabricație")]
    public virtual string LotFabricatie { get; set; }
}
