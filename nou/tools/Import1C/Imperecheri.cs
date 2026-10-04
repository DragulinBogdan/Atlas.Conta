using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

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
    // 107f: fără rând `Imperechere`; idempotența o ține legătura.
    public const string ViewDeschidere = "StingereDeschidere";

    // Tipurile-țintă care se sar prin construcție, nu din lipsă de date.
    const string SoldDeschidere = "IntroducereaSoldurilor";
    static readonly string[] Retururi = ["ReturDeLaClient", "ReturLaFurnizor"];

    public static int Create { get; private set; }
    public static int Recuperate { get; private set; }
    public static int Existente { get; private set; }
    static readonly Dictionary<string, int> sarite = new(StringComparer.Ordinal);
    static readonly Dictionary<string, decimal> valoareSarita = new(StringComparer.Ordinal);
    static int detaliiRefuz;

    public static int StinsePeDeschidere { get; private set; }
    public static int ExistenteDeschidere { get; private set; }
    public static decimal SumaStinsaPeDeschidere { get; private set; }
    // 107h: mișcarea cerută de sursă și neaplicată pe partidă, semnată debit − credit.
    public static readonly Dictionary<Guid, decimal> RefuzatPePartida = [];
    public static readonly Dictionary<Guid, decimal> PlafonatPePartida = [];
    public static int Plafonate { get; private set; }
    public static int EsecuriTehnice { get; private set; }
    public static bool ProbaEsec { get; set; }
    static bool probaEsecFacuta;
    static Dictionary<(Guid Document, Guid Partida), (decimal Total, decimal Ramas)> aplicate;
    static Dictionary<string, List<(Guid Partida, Guid Cont, Guid Partener)>> indexDeschidere;

    static IReadOnlyList<(Guid Partida, Guid Cont, Guid Partener)> PartideDeschidere(BuclaImport bucla,
            string tipRef, string id) {
        if (indexDeschidere == null) {
            indexDeschidere = new Dictionary<string, List<(Guid, Guid, Guid)>>(StringComparer.Ordinal);
            using var os = bucla.CreeazaObjectSpace();
            var fapte = os.GetObjectsQuery<Postare>()
                .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere && p.FelUnitate == N.FelUnitate.Partida)
                .Select(p => new { p.Unitate, p.Cont, p.Partener }).ToList()
                .ToDictionary(x => x.Unitate.Value);
            foreach (var (cheie, partida) in Legaturi.Incarca(os, Deschidere.ViewPartide)) {
                var referinta = cheie.Split('|')[2];
                if (referinta == Deschidere.CheieFaraDocument || !fapte.TryGetValue(partida, out var f) || f.Partener == null)
                    continue;
                (indexDeschidere.TryGetValue(referinta, out var lista)
                    ? lista : indexDeschidere[referinta] = []).Add((partida, f.Cont, f.Partener.Value));
            }
        }
        var refCheie = Deschidere.RefCheie(tipRef, id);
        return refCheie != null && indexDeschidere.TryGetValue(refCheie, out var l) ? l : [];
    }

    // Câte instanțe se lasă pe un singur ObjectSpace înainte de reciclare: fiecare
    // imperechere comite, iar change tracker-ul ar crește la zeci de mii de
    // entități pe o lună de extrase.
    const int LotObjectSpace = 200;

    // Sursa cu sens cunoscut se agregă semnat: suma e modulul netului, sensul e semnul lui, iar netul zero dă
    // sumă zero (sărită). Sensul necunoscut (null) rămâne necunoscut și suma se adună ca până acum.
    internal static List<((string View, string CheieStingator, string TintaTip, string TintaId, string TipRef) Key,
            decimal Suma, int? Sens)> Agrega(IEnumerable<StingereSursa> sursa) =>
        sursa.Where(s => s.Tinta != null)
            .GroupBy(s => (s.View, s.CheieStingator, TintaTip: s.Tinta.Tip, TintaId: s.Tinta.Id, TipRef: s.Tinta.TipRef))
            .Select(g => {
                if (g.Any(x => x.Sens == null))
                    return (g.Key, g.Sum(x => x.Suma), (int?)null);
                var net = g.Sum(x => x.Sens.Value * x.Suma);
                return (g.Key, Math.Abs(net), (int?)Math.Sign(net));
            })
            .ToList();

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
        var perechi = Agrega(sursa);
        // Descrierile țintelor, pentru triajul „dinaintea ferestrei" de mai jos.
        var descrieri = sursa.Where(s => s.Tinta?.Descriere != null)
            .GroupBy(s => s.Tinta.Id, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First().Tinta.Descriere, StringComparer.Ordinal);

        Dictionary<string, Guid> legaturi;
        Dictionary<string, Guid> legaturiDeschidere;
        using (var citire = bucla.CreeazaObjectSpace()) {
            legaturi = Legaturi.Incarca(citire, View);
            legaturiDeschidere = Legaturi.Incarca(citire, ViewDeschidere);
        }
        aplicate = null;
        IObjectSpace os = null;
        var peLot = 0;
        var create = 0;
        var existenteLaStart = Existente;
        try {
            foreach (var (cheie, suma, sens) in perechi) {
                var cheieLegatura = $"{cheie.View}/{cheie.CheieStingator}->{cheie.TintaId}";
                if (legaturi.ContainsKey(cheieLegatura)) {
                    Existente++;
                    continue;
                }
                if (suma <= 0) {
                    Sare("sumă ne-pozitivă după agregare", suma);
                    continue;
                }
                if (bucla.Tinta(cheie.TintaTip, cheie.TintaId) == null
                        && PartideDeschidere(bucla, cheie.TipRef, cheie.TintaId) is { Count: > 0 } candidate) {
                    var cheieDeschidere = $"{cheie.View}/{cheie.CheieStingator}->{cheie.TipRef}/{cheie.TintaId}";
                    if (legaturiDeschidere.TryGetValue(cheieDeschidere, out var partidaLegata)) {
                        ExistenteDeschidere++;
                        RederivaPlafonarea(bucla, bucla.Tinta(cheie.View, cheie.CheieStingator), partidaLegata,
                            suma, cheieDeschidere);
                        continue;
                    }
                    var stingatorDeschidere = bucla.Tinta(cheie.View, cheie.CheieStingator);
                    if (stingatorDeschidere == null) {
                        Sare("stingătorul n-a devenit document (rândul lui e transcris în punte)", suma);
                        continue;
                    }
                    if (bucla.Stare(stingatorDeschidere.Value) != StareDocument.Operat) {
                        Sare("stingătorul nu e operat (partidă de deschidere)", suma);
                        continue;
                    }
                    if (os == null || peLot >= LotObjectSpace) {
                        os?.Dispose();
                        os = bucla.CreeazaObjectSpace();
                        peLot = 0;
                    }
                    peLot++;
                    if (CreeazaPeDeschidere(bucla, os, stingatorDeschidere.Value, candidate, suma, sens, cheieDeschidere))
                        create++;
                    else
                        peLot = LotObjectSpace;
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
                // 107g
                var data = stingator.DataInregistrare > tinta.DataInregistrare
                    ? stingator.DataInregistrare : tinta.DataInregistrare;
                existent = ImperechereService.Imperecheaza(os, stingator, tinta, suma, data: data);
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
            if (EsteEroareTehnica(ex))
                EsecuriTehnice++;
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

    static bool CreeazaPeDeschidere(BuclaImport bucla, IObjectSpace os, Guid stingatorId,
            IReadOnlyList<(Guid Partida, Guid Cont, Guid Partener)> candidate, decimal suma, int? sensSursa,
            string cheieLegatura) {
        var proprii = os.GetObjectsQuery<Postare>()
            .Where(p => p.DocumentId == stingatorId && p.Carte == N.Carte.Contabil
                && p.FelUnitate == N.FelUnitate.Partida && p.Partener != null
                && p.Tranzactie.Fel == N.FelTranzactie.Operare)
            .Select(p => new { p.Cont, p.Partener, p.Unitate, Semnat = p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare })
            .ToList();
        var partida = candidate.FirstOrDefault(c => proprii.Any(p => p.Cont == c.Cont && p.Partener == c.Partener));
        if (partida == default) {
            Sare("partida de deschidere există, dar stingătorul nu postează pe (cont, partener) al ei", suma);
            return false;
        }
        var restPartida = os.GetObjectsQuery<Postare>()
            .Where(p => p.Unitate == partida.Partida && p.Carte == N.Carte.Contabil)
            .Select(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare).ToList().Sum();
        var sensPartida = Math.Sign(restPartida);
        var sensMiscare = sensSursa
            ?? Math.Sign(proprii.Where(p => p.Cont == partida.Cont && p.Partener == partida.Partener).Sum(p => p.Semnat));
        if (sensMiscare != 0 && sensMiscare == sensPartida) {
            Refuza(partida.Partida, sensMiscare * suma, suma,
                "partida de deschidere refuză: sursa mișcă partida în sensul soldului ei (semn inversat, M1-D7)");
            return false;
        }
        var restAbs = Math.Abs(restPartida);
        if (restAbs < 0.01m) {
            Refuza(partida.Partida, sensMiscare * suma, suma,
                "partida de deschidere e deja stinsă (sursa stinge peste rest)");
            return false;
        }
        var deStins = Math.Min(suma, restAbs);
        if (ProbaEsec && !probaEsecFacuta) {
            probaEsecFacuta = true;
            ProbeazaEsecul(bucla, stingatorId, partida.Partida, deStins, cheieLegatura);
        }
        var rezultat = Stinge(os, stingatorId, partida.Partida, deStins, cheieLegatura, null, out var eroare);
        if (rezultat == RezultatStingere.Stins) {
            StinsePeDeschidere++;
            SumaStinsaPeDeschidere += deStins;
            Plafoneaza(partida.Partida, -sensPartida * (suma - deStins));
            return true;
        }
        if (rezultat == RezultatStingere.Refuzat)
            Refuza(partida.Partida, -sensPartida * suma, suma,
                "partida de deschidere refuză: stingătorul n-are rest pe partida proprie (M1-D7)");
        else {
            EsecuriTehnice++;
            Sare($"EȘEC — stingerea pe partida inițială a căzut tehnic ({eroare.GetType().Name})", suma);
        }
        if (rezultat == RezultatStingere.Esec || ++detaliiRefuzDeschidere <= 60) {
            var unitati = proprii.Where(p => p.Cont == partida.Cont && p.Partener == partida.Partener)
                .Select(p => p.Unitate).Distinct().ToList();
            var restPropriu = os.GetObjectsQuery<Postare>()
                .Where(p => p.Unitate != null && unitati.Contains(p.Unitate) && p.Carte == N.Carte.Contabil)
                .Select(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare).ToList().Sum();
            bucla.Avert($"Stingerea pe partida inițială {cheieLegatura} ({suma:N2}, de stins {deStins:N2}) "
                + $"{(rezultat == RezultatStingere.Esec ? "A CĂZUT TEHNIC" : "refuzată")}: "
                + $"rest partidă {restPartida:N2}, rest propriu {restPropriu:N2} pe {unitati.Count} unități — "
                + $"{eroare.GetType().Name}: {eroare.Message.Split('\n')[0]}");
        }
        return false;
    }

    internal enum RezultatStingere { Stins, Refuzat, Esec }

    internal const string DefectInceput = "începutul tranzacției";
    internal const string DefectPersistare = "persistare";

    static RezultatStingere Stinge(IObjectSpace os, Guid stingatorId, Guid partida, decimal deStins,
            string cheieLegatura, string defect, out Exception eroare) {
        eroare = null;
        try {
            if (defect == DefectInceput)
                throw new TimeoutException("probă");
            using var tx = TranzactieComanda.Incepe(os);
            var stingator = os.GetObjectByKey<Document>(stingatorId);
            Materializare.Imperecheaza(os, stingator, partida, deStins, stingator.DataInregistrare);
            Legaturi.Leaga(os, ViewDeschidere, cheieLegatura, partida);
            if (defect == DefectPersistare)
                throw new InvalidOperationException("probă");
            os.CommitChanges();
            tx.Commit();
            return RezultatStingere.Stins;
        }
        catch (Exception ex) {
            os.Rollback();
            eroare = ex;
            return EsteRefuzDeBusiness(ex) ? RezultatStingere.Refuzat : RezultatStingere.Esec;
        }
    }

    // Singurul refuz nominalizat al stingerii pe partidă inițială; orice altceva e eșec.
    internal static bool EsteRefuzDeBusiness(Exception ex) =>
        ex is OperareException
            && ex.Message.StartsWith(Materializare.StingereDeschidereInvalida, StringComparison.Ordinal)
            && ex.Message.Contains("restul disponibil");

    internal static bool EsteEroareTehnica(Exception ex) => ex switch {
        OperareException => ex.Message.StartsWith(TranzactieComanda.ScriereOcupata, StringComparison.Ordinal),
        OverflowException => false,
        _ => true,
    };

    static void Refuza(Guid partida, decimal semnat, decimal suma, string motiv) {
        Sare(semnat == 0m ? motiv + " — sens nedeterminat, fără explicație în contractul 5" : motiv, suma);
        if (semnat != 0m)
            RefuzatPePartida[partida] = RefuzatPePartida.GetValueOrDefault(partida) + semnat;
    }

    static void Plafoneaza(Guid partida, decimal semnat) {
        if (semnat == 0m)
            return;
        Plafonate++;
        PlafonatPePartida[partida] = PlafonatPePartida.GetValueOrDefault(partida) + semnat;
        valoarePlafonata += Math.Abs(semnat);
    }

    static void RederivaPlafonarea(BuclaImport bucla, Guid? stingator, Guid partida, decimal suma, string cheieLegatura) {
        aplicate ??= Aplicate(bucla);
        var cheie = (stingator ?? Guid.Empty, partida);
        if (!aplicate.TryGetValue(cheie, out var transfer) || transfer.Total == 0m) {
            EsecuriTehnice++;
            bucla.Avert($"Legătura {ViewDeschidere}/{cheieLegatura} există, dar cubul n-are transferul ei pe partida {partida}.");
            return;
        }
        var aplicat = Math.Min(Math.Abs(transfer.Ramas), suma);
        aplicate[cheie] = (transfer.Total, transfer.Ramas - Math.Sign(transfer.Total) * aplicat);
        Plafoneaza(partida, Math.Sign(transfer.Total) * (suma - aplicat));
    }

    static Dictionary<(Guid, Guid), (decimal, decimal)> Aplicate(BuclaImport bucla) {
        using var os = bucla.CreeazaObjectSpace();
        var initiale = os.GetObjectsQuery<Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Deschidere && p.FelUnitate == N.FelUnitate.Partida)
            .Select(p => p.Unitate);
        return os.GetObjectsQuery<Postare>()
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Transfer && p.Carte == N.Carte.Contabil
                && p.DocumentId != null && initiale.Contains(p.Unitate))
            .GroupBy(p => new { p.DocumentId, p.Unitate })
            .Select(g => new { g.Key.DocumentId, g.Key.Unitate,
                Semnat = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) })
            .ToList()
            .ToDictionary(x => (x.DocumentId.Value, x.Unitate.Value), x => (x.Semnat, x.Semnat));
    }

    static void ProbeazaEsecul(BuclaImport bucla, Guid stingatorId, Guid partida, decimal deStins, string cheieLegatura) {
        foreach (var defect in new[] { DefectInceput, DefectPersistare }) {
            RezultatStingere rezultat;
            using (var os = bucla.CreeazaObjectSpace())
                rezultat = Stinge(os, stingatorId, partida, deStins, cheieLegatura, defect, out _);
            using var citire = bucla.CreeazaObjectSpace();
            var legata = Legaturi.Incarca(citire, ViewDeschidere).ContainsKey(cheieLegatura);
            var transferuri = citire.GetObjectsQuery<Postare>().Count(p => p.DocumentId == stingatorId
                && p.Unitate == partida && p.Tranzactie.Fel == N.FelTranzactie.Transfer);
            bucla.Check($"  probă M1-R4: eroarea la {defect} e eșec, fără legătură ({(legata ? 1 : 0)}) "
                + $"și fără transfer în cub ({transferuri})",
                rezultat == RezultatStingere.Esec && !legata && transferuri == 0);
        }
    }

    static decimal valoarePlafonata;
    static int detaliiRefuzDeschidere;

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
        if (Create == 0 && sarite.Count == 0 && StinsePeDeschidere == 0)
            return;
        Console.WriteLine($"  Imperecheri (total rulare): {Create} create, {Recuperate} recuperate, "
            + $"{Existente} deja legate; pe partide inițiale: {StinsePeDeschidere} stinse "
            + $"(Σ {SumaStinsaPeDeschidere:N2} lei), {ExistenteDeschidere} deja legate, "
            + $"{RefuzatPePartida.Count} partide cu refuzuri (Σ {RefuzatPePartida.Values.Sum(v => Math.Abs(v)):N2}), "
            + $"{Plafonate} plafonate la restul partidei (excedent Σ {valoarePlafonata:N2}), "
            + $"{EsecuriTehnice} erori tehnice.");
        foreach (var s in sarite.OrderByDescending(x => x.Value))
            Console.WriteLine($"    {s.Value,8} sărite — {s.Key} (Σ {valoareSarita[s.Key]:N2} lei)");
    }
}
