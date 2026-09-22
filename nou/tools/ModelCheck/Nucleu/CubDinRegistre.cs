#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Portul fidel al lui <c>run-nucleu/fizica/pas1/02-transform.sql</c> (B-D7):
/// rândurile de registru ale unui set de documente devin tranzacții ale cubului.
/// </summary>
static class CubDinRegistre {
    /// <summary>Contul santinelă al postărilor fiscale: registrul fiscal n-are cont, nucleul îl cere.</summary>
    public static readonly Guid ContFiscal = new("f15ca100-0000-4000-8000-000000000001");

    sealed record LotFizic(Guid Id, Guid ProdusId, DateOnly Data, Guid Cont);

    sealed record TertDoc(Guid Cont, N.Latura Latura, Guid? Partener, DateOnly Data);

    // Postgres nu garantează nicio ordine fără `ORDER BY`, iar normalizările aleg
    // „primul candidat" din listă: oracolul se citește pe secvența LINIILOR sursei,
    // aceeași pe care o folosesc ambele motoare (S-D6/B-r11).
    static List<T> Ordonate<T>(
            List<T> randuri,
            Func<T, (Guid? Document, Guid? Linie, Guid Id)> cheie,
            IReadOnlyDictionary<Guid, int> pozitii) =>
        [.. randuri.OrderBy(rand => {
            var (document, linie, id) = cheie(rand);
            return (document ?? Guid.Empty,
                linie is Guid alLiniei ? pozitii.GetValueOrDefault(alLiniei) : 0,
                linie ?? Guid.Empty,
                id);
        })];

    /// <summary>
    /// Ceilalți stingători ai documentelor stinse de <paramref name="document"/>: plafonul
    /// unei împerecheri e RESTUL partidei stinsului, deci oracolul trebuie să vadă TOATE
    /// împerecherile ei, nu doar pe ale unui stingător (MEDIU-3).
    /// </summary>
    public static List<Guid> StingatoriiVecini(IObjectSpace os, Guid document) {
        ArgumentNullException.ThrowIfNull(os);
        return [.. os.GetObjectsQuery<Imperechere>()
            .Where(v => v.DocumentStingatorId != document
                && os.GetObjectsQuery<Imperechere>()
                    .Any(i => i.DocumentStingatorId == document && i.DocumentId == v.DocumentId))
            .Select(v => v.DocumentStingatorId)
            .Distinct()
            .ToList()];
    }

    public static IReadOnlyList<N.Tranzactie> Transforma(IObjectSpace os, IReadOnlyCollection<Guid> documente) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(documente);
        var ids = documente.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var imperecheri = os.GetObjectsQuery<Imperechere>()
            .Where(i => ids.Contains(i.DocumentStingatorId))
            .Select(i => new { i.ID, i.DocumentStingatorId, i.DocumentId, i.Suma, i.Data, i.Autogenerat })
            .ToList()
            .OrderBy(i => (i.Data, i.ID))
            .ToList();
        // `_TertDoc` al SQL-ului se calculează peste TOATE rândurile contabile: partida
        // documentului stins e nevoie chiar dacă el nu e în setul cerut.
        var idsTot = ids.Concat(imperecheri.Select(i => i.DocumentId)).Distinct().ToList();

        var dateDocument = os.GetObjectsQuery<Document>()
            .Where(d => idsTot.Contains(d.ID))
            .Select(d => new { d.ID, d.DataInregistrare })
            .ToList()
            .ToDictionary(d => d.ID, d => d.DataInregistrare);

        var pozitii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => idsTot.Contains(d.DocumentId))
            .Select(d => new { d.ID, d.Pozitie })
            .ToList()
            .ToDictionary(d => d.ID, d => d.Pozitie);

        var contabile = Ordonate(
            os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && idsTot.Contains(r.DocumentId.Value))
                .ToList(),
            r => (r.DocumentId, r.DetaliuId, r.ID), pozitii);
        var stoc = Ordonate(
            os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value))
                .ToList(),
            r => (r.DocumentId, r.DetaliuId, r.ID), pozitii);
        var fiscale = Ordonate(
            os.GetObjectsQuery<RegistruTva>()
                .Where(r => ids.Contains(r.DocumentId))
                .ToList(),
            r => (r.DocumentId, r.DetaliuId, r.ID), pozitii);

        var stornate = contabile.Count(r => r.Storno) + stoc.Count(r => r.Storno) + fiscale.Count(r => r.Storno);
        if (stornate > 0)
            Console.WriteLine($"     CubDinRegistre: {stornate} rânduri cu Storno excluse — "
                + "stornoul e tranzacție distinctă, nu intră în oracolul pilotului (B-D7).");
        contabile = [.. contabile.Where(r => !r.Storno)];
        stoc = [.. stoc.Where(r => !r.Storno)];
        fiscale = [.. fiscale.Where(r => !r.Storno)];

        var rolTert = ConturiRolTert(os, contabile);
        var felRepartitor = FeluriRepartitor(os, contabile);
        var loturi = Loturi(os, stoc);

        var postari = new Dictionary<Guid, List<N.Postare>>();
        foreach (var id in ids)
            postari[id] = [];

        foreach (var r in contabile) {
            if (r.DocumentId is not Guid document || !postari.TryGetValue(document, out var ale))
                continue;
            var data = dateDocument[document];
            ale.Add(Contabila(r, document, N.Latura.Debit, data, rolTert, felRepartitor));
            ale.Add(Contabila(r, document, N.Latura.Credit, data, rolTert, felRepartitor));
        }

        // T-D2: rândurile aceluiași lot (deci ACELUIAȘI cont) cu o latură negativă și
        // una pozitivă sunt o MUTARE între gestiuni — tranzacție `Transfer` a
        // documentului. Restul rândurilor rămân în `Operare`.
        var liniiContabile = contabile
            .Select(r => (Document: r.DocumentId!.Value, Linie: r.DetaliuId))
            .ToHashSet();
        var mutate = new HashSet<(Guid Document, Guid Lot)>();
        foreach (var grup in stoc.GroupBy(r => (Document: r.DocumentId!.Value, Lot: r.LotId)))
            if (EMutare([.. grup], grup.Key.Document, liniiContabile))
                mutate.Add(grup.Key);
        var deTransfer = new Dictionary<Guid, List<N.Postare>>();
        foreach (var r in stoc)
            if (r.DocumentId is Guid document) {
                var data = dateDocument[document];
                if (!mutate.Contains((document, r.LotId))) {
                    postari[document].Add(DeStoc(r, document, data, loturi));
                    continue;
                }
                if (!deTransfer.TryGetValue(document, out var ale))
                    deTransfer[document] = ale = [];
                ale.Add(DeMutare(r, document, data, loturi));
            }

        foreach (var r in fiscale)
            postari[r.DocumentId].AddRange(Fiscale(r, dateDocument[r.DocumentId]));

        var tranzactii = new List<N.Tranzactie>();
        foreach (var id in ids) {
            if (postari[id].Count > 0)
                tranzactii.Add(new N.Tranzactie(N.FelTranzactie.Operare, dateDocument[id], id, postari[id]));
            if (deTransfer.TryGetValue(id, out var mutari))
                tranzactii.Add(new N.Tranzactie(N.FelTranzactie.Transfer, dateDocument[id], id, mutari));
        }

        if (imperecheri.Count > 0) {
            var tertDoc = TertPeDocument(contabile, dateDocument, rolTert, felRepartitor);
            // MAJOR-A: partida stinsului ține și recepția, care stă pe NIR-ul lui conex
            // (TR-D3) — soldul ei se citește pe stins ∪ conexele lui autogenerate.
            var soldPeCont = SoldPeCont(contabile, ConexeleContabile(os, idsTot));
            // MEDIU-3: plafonul e RESTUL partidei, deci soldul scade cu ce au mutat
            // împerecherile ANTERIOARE (ordinea `(Data, ID)`), iar rândul invers
            // desface EXACT ce a mutat ACEST stingător.
            var mutatPeStins = new Dictionary<(Guid Stins, Guid Cont), decimal>();
            var mutatPePereche = new Dictionary<(Guid Stingator, Guid Stins, Guid Cont), decimal>();
            foreach (var imp in imperecheri)
                if (DeImperechere(imp.DocumentStingatorId, imp.DocumentId, imp.Suma, imp.Data,
                        imp.Autogenerat ? null : imp.ID,
                        tertDoc, soldPeCont, mutatPeStins, mutatPePereche, dateDocument) is { } tranzactie)
                    tranzactii.Add(tranzactie);
        }
        return tranzactii;
    }

    // Rândurile contabile ale conexelor autogenerate ale setului, cheiate pe SURSĂ
    // (TR-D3): aceeași legătură pe care o citește `Normalizari.Context.SursaConexului`
    // — documentul autogenerat al tipului-țintă declarat de `PoliticiConex`, nu
    // SECUNDARUL (plata automată), care are declarația lui.
    static List<(Guid Sursa, Guid ContDebit, Guid ContCredit, decimal Valoare)> ConexeleContabile(
            IObjectSpace os, IReadOnlyList<Guid> documente) {
        var copii = os.GetObjectsQuery<Document>()
            .Where(c => c.Autogenerat && c.DocumentSursaId != null
                && documente.Contains(c.DocumentSursaId.Value))
            .Select(c => new { c.ID, c.ClrType, Sursa = c.DocumentSursaId!.Value })
            .ToList();
        if (copii.Count == 0)
            return [];
        var tipuri = os.GetObjectsQuery<TipDocument>()
            .Select(t => new { t.ID, t.ClrType })
            .ToList();
        var clrPerTip = tipuri.Where(t => t.ClrType != null).ToDictionary(t => t.ID, t => t.ClrType!);
        var tintaPerClr = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var politica in os.GetObjectsQuery<PoliticaConex>()
                .Select(p => new { p.TipDocumentSursaId, p.TipDocumentTintaId })
                .ToList())
            if (clrPerTip.TryGetValue(politica.TipDocumentSursaId, out var sursa)
                    && clrPerTip.TryGetValue(politica.TipDocumentTintaId, out var tinta))
                tintaPerClr[sursa] = tinta;
        if (tintaPerClr.Count == 0)
            return [];
        var clrSursei = os.GetObjectsQuery<Document>()
            .Where(d => documente.Contains(d.ID))
            .Select(d => new { d.ID, d.ClrType })
            .ToList()
            .ToDictionary(d => d.ID, d => d.ClrType);
        var conexe = new Dictionary<Guid, Guid>();
        foreach (var copil in copii)
            if (clrSursei.GetValueOrDefault(copil.Sursa) is { } alSursei
                    && tintaPerClr.GetValueOrDefault(alSursei) == copil.ClrType)
                conexe[copil.ID] = copil.Sursa;
        if (conexe.Count == 0)
            return [];
        var ids = conexe.Keys.ToList();
        return [.. os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value) && !r.Storno)
            .Select(r => new { r.DocumentId, r.ContDebitId, r.ContCreditId, r.Valoare })
            .ToList()
            .Select(r => (conexe[r.DocumentId!.Value], r.ContDebitId, r.ContCreditId, r.Valoare))];
    }

    static N.Postare Contabila(
            RegistruContabil r,
            Guid document,
            N.Latura latura,
            DateOnly data,
            IReadOnlyDictionary<Guid, RolTertCont> rolTert,
            IReadOnlyDictionary<Guid, string> felRepartitor) {
        var peDebit = latura == N.Latura.Debit;
        var cont = peDebit ? r.ContDebitId : r.ContCreditId;
        var alLaturii = peDebit ? r.DebitRepartitorId : r.CreditRepartitorId;
        var alPerechii = peDebit ? r.CreditRepartitorId : r.DebitRepartitorId;
        var partener = Tert(alLaturii, felRepartitor) ?? Tert(alPerechii, felRepartitor);
        N.Unitate? unitate = null;
        if (rolTert.GetValueOrDefault(cont) != RolTertCont.Niciunul) {
            if (partener is not Guid tert)
                throw new InvalidOperationException(
                    $"Rândul contabil {r.ID} al documentului {document} postează pe contul de terț {cont} "
                    + "fără repartitor de tip Partener/Angajat pe niciuna dintre laturi: partida nu se poate "
                    + "deschide (C5 cere partener pe partidă). B-D10, oprire.");
            unitate = N.Unitate.DeschidePartida(cont, tert, document, data);
        }
        return new N.Postare(
            new N.Coordonate {
                Cont = cont,
                Latura = latura,
                Data = data,
                Partener = partener,
                Gestiune = Gestiune(alLaturii, felRepartitor),
                Produs = peDebit ? r.DebitMaterialId : r.CreditMaterialId,
                Unitate = unitate,
                Analiza = Analiza(r, peDebit),
            },
            0m,
            0m,
            r.Valoare,
            new N.Cauza(document, r.DetaliuId));
    }

    // Măsurile poartă semnul în registru; nucleul cere `Latura` pe orice postare și
    // valoare pozitivă în `Operare`, deci semnul valorii trece pe latură (B-D7).
    // T-D2: mutarea e ieșirea și intrarea aceluiași lot, deci ale aceluiași cont, care se
    // sting între ele. Linia CU picior contabil duce valoarea pe ALT cont (consumul, vânzarea):
    // acolo rândul de stoc e piciorul unificat de TR-D4, nu o mutare.
    static bool EMutare(
            IReadOnlyList<RegistruStoc> randuri,
            Guid document,
            IReadOnlySet<(Guid Document, Guid? Linie)> liniiContabile) =>
        randuri.Any(Negativ) && randuri.Any(r => !Negativ(r))
        && randuri.Sum(r => r.Cantitate) == 0m && randuri.Sum(r => r.Valoare) == 0m
        && !randuri.Any(r => liniiContabile.Contains((document, r.DetaliuId)));

    static bool Negativ(RegistruStoc r) => r.Valoare != 0m ? r.Valoare < 0m : r.Cantitate < 0m;

    // 090f: capetele mutării stau pe ACEEAȘI latură, cu măsurile semnate.
    static N.Postare DeMutare(
            RegistruStoc r, Guid document, DateOnly data, IReadOnlyDictionary<Guid, LotFizic> loturi) {
        var lot = Lotul(r, document, loturi);
        return new N.Postare(
            new N.Coordonate {
                Cont = lot.Cont,
                Latura = N.Latura.Debit,
                Data = data,
                Gestiune = r.RepartitorId,
                Produs = lot.ProdusId,
                Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, lot.Cont, null, lot.ProdusId, lot.Data),
            },
            r.Cantitate,
            0m,
            r.Valoare,
            new N.Cauza(document, r.DetaliuId));
    }

    static LotFizic Lotul(
            RegistruStoc r, Guid document, IReadOnlyDictionary<Guid, LotFizic> loturi) =>
        loturi.TryGetValue(r.LotId, out var lot)
            ? lot
            : throw new InvalidOperationException(
                $"Rândul de stoc {r.ID} al documentului {document} numește lotul {r.LotId}, "
                + "care n-are produs cu tip și cont implicit: contul postării nu se poate rezolva. B-D10, oprire.");

    static N.Postare DeStoc(
            RegistruStoc r, Guid document, DateOnly data, IReadOnlyDictionary<Guid, LotFizic> loturi) {
        var lot = Lotul(r, document, loturi);
        return new N.Postare(
            new N.Coordonate {
                Cont = lot.Cont,
                // Semnul valorii dă latura; la valoare zero (lot primit gratuit) o dă
                // semnul cantității, altfel TR-D4 ar căuta perechea pe latura greșită (MINOR-4).
                Latura = (r.Valoare != 0m ? r.Valoare >= 0m : r.Cantitate >= 0m)
                    ? N.Latura.Debit
                    : N.Latura.Credit,
                Data = data,
                Gestiune = r.RepartitorId,
                Produs = lot.ProdusId,
                Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, lot.Cont, null, lot.ProdusId, lot.Data),
            },
            r.Cantitate,
            0m,
            Math.Abs(r.Valoare),
            new N.Cauza(document, r.DetaliuId));
    }

    static IEnumerable<N.Postare> Fiscale(RegistruTva r, DateOnly data) {
        // `Data` fiscală = data documentului fizic; în cub toate postările sunt datate
        // ca tranzacția (DATA_STRAINA), iar reperul fiscal e `PerioadaDeclarare` (B-D8 pct. 5).
        foreach (var (rol, valoare) in new[] { (N.RolTva.Baza, r.Baza), (N.RolTva.Taxa, r.Tva) })
            yield return new N.Postare(
                new N.Coordonate {
                    Cont = ContFiscal,
                    Latura = N.Latura.Debit,
                    Data = data,
                    Partener = r.PartenerId,
                    CodTva = new N.CodTva(
                        r.TipTvaId,
                        r.Sens == SensTva.Achizitie ? N.SensTva.Achizitie : N.SensTva.Livrare,
                        rol),
                    PerioadaDeclarare = (r.PerioadaAn * 100) + r.PerioadaLuna,
                },
                0m,
                0m,
                valoare,
                new N.Cauza(r.DocumentId, r.DetaliuId));
    }

    // Amendamentul 3 al transformării: împerecherea e tranzacție datată, pe contul și
    // latura postării de terț de referință a STINGĂTORULUI, cu partenerul STINSULUI.
    static N.Tranzactie? DeImperechere(
            Guid stingator,
            Guid stins,
            decimal suma,
            DateOnly data,
            Guid? ulterioara,
            IReadOnlyDictionary<Guid, TertDoc> tertDoc,
            IReadOnlyDictionary<(Guid Document, Guid Cont), decimal> soldPeCont,
            Dictionary<(Guid Stins, Guid Cont), decimal> mutatPeStins,
            Dictionary<(Guid Stingator, Guid Stins, Guid Cont), decimal> mutatPePereche,
            IReadOnlyDictionary<Guid, DateOnly> dateDocument) {
        // B-D8 pct. 10: fără cont cu `RolTert` (profilul bugetar) stingerea n-are partidă
        // pe care s-o mute, deci împerecherea n-are corespondent în cub.
        if (!tertDoc.TryGetValue(stingator, out var referinta)) {
            Console.WriteLine($"     CubDinRegistre: împerecherea {stingator} → {stins} n-are postare de "
                + "terț cu partidă (profil fără RolTert) — tranzacția de împerechere nu intră în oracol.");
            return null;
        }
        var partener = tertDoc.GetValueOrDefault(stins)?.Partener ?? referinta.Partener;
        if (partener is not Guid tert)
            throw new InvalidOperationException(
                $"Împerecherea {stingator} → {stins} n-are partener pe niciuna dintre partide. B-D10, oprire.");
        // MAJOR-A/MEDIU-3: împerecherea e pe DOCUMENT, partida e pe CONT — se mută cel
        // mult RESTUL partidei stinsului pe contul de referință (soldul lui cu conexul
        // absorbit, minus ce au mutat împerecherile anterioare); restul rămâne pe a
        // stingătorului, ca la declarant.
        // F27-D8: `Imperecheri` e ALGEBRIC — rândul INVERS al unei desfaceri poartă
        // sumă negativă și mută înapoi, de pe partida stinsului pe a stingătorului.
        var semn = suma < 0m ? -1m : 1m;
        var cheieStins = (stins, referinta.Cont);
        var cheiePereche = (stingator, stins, referinta.Cont);
        // Latura de referință dă direcția în care transferul duce soldul spre zero.
        var directie = referinta.Latura == N.Latura.Debit ? 1m : -1m;
        var plafon = semn < 0m
            ? Math.Abs(mutatPePereche.GetValueOrDefault(cheiePereche))
            : Math.Abs(soldPeCont.GetValueOrDefault(cheieStins)
                + (directie * mutatPeStins.GetValueOrDefault(cheieStins)));
        var mutata = Math.Min(Math.Abs(suma), plafon);
        if (mutata <= 0m) {
            Console.WriteLine($"     CubDinRegistre: împerecherea {stingator} → {stins} nu mută nimic — "
                + (semn < 0m
                    ? $"partida stinsului n-a primit nimic de la acest stingător pe {referinta.Cont}"
                    : $"stinsul n-are rest pe contul de referință {referinta.Cont}")
                + " (MAJOR-A).");
            return null;
        }
        mutatPeStins[cheieStins] = mutatPeStins.GetValueOrDefault(cheieStins) + (semn * mutata);
        mutatPePereche[cheiePereche] = mutatPePereche.GetValueOrDefault(cheiePereche) + (semn * mutata);
        N.Postare Pe(Guid document, decimal valoare) => new(
            new N.Coordonate {
                Cont = referinta.Cont,
                Latura = referinta.Latura,
                Data = data,
                Partener = tert,
                Unitate = N.Unitate.DeschidePartida(
                    referinta.Cont, tert, document, dateDocument.GetValueOrDefault(document, data)),
            },
            0m,
            0m,
            valoare,
            // S-D13: transferul ULTERIOAR operării se recunoaște după identitatea împerecherii
            // pe cauză — piciorul lui de bani nu se sparge (amendament B-D8 pct. 11).
            new N.Cauza(stingator, ulterioara));
        return new N.Tranzactie(
            N.FelTranzactie.Transfer, data, stingator,
            [Pe(stingator, -semn * mutata), Pe(stins, semn * mutata)]);
    }

    // Soldul semnat (D − C) al fiecărui document pe fiecare cont atins: plafonul
    // nominalizării (MAJOR-A). Rândurile conexului autogenerat intră pe SURSĂ (TR-D3).
    static Dictionary<(Guid Document, Guid Cont), decimal> SoldPeCont(
            IReadOnlyList<RegistruContabil> contabile,
            IReadOnlyList<(Guid Sursa, Guid ContDebit, Guid ContCredit, decimal Valoare)> aleConexelor) {
        var sume = new Dictionary<(Guid Document, Guid Cont), decimal>();
        void Adauga(Guid document, Guid debit, Guid credit, decimal valoare) {
            sume[(document, debit)] = sume.GetValueOrDefault((document, debit)) + valoare;
            sume[(document, credit)] = sume.GetValueOrDefault((document, credit)) - valoare;
        }
        foreach (var r in contabile)
            if (r.DocumentId is Guid document)
                Adauga(document, r.ContDebitId, r.ContCreditId, r.Valoare);
        foreach (var (sursa, debit, credit, valoare) in aleConexelor)
            Adauga(sursa, debit, credit, valoare);
        return sume;
    }

    // Partida de referință a unui document = postarea lui de terț cu |Valoare| maximă
    // (`ORDER BY abs(Valoare) DESC, RandId, Latura` al lui `_TertDoc`).
    static Dictionary<Guid, TertDoc> TertPeDocument(
            IReadOnlyList<RegistruContabil> contabile,
            IReadOnlyDictionary<Guid, DateOnly> dateDocument,
            IReadOnlyDictionary<Guid, RolTertCont> rolTert,
            IReadOnlyDictionary<Guid, string> felRepartitor) {
        var candidati = new List<(Guid Document, decimal Absolut, Guid Rand, N.Latura Latura, TertDoc Tert)>();
        foreach (var r in contabile) {
            if (r.DocumentId is not Guid document)
                continue;
            foreach (var latura in new[] { N.Latura.Debit, N.Latura.Credit }) {
                var peDebit = latura == N.Latura.Debit;
                var cont = peDebit ? r.ContDebitId : r.ContCreditId;
                if (rolTert.GetValueOrDefault(cont) == RolTertCont.Niciunul)
                    continue;
                var alLaturii = peDebit ? r.DebitRepartitorId : r.CreditRepartitorId;
                var alPerechii = peDebit ? r.CreditRepartitorId : r.DebitRepartitorId;
                candidati.Add((document, Math.Abs(r.Valoare), r.ID, latura,
                    new TertDoc(cont, latura, Tert(alLaturii, felRepartitor) ?? Tert(alPerechii, felRepartitor),
                        dateDocument.GetValueOrDefault(document))));
            }
        }
        return candidati
            .GroupBy(c => c.Document)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(c => c.Absolut).ThenBy(c => c.Rand).ThenBy(c => c.Latura)
                    .First().Tert);
    }

    static Guid? Tert(Guid? repartitor, IReadOnlyDictionary<Guid, string> felRepartitor) =>
        repartitor is Guid id && felRepartitor.GetValueOrDefault(id) is "Partener" or "Angajat" ? id : null;

    static Guid? Gestiune(Guid? repartitor, IReadOnlyDictionary<Guid, string> felRepartitor) =>
        repartitor is Guid id
        && felRepartitor.GetValueOrDefault(id) is "Gestiune" or "ContPropriu" or "UnitateInterna"
            ? id
            : null;

    static N.Analiza Analiza(RegistruContabil r, bool peDebit) => peDebit
        ? new N.Analiza(r.DebitCodFunctionalId, r.DebitCodEconomicId, r.DebitSursaFinantareId,
            r.DebitUnitateId, r.DebitProiectId, r.DebitCentruCostId)
        : new N.Analiza(r.CreditCodFunctionalId, r.CreditCodEconomicId, r.CreditSursaFinantareId,
            r.CreditUnitateId, r.CreditProiectId, r.CreditCentruCostId);

    static Dictionary<Guid, RolTertCont> ConturiRolTert(IObjectSpace os, IReadOnlyList<RegistruContabil> contabile) {
        var ids = contabile.SelectMany(r => new[] { r.ContDebitId, r.ContCreditId }).Distinct().ToList();
        return ids.Count == 0
            ? []
            : os.GetObjectsQuery<Cont>()
                .Where(c => ids.Contains(c.ID))
                .Select(c => new { c.ID, c.RolTert })
                .ToList()
                .ToDictionary(c => c.ID, c => c.RolTert);
    }

    // Felul repartitorului = discriminatorul `ClrType` citit prin PROIECȚIE, nu prin `is` (89b).
    static Dictionary<Guid, string> FeluriRepartitor(IObjectSpace os, IReadOnlyList<RegistruContabil> contabile) {
        var ids = contabile
            .SelectMany(r => new[] { r.DebitRepartitorId, r.CreditRepartitorId })
            .OfType<Guid>()
            .Distinct()
            .ToList();
        return ids.Count == 0
            ? []
            : os.GetObjectsQuery<Repartitor>()
                .Where(r => ids.Contains(r.ID))
                .Select(r => new { r.ID, r.ClrType })
                .ToList()
                .ToDictionary(r => r.ID, r => r.ClrType);
    }

    // Lanțul sigur rând de stoc → cont: Lot → Produs → TipMaterial.ContImplicit.
    static Dictionary<Guid, LotFizic> Loturi(IObjectSpace os, IReadOnlyList<RegistruStoc> stoc) {
        var idsLot = stoc.Select(r => r.LotId).Distinct().ToList();
        if (idsLot.Count == 0)
            return [];
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => idsLot.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId, l.Data })
            .ToList();
        var idsProdus = loturi.Select(l => l.ProdusId).Distinct().ToList();
        var tipPerProdus = os.GetObjectsQuery<Produs>()
            .Where(p => idsProdus.Contains(p.ID))
            .Select(p => new { p.ID, p.TipMaterialId })
            .ToList()
            .ToDictionary(p => p.ID, p => p.TipMaterialId);
        var idsTip = tipPerProdus.Values.OfType<Guid>().Distinct().ToList();
        var contPerTip = idsTip.Count == 0
            ? []
            : os.GetObjectsQuery<TipMaterial>()
                .Where(t => idsTip.Contains(t.ID))
                .Select(t => new { t.ID, t.ContImplicitId })
                .ToList()
                .ToDictionary(t => t.ID, t => t.ContImplicitId);
        var rezultat = new Dictionary<Guid, LotFizic>();
        foreach (var l in loturi) {
            var tip = tipPerProdus.GetValueOrDefault(l.ProdusId);
            var cont = tip is Guid t ? contPerTip.GetValueOrDefault(t) : null;
            if (cont is Guid c)
                rezultat[l.ID] = new LotFizic(l.ID, l.ProdusId, l.Data, c);
        }
        return rezultat;
    }
}
