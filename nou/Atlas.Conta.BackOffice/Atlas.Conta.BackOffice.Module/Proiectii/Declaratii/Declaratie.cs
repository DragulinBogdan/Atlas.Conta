using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

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
    /// <summary>Parametrii cererii, în forma din care `FisiereDeclaratii.Regenereaza` reface fișierul (122 i).</summary>
    public string Parametri { get; set; }
    public string Sha256 => Continut == null ? null : Convert.ToHexStringLower(SHA256.HashData(Continut));
}

/// <summary>Parametrii fișierului D300: externii decontului, datele de perioadă și antetul de la export.</summary>
public sealed record ParametriFisierD300(ParametriD300 Externi, ParametriXmlD300 Xml, AntetDeclaratie Antet);

/// <summary>Parametrii fișierului D394: datele de perioadă și antetul de la export.</summary>
public sealed record ParametriFisierD394(ParametriXmlD394 Xml, AntetDeclaratie Antet);

/// <summary>Fișierul unei perioade refăcut din faptele cubului și din parametrii păstrați la export (122 i).</summary>
public static class FisiereDeclaratii {
    static readonly JsonSerializerOptions Optiuni = new() { IncludeFields = true };

    public static Type[] Citite(FormularFiscal formular) => formular == FormularFiscal.D394
        ? [typeof(Cub.Postare), typeof(Societate), typeof(Document), typeof(PlajaFacturi)]
        : [typeof(Cub.Postare), typeof(Societate)];

    public static string Serializeaza<T>(T parametri) => JsonSerializer.Serialize(parametri, Optiuni);

    public static bool Valizi(FormularFiscal formular, string parametri) {
        try {
            return formular == FormularFiscal.D394
                ? JsonSerializer.Deserialize<ParametriFisierD394>(parametri, Optiuni) is { Xml: not null, Antet: not null }
                : JsonSerializer.Deserialize<ParametriFisierD300>(parametri, Optiuni) is { Externi: not null, Xml: not null, Antet: not null };
        }
        catch (JsonException) {
            return false;
        }
    }

    /// <summary>Parametrii trebuie să fi trecut de `Valizi`.</summary>
    public static FisierDeclaratie Regenereaza(IObjectSpace os, FormularFiscal formular,
            DateOnly inceput, DateOnly sfarsit, string parametri) {
        if (formular == FormularFiscal.D394) {
            var d394 = JsonSerializer.Deserialize<ParametriFisierD394>(parametri, Optiuni);
            return D394Xml.Genereaza(os, inceput, sfarsit, d394.Xml, d394.Antet);
        }
        var d300 = JsonSerializer.Deserialize<ParametriFisierD300>(parametri, Optiuni);
        return D300Xml.Genereaza(os, inceput, sfarsit, d300.Externi, d300.Xml, d300.Antet);
    }

    /// <summary>Refuzurile lunii refăcute din parametri; o altă amprentă înseamnă alt fișier decât cel exportat.</summary>
    public static List<string> Verifica(IObjectSpace os, FormularFiscal formular, int an, int luna, string sha, string parametri) {
        var inceput = new DateOnly(an, luna, 1);
        var fisier = Regenereaza(os, formular, inceput, inceput.AddMonths(1).AddDays(-1), parametri);
        if (fisier.Refuzuri.Count > 0)
            return [.. fisier.Refuzuri.Select(r => $"{r.Cod}: {r.Mesaj}")];
        return fisier.Sha256 == sha ? []
            : ["DEPUNERE_FISIER_DIFERIT: fișierul refăcut din fapte și parametri diferă de cel exportat; "
                + "exportați din nou înainte de confirmare."];
    }
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

    /// <summary>
    /// Părțile unui total rotunjit, în lei, cu suma egală cu totalul: latura negativă ia rotunjirea sumei ei,
    /// cea pozitivă restul, iar fiecare latură se repartizează Hamilton pe valorile ei (122 b).
    /// </summary>
    public static decimal[] Lei(decimal total, IReadOnlyList<decimal> parti) {
        var cote = new decimal[parti.Count];
        var negativa = Lei(parti.Where(p => p < 0m).Sum());
        Latura(1, total - negativa);
        Latura(-1, negativa);
        return cote;

        void Latura(int semn, decimal suma) {
            var indici = Enumerable.Range(0, parti.Count).Where(i => Math.Sign(parti[i]) == semn).ToList();
            if (indici.Count == 0)
                return;
            var repartizate = N.Repartizare.Hamilton(suma, [.. indici.Select(i => Math.Abs(parti[i]))], 0);
            for (var k = 0; k < indici.Count; k++)
                cote[indici[k]] = repartizate[k];
        }
    }
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
