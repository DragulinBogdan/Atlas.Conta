#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Insula recepției conexe: dovada absorbției este faptul sursei, nu politica curentă.</summary>
public static class ReceptiiConexe {
    public const string Provenienta = CoduriRefuz.NirProvenientaInvalida;
    public const string Activa = CoduriRefuz.NirReceptieActiva;
    public const string Stoc = CoduriRefuz.NirStocInsuficient;

    static readonly ConditionalWeakTable<IObjectSpace, CitireComanda> citiri = new();

    sealed class CitireComanda(IObjectSpace os, NIR nir, bool blocheaza, CitireComanda? anterior) : IDisposable {
        public Guid Document { get; } = nir.ID;
        public Lazy<ReceptieSursaFapt?> Fapt { get; } = new(() => {
            if (blocheaza && Sursa(nir) is Guid sursa) {
                Blocheaza(os, sursa);
                var original = os.GetObjectByKey<Document>(sursa);
                if (original != null && os is EFCoreObjectSpace ef && ef.DbContext.Entry(original).State == EntityState.Unchanged)
                    ef.DbContext.Entry(original).Reload();
            }
            return CitesteDinBaza(os, nir);
        });
        public void Dispose() {
            citiri.Remove(os);
            if (anterior != null) citiri.Add(os, anterior);
        }
    }

    internal static IDisposable? IncepeCitirea(IObjectSpace os, Document doc, bool blocheaza) {
        if (doc is not NIR nir) return null;
        citiri.TryGetValue(os, out var anterior);
        if (anterior?.Document == doc.ID) return null;
        var citire = new CitireComanda(os, nir, blocheaza, anterior);
        citiri.Remove(os);
        citiri.Add(os, citire);
        return citire;
    }

    public static Guid? Sursa(Document doc) => doc is NIR nir
        ? nir.SursaReceptieiId : null;

    public static bool EsteAcoperita(IObjectSpace os, Document doc) =>
        doc is NIR nir && Citeste(os, nir) is not null;

    public static bool EsteLinieAcoperita(IObjectSpace os, NIR nir, DocumentDetaliu linie) {
        var sursa = Citeste(os, nir);
        return sursa != null && Identifica(linie, sursa.Linii) != null;
    }

    static Guid? Identifica(DocumentDetaliu linie, IReadOnlyList<LinieReceptieSursa> surse) =>
        Identifica((linie as NirDetaliu)?.LinieSursaReceptieId, linie.LotId, linie.TipMaterialId, surse);

    public static Guid? Identifica(Guid? original, Guid? lot, Guid tip, IReadOnlyList<LinieReceptieSursa> surse) {
        if (original is Guid id) return id;
        var candidati = surse.Where(s => s.Stoc.Unitate?.Id == lot && s.TipMaterial == tip).ToList();
        return candidati.Count == 1 ? candidati[0].Linie : null;
    }

    internal static ReceptieSursaFapt? Citeste(IObjectSpace os, NIR nir) =>
        citiri.TryGetValue(os, out var citire) && citire.Document == nir.ID
            ? citire.Fapt.Value : CitesteDinBaza(os, nir);

    static ReceptieSursaFapt? CitesteDinBaza(IObjectSpace os, NIR nir) {
        if (Sursa(nir) is not Guid id) return null;
        var doc = os.GetObjectByKey<Document>(id);
        if (doc == null || !doc.AcoperaReceptia(nir)) {
            if (nir.SursaReceptieiId != null || nir.TranzactieReceptieSursaId != null)
                throw new OperareException($"{Provenienta}: Sursa nu acoperă structural recepția.");
            return null;
        }
        var randuri = os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == id
            && p.Tranzactie.Fel == N.FelTranzactie.Operare && p.Carte == N.Carte.Contabil
            && p.FelUnitate == N.FelUnitate.Lot && p.Unitate != null && p.Latura == N.Latura.Debit).ToList();
        if (randuri.Count == 0) {
            if (nir.TranzactieReceptieSursaId != null)
                throw new OperareException($"{Provenienta}: Recepția istorică a sursei lipsește.");
            return null;
        }
        var tranzactii = randuri.Select(p => p.TranzactieId).Distinct().ToList();
        if (tranzactii.Count != 1 || nir.TranzactieReceptieSursaId is Guid fixata && tranzactii[0] != fixata)
            throw new OperareException($"{Provenienta}: Recepția sursei nu are o tranzacție univocă.");
        var linii = new List<LinieReceptieSursa>();
        var tipuri = doc.Detalii.ToDictionary(d => d.ID, d => d.TipMaterialId);
        foreach (var grup in randuri.GroupBy(p => p.LinieId)) {
            var capete = grup.Select(p => {
                var c = Randuri.Citeste(p).Coordonate;
                return new N.Capat { Cont = c.Cont, Gestiune = c.Gestiune, Produs = c.Produs,
                    Unitate = c.Unitate, Analiza = c.Analiza };
            }).Distinct().ToList();
            if (grup.Key is not Guid linie || !tipuri.TryGetValue(linie, out var tip)
                    || capete.Count != 1 || grup.Sum(p => p.Cantitate) <= 0)
                throw new OperareException($"{Provenienta}: Linia sursei nu are o recepție univocă.");
            linii.Add(new(linie, tip, capete[0], grup.Sum(p => p.Cantitate), grup.Sum(p => p.Valoare)));
        }
        return new(id, tranzactii[0], linii, new Dictionary<Guid, ConstatareReceptie>(), [], nir.CauzeDiferentaPermise());
    }

    public static Operand Completeaza(IObjectSpace os, Document doc, Operand operand) {
        if (doc is not NIR nir || Citeste(os, nir) is not { } sursa) return operand;
        var original = os.GetObjectByKey<Document>(sursa.Document);
        if (original.Stare != StareDocument.Operat || original.DataInregistrare > doc.DataInregistrare
                || original.PredatorId != doc.PredatorId || original.PrimitorId != doc.PrimitorId)
            throw new OperareException($"{Provenienta}: Sursa trebuie să fie operată, anterioară și cu aceleași laturi.");
        VerificaUnica(os, nir, sursa.Document);
        var constatari = doc.Detalii.ToDictionary(d => d.ID, d => new ConstatareReceptie(
            Identifica(d, sursa.Linii), (d as NirDetaliu)?.CauzaDiferentei, (d as NirDetaliu)?.PartenerDiferentaId));
        var politici = os.GetObjectsQuery<PoliticaDiferenta>().Where(p => p.TipDocumentId == operand.Document.TipDocumentId)
            .Select(p => new PoliticaDiferentaFapt(p.ClasaId, p.Cauza, p.ContId, p.ContPersonalId)).ToList();
        var idsCont = politici.SelectMany(p => new[] { p.Cont, p.ContPersonal ?? p.Cont }).Distinct().ToList();
        var conturi = operand.Conturi.ToDictionary(p => p.Key, p => p.Value);
        foreach (var c in os.GetObjectsQuery<Cont>().Where(c => idsCont.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol, c.UrmarestePartide }).ToList())
            conturi[c.ID] = new(c.ID, c.Simbol, c.UrmarestePartide);
        var repartitori = operand.Repartitori.ToDictionary(p => p.Key, p => p.Value);
        var idsTerti = constatari.Values.Select(c => c.Imputat).OfType<Guid>().Distinct().ToList();
        foreach (var r in Fapte.Repartitori(os, idsTerti)) repartitori[r.Key] = r.Value;
        return operand with { ReceptieSursa = sursa with { Constatari = constatari, Politici = politici },
            Conturi = conturi, Repartitori = repartitori };
    }

    static void Blocheaza(IObjectSpace os, Guid sursa) {
        if (os is not EFCoreObjectSpace ef || ef.DbContext.Database.CurrentTransaction == null)
            throw new OperareException("Recepția conexă cere tranzacția comenzii.");
        // Domeniu separat; coliziunea hash serializează inutil, nu compromite unicitatea.
        ef.DbContext.Database.ExecuteSqlInterpolated($"SELECT pg_advisory_xact_lock(97002, {sursa.GetHashCode()})");
    }

    static void VerificaUnica(IObjectSpace os, NIR nir, Guid sursa) {
        if (os.GetObjectsQuery<NIR>().Any(n => n.ID != nir.ID && n.Stare == StareDocument.Operat
                && n.SursaReceptieiId == sursa))
            throw new OperareException($"{Activa}: Factura are deja o recepție activă; corectați cumulul acesteia.");
    }

    public static void Fixeaza(IObjectSpace os, Document doc) {
        if (doc is not NIR nir || Sursa(nir) is not Guid id) return;
        if (Citeste(os, nir) is not { } sursa) return;
        VerificaUnica(os, nir, id);
        nir.SursaReceptieiId = id;
        nir.TranzactieReceptieSursaId = sursa.Tranzactie;
        foreach (var linie in nir.Detalii.OfType<NirDetaliu>())
            linie.LinieSursaReceptieId ??= Identifica(linie, sursa.Linii);
        MaterializeazaValori(os, nir, fixeazaCauza: true);
    }

    public static void MaterializeazaValori(IObjectSpace os, NIR nir, bool fixeazaCauza = false) {
        if (Citeste(os, nir) is not { } sursa) return;
        foreach (var linie in nir.Detalii.Where(d => !os.IsObjectToDelete(d))) {
            var id = Identifica(linie, sursa.Linii);
            var original = sursa.Linii.SingleOrDefault(l => l.Linie == id);
            if (original != null)
                linie.Valoare = linie.Cantitate == original.Cantitate ? original.Valoare
                    : Scara.RotunjesteBani(original.Valoare * linie.Cantitate / original.Cantitate);
            var delta = linie.Cantitate - (original?.Cantitate ?? 0m);
            if (linie is NirDetaliu imputare && (delta == 0 || imputare.CauzaDiferentei != CauzaDiferentei.Imputabila))
                GolesteImputatul(imputare);
            if (fixeazaCauza && delta != 0 && linie is NirDetaliu detaliu)
                detaliu.CauzaDiferentei ??= delta < 0 ? CauzaDiferentei.InClarificare : CauzaDiferentei.Plus;
        }
    }

    static void GolesteImputatul(NirDetaliu linie) {
        linie.PartenerDiferenta = null;
        linie.PartenerDiferentaId = null;
    }

    public static void VerificaEditare(IObjectSpace os, object obiect, ICollection<string> erori) {
        if (os is not EFCoreObjectSpace ef) return;
        var entry = ef.DbContext.Entry(obiect);
        string[] campuri = obiect switch {
            NIR => [nameof(NIR.SursaReceptieiId), nameof(NIR.TranzactieReceptieSursaId)],
            NirDetaliu => [nameof(NirDetaliu.LinieSursaReceptieId)],
            _ => [],
        };
        foreach (var camp in campuri) {
            var p = entry.Property(camp);
            if (os.IsNewObject(obiect) ? p.CurrentValue != null : !Equals(p.CurrentValue, p.OriginalValue))
                erori.Add($"{Provenienta}: Proveniența recepției o scrie numai serverul.");
        }
        if (obiect is not NirDetaliu linie) return;
        if (linie.CauzaDiferentei != CauzaDiferentei.Imputabila) GolesteImputatul(linie);
        var stearsa = os.IsObjectToDelete(linie);
        var identitateSchimbata = !os.IsNewObject(linie) && new[] { nameof(linie.LotId), nameof(linie.TipMaterialId) }
            .Any(camp => !Equals(entry.Property(camp).CurrentValue, entry.Property(camp).OriginalValue));
        if (!stearsa && !identitateSchimbata) return;
        if (linie.Document is NIR nir && EsteLinieAcoperita(os, nir, linie))
            erori.Add($"{Provenienta}: Linia-sursă nu se șterge și nu se reidentifică; constatați cantitatea și cauza diferenței.");
    }

    public static void VerificaFaraDependenti(IObjectSpace os, Document doc) {
        var id = Sursa(doc) ?? doc.ID;
        Blocheaza(os, id);
        MotorOperare.Refuza(MotivDependenti(os, doc));
    }

    /// <summary>Recepția activă a altui document depinde de sursă, citit fără blocaj; null = liber.</summary>
    public static string MotivDependenti(IObjectSpace os, Document doc) =>
        os.GetObjectsQuery<NIR>().Any(n => n.ID != doc.ID && n.Stare == StareDocument.Operat
            && n.SursaReceptieiId == doc.ID)
            ? $"{Activa}: Recepția sau corecția ei activă depinde de sursă."
            : null;

    // Păstrează codul refuzului NIR; domeniul și calculul soldului sunt comune.
    internal static string CodRefuzStoc(Document doc) => doc is NIR ? Stoc : "STOC_INSUFICIENT";

    public static void VerificaAnaliza(IObjectSpace os, Document doc, IEnumerable<N.Postare> propuse) {
        if (doc is not NIR || !EsteAcoperita(os, doc)) return;
        var postari = propuse.Where(p => p.Coordonate.Gestiune == N.GestiuniVirtuale.Inventar).ToList();
        var ids = postari.Select(p => p.Coordonate.Cont).Distinct().ToList();
        var conturi = os.GetObjectsQuery<Cont>().Where(c => ids.Contains(c.ID))
            .Select(c => new { c.ID, c.Simbol, c.DimensiuniObligatorii }).ToDictionary(c => c.ID);
        var angajamente = doc.Detalii.ToDictionary(d => d.ID, d => d.AngajamentId);
        var erori = new List<string>();
        foreach (var p in postari) {
            var c = p.Coordonate;
            var cont = conturi[c.Cont];
            MotorOperare.VerificaLatura(cont.Simbol, cont.DimensiuniObligatorii,
                GardAnaliza.Dimensiuni(c.Partener, c.Produs, c.Analiza),
                p.Cauza.Linie is Guid id ? angajamente.GetValueOrDefault(id) : null,
                c.Latura.ToString(), p.Cauza.Linie?.ToString(), erori);
        }
        if (erori.Count > 0) throw new OperareException(string.Join("\n", erori));
    }
}
