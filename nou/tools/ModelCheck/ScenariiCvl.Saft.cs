using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Saft;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SC-CVL-32 (CV-D9, CV-r1): exportul SAF-T rămâne pe refuz în lunile în care o corecție e operată sau stornată.
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
        Verifica(id, "D406 ianuarie: corecția parțială, cu contrapartide pe stoc și pe consum, e refuzată cu SAFT_PROVENIENTA_AMBIGUA",
            Pe(ianuarie, a).Any(r => r.Cod == SaftProiectii.RefuzProvenienta));
        Verifica(id, "D406 ianuarie: corecția lotului intact, cu cantitate comercială 0, e refuzată cu SAFT_SURSA_INCOMPLETA",
            Pe(ianuarie, b).Any(r => r.Cod == SaftProiectii.RefuzSursa));
        var compensare = februarie.Jurnale.Where(j => j.Tranzactii.Any(t => t.DocumentId == a)).ToList();
        Verifica(id, "D406 februarie: numai compensarea, fără refuz pe documentele cazului, în GL sub jurnalul FCT și fără factură",
            Pe(februarie, [.. aleCazului]).Count == 0 && februarie.Refuzuri.All(r => r.DocumentId != null)
            && compensare.Count == 1 && compensare[0].JournalID == "FCT"
            && compensare[0].Tranzactii.Where(t => t.DocumentId == a).SelectMany(t => t.Linii).Sum(l => Math.Abs(l.Amount)) == 240m
            && !februarie.FacturiPrimite.Any(f => f.DocumentId == a));
        Verifica(id, "D406 martie: evenimentul de storno al corecției e refuzat", Pe(martie, a).Count > 0);
        Verifica(id, "declarația de stocuri, în fiecare lună: SAFT_MISCARE_VALORICA pe corecție",
            luni.All(l => Pe(l.Stocuri, a).Any(r => r.Cod == SaftProiectii.RefuzMiscareValorica)));
        Verifica(id, "la refuz nu se emite fișier: D406 ianuarie și martie și declarațiile de stocuri",
            new[] { ianuarie, martie }.Concat(luni.Select(l => l.Stocuri)).All(NuSeScrie));
        var primite = luni.SelectMany(l => l.Facturi.FacturiPrimite.Where(f => f.DocumentId == a || f.DocumentId == b)
            .Select(f => (l.Luna, Factura: f))).ToList();
        foreach (var (luna, f) in primite)
            Console.WriteLine($"     MĂSURAT ({id}): luna {luna:00}, factura primită {f.InvoiceNo} {f.InvoiceType}, storno {f.Storno}: "
                + string.Join("; ", f.Linii.Select(l => $"cantitate {l.Quantity}")));
        Verifica(id, "factura corecției, refuzată, nu poartă nicio linie cu cantitate inventată",
            primite.All(x => x.Factura.Linii.All(l => l.Quantity == 0m)));
    }

    static bool NuSeScrie(SaftDto d) {
        try {
            using var ms = new MemoryStream();
            SaftXml.Scrie(d, ms);
            return false;
        }
        catch (InvalidOperationException) { return true; }
    }
}
