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

// ══════════════════════════════════════════════════════════════════════════
// Felia 27, pasul 5 (F27-D6): corecția peste graniță = storno legat + document
// nou, cu motiv. Scena e a anului 2034.
// ══════════════════════════════════════════════════════════════════════════
static class VerificaCorectie {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-COR";
        const int An = 2034;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipStoc = privat ? "371" : "302.01.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        void CurataCor(IObjectSpace os) {
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<DepunereDeclaratie>()
                .Where(d => d.Perioada / 100 == An).ToList());
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            var produsIds = os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID).ToList();
            var lotIds = os.GetObjectsQuery<Lot>()
                .Where(l => produsIds.Contains(l.ProdusId)).Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Corecțiile ÎNAINTEA originalelor: `CorecteazaId` e FK `Restrict`, deci
            // originalul nu se șterge cât timp corecția îl arată (F27-D6).
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID))
                    .OrderByDescending(d => d.CorecteazaId != null)
                    .ThenByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => lotIds.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => produsIds.Contains(p.ID)).ToList());
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).Select(p => p.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        // Calea REALĂ a refuzului de culegere: dispecerul din gardian, nu regula.
        string RefuzGardianCor(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        List<Atlas.Conta.BackOffice.Module.Cub.Citiri.FaptFiscal> FiscaleCub(IObjectSpace os, Guid docId) =>
            CubScena.Fapte(os, docId).OrderBy(f => f.Storno).ToList();

        using (var os = s.Provider.CreateObjectSpace())
            CurataCor(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            Console.WriteLine($"     MĂSURAT (COR-V0/{eticheta}): {perioade} perioade și {documente} documente în "
                + $"{An}, {inchise} perioade închise în bază.");
            s.Check($"COR-V0 ({eticheta}) precondiție: anul {An} e liber și nicio perioadă a bazei nu e închisă — "
                + $"altfel închiderea lui 01/{An} n-ar fi capăt de lanț, iar snapshot-ul măsurat mai jos ar fi "
                + "peste conținut străin",
                perioade == 0 && documente == 0 && inchise == 0);
        }

        // ── Scena: două NIR-uri (fiecare naște un lot) și un transfer care CONSUMĂ
        //    lotul al doilea; pe privat, în plus, faptele fiscale ale lunii ──
        Guid idGestA, idGestB, idFurnizor, idProdus, idCodEc, idTipStoc;
        Guid idNirA, idNirB, idBtr, idLotA, idLotB, idLinieNirA;
        Guid idFct1 = Guid.Empty, idFct2 = Guid.Empty, idFcl = Guid.Empty;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 1, 2, 3 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var tipStoc = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipStoc);
            var gestA = os.CreateObject<Gestiune>();
            gestA.Cod = Marcaj + "-GA";
            gestA.Denumire = "Gestiune corecție A";
            var gestB = os.CreateObject<Gestiune>();
            gestB.Cod = Marcaj + "-GB";
            gestB.Denumire = "Gestiune corecție B";
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-F";
            furnizor.Denumire = "Furnizor corecție";
            furnizor.CodFiscal = "RO33333340";
            furnizor.InregistratTva = true;
            var produs = os.CreateObject<Produs>();
            produs.Cod = Marcaj + "-P";
            produs.Denumire = "Produs corecție";
            produs.UM = "BUC";
            produs.TipMaterial = tipStoc;
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic corecție";
            os.CommitChanges();
            idGestA = gestA.ID; idGestB = gestB.ID; idFurnizor = furnizor.ID;
            idProdus = produs.ID; idCodEc = codEc.ID; idTipStoc = tipStoc.ID;
        }

        NIR CreeazaNir(IObjectSpace os, Guid gestiuneId, decimal cantitate, decimal pret,
                out Lot lot, out NirDetaliu linie) {
            var nir = os.CreateObject<NIR>();
            nir.Data = Zi(1, 12);
            nir.DataInregistrare = Zi(1, 12);
            nir.PredatorId = idFurnizor;
            nir.PrimitorId = gestiuneId;
            var l = os.CreateObject<NirDetaliu>();
            l.Document = nir;
            l.TipMaterialId = idTipStoc;
            l.Cantitate = cantitate;
            l.PretUnitar = pret;
            l.CodEconomicId = idCodEc;
            lot = l.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus), os.GetObjectByKey<Gestiune>(gestiuneId));
            linie = l;
            return nir;
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var nirA = CreeazaNir(os, idGestA, 10m, 10m, out var lotA, out var linieA);
            var nirB = CreeazaNir(os, idGestB, 20m, 5m, out var lotB, out _);
            os.CommitChanges();
            MotorOperare.Opereaza(os, nirA);
            MotorOperare.Opereaza(os, nirB);

            var btr = os.CreateObject<NotaTransfer>();
            btr.Numar = Marcaj + "-BTR";
            btr.Data = Zi(1, 20);
            btr.DataInregistrare = Zi(1, 20);
            btr.PredatorId = idGestB;
            btr.PrimitorId = idGestA;
            btr.NumarPV = Marcaj;
            var linieBtr = os.CreateObject<DocumentDetaliu>();
            linieBtr.Document = btr;
            linieBtr.TipMaterialId = idTipStoc;
            linieBtr.LotId = lotB.ID;
            linieBtr.Cantitate = 6m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, btr);

            idNirA = nirA.ID; idNirB = nirB.ID; idBtr = btr.ID;
            idLotA = lotA.ID; idLotB = lotB.ID; idLinieNirA = linieA.ID;
            s.Check($"COR — precondiție de scenă ({eticheta}): două NIR-uri operate în 01/{An} (fiecare cu lotul lui) "
                + "și un transfer care CONSUMĂ lotul al doilea — exact cele două roluri pe care regula lotului le "
                + "distinge la corecție",
                nirA.Stare == StareDocument.Operat && nirB.Stare == StareDocument.Operat
                && btr.Stare == StareDocument.Operat
                && os.GetObjectByKey<Lot>(idLotA).Data == Zi(1, 12));
        }

        if (privat)
            using (var os = s.Provider.CreateObjectSpace()) {
                var idTipCheltuiala = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628").ID;
                var idTipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704").ID;
                var idN21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21").ID;
                var idSediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU").ID;

                FacturaIntrare Fct(string sufix, int zi, decimal pret) {
                    var f = os.CreateObject<FacturaIntrare>();
                    f.Numar = Marcaj + sufix;
                    f.Data = Zi(1, zi);
                    f.DataInregistrare = Zi(1, zi);
                    f.PredatorId = idFurnizor;
                    f.PrimitorId = idGestA;
                    var l = os.CreateObject<FacturaIntrareDetaliu>();
                    l.Document = f;
                    l.TipMaterialId = idTipCheltuiala;
                    l.Cantitate = 1m;
                    l.PretUnitar = pret;
                    l.TipTvaId = idN21;
                    l.CodEconomicId = idCodEc;
                    return f;
                }

                var fct1 = Fct("-FCT1", 15, 100m);
                var fct2 = Fct("-FCT2", 16, 200m);
                var fcl = os.CreateObject<FacturaIesire>();
                fcl.Numar = Marcaj + "-FCL";
                fcl.Data = Zi(1, 17);
                fcl.DataInregistrare = Zi(1, 17);
                fcl.PredatorId = idSediu;
                fcl.PrimitorId = idFurnizor;
                var linieFcl = os.CreateObject<FacturaIesireDetaliu>();
                linieFcl.Document = fcl;
                linieFcl.TipMaterialId = idTipVenit;
                linieFcl.Cantitate = 1m;
                linieFcl.PretUnitar = 500m;
                linieFcl.TipTvaId = idN21;
                linieFcl.CodEconomicId = idCodEc;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fct1);
                MotorOperare.Opereaza(os, fct2);
                MotorOperare.Opereaza(os, fcl);
                idFct1 = fct1.ID; idFct2 = fct2.ID; idFcl = fcl.ID;

                var r1Cub = FiscaleCub(os, fct1.ID);
                s.Check($"COR — precondiție fiscală ({eticheta}): faptele lui ianuarie se declară în 01/{An} "
                    + "(perioada lor, încă deschisă) — reperul contra căruia se citește efectul motivului",
                    r1Cub.Count == 1 && r1Cub[0].PerioadaAn == An && r1Cub[0].PerioadaLuna == 1
                    && FiscaleCub(os, fcl.ID)[0].PerioadaLuna == 1);
            }

        // ── Închiderea lui ianuarie: granița peste care corecția devine singura cale ──
        decimal snapshotDebitInitial, snapshotCreditInitial, snapshotCantitateInitiala;
        int snapshotRanduriInitial;
        using (var os = s.Provider.CreateObjectSpace()) {
            s.InchideAcceptTot(os, An, 1, Marcaj);
            var p = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 1);
            snapshotRanduriInitial = os.GetObjectsQuery<SoldPerioadaContabil>().Count(s => s.An == An && s.Luna == 1);
            snapshotDebitInitial = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Debit) ?? 0m;
            snapshotCreditInitial = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Credit) ?? 0m;
            snapshotCantitateInitiala = os.GetObjectsQuery<SoldPerioadaStoc>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Cantitate) ?? 0m;
            s.Check($"COR — precondiție de scenă ({eticheta}): 01/{An} se închide (capăt de lanț) și își "
                + "materializează soldurile — corecția de mai jos NU are voie să le atingă",
                p.Inchisa && p.InchisaPrimaOara != null && snapshotRanduriInitial > 0);
        }

        if (privat)
            using (var os = s.Provider.CreateObjectSpace())
                FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 1,
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Versiune(os, FormularFiscal.D394, new(An, 1, 1), new(An, 1, 31)), Marcaj);

        // ── COR-V1/V2: refuzurile comenzii ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.CreateObject<NotaTransfer>();
            draft.Numar = Marcaj + "-DRAFT";
            draft.Data = Zi(2, 3);
            draft.DataInregistrare = Zi(2, 3);
            draft.PredatorId = idGestA;
            draft.PrimitorId = idGestB;
            draft.NumarPV = Marcaj;
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = draft;
            linie.TipMaterialId = idTipStoc;
            linie.LotId = idLotA;
            linie.Cantitate = 1m;
            os.CommitChanges();
            var refuzDraft = s.Refuz(() => CorectieService.Corecteaza(os, draft.ID, Zi(2, 10), MotivCorectie.FaptNou));
            Console.WriteLine($"     MĂSURAT (COR-V1/{eticheta}): corecția unui DRAFT a ieșit cu "
                + $"„{s.PrimaLinie(refuzDraft)}”.");
            s.Check($"COR-V1 ({eticheta}) un document care NU e operat nu se corectează: corecția e storno + "
                + "document nou, iar un draft n-are ce storna — se editează",
                refuzDraft != null && refuzDraft.Contains("Se corectează doar un document operat."));

            var refuzPerioada = s.Refuz(() => CorectieService.Corecteaza(os, idNirA, Zi(1, 25), MotivCorectie.FaptNou));
            Console.WriteLine($"     MĂSURAT (COR-V2/{eticheta}): corecția cu dată în perioada închisă a ieșit cu "
                + $"„{s.PrimaLinie(refuzPerioada)}”.");
            s.Check($"COR-V2 ({eticheta}) data corecției cade sub gardianul de perioadă al STORNĂRII, nemodificat: "
                + $"nu se poate corecta ÎN 01/{An}, fiindcă acolo nu se mai scrie nimic",
                refuzPerioada != null && refuzPerioada.Contains($"01/{An}"));

            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == draft.ID).ToList());
            pj.Adauga(draft);
            pj.Executa();
        }

        // ── COR-V3/V4: comanda pe NIR-ul care a NĂSCUT lotul ──
        Guid idCorectieNir;
        using (var os = s.Provider.CreateObjectSpace()) {
            var (stornat, corectie) = CorectieService.Corecteaza(os, idNirA, Zi(2, 10), MotivCorectie.FaptNou);
            idCorectieNir = corectie.ID;
            var original = os.GetObjectByKey<NIR>(idNirA);
            var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == corectie.ID).ToList();
            var linieOriginala = os.GetObjectByKey<NirDetaliu>(idLinieNirA);
            var copie = linii[0] as NirDetaliu;
            var stocCub = CubScena.Stoc(os, idNirA);
            var noteCub = CubScena.Note(os, idNirA);
            Console.WriteLine($"     MĂSURAT (COR-V3/{eticheta}): original „{original.Numar}” {original.Stare}, "
                + $"{stocCub.Count(p => p.Storno)} mișcări de stoc inverse la "
                + $"{s.Ziua(stocCub.Where(p => p.Storno).Select(p => p.Data).FirstOrDefault())}, "
                + $"{noteCub.Count(p => p.Storno)} postări contabile inverse; corecția „{corectie.Numar}” {corectie.Stare} cu "
                + $"{linii.Count} linii, dată {s.Ziua(corectie.Data)}, înregistrare {s.Ziua(corectie.DataInregistrare)}.");
            s.Check($"COR-V3 ({eticheta}) comanda stornează originalul la data corecției — rândurile inverse de stoc "
                + $"și contabile cad la {Zi(2, 10):dd.MM.yyyy}, iar documentul trece în `Stornat`: e chiar "
                + "`MotorOperare.Storneaza`, neatins",
                original.Stare == StareDocument.Stornat && stornat.ID == idNirA
                && stocCub.Count(p => p.Storno) == stocCub.Count(p => !p.Storno)
                && stocCub.Where(p => p.Storno).All(p => p.Data == Zi(2, 10))
                && noteCub.Count(p => p.Storno) == noteCub.Count(p => !p.Storno)
                && noteCub.Where(p => p.Storno).All(p => p.Data == Zi(2, 10)));
            s.Check($"COR-V4 ({eticheta}) documentul nou e de ACELAȘI tip concret, cu `Numar` și `Data` ale "
                + "documentului fizic PĂSTRATE (seria nu se consumă din nou), data înregistrării = data corecției, "
                + "legătura și motivul scrise de motor, starea Draft; culegerea e copiată integral prin metadata EF",
                MotorOperare.ClasaReala(corectie) == typeof(NIR)
                && corectie.Numar == original.Numar && corectie.Data == original.Data
                && corectie.DataInregistrare == Zi(2, 10)
                && corectie.CorecteazaId == idNirA && corectie.MotivCorectie == MotivCorectie.FaptNou
                && corectie.Stare == StareDocument.Draft
                && corectie.PredatorId == original.PredatorId && corectie.PrimitorId == original.PrimitorId
                && linii.Count == 1 && copie != null
                && copie.TipMaterialId == linieOriginala.TipMaterialId
                && copie.Cantitate == linieOriginala.Cantitate && copie.PretUnitar == linieOriginala.PretUnitar
                && copie.CodEconomicId == linieOriginala.CodEconomicId
                && copie.ID != linieOriginala.ID);

            var lotNou = os.GetObjectsQuery<Lot>().FirstOrDefault(l => l.LinieIntrareId == copie.ID);
            Console.WriteLine($"     MĂSURAT (COR-V5/{eticheta}): lot nou {(lotNou == null ? "<lipsă>" : lotNou.ID.ToString())}, "
                + $"dată {(lotNou == null ? "-" : s.Ziua(lotNou.Data))}, preț {lotNou?.PretUnitar}.");
            s.Check($"COR-V5 ({eticheta}) linia care a NĂSCUT lotul primește unul PROPRIU, nou și nefinalizat "
                + "(preț 0, dată goală — motorul îl finalizează la operare): marfa recepționată o dată nu se "
                + "recepționează a doua oară pe același lot, iar lotul vechi rămâne cu istoria lui",
                lotNou != null && lotNou.ID != idLotA && lotNou.ProdusId == idProdus
                && lotNou.GestiuneId == idGestA && lotNou.Data == default && lotNou.PretUnitar == 0m
                && copie.LotId == lotNou.ID);
        }

        // ── COR-V6: operarea corecției (cu o linie schimbată — corecția reală) ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var corectie = os.GetObjectByKey<NIR>(idCorectieNir);
            var linie = os.GetObjectsQuery<DocumentDetaliu>().First(d => d.DocumentId == idCorectieNir) as NirDetaliu;
            linie.Cantitate = 12m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, corectie);
            var lotNou = os.GetObjectsQuery<Lot>().First(l => l.LinieIntrareId == linie.ID);
            var stocCub = CubScena.Stoc(os, idCorectieNir);
            Console.WriteLine($"     MĂSURAT (COR-V6/{eticheta}): {stocCub.Count} mișcări de stoc la "
                + $"{s.Ziua(stocCub.Select(p => p.Data).FirstOrDefault())}, lot nou la {s.Ziua(lotNou.Data)} cu preț "
                + $"{lotNou.PretUnitar}; sold lot vechi {CubScena.Chei(os, [idLotA]).Sum(c => c.Cantitate)}, "
                + $"sold lot nou {CubScena.Chei(os, [lotNou.ID]).Sum(c => c.Cantitate)}.");
            s.Check($"COR-V6 ({eticheta}) draftul corectat se operează NORMAL — registrele cad la data înregistrării "
                + $"({Zi(2, 10):dd.MM.yyyy}), lotul nou se naște acolo și primește prețul din linia CORECTATĂ, iar "
                + "lotul vechi rămâne cu sold zero prin rândurile de storno: adevărul corectat se vede din prima "
                + "perioadă deschisă încolo",
                corectie.Stare == StareDocument.Operat
                && stocCub.Count > 0 && stocCub.All(p => p.Data == Zi(2, 10))
                && lotNou.Data == Zi(2, 10) && lotNou.PretUnitar == 10m
                && CubScena.Chei(os, [idLotA]).Sum(c => c.Cantitate) == 0m
                && CubScena.Chei(os, [lotNou.ID]).Sum(c => c.Cantitate) == 12m);
        }

        // ── COR-V7: linia care CONSUMĂ un lot îl păstrează ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var (_, corectie) = CorectieService.Corecteaza(os, idBtr, Zi(2, 11), MotivCorectie.FaptNou);
            var copie = os.GetObjectsQuery<DocumentDetaliu>().First(d => d.DocumentId == corectie.ID);
            var loturiProprii = os.GetObjectsQuery<Lot>().Count(l => l.LinieIntrareId == copie.ID);
            Console.WriteLine($"     MĂSURAT (COR-V7/{eticheta}): linia copiată are lot {copie.LotId}, "
                + $"{loturiProprii} loturi proprii.");
            s.Check($"COR-V7 ({eticheta}) linia care doar CONSUMĂ un lot îl PĂSTREAZĂ (FK copiat ca oricare altul) "
                + "și nu naște nimic — regula lotului e inversa celei prin care motorul recunoaște nașterea "
                + "(`Lot.LinieIntrareId == linie.ID`), nu un `is` pe tipul liniei",
                copie.LotId == idLotB && loturiProprii == 0);
        }

        // ── COR-V8/V9: legătura e 1:1 și e a motorului ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuzAlDoilea = s.Refuz(() => CorectieService.Corecteaza(os, idNirA, Zi(2, 12), MotivCorectie.FaptNou));
            Console.WriteLine($"     MĂSURAT (COR-V8/{eticheta}): a doua corecție a aceluiași original a ieșit cu "
                + $"„{s.PrimaLinie(refuzAlDoilea)}”.");
            s.Check($"COR-V8 ({eticheta}) un original nu se corectează de două ori: comanda refuză înainte de orice "
                + "scriere, NUMIND documentul care îl corectează deja — refuzul specific bate refuzul generic de "
                + "stare, deși originalul corectat e oricum `Stornat`",
                refuzAlDoilea != null && refuzAlDoilea.Contains("deja corectat")
                && refuzAlDoilea.Contains("legătura de corecție e 1:1"));
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.CreateObject<NotaTransfer>();
            draft.Numar = Marcaj + "-MANUAL";
            draft.Data = Zi(2, 15);
            draft.DataInregistrare = Zi(2, 15);
            draft.PredatorId = idGestA;
            draft.PrimitorId = idGestB;
            draft.NumarPV = Marcaj;
            draft.CorecteazaId = idNirB;
            draft.MotivCorectie = MotivCorectie.EroareMateriala;
            var refuz = RefuzGardianCor(os);
            Console.WriteLine($"     MĂSURAT (COR-V9/{eticheta}): draftul cu legătură scrisă de mână a ieșit cu "
                + $"„{s.PrimaLinie(refuz)}”.");
            s.Check($"COR-V9 ({eticheta}) legătura de corecție scrisă pe ușa securizată e refuzată la commit, pe "
                + "calea reală a gardianului — e a MOTORULUI, ca `Stare` și grupul conex",
                refuz != null && refuz.Contains("Legătura de corecție o scrie doar motorul"));
            s.Check($"COR-V10 ({eticheta}) ACELAȘI commit raportează și invariantul rupt: originalul arătat e OPERAT, "
                + "iar corecția cere un original STORNAT — regula se verifică oriunde se scrie legătura, nu doar "
                + "pe ușa pe care comanda n-o folosește",
                refuz != null && refuz.Contains("nu e stornat"));
            // Al doilea draft spre un original DEJA corectat: 1:1 la commit.
            draft.CorecteazaId = idNirA;
            var refuzUnicitate = RefuzGardianCor(os);
            Console.WriteLine($"     MĂSURAT (COR-V11/{eticheta}): al doilea document spre același original a ieșit "
                + $"cu „{s.PrimaLinie(refuzUnicitate)}”.");
            s.Check($"COR-V11 ({eticheta}) două documente nu pot corecta același original: invariantul 1:1 se vede "
                + "la commit, nu doar în comandă",
                refuzUnicitate != null && refuzUnicitate.Contains("legătura de corecție e 1:1"));
        }

        // ── COR-V12: snapshot-ul perioadei închise, NEATINS ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var randuri = os.GetObjectsQuery<SoldPerioadaContabil>().Count(s => s.An == An && s.Luna == 1);
            var debit = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Debit) ?? 0m;
            var credit = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Credit) ?? 0m;
            var cantitate = os.GetObjectsQuery<SoldPerioadaStoc>()
                .Where(s => s.An == An && s.Luna == 1).Sum(s => (decimal?)s.Cantitate) ?? 0m;
            var raport = SolduriService.Reconstruieste(os);
            var rand = raport.Referinte.FirstOrDefault(r => r.An == An && r.Luna == 1);
            Console.WriteLine($"     MĂSURAT (COR-V12/{eticheta}): snapshot 01/{An} — {randuri} rânduri contabile "
                + $"(erau {snapshotRanduriInitial}), debit {debit} (era {snapshotDebitInitial}), cantitate "
                + $"{cantitate} (era {snapshotCantitateInitiala}); reconstrucția raportează "
                + $"{rand?.ContabilDiferite} diferențe contabile și {rand?.StocDiferite} de stoc.");
            s.Check($"COR-V12 ({eticheta}) corecția NU atinge perioada închisă: snapshot-ul lui 01/{An} e identic "
                + "cu cel de la închidere, iar reconstrucția integrală din registre raportează zero diferențe — "
                + "storno-ul și documentul nou trăiesc în fereastra DESCHISĂ",
                randuri == snapshotRanduriInitial && debit == snapshotDebitInitial
                && credit == snapshotCreditInitial && cantitate == snapshotCantitateInitiala
                && rand != null && rand.ContabilDiferite == 0 && rand.StocDiferite == 0);
        }

        // ── COR-V13: ce a fost scris de corecție se vede în februarie ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var noteOriginalCub = CubScena.Note(os, idNirA);
            var noteCorectieCub = CubScena.Note(os, idCorectieNir);
            Console.WriteLine($"     MĂSURAT (COR-V13/{eticheta}): debitele originalului însumează "
                + $"{noteOriginalCub.Where(p => p.Debit).Sum(p => p.Valoare)}, ale corecției "
                + $"{noteCorectieCub.Where(p => p.Debit).Sum(p => p.Valoare)}.");
            s.Check($"COR-V13 ({eticheta}) suma algebrică a notelor originalului e ZERO (operarea + storno-ul se "
                + "anulează), iar corecția aduce valoarea corectată (12 × 10) — perioada deschisă arată adevărul "
                + "corectat, cea închisă rămâne cum a fost declarată",
                noteOriginalCub.Count > 0
                && noteOriginalCub.Where(p => p.Debit).Sum(p => p.Valoare) == 0m
                && noteOriginalCub.Where(p => p.Credit).Sum(p => p.Valoare) == 0m
                && noteCorectieCub.Where(p => p.Debit).Sum(p => p.Valoare) == 120m
                && noteCorectieCub.Where(p => p.Credit).Sum(p => p.Valoare) == 120m);
        }

        if (privat) {
            // ── COR-V14…V16: eroarea materială ⇒ perioada ORIGINALULUI ──
            Guid idCorectieFct;
            using (var os = s.Provider.CreateObjectSpace()) {
                var perioadaOriginal = FiscaleCub(os, idFct1).First(f => !f.Storno);
                var (_, corectie) = CorectieService.Corecteaza(os, idFct1, Zi(2, 10), MotivCorectie.EroareMateriala);
                idCorectieFct = corectie.ID;
                var storno = FiscaleCub(os, idFct1).Where(f => f.Storno).ToList();
                Console.WriteLine($"     MĂSURAT (COR-V14/{eticheta}): {storno.Count} fapte fiscale inverse, "
                    + $"`Data` {s.Ziua(storno[0].Data)}, perioada {storno[0].PerioadaLuna:00}/{storno[0].PerioadaAn} "
                    + $"(originalul: {perioadaOriginal.PerioadaLuna:00}/{perioadaOriginal.PerioadaAn}).");
                var stornoCub = ProbeCub.Postari(os, idFct1, N.FelTranzactie.Storno)
                    .Where(p => p.PerioadaDeclarare != null).ToList();
                s.Check($"STR-CORECTIE ({eticheta}): la EROARE MATERIALĂ postările `Storno` din CUB poartă "
                    + $"`PerioadaDeclarare` = {An}01 (a originalului), ca rândurile `RegistruTva` — altfel "
                    + "orice jurnal citit din cub ar pune stornoul în luna corecției",
                    stornoCub.Count > 0 && stornoCub.All(p => p.PerioadaDeclarare == (An * 100) + 1));
                var inverseFiscaleFct1 = ProbeCub.Postari(os, idFct1, N.FelTranzactie.Storno)
                    .Where(p => p.TipTvaId != null).ToList();
                s.Check($"D9-A5 (a) ({eticheta}): la EROARE MATERIALĂ postările `Storno` cu `TipTvaId` se nasc finale, "
                    + $"cu `InversaTehnica` și cu perioada de declarare a originalului ({An}01)",
                    inverseFiscaleFct1.Count > 0
                    && inverseFiscaleFct1.All(p => p.InversaTehnica && p.PerioadaDeclarare == (An * 100) + 1));
                var inverseFct1Cub = FiscaleCub(os, idFct1).Where(f => f.Storno).ToList();
                s.Check($"COR-V14 ({eticheta}) la EROARE MATERIALĂ rândurile inverse păstrează data faptului "
                    + $"stornării ({Zi(2, 10):dd.MM.yyyy}) dar se DECLARĂ în perioada originalului (01/{An}) — "
                    + "excepția cu motiv de la JT-D5, scrisă de comandă, nu de `Storneaza`",
                    inverseFct1Cub.Count == 1 && inverseFct1Cub[0].Data == Zi(2, 10)
                    && inverseFct1Cub[0].PerioadaAn == An && inverseFct1Cub[0].PerioadaLuna == 1
                    && inverseFct1Cub[0].Baza == -100m);
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                var linie = os.GetObjectsQuery<DocumentDetaliu>().First(d => d.DocumentId == idCorectieFct)
                    as FacturaIntrareDetaliu;
                linie.PretUnitar = 150m;
                os.CommitChanges();
                var corectie = os.GetObjectByKey<FacturaIntrare>(idCorectieFct);
                MotorOperare.Opereaza(os, corectie);
                var randuriCub = FiscaleCub(os, idCorectieFct);
                Console.WriteLine($"     MĂSURAT (COR-V15/{eticheta}): {randuriCub.Count} fapte, `Data` "
                    + $"{s.Ziua(randuriCub[0].DataJurnal())}, perioada {randuriCub[0].PerioadaLuna:00}/{randuriCub[0].PerioadaAn}, "
                    + $"bază {randuriCub[0].Baza}.");
                s.Check($"COR-V15 ({eticheta}) rândurile documentului NOU păstrează data faptului fiscal "
                    + $"({Zi(1, 15):dd.MM.yyyy}, a documentului fizic) și se declară tot în 01/{An}: la eroare "
                    + "materială diferența aparține perioadei originale, deci apare ca rectificativă acolo",
                    randuriCub.Count == 1 && randuriCub[0].DataJurnal() == Zi(1, 15)
                    && randuriCub[0].PerioadaAn == An && randuriCub[0].PerioadaLuna == 1
                    && randuriCub[0].Baza == 150m);

                var rect = TvaProiectii.Rectificativa(os, An, 1);
                var idsRect = rect.Randuri.Select(r => r.DocumentId).Distinct().OrderBy(x => x).ToList();
                Console.WriteLine($"     MĂSURAT (COR-V16/{eticheta}): rectificativa lui 01/{An} — "
                    + $"{rect.Randuri.Count} rânduri pe {idsRect.Count} documente, bază "
                    + $"{rect.Agregat.Sum(a => a.Baza)}, TVA {rect.Agregat.Sum(a => a.Tva)}.");
                s.Check($"COR-V16 ({eticheta}) conținutul de rectificativă al lui 01/{An} e EXACT perechea corecției: "
                    + "rândul invers al originalului (−100) plus rândul documentului nou (+150), adică diferența "
                    + "de 50 pe care o declarăm. Nu există flag — e consecința celor două timestamp-uri",
                    rect.EsteRectificativa && rect.Randuri.Count == 2
                    && idsRect.Count == 2 && idsRect.Contains(idFct1) && idsRect.Contains(idCorectieFct)
                    && rect.Randuri.Sum(r => r.Baza) == 50m
                    && rect.Agregat.Sum(a => a.Baza) == 50m);
            }

            // ── COR-V17/V18: faptul nou ⇒ regula normală D5, pe ambele direcții ──
            using (var os = s.Provider.CreateObjectSpace()) {
                var (_, corectie) = CorectieService.Corecteaza(os, idFct2, Zi(2, 12), MotivCorectie.FaptNou);
                var storno = FiscaleCub(os, idFct2).Where(f => f.Storno).ToList();
                MotorOperare.Opereaza(os, corectie);
                var randuri = FiscaleCub(os, corectie.ID);
                Console.WriteLine($"     MĂSURAT (COR-V17/{eticheta}): storno perioada "
                    + $"{storno[0].PerioadaLuna:00}/{storno[0].PerioadaAn}, document nou perioada "
                    + $"{randuri[0].PerioadaLuna:00}/{randuri[0].PerioadaAn}, `Data` {s.Ziua(randuri[0].DataJurnal())}.");
                var stornoCubFaptNou = ProbeCub.Postari(os, idFct2, N.FelTranzactie.Storno)
                    .Where(p => p.PerioadaDeclarare != null).ToList();
                s.Check($"STR-CORECTIE ({eticheta}): la FAPT NOU postările `Storno` din CUB rămân în perioada "
                    + $"STORNĂRII ({An}02) — re-ștampilarea e a motivului, nu a stornoului",
                    stornoCubFaptNou.Count > 0
                    && stornoCubFaptNou.All(p => p.PerioadaDeclarare == (An * 100) + 2));
                var inverseFct2Cub = FiscaleCub(os, idFct2).Where(f => f.Storno).ToList();
                var nouFct2Cub = FiscaleCub(os, corectie.ID);
                s.Check($"COR-V17 ({eticheta}) la FAPT NOU nimic nu se mută: rândul invers se declară în perioada "
                    + $"STORNĂRII (02/{An}, JT-D5), iar documentul nou cade pe regula normală a politicii — "
                    + $"deductibilul e `PerioadaInregistrarii`, deci 02/{An}. Perioada închisă rămâne neatinsă "
                    + "fiscal",
                    inverseFct2Cub.Count == 1 && inverseFct2Cub[0].PerioadaAn == An && inverseFct2Cub[0].PerioadaLuna == 2
                    && nouFct2Cub.Count == 1 && nouFct2Cub[0].DataJurnal() == Zi(2, 12)
                    && nouFct2Cub[0].PerioadaAn == An && nouFct2Cub[0].PerioadaLuna == 2);
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                var (_, corectie) = CorectieService.Corecteaza(os, idFcl, Zi(2, 13), MotivCorectie.FaptNou);
                MotorOperare.Opereaza(os, corectie);
                var randuriCub = FiscaleCub(os, corectie.ID);
                Console.WriteLine($"     MĂSURAT (COR-V18/{eticheta}): documentul nou al FCL are perioada "
                    + $"{randuriCub[0].PerioadaLuna:00}/{randuriCub[0].PerioadaAn}, `Data` {s.Ziua(randuriCub[0].DataJurnal())}.");
                s.Check($"COR-V18 ({eticheta}) FAPT NOU pe factura de ieșire propune data evenimentului nou, "
                    + "iar D300/D394 folosesc propria perioadă",
                    randuriCub.Count == 1 && randuriCub[0].DataJurnal() == Zi(2, 13)
                    && randuriCub[0].PerioadaAn == An && randuriCub[0].PerioadaLuna == 2
                    && randuriCub[0].PerioadaD394 == An * 100 + 2);
            }
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var postare = os.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>().First(p => p.DocumentId == idNirA);
            var idPostare = postare.ID;
            var valoareInainte = postare.Valoare;
            postare.Valoare += 1m;
            string refuzModificare = null;
            try { os.CommitChanges(); }
            catch (Exception e) { refuzModificare = e.ToString(); }
            decimal valoareDupa;
            using (var proaspat = s.Provider.CreateObjectSpace())
                valoareDupa = proaspat.GetObjectsQuery<Atlas.Conta.BackOffice.Module.Cub.Postare>()
                    .Where(p => p.ID == idPostare).Select(p => p.Valoare).Single();
            Console.WriteLine($"     MĂSURAT (D9-A5 b/{eticheta}): commit-ul postării modificate a ieșit cu "
                + $"„{s.PrimaLinie(refuzModificare)}”; valoarea în bază {valoareDupa} (înainte {valoareInainte}).");
            s.Check($"D9-A5 (b) ({eticheta}): o `Postare` existentă modificată pe un ObjectSpace de sistem e refuzată la "
                + "`CommitChanges` (`POSTARE_MODIFICATA`), iar baza rămâne neatinsă",
                refuzModificare != null && refuzModificare.Contains("POSTARE_MODIFICATA") && valoareDupa == valoareInainte);
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataCor(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var loturi = os.GetObjectsQuery<Lot>()
                .Count(l => l.Produs.Cod.StartsWith(Marcaj));
            s.Check($"COR-V19 ({eticheta}) fără reziduu: nicio perioadă {An}, niciun document și niciun lot rămase — "
                + "scena e re-rulabilă identic, inclusiv prin FK-ul `Restrict` al legăturii de corecție",
                perioade == 0 && documente == 0 && loturi == 0);
        }
    }
}
