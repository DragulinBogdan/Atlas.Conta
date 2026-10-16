using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public partial class CorectieValoricaTeste {
    /// <summary>Cubul unui singur lot, scris numai prin motor, storno și mecanismul corecției.</summary>
    sealed class CubMic {
        public static readonly Guid Stoc = Gen.Conturi[0], Consum = Gen.Conturi[1], Furnizori = Gen.Conturi[2], Venit = Gen.Conturi[3];
        static readonly Guid Produs = Gen.Produse[0];
        static readonly Repartitor Magazie = Gen.Locuri[0], AltaMagazie = Gen.Locuri[1], Loc = Gen.Locuri[2], Furnizor = Gen.Terti[0];
        static readonly Guid Lot = Gen.Unitati[0], LinieIntrare = Guid.NewGuid();

        readonly Dictionary<Guid, Guid> proprietari = [];
        readonly Rotunjire rotunjire = new(MidpointRounding.AwayFromZero);
        DateTime ceas = new(2026, 1, 1);
        UnitateSold lot = null!;

        public List<FaptPostat> Fapte { get; } = [];
        public List<ExplicatieValorica> Explicatii { get; } = [];
        public Dictionary<Guid, decimal> ValoriLinii { get; } = [];
        public Dictionary<Guid, decimal> Receptionate { get; } = [];
        public Guid Receptie { get; } = Guid.NewGuid();
        public Guid Bon { get; private set; }
        public ReperLot Reper => new(Lot, Stoc, Magazie.Id, LinieIntrare, Furnizor.Id);
        public int? ExercitiuInchis { get; set; }

        Capat PeStoc => new() { Cont = Stoc, Repartitor = Magazie, Produs = Produs, Unitate = lot };
        Capat PeConsum => new() { Cont = Consum, Repartitor = Loc, Produs = Produs, Unitate = lot with { Cont = Consum } };

        Capat Tert(Guid document, DateOnly data) => new() {
            Cont = Furnizori, Repartitor = Furnizor, Produs = Produs, Contrapondere = Contrapondere.Tert,
            Unitate = UnitateSold.DeschidePartida(Furnizori, Furnizor.Id, document, data),
        };

        public void Receptioneaza(decimal cantitate, decimal valoare, DateOnly data) {
            lot = new UnitateSold(Lot, FelUnitate.Lot, Stoc, null, Produs, data);
            Receptionate[Lot] = cantitate;
            Opereaza(Receptie, data, new Miscare(Tert(Receptie, data), PeStoc, cantitate, 0m, valoare, new(Receptie, LinieIntrare)));
        }

        public Guid Consuma(decimal cantitate, DateOnly data) {
            var document = Guid.NewGuid();
            if (Bon == default) Bon = document;
            var sold = Fapte.Where(f => f.Postare.Coordonate.Cont == Stoc && f.Postare.Coordonate.Data <= data)
                .Aggregate(Sold.Zero, (s, f) => s + Sold.Din(f.Postare));
            Opereaza(document, data, new Miscare(PeStoc, PeConsum, cantitate, 0m,
                Evaluare.Iesire(sold, cantitate, rotunjire), new(document, Guid.NewGuid())));
            return document;
        }

        public void Transfera(decimal cantitate, DateOnly data) {
            var document = Guid.NewGuid();
            var contract = Motor.Transfera(document, data,
                [new Mutare(PeStoc, PeStoc with { Repartitor = AltaMagazie }, Sens.Debit, cantitate, 0m, cantitate * 10m, new(document, null))],
                rotunjire);
            Assert.True(contract.EsteAcceptat);
            Scrie(contract.Tranzactii.Single());
        }

        public void Vinde(decimal cantitate, DateOnly data) {
            var document = Guid.NewGuid();
            Opereaza(document, data, new Miscare(PeStoc,
                new Capat { Cont = Venit, Produs = Produs, Contrapondere = Contrapondere.Tert },
                cantitate, 0m, cantitate * 10m, new(document, Guid.NewGuid())));
        }

        public Guid Readuce(decimal cantitate, decimal valoare, DateOnly data) {
            var document = Guid.NewGuid();
            Opereaza(document, data, new Miscare(PeStoc,
                new Capat { Cont = Venit, Produs = Produs, Contrapondere = Contrapondere.Tert },
                -cantitate, 0m, -valoare, new(document, Guid.NewGuid())));
            return document;
        }

        public Guid Corecteaza(decimal delta, DateOnly data, ReperLot? reper = null) {
            var document = Guid.NewGuid();
            var cauza = new Cauza(document, Guid.NewGuid());
            var alocari = CorectieValorica.Aloca(CorectieValorica.Beneficiari(reper ?? Reper, Fapte, data, ExercitiuInchis), delta);
            Assert.Equal(delta, alocari.Sum(a => a.Valoare));
            var tranzactie = Opereaza(document, data, [.. alocari.Where(a => a.Valoare != 0m)
                .Select(a => CorectieValorica.Miscarea(a, Tert(document, data), cauza))]);
            Explicatii.Add(new(tranzactie, [.. alocari.Select(a => CorectieValorica.Decizia(a, cauza.Linie!.Value))]));
            ValoriLinii[cauza.Linie!.Value] = delta;
            return document;
        }

        public void Storneaza(Guid document, DateOnly data) {
            var compensari = CorectieValorica.LaStornare(Fapte, document, data);
            var inversa = Storno.Inverseaza(
                Fapte.Where(f => f.Fel == FelTranzactie.Operare && f.Postare.Cauza.Document == document)
                    .Select(f => (f.Tranzactie, f.Postare with { InversaDin = new(f.Id, f.Postare.Spatiu()) })),
                document, data, null);
            Assert.Empty(Conservare.Verifica(inversa));
            Scrie(inversa);
            foreach (var aleCorectiei in compensari.GroupBy(c => c.Radacina.Postare.Cauza.Document))
                Explicatii.Add(new(Opereaza(aleCorectiei.Key, data, [.. aleCorectiei.Select(CorectieValorica.Miscarea)]),
                    [.. aleCorectiei.Select(CorectieValorica.Decizia)]));
        }

        public void Retrage(Guid document) {
            CorectieValorica.LaRetragere(Fapte, document);
            Explicatii.RemoveAll(e => proprietari[e.Tranzactie] == document);
            Fapte.RemoveAll(f => f.Postare.Cauza.Document == document);
        }

        public IEnumerable<FaptPostat> Operare(Guid document) =>
            Fapte.Where(f => f.Fel == FelTranzactie.Operare && proprietari[f.Tranzactie] == document);

        public int Tranzactii(Guid document, FelTranzactie fel) =>
            Fapte.Where(f => f.Fel == fel && proprietari[f.Tranzactie] == document).Select(f => f.Tranzactie).Distinct().Count();

        public Guid Proprietar(FaptPostat fapt) => proprietari[fapt.Tranzactie];

        /// <summary>Cantitatea și valoarea stocului, valoarea consumului și datoria.</summary>
        public (decimal Cantitate, decimal Stoc, decimal Consum, decimal Datorie) Stare() => (
            Fapte.Where(f => f.Postare.Coordonate.Cont == Stoc && f.Postare.Coordonate.Repartitor == Magazie).Sum(f => f.Postare.Cantitate),
            Pe(Stoc), Pe(Consum), -Pe(Furnizori));

        decimal Pe(Guid cont) => Fapte.Where(f => f.Postare.Coordonate.Cont == cont).Sum(CorectieValorica.Masura);

        public string? Audit() => CorectieValorica.Auditeaza(Fapte, Explicatii, ValoriLinii, Receptionate);

        public void Muta(Func<FaptPostat, bool> care, Func<FaptPostat, FaptPostat> cum) {
            for (var i = 0; i < Fapte.Count; i++)
                if (care(Fapte[i])) Fapte[i] = cum(Fapte[i]);
        }

        public void MutaDecizii(Func<Decizie, Decizie> cum) {
            for (var i = 0; i < Explicatii.Count; i++)
                Explicatii[i] = Explicatii[i] with { Decizii = [.. Explicatii[i].Decizii.Select(cum)] };
        }

        public Guid Adauga(Guid document, IEnumerable<FaptPostat> fapte) {
            var id = Guid.NewGuid();
            proprietari[id] = document;
            ceas = ceas.AddSeconds(1);
            Fapte.AddRange(fapte.Select(f => f with { Id = Guid.NewGuid(), Tranzactie = id, ScrisLa = ceas }));
            return id;
        }

        Guid Opereaza(Guid document, DateOnly data, params Miscare[] miscari) {
            var contract = Motor.Opereaza(new Declaratie(document, data, miscari, [], []), rotunjire);
            Assert.True(contract.EsteAcceptat, string.Join("; ", contract.Refuzuri.Select(r => r.Mesaj)));
            return Scrie(contract.Tranzactii.Single());
        }

        Guid Scrie(Tranzactie tranzactie) {
            var id = Guid.NewGuid();
            proprietari[id] = tranzactie.Document!.Value;
            ceas = ceas.AddSeconds(1);
            Fapte.AddRange(tranzactie.Postari.Select((p, i) => new FaptPostat(Guid.NewGuid(), id, tranzactie.Fel, ceas, i + 1, p)));
            return id;
        }
    }
}
