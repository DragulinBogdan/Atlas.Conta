using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Gardienii opresc operațiunea cu mesaj pentru utilizator (XAF o afișează curat).
public class OperareException : UserFriendlyException {
    public OperareException(string message) : base(message) { }
}

// Decizia 14: motorul generic de operare — consumă DOAR clasa de bază; ce e
// specific tipului intră prin hooks (PregatesteOperare/ValideazaOperare) și
// prin politici (RegulaStoc/RegulaContare). Fiecare metodă publică e o
// tranzacție: un singur CommitChanges la final.
//
// Limitare asumată (single-operator back-office): verificarea de sold și
// commit-ul nu sunt serializate între utilizatori concurenți; la nevoie se
// adaugă advisory lock Postgres per cheie de stoc — aditiv, doar aici.
public static class MotorOperare {
    // Întoarce documentul conex generat (draft autogenerat, decizia 17) sau null.
    public static Document Opereaza(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException("Doar un document în starea Draft poate fi operat.");
        GardianPerioada.VerificaDeschisa(os, doc.Data);

        doc.PregatesteOperare(os);
        var erori = new List<string>();
        doc.ValideazaOperare(os, erori);
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));

        var tipDoc = GasesteTipDocument(os, doc);
        AsignaNumar(os, doc, tipDoc);

        // Clasa/natura/contul fiecărui Tip de pe linii, preîncărcate — motorul nu
        // se bazează pe navigații (contextul apelant nu garantează lazy loading).
        var idsTip = doc.Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var claseTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura, t.Denumire, t.ContImplicitId })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura, t.Denumire, t.ContImplicitId));

        // 1. Mișcările de stoc se CALCULEAZĂ întâi (delta), gardianul de sold
        //    le verifică, abia apoi se materializează rândurile. Per latură,
        //    regula specifică pe Clasa liniei bate regula generică (Clasa=null =
        //    orice clasă cu Natura=Stoc) — altfel s-ar aplica amândouă.
        var reguliStoc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        var miscari = new List<(DocumentDetaliu Detaliu, RegulaStoc Regula, MiscareStoc Miscare)>();
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            foreach (var latura in reguliStoc.GroupBy(r => r.Latura)) {
                var aplicabile = latura.Where(r => r.ClasaId != null && r.ClasaId == info.ClasaId).ToList();
                if (aplicabile.Count == 0 && info.Natura == NaturaClasa.Stoc)
                    aplicabile = latura.Where(r => r.ClasaId == null).ToList();
                foreach (var regula in aplicabile) {
                    if (d.LotId == null)
                        throw new OperareException(
                            $"Linia cu {info.Denumire} intră în regulile de stoc dar nu are lot.");
                    var repartitorId = regula.Latura == LaturaDocument.Predator ? doc.PredatorId : doc.PrimitorId;
                    miscari.Add((d, regula, new MiscareStoc(
                        new CheieStoc(d.LotId.Value, repartitorId, regula.TipStoc), doc.Data, regula.Semn * d.Cantitate)));
                }
            }
        }
        StocService.VerificaSoldIntermediar(os, miscari.Select(m => m.Miscare).ToList());

        foreach (var (detaliu, regula, miscare) in miscari) {
            var rand = os.CreateObject<RegistruStoc>();
            rand.Data = doc.Data;
            rand.TipStoc = miscare.Cheie.TipStoc;
            rand.LotId = miscare.Cheie.LotId;
            rand.RepartitorId = miscare.Cheie.RepartitorId;
            rand.Cantitate = miscare.Cantitate;
            rand.Valoare = regula.Semn * detaliu.Valoare;
            rand.Document = doc;
            rand.Detaliu = detaliu;
        }

        // 2. Finalizarea loturilor născute de liniile documentului (NIR manual,
        //    FacturaIntrare pentru lanțul conex, plus de inventar, producție):
        //    lotul e creat la culegere de linia de intrare (baza nu poartă
        //    ProdusId — testul bazei §2); motorul îi fixează prețul
        //    (= Valoare/Cantitate, decizia 13), data și atributele culese.
        var idsDetalii = doc.Detalii.Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>().Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            var linie = doc.Detalii.First(d => d.ID == lot.LinieIntrareId);
            if (linie.Cantitate <= 0)
                throw new OperareException("O linie care creează lot trebuie să aibă cantitate pozitivă.");
            lot.PretUnitar = linie.Valoare / linie.Cantitate;
            lot.Data = doc.Data;
            if (linie is ILinieCuAtributeLot atribute) {
                lot.DataExpirare = atribute.DataExpirare;
                lot.LotFabricatie = atribute.LotFabricatie;
            }
        }

        // 3. Rândurile contabile: potrivirea regulii pe linie = TipMaterial exact
        //    → NaturaFiltru → regula generică; fără regulă = linia nu contează pe
        //    acest tip de document (NotaTransfer — 23c; liniile de stoc pe FCT —
        //    recepția contează pe NIR). Conturile se rezolvă din sursa declarată
        //    (TipMaterial / partenerul unei laturi), cu contul explicit fallback.
        var reguliContare = os.GetObjectsQuery<RegulaContare>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        var partenerPredator = os.GetObjectByKey<Repartitor>(doc.PredatorId) as Partener;
        var partenerPrimitor = os.GetObjectByKey<Repartitor>(doc.PrimitorId) as Partener;
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            var regula = reguliContare.FirstOrDefault(r => r.TipMaterialId == d.TipMaterialId)
                ?? reguliContare.FirstOrDefault(r => r.TipMaterialId == null && r.NaturaFiltru == info.Natura)
                ?? reguliContare.FirstOrDefault(r => r.TipMaterialId == null && r.NaturaFiltru == null);
            if (regula == null)
                continue;
            var contDebit = RezolvaCont(regula.SursaContDebit, regula.ContDebitId,
                    info.ContImplicitId, partenerPredator, partenerPrimitor)
                ?? throw new OperareException(
                    $"Contul debitor nu se poate rezolva pentru linia cu {info.Denumire} ({tipDoc.Cod}, sursă {regula.SursaContDebit}).");
            var contCredit = RezolvaCont(regula.SursaContCredit, regula.ContCreditId,
                    info.ContImplicitId, partenerPredator, partenerPrimitor)
                ?? throw new OperareException(
                    $"Contul creditor nu se poate rezolva pentru linia cu {info.Denumire} ({tipDoc.Cod}, sursă {regula.SursaContCredit}).");
            var rand = os.CreateObject<RegistruContabil>();
            rand.Data = doc.Data;
            rand.ContDebitId = contDebit;
            rand.ContCreditId = contCredit;
            rand.Valoare = d.Valoare;
            rand.DimensiuniDebit = DimensiuniResolver.Rezolva(d.Dimensiuni, regula.DimensiuniOverrideDebit,
                regula.DimensiuniComun, new Dimensiuni { RepartitorId = doc.PredatorId });
            rand.DimensiuniCredit = DimensiuniResolver.Rezolva(d.Dimensiuni, regula.DimensiuniOverrideCredit,
                regula.DimensiuniComun, new Dimensiuni { RepartitorId = doc.PrimitorId });
            rand.Document = doc;
            rand.Detaliu = d;
        }

        // 4. Documentul conex (decizia 17, 00 §6): draft autogenerat în aceeași
        //    tranzacție cu operarea sursei; utilizatorul îl completează și îl
        //    operează separat (abia atunci mișcă registre și primește număr).
        Document conex = null;
        var politicaConex = os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipDoc.ID);
        if (politicaConex != null)
            conex = GenereazaConex(os, doc, politicaConex,
                doc.Detalii.ToDictionary(d => d.ID, d => claseTip.GetValueOrDefault(d.TipMaterialId).Natura));

        doc.Stare = StareDocument.Operat;
        doc.DataOperare = DateTime.UtcNow;
        os.CommitChanges();
        return conex;
    }

    // Sursa declarativă a contului unei laturi (testul bazei §7.2); contul
    // explicit al regulii e valoare directă sau fallback când sursa nu rezolvă.
    static Guid? RezolvaCont(SursaCont sursa, Guid? contExplicit, Guid? contTipMaterial,
        Partener partenerPredator, Partener partenerPrimitor) => sursa switch {
        SursaCont.TipMaterial => contTipMaterial ?? contExplicit,
        SursaCont.PartenerPredator => partenerPredator?.ContImplicitId ?? contExplicit,
        SursaCont.PartenerPrimitor => partenerPrimitor?.ContImplicitId ?? contExplicit,
        _ => contExplicit,
    };

    // Clonarea 00 §6: header (cu InverseazaLaturi), DOAR liniile care trec
    // filtrul de natură; liniile clonate poartă aceleași loturi și dimensiuni.
    // Fără linii eligibile nu se generează nimic (o factură doar de servicii
    // nu produce NIR).
    static Document GenereazaConex(IObjectSpace os, Document sursa, PoliticaConex politica,
        IReadOnlyDictionary<Guid, NaturaClasa> naturaPerLinie) {
        var linii = sursa.Detalii
            .Where(d => politica.NaturaFiltru == null || naturaPerLinie.GetValueOrDefault(d.ID) == politica.NaturaFiltru)
            .ToList();
        if (linii.Count == 0)
            return null;

        var tipTinta = os.GetObjectByKey<TipDocument>(politica.TipDocumentTintaId);
        var tipClr = typeof(Document).Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == tipTinta.ClrType && typeof(Document).IsAssignableFrom(t))
            ?? throw new OperareException($"Clasa documentului conex ({tipTinta?.ClrType}) nu există.");
        var conex = (Document)os.CreateObject(tipClr);
        conex.Data = sursa.Data;
        conex.PredatorId = politica.InverseazaLaturi ? sursa.PrimitorId : sursa.PredatorId;
        conex.PrimitorId = politica.InverseazaLaturi ? sursa.PredatorId : sursa.PrimitorId;
        conex.DocumentSursa = sursa;
        conex.Autogenerat = true;
        foreach (var s in linii) {
            var d = os.CreateObject<DocumentDetaliu>();
            d.Document = conex;
            d.TipMaterialId = s.TipMaterialId;
            d.LotId = s.LotId;
            d.Cantitate = s.Cantitate;
            d.Valoare = s.Valoare;
            d.AngajamentId = s.AngajamentId;
            d.Dimensiuni = DimensiuniResolver.Rezolva(s.Dimensiuni);
        }
        return conex;
    }

    // Corecția directă din decizia 14: întoarcerea în Draft, permisă DOAR fără
    // dependenți (simularea eliminării rândurilor proprii ține soldurile ≥ 0 și
    // niciun alt document nu a atins loturile create) și în perioadă deschisă.
    public static void AnuleazaOperarea(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi anulat.");
        GardianPerioada.VerificaDeschisa(os, doc.Data);
        VerificaFaraConexeOperate(os, doc);
        StergeConexeDraftAutogenerate(os, doc);

        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID).ToList();
        var randuriContabile = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();

        // Simularea eliminării: delta goală, rândurile proprii excluse, dar
        // cheile lor re-verificate de la prima dată afectată.
        if (randuriStoc.Count > 0) {
            var primaData = randuriStoc.Min(r => r.Data);
            var santinele = randuriStoc
                .Select(r => new CheieStoc(r.LotId, r.RepartitorId, r.TipStoc)).Distinct()
                .Select(c => new MiscareStoc(c, primaData, 0m)).ToList();
            StocService.VerificaSoldIntermediar(os, santinele, randuriStoc.Select(r => r.ID).ToList());
        }

        // Loturile create de liniile documentului nu au voie să fi fost atinse
        // de altcineva (nici măcar cu mișcări care lasă soldul ≥ 0).
        var idsDetalii = doc.Detalii.Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>().Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            if (os.GetObjectsQuery<RegistruStoc>().Any(r => r.LotId == lot.ID && r.DocumentId != doc.ID))
                throw new OperareException(
                    $"Lotul {lot.Produs?.Denumire} din {lot.Data:yyyy-MM-dd} e folosit de alte documente — folosiți storno.");
        }

        os.Delete(randuriStoc);
        os.Delete(randuriContabile);
        doc.Stare = StareDocument.Draft;
        doc.DataOperare = null;
        os.CommitChanges();
    }

    // Storno (decizia 14): rânduri inverse la data stornării, registrele rămân
    // append-only. Singura cale de corecție peste graniță (perioada documentului
    // închisă sau dependenți existenți), cât timp perioada stornării e deschisă
    // și soldurile rămân ≥ 0 din data stornării încolo.
    public static void Storneaza(IObjectSpace os, Document doc, DateOnly dataStorno) {
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi stornat.");
        if (dataStorno < doc.Data)
            throw new OperareException("Data stornării nu poate preceda data documentului.");
        GardianPerioada.VerificaDeschisa(os, dataStorno);
        VerificaFaraConexeOperate(os, doc);
        StergeConexeDraftAutogenerate(os, doc);

        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID).ToList();
        var randuriContabile = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();

        var delta = randuriStoc
            .Select(r => new MiscareStoc(new CheieStoc(r.LotId, r.RepartitorId, r.TipStoc), dataStorno, -r.Cantitate))
            .ToList();
        StocService.VerificaSoldIntermediar(os, delta);

        foreach (var r in randuriStoc) {
            var invers = os.CreateObject<RegistruStoc>();
            invers.Data = dataStorno;
            invers.TipStoc = r.TipStoc;
            invers.LotId = r.LotId;
            invers.RepartitorId = r.RepartitorId;
            invers.Cantitate = -r.Cantitate;
            invers.Valoare = -r.Valoare;
            invers.Storno = true;
            invers.Document = doc;
            invers.DetaliuId = r.DetaliuId;
        }
        foreach (var r in randuriContabile) {
            var invers = os.CreateObject<RegistruContabil>();
            invers.Data = dataStorno;
            invers.NumarNota = r.NumarNota;
            invers.ContDebitId = r.ContDebitId;
            invers.ContCreditId = r.ContCreditId;
            invers.Valoare = -r.Valoare;
            invers.DimensiuniDebit = DimensiuniResolver.Rezolva(r.DimensiuniDebit);
            invers.DimensiuniCredit = DimensiuniResolver.Rezolva(r.DimensiuniCredit);
            invers.Storno = true;
            invers.Document = doc;
            invers.DetaliuId = r.DetaliuId;
        }

        doc.Stare = StareDocument.Stornat;
        os.CommitChanges();
    }

    // Anularea/stornarea operează pe grupul conex (00 §8): copiii cu registre
    // (Operat) se anulează/stornează întâi — refuz conservator; copiii DRAFT
    // autogenerați sunt un artefact al operării anulate și se șterg odată cu ea
    // (re-operarea sursei generează un draft proaspăt).
    static void VerificaFaraConexeOperate(IObjectSpace os, Document doc) {
        if (os.GetObjectsQuery<Document>().Any(x => x.DocumentSursaId == doc.ID && x.Stare == StareDocument.Operat))
            throw new OperareException(
                "Documentul are documente generate (conexe) încă operate — anulați/stornați întâi acele documente.");
    }

    static void StergeConexeDraftAutogenerate(IObjectSpace os, Document doc) {
        foreach (var copil in os.GetObjectsQuery<Document>()
            .Where(x => x.DocumentSursaId == doc.ID && x.Autogenerat && x.Stare == StareDocument.Draft).ToList()) {
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
            os.Delete(copil);
        }
    }

    // EF Core dă proxy-uri de change-tracking — numele CLR real e pe tipul de bază.
    static TipDocument GasesteTipDocument(IObjectSpace os, Document doc) {
        var tip = doc.GetType();
        while (tip.Assembly.IsDynamic || tip.Name.EndsWith("Proxy"))
            tip = tip.BaseType;
        var nume = tip.Name;
        return os.FirstOrDefault<TipDocument>(t => t.ClrType == nume)
            ?? throw new OperareException($"Lipsește ancora TipDocument pentru clasa {nume} (seed).");
    }

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
}
