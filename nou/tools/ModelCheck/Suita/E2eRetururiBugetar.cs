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

// ========== Scenariul e2e 1C-a: retururile la BUGETAR (tipuri inerte) ==========
// Ancorele RLF/RDC trăiesc în nucleu (ambele profiluri), politicile sunt DATE de
// profil: la bugetar nu există reguli/numerotare/TVA implicit, deci tipurile sunt
// inerte — ca DSC/ITV/BPR (decizia 29).
static class E2eRetururiBugetar {
    public static void Ruleaza(Suita s) {
        using (var os = s.Provider.CreateObjectSpace()) {
            var tipRlf = os.FirstOrDefault<TipDocument>(t => t.Cod == "RLF");
            var tipRdc = os.FirstOrDefault<TipDocument>(t => t.Cod == "RDC");
            s.Check("Seed bugetar: ancorele TipDocument RLF/RDC există (nucleu), cu ClrType-urile claselor",
                tipRlf != null && tipRlf.ClrType == nameof(ReturFurnizor)
                && tipRdc != null && tipRdc.ClrType == nameof(ReturClient));
            bool Inert(TipDocument tip) =>
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tip.ID)
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tip.ID)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tip.ID) == null
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tip.ID) == null
                && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tip.ID) == null
                && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tip.ID) == null
                && tip.TipTvaImplicitId == null;
            s.Check("Bugetar: RLF și RDC sunt tipuri INERTE — fără reguli de stoc/contare, numerotare, politici sau TVA implicit",
                Inert(tipRlf) && Inert(tipRdc));
        }
    }
}
