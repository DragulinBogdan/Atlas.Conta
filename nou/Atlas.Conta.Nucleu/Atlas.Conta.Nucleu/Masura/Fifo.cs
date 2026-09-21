namespace Atlas.Conta.Nucleu;

public sealed record Disponibil(Unitate Unitate, decimal Masura);

public sealed record Pin(Guid Unitate, decimal Masura);

public sealed record Alocare(Unitate Unitate, decimal Masura);

public sealed record Nominalizare(IReadOnlyList<Alocare> Alocari, decimal Ramas);

public static class Fifo {
    public static Nominalizare Nominalizeaza(
        decimal cerere,
        IReadOnlyList<Disponibil> candidati,
        IReadOnlyList<Pin>? pinuri = null) {
        ArgumentNullException.ThrowIfNull(candidati);
        if (cerere <= 0m)
            throw new ArgumentException($"cererea {cerere} se cere > 0.", nameof(cerere));
        var unitati = new Dictionary<Guid, Unitate>();
        var disponibil = new Dictionary<Guid, decimal>();
        var ordine = new List<Unitate>();
        foreach (var candidat in candidati) {
            ArgumentNullException.ThrowIfNull(candidat);
            if (!unitati.TryAdd(candidat.Unitate.Id, candidat.Unitate))
                throw new ArgumentException(
                    $"unitatea {candidat.Unitate.Id} apare de două ori între candidați.",
                    nameof(candidati));
            if (candidat.Masura <= 0m)
                continue;
            disponibil.Add(candidat.Unitate.Id, candidat.Masura);
            ordine.Add(candidat.Unitate);
        }
        var numite = pinuri ?? [];
        foreach (var pin in numite) {
            ArgumentNullException.ThrowIfNull(pin, nameof(pinuri));
            if (!unitati.ContainsKey(pin.Unitate))
                throw new ArgumentException(
                    $"pin-ul numește unitatea {pin.Unitate}, care nu e între candidați.",
                    nameof(pinuri));
            if (pin.Masura <= 0m)
                throw new ArgumentException(
                    $"pin-ul pe unitatea {pin.Unitate} are măsura {pin.Masura}, se cere > 0.",
                    nameof(pinuri));
        }
        var alocari = new List<Alocare>();
        var alocat = new Dictionary<Guid, decimal>();
        var ramas = cerere;
        // N-D7: unitatea numită se consumă ÎNTÂI și nu cade pe FIFO când nu ajunge.
        foreach (var pin in numite)
            Aloca(unitati[pin.Unitate], pin.Masura, disponibil, alocat, alocari, ref ramas);
        ordine.Sort(Intai);
        foreach (var unitate in ordine)
            Aloca(unitate, decimal.MaxValue, disponibil, alocat, alocari, ref ramas);
        return new Nominalizare(alocari, ramas);
    }

    public static int Intai(Unitate unul, Unitate altul) {
        ArgumentNullException.ThrowIfNull(unul);
        ArgumentNullException.ThrowIfNull(altul);
        var peData = unul.Deschisa.CompareTo(altul.Deschisa);
        return peData != 0 ? peData : unul.Id.CompareTo(altul.Id);
    }

    static void Aloca(
        Unitate unitate,
        decimal plafon,
        Dictionary<Guid, decimal> disponibil,
        Dictionary<Guid, decimal> alocat,
        List<Alocare> alocari,
        ref decimal ramas) {
        var liber = disponibil.GetValueOrDefault(unitate.Id) - alocat.GetValueOrDefault(unitate.Id);
        var masura = Math.Min(Math.Min(plafon, liber), ramas);
        if (masura <= 0m)
            return;
        alocari.Add(new Alocare(unitate, masura));
        alocat[unitate.Id] = alocat.GetValueOrDefault(unitate.Id) + masura;
        ramas -= masura;
    }
}
