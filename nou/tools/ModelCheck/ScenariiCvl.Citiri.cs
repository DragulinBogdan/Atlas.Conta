using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// SC-CVL-20: cititorii de produs pe cazul parțial, înainte și după corecție, pe citirea din mișcări și pe cea din snapshot.
sealed partial class ScenariiCvl {
    sealed record CazCititor(Caz Caz, Guid Furnizor, Guid Produs, Guid ContConsum);

    T CuFurnizor<T>(Guid furnizor, Func<T> actiune) {
        var scena = Furnizor;
        Furnizor = furnizor;
        try { return actiune(); }
        finally { Furnizor = scena; }
    }

    // Partenerul propriu izolează datoria cazului în balanță și în fișă.
    CazCititor PregatesteCititori() {
        var furnizor = CuSpatiu(os => {
            var p = os.CreateObject<Partener>(); p.Cod = Marcaj + "-FURN-20"; p.Denumire = p.Cod;
            os.CommitChanges(); return p.ID;
        });
        var caz = CuFurnizor(furnizor, Partial);
        return CuSpatiu(os => new CazCititor(caz, furnizor, os.GetObjectByKey<Lot>(caz.Lot).ProdusId,
            os.GetObjectsQuery<C.Postare>().First(p => p.Unitate == caz.Lot && p.Repartitor == Loc).Cont));
    }

    void Cititori(CazCititor cititor) {
        const string id = "SC-CVL-20";
        var caz = cititor.Caz;
        var (inceput, sfarsit) = (Zi(1, 2), Zi(28, 2));
        CititoriEgali(id, "înaintea corecției", cititor, inceput, sfarsit, (400m, 0m), (600m, 0m), (1000m, 0m));
        var a = CuFurnizor(cititor.Furnizor, () => Corectie(caz, 200m, 5, 2));
        CititoriEgali(id, "după corecție", cititor, inceput, sfarsit, (400m, 80m), (600m, 120m), (1000m, 200m));
        Este(id, "lectura la 31.01 rămâne cea dinaintea corecției", caz, 40m, 400m, 600m, 1000m, Zi(31));
        Verifica(id, "explicația: ponderi 40 și 60, cote 80 și 120",
            Alocari(a).Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(40m, 80m), (60m, 120m)]));

        var numar = CuSpatiu(os => os.GetObjectByKey<Document>(a).Numar);
        var fise = CuSpatiu(os => new[] {
            ContabilProiectii.FisaCont(os, Cont(Stoc), inceput, sfarsit, materialId: cititor.Produs).ToList(),
            ContabilProiectii.FisaCont(os, cititor.ContConsum, inceput, sfarsit, materialId: cititor.Produs).ToList(),
            ContabilProiectii.FisaCont(os, Cont(ContFurnizor), inceput, sfarsit, repartitorId: cititor.Furnizor).ToList(),
        });
        Console.WriteLine($"     MĂSURAT ({id}): fișele din februarie: " + string.Join("; ", fise.Select(f =>
            string.Join(", ", f.Select(r => $"{r.NumarNota} D{r.Debit}/C{r.Credit} sold {r.SoldCurent}")))));
        Verifica(id, "fișele poartă corecția sub numărul ei: stocul 80, consumul 120, furnizorul câte un credit pe alocare, 80 și 120",
            fise.All(f => f.Count > 0 && f.All(r => r.NumarNota == numar && r.DocumentId == a))
            && fise[0].Select(r => (r.Debit, r.SoldCurent)).SequenceEqual([(80m, 480m)])
            && fise[1].Select(r => (r.Debit, r.SoldCurent)).SequenceEqual([(120m, 720m)])
            && fise[2].Select(r => r.Credit).SequenceEqual([80m, 120m]) && fise[2][^1].SoldCurent == -1200m);

        var tranzactii = CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Count());
        var inainte = Snapshoturi();
        var raport = CuSpatiu(os => PerioadeApply.Reconstruieste(os)).Referinte;
        Console.WriteLine($"     MĂSURAT ({id}): reconstrucția a recalculat {raport.Length} referințe; diferențe "
            + string.Join(", ", raport.Select(r => $"{r.An}-{r.Luna:00}: {r.ContabilDiferite}/{r.StocDiferite}/{r.PartideDiferite}")));
        Verifica(id, "reconstrucția snapshot-ului găsește zero diferențe, nu scrie tranzacții și lasă snapshot-ul identic",
            raport.Length > 0 && raport.All(r => r.ContabilDiferite == 0 && r.StocDiferite == 0 && r.PartideDiferite == 0)
            && CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Count()) == tranzactii && Snapshoturi() == inainte);
        Verifica(id, "documentele cazului rămân operate", caz.Documente.All(d => StareaDocumentului(d) == StareDocument.Operat));
    }

    // Balanța și fișa pe stoc, consum și datorie, din mișcări și din snapshot, egale cu suma faptelor.
    void CititoriEgali(string id, string pas, CazCititor cititor, DateOnly inceput, DateOnly sfarsit,
            (decimal Initial, decimal Rulaj) stoc, (decimal Initial, decimal Rulaj) consum, (decimal Initial, decimal Rulaj) datorie) {
        var fapte = Citeste(cititor.Caz, sfarsit);
        Verifica(id, $"{pas}: suma faptelor {fapte}",
            fapte == new Stare(40m, stoc.Initial + stoc.Rulaj, consum.Initial + consum.Rulaj, datorie.Initial + datorie.Rulaj));
        foreach (var citire in new[] { C.Citiri.CitireCumul.Vizibila, C.Citiri.CitireCumul.Integrala }) {
            var (pe, fisa) = CuSpatiu(os => {
                decimal[] Balanta(Guid cont, Guid? material, Guid? repartitor, int semn) =>
                    ContabilProiectii.Balanta(os, inceput, sfarsit, materialId: material, repartitorId: repartitor, citire: citire)
                        .Where(r => r.ContId == cont).ToList() is var r && r.Count == 1
                        ? [semn * (r[0].InitialDebit - r[0].InitialCredit), semn * (r[0].RulajDebit - r[0].RulajCredit)]
                        : [];
                decimal[] Fisa(Guid cont, Guid? material, Guid? repartitor, int semn) =>
                    ContabilProiectii.FisaSold(os, cont, inceput, sfarsit, materialId: material, repartitorId: repartitor, citire: citire) is var f
                        ? [semn * (f.SoldInitialDebit - f.SoldInitialCredit), semn * (f.RulajDebit - f.RulajCredit)]
                        : [];
                return (new[] {
                    Balanta(Cont(Stoc), cititor.Produs, null, 1), Balanta(cititor.ContConsum, cititor.Produs, null, 1),
                    Balanta(Cont(ContFurnizor), null, cititor.Furnizor, -1),
                }, new[] {
                    Fisa(Cont(Stoc), cititor.Produs, null, 1), Fisa(cititor.ContConsum, cititor.Produs, null, 1),
                    Fisa(Cont(ContFurnizor), null, cititor.Furnizor, -1),
                });
            });
            decimal[][] asteptat = [[stoc.Initial, stoc.Rulaj], [consum.Initial, consum.Rulaj], [datorie.Initial, datorie.Rulaj]];
            string Text(decimal[][] v) => string.Join("; ", v.Select(x => string.Join("/", x)));
            Verifica(id, $"{pas}, citirea {citire}: balanța și fișa pe stoc, consum și datorie dau {Text(asteptat)} (balanța {Text(pe)}, fișa {Text(fisa)})",
                pe.Zip(asteptat).All(x => x.First.SequenceEqual(x.Second)) && fisa.Zip(asteptat).All(x => x.First.SequenceEqual(x.Second)));
        }
    }

    string Snapshoturi() => CuSpatiu(os => string.Join("\n",
        os.GetObjectsQuery<SoldPerioadaStoc>().Where(s => s.An == An).ToList()
            .Select(s => $"S {s.Luna} {s.LotId} {s.ContId} {s.GestiuneId} {s.Cantitate} {s.Valoare}")
            .Concat(os.GetObjectsQuery<SoldPerioadaContabil>().Where(s => s.An == An).ToList()
                .Select(s => $"C {s.Luna} {s.ContId} {s.RepartitorId} {s.MaterialId} {s.CodEconomicId} {s.Debit} {s.Credit}"))
            .Order()));
}
