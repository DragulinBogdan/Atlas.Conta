using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>
/// Regimul dual (S-D3, S-D4, S-D5): pe tipurile cu <c>PosteazaInCub</c> declarația
/// frunzei se materializează în ACEEAȘI tranzacție de comandă cu registrele vechi.
/// </summary>
public static partial class Materializare {
    public static bool EsteConexAcoperit(IObjectSpace os, Document doc) =>
        ReceptiiConexe.EsteAcoperita(os, doc);

    public static void Opereaza(IObjectSpace os, Document doc, TipDocument tip) {
        using var receptie = ReceptiiConexe.IncepeCitirea(os, doc, blocheaza: true);
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(tip);
        if (!tip.PosteazaInCub)
            return;
        if (doc.Detalii.OfType<ILinieCuImobilizare>().Any())
            BlocheazaNominalizarea(os);
        ReceptiiConexe.Fixeaza(os, doc);
        var contract = Contracteaza(os, doc, tip);
        if (!contract.EsteAcceptat)
            throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
        ReceptiiConexe.VerificaAnaliza(os, doc, contract.Tranzactii.SelectMany(t => t.Postari));
        ReceptiiConexe.VerificaStoc(os, doc, contract.Tranzactii.SelectMany(t => t.Postari));
        VerificaPozitiaFaraFisa(os, contract.Tranzactii.SelectMany(t => t.Postari), blocheaza: true);
        foreach (var tranzactie in contract.Tranzactii.Where(t => t.Postari.Count > 0))
            Scrie(os, doc.ID, tranzactie);
    }

    /// <summary>Refuzurile declarației pentru dry-run (S-D4): citește, nu scrie nimic.</summary>
    public static IReadOnlyList<string> Refuzuri(IObjectSpace os, Document doc, TipDocument tip) {
        using var receptie = ReceptiiConexe.IncepeCitirea(os, doc, blocheaza: false);
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(tip);
        if (!tip.PosteazaInCub)
            return [];
        N.Contract contract;
        try {
            contract = Contracteaza(os, doc, tip);
            if (contract.EsteAcceptat)
                ReceptiiConexe.VerificaAnaliza(os, doc, contract.Tranzactii.SelectMany(t => t.Postari));
            if (contract.EsteAcceptat)
                ReceptiiConexe.VerificaStoc(os, doc, contract.Tranzactii.SelectMany(t => t.Postari));
            if (contract.EsteAcceptat)
                VerificaPozitiaFaraFisa(os, contract.Tranzactii.SelectMany(t => t.Postari), blocheaza: false);
        }
        catch (OperareException eroare) {
            return [eroare.Message];
        }
        return contract.EsteAcceptat ? [] : Mesaje(contract.Refuzuri);
    }

    public static void Storneaza(IObjectSpace os, Document doc, DateOnly dataStorno) {
        ArgumentNullException.ThrowIfNull(doc);
        ReceptiiConexe.VerificaFaraDependenti(os, doc);
        if (StingeriDeschidere(os, doc.ID).Any(p => p.Data > dataStorno))
            throw new OperareException($"{StingereDeschidereInvalida}: Data stornării precedă data stingerii partidei inițiale.");
        VerificaPartideFaraDependenti(os, doc, dataStorno);
        VerificaSuportFaraDependenti(os, doc, dataStorno);
        var transferuriStoc = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == doc.ID && p.Tranzactie.Fel == N.FelTranzactie.Transfer
                && (p.Spatiu == N.Spatiu.Stoc || p.FelUnitate == N.FelUnitate.Fisa) && p.Unitate != null)
            .Select(p => p.TranzactieId);
        var partideInitiale = os.GetObjectsQuery<Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere && p.Spatiu == N.Spatiu.Contabil && p.Unitate != null)
            .Select(p => p.Unitate);
        var transferuriDeschidere = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == doc.ID && p.Tranzactie.Fel == N.FelTranzactie.Transfer
                && p.Unitate != null && partideInitiale.Contains(p.Unitate)).Select(p => p.TranzactieId);
        var aleDocumentului = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == doc.ID
                && (p.Tranzactie.Fel == N.FelTranzactie.Operare
                    || transferuriStoc.Contains(p.TranzactieId) || transferuriDeschidere.Contains(p.TranzactieId)))
            .ToList();
        // Operat înainte ca tipul lui să fie migrat: stornoul nu atinge cubul (S-D5).
        if (aleDocumentului.Count == 0)
            return;
        var tinte = aleDocumentului.Select(p => p.ID).ToList();
        var atribuite = os.GetObjectsQuery<Postare>()
            .Where(p => p.Atribuit != null && tinte.Contains(p.Atribuit.Value))
            .ToList();
        var citite = aleDocumentului.Concat(atribuite)
            .DistinctBy(p => p.ID)
            .Select(p => (p.ID, Randuri.Citeste(p) with {
                InversaDin = new N.ReferintaPostare(p.ID, p.Spatiu),
            }))
            .ToList();
        var tranzactie = N.Storno.Inverseaza(
            N.Storno.Selecteaza(citite, doc.ID),
            doc.ID,
            dataStorno,
            (dataStorno.Year * 100) + dataStorno.Month);
        var refuzuri = N.Conservare.Verifica(tranzactie);
        if (refuzuri.Count > 0)
            throw new OperareException(string.Join("\n", Mesaje(refuzuri)));
        ReceptiiConexe.VerificaStoc(os, doc, tranzactie.Postari);
        VerificaPozitiaFaraFisa(os, tranzactie.Postari, blocheaza: true);
        Scrie(os, doc.ID, tranzactie);
    }

    /// <summary>
    /// S-D13: împerecherea de după operare devine o tranzacție <c>Transfer</c> pe
    /// stingător. Fără commit: e în tranzacția apelantului.
    /// </summary>
    public static void Imperecheaza(
            IObjectSpace os, Document stingator, Document stins, decimal suma, DateOnly data) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(stingator);
        ArgumentNullException.ThrowIfNull(stins);
        if (!MotorOperare.GasesteTipDocument(os, stingator).PosteazaInCub
            || !MotorOperare.GasesteTipDocument(os, stins).PosteazaInCub)
            return;
        var aleStingatorului = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == stingator.ID && p.Carte == N.Carte.Contabil
                && p.FelUnitate == N.FelUnitate.Partida)
            .ToList();
        var aleStinsului = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == stins.ID && p.Tranzactie.Fel == N.FelTranzactie.Operare
                && p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida)
            .ToList();
        var idOperareStingator = aleStingatorului.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare).Select(p => p.ID).ToArray();
        var partideleStingatorului = aleStingatorului.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .Select(p => p.Unitate).OfType<Guid>().Distinct().ToArray();
        var primiteDeStingator = os.GetObjectsQuery<Postare>()
            .Where(p => p.Unitate != null && partideleStingatorului.Contains(p.Unitate.Value)
                && p.Carte == N.Carte.Contabil && !idOperareStingator.Contains(p.ID)).ToList();
        var partideleStinsului = aleStinsului.Select(p => p.Unitate).OfType<Guid>().Distinct().ToList();
        var primiteDeStins = partideleStinsului.Count == 0
            ? []
            : os.GetObjectsQuery<Postare>()
                .Where(p => p.Unitate != null && partideleStinsului.Contains(p.Unitate.Value)
                    && p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida
                    && (p.DocumentId != stins.ID || p.Tranzactie.Fel != N.FelTranzactie.Operare))
                .ToList();
        var rezultat = Transferuri.Muta(new Transferuri.Cerere(
            stingator.ID,
            stingator.DataInregistrare,
            [.. Citeste(aleStingatorului, N.FelTranzactie.Operare)],
            [.. primiteDeStingator.Select(Randuri.Citeste)],
            stins.ID,
            stins.DataInregistrare,
            [.. aleStinsului.Select(Randuri.Citeste)],
            [.. primiteDeStins.Select(Randuri.Citeste)],
            suma,
            data));
        if (rezultat.Refuz is { } refuz)
            throw new OperareException(string.Join("\n", Mesaje([refuz])));
        if (rezultat.Mutare is not { } mutare)
            return;
        var contract = N.Motor.Transfera(
            stingator.ID, rezultat.Data, [mutare], new N.Rotunjire(Scara.ConventieBani));
        if (!contract.EsteAcceptat)
            throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
        foreach (var tranzactie in contract.Tranzactii)
            Scrie(os, stingator.ID, tranzactie);
    }

    static IEnumerable<N.Postare> Citeste(IEnumerable<Postare> randuri, N.FelTranzactie fel) =>
        randuri.Where(p => p.Tranzactie.Fel == fel).Select(Randuri.Citeste);

    public static void Anuleaza(IObjectSpace os, Document doc) {
        ArgumentNullException.ThrowIfNull(doc);
        ReceptiiConexe.VerificaFaraDependenti(os, doc);
        VerificaPartideFaraDependenti(os, doc, doc.DataInregistrare);
        VerificaSuportFaraDependenti(os, doc, doc.DataInregistrare);
        if (StingeriDeschidere(os, doc.ID).Any())
            throw new OperareException($"{StingereDeschidereInvalida}: Documentul are stingere de partidă inițială; folosiți storno.");
        var tranzactii = os.GetObjectsQuery<Tranzactie>()
            .Where(t => t.DocumentId == doc.ID
                && (t.Fel == N.FelTranzactie.Operare || t.Fel == N.FelTranzactie.Transfer))
            .ToList();
        if (tranzactii.Count == 0)
            return;
        var ids = tranzactii.Select(t => t.ID).ToList();
        var postari = os.GetObjectsQuery<Postare>().Where(p => ids.Contains(p.TranzactieId)).ToList();
        ReceptiiConexe.VerificaStoc(os, doc, postari.Select(p => Randuri.Citeste(p) with { Cantitate = -p.Cantitate }));
        VerificaPozitiaFaraFisa(os, postari.Select(p => Randuri.Citeste(p) with { Valoare = -p.Valoare }), blocheaza: true);
        os.Delete(postari);
        os.Delete(tranzactii);
    }

    static void VerificaPartideFaraDependenti(IObjectSpace os, Document doc, DateOnly deLa) {
        var unitati = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == doc.ID && p.Tranzactie.Fel == N.FelTranzactie.Operare
                && p.Spatiu == N.Spatiu.Contabil && p.Unitate != null && p.Partener != null)
            .ToList().Select(p => Randuri.Citeste(p).Coordonate.Unitate)
            .Where(u => IdentitatiPartide.EsteProprie(u, doc.ID)).Select(u => u.Id).Distinct().ToList();
        if (unitati.Count == 0) return;
        var dependenti = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId != doc.ID && p.Unitate != null && unitati.Contains(p.Unitate.Value)
                && p.Spatiu == N.Spatiu.Contabil && p.Carte == N.Carte.Contabil && p.LinieId != null)
            .GroupBy(p => new { p.DocumentId, p.Unitate, p.Data })
            .Select(g => new { g.Key.DocumentId, g.Key.Unitate, g.Key.Data,
                Net = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) })
            .Where(p => p.Net != 0m).ToList();
        // Inversarea ulterioară a dependentului nu eliberează retroactiv sursa.
        // Verificăm soldul la data cerută și fiecare schimbare deja înregistrată
        // după ea; mișcările din aceeași zi se compensează împreună.
        var activ = dependenti.GroupBy(p => new { p.DocumentId, p.Unitate }).Any(g => {
            decimal sold = 0;
            foreach (var zi in g.GroupBy(p => p.Data < deLa ? deLa : p.Data).OrderBy(p => p.Key)) {
                sold += zi.Sum(p => p.Net);
                if (sold != 0m) return true;
            }
            return false;
        });
        if (activ)
            throw new OperareException($"{CoduriRefuz.PartidaCuDependenti}: Partida documentului este nominalizată de alte documente active.");
    }

    static N.Contract Contracteaza(IObjectSpace os, Document doc, TipDocument tip) =>
        doc.Declarant() is null
            ? throw new OperareException(
                $"Tipul {tip.Cod} e marcat PosteazaInCub, dar clasa "
                + $"{MotorOperare.ClasaReala(doc).Name} nu declară.")
            : Contractare.Contracteaza(os, doc);

    static Guid Scrie(IObjectSpace os, Guid? documentId, N.Tranzactie tranzactie) {
        var tracker = (os as EFCoreObjectSpace)?.DbContext.ChangeTracker;
        var incarcare = tracker?.LazyLoadingEnabled;
        try {
            if (tracker != null) tracker.LazyLoadingEnabled = false;
            var rand = os.CreateObject<Tranzactie>();
            rand.DocumentId = documentId;
            rand.Fel = tranzactie.Fel;
            rand.Data = tranzactie.Data;
            rand.ScrisLa = DateTime.UtcNow;
            foreach (var postare in tranzactie.Postari)
                Randuri.Scrie(postare, rand, os.CreateObject<Postare>());
            return rand.ID;
        }
        finally { if (tracker != null) tracker.LazyLoadingEnabled = incarcare.Value; }
    }

    static IReadOnlyList<string> Mesaje(IReadOnlyList<N.Refuz> refuzuri) =>
        [.. refuzuri.Select(Contractare.Mesaj)];
}
