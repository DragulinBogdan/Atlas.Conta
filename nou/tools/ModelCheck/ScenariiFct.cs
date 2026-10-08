using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiFct(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "FCT", 2002) {
    protected override void Executa() {
        Simpla();
        Multiple();
        StornoSiAnulare();
        Fiscal();
        Refuzuri();
        Dependenti();
        AnalizaReceptiei();
        RegulaReceptiei();
        PestePerioada();
    }

    // 112: operand scris de mână — politica de TVA și regula de servicii duc spre alte conturi decât recepția.
    void RegulaReceptiei() {
        Guid stoc = Guid.NewGuid(), datorie = Guid.NewGuid(), alTaxei = Guid.NewGuid(), alServiciilor = Guid.NewGuid();
        Guid tip = Guid.NewGuid(), produs = Guid.NewGuid(), linie = Guid.NewGuid(), lot = Guid.NewGuid();
        var predator = new RepartitorFapt(Guid.NewGuid(), FelRepartitor.Partener, datorie, default);
        var primitor = new RepartitorFapt(Guid.NewGuid(), FelRepartitor.Gestiune, null, default);
        RegulaContareFapt Regula(NaturaClasa natura, SursaCont sursaCredit, Guid credit) => new(Guid.NewGuid(), null, natura,
            null, SursaCont.TipMaterial, null, sursaCredit, credit, true, null, null, null);
        var receptie = Regula(NaturaClasa.Stoc, SursaCont.RepartitorPredator, datorie);
        var servicii = Regula(NaturaClasa.Serviciu, SursaCont.Explicit, alServiciilor);
        var operand = new Operand(
            new(Guid.NewGuid(), "FCT", Guid.NewGuid(), Ianuarie, Ianuarie, "F-1", false, null, predator, primitor,
                null, null, null),
            [new(linie, tip, null, NaturaClasa.Stoc, stoc, lot,
                new LotFapt(lot, produs, tip, stoc, Ianuarie, 10) { LinieIntrareId = linie, GestiuneId = primitor.Id },
                10, 100, 0, null, 10, produs, tip, null, null, null, null, N.Analiza.Fara, null)],
            [receptie, servicii], [],
            new PoliticaTvaFapt(DirectieTva.Deductibil, SursaCont.Explicit, alTaxei) { Id = Guid.NewGuid() },
            new Dictionary<Guid, TipTvaFapt>(),
            new Dictionary<Guid, ContFapt> { [stoc] = new(stoc, "S", false, false), [datorie] = new(datorie, "F", true, false),
                [alTaxei] = new(alTaxei, "T", false, false), [alServiciilor] = new(alServiciilor, "V", false, false) },
            new Dictionary<CheieLotFapt, N.Sold>(), null, [], null, null, null, new(An, 1)) {
                Repartitori = new Dictionary<Guid, RepartitorFapt> { [predator.Id] = predator, [primitor.Id] = primitor },
            };
        (N.Declaratie Declaratie, List<N.Refuz> Refuzuri) Declara(params RegulaContareFapt[] reguli) {
            var refuzuri = new List<N.Refuz>();
            return (DeclarantFacturaIntrare.Instanta.Declara(operand with { ReguliContare = reguli },
                new N.Rotunjire(MidpointRounding.AwayFromZero), refuzuri), refuzuri);
        }

        var (pe, refuzate) = Declara(receptie, servicii);
        Verifica("SC-FCT-15", "D stoc 100/+10 pe lot în gestiunea primitoare, C furnizorul regulii 100 pe gestiunea Furnizor, cu partidă",
            refuzate.Count == 0 && pe.Miscari is [var m] && m.Cantitate == 10 && m.Valoare == 100
            && m.La.Cont == stoc && m.La.Unitate?.Id == lot && m.La.Gestiune == primitor.Id
            && m.DeLa.Cont == datorie && m.DeLa.Gestiune == N.GestiuniVirtuale.Furnizor
            && m.DeLa.Unitate is { Fel: N.FelUnitate.Partida });
        Verifica("SC-FCT-15", "ambele conturi sunt decise de regula naturii Stoc, singura politică consumată",
            refuzate.Count == 0
            && pe.Decizii.OfType<N.ContRezolvat>().Select(d => (d.Cont, d.Regula)).ToArray() is [var debit, var credit]
            && debit == (stoc, receptie.Id) && credit == (datorie, receptie.Id)
            && pe.Ipoteze.OfType<N.VersiunePolitica>().ToArray() is [{ Fel: nameof(RegulaContare) } consumata]
            && consumata.Rand == receptie.Id);
        var (mutata, refuzMutata) = Declara(receptie with { SursaContCredit = SursaCont.Explicit, ContCreditId = alServiciilor }, servicii);
        Verifica("SC-FCT-15", "creditul regulii mutat pe un cont explicit fără partide mută contul recepției",
            refuzMutata.Count == 0 && mutata.Miscari is [var mm] && mm.DeLa.Cont == alServiciilor && mm.DeLa.Unitate == null
            && mm.La.Cont == stoc && mm.Cantitate == 10 && mm.Valoare == 100);

        var (fara, refuzFara) = Declara(servicii);
        Verifica("SC-FCT-16", "fără regula naturii Stoc recepția e refuzată pe linie, cu regula de servicii și politica de TVA prezente",
            fara == null && refuzFara is [{ Cod: CoduriRefuz.RegulaContareLipsa } lipsa] && lipsa.Linie == linie);
        var (altCont, refuzAltCont) = Declara(receptie with { SursaContDebit = SursaCont.Explicit, ContDebitId = alServiciilor }, servicii);
        Verifica("SC-FCT-16", "debitul regulii pe alt cont decât al lotului: CONT_STOC_LIPSA",
            altCont == null && refuzAltCont is [{ Cod: CoduriRefuz.ContStocLipsa }]);
    }

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        purja.Adauga(os.GetObjectsQuery<Angajament>().Where(a => a.Cod == Marcaj));

    // D9-A3: analiza obligatorie a recepției se judecă la operarea facturii.
    void AnalizaReceptiei() {
        if (Privat) return;
        var politica = CuSpatiu(os => os.GetObjectsQuery<PoliticaValidare>().Single(p => p.TipDocument.Cod == "FCT").ID);
        void Clasificatie(bool ceruta) => Comanda(os => {
            os.GetObjectByKey<PoliticaValidare>(politica).CereClasificatieBugetara = ceruta; os.CommitChanges();
        });
        var angajament = CuSpatiu(os => {
            var a = os.CreateObject<Angajament>(); a.Cod = Marcaj; a.Denumire = Marcaj; os.CommitChanges(); return a.ID;
        });
        FacturaScena Culeasa(Guid? economic, Guid? angajat) {
            var f = Factura(Ianuarie, new LinieFctScena(10, 10));
            Comanda(os => {
                var l = os.GetObjectByKey<FacturaIntrareDetaliu>(f.Linii[0].Id);
                l.CodEconomicId = economic; l.AngajamentId = angajat; os.CommitChanges();
            });
            return f;
        }
        RandScena[] Receptia(FacturaScena f, Guid? economic) =>
            [.. Randuri(f, 0, 10, 100).Select(r => r with { Economic = economic })];
        Verifica("SC-FCT-12", "contul furnizorului cere cod economic, contul de stoc nu cere nimic", CuSpatiu(os =>
            os.GetObjectByKey<Cont>(Cont(ContFurnizor)).DimensiuniObligatorii.HasFlag(DimensiuneFlags.CodEconomic)
            && os.GetObjectByKey<Cont>(Cont(Stoc)).DimensiuniObligatorii == DimensiuneFlags.Niciuna));
        Clasificatie(false);
        try {
            var fara = Culeasa(null, null);
            var dryRun = CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(fara.Id));
            Verifica("SC-FCT-12", "fără cod economic și fără angajament: dry-run refuzat pe contul furnizorului — "
                + string.Join(" | ", dryRun), dryRun.Count == 1 && dryRun[0].Contains(ContFurnizor)
                && dryRun[0].Contains("credit") && dryRun[0].EndsWith("cere: Cod economic."));
            Refuza("SC-FCT-12", () => Opereaza(fara.Id), "cere: Cod economic"); FaraEfecte("SC-FCT-12", fara.Id);
            Verifica("SC-FCT-12", "refuzul nu naște NIR conex", CuSpatiu(os =>
                !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == fara.Id)));

            var angajata = Culeasa(null, angajament);
            var nirAngajat = Opereaza(angajata.Id).ConexId!.Value;
            Postari("SC-FCT-13", angajata.Id, N.FelTranzactie.Operare, Ianuarie, Receptia(angajata, null));
            Opereaza(nirAngajat);
            SoldLot("SC-FCT-13", angajata.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);

            var explicita = Culeasa(Economic, null);
            var nirExplicit = Opereaza(explicita.Id).ConexId!.Value;
            Postari("SC-FCT-14", explicita.Id, N.FelTranzactie.Operare, Ianuarie, Receptia(explicita, Economic));
            Opereaza(nirExplicit);
            SoldLot("SC-FCT-14", explicita.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);
        }
        finally { Clasificatie(true); }
    }

    RandScena[] Randuri(FacturaScena f, int index, decimal q, decimal net, decimal taxa = 0,
        string tva = null, int? perioada = null) {
        var l = f.Linii[index];
        var intern = l.Lot != null ? Stoc : Serviciu;
        var cont = Cont(intern);
        if (!Privat) { net += taxa; taxa = 0; tva = null; }
        var tv = tva == null ? (Guid?)null : Tva(tva);
        var debit = new RandScena(cont, N.Latura.Debit, net, q, Magazie, l.Lot, l.Produs,
            tv != null ? Furnizor : null, l.Id, tv, tv != null ? N.RolTva.Baza : null,
            tv != null ? perioada ?? An * 100 + 1 : null, tv != null ? N.SensTva.Achizitie : null,
            l.Lot != null ? N.Spatiu.Stoc : N.Spatiu.Contabil, Economic);
        var credit = new RandScena(Cont(ContFurnizor), N.Latura.Credit, net, -q,
            l.Lot != null ? N.GestiuniVirtuale.Furnizor : null, Partida(f.Id, ContFurnizor),
            l.Produs, Furnizor, l.Id, Economic: Economic);
        var randuri = new List<RandScena> { debit, credit };
        if (taxa != 0) {
            randuri.Add(debit with { Cont = Privat ? Cont("4426") : cont, Cantitate = 0, Valoare = taxa,
                Unitate = Privat ? null : l.Lot, Spatiu = Privat ? N.Spatiu.Contabil : debit.Spatiu, Rol = N.RolTva.Taxa });
            randuri.Add(credit with { Cantitate = 0, Valoare = taxa, Gestiune = Privat ? null : credit.Gestiune });
        }
        return randuri.ToArray();
    }

    void Simpla() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10));
        Verifica("SC-FCT-01", "dry-run acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(f.Id)).Count == 0);
        FaraEfecte("SC-FCT-01", f.Id);
        var nir = Opereaza(f.Id).ConexId!.Value;
        Postari("SC-FCT-01", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 10, 100));
        SoldLot("SC-FCT-01", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);
        SoldPartida("SC-FCT-01", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -100);
        var inainte = Amprenta(f.Id);
        Opereaza(nir);
        Verifica("SC-X-02", "NIR conex nu schimbă postările FCT", Amprenta(f.Id) == inainte);
        SoldLot("SC-X-02", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, 100);
    }

    void Multiple() {
        var f = Factura(Ianuarie, new LinieFctScena(4, 10), new LinieFctScena(2, 15));
        Opereaza(f.Id);
        Postari("SC-FCT-02", f.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(f, 0, 4, 40), .. Randuri(f, 1, 2, 30)]);
        SoldLot("SC-FCT-02", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 4, 40);
        SoldLot("SC-FCT-02", f.Linii[1].Lot!.Value, Magazie, Ianuarie, 2, 30);
        SoldPartida("SC-FCT-02", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -70);
    }

    void StornoSiAnulare() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10)); Opereaza(f.Id);
        var data = new DateOnly(An, 1, 20);
        Storneaza(f.Id, data);
        Postari("SC-FCT-03", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 10, 100));
        Postari("SC-FCT-03", f.Id, N.FelTranzactie.Storno, data, Randuri(f, 0, -10, -100));
        SoldLot("SC-FCT-03", f.Linii[0].Lot!.Value, Magazie, data, 0, 0);
        SoldPartida("SC-FCT-03", Partida(f.Id, ContFurnizor)!.Value, data, 0);
        var neschimbat = Amprenta(f.Id);
        Refuza("SC-FCT-03", () => Storneaza(f.Id, data), "Operat");
        Verifica("SC-FCT-03", "storno repetat nu scrie", Amprenta(f.Id) == neschimbat);
        var a = Factura(Ianuarie, new LinieFctScena(10, 10)); var conex = Opereaza(a.Id).ConexId!.Value;
        Anuleaza(a.Id); FaraEfecte("SC-FCT-05", a.Id);
        Verifica("SC-FCT-05", "conex Draft eliminat; lotul de culegere păstrat fără sold", CuSpatiu(os =>
            !os.GetObjectsQuery<Document>().Any(d => d.ID == conex)
            && os.GetObjectsQuery<Lot>().Any(l => l.ID == a.Linii[0].Lot)));
        SoldLot("SC-FCT-05", a.Linii[0].Lot!.Value, Magazie, data, 0, 0);
    }

    void Fiscal() {
        var t21 = Privat ? "N21" : "CAP21"; var t11 = Privat ? "N11" : "CAP11";
        var f = Factura(Ianuarie, new LinieFctScena(10, 10, t21), new LinieFctScena(5, 10, t11)); Opereaza(f.Id);
        Postari("SC-FCT-07", f.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(f, 0, 10, 100, 21, t21), .. Randuri(f, 1, 5, 50, 5.50m, t11)]);
        SoldLot("SC-FCT-07", f.Linii[0].Lot!.Value, Magazie, Ianuarie, 10, Privat ? 100 : 121);
        SoldLot("SC-FCT-07", f.Linii[1].Lot!.Value, Magazie, Ianuarie, 5, Privat ? 50 : 55.50m);
        if (!Privat) return;
        foreach (var cod in new[] { "NED21", "TI21" }) {
            var special = Factura(Ianuarie, new LinieFctScena(1, 100, cod, false)); Opereaza(special.Id);
            var r = Randuri(special, 0, 0, 100, 21, cod);
            if (cod == "NED21") r[2] = r[2] with { Cont = Cont(Serviciu) };
            else r[3] = r[3] with { Cont = Cont("4427"), Partener = Furnizor, Unitate = null,
                Tva = Tva(cod), Rol = N.RolTva.Autocolectare, Sens = N.SensTva.Achizitie, Perioada = An * 100 + 1 };
            Postari("SC-X-14", special.Id, N.FelTranzactie.Operare, Ianuarie, r);
            if (cod == "TI21") TaxareInversa(special.Id);
            Storneaza(special.Id, Ianuarie);
            Postari("SC-X-14", special.Id, N.FelTranzactie.Storno, Ianuarie,
                r.Select(p => p with { Valoare = -p.Valoare }).ToArray());
        }
        SoldPartida("SC-FCT-07", Partida(f.Id, ContFurnizor)!.Value, Ianuarie, -176.50m);
        var mic = Factura(Ianuarie, new LinieFctScena(1, 0.01m, "N21", false), new LinieFctScena(1, 0.01m, "N21", false));
        Opereaza(mic.Id);
        Postari("SC-FCT-08", mic.Id, N.FelTranzactie.Operare, Ianuarie,
            [.. Randuri(mic, 0, 0, 0.01m, 0, "N21"), .. Randuri(mic, 1, 0, 0.01m, 0, "N21")]);
        SoldPartida("SC-FCT-08", Partida(mic.Id, ContFurnizor)!.Value, Ianuarie, -0.02m);
        var cules = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", false, 21.01m)); Opereaza(cules.Id);
        Postari("SC-FCT-08", cules.Id, N.FelTranzactie.Operare, Ianuarie,
            Randuri(cules, 0, 0, 100, 21.01m, "N21"));
        SoldPartida("SC-FCT-08", Partida(cules.Id, ContFurnizor)!.Value, Ianuarie, -121.01m);
        var trei = Factura(Ianuarie, new LinieFctScena(1, 10.03m, "N21", false), new LinieFctScena(1, 10.03m, "N21", false),
            new LinieFctScena(1, 10.03m, "N21", false));
        Opereaza(trei.Id);
        Postari("SC-FCT-11", trei.Id, N.FelTranzactie.Operare, Ianuarie, [.. Randuri(trei, 0, 0, 10.03m, 2.11m, "N21"),
            .. Randuri(trei, 1, 0, 10.03m, 2.11m, "N21"), .. Randuri(trei, 2, 0, 10.03m, 2.10m, "N21")]);
        SoldPartida("SC-FCT-11", Partida(trei.Id, ContFurnizor)!.Value, Ianuarie, -36.41m);
        Verifica("SC-FCT-11", "documentul operat poartă taxa postată: 2,11 / 2,11 / 2,10, total 36,41 = totalul de stins",
            CuSpatiu(os => {
                var d = os.GetObjectByKey<FacturaIntrare>(trei.Id);
                return d.Detalii.OrderBy(l => l.Pozitie).Select(l => l.ValoareTva).ToArray() is [2.11m, 2.11m, 2.10m]
                    && d.Total == 36.41m && Atlas.Conta.BackOffice.Module.Motor.ImperechereService.Total(os, trei.Id) == 36.41m;
            }));
    }

    void Refuzuri() {
        foreach (var (id, cod, fragment) in new[] {
            ("SC-FCT-09a", CoduriRefuz.NumarLipsa, "număr"),
            ("SC-FCT-09b", CoduriRefuz.CantitateNepozitiva, "cantitate"),
            ("SC-FCT-09c", CoduriRefuz.LotLipsa, "lot") }) {
            var f = Factura(Ianuarie, new LinieFctScena(1, 10));
            using (var os = Deschide()) {
                var doc = os.GetObjectsQuery<FacturaIntrare>().Single(d => d.ID == f.Id);
                var l = os.GetObjectsQuery<FacturaIntrareDetaliu>().Single(d => d.DocumentId == f.Id);
                if (id.EndsWith('a')) doc.Numar = null;
                if (id.EndsWith('b')) l.Cantitate = 0;
                if (id.EndsWith('c')) { l.Lot = null; l.LotId = null; }
                os.CommitChanges();
            }
            RefuzDeclaratie(id, f.Id, cod);
            Verifica(id, "dry-run refuzat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(f.Id)).Count > 0);
            FaraEfecte(id, f.Id);
            Refuza(id, () => Opereaza(f.Id), fragment);
            FaraEfecte(id, f.Id);
        }
    }

    void Dependenti() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10)); var nir = Opereaza(f.Id).ConexId!.Value;
        Opereaza(nir);
        var bcs = Consum(f.Linii[0].Lot!.Value, 4); Opereaza(bcs);
        var amprente = new[] { f.Id, nir, bcs }.Select(Amprenta).ToArray();
        Refuza("SC-X-01", () => Storneaza(f.Id, new(An, 1, 20)), "conex");
        Comanda(os => ComenziDocument.Sistem(os).Storneaza(nir, new(An, 1, 20)));
        Verifica("SC-X-01", "storno NIR cu deltă zero schimbă starea, fără inversă economică",
            CuSpatiu(os => os.GetObjectByKey<Document>(nir).Stare) == StareDocument.Stornat);
        Verifica("SC-X-01", "refuzul FCT și storno NIR păstrează postările întregului lanț",
            new[] { f.Id, nir, bcs }.Select(Amprenta).SequenceEqual(amprente));
        SoldLot("SC-X-01", f.Linii[0].Lot!.Value, Magazie, new(An, 1, 20), 6, 60);
        SoldPartida("SC-X-01", Partida(f.Id, ContFurnizor)!.Value, new(An, 1, 20), -100);
    }

    void TaxareInversa(Guid factura) {
        Verifica("SC-FCT-10", "total de stins 100 pe antet și pe cub, fără taxa autolichidată", CuSpatiu(os =>
            Atlas.Conta.BackOffice.Module.Motor.ImperechereService.Total(os, factura) == 100m));
        Verifica("SC-FCT-10", "plata autogenerată plătește 100", CuSpatiu(os => {
            var f = os.GetObjectByKey<FacturaIntrare>(factura); f.GenereazaPlata = true;
            var plata = f.GenereazaSecundar(os);
            return os.ModifiedObjects.OfType<DocumentDetaliu>().Where(d => d.Document == plata).Sum(d => d.Valoare) == 100m;
        }));
    }

    void PestePerioada() {
        var tva = Privat ? "N21" : "CAP21";
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, tva, false)); Opereaza(f.Id);
        var c = Factura(Ianuarie, new LinieFctScena(1, 100, tva, false)); Opereaza(c.Id);
        InchideIanuarie();
        var inainte = Amprenta(f.Id);
        Refuza("SC-FCT-04", () => Anuleaza(f.Id), "închis");
        Verifica("SC-FCT-04", "anularea refuzată păstrează postările", Amprenta(f.Id) == inainte);
        Storneaza(f.Id, Februarie);
        Postari("SC-FCT-04", f.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(f, 0, 0, 100, 21, tva));
        Postari("SC-FCT-04", f.Id, N.FelTranzactie.Storno, Februarie, Randuri(f, 0, 0, -100, -21, tva, An * 100 + 2));
        var nou = Corecteaza(c.Id); FaraEfecte("SC-FCT-06", nou);
        Postari("SC-FCT-06", c.Id, N.FelTranzactie.Storno, Februarie, Randuri(c, 0, 0, -100, -21, tva));
        FacturaScena corectie;
        using (var os = Deschide()) {
            var d = os.GetObjectsQuery<FacturaIntrare>().Single(d => d.ID == nou);
            Verifica("SC-FCT-06", "corecție legată, motiv și date", d.CorecteazaId == c.Id
                && d.MotivCorectie == MotivCorectie.EroareMateriala && d.Data == Ianuarie && d.DataInregistrare == Februarie);
            var l = os.GetObjectsQuery<FacturaIntrareDetaliu>().Single(l => l.DocumentId == nou);
            l.PretUnitar = 80; l.ValoareTva = 0;
            corectie = new(nou, [new(l.ID, null, null)]); os.CommitChanges();
        }
        Opereaza(nou);
        Postari("SC-FCT-06", nou, N.FelTranzactie.Operare, Februarie, Randuri(corectie, 0, 0, 80, 16.80m, tva));
        Postari("SC-FCT-06", c.Id, N.FelTranzactie.Operare, Ianuarie, Randuri(c, 0, 0, 100, 21, tva));
        {
            SoldPartida("SC-FCT-06", Partida(c.Id, ContFurnizor)!.Value, new(An, 1, 31), -121);
            SoldPartida("SC-FCT-06", Partida(c.Id, ContFurnizor)!.Value, Februarie, 0);
            SoldPartida("SC-FCT-06", Partida(nou, ContFurnizor)!.Value, Februarie, -96.80m);
        }
    }
}
