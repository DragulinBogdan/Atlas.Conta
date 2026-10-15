using System.Xml.Linq;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Datele de perioadă ale D394, cu implicitele formularului (E2-D4).</summary>
public sealed record ParametriXmlD394(bool PersoaneAfiliate = false, bool SchimbOptiune = false);

/// <summary>Declarația 394 pe schema v5 (E2-D1…D7).</summary>
public static class D394Xml {
    static readonly string[] Blocante = [
        nameof(CodAvertismentD394.Tip1FaraCui), nameof(CodAvertismentD394.PfFaraCnp),
        nameof(CodAvertismentD394.TvaPeTipFaraColoana), nameof(CodAvertismentD394.CotaNeintreaga),
        nameof(CodAvertismentD394.FaraOp11), nameof(CodAvertismentD394.CombinatieRefuzata),
    ];

    static readonly int[] CoteInformatii = [24, 21, 11, 20, 19, 9, 5];

    public static FisierDeclaratie Genereaza(IObjectSpace os, DateOnly inceput, DateOnly sfarsit, ParametriXmlD394 parametri) {
        parametri ??= new ParametriXmlD394();
        var dto = D394Proiectii.D394(os, inceput, sfarsit, lei: true);
        var fisier = new FisierDeclaratie { VersiuneExportata = dto.VersiuneExportata };
        fisier.Refuzuri.AddRange(dto.RefuzuriXml);
        if (fisier.Refuzuri.Count > 0)
            return fisier;
        var antet = AntetDeclaratie.Citeste(os);
        var perioada = PerioadaDeclarata.Din(inceput, sfarsit, antet.Periodicitate, out _);
        XNamespace ns = FormulareAnaf.D394v5.SpatiuNume;
        var efectuate = dto.Operatiuni.Count > 0;
        long L(decimal v) => (long)v;

        var rezumat2 = efectuate
            ? dto.RezumatCote.Select(r => new XElement(ns + "rezumat2",
                new XAttribute("cota", r.Cota),
                Zero("bazaFSLcod", "TVAFSLcod", "bazaFSL", "TVAFSL", "bazaFSA", "TVAFSA", "bazaFSAI", "TVAFSAI", "bazaBFAI", "TVABFAI"),
                new XAttribute("nrFacturiL", r.NrFacturiL), new XAttribute("bazaL", L(r.BazaL)), new XAttribute("tvaL", L(r.TvaL)),
                new XAttribute("nrFacturiA", r.NrFacturiA), new XAttribute("bazaA", L(r.BazaA)), new XAttribute("tvaA", L(r.TvaA)),
                new XAttribute("nrFacturiAI", r.NrFacturiAI), new XAttribute("bazaAI", L(r.BazaAI)), new XAttribute("tvaAI", L(r.TvaAI)),
                r.Cota == 24 ? null : Zero("baza_incasari_i1", "tva_incasari_i1", "baza_incasari_i2", "tva_incasari_i2"),
                Zero("bazaL_PF", "tvaL_PF"))).ToList()
            : [];
        var total = dto.NrCui1 + dto.NrCui2 + dto.NrCui3 + dto.NrCui4
            + dto.RezumatCote.Sum(r => L(r.BazaL) + L(r.BazaA) + L(r.BazaAI));

        var radacina = new XElement(ns + "declaratie394",
            new XAttribute("luna", perioada.Luna), new XAttribute("an", perioada.An), new XAttribute("tip_D394", perioada.Tip),
            new XAttribute("sistemTVA", 0), new XAttribute("op_efectuate", efectuate ? 1 : 0),
            new XAttribute("cui", antet.Cui), new XAttribute("caen", antet.Caen), new XAttribute("den", antet.Denumire),
            new XAttribute("adresa", antet.Adresa), new XAttribute("telefon", antet.Telefon),
            antet.Email == null ? null : new XAttribute("mail", antet.Email),
            new XAttribute("totalPlata_A", efectuate ? total : 0),
            new XAttribute("denR", $"{antet.DeclarantNume} {antet.DeclarantPrenume}"),
            new XAttribute("functie_reprez", antet.DeclarantFunctie), new XAttribute("adresaR", antet.DeclarantAdresa),
            new XAttribute("tip_intocmit", (int)antet.IntocmitDe), new XAttribute("den_intocmit", antet.IntocmitDenumire),
            new XAttribute("cif_intocmit", antet.IntocmitCif),
            antet.IntocmitCalitate == null ? null
                : new XAttribute(antet.IntocmitDe == TipIntocmitor.PersoanaJuridica ? "calitate_intocmit" : "functie_intocmit", antet.IntocmitCalitate),
            new XAttribute("optiune", antet.OptiuneConsultare ? 1 : 0),
            parametri.SchimbOptiune ? new XAttribute("schimb_optiune", 1) : null,
            new XAttribute("prsAfiliat", parametri.PersoaneAfiliate ? 1 : 0),
            new XElement(ns + "informatii",
                new XAttribute("nrCui1", dto.NrCui1), new XAttribute("nrCui2", dto.NrCui2),
                new XAttribute("nrCui3", dto.NrCui3), new XAttribute("nrCui4", dto.NrCui4),
                Zero("nr_BF_i1", "incasari_i1", "incasari_i2", "nrFacturi_terti", "nrFacturi_benef"),
                new XAttribute("nrFacturi", dto.NrFacturi),
                Zero("nrFacturiL_PF", "nrFacturiLS_PF", "val_LS_PF"),
                CoteInformatii.Select(c => new XAttribute($"tvaDedAI{c}", 0)),
                new XAttribute("solicit", 0)),
            dto.Rezumat.Select(r => Rezumat1(ns, r, dto.Operatiuni)),
            rezumat2,
            dto.SeriiFacturi.Select(s => new XElement(ns + "serieFacturi",
                new XAttribute("tip", s.Tip), s.Serie == null ? null : new XAttribute("serieI", s.Serie),
                new XAttribute("nrI", s.NrI), new XAttribute("nrF", s.NrF))),
            dto.Operatiuni.Select(o => new XElement(ns + "op1",
                new XAttribute("tip", o.Tip), new XAttribute("tip_partener", o.TipPartener), new XAttribute("cota", o.Cota),
                o.CuiP == null ? null : new XAttribute("cuiP", o.CuiP), new XAttribute("denP", o.Denumire),
                new XAttribute("nrFact", o.NrFact), new XAttribute("baza", L(o.Baza)),
                o.Tva is decimal t ? new XAttribute("tva", L(t)) : null,
                o.Bunuri.Select(b => new XElement(ns + "op11",
                    new XAttribute("nrFactPR", b.NrFact), new XAttribute("codPR", b.CodPR), new XAttribute("bazaPR", L(b.Baza)),
                    b.Tva is decimal tb && o.TipPartener == 1 ? new XAttribute("tvaPR", L(tb)) : null)))));

        fisier.Continut = XmlDeclaratie.Octeti(new XDocument(new XDeclaration("1.0", "UTF-8", null), radacina));
        fisier.NumeFisier = $"D394_{antet.Cui}_{perioada.An:0000}{perioada.Luna:00}.xml";
        return fisier;
    }

    static XElement Rezumat1(XNamespace ns, D394Rezumat r, List<D394Operatiune> operatiuni) {
        XAttribute A(string nume, object valoare) => valoare == null ? null
            : new XAttribute(nume, valoare is decimal d ? (long)d : valoare);
        var detalii = r.TipPartener != 1 ? [] : operatiuni
            .Where(o => o.TipPartener == 1 && o.Cota == r.Cota && o.Tip == (r.Cota == 0 ? "V" : "C"))
            .SelectMany(o => o.Bunuri)
            .GroupBy(b => b.CodPR >= 1000 ? 21 : b.CodPR).OrderBy(g => g.Key)
            .Select(g => r.Cota == 0
                ? new XElement(ns + "detaliu", new XAttribute("bun", g.Key),
                    new XAttribute("nrLivV", g.Sum(b => b.NrFact)), new XAttribute("bazaLivV", (long)g.Sum(b => b.Baza)))
                : new XElement(ns + "detaliu", new XAttribute("bun", g.Key),
                    new XAttribute("nrAchizC", g.Sum(b => b.NrFact)), new XAttribute("bazaAchizC", (long)g.Sum(b => b.Baza)),
                    new XAttribute("tvaAchizC", (long)g.Sum(b => b.Tva ?? 0m))))
            .ToList();
        return new XElement(ns + "rezumat1",
            A("tip_partener", r.TipPartener), A("cota", r.Cota),
            A("facturiL", r.FacturiL), A("bazaL", r.BazaL), A("tvaL", r.TvaL),
            A("facturiLS", r.FacturiLS), A("bazaLS", r.BazaLS),
            A("facturiA", r.FacturiA), A("bazaA", r.BazaA), A("tvaA", r.TvaA),
            A("facturiAI", r.FacturiAI), A("bazaAI", r.BazaAI), A("tvaAI", r.TvaAI),
            A("facturiAS", r.FacturiAS), A("bazaAS", r.BazaAS),
            A("facturiV", r.FacturiV), A("bazaV", r.BazaV),
            A("facturiC", r.FacturiC), A("bazaC", r.BazaC), A("tvaC", r.TvaC),
            A("facturiN", r.FacturiN), A("bazaN", r.BazaN),
            detalii);
    }

    static IEnumerable<XAttribute> Zero(params string[] nume) => nume.Select(n => new XAttribute(n, 0));

    /// <summary>Refuzurile fișierului pentru perioada cerută, înaintea serializării (E2-D1, E2-D4, E2-D5).</summary>
    public static List<RefuzDeclaratie> Refuzuri(AntetDeclaratie antet, DateOnly inceput, DateOnly sfarsit, D394Dto dto) {
        var refuzuri = new List<RefuzDeclaratie>();
        void Refuz(string cod, string mesaj) => refuzuri.Add(new RefuzDeclaratie { Cod = cod, Mesaj = mesaj });

        if (FormulareAnaf.Pentru(FormularFiscal.D394, inceput) == null)
            Refuz(CoduriDeclaratie.PerioadaNesuportata,
                $"Fișierul D394 se generează pe schema v5, pentru operațiunile de la {FormulareAnaf.D394v5.ValabilDeLa:dd.MM.yyyy}.");
        if (antet == null)
            Refuz(CoduriDeclaratie.AntetIncomplet, "Societatea nu are rândul de identificare completat.");
        else {
            if (PerioadaDeclarata.Din(inceput, sfarsit, antet.Periodicitate, out var motiv) == null)
                Refuz(CoduriDeclaratie.PerioadaNesuportata, motiv);
            if (antet.LipsaD394() is { Count: > 0 } lipsa)
                Refuz(CoduriDeclaratie.AntetIncomplet, "Completați la societate: " + string.Join(", ", lipsa) + ".");
        }
        foreach (var n in dto.Neincluse.Where(n => n.Baza != 0m || n.Tva != 0m))
            Refuz(CoduriDeclaratie.DateNeincluse,
                $"{n.Cauza}: tipul de TVA {n.TipTvaCod ?? n.TipTvaId.ToString()} ({n.SensTva}) cu baza {n.Baza:0.00} nu intră în declarație.");
        foreach (var a in dto.Avertismente.Where(a => Blocante.Contains(a.Cod)))
            Refuz(CoduriDeclaratie.DateNeincluse, $"{a.Cod}: {a.Mesaj} {string.Join("; ", a.Exemple)}");
        if (dto.RezumatCote.Count > 5)
            Refuz(CoduriDeclaratie.DateNeincluse, $"Perioada are {dto.RezumatCote.Count} cote, iar formularul primește cel mult 5.");
        return refuzuri;
    }
}
