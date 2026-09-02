using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// IMPLICITELE DE CULEGERE — o singură sursă (felia 23, F23-D1/D2).
//
// Ce e și ce NU e. Implicitul e o AFORDANȚĂ: propune o valoare pe o linie NOUĂ
// care n-are una, și atât. Nu validează nimic (un partener invizibil nu produce
// 422, produce „fără partener"), nu rulează în motor (datoria P1 rămâne la
// culegere — 38d) și nu atinge liniile EXISTENTE, unde absența unui câmp e
// golire deliberată (56, round-trip).
//
// Trei apelanți, o funcție: controllerul XAF de creare a liniei
// (`DefaultTipTvaController`), cele cinci Apply-uri (prin wrapper-ul
// `TvaService.AplicaTipTvaImplicit`, ca semnătura lor să nu se schimbe) și
// endpoint-ul de citire prin care clientul precompletează perechea (id,
// etichetă) și afișează SURSA. Serverul și clientul nu pot diverge fiindcă
// amândoi întreabă funcția asta.
//
// REGIMUL E AL PARTENERULUI, COTA E A PRODUSULUI. `TipTva` = cotă × regim, iar
// cele două jumătăți vin din surse diferite; de-aia rezolvarea are DOUĂ
// picioare care se împacă la final, nu o listă de trepte care se scurtcircuitează
// la prima potrivire.
//
// Lucrează pe FK-uri + `IObjectSpace` (25b), fără navigații lazy, și fără `is`
// pe frunze (produsul vine prin contractul `DocumentDetaliu.ProdusCules`,
// partenerul prin întrebarea pusă NOMENCLATORULUI, nu documentului).
public static class ImpliciteService {
    /// <summary>
    /// Tipul de TVA propus, sursa lui și motivul — trei date, nu una: clientul
    /// arată sub câmp de unde vine valoarea, iar când NU vine niciuna motivul
    /// spune de ce (partener invizibil, tip inactiv sărit, profil fără politică).
    /// </summary>
    public readonly record struct RezultatImplicit(Guid? TipTvaId, SursaImplicit Sursa, string Motiv);

    // Un singur text pentru „partenerul nu se vede", indiferent dacă lipsește de
    // pe document, nu există în bază sau e invizibil pe ușa securizată. NU e o
    // simplificare: distincția ar fi un ORACOL DE EXISTENȚĂ (80a) — un
    // utilizator fără drept pe partener ar afla, din motivul implicitului, că
    // partenerul cerut există.
    const string FaraPartener =
        "Fără partener vizibil pe document — rezolvarea a căzut pe rândurile generice.";

    // Informația minimă despre un tip candidat: regimul (jumătatea care decide
    // împăcarea) și viața lui (F23-D3).
    readonly record struct InfoTip(Guid Id, string Cod, RegimTva Regim, bool Activ);

    /// <summary>
    /// Rezolvarea din F23-D2, pașii 1–6. `data` e data DOCUMENTULUI (nu ziua de
    /// azi): rândurile de politică au valabilitate, iar o factură cu dată în urmă
    /// primește implicitul care era în vigoare atunci.
    /// </summary>
    public static RezultatImplicit TipTva(IObjectSpace os, Guid tipDocumentId,
            Guid? partenerId, Guid? produsId, DateOnly data) {
        var note = new List<string>();

        // ── Pasul 4: clasa fiscală, din funcția LEGII ───────────────────────
        // Se citește proiectat, nu prin navigație; `null` = n-avem partener
        // (lipsă / inexistent / invizibil — nedistinse, vezi `FaraPartener`),
        // caz în care doar rândurile generice de politică mai pot potrivi.
        ClasaFiscalaPartener? clasa = null;
        Guid? candidatPartener = null;
        if (partenerId is Guid pid && pid != Guid.Empty) {
            var p = os.GetObjectsQuery<Partener>()
                .Where(x => x.ID == pid)
                .Select(x => new { x.TipPersoana, x.Tara, x.InregistratTva, x.TipTvaImplicitId })
                .FirstOrDefault();
            if (p == null)
                note.Add(FaraPartener);
            else {
                clasa = ClasaFiscala.APartenerului(p.TipPersoana, p.Tara, p.InregistratTva);
                candidatPartener = p.TipTvaImplicitId;
            }
        }
        else
            note.Add(FaraPartener);

        // ── Pasul 1b: rândul de politică cel mai SPECIFIC ───────────────────
        // Candidații: rândurile tipului valabile la data documentului, pe clasa
        // exactă SAU generice. Ordonarea în MEMORIE (lista are cel mult câteva
        // rânduri per tip): clasa exactă bate `null`, iar la egalitate de clasă
        // bate `ValabilDeLa` cel mai recent — „dintotdeauna" (null) e cel mai
        // vechi posibil, deci pierde în fața oricărei date.
        var candidatPolitica = os.GetObjectsQuery<PoliticaTvaImplicit>()
            .Where(r => r.TipDocumentId == tipDocumentId
                && (r.ValabilDeLa == null || r.ValabilDeLa <= data)
                && (r.ClasaFiscala == null || r.ClasaFiscala == clasa))
            .Select(r => new { r.ClasaFiscala, r.ValabilDeLa, r.TipTvaId })
            .ToList()
            .Where(r => r.ClasaFiscala == null || r.ClasaFiscala == clasa)
            .OrderByDescending(r => r.ClasaFiscala != null)
            .ThenByDescending(r => r.ValabilDeLa ?? DateOnly.MinValue)
            .Select(r => (Guid?)r.TipTvaId)
            .FirstOrDefault();

        // ── Pasul 1c: ancora tipului de document (datoria P1) ───────────────
        var candidatAncora = os.GetObjectsQuery<TipDocument>()
            .Where(t => t.ID == tipDocumentId)
            .Select(t => t.TipTvaImplicitId)
            .FirstOrDefault();

        // ── Pasul 2: purtătorul de COTĂ ─────────────────────────────────────
        Guid? candidatProdus = null;
        if (produsId is Guid prid && prid != Guid.Empty)
            candidatProdus = os.GetObjectsQuery<Produs>()
                .Where(x => x.ID == prid)
                .Select(x => x.TipTvaImplicitId)
                .FirstOrDefault();

        // O singură interogare pentru toate tipurile candidate: regimul lor
        // decide împăcarea, iar `Activ` decide dacă mai sunt în joc.
        var ids = new[] { candidatPartener, candidatPolitica, candidatAncora, candidatProdus }
            .Where(x => x != null).Select(x => x.Value).Distinct().ToList();
        var tipuri = ids.Count == 0
            ? []
            : os.GetObjectsQuery<TipTva>().Where(t => ids.Contains(t.ID))
                .Select(t => new InfoTip(t.ID, t.Cod, t.Regim, t.Activ))
                .ToDictionary(t => t.Id);

        // ── Pasul 6: tipul INACTIV nu se alege niciodată ────────────────────
        // Treapta care l-ar întoarce se SARE — cu motiv, ca defectul de
        // configurare să fie citibil pe ecran, nu doar în raportul de profil
        // (F23-D8). Se sare TREAPTA, nu se caută al doilea-cel-mai-specific
        // rând de politică: alegerea „următorului" ar face rezolvarea să depindă
        // de ce s-a dezactivat, adică ar fi un al doilea mecanism, ascuns.
        InfoTip? Viu(Guid? id, string treapta) {
            if (id == null || !tipuri.TryGetValue(id.Value, out var info))
                return null;
            if (info.Activ)
                return info;
            note.Add($"Tipul „{info.Cod}” ({treapta}) e INACTIV și a fost sărit.");
            return null;
        }

        var partener = Viu(candidatPartener, "regimul propriu al partenerului");
        var politica = Viu(candidatPolitica, "politica tipului × clasa fiscală");
        var ancora = Viu(candidatAncora, "ancora tipului de document");
        var produs = Viu(candidatProdus, "cota proprie a produsului");

        // ── Pasul 1: R = purtătorul de REGIM, în ordinea trepte lor ─────────
        var (r, sursaR) =
            partener != null ? (partener, SursaImplicit.Partener)
            : politica != null ? (politica, SursaImplicit.Politica)
            : ancora != null ? (ancora, SursaImplicit.Ancora)
            : ((InfoTip?)null, SursaImplicit.Niciuna);

        // ── Pasul 3: împăcarea ──────────────────────────────────────────────
        // Produsul își impune COTA doar când regimul coincide (pâinea rămâne 11%
        // de la orice furnizor înregistrat, iar pe bugetar `CAP11` bate `CAP21`
        // la fel). Când regimurile DIFERĂ, regimul partenerului bate cota
        // produsului: o livrare intracomunitară e scutită indiferent de produs.
        if (produs != null && r != null && produs.Value.Regim == r.Value.Regim)
            return new RezultatImplicit(produs.Value.Id, SursaImplicit.Produs,
                Text(note, $"Cota produsului („{produs.Value.Cod}”) peste regimul dat de "
                    + $"{Eticheta(sursaR)} — același regim."));
        if (r != null)
            return new RezultatImplicit(r.Value.Id, sursaR,
                Text(note, produs != null
                    ? $"Regimul dat de {Eticheta(sursaR)} („{r.Value.Cod}”) bate cota produsului "
                        + $"(„{produs.Value.Cod}”) — regimuri diferite."
                    : $"Tipul vine din {Eticheta(sursaR)} („{r.Value.Cod}”)."));
        if (produs != null)
            return new RezultatImplicit(produs.Value.Id, SursaImplicit.Produs,
                Text(note, $"Nicio sursă de regim — rămâne cota produsului („{produs.Value.Cod}”)."));
        return new RezultatImplicit(null, SursaImplicit.Niciuna,
            Text(note, "Niciun implicit configurat pentru acest tip de document."));
    }

    /// <summary>
    /// Partenerul documentului, FĂRĂ `is`/`switch` pe frunze (F23-D2.5,
    /// invariantul II): întrebarea se pune nomenclatorului („e `Partener` cel de
    /// pe latura asta?"), nu documentului („ce fel de document ești?"). Sub TPT
    /// `GetObjectByKey&lt;Partener&gt;` întoarce null pentru un repartitor de altă
    /// frunză — și tot null pentru unul invizibil pe ușa securizată, ceea ce e
    /// exact răspunsul pe care rezolvarea îl vrea (80a: fără oracol de existență).
    /// Dacă AMBELE laturi ar fi parteneri (nu există azi), câștigă predatorul.
    /// </summary>
    public static Guid? PartenerulDocumentului(IObjectSpace os, Document doc) {
        if (doc == null)
            return null;
        if (doc.PredatorId != Guid.Empty && os.GetObjectByKey<Partener>(doc.PredatorId) != null)
            return doc.PredatorId;
        if (doc.PrimitorId != Guid.Empty && os.GetObjectByKey<Partener>(doc.PrimitorId) != null)
            return doc.PrimitorId;
        return null;
    }

    static string Eticheta(SursaImplicit sursa) => sursa switch {
        SursaImplicit.Partener => "regimul propriu al partenerului",
        SursaImplicit.Produs => "cota proprie a produsului",
        SursaImplicit.Politica => "politica tipului × clasa fiscală",
        SursaImplicit.Ancora => "ancora tipului de document",
        _ => "nicio sursă",
    };

    // Motivul = verdictul, plus notele adunate pe drum (partenerul care nu se
    // vede, tipurile inactive sărite). Notele stau ÎNAINTEA verdictului fiindcă
    // ele explică de ce verdictul e cel care e.
    static string Text(List<string> note, string verdict) =>
        note.Count == 0 ? verdict : string.Join(" ", note.Append(verdict));
}
