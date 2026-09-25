using System.Runtime.CompilerServices;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>Cifrele unei perioade de referință la reconstrucție, înainte de rescriere.</summary>
public sealed record RandReconstructie(int An, int Luna,
    long ContabilExistente, long ContabilRecalculate, long ContabilDiferite,
    long StocExistente, long StocRecalculate, long StocDiferite,
    decimal DiferentaDebit, decimal DiferentaCredit, decimal DiferentaCantitate, decimal DiferentaValoare,
    long PartideExistente, long PartideRecalculate, long PartideDiferite, decimal DiferentaRest);

/// <summary>Raportul comenzii de reconstrucție: un rând per perioadă de referință, chiar și fără diferențe.</summary>
public sealed record RaportReconstructie(IReadOnlyList<RandReconstructie> Referinte);

// Formele pe care le cere `SqlQuery<T>` pentru un rezultat NEscalar: publice,
// nesigilate, cu proprietăți `virtual` — `UseChangeTrackingProxies` refuză
// altfel tipul, la execuție (precedentul `FisaContSql`).
public class DiferentaContabilSql {
    public virtual long Existente { get; set; }
    public virtual long Recalculate { get; set; }
    public virtual long Diferite { get; set; }
    public virtual decimal DiferentaDebit { get; set; }
    public virtual decimal DiferentaCredit { get; set; }
}

public class DiferentaStocSql {
    public virtual long Existente { get; set; }
    public virtual long Recalculate { get; set; }
    public virtual long Diferite { get; set; }
    public virtual decimal DiferentaCantitate { get; set; }
    public virtual decimal DiferentaValoare { get; set; }
}

public class DiferentaPartideSql {
    public virtual long Existente { get; set; }
    public virtual long Recalculate { get; set; }
    public virtual long Diferite { get; set; }
    public virtual decimal DiferentaRest { get; set; }
}

public class PerioadaSnapshotSql {
    public virtual int An { get; set; }
    public virtual int Luna { get; set; }
}

public struct SnapshotPartida {
    public Guid UnitateId { get; set; }
    public Guid ContId { get; set; }
    public Guid PartenerId { get; set; }
    public DateOnly Deschisa { get; set; }
    public Guid? DocumentId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

// Soldurile materializate la închidere (F27-D3). Scrierea e SQL brut Postgres,
// în tranzacția comenzii: cheia completă a atomului are 9 coloane și 8 dintre
// ele sunt nullable, deci incrementala se face `UNION ALL` + `GROUP BY`, nu
// JOIN — `IS NOT DISTINCT FROM` nu e hashable, iar planul ar cădea pe nested
// loop (spike B.1). Ca la fișa de cont, `"GCRecord" = 0` se scrie EXPLICIT:
// SQL-ul brut nu trece prin filtrul global (66).
public static class SolduriService {
    static readonly string[] Dimensiuni = [
        "RepartitorId", "GestiuneId", "MaterialId", "CodFunctionalId", "CodEconomicId",
        "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId"
    ];

    const string Contabil = "\"SolduriPerioadaContabil\"";
    const string Stoc = "\"SolduriPerioadaStoc\"";
    const string Partide = "\"PartideDeschise\"";

    // Sentinela de comparare a dimensiunilor lipsă: `IS NOT DISTINCT FROM` ar
    // fi corect, dar nu e hashable. `Guid.Empty` nu poate fi id de rând real
    // (FK-urile trimit în nomenclatoare), deci coalescența e fără pierdere.
    const string Nil = "'00000000-0000-0000-0000-000000000000'::uuid";

    /// <summary>Perioadele DE REFERINȚĂ: ultima închisă plus fiecare decembrie închis.</summary>
    public static IReadOnlyList<(int An, int Luna)> Referinte(IObjectSpace os) {
        var inchise = os.GetObjectsQuery<PerioadaFiscala>()
            .Where(p => p.Inchisa)
            .Select(p => new { p.An, p.Luna })
            .ToList()
            .OrderBy(p => p.An).ThenBy(p => p.Luna)
            .ToList();
        if (inchise.Count == 0)
            return [];
        var referinte = new SortedSet<(int An, int Luna)>();
        referinte.Add((inchise[^1].An, inchise[^1].Luna));
        foreach (var p in inchise.Where(p => p.Luna == 12))
            referinte.Add((p.An, p.Luna));
        return referinte.ToList();
    }

    /// <summary>Scrie snapshot-ul perioadei: incremental din P−1 dacă îl are, altfel `SUM` integral.</summary>
    public static void Materializeaza(IObjectSpace os, int an, int luna) {
        Elimina(os, an, luna);
        var (anPrec, lunaPrec) = Precedenta(an, luna);
        var precedentaContabil = SnapshotContabilDinCub(os, anPrec, lunaPrec) ? (anPrec, lunaPrec) : ((int, int)?)null;
        var precedentaStoc = AreRanduri(os, Stoc, anPrec, lunaPrec) ? (anPrec, lunaPrec) : ((int, int)?)null;
        ScrieContabil(os, an, luna, precedentaContabil);
        ScrieStoc(os, an, luna, precedentaStoc);
        MaterializeazaPartide(os, an, luna);
    }

    public static void MaterializeazaPartide(IObjectSpace os, int an, int luna) {
        EliminaPartide(os, an, luna);
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var sursa = SursaPartide(os, P, an, luna);
        Executa(os, $"""
            INSERT INTO {Partide} ("ID", "GCRecord", "OptimisticLockField", "An", "Luna",
                "UnitateId", "ContId", "PartenerId", "Deschisa", "DocumentId", "Debit", "Credit", "Rest", "DinCub")
            SELECT gen_random_uuid(), 0, 0, {P(an)}, {P(luna)},
                s."UnitateId", s."ContId", s."PartenerId", s."Deschisa", s."DocumentId",
                s."Debit", s."Credit", ABS(s."Debit" - s."Credit"), true
            FROM ({sursa}) s
            """, argumente.ToArray());
    }

    static string SursaPartide(IObjectSpace os, Func<object, string> parametrul, int an, int luna) =>
        SqlInterogare.Compune(
            from s in Cub.Citiri.Partide.Solduri(os, Sfarsit(an, luna))
            join o in Cub.Citiri.Partide.Origini(os)
                on new { s.UnitateId, s.ContId, s.PartenerId } equals new { o.UnitateId, o.ContId, o.PartenerId } into origine
            from o in origine.DefaultIfEmpty()
            where s.Debit != s.Credit
            select new SnapshotPartida { UnitateId = s.UnitateId, ContId = s.ContId,
                PartenerId = s.PartenerId, Deschisa = s.Deschisa,
                DocumentId = o.DocumentId, Debit = s.Debit, Credit = s.Credit }, parametrul);

    /// <summary>Perioada are deja snapshot scris?</summary>
    public static bool AreSnapshot(IObjectSpace os, int an, int luna) =>
        AreRanduri(os, Contabil, an, luna) || AreRanduri(os, Stoc, an, luna)
        || AreRanduri(os, Partide, an, luna);

    // F27-D3: ștergere FIZICĂ, nu `GCRecord`. Snapshot-ul nu e nomenclator și
    // n-are urmă de păstrat — e o proiecție rescrisă din registre, iar un rând
    // „șters logic" ar rupe unicitatea cheii la următoarea materializare.
    /// <summary>Șterge snapshot-ul perioadei, pe ambele tabele.</summary>
    public static void Elimina(IObjectSpace os, int an, int luna) {
        Executa(os, $"DELETE FROM {Contabil} WHERE \"An\" = {{0}} AND \"Luna\" = {{1}}", an, luna);
        Executa(os, $"DELETE FROM {Stoc} WHERE \"An\" = {{0}} AND \"Luna\" = {{1}}", an, luna);
        EliminaPartide(os, an, luna);
    }

    static void EliminaPartide(IObjectSpace os, int an, int luna) =>
        Executa(os, $"DELETE FROM {Partide} WHERE \"An\" = {{0}} AND \"Luna\" = {{1}}", an, luna);

    // Prima instrucțiune a comenzii, ca `PerioadaService.Blocheaza`: fără ea o
    // închidere care comite după citirea referințelor ar rămâne fără snapshot —
    // pasul final de mai jos îl șterge ca „perioadă care nu e referință”.
    // Subiectul e LANȚUL ÎNTREG, nu o lună: reconstrucția atinge toate
    // referințele, iar ea e rară.
    static void BlocheazaLantul(IObjectSpace os) {
        const string sql = """
            SELECT "ID" AS "Value"
            FROM "PerioadeFiscale"
            WHERE "GCRecord" = 0
            FOR UPDATE
            """;
        Interogheaza<Guid>(os, sql);
    }

    /// <summary>Recalculează integral fiecare referință, RAPORTEAZĂ diferențele, apoi rescrie.</summary>
    public static RaportReconstructie Reconstruieste(IObjectSpace os) {
        BlocheazaLantul(os);
        var referinte = Referinte(os);
        var randuri = new List<RandReconstructie>();
        foreach (var (an, luna) in referinte) {
            var contabil = DiferenteContabil(os, an, luna);
            var stoc = DiferenteStoc(os, an, luna);
            var partide = DiferentePartide(os, an, luna);
            randuri.Add(new RandReconstructie(an, luna,
                contabil.Existente, contabil.Recalculate, contabil.Diferite,
                stoc.Existente, stoc.Recalculate, stoc.Diferite,
                contabil.DiferentaDebit, contabil.DiferentaCredit,
                stoc.DiferentaCantitate, stoc.DiferentaValoare,
                partide.Existente, partide.Recalculate, partide.Diferite, partide.DiferentaRest));
        }
        // Rescrierea vine DUPĂ raport (35b): diferența se constată pe ce era în
        // bază, nu pe ce urmează să scriem.
        foreach (var (an, luna) in referinte) {
            Elimina(os, an, luna);
            ScrieContabil(os, an, luna, null);
            ScrieStoc(os, an, luna, null);
            MaterializeazaPartide(os, an, luna);
        }
        foreach (var p in PerioadeCuSnapshot(os).Where(p => !referinte.Contains(p)))
            Elimina(os, p.An, p.Luna);
        return new RaportReconstructie(randuri);
    }

    // ═══════════════════ citirea ═══════════════════

    /// <summary>Ultima perioadă DE REFERINȚĂ al cărei sfârșit e `&lt;= panaLa`; null = citire integrală din registre.</summary>
    public static (int An, int Luna, DateOnly Sfarsit)? Referinta(IObjectSpace os, DateOnly panaLa) {
        (int An, int Luna, DateOnly Sfarsit)? gasita = null;
        foreach (var (an, luna) in Referinte(os)) {
            var sfarsit = Sfarsit(an, luna);
            if (sfarsit <= panaLa && (gasita == null || sfarsit > gasita.Value.Sfarsit))
                gasita = (an, luna, sfarsit);
        }
        return gasita;
    }

    static bool SnapshotContabilDinCub(IObjectSpace os, int an, int luna) {
        var surse = os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == an && s.Luna == luna)
            .Select(s => s.DinCub).Distinct().ToArray();
        if (surse.Contains(false))
            throw new OperareException($"CITIRE_SNAPSHOT_VECHI: {luna:D2}/{an}; reconstruiți soldurile din cub înaintea citirii.");
        return surse.Length != 0;
    }

    /// <summary>Atomii contabili până la `panaLa`, porniți de la ultima referință care se termină până la `granita`.</summary>
    public static IQueryable<AtomContabil> AtomiCumulati(IObjectSpace os, DateOnly panaLa, DateOnly? granita = null) {
        var atomi = ContabilProiectii.Atomi(os);
        if (os is ISecuredObjectSpace || Referinta(os, granita ?? panaLa) is not { } r)
            return atomi.Where(a => a.Data <= panaLa);
        var (an, luna, sfarsit) = r;
        if (!SnapshotContabilDinCub(os, an, luna))
            return atomi.Where(a => a.Data <= panaLa);
        return os.GetObjectsQuery<SoldPerioadaContabil>().IgnoreAutoIncludes()
            .Where(s => s.An == an && s.Luna == luna)
            .Select(s => new AtomContabil {
                Data = sfarsit,
                ContId = s.ContId,
                Debit = s.Debit,
                Credit = s.Credit,
                RepartitorId = s.RepartitorId,
                GestiuneId = s.GestiuneId,
                MaterialId = s.MaterialId,
                CodFunctionalId = s.CodFunctionalId,
                CodEconomicId = s.CodEconomicId,
                SursaFinantareId = s.SursaFinantareId,
                UnitateId = s.UnitateId,
                ProiectId = s.ProiectId,
                CentruCostId = s.CentruCostId
            })
            .Concat(atomi.Where(a => a.Data > sfarsit && a.Data <= panaLa));
    }

    // Clasă cu setteri, proiectată prin inițializator de obiect, ca
    // `AtomContabil`: peste o proiecție de CONSTRUCTOR, EF nu mai vede membrii,
    // iar orice `Where` de deasupra cade în evaluare pe client.
    /// <summary>O mișcare de stoc cumulată: rândul sintetic al referinței sau un rând de registru de după ea.</summary>
    public sealed class MiscareCumulata {
        public Guid Id { get; set; }
        public Guid LotId { get; set; }
        public Guid RepartitorId { get; set; }
        public TipStoc TipStoc { get; set; }
        public DateOnly Data { get; set; }
        public decimal Cantitate { get; set; }
        public decimal Valoare { get; set; }
    }

    // Oglinda de stoc a lui `AtomiCumulati`, pe cheia registrului. `panaLa` null
    // = „azi/tot". Cele două filtre opționale se aplică PER RAMURĂ, fiindcă n-au
    // aceeași semnificație pe amândouă: rândurile unui document nu pot fi
    // excluse din snapshot (un document cu rânduri în perioadă închisă nu se mai
    // poate anula), iar produsul trăiește pe navigația `Lot`, absentă din
    // proiecție.
    /// <summary>Mișcările de stoc până la `panaLa`, pornite de la ultima referință care se termină până la `granita`.</summary>
    public static IQueryable<MiscareCumulata> MiscariCumulate(IObjectSpace os, DateOnly? panaLa,
            DateOnly? granita = null, Guid? faraDocumentId = null, Guid? produsId = null) {
        var registru = os.GetObjectsQuery<RegistruStoc>().IgnoreAutoIncludes();
        if (panaLa is { } pl)
            registru = registru.Where(r => r.Data <= pl);
        if (faraDocumentId is { } docId)
            registru = registru.Where(r => r.DocumentId != docId);
        if (produsId is { } pid)
            registru = registru.Where(r => r.Lot.ProdusId == pid);
        IQueryable<MiscareCumulata> Proiecteaza(IQueryable<RegistruStoc> sursa) =>
            sursa.Select(r => new MiscareCumulata {
                Id = r.ID, LotId = r.LotId, RepartitorId = r.RepartitorId, TipStoc = r.TipStoc,
                Data = r.Data, Cantitate = r.Cantitate, Valoare = r.Valoare
            });
        if (Referinta(os, granita ?? panaLa ?? DateOnly.MaxValue) is not { } r0)
            return Proiecteaza(registru);
        var (an, luna, sfarsit) = r0;
        var snapshot = os.GetObjectsQuery<SoldPerioadaStoc>().IgnoreAutoIncludes()
            .Where(s => s.An == an && s.Luna == luna);
        if (produsId is { } pidSnap)
            snapshot = snapshot.Where(s => s.Lot.ProdusId == pidSnap);
        return snapshot
            .Select(s => new MiscareCumulata {
                Id = s.ID, LotId = s.LotId, RepartitorId = s.RepartitorId, TipStoc = s.TipStoc,
                Data = sfarsit, Cantitate = s.Cantitate, Valoare = s.Valoare
            })
            .Concat(Proiecteaza(registru.Where(r => r.Data > sfarsit)));
    }

    // ═══════════════════ scrierea ═══════════════════

    static void ScrieContabil(IObjectSpace os, int an, int luna, (int An, int Luna)? precedenta) {
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {Contabil} (\"ID\", \"GCRecord\", \"OptimisticLockField\", \"An\", \"Luna\", \"ContId\", ");
        sb.Append(string.Join(", ", Dimensiuni.Select(d => $"\"{d}\"")));
        sb.Append(", \"Debit\", \"Credit\", \"DinCub\")\n");
        sb.Append($"SELECT gen_random_uuid(), 0, 0, {P(an)}, {P(luna)}, k.\"ContId\", ");
        sb.Append(string.Join(", ", Dimensiuni.Select(d => $"k.\"{d}\"")));
        sb.Append(", SUM(k.\"Debit\"), SUM(k.\"Credit\"), true\n");
        sb.Append($"FROM (\n{SursaContabil(os, P, an, luna, precedenta)}\n) k\n");
        sb.Append("GROUP BY k.\"ContId\", ");
        sb.Append(string.Join(", ", Dimensiuni.Select(d => $"k.\"{d}\"")));
        // Cheile integral zero se OMIT: „absentă” și „zero” sunt același răspuns
        // pentru consumator, iar pe stoc taie ~93 % din rânduri (spike B.4).
        sb.Append("\nHAVING SUM(k.\"Debit\") <> 0 OR SUM(k.\"Credit\") <> 0");
        Executa(os, sb.ToString(), argumente.ToArray());
    }

    static void ScrieStoc(IObjectSpace os, int an, int luna, (int An, int Luna)? precedenta) {
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var sb = new StringBuilder();
        sb.Append($"INSERT INTO {Stoc} (\"ID\", \"GCRecord\", \"OptimisticLockField\", \"An\", \"Luna\", "
            + "\"LotId\", \"RepartitorId\", \"TipStoc\", \"Cantitate\", \"Valoare\")\n");
        sb.Append($"SELECT gen_random_uuid(), 0, 0, {P(an)}, {P(luna)}, "
            + "k.\"LotId\", k.\"RepartitorId\", k.\"TipStoc\", SUM(k.\"Cantitate\"), SUM(k.\"Valoare\")\n");
        sb.Append($"FROM (\n{SursaStoc(P, an, luna, precedenta)}\n) k\n");
        sb.Append("GROUP BY k.\"LotId\", k.\"RepartitorId\", k.\"TipStoc\"\n");
        sb.Append("HAVING SUM(k.\"Cantitate\") <> 0 OR SUM(k.\"Valoare\") <> 0");
        Executa(os, sb.ToString(), argumente.ToArray());
    }

    static string SursaContabil(IObjectSpace os, Func<object, string> P, int an, int luna, (int An, int Luna)? precedenta) {
        var sfarsit = Sfarsit(an, luna);
        var atomi = ContabilProiectii.Atomi(os).Where(a => a.Data <= sfarsit);
        if (precedenta != null) {
            var inceput = Inceput(an, luna);
            atomi = atomi.Where(a => a.Data >= inceput);
        }
        var sursa = SqlInterogare.Compune(atomi.Select(a => new AtomContabil {
            ContId = a.ContId, RepartitorId = a.RepartitorId, GestiuneId = a.GestiuneId,
            MaterialId = a.MaterialId, CodFunctionalId = a.CodFunctionalId, CodEconomicId = a.CodEconomicId,
            SursaFinantareId = a.SursaFinantareId, UnitateId = a.UnitateId, ProiectId = a.ProiectId,
            CentruCostId = a.CentruCostId, Debit = a.Debit, Credit = a.Credit
        }), P);
        if (precedenta is not { } prec) return sursa;
        var dim = string.Join(", ", Dimensiuni.Select(d => $"\"{d}\""));
        return $"SELECT \"ContId\", {dim}, \"Debit\", \"Credit\" FROM {Contabil} "
            + $"WHERE \"An\" = {P(prec.An)} AND \"Luna\" = {P(prec.Luna)}\nUNION ALL\n{sursa}";
    }

    static string SursaStoc(Func<object, string> P, int an, int luna, (int An, int Luna)? precedenta) {
        var sb = new StringBuilder();
        if (precedenta is { } prec) {
            sb.Append("SELECT \"LotId\", \"RepartitorId\", \"TipStoc\", \"Cantitate\", \"Valoare\" "
                + $"FROM {Stoc} WHERE \"An\" = {P(prec.An)} AND \"Luna\" = {P(prec.Luna)}\n");
            sb.Append("UNION ALL\n");
        }
        sb.Append("SELECT r.\"LotId\", r.\"RepartitorId\", r.\"TipStoc\", r.\"Cantitate\", r.\"Valoare\"\n"
            + "FROM \"RegistruStoc\" r\nWHERE r.\"GCRecord\" = 0");
        if (precedenta != null)
            sb.Append($" AND r.\"Data\" >= {P(Inceput(an, luna))}");
        sb.Append($" AND r.\"Data\" <= {P(Sfarsit(an, luna))}");
        return sb.ToString();
    }

    // ═══════════════════ reconstrucția ═══════════════════

    static DiferentaContabilSql DiferenteContabil(IObjectSpace os, int an, int luna) {
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var dim = string.Join(", ", Dimensiuni.Select(d => $"\"{d}\""));
        var kdim = string.Join(", ", Dimensiuni.Select(d => $"k.\"{d}\""));
        var potrivire = string.Join("\n     AND ", Dimensiuni.Select(d =>
            $"COALESCE(e.\"{d}\", {Nil}) = COALESCE(r.\"{d}\", {Nil})"));
        var sql = $"""
            WITH recalc AS (
              SELECT k."ContId", {kdim}, SUM(k."Debit") AS "Debit", SUM(k."Credit") AS "Credit"
              FROM (
            {SursaContabil(os, P, an, luna, null)}
              ) k
              GROUP BY k."ContId", {kdim}
              HAVING SUM(k."Debit") <> 0 OR SUM(k."Credit") <> 0
            ),
            existent AS (
              SELECT "ContId", {dim}, "Debit", "Credit" FROM {Contabil}
              WHERE "An" = {P(an)} AND "Luna" = {P(luna)}
            ),
            j AS (
              SELECT e."ContId" AS "ContIdE", r."ContId" AS "ContIdR",
                     e."Debit" AS "DebitE", e."Credit" AS "CreditE",
                     r."Debit" AS "DebitR", r."Credit" AS "CreditR"
              FROM existent e FULL OUTER JOIN recalc r
                ON e."ContId" = r."ContId"
               AND {potrivire}
            )
            SELECT (SELECT COUNT(*) FROM existent) AS "Existente",
                   (SELECT COUNT(*) FROM recalc) AS "Recalculate",
                   COUNT(*) FILTER (WHERE "ContIdE" IS NULL OR "ContIdR" IS NULL
                                       OR "DebitE" <> "DebitR" OR "CreditE" <> "CreditR") AS "Diferite",
                   COALESCE(SUM(ABS(COALESCE("DebitR", 0) - COALESCE("DebitE", 0))), 0) AS "DiferentaDebit",
                   COALESCE(SUM(ABS(COALESCE("CreditR", 0) - COALESCE("CreditE", 0))), 0) AS "DiferentaCredit"
            FROM j
            """;
        return Interogheaza<DiferentaContabilSql>(os, sql, argumente.ToArray()).Single();
    }

    static DiferentaStocSql DiferenteStoc(IObjectSpace os, int an, int luna) {
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var sql = $"""
            WITH recalc AS (
              SELECT k."LotId", k."RepartitorId", k."TipStoc",
                     SUM(k."Cantitate") AS "Cantitate", SUM(k."Valoare") AS "Valoare"
              FROM (
            {SursaStoc(P, an, luna, null)}
              ) k
              GROUP BY k."LotId", k."RepartitorId", k."TipStoc"
              HAVING SUM(k."Cantitate") <> 0 OR SUM(k."Valoare") <> 0
            ),
            existent AS (
              SELECT "LotId", "RepartitorId", "TipStoc", "Cantitate", "Valoare" FROM {Stoc}
              WHERE "An" = {P(an)} AND "Luna" = {P(luna)}
            ),
            j AS (
              SELECT e."LotId" AS "LotIdE", r."LotId" AS "LotIdR",
                     e."Cantitate" AS "CantitateE", e."Valoare" AS "ValoareE",
                     r."Cantitate" AS "CantitateR", r."Valoare" AS "ValoareR"
              FROM existent e FULL OUTER JOIN recalc r
                ON e."LotId" = r."LotId" AND e."RepartitorId" = r."RepartitorId"
               AND e."TipStoc" = r."TipStoc"
            )
            SELECT (SELECT COUNT(*) FROM existent) AS "Existente",
                   (SELECT COUNT(*) FROM recalc) AS "Recalculate",
                   COUNT(*) FILTER (WHERE "LotIdE" IS NULL OR "LotIdR" IS NULL
                                       OR "CantitateE" <> "CantitateR" OR "ValoareE" <> "ValoareR") AS "Diferite",
                   COALESCE(SUM(ABS(COALESCE("CantitateR", 0) - COALESCE("CantitateE", 0))), 0) AS "DiferentaCantitate",
                   COALESCE(SUM(ABS(COALESCE("ValoareR", 0) - COALESCE("ValoareE", 0))), 0) AS "DiferentaValoare"
            FROM j
            """;
        return Interogheaza<DiferentaStocSql>(os, sql, argumente.ToArray()).Single();
    }

    static DiferentaPartideSql DiferentePartide(IObjectSpace os, int an, int luna) {
        var argumente = new List<object>();
        string P(object v) { argumente.Add(v); return "{" + (argumente.Count - 1) + "}"; }
        var sursa = SursaPartide(os, P, an, luna);
        var sql = $"""
            WITH recalc AS ({sursa}), existent AS (
                SELECT * FROM {Partide} WHERE "An" = {P(an)} AND "Luna" = {P(luna)}
            ), j AS (
                SELECT e."ID", r."UnitateId", e."DinCub", e."Deschisa" AS "DataE", r."Deschisa" AS "DataR",
                    e."DocumentId" AS "DocE", r."DocumentId" AS "DocR",
                    e."Debit" AS "DE", e."Credit" AS "CE", e."Rest" AS "RE",
                    r."Debit" AS "DR", r."Credit" AS "CR"
                FROM existent e FULL OUTER JOIN recalc r
                    ON e."UnitateId" = r."UnitateId" AND e."ContId" = r."ContId" AND e."PartenerId" = r."PartenerId"
            )
            SELECT (SELECT COUNT(*) FROM existent) AS "Existente", (SELECT COUNT(*) FROM recalc) AS "Recalculate",
                COUNT(*) FILTER (WHERE "ID" IS NULL OR "UnitateId" IS NULL OR NOT "DinCub"
                    OR "DE" <> "DR" OR "CE" <> "CR" OR "RE" <> ABS("DR" - "CR")
                    OR "DataE" <> "DataR" OR "DocE" IS DISTINCT FROM "DocR") AS "Diferite",
                COALESCE(SUM(ABS(COALESCE(ABS("DR" - "CR"), 0) - COALESCE("RE", 0))), 0) AS "DiferentaRest"
            FROM j
            """;
        return Interogheaza<DiferentaPartideSql>(os, sql, argumente.ToArray()).Single();
    }

    static IReadOnlyList<(int An, int Luna)> PerioadeCuSnapshot(IObjectSpace os) {
        const string sql = $"""
            SELECT "An", "Luna" FROM {Contabil} GROUP BY "An", "Luna"
            UNION
            SELECT "An", "Luna" FROM {Stoc} GROUP BY "An", "Luna"
            UNION
            SELECT "An", "Luna" FROM {Partide} GROUP BY "An", "Luna"
            """;
        return Interogheaza<PerioadaSnapshotSql>(os, sql).Select(p => (p.An, p.Luna)).ToList();
    }

    // ═══════════════════ primitivele ═══════════════════

    static bool AreRanduri(IObjectSpace os, string tabela, int an, int luna) =>
        Interogheaza<long>(os, $"SELECT COUNT(*) AS \"Value\" FROM {tabela} WHERE \"An\" = {{0}} AND \"Luna\" = {{1}}",
            an, luna).Single() > 0;

    static void Executa(IObjectSpace os, string sql, params object[] argumente) =>
        Db(os).ExecuteSql(FormattableStringFactory.Create(sql, argumente));

    static IReadOnlyList<T> Interogheaza<T>(IObjectSpace os, string sql, params object[] argumente) =>
        Db(os).SqlQuery<T>(FormattableStringFactory.Create(sql, argumente)).ToList();

    static DatabaseFacade Db(IObjectSpace os) {
        if (os is not EFCoreObjectSpace efCore)
            throw new InvalidOperationException(
                $"Soldurile de perioadă cer un ObjectSpace EF Core; „{os?.GetType().Name ?? "null"}” nu expune `DbContext`.");
        return efCore.DbContext.Database;
    }

    static (int An, int Luna) Precedenta(int an, int luna) => luna == 1 ? (an - 1, 12) : (an, luna - 1);

    static DateOnly Inceput(int an, int luna) => new(an, luna, 1);

    static DateOnly Sfarsit(int an, int luna) => new(an, luna, DateTime.DaysInMonth(an, luna));
}
