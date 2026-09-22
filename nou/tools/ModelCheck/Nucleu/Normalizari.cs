#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Diferențele DECLARATE dintre registrele de azi și cubul declarantului (B-D8):
/// lista e ÎNCHISĂ, se aplică DOAR pe oracol și în ordinea fixă din <see cref="Toate"/>.
/// Orice reziduu rămâne defect sau propunere de normalizare nouă, niciodată tăcut.
/// </summary>
static class Normalizari {
    // `Postare` și `Tranzactie` au egalitate STRUCTURALĂ (090b): urmărirea se face
    // pe INDICI, nu pe seturi de instanțe — două postări identice sunt distincte.
    static readonly List<string> avertismente = [];

    public static IReadOnlyList<string> Avertismente => avertismente.ToArray();

    // S-D13 (amendament B-D8 pct. 11): grupurile (linie, cont, latură) nominalizate de o
    // împerechere ULTERIOARă operării. Acolo `Operare` ține piciorul de bani întreg și doar
    // `Transfer` mută partida, deci contrapartida NU se sparge; la nominalizarea din
    // `Operare` (împerecherea autogenerată a sursei) se sparge mai departe.
    static readonly HashSet<(Guid? Linie, Guid Cont, N.Latura Latura)> faraSpargere = [];

    public static void Reseteaza() {
        avertismente.Clear();
        faraSpargere.Clear();
    }

    /// <param name="SursaConexului">documentul conex autogenerat → documentul sursă.</param>
    /// <param name="LiniaSursa">linia conexului → linia sursei care i-a născut lotul.</param>
    /// <param name="ConturiTva">4426/4427 ale tipurilor de TVA atinse.</param>
    /// <param name="Capitalizate">tipurile de TVA atinse al căror regim capitalizează taxa în cost.</param>
    public sealed record Context(
        IReadOnlyDictionary<Guid, Guid> SursaConexului,
        IReadOnlyDictionary<Guid, Guid> LiniaSursa,
        IReadOnlySet<Guid> ConturiTva,
        IReadOnlySet<Guid> Capitalizate) {

        public static readonly Context Gol = new(
            new Dictionary<Guid, Guid>(), new Dictionary<Guid, Guid>(), new HashSet<Guid>(), new HashSet<Guid>());
    }

    public static IReadOnlyList<N.Tranzactie> Toate(IReadOnlyList<N.Tranzactie> tranzactii, Context context) {
        // Pct. 9 stă PRIMUL: TR-D4 rescrie gestiunea piciorului contabil din rândul
        // de stoc, iar perechea D/C a rândului n-ar mai putea fi citită.
        var rezultat = RepartitorPePiciorulPropriu(tranzactii);
        rezultat = TrD3AbsoarbeNirConex(rezultat, context);
        rezultat = TrD4UnificaStocCuContabil(rezultat);
        rezultat = TrD41CostulIesiriiEAlTertului(rezultat);
        rezultat = TrD2NominalizeazaPrinImperechere(rezultat);
        rezultat = TrD2DesparteContrapartida(rezultat);
        rezultat = FiscalCapitalizatulSeDesface(rezultat, context);
        rezultat = FiscalCaAtribute(rezultat, context);
        return M6PartenerDoarPeTert(rezultat);
    }

    // ── B-D8 pct. 9 — repartitorul stă pe piciorul PROPRIU, nu pe cel de terț ──
    //
    // Convenția 00 §5 dă fiecărei laturi repartitorul ei (debit ← predator, credit
    // ← primitor), deci pe un rând cu EXACT un terț dimensiunea internă cade pe
    // piciorul de TERȚ și cea de partener pe piciorul propriu. În cub gestiunea e a
    // locului unde stă valoarea: se mută pe piciorul propriu, iar cel de terț rămâne
    // fără ea. Rândurile fără partener (BCS, viramentul) nu se ating.
    public static IReadOnlyList<N.Tranzactie> RepartitorPePiciorulPropriu(
            IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.Select(Repartitorul)];
    }

    static N.Tranzactie Repartitorul(N.Tranzactie tranzactie) {
        var postari = tranzactie.Postari;
        var rezultat = postari.ToList();
        var folosit = new bool[postari.Count];
        var mutari = 0;
        for (var d = 0; d < postari.Count; d++) {
            var debit = postari[d];
            if (folosit[d] || !EContabila(debit) || debit.Coordonate.Latura != N.Latura.Debit)
                continue;
            for (var c = 0; c < postari.Count; c++) {
                var credit = postari[c];
                if (folosit[c] || !EContabila(credit) || credit.Coordonate.Latura != N.Latura.Credit
                    || credit.Cauza.Linie != debit.Cauza.Linie || credit.Valoare != debit.Valoare)
                    continue;
                // Rândul poartă exact o dimensiune de terț când UN picior are gestiune
                // (repartitorul intern) și celălalt nu.
                if (debit.Coordonate.Gestiune is { } aDebitului && credit.Coordonate.Gestiune is null)
                    (rezultat[d], rezultat[c]) = (Fara(debit), Cu(credit, aDebitului));
                else if (debit.Coordonate.Gestiune is null && credit.Coordonate.Gestiune is { } aCreditului)
                    (rezultat[d], rezultat[c]) = (Cu(debit, aCreditului), Fara(credit));
                else
                    break;
                folosit[d] = folosit[c] = true;
                mutari++;
                break;
            }
        }
        return mutari == 0 ? tranzactie : tranzactie with { Postari = rezultat };
    }

    static bool EContabila(N.Postare postare) =>
        postare.Coordonate.Cont != CubDinRegistre.ContFiscal && postare.Coordonate.Partener is not null;

    static N.Postare Fara(N.Postare postare) =>
        postare with { Coordonate = postare.Coordonate with { Gestiune = null } };

    static N.Postare Cu(N.Postare postare, Guid gestiune) =>
        postare with { Coordonate = postare.Coordonate with { Gestiune = gestiune } };

    // ── B-D8 pct. 11 — contrapartida unei linii nominalizate PARȚIAL se sparge la fel ──
    //
    // Sub TR-D2 o linie stinsă doar în parte din sursă e DOUĂ mișcări, iar o mișcare
    // are două capete; registrele de azi sparg doar piciorul cu partidă. Piciorul
    // fără partidă al aceleiași linii se sparge în aceleași sume, în ordinea lor.
    public static IReadOnlyList<N.Tranzactie> TrD2DesparteContrapartida(
            IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.Select(Desparte)];
    }

    static N.Tranzactie Desparte(N.Tranzactie tranzactie) {
        var postari = tranzactie.Postari;
        var grupuri = postari
            .Where(p => p.Coordonate.Unitate?.Fel == N.FelUnitate.Partida)
            .GroupBy(p => (p.Cauza.Linie, p.Coordonate.Cont, p.Coordonate.Latura))
            // Spargerea e a NOMINALIZĂRII: două postări pe ACEEAȘI partidă (netul și
            // taxa aceleiași linii de factură) n-au de ce să spargă contrapartida.
            .Where(g => g.Select(p => p.Coordonate.Unitate!.Id).Distinct().Count() > 1)
            .Where(g => !faraSpargere.Contains(g.Key))                                // S-D13
            .ToList();
        if (grupuri.Count == 0)
            return tranzactie;
        var bucati = new Dictionary<int, List<decimal>>();
        foreach (var grup in grupuri) {
            var sume = grup.Select(p => p.Valoare).ToList();
            var total = sume.Sum();
            var tinta = -1;
            for (var i = 0; i < postari.Count && tinta < 0; i++) {
                var postare = postari[i];
                if (!bucati.ContainsKey(i)
                    && postare.Coordonate.Unitate is null
                    && postare.Coordonate.Cont != CubDinRegistre.ContFiscal
                    && postare.Cantitate == 0m
                    && postare.Cauza.Linie == grup.Key.Linie
                    && postare.Coordonate.Latura != grup.Key.Latura
                    && postare.Valoare == total)
                    tinta = i;
            }
            if (tinta < 0) {
                Avertizeaza($"TR-D2 pct. 11: linia {grup.Key.Linie} e nominalizată în {sume.Count} bucăți, "
                    + $"dar n-are contrapartidă fără partidă de {total} care să se spargă la fel.");
                continue;
            }
            bucati[tinta] = sume;
        }
        if (bucati.Count == 0)
            return tranzactie;
        var rezultat = new List<N.Postare>(postari.Count + bucati.Sum(b => b.Value.Count) - bucati.Count);
        for (var i = 0; i < postari.Count; i++) {
            if (!bucati.TryGetValue(i, out var sume)) {
                rezultat.Add(postari[i]);
                continue;
            }
            foreach (var suma in sume)
                rezultat.Add(postari[i] with { Valoare = suma });
        }
        return tranzactie with { Postari = rezultat };
    }

    // ── B-D8 pct. 1 — TR-D3: NIR-ul conex n-are tranzacție proprie ───────────
    //
    // Pe lângă rescrierea cauzei, absorbția re-datează postările ca tranzacția
    // sursei și re-cheiază partida deschisă de conex pe sursă: sub TR-D3 conexul
    // nu există, deci nici data și nici partida lui nu pot supraviețui.
    public static IReadOnlyList<N.Tranzactie> TrD3AbsoarbeNirConex(
            IReadOnlyList<N.Tranzactie> tranzactii, Context context) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        ArgumentNullException.ThrowIfNull(context);
        if (context.SursaConexului.Count == 0)
            return [.. tranzactii];

        var adaugate = new Dictionary<Guid, List<N.Postare>>();
        var absorbite = new bool[tranzactii.Count];
        for (var i = 0; i < tranzactii.Count; i++) {
            var tranzactie = tranzactii[i];
            if (tranzactie.Fel != N.FelTranzactie.Operare
                || tranzactie.Document is not Guid conex
                || !context.SursaConexului.TryGetValue(conex, out var sursa))
                continue;
            var alSursei = tranzactii.FirstOrDefault(
                t => t.Fel == N.FelTranzactie.Operare && t.Document == sursa);
            if (alSursei is null) {
                // Excepție DECLARATĂ a gate-ului, nu normalizare: sursa fără rânduri
                // contabile proprii n-are tranzacție în care conexul să fie absorbit. // S-D16
                Avertizeaza($"TR-D3: conexul {conex} n-are tranzacția sursei {sursa} în set — neabsorbit.");
                continue;
            }
            absorbite[i] = true;
            if (!adaugate.TryGetValue(sursa, out var ale))
                adaugate[sursa] = ale = [];
            foreach (var postare in tranzactie.Postari)
                ale.Add(Absoarbe(postare, conex, sursa, alSursei.Data, context));
        }

        var rezultat = new List<N.Tranzactie>();
        for (var i = 0; i < tranzactii.Count; i++) {
            if (absorbite[i])
                continue;
            var tranzactie = tranzactii[i];
            rezultat.Add(
                tranzactie.Fel == N.FelTranzactie.Operare
                && tranzactie.Document is Guid sursa
                && adaugate.TryGetValue(sursa, out var ale)
                    ? tranzactie with { Postari = [.. tranzactie.Postari, .. ale] }
                    : tranzactie);
        }
        return rezultat;
    }

    static N.Postare Absoarbe(N.Postare postare, Guid conex, Guid sursa, DateOnly data, Context context) {
        Guid? linie = null;
        if (postare.Cauza.Linie is Guid original) {
            linie = context.LiniaSursa.GetValueOrDefault(original, original);
            if (linie == original)
                Avertizeaza($"TR-D3: linia {original} a conexului {conex} n-are linie-sursă pe {sursa} — "
                    + "cauza rămâne a ei.");
        }
        var unitate = postare.Coordonate.Unitate;
        if (unitate is { Fel: N.FelUnitate.Partida, Partener: Guid partener }
            && unitate.Id == N.Unitate.DeschidePartida(unitate.Cont, partener, conex, data).Id)
            unitate = N.Unitate.DeschidePartida(unitate.Cont, partener, sursa, data);
        return postare with {
            Coordonate = postare.Coordonate with { Data = data, Unitate = unitate },
            Cauza = new N.Cauza(sursa, linie),
        };
    }

    // ── B-D8 pct. 2 — TR-D4: postarea de stoc și piciorul contabil al liniei sunt UNA ──
    public static IReadOnlyList<N.Tranzactie> TrD4UnificaStocCuContabil(IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.Select(Unifica)];
    }

    static N.Tranzactie Unifica(N.Tranzactie tranzactie) {
        // T-D2: transferul de stoc n-are picior contabil — unificarea e a lui `Operare`.
        if (tranzactie.Fel != N.FelTranzactie.Operare)
            return tranzactie;
        var postari = tranzactie.Postari;
        var unificata = new N.Postare?[postari.Count];
        var absorbita = new bool[postari.Count];
        var consumat = new bool[postari.Count];
        var unificari = 0;
        for (var s = 0; s < postari.Count; s++) {
            var stoc = postari[s];
            if (stoc.Coordonate.Unitate?.Fel != N.FelUnitate.Lot)
                continue;
            var ales = -1;
            for (var c = 0; c < postari.Count; c++) {
                var picior = postari[c];
                if (consumat[c]
                    || picior.Coordonate.Unitate is not null
                    || picior.Coordonate.Cont == CubDinRegistre.ContFiscal
                    || picior.Cauza.Linie != stoc.Cauza.Linie
                    || picior.Coordonate.Latura != stoc.Coordonate.Latura
                    || picior.Valoare != stoc.Valoare)
                    continue;
                if (picior.Coordonate.Cont == stoc.Coordonate.Cont) {
                    ales = c;
                    break;
                }
                if (ales < 0)
                    ales = c;
            }
            if (ales < 0) {
                Avertizeaza($"TR-D4: rândul de stoc al liniei {stoc.Cauza.Linie} "
                    + $"({stoc.Coordonate.Latura}, {stoc.Valoare}) n-are picior contabil pereche — "
                    + "rămâne postare separată.");
                continue;
            }
            var picioarea = postari[ales];
            // Unitatea se mută pe contul postării unificate: C5 cere unitatea pe contul ei.
            unificata[ales] = picioarea with {
                Coordonate = picioarea.Coordonate with {
                    Gestiune = stoc.Coordonate.Gestiune,
                    Produs = stoc.Coordonate.Produs,
                    Unitate = stoc.Coordonate.Unitate! with { Cont = picioarea.Coordonate.Cont },
                },
                Cantitate = stoc.Cantitate,
            };
            consumat[ales] = true;
            absorbita[s] = true;
            unificari++;
        }
        if (unificari == 0)
            return tranzactie;
        var rezultat = new List<N.Postare>(postari.Count);
        for (var i = 0; i < postari.Count; i++) {
            if (absorbita[i])
                continue;
            rezultat.Add(unificata[i] ?? postari[i]);
        }
        return tranzactie with { Postari = rezultat };
    }

    // ── T-D4.1 — capătul fără stoc al unei linii pur de IEȘIRE nu poartă gestiune ──
    //
    // Convenția registrului ține dimensiunea soldului de stoc pe AMBELE picioare ale
    // notei (32c); în cub capătul fără lot al unei linii care doar scoate marfă din
    // patrimoniu e în afara evidenței — gestiune virtuală, citită ca lipsă (N-D4).
    // Gardurile țin regula pe tipurile care CHIAR ies: o linie cu vreun rând de stoc
    // pozitiv (consumul BCS, recepția FCT), cu unitate, cu partener sau pe altă
    // gestiune decât ieșirea ei rămâne neatinsă.
    public static IReadOnlyList<N.Tranzactie> TrD41CostulIesiriiEAlTertului(
            IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.Select(CostulIesirii)];
    }

    static N.Tranzactie CostulIesirii(N.Tranzactie tranzactie) {
        if (tranzactie.Fel != N.FelTranzactie.Operare)
            return tranzactie;
        var postari = tranzactie.Postari;
        // Postarea fără linie (cauza e a documentului) intră pe cheia zero, ca oricare alta.
        static Guid Cheia(N.Postare postare) => postare.Cauza.Linie ?? Guid.Empty;
        var iesiri = new Dictionary<Guid, Guid?>();
        var respinse = new HashSet<Guid>();
        foreach (var postare in postari) {
            if (postare.Coordonate.Unitate?.Fel != N.FelUnitate.Lot)
                continue;
            var coordonate = postare.Coordonate;
            if (coordonate.Latura != N.Latura.Credit || coordonate.Gestiune is null
                    || (iesiri.TryGetValue(Cheia(postare), out var deja)
                        && deja != coordonate.Gestiune))
                respinse.Add(Cheia(postare));
            else
                iesiri[Cheia(postare)] = coordonate.Gestiune;
        }
        if (iesiri.Count == 0)
            return tranzactie;
        var rezultat = postari.ToList();
        var schimbari = 0;
        for (var i = 0; i < postari.Count; i++) {
            var coordonate = postari[i].Coordonate;
            if (coordonate.Unitate is not null || coordonate.Partener is not null
                || coordonate.Cont == CubDinRegistre.ContFiscal
                || respinse.Contains(Cheia(postari[i]))
                || !iesiri.TryGetValue(Cheia(postari[i]), out var gestiune)
                || coordonate.Gestiune != gestiune)
                continue;
            rezultat[i] = postari[i] with { Coordonate = coordonate with { Gestiune = null } };
            schimbari++;
        }
        return schimbari == 0 ? tranzactie : tranzactie with { Postari = rezultat };
    }

    // ── B-D8 pct. 3 — TR-D2a: stingerea e postarea care numește partida ──────
    //
    // Amendat la TR-D7a pasul 5 (S-D13): `Imperecheri` e ALGEBRIC (F27-D8),
    // deci rândul INVERS al unei desfaceri se pliază la fel, în sens opus: `DeImperechere`
    // îl construiește cu semn, iar `Nominalizeaza` îl duce înapoi pe partida proprie a
    // stingătorului. Fără el oracolul ar ține partida stinsă pentru totdeauna, iar cubul
    // (care scrie transferul invers) ar apărea ca diferență.
    public static IReadOnlyList<N.Tranzactie> TrD2NominalizeazaPrinImperechere(
            IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        if (!tranzactii.Any(EDeImperechere))
            return [.. tranzactii];

        var postari = new List<N.Postare>?[tranzactii.Count];
        var consumat = new bool[tranzactii.Count];
        for (var i = 0; i < tranzactii.Count; i++)
            if (tranzactii[i].Fel == N.FelTranzactie.Operare)
                postari[i] = [.. tranzactii[i].Postari];

        for (var i = 0; i < tranzactii.Count; i++) {
            var transfer = tranzactii[i];
            if (!EDeImperechere(transfer))
                continue;
            var iesire = transfer.Postari.FirstOrDefault(p => p.Valoare < 0m);
            var intrare = transfer.Postari.FirstOrDefault(p => p.Valoare > 0m);
            var alStingatorului = -1;
            for (var s = 0; s < tranzactii.Count && alStingatorului < 0; s++)
                if (tranzactii[s].Fel == N.FelTranzactie.Operare && tranzactii[s].Document == transfer.Document)
                    alStingatorului = s;
            if (iesire?.Coordonate.Unitate is not { } proprie
                || intrare?.Coordonate.Unitate is not { } stinsa
                || alStingatorului < 0) {
                Avertizeaza($"TR-D2a: împerecherea lui {transfer.Document} nu se poate aplica "
                    + "(lipsesc partidele sau tranzacția stingătorului) — rămâne transfer.");
                continue;
            }
            if (Nominalizeaza(postari[alStingatorului]!, proprie, stinsa, intrare.Coordonate.Partener,
                    -iesire.Valoare, transfer.Postari[0].Cauza.Linie is not null))
                consumat[i] = true;
        }

        var rezultat = new List<N.Tranzactie>();
        for (var i = 0; i < tranzactii.Count; i++) {
            if (consumat[i])
                continue;
            rezultat.Add(postari[i] is { } ale ? tranzactii[i] with { Postari = ale } : tranzactii[i]);
        }
        return rezultat;
    }

    // T-D2: transferul cu unități de fel `Lot` e MUTAREA de stoc a documentului, nu o
    // stingere de partidă — nu se pliază în `Operare`.
    public static bool EDeImperechere(N.Tranzactie tranzactie) {
        ArgumentNullException.ThrowIfNull(tranzactie);
        return tranzactie.Fel == N.FelTranzactie.Transfer
            && !tranzactie.Postari.Any(p => p.Coordonate.Unitate?.Fel == N.FelUnitate.Lot);
    }

    static bool Nominalizeaza(
            List<N.Postare> postari, N.Unitate proprie, N.Unitate stinsa, Guid? partener, decimal suma,
            bool ulterioara) {
        var ramas = suma;
        for (var i = 0; i < postari.Count && ramas > 0m; i++) {
            var postare = postari[i];
            if (postare.Coordonate.Unitate?.Id != proprie.Id || postare.Valoare <= 0m)
                continue;
            var luat = Math.Min(ramas, postare.Valoare);
            var rest = postare.Valoare - luat;
            postari[i] = postare with {
                Coordonate = postare.Coordonate with {
                    Unitate = stinsa,
                    Partener = partener ?? postare.Coordonate.Partener,
                },
                Valoare = luat,
            };
            if (ulterioara)
                faraSpargere.Add(
                    (postare.Cauza.Linie, postare.Coordonate.Cont, postare.Coordonate.Latura));
            if (rest > 0m)
                postari.Insert(++i, postare with { Valoare = rest });
            ramas -= luat;
        }
        if (ramas > 0m) {
            Avertizeaza($"TR-D2a: {ramas} din stingere n-a găsit postare de terț pe partida proprie {proprie.Id}.");
            return false;
        }
        return true;
    }

    // ── B-D8 pct. 5, amendat la pasul 5 — capitalizatul se desface în bază + taxă ──
    //
    // Pe regimul Capitalizat `Valoare` e BRUTĂ, iar `RegistruTvaService.Cifre` o
    // desface înapoi: jurnalul are DOUĂ cifre acolo unde registrul contabil are
    // una. Jurnalul fiind proiecția pe `CodTva` (090a), postarea de cost se sparge
    // în cele două cifre pe ACELAȘI cont, cu Σ și cantitatea neschimbate.
    public static IReadOnlyList<N.Tranzactie> FiscalCapitalizatulSeDesface(
            IReadOnlyList<N.Tranzactie> tranzactii, Context context) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        ArgumentNullException.ThrowIfNull(context);
        return context.Capitalizate.Count == 0
            ? [.. tranzactii]
            : [.. tranzactii.Select(t => Capitalizatul(t, context))];
    }

    static N.Tranzactie Capitalizatul(N.Tranzactie tranzactie, Context context) {
        var initiale = tranzactie.Postari;
        var fiscale = initiale
            .Select((postare, indice) => (Postare: postare, Indice: indice))
            .Where(x => x.Postare.Coordonate.Cont == CubDinRegistre.ContFiscal
                && context.Capitalizate.Contains(x.Postare.Coordonate.CodTva!.TipTva))
            .GroupBy(x => (x.Postare.Cauza.Linie, x.Postare.Coordonate.CodTva!.TipTva))
            .ToList();
        if (fiscale.Count == 0)
            return tranzactie;

        var eliminata = new bool[initiale.Count];
        var sparta = new Dictionary<int, List<N.Postare>>();
        foreach (var pereche in fiscale) {
            var baza = pereche.FirstOrDefault(x => x.Postare.Coordonate.CodTva!.Rol == N.RolTva.Baza);
            var taxa = pereche.FirstOrDefault(x => x.Postare.Coordonate.CodTva!.Rol == N.RolTva.Taxa);
            // Taxa zero (cotă 0 capitalizată) nu sparge nimic: rândul de taxă cade
            // pe regula generică, iar baza e chiar valoarea postării.
            if (baza.Postare is null || taxa.Postare is null || taxa.Postare.Valoare == 0m)
                continue;
            var brut = baza.Postare.Valoare + taxa.Postare.Valoare;
            var laturaBazei = baza.Postare.Coordonate.CodTva!.Sens == N.SensTva.Achizitie
                ? N.Latura.Debit
                : N.Latura.Credit;
            var tinte = new List<int>();
            for (var t = 0; t < initiale.Count; t++) {
                var tinta = initiale[t];
                if (!sparta.ContainsKey(t)
                    && tinta.Coordonate.Cont != CubDinRegistre.ContFiscal
                    && tinta.Cauza.Linie == pereche.Key.Linie
                    && tinta.Valoare == brut)
                    tinte.Add(t);
            }
            if (tinte.Count != 2) {
                Avertizeaza($"Fiscal capitalizat: linia {pereche.Key.Linie} are {tinte.Count} postări de "
                    + $"{brut} în loc de două — brutul rămâne o singură postare.");
                continue;
            }
            foreach (var t in tinte) {
                var postare = initiale[t];
                sparta[t] = postare.Coordonate.Latura == laturaBazei
                    ? [
                        Fiscala(postare, baza.Postare),
                        Fiscala(postare, taxa.Postare) with { Cantitate = 0m },
                    ]
                    : [
                        postare with { Valoare = baza.Postare.Valoare },
                        postare with { Valoare = taxa.Postare.Valoare, Cantitate = 0m },
                    ];
            }
            eliminata[baza.Indice] = true;
            eliminata[taxa.Indice] = true;
        }
        if (sparta.Count == 0)
            return tranzactie;
        var rezultat = new List<N.Postare>(initiale.Count);
        for (var i = 0; i < initiale.Count; i++) {
            if (eliminata[i])
                continue;
            if (sparta.TryGetValue(i, out var bucati))
                rezultat.AddRange(bucati);
            else
                rezultat.Add(initiale[i]);
        }
        return tranzactie with { Postari = rezultat };
    }

    static N.Postare Fiscala(N.Postare postare, N.Postare fiscala) => postare with {
        Coordonate = postare.Coordonate with {
            CodTva = fiscala.Coordonate.CodTva,
            PerioadaDeclarare = fiscala.Coordonate.PerioadaDeclarare,
            Partener = fiscala.Coordonate.Partener,
        },
        Valoare = fiscala.Valoare,
    };

    // ── B-D8 pct. 5 — faptul fiscal devine atribut al postării liniei ────────
    public static IReadOnlyList<N.Tranzactie> FiscalCaAtribute(
            IReadOnlyList<N.Tranzactie> tranzactii, Context context) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        ArgumentNullException.ThrowIfNull(context);
        return [.. tranzactii.Select(t => Fiscal(t, context))];
    }

    static N.Tranzactie Fiscal(N.Tranzactie tranzactie, Context context) {
        var initiale = tranzactie.Postari;
        if (!initiale.Any(p => p.Coordonate.Cont == CubDinRegistre.ContFiscal))
            return tranzactie;
        var postari = initiale.ToList();
        var eliminata = new bool[postari.Count];
        for (var f = 0; f < initiale.Count; f++) {
            var fiscala = initiale[f];
            if (fiscala.Coordonate.Cont != CubDinRegistre.ContFiscal)
                continue;
            var cod = fiscala.Coordonate.CodTva!;
            if (cod.Rol == N.RolTva.Taxa && fiscala.Valoare == 0m) {
                eliminata[f] = true;
                continue;
            }
            var laturaBazei = cod.Sens == N.SensTva.Achizitie ? N.Latura.Debit : N.Latura.Credit;
            var tinte = new List<int>();
            for (var t = 0; t < postari.Count; t++) {
                var tinta = postari[t];
                if (eliminata[t]
                    || tinta.Coordonate.Cont == CubDinRegistre.ContFiscal
                    || tinta.Cauza.Linie != fiscala.Cauza.Linie)
                    continue;
                // Taxarea inversă are AMBELE picioare pe conturi de TVA (4426 = 4427):
                // rândul fiscal e al celui de pe latura bazei, celălalt n-are fapt (B-D6).
                var eTaxa = context.ConturiTva.Contains(tinta.Coordonate.Cont);
                if ((cod.Rol == N.RolTva.Taxa ? eTaxa : !eTaxa) && tinta.Coordonate.Latura == laturaBazei)
                    tinte.Add(t);
            }
            if (tinte.Count != 1) {
                Avertizeaza($"Fiscal: rândul {cod.Rol} al liniei {fiscala.Cauza.Linie} are {tinte.Count} postări "
                    + "candidate în loc de una — postarea fiscală rămâne în cub.");
                continue;
            }
            var indice = tinte[0];
            postari[indice] = postari[indice] with {
                Coordonate = postari[indice].Coordonate with {
                    CodTva = cod,
                    PerioadaDeclarare = fiscala.Coordonate.PerioadaDeclarare,
                    Partener = fiscala.Coordonate.Partener,
                },
            };
            eliminata[f] = true;
        }
        var rezultat = new List<N.Postare>(postari.Count);
        for (var i = 0; i < postari.Count; i++)
            if (!eliminata[i])
                rezultat.Add(postari[i]);
        return tranzactie with { Postari = rezultat };
    }

    // ── B-D8 pct. 4 — M6: partenerul se scrie din unitate, nu din latură ─────
    public static IReadOnlyList<N.Tranzactie> M6PartenerDoarPeTert(IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        return [.. tranzactii.Select(t => t with {
            Postari = [.. t.Postari.Select(p =>
                p.Coordonate.Unitate?.Fel == N.FelUnitate.Partida || p.Coordonate.CodTva is not null
                    ? p
                    : p with { Coordonate = p.Coordonate with { Partener = null } })],
        })];
    }

    // ── contextul, citit pe SETURI ───────────────────────────────────────────
    public static Context Citeste(
            IObjectSpace os,
            IReadOnlyCollection<Guid> documente,
            IReadOnlyDictionary<Guid, Guid>? conexe = null) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(documente);
        var ids = documente.Distinct().ToList();
        var tipuri = TipuriTva(os, ids);
        return new Context(
            conexe ?? new Dictionary<Guid, Guid>(),
            conexe is null || conexe.Count == 0 ? new Dictionary<Guid, Guid>() : LiniiSursa(os, conexe),
            tipuri.Conturi,
            tipuri.Capitalizate);
    }

    // Cheia liniei la TR-D3: `Lot.LinieIntrareId` (lotul e născut de linia sursei);
    // fallback pe perechea 1:1 a lotului, când linia purtătoare nu e a sursei.
    static Dictionary<Guid, Guid> LiniiSursa(IObjectSpace os, IReadOnlyDictionary<Guid, Guid> conexe) {
        var idsDocument = conexe.Keys.Concat(conexe.Values).Distinct().ToList();
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => idsDocument.Contains(d.DocumentId))
            .Select(d => new { d.ID, d.DocumentId, d.LotId })
            .ToList();
        var idsLot = linii.Select(l => l.LotId).OfType<Guid>().Distinct().ToList();
        var linieIntrare = idsLot.Count == 0
            ? []
            : os.GetObjectsQuery<Lot>()
                .Where(l => idsLot.Contains(l.ID))
                .Select(l => new { l.ID, l.LinieIntrareId })
                .ToList()
                .ToDictionary(l => l.ID, l => l.LinieIntrareId);

        var rezultat = new Dictionary<Guid, Guid>();
        foreach (var (conex, sursa) in conexe) {
            var aleSursei = linii.Where(l => l.DocumentId == sursa).ToList();
            foreach (var linie in linii.Where(l => l.DocumentId == conex)) {
                if (linie.LotId is not Guid lot)
                    continue;
                if (linieIntrare.GetValueOrDefault(lot) is Guid nascuta && aleSursei.Any(l => l.ID == nascuta)) {
                    rezultat[linie.ID] = nascuta;
                    continue;
                }
                var pereche = aleSursei.Where(l => l.LotId == lot).ToList();
                if (pereche.Count == 1)
                    rezultat[linie.ID] = pereche[0].ID;
            }
        }
        return rezultat;
    }

    static (HashSet<Guid> Conturi, HashSet<Guid> Capitalizate) TipuriTva(
            IObjectSpace os, IReadOnlyList<Guid> documente) {
        var idsTip = documente.Count == 0
            ? []
            : os.GetObjectsQuery<RegistruTva>()
                .Where(r => documente.Contains(r.DocumentId))
                .Select(r => r.TipTvaId)
                .Distinct()
                .ToList();
        if (idsTip.Count == 0)
            return ([], []);
        var tipuri = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Regim, t.ContTvaDeductibilId, t.ContTvaColectatId })
            .ToList();
        return (
            [.. tipuri.SelectMany(t => new[] { t.ContTvaDeductibilId, t.ContTvaColectatId }).OfType<Guid>()],
            [.. tipuri.Where(t => t.Regim == RegimTva.Capitalizat).Select(t => t.ID)]);
    }

    static void Avertizeaza(string mesaj) {
        avertismente.Add(mesaj);
        Console.WriteLine($"     Normalizari: {mesaj}");
    }
}
