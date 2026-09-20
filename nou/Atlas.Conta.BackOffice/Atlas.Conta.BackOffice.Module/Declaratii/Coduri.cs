#nullable enable
namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Codurile de refuz ale declaranților: stabile, fără localizare — mesajul
/// românesc al motorului vechi rămâne al lui până la TR-D7 (B-D3).
/// </summary>
public static class CoduriRefuz {
    public const string PredatorNepotrivit = "PREDATOR_NEPOTRIVIT";
    public const string PrimitorNepotrivit = "PRIMITOR_NEPOTRIVIT";
    public const string ContPropriuLipsa = "CONT_PROPRIU_LIPSA";
    public const string LaturiIdentice = "LATURI_IDENTICE";
    public const string LiniiLipsa = "LINII_LIPSA";
    public const string LotLipsa = "LOT_LIPSA";
    public const string CantitateNepozitiva = "CANTITATE_NEPOZITIVA";
    public const string ValoareNepozitiva = "VALOARE_NEPOZITIVA";
    public const string ViramentMixt = "VIRAMENT_MIXT";
    public const string RegulaContareLipsa = "REGULA_CONTARE_LIPSA";
}
