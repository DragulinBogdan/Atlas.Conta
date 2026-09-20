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

    /// <param name="conexe">documentul conex autogenerat → documentul sursă (TR-D3).</param>
    public static void Proba(
            IObjectSpace os,
            Action<string, bool> check,
            string prefix,
            IReadOnlyList<Document> documente,
            IReadOnlyDictionary<Guid, Guid>? conexe = null) {
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
            if (contract.Tranzactie is not { } tranzactie)
                continue;

            var aleLui = new List<Guid> { doc.ID };
            var conexeAleLui = new Dictionary<Guid, Guid>();
            if (conexe is not null)
                foreach (var (conex, sursa) in conexe)
                    if (sursa == doc.ID) {
                        aleLui.Add(conex);
                        conexeAleLui[conex] = sursa;
                    }

            Normalizari.Reseteaza();
            var oracol = Normalizari.Toate(
                CubDinRegistre.Transforma(os, aleLui),
                Normalizari.Citeste(os, aleLui, conexeAleLui));
            check($"{eticheta}: normalizările B-D8 fără reziduu", Normalizari.Avertismente.Count == 0);

            var raport = Comparabil.Compara(
                Comparabil.Proiecteaza(oracol),
                Comparabil.Proiecteaza(tranzactie),
                Nume(os, oracol, tranzactie));
            if (!raport.Egal)
                Console.WriteLine(raport.ToString());
            check($"{eticheta}: postările = registrele normalizate", raport.Egal);

            var conservare = N.Conservare.Verifica(tranzactie);
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

    /// <summary>Simbolurile conturilor atinse, ca diff-ul să se poată citi fără bază.</summary>
    public static Func<Guid, string> Nume(
            IObjectSpace os, IEnumerable<N.Tranzactie> oracol, params N.Tranzactie[] obtinut) {
        var ids = oracol.Concat(obtinut)
            .SelectMany(t => t.Postari)
            .Select(p => p.Coordonate.Cont)
            .Distinct()
            .ToList();
        var simboluri = ids.Count == 0
            ? []
            : os.GetObjectsQuery<Cont>()
                .Where(c => ids.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol })
                .ToList()
                .ToDictionary(c => c.ID, c => c.Simbol);
        return id => simboluri.GetValueOrDefault(id) ?? id.ToString()[..8];
    }
}
