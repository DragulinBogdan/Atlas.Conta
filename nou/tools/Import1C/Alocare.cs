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
sealed class AlocareIesire {
    // Diagnostic, nu eșec (§12.1): se raportează per lună.
    public int Realocari { get; private set; }
    public decimal CantitateRealocata { get; private set; }
    public int PinuriGoale { get; private set; }
    public decimal Nedescarcat { get; private set; }

    int realocariLaStart;
    decimal cantitateLaStart;

    public void IncepeLuna() {
        realocariLaStart = Realocari;
        cantitateLaStart = CantitateRealocata;
    }

    public (int Realocari, decimal Cantitate) DeltaLunii() =>
        (Realocari - realocariLaStart, CantitateRealocata - cantitateLaStart);

    // `dejaAlocat` = alocările deja făcute în ACELAȘI document, încă necomise
    // (`Sold` nu le vede — pattern-ul DescarcareService, 38b). Se ACTUALIZEAZĂ
    // aici: apelantul îl ține per document și îl transmite la fiecare linie.
    public (IReadOnlyList<(Guid LotId, decimal Cantitate)> Alocari, decimal Ramas) Aloca(
            IObjectSpace os, Guid? lotDoritId, Guid produsId, Guid gestiuneId, TipStoc tipStoc,
            DateOnly data, decimal cantitate, Dictionary<Guid, decimal> dejaAlocat) {
        var alocari = new List<(Guid LotId, decimal Cantitate)>();
        var ramas = cantitate;
        if (cantitate <= 0)
            return (alocari, 0m);

        // 1. Pinul, cât acoperă. Soldul se citește LA DATĂ (gardianul motorului
        //    verifică prefix-sum pe zile — 25d), minus ce a prins deja o linie
        //    anterioară a aceluiași document.
        if (lotDoritId is { } pin) {
            var sold = StocService.Sold(os, new CheieStoc(pin, gestiuneId, tipStoc), data)
                - dejaAlocat.GetValueOrDefault(pin);
            var iaDePePin = Math.Min(ramas, Math.Max(0m, sold));
            if (iaDePePin > 0) {
                alocari.Add((pin, iaDePePin));
                dejaAlocat[pin] = dejaAlocat.GetValueOrDefault(pin) + iaDePePin;
                ramas -= iaDePePin;
            }
            else
                PinuriGoale++;
        }

        // 2. Deficitul, FIFO în produs × gestiune. `dejaAlocat` e la zi (pasul 1
        //    l-a actualizat), deci FIFO nu re-alocă ce a prins pinul.
        if (ramas > 0) {
            var (fifo, rest) = StocService.AlocaFifoTolerant(os, produsId, gestiuneId, tipStoc,
                data, ramas, dejaAlocat);
            foreach (var (lotId, q) in fifo) {
                alocari.Add((lotId, q));
                dejaAlocat[lotId] = dejaAlocat.GetValueOrDefault(lotId) + q;
            }
            var realocat = ramas - rest;
            // Realocare = doar deplasarea cerută de netare: fără pin, FIFO e
            // pickingul normal al modelului, nu o supapă.
            if (lotDoritId != null && realocat > 0) {
                Realocari++;
                CantitateRealocata += realocat;
            }
            ramas = rest;
        }

        Nedescarcat += ramas;
        return (alocari, ramas);
    }
}
