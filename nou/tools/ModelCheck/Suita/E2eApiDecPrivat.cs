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

// ======== Felia Api DEC — semantica override-ului de TVA + 4426 = 542 (privat) ========
// Complementul blocului bugetar `E2E-API-DEC` (F8-D13.1), pe același tipar ca
// FCT: la bugetar toate regimurile sunt Capitalizat, deci acolo override-ul
// are DOAR refuzuri; semantica POZITIVĂ (păstrare fără declanșator, cedare la
// schimbarea bazei) cere un regim cu TVA separat și trăiește aici. Nu se
// inventează tipuri de TVA în seedul bugetar pentru probe (decizia 21,
// precedentul 56f).
//
// În plus față de FCT: DEC e singurul tip cu PoliticaTva pe latura
// PREDATORULUI care e un ANGAJAT — rândul de TVA iese 4426 = 542 (bonul cu
// TVA deductibil justificat pe decont), iar creditul cade pe fallback-ul 542
// al regulii.
static class E2eApiDecPrivat {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajApiDecPrv = "E2E-APIDEC-PRV";
            using var os = s.Provider.CreateObjectSpace();
            void CurataApiDecPrv(IObjectSpace o) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(o);
                var repIds = o.GetObjectsQuery<Repartitor>()
                    .Where(x => x.Cod.StartsWith(MarcajApiDecPrv)).Select(x => x.ID).ToList();
                var docs = o.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(o.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                pj.Adauga(docs);
                pj.Adauga(o.GetObjectsQuery<Repartitor>().Where(x => x.Cod.StartsWith(MarcajApiDecPrv)).ToList());
                pj.Executa();
            }
            CurataApiDecPrv(os);

            var n21Dec = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var sddDec = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            var cont4426Dec = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
            var cont542Dec = os.FirstOrDefault<Cont>(c => c.Simbol == "542");
            // Tipul trebuie să aibă cont implicit: regula DEC rezolvă debitul din
            // `SursaCont.TipMaterial`, FĂRĂ fallback (32b).
            var tipCheltuiala = os.GetObjectsQuery<TipMaterial>()
                .First(t => t.Clasa.Natura == NaturaClasa.Serviciu && t.ContImplicitId != null);
            var titularPrv = os.CreateObject<Angajat>();
            titularPrv.Cod = MarcajApiDecPrv + "-ANG";
            titularPrv.Denumire = "Titular Api DEC Privat";
            var unitatePrv = os.CreateObject<UnitateInterna>();
            unitatePrv.Cod = MarcajApiDecPrv + "-U";
            unitatePrv.Denumire = "Unitate Api DEC Privat";
            os.CommitChanges();

            var wDec = new DecontWriteDto {
                Data = new DateOnly(2026, 3, 12),
                PredatorId = titularPrv.ID, PrimitorId = unitatePrv.ID,
                Linii = { new DecontLinieWriteDto {
                    TipMaterialId = tipCheltuiala.ID, Descriere = "Bon justificat",
                    Cantitate = 0m, PretUnitar = 100m, TipTvaId = n21Dec.ID } }
            };
            var idDecPrv = DecontApply.Aplica(os, null, wDec);
            var linieDecPrv = DecontApply.Citeste(os, idDecPrv).Linii[0];
            s.Check("Api DEC privat/N21: calculul la culegere — net 100 + TVA 21, cu cantitatea pro-forma 0 → 1 (F8-D2)",
                linieDecPrv is { Valoare: 100m, ValoareTva: 21m, Cantitate: 1m });
            wDec.Linii[0].Id = linieDecPrv.Id;
            wDec.Linii[0].ValoareTva = 21.37m;
            DecontApply.Aplica(os, idDecPrv, wDec);
            s.Check("Api DEC privat: override pe regim Normal → acceptat, aplicat DUPĂ calcul (36a — bonul bate rotunjirea)",
                DecontApply.Citeste(os, idDecPrv).Linii[0].ValoareTva == 21.37m);
            wDec.Linii[0].ValoareTva = null;
            DecontApply.Aplica(os, idDecPrv, wDec);
            s.Check("Api DEC privat: PUT ulterior FĂRĂ declanșatori (baza/TipTva neatinse) → override-ul PĂSTRAT",
                DecontApply.Citeste(os, idDecPrv).Linii[0].ValoareTva == 21.37m);
            wDec.Linii[0].PretUnitar = 200m;
            DecontApply.Aplica(os, idDecPrv, wDec);
            s.Check("Api DEC privat: schimbarea BAZEI redeclanșează calculul standard → override-ul cedează (200 + 42)",
                DecontApply.Citeste(os, idDecPrv).Linii[0] is { Valoare: 200m, ValoareTva: 42m });
            if (sddDec != null) {
                wDec.Linii[0].TipTvaId = sddDec.ID;
                wDec.Linii[0].ValoareTva = 5m;
                s.CheckRefuza("Api DEC privat: override pe regim Scutit (SDD) → refuz (regimul nu poartă TVA separat)",
                    () => DecontApply.Aplica(os, idDecPrv, wDec));
                wDec.Linii[0].TipTvaId = n21Dec.ID;
                wDec.Linii[0].ValoareTva = null;
                DecontApply.Aplica(os, idDecPrv, wDec);
            }
            ComenziDocument.Sistem(os).Opereaza(idDecPrv);
            var notePrvDecCub = CubScena.Note(os, idDecPrv).Where(p => !p.Storno).ToList();
            s.Check("Api DEC privat operat: nota principală (cheltuiala = 542, 200 net) + rândul de TVA 4426 = 542 (42) — PoliticaTva pe latura predatorului, care e ANGAJATUL",
                notePrvDecCub.Nota(tipCheltuiala.ContImplicitId, cont542Dec.ID, 200m)
                && notePrvDecCub.Nota(cont4426Dec.ID, cont542Dec.ID, 42m)
                && DecontApply.Citeste(os, idDecPrv) is { Total: 242m, PoateAnula: true });
            ComenziDocument.Sistem(os).AnuleazaOperarea(idDecPrv);
            DecontApply.Sterge(os, idDecPrv);
            CurataApiDecPrv(os);
            s.Check("Curățenie finală felia Api DEC privat (fără reziduuri e2e)",
                DecontApply.Citeste(os, idDecPrv) == null
                && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiDecPrv)));
        }
    }
}
