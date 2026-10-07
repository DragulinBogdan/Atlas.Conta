using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ============ Felia Api FCT — semantica override-ului de TVA (privat) ============
// Complementul blocului bugetar (review advers F2-D1): pe regimul Normal
// override-ul e LEGITIM (36a — factura furnizorului bate rotunjirea), iar
// recalculul e CONDIȚIONAT de declanșatorii din UI — un PUT care nu atinge
// baza/TipTva nu pierde override-ul (clientul nu retrimite ValoareTva).
static class E2eApiFctPrivat {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajApiPrv = "E2E-APIPRV";
            using var os = s.Provider.CreateObjectSpace();
            void CurataApiPrv(IObjectSpace o) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(o);
                foreach (var d in o.GetObjectsQuery<FacturaIntrare>()
                        .Where(x => x.Numar.StartsWith(MarcajApiPrv)).ToList()) {
                    pj.Adauga(o.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == d.ID).ToList());
                    pj.Adauga(d);
                }
                foreach (var r in o.GetObjectsQuery<Repartitor>()
                        .Where(x => x.Cod.StartsWith(MarcajApiPrv)).ToList())
                    pj.Adauga(r);
                pj.Executa();
            }
            CurataApiPrv(os);
            var n21Api = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var sddApi = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            var tipServApi = os.GetObjectsQuery<TipMaterial>()
                .First(t => t.Clasa.Natura == NaturaClasa.Serviciu);
            var furnizorApi = os.CreateObject<Partener>();
            furnizorApi.Cod = MarcajApiPrv + "-F"; furnizorApi.Denumire = "Furnizor Api Privat";
            var gestiuneApi = os.CreateObject<Gestiune>();
            gestiuneApi.Cod = MarcajApiPrv + "-G"; gestiuneApi.Denumire = "Gestiune Api Privat";
            os.CommitChanges();

            var w = new FacturaIntrareWriteDto {
                Numar = MarcajApiPrv + "-1", Data = new DateOnly(2026, 3, 10),
                PredatorId = furnizorApi.ID, PrimitorId = gestiuneApi.ID,
                Linii = { new FacturaIntrareLinieWriteDto {
                    TipMaterialId = tipServApi.ID, Cantitate = 1m, PretUnitar = 100m,
                    TipTvaId = n21Api.ID } }
            };
            var idFctPrv = FacturaIntrareApply.Aplica(os, null, w);
            var liniePrv = FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0];
            s.Check("Api privat/N21: calculul la culegere — net 100 + TVA 21",
                liniePrv is { Valoare: 100m, ValoareTva: 21m });
            w.Linii[0].Id = liniePrv.Id;
            w.Linii[0].ValoareTva = 21.37m;
            FacturaIntrareApply.Aplica(os, idFctPrv, w);
            s.Check("Override pe regim Normal → acceptat, aplicat DUPĂ calcul (36a)",
                FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0].ValoareTva == 21.37m);
            w.Linii[0].ValoareTva = null;
            FacturaIntrareApply.Aplica(os, idFctPrv, w);
            s.Check("PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
                FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0].ValoareTva == 21.37m);
            w.Linii[0].PretUnitar = 200m;
            FacturaIntrareApply.Aplica(os, idFctPrv, w);
            s.Check("Schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
                FacturaIntrareApply.Citeste(os, idFctPrv).Linii[0] is { Valoare: 200m, ValoareTva: 42m });
            if (sddApi != null) {
                w.Linii[0].TipTvaId = sddApi.ID;
                w.Linii[0].ValoareTva = 5m;
                s.CheckRefuza("Override pe regim Scutit (SDD) → refuz (F2-D1: regimul nu poartă TVA separat)",
                    () => FacturaIntrareApply.Aplica(os, idFctPrv, w));
            }
            CurataApiPrv(os);
        }
    }
}
