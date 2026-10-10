#pragma warning disable C111 // C111-D10
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static partial class Imobilizari {
    public sealed record ConturiFisa(Guid? Activ, Guid? Amortizare);

    internal static IQueryable<Postare> Nominalizari(IObjectSpace os) {
        var pif = os.GetObjectsQuery<PunereInFunctiuneDetaliu>().Select(l => l.ID);
        var amo = os.GetObjectsQuery<AmortizareLunaraDetaliu>().Select(l => l.ID);
        return os.GetObjectsQuery<Postare>().Where(p => p.FelUnitate == N.FelUnitate.Fisa
            && p.Unitate != null && p.InversaDinId == null && p.LinieId != null
            && (pif.Contains(p.LinieId.Value) || amo.Contains(p.LinieId.Value)));
    }

    public static IQueryable<Postare> PozitiiFaraFisa(IObjectSpace os) => os.GetObjectsQuery<Postare>()
        .Where(Transformare.FaraContrapondere)
        .Where(p => p.Spatiu == N.Spatiu.Contabil && p.Carte == N.Carte.Contabil && p.Unitate == null);

    public sealed class PozitieCitita {
        public Guid ID { get; set; }
        public N.Spatiu Spatiu { get; set; }
        public N.FelTranzactie Fel { get; set; }
        public Guid? LinieId { get; set; }
        public DateOnly Data { get; set; }
        public N.Latura Latura { get; set; }
        public decimal Valoare { get; set; }
        public Guid Cont { get; set; }
        public Guid? Tert { get; set; }
        public Guid? Loc { get; set; }
        public Guid? Produs { get; set; }
        public Guid? Valuta { get; set; }
        public Guid? CodFunctional { get; set; }
        public Guid? CodEconomic { get; set; }
        public Guid? SursaFinantare { get; set; }
        public Guid? UnitateOrganizatorica { get; set; }
        public Guid? Proiect { get; set; }
        public Guid? CentruCost { get; set; }
        public Guid? SuportId { get; set; }
        public Guid? InversaDinId { get; set; }
        public bool Stornata { get; set; }
        public bool FaraLinieSursa { get; set; }
    }

    /// <summary>Pozițiile fără fișă ale altor documente pe conturile date.</summary>
    public static List<PozitieCitita> PozitiiCitite(IObjectSpace os, List<Guid> conturi, Guid faraDocument) {
        var note = os.GetObjectsQuery<NotaContabila>().Where(d => d.Stare == StareDocument.Operat).Select(d => d.ID);
        var stornate = os.GetObjectsQuery<Document>().Where(d => d.Stare == StareDocument.Stornat).Select(d => d.ID);
        return PozitiiFaraFisa(os)
            .Where(p => conturi.Contains(p.Cont) && p.DocumentId != faraDocument)
            .Select(p => new PozitieCitita {
                ID = p.ID, Spatiu = p.Spatiu, Fel = p.Tranzactie.Fel, LinieId = p.LinieId, Data = p.Data,
                Latura = p.Latura, Valoare = p.Valoare, Cont = p.Cont, Tert = p.Partener, Loc = p.Gestiune,
                Produs = p.Produs, Valuta = p.Valuta, CodFunctional = p.CodFunctional, CodEconomic = p.CodEconomic,
                SursaFinantare = p.SursaFinantare, UnitateOrganizatorica = p.UnitateOrganizatorica,
                Proiect = p.Proiect, CentruCost = p.CentruCost, SuportId = p.SuportId, InversaDinId = p.InversaDinId,
                Stornata = p.DocumentId != null && stornate.Contains(p.DocumentId.Value),
                FaraLinieSursa = p.Tranzactie.Fel == N.FelTranzactie.Deschidere
                    || (p.DocumentId != null && note.Contains(p.DocumentId.Value)) })
            .ToList();
    }

    public static IReadOnlyDictionary<Guid, ConturiFisa> Conturi(IObjectSpace os, List<Guid> fise,
            DateOnly laData, Guid? exceptie = null) {
        var inverse = os.GetObjectsQuery<Postare>().Where(p => p.InversaDinId != null && p.Data <= laData)
            .Select(p => p.InversaDinId);
        var pozitii = Nominalizari(os).Where(p => fise.Contains(p.Unitate.Value) && p.Data <= laData
                && p.DocumentId != exceptie && !inverse.Contains(p.ID))
            .Select(p => new { Fisa = p.Unitate.Value, p.Cont, p.Latura }).Distinct().ToList();
        return pozitii.GroupBy(p => p.Fisa).ToDictionary(g => g.Key, g => {
            Guid? Cont(N.Latura latura) {
                var conturi = g.Where(p => p.Latura == latura).Select(p => p.Cont).Distinct().ToArray();
                if (conturi.Length > 1) throw new OperareException(
                    $"Fișa {g.Key} are conturi istorice multiple pe {latura}; este necesară reconcilierea istoricului.");
                return conturi.Length == 0 ? null : conturi[0];
            }
            return new ConturiFisa(Cont(N.Latura.Debit), Cont(N.Latura.Credit));
        });
    }
}
