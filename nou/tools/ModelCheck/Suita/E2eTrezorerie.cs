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

// ============== Scenariul e2e 3c: Plata/Incasare + Imperechere ==============
// Trezoreria (decizia 31): FCT cu grupul DECONT_* → draft Plata autogenerat cu
// liniile-defalcare → operare (contare per latură din ContImplicit pe
// Repartitor: 401 = 770) + imperecherea automată → gardianul de imperecheri la
// anulare/storno → încasare manuală (411 fallback → casă) + imperechere
// manuală cu invarianții (stare, contrapartidă, rest) → avans către angajat
// (542 din ContImplicit) → storno după ștergerea stingerii.
static class E2eTrezorerie {
    public static void Ruleaza(Suita s) {
        const string MarcajTrz = "E2E-TRZ";

        void CurataTrz(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajTrz)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajTrz).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajTrz).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajTrz)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajTrz + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataTrz(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "751.01.00");
            var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
            var trezoreria = os.FirstOrDefault<ContPropriu>(c => c.Cod == "TREZ");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
            var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
            var cont542 = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
            var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");

            // Politicile trezoreriei din seed (decizia 31).
            s.Check("Seed: conturi proprii CASA→531.01.01, TREZ→770.00.00 (bancă)",
                casa?.ContImplicitId == cont531.ID && trezoreria?.ContImplicitId == cont770.ID && trezoreria.EsteBanca);
            s.Check("Seed: Tipul tehnic TRZ (defalcare) fără cont implicit",
                tipTrz != null && tipTrz.ContImplicitId == null && tipTrz.Clasa.Natura == NaturaClasa.Tehnica);
            // Rândul GENERIC al tipului (fără TipMaterial, fără filtru de natură) — de la
            // F7-D6 fiecare tip de trezorerie are DOUĂ rânduri: genericul de mai jos și
            // cel per TipMaterial=VIR (viramentul intern). Căutarea „prima regulă a
            // tipului" ar fi devenit nedeterministă; proba rămâne despre generic.
            var regulaPlt = os.FirstOrDefault<RegulaContare>(
                r => r.TipDocument.Cod == "PLT" && r.TipMaterialId == null);
            s.Check("Seed PLT: debit RepartitorPrimitor (fallback 401), credit RepartitorPredator fără fallback",
                regulaPlt != null && regulaPlt.SursaContDebit == SursaCont.RepartitorPrimitor
                && regulaPlt.ContDebitId == cont401.ID
                && regulaPlt.SursaContCredit == SursaCont.RepartitorPredator && regulaPlt.ContCreditId == null);
            var regulaInc = os.FirstOrDefault<RegulaContare>(
                r => r.TipDocument.Cod == "INC" && r.TipMaterialId == null);
            s.Check("Seed INC: debit RepartitorPrimitor fără fallback, credit RepartitorPredator (fallback 411)",
                regulaInc != null && regulaInc.SursaContDebit == SursaCont.RepartitorPrimitor
                && regulaInc.ContDebitId == null && regulaInc.ContCreditId == cont411.ID);
            s.Check("Seed: fără reguli de stoc pe PLT/INC (pur contabile)",
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "PLT" || r.TipDocument.Cod == "INC"));

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajTrz + "-FURN";
            furnizor.Denumire = "Furnizor probă trezorerie";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajTrz + "-CL";
            client.Denumire = "Client probă trezorerie";
            var angajat = os.CreateObject<Angajat>();
            angajat.Cod = MarcajTrz + "-ANG";
            angajat.Denumire = "Angajat probă trezorerie";
            angajat.ContImplicit = cont542; // avansurile de trezorerie ale titularului
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajTrz + "-CE";
            codEc.Denumire = "Cod economic probă trezorerie";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajTrz;
            produs.Denumire = "Produs probă trezorerie";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            os.CommitChanges();

            // --- FCT cu plata automată (00 §7): DECONT_* → draft Plata + defalcare ---
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = "E2E-TF1";
            fct.Data = new DateOnly(2026, 3, 3);
            fct.Predator = furnizor;
            fct.Primitor = mag1;
            fct.GenereazaPlata = true;
            var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
            linieStoc.Document = fct;
            linieStoc.TipMaterial = tipMateriale;
            linieStoc.Cantitate = 5m;
            linieStoc.PretUnitar = 10m;
            linieStoc.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");
            linieStoc.CodEconomicId = codEc.ID;
            linieStoc.CreeazaLot(os, produs, mag1);
            var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
            linieServiciu.Document = fct;
            linieServiciu.TipMaterial = tipServicii;
            linieServiciu.Cantitate = 1m;
            linieServiciu.PretUnitar = 100m;
            linieServiciu.CodEconomicId = codEc.ID;
            os.CommitChanges();

            s.CheckRefuza("GenereazaPlata fără cont propriu cules → refuz", () => MotorOperare.Opereaza(os, fct));
            fct.PlataContPropriu = trezoreria;
            fct.PlataNumar = "OP-77";
            fct.PlataData = new DateOnly(2026, 3, 4);
            fct.PlataTipInstrument = TipInstrumentPlata.Cec;
            os.CommitChanges();

            var conex = MotorOperare.Opereaza(os, fct);
            s.Check("Operarea FCT întoarce conexul NIR; plata e al doilea copil autogenerat", conex is NIR);
            var plataAuto = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fct.ID);
            s.Check("Plata draft: header din grupul DECONT_* (TREZ → furnizor, OP-77, CEC, data plății)",
                plataAuto.Stare == StareDocument.Draft && plataAuto.Autogenerat
                && plataAuto.PredatorId == trezoreria.ID && plataAuto.PrimitorId == furnizor.ID
                && plataAuto.Numar == "OP-77" && plataAuto.TipInstrument == TipInstrumentPlata.Cec
                && plataAuto.Data == new DateOnly(2026, 3, 4));
            s.Check("Plata draft: liniile clonează defalcarea facturii (2 linii, 159,5, dimensiuni, fără lot)",
                plataAuto.Detalii.Count == 2 && plataAuto.Detalii.Sum(d => d.Valoare) == 159.5m
                && plataAuto.Detalii.All(d => d.DimensiuniCulese().CodEconomicId == codEc.ID && d.LotId == null));

            // --- Operarea plății: contare din laturi + imperecherea automată ---
            s.Check("Plata autogenerată nu generează alt conex", MotorOperare.Opereaza(os, plataAuto) == null);
            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();
            var notePlataCub = NoteCub(plataAuto);
            s.Check("Plata contează per linie de defalcare: 401 = 770 (59,5 + 100)",
                notePlataCub.All(p => p.Debit ? p.Cont == cont401.ID : p.Cont == cont770.ID)
                && notePlataCub.Rulaj(cont401.ID, N.Latura.Debit) == 159.5m
                && notePlataCub.Rulaj(cont770.ID, N.Latura.Credit) == 159.5m);
            s.Check("Plata nu mișcă stoc",
                CubScena.FaraStoc(os, plataAuto.ID));
            var impAuto = os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID);
            s.Check("Imperecherea automată: FCT stinsă integral (159,5, Autogenerat)",
                impAuto.DocumentId == fct.ID && impAuto.Suma == 159.5m && impAuto.Autogenerat
                && ImperechereService.Ramas(os, fct.ID) == 0m && ImperechereService.Ramas(os, plataAuto.ID) == 0m);

            // SC-CIT-62: nominalizarea a fost verificată cu NIR încă Draft.
            // Oracolul dual cere și recepția: numai în registre ea aparține NIR-ului.
            MotorOperare.Opereaza(os, conex);
            // --- NUC-PLT-FCT (B-D5, pas 4): plata NASCUTA din factura, cu stingerea automata ---
            ProbeNucleu.Proba(os, s.Check, "NUC-PLT-FCT", [plataAuto]);

            // --- Gardianul de imperecheri: corecția cere întâi ștergerea stingerii ---
            s.CheckRefuza("Anularea plății cu imperechere → refuz", () => MotorOperare.AnuleazaOperarea(os, plataAuto));
            s.CheckRefuza("Stornarea FCT cu plata operată → refuz", () =>
                MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22)));
            ImperechereService.Sterge(os, impAuto.ID);
            MotorOperare.AnuleazaOperarea(os, plataAuto);
            s.Check("După ștergerea imperecherii, anularea plății merge (Draft, notele șterse)",
                plataAuto.Stare == StareDocument.Draft && CubScena.FaraNote(os, plataAuto.ID));
            MotorOperare.Opereaza(os, plataAuto);
            s.Check("Re-operarea plății re-creează imperecherea automată",
                os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID).Suma == 159.5m);

            // --- Încasare manuală + imperechere manuală cu FCL (invarianții stingerii) ---
            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = new DateOnly(2026, 3, 6);
            fcl.Predator = sediu;
            fcl.Primitor = client;
            var linieVenit = os.CreateObject<FacturaIesireDetaliu>();
            linieVenit.Document = fcl;
            linieVenit.TipMaterial = tipVenit;
            linieVenit.Cantitate = 1m;
            linieVenit.PretUnitar = 119m;
            linieVenit.CodEconomicId = codEc.ID; // 751 cere E (3d)
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl);

            var inc = os.CreateObject<Incasare>();
            inc.Data = new DateOnly(2026, 3, 10);
            inc.Predator = casa; // intenționat greșit — plătitorul nu poate fi cont propriu
            inc.Primitor = casa;
            inc.TipInstrument = TipInstrumentPlata.Chitanta;
            var linieInc = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieInc.Document = inc;
            linieInc.TipMaterial = tipTrz;
            s.CheckRefuza("Laturi greșite + linie fără valoare → refuz", () => MotorOperare.Opereaza(os, inc));
            inc.Predator = client;
            linieInc.Valoare = 119m;
            os.CommitChanges();
            // Casa (531) poartă defalcarea E — INC nu are politică de tip, dar contul
            // cere codul economic pe nota rezolvată (3d).
            s.CheckRefuza("Încasare fără cod economic (531 cere E) → refuz", () => MotorOperare.Opereaza(os, inc));
            linieInc.CodEconomicId = codEc.ID;
            os.CommitChanges();
            s.Check("Încasarea nu generează conex", MotorOperare.Opereaza(os, inc) == null);
            s.Check("Încasare operată cu număr din politică", inc.Numar?.StartsWith("INC-") == true);
            var noteIncCub = NoteCub(inc);
            s.Check("Contare încasare: 531.01.01 (casa) = 411.01.01 (fallback client), 119",
                noteIncCub.Count == 2 && noteIncCub.Nota(cont531.ID, cont411.ID, 119m));

            // --- NUC-INC (B-D5, pas 4): incasare manuala, fara sursa; proba sta INAINTEA
            //     imperecherii manuale (nominalizarea prin document e TR-D9, B-D5) ---
            ProbeNucleu.Proba(os, s.Check, "NUC-INC", [inc]);

            var fclDraft = os.CreateObject<FacturaIesire>();
            fclDraft.Data = new DateOnly(2026, 3, 11);
            fclDraft.Predator = sediu;
            fclDraft.Primitor = client;
            s.CheckRefuza("Imperechere cu document neoperat → refuz", () =>
                ImperechereService.Imperecheaza(os, inc, fclDraft, 1m));
            s.CheckRefuza("Imperechere fără contrapartidă comună (încasarea clientului × factura furnizorului) → refuz",
                () => ImperechereService.Imperecheaza(os, inc, fct, 1m));
            s.CheckRefuza("Imperechere peste restul neasignat → refuz", () =>
                ImperechereService.Imperecheaza(os, inc, fcl, 200m));
            var impManual = ImperechereService.Imperecheaza(os, inc, fcl, 119m);
            s.Check("Imperechere manuală: FCL stinsă integral, resturile 0",
                !impManual.Autogenerat && ImperechereService.Ramas(os, fcl.ID) == 0m
                && ImperechereService.Ramas(os, inc.ID) == 0m);
            s.CheckRefuza("A doua stingere pe aceeași încasare (rest 0) → refuz", () =>
                ImperechereService.Imperecheaza(os, inc, fcl, 1m));

            // --- Avansul către angajat: 542 din ContImplicit bate fallback-ul 401 ---
            var avans = os.CreateObject<Plata>();
            avans.Data = new DateOnly(2026, 3, 12);
            avans.Predator = casa;
            avans.Primitor = angajat;
            avans.TipInstrument = TipInstrumentPlata.DispozitieCasa;
            var linieAvans = os.CreateObject<DocumentTrezorerieDetaliu>();
            linieAvans.Document = avans;
            linieAvans.TipMaterial = tipTrz;
            linieAvans.Valoare = 50m;
            os.CommitChanges();
            // 31f închis: obligativitatea clasificației pe liniile de plată = politică.
            s.CheckRefuza("Plată fără clasificație bugetară (politica PLT) → refuz",
                () => MotorOperare.Opereaza(os, avans));
            linieAvans.CodEconomicId = codEc.ID;
            os.CommitChanges();
            MotorOperare.Opereaza(os, avans);
            s.Check("Avans operat cu număr din politică", avans.Numar?.StartsWith("PLT-") == true);
            s.Check("D9-A10 avans: debitul 542 poartă angajatul, creditul casei contul propriu",
                NoteCub(avans) is { Count: 2 } noteAvansCub
                && noteAvansCub.Single(p => p.Debit).Repartitor == angajat.ID
                && noteAvansCub.Single(p => p.Credit).Gestiune == casa.ID);

            var contractAvans = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, avans);
            var postariAvans = contractAvans.Tranzactii.SelectMany(t => t.Postari).ToArray();
            var partidaAvans = N.Unitate.DeschidePartida(cont542.ID, angajat.ID, avans.ID, avans.DataInregistrare);
            s.Check("SC-DEC-16 (096): PLT 50 pe angajat deschide partidă fără rol comercial",
                contractAvans.EsteAcceptat && postariAvans.Length == 2
                && postariAvans.Any(p => p.Coordonate.Cont == cont542.ID && p.Coordonate.Latura == N.Latura.Debit
                    && p.Coordonate.Unitate == partidaAvans && p.Coordonate.Partener == angajat.ID && p.Valoare == 50)
                && postariAvans.Any(p => p.Coordonate.Cont == cont531.ID && p.Coordonate.Latura == N.Latura.Credit
                    && p.Coordonate.Unitate == null && p.Valoare == 50)
                && cont542.RolTert == RolTertCont.Niciunul);
            s.Check("SC-DEC-16: conservare și determinism pentru avans",
                !contractAvans.Tranzactii.SelectMany(N.Conservare.Verifica).Any()
                && Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, avans) == contractAvans);

            // --- Storno: refuzat cât există stingerea, curat după ștergerea ei ---
            s.CheckRefuza("Stornarea încasării cu imperechere → refuz", () =>
                MotorOperare.Storneaza(os, inc, new DateOnly(2026, 7, 22)));
            ImperechereService.Sterge(os, impManual.ID);
            MotorOperare.Storneaza(os, inc, new DateOnly(2026, 7, 22));
            var stornoIncCub = CubScena.Note(os, inc.ID).Where(p => p.Storno).ToList();
            s.Check("Storno încasare → nota inversată append-only (−119) la data stornării",
                inc.Stare == StareDocument.Stornat && stornoIncCub.Count == 2
                && stornoIncCub.Nota(cont531.ID, cont411.ID, -119m));

            // 82: același motor, cu rest parțial/zero și cu linii încă necomise.
            foreach (var idStingere in os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == plataAuto.ID).Select(i => i.ID).ToArray())
                ImperechereService.Sterge(os, idStingere);
            MotorOperare.AnuleazaOperarea(os, plataAuto);

            Plata PlataCuSursa(Document sursa, decimal valoare, bool automata = true) {
                var p = os.CreateObject<Plata>();
                p.Data = new DateOnly(2026, 3, 12);
                p.Predator = trezoreria;
                p.Primitor = furnizor;
                p.Autogenerat = automata;
                p.DocumentSursa = sursa;
                var d = os.CreateObject<DocumentTrezorerieDetaliu>();
                d.Document = p;
                d.TipMaterial = tipTrz;
                d.Valoare = valoare;
                d.CodEconomicId = codEc.ID;
                os.CommitChanges();
                return p;
            }

            var partialaManuala = PlataCuSursa(fct, 60m, automata: false);
            MotorOperare.Opereaza(os, partialaManuala);
            s.Check("82: plata cu sursă, dar neautogenerată, NU stinge automat",
                !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == partialaManuala.ID));
            ImperechereService.Imperecheaza(os, partialaManuala, fct, 60m);

            var partialaAuto = PlataCuSursa(fct, 40m);
            partialaAuto.Detalii.Single().Valoare = 30m; // fără commit intermediar
            MotorOperare.Opereaza(os, partialaAuto);
            s.Check("82: stingerea automată folosește liniile curente (30), nu valoarea persistată (40)",
                os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == partialaAuto.ID).Suma == 30m
                && ImperechereService.Ramas(os, fct.ID) == 69.5m);

            var restAuto = PlataCuSursa(fct, 100m);
            MotorOperare.Opereaza(os, restAuto);
            s.Check("82: plata automată se plafonează la restul sursei (69,5 din 100)",
                os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == restAuto.ID).Suma == 69.5m
                && ImperechereService.Ramas(os, fct.ID) == 0m);

            var faraRest = PlataCuSursa(fct, 20m);
            MotorOperare.Opereaza(os, faraRest);
            s.Check("82: sursa fără rest permite operarea, fără împerechere zero",
                faraRest.Stare == StareDocument.Operat
                && !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == faraRest.ID));

            // Sursa FCL e din nou nestinsă după ștergerea împerecherii încasării.
            // Beneficiarul plății e FURN, sursa e a CL: refuzul vine din Creeaza,
            // DUPĂ materializare. ObjectSpace-ul comenzii se aruncă fără commit.
            var refuzata = PlataCuSursa(fcl, 10m);
            using (var comanda = s.Provider.CreateObjectSpace()) {
                var mesaj = s.Refuz(() => MotorOperare.Opereaza(comanda, comanda.GetObjectByKey<Plata>(refuzata.ID)));
                s.Check("82: împerecherea automată păstrează refuzul de contrapartidă",
                    mesaj?.Contains("aceeași contrapartidă") == true);
            }
            using (var citire = s.Provider.CreateObjectSpace()) {
                var persistat = citire.GetObjectByKey<Plata>(refuzata.ID);
                s.Check("82: refuzul stingerii NU persistă operarea, numărul sau registrele/relația",
                    persistat.Stare == StareDocument.Draft && persistat.DataOperare == null && persistat.Numar == null
                    && CubScena.FaraPostari(citire, refuzata.ID)
                    && !citire.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == refuzata.ID));
            }

            CurataTrz(os);
            s.Check("Curățenie finală trezorerie (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajTrz))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajTrz));
        }
    }
}
