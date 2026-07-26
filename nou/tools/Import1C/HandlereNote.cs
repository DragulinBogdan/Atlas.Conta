using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 5 al feliei 1C-c, partea a II-a: NOTELE — compensarea și familia de
// documente pur contabile (operațiunea manuală, salariile, casarea de mijloace
// fixe, închiderea de lună, importul, reevaluarea).
//
// Toate devin `NotaContabila` cu postare explicită pe linie (46b) și toate se
// construiesc la fel: **transcrierea EXACTĂ a rândurilor de registru 1C**. Nu e
// lene — e singura formă onestă: sunt documente al căror conținut ESTE nota, iar
// orice re-derivare (din secțiuni, din politici Atlas) ar inventa o a doua
// versiune a unui adevăr pe care sursa îl are deja scris.
//
// Două excepții, amândouă deliberate:
//  * rândurile de TVA ale închiderii de lună (4427 = 4426 și 4427 = 4423/4424)
//    NU se transcriu — le generează `InchidereTvaService` la pasul 6, iar
//    reconcilierea le compară (design §6: forcing function-ul TVA-ului
//    structural din P1). Se numără separat, ca să se vadă ce a rămas ITV-ului;
//  * rândurile cu valoare ZERO se sar: nota Atlas cere valoare nenulă pe linie
//    (46b — negativul e permis, zeroul n-ar posta nimic).

static class NoteComune {
    // Linia unei note, deja mapată pe planul OMFP.
    public sealed record Linie(string Debit, string Credit, decimal Valoare,
        Guid? RepartitorDebitId, Guid? RepartitorCreditId, string Descriere);

    public static int RanduriZero { get; private set; }
    public static int RanduriTvaSarite { get; private set; }

    // Rândurile de TVA pe care le va genera închiderea Atlas (pasul 6). Se
    // recunosc după forma lor, nu după poziție: colectata se închide contra
    // deductibilei, iar excedentul iese în TVA de plată / de recuperat.
    static readonly string[] ContrapartideTva = ["4426", "4423", "4424"];

    public static bool EsteRandDeInchidereTva(string debit, string credit) =>
        debit == "4427" && ContrapartideTva.Contains(credit);

    // Transcrierea unui document: rândurile de registru → linii de notă.
    // `repartitori` dă, pentru un număr de linie, contrapartidele pe cele două
    // laturi (doar Operațiunea le are — restul familiei postează pe conturi).
    public static List<Linie> Transcrie(Catalog cat, string view, string docId,
            IEnumerable<FlaxRandNota> randuri, bool sareTva,
            Func<int, (Guid? Debit, Guid? Credit)> repartitori = null) {
        var linii = new List<Linie>();
        foreach (var r in randuri) {
            var debit = cat.Mapeaza(r.ContDebit)
                ?? throw new InvalidOperationException(
                    $"1C:{view}/{docId} rândul {r.Linie}: contul debitor „{r.ContDebit}” "
                    + "nu se mapează pe planul OMFP.");
            var credit = cat.Mapeaza(r.ContCredit)
                ?? throw new InvalidOperationException(
                    $"1C:{view}/{docId} rândul {r.Linie}: contul creditor „{r.ContCredit}” "
                    + "nu se mapează pe planul OMFP.");
            if (sareTva && EsteRandDeInchidereTva(debit, credit)) {
                RanduriTvaSarite++;
                continue;
            }
            if (r.Suma == 0m) {
                RanduriZero++;
                continue;
            }
            var (repDebit, repCredit) = repartitori?.Invoke(r.Linie) ?? (null, null);
            linii.Add(new Linie(debit, credit, r.Suma, repDebit, repCredit, r.Explicatie));
        }
        return linii;
    }

    // Materializarea e comună tuturor (inclusiv compensării): laturi interne
    // (SEDIU pe ambele — 46b cere repartitori NE-parteneri), linia poartă Tipul
    // tehnic TRZ și postarea explicită.
    public static Document Materializeaza(IObjectSpace os, Catalog cat, DateOnly data,
            string numar, IReadOnlyList<Linie> linii) {
        if (linii == null || linii.Count == 0)
            return null;
        var nota = os.CreateObject<NotaContabila>();
        nota.Data = data;
        nota.Numar = numar;
        nota.PredatorId = cat.SediuId;
        nota.PrimitorId = cat.SediuId;
        foreach (var l in linii) {
            var d = os.CreateObject<NotaContabilaDetaliu>();
            d.Document = nota;
            d.TipMaterialId = cat.TipTrezorerieId;
            d.ContDebitId = cat.Plan[l.Debit];
            d.ContCreditId = cat.Plan[l.Credit];
            d.RepartitorDebitId = l.RepartitorDebitId;
            d.RepartitorCreditId = l.RepartitorCreditId;
            d.Valoare = l.Valoare;
            d.Descriere = l.Descriere;
        }
        return nota;
    }

    public static void Raporteaza() {
        if (RanduriZero > 0 || RanduriTvaSarite > 0)
            Console.WriteLine($"  NTC: {RanduriZero} rânduri cu valoare zero sărite; "
                + $"{RanduriTvaSarite} rânduri de TVA sărite pentru ITV (4427 = 4426/4423/4424, "
                + "le generează închiderea Atlas la pasul 6).");
    }
}

// ======================= 1. Compensare → NTC + stingeri =======================
//
// Compensarea e stingere REALĂ (869 pe an), iar decizia 48b i-a dat rolul de
// stingător: o notă operată poate sting documente, cu contrapartidele pe liniile
// ei explicite. De aici două cerințe care se bat cap în cap doar aparent:
//
//  * nota trebuie să EXISTE chiar și când nu mișcă solduri — 819 din 869 sunt
//    „fără rulaj", cu ambele conturi identice (411.1 = 411.1 prin hub-ul tehnic
//    891). Rândul e inofensiv la sold ȘI e vehiculul stingerii: fără el n-ar
//    exista niciun document care să poarte capacitatea de stingere;
//  * repartitorii de pe linii sunt OBLIGATORII pentru capacitate
//    (`NotaContabila.CapacitateStingere` numără per repartitor, pe ambele
//    laturi) — se iau din subconto-ul rândului, cu antetul ca rezervă.
//
// Rândurile pe 891 se transcriu ca atare (48b): hub-ul tehnic al sursei e cont
// ca oricare altul în contractul de reconciliere.
static class HandlerCompensare {
    public const string View = "Compensare";

    public static readonly HandlerTip Handler =
        new(View, "Compensare (notă contabilă + stingeri)", Importa);

    public static int Note { get; private set; }
    public static int AnteteFaraRanduri { get; private set; }
    public static int LiniiFaraRepartitor { get; private set; }
    public static int LiniiCuRepartitorDinAntet { get; private set; }

    static void Importa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.Compensari(ctx.An, ctx.Luna))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    AnteteFaraRanduri++;
                    return;
                }
                List<NoteComune.Linie> linii = null;
                if (!bucla.EsteCunoscut(View, h.Id)) {
                    try {
                        linii = Planifica(ctx, h, randuri);
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(View, h.Id, ex);
                        return;
                    }
                    if (linii.Count == 0)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (linii is { Count: 0 })
                    return;
                if (bucla.ImportaDocument(View, h.Id,
                        os => NoteComune.Materializeaza(os, bucla.Catalog, DateOnly.FromDateTime(h.Data),
                            h.Numar, linii),
                        motivFaraDraft: Motive.FaraPlanLaReluare) == StareImport.Importat)
                    Note++;
            });
    }

    static List<NoteComune.Linie> Planifica(ContextLuna ctx, FlaxCompensare h,
            IReadOnlyList<FlaxRandNota> randuri) {
        var bucla = ctx.Bucla;
        var subconto = Subconto.IndexeazaTot(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        // Rezerva de antet, îngustată la ce cere realitatea (măsurat pe ianuarie–
        // martie: 2.055 rânduri de compensare, TOATE cu partener în subconto pe
        // latura de debit, 17 și pe credit, niciunul fără): se folosește DOAR când
        // sursa nu dă niciun partener pe niciuna dintre laturi. Umplerea „ca să fie"
        // ar pune contrapartida și pe latura hub-ului tehnic 891 — o dimensiune pe
        // care sursa n-o are — și ar dubla plafonul de stingere al notei degeaba.
        var partenerDebitAntet = bucla.LaCerere.AsiguraPartener(h.PartenerDebitId);
        var partenerCreditAntet = bucla.LaCerere.AsiguraPartener(h.PartenerCreditId);

        return NoteComune.Transcrie(bucla.Catalog, View, h.Id, randuri, sareTva: false,
            linie => {
                var debit = Repartitor(bucla, subconto, linie, Subconto.Debit);
                var credit = Repartitor(bucla, subconto, linie, Subconto.Credit);
                if (debit != null || credit != null)
                    return (debit, credit);
                LiniiCuRepartitorDinAntet++;
                if (partenerDebitAntet == null && partenerCreditAntet == null)
                    LiniiFaraRepartitor++;
                return (partenerDebitAntet, partenerCreditAntet);
            });
    }

    static Guid? Repartitor(BuclaImport bucla,
            Dictionary<(int, int), List<FlaxSubcontoNota>> subconto, int linie, int latura) {
        var referinta = subconto.Latura(linie, latura).DeTip(Subconto.TipPartener, Subconto.TipPersoana);
        return referinta?.Tip switch {
            Subconto.TipPartener => bucla.LaCerere.AsiguraPartener(referinta.Id),
            Subconto.TipPersoana => bucla.Catalog.Angajati.TryGetValue(referinta.Id, out var a) ? a : null,
            _ => null,
        };
    }

    // Trecerea 2: fiecare rând de compensare stinge documentul din subconto-ul
    // lui, cu VALOAREA ABSOLUTĂ a rândului — stingerea e o mărime pozitivă, iar
    // semnul rândului spune doar pe ce latură cade mișcarea.
    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        foreach (var h in bucla.Flax.Compensari(ctx.An, ctx.Luna)) {
            var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
            if (randuri.Count == 0)
                continue;
            var subconto = Subconto.IndexeazaTot(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
            foreach (var r in randuri) {
                if (r.Suma == 0m)
                    continue;
                // CÂTE O stingere per latură care poartă document (review advers,
                // D1): rândul 401=4111 cu factura furnizorului pe debit ȘI a
                // clientului pe credit stinge DOUĂ documente în 1C (104 linii pe
                // 2025) — exact cazul pentru care capacitatea notei e per latură
                // (2X pe același partener). Doar debitul ar pierde creditul.
                var debit = subconto.Latura(r.Linie, Subconto.Debit).DeFel(Subconto.FelDocumente);
                var credit = subconto.Latura(r.Linie, Subconto.Credit).DeFel(Subconto.FelDocumente);
                if (debit != null)
                    yield return new StingereSursa(View, h.Id, debit, Math.Abs(r.Suma));
                if (credit != null)
                    yield return new StingereSursa(View, h.Id, credit, Math.Abs(r.Suma));
            }
        }
    }

    public static void Raporteaza() {
        if (Note == 0 && AnteteFaraRanduri == 0)
            return;
        Console.WriteLine($"  CMP: {Note} compensări ca notă contabilă, {AnteteFaraRanduri} antete "
            + $"fără rânduri contabile (sărite), {LiniiFaraRepartitor} linii fără contrapartidă "
            + $"(nu pot sting nimic), {LiniiCuRepartitorDinAntet} linii cu contrapartida luată "
            + "din antet (subconto-ul rândului n-avea niciuna).");
    }
}

// ======================= 2. Familia notelor transcrise =======================
//
// Șase tipuri, o singură rutină: operațiunea manuală (874/an — nota contabilă
// manuală, cerința reală din decizia 9), salariile, casarea de mijloace fixe,
// închiderea de lună, importul și reevaluarea de imobilizări.
//
// Antetul dă identitatea, numărul și data; rândurile vin din registrul contabil
// al lunii (deja în memorie — se citește o dată per lună, nu per document).
// Ultimele două n-au view generat: antetul se ia din structura generică 1C
// (`FlaxDb.AnteteRaw`), rândurile rămân din același loc ca la toate celelalte.
static class HandlereNoteSimple {
    // `Repartitori` = tipul poartă contrapartide pe linii (doar Operațiunea, prin
    // secțiunea ei); `SareTva` = rândurile de TVA îi aparțin închiderii Atlas.
    sealed record Sursa(string View, string Descriere, bool SareTva, bool Repartitori,
        Func<ContextLuna, List<FlaxDocumentSimplu>> Antete);

    static readonly Sursa[] Surse = [
        new("Operatia", "Operațiune manuală (notă contabilă)", false, true,
            ctx => ctx.Bucla.Flax.Operatii(ctx.An, ctx.Luna)
                .Select(o => new FlaxDocumentSimplu(o.Id, o.Numar, o.Data)).ToList()),
        new("Salarii", "Salarii (notă contabilă)", false, false,
            ctx => ctx.Bucla.Flax.Salarii(ctx.An, ctx.Luna)),
        new("CasareMF", "Casare de mijloace fixe (notă contabilă)", false, false,
            ctx => ctx.Bucla.Flax.CasariMF(ctx.An, ctx.Luna)),
        new("InchidereLunaDeExercitiu", "Închiderea lunii (notă contabilă, fără TVA)", true, false,
            ctx => ctx.Bucla.Flax.InchideriLuna(ctx.An, ctx.Luna)),
        new("Import", "Import (notă contabilă)", false, false,
            ctx => ctx.Bucla.Flax.Importuri(ctx.An, ctx.Luna)),
        // Fără view generat: TypeRef 0x18C0 ⇒ tabela generică `_Document6336`.
        new("ReevaluareMF", "Reevaluare de imobilizări (notă contabilă)", false, false,
            ctx => ctx.Bucla.Flax.AnteteFostRaw("ReevaluareMF", 6336, ctx.An, ctx.Luna)),
    ];

    public static IEnumerable<HandlerTip> Handlere =>
        Surse.Select(s => new HandlerTip(s.View, s.Descriere, ctx => Importa(ctx, s)));

    static readonly Dictionary<string, int> note = new(StringComparer.Ordinal);
    public static int AnteteFaraRanduri { get; private set; }

    static void Importa(ContextLuna ctx, Sursa sursa) {
        var bucla = ctx.Bucla;
        // Contrapartidele Operațiunii stau în secțiunea ei, aliniată 1:1 cu
        // rândurile de registru pe (document, linie) — verificat pe tot anul
        // (2.904 rânduri, zero diferențe de cont sau sumă).
        var dimensiuni = sursa.Repartitori
            ? bucla.Flax.OperatiiRanduri(ctx.An, ctx.Luna)
                .ToDictionary(x => (x.DocumentId, x.Linie), x => x, TupluOrdinal.Instanta)
            : null;

        foreach (var h in sursa.Antete(ctx))
            ctx.Planifica(h.Data, h.Numar, () => {
                var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
                if (randuri.Count == 0) {
                    AnteteFaraRanduri++;
                    return;
                }
                List<NoteComune.Linie> linii = null;
                if (!bucla.EsteCunoscut(sursa.View, h.Id)) {
                    try {
                        linii = NoteComune.Transcrie(bucla.Catalog, sursa.View, h.Id, randuri, sursa.SareTva,
                            dimensiuni == null ? null : linie => Repartitori(bucla, dimensiuni, h.Id, linie));
                    }
                    catch (Exception ex) {
                        bucla.EsecPlanificare(sursa.View, h.Id, ex);
                        return;
                    }
                    if (linii.Count == 0)
                        bucla.NumaraSursaFaraCorespondent();
                }
                if (linii is { Count: 0 })
                    return;
                if (bucla.ImportaDocument(sursa.View, h.Id,
                        os => NoteComune.Materializeaza(os, bucla.Catalog, DateOnly.FromDateTime(h.Data),
                            h.Numar, linii),
                        motivFaraDraft: Motive.FaraPlanLaReluare) == StareImport.Importat)
                    note[sursa.View] = note.GetValueOrDefault(sursa.View) + 1;
            });
    }

    static (Guid? Debit, Guid? Credit) Repartitori(BuclaImport bucla,
            Dictionary<(string, int), FlaxOperatieRand> dimensiuni, string docId, int linie) {
        if (!dimensiuni.TryGetValue((docId, linie), out var d))
            return (null, null);
        return (Repartitor(bucla, d.PartenerDebitId, d.PersoanaDebitId),
            Repartitor(bucla, d.PartenerCreditId, d.PersoanaCreditId));
    }

    static Guid? Repartitor(BuclaImport bucla, string partenerHex, string persoanaHex) =>
        partenerHex != null ? bucla.LaCerere.AsiguraPartener(partenerHex)
            : persoanaHex != null && bucla.Catalog.Angajati.TryGetValue(persoanaHex, out var a) ? a
            : null;

    public static void Raporteaza() {
        if (note.Count == 0 && AnteteFaraRanduri == 0)
            return;
        Console.WriteLine($"  Note transcrise: "
            + string.Join(", ", note.OrderByDescending(x => x.Value).Select(x => $"{x.Key} {x.Value}"))
            + $"; {AnteteFaraRanduri} antete fără rânduri contabile (sărite).");
    }
}

// Comparator pentru cheia (document, linie): id-urile 1C sunt hex, deci
// comparația ordinală e și corectă, și cea mai ieftină.
sealed class TupluOrdinal : IEqualityComparer<(string, int)> {
    public static readonly TupluOrdinal Instanta = new();

    public bool Equals((string, int) x, (string, int) y) =>
        x.Item2 == y.Item2 && string.Equals(x.Item1, y.Item1, StringComparison.Ordinal);

    public int GetHashCode((string, int) obj) =>
        HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Item1 ?? ""), obj.Item2);
}
