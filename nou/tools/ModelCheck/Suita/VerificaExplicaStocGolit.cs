using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// D9-D8: tipurile golite de reguli de stoc nu mai potrivesc nimic în „Explică”, iar răspunsul rămâne întreg.
static class VerificaExplicaStocGolit {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        using var os = s.Provider.CreateObjectSpace();
        var bcs = os.FirstOrDefault<TipDocument>(t => t.Cod == "BCS");
        var material = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.01.00"));
        var gestiune = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var reguli = os.GetObjectsQuery<RegulaStoc>().Count(r => r.TipDocumentId == bcs.ID);
        var raspuns = ExplicaApply.Explica(os, new ExplicaCerere(bcs.ID, material.ID, +1, new DateOnly(2026, 5, 5),
            gestiune?.ID, null, null, null));
        Console.WriteLine($"     MĂSURAT (D9-D8-EXPLICA/{eticheta}): BCS × {material.Cod}: {reguli} reguli de stoc în bază; "
            + $"stoc {raspuns.Stoc.Length} laturi — „{raspuns.ConcluzieStoc}”; contare nivel {raspuns.Contare?.Nivel}, "
            + $"numerotare {raspuns.Numerotare?.Serie}.");
        s.Check($"D9-D8-EXPLICA ({eticheta}) „Explică” pe BCS nu mai potrivește nicio regulă de stoc (rândurile au ieșit "
            + "din seed), iar răspunsul rămâne valid: contarea și numerotarea tipului se explică în continuare",
            reguli == 0 && raspuns.Stoc.Length == 0 && raspuns.ConcluzieStoc.Contains("nicio regulă de stoc")
            && raspuns.Contare != null && raspuns.Numerotare?.Serie == "BCS-");
    }
}
