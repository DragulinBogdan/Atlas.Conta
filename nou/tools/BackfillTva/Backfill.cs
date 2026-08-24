using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;

namespace BackfillTva;

// Forma minimă a unui fapt fiscal, cât are nevoie reconcilierea. Există doar
// pentru DRY-RUN: acolo rândurile nu se scriu, deci reconcilierea n-are ce
// reciti din bază și ar fi o poartă vacuă (lecția 50b). În rularea de lucru
// reconcilierea citește ÎNAPOI din Postgres, independent de structurile fazei
// de scriere — contractul pasului 4 (47e), păstrat.
public readonly record struct RandFiscal(Guid DocumentId, SensTva Sens, Guid? PartenerId,
    RegimTva Regim, decimal Baza, decimal Tva, bool Storno);

public sealed class RezultatBackfill {
    public int Candidati, Sarite, Atinse, CuRanduri, Randuri, RanduriStorno, RanduriOriginaleStornate;
    public int StornateReconstituite, StornateFaraUrma, StornateAmbigue;
    public TimeSpan Durata;
    public List<RandFiscal> Derivate;
}

public static class Backfill {
    // Documentele se materializează în LOTURI, polimorf (tiparul 60b): sub TPT un
    // singur query pe bază întoarce tipul derivat corect. `GetObjectByKey` în
    // buclă e interzis — măsurat 11,3s pentru 335 de documente (decizia 60b), pe
    // 200.000 ar fi ore.
    const int Lot = 500;

    public static RezultatBackfill Executa(IObjectSpaceProvider provider, bool dryRun,
            Action<string> avert, Action<string, bool> check) {
        var rez = new RezultatBackfill { Derivate = dryRun ? new List<RandFiscal>() : null };
        var cronometru = System.Diagnostics.Stopwatch.StartNew();

        Console.WriteLine("\n--- Candidații ---");

        // Mulțimea candidaților se restrânge la TIPURILE cu `PoliticaTva`, fiindcă
        // ăsta e chiar criteriul JT-D2: un document al unui tip fără politică
        // produce ZERO rânduri prin construcție, iar a-l materializa doar ca să
        // afle asta ar fi însemnat 200.000 de documente în loc de 60.000.
        // `RegistruTvaService.Deriva` rămâne AUTORITATEA — filtrul de aici e doar
        // o pre-selecție care nu poate include nimic ce serviciul ar refuza
        // (subtipurile concrete, ex. `InchidereTva : NotaContabila`, intră în
        // mulțime prin tabela bazei lor, dar serviciul le cere politica PROPRIE și
        // le sare dacă n-o au).
        var peTip = new Dictionary<string, HashSet<Guid>>();
        List<(Guid ID, StareDocument Stare)> deFacut;
        using (var os = provider.CreateObjectSpace()) {
            var numeCuPolitica = os.GetObjectsQuery<PoliticaTva>()
                .Select(p => p.TipDocument.ClrType).Distinct().ToList();
            Console.WriteLine($"    Profilul bazei declară {numeCuPolitica.Count} tipuri de document ca "
                + $"eveniment de TVA: {string.Join(", ", numeCuPolitica.OrderBy(n => n))}");
            // Zero tipuri NU e un eșec de contract, ci profilul bugetar (JT-D2:
            // neplătitorul n-are jurnale, deși liniile lui chiar poartă TipTva).
            // Un `Check` picat aici ar fi transformat un no-op corect într-o
            // alarmă — exact felul de zgomot care face contractele să fie ignorate.
            if (numeCuPolitica.Count == 0) {
                // Profil neplătitor (bugetarul): nimic de backfill-uit — corect,
                // nu o eroare. Reconcilierea de după rămâne validă (0 documente).
                avert("Baza n-are niciun rând `PoliticaTva` — profil de neplătitor (bugetar). "
                    + "Nu există fapte fiscale de reconstituit.");
                rez.Durata = cronometru.Elapsed;
                return rez;
            }

            foreach (var (nume, ids) in Reconciliere.TipuriCuPolitica(os))
                peTip[nume] = ids;
            foreach (var nume in numeCuPolitica.Where(n => !peTip.ContainsKey(n)))
                check($"Ancora `TipDocument.ClrType` = „{nume}” are o clasă mapată în model", false);

            var stari = os.GetObjectsQuery<Document>()
                .Where(d => d.Stare == StareDocument.Operat || d.Stare == StareDocument.Stornat)
                .Select(d => new { d.ID, d.Stare }).ToList();
            // Idempotența (JT-D8): un document care ARE deja rânduri fiscale se
            // sare. Corolar asumat: documentele care produc ZERO rânduri (toate
            // liniile fără `TipTva`) rămân candidați la fiecare rulare — se
            // re-derivă, nu se scrie nimic. Alternativa (un marcaj „am trecut pe
            // aici”) ar fi fost stare nouă persistată pentru o economie de timp.
            var cuRanduri = os.GetObjectsQuery<RegistruTva>()
                .Select(r => r.DocumentId).Distinct().ToList().ToHashSet();

            var candidati = stari.Where(s => peTip.Values.Any(set => set.Contains(s.ID))).ToList();
            rez.Candidati = candidati.Count;
            rez.Sarite = candidati.Count(c => cuRanduri.Contains(c.ID));
            deFacut = candidati.Where(c => !cuRanduri.Contains(c.ID))
                .Select(c => (c.ID, c.Stare)).ToList();

            foreach (var (nume, set) in peTip.OrderBy(x => x.Key)) {
                var ale = candidati.Where(c => set.Contains(c.ID)).ToList();
                Console.WriteLine($"    {nume,-16} {ale.Count,8} candidați "
                    + $"({ale.Count(c => c.Stare == StareDocument.Stornat)} stornate, "
                    + $"{ale.Count(c => cuRanduri.Contains(c.ID))} deja cu rânduri fiscale)");
            }
            Console.WriteLine($"    TOTAL: {rez.Candidati} candidați Operat/Stornat, "
                + $"{rez.Sarite} săriți (au deja rânduri), {deFacut.Count} de procesat.");
        }

        Console.WriteLine($"\n--- Derivarea{(dryRun ? " (DRY-RUN)" : "")} ---");
        var cronometruLot = System.Diagnostics.Stopwatch.StartNew();
        var ultimulRaportat = 0;

        for (var start = 0; start < deFacut.Count; start += Lot) {
            var felie = deFacut.GetRange(start, Math.Min(Lot, deFacut.Count - start));
            var ids = felie.Select(f => f.ID).ToList();
            // Un ObjectSpace PROPRIU per lot, comis la finalul lui: nimic nu se
            // reține peste buclă (tiparul `Bucla`/`Reluare` din Import1C), deci
            // change tracker-ul nu crește cu numărul de documente.
            using var os = provider.CreateObjectSpace();

            // `Include(Detalii)` e ce ține N+1-ul afară: `Deriva` enumeră liniile
            // documentului, iar sub lazy loading fiecare document ar fi însemnat
            // încă un round-trip.
            var documente = os.GetObjectsQuery<Document>()
                .Where(d => ids.Contains(d.ID))
                .Include(d => d.Detalii)
                .ToList();

            var dateStorno = DateleStornarii(os, felie);

            foreach (var doc in documente) {
                rez.Atinse++;
                // Caveat asumat al oricărui backfill, scris ca să nu fie
                // descoperit mai târziu: JT-D3 spune că `Regim` și `Cota` se
                // SNAPSHOT-ează pe rând fiindcă `TipTva` e nomenclator editabil.
                // La operare snapshot-ul e al momentului operării; aici e al
                // momentului BACKFILL-ului. Pentru documentele reconstituite,
                // o cotă corectată în nomenclator între operare și backfill
                // intră în jurnal cu valoarea de azi — singura alternativă ar fi
                // fost o istorie a nomenclatorului, pe care modelul n-o ține.
                var randuri = RegistruTvaService.Deriva(os, doc);
                if (randuri.Count == 0)
                    continue;

                // STORNATUL (partea delicată): un document stornat are în registre
                // AMBELE seturi — rândurile originale ȘI inversele scrise la data
                // stornării. Ca să reproducă starea pe care ar fi avut-o dacă
                // felia ar fi existat de la început, backfill-ul trebuie să le
                // scrie pe amândouă; altfel jurnalul ar arăta o operațiune
                // taxabilă care a fost anulată, iar cusătura JT-D6 ar pica pe
                // document (partea contabilă e deja netă zero).
                //
                // Data stornării NU e pe document: se citește din registrele lui,
                // de pe rândurile marcate `Storno` (contabile SAU de stoc — un tip
                // fără contare poate avea doar mișcări de stoc). Când nu se poate
                // citi, documentul se SARE ÎNTREG și se raportează: a scrie doar
                // originalele ar fi însemnat să declarăm o operațiune care în
                // registrul contabil nu mai există, iar a le neta „pe loc” ar fi
                // fost o normalizare tăcută a unei stări pe care unealta n-o
                // înțelege.
                DateOnly dataStorno = default;
                if (doc.Stare == StareDocument.Stornat) {
                    var date = dateStorno.GetValueOrDefault(doc.ID) ?? new List<DateOnly>();
                    if (date.Count == 0) {
                        rez.StornateFaraUrma++;
                        avert($"Documentul STORNAT {doc.Numar} ({doc.ID}) n-are niciun rând de storno în "
                            + "registrele contabil/stoc — data stornării e necunoscută, deci faptele lui "
                            + "fiscale NU se pot reconstitui. Document sărit integral (nescris), nu normalizat.");
                        continue;
                    }
                    if (date.Count > 1) {
                        rez.StornateAmbigue++;
                        avert($"Documentul STORNAT {doc.Numar} ({doc.ID}) are rânduri de storno la "
                            + $"{date.Count} date distincte ({string.Join(", ", date.OrderBy(d => d))}) — "
                            + "motorul scrie storno-ul la o singură dată, deci starea e neinterpretabilă. "
                            + "Document sărit integral (nescris).");
                        continue;
                    }
                    dataStorno = date[0];
                    rez.StornateReconstituite++;
                }

                foreach (var t in randuri) {
                    Scrie(os, rez, dryRun, doc, t, doc.Data, storno: false);
                    if (doc.Stare != StareDocument.Stornat)
                        continue;
                    rez.RanduriOriginaleStornate++;
                    Scrie(os, rez, dryRun, doc, t, dataStorno, storno: true);
                }
                rez.CuRanduri++;
            }

            if (!dryRun)
                os.CommitChanges();

            if (rez.Atinse - ultimulRaportat >= 1000) {
                ultimulRaportat = rez.Atinse;
                Console.WriteLine($"    …{rez.Atinse}/{deFacut.Count} documente atinse, "
                    + $"{rez.Randuri} rânduri fiscale — {cronometruLot.Elapsed:hh\\:mm\\:ss}");
            }
        }

        rez.Durata = cronometru.Elapsed;
        Console.WriteLine($"    {rez.Atinse} documente atinse, {rez.CuRanduri} cu rânduri, "
            + $"{rez.Randuri} rânduri fiscale scrise ({rez.RanduriStorno} inverse de storno), "
            + $"{rez.Sarite} sărite ca deja făcute — {rez.Durata:hh\\:mm\\:ss}.");

        check($"Backfill: fiecare document atins a fost tranșat — {rez.CuRanduri} cu rânduri + "
            + $"{rez.Atinse - rez.CuRanduri - rez.StornateFaraUrma - rez.StornateAmbigue} fără linii fiscale + "
            + $"{rez.StornateFaraUrma + rez.StornateAmbigue} stornate nereconstituibile = {rez.Atinse}",
            rez.Atinse == deFacut.Count);
        check($"Stornatele reconstituite au perechea COMPLETĂ: {rez.RanduriStorno} rânduri inverse pentru "
            + $"{rez.RanduriOriginaleStornate} rânduri originale ale celor {rez.StornateReconstituite} documente "
            + "stornate — unu la unu, deci suma algebrică pe document e zero pe ambele coloane",
            rez.RanduriStorno == rez.RanduriOriginaleStornate
            && (rez.StornateReconstituite > 0) == (rez.RanduriStorno > 0));
        return rez;
    }

    // Data stornării, citită din registrele documentului. Se caută în AMBELE
    // registre pentru că un tip fără reguli de contare (BTR/ASM) are doar mișcări
    // de stoc — chiar dacă tipurile care produc fapte fiscale postează azi
    // întotdeauna, regula nu se sprijină pe asta.
    static Dictionary<Guid, List<DateOnly>> DateleStornarii(IObjectSpace os,
            List<(Guid ID, StareDocument Stare)> felie) {
        var idsStornate = felie.Where(f => f.Stare == StareDocument.Stornat).Select(f => f.ID).ToList();
        var rezultat = new Dictionary<Guid, List<DateOnly>>();
        if (idsStornate.Count == 0)
            return rezultat;

        void Aduna(IEnumerable<(Guid Doc, DateOnly Data)> sursa) {
            foreach (var (doc, data) in sursa) {
                if (!rezultat.TryGetValue(doc, out var lista))
                    rezultat[doc] = lista = new List<DateOnly>();
                if (!lista.Contains(data))
                    lista.Add(data);
            }
        }
        Aduna(os.GetObjectsQuery<RegistruContabil>()
            .Where(r => r.Storno && r.DocumentId != null && idsStornate.Contains(r.DocumentId.Value))
            .Select(r => new { Doc = r.DocumentId.Value, r.Data }).Distinct().ToList()
            .Select(x => (x.Doc, x.Data)));
        Aduna(os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Storno && r.DocumentId != null && idsStornate.Contains(r.DocumentId.Value))
            .Select(r => new { Doc = r.DocumentId.Value, r.Data }).Distinct().ToList()
            .Select(x => (x.Doc, x.Data)));
        return rezultat;
    }

    // Materializarea unui rând, pe aceeași formă ca în motor (`MotorOperare`:
    // originalele la data documentului, inversele la data stornării, cu bază și
    // TVA negate și restul identității fiscale copiată ca snapshot).
    static void Scrie(IObjectSpace os, RezultatBackfill rez, bool dryRun, Document doc,
            RegistruTvaService.RandTva t, DateOnly data, bool storno) {
        var semn = storno ? -1m : 1m;
        rez.Randuri++;
        if (storno)
            rez.RanduriStorno++;
        rez.Derivate?.Add(new RandFiscal(doc.ID, t.Sens, t.PartenerId, t.Regim,
            semn * t.Baza, semn * t.Tva, storno));
        if (dryRun)
            return;
        var rand = os.CreateObject<RegistruTva>();
        rand.Data = data;
        rand.Document = doc;
        rand.DetaliuId = t.DetaliuId;
        rand.Sens = t.Sens;
        rand.PartenerId = t.PartenerId;
        rand.TipTvaId = t.TipTvaId;
        rand.Regim = t.Regim;
        rand.Cota = t.Cota;
        rand.Baza = semn * t.Baza;
        rand.Tva = semn * t.Tva;
        rand.Storno = storno;
    }
}
