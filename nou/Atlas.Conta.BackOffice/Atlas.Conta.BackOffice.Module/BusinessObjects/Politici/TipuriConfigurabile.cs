namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// 83i — o singură listă a tipurilor pe care le deține CONFIGURAȚIA, enumerată
// explicit: seed-ul rolului `Configurator`, raportul de profil și probele o
// citesc de aici. Reflecția peste `ICuProvenienta` rămâne a PROBEI, care
// verifică egalitatea în ambele sensuri — un tip nou fără drept și fără
// etichetă pică zgomotos, nu tăcut.
public static class Politici {
    /// <summary>Cele 17 tipuri `ICuProvenienta`: politicile + nomenclatoarele pe care seed-ul le tratează ca ale lui.</summary>
    public static IReadOnlyList<Type> TipuriConfigurabile { get; } = [
        typeof(TipDocument), typeof(TipTva), typeof(Cont), typeof(ClasaProdus), typeof(TipMaterial),
        typeof(RegulaStoc), typeof(RegulaContare), typeof(PoliticaConex), typeof(PoliticaScadenta),
        typeof(PoliticaValidare), typeof(PoliticaTva), typeof(PoliticaInchidereTva),
        typeof(PoliticaNumerotare), typeof(PoliticaMiscareSaft), typeof(PoliticaTvaImplicit),
        typeof(MapareD300), typeof(MapareD394),
    ];
}
