namespace Atlas.Conta.Nucleu;

// Egalitatea de record peste `IReadOnlyList` e de referință; 090b cere egalitate STRUCTURALĂ.
static class Secvente {
    public static bool Egale<T>(IReadOnlyList<T> unele, IReadOnlyList<T> altele) {
        if (ReferenceEquals(unele, altele))
            return true;
        if (unele.Count != altele.Count)
            return false;
        for (var i = 0; i < unele.Count; i++)
            if (!EqualityComparer<T>.Default.Equals(unele[i], altele[i]))
                return false;
        return true;
    }

    public static void Adauga<T>(ref HashCode cod, IReadOnlyList<T> secventa) {
        cod.Add(secventa.Count);
        foreach (var element in secventa)
            cod.Add(element);
    }
}
