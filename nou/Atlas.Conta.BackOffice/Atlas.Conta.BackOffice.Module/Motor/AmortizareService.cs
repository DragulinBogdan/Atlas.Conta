using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>Un rând de `RegistruImobilizari`, citit o dată și consumat în memorie.</summary>
public readonly record struct RandRegistru(
    Guid ID, Guid ImobilizareId, DateOnly Data, FelMiscareImobilizare Fel, bool Storno, Guid DetaliuId,
    decimal Valoare, decimal ValoareFiscala,
    decimal Amortizare, decimal AmortizareFiscala, decimal AmortizareDeductibila, int Luni,
    MetodaAmortizare? Metoda, int? DurataLuni, decimal? ValoareReziduala,
    MetodaAmortizare? MetodaFiscala, int? DurataFiscalaLuni,
    CategorieFiscala? CategorieFiscala, bool? UtilizareExclusiva);

/// <summary>Situația unei fișe la o dată: sumele registrului + parametrii ultimului eveniment.</summary>
public sealed record SituatieImobilizare(
    decimal Valoare, decimal ValoareFiscala,
    decimal Amortizare, decimal AmortizareFiscala, decimal AmortizareDeductibila,
    int Luni,
    MetodaAmortizare? Metoda, int? DurataLuni, decimal? ValoareReziduala,
    MetodaAmortizare? MetodaFiscala, int? DurataFiscalaLuni,
    CategorieFiscala? CategorieFiscala, bool? UtilizareExclusiva,
    DateOnly? DataUltimEveniment) {
    public decimal NetContabil => Valoare - Amortizare;
    public decimal NetFiscal => ValoareFiscala - AmortizareFiscala;
}

/// <summary>Intrările aritmeticii unei luni, ca valori pure.</summary>
public readonly record struct BazaAmortizare(
    MetodaAmortizare Metoda,
    decimal ValoareDeAmortizat,
    int LuniRamase,
    int LuniDeLaEveniment,
    decimal RestCurent,
    decimal Brut,
    int LuniDeLaPunere);

/// <summary>O regulă de deductibilitate desprinsă de bază, ca valoare.</summary>
public readonly record struct RegulaSnapshot(
    CategorieFiscala Categorie, bool DoarNeexclusiv, FelDeductibilitate Fel,
    decimal Valoare, DateOnly DeLa, DateOnly? PanaLa);

/// <summary>Cele două linii ale ieșirii unei fișe, recalculate din registru și politică.</summary>
public readonly record struct LinieIesire(
    FelLinieIesire Fel, decimal Valoare, Guid? ContDebitId, Guid? ContCreditId);

/// <summary>O linie a amortizării lunii: cele trei cifre, conturile și dimensiunile.</summary>
public sealed record LinieAmortizare(
    Guid ImobilizareId, string NumarInventar, string Denumire, Guid TipMaterialId,
    decimal Contabil, decimal Fiscal, decimal Deductibil,
    Guid? ContCheltuialaId, Guid? ContAmortizareId,
    Guid LocId, Guid? CentruCostId);

/// <summary>Verdictul unei încercări de amortizare. `Document != null` ⇔ `Motiv == null`.</summary>
public sealed record RezultatAmortizare(
    AmortizareLunara Document,
    MotivNegenerare? Motiv,
    Guid? BlocantId,
    string Detaliu,
    IReadOnlyList<LinieAmortizare> Linii);

// Singura aritmetică a amortizării (F26-D7).
public static class AmortizareService {
    static readonly FelMiscareImobilizare[] Evenimente = [
        FelMiscareImobilizare.Intrare, FelMiscareImobilizare.Modernizare,
        FelMiscareImobilizare.Revizuire, FelMiscareImobilizare.Reevaluare,
    ];

    // ═══════════════════════════ Citirea registrului ═══════════════════════════

    public static SituatieImobilizare Situatie(IObjectSpace os, Guid imobilizareId, DateOnly laData) =>
        Situatie(Randuri(os, [imobilizareId], laData), laData);

    public static SituatieImobilizare Situatie(IEnumerable<RandRegistru> toate, DateOnly laData) {
        var randuri = toate.Where(r => r.Data <= laData).ToList();
        // Storno-ul copiază parametrii, deci evenimentul stornat iese și din rezolvarea lor (F26-D2).
        var stornate = randuri.Where(r => r.Storno).Select(r => r.DetaliuId).ToHashSet();
        var evenimente = randuri
            .Where(r => !r.Storno && Evenimente.Contains(r.Fel) && !stornate.Contains(r.DetaliuId))
            .OrderBy(r => r.Data).ThenBy(r => r.ID)
            .ToList();

        return new SituatieImobilizare(
            randuri.Sum(r => r.Valoare), randuri.Sum(r => r.ValoareFiscala),
            randuri.Sum(r => r.Amortizare), randuri.Sum(r => r.AmortizareFiscala),
            randuri.Sum(r => r.AmortizareDeductibila), randuri.Sum(r => r.Luni),
            Coalesce(evenimente, r => r.Metoda), Coalesce(evenimente, r => r.DurataLuni),
            Coalesce(evenimente, r => r.ValoareReziduala), Coalesce(evenimente, r => r.MetodaFiscala),
            Coalesce(evenimente, r => r.DurataFiscalaLuni), Coalesce(evenimente, r => r.CategorieFiscala),
            Coalesce(evenimente, r => r.UtilizareExclusiva),
            evenimente.Count == 0 ? null : evenimente[^1].Data);
    }

    static List<RandRegistru> Randuri(IObjectSpace os, List<Guid> fise, DateOnly panaLa) =>
        os.GetObjectsQuery<RegistruImobilizari>()
            .Where(r => fise.Contains(r.ImobilizareId) && r.Data <= panaLa)
            .Select(r => new {
                r.ID, r.ImobilizareId, r.Data, r.Fel, r.Storno, r.DetaliuId,
                r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala,
                r.AmortizareDeductibila, r.Luni,
                r.Metoda, r.DurataLuni, r.ValoareReziduala,
                r.MetodaFiscala, r.DurataFiscalaLuni, r.CategorieFiscala, r.UtilizareExclusiva,
            })
            .ToList()
            .Select(r => new RandRegistru(r.ID, r.ImobilizareId, r.Data, r.Fel, r.Storno, r.DetaliuId,
                r.Valoare, r.ValoareFiscala, r.Amortizare, r.AmortizareFiscala, r.AmortizareDeductibila,
                r.Luni, r.Metoda, r.DurataLuni, r.ValoareReziduala, r.MetodaFiscala, r.DurataFiscalaLuni,
                r.CategorieFiscala, r.UtilizareExclusiva))
            .ToList();

    // Coalesce ÎNAPOI: null pe un eveniment înseamnă „neschimbat” (F26-D2).
    static T? Coalesce<TRand, T>(List<TRand> evenimente, Func<TRand, T?> citeste) where T : struct {
        for (var i = evenimente.Count - 1; i >= 0; i--)
            if (citeste(evenimente[i]) is T valoare)
                return valoare;
        return null;
    }

    // ═══════════════════════════ Aritmetica, pură ══════════════════════════════

    /// <summary>Suma LUNII pentru o bază dată: rotunjită și plafonată la restul curent.</summary>
    public static decimal CotaLunara(BazaAmortizare b) {
        if (b.RestCurent <= 0m)
            return 0m;
        var suma = b.Metoda switch {
            MetodaAmortizare.Accelerata when b.LuniDeLaPunere < 12 => Scara.RotunjesteBani(b.Brut * 0.5m / 12),
            MetodaAmortizare.Degresiva => Degresiva(b),
            _ => Liniara(b),
        };
        return Math.Min(suma, b.RestCurent);
    }

    static decimal Liniara(BazaAmortizare b) =>
        b.LuniRamase <= 0 ? b.RestCurent : Scara.RotunjesteBani(b.ValoareDeAmortizat / b.LuniRamase);

    // AD1 (F26-r4 lasă AD2 afară): graficul pornește la ultimul eveniment și trece
    // la liniar din anul în care rata degresivă nu mai bate media rămasă.
    static decimal Degresiva(BazaAmortizare b) {
        if (b.LuniRamase <= 0 || b.LuniDeLaEveniment >= b.LuniRamase)
            return b.RestCurent;
        var ani = Math.Max(1, (b.LuniRamase + 11) / 12);
        var k = ani <= 5 ? 1.5m : ani <= 10 ? 2.0m : 2.5m;
        var anCurent = b.LuniDeLaEveniment / 12;
        var rest = b.ValoareDeAmortizat;
        var liniar = false;
        var anual = 0m;
        for (var an = 0; an <= anCurent; an++) {
            var degresiv = rest * k / ani;
            var mediu = rest / (ani - an);
            liniar = liniar || degresiv <= mediu;
            anual = liniar ? mediu : degresiv;
            if (an < anCurent)
                rest -= anual;
        }
        if (b.LuniDeLaEveniment == b.LuniRamase - 1)
            return b.RestCurent;
        var lunar = Scara.RotunjesteBani(anual / 12);
        return b.LuniDeLaEveniment % 12 == 11 ? Scara.RotunjesteBani(anual) - 11 * lunar : lunar;
    }

    /// <summary>Partea deductibilă a amortizării fiscale, după regulile valabile la dată.</summary>
    public static decimal Deductibil(decimal fiscal, CategorieFiscala categorie, bool utilizareExclusiva,
            IReadOnlyList<RegulaSnapshot> reguli, DateOnly data) {
        decimal Aplica(decimal valoare, FelDeductibilitate fel) {
            RegulaSnapshot? castigator = null;
            foreach (var r in reguli) {
                if (r.Categorie != categorie || r.Fel != fel || (r.DoarNeexclusiv && utilizareExclusiva))
                    continue;
                if (r.DeLa > data || (r.PanaLa != null && r.PanaLa < data))
                    continue;
                if (castigator == null || r.DeLa > castigator.Value.DeLa)
                    castigator = r;
            }
            if (castigator == null)
                return valoare;
            return fel == FelDeductibilitate.PlafonLunar
                ? Math.Min(valoare, castigator.Value.Valoare)
                : Scara.RotunjesteBani(valoare * castigator.Value.Valoare / 100m);
        }
        return Aplica(Aplica(fiscal, FelDeductibilitate.PlafonLunar), FelDeductibilitate.Procent);
    }

    // ═══════════════════════════ Ieșirea din patrimoniu ════════════════════════

    /// <summary>Cele două note ale ieșirii unei fișe, la o dată: cumulatul și restul.</summary>
    public static IReadOnlyList<LinieIesire> LiniiIesire(IObjectSpace os, Guid imobilizareId, DateOnly data,
            PoliticaAmortizare politica) {
        var situatie = Situatie(os, imobilizareId, data);
        var fisa = os.GetObjectByKey<Imobilizare>(imobilizareId);
        var contImplicit = fisa == null ? null
            : os.GetObjectByKey<TipMaterial>(fisa.TipMaterialId)?.ContImplicitId;
        var linii = new List<LinieIesire> {
            new(FelLinieIesire.AmortizareCumulata, situatie.Amortizare,
                politica?.ContAmortizareId, contImplicit),
        };
        if (situatie.NetContabil != 0m)
            linii.Add(new(FelLinieIesire.ValoareRamasa, situatie.NetContabil,
                politica?.ContCheltuialaCedareId, contImplicit));
        return linii;
    }

    // ═══════════════════════════ Amortizarea lunii ═════════════════════════════

    /// <summary>SINGURA aritmetică a lunii: consumată de generator ȘI de gardianul de operare.</summary>
    public static IReadOnlyList<LinieAmortizare> CalculeazaLinii(IObjectSpace os, int an, int luna) =>
        Calcul(os, an, luna).Linii;

    public static RezultatAmortizare Incearca(IObjectSpace os, int an, int luna, Guid unitateId,
            Guid? inlocuieste = null) {
        var unitate = os.GetObjectByKey<Repartitor>(unitateId);
        if (unitate is not UnitateInterna)
            throw new OperareException(
                "Laturile amortizării lunare sunt unitatea internă care o înregistrează — "
                + $"{(unitate == null ? unitateId.ToString() : $"„{unitate.Denumire}”")} nu e o unitate internă.");

        var analiza = Analizeaza(os, an, luna, cronologiaCaMotiv: false, inlocuieste);
        if (analiza.Rezultat.Motiv != null)
            return analiza.Rezultat;

        var amo = os.CreateObject<AmortizareLunara>();
        amo.Data = analiza.UltimaZi;
        amo.PredatorId = unitateId;
        amo.PrimitorId = unitateId;
        amo.Autogenerat = false;
        foreach (var l in analiza.Rezultat.Linii) {
            var linie = os.CreateObject<AmortizareLunaraDetaliu>();
            linie.Document = amo;
            linie.ImobilizareId = l.ImobilizareId;
            linie.TipMaterialId = l.TipMaterialId;
            linie.Cantitate = 1m;
            linie.Valoare = l.Contabil;
            linie.ValoareFiscala = l.Fiscal;
            linie.ValoareDeductibila = l.Deductibil;
            linie.ContDebitId = l.ContCheltuialaId;
            linie.ContCreditId = l.ContAmortizareId;
            linie.RepartitorDebitId = l.LocId;
            linie.RepartitorCreditId = l.LocId;
            linie.CentruCostId = l.CentruCostId;
        }
        return analiza.Rezultat with { Document = amo };
    }

    public static RezultatAmortizare Previzualizeaza(IObjectSpace os, int an, int luna,
            Guid? inlocuieste = null) =>
        Analizeaza(os, an, luna, cronologiaCaMotiv: true, inlocuieste).Rezultat;

    public static AmortizareLunara Genereaza(IObjectSpace os, int an, int luna, Guid unitateId) =>
        Incearca(os, an, luna, unitateId).Document;

    public static string Eticheta(MotivNegenerare motiv) =>
        (Attribute.GetCustomAttribute(typeof(MotivNegenerare).GetField(motiv.ToString())!,
            typeof(XafDisplayNameAttribute)) as XafDisplayNameAttribute)?.DisplayName ?? motiv.ToString();

    // ───────────────────── Ordinea gardienilor, o singură dată ─────────────────
    sealed record Analiza(DateOnly UltimaZi, RezultatAmortizare Rezultat);

    static Analiza Analizeaza(IObjectSpace os, int an, int luna, bool cronologiaCaMotiv, Guid? inlocuieste) {
        var primaZi = new DateOnly(an, luna, 1);
        var ultimaZi = UltimaZiLuna(primaZi);
        var calcul = Calcul(os, an, luna);

        Analiza Refuz(MotivNegenerare motiv, Guid? blocant, string detaliu,
                IReadOnlyList<LinieAmortizare> linii) =>
            new(ultimaZi, new RezultatAmortizare(null, motiv, blocant, detaliu, linii));

        if (calcul.FaraPolitica != null)
            return Refuz(MotivNegenerare.FisaFaraPolitica, null, calcul.FaraPolitica, []);

        var vie = os.GetObjectsQuery<AmortizareLunara>()
            .Where(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat
                && (inlocuieste == null || d.ID != inlocuieste))
            .Select(d => (Guid?)d.ID).FirstOrDefault();
        if (vie != null)
            return Refuz(MotivNegenerare.AmortizareVie, vie, null, calcul.Linii);

        var ulterioara = os.GetObjectsQuery<AmortizareLunara>()
            .Where(d => d.Data > ultimaZi && d.Stare != StareDocument.Stornat)
            .OrderBy(d => d.Data).Select(d => (Guid?)d.ID).FirstOrDefault();
        if (ulterioara != null) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"Există o amortizare vie pentru o lună ulterioară lui {luna:00}/{an} — "
                    + "amortizările se generează cronologic.");
            return Refuz(MotivNegenerare.NeCronologica, ulterioara, null, calcul.Linii);
        }

        var anterior = os.GetObjectsQuery<AmortizareLunara>()
            .Where(d => d.Data < primaZi && d.Stare == StareDocument.Draft
                && (inlocuieste == null || d.ID != inlocuieste))
            .OrderBy(d => d.Data).Select(d => (Guid?)d.ID).FirstOrDefault();
        if (anterior != null) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"O lună anterioară lui {luna:00}/{an} are un draft de amortizare neoperat — "
                    + "operați-l sau ștergeți-l întâi; amortizările se generează cronologic.");
            return Refuz(MotivNegenerare.DraftAnterior, anterior, null, calcul.Linii);
        }

        var precedenta = primaZi.AddMonths(-1);
        var ultimaZiPrecedenta = primaZi.AddDays(-1);
        var operataPrecedent = os.GetObjectsQuery<AmortizareLunara>()
            .Any(d => d.Data >= precedenta && d.Data <= ultimaZiPrecedenta
                && d.Stare == StareDocument.Operat);
        if (!operataPrecedent && Calcul(os, precedenta.Year, precedenta.Month).Linii.Count > 0) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"Luna {precedenta.Month:00}/{precedenta.Year} avea fișe de amortizat și n-are amortizare "
                    + "operată — cronologia amortizării e strictă.");
            return Refuz(MotivNegenerare.LunaLipsa, null, null, calcul.Linii);
        }

        try {
            GardianPerioada.VerificaDeschisa(os, ultimaZi);
        }
        catch (OperareException) when (cronologiaCaMotiv) {
            return Refuz(MotivNegenerare.PerioadaInchisa, null, null, calcul.Linii);
        }

        if (calcul.Linii.Count == 0)
            return Refuz(MotivNegenerare.FaraFise, null, null, []);

        return new Analiza(ultimaZi, new RezultatAmortizare(null, null, null, null, calcul.Linii));
    }

    // ───────────────────── Liniile lunii, dintr-o singură citire ───────────────
    sealed record CalculLuna(IReadOnlyList<LinieAmortizare> Linii, string FaraPolitica);

    static CalculLuna Calcul(IObjectSpace os, int an, int luna) {
        var primaZi = new DateOnly(an, luna, 1);
        var ultimaZi = UltimaZiLuna(primaZi);
        var ultimaZiPrecedenta = primaZi.AddDays(-1);

        // Eligibilitatea e după DATE, nu după `Stare`: o fișă ieșită într-o lună
        // ULTERIOARĂ e legitimă pe amortizarea lunii de față (F26-D7).
        var fise = os.GetObjectsQuery<Imobilizare>()
            .Where(f => f.DataPunereInFunctiune != null && f.DataPunereInFunctiune < primaZi
                && (f.DataIesire == null || f.DataIesire > ultimaZi))
            .Select(f => new {
                f.ID, f.NumarInventar, f.Denumire, f.TipMaterialId, f.LocId, f.CentruCostId,
                f.DataPunereInFunctiune,
            })
            .ToList();
        if (fise.Count == 0)
            return new CalculLuna([], null);

        var ids = fise.Select(f => f.ID).ToList();
        var perFisa = Randuri(os, ids, ultimaZi).GroupBy(r => r.ImobilizareId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var tipuri = fise.Select(f => f.TipMaterialId).Distinct().ToList();
        var politici = os.GetObjectsQuery<PoliticaAmortizare>()
            .Where(p => tipuri.Contains(p.TipMaterialId))
            .Select(p => new { p.TipMaterialId, p.ContAmortizareId, p.ContCheltuialaAmortizareId })
            .ToList()
            .Where(p => p.ContAmortizareId != null && p.ContCheltuialaAmortizareId != null)
            .ToDictionary(p => p.TipMaterialId, p => (p.ContAmortizareId, p.ContCheltuialaAmortizareId));

        var reguli = os.GetObjectsQuery<RegulaDeductibilitate>()
            .Where(r => r.DeLa <= ultimaZi && (r.PanaLa == null || r.PanaLa >= ultimaZi))
            .Select(r => new { r.Categorie, r.DoarNeexclusiv, r.Fel, r.Valoare, r.DeLa, r.PanaLa })
            .ToList()
            .Select(r => new RegulaSnapshot(r.Categorie, r.DoarNeexclusiv, r.Fel, r.Valoare, r.DeLa, r.PanaLa))
            .ToList();

        var linii = new List<LinieAmortizare>();
        string faraPolitica = null;
        foreach (var f in fise.OrderBy(f => f.NumarInventar)) {
            var randuri = perFisa.GetValueOrDefault(f.ID) ?? [];
            var laM1 = Situatie(randuri, ultimaZiPrecedenta);
            if (laM1.DataUltimEveniment == null)
                continue;
            var contabil = Cifra(randuri, laM1, f.DataPunereInFunctiune, fiscal: false);
            var fiscal = Cifra(randuri, laM1, f.DataPunereInFunctiune, fiscal: true);
            if (contabil <= 0m && fiscal <= 0m)
                continue;
            if (!politici.TryGetValue(f.TipMaterialId, out var conturi)) {
                faraPolitica ??= f.NumarInventar;
                continue;
            }
            var deductibil = Deductibil(fiscal, laM1.CategorieFiscala ?? CategorieFiscala.Standard,
                laM1.UtilizareExclusiva ?? false, reguli, ultimaZi);
            linii.Add(new LinieAmortizare(f.ID, f.NumarInventar, f.Denumire, f.TipMaterialId,
                contabil, fiscal, deductibil,
                contabil == 0m ? null : conturi.ContCheltuialaAmortizareId,
                contabil == 0m ? null : conturi.ContAmortizareId,
                f.LocId, f.CentruCostId));
        }
        return new CalculLuna(linii, faraPolitica);
    }

    // Baza = situația la SFÂRȘITUL lunii ultimului eveniment; luna evenimentului postează încă cota
    // veche, iar restul și lunile se citesc la sfârșitul lunii precedente (F26-D7, formula 1C).
    static decimal Cifra(List<RandRegistru> randuri, SituatieImobilizare laM1, DateOnly? punere, bool fiscal) {
        var sfarsitEveniment = UltimaZiLuna(laM1.DataUltimEveniment.Value);
        var baza = Situatie(randuri, sfarsitEveniment);
        var metoda = (fiscal ? baza.MetodaFiscala : baza.Metoda) ?? MetodaAmortizare.Liniara;
        // Faza accelerată consumată ⇒ baza se citește la capătul ei, nu la eveniment (F26-D7).
        if (metoda == MetodaAmortizare.Accelerata && punere != null && laM1.Luni >= 12) {
            var capat = UltimaZiLuna(new DateOnly(punere.Value.Year, punere.Value.Month, 1).AddMonths(12));
            if (capat > sfarsitEveniment)
                baza = Situatie(randuri, capat);
        }
        var durata = (fiscal ? baza.DurataFiscalaLuni : baza.DurataLuni) ?? 0;
        if (durata <= 0)
            return 0m;
        var reziduala = fiscal ? 0m : baza.ValoareReziduala ?? 0m;
        var restCurent = fiscal
            ? laM1.ValoareFiscala - laM1.AmortizareFiscala
            : laM1.Valoare - (laM1.ValoareReziduala ?? 0m) - laM1.Amortizare;
        if (restCurent <= 0m)
            return 0m;
        return CotaLunara(new BazaAmortizare(
            metoda,
            fiscal ? baza.ValoareFiscala - baza.AmortizareFiscala : baza.Valoare - reziduala - baza.Amortizare,
            durata - baza.Luni,
            laM1.Luni - baza.Luni,
            restCurent,
            fiscal ? baza.ValoareFiscala : baza.Valoare,
            laM1.Luni));
    }

    static DateOnly UltimaZiLuna(DateOnly data) =>
        new(data.Year, data.Month, DateTime.DaysInMonth(data.Year, data.Month));
}
