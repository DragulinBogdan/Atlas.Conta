namespace Atlas.Conta.BackOffice.ModelCheck;

// C111-D10: `Postare.Partener` și `Postare.Gestiune` se numesc în producție doar în lista de mai jos;
// compilatorul refuză restul prin diagnosticul `C111`.
static class ProbePoartaCoordonate {
    const string Pragma = "#pragma warning disable C111";

    static readonly (string Cale, string Rol)[] Permise = [
        ("Atlas.Conta.BackOffice.Module/Cub/Citiri/", "cititorii comuni"),
        ("Atlas.Conta.BackOffice.Module/Cub/Randuri.cs", "scriitorul cubului"),
        ("Atlas.Conta.BackOffice.Module/UI/ContaUiBaseline.cs", "coloanele listei de postări"),
    ];

    public static void VerificaSursa(Action<string, bool> check) {
        var radacina = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice"));
        var fisiere = Directory.EnumerateFiles(radacina, "*.cs", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(radacina, f).Replace('\\', '/'))
            .Where(f => !f.Split('/').Any(d => d is "bin" or "obj" or "Migrations"))
            .ToList();
        var cuPragma = fisiere.Where(f => File.ReadAllText(Path.Combine(radacina, f)).Contains(Pragma, StringComparison.Ordinal)).ToList();
        var incalcari = cuPragma.Where(f => !Permise.Any(p => f.StartsWith(p.Cale, StringComparison.Ordinal))).ToList();
        var nefolosite = Permise.Where(p => !cuPragma.Any(f => f.StartsWith(p.Cale, StringComparison.Ordinal))).Select(p => p.Cale).ToList();

        var proiecte = Directory.EnumerateFiles(radacina, "*.csproj", SearchOption.AllDirectories)
            .Where(f => !f.Replace('\\', '/').Split('/').Any(d => d is "bin" or "obj"))
            .Select(f => (Nume: Path.GetFileName(f), Text: File.ReadAllText(f))).ToList();
        var modul = proiecte.Single(p => p.Nume == "Atlas.Conta.BackOffice.Module.csproj").Text;
        var eroare = modul.Contains("<WarningsAsErrors>$(WarningsAsErrors);C111</WarningsAsErrors>", StringComparison.Ordinal);
        var tacute = proiecte.Where(p => p.Text.Contains("C111", StringComparison.Ordinal) && p.Text.Contains("<NoWarn>", StringComparison.Ordinal))
            .Select(p => p.Nume).ToList();

        Console.WriteLine($"     MĂSURAT (C111-D10): {fisiere.Count} fișiere de producție; {cuPragma.Count} cu excepția `C111`; "
            + $"încălcări [{string.Join("; ", incalcari)}]; permise nefolosite [{string.Join("; ", nefolosite)}]; "
            + $"proiecte de producție care tac diagnosticul [{string.Join("; ", tacute)}].");
        check("C111-D10: `Postare.Partener` și `Postare.Gestiune` se numesc în producție doar în "
            + string.Join(", ", Permise.Select(p => p.Rol)) + "; în rest diagnosticul `C111` e eroare de compilare",
            fisiere.Count > 100 && eroare && incalcari.Count == 0 && nefolosite.Count == 0 && tacute.Count == 0);
    }
}
