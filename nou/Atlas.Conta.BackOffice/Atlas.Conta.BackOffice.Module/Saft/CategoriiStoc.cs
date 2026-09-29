using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Saft;

public enum RolCategorieStoc { Raportabila, Nestoc, Neacoperita }

/// <summary>Categoria de stoc a unui cont: prima valoare explicită urcând pe părinți (S3-D1).</summary>
public sealed class CategoriiStoc {
    readonly Dictionary<Guid, (Guid? Parinte, TipStoc? Categorie)> conturi;
    readonly Dictionary<Guid, TipStoc?> rezolvate = [];

    public CategoriiStoc(IObjectSpace os) => conturi = os.GetObjectsQuery<Cont>()
        .Select(c => new { c.ID, c.ParinteId, c.CategorieStoc }).ToList()
        .ToDictionary(c => c.ID, c => (c.ParinteId, c.CategorieStoc));

    public TipStoc? Rezolva(Guid cont) {
        if (rezolvate.TryGetValue(cont, out var gasita)) return gasita;
        var vizitate = new HashSet<Guid>();
        TipStoc? categorie = null;
        for (Guid? c = cont; c is Guid id && vizitate.Add(id) && conturi.TryGetValue(id, out var info); c = info.Parinte)
            if (info.Categorie is TipStoc explicita) { categorie = explicita; break; }
        return rezolvate[cont] = categorie;
    }

    public static RolCategorieStoc Rol(TipStoc categorie) => categorie switch {
        TipStoc.Magazie or TipStoc.Marfuri => RolCategorieStoc.Raportabila,
        TipStoc.Consum => RolCategorieStoc.Nestoc,
        _ => RolCategorieStoc.Neacoperita,
    };
}
