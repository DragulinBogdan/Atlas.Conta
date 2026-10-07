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

// ================= Scenariul e2e 1C-a: InchidereTva (privat) =================
// Închiderea lunară de TVA (design FAZA 1C §6) — forcing function-ul TVA-ului
// structural din P1: ITV E o notă contabilă GENERATĂ din soldurile registrului
// (InchidereTvaService), nu culeasă. Acoperă: transferul 4427=4426 pe minim +
// excedentul de plată (septembrie), idempotența pe draft existent, excedentul
// de recuperat 4424=4426 (octombrie — cumulativul dovedește că septembrie s-a
// închis la 0), regenerarea DUPĂ storno.
// Lunile 9/10 sunt NEFOLOSITE de celelalte blocuri (care lucrează în 3–7 și se
// curăță după ele) — precondiția de sold 0 se verifică explicit.
static class E2eInchidereTvaPrivat {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajItv = "E2E-ITV";

            void CurataItv(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajItv)).Select(r => r.ID).ToList();
                // Documentele după repartitorii marcați prind și ITV-urile: laturile
                // lor sunt unitatea marcată (predator = primitor).
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                    pj.Adauga(doc);
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajItv)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataItv(os);

                Cont ContItv(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
                var cont4423 = ContItv("4423");
                var cont4424 = ContItv("4424");
                var cont4426 = ContItv("4426");
                var cont4427 = ContItv("4427");
                var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
                var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
                var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");

                var tipItv = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
                var politicaItv = os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocument.Cod == "ITV");
                s.Check("Seed ITV privat: ancoră + numerotare ITV- + conturile închiderii ca DATE (4426/4427/4423/4424), fără reguli de stoc/contare",
                    tipItv != null && tipItv.ClrType == nameof(InchidereTva)
                    && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "ITV")?.Serie == "ITV-"
                    && politicaItv != null
                    && politicaItv.ContDeductibilaId == cont4426.ID && politicaItv.ContColectataId == cont4427.ID
                    && politicaItv.ContDePlataId == cont4423.ID && politicaItv.ContDeRecuperatId == cont4424.ID
                    && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == tipItv.ID)
                    && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == tipItv.ID));

                // Soldurile CUMULATE, exact ca în serviciu (4426 activ, 4427 pasiv).
                decimal SoldDebitorCub(Cont cont, DateOnly la) => CubScena.SoldCont(os, cont.ID, la);
                decimal SoldCreditorCub(Cont cont, DateOnly la) => -SoldDebitorCub(cont, la);

                var finalSep = new DateOnly(2026, 9, 30);
                var finalOct = new DateOnly(2026, 10, 31);
                s.Check("Precondiție: soldurile cumulate 4426/4427 la 30.09.2026 sunt 0 (blocurile anterioare s-au curățat)",
                    SoldDebitorCub(cont4426, finalSep) == 0m && SoldCreditorCub(cont4427, finalSep) == 0m);

                var furnizor = os.CreateObject<Partener>();
                furnizor.Cod = MarcajItv + "-FURN";
                furnizor.Denumire = "Furnizor probă închidere TVA";
                var client = os.CreateObject<Partener>();
                client.Cod = MarcajItv + "-CL";
                client.Denumire = "Client probă închidere TVA";
                var gestiune = os.CreateObject<Gestiune>();
                gestiune.Cod = MarcajItv + "-MAG";
                gestiune.Denumire = "Gestiune probă închidere TVA";
                var unitate = os.CreateObject<UnitateInterna>();
                unitate.Cod = MarcajItv + "-UI";
                unitate.Denumire = "Unitate probă închidere TVA"; // laturile ITV
                os.CommitChanges();

                // --- Septembrie: 4426 = 21 (FCT servicii 100) și 4427 = 42 (FCL 200) ---
                var fctSep = os.CreateObject<FacturaIntrare>();
                fctSep.Numar = "E2E-ITV-FF1";
                fctSep.Data = new DateOnly(2026, 9, 10);
                fctSep.Predator = furnizor;
                fctSep.Primitor = gestiune;
                var linieServiciuSep = os.CreateObject<FacturaIntrareDetaliu>();
                linieServiciuSep.Document = fctSep;
                linieServiciuSep.TipMaterial = tip628;
                linieServiciuSep.Cantitate = 1m;
                linieServiciuSep.PretUnitar = 100m;
                linieServiciuSep.TipTva = n21;

                var fclSep = os.CreateObject<FacturaIesire>();
                fclSep.Data = new DateOnly(2026, 9, 12);
                fclSep.Predator = unitate;
                fclSep.Primitor = client;
                var linieVenitSep = os.CreateObject<FacturaIesireDetaliu>();
                linieVenitSep.Document = fclSep;
                linieVenitSep.TipMaterial = tip704;
                linieVenitSep.Cantitate = 1m;
                linieVenitSep.PretUnitar = 200m;
                linieVenitSep.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctSep);
                MotorOperare.Opereaza(os, fclSep);
                s.Check("Septembrie: TVA-ul lunii în registru — 4426 deductibilă 21, 4427 colectată 42",
                    SoldDebitorCub(cont4426, finalSep) == 21m && SoldCreditorCub(cont4427, finalSep) == 42m);

                var itvSep = InchidereTvaService.Genereaza(os, 2026, 9, unitate.ID);
                s.Check("Generare septembrie: draft ITV la ultima zi a lunii, laturi = unitatea internă, NEautogenerat (document lunar de primă clasă)",
                    itvSep != null && itvSep.Stare == StareDocument.Draft && itvSep.Data == finalSep
                    && itvSep.PredatorId == unitate.ID && itvSep.PrimitorId == unitate.ID && !itvSep.Autogenerat);
                var liniiSep = itvSep.Detalii.OfType<NotaContabilaDetaliu>().ToList();
                s.Check("Generare septembrie: EXACT 2 linii — transferul 4427 = 4426 pe minim (21) și TVA de plată 4427 = 4423 (21); fără rând de recuperat",
                    itvSep.Detalii.Count == 2 && liniiSep.Count == 2
                    && liniiSep.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4426.ID && l.Valoare == 21m)
                    && liniiSep.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4423.ID && l.Valoare == 21m));
                os.CommitChanges();
                s.Check("Idempotență: a doua chemare pe aceeași lună → null (draftul existent ține locul)",
                    InchidereTvaService.Genereaza(os, 2026, 9, unitate.ID) == null);

                MotorOperare.Opereaza(os, itvSep);
                s.Check("Operare septembrie: Operat + număr din politică (ITV-), rândurile contabile = liniile",
                    itvSep.Stare == StareDocument.Operat && itvSep.Numar?.StartsWith("ITV-") == true);
                s.Check("După închidere: 4423 TVA de plată = 21, iar 4426/4427 rămân la 0 pe 30.09",
                    SoldCreditorCub(cont4423, finalSep) == 21m
                    && SoldDebitorCub(cont4426, finalSep) == 0m && SoldCreditorCub(cont4427, finalSep) == 0m);
                // Fix post-review: generarea „în urmă" ar închide a doua oară
                // soldurile deja închise cumulat de o lună ulterioară vie.
                s.CheckRefuza("Generare pentru o lună ANTERIOARĂ unei închideri vii → refuz zgomotos (închiderile sunt cronologice)",
                    () => InchidereTvaService.Genereaza(os, 2026, 8, unitate.ID));

                // --- Octombrie: doar achiziție (4426 = 10,5) → excedent de recuperat ---
                var fctOct = os.CreateObject<FacturaIntrare>();
                fctOct.Numar = "E2E-ITV-FF2";
                fctOct.Data = new DateOnly(2026, 10, 5);
                fctOct.Predator = furnizor;
                fctOct.Primitor = gestiune;
                var linieServiciuOct = os.CreateObject<FacturaIntrareDetaliu>();
                linieServiciuOct.Document = fctOct;
                linieServiciuOct.TipMaterial = tip628;
                linieServiciuOct.Cantitate = 1m;
                linieServiciuOct.PretUnitar = 50m;
                linieServiciuOct.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctOct);

                var itvOct = InchidereTvaService.Genereaza(os, 2026, 10, unitate.ID);
                var liniiOct = itvOct?.Detalii.OfType<NotaContabilaDetaliu>().ToList() ?? [];
                s.Check("Generare octombrie: O SINGURĂ linie — TVA de recuperat 4424 = 4426 (10,5); soldul cumulat dovedește că septembrie s-a închis la 0",
                    itvOct != null && itvOct.Data == finalOct && itvOct.Detalii.Count == 1
                    && liniiOct.Single().ContDebitId == cont4424.ID
                    && liniiOct.Single().ContCreditId == cont4426.ID && liniiOct.Single().Valoare == 10.5m);
                os.CommitChanges();
                MotorOperare.Opereaza(os, itvOct);
                s.Check("Operare octombrie: 4424 TVA de recuperat = 10,5 și 4426 revine la 0",
                    itvOct.Stare == StareDocument.Operat
                    && SoldDebitorCub(cont4424, finalOct) == 10.5m && SoldDebitorCub(cont4426, finalOct) == 0m);

                // --- Storno pe ULTIMA închidere → regenerarea lunii e permisă ---
                // (stornarea septembrie ar strica soldurile lui octombrie deja operat).
                MotorOperare.Storneaza(os, itvOct, finalOct);
                s.Check("Storno octombrie: rânduri inverse la data stornării, soldul de recuperat revine în 4426",
                    itvOct.Stare == StareDocument.Stornat
                    && SoldDebitorCub(cont4424, finalOct) == 0m && SoldDebitorCub(cont4426, finalOct) == 10.5m);
                var itvOctBis = InchidereTvaService.Genereaza(os, 2026, 10, unitate.ID);
                var liniiOctBis = itvOctBis?.Detalii.OfType<NotaContabilaDetaliu>().ToList() ?? [];
                s.Check("Regenerare după storno: draft NOU cu aceeași linie 4424 = 4426 (10,5) — guard-ul de idempotență exclude Stornatul",
                    itvOctBis != null && itvOctBis.ID != itvOct.ID && itvOctBis.Detalii.Count == 1
                    && liniiOctBis.Single().ContDebitId == cont4424.ID && liniiOctBis.Single().Valoare == 10.5m);
                os.CommitChanges();

                // Fix post-review (anti-stale): între generarea draftului și operare
                // mai intră un document de TVA în lună → soldurile nu mai sunt cele
                // din linii, operarea trebuie să REFUZE (altfel luna nu se închide
                // la 0 și regenerarea rămâne blocată de „închiderea vie").
                var fctOct2 = os.CreateObject<FacturaIntrare>();
                fctOct2.Numar = "E2E-ITV-FF3";
                fctOct2.Data = new DateOnly(2026, 10, 20);
                fctOct2.Predator = furnizor;
                fctOct2.Primitor = gestiune;
                var linieOct2 = os.CreateObject<FacturaIntrareDetaliu>();
                linieOct2.Document = fctOct2;
                linieOct2.TipMaterial = tip628;
                linieOct2.Cantitate = 1m;
                linieOct2.PretUnitar = 10m;
                linieOct2.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctOct2);
                s.CheckRefuza("Operarea unui draft STALE (soldurile s-au schimbat de la generare) → refuz, cu îndemnul de regenerare",
                    () => MotorOperare.Opereaza(os, itvOctBis));
                s.Check("Draftul stale nu a lăsat rânduri-fantomă (33d)",
                    CubScena.FaraNote(os, itvOctBis.ID));

                os.Delete(itvOctBis.Detalii.ToList());
                os.Delete(itvOctBis);
                os.CommitChanges();

                CurataItv(os);
                s.Check("Curățenie finală închidere TVA (fără reziduuri e2e; soldurile de TVA revin la 0)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajItv))
                    && !os.GetObjectsQuery<InchidereTva>().Any()
                    && SoldDebitorCub(cont4426, finalOct) == 0m && SoldCreditorCub(cont4427, finalOct) == 0m);
            }
        }
    }
}
