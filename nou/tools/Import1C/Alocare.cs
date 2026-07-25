using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// SUPAPA DE IMPORT (decizia 48a): pinul pe lot vs. soldurile netate.
//
// Netarea deschiderii (47d) a rearanjat soldurile ȘI prețurile ÎN INTERIORUL
// grupei produs × depozit — deliberat, ca Atlas să nu pornească cu solduri
// negative. Consecința: un document 2025 care pin-uiește lotul original din 1C
// poate găsi lotul golit, deși grupa are marfa. Decizia 37d („pinul e intenția,
// fără fallback FIFO") rămâne INTACTĂ în motor — artefactul e al importului,
// deci se plătește în import, aici.
//
// Regula: ce acoperă pinul se ia de pe pin; deficitul se realocă FIFO în
// interiorul produs × gestiune (grupa în care netarea a conservat sumele, deci
// acoperirea există prin construcție — mai puțin grupele sărite la deschidere).
// Restul rămas după FIFO NU e excepție aici: se întoarce apelantului, care
// decide (backorder, raport, refuz). Gardianul de sold al motorului rămâne
// autoritatea finală la operare.
//
// **Disponibilul e cel al ÎNTREGII linii de timp, nu soldul la dată** (pasul 4):
// gardianul motorului cere sold ≥ 0 la sfârșitul FIECĂREI zile de la data
// documentului încolo (25d), iar importul scrie documentele grupate pe TIP, nu
// strict cronologic — deci în momentul în care se planifică o vânzare de pe 9
// ianuarie, registrul poate conține deja transferul de pe 20. Un „sold la dată"
// ar spune că lotul are marfă, iar operarea ar pica pe ziua de 20. Disponibilul
// real e MINIMUL sumelor cumulate pe zilele ≥ data — exact mărimea pe care o poți
// scoate fără să duci vreo zi ulterioară sub zero.
sealed class AlocareIesire {
    // Diagnostic, nu eșec (§12.1): se raportează per lună.
    public int Realocari { get; private set; }
    public decimal CantitateRealocata { get; private set; }
    public int PinuriGoale { get; private set; }
    public decimal Nedescarcat { get; private set; }

    // Produsele pe care supapa chiar le-a ATINS: pinul a fost înlocuit cu FIFO,
    // sau ieșirea n-a avut acoperire. Amândouă schimbă costul cu care Atlas
    // descarcă față de 1C, deci sunt exact mulțimea în care contractul lunar are
    // voie să vadă o diferență de VALOARE la cantitate exactă (§8.3). E o
    // evidență a ceea ce s-a întâmplat, nu o categorie presupusă.
    //
    // Se PERSISTĂ (tabela de legături „1C:ProdusRealocat", cheia = id-ul Atlas al
    // produsului): altfel verdictul contractului ar depinde de faptul că rularea
    // curentă a importat sau a sărit documentele — o reluare care sare tot n-ar
    // mai ști ce a atins supapa și ar raporta drept nejustificate exact
    // diferențele pe care ea le-a produs.
    public HashSet<Guid> ProduseRealocate { get; } = [];

    public const string ViewRealocate = "ProdusRealocat";

    public void Incarca(IObjectSpace os) {
        foreach (var tinta in Legaturi.Incarca(os, ViewRealocate).Values)
            ProduseRealocate.Add(tinta);
    }

    // Marchează produsul, în ObjectSpace-ul documentului curent: legătura se
    // comite odată cu el, deci o rulare întreruptă nu lasă evidența în urma
    // faptelor.
    void Marcheaza(IObjectSpace os, Guid produsId) {
        if (ProduseRealocate.Add(produsId))
            Legaturi.Leaga(os, ViewRealocate, produsId.ToString(), produsId);
    }

    int realocariLaStart;
    decimal cantitateLaStart;

    public void IncepeLuna() {
        realocariLaStart = Realocari;
        cantitateLaStart = CantitateRealocata;
    }

    public (int Realocari, decimal Cantitate) DeltaLunii() =>
        (Realocari - realocariLaStart, CantitateRealocata - cantitateLaStart);

    // `dejaAlocat` = alocările deja făcute în ACELAȘI document, încă necomise
    // (registrul nu le vede — pattern-ul DescarcareService, 38b). Se ACTUALIZEAZĂ
    // aici: apelantul îl ține per document și îl transmite la fiecare linie.
    public (IReadOnlyList<(Guid LotId, decimal Cantitate)> Alocari, decimal Ramas) Aloca(
            IObjectSpace os, Guid? lotDoritId, Guid produsId, Guid gestiuneId, TipStoc tipStoc,
            DateOnly data, decimal cantitate, Dictionary<Guid, decimal> dejaAlocat) {
        var alocari = new List<(Guid LotId, decimal Cantitate)>();
        var ramas = cantitate;
        if (cantitate <= 0)
            return (alocari, 0m);

        // O singură interogare per cerere, pentru toată grupa produs × gestiune ×
        // registru: pinul și FIFO-ul se servesc din aceleași rânduri (înainte era
        // un `Sold` per lot).
        var randuri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Lot.ProdusId == produsId && r.RepartitorId == gestiuneId
                && r.TipStoc == tipStoc)
            .Select(r => new { r.LotId, r.Data, r.Cantitate, DataLot = r.Lot.Data })
            .ToList();
        var loturi = randuri.GroupBy(r => r.LotId)
            .Select(g => new {
                LotId = g.Key,
                DataLot = g.Min(x => x.DataLot),
                Disponibil = Disponibil(g.Select(x => (x.Data, x.Cantitate)), data),
            })
            .ToDictionary(x => x.LotId);

        decimal Liber(Guid lotId) => loturi.TryGetValue(lotId, out var l)
            ? Math.Max(0m, l.Disponibil - dejaAlocat.GetValueOrDefault(lotId))
            : 0m;

        void Ia(Guid lotId, decimal cantitateLuata) {
            alocari.Add((lotId, cantitateLuata));
            dejaAlocat[lotId] = dejaAlocat.GetValueOrDefault(lotId) + cantitateLuata;
            ramas -= cantitateLuata;
        }

        // 1. Pinul, cât acoperă.
        if (lotDoritId is { } pin) {
            var iaDePePin = Math.Min(ramas, Liber(pin));
            if (iaDePePin > 0)
                Ia(pin, iaDePePin);
            else
                PinuriGoale++;
        }

        // 2. Deficitul, FIFO în produs × gestiune (vechimea lotului, apoi ID-ul —
        //    aceeași ordine ca `StocService.AlocaFifoTolerant`).
        var inainteDeFifo = ramas;
        foreach (var lot in loturi.Values.OrderBy(l => l.DataLot).ThenBy(l => l.LotId)) {
            if (ramas <= 0)
                break;
            if (lotDoritId == lot.LotId)
                continue;
            var liber = Liber(lot.LotId);
            if (liber > 0)
                Ia(lot.LotId, Math.Min(ramas, liber));
        }
        // Realocare = doar deplasarea cerută de netare: fără pin, FIFO e pickingul
        // normal al modelului, nu o supapă.
        if (lotDoritId != null && inainteDeFifo > ramas) {
            Realocari++;
            CantitateRealocata += inainteDeFifo - ramas;
            Marcheaza(os, produsId);
        }
        // Ieșirea neacoperită (returul fără marfă în stoc) atinge la fel evaluarea
        // produsului: 1C a scos marfa la costul lotului lui, Atlas n-a scos-o
        // deloc. Contractul lunar are nevoie de mulțimea asta ca să poată numi
        // diferența, nu s-o presupună.
        if (ramas > 0)
            Marcheaza(os, produsId);

        Nedescarcat += ramas;
        return (alocari, ramas);
    }

    // Cantitatea maximă care poate ieși la `data` fără ca soldul cumulat să scadă
    // sub zero la sfârșitul vreunei zile ≥ `data` (gardianul 25d, în oglindă).
    static decimal Disponibil(IEnumerable<(DateOnly Data, decimal Cantitate)> miscari, DateOnly data) {
        decimal cumulat = 0, minim = decimal.MaxValue;
        var punctat = false;
        foreach (var zi in miscari.GroupBy(m => m.Data).OrderBy(g => g.Key)) {
            // Punctul de control al zilei documentului: ce era în stoc ÎNAINTE de
            // prima zi ulterioară (marfa care intră mai târziu nu se poate vinde
            // acum).
            if (zi.Key > data && !punctat) {
                minim = Math.Min(minim, cumulat);
                punctat = true;
            }
            cumulat += zi.Sum(m => m.Cantitate);
            if (zi.Key >= data) {
                minim = Math.Min(minim, cumulat);
                punctat = true;
            }
        }
        return minim == decimal.MaxValue ? cumulat : minim;
    }
}
