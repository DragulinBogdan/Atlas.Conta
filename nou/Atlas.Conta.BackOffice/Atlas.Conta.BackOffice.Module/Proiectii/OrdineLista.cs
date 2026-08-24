using DevExtreme.AspNet.Data;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// ═══ De ce ordinea unei proiecții TREBUIE declarată aici, nu doar în LINQ ═══
//
// `DataSourceLoader` NU compune sortarea peste `OrderBy`-ul proiecției: când
// cererea nu poartă `sort=` și ARE paginare, biblioteca își inventează singură o
// ordine și o pune ca `OrderBy` (nu `ThenBy`), iar în EF Core `OrderBy`
// ȘTERGE orice ordonare anterioară. Lanțul, verificat pe surse (DevExtreme.AspNet.Data
// 5.1.0, EF Core 10.0.11):
//
//   1. `DataSourceLoadContext.EnsurePrimaryKeyAndDefaultSort` — `PrimaryKey` e gol
//      (`Utils.GetPrimaryKey` cere atributul `[Key]`, pe care DTO-urile de sârmă
//      nu-l au), deci pe provider EF Core cade pe
//      `EFSorting.FindSortableMember(tipulDtoului)`;
//   2. `FindSortableMember` întoarce PRIMUL membru care arată a cheie prin
//      convenția EF „code first": o proprietate numită `Id` (case-insensitive) de
//      tip sortabil — adică exact `Id`-ul rândurilor noastre de raportare;
//   3. `LoadExpressionBuilder.AddSort` compilează sortarea cu
//      `Utils.GetSortMethod(first: true, …)` ⇒ `Queryable.OrderBy`, nu `ThenBy`;
//   4. `SelectExpression.ApplyOrdering` (EF Core), documentat „This overwrites any
//      previous ordering specified", face `_orderings.Clear()`.
//
// Rezultatul: `ORDER BY "Data", "Id", "Sens" DESC` scris în proiecție ajunge la
// Postgres ca `ORDER BY "Id"`. Pe fișa de cont asta e o MINCIUNĂ, nu o
// neplăcere: soldul curent e cumulat de fereastra SQL într-o ordine, iar
// rândurile se afișează în alta — divergența apare exact acolo unde ordinea de
// INSERARE (Id-uri UUIDv7, temporale) diferă de `Data` documentului: operare
// retroactivă, corecții, reimportări.
//
// Fixul e să nu-i lăsăm loc: cine încarcă o proiecție cu ordine SEMNIFICATIVĂ îi
// declară ordinea aici, în forma pe care biblioteca o consumă (`Sort`), deci ea
// ajunge pe nivelul EXTERIOR — deasupra căruia vin `Skip`/`Take` — și paginarea
// taie exact secvența cerută. Ordinea din LINQ rămâne în proiecție pentru
// consumatorii DIRECȚI (ModelCheck, orice `.ToList()`), unde nimeni n-o rescrie.
public static class OrdineLista {
    public static SortingInfo Crescator(string selector) => new() { Selector = selector };
    public static SortingInfo Descrescator(string selector) => new() { Selector = selector, Desc = true };

    // Ordinea declarată a unei proiecții, în două roluri care sunt de fapt unul
    // singur — „ORDER BY-ul care ajunge la Postgres trebuie să fie TOTAL":
    //
    //  • cererea n-are `sort=` ⇒ ordinea declarată devine ordinea, în locul celei
    //    inventate de bibliotecă;
    //  • cererea ARE `sort=` ⇒ sortarea clientului rămâne PRIMARĂ (grila sortează
    //    ce vrea), iar selectorii declarați care nu apar deja în ea se adaugă
    //    DUPĂ, ca tiebreak.
    //
    // A doua ramură nu e un lux (review advers D2): sortarea unei grile se face
    // aproape întotdeauna pe o coloană NE-unică — „Cont" pe balanța analitică,
    // orice coloană a jurnalului — iar `ORDER BY` ne-unic sub `LIMIT/OFFSET` n-are
    // ordine garantată: același rând poate apărea pe două pagini sau pe niciuna.
    // Dedupe pe SELECTOR, nu pe (selector, direcție): dacă clientul a sortat
    // descrescător pe o coloană din cheie, a o re-adăuga crescător ar fi inertă în
    // cel mai bun caz și derutantă în cel mai rău.
    //
    // Proiecțiile cu ordine NEnegociabilă (fișa de cont) își golesc întâi `Sort`-ul
    // clientului — decizia aia trăiește în controllerul lor, aici nu se presupune
    // nimic despre ea.
    public static void AplicaOrdineImplicita(DataSourceLoadOptionsBase optiuni, params SortingInfo[] ordine) {
        if (optiuni == null || ordine == null || ordine.Length == 0)
            return;
        if (optiuni.Sort is not { Length: > 0 } cerutaDeClient) {
            optiuni.Sort = ordine;
            return;
        }
        var deja = new HashSet<string>(
            cerutaDeClient.Where(s => s?.Selector != null).Select(s => s.Selector),
            StringComparer.OrdinalIgnoreCase);
        var tiebreak = ordine.Where(s => s?.Selector != null && deja.Add(s.Selector)).ToArray();
        if (tiebreak.Length > 0)
            optiuni.Sort = cerutaDeClient.Concat(tiebreak).ToArray();
    }
}
