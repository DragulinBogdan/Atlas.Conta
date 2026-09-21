#nullable enable
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

/// <summary>Rândul persistat ↔ postarea nucleului (S-D5), fără nicio citire în plus.</summary>
public static class Randuri {
    public static N.Postare Citeste(Postare rand) {
        ArgumentNullException.ThrowIfNull(rand);
        if (rand.DocumentId is not Guid document)
            throw new InvalidOperationException(
                $"Postarea {rand.ID} n-are document: deschiderea ca tranzacție e a lui TR-D7b (TR-r10).");
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
                    ? new N.CodTva(tipTva, sens, rol)
                    : null,
                PerioadaDeclarare = rand.PerioadaDeclarare,
                Valuta = rand.Valuta,
                Carte = rand.Carte,
                Analiza = new N.Analiza(rand.CodFunctional, rand.CodEconomic, rand.SursaFinantare,
                    rand.UnitateOrganizatorica, rand.Proiect, rand.CentruCost),
            },
            rand.Cantitate,
            rand.ValoareValuta,
            rand.Valoare,
            new N.Cauza(document, rand.LinieId),
            rand.Atribuit);
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
        rand.DocumentId = postare.Cauza.Document;
        rand.LinieId = postare.Cauza.Linie;
        rand.Data = coordonate.Data;
        rand.Cont = coordonate.Cont;
        rand.Latura = coordonate.Latura;
        rand.Partener = coordonate.Partener;
        rand.Gestiune = coordonate.Gestiune;
        rand.Produs = coordonate.Produs;
        rand.Unitate = coordonate.Unitate?.Id;
        rand.UnitateDeschisa = coordonate.Unitate?.Deschisa;
        rand.TipTvaId = coordonate.CodTva?.TipTva;
        rand.SensTva = coordonate.CodTva?.Sens;
        rand.RolTva = coordonate.CodTva?.Rol;
        rand.PerioadaDeclarare = coordonate.PerioadaDeclarare;
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
        var fel = Felul(rand.Spatiu);
        return new N.Unitate(
            id,
            fel,
            rand.Cont,
            fel == N.FelUnitate.Partida ? rand.Partener : null,
            fel == N.FelUnitate.Lot ? rand.Produs : null,
            rand.UnitateDeschisa
                ?? throw new InvalidOperationException($"Postarea {rand.ID} are unitate fără dată de deschidere."));
    }

    // S-D1: `FelUnitate` nu se persistă — la TR-D7 `Stoc ⇔ Lot`, `Contabil ⇒ Partida`.
    static N.FelUnitate Felul(N.Spatiu spatiu) =>
        spatiu == N.Spatiu.Stoc ? N.FelUnitate.Lot : N.FelUnitate.Partida;

    // Cont/Partener/Produs ale unității nu au coloane proprii: se citesc înapoi din
    // ale postării, deci o unitate care se abate de la ele s-ar pierde tăcut.
    static void VerificaUnitatea(N.Postare postare, N.Spatiu spatiu) {
        if (postare.Coordonate.Unitate is not { } unitate)
            return;
        var fel = Felul(spatiu);
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
