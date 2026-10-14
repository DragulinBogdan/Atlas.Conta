namespace Atlas.Conta.Nucleu;

/// <summary>Lotul corecției: poziția lui de stoc, linia de intrare și furnizorul recepției.</summary>
public sealed record ReperLot(Guid Lot, Guid ContStoc, Guid Gestiune, Guid LinieIntrare, Guid Furnizor);

/// <summary>Beneficiarul unei corecții: stocul rămas, ancorat în recepție, sau un consum activ.</summary>
public sealed record BeneficiarValoric(FaptPostat Fapt, bool Stoc, decimal Pondere, decimal ValoareActiva);

public sealed record AlocareLot(BeneficiarValoric Beneficiar, decimal Valoare);

/// <summary>Rădăcina activă a unui consum inversat, cu piciorul de stoc al consumului.</summary>
public sealed record CompensareLot(FaptPostat Radacina, FaptPostat Beneficiar, FaptPostat Stoc);

/// <summary>Corecția valorii unui lot fără schimbarea cantității, judecată pe forma faptelor (CV-D10).</summary>
public static partial class CorectieValorica {
    /// <summary>Beneficiarii lotului la data corecției, în ordinea repartizării.</summary>
    public static IReadOnlyList<BeneficiarValoric> Beneficiari(
            ReperLot reper, IReadOnlyList<FaptPostat> fapte, DateOnly data) {
        ArgumentNullException.ThrowIfNull(reper);
        var citite = new Fapte(fapte);
        var peLot = citite.Toate.Where(f => Lot(f) == reper.Lot).ToList();
        if (peLot.Any(f => f.Postare.Coordonate.Data > data))
            throw Refuza(MotivCorectieValorica.Cronologie, "Corecția precedă o mișcare deja operată a lotului.");

        bool PeStoc(FaptPostat f) => Lot(f) == reper.Lot && f.Postare.Coordonate.Cont == reper.ContStoc
            && f.Postare.Coordonate.Repartitor?.Id == reper.Gestiune;

        FaptPostat? receptie = null;
        var consumuri = new List<FaptPostat>();
        foreach (var fapt in peLot.Where(f => !EsteValoric(f))) {
            if (fapt.Fel == FelTranzactie.Storno) {
                if (citite.Sursa(fapt) is not { Fel: FelTranzactie.Operare } sursa || EsteValoric(sursa) || Lot(sursa) != reper.Lot)
                    throw Neeligibil("o inversă fără originalul ei pe lot");
                continue;
            }
            if (fapt.Fel != FelTranzactie.Operare || citite.Partener(fapt) is not { } partener)
                throw Neeligibil("o mișcare care nu e recepție sau consum");
            var debit = fapt.Postare.Coordonate.Sens == Sens.Debit;
            if (Lot(partener) != reper.Lot) {
                if (!debit || !PeStoc(fapt) || fapt.Postare.Cantitate <= 0m || receptie is not null
                        || fapt.Postare.Cauza.Linie != reper.LinieIntrare || citite.EsteInversat(fapt)
                        || partener.Postare.Coordonate.Repartitor?.Id != reper.Furnizor)
                    throw Neeligibil("o mișcare cu terțul care nu e recepția activă a lotului de la furnizorul corecției");
                receptie = fapt;
                continue;
            }
            var (intrat, iesit) = debit ? (fapt, partener) : (partener, fapt);
            if (intrat.Postare.Coordonate.Sens != Sens.Debit || iesit.Postare.Coordonate.Sens != Sens.Credit
                    || !PeStoc(iesit) || PeStoc(intrat) || intrat.Postare.Cantitate <= 0m)
                throw Neeligibil("o mișcare internă care nu e consum din poziția de stoc");
            if (debit && !citite.EsteInversat(fapt))
                consumuri.Add(fapt);
        }
        if (receptie is null)
            throw Neeligibil("nicio recepție activă");

        var peStoc = peLot.Where(PeStoc).ToList();
        var ramas = peStoc.Sum(f => f.Postare.Cantitate);
        if (ramas < 0m || ramas + consumuri.Sum(f => f.Postare.Cantitate) != receptie.Postare.Cantitate)
            throw Neeligibil("stocul rămas și consumurile active nu dau cantitatea recepționată");
        if (consumuri.FirstOrDefault(f => f.Postare.Coordonate.Data.Year < data.Year) is { } vechi)
            throw Refuza(MotivCorectieValorica.AnInchis,
                $"Consumul din {vechi.Postare.Coordonate.Data:yyyy-MM-dd} este într-un an anterior corecției.");

        var beneficiari = consumuri
            .Select(f => new BeneficiarValoric(f, false, f.Postare.Cantitate,
                Masura(f) + citite.RadaciniActive(f).Sum(Masura)))
            .ToList();
        if (ramas > 0m)
            beneficiari.Add(new BeneficiarValoric(receptie, true, ramas, peStoc.Sum(Masura)));
        return [.. beneficiari.OrderBy(b => b.Fapt, Ordine)];
    }

    /// <summary>Hamilton pe poziții, apoi pe beneficiarii fiecărei poziții; alocările ies în ordinea repartizării.</summary>
    public static IReadOnlyList<AlocareLot> Aloca(IReadOnlyList<BeneficiarValoric> beneficiari, decimal delta) {
        ArgumentNullException.ThrowIfNull(beneficiari);
        var alocari = Repartizeaza([.. beneficiari.Select(b => (b.Fapt, b.Pondere))], delta)
            .Select(c => new AlocareLot(beneficiari[c.Indice], c.Cota)).ToList();
        if (alocari.Any(a => a.Beneficiar.ValoareActiva + a.Valoare < 0m))
            throw Refuza(MotivCorectieValorica.ValoareInsuficienta,
                $"Corecția de {delta} ar lăsa valoare negativă pe un beneficiar al lotului.");
        return alocari;
    }

    /// <summary>Gardurile stornării unui document și compensările pe care le declanșează.</summary>
    public static IReadOnlyList<CompensareLot> LaStornare(IReadOnlyList<FaptPostat> fapte, Guid document, DateOnly data) {
        var citite = new Fapte(fapte);
        var proprii = citite.Proprii(document, FelTranzactie.Operare);
        var familie = proprii.Concat(proprii.SelectMany(citite.Atribuite).SelectMany(r =>
            citite.Inverse(r).Append(r).Concat(citite.Atribuite(r).SelectMany(k => citite.Inverse(k).Append(k)))));
        if (familie.Any(f => f.Postare.Coordonate.Data > data))
            throw Refuza(MotivCorectieValorica.Cronologie, "Data stornării precedă un fapt al corecțiilor de valoare ale lotului.");

        var compensari = new List<CompensareLot>();
        foreach (var fapt in proprii.Where(f => !EsteValoric(f) && f.Postare.Cantitate > 0m)) {
            if (citite.Partener(fapt) is not { } partener)
                continue;
            if (Lot(partener) != Lot(fapt)) {
                if (citite.Radacini.Any(r => Lot(r) == Lot(fapt) && !citite.EsteInversat(r)))
                    throw Refuza(MotivCorectieValorica.Dependenti, "Lotul are o corecție de valoare activă; stornați întâi corecția.");
                continue;
            }
            compensari.AddRange(citite.RadaciniActive(fapt).Select(r => new CompensareLot(r, fapt, partener)));
        }
        VerificaInversareaCorectiei(citite, document);
        return compensari;
    }

    /// <summary>Gardurile retragerii unui document.</summary>
    public static void LaRetragere(IReadOnlyList<FaptPostat> fapte, Guid document) {
        var citite = new Fapte(fapte);
        var proprii = citite.Proprii(document, FelTranzactie.Operare, FelTranzactie.Transfer);
        if (proprii.Any(f => citite.Atribuite(f).Any()))
            throw Refuza(MotivCorectieValorica.Dependenti, "Documentul poartă corecții de valoare sau compensările lor; folosiți storno.");
        VerificaInversareaCorectiei(citite, document);
    }

    /// <summary>Rădăcina unei alocări: debitul pe poziția beneficiarului, creditul pe capătul de terț dat.</summary>
    public static Miscare Miscarea(AlocareLot alocare, Capat tert, Cauza cauza) {
        ArgumentNullException.ThrowIfNull(alocare);
        return new(tert, Capatul(alocare.Beneficiar.Fapt), 0m, 0m, alocare.Valoare, cauza) {
            AtribuitLa = alocare.Beneficiar.Fapt.Id,
        };
    }

    /// <summary>Perechea de compensare: scoate alocarea activă de pe consum și o pune pe stoc.</summary>
    public static Miscare Miscarea(CompensareLot compensare) {
        ArgumentNullException.ThrowIfNull(compensare);
        var radacina = compensare.Radacina;
        return new(Capatul(compensare.Stoc), Capatul(radacina), 0m, 0m, -radacina.Postare.Valoare, radacina.Postare.Cauza) {
            AtribuitLa = radacina.Id, AtribuitDeLa = radacina.Id,
        };
    }

    public static AlocareValorica Decizia(AlocareLot alocare, Guid linie) {
        ArgumentNullException.ThrowIfNull(alocare);
        var fapt = alocare.Beneficiar.Fapt;
        return new(linie, fapt.Postare.Coordonate.Unitate!, Referinta(fapt), alocare.Beneficiar.Pondere, alocare.Valoare);
    }

    public static CompensareValorica Decizia(CompensareLot compensare) {
        ArgumentNullException.ThrowIfNull(compensare);
        var radacina = compensare.Radacina;
        return new(radacina.Postare.Cauza.Linie ?? Guid.Empty, Referinta(radacina), Referinta(compensare.Beneficiar),
            radacina.Postare.Coordonate.Unitate!, compensare.Stoc.Postare.Coordonate.Unitate!, Masura(radacina));
    }

    static ReferintaPostare Referinta(FaptPostat fapt) => new(fapt.Id, fapt.Postare.Spatiu());

    static Capat Capatul(FaptPostat fapt) {
        var c = fapt.Postare.Coordonate;
        return new() { Cont = c.Cont, Repartitor = c.Repartitor, Produs = c.Produs, Unitate = c.Unitate, Carte = c.Carte, Analiza = c.Analiza };
    }

    /// <summary>Măsura semnată de sold: debitul adună, creditul scade.</summary>
    public static decimal Masura(FaptPostat fapt) {
        ArgumentNullException.ThrowIfNull(fapt);
        return fapt.Postare.Coordonate.Sens == Sens.Debit ? fapt.Postare.Valoare : -fapt.Postare.Valoare;
    }

    /// <summary>Postarea pe lot fără cantitate, atribuită: rădăcină, compensare sau inversa lor.</summary>
    public static bool EsteValoric(FaptPostat fapt) {
        ArgumentNullException.ThrowIfNull(fapt);
        return Lot(fapt) is not null && fapt.Postare.Cantitate == 0m && fapt.Postare.Atribuit is not null;
    }

    static void VerificaInversareaCorectiei(Fapte citite, Guid document) {
        var radacini = citite.Radacini.Where(r => r.Postare.Cauza.Document == document).ToList();
        if (radacini.Count == 0)
            return;
        var prima = radacini.Min(r => r.ScrisLa);
        var loturi = radacini.Select(Lot).ToHashSet();
        if (citite.Toate.Any(f => loturi.Contains(Lot(f)) && f.Postare.Cantitate < 0m
                && f.Fel is FelTranzactie.Operare or FelTranzactie.Transfer && !citite.EsteInversat(f)
                && f.Postare.Cauza.Document != document && f.ScrisLa >= prima))
            throw Refuza(MotivCorectieValorica.Dependenti,
                "Lotul are o ieșire activă făcută după corecție; neutralizați întâi ieșirea.");

        var valorice = citite.Toate.Where(f => f.Fel == FelTranzactie.Operare && EsteValoric(f)
            && f.Postare.Cauza.Document == document).ToList();
        static (Guid?, Guid, Repartitor?) Loc(FaptPostat f) =>
            (Lot(f), f.Postare.Coordonate.Cont, f.Postare.Coordonate.Repartitor);
        foreach (var loc in valorice.Select(Loc).Distinct())
            if (citite.Toate.Where(f => Loc(f) == loc).Sum(Masura) - valorice.Where(f => Loc(f) == loc).Sum(Masura) < 0m)
                throw Negativ();
        foreach (var beneficiar in radacini.Select(r => citite.Tinta(r)!).Where(b => !citite.EsteInversat(b)
                     && citite.Partener(b) is { } partener && Lot(partener) == Lot(b)))
            if (Masura(beneficiar) + citite.RadaciniActive(beneficiar)
                    .Where(r => r.Postare.Cauza.Document != document).Sum(Masura) < 0m)
                throw Negativ();

        static CorectieValoricaRefuzata Negativ() => Refuza(MotivCorectieValorica.ValoareInsuficienta,
            "Fără această corecție, un beneficiar al lotului ar rămâne cu valoare negativă.");
    }

    static Guid? Lot(FaptPostat fapt) =>
        fapt.Postare.Coordonate.Unitate is { Fel: FelUnitate.Lot } unitate ? unitate.Id : null;

    /// <summary>Hamilton pe poziții, apoi pe beneficiarii fiecărei poziții; cotele ies în ordinea repartizării, cu indicele beneficiarului.</summary>
    static List<(int Indice, decimal Cota)> Repartizeaza(IReadOnlyList<(FaptPostat Fapt, decimal Pondere)> beneficiari, decimal delta) {
        var grupuri = new List<List<int>>();
        for (var i = 0; i < beneficiari.Count; i++) {
            var grup = grupuri.FirstOrDefault(g => Pozitie(beneficiari[g[0]].Fapt) == Pozitie(beneficiari[i].Fapt));
            if (grup is null)
                grupuri.Add(grup = []);
            grup.Add(i);
        }
        var cote = Repartizare.Hamilton(delta,
            grupuri.Select(g => (IReadOnlyList<decimal>)[.. g.Select(i => beneficiari[i].Pondere)]).ToList(), Scara.Bani);
        return [.. grupuri.SelectMany((g, i) => g.Select((indice, j) => (indice, cote[i][j])))];
    }

    static (Guid Cont, Repartitor? Repartitor, Analiza Analiza) Pozitie(FaptPostat fapt) =>
        (fapt.Postare.Coordonate.Cont, fapt.Postare.Coordonate.Repartitor, fapt.Postare.Coordonate.Analiza);

    static readonly Comparer<FaptPostat> Ordine = Comparer<FaptPostat>.Create((a, b) => {
        var ordine = a.Postare.Coordonate.Data.CompareTo(b.Postare.Coordonate.Data);
        if (ordine == 0) ordine = a.ScrisLa.CompareTo(b.ScrisLa);
        if (ordine == 0) ordine = a.Tranzactie.CompareTo(b.Tranzactie);
        if (ordine == 0) ordine = a.Ordinal.CompareTo(b.Ordinal);
        return ordine != 0 ? ordine : a.Id.CompareTo(b.Id);
    });

    static CorectieValoricaRefuzata Refuza(MotivCorectieValorica motiv, string mesaj) => new(motiv, mesaj);

    static CorectieValoricaRefuzata Neeligibil(string motiv) =>
        Refuza(MotivCorectieValorica.LotNeeligibil, $"Lotul nu se corectează valoric în acest pachet: are {motiv}.");

    sealed class Fapte {
        readonly Dictionary<Guid, FaptPostat> dupaId;
        readonly ILookup<(Guid, int), FaptPostat> perechi;
        readonly ILookup<Guid, FaptPostat> atribuite;
        readonly ILookup<Guid, FaptPostat> inverse;

        public Fapte(IReadOnlyList<FaptPostat> fapte) {
            ArgumentNullException.ThrowIfNull(fapte);
            Toate = fapte;
            dupaId = fapte.ToDictionary(f => f.Id);
            perechi = fapte.Where(f => f.Postare.Pereche is not null).ToLookup(f => (f.Tranzactie, f.Postare.Pereche!.Value));
            atribuite = fapte.Where(f => f.Fel == FelTranzactie.Operare && EsteValoric(f)).ToLookup(f => f.Postare.Atribuit!.Value);
            inverse = fapte.Where(f => f.Fel == FelTranzactie.Storno && f.Postare.InversaDin is not null)
                .ToLookup(f => f.Postare.InversaDin!.Id);
            Radacini = [.. atribuite.SelectMany(g => g).Where(f => Tinta(f) is { } tinta && !EsteValoric(tinta))];
        }

        public IReadOnlyList<FaptPostat> Toate { get; }

        /// <summary>Alocările: postările valorice a căror țintă nu e ea însăși valorică.</summary>
        public IReadOnlyList<FaptPostat> Radacini { get; }

        public FaptPostat? Tinta(FaptPostat fapt) =>
            fapt.Postare.Atribuit is Guid id
                ? dupaId.GetValueOrDefault(id) ?? throw Refuza(MotivCorectieValorica.Provenienta, $"Postarea {fapt.Id} e atribuită unei postări absente.")
                : null;

        public FaptPostat? Dupa(Guid id) => dupaId.GetValueOrDefault(id);

        public FaptPostat? Sursa(FaptPostat fapt) =>
            fapt.Postare.InversaDin is { } sursa ? dupaId.GetValueOrDefault(sursa.Id) : null;

        public FaptPostat? Partener(FaptPostat fapt) =>
            fapt.Postare.Pereche is int pereche
                ? perechi[(fapt.Tranzactie, pereche)].SingleOrDefault(f => f.Id != fapt.Id)
                : null;

        public bool EsteInversat(FaptPostat fapt) => inverse.Contains(fapt.Id);

        public IEnumerable<FaptPostat> Inverse(FaptPostat fapt) => inverse[fapt.Id];

        /// <summary>Postările `Operare` valorice atribuite faptului: rădăcinile unui beneficiar sau compensările unei rădăcini.</summary>
        public IEnumerable<FaptPostat> Atribuite(FaptPostat fapt) => atribuite[fapt.Id];

        public IEnumerable<FaptPostat> RadaciniActive(FaptPostat beneficiar) =>
            atribuite[beneficiar.Id].Where(r => !EsteInversat(r));

        public List<FaptPostat> Proprii(Guid document, params FelTranzactie[] feluri) =>
            [.. Toate.Where(f => f.Postare.Cauza.Document == document && feluri.Contains(f.Fel) && Lot(f) is not null)];
    }
}
