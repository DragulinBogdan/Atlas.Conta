using System.Xml.Linq;
using Xunit;

namespace Atlas.Conta.Nucleu.Teste;

public class ArhitecturaTeste {
    [Fact]
    public void NucleulReferaDoarBcl() {
        var referinte = typeof(Postare).Assembly.GetReferencedAssemblies();
        Assert.NotEmpty(referinte);
        foreach (var referinta in referinte) {
            var nume = referinta.Name ?? "";
            Assert.True(
                nume.StartsWith("System.", StringComparison.Ordinal) || nume is "netstandard" or "mscorlib",
                $"referință în afara BCL: {nume}");
        }
    }

    [Fact]
    public void ProiectulNucleuluiNuAreNicioReferinta() {
        var proiect = XDocument.Load(
            Path.Combine(FolderulSolutiei(), "Atlas.Conta.Nucleu", "Atlas.Conta.Nucleu.csproj"));
        Assert.Empty(proiect.Descendants("PackageReference"));
        Assert.Empty(proiect.Descendants("ProjectReference"));
    }

    static string FolderulSolutiei() {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(Path.Combine(folder.FullName, "Atlas.Conta.Nucleu.slnx")))
            folder = folder.Parent;
        return folder?.FullName
            ?? throw new InvalidOperationException(
                $"nu găsesc folderul cu Atlas.Conta.Nucleu.slnx pornind de la {AppContext.BaseDirectory}");
    }
}
