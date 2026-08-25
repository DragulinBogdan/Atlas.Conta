namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Statele membre ale UE (27, fără UK), coduri ISO 3166-1 alpha-2 — LEGE, nu
// politică (D4-D1): intră în derivarea `tip_partener` (3 = stabilit în alt
// stat membru, 4 = în afara UE). Grecia e `GR` în ISO (nu `EL`, codul folosit
// de VIES pentru CUI-uri): `Tara` e cod de țară, nu prefix de CUI.
public static class TariUe {
    public static readonly IReadOnlySet<string> Coduri = new HashSet<string>(StringComparer.Ordinal) {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE",
        "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
    };

    public static bool Contine(string codTara) =>
        codTara != null && Coduri.Contains(codTara);
}
