using System.ComponentModel.DataAnnotations.Schema;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// RLF / RDC (design FAZA 1C §7 + „Rezoluția spike-ului storno"): retururile sunt
// cerință de PRODUS (fluxul de magazin), nu doar de import.
//
// REPREZENTAREA STORNO, fixată pentru ambele: valori NEGATIVE pe corespondența
// ORIGINALĂ (minus pe aceeași latură), FĂRĂ flag-ul `Storno` pe rânduri —
//  * e deja convenția motorului (`Storneaza` păstrează conturile și neagă
//    valoarea — 25d) și reprezentarea 1C (rulaje reconciliabile);
//  * liniile se CULEG pozitive, `PregatesteOperare` le semnează negativ
//    (idempotent prin Abs — precedentul LDI 28a); direcția e fixă per tip,
//    deci nu e nevoie de enum pe linie;
//  * stocul și pasul TVA din motor NU se schimbă: semnul liniei face treaba,
//    regula spune doar latura și registrul;
//  * singura extensie de motor e `RegulaContare.PastreazaSemn` (valoarea se
//    postează cu semnul ei, fără normalizarea SemnFiltru).
// Flag-ul `Storno` rămâne al meta-operației `Storneaza`: stornarea unui retur
// produce rânduri POZITIVE cu Storno=true — consecință naturală a convenției.
// Proveniența unui rând de retur se citește din TipDocument.
//
// Ambele folosesc detaliul de BAZĂ (`DocumentDetaliu`): nu au niciun câmp propriu
// de linie — lotul, cantitatea, valoarea și TVA-ul sunt toate pe bază.

// RLF: marfa se întoarce la furnizor pe LOTUL ORIGINAL. Laturi Gestiune →
// Partener; stoc −q (regula +1 pe predator × linia negativă); contare
// 3xx = 401 cu −V (stornarea achiziției) + 4426 = 401 cu −TVA (PoliticaTva).
public class ReturFurnizor : Document, IDocumentCuIesireFiscala, IDocumentFiscalPrimit {
    [DevExpress.ExpressApp.DC.XafDisplayName("Exigibilitate TVA")]
    public virtual DateOnly? DataExigibilitate { get; set; }
    [DevExpress.ExpressApp.DC.XafDisplayName("Data primirii")]
    public virtual DateOnly? DataPrimire { get; set; }

    public override Declaratii.IDeclarant Declarant() => Declaratii.DeclarantReturFurnizor.Instanta;

    public override Declaratii.ContractLaturi Laturi() =>
        new(Declaratii.Latura.Gestiune, Declaratii.Latura.Externa);

    // Rolul de STINS (F19-D16): RLF stornează achiziția (3xx = 401 cu −V), deci
    // lasă un sold DEBITOR pe 401 — se stinge creditând contrapartida (o
    // încasare de la furnizor, sau jumătatea de credit a unei note).
    //
    // TR-D8/101: restul vine din cub în modul, iar sensul rămâne explicit.
    // Calea directă cere partidă proprie și cont comun; lista de candidați
    // are propriul catalog, separat de raportul general al partidelor.
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Creanta;

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        CalculeazaValori(os, Detalii, pastreazaTvaCules: true);
        foreach (var linie in Detalii.Where(l => l.LotId != null)) {
            linie.Cantitate = -linie.Cantitate;
            linie.Valoare = -linie.Valoare;
            linie.ValoareTva = -linie.ValoareTva;
        }
    }

    public override bool CuTva() => true;
    public override bool SemnulEAlOperarii() => true;

    // F18: returul care golește lotul nu preia soldul valoric rămas (IDocumentCuIesireFiscala).
    public override decimal? BazaLinie(DevExpress.ExpressApp.IObjectSpace os, DocumentDetaliu linie) =>
        Lot.ValoareLaPretulLotului(os, linie, Math.Abs(linie.Cantitate));

    // Forma culegerii e pozitivă: semnul e al operării (28a/46e), deci recalculul e idempotent.
    protected override void CalculeazaLinie(DocumentDetaliu linie, decimal baza, Motor.ContextTva tva, bool pastreazaTvaCules) {
        linie.Cantitate = Math.Abs(linie.Cantitate);
        linie.ValoareTva = Math.Abs(linie.ValoareTva);
        base.CalculeazaLinie(linie, baza, tva, pastreazaTvaCules);
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);

        // Proiecție server-side, fără navigații lazy în enumerare (25b, ca DSC/ASM).
        var idsLot = Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var infoLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.Produs.TipMaterialId, l.LinieIntrareId })
            .ToDictionary(l => l.ID, l => (l.TipMaterialId, l.LinieIntrareId));
        // Capitalizat ar umfla valoarea liniei peste costul lotului (net × cotă)
        // și ar rupe identificarea specifică (decizia 13) — refuz, simetric cu
        // guard-ul RDC (review advers 1C-a).
        var regimuri = Motor.TvaService.IncarcaTipuri(os, Detalii);

        foreach (var linie in Detalii) {
            if (linie.TipTvaId != null && regimuri.GetValueOrDefault(linie.TipTvaId.Value).Regim == RegimTva.Capitalizat)
                erori.Add("Regimul de TVA capitalizat nu are sens pe retur — valoarea returului e costul lotului; folosiți regimul achiziției originale.");
            if (linie.LotId == null) {
                erori.Add("Fiecare linie de retur descarcă LOTUL ORIGINAL al intrării (ieșirea e pe lot — decizia 13).");
                continue;
            }
            if (linie.Cantitate == 0m)
                erori.Add("Cantitatea returnată nu poate fi zero.");
            if (!infoLot.TryGetValue(linie.LotId.Value, out var lot))
                continue;
            if (lot.LinieIntrareId == linie.ID)
                erori.Add("Returul descarcă un lot existent, nu unul creat de linia proprie.");
            if (lot.TipMaterialId != null && lot.TipMaterialId != linie.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei.");
        }
    }
}

// RDC: UN SINGUR document, linii pe DOUĂ roluri, distinse prin LotId (§7 —
// perechea FCL+DSC există pentru decuplarea temporală, pe care returul n-o are;
// 1C importă 1:1 din același document). Laturi Partener → Gestiune.
//  * linie de VENIT (LotId == null): Tip VEN (Natura=Serviciu), `Valoare`
//    culeasă = venitul stornat, TipTva/ValoareTva → 4111 = 70x cu −V și
//    4111 = 4427 cu −TVA;
//  * linie de STOC/COST (LotId != null): Tip marfă, lotul ORIGINAL, cantitate;
//    fără TVA (TVA-ul e integral pe liniile de venit, ca FCL/DSC) → stoc +q
//    (regula −1 pe primitor × linia negativă) și 607 = 371 cu −cost.
// Generarea automată a liniilor de cost din pin-urile liniilor de venit rămâne
// AMÂNATĂ (acțiune/serviciu ulterior, precedent DescarcareService); importul 1C
// aduce ambele feluri de linii direct.
[GardContare(NaturaClasa.Stoc, NivelContare.TipMaterialExact,
    "Linia cu lot a returului nu are regulă de contare de cost pentru Tipul ei (6xx = cont de stoc, storno) — adăugați rândul de politică (sau rulați updater-ul).")]
public class ReturClient : Document, IDocumentFiscal {
    [DevExpress.ExpressApp.DC.XafDisplayName("Exigibilitate TVA")]
    public virtual DateOnly? DataExigibilitate { get; set; }

    public override Declaratii.IDeclarant Declarant() => Declaratii.DeclarantReturClient.Instanta;

    public override Declaratii.ContractLaturi Laturi() =>
        new(Declaratii.Latura.Externa, Declaratii.Latura.Gestiune);

    // Oglinda RLF-ului: RDC stornează livrarea (creditează 4111 cu −V), deci lasă
    // un sold CREDITOR pe contul clientului — se stinge debitând (plata de
    // rambursare, jumătatea de debit a notei). TR-D8/101 citește restul din
    // cub; totalul negativ al documentului nu ascunde datoria partidei proprii.
    public override SensStingere? SensDeStins(DevExpress.ExpressApp.IObjectSpace os) =>
        SensStingere.Datorie;

    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        CalculeazaValori(os, Detalii, pastreazaTvaCules: true);
        foreach (var linie in Detalii) {
            if (linie.LotId != null)
                linie.Cantitate = -linie.Cantitate;
            linie.Valoare = -linie.Valoare;
            linie.ValoareTva = -linie.ValoareTva;
        }
    }

    public override bool CuTva() => true;
    public override bool LinieFiscala(DocumentDetaliu linie) => linie.LotId == null;
    public override bool SemnulEAlOperarii() => true;
    public override IReadOnlySet<string> IntrariBaza() => intrariBaza;
    static readonly IReadOnlySet<string> intrariBaza = IntrariBazaCu(nameof(DocumentDetaliu.Valoare));

    // Venitul (fără lot) are baza culeasă; marfa returnată revine la prețul lotului original.
    public override decimal? BazaLinie(DevExpress.ExpressApp.IObjectSpace os, DocumentDetaliu linie) =>
        linie.LotId == null ? Math.Abs(linie.Valoare) : Lot.ValoareLaPretulLotului(os, linie, Math.Abs(linie.Cantitate));

    // Forma culegerii e pozitivă. Cantitatea venitului e pro-formă; linia de cost
    // e mișcare internă venit↔stoc, fără identitate fiscală (felia 11).
    protected override void CalculeazaLinie(DocumentDetaliu linie, decimal baza, Motor.ContextTva tva, bool pastreazaTvaCules) {
        if (linie.LotId == null) {
            linie.Cantitate = linie.Cantitate == 0m ? 1m : Math.Abs(linie.Cantitate);
            linie.ValoareTva = Math.Abs(linie.ValoareTva);
            base.CalculeazaLinie(linie, baza, tva, pastreazaTvaCules);
            return;
        }
        linie.Cantitate = Math.Abs(linie.Cantitate);
        linie.Valoare = Scara.RotunjesteBani(baza);
        linie.ValoareTva = 0m;
        linie.TipTva = null;
        linie.TipTvaId = null;
    }

    // Totalul = DOAR liniile de venit (brutul care ajustează creanța); liniile de
    // cost sunt mișcare internă venit↔stoc. `Total` e virtual pe bază tocmai
    // pentru cazul ăsta (design §7).
    // XAF0033 („EF Core business class properties should not be overridden") NU
    // se aplică: proprietatea e `[NotMapped]`, get-only și CALCULATĂ — nu e
    // membru persistent, deci nu există maparea pe care analizorul o apără.
#pragma warning disable XAF0033
    [NotMapped]
    public override decimal Total =>
        Detalii.Where(d => d.LotId == null).Sum(d => d.Valoare + d.ValoareTva);
#pragma warning restore XAF0033

    // Oglinda server-side a totalului antetului. Citirea operațională a
    // partidelor folosește cubul; costul fără partidă nu intră în decontare.
    public override IQueryable<DocumentDetaliu> LiniiCreanta(IQueryable<DocumentDetaliu> linii) =>
        linii.Where(d => d.LotId == null);

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);

        var claseTip = Motor.Fapte.ClaseTip(os, Detalii.Select(d => d.TipMaterialId));
        // Regimul TVA al liniilor de venit: Capitalizat n-are sens pe un venit
        // stornat (ar îngloba TVA-ul în valoare) și ar face semnarea
        // ne-idempotentă la re-operare (brutul ar compunda) — refuz.
        var regimuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        var idsLot = Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var infoLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.Produs.TipMaterialId, l.LinieIntrareId })
            .ToDictionary(l => l.ID, l => (l.TipMaterialId, l.LinieIntrareId));

        foreach (var linie in Detalii) {
            var natura = claseTip.GetValueOrDefault(linie.TipMaterialId).Natura;
            // Rolul liniei = LotId (nu un enum): venitul n-are lot, marfa care
            // revine îl are pe cel ORIGINAL.
            if (linie.LotId == null) {
                if (linie.Valoare == 0m)
                    erori.Add("Linia de venit a returului cere valoarea stornată (prețul de vânzare).");
                if (natura != NaturaClasa.Serviciu)
                    erori.Add("Linia de venit a returului poartă un Tip de venit (natura Serviciu); marfa care revine se culege pe o linie cu lot.");
                if (linie.TipTvaId != null && regimuri.GetValueOrDefault(linie.TipTvaId.Value).Regim == RegimTva.Capitalizat)
                    erori.Add("Regimul de TVA capitalizat nu are sens pe venitul stornat — folosiți regimul vânzării originale.");
                continue;
            }
            if (linie.Cantitate == 0m)
                erori.Add("Cantitatea mărfii returnate nu poate fi zero.");
            if (natura != NaturaClasa.Stoc)
                erori.Add("Linia cu lot a returului poartă un Tip de stoc (marfa revine pe lotul original).");
            if (!infoLot.TryGetValue(linie.LotId.Value, out var lot))
                continue;
            if (lot.LinieIntrareId == linie.ID)
                erori.Add("Marfa returnată revine pe lotul ORIGINAL — linia returului nu creează lot nou.");
            if (lot.TipMaterialId != null && lot.TipMaterialId != linie.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei.");
        }
    }
}
