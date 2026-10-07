using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ============ Felia 31 (TR-D7a): schema cubului și ordinea liniilor ============
// S-D2 — forma fizică a lui `Postare` NU e în modelul EF (XAF EF Core n-are chei
// compuse, iar Postgres cere cheia partiției în orice constrângere unică): ea
// trăiește în SQL-ul migrației, deci proba ei e pe CATALOGUL bazei, nu pe model.
static class VerificaSchemaCub {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        using var os = s.Provider.CreateObjectSpace();
        var ctxCub = ((EFCoreObjectSpace)os).DbContext;
        List<string> Valori(FormattableString sql) => ctxCub.Database.SqlQuery<string>(sql).ToList();

        // ---- STR-SCHEMA-1: părintele e partiționat LIST pe `Spatiu`, cu două partiții ----
        // `partattrs` e `int2vector` (indexat de la 0), spre deosebire de `conkey` (`int2[]`).
        var partitionare = Valori($@"
        SELECT (p.partstrat::text || ':' || a.attname::text) AS ""Value""
        FROM pg_partitioned_table p
        JOIN pg_class c ON c.oid = p.partrelid
        JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = p.partattrs[0]
        WHERE c.relname = 'Postare'");
        var partitii = Valori($@"
        SELECT (c.relname::text || ' ' || pg_get_expr(c.relpartbound, c.oid)) AS ""Value""
        FROM pg_class c
        JOIN pg_inherits i ON i.inhrelid = c.oid
        JOIN pg_class parinte ON parinte.oid = i.inhparent
        WHERE parinte.relname = 'Postare'
        ORDER BY c.relname");
        Console.WriteLine($"     MĂSURAT (STR-SCHEMA-1/{eticheta}): partiționare [{string.Join(", ", partitionare)}]; "
            + $"partiții [{string.Join("; ", partitii)}].");
        s.Check($"STR-SCHEMA-1 ({eticheta}) `Postare` e PARTITION BY LIST (\"Spatiu\") cu exact două partiții — "
            + "`Postare_Contabil` FOR VALUES IN (1) și `Postare_Stoc` FOR VALUES IN (2), adică `N.Spatiu` (N-D2)",
            partitionare.SequenceEqual(["l:Spatiu"])
            && partitii.SequenceEqual([
                "Postare_Contabil FOR VALUES IN ('1')", "Postare_Stoc FOR VALUES IN ('2')"]));

        // ---- STR-SCHEMA-2: cheia primară e `(Spatiu, ID)`, în ordinea asta ----
        var cheie = Valori($@"
        SELECT (con.conname::text || ':' || a.attname::text) AS ""Value""
        FROM pg_constraint con
        JOIN pg_class c ON c.oid = con.conrelid
        CROSS JOIN LATERAL unnest(con.conkey) WITH ORDINALITY k(attnum, ord)
        JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = k.attnum
        WHERE c.relname = 'Postare' AND con.contype = 'p'
        ORDER BY k.ord");
        Console.WriteLine($"     MĂSURAT (STR-SCHEMA-2/{eticheta}): PK [{string.Join(", ", cheie)}].");
        s.Check($"STR-SCHEMA-2 ({eticheta}) cheia primară a bazei e `(Spatiu, ID)` — divergență DECLARATĂ față de "
            + "snapshot-ul EF, care poartă doar `ID` (S-r4)",
            cheie.SequenceEqual(["PK_Postare:Spatiu", "PK_Postare:ID"]));

        // ---- STR-SCHEMA-3: FK-urile stau pe PARTIȚII, exact setul din S-D2 ----
        var chei = Valori($@"
        SELECT (c.relname::text || '.' || a.attname::text || '→' || tinta.relname::text || ' [' || con.conname::text || ']') AS ""Value""
        FROM pg_constraint con
        JOIN pg_class c ON c.oid = con.conrelid
        JOIN pg_class tinta ON tinta.oid = con.confrelid
        JOIN pg_attribute a ON a.attrelid = c.oid AND a.attnum = con.conkey[1]
        WHERE con.contype = 'f' AND c.relname IN ('Postare', 'Postare_Contabil', 'Postare_Stoc')
        ORDER BY 1");
        string[] cheiAsteptate = [
            "Postare_Contabil.Cont→Conturi [FK_Postare_Contabil_Conturi_Cont]",
            "Postare_Contabil.DocumentId→Documente [FK_Postare_Contabil_Documente_DocumentId]",
            "Postare_Contabil.Partener→Repartitori [FK_Postare_Contabil_Repartitori_Partener]",
            "Postare_Contabil.Produs→Produse [FK_Postare_Contabil_Produse_Produs]",
            "Postare_Contabil.TranzactieId→Tranzactie [FK_Postare_Contabil_Tranzactie_TranzactieId]",
            "Postare_Stoc.Cont→Conturi [FK_Postare_Stoc_Conturi_Cont]",
            "Postare_Stoc.DocumentId→Documente [FK_Postare_Stoc_Documente_DocumentId]",
            "Postare_Stoc.Partener→Repartitori [FK_Postare_Stoc_Repartitori_Partener]",
            "Postare_Stoc.Produs→Produse [FK_Postare_Stoc_Produse_Produs]",
            "Postare_Stoc.TranzactieId→Tranzactie [FK_Postare_Stoc_Tranzactie_TranzactieId]",
            "Postare_Stoc.Unitate→Loturi [FK_Postare_Stoc_Loturi_Unitate]",
        ];
        Console.WriteLine($"     MĂSURAT (STR-SCHEMA-3/{eticheta}): {chei.Count} FK-uri [{string.Join("; ", chei)}].");
        s.Check($"STR-SCHEMA-3 ({eticheta}) fiecare FK din S-D2 există pe PARTIȚIA lui (părintele partiționat n-are "
            + "niciunul), iar setul e ÎNCHIS: fără FK pe `Gestiune` (gestiunile virtuale sunt id-uri fără rând — S-r3), "
            + "fără FK pe `Unitate` pe Contabil (partida e hash determinist, nu rând) și fără FK pe `Valuta` (B-r6)",
            chei.OrderBy(x => x, StringComparer.Ordinal).SequenceEqual(cheiAsteptate.OrderBy(x => x, StringComparer.Ordinal)));

        // ---- STR-SCHEMA-4: setul minim de indexi (TR-r3 îl re-măsoară la TR-D8) ----
        var indexi = Valori($@"
        SELECT (tablename::text || '.' || indexname::text) AS ""Value""
        FROM pg_indexes WHERE tablename IN ('Postare', 'Postare_Contabil', 'Postare_Stoc', 'Tranzactie')
        ORDER BY 1").ToHashSet(StringComparer.Ordinal);
        string[] indexiAsteptati = [
            "Postare.PK_Postare",
            "Postare.IX_Postare_TranzactieId",
            "Postare.IX_Postare_DocumentId",
            "Postare_Stoc.IX_Postare_Stoc_Produs_Data",
            "Postare_Contabil.IX_Postare_Contabil_Partener_Cont_Data",
            "Postare_Contabil.IX_Postare_Contabil_Data",
            "Postare_Contabil.IX_Postare_Contabil_PerioadaDeclarare_TipTvaId",
            "Tranzactie.IX_Tranzactie_DocumentId",
        ];
        var indexiLipsa = indexiAsteptati.Where(i => !indexi.Contains(i)).ToList();
        // Cei doi indexi PARȚIALI și cel cu `INCLUDE` nu pot sta pe tabela partiționată
        // (Postgres îi refuză pe părinte), deci stau pe partițiile lor — exact S-D2.
        var definitii = Valori($@"
        SELECT indexdef::text AS ""Value"" FROM pg_indexes
        WHERE indexname IN ('IX_Postare_Stoc_Produs_Data', 'IX_Postare_Contabil_Partener_Cont_Data',
                            'IX_Postare_Contabil_PerioadaDeclarare_TipTvaId')
        ORDER BY 1");
        var formeGresite = new List<string>();
        foreach (var d in definitii) {
            if (d.Contains("IX_Postare_Stoc_Produs_Data") && !d.Contains("INCLUDE (\"Cantitate\", \"Valoare\", \"Gestiune\", \"Unitate\")"))
                formeGresite.Add(d);
            if (d.Contains("IX_Postare_Contabil_Partener_Cont_Data") && !d.Contains("WHERE (\"Partener\" IS NOT NULL)"))
                formeGresite.Add(d);
            if (d.Contains("IX_Postare_Contabil_PerioadaDeclarare_TipTvaId") && !d.Contains("WHERE (\"PerioadaDeclarare\" IS NOT NULL)"))
                formeGresite.Add(d);
        }
        Console.WriteLine($"     MĂSURAT (STR-SCHEMA-4/{eticheta}): {indexi.Count} indexi pe cele patru tabele; "
            + $"lipsă [{string.Join(", ", indexiLipsa)}]; formă greșită [{string.Join("; ", formeGresite)}].");
        s.Check($"STR-SCHEMA-4 ({eticheta}) indexii minimi din S-D2 există: PK + `(TranzactieId)`/`(DocumentId)` pe "
            + "părinte, S2 `(Produs, Data) INCLUDE (…)` pe Stoc, C2 `(Partener, Cont, Data) WHERE …`, C3 `(Data)` și "
            + "F1 `(PerioadaDeclarare, TipTvaId) WHERE …` pe Contabil, `Tranzactie(DocumentId)`",
            indexiLipsa.Count == 0 && definitii.Count == 3 && formeGresite.Count == 0);

        // ---- STR-SCHEMA-5: coloanele modelului EF = coloanele bazei, pe ambele entități ----
        var modelCub = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions
            .GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>(ctxCub).Model;
        var abateriCub = new List<string>();
        foreach (var tip in new[] { typeof(Atlas.Conta.BackOffice.Module.Cub.Postare),
                                    typeof(Atlas.Conta.BackOffice.Module.Cub.Tranzactie) }) {
            var entitate = modelCub.FindEntityType(tip);
            if (entitate == null) {
                abateriCub.Add($"{tip.Name}: lipsește din model");
                continue;
            }
            var tabela = entitate.GetTableName();
            var coloaneModel = entitate.GetProperties().Select(p => p.GetColumnName()).ToHashSet(StringComparer.Ordinal);
            var coloaneBaza = Valori($"SELECT column_name::text AS \"Value\" FROM information_schema.columns WHERE table_name = {tabela}")
                .ToHashSet(StringComparer.Ordinal);
            if (!coloaneBaza.SetEquals(coloaneModel))
                abateriCub.Add($"{tabela}: doar în bază [{string.Join(",", coloaneBaza.Except(coloaneModel))}]; "
                    + $"doar în model [{string.Join(",", coloaneModel.Except(coloaneBaza))}]");
            // Fără `OptimisticLockField`: POCO-urile nu sunt `Editabila` (S-D1, 104e).
            if (coloaneBaza.Contains("OptimisticLockField"))
                abateriCub.Add($"{tabela}: are timbru de entitate editabilă");
            if (entitate.GetDeclaredQueryFilters().Any())
                abateriCub.Add($"{tabela}: are filtru global de interogare");
        }
        Console.WriteLine($"     MĂSURAT (STR-SCHEMA-5/{eticheta}): abateri [{string.Join("; ", abateriCub)}].");
        s.Check($"STR-SCHEMA-5 ({eticheta}) coloanele lui `Postare` și `Tranzactie` din bază sunt exact cele ale "
            + "modelului EF, fără `OptimisticLockField` și fără filtru global — cubul e append-only (S-D1)",
            abateriCub.Count == 0);
    }
}
