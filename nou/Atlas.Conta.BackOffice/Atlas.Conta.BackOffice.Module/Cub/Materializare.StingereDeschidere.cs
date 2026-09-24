using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static partial class Materializare {
    public const string StingereDeschidereInvalida = "STINGERE_DESCHIDERE_INVALIDA";

    /// <summary>Stinge o partidă inițială în tranzacția apelantului, fără commit sau document fictiv.</summary>
    public static void Imperecheaza(IObjectSpace os, Document stingator, Guid partida, decimal suma, DateOnly data) {
        ArgumentNullException.ThrowIfNull(stingator);
        CereTranzactie(os);
        var db = ((EFCoreObjectSpace)os).DbContext;
        if (db.Entry(stingator).State != EntityState.Unchanged)
            throw new OperareException($"{StingereDeschidereInvalida}: Stingătorul trebuie să fie salvat.");
        _ = db.Database.SqlQuery<Guid>($"""SELECT "ID" AS "Value" FROM "Documente" WHERE "ID" = {stingator.ID} AND "GCRecord" = 0 FOR UPDATE""").ToList();
        db.Entry(stingator).Reload();
        GardianPerioada.VerificaDeschisa(os, data);
        var initiale = os.GetObjectsQuery<Postare>().Where(p => p.Unitate == partida && p.Carte == N.Carte.Contabil
            && p.FelUnitate == N.FelUnitate.Partida
            && p.Spatiu == N.Spatiu.Contabil && p.Tranzactie.Fel == N.FelTranzactie.Deschidere).ToList();
        if (initiale.Count != 1 || stingator.Stare != StareDocument.Operat || suma <= 0
            || !N.Scara.EsteLa(suma, N.Scara.Bani) || data < stingator.DataInregistrare
            || data < initiale[0].Data)
            throw new OperareException($"{StingereDeschidereInvalida}: Partidă, stare, sumă sau dată invalidă.");
        var initiala = initiale[0];
        _ = db.Database.SqlQuery<Guid>($"""SELECT "ID" AS "Value" FROM "Tranzactie" WHERE "ID" = {initiala.TranzactieId} FOR UPDATE""").ToList();
        var aleStingatorului = os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == stingator.ID
            && p.Carte == N.Carte.Contabil && p.Spatiu == N.Spatiu.Contabil).ToList();
        var primite = os.GetObjectsQuery<Postare>().Where(p => p.Unitate == partida && p.Carte == N.Carte.Contabil).ToList();
        var inMemorie = os.ModifiedObjects.OfType<Postare>().Where(p => os.IsNewObject(p)).ToArray();
        aleStingatorului = [.. aleStingatorului.Concat(inMemorie.Where(p => p.DocumentId == stingator.ID
            && p.Carte == N.Carte.Contabil && p.Spatiu == N.Spatiu.Contabil)).DistinctBy(p => p.ID)];
        primite = [.. primite.Concat(inMemorie.Where(p => p.Unitate == partida && p.Carte == N.Carte.Contabil)).DistinctBy(p => p.ID)];
        var operare = aleStingatorului.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare
            && p.Cont == initiala.Cont && p.Partener == initiala.Partener).ToList();
        var proprie = IdentitatiPartide.Gaseste(operare.Select(p => Randuri.Citeste(p).Coordonate.Unitate)
            .OfType<N.Unitate>(), stingator.ID, initiala.Cont, initiala.Partener!.Value);
        var latura = initiala.Latura == N.Latura.Debit ? N.Latura.Credit : N.Latura.Debit;
        if (proprie == null || !Disponibil(primite, initiala.Latura, data, suma)
            || !Disponibil(aleStingatorului.Where(p => p.Unitate == proprie.Id), latura, data, suma))
            throw new OperareException($"{StingereDeschidereInvalida}: Cont/partener incompatibil sau suma depășește restul disponibil.");
        var rezultat = Transferuri.Muta(new(stingator.ID, stingator.DataInregistrare,
            [.. operare.Select(Randuri.Citeste)],
            [.. aleStingatorului.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Data <= data).Select(Randuri.Citeste)],
            Guid.Empty, initiala.Data, [Randuri.Citeste(initiala)],
            [.. primite.Where(p => p.Tranzactie.Fel != N.FelTranzactie.Deschidere && p.Data <= data).Select(Randuri.Citeste)],
            suma, data, partida));
        if (rezultat.Refuz != null || rezultat.Mutare is null)
            throw new OperareException($"{StingereDeschidereInvalida}: {rezultat.Refuz?.Mesaj ?? rezultat.Sarit}");
        var contract = N.Motor.Transfera(stingator.ID, data, [rezultat.Mutare], new N.Rotunjire(Scara.ConventieBani));
        if (!contract.EsteAcceptat) throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
        foreach (var t in contract.Tranzactii) Scrie(os, stingator.ID, t);
    }

    static bool Disponibil(IEnumerable<Postare> postari, N.Latura latura, DateOnly data, decimal suma) {
        var zile = postari.GroupBy(p => p.Data < data ? data : p.Data).OrderBy(g => g.Key);
        decimal sold = 0;
        foreach (var zi in zile) {
            sold += zi.Sum(p => p.Latura == latura ? p.Valoare : -p.Valoare);
            if (sold < suma) return false;
        }
        return sold >= suma;
    }
}
