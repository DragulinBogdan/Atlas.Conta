using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Identitatea declarantului, citită din `Societate` (E2-D4).</summary>
public sealed class AntetDeclaratie {
    public string Cui, Denumire, Adresa, Telefon, Email, Banca, Cont, Caen;
    public PeriodicitateTva Periodicitate;
    public string DeclarantNume, DeclarantPrenume, DeclarantFunctie, DeclarantAdresa;
    public TipIntocmitor IntocmitDe;
    public string IntocmitDenumire, IntocmitCif, IntocmitCalitate;
    public bool OptiuneConsultare;

    public static AntetDeclaratie Citeste(IObjectSpace os) {
        var s = os.GetObjectsQuery<Societate>()
            .Select(x => new {
                x.CodFiscal, x.Denumire, x.Strada, x.Numar, x.DetaliiAdresa, x.Localitate, x.CodPostal,
                Judet = x.Judet.Denumire, x.Telefon, x.Email,
                Banca = x.ContBancar.Denumire, x.ContBancar.Iban, x.Caen, x.PeriodicitateTva,
                x.DeclarantNume, x.DeclarantPrenume, x.DeclarantFunctie, x.DeclarantAdresa,
                x.IntocmitDe, x.IntocmitDenumire, x.IntocmitCif, x.IntocmitCalitate, x.OptiuneConsultareD394,
            })
            .FirstOrDefault();
        if (s == null)
            return null;
        var cui = Cifre(s.CodFiscal);
        var adresa = Uneste(s.Strada is { Length: > 0 } strada ? "Str. " + strada : null,
            s.Numar is { Length: > 0 } nr ? "nr. " + nr : null, s.DetaliiAdresa, s.Localitate,
            s.Judet is { Length: > 0 } judet ? "jud. " + judet : null, s.CodPostal);
        return new AntetDeclaratie {
            Cui = cui, Denumire = Curat(s.Denumire), Adresa = adresa, Telefon = Curat(s.Telefon), Email = Curat(s.Email),
            Banca = Curat(s.Banca), Cont = Curat(s.Iban), Caen = Curat(s.Caen), Periodicitate = s.PeriodicitateTva,
            DeclarantNume = Curat(s.DeclarantNume), DeclarantPrenume = Curat(s.DeclarantPrenume),
            DeclarantFunctie = Curat(s.DeclarantFunctie), DeclarantAdresa = Curat(s.DeclarantAdresa) ?? adresa,
            IntocmitDe = s.IntocmitDe,
            IntocmitDenumire = Curat(s.IntocmitDenumire) ?? Curat(s.Denumire),
            IntocmitCif = Cifre(s.IntocmitCif) ?? cui,
            IntocmitCalitate = Curat(s.IntocmitCalitate),
            OptiuneConsultare = s.OptiuneConsultareD394,
        };
    }

    /// <summary>Câmpurile obligatorii lipsă sau invalide pentru D300.</summary>
    public List<string> LipsaD300() {
        var lipsa = Comune();
        if (DeclarantNume == null) lipsa.Add("numele declarantului");
        if (DeclarantPrenume == null) lipsa.Add("prenumele declarantului");
        if (DeclarantFunctie == null) lipsa.Add("funcția declarantului");
        if (Banca == null) lipsa.Add("banca (contul bancar al societății)");
        else if (Banca.IndexOfAny([',', '#']) >= 0 || Banca.Length > 50) lipsa.Add("banca de cel mult 50 de caractere, fără „,” și „#”");
        if (Cont == null) lipsa.Add("IBAN-ul contului bancar");
        else if (Cont.IndexOfAny([',', '#']) >= 0) lipsa.Add("IBAN-ul fără caracterele „,” și „#”");
        return lipsa;
    }

    /// <summary>Câmpurile obligatorii lipsă sau invalide pentru D394.</summary>
    public List<string> LipsaD394() {
        var lipsa = Comune();
        if (Telefon == null) lipsa.Add("telefonul societății");
        if (DeclarantNume == null || DeclarantPrenume == null) lipsa.Add("numele și prenumele reprezentantului");
        if (DeclarantFunctie == null) lipsa.Add("funcția reprezentantului");
        if (IntocmitCif == null || IntocmitCif.Length > 13) lipsa.Add("CUI-ul sau CNP-ul celui care întocmește");
        if (IntocmitDe == TipIntocmitor.PersoanaJuridica && IntocmitCalitate == null)
            lipsa.Add("calitatea celui care întocmește");
        return lipsa;
    }

    List<string> Comune() {
        var lipsa = new List<string>();
        if (Cui == null || !SaftReguli.CuiValid(Cui)) lipsa.Add("codul fiscal valid al societății");
        if (Denumire == null) lipsa.Add("denumirea societății");
        if (Adresa == null) lipsa.Add("adresa societății");
        if (Caen == null) lipsa.Add("codul CAEN");
        if (Telefon is { Length: > 15 }) lipsa.Add("telefonul de cel mult 15 caractere");
        return lipsa;
    }

    static string Curat(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    static string Cifre(string text) =>
        Curat(text) is { } t && new string(t.Where(char.IsAsciiDigit).ToArray()) is { Length: > 0 } c ? c : null;

    static string Uneste(params string[] parti) =>
        parti.Select(Curat).Where(p => p != null).ToList() is { Count: > 0 } l ? string.Join(", ", l) : null;
}
