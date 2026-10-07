#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public abstract record ImobilizareCuleasa(Guid Fisa);
public sealed record PifCules(Guid Fisa, FelLiniePif Fel, Guid? LinieSursa,
    decimal Fiscal, decimal AmortizareInitiala, decimal AmortizareFiscalaInitiala) : ImobilizareCuleasa(Fisa);
public sealed record AmoCules(Guid Fisa, decimal Fiscal) : ImobilizareCuleasa(Fisa);
public sealed record CasCules(Guid Fisa, FelLinieIesire Fel) : ImobilizareCuleasa(Fisa);

public sealed record FisaFapt(Guid Id, Guid Cont, Guid ContAmortizare, Guid ContCheltuiala,
    Guid ContCedare, Guid Loc, DateOnly Deschisa, decimal BrutFiscal, decimal AmortizareFiscala) {
    /// <summary>Rândul `PoliticaAmortizare` al tipului fișei, sau null (D9-A8).</summary>
    public N.VersiunePolitica? Politica { get; init; }
}
public sealed record SuportFapt(N.ReferintaPostare Referinta, Guid? Linie, N.Capat Capat,
    N.Latura Latura, decimal Disponibil, DateOnly Data, bool FaraLinieSursa);
public sealed record DisponibilFapt(N.Capat Capat, decimal DebitNetMinim, decimal CreditNetMinim);
