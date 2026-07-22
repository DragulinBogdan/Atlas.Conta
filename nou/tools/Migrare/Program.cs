using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Migrare;

// Pasul 4 — migrarea datelor (decizia 12): green-field la graniță de ciclu.
// Sursa NU e istoricul legacy, ci DESCHIDEREA materializată de trecerea de an
// legacy în baza anului nou: nomenclatoare + solduri_repartitori + LDI-ul
// administrativ de 31.12 + gest_gnmcl. Ținta: nomenclatoarele noi + rânduri de
// registru cu DocumentId=null (decizia 25e). Idempotentă: corelarea legacy→nou
// trăiește în MigrareLegatura; rândurile de deschidere se rescriu integral.
// Reconcilierea (pasul 4 din plan): soldurile citite ÎNAPOI din Postgres se
// compară per cont cu soldurile legacy; diferențele stoc↔contabilitate ale
// legacy-ului se raportează explicit, nu se ascund.

var legacyCs = args.Length > 0
    ? args[0]
    : "Server=(local);Database=Contabilitate_2026;Integrated Security=True;TrustServerCertificate=True";
var pgCs = args.Length > 1
    ? args[1]
    : "Host=localhost;Port=5444;Username=postgres;Password=postgres;Database=Atlas.Conta.BackOffice";

var esecuri = 0;
void Check(string nume, bool ok) {
    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {nume}");
    if (!ok)
        esecuri++;
}
var avertismente = new List<string>();
void Avert(string mesaj) => avertismente.Add(mesaj);

// ============================ Citirea legacy ============================

using var legacy = new LegacyDb(legacyCs);
var an = legacy.AnDeschidere();
if (an == 0) {
    Console.WriteLine("FAIL solduri_repartitori e gol — baza legacy nu are deschidere materializată.");
    return 1;
}
var dataDeschidere = new DateTime(an - 1, 12, 31);
Console.WriteLine($"Deschidere an {an} (rânduri de registru la {dataDeschidere:yyyy-MM-dd}).");

var repartitoriLegacy = legacy.Repartitori();
var casierieLegacy = legacy.Casierie();
var solduriLegacy = legacy.Solduri();
var stocLegacy = legacy.StocDeschidere(dataDeschidere);
var loturiLegacy = legacy.Loturi();
var produseLegacy = legacy.Produse();
var tipCont = legacy.TipuriMaterial();
Console.WriteLine($"Legacy: {repartitoriLegacy.Count} repartitori, {casierieLegacy.Count} conturi proprii, "
    + $"{solduriLegacy.Count} solduri, {stocLegacy.Count} linii stoc deschidere.");

using var provider = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
    (builder, _) => builder
        .UseNpgsql(pgCs)
        .UseChangeTrackingProxies()
        .UseObjectSpaceLinkProxies()
        .UseLazyLoadingProxies());

// Corelarea legacy→nou per tabelă (idempotență): cheie = id-ul legacy ca text.
Dictionary<string, Guid> Legaturi(IObjectSpace os, string tabela) =>
    os.GetObjectsQuery<MigrareLegatura>().Where(m => m.Tabela == tabela)
        .ToDictionary(m => m.CheieLegacy, m => m.TintaId);
void Leaga(IObjectSpace os, string tabela, string cheie, Guid tinta) {
    var l = os.CreateObject<MigrareLegatura>();
    l.Tabela = tabela;
    l.CheieLegacy = cheie;
    l.TintaId = tinta;
}

// Tăierea segmentelor terminale până la un simbol din plan (aceeași regulă ca
// ContImplicit din seed): analiticele legacy sub sintetic (302.02.00.1) cad pe
// contul sintetic; detalierea rămâne în Clasă/Tip și dimensiuni.
string TrimCont(string simbol, IReadOnlyDictionary<string, Guid> plan) {
    var s = simbol?.Trim() ?? "";
    while (s.Length > 0 && !plan.ContainsKey(s))
        s = s.Contains('.') ? s[..s.LastIndexOf('.')] : "";
    return s.Length > 0 ? s : null;
}

// ==================== Nomenclatoare: dimensiuni + perioade ====================

using (var os = provider.CreateObjectSpace()) {
    var coduriF = legacy.CoduriFunctionale().ToDictionary(c => c.Cod, c => c.Denumire);
    foreach (var cod in solduriLegacy.Select(s => s.CodFunctional).Concat(stocLegacy.Select(s => s.CodFunctional)))
        if (cod != null)
            coduriF.TryAdd(cod, null);
    var existenteF = os.GetObjectsQuery<CodFunctional>().Select(c => c.Cod).ToHashSet();
    foreach (var (cod, den) in coduriF)
        if (!existenteF.Contains(cod)) {
            var c = os.CreateObject<CodFunctional>();
            c.Cod = cod;
            c.Denumire = den ?? cod;
        }

    var coduriE = legacy.CoduriEconomice().ToDictionary(c => c.Cod, c => c.Denumire);
    foreach (var cod in solduriLegacy.Select(s => s.CodEconomic).Concat(stocLegacy.Select(s => s.CodEconomic)))
        if (cod != null)
            coduriE.TryAdd(cod, null);
    var existenteE = os.GetObjectsQuery<CodEconomic>().Select(c => c.Cod).ToHashSet();
    foreach (var (cod, den) in coduriE)
        if (!existenteE.Contains(cod)) {
            var c = os.CreateObject<CodEconomic>();
            c.Cod = cod;
            c.Denumire = den ?? cod;
        }

    var unitati = Legaturi(os, "OI_UNITATI");
    var deLegatU = new List<(string Cheie, Unitate Entitate)>();
    foreach (var u in legacy.Unitati())
        if (!unitati.ContainsKey(u.Id.ToString())) {
            var e = os.CreateObject<Unitate>();
            e.Cod = u.Id.ToString();
            e.Denumire = u.Denumire;
            deLegatU.Add((u.Id.ToString(), e));
        }
    var proiecte = Legaturi(os, "OI_PROIECTE");
    var deLegatP = new List<(string Cheie, Proiect Entitate)>();
    foreach (var p in legacy.Proiecte())
        if (!proiecte.ContainsKey(p.Id.ToString())) {
            var e = os.CreateObject<Proiect>();
            e.Cod = p.Id.ToString();
            e.Denumire = p.Denumire;
            deLegatP.Add((p.Id.ToString(), e));
        }

    // Starea perioadelor se aliniază din legacy (pivotul gardienilor, decizia 14).
    var perioade = os.GetObjectsQuery<PerioadaFiscala>().ToList();
    foreach (var p in legacy.Perioade()) {
        var existenta = perioade.FirstOrDefault(x => x.An == p.An && x.Luna == p.Luna);
        if (existenta == null) {
            existenta = os.CreateObject<PerioadaFiscala>();
            existenta.An = p.An;
            existenta.Luna = p.Luna;
        }
        existenta.Inchisa = p.Inchisa;
    }

    os.CommitChanges();
    foreach (var (cheie, e) in deLegatU)
        Leaga(os, "OI_UNITATI", cheie, e.ID);
    foreach (var (cheie, e) in deLegatP)
        Leaga(os, "OI_PROIECTE", cheie, e.ID);
    os.CommitChanges();
    Console.WriteLine($"Nomenclatoare: {coduriF.Count} coduri funcționale, {coduriE.Count} coduri economice.");
}

// ============================ Repartitori ============================
// Clasificările m2m legacy (inventar 10 §1) → clasa TPT (decizia 16):
// identitățile exclusive devin clase, calitățile transversale devin flags.
// Purtătorii de stoc de deschidere sunt FORȚAȚI Gestiune (lotul cere gestiune),
// indiferent de clasificarea legacy — semnalul datelor bate eticheta.

using (var os = provider.CreateObjectSpace()) {
    var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
    var legaturi = Legaturi(os, "REPARTITORI");
    var stocHolders = stocLegacy.Select(s => s.PredatorId).ToHashSet();

    var deLegat = new List<(string Cheie, Repartitor Entitate)>();
    foreach (var r in repartitoriLegacy) {
        var cheie = r.Id.ToString();
        // 7=Gestiuni, 18=Gestiuni imobilizări; 3=Salariați; 11=Unități, 19=Departament;
        // restul clasificărilor de identitate (1/2/4/5/10/22) = Partener.
        var tinta =
            stocHolders.Contains(r.Id) || r.Tipuri.Contains(7) || r.Tipuri.Contains(18) ? typeof(Gestiune)
            : r.Tipuri.Contains(3) ? typeof(Angajat)
            : r.Tipuri.Contains(11) || r.Tipuri.Contains(19) ? typeof(UnitateInterna)
            : r.Tipuri.Count > 0 || !r.GestInt ? typeof(Partener)
            : typeof(UnitateInterna);

        Repartitor entitate;
        if (legaturi.TryGetValue(cheie, out var id)) {
            entitate = os.GetObjectByKey<Repartitor>(id);
            if (entitate == null) {
                Avert($"Repartitor legacy {r.Id} are legătură orfană — recreat.");
                os.Delete(os.FirstOrDefault<MigrareLegatura>(m => m.Tabela == "REPARTITORI" && m.CheieLegacy == cheie));
                legaturi.Remove(cheie);
            }
            else if (entitate.GetType().BaseType != tinta && entitate.GetType() != tinta)
                Avert($"Repartitor legacy {r.Id} ({r.Nume}) e {entitate.GetType().Name} în nou, "
                    + $"clasificarea ar cere {tinta.Name} — rămâne cum e.");
        }
        if (!legaturi.ContainsKey(cheie)) {
            entitate = (Repartitor)os.CreateObject(tinta);
            entitate.Cod = cheie;
            deLegat.Add((cheie, entitate));
        }
        else
            entitate = os.GetObjectByKey<Repartitor>(legaturi[cheie]);

        entitate.Denumire = r.Nume;
        // Calități transversale: 20=Gestionar, 21=Comisie, 19=Departament,
        // 8=Centre de cost, 9=Locuri de consum.
        var calitati = CalitateRepartitor.Niciuna;
        if (r.Tipuri.Contains(20)) calitati |= CalitateRepartitor.Gestionar;
        if (r.Tipuri.Contains(21)) calitati |= CalitateRepartitor.Comisie;
        if (r.Tipuri.Contains(19)) calitati |= CalitateRepartitor.Departament;
        if (r.Tipuri.Contains(8)) calitati |= CalitateRepartitor.CentruCost;
        if (r.Tipuri.Contains(9)) calitati |= CalitateRepartitor.LocConsum;
        entitate.Calitati |= calitati;
        var cont = TrimCont(r.Cont, plan);
        if (cont != null)
            entitate.ContImplicitId = plan[cont];
        if (entitate is Partener partener) {
            partener.CodFiscal = r.CodFiscal;
            partener.RegistruComert = r.RegComert;
        }
    }
    os.CommitChanges();
    foreach (var (cheie, e) in deLegat)
        Leaga(os, "REPARTITORI", cheie, e.ID);
    os.CommitChanges();
    Console.WriteLine($"Repartitori: {repartitoriLegacy.Count} procesați ({deLegat.Count} noi).");

    // Conturile proprii (legacy `casierie`, fără legătură de repartitor în 2026):
    // rând nou per casă/cont de trezorerie; denumirea-IBAN devine Iban.
    var legaturiCas = Legaturi(os, "CASIERIE");
    var deLegatCas = new List<(string Cheie, ContPropriu Entitate)>();
    foreach (var c in casierieLegacy) {
        var cheie = c.Id.ToString();
        ContPropriu entitate;
        if (legaturiCas.TryGetValue(cheie, out var id))
            entitate = os.GetObjectByKey<ContPropriu>(id);
        else {
            entitate = os.CreateObject<ContPropriu>();
            entitate.Cod = $"CP{c.Id}";
            deLegatCas.Add((cheie, entitate));
        }
        entitate.Denumire = c.Denumire;
        var esteIban = c.Denumire?.StartsWith("RO") == true && c.Denumire.Length == 24;
        entitate.Iban = esteIban ? c.Denumire : null;
        entitate.EsteBanca = c.EsteBanca || esteIban;
        var cont = TrimCont(c.ContCorespondent, plan);
        if (cont != null)
            entitate.ContImplicitId = plan[cont];
        else
            Avert($"Casierie {c.Id} ({c.Denumire}): cont corespondent „{c.ContCorespondent}” nerezolvabil în plan.");
    }
    os.CommitChanges();
    foreach (var (cheie, e) in deLegatCas)
        Leaga(os, "CASIERIE", cheie, e.ID);
    os.CommitChanges();
    Console.WriteLine($"Conturi proprii: {casierieLegacy.Count} procesate ({deLegatCas.Count} noi).");
}

// ============================ Produse + Loturi ============================
// Doar cele referite de stocul de deschidere (inventar 10 §3 — catalogul legacy
// are și intrări moarte). Lotul păstrează identitatea codmat (decizia 13).

using (var os = provider.CreateObjectSpace()) {
    var tipuriNoi = os.GetObjectsQuery<TipMaterial>()
        .Select(t => new { t.ID, t.Cod, ClasaCod = t.Clasa.Cod })
        .ToDictionary(t => t.Cod, t => (t.ID, t.ClasaCod));
    Guid? TipNou(int? tipLegacy) =>
        tipLegacy != null && tipCont.TryGetValue(tipLegacy.Value, out var cont)
            && tipuriNoi.TryGetValue(cont, out var tip) ? tip.ID : null;

    var repartitori = Legaturi(os, "REPARTITORI");
    var legaturiProd = Legaturi(os, "GEST_SUMATOR");
    var legaturiLot = Legaturi(os, "GEST_GNMCL");

    var codmats = stocLegacy.Select(s => s.Codmat).Distinct().ToList();
    var deLegatProd = new List<(string Cheie, Produs Entitate)>();
    var produseNoi = new Dictionary<string, Produs>();

    foreach (var codmat in codmats) {
        if (!loturiLegacy.TryGetValue(codmat, out var lot)) {
            Avert($"Codmat {codmat} din stocul de deschidere lipsește din gest_gnmcl.");
            continue;
        }
        // Produsul: sumatorul legacy; loturile fără sumator primesc produs
        // surogat 1:1 (cheiat pe codmat) — catalogul se curăță ulterior din UI.
        var cheieProd = lot.SumatorId != null ? lot.SumatorId.ToString() : $"GNMCL:{codmat}";
        if (!legaturiProd.ContainsKey(cheieProd) && !produseNoi.ContainsKey(cheieProd)) {
            var sumator = lot.SumatorId != null ? produseLegacy.GetValueOrDefault(lot.SumatorId.Value) : null;
            var p = os.CreateObject<Produs>();
            p.Cod = cheieProd;
            p.Denumire = sumator?.Denumire ?? lot.Denumire ?? $"Codmat {codmat}";
            p.UM = sumator?.UM ?? lot.UM;
            p.TipMaterialId = TipNou(lot.TipMaterialId ?? sumator?.TipMaterialId);
            deLegatProd.Add((cheieProd, p));
            produseNoi[cheieProd] = p;
        }
    }
    os.CommitChanges();
    foreach (var (cheie, e) in deLegatProd) {
        Leaga(os, "GEST_SUMATOR", cheie, e.ID);
        legaturiProd[cheie] = e.ID;
    }
    os.CommitChanges();

    var deLegatLot = new List<(string Cheie, Lot Entitate)>();
    foreach (var grup in stocLegacy.GroupBy(s => s.Codmat)) {
        var cheie = grup.Key.ToString();
        if (legaturiLot.ContainsKey(cheie) || !loturiLegacy.TryGetValue(grup.Key, out var lot))
            continue;
        var cheieProd = lot.SumatorId != null ? lot.SumatorId.ToString() : $"GNMCL:{grup.Key}";
        var primul = grup.First();
        if (!repartitori.TryGetValue(primul.PredatorId.ToString(), out var gestiuneId)) {
            Avert($"Codmat {grup.Key}: predatorul legacy {primul.PredatorId} nu are corespondent — lot sărit.");
            continue;
        }
        var l = os.CreateObject<Lot>();
        l.ProdusId = legaturiProd[cheieProd];
        l.GestiuneId = gestiuneId;
        l.PretUnitar = grup.Select(g => g.PretUnitar).FirstOrDefault(p => p != 0);
        l.Data = DateOnly.FromDateTime(lot.DataCod ?? dataDeschidere);
        l.DataExpirare = lot.DataExpirare != null ? DateOnly.FromDateTime(lot.DataExpirare.Value) : null;
        l.LotFabricatie = lot.LotFabricatie;
        deLegatLot.Add((cheie, l));
    }
    os.CommitChanges();
    foreach (var (cheie, e) in deLegatLot)
        Leaga(os, "GEST_GNMCL", cheie, e.ID);
    os.CommitChanges();
    Console.WriteLine($"Produse: {deLegatProd.Count} noi; loturi: {deLegatLot.Count} noi (din {codmats.Count} codmat).");
}

// ======================= Rândurile de deschidere =======================
// Se rescriu integral la fiecare rulare (singurele rânduri cu DocumentId=null —
// decizia 25e — deci exclusiv proprietatea migrării). Solduri: cont contra
// 891.01.00 „Bilanț de deschidere”, dimensiunile pe latura contului. Clasa 8
// (evidența angajamentelor/creditelor bugetare) NU se migrează — aparține
// modulului de angajamente (deciziile 9/22c).

var clasa8 = solduriLegacy.Where(s => s.Cont.StartsWith('8')).ToList();
var solduriBilant = solduriLegacy.Where(s => !s.Cont.StartsWith('8')).ToList();

using (var os = provider.CreateObjectSpace()) {
    os.Delete(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == null).ToList());
    os.Delete(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == null).ToList());
    os.CommitChanges();

    var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
    var cont891 = plan.TryGetValue("891.01.00", out var id891) ? id891
        : throw new InvalidOperationException("Planul nu conține 891.01.00 (bilanț de deschidere).");
    var repartitori = Legaturi(os, "REPARTITORI");
    var unitati = Legaturi(os, "OI_UNITATI");
    var proiecte = Legaturi(os, "OI_PROIECTE");
    var coduriF = os.GetObjectsQuery<CodFunctional>().ToDictionary(c => c.Cod, c => c.ID);
    var coduriE = os.GetObjectsQuery<CodEconomic>().ToDictionary(c => c.Cod, c => c.ID);
    var data = DateOnly.FromDateTime(dataDeschidere);

    Dimensiuni Dims(LegacySold s) => new() {
        RepartitorId = s.RepartitorId != null ? repartitori.GetValueOrDefault(s.RepartitorId.Value.ToString()) : null,
        CodFunctionalId = s.CodFunctional != null ? coduriF.GetValueOrDefault(s.CodFunctional) : null,
        CodEconomicId = s.CodEconomic != null ? coduriE.GetValueOrDefault(s.CodEconomic) : null,
        UnitateId = s.UnitateId != null ? unitati.GetValueOrDefault(s.UnitateId.Value.ToString()) : null,
        ProiectId = s.ProiectId != null ? proiecte.GetValueOrDefault(s.ProiectId.Value.ToString()) : null,
    };

    var randuri = 0;
    foreach (var sold in solduriBilant) {
        var simbol = TrimCont(sold.Cont, plan);
        if (simbol == null) {
            Avert($"Sold pe „{sold.Cont}” nerezolvabil în plan — rând sărit.");
            esecuri++;
            continue;
        }
        // Debitor: cont = 891; creditor: 891 = cont (un rând per sens; câteva
        // rânduri legacy au ambele sensuri pe aceeași cheie).
        if (sold.Debitor != 0) {
            var r = os.CreateObject<RegistruContabil>();
            r.Data = data;
            r.NumarNota = "DESCHIDERE";
            r.ContDebitId = plan[simbol];
            r.ContCreditId = cont891;
            r.Valoare = sold.Debitor;
            r.DimensiuniDebit = Dims(sold);
            randuri++;
        }
        if (sold.Creditor != 0) {
            var r = os.CreateObject<RegistruContabil>();
            r.Data = data;
            r.NumarNota = "DESCHIDERE";
            r.ContDebitId = cont891;
            r.ContCreditId = plan[simbol];
            r.Valoare = sold.Creditor;
            r.DimensiuniCredit = Dims(sold);
            randuri++;
        }
    }
    os.CommitChanges();
    Console.WriteLine($"Registru contabil: {randuri} rânduri de deschidere "
        + $"(clasa 8 amânată la modulul angajamente: {clasa8.Count} rânduri, "
        + $"{clasa8.Sum(s => s.Debitor - s.Creditor):N2}).");

    // Stocul: un rând per linie LDI (Lot × gestiune), tipul de stoc din clasa
    // Tipului — aceeași mapare ca politicile NIR/LDI (decizia 26c/28a).
    var tipuriNoi = os.GetObjectsQuery<TipMaterial>()
        .Select(t => new { t.Cod, ClasaCod = t.Clasa.Cod })
        .ToDictionary(t => t.Cod, t => t.ClasaCod);
    TipStoc TipStocDin(int? tipLegacy) {
        var clasa = tipLegacy != null && tipCont.TryGetValue(tipLegacy.Value, out var cont)
            ? tipuriNoi.GetValueOrDefault(cont) : null;
        return clasa switch {
            "G" => TipStoc.Gratuit,
            "OF" => TipStoc.Folosinta,
            "MF" => TipStoc.Marfuri,
            "MC" => TipStoc.Custodie,
            _ => TipStoc.Magazie,
        };
    }
    var loturi = Legaturi(os, "GEST_GNMCL");
    var randuriStoc = 0;
    foreach (var item in stocLegacy) {
        if (!loturi.TryGetValue(item.Codmat.ToString(), out var lotId)
            || !repartitori.TryGetValue(item.PredatorId.ToString(), out var repId)) {
            esecuri++;
            continue; // avertizat deja la faza de loturi
        }
        var r = os.CreateObject<RegistruStoc>();
        r.Data = data;
        r.TipStoc = TipStocDin(item.TipMaterialId);
        r.LotId = lotId;
        r.RepartitorId = repId;
        r.Cantitate = item.Cantitate;
        r.Valoare = item.Valoare;
        randuriStoc++;
    }
    os.CommitChanges();
    Console.WriteLine($"Registru stoc: {randuriStoc} rânduri de deschidere.");
}

// ============================ Reconcilierea ============================
// Contractul pasului 4: soldurile de deschidere din NOU (citite înapoi din
// Postgres) = soldurile de închidere legacy, per cont sintetic.

using (var os = provider.CreateObjectSpace()) {
    var plan = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);

    var deschidere = os.GetObjectsQuery<RegistruContabil>()
        .Where(r => r.DocumentId == null)
        .Select(r => new { Debit = r.ContDebit.Simbol, Credit = r.ContCredit.Simbol, r.Valoare })
        .ToList();
    var totalDebit = deschidere.Sum(r => r.Valoare);
    var solduriNoi = deschidere.GroupBy(r => r.Debit).ToDictionary(g => g.Key, g => g.Sum(r => r.Valoare));
    foreach (var grup in deschidere.GroupBy(r => r.Credit))
        solduriNoi[grup.Key] = solduriNoi.GetValueOrDefault(grup.Key) - grup.Sum(r => r.Valoare);

    Console.WriteLine($"Rulaj deschidere: {totalDebit:N2} (Σ debit = Σ credit prin construcție — un rând poartă ambele laturi).");
    Check($"891.01.00 se închide la zero (bilanțul legacy e echilibrat): {solduriNoi.GetValueOrDefault("891.01.00"):N2}",
        Math.Abs(solduriNoi.GetValueOrDefault("891.01.00")) < 0.005m);

    var solduriVechi = solduriBilant
        .GroupBy(s => TrimCont(s.Cont, plan) ?? s.Cont)
        .ToDictionary(g => g.Key, g => g.Sum(s => s.Debitor - s.Creditor));
    var diferente = solduriVechi
        .Select(v => (Cont: v.Key, Vechi: v.Value, Nou: solduriNoi.GetValueOrDefault(v.Key)))
        .Where(x => Math.Abs(x.Vechi - x.Nou) >= 0.005m)
        .ToList();
    Check($"Solduri per cont sintetic: {solduriVechi.Count} conturi legacy, diferențe: {diferente.Count}",
        diferente.Count == 0);
    foreach (var (cont, vechi, nou) in diferente)
        Console.WriteLine($"     {cont}: legacy {vechi:N2} vs nou {nou:N2}");

    // Stoc: rândurile noi trebuie să egaleze LDI-ul legacy per cont de stoc…
    var stocNou = os.GetObjectsQuery<RegistruStoc>()
        .Where(r => r.DocumentId == null)
        .Select(r => new { Tip = r.Lot.Produs.TipMaterial.Cod, r.Valoare })
        .ToList()
        .GroupBy(r => TrimCont(r.Tip, plan) ?? r.Tip ?? "?")
        .ToDictionary(g => g.Key, g => g.Sum(r => r.Valoare));
    var stocVechi = stocLegacy
        .GroupBy(s => TrimCont(s.TipMaterialId != null ? tipCont.GetValueOrDefault(s.TipMaterialId.Value) : null, plan) ?? "?")
        .ToDictionary(g => g.Key, g => g.Sum(s => s.Valoare));
    var difStoc = stocVechi.Keys.Union(stocNou.Keys)
        .Where(k => Math.Abs(stocVechi.GetValueOrDefault(k) - stocNou.GetValueOrDefault(k)) >= 0.005m)
        .ToList();
    Check($"Stoc deschidere per cont: {stocVechi.Count} conturi, diferențe față de LDI legacy: {difStoc.Count}",
        difStoc.Count == 0);
    foreach (var k in difStoc)
        Console.WriteLine($"     {k}: LDI legacy {stocVechi.GetValueOrDefault(k):N2} vs nou {stocNou.GetValueOrDefault(k):N2}");

    // …iar nepotrivirea stoc↔contabilitate e a LEGACY-ului: se raportează, nu se
    // ascunde (303.x fără detaliu de stoc în baza sursă, combustibil etc.).
    Console.WriteLine("Stoc vs sold contabil per cont (diferențele sunt ale legacy-ului, informativ):");
    foreach (var cont in stocNou.Keys.Union(solduriVechi.Keys.Where(k => k.StartsWith('3'))).OrderBy(k => k)) {
        var stoc = stocNou.GetValueOrDefault(cont);
        var sold = solduriNoi.GetValueOrDefault(cont);
        Console.WriteLine($"     {cont}: stoc {stoc,15:N2}   sold {sold,15:N2}   {(Math.Abs(stoc - sold) < 0.005m ? "=" : $"Δ {sold - stoc:N2}")}");
    }
}

foreach (var a in avertismente)
    Console.WriteLine($"AVERT {a}");
Console.WriteLine(esecuri == 0
    ? "\nMigrare încheiată fără eșecuri."
    : $"\nMigrare încheiată cu {esecuri} eșecuri.");
return esecuri == 0 ? 0 : 1;
