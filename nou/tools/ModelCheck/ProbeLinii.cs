using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.UI;
using Atlas.DXF.Core.Views.Discovery;
using DevExpress.ExpressApp.Model;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Probele axei 2 a formatului (106h): grilele de linii pe roluri, pe modelul real.
static class ProbeLinii {
    public static void Verifica(IModelApplication model, string eticheta, Action<string, bool> check) {
        if (model == null) {
            check($"106h ({eticheta}) modelul aplicației lipsește — probele liniilor nu pot rula", false);
            return;
        }
        var registry = new UiBaselineRegistry();
        foreach (var provider in UiBaselineDiscovery.Discover(typeof(Document).Assembly))
            provider.Register(registry);

        var grile = typeof(Document).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && typeof(Document).IsAssignableFrom(t))
            .Select(t => (Doc: t, Det: t.GetCustomAttribute<TipDetaliuAttribute>(inherit: false)?.TipDetaliu))
            .Where(x => x.Det != null && x.Det != typeof(DocumentDetaliu))
            .Select(x => (x.Doc, x.Det,
                Lv: model.Views[x.Det.Name + "_ListView"] as IModelListView,
                Dv: model.Views[x.Det.Name + "_DetailView"] as IModelDetailView))
            .GroupBy(x => x.Det).Select(g => g.First())
            .OrderBy(x => x.Det.Name)
            .ToList();

        var abateriOrdine = new List<string>();
        var masuratOrdine = new List<string>();
        foreach (var g in grile) {
            var roluri = registry.ColumnsFor(g.Det);
            if (g.Lv == null || roluri == null) {
                abateriOrdine.Add($"{g.Det.Name}: {(g.Lv == null ? "fără grilă" : "fără roluri")}");
                continue;
            }
            var vizibile = g.Lv.Columns.Where(c => c.Index is null or >= 0)
                .OrderBy(c => c.Index ?? int.MaxValue).Select(c => c.PropertyName).ToList();
            var asteptate = roluri.Placed.Where(m => g.Lv.Columns.Any(c => c.PropertyName == m)).ToList();
            if (!vizibile.Take(asteptate.Count).SequenceEqual(asteptate))
                abateriOrdine.Add($"{g.Det.Name}: [{string.Join(",", vizibile)}] nu începe cu [{string.Join(",", asteptate)}]");
            foreach (var ascuns in roluri.Hidden)
                if (g.Lv.Columns.FirstOrDefault(c => c.PropertyName == ascuns) is { } col && col.Index != -1)
                    abateriOrdine.Add($"{g.Det.Name}.{ascuns}: rol nepurtat, dar vizibil (Index={col.Index})");
            masuratOrdine.Add($"{g.Det.Name}={string.Join(",", vizibile)}");
        }
        Console.WriteLine($"     MĂSURAT (106h-1/{eticheta}): {string.Join("; ", masuratOrdine)}; abateri [{string.Join("; ", abateriOrdine)}].");
        check($"106h-1 ({eticheta}) fiecare grilă tipizată de linii începe cu coloanele rolurilor, în ordinea vocabularului, "
            + "iar rolurile nepurtate de tip sunt ascunse",
            grile.Count >= 10 && abateriOrdine.Count == 0);

        var abateriBlocaj = new List<string>();
        var blocate = new List<string>();
        foreach (var g in grile) {
            foreach (var membru in registry.ReadOnlyMembersFor(g.Det)) {
                var col = g.Lv?.Columns.FirstOrDefault(c => c.PropertyName == membru);
                var item = g.Dv?.Items[membru] as IModelCommonMemberViewItem;
                if (col == null || item == null)
                    abateriBlocaj.Add($"{g.Det.Name}.{membru}: lipsește de pe {(col == null ? "grilă" : "dialog")}");
                else if (col.AllowEdit || item.AllowEdit)
                    abateriBlocaj.Add($"{g.Det.Name}.{membru}: grilă={col.AllowEdit}, dialog={item.AllowEdit}");
                else
                    blocate.Add($"{g.Det.Name}.{membru}");
            }
        }
        Console.WriteLine($"     MĂSURAT (106h-2/{eticheta}): blocate [{string.Join(", ", blocate)}]; abateri [{string.Join("; ", abateriBlocaj)}].");
        check($"106h-2 ({eticheta}) fiecare membru declarat `ReadOnly` e blocat pe grilă ȘI pe dialogul liniei",
            blocate.Count >= 8 && abateriBlocaj.Count == 0);

        var abateriRezultat = new List<string>();
        var rezultate = new List<string>();
        var peGrilaGenerica = new List<string>();
        foreach (var tip in typeof(Document).Assembly.GetTypes()
                     .Where(t => !t.IsAbstract && typeof(Document).IsAssignableFrom(t))
                     .OrderBy(t => t.Name)) {
            var document = (Document)Activator.CreateInstance(tip)!;
            var calculeaza = tip.GetMethod(nameof(Document.BazaLinie))!.DeclaringType != typeof(Document)
                && !document.IntrariBaza().Contains(nameof(DocumentDetaliu.Valoare));
            if (!calculeaza)
                continue;
            var det = tip.GetCustomAttribute<TipDetaliuAttribute>(inherit: false)?.TipDetaliu;
            if (det == null || det == typeof(DocumentDetaliu)) {
                peGrilaGenerica.Add(tip.Name);
                continue;
            }
            if (registry.ReadOnlyMembersFor(det).Contains(nameof(DocumentDetaliu.Valoare)))
                rezultate.Add($"{tip.Name}/{det.Name}");
            else
                abateriRezultat.Add($"{tip.Name}/{det.Name}");
        }
        Console.WriteLine($"     MĂSURAT (106h-3/{eticheta}): `Valoare` calculată și blocată pe [{string.Join(", ", rezultate)}]; "
            + $"neblocată [{string.Join(", ", abateriRezultat)}]; pe grila generică, fără blocaj (106-r6): [{string.Join(", ", peGrilaGenerica)}].");
        check($"106h-3 ({eticheta}) pe fiecare tip cu detaliu propriu care calculează `Valoare` (BazaLinie suprascris, "
            + "`Valoare` nu e intrare), `Valoare` e declarată `ReadOnly`",
            rezultate.Count >= 6 && abateriRezultat.Count == 0);
    }
}
