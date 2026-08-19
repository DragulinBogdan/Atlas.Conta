using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;

namespace BackfillTva;

public sealed class RezultatReconciliere {
    public int Documente, Randuri, Divergente, LiniiFaraTipTva, LiniiCuPolitica, FaraPartener;
}

// Contractul feliei 11, JT-D6: cusătura structurală dintre registrul FISCAL și
// cel CONTABIL, per DOCUMENT, peste TOATĂ baza.
//
// Semantica e IDENTICĂ cu verificarea 1 din `ModelCheck.VerificaRegistruTva` —
// aceeași definiție, deliberat, fiindcă două definiții ale aceleiași cusături ar
// fi însemnat două adevăruri. Ce diferă e doar structura de date: ModelCheck
// filtrează liste (bază mică), aici se lucrează pe dicționare — pe 60.000 de
// documente o căutare liniară per document ar fi fost O(n²).
//
// Conturile de TVA se citesc ca DATE (`TipTva.ContTvaDeductibil/Colectat`,
// decizia 29: motorul e agnostic la plan, deci și proba lui) — niciun simbol
// hardcodat, nici 4426, nici 4427.
public static class Reconciliere {
    public static RezultatReconciliere Executa(IObjectSpaceProvider provider,
            List<RandFiscal> derivate, Action<string> avert, Action<string, bool> check) {
        var rez = new RezultatReconciliere();
        using var os = provider.CreateObjectSpace();

        Console.WriteLine("\n=== Reconcilierea JT-D6 (per document, peste toată baza) ===");

        var conturiTva = os.GetObjectsQuery<TipTva>()
            .Select(t => new { t.ContTvaDeductibilId, t.ContTvaColectatId }).ToList()
            .SelectMany(t => new[] { t.ContTvaDeductibilId, t.ContTvaColectatId })
            .Where(id => id != null).Select(id => id.Value).Distinct().ToList();

        // Rândurile fiscale se RECITESC din Postgres, independent de structurile
        // fazei de scriere (contractul 47e). În dry-run se adaugă și cele care
        // S-AR fi scris — altfel proba ar fi vacuă exact pe rularea în care e cel
        // mai util să știi dacă derivarea închide cusătura.
        var fiscale = os.GetObjectsQuery<RegistruTva>()
            .Select(r => new { r.DocumentId, r.Sens, r.PartenerId, r.Regim, r.Baza, r.Tva, r.Storno })
            .ToList()
            .Select(r => new RandFiscal(r.DocumentId, r.Sens, r.PartenerId, r.Regim, r.Baza, r.Tva, r.Storno))
            .ToList();
        var dinBaza = fiscale.Count;
        if (derivate is { Count: > 0 }) {
            fiscale.AddRange(derivate);
            Console.WriteLine($"    DRY-RUN: {dinBaza} rânduri existente în bază + {derivate.Count} derivate "
                + "în rularea asta (nescrise) = mulțimea reconciliată.");
        }
        rez.Randuri = fiscale.Count;

        // Partea FISCALĂ, per document: doar regimurile care POSTEAZĂ.
        // `Scutit`/`Neimpozabil`/`Capitalizat` nu produc rând contabil de TVA — e
        // chiar motivul de existență al registrului (design §„de ce nu o proiecție
        // peste RegistruContabil”) — deci n-au ce căuta în cusătură.
        var fiscalPeDoc = fiscale
            .Where(r => r.Regim is RegimTva.Normal or RegimTva.TaxareInversa)
            .GroupBy(r => r.DocumentId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Tva));
        var docIds = fiscale.Select(r => r.DocumentId).ToHashSet();
        rez.Documente = docIds.Count;

        // Partea CONTABILĂ. Se însumează pe RÂND, nu pe latură: `TaxareInversa`
        // postează 4426 = 4427, adică UN rând care atinge ambele conturi de TVA —
        // o însumare per latură l-ar fi numărat de două ori. Motivul e semantic:
        // autolichidarea e o SINGURĂ operațiune taxabilă, cu o singură cifră.
        //
        // Nu se filtrează `Storno` pe niciuna dintre părți: ambele registre sunt
        // append-only și suma lor ALGEBRICĂ e adevărul (R-D7); un storno peste
        // graniță de lună cade în altă perioadă pe ambele părți deodată, deci
        // cusătura per document rămâne exactă acolo unde una pe perioadă ar pica.
        var contabilPeDoc = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null
                && (conturiTva.Contains(r.ContDebitId) || conturiTva.Contains(r.ContCreditId)))
            .Select(r => new { DocumentId = r.DocumentId.Value, r.Valoare })
            .ToList()
            .GroupBy(r => r.DocumentId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Valoare));

        var divergente = docIds
            .Select(id => (Id: id, Fiscal: fiscalPeDoc.GetValueOrDefault(id),
                Contabil: contabilPeDoc.GetValueOrDefault(id)))
            .Where(x => x.Fiscal != x.Contabil)
            .ToList();
        rez.Divergente = divergente.Count;

        if (divergente.Count > 0)
            RaporteazaDivergente(os, fiscale, divergente);

        check($"JT-D6, pe toate cele {rez.Documente} documente cu rânduri fiscale ale bazei "
            + $"({rez.Randuri} rânduri): Σ TVA al regimurilor care POSTEAZĂ (Normal + TaxareInversa) == "
            + "Σ valoarea rândurilor contabile ale ACELUIAȘI document pe conturile de TVA — registrul fiscal "
            + "se închide pe cifra contabilă prin construcție. Divergențe: " + divergente.Count,
            divergente.Count == 0);
        // Anti-vacuitate (lecția porților vacue, 50b): pe o bază fără niciun
        // document fiscal cusătura de mai sus e adevărată și cu o derivare complet
        // greșită. Nu e un eșec — o bază de neplătitor chiar n-are ce reconcilia —
        // dar trebuie SPUS, nu lăsat să treacă drept probă.
        if (rez.Documente == 0)
            avert("Cusătura JT-D6 s-a măsurat pe ZERO documente (baza n-are rânduri fiscale) — "
                + "poartă vacuă, nu probă. Rulează backfill-ul, sau baza e a unui neplătitor.");
        else
            check($"Proba NU e vacuă: {rez.Documente} documente cu rânduri fiscale, "
                + $"Σ contabil pe conturile de TVA {contabilPeDoc.Where(x => docIds.Contains(x.Key)).Sum(x => x.Value):N2} lei",
                contabilPeDoc.Where(x => docIds.Contains(x.Key)).Sum(x => x.Value) != 0m);

        MasoaraComplementul(os, conturiTva, docIds);
        Masoara(os, rez, fiscale, check);
        Distributie(fiscale);
        return rez;
    }

    // ═══ COMPLEMENTUL cusăturii (review advers D2) ═══
    //
    // JT-D6 iterează documentele PREZENTE în `RegistruTva`. Mulțimea complementară
    // — documente care postează pe conturile de TVA dar NU produc fapte fiscale —
    // e invizibilă prin construcție: contractul putea raporta „ÎNDEPLINIT" în timp
    // ce milioane de lei de TVA postat stăteau în afara oricărui jurnal.
    //
    // Nu e o verificare, ci o MĂSURĂTOARE defalcată pe tip de document: unele
    // intrări sunt corecte prin design (`InchidereTva` mișcă 4426/4427 fără să fie
    // operațiune taxabilă), altele sunt artefacte de import (`NotaContabila`
    // punte), altele ar fi defecte adevărate. Diferența se ia cu ochii pe listă —
    // dar lista trebuie să EXISTE. Disciplina „Acoperit cere acoperitor" (50b):
    // ce nu e acoperit se numește, nu se presupune.
    static void MasoaraComplementul(IObjectSpace os, List<Guid> conturiTva, HashSet<Guid> cuFapteFiscale) {
        var randuri = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null
                && (conturiTva.Contains(r.ContDebitId) || conturiTva.Contains(r.ContCreditId)))
            .Select(r => new { DocumentId = r.DocumentId.Value, r.Valoare })
            .ToList()
            .Where(r => !cuFapteFiscale.Contains(r.DocumentId))
            .ToList();
        if (randuri.Count == 0) {
            Console.WriteLine("    MĂSURAT (complementul D2): niciun document postează pe conturile de TVA "
                + "fără să aibă fapte fiscale — mulțimile coincid exact.");
            return;
        }

        // Tipul se citește din clasa CLR, materializând POLIMORF într-un singur
        // query (tiparul 60b) — sub TPT nu există discriminator.
        var ids = randuri.Select(r => r.DocumentId).Distinct().ToList();
        var tipuri = os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
            .ToDictionary(d => d.ID, d => {
                var t = d.GetType();
                while (t.Assembly.IsDynamic || t.Name.EndsWith("Proxy"))
                    t = t.BaseType;
                return t.Name;
            });

        Console.WriteLine($"    MĂSURAT (complementul D2): {randuri.Count} rânduri contabile pe conturi de TVA "
            + $"aparțin unor documente FĂRĂ fapte fiscale ({ids.Count} documente). "
            + "Unele sunt corecte prin design (închiderea lunară nu e operațiune taxabilă), "
            + "altele pot fi artefacte de import — se citesc pe tip:");
        foreach (var g in randuri.GroupBy(r => tipuri.GetValueOrDefault(r.DocumentId, "(document invizibil)"))
                     .OrderByDescending(g => Math.Abs(g.Sum(r => r.Valoare))))
            Console.WriteLine($"        {g.Key,-20} {g.Count(),7} rânduri / "
                + $"{g.Select(r => r.DocumentId).Distinct().Count(),7} documente / Σ {g.Sum(r => r.Valoare),18:N2}");
    }

    // Măsurătorile declarate ale feliei (JT-D2 + riscul 4 din design): găurile se
    // NUMĂRĂ, nu se umplu cu regimuri presupuse sau parteneri inventați.
    static void Masoara(IObjectSpace os, RezultatReconciliere rez, List<RandFiscal> fiscale,
            Action<string, bool> check) {
        var peTip = TipuriCuPolitica(os);
        var idsCuPolitica = peTip.Values.SelectMany(x => x).ToHashSet();
        var operateCuPolitica = os.GetObjectsQuery<Document>()
            .Where(d => d.Stare == StareDocument.Operat)
            .Select(d => d.ID).ToList()
            .Where(idsCuPolitica.Contains).ToHashSet();

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Select(d => new { d.DocumentId, Fara = d.TipTvaId == null }).ToList()
            .Where(l => operateCuPolitica.Contains(l.DocumentId)).ToList();
        rez.LiniiCuPolitica = linii.Count;
        rez.LiniiFaraTipTva = linii.Count(l => l.Fara);
        rez.FaraPartener = fiscale.Count(r => r.PartenerId == null);

        Console.WriteLine($"    MĂSURAT (JT-D2): {rez.LiniiFaraTipTva} linii FĂRĂ TipTva pe cele "
            + $"{operateCuPolitica.Count} documente OPERATE ale unui tip cu PoliticaTva "
            + $"(din {rez.LiniiCuPolitica} linii ale lor) — rămân în afara jurnalului, gaură a DATELOR.");
        Console.WriteLine($"    MĂSURAT (riscul 4): {rez.FaraPartener} rânduri fiscale fără partener "
            + $"(din {fiscale.Count}) — `SursaContrapartida` care nu e o latură.");

        // Invariantul care ÎNCAPE aici și chiar are dinți: partenerul, când
        // există, e chiar una dintre laturile documentului lui — adică
        // repartitorul laturii cerute de politică, nu un terț inventat.
        var laturi = os.GetObjectsQuery<Document>()
            .Select(d => new { d.ID, d.PredatorId, d.PrimitorId }).ToList()
            .ToDictionary(d => d.ID, d => (d.PredatorId, d.PrimitorId));
        var straini = fiscale.Where(r => r.PartenerId != null)
            .Count(r => !laturi.TryGetValue(r.DocumentId, out var l)
                || (r.PartenerId != l.PredatorId && r.PartenerId != l.PrimitorId));
        check("Partenerul rândului fiscal — când există — e una dintre LATURILE documentului lui "
            + $"(repartitorul cerut de politică, nu un terț): {straini} rânduri străine",
            straini == 0);
    }

    // Prima privire reală asupra jurnalului: ce a intrat, pe ce sens și ce regim.
    static void Distributie(List<RandFiscal> fiscale) {
        Console.WriteLine("\n--- Distribuția rândurilor fiscale (Sens × Regim) ---");
        Console.WriteLine($"    {"Sens",-10} {"Regim",-16} {"rânduri",10} {"Σ bază",18} {"Σ TVA",18}");
        foreach (var g in fiscale.GroupBy(r => (r.Sens, r.Regim))
                     .OrderBy(g => g.Key.Sens).ThenBy(g => g.Key.Regim))
            Console.WriteLine($"    {g.Key.Sens,-10} {g.Key.Regim,-16} {g.Count(),10} "
                + $"{g.Sum(r => r.Baza),18:N2} {g.Sum(r => r.Tva),18:N2}");
        Console.WriteLine($"    {"TOTAL",-27} {fiscale.Count,10} "
            + $"{fiscale.Sum(r => r.Baza),18:N2} {fiscale.Sum(r => r.Tva),18:N2}");
        var storno = fiscale.Where(r => r.Storno).ToList();
        if (storno.Count > 0)
            Console.WriteLine($"    (din care storno: {storno.Count} rânduri, "
                + $"Σ bază {storno.Sum(r => r.Baza):N2}, Σ TVA {storno.Sum(r => r.Tva):N2})");
    }

    // Divergența pe date reale e informația cea mai valoroasă a rulării — se
    // itemizează cu tot ce trebuie ca s-o poți urmări în bază, nu se numără doar.
    static void RaporteazaDivergente(IObjectSpace os,
            List<RandFiscal> fiscale, List<(Guid Id, decimal Fiscal, decimal Contabil)> divergente) {
        var primele = divergente.Take(10).ToList();
        var ids = primele.Select(d => d.Id).ToList();
        var documente = os.GetObjectsQuery<Document>()
            .Where(d => ids.Contains(d.ID)).ToList()
            .ToDictionary(d => d.ID);
        Console.WriteLine($"    {divergente.Count} DIVERGENȚE — primele {primele.Count}:");
        foreach (var (id, fiscal, contabil) in primele) {
            var doc = documente.GetValueOrDefault(id);
            var tip = doc == null ? "?" : NumeClr(doc);
            Console.WriteLine($"      {tip,-16} {doc?.Numar,-16} {doc?.Data:yyyy-MM-dd} {id}: "
                + $"fiscal {fiscal:N2} vs contabil {contabil:N2} (Δ {fiscal - contabil:N2})");
            foreach (var g in fiscale.Where(r => r.DocumentId == id)
                         .GroupBy(r => (r.Regim, r.Storno)).OrderBy(g => g.Key.Regim))
                Console.WriteLine($"          {g.Key.Regim,-14}{(g.Key.Storno ? " storno" : "       ")} "
                    + $"{g.Count(),4} rânduri  bază {g.Sum(r => r.Baza),14:N2}  TVA {g.Sum(r => r.Tva),12:N2}");
        }
    }

    // TPT n-are discriminator: „de ce tip e documentul” se citește din clasa CLR
    // (tiparul din ModelCheck / ApiProiectii).
    static string NumeClr(Document d) {
        var t = d.GetType();
        while (t.Assembly.IsDynamic || t.Name.EndsWith("Proxy"))
            t = t.BaseType;
        return t.Name;
    }

    // Cititorul comun al mulțimii „documente ale unui tip declarat eveniment de
    // TVA de profilul bazei” (criteriul JT-D2, jumătatea POLITICĂ). E partajat cu
    // faza de backfill ca CITITOR, nu ca rezultat: reconcilierea îl re-execută pe
    // ObjectSpace-ul ei, deci nu moștenește nimic din faza de scriere.
    public static Dictionary<string, HashSet<Guid>> TipuriCuPolitica(IObjectSpace os) {
        var db = ((DevExpress.ExpressApp.EFCore.EFCoreObjectSpace)os).DbContext;
        var rezultat = new Dictionary<string, HashSet<Guid>>();
        var asamblare = typeof(Document).Assembly;
        foreach (var nume in os.GetObjectsQuery<PoliticaTva>()
                     .Select(p => p.TipDocument.ClrType).Distinct().ToList()) {
            var tip = asamblare.GetTypes()
                .FirstOrDefault(t => t.Name == nume && typeof(Document).IsAssignableFrom(t));
            var tabela = tip == null ? null : db.Model.FindEntityType(tip)?.GetTableName();
            if (tabela == null)
                continue;
            // Tabela derivată TPT n-are `GCRecord` (soft delete-ul stă pe
            // `Documente`): mulțimea se intersectează oricum cu documentele VII,
            // citite prin LINQ de apelant.
            //
            // EF1002 suprimat cu motiv: numele tabelei NU vine din afară, ci din
            // modelul EF al aceluiași DbContext (`FindEntityType(...).GetTableName()`)
            // — nu există intrare de utilizator pe traseu. Alternativa parametrizată
            // nu există: un nume de tabelă nu poate fi parametru SQL.
#pragma warning disable EF1002
            rezultat[nume] = db.Database
                .SqlQueryRaw<Guid>($"SELECT \"ID\" AS \"Value\" FROM \"{tabela}\"")
                .ToList().ToHashSet();
#pragma warning restore EF1002
        }
        return rezultat;
    }
}
