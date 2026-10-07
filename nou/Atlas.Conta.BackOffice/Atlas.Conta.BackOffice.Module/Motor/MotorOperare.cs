using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Gardienii opresc operațiunea cu mesaj pentru utilizator (XAF o afișează curat).
public class OperareException : UserFriendlyException {
    public OperareException(string message) : base(message) { }
}

/// <summary>
/// Coaja comenzii (14, 33d): gardurile, contractul declarației, materializarea în cub și
/// scrierile pe document. Consumă numai clasa de bază; fiecare metodă publică e o tranzacție.
/// </summary>
public static class MotorOperare {
    /// <summary>
    /// Refuzurile operării, fără nimic persistat. Modifică obiectele din ObjectSpace-ul primit
    /// (<c>PregatesteOperare</c>): apelantul îl aruncă după apel. Valorile lăsate pe linii sunt estimări.
    /// </summary>
    public static IReadOnlyList<string> Valideaza(IObjectSpace os, Document doc) {
        using var receptie = Cub.ReceptiiConexe.IncepeCitirea(os, doc, blocheaza: false);
        try {
            var (tipDoc, _) = PregatesteSiValideaza(os, doc);
            return Cub.Materializare.Refuzuri(os, doc, tipDoc);
        }
        catch (OperareException ex) {
            return ex.Message
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();
        }
    }

    static (TipDocument Tip, Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> ClaseTip)
            PregatesteSiValideaza(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException("Doar un document în starea Draft poate fi operat.");
        // F27-D4: căile care nu culeg câmpul intră cu `default`.
        if (doc.DataInregistrare == default)
            doc.DataInregistrare = doc.Data;
        if (doc.DataInregistrare < doc.Data)
            throw new OperareException("Data înregistrării nu poate preceda data documentului.");
        GardianPerioada.VerificaDeschisa(os, doc.DataInregistrare);
        var tipDoc = GasesteTipDocument(os, doc);
        Refuza(Cub.Materializare.MotivNuDeclara(os, doc, tipDoc));

        // F13-D1: `PregatesteOperare` aduce la 0 taxa culeasă pe taxare inversă la livrare; gardul o judecă pe valoarea dinainte.
        var tvaCulesInainte = Liniile(doc)
            .Select((d, i) => (Linie: d, Pozitie: i + 1, TvaCules: d.TvaCules ? d.ValoareTva : 0m))
            .ToList();

        doc.PregatesteOperare(os);
        var claseTip = Fapte.ClaseTip(os, doc.Detalii.Select(d => d.TipMaterialId));

        var erori = new List<string>();
        TvaService.VerificaTvaCulesTaxareInversa(os, tipDoc, tvaCulesInainte, erori);
        ValideazaDeclarativ(os, doc, tipDoc, claseTip, erori);
        doc.ValideazaOperare(os, erori);
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));
        return (tipDoc, claseTip);
    }

    /// <summary>Operează documentul; întoarce documentul conex sau secundar generat, dacă există.</summary>
    public static Document Opereaza(IObjectSpace os, Document doc) {
        using var tranzactie = TranzactieComanda.Asigura(os);
        FiscalitateService.BlocheazaScrierea(os, doc);
        using var receptie = Cub.ReceptiiConexe.IncepeCitirea(os, doc, blocheaza: true);
        var (tipDoc, claseTip) = PregatesteSiValideaza(os, doc);
        var refuzuri = Cub.Materializare.Refuzuri(os, doc, tipDoc);
        if (refuzuri.Count > 0)
            throw new OperareException(string.Join("\n", refuzuri));

        // 33d: prima scriere pe document vine după ultimul refuz citit fără scriere.
        AsignaNumar(os, doc, tipDoc);
        AplicaScadenta(os, doc, tipDoc);
        FinalizeazaLoturile(os, doc);
        if (doc is IDocumentCuEfecteProprii propriu)
            propriu.LaOperare(os);

        Document conex = null;
        var politicaConex = Fapte.Conex(os, tipDoc.ID);
        if (politicaConex != null)
            conex = GenereazaConex(os, doc, politicaConex.Value, claseTip);

        var secundar = doc.GenereazaSecundar(os);
        if (secundar != null) {
            secundar.DocumentSursa = doc;
            secundar.Autogenerat = true;
        }

        doc.Stare = StareDocument.Operat;
        doc.DataOperare = DateTime.UtcNow;

        ScrieValorileEvaluate(doc, Cub.Materializare.Opereaza(os, doc, tipDoc));
        ImperechereService.CreeazaAutomataLaOperare(os, doc);

        os.CommitChanges();
        tranzactie?.Commit();
        return conex ?? secundar;
    }

    // 13, 26e: lotul născut de o linie își primește prețul de intrare și data la operare.
    static void FinalizeazaLoturile(IObjectSpace os, Document doc) {
        var idsDetalii = Liniile(doc).Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>()
                .Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            var linie = doc.Detalii.First(d => d.ID == lot.LinieIntrareId);
            if (linie.Cantitate <= 0)
                throw new OperareException("O linie care creează lot trebuie să aibă cantitate pozitivă.");
            lot.PretUnitar = Scara.RotunjestePret(linie.Valoare / linie.Cantitate);
            lot.Data = doc.DataInregistrare;
            if (linie is ILinieCuAtributeLot atribute) {
                lot.DataExpirare = atribute.DataExpirare;
                lot.LotFabricatie = atribute.LotFabricatie;
            }
        }
    }

    // D9-D3 (a): linia ieșirii evaluate din sold poartă valoarea deciziei, cu semnul cantității ei.
    static void ScrieValorileEvaluate(Document doc, Nucleu.Contract contract) {
        foreach (var iesiri in contract.Decizii.OfType<Nucleu.ValoareIesire>().GroupBy(d => d.Linie)) {
            var linie = doc.Detalii.First(d => d.ID == iesiri.Key);
            linie.Valoare = Math.Sign(linie.Cantitate) * iesiri.Sum(d => d.Valoare);
        }
    }

    // Obligativitățile per tip din PoliticaValidare (3d): reguli de PROFIL, nu
    // de structură (decizia 29) — la privat rândurile lipsesc și nimic nu se
    // cere. Angajamentul SAU codul economic satisface clasificația bugetară
    // (aceeași alternativă ca în validarea hardcodată pe care o înlocuiește).
    static void ValideazaDeclarativ(IObjectSpace os, Document doc, TipDocument tipDoc,
        Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip,
        ICollection<string> erori) {
        var politica = os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipDoc.ID);
        if (politica == null)
            return;
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            if (politica.CereClasificatieBugetara && d.AngajamentId == null && d.DimensiuniCulese().CodEconomicId == null)
                erori.Add($"Linia cu {info.Denumire} cere clasificație bugetară: angajament sau cod economic.");
            if (politica.NaturaInterzisa != null && info.Natura == politica.NaturaInterzisa)
                erori.Add($"Liniile cu natura {politica.NaturaInterzisa} nu sunt permise pe {tipDoc.Cod} (linia cu {info.Denumire}).");
        }
    }

    // Clonarea 00 §6: header (cu InverseazaLaturi), DOAR liniile care trec
    // filtrul de natură; liniile clonate poartă aceleași loturi și dimensiuni.
    // Fără linii eligibile nu se generează nimic (o factură doar de servicii
    // nu produce NIR).
    static Document GenereazaConex(IObjectSpace os, Document sursa, PoliticaConexFapt politica,
        Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip) {
        var linii = Liniile(sursa)                                                   // S-D6
            .Where(d => Potrivire.Conex(politica, Fapte.Linie(d, claseTip)))
            .ToList();
        if (linii.Count == 0)
            return null;

        var tipTinta = os.GetObjectByKey<TipDocument>(politica.TipDocumentTintaId);
        var tipClr = typeof(Document).Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == tipTinta.ClrType && typeof(Document).IsAssignableFrom(t))
            ?? throw new OperareException($"Clasa documentului conex ({tipTinta?.ClrType}) nu există.");
        var conex = (Document)os.CreateObject(tipClr);
        conex.Data = sursa.Data;
        conex.DataInregistrare = sursa.DataInregistrare;
        conex.PredatorId = politica.InverseazaLaturi ? sursa.PrimitorId : sursa.PredatorId;
        conex.PrimitorId = politica.InverseazaLaturi ? sursa.PredatorId : sursa.PrimitorId;
        conex.DocumentSursa = sursa;
        conex.Autogenerat = true;
        conex.PreiaSursaConexa(sursa);
        // DIM-2: liniile clonei se nasc pe FRUNZA declarată a țintei ([TipDetaliu]
        // — aceeași declarație pe care o consumă UI-ul, 40a); o linie de bază ar
        // face PreiaDimensiuni no-op și clona ar pierde dimensiunile culese.
        var tipDetaliu = tipClr.GetCustomAttribute<UI.TipDetaliuAttribute>(inherit: false)?.TipDetaliu
            ?? typeof(DocumentDetaliu);
        foreach (var s in linii) {
            var d = (DocumentDetaliu)os.CreateObject(tipDetaliu);
            d.Document = conex;
            d.TipMaterialId = s.TipMaterialId;
            d.LotId = s.LotId;
            d.Cantitate = s.Cantitate;
            d.Valoare = s.Valoare;
            // TipTva se clonează ca informație; ValoareTva NU — TVA-ul liniei
            // se postează pe documentul sursă (P1, design §4/§6: NIR-ul duce
            // netul, factura duce rândurile 4426).
            d.TipTvaId = s.TipTvaId;
            d.AngajamentId = s.AngajamentId;
            d.PreiaDimensiuni(s.DimensiuniCulese());
            conex.PreiaLinieConexa(s, d);
        }
        return conex;
    }

    /// <summary>Corecția directă (14): întoarcerea în Draft, numai fără dependenți și în perioadă deschisă.</summary>
    public static void AnuleazaOperarea(IObjectSpace os, Document doc) {
        using var tranzactie = TranzactieComanda.Asigura(os);
        FiscalitateService.BlocheazaScrierea(os, doc);
        FiscalitateService.VerificaAnularea(os, doc);
        Cub.Materializare.BlocheazaFise(os, doc);
        Cub.Materializare.BlocheazaDocumente(os, doc);
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi anulat.");
        GardianPerioada.VerificaDeschisa(os, doc.DataInregistrare);
        VerificaFaraLaturaPerecheOperata(os, doc);
        VerificaFaraConexeOperate(os, doc);
        VerificaFaraImperecheri(os, doc);
        Cub.Citiri.Loturi.VerificaRetragere(os, doc);
        StergeConexeDraftAutogenerate(os, doc);

        Refuza(MotivLoturiFolosite(os, doc));

        if (doc is IDocumentCuEfecteProprii propriu)
            propriu.LaAnulare(os);
        Cub.Materializare.Anuleaza(os, doc);
        doc.Stare = StareDocument.Draft;
        doc.DataOperare = null;
        os.CommitChanges();
        tranzactie?.Commit();
    }

    /// <summary>
    /// Stornează documentul la data dată (14). <paramref name="inversaFiscala"/> e atribuirea
    /// fiscală a inversei, când stornarea e a unei corecții de eroare materială (D9-A5).
    /// </summary>
    public static void Storneaza(IObjectSpace os, Document doc, DateOnly dataStorno,
            FiscalitateService.Atribuire inversaFiscala = null) {
        using var tranzactie = TranzactieComanda.Asigura(os);
        FiscalitateService.BlocheazaScrierea(os, doc);
        Cub.Materializare.BlocheazaFise(os, doc);
        Cub.Materializare.BlocheazaDocumente(os, doc);
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi stornat.");
        if (dataStorno < doc.DataInregistrare)
            throw new OperareException("Data stornării nu poate preceda data înregistrării documentului.");
        GardianPerioada.VerificaDeschisa(os, dataStorno);
        VerificaFaraLaturaPerecheOperata(os, doc);
        VerificaFaraConexeOperate(os, doc);
        Cub.Citiri.Loturi.VerificaRetragere(os, doc, dataStorno);
        ImperechereService.InverseazaLaStorno(os, doc, dataStorno);                   // F27-D8
        StergeConexeDraftAutogenerate(os, doc);

        if (doc is IDocumentCuEfecteProprii propriu)
            propriu.LaStornare(os, dataStorno);

        Cub.Materializare.Storneaza(os, doc, dataStorno, inversaFiscala);

        doc.Stare = StareDocument.Stornat;
        os.CommitChanges();
        tranzactie?.Commit();
    }

    // Oglinda exactă a gardianului de grup conex pentru legătura de pereche
    // (F8-D9): pointer-ul ≈ copil, ținta ≈ sursă. Piciorul care DECLARĂ legătura
    // e frunza și se anulează liber (nimeni nu depinde de el); ținta nu, cât timp
    // pointer-ul e OPERAT — altfel ținta ar reveni în Draft, s-ar re-opera și ar
    // genera din nou o pereche, lângă cea deja operată: dublă postare pe 581.
    //
    // Se apelează ÎNAINTEA gardianului de grup conex, deliberat: perechea
    // autogenerată e acoperită de amândoi (are și `DocumentSursaId`, și link),
    // iar mesajul ăsta e cel specific — „latura pereche" spune operatorului
    // exact ce vede pe ecran.
    static void VerificaFaraLaturaPerecheOperata(IObjectSpace os, Document doc) =>
        Refuza(MotivLaturaPerecheOperata(os, doc));

    // Anularea/stornarea operează pe grupul conex (00 §8): copiii cu registre
    // (Operat) se anulează/stornează întâi — refuz conservator; copiii DRAFT
    // autogenerați sunt un artefact al operării anulate și se șterg odată cu ea
    // (re-operarea sursei generează un draft proaspăt).
    static void VerificaFaraConexeOperate(IObjectSpace os, Document doc) =>
        Refuza(MotivConexeOperate(os, doc));

    // Stingerea leagă REGISTRELE celor două documente (decizia 17); anularea
    // sau stornarea uneia dintre părți ar lăsa imperecherea fără acoperire —
    // refuz conservator: utilizatorul șterge întâi imperecherile (link simplu,
    // fără registre proprii), apoi corectează documentul.
    static void VerificaFaraImperecheri(IObjectSpace os, Document doc) =>
        Refuza(MotivImperecheri(os, doc));

    internal static void Refuza(string motiv) {
        if (motiv != null)
            throw new OperareException(motiv);
    }

    // Motivele dependenților, citite și de regimul pe stare (106b, 106k): null = liber.
    public static string MotivLaturaPerecheOperata(IObjectSpace os, Document doc) =>
        os.GetObjectsQuery<DocumentTrezorerie>()
            .Any(x => x.LaturaPerecheId == doc.ID && x.Stare == StareDocument.Operat)
            ? "Documentul e declarat latura pereche a unui virament încă operat — anulați/stornați întâi acel document."
            : null;

    public static string MotivConexeOperate(IObjectSpace os, Document doc) =>
        os.GetObjectsQuery<Document>().Any(x => x.DocumentSursaId == doc.ID && x.Stare == StareDocument.Operat)
            ? "Documentul are documente generate (conexe) încă operate — anulați/stornați întâi acele documente."
            : null;

    internal const string MesajImperecheri =
        "Documentul are imperecheri (stingeri) — ștergeți-le întâi, apoi anulați/stornați.";

    public static string MotivImperecheri(IObjectSpace os, Document doc) =>
        os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == doc.ID || i.DocumentId == doc.ID)
            ? MesajImperecheri
            : null;

    /// <summary>Loturile născute de liniile documentului nu au mișcări ale altor documente; null = liber.</summary>
    public static string MotivLoturiFolosite(IObjectSpace os, Document doc) {
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == doc.ID).Select(d => d.ID);
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.LinieIntrareId != null && linii.Contains(l.LinieIntrareId.Value))
            .Select(l => new { l.ID, l.Produs.Denumire, l.Data }).ToList();
        if (loturi.Count == 0)
            return null;
        var ids = loturi.Select(l => l.ID).ToList();
        var folosite = Cub.Citiri.Loturi.Postari(os)
            .Where(r => r.Unitate != null && ids.Contains(r.Unitate.Value) && r.DocumentId != doc.ID)
            .Select(r => r.Unitate.Value).Distinct().ToHashSet();
        var lot = loturi.FirstOrDefault(l => folosite.Contains(l.ID));
        return lot == null ? null
            : $"Lotul {lot.Denumire} din {lot.Data:yyyy-MM-dd} e folosit de alte documente — folosiți storno.";
    }

    static void StergeConexeDraftAutogenerate(IObjectSpace os, Document doc) {
        var stersi = false;
        foreach (var copil in os.GetObjectsQuery<Document>()
            .Where(x => x.DocumentSursaId == doc.ID && x.Autogenerat && x.Stare == StareDocument.Draft).ToList()) {
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
            os.Delete(copil);
            stersi = true;
        }
        // Review advers F5-F4: de când draftul conex e editabil (F5-D8b), operatorul
        // poate adăuga pe el linii MANUALE, iar acelea nasc loturi proprii la
        // culegere. Ștergerea de aici se face în ObjectSpace-ul non-secured al
        // motorului, unde nu rulează niciun controller de culegere — fără pasul
        // ăsta loturile lor ar rămâne orfane (preț 0, dată 0001-01-01, fără linie
        // mamă), permanent în nomenclator și în lookup-uri. Fără impact contabil
        // (fără rânduri de stoc nu intră în picking), dar e exact zgomotul pe
        // care curățenia există să-l prevină.
        if (stersi)
            LoturiCulegereService.CurataOrfane(os);
    }

    // S-D6: aceeași secvență a liniilor în ambele motoare (`Fapte.Operand`).
    static IEnumerable<DocumentDetaliu> Liniile(Document doc) =>
        doc.Detalii.OrderBy(d => d.Pozitie).ThenBy(d => d.ID);

    // Ancora TipDocument după numele CLR al clasei — reutilizabilă (motor,
    // DescarcareService, TvaService): totul se cheiază pe TipDocument.ID.
    internal static TipDocument GasesteTipDocument(IObjectSpace os, string clrType) =>
        os.FirstOrDefault<TipDocument>(t => t.ClrType == clrType)
            ?? throw new OperareException($"Lipsește ancora TipDocument pentru clasa {clrType} (seed).");

    internal static TipDocument GasesteTipDocument(IObjectSpace os, Document doc) =>
        GasesteTipDocument(os, ClasaReala(doc).Name);

    // EF Core dă proxy-uri de change-tracking — clasa reală e pe tipul de bază.
    // Publică de la F27-D6: gate-ul de creare al corecției întreabă pe TIPUL
    // CONCRET al documentului, iar tierul REST e alt assembly.
    public static Type ClasaReala(Document doc) {
        var tip = doc.GetType();
        while (tip.Assembly.IsDynamic || tip.Name.EndsWith("Proxy"))
            tip = tip.BaseType;
        return tip;
    }

    static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, GardContareAttribute>
        garduriContare = new();

    internal static GardContareAttribute GardContare(Document doc) =>
        garduriContare.GetOrAdd(ClasaReala(doc), t => t.GetCustomAttribute<GardContareAttribute>(false));

    static void AsignaNumar(IObjectSpace os, Document doc, TipDocument tipDoc) {
        if (!string.IsNullOrWhiteSpace(doc.Numar))
            return;
        var politica = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipDoc.ID);
        if (politica == null)
            throw new OperareException(
                $"Documentul nu are număr și tipul {tipDoc.Cod} nu are politică de numerotare.");
        var n = politica.UrmatorulNumar;
        politica.UrmatorulNumar = n + 1;
        doc.Numar = string.IsNullOrWhiteSpace(politica.Format)
            ? $"{politica.Serie}{n}"
            : string.Format(politica.Format, n, politica.Serie);
    }

    // Scadența cu default de politică (inventar 07): se aplică doar când nu a
    // fost culeasă — politica e default, nu constrângere. Tipurile fără rând de
    // politică (ex. FCT — scadența furnizorului se culege) rămân neatinse.
    static void AplicaScadenta(IObjectSpace os, Document doc, TipDocument tipDoc) {
        if (doc is not IDocumentCuScadenta scadenta || scadenta.DataScadenta != null)
            return;
        var politica = os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipDoc.ID);
        if (politica != null)
            scadenta.DataScadenta = doc.Data.AddDays(politica.ZileDefault);
    }
}
