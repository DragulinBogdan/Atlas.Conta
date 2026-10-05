using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiBcs(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide) {
    const string Marcaj = "E2E-SC-BCS";
    const int An = 2001;
    static readonly DateOnly Intrare = new(An, 1, 5), Consum = new(An, 1, 10);
    static readonly DateOnly Sfarsit = new(An, 1, 31), Februarie = new(An, 2, 5);
    Guid magazie, loc, furnizor, tip, debit, credit;
    Guid? codEconomic;
    int numar;

    public void Ruleaza() {
        Curata();
        Pregateste();
        try {
            OperareSiStorno();
            MaiMulteLinii();
            ValoriLinii();
            Anulare();
            UltimaIesire();
            Refuzuri();
            PerioadaInchisa();
            using var os = deschide();
            var documente = os.GetObjectsQuery<BonConsum>().Where(d => d.PredatorId == magazie
                && d.PrimitorId == loc).Select(d => d.ID).ToList();
            var tranzactii = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId != null
                && documente.Contains(t.DocumentId.Value)).Select(t => new { t.DocumentId, t.Fel }).ToList();
            foreach (var t in tranzactii) {
                var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == t.DocumentId)
                    .Select(l => l.ID).ToArray();
                UnicitateFiscala.FaraFapte(os, check, privat, t.DocumentId!.Value, t.Fel, linii);
            }
        }
        finally { Curata(); }
    }

    void Verifica(string id, string mesaj, bool rezultat) =>
        check($"{id} ({(privat ? "privat" : "bugetar")}): {mesaj}", rezultat);

    T Citeste<T>(Func<IObjectSpace, T> actiune) {
        using var os = deschide();
        return actiune(os);
    }

    void Comanda(Action<IObjectSpace> actiune) {
        using var os = deschide();
        actiune(os);
    }

    void Pregateste() {
        using var os = deschide();
        var inceput = new DateOnly(An, 1, 1);
        var sfarsit = new DateOnly(An, 12, 31);
        if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An)
            || os.GetObjectsQuery<Document>().Any(d => d.DataInregistrare >= inceput && d.DataInregistrare <= sfarsit))
            throw new InvalidOperationException($"SC-BCS cere anul {An} liber pe baza de test.");
        foreach (var luna in new[] { 1, 2 }) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = An;
            p.Luna = luna;
        }
        magazie = os.GetObjectsQuery<Gestiune>().Single(g => g.Cod == "MAG1").ID;
        tip = os.GetObjectsQuery<TipMaterial>().Single(t => t.Cod == (privat ? "302" : "302.01.00")).ID;
        debit = os.GetObjectsQuery<Cont>().Single(c => c.Simbol == (privat ? "602" : "602.01.00")).ID;
        credit = os.GetObjectsQuery<Cont>().Single(c => c.Simbol == (privat ? "302" : "302.01.00")).ID;
        var locatie = os.CreateObject<UnitateInterna>();
        locatie.Cod = Marcaj + "-LOC";
        locatie.Denumire = "Loc consum scenarii BCS";
        locatie.Calitati = CalitateRepartitor.LocConsum;
        loc = locatie.ID;
        var partener = os.CreateObject<Partener>();
        partener.Cod = Marcaj + "-FURN";
        partener.Denumire = "Furnizor scenarii BCS";
        furnizor = partener.ID;
        if (!privat) {
            var economic = os.CreateObject<CodEconomic>();
            economic.Cod = Marcaj;
            economic.Denumire = "Clasificație scenarii BCS";
            codEconomic = economic.ID;
        }
        os.CommitChanges();
    }

    Guid Receptioneaza(decimal cantitate = 10m, decimal pret = 10m) {
        Guid facturaId, lotId;
        using (var os = deschide()) {
            var produs = os.CreateObject<Produs>();
            produs.Cod = Marcaj + "-" + ++numar;
            produs.Denumire = produs.Cod;
            produs.UM = "BUC";
            produs.TipMaterial = os.GetObjectsQuery<TipMaterial>().Single(t => t.ID == tip);
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = produs.Cod;
            fct.Data = Intrare;
            fct.Predator = os.GetObjectsQuery<Partener>().Single(p => p.ID == furnizor);
            fct.Primitor = os.GetObjectsQuery<Gestiune>().Single(g => g.ID == magazie);
            var linie = os.CreateObject<FacturaIntrareDetaliu>();
            linie.Document = fct;
            linie.TipMaterial = produs.TipMaterial;
            linie.Cantitate = cantitate;
            linie.PretUnitar = pret;
            linie.CodEconomicId = codEconomic;
            if (privat) linie.TipTva = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == "SFD");
            lotId = linie.CreeazaLot(os, produs, (Gestiune)fct.Primitor).ID;
            facturaId = fct.ID;
            os.CommitChanges();
        }
        var rezultat = Citeste(os => ComenziDocument.Sistem(os).Opereaza(facturaId));
        if (rezultat.ConexId is not Guid nir)
            throw new InvalidOperationException("SC-BCS: recepția cere NIR conex.");
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(nir));
        return lotId;
    }

    Guid Culege(DateOnly data, params (Guid Lot, decimal Cantitate)[] linii) {
        using var os = deschide();
        var doc = os.CreateObject<BonConsum>();
        doc.Data = data;
        doc.Predator = os.GetObjectsQuery<Gestiune>().Single(g => g.ID == magazie);
        doc.Primitor = os.GetObjectsQuery<UnitateInterna>().Single(g => g.ID == loc);
        foreach (var (lotId, cantitate) in linii) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = doc;
            d.TipMaterial = os.GetObjectsQuery<TipMaterial>().Single(t => t.ID == tip);
            if (lotId != Guid.Empty) d.Lot = os.GetObjectsQuery<Lot>().Single(l => l.ID == lotId);
            d.Cantitate = cantitate;
        }
        os.CommitChanges();
        return doc.ID;
    }

    void Postari(string id, Guid docId, N.FelTranzactie fel, DateOnly data,
        params (Guid Lot, decimal Cantitate, decimal Valoare)[] asteptate) {
        using var os = deschide();
        var randuri = os.GetObjectsQuery<C.Postare>()
            .Where(p => p.DocumentId == docId && p.Tranzactie.Fel == fel).ToList();
        var tranzactii = os.GetObjectsQuery<C.Tranzactie>()
            .Where(t => t.DocumentId == docId && t.Fel == fel).ToList();
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == docId).ToList();
        UnicitateFiscala.FaraFapte(os, check, privat, docId, fel, linii.Select(l => l.ID).ToArray());
        var corecte = tranzactii.Count == 1 && tranzactii[0].Data == data
            && randuri.Count == asteptate.Length * 2;
        foreach (var (lot, cantitate, valoare) in asteptate) {
            var produs = os.GetObjectsQuery<Lot>().Single(l => l.ID == lot).ProdusId;
            var linie = linii.Single(l => l.LotId == lot && l.Cantitate == Math.Abs(cantitate));
            bool Comun(C.Postare p) => p.Spatiu == N.Spatiu.Stoc && p.Carte == N.Carte.Contabil
                && p.Data == data && p.Unitate == lot && p.Produs == produs && p.LinieId == linie.ID
                && p.Partener == null && p.TipTvaId == null && p.PerioadaDeclarare == null
                && p.Valuta == null && p.ValoareValuta == 0m && p.Valoare == valoare;
            corecte &= randuri.Count(p => Comun(p) && p.Cont == debit && p.Latura == N.Latura.Debit
                && p.Gestiune == loc && p.Cantitate == cantitate) == 1;
            corecte &= randuri.Count(p => Comun(p) && p.Cont == credit && p.Latura == N.Latura.Credit
                && p.Gestiune == magazie && p.Cantitate == -cantitate) == 1;
        }
        Verifica(id, $"{fel}: exact {asteptate.Length * 2} postări, conturi/laturi/unități/date/măsuri așteptate", corecte);
        if (!corecte) foreach (var p in randuri)
            Console.WriteLine($"     ACTUAL {p.Spatiu}/{p.Carte} {p.Cont}/{p.Latura} unitate={p.Unitate} gestiune={p.Gestiune} q={p.Cantitate} v={p.Valoare} vv={p.ValoareValuta}");
    }

    void Sold(string id, Guid lot, DateOnly data, decimal cantitate, decimal valoare) {
        using var os = deschide();
        var randuri = os.GetObjectsQuery<C.Postare>().Where(p => p.Unitate == lot && p.Cont == credit
            && p.Gestiune == magazie && p.Carte == N.Carte.Contabil && p.Data <= data).ToList();
        var q = randuri.Sum(p => p.Cantitate);
        var v = randuri.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare);
        Verifica(id, $"sold cub la {data}: {cantitate}/{valoare} (obținut {q}/{v})", q == cantitate && v == valoare);
    }

    void FaraEfecte(string id, Guid docId) {
        using var os = deschide();
        Verifica(id, "Draft; zero tranzacții/postări/registre proprii",
            os.GetObjectsQuery<Document>().Single(d => d.ID == docId).Stare == StareDocument.Draft
            && !os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == docId)
            && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == docId)
            && !os.GetObjectsQuery<RegistruStoc>().Any(p => p.DocumentId == docId)
            && !os.GetObjectsQuery<RegistruContabil>().Any(p => p.DocumentId == docId));
    }

    void OperareSiStorno() {
        var lot = Receptioneaza();
        Sold("SC-X-02", lot, Intrare, 10m, 100m);
        var doc = Culege(Consum, (lot, 4m));
        Verifica("SC-BCS-01", "dry-run acceptat", Citeste(os => ComenziDocument.Sistem(os).Valideaza(doc)).Count == 0);
        FaraEfecte("SC-BCS-01", doc);
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc));
        Postari("SC-BCS-01", doc, N.FelTranzactie.Operare, Consum, (lot, 4m, 40m));
        Sold("SC-BCS-01", lot, Sfarsit, 6m, 60m);
        Comanda(os => ComenziDocument.Sistem(os).Storneaza(doc, new(An, 1, 20)));
        Postari("SC-BCS-03", doc, N.FelTranzactie.Storno, new(An, 1, 20), (lot, -4m, -40m));
        Sold("SC-BCS-03", lot, Sfarsit, 10m, 100m);
        Refuza("SC-BCS-10", () => Comanda(os => ComenziDocument.Sistem(os).Storneaza(doc, new(An, 1, 21))), "Operat");
        Verifica("SC-BCS-10", "repetarea stornoului nu adaugă tranzacții",
            Citeste(os => os.GetObjectsQuery<C.Tranzactie>().Count(t => t.DocumentId == doc)) == 2);
        Sold("SC-BCS-10", lot, Sfarsit, 10m, 100m);
    }

    void MaiMulteLinii() {
        var a = Receptioneaza();
        var b = Receptioneaza(10m, 15m);
        var doc = Culege(Consum, (a, 4m), (b, 2m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc));
        Postari("SC-BCS-02", doc, N.FelTranzactie.Operare, Consum, (a, 4m, 40m), (b, 2m, 30m));
        Sold("SC-BCS-02", a, Sfarsit, 6m, 60m);
        Sold("SC-BCS-02", b, Sfarsit, 8m, 120m);
        var comun = Receptioneaza();
        var repetat = Culege(Consum, (comun, 6m), (comun, 4m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(repetat));
        Postari("SC-BCS-02b", repetat, N.FelTranzactie.Operare, Consum, (comun, 6m, 60m), (comun, 4m, 40m));
        Sold("SC-BCS-02b", comun, Sfarsit, 0m, 0m);
        var fractionar = Receptioneaza(10m, 8m);
        var fractie = Culege(Consum, (fractionar, 0.125m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(fractie));
        Postari("SC-BCS-02c", fractie, N.FelTranzactie.Operare, Consum, (fractionar, 0.125m, 1m));
        Sold("SC-BCS-02c", fractionar, Sfarsit, 9.875m, 79m);
    }

    void Linii(string id, string mesaj, Guid docId, decimal[] peLinie, decimal[] postate) {
        using var os = deschide();
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == docId)
            .OrderBy(d => d.Pozitie).Select(d => new { d.ID, d.Valoare }).ToList();
        var iesiri = os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == docId && p.Cont == credit
            && p.Latura == N.Latura.Credit && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .Select(p => new { p.LinieId, p.Valoare }).ToList();
        var alePostarilor = linii.Select(l => iesiri.Where(p => p.LinieId == l.ID).Sum(p => p.Valoare)).ToArray();
        Verifica(id, $"{mesaj}: linii {string.Join("; ", peLinie)} (obținut {string.Join("; ", linii.Select(l => l.Valoare))}), "
            + $"postări de ieșire {string.Join("; ", postate)} (obținut {string.Join("; ", alePostarilor)})",
            linii.Select(l => l.Valoare).SequenceEqual(peLinie) && alePostarilor.SequenceEqual(postate));
    }

    void ValoriLinii() {
        var lot = Receptioneaza(3m, 3.333333m);
        Guid doc;
        using (var os = deschide()) {
            var bon = os.CreateObject<BonConsum>();
            bon.Data = Consum;
            bon.Predator = os.GetObjectsQuery<Gestiune>().Single(g => g.ID == magazie);
            bon.Primitor = os.GetObjectsQuery<UnitateInterna>().Single(g => g.ID == loc);
            foreach (var pozitie in new[] { 1, 2 }) {
                var d = os.CreateObject<DocumentDetaliu>();
                d.Document = bon; d.Pozitie = pozitie;
                d.TipMaterial = os.GetObjectsQuery<TipMaterial>().Single(t => t.ID == tip);
                d.Lot = os.GetObjectsQuery<Lot>().Single(l => l.ID == lot);
                d.Cantitate = 1m;
            }
            os.CommitChanges();
            doc = bon.ID;
        }
        Verifica("SC-BCS-16", "dry-run acceptat", Citeste(os => ComenziDocument.Sistem(os).Valideaza(doc)).Count == 0);
        FaraEfecte("SC-BCS-16", doc);
        Linii("SC-BCS-16", "dry-run-ul nu lasă valori pe linii", doc, [0m, 0m], [0m, 0m]);
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc));
        Linii("SC-BCS-16", "două linii de câte 1 din lotul 3/10", doc, [3.33m, 3.33m], [3.33m, 3.34m]);
        Sold("SC-BCS-16", lot, Sfarsit, 1m, 3.33m);
        var ultima = Culege(new(An, 1, 11), (lot, 1m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(ultima));
        Linii("SC-BCS-16", "ultima bucată", ultima, [3.34m], [3.33m]);
        Sold("SC-BCS-16", lot, Sfarsit, 0m, 0m);
    }

    void Anulare() {
        var lot = Receptioneaza();
        var doc = Culege(Consum, (lot, 4m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc));
        Sold("SC-BCS-05", lot, Sfarsit, 6m, 60m);
        Comanda(os => ComenziDocument.Sistem(os).AnuleazaOperarea(doc));
        FaraEfecte("SC-BCS-05", doc);
        Sold("SC-BCS-05", lot, Sfarsit, 10m, 100m);
        Verifica("SC-BCS-05", "lotul primit prin FCT rămâne", Citeste(os => os.GetObjectsQuery<Lot>().Any(l => l.ID == lot)));
    }

    void UltimaIesire() {
        var lot = Receptioneaza(3m, 0.333333m);
        Sold("SC-BCS-07", lot, Intrare, 3m, 1m);
        var prima = Culege(Consum, (lot, 1m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(prima));
        Postari("SC-BCS-07", prima, N.FelTranzactie.Operare, Consum, (lot, 1m, 0.33m));
        Sold("SC-BCS-07", lot, Consum, 2m, 0.67m);
        var ultima = Culege(new(An, 1, 11), (lot, 2m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(ultima));
        Postari("SC-BCS-07", ultima, N.FelTranzactie.Operare, new(An, 1, 11), (lot, 2m, 0.67m));
        Sold("SC-BCS-07", lot, Sfarsit, 0m, 0m);

        var dual = Receptioneaza(3m, 3.333333m);
        var valori = new[] { 3.33m, 3.34m, 3.33m };
        var peLinie = new[] { 3.33m, 3.33m, 3.34m };
        for (var i = 0; i < valori.Length; i++) {
            var data = new DateOnly(An, 1, 12 + i);
            var doc = Culege(data, (dual, 1m));
            Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc));
            Postari("SC-BCS-15", doc, N.FelTranzactie.Operare, data, (dual, 1m, valori[i]));
            Linii("SC-BCS-15", "linia din soldul registrului", doc, [peLinie[i]], [valori[i]]);
        }
        Sold("SC-BCS-15 (T-r13: evaluare din cub, 0/0)", dual, Sfarsit, 0m, 0m);
    }

    // Ușa entității refuză azi cu textul validării vechi (`ValideazaOperare`,
    // `StocService`, starea documentului), înaintea declarantului; codul stabil
    // se probează pe ușa declarației (`RefuzDeclaratie`) până la TR-D8.
    void Refuza(string id, Action actiune, string fragment) {
        try { actiune(); Verifica(id, "comanda trebuia refuzată", false); }
        catch (OperareException e) {
            Verifica(id, $"refuz pe ușa entității, text vechi „{fragment}” ({e.Message.Split('\n')[0]})",
                e.Message.Contains(fragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    void RefuzDeclaratie(string id, Guid docId, string cod) =>
        Verifica(id, $"refuz pe ușa declarației cu codul stabil `{cod}`", Citeste(os => {
            var doc = os.GetObjectByKey<Document>(docId);
            var tip = os.GetObjectsQuery<TipDocument>().Single(t => t.ClrType == nameof(BonConsum));
            return C.Materializare.Refuzuri(os, doc, tip)
                .Any(l => l.StartsWith(cod + ":", StringComparison.Ordinal));
        }));

    void Refuzuri() {
        var lot = Receptioneaza();
        foreach (var (id, linii, fragment, cod) in new[] {
            ("SC-BCS-08a", new[] { (lot, 0m) }, "cantitate", CoduriRefuz.CantitateNepozitiva),
            ("SC-BCS-08b", new[] { (lot, -1m) }, "cantitate", CoduriRefuz.CantitateNepozitiva),
            ("SC-BCS-08c", new[] { (Guid.Empty, 1m) }, "lot", CoduriRefuz.LotLipsa) }) {
            var doc = Culege(Consum, linii);
            Verifica(id, $"dry-run refuzat cu textul vechi „{fragment}”",
                Citeste(os => ComenziDocument.Sistem(os).Valideaza(doc)).Any(m => m.Contains(fragment, StringComparison.OrdinalIgnoreCase)));
            RefuzDeclaratie(id, doc, cod);
            FaraEfecte(id, doc);
            Refuza(id, () => Comanda(os => ComenziDocument.Sistem(os).Opereaza(doc)), fragment);
            FaraEfecte(id, doc);
            Sold(id, lot, Sfarsit, 10m, 100m);
        }
        var insuficient = Culege(Consum, (lot, 6m), (lot, 5m));
        Verifica("SC-BCS-09", "dry-run refuzat de gardianul cubului, cu cod stabil",
            Citeste(os => ComenziDocument.Sistem(os).Valideaza(insuficient)).Any(m => m.Contains("STOC_INSUFICIENT", StringComparison.Ordinal)));
        FaraEfecte("SC-BCS-09", insuficient);
        Refuza("SC-BCS-09", () => Comanda(os => ComenziDocument.Sistem(os).Opereaza(insuficient)), "STOC_INSUFICIENT");
        FaraEfecte("SC-BCS-09", insuficient);
        Sold("SC-BCS-09", lot, Sfarsit, 10m, 100m);
    }

    void PerioadaInchisa() {
        var a = Receptioneaza();
        var b = Receptioneaza();
        var storno = Culege(Consum, (a, 4m));
        var original = Culege(Consum, (b, 4m));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(storno));
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(original));
        Comanda(os => inchide(os, An, 1));
        Verifica("SC-BCS-04", "ianuarie închis prin comandă",
            Citeste(os => os.GetObjectsQuery<PerioadaFiscala>().Single(p => p.An == An && p.Luna == 1).Inchisa));
        Refuza("SC-BCS-11", () => Comanda(os => ComenziDocument.Sistem(os).AnuleazaOperarea(original)), "închis");
        var refuzat = Culege(Consum, (b, 1m));
        Refuza("SC-BCS-11", () => Comanda(os => ComenziDocument.Sistem(os).Opereaza(refuzat)), "închis");
        FaraEfecte("SC-BCS-11", refuzat);
        Sold("SC-BCS-11", b, Sfarsit, 6m, 60m);
        Comanda(os => ComenziDocument.Sistem(os).Storneaza(storno, Februarie));
        Postari("SC-BCS-04", storno, N.FelTranzactie.Storno, Februarie, (a, -4m, -40m));
        Sold("SC-BCS-04", a, Sfarsit, 6m, 60m);
        Sold("SC-BCS-04", a, Februarie, 10m, 100m);
        var corectie = Citeste(os => ComenziDocument.Sistem(os).Corecteaza(original, Februarie, MotivCorectie.EroareMateriala));
        FaraEfecte("SC-BCS-06", corectie.CorectieId);
        Postari("SC-BCS-06", original, N.FelTranzactie.Storno, Februarie, (b, -4m, -40m));
        Sold("SC-BCS-06", b, Februarie, 10m, 100m);
        using (var os = deschide()) {
            var doc = os.GetObjectsQuery<Document>().Single(d => d.ID == corectie.CorectieId);
            Verifica("SC-BCS-06", "legătură/motiv/date păstrate; original Stornat",
                doc.CorecteazaId == original && doc.MotivCorectie == MotivCorectie.EroareMateriala
                && doc.Data == Consum && doc.DataInregistrare == Februarie
                && corectie.StareOriginal == StareDocument.Stornat);
            os.GetObjectsQuery<DocumentDetaliu>().Single(d => d.DocumentId == doc.ID).Cantitate = 3m;
            os.CommitChanges();
        }
        Comanda(os => ComenziDocument.Sistem(os).Opereaza(corectie.CorectieId));
        Postari("SC-BCS-06", corectie.CorectieId, N.FelTranzactie.Operare, Februarie, (b, 3m, 30m));
        Sold("SC-BCS-06", b, Sfarsit, 6m, 60m);
        Sold("SC-BCS-06", b, Februarie, 7m, 70m);
    }

    void Curata() {
        using var os = deschide();
        var reps = os.GetObjectsQuery<Repartitor>()
            .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
        if (reps.Count == 0) return;
        var docs = os.GetObjectsQuery<Document>()
            .Where(d => reps.Contains(d.PredatorId) || reps.Contains(d.PrimitorId)).Select(d => d.ID).ToList();
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.Produs.Cod.StartsWith(Marcaj)).Select(l => l.ID).ToList();
        var pj = new Purja(os);
        ProbeCub.Purjeaza(pj, os, docs);
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<PartidaDeschisa>().Where(s => s.An == An));
        pj.Adauga(os.GetObjectsQuery<Imperechere>().Where(i => docs.Contains(i.DocumentId) || docs.Contains(i.DocumentStingatorId)));
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
