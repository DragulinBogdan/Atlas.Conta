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

static class RuleazaScenele {
    public static int Ruleaza(Suita s, bool privat) {
        AcoperireInvarianti.Reseteaza();
        var selectate = Scenarii.Selecteaza(ScenelePeTip.Ruleaza(s, privat), s.FiltruScenarii);
        if (s.FiltruScenarii != null) {
            var eticheta = privat ? "privat" : "bugetar";
            Console.WriteLine($"Scenarii {string.Join(",", s.FiltruScenarii.Order())} pe {eticheta}: "
                + (selectate.Count == 0 ? "NICIUNA" : string.Join(", ", selectate.Select(s => s.Nume))));
            if (selectate.Count == 0) {
                Console.WriteLine($"Niciun scenariu pe profilul {eticheta} pentru tipurile cerute "
                    + "(catalogul: docs/nucleu/scenarii/).");
                return 0;
            }
        }
        foreach (var scena in selectate) {
            var ceas = Stopwatch.StartNew();
            try { scena.Ruleaza(); }
            catch (Exception ex) {
                s.Check($"Scena {scena.Nume}: excepție neașteptată", false);
                Console.WriteLine(ex);
                break;
            }
            if (s.FiltruScenarii != null)
                Console.WriteLine($"     {scena.Nume}: {ceas.Elapsed.TotalSeconds:0.0} s");
        }
        return selectate.Count;
    }
}
