using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// Proiecția de REST (F3-D4): documentele OPERATE care mai au ceva de stins —
// candidații panoului de stingeri. Regula modulului rămâne cea din 42c: nimic
// nu se calculează în client, iar agregarea se face ÎNTÂI, cu join-urile pe
// REZULTATUL agregat (nu subquery corelat per rând, nu navigație lazy).
//
// ═══ De ce UNION PER TIP CONCRET și nu un query pe `Document` ═══
// Sub TPT nu există discriminator: un `GetObjectsQuery<Document>()` n-ar putea
// da nici codul tipului (vocabularul de rutare al clientului), nici
// CONTRAPARTIDA — care e o latură DIFERITĂ per tip (furnizorul e predator pe
// FCT, clientul e primitor pe FCL…). Uniunea de ramuri concrete pune ambele în
// SQL, cu literal per ramură.
//
// ═══ MĂRGINIREA (F27-D7) ═══
// Totalul nu se mai agregă la citire: e `Document.TotalStingere`, scris de motor
// la operare. De aceea `ReturClient` INTRĂ acum în uniune — totalul lui e deja
// filtrat prin `LiniiCreanta` la scriere, deci proiecția nu mai poate diverge de
// serviciu, iar amânarea din antetul vechi e închisă.
// Restul unui document la o dată = restul lui la ULTIMA perioadă de referință
// (`PartidaDeschisa`) minus imperecherile de după ea. Costul e mărginit de
// fereastra deschisă plus numărul partidelor, nu de tot istoricul.

// Rândul „mai am de stins": PLAT prin construcție (deciziile 6/7).
public sealed class DocumentCuRestRand {
    public Guid DocumentId { get; set; }
    // Codul ancorei `TipDocument` — literal per ramură a uniunii, nu coloană.
    public string Tip { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    // Latura PARTENER/ANGAJAT (nu contul propriu, nu gestiunea): cheia pe care
    // invariantul stingerii cere potrivire (`ValideazaCreare` — contrapartida
    // stingătorului trebuie să apară pe documentul stins).
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    // Sensul stingerii pe care documentul îl CONSUMĂ (F19-D16), ca LITERAL per
    // ramură — exact cum e `Tip`. Dublează `Document.SensDeStins` (funcție de
    // TIP, deci netraductibilă în SQL), deci are check de consistență pe fiecare
    // rând în ModelCheck (42c: o proiecție care dublează un calcul al motorului
    // se măsoară contra lui, nu se afirmă).
    public string Sens { get; set; }

    public decimal Total { get; set; }
    public decimal Asignat { get; set; }
    public decimal Rest { get; set; }
}

// Atomul de unpivot al imperecherii, ca TIP NUMIT: `Concat` cere aceeași formă
// pe ambele laturi, iar tipul numit face uniunea explicită și refolosibilă.
public sealed class SumaPeDocument {
    public Guid DocumentId { get; set; }
    public decimal Suma { get; set; }
}

// Antetul unei ramuri, înainte de join-urile pe agregate: aceleași coloane
// pentru toate tipurile, ca uniunea să fie o singură formă.
sealed class AntetCuRest {
    public Guid DocumentId { get; set; }
    public string Tip { get; set; }
    public string Numar { get; set; }
    public DateOnly Data { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    public string Sens { get; set; }
    public decimal Total { get; set; }
}

public static class ImperecheriProiectii {
    // Literalii de sens, o singură dată: `SensStingere.X.ToString()` nu se
    // traduce în SQL, iar un string „liber" în șase ramuri ar putea devia de
    // enum fără ca nimic să se plângă.
    static readonly string SensDatorie = SensStingere.Datorie.ToString();
    static readonly string SensCreanta = SensStingere.Creanta.ToString();

    // ── Atomii (42c) ────────────────────────────────────────────────────────

    // Unpivot-ul imperecherii pe AMBELE laturi: un rând contribuie la restul
    // stingătorului ȘI la restul documentului stins (un document poate sta pe
    // ambele roluri — lanțul avans↔regularizare, 31d). Geamănul în SQL al lui
    // `ImperechereService.Asignat`, care face același lucru cu un `||` pe un
    // singur document. ALGEBRIC: rândurile inverse (F27-D8) intră cu semn.
    // CUSĂTURĂ, deliberat NEfuzionată: serviciul răspunde pentru UN document
    // (predicat, nu grup) și e apelat din motor pe cale caldă; aici avem nevoie
    // de forma agregabilă. Refactorizarea serviciului pe unpivot ar schimba
    // planul SQL al unei căi validate, fără câștig — cele două rămân separate,
    // iar ModelCheck le compară pe fiecare rând al proiecției (F3-D9).
    //
    // `dupa`/`panaLa` (F27-D7) taie FEREASTRA: „ce s-a stins după referință".
    // Fără ele e exact forma de dinainte.
    public static IQueryable<SumaPeDocument> Asignari(IObjectSpace os,
            DateOnly? dupa = null, DateOnly? panaLa = null) {
        var legaturi = os.GetObjectsQuery<Imperechere>();
        if (dupa is DateOnly d)
            legaturi = legaturi.Where(i => i.Data > d);
        if (panaLa is DateOnly p)
            legaturi = legaturi.Where(i => i.Data <= p);
        return legaturi
            .Select(i => new SumaPeDocument { DocumentId = i.DocumentStingatorId, Suma = i.Suma })
            .Concat(legaturi
                .Select(i => new SumaPeDocument { DocumentId = i.DocumentId, Suma = i.Suma }));
    }

    // ── Proiecția ───────────────────────────────────────────────────────────

    // Documentele operate cu rest > 0 la `laData` (null = „tot"), opțional
    // filtrate pe contrapartidă și pe sens — candidații de stins (F3-D4).
    public static IQueryable<DocumentCuRestRand> DocumenteCuRest(
        IObjectSpace os, Guid? contrapartidaId = null, SensStingere? sens = null,
        DateOnly? laData = null) {

        // Contrapartida per tip: FCT → furnizorul (predator), FCL → clientul
        // (primitor), PLT → beneficiarul (primitor), INC → plătitorul
        // (predator), DEC → titularul (predator), RDC → clientul (predator).
        var antete =
            os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCT", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m })
            .Concat(os.GetObjectsQuery<FacturaIesire>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCL", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta, Total = d.TotalStingere ?? 0m }))
            // PLT/INC: picioarele de VIRAMENT INTERN (F7) sunt EXCLUSE. Au
            // `Rest > 0` pe veci (nu se sting niciodată — `CapacitateStingere` e
            // null, `PoateFiStins` e false pe ele), iar contrapartida lor E un
            // cont propriu, deci un panou filtrat pe un cont propriu chiar
            // le-ar întoarce ca „de stins" — un candidat pe care serverul îl
            // refuză la creare. Filtrul e oglinda predicatului de domeniu
            // (`DocumentTrezorerie.EsteVirament`: AMBELE laturi conturi
            // proprii); sub TPT testul de tip devine LEFT JOIN pe tabela mică
            // `ContPropriu` + IS NULL, nu o a doua interogare.
            .Concat(os.GetObjectsQuery<Plata>()
                .Where(d => d.Stare == StareDocument.Operat
                    && !(d.Predator is ContPropriu && d.Primitor is ContPropriu))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "PLT", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<Incasare>()
                .Where(d => d.Stare == StareDocument.Operat
                    && !(d.Predator is ContPropriu && d.Primitor is ContPropriu))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "INC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }))
            .Concat(os.GetObjectsQuery<Decont>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "DEC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }))
            // RDC (F27-D7): totalul lui e deja cel filtrat prin `LiniiCreanta`,
            // fiindcă motorul îl scrie pe document — nu mai există al doilea
            // adevăr de evitat. Returul lasă un sold CREDITOR pe contul
            // clientului, deci consumă jumătatea de datorie (`SensDeStins`).
            .Concat(os.GetObjectsQuery<ReturClient>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "RDC", Numar = d.Numar, Data = d.Data,
                    DataInregistrare = d.DataInregistrare,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie, Total = d.TotalStingere ?? 0m }));

        if (laData is DateOnly zi)
            antete = antete.Where(a => a.DataInregistrare <= zi);
        if (contrapartidaId is Guid cp)
            antete = antete.Where(a => a.ContrapartidaId == cp);
        // Filtrul de SENS (F19-D16): panoul de compensare cere candidații UNEI
        // jumătăți de plafon. Fără el ar propune o factură de client sub
        // jumătatea de datorie a notei — o stingere pe care serviciul o refuză.
        if (sens is SensStingere sensCerut) {
            var literal = sensCerut.ToString();
            antete = antete.Where(a => a.Sens == literal);
        }

        // Referința = ultima perioadă DE REFERINȚĂ care se termină până la dată
        // (F27-D3); `laData` null = „tot", deci ultima referință a bazei.
        var referinta = SolduriService.Referinta(os, laData ?? DateOnly.MaxValue);
        var faraReferinta = referinta == null;
        var sfarsitReferinta = referinta?.Sfarsit ?? DateOnly.MinValue;
        var anReferinta = referinta?.An ?? 0;
        var lunaReferinta = referinta?.Luna ?? 0;

        // Fereastra DESCHISĂ: ce s-a stins DUPĂ referință. Un fapt de stingere
        // nu poate cădea înaintea ei fără să fi fost scris într-o perioadă
        // deschisă, iar gardianul de perioadă îl refuză (F27-D8) — deci partida
        // scrisă la închidere rămâne punctul de pornire valabil.
        var fereastra = Asignari(os, referinta?.Sfarsit, laData)
            .GroupBy(a => a.DocumentId)
            .Select(g => new SumaPeDocument { DocumentId = g.Key, Suma = g.Sum(x => x.Suma) });
        // Partidele referinței ca ATOMI, nu ca entități: peste un
        // `DefaultIfEmpty()` de entitate, `p != null` nu se traduce (EF îl lasă
        // în evaluare pe client), iar un scalar nullable e exact aceeași
        // întrebare, exprimabilă în SQL.
        var partide = os.GetObjectsQuery<PartidaDeschisa>()
            .Where(p => p.An == anReferinta && p.Luna == lunaReferinta)
            .Select(p => new SumaPeDocument { DocumentId = p.DocumentId, Suma = p.Rest });

        var randuri =
            from a in antete
            // LEFT pe partide: restul documentului la sfârșitul referinței.
            join p in partide on a.DocumentId equals p.DocumentId into gp
            from p in gp.DefaultIfEmpty()
            // LEFT pe fereastră: un document neatins de nicio stingere de după
            // referință apare cu restul lui întreg, nu dispare din listă.
            join s in fereastra on a.DocumentId equals s.DocumentId into gs
            from s in gs.DefaultIfEmpty()
            let restPartida = (decimal?)p.Suma
            let dinFereastra = (decimal?)s.Suma
            // Restul la REFERINȚĂ, în trei feluri: partida scrisă la închidere;
            // totalul, pentru documentul înregistrat DUPĂ referință; zero pentru
            // documentul înregistrat înainte și absent din partide — el era stins
            // integral, iar dacă reapare aici o face printr-o desfacere de după.
            let restReferinta = restPartida
                ?? (faraReferinta || a.DataInregistrare > sfarsitReferinta ? a.Total : 0m)
            let rest = restReferinta - (dinFereastra ?? 0m)
            // Candidații: partidele referinței ∪ documentele de după ea ∪
            // documentele atinse de o stingere din fereastra deschisă.
            where faraReferinta || restPartida != null || dinFereastra != null
                || a.DataInregistrare > sfarsitReferinta
            select new DocumentCuRestRand {
                DocumentId = a.DocumentId,
                Tip = a.Tip,
                Numar = a.Numar,
                Data = a.Data,
                ContrapartidaId = a.ContrapartidaId,
                ContrapartidaDenumire = a.ContrapartidaDenumire,
                Sens = a.Sens,
                Total = a.Total,
                // `Asignat` e diferența, nu o a treia agregare: prin definiție
                // `Asignat = Total − Rest`, deci o citire în plus n-ar adăuga
                // adevăr, doar un al doilea drum spre el.
                Asignat = a.Total - rest,
                Rest = rest
            };

        // Filtrul pe REST se aplică după calcul (EF îl împinge în SQL peste
        // subinterogare): un document stins integral nu e candidat. Rămâne
        // `IQueryable` — DataSourceLoader pune deasupra filtrarea/sortarea/
        // paginarea clientului (43c).
        return randuri.Where(r => r.Rest > 0m);
    }
}
