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

// ================= Scenariul e2e 1C-a: NotaContabila (privat) =================
// Nota e NEUTRĂ față de profil (fără plan hardcodat, fără politici în afara
// numerotării): aceleași mecanisme, conturi OMFP. La privat niciun cont nu
// cere dimensiuni, deci rămâne nucleul — postare explicită, valoare CA ATARE
// (inclusiv negativă), storno.
static class E2eNotaContabilaPrivat {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajNtcPrv = "E2E-NTP";

            void CurataNtcPrv(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtcPrv)).Select(r => r.ID).ToList();
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                pj.Adauga(docs);
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtcPrv)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataNtcPrv(os);

                var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
                var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
                var cont605 = os.FirstOrDefault<Cont>(c => c.Simbol == "605");
                var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
                var cont471 = os.FirstOrDefault<Cont>(c => c.Simbol == "471");

                var tipNtc = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");
                s.Check("Seed NTC privat: ancoră + numerotare NTC-, fără reguli de stoc/contare și fără PoliticaTva",
                    tipNtc != null
                    && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC")?.Serie == "NTC-"
                    && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipNtc.ID)
                    && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipNtc.ID)
                    && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipNtc.ID) == null);

                var unitate = os.CreateObject<UnitateInterna>();
                unitate.Cod = MarcajNtcPrv + "-UI";
                unitate.Denumire = "Unitate probă notă privat";
                os.CommitChanges();

                var ntc = os.CreateObject<NotaContabila>();
                ntc.Data = new DateOnly(2026, 4, 6);
                ntc.Predator = sediu;
                ntc.Primitor = unitate;
                var linieUtilitati = os.CreateObject<NotaContabilaDetaliu>();
                linieUtilitati.Document = ntc;
                linieUtilitati.TipMaterial = tipTrz;
                linieUtilitati.Descriere = "Utilități de regularizat";
                linieUtilitati.ContDebit = cont605;
                linieUtilitati.ContCredit = cont401;
                linieUtilitati.Valoare = 300m;
                var linieMinus = os.CreateObject<NotaContabilaDetaliu>();
                linieMinus.Document = ntc;
                linieMinus.TipMaterial = tipTrz;
                linieMinus.Descriere = "Reportare în avans (minus)";
                linieMinus.ContDebit = cont471;
                linieMinus.ContCredit = cont605;
                linieMinus.Valoare = -50m;
                os.CommitChanges();

                MotorOperare.Opereaza(os, ntc);
                s.Check("Privat: operare → Operat + număr NTC-",
                    ntc.Stare == StareDocument.Operat && ntc.Numar?.StartsWith("NTC-") == true);
                var notePrvCub = CubScena.Note(os, ntc.ID).Where(p => !p.Storno).ToList();
                s.Check("Privat: exact 2 rânduri, conturile OMFP explicite (605 = 401; 471 = 605)",
                    notePrvCub.Count == 4
                    && notePrvCub.Nota(cont605.ID, cont401.ID)
                    && notePrvCub.Nota(cont471.ID, cont605.ID));
                s.Check("Privat: valorile CA ATARE (300 / −50)",
                    notePrvCub.Any(p => p.Valoare == 300m) && notePrvCub.Any(p => p.Valoare == -50m));
                s.Check("D9-A10 NTC privat: linia fără repartitor ia implicitul laturii antetului (debit←predator SEDIU, credit←primitor)",
                    notePrvCub.Where(p => p.Debit).All(p => p.Gestiune == sediu.ID)
                    && notePrvCub.Where(p => p.Credit).All(p => p.Gestiune == unitate.ID));

                s.Check("Privat: nota nu postează TVA (fără TipTva pe linii) și nu mișcă stoc",
                    notePrvCub.All(p => p.LinieId != null)
                    && CubScena.FaraStoc(os, ntc.ID));

                MotorOperare.Storneaza(os, ntc, new DateOnly(2026, 7, 22));
                var stornoPrvCub = CubScena.Note(os, ntc.ID).Where(p => p.Storno).ToList();
                s.Check("Privat: storno → rânduri inverse (−300 / +50) la data stornării",
                    ntc.Stare == StareDocument.Stornat
                    && stornoPrvCub.All(p => p.Data == new DateOnly(2026, 7, 22))
                    && stornoPrvCub.Count(p => p.Debit && (p.Valoare == -300m || p.Valoare == 50m)) == 2
                    && stornoPrvCub.Count(p => p.Credit && (p.Valoare == -300m || p.Valoare == 50m)) == 2);

                CurataNtcPrv(os);
                s.Check("Curățenie finală notă contabilă privat (fără reziduuri e2e)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNtcPrv)));
            }
        }
    }
}
