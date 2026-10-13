using CitireCumul = Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Catalogul `docs/nucleu/scenarii/C111.md`: formele coordonatelor din C111-D5 și schimbările S1–S14.</summary>
static class ScenariiC111 {
    static readonly string[] Comune = ["citiri", "stoc", "suport", "deschidere", "nir", "feluri", "virament"];
    // Faptul fiscal și SAF-T sunt ale profilului privat.
    static readonly string[] NumaiPrivat = ["decont", "avans", "tva-explicit", "tva-partener", "saft", "pozitie"];

    public static void Ruleaza(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
            Action<IObjectSpace, int, int> inchide) {
        string[] cazuri = privat ? [.. NumaiPrivat, .. Comune] : Comune;
        foreach (var caz in cazuri)
            try { new ScenaC111(deschide, check, privat, inchide, caz).Ruleaza(); }
            catch (Exception e) {
                check($"SC-C111 ({(privat ? "privat" : "bugetar")}) cazul „{caz}”: excepție neașteptată — {e.Message.Split('\n')[0]}", false);
                Console.WriteLine(e);
            }
    }
}

sealed class ScenaC111(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
    Action<IObjectSpace, int, int> inchide, string caz)
    : ScenaDocumente(deschide, check, privat, inchide, "C111", 2033) {
    const N.Sens Debit = N.Sens.Debit, Credit = N.Sens.Credit;
    const N.FelTranzactie Operare = N.FelTranzactie.Operare, Transfer = N.FelTranzactie.Transfer, Storno = N.FelTranzactie.Storno;
    string Avans => Privat ? "542" : "542.01.00";
    string Activ => Privat ? "214" : "214.00.00";
    string Capital => Privat ? "1012" : "117.00.00";
    string Numerar => Privat ? "5311" : "531.01.01";
    int Perioada => An * 100 + 1;
    DateOnly Sfarsit => new(An, 1, 31);
    int numar;

    public new void Ruleaza() {
        try { base.Ruleaza(); }
        finally { PurjeazaConturi(); }
    }

    protected override void Executa() {
        PurjeazaConturi();
        switch (caz) {
            case "decont": CazDecont(); break;
            case "citiri": CazCitiri(); break;
            case "stoc": CazStoc(); break;
            case "avans": CazAvans(); break;
            case "tva-explicit": CazTvaExplicit(); break;
            case "tva-partener": CazTvaPartener(); break;
            case "saft": CazSaft(); break;
            case "suport": CazSuport(); break;
            case "pozitie": CazPozitie(); break;
            case "deschidere": CazDeschidere(); break;
            case "nir": CazNir(); break;
            case "feluri": CazFeluri(); break;
            case "virament": CazVirament(); break;
            default: throw new ArgumentOutOfRangeException(nameof(caz), caz, null);
        }
    }

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja purja) {
        var inceput = new DateOnly(An, 1, 1); var capat = new DateOnly(An, 12, 31);
        var deschideri = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.Fel == N.FelTranzactie.Deschidere
            && t.Data >= inceput && t.Data <= capat).Select(t => t.ID).ToList();
        purja.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => deschideri.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        purja.AdaugaCheie<C.Tranzactie>(deschideri);
    }

    // Conturile scenei sunt ținute de tipuri și repartitori: se purjează după ei.
    void PurjeazaConturi() => Comanda(os => {
        var purja = new Purja(os);
        purja.Adauga(os.GetObjectsQuery<Cont>().Where(c => c.Simbol.StartsWith(Marcaj)));
        purja.Executa();
    });

    // ───────────────────────── infrastructura cazurilor ─────────────────────────

    static RandScena Rand(C.Postare p) => new(p.Cont, p.Sens, p.Valoare, p.Cantitate,
        p.Repartitor, p.Unitate, p.Produs, p.PartenerFiscal, p.LinieId, p.TipTvaId, p.RolTva,
        p.PerioadaDeclarare, p.SensTva, p.Spatiu, p.CodEconomic, p.Carte, p.Contrapondere, p.Loc);

    void Randuri(string id, string mesaj, IReadOnlyCollection<C.Postare> postari, params RandScena[] asteptate) {
        var actual = postari.Select(Rand).ToList();
        var ok = actual.Count == asteptate.Length
            && asteptate.All(a => actual.Count(r => r == a) == asteptate.Count(r => r == a));
        Verifica(id, $"{mesaj}: {asteptate.Length} postări cu coordonatele și măsurile așteptate", ok);
        if (ok) return;
        foreach (var r in actual.Except(asteptate)) Console.WriteLine("     ÎN PLUS " + r);
        foreach (var r in asteptate.Except(actual)) Console.WriteLine("     LIPSĂ " + r);
    }

    List<C.Postare> Cub(Guid doc, N.FelTranzactie fel) => CuSpatiu(os => os.GetObjectsQuery<C.Postare>()
        .Where(p => p.DocumentId == doc && p.Tranzactie.Fel == fel).ToList());

    bool Trece(string id, string mesaj, Action actiune) {
        try { actiune(); return true; }
        catch (OperareException e) {
            Verifica(id, $"{mesaj} — refuzat: {e.Message.Split('\n')[0]}", false);
            return false;
        }
    }

    static RandScena[] Inverse(IEnumerable<RandScena> randuri) =>
        [.. randuri.Select(r => r with { Valoare = -r.Valoare, Cantitate = -r.Cantitate })];

    Guid ContScena(string sufix, Action<Cont> configureaza = null) => CuSpatiu(os => {
        var c = os.CreateObject<Cont>(); c.Simbol = Marcaj + sufix; c.Denumire = c.Simbol;
        configureaza?.Invoke(c);
        os.CommitChanges(); return c.ID;
    });

    Guid PartenerNou(string sufix) => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + sufix; p.Denumire = p.Cod;
        os.CommitChanges(); return p.ID;
    });

    Guid TipPe(string sufix, string model, Guid cont) => CuSpatiu(os => {
        var clasa = os.GetObjectByKey<TipMaterial>(Tip(os, model)).ClasaId;
        var t = os.CreateObject<TipMaterial>(); t.Cod = Marcaj + sufix; t.Denumire = t.Cod;
        t.ClasaId = clasa; t.ContImplicitId = cont;
        os.CommitChanges(); return t.ID;
    });

    FacturaScena NotaPe(DateOnly data, Guid debit, Guid credit, decimal valoare, Guid? repartitorDebit = null,
            Guid? repartitorCredit = null) => CuSpatiu(os => {
        var doc = os.CreateObject<NotaContabila>(); doc.Data = data; doc.PredatorId = Loc; doc.PrimitorId = Loc;
        var l = os.CreateObject<NotaContabilaDetaliu>(); l.Document = doc; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, "TRZ"); l.ContDebitId = debit; l.ContCreditId = credit; l.Valoare = valoare;
        l.RepartitorDebitId = repartitorDebit; l.RepartitorCreditId = repartitorCredit; l.CodEconomicId = Economic;
        os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, null, null)]);
    });

    FacturaScena DecontCules(Guid titular, string tva, Action<DecontDetaliu> linie = null) => CuSpatiu(os => {
        var d = os.CreateObject<Decont>(); d.Data = Ianuarie; d.PredatorId = titular; d.PrimitorId = Loc;
        var l = os.CreateObject<DecontDetaliu>(); l.Document = d; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Serviciu); l.Cantitate = 1; l.PretUnitar = 100; l.CodEconomicId = Economic;
        if (tva != null) l.TipTvaId = Tva(tva);
        linie?.Invoke(l);
        os.CommitChanges(); return new FacturaScena(d.ID, [new(l.ID, null, null)]);
    });

    FacturaScena Vanzare(params (Guid Tip, decimal Pret, string Tva)[] linii) => CuSpatiu(os => {
        var d = os.CreateObject<FacturaIesire>(); d.Data = Ianuarie; d.PredatorId = Magazie; d.PrimitorId = Client;
        var rezultat = new List<LinieScena>();
        foreach (var (tip, pret, tva) in linii) {
            var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = rezultat.Count + 1;
            l.Cantitate = 1; l.PretUnitar = pret; l.TipMaterialId = tip; l.CodEconomicId = Economic;
            if (tva != null) l.TipTvaId = Tva(tva);
            rezultat.Add(new(l.ID, null, null));
        }
        os.CommitChanges(); return new FacturaScena(d.ID, [.. rezultat]);
    });

    // Politica de TVA a tipului, schimbată pe durata acțiunii și readusă la valorile găsite.
    void CuPoliticaTva(string tipDocument, SursaCont sursa, Guid? fallback, Action actiune) {
        var (id, sursaVeche, fallbackVechi) = CuSpatiu(os => {
            var p = os.GetObjectsQuery<PoliticaTva>().Single(p => p.TipDocument.Cod == tipDocument);
            return (p.ID, p.SursaContrapartida, p.ContrapartidaFallbackId);
        });
        void Seteaza(SursaCont s, Guid? f) => Comanda(os => {
            var p = os.GetObjectByKey<PoliticaTva>(id); p.SursaContrapartida = s; p.ContrapartidaFallbackId = f;
            os.CommitChanges();
        });
        Seteaza(sursa, fallback ?? fallbackVechi);
        try { actiune(); }
        finally { Seteaza(sursaVeche, fallbackVechi); }
    }

    // ───────────────────────── SC-C111-04, 23, 24: decontul ─────────────────────────

    void CazDecont() {
        var titular = CuSpatiu(os => {
            var a = os.CreateObject<Angajat>(); a.Cod = Marcaj + "-A"; a.Denumire = a.Cod; a.ContImplicitId = Cont(Avans);
            os.CommitChanges(); return a.ID;
        });
        var n21 = Tva("N21");
        RandScena[] Asteptate(FacturaScena d, Guid contBaza, Guid repartitorBaza, Guid? unitateBaza = null) {
            var l = d.Linii[0].Id;
            var avans = N.UnitateSold.DeschidePartida(Cont(Avans), titular, d.Id, Ianuarie).Id;
            return [
                new(contBaza, Debit, 100, Repartitor: repartitorBaza, Unitate: unitateBaza, PartenerFiscal: titular, Linie: l,
                    Tva: n21, Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Loc),
                new(Cont(Avans), Credit, 100, Repartitor: titular, Unitate: avans, Linie: l),
                new(Cont("4426"), Debit, 21, Repartitor: Loc, PartenerFiscal: titular, Linie: l,
                    Tva: n21, Rol: N.RolTva.Taxa, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Loc),
                new(Cont(Avans), Credit, 21, Repartitor: titular, Unitate: avans, Linie: l)];
        }

        var cuTert = DecontCules(titular, "N21", l => l.RepartitorDebitId = Furnizor);
        if (Trece("SC-C111-04", "DEC cu TVA și terț pe linie, pe cont fără partide", () => Opereaza(cuTert.Id)))
            Postari("SC-C111-04", cuTert.Id, Operare, Ianuarie, Asteptate(cuTert, Cont(Serviciu), Furnizor));

        var cuLoc = DecontCules(titular, "N21", l => l.RepartitorDebitId = Magazie);
        if (Trece("SC-C111-23", "DEC cu TVA și repartitor intern pe linie, altul decât primitorul", () => Opereaza(cuLoc.Id))) {
            var randuri = Asteptate(cuLoc, Cont(Serviciu), Magazie);
            Postari("SC-C111-23", cuLoc.Id, Operare, Ianuarie, randuri);
            var data = new DateOnly(An, 1, 20);
            Storneaza(cuLoc.Id, data);
            Postari("SC-C111-23", cuLoc.Id, Storno, data, Inverse(randuri));
        }

        var cuPartida = DecontCules(titular, "N21", l => { l.ContDebitId = Cont("401"); l.RepartitorDebitId = Furnizor; });
        var (acceptat, refuzuriVechi) = CuSpatiu(os => {
            var doc = os.GetObjectByKey<Document>(cuPartida.Id); doc.PregatesteOperare(os);
            var contract = Contractare.Contracteaza(os, doc);
            if (!contract.EsteAcceptat) return (false, contract.Refuzuri.Select(r => r.Cod).ToList());
            var tranzactie = contract.Tranzactii.Single();
            // Forma dinaintea lui S10: partenerul fiscal scris peste repartitorul liniei.
            var veche = tranzactie with { Postari = [.. tranzactie.Postari.Select(p =>
                p.Coordonate.Cont == Cont("401") && p.Coordonate.CodTva != null
                    ? p with { Coordonate = p.Coordonate with { Repartitor = new N.Repartitor(titular, N.FelRepartitor.Angajat) } }
                    : p)] };
            return (true, N.Conservare.Verifica(veche).Select(r => r.Cod).ToList());
        });
        Verifica("SC-C111-24", "martor: cu partenerul fiscal scris peste terțul liniei nucleul refuză cu UNITATE_NEPOTRIVITA — "
            + string.Join(", ", refuzuriVechi), acceptat && refuzuriVechi.Contains(N.Coduri.UnitateNepotrivita));
        if (Trece("SC-C111-24", "DEC cu TVA și terț pe linie, pe cont cu partide", () => Opereaza(cuPartida.Id))) {
            var partida = N.UnitateSold.DeschidePartida(Cont("401"), Furnizor, cuPartida.Id, Ianuarie).Id;
            Postari("SC-C111-24", cuPartida.Id, Operare, Ianuarie, Asteptate(cuPartida, Cont("401"), Furnizor, partida));
            SoldPartida("SC-C111-24", partida, Ianuarie, 100);
        }
    }

    // ───────────────────────── SC-C111-07, 08, 09, 10: citirile pe o singură axă ─────────────────────────

    void CazCitiri() {
        var nul = ContScena("-NUL"); var contra = ContScena("-B"); var banca = ContScena("-BANCA");
        var casa = CuSpatiu(os => {
            var c = os.CreateObject<ContPropriu>(); c.Cod = Marcaj + "-CASA"; c.Denumire = c.Cod; c.ContImplicitId = banca;
            os.CommitChanges(); return c.ID;
        });

        var nota = NotaPe(Ianuarie, nul, contra, 200, repartitorCredit: Furnizor); Opereaza(nota.Id);
        Postari("SC-C111-07", nota.Id, Operare, Ianuarie,
            new(nul, Debit, 200, Repartitor: Loc, Linie: nota.Linii[0].Id, Economic: Economic),
            new(contra, Credit, 200, Repartitor: Furnizor, Linie: nota.Linii[0].Id, Economic: Economic));
        Verifica("SC-C111-07", "terțul de pe contul fără partide nu deschide partidă", CuSpatiu(os =>
            !C.Citiri.Partide.Postari(os).Any(p => p.DocumentId == nota.Id)));
        Opereaza(NotaPe(Ianuarie, nul, contra, 30, repartitorCredit: Furnizor).Id);
        Opereaza(NotaPe(Ianuarie, nul, contra, 11, Furnizor, Furnizor).Id);

        (decimal? PeLoc, decimal? PeTert, decimal? Total, int FaraRepartitor) Fise(IObjectSpace os, DateOnly deLa, DateOnly panaLa,
                CitireCumul citire) {
            decimal? Sold(Guid? repartitor) => ContabilProiectii.FisaCont(os, nul, deLa, panaLa, repartitorId: repartitor, citire: citire)
                .ToList().Select(r => (decimal?)r.SoldCurent).Max();
            return (Sold(Loc), Sold(Furnizor), Sold(null),
                ContabilProiectii.FisaCont(os, nul, deLa, panaLa, repartitorNul: true, citire: citire).ToList().Count);
        }
        var ianuarie = CuSpatiu(os => Fise(os, Ianuarie, Sfarsit, CitireCumul.Vizibila));
        Verifica("SC-C111-08", $"fișa pe Loc 230, pe furnizor 11, fără filtru 241, nimic fără repartitor (obținut {ianuarie})",
            ianuarie == (230, 11, 241, 0));

        var plata = CuSpatiu(os => {
            var doc = os.CreateObject<Plata>(); doc.Data = Ianuarie; doc.PredatorId = casa; doc.PrimitorId = Furnizor;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, "TRZ"); l.Valoare = 40; l.CodEconomicId = Economic;
            os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, null, null)]);
        });
        Opereaza(plata.Id);
        Postari("SC-C111-10", plata.Id, Operare, Ianuarie,
            new(Cont(ContFurnizor), Debit, 40, Repartitor: Furnizor, Unitate: Partida(plata.Id, ContFurnizor),
                Linie: plata.Linii[0].Id, Economic: Economic),
            new(banca, Credit, 40, Repartitor: casa, Linie: plata.Linii[0].Id, Economic: Economic));

        // Regula cu ambele conturi explicite lasă piciorul propriu fără niciun repartitor.
        var tipExplicit = TipPe("-EXP", "TRZ", contra);
        var regula = CuSpatiu(os => {
            var r = os.CreateObject<RegulaContare>();
            r.TipDocumentId = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "PLT").ID;
            r.TipMaterialId = tipExplicit;
            r.SursaContDebit = SursaCont.Explicit; r.SursaContCredit = SursaCont.Explicit;
            r.ContDebitId = ContScena("-T"); r.ContCreditId = banca;
            os.CommitChanges(); return r.ID;
        });
        try {
            var explicita = CuSpatiu(os => {
                var doc = os.CreateObject<Plata>(); doc.Data = Ianuarie; doc.PredatorId = casa; doc.PrimitorId = Furnizor;
                var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc; l.Pozitie = 1;
                l.TipMaterialId = tipExplicit; l.Valoare = 7; l.CodEconomicId = Economic;
                os.CommitChanges(); return doc.ID;
            });
            Opereaza(explicita);
            Verifica("SC-C111-10", "plata cu ambele conturi explicite creditează contul propriu cu 7, fără niciun repartitor",
                Cub(explicita, Operare).Count(p => p.Cont == banca && p.Sens == Credit && p.Valoare == 7 && p.Repartitor == null) == 1);
        }
        finally { Comanda(os => { os.Delete(os.GetObjectByKey<RegulaContare>(regula)); os.CommitChanges(); }); }

        var (peNul, peContra, peBanca, balanta) = CuSpatiu(os => {
            var b = ContabilProiectii.Balanta(os, new(An, 1, 1), Sfarsit).ToList();
            decimal Sold(Guid cont) => b.Where(r => r.ContId == cont).Sum(r => r.SoldFinalDebit - r.SoldFinalCredit);
            return (ContabilProiectii.SoldParteneri(os, Sfarsit, nul).ToList(),
                ContabilProiectii.SoldParteneri(os, Sfarsit, contra).ToList(),
                ContabilProiectii.SoldParteneri(os, Sfarsit, banca).ToList(),
                (Nul: Sold(nul), Contra: Sold(contra), Banca: Sold(banca)));
        });
        static decimal Net(IEnumerable<SoldPartenerRand> randuri) => randuri.Sum(r => r.SoldDebitor - r.SoldCreditor);
        static string Descrie(IEnumerable<SoldPartenerRand> randuri) => string.Join("; ", randuri.Select(r =>
            $"{r.RepartitorDenumire ?? "fără terț"} {r.SoldDebitor - r.SoldCreditor}"));
        Verifica("SC-C111-10", $"contul cu Loc și furnizor: furnizorul 11 pe rând propriu, 230 în rândul fără terț, Loc fără rând ({Descrie(peNul)})",
            peNul.Count == 2 && peNul.Count(r => r.RepartitorId == Furnizor && r.SoldDebitor == 11) == 1
            && peNul.Count(r => r.RepartitorId == null && r.SoldDebitor == 230) == 1);
        Verifica("SC-C111-10", $"contul propriu al plății n-are rând: cei 40 de pe el și cei 7 fără repartitor stau în rândul fără terț ({Descrie(peBanca)})",
            peBanca.Count == 1 && peBanca[0].RepartitorId == null && peBanca[0].SoldCreditor == 47);
        var (fisaNul, fisaBanca, fisaContra, fisaFaraRepartitor) = CuSpatiu(os => {
            List<FisaContRand> Fisa(Guid cont, bool faraTert, bool repartitorNul = false) => ContabilProiectii
                .FisaCont(os, cont, new(An, 1, 1), Sfarsit, repartitorNul: repartitorNul, faraTert: faraTert).ToList();
            return (Fisa(nul, true), Fisa(banca, true), Fisa(contra, true), Fisa(banca, false, repartitorNul: true));
        });
        static decimal? Ultim(List<FisaContRand> fisa) => fisa.Count == 0 ? null : fisa[^1].SoldCurent;
        Verifica("SC-C111-31", "fișa fără terț arată ce adună rândul fără terț: 230 pe contul cu Loc și furnizor, "
            + $"47 pe contul plății, din care 7 fără repartitor, nimic pe contul numai cu terț (obținut {Ultim(fisaNul)}, "
            + $"{Ultim(fisaBanca)} în {fisaBanca.Count} rânduri, {Ultim(fisaFaraRepartitor)}, {fisaContra.Count} rânduri)",
            Ultim(fisaNul) == 230 && fisaNul.Count == 2 && Ultim(fisaBanca) == -47 && fisaBanca.Count == 2
            && Ultim(fisaFaraRepartitor) == -7 && fisaContra.Count == 0);
        var (soldBanca, soldFaraRepartitor, soldContra) = CuSpatiu(os => (
            ContabilProiectii.FisaSold(os, banca, new(An, 1, 1), Sfarsit, faraTert: true),
            ContabilProiectii.FisaSold(os, banca, new(An, 1, 1), Sfarsit, repartitorNul: true),
            ContabilProiectii.FisaSold(os, contra, new(An, 1, 1), Sfarsit, faraTert: true)));
        Verifica("SC-C111-32", "sinteza fișei fără terț pe contul plății: fără sold inițial, rulaj creditor 47, sold final creditor 47; "
            + $"fără repartitor 7; nimic pe contul numai cu terț (obținut {soldBanca.RulajCredit}/{soldBanca.SoldFinalCredit}, "
            + $"{soldFaraRepartitor.SoldFinalCredit}, {soldContra.SoldFinalDebit}/{soldContra.SoldFinalCredit})",
            soldBanca is { SoldInitialDebit: 0, SoldInitialCredit: 0, RulajDebit: 0, RulajCredit: 47, SoldFinalDebit: 0, SoldFinalCredit: 47 }
            && soldFaraRepartitor is { RulajCredit: 7, SoldFinalCredit: 7 }
            && soldContra is { RulajDebit: 0, RulajCredit: 0, SoldFinalDebit: 0, SoldFinalCredit: 0 });
        Verifica("SC-C111-10", $"contul numai cu terț n-are rând fără terț ({Descrie(peContra)})",
            peContra.Count == 1 && peContra[0].RepartitorId == Furnizor && peContra[0].SoldCreditor == 241);
        Verifica("SC-C111-10", $"suma rândurilor fiecărui cont e soldul lui din balanță ({Net(peNul)}/{balanta.Nul}, "
            + $"{Net(peContra)}/{balanta.Contra}, {Net(peBanca)}/{balanta.Banca})",
            Net(peNul) == 241 && Net(peNul) == balanta.Nul && Net(peContra) == balanta.Contra
            && Net(peBanca) == -47 && Net(peBanca) == balanta.Banca);

        InchideIanuarie();
        var snapshot = CuSpatiu(os => os.GetObjectsQuery<SoldPerioadaContabil>()
            .Where(s => s.An == An && s.Luna == 1 && s.ContId == nul)
            .Select(s => new { s.RepartitorId, s.Debit, s.Credit }).ToList()
            .GroupBy(s => s.RepartitorId).Select(g => (Repartitor: g.Key, Net: g.Sum(s => s.Debit - s.Credit))).ToList());
        Verifica("SC-C111-09", "snapshot-ul lunii închise ține aceeași axă: Loc 230, furnizor 11, nimic fără repartitor",
            snapshot.Count == 2 && snapshot.Contains((Loc, 230m)) && snapshot.Contains((Furnizor, 11m)));
        var (atomiBanca, randuriBanca) = CuSpatiu(os => (
            os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == An && s.Luna == 1 && s.ContId == banca)
                .Select(s => new { s.RepartitorId, s.Debit, s.Credit }).ToList()
                .GroupBy(s => s.RepartitorId).Select(g => (Repartitor: g.Key, Net: g.Sum(s => s.Debit - s.Credit))).ToList(),
            ContabilProiectii.SoldParteneri(os, Sfarsit, banca).ToList()));
        Verifica("SC-C111-10", "după închidere snapshot-ul ține separat contul propriu (40) și postarea fără repartitor (7), "
            + $"iar rândul fără terț rămâne 47 ({Descrie(randuriBanca)})",
            atomiBanca.Count == 2 && atomiBanca.Contains((casa, -40m)) && atomiBanca.Contains((null, -7m))
            && randuriBanca.Count == 1 && randuriBanca[0].RepartitorId == null && randuriBanca[0].SoldCreditor == 47);
        Opereaza(NotaPe(Februarie, nul, contra, 1, repartitorCredit: Furnizor).Id);
        Opereaza(NotaPe(Februarie, nul, contra, 2, Furnizor, Furnizor).Id);
        foreach (var citire in new[] { CitireCumul.Vizibila, CitireCumul.Integrala }) {
            var (fise, initiale) = CuSpatiu(os => (Fise(os, new(An, 2, 1), new(An, 2, 28), citire),
                ContabilProiectii.Balanta(os, new(An, 2, 1), new(An, 2, 28), analitic: true, citire: citire)
                    .Where(b => b.ContId == nul).ToList()
                    .Select(b => (b.RepartitorId, b.SoldInitialDebit)).OrderBy(b => b.SoldInitialDebit).ToList()));
            Verifica("SC-C111-09", $"citirea {citire} după închidere: fișa pe Loc 231, pe furnizor 13, fără filtru 244, "
                + $"nimic fără repartitor (obținut {fise}); soldurile inițiale 11 pe furnizor și 230 pe Loc",
                fise == (231, 13, 244, 0) && initiale.SequenceEqual([(Furnizor, 11m), (Loc, 230m)]));
            var sinteze = CuSpatiu(os => {
                (decimal Initial, decimal Final, int Randuri, bool Inchisa) Sinteza(DateOnly deLa, DateOnly panaLa,
                        Guid? repartitor = null, bool repartitorNul = false, bool faraTert = false) {
                    var s = ContabilProiectii.FisaSold(os, nul, deLa, panaLa, repartitorId: repartitor,
                        repartitorNul: repartitorNul, citire: citire, faraTert: faraTert);
                    var randuri = ContabilProiectii.FisaCont(os, nul, deLa, panaLa, repartitorId: repartitor,
                        repartitorNul: repartitorNul, citire: citire, faraTert: faraTert).ToList();
                    var initial = s.SoldInitialDebit - s.SoldInitialCredit;
                    var final = s.SoldFinalDebit - s.SoldFinalCredit;
                    return (initial, final, randuri.Count, s.RulajDebit == randuri.Sum(r => r.Debit)
                        && s.RulajCredit == randuri.Sum(r => r.Credit) && final == initial + s.RulajDebit - s.RulajCredit
                        && (randuri.Count == 0 || randuri[^1].SoldCurent == final));
                }
                DateOnly f1 = new(An, 2, 1), f2 = new(An, 2, 28), m1 = new(An, 3, 1), m2 = new(An, 3, 31);
                return (Tot: Sinteza(f1, f2), PeLoc: Sinteza(f1, f2, Loc), PeTert: Sinteza(f1, f2, Furnizor),
                    FaraTert: Sinteza(f1, f2, faraTert: true), FaraRepartitor: Sinteza(f1, f2, repartitorNul: true),
                    FaraMiscari: Sinteza(m1, m2, faraTert: true));
            });
            Verifica("SC-C111-32", $"citirea {citire} după închidere: sinteza fișei e rândul balanței și se închide cu rândurile ei: "
                + "241 → 244 pe cont, 230 → 231 pe Loc și fără terț, 11 → 13 pe furnizor, nimic fără repartitor; luna fără mișcări "
                + $"arată soldul 231 fără niciun rând (obținut {sinteze})",
                sinteze.Tot == (241, 244, 2, true) && sinteze.PeLoc == (230, 231, 1, true) && sinteze.PeTert == (11, 13, 1, true)
                && sinteze.FaraTert == (230, 231, 1, true) && sinteze.FaraRepartitor == (0, 0, 0, true)
                && sinteze.FaraMiscari == (231, 231, 0, true));
            var faraTert = CuSpatiu(os => ContabilProiectii
                .FisaCont(os, nul, new(An, 2, 1), new(An, 2, 28), citire: citire, faraTert: true).ToList());
            Verifica("SC-C111-31", $"citirea {citire} după închidere: fișa fără terț pornește de la 230 și ajunge la 231 "
                + $"(obținut {faraTert.Count} rânduri, {faraTert.LastOrDefault()?.SoldCurent})",
                faraTert.Count == 1 && faraTert[0].SoldCurent == 231);
        }
    }

    // ───────────────────────── SC-C111-06, 16: recepția și descărcarea ─────────────────────────

    void CazStoc() {
        var tva = Privat ? "N21" : null;
        var f = Factura(Ianuarie, new LinieFctScena(10, 10, tva));
        var nir = Opereaza(f.Id).ConexId!.Value;
        var l = f.Linii[0]; var datorie = Partida(f.Id, ContFurnizor);
        RandScena[] receptie = Privat
            ? [new(Cont(Stoc), Debit, 100, 10, Magazie, l.Lot, l.Produs, PartenerFiscal: Furnizor, Linie: l.Id,
                    Tva: Tva("N21"), Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Spatiu: N.Spatiu.Stoc,
                    Loc: Magazie),
                new(Cont(ContFurnizor), Credit, 100, -10, Furnizor, datorie, l.Produs, Linie: l.Id,
                    Contrapondere: N.Contrapondere.Tert),
                new(Cont("4426"), Debit, 21, Repartitor: Magazie, Produs: l.Produs, PartenerFiscal: Furnizor, Linie: l.Id,
                    Tva: Tva("N21"), Rol: N.RolTva.Taxa, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Magazie),
                new(Cont(ContFurnizor), Credit, 21, Repartitor: Furnizor, Unitate: datorie, Produs: l.Produs, Linie: l.Id)]
            : [new(Cont(Stoc), Debit, 100, 10, Magazie, l.Lot, l.Produs, Linie: l.Id, Spatiu: N.Spatiu.Stoc, Economic: Economic),
                new(Cont(ContFurnizor), Credit, 100, -10, Furnizor, datorie, l.Produs, Linie: l.Id, Economic: Economic,
                    Contrapondere: N.Contrapondere.Tert)];
        Postari("SC-C111-06", f.Id, Operare, Ianuarie, receptie);
        Opereaza(nir);
        var (peFurnizor, peGestiune) = CuSpatiu(os => (
            ContabilProiectii.Balanta(os, Ianuarie, Ianuarie, analitic: true, repartitorId: Furnizor).ToList(),
            ContabilProiectii.Balanta(os, Ianuarie, Ianuarie, analitic: true, repartitorId: Magazie, materialId: l.Produs).ToList()));
        Verifica("SC-C111-06", "balanța pe furnizor arată numai datoria: nici stocul, nici taxa",
            peFurnizor.Count == 1 && peFurnizor[0].ContId == Cont(ContFurnizor)
            && peFurnizor[0].RulajCredit == (Privat ? 121 : 100) && peFurnizor[0].RulajDebit == 0);
        Verifica("SC-C111-06", "balanța pe gestiune arată stocul" + (Privat ? " și taxa" : "") + ", fără datorie",
            peGestiune.Count == (Privat ? 2 : 1) && peGestiune.Single(b => b.ContId == Cont(Stoc)).RulajDebit == 100
            && (!Privat || peGestiune.Single(b => b.ContId == Cont("4426")).RulajDebit == 21));

        if (!Privat) return;
        var marfa = Receptioneaza(new LinieFctScena(4, 10, Tip: "371")).Linii[0];
        var dsc = Iesire(false, (marfa, 1));
        Comanda(os => { os.GetObjectByKey<Document>(dsc.Id).PrimitorId = Loc; os.CommitChanges(); });
        if (Trece("SC-C111-16", "DSC cu primitor intern", () => Opereaza(dsc.Id)))
            Postari("SC-C111-16", dsc.Id, Operare, Ianuarie,
                new(Cont("607"), Debit, 10, 1, Produs: marfa.Produs, Linie: dsc.Linii[0].Id, Contrapondere: N.Contrapondere.Consum),
                new(Cont("371"), Credit, 10, -1, Magazie, marfa.Lot, marfa.Produs, Linie: dsc.Linii[0].Id, Spatiu: N.Spatiu.Stoc));
    }

    // ───────────────────────── SC-C111-13: regularizarea de avans cu fapt fiscal ─────────────────────────

    void CazAvans() {
        var tipAvans = TipPe("-AV", "704", Cont("419"));
        var venit = CuSpatiu(os => Tip(os, "704"));
        var n21 = Tva("N21");
        foreach (var cuTva in new[] { true, false }) {
            var varianta = cuTva ? "cu TVA" : "fără TVA";
            var f = Vanzare((venit, 200, "N21"), (tipAvans, -100, cuTva ? "N21" : null));
            if (!Trece("SC-C111-13", $"FCL cu regularizare de avans {varianta}", () => Opereaza(f.Id))) continue;
            var creanta = Partida(f.Id, "4111", Client); var avans = Partida(f.Id, "419", Client);
            RandScena[] Linie(int i, Guid cont, Guid repartitor, Guid? unitate, decimal net, decimal taxa) {
                var l = f.Linii[i].Id;
                var debit = new RandScena(Cont("4111"), Debit, net, Repartitor: Client, Unitate: creanta, Linie: l);
                var baza = new RandScena(cont, Credit, net, Repartitor: repartitor, Unitate: unitate, PartenerFiscal: Client, Linie: l,
                    Tva: n21, Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Livrare, Loc: Magazie);
                return [debit, baza, debit with { Valoare = taxa },
                    new(Cont("4427"), Credit, taxa, Repartitor: Magazie, PartenerFiscal: Client, Linie: l,
                        Tva: n21, Rol: N.RolTva.Taxa, Perioada: Perioada, SensTva: N.SensTva.Livrare, Loc: Magazie)];
            }
            // C111-Q14: fără fapt fiscal, locul scos din repartitor de titularul partidei rămâne pe postare.
            RandScena[] regularizare = cuTva
                ? Linie(1, Cont("419"), Client, avans, -100, -21)
                : [new(Cont("4111"), Debit, -100, Repartitor: Client, Unitate: creanta, Linie: f.Linii[1].Id),
                    new(Cont("419"), Credit, -100, Repartitor: Client, Unitate: avans, Linie: f.Linii[1].Id, Loc: Magazie)];
            Postari("SC-C111-13", f.Id, Operare, Ianuarie, [.. Linie(0, Cont("704"), Magazie, null, 200, 42), .. regularizare]);
            SoldPartida("SC-C111-13", avans!.Value, Ianuarie, 100);
            var (peEmitent, peClient, balantaEmitent) = CuSpatiu(os => (
                ContabilProiectii.FisaCont(os, Cont("419"), Ianuarie, Ianuarie, repartitorId: Magazie).ToList()
                    .Where(r => r.DocumentId == f.Id).ToList(),
                ContabilProiectii.FisaCont(os, Cont("419"), Ianuarie, Ianuarie, repartitorId: Client).ToList()
                    .Where(r => r.DocumentId == f.Id).ToList(),
                ContabilProiectii.Balanta(os, Ianuarie, Ianuarie, analitic: true, repartitorId: Magazie).ToList()));
            Verifica("SC-C111-13", $"S2 ({varianta}): filtrul pe emitent nu găsește rândul de pe 419; filtrul pe client îl găsește, credit −100",
                peEmitent.Count == 0 && peClient.Count == 1 && peClient[0].Credit == -100
                && balantaEmitent.Where(b => b.ContId == Cont("419")).All(b => b.RulajCredit == 0 && b.RulajDebit == 0));
        }

        var achizitie = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false, Tip: "4091"));
        if (!Trece("C111-Q14", "FCT cu avans la furnizor fără TVA", () => Opereaza(achizitie.Id))) return;
        var linie = achizitie.Linii[0].Id;
        Postari("C111-Q14", achizitie.Id, Operare, Ianuarie,
            new(Cont("4091"), Debit, 100, Repartitor: Furnizor, Unitate: Partida(achizitie.Id, "4091"), Linie: linie, Loc: Magazie),
            new(Cont("401"), Credit, 100, Repartitor: Furnizor, Unitate: Partida(achizitie.Id, "401"), Linie: linie));
    }

    // ───────────────────────── SC-C111-25: politica de TVA pe sursă explicită ─────────────────────────

    void CazTvaExplicit() {
        var contExplicit = ContScena("-X");
        var n21 = Tva("N21");
        var lot = Receptioneaza(new LinieFctScena(10, 10, Tip: "371")).Linii[0];
        CuPoliticaTva("FCL", SursaCont.Explicit, contExplicit, () => {
            var f = Vanzare((CuSpatiu(os => Tip(os, "704")), 100, "N21"));
            if (!Trece("SC-C111-25", "FCL cu politica de TVA pe sursă Explicit", () => Opereaza(f.Id))) return;
            var l = f.Linii[0].Id;
            Postari("SC-C111-25", f.Id, Operare, Ianuarie,
                new(Cont("4111"), Debit, 100, Repartitor: Client, Unitate: Partida(f.Id, "4111", Client), Linie: l),
                new(Cont("704"), Credit, 100, Repartitor: Magazie, Linie: l, Tva: n21, Rol: N.RolTva.Baza,
                    Perioada: Perioada, SensTva: N.SensTva.Livrare, Loc: Magazie),
                new(contExplicit, Debit, 21, Linie: l),
                new(Cont("4427"), Credit, 21, Repartitor: Magazie, Linie: l, Tva: n21, Rol: N.RolTva.Taxa,
                    Perioada: Perioada, SensTva: N.SensTva.Livrare, Loc: Magazie));
        });
        CuPoliticaTva("RLF", SursaCont.Explicit, contExplicit, () => {
            var d = CuSpatiu(os => {
                var doc = os.CreateObject<ReturFurnizor>(); doc.Data = Ianuarie; doc.PredatorId = Magazie; doc.PrimitorId = Furnizor;
                var l = os.CreateObject<DocumentDetaliu>(); l.Document = doc; l.Pozitie = 1;
                l.TipMaterialId = Tip(os, "371"); l.Cantitate = 2; l.LotId = lot.Lot; l.TipTvaId = n21;
                os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, lot.Lot, lot.Produs)]);
            });
            if (!Trece("SC-C111-25", "RLF cu politica de TVA pe sursă Explicit", () => Opereaza(d.Id))) return;
            var linie = d.Linii[0].Id;
            Postari("SC-C111-25", d.Id, Operare, Ianuarie,
                new(Cont("371"), Debit, -20, -2, Magazie, lot.Lot, lot.Produs, Linie: linie, Tva: n21, Rol: N.RolTva.Baza,
                    Perioada: Perioada, SensTva: N.SensTva.Achizitie, Spatiu: N.Spatiu.Stoc, Loc: Magazie),
                new(Cont("401"), Credit, -20, 2, Furnizor, Partida(d.Id, "401"), lot.Produs, Linie: linie,
                    Contrapondere: N.Contrapondere.Tert),
                new(Cont("4426"), Debit, -4.2m, Repartitor: Magazie, Produs: lot.Produs, Linie: linie, Tva: n21,
                    Rol: N.RolTva.Taxa, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Magazie),
                new(contExplicit, Credit, -4.2m, Produs: lot.Produs, Linie: linie));
        });
        Verifica("SC-C111-25", "politicile de TVA sunt readuse la seed", CuSpatiu(os =>
            os.GetObjectsQuery<PoliticaTva>().Where(p => p.TipDocument.Cod == "FCL" || p.TipDocument.Cod == "RLF").ToList()
                .All(p => p.SursaContrapartida == SursaCont.RepartitorPrimitor && p.ContrapartidaFallbackId != contExplicit)));
    }

    // ───────────────────────── SC-C111-28: partenerul fiscal e al politicii ─────────────────────────

    void CazTvaPartener() {
        var tipAvans = TipPe("-AV", "704", Cont("419"));
        var ti21 = Tva("TI21");
        var documente = new List<(SursaCont Sursa, Guid? Partener, FacturaScena Doc, RandScena[] Randuri)>();
        foreach (var (sursa, partener) in new (SursaCont, Guid?)[] {
                (SursaCont.RepartitorPrimitor, Client), (SursaCont.Explicit, null), (SursaCont.RepartitorPredator, Magazie) })
            CuPoliticaTva("FCL", sursa, null, () => {
                var f = Vanzare((tipAvans, 100, "TI21"));
                if (!Trece("SC-C111-28", $"FCL cu baza pe 419, politica pe {sursa}", () => Opereaza(f.Id))) return;
                var l = f.Linii[0].Id;
                RandScena[] randuri = [
                    new(Cont("4111"), Debit, 100, Repartitor: Client, Unitate: Partida(f.Id, "4111", Client), Linie: l),
                    new(Cont("419"), Credit, 100, Repartitor: Client, Unitate: Partida(f.Id, "419", Client),
                        PartenerFiscal: partener, Linie: l, Tva: ti21, Rol: N.RolTva.Baza, Perioada: Perioada,
                        SensTva: N.SensTva.Livrare, Loc: Magazie)];
                Postari("SC-C111-28", f.Id, Operare, Ianuarie, randuri);
                documente.Add((sursa, partener, f, randuri));
            });
        if (documente.Count != 3) return;
        var d394 = CuSpatiu(os => D394Proiectii.D394(os, new(An, 1, 1), Sfarsit));
        Console.WriteLine($"     MĂSURAT (SC-C111-28): operațiuni [{string.Join("; ", d394.Operatiuni.Select(o => $"{o.Denumire} {o.Tip} {o.Baza}"))}]; "
            + $"neincluse [{string.Join("; ", d394.Neincluse.Select(n => $"{n.Cauza} {n.TipTvaCod} {n.RepartitorDenumire} {n.Baza}"))}].");
        Verifica("SC-C111-28", "D394: 100 pe client din politica RepartitorPrimitor, o singură operațiune",
            d394.Operatiuni.Count == 1 && d394.Operatiuni[0] is { Baza: 100, SensTva: nameof(N.SensTva.Livrare) }
            && d394.Operatiuni[0].Denumire == Marcaj + "-CLIENT");
        Verifica("SC-C111-28", "D394: 100 fără partener din politica Explicit; 100 pe emitent, care nu e partener, din RepartitorPredator",
            d394.Neincluse.Count == 2
            && d394.Neincluse.Count(n => n.Cauza == nameof(CauzaNeincludere.FaraPartener) && n.RepartitorId == null
                && n.Baza == 100 && n.TipTvaCod == "TI21") == 1
            && d394.Neincluse.Count(n => n.Cauza == nameof(CauzaNeincludere.RepartitorNePartener) && n.RepartitorId == Magazie
                && n.Baza == 100 && n.TipTvaCod == "TI21") == 1);
        var data = new DateOnly(An, 1, 20);
        foreach (var (sursa, _, f, randuri) in documente) {
            Storneaza(f.Id, data);
            Postari("SC-C111-28", f.Id, Storno, data, Inverse(randuri));
        }
    }

    // ───────────────────────── SC-C111-26: terțul comercial în SAF-T ─────────────────────────

    void CazSaft() {
        var comercial = ContScena("-R", c => { c.RolTert = RolTertCont.Client; c.UrmarestePartide = false; });
        var contra = ContScena("-B");
        var altTert = PartenerNou("-T2");
        var titular = CuSpatiu(os => {
            var a = os.CreateObject<Angajat>(); a.Cod = Marcaj + "-A"; a.Denumire = a.Cod; a.ContImplicitId = Cont(Avans);
            os.CommitChanges(); return a.ID;
        });
        var tipComercial = TipPe("-TR", Serviciu, comercial);

        var cuTert = NotaPe(Ianuarie, comercial, contra, 100, repartitorDebit: Client); Opereaza(cuTert.Id);
        var faraTert = NotaPe(Ianuarie, comercial, contra, 30); Opereaza(faraTert.Id);
        var factura = Factura(Ianuarie, new LinieFctScena(1, 50, "N21", Stoc: false, Tip: Marcaj + "-TR"));
        var decont = DecontCules(titular, "N21", l => { l.ContDebitId = comercial; l.RepartitorDebitId = altTert; });
        if (!Trece("SC-C111-26", "FCT cu baza pe contul cu rol comercial", () => Opereaza(factura.Id))
            || !Trece("SC-C111-26", "DEC cu terț pe linie pe contul cu rol comercial", () => Opereaza(decont.Id))) return;
        Randuri("SC-C111-26", "formele din cub pe contul cu rol comercial, toate fără partidă", CuSpatiu(os =>
            os.GetObjectsQuery<C.Postare>().Where(p => p.Cont == comercial).ToList()),
            new(comercial, Debit, 100, Repartitor: Client, Linie: cuTert.Linii[0].Id),
            new(comercial, Debit, 30, Repartitor: Loc, Linie: faraTert.Linii[0].Id),
            new(comercial, Debit, 50, Repartitor: Magazie, PartenerFiscal: Furnizor, Linie: factura.Linii[0].Id, Tva: Tva("N21"),
                Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Magazie),
            new(comercial, Debit, 100, Repartitor: altTert, PartenerFiscal: titular, Linie: decont.Linii[0].Id, Tva: Tva("N21"),
                Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Loc));

        var saft = CuSpatiu(os => SaftProiectii.SaftPeCub(os, An, 1));
        var simbol = SaftReguli.SimbolSaft(Marcaj + "-R");
        var linii = saft.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii.Select(l => (t.DocumentId, Linie: l)))
            .Where(x => x.Linie.AccountID == simbol).ToList();
        SaftLinieTranzactie Registru(Guid doc) => linii.Where(x => x.DocumentId == doc).Select(x => x.Linie).SingleOrDefault();
        SaftTert Tert(Guid partener) => saft.Clienti.SingleOrDefault(t => t.PartenerId == partener);
        static bool FaraTert(SaftLinieTranzactie l) => l != null && l.CustomerID == l.SupplierID;
        var avertisment = saft.Avertismente.SingleOrDefault(a => a.Cod == nameof(CodAvertismentSaft.TertFaraPartener));
        var neincluse = saft.Neincluse.Where(n => n.ContId == comercial).ToList();
        Console.WriteLine($"     MĂSURAT (SC-C111-26): refuzuri [{string.Join("; ", saft.Refuzuri.Select(r => r.Mesaj))}]; "
            + $"registru [{string.Join("; ", linii.Select(x => $"{x.Linie.Amount} C={x.Linie.CustomerID} S={x.Linie.SupplierID}"))}]; "
            + $"clienți [{string.Join("; ", saft.Clienti.Select(t => $"{t.Name} {t.ClosingDebitBalance}"))}]; "
            + $"neincluse [{string.Join("; ", neincluse.Select(n => $"{n.Cauza} {n.RepartitorDenumire} {n.Debit}"))}]; "
            + $"TertFaraPartener {avertisment?.Numar}/{avertisment?.Suma}.");
        Verifica("SC-C111-26", "(a) nota cu terț: terțul în registru și în soldul de nomenclator, 100",
            Tert(Client) is { ClosingDebitBalance: 100 } client && Registru(cuTert.Id)?.CustomerID == client.Id
            && !FaraTert(Registru(cuTert.Id)));
        Verifica("SC-C111-26", "(a') nota pe repartitor intern: fără terț în registru, soldul 30 în neincluse cu RepartitorNePartener",
            FaraTert(Registru(faraTert.Id))
            && neincluse.Count(n => n.Cauza == nameof(CauzaNeincludere.RepartitorNePartener) && n.RepartitorId == Loc
                && n.Debit == 30) == 1);
        Verifica("SC-C111-26", "(b) piciorul intern cu fapt fiscal: fără terț în registru, soldul 50 în neincluse cu RepartitorNePartener; "
            + "partenerul fiscal nu e terț comercial",
            FaraTert(Registru(factura.Id))
            && neincluse.Count(n => n.Cauza == nameof(CauzaNeincludere.RepartitorNePartener) && n.RepartitorId == Magazie
                && n.Debit == 50) == 1
            && Tert(Furnizor) == null);
        Verifica("SC-C111-26", "(c) decontul cu terț pe linie: terțul liniei în registru și în sold, 100; titularul nicăieri",
            Tert(altTert) is { ClosingDebitBalance: 100 } alt && Registru(decont.Id)?.CustomerID == alt.Id
            && !saft.Clienti.Concat(saft.Furnizori).Any(t => t.PartenerId == titular));
        Verifica("SC-C111-26", "avertismentul TertFaraPartener numără cele două postări fără terț (80), iar neinclusele contului sunt numai ele",
            avertisment is { Numar: 2, Suma: 80 } && neincluse.Count == 2);
    }

    // ───────────────────────── SC-C111-18, 29: imobilizările ─────────────────────────

    Guid Fisa() => CuSpatiu(os => {
        var f = os.CreateObject<Imobilizare>(); f.NumarInventar = Marcaj + "-F" + ++numar;
        f.Denumire = f.NumarInventar; f.TipMaterialId = Tip(os, Activ); f.LocId = Magazie; f.CodEconomicId = Economic;
        os.CommitChanges(); return f.ID;
    });

    FacturaScena Pif(Guid fisa, decimal valoare, Guid? sursa = null) => CuSpatiu(os => {
        var p = os.CreateObject<PunereInFunctiune>(); p.Data = Ianuarie; p.DataInregistrare = p.Data;
        p.PredatorId = Loc; p.PrimitorId = Magazie;
        var l = os.CreateObject<PunereInFunctiuneDetaliu>(); l.Document = p; l.Pozitie = 1;
        l.ImobilizareId = fisa; l.TipMaterialId = Tip(os, Activ); l.Cantitate = 1; l.Valoare = valoare;
        l.ValoareFiscala = valoare; l.LinieSursaId = sursa; l.Fel = FelLiniePif.Intrare;
        l.Metoda = MetodaAmortizare.Liniara; l.MetodaFiscala = MetodaAmortizare.Liniara;
        l.DurataLuni = 12; l.DurataFiscalaLuni = 12;
        l.CategorieFiscala = CategorieFiscala.Standard; l.UtilizareExclusiva = true;
        os.CommitChanges(); return new FacturaScena(p.ID, [new(l.ID, null, null)]);
    });

    void CazSuport() {
        var activ = Cont(Activ);
        var nota = Nota(Ianuarie, new LinieNtcScena(Activ, Capital, 1200, Destinatie, Loc)); Opereaza(nota.Id);
        var suport = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Single(p => p.DocumentId == nota.Id && p.Cont == activ).ID);
        var fisa = Fisa(); var pif = Pif(fisa, 1200);
        if (Trece("SC-C111-18", "PIF cu suportul postat pe alt loc decât laturile lui", () => Opereaza(pif.Id))) {
            var linie = pif.Linii[0].Id;
            var transfer = Cub(pif.Id, Transfer);
            Randuri("SC-C111-18", "PIF mută suportul pe fișă pe locul suportului", transfer,
                new(activ, Debit, -1200, Repartitor: Destinatie, Linie: linie, Economic: Economic),
                new(activ, Debit, 1200, Repartitor: Destinatie, Unitate: fisa, Linie: linie, Economic: Economic));
            Verifica("SC-C111-18", "ambele postări ale mutării numesc suportul și n-au bloc fiscal sau contrapondere",
                transfer.Count == 2 && transfer.All(p => p.SuportId == suport && p.PartenerFiscal == null && p.Loc == null
                    && p.Contrapondere == null));
            Randuri("SC-C111-18", "valoarea fiscală a PIF stă pe locul fișei", Cub(pif.Id, Operare),
                new(activ, Debit, 1200, Repartitor: Magazie, Unitate: fisa, Linie: linie, Carte: N.Carte.Fiscal),
                new(activ, Credit, 1200, Repartitor: Magazie, Linie: linie, Carte: N.Carte.Fiscal));
            var amo = CuSpatiu(os => {
                var d = AmortizareService.Genereaza(os, An, 2, Loc) ?? throw new InvalidOperationException("Scena cere o AMO eligibilă.");
                os.CommitChanges(); return d.ID;
            });
            Opereaza(amo);
            var amortizare = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == amo).ToList());
            Verifica("SC-C111-18", "AMO postează pe locul fișei, pe ambele cărți",
                amortizare.Count == 4 && amortizare.All(p => p.Repartitor == Magazie && p.Contrapondere == null
                    && p.PartenerFiscal == null && p.Loc == null));
        }
        Verifica("SC-C111-18", "repartitorul fără fel cunoscut e refuz explicit la recitire, nu nul", CuSpatiu(os => {
            try {
                C.Randuri.Citeste(os, [new C.Postare { ID = Guid.NewGuid(), DocumentId = nota.Id, Cont = activ,
                    Sens = Debit, Data = Ianuarie, Repartitor = Guid.NewGuid(), Carte = N.Carte.Contabil, Valoare = 1 }]);
                return false;
            }
            catch (N.RefuzException e) { return e.Refuz.Cod == CoduriRefuz.RepartitorFaraFel; }
        }));
    }

    void CazPozitie() {
        var activ = Cont(Activ);
        var altFurnizor = PartenerNou("-FURN2");
        var prima = Factura(Ianuarie, new LinieFctScena(1, 1200, "N21", Stoc: false, Tip: Activ));
        var aDoua = Factura(Ianuarie, new LinieFctScena(1, 800, "N21", Stoc: false, Tip: Activ));
        Comanda(os => { os.GetObjectByKey<Document>(aDoua.Id).PredatorId = altFurnizor; os.CommitChanges(); });
        Opereaza(prima.Id); Opereaza(aDoua.Id);
        RandScena Achizitie(FacturaScena f, Guid furnizor, decimal valoare) => new(activ, Debit, valoare, Repartitor: Magazie,
            PartenerFiscal: furnizor, Linie: f.Linii[0].Id, Tva: Tva("N21"), Rol: N.RolTva.Baza, Perioada: Perioada,
            SensTva: N.SensTva.Achizitie, Loc: Magazie);
        Randuri("SC-C111-29", "cele două achiziții stau pe același loc, fiecare cu furnizorul ei ca partener fiscal",
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => (p.DocumentId == prima.Id || p.DocumentId == aDoua.Id)
                && p.Cont == activ).ToList()),
            Achizitie(prima, Furnizor, 1200), Achizitie(aDoua, altFurnizor, 800));
        var iesire = Nota(new(An, 1, 10), new LinieNtcScena(Capital, Activ, 1500, Loc, Magazie));
        if (!Trece("SC-C111-29", "nota fără partener care creditează contul sub totalul celor două achiziții", () => Opereaza(iesire.Id)))
            return;
        var peParteneri = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Cont == activ && p.Unitate == null
                && p.Carte == N.Carte.Contabil && p.Repartitor == Magazie
                && (p.DocumentId == prima.Id || p.DocumentId == aDoua.Id || p.DocumentId == iesire.Id)).ToList())
            .GroupBy(p => p.PartenerFiscal).Select(g => g.Sum(p => p.Sens == Debit ? p.Valoare : -p.Valoare)).Order().ToList();
        Verifica("SC-C111-29", "poziția locului rămâne 500; pe cheia veche, cu partenerul, grupul notei ar fi fost −1500",
            peParteneri.Sum() == 500 && peParteneri.SequenceEqual([-1500m, 800m, 1200m]));
        var fisaNoua = Fisa(); var punere = Pif(fisaNoua, 500, aDoua.Linii[0].Id);
        Comanda(os => { var d = os.GetObjectByKey<Document>(punere.Id); d.Data = new(An, 1, 15); d.DataInregistrare = d.Data; os.CommitChanges(); });
        if (!Trece("SC-C111-29", "PIF pe a doua achiziție, din restul poziției", () => Opereaza(punere.Id))) return;
        var achizitie = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Single(p => p.DocumentId == aDoua.Id && p.Cont == activ).ID);
        var mutare = Cub(punere.Id, Transfer);
        Randuri("SC-C111-29", "PIF poartă locul, fără partener fiscal", mutare,
            new(activ, Debit, -500, Repartitor: Magazie, Linie: punere.Linii[0].Id),
            new(activ, Debit, 500, Repartitor: Magazie, Unitate: fisaNoua, Linie: punere.Linii[0].Id));
        Verifica("SC-C111-29", "furnizorul se regăsește prin suport: `SuportId` duce la postarea de achiziție, care îl poartă ca partener fiscal",
            mutare.Count == 2 && mutare.All(p => p.SuportId == achizitie)
            && CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Single(p => p.ID == achizitie).PartenerFiscal) == altFurnizor);
    }

    // ───────────────────────── SC-C111-19, 20: deschiderea și stornoul peste mai multe tranzacții ─────────────────────────

    void CazDeschidere() {
        var referinta = Guid.NewGuid();
        var analiza = new N.Analiza(null, Economic, null, null, null, null);
        var (lot, produs) = CuSpatiu(os => {
            var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-P" + ++numar; p.Denumire = p.Cod;
            p.UM = "BUC"; p.TipMaterialId = Tip(os, Stoc);
            var l = os.CreateObject<Lot>(); l.Produs = p; l.GestiuneId = Magazie; l.Data = Ianuarie; l.PretUnitar = 10;
            os.CommitChanges(); return (l.ID, p.ID);
        });
        var casa = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID);
        var ancora = CuSpatiu(os => os.GetObjectsQuery<Cont>().First(c => c.Simbol.StartsWith("891") && !c.Sumator).ID);
        var datorie = Cont(ContFurnizor);
        var initiala = N.UnitateSold.DeschidePartidaInitiala(datorie, Furnizor, referinta, Ianuarie).Id;
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Deschide(os, Ianuarie,
                [new(Cont(Stoc), Debit, 40, true), new(datorie, Debit, 50, true),
                    new(Cont(Numerar), Debit, 10) { Repartitor = casa, Analiza = analiza }, new(ancora, Credit, 100)],
                [new(Cont(Stoc), lot, Magazie, 4, 40)],
                [new(datorie, Furnizor, referinta, Debit, 50) { Analiza = analiza }]);
            os.CommitChanges(); tx.Commit();
        });
        Randuri("SC-C111-19", "deschiderea: lotul pe gestiune, partida pe partener, soldul fără unitate pe repartitorul dat, "
            + "fără fapt fiscal și fără contrapondere",
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                && p.Data == Ianuarie).ToList()),
            new(Cont(Stoc), Debit, 40, 4, Magazie, lot, produs, Spatiu: N.Spatiu.Stoc),
            new(datorie, Debit, 50, Repartitor: Furnizor, Unitate: initiala, Economic: Economic),
            new(Cont(Numerar), Debit, 10, Repartitor: casa, Economic: Economic),
            new(ancora, Credit, 100));

        if (!Privat) return;
        var f = Factura(new(An, 1, 10), new LinieFctScena(1, 100, "N21", false), new LinieFctScena(1, 100, "TI21", false));
        Opereaza(f.Id);
        var data = new DateOnly(An, 1, 10);
        var proprie = Partida(f.Id, ContFurnizor);
        RandScena[] Linie(int i, string cod, bool inversa) {
            var l = f.Linii[i].Id; var t = Tva(cod);
            var baza = new RandScena(Cont(Serviciu), Debit, 100, Repartitor: Magazie, PartenerFiscal: Furnizor, Linie: l, Tva: t,
                Rol: N.RolTva.Baza, Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Magazie);
            var credit = new RandScena(datorie, Credit, 100, Repartitor: Furnizor, Unitate: proprie, Linie: l);
            return [baza, credit, baza with { Cont = Cont("4426"), Valoare = 21, Rol = N.RolTva.Taxa },
                inversa
                    ? new(Cont("4427"), Credit, 21, PartenerFiscal: Furnizor, Linie: l, Tva: t, Rol: N.RolTva.Autocolectare,
                        Perioada: Perioada, SensTva: N.SensTva.Achizitie, Loc: Magazie)
                    : credit with { Valoare = 21 }];
        }
        RandScena[] operare = [.. Linie(0, "N21", false), .. Linie(1, "TI21", true)];
        Postari("SC-C111-20", f.Id, Operare, data, operare);
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(f.Id), initiala, 50, data);
            os.CommitChanges(); tx.Commit();
        });
        RandScena[] stingere = [
            new(datorie, Credit, 50, Repartitor: Furnizor, Unitate: initiala),
            new(datorie, Credit, -50, Repartitor: Furnizor, Unitate: proprie)];
        Postari("SC-C111-20", f.Id, Transfer, data, stingere);
        var stornare = new DateOnly(An, 1, 20);
        Storneaza(f.Id, stornare);
        Postari("SC-C111-20", f.Id, Storno, stornare, Inverse([.. operare, .. stingere]));
        var toate = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == f.Id)
            .Select(p => new { p.ID, p.TranzactieId, Fel = p.Tranzactie.Fel, p.InversaDinId, p.Pereche, p.TipTvaId, p.RolTva,
                p.PartenerFiscal, p.Loc, p.Valoare }).ToList());
        var originale = toate.Where(p => p.Fel != Storno).ToDictionary(p => p.ID);
        var inverse = toate.Where(p => p.Fel == Storno).ToList();
        var perechiSursa = originale.Values.GroupBy(p => p.TranzactieId)
            .Select(g => g.Select(p => p.Pereche).OfType<int>().ToHashSet()).ToList();
        Verifica("SC-C111-20", "stornoul reunește două tranzacții sursă ale căror perechi locale se suprapun; în storno perechile sunt distincte",
            perechiSursa.Count == 2 && perechiSursa[0].Overlaps(perechiSursa[1])
            && inverse.Count == originale.Count && inverse.Select(p => p.TranzactieId).Distinct().Count() == 1
            && inverse.GroupBy(p => p.Pereche).All(g => g.Key != null && g.Count() == 2)
            && inverse.Any(i => originale[i.InversaDinId.Value].Pereche != i.Pereche));
        Verifica("SC-C111-20", "fiecare inversă poartă partenerul fiscal și locul originalului ei, prin `InversaDin`: "
            + "cinci cu furnizorul și gestiunea, restul fără",
            inverse.All(i => i.InversaDinId is Guid id && originale.TryGetValue(id, out var o)
                && i.TipTvaId == o.TipTvaId && i.RolTva == o.RolTva && i.PartenerFiscal == o.PartenerFiscal
                && i.Loc == o.Loc && i.Valoare == -o.Valoare)
            && inverse.Count(i => i.PartenerFiscal == Furnizor && i.Loc == Magazie) == 5
            && inverse.Count(i => i.PartenerFiscal == null && i.Loc == null) == inverse.Count - 5);
    }

    // ───────────────────────── SC-C111-14: diferența imputabilă la NIR ─────────────────────────

    void CazNir() {
        string imputat = Privat ? "461" : "461.01.09", clarificare = Privat ? "473" : "473.01.09";
        Guid? functional = null, finantare = null;
        if (!Privat) Comanda(os => {
            var f = os.CreateObject<CodFunctional>(); f.Cod = Marcaj; f.Denumire = Marcaj; functional = f.ID;
            var b = os.CreateObject<SursaFinantare>(); b.Cod = Marcaj; b.Denumire = Marcaj; finantare = b.ID;
            os.CommitChanges();
        });
        (FacturaScena Factura, Guid Nir, Guid Linie) Constatat(CauzaDiferentei? cauza, Guid? tert) {
            var f = Factura(Ianuarie, new LinieFctScena(4, 25, Privat ? "SFD" : "CAP0"));
            var nir = Opereaza(f.Id).ConexId!.Value;
            var linie = CuSpatiu(os => {
                var l = os.GetObjectByKey<NIR>(nir).Detalii.OfType<NirDetaliu>().Single();
                l.CodFunctionalId = functional; l.SursaFinantareId = finantare;
                l.Cantitate = 3; l.CauzaDiferentei = cauza; l.PartenerDiferentaId = tert;
                os.CommitChanges(); return l.ID;
            });
            return (f, nir, linie);
        }

        var (factura, nir, linie) = Constatat(CauzaDiferentei.Imputabila, Client);
        if (Trece("SC-C111-14", "NIR cu lipsă imputabilă unui terț", () => Opereaza(nir))) {
            var lot = factura.Linii[0];
            var partida = N.UnitateSold.DeschidePartida(Cont(imputat), Client, nir, Ianuarie).Id;
            var postari = Cub(nir, Operare);
            Randuri("SC-C111-14", "diferența imputabilă: inventar ca marcaj, terțul imputat ca repartitor, pe partida lui", postari,
                new(Cont(Stoc), Credit, 25, -1, Magazie, lot.Lot, lot.Produs, Linie: linie, Spatiu: N.Spatiu.Stoc, Economic: Economic),
                new(Cont(imputat), Debit, 25, 1, Client, partida, lot.Produs, Linie: linie, Economic: Economic,
                    Contrapondere: N.Contrapondere.Inventar));
            SoldPartida("SC-C111-14", partida, Ianuarie, 25);
        }

        var cont = Cont(clarificare);
        var feluri = CuSpatiu(os => os.GetObjectByKey<Cont>(cont).FeluriRepartitor);
        Comanda(os => { os.GetObjectByKey<Cont>(cont).FeluriRepartitor = FeluriRepartitor.Partener; os.CommitChanges(); });
        try {
            var (_, faraTert, _) = Constatat(null, null);
            Refuza("SC-C111-14", () => Opereaza(faraTert), "Repartitor");
            FaraEfecte("SC-C111-14", faraTert);
        }
        finally { Comanda(os => { os.GetObjectByKey<Cont>(cont).FeluriRepartitor = feluri; os.CommitChanges(); }); }
    }

    // ───────────────────────── SC-C111-21: viramentul intern, tranzitul fără repartitor (S7) ─────────────────────────

    void CazVirament() {
        // Conturile proprii ale scenei, pe conturile celor din seed: purja le găsește după laturi.
        var (casa, contCasa, banca, contBanca, vir, tranzit) = CuSpatiu(os => {
            var tip = os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod == "VIR").Select(t => new { t.ID, t.ContImplicitId }).Single();
            var destinatie = Privat ? "BANCA" : "TREZ";
            var conturi = os.GetObjectsQuery<ContPropriu>().Where(c => c.Cod == "CASA" || c.Cod == destinatie)
                .Select(c => new { c.Cod, c.ContImplicitId }).ToList().ToDictionary(c => c.Cod, c => c.ContImplicitId.Value);
            ContPropriu Propriu(string sufix, Guid cont) {
                var c = os.CreateObject<ContPropriu>(); c.Cod = Marcaj + sufix; c.Denumire = c.Cod; c.ContImplicitId = cont;
                return c;
            }
            var sursa = Propriu("-VIR-A", conturi["CASA"]); var tinta = Propriu("-VIR-B", conturi[destinatie]);
            os.CommitChanges();
            return (sursa.ID, conturi["CASA"], tinta.ID, conturi[destinatie], tip.ID, tip.ContImplicitId.Value);
        });
        var plecare = CuSpatiu(os => {
            var doc = os.CreateObject<Plata>(); doc.Data = Ianuarie; doc.PredatorId = casa; doc.PrimitorId = banca;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc; l.Pozitie = 1;
            l.TipMaterialId = vir; l.Valoare = 50; l.CodEconomicId = Economic;
            os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, null, null)]);
        });
        if (!Trece("SC-C111-21", "plecarea viramentului", () => Opereaza(plecare.Id))) return;
        Postari("SC-C111-21", plecare.Id, Operare, Ianuarie,
            new(tranzit, Debit, 50, Linie: plecare.Linii[0].Id, Economic: Economic),
            new(contCasa, Credit, 50, Repartitor: casa, Linie: plecare.Linii[0].Id, Economic: Economic));
        var (sosire, linieSosire) = CuSpatiu(os => {
            var id = os.GetObjectsQuery<Document>().Single(d => d.DocumentSursaId == plecare.Id).ID;
            return (id, os.GetObjectsQuery<DocumentDetaliu>().Single(d => d.DocumentId == id).ID);
        });
        if (!Trece("SC-C111-21", "sosirea viramentului", () => Opereaza(sosire))) return;
        Postari("SC-C111-21", sosire, Operare, Ianuarie,
            new(contBanca, Debit, 50, Repartitor: banca, Linie: linieSosire, Economic: Economic),
            new(tranzit, Credit, 50, Linie: linieSosire, Economic: Economic));
        var peTranzit = CuSpatiu(os => ContabilProiectii.Balanta(os, new(An, 1, 1), Sfarsit, analitic: true)
            .Where(b => b.ContId == tranzit).ToList());
        Verifica("SC-C111-21", "tranzitul are un singur rând analitic, fără repartitor, închis: rulaj 50 pe debit și pe credit, sold zero",
            peTranzit.Count == 1 && peTranzit[0].RepartitorId == null && peTranzit[0].RulajDebit == 50 && peTranzit[0].RulajCredit == 50
            && peTranzit[0].SoldFinalDebit == 0 && peTranzit[0].SoldFinalCredit == 0);
    }

    // ───────────────────────── SC-C111-30: felurile de repartitor declarate de cont (S6) ─────────────────────────

    void CazFeluri() {
        var declarate = CuSpatiu(os => os.GetObjectsQuery<Cont>().Where(c => c.FeluriRepartitor != FeluriRepartitor.Nedeclarat)
            .Select(c => new { c.Simbol, c.FeluriRepartitor }).ToList().ToDictionary(c => c.Simbol, c => c.FeluriRepartitor));
        const FeluriRepartitor Optional = FeluriRepartitor.Partener | FeluriRepartitor.Niciunul;
        bool Declara(FeluriRepartitor feluri, params string[] simboluri) =>
            simboluri.All(s => declarate.GetValueOrDefault(s) == feluri);
        Verifica("SC-C111-30", $"seed-ul declară felurile numai pe bugetar, fiecare cont cu felurile lui: {declarate.Count} conturi",
            Privat ? declarate.Count == 0
                : declarate.Count == 20
                    && Declara(FeluriRepartitor.Partener, "401.01.00", "401.02.00", "404.01.00", "411.01.01", "411.02.08",
                        "442.08", "442.08.01", "458.01.00", "458.05.01", "458.05.02", "461.01.09", "774", "804.90.00", "805.00.00")
                    && Declara(FeluriRepartitor.Partener | FeluriRepartitor.Angajat, "462.01.01", "462.01.09")
                    && Declara(Optional, "437.01.00", "437.02.00", "803.00.01")
                    && Declara(FeluriRepartitor.ContPropriu, "552.00.00"));

        var terti = ContScena("-FT", c => c.FeluriRepartitor = FeluriRepartitor.Partener);
        var optional = ContScena("-FO", c => c.FeluriRepartitor = Optional);
        var mandat = ContScena("-FM", c => c.FeluriRepartitor = FeluriRepartitor.ContPropriu);
        var liber = ContScena("-FL");
        var casa = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID);

        void Deschide(Guid? peMandat) => Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Deschide(os, Ianuarie,
                [new(mandat, Debit, 10) { Repartitor = peMandat }, new(optional, Debit, 5), new(liber, Credit, 15)], [], []);
            os.CommitChanges(); tx.Commit();
        });
        int Deschideri() => CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Count(t => t.Fel == N.FelTranzactie.Deschidere && t.Data == Ianuarie));
        Refuza("SC-C111-30", () => Deschide(null), "cere: Repartitor.");
        Refuza("SC-C111-30", () => Deschide(Furnizor), "Repartitor de felul ContPropriu (primit: Partener)");
        Verifica("SC-C111-30", "deschiderile refuzate nu lasă nimic în cub", Deschideri() == 0);
        if (Trece("SC-C111-30", "deschiderea cu contul propriu pe contul de mandat și fără repartitor pe contul care admite lipsa",
                () => Deschide(casa)))
            Randuri("SC-C111-30", "deschiderea: contul propriu pe mandat, nimic pe contul care admite lipsa",
                CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                    && p.Data == Ianuarie).ToList()),
                new(mandat, Debit, 10, Repartitor: casa), new(optional, Debit, 5), new(liber, Credit, 15));

        var peTert = NotaPe(Ianuarie, terti, liber, 10, repartitorDebit: Furnizor);
        if (Trece("SC-C111-30", "nota cu terț pe contul care declară partener", () => Opereaza(peTert.Id)))
            Postari("SC-C111-30", peTert.Id, Operare, Ianuarie,
                new(terti, Debit, 10, Repartitor: Furnizor, Linie: peTert.Linii[0].Id, Economic: Economic),
                new(liber, Credit, 10, Repartitor: Loc, Linie: peTert.Linii[0].Id, Economic: Economic));
        foreach (var (cont, nume) in new[] { (terti, "partener"), (optional, "partener sau niciunul") }) {
            var peLoc = NotaPe(Ianuarie, cont, liber, 10);
            Refuza("SC-C111-30", () => Opereaza(peLoc.Id), "Repartitor de felul Partener (primit:");
            FaraEfecte("SC-C111-30", peLoc.Id);
        }
        var optionalCuTert = NotaPe(Ianuarie, optional, liber, 10, repartitorDebit: Furnizor);
        Trece("SC-C111-30", "nota cu terț pe contul care admite și lipsa", () => Opereaza(optionalCuTert.Id));
        var peLiber = NotaPe(Ianuarie, liber, liber, 10);
        Trece("SC-C111-30", "contul care nu declară feluri nu judecă repartitorul", () => Opereaza(peLiber.Id));
        TertPeContOptional(casa, Optional);
    }

    // ───────────────────────── SC-C111-33: terțul documentului pe contul care admite lipsa (D5) ─────────────────────────

    void TertPeContOptional(Guid casa, FeluriRepartitor optional) {
        var cont = ContScena("-FP", c => c.FeluriRepartitor = optional);
        var (numerar, tert) = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-TERT-FP"; p.Denumire = p.Cod; p.ContImplicitId = cont;
            os.CommitChanges();
            return (os.GetObjectsQuery<ContPropriu>().Where(c => c.ID == casa).Select(c => c.ContImplicitId).Single().Value, p.ID);
        });
        FacturaScena Document(bool inc, decimal valoare) => CuSpatiu(os => {
            DocumentTrezorerie doc = inc ? os.CreateObject<Incasare>() : os.CreateObject<Plata>();
            doc.Data = Ianuarie; doc.PredatorId = inc ? tert : casa; doc.PrimitorId = inc ? casa : tert;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, "TRZ"); l.Valoare = valoare; l.CodEconomicId = Economic;
            os.CommitChanges(); return new FacturaScena(doc.ID, [new(l.ID, null, null)]);
        });
        var plata = Document(false, 10); var incasare = Document(true, 4);
        if (!Trece("SC-C111-33", "plata către terțul cu cont care admite lipsa", () => Opereaza(plata.Id))
            || !Trece("SC-C111-33", "încasarea de la același terț", () => Opereaza(incasare.Id)))
            return;
        Postari("SC-C111-33", plata.Id, Operare, Ianuarie,
            new(cont, Debit, 10, Repartitor: tert, Linie: plata.Linii[0].Id, Economic: Economic),
            new(numerar, Credit, 10, Repartitor: casa, Linie: plata.Linii[0].Id, Economic: Economic));
        Postari("SC-C111-33", incasare.Id, Operare, Ianuarie,
            new(numerar, Debit, 4, Repartitor: casa, Linie: incasare.Linii[0].Id, Economic: Economic),
            new(cont, Credit, 4, Repartitor: tert, Linie: incasare.Linii[0].Id, Economic: Economic));
        var (peTert, faraTert, solduri) = CuSpatiu(os => (
            ContabilProiectii.FisaCont(os, cont, new(An, 1, 1), Sfarsit, repartitorId: tert).ToList(),
            ContabilProiectii.FisaCont(os, cont, new(An, 1, 1), Sfarsit, faraTert: true).ToList(),
            ContabilProiectii.SoldParteneri(os, Sfarsit, cont).ToList()));
        Verifica("SC-C111-33", "fișa pe terț are plata și încasarea, sold final debitor 6; fișa fără terț e goală; soldurile pe terți "
            + $"au un singur rând, al terțului (obținut {peTert.Count} rânduri, {(peTert.Count == 0 ? null : peTert[^1].SoldCurent)}, "
            + $"{faraTert.Count} fără terț, {solduri.Count} rânduri de sold)",
            peTert.Count == 2 && peTert[^1].SoldCurent == 6 && faraTert.Count == 0
            && solduri.Count == 1 && solduri[0].RepartitorId == tert && solduri[0].SoldDebitor == 6);
    }
}
