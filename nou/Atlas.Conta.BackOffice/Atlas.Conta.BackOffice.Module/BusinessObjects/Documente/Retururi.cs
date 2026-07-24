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
public class ReturFurnizor : Document {
    // Valoarea e COSTUL lotului (prețul nu se culege — pattern BTR/BCS/DSC);
    // TVA-ul urmează factura furnizorului, deci `pastreazaTvaCules` (ca FCT):
    // valoarea culeasă/importată bate rotunjirea noastră.
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var linie in Detalii.Where(l => l.LotId != null)) {
            var q = Math.Abs(linie.Cantitate);
            var net = q * os.GetObjectByKey<Lot>(linie.LotId.Value).PretUnitar;
            // Normalizarea la pozitiv ÎNAINTE de calcul face semnarea idempotentă
            // (re-operarea după anulare nu dublează minusul).
            linie.ValoareTva = Math.Abs(linie.ValoareTva);
            Motor.TvaService.CalculeazaValori(linie, net, tipuri, pastreazaTvaCules: true);
            linie.Cantitate = -q;
            linie.Valoare = -linie.Valoare;
            linie.ValoareTva = -linie.ValoareTva;
        }
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Gestiune)
            erori.Add("Predatorul returului la furnizor este gestiunea din care iese marfa.");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Partener)
            erori.Add("Primitorul returului la furnizor este furnizorul (partener).");

        // Proiecție server-side, fără navigații lazy în enumerare (25b, ca DSC/ASM).
        var idsLot = Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var infoLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.Produs.TipMaterialId, l.LinieIntrareId })
            .ToDictionary(l => l.ID, l => (l.TipMaterialId, l.LinieIntrareId));

        foreach (var linie in Detalii) {
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
public class ReturClient : Document {
    public override void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) {
        var tipuri = Motor.TvaService.IncarcaTipuri(os, Detalii);
        foreach (var linie in Detalii) {
            if (linie.LotId == null) {
                // Cantitatea liniei de venit e pro-formă și rămâne POZITIVĂ
                // (nu intră în stoc); semnul storno stă pe valori.
                linie.Cantitate = linie.Cantitate == 0m ? 1m : Math.Abs(linie.Cantitate);
                var net = Math.Abs(linie.Valoare);
                linie.ValoareTva = Math.Abs(linie.ValoareTva);
                Motor.TvaService.CalculeazaValori(linie, net, tipuri, pastreazaTvaCules: true);
                linie.Valoare = -linie.Valoare;
                linie.ValoareTva = -linie.ValoareTva;
            }
            else {
                // Costul revine la prețul lotului original (nu se culege).
                var q = Math.Abs(linie.Cantitate);
                linie.Cantitate = -q;
                linie.Valoare = -(q * os.GetObjectByKey<Lot>(linie.LotId.Value).PretUnitar);
                linie.ValoareTva = 0m;
            }
        }
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

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not Partener)
            erori.Add("Predatorul returului de la client este clientul (partener).");
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not Gestiune)
            erori.Add("Primitorul returului de la client este gestiunea în care revine marfa.");

        var idsTip = Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturaPerTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);
        // Fără regulă de contare de cost per Tip, linia cu lot ar mișca stocul
        // fără să posteze NIMIC (motorul sare linia fără regulă) — refuz explicit,
        // exact defectul închis pe DSC la 38c; un Tip creat între updater-e nu
        // trece neobservat.
        var tipRdc = Motor.MotorOperare.GasesteTipDocument(os, this);
        var tipuriCuRegula = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipRdc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
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
            var natura = naturaPerTip.GetValueOrDefault(linie.TipMaterialId);
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
            if (!tipuriCuRegula.Contains(linie.TipMaterialId))
                erori.Add("Linia cu lot a returului nu are regulă de contare de cost pentru Tipul ei (6xx = cont de stoc, storno) — adăugați rândul de politică (sau rulați updater-ul).");
            if (!infoLot.TryGetValue(linie.LotId.Value, out var lot))
                continue;
            if (lot.LinieIntrareId == linie.ID)
                erori.Add("Marfa returnată revine pe lotul ORIGINAL — linia returului nu creează lot nou.");
            if (lot.TipMaterialId != null && lot.TipMaterialId != linie.TipMaterialId)
                erori.Add("Lotul liniei aparține unui produs cu alt Tip decât Tipul liniei.");
        }
    }
}
