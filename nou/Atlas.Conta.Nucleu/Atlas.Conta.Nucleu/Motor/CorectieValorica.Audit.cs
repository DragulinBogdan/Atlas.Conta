namespace Atlas.Conta.Nucleu;

/// <summary>Deciziile explicației persistate pe o tranzacție.</summary>
public sealed record ExplicatieValorica(Guid Tranzactie, IReadOnlyList<Decizie> Decizii);

public static partial class CorectieValorica {
    /// <summary>
    /// Invariantul de audit al explicației corecțiilor (CV-D8) pe faptele loturilor corectate:
    /// null dacă ține, altfel prima încălcare. <paramref name="valoriLinii"/> = valoarea liniilor de corecție;
    /// <paramref name="receptionate"/> = cantitatea recepționată a fiecărui lot.
    /// </summary>
    public static string? Auditeaza(IReadOnlyList<FaptPostat> fapte, IReadOnlyList<ExplicatieValorica> explicatii,
            IReadOnlyDictionary<Guid, decimal> valoriLinii, IReadOnlyDictionary<Guid, decimal> receptionate) {
        ArgumentNullException.ThrowIfNull(fapte);
        ArgumentNullException.ThrowIfNull(explicatii);
        ArgumentNullException.ThrowIfNull(valoriLinii);
        ArgumentNullException.ThrowIfNull(receptionate);
        try {
            return Audit(new Fapte(fapte), explicatii, valoriLinii, receptionate);
        }
        catch (CorectieValoricaRefuzata e) {
            return e.Message;
        }
    }

    static string? Audit(Fapte citite, IReadOnlyList<ExplicatieValorica> explicatii,
            IReadOnlyDictionary<Guid, decimal> valoriLinii, IReadOnlyDictionary<Guid, decimal> receptionate) {
        if (citite.Toate.Where(f => f.Fel == FelTranzactie.Storno && f.Postare.InversaDin is not null)
                .GroupBy(f => f.Postare.InversaDin!.Id).FirstOrDefault(g => g.Count() > 1) is { } dubla)
            return $"Postarea {dubla.Key} are {dubla.Count()} inverse.";
        var valorice = citite.Toate.Where(f => f.Fel == FelTranzactie.Operare && EsteValoric(f)).ToList();
        if (valorice.FirstOrDefault(f => f.Postare.Valoare == 0m) is { } goala)
            return $"Postarea valorică {goala.Id} nu are valoare.";

        var dupaTranzactie = explicatii.ToDictionary(e => e.Tranzactie);
        var compensate = new HashSet<Guid>();
        foreach (var tranzactie in valorice.Select(f => f.Tranzactie).Concat(dupaTranzactie.Keys).Distinct()) {
            var postate = valorice.Where(f => f.Tranzactie == tranzactie).ToList();
            if (!dupaTranzactie.TryGetValue(tranzactie, out var explicatie))
                return $"Tranzacția {tranzactie} are {postate.Count} postări valorice pe lot fără explicație proprie.";
            var alocari = explicatie.Decizii.OfType<AlocareValorica>().ToList();
            var compensari = explicatie.Decizii.OfType<CompensareValorica>().ToList();
            if (alocari.Count == 0 && compensari.Count == 0 && postate.Count == 0)
                continue;
            if (Explica(postate, alocari, compensari) is { } neexplicat)
                return $"Tranzacția {tranzactie}: {neexplicat}";
            foreach (var linie in alocari.GroupBy(a => a.Linie))
                if (Linia(citite, linie.Key, [.. linie], valoriLinii, receptionate) is { } gresita)
                    return $"Tranzacția {tranzactie}, linia {linie.Key}: {gresita}";
            foreach (var compensare in compensari) {
                if (!compensate.Add(compensare.Radacina.Id))
                    return $"Rădăcina {compensare.Radacina.Id} e compensată de mai multe ori.";
                if (Compensarea(citite, compensare, postate) is { } gresita)
                    return $"Tranzacția {tranzactie}: {gresita}";
            }
        }
        return null;
    }

    static string? Explica(List<FaptPostat> postate, List<AlocareValorica> alocari, List<CompensareValorica> compensari) {
        var explicate = new HashSet<Guid>();
        FaptPostat? Ia(Func<FaptPostat, bool> potrivire) =>
            postate.FirstOrDefault(f => !explicate.Contains(f.Id) && potrivire(f)) is { } f && explicate.Add(f.Id) ? f : null;
        static bool Pe(FaptPostat f, UnitateSold unitate, Sens sens) =>
            f.Postare.Coordonate.Unitate?.Id == unitate.Id && f.Postare.Coordonate.Cont == unitate.Cont && f.Postare.Coordonate.Sens == sens;

        foreach (var a in alocari.Where(a => a.Valoare != 0m))
            if (Ia(f => f.Postare.Cauza.Linie == a.Linie && Pe(f, a.Unitate, Sens.Debit)
                    && f.Postare.Valoare == a.Valoare && f.Postare.Atribuit == a.Beneficiar.Id) is null)
                return $"alocarea de {a.Valoare} spre {a.Beneficiar.Id} nu are rădăcina ei.";
        foreach (var c in compensari) {
            if (c.Valoare == 0m)
                return $"compensarea rădăcinii {c.Radacina.Id} are valoare zero.";
            var sursa = Ia(f => f.Postare.Atribuit == c.Radacina.Id && Pe(f, c.Sursa, Sens.Debit) && f.Postare.Valoare == -c.Valoare);
            var destinatie = Ia(f => f.Postare.Atribuit == c.Radacina.Id && Pe(f, c.Destinatie, Sens.Credit) && f.Postare.Valoare == -c.Valoare);
            if (sursa is null || destinatie is null || sursa.Postare.Pereche != destinatie.Postare.Pereche)
                return $"compensarea de {c.Valoare} a rădăcinii {c.Radacina.Id} nu are perechea ei.";
        }
        return postate.FirstOrDefault(f => !explicate.Contains(f.Id)) is { } fara
            ? $"postarea valorică {fara.Id} nu are decizie."
            : null;
    }

    static string? Linia(Fapte citite, Guid linie, List<AlocareValorica> alocari,
            IReadOnlyDictionary<Guid, decimal> valoriLinii, IReadOnlyDictionary<Guid, decimal> receptionate) {
        if (!valoriLinii.TryGetValue(linie, out var valoare) || alocari.Sum(a => a.Valoare) != valoare)
            return $"suma alocărilor ({alocari.Sum(a => a.Valoare)}) nu e valoarea liniei.";
        var loturi = alocari.Select(a => a.Unitate.Id).Distinct().ToList();
        if (loturi.Count != 1 || alocari.Any(a => a.Pondere <= 0m)
                || !receptionate.TryGetValue(loturi[0], out var cantitate) || alocari.Sum(a => a.Pondere) != cantitate)
            return "ponderile nu sunt pozitive sau nu dau cantitatea recepționată a lotului.";
        if (alocari.Select(a => a.Beneficiar.Id).Distinct().Count() != alocari.Count)
            return "un beneficiar are mai multe alocări.";
        var beneficiari = new List<(FaptPostat Fapt, decimal Pondere)>();
        foreach (var a in alocari) {
            if (citite.Dupa(a.Beneficiar.Id) is not { } fapt || fapt.Fel != FelTranzactie.Operare || EsteValoric(fapt)
                    || fapt.Postare.Coordonate.Sens != Sens.Debit || fapt.Postare.Cantitate <= 0m
                    || fapt.Postare.Coordonate.Unitate?.Id != a.Unitate.Id || fapt.Postare.Coordonate.Cont != a.Unitate.Cont)
                return $"beneficiarul {a.Beneficiar.Id} nu e o intrare pe poziția alocării.";
            if (citite.Partener(fapt) is { } partener && Lot(partener) == a.Unitate.Id && a.Pondere != fapt.Postare.Cantitate)
                return $"ponderea consumului {a.Beneficiar.Id} nu e cantitatea lui.";
            beneficiari.Add((fapt, a.Pondere));
        }
        var cote = Repartizeaza(beneficiari, valoare);
        return cote.Select(c => c.Indice).SequenceEqual(Enumerable.Range(0, alocari.Count))
                && cote.Select(c => c.Cota).SequenceEqual(alocari.Select(a => a.Valoare))
            ? null
            : "cotele nu rezultă din repartizarea pe ponderile și ordinea persistate.";
    }

    static string? Compensarea(Fapte citite, CompensareValorica c, List<FaptPostat> postate) {
        if (citite.Dupa(c.Radacina.Id) is not { } radacina || citite.Dupa(c.Beneficiar.Id) is not { } beneficiar)
            return $"compensarea referă o rădăcină sau un beneficiar absent.";
        if (radacina.Fel != FelTranzactie.Operare || !EsteValoric(radacina) || radacina.Postare.Atribuit != beneficiar.Id
                || EsteValoric(beneficiar) || radacina.Postare.Cauza.Linie != c.Linie)
            return $"{c.Radacina.Id} nu e alocarea corecției pe beneficiarul {c.Beneficiar.Id}.";
        if (c.Valoare != Masura(radacina) || radacina.Postare.Coordonate.Unitate?.Id != c.Sursa.Id
                || radacina.Postare.Coordonate.Cont != c.Sursa.Cont)
            return $"compensarea de {c.Valoare} nu e valoarea activă a rădăcinii pe poziția ei.";
        if (citite.Partener(beneficiar) is not { } stoc || stoc.Postare.Coordonate.Unitate?.Id != c.Destinatie.Id
                || stoc.Postare.Coordonate.Cont != c.Destinatie.Cont)
            return "destinația compensării nu e poziția de stoc a beneficiarului.";
        var scrisa = postate.Where(f => f.Postare.Atribuit == c.Radacina.Id).ToList();
        if (scrisa.Count == 0)
            return null;
        if (scrisa.Any(f => f.Postare.Cauza != radacina.Postare.Cauza))
            return $"compensarea rădăcinii {c.Radacina.Id} nu aparține corecției ei.";
        if (citite.Inverse(radacina).Any(i => i.ScrisLa <= scrisa[0].ScrisLa))
            return $"compensarea rădăcinii {c.Radacina.Id} vine după inversa ei.";
        var inverse = citite.Inverse(beneficiar).ToList();
        return inverse.Count == 1 && inverse[0].Postare.Coordonate.Data == scrisa[0].Postare.Coordonate.Data
            ? null
            : $"beneficiarul {c.Beneficiar.Id} nu are o singură inversă la data compensării.";
    }
}
