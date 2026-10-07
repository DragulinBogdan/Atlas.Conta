using System.Text.Json;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiExplicatii {
    string CodPoliticii => Marcaj + "-POLITICA";
    const string CodMartorPolitica = "E2E-CIT111-MARTOR";

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        CurataPolitica(os, purja, CodPoliticii);

    Guid CreeazaPolitica(string cod) => CuSpatiu(os => {
        var tipStoc = Tip(os, Stoc);
        var tip = TipPropriu(os, cod);
        var sursa = os.GetObjectsQuery<RegulaContare>().Single(r => r.TipDocument.Cod == "BCS"
            && r.TipMaterialId == tipStoc && r.NaturaFiltru == null && r.SemnFiltru == null);
        var regula = os.CreateObject<RegulaContare>();
        var db = ((EFCoreObjectSpace)os).DbContext;
        foreach (var p in db.Entry(sursa).Properties)
            if (p.Metadata.Name is not (nameof(RegulaContare.ID) or nameof(RegulaContare.OptimisticLockField)
                    or nameof(RegulaContare.TipMaterialId)))
                db.Entry(regula).Property(p.Metadata.Name).CurrentValue = p.CurrentValue;
        regula.TipMaterialId = tip.ID; regula.DinSeed = true;
        os.CommitChanges(); return regula.ID;
    });

    string AmprentaRegulii(Guid id) => CuSpatiu(os => {
        var regula = os.GetObjectByKey<RegulaContare>(id);
        return regula == null ? null : JsonSerializer.Serialize(((EFCoreObjectSpace)os).DbContext.Entry(regula)
            .Properties.OrderBy(p => p.Metadata.Name).ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
    });

    void IzolareaPoliticii() {
        void CurataMartor() => Comanda(os => {
            var purja = new Purja(os); CurataPolitica(os, purja, CodMartorPolitica);
            purja.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod == CodMartorPolitica)); purja.Executa();
        });
        CurataMartor();
        var seed = CuSpatiu(os => {
            var tip = Tip(os, Stoc);
            return os.GetObjectsQuery<RegulaContare>().Single(r => r.TipDocument.Cod == "BCS"
                && r.TipMaterialId == tip && r.NaturaFiltru == null && r.SemnFiltru == null).ID;
        });
        var seedInainte = AmprentaRegulii(seed);
        var refuzuriInainte = RefuzuriSeed();
        try {
            var martor = CreeazaPolitica(CodMartorPolitica);
            Comanda(os => { os.GetObjectByKey<RegulaContare>(martor).DinSeed = false; os.CommitChanges(); });
            var martorInainte = AmprentaRegulii(martor);
            foreach (var sterge in new[] { false, true }) {
                var regula = CreeazaPolitica(CodPoliticii);
                Comanda(os => {
                    var r = os.GetObjectByKey<RegulaContare>(regula);
                    if (sterge) os.Delete(r); else r.PastreazaSemn = !r.PastreazaSemn;
                    GardianEditare.Verifica(os); os.CommitChanges();
                });
                Verifica("SC-CIT-111-IZOLARE", sterge ? "ștergerea lasă refuzul de seed al fixture-ului"
                        : "editarea confirmată lasă regula fixture-ului ca a clientului",
                    sterge ? RefuzuriSeed().Length == refuzuriInainte.Length + 1
                        : CuSpatiu(os => !os.GetObjectByKey<RegulaContare>(regula).DinSeed));
                PurjeazaNomenclatoare();
                Verifica("SC-CIT-111-IZOLARE", $"reluarea după {(sterge ? "ștergere" : "editare")} curăță regula și refuzul proprii; "
                        + "regula BCS străină, regula din seed și refuzurile preexistente rămân identice",
                    AmprentaRegulii(regula) == null && RefuzuriSeed().SequenceEqual(refuzuriInainte)
                        && AmprentaRegulii(seed) == seedInainte && AmprentaRegulii(martor) == martorInainte);
            }
        }
        finally { PurjeazaNomenclatoare(); CurataMartor(); }
    }
}
