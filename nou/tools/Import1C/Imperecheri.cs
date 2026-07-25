using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 5 al feliei 1C-c, partea a III-a: TRECEREA 2 a lunii — stingerile.
//
// De ce o trecere separată (§12.2): imperecherea NU postează registre, deci
// amânarea ei față de operarea documentelor e gratuită și rezolvă dintr-o
// mișcare problema de ordine — plata din 3 ianuarie stinge o factură emisă pe 20,
// iar handlerele importă grupat pe tip, nu strict cronologic. După ce luna e
// operată, toate documentele ei există.
//
// Sursele sunt aceleași rânduri de registru din care s-au născut documentele de
// trezorerie și notele de compensare: `Stingeri(ctx)` re-derivă exact aceleași
// chei (analiza e o funcție pură de sursă, fără bază de date). Consecința e că
// trecerea 2 e INDEPENDENTĂ de starea în memorie a trecerii 1: o rulare care
// moare între ele reia stingerile la următoarea, fără să le piardă și fără să le
// dubleze (idempotența e pe `MigrareLegatura`, ca peste tot).
//
// Ce NU se împerechează, cu contor și raport (48b — invariant picat pe date
// reale = raport, nu stop):
//  * ținta e soldul de deschidere (`IntroducereaSoldurilor`) — modelul 34d/47c:
//    terții pornesc pe sold global per partener, fără facturi istorice, deci
//    plata stinge soldul fără link;
//  * ținta e un retur (RDC/RLF) — totalul lor e negativ (46f), iar imperecherea
//    cere sume pozitive; designul compensare/rambursare pe retur e amânat;
//  * ținta n-a fost (încă) importată — tip nemapat, document dintr-o lună
//    ulterioară sau un tip pe care Atlas îl sparge în mai multe documente
//    (extrasul, plata de casă), unde „documentul stins" n-are corespondent unic;
//  * stingătorul n-a devenit document (rândul lui a intrat în punte);
//  * invariantul serviciului refuză (sumă peste rest, contrapartidă diferită).
static class Imperecheri1C {
    // Tabela de legături proprie: „1C:Imperechere". Cheia = cheia stingătorului
    // (care conține deja identitatea documentului sursă) + ținta, deci o pereche
    // stingător↔stins are exact o legătură.
    public const string View = "Imperechere";

    // Tipurile-țintă care se sar prin construcție, nu din lipsă de date.
    const string SoldDeschidere = "IntroducereaSoldurilor";
    static readonly string[] Retururi = ["ReturDeLaClient", "ReturLaFurnizor"];

    public static int Create { get; private set; }
    public static int Recuperate { get; private set; }
    public static int Existente { get; private set; }
    static readonly Dictionary<string, int> sarite = new(StringComparer.Ordinal);
    static readonly Dictionary<string, decimal> valoareSarita = new(StringComparer.Ordinal);
    static int detaliiRefuz;

    // Câte instanțe se lasă pe un singur ObjectSpace înainte de reciclare: fiecare
    // imperechere comite, iar change tracker-ul ar crește la zeci de mii de
    // entități pe o lună de extrase.
    const int LotObjectSpace = 200;

    public static void Executa(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var sursa = HandlerExtras.Stingeri(ctx)
            .Concat(HandlerPlataCasa.Stingeri(ctx))
            .Concat(HandlerIncasareCasa.Stingeri(ctx))
            .Concat(HandlerCard.Stingeri(ctx))
            .Concat(HandlerCompensare.Stingeri(ctx))
            .ToList();

        // Agregarea per pereche (stingător, stins): mai multe rânduri de registru
        // pot stinge același document (o plată care acoperă două poziții ale
        // aceleiași facturi). Un singur link cu suma totală — imperecherea pe
        // poziții rămâne amânată (31f).
        var perechi = sursa
            .Where(s => s.Tinta != null)
            .GroupBy(s => (s.View, s.CheieStingator, TintaTip: s.Tinta.Tip, TintaId: s.Tinta.Id))
            .Select(g => (g.Key, Suma: g.Sum(x => x.Suma)))
            .ToList();
        // Descrierile țintelor, pentru triajul „dinaintea ferestrei" de mai jos.
        var descrieri = sursa.Where(s => s.Tinta?.Descriere != null)
            .GroupBy(s => s.Tinta.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Tinta.Descriere, StringComparer.Ordinal);

        Dictionary<string, Guid> legaturi;
        using (var citire = bucla.CreeazaObjectSpace())
            legaturi = Legaturi.Incarca(citire, View);
        IObjectSpace os = null;
        var peLot = 0;
        var create = 0;
        var existenteLaStart = Existente;
        try {
            foreach (var (cheie, suma) in perechi) {
                var cheieLegatura = $"{cheie.View}/{cheie.CheieStingator}->{cheie.TintaId}";
                if (legaturi.ContainsKey(cheieLegatura)) {
                    Existente++;
                    continue;
                }
                if (suma <= 0) {
                    Sare("sumă ne-pozitivă după agregare", suma);
                    continue;
                }
                if (cheie.TintaTip == null) {
                    Sare("tip de document-țintă necunoscut în sursă", suma);
                    continue;
                }
                if (cheie.TintaTip == SoldDeschidere) {
                    Sare("ținta e soldul de deschidere (34d — terții pornesc pe sold global)", suma);
                    continue;
                }
                var stingatorId = bucla.Tinta(cheie.View, cheie.CheieStingator);
                if (stingatorId == null) {
                    Sare("stingătorul n-a devenit document (rândul lui e transcris în punte)", suma);
                    continue;
                }
                var tintaId = bucla.Tinta(cheie.TintaTip, cheie.TintaId);
                if (tintaId == null) {
                    // Cel mai frecvent caz al lunii ianuarie, și e legitim: plata
                    // stinge o factură din 2024, dinaintea ferestrei de import —
                    // adică exact soldul de deschidere (34d). Se separă de restul
                    // („încă neimportată") prin data din descrierea 1C a țintei,
                    // parsată cu regula loturilor (47d), ca raportul să nu pună la
                    // un loc un fapt de model cu o gaură de acoperire.
                    // Granița e ÎNCEPUTUL ANULUI importat, nu prima zi a lunii
                    // curente: o țintă din ianuarie rămasă neimportată în februarie
                    // e o gaură de acoperire, nu sold de deschidere.
                    var dataTinta = Deschidere.ParseData(descrieri.GetValueOrDefault(cheie.TintaId));
                    Sare(dataTinta != null && dataTinta < new DateOnly(ctx.An, 1, 1)
                        ? $"ținta {cheie.TintaTip} e dinaintea ferestrei de import (sold de deschidere)"
                        : $"ținta {cheie.TintaTip} nu e importată (lună ulterioară sau tip nemapat)", suma);
                    continue;
                }
                // Retururile se verifică DUPĂ rezolvarea țintei: un retur din 2024
                // e „dinaintea ferestrei" (motivul real pentru care nu se poate
                // stinge), nu „retur amânat" — categoriile trebuie să spună ce s-a
                // întâmplat, nu care test a picat primul.
                if (Retururi.Contains(cheie.TintaTip)) {
                    Sare("ținta e un retur importat (total negativ — 46f, amânat)", suma);
                    continue;
                }
                if (bucla.Stare(stingatorId.Value) != StareDocument.Operat
                        || bucla.Stare(tintaId.Value) != StareDocument.Operat) {
                    Sare("stingătorul sau ținta nu sunt operate", suma);
                    continue;
                }

                if (os == null || peLot >= LotObjectSpace) {
                    os?.Dispose();
                    os = bucla.CreeazaObjectSpace();
                    peLot = 0;
                }
                peLot++;
                if (Creeaza(bucla, os, stingatorId.Value, tintaId.Value, suma, cheieLegatura))
                    create++;
                else
                    // Un commit refuzat lasă ObjectSpace-ul cu modificări retrase,
                    // dar nu merită pariat pe curățenia lui: se reciclează.
                    peLot = LotObjectSpace;
            }
        }
        finally {
            os?.Dispose();
        }
        if (perechi.Count > 0)
            Console.WriteLine($"  Imperecheri: {create} create în luna asta "
                + $"({perechi.Count} perechi în sursă, {Existente - existenteLaStart} deja legate).");
    }

    static bool Creeaza(BuclaImport bucla, IObjectSpace os, Guid stingatorId, Guid tintaId,
            decimal suma, string cheieLegatura) {
        try {
            // Recuperarea rulării întrerupte între imperechere și legătura ei
            // (`Imperecheaza` comite singur): perechea e unică prin agregare, deci
            // un link existent pe exact aceleași două documente nu poate fi decât
            // al nostru — se adoptă, nu se dublează (mecanica 47b).
            var existent = os.FirstOrDefault<Imperechere>(
                i => i.DocumentStingatorId == stingatorId && i.DocumentId == tintaId);
            if (existent == null) {
                var stingator = os.GetObjectByKey<Document>(stingatorId);
                var tinta = os.GetObjectByKey<Document>(tintaId);
                existent = ImperechereService.Imperecheaza(os, stingator, tinta, suma);
                Create++;
            }
            else
                Recuperate++;
            Legaturi.Leaga(os, View, cheieLegatura, existent.ID);
            os.CommitChanges();
            return true;
        }
        catch (Exception ex) {
            os.Rollback();
            Sare(Motiv(ex), suma);
            if (++detaliiRefuz <= 20) {
                var cauze = new List<string>();
                for (var e = ex; e != null; e = e.InnerException)
                    cauze.Add($"{e.GetType().Name}: {e.Message}");
                bucla.Avert($"Imperecherea {cheieLegatura} ({suma:N2}) a fost refuzată: "
                    + string.Join(" ← ", cauze));
            }
            return false;
        }
    }

    // Triajul refuzurilor. Cele de BUSINESS sunt divergențe reale între sursă și
    // model, fiecare cu înțelesul ei (48b: raport, nu stop). Restul e DEFECT și
    // se strigă ca atare — vezi nota de mai jos.
    static string Motiv(Exception ex) => ex switch {
        OperareException when ex.Message.Contains("restul nestins") =>
            "sursa stinge peste totalul documentului stins (divergență de valoare 1C↔Atlas)",
        OperareException when ex.Message.Contains("restul neasignat") =>
            "sursa stinge peste totalul stingătorului (rânduri de semn opus în același grup)",
        OperareException when ex.Message.Contains("contrapartidă") =>
            "ținta nu poartă contrapartida pe latură (notă contabilă transcrisă, aviz, document intern)",
        OperareException => "invariantul imperecherii refuză (alt motiv)",
        // DEFECT DE MODEL, semnalat pentru arhitect (nu se repară în import —
        // Module e read-only în felia asta): coloanele de bani sunt `numeric`
        // FĂRĂ scară fixată, iar valorile materializate de motor moștenesc scara
        // împărțirii care le-a produs (PretUnitar = net / cantitate ⇒ Valoare =
        // „6449,3900000000000000000000001", 29 de cifre semnificative). Fiecare
        // valoare încape singură în `decimal`, dar `ImperechereService.Total` le
        // ADUNĂ server-side (`Valoare + ValoareTva`), iar suma depășește mantisa.
        // Reparația e a Module-ului (rotunjirea valorii la materializare sau
        // scară fixă pe coloane) și atinge orice consumator care adună bani în
        // SQL — inclusiv proiecțiile pasului 5 (42c).
        OverflowException =>
            "citirea totalului pică pe scara numerică a valorii (defect de Module — vezi raportul)",
        _ => $"eroare neașteptată ({ex.GetType().Name})",
    };

    static void Sare(string motiv, decimal suma) {
        sarite[motiv] = sarite.GetValueOrDefault(motiv) + 1;
        valoareSarita[motiv] = valoareSarita.GetValueOrDefault(motiv) + suma;
    }

    public static void Raporteaza() {
        if (Create == 0 && sarite.Count == 0)
            return;
        Console.WriteLine($"  Imperecheri (total rulare): {Create} create, {Recuperate} recuperate, "
            + $"{Existente} deja legate.");
        foreach (var s in sarite.OrderByDescending(x => x.Value))
            Console.WriteLine($"    {s.Value,8} sărite — {s.Key} (Σ {valoareSarita[s.Key]:N2} lei)");
    }
}
