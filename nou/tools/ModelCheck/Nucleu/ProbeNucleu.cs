#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Proba feliei 30 (B-D7): declarantul frunzei, pus lângă motorul vechi pe
/// ACELEAȘI documente operate, produce exact postările pe care oracolul le
/// scoate din registre, normalizate cu lista închisă B-D8.
/// </summary>
static class ProbeNucleu {
    /// <summary>
    /// Invariantul VI: operandul se citește pe seturi, nu per linie. Pragul e un
    /// NUMĂR DE TABELE, constant în numărul de linii ale documentului (MINOR-7).
    /// </summary>
    public const int PragInterogari = 16;

    public static void Proba(
            IObjectSpace os,
            Action<string, bool> check,
            string prefix,
            IReadOnlyList<Document> documente) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(check);
        ArgumentNullException.ThrowIfNull(documente);
        foreach (var doc in documente) {
            if (doc.Declarant() is null)
                continue;
            var eticheta = $"{prefix} {doc.GetType().Name} {doc.Numar ?? doc.ID.ToString()[..8]}";

            NumaratorSql.Instanta.Reseteaza();
            var contract = Contractare.Contracteaza(os, doc);
            var interogari = NumaratorSql.Instanta.Numar;

            if (!contract.EsteAcceptat)
                foreach (var refuz in contract.Refuzuri)
                    Console.WriteLine($"       refuz {refuz.Cod}: {refuz.Mesaj}");
            check($"{eticheta}: contract acceptat", contract.EsteAcceptat);
            if (contract.Tranzactii.Count == 0)
                continue;

            var conservare = contract.Tranzactii.SelectMany(N.Conservare.Verifica).ToList();
            foreach (var refuz in conservare)
                Console.WriteLine($"       conservare {refuz.Cod}: {refuz.Mesaj}");
            check($"{eticheta}: Conservare.Verifica gol", conservare.Count == 0);

            check($"{eticheta}: determinism (două contractări ⇒ contracte egale)",
                Contractare.Contracteaza(os, doc) == contract);

            if (interogari > PragInterogari)
                Console.WriteLine($"       Fapte.Operand: {interogari} interogări, pragul e {PragInterogari}");
            check($"{eticheta}: Fapte.Operand ≤ {PragInterogari} interogări (citire pe seturi)",
                interogari <= PragInterogari);
        }
    }
}
