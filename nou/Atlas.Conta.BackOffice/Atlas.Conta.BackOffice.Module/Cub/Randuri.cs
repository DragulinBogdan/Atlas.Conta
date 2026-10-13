#nullable enable
using Atlas.Conta.BackOffice.Module.Declaratii;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Rândul persistat ↔ postarea nucleului (S-D5); felul repartitorului vine din afară (C111-D3).</summary>
public static class Randuri {
    /// <summary>Repartitorii rândurilor, citiți o singură dată.</summary>
    public static IReadOnlyDictionary<Guid, RepartitorFapt> Repartitori(IObjectSpace os, IEnumerable<Postare> randuri) =>
        Motor.Fapte.Repartitori(os, [.. randuri.Select(r => r.Repartitor).OfType<Guid>().Distinct()]);

    public static List<N.Postare> Citeste(IObjectSpace os, IReadOnlyCollection<Postare> randuri) {
        var repartitori = Repartitori(os, randuri);
        return [.. randuri.Select(r => Citeste(r, repartitori))];
    }

    public static N.Postare Citeste(Postare rand, IReadOnlyDictionary<Guid, RepartitorFapt> repartitori) {
        ArgumentNullException.ThrowIfNull(rand);
        ArgumentNullException.ThrowIfNull(repartitori);
        if (rand.DocumentId is null && rand.Tranzactie?.Fel != N.FelTranzactie.Deschidere)
            throw new InvalidOperationException(
                $"Postarea {rand.ID} fără document nu aparține deschiderii.");
        return new N.Postare(
            new N.Coordonate {
                Cont = rand.Cont,
                Sens = rand.Sens,
                Data = rand.Data,
                Repartitor = rand.Repartitor is Guid repartitor ? Motor.Fapte.Coordonata(repartitori, repartitor) : null,
                Contrapondere = rand.Contrapondere,
                Produs = rand.Produs,
                Unitate = Unitatea(rand),
                CodTva = rand.TipTvaId is Guid tipTva && rand.SensTva is N.SensTva sensTva && rand.RolTva is N.RolTva rol
                    ? new N.CodTva(tipTva, sensTva, rol) {
                        Regim = rand.RegimTva!.Value, Cota = rand.CotaTva!.Value, DeImport = rand.DeImport!.Value,
                    }
                    : null,
                PerioadaDeclarare = rand.PerioadaDeclarare,
                PartenerFiscal = rand.PartenerFiscal,
                Loc = rand.Loc,
                ReperFiscal = rand.TipTvaId == null ? null : new N.ReperFiscal(
                    rand.DocumentFiscalId!.Value, rand.DataDocument!.Value, rand.DataExigibilitate!.Value,
                    rand.DataPrimire, rand.DataInregistrare!.Value, rand.PerioadaD394!.Value,
                    rand.RegularizareD300, rand.InversaTehnica),
                Valuta = rand.Valuta,
                Carte = rand.Carte,
                Analiza = new N.Analiza(rand.CodFunctional, rand.CodEconomic, rand.SursaFinantare,
                    rand.UnitateOrganizatorica, rand.Proiect, rand.CentruCost),
            },
            rand.Cantitate,
            rand.ValoareValuta,
            rand.Valoare,
            new N.Cauza(rand.DocumentId ?? Guid.Empty, rand.LinieId),
            rand.Atribuit) {
                Suport = Referinta(rand.SuportId, rand.SuportSpatiu),
                InversaDin = Referinta(rand.InversaDinId, rand.InversaDinSpatiu),
                Pereche = rand.Pereche,
            };
    }

    public static void Scrie(N.Postare postare, Tranzactie tranzactie, int ordinal, Postare rand) {
        ArgumentNullException.ThrowIfNull(postare);
        ArgumentNullException.ThrowIfNull(tranzactie);
        ArgumentNullException.ThrowIfNull(rand);
        var coordonate = postare.Coordonate;
        var spatiu = N.Postari.Spatiu(postare);
        VerificaUnitatea(postare, spatiu);
        rand.Spatiu = spatiu;
        rand.Tranzactie = tranzactie;
        rand.Ordinal = ordinal;
        rand.DocumentId = tranzactie.Fel == N.FelTranzactie.Deschidere ? null : postare.Cauza.Document;
        rand.LinieId = postare.Cauza.Linie;
        rand.Data = coordonate.Data;
        rand.Cont = coordonate.Cont;
        rand.Sens = coordonate.Sens;
        rand.Repartitor = coordonate.Repartitor?.Id;
        rand.Contrapondere = coordonate.Contrapondere;
        rand.Produs = coordonate.Produs;
        rand.Unitate = coordonate.Unitate?.Id;
        rand.UnitateDeschisa = coordonate.Unitate?.Deschisa;
        rand.FelUnitate = coordonate.Unitate?.Fel;
        rand.SuportId = postare.Suport?.Id;
        rand.SuportSpatiu = postare.Suport?.Spatiu;
        rand.InversaDinId = postare.InversaDin?.Id;
        rand.InversaDinSpatiu = postare.InversaDin?.Spatiu;
        rand.TipTvaId = coordonate.CodTva?.TipTva;
        rand.SensTva = coordonate.CodTva?.SensTva;
        rand.RolTva = coordonate.CodTva?.Rol;
        rand.PartenerFiscal = coordonate.PartenerFiscal;
        rand.Loc = coordonate.Loc;
        rand.PerioadaDeclarare = coordonate.PerioadaDeclarare;
        rand.RegimTva = coordonate.CodTva?.Regim;
        rand.CotaTva = coordonate.CodTva?.Cota;
        rand.DeImport = coordonate.CodTva?.DeImport;
        rand.DocumentFiscalId = coordonate.ReperFiscal?.DocumentFiscal;
        rand.DataDocument = coordonate.ReperFiscal?.DataDocument;
        rand.DataExigibilitate = coordonate.ReperFiscal?.DataExigibilitate;
        rand.DataPrimire = coordonate.ReperFiscal?.DataPrimire;
        rand.DataInregistrare = coordonate.ReperFiscal?.DataInregistrare;
        rand.PerioadaD394 = coordonate.ReperFiscal?.PerioadaD394;
        rand.RegularizareD300 = coordonate.ReperFiscal?.RegularizareD300 ?? false;
        rand.InversaTehnica = coordonate.ReperFiscal?.InversaTehnica ?? false;
        rand.Valuta = coordonate.Valuta;
        rand.Carte = coordonate.Carte;
        rand.CodFunctional = coordonate.Analiza.CodFunctional;
        rand.CodEconomic = coordonate.Analiza.CodEconomic;
        rand.SursaFinantare = coordonate.Analiza.SursaFinantare;
        rand.UnitateOrganizatorica = coordonate.Analiza.UnitateOrganizatorica;
        rand.Proiect = coordonate.Analiza.Proiect;
        rand.CentruCost = coordonate.Analiza.CentruCost;
        rand.Atribuit = postare.Atribuit;
        rand.Pereche = postare.Pereche;
        rand.Cantitate = postare.Cantitate;
        rand.ValoareValuta = postare.ValoareValuta;
        rand.Valoare = postare.Valoare;
    }

    public static N.UnitateSold? Unitatea(Postare rand) {
        ArgumentNullException.ThrowIfNull(rand);
        if (rand.Unitate is not Guid id)
            return null;
        var fel = rand.FelUnitate
            ?? throw new InvalidOperationException($"Postarea {rand.ID} are unitate fără fel.");
        return new N.UnitateSold(
            id,
            fel,
            rand.Cont,
            fel == N.FelUnitate.Partida ? rand.Repartitor : null,
            fel == N.FelUnitate.Lot ? rand.Produs : null,
            rand.UnitateDeschisa
                ?? throw new InvalidOperationException($"Postarea {rand.ID} are unitate fără dată de deschidere."));
    }

    static N.ReferintaPostare? Referinta(Guid? id, N.Spatiu? spatiu) =>
        (id, spatiu) switch {
            (null, null) => null,
            (Guid cheie, N.Spatiu partitie) => new(cheie, partitie),
            _ => throw new InvalidOperationException("Referință de postare incompletă."),
        };

    // Contul, titularul și produsul unității nu au coloane proprii: se citesc înapoi din
    // ale postării, deci o unitate care se abate de la ele s-ar pierde tăcut.
    static void VerificaUnitatea(N.Postare postare, N.Spatiu spatiu) {
        if (postare.Coordonate.Unitate is not { } unitate)
            return;
        var fel = unitate.Fel;
        if ((spatiu == N.Spatiu.Stoc) != (fel == N.FelUnitate.Lot))
            throw new InvalidOperationException($"Unitatea {unitate.Id} ({fel}) nu aparține spațiului {spatiu}.");
        var asteptat = new N.UnitateSold(
            unitate.Id,
            fel,
            postare.Coordonate.Cont,
            fel == N.FelUnitate.Partida ? postare.Coordonate.Repartitor?.Id : null,
            fel == N.FelUnitate.Lot ? postare.Coordonate.Produs : null,
            unitate.Deschisa);
        if (unitate != asteptat)
            throw new InvalidOperationException(
                $"Unitatea {unitate.Id} ({unitate.Fel}) nu se poate reconstrui din rând: "
                + $"așteptat {asteptat}, primit {unitate}.");
    }
}
