using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// NTC (design FAZA 1C §5): nota contabilă — mecanismul de import de note
// anticipat de decizia 9 (salarii, imobilizări, închideri intră în contabilitate
// prin note) și, în același timp, tip de CULEGERE manuală (nota contabilă
// manuală e cerință reală, nu doar ușă de import).
//
// Nota NU are reguli de stoc și NU are reguli de contare: fiecare linie își
// poartă postarea explicit (ILinieCuPostareExplicita — mecanismul 32a, extins
// în motor: postarea explicită COMPLETĂ bate și ABSENȚA regulii). Gardienii
// generici rămân activi (perioadă, dimensiuni obligatorii per cont, storno).
//
// Laturile: nota nu are predator/primitor economic (contrapartidele trăiesc pe
// conturile liniilor) — convenția fixată e că ambele laturi sunt repartitori
// INTERNI (ex. SEDIU), purtători ai dimensiunii Repartitor implicite din 00 §5.
[TipDetaliu(typeof(NotaContabilaDetaliu))]
public class NotaContabila : Document, IDocumentCuPostareExplicita {
    // Fără PregatesteOperare: `Valoare` se culege direct pe linie (nu există
    // lanț de valori — nici cantitate, nici preț, nici TVA calculat).

    // Rolul de STINGĂTOR (decizia 48b — Compensarea din 1C, 869/an): nota
    // operată poate stinge documente, iar invariantul de contrapartidă al
    // trezoreriei (31d) se reformulează pe ea — contrapartidele stinse sunt
    // repartitorii EXPLICIȚI de pe linii (401 = 4111 pe partenerul X stinge
    // atât factura lui de furnizor, cât și pe cea de client). Plafonul per
    // contrapartidă = suma liniilor pe care ea apare, NUMĂRATĂ PER LATURĂ:
    // rândul 401 = 4111 de X lei pe același partener duce X pe debit (datoria)
    // și X pe credit (creanța), adică exact cele două stingeri legitime.
    // Fără repartitori pe linii nota nu stinge nimic (dicționar gol → refuz).
    // Proiecție pe FK-uri, fără navigația Detalii (25b).
    public override IReadOnlyDictionary<Guid, decimal> CapacitateStingere(DevExpress.ExpressApp.IObjectSpace os) {
        var id = ID;
        var linii = os.GetObjectsQuery<NotaContabilaDetaliu>()
            .Where(d => d.DocumentId == id)
            .Select(d => new { d.Valoare, d.RepartitorDebitId, d.RepartitorCreditId })
            .ToList();
        var capacitati = new Dictionary<Guid, decimal>();
        void Adauga(Guid? repartitorId, decimal valoare) {
            if (repartitorId != null)
                capacitati[repartitorId.Value] = capacitati.GetValueOrDefault(repartitorId.Value) + Math.Abs(valoare);
        }
        foreach (var l in linii) {
            Adauga(l.RepartitorDebitId, l.Valoare);
            Adauga(l.RepartitorCreditId, l.Valoare);
        }
        return capacitati;
    }

    public override void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        base.ValideazaOperare(os, erori);
        if (os.GetObjectByKey<Repartitor>(PredatorId) is Partener
                || os.GetObjectByKey<Repartitor>(PrimitorId) is Partener)
            erori.Add("Laturile notei contabile sunt repartitori interni (ex. SEDIU) — partenerul apare pe contul liniei, nu pe latură.");
        foreach (var d in Detalii) {
            // Linia de BAZĂ n-are cum să poarte postarea explicită, deci ar fi
            // sărită mut de motor (precedentul DSC 38c: refuz explicit).
            if (d is not NotaContabilaDetaliu linie) {
                erori.Add("Linia notei contabile trebuie culeasă ca linie de notă, nu ca detaliu generic.");
                continue;
            }
            if (linie.ContDebitId == null || linie.ContCreditId == null)
                erori.Add("Fiecare linie de notă contabilă poartă contul debitor ȘI contul creditor (nota nu are reguli de contare).");
            // Negativul e PERMIS (importul 1C aduce note storno cu minus); doar
            // zero e fără sens — n-ar posta nimic.
            if (linie.Valoare == 0)
                erori.Add("Fiecare linie de notă contabilă poartă o valoare nenulă (negativul e permis — note storno).");
        }
    }
}

// Postarea explicită pe linie ca DATE de primă clasă (contractul existent al
// Decontului — 32a): pe NTC conturile sunt OBLIGATORII (validare), repartitorii
// per latură rămân opționali (fără ei cade default-ul polimorf al header-ului).
// Restul semanticii bazei (lot, cantitate, TVA) nu se folosește pe notă.
public class NotaContabilaDetaliu : DocumentDetaliu, ILinieCuPostareExplicita {
    public virtual string Descriere { get; set; }

    // Alegere din planul mare, ca DecontDetaliu (nomenclator mare — lookup
    // standard; SmartLookup revertat, decizia 40d/gate).
    public virtual Guid? ContDebitId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContDebit { get; set; }
    public virtual Guid? ContCreditId { get; set; }
    [EditorAlias(EditorAliases.LookupPropertyEditor)]
    public virtual Cont ContCredit { get; set; }
    public virtual Guid? RepartitorDebitId { get; set; }
    public virtual Repartitor RepartitorDebit { get; set; }
    public virtual Guid? RepartitorCreditId { get; set; }
    public virtual Repartitor RepartitorCredit { get; set; }

    // DIM-2 (decizia 54c, inventar §2): defalcarea E pe conturile care o cer
    // (trezorerie/venituri) — nota de import/manuală o poartă pe linie.
    public virtual Guid? CodEconomicId { get; set; }
    [XafDisplayName("Cod economic")]
    public virtual CodEconomic CodEconomic { get; set; }

    public override Dimensiuni DimensiuniCulese() => new() { CodEconomicId = CodEconomicId };
    public override void PreiaDimensiuni(Dimensiuni s) => CodEconomicId = s.CodEconomicId;
}
