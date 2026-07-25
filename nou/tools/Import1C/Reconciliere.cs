using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 4 al feliei 1C-b: RECONCILIEREA DESCHIDERII — contractul design §8
// (decizia 45e) restrâns la deschidere, precedentul de formă fiind faza 6 din
// `tools/Migrare` (decizia 34f).
//
// PRINCIPIUL, și e singurul lucru care face reconcilierea să valoreze ceva:
// aici NU se refolosește NIMIC din structurile în memorie ale fazei de
// deschidere — nici celulele, nici descriptorii, nici rezultatele netării.
// Baza se recitește INTEGRAL din Postgres (rândurile cu `DocumentId == null`,
// decizia 25e) prin proiecții proprii, iar sursa se re-agregă din listele brute
// FlaxDb. O reconciliere care ar compara calculul cu el însuși ar trece și
// atunci când scrierea în bază a eșuat — ceea ce e exact ce trebuie prins.
//
// Singurul lucru primit din faza 3 e lista diferențelor RAPORTATE ale sursei
// (`Deschidere.DiferentaSursa`): grupele total-negative și pozițiile orfane. Ele
// sunt nepotriviri AȘTEPTATE — sursa are bani pe care Atlas nu-i poate
// reprezenta ca lot (34f: diferențele sursei se raportează, nu se ascund). Nu se
// scad din comparație și nu lărgesc toleranța: se raportează ca AVERT, cu
// eticheta lor. Orice ALTĂ nepotrivire e eșec.
static class Reconciliere {

    // Toleranța contractului (design §8): per poziție, la bani. Cantitățile se
    // compară cu aceeași toleranță — netarea mută cantități între loturi, dar
    // reconcilierea lucrează pe granularitatea produs × gestiune, unde mutarea
    // e invizibilă; o diferență de cantitate e o pierdere reală.
    const decimal Eps = 0.005m;

    public record Rezultat(
        int SimboluriContabile, int DiferenteContabile,
        decimal AncoraDb, decimal AncoraSursa,
        int CheiStoc, int Nejustificate, int JustificateGasite,
        decimal JustificatQ, decimal JustificatV);

    public static Rezultat Executa(
            IObjectSpaceProvider provider,
            IReadOnlyList<FlaxSold> solduri,
            IReadOnlySet<string> extrabilantiere1C,
            IReadOnlyList<FlaxPozitieStoc> stoc,
            IReadOnlyList<FlaxPozitieStoc> stocOrfan,
            IReadOnlyList<Deschidere.DiferentaSursa> justificate,
            Func<string, string> mapeaza,
            Action<string> avert, Action<string, bool> check) {
        using var os = provider.CreateObjectSpace();

        var (simboluri, difContabile, ancoraDb, ancoraSursa) =
            Contabil(os, solduri, extrabilantiere1C, mapeaza, check);
        var (chei, nejustificate, gasite, q, v) =
            Stoc(os, stoc, stocOrfan, justificate, avert, check);

        return new Rezultat(simboluri, difContabile, ancoraDb, ancoraSursa,
            chei, nejustificate, gasite, q, v);
    }

    // ============ 1 + 2. Sold per cont OMFP, și ancora separat ============
    //
    // Agregarea per SIMBOL e obligatorie pe AMBELE părți, nu o comoditate: mai
    // multe conturi 1C cad pe același simbol OMFP (446.1 / 446.7 / 4465 → 446
    // prin `overrideCont` + tăierea segmentelor), deci comparația per cont-sursă
    // ar raporta diferențe fantomă la fiecare colaps.
    static (int Simboluri, int Diferente, decimal AncoraDb, decimal AncoraSursa) Contabil(
            IObjectSpace os,
            IReadOnlyList<FlaxSold> solduri,
            IReadOnlySet<string> extrabilantiere1C,
            Func<string, string> mapeaza,
            Action<string, bool> check) {

        // ---- Partea BAZĂ: rândurile de deschidere, netate per simbol ----
        var randuri = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.DocumentId == null)
            .Select(r => new { D = r.ContDebit.Simbol, C = r.ContCredit.Simbol, r.Valoare })
            .ToList();
        var db = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var r in randuri) {
            db[r.D] = db.GetValueOrDefault(r.D) + r.Valoare;
            db[r.C] = db.GetValueOrDefault(r.C) - r.Valoare;
        }

        // ---- Partea SURSĂ: soldurile 1C, mapate și agregate ----
        // Se exclud extrabilanțierele (clasa 8 a planului 1C — nu intră în
        // bilanțul de deschidere, deciziile 9/22c) și contul-ancoră al sursei
        // (verificat separat, ca reziduu propriu al sursei).
        var sursa = new Dictionary<string, decimal>(StringComparer.Ordinal);
        var nemapate = new List<FlaxSold>();
        var extra = 0;
        foreach (var s in solduri) {
            if (extrabilantiere1C.Contains(s.Cont)) {
                extra++;
                continue;
            }
            var simbol = mapeaza(s.Cont);
            if (simbol == null) {
                nemapate.Add(s);
                continue;
            }
            sursa[simbol] = sursa.GetValueOrDefault(simbol) + s.SoldIni;
        }

        // Un cont nemapat e invizibil în ambele părți (nu s-a scris, nu se poate
        // agrega) — deci comparația de mai jos ar trece peste el tăcut. Gardianul
        // propriu al reconcilierii: banii sursei trebuie să fie TOȚI în joc.
        check($"Reconciliere contabilă: toate cele {solduri.Count - extra} solduri 1C bilanțiere se "
            + $"mapează pe planul OMFP ({nemapate.Count} nemapate, "
            + $"{nemapate.Sum(s => Math.Abs(s.SoldIni)):N2} în valoare absolută)",
            nemapate.Count == 0);
        foreach (var s in nemapate.OrderByDescending(s => Math.Abs(s.SoldIni)))
            check($"  cont 1C {s.Cont} (sold {s.SoldIni:N2}) nemapat — banii lui nu sunt "
                + "în deschiderea Atlas", false);

        // Ancora se scoate din comparația per simbol: în bază ea e contrapartida
        // TUTUROR rândurilor (soldul ei = −Σ restul), în sursă e un cont oarecare
        // cu reziduul de rotunjire pe el. Nu sunt aceeași mărime — de aceea au
        // verificare proprie (check 2), pe alt contract.
        var ancoraDb = db.GetValueOrDefault(Deschidere.Ancora);
        var ancoraSursa = sursa.GetValueOrDefault(Deschidere.Ancora);
        db.Remove(Deschidere.Ancora);
        sursa.Remove(Deschidere.Ancora);

        var simboluri = db.Keys.Union(sursa.Keys).OrderBy(s => s, StringComparer.Ordinal).ToList();
        var diferente = simboluri
            .Select(s => (Simbol: s, Db: db.GetValueOrDefault(s), Sursa: sursa.GetValueOrDefault(s)))
            .Where(x => Math.Abs(x.Db - x.Sursa) >= Eps)
            .ToList();
        foreach (var (simbol, valDb, valSursa) in diferente.OrderByDescending(x => Math.Abs(x.Db - x.Sursa)))
            check($"  cont {simbol}: bază {valDb:N2} = sursă 1C {valSursa:N2} "
                + $"(Δ {valDb - valSursa:N2})", false);
        check($"Sold per cont OMFP: {simboluri.Count} simboluri comparate, "
            + $"{diferente.Count} diferențe (din {randuri.Count} rânduri de deschidere)",
            diferente.Count == 0);

        // Contractul „891 → 0" se citește „ancora reproduce EXACT sursa": zero e
        // cazul unei surse curate, iar aici sursa își parchează pe propriul cont
        // de deschidere un reziduu de rotunjire (−0,01 la 01.01.2025). Ancora
        // Atlas trebuie să-l poarte identic — altfel s-a pierdut un leu pe drum.
        check($"Ancora {Deschidere.Ancora}: sold în bază {ancoraDb:N2} = soldul 1C al contului "
            + $"de deschidere al sursei {ancoraSursa:N2}", Math.Abs(ancoraDb - ancoraSursa) < Eps);

        return (simboluri.Count, diferente.Count, ancoraDb, ancoraSursa);
    }

    // ==================== 3. Stoc per produs × gestiune ====================
    //
    // Granularitatea NU e lotul, ci produsul × gestiunea (contractul design §8,
    // punctul 3): loturile 1C sunt subconto cu poziții negative, iar netarea le
    // rearanjează deliberat ÎN INTERIORUL grupei produs × depozit — exact cheia
    // de aici. Per lot n-ar fi comparabile; per produs × gestiune netarea e
    // invizibilă, deci orice diferență e reală.
    static (int Chei, int Nejustificate, int JustificateGasite, decimal Q, decimal V) Stoc(
            IObjectSpace os,
            IReadOnlyList<FlaxPozitieStoc> stoc,
            IReadOnlyList<FlaxPozitieStoc> stocOrfan,
            IReadOnlyList<Deschidere.DiferentaSursa> justificate,
            Action<string> avert, Action<string, bool> check) {

        // ---- Partea BAZĂ: rândurile de stoc de deschidere ----
        // Proiecție cu Select pe FK-uri + denumiri (join în SQL), NU navigații
        // lazy într-o buclă peste mii de rânduri (decizia 25b).
        var randuri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.DocumentId == null)
            .Select(r => new {
                ProdusId = r.Lot.ProdusId,
                ProdusDesc = r.Lot.Produs.Denumire,
                r.RepartitorId,
                RepartitorDesc = r.Repartitor.Denumire,
                r.Cantitate,
                r.Valoare,
            })
            .ToList();

        // Traducerea înapoi la identitatea 1C: legăturile inversate (TintaId →
        // hex). Fără ea, comparația ar trebui să treacă prin coduri naturale —
        // care la Nomenclator NU sunt unice (250.018 coduri distincte pe 312.659
        // rânduri, vezi Nomenclatoare.cs).
        var produsHex = Inverseaza(os, "Nomenclator", avert);
        var depozitHex = Inverseaza(os, "Depozite", avert);

        // Denumirile 1C, adunate din ambele părți: raportul trebuie să ajungă la
        // om, iar un hex de 32 de caractere nu se poate căuta în 1C. Numele nule
        // nu ocupă slotul — altfel partea care le are prima l-ar bloca.
        var nume = new Dictionary<string, string>(StringComparer.Ordinal);
        void Nume(string hex, string desc) {
            if (desc != null)
                nume.TryAdd(hex ?? "", desc);
        }

        var db = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var r in randuri) {
            var p = produsHex.GetValueOrDefault(r.ProdusId) ?? $"(produs nelegat {r.ProdusId})";
            var d = depozitHex.GetValueOrDefault(r.RepartitorId) ?? $"(gestiune nelegată {r.RepartitorId})";
            Nume(p, r.ProdusDesc);
            Nume(d, r.RepartitorDesc);
            var acum = db.GetValueOrDefault((p, d));
            db[(p, d)] = (acum.Q + r.Cantitate, acum.V + r.Valoare);
        }

        // ---- Partea SURSĂ: BalantaNivel3 agregată, toate conturile de stoc ----
        // Pozițiile ORFANE (fără produs sau fără depozit) intră și ele: sunt bani
        // reali în soldul contabil al sursei, iar excluderea lor aici ar face
        // reconcilierea să treacă fără să le vadă. Intră ca poziții de sursă cu
        // cheie parțial nulă (nu pot avea corespondent în bază — un lot fără
        // produs nu există în Atlas, decizia 13) și ies pe partea justificată.
        var sursa = new Dictionary<(string P, string D), (decimal Q, decimal V)>();
        foreach (var p in stoc.Concat(stocOrfan)) {
            var cheie = (p.NomenclatorId, p.DepozitId);
            Nume(p.NomenclatorId, p.NomenclatorDesc);
            Nume(p.DepozitId, p.DepozitDesc);
            var acum = sursa.GetValueOrDefault(cheie);
            sursa[cheie] = (acum.Q + p.Cantitate, acum.V + p.Valoare);
        }

        // ---- Diferențele, pe reuniunea cheilor ----
        // Justificate = grupele total-negative primite din faza 3 + pozițiile
        // orfane, transformate aici în aceeași formă (identitatea lor 1C E cheia
        // parțial nulă de mai sus).
        var toateJustificate = justificate.Concat(stocOrfan
            .GroupBy(p => (p.NomenclatorId, p.DepozitId))
            .Select(g => new Deschidere.DiferentaSursa(
                g.Key.NomenclatorId, g.Key.DepozitId,
                g.First().NomenclatorDesc, g.First().DepozitDesc,
                g.Sum(p => p.Cantitate), g.Sum(p => p.Valoare),
                "poziție orfană (fără produs sau fără depozit)")))
            .ToList();
        var justificateHex = toateJustificate
            .GroupBy(j => (j.ProdusHex, j.DepozitHex))
            .ToDictionary(g => g.Key,
                g => (Q: g.Sum(x => x.Cantitate), V: g.Sum(x => x.Valoare),
                      Motiv: string.Join(" + ", g.Select(x => x.Motiv).Distinct())));
        foreach (var j in toateJustificate) {
            Nume(j.ProdusHex, j.ProdusDesc);
            Nume(j.DepozitHex, j.DepozitDesc);
        }

        var chei = db.Keys.Union(sursa.Keys)
            .OrderBy(k => k.P, StringComparer.Ordinal).ThenBy(k => k.D, StringComparer.Ordinal)
            .ToList();
        var nejustificate = 0;
        var gasite = 0;
        var sumaQ = 0m;
        var sumaV = 0m;
        foreach (var k in chei) {
            var (qDb, vDb) = db.GetValueOrDefault(k);
            var (qSursa, vSursa) = sursa.GetValueOrDefault(k);
            if (Math.Abs(qDb - qSursa) < Eps && Math.Abs(vDb - vSursa) < Eps)
                continue;
            var eticheta = $"„{Descrie(nume, k.P)}” × gestiune „{Descrie(nume, k.D)}”";
            if (justificateHex.TryGetValue(k, out var j)) {
                gasite++;
                sumaQ += Math.Abs(qDb - qSursa);
                sumaV += Math.Abs(vDb - vSursa);
                avert($"Stoc {eticheta}: bază {qDb:N3} buc / {vDb:N2} lei vs sursă "
                    + $"{qSursa:N3} buc / {vSursa:N2} lei — diferență a sursei ({j.Motiv}; "
                    + $"raportată la deschidere ca {j.Q:N3} buc / {j.V:N2} lei).");
                continue;
            }
            nejustificate++;
            check($"  stoc {eticheta}: bază {qDb:N3} buc / {vDb:N2} lei "
                + $"= sursă {qSursa:N3} buc / {vSursa:N2} lei", false);
        }
        check($"Stoc per produs × gestiune: {chei.Count} chei comparate, {nejustificate} nepotriviri "
            + $"nejustificate ({gasite} justificate — diferențe ale sursei)", nejustificate == 0);
        // Restul listei justificate = grupe al căror total cade SUB toleranță
        // (skip-ul din faza 3 lucrează la gram, reconcilierea la ban): nu apar ca
        // diferență, deci nu sunt eșec — dar nici nu se pierd din raport.
        if (gasite != justificateHex.Count)
            avert($"Stoc: {justificateHex.Count - gasite} din {justificateHex.Count} diferențe "
                + "justificate ale sursei nu apar ca nepotrivire (Δ sub toleranța de "
                + $"{Eps} — nesemnificative la nivel de produs × gestiune).");
        Console.WriteLine($"Diferențe justificate (diferențe ale sursei, raportate): {gasite} chei "
            + $"din {justificateHex.Count}, Σ absolută {sumaQ:N3} buc / {sumaV:N2} lei.");

        return (chei.Count, nejustificate, gasite, sumaQ, sumaV);
    }

    // Legăturile 1C, inversate. Duplicatele de țintă n-ar trebui să existe
    // (`ImportLaCerere.Materializeaza` recuperează doar entități NELEGATE), dar
    // dacă apar, traducerea devine ambiguă exact acolo unde reconcilierea are
    // nevoie de ea — deci se raportează, nu se rezolvă tăcut.
    static Dictionary<Guid, string> Inverseaza(IObjectSpace os, string view, Action<string> avert) {
        var directe = Legaturi.Incarca(os, view);
        var invers = new Dictionary<Guid, string>();
        foreach (var (hex, tinta) in directe) {
            if (invers.TryGetValue(tinta, out var deja)) {
                avert($"Legături 1C:{view} ambigue — ținta {tinta} e legată și de {deja}, și de "
                    + $"{hex}; reconcilierea folosește {deja}.");
                continue;
            }
            invers[tinta] = hex;
        }
        return invers;
    }

    static string Descrie(IReadOnlyDictionary<string, string> nume, string hex) =>
        nume.TryGetValue(hex ?? "", out var d) && d != null ? $"{d} [{hex}]" : hex ?? "(fără id)";
}
