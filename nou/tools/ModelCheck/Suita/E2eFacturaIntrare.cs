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

// ========================= Scenariul e2e 3c: FCT → NIR =========================
// Lanțul de cumpărare: factura cu linie de stoc (lot creat la culegere) + linie
// de serviciu → operare (postează DOAR serviciul, finalizează lotul, generează
// NIR conex cu liniile de stoc) → operare NIR (+1 stoc, contează recepția) →
// gardienii grupului conex (anulare/storno pe părinte) → surse de cont
// (TipMaterial / ContImplicit partener, fallback 401/404).
static class E2eFacturaIntrare {
    public static void Ruleaza(Suita s) {
        const string MarcajFct = "E2E-FCT-PRB";

        void CurataFct(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            foreach (var fct in os.GetObjectsQuery<FacturaIntrare>().Where(d => d.Numar.StartsWith("E2E-FF")).ToList()) {
                foreach (var copil in os.GetObjectsQuery<Document>().Where(x => x.DocumentSursaId == fct.ID).ToList()) {
                    pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
                    pj.Adauga(copil);
                }
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == fct.ID).ToList());
                pj.Adauga(fct);
            }
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == MarcajFct).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == MarcajFct).ToList());
            pj.Adauga(os.GetObjectsQuery<Partener>().Where(p => p.Cod.StartsWith("E2E-FURN")).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == "E2E-CE").ToList());
            pj.Adauga(os.GetObjectsQuery<SursaFinantare>().Where(c => c.Cod == "E2E-SF").ToList());
            pj.Adauga(os.GetObjectsQuery<CodFunctional>().Where(c => c.Cod == "E2E-CF").ToList());
            pj.Adauga(os.GetObjectsQuery<Proiect>().Where(c => c.Cod == "E2E-PR").ToList());
            pj.Adauga(os.GetObjectsQuery<Angajament>().Where(c => c.Cod == "E2E-ANG").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataFct(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipMateriale = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302.01.00");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404.01.00");
            // P1: cota nu se mai culege pe linie — regimul Capitalizat (neplătitor)
            // vine din nomenclatorul TipTva al profilului; 19% e rândul istoric.
            var cap19 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP19");

            s.Check("Seed P1 (bugetar): TipTva Capitalizat ca date, fără conturi de TVA și fără PoliticaTva",
                cap19 != null && cap19.Regim == RegimTva.Capitalizat && cap19.Cota == 19m
                && cap19.ContTvaDeductibilId == null
                && !os.GetObjectsQuery<PoliticaTva>().Any());

            // Maparea Clasă/Tip → cont derivată de seed din simboluri (decizia 4).
            s.Check("Seed: Tip 302.01.00 → cont 302.01.00 (potrivire exactă)",
                tipMateriale.ContImplicitId != null
                && os.GetObjectByKey<Cont>(tipMateriale.ContImplicitId.Value).Simbol == "302.01.00");
            s.Check("Seed: Tip 628.00.00 → cont 628.* (tăierea segmentelor)",
                tipServicii.ContImplicitId != null
                && os.GetObjectByKey<Cont>(tipServicii.ContImplicitId.Value).Simbol.StartsWith("628"));

            // Politicile de validare per tip (3d): profilul bugetar cere clasificație
            // pe documentele de angajare/plată; INC rămâne fără rând (venituri —
            // defalcarea E a conturilor de trezorerie acoperă nivelul de cont).
            bool CereClasificatie(string cod) =>
                os.FirstOrDefault<PoliticaValidare>(p => p.TipDocument.Cod == cod)?.CereClasificatieBugetara == true;
            s.Check("Seed 3d: FCT/DEC/PLT cer clasificație bugetară; INC nu",
                CereClasificatie("FCT") && CereClasificatie("DEC") && CereClasificatie("PLT") && !CereClasificatie("INC"));

            // P2 (design §7): la bugetar ancora DSC există (nucleu, ca BPR) dar e INERTĂ —
            // fără politici, deci hook-ul GenereazaSecundar nu descarcă nimic; FCL rămâne
            // pur creanță cu natura Stoc interzisă. Datoria P1 (37f): default CAP21.
            s.Check("Seed bugetar: ancora DSC există și e inertă (0 RegulaStoc, 0 RegulaContare pe DSC)",
                os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC") != null
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "DSC")
                && !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocument.Cod == "DSC"));
            var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");
            s.Check("Seed bugetar: TipTvaImplicit CAP21 pe FCT/FCL/DEC; NIR/DSC null",
                os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT")?.TipTvaImplicitId == cap21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL")?.TipTvaImplicitId == cap21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "DEC")?.TipTvaImplicitId == cap21.ID
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR")?.TipTvaImplicitId == null
                && os.FirstOrDefault<TipDocument>(t => t.Cod == "DSC")?.TipTvaImplicitId == null);

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = "E2E-FURN";
            furnizor.Denumire = "Furnizor probă e2e";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = "E2E-CE";
            codEc.Denumire = "Cod economic probă e2e";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajFct;
            produs.Denumire = "Produs probă FCT";
            produs.UM = "BUC";
            produs.TipMaterial = tipMateriale;
            os.CommitChanges();

            var fct = os.CreateObject<FacturaIntrare>();
            fct.Data = new DateOnly(2026, 3, 3);
            fct.Predator = furnizor;
            fct.Primitor = mag1;
            var linieStoc = os.CreateObject<FacturaIntrareDetaliu>();
            linieStoc.Document = fct;
            linieStoc.TipMaterial = tipMateriale;
            linieStoc.Cantitate = 5m;
            linieStoc.PretUnitar = 10m;
            linieStoc.TipTva = cap19;
            linieStoc.LotFabricatie = "LOT-A";
            linieStoc.DataExpirare = new DateOnly(2027, 1, 1);
            var linieServiciu = os.CreateObject<FacturaIntrareDetaliu>();
            linieServiciu.Document = fct;
            linieServiciu.TipMaterial = tipServicii;
            linieServiciu.Cantitate = 1m;
            linieServiciu.PretUnitar = 100m;

            // Validările proprii FCT: număr furnizor, clasificație bugetară, lot pe stoc.
            s.CheckRefuza("FCT fără număr/clasificație/lot → refuz", () => MotorOperare.Opereaza(os, fct));
            fct.Numar = "E2E-FF1";
            linieStoc.CodEconomicId = codEc.ID;
            linieServiciu.CodEconomicId = codEc.ID;
            var lot = linieStoc.CreeazaLot(os, produs, mag1);
            os.CommitChanges();

            // --- Operare FCT: postează serviciul, finalizează lotul, generează NIR ---
            var conex = MotorOperare.Opereaza(os, fct);
            s.Check("FCT operată; lanțul de valori materializat (59,5 / 100)",
                fct.Stare == StareDocument.Operat && linieStoc.Valoare == 59.5m && linieServiciu.Valoare == 100m);
            s.Check("Lot finalizat: preț 11,9 (cu TVA capitalizat) + atribute copiate",
                lot.PretUnitar == 11.9m && lot.Data == fct.Data
                && lot.LotFabricatie == "LOT-A" && lot.DataExpirare == new DateOnly(2027, 1, 1));
            var noteFctCub = CubScena.Note(os, fct.ID);
            s.Check("FCT contează DOAR linia de serviciu: 628 = 401, 100",
                noteFctCub.Count(p => p.LinieId == linieServiciu.ID) == 2
                && noteFctCub.Nota(tipServicii.ContImplicitId, cont401.ID, 100m, linieServiciu.ID));
            s.Check("Nota FCT: dimensiuni rezolvate (cod economic + repartitori laturi) [cub]",
                noteFctCub.Any(p => p.LinieId == linieServiciu.ID && p.Debit && p.CodEconomic == codEc.ID));
            s.Check("D9-A10 FCT: debitul 628 poartă gestiunea primitoare MAG1, creditul 401 furnizorul (nu convenția pozițională)",
                noteFctCub.Where(p => p.LinieId == linieServiciu.ID && p.Debit).All(p => p.Gestiune == mag1.ID)
                && noteFctCub.Where(p => p.LinieId == linieServiciu.ID && p.Credit).All(p => p.Repartitor == furnizor.ID));

            s.Check("Conex generat: NIR draft autogenerat, aceleași laturi",
                conex is NIR { Stare: StareDocument.Draft, Autogenerat: true }
                && conex.DocumentSursaId == fct.ID
                && conex.PredatorId == furnizor.ID && conex.PrimitorId == mag1.ID);
            s.Check("NIR-ul preia DOAR linia de stoc, cu lot, cantitate, valoare, dimensiuni",
                conex.Detalii.Count == 1 && conex.Detalii[0].TipMaterialId == tipMateriale.ID
                && conex.Detalii[0].LotId == lot.ID && conex.Detalii[0].Cantitate == 5m
                && conex.Detalii[0].Valoare == 59.5m && conex.Detalii[0].DimensiuniCulese().CodEconomicId == codEc.ID);

            // --- Operare NIR: singurul +1 al lanțului + contarea recepției ---
            var nir = (NIR)conex;
            s.Check("NIR-ul nu generează alt conex", MotorOperare.Opereaza(os, nir) == null);
            s.Check("NIR operat cu număr din politică", nir.Stare == StareDocument.Operat && nir.Numar?.StartsWith("NIR-") == true);
            var stocFctCub = CubScena.Stoc(os, fct.ID);
            s.Check("NIR → +5/+59,5 Magazie pe gestiunea primitoare",
                stocFctCub.Count == 1 && stocFctCub[0].Cont == tipMateriale.ContImplicitId && stocFctCub[0].Gestiune == mag1.ID
                && stocFctCub[0].Cantitate == 5m && stocFctCub[0].Semnata == 59.5m && stocFctCub[0].Unitate == lot.ID);
            var receptieFctCub = CubScena.Note(os, fct.ID).Where(p => p.LinieId == linieStoc.ID).ToList();
            s.Check("NIR contează recepția: 302.01.00 = 401, 59,5",
                receptieFctCub.Nota(tipMateriale.ContImplicitId, cont401.ID, 59.5m));
            s.Check("Nota NIR: Materialul implicit din lot (produsul) pe ambele laturi (3d)",
                receptieFctCub.Count == 2 && receptieFctCub.All(p => p.Produs == produs.ID));
            s.Check("Sold lot după recepție: 5 pe MAG1",
                CubScena.Sold(os, lot.ID, mag1.ID).Cantitate == 5m);

            // --- NUC-FCT (B-D6, pas 5): recepția TR-D3 declarată pe factură, cu NIR-ul absorbit ---
            ProbeNucleu.Proba(os, s.Check, "NUC-FCT", [fct]);

            // --- Grupul conex la anulare/storno ---
            s.CheckRefuza("Anularea FCT cu NIR operat → refuzată", () => MotorOperare.AnuleazaOperarea(os, fct));
            MotorOperare.AnuleazaOperarea(os, nir);
            s.Check("NIR anulat (corecție directă): Draft, fără rânduri",
                nir.Stare == StareDocument.Draft && CubScena.Sold(os, lot.ID, mag1.ID) is { Cantitate: 5m, Valoare: 59.5m });
            MotorOperare.AnuleazaOperarea(os, fct);
            s.Check("Anularea FCT șterge NIR-ul draft autogenerat",
                fct.Stare == StareDocument.Draft && !os.GetObjectsQuery<NIR>().Any(x => x.DocumentSursaId == fct.ID));

            // --- Re-operare + storno pe tot lanțul ---
            var conex2 = MotorOperare.Opereaza(os, fct);
            s.Check("Re-operarea FCT generează un NIR draft proaspăt", conex2 is NIR { Stare: StareDocument.Draft });
            MotorOperare.Opereaza(os, conex2);
            s.CheckRefuza("Stornarea FCT cu NIR operat → refuzată", () =>
                MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22)));
            MotorOperare.Storneaza(os, conex2, new DateOnly(2026, 7, 22));
            s.Check("Storno NIR → sold lot 0",
                CubScena.Sold(os, lot.ID, mag1.ID) is { Cantitate: 5m, Valoare: 59.5m });
            MotorOperare.Storneaza(os, fct, new DateOnly(2026, 7, 22));
            var stornoFctCub = CubScena.Note(os, fct.ID).Where(p => p.Storno && p.LinieId == linieServiciu.ID).ToList();
            s.Check("Storno FCT → nota serviciului inversată, append-only",
                fct.Stare == StareDocument.Stornat && stornoFctCub.Count == 2
                && stornoFctCub.Nota(tipServicii.ContImplicitId, cont401.ID, -100m));

            // --- Sursa de cont RepartitorPredator: ContImplicit bate fallback-ul 401 ---
            var furnizorImobilizari = os.CreateObject<Partener>();
            furnizorImobilizari.Cod = "E2E-FURN2";
            furnizorImobilizari.Denumire = "Furnizor cu cont propriu";
            furnizorImobilizari.ContImplicit = cont404;
            var fct2 = os.CreateObject<FacturaIntrare>();
            fct2.Numar = "E2E-FF2";
            fct2.Data = new DateOnly(2026, 3, 4);
            fct2.Predator = furnizorImobilizari;
            fct2.Primitor = mag1;
            var linie2 = os.CreateObject<FacturaIntrareDetaliu>();
            linie2.Document = fct2;
            linie2.TipMaterial = tipServicii;
            linie2.Cantitate = 1m;
            linie2.PretUnitar = 200m;
            // Clasificația prin ANGAJAMENT (nu cod economic): satisface și politica de
            // tip, și — prin puntea din 3d — defalcarea E a contului 404.
            var angajament = os.CreateObject<Angajament>();
            angajament.Cod = "E2E-ANG";
            angajament.Denumire = "Angajament probă e2e";
            linie2.Angajament = angajament;
            os.CommitChanges();

            // 404 poartă defalcarea BFEPR — fără Sursă de finanțare / Cod funcțional /
            // Proiect pe dimensiunile REZOLVATE, operarea se refuză (3d).
            s.CheckRefuza("404 (BFEPR): Sursă de finanțare/Cod funcțional/Proiect lipsă → refuz",
                () => MotorOperare.Opereaza(os, fct2));
            var sursaFin = os.CreateObject<SursaFinantare>();
            sursaFin.Cod = "E2E-SF";
            sursaFin.Denumire = "Sursă de finanțare probă e2e";
            var codFn = os.CreateObject<CodFunctional>();
            codFn.Cod = "E2E-CF";
            codFn.Denumire = "Cod funcțional probă e2e";
            var proiect = os.CreateObject<Proiect>();
            proiect.Cod = "E2E-PR";
            proiect.Denumire = "Proiect probă e2e";
            os.CommitChanges();
            linie2.SursaFinantareId = sursaFin.ID;
            linie2.CodFunctionalId = codFn.ID;
            linie2.ProiectId = proiect.ID;
            os.CommitChanges();
            s.Check("FCT doar cu servicii NU generează NIR", MotorOperare.Opereaza(os, fct2) == null);
            var credit404Cub = CubScena.Note(os, fct2.ID).Where(p => p.Credit).ToList();
            s.Check("Creditul vine din ContImplicit al partenerului (404, nu fallback 401)",
                credit404Cub.Count > 0 && credit404Cub.All(p => p.Cont == cont404.ID));
            s.Check("Puntea angajamentului: E pe 404 satisfăcut fără cod economic; B/F/P rezolvate pe notă",
                credit404Cub.Count > 0 && credit404Cub.All(p => p.CodEconomic == null
                    && p.SursaFinantare == sursaFin.ID
                    && p.CodFunctional == codFn.ID
                    && p.Proiect == proiect.ID));
            MotorOperare.Storneaza(os, fct2, new DateOnly(2026, 7, 22));

            CurataFct(os);
            s.Check("Curățenie finală FCT/NIR (fără reziduuri e2e)",
                !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith("E2E-FF"))
                && !os.GetObjectsQuery<Partener>().Any(p => p.Cod.StartsWith("E2E-FURN")));
        }
    }
}
