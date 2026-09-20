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

    public static void Reseteaza() => avertismente.Clear();

    /// <param name="SursaConexului">documentul conex autogenerat → documentul sursă.</param>
    /// <param name="LiniaSursa">linia conexului → linia sursei care i-a născut lotul.</param>
    /// <param name="ConturiTva">4426/4427 ale tipurilor de TVA atinse.</param>
    public sealed record Context(
        IReadOnlyDictionary<Guid, Guid> SursaConexului,
        IReadOnlyDictionary<Guid, Guid> LiniaSursa,
        IReadOnlySet<Guid> ConturiTva) {

        public static readonly Context Gol =
            new(new Dictionary<Guid, Guid>(), new Dictionary<Guid, Guid>(), new HashSet<Guid>());
    }

    public static IReadOnlyList<N.Tranzactie> Toate(IReadOnlyList<N.Tranzactie> tranzactii, Context context) {
        var rezultat = TrD3AbsoarbeNirConex(tranzactii, context);
        rezultat = TrD4UnificaStocCuContabil(rezultat);
        rezultat = TrD2NominalizeazaPrinImperechere(rezultat);
        rezultat = FiscalCaAtribute(rezultat, context);
        return M6PartenerDoarPeTert(rezultat);
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

    // ── B-D8 pct. 3 — TR-D2a: stingerea e postarea care numește partida ──────
    public static IReadOnlyList<N.Tranzactie> TrD2NominalizeazaPrinImperechere(
            IReadOnlyList<N.Tranzactie> tranzactii) {
        ArgumentNullException.ThrowIfNull(tranzactii);
        if (!tranzactii.Any(t => t.Fel == N.FelTranzactie.Transfer))
            return [.. tranzactii];

        var postari = new List<N.Postare>?[tranzactii.Count];
        var consumat = new bool[tranzactii.Count];
        for (var i = 0; i < tranzactii.Count; i++)
            if (tranzactii[i].Fel == N.FelTranzactie.Operare)
                postari[i] = [.. tranzactii[i].Postari];

        for (var i = 0; i < tranzactii.Count; i++) {
            var transfer = tranzactii[i];
            if (transfer.Fel != N.FelTranzactie.Transfer)
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
            if (Nominalizeaza(postari[alStingatorului]!, proprie, stinsa, intrare.Coordonate.Partener, -iesire.Valoare))
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

    static bool Nominalizeaza(
            List<N.Postare> postari, N.Unitate proprie, N.Unitate stinsa, Guid? partener, decimal suma) {
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
                var eTaxa = context.ConturiTva.Contains(tinta.Coordonate.Cont);
                if (cod.Rol == N.RolTva.Taxa ? eTaxa : !eTaxa && tinta.Coordonate.Latura == laturaBazei)
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
        return new Context(
            conexe ?? new Dictionary<Guid, Guid>(),
            conexe is null || conexe.Count == 0 ? new Dictionary<Guid, Guid>() : LiniiSursa(os, conexe),
            ConturiTva(os, ids));
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

    static HashSet<Guid> ConturiTva(IObjectSpace os, IReadOnlyList<Guid> documente) {
        var idsTip = documente.Count == 0
            ? []
            : os.GetObjectsQuery<RegistruTva>()
                .Where(r => documente.Contains(r.DocumentId))
                .Select(r => r.TipTvaId)
                .Distinct()
                .ToList();
        if (idsTip.Count == 0)
            return [];
        return [.. os.GetObjectsQuery<TipTva>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ContTvaDeductibilId, t.ContTvaColectatId })
            .ToList()
            .SelectMany(t => new[] { t.ContTvaDeductibilId, t.ContTvaColectatId })
            .OfType<Guid>()];
    }

    static void Avertizeaza(string mesaj) {
        avertismente.Add(mesaj);
        Console.WriteLine($"     Normalizari: {mesaj}");
    }
}
