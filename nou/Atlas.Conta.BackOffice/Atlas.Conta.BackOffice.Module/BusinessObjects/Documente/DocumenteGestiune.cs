using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Detaliul BCS/BTR = baza pură + validare (testul bazei §6); NIR are frunză
// proprie din DIM-2 (dimensiunile primite prin clona conexă din FCT).

// NIR (02): singura intrare în stoc a lanțului de cumpărare (+1 primitor).
// De regulă autogenerat din FacturaIntrare (conex) — liniile clonate referă
// loturile născute la culegerea facturii; NIR-ul cules manual își creează
// loturile pe propriile linii (CreeazaLot). Recepția CONTEAZĂ aici (3xx = 401,
// închiderea întrebării 00 §13.1) — factura postează doar liniile non-stoc.
[TipDetaliu(typeof(NirDetaliu))]
public class NIR : Document {
    // Rolul de STINS (F19-D16, review F4): recepția CONTEAZĂ pe NIR (26a) —
    // `3xx = 401` se postează aici, nu pe factură —, deci NIR-ul lasă un sold
    // CREDITOR pe contul furnizorului, iar furnizorul e chiar PREDATORUL lui.
    // Se stinge debitând: plata, sau jumătatea de debit a unei note de
    // compensare. Nu declară `CapacitateStingere`: NIR-ul nu stinge nimic.
    //
    // Declarația e IEȘIREA din fundătura găsită de review: fără ea, un NIR fără
    // factură (partenerul e pe latură, deci intră în `peLaturi`) în fața unei
    // note `401 = 4111` pe acel furnizor primea refuzul de ambiguitate — corect
    // ca principiu, dar nerezolvabil pe NICIO cale de apelant. Ieșirea e
    // MODELAREA (tipul își declară natura soldului), nu un câmp `Sens` în DTO
    // care ar lăsa apelantul să aleagă arbitrar jumătatea.
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Datorie;

    // Cele două cazuri ale recepției, cu o formulă fiecare (F5-D6):
    //  (a) lot STRĂIN (născut pe altă linie — cazul conex): valoarea vine din
    //      prețul finalizat al lotului, deci recepția parțială (operatorul scade
    //      cantitatea primită) se reevaluează corect;
    //  (b) lot PROPRIU (recepție manuală, fără factură): valoarea se
    //      materializează din prețul CULES pe linie — altfel un NIR manual s-ar
    //      opera cu Valoare 0, iar prețul lotului (Valoare/Cantitate, 26e) ar
    //      ieși tot 0. Prețul trăiește pe frunză (`NirDetaliu`); liniile de tip
    //      BAZĂ ale NIR-urilor istorice/importate n-au de unde-l lua și rămân cu
    //      valoarea lor (importul a scris-o deja).
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            if (lot.LinieIntrareId != d.ID)
                d.Valoare = Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar);
            else if (d is NirDetaliu nd)
                d.Valoare = Scara.RotunjesteBani(nd.PretUnitar * d.Cantitate);
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul NIR-ului trebuie să fie un partener (furnizor).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul NIR-ului trebuie să fie o gestiune.");
        // Natura Clasei per Tip prin PROIECȚIE (disciplina 25b): nicio navigație
        // lazy atinsă în enumerare.
        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);

        foreach (var d in Detalii) {
            if (d.LotId == null) {
                // F5-D7: pe recepția manuală lipsa lotului are o CAUZĂ pe care
                // operatorul o poate remedia — produsul necules. Mesajul generic
                // („referă un lot") îi spunea ce lipsește, nu ce să facă; rămâne
                // pentru restul cazurilor (linie de tip bază pe un NIR istoric,
                // sau lot nenăscut încă fiindcă latura primitoare nu e gestiune —
                // refuzat separat mai sus).
                if (d is ILinieCareNasteLot culege && culege.ProdusId == null
                        && naturi.GetValueOrDefault(d.TipMaterialId) == NaturaClasa.Stoc)
                    erori.Add("Liniile de stoc ale NIR-ului își creează lotul la culegere (alegeți produsul).");
                else
                    erori.Add("Fiecare linie de NIR referă un lot (recepția e pe lot — decizia 13).");
            }
            else {
                var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
                if (lot.GestiuneId != PrimitorId)
                    erori.Add("Lotul fiecărei linii aparține gestiunii primitoare.");
                // F5-D7b: linia care și-a NĂSCUT lotul (recepție manuală) cere un
                // preț. Fără el, `PregatesteOperare` materializează Valoare 0, iar
                // motorul finalizează lotul cu PretUnitar 0 (Valoare/Cantitate,
                // 26e): marfă cu valoare zero în stoc, pe care FIFO o propagă în
                // TOATE ieșirile ulterioare, iar corecția e posibilă doar cât timp
                // lotul n-are dependenți. Precedentul exact e plusul de inventar
                // (28e — „plus cu preț de evaluare pozitiv"). Liniile cu lot
                // STRĂIN (clona conexă) și cele de tip BAZĂ nu sunt atinse:
                // acolo prețul e al lotului, nu al liniei (F5-D6b).
                if (lot.LinieIntrareId == d.ID && d is NirDetaliu nd && nd.PretUnitar <= 0)
                    erori.Add("Prețul unitar al liniei de recepție trebuie să fie pozitiv "
                        + "(lotul se naște cu acest preț).");
            }
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea recepționată trebuie să fie pozitivă.");
        }

        // Identitatea dublă a liniei (Tip + Produs) trebuie să fie coerentă —
        // oglinda validării de pe FCT (GATE 53f) / FCL (review P2 defect 4): un
        // produs de alt Tip ar conta pe conturile Tipului greșit, iar lotul născut
        // de linie ar ajunge în registrul altui Tip decât cel postat. Totul pe
        // proiecții (25b).
        //
        // NU se adaugă refuzul „linia trebuie să fie de tipul derivat" (regula
        // FCT/FCL): NIR-urile istorice și cele importate poartă linii de tip BAZĂ,
        // iar `NirApply.Citeste` le arată deliberat — un refuz aici le-ar face
        // ne-anulabile și ne-stornabile.
        var idsProdus = Detalii.OfType<ILinieCareNasteLot>()
            .Where(d => d.ProdusId != null).Select(d => d.ProdusId.Value).Distinct().ToList();
        if (idsProdus.Count > 0) {
            var tipPerProdus = os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.TipMaterialId })
                .ToDictionary(p => p.ID, p => p.TipMaterialId);
            foreach (var d in Detalii.OfType<NirDetaliu>())
                if (d.ProdusId != null && tipPerProdus.TryGetValue(d.ProdusId.Value, out var tipProdus)
                        && tipProdus != null && tipProdus != d.TipMaterialId)
                    erori.Add("Produsul liniei aparține altui Tip decât Tipul liniei — corectați Tipul sau produsul.");
        }
    }
}

// DIM-2 (decizia 54e, inventar §2): NIR primește prin clona conexă tot ce
// culege FCT — frunza poartă reuniunea FCT, culegibilă și pe NIR manual (Î3):
// fără ea, NIR-ul manual n-ar putea satisface defalcarea conturilor 3xx.
//
// F5-D1: frunza capătă și capătul de CULEGERE al recepției manuale (marfa
// intrată pe aviz, factura vine ulterior): produsul care naște lotul, prețul
// de recepție și atributele lui. Pe NIR-ul CONEX câmpurile astea rămân goale —
// clona din FCT aduce lotul deja născut pe linia facturii (lot STRĂIN), iar
// valoarea vine din prețul lui; recepția conexă nu-și alege marfa, o
// moștenește (F5-D4).
public class NirDetaliu : DocumentDetaliu, ILinieCuAtributeLot, ILinieCareNasteLot {
    // F5-D1/F5-D2: identitatea liniei de stoc pe recepția manuală — oglinda lui
    // FacturaIntrareDetaliu.ProdusId (GATE XAF D1). Nullable în schemă (aceeași
    // frunză poartă și liniile clonei conexe, unde produsul e al lotului);
    // obligatoriu pe liniile de stoc fără lot, prin validare.
    public virtual Guid? ProdusId { get; set; }
    // Catalog de produse (potențial mare).
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Produs Produs { get; set; }

    // Prețul de recepție cules de pe hârtia furnizorului: `Valoare` rămâne
    // REZULTAT (GATE 53c), materializat din `PretUnitar × Cantitate` — la
    // culegere pe calea API și la operare în `NIR.PregatesteOperare` (F5-D6a).
    // Scara PREȚ (18,6 — decizia 49e), pe numele proprietății.
    // IGNORAT pe liniile cu lot străin: acolo prețul e al lotului (F5-D6b).
    [XafDisplayName("Preț unitar")]
    public virtual decimal PretUnitar { get; set; }

    // Atribute de lot culese la intrare; motorul le copiază pe Lot la operare.
    [XafDisplayName("Dată expirare")]
    public virtual DateOnly? DataExpirare { get; set; }
    [XafDisplayName("Lot fabricație")]
    public virtual string LotFabricatie { get; set; }

    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public virtual Guid? SursaFinantareId { get; set; }
    [XafDisplayName("Sursă de finanțare")]
    public virtual SursaFinantare SursaFinantare { get; set; }

    public virtual Guid? CodFunctionalId { get; set; }
    [XafDisplayName("Cod funcțional")]
    public virtual CodFunctional CodFunctional { get; set; }

    public virtual Guid? ProiectId { get; set; }
    [XafDisplayName("Proiect")]
    public virtual Proiect Proiect { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() {
        CodEconomicId = CodEconomicId, SursaFinantareId = SursaFinantareId,
        CodFunctionalId = CodFunctionalId, ProiectId = ProiectId
    };
    public override void PreiaDimensiuni(Dimensiuni s) {
        CodEconomicId = s.CodEconomicId; SursaFinantareId = s.SursaFinantareId;
        CodFunctionalId = s.CodFunctionalId; ProiectId = s.ProiectId;
    }
}

// BCS (03): −magazie (predator) / +consum (primitor) — consumul nu „dispare",
// alimentează DOUĂ registre simultan (rămâne pe responsabilul locului de
// consum). Valoarea vine din lot (prețul nu se culege). Lotul NU e legat de
// gestiunea predatoare prin schemă — locația curentă e soldul din registru,
// iar gardianul de sold intermediar refuză consumul de unde lotul nu există.
// `preț × cantitate` de aici e valoarea IMPLICITĂ a ieșirii: pe linia care
// golește cheia de stoc motorul o înlocuiește cu tot soldul valoric rămas
// (D18-D2, `StocService.AplicaValoareIesire`) — valabil pentru frunzele cu
// ieșiri de EVALUARE (BCS, BTR, DSC, LDI−, ASM consum); RLF declară
// `IDocumentCuIesireFiscala` și rămâne la `preț × cantitate` (review F5).
public class BonConsum : Document {
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            d.Valoare = Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar);
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune)
            erori.Add("Predatorul bonului de consum trebuie să fie o gestiune.");
        // Locul de consum e calitate transversală, nu clasă (decizia 16) —
        // orice repartitor intern o poate purta (unitate, gestiune, angajat).
        var primitor = os.GetObjectByKey<Repartitor>(PrimitorId);
        if (primitor is Partener || !primitor.Calitati.HasFlag(CalitateRepartitor.LocConsum))
            erori.Add("Primitorul trebuie să fie un loc de consum intern (calitatea LocConsum).");
        foreach (var d in Detalii) {
            if (d.LotId == null)
                erori.Add("Fiecare linie de consum referă un lot (descărcarea e pe lot — decizia 13).");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea consumată trebuie să fie pozitivă.");
        }
    }
}

// BTR (04): −predator/+primitor pe același tip de stoc; lotul își schimbă
// gestiunea, prețul rămâne al lotului. Primul vertical slice al motorului.
public class NotaTransfer : Document, IDocumentCuPV {
    [XafDisplayName("Număr PV")]
    public virtual string NumarPV { get; set; }
    [XafDisplayName("Dată PV")]
    public virtual DateOnly? DataPV { get; set; }

    // Valoarea liniei = prețul lotului × cantitate (04: formulă fără re-aplicare
    // de TVA — prețul lotului e deja valoarea de registru per unitate). Linia
    // care golește lotul în sursă ia tot soldul valoric rămas (D18-D2, în
    // motor) — valoarea e comună ambelor laturi, deci restul se MUTĂ pe destinație.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            d.Valoare = Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar);
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune
            || os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Transferul se face între două gestiuni.");
        else if (PredatorId == PrimitorId)
            erori.Add("Gestiunea sursă și cea destinație trebuie să difere.");
        foreach (var d in Detalii) {
            if (d.LotId == null)
                erori.Add("Fiecare linie de transfer referă un lot.");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea transferată trebuie să fie pozitivă.");
        }
    }
}
