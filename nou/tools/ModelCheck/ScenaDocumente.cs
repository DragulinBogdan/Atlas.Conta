using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

record RandScena(Guid Cont, N.Latura Latura, decimal Valoare, decimal Cantitate = 0,
    Guid? Gestiune = null, Guid? Unitate = null, Guid? Produs = null, Guid? Partener = null,
    Guid? Linie = null, Guid? Tva = null, N.RolTva? Rol = null, int? Perioada = null,
    N.SensTva? Sens = null, N.Spatiu Spatiu = N.Spatiu.Contabil, Guid? Economic = null,
    N.Carte Carte = N.Carte.Contabil);

record LinieFctScena(decimal Cantitate, decimal Pret, string Tva = null, bool Stoc = true,
    decimal TaxaCuleasa = 0, string Tip = null);
record LinieScena(Guid Id, Guid? Lot, Guid? Produs);
record FacturaScena(Guid Id, LinieScena[] Linii);
record LinieNtcScena(string Debit, string Credit, decimal Valoare, Guid? RepartitorDebit = null, Guid? RepartitorCredit = null);

abstract class ScenaDocumente(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide, string cod, int an) {
    protected readonly bool Privat = privat;
    protected readonly int An = an;
    protected virtual int UltimulAn => An;
    protected string Marcaj => "E2E-SC-" + cod;
    protected Guid Magazie, Destinatie, Loc, Furnizor, Client;
    protected Guid? Economic;
    protected string Stoc => Privat ? "302" : "302.01.00";
    protected string Serviciu => Privat ? "628" : "628.00.00";
    protected string ContFurnizor => Privat ? "401" : "401.01.00";
    protected string ContClient => Privat ? "4111" : "411.01.01";
    protected DateOnly Ianuarie => new(An, 1, 5);
    protected DateOnly Februarie => new(An, 2, 5);
    int numar;
    Dictionary<string, Guid> conturi;
    readonly Dictionary<(Guid Doc, N.FelTranzactie Fel, DateOnly Data), RandScena[]> matriceFiscale = [];

    public void Ruleaza() {
        Curata();
        Exception initiala = null;
        try {
            Pregateste(); Executa(); VerificaMatriceFiscale(); VerificaRegim();
            Comanda(os => AcoperireInvarianti.Verifica(os, Verifica));
        }
        catch (Exception e) { initiala = e; throw; }
        finally {
            try { if (!Pastreaza || initiala != null) Curata(); }
            catch (Exception e) when (initiala != null) { throw new AggregateException(initiala, e); }
        }
    }

    protected abstract void Executa();
    protected virtual void CurataCubSuplimentar(IObjectSpace os, Purja purja) { }
    protected virtual void CurataNomenclatoare(IObjectSpace os, Purja purja) { }
    protected virtual bool Pastreaza => false;

    protected FacturaScena Nota(DateOnly data, params LinieNtcScena[] linii) {
        using var os = Deschide();
        var doc = os.CreateObject<NotaContabila>();
        doc.Data = data; doc.PredatorId = Loc; doc.PrimitorId = Loc;
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var d = os.CreateObject<NotaContabilaDetaliu>(); d.Document = doc;
            d.Pozitie = rezultat.Count + 1; d.TipMaterialId = Tip(os, "TRZ");
            d.ContDebitId = Cont(spec.Debit); d.ContCreditId = Cont(spec.Credit);
            d.Valoare = spec.Valoare; d.CodEconomicId = Economic;
            d.RepartitorDebitId = spec.RepartitorDebit; d.RepartitorCreditId = spec.RepartitorCredit;
            rezultat.Add(new(d.ID, null, null));
        }
        os.CommitChanges(); return new(doc.ID, rezultat.ToArray());
    }
    protected IObjectSpace Deschide() => deschide();
    protected T CuSpatiu<T>(Func<IObjectSpace, T> actiune) { using var os = Deschide(); return actiune(os); }
    protected void Comanda(Action<IObjectSpace> actiune) { using var os = Deschide(); actiune(os); }
    protected void Verifica(string id, string mesaj, bool rezultat) =>
        check($"{id} ({(Privat ? "privat" : "bugetar")}): {mesaj}", rezultat);
    protected Guid Cont(string simbol) => conturi[simbol];
    protected Guid Tip(IObjectSpace os, string codTip) => os.GetObjectsQuery<TipMaterial>().Single(t => t.Cod == codTip).ID;
    protected Guid Tva(string codTva) => CuSpatiu(os => os.GetObjectsQuery<TipTva>().Single(t => t.Cod == codTva).ID);
    protected Guid? Partida(Guid doc, string simbol, Guid? partener = null) =>
        N.Unitate.DeschidePartida(Cont(simbol), partener ?? Furnizor, doc, Ianuarie).Id;
    protected OperareRezultat Opereaza(Guid doc) => CuSpatiu(os => ComenziDocument.Sistem(os).Opereaza(doc));
    protected void Storneaza(Guid doc, DateOnly data) {
        var inainte = Amprenta(doc);
        CuRegim(doc, ComandaDocument.Storneaza, data, () => CuSpatiu(os => ComenziDocument.Sistem(os).Storneaza(doc, data)));
        PostariPastrate(doc, inainte);
    }
    protected void Anuleaza(Guid doc) {
        CuRegim(doc, ComandaDocument.AnuleazaOperarea, null, () => CuSpatiu(os => ComenziDocument.Sistem(os).AnuleazaOperarea(doc)));
        foreach (var cheie in matriceFiscale.Keys.Where(k => k.Doc == doc).ToArray()) matriceFiscale.Remove(cheie);
    }

    protected RegimDocument Regim(Guid doc) => CuSpatiu(os => RegimDocument.Calculeaza(os, doc));
    protected static bool Refuzata(RegimDocument regim, ComandaDocument comanda, string fragmentMotiv) =>
        Refuzata(regim, comanda.ToString(), fragmentMotiv);
    protected static bool Refuzata(RegimDocument regim, string comanda, string fragmentMotiv) =>
        !regim.Poate(comanda) && (regim.Motiv(comanda) ?? "").Contains(fragmentMotiv, StringComparison.OrdinalIgnoreCase);

    // SC-X-24 (106k): fiecare comandă de retragere a scenei e comparată cu regimul citit înaintea ei.
    readonly List<string> abateriRegim = [];
    readonly Dictionary<RefuzNepromis, int> nepromiseRegim = [];
    int comenziRegim;

    /// <summary>Câte refuzuri pe limita 106-r7 probează scena; orice alt număr e abatere.</summary>
    protected virtual int LimiteRegim => 0;

    protected RefuzNepromis ClasaRefuz(Guid doc, ComandaDocument comanda, DateOnly? data, string mesaj) =>
        CuSpatiu(os => ProbeRegim.Clasifica(os, doc, comanda, data, mesaj));

    T CuRegim<T>(Guid doc, ComandaDocument comanda, DateOnly? data, Func<T> executa) {
        var regim = CuSpatiu(os => os.GetObjectByKey<Document>(doc) is { } d ? RegimDocument.Calculeaza(os, d) : null);
        if (regim == null) return executa();
        var motiv = regim.Motiv(comanda.ToString());
        lock (abateriRegim) comenziRegim++;
        try {
            var rezultat = executa();
            if (motiv != null)
                lock (abateriRegim) abateriRegim.Add($"{comanda} a reușit, regimul o refuza: {motiv}");
            return rezultat;
        }
        catch (OperareException e) {
            if (motiv == null) {
                var clasa = ClasaRefuz(doc, comanda, data, e.Message);
                lock (abateriRegim) {
                    nepromiseRegim[clasa] = nepromiseRegim.GetValueOrDefault(clasa) + 1;
                    if (clasa == RefuzNepromis.Abatere)
                        abateriRegim.Add($"{comanda} oferită de regim, refuzată de comandă: {e.Message}");
                }
            }
            throw;
        }
    }

    void VerificaRegim() {
        if (comenziRegim == 0) return;
        foreach (var abatere in abateriRegim) Console.WriteLine($"     ABATERE REGIM ({cod}): {abatere}");
        int Nepromise(RefuzNepromis clasa) => nepromiseRegim.GetValueOrDefault(clasa);
        Verifica("SC-X-24", $"{cod}: {comenziRegim} comenzi de retragere conforme cu regimul citit înaintea lor; lăsate comenzii: "
            + $"{Nepromise(RefuzNepromis.PeValori)} pe valori, {Nepromise(RefuzNepromis.PeDataCeruta)} pe data cerută, "
            + $"{Nepromise(RefuzNepromis.Limita106r7)} pe limita 106-r7 (declarate {LimiteRegim})",
            abateriRegim.Count == 0 && Nepromise(RefuzNepromis.Limita106r7) == LimiteRegim);
    }
    protected void InchideIanuarie() => Comanda(os => inchide(os, An, 1));

    protected sealed record TranzactieScena(Guid Id, N.FelTranzactie Fel, bool Explicata, Guid? Din);

    protected List<TranzactieScena> Tranzactii(Guid doc) => CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>()
        .Where(t => t.DocumentId == doc).OrderBy(t => t.ScrisLa).ThenBy(t => t.Fel)
        .Select(t => new { t.ID, t.Fel, Explicata = t.Explicatie != null, t.ExplicatieDinId }).ToList()
        .Select(t => new TranzactieScena(t.ID, t.Fel, t.Explicata, t.ExplicatieDinId)).ToList());

    protected C.Citiri.ExplicatieTranzactie Explicatia(Guid doc, N.FelTranzactie fel) {
        var tranzactie = Tranzactii(doc).Last(t => t.Fel == fel).Id;
        return CuSpatiu(os => C.Citiri.Explicatii.PeTranzactie(os, tranzactie));
    }
    protected void Inchide(int an, int luna) => Comanda(os => inchide(os, an, luna));
    protected Guid Corecteaza(Guid doc) {
        var inainte = Amprenta(doc);
        var corectie = CuRegim(doc, ComandaDocument.Corecteaza, Februarie,
            () => CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(doc, Februarie, MotivCorectie.EroareMateriala).CorectieId));
        PostariPastrate(doc, inainte);
        return corectie;
    }

    void PostariPastrate(Guid doc, string inainte) {
        var originale = inainte.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var dupa = Amprenta(doc).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        Verifica($"SC-{cod}-IMUTABIL", "identitățile și toate coordonatele postărilor originale păstrate",
            originale.Length > 0 && originale.All(dupa.Contains));
    }

    void Pregateste() {
        using var os = Deschide();
        var inceput = new DateOnly(An, 1, 1);
        var sfarsit = new DateOnly(UltimulAn, 12, 31);
        if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An >= An && p.An <= UltimulAn)
            || os.GetObjectsQuery<Document>().Any(d => d.DataInregistrare >= inceput && d.DataInregistrare <= sfarsit))
            throw new InvalidOperationException($"{Marcaj} cere anul {An} liber.");
        conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        foreach (var luna in new[] { 1, 2 }) {
            var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = luna;
        }
        Magazie = os.GetObjectsQuery<Gestiune>().Single(g => g.Cod == "MAG1").ID;
        var destinatie = os.CreateObject<Gestiune>();
        destinatie.Cod = Marcaj + "-MAG"; destinatie.Denumire = destinatie.Cod;
        Destinatie = destinatie.ID;
        var loc = os.CreateObject<UnitateInterna>();
        loc.Cod = Marcaj + "-LOC"; loc.Denumire = loc.Cod; loc.Calitati = CalitateRepartitor.LocConsum;
        Loc = loc.ID;
        foreach (var esteClient in new[] { false, true }) {
            var p = os.CreateObject<Partener>();
            p.Cod = Marcaj + (esteClient ? "-CLIENT" : "-FURN"); p.Denumire = p.Cod;
            if (esteClient) Client = p.ID; else Furnizor = p.ID;
        }
        if (!Privat) {
            var c = os.CreateObject<CodEconomic>(); c.Cod = Marcaj; c.Denumire = Marcaj; Economic = c.ID;
        }
        os.CommitChanges();
    }

    protected FacturaScena Factura(DateOnly data, params LinieFctScena[] linii) {
        using var os = Deschide();
        var doc = os.CreateObject<FacturaIntrare>();
        doc.Numar = Marcaj + "-" + ++numar; doc.Data = data;
        doc.Predator = os.GetObjectsQuery<Partener>().Single(p => p.ID == Furnizor);
        doc.Primitor = os.GetObjectsQuery<Gestiune>().Single(p => p.ID == Magazie);
        var rezultat = new List<LinieScena>();
        foreach (var spec in linii) {
            var d = os.CreateObject<FacturaIntrareDetaliu>();
            d.Document = doc; d.Pozitie = rezultat.Count + 1;
            d.TipMaterialId = Tip(os, spec.Tip ?? (spec.Stoc ? Stoc : Serviciu));
            d.Cantitate = spec.Cantitate; d.PretUnitar = spec.Pret;
            d.CodEconomicId = Economic;
            if (spec.Tva != null) d.TipTva = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == spec.Tva);
            Guid? lotId = null, produsId = null;
            if (spec.Stoc) {
                var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-P" + ++numar;
                p.Denumire = p.Cod; p.UM = "BUC"; p.TipMaterialId = d.TipMaterialId;
                lotId = d.CreeazaLot(os, p, (Gestiune)doc.Primitor).ID; produsId = p.ID;
            }
            if (spec.TaxaCuleasa != 0m)
                Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.Mapata(os, doc, d, null, spec.TaxaCuleasa);
            rezultat.Add(new(d.ID, lotId, produsId));
        }
        os.CommitChanges();
        return new(doc.ID, rezultat.ToArray());
    }

    protected FacturaScena Receptioneaza(params LinieFctScena[] linii) {
        var f = Factura(Ianuarie, linii);
        var rezultat = Opereaza(f.Id);
        if (rezultat.ConexId is not Guid nir) throw new InvalidOperationException("Recepția cere NIR conex.");
        Opereaza(nir);
        return f;
    }

    protected Guid Consum(Guid lot, decimal cantitate, Guid? gestiune = null) {
        using var os = Deschide();
        var d = os.CreateObject<BonConsum>(); d.Data = new(An, 1, 10);
        d.PredatorId = gestiune ?? Magazie; d.PrimitorId = Loc;
        var l = os.CreateObject<DocumentDetaliu>(); l.Document = d;
        l.TipMaterialId = Tip(os, Stoc); l.LotId = lot; l.Cantitate = cantitate;
        os.CommitChanges(); return d.ID;
    }

    protected FacturaScena Trezorerie(bool incasare, params decimal[] sume) {
        using var os = Deschide();
        DocumentTrezorerie doc = incasare ? os.CreateObject<Incasare>() : os.CreateObject<Plata>();
        var casa = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "CASA").ID;
        doc.Data = Ianuarie; doc.PredatorId = incasare ? Client : casa;
        doc.PrimitorId = incasare ? casa : Furnizor;
        var linii = new List<LinieScena>();
        foreach (var suma in sume) {
            var l = os.CreateObject<DocumentTrezorerieDetaliu>(); l.Document = doc;
            l.Pozitie = linii.Count + 1; l.TipMaterialId = Tip(os, "TRZ");
            l.Valoare = suma; l.CodEconomicId = Economic;
            linii.Add(new(l.ID, null, null));
        }
        os.CommitChanges(); return new(doc.ID, linii.ToArray());
    }

    protected Guid Imperecheaza(Guid stingator, Guid stins, decimal suma, DateOnly data) =>
        CuSpatiu(os => ImperechereService.Imperecheaza(os, os.GetObjectByKey<Document>(stingator),
            os.GetObjectByKey<Document>(stins), suma, data: data).ID);

    protected FacturaScena Iesire(bool transfer, params (LinieScena Lot, decimal Cantitate)[] linii) {
        using var os = Deschide();
        Document doc = transfer ? os.CreateObject<NotaTransfer>() : os.CreateObject<DescarcareGestiune>();
        doc.Data = Ianuarie; doc.PredatorId = Magazie; doc.PrimitorId = transfer ? Destinatie : Client;
        var rezultat = new List<LinieScena>();
        foreach (var (lot, q) in linii) {
            DocumentDetaliu l = transfer ? os.CreateObject<DocumentDetaliu>() : os.CreateObject<DescarcareGestiuneDetaliu>();
            l.Document = doc; l.Pozitie = rezultat.Count + 1; l.Cantitate = q; l.LotId = lot.Lot;
            l.TipMaterialId = os.GetObjectsQuery<Lot>().Where(x => x.ID == lot.Lot).Select(x => x.Produs.TipMaterialId).Single()!.Value;
            rezultat.Add(new(l.ID, lot.Lot, lot.Produs));
        }
        os.CommitChanges(); return new(doc.ID, rezultat.ToArray());
    }

    protected void TransferPartida(string id, Guid stingator, Guid stins, string cont,
            Guid partener, N.Latura latura, decimal suma, DateOnly data) {
        Postari(id, stingator, N.FelTranzactie.Transfer, data,
            new(Cont(cont), latura, -suma, Unitate: Partida(stingator, cont, partener), Partener: partener),
            new(Cont(cont), latura, suma, Unitate: Partida(stins, cont, partener), Partener: partener));
    }

    protected void TransferPeCititori(string id, string intrare, int transferuri, int inverse, int peIntrare, params Guid[] documente) {
        using var os = Deschide();
        var transfer = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value)
            && p.Tranzactie.Fel == N.FelTranzactie.Transfer).Select(p => p.ID).ToList();
        var inv = os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Storno
            && p.InversaDinId != null && transfer.Contains(p.InversaDinId.Value)).Select(p => p.ID).ToList();
        var numarate = ProbeTransferCititori.Numara(os, [.. transfer, .. inv]);
        Console.WriteLine($"     MĂSURAT ({id}): {transfer.Count} postări Transfer, {inv.Count} inverse; "
            + string.Join(", ", numarate.Select(n => $"{n.Nume} {n.Numar}")) + ".");
        Verifica(id, $"N-r8: {transferuri} postări Transfer și {inverse} inverse; {peIntrare} pe `{intrare}`, "
            + "zero pe intrările care exclud și pe cealaltă intrare care include",
            transfer.Count == transferuri && inv.Count == inverse
            && numarate.Any(n => n.Nume == intrare && n.Regim == ProbeTransferCititori.Regim.Include)
            && numarate.Where(n => n.Regim != ProbeTransferCititori.Regim.Indiferent)
                .All(n => n.Numar == (n.Nume == intrare ? peIntrare : 0)));
    }

    // T-r14: contraponderile Transformare nu ajung prin nicio intrare comună.
    protected void ContraponderiPeCititori(string id, int asteptate, params Guid[] documente) {
        using var os = Deschide();
        var contraponderi = os.GetObjectsQuery<C.Postare>().Where(C.Citiri.Transformare.Contrapondere)
            .Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value)).Select(p => p.ID).ToList();
        var numarate = ProbeTransferCititori.Numara(os, contraponderi);
        Console.WriteLine($"     MĂSURAT ({id}): {contraponderi.Count} contraponderi; "
            + string.Join(", ", numarate.Select(n => $"{n.Nume} {n.Numar}")) + ".");
        Verifica(id, $"T-r14: {asteptate} contraponderi Transformare în cub, zero pe fiecare intrare comună",
            contraponderi.Count == asteptate && numarate.All(n => n.Numar == 0));
    }

    // N-r8: jurnalul și fișa de cont nu listează `Transfer` și inversele lui; intrarea pe unitate le listează cu felul lor.
    protected void ListareTransfer(string id, string intrare, params Guid[] documente) {
        using var os = Deschide();
        var transfer = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId != null && documente.Contains(p.DocumentId.Value)
            && p.Tranzactie.Fel == N.FelTranzactie.Transfer).Select(p => new { p.ID, p.Cont }).ToList();
        var idTransfer = transfer.Select(p => p.ID).ToList();
        var inverse = os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Storno
            && p.InversaDinId != null && idTransfer.Contains(p.InversaDinId.Value)).Select(p => new { p.ID, p.Cont }).ToList();
        var toate = transfer.Concat(inverse).Select(p => p.ID).ToList();
        var jurnal = ContabilProiectii.RegistruJurnal(os).Count(r => toate.Contains(r.Id));
        var fisa = transfer.Concat(inverse).Select(p => p.Cont).Distinct().ToList()
            .Sum(cont => ContabilProiectii.FisaCont(os, cont, DateOnly.MinValue, DateOnly.MaxValue).Count(r => toate.Contains(r.Id)));
        var peUnitate = ProbeTransferCititori.Intrari.Single(i => i.Nume == intrare).Postari(os).Where(i => toate.Contains(i)).ToList();
        var feluri = os.GetObjectsQuery<C.Postare>().Where(p => peUnitate.Contains(p.ID))
            .GroupBy(p => p.Tranzactie.Fel).Select(g => new { g.Key, Numar = g.Count() }).ToDictionary(g => g.Key, g => g.Numar);
        var listate = transfer.Concat(inverse).Count(p => peUnitate.Contains(p.ID));
        Console.WriteLine($"     MĂSURAT ({id}): {transfer.Count} postări Transfer, {inverse.Count} inverse; jurnal {jurnal}, fișă de cont {fisa}; "
            + $"`{intrare}` {listate} [{string.Join(", ", feluri.OrderBy(f => f.Key).Select(f => $"{f.Key} {f.Value}"))}].");
        Verifica(id, $"N-r8: jurnalul și fișa de cont nu listează `Transfer` și inversele lui; `{intrare}` le listează cu felul fiecăruia",
            transfer.Count > 0 && jurnal == 0 && fisa == 0 && listate > 0
            && feluri.GetValueOrDefault(N.FelTranzactie.Transfer) == transfer.Count(p => peUnitate.Contains(p.ID))
            && feluri.GetValueOrDefault(N.FelTranzactie.Storno) == inverse.Count(p => peUnitate.Contains(p.ID))
            && feluri.Keys.All(f => f is N.FelTranzactie.Transfer or N.FelTranzactie.Storno));
    }

    protected void Postari(string id, Guid doc, N.FelTranzactie fel, DateOnly data, params RandScena[] asteptate) {
        using var os = Deschide();
        var randuri = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Tranzactie.Fel == fel
            && (fel != N.FelTranzactie.Transfer || p.Data == data)).ToList();
        var tranzactii = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId == doc && t.Fel == fel
            && (fel != N.FelTranzactie.Transfer || t.Data == data)).ToList();
        var actual = randuri.Select(p => new RandScena(p.Cont, p.Latura, p.Valoare, p.Cantitate,
            p.Gestiune, p.Unitate, p.Produs, p.Partener, p.LinieId, p.TipTvaId, p.RolTva,
            p.PerioadaDeclarare, p.SensTva, p.Spatiu, p.CodEconomic, p.Carte)).ToList();
        var egale = actual.Count == asteptate.Length && asteptate.All(a =>
            actual.Count(r => r == a) == asteptate.Count(r => r == a));
        var ok = egale && tranzactii.Count == 1 && tranzactii[0].Data == data
            && randuri.All(p => p.Data == data && p.Valuta == null && p.Atribuit == null
                && p.ValoareValuta == 0 && p.CodFunctional == null && p.SursaFinantare == null
                && p.UnitateOrganizatorica == null && p.Proiect == null && p.CentruCost == null);
        Verifica(id, $"{fel}: {asteptate.Length} postări cu coordonatele și măsurile așteptate", ok);
        UnicitateFiscala.Verifica(os, check, Privat, doc, fel, actual, asteptate);
        matriceFiscale[(doc, fel, data)] = asteptate;
        if (!ok) {
            foreach (var r in actual.Except(asteptate)) Console.WriteLine("     ÎN PLUS " + r);
            foreach (var r in asteptate.Except(actual)) Console.WriteLine("     LIPSĂ " + r);
        }
    }

    void VerificaMatriceFiscale() {
        using var os = Deschide();
        var ids = matriceFiscale.Keys.Select(k => k.Doc).Distinct().ToList();
        var postari = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId != null && ids.Contains(p.DocumentId.Value))
            .Select(p => new { p.DocumentId, p.Tranzactie.Fel, p.Data, p.Cont, p.Latura, p.Valoare,
                p.LinieId, p.TipTvaId, p.RolTva, p.SensTva, p.Carte }).ToList();
        var corecte = matriceFiscale.All(m => UnicitateFiscala.Corecte(postari
            .Where(p => p.DocumentId == m.Key.Doc && p.Fel == m.Key.Fel && p.Data == m.Key.Data)
            .Select(p => new RandScena(p.Cont, p.Latura, p.Valoare, Linie: p.LinieId, Tva: p.TipTvaId,
                Rol: p.RolTva, Sens: p.SensTva, Carte: p.Carte)).ToList(), m.Value));
        if (matriceFiscale.Count > 0)
            Verifica("SC-X-14", $"{cod}: {matriceFiscale.Count} matrice reverificate înainte de curățenie", corecte);
    }

    protected string Amprenta(Guid doc) => CuSpatiu(os => string.Join("\n", os.GetObjectsQuery<C.Postare>()
        .Where(p => p.DocumentId == doc).OrderBy(p => p.ID).AsEnumerable().Select(p =>
            System.Text.Json.JsonSerializer.Serialize(new { p.ID, p.TranzactieId, p.LinieId, p.Data,
                p.Cont, p.Latura, p.Gestiune, p.Unitate, p.Produs, p.Partener, p.TipTvaId,
                p.SensTva, p.RolTva, p.PerioadaDeclarare, p.Cantitate, p.Valoare, p.ValoareValuta,
                p.Spatiu, p.Carte, p.Valuta, p.UnitateDeschisa, p.Atribuit, p.CodEconomic,
                p.FelUnitate, p.SuportId, p.SuportSpatiu, p.InversaDinId, p.InversaDinSpatiu,
                p.CodFunctional, p.SursaFinantare, p.UnitateOrganizatorica, p.Proiect, p.CentruCost }))));

    protected void SoldLot(string id, Guid lot, Guid gestiune, DateOnly data, decimal q, decimal v) {
        var sold = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == lot
            && p.Gestiune == gestiune && p.Data <= data && p.Carte == N.Carte.Contabil).ToList());
        Verifica(id, $"lot/gestiune la {data}: {q}/{v}", sold.Sum(p => p.Cantitate) == q
            && sold.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) == v);
    }

    protected decimal[] ValoriLinii(Guid doc) => CuSpatiu(os => os.GetObjectsQuery<DocumentDetaliu>()
        .Where(l => l.DocumentId == doc).OrderBy(l => l.Pozitie).Select(l => l.Valoare).ToArray());

    protected void ValoriLinii(string id, string mesaj, Guid doc, params decimal[] asteptate) {
        var valori = ValoriLinii(doc);
        Verifica(id, $"{mesaj}: valorile liniilor {string.Join("; ", asteptate)} (obținut {string.Join("; ", valori)})",
            valori.SequenceEqual(asteptate));
    }

    protected void SoldPartida(string id, Guid unitate, DateOnly data, decimal net) {
        var sold = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == unitate
            && p.Data <= data && p.Carte == N.Carte.Contabil).ToList());
        Verifica(id, $"sold partidă {net} la {data}",
            sold.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) == net);
    }

    protected void FaraEfecte(string id, Guid doc) {
        using var os = Deschide();
        Verifica(id, "Draft și zero efecte persistate proprii",
            os.GetObjectsQuery<Document>().Single(d => d.ID == doc).Stare == StareDocument.Draft
            && !os.GetObjectsQuery<C.Tranzactie>().Any(p => p.DocumentId == doc)
            && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == doc));
    }

    protected void Refuza(string id, Action actiune, string fragment) {
        try { actiune(); Verifica(id, "trebuia refuzat", false); }
        catch (OperareException e) {
            Verifica(id, $"refuz de domeniu: {e.Message}", e.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected void RefuzDeclaratie(string id, Guid doc, string codRefuz) => Verifica(id,
        "cod declarație " + codRefuz, CuSpatiu(os => {
            var d = os.GetObjectByKey<Document>(doc);
            var tip = os.GetObjectsQuery<TipDocument>().Single(t => t.ClrType == d.ClrType);
            return C.Materializare.Refuzuri(os, d, tip)
                .Any(m => m.StartsWith(codRefuz + ":", StringComparison.Ordinal));
        }));

    void Curata() {
        using var os = Deschide();
        var reps = os.GetObjectsQuery<Repartitor>()
            .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
        if (reps.Count == 0) return;
        var liniiNascatoare = os.GetObjectsQuery<Lot>()
            .Where(l => l.Produs.Cod.StartsWith(Marcaj) && l.LinieIntrareId != null)
            .Select(l => l.LinieIntrareId.Value).ToList();
        var documenteNascatoare = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => liniiNascatoare.Contains(l.ID)).Select(l => l.DocumentId).ToList();
        var docs = os.GetObjectsQuery<Document>()
            .Where(d => reps.Contains(d.PredatorId) || reps.Contains(d.PrimitorId)
                || documenteNascatoare.Contains(d.ID)).Select(d => d.ID).ToList();
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
        var pj = new Purja(os);
        CurataCubSuplimentar(os, pj);
        ProbeCub.Purjeaza(pj, os, docs);
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An >= An && s.An <= UltimulAn));
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An >= An && s.An <= UltimulAn));
        pj.Adauga(os.GetObjectsQuery<PartidaDeschisa>().Where(s => s.An >= An && s.An <= UltimulAn));
        pj.Adauga(os.GetObjectsQuery<Imperechere>().Where(i => docs.Contains(i.DocumentId) || docs.Contains(i.DocumentStingatorId)));
        pj.Adauga(os.GetObjectsQuery<DviFactura>().Where(i => docs.Contains(i.DviId) || docs.Contains(i.FacturaId)));
        pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docs.Contains(d.DocumentId)));
        pj.Adauga(os.GetObjectsQuery<Document>().Where(d => docs.Contains(d.ID)));
        CurataNomenclatoare(os, pj);
        pj.Adauga(os.GetObjectsQuery<Imobilizare>().Where(f => f.NumarInventar.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)));
        pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => reps.Contains(r.ID)));
        pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<CodFunctional>().Where(c => c.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<SursaFinantare>().Where(c => c.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Proiect>().Where(c => c.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<InchiderePerioada>().Where(i => i.Perioada.An >= An && i.Perioada.An <= UltimulAn));
        pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.An >= An && p.An <= UltimulAn));
        pj.Executa();
    }
}
