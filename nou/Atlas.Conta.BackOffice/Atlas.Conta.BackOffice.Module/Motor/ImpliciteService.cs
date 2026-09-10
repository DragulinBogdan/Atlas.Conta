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
// REGIMUL E AL PARTENERULUI, COTA E A PRODUSULUI. Aici trăiesc DOAR citirile
// (25b: FK-uri și proiecții, fără navigații lazy și fără `is` pe frunze);
// clasamentul, treapta sărită și împăcarea sunt funcția PURĂ
// `Potrivire.TvaImplicit` (F24-D5).
public static class ImpliciteService {
    /// <summary>
    /// Tipul de TVA propus, sursa lui și motivul — trei date, nu una: clientul
    /// arată sub câmp de unde vine valoarea, iar când NU vine niciuna motivul
    /// spune de ce (partener invizibil, tip inactiv sărit, profil fără politică).
    /// </summary>
    public readonly record struct RezultatImplicit(Guid? TipTvaId, SursaImplicit Sursa, string Motiv);

    /// <summary>
    /// Rezolvarea din F23-D2, pașii 1–6. `data` e data DOCUMENTULUI (nu ziua de
    /// azi): rândurile de politică au valabilitate, iar o factură cu dată în urmă
    /// primește implicitul care era în vigoare atunci.
    /// </summary>
    public static RezultatImplicit TipTva(IObjectSpace os, Guid tipDocumentId,
            Guid? partenerId, Guid? produsId, DateOnly data) =>
        Explica(os, tipDocumentId, partenerId, produsId, data).Rezultat;

    /// <summary>
    /// Aceleași citiri, cu verdictul ÎNTREG (rândul câștigător + candidații cu
    /// motivul lor): ce consumă `GET api/politici/explica`. `TipTva` de mai sus
    /// e wrapper-ul peste `.Rezultat`, ca explicația și culegerea să nu poată
    /// diverge (F24-D6).
    /// </summary>
    internal static PotrivireTvaImplicit Explica(IObjectSpace os, Guid tipDocumentId,
            Guid? partenerId, Guid? produsId, DateOnly data) {
        // Pasul 4: clasa fiscală, din funcția LEGII. `partenerLipsa` acoperă
        // deopotrivă lipsa, inexistența și invizibilitatea — nedistinse (80a).
        ClasaFiscalaPartener? clasa = null;
        Guid? candidatPartener = null;
        var partenerLipsa = true;
        if (partenerId is Guid pid && pid != Guid.Empty) {
            var p = os.GetObjectsQuery<Partener>()
                .Where(x => x.ID == pid)
                .Select(x => new { x.TipPersoana, x.Tara, x.InregistratTva, x.TipTvaImplicitId })
                .FirstOrDefault();
            if (p != null) {
                clasa = ClasaFiscala.APartenerului(p.TipPersoana, p.Tara, p.InregistratTva);
                candidatPartener = p.TipTvaImplicitId;
                partenerLipsa = false;
            }
        }

        // Pasul 1b: rândurile tipului, NEFILTRATE — filtrul de dată și de clasă
        // e al funcției pure, care are de dat un motiv per rând.
        var randuriTip = Fapte.PoliticiTvaImplicit(os, tipDocumentId);

        // Pasul 1c: ancora tipului de document (datoria P1).
        var candidatAncora = os.GetObjectsQuery<TipDocument>()
            .Where(t => t.ID == tipDocumentId)
            .Select(t => t.TipTvaImplicitId)
            .FirstOrDefault();

        // Pasul 2: purtătorul de COTĂ.
        Guid? candidatProdus = null;
        if (produsId is Guid prid && prid != Guid.Empty)
            candidatProdus = os.GetObjectsQuery<Produs>()
                .Where(x => x.ID == prid)
                .Select(x => x.TipTvaImplicitId)
                .FirstOrDefault();

        // O singură interogare pentru toate tipurile candidate: cele trei trepte
        // de subiect/ancoră ȘI tipurile TUTUROR rândurilor de politică — rândul
        // câștigător îl alege funcția pură, care are nevoie de `Activ` pe el.
        var ids = new[] { candidatPartener, candidatAncora, candidatProdus }
            .Where(x => x != null).Select(x => x.Value)
            .Concat(randuriTip.Select(r => r.TipTvaId))
            .Distinct().ToList();

        return Potrivire.TvaImplicit(randuriTip, clasa, data, candidatPartener, candidatAncora,
            candidatProdus, Fapte.TipuriTva(os, ids), partenerLipsa);
    }

    /// <summary>
    /// Partenerul documentului, FĂRĂ `is`/`switch` pe frunze (F23-D2.5,
    /// invariantul II): întrebarea se pune nomenclatorului („e `Partener` cel de
    /// pe latura asta?"), nu documentului („ce fel de document ești?"). Răspunsul
    /// e `null` deopotrivă pentru un repartitor de ALTĂ frunză și pentru un
    /// partener INVIZIBIL pe ușa securizată — exact ce vrea rezolvarea (80a: fără
    /// oracol de existență). Dacă AMBELE laturi ar fi parteneri (nu există azi),
    /// câștigă predatorul.
    ///
    /// Întrebarea se pune prin INTEROGARE, nu prin `GetObjectByKey&lt;Partener&gt;`:
    /// măsurat pe host (F23 pas 2), `BaseObjectSpace.GetObjectByKey&lt;T&gt;` CASTEAZĂ
    /// rândul găsit, deci pe un id de `UnitateInterna` aruncă
    /// `InvalidCastException` („Castle.Proxies.UnitateInternaProxy … to
    /// Partener") în loc să întoarcă null. Cum predatorul e intern pe FCL/RLF/RDC,
    /// varianta cu `GetObjectByKey` scotea 500 pe TOATE scrierile celor cinci
    /// felii cu TVA. `Any` pe nomenclator răspunde la aceeași întrebare fără cast.
    /// </summary>
    public static Guid? PartenerulDocumentului(IObjectSpace os, Document doc) {
        if (doc == null)
            return null;
        if (EstePartener(os, doc.PredatorId))
            return doc.PredatorId;
        if (EstePartener(os, doc.PrimitorId))
            return doc.PrimitorId;
        return null;
    }

    static bool EstePartener(IObjectSpace os, Guid id) =>
        id != Guid.Empty && os.GetObjectsQuery<Partener>().Any(p => p.ID == id);
}
