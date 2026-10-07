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

// ============ F13-D1: axa pe care stă regula, pe AMBELE profiluri (56f) ============
// Scenele de fond ale taxării inverse trăiesc în blocul privat (FCL fără taxă,
// RLF cu autolichidare, gardul pe TVA-ul cules). Aici se verifică doar PREMISA
// lor — că sensul vine din `PoliticaTva.Directie`, nu dintr-un câmp al lui
// `TipTva` sau din clasa documentului — și se spune EXPLICIT de ce profilul
// bugetar n-are ce proba, în loc ca absența să treacă drept trecere.
static class VerificaAxaTaxareInversa {
    public static void Ruleaza(Suita s) {
        using var os = s.Provider.CreateObjectSpace();
        var tiTest = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
        if (tiTest == null)
            Console.WriteLine($"SKIP F13-D1 (taxarea inversă pe livrare): profilul {s.Profil} n-are TI21 în "
                + "nomenclatorul TipTva — bugetarul e neplătitor (nicio PoliticaTva, niciun regim de taxare "
                + "inversă seed-uit), deci pe latura asta nu există operațiune de probat.");
        else {
            var fct = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT");
            var fcl = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL");
            s.Check("F13-D1 premisa: latura fiscală a unui tip de document se citește din `PoliticaTva.Directie` "
                + "(FCT → Deductibil, FCL → Colectat), nu de pe `TipTva` și nu din clasa documentului — "
                + "`TvaService.DirectiePentruTip` e singura sursă a sensului, pentru culegere și pentru motor",
                TvaService.DirectiePentruTip(os, fct.ID) == DirectieTva.Deductibil
                && TvaService.DirectiePentruTip(os, fcl.ID) == DirectieTva.Colectat
                // …iar un tip fără politică rămâne `null` = „nu e eveniment de TVA":
                // exact cazul în care motorul nu postează nimic, deci `CalculeazaValori`
                // păstrează comportamentul dinainte de F13.
                && TvaService.DirectiePentruTip(os,
                    os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR").ID) == null);

            // Inventarul datelor de DINAINTE de F13 (review F13, defect 2): o livrare în
            // taxare inversă operată de vechiul motor are `ValoareTva ≠ 0` pe linie,
            // 4426 = 4427 în registru și TVA pe rândul fiscal. Felia nu le migrează
            // (decizia 70): se RAPORTEAZĂ — pe baza asta trebuie să fie zero, fiindcă
            // scenele își purjează urmele; pe o bază reală cifra e decizia
            // utilizatorului (anulare + golire + re-operare, sau storno).
            var candidate = os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => d.ValoareTva != 0m && d.TipTva.Regim == RegimTva.TaxareInversa)
                .Select(d => new { d.ID, d.DocumentId }).ToList();
            var docIds = candidate.Select(c => c.DocumentId).Distinct().ToList();
            var peLivrare = 0;
            foreach (var docId in docIds) {
                var doc = os.GetObjectByKey<Document>(docId);
                if (doc != null && TvaService.DirectiePentru(os, doc) == DirectieTva.Colectat)
                    peLivrare += candidate.Count(c => c.DocumentId == docId);
            }
            Console.WriteLine($"     MĂSURAT (F13-D1, date pre-F13): {candidate.Count} linii TI cu TVA nenul, "
                + $"din care {peLivrare} pe tipuri de LIVRARE (moștenire a vechiului motor).");
            s.Check("F13-D1 inventar pre-F13: pe baza de probă nicio livrare în taxare inversă nu mai poartă TVA "
                + "(scenele își purjează urmele; pe o bază reală cifra e raport, nu eșec)", peLivrare == 0);
        }
    }
}
