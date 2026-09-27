using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Culegere;

/// <summary>Normalizările de culegere proprii unui tip: câmpurile celeilalte direcții se golesc persistat (F6-D3).</summary>
public static class NormalizariTip {
    public static void Aplica(IObjectSpace os, Document doc) {
        switch (doc) {
            case ListaDiferenteInventar ldi:
                foreach (var linie in Vii(os, ldi).OfType<ListaDiferenteInventarDetaliu>())
                    if (linie.Directie == DirectieDiferenta.Minus)
                        GolesteIntrarea(linie);
                break;
            case Asamblare asm:
                foreach (var linie in Vii(os, asm).OfType<AsamblareDetaliu>()) {
                    if (linie.Directie == DirectieAsamblare.Consum)
                        GolesteIntrarea(linie);
                    else if (linie.Directie == DirectieAsamblare.Produs)
                        RupePinulStrain(os, linie);
                }
                break;
            case NIR nir:
                foreach (var linie in Vii(os, nir).OfType<NirDetaliu>()) {
                    if (linie.LotId is Guid lotId && os.GetObjectByKey<Lot>(lotId)?.LinieIntrareId != linie.ID) {
                        linie.Produs = null;
                        linie.ProdusId = null;
                    }
                    if (linie.CauzaDiferentei != CauzaDiferentei.Imputabila) {
                        linie.PartenerDiferenta = null;
                        linie.PartenerDiferentaId = null;
                    }
                }
                break;
            case IesireImobilizare cas:
                RegenereazaCas(os, cas, Vii(os, cas).OfType<IesireImobilizareDetaliu>()
                    .Select(l => l.ImobilizareId).Where(id => id != Guid.Empty).Distinct().ToList());
                break;
        }
    }

    /// <summary>
    /// Liniile ieșirii sunt situația fișelor la data înregistrării, nu culegere; se refac
    /// numai când diferă. Fișa fără politică de amortizare rămâne cum e (refuzul e al operării).
    /// </summary>
    public static void RegenereazaCas(IObjectSpace os, IesireImobilizare doc, IReadOnlyList<Guid> fise) {
        var existente = Vii(os, doc).OfType<IesireImobilizareDetaliu>().ToList();
        var pastrate = new List<IesireImobilizareDetaliu>();
        foreach (var fisaId in fise) {
            var fisa = os.GetObjectByKey<Imobilizare>(fisaId);
            var politica = fisa == null ? null
                : os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == fisa.TipMaterialId);
            if (politica == null) {
                pastrate.AddRange(existente.Where(l => l.ImobilizareId == fisaId));
                continue;
            }
            var linii = AmortizareService.LiniiIesire(os, fisaId, doc.DataInregistrare, politica);
            var actuale = existente.Where(l => l.ImobilizareId == fisaId).ToList();
            if (Corespund(actuale, linii, fisa)) {
                pastrate.AddRange(actuale);
                continue;
            }
            foreach (var linie in linii) {
                var detaliu = os.CreateObject<IesireImobilizareDetaliu>();
                detaliu.Document = doc;
                detaliu.Imobilizare = fisa;
                detaliu.ImobilizareId = fisa.ID;
                detaliu.TipMaterialId = fisa.TipMaterialId;
                detaliu.Fel = linie.Fel;
                detaliu.Valoare = linie.Valoare;
                detaliu.Cantitate = 1m;
                detaliu.ContDebitId = linie.ContDebitId;
                detaliu.ContCreditId = linie.ContCreditId;
                // F26-D8: ambii repartitori = locul fișei.
                detaliu.RepartitorDebitId = fisa.LocId;
                detaliu.RepartitorCreditId = fisa.LocId;
                detaliu.CodEconomicId = fisa.CodEconomicId;
            }
        }
        var deSters = existente.Except(pastrate).ToList();
        if (deSters.Count > 0)
            os.Delete(deSters);
    }

    static bool Corespund(List<IesireImobilizareDetaliu> actuale, IReadOnlyList<LinieIesire> linii, Imobilizare fisa) =>
        actuale.Count == linii.Count
        && actuale.All(a => a.TipMaterialId == fisa.TipMaterialId && a.Cantitate == 1m
            && a.RepartitorDebitId == fisa.LocId && a.RepartitorCreditId == fisa.LocId
            && a.CodEconomicId == fisa.CodEconomicId)
        && actuale.Select(a => new LinieIesire(a.Fel, a.Valoare, a.ContDebitId, a.ContCreditId))
            .OrderBy(l => l.Fel).SequenceEqual(linii.OrderBy(l => l.Fel));

    static IEnumerable<DocumentDetaliu> Vii(IObjectSpace os, Document doc) =>
        doc.Detalii.Where(l => !os.IsObjectToDelete(l)).ToList();

    static void GolesteIntrarea(ListaDiferenteInventarDetaliu linie) {
        linie.Produs = null;
        linie.ProdusId = null;
        linie.PretEvaluare = null;
        linie.DataExpirare = null;
        linie.LotFabricatie = null;
    }

    static void GolesteIntrarea(AsamblareDetaliu linie) {
        linie.Produs = null;
        linie.ProdusId = null;
        linie.PretEvaluare = null;
        linie.DataExpirare = null;
        linie.LotFabricatie = null;
    }

    // Linia de produs își deține lotul; pinul rămas dintr-un consum anterior o face ne-operabilă (E2E-API-ASM).
    static void RupePinulStrain(IObjectSpace os, AsamblareDetaliu linie) {
        if (linie.LotId is not Guid pin)
            return;
        var lot = os.GetObjectByKey<Lot>(pin);
        if (lot == null || lot.LinieIntrareId != linie.ID) {
            linie.Lot = null;
            linie.LotId = null;
        }
    }
}
