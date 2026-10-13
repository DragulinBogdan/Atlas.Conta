using Npgsql;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static partial class ScaraVolum {
    static void PregatesteOriginiD9b(NpgsqlConnection sql) => Executa(sql, """
        create table scara.origine as select * from "OriginiPartide";
        create table scara.initiala (vechi uuid, c integer, nou uuid, referinta uuid, partener uuid,
            primary key (vechi, c));
        """);

    static void CopiazaD9b(NpgsqlConnection sql, int deLa, int panaLa) {
        var initiale = new List<(Guid Vechi, int Copie, Guid Nou, Guid Referinta, Guid Partener)>();
        using (var cmd = Comanda(sql, """
            select o."ID", k.c, o."ContId", scara.h(o."ReferintaInitiala", k.c),
                case when p."ID" is null then o."PartenerId" else scara.h(o."PartenerId", k.c) end
            from scara.origine o left join scara.partener p on p."ID" = o."PartenerId"
            cross join generate_series(@de_la, @pana_la - 1) k(c) where o."DocumentId" is null
            """, ("de_la", deLa), ("pana_la", panaLa)))
        using (var r = cmd.ExecuteReader())
            while (r.Read()) initiale.Add((r.GetGuid(0), r.GetInt32(1),
                N.UnitateSold.DeschidePartidaInitiala(r.GetGuid(2), r.GetGuid(4), r.GetGuid(3), DateOnly.MinValue).Id,
                r.GetGuid(3), r.GetGuid(4)));
        using (var copie = sql.BeginBinaryImport("COPY scara.initiala (vechi, c, nou, referinta, partener) FROM STDIN (FORMAT BINARY)")) {
            foreach (var r in initiale) {
                copie.StartRow(); copie.Write(r.Vechi); copie.Write(r.Copie); copie.Write(r.Nou);
                copie.Write(r.Referinta); copie.Write(r.Partener);
            }
            copie.Complete();
        }
        var text = Sectiuni.Value["copiaza"]
            .Replace("coalesce(o.unitate_noua, scara.h(p.\"Unitate\", k.c))", "coalesce(o.unitate_noua, oi.nou, scara.h(p.\"Unitate\", k.c))")
            .Replace("left join deschidere d on", "left join scara.initiala oi on oi.vechi = p.\"Unitate\" and oi.c = k.c\nleft join deschidere d on");
        Executa(sql, text, ("de_la", deLa), ("pana_la", panaLa));
        Executa(sql, """
            insert into "OriginiPartide" ("ID", "ContId", "PartenerId", "DocumentId", "ReferintaInitiala", "OrdinalInitial")
            select case when o."DocumentId" is null then i.nou else cub_partida_id(scara.h(o."DocumentId", k.c), o."ContId",
                    case when p."ID" is null then o."PartenerId" else scara.h(o."PartenerId", k.c) end) end,
                o."ContId", case when p."ID" is null then o."PartenerId" else scara.h(o."PartenerId", k.c) end,
                scara.h(o."DocumentId", k.c), i.referinta,
                o."OrdinalInitial" + k.c * (select count(*)::integer from scara.postare p
                    join scara.tranzactie t on t."ID" = p."TranzactieId" where t."Fel" = 4)
            from scara.origine o cross join generate_series(@de_la, @pana_la - 1) k(c)
            left join scara.partener p on p."ID" = o."PartenerId"
            left join scara.initiala i on i.vechi = o."ID" and i.c = k.c;
            """, ("de_la", deLa), ("pana_la", panaLa));
    }
}
