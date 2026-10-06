#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public sealed record ExplicatiePurtata(Guid Purtator, Explicatie Explicatie);

/// <summary>
/// Explicațiile care acoperă o tranzacție: a ei, cea referită sau, la storno, ale originalelor.
/// Lista e goală pe împerechere, desfacere și deschidere.
/// </summary>
public sealed record ExplicatieTranzactie(
    Guid Tranzactie, N.FelTranzactie Fel, Guid? Document, IReadOnlyList<ExplicatiePurtata> Origini);

public static class Explicatii {
    public const string Lipsa = "CITIRE_EXPLICATIE_LIPSA";
    public const string Referinta = "CITIRE_EXPLICATIE_REFERINTA";
    public const string Iesire = "CITIRE_EXPLICATIE_IESIRE";
    public const string Evaluare = "CITIRE_EXPLICATIE_EVALUARE";
    public const string Linie = "CITIRE_EXPLICATIE_LINIE";
    public const string Mecanism = "CITIRE_EXPLICATIE_MECANISM";
    public const string Stingere = "CITIRE_EXPLICATIE_STINGERE";
    public const string Storno = "CITIRE_EXPLICATIE_STORNO";

    public static bool Vizibila(IObjectSpace os, Guid tranzactie) =>
        os.GetObjectsQuery<Tranzactie>().Any(t => t.ID == tranzactie);

    public static ExplicatieTranzactie? PeTranzactie(IObjectSpace os, Guid tranzactie) {
        var tranzactii = os.GetObjectsQuery<Tranzactie>();
        var rand = tranzactii.Where(t => t.ID == tranzactie)
            .Select(t => new { t.Fel, t.DocumentId }).FirstOrDefault();
        if (rand is null) return null;
        var surse = tranzactii.Where(t => t.ID == tranzactie);
        if (rand.Fel == N.FelTranzactie.Storno) {
            var postari = os.GetObjectsQuery<Postare>();
            var inverse = postari.Where(p => p.TranzactieId == tranzactie && p.InversaDinId != null);
            var originale = postari.Where(o => inverse.Any(i => i.InversaDinId == o.ID && i.InversaDinSpatiu == o.Spatiu))
                .Select(o => o.TranzactieId);
            surse = tranzactii.Where(t => originale.Contains(t.ID));
        }
        var purtatori = surse.Select(t => t.Explicatie != null ? (Guid?)t.ID : t.ExplicatieDinId)
            .Where(id => id != null).Distinct().ToList();
        var origini = tranzactii.Where(t => purtatori.Contains(t.ID) && t.Explicatie != null)
            .OrderBy(t => t.ScrisLa).ThenBy(t => t.ID)
            .Select(t => new { t.ID, t.Explicatie }).ToList()
            .Select(t => new ExplicatiePurtata(t.ID, Explicatie.Citeste(t.Explicatie))).ToList();
        return new(tranzactie, rand.Fel, rand.DocumentId, origini);
    }

    /// <summary>Invariantul de audit al explicației (X-D4 d), pe baza întreagă.</summary>
    public static void VerificaAcoperire(IObjectSpace os) {
        var tranzactii = os.GetObjectsQuery<Tranzactie>();
        var postari = os.GetObjectsQuery<Postare>();

        var neexplicate = tranzactii.LongCount(t => t.Fel == N.FelTranzactie.Operare && t.DocumentId != null && t.Explicatie == null);
        if (neexplicate != 0)
            throw new OperareException($"{Lipsa}: {neexplicate} tranzacții Operare fără explicația contractului.");

        var rupte = tranzactii.LongCount(t => t.ExplicatieDinId != null
            && !tranzactii.Any(p => p.ID == t.ExplicatieDinId && p.Explicatie != null && p.DocumentId == t.DocumentId));
        if (rupte != 0)
            throw new OperareException($"{Referinta}: {rupte} tranzacții referă o explicație absentă sau a altui document.");

        var neoglindite = postari.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Storno && p.FelUnitate == N.FelUnitate.Lot)
            .LongCount(p => !postari.Any(o => o.ID == p.InversaDinId && o.Spatiu == p.InversaDinSpatiu
                && o.Unitate == p.Unitate && o.Cantitate == -p.Cantitate && o.Valoare == -p.Valoare));
        if (neoglindite != 0)
            throw new OperareException($"{Storno}: {neoglindite} inverse pe lot nu oglindesc postarea explicată a originalului.");

        var purtatori = tranzactii.Where(t => t.Explicatie != null)
            .Select(t => new { t.ID, Document = t.DocumentId!.Value, t.Explicatie }).ToList();
        var referinte = tranzactii.Where(t => t.ExplicatieDinId != null)
            .Select(t => new { t.ID, Purtator = t.ExplicatieDinId!.Value }).ToDictionary(t => t.ID, t => t.Purtator);
        var cuExplicatie = purtatori.Select(p => p.ID).ToHashSet();
        Guid? Purtator(Guid tranzactie) =>
            cuExplicatie.Contains(tranzactie) ? tranzactie : referinte.TryGetValue(tranzactie, out var p) ? p : null;

        var iesiri = postari.Where(p => p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Lot && p.Cantitate < 0m
                && (p.Tranzactie.Fel == N.FelTranzactie.Operare || p.Tranzactie.Fel == N.FelTranzactie.Transfer))
            .Select(p => new { p.TranzactieId, p.LinieId, Unitate = p.Unitate!.Value, p.Cont, p.Latura, p.Cantitate, p.Valoare })
            .ToList()
            .ToLookup(p => Purtator(p.TranzactieId), p => new IesirePeLot(p.LinieId, p.Unitate, p.Cont,
                -p.Cantitate, p.Latura == N.Latura.Credit ? p.Valoare : -p.Valoare));
        if (iesiri[null].Any())
            throw new OperareException($"{Lipsa}: {iesiri[null].Count()} ieșiri pe lot în tranzacții fără explicație.");

        var pePartide = postari.Where(p => p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida
                && p.Partener != null && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .Select(p => new { p.TranzactieId, p.LinieId, Unitate = p.Unitate!.Value, p.Cont,
                Partener = p.Partener!.Value, Deschisa = p.UnitateDeschisa!.Value, p.Valoare })
            .ToList().ToLookup(p => Purtator(p.TranzactieId));

        var documente = purtatori.Select(p => p.Document).Distinct().ToList();
        var declaranti = os.GetObjectsQuery<Document>().Where(d => documente.Contains(d.ID)).ToList()
            .ToDictionary(d => d.ID, d => d.Declarant());
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(l => documente.Contains(l.DocumentId))
            .Select(l => new { l.ID, l.Cantitate, l.Valoare }).ToList()
            .ToDictionary(l => l.ID, l => (l.Cantitate, l.Valoare));

        var rotunjire = new N.Rotunjire(Scara.ConventieBani);
        foreach (var purtator in purtatori) {
            var explicatie = Explicatie.Citeste(purtator.Explicatie);

            var declarant = declaranti.GetValueOrDefault(purtator.Document);
            if (declarant is null || declarant.GetType().Name != explicatie.Declarant)
                throw new OperareException($"{Mecanism}: tranzacția {purtator.ID}: explicația numește declarantul "
                    + $"{explicatie.Declarant}, documentul declară prin {declarant?.GetType().Name ?? "niciunul"}.");
            var sursa = declarant.SursaValoareDeclarata;
            if (explicatie.Decizii.Any(d => sursa is null ? d is N.ValoareDeclarata
                    : d is N.ValoareIesire || d is N.ValoareDeclarata v && v.Sursa != sursa))
                throw new OperareException($"{Mecanism}: tranzacția {purtator.ID}: {explicatie.Declarant} "
                    + (sursa is null ? "evaluează ieșirile din sold; explicația poartă o valoare declarată."
                        : $"declară valoarea din sursa {sursa}; explicația poartă alt mecanism."));

            var decise = explicatie.Decizii.Select(d => d switch {
                N.ValoareIesire i => new IesirePeLot(i.Linie, i.Unitate.Id, i.Unitate.Cont, i.Cantitate, i.Valoare),
                N.ValoareDeclarata i => new IesirePeLot(i.Linie, i.Unitate.Id, i.Unitate.Cont, i.Cantitate, i.Valoare),
                _ => null,
            }).OfType<IesirePeLot>().ToList();
            var postate = iesiri[purtator.ID].ToList();
            if (decise.Count != postate.Count || decise.Except(postate).Any()
                    || decise.Select(d => (d.Linie, d.Unitate, d.Cont)).Distinct().Count() != decise.Count)
                throw new OperareException($"{Iesire}: tranzacția {purtator.ID} are {postate.Count} ieșiri pe lot și "
                    + $"{decise.Count} decizii de valoare; fiecare ieșire cere exact o decizie cu linia, unitatea, cantitatea și valoarea ei.");

            foreach (var (iesire, inainte) in explicatie.IesiriEvaluate())
                if (inainte is null || !Evaluata(inainte, iesire, rotunjire))
                    throw new OperareException($"{Evaluare}: tranzacția {purtator.ID}, linia {iesire.Linie}: valoarea "
                        + $"{iesire.Valoare} nu rezultă din soldul citit al unității {iesire.Unitate.Id}.");

            // D9-D3 (a): linia poartă valoarea deciziei, cu semnul cantității ei.
            foreach (var peLinie in explicatie.Decizii.OfType<N.ValoareIesire>().GroupBy(i => i.Linie)) {
                var decisa = peLinie.Sum(i => i.Valoare);
                if (!linii.TryGetValue(peLinie.Key, out var linie) || linie.Valoare != Math.Sign(linie.Cantitate) * decisa)
                    throw new OperareException($"{Linie}: tranzacția {purtator.ID}, linia {peLinie.Key}: valoarea liniei "
                        + $"({(linii.ContainsKey(peLinie.Key) ? linie.Valoare : "absentă")}) nu e valoarea decisă a ieșirii ({decisa}).");
            }

            foreach (var pePartida in explicatie.Decizii.OfType<N.AlocareFifo>().GroupBy(a => (a.Unitate.Id, a.Unitate.Cont))) {
                var citit = explicatie.SoldCitit(pePartida.First().Unitate);
                if (citit is null || pePartida.Sum(a => Math.Abs(a.Masura)) > Math.Abs(citit.Net))
                    throw new OperareException($"{Stingere}: tranzacția {purtator.ID}: alocarea FIFO de "
                        + $"{pePartida.Sum(a => Math.Abs(a.Masura))} pe partida {pePartida.Key.Id} nu rezultă din soldul ei citit.");
            }

            var alocate = explicatie.Decizii.OfType<N.AlocareFifo>()
                .GroupBy(a => ((Guid?)a.Linie, a.Unitate.Id)).ToDictionary(g => g.Key, g => g.Sum(a => a.Masura));
            var stinse = pePartide[purtator.ID].GroupBy(p => (p.LinieId, p.Unitate)).ToList();
            foreach (var alocare in alocate)
                if (stinse.FirstOrDefault(g => g.Key == alocare.Key)?.Sum(p => Math.Abs(p.Valoare)) != alocare.Value)
                    throw new OperareException($"{Stingere}: tranzacția {purtator.ID}: alocarea FIFO de {alocare.Value} pe "
                        + $"partida {alocare.Key.Id} nu are postarea ei.");
            foreach (var grup in stinse.Where(g => !alocate.ContainsKey(g.Key))) {
                var p = grup.First();
                if (!IdentitatiPartide.EsteProprie(
                        new N.Unitate(p.Unitate, N.FelUnitate.Partida, p.Cont, p.Partener, null, p.Deschisa), purtator.Document))
                    throw new OperareException($"{Stingere}: tranzacția {purtator.ID}: stingerea partidei {p.Unitate} "
                        + "nu are `AlocareFifo`.");
            }
        }
    }

    static bool Evaluata(N.Sold inainte, N.ValoareIesire iesire, N.Rotunjire rotunjire) {
        try { return N.Evaluare.Iesire(inainte, iesire.Cantitate, rotunjire) == iesire.Valoare; }
        catch (N.RefuzException) { return false; }
        catch (ArgumentException) { return false; }
    }

    sealed record IesirePeLot(Guid? Linie, Guid Unitate, Guid Cont, decimal Cantitate, decimal Valoare);
}
