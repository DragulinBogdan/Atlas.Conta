using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// PIF: intrarea, modernizarea și revizuirea parametrilor; nu postează (F26-D5).
[TipDetaliu(typeof(PunereInFunctiuneDetaliu))]
[XafDisplayName("Punere în funcțiune")]
public class PunereInFunctiune : Document, IDocumentCuRegistruPropriu {
    static readonly FelMiscareImobilizare[] FeluriRegistru = [
        FelMiscareImobilizare.Intrare, FelMiscareImobilizare.Modernizare, FelMiscareImobilizare.Revizuire,
    ];

    static FelMiscareImobilizare Fel(FelLiniePif fel) => FeluriRegistru[(int)fel - 1];

    // Nu închide nicio datorie: nu postează nimic (86g).
    public override bool PoateFiStins(IObjectSpace os) => false;

    public override void PregatesteOperare(IObjectSpace os) {
        foreach (var linie in Detalii.OfType<PunereInFunctiuneDetaliu>())
            if (linie.ValoareFiscala == 0m)
                linie.ValoareFiscala = linie.Valoare;
    }

    public override void ValideazaOperare(IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is not UnitateInterna)
            erori.Add("Predatorul punerii în funcțiune trebuie să fie o unitate internă.");

        var fise = Fise(os, Detalii);
        foreach (var linie in Detalii) {
            if (linie is not PunereInFunctiuneDetaliu l) {
                erori.Add("Linia punerii în funcțiune trebuie culeasă ca linie de PIF, nu ca detaliu generic.");
                continue;
            }
            if (!fise.TryGetValue(l.ImobilizareId, out var fisa)) {
                erori.Add("Fiecare linie a punerii în funcțiune poartă fișa de imobilizare.");
                continue;
            }
            ValideazaLinie(os, l, fisa, erori);
        }
        foreach (var grup in Detalii.OfType<PunereInFunctiuneDetaliu>().GroupBy(l => l.ImobilizareId))
            if (grup.Count() > 1)
                erori.Add($"Fișa {(fise.TryGetValue(grup.Key, out var f) ? f.NumarInventar : grup.Key.ToString())} "
                    + $"apare pe {grup.Count()} linii ale aceleiași puneri în funcțiune — "
                    + "un document poartă un singur eveniment per fișă.");

        var lunaPif = Luna(Data);
        var amoUlterioara = os.GetObjectsQuery<AmortizareLunara>()
            .Where(a => a.Stare == StareDocument.Operat && a.Data > Data)
            .OrderBy(a => a.Data).Select(a => new { a.Numar, a.Data }).FirstOrDefault();
        if (amoUlterioara != null && Luna(amoUlterioara.Data) > lunaPif)
            erori.Add($"Există o amortizare operată pentru o lună ulterioară ({amoUlterioara.Numar}, "
                + $"{amoUlterioara.Data:dd.MM.yyyy}) — un eveniment retroactiv i-ar schimba baza de calcul. "
                + "Stornați-o înainte.");
    }

    void ValideazaLinie(IObjectSpace os, PunereInFunctiuneDetaliu l, Imobilizare fisa,
            ICollection<string> erori) {
        var eticheta = fisa.NumarInventar;
        if (fisa.LocId != PrimitorId)
            erori.Add($"Fișa {eticheta} are alt loc decât primitorul documentului — "
                + "un document de punere în funcțiune acoperă un singur loc.");
        if (fisa.TipMaterialId != l.TipMaterialId)
            erori.Add($"Tipul (contul) liniei nu e cel al fișei {eticheta}.");

        var stareCeruta = l.Fel == FelLiniePif.Intrare ? StareImobilizare.Noua : StareImobilizare.InFunctiune;
        if (fisa.Stare != stareCeruta)
            erori.Add($"Fișa {eticheta} e în starea „{fisa.Stare}”, iar linia „{l.Fel}” o cere „{stareCeruta}”.");

        if (l.Fel == FelLiniePif.Revizuire) {
            if (l.Valoare != 0m)
                erori.Add($"Revizuirea parametrilor fișei {eticheta} nu mișcă valoare — `Valoare` trebuie să fie 0.");
        }
        else if (l.Valoare <= 0m) {
            erori.Add($"Linia „{l.Fel}” a fișei {eticheta} trebuie să aibă valoare pozitivă.");
        }

        if (l.Fel != FelLiniePif.Modernizare) {
            if (l.Metoda == null || l.DurataLuni == null || l.MetodaFiscala == null
                    || l.DurataFiscalaLuni == null || l.CategorieFiscala == null || l.UtilizareExclusiva == null)
                erori.Add($"Linia „{l.Fel}” a fișei {eticheta} cere parametrii completi "
                    + "(metodă, durată, metodă fiscală, durată fiscală, categorie fiscală, utilizare exclusivă).");
        }
        if (l.DurataLuni < 0 || l.DurataFiscalaLuni < 0)
            erori.Add($"Duratele fișei {eticheta} nu pot fi negative.");
        // Graficul degresiv e pe ANI întregi (F26-D7).
        if ((l.Metoda == MetodaAmortizare.Degresiva && l.DurataLuni % 12 != 0)
                || (l.MetodaFiscala == MetodaAmortizare.Degresiva && l.DurataFiscalaLuni % 12 != 0))
            erori.Add($"Metoda degresivă a fișei {eticheta} cere o durată multiplu de 12 luni — "
                + "graficul ei se construiește pe ani întregi.");
        if (l.LuniAmortizateInitial < 0)
            erori.Add($"Lunile amortizate inițial ale fișei {eticheta} nu pot fi negative.");
        if ((l.Fel != FelLiniePif.Intrare || l.LinieSursaId != null)
                && (l.AmortizareInitiala != 0m || l.AmortizareFiscalaInitiala != 0m || l.LuniAmortizateInitial != 0))
            erori.Add($"Cifrele inițiale (amortizare cumulată, luni) se culeg doar pe intrarea FĂRĂ linie sursă "
                + $"a fișei {eticheta} — deschidere, producție proprie sau migrare.");

        var situatie = AmortizareService.Situatie(os, fisa.ID, Data);
        var brut = situatie.Valoare + l.Valoare;
        if (l.ValoareReziduala != null && l.ValoareReziduala >= brut)
            erori.Add($"Valoarea reziduală a fișei {eticheta} ({l.ValoareReziduala}) trebuie să fie sub "
                + $"valoarea brută ({brut}).");
        if (l.Fel != FelLiniePif.Modernizare) {
            var luniAmortizate = l.Fel == FelLiniePif.Intrare ? l.LuniAmortizateInitial : situatie.Luni;
            var minim = l.Fel == FelLiniePif.Revizuire ? luniAmortizate + 1 : luniAmortizate;
            if (l.DurataLuni < minim || (l.DurataFiscalaLuni > 0 && l.DurataFiscalaLuni < minim))
                erori.Add($"Duratele fișei {eticheta} ({l.DurataLuni} luni, fiscal {l.DurataFiscalaLuni}) trebuie "
                    + $"să depășească lunile deja amortizate ({luniAmortizate}) — altfel tot restul ar cădea "
                    + "într-o singură lună.");
        }

        VerificaBandaCatalogului(os, l, fisa, eticheta, erori);
        VerificaLinieSursa(os, l, eticheta, erori);
    }

    // Fără clasificare pe fișă nu există bandă, deci nici verificare (F26-D4).
    static void VerificaBandaCatalogului(IObjectSpace os, PunereInFunctiuneDetaliu l, Imobilizare fisa,
            string eticheta, ICollection<string> erori) {
        if (fisa.ClasificareId == null || l.DurataFiscalaLuni == null)
            return;
        var clasificare = os.GetObjectByKey<ClasificareImobilizari>(fisa.ClasificareId.Value);
        if (clasificare?.DurataMinAni == null || clasificare.DurataMaxAni == null)
            return;
        var min = clasificare.DurataMinAni.Value * 12;
        var max = clasificare.DurataMaxAni.Value * 12;
        if (l.DurataFiscalaLuni < min || l.DurataFiscalaLuni > max)
            erori.Add($"Durata fiscală a fișei {eticheta} ({l.DurataFiscalaLuni} luni) e în afara benzii "
                + $"clasificării {clasificare.Cod} ({min}–{max} luni).");
    }

    // Linia sursă e evidență + PLAFON: o linie cu cantitatea 3 hrănește 3 fișe (F26-D5).
    void VerificaLinieSursa(IObjectSpace os, PunereInFunctiuneDetaliu l, string eticheta,
            ICollection<string> erori) {
        if (l.LinieSursaId == null)
            return;
        var sursa = os.GetObjectByKey<DocumentDetaliu>(l.LinieSursaId.Value);
        var factura = sursa == null ? null
            : os.GetObjectByKey<Document>(sursa.DocumentId) as FacturaIntrare;
        if (factura == null) {
            erori.Add($"Linia sursă a fișei {eticheta} trebuie să fie o linie de factură de intrare.");
            return;
        }
        if (factura.Stare != StareDocument.Operat)
            erori.Add($"Factura sursă {factura.Numar} e în starea „{factura.Stare}” — la punerea în funcțiune "
                + "se leagă doar linii de facturi operate.");
        var clasa = Fapte.ClaseTip(os, [sursa.TipMaterialId]).GetValueOrDefault(sursa.TipMaterialId);
        if (clasa.Natura != NaturaClasa.Imobilizare)
            erori.Add($"Linia sursă a fișei {eticheta} nu e de clasă de imobilizări (natura „{clasa.Natura}”).");

        var sursaId = l.LinieSursaId.Value;
        var consumat = ConsumatPeLinieSursa(os, sursaId, ID);
        var peDocument = Detalii.OfType<PunereInFunctiuneDetaliu>()
            .Where(d => d.LinieSursaId == sursaId).Sum(d => d.Valoare);
        if (consumat + peDocument > sursa.Valoare)
            erori.Add($"Linia sursă a fișei {eticheta} are valoarea {sursa.Valoare}, iar punerile în funcțiune "
                + $"nestornate cumulează {consumat + peDocument} — plafonul liniei de factură e depășit.");
    }

    /// <summary>Σ `Valoare` a liniilor PIF nestornate legate de o linie sursă, fără documentul dat.</summary>
    public static decimal ConsumatPeLinieSursa(IObjectSpace os, Guid linieSursaId, Guid? faraDocument) =>
        ConsumatPeLiniiSursa(os, [linieSursaId], faraDocument).GetValueOrDefault(linieSursaId);

    /// <summary>Aceeași aritmetică, pe lot: o singură interogare grupată pe linia sursă.</summary>
    public static Dictionary<Guid, decimal> ConsumatPeLiniiSursa(IObjectSpace os,
            IReadOnlyCollection<Guid> liniiSursa, Guid? faraDocument) {
        if (liniiSursa.Count == 0)
            return new Dictionary<Guid, decimal>();
        var cerute = liniiSursa.Distinct().ToList();
        var linii = os.GetObjectsQuery<PunereInFunctiuneDetaliu>()
            .Where(d => d.LinieSursaId != null && cerute.Contains(d.LinieSursaId.Value));
        if (faraDocument is Guid exclus)
            linii = linii.Where(d => d.DocumentId != exclus);
        return linii
            .Join(os.GetObjectsQuery<PunereInFunctiune>().Where(p => p.Stare != StareDocument.Stornat),
                d => d.DocumentId, p => p.ID, (d, p) => new { Sursa = d.LinieSursaId.Value, d.Valoare })
            .ToList()
            .GroupBy(x => x.Sursa)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Valoare));
    }

    public void MaterializeazaRegistrul(IObjectSpace os) {
        var fise = Fise(os, Detalii);
        foreach (var l in Detalii.OfType<PunereInFunctiuneDetaliu>()) {
            var rand = os.CreateObject<RegistruImobilizari>();
            rand.Data = Data;
            rand.ImobilizareId = l.ImobilizareId;
            rand.Fel = Fel(l.Fel);
            rand.Valoare = l.Valoare;
            rand.ValoareFiscala = l.Fel == FelLiniePif.Revizuire ? 0m : l.ValoareFiscala;
            rand.Amortizare = l.AmortizareInitiala;
            rand.AmortizareFiscala = l.AmortizareFiscalaInitiala;
            rand.AmortizareDeductibila = l.AmortizareFiscalaInitiala;
            rand.Luni = l.LuniAmortizateInitial;
            rand.Metoda = l.Metoda;
            rand.DurataLuni = l.DurataLuni;
            rand.ValoareReziduala = l.ValoareReziduala;
            rand.MetodaFiscala = l.MetodaFiscala;
            rand.DurataFiscalaLuni = l.DurataFiscalaLuni;
            rand.CategorieFiscala = l.CategorieFiscala;
            rand.UtilizareExclusiva = l.UtilizareExclusiva;
            rand.RepartitorId = PrimitorId;
            rand.Document = this;
            rand.Detaliu = l;

            var fisa = fise[l.ImobilizareId];
            fisa.Stare = StareImobilizare.InFunctiune;
            if (l.Fel == FelLiniePif.Intrare)
                fisa.DataPunereInFunctiune = Data;
        }
    }

    public void EliminaRegistrul(IObjectSpace os) {
        var fise = Fise(os, Detalii);
        VerificaFaraFapteUlterioare(os, ID, Data, fise.Keys);
        os.Delete(RanduriProprii(os));
        foreach (var l in Detalii.OfType<PunereInFunctiuneDetaliu>())
            if (l.Fel == FelLiniePif.Intrare)
                ReaduLaNoua(fise[l.ImobilizareId]);
    }

    public void StorneazaRegistrul(IObjectSpace os, DateOnly data) {
        var fise = Fise(os, Detalii);
        VerificaFaraFapteUlterioare(os, ID, Data, fise.Keys);
        VerificaLunaStornarii(Data, data);
        foreach (var r in RanduriProprii(os))
            Inverseaza(os, r, data);
        foreach (var l in Detalii.OfType<PunereInFunctiuneDetaliu>())
            if (l.Fel == FelLiniePif.Intrare)
                ReaduLaNoua(fise[l.ImobilizareId]);
    }

    static void ReaduLaNoua(Imobilizare fisa) {
        fisa.Stare = StareImobilizare.Noua;
        fisa.DataPunereInFunctiune = null;
    }

    List<RegistruImobilizari> RanduriProprii(IObjectSpace os) {
        var id = ID;
        return os.GetObjectsQuery<RegistruImobilizari>().Where(r => r.DocumentId == id).ToList();
    }

    // Corecția directă doar fără dependenți (14): un fapt ulterior viu s-a calculat pe cifrele astea.
    internal static void VerificaFaraFapteUlterioare(IObjectSpace os, Guid id, DateOnly data,
            IEnumerable<Guid> fise) {
        var ids = fise.ToList();
        var ulterioare = os.GetObjectsQuery<RegistruImobilizari>()
            .Where(r => ids.Contains(r.ImobilizareId) && r.DocumentId != id && r.Data >= data)
            .Select(r => new { r.Storno, r.DetaliuId, r.Fel, r.Data }).ToList();
        var stornate = ulterioare.Where(r => r.Storno).Select(r => r.DetaliuId).ToHashSet();
        var viu = ulterioare.FirstOrDefault(r => !r.Storno && !stornate.Contains(r.DetaliuId));
        if (viu != null)
            throw new OperareException(
                $"Fișele documentului au fapte ulterioare nestornate („{viu.Fel}” din {viu.Data:dd.MM.yyyy}) — "
                + "anulați-le sau stornați-le pe acelea întâi.");
    }

    // Situația la o dată e o sumă de rânduri ≤ dată: un rând invers datat în altă lună ar lăsa
    // lunile dintre document și storno cu o situație falsă (87g).
    internal static void VerificaLunaStornarii(DateOnly dataDocument, DateOnly dataStorno) {
        if (Luna(dataStorno) != Luna(dataDocument))
            throw new OperareException(
                $"Un document de imobilizări se stornează cu o dată din luna lui ({dataDocument:MM.yyyy}) — "
                + $"stornarea pe {dataStorno:dd.MM.yyyy} ar lăsa situația fișelor falsă între cele două date.");
    }

    internal static void Inverseaza(IObjectSpace os, RegistruImobilizari r, DateOnly data) {
        var invers = os.CreateObject<RegistruImobilizari>();
        invers.Data = data;
        invers.ImobilizareId = r.ImobilizareId;
        invers.Fel = r.Fel;
        invers.Valoare = -r.Valoare;
        invers.ValoareFiscala = -r.ValoareFiscala;
        invers.Amortizare = -r.Amortizare;
        invers.AmortizareFiscala = -r.AmortizareFiscala;
        invers.AmortizareDeductibila = -r.AmortizareDeductibila;
        invers.Luni = -r.Luni;
        invers.Metoda = r.Metoda;
        invers.DurataLuni = r.DurataLuni;
        invers.ValoareReziduala = r.ValoareReziduala;
        invers.MetodaFiscala = r.MetodaFiscala;
        invers.DurataFiscalaLuni = r.DurataFiscalaLuni;
        invers.CategorieFiscala = r.CategorieFiscala;
        invers.UtilizareExclusiva = r.UtilizareExclusiva;
        invers.RepartitorId = r.RepartitorId;
        invers.DocumentId = r.DocumentId;
        invers.DetaliuId = r.DetaliuId;
        invers.Storno = true;
    }

    internal static int Luna(DateOnly data) => data.Year * 12 + data.Month;

    internal static Dictionary<Guid, Imobilizare> Fise(IObjectSpace os,
            IEnumerable<DocumentDetaliu> detalii) {
        var ids = detalii.Select(IdFisa).Where(id => id != Guid.Empty).Distinct().ToList();
        return os.GetObjectsQuery<Imobilizare>().Where(f => ids.Contains(f.ID)).ToList()
            .ToDictionary(f => f.ID);
    }

    static Guid IdFisa(DocumentDetaliu d) => d switch {
        PunereInFunctiuneDetaliu p => p.ImobilizareId,
        IesireImobilizareDetaliu i => i.ImobilizareId,
        AmortizareLunaraDetaliu a => a.ImobilizareId,
        _ => Guid.Empty,
    };
}

[XafDisplayName("Linie de punere în funcțiune")]
public class PunereInFunctiuneDetaliu : DocumentDetaliu {
    public virtual Guid ImobilizareId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Imobilizare")]
    public virtual Imobilizare Imobilizare { get; set; }

    [XafDisplayName("Fel")]
    public virtual FelLiniePif Fel { get; set; } = FelLiniePif.Intrare;

    public virtual Guid? LinieSursaId { get; set; }
    [XafDisplayName("Linie sursă")]
    public virtual DocumentDetaliu LinieSursa { get; set; }

    // Implicit = `Valoare` (`PregatesteOperare`); culeasă doar când brutul fiscal diferă.
    [XafDisplayName("Valoare fiscală")]
    public virtual decimal ValoareFiscala { get; set; }

    // Doar pe `Intrare` fără linie sursă: deschidere / migrare (F26-r13).
    [XafDisplayName("Amortizare inițială")]
    public virtual decimal AmortizareInitiala { get; set; }
    [XafDisplayName("Amortizare fiscală inițială")]
    public virtual decimal AmortizareFiscalaInitiala { get; set; }
    [XafDisplayName("Luni amortizate inițial")]
    public virtual int LuniAmortizateInitial { get; set; }

    // Obligatorii la `Intrare` și `Revizuire`, null la `Modernizare` (F26-D5).
    [XafDisplayName("Metodă")]
    public virtual MetodaAmortizare? Metoda { get; set; }
    [XafDisplayName("Durată (luni)")]
    public virtual int? DurataLuni { get; set; }
    [XafDisplayName("Valoare reziduală")]
    public virtual decimal? ValoareReziduala { get; set; }
    [XafDisplayName("Metodă fiscală")]
    public virtual MetodaAmortizare? MetodaFiscala { get; set; }
    [XafDisplayName("Durată fiscală (luni)")]
    public virtual int? DurataFiscalaLuni { get; set; }
    [XafDisplayName("Categorie fiscală")]
    public virtual CategorieFiscala? CategorieFiscala { get; set; }
    [XafDisplayName("Utilizare exclusivă")]
    public virtual bool? UtilizareExclusiva { get; set; }
}

// CAS: ieșirea din patrimoniu, două note per fișă din politică (F26-D6).
[TipDetaliu(typeof(IesireImobilizareDetaliu))]
[XafDisplayName("Ieșire de imobilizări")]
public class IesireImobilizare : Document, IDocumentCuPostareExplicita, IDocumentCuRegistruPropriu {
    [XafDisplayName("Cauză")]
    public virtual CauzaIesire Cauza { get; set; } = CauzaIesire.Casare;

    // Nu închide nicio datorie: notele ei sting valoarea activului, nu un terț (86g).
    public override bool PoateFiStins(IObjectSpace os) => false;

    public override void ValideazaOperare(IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PrimitorId) is not UnitateInterna)
            erori.Add("Primitorul ieșirii de imobilizări trebuie să fie o unitate internă.");

        var fise = PunereInFunctiune.Fise(os, Detalii);
        var linii = new List<IesireImobilizareDetaliu>();
        foreach (var linie in Detalii) {
            if (linie is not IesireImobilizareDetaliu l) {
                erori.Add("Linia ieșirii de imobilizări trebuie culeasă ca linie de CAS, nu ca detaliu generic.");
                continue;
            }
            linii.Add(l);
            if (!fise.TryGetValue(l.ImobilizareId, out var fisa)) {
                erori.Add("Fiecare linie a ieșirii poartă fișa de imobilizare.");
                continue;
            }
            if (fisa.Stare != StareImobilizare.InFunctiune)
                erori.Add($"Fișa {fisa.NumarInventar} e în starea „{fisa.Stare}” — iese din patrimoniu doar "
                    + "o fișă în funcțiune.");
            if (fisa.LocId != PredatorId)
                erori.Add($"Fișa {fisa.NumarInventar} are alt loc decât predatorul documentului.");
            if (l.ContDebitId == null || l.ContCreditId == null)
                erori.Add($"Fiecare linie a fișei {fisa.NumarInventar} poartă contul debitor ȘI contul creditor "
                    + "(ieșirea nu are reguli de contare).");
        }

        foreach (var grup in linii.GroupBy(l => l.ImobilizareId)) {
            var feluri = grup.Select(l => l.Fel).ToList();
            if (feluri.Count != feluri.Distinct().Count())
                erori.Add("O fișă nu poate avea două linii cu același fel pe aceeași ieșire.");
        }

        // Luna ieșirii nu se amortizează (F26-D6).
        if (AmortizareOperataDinLuna(os, Data) is { } amo)
            erori.Add($"Amortizarea {amo.Numar} ({amo.Data:dd.MM.yyyy}) e operată pentru luna ieșirii sau "
                + "pentru una ulterioară — luna ieșirii nu se amortizează. Stornați-o înainte.");

        foreach (var grup in linii.GroupBy(l => l.ImobilizareId)) {
            if (!fise.TryGetValue(grup.Key, out var fisa))
                continue;
            var politica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == fisa.TipMaterialId);
            if (politica == null) {
                erori.Add($"Tipul fișei {fisa.NumarInventar} n-are rând de politică de amortizare — "
                    + "conturile ieșirii vin exclusiv din ea.");
                continue;
            }
            var asteptate = AmortizareService.LiniiIesire(os, grup.Key, Data, politica)
                .OrderBy(l => l.Fel).ToList();
            var culese = grup
                .Select(l => new LinieIesire(l.Fel, l.Valoare, l.ContDebitId, l.ContCreditId))
                .OrderBy(l => l.Fel).ToList();
            if (!culese.SequenceEqual(asteptate))
                erori.Add($"Liniile fișei {fisa.NumarInventar} nu mai corespund situației din registru la "
                    + $"{Data:dd.MM.yyyy} — așteptat "
                    + string.Join(", ", asteptate.Select(l => $"{l.Fel} {l.Valoare}"))
                    + ". Regenerați liniile ieșirii.");
        }
    }

    public void MaterializeazaRegistrul(IObjectSpace os) {
        var fise = PunereInFunctiune.Fise(os, Detalii);
        foreach (var grup in Detalii.OfType<IesireImobilizareDetaliu>().GroupBy(l => l.ImobilizareId)) {
            var situatie = AmortizareService.Situatie(os, grup.Key, Data);
            var rand = os.CreateObject<RegistruImobilizari>();
            rand.Data = Data;
            rand.ImobilizareId = grup.Key;
            rand.Fel = FelMiscareImobilizare.Iesire;
            rand.Valoare = -situatie.Valoare;
            rand.ValoareFiscala = -situatie.ValoareFiscala;
            rand.Amortizare = -situatie.Amortizare;
            rand.AmortizareFiscala = -situatie.AmortizareFiscala;
            rand.AmortizareDeductibila = -situatie.AmortizareDeductibila;
            rand.Luni = 0;
            rand.RepartitorId = PredatorId;
            rand.Document = this;
            rand.Detaliu = grup.First();

            var fisa = fise[grup.Key];
            fisa.Stare = StareImobilizare.Iesita;
            fisa.DataIesire = Data;
        }
    }

    public void EliminaRegistrul(IObjectSpace os) {
        VerificaFaraAmortizareUlterioara(os);
        var id = ID;
        os.Delete(os.GetObjectsQuery<RegistruImobilizari>().Where(r => r.DocumentId == id).ToList());
        ReaduInFunctiune(os);
    }

    public void StorneazaRegistrul(IObjectSpace os, DateOnly data) {
        VerificaFaraAmortizareUlterioara(os);
        PunereInFunctiune.VerificaLunaStornarii(Data, data);
        var id = ID;
        foreach (var r in os.GetObjectsQuery<RegistruImobilizari>().Where(r => r.DocumentId == id).ToList())
            PunereInFunctiune.Inverseaza(os, r, data);
        ReaduInFunctiune(os);
    }

    // Lunile de după ieșire s-au generat FĂRĂ fișele ieșite (simetricul refuzului de la operare).
    void VerificaFaraAmortizareUlterioara(IObjectSpace os) {
        if (AmortizareOperataDinLuna(os, Data) is { } amo)
            throw new OperareException(
                $"Amortizarea {amo.Numar} ({amo.Data:dd.MM.yyyy}) e operată pentru luna ieșirii sau una "
                + "ulterioară, fără fișele ieșite — readuse în funcțiune, lunile acelea le-ar lipsi. "
                + "Stornați-o pe aceea întâi.");
    }

    static (string Numar, DateOnly Data)? AmortizareOperataDinLuna(IObjectSpace os, DateOnly data) {
        var primaZi = new DateOnly(data.Year, data.Month, 1);
        var amo = os.GetObjectsQuery<AmortizareLunara>()
            .Where(a => a.Stare == StareDocument.Operat && a.Data >= primaZi)
            .OrderBy(a => a.Data).Select(a => new { a.Numar, a.Data }).FirstOrDefault();
        return amo == null ? null : (amo.Numar, amo.Data);
    }

    public override IReadOnlyList<string> MesajeDupaOperare(IObjectSpace os) {
        var primaZi = new DateOnly(Data.Year, Data.Month, 1);
        var ultimaZiPrecedenta = primaZi.AddDays(-1);
        var primaZiPrecedenta = new DateOnly(ultimaZiPrecedenta.Year, ultimaZiPrecedenta.Month, 1);
        if (os.GetObjectsQuery<AmortizareLunara>().Any(a => a.Stare == StareDocument.Operat
                && a.Data >= primaZiPrecedenta && a.Data <= ultimaZiPrecedenta))
            return [];
        var puseInainte = PunereInFunctiune.Fise(os, Detalii).Values
            .Any(f => f.DataPunereInFunctiune < primaZiPrecedenta);
        return puseInainte
            ? [$"Luna precedentă ({ultimaZiPrecedenta:MM.yyyy}) n-are amortizare operată, deși fișele ieșite "
                + "erau în funcțiune — generați și operați amortizarea ei sau confirmați că nu mai aveau rest."]
            : [];
    }

    void ReaduInFunctiune(IObjectSpace os) {
        foreach (var fisa in PunereInFunctiune.Fise(os, Detalii).Values) {
            fisa.Stare = StareImobilizare.InFunctiune;
            fisa.DataIesire = null;
        }
    }
}

[XafDisplayName("Linie de ieșire de imobilizări")]
public class IesireImobilizareDetaliu : DocumentDetaliu, ILinieCuPostareExplicita {
    public virtual Guid ImobilizareId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Imobilizare")]
    public virtual Imobilizare Imobilizare { get; set; }

    [XafDisplayName("Fel")]
    public virtual FelLinieIesire Fel { get; set; } = FelLinieIesire.AmortizareCumulata;

    public virtual Guid? ContDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cont debitor")]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cont creditor")]
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Repartitor debit")]
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Repartitor credit")]
    public virtual Repartitor RepartitorCredit { get; set; }

    public virtual Guid? CodEconomicId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() =>
        new() { RepartitorId = RepartitorDebitId, CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}

// AMO: amortizarea lunii, GENERATĂ pe tiparul ITV; derivă din `Document`, nu din NTC (F26-D7).
[TipDetaliu(typeof(AmortizareLunaraDetaliu))]
[XafDisplayName("Amortizare lunară")]
public class AmortizareLunara : Document, IDocumentCuPostareExplicita, IDocumentCuRegistruPropriu {
    public override bool PoateFiStins(IObjectSpace os) => false;

    public override void ValideazaOperare(IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (PredatorId != PrimitorId || os.GetObjectByKey<Repartitor>(PredatorId) is not UnitateInterna)
            erori.Add("Amortizarea lunară are pe ambele laturi aceeași unitate internă.");
        var ultimaZi = new DateOnly(Data.Year, Data.Month, DateTime.DaysInMonth(Data.Year, Data.Month));
        if (Data != ultimaZi)
            erori.Add($"Amortizarea lunară se datează în ultima zi a lunii ({ultimaZi:dd.MM.yyyy}) — "
                + "data o pune generatorul.");

        var fise = PunereInFunctiune.Fise(os, Detalii);
        foreach (var linie in Detalii) {
            if (linie is not AmortizareLunaraDetaliu l) {
                erori.Add("Linia amortizării trebuie culeasă ca linie de AMO, nu ca detaliu generic.");
                continue;
            }
            if (!fise.TryGetValue(l.ImobilizareId, out var fisa)) {
                erori.Add("Fiecare linie a amortizării poartă fișa de imobilizare.");
                continue;
            }
            if (l.RepartitorDebitId == null)
                erori.Add($"Linia fișei {fisa.NumarInventar} poartă locul explicit — el e dimensiunea "
                    + "rândului de registru, indiferent dacă linia postează.");
            // Linia care nu postează (contabil 0, fiscal > 0) rămâne fără conturi (F26-D7).
            if (l.Valoare != 0m && (l.ContDebitId == null || l.ContCreditId == null))
                erori.Add($"Linia fișei {fisa.NumarInventar} poartă conturile explicite "
                    + "(amortizarea nu are reguli de contare).");
        }

        var analiza = AmortizareService.Previzualizeaza(os, Data.Year, Data.Month, ID);
        if (analiza.Motiv != null) {
            erori.Add($"Amortizarea lunii {Data.Month:00}/{Data.Year} nu se poate opera: "
                + AmortizareService.Eticheta(analiza.Motiv.Value)
                + (analiza.Detaliu == null ? "." : $" ({analiza.Detaliu})."));
            return;
        }
        var nepotrivite = Nepotriviri(analiza);
        if (nepotrivite.Count > 0) {
            var diferite = nepotrivite
                .Select(id => fise.TryGetValue(id, out var f) ? f.NumarInventar : id.ToString())
                .Take(5).ToList();
            erori.Add($"Liniile amortizării {Data.Month:00}/{Data.Year} nu mai corespund situației fișelor "
                + $"({string.Join(", ", diferite)}) — regenerați amortizarea lunii.");
        }
    }

    /// <summary>Criteriul anti-stale al gardianului, expus și ușii care raportează `Stale`.</summary>
    public bool LiniileCorespund(RezultatAmortizare analiza) => Nepotriviri(analiza).Count == 0;

    IReadOnlyList<Guid> Nepotriviri(RezultatAmortizare analiza) {
        var asteptate = analiza.Linii.Select(Cheie).OrderBy(c => c.Fisa).ToList();
        var culese = Detalii.OfType<AmortizareLunaraDetaliu>().Select(Cheie).OrderBy(c => c.Fisa).ToList();
        return asteptate.SequenceEqual(culese)
            ? []
            : asteptate.Except(culese).Concat(culese.Except(asteptate))
                .Select(c => c.Fisa).Distinct().ToList();
    }

    static (Guid Fisa, decimal Contabil, decimal Fiscal, decimal Deductibil,
        Guid? Debit, Guid? Credit, Guid? Loc, Guid? CentruCost, Guid? CodEconomic) Cheie(LinieAmortizare l) =>
        (l.ImobilizareId, l.Contabil, l.Fiscal, l.Deductibil, l.ContCheltuialaId, l.ContAmortizareId,
            l.LocId, l.CentruCostId, l.CodEconomicId);

    static (Guid Fisa, decimal Contabil, decimal Fiscal, decimal Deductibil,
        Guid? Debit, Guid? Credit, Guid? Loc, Guid? CentruCost, Guid? CodEconomic) Cheie(AmortizareLunaraDetaliu d) =>
        (d.ImobilizareId, d.Valoare, d.ValoareFiscala, d.ValoareDeductibila,
            d.ContDebitId, d.ContCreditId, d.RepartitorDebitId, d.CentruCostId, d.CodEconomicId);

    public void MaterializeazaRegistrul(IObjectSpace os) {
        foreach (var l in Detalii.OfType<AmortizareLunaraDetaliu>()) {
            var rand = os.CreateObject<RegistruImobilizari>();
            rand.Data = Data;
            rand.ImobilizareId = l.ImobilizareId;
            rand.Fel = FelMiscareImobilizare.Amortizare;
            rand.Amortizare = l.Valoare;
            rand.AmortizareFiscala = l.ValoareFiscala;
            rand.AmortizareDeductibila = l.ValoareDeductibila;
            rand.Luni = 1;
            rand.RepartitorId = l.RepartitorDebitId ?? PrimitorId;
            rand.Document = this;
            rand.Detaliu = l;
        }
    }

    public void EliminaRegistrul(IObjectSpace os) {
        VerificaFaraDependenti(os);
        var id = ID;
        os.Delete(os.GetObjectsQuery<RegistruImobilizari>().Where(r => r.DocumentId == id).ToList());
    }

    public void StorneazaRegistrul(IObjectSpace os, DateOnly data) {
        VerificaFaraDependenti(os);
        PunereInFunctiune.VerificaLunaStornarii(Data, data);
        var id = ID;
        foreach (var r in os.GetObjectsQuery<RegistruImobilizari>().Where(r => r.DocumentId == id).ToList())
            PunereInFunctiune.Inverseaza(os, r, data);
    }

    void VerificaFaraDependenti(IObjectSpace os) {
        VerificaFaraAmortizareUlterioara(os);
        PunereInFunctiune.VerificaFaraFapteUlterioare(os, ID, Data,
            Detalii.OfType<AmortizareLunaraDetaliu>().Select(l => l.ImobilizareId));
    }

    void VerificaFaraAmortizareUlterioara(IObjectSpace os) {
        var data = Data;
        var ulterioara = os.GetObjectsQuery<AmortizareLunara>()
            .Where(a => a.Data > data && a.Stare == StareDocument.Operat)
            .OrderBy(a => a.Data).Select(a => new { a.Numar, a.Data }).FirstOrDefault();
        if (ulterioara != null)
            throw new OperareException(
                $"Există o amortizare operată pentru o lună ulterioară ({ulterioara.Numar}, "
                + $"{ulterioara.Data:dd.MM.yyyy}) — cronologia amortizării e strictă. Stornați-o pe aceea întâi.");
    }
}

[XafDisplayName("Linie de amortizare lunară")]
public class AmortizareLunaraDetaliu : DocumentDetaliu, ILinieCuPostareExplicita {
    public virtual Guid ImobilizareId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Imobilizare")]
    public virtual Imobilizare Imobilizare { get; set; }

    // `Valoare` (bază) = amortizarea CONTABILĂ, singura care postează (F26-D7).
    [XafDisplayName("Amortizare fiscală")]
    public virtual decimal ValoareFiscala { get; set; }
    [XafDisplayName("Amortizare deductibilă")]
    public virtual decimal ValoareDeductibila { get; set; }

    public virtual Guid? ContDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cont de cheltuială")]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cont de amortizare")]
    public virtual Cont ContCredit { get; set; }
    // Ambii repartitori = LOCUL fișei la data lunii (F26-D8).
    public virtual Guid? RepartitorDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Loc")]
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Repartitor credit")]
    public virtual Repartitor RepartitorCredit { get; set; }

    public virtual Guid? CentruCostId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Centru de cost")]
    public virtual Repartitor CentruCost { get; set; }

    // Copiat de pe fișă la generare și verificat anti-stale, ca locul (F26-r16).
    public virtual Guid? CodEconomicId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() {
        RepartitorId = RepartitorDebitId, CentruCostId = CentruCostId, CodEconomicId = CodEconomicId
    };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
