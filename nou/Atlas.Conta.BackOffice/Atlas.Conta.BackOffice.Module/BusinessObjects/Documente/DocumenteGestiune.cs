using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;

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
    // Liniile care referă un lot străin (născut pe altă linie — cazul conex) își
    // iau valoarea din prețul finalizat al lotului; liniile care și-au creat
    // propriul lot au Valoare culeasă (prețul lotului se derivă abia la operare).
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        foreach (var d in Detalii.Where(d => d.LotId != null)) {
            var lot = os.GetObjectByKey<Lot>(d.LotId.Value);
            if (lot.LinieIntrareId != d.ID)
                d.Valoare = Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar);
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul NIR-ului trebuie să fie un partener (furnizor).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul NIR-ului trebuie să fie o gestiune.");
        foreach (var d in Detalii) {
            if (d.LotId == null)
                erori.Add("Fiecare linie de NIR referă un lot (recepția e pe lot — decizia 13).");
            else if (os.GetObjectByKey<Lot>(d.LotId.Value).GestiuneId != PrimitorId)
                erori.Add("Lotul fiecărei linii aparține gestiunii primitoare.");
            if (d.Cantitate <= 0)
                erori.Add("Cantitatea recepționată trebuie să fie pozitivă.");
        }
    }
}

// DIM-2 (decizia 54e, inventar §2): NIR primește prin clona conexă tot ce
// culege FCT — frunza poartă reuniunea FCT, culegibilă și pe NIR manual (Î3):
// fără ea, NIR-ul manual n-ar putea satisface defalcarea conturilor 3xx.
public class NirDetaliu : DocumentDetaliu {
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
    public virtual string NumarPV { get; set; }
    public virtual DateOnly? DataPV { get; set; }

    // Valoarea liniei = prețul lotului × cantitate (04: formulă fără re-aplicare
    // de TVA — prețul lotului e deja valoarea de registru per unitate).
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
