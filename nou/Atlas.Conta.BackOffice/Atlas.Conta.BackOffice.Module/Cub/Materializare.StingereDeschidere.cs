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
        BlocheazaDocumente(os, stingator);
        GardianPerioada.VerificaDeschisa(os, data);
        var initiale = os.GetObjectsQuery<Postare>().Where(p => p.Unitate == partida && p.Carte == N.Carte.Contabil
            && p.FelUnitate == N.FelUnitate.Partida
            && p.Spatiu == N.Spatiu.Contabil && p.Tranzactie.Fel == N.FelTranzactie.Deschidere).ToList();
        if (initiale.Count == 1 && data < initiale[0].Data)
            throw new OperareException($"{StingereDeschidereInvalida}: Data stingerii precedă deschiderea.");
        if (initiale.Count != 1 || stingator.Stare != StareDocument.Operat || suma <= 0
            || !N.Scara.EsteLa(suma, N.Scara.Bani) || data < stingator.DataInregistrare
            || data < initiale[0].Data)
            throw new OperareException($"{StingereDeschidereInvalida}: Partidă, stare, sumă sau dată invalidă.");
        var initiala = initiale[0];
        if (initiala.Valuta != null || initiala.ValoareValuta != 0)
            throw new OperareException($"{StingereDeschidereInvalida}: Stingerea partidelor în valută nu este încă acoperită.");
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
        var peProprie = proprie == null ? [] : os.GetObjectsQuery<Postare>()
            .Where(p => p.Unitate == proprie.Id && p.Carte == N.Carte.Contabil).ToList();
        peProprie = [.. peProprie.Concat(inMemorie.Where(p => p.Unitate == proprie?.Id
            && p.Carte == N.Carte.Contabil)).DistinctBy(p => p.ID)];
        if (proprie == null || !Disponibil(primite, initiala.Latura, data, suma)
            || !Disponibil(peProprie, latura, data, suma))
            throw new OperareException($"{StingereDeschidereInvalida}: Cont/partener incompatibil sau suma depășește restul disponibil.");
        var rezultat = Transferuri.Muta(new(stingator.ID, stingator.DataInregistrare,
            [.. operare.Select(Randuri.Citeste)],
            [.. peProprie.Where(p => !operare.Any(o => o.ID == p.ID) && p.Data <= data).Select(Randuri.Citeste)],
            Guid.Empty, initiala.Data, [Randuri.Citeste(initiala)],
            [.. primite.Where(p => p.Tranzactie.Fel != N.FelTranzactie.Deschidere && p.Data <= data).Select(Randuri.Citeste)],
            suma, data, partida));
        if (rezultat.Refuz != null || rezultat.Mutare is null)
            throw new OperareException($"{StingereDeschidereInvalida}: {rezultat.Refuz?.Mesaj ?? rezultat.Sarit}");
        var contract = N.Motor.Transfera(stingator.ID, data, [rezultat.Mutare], new N.Rotunjire(Scara.ConventieBani));
        if (!contract.EsteAcceptat) throw new OperareException(string.Join("\n", Mesaje(contract.Refuzuri)));
        foreach (var t in contract.Tranzactii) Scrie(os, stingator.ID, t);
    }

    internal static void BlocheazaDocumente(IObjectSpace os, params Document[] documente) {
        CereTranzactie(os);
        var db = ((EFCoreObjectSpace)os).DbContext;
        foreach (var doc in documente.DistinctBy(d => d.ID).OrderBy(d => d.ID)) {
            _ = db.Database.SqlQuery<Guid>($"""SELECT "ID" AS "Value" FROM "Documente" WHERE "ID" = {doc.ID} AND "GCRecord" = 0 FOR UPDATE""").ToList();
            if (db.Entry(doc).State == EntityState.Unchanged) db.Entry(doc).Reload();
        }
    }

    public sealed class AsignareInitiala {
        public Guid DocumentId { get; set; }
        public DateOnly Data { get; set; }
        public Guid? Partener { get; set; }
        public N.Latura LaturaInitiala { get; set; }
        public decimal Suma { get; set; }
    }

    public static IQueryable<AsignareInitiala> AsignariDeschidere(IObjectSpace os) {
        var postari = os.GetObjectsQuery<Postare>();
        var initiale = postari.Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere
            && p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida);
        return from p in postari
            join i in initiale on p.Unitate equals i.Unitate
            where p.DocumentId != null && p.Carte == N.Carte.Contabil
            select new AsignareInitiala { DocumentId = p.DocumentId.Value, Data = p.Data,
                Partener = i.Partener, LaturaInitiala = i.Latura,
                Suma = p.Latura == i.Latura ? -p.Valoare : p.Valoare };
    }

    public static decimal AsignatDeschidere(IObjectSpace os, Guid document, Guid? partener = null, SensStingere? sens = null) {
        var asignari = AsignariDeschidere(os).Where(p => p.DocumentId == document);
        if (partener != null) asignari = asignari.Where(p => p.Partener == partener);
        if (sens != null) {
            var latura = sens == SensStingere.Datorie ? N.Latura.Credit : N.Latura.Debit;
            asignari = asignari.Where(p => p.LaturaInitiala == latura);
        }
        return asignari.Sum(p => (decimal?)p.Suma) ?? 0m;
    }

    static IQueryable<Postare> StingeriDeschidere(IObjectSpace os, Guid document) {
        var initiale = os.GetObjectsQuery<Postare>().Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere
            && p.Carte == N.Carte.Contabil && p.FelUnitate == N.FelUnitate.Partida).Select(p => p.Unitate);
        return os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == document
            && p.Tranzactie.Fel == N.FelTranzactie.Transfer && initiale.Contains(p.Unitate));
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
