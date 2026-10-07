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

// ===================== Scenariul e2e 1C-a: NotaContabila =====================
// Nota contabilă (design FAZA 1C §5): tipul FĂRĂ nicio regulă de contare —
// postarea explicită COMPLETĂ a liniei bate ABSENȚA regulii (mecanismul 32a
// extins în motor). Acoperă: valoarea CA ATARE (inclusiv negativă — nota storno
// de import), repartitorul explicit vs. default-ul polimorf, laturile interne,
// gardianul generic de dimensiuni obligatorii per cont (628 cere E), anularea
// directă, re-operarea și storno-ul.
static class E2eNotaContabila {
    public static void Ruleaza(Suita s) {
        const string MarcajNtc = "E2E-NTC";

        void CurataNtc(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtc)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            pj.Adauga(docs);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajNtc)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajNtc + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataNtc(os);

            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            // Conturi CPLAN: 581.x și 623 fără defalcare obligatorie; 628 cere E.
            var cont581a = os.FirstOrDefault<Cont>(c => c.Simbol == "581.01.01");
            var cont581b = os.FirstOrDefault<Cont>(c => c.Simbol == "581.01.02");
            var cont623 = os.FirstOrDefault<Cont>(c => c.Simbol == "623.00.00");
            var cont628 = os.FirstOrDefault<Cont>(c => c.Simbol == "628.00.00");

            var tipNtc = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");
            s.Check("Seed NTC: ancora TipDocument + numerotare NTC-, FĂRĂ reguli de stoc/contare și fără politici de TVA/scadență/validare",
                tipNtc != null && tipNtc.ClrType == nameof(NotaContabila)
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "NTC")?.Serie == "NTC-"
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipNtc.ID)
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipNtc.ID)
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipNtc.ID) == null
                && os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipNtc.ID) == null
                && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipNtc.ID) == null);
            s.Check("Precondiție de plan: 581.01.01/581.01.02/623.00.00 fără defalcare, 628.00.00 cere cod economic",
                cont581a.DimensiuniObligatorii == DimensiuneFlags.Niciuna
                && cont581b.DimensiuniObligatorii == DimensiuneFlags.Niciuna
                && cont623.DimensiuniObligatorii == DimensiuneFlags.Niciuna
                && cont628.DimensiuniObligatorii.HasFlag(DimensiuneFlags.CodEconomic));

            // Laturile notei = repartitori INTERNI (convenția tipului); partenerul de
            // probă există doar pentru refuzul de latură.
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = MarcajNtc + "-UI";
            unitate.Denumire = "Unitate probă notă contabilă";
            var partener = os.CreateObject<Partener>();
            partener.Cod = MarcajNtc + "-PART";
            partener.Denumire = "Partener probă notă contabilă";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajNtc + "-CE";
            codEc.Denumire = "Cod economic probă notă contabilă";
            os.CommitChanges();

            // --- Nota: două linii, a doua NEGATIVĂ (storno de notă din import) ---
            var ntc = os.CreateObject<NotaContabila>();
            ntc.Data = new DateOnly(2026, 4, 6);
            ntc.Predator = sediu;
            ntc.Primitor = unitate;
            var linieViramente = os.CreateObject<NotaContabilaDetaliu>();
            linieViramente.Document = ntc;
            linieViramente.TipMaterial = tipTrz;
            linieViramente.Descriere = "Virament intern";
            linieViramente.ContDebit = cont581a;
            linieViramente.ContCredit = cont581b;
            linieViramente.Valoare = 100m;
            linieViramente.RepartitorDebit = mag1; // postare explicită și pe repartitor
            var linieStorno = os.CreateObject<NotaContabilaDetaliu>();
            linieStorno.Document = ntc;
            linieStorno.TipMaterial = tipTrz;
            linieStorno.Descriere = "Corecție cu minus (notă storno)";
            linieStorno.ContDebit = cont623;
            linieStorno.ContCredit = cont581a;
            linieStorno.Valoare = -40m;
            os.CommitChanges();

            s.Check("Nota contabilă nu generează conex/secundar", MotorOperare.Opereaza(os, ntc) == null);
            s.Check("Operare → stare Operat + număr din politică (NTC-)",
                ntc.Stare == StareDocument.Operat && ntc.Numar?.StartsWith("NTC-") == true);
            s.Check("Nota nu mișcă stoc",
                CubScena.FaraStoc(os, ntc.ID));

            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();
            var noteNtcCub = NoteCub(ntc);
            s.Check("Conturile = cele EXPLICITE ale liniilor (581.01.01 = 581.01.02; 623 = 581.01.01)",
                noteNtcCub.Nota(cont581a.ID, cont581b.ID, null, linieViramente.ID)
                && noteNtcCub.Nota(cont623.ID, cont581a.ID, null, linieStorno.ID));
            s.Check("Valorile se postează CA ATARE, inclusiv negativa (100 / −40), fără flag de storno",
                noteNtcCub.Count == 4
                && noteNtcCub.Where(p => p.LinieId == linieViramente.ID).All(p => p.Valoare == 100m && p.Fel == N.FelTranzactie.Operare)
                && noteNtcCub.Where(p => p.LinieId == linieStorno.ID).All(p => p.Valoare == -40m && p.Fel == N.FelTranzactie.Operare)
                && noteNtcCub.Count(p => p.LinieId == linieViramente.ID) == 2 && noteNtcCub.Count(p => p.LinieId == linieStorno.ID) == 2);
            s.Check("D9-A10 NTC: debitul cules MAG1 pe prima linie, implicitul predator SEDIU pe a doua; creditul implicit primitor pe ambele",
                noteNtcCub.Single(p => p.Debit && p.LinieId == linieViramente.ID).Gestiune == mag1.ID
                && noteNtcCub.Single(p => p.Debit && p.LinieId == linieStorno.ID).Gestiune == sediu.ID
                && noteNtcCub.Where(p => p.Credit).All(p => p.Gestiune == unitate.ID));

            // --- Refuzurile: invarianții tipului + gardianul generic de dimensiuni ---
            void RefuzNtc(string nume, Repartitor predator, Repartitor primitor, Action<NotaContabilaDetaliu> configurare) {
                var doc = os.CreateObject<NotaContabila>();
                doc.Data = new DateOnly(2026, 4, 7);
                doc.Predator = predator;
                doc.Primitor = primitor;
                var linie = os.CreateObject<NotaContabilaDetaliu>();
                linie.Document = doc;
                linie.TipMaterial = tipTrz;
                linie.ContDebit = cont581a;
                linie.ContCredit = cont581b;
                linie.Valoare = 10m;
                configurare(linie);
                os.CommitChanges();
                s.CheckRefuza(nume, () => MotorOperare.Opereaza(os, doc));
                s.Check(nume + " — fără rânduri-fantomă în ObjectSpace (33d)",
                    CubScena.FaraNote(os, doc.ID));
                os.Delete(doc.Detalii.ToList());
                os.Delete(doc);
                os.CommitChanges();
            }

            RefuzNtc("Linie fără cont creditor → refuz", sediu, unitate, l => l.ContCredit = null);
            RefuzNtc("Linie cu valoare 0 → refuz", sediu, unitate, l => l.Valoare = 0m);
            RefuzNtc("Latură cu Partener (nota are laturi interne) → refuz", sediu, partener, _ => { });
            RefuzNtc("Cont cu defalcare obligatorie (628 cere E) fără cod economic → refuzul gardianului generic",
                sediu, unitate, l => l.ContDebit = cont628);

            // Linia de BAZĂ ar fi sărită mut de motor (n-are postare explicită) — refuz
            // explicit, ca pe DSC/FCL (38c).
            var ntcBaza = os.CreateObject<NotaContabila>();
            ntcBaza.Data = new DateOnly(2026, 4, 7);
            ntcBaza.Predator = sediu;
            ntcBaza.Primitor = unitate;
            var linieBaza = os.CreateObject<DocumentDetaliu>(); // NU NotaContabilaDetaliu
            linieBaza.Document = ntcBaza;
            linieBaza.TipMaterial = tipTrz;
            linieBaza.Valoare = 10m;
            os.CommitChanges();
            s.CheckRefuza("Linie de bază DocumentDetaliu («detaliu generic») pe notă → refuz",
                () => MotorOperare.Opereaza(os, ntcBaza));
            os.Delete(ntcBaza.Detalii.ToList());
            os.Delete(ntcBaza);
            os.CommitChanges();

            // Același cont 628, cu codul economic cules pe linie → gardianul e satisfăcut.
            var ntcDefalcare = os.CreateObject<NotaContabila>();
            ntcDefalcare.Data = new DateOnly(2026, 4, 8);
            ntcDefalcare.Predator = sediu;
            ntcDefalcare.Primitor = unitate;
            var linieDefalcare = os.CreateObject<NotaContabilaDetaliu>();
            linieDefalcare.Document = ntcDefalcare;
            linieDefalcare.TipMaterial = tipTrz;
            linieDefalcare.ContDebit = cont628;
            linieDefalcare.ContCredit = cont581a;
            linieDefalcare.Valoare = 55m;
            linieDefalcare.CodEconomicId = codEc.ID;
            os.CommitChanges();
            MotorOperare.Opereaza(os, ntcDefalcare);
            var debitDefalcareCub = NoteCub(ntcDefalcare).Where(p => p.Debit).ToList();
            s.Check("628 cu cod economic cules pe linie → operare acceptată, dimensiunea pe latura contului",
                ntcDefalcare.Stare == StareDocument.Operat
                && debitDefalcareCub.Count == 1 && debitDefalcareCub[0].CodEconomic == codEc.ID);

            // --- Corecție directă → re-operare → storno ---
            var numarNtc = ntc.Numar;
            MotorOperare.AnuleazaOperarea(os, ntc);
            s.Check("Anulare directă → Draft + registrele goale",
                ntc.Stare == StareDocument.Draft
                && CubScena.FaraNote(os, ntc.ID));
            MotorOperare.Opereaza(os, ntc);
            s.Check("Re-operare → același număr, aceleași 2 rânduri (100 / −40)",
                ntc.Stare == StareDocument.Operat && ntc.Numar == numarNtc
                && NoteCub(ntc).Where(p => p.Debit).Sum(p => p.Valoare) == 60m
                && NoteCub(ntc).Where(p => p.Credit).Sum(p => p.Valoare) == 60m);
            MotorOperare.Storneaza(os, ntc, new DateOnly(2026, 7, 22));
            var stornoNtcCub = CubScena.Note(os, ntc.ID).Where(p => p.Storno).ToList();
            s.Check("Storno → rânduri inverse append-only la data stornării (−100 / +40, nota negativă devine pozitivă)",
                ntc.Stare == StareDocument.Stornat
                && stornoNtcCub.Count(p => p.Debit && p.Data == new DateOnly(2026, 7, 22)
                    && (p.Valoare == -100m || p.Valoare == 40m)) == 2
                && stornoNtcCub.Count(p => p.Credit && p.Data == new DateOnly(2026, 7, 22)
                    && (p.Valoare == -100m || p.Valoare == 40m)) == 2);

            CurataNtc(os);
            s.Check("Curățenie finală notă contabilă (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajNtc)));
        }
    }
}
