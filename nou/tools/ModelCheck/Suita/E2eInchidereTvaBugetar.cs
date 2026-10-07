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

// ============ Scenariul e2e 1C-a: InchidereTva la BUGETAR (tip inert) ============
// Dovada agnosticismului: ancora ITV există în nucleu pentru ambele profiluri, dar
// conturile închiderii sunt DATE de profil (PoliticaInchidereTva) — fără rând,
// generatorul întoarce null și tipul rămâne inert, ca DSC/BPR (decizia 29).
static class E2eInchidereTvaBugetar {
    public static void Ruleaza(Suita s) {
        using (var os = s.Provider.CreateObjectSpace()) {
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipItv = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
            s.Check("Seed bugetar: ancora TipDocument ITV există (nucleu), FĂRĂ politică de închidere și fără numerotare",
                tipItv != null && tipItv.ClrType == nameof(InchidereTva)
                && os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocumentId == tipItv.ID) == null
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipItv.ID) == null);
            s.Check("Generatorul la bugetar → null (tip inert: conturile închiderii nu sunt hardcodate în motor)",
                InchidereTvaService.Genereaza(os, 2026, 9, sediu.ID) == null);
            s.Check("Bugetar: niciun document ITV creat de apelul de mai sus",
                !os.GetObjectsQuery<InchidereTva>().Any());

            // ── Felia API ITV la BUGETAR (F21-D9.8) ──
            // Ușa nu inventează un profil pe care baza nu-l are: previzualizarea spune
            // `ProfilInert` cu soldurile `null` (nu 0 — fără conturi nu există cifră, iar
            // un 0 acolo ar fi zis „n-ai ce închide" în loc de „profilul n-are închidere
            // de TVA"), iar lista e goală fiindcă tipul e inert, nu fiindcă am filtrat-o.
            var prevBugetar = InchidereTvaApply.Previzualizeaza(os, 2026, 9);
            s.Check("Api ITV la bugetar: `Previzualizeaza` ⇒ `ProfilInert` cu soldurile NULL (distincte de 0 = „luna "
                + "n-are ce închide”), fără document care să blocheze și cu cele trei linii zero",
                prevBugetar.Motiv == nameof(MotivNegenerare.ProfilInert)
                && prevBugetar.Sold4426 == null && prevBugetar.Sold4427 == null
                && prevBugetar.InchidereVieId == null
                && prevBugetar.Transfer == 0m && prevBugetar.DePlata == 0m && prevBugetar.DeRecuperat == 0m
                && prevBugetar.An == 2026 && prevBugetar.Luna == 9);
            var rezBugetar = InchidereTvaApply.Genereaza(os,
                new GenerareItvRequestDto { An = 2026, Luna = 9, UnitateId = sediu.ID });
            s.Check("Api ITV la bugetar: `Genereaza` ⇒ RAPORT cu `ProfilInert` (200, nu eroare), lista de închideri "
                + "rămâne GOALĂ și nu s-a scris nimic",
                rezBugetar.DocumentId == null && rezBugetar.Motiv == nameof(MotivNegenerare.ProfilInert)
                && rezBugetar.Sold4426 == null && rezBugetar.Sold4427 == null
                && !InchidereTvaApply.Lista(os).Any()
                && !os.GetObjectsQuery<InchidereTva>().Any());

            // Gardienii adăugați de review-ul 79 (`DraftAnterior`, `PerioadaInchisa`) NU
            // au mutat ordinea: profilul rămâne PRIMUL. Proba o măsoară pe o lună din
            // 2099, pentru care nu există `PerioadaFiscala` deloc — adică o perioadă pe
            // care `GardianPerioada` o tratează ca ÎNCHISĂ (14). Dacă gardianul de
            // perioadă ar fi ajuns înaintea celui de profil, motivul ar fi ieșit
            // `PerioadaInchisa`, iar bugetarul ar fi raportat despre calendar în loc de
            // profil — cu soldurile `null` devenite deodată explicabile altfel.
            var prevBugetar2099 = InchidereTvaApply.Previzualizeaza(os, 2099, 12);
            s.Check("Api ITV la bugetar: ordinea gardienilor e neschimbată — pe o lună fără `PerioadaFiscala` (12/2099, "
                + "deci tratată ca ÎNCHISĂ) motivul rămâne `ProfilInert`, nu `PerioadaInchisa`: un profil fără "
                + "închidere de TVA nu ajunge să vorbească despre calendar",
                prevBugetar2099.Motiv == nameof(MotivNegenerare.ProfilInert)
                && prevBugetar2099.Sold4426 == null && prevBugetar2099.Sold4427 == null);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var tipAsm = os.FirstOrDefault<TipDocument>(t => t.Cod == "ASM");
            s.Check("Seed bugetar: ancora TipDocument ASM există (nucleu), cu ClrType-ul clasei",
                tipAsm != null && tipAsm.ClrType == nameof(Asamblare));
            s.Check("Bugetar: ASM cu numerotare; fără reguli de stoc (D9-D8) și fără politici contabile/fiscale/scadență/validare",
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipAsm.ID)
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipAsm.ID)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipAsm.ID) != null
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipAsm.ID) == null
                && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipAsm.ID) == null
                && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipAsm.ID) == null);
        }
    }
}
