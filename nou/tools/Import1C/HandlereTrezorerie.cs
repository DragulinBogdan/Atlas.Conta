using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 5 al feliei 1C-c, partea I: TREZORERIA — extrasul de cont, plățile și
// încasările de casă, încasările pe card. Patru surse 1C, un singur motor.
//
// **De ce un singur motor**: verificat pe date (25.07.2026), toate patru postează
// aceeași formă — un rând de registru cu contul propriu pe o latură, contul de
// decontare pe cealaltă, iar partenerul și documentul stins ca subconto pe latura
// contului de decontare. Diferă doar de unde vine antetul (view sau tabelă
// generică) și care e contul propriu (bancă din antet, casierie din antet, ori
// 5125 fix la card).
//
// **De ce din RÂNDURILE DE NOTĂ și nu din secțiuni** (aceeași alegere ca la
// BTR/BCS/DSC, pasul 3): secțiunea de extras e AGREGATĂ — un rând de 136.048,31
// către un furnizor se sparge în 30 de rânduri de registru, unul per factură
// stinsă, iar suma lor diferă de secțiune prin rotunjire (verificat: 136.048,30).
// Registrul e cel din care se ridică Balanța, deci el e sursa: sumele ies exacte,
// iar documentul stins vine odată cu rândul, nu printr-o potrivire pozițională.
//
// **Granularitatea**: un `Plata`/`Incasare` Atlas per (document × direcție × cont
// de decontare × contrapartidă), nu per rând de registru — altfel anul ar produce
// ~54.000 de documente de trezorerie în loc de ~30.000, fără să câștige nimic:
// imperecherea e la nivel de document (31f), iar rândurile grupului sting fiecare
// documentul lui, prin `Imperechere` separate.
//
// **Ce NU devine PLT/INC** (decizia arhitectului, pasul 5): tot restul —
// impozitele și contribuțiile, comisioanele bancare, diferențele de curs,
// măturarea tamponului de card (512.1 = 512.5), transferurile interne (581),
// rândurile de decontare fără partener. Toate se transcriu prin puntea NTC a
// documentului. Nu inventăm parteneri-taxă cu cont implicit ca să forțăm forma
// tipizată: contul e al sursei, iar Atlas n-are (încă) mecanism pentru el.

// Antetul unei surse de trezorerie, normalizat: identitatea 1C + contul propriu
// Atlas pe care stau banii + simbolul lui de evidență (latura pe care o postează
// motorul prin `SursaCont.Repartitor*`).
sealed record AntetTrezorerie(string Id, string Numar, DateOnly Data, Guid ContPropriuId,
    string SimbolTrezorerie, TipInstrumentPlata Instrument, string NumarExtras);

// Un rând de registru care POATE deveni PLT/INC: direcția, contul de decontare,
// contrapartida 1C (parteneri sau persoane fizice) și documentul pe care îl stinge.
sealed record RandDecontare(FlaxRandNota Rand, bool Incasare, string Cor,
    string ContrapartidaHex, string ContrapartidaTip, FlaxRef DocStins);

// Grupul care devine un document Atlas. `Cheie` e derivată DIN SURSĂ, fără nicio
// atingere de bază: de ea atârnă și gardul de reluare (a doua rulare trebuie să
// știe ce chei ar fi trebuit să existe înainte să planifice ceva), și trecerea 2
// a imperecherilor, care recalculează exact aceleași chei.
sealed class GrupTrezorerie {
    public string Cheie;
    // Poziția în analiză (1, 2, …) — intră în numărul documentului generat și se
    // fixează ÎNAINTE de orice respingere, ca numărul să nu depindă de rulare.
    public int Index;
    public bool Incasare;
    public string Cor;
    public string ContrapartidaHex;
    public string ContrapartidaTip;
    public decimal Suma;
    public List<(FlaxRef Tinta, decimal Suma)> Stingeri = [];
}

sealed record AnalizaTrezorerie(List<GrupTrezorerie> Grupuri,
    List<(FlaxRandNota Rand, string Eticheta)> Punte);

// O stingere derivată din sursă: cine stinge (cheia documentului Atlas) și ce
// stinge (referința 1C a documentului). Trecerea 2 (Imperecheri.cs) o traduce în
// `Imperechere`; trecerea 1 nu face nimic cu ea.
sealed record StingereSursa(string View, string CheieStingator, FlaxRef Tinta, decimal Suma);

static class MotorTrezorerie {
    // Conturile pe care Atlas le poate reprezenta ca LATURĂ de document, adică
    // cele a căror contrapartidă e un repartitor (furnizor, client, angajat,
    // debitor/creditor divers). Restul rămâne cont, deci notă.
    //
    // Consecință asumată și vizibilă în punte: motorul postează contul IMPLICIT
    // al contrapartidei (partenerii importați n-au unul — 47b), deci 401 la plată
    // și 4111 la încasare. Un rând pe 404/419/473 iese ca diferență de FORMĂ
    // (404 = 401) în nota-punte, exact ca la restul importului — valoarea rămâne
    // neatinsă, soldurile se reconciliază.
    static readonly string[] PrefixeDecontare =
        ["401", "403", "404", "405", "409", "411", "413", "419", "461", "462", "473"];

    static bool EsteDecontare(string simbol) =>
        simbol != null && PrefixeDecontare.Any(simbol.StartsWith);

    // ---- Contoare de raport (toate sursele la un loc + defalcare pe categorii) ----
    public static int Documente { get; private set; }
    public static int Plati { get; private set; }
    public static int Incasari { get; private set; }
    public static int RanduriPunte { get; private set; }
    public static int ContrapartideNerezolvate { get; private set; }
    public static int AnteteFaraRanduri { get; private set; }
    static readonly Dictionary<string, int> peSursa = new(StringComparer.Ordinal);
    static readonly Dictionary<string, int> peCategorie = new(StringComparer.Ordinal);

    // ======================= Analiza (fără bază de date) =======================

    public static AnalizaTrezorerie Analizeaza(Catalog cat, string docId,
            IReadOnlyList<FlaxRandNota> randuri,
            Dictionary<(int Linie, int Latura), List<FlaxSubcontoNota>> subconto,
            string simbolTrezorerie) {
        var grupuri = new List<GrupTrezorerie>();
        var punte = new List<(FlaxRandNota, string)>();

        foreach (var r in randuri) {
            var debit = cat.Mapeaza(r.ContDebit);
            var credit = cat.Mapeaza(r.ContCredit);
            // Direcția o dă latura pe care stă contul propriu: banii intră când e
            // pe debit, ies când e pe credit. Un rând care nu-l atinge deloc (sau
            // îl atinge pe ambele laturi) nu e mișcare de trezorerie a acestui
            // cont — e altceva, contabilizat pe același document.
            var trezorerieDebit = debit == simbolTrezorerie;
            var trezorerieCredit = credit == simbolTrezorerie;
            if (trezorerieDebit == trezorerieCredit) {
                punte.Add((r, "TRZ: rând fără latura contului propriu"));
                continue;
            }
            var incasare = trezorerieDebit;
            var cor = incasare ? credit : debit;
            if (!EsteDecontare(cor)) {
                punte.Add((r, "TRZ: rând pe cont ne-decontare (taxe, comisioane, transferuri)"));
                continue;
            }
            var latura = incasare ? Subconto.Credit : Subconto.Debit;
            var analitice = subconto.Latura(r.Linie, latura);
            var contrapartida = analitice.DeTip(Subconto.TipPartener, Subconto.TipPersoana);
            if (contrapartida == null) {
                punte.Add((r, "TRZ: rând de decontare fără partener în subconto"));
                continue;
            }
            var rand = new RandDecontare(r, incasare, cor, contrapartida.Id, contrapartida.Tip,
                analitice.DeFel(Subconto.FelDocumente));
            var cheie = Cheie(docId, incasare, cor, contrapartida.Id);
            var grup = grupuri.FirstOrDefault(g => g.Cheie == cheie);
            if (grup == null)
                grupuri.Add(grup = new GrupTrezorerie {
                    Cheie = cheie,
                    Index = grupuri.Count + 1,
                    Incasare = incasare,
                    Cor = cor,
                    ContrapartidaHex = contrapartida.Id,
                    ContrapartidaTip = contrapartida.Tip,
                });
            grup.Suma += rand.Rand.Suma;
            if (rand.DocStins != null)
                grup.Stingeri.Add((rand.DocStins, rand.Rand.Suma));
        }

        // Un grup cu total ≤ 0 nu se poate reprezenta ca plată/încasare (liniile
        // de trezorerie cer valoare pozitivă — 31a) și nici n-ar avea sens: e o
        // stornare netă de decontare. Rândurile lui se întorc în punte, întregi.
        foreach (var g in grupuri.Where(g => g.Suma <= 0).ToList()) {
            grupuri.Remove(g);
            foreach (var r in randuri.Where(r => RandInGrup(cat, r, subconto, simbolTrezorerie, g)))
                punte.Add((r, EtichetaNepozitiv));
        }
        return new AnalizaTrezorerie(grupuri, punte);
    }

    public const string EtichetaNepozitiv = "TRZ: grup de decontare cu total ne-pozitiv";

    // Re-testul apartenenței unui rând la un grup respins: se reface exact
    // clasificarea de mai sus (aceleași reguli), ca rândurile să ajungă în punte
    // fără să ținem o a doua listă în paralel cu grupurile.
    static bool RandInGrup(Catalog cat, FlaxRandNota r,
            Dictionary<(int, int), List<FlaxSubcontoNota>> subconto, string simbolTrezorerie,
            GrupTrezorerie grup) {
        var debit = cat.Mapeaza(r.ContDebit);
        var credit = cat.Mapeaza(r.ContCredit);
        var incasare = debit == simbolTrezorerie;
        if (incasare == (credit == simbolTrezorerie) || incasare != grup.Incasare)
            return false;
        var cor = incasare ? credit : debit;
        if (cor != grup.Cor)
            return false;
        var contrapartida = subconto.Latura(r.Linie, incasare ? Subconto.Credit : Subconto.Debit)
            .DeTip(Subconto.TipPartener, Subconto.TipPersoana);
        return contrapartida?.Id == grup.ContrapartidaHex;
    }

    public static string Cheie(string docId, bool incasare, string cor, string contrapartidaHex) =>
        $"{docId}#T{(incasare ? "I" : "P")}{cor}@{contrapartidaHex}";

    // ======================= Importul unui document sursă =======================

    public static void Importa(ContextLuna ctx, string view, AntetTrezorerie h) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
        // Antet postat care nu postează: garda pasului 3. Fără rânduri de
        // registru documentul n-a mișcat bani, deci n-avem ce importa.
        if (randuri.Count == 0) {
            AnteteFaraRanduri++;
            return;
        }
        var subconto = Subconto.IndexeazaTot(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var analiza = Analizeaza(cat, h.Id, randuri, subconto, h.SimbolTrezorerie);
        // Cheia punții se fixează AICI, din analiza brută: dacă s-ar recalcula
        // după respingerea unui grup, gardul de reluare ar căuta o cheie și
        // planificarea ar scrie alta — iar puntea s-ar rescrie la fiecare rulare.
        var cheiePunte = analiza.Grupuri.Count > 0 ? h.Id + "#punte" : h.Id;
        var cunoscut = analiza.Grupuri.All(g => bucla.EsteCunoscut(view, g.Cheie))
            && (analiza.Punte.Count == 0 || bucla.EsteCunoscut(view, cheiePunte));

        var contrapartide = new Dictionary<string, Guid>(StringComparer.Ordinal);
        if (!cunoscut) {
            var punte = new Punte();
            var respinse = new List<GrupTrezorerie>();
            foreach (var g in analiza.Grupuri) {
                var id = Contrapartida(bucla, g);
                if (id == null) {
                    // Contrapartidă nerezolvabilă (referință moartă în sursă):
                    // grupul se dizolvă în punte, ca soldul contului de decontare
                    // să rămână al sursei. Se replanifică la fiecare rulare —
                    // inofensiv (nu se scrie nimic de două ori), dar numărat.
                    ContrapartideNerezolvate++;
                    respinse.Add(g);
                    continue;
                }
                contrapartide[g.Cheie] = id.Value;
            }
            foreach (var g in respinse)
                analiza.Grupuri.Remove(g);

            ConstruiestePunte(cat, punte, randuri, analiza, h, respinse, subconto);
            Punti.Scrie(bucla, view, cheiePunte, h.Numar, h.Data, punte, bucla.ContorPunti, bucla.Avert);
            if (analiza.Grupuri.Count == 0 && !punte.AreCeva)
                bucla.NumaraSursaFaraCorespondent();
        }

        foreach (var g in analiza.Grupuri) {
            var numar = $"{h.Numar}-{g.Index}";
            var contrapartidaId = contrapartide.GetValueOrDefault(g.Cheie);
            var stare = bucla.ImportaDocument(view, g.Cheie, os => contrapartidaId == Guid.Empty
                ? null
                : Materializeaza(os, cat, h, g, contrapartidaId, numar));
            if (stare != StareImport.Importat)
                continue;
            Documente++;
            peSursa[view] = peSursa.GetValueOrDefault(view) + 1;
            if (g.Incasare)
                Incasari++;
            else
                Plati++;
        }
    }

    // Contrapartida Atlas: partener importat la cerere sau angajat legat (1C ține
    // salariații ca persoane fizice, nu ca parteneri — 47b).
    static Guid? Contrapartida(BuclaImport bucla, GrupTrezorerie g) => g.ContrapartidaTip switch {
        Subconto.TipPartener => bucla.LaCerere.AsiguraPartener(g.ContrapartidaHex),
        Subconto.TipPersoana => bucla.Catalog.Angajati.TryGetValue(g.ContrapartidaHex, out var a)
            ? a : null,
        _ => null,
    };

    // Puntea: TOATE rândurile documentului ca țintă, apoi ce postează Atlas prin
    // documentele de trezorerie. Rândurile acoperite se anulează în deltă (când
    // contul de decontare al sursei coincide cu contul implicit al contrapartidei);
    // ce rămâne — diferențele de formă și rândurile netipizabile — devine nota.
    // Etichetele se pun DOAR pe rândurile de punte: un rând acoperit declarat sub
    // o categorie ar umple raportul cu diferențe inexistente (mecanica Punte.cs).
    static void ConstruiestePunte(Catalog cat, Punte punte, IReadOnlyList<FlaxRandNota> randuri,
            AnalizaTrezorerie analiza, AntetTrezorerie h, List<GrupTrezorerie> respinse,
            Dictionary<(int, int), List<FlaxSubcontoNota>> subconto) {
        var etichete = analiza.Punte.ToDictionary(x => x.Rand.Linie, x => x.Eticheta);
        foreach (var g in respinse)
            foreach (var r in randuri.Where(r => RandInGrup(cat, r, subconto, h.SimbolTrezorerie, g)))
                etichete[r.Linie] = "TRZ: contrapartidă nerezolvabilă în sursă";
        foreach (var r in randuri) {
            var eticheta = etichete.GetValueOrDefault(r.Linie);
            if (eticheta != null) {
                RanduriPunte++;
                peCategorie[eticheta] = peCategorie.GetValueOrDefault(eticheta) + 1;
            }
            punte.Categoria(eticheta).Tinta1C(cat.Mapeaza(r.ContDebit), cat.Mapeaza(r.ContCredit), r.Suma);
        }
        foreach (var g in analiza.Grupuri)
            if (g.Incasare)
                punte.ActualAtlas(h.SimbolTrezorerie, Catalog.ContCreantaImplicit, g.Suma);
            else
                punte.ActualAtlas(Catalog.ContDatorieImplicit, h.SimbolTrezorerie, g.Suma);
    }

    static Document Materializeaza(IObjectSpace os, Catalog cat, AntetTrezorerie h,
            GrupTrezorerie g, Guid contrapartidaId, string numar) {
        if (g.Suma <= 0)
            return null;
        DocumentTrezorerie doc;
        if (g.Incasare) {
            var inc = os.CreateObject<Incasare>();
            inc.PredatorId = contrapartidaId;
            inc.PrimitorId = h.ContPropriuId;
            doc = inc;
        }
        else {
            var plt = os.CreateObject<Plata>();
            plt.PredatorId = h.ContPropriuId;
            plt.PrimitorId = contrapartidaId;
            doc = plt;
        }
        doc.Data = h.Data;
        doc.Numar = numar;
        doc.TipInstrument = h.Instrument;
        doc.NumarExtras = h.NumarExtras;
        doc.DataExtras = h.NumarExtras == null ? null : h.Data;
        var linie = os.CreateObject<DocumentDetaliu>();
        linie.Document = doc;
        linie.TipMaterialId = cat.TipTrezorerieId;
        linie.Valoare = g.Suma;
        return doc;
    }

    // ======================= Trecerea 2: stingerile derivate din sursă =======================

    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx, string view, AntetTrezorerie h) {
        var bucla = ctx.Bucla;
        var randuri = bucla.RanduriLuna.GetValueOrDefault(h.Id) ?? [];
        if (randuri.Count == 0)
            return [];
        var subconto = Subconto.IndexeazaTot(bucla.SubcontoLuna.GetValueOrDefault(h.Id) ?? []);
        var analiza = Analizeaza(bucla.Catalog, h.Id, randuri, subconto, h.SimbolTrezorerie);
        return analiza.Grupuri.SelectMany(g => g.Stingeri
            .Select(s => new StingereSursa(view, g.Cheie, s.Tinta, s.Suma)));
    }

    public static void Raporteaza() {
        if (Documente == 0 && RanduriPunte == 0)
            return;
        Console.WriteLine($"  TRZ: {Documente} documente de trezorerie ({Plati} plăți, {Incasari} încasări) "
            + $"— " + string.Join(", ", peSursa.OrderByDescending(x => x.Value).Select(x => $"{x.Key} {x.Value}"))
            + $"; {AnteteFaraRanduri} antete fără rânduri contabile (sărite), "
            + $"{ContrapartideNerezolvate} grupuri cu contrapartidă nerezolvabilă.");
        Console.WriteLine($"  TRZ: {RanduriPunte} rânduri transcrise contabil (nu devin plată/încasare):");
        foreach (var c in peCategorie.OrderByDescending(x => x.Value))
            Console.WriteLine($"    {c.Value,8} × {c.Key}");
    }
}

// ======================= 1. ExtrasDeCont → PLT/INC per rând + notă =======================
static class HandlerExtras {
    public const string View = "ExtrasDeCont";

    public static readonly HandlerTip Handler =
        new(View, "Extras de cont (plăți/încasări + notă)", ctx => {
            foreach (var h in Antete(ctx))
                MotorTrezorerie.Importa(ctx, View, h);
        });

    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx) =>
        Antete(ctx).SelectMany(h => MotorTrezorerie.Stingeri(ctx, View, h));

    static List<AntetTrezorerie> Antete(ContextLuna ctx) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var antete = new List<AntetTrezorerie>();
        foreach (var h in bucla.Flax.Extrase(ctx.An, ctx.Luna)) {
            // Contul propriu vine din ANTET (`ContBancar`), nu din simbolul
            // rândurilor: verificat pe ianuarie — toate cele 210 extrase trimit
            // la un cont bancar propriu importat (47b).
            if (!cat.ConturiProprii.TryGetValue(h.ContBancarId ?? "", out var contPropriuId)) {
                bucla.EsecPlanificare(View, h.Id, new InvalidOperationException(
                    $"Contul bancar 1C {h.ContBancarId} al extrasului nu e legat de un ContPropriu."));
                continue;
            }
            var simbol = cat.SimbolContPropriu(contPropriuId);
            if (simbol == null) {
                bucla.EsecPlanificare(View, h.Id, new InvalidOperationException(
                    "Contul bancar al extrasului n-are cont de evidență (ContImplicit)."));
                continue;
            }
            antete.Add(new AntetTrezorerie(h.Id, h.Numar, DateOnly.FromDateTime(h.Data),
                contPropriuId, simbol, TipInstrumentPlata.OrdinPlata, h.Numar));
        }
        return antete;
    }
}

// ======================= 2. Plata / Incasare (casă) =======================
// Aceeași mecanică; contul propriu e casieria antetului. ATENȚIE (verificat pe
// date): `ContBancar_ID` e completat pe TOATE documentele de casă cu o valoare
// reziduală — casieria e cea care contează, iar simbolul din `ContCasa` (531.1)
// o confirmă. De aceea casieria are prioritate, iar contul bancar rămâne doar
// rezerva documentelor care chiar n-au casierie.
static class HandlerPlataCasa {
    public const string View = "Plata";

    public static readonly HandlerTip Handler =
        new(View, "Plată de casă", ctx => Importa(ctx, View, ctx.Bucla.Flax.Plati(ctx.An, ctx.Luna)));

    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx) =>
        Antete(ctx, View, ctx.Bucla.Flax.Plati(ctx.An, ctx.Luna))
            .SelectMany(h => MotorTrezorerie.Stingeri(ctx, View, h));

    public static void Importa(ContextLuna ctx, string view, List<FlaxTrezorerie> sursa) {
        foreach (var h in Antete(ctx, view, sursa))
            MotorTrezorerie.Importa(ctx, view, h);
    }

    public static List<AntetTrezorerie> Antete(ContextLuna ctx, string view, List<FlaxTrezorerie> sursa) {
        var bucla = ctx.Bucla;
        var cat = bucla.Catalog;
        var antete = new List<AntetTrezorerie>();
        foreach (var h in sursa) {
            Guid contPropriuId;
            if (cat.ConturiProprii.TryGetValue(h.CasierieId ?? "", out var casa))
                contPropriuId = casa;
            else if (cat.ConturiProprii.TryGetValue(h.ContBancarId ?? "", out var banca))
                contPropriuId = banca;
            else {
                bucla.EsecPlanificare(view, h.Id, new InvalidOperationException(
                    $"Nici casieria 1C {h.CasierieId}, nici contul bancar {h.ContBancarId} "
                    + "nu sunt legate de un ContPropriu."));
                continue;
            }
            var simbol = cat.SimbolContPropriu(contPropriuId);
            if (simbol == null) {
                bucla.EsecPlanificare(view, h.Id, new InvalidOperationException(
                    "Contul propriu al documentului de casă n-are cont de evidență (ContImplicit)."));
                continue;
            }
            antete.Add(new AntetTrezorerie(h.Id, h.Numar, DateOnly.FromDateTime(h.Data),
                contPropriuId, simbol, TipInstrumentPlata.DispozitieCasa, null));
        }
        return antete;
    }
}

static class HandlerIncasareCasa {
    public const string View = "Incasare";

    public static readonly HandlerTip Handler =
        new(View, "Încasare de casă",
            ctx => HandlerPlataCasa.Importa(ctx, View, ctx.Bucla.Flax.Incasari(ctx.An, ctx.Luna)));

    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx) =>
        HandlerPlataCasa.Antete(ctx, View, ctx.Bucla.Flax.Incasari(ctx.An, ctx.Luna))
            .SelectMany(h => MotorTrezorerie.Stingeri(ctx, View, h));
}

// ======================= 3. IncasareCard (7633) =======================
// Singurul tip de trezorerie fără view generat: antetul se citește din structura
// generică (`AnteteRaw`), restul e identic. Postează exclusiv 512.5 = 411.1, deci
// contul propriu e „sume în curs de decontare" — același `ContPropriu` 5125 pe
// care îl folosesc încasările pe card inline de pe facturi (pasul 4).
static class HandlerCard {
    // Numele e al UNELTEI, nu al sursei (vezi `FlaxDb.TipuriFaraColoana`), dar e
    // și cheia legăturilor „1C:IncasareCard" — deci nu se schimbă după prima
    // rulare fără reimport.
    public const string View = "IncasareCard";
    public const int Tabela = 7633;

    public static readonly HandlerTip Handler =
        new(View, "Încasare pe card (tabelă generică 7633)", ctx => {
            foreach (var h in Antete(ctx))
                MotorTrezorerie.Importa(ctx, View, h);
        });

    public static IEnumerable<StingereSursa> Stingeri(ContextLuna ctx) =>
        Antete(ctx).SelectMany(h => MotorTrezorerie.Stingeri(ctx, View, h));

    static List<AntetTrezorerie> Antete(ContextLuna ctx) {
        var cat = ctx.Bucla.Catalog;
        var contPropriuId = cat.ContPropriuCard();
        var simbol = cat.SimbolContPropriu(contPropriuId);
        return ctx.Bucla.Flax.AnteteRaw(Tabela, ctx.An, ctx.Luna)
            .Select(h => new AntetTrezorerie(h.Id, h.Numar, DateOnly.FromDateTime(h.Data),
                contPropriuId, simbol, TipInstrumentPlata.OrdinPlata, null))
            .ToList();
    }
}
