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

// ========================= Scenariul e2e P1: profil privat =========================
// TVA structural pe baza privată (OMFP 1802): FCT cu linie stoc + linie serviciu
// (NIR net, 4426 pe factură — inclusiv pentru linia de stoc fără regulă
// principală, 401 brut, imperecherea plății automate pe brut) → FCL cu 4427 →
// DEC cu 4426 = 542 → taxare inversă (4426 = 4427) → capitalizat (nedeductibil)
// → ValoareTva culeasă manual păstrată → storno cu rânduri TVA inverse.
static class E2eP1Privat {
    public static void Ruleaza(Suita s) {
        const string MarcajPrv = "E2E-PRV";

        void CurataPrv(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajPrv)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajPrv).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajPrv).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajPrv)).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataPrv(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
            var tip302 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
            var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
            var tip345 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "345");
            var tip381 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "381");
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
            var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var ti21 = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
            var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
            Cont ContSimbol(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
            var cont401 = ContSimbol("401");
            var cont4111 = ContSimbol("4111");
            var cont4426 = ContSimbol("4426");
            var cont4427 = ContSimbol("4427");
            var cont542 = ContSimbol("542");
            var cont5121 = ContSimbol("5121");

            // --- Seed-ul profilului privat ---
            s.Check("Seed: N21 = Normal 21%, conturile 4426/4427 (+4428 rezervat) și codurile SAF-T ca date",
                n21 != null && n21.Regim == RegimTva.Normal && n21.Cota == 21m
                && n21.ContTvaDeductibilId == cont4426.ID && n21.ContTvaColectatId == cont4427.ID
                && n21.ContTvaNeexigibilId != null
                && n21.CodSafTLivrare == "310344" && n21.CodSafTAchizitie == "301104");
            s.Check("Seed: derivarea contului implicit pe simboluri OMFP (302 → 302, exact)",
                tip302?.ContImplicitId != null
                && os.GetObjectByKey<Cont>(tip302.ContImplicitId.Value).Simbol == "302");
            Cont DebitBcs(TipMaterial tip) {
                var r = os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "BCS" && x.TipMaterialId == tip.ID);
                return r?.ContDebitId == null ? null : os.GetObjectByKey<Cont>(r.ContDebitId.Value);
            }
            s.Check("Seed BCS: 302→602 (mecanic) + excepțiile profilului 371→607, 345→711, 381→608",
                DebitBcs(tip302)?.Simbol == "602" && DebitBcs(tip371)?.Simbol == "607"
                && DebitBcs(tip345)?.Simbol == "711" && DebitBcs(tip381)?.Simbol == "608");
            var plusLdi = os.FirstOrDefault<RegulaContare>(r => r.TipDocument.Cod == "LDI" && r.TipMaterialId == null);
            s.Check("Seed LDI: plusul de inventar → credit 7588 (nu 791 — decizia 29c)",
                plusLdi != null && plusLdi.SemnFiltru == +1
                && plusLdi.ContCredit?.Simbol == "7588");
            PoliticaTva Tva(string cod) => os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == cod);
            s.Check("Seed: PoliticaTva — FCT/DEC deduc (401/542), FCL colectează (4111); NIR/PLT/INC fără rând; "
                + "declararea faptului întârziat urmează DIRECȚIA (deductibilul în perioada înregistrării — art. 301, "
                + "colectatul în perioada faptului)",
                Tva("FCT")?.Directie == DirectieTva.Deductibil && Tva("FCT").SursaContrapartida == SursaCont.RepartitorPredator
                && Tva("FCT").ContrapartidaFallback?.Simbol == "401"
                && Tva("DEC")?.Directie == DirectieTva.Deductibil && Tva("DEC").ContrapartidaFallback?.Simbol == "542"
                && Tva("FCL")?.Directie == DirectieTva.Colectat && Tva("FCL").SursaContrapartida == SursaCont.RepartitorPrimitor
                && Tva("FCL").ContrapartidaFallback?.Simbol == "4111"
                && Tva("NIR") == null && Tva("PLT") == null && Tva("INC") == null
                && new[] { "FCT", "DEC", "RLF", "DVI" }
                    .All(c => Tva(c)?.Directie == DirectieTva.Deductibil)
                && new[] { "FCL", "RDC" }
                    .All(c => Tva(c)?.Directie == DirectieTva.Colectat));
            s.Check("Seed: profilul de validare privat — fără clasificație bugetară; FCL NU mai interzice stocul (P2, descărcarea de gestiune preia vânzarea din stoc)",
                os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCT") == null
                && os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == "FCL")?.NaturaInterzisa != NaturaClasa.Stoc);
            s.Check("Seed: plan OMFP fără defalcări obligatorii (pornesc goale — design §5)",
                !os.GetObjectsQuery<Cont>().Any(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna));

            // G3 (decizia 50f): Tipurile create ad-hoc de importul 1C sunt acum seed
            // EXPLICIT — cod, clasă și cont implicit din profil, nu ghicite din prima
            // cifră a simbolului. Upsert pe Cod ⇒ bazele importate se corectează.
            (string Clasa, string[] Coduri)[] promovate = [
                ("TER", ["408", "4091", "4092", "419", "447", "473", "5328", "S371"]),
                ("C", ["6021", "6022", "6028", "604", "6422", "6458", "6581", "6651", "667"]),
                ("S", ["6051", "6052", "6053", "6231", "6232", "624", "627"]),
                ("VEN", ["767", "7581", "7588"]),
            ];
            var tipuriPromovate = promovate
                .SelectMany(g => g.Coduri.Select(cod => (g.Clasa, Cod: cod)))
                .Select(x => (x.Clasa, x.Cod, Tip: os.FirstOrDefault<TipMaterial>(t => t.Cod == x.Cod))).ToList();
            s.Check($"Seed G3: toate cele {tipuriPromovate.Count} Tipuri promovate există, în clasa declarată "
                + "(TER=terți/regularizări, C=cheltuieli, S=servicii, VEN=venituri)",
                tipuriPromovate.Count == 27
                && tipuriPromovate.All(x => x.Tip != null && x.Tip.Clasa?.Cod == x.Clasa));
            s.Check("Seed G3: clasa nouă TER e de natură Serviciu (paritate cu clasificarea ad-hoc — "
                + "regulile de contare se potrivesc pe natură)",
                os.FirstOrDefault<ClasaProdus>(c => c.Cod == "TER")?.Natura == NaturaClasa.Serviciu);
            s.Check("Seed G3: fiecare Tip promovat are cont implicit — cele 26 numerice pe simbolul lor, "
                + "puntea S371 explicit pe 371 (Cod-ul ei nu e simbol de cont)",
                tipuriPromovate.All(x => x.Tip?.ContImplicitId != null
                    && os.GetObjectByKey<Cont>(x.Tip.ContImplicitId.Value).Simbol == (x.Cod == "S371" ? "371" : x.Cod)));
            s.Check("Seed G3: denumirile vin din planul OMFP, nu din import („1C: cont X”)",
                tipuriPromovate.All(x => x.Tip?.Denumire != null && !x.Tip.Denumire.StartsWith("1C:")));

            // Decizia 51c: convenția de rotunjire e dată de profil, un rând per bază.
            var setareProfil = os.GetObjectsQuery<SetareProfil>().ToList();
            s.Check("Seed 51c: un singur rând SetareProfil, pe profilul bazei, cu convenția de rotunjire "
                + "aplicată în Scara (Flax rămâne AwayFromZero)",
                setareProfil.Count == 1 && setareProfil[0].Profil == ProfilContabil.Privat
                && setareProfil[0].RotunjireBani == MidpointRounding.AwayFromZero
                && Scara.ConventieBani == MidpointRounding.AwayFromZero);

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajPrv + "-FURN";
            furnizor.Denumire = "Furnizor probă privat";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajPrv + "-CL";
            client.Denumire = "Client probă privat";
            var angajat = os.CreateObject<Angajat>();
            angajat.Cod = MarcajPrv + "-ANG";
            angajat.Denumire = "Titular probă privat"; // fără ContImplicit — fallback 542
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajPrv;
            produs.Denumire = "Produs probă privat";
            produs.UM = "BUC";
            produs.TipMaterial = tip302;
            os.CommitChanges();

            List<PostareScena> NoteCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => !p.Storno).ToList();
            List<PostareScena> StornoCub(Document doc) => CubScena.Note(os, doc.ID).Where(p => p.Storno).ToList();

            // --- FCT: linie stoc + linie serviciu, N21, plata automată pe brut ---
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = "E2E-PRV-FF1";
            fct.Data = new DateOnly(2026, 3, 3);
            fct.Predator = furnizor;
            fct.Primitor = mag1;
            fct.GenereazaPlata = true;
            fct.PlataContPropriu = banca;
            fct.PlataNumar = "OP-P1";
            fct.PlataData = new DateOnly(2026, 3, 4);
            var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
            linieStoc.Document = fct;
            linieStoc.TipMaterial = tip302;
            linieStoc.Cantitate = 5m;
            linieStoc.PretUnitar = 10m;
            linieStoc.TipTva = n21;
            var lot = linieStoc.CreeazaLot(os, produs, mag1);
            var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
            linieServiciu.Document = fct;
            linieServiciu.TipMaterial = tip628;
            linieServiciu.Cantitate = 1m;
            linieServiciu.PretUnitar = 100m;
            linieServiciu.TipTva = n21;
            os.CommitChanges();

            var conex = MotorOperare.Opereaza(os, fct);
            s.Check("FCT privat: lanțul de valori NET + TVA separat (50/10,5 și 100/21)",
                linieStoc.Valoare == 50m && linieStoc.ValoareTva == 10.5m
                && linieServiciu.Valoare == 100m && linieServiciu.ValoareTva == 21m);
            s.Check("Lot finalizat la NET: preț 10 (fără TVA capitalizat)",
                lot.PretUnitar == 10m);
            s.Check("Total factură = BRUT (181,5) — imperecherea stinge brutul",
                fct.Total == 181.5m);
            var noteFctCub = NoteCub(fct);
            s.Check("FCT: 3 note — serviciul net (628 = 401, 100) + câte un rând 4426 per linie",
                noteFctCub.Nota(tip628.ContImplicitId, cont401.ID, 100m)
                && noteFctCub.Count(p => p.Debit && p.Cont == cont4426.ID
                    && noteFctCub.Any(c => c.Credit && c.Cont == cont401.ID && c.LinieId == p.LinieId && c.Valoare == p.Valoare)) == 2);
            s.Check("Rândul 4426 al liniei de STOC există deși linia nu are regulă principală (netul e pe NIR)",
                noteFctCub.Any(p => p.LinieId == linieStoc.ID && p.Debit && p.Cont == cont4426.ID && p.Valoare == 10.5m));
            s.Check("Rândul 4426 al serviciului: 21; dimensiunile din default-ul polimorf al header-ului [cub]",
                noteFctCub.Any(p => p.LinieId == linieServiciu.ID && p.Debit && p.Cont == cont4426.ID && p.Valoare == 21m));
            s.Check("D9-A10 FCT: piciorul intern (628, 4426) poartă gestiunea primitoare, piciorul de terț (401) furnizorul",
                noteFctCub.Where(p => p.Debit).All(p => p.Gestiune == mag1.ID)
                && noteFctCub.Where(p => p.Cont == cont401.ID).All(p => p.Repartitor == furnizor.ID));

            // --- NIR conex: netul, fără TVA ---
            s.Check("Conex: NIR draft cu linia de stoc la NET, TipTva clonat ca informație, ValoareTva 0",
                conex is NIR { Stare: StareDocument.Draft } && conex.Detalii.Count == 1
                && conex.Detalii[0].Valoare == 50m && conex.Detalii[0].ValoareTva == 0m
                && conex.Detalii[0].TipTvaId == n21.ID);
            MotorOperare.Opereaza(os, conex);
            var stocFctCub = CubScena.Stoc(os, fct.ID);
            s.Check("NIR: +5/50 în stoc (evaluare la net) și o singură notă 302 = 401, 50 — fără rânduri TVA",
                stocFctCub.Count == 1 && stocFctCub[0].Cantitate == 5m && stocFctCub[0].Semnata == 50m
                && NoteCub(fct).Nota(tip302.ContImplicitId, cont401.ID, 50m));

            // --- Plata automată: defalcarea pe BRUT + imperecherea integrală ---
            var plataAuto = os.GetObjectsQuery<Plata>().Single(p => p.DocumentSursaId == fct.ID);
            s.Check("Plata autogenerată: liniile clonează defalcarea BRUTĂ (60,5 + 121 = 181,5), fără TipTva",
                plataAuto.Detalii.Count == 2 && plataAuto.Detalii.Sum(d => d.Valoare) == 181.5m
                && plataAuto.Detalii.All(d => d.TipTvaId == null && d.ValoareTva == 0m));
            MotorOperare.Opereaza(os, plataAuto);
            var notePlataCub = NoteCub(plataAuto);
            s.Check("Plata contează 401 = 5121 (banca) pe brut",
                notePlataCub.All(p => p.Debit ? p.Cont == cont401.ID : p.Cont == cont5121.ID)
                && notePlataCub.Rulaj(cont401.ID, N.Latura.Debit) == 181.5m
                && notePlataCub.Rulaj(cont5121.ID, N.Latura.Credit) == 181.5m);
            s.Check("Imperecherea automată stinge BRUTUL facturii (181,5; rest 0)",
                os.GetObjectsQuery<Imperechere>().Single(i => i.DocumentStingatorId == plataAuto.ID).Suma == 181.5m
                && ImperechereService.Ramas(os, fct.ID) == 0m);

            // --- NUC-PLT-FCT (B-D5, pas 4): PRIVAT — 401 are RolTert, deci aici se probeaza
            //     nominalizarea TR-D2a (partida facturii) si partenerul pe piciorul de tert ---
            ProbeNucleu.Proba(os, s.Check, "NUC-PLT-FCT", [plataAuto]);

            // --- NUC-FCT-P1 (B-D6, pas 5): recepția TR-D3 + 4426 pe ambele linii, pe profilul
            //     cu `RolTert` — o singură partidă pe 401 poartă și netul, și taxa ---
            ProbeNucleu.Proba(os, s.Check, "NUC-FCT-P1", [fct]);

            // --- FCL: 4427 colectat ---
            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Data = new DateOnly(2026, 3, 6);
            fcl.Predator = sediu;
            fcl.Primitor = client;
            var linieVenit = os.CreateObject<FacturaIesireDetaliu>();
            linieVenit.Document = fcl;
            linieVenit.TipMaterial = tip704;
            linieVenit.Cantitate = 1m;
            linieVenit.PretUnitar = 200m;
            linieVenit.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl);
            var noteFclCub = NoteCub(fcl);
            s.Check("FCL: 4111 = 704 net (200) + 4111 = 4427 (42); scadența default +30; total brut 242",
                noteFclCub.Nota(cont4111.ID, tip704.ContImplicitId, 200m)
                && noteFclCub.Nota(cont4111.ID, cont4427.ID, 42m)
                && fcl.DataScadenta == fcl.Data.AddDays(30) && fcl.Total == 242m);

            // --- DEC: 4426 = 542 pe titular ---
            var dec = os.CreateObject<Decont>();
            dec.Data = new DateOnly(2026, 3, 8);
            dec.Predator = angajat;
            dec.Primitor = sediu;
            var linieDec = os.CreateObject<DecontDetaliu>();
            linieDec.Document = dec;
            linieDec.TipMaterial = tip628;
            linieDec.PretUnitar = 30m; // cantitatea pro-formă 0 → 1
            linieDec.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, dec);
            var noteDecCub = NoteCub(dec);
            s.Check("DEC: cheltuiala net (628 = 542, 30) + TVA justificat (4426 = 542, 6,3), creditul pe TITULAR [cub]",
                noteDecCub.Nota(tip628.ContImplicitId, cont542.ID, 30m)
                && noteDecCub.Nota(cont4426.ID, cont542.ID, 6.3m));
            s.Check("D9-A10 DEC: creditul 542 poartă titularul pe ambele linii (terțul liniei)",
                noteDecCub.Where(p => p.Credit).All(p => p.Cont == cont542.ID && p.Repartitor == angajat.ID));

            // --- Taxare inversă: 4426 = 4427, apoi storno cu rândurile TVA inverse ---
            var fctTi = os.CreateObject<FacturaIntrare>();
            fctTi.Numar = "E2E-PRV-FF2";
            fctTi.Data = new DateOnly(2026, 3, 9);
            fctTi.Predator = furnizor;
            fctTi.Primitor = mag1;
            var linieTi = os.CreateObject<FacturaIntrareDetaliu>();
            linieTi.Document = fctTi;
            linieTi.TipMaterial = tip628;
            linieTi.Cantitate = 1m;
            linieTi.PretUnitar = 100m;
            linieTi.TipTva = ti21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctTi);
            var noteTiCub = NoteCub(fctTi);
            s.Check("Taxare inversă: serviciul net (628 = 401, 100) + autolichidare 4426 = 4427 (21), total facturii NET",
                noteTiCub.Nota(tip628.ContImplicitId, cont401.ID, 100m)
                && noteTiCub.Nota(cont4426.ID, cont4427.ID, 21m)
                && fctTi.Total == 121m);

            // --- NUC-FCT-TI (B-D6, pas 5): autolichidarea — 4426 poartă faptul fiscal,
            //     4427 nu (azi nu există rând fiscal colectat pe taxare inversă) ---
            ProbeNucleu.Proba(os, s.Check, "NUC-FCT-TI", [fctTi]);

            MotorOperare.Storneaza(os, fctTi, new DateOnly(2026, 7, 23));
            var stornoTiCub = StornoCub(fctTi);
            s.Check("Storno cu TVA: rândurile inverse includ și rândul 4426 = 4427 (−100, −21)",
                stornoTiCub.Any(p => p.Valoare == -100m)
                && stornoTiCub.Any(p => p.Debit && p.Cont == cont4426.ID && p.Valoare == -21m));

            // --- F13-D1: taxarea inversă pe LIVRARE nu poartă TVA ---
            //
            // Cealaltă jumătate a perechii de mai sus, și proba că regula e per
            // (regim × latură), nu per regim. Cod fiscal art. 331: furnizorul emite
            // factura FĂRĂ taxă, cu mențiunea „taxare inversă"; o declară și o deduce
            // BENEFICIARUL. Până la F13 motorul îi calcula totuși 21% și posta
            // 4426 = 4427 pe o factură EMISĂ — taxă inventată, pe care D300 o ocolea
            // printr-o excepție și pe care Import1C o compensa la sursă.
            //
            // Proba e discriminantă pe trei planuri deodată: linia (ValoareTva 0 și
            // Total = netul), registrul CONTABIL (niciun rând de TVA — nici măcar
            // unul de zero) și registrul FISCAL (rândul EXISTĂ, cu bază și fără
            // taxă: 68 — liniile fără TVA postat apar legal în jurnal, și de acolo
            // pe rd. 13 al decontului).
            var fclTi = os.CreateObject<FacturaIesire>();
            fclTi.Data = new DateOnly(2026, 3, 11);
            fclTi.Predator = sediu;
            fclTi.Primitor = client;
            var linieFclTi = os.CreateObject<FacturaIesireDetaliu>();
            linieFclTi.Document = fclTi;
            linieFclTi.TipMaterial = tip704;
            linieFclTi.Cantitate = 1m;
            linieFclTi.PretUnitar = 300m;
            linieFclTi.TipTva = ti21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclTi);
            var noteFclTiCub = NoteCub(fclTi);
            var fiscalFclTiCub = CubScena.Fapte(os, fclTi.ID);
            s.Check("F13-D1 FCL + TI21: livrarea în taxare inversă NU poartă taxă — linia rămâne pe net (300, "
                + "ValoareTva 0, Total = netul), registrul contabil are DOAR venitul (4111 = 704) fără 4426/4427, "
                + "iar rândul fiscal există cu bază 300 și TVA 0 (jurnalul îl cere, decontul îl pune pe rd. 13)",
                linieFclTi.Valoare == 300m && linieFclTi.ValoareTva == 0m && fclTi.Total == 300m
                && noteFclTiCub.Nota(cont4111.ID, tip704.ContImplicitId, 300m)
                && !noteFclTiCub.Any(p => p.Cont == cont4426.ID || p.Cont == cont4427.ID)
                && fiscalFclTiCub.Count == 1
                && fiscalFclTiCub[0] is { Sens: SensTva.Livrare, Regim: RegimTva.TaxareInversa, Cota: 21m,
                    Baza: 300m, Tva: 0m, Storno: false });
            // Taxarea inversa pe livrare in cub: 4111 = 704 cu `CodTva` Baza si FARA
            // postare de taxa — randul fiscal are `Tva = 0`, eliminat de `FiscalCaAtribute`.
            ProbeCub.ProbaOperare(os, s.Check, "NUC-FCL-TI", fclTi);
            MotorOperare.Storneaza(os, fclTi, new DateOnly(2026, 7, 24));
            var stornoFclTiCub = StornoCub(fclTi);
            s.Check("F13-D1 storno FCL-TI: se inversează DOAR venitul (−300) — nu există rând de TVA de stornat, "
                + "nici pe 4426, nici pe 4427 (simetria cu operarea: ce n-a fost postat n-are ce fi inversat)",
                stornoFclTiCub.Nota(cont4111.ID, tip704.ContImplicitId, -300m)
                && stornoFclTiCub.All(p => p.Valoare == -300m)
                && !stornoFclTiCub.Any(p => p.Cont == cont4426.ID || p.Cont == cont4427.ID));

            // --- F13-D1, gardul: TVA CULES pe taxare inversă la livrare = refuz ---
            //
            // 62f („un gard care tace devine capcană"): `TvaService` aduce oricum
            // valoarea la 0, deci fără gard operatorul ar vedea TVA-ul lui dispărând
            // în tăcere. Ori linia are alt regim decât crede el, ori suma e greșită —
            // amândouă merită spuse. Gardul stă în MOTOR (o singură sursă de reguli,
            // 42a), deci acoperă și calea XAF, și PUT-ul de API; citește valorile
            // CULESE, adică dinainte ca `PregatesteOperare` să le zerorizeze.
            var fclTiCules = os.CreateObject<FacturaIesire>();
            fclTiCules.Data = new DateOnly(2026, 3, 12);
            fclTiCules.Predator = sediu;
            fclTiCules.Primitor = client;
            var linieFclTiCules = os.CreateObject<FacturaIesireDetaliu>();
            linieFclTiCules.Document = fclTiCules;
            linieFclTiCules.TipMaterial = tip704;
            linieFclTiCules.Cantitate = 1m;
            linieFclTiCules.PretUnitar = 300m;
            linieFclTiCules.TipTva = ti21;
            linieFclTiCules.ValoareTva = 63m;
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fclTiCules, linieFclTiCules, nameof(DocumentDetaliu.ValoareTva));
            os.CommitChanges();
            var eroriFclTiCules = MotorOperare.Valideaza(os, fclTiCules);
            // Dry-run-ul NU e read-only pe ObjectSpace-ul primit (contract de apelant
            // `MotorOperare.Valideaza`): `PregatesteOperare` a rulat deja și a adus
            // TVA-ul cules la 0. Aici, unde OS-ul e partajat, valoarea se reface — a
            // doua probă trebuie să pornească din ACEEAȘI stare culeasă, altfel ar
            // trece degeaba.
            linieFclTiCules.ValoareTva = 63m;
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fclTiCules, linieFclTiCules, nameof(DocumentDetaliu.ValoareTva));
            s.CheckRefuza("F13-D1 gard: FCL + TI21 cu ValoareTva CULES (63) → refuz la operare, nu înghițire tăcută",
                () => MotorOperare.Opereaza(os, fclTiCules));
            s.Check("F13-D1 gard: mesajul numește linia și suma culeasă, iar dry-run-ul (`Valideaza`) îl arată "
                + "clientului ÎNAINTE de comandă — plus nimic materializat (33d)",
                eroriFclTiCules.Count == 1
                && eroriFclTiCules[0].StartsWith("Taxarea inversă pe livrare nu poartă TVA;")
                && eroriFclTiCules[0].Contains("linia 1")
                && eroriFclTiCules[0].Contains("63")
                && fclTiCules.Stare == StareDocument.Draft
                && CubScena.FaraNote(os, fclTiCules.ID)
                && CubScena.FaraFapte(os, fclTiCules.ID));
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fclTiCules, linieFclTiCules, nameof(DocumentDetaliu.ValoareTva));

            // --- Capitalizat (nedeductibil): comportamentul bugetar, ca date ---
            var fctNed = os.CreateObject<FacturaIntrare>();
            fctNed.Numar = "E2E-PRV-FF3";
            fctNed.Data = new DateOnly(2026, 3, 10);
            fctNed.Predator = furnizor;
            fctNed.Primitor = mag1;
            var linieNed = os.CreateObject<FacturaIntrareDetaliu>();
            linieNed.Document = fctNed;
            linieNed.TipMaterial = tip628;
            linieNed.Cantitate = 1m;
            linieNed.PretUnitar = 100m;
            linieNed.TipTva = ned21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctNed);
            s.Check("Capitalizat (NED21): Valoare = brut 121, ValoareTva 0, o singură notă 628 = 401 — fără rând 4426",
                linieNed.Valoare == 121m && linieNed.ValoareTva == 0m
                && NoteCub(fctNed).Rulaj(tip628.ContImplicitId, N.Latura.Debit) == 121m
                && NoteCub(fctNed).Rulaj(cont401.ID, N.Latura.Credit) == 121m
                && !NoteCub(fctNed).Any(p => p.Cont == cont4426.ID));

            // --- NUC-FCT-CAP (B-D6, pas 5): brutul se declară ca bază (100) + taxă (21) pe
            //     ACELAȘI cont de cost, fiindcă jurnalul are două cifre acolo unde registrul
            //     contabil are una (amendament la B-D8 pct. 5) ---
            var contractNed = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fctNed);
            var costNed = contractNed.Tranzactii.SelectMany(t => t.Postari)
                .Where(p => p.Coordonate.Cont == tip628.ContImplicitId)
                .Select(p => (Rol: p.Coordonate.CodTva?.Rol, p.Valoare))
                .OrderBy(p => p.Rol)
                .ToList();
            s.Check("NUC-FCT-CAP: postarea de 121 pe 628 se desface în 100 (Bază) + 21 (Taxă), "
                + "Σ neschimbată — jurnalul e proiecția pe `CodTva` (090a)",
                contractNed.EsteAcceptat
                && costNed is [(N.RolTva.Baza, 100m), (N.RolTva.Taxa, 21m)]
                && costNed.Sum(p => p.Valoare) == 121m);
            ProbeNucleu.Proba(os, s.Check, "NUC-FCT-CAP", [fctNed]);

            // --- ValoareTva culeasă pe FCT bate rotunjirea noastră (design §3) ---
            var fctManual = os.CreateObject<FacturaIntrare>();
            fctManual.Numar = "E2E-PRV-FF4";
            fctManual.Data = new DateOnly(2026, 3, 11);
            fctManual.Predator = furnizor;
            fctManual.Primitor = mag1;
            var linieManual = os.CreateObject<FacturaIntrareDetaliu>();
            linieManual.Document = fctManual;
            linieManual.TipMaterial = tip628;
            linieManual.Cantitate = 1m;
            linieManual.PretUnitar = 100m;
            linieManual.TipTva = n21;
            linieManual.ValoareTva = 20.9m; // TVA-ul de pe factura furnizorului
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fctManual, linieManual, nameof(DocumentDetaliu.ValoareTva));
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctManual);
            s.Check("ValoareTva culeasă manual (20,9) nu se suprascrie la operare; rândul 4426 o postează",
                linieManual.ValoareTva == 20.9m
                && NoteCub(fctManual).Rulaj(cont4426.ID, N.Latura.Debit) == 20.9m);

            // --- NUC-FCT-OVERRIDE (B-D6, pas 5): COMPORTAMENT NOU (090j) — taxa culeasă
            //     rămâne autoritară, dar se validează contra celei decise pe document ×
            //     cotă; 20,9 se abate cu 0,10 de la 21,00, peste toleranța de 0,01 × o
            //     linie cu TVA, deci declarantul REFUZĂ un document pe care motorul vechi
            //     îl operează. Diferența e CONSEMNATĂ, nu normalizată (B-D8 pct. 7).
            // S-D15: seed-ul nu mai pune toleranță (taxa culeasă e autoritară, ca motorul
            // vechi); gardul se probează cu valoarea pusă LOCAL pe politica tipului.
            var contractFaraGard = Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fctManual);
            s.Check("NUC-FCT-TOLERANTA-NULL: fără toleranță pe politică, taxa culeasă (20,90) rămâne autoritară "
                + "și documentul trece, ca în motorul vechi (S-D15)",
                contractFaraGard.EsteAcceptat
                && contractFaraGard.Tranzactii.SelectMany(t => t.Postari).Any(p => p.Coordonate.Cont == cont4426.ID && p.Valoare == 20.9m));
            using (ProbeCub.CuToleranta(os, fctManual, 0.01m)) {
                var contractOverride =
                    Atlas.Conta.BackOffice.Module.Declaratii.Contractare.Contracteaza(os, fctManual);
                Console.WriteLine("     MĂSURAT (090j/PRIVAT): taxa culeasă 20,90, taxa pe document × cotă 21,00, "
                    + "toleranța 0,01 ⇒ "
                    + string.Join(" | ", contractOverride.Refuzuri.Select(r => $"{r.Cod}: {r.Mesaj}")));
                s.Check("NUC-FCT-OVERRIDE: taxa culeasă peste toleranță → refuz TVA_IN_AFARA_TOLERANTEI, "
                    + "singurul refuz al contractului",
                    !contractOverride.EsteAcceptat
                    && contractOverride.Refuzuri.Count == 1
                    && contractOverride.Refuzuri[0].Cod == N.Coduri.TvaInAfaraTolerantei);
            }

            // --- Aceeași regulă pe FCL și DEC (36a uniformizat — decizia 48b) ---
            // Recalculul din cotă ar da 21,00; documentul real poartă 20,99, iar
            // rândul de TVA trebuie să posteze EXACT valoarea culeasă.
            var fclManual = os.CreateObject<FacturaIesire>();
            fclManual.Data = new DateOnly(2026, 3, 12);
            fclManual.Predator = sediu;
            fclManual.Primitor = client;
            var linieFclManual = os.CreateObject<FacturaIesireDetaliu>();
            linieFclManual.Document = fclManual;
            linieFclManual.TipMaterial = tip704;
            linieFclManual.Cantitate = 1m;
            linieFclManual.PretUnitar = 100m;
            linieFclManual.TipTva = n21;
            linieFclManual.ValoareTva = 20.99m; // TVA-ul de pe factura emisă (rotunjirea ei)
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, fclManual, linieFclManual, nameof(DocumentDetaliu.ValoareTva));
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclManual);
            s.Check("FCL: ValoareTva culeasă (20,99) nu se suprascrie; 4111 = 4427 postează exact 20,99",
                linieFclManual.Valoare == 100m && linieFclManual.ValoareTva == 20.99m
                && NoteCub(fclManual).Nota(cont4111.ID, cont4427.ID, 20.99m)
                && fclManual.Total == 120.99m);

            var decManual = os.CreateObject<Decont>();
            decManual.Data = new DateOnly(2026, 3, 13);
            decManual.Predator = angajat;
            decManual.Primitor = sediu;
            var linieDecManual = os.CreateObject<DecontDetaliu>();
            linieDecManual.Document = decManual;
            linieDecManual.TipMaterial = tip628;
            linieDecManual.PretUnitar = 100m;
            linieDecManual.TipTva = n21;
            linieDecManual.ValoareTva = 20.99m; // TVA-ul de pe bonul justificat
            Atlas.Conta.BackOffice.Module.Culegere.CulegereDocument.LinieSchimbata(os, decManual, linieDecManual, nameof(DocumentDetaliu.ValoareTva));
            os.CommitChanges();
            MotorOperare.Opereaza(os, decManual);
            s.Check("DEC: ValoareTva culeasă (20,99) nu se suprascrie; 4426 = 542 postează exact 20,99",
                linieDecManual.Valoare == 100m && linieDecManual.ValoareTva == 20.99m
                && NoteCub(decManual).Nota(cont4426.ID, cont542.ID, 20.99m));

            CurataPrv(os);
            s.Check("Curățenie finală privat (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajPrv))
                && !os.GetObjectsQuery<Produs>().Any(p => p.Cod == MarcajPrv));
        }
    }
}
