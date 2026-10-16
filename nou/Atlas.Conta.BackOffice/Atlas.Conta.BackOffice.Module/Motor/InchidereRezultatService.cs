using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>
/// Generatorul închiderii lunare a conturilor de rezultat (E3-D3, E3-D4). Liniile se nasc la generare,
/// din soldurile cumulate la sfârșitul lunii și din rândurile <see cref="PoliticaInchidereConturi"/>;
/// commit-ul e al apelantului.
/// </summary>
public static class InchidereRezultatService {
    /// <summary>Atomul contabil: contul plus coordonatele soldului.</summary>
    public readonly record struct CheieSold(Guid Cont, Guid? Repartitor, Guid? Material, Guid? CodFunctional,
        Guid? CodEconomic, Guid? SursaFinantare, Guid? Unitate, Guid? Proiect, Guid? CentruCost);

    /// <summary>O cheie-sursă închisă pe destinație; <paramref name="SursaPeDebit"/> = sursa avea sold creditor.</summary>
    public sealed record LinieInchidere(Guid PoliticaId, CheieSold Sursa, Guid ContDestinatie,
        Guid? RepartitorDestinatie, DimensiuneFlags Pastrate, bool SursaPeDebit, decimal Valoare);

    /// <summary>`Document != null` ⇔ `Motiv == null`; `Linii` însoțesc orice verdict de după profil.</summary>
    public sealed record RezultatInchidere(
        InchidereRezultat Document,
        MotivNegenerare? Motiv,
        Guid? InchidereVieId,
        IReadOnlyList<LinieInchidere> Linii);

    /// <summary>Liniile închiderii la <paramref name="panaLa"/>: rândurile se aplică în ordine, pe soldul curent.</summary>
    public static IReadOnlyList<LinieInchidere> CalculeazaLinii(IObjectSpace os, DateOnly panaLa, bool cuAnuale) {
        var reguli = os.GetObjectsQuery<PoliticaInchidereConturi>()
            .Where(p => p.Moment == MomentInchidere.Lunar || (cuAnuale && p.Moment == MomentInchidere.Anual))
            .Select(p => new { p.ID, p.Moment, p.Ordine, p.ContSursaId, p.ContDestinatieId, p.PastreazaRepartitor, p.CoordonatePastrate })
            .ToList()
            .OrderBy(p => p.Moment).ThenBy(p => p.Ordine).ThenBy(p => p.ID)
            .ToList();
        if (reguli.Count == 0)
            return [];

        var plan = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.ParinteId, c.Simbol }).ToList();
        var copii = plan.Where(c => c.ParinteId != null).ToLookup(c => c.ParinteId.Value, c => c.ID);
        var simboluri = plan.ToDictionary(c => c.ID, c => c.Simbol ?? "");
        var descendenti = reguli.Select(r => r.ContSursaId).Distinct()
            .ToDictionary(id => id, id => Descendenti(id, copii));
        var atinse = descendenti.Values.SelectMany(d => d).Distinct().ToList();

        var sold = SolduriService.AtomiCumulati(os, CitireCumul.Integrala, panaLa)
            .Where(a => atinse.Contains(a.ContId))
            .GroupBy(a => new { a.ContId, a.RepartitorId, a.MaterialId, a.CodFunctionalId, a.CodEconomicId,
                a.SursaFinantareId, a.UnitateId, a.ProiectId, a.CentruCostId })
            .Select(g => new { g.Key, Net = g.Sum(a => a.Debit) - g.Sum(a => a.Credit) })
            .ToList()
            .Where(s => s.Net != 0m)
            .ToDictionary(
                s => new CheieSold(s.Key.ContId, s.Key.RepartitorId, s.Key.MaterialId, s.Key.CodFunctionalId,
                    s.Key.CodEconomicId, s.Key.SursaFinantareId, s.Key.UnitateId, s.Key.ProiectId, s.Key.CentruCostId),
                s => s.Net);

        var linii = new List<LinieInchidere>();
        foreach (var r in reguli) {
            var sursa = descendenti[r.ContSursaId];
            var chei = sold.Where(s => s.Value != 0m && sursa.Contains(s.Key.Cont)).Select(s => s.Key)
                .OrderBy(k => simboluri.GetValueOrDefault(k.Cont)).ThenBy(k => k.Repartitor).ThenBy(k => k.Material)
                .ThenBy(k => k.CodFunctional).ThenBy(k => k.CodEconomic).ThenBy(k => k.SursaFinantare)
                .ThenBy(k => k.Unitate).ThenBy(k => k.Proiect).ThenBy(k => k.CentruCost)
                .ToList();
            foreach (var k in chei) {
                var net = Scara.RotunjesteBani(sold[k]);
                sold[k] = 0m;
                if (net == 0m)
                    continue;
                var p = r.CoordonatePastrate;
                Guid? Pastrat(DimensiuneFlags f, Guid? v) => (p & f) == f ? v : null;
                var destinatie = new CheieSold(r.ContDestinatieId,
                    r.PastreazaRepartitor ? k.Repartitor : null,
                    Pastrat(DimensiuneFlags.Material, k.Material),
                    Pastrat(DimensiuneFlags.CodFunctional, k.CodFunctional),
                    Pastrat(DimensiuneFlags.CodEconomic, k.CodEconomic),
                    Pastrat(DimensiuneFlags.SursaFinantare, k.SursaFinantare),
                    Pastrat(DimensiuneFlags.Unitate, k.Unitate),
                    Pastrat(DimensiuneFlags.Proiect, k.Proiect),
                    Pastrat(DimensiuneFlags.CentruCost, k.CentruCost));
                sold[destinatie] = sold.GetValueOrDefault(destinatie) + net;
                linii.Add(new LinieInchidere(r.ID, k, r.ContDestinatieId, destinatie.Repartitor, p,
                    SursaPeDebit: net < 0m, Math.Abs(net)));
            }
        }
        return linii;
    }

    /// <summary>Conturile pe care le închide luna au sold la sfârșitul ei (REZ-NEINCHIS); în decembrie, și cele anuale.</summary>
    public static bool AreRezidual(IObjectSpace os, int an, int luna) =>
        CalculeazaLinii(os, UltimaZi(an, luna), cuAnuale: luna == 12).Count > 0;

    /// <summary>Comanda: draftul lunii pe unitatea internă dată. NU comite.</summary>
    public static RezultatInchidere Incearca(IObjectSpace os, int an, int luna, Guid unitateId, Guid? inlocuieste = null) {
        var unitate = os.GetObjectByKey<Repartitor>(unitateId);
        if (unitate is not UnitateInterna)
            throw new OperareException(
                "Laturile închiderii conturilor de rezultat sunt unitatea internă care închide luna — "
                + $"{(unitate == null ? unitateId.ToString() : $"„{unitate.Denumire}”")} nu e o unitate internă.");

        var rezultat = Analizeaza(os, an, luna, cronologiaCaMotiv: false, inlocuieste);
        if (rezultat.Motiv != null)
            return rezultat;

        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ")
            ?? throw new OperareException(
                "Lipsește Tipul tehnic „TRZ” (seed) — liniile închiderii conturilor nu au ce Tip să poarte.");

        var doc = os.CreateObject<InchidereRezultat>();
        doc.Data = UltimaZi(an, luna);
        doc.PredatorId = unitateId;
        doc.PrimitorId = unitateId;
        doc.Autogenerat = false;

        var simboluri = SimboluriLinii(os, rezultat.Linii);
        var pozitie = 0;
        foreach (var l in rezultat.Linii) {
            var linie = os.CreateObject<InchidereRezultatDetaliu>();
            linie.Document = doc;
            linie.Pozitie = ++pozitie;
            linie.TipMaterialId = tipTrz.ID;
            linie.Descriere = $"Închidere {simboluri[l.Sursa.Cont]} în {simboluri[l.ContDestinatie]} {luna:00}/{an}";
            linie.SursaPeDebit = l.SursaPeDebit;
            linie.CoordonatePastrate = l.Pastrate;
            linie.ContDebitId = l.SursaPeDebit ? l.Sursa.Cont : l.ContDestinatie;
            linie.ContCreditId = l.SursaPeDebit ? l.ContDestinatie : l.Sursa.Cont;
            linie.RepartitorDebitId = l.SursaPeDebit ? l.Sursa.Repartitor : l.RepartitorDestinatie;
            linie.RepartitorCreditId = l.SursaPeDebit ? l.RepartitorDestinatie : l.Sursa.Repartitor;
            linie.ProdusId = l.Sursa.Material;
            linie.CodFunctionalId = l.Sursa.CodFunctional;
            linie.CodEconomicId = l.Sursa.CodEconomic;
            linie.SursaFinantareId = l.Sursa.SursaFinantare;
            linie.UnitateId = l.Sursa.Unitate;
            linie.ProiectId = l.Sursa.Proiect;
            linie.CentruCostId = l.Sursa.CentruCost;
            linie.Valoare = l.Valoare;
        }
        return rezultat with { Document = doc };
    }

    /// <summary>Raportul: ce s-ar închide și, dacă nu se poate, de ce. Nu scrie nimic.</summary>
    public static RezultatInchidere Previzualizeaza(IObjectSpace os, int an, int luna) =>
        Analizeaza(os, an, luna, cronologiaCaMotiv: true, inlocuieste: null);

    public static InchidereRezultat Genereaza(IObjectSpace os, int an, int luna, Guid unitateId) =>
        Incearca(os, an, luna, unitateId).Document;

    /// <summary>Gardianul de operare: liniile închid exact soldurile de acum, iar nicio lună ulterioară nu e închisă operat.</summary>
    public static IReadOnlyList<string> ValideazaOperare(IObjectSpace os, InchidereRezultat doc) {
        var erori = new List<string>();
        if (Stale(os, doc))
            erori.Add("Soldurile conturilor de rezultat s-au schimbat de la generare (sau politica închiderii) — "
                + "ștergeți draftul și regenerați închiderea.");

        var data0 = doc.Data;
        var ulterioara = os.GetObjectsQuery<InchidereRezultat>()
            .Where(d => d.Data > data0 && d.Stare == StareDocument.Operat)
            .OrderBy(d => d.Data)
            .Select(d => new { d.Numar, d.Data })
            .FirstOrDefault();
        if (ulterioara != null)
            erori.Add($"Există o închidere a conturilor de rezultat operată pentru o lună ulterioară ({ulterioara.Numar}, "
                + $"{ulterioara.Data:dd.MM.yyyy}) — a opera acum închiderea lunii {doc.Data.Month:00}/{doc.Data.Year} "
                + "ar închide aceleași solduri a doua oară. Stornați-o pe cea ulterioară sau ștergeți acest draft.");
        return erori;
    }

    /// <summary>Draftul nu mai închide exact soldurile de acum (același criteriu ca la operare).</summary>
    public static bool Stale(IObjectSpace os, InchidereRezultat doc) {
        var data = InchidereTvaService.DataSoldurilor(doc.Data, doc.DataInregistrare, doc.CorecteazaId);
        return !CalculeazaLinii(os, data, cuAnuale: doc.Data.Month == 12)
            .Select(l => Amprenta(l.SursaPeDebit, l.Sursa, l.ContDestinatie, l.RepartitorDestinatie, l.Pastrate, l.Valoare))
            .OrderBy(a => a).SequenceEqual(Amprente(doc));
    }

    static List<string> Amprente(InchidereRezultat doc) =>
        doc.Detalii.OfType<InchidereRezultatDetaliu>().Select(d => {
            var sursa = new CheieSold(
                (d.SursaPeDebit ? d.ContDebitId : d.ContCreditId) ?? Guid.Empty,
                d.SursaPeDebit ? d.RepartitorDebitId : d.RepartitorCreditId,
                d.ProdusId, d.CodFunctionalId, d.CodEconomicId, d.SursaFinantareId, d.UnitateId, d.ProiectId, d.CentruCostId);
            return Amprenta(d.SursaPeDebit, sursa, (d.SursaPeDebit ? d.ContCreditId : d.ContDebitId) ?? Guid.Empty,
                d.SursaPeDebit ? d.RepartitorCreditId : d.RepartitorDebitId, d.CoordonatePastrate, d.Valoare);
        }).Concat(doc.Detalii.Where(d => d is not InchidereRezultatDetaliu).Select(d => $"linie străină {d.ID}"))
        .OrderBy(a => a).ToList();

    static string Amprenta(bool sursaPeDebit, CheieSold sursa, Guid destinatie, Guid? repartitorDestinatie,
            DimensiuneFlags pastrate, decimal valoare) =>
        $"{sursaPeDebit}|{sursa}|{destinatie}|{repartitorDestinatie}|{(int)pastrate}|{valoare:0.00}";

    static RezultatInchidere Analizeaza(IObjectSpace os, int an, int luna, bool cronologiaCaMotiv, Guid? inlocuieste) {
        var primaZi = new DateOnly(an, luna, 1);
        var ultimaZi = UltimaZi(an, luna);

        if (!os.GetObjectsQuery<PoliticaInchidereConturi>().Any())
            return new(null, MotivNegenerare.ProfilInert, null, []);

        var linii = CalculeazaLinii(os, ultimaZi, cuAnuale: luna == 12);
        RezultatInchidere Refuzat(MotivNegenerare motiv, Guid? blocantId) => new(null, motiv, blocantId, linii);

        var vie = os.GetObjectsQuery<InchidereRezultat>()
            .Where(d => d.Data >= primaZi && d.Data <= ultimaZi && d.Stare != StareDocument.Stornat
                && (inlocuieste == null || d.ID != inlocuieste))
            .Select(d => (Guid?)d.ID)
            .FirstOrDefault();
        if (vie != null)
            return Refuzat(MotivNegenerare.InchidereVie, vie);

        var ulterioara = os.GetObjectsQuery<InchidereRezultat>()
            .Where(d => d.Data > ultimaZi && d.Stare != StareDocument.Stornat)
            .Select(d => (Guid?)d.ID)
            .FirstOrDefault();
        if (ulterioara != null) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"Există o închidere a conturilor de rezultat vie pentru o lună ulterioară lui {luna:00}/{an} — "
                    + "închiderile se generează cronologic.");
            return Refuzat(MotivNegenerare.NeCronologica, ulterioara);
        }

        var anterior = os.GetObjectsQuery<InchidereRezultat>()
            .Where(d => d.Data < primaZi && d.Stare == StareDocument.Draft
                && (inlocuieste == null || d.ID != inlocuieste))
            .OrderBy(d => d.Data)
            .Select(d => (Guid?)d.ID)
            .FirstOrDefault();
        if (anterior != null) {
            if (!cronologiaCaMotiv)
                throw new OperareException(
                    $"O lună anterioară lui {luna:00}/{an} are un draft de închidere a conturilor de rezultat neoperat — "
                    + "operați-l sau ștergeți-l întâi; închiderile se generează cronologic.");
            return Refuzat(MotivNegenerare.DraftAnterior, anterior);
        }

        try {
            GardianPerioada.VerificaDeschisa(os, ultimaZi);
        }
        catch (OperareException) when (cronologiaCaMotiv) {
            return Refuzat(MotivNegenerare.PerioadaInchisa, null);
        }

        if (linii.Count == 0)
            return Refuzat(MotivNegenerare.FaraSold, null);
        return new(null, null, null, linii);
    }

    static HashSet<Guid> Descendenti(Guid radacina, ILookup<Guid, Guid> copii) {
        var rezultat = new HashSet<Guid> { radacina };
        var deVizitat = new Stack<Guid>([radacina]);
        while (deVizitat.Count > 0)
            foreach (var copil in copii[deVizitat.Pop()])
                if (rezultat.Add(copil))
                    deVizitat.Push(copil);
        return rezultat;
    }

    static Dictionary<Guid, string> SimboluriLinii(IObjectSpace os, IReadOnlyList<LinieInchidere> linii) {
        var ids = linii.SelectMany(l => new[] { l.Sursa.Cont, l.ContDestinatie }).Distinct().ToList();
        return os.GetObjectsQuery<Cont>().Where(c => ids.Contains(c.ID)).ToDictionary(c => c.ID, c => c.Simbol);
    }

    static DateOnly UltimaZi(int an, int luna) => new(an, luna, DateTime.DaysInMonth(an, luna));
}
