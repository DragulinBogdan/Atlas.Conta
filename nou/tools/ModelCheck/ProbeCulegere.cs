using System.Text.RegularExpressions;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Probele pasului 3 al deciziei 104 (c): culegerea are o singură sursă, iar adaptorii doar o cheamă.
static class ProbeCulegere {
    const string Marcaj = "E2E-C104C";

    // Adaptorii nu calculează culegerea singuri: nicio chemare a serviciilor ei din `Api/` sau `Controllers/`.
    static readonly (string Director, Regex Interzis, string[] Exceptii)[] Reguli = [
        ("Api", new(@"\b(LoturiCulegereService\.|TvaService\.CalculeazaValori|TvaService\.IncarcaTipuri|AmortizareService\.LiniiIesire|\.CalculeazaValori\(|VerificaScara\()"),
            ["ImperechereApply"]),
        ("Api", new(@"\bImpliciteService\.TipTva\b"), ["Implicite"]),
        ("Controllers", new(@"\b(LoturiCulegereService|TvaService|ImpliciteService)\.|\.CalculeazaValori\("), []),
    ];

    public static void VerificaSursa(Action<string, bool> check) {
        var modul = Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..",
            "Atlas.Conta.BackOffice", "Atlas.Conta.BackOffice.Module"));
        var incalcari = new List<string>();
        var fisiere = 0;
        foreach (var (director, interzis, exceptii) in Reguli) {
            foreach (var fisier in Directory.EnumerateFiles(Path.Combine(modul, director), "*.cs", SearchOption.AllDirectories)) {
                var relativ = Path.GetRelativePath(modul, fisier);
                if (exceptii.Any(e => relativ.Contains(e)))
                    continue;
                fisiere++;
                var linii = File.ReadAllLines(fisier);
                for (var i = 0; i < linii.Length; i++) {
                    var cod = linii[i].Split("//")[0];
                    if (interzis.IsMatch(cod))
                        incalcari.Add($"{relativ}:{i + 1}");
                }
            }
        }
        Console.WriteLine($"     MĂSURAT (104c-S1): {fisiere} fișiere de adaptor; încălcări [{string.Join("; ", incalcari)}].");
        check("104c-S1: `Api/*Apply` și controllerele XAF sunt adaptori — nu cheamă loturile, formula valorilor, "
            + "implicitul de TVA, generarea liniilor CAS sau garda de scară; le cheamă `CulegereDocument` și gardianul",
            fisiere > 20 && incalcari.Count == 0);
    }

    public static void Verifica(IObjectSpaceProvider provider, Action<string, bool> check, Action<string, Action> refuza) {
        using var os = provider.CreateObjectSpace();
        Curata(os);
        var tipStoc = os.GetObjectsQuery<TipMaterial>().First(t => t.Clasa.Natura == NaturaClasa.Stoc);
        var n9 = os.FirstOrDefault<TipTva>(t => t.Regim == RegimTva.Normal && t.Cota == 9m && t.Activ);
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-F";
        furnizor.Denumire = "Furnizor C104c";
        furnizor.Tara = "RO";
        furnizor.InregistratTva = true;
        var gestiune = os.CreateObject<Gestiune>();
        gestiune.Cod = Marcaj + "-G";
        gestiune.Denumire = "Gestiune C104c";
        var produs = os.CreateObject<Produs>();
        produs.Cod = Marcaj + "-P";
        produs.Denumire = "Produs C104c";
        produs.UM = "BUC";
        produs.TipMaterial = tipStoc;
        produs.TipTvaImplicit = n9;
        os.CommitChanges();
        var data = new DateOnly(2026, 3, 12);

        try {
            // ── Calea API ──
            var idApi = FacturaIntrareApply.Aplica(os, null, new FacturaIntrareWriteDto {
                Numar = Marcaj + "-API", Data = data, PredatorId = furnizor.ID, PrimitorId = gestiune.ID,
                Linii = { new FacturaIntrareLinieWriteDto { ProdusId = produs.ID, Cantitate = 2m, PretUnitar = 50m } },
            });
            var api = os.GetObjectByKey<FacturaIntrare>(idApi);
            var linieApi = api.Detalii.OfType<FacturaIntrareDetaliu>().Single();

            // ── Calea XAF: evenimentele ecranului, apoi Committing-ul ──
            var xaf = os.CreateObject<FacturaIntrare>();
            CulegereDocument.Nou(xaf);
            var azi = xaf.Data;
            xaf.Data = data;
            CulegereDocument.DataMutata(xaf, azi);
            xaf.Numar = Marcaj + "-XAF";
            xaf.Predator = furnizor;
            xaf.Primitor = gestiune;
            var linieXaf = os.CreateObject<FacturaIntrareDetaliu>();
            linieXaf.Document = xaf;
            xaf.Detalii.Add(linieXaf);
            linieXaf.Produs = produs;
            CulegereDocument.LinieSchimbata(os, xaf, linieXaf, nameof(FacturaIntrareDetaliu.Produs));
            linieXaf.Cantitate = 2m;
            CulegereDocument.LinieSchimbata(os, xaf, linieXaf, nameof(DocumentDetaliu.Cantitate));
            linieXaf.PretUnitar = 50m;
            CulegereDocument.LinieSchimbata(os, xaf, linieXaf, nameof(FacturaIntrareDetaliu.PretUnitar));
            var inainteDeSalvare = (linieXaf.Valoare, linieXaf.ValoareTva);
            CulegereDocument.InainteDeSalvare(os);
            os.CommitChanges();

            Console.WriteLine($"     MĂSURAT (104c-E1): API {linieApi.Valoare}/{linieApi.ValoareTva} tip {linieApi.TipMaterialId == tipStoc.ID} "
                + $"TVA {linieApi.TipTvaId} lot {linieApi.LotId != null}; XAF {linieXaf.Valoare}/{linieXaf.ValoareTva} "
                + $"(pe ecran {inainteDeSalvare}) TVA {linieXaf.TipTvaId} lot {linieXaf.LotId != null}; N9 {n9?.ID}.");
            check("104c-E1: aceeași culegere pe calea XAF (evenimente + Committing) și pe calea API dă aceeași linie: "
                + "tipul din produs, TipTva implicit cu cota produsului, valorile afișate înaintea salvării, lotul născut",
                linieApi.TipMaterialId == tipStoc.ID && linieXaf.TipMaterialId == tipStoc.ID
                && linieApi.TipTvaId != null && linieApi.TipTvaId == linieXaf.TipTvaId
                && (n9 == null || linieApi.TipTvaId == n9.ID)
                && linieApi.Valoare == 100m && linieXaf.Valoare == 100m
                && linieApi.ValoareTva == linieXaf.ValoareTva && linieApi.ValoareTva > 0m
                && inainteDeSalvare == (linieXaf.Valoare, linieXaf.ValoareTva)
                && linieApi.LotId != null && linieXaf.LotId != null
                && xaf.DataInregistrare == data);

            check("104c-E2: `DataPrimire` necules rămâne gol (înseamnă data înregistrării), nu se materializează la culegere",
                api.DataPrimire == null && FacturaIntrareApply.Citeste(os, idApi).DataPrimire == api.DataInregistrare);

            // ── TVA-ul cules: câmpul prezent e intenția, 0 nu e TVA cules (104c, pasul 3) ──
            var normal = os.GetObjectsQuery<TipTva>()
                .Where(t => t.Regim == RegimTva.Normal && t.Cota > 0m && t.Activ).OrderBy(t => t.Cod).FirstOrDefault();
            if (normal != null) {
                FacturaIntrareWriteDto Tva(decimal? valoareTva, decimal pret, Guid? linieId) => new() {
                    Numar = Marcaj + "-T", Data = data, PredatorId = furnizor.ID, PrimitorId = gestiune.ID,
                    Linii = { new FacturaIntrareLinieWriteDto { Id = linieId, ProdusId = produs.ID, TipTvaId = normal.ID,
                        Cantitate = 1m, PretUnitar = pret, ValoareTva = valoareTva } },
                };
                var idT = FacturaIntrareApply.Aplica(os, null, Tva(null, 100m, null));
                var linieT = os.GetObjectByKey<FacturaIntrare>(idT).Detalii.Single();
                var absent = linieT.ValoareTva;
                FacturaIntrareApply.Aplica(os, idT, Tva(17m, 100m, linieT.ID));
                var cules = linieT.ValoareTva;
                FacturaIntrareApply.Aplica(os, idT, Tva(null, 100m, linieT.ID));
                var pastrat = linieT.ValoareTva;
                FacturaIntrareApply.Aplica(os, idT, Tva(null, 200m, linieT.ID));
                var recalculat = linieT.ValoareTva;
                Console.WriteLine($"     MĂSURAT (104c-T1): {normal.Cod} pe 100: absent {absent}, cules 17 → {cules}, "
                    + $"re-salvat fără câmp {pastrat}, baza 200 fără câmp {recalculat}.");
                check("104c-T1: TVA-ul cules pe API: absent = din cotă; explicit 17 rămâne 17 și la re-salvarea fără câmp; "
                    + "schimbarea bazei fără valoare nouă îl recalculează",
                    absent == normal.Cota && cules == 17m && pastrat == 17m && recalculat == 2 * normal.Cota);

                using var osZero = provider.CreateObjectSpace();
                refuza("104c-T2: TVA explicit 0 pe o linie a cărei cotă dă taxă e refuzat, nu înlocuit tăcut cu TVA-ul din cotă",
                    () => FacturaIntrareApply.Aplica(osZero, idT, Tva(0m, 200m, linieT.ID)));
                osZero.Rollback();
                var linieZero = osZero.GetObjectByKey<FacturaIntrare>(idT).Detalii.Single();
                linieZero.ValoareTva = 0m;
                CulegereDocument.LinieSchimbata(osZero, linieZero.Document, linieZero, nameof(DocumentDetaliu.ValoareTva));
                check("104c-T3: în ecranul XAF, TVA-ul adus la 0 revine vizibil la cotă (aceeași regulă ca pe API)",
                    linieZero.ValoareTva == 2 * normal.Cota);
                osZero.Rollback();
            }

            // ── Regulile culegerii sunt ale gardianului, pe orice ușă ──
            using var osGard = provider.CreateObjectSpace();
            new GardianEditare().OnObjectSpaceCreated(osGard);
            refuza("104c-V1: linia fără tip și fără produs e refuzată de gardian („Tipul (contul/clasa) liniei este obligatoriu.”)",
                () => FacturaIntrareApply.Aplica(osGard, null, new FacturaIntrareWriteDto {
                    Numar = Marcaj + "-V1", Data = data, PredatorId = furnizor.ID, PrimitorId = gestiune.ID,
                    Linii = { new FacturaIntrareLinieWriteDto { Cantitate = 1m, PretUnitar = 10m } },
                }));
            osGard.Rollback();
            refuza("104c-V2: cantitatea cu 4 zecimale e refuzată de gardian din scara coloanei (49e), nu de Postgres",
                () => FacturaIntrareApply.Aplica(osGard, null, new FacturaIntrareWriteDto {
                    Numar = Marcaj + "-V2", Data = data, PredatorId = furnizor.ID, PrimitorId = gestiune.ID,
                    Linii = { new FacturaIntrareLinieWriteDto { TipMaterialId = tipStoc.ID, ProdusId = produs.ID,
                        Cantitate = 1.2345m, PretUnitar = 10m } },
                }));
            osGard.Rollback();
        }
        finally {
            Curata(os);
        }
    }

    static void Curata(IObjectSpace os) {
        var pj = new Purja(os);
        pj.Adauga(os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Numar.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)));
        pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)));
        pj.Executa();
    }
}
