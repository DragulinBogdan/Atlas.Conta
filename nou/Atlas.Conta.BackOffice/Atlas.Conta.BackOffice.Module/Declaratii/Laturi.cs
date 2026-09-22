#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Partea repartitorului față de patrimoniu, derivată din <see cref="FelRepartitor"/> (T-D13).</summary>
[Flags]
public enum Parte {
    Niciuna = 0,
    Extern = 1,
    Intern = 2,
    Propriu = 4,
}

/// <summary>
/// Contractul unei laturi: părțile permise, calitatea cerută și, doar unde structura o
/// cere (lotul trăiește într-o <see cref="BusinessObjects.Gestiune"/>), felul exact.
/// </summary>
public readonly record struct Latura(
        Parte Permisa,
        CalitateRepartitor Calitate = CalitateRepartitor.Niciuna,
        FelRepartitor? Fel = null) {
    public static readonly Latura Externa = new(Parte.Extern);
    public static readonly Latura Interna = new(Parte.Intern);
    public static readonly Latura Proprie = new(Parte.Propriu);
    public static readonly Latura Gestiune = new(Parte.Intern, Fel: FelRepartitor.Gestiune);
    public static readonly Latura InternaSauProprie = new(Parte.Intern | Parte.Propriu);
    public static readonly Latura ExternaSauProprie = new(Parte.Extern | Parte.Propriu);
    public static readonly Latura ExternaSauInterna = new(Parte.Extern | Parte.Intern);

    public Latura Cu(CalitateRepartitor calitate) => this with { Calitate = calitate };

    public bool Admite(RepartitorFapt repartitor) =>
        repartitor.Parte is Parte parte
        && (Permisa & parte) != 0
        && (Fel is null || repartitor.Fel == Fel)
        && (Calitate == CalitateRepartitor.Niciuna || repartitor.Calitati.HasFlag(Calitate));
}

/// <summary>Laturile permise ale unui tip de document: structură, nu politică (decizia 4).</summary>
public sealed record ContractLaturi(Latura Predator, Latura Primitor);

public static class Laturi {
    public static Parte? ParteA(FelRepartitor? fel) => fel switch {
        FelRepartitor.Partener or FelRepartitor.Angajat => Parte.Extern,
        FelRepartitor.Gestiune or FelRepartitor.UnitateInterna => Parte.Intern,
        FelRepartitor.ContPropriu => Parte.Propriu,
        _ => null,
    };

    /// <summary>Refuzurile contractului pe faptele laturilor: aceeași funcție pe ambele uși.</summary>
    public static void Verifica(ContractLaturi contract, RepartitorFapt predator, RepartitorFapt primitor,
            ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(predator);
        ArgumentNullException.ThrowIfNull(primitor);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (!contract.Predator.Admite(predator))
            refuzuri.Add(new N.Refuz(CoduriRefuz.PredatorNepotrivit,
                Mesaj("Predatorul", contract.Predator, predator), null));
        if (!contract.Primitor.Admite(primitor))
            refuzuri.Add(new N.Refuz(CoduriRefuz.PrimitorNepotrivit,
                Mesaj("Primitorul", contract.Primitor, primitor), null));
    }

    /// <summary>Ușa entității (validarea de operare): faptele laturilor citite din bază, apoi aceeași verificare.</summary>
    public static IReadOnlyList<N.Refuz> Verifica(IObjectSpace os, Document doc) {
        ArgumentNullException.ThrowIfNull(doc);
        var refuzuri = new List<N.Refuz>();
        var (predator, primitor) = Motor.Fapte.Laturile(os, doc);
        Verifica(doc.Laturi(), predator, primitor, refuzuri);
        return refuzuri;
    }

    static string Mesaj(string latura, Latura ceruta, RepartitorFapt real) =>
        $"{latura} trebuie să fie {Descrie(ceruta)}; e {Descrie(real, ceruta.Calitate)}.";

    static string Descrie(Latura latura) {
        if (latura.Fel == FelRepartitor.Gestiune)
            return "o gestiune";
        if (latura.Fel is FelRepartitor fel)
            return $"un repartitor de felul {fel}";
        var parti = new List<string>(3);
        if (latura.Permisa.HasFlag(Parte.Propriu))
            parti.Add("un cont propriu (casă/bancă)");
        if (latura.Permisa.HasFlag(Parte.Intern))
            parti.Add("un repartitor intern (gestiune sau unitate internă)");
        if (latura.Permisa.HasFlag(Parte.Extern))
            parti.Add("un terț (partener sau angajat)");
        var text = string.Join(" sau ", parti);
        return latura.Calitate == CalitateRepartitor.Niciuna ? text : $"{text} cu calitatea {latura.Calitate}";
    }

    static string Descrie(RepartitorFapt real, CalitateRepartitor calitate) =>
        real.Fel is not FelRepartitor fel ? "lipsă"
        : calitate != CalitateRepartitor.Niciuna && !real.Calitati.HasFlag(calitate) ? $"{fel} fără calitatea {calitate}"
        : fel.ToString();
}
