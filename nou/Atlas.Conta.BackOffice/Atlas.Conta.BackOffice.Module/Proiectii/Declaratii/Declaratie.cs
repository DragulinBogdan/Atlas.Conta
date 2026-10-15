using System.Text;
using System.Xml;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

public static class CoduriDeclaratie {
    public const string PerioadaNesuportata = "DECLARATIE_PERIOADA_NESUPORTATA";
    public const string AntetIncomplet = "DECLARATIE_ANTET_INCOMPLET";
    public const string DateNeincluse = "DECLARATIE_DATE_NEINCLUSE";
}

public sealed class RefuzDeclaratie {
    public string Cod { get; set; }
    public string Mesaj { get; set; }
}

/// <summary>Fișierul unei declarații; gol cât există refuzuri (E2-D4, E2-D5).</summary>
public sealed class FisierDeclaratie {
    public List<RefuzDeclaratie> Refuzuri { get; } = [];
    public byte[] Continut { get; set; }
    public string NumeFisier { get; set; }
    public string VersiuneExportata { get; set; }
}

/// <summary>Versiunea de formular ANAF pe care se serializează o perioadă (E2-D1).</summary>
public sealed record FormularAnaf(FormularFiscal Formular, string SpatiuNume, DateOnly ValabilDeLa);

public static class FormulareAnaf {
    public static readonly FormularAnaf D300v12 = new(FormularFiscal.D300, "mfp:anaf:dgti:d300:declaratie:v12", new(2026, 1, 1));
    public static readonly FormularAnaf D394v5 = new(FormularFiscal.D394, "mfp:anaf:dgti:d394:declaratie:v5", new(2025, 8, 1));

    static readonly FormularAnaf[] Toate = [D300v12, D394v5];

    public static FormularAnaf Pentru(FormularFiscal formular, DateOnly inceput) =>
        Toate.Where(f => f.Formular == formular && f.ValabilDeLa <= inceput).MaxBy(f => f.ValabilDeLa);
}

/// <summary>Luna sau trimestrul declarat; `Luna` e ultima lună a perioadei (E2-D1).</summary>
public sealed record PerioadaDeclarata(int An, int Luna, PeriodicitateTva Periodicitate) {
    public string Tip => Periodicitate == PeriodicitateTva.Trimestrial ? "T" : "L";

    public static PerioadaDeclarata Din(DateOnly inceput, DateOnly sfarsit, PeriodicitateTva periodicitate, out string motiv) {
        var luni = periodicitate == PeriodicitateTva.Trimestrial ? 3 : 1;
        motiv = null;
        if (inceput.Day != 1 || sfarsit != inceput.AddMonths(luni).AddDays(-1))
            motiv = periodicitate == PeriodicitateTva.Trimestrial
                ? "Societatea declară trimestrial: perioada trebuie să fie un trimestru calendaristic întreg."
                : "Societatea declară lunar: perioada trebuie să fie o lună calendaristică întreagă.";
        else if (periodicitate == PeriodicitateTva.Trimestrial && (inceput.Month - 1) % 3 != 0)
            motiv = "Trimestrul începe în ianuarie, aprilie, iulie sau octombrie.";
        return motiv == null ? new PerioadaDeclarata(sfarsit.Year, sfarsit.Month, periodicitate) : null;
    }
}

public static class RotunjireDeclaratie {
    /// <summary>Lei întregi: sub 50 de bani se neglijează, de la 50 de bani se întregește (E2-D3).</summary>
    public static decimal Lei(decimal valoare) => Math.Round(valoare, 0, MidpointRounding.AwayFromZero);
}

public static class XmlDeclaratie {
    /// <summary>Octeții fișierului: UTF-8 fără BOM, indentat, cu `\n` pe orice platformă.</summary>
    public static byte[] Octeti(XDocument document) {
        using var flux = new MemoryStream();
        using (var scriitor = XmlWriter.Create(flux, new XmlWriterSettings {
            Encoding = new UTF8Encoding(false), Indent = true, NewLineChars = "\n",
        }))
            document.Save(scriitor);
        return flux.ToArray();
    }
}
