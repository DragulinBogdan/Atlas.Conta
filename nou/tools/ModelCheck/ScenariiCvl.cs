using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// docs/nucleu/scenarii/CVL.md: corecția valorii lotului prin FCT cu linie de corecție (TR-D9b-CV).
sealed partial class ScenariiCvl(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "CVL", 2042) {
    sealed record Caz(Guid Factura, Guid Nir, Guid Linie, Guid Lot, List<Guid> Documente);
    readonly record struct Stare(decimal Cantitate, decimal Stoc, decimal Consum, decimal Datorie) {
        public override string ToString() => $"{Cantitate}/{Stoc}, C{Consum}, D{Datorie}";
    }

    int numar;
    DateOnly Zi(int zi, int luna = 1) => new(An, luna, zi);

    protected override void Executa() {
        OperareSiEvaluare();
        Diminuare();
        Rotunjire();
        MaiMulteConsumuriSiLoturi();
        RetragereSiStorno();
        OrdineaCompensarii();
        ToateOrdinile();
        IesireUlterioara();
        Dependenti();
        StornoReceptie();
        Cronologie();
        Structura();
        Perimetru();
        Partide();
        CorectieContraConsum();
        CorectieContraStorno();
        StornoContraStorno();
        EsecDupaCompensare();
        PerioadaInchisa();
    }

    Caz Lot(decimal cantitate = 100m, decimal pret = 10m) {
        var f = Factura(Ianuarie, new LinieFctScena(cantitate, pret));
        var nir = Opereaza(f.Id).ConexId ?? throw new InvalidOperationException("Recepția cere NIR conex.");
        Opereaza(nir);
        return new(f.Id, nir, f.Linii[0].Id, f.Linii[0].Lot!.Value, [f.Id]);
    }

    Caz Partial() { var caz = Lot(); Bon(caz, 60m); return caz; }

    Guid Bon(Caz caz, decimal cantitate, int zi = 10, int luna = 1, bool opereaza = true) {
        var id = CuSpatiu(os => {
            var d = os.CreateObject<BonConsum>(); d.Data = Zi(zi, luna);
            d.PredatorId = Magazie; d.PrimitorId = Loc;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = caz.Lot; l.Cantitate = cantitate;
            os.CommitChanges(); return d.ID;
        });
        if (opereaza) Opereaza(id);
        return id;
    }

    Guid Draft(DateOnly data, params (Caz Caz, decimal Delta)[] linii) => Draft(data, null, linii);

    Guid Draft(DateOnly data, Action<IObjectSpace, FacturaIntrare, FacturaIntrareDetaliu> ajusteaza, params (Caz Caz, decimal Delta)[] linii) {
        var id = CuSpatiu(os => {
            var doc = os.CreateObject<FacturaIntrare>();
            doc.Numar = $"{Marcaj}-C{++numar}"; doc.Data = data; doc.PredatorId = Furnizor; doc.PrimitorId = Magazie;
            foreach (var (caz, delta) in linii) {
                var d = os.CreateObject<FacturaIntrareDetaliu>();
                d.Document = doc; d.Pozitie = doc.Detalii.Count; d.TipMaterialId = Tip(os, Stoc);
                d.CorectieValoareLot = true; d.LotId = caz.Lot; d.Valoare = delta; d.CodEconomicId = Economic;
                ajusteaza?.Invoke(os, doc, d);
            }
            os.CommitChanges(); return doc.ID;
        });
        foreach (var (caz, _) in linii) caz.Documente.Add(id);
        return id;
    }

    Guid Corectie(Caz caz, decimal delta, int zi = 15, int luna = 1) {
        var id = Draft(Zi(zi, luna), (caz, delta));
        Opereaza(id);
        return id;
    }

    Stare Citeste(Caz caz, DateOnly? laData = null) => CuSpatiu(os => {
        var zi = laData ?? DateOnly.MaxValue;
        var furnizor = Cont(ContFurnizor);
        var postari = os.GetObjectsQuery<C.Postare>().Where(p => p.Carte == N.Carte.Contabil && p.Data <= zi
            && (p.Unitate == caz.Lot || p.Cont == furnizor && p.DocumentId != null && caz.Documente.Contains(p.DocumentId.Value)))
            .ToList();
        decimal M(C.Postare p) => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare;
        var peLot = postari.Where(p => p.Unitate == caz.Lot).ToList();
        return new Stare(peLot.Where(p => p.Repartitor == Magazie).Sum(p => p.Cantitate),
            peLot.Where(p => p.Repartitor == Magazie).Sum(M), peLot.Where(p => p.Repartitor == Loc).Sum(M),
            -postari.Where(p => p.Cont == furnizor && p.Unitate != caz.Lot).Sum(M));
    });

    void Este(string id, string pas, Caz caz, decimal q, decimal v, decimal c, decimal d, DateOnly? laData = null) {
        var stare = Citeste(caz, laData);
        Verifica(id, $"{pas}: {new Stare(q, v, c, d)} (obținut {stare})", stare == new Stare(q, v, c, d));
    }

    List<C.Postare> Valorice(Guid doc, N.FelTranzactie fel) => CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
        .Where(p => p.Tranzactie.DocumentId == doc && p.Tranzactie.Fel == fel && p.FelUnitate == N.FelUnitate.Lot)
        .OrderBy(p => p.Tranzactie.ScrisLa).ThenBy(p => p.Ordinal).ToList());

    StareDocument StareaDocumentului(Guid doc) => CuSpatiu(os => os.GetObjectsQuery<Document>().Single(d => d.ID == doc).Stare);

    void OperareSiEvaluare() {
        var intact = Lot();
        var a = Draft(Zi(15), (intact, 200m));
        Verifica("SC-CVL-01", "dry-run fără refuzuri și fără efecte",
            CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(a)).Count == 0 && Tranzactii(a).Count == 0
            && Citeste(intact) == new Stare(100m, 1000m, 0m, 1000m) && ValoriLinii(a).SequenceEqual([200m]));
        var rezultat = Opereaza(a);
        Este("SC-CVL-01", "lot intact, +200", intact, 100m, 1200m, 0m, 1200m);
        Verifica("SC-CVL-25", "corecția nu generează NIR, nu naște lot și lasă recepția operată, cu prețul de intrare 10",
            rezultat.ConexId == null && StareaDocumentului(intact.Factura) == StareDocument.Operat
            && CuSpatiu(os => os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId != null
                && os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.ID == l.LinieIntrareId && d.DocumentId == a)) == 0
                && os.GetObjectsQuery<Lot>().Single(l => l.ID == intact.Lot).PretUnitar == 10m
                && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == a)));
        Comanda(os => ProbeCub.ProbaOperare(os, check, $"CVL ({(Privat ? "privat" : "bugetar")})", os.GetObjectByKey<Document>(a)));
        SoldPartida("SC-CVL-25", Partida(intact.Factura, ContFurnizor)!.Value, Zi(28), -1000m);
        SoldPartida("SC-CVL-25", Partida(a, ContFurnizor)!.Value, Zi(28), -200m);
        ValoriLinii("SC-CVL-01", "ieșirea nouă din soldul ajustat", Bon(intact, 25m, 18), 300m);
        Este("SC-CVL-01", "după ieșirea de 25", intact, 75m, 900m, 300m, 1200m);
        ValoriLinii("SC-CVL-01", "ultima ieșire ia restul", Bon(intact, 75m, 18), 900m);
        Este("SC-CVL-01", "lot epuizat", intact, 0m, 0m, 1200m, 1200m);

        var partial = Lot();
        var vechi = Bon(partial, 60m);
        var b = Corectie(partial, 200m);
        Este("SC-CVL-02", "consumate 60, +200", partial, 40m, 480m, 720m, 1200m);
        ValoriLinii("SC-CVL-02", "bonul vechi rămâne neschimbat", vechi, 600m);
        var radacini = Valorice(b, N.FelTranzactie.Operare);
        var decizii = Explicatia(b, N.FelTranzactie.Operare).Origini.Single().Explicatie.Decizii.OfType<N.AlocareValorica>().ToList();
        Verifica("SC-CVL-26", "rădăcinile: debit, cantitate zero, atribuite recepției și consumului, 80 și 120",
            radacini.Count == 2 && radacini.All(p => p.Sens == N.Sens.Debit && p.Cantitate == 0m && p.Atribuit != null
                && p.InversaDinId == null && p.Contrapondere == null && p.DocumentId == b)
            && radacini.Single(p => p.Repartitor == Magazie).Valoare == 80m && radacini.Single(p => p.Repartitor == Loc).Valoare == 120m
            && CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == b && p.FelUnitate == N.FelUnitate.Partida)
                .ToList()).All(p => p.Contrapondere == N.Contrapondere.Tert && p.Atribuit == null && p.Cantitate == 0m));
        Verifica("SC-CVL-31", "explicația: două alocări în ordinea repartizării, ponderi 40 și 60, cote 80 și 120",
            decizii.Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(40m, 80m), (60m, 120m)])
            && decizii.Select(d => d.Beneficiar.Id).SequenceEqual(
                [radacini.Single(p => p.Repartitor == Magazie).Atribuit!.Value, radacini.Single(p => p.Repartitor == Loc).Atribuit!.Value]));
        ValoriLinii("SC-CVL-02", "ieșirea nouă de 10", Bon(partial, 10m, 18), 120m);
        ValoriLinii("SC-CVL-02", "ultima ieșire de 30", Bon(partial, 30m, 18), 360m);
        Este("SC-CVL-02", "lot epuizat", partial, 0m, 0m, 1200m, 1200m);

        var epuizat = Lot();
        Bon(epuizat, 100m);
        Corectie(epuizat, 200m);
        Este("SC-CVL-03", "lot epuizat, +200 numai pe consum", epuizat, 0m, 0m, 1200m, 1200m);
    }

    void Diminuare() {
        var intact = Lot();
        Corectie(intact, -200m);
        Este("SC-CVL-04", "intact, −200", intact, 100m, 800m, 0m, 800m);
        var epuizat = Lot();
        Bon(epuizat, 100m);
        Corectie(epuizat, -200m);
        Este("SC-CVL-04", "epuizat, −200", epuizat, 0m, 0m, 800m, 800m);

        var partial = Lot();
        var bon = Bon(partial, 60m);
        var a = Corectie(partial, -200m);
        Este("SC-CVL-04", "parțial, −200", partial, 40m, 320m, 480m, 800m);
        Verifica("SC-CVL-04", "diminuarea păstrează debitul, cu valori negative",
            Valorice(a, N.FelTranzactie.Operare).All(p => p.Sens == N.Sens.Debit && p.Valoare < 0m));
        Storneaza(bon, Zi(20));
        Este("SC-CVL-04", "storno bon compensează −120 spre stoc", partial, 100m, 800m, 0m, 800m);
        Storneaza(a, Zi(20));
        Este("SC-CVL-04", "storno corecție", partial, 100m, 1000m, 0m, 1000m);

        var invers = Partial();
        var c = Corectie(invers, -200m);
        Storneaza(c, Zi(20));
        Storneaza(BonulLui(invers), Zi(20));
        Este("SC-CVL-04", "ordinea inversă, compensare zero", invers, 100m, 1000m, 0m, 1000m);

        var zero = Partial();
        var peste = Draft(Zi(15), (zero, -1000.01m));
        Refuza("SC-CVL-05", () => Opereaza(peste), CoduriRefuz.CvValoareInsuficienta);
        Este("SC-CVL-05", "refuzul nu lasă efecte", zero, 40m, 400m, 600m, 1000m);
        Corectie(zero, -1000m);
        Este("SC-CVL-05", "diminuare până la zero", zero, 40m, 0m, 0m, 0m);
        ValoriLinii("SC-CVL-05", "ieșirea restului fără valoare", Bon(zero, 40m, 18), 0m);
        Este("SC-CVL-05", "lot epuizat fără valoare", zero, 0m, 0m, 0m, 0m);
    }

    Guid BonulLui(Caz caz) => CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == caz.Lot
        && p.Repartitor == Loc && p.Tranzactie.Fel == N.FelTranzactie.Operare && p.Cantitate > 0m)
        .OrderBy(p => p.Tranzactie.ScrisLa).Select(p => p.DocumentId!.Value).First());

    void Rotunjire() {
        var caz = Lot(3m, 0.333333m);
        ValoriLinii("SC-CVL-06", "primul bon", Bon(caz, 1m, 10), 0.33m);
        ValoriLinii("SC-CVL-06", "al doilea bon", Bon(caz, 1m, 11), 0.34m);
        Este("SC-CVL-06", "înaintea corecției", caz, 1m, 0.33m, 0.67m, 1m);
        var a = Corectie(caz, 0.02m);
        Este("SC-CVL-06", "Hamilton pe poziții: stoc 0,01, consum 0,01", caz, 1m, 0.34m, 0.68m, 1.02m);
        var decizii = Explicatia(a, N.FelTranzactie.Operare).Origini.Single().Explicatie.Decizii.OfType<N.AlocareValorica>().ToList();
        Verifica("SC-CVL-06", "trei decizii cu pondere 1; al doilea bon are cota zero și nicio postare",
            decizii.Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(1m, 0.01m), (1m, 0.01m), (1m, 0m)])
            && Valorice(a, N.FelTranzactie.Operare).Count == 2);
        ValoriLinii("SC-CVL-06", "ultima ieșire ia restul", Bon(caz, 1m, 18), 0.34m);
        Este("SC-CVL-06", "lot epuizat", caz, 0m, 0m, 1.02m, 1.02m);
    }

    void MaiMulteConsumuriSiLoturi() {
        var caz = Lot();
        Bon(caz, 20m, 10);
        Bon(caz, 40m, 11);
        var a = Corectie(caz, 200m);
        Este("SC-CVL-07", "două consumuri, +200", caz, 40m, 480m, 720m, 1200m);
        Verifica("SC-CVL-07", "atribuirile pe consum: 40 și 80",
            Valorice(a, N.FelTranzactie.Operare).Where(p => p.Repartitor == Loc).Select(p => p.Valoare).Order().SequenceEqual([40m, 80m]));

        var mare = Lot();
        var mic = Lot(10m, 15m);
        var doua = Draft(Zi(15), (mare, 200m), (mic, -30m));
        Opereaza(doua);
        // Datoria numără toată corecția comună: 200 − 30 peste fiecare recepție.
        Este("SC-CVL-07", "două loturi, linia +200", mare, 100m, 1200m, 0m, 1170m);
        Este("SC-CVL-07", "două loturi, linia −30", mic, 10m, 120m, 0m, 320m);
        Verifica("SC-CVL-07", "o singură Operare și o singură partidă a corecției pentru ambele loturi",
            Tranzactii(doua).Count == 1 && CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
                .Where(p => p.DocumentId == doua && p.FelUnitate == N.FelUnitate.Partida).Select(p => p.Unitate).Distinct().Count()) == 1);

        var bun = Lot();
        var epuizabil = Lot(10m, 15m);
        var mixt = Draft(Zi(15), (bun, 200m), (epuizabil, -150.01m));
        Refuza("SC-CVL-07", () => Opereaza(mixt), CoduriRefuz.CvValoareInsuficienta);
        FaraEfecte("SC-CVL-07", mixt);
        Este("SC-CVL-07", "o linie invalidă anulează toată comanda", bun, 100m, 1000m, 0m, 1000m);
    }

    void RetragereSiStorno() {
        var caz = Partial();
        var a = Corectie(caz, 200m);
        Anuleaza(a);
        FaraEfecte("SC-CVL-08", a);
        Este("SC-CVL-08", "retragerea corecției", caz, 40m, 400m, 600m, 1000m);
        Opereaza(a);
        Este("SC-CVL-08", "reoperarea nu dublează efectul", caz, 40m, 480m, 720m, 1200m);

        Storneaza(a, Zi(20));
        Este("SC-CVL-09", "storno corecție, parțial", caz, 40m, 400m, 600m, 1000m);
        Verifica("SC-CVL-09", "inversele exacte −80 și −120, cu atribuirea originalelor",
            Valorice(a, N.FelTranzactie.Storno).Select(p => p.Valoare).Order().SequenceEqual([-120m, -80m])
            && Valorice(a, N.FelTranzactie.Storno).All(p => p.InversaDinId != null && p.Atribuit != null));
        Refuza("SC-CVL-09", () => Storneaza(a, Zi(20)), "Operat");

        var intact = Lot();
        Storneaza(Corectie(intact, 200m), Zi(20));
        Este("SC-CVL-09", "storno corecție, intact", intact, 100m, 1000m, 0m, 1000m);
        var epuizat = Lot();
        Bon(epuizat, 100m);
        Storneaza(Corectie(epuizat, 200m), Zi(20));
        Este("SC-CVL-09", "storno corecție, epuizat", epuizat, 0m, 0m, 1000m, 1000m);
    }

    void OrdineaCompensarii() {
        var caz = Lot();
        var bon = Bon(caz, 60m);
        var a = Corectie(caz, 200m);
        var radacina = Valorice(a, N.FelTranzactie.Operare).Single(p => p.Repartitor == Loc);
        Storneaza(bon, Zi(20));
        Este("SC-CVL-10 / SC-X-05", "storno bon: inversa 60/600 și compensarea 120 spre stoc", caz, 100m, 1200m, 0m, 1200m);
        SoldPartida("SC-CVL-10", Partida(a, ContFurnizor)!.Value, Zi(28), -200m);

        var tranzactii = Tranzactii(a);
        var compensare = Valorice(a, N.FelTranzactie.Operare).Where(p => p.Data == Zi(20)).ToList();
        Verifica("SC-CVL-28", "comanda scrie Storno pe bon și a doua Operare, explicată, pe corecție; bonul Stornat, corecția Operată",
            Tranzactii(bon).Select(t => t.Fel).SequenceEqual([N.FelTranzactie.Operare, N.FelTranzactie.Storno])
            && tranzactii.Count == 2 && tranzactii.All(t => t.Fel == N.FelTranzactie.Operare && t.Explicata && t.Din == null)
            && StareaDocumentului(bon) == StareDocument.Stornat && StareaDocumentului(a) == StareDocument.Operat);
        Verifica("SC-CVL-26", "perechea de compensare: debit consum −120, credit stoc −120, cantitate zero, atribuită rădăcinii, cauza corecției",
            compensare.Count == 2 && compensare.All(p => p.Valoare == -120m && p.Cantitate == 0m && p.Atribuit == radacina.ID
                && p.DocumentId == a && p.LinieId == radacina.LinieId && p.InversaDinId == null && p.Pereche == compensare[0].Pereche)
            && compensare.Single(p => p.Sens == N.Sens.Debit).Repartitor == Loc
            && compensare.Single(p => p.Sens == N.Sens.Credit).Repartitor == Magazie);
        Verifica("SC-CVL-28", "postările cu cauza bonului se închid la zero pe fiecare cont",
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == bon).ToList())
                .GroupBy(p => p.Cont).All(g => g.Sum(p => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare) == 0m));
        var explicata = Explicatia(a, N.FelTranzactie.Operare).Origini.Single().Explicatie;
        Verifica("SC-CVL-31", "explicația proprie a compensării: rădăcina, beneficiarul, 120, fără ieșire",
            explicata.Decizii.Count == 1 && explicata.Decizii[0] is N.CompensareValorica { Valoare: 120m } decizie
            && decizie.Radacina.Id == radacina.ID && decizie.Beneficiar.Id == radacina.Atribuit
            && explicata.Declarant == nameof(DeclarantFacturaIntrare));
        Refuza("SC-CVL-28", () => Anuleaza(a), CoduriRefuz.CvDependenti);

        Storneaza(a, Zi(25));
        Este("SC-CVL-10", "storno corecție după compensare: net −200 pe stoc", caz, 100m, 1000m, 0m, 1000m);
        Verifica("SC-CVL-28", "stornoul corecției inversează alocările și compensarea, fără cauze străine; explicația are ambele origini",
            Valorice(a, N.FelTranzactie.Storno).Count == 4 && Tranzactii(a).Count(t => t.Fel == N.FelTranzactie.Storno) == 1
            && CuSpatiu(os => !os.GetObjectsQuery<C.Postare>().Any(p => p.Tranzactie.DocumentId == a && p.DocumentId != a))
            && Explicatia(a, N.FelTranzactie.Storno).Origini.Count == 2);

        var invers = Lot();
        var bon2 = Bon(invers, 60m);
        var b = Corectie(invers, 200m);
        Storneaza(b, Zi(20));
        Storneaza(bon2, Zi(20));
        Este("SC-CVL-11 / SC-X-06", "corecția stornată întâi: compensare zero", invers, 100m, 1000m, 0m, 1000m);
        Verifica("SC-CVL-11", "corecția stornată nu primește a doua Operare", Tranzactii(b).Count(t => t.Fel == N.FelTranzactie.Operare) == 1);
    }

    void ToateOrdinile() {
        foreach (var (prima, aDoua) in new[] { (200m, 100m), (200m, -100m) })
            foreach (var ordine in new[] { "BAC", "BCA", "ABC", "ACB", "CBA", "CAB" }) {
                var caz = Lot();
                var bon = Bon(caz, 60m);
                var corectii = new Dictionary<char, Guid> { ['A'] = Corectie(caz, prima), ['C'] = Corectie(caz, aDoua) };
                var active = new Dictionary<char, decimal> { ['A'] = prima, ['C'] = aDoua };
                var bonActiv = true;
                var corect = true;
                foreach (var pas in ordine) {
                    if (pas == 'B') { Storneaza(bon, Zi(20)); bonActiv = false; }
                    else { Storneaza(corectii[pas], Zi(20)); active[pas] = 0m; }
                    var delta = active.Values.Sum();
                    corect &= Citeste(caz) == (bonActiv
                        ? new Stare(40m, 400m + delta * 0.4m, 600m + delta * 0.6m, 1000m + delta)
                        : new Stare(100m, 1000m + delta, 0m, 1000m + delta));
                }
                Verifica("SC-CVL-12 / SC-X-07", $"corecțiile {prima} și {aDoua}, ordinea {ordine}: stările intermediare și finalul 100/1000",
                    corect && Citeste(caz) == new Stare(100m, 1000m, 0m, 1000m)
                    && corectii.Values.All(c => Tranzactii(c).Count(t => t.Fel == N.FelTranzactie.Operare) <= 2));
            }
    }

    void IesireUlterioara() {
        var caz = Partial();
        var a = Corectie(caz, 200m);
        var nou = Bon(caz, 20m, 18);
        Este("SC-CVL-13", "ieșire nouă 20/240", caz, 20m, 240m, 960m, 1200m);
        Refuza("SC-CVL-13", () => Storneaza(a, Zi(20)), CoduriRefuz.CvDependenti);
        Refuza("SC-CVL-13", () => Anuleaza(a), CoduriRefuz.CvDependenti);
        Este("SC-CVL-13", "refuzurile nu schimbă nimic", caz, 20m, 240m, 960m, 1200m);
        Storneaza(nou, Zi(20));
        Este("SC-CVL-13", "storno ieșire nouă: exact 20/240, fără compensare", caz, 40m, 480m, 720m, 1200m);
        Verifica("SC-CVL-13", "corecția nu primește Operare la stornarea unei ieșiri ulterioare", Tranzactii(a).Count == 1);
        Storneaza(a, Zi(20));
        Este("SC-CVL-13", "apoi storno corecție", caz, 40m, 400m, 600m, 1000m);
    }

    void Dependenti() {
        var caz = Lot();
        var bon = Bon(caz, 60m);
        var a = Corectie(caz, 200m);
        Refuza("SC-CVL-14", () => Anuleaza(bon), CoduriRefuz.CvDependenti);
        Este("SC-CVL-14", "retragerea bonului cu alocare refuzată, fără efecte", caz, 40m, 480m, 720m, 1200m);
        Storneaza(a, Zi(20));
        Refuza("SC-CVL-30", () => Anuleaza(bon), CoduriRefuz.CvDependenti);
        Storneaza(bon, Zi(20));
        Este("SC-CVL-30", "după corecția stornată, storno bon cu compensare zero", caz, 100m, 1000m, 0m, 1000m);
    }

    void StornoReceptie() {
        var intact = Lot();
        Anuleaza(intact.Nir);
        var a = Corectie(intact, 200m);
        Refuza("SC-CVL-14", () => Anuleaza(intact.Factura), "folosit de alte documente");
        Refuza("SC-CVL-30", () => Storneaza(intact.Factura, Zi(20)), CoduriRefuz.CvDependenti);
        Este("SC-CVL-30", "recepția cu corecție activă nu se stornează", intact, 100m, 1200m, 0m, 1200m);
        Storneaza(a, Zi(20));
        Refuza("SC-CVL-30", () => Storneaza(intact.Factura, Zi(18)), CoduriRefuz.CvCronologie);
        Refuza("SC-CVL-30", () => Anuleaza(intact.Factura), "folosit de alte documente");
        Storneaza(intact.Factura, Zi(21));
        Este("SC-CVL-30", "storno recepție după corecția neutralizată", intact, 0m, 0m, 0m, 0m);
        Verifica("SC-CVL-30", "stornoul recepției conține numai inversele ei",
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.DocumentId == intact.Factura
                && p.Tranzactie.Fel == N.FelTranzactie.Storno).ToList()).All(p => p.DocumentId == intact.Factura && p.Atribuit == null));

        var relocat = Lot();
        Anuleaza(relocat.Nir);
        var bon = Bon(relocat, 60m);
        Corectie(relocat, 200m);
        Storneaza(bon, Zi(20));
        Refuza("SC-CVL-30", () => Storneaza(relocat.Factura, Zi(25)), CoduriRefuz.CvDependenti);
        Este("SC-CVL-30", "cei 200 relocați pe stoc opresc stornarea recepției", relocat, 100m, 1200m, 0m, 1200m);
    }

    void Cronologie() {
        var caz = Lot();
        var bon = Bon(caz, 60m);
        var a = Corectie(caz, 200m);
        Refuza("SC-CVL-27", () => Storneaza(bon, Zi(14)), CoduriRefuz.CvCronologie);
        Storneaza(a, Zi(25));
        Refuza("SC-CVL-27", () => Storneaza(bon, Zi(20)), CoduriRefuz.CvCronologie);
        Este("SC-CVL-27", "la 22.01 corecția e încă activă", caz, 40m, 480m, 720m, 1200m, Zi(22));
        Este("SC-CVL-27", "după 25.01", caz, 40m, 400m, 600m, 1000m);
        Storneaza(bon, Zi(25));
        Este("SC-CVL-27", "aceeași zi, serial: compensare zero", caz, 100m, 1000m, 0m, 1000m);

        var oglinda = Lot();
        var bon2 = Bon(oglinda, 60m);
        var b = Corectie(oglinda, 200m);
        Storneaza(bon2, Zi(25));
        Refuza("SC-CVL-27", () => Storneaza(b, Zi(20)), CoduriRefuz.CvCronologie);
        Este("SC-CVL-27", "oglinda: la 22.01", oglinda, 40m, 480m, 720m, 1200m, Zi(22));
        Este("SC-CVL-27", "oglinda: după 25.01", oglinda, 100m, 1200m, 0m, 1200m);

        var antedatat = Partial();
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(8), (antedatat, 200m))), CoduriRefuz.CvCronologie);
        var corectat = Lot();
        Corectie(corectat, 200m);
        Refuza("SC-CVL-19", () => Opereaza(Bon(corectat, 10m, 12, opereaza: false)), CoduriRefuz.CvCronologie);
        Este("SC-CVL-19", "refuzurile de cronologie nu lasă efecte", corectat, 100m, 1200m, 0m, 1200m);
    }

    void Structura() {
        var caz = Lot();
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (_, _, d) => d.Cantitate = 1m, (caz, 200m))), CoduriRefuz.CvStructura);
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (os, _, d) => d.TipTva = os.GetObjectsQuery<TipTva>().First(), (caz, 200m))),
            CoduriRefuz.CvStructura);
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (caz, 200m), (caz, 100m))), CoduriRefuz.CvStructura);
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (caz, 0m))), CoduriRefuz.ValoareZero);
        Refuza("SC-CVL-18", () => Opereaza(Draft(Zi(15), (_, _, d) => d.LotId = null, (caz, 200m))), CoduriRefuz.LotLipsa);
        Refuza("SC-CVL-25", () => Opereaza(Draft(Zi(15), (os, doc, _) => {
            if (doc.Detalii.Count > 1) return;
            var obisnuita = os.CreateObject<FacturaIntrareDetaliu>();
            obisnuita.Document = doc; obisnuita.Pozitie = 9; obisnuita.TipMaterialId = Tip(os, Serviciu);
            obisnuita.Cantitate = 1m; obisnuita.PretUnitar = 50m; obisnuita.CodEconomicId = Economic;
        }, (caz, 200m))), CoduriRefuz.CvStructura);
        Refuza("SC-CVL-25", () => Opereaza(Draft(Zi(15), (_, _, d) => d.CorectieValoareLot = false, (caz, 200m))), "pozitiv");
        Este("SC-CVL-18", "refuzurile de structură nu lasă efecte", caz, 100m, 1000m, 0m, 1000m);
    }

    void Perimetru() {
        var transferat = Lot();
        Opereaza(Iesire(true, (new LinieScena(transferat.Linie, transferat.Lot, null), 10m)).Id);
        Refuza("SC-CVL-19", () => Opereaza(Draft(Zi(15), (transferat, 200m))), CoduriRefuz.CvLotNeeligibil);
    }

    void Partide() {
        var caz = Lot();
        var a = Corectie(caz, 200m);
        var (receptie, corectie) = (Partida(caz.Factura, ContFurnizor)!.Value, Partida(a, ContFurnizor)!.Value);
        var stingere = Nota(Zi(16), new LinieNtcScena(ContFurnizor, Serviciu, 50m, Furnizor, PartidaDebit: corectie));
        Opereaza(stingere.Id);
        SoldPartida("SC-CVL-24", corectie, Zi(28), -150m);
        SoldPartida("SC-CVL-24", receptie, Zi(28), -1000m);
        Refuza("SC-CVL-24", () => Storneaza(a, Zi(20)), CoduriRefuz.PartidaCuDependenti);
        Este("SC-CVL-24", "refuzul nu schimbă lotul", caz, 100m, 1200m, 0m, 1200m);
        Storneaza(stingere.Id, Zi(18));
        SoldPartida("SC-CVL-24", corectie, Zi(28), -200m);
        Storneaza(a, Zi(20));
        SoldPartida("SC-CVL-24", corectie, Zi(28), 0m);
        Este("SC-CVL-24", "storno corecție după neutralizarea stingerii", caz, 100m, 1000m, 0m, 1000m);

        var minus = Lot();
        var b = Corectie(minus, -200m);
        (receptie, corectie) = (Partida(minus.Factura, ContFurnizor)!.Value, Partida(b, ContFurnizor)!.Value);
        SoldPartida("SC-CVL-24", receptie, Zi(28), -1000m);
        SoldPartida("SC-CVL-24", corectie, Zi(28), 200m);
        Opereaza(Nota(Zi(16), new LinieNtcScena(ContFurnizor, ContFurnizor, 200m, Furnizor, Furnizor,
            PartidaDebit: receptie, PartidaCredit: corectie)).Id);
        SoldPartida("SC-CVL-24", receptie, Zi(28), -800m);
        SoldPartida("SC-CVL-24", corectie, Zi(28), 0m);
    }

    void PerioadaInchisa() {
        var februarie = Partial();
        var ianuarie = Lot();
        var bon = Bon(ianuarie, 60m);
        var a = Corectie(ianuarie, 200m);
        var cuBonStornat = Lot();
        var bon2 = Bon(cuBonStornat, 60m);
        Corectie(cuBonStornat, 200m);
        var corectat = Lot();
        Bon(corectat, 60m);
        var c = Corectie(corectat, 200m);
        var castiga = Partial();
        var pierde = Partial();
        var cititor = PregatesteCititori();
        var saft = PregatesteSaft();
        CorectieInainteaInchiderii(castiga);
        Cititori(cititor);
        StornoBonSaft(saft);

        Corectie(februarie, 200m, 5, 2);
        Este("SC-CVL-15", "corecție în februarie pe consum din ianuarie", februarie, 40m, 480m, 720m, 1200m);
        Este("SC-CVL-15", "31.01 rămâne neschimbat", februarie, 40m, 400m, 600m, 1000m, Zi(31));

        Refuza("SC-CVL-16", () => Anuleaza(a), "închis");
        Refuza("SC-CVL-16", () => CuSpatiu(os => ComenziDocument.Sistem(os).Storneaza(a, Zi(31))), "închis");
        Storneaza(a, Zi(5, 2));
        Este("SC-CVL-16", "storno corecție în februarie", ianuarie, 40m, 400m, 600m, 1000m);
        Este("SC-CVL-16", "lectura la 31.01 păstrează corecția", ianuarie, 40m, 480m, 720m, 1200m, Zi(31));
        Storneaza(bon2, Zi(5, 2));
        Este("SC-CVL-16", "storno bon în februarie, cu corecția activă", cuBonStornat, 100m, 1200m, 0m, 1200m);
        Este("SC-CVL-16", "ianuarie neschimbat", cuBonStornat, 40m, 480m, 720m, 1200m, Zi(31));
        Verifica("SC-CVL-16", "bonul din ianuarie rămâne cu valoarea lui", ValoriLinii(bon).SequenceEqual([600m]));

        var inlocuitor = Corecteaza(c);
        corectat.Documente.Add(inlocuitor);
        Este("SC-CVL-17", "Corectează: inversa în februarie", corectat, 40m, 400m, 600m, 1000m);
        Comanda(os => {
            var linie = os.GetObjectsQuery<DocumentDetaliu>().Single(d => d.DocumentId == inlocuitor);
            linie.Valoare = 150m; os.CommitChanges();
        });
        Opereaza(inlocuitor);
        Este("SC-CVL-17", "înlocuitorul +150: efect net +150", corectat, 40m, 460m, 690m, 1150m);
        Este("SC-CVL-17", "ianuarie neschimbat", corectat, 40m, 480m, 720m, 1200m, Zi(31));
        InchidereaInainteaCorectiei(pierde);
        Saft(saft);
    }
}
