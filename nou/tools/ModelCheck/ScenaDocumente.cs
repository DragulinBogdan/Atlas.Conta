using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
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
        try { Pregateste(); Executa(); VerificaMatriceFiscale(); }
        catch (Exception e) { initiala = e; throw; }
        finally {
            try { Curata(); }
            catch (Exception e) when (initiala != null) { throw new AggregateException(initiala, e); }
        }
    }

    protected abstract void Executa();

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
    protected Guid? Partida(Guid doc, string simbol, Guid? partener = null) => Privat
        ? N.Unitate.DeschidePartida(Cont(simbol), partener ?? Furnizor, doc, Ianuarie).Id : null;
    protected OperareRezultat Opereaza(Guid doc) => CuSpatiu(os => OperareApi.Opereaza(os, doc));
    protected void Storneaza(Guid doc, DateOnly data) {
        var inainte = Amprenta(doc);
        Comanda(os => OperareApi.Storneaza(os, doc, data));
        PostariPastrate(doc, inainte);
    }
    protected void Anuleaza(Guid doc) {
        Comanda(os => OperareApi.AnuleazaOperarea(os, doc));
        foreach (var cheie in matriceFiscale.Keys.Where(k => k.Doc == doc).ToArray()) matriceFiscale.Remove(cheie);
    }
    protected void InchideIanuarie() => Comanda(os => inchide(os, An, 1));
    protected Guid Corecteaza(Guid doc) {
        var inainte = Amprenta(doc);
        var corectie = CuSpatiu(os => OperareApi.Corecteaza(os, doc, Februarie, MotivCorectie.EroareMateriala).CorectieId);
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
        if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An)
            || os.GetObjectsQuery<Document>().Any(d => d.DataInregistrare.Year == An))
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
            d.Cantitate = spec.Cantitate; d.PretUnitar = spec.Pret; d.ValoareTva = spec.TaxaCuleasa;
            d.CodEconomicId = Economic;
            if (spec.Tva != null) d.TipTva = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == spec.Tva);
            Guid? lotId = null, produsId = null;
            if (spec.Stoc) {
                var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-P" + ++numar;
                p.Denumire = p.Cod; p.UM = "BUC"; p.TipMaterialId = d.TipMaterialId;
                lotId = d.CreeazaLot(os, p, (Gestiune)doc.Primitor).ID; produsId = p.ID;
            }
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
        if (!Privat) {
            Verifica(id, "fără transfer de partidă pe conturi fără RolTert", CuSpatiu(os =>
                !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == stingator && p.Tranzactie.Fel == N.FelTranzactie.Transfer)));
            return;
        }
        Postari(id, stingator, N.FelTranzactie.Transfer, data,
            new(Cont(cont), latura, -suma, Unitate: Partida(stingator, cont, partener), Partener: partener),
            new(Cont(cont), latura, suma, Unitate: Partida(stins, cont, partener), Partener: partener));
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
                p.CodFunctional, p.SursaFinantare, p.UnitateOrganizatorica, p.Proiect, p.CentruCost }))));

    protected void SoldLot(string id, Guid lot, Guid gestiune, DateOnly data, decimal q, decimal v) {
        var sold = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == lot
            && p.Gestiune == gestiune && p.Data <= data && p.Carte == N.Carte.Contabil).ToList());
        Verifica(id, $"lot/gestiune la {data}: {q}/{v}", sold.Sum(p => p.Cantitate) == q
            && sold.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) == v);
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
            && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == doc)
            && !os.GetObjectsQuery<RegistruContabil>().Any(p => p.DocumentId == doc)
            && !os.GetObjectsQuery<RegistruStoc>().Any(p => p.DocumentId == doc)
            && !os.GetObjectsQuery<RegistruTva>().Any(p => p.DocumentId == doc));
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
        var reps = os.GetObjectsQuery<Repartitor>().IgnoreQueryFilters()
            .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
        if (reps.Count == 0) return;
        var docs = os.GetObjectsQuery<Document>().IgnoreQueryFilters()
            .Where(d => reps.Contains(d.PredatorId) || reps.Contains(d.PrimitorId)).Select(d => d.ID).ToList();
        var loturi = os.GetObjectsQuery<Lot>().IgnoreQueryFilters()
            .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
        var pj = new Purja(os);
        ProbeCub.Purjeaza(pj, os, docs);
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<PartidaDeschisa>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<Imperechere>().Where(i => docs.Contains(i.DocumentId) || docs.Contains(i.DocumentStingatorId)));
        pj.Adauga(os.GetObjectsQuery<DviFactura>().Where(i => docs.Contains(i.DviId) || docs.Contains(i.FacturaId)));
        pj.Adauga(os.GetObjectsQuery<RegistruTva>().Where(r => docs.Contains(r.DocumentId)));
        pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => loturi.Contains(r.LotId)));
        pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docs.Contains(r.DocumentId.Value)));
        pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docs.Contains(d.DocumentId)));
        pj.Adauga(os.GetObjectsQuery<Document>().Where(d => docs.Contains(d.ID)));
        pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => loturi.Contains(l.ID)));
        pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => reps.Contains(r.ID)));
        pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == Marcaj));
        pj.Adauga(os.GetObjectsQuery<InchiderePerioada>().Where(i => i.Perioada.An == An));
        pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.An == An));
        pj.Executa();
    }
}
