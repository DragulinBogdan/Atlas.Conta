using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// FAZA 1C §6: generatorul închiderii lunare de TVA. Precedentul de formă e
// DescarcareService (37b) — liniile se nasc la GENERARE (draftul concret e
// documentul revizuibil), operarea nu creează linii nicăieri în motor; guard-urile
// întorc null (profil inert / nimic de închis), proiecțiile sunt server-side, iar
// commit-ul aparține APELANTULUI (consola 1C-d, acțiunea UI dacă apare, un
// endpoint la pasul 5).
//
// Conturile vin exclusiv din `PoliticaInchidereTva` (decizia 29 — niciun simbol
// în motor): fără rând de politică (profilul bugetar) nu se generează nimic.
public static class InchidereTvaService {
    // Draftul lunii (an, luna) pe unitatea internă dată (ambele laturi — nota nu
    // are contrapartidă economică, convenția NotaContabila). Null = nimic de
    // închis: profil inert, lună deja închisă (draft sau document operat), sau
    // ambele solduri zero.
    public static InchidereTva Genereaza(IObjectSpace os, int an, int luna, Guid unitateId) {
        // 1. Profilul: ancora + setul COMPLET de conturi. Politică lipsă sau
        //    incompletă ⇒ tip inert (bugetar), exact ca DSC fără reguli de stoc.
        var tipItv = MotorOperare.GasesteTipDocument(os, nameof(InchidereTva));
        var politica = os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocumentId == tipItv.ID);
        if (politica == null || politica.ContDeductibilaId == null || politica.ContColectataId == null
                || politica.ContDePlataId == null || politica.ContDeRecuperatId == null)
            return null;
        var deductibilaId = politica.ContDeductibilaId.Value;
        var colectataId = politica.ContColectataId.Value;

        var primaZi = new DateOnly(an, luna, 1);
        var ultimaZi = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna));

        // 2. Idempotență/regenerare: luna are deja o închidere vie (Draft sau
        //    Operat) ⇒ nu se mai generează una. Stornatul nu contează — după
        //    storno regenerarea e permisă natural (soldurile revin).
        if (os.GetObjectsQuery<InchidereTva>()
                .Any(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat))
            return null;

        // 2b. Cronologia e obligatorie (review advers 1C-a): o închidere vie
        //     ULTERIOARĂ lunii cerute a închis deja cumulat și soldurile lunii
        //     ăsteia — a genera „în urmă" ar închide aceleași solduri a doua
        //     oară (4423/4424 dublate). Refuz zgomotos, nu null.
        if (os.GetObjectsQuery<InchidereTva>()
                .Any(d => d.Data > ultimaZi && d.Stare != StareDocument.Stornat))
            throw new OperareException(
                $"Există o închidere de TVA vie pentru o lună ulterioară lui {luna:00}/{an} — închiderile se generează cronologic.");

        // 3. Soldurile CUMULATE la ultima zi a lunii (nu rulajele lunii).
        var (sold4426, sold4427) = Solduri(os, deductibilaId, colectataId, ultimaZi);
        if (sold4426 == 0m && sold4427 == 0m)
            return null;

        // 4. Draftul: document lunar de PRIMĂ CLASĂ (Autogenerat = false) —
        //    nu e copil de grup conex, nu se șterge la anularea altcuiva.
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ")
            ?? throw new OperareException(
                "Lipsește Tipul tehnic „TRZ” (seed) — liniile închiderii de TVA nu au ce Tip să poarte.");

        var itv = os.CreateObject<InchidereTva>();
        itv.Data = ultimaZi;
        itv.PredatorId = unitateId;
        itv.PrimitorId = unitateId;
        itv.Autogenerat = false;

        void Linie(string descriere, Guid contDebitId, Guid contCreditId, decimal valoare) {
            if (valoare <= 0m)
                return;
            var linie = os.CreateObject<NotaContabilaDetaliu>();
            linie.Document = itv;
            linie.TipMaterialId = tipTrz.ID;
            linie.Descriere = descriere;
            linie.ContDebitId = contDebitId;
            linie.ContCreditId = contCreditId;
            linie.Valoare = valoare;
        }

        // Transferul deductibilei în colectată, pe minimul celor două solduri;
        // excedentul rămas se regularizează într-un singur sens.
        var transfer = Math.Min(sold4426, sold4427);
        Linie($"Închidere TVA {luna:00}/{an}: transfer deductibilă în colectată",
            colectataId, deductibilaId, transfer);
        Linie($"Închidere TVA {luna:00}/{an}: TVA de plată",
            colectataId, politica.ContDePlataId.Value, sold4427 - transfer);
        Linie($"Închidere TVA {luna:00}/{an}: TVA de recuperat",
            politica.ContDeRecuperatId.Value, deductibilaId, sold4426 - transfer);

        return itv;
    }

    // Soldurile cumulate ale conturilor de TVA la o dată: lunile anterioare, o
    // dată închise, lasă sold 0, iar primul run prinde și soldul de deschidere.
    // Rândurile de storno intră în calcul — sunt rânduri reale de registru.
    // 4426 e cont de activ (sold debitor), 4427 de pasiv (sold creditor); un
    // sold pe sensul „greșit" nu se închide (Max 0) — s-ar transforma într-o
    // notă absurdă; cazul e patologic și se vede în reconciliere.
    // Partajat cu validarea anti-stale din `InchidereTva.ValideazaOperare`.
    internal static (decimal Sold4426, decimal Sold4427) Solduri(
        IObjectSpace os, Guid deductibilaId, Guid colectataId, DateOnly panaLa) {
        decimal Debit(Guid contId) => os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.Data <= panaLa && r.ContDebitId == contId)
            .Sum(r => (decimal?)r.Valoare) ?? 0m;
        decimal Credit(Guid contId) => os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.Data <= panaLa && r.ContCreditId == contId)
            .Sum(r => (decimal?)r.Valoare) ?? 0m;
        return (Math.Max(0m, Debit(deductibilaId) - Credit(deductibilaId)),
                Math.Max(0m, Credit(colectataId) - Debit(colectataId)));
    }
}
