#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>Ipotezele rândurilor de politică din care declarantul a luat un fapt folosit la o postare (D9-A8).</summary>
static class PoliticiConsumate {
    public static N.VersiunePolitica Versiunea(in RegulaContareFapt regula) =>
        new(nameof(RegulaContare), regula.Id, regula.Versiune);

    public static N.VersiunePolitica Versiunea(in RegulaStocFapt regula) =>
        new(nameof(RegulaStoc), regula.Id, regula.Versiune);

    public static N.VersiunePolitica Versiunea(PoliticaTvaFapt politica) =>
        new(nameof(PoliticaTva), politica.Id, politica.Versiune);

    public static N.VersiunePolitica Versiunea(PoliticaDiferentaFapt politica) =>
        new(nameof(PoliticaDiferenta), politica.Id, politica.Versiune);

    /// <summary>Politica de TVA, consumată când cel puțin o linie are fapt fiscal.</summary>
    public static N.VersiunePolitica? Fiscala(Operand operand, IEnumerable<TipTvaFapt?> tipuri) =>
        operand.PoliticaTva is { } politica && tipuri.Any(t => t != null) ? Versiunea(politica) : null;

    /// <summary>
    /// Perioada deschisă, apoi regulile de contare numite de <c>ContRezolvat</c> în ordinea primei apariții,
    /// apoi celelalte rânduri consumate; fără dubluri.
    /// </summary>
    public static List<N.Ipoteza> Ipoteze(Operand operand, IEnumerable<N.Decizie> decizii, params N.VersiunePolitica?[] altele) {
        var ipoteze = new List<N.Ipoteza> { operand.PerioadaDeschisa };
        var vazute = new HashSet<N.VersiunePolitica>();
        foreach (var id in decizii.OfType<N.ContRezolvat>().Select(d => d.Regula).OfType<Guid>()) {
            var regula = operand.ReguliContare.FirstOrDefault(r => r.Id == id);
            if (regula.Id != id)
                throw new InvalidOperationException($"Decizia numește regula de contare {id}, care nu e în operand.");
            Adauga(Versiunea(regula));
        }
        foreach (var alta in altele)
            if (alta is not null) Adauga(alta);
        return ipoteze;

        void Adauga(N.VersiunePolitica v) { if (vazute.Add(v)) ipoteze.Add(v); }
    }
}
