using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiDecl {
    void PunteD300D394() {
        Opereaza(VanzareNumerotata(Luna(8), 50, "N21"));
        Opereaza(VanzareNumerotata(Luna(8), 100, "SFD"));
        var decalata = Achizitie(Luna(9, 2), 200, "N21");
        Comanda(os => {
            var f = os.GetObjectByKey<FacturaIntrare>(decalata);
            f.DataInregistrare = Luna(9, 2); f.DataPrimire = Luna(8, 28);
            os.CommitChanges();
        });
        Opereaza(decalata);
        var numar = CuSpatiu(os => os.GetObjectByKey<FacturaIntrare>(decalata).Numar);

        var (august, augustSfarsit) = Perioada(8);
        ExportaD394("SC-DECL-17", august, augustSfarsit);
        var dAug = CuSpatiu(os => D394Proiectii.D394(os, august, augustSfarsit));
        Verifica("SC-DECL-17", "livrarea scutită SFD 100 iese în `Excluse` cu motivul, nu în `Neincluse`, și nu oprește fișierul (123 a)",
            dAug.Excluse.SingleOrDefault() is { TipTvaCod: "SFD", SensTva: "Livrare", Baza: 100m, Tva: 0m } x
            && !string.IsNullOrWhiteSpace(x.Motiv) && dAug.Neincluse.Count == 0
            && Rand(dAug, "SFD", SensTva.Livrare) is { BazaD300: 100m, BazaExclusa: 100m, BazaDeclarata: 0m, BazaDiferenta: 100m });

        var (septembrie, septembrieSfarsit) = Perioada(9);
        var dSep = CuSpatiu(os => D394Proiectii.D394(os, septembrie, septembrieSfarsit));
        var achAug = Rand(dAug, "N21", SensTva.Achizitie);
        var achSep = Rand(dSep, "N21", SensTva.Achizitie);
        Verifica("SC-DECL-18", $"factura primită în august și înregistrată în septembrie: în D394 august, în D300 septembrie; puntea o numește ({numar})",
            achAug is { BazaD300: 0m, BazaDoarD394: 200m, TvaDoarD394: 42m, BazaDeclarata: 200m, BazaDiferenta: -200m, NrDocumenteDecalate: 1 }
            && achAug.DocumenteDecalate.Single().StartsWith(numar)
            && achSep is { BazaD300: 200m, BazaDoarD300: 200m, TvaDoarD300: 42m, BazaDeclarata: 0m, BazaDiferenta: 200m, NrDocumenteDecalate: 1 });

        var (aprilie, aprilieSfarsit) = Perioada(4);
        var dApr = CuSpatiu(os => D394Proiectii.D394(os, aprilie, aprilieSfarsit));
        var toate = new[] { dApr, dAug, dSep }.SelectMany(d => d.Punte).ToList();
        Verifica("SC-DECL-18", $"puntea se închide pe fiecare rând (D300 − doar D300 + doar D394 = declarat + exclus + neinclus), {toate.Count} rânduri",
            toate.Count > 0 && toate.All(r =>
                r.BazaD300 - r.BazaDoarD300 + r.BazaDoarD394 == r.BazaDeclarata + r.BazaExclusa + r.BazaNeinclusa
                && r.TvaD300 - r.TvaDoarD300 + r.TvaDoarD394 == r.TvaDeclarata + r.TvaExclusa + r.TvaNeinclusa)
            && new[] { dApr, dAug, dSep }.All(d =>
                d.Punte.Sum(r => r.BazaDeclarata) == d.Operatiuni.Sum(o => o.Baza)
                && d.Punte.Sum(r => r.BazaExclusa) == d.Excluse.Sum(e => e.Baza)
                && d.Punte.Sum(r => r.BazaNeinclusa) == d.Neincluse.Sum(n => n.Baza)));
    }

    static D394Punte Rand(D394Dto d, string tipTva, SensTva sens) =>
        d.Punte.SingleOrDefault(r => r.TipTvaCod == tipTva && r.SensTva == sens.ToString() && r.Cota == (tipTva == "SFD" ? 0m : 21m));
}
