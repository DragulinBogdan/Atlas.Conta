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

// S-D6 — `Pozitie` pe `DocumentDetaliu`: ordinea de CULEGERE, atribuită o
// singură dată, în `SaveChanges`-ul contextului (o ușă pentru UI, WebApi,
// Import1C și conexul clonat). Documentul rămâne Draft: proba e despre
// SALVARE, nu despre motor.
static class VerificaPozitieLinii {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        var idDoc = new Guid("57a00000-0000-0000-0000-000000000031");
        using var os = s.Provider.CreateObjectSpace();

        void CurataPozitie() {
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == idDoc));
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => d.ID == idDoc));
            pj.Executa();
        }
        CurataPozitie();

        var gestiune = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var tipMaterial = os.FirstOrDefault<TipMaterial>(t => t.Cod == (privat ? "302" : "302.01.00"));
        var doc = os.CreateObject<BonConsum>();
        doc.ID = idDoc;
        doc.Data = new DateOnly(2026, 3, 1);
        doc.DataInregistrare = doc.Data;
        doc.Predator = gestiune;
        doc.Primitor = gestiune;
        DocumentDetaliu Linie(decimal cantitate) {
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = doc;
            linie.TipMaterial = tipMaterial;
            linie.Cantitate = cantitate;
            return linie;
        }

        var primele = new[] { Linie(1m), Linie(2m), Linie(3m) };
        os.CommitChanges();
        Console.WriteLine($"     MĂSURAT (STR-POZITIE/{eticheta}): prima salvare → "
            + $"[{string.Join(", ", primele.Select(l => l.Pozitie))}].");
        s.Check($"STR-POZITIE-1 ({eticheta}) cele trei linii noi primesc `Pozitie` 1, 2, 3 în ordinea adăugării, la "
            + "salvare (B-r11)",
            primele.Select(l => l.Pozitie).SequenceEqual([1, 2, 3]));

        var aPatra = Linie(4m);
        os.CommitChanges();
        Console.WriteLine($"     MĂSURAT (STR-POZITIE/{eticheta}): a doua salvare → a patra linie {aPatra.Pozitie}, "
            + $"primele [{string.Join(", ", primele.Select(l => l.Pozitie))}].");
        s.Check($"STR-POZITIE-2 ({eticheta}) a patra linie, adăugată la a DOUA salvare, ia `Pozitie` 4 (max + 1 din "
            + "bază), iar primele trei rămân neschimbate — poziția se atribuie o singură dată",
            aPatra.Pozitie == 4 && primele.Select(l => l.Pozitie).SequenceEqual([1, 2, 3]));

        CurataPozitie();
        using var osFinal = s.Provider.CreateObjectSpace();
        s.Check($"STR-POZITIE — curățenie finală ({eticheta}): documentul de probă și liniile lui nu mai există",
            !osFinal.GetObjectsQuery<Document>().Any(d => d.ID == idDoc)
            && !osFinal.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idDoc));
    }
}
