#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Rândul persistat ↔ postarea nucleului (S-D5), fără nicio citire în plus.</summary>
public static class Randuri {
    public static N.Postare Citeste(Postare rand) {
        ArgumentNullException.ThrowIfNull(rand);
        if (rand.DocumentId is null && rand.Tranzactie?.Fel != N.FelTranzactie.Deschidere)
            throw new InvalidOperationException(
                $"Postarea {rand.ID} fără document nu aparține deschiderii.");
        return new N.Postare(
            new N.Coordonate {
                Cont = rand.Cont,
                Latura = rand.Latura,
                Data = rand.Data,
                Partener = rand.Partener,
                Gestiune = rand.Gestiune,
                Produs = rand.Produs,
                Unitate = Unitatea(rand),
                CodTva = rand.TipTvaId is Guid tipTva && rand.SensTva is N.SensTva sens && rand.RolTva is N.RolTva rol
                    ? new N.CodTva(tipTva, sens, rol) {
                        Regim = rand.RegimTva!.Value, Cota = rand.CotaTva!.Value, DeImport = rand.DeImport!.Value,
                    }
                    : null,
                PerioadaDeclarare = rand.PerioadaDeclarare,
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
            };
    }

    public static void Scrie(N.Postare postare, Tranzactie tranzactie, Postare rand) {
        ArgumentNullException.ThrowIfNull(postare);
        ArgumentNullException.ThrowIfNull(tranzactie);
        ArgumentNullException.ThrowIfNull(rand);
        var coordonate = postare.Coordonate;
        var spatiu = N.Postari.Spatiu(postare);
        VerificaUnitatea(postare, spatiu);
        rand.Spatiu = spatiu;
        rand.Tranzactie = tranzactie;
        rand.DocumentId = tranzactie.Fel == N.FelTranzactie.Deschidere ? null : postare.Cauza.Document;
        rand.LinieId = postare.Cauza.Linie;
        rand.Data = coordonate.Data;
        rand.Cont = coordonate.Cont;
        rand.Latura = coordonate.Latura;
        rand.Partener = coordonate.Partener;
        rand.Gestiune = coordonate.Gestiune;
        rand.Produs = coordonate.Produs;
        rand.Unitate = coordonate.Unitate?.Id;
        rand.UnitateDeschisa = coordonate.Unitate?.Deschisa;
        rand.FelUnitate = coordonate.Unitate?.Fel;
        rand.SuportId = postare.Suport?.Id;
        rand.SuportSpatiu = postare.Suport?.Spatiu;
        rand.InversaDinId = postare.InversaDin?.Id;
        rand.InversaDinSpatiu = postare.InversaDin?.Spatiu;
        rand.TipTvaId = coordonate.CodTva?.TipTva;
        rand.SensTva = coordonate.CodTva?.Sens;
        rand.RolTva = coordonate.CodTva?.Rol;
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
        rand.Cantitate = postare.Cantitate;
        rand.ValoareValuta = postare.ValoareValuta;
        rand.Valoare = postare.Valoare;
    }

    static N.Unitate? Unitatea(Postare rand) {
        if (rand.Unitate is not Guid id)
            return null;
        var fel = rand.FelUnitate
            ?? throw new InvalidOperationException($"Postarea {rand.ID} are unitate fără fel.");
        return new N.Unitate(
            id,
            fel,
            rand.Cont,
            fel == N.FelUnitate.Partida ? rand.Partener : null,
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

    // Cont/Partener/Produs ale unității nu au coloane proprii: se citesc înapoi din
    // ale postării, deci o unitate care se abate de la ele s-ar pierde tăcut.
    static void VerificaUnitatea(N.Postare postare, N.Spatiu spatiu) {
        if (postare.Coordonate.Unitate is not { } unitate)
            return;
        var fel = unitate.Fel;
        if ((spatiu == N.Spatiu.Stoc) != (fel == N.FelUnitate.Lot))
            throw new InvalidOperationException($"Unitatea {unitate.Id} ({fel}) nu aparține spațiului {spatiu}.");
        var asteptat = new N.Unitate(
            unitate.Id,
            fel,
            postare.Coordonate.Cont,
            fel == N.FelUnitate.Partida ? postare.Coordonate.Partener : null,
            fel == N.FelUnitate.Lot ? postare.Coordonate.Produs : null,
            unitate.Deschisa);
        if (unitate != asteptat)
            throw new InvalidOperationException(
                $"Unitatea {unitate.Id} ({unitate.Fel}) nu se poate reconstrui din rând: "
                + $"așteptat {asteptat}, primit {unitate}.");
    }
}
