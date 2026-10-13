#nullable enable
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Ieșirea unei linii pe o unitate: evaluată din <paramref name="SoldInainte"/> sau declarată de <paramref name="Sursa"/>.</summary>
public sealed record IesireExplicata(N.UnitateSold Unitate, decimal Cantitate, decimal Valoare, N.Sold? SoldInainte, string? Sursa);

/// <summary>Stingerea FIFO a unei linii pe o partidă, cu soldul citit al partidei.</summary>
public sealed record StingereExplicata(N.UnitateSold Unitate, decimal Masura, N.Sold? SoldCitit);

public sealed record LinieExplicata(
    Guid Linie,
    IReadOnlyList<IesireExplicata> Iesiri,
    IReadOnlyList<StingereExplicata> Stingeri,
    IReadOnlyList<N.ContRezolvat> Conturi,
    IReadOnlyList<N.PartidaDeschisa> PartideDeschise);

/// <summary>Explicația unui contract acceptat (090 j): deciziile și ipotezele lui, în ordinea declarației.</summary>
public sealed record Explicatie(
        string Declarant,
        int JumatatiDeBan,
        IReadOnlyList<N.Decizie> Decizii,
        IReadOnlyList<N.Ipoteza> Ipoteze) {
    public const int Versiune = 2;

    public static Explicatie Din(N.Contract contract, string declarant) {
        ArgumentNullException.ThrowIfNull(contract);
        return new(declarant, contract.JumatatiDeBan, contract.Decizii, contract.Ipoteze);
    }

    public bool Equals(Explicatie? alta) =>
        alta is not null && Declarant == alta.Declarant && JumatatiDeBan == alta.JumatatiDeBan
        && Decizii.SequenceEqual(alta.Decizii) && Ipoteze.SequenceEqual(alta.Ipoteze);

    public override int GetHashCode() => HashCode.Combine(Declarant, JumatatiDeBan, Decizii.Count, Ipoteze.Count);

    /// <summary>Ieșirile evaluate din sold, în ordinea deciziilor, fiecare cu soldul unității dinaintea ei.</summary>
    public IEnumerable<(N.ValoareIesire Iesire, N.Sold? Inainte)> IesiriEvaluate() {
        var curente = new Dictionary<(Guid, Guid), N.Sold?>();
        foreach (var iesire in Decizii.OfType<N.ValoareIesire>()) {
            var cheie = (iesire.Unitate.Id, iesire.Unitate.Cont);
            if (!curente.TryGetValue(cheie, out var inainte))
                inainte = SoldCitit(iesire.Unitate);
            yield return (iesire, inainte);
            curente[cheie] = inainte is null ? null
                : inainte with { Credit = inainte.Credit + iesire.Valoare, Cantitate = inainte.Cantitate - iesire.Cantitate };
        }
    }

    public N.Sold? SoldCitit(N.UnitateSold unitate) =>
        Ipoteze.OfType<N.SoldUnitateCitit>()
            .FirstOrDefault(i => i.Unitate.Id == unitate.Id && i.Unitate.Cont == unitate.Cont)?.Sold;

    /// <summary>„De ce acest lot”: deciziile grupate pe linie, în ordinea primei apariții.</summary>
    public IReadOnlyList<LinieExplicata> Linii() {
        var evaluate = IesiriEvaluate().ToDictionary(p => p.Iesire, p => p.Inainte, ReferenceEqualityComparer.Instance);
        var linii = new List<Guid>();
        foreach (var linie in Decizii.Select(Linia).OfType<Guid>())
            if (!linii.Contains(linie)) linii.Add(linie);
        return [.. linii.Select(linie => {
            var ale = Decizii.Where(d => Linia(d) == linie).ToList();
            return new LinieExplicata(linie,
                [.. ale.Select(d => d switch {
                    N.ValoareIesire i => new IesireExplicata(i.Unitate, i.Cantitate, i.Valoare, (N.Sold?)evaluate[i], null),
                    N.ValoareDeclarata i => new IesireExplicata(i.Unitate, i.Cantitate, i.Valoare, null, i.Sursa),
                    _ => null,
                }).OfType<IesireExplicata>()],
                [.. ale.OfType<N.AlocareFifo>().Select(a => new StingereExplicata(a.Unitate, a.Masura, SoldCitit(a.Unitate)))],
                [.. ale.OfType<N.ContRezolvat>()],
                [.. ale.OfType<N.PartidaDeschisa>()]);
        })];
    }

    static Guid? Linia(N.Decizie decizie) => decizie switch {
        N.AlocareFifo d => d.Linie,
        N.ValoareIesire d => d.Linie,
        N.ValoareDeclarata d => d.Linie,
        N.PartidaDeschisa d => d.Linie,
        N.ContRezolvat d => d.Linie,
        N.AlocareValorica d => d.Linie,
        N.CompensareValorica d => d.Linie,
        _ => null,
    };

    public string Scrie() {
        var tampon = new ArrayBufferWriter<byte>();
        using (var w = new Utf8JsonWriter(tampon)) {
            w.WriteStartObject();
            w.WriteNumber("v", Versiune);
            w.WriteString("declarant", Declarant);
            w.WriteNumber("jumatati", JumatatiDeBan);
            w.WriteStartArray("decizii");
            foreach (var decizie in Decizii) Scrie(w, decizie);
            w.WriteEndArray();
            w.WriteStartArray("ipoteze");
            foreach (var ipoteza in Ipoteze) Scrie(w, ipoteza);
            w.WriteEndArray();
            w.WriteEndObject();
        }
        return Encoding.UTF8.GetString(tampon.WrittenSpan);
    }

    public static Explicatie Citeste(string json) {
        ArgumentNullException.ThrowIfNull(json);
        using var document = JsonDocument.Parse(json);
        var e = document.RootElement;
        var versiune = e.GetProperty("v").GetInt32();
        if (versiune != Versiune)
            throw new InvalidOperationException($"Explicație în versiunea {versiune}; cititorul cunoaște versiunea {Versiune}.");
        return new(
            e.GetProperty("declarant").GetString()!,
            e.GetProperty("jumatati").GetInt32(),
            [.. e.GetProperty("decizii").EnumerateArray().Select(Decizia)],
            [.. e.GetProperty("ipoteze").EnumerateArray().Select(Ipoteza)]);
    }

    static void Scrie(Utf8JsonWriter w, N.Decizie decizie) {
        w.WriteStartObject();
        w.WriteString("fel", decizie.GetType().Name);
        switch (decizie) {
            case N.AlocareFifo d:
                w.WriteString("linie", d.Linie);
                Scrie(w, d.Unitate);
                w.WriteNumber("masura", d.Masura);
                break;
            case N.ValoareIesire d:
                w.WriteString("linie", d.Linie);
                Scrie(w, d.Unitate);
                w.WriteNumber("cantitate", d.Cantitate);
                w.WriteNumber("valoare", d.Valoare);
                break;
            case N.ValoareDeclarata d:
                w.WriteString("linie", d.Linie);
                Scrie(w, d.Unitate);
                w.WriteNumber("cantitate", d.Cantitate);
                w.WriteNumber("valoare", d.Valoare);
                w.WriteString("sursa", d.Sursa);
                break;
            case N.PartidaDeschisa d:
                w.WriteString("linie", d.Linie);
                Scrie(w, d.Unitate);
                break;
            case N.ContRezolvat d:
                w.WriteString("linie", d.Linie);
                w.WriteString("cont", d.Cont);
                w.WriteString("sursa", d.Sursa);
                if (d.Regula is Guid regula) w.WriteString("regula", regula);
                break;
            case N.AlocareValorica d:
                w.WriteString("linie", d.Linie);
                Scrie(w, d.Unitate);
                Scrie(w, "beneficiar", d.Beneficiar);
                w.WriteNumber("pondere", d.Pondere);
                w.WriteNumber("valoare", d.Valoare);
                break;
            case N.CompensareValorica d:
                w.WriteString("linie", d.Linie);
                Scrie(w, "radacina", d.Radacina);
                Scrie(w, "beneficiar", d.Beneficiar);
                Scrie(w, d.Sursa, "sursa");
                Scrie(w, d.Destinatie, "destinatie");
                w.WriteNumber("valoare", d.Valoare);
                break;
            default:
                throw new InvalidOperationException($"Decizia {decizie.GetType().Name} nu are formă persistată.");
        }
        w.WriteEndObject();
    }

    static N.Decizie Decizia(JsonElement e) => e.GetProperty("fel").GetString() switch {
        nameof(N.AlocareFifo) => new N.AlocareFifo(e.GetProperty("linie").GetGuid(), Unitatea(e), e.GetProperty("masura").GetDecimal()),
        nameof(N.ValoareIesire) => new N.ValoareIesire(e.GetProperty("linie").GetGuid(), Unitatea(e),
            e.GetProperty("cantitate").GetDecimal(), e.GetProperty("valoare").GetDecimal()),
        nameof(N.ValoareDeclarata) => new N.ValoareDeclarata(e.GetProperty("linie").GetGuid(), Unitatea(e),
            e.GetProperty("cantitate").GetDecimal(), e.GetProperty("valoare").GetDecimal(), e.GetProperty("sursa").GetString()!),
        nameof(N.PartidaDeschisa) => new N.PartidaDeschisa(e.GetProperty("linie").GetGuid(), Unitatea(e)),
        nameof(N.ContRezolvat) => new N.ContRezolvat(e.GetProperty("linie").GetGuid(), e.GetProperty("cont").GetGuid(),
            e.GetProperty("sursa").GetString()!, e.TryGetProperty("regula", out var regula) ? regula.GetGuid() : null),
        nameof(N.AlocareValorica) => new N.AlocareValorica(e.GetProperty("linie").GetGuid(), Unitatea(e), Referinta(e, "beneficiar"),
            e.GetProperty("pondere").GetDecimal(), e.GetProperty("valoare").GetDecimal()),
        nameof(N.CompensareValorica) => new N.CompensareValorica(e.GetProperty("linie").GetGuid(), Referinta(e, "radacina"),
            Referinta(e, "beneficiar"), Unitatea(e, "sursa"), Unitatea(e, "destinatie"), e.GetProperty("valoare").GetDecimal()),
        var fel => throw new InvalidOperationException($"Decizie necunoscută în explicație: {fel}."),
    };

    static void Scrie(Utf8JsonWriter w, N.Ipoteza ipoteza) {
        w.WriteStartObject();
        w.WriteString("fel", ipoteza.GetType().Name);
        switch (ipoteza) {
            case N.SoldUnitateCitit i:
                Scrie(w, i.Unitate);
                w.WriteStartObject("sold");
                w.WriteNumber("debit", i.Sold.Debit);
                w.WriteNumber("credit", i.Sold.Credit);
                w.WriteNumber("cantitate", i.Sold.Cantitate);
                w.WriteNumber("valoareValuta", i.Sold.ValoareValuta);
                w.WriteEndObject();
                break;
            case N.PerioadaDeschisa i:
                w.WriteNumber("an", i.An);
                w.WriteNumber("luna", i.Luna);
                break;
            case N.VersiunePolitica i:
                w.WriteString("politica", i.Fel);
                w.WriteString("rand", i.Rand);
                w.WriteNumber("versiune", i.Versiune);
                break;
            default:
                throw new InvalidOperationException($"Ipoteza {ipoteza.GetType().Name} nu are formă persistată.");
        }
        w.WriteEndObject();
    }

    static N.Ipoteza Ipoteza(JsonElement e) {
        switch (e.GetProperty("fel").GetString()) {
            case nameof(N.SoldUnitateCitit):
                var sold = e.GetProperty("sold");
                return new N.SoldUnitateCitit(Unitatea(e), new N.Sold(sold.GetProperty("debit").GetDecimal(),
                    sold.GetProperty("credit").GetDecimal(), sold.GetProperty("cantitate").GetDecimal(),
                    sold.GetProperty("valoareValuta").GetDecimal()));
            case nameof(N.PerioadaDeschisa):
                return new N.PerioadaDeschisa(e.GetProperty("an").GetInt32(), e.GetProperty("luna").GetInt32());
            case nameof(N.VersiunePolitica):
                return new N.VersiunePolitica(e.GetProperty("politica").GetString()!, e.GetProperty("rand").GetGuid(),
                    e.GetProperty("versiune").GetInt32());
            case var fel:
                throw new InvalidOperationException($"Ipoteză necunoscută în explicație: {fel}.");
        }
    }

    static void Scrie(Utf8JsonWriter w, string nume, N.ReferintaPostare referinta) {
        w.WriteStartObject(nume);
        w.WriteString("id", referinta.Id);
        w.WriteString("spatiu", referinta.Spatiu.ToString());
        w.WriteEndObject();
    }

    static N.ReferintaPostare Referinta(JsonElement parinte, string nume) {
        var e = parinte.GetProperty(nume);
        return new(e.GetProperty("id").GetGuid(), Enum.Parse<N.Spatiu>(e.GetProperty("spatiu").GetString()!));
    }

    static void Scrie(Utf8JsonWriter w, N.UnitateSold unitate, string nume = "unitate") {
        w.WriteStartObject(nume);
        w.WriteString("id", unitate.Id);
        w.WriteString("fel", unitate.Fel.ToString());
        w.WriteString("cont", unitate.Cont);
        if (unitate.Partener is Guid partener) w.WriteString("partener", partener);
        if (unitate.Produs is Guid produs) w.WriteString("produs", produs);
        w.WriteString("deschisa", Zi(unitate.Deschisa));
        w.WriteEndObject();
    }

    static N.UnitateSold Unitatea(JsonElement parinte, string nume = "unitate") {
        var e = parinte.GetProperty(nume);
        return new(e.GetProperty("id").GetGuid(), Enum.Parse<N.FelUnitate>(e.GetProperty("fel").GetString()!),
            e.GetProperty("cont").GetGuid(),
            e.TryGetProperty("partener", out var partener) ? partener.GetGuid() : null,
            e.TryGetProperty("produs", out var produs) ? produs.GetGuid() : null,
            Zi(e.GetProperty("deschisa")));
    }

    static string Zi(DateOnly zi) => zi.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    static DateOnly Zi(JsonElement e) => DateOnly.ParseExact(e.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
