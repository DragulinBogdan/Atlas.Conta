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

// ============ 091: scenele catalogului, pe tipul pe care îl probează ============
// Ordinea listei e ordinea suitei integrale. PLT/INC și FCT au scenă doar pe
// privat (partidele și regimurile de TVA sunt ale profilului); DSC e inert la
// bugetar, scena FCL ∪ DSC îl sare acolo.
static class ScenelePeTip {
    public static List<Scena> Ruleaza(Suita s, bool privat) {
        var scene = new List<Scena> {
            new(nameof(ScenariiImo), ["IMO"], () => ScenariiImo.Ruleaza(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna))),
            new(nameof(ScenariiDec), ["DEC"], () => new ScenariiDec(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiDeschidere), ["DESCHIDERE"], () => new ScenariiDeschidere(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiNir), ["NIR"], () => new ScenariiNir(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiLdi), ["LDI"], () => new ScenariiLdi(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiAsm), ["ASM"], () => new ScenariiAsm(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiDvi), ["DVI"], () => new ScenariiDvi(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiRlf), ["RLF"], () => new ScenariiRlf(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiRdc), ["RDC"], () => new ScenariiRdc(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiNtc), ["NTC"], () => new ScenariiNtc(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiRegim), ["REGIM"], () => new ScenariiRegim(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiItv), ["ITV"], () => new ScenariiItv(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiFct), ["FCT"], () => new ScenariiFct(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiTaiere), ["X", "DSC"], () => new ScenariiTaiere(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiConsumatori), ["X"], () => new ScenariiConsumatori(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna), s.ConnectionString).Ruleaza()),
            new(nameof(ScenariiTrezorerie), ["PLT", "INC"], () => new ScenariiTrezorerie(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiBalanta), ["CITIRI"], () => VerificaBalanta.Ruleaza(s, privat)),
            new(nameof(ScenariiFisa), ["CITIRI"], () => VerificaFisaJurnal.Ruleaza(s, privat)),
            new(nameof(ScenariiStocCub), ["CITIRI"], () => new ScenariiStocCub(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiPartideCub), ["CITIRI"], () => new ScenariiPartideCub(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiFiscale), ["CITIRI", "FISCALE"], () => new ScenariiFiscale(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiTvaIntervale), ["CITIRI", "FISCALE"], () => new ScenariiTvaIntervale(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiSaft), ["SAFT"], () => new ScenariiSaft(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(VerificaSaft), ["SAFT"], () => VerificaSaft.Ruleaza(s, privat)),
            new(nameof(VerificaSaftStocuri), ["SAFT"], () => VerificaSaftStocuri.Ruleaza(s, privat)),
            new(nameof(ScenariiSaftStocuri), ["SAFT"], () => new ScenariiSaftStocuri(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiSnapshotStoc), ["CITIRI"], () => new ScenariiSnapshotStoc(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiExplicatii), ["CITIRI"], () => new ScenariiExplicatii(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiConcurenta), ["X"], () => new ScenariiConcurenta(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiCitiri), ["CITIRI"], () => new ScenariiCitiri(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiBtr), ["BTR"], () => new ScenariiBtr(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiVanzare), privat ? ["FCL", "DSC"] : ["FCL"], () => new ScenariiVanzare(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(ScenariiBcs), ["BCS"], () => new ScenariiBcs(
                s.Deschide, s.Check, privat,
                (os, an, luna) => s.InchideAcceptTot(os, an, luna)).Ruleaza()),
            new(nameof(VerificaNucleuBcs), ["BCS"], () => VerificaNucleuBcs.Ruleaza(s, privat)),
            new(nameof(VerificaNucleuBtr), ["BTR"], () => VerificaNucleuBtr.Ruleaza(s, privat)),
        };
        if (privat) {
            scene.Add(new(nameof(VerificaNucleuTrezorerie), ["PLT", "INC"], () => VerificaNucleuTrezorerie.Ruleaza(s, privat)));
            scene.Add(new(nameof(VerificaNucleuFct), ["FCT"], () => VerificaNucleuFct.Ruleaza(s, privat)));
        }
        scene.Add(new(nameof(VerificaNucleuFclDsc), ["FCL", "DSC"], () => VerificaNucleuFclDsc.Ruleaza(s, privat)));
        return scene;
    }
}
