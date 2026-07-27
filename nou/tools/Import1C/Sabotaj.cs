using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// AUTO-TESTUL contractului lunar (`--sabotaj`), rescris la pasul 4 al lotului
// pre-1C-d-final ca să repare defectul D6 (decizia 50e).
//
// CE ERA GREȘIT. Proba de până acum altera cu 1 leu un rând contabil al lunii,
// ales din afara unui „bucket al netării" scris ca listă de PREFIXE (3xx, 60x,
// 401). Bucket-ul era o copie îmbătrânită a plafonului real: plafonul
// contractului 1 se derivă acum din justificările MĂSURATE ale contractului 3 și
// din oglinzile de cost citite din politici (`Catalog.CosturiPentruContStoc` —
// 607, 711, 608…), iar toleranța per cont crește cu numărul de divergențe
// înregistrate pe el (EpsV × înregistrări poate depăși un leu). Un rând sabotat
// pe un cont plafonat era absorbit, contractul rămânea verde pe el, iar rularea
// tot ieșea cu eșecuri (din alte cauze) — deci auto-testul „trecea" fără să fi
// probat nimic. Exact fals-negativul pe care îl vânează D6.
//
// CE FACE ACUM. Două probe, pe două contracte diferite, cu ținta derivată din
// ACELEAȘI structuri pe care le folosește reconcilierea:
//   * proba CONTABILĂ (+1 leu pe un rând de registru al unui document al lunii):
//     rândul se alege cu debit ȘI credit în afara mulțimii conturilor care pot
//     primi plafon sau toleranță pe ORICE cale (`ConturiPlafonabile`) — și cu
//     debit ≠ credit, altfel diferența s-ar anula în ea însăși;
//   * proba de STOC (+0,001 buc pe un rând de registru de stoc al lunii): cheia
//     produs × gestiune se alege dintre cele care se închid EXACT azi și pe care
//     nicio categorie a contractului 3 nu le poate justifica (fără divergențe
//     măsurate, fără marcaj de netare/realocare, fără celule negative în sursă,
//     fără diferență raportată la deschidere). Cantitatea e discriminantul
//     contractului 3 (§8.3 amendat) — o alterare de cantitate nu poate cădea în
//     nicio poartă de VALOARE.
//
// Verdictul cere AMBELE probe detectate. O probă care scapă e raportată pe nume:
// asta e informația pe care fals-negativul o ascundea.
//
// CE NU SE SABOTEAZĂ, și de ce (limitare documentată, nu tăcere):
//   * contractul 2 (închiderea de TVA) — rândurile lui sunt generate de ITV din
//     SOLDURILE de TVA, iar soldurile sunt exact rânduri de `RegistruContabil`:
//     a-l sabota înseamnă a sabota aceleași rânduri pe care le atinge proba
//     contabilă, doar cu efect colateral asupra unei alte comparații. Un rând ITV
//     alterat DUPĂ generare ar pica trivial o comparație de egalitate — probă
//     fără conținut. Sensibilitatea reală a contractului 2 e demonstrată de
//     istoria feliei: a picat pe date reale de fiecare dată când generarea a fost
//     greșită (49a, 46c).
//   * contractul 4 (deriva de rotunjire) — nu e o egalitate, e un PRAG pe un
//     agregat calculat din contorul motorului (`Scara.MidpointBani`), nu din
//     rândurile scrise. Sabotarea unui rând de registru nu-l atinge deloc, iar
//     alterarea contorului ar însemna sabotarea instrumentului de măsură, nu a
//     măsurătorii.
static partial class ReconciliereLuna {

    // Ce a alterat proba, ca verdictul să poată întreba exact pe cheia lovită.
    // `Esec` nenul = proba nu s-a putut pune (nu există rând eligibil) — caz în
    // care auto-testul PICĂ zgomotos, nu se sare tăcut.
    internal sealed record ProbeSabotaj(
        Guid? RandContabil, string ContDebit, string ContCredit, decimal ValoareContabila,
        Guid? RandStoc, string ProdusHex, string DepozitHex,
        int ConturiPlafonabile, int CheiEligibile,
        string EsecContabil, string EsecStoc);

    internal sealed record VerdictSabotaj(bool ContabilDetectat, bool StocDetectat,
            IReadOnlyList<string> Mesaje) {
        public bool Trecut => ContabilDetectat && StocDetectat;
    }

    // Alterarea probei de stoc. Peste EpsQ (0,0005) cu un ordin de mărime, sub
    // orice cantitate reală — se vede în contract, nu deranjează nicio poartă.
    const decimal AlterareCantitate = 0.001m;

    // ==================== Punerea probelor ====================

    public static ProbeSabotaj PuneProbele(ContextLuna ctx, Stare stare, Action<string> avert) {
        var bucla = ctx.Bucla;
        using var os = bucla.CreeazaObjectSpace();

        // Aceleași intrări pe care le citește contractul: registrul divergențelor
        // cumulat la zi și pozițiile brute ale sursei la fine de lună.
        var registru = bucla.Divergente.PanaLa(ctx.An, ctx.Luna);
        var pozitii = bucla.Flax.StocLaFineDeLuna(ctx.An, ctx.Luna)
            .Concat(bucla.Flax.StocFaraIdentitateLaFineDeLuna(ctx.An, ctx.Luna))
            .ToList();

        var plafonabile = ConturiPlafonabile(os, bucla.Catalog, registru, pozitii);
        var contabil = AlegeRandContabil(os, ctx, plafonabile);
        var (stoc, cheiEligibile) = AlegeRandStoc(os, ctx, stare, registru, pozitii, avert);

        if (contabil != null) {
            var rand = os.GetObjectByKey<RegistruContabil>(contabil.Value.Id);
            rand.Valoare += 1m;
        }
        if (stoc != null) {
            var rand = os.GetObjectByKey<RegistruStoc>(stoc.Value.Id);
            rand.Cantitate += AlterareCantitate;
        }
        if (contabil != null || stoc != null)
            os.CommitChanges();

        var probe = new ProbeSabotaj(
            contabil?.Id, contabil?.Debit, contabil?.Credit, contabil?.Valoare ?? 0m,
            stoc?.Id, stoc?.ProdusHex, stoc?.DepozitHex,
            plafonabile.Count, cheiEligibile,
            contabil != null ? null
                : $"luna {ctx.Luna:00}/{ctx.An} n-are niciun rând contabil de document cu debit ≠ "
                    + $"credit și ambele conturi în afara celor {plafonabile.Count} conturi care pot "
                    + "primi plafon sau toleranță",
            stoc != null ? null
                : $"luna {ctx.Luna:00}/{ctx.An} n-are nicio cheie de stoc care să se închidă exact "
                    + "și să fie în afara tuturor categoriilor de justificare ale contractului 3");

        Console.WriteLine($"""

            *** SABOTAJ (--sabotaj) — două probe pe luna {ctx.Luna:00}/{ctx.An} ***
                proba CONTABILĂ: {(contabil == null ? "NEPUSĂ — " + probe.EsecContabil
                    : $"+1 leu pe rândul {contabil.Value.Id} ({contabil.Value.Debit} = "
                        + $"{contabil.Value.Credit}, {contabil.Value.Valoare:N2} lei); "
                        + $"{plafonabile.Count} conturi excluse ca plafonabile. "
                        + "Contractul (1) TREBUIE să pice pe ambele conturi.")}
                proba de STOC: {(stoc == null ? "NEPUSĂ — " + probe.EsecStoc
                    : $"+{AlterareCantitate:N3} buc pe rândul {stoc.Value.Id} (produs "
                        + $"{stoc.Value.ProdusHex} × gestiune {stoc.Value.DepozitHex}); "
                        + $"{cheiEligibile} chei eligibile. "
                        + "Contractul (3) TREBUIE să pice pe cheia asta.")}
            """);
        return probe;
    }

    // Conturile care pot primi PLAFON sau TOLERANȚĂ în contractul 1, derivate din
    // aceleași surse pe care le folosește contractul — niciun prefix, nicio listă
    // scrisă de mână. Un cont de aici nu poate fi ținta probei: diferența ar fi
    // (poate corect) declarată justificată, iar proba n-ar dovedi nimic.
    static IReadOnlySet<string> ConturiPlafonabile(IObjectSpace os, Catalog cat,
            IReadOnlyList<Divergenta> registru, IReadOnlyList<FlaxPozitieStoc> pozitii) {
        var set = new HashSet<string>(StringComparer.Ordinal);
        void Adauga(string simbol) {
            if (!string.IsNullOrEmpty(simbol))
                set.Add(simbol);
        }

        // (a) Conturile de stoc ALE MODELULUI: plafonul se atribuie pe drumul
        //     produs → Tip → `ContImplicit` (partea „bază" a defalcării per cont
        //     din contractul 3). Se iau TOATE Tipurile, nu doar cele cu
        //     Natura = Stoc: un produs poate purta orice Tip, iar mulțimea trebuie
        //     să fie supra-, nu sub-acoperitoare.
        foreach (var simbol in os.GetObjectsQuery<TipMaterial>()
                     .Select(t => t.ContImplicit.Simbol).ToList())
            Adauga(simbol);

        // (b) Oglinzile de cost din POLITICI (371 → 607, 345 → 711…) — exact mapa
        //     pe care contractul 1 o consultă când varsă plafonul unei chei de
        //     stoc pe conturi. Plus contul venitului din plusul de inventar, tot
        //     din politici (7588 la privat, 791 la bugetar).
        foreach (var simbol in cat.ConturiDinOglinzi)
            Adauga(simbol);
        Adauga(cat.ContPlusInventar);

        // (c) Conturile de stoc ALE SURSEI: partea „1C" a aceleiași defalcări
        //     (`BalantaNivel3.Cont` mapat pe planul OMFP). Un cont care apare doar
        //     acolo primește la fel de bine plafon.
        foreach (var p in pozitii)
            Adauga(cat.Mapeaza(p.Cont));

        // (d) TOLERANȚA per cont: `EpsV × numărul de divergențe înregistrate`.
        //     Pe un cont cu 200+ înregistrări toleranța trece de un leu, adică
        //     exact peste alterarea probei. Se exclud toate conturile care apar în
        //     registru cu valoare NEPOSTATĂ (numai ele intră în contorul ăla).
        foreach (var d in registru.Where(d => d.ValoareNepostata != 0m)) {
            Adauga(d.ContDebit);
            Adauga(d.ContCredit);
        }
        return set;
    }

    static (Guid Id, string Debit, string Credit, decimal Valoare)? AlegeRandContabil(
            IObjectSpace os, ContextLuna ctx, IReadOnlySet<string> plafonabile) =>
        os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId != null && r.Data >= ctx.Prima && r.Data <= ctx.Ultima)
            .Select(r => new { r.ID, Debit = r.ContDebit.Simbol, Credit = r.ContCredit.Simbol, r.Valoare })
            .ToList()
            // Debit ≠ credit: un rând cu aceleași conturi pe ambele laturi mișcă
            // soldul cu +1 și −1 pe același cont, deci sabotajul s-ar anula în el
            // însuși și proba ar fi invizibilă prin construcție.
            .Where(r => r.Debit != null && r.Credit != null && r.Debit != r.Credit
                && !plafonabile.Contains(r.Debit) && !plafonabile.Contains(r.Credit))
            .OrderBy(r => r.ID)
            .Select(r => ((Guid, string, string, decimal)?)(r.ID, r.Debit, r.Credit, r.Valoare))
            .FirstOrDefault();

    // Cheia probei de stoc: una care se închide EXACT azi (altfel proba n-ar putea
    // deosebi alterarea de diferența care era deja acolo) și pe care nicio
    // categorie de justificare a contractului 3 nu o poate prinde. Condițiile
    // oglindesc, una câte una, porțile din `Stoc` — dacă acolo apare o categorie
    // nouă, ea trebuie exclusă și aici, altfel proba redevine fals-negativă.
    static ((Guid Id, string ProdusHex, string DepozitHex)? Rand, int CheiEligibile) AlegeRandStoc(
            IObjectSpace os, ContextLuna ctx, Stare stare, IReadOnlyList<Divergenta> registru,
            IReadOnlyList<FlaxPozitieStoc> pozitii, Action<string> avert) {
        var produsPeLot = os.GetObjectsQuery<Lot>()
            .Select(l => new { l.ID, l.ProdusId })
            .ToList()
            .ToDictionary(l => l.ID, l => l.ProdusId);
        var produsHex = Reconciliere.InverseazaProduse(os, avert);
        var depozitHex = Reconciliere.Inverseaza(os, "Depozite", avert);

        // Baza, agregată exact ca în contract (aceleași registre comparabile,
        // aceeași scară de agregare).
        var db = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var r in os.GetObjectsQuery<RegistruStoc>()
                     .Where(r => r.Data <= ctx.Ultima && RegistreComparabile.Contains(r.TipStoc))
                     .GroupBy(r => new { r.LotId, r.RepartitorId })
                     .Select(g => new {
                         g.Key.LotId, g.Key.RepartitorId,
                         Q = g.Sum(x => Math.Round(x.Cantitate, ScaraAgregare)),
                         V = g.Sum(x => Math.Round(x.Valoare, ScaraAgregare)),
                     })
                     .ToList()) {
            var cheie = Cheie(r.LotId, r.RepartitorId);
            var acum = db.GetValueOrDefault(cheie);
            db[cheie] = (acum.Q + r.Q, acum.V + r.V);
        }

        var sursa = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var p in pozitii) {
            var cheie = (p.NomenclatorId, p.DepozitId);
            var acum = sursa.GetValueOrDefault(cheie);
            sursa[cheie] = (acum.Q + p.Cantitate, acum.V + p.Valoare);
        }

        // Mulțimile din care contractul 3 își scoate justificările.
        var masurat = registru
            .Where(d => d.ProdusHex != null)
            .Select(d => (P: d.ProdusHex, D: d.DepozitHex ?? ""))
            .ToHashSet();
        var deschidere = stare.JustificateDeschidere
            .Select(j => (P: j.ProdusHex, D: j.DepozitHex)).ToHashSet();
        var realocateHex = ctx.Bucla.Alocare.ProduseRealocate
            .Select(id => produsHex.GetValueOrDefault(id))
            .Where(h => h != null)
            .ToHashSet(StringComparer.Ordinal);
        var negativeSursa = pozitii
            .Where(p => p.Cantitate < -EpsQ || p.Valoare < -EpsV)
            .Select(p => (P: p.NomenclatorId, D: p.DepozitId))
            .ToHashSet();

        bool Eligibila((string P, string D) k) =>
            k.P is not (null or IdGol) && k.D is not (null or IdGol)
            && !masurat.Contains(k)
            && !deschidere.Contains(k)
            && !negativeSursa.Contains(k)
            && !stare.NegativeIstoric.ContainsKey(k)
            && !stare.ValoriFaraCantitateDeschidere.ContainsKey(k)
            && !stare.ProduseNetate.Contains(k.P)
            && !realocateHex.Contains(k.P);

        var eligibile = db
            .Where(x => {
                var (qS, vS) = sursa.GetValueOrDefault(x.Key);
                return Math.Abs(x.Value.Q - qS) < EpsQ && Math.Abs(x.Value.V - vS) < EpsV
                    && Eligibila(x.Key);
            })
            .Select(x => x.Key)
            .ToHashSet();
        if (eligibile.Count == 0)
            return (null, 0);

        // Rândul concret: unul al unui DOCUMENT al lunii (ca proba contabilă) pe o
        // cheie eligibilă. Rândurile de deschidere n-ar fi greșite — rularea nu le
        // mai rescrie după ce a intrat în bucla lunilor —, dar rândul de document
        // e proba onestă: exact ce scrie importul în luna probată.
        foreach (var r in os.GetObjectsQuery<RegistruStoc>()
                     .Where(r => r.DocumentId != null && r.Data >= ctx.Prima && r.Data <= ctx.Ultima
                         && RegistreComparabile.Contains(r.TipStoc))
                     .Select(r => new { r.ID, r.LotId, r.RepartitorId })
                     .ToList()
                     .OrderBy(r => r.ID)) {
            var cheie = Cheie(r.LotId, r.RepartitorId);
            if (eligibile.Contains(cheie))
                return ((r.ID, cheie.P, cheie.D), eligibile.Count);
        }
        return (null, eligibile.Count);

        (string P, string D) Cheie(Guid lotId, Guid repartitorId) => (
            produsPeLot.TryGetValue(lotId, out var produsId)
                ? produsHex.GetValueOrDefault(produsId) ?? $"(produs nelegat {produsId})"
                : $"(lot necunoscut {lotId})",
            depozitHex.GetValueOrDefault(repartitorId) ?? $"(gestiune nelegată {repartitorId})");
    }

    // ==================== Verdictul ====================

    // Detecția se citește din ce a PICAT contractul, nu din ce a raportat unealta
    // despre sine: conturile și cheile picate se adună în `Stare` chiar de
    // funcțiile care emit verdictul.
    public static VerdictSabotaj Verifica(ProbeSabotaj probe, Stare stare) {
        var mesaje = new List<string>();

        var contabilDetectat = false;
        if (probe.EsecContabil != null)
            mesaje.Add($"proba CONTABILĂ n-a putut fi pusă: {probe.EsecContabil}.");
        else {
            var debit = stare.ConturiPicate.Contains(probe.ContDebit);
            var credit = stare.ConturiPicate.Contains(probe.ContCredit);
            contabilDetectat = debit && credit;
            mesaje.Add(contabilDetectat
                ? $"proba CONTABILĂ DETECTATĂ: contractul (1) a picat pe {probe.ContDebit} și pe "
                    + $"{probe.ContCredit}."
                : "proba CONTABILĂ A SCĂPAT: contractul (1) n-a picat pe "
                    + (debit || credit
                        ? $"{(debit ? probe.ContCredit : probe.ContDebit)} (a picat doar pe "
                            + $"{(debit ? probe.ContDebit : probe.ContCredit)})"
                        : $"niciunul dintre conturile {probe.ContDebit} / {probe.ContCredit}")
                    + " — leul adăugat a fost absorbit de o justificare, adică de un plafon sau o "
                    + "toleranță pe care alegerea probei nu le-a văzut (fix D6: mulțimea "
                    + "`ConturiPlafonabile` s-a despărțit de contract).");
        }

        var stocDetectat = false;
        if (probe.EsecStoc != null)
            mesaje.Add($"proba de STOC n-a putut fi pusă: {probe.EsecStoc}.");
        else {
            stocDetectat = stare.CheiStocPicate.Contains((probe.ProdusHex, probe.DepozitHex));
            mesaje.Add(stocDetectat
                ? $"proba de STOC DETECTATĂ: contractul (3) a picat pe cheia {probe.ProdusHex} × "
                    + $"{probe.DepozitHex}."
                : $"proba de STOC A SCĂPAT: contractul (3) n-a picat pe cheia {probe.ProdusHex} × "
                    + $"{probe.DepozitHex} — cele {AlterareCantitate:N3} buc adăugate au încăput "
                    + "într-o categorie de justificare, deși cheia a fost aleasă în afara tuturor.");
        }
        return new VerdictSabotaj(contabilDetectat, stocDetectat, mesaje);
    }
}
