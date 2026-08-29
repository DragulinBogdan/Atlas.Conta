using Atlas.Conta.BackOffice.Module.BusinessObjects;
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
// ═══ `ReturClient` EXCLUS DELIBERAT ═══
// RDC suprascrie `LiniiCreanta` (doar liniile de venit — FAZA 1C §7), deci
// `ImperechereService.Total` pentru el NU e Σ tuturor liniilor. Un `GROUP BY`
// universal peste `DocumentDetaliu` l-ar include cu un total mai mare decât
// creanța reală și ar diverge TĂCUT de serviciu — exact genul de „al doilea
// adevăr" pe care proiecțiile îl evită. Includerea lui cere o soluție pentru
// `LiniiCreanta` polimorf în SQL (filtrul e al tipului, nu al coloanei) și e
// documentată ca decizie viitoare în contractul feliei, nu improvizată aici.
// Consecință asumată azi: un retur de la client nu apare printre candidații de
// stins; imperecherea lui rămâne pe calea directă (serviciu / XAF).

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
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaDenumire { get; set; }
    public string Sens { get; set; }
}

public static class ImperecheriProiectii {
    // Literalii de sens, o singură dată: `SensStingere.X.ToString()` nu se
    // traduce în SQL, iar un string „liber" în cinci ramuri ar putea devia de
    // enum fără ca nimic să se plângă.
    static readonly string SensDatorie = SensStingere.Datorie.ToString();
    static readonly string SensCreanta = SensStingere.Creanta.ToString();

    // ── Atomii (42c) ────────────────────────────────────────────────────────

    // Unpivot-ul imperecherii pe AMBELE laturi: un rând contribuie la restul
    // stingătorului ȘI la restul documentului stins (un document poate sta pe
    // ambele roluri — lanțul avans↔regularizare, 31d). Geamănul în SQL al lui
    // `ImperechereService.Asignat`, care face același lucru cu un `||` pe un
    // singur document.
    // CUSĂTURĂ, deliberat NEfuzionată: serviciul răspunde pentru UN document
    // (predicat, nu grup) și e apelat din motor pe cale caldă; aici avem nevoie
    // de forma agregabilă. Refactorizarea serviciului pe unpivot ar schimba
    // planul SQL al unei căi validate, fără câștig — cele două rămân separate,
    // iar ModelCheck le compară pe fiecare rând al proiecției (F3-D9).
    public static IQueryable<SumaPeDocument> Asignari(IObjectSpace os) =>
        os.GetObjectsQuery<Imperechere>()
            .Select(i => new SumaPeDocument { DocumentId = i.DocumentStingatorId, Suma = i.Suma })
            .Concat(os.GetObjectsQuery<Imperechere>()
                .Select(i => new SumaPeDocument { DocumentId = i.DocumentId, Suma = i.Suma }));

    // Totalul BRUT per document, agregat pe BAZA detaliului — definiția lui
    // `Document.Total`. Valabil pentru toate tipurile din uniune: niciunul nu
    // suprascrie `LiniiCreanta` (singurul care o face, ReturClient, e exclus).
    public static IQueryable<SumaPeDocument> Brut(IObjectSpace os) =>
        os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new SumaPeDocument {
                DocumentId = g.Key, Suma = g.Sum(x => x.Valoare + x.ValoareTva)
            });

    // ── Proiecția ───────────────────────────────────────────────────────────

    // Documentele operate cu rest > 0, opțional filtrate pe contrapartidă
    // (candidații de stins pentru o plată/încasare — F3-D4).
    public static IQueryable<DocumentCuRestRand> DocumenteCuRest(
        IObjectSpace os, Guid? contrapartidaId = null, SensStingere? sens = null) {

        // Contrapartida per tip: FCT → furnizorul (predator), FCL → clientul
        // (primitor), PLT → beneficiarul (primitor), INC → plătitorul
        // (predator), DEC → titularul (predator). RDC lipsește DELIBERAT (vezi
        // antetul fișierului).
        var antete =
            os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCT", Numar = d.Numar, Data = d.Data,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie })
            .Concat(os.GetObjectsQuery<FacturaIesire>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "FCL", Numar = d.Numar, Data = d.Data,
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta }))
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
                    ContrapartidaId = d.PrimitorId, ContrapartidaDenumire = d.Primitor.Denumire,
                    Sens = SensCreanta }))
            .Concat(os.GetObjectsQuery<Incasare>()
                .Where(d => d.Stare == StareDocument.Operat
                    && !(d.Predator is ContPropriu && d.Primitor is ContPropriu))
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "INC", Numar = d.Numar, Data = d.Data,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie }))
            .Concat(os.GetObjectsQuery<Decont>().Where(d => d.Stare == StareDocument.Operat)
                .Select(d => new AntetCuRest {
                    DocumentId = d.ID, Tip = "DEC", Numar = d.Numar, Data = d.Data,
                    ContrapartidaId = d.PredatorId, ContrapartidaDenumire = d.Predator.Denumire,
                    Sens = SensDatorie }));

        if (contrapartidaId is Guid cp)
            antete = antete.Where(a => a.ContrapartidaId == cp);
        // Filtrul de SENS (F19-D16): panoul de compensare cere candidații UNEI
        // jumătăți de plafon. Fără el ar propune o factură de client sub
        // jumătatea de datorie a notei — o stingere pe care serviciul o refuză.
        if (sens is SensStingere sensCerut) {
            var literal = sensCerut.ToString();
            antete = antete.Where(a => a.Sens == literal);
        }

        var totaluri = Brut(os);
        var asignate = Asignari(os)
            .GroupBy(a => a.DocumentId)
            .Select(g => new SumaPeDocument { DocumentId = g.Key, Suma = g.Sum(x => x.Suma) });

        var randuri =
            from a in antete
            // LEFT JOIN pe ambele agregate: un document operat fără linii
            // (imposibil azi) sau fără nicio stingere trebuie să apară cu 0, nu
            // să dispară din listă.
            join t in totaluri on a.DocumentId equals t.DocumentId into gt
            from t in gt.DefaultIfEmpty()
            join s in asignate on a.DocumentId equals s.DocumentId into gs
            from s in gs.DefaultIfEmpty()
            select new DocumentCuRestRand {
                DocumentId = a.DocumentId,
                Tip = a.Tip,
                Numar = a.Numar,
                Data = a.Data,
                ContrapartidaId = a.ContrapartidaId,
                ContrapartidaDenumire = a.ContrapartidaDenumire,
                Sens = a.Sens,
                Total = (decimal?)t.Suma ?? 0m,
                Asignat = (decimal?)s.Suma ?? 0m,
                Rest = ((decimal?)t.Suma ?? 0m) - ((decimal?)s.Suma ?? 0m)
            };

        // Filtrul pe REST se aplică după calcul (EF îl împinge în SQL peste
        // subinterogare): un document stins integral nu e candidat. Rămâne
        // `IQueryable` — DataSourceLoader pune deasupra filtrarea/sortarea/
        // paginarea clientului (43c).
        return randuri.Where(r => r.Rest > 0m);
    }
}
