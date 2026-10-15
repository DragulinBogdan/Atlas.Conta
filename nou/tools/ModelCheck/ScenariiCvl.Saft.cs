using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SC-CVL-32 (CV-r1, E2-D10): luna în care o corecție e operată, compensată sau stornată se exportă în D406 L și S.
sealed partial class ScenariiCvl {
    sealed record CazSaft(Caz Partial, Guid CorectiePartiala, Caz Intact, Guid CorectieIntacta);

    CazSaft PregatesteSaft() {
        var partial = Partial();
        var intact = Lot();
        return new(partial, Corectie(partial, 200m), intact, Corectie(intact, 200m));
    }

    void StornoBonSaft(CazSaft caz) => Storneaza(BonulLui(caz.Partial), Zi(5, 2));

    void Saft(CazSaft caz) {
        const string id = "SC-CVL-32";
        Comanda(os => { var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = 3; os.CommitChanges(); });
        Storneaza(caz.CorectiePartiala, Zi(5, 3));
        var luni = new[] { 1, 2, 3 }.Select(luna => (Luna: luna,
            Facturi: CuSpatiu(os => SaftProiectii.SaftPeCub(os, An, luna, Zi(28, luna))),
            Stocuri: CuSpatiu(os => SaftProiectii.SaftStocuriPeCub(os, An, luna, Zi(28, luna))))).ToList();
        if (!Privat) {
            Verifica(id, "SAF-T nu se aplică profilului bugetar: nicio proiecție pe cele trei luni",
                luni.All(l => l.Facturi.Neaplicabil != null && l.Stocuri.Neaplicabil != null));
            return;
        }
        var (a, b) = (caz.CorectiePartiala, caz.CorectieIntacta);
        var aleCazului = caz.Partial.Documente.Concat(caz.Intact.Documente).ToHashSet();
        List<SaftRefuz> Pe(SaftDto d, params Guid[] documente) =>
            [.. d.Refuzuri.Where(r => r.DocumentId is Guid doc && documente.Contains(doc))];
        foreach (var (luna, facturi, stocuri) in luni)
            foreach (var r in facturi.Refuzuri.Concat(stocuri.Refuzuri).Where(r => r.DocumentId is not Guid doc || aleCazului.Contains(doc)))
                Console.WriteLine($"     MĂSURAT ({id}): luna {luna:00}, {r.Cod}: {r.Mesaj}");

        var (ianuarie, februarie, martie) = (luni[0].Facturi, luni[1].Facturi, luni[2].Facturi);
        Verifica(id, "D406 și declarația de stocuri, ianuarie–martie: niciun refuz pe documentele cazului (CV-r1, E2-D10)",
            luni.All(l => Pe(l.Facturi, [.. aleCazului]).Count == 0 && Pe(l.Stocuri, [.. aleCazului]).Count == 0));
        static string Linii(SaftFactura f) => f == null ? "<lipsă>" : string.Join(",", f.Linii.OrderBy(l => l.AccountID, StringComparer.Ordinal)
            .Select(l => $"{l.AccountID}:{l.InvoiceLineAmount:0.##}/{l.Quantity:0.##}"));
        var fa = ianuarie.FacturiPrimite.SingleOrDefault(f => f.DocumentId == a && !f.Storno);
        var fb = ianuarie.FacturiPrimite.SingleOrDefault(f => f.DocumentId == b && !f.Storno);
        Console.WriteLine($"     MĂSURAT ({id}): ianuarie, parțială {Linii(fa)}, intactă {Linii(fb)}");
        bool Corectie(SaftFactura f) => f is { InvoiceType: "380" }
            && f.Linii.All(l => l.Quantity == 0m && l.UnitPrice == 0m && l.TaxInformation.TaxCode == SaftReguli.TaxCodeNefiscal);
        Verifica(id, "D406 ianuarie: corecția parțială e factura 380 cu liniile 302 = 80 și 602 = 120, cantitate 0, fără TVA (000000)",
            Corectie(fa) && Linii(fa) == $"{Stoc}:80/0,602:120/0");
        Verifica(id, "D406 ianuarie: corecția lotului intact e factura 380 cu linia 302 = 200, cantitate 0, fără TVA",
            Corectie(fb) && Linii(fb) == $"{Stoc}:200/0");
        var compensare = februarie.Jurnale.Where(j => j.Tranzactii.Any(t => t.DocumentId == a)).ToList();
        Verifica(id, "D406 februarie: numai compensarea, în GL sub jurnalul FCT și fără factură",
            compensare.Count == 1 && compensare[0].JournalID == "FCT"
            && compensare[0].Tranzactii.Where(t => t.DocumentId == a).SelectMany(t => t.Linii).Sum(l => Math.Abs(l.Amount)) == 240m
            && !februarie.FacturiPrimite.Any(f => f.DocumentId == a));
        var stornoA = martie.FacturiPrimite.SingleOrDefault(f => f.DocumentId == a && f.Storno);
        Console.WriteLine($"     MĂSURAT ({id}): martie, stornoul corecției {Linii(stornoA)} ({stornoA?.InvoiceType})");
        Verifica(id, "D406 martie: stornoul corecției e evenimentul de storno, net −200 pe linii cu cantitate 0",
            stornoA != null && stornoA.Linii.Sum(l => l.InvoiceLineAmount) == -200m && stornoA.Linii.All(l => l.Quantity == 0m));

        string Miscari(SaftDto d, Guid doc) => string.Join(",", d.MiscariStoc.Where(m => m.DocumentId == doc)
            .SelectMany(m => m.Linii.Select(l => $"{m.MovementType}:{l.AccountId}:{l.BookValue:0.##}/{l.Quantity:0.##}")).Order());
        foreach (var (luna, _, stocuri) in luni)
            Console.WriteLine($"     MĂSURAT ({id}): stocuri luna {luna:00}, parțială [{Miscari(stocuri, a)}], intactă [{Miscari(stocuri, b)}]");
        Verifica(id, "stocuri ianuarie: mișcarea 90 numai valorică pe lot, 302 +80 (parțială) și +200 (intactă), fără capătul de consum",
            Miscari(luni[0].Stocuri, a) == $"90:{Stoc}:80/0" && Miscari(luni[0].Stocuri, b) == $"90:{Stoc}:200/0");
        Verifica(id, "stocuri februarie: compensarea la stornoul consumului revine pe lot, 90 cu +120",
            Miscari(luni[1].Stocuri, a) == $"90:{Stoc}:120/0");
        Verifica(id, "stocuri martie: stornoul corecției, 90 cu −200", Miscari(luni[2].Stocuri, a) == $"90:{Stoc}:-200/0");
        CuSocietate(() => {
            foreach (var luna in new[] { 1, 2, 3 }) {
                Fisier(id, $"L {luna:00}", CuSpatiu(os => SaftProiectii.SaftPeCub(os, An, luna, Zi(28, luna))));
                Fisier(id, $"S {luna:00}", CuSpatiu(os => SaftProiectii.SaftStocuriPeCub(os, An, luna, Zi(28, luna))));
            }
        });
        var primite = luni.SelectMany(l => l.Facturi.FacturiPrimite.Where(f => f.DocumentId == a || f.DocumentId == b)).ToList();
        Verifica(id, "facturile corecției nu poartă nicio linie cu cantitate inventată", primite.SelectMany(f => f.Linii).All(l => l.Quantity == 0m));
    }

    void CuSocietate(Action actiune) {
        var inainte = CuSpatiu(os => os.GetObjectsQuery<Societate>().Select(x => new {
            x.CodFiscal, x.InregistratTva, x.Tara, x.Denumire, x.ContactNume, x.ContactPrenume, x.Telefon, x.ContBancarId,
        }).First());
        var ibanInainte = CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban);
        Comanda(os => {
            var soc = os.GetObjectsQuery<Societate>().First();
            var banca = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA");
            banca.Iban = "RO49AAAA1B31007593840000";
            (soc.CodFiscal, soc.InregistratTva, soc.Tara, soc.Denumire) = ("12345674", true, "RO", "Atlas Probă CVL SRL");
            (soc.ContactNume, soc.ContactPrenume, soc.Telefon, soc.ContBancarId) = ("Popescu", "Ion", "0264000000", banca.ID);
            os.CommitChanges();
        });
        try { actiune(); }
        finally {
            Comanda(os => {
                var soc = os.GetObjectsQuery<Societate>().First();
                (soc.CodFiscal, soc.InregistratTva, soc.Tara, soc.Denumire) = (inainte.CodFiscal, inainte.InregistratTva, inainte.Tara, inainte.Denumire);
                (soc.ContactNume, soc.ContactPrenume, soc.Telefon, soc.ContBancarId) =
                    (inainte.ContactNume, inainte.ContactPrenume, inainte.Telefon, inainte.ContBancarId);
                os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban = ibanInainte;
                os.CommitChanges();
            });
        }
    }

    void Fisier(string id, string eticheta, SaftDto d) {
        if (d.Refuzuri.Count > 0) {
            Verifica(id, $"SAF-T {eticheta}: fișierul lunii fără refuzuri ({string.Join("; ", d.Refuzuri.Take(3).Select(r => r.Cod + " " + r.Mesaj))})", false);
            return;
        }
        var cale = Path.Combine(Duk.DirectorTemporar(), $"{Marcaj}-{eticheta.Replace(' ', '-')}.xml");
        using (var f = File.Create(cale))
            SaftXml.Scrie(d, f);
        var xsd = XsdD406.Valideaza(cale);
        var duk = Duk.Valideaza(cale);
        Verifica(id, $"SAF-T {eticheta}: XSD {(xsd.Count == 0 ? "valid" : string.Join("; ", xsd.Take(2)))}, DUK {duk.Rezumat}"
            + (duk.Valid ? "" : " — " + string.Join("; ", duk.Erori.Take(3))), xsd.Count == 0 && duk.Valid);
    }
}
