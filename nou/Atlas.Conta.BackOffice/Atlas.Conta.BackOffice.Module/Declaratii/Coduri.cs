#nullable enable
namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// Codurile de refuz ale declaranților: stabile, fără localizare — mesajul
/// românesc al motorului vechi rămâne al lui până la TR-D7 (B-D3).
/// </summary>
public static class CoduriRefuz {
    public const string ReceptieStructuraInvalida = "RECEPTIE_STRUCTURA_INVALIDA";
    public const string InventarStructuraInvalida = "INVENTAR_STRUCTURA_INVALIDA";
    public const string InventarStocNeacoperit = "INVENTAR_STOC_NEACOPERIT";
    public const string AsamblareNebalansata = "ASAMBLARE_NEBALANSATA";
    public const string AsamblareStructuraInvalida = "ASAMBLARE_STRUCTURA_INVALIDA";
    public const string AsamblareDeltaFaraAncora = "ASAMBLARE_DELTA_FARA_ANCORA";
    public const string AsamblareProdusNepozitiv = "ASAMBLARE_PRODUS_NEPOZITIV";
    public const string PredatorNepotrivit = "PREDATOR_NEPOTRIVIT";
    public const string PrimitorNepotrivit = "PRIMITOR_NEPOTRIVIT";
    public const string LaturiIdentice = "LATURI_IDENTICE";
    public const string LiniiLipsa = "LINII_LIPSA";
    public const string GestiuniIdentice = "GESTIUNI_IDENTICE";
    public const string LotLipsa = "LOT_LIPSA";
    public const string ContStocLipsa = "CONT_STOC_LIPSA";
    public const string CantitateNepozitiva = "CANTITATE_NEPOZITIVA";
    public const string ValoareNepozitiva = "VALOARE_NEPOZITIVA";
    public const string ViramentMixt = "VIRAMENT_MIXT";
    public const string RegulaContareLipsa = "REGULA_CONTARE_LIPSA";
    public const string NumarLipsa = "NUMAR_LIPSA";
    public const string ProdusAltTip = "PRODUS_ALT_TIP";
    public const string TipTvaLipsa = "TIP_TVA_LIPSA";
    public const string ContTvaLipsa = "CONT_TVA_LIPSA";
    public const string DirectieTvaNepotrivita = "DIRECTIE_TVA_NEPOTRIVITA";
    public const string ContExplicitLipsa = "CONT_EXPLICIT_LIPSA";
    public const string ValoareZero = "VALOARE_ZERO";
    public const string RepartitorExplicitLipsa = "REPARTITOR_EXPLICIT_LIPSA";
    public const string PartidaCuDependenti = "PARTIDA_CU_DEPENDENTI";
    public const string NaturaNepotrivita = "NATURA_NEPOTRIVITA";
    public const string CantitateZero = "CANTITATE_ZERO";
    public const string TvaCapitalizat = "TVA_CAPITALIZAT";
    public const string TvaImportNepotrivit = "TVA_IMPORT_NEPOTRIVIT";
    public const string PoliticaTvaLipsa = "POLITICA_TVA_LIPSA";
    public const string FisaLipsa = "FISA_LIPSA";
    public const string PoliticaAmortizareLipsa = "POLITICA_AMORTIZARE_LIPSA";
    public const string SuportInsuficient = "SUPORT_INSUFICIENT";
    public const string SuportCuDependenti = "SUPORT_CU_DEPENDENTI";
}
