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

// ============ Felia 14 (D394): proiecția declarației informative — D4-V2…V7 ============
// CUSĂTURA pe care o probează: registrul fiscal (felia 11) → rândurile `op1`
// ale formularului 394, prin politica `(TipTva × Sens) → tip` (D4-D2) și prin
// clasificarea partenerului din nomenclator (D4-D1). Trei mecanisme, o cifră:
// mapările (DATE), identitatea fiscală (NOMENCLATOR) și regula nrFact (COD).
//
// DE CE o scenă proprie și nu D300-ul: acolo un furnizor și un client fără cod
// fiscal sunt suficienți; aici contează exact CINE e partenerul — cele patru
// tipuri plus persoana fizică, două grafii ale aceluiași CUI, un furnizor cu
// TVA la încasare — și regula per document a numărului de facturi, care cere
// facturi multi-cotă construite deliberat (egalitate de TVA, L + V pe aceeași
// factură, storno). Luna scenei e AUGUST 2026, pe care nicio altă scenă n-o
// atinge; stornoul cade în a doua jumătate a lunii, ca perioada să se poată
// tăia în două fără să iasă din lună.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaD300`.
// Premisa probelor de neaplicabilitate bugetară (D4-V2, D16-V2): o FCT operată prin motor, cu linie pe TipTva.
static class FctBugetaraOperata {
    public static FacturaIntrare Ruleaza(Suita s, IObjectSpace os, string marcaj, DateOnly data) {
        PurjaFctBugetara.Ruleaza(s, os, marcaj);
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = marcaj + "-FURN";
        furnizor.Denumire = "Furnizor premisă " + marcaj;
        var codEc = os.CreateObject<CodEconomic>();
        codEc.Cod = marcaj + "-CE";
        codEc.Denumire = "Cod economic premisă " + marcaj;
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = marcaj + "-FCT";
        fct.Data = data;
        fct.Predator = furnizor;
        fct.Primitor = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var linie = os.CreateObject<FacturaIntrareDetaliu>();
        linie.Document = fct;
        linie.TipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
        linie.Cantitate = 1m;
        linie.PretUnitar = 100m;
        linie.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");
        linie.CodEconomicId = codEc.ID;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fct);
        os.CommitChanges();
        return fct;
    }
}
