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

// ═══════════ Felia 23 — implicitele de culegere și întreținerea politicilor ═══════════
// Șase grupuri, în ordinea în care se sprijină unul pe altul: modelul (F23-V1)
// spune că schema apără cheile; rezolvarea (V2) e funcția pe care o cheamă toate
// cele trei uși; seed-ul (V3) e cel care umple tabelul și îl repară idempotent;
// gardianul (V4) e jumătatea de FOND a validării, fiindcă regulile XAF nu rulează
// pe API (55b); raportul (V5) e cel care ARATĂ ce s-a abătut de la profilul
// livrat; iar V6 măsoară că funcția legii, mutată din D394, dă aceleași cifre.
//
// Convenția de scenă: marcajul `E2E-F23` și purjă FIZICĂ la final (70e) —
// `os.Delete` DOAR acolo unde ștergerea logică e chiar obiectul probei (V3:
// „șters de utilizator”; V5: referința spre un rând șters).

// ---------------------------------------------------------------------------
// F23-V1 — MODELUL: unicitatea din F23-D3 e în SCHEMĂ, nu în convenție
// ---------------------------------------------------------------------------
// Până la felia asta nouă politici per tip de document și cinci coduri de
// nomenclator erau chei DOAR prin convenție (seed-ul le trata ca atare, motorul
// le citea cu `FirstOrDefault`). Proba citește indexurile din modelul
// DESIGN-TIME — același din care iese migrația, singurul care poartă filtrele și
// adnotările relaționale — și le compară LITERAL, ca la D17-V1.
//
// Cele trei chei cu coloane nullable au nevoie de `NULLS NOT DISTINCT`: în
// Postgres `NULL <> NULL`, deci fără adnotare două rânduri „orice clasă” /
// „regulă generică” ar fi trecut nestingherite pe aceeași cheie — exact dublura
// pe care indexul există s-o oprească.
static class VerificaF23Model {
    public static void Ruleaza(Suita s, bool privat) {
        using var os = s.Provider.CreateObjectSpace();
        var ctxModel = ((EFCoreObjectSpace)os).DbContext;
        var model = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions
            .GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>(ctxModel).Model;

        // Tip · coloanele cheii · cheia are nullable-uri (⇒ `NULLS NOT DISTINCT`)
        (Type Tip, string[] Coloane, bool NullsNotDistinct)[] asteptate = [
            (typeof(PoliticaTva), [nameof(PoliticaTva.TipDocumentId)], false),
            (typeof(PoliticaConex), [nameof(PoliticaConex.TipDocumentSursaId)], false),
            (typeof(PoliticaScadenta), [nameof(PoliticaScadenta.TipDocumentId)], false),
            (typeof(PoliticaValidare), [nameof(PoliticaValidare.TipDocumentId)], false),
            (typeof(PoliticaNumerotare), [nameof(PoliticaNumerotare.TipDocumentId)], false),
            (typeof(PoliticaInchidereTva), [nameof(PoliticaInchidereTva.TipDocumentId)], false),
            (typeof(RegulaStoc), [nameof(RegulaStoc.TipDocumentId), nameof(RegulaStoc.Latura),
                nameof(RegulaStoc.ClasaId)], true),
            (typeof(RegulaContare), [nameof(RegulaContare.TipDocumentId), nameof(RegulaContare.TipMaterialId),
                nameof(RegulaContare.NaturaFiltru), nameof(RegulaContare.SemnFiltru)], true),
            (typeof(PoliticaTvaImplicit), [nameof(PoliticaTvaImplicit.TipDocumentId),
                nameof(PoliticaTvaImplicit.ClasaFiscala), nameof(PoliticaTvaImplicit.ValabilDeLa)], true),
            (typeof(TipDocument), [nameof(TipDocument.Cod)], false),
            (typeof(TipDocument), [nameof(TipDocument.ClrType)], false),
            (typeof(TipTva), [nameof(TipTva.Cod)], false),
            (typeof(Cont), [nameof(Cont.Simbol)], false),
            (typeof(ClasaProdus), [nameof(ClasaProdus.Cod)], false),
            (typeof(TipMaterial), [nameof(TipMaterial.Cod)], false),
        ];

        var lipsuri = new List<string>();
        var masurate = new List<string>();
        foreach (var (tip, coloane, nnd) in asteptate) {
            var et = model.FindEntityType(tip);
            var index = et?.GetIndexes().FirstOrDefault(i =>
                i.Properties.Select(p => p.Name).SequenceEqual(coloane));
            var cheie = $"{tip.Name}({string.Join(",", coloane)})";
            if (index == null) {
                lipsuri.Add($"{cheie}: LIPSĂ");
                continue;
            }
            var filtru = index.GetFilter();
            // `GetAreNullsDistinct` e forma de CITIRE a lui `AreNullsDistinct(false)`
            // (Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3, `NpgsqlIndexExtensions`):
            // builder-ul scrie, extensia asta citește.
            var distincte = index.GetAreNullsDistinct();
            masurate.Add($"{cheie} unic={index.IsUnique} filtru=„{filtru ?? "<niciunul>"}” "
                + $"nullsDistinct={distincte?.ToString() ?? "<neconfigurat>"}");
            if (!index.IsUnique)
                lipsuri.Add($"{cheie}: nu e UNIC");
            if (filtru != null)
                lipsuri.Add($"{cheie}: filtru „{filtru ?? "<niciunul>"}”");
            // `GetAreNullsDistinct` întoarce `bool?`: `null` = neconfigurat, adică
            // default-ul Postgres („NULL <> NULL”, deci nulurile SUNT distincte).
            // Cele trei chei cu nullable-uri cer explicit `false`; celelalte n-au
            // voie să-l poarte. `null` NU e o trecere pentru primele — de-aia
            // comparația e pe `!= false`, nu pe `== nnd`.
            if (nnd ? distincte != false : distincte == false)
                lipsuri.Add($"{cheie}: AreNullsDistinct={distincte?.ToString() ?? "<neconfigurat>"}, "
                    + $"așteptat {(nnd ? "false" : "neconfigurat/true")}");
        }
        Console.WriteLine($"     MĂSURAT (F23-V1/indexuri): {asteptate.Length} chei; "
            + string.Join("; ", masurate) + ".");
        s.Check($"F23-V1 ({(privat ? "privat" : "bugetar")}) cele 15 chei din F23-D3 sunt indexuri UNICE în model, "
            + "nefiltrate (104f), iar cele TREI cu coloane nullable (`PoliticaTvaImplicit`, `RegulaStoc`, `RegulaContare`) "
            + "poartă `NULLS NOT DISTINCT` — fără el două rânduri „orice clasă” / „regulă generică” ar fi trecut, "
            + "iar motorul ar fi devenit nedeterminist TĂCUT"
            + (lipsuri.Count > 0 ? $" — abateri: {string.Join(", ", lipsuri)}" : ""),
            lipsuri.Count == 0);

        // `Activ` vine din familia `Nomenclator` (104e): inițializatorul `= true` face rândul nou viu. Fără
        // default în schemă — pe un `bool` el ar rescrie `false` în `true` la inserare (santinela EF), iar
        // bazele se recreează (102b), deci nu există rânduri de păstrat vii la adăugarea coloanei.
        var propActiv = model.FindEntityType(typeof(TipTva))?.FindProperty(nameof(TipTva.Activ));
        Console.WriteLine($"     MĂSURAT (F23-V1/Activ): default în model = "
            + $"{propActiv?.FindAnnotation(Microsoft.EntityFrameworkCore.Metadata.RelationalAnnotationNames.DefaultValue)?.Value ?? "<niciunul>"}; nullable={propActiv?.IsNullable}.");
        s.Check("F23-V1 `TipTva.Activ` e NOT NULL, fără default în schemă, iar un tip nou e viu (104e)",
            propActiv != null && !propActiv.IsNullable
            && propActiv.FindAnnotation(Microsoft.EntityFrameworkCore.Metadata.RelationalAnnotationNames.DefaultValue) == null
            && new TipTva().Activ);

        // Proveniența (F23-D4) se descoperă prin REFLECȚIE pe assembly-ul Module, nu
        // dintr-o listă scrisă aici: o politică nouă care declară interfața intră
        // automat în probă, iar una care o pierde pică.
        var tipuriProvenienta = typeof(ICuProvenienta).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICuProvenienta).IsAssignableFrom(t))
            .OrderBy(t => t.Name, StringComparer.Ordinal).ToList();
        var faraColoana = tipuriProvenienta
            .Where(t => model.FindEntityType(t)?.FindProperty(nameof(ICuProvenienta.DinSeed)) == null)
            .Select(t => t.Name).ToList();
        Console.WriteLine($"     MĂSURAT (F23-V1/proveniență): {tipuriProvenienta.Count} tipuri `ICuProvenienta` "
            + $"({string.Join(", ", tipuriProvenienta.Select(t => t.Name))})"
            + (faraColoana.Count > 0 ? $"; FĂRĂ coloană: {string.Join(", ", faraColoana)}" : "; toate au coloană")
            + ".");
        s.Check("F23-V1 `DinSeed` există ca proprietate MAPATĂ pe toate cele 22 de tipuri `ICuProvenienta`, "
            + "inclusiv maparea fiscală SAF-T — lista se descoperă prin reflecție",
            tipuriProvenienta.Count == 22 && faraColoana.Count == 0);
    }
}
