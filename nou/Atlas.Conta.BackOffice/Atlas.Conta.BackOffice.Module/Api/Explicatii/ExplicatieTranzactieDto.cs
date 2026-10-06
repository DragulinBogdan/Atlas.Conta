#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>Accesul complet cerut de citirea explicației (X-D4 c): soldurile unităților și valorile altor linii.</summary>
public static class ExplicatieAcces {
    public const string Refuz = "EXPLICATIE_ACCES_INCOMPLET";

    public static readonly Type[] Citite = [
        typeof(C.Tranzactie), typeof(C.Postare), typeof(Document), typeof(DocumentDetaliu), typeof(Lot),
        typeof(Produs), typeof(Cont), typeof(Repartitor),
    ];
}

public sealed class ExplicatieTranzactieDto {
    public Guid TranzactieId { get; set; }
    public string Fel { get; set; } = "";
    public Guid? DocumentId { get; set; }
    /// <summary>Goală pe împerechere, desfacere și deschidere; la storno, explicațiile originalelor.</summary>
    public List<ExplicatieContractDto> Origini { get; set; } = [];

    public static ExplicatieTranzactieDto Din(ExplicatieTranzactie citita) => new() {
        TranzactieId = citita.Tranzactie, Fel = citita.Fel.ToString(), DocumentId = citita.Document,
        Origini = [.. citita.Origini.Select(o => ExplicatieContractDto.Din(o.Purtator, o.Explicatie))],
    };
}

public sealed class ExplicatieContractDto {
    public Guid PurtatorId { get; set; }
    public int Versiune { get; set; }
    public string Declarant { get; set; } = "";
    public int JumatatiDeBan { get; set; }
    public int? PerioadaAn { get; set; }
    public int? PerioadaLuna { get; set; }
    public string? Politica { get; set; }
    public DateOnly? PoliticaValabilaDeLa { get; set; }
    public List<ExplicatieLinieDto> Linii { get; set; } = [];

    public static ExplicatieContractDto Din(Guid purtator, C.Explicatie e) {
        var perioada = e.Ipoteze.OfType<N.PerioadaDeschisa>().FirstOrDefault();
        var politica = e.Ipoteze.OfType<N.VersiunePolitica>().FirstOrDefault();
        return new() {
            PurtatorId = purtator, Versiune = C.Explicatie.Versiune, Declarant = e.Declarant,
            JumatatiDeBan = e.JumatatiDeBan, PerioadaAn = perioada?.An, PerioadaLuna = perioada?.Luna,
            Politica = politica?.Nume, PoliticaValabilaDeLa = politica?.ValabilDeLa,
            Linii = [.. e.Linii().Select(l => new ExplicatieLinieDto {
                LinieId = l.Linie,
                Iesiri = [.. l.Iesiri.Select(i => new ExplicatieIesireDto {
                    Unitate = ExplicatieUnitateDto.Din(i.Unitate), Cantitate = i.Cantitate, Valoare = i.Valoare,
                    SoldInainte = ExplicatieSoldDto.Din(i.SoldInainte), Sursa = i.Sursa,
                })],
                Stingeri = [.. l.Stingeri.Select(s => new ExplicatieStingereDto {
                    Unitate = ExplicatieUnitateDto.Din(s.Unitate), Masura = s.Masura,
                    SoldCitit = ExplicatieSoldDto.Din(s.SoldCitit),
                })],
                Conturi = [.. l.Conturi.Select(c => new ExplicatieContDto { ContId = c.Cont, Sursa = c.Sursa })],
                PartideDeschise = [.. l.PartideDeschise.Select(p => ExplicatieUnitateDto.Din(p.Unitate))],
            })],
        };
    }
}

public sealed class ExplicatieLinieDto {
    public Guid LinieId { get; set; }
    public List<ExplicatieIesireDto> Iesiri { get; set; } = [];
    public List<ExplicatieStingereDto> Stingeri { get; set; } = [];
    public List<ExplicatieContDto> Conturi { get; set; } = [];
    public List<ExplicatieUnitateDto> PartideDeschise { get; set; } = [];
}

public sealed class ExplicatieUnitateDto {
    public Guid Id { get; set; }
    public string Fel { get; set; } = "";
    public Guid ContId { get; set; }
    public Guid? PartenerId { get; set; }
    public Guid? ProdusId { get; set; }
    public DateOnly Deschisa { get; set; }

    public static ExplicatieUnitateDto Din(N.Unitate u) => new() {
        Id = u.Id, Fel = u.Fel.ToString(), ContId = u.Cont, PartenerId = u.Partener, ProdusId = u.Produs,
        Deschisa = u.Deschisa,
    };
}

public sealed class ExplicatieSoldDto {
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Cantitate { get; set; }
    public decimal ValoareValuta { get; set; }

    public static ExplicatieSoldDto? Din(N.Sold? s) => s is null ? null : new() {
        Debit = s.Debit, Credit = s.Credit, Cantitate = s.Cantitate, ValoareValuta = s.ValoareValuta,
    };
}

/// <summary>Ieșirea pe o unitate: evaluată din <see cref="SoldInainte"/> sau declarată de <see cref="Sursa"/>.</summary>
public sealed class ExplicatieIesireDto {
    public ExplicatieUnitateDto Unitate { get; set; } = new();
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
    public ExplicatieSoldDto? SoldInainte { get; set; }
    public string? Sursa { get; set; }
}

public sealed class ExplicatieStingereDto {
    public ExplicatieUnitateDto Unitate { get; set; } = new();
    public decimal Masura { get; set; }
    public ExplicatieSoldDto? SoldCitit { get; set; }
}

public sealed class ExplicatieContDto {
    public Guid ContId { get; set; }
    public string Sursa { get; set; } = "";
}
