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
    public static void Opereaza(IObjectSpace os, Document doc) {
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

        // Clasa fiecărui Tip de pe linii, preîncărcată — motorul nu se bazează
        // pe navigații (contextul apelant nu garantează lazy loading).
        var idsTip = doc.Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var claseTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura, t.Denumire })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura, t.Denumire));

        // 1. Mișcările de stoc se CALCULEAZĂ întâi (delta), gardianul de sold
        //    le verifică, abia apoi se materializează rândurile.
        var reguliStoc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        var miscari = new List<(DocumentDetaliu Detaliu, RegulaStoc Regula, MiscareStoc Miscare)>();
        foreach (var regula in reguliStoc) {
            foreach (var d in doc.Detalii.Where(d => SeAplica(regula, claseTip.GetValueOrDefault(d.TipMaterialId)))) {
                if (d.LotId == null)
                    throw new OperareException(
                        $"Linia cu {claseTip.GetValueOrDefault(d.TipMaterialId).Denumire} intră în regulile de stoc dar nu are lot.");
                var repartitorId = regula.Latura == LaturaDocument.Predator ? doc.PredatorId : doc.PrimitorId;
                miscari.Add((d, regula, new MiscareStoc(
                    new CheieStoc(d.LotId.Value, repartitorId, regula.TipStoc), doc.Data, regula.Semn * d.Cantitate)));
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

        // 2. Finalizarea loturilor născute de liniile documentului (NIR, plus
        //    de inventar, producție): lotul e creat la culegere de linia de
        //    intrare (baza nu poartă ProdusId — testul bazei §2); motorul îi
        //    fixează prețul (= Valoare/Cantitate, decizia 13) și data.
        var idsDetalii = doc.Detalii.Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>().Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            var linie = doc.Detalii.First(d => d.ID == lot.LinieIntrareId);
            if (linie.Cantitate <= 0)
                throw new OperareException("O linie care creează lot trebuie să aibă cantitate pozitivă.");
            lot.PretUnitar = linie.Valoare / linie.Cantitate;
            lot.Data = doc.Data;
        }

        // 3. Rândurile contabile: regulă per (tip document, Tip material), cu
        //    fallback pe regula fără Tip; fără regulă = tipul nu contează
        //    (NotaTransfer — decizia 23c). Dimensiunile se rezolvă per latură.
        var reguliContare = os.GetObjectsQuery<RegulaContare>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        foreach (var d in doc.Detalii) {
            var regula = reguliContare.FirstOrDefault(r => r.TipMaterialId == d.TipMaterialId)
                ?? reguliContare.FirstOrDefault(r => r.TipMaterialId == null);
            if (regula == null)
                continue;
            if (regula.ContDebitId == null || regula.ContCreditId == null)
                throw new OperareException(
                    $"Regula de contare pentru {tipDoc.Cod}/{regula.TipMaterial?.Denumire ?? "*"} nu are ambele conturi.");
            var rand = os.CreateObject<RegistruContabil>();
            rand.Data = doc.Data;
            rand.ContDebitId = regula.ContDebitId.Value;
            rand.ContCreditId = regula.ContCreditId.Value;
            rand.Valoare = d.Valoare;
            rand.DimensiuniDebit = DimensiuniResolver.Rezolva(d.Dimensiuni, regula.DimensiuniOverrideDebit,
                regula.DimensiuniComun, new Dimensiuni { RepartitorId = doc.PredatorId });
            rand.DimensiuniCredit = DimensiuniResolver.Rezolva(d.Dimensiuni, regula.DimensiuniOverrideCredit,
                regula.DimensiuniComun, new Dimensiuni { RepartitorId = doc.PrimitorId });
            rand.Document = doc;
            rand.Detaliu = d;
        }

        doc.Stare = StareDocument.Operat;
        doc.DataOperare = DateTime.UtcNow;
        os.CommitChanges();
    }

    // Corecția directă din decizia 14: întoarcerea în Draft, permisă DOAR fără
    // dependenți (simularea eliminării rândurilor proprii ține soldurile ≥ 0 și
    // niciun alt document nu a atins loturile create) și în perioadă deschisă.
    public static void AnuleazaOperarea(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi anulat.");
        GardianPerioada.VerificaDeschisa(os, doc.Data);
        VerificaFaraConexeOperate(os, doc);

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

    // Anularea/stornarea operează pe grupul conex (00 §8): cât timp mecanismul
    // complet vine la 3c, gardianul conservator refuză părintele cu copii
    // încă operați — copiii se anulează/stornează întâi.
    static void VerificaFaraConexeOperate(IObjectSpace os, Document doc) {
        if (os.GetObjectsQuery<Document>().Any(x => x.DocumentSursaId == doc.ID && x.Stare == StareDocument.Operat))
            throw new OperareException(
                "Documentul are documente generate (conexe) încă operate — anulați/stornați întâi acele documente.");
    }

    // Filtrul regulii de stoc (decizia 14: tip × semn × filtru = date):
    // Clasa explicită sau, implicit, orice clasă purtătoare de stoc (23b).
    static bool SeAplica(RegulaStoc regula, (Guid ClasaId, NaturaClasa Natura, string Denumire) clasaTip) {
        if (clasaTip.ClasaId == Guid.Empty)
            return false;
        return regula.ClasaId == null
            ? clasaTip.Natura == NaturaClasa.Stoc
            : clasaTip.ClasaId == regula.ClasaId;
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
