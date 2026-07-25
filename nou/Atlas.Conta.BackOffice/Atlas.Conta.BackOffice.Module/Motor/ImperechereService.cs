using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Stingerea (decizia 17): m2m stingător↔document cu sume parțiale. Invarianții
// trăiesc aici — aceeași cale pentru UI, harness și viitorul Web API, ca la
// MotorOperare. `ramas` e calcul (Total − Σ imperecheri), nu coloană.
//
// Un document poate apărea în AMBELE roluri (Plata de avans ↔ Incasarea de
// regularizare: încasarea stă pe latura de document), deci suma asignată se
// numără pe ambele coloane.
//
// Rolul de STINGĂTOR e polimorf (decizia 48b): îl declară
// `Document.CapacitateStingere` — azi trezoreria (o contrapartidă, plafon =
// totalul ei) și nota contabilă (compensarea: contrapartidele explicite ale
// liniilor, plafon per contrapartidă).
public static class ImperechereService {
    // Totalul documentului, din liniile PERSISTATE (nu navigația Detalii —
    // apelanții nu garantează lazy loading, iar imperecherea se face pe
    // documente deja operate, deci comise). BRUT (P1, design §3): plata stinge
    // Valoare + ValoareTva; la regimurile capitalizate ValoareTva e 0.
    // Filtrul `LiniiCreanta` ține serviciul în oglindă cu `Document.Total`
    // suprascris (ReturClient: doar liniile de venit — review advers 1C-a).
    public static decimal Total(IObjectSpace os, Guid documentId) {
        var doc = os.GetObjectByKey<Document>(documentId);
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == documentId);
        if (doc != null)
            linii = doc.LiniiCreanta(linii);
        return linii.Select(d => (decimal?)(d.Valoare + d.ValoareTva)).Sum() ?? 0m;
    }

    public static decimal Asignat(IObjectSpace os, Guid documentId) =>
        os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == documentId || i.DocumentId == documentId)
            .Select(i => (decimal?)i.Suma).Sum() ?? 0m;

    public static decimal Ramas(IObjectSpace os, Guid documentId) =>
        Total(os, documentId) - Asignat(os, documentId);

    // Tranzacția publică (UI / Web API): validează, creează, comite.
    public static Imperechere Imperecheaza(IObjectSpace os,
        Document stingator, Document document, decimal suma) {
        var imperechere = Creeaza(os, stingator, document, suma, autogenerat: false);
        os.CommitChanges();
        return imperechere;
    }

    // Fără commit — motorul o cheamă din tranzacția operării plății autogenerate.
    internal static Imperechere Creeaza(IObjectSpace os,
        Document stingator, Document document, decimal suma, bool autogenerat) {
        // Validarea rulează ÎNAINTE de CreateObject — comportamentul existent
        // (motorul nu lasă rând-fantomă pe eșec) rămâne exact.
        ValideazaCreare(os, stingator, document, suma);
        var imperechere = os.CreateObject<Imperechere>();
        imperechere.DocumentStingator = stingator;
        imperechere.Document = document;
        imperechere.Suma = suma;
        imperechere.Autogenerat = autogenerat;
        return imperechere;
    }

    // Invarianții stingerii, extrași ca să fie refolosibili de gardianul UI
    // (ImperechereController: New generic e permis, dar validat la commit —
    // decizia 31d). Aruncă UserFriendlyException (OperareException) cu mesaj de
    // business; null-guard pe navigații (culegerea prin UI le poate lăsa goale —
    // motorul le trimite mereu setate).
    internal static void ValideazaCreare(IObjectSpace os,
        Document stingator, Document document, decimal suma) {
        if (stingator == null || document == null)
            throw new OperareException(
                "Imperecherea leagă un document care stinge de un document stins — ambele sunt obligatorii.");
        if (stingator.Stare != StareDocument.Operat || document.Stare != StareDocument.Operat)
            throw new OperareException("Imperecherea leagă două documente operate (registrele lor există).");
        if (document.ID == stingator.ID)
            throw new OperareException("Un document nu se poate stinge pe el însuși.");
        // Două documente de trezorerie de ACELAȘI sens nu se sting reciproc
        // (Plata↔Plata ar consuma restul stingibil al ambelor fără să stingă
        // nimic real — review advers); Plata↔Incasare rămâne permisă (lanțul
        // avans↔regularizare, decizia 31d).
        if ((stingator is Plata && document is Plata)
            || (stingator is Incasare && document is Incasare))
            throw new OperareException(
                "Două documente de trezorerie de același sens nu se imperechează — imperecherea stinge un document de sens opus.");
        if (suma <= 0)
            throw new OperareException("Suma imperecheată trebuie să fie pozitivă.");

        // Cine poate sta pe rolul de stingător e decizia TIPULUI (48b): tipurile
        // fără hook (facturi, NIR, bonuri…) nu sting nimic. Relaxarea FK-ului la
        // `Document` a mutat filtrul din schemă în validare.
        var capacitati = stingator.CapacitateStingere(os)
            ?? throw new OperareException(
                "Doar plățile/încasările și notele contabile pot stinge un document (compensarea e notă contabilă).");
        if (capacitati.Count == 0)
            throw new OperareException(
                "Documentul care stinge nu poartă nicio contrapartidă (nota contabilă cere repartitori expliciți pe linii).");

        // Contrapartida stingătorului (furnizor/client/angajat) trebuie să apară
        // pe documentul stins — echivalentul grupării pe partener din legacy
        // (spDecontariObligatii); acoperă și lanțul avans↔decont↔regularizare.
        Guid? contrapartida = null;
        foreach (var candidat in capacitati.Keys)
            if (candidat == document.PredatorId || candidat == document.PrimitorId) {
                contrapartida = candidat;
                break;
            }
        if (contrapartida == null)
            throw new OperareException(
                "Documentul care stinge și documentul stins nu împart aceeași contrapartidă (partener/angajat).");

        var ramasStingator = capacitati[contrapartida.Value] - AsignatFataDe(os, stingator.ID, contrapartida.Value);
        if (suma > ramasStingator)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul neasignat al documentului care stinge ({ramasStingator:0.##}).");
        var ramasDocument = Ramas(os, document.ID);
        if (suma > ramasDocument)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul nestins al documentului ({ramasDocument:0.##}).");
    }

    // Cât a consumat deja stingătorul din plafonul lui față de o contrapartidă:
    // stingerile lui către documente pe care apare acea contrapartidă, PLUS tot
    // ce s-a stins pe el (rolul de document stins — lanțul avans↔regularizare).
    // Pentru trezorerie (o singură contrapartidă, obligatorie pe toate
    // documentele stinse de ea) suma e identică cu `Asignat` global de dinainte.
    static decimal AsignatFataDe(IObjectSpace os, Guid stingatorId, Guid contrapartidaId) {
        var stingeri = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == stingatorId)
            .Select(i => new { i.DocumentId, i.Suma }).ToList();
        var idsStinse = stingeri.Select(x => x.DocumentId).Distinct().ToList();
        var laturi = os.GetObjectsQuery<Document>()
            .Where(d => idsStinse.Contains(d.ID))
            .Select(d => new { d.ID, d.PredatorId, d.PrimitorId })
            .ToDictionary(d => d.ID, d => (d.PredatorId, d.PrimitorId));
        var caStingator = stingeri
            .Where(x => laturi.TryGetValue(x.DocumentId, out var l)
                && (l.PredatorId == contrapartidaId || l.PrimitorId == contrapartidaId))
            .Sum(x => x.Suma);
        var caStins = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentId == stingatorId)
            .Select(i => (decimal?)i.Suma).Sum() ?? 0m;
        return caStingator + caStins;
    }
}
