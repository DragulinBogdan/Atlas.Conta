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

// ---------------------------------------------------------------------------
// F24-R1…R3 — o singură listă a tipurilor configurabile, un singur rol
// ---------------------------------------------------------------------------
// 83i: lista e DECLARATĂ, nu dedusă; REFLECȚIA e a probei, în ambele sensuri.
// Un tip `ICuProvenienta` nou care n-a intrat în listă rămâne fără etichetă în
// raportul de profil și fără drept pentru `Configurator` — se află aici, nu pe
// baza clientului.
//
// Rolul se probează pe RÂNDURILE de permisiuni: ModelCheck n-are strategie de
// securitate (80i), deci comportamentul (403 vs 422, ordinea de pe sârmă) se
// măsoară pe HTTP, în `refuzuri.ps1`.
static class VerificaF24Rol {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";

        var dinReflectie = typeof(ICuProvenienta).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICuProvenienta).IsAssignableFrom(t))
            .ToHashSet();
        var dinLista = Politici.TipuriConfigurabile.ToHashSet();
        var lipsaDinLista = dinReflectie.Except(dinLista).Select(t => t.Name).OrderBy(n => n).ToList();
        var inPlusInLista = dinLista.Except(dinReflectie).Select(t => t.Name).OrderBy(n => n).ToList();
        var asteptateCitite = Politici.TipuriConfigurabile.Concat([typeof(Partener), typeof(Produs)])
            .OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
        var cititeOk = Atlas.Conta.BackOffice.Module.Api.Politici.PoliticiApply.TipuriCitite
            .SequenceEqual(asteptateCitite);
        Console.WriteLine($"     MĂSURAT (F24-R1/{eticheta}): {dinLista.Count} tipuri în listă, "
            + $"{dinReflectie.Count} `ICuProvenienta` concrete în assembly, "
            + $"{Atlas.Conta.BackOffice.Module.Api.Politici.PoliticiApply.TipuriCitite.Count} tipuri citite de "
            + "raport"
            + (lipsaDinLista.Count > 0 ? $"; LIPSESC din listă: {string.Join(", ", lipsaDinLista)}" : "")
            + (inPlusInLista.Count > 0 ? $"; în PLUS în listă: {string.Join(", ", inPlusInLista)}" : "") + ".");
        s.Check($"F24-R1 ({eticheta}) `Politici.TipuriConfigurabile` == mulțimea tipurilor concrete "
            + "`ICuProvenienta` din assembly-ul Module, în AMBELE sensuri (22 de tipuri), iar "
            + "`PoliticiApply.TipuriCitite` == lista ∪ {Partener, Produs}: lista declarată și descoperirea prin "
            + "reflecție nu pot diverge fără să pice proba",
            lipsaDinLista.Count == 0 && inPlusInLista.Count == 0 && dinLista.Count == 22 && cititeOk);

        string exceptieRaport = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            try { VerificareProfilService.Raporteaza(os); }
            catch (InvalidOperationException e) { exceptieRaport = e.Message; }
        }
        s.Check($"F24-R2 ({eticheta}) raportul de profil are etichetă pentru FIECARE tip din listă: bucla cere "
            + "dicționarul de etichete ÎNAINTE de a interoga tabelul, deci un tip fără etichetă oprește raportul "
            + "cu numele lui, nu-l sare tăcut"
            + (exceptieRaport == null ? "" : $" — a aruncat: {exceptieRaport}"),
            exceptieRaport == null);

        // Două treceri: pe privat rolul nu există (se creează), pe bugetar există
        // din updater-ul host-ului (se realiniază). Niciuna nu are voie să dubleze.
        using (var os = s.Provider.CreateObjectSpace()) {
            Updater.SeedRolConfigurator(os);
            os.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            Updater.SeedRolConfigurator(os);
            os.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var randuri = os.GetObjectsQuery<PermissionPolicyRole>().Count(r => r.Name == "Configurator");
            var rol = os.GetObjectsQuery<PermissionPolicyRole>().ToList()
                .FirstOrDefault(r => r.Name == "Configurator");
            var permisiuni = rol?.TypePermissions ?? [];
            bool Scrie(Type tip) => permisiuni.Any(p => p.TargetTypeFullName == tip.FullName
                && p.CreateState == SecurityPermissionState.Allow
                && p.WriteState == SecurityPermissionState.Allow
                && p.DeleteState == SecurityPermissionState.Allow);
            var faraDrept = Politici.TipuriConfigurabile.Where(t => !Scrie(t)).Select(t => t.Name).ToList();
            Type[] interzise = [typeof(Document), typeof(RegistruContabil), typeof(RegistruStoc),
                typeof(RegistruTva), typeof(RegistruImobilizari), typeof(Atlas.Conta.BackOffice.Module.Cub.Postare),
                typeof(Atlas.Conta.BackOffice.Module.Cub.Tranzactie), typeof(Societate), typeof(SetareProfil),
                typeof(ApplicationUser), typeof(PermissionPolicyRole)];
            var scrieriInterzise = interzise.Where(t => permisiuni.Any(p =>
                    p.TargetTypeFullName == t.FullName
                    && (p.CreateState == SecurityPermissionState.Allow
                        || p.WriteState == SecurityPermissionState.Allow
                        || p.DeleteState == SecurityPermissionState.Allow)))
                .Select(t => t.Name).ToList();
            var politicaOk = rol?.PermissionPolicy == SecurityPermissionPolicy.ReadOnlyAllByDefault;
            Console.WriteLine($"     MĂSURAT (F24-R3/{eticheta}): {randuri} rând(uri) `Configurator` după două "
                + $"apeluri; {permisiuni.Count} permisiuni de tip; politica {rol?.PermissionPolicy}"
                + (faraDrept.Count > 0 ? $"; FĂRĂ drept de scriere: {string.Join(", ", faraDrept)}" : "")
                + (scrieriInterzise.Count > 0 ? $"; scriere INTERZISĂ acordată pe: "
                    + string.Join(", ", scrieriInterzise) : "") + ".");
            s.Check($"F24-R3 ({eticheta}) `Updater.SeedRolConfigurator` e idempotent (un singur rând după două "
                + "apeluri) și scrie exact separarea din 83h: `ReadOnlyAllByDefault` (deci Read și Navigate pe "
                + "tot, fără enumerare) + Create/Write/Delete pe fiecare tip din listă, și pe niciun document, "
                + "registru, `Postare`, `Tranzactie`, `Societate`, `SetareProfil`, user sau rol",
                randuri == 1 && politicaOk && faraDrept.Count == 0 && scrieriInterzise.Count == 0);
        }
    }
}
