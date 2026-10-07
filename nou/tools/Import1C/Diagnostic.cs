using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Import1C;

// PASUL 6: interogarea ȚINTITĂ pe care o cere diagnosticul contractului picat —
// „ce mișcări are Atlas pe produsul ăsta, pe ce document, în ce gestiune".
//
// De ce în unealtă și nu ad-hoc: identitatea produsului e hex-ul 1C, iar
// traducerea lui în Atlas trece prin `MigrareLegatura`; o interogare SQL scrisă
// de mână ar trebui să refacă exact aceeași traducere și ar putea diverge tăcut
// de reconciliere fix acolo unde se caută cauza. Rulează imediat după seed
// (`--diag=<hexProdus>`), înaintea oricărei faze scumpe, și oprește procesul.
static class Diagnostic {
    public static void Produs(IObjectSpaceProvider provider, string hexProdus, Action<string> avert) {
        using var os = provider.CreateObjectSpace();
        var hex = hexProdus.ToUpperInvariant();
        // Un nomenclator 1C poate avea MAI MULTE produse Atlas — câte unul per
        // cont de stoc pe care sursa îl ține (`ImportLaCerere`). Diagnosticul le
        // arată pe toate: exact contradicția dintre gemeni e ce se caută aici.
        var produseId = Legaturi.Incarca(os, ImportLaCerere.ViewProduse)
            .Where(x => ImportLaCerere.NomenclatorDinCheie(x.Key) == hex)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => x.Value)
            .ToList();
        if (produseId.Count == 0) {
            Console.WriteLine($"Produsul 1C {hexProdus} nu are legătură în Atlas (n-a fost importat).");
            return;
        }
        foreach (var produsId in produseId) {
            var produs = os.GetObjectByKey<Produs>(produsId);
            Console.WriteLine($"Produs 1C {hexProdus} → Atlas {produsId} „{produs?.Denumire}” "
                + $"(cod {produs?.Cod}, tip {produs?.TipMaterial?.Cod}).");
        }

        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => produseId.Contains(l.ProdusId))
            .Select(l => new { l.ID, l.Data, l.PretUnitar, Gestiune = l.Gestiune.Denumire })
            .ToList();
        var cheiLot = Reconciliere.Inverseaza(os, "Lot", avert);
        Console.WriteLine($"Loturi: {loturi.Count}");
        foreach (var l in loturi)
            Console.WriteLine($"   {l.ID} {l.Data:yyyy-MM-dd} preț {l.PretUnitar,12:N4} "
                + $"gestiune-naștere „{l.Gestiune}” cheie 1C {cheiLot.GetValueOrDefault(l.ID) ?? "(fără)"}");

        var ids = loturi.Select(l => (Guid?)l.ID).ToList();
        var simbolPeCont = os.GetObjectsQuery<Cont>().Select(c => new { c.ID, c.Simbol }).ToList()
            .ToDictionary(c => c.ID, c => c.Simbol);
        var gestiuni = os.GetObjectsQuery<Gestiune>().Select(g => new { g.ID, g.Denumire }).ToList()
            .ToDictionary(g => g.ID, g => g.Denumire);
        var postari = Loturi.Postari(os)
            .Where(p => ids.Contains(p.Unitate))
            .Select(p => new { p.Data, p.Cont, p.Gestiune, p.Cantitate, p.Latura, p.Valoare, p.Tranzactie.Fel, p.DocumentId })
            .ToList();
        var idsDocumente = postari.Select(p => p.DocumentId).Distinct().ToList();
        var documente = os.GetObjectsQuery<Document>()
            .Where(d => idsDocumente.Contains(d.ID))
            .Select(d => new { d.ID, d.Numar, Tip = d.GetType().Name })
            .ToList()
            .ToDictionary(d => d.ID);
        var randuri = postari
            .Select(p => new {
                p.Data, Cont = simbolPeCont.GetValueOrDefault(p.Cont),
                Gestiune = gestiuni.GetValueOrDefault(p.Gestiune.Value), p.Cantitate,
                Valoare = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare,
                Storno = p.Fel == N.FelTranzactie.Storno,
                Document = p.DocumentId is { } id && documente.TryGetValue(id, out var d) ? $"{d.Tip} {d.Numar}" : "deschidere",
            })
            .OrderBy(r => r.Data).ThenBy(r => r.Document, StringComparer.Ordinal)
            .ToList();
        Console.WriteLine($"Postări pe loturi: {randuri.Count}");
        foreach (var r in randuri)
            Console.WriteLine($"   {r.Data:yyyy-MM-dd} {r.Cont,-8} {r.Cantitate,10:N3} buc "
                + $"{r.Valoare,14:N2} lei  gestiune „{r.Gestiune}”  {r.Document}"
                + (r.Storno ? "  [STORNO]" : ""));
        Console.WriteLine($"Sold pe gestiune: " + string.Join("; ", randuri
            .GroupBy(r => r.Gestiune)
            .Select(g => $"„{g.Key}” {g.Sum(r => r.Cantitate):N3} buc / {g.Sum(r => r.Valoare):N2} lei")));

        // Ce a ARUNCAT unealta pe produsul ăsta: registrul divergențelor, adică
        // exact ce folosește contractul lunar ca să justifice (sau nu) diferența.
        // Fără el, diagnosticul unei chei picate ar rămâne la „Atlas are mai mult,
        // nu se știe de ce".
        var registru = new RegistruDivergente();
        registru.Incarca(os, avert);
        var ale = registru.PanaLa(9999, 12).Where(d => d.ProdusHex == hex)
            .OrderBy(d => d.An).ThenBy(d => d.Luna).ThenBy(d => d.Sursa, StringComparer.Ordinal)
            .ToList();
        Console.WriteLine($"Divergențe înregistrate pe produs: {ale.Count}");
        foreach (var d in ale)
            Console.WriteLine($"   {d.An}-{d.Luna:00} gestiune {d.DepozitHex} "
                + $"{d.Cantitate,10:N3} buc {d.Valoare,14:N2} lei  {d.Sursa}"
                + (d.ValoareNepostata != 0m
                    ? $"  nepostat {d.ValoareNepostata:N2} pe {d.ContDebit} = {d.ContCredit}" : "")
                + $"  — {d.Categorie}");
        foreach (var g in ale.GroupBy(d => (d.An, d.Luna, d.DepozitHex))
                     .OrderBy(g => g.Key.An).ThenBy(g => g.Key.Luna))
            Console.WriteLine($"   Σ {g.Key.An}-{g.Key.Luna:00} gestiune {g.Key.DepozitHex}: "
                + $"{g.Sum(d => d.Cantitate):N3} buc / {g.Sum(d => d.Valoare):N2} lei "
                + $"(cumulat la lună, contractul le adună de la începutul anului)");
    }
}
