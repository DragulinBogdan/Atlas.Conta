#nullable enable
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Gate-ul READ-ONLY de dinaintea comutării (S-D9.1): pe fiecare document OPERAT
/// al tipurilor cerute, declarația frunzei contra registrelor normalizate (B-D8).
/// Nu scrie nimic — nicio comitere, niciun rând.
/// </summary>
static class GatePeBaza {
    const int Lot = 200;

    sealed class Contor {
        public int Documente;
        public int FaraDeclarant;
        public int Egale;
        public int Refuzate;
        public int Diferite;
        public int Exceptii;
        public readonly Dictionary<string, (int Cate, HashSet<Guid> Documente, List<string> Exemple)> Refuzuri = [];
        public readonly Dictionary<string, (int Cate, List<string> Exemple, string Diff)> Feluri = [];
        public readonly Dictionary<string, int> Avertismente = [];
        public readonly List<string> TextExceptii = [];
        public readonly List<string> Conservare = [];
    }

    /// <summary>Abaterea taxei culese de cea decisă de nucleu, per document × cotă (B-r1).</summary>
    sealed record Abatere(
        Guid Document, string Eticheta, string Cota, int Linii, decimal Culeasa, decimal Nucleu) {
        public decimal Valoare => Math.Abs(Culeasa - Nucleu);
    }

    public static async Task<int> Ruleaza(string baza, IReadOnlyList<string> coduri, string caleRaport) {
        ArgumentNullException.ThrowIfNull(baza);
        ArgumentNullException.ThrowIfNull(coduri);
        var connectionString = "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=" + baza;
        var jurnal = new StringBuilder();
        void Scrie(string linie) {
            Console.WriteLine(linie);
            jurnal.AppendLine(linie);
        }

        using (var ctx = new BackOfficeEFCoreDbContext(new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>()
                .UseNpgsql(connectionString).UseChangeTrackingProxies().Options)) {
            if (!await ctx.Database.CanConnectAsync()) {
                Console.WriteLine($"Baza „{baza}” nu există sau nu răspunde.");
                return 2;
            }
            var pending = (await ctx.Database.GetPendingMigrationsAsync()).ToList();
            if (pending.Count > 0) {
                Console.WriteLine($"Baza „{baza}” are {pending.Count} migrații în așteptare "
                    + $"({string.Join(", ", pending)}); gate-ul e READ-ONLY — aplicați-le pe CLONĂ întâi.");
                return 2;
            }
            var conventie = ctx.SetariProfil.AsNoTracking()
                .Select(s => (MidpointRounding?)s.RotunjireBani).FirstOrDefault();
            if (conventie is MidpointRounding valoare)
                Scara.FixeazaConventia(valoare);
            Scrie($"Baza: {baza}; convenția de rotunjire: {Scara.ConventieBani}"
                + (conventie == null ? " (fără rând SetareProfil — default)" : " (din SetareProfil)"));
        }

        using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
            (builder, _) => builder
                .UseNpgsql(connectionString)
                .UseChangeTrackingProxies()
                .UseObjectSpaceLinkProxies()
                .UseLazyLoadingProxies());

        Dictionary<string, string> clrPerCod;
        Dictionary<string, string> tintaConex;
        Dictionary<string, N.DirectieTva> directiePerCod;
        Dictionary<Guid, (N.RegimTva Regim, decimal Cota)> tipuriTva;
        using (var os = provider.CreateObjectSpace()) {
            var tipuri = os.GetObjectsQuery<TipDocument>().Select(t => new { t.ID, t.Cod, t.ClrType }).ToList();
            clrPerCod = tipuri.Where(t => t.ClrType != null).ToDictionary(t => t.Cod, t => t.ClrType!);
            tintaConex = os.GetObjectsQuery<PoliticaConex>()
                .Select(p => new { p.TipDocumentSursaId, p.TipDocumentTintaId })
                .ToList()
                .Select(p => (
                    Sursa: tipuri.FirstOrDefault(t => t.ID == p.TipDocumentSursaId)?.Cod,
                    Tinta: tipuri.FirstOrDefault(t => t.ID == p.TipDocumentTintaId)?.ClrType))
                .Where(p => p.Sursa != null && p.Tinta != null)
                .ToDictionary(p => p.Sursa!, p => p.Tinta!);
            directiePerCod = os.GetObjectsQuery<PoliticaTva>()
                .Select(p => new { p.TipDocumentId, p.Directie })
                .ToList()
                .Select(p => (Cod: tipuri.FirstOrDefault(t => t.ID == p.TipDocumentId)?.Cod, p.Directie))
                .Where(p => p.Cod != null)
                .ToDictionary(p => p.Cod!, p => (N.DirectieTva)(int)p.Directie);
            tipuriTva = os.GetObjectsQuery<TipTva>()
                .Select(t => new { t.ID, t.Regim, t.Cota })
                .ToList()
                .ToDictionary(t => t.ID, t => ((N.RegimTva)(int)t.Regim, t.Cota));
        }

        var cronometru = Stopwatch.StartNew();
        var contori = new Dictionary<string, Contor>();
        var abateri = new List<Abatere>();
        var regulaLipsa = new List<(Guid Document, Guid? Linie)>();
        var nescris = true;

        foreach (var cod in coduri) {
            if (!clrPerCod.TryGetValue(cod, out var clrType)) {
                Scrie($"Tipul „{cod}” nu există în `TipuriDocument` pe baza asta — sărit.");
                continue;
            }
            var contor = new Contor();
            contori[cod] = contor;
            List<Guid> ids;
            using (var os = provider.CreateObjectSpace())
                ids = os.GetObjectsQuery<Document>()
                    .Where(d => d.ClrType == clrType && d.Stare == StareDocument.Operat)
                    .OrderBy(d => d.ID)
                    .Select(d => d.ID)
                    .ToList();
            contor.Documente = ids.Count;
            Scrie($"\n── {cod} ({clrType}): {ids.Count} documente operate ──");

            for (var inceput = 0; inceput < ids.Count; inceput += Lot) {
                var alLotului = ids.Skip(inceput).Take(Lot).ToList();
                using var os = provider.CreateObjectSpace();
                var documente = os.GetObjectsQuery<Document>()
                    .Where(d => alLotului.Contains(d.ID))
                    .ToList()
                    .OrderBy(d => alLotului.IndexOf(d.ID))
                    .ToList();
                var conexe = Conexele(os, cod, tintaConex, alLotului);
                foreach (var doc in documente)
                    Masoara(os, doc, conexe,
                        directiePerCod.TryGetValue(cod, out var directie) ? directie : null, tipuriTva,
                        contor, abateri, regulaLipsa);
                if (os.IsModified) {
                    nescris = false;
                    Scrie($"   ATENȚIE: ObjectSpace-ul lotului {inceput / Lot} are modificări — "
                        + "gate-ul trebuie să fie READ-ONLY.");
                }
                if ((inceput / Lot) % 25 == 0 || inceput + Lot >= ids.Count)
                    Console.WriteLine($"   … {Math.Min(inceput + Lot, ids.Count)}/{ids.Count} "
                        + $"({cronometru.Elapsed:hh\\:mm\\:ss}; egale {contor.Egale}, refuzate {contor.Refuzate}, "
                        + $"diferite {contor.Diferite}, excepții {contor.Exceptii})");
            }
            Raporteaza(Scrie, cod, contor);
        }

        RaporteazaTva(Scrie, abateri);
        RaporteazaRegulaLipsa(Scrie, provider, regulaLipsa);

        Scrie($"\nDurata totală: {cronometru.Elapsed:hh\\:mm\\:ss}; "
            + $"memorie de vârf a procesului: {Process.GetCurrentProcess().PeakWorkingSet64 / 1024 / 1024} MB; "
            + $"read-only: {(nescris ? "DA" : "NU — vezi avertismentele de mai sus")}.");

        var total = contori.Values.Sum(c => c.Diferite + c.Refuzate + c.Exceptii);
        Scrie(total == 0
            ? "\nVERDICT: 100 % egal pe tipurile cerute."
            : $"\nVERDICT: {total} documente NU sunt egale (refuzate + diferite + excepții) — S-D12 (d).");

        Directory.CreateDirectory(Path.GetDirectoryName(caleRaport)!);
        await File.WriteAllTextAsync(caleRaport, jurnal.ToString(), new UTF8Encoding(false));
        Console.WriteLine($"\nRaport: {caleRaport}");
        return total == 0 ? 0 : 1;
    }

    // Conexul autogenerat al fiecărui document al lotului (TR-D3): registrele lui
    // sunt ale aceleiași fizici, deci intră în oracol. Documentul SECUNDAR (plata
    // automată) e tot autogenerat, dar are declarația lui — se recunoaște după
    // clasa-țintă a politicii de conex.
    static Dictionary<Guid, Guid> Conexele(
            IObjectSpace os, string cod, IReadOnlyDictionary<string, string> tintaConex,
            IReadOnlyList<Guid> documente) {
        if (!tintaConex.TryGetValue(cod, out var clrTinta))
            return [];
        return os.GetObjectsQuery<Document>()
            .Where(c => c.Autogenerat && c.ClrType == clrTinta
                && c.DocumentSursaId != null && documente.Contains(c.DocumentSursaId.Value))
            .Select(c => new { c.ID, c.DocumentSursaId })
            .ToList()
            .ToDictionary(c => c.ID, c => c.DocumentSursaId!.Value);
    }

    static void Masoara(
            IObjectSpace os,
            Document doc,
            IReadOnlyDictionary<Guid, Guid> conexe,
            N.DirectieTva? directie,
            IReadOnlyDictionary<Guid, (N.RegimTva Regim, decimal Cota)> tipuriTva,
            Contor contor,
            List<Abatere> abateri,
            List<(Guid, Guid?)> regulaLipsa) {
        var eticheta = $"{doc.Numar ?? "—"} [{doc.ID.ToString()[..8]}]";
        try {
            if (doc.Declarant() is null) {
                contor.FaraDeclarant++;
                return;
            }
            if (directie is { } sensul)
                AbaterileTaxei(doc, eticheta, sensul, tipuriTva, abateri);
            var contract = Contractare.Contracteaza(os, doc);
            if (!contract.EsteAcceptat) {
                contor.Refuzate++;
                foreach (var refuz in contract.Refuzuri) {
                    var (cate, documente, exemple) = contor.Refuzuri.GetValueOrDefault(
                        refuz.Cod, (0, new HashSet<Guid>(), new List<string>()));
                    documente.Add(doc.ID);
                    if (exemple.Count < 5)
                        exemple.Add($"{eticheta} {refuz.Mesaj}");
                    contor.Refuzuri[refuz.Cod] = (cate + 1, documente, exemple);
                    if (refuz.Cod == CoduriRefuz.RegulaContareLipsa && regulaLipsa.Count < 2000)
                        regulaLipsa.Add((doc.ID, refuz.Linie));
                }
                return;
            }

            var aleLui = new List<Guid> { doc.ID };
            var conexeAleLui = new Dictionary<Guid, Guid>();
            foreach (var (conex, sursa) in conexe)
                if (sursa == doc.ID) {
                    aleLui.Add(conex);
                    conexeAleLui[conex] = sursa;
                }

            Normalizari.Reseteaza();
            var oracol = Normalizari.Toate(
                CubDinRegistre.Transforma(os, aleLui),
                Normalizari.Citeste(os, aleLui, conexeAleLui));
            foreach (var avertisment in Normalizari.Avertismente.Distinct())
                contor.Avertismente[Sablon(avertisment)] =
                    contor.Avertismente.GetValueOrDefault(Sablon(avertisment)) + 1;

            var conservare = N.Conservare.Verifica(contract.Tranzactie!);
            if (conservare.Count > 0 && contor.Conservare.Count < 5)
                contor.Conservare.Add($"{eticheta}: {string.Join("; ", conservare.Select(r => r.Cod))}");

            var nume = ProbeNucleu.Nume(os, oracol, contract.Tranzactie!);
            var raport = Comparabil.Compara(
                Comparabil.Proiecteaza(oracol), Comparabil.Proiecteaza(contract.Tranzactie!), nume);
            if (raport.Egal && Normalizari.Avertismente.Count == 0) {
                contor.Egale++;
                return;
            }
            contor.Diferite++;
            var fel = Felul(raport, nume);
            var (cateFel, exempleFel, diff) = contor.Feluri.GetValueOrDefault(fel, (0, new List<string>(), ""));
            if (exempleFel.Count < 5)
                exempleFel.Add(eticheta);
            contor.Feluri[fel] = (cateFel + 1, exempleFel,
                cateFel < 3 ? diff + $"\n       ── {eticheta} ──\n{raport}" : diff);
        }
        catch (Exception e) {
            contor.Exceptii++;
            if (contor.TextExceptii.Count < 10)
                contor.TextExceptii.Add($"{eticheta}: {e.GetType().Name}: {e.Message.Split('\n')[0]}");
        }
    }

    // Felul reziduului: ce coordonate lipsesc / sunt în plus, fără cifre — cheia
    // pe care se grupează documentele diferite.
    static string Felul(RaportComparatie raport, Func<Guid, string> nume) {
        var parti = raport.Lipsa.Select(p => "LIPSĂ " + Semnatura(p, nume))
            .Concat(raport.InPlus.Select(p => "ÎN PLUS " + Semnatura(p, nume)))
            .Distinct()
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
        return parti.Count == 0 ? "(doar avertismente de normalizare)" : string.Join(" | ", parti);
    }

    static string Semnatura(PostareComparabila postare, Func<Guid, string> nume) {
        var text = new StringBuilder();
        text.Append(postare.Latura == N.Latura.Debit ? 'D' : 'C').Append(' ').Append(nume(postare.Cont));
        if (postare.Unitate is { } unitate)
            text.Append('/').Append(unitate.Fel);
        if (postare.CodTva is { } cod)
            text.Append("/tva:").Append(cod.Rol);
        if (postare.Cantitate != 0m)
            text.Append("/cant");
        if (postare.PerioadaDeclarare != null)
            text.Append("/per");
        return text.ToString();
    }

    // Avertismentele poartă id-uri în text; șablonul îi ține grupați pe fel.
    static string Sablon(string avertisment) {
        var text = new StringBuilder();
        foreach (var bucata in avertisment.Split(' '))
            text.Append(Guid.TryParse(bucata.Trim('.', ',', ':'), out _) ? "<id>" : bucata).Append(' ');
        return text.ToString().Trim();
    }

    // B-r1: taxa culeasă contra celei decise de nucleu, per document × cotă, FĂRĂ
    // toleranță — aceeași aritmetică pe care o face `Fiscal.Taxa` (Σ valorilor ALESE
    // per cotă contra `Tva.PeDocument`), ca măsurătoarea să fie a declarantului.
    static void AbaterileTaxei(
            Document doc,
            string eticheta,
            N.DirectieTva directie,
            IReadOnlyDictionary<Guid, (N.RegimTva Regim, decimal Cota)> tipuriTva,
            List<Abatere> abateri) {
        var linii = doc.Detalii.OrderBy(d => d.Pozitie).ThenBy(d => d.ID)
            .Where(d => d.TipTvaId != null && tipuriTva.ContainsKey(d.TipTvaId.Value))
            .Select(d => new { d.ID, d.Valoare, d.ValoareTva, Cheie = tipuriTva[d.TipTvaId!.Value] })
            .ToList();
        if (linii.Count == 0)
            return;
        var taxa = N.Tva.PeDocument(
            [.. linii.Select(l => new N.LinieTva(l.ID, l.Valoare, l.Cheie.Regim, l.Cheie.Cota))],
            directie,
            new N.Rotunjire(Scara.ConventieBani));
        foreach (var grup in linii.GroupBy(l => l.Cheie)) {
            var culeasa = grup.Sum(l =>
                l.ValoareTva != 0m ? l.ValoareTva : taxa.PerLinie.GetValueOrDefault(l.ID));
            abateri.Add(new Abatere(doc.ID, eticheta,
                $"{grup.Key.Regim}/{grup.Key.Cota.ToString("0.##", CultureInfo.InvariantCulture)}",
                grup.Count(), culeasa, taxa.PerCota[grup.Key]));
        }
    }

    static void Raporteaza(Action<string> scrie, string cod, Contor contor) {
        scrie($"{cod}: {contor.Documente} documente — EGALE {contor.Egale}, REFUZATE {contor.Refuzate}, "
            + $"DIFERITE {contor.Diferite}, EXCEPȚII {contor.Exceptii}, fără declarant {contor.FaraDeclarant}");
        foreach (var (codRefuz, (cate, documente, exemple)) in contor.Refuzuri.OrderByDescending(r => r.Value.Cate)) {
            scrie($"   refuz {codRefuz}: {cate} refuzuri pe {documente.Count} documente");
            foreach (var exemplu in exemple)
                scrie($"       {exemplu}");
            scrie($"       id-uri: {string.Join(", ", documente.Take(50).Select(d => d.ToString()))}"
                + (documente.Count > 50 ? $" … încă {documente.Count - 50}" : ""));
        }
        foreach (var (fel, (cate, exemple, diff)) in contor.Feluri.OrderByDescending(f => f.Value.Cate)) {
            scrie($"   reziduu „{fel}”: {cate} documente; exemple: {string.Join(", ", exemple)}");
            if (diff.Length > 0)
                scrie(diff.TrimEnd());
        }
        foreach (var (avertisment, cate) in contor.Avertismente.OrderByDescending(a => a.Value))
            scrie($"   Normalizari.Avertismente ×{cate}: {avertisment}");
        foreach (var text in contor.Conservare)
            scrie($"   conservare NEÎNDEPLINITĂ: {text}");
        foreach (var text in contor.TextExceptii)
            scrie($"   excepție: {text}");
    }

    static void RaporteazaTva(Action<string> scrie, IReadOnlyList<Abatere> abateri) {
        if (abateri.Count == 0)
            return;
        scrie($"\n── B-r1: abaterea taxei culese de cea decisă de nucleu ({abateri.Count} perechi "
            + $"document × cotă, pe {abateri.Select(a => a.Document).Distinct().Count()} documente) ──");
        var praguri = new (string Eticheta, Func<Abatere, bool> Test)[] {
            ("= 0", a => a.Valoare == 0m),
            ("≤ 0,01 × liniile cotei (toleranța de azi)", a => a.Valoare <= 0.01m * a.Linii),
            ("≤ 0,05", a => a.Valoare <= 0.05m),
            ("≤ 0,10", a => a.Valoare <= 0.10m),
            ("≤ 0,50", a => a.Valoare <= 0.50m),
        };
        var ramase = abateri.ToList();
        foreach (var (etichetaPrag, test) in praguri) {
            var aleLui = ramase.Where(test).ToList();
            scrie($"   {etichetaPrag,-45} {aleLui.Count,8}");
            ramase = [.. ramase.Except(aleLui)];
        }
        scrie($"   {"> 0,50",-45} {ramase.Count,8}");

        var maxim = abateri.OrderByDescending(a => a.Valoare).First();
        scrie($"   maximul: {maxim.Valoare.ToString("0.####", CultureInfo.InvariantCulture)} pe "
            + $"{maxim.Eticheta}, cota {maxim.Cota}, {maxim.Linii} linii "
            + $"(culeasă {maxim.Culeasa}, nucleu {maxim.Nucleu})");

        foreach (var toleranta in new[] { 0.01m, 0.05m, 0.10m }) {
            var refuzate = abateri
                .Where(a => a.Valoare > toleranta * a.Linii)
                .Select(a => a.Document)
                .Distinct()
                .Count();
            scrie($"   documente REFUZATE la toleranța {toleranta.ToString("0.00", CultureInfo.InvariantCulture)} "
                + $"per linie a cotei: {refuzate}");
        }
        foreach (var exemplu in abateri.OrderByDescending(a => a.Valoare).Take(10))
            scrie($"       {exemplu.Eticheta} cota {exemplu.Cota} ×{exemplu.Linii}: culeasă {exemplu.Culeasa}, "
                + $"nucleu {exemplu.Nucleu}, abatere "
                + $"{exemplu.Valoare.ToString("0.####", CultureInfo.InvariantCulture)}");
    }

    // B-r10: ce face motorul VECHI cu linia pe care declarantul o refuză fără regulă
    // de contare — are sau nu notă în `RegistruContabil` pe linia aceea.
    static void RaporteazaRegulaLipsa(
            Action<string> scrie, EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext> provider,
            IReadOnlyList<(Guid Document, Guid? Linie)> cazuri) {
        scrie($"\n── B-r10: linii fără regulă de contare ({CoduriRefuz.RegulaContareLipsa}) ──");
        scrie($"   refuzuri: {cazuri.Count} pe {cazuri.Select(c => c.Document).Distinct().Count()} documente; "
            + $"cu linie numită: {cazuri.Count(c => c.Linie != null)}");
        var exemple = cazuri.Where(c => c.Linie != null).Take(3).ToList();
        if (exemple.Count == 0)
            return;
        using var os = provider.CreateObjectSpace();
        foreach (var (document, linie) in exemple) {
            var note = os.GetObjectsQuery<RegistruContabil>().Count(r => r.DetaliuId == linie);
            var stoc = os.GetObjectsQuery<RegistruStoc>().Count(r => r.DetaliuId == linie);
            var detaliu = os.GetObjectByKey<DocumentDetaliu>(linie!.Value);
            scrie($"       document {document.ToString()[..8]} linia {linie.Value.ToString()[..8]}"
                + $" (tip material {detaliu?.TipMaterialId.ToString()[..8]}, valoare {detaliu?.Valoare}): "
                + $"{note} rânduri în RegistruContabil, {stoc} în RegistruStoc");
        }
    }
}
