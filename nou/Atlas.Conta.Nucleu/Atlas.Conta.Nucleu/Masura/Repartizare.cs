namespace Atlas.Conta.Nucleu;

public static class Repartizare {
    public static decimal[] Hamilton(decimal total, IReadOnlyList<decimal> ponderi, int scara) {
        ArgumentNullException.ThrowIfNull(ponderi);
        var unitate = Unitatea(scara);
        VerificaTotalul(total, scara);
        var suma = Aduna(ponderi, nameof(ponderi));
        if (suma <= 0m)
            throw new ArgumentException($"Σ ponderi = {suma}, se cere > 0.", nameof(ponderi));
        return Imparte(total, ponderi, suma, unitate);
    }

    public static decimal[][] Hamilton(decimal total, IReadOnlyList<IReadOnlyList<decimal>> grupuri, int scara) {
        ArgumentNullException.ThrowIfNull(grupuri);
        var unitate = Unitatea(scara);
        VerificaTotalul(total, scara);
        var sume = new decimal[grupuri.Count];
        var suma = 0m;
        for (var g = 0; g < grupuri.Count; g++) {
            ArgumentNullException.ThrowIfNull(grupuri[g]);
            sume[g] = Aduna(grupuri[g], nameof(grupuri));
            suma += sume[g];
        }
        if (suma <= 0m)
            throw new ArgumentException($"Σ ponderi peste toate grupurile = {suma}, se cere > 0.", nameof(grupuri));
        var peGrupuri = Imparte(total, sume, suma, unitate);
        var cote = new decimal[grupuri.Count][];
        for (var g = 0; g < grupuri.Count; g++)
            // N-D8: grupul fără pondere ia 0 din împărțirea de sus, deci n-are ce distribui.
            cote[g] = sume[g] == 0m
                ? new decimal[grupuri[g].Count]
                : Imparte(peGrupuri[g], grupuri[g], sume[g], unitate);
        return cote;
    }

    static decimal[] Imparte(decimal total, IReadOnlyList<decimal> ponderi, decimal sumaPonderi, decimal unitate) {
        var unitati = Math.Abs(total) / unitate;
        var baze = new decimal[ponderi.Count];
        var fractii = new decimal[ponderi.Count];
        var rest = unitati;
        for (var i = 0; i < ponderi.Count; i++) {
            var exact = unitati * ponderi[i] / sumaPonderi;
            baze[i] = decimal.Truncate(exact);
            fractii[i] = exact - baze[i];
            rest -= baze[i];
        }
        foreach (var i in fractii
                     .Select((fractie, indice) => (Fractie: fractie, Indice: indice))
                     .OrderByDescending(f => f.Fractie)
                     .Take((int)rest)
                     .Select(f => f.Indice))
            baze[i] += 1m;
        var cote = new decimal[ponderi.Count];
        for (var i = 0; i < cote.Length; i++) {
            var cota = baze[i] * unitate;
            cote[i] = total < 0m ? -cota : cota;
        }
        return cote;
    }

    static decimal Aduna(IReadOnlyList<decimal> ponderi, string nume) {
        var suma = 0m;
        foreach (var pondere in ponderi) {
            if (pondere < 0m)
                throw new ArgumentException($"ponderea {pondere} e negativă.", nume);
            suma += pondere;
        }
        return suma;
    }

    static void VerificaTotalul(decimal total, int scara) {
        if (!Scara.EsteLa(total, scara))
            throw new ArgumentException(
                $"totalul {total} are mai multe zecimale decât scara {scara}, deci nu poate fi Σ al cotelor.",
                nameof(total));
    }

    static decimal Unitatea(int scara) =>
        scara >= 0 && scara <= Scara.Precizie
            ? new decimal(1, 0, 0, false, (byte)scara)
            : throw new ArgumentOutOfRangeException(nameof(scara), scara, $"scara se cere în 0…{Scara.Precizie}.");
}
