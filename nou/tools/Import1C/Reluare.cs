using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// LOTUL DE ROBUSTEȚE PRE-1C-d, pasul 3: reluarea unei rulări întrerupte.
//
// Două defecte cu aceeași rădăcină — artefactele unei rulări ANTERIOARE nu se pot
// da înapoi, deci reluarea le moștenește orbește:
//
//  * **D4** — un draft rămas de la o operare eșuată poartă liniile și ALOCAREA de
//    atunci. Re-operarea lui (comportamentul de până acum) rulează o alocare
//    învechită peste o stare de stoc care s-a schimbat între timp: ori eșuează la
//    fel, ori trece pe loturi care nu mai sunt cele corecte. Fix-ul: draftul se
//    ȘTERGE integral și documentul se reimportă pe calea normală, cu
//    replanificare (`Drafturi` de mai jos + `BuclaImport.Executa`).
//  * **D3** — puntea scrisă de o rulare anterioară blochează DEFINITIV
//    materializarea documentului de stoc (`Reluare1C.Blocheaza`, Vanzare1C.cs):
//    costul neacoperit e deja transcris contabil acolo, deci a-l posta acum ar
//    însemna dublă postare. Singura ieșire era reconstrucția întregii baze.
//    Fix-ul: `--deblocheaza <view>:<cheie>`, comandă EXPLICITĂ de operator —
//    riscul dublei postări nu se automatizează.
//
// Ambele lucrează pe artefacte, nu pe „starea corectă": ce se șterge se enumeră
// și se raportează, iar orice situație care ar cere o decizie (un lot al draftului
// folosit de altcineva, un document dependent operat) e REFUZ zgomotos, nu
// ștergere oarbă.

// Ce a atins o ștergere de draft — pentru raport și pentru curățarea indexului de
// loturi din memorie (`Catalog.UitaLot`).
sealed record RezultatStergere(int Documente, int Linii, int Loturi, int Registre, int Legaturi,
    IReadOnlyList<Guid> LoturiSterse);

static class Drafturi {
    // Grupul de șters: documentul + copiii lui autogenerați (conexul NIR, plata
    // secundară, descărcarea) — un draft nu ar trebui să aibă copii OPERAȚI
    // (copiii se nasc în timpul operării, deci un părinte Draft n-a ajuns acolo),
    // dar dacă are, e refuz.
    sealed record Grup(List<Guid> Documente, List<Guid> Linii, List<Guid> Loturi);

    static Grup Aduna(IObjectSpace os, Guid radacina, out string refuz) {
        refuz = null;
        var documente = new List<Guid> { radacina };
        for (var i = 0; i < documente.Count; i++) {
            var parinte = documente[i];
            foreach (var copil in os.GetObjectsQuery<Document>()
                         .Where(d => d.DocumentSursaId == parinte)
                         .Select(d => new { d.ID, d.Stare }).ToList()) {
                if (copil.Stare != StareDocument.Draft) {
                    refuz = $"documentul are un copil {copil.Stare} ({copil.ID}) — grupul conex "
                        + "nu se poate desface prin ștergere";
                    return null;
                }
                if (!documente.Contains(copil.ID))
                    documente.Add(copil.ID);
            }
        }

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => documente.Contains(d.DocumentId)).Select(d => d.ID).ToList();
        var liniiN = linii.Select(l => (Guid?)l).ToList();
        var loturi = new List<Guid>();
        if (linii.Count > 0)
            loturi = os.GetObjectsQuery<Lot>()
                .Where(l => liniiN.Contains(l.LinieIntrareId)).Select(l => l.ID).ToList();
        var loturiN = loturi.Select(l => (Guid?)l).ToList();

        // Gardienii: nimic din afara grupului nu are voie să depindă de ce ștergem.
        if (os.GetObjectsQuery<Imperechere>()
                .Any(i => documente.Contains(i.DocumentId) || documente.Contains(i.DocumentStingatorId))) {
            refuz = "documentul are imperecheri (stingeri) — se șterg întâi ele";
            return null;
        }
        if (loturi.Count > 0) {
            var strain = os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => loturiN.Contains(d.LotId) && !documente.Contains(d.DocumentId))
                .Select(d => new { d.ID, d.DocumentId }).FirstOrDefault();
            if (strain != null) {
                refuz = $"un lot născut de draft e referit de linia {strain.ID} a documentului "
                    + $"{strain.DocumentId}";
                return null;
            }
            var miscareStraina = os.GetObjectsQuery<RegistruStoc>()
                .Where(r => loturi.Contains(r.LotId))
                .Select(r => new { r.ID, r.DocumentId }).ToList()
                .FirstOrDefault(r => r.DocumentId == null || !documente.Contains(r.DocumentId.Value));
            if (miscareStraina != null) {
                refuz = $"un lot născut de draft are mișcări de stoc ale altui document "
                    + $"({miscareStraina.DocumentId?.ToString() ?? "deschidere"})";
                return null;
            }
        }
        return new Grup(documente, linii, loturi);
    }

    // Se poate șterge? Se răspunde ÎNAINTE de a construi draftul nou (apelantul
    // reconstruiește pe cheltuiala lui) — altfel un refuz ar lăsa în urmă un draft
    // pe jumătate construit și un index de loturi otrăvit.
    public static string Refuz(IObjectSpaceProvider provider, Guid documentId) {
        using var os = provider.CreateObjectSpace();
        if (os.GetObjectByKey<Document>(documentId) == null)
            return null;
        Aduna(os, documentId, out var refuz);
        return refuz;
    }

    // Ștergerea propriu-zisă, într-un singur commit: rândurile de registru (un
    // draft n-ar trebui să aibă — motorul nu lasă rânduri la refuz, 33d — dar dacă
    // are, sunt ale lui și pleacă odată cu el), liniile, documentele, loturile
    // născute de linii și TOATE legăturile care trimit la ele (documentul, lotul,
    // aliasul de lot). `null` = refuz (mesajul în `refuz`).
    // `inainteDeCommit` (D5): apelantul își strecoară propriile ștergeri în ACEEAȘI
    // tranzacție (registrul divergențelor — rândurile documentului care pleacă).
    // Se invocă doar pe căile care chiar comit; la refuz nu se atinge nimic.
    public static RezultatStergere Sterge(IObjectSpaceProvider provider, Guid documentId,
            out string refuz, Action<IObjectSpace> inainteDeCommit = null) {
        using var os = provider.CreateObjectSpace();
        var doc = os.GetObjectByKey<Document>(documentId);
        if (doc == null) {
            // Legătură orfană: documentul a dispărut, dar legătura lui trebuie să
            // plece, altfel rularea următoare tot ar crede că documentul există.
            refuz = null;
            var orfane = StergeLegaturi(os, [documentId]);
            inainteDeCommit?.Invoke(os);
            os.CommitChanges();
            return new RezultatStergere(0, 0, 0, 0, orfane, []);
        }
        var grup = Aduna(os, documentId, out refuz);
        if (grup == null)
            return null;

        var documenteN = grup.Documente.Select(d => (Guid?)d).ToList();
        var registre = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => documenteN.Contains(r.DocumentId)).ToList();
        var contabile = os.GetObjectsQuery<RegistruContabil>()
            .Where(r => documenteN.Contains(r.DocumentId)).ToList();
        os.Delete(registre);
        os.Delete(contabile);
        // Ordinea contează pentru FK-ul `DocumentDetaliu.LotId` (Restrict): liniile
        // pleacă înaintea loturilor. `Lot.LinieIntrareId` e coloană FĂRĂ FK
        // (decizia 26e), deci sensul celălalt nu constrânge nimic.
        os.Delete(os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => grup.Documente.Contains(d.DocumentId)).ToList());
        os.Delete(os.GetObjectsQuery<Document>()
            .Where(d => grup.Documente.Contains(d.ID)).ToList());
        os.Delete(os.GetObjectsQuery<Lot>().Where(l => grup.Loturi.Contains(l.ID)).ToList());
        var legaturi = StergeLegaturi(os, [.. grup.Documente, .. grup.Loturi]);
        inainteDeCommit?.Invoke(os);
        os.CommitChanges();

        return new RezultatStergere(grup.Documente.Count, grup.Linii.Count, grup.Loturi.Count,
            registre.Count + contabile.Count, legaturi, grup.Loturi);
    }

    // Loturile NĂSCUTE de un document stornat rămân în bază (registrele sunt
    // append-only: lotul are perechea intrare + storno, sold zero), dar CHEIA lor
    // de import trebuie eliberată — altfel reimportul documentului, care recreează
    // aceleași loturi cu aceeași cheie canonică, pică pe indexul unic al
    // legăturilor. Lotul vechi rămâne inert și nemaiindexat; ăsta e prețul
    // stornării, și e vizibil în raport.
    public static int EliberaCheileLoturilor(IObjectSpaceProvider provider, Guid documentId) {
        using var os = provider.CreateObjectSpace();
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => d.DocumentId == documentId).Select(d => d.ID).ToList();
        if (linii.Count == 0)
            return 0;
        var liniiN = linii.Select(l => (Guid?)l).ToList();
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => liniiN.Contains(l.LinieIntrareId)).Select(l => l.ID).ToList();
        if (loturi.Count == 0)
            return 0;
        var sterse = StergeLegaturi(os, loturi);
        os.CommitChanges();
        return sterse;
    }

    // Legăturile de import care trimit la entitățile șterse. Se merge pe ȚINTĂ, nu
    // pe cheie: aceeași ștergere trebuie să ia și legătura documentului
    // („1C:<view>"), și pe cele ale loturilor („1C:Lot", „1C:LotAlias"), fără ca
    // apelantul să știe ce sufixe folosește fiecare handler. Rămâne intactă
    // „1C:ProdusRealocat" — ținta ei e produsul, care nu se șterge.
    static int StergeLegaturi(IObjectSpace os, IReadOnlyList<Guid> tinte) {
        var moarte = os.GetObjectsQuery<MigrareLegatura>()
            .Where(m => m.Tabela.StartsWith("1C:") && tinte.Contains(m.TintaId)).ToList();
        os.Delete(moarte);
        return moarte.Count;
    }
}

// ======================= D3: deblocarea țintită =======================
//
// `--deblocheaza <view>:<cheieHex>[,<view>:<cheieHex>…]` (repetabil). Pentru
// fiecare cheie-sursă: TOATE artefactele rulărilor anterioare (documentul
// tipizat, descărcările per depozit, încasările, puntea NTC — orice legătură
// `cheie`, `cheie#…`, `cheie@…`) se dau înapoi:
//   * documentele OPERATE se STORNEAZĂ prin motor, la data lor (registrele rămân
//     append-only — decizia 14; perechea document + storno se anulează în solduri);
//   * drafturile se ȘTERG integral (mecanismul D4 de mai sus);
//   * legăturile implicate se șterg, deci rularea următoare replanifică documentul
//     de la zero — cu punte nouă, ajustată la stocul de atunci.
//
// Ordinea dependențelor (descărcarea e copil autogenerat al facturii, deci
// factura nu se poate storna înaintea ei) nu se codifică: se încearcă în pase
// repetate până când nu mai există progres — ce rămâne e refuz raportat.
static class Deblocare {
    public static bool Executa(IObjectSpaceProvider provider, int an,
            IReadOnlyList<(string View, string Cheie)> cereri,
            Action<string> avert, Action<string, bool> check) {
        Console.WriteLine($"\n=== Deblocare țintită (--deblocheaza): {cereri.Count} chei ===");
        // Stornarea cere perioada deschisă (gardianul tratează perioada LIPSĂ ca
        // închisă, decizia 14), iar deblocarea rulează înaintea buclei, care e cea
        // care creează perioadele. Idempotent, deci se poate cere aici.
        Perioade.Asigura(provider, an);

        var totul = true;
        foreach (var (view, cheie) in cereri)
            totul &= Una(provider, view, cheie, avert, check);
        return totul;
    }

    static bool Una(IObjectSpaceProvider provider, string view, string cheie,
            Action<string> avert, Action<string, bool> check) {
        var tabela = Legaturi.Tabela(view);
        List<(string Cheie, Guid Tinta)> artefacte;
        using (var os = provider.CreateObjectSpace())
            artefacte = os.GetObjectsQuery<MigrareLegatura>()
                .Where(m => m.Tabela == tabela && m.CheieLegacy.StartsWith(cheie))
                .Select(m => new { m.CheieLegacy, m.TintaId }).ToList()
                // Sufixele handlerelor: „#punte", „#dsc@<depozit>", „#card",
                // „#numerar", „#inc<linie>", „#btr", „@<depozit>". Filtrul pe prefix
                // se strânge aici, ca o cheie 1C care e prefixul alteia să nu tragă
                // după ea documentele vecinului.
                .Where(m => m.CheieLegacy.Length == cheie.Length
                    || m.CheieLegacy[cheie.Length] is '#' or '@')
                .Select(m => (m.CheieLegacy, m.TintaId))
                .OrderBy(m => m.CheieLegacy, StringComparer.Ordinal)
                .ToList();

        if (artefacte.Count == 0) {
            check($"--deblocheaza {view}:{cheie}: nu există artefacte de dat înapoi", false);
            avert($"--deblocheaza {view}:{cheie}: nicio legătură în bază — cheia e greșită sau "
                + "documentul n-a fost niciodată importat.");
            return false;
        }

        Console.WriteLine($"  {view}:{cheie} — {artefacte.Count} artefacte:");
        var ramase = new List<(string Cheie, Guid Tinta)>();
        using (var os = provider.CreateObjectSpace())
            foreach (var a in artefacte) {
                var doc = os.GetObjectByKey<Document>(a.Tinta);
                Console.WriteLine($"    {a.Cheie,-52} {(doc == null ? "(document lipsă)" : doc.Stare.ToString())}");
                ramase.Add(a);
            }

        // Pase repetate: ordinea dintre artefacte e dictată de gardienii motorului
        // (conexe operate, imperecheri), nu de noi.
        var atinse = new List<string>();
        var ultimulRefuz = "";
        for (var progres = true; progres && ramase.Count > 0;) {
            progres = false;
            foreach (var a in ramase.ToList()) {
                var rezultat = DaInapoi(provider, a.Tinta, out var motiv);
                if (rezultat == null) {
                    ultimulRefuz = $"{a.Cheie}: {motiv}";
                    continue;
                }
                atinse.Add($"{a.Cheie} → {rezultat}");
                ramase.Remove(a);
                progres = true;
            }
        }

        foreach (var linie in atinse)
            Console.WriteLine($"    dat înapoi: {linie}");

        if (ramase.Count > 0) {
            check($"--deblocheaza {view}:{cheie}: toate artefactele date înapoi", false);
            avert($"--deblocheaza {view}:{cheie}: {ramase.Count} artefacte NU s-au putut da înapoi "
                + $"({string.Join(", ", ramase.Select(r => r.Cheie))}) — ultimul refuz: {ultimulRefuz}. "
                + "Legăturile lor rămân, deci nu se dublează nimic; rezolvă dependența (imperecheri, "
                + "documente conexe) și repetă.");
            return false;
        }

        // Legăturile pleacă la FINAL, toate odată: cât timp există, o rulare
        // întreruptă la mijloc vede documentele stornate și le sare (conservator),
        // în loc să le importe a doua oară.
        int sterse;
        using (var os = provider.CreateObjectSpace()) {
            var chei = artefacte.Select(a => a.Cheie).ToList();
            var moarte = os.GetObjectsQuery<MigrareLegatura>()
                .Where(m => m.Tabela == tabela && chei.Contains(m.CheieLegacy)).ToList();
            sterse = moarte.Count;
            os.Delete(moarte);
            // Divergențele înregistrate de rularea deblocată pleacă odată cu
            // artefactele ei: documentul se replanifică de la zero, deci le
            // rescrie. Sursa e al 4-lea câmp al cheii sintetice (vezi
            // `RegistruDivergente`), de aici căutarea pe conținut — e o comandă de
            // operator, nu o cale caldă.
            var tabelaDivergente = Legaturi.Tabela(RegistruDivergente.View);
            var fragment = $"|{RegistruDivergente.Sursa(view, cheie)}|";
            var divergente = os.GetObjectsQuery<MigrareLegatura>()
                .Where(m => m.Tabela == tabelaDivergente && m.CheieLegacy.Contains(fragment)).ToList();
            sterse += divergente.Count;
            os.Delete(divergente);
            os.CommitChanges();
        }
        check($"--deblocheaza {view}:{cheie}: {artefacte.Count} artefacte date înapoi, {sterse} legături "
            + "șterse la final (restul au plecat odată cu drafturile) — documentul se replanifică "
            + "la rularea următoare", true);
        return true;
    }

    // Un artefact: stornare (operat), ștergere (draft), nimic (stornat / dispărut).
    // Întoarce descrierea a ce s-a făcut sau null + motivul refuzului.
    static string DaInapoi(IObjectSpaceProvider provider, Guid documentId, out string motiv) {
        motiv = null;
        using (var os = provider.CreateObjectSpace()) {
            var doc = os.GetObjectByKey<Document>(documentId);
            if (doc == null)
                return "legătură orfană (documentul lipsește)";
            if (doc.Stare == StareDocument.Stornat) {
                // Stornat de o sesiune anterioară: n-avem ce mai da înapoi, dar
                // cheile lui de lot tot trebuie eliberate — altfel reimportul ar
                // pica pe indexul unic exact ca la stornarea făcută de noi.
                var cheiVechi = Drafturi.EliberaCheileLoturilor(provider, documentId);
                return "deja stornat"
                    + (cheiVechi > 0 ? $", {cheiVechi} chei de lot eliberate" : "");
            }
            if (doc.Stare == StareDocument.Operat) {
                var data = doc.Data;
                try {
                    MotorOperare.Storneaza(os, doc, data);
                }
                catch (Exception ex) {
                    motiv = ex.Message;
                    return null;
                }
                var chei = Drafturi.EliberaCheileLoturilor(provider, documentId);
                return $"stornat la {data:yyyy-MM-dd}"
                    + (chei > 0 ? $", {chei} chei de lot eliberate" : "");
            }
        }
        // Draft: aceeași ștergere ca la reluare (D4). Loturile lui dispar cu el,
        // deci pin-urile rulării următoare cad în supapa FIFO (48a) — corect:
        // loturile alea nu mai există.
        var rezultat = Drafturi.Sterge(provider, documentId, out var refuz);
        if (rezultat == null) {
            motiv = refuz;
            return null;
        }
        return $"draft șters ({rezultat.Linii} linii, {rezultat.Loturi} loturi, "
            + $"{rezultat.Legaturi} legături)";
    }

    // „<view>:<cheie>" — cheia 1C e hex, deci fără „:" în ea; separatorul e primul.
    public static bool Parseaza(string valoare, List<(string View, string Cheie)> tinta) {
        // Contorul e de dinaintea apelului, nu de la zero: flag-ul e REPETABIL, deci
        // `tinta` poate fi deja plină de la un `--deblocheaza` anterior, iar un
        // „> 0" naiv ar accepta tăcut o valoare goală pe al doilea flag.
        var inainte = tinta.Count;
        foreach (var bucata in valoare.Split(',', StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries)) {
            var i = bucata.IndexOf(':');
            if (i <= 0 || i == bucata.Length - 1)
                return false;
            tinta.Add((bucata[..i], bucata[(i + 1)..]));
        }
        return tinta.Count > inainte;
    }
}
