using System.Linq.Expressions;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExtreme.AspNet.Data.Helpers;

namespace Atlas.Conta.BackOffice.Module.Api;

// Căutarea fără diacritice pe PROIECȚII (decizia 78) — perechea lui F20-D1:
// coloana generată `Cautare` acoperă ușa OData a nomenclatoarelor, dar filtrele
// grilelor (FilterRow, căutarea din HeaderFilter) trec prin `DataSourceLoader`
// peste proiecții, unde câmpul căutat e un JOIN (`PredatorDenumire` =
// `Partener.Denumire`) — o coloană persistată acolo ar fi denormalizare cu
// staleness la redenumire, nu o soluție.
//
// În schimb: UN compilator custom, înregistrat o dată per proces
// (`RegisterBinaryExpressionCompiler` e o listă STATICĂ a bibliotecii), care
// rescrie predicatele de STRING — și doar pe ele — în
//     Cautare.FaraDiacritice(camp) operatie literalNormalizat
// unde funcția e tradusă de EF pe exact fragmentul SQL al coloanei generate
// (`BackOfficeEFCoreDbContext.AplicaFunctiaFaraDiacritice`), iar literalul se
// normalizează AICI, în C# — pe partea constantei nu are ce căuta un apel SQL.
// Ambele părți trec prin ACELAȘI tabel De/La (`Comun/Cautare.cs`), deci
// semantica e identică cu cea a lookup-urilor pe `Cautare`.
//
// Ce NU se atinge, deliberat:
//   * `=`/`<>` și restul comparațiilor — lista de valori a `HeaderFilter`
//     trimite valoarea EXACTĂ din listă („=”), iar o egalitate normalizată ar
//     topi valori distincte („Țeavă” == „Teava”) și ar ocoli indexurile;
//   * câmpurile ne-string — cad pe compilarea standard (return null);
//   * un accessor pe care nu-l putem rezolva static — tot pe standard, ca un
//     câmp nou să nu poată rupe filtrarea din cauza rescrierii.
//
// `coalesce(camp, '')` înaintea funcției: sursele materializate în memorie
// (fișa de cont — SQL brut → listă) execută expresia ca LINQ-to-Objects, unde
// `null.Contains` ar arunca; în SQL, `coalesce` e neutru pentru `contains`/
// `startswith`/`endswith` (nulul nu se potrivea nici înainte), iar pe
// `notcontains` face nulul să CONTEZE ca „nu conține” — ce așteaptă operatorul.
public static class CautareFiltru {
    static readonly MethodInfo FaraDiacritice =
        typeof(Cautare).GetMethod(nameof(Cautare.FaraDiacritice), [typeof(string)])!;
    static readonly MethodInfo Contine = typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;
    static readonly MethodInfo Incepe = typeof(string).GetMethod(nameof(string.StartsWith), [typeof(string)])!;
    static readonly MethodInfo Termina = typeof(string).GetMethod(nameof(string.EndsWith), [typeof(string)])!;

    static int inregistrat;

    /// <summary>
    /// Idempotent și thread-safe — lista de compilatoare a bibliotecii e
    /// statică pe proces, iar host-ul (Startup) și ModelCheck o ating amândoi.
    /// </summary>
    public static void Inregistreaza() {
        if (Interlocked.Exchange(ref inregistrat, 1) == 1)
            return;
        CustomFilterCompilers.RegisterBinaryExpressionCompiler(Compileaza);
    }

    static Expression Compileaza(IBinaryExpressionInfo criteriu) {
        var metoda = criteriu.Operation switch {
            "contains" or "notcontains" => Contine,
            "startswith" => Incepe,
            "endswith" => Termina,
            _ => null,
        };
        if (metoda == null)
            return null;

        // Accessorul, static: proprietate cu proprietate pe DTO-ul proiecției
        // (în practică un singur segment — DTO-urile sunt plate prin decizia 6).
        Expression camp = criteriu.DataItemExpression;
        foreach (var segment in (criteriu.AccessorText ?? "").Split('.')) {
            var proprietate = camp.Type.GetProperty(segment);
            if (proprietate == null)
                return null;
            camp = Expression.Property(camp, proprietate);
        }
        if (camp.Type != typeof(string))
            return null;

        var literal = Cautare.Normalizeaza(Convert.ToString(criteriu.Value ?? ""));
        var normalizat = Expression.Call(FaraDiacritice,
            Expression.Coalesce(camp, Expression.Constant("")));
        Expression predicat = Expression.Call(normalizat, metoda, Expression.Constant(literal));
        return criteriu.Operation == "notcontains" ? Expression.Not(predicat) : predicat;
    }
}
