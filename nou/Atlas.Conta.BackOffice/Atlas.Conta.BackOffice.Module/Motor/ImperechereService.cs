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
//
// De la F19-D16 plafonul are și LATURĂ: e defalcat per (contrapartidă × SENS),
// iar sensul consumat e al documentului STINS (`Document.SensDeStins`, al
// treilea hook polimorf al rolului). Fără el o notă 401 = 4111 de 60 pe X avea
// plafon 120 și îl consuma INTEGRAL pe două documente de aceeași natură —
// tavanul de 120 e corect (60 datorie + 60 creanță), lipsea regula că fiecare
// jumătate se consumă pe latura ei.
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
    //
    // `contrapartidaId` (F19-D16) = alegerea EXPLICITĂ a grupului de plafon, când
    // apelantul o știe (panoul de compensare afișează candidații grupați per
    // contrapartidă × sens). `null` = se deduce; deducția REFUZĂ ambiguitatea în
    // loc s-o rezolve tăcut — vezi `ValideazaCreare`.
    public static Imperechere Imperecheaza(IObjectSpace os,
        Document stingator, Document document, decimal suma, Guid? contrapartidaId = null) {
        var imperechere = Creeaza(os, stingator, document, suma, autogenerat: false, contrapartidaId);
        os.CommitChanges();
        return imperechere;
    }

    // Fără commit — motorul o cheamă din tranzacția operării plății autogenerate.
    internal static Imperechere Creeaza(IObjectSpace os,
        Document stingator, Document document, decimal suma, bool autogenerat,
        Guid? contrapartidaId = null) {
        // Rotunjirea ÎNAINTE de validare, nu după: altfel suma validată contra
        // restului n-ar fi cea persistată în `numeric(18,2)` și o stingere ar
        // putea depăși plafonul cu bani mărunți (`Scara`).
        suma = Scara.RotunjesteBani(suma);
        // Validarea rulează ÎNAINTE de CreateObject — comportamentul existent
        // (motorul nu lasă rând-fantomă pe eșec) rămâne exact.
        ValideazaCreare(os, stingator, document, suma, contrapartidaId);
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
        Document stingator, Document document, decimal suma, Guid? contrapartidaId = null) {
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

        // Rolul de STINS e la fel de polimorf ca rolul de stingător (F7-D5):
        // viramentul intern nu închide nicio datorie, deci nu se stinge. Nu e
        // redundant cu invariantul de contrapartidă de mai jos — capacitățile
        // unei note contabile sunt repartitorii expliciți ai liniilor ei, deci
        // pot cădea pe ORICE latură, inclusiv pe conturile proprii ale unui
        // picior de virament.
        if (!document.PoateFiStins(os))
            throw new OperareException(
                "Documentul nu poate fi stins: un virament intern nu închide nicio datorie sau creanță "
                + "(contul de tranzit se închide singur când ambele picioare sunt operate).");

        // Contrapartida stingătorului (furnizor/client/angajat) trebuie să apară
        // pe documentul stins — echivalentul grupării pe partener din legacy
        // (spDecontariObligatii); acoperă și lanțul avans↔decont↔regularizare.
        var peLaturi = capacitati.Keys
            .Where(k => k == document.PredatorId || k == document.PrimitorId)
            .ToList();
        if (peLaturi.Count == 0)
            throw new OperareException(
                "Documentul care stinge și documentul stins nu împart aceeași contrapartidă (partener/angajat).");
        // Alegerea EXPLICITĂ a apelantului bate deducția (F19-D16, a doua axă):
        // panoul de compensare știe sub ce grup a afișat candidatul, motorul n-are
        // de unde. Până la F19-D16 se lua „primul key care se potrivește cu o
        // latură", iar cheile se umplu debit-întâi, în ordinea liniilor: un
        // document care poartă DOUĂ dintre contrapartidele notei pe cele două
        // laturi ale lui consuma plafonul ALTUI grup decât cel afișat.
        if (contrapartidaId is Guid ceruta) {
            if (!peLaturi.Contains(ceruta))
                throw new OperareException(
                    "Contrapartida cerută nu apare și pe documentul care stinge, și pe laturile documentului stins.");
            peLaturi = new List<Guid> { ceruta };
        }

        // ═══ Plafonul are LATURĂ (F19-D16) ═══
        // Fiecare jumătate a plafonului se consumă pe latura ei. Ce fel de sold
        // poartă documentul stins e decizia TIPULUI lui (`SensDeStins`, hook
        // polimorf ca `CapacitateStingere`/`PoateFiStins`) — motorul nu cunoaște
        // niciun tip. Un tip care NU declară lasă alegerea deschisă; atunci ea
        // trebuie să fie UNICĂ, altfel se refuză: niciodată „prima jumătate".
        var sensCerut = document.SensDeStins(os);
        var perechi = new List<(Guid Contrapartida, SensStingere Sens)>();
        foreach (var cp in peLaturi) {
            var plafon = capacitati[cp];
            if (sensCerut is SensStingere cerut) {
                if (plafon[cerut] != 0m)
                    perechi.Add((cp, cerut));
            } else {
                foreach (var sens in plafon.Sensuri)
                    perechi.Add((cp, sens));
            }
        }
        if (perechi.Count == 0)
            throw new OperareException(
                $"Documentul care stinge n-are capacitate pe sensul cerut de documentul stins ({Eticheta(sensCerut)}): "
                + "plata stinge datorii, încasarea stinge creanțe, iar nota de compensare le face pe amândouă — "
                + "fiecare jumătate pe latura ei.");
        if (perechi.Select(p => p.Contrapartida).Distinct().Count() > 1)
            throw new OperareException(
                "Documentul stins poartă MAI MULTE dintre contrapartidele documentului care stinge, pe laturi "
                + "diferite — alegeți explicit contrapartida (grupul de plafon), altfel stingerea ar consuma un "
                + "plafon la întâmplare.");
        if (perechi.Count > 1)
            // Refuz ACȚIONABIL (review F4): spune și ce are apelantul de făcut.
            // Ieșirea NU e un câmp `Sens` pe care apelantul să-l aleagă — ar fi
            // exact arbitrarul pe care refuzul există ca să-l oprească —, ci
            // MODELAREA: tipul care chiar poate fi stins își declară natura
            // soldului (`SensDeStins`), ca facturile, decontul, trezoreria și
            // NIR-ul (F19-D16).
            throw new OperareException(
                "Documentul stins nu declară ce fel de sold poartă pe contul contrapartidei, iar documentul care "
                + "stinge are capacitate pe AMBELE sensuri față de ea (și datorie, și creanță) — jumătatea "
                + "consumată ar fi arbitrară. Stingeți-l cu un document care are o singură jumătate față de "
                + "contrapartida asta (plata stinge datorii, încasarea stinge creanțe), sau declarați natura "
                + "soldului pe tipul documentului stins.");

        var (contrapartida, sensAles) = perechi[0];
        var ramasStingator = capacitati[contrapartida][sensAles]
            - AsignatFataDe(os, stingator.ID, contrapartida, sensAles);
        if (suma > ramasStingator)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul neasignat al documentului care stinge, "
                + $"pe sensul {Eticheta(sensAles)} ({ramasStingator:0.##}).");
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
    //
    // PUBLICĂ de la F19 (panoul de compensare al notei, `NotaContabilaApply.
    // Candidati`): plafonul afișat = `CapacitateStingere[cp][sens] −
    // AsignatFataDe(cp, sens)`, adică EXACT cele două cifre pe care le compară
    // `ValideazaCreare` mai sus.
    // Partajarea funcției e deliberată — o formulă rescrisă în felia de citire ar
    // fi „al doilea adevăr" al plafonului și ar propune (sau ar ascunde) stingeri
    // pe care serverul le tratează invers.
    public static decimal AsignatFataDe(IObjectSpace os, Guid stingatorId, Guid contrapartidaId,
        SensStingere sens) {
        var stingeri = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == stingatorId)
            .Select(i => new { i.DocumentId, i.Suma }).ToList();
        var idsStinse = stingeri.Select(x => x.DocumentId).Distinct().ToList();
        // POLIMORF, o SINGURĂ interogare pe o mulțime MĂRGINITĂ (documentele
        // stinse de ACEST stingător): avem nevoie și de laturi, și de
        // `SensDeStins`, care e hook de TIP. 60b interzice rezoluția polimorfă
        // PER RÂND (grile de mii de documente); aici e un `IN` pe câteva id-uri,
        // pe o cale de COMANDĂ. Alternativa — o coloană `Sens` pe `Imperechere` —
        // ar persista o valoare DERIVATĂ din tipul documentului stins, cu
        // migrație și backfill, pentru un câștig pe care nicio cifră nu-l cere (59).
        var stinse = os.GetObjectsQuery<Document>()
            .Where(d => idsStinse.Contains(d.ID))
            .ToList()
            .ToDictionary(d => d.ID);
        var caStingator = 0m;
        foreach (var x in stingeri) {
            if (!stinse.TryGetValue(x.DocumentId, out var d))
                continue;
            if (d.PredatorId != contrapartidaId && d.PrimitorId != contrapartidaId)
                continue;
            // Un document care NU declară un sens s-a putut consuma din oricare
            // jumătate, deci se scade din AMBELE: conservator prin construcție —
            // nu deschide plafon nicăieri. Pentru trezorerie (o contrapartidă,
            // un sens) cifra e IDENTICĂ cu cea de dinainte de F19-D16.
            if (d.SensDeStins(os) is SensStingere sensStins && sensStins != sens)
                continue;
            caStingator += x.Suma;
        }
        var caStins = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentId == stingatorId)
            .Select(i => (decimal?)i.Suma).Sum() ?? 0m;
        return caStingator + caStins;
    }

    // Eticheta de mesaj a sensului: refuzurile motorului sunt de DOMENIU (le
    // citește un contabil), nu nume de enum.
    static string Eticheta(SensStingere? sens) => sens switch {
        SensStingere.Datorie => "datorie",
        SensStingere.Creanta => "creanță",
        _ => "nedeclarat"
    };
}
