namespace Atlas.Conta.Nucleu;

public static class Conservare {
    public static IReadOnlyList<Refuz> Verifica(Tranzactie tranzactie) {
        var refuzuri = new List<Refuz>();
        VerificaDocumentul(tranzactie, refuzuri);
        // C6 (N-D3)
        if (tranzactie.Fel != FelTranzactie.Deschidere) {
            VerificaValoarea(tranzactie, refuzuri);
            VerificaCantitatea(tranzactie, refuzuri);
        }
        if (tranzactie.Fel == FelTranzactie.Transfer)
            VerificaTransferul(tranzactie, refuzuri);
        VerificaSemnul(tranzactie, refuzuri);
        VerificaFormele(tranzactie, refuzuri);
        return refuzuri;
    }

    static void VerificaDocumentul(Tranzactie tranzactie, List<Refuz> refuzuri) {
        if (tranzactie.Fel == FelTranzactie.Deschidere) {
            if (tranzactie.Document is not null)
                refuzuri.Add(new Refuz(
                    Coduri.DocumentNeasteptat,
                    $"deschiderea nu are document, dar tranzacția poartă {tranzactie.Document}",
                    null));
            return;
        }
        if (tranzactie.Document is not { } document) {
            refuzuri.Add(new Refuz(
                Coduri.DocumentLipsa,
                $"tranzacția de fel {tranzactie.Fel} nu are document",
                null));
            return;
        }
        // 090i: stornoul inversează și postările ATRIBUITE, care poartă cauza altui document.
        var atribuitulEPermis = tranzactie.Fel == FelTranzactie.Storno;
        foreach (var postare in tranzactie.Postari)
            if (postare.Cauza.Document != document && !(atribuitulEPermis && postare.Atribuit is not null))
                refuzuri.Add(new Refuz(
                    Coduri.CauzaStraina,
                    $"postarea are cauza pe documentul {postare.Cauza.Document}, nu pe {document}",
                    postare.Cauza.Linie));
    }

    static void VerificaValoarea(Tranzactie tranzactie, List<Refuz> refuzuri) {
        var suma = 0m;
        foreach (var postare in tranzactie.Postari)
            suma += postare.Coordonate.Latura == Latura.Debit ? postare.Valoare : -postare.Valoare;
        if (suma != 0m)
            refuzuri.Add(new Refuz(
                Coduri.ConservareValoare,
                $"Σ debit − Σ credit = {suma}, nu 0",
                null));
    }

    static void VerificaCantitatea(Tranzactie tranzactie, List<Refuz> refuzuri) {
        // N-D4
        foreach (var (produs, suma) in Aduna(
                     tranzactie.Postari.Where(p => p.Cantitate != 0m),
                     p => p.Coordonate.Produs,
                     p => p.Cantitate))
            if (suma != 0m)
                refuzuri.Add(new Refuz(
                    Coduri.ConservareCantitate,
                    $"Σ cantitate pe produsul {Nume(produs)} = {suma}, nu 0",
                    null));
    }

    static void VerificaTransferul(Tranzactie tranzactie, List<Refuz> refuzuri) {
        foreach (var (cheie, suma) in Aduna(
                     tranzactie.Postari,
                     p => (Cont: p.Coordonate.Cont, Latura: p.Coordonate.Latura),
                     p => p.Valoare))
            if (suma != 0m)
                refuzuri.Add(new Refuz(
                    Coduri.ConservareTransfer,
                    $"Σ valoare pe (contul {cheie.Cont}, {cheie.Latura}) = {suma}, nu 0",
                    null));
        foreach (var (cheie, suma) in Aduna(
                     tranzactie.Postari.Where(p => p.Cantitate != 0m),
                     p => (Cont: p.Coordonate.Cont, Produs: p.Coordonate.Produs),
                     p => p.Cantitate))
            if (suma != 0m)
                refuzuri.Add(new Refuz(
                    Coduri.ConservareTransfer,
                    $"Σ cantitate pe (contul {cheie.Cont}, produsul {Nume(cheie.Produs)}) = {suma}, nu 0",
                    null));
    }

    static void VerificaSemnul(Tranzactie tranzactie, List<Refuz> refuzuri) {
        if (tranzactie.Fel is not (FelTranzactie.Operare or FelTranzactie.Deschidere))
            return;
        foreach (var postare in tranzactie.Postari)
            if (postare.Valoare < 0m)
                refuzuri.Add(new Refuz(
                    Coduri.SemnNegativ,
                    $"valoarea {postare.Valoare} e negativă într-o tranzacție de fel {tranzactie.Fel}",
                    postare.Cauza.Linie));
    }

    static void VerificaFormele(Tranzactie tranzactie, List<Refuz> refuzuri) {
        foreach (var postare in tranzactie.Postari) {
            var coordonate = postare.Coordonate;
            var linie = postare.Cauza.Linie;
            if (postare.Cantitate != 0m) {
                if (coordonate.Gestiune is null)
                    refuzuri.Add(new Refuz(
                        Coduri.GestiuneLipsa,
                        $"cantitatea {postare.Cantitate} nu are gestiune",
                        linie));
                if (coordonate.Unitate is null)
                    refuzuri.Add(new Refuz(
                        Coduri.UnitateLipsa,
                        $"cantitatea {postare.Cantitate} nu are unitate",
                        linie));
            }
            if (coordonate.Unitate is not { } unitate)
                continue;
            switch (unitate.Fel) {
                case FelUnitate.Lot:
                    if (coordonate.Produs is null)
                        refuzuri.Add(new Refuz(Coduri.ProdusLipsa, "lotul nu are produs", linie));
                    if (coordonate.Gestiune is null)
                        refuzuri.Add(new Refuz(Coduri.GestiuneLipsa, "lotul nu are gestiune", linie));
                    if (coordonate.Produs != unitate.Produs)
                        refuzuri.Add(new Refuz(
                            Coduri.UnitateNepotrivita,
                            $"lotul {unitate.Id} e pe produsul {Nume(unitate.Produs)}, postarea pe {Nume(coordonate.Produs)}",
                            linie));
                    break;
                case FelUnitate.Partida:
                    if (coordonate.Partener is null)
                        refuzuri.Add(new Refuz(Coduri.PartenerLipsa, "partida nu are partener", linie));
                    if (coordonate.Partener != unitate.Partener)
                        refuzuri.Add(new Refuz(
                            Coduri.UnitateNepotrivita,
                            $"partida {unitate.Id} e pe partenerul {Nume(unitate.Partener)}, postarea pe {Nume(coordonate.Partener)}",
                            linie));
                    break;
            }
        }
    }

    // `Dictionary<Guid?,_>` cade pe CS8714 sub TreatWarningsAsErrors.
    static List<(TCheie Cheie, decimal Suma)> Aduna<TCheie>(
        IEnumerable<Postare> postari,
        Func<Postare, TCheie> cheie,
        Func<Postare, decimal> masura) {
        var chei = new List<TCheie>();
        var sume = new List<decimal>();
        foreach (var postare in postari) {
            var k = cheie(postare);
            var i = chei.IndexOf(k);
            if (i < 0) {
                chei.Add(k);
                sume.Add(0m);
                i = chei.Count - 1;
            }
            sume[i] += masura(postare);
        }
        return chei.Select((k, i) => (k, sume[i])).ToList();
    }

    static string Nume(Guid? id) => id?.ToString() ?? "(fără)";
}
