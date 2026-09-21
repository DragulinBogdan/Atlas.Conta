using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>
/// Regimul dual (S-D3, S-D4, S-D5): pe tipurile cu <c>PosteazaInCub</c> declarația
/// frunzei se materializează în ACEEAȘI tranzacție de comandă cu registrele vechi.
/// </summary>
public static class Materializare {
    public static void Opereaza(IObjectSpace os, Document doc, TipDocument tip) {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(tip);
        if (!tip.PosteazaInCub)
            return;
        var contract = Contracteaza(os, doc, tip);
        if (!contract.EsteAcceptat)
            throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
        foreach (var tranzactie in contract.Tranzactii)
            Scrie(os, doc.ID, tranzactie);
    }

    /// <summary>Refuzurile declarației pentru dry-run (S-D4): citește, nu scrie nimic.</summary>
    public static IReadOnlyList<string> Refuzuri(IObjectSpace os, Document doc, TipDocument tip) {
        ArgumentNullException.ThrowIfNull(doc);
        ArgumentNullException.ThrowIfNull(tip);
        if (!tip.PosteazaInCub)
            return [];
        N.Contract contract;
        try {
            contract = Contracteaza(os, doc, tip);
        }
        catch (OperareException eroare) {
            return [eroare.Message];
        }
        return contract.EsteAcceptat ? [] : Mesaje(contract.Refuzuri);
    }

    public static void Storneaza(IObjectSpace os, Document doc, DateOnly dataStorno) {
        ArgumentNullException.ThrowIfNull(doc);
        if (!MotorOperare.GasesteTipDocument(os, doc).PosteazaInCub)
            return;
        // T-D2: transferul de stoc e al documentului; cel pe partidă e al împerecherii (S-D13).
        var aleDocumentului = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == doc.ID
                && (p.Tranzactie.Fel == N.FelTranzactie.Operare
                    || (p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Spatiu == N.Spatiu.Stoc)))
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
            .Select(p => (p.ID, Randuri.Citeste(p)))
            .ToList();
        var tranzactie = N.Storno.Inverseaza(
            N.Storno.Selecteaza(citite, doc.ID),
            doc.ID,
            dataStorno,
            (dataStorno.Year * 100) + dataStorno.Month);
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
            .Where(p => p.DocumentId == stingator.ID)
            .ToList();
        var aleStinsului = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == stins.ID && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .ToList();
        // Plafonul e RESTUL partidei stinsului: `Operare` (care poartă deja recepția,
        // TR-D3) plus ce au așezat pe ea transferurile ORICĂRUI stingător.
        var partideleStinsului = aleStinsului.Select(p => p.Unitate).OfType<Guid>().Distinct().ToList();
        var primiteDeStins = partideleStinsului.Count == 0
            ? []
            : os.GetObjectsQuery<Postare>()
                .Where(p => p.Unitate != null && partideleStinsului.Contains(p.Unitate.Value)
                    && p.Tranzactie.Fel == N.FelTranzactie.Transfer)
                .ToList();
        var rezultat = Transferuri.Muta(new Transferuri.Cerere(
            stingator.ID,
            stingator.DataInregistrare,
            [.. Citeste(aleStingatorului, N.FelTranzactie.Operare)],
            [.. Citeste(aleStingatorului, N.FelTranzactie.Transfer)],
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
        if (!MotorOperare.GasesteTipDocument(os, doc).PosteazaInCub)
            return;
        // T-D2: sub `VerificaFaraImperecheri`, orice `Transfer` al documentului e al lui.
        var tranzactii = os.GetObjectsQuery<Tranzactie>()
            .Where(t => t.DocumentId == doc.ID
                && (t.Fel == N.FelTranzactie.Operare || t.Fel == N.FelTranzactie.Transfer))
            .ToList();
        if (tranzactii.Count == 0)
            return;
        var ids = tranzactii.Select(t => t.ID).ToList();
        os.Delete(os.GetObjectsQuery<Postare>().Where(p => ids.Contains(p.TranzactieId)).ToList());
        os.Delete(tranzactii);
    }

    static N.Contract Contracteaza(IObjectSpace os, Document doc, TipDocument tip) =>
        doc.Declarant() is null
            ? throw new OperareException(
                $"Tipul {tip.Cod} e marcat PosteazaInCub, dar clasa "
                + $"{MotorOperare.ClasaReala(doc).Name} nu declară.")
            : Contractare.Contracteaza(os, doc);

    static void Scrie(IObjectSpace os, Guid documentId, N.Tranzactie tranzactie) {
        var rand = os.CreateObject<Tranzactie>();
        rand.DocumentId = documentId;
        rand.Fel = tranzactie.Fel;
        rand.Data = tranzactie.Data;
        rand.ScrisLa = DateTime.UtcNow;
        foreach (var postare in tranzactie.Postari)
            Randuri.Scrie(postare, rand, os.CreateObject<Postare>());
    }

    static IReadOnlyList<string> Mesaje(IReadOnlyList<N.Refuz> refuzuri) =>
        [.. refuzuri.Select(r => r.Linie is Guid linie
            ? $"{r.Cod}: {r.Mesaj} [{linie}]"
            : $"{r.Cod}: {r.Mesaj}")];
}
