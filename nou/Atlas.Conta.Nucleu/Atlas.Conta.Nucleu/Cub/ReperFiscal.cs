namespace Atlas.Conta.Nucleu;

public sealed record ReperFiscal(
    Guid DocumentFiscal,
    DateOnly DataDocument,
    DateOnly DataExigibilitate,
    DateOnly? DataPrimire,
    DateOnly DataInregistrare,
    int PerioadaD394,
    bool RegularizareD300 = false,
    bool InversaTehnica = false);
