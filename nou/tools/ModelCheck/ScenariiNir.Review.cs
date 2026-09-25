using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiNir {
    void ProvenientaCorectiei() {
        var (f, nir) = Constatat(4); Opereaza(nir);
        var nou = CuSpatiu(os => OperareApi.Corecteaza(os, nir, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
        Verifica("SC-NIR-32", "corecția primește sursa recepției fără a fi autogenerată", CuSpatiu(os => {
            var n = os.GetObjectByKey<NIR>(nou);
            return n.SursaReceptieiId == f.Id && !n.Autogenerat && n.DocumentSursaId == null;
        }));
        Schimba(nou, 3); Opereaza(nou);
        Delta("SC-NIR-32", nou, Clarificare, 25); Sold("SC-NIR-32", f.Linii[0], 3, 75);
    }

    void CitireaSursei() {
        foreach (var numar in new[] { 1, 12 }) {
            var f = Factura(Ianuarie, [.. Enumerable.Repeat(new LinieFctScena(4, 25, Privat ? "SFD" : "CAP0"), numar)]);
            var nir = Opereaza(f.Id).ConexId!.Value;
            using var os = Deschide(); var doc = os.GetObjectByKey<NIR>(nir);
            var linii = doc.Detalii.OfType<NirDetaliu>().ToList();
            foreach (var l in linii) { l.Cantitate = 3; l.CodFunctionalId = functionalNir; l.SursaFinantareId = finantareNir; }
            NumaratorSql.Instanta.Reseteaza();
            var erori = new List<string>();
            foreach (var l in linii) l.Verifica(os, erori);
            Verifica("SC-NIR-33", $"{numar} editări cantitative nu citesc sursa în gardianul editării",
                erori.Count == 0 && NumaratorSql.Instanta.Numar == 0);
            os.CommitChanges();
            NumaratorSql.Instanta.Reseteaza();
            MotorOperare.Opereaza(os, doc);
            var citiri = NumaratorSql.Instanta.Comenzi.Count(sql => sql.Contains("FROM \"Postare\"")
                && sql.Contains("JOIN \"Tranzactie\"") && sql.Contains("p.\"Latura\" = 1")
                && sql.Contains("p.\"FelUnitate\" = 1"));
            Verifica("SC-NIR-33", $"{numar} linii: recepția sursei citită o singură dată ({citiri})", citiri == 1);
        }
    }

    void AnalizaIstorica() {
        if (Privat) return;
        var (f, nir) = Constatat(3);
        var id = Cont(Stoc);
        var flags = CuSpatiu(os => os.GetObjectByKey<Cont>(id).DimensiuniObligatorii);
        Comanda(os => { os.GetObjectByKey<Cont>(id).DimensiuniObligatorii |= DimensiuneFlags.CodFunctional; os.CommitChanges(); });
        try {
            Verifica("SC-NIR-34", "sursa nu are codul funcțional cerut ulterior", CuSpatiu(os =>
                os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == f.Id && p.Cont == id && p.CodFunctional == null)));
            Verifica("SC-NIR-34", "dry-run păstrează analiza istorică", CuSpatiu(os => OperareApi.Valideaza(os, nir)).Count == 0);
            Opereaza(nir); Delta("SC-NIR-34", nir, Clarificare, 25);
            Verifica("SC-NIR-34", "diferența are analiza nouă, stocul o păstrează pe cea istorică", CuSpatiu(os =>
                os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == nir && p.Cont == id && p.CodFunctional == null)
                && os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == nir && p.Gestiune == N.GestiuniVirtuale.Inventar
                    && p.CodFunctional == functionalNir)));
        }
        finally { Comanda(os => { os.GetObjectByKey<Cont>(id).DimensiuniObligatorii = flags; os.CommitChanges(); }); }
    }

    void ImputatInert() {
        foreach (var q in new[] { 3m, 4m }) {
            var (_, nir) = Constatat(q, q == 3 ? CauzaDiferentei.PeDrum : CauzaDiferentei.Imputabila, Client);
            Opereaza(nir);
            Verifica("SC-NIR-35", $"imputatul fără efect economic este golit ({q})", CuSpatiu(os =>
                os.GetObjectByKey<NIR>(nir).Detalii.OfType<NirDetaliu>().All(l => l.PartenerDiferentaId == null)
                && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == nir && p.FelUnitate == N.FelUnitate.Partida)));
        }
    }

    void FacturaPe408() {
        if (Privat) return;
        var anterior = CuSpatiu(os => os.GetObjectByKey<Partener>(Furnizor).ContImplicitId);
        Comanda(os => { os.GetObjectByKey<Partener>(Furnizor).ContImplicitId = Cont(Nesosite); os.CommitChanges(); });
        try {
            var f = Factura(Ianuarie, new LinieFctScena(1, 100, "CAP0", false, Tip: Serviciu));
            Comanda(os => {
                var l = os.GetObjectByKey<FacturaIntrare>(f.Id).Detalii.OfType<FacturaIntrareDetaliu>().Single();
                l.CodFunctionalId = functionalNir; l.SursaFinantareId = finantareNir; os.CommitChanges();
            });
            Opereaza(f.Id);
            Verifica("SC-NIR-36/FCT", "FCT bugetar pe 408: debit 100, credit furnizor 100", Net(f.Id, Serviciu) == 100 && Net(f.Id, Nesosite) == -100);
            SoldPartida("SC-NIR-36/FCT", N.Unitate.DeschidePartida(Cont(Nesosite), Furnizor, f.Id, Ianuarie).Id, Ianuarie, -100);
        }
        finally { Comanda(os => { os.GetObjectByKey<Partener>(Furnizor).ContImplicitId = anterior; os.CommitChanges(); }); }
    }
}
