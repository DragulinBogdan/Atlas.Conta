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
//
// ═══ Ce a adus felia 21 (F21-D2) ═══
// `Genereaza` colapsa TREI cauze de „nimic de generat" pe același `null` (profil
// inert / închidere vie / solduri zero), iar apelantul le compensa cu un string
// scris de el (`motivFaraDraft`, Import1C) — un ecran nu poate spune DE CE n-a
// generat dintr-un `null`. Serviciul capătă deci un REZULTAT cu cauză
// (`RezultatInchidere` + `MotivNegenerare`) și o ușă de RAPORT
// (`Previzualizeaza`), pe lângă comanda de azi.
//
// Trei reguli țin ușile oneste:
//   * `CalculeazaLinii` e SINGURA aritmetică a celor trei linii, iar
//     generatorul o consumă — previzualizarea nu e o COPIE a calculului, e
//     ACELAȘI calcul (42c);
//   * `Analizeaza` e singura ordonare a gardienilor, iar raportul și comanda
//     diferă printr-un singur bit: cronologia (46c: refuz zgomotos la comandă,
//     motiv la raport);
//   * `Genereaza` rămâne exact ce era — apelanții vechi (Import1C, probele de
//     motor) nu se ating.
public static class InchidereTvaService {

    // Cele trei linii ale închiderii, ca VALORI: transferul deductibilei în
    // colectată (pe minimul soldurilor) plus excedentul, care cade într-un
    // singur sens. Zero = linia nu se generează.
    public readonly record struct LiniiInchidere(decimal Transfer, decimal DePlata, decimal DeRecuperat);

    // Verdictul unei încercări de închidere. `Document != null` ⇔ `Motiv == null`.
    // `Sold4426`/`Sold4427` sunt null DOAR pe `ProfilInert` — acolo nu există
    // conturi, deci nu există cifră de arătat (un 0 ar fi o minciună liniștitoare).
    // `InchidereVieId` = documentul care blochează, pe `InchidereVie` (închiderea
    // lunii) și pe `NeCronologica` (închiderea ulterioară).
    public sealed record RezultatInchidere(
        InchidereTva Document,
        MotivNegenerare? Motiv,
        decimal? Sold4426,
        decimal? Sold4427,
        Guid? InchidereVieId,
        LiniiInchidere Linii);

    // SINGURA aritmetică a închiderii (F21-D2a). Rotunjirea se aplică AICI, nu la
    // scrierea liniei: o valoare sub ban ar fi produs altfel o linie cu `Valoare`
    // 0, pe care `NotaContabila.ValideazaOperare` o refuză oricum la operare —
    // adică un draft neoperabil. Pe soldurile reale (registrul e la scara banilor)
    // rotunjirea e oricum identitate.
    public static LiniiInchidere CalculeazaLinii(decimal sold4426, decimal sold4427) {
        var transfer = Math.Min(sold4426, sold4427);
        return new LiniiInchidere(
            Scara.RotunjesteBani(transfer),
            Scara.RotunjesteBani(sold4427 - transfer),
            Scara.RotunjesteBani(sold4426 - transfer));
    }

    // Comanda: încearcă să genereze draftul lunii pe unitatea internă dată
    // (ambele laturi — nota nu are contrapartidă economică, convenția
    // NotaContabila). NU comite (contractul de apelant din antet).
    //
    // Ordinea gardienilor e cea de la 1C-a, neschimbată: profil → închidere vie
    // → cronologie (THROW) → solduri → TRZ. Singura adăugire e verificarea
    // ARGUMENTULUI, înaintea tuturor: `unitateId` care nu e o `UnitateInterna`
    // nu e o stare a bazei pe care raportul s-o poată descrie, e o cerere greșită.
    public static RezultatInchidere Incearca(IObjectSpace os, int an, int luna, Guid unitateId) {
        // F21-D2f: oglinda gardului din `NotaContabila.ValideazaOperare` („laturile
        // sunt repartitori INTERNI"), adusă la GENERARE. Fără el, o cerere HTTP cu
        // id de partener ar fi produs un draft care se refuză abia la operare, cu
        // un mesaj despre altceva.
        var unitate = os.GetObjectByKey<Repartitor>(unitateId);
        if (unitate is not UnitateInterna)
            throw new OperareException(
                $"Laturile închiderii de TVA sunt unitatea internă care închide luna — "
                + $"{(unitate == null ? unitateId.ToString() : $"„{unitate.Denumire}”")} nu e o unitate internă.");

        var analiza = Analizeaza(os, an, luna, cronologiaCaMotiv: false);
        if (analiza.Rezultat.Motiv != null)
            return analiza.Rezultat;

        // Tipul tehnic al liniilor: politica de conturi e a PROFILULUI, dar `TRZ`
        // e ancoră de seed — lipsa lui e defect de instalare, nu stare descriptibilă.
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ")
            ?? throw new OperareException(
                "Lipsește Tipul tehnic „TRZ” (seed) — liniile închiderii de TVA nu au ce Tip să poarte.");

        var politica = analiza.Politica;
        var linii = analiza.Rezultat.Linii;

        // Draftul: document lunar de PRIMĂ CLASĂ (Autogenerat = false) — nu e
        // copil de grup conex, nu se șterge la anularea altcuiva.
        var itv = os.CreateObject<InchidereTva>();
        itv.Data = analiza.UltimaZi;
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

        Linie($"Închidere TVA {luna:00}/{an}: transfer deductibilă în colectată",
            politica.ContColectataId.Value, politica.ContDeductibilaId.Value, linii.Transfer);
        Linie($"Închidere TVA {luna:00}/{an}: TVA de plată",
            politica.ContColectataId.Value, politica.ContDePlataId.Value, linii.DePlata);
        Linie($"Închidere TVA {luna:00}/{an}: TVA de recuperat",
            politica.ContDeRecuperatId.Value, politica.ContDeductibilaId.Value, linii.DeRecuperat);

        return analiza.Rezultat with { Document = itv };
    }

    // Raportul: ce s-ar închide pe luna dată și, dacă nu se poate, DE CE. NU
    // scrie nimic — nici draft, nici commit; `Document` e întotdeauna null.
    //
    // N-are `unitateId`: unitatea e a documentului care s-ar CREA, iar aici nu se
    // creează nimic. Ecranul o culege oricum separat (F21-D4).
    //
    // Diferența față de `Incearca`: cronologia iese ca MOTIV, nu ca excepție.
    // Un raport care aruncă e un raport care nu poate spune „nu se poate,
    // fiindcă…" — exact întrebarea la care e chemat să răspundă (F21-D2b).
    public static RezultatInchidere Previzualizeaza(IObjectSpace os, int an, int luna) =>
        Analizeaza(os, an, luna, cronologiaCaMotiv: true).Rezultat;

    // Ușa apelanților vechi (Import1C, probele de motor 1C-a): semnătura și
    // comportamentul de la 46c, neatinse — inclusiv cele trei `null`-uri și cele
    // două excepții.
    public static InchidereTva Genereaza(IObjectSpace os, int an, int luna, Guid unitateId) =>
        Incearca(os, an, luna, unitateId).Document;

    // ═══════════════════ Ordinea gardienilor, o singură dată ═══════════════════
    //
    // Raportul și comanda pun ACELEAȘI întrebări, în ACEEAȘI ordine; singura
    // diferență e ce se întâmplă la cronologie. Două copii ale ordinii ar fi
    // divergeat tăcut la prima schimbare de gardian, iar ecranul ar fi promis
    // ceva ce comanda refuză.
    sealed record Analiza(PoliticaInchidereTva Politica, DateOnly UltimaZi, RezultatInchidere Rezultat);

    static Analiza Analizeaza(IObjectSpace os, int an, int luna, bool cronologiaCaMotiv) {
        var primaZi = new DateOnly(an, luna, 1);
        var ultimaZi = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna));

        Analiza Refuz(PoliticaInchidereTva politica, MotivNegenerare motiv,
                decimal? s4426, decimal? s4427, Guid? blocantId, LiniiInchidere linii) =>
            new(politica, ultimaZi, new RezultatInchidere(null, motiv, s4426, s4427, blocantId, linii));

        // 1. Profilul: ancora + setul COMPLET de conturi. Politică lipsă sau
        //    incompletă ⇒ tip inert (bugetar), exact ca DSC fără reguli de stoc.
        //    Fără conturi nu există nici solduri — de aici cele două `null`-uri.
        var tipItv = MotorOperare.GasesteTipDocument(os, nameof(InchidereTva));
        var politica = os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocumentId == tipItv.ID);
        if (politica == null || politica.ContDeductibilaId == null || politica.ContColectataId == null
                || politica.ContDePlataId == null || politica.ContDeRecuperatId == null)
            return Refuz(politica, MotivNegenerare.ProfilInert, null, null, null, default);

        // Soldurile se citesc o singură dată și însoțesc ORICE verdict de la aici
        // în jos: pe „luna e deja închisă" cifra 0/0 e chiar explicația, iar pe
        // „nu se poate genera în urmă" e ce ar fi trebuit să se închidă.
        var (sold4426, sold4427) = Solduri(
            os, politica.ContDeductibilaId.Value, politica.ContColectataId.Value, ultimaZi);
        var linii = CalculeazaLinii(sold4426, sold4427);

        // 2. Idempotență/regenerare: luna are deja o închidere vie (Draft sau
        //    Operat) ⇒ nu se mai generează una. Stornatul nu contează — după
        //    storno regenerarea e permisă natural (soldurile revin).
        var vie = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat)
            .Select(d => (Guid?)d.ID)
            .FirstOrDefault();
        if (vie != null)
            return Refuz(politica, MotivNegenerare.InchidereVie, sold4426, sold4427, vie, linii);

        // 2b. Cronologia e obligatorie (review advers 1C-a): o închidere vie
        //     ULTERIOARĂ lunii cerute a închis deja cumulat și soldurile lunii
        //     ăsteia — a genera „în urmă" ar închide aceleași solduri a doua
        //     oară (4423/4424 dublate). La COMANDĂ: refuz zgomotos, nu null.
        var ulterioara = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.Data > ultimaZi && d.Stare != StareDocument.Stornat)
            .Select(d => (Guid?)d.ID)
            .FirstOrDefault();
        if (ulterioara != null) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"Există o închidere de TVA vie pentru o lună ulterioară lui {luna:00}/{an} — închiderile se generează cronologic.");
            return Refuz(politica, MotivNegenerare.NeCronologica, sold4426, sold4427, ulterioara, linii);
        }

        // 3. Soldurile CUMULATE la ultima zi a lunii (nu rulajele lunii).
        if (sold4426 == 0m && sold4427 == 0m)
            return Refuz(politica, MotivNegenerare.FaraSold, sold4426, sold4427, null, default);

        return new Analiza(politica, ultimaZi,
            new RezultatInchidere(null, null, sold4426, sold4427, null, linii));
    }

    // Soldurile cumulate ale conturilor de TVA la o dată: lunile anterioare, o
    // dată închise, lasă sold 0, iar primul run prinde și soldul de deschidere.
    // Rândurile de storno intră în calcul — sunt rânduri reale de registru.
    // 4426 e cont de activ (sold debitor), 4427 de pasiv (sold creditor); un
    // sold pe sensul „greșit" nu se închide (Max 0) — s-ar transforma într-o
    // notă absurdă; cazul e patologic și se vede în reconciliere.
    // Partajat cu validarea anti-stale din `InchidereTva.ValideazaOperare` și,
    // de la felia 21, cu `InchidereTvaApply` (cifrele „curente" din ReadDto și
    // verdictul `Stale` sunt ale ACELEIAȘI funcții ca gardianul — altfel ecranul
    // ar spune altceva decât refuză operarea). De aici `public` (F21-D2d).
    public static (decimal Sold4426, decimal Sold4427) Solduri(
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
