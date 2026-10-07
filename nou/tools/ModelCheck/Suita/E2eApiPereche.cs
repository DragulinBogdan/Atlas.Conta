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

// ============ Felia 8, pasul 3: legătura de pereche prin API (E2E-APER) ======
// F8-D13, partea a doua. Exersează API-ul legăturii (`LaturaPerecheId` cules +
// `Pereche` derivată + endpoint-ul de candidați) PESTE mecanica pasului 1:
// suprimarea generării în AMBELE sensuri (F8-D7), cele șapte refuzuri ale
// validării (F8-D8, inclusiv amendamentul reciprocității), gardianul simetric
// de anulare/storno (F8-D9) și avertismentul CONSULTATIV (F8-D10) — care
// trebuie ȘI să apară când are ce spune, ȘI să tacă altfel: un mesaj care apare
// mereu e zgomot, unul care nu apare niciodată e mort.
static class E2eApiPereche {
    public static void Ruleaza(Suita s) {
        const string MarcajAper = "E2E-APER";

        void CurataAper(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(MarcajAper)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            // Legăturile de pereche se RUP întâi: FK-ul e `Restrict` (F8-D6) și leagă
            // documentele între ele pe o axă pe care ordinea de ștergere n-o declară
            // nimeni (spre deosebire de `DocumentSursa`, unde copiii se cunosc).
            foreach (var t in docs.OfType<DocumentTrezorerie>().Where(t => t.LaturaPerecheId != null)) {
                t.LaturaPereche = null;
                t.LaturaPerecheId = null;
            }
            os.CommitChanges();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Copiii (perechea autogenerată) înaintea părinților — FK-ul DocumentSursa.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAper)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajAper)).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataAper(os);

            var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
            var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");
            var cont581 = os.FirstOrDefault<Cont>(c => c.Simbol == "581");

            ContPropriu ContPropriuAper(string sufix, Cont contImplicit) {
                var cp = os.CreateObject<ContPropriu>();
                cp.Cod = MarcajAper + sufix;
                cp.Denumire = "Cont propriu probă pereche " + sufix;
                cp.ContImplicit = contImplicit;
                return cp;
            }
            var casa = ContPropriuAper("-CASA", cont531);
            var banca = ContPropriuAper("-BANCA", cont770);
            var banca2 = ContPropriuAper("-BANCA2", cont770);
            var partenerAper = os.CreateObject<Partener>();
            partenerAper.Cod = MarcajAper + "-P";
            partenerAper.Denumire = "Partener probă pereche";
            var codEcAper = os.CreateObject<CodEconomic>();
            codEcAper.Cod = MarcajAper + "-CE";
            codEcAper.Denumire = "Cod economic probă pereche";
            os.CommitChanges();

            // Un virament cules: laturi de conturi proprii + linie de natura Virament.
            TrezorerieWriteDto ScrieVir(DateOnly data, Repartitor pred, Repartitor prim,
                decimal valoare, Guid? pereche = null) => new() {
                    Data = data, PredatorId = pred.ID, PrimitorId = prim.ID,
                    TipInstrument = "DispozitieCasa", LaturaPerecheId = pereche,
                    Linii = { new TrezorerieLinieWriteDto {
                        TipMaterialId = tipVir.ID, Valoare = valoare, CodEconomicId = codEcAper.ID } }
                };
            // Martorul „nu e virament": contrapartidă obișnuită + Tipul tehnic TRZ.
            TrezorerieWriteDto ScrieNormal(DateOnly data, Repartitor pred, Repartitor prim,
                decimal valoare, Guid? pereche = null) => new() {
                    Data = data, PredatorId = pred.ID, PrimitorId = prim.ID,
                    LaturaPerecheId = pereche,
                    Linii = { new TrezorerieLinieWriteDto {
                        TipMaterialId = tipTrz.ID, Valoare = valoare, CodEconomicId = codEcAper.ID } }
                };

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (`PregatesteOperare` SCRIE).
            IReadOnlyList<string> DryRunAper(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }
            // Refuzurile de gardian se citesc pe MESAJ, nu doar pe „a aruncat": F8-D9
            // cere ca mesajul SPECIFIC (latura pereche) să ajungă înaintea celui de grup
            // conex, iar ordinea aia e o alegere de cod, nu un accident.
            string MesajRefuz(Action actiune) {
                try { actiune(); return null; }
                catch (OperareException e) { return e.Message; }
            }

            // ── (A) Nonregresia F7 + `Pereche` simetrică + avertismentul ABSENT ──────
            var idA1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 4), casa, banca, 100m));
            decimal Sold581Cub(params Guid[] docIds) => CubScena.SoldCont(os, cont581.ID, null, docIds);
            var rezA1 = ComenziDocument.Sistem(os).Opereaza(idA1);
            var idAc = rezA1.ConexId ?? Guid.Empty;
            s.Check("ANCORA F8-D13.5 (nonregresie F7): perechea AUTOGENERATĂ primește `LaturaPerecheId` = sursa — legătura o scrie motorul pe COPIL, singura parte care e Draft",
                idAc != Guid.Empty && os.GetObjectByKey<Incasare>(idAc).LaturaPerecheId == idA1);
            s.Check("ANCORA F8-D13.4 (ABSENT): la primul virament de pe aceste laturi nu există picioare operate compatibile ⇒ NICIUN avertisment consultativ (rămâne doar informarea de conex)",
                rezA1.Mesaje.Count == 1 && rezA1.Mesaje[0].Contains("documentul conex")
                && !rezA1.Mesaje.Any(m => m.Contains("picioare operate compatibile")));

            var citA1 = TrezorerieApply.Citeste<Plata>(os, idA1);
            var citAc = TrezorerieApply.Citeste<Incasare>(os, idAc);
            var numarA1 = citA1.Numar;
            s.Check("ANCORA F8-D13.6: `Pereche` e SIMETRICĂ — copilul o vede prin linkul PROPRIU, sursa DERIVAT (cine mă arată pe mine); tipul e rezolvat polimorf (CodTip), nu presupus din rută",
                citA1.LaturaPerecheId == null
                && citA1.Pereche != null && citA1.Pereche.Id == idAc && citA1.Pereche.Tip == "INC"
                && citA1.Pereche.Stare == "Draft" && citA1.Pereche.Numar == null
                && citAc.LaturaPerecheId == idA1
                && citAc.Pereche != null && citAc.Pereche.Id == idA1 && citAc.Pereche.Tip == "PLT"
                && citAc.Pereche.Stare == "Operat" && citAc.Pereche.Numar == numarA1
                && numarA1?.StartsWith("PLT-") == true);

            var rezAc = ComenziDocument.Sistem(os).Opereaza(idAc);
            s.Check("ANCORA F8-D13.5 (nonregresie F7): latura pereche operată NU generează un al treilea document (gardul de recursie ține), 581 se închide la 0, ZERO imperecheri",
                rezAc.ConexId == null
                && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idAc)
                && Sold581Cub(idA1, idAc) == 0m
                && !os.GetObjectsQuery<Imperechere>().Any(i =>
                    i.DocumentStingatorId == idA1 || i.DocumentId == idA1
                    || i.DocumentStingatorId == idAc || i.DocumentId == idAc));

            // ── (B) Gardianul F8-D9 pe perechea AUTOGENERATĂ: care mesaj iese ────────
            var mesajTintaAuto = MesajRefuz(() => ComenziDocument.Sistem(os).AnuleazaOperarea(idA1));
            s.Check("ANCORA F8-D13.3: anularea ȚINTEI cu pointer-ul Operat = refuz, cu mesajul SPECIFIC de latură pereche — nu cel de grup conex, deși aici AMBII gardieni s-ar aplica (ordinea e fixată în cod)",
                mesajTintaAuto != null && mesajTintaAuto.Contains("latura pereche")
                && !mesajTintaAuto.Contains("conexe"));
            s.Check("…iar STORNAREA țintei primește exact același refuz (gardianul e pe ambele căi de corecție)",
                MesajRefuz(() => ComenziDocument.Sistem(os).Storneaza(idA1, new DateOnly(2026, 5, 20))) is string m
                && m.Contains("latura pereche"));
            s.Check("ANCORA F8-D13.3: pointer-ul (piciorul care DECLARĂ legătura) se anulează LIBER — el e frunza, nimeni nu depinde de el",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idAc).StareNoua == StareDocument.Draft);
            s.Check("ANCORA F8-D13.3: după anularea pointer-ului, ținta se anulează; aici pointer-ul e ȘI copil autogenerat, deci dispare cu ea (artefact al operării)",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idA1).StareNoua == StareDocument.Draft
                && TrezorerieApply.Citeste<Incasare>(os, idAc) == null
                && TrezorerieApply.Citeste<Plata>(os, idA1) is { Stare: "Draft", Pereche: null });

            // ── (C1) SCENARIUL 64k CANONIC, capăt la capăt ──────────────────────────
            // Fix de criteriu (verificarea main-ului): piciorul de ieșire operat ȘI-A
            // GENERAT un draft care-l arată, iar operatorul îl IGNORĂ (nu-l șterge) și
            // culege manual al doilea picior. Pe criteriul „fără pereche" avertismentul
            // ar fi TĂCUT exact aici — draftul propriu excludea candidatul —, adică
            // jumătatea consultativă a lui F8-D10 era moartă pe scenariul pentru care a
            // fost scrisă. Criteriul corect e „fără pereche OPERATĂ": 581 se închide doar
            // când al doilea picior e operat, iar un draft e o intenție.
            var idAc2 = ComenziDocument.Sistem(os).Opereaza(idA1).ConexId ?? Guid.Empty;
            var etichetaDraftBlocant = $"({TrezorerieApply.Citeste<Incasare>(os, idAc2).Data:dd.MM.yyyy})";
            var idC64 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 5), casa, banca, 100m));
            var rezC64 = ComenziDocument.Sistem(os).Opereaza(idC64);
            s.Check("ANCORA 64k (PREZENT, draftul generat ÎNCĂ EXISTĂ): avertismentul APARE și numește ȘI piciorul candidat, ȘI draftul care blochează legarea — pe criteriul vechi („fără pereche”) aici era TĂCERE, exact pe scenariul canonic",
                rezC64.Mesaje.Any(m => m.Contains("picioare operate compatibile")
                    && m.Contains(numarA1) && m.Contains("blocat de draftul")
                    && m.Contains(etichetaDraftBlocant)));
            s.Check("ANCORA 64k: avertismentul dă ordinea EXECUTABILĂ (ștergeți draftul, apoi alegeți «latura pereche») — F8-D8 chiar refuză legarea cât timp draftul arată spre țintă, iar un sfat care nu se poate executa e mai rău decât tăcerea",
                rezC64.Mesaje.Any(m => m.Contains("ștergeți draftul generat și alegeți «latura pereche»")));
            s.Check("ANCORA 64k: candidatul blocat apare ȘI în endpoint, cu `PerecheDraftNumar` completat — clientul nu oferă o opțiune care pică la operare",
                TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null)
                    .SingleOrDefault(c => c.Id == idA1) is { Stare: "Operat" } blocatDeDraft
                && blocatDeDraft.PerecheDraftNumar == etichetaDraftBlocant);
            s.Check("ANCORA F8-D13.4: avertismentul e CONSULTATIV, nu refuz — documentul e Operat, perechea s-a generat oricum (două viramente identice între aceleași conturi sunt legitime — 64k)",
                rezC64.StareNoua == StareDocument.Operat && rezC64.ConexId != null);
            // Refuzul F8-D8 punctul 5 pe un draft AUTOGENERAT: mesajul își spune cazul.
            var idC64Legat = TrezorerieApply.Aplica<Incasare>(os, null,
                ScrieVir(new DateOnly(2026, 5, 5), casa, banca, 100m, pereche: idA1));
            var eroriDraftAuto = DryRunAper(idC64Legat);
            s.Check("ANCORA F8-D13.2 (punctul 5, pointer AUTOGENERAT): refuzul NU mai spune „e deja perechea altui document” despre un draft pe care tocmai l-a născut sistemul — îl numește și dă remediul",
                eroriDraftAuto.Count == 1
                && eroriDraftAuto[0].Contains("latură pereche GENERATĂ automat")
                && eroriDraftAuto[0].Contains("ștergeți acel draft"));
            TrezorerieApply.Sterge<Incasare>(os, idC64Legat);
            ComenziDocument.Sistem(os).AnuleazaOperarea(idC64);
            TrezorerieApply.Sterge<Incasare>(os, idC64);

            // ── (C2) Același flux DUPĂ ștergerea draftului blocant ──────────────────
            TrezorerieApply.Sterge<Incasare>(os, idAc2);
            s.Check("ANCORA 64k (celălalt capăt): după ștergerea draftului generat, candidatul apare cu `PerecheDraftNumar` NULL — adică „se poate lega direct”",
                TrezorerieApply.Citeste<Plata>(os, idA1) is { Stare: "Operat", Pereche: null }
                && TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null)
                    .SingleOrDefault(c => c.Id == idA1) is { PerecheDraftNumar: null });
            var idC = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 6), casa, banca, 100m));
            var rezC = ComenziDocument.Sistem(os).Opereaza(idC);
            s.Check("ANCORA F8-D13.4 (PREZENT, candidat liber): avertismentul îl numește FĂRĂ mențiunea de draft blocant — textul descrie starea reală, nu un șablon fix",
                rezC.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarA1)
                    && !m.Contains("blocat de draftul")));
            ComenziDocument.Sistem(os).AnuleazaOperarea(idC);
            TrezorerieApply.Sterge<Incasare>(os, idC);

            // ── (D) Legătura CULEASĂ suprimă generarea în AMBELE sensuri ─────────────
            var idD1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m));
            var idD2 = TrezorerieApply.Aplica<Incasare>(os, null,
                ScrieVir(new DateOnly(2026, 5, 9), casa, banca, 200m, pereche: idD1));
            var citD1 = TrezorerieApply.Citeste<Plata>(os, idD1);
            var citD2 = TrezorerieApply.Citeste<Incasare>(os, idD2);
            s.Check("ANCORA F8-D13.6: `LaturaPerecheId` e câmp CULES — `Aplica` îl scrie, iar `Citeste` întoarce `Pereche` COMPLETĂ pe AMBELE picioare (unul prin link propriu, celălalt derivat), pe două DRAFTURI",
                citD2.LaturaPerecheId == idD1 && citD2.Pereche != null && citD2.Pereche.Id == idD1
                && citD2.Pereche.Tip == "PLT" && citD2.Pereche.Stare == "Draft" && citD2.Pereche.Numar == null
                && citD1.LaturaPerecheId == null && citD1.Pereche != null && citD1.Pereche.Id == idD2
                && citD1.Pereche.Tip == "INC" && citD1.Pereche.Stare == "Draft");

            // Amendamentul F8-D8: reciprocitatea, pe chiar perechea de mai sus.
            TrezorerieApply.Aplica<Plata>(os, idD1,
                ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m, pereche: idD2));
            var eroriReciproc = DryRunAper(idD1);
            s.Check("ANCORA F8-D13.2 (amendament): legătura RECIPROCĂ A→B peste B→A = refuz — capcană cu ieșire zero (după operare fiecare l-ar bloca pe celălalt la anulare, iar linkul nu se mai poate șterge: nu mai sunt Draft)",
                eroriReciproc.Count == 1 && eroriReciproc[0].Contains("vă declară DEJA ca latură pereche"));
            TrezorerieApply.Aplica<Plata>(os, idD1, ScrieVir(new DateOnly(2026, 5, 8), casa, banca, 200m));
            s.Check("…legătura se RETRAGE la fel de simplu cum s-a pus (e câmp cules): dry-run curat după golire",
                TrezorerieApply.Citeste<Plata>(os, idD1).LaturaPerecheId == null && DryRunAper(idD1).Count == 0);

            var rezD1 = ComenziDocument.Sistem(os).Opereaza(idD1);
            s.Check("ANCORA F8-D13.1b: la operarea PRIMULUI picior — cel care NU poartă linkul — generarea se suprimă fiindcă CINEVA ÎL ARATĂ PE EL; fără jumătatea a doua a lui F8-D7 s-ar fi născut un al treilea document, adică gaura 64k mutată cu o zi mai devreme",
                rezD1.ConexId == null
                && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idD1)
                && !rezD1.Mesaje.Any(m => m.Contains("picioare operate compatibile")));
            var rezD2 = ComenziDocument.Sistem(os).Opereaza(idD2);
            s.Check("ANCORA F8-D13.1a: piciorul cules CU link nu generează nimic la rândul lui; perechea declarată manual închide 581 la 0 cu EXACT două rânduri și ZERO imperecheri",
                rezD2.ConexId == null
                && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idD2)
                && CubScena.Note(os, idD1, idD2).Count == 4
                && Sold581Cub(idD1, idD2) == 0m
                && !os.GetObjectsQuery<Imperechere>().Any(i =>
                    i.DocumentStingatorId == idD1 || i.DocumentId == idD1
                    || i.DocumentStingatorId == idD2 || i.DocumentId == idD2));

            // ── (E) Criteriul nou nu deschide poarta prea larg + gardianul F8-D9 ────
            // Aici perechea D e OPERATĂ pe ambele picioare: 581 s-a închis, deci niciunul
            // nu mai e candidat — spre deosebire de cel blocat doar de un DRAFT (C1).
            s.Check("ANCORA (limita criteriului nou): piciorul cu pereche OPERATĂ nu e candidat pe NICIUNA dintre rute — „fără pereche OPERATĂ” lărgește lista exact cu drafturile, nu cu perechile deja produse",
                !TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null).Any(c => c.Id == idD1)
                && !TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca.ID, null).Any(c => c.Id == idD2));
            var idE3 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 11), casa, banca, 50m));
            var rezE3 = ComenziDocument.Sistem(os).Opereaza(idE3);
            s.Check("ANCORA (limita criteriului nou): nici avertismentul nu-l pomenește pe piciorul cu pereche OPERATĂ, deși îl pomenește pe cel liber — proba NU e vacuă (mesajul chiar apare)",
                rezE3.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarA1))
                && !rezE3.Mesaje.Any(m => m.Contains(TrezorerieApply.Citeste<Plata>(os, idD1).Numar)));
            ComenziDocument.Sistem(os).AnuleazaOperarea(idE3);
            TrezorerieApply.Sterge<Incasare>(os, idE3);

            // Affordance ONESTĂ pe legătura manuală (afordanța găsită cu criteriul greșit):
            // gardianul F8-D9 refuză anularea țintei, dar `PoateAnula` nu-l oglindea —
            // grupul conex acoperă doar perechea AUTOGENERATĂ (care e și copil).
            s.Check("ANCORA (affordance onestă, F3-D2): `PoateAnula/PoateStorna` = FALSE pe ținta unei legături MANUALE cu pointer-ul Operat — până acum spuneau „da” despre un document pe care motorul îl refuză (grupul conex nu vede legătura declarată)",
                TrezorerieApply.Citeste<Plata>(os, idD1) is { Stare: "Operat", PoateAnula: false, PoateStorna: false }
                && TrezorerieApply.Citeste<Incasare>(os, idD2) is { PoateAnula: true, PoateStorna: true });

            // ── Gardianul F8-D9 pe legătura MANUALĂ (fără nicio relație de grup) ────
            var mesajTintaManuala = MesajRefuz(() => ComenziDocument.Sistem(os).AnuleazaOperarea(idD1));
            s.Check("ANCORA F8-D13.3: ținta unei legături DECLARATE MANUAL n-are `DocumentSursa`, deci gardianul de grup conex n-are ce apăra — o apără exclusiv cel nou (F8-D9), altfel ținta s-ar re-opera și ar genera o pereche lângă cea deja operată",
                mesajTintaManuala != null && mesajTintaManuala.Contains("latura pereche")
                && !mesajTintaManuala.Contains("conexe"));
            ComenziDocument.Sistem(os).AnuleazaOperarea(idD2);
            s.Check("…pointer-ul se anulează liber, iar DUPĂ el se anulează și ținta; legătura CULEASĂ supraviețuiește anulării (e a operatorului, nu artefact al operării — spre deosebire de draftul autogenerat)",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idD1).StareNoua == StareDocument.Draft
                && TrezorerieApply.Citeste<Incasare>(os, idD2) is { Stare: "Draft" } dupaAnulare
                && dupaAnulare.LaturaPerecheId == idD1);

            // ── (F) Cele șapte refuzuri ale validării legăturii (F8-D8), separat ─────
            var seriePltInainte = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar;
            var dataF = new DateOnly(2026, 5, 12);

            // Ținte-fixtură, fiecare pentru exact un refuz.
            var idTintaVir = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca, 10m));
            var idTintaNormala = TrezorerieApply.Aplica<Incasare>(os, null, ScrieNormal(dataF, partenerAper, casa, 10m));
            var idTintaAlteLaturi = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca2, 10m));
            var idTintaAratata = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(dataF, casa, banca, 10m));
            var idPointerulEi = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAratata));

            var idFSelf = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(dataF, casa, banca, 10m));
            s.Check("ANCORA (fix review): `Aplica` refuză self-linkul PE LOC, cu mesajul MOTORULUI — apply-ul cunoaște `id`, iar acceptarea lui fabrica un draft pe care operatorul nu-l putea nici opera, nici șterge",
                MesajRefuz(() => TrezorerieApply.Aplica<Plata>(os, idFSelf,
                    ScrieVir(dataF, casa, banca, 10m, pereche: idFSelf))) is string mSelfApply
                && mSelfApply.Contains("nu poate fi documentul însuși"));
            // Regula rămâne a MOTORULUI, nu a apply-ului: celelalte căi (UI-ul XAF, un
            // import) scriu câmpul direct, deci proba ei se face tot direct — altfel,
            // odată cu refuzul de la graniță, s-ar fi pierdut proba regulii de fond.
            var docSelf = os.GetObjectByKey<Plata>(idFSelf);
            docSelf.LaturaPereche = docSelf;
            os.CommitChanges();
            var eroriSelf = DryRunAper(idFSelf);
            s.Check("ANCORA F8-D13.2 (1/7 self-link): documentul care se arată PE SINE = refuz — și-ar suprima propria generare, apoi s-ar bloca singur la anulare (gardianul s-ar vedea pe el însuși)",
                eroriSelf.Count == 1 && eroriSelf[0].Contains("nu poate fi documentul însuși"));

            var idFNuEVirament = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieNormal(dataF, casa, partenerAper, 10m, pereche: idTintaVir));
            var eroriNuEVirament = DryRunAper(idFNuEVirament);
            s.Check("ANCORA F8-D13.2 (2/7 declarantul nu e virament): o plată obișnuită cu link = refuz — n-ar suprima nimic (nu generează oricum), dar ar BLOCA la anulare un document nevinovat prin gardianul F8-D9",
                eroriNuEVirament.Any(e => e.Contains("există doar la viramentul intern")));

            var idFTintaNuEVirament = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idTintaNormala));
            var eroriTintaNuEVirament = DryRunAper(idFTintaNuEVirament);
            s.Check("ANCORA F8-D13.2 (3/7 ținta nu e virament): predicatul e UNUL singur (`EsteVirament`) și se aplică în ambele capete ale legăturii",
                eroriTintaNuEVirament.Any(e => e.Contains("nu e un virament intern")));

            var idFTipGresit = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idA1));
            var eroriTipGresit = DryRunAper(idFTipGresit);
            s.Check("ANCORA F8-D13.2 (4/7 tip neopus): două PLĂȚI „pereche” = refuz prin CONTRACT (`TipLaturaPereche`, fără `is` pe tip în bază) — altfel ieșirea s-ar posta de două ori și 581 n-ar mai reveni la zero",
                eroriTipGresit.Count == 1 && eroriTipGresit[0].Contains("tipul opus"));

            var idFAlteLaturi = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAlteLaturi));
            var eroriAlteLaturi = DryRunAper(idFAlteLaturi);
            s.Check("ANCORA F8-D13.2 (5/7 alte laturi): picioarele stau pe ACELEAȘI conturi (F7-D1 — direcția o poartă tipul); altfel tranzitul 581 ar rămâne deschis pe amândouă",
                eroriAlteLaturi.Count == 1 && eroriAlteLaturi[0].Contains("alte laturi"));

            var idFTintaLegata = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idD2));
            var eroriTintaLegata = DryRunAper(idFTintaLegata);
            s.Check("ANCORA F8-D13.2 (6/7 ținta are DEJA link propriu spre un al treilea): un virament are exact două picioare",
                eroriTintaLegata.Count == 1 && eroriTintaLegata[0].Contains("deja declarat perechea altui document"));

            var idFTintaAratata = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(dataF, casa, banca, 10m, pereche: idTintaAratata));
            var eroriTintaAratata = DryRunAper(idFTintaAratata);
            s.Check("ANCORA F8-D13.2 (7/7 ținta e ARĂTATĂ de altcineva): cealaltă jumătate a simetriei — două picioare de intrare declarate pereche ale aceleiași ieșiri s-ar opera amândouă și ar dubla postarea, tăcut",
                eroriTintaAratata.Count == 1 && eroriTintaAratata[0].Contains("deja arătat ca pereche de alt document"));

            var idsRefuzAper = new List<Guid> {
                idFSelf, idFNuEVirament, idFTintaNuEVirament, idFTipGresit,
                idFAlteLaturi, idFTintaLegata, idFTintaAratata
            };
            s.Check("ANCORA F8-D13.2: niciunul dintre cele șapte refuzuri n-a lăsat rânduri-fantomă și n-a consumat serie (dry-run = calculează+validează, fără materializare — 33d)",
                idsRefuzAper.All(i => TrezorerieApply.Citeste<Plata>(os, i).Numar == null)
                && idsRefuzAper.All(i => CubScena.FaraNote(os, i))
                && idsRefuzAper.All(i => CubScena.FaraStoc(os, i))
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar == seriePltInainte);

            // Ștergerea unui picior ARĂTAT de altul: refuz de DOMENIU, nu FK Restrict brut.
            var mesajStergere = MesajRefuz(() => TrezorerieApply.Sterge<Incasare>(os, idTintaAratata));
            s.Check("ANCORA F8-D13.6: ștergerea unui picior pe care ALTUL îl declară pereche = refuz de DOMENIU, cu remediul — fără pre-check ar fi ieșit ca violare de constraint (FK Restrict, tradusă generic — 60a): adevărată, dar fără ieșire",
                mesajStergere != null && mesajStergere.Contains("latura pereche")
                && mesajStergere.Contains("ștergeți întâi acea legătură"));

            // Cazul degenerat scos de probă: self-link-ul se arată PE SINE, deci pre-checkul
            // de mai sus îl prinde și pe el — iar FK-ul self-referențial cu `Restrict` e
            // verificat imediat, deci nici baza n-ar lăsa rândul să plece cu tot cu propria
            // referință. Fără mesaj propriu, operatorul rămâne cu un draft ne-operabil (F8-D8)
            // și neștergibil, informat că e „perechea lui însuși".
            var mesajSelf = MesajRefuz(() => TrezorerieApply.Sterge<Plata>(os, idFSelf));
            s.Check("ANCORA F8-D13.6 (caz degenerat): draftul care se arată PE SINE nu se poate nici opera (F8-D8), nici șterge (pre-check-ul de legătură se vede pe el însuși) — refuzul îi spune EXACT ieșirea: goliți câmpul, apoi ștergeți",
                mesajSelf != null && mesajSelf.Contains("se declară PE SINE")
                && mesajSelf.Contains("goliți întâi"));
            TrezorerieApply.Aplica<Plata>(os, idFSelf, ScrieVir(dataF, casa, banca, 10m));

            foreach (var idRefuz in idsRefuzAper)
                TrezorerieApply.Sterge<Plata>(os, idRefuz);
            TrezorerieApply.Sterge<Plata>(os, idPointerulEi);
            s.Check("…iar după ștergerea legăturilor care-l arătau, piciorul se șterge normal (refuzul era al legăturii, nu al documentului)",
                MesajRefuz(() => TrezorerieApply.Sterge<Incasare>(os, idTintaAratata)) == null
                && TrezorerieApply.Citeste<Incasare>(os, idTintaAratata) == null);
            TrezorerieApply.Sterge<Incasare>(os, idTintaVir);
            TrezorerieApply.Sterge<Incasare>(os, idTintaNormala);
            TrezorerieApply.Sterge<Incasare>(os, idTintaAlteLaturi);

            // ── (G) Endpoint-ul de candidați ────────────────────────────────────────
            // Starea la acest punct, pe laturile casa→banca: idA1 = PLT Operat FĂRĂ
            // pereche (candidat), idD1 = PLT Draft ARĂTAT de idD2 (nu), idD2 = INC Draft
            // cu link propriu (nu). Se adaugă un INC liber și o plată obișnuită.
            var idGLiber = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 14), casa, banca, 300m));
            var idGNormala = TrezorerieApply.Aplica<Plata>(os, null, ScrieNormal(new DateOnly(2026, 5, 14), casa, partenerAper, 55m));

            var candidatiPtPlata = TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca.ID, null);
            var candidatiPtIncasare = TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, null);
            s.Check("ANCORA F8-D13.6: ruta PLT oferă picioare de tipul OPUS (INC) — inclusiv DRAFT-uri, fiindcă validarea legăturii nu cere stare —, cu `Total` și `Stare` calculate pe SERVER",
                candidatiPtPlata.Count == 1 && candidatiPtPlata[0].Id == idGLiber
                && candidatiPtPlata[0].Stare == "Draft" && candidatiPtPlata[0].Total == 300m
                && candidatiPtPlata[0].Data == new DateOnly(2026, 5, 14));
            s.Check("ANCORA F8-D13.6: ruta INC oferă piciorul de PLATĂ operat și liber (numărul și totalul lui, `PerecheDraftNumar` null)",
                candidatiPtIncasare.SingleOrDefault(c => c.Id == idA1) is { Stare: "Operat", Total: 100m, PerecheDraftNumar: null } liber
                && liber.Numar == numarA1);
            s.Check("ANCORA F8-D13.6: linkul PROPRIU rămâne excludere ABSOLUTĂ (idD2 nu se oferă — remediul e al legăturii lui, nu al nostru), dar cel ARĂTAT doar de un DRAFT e DIVULGAT, nu ascuns: apare cu draftul blocant numit, ca operatorul să știe ordinea",
                !candidatiPtPlata.Any(c => c.Id == idD2)
                && candidatiPtIncasare.SingleOrDefault(c => c.Id == idD1) is { Stare: "Draft" } blocat
                && blocat.PerecheDraftNumar == TrezorerieApply.Citeste<Incasare>(os, idD2).Numar);
            s.Check("ANCORA F8-D13.6: `exclusId` scoate documentul curent din propria listă (formularul întreabă înainte de a fi salvat, deci se poate întreba și despre sine)",
                !TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, banca.ID, idA1).Any(c => c.Id == idA1));
            s.Check("ANCORA F8-D13.6: laturile sunt filtrul PRIMAR, iar predicatul de virament NU se presupune — pe alte conturi lista e goală, iar o pereche de laturi ne-virament (casa→partener) nu oferă nimic deși EXISTĂ un document pe ea",
                TrezorerieApply.CandidatiPereche<Plata, Incasare>(os, casa.ID, banca2.ID, null).Count == 0
                && TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casa.ID, partenerAper.ID, null).Count == 0
                && TrezorerieApply.Citeste<Plata>(os, idGNormala) is { EsteVirament: false });
            var cablareGresita = false;
            try { TrezorerieApply.CandidatiPereche<Plata, Plata>(os, casa.ID, banca.ID, null); }
            catch (InvalidOperationException) { cablareGresita = true; }
            s.Check("ANCORA F8-D13.6: cusătura „tip opus” e VERIFICATĂ contra contractului domeniului (`TipLaturaPereche`) — o rută cablată greșit pică zgomotos, nu întoarce tăcut candidați de tipul greșit",
                cablareGresita);

            TrezorerieApply.Sterge<Incasare>(os, idGLiber);
            TrezorerieApply.Sterge<Plata>(os, idGNormala);

            // ── (H) Perechea STORNATĂ nu ține (fix review D1) ────────────────────────
            // Un picior stornat are registrele INVERSATE: 581 e redeschis, perechea NU
            // s-a produs. Felia avea DOUĂ criterii pentru aceeași întrebare — gardianul
            // de anulare/storno judeca `== Operat` (corect), iar citirea perechii și
            // filtrele de candidați numărau orice pointer ne-Draft (greșit). Consecința
            // pornea de la storno, unealta NORMALĂ de corecție când perioada e închisă
            // (decizia 14): piciorul rămas descoperit dispărea din listă și din
            // avertisment (gaura 64k, tăcută), iar legarea manuală era refuzată cu un
            // remediu IMPOSIBIL — „ștergeți acea legătură", pe un document stornat, care
            // nu se mai editează și nu se mai șterge.
            //
            // Laturi PROPRII secțiunii: listele de candidați se citesc pe (predator,
            // primitor), deci izolarea e condiția ca probele să fie deterministe.
            var casaH = ContPropriuAper("-CASA-H", cont531);
            var bancaH = ContPropriuAper("-BANCA-H", cont770);
            var casaI = ContPropriuAper("-CASA-I", cont531);
            var bancaI = ContPropriuAper("-BANCA-I", cont770);
            os.CommitChanges();

            var idH1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 16), casaH, bancaH, 400m));
            var idHc = ComenziDocument.Sistem(os).Opereaza(idH1).ConexId ?? Guid.Empty;
            ComenziDocument.Sistem(os).Opereaza(idHc);
            ComenziDocument.Sistem(os).Storneaza(idHc, new DateOnly(2026, 5, 17));
            var numarH1 = TrezorerieApply.Citeste<Plata>(os, idH1).Numar;

            s.Check("ANCORA D1 (descriptiv vs decizional): pe sursa cu perechea STORNATĂ, `Pereche` o ARATĂ în continuare (o poți deschide), dar `PerecheActiva` = FALSE — 581 e din nou deschis, iar clientul ramifică pe boolean, nu pe stare",
                TrezorerieApply.Citeste<Plata>(os, idH1) is { Stare: "Operat", PerecheActiva: false } citH1
                && citH1.Pereche != null && citH1.Pereche.Id == idHc && citH1.Pereche.Stare == "Stornat");
            s.Check("ANCORA D1-A: sursa REDEVINE candidat pe endpoint după stornarea perechii (înainte, pointer-ul „nu e Draft” o scotea definitiv) — și fără mențiune de draft blocant, fiindcă nu există",
                TrezorerieApply.CandidatiPereche<Incasare, Plata>(os, casaH.ID, bancaH.ID, null)
                    .SingleOrDefault(c => c.Id == idH1) is { Stare: "Operat", PerecheDraftNumar: null });

            // Legarea manuală de o țintă al cărei POINTER e stornat: PERMISĂ (remediul
            // imposibil a dispărut).
            var idH3 = TrezorerieApply.Aplica<Incasare>(os, null,
                ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idH1));
            s.Check("ANCORA D1: legarea manuală de o țintă arătată doar de un pointer STORNAT trece dry-run-ul — altfel refuzul era o fundătură (pointer-ul stornat nu se mai poate nici edita, nici șterge)",
                DryRunAper(idH3).Count == 0);
            // Aceeași regulă pe cealaltă jumătate a punctului 5: ținta are LINK PROPRIU,
            // dar spre un document stornat.
            var idHLegatDeStornat = TrezorerieApply.Aplica<Plata>(os, null,
                ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idHc));
            var idHSpreEa = TrezorerieApply.Aplica<Incasare>(os, null,
                ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m, pereche: idHLegatDeStornat));
            s.Check("ANCORA D1: nici linkul PROPRIU al țintei nu mai blochează dacă arată spre un document STORNAT — o singură noțiune („capătul stornat nu contează”), aplicată în ambele jumătăți ale punctului 5",
                DryRunAper(idHSpreEa).Count == 0);
            TrezorerieApply.Sterge<Incasare>(os, idHSpreEa);
            TrezorerieApply.Sterge<Plata>(os, idHLegatDeStornat);
            TrezorerieApply.Sterge<Incasare>(os, idH3);

            // Avertismentul: piciorul descoperit reintră în listă.
            var idH2 = TrezorerieApply.Aplica<Incasare>(os, null, ScrieVir(new DateOnly(2026, 5, 18), casaH, bancaH, 400m));
            var rezH2 = ComenziDocument.Sistem(os).Opereaza(idH2);
            s.Check("ANCORA D1-A: un nou picior cules manual îl NUMEȘTE pe cel rămas descoperit în avertismentul consultativ — pe criteriul vechi era tăcere, deci gaura 64k se redeschidea tăcut după orice storno",
                rezH2.Mesaje.Any(m => m.Contains("picioare operate compatibile") && m.Contains(numarH1)));
            ComenziDocument.Sistem(os).AnuleazaOperarea(idH2);
            var idH2Conex = os.GetObjectsQuery<DocumentTrezorerie>()
                .Where(x => x.DocumentSursaId == idH2).Select(x => (Guid?)x.ID).FirstOrDefault();
            s.Check("…iar anularea lui i-a șters draftul autogenerat (artefact al operării) — starea rămâne curată pentru proba următoare",
                idH2Conex == null);
            TrezorerieApply.Sterge<Incasare>(os, idH2);

            // D1-B: anulare + re-operare REGENEREAZĂ perechea.
            s.Check("ANCORA D1-B (gardianul rămâne pe Operat): ținta cu pointer STORNAT se anulează LIBER — gardianul apără registre, iar registrele pointer-ului sunt deja inversate",
                ComenziDocument.Sistem(os).AnuleazaOperarea(idH1).StareNoua == StareDocument.Draft
                && os.GetObjectByKey<Incasare>(idHc) is { Stare: StareDocument.Stornat });
            var rezH1Reoperat = ComenziDocument.Sistem(os).Opereaza(idH1);
            var idHc2 = rezH1Reoperat.ConexId ?? Guid.Empty;
            s.Check("ANCORA D1-B: re-operarea sursei REGENEREAZĂ latura pereche (suprimarea se uită la perechea ACTIVĂ, nu la orice pointer) — altfel indiciul din client prescria exact o operațiune care nu făcea nimic",
                idHc2 != Guid.Empty && os.GetObjectByKey<Incasare>(idHc2).LaturaPerecheId == idH1);
            ComenziDocument.Sistem(os).Opereaza(idHc2);
            s.Check("ANCORA D1-B: perechea regenerată se operează (pointer-ul stornat nu mai blochează validarea) și 581 se închide la 0 peste TOATE cele trei documente — stornarea a golit contribuția piciorului anulat",
                TrezorerieApply.Citeste<Incasare>(os, idHc2) is { Stare: "Operat" }
                && Sold581Cub(idH1, idHc, idHc2) == 0m
                && TrezorerieApply.Citeste<Plata>(os, idH1) is { PerecheActiva: true } dupaRegenCub
                && dupaRegenCub.Pereche.Id == idHc2);

            // ── (I) D2: linkul golit pe copilul autogenerat ─────────────────────────
            // `LaturaPerecheId` e câmp CULES, deci pe draftul autogenerat operatorul îl
            // poate goli cu un click. Datele rămân corecte (gardul `Autogenerat` ține),
            // dar o citire numai pe link ar declara „latura pereche lipsește, 581 rămâne
            // deschis" despre un document care o ARE — iar sfatul „culegeți manual
            // piciorul celălalt" ar produce chiar dubla postare pe care felia o închide.
            var idI1 = TrezorerieApply.Aplica<Plata>(os, null, ScrieVir(new DateOnly(2026, 5, 19), casaI, bancaI, 700m));
            var idIc = ComenziDocument.Sistem(os).Opereaza(idI1).ConexId ?? Guid.Empty;
            s.Check("ANCORA D1 (PerecheActiva pe DRAFT): perechea abia generată e Draft — intenția celui de-al doilea picior — deci ACTIVĂ; `Stornat` e singura stare care nu contează",
                TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } inainteDeGolire
                && inainteDeGolire.Pereche.Stare == "Draft");
            TrezorerieApply.Aplica<Incasare>(os, idIc, ScrieVir(new DateOnly(2026, 5, 19), casaI, bancaI, 700m));
            s.Check("ANCORA D2: după golirea linkului pe copilul autogenerat, SURSA își vede perechea prin grupul conex (`DocumentSursaId` + `Autogenerat`) și o raportează ACTIVĂ — panoul nu mai poate spune „581 rămâne deschis” despre un document care are perechea",
                os.GetObjectByKey<Incasare>(idIc).LaturaPerecheId == null
                && TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } dupaGolire
                && dupaGolire.Pereche != null && dupaGolire.Pereche.Id == idIc);
            s.Check("ANCORA D2 (oglinda, adăugire peste literă — vezi raportul): și COPILUL își vede sursa, altfel aceeași minciună se citea de pe celălalt ecran — iar acolo sfatul „re-operați” e inert prin construcție (`Autogenerat`)",
                TrezorerieApply.Citeste<Incasare>(os, idIc) is { PerecheActiva: true } citCopil
                && citCopil.Pereche != null && citCopil.Pereche.Id == idI1);
            bool aGeneratDinNou;
            using (var osProba = s.Provider.CreateObjectSpace()) {
                // ObjectSpace ARUNCAT, necomis: hook-ul CREEAZĂ documentul dacă decide
                // să genereze, iar proba e chiar despre absența lui.
                aGeneratDinNou = osProba.GetObjectByKey<Plata>(idI1).GenereazaSecundar(osProba) != null;
            }
            s.Check("ANCORA D2: suprimarea generării ține și cu linkul golit — sursa nu naște un al doilea copil (gardul de grup conex, nu doar cel de link)",
                !aGeneratDinNou);
            ComenziDocument.Sistem(os).Opereaza(idIc);
            s.Check("ANCORA D1 (PerecheActiva pe OPERAT): perechea operată e activă, 581 se închide la 0 — golirea linkului a rămas o chestiune de AFIȘARE, datele n-au fost niciodată în pericol",
                TrezorerieApply.Citeste<Plata>(os, idI1) is { PerecheActiva: true } dupaOperareCub
                && dupaOperareCub.Pereche.Stare == "Operat"
                && Sold581Cub(idI1, idIc) == 0m);

            CurataAper(os);
            s.Check("Curățenie finală felia pereche prin API (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAper))
                && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajAper))
                && os.GetObjectByKey<Plata>(idA1) == null
                && os.GetObjectByKey<Incasare>(idD2) == null);
        }
    }
}
