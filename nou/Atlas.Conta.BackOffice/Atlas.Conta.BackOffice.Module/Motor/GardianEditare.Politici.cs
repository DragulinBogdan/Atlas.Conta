using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

public sealed partial class GardianEditare {
    // ═══ F23-D5 — invarianții politicilor deschise pe OData ═══

    // `TipDocument` e ANCORA (decizia 20): oglindește clasele 1:1, iar `Cod` și
    // `ClrType` sunt identitatea prin care motorul își găsește tipul
    // (`GasesteTipDocument`) și prin care politicile îl referă. Un rând NOU n-are
    // clasă în spate, deci ar fi o ancoră spre nimic; o schimbare de `Cod` sau
    // `ClrType` ar rupe tăcut rezoluția de tip a întregului motor. Ce RĂMÂNE
    // editabil e exact ce e politică: `Denumire` și `TipTvaImplicit`.
    static void VerificaTipDocument(IObjectSpace os, TipDocument tip, ICollection<string> erori) {
        if (EsteSters(os, tip)) {
            erori.Add($"Tipul de document {tip.Cod ?? tip.Denumire} e ancora clasei de document "
                + "(decizia 20) — nu se șterge; clasele de document sunt cod, nu date.");
            return;
        }
        if (os.IsNewObject(tip)) {
            erori.Add("Tipurile de document nu se creează din date — ancora oglindește clasele "
                + "de document 1:1 (decizia 20), iar un rând fără clasă în spate n-ar avea ce "
                + "documenta. Editați rândul existent.");
            return;
        }
        var originale = Originale(os, tip);
        if (originale != null) {
            if (!string.Equals(originale[nameof(TipDocument.Cod)] as string, tip.Cod, StringComparison.Ordinal))
                erori.Add($"Codul tipului de document ({originale[nameof(TipDocument.Cod)]}) e identitatea "
                    + "lui — politicile îl referă prin el, iar schimbarea l-ar rupe tăcut.");
            if (!string.Equals(originale[nameof(TipDocument.ClrType)] as string, tip.ClrType, StringComparison.Ordinal))
                erori.Add($"Clasa CLR a tipului de document {tip.Cod} ({originale[nameof(TipDocument.ClrType)]}) "
                    + "e legătura cu codul — o scrie release-ul, nu culegerea.");
        }
        VerificaTipTvaActiv(os, tip.TipTvaImplicitId ?? tip.TipTvaImplicit?.ID,
            $"ancora tipului de document {tip.Cod}", erori);
    }

    // `TipTva`: cota e procent, iar `Activ = false` pe un tip REFERIT ca implicit
    // ar fi un gard care tace (62f) — rezolvarea implicitelor sare treapta cu un
    // motiv pe care nimeni nu-l citește, iar operatorul ar culege altceva decât
    // spune configurația. Refuzul vine cu LISTA referințelor: „scoate-l de aici,
    // apoi dezactivează-l" e o instrucțiune, „nu se poate" nu e.
    static void VerificaTipTva(IObjectSpace os, TipTva tip, ICollection<string> erori) {
        if (EsteSters(os, tip)) {
            // Ștergerea e mai distructivă decât dezactivarea, deci nu poate fi mai
            // permisivă (62f): rândurile care îl referă rămân fără țintă, iar
            // seed-ul următor cade pe „lipsește din bază tipul de TVA".
            var id = tip.ID;
            var folosit = ReferinteImplicite(os, id);
            if (os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.TipTvaId == id))
                folosit.Add("linii de document");
            if (Cub.Citiri.Fiscale.EsteReferit(os, id))
                folosit.Add("rânduri din jurnalul de TVA");
            if (folosit.Count > 0)
                erori.Add($"Tipul de TVA „{tip.Cod ?? tip.Denumire}” nu se poate șterge: e referit de "
                    + $"{string.Join("; ", folosit)} — dezactivați-l în loc.");
            return;
        }
        if (tip.ValabilDeLa is { } deLa && tip.ValabilPanaLa is { } panaLa && deLa > panaLa)
            erori.Add("TVA_INTERVAL_INVALID: data de început a intervalului depășește data de sfârșit.");
        if (tip.Cota is < 0m or > 100m)
            erori.Add($"Cota tipului de TVA {tip.Cod ?? tip.Denumire} e {tip.Cota} — cota e un procent "
                + "între 0 și 100.");
        if (tip.Activ)
            return;
        // Doar TRANZIȚIA spre inactiv se verifică: un tip care era deja inactiv
        // rămâne editabil (altfel n-ai mai putea nici să-i corectezi denumirea).
        var eraActiv = os.IsNewObject(tip)
            || (Originale(os, tip)?[nameof(TipTva.Activ)] as bool?) == true;
        if (!eraActiv)
            return;
        var referinte = ReferinteImplicite(os, tip.ID);
        if (referinte.Count > 0)
            erori.Add($"Tipul de TVA {tip.Cod ?? tip.Denumire} nu poate fi dezactivat cât timp e implicit "
                + $"în: {string.Join("; ", referinte)}. Un implicit care țintește un tip inactiv ar fi "
                + "sărit tăcut la culegere. Schimbați întâi referințele.");
    }

    // Cele patru locuri în care un `TipTva` poate fi IMPLICIT (F23-D1/D2).
    // Interogări pe FK, fără navigații (25b); etichetele sunt lizibile fiindcă
    // mesajul trebuie să spună UNDE, nu doar CÂT.
    static List<string> ReferinteImplicite(IObjectSpace os, Guid tipTvaId) {
        var referinte = new List<string>();
        referinte.AddRange(os.GetObjectsQuery<TipDocument>()
            .Where(t => t.TipTvaImplicitId == tipTvaId).Select(t => t.Cod).ToList()
            .Select(c => $"ancora tipului de document {c}"));
        referinte.AddRange(os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(p => p.TipTvaId == tipTvaId)
            .Select(p => new { p.TipDocument.Cod, p.ClasaFiscala }).ToList()
            .Select(p => $"politica {p.Cod} × {p.ClasaFiscala?.ToString() ?? "orice clasă"}"));
        referinte.AddRange(os.GetObjectsQuery<Partener>()
            .Where(p => p.TipTvaImplicitId == tipTvaId).Select(p => p.Cod).Take(10).ToList()
            .Select(c => $"partenerul {c}"));
        referinte.AddRange(os.GetObjectsQuery<Produs>()
            .Where(p => p.TipTvaImplicitId == tipTvaId).Select(p => p.Cod).Take(10).ToList()
            .Select(c => $"produsul {c}"));
        return referinte;
    }

    // Un implicit nu poate ținti un tip inactiv — regula geamănă a celei de mai
    // sus, pe cealaltă direcție (acolo se dezactivează ținta, aici se scrie
    // referința). Fără ea, gardul ar fi ocolibil în doi pași.
    static void VerificaTipTvaActiv(IObjectSpace os, Guid? tipTvaId, string unde, ICollection<string> erori) {
        if (tipTvaId is not Guid id || id == Guid.Empty)
            return;
        var tip = os.GetObjectsQuery<TipTva>().Where(t => t.ID == id)
            .Select(t => new { t.Cod, t.Activ }).FirstOrDefault();
        if (tip != null && !tip.Activ)
            erori.Add($"Tipul de TVA „{tip.Cod}” e inactiv și nu poate fi implicit ({unde}) — "
                + "rezolvarea l-ar sări tăcut la culegere.");
    }

    // `PoliticaTvaImplicit` (F23-D2): ținta activă + MESAJUL unicității.
    // Indexul o apără oricum (`NULLS NOT DISTINCT`, F23-D3), dar un `23505` brut
    // nu spune ce cheie s-a repetat; gardianul vorbește înaintea bazei, iar
    // constraint-ul rămâne plasa (60a).
    static void VerificaPoliticaTvaImplicit(
            IObjectSpace os, PoliticaTvaImplicit politica, ICollection<string> erori) {
        if (EsteSters(os, politica))
            return;
        var tipDocId = politica.TipDocument?.ID ?? politica.TipDocumentId;
        var eticheta = politica.TipDocument?.Cod ?? "(fără tip)";
        VerificaTipTvaActiv(os, politica.TipTvaId != Guid.Empty ? politica.TipTvaId : politica.TipTva?.ID,
            $"implicitul {eticheta} × {politica.ClasaFiscala?.ToString() ?? "orice clasă"}", erori);
        if (tipDocId == Guid.Empty)
            return;
        var clasa = politica.ClasaFiscala;
        var valabil = politica.ValabilDeLa;
        var duplicat = os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(p => p.TipDocumentId == tipDocId)
            .Select(p => new { p.ID, p.ClasaFiscala, p.ValabilDeLa }).ToList()
            .Any(p => p.ID != politica.ID && p.ClasaFiscala == clasa && p.ValabilDeLa == valabil);
        if (duplicat)
            erori.Add($"Există deja un implicit de TVA pe {eticheta} × "
                + $"{politica.ClasaFiscala?.ToString() ?? "orice clasă"}"
                + (politica.ValabilDeLa == null ? " (dintotdeauna)" : $" de la {politica.ValabilDeLa:dd.MM.yyyy}")
                + " — două rânduri pe aceeași cheie ar face rezolvarea nedeterministă. "
                + "Editați rândul existent sau dați-i o dată de valabilitate diferită.");
    }

    // `PoliticaTva`: sursa Explicit fără cont e o contrapartidă care nu se poate
    // rezolva niciodată — rândul de TVA n-ar avea unde să cadă la operare.
    static void VerificaPoliticaTva(PoliticaTva politica, ICollection<string> erori) {
        if (politica.SursaContrapartida == SursaCont.Explicit
                && politica.ContrapartidaFallbackId == null && politica.ContrapartidaFallback == null)
            erori.Add($"Politica de TVA pe {politica.TipDocument?.Cod ?? "(fără tip)"} are sursa "
                + "contrapartidei „Explicit”, dar niciun cont — sursa explicită E contul.");
    }

    // `RegulaContare`: trei invarianți, toți despre potrivirea din motor.
    static void VerificaRegulaContare(IObjectSpace os, RegulaContare regula, ICollection<string> erori) {
        var eticheta = regula.TipDocument?.Cod ?? "(fără tip)";
        // D9-A4: o regulă pe un tip al cărui declarant nu contează prin reguli n-ar avea niciun efect.
        var tip = regula.TipDocument ?? os.GetObjectByKey<TipDocument>(regula.TipDocumentId);
        if (!EsteSters(os, regula) && Declaratii.Contractare.DeclarantulTipului(tip?.ClrType) is not { ConteazaPrinReguli: true })
            erori.Add($"{Declaratii.CoduriRefuz.RegulaContareFaraConsumator}: tipul {eticheta} nu contează prin reguli de contare "
                + "— regula n-ar avea niciun efect.");
        if (regula.SursaContDebit == SursaCont.Explicit
                && regula.ContDebitId == null && regula.ContDebit == null)
            erori.Add($"Regula de contare pe {eticheta} are sursa contului debitor „Explicit”, "
                + "dar niciun cont — sursa explicită E contul.");
        if (regula.SursaContCredit == SursaCont.Explicit
                && regula.ContCreditId == null && regula.ContCredit == null)
            erori.Add($"Regula de contare pe {eticheta} are sursa contului creditor „Explicit”, "
                + "dar niciun cont — sursa explicită E contul.");
        // Aceeași formă ca `PoliticaMiscareSaft.Semn`: un `2` ar face regula să
        // nu se potrivească NICIODATĂ, tăcut.
        if (regula.SemnFiltru is not (null or -1 or 1))
            erori.Add($"Regula de contare pe {eticheta} are filtrul de semn {regula.SemnFiltru} — "
                + "semnul e −1, +1 sau gol (orice semn).");
        // Cele două trepte de potrivire sunt ALTERNATIVE (26c): TipMaterial exact
        // bate `NaturaFiltru`, care bate regula generică. O regulă cu amândouă
        // n-ar fi „mai specifică", ar fi doar de neînțeles — iar motorul o
        // potrivește oricum pe treapta exactă, ignorând natura.
        var areTipMaterial = regula.TipMaterialId != null || regula.TipMaterial != null;
        if (areTipMaterial && regula.NaturaFiltru != null)
            erori.Add($"Regula de contare pe {eticheta} are și tip de material, și filtru de natură — "
                + "cele două sunt trepte ALTERNATIVE de potrivire (tipul exact bate natura, care bate "
                + "regula generică). Păstrați una.");
    }

    // `RegulaStoc`: semnul E direcția mișcării; nu există „orice semn" aici (spre
    // deosebire de filtre), fiindcă regula SCRIE rândul, nu îl caută.
    static void VerificaRegulaStoc(RegulaStoc regula, ICollection<string> erori) {
        if (regula.Semn is not (-1 or 1))
            erori.Add($"Regula de stoc pe {regula.TipDocument?.Cod ?? "(fără tip)"} are semnul "
                + $"{regula.Semn} — semnul e −1 (ieșire) sau +1 (intrare).");
    }

    // `PoliticaNumerotare`: seria compune numărul server-owned (53b), iar un
    // contor sub 1 ar produce numere de document zero sau negative.
    //
    // DEVIERE DECLARATĂ de la F23-D5, pe măsurătoare: contractul cerea și
    // „`Format` nevid". `Format` e însă OPȚIONAL în motor — `MotorOperare:814`
    // citește `IsNullOrWhiteSpace(politica.Format)` ca „serie + număr" —, iar
    // `ContaSeeder.SeedNumerotare` nu-l scrie NICIODATĂ. Regula din contract ar
    // fi refuzat orice editare a oricărei politici de numerotare pe orice bază
    // seed-uită, adică gardianul ar fi respins rândurile propriului seed
    // (măsurat: toate cele 14 rânduri private și 9 bugetare).
    //
    // Ce rămâne, și e regula pe care contractul o voia de fapt: un `Format`
    // CULES trebuie să fie utilizabil. `string.Format` aruncă la OPERARE pe un
    // șablon cu indice inexistent („{2}") sau cu acoladă neînchisă — adică un
    // document care nu se mai poate opera, cu o excepție de framework în loc de
    // un mesaj. Se probează aici, pe valoarea culeasă.
    static void VerificaPoliticaNumerotare(PoliticaNumerotare politica, ICollection<string> erori) {
        var eticheta = politica.TipDocument?.Cod ?? "(fără tip)";
        if (string.IsNullOrWhiteSpace(politica.Serie))
            erori.Add($"Politica de numerotare pe {eticheta} n-are serie — numărul documentului se "
                + "compune din ea.");
        if (politica.UrmatorulNumar < 1)
            erori.Add($"Politica de numerotare pe {eticheta} are următorul număr {politica.UrmatorulNumar} "
                + "— numerotarea începe de la 1.");
        if (string.IsNullOrWhiteSpace(politica.Format))
            return;
        try {
            // Aceiași doi parametri ca la operare (`MotorOperare.AsignaNumar`):
            // {0} = numărul, {1} = seria.
            string.Format(politica.Format, 1, politica.Serie ?? "");
        }
        catch (FormatException) {
            erori.Add($"Politica de numerotare pe {eticheta} are formatul „{politica.Format}”, care nu se "
                + "poate compune — șablonul acceptă {0} (numărul) și {1} (seria). Un format greșit ar "
                + "opri operarea documentului, nu culegerea lui.");
        }
    }

    static void VerificaPoliticaScadenta(PoliticaScadenta politica, ICollection<string> erori) {
        if (politica.ZileDefault < 0)
            erori.Add($"Politica de scadență pe {politica.TipDocument?.Cod ?? "(fără tip)"} are "
                + $"{politica.ZileDefault} zile — scadența implicită nu poate fi în trecut față de document.");
    }

    // 46c, 79a: cele patru conturi sunt complete sau goale.
    static void VerificaPoliticaInchidereTva(PoliticaInchidereTva politica, ICollection<string> erori) {
        var conturi = new[] {
            politica.ContDeductibilaId ?? politica.ContDeductibila?.ID,
            politica.ContColectataId ?? politica.ContColectata?.ID,
            politica.ContDePlataId ?? politica.ContDePlata?.ID,
            politica.ContDeRecuperatId ?? politica.ContDeRecuperat?.ID,
        };
        var completate = conturi.Count(c => c != null && c != Guid.Empty);
        if (completate is > 0 and < 4)
            erori.Add($"Politica de închidere TVA pe {politica.TipDocument?.Cod ?? "(fără tip)"} are "
                + $"{completate} din 4 conturi — închiderea cere setul COMPLET (deductibilă, colectată, "
                + "de plată, de recuperat). Completați-le pe toate sau goliți-le pe toate (tip inert).");
    }

    // F27-D2: un singur rând per fel.
    static void VerificaPoliticaInchidere(IObjectSpace os, PoliticaInchidere politica,
            ICollection<string> erori) {
        if (EsteSters(os, politica))
            return;
        var fel = politica.Fel;
        var duplicat = os.GetObjectsQuery<PoliticaInchidere>()
            .Where(p => p.Fel == fel).Select(p => p.ID).ToList()
            .Any(id => id != politica.ID);
        if (duplicat)
            erori.Add($"Există deja o politică de închidere pe felul „{fel}” — "
                + "un fel are o singură severitate. Editați rândul existent.");
    }

    // E3-D4: destinația în afara sursei și cu coordonatele ei obligatorii păstrate (15); un rând per sursă și moment.
    static void VerificaPoliticaInchidereConturi(IObjectSpace os, PoliticaInchidereConturi politica,
            ICollection<string> erori) {
        if (EsteSters(os, politica))
            return;
        var sursa = politica.ContSursa
            ?? (politica.ContSursaId != Guid.Empty ? os.GetObjectByKey<Cont>(politica.ContSursaId) : null);
        var destinatie = politica.ContDestinatie
            ?? (politica.ContDestinatieId != Guid.Empty ? os.GetObjectByKey<Cont>(politica.ContDestinatieId) : null);
        if (sursa == null || destinatie == null) {
            erori.Add("Politica de închidere a conturilor cere contul-sursă și contul-destinație.");
            return;
        }
        for (var c = destinatie; c != null; c = c.Parinte ?? (c.ParinteId is Guid p ? os.GetObjectByKey<Cont>(p) : null))
            if (c.ID == sursa.ID) {
                erori.Add($"Contul-destinație {destinatie.Simbol} e în contul-sursă {sursa.Simbol} — "
                    + "închiderea și-ar muta soldul în ea însăși.");
                break;
            }
        var lipsa = destinatie.DimensiuniObligatorii & ~politica.CoordonatePastrate;
        var faraRepartitor = !politica.PastreazaRepartitor && destinatie.FeluriRepartitor != FeluriRepartitor.Nedeclarat
            && !destinatie.FeluriRepartitor.HasFlag(FeluriRepartitor.Niciunul);
        if (lipsa != DimensiuneFlags.Niciuna || faraRepartitor)
            erori.Add($"Contul-destinație {destinatie.Simbol} cere {string.Join(", ", new[] {
                    faraRepartitor ? "repartitorul" : null, lipsa != DimensiuneFlags.Niciuna ? lipsa.ToString() : null,
                }.OfType<string>())} — rândul trebuie să le păstreze (decizia 15).");
        var (sursaId, moment) = (sursa.ID, politica.Moment);
        if (os.GetObjectsQuery<PoliticaInchidereConturi>()
                .Where(p => p.ContSursaId == sursaId && p.Moment == moment).Select(p => p.ID).ToList()
                .Any(id => id != politica.ID))
            erori.Add($"Există deja un rând de închidere pe contul-sursă {sursa.Simbol} cu momentul „{moment}”. "
                + "Editați rândul existent.");
    }

    // F23-D5, 123 b: aceleași reguli ca atributele XAF ale mapărilor.
    static void VerificaMapareD300(IObjectSpace os, MapareD300 mapare, ICollection<string> erori) {
        if (EsteSters(os, mapare))
            return;
        var rand = mapare.Rand
            ?? (mapare.RandId is Guid randId && randId != Guid.Empty ? os.GetObjectByKey<RandD300>(randId) : null);
        if (!MapareD300.EsteDeOperatiuni(rand))
            erori.Add($"Maparea D300 țintește rd. {rand.Cod}, de fel „{rand.Fel}” — rândurile de total, "
                + "oglindă și extern se calculează, nu se alimentează din mapări.");
        if (!MapareD300.AreRandSauMotiv(mapare))
            erori.Add(MapareD300.MesajMotiv);
        if (!MapareD300.ExcludereaESingura(os, mapare))
            erori.Add(MapareD300.MesajExcludere);
        if (!MapareD300.FaraAscendentMapat(os, mapare))
            erori.Add("Aceeași pereche (tip de TVA × sens) țintește deja un rând aflat pe aceeași "
                + "verticală „din care” cu cel ales — cifra ar intra de două ori în rândul-părinte și "
                + "în totalul lui. Păstrați o singură mapare pe verticală.");
        foreach (var e in CoerentaMapariTva.Incoerente(os, mapare.TipTva?.ID ?? mapare.TipTvaId, mapare.SensTva))
            erori.Add(e);
    }

    static void VerificaMapareD394(IObjectSpace os, MapareD394 mapare, ICollection<string> erori) {
        if (EsteSters(os, mapare))
            return;
        if (!MapareD394.TintaPermisa(mapare.Tip, mapare.SensTva))
            erori.Add($"Maparea D394 țintește {mapare.Tip} pe {mapare.SensTva}. " + MapareD394.MesajTinta);
        if (!MapareD394.AreMotiv(mapare))
            erori.Add(MapareD394.MesajMotiv);
        foreach (var e in CoerentaMapariTva.Incoerente(os, mapare.TipTva?.ID ?? mapare.TipTvaId, mapare.SensTva))
            erori.Add(e);
    }

    // Numele CLASEI de domeniu al unui rând, nu al proxy-ului de change tracking
    // (`RegulaContareProxy`) — aceeași dezproxare ca la `VerificaCodDenumire`.
}
