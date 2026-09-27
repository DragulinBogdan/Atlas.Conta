using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed record CalificareTva(RegimTva Regim, decimal Cota, bool DeImport);
public sealed record TipDiagnosticTva(CalificareTva Calificare, DateOnly? DeLa, DateOnly? PanaLa);
public sealed record SursaDiagnosticTva(Guid DocumentId, Guid? PartenerId, SensTva Sens,
    CalificareTva Calificare, DateOnly Exigibilitate, bool AvansPozitiv, bool Compensata);
public sealed record LinieDiagnosticTva(Guid Id, Guid DocumentId, Guid? PartenerId, SensTva Sens,
    CalificareTva Calificare, DateOnly Exigibilitate, decimal Baza, decimal Taxa,
    bool Automata, bool Avans, bool Ajustare, Guid? LinieAvansId, SursaDiagnosticTva Sursa,
    TipDiagnosticTva TipActual);
public sealed record AvertismentTva(Guid LinieId, string Cod, string Motiv);

public static class DiagnosticTva {
    public static IReadOnlyList<AvertismentTva> Verifica(IReadOnlyList<LinieDiagnosticTva> linii,
            MidpointRounding conventie) {
        var rezultat = new List<AvertismentTva>();
        foreach (var grup in linii.GroupBy(l => (l.DocumentId, l.Sens))) {
            var taxa = N.Tva.PeDocument(grup.Select(l => new N.LinieTva(l.Id, l.Baza,
                (N.RegimTva)l.Calificare.Regim, l.Calificare.Cota)).ToArray(),
                grup.Key.Sens == SensTva.Achizitie ? N.DirectieTva.Deductibil : N.DirectieTva.Colectat,
                new N.Rotunjire(conventie));
            foreach (var l in grup) {
                void Adauga(string cod, string motiv) => rezultat.Add(new(l.Id, cod, motiv));
                var calificare = l.Calificare;
                var produceTaxa = calificare.Regim == RegimTva.Normal
                    || calificare.Regim == RegimTva.TaxareInversa && l.Sens == SensTva.Achizitie;
                if (produceTaxa && !l.Automata && Math.Abs(l.Taxa - taxa.PerLinie[l.Id]) > 0.01m)
                    Adauga("TVA_TAXA_DIFERITA_DE_COTA", "Taxa liniei diferă de taxa repartizată la cotă cu mai mult de 0,01 lei.");
                if (l.TipActual != null && calificare != l.TipActual.Calificare)
                    Adauga("TVA_CALIFICARE_MODIFICATA", "Calificarea înghețată diferă de calificarea curentă a tipului.");
                var reper = l.Exigibilitate;
                if (l.Avans && l.Baza < 0m) {
                    if (l.LinieAvansId == null) {
                        Adauga("TVA_AVANS_FARA_REFERINTA", "Regularizarea nu identifică linia avansului.");
                        continue;
                    }
                    var sursa = l.Sursa;
                    if (sursa == null || sursa.DocumentId == l.DocumentId || !sursa.AvansPozitiv
                            || sursa.PartenerId != l.PartenerId || sursa.Sens != l.Sens) {
                        Adauga("TVA_AVANS_REFERINTA_INVALIDA", "Sursa avansului nu poate fi validată pentru această linie.");
                        continue;
                    }
                    if (sursa.Compensata) {
                        Adauga("TVA_AVANS_SURSA_COMPENSATA", "Sursa avansului este compensată; alegeți explicit sursa corectă.");
                        continue;
                    }
                    if (calificare != sursa.Calificare)
                        Adauga("TVA_AVANS_CALIFICARE_DIFERITA", "Calificarea regularizării diferă de calificarea avansului.");
                    reper = sursa.Exigibilitate;
                } else if (l.Ajustare) {
                    Adauga("TVA_AJUSTARE_FARA_SURSA", "Proveniența fiscală nu este rezolvată; cota originală nu poate fi validată.");
                    continue;
                }
                if (l.TipActual != null && !IntervalTva.Contine(reper, l.TipActual.DeLa, l.TipActual.PanaLa))
                    Adauga("TVA_IN_AFARA_INTERVALULUI", "Reperul fiscal este în afara intervalului curent al tipului.");
            }
        }
        return rezultat;
    }
}
