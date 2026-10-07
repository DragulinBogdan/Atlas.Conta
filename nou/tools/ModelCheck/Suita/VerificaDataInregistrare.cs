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

// Felia 27, pasul 3 (F27-D4): data înregistrării. Scena stă în 2032, după
// scena soldurilor (2031–2032), a cărei purjă mătură și anul ăsta.
static class VerificaDataInregistrare {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-DIR";
        const int An = 2032;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipStoc = privat ? "371" : "302.01.00";
        var codTipF = privat ? "214" : "214.00.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        void CurataDir(IObjectSpace os) {
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var pj = new Purja(os);
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            var produsIds = os.GetObjectsQuery<Produs>()
                .Where(p => p.Cod.StartsWith(Marcaj)).Select(p => p.ID).ToList();
            var lotIds = os.GetObjectsQuery<Lot>()
                .Where(l => produsIds.Contains(l.ProdusId)).Select(l => l.ID).ToList();
            var fiseIds = os.GetObjectsQuery<Imobilizare>()
                .Where(f => f.NumarInventar.StartsWith(Marcaj)).Select(f => f.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruImobilizari>()
                .Where(r => fiseIds.Contains(r.ImobilizareId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID)).OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => lotIds.Contains(l.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(p => produsIds.Contains(p.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imobilizare>()
                .Where(f => fiseIds.Contains(f.ID)).ToList());
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
        string RefuzGardianDir(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        List<PostareScena> StocDocCub(IObjectSpace os, Guid docId) => CubScena.Stoc(os, docId);
        List<PostareScena> NoteDocCub(IObjectSpace os, Guid docId) => CubScena.Note(os, docId);

        using (var os = s.Provider.CreateObjectSpace())
            CurataDir(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            Console.WriteLine($"     MĂSURAT (DIR-V0/{eticheta}): {perioade} perioade și {documente} documente în "
                + $"{An}, {inchise} perioade închise în bază.");
            s.Check($"DIR-V0 ({eticheta}) precondiție: anul {An} e liber și nicio perioadă a bazei nu e închisă — "
                + $"altfel închiderea lui 01/{An} de mai jos n-ar fi capăt de lanț, iar registrele documentului "
                + "întârziat ar fi măsurate peste conținut străin",
                perioade == 0 && documente == 0 && inchise == 0);
        }

        Guid idGestA, idGestB, idUnitate, idFurnizor, idProdus, idCodEc, idTipStoc, idTipF, idFisa;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 1, 2, 3, 4 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var tipStoc = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipStoc);
            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            var gestA = os.CreateObject<Gestiune>();
            gestA.Cod = Marcaj + "-GA";
            gestA.Denumire = "Gestiune dată înregistrare A";
            var gestB = os.CreateObject<Gestiune>();
            gestB.Cod = Marcaj + "-GB";
            gestB.Denumire = "Gestiune dată înregistrare B";
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-UI";
            unitate.Denumire = "Unitate dată înregistrare";
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-F";
            furnizor.Denumire = "Furnizor dată înregistrare";
            furnizor.CodFiscal = "RO33333338";
            furnizor.InregistratTva = true;
            var produs = os.CreateObject<Produs>();
            produs.Cod = Marcaj + "-P";
            produs.Denumire = "Produs dată înregistrare";
            produs.UM = "BUC";
            produs.TipMaterial = tipStoc;
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic dată înregistrare";
            var fisa = os.CreateObject<Imobilizare>();
            fisa.NumarInventar = Marcaj + "-FISA";
            fisa.Denumire = "Fișă dată înregistrare";
            fisa.TipMaterialId = tipF.ID;
            fisa.LocId = gestA.ID;
            os.CommitChanges();
            idGestA = gestA.ID; idGestB = gestB.ID; idUnitate = unitate.ID; idFurnizor = furnizor.ID;
            idProdus = produs.ID; idCodEc = codEc.ID; idTipStoc = tipStoc.ID; idTipF = tipF.ID; idFisa = fisa.ID;

            s.InchideAcceptTot(os, An, 1, Marcaj);
            s.Check($"DIR — precondiție de scenă ({eticheta}): 01/{An} se închide (capăt de lanț prin absența lui "
                + $"12/{An - 1}), deci tot ce urmează probează granița, nu o perioadă oarecare",
                os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == 1).Inchisa);
        }

        // ── DIR-V1…V4: documentul întârziat ──
        Guid idNirTarziu, idLotA;
        using (var os = s.Provider.CreateObjectSpace()) {
            var nir = os.CreateObject<NIR>();
            nir.Data = Zi(1, 20);
            nir.PredatorId = idFurnizor;
            nir.PrimitorId = idGestA;
            var linie = os.CreateObject<NirDetaliu>();
            linie.Document = nir;
            linie.TipMaterialId = idTipStoc;
            linie.Cantitate = 10m;
            linie.PretUnitar = 10m;
            linie.CodEconomicId = idCodEc;
            var lot = linie.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus),
                os.GetObjectByKey<Gestiune>(idGestA));
            os.CommitChanges();
            idNirTarziu = nir.ID;
            idLotA = lot.ID;

            s.Check($"DIR-V1a ({eticheta}) documentul cules fără data înregistrării o are `default` — implicitul NU "
                + "stă în setter (proxy-urile EF și materializarea l-ar face nesigur), ci la seam-uri",
                nir.DataInregistrare == default);

            var refuzPerioada = s.Refuz(() => MotorOperare.Opereaza(os, nir));
            Console.WriteLine($"     MĂSURAT (DIR-V1/{eticheta}): operarea cu data înregistrării implicită a ieșit "
                + $"cu „{s.PrimaLinie(refuzPerioada)}”.");
            s.Check($"DIR-V1 ({eticheta}) cu data înregistrării IMPLICITĂ (normalizată la {Zi(1, 20):dd.MM.yyyy}), "
                + $"operarea cade pe gardianul perioadei 01/{An} — implicitul „= data documentului” păstrează exact "
                + "comportamentul de dinaintea feliei",
                refuzPerioada != null && refuzPerioada.Contains($"01/{An}"));

            nir.DataInregistrare = Zi(1, 15);
            var refuzGardian = RefuzGardianDir(os);
            s.Check($"DIR-V2 ({eticheta}) data înregistrării ÎNAINTEA datei documentului e refuzată la commit, pe "
                + "calea reală a gardianului de Committing: evidența nu poate primi un document înainte ca el să "
                + "existe",
                refuzGardian != null
                && refuzGardian.Contains("Data înregistrării nu poate preceda data documentului."));

            var refuzMotor = s.Refuz(() => MotorOperare.Opereaza(os, nir));
            s.Check($"DIR-V3 ({eticheta}) același refuz vine și din MOTOR, cu text identic — căile standalone "
                + "(Import1C, Migrare, ModelCheck) nu trec prin gardianul de Committing, deci regula are nevoie de "
                + "ambele uși",
                refuzMotor != null
                && refuzMotor.Contains("Data înregistrării nu poate preceda data documentului."));

            nir.DataInregistrare = Zi(2, 5);
            MotorOperare.Opereaza(os, nir);
            var lotDupa = os.GetObjectByKey<Lot>(idLotA);
            var stocCub = StocDocCub(os, nir.ID);
            var noteCub = NoteDocCub(os, nir.ID);
            Console.WriteLine($"     MĂSURAT (DIR-V4/{eticheta}): {stocCub.Count} mișcări de stoc la "
                + $"{s.Ziua(stocCub.Select(r => r.Data).FirstOrDefault())}, {noteCub.Count} postări contabile la "
                + $"{s.Ziua(noteCub.Select(r => r.Data).FirstOrDefault())}, lot la {lotDupa.Data:dd.MM.yyyy}, "
                + $"număr „{nir.Numar}”.");
            s.Check($"DIR-V4 ({eticheta}) documentul cu `Data` {Zi(1, 20):dd.MM.yyyy} într-o perioadă ÎNCHISĂ și data "
                + $"înregistrării {Zi(2, 5):dd.MM.yyyy} în cea deschisă SE OPEREAZĂ, iar registrele contabil și de "
                + "stoc ȘI lotul se nasc la data înregistrării — documentul întârziat e flux normal, nu excepție; "
                + "numărul rămâne al seriei tipului, consumat la materializare",
                nir.Stare == StareDocument.Operat
                && stocCub.Count > 0 && stocCub.All(p => p.Data == Zi(2, 5))
                && noteCub.Count > 0 && noteCub.All(p => p.Data == Zi(2, 5))
                && lotDupa.Data == Zi(2, 5)
                && !string.IsNullOrWhiteSpace(nir.Numar) && nir.Numar.StartsWith("NIR-"));
        }

        // ── DIR-V5: faptul fiscal rămâne pe data documentului; conexul moștenește ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + "-FCT";
            fct.Data = Zi(1, 22);
            fct.DataInregistrare = Zi(2, 6);
            fct.PredatorId = idFurnizor;
            fct.PrimitorId = idGestA;
            var linie = os.CreateObject<FacturaIntrareDetaliu>();
            linie.Document = fct;
            linie.TipMaterialId = idTipStoc;
            linie.Cantitate = 4m;
            linie.PretUnitar = 25m;
            linie.CodEconomicId = idCodEc;
            if (privat)
                linie.TipTva = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            linie.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus), os.GetObjectByKey<Gestiune>(idGestA));
            os.CommitChanges();

            var conex = MotorOperare.Opereaza(os, fct);
            var tvaCub = CubScena.Fapte(os, fct.ID);
            var noteCub = NoteDocCub(os, fct.ID);
            var stocCub = StocDocCub(os, fct.ID);
            Console.WriteLine($"     MĂSURAT (DIR-V5/{eticheta}): {noteCub.Count} postări contabile la "
                + $"{s.Ziua(noteCub.Select(r => r.Data).FirstOrDefault())}, {stocCub.Count} mișcări de stoc la "
                + $"{s.Ziua(stocCub.Select(r => r.Data).FirstOrDefault())}, {tvaCub.Count} fapte de TVA la "
                + $"{s.Ziua(tvaCub.Select(r => r.DataDocument).FirstOrDefault())}; conex = "
                + $"{conex?.GetType().Name ?? "<niciunul>"} cu {s.Ziua(conex?.Data ?? default)} / "
                + $"{s.Ziua(conex?.DataInregistrare ?? default)}.");
            s.Check($"DIR-V5 ({eticheta}) pe factura întârziată tot ce scrie motorul în registrele cu SOLD cade la "
                + $"data înregistrării, dar `RegistruTva.Data` RĂMÂNE data faptului fiscal "
                + $"({Zi(1, 22):dd.MM.yyyy}) — jurnalele sunt pe data facturii (F27-D5; perioada de declarare vine "
                + "la pasul 4). Conexul moștenește AMBELE date ale sursei",
                noteCub.All(p => p.Data == Zi(2, 6)) && stocCub.All(p => p.Data == Zi(2, 6))
                && tvaCub.All(f => f.DataDocument == Zi(1, 22))
                && (!privat || (noteCub.Count > 0 && tvaCub.Count > 0))
                && conex is NIR && conex.Data == Zi(1, 22) && conex.DataInregistrare == Zi(2, 6));
        }

        // ── DIR-V6: anularea privește perioada ÎNREGISTRĂRII ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var nir = os.GetObjectByKey<NIR>(idNirTarziu);
            MotorOperare.AnuleazaOperarea(os, nir);
            s.Check($"DIR-V6 ({eticheta}) anularea documentului întârziat e PERMISĂ deși `Data` lui e în 01/{An}, "
                + "închisă: gardianul de perioadă al anulării întreabă despre perioada în care documentul a intrat "
                + "în evidență, adică exact perioada rândurilor care se șterg",
                nir.Stare == StareDocument.Draft
                && CubScena.FaraStoc(os, idNirTarziu) && CubScena.FaraNote(os, idNirTarziu));
            MotorOperare.Opereaza(os, nir);
            s.Check($"DIR-V6b ({eticheta}) re-operarea îl aduce înapoi, cu registrele tot la data înregistrării",
                nir.Stare == StareDocument.Operat && StocDocCub(os, idNirTarziu).All(p => p.Data == Zi(2, 5)));
        }

        // ── DIR-V7: stornoul se măsoară de la data înregistrării ──
        Guid idLotB;
        using (var os = s.Provider.CreateObjectSpace()) {
            var nirB = os.CreateObject<NIR>();
            nirB.Data = Zi(2, 1);
            nirB.DataInregistrare = Zi(2, 1);
            nirB.PredatorId = idFurnizor;
            nirB.PrimitorId = idGestA;
            var linie = os.CreateObject<NirDetaliu>();
            linie.Document = nirB;
            linie.TipMaterialId = idTipStoc;
            linie.Cantitate = 6m;
            linie.PretUnitar = 12m;
            linie.CodEconomicId = idCodEc;
            var lotB = linie.CreeazaLot(os, os.GetObjectByKey<Produs>(idProdus),
                os.GetObjectByKey<Gestiune>(idGestA));
            os.CommitChanges();
            MotorOperare.Opereaza(os, nirB);
            idLotB = lotB.ID;

            var refuzStorno = s.Refuz(() => MotorOperare.Storneaza(os, os.GetObjectByKey<NIR>(idNirTarziu), Zi(2, 3)));
            Console.WriteLine($"     MĂSURAT (DIR-V7/{eticheta}): stornoul pe {Zi(2, 3):dd.MM.yyyy} al documentului "
                + $"înregistrat pe {Zi(2, 5):dd.MM.yyyy} a ieșit cu „{s.PrimaLinie(refuzStorno)}”.");
            s.Check($"DIR-V7 ({eticheta}) o dată de storno ULTERIOARĂ datei documentului dar ANTERIOARĂ datei "
                + "înregistrării e refuzată cu textul nou: rândurile inverse n-au voie să cadă înaintea rândurilor "
                + "pe care le anulează",
                refuzStorno != null
                && refuzStorno.Contains("Data stornării nu poate preceda data înregistrării documentului."));
        }

        // ── DIR-V9/V10: ordinea FIFO și gardianul de sold sunt ale datei înregistrării ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var lotA = os.GetObjectByKey<Lot>(idLotA);
            var lotB = os.GetObjectByKey<Lot>(idLotB);
            var contLotACub = CubScena.Chei(os, [idLotA]).FirstOrDefault(c => c.GestiuneId == idGestA).ContId;
            var disponibileCub = Atlas.Conta.BackOffice.Module.Cub.Citiri.Loturi.Disponibile(os,
                Atlas.Conta.BackOffice.Module.Cub.Citiri.CitireCumul.Integrala, Zi(2, 10), idProdus, idGestA, contLotACub).ToList();
            Console.WriteLine($"     MĂSURAT (DIR-V9/{eticheta}): lotul A (document {Zi(1, 20):dd.MM.yyyy}) e datat "
                + $"{lotA.Data:dd.MM.yyyy}, lotul B (document {Zi(2, 1):dd.MM.yyyy}) e datat {lotB.Data:dd.MM.yyyy}; "
                + $"FIFO alocă întâi {(disponibileCub.Count > 0 && disponibileCub[0].LotId == idLotB ? "B" : "A")}.");
            s.Check($"DIR-V9 ({eticheta}) lotul documentului întârziat e mai NOU decât al celui cules la timp, deși "
                + "documentul lui e mai vechi — FIFO consumă în ordinea intrării în EVIDENȚĂ, singura ordine "
                + "compatibilă cu „sold ≥ 0 la orice dată” (proba invariantului VI)",
                lotA.Data == Zi(2, 5) && lotB.Data == Zi(2, 1) && lotA.Data > lotB.Data
                && disponibileCub.Count > 0 && disponibileCub[0].LotId == idLotB && disponibileCub[0].Cantitate >= 3m
                && disponibileCub.FindIndex(s => s.LotId == idLotB) < disponibileCub.FindIndex(s => s.LotId == idLotA));

            var refuzSoldCub = CubScena.RefuzSold(os, idLotA, idGestA, idProdus, contLotACub, Zi(2, 3), -1m);
            Console.WriteLine($"     MĂSURAT (DIR-V10/{eticheta}): ieșirea din lotul A pe {Zi(2, 3):dd.MM.yyyy} a "
                + $"ieșit cu „{s.PrimaLinie(refuzSoldCub)}”.");
            s.Check($"DIR-V10 ({eticheta}) o ieșire înregistrată ÎNAINTEA intrării lotului cade pe gardianul de sold, "
                + "cu textul lui neschimbat — nu există fereastră în care stocul să fie negativ",
                refuzSoldCub != null && refuzSoldCub.Contains($"Sold negativ (-1) la {Zi(2, 3):yyyy-MM-dd}"));
        }

        // ── DIR-V8: stornoul valid ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var nir = os.GetObjectByKey<NIR>(idNirTarziu);
            MotorOperare.Storneaza(os, nir, Zi(2, 10));
            var inverseCub = StocDocCub(os, idNirTarziu).Where(p => p.Storno).ToList();
            var inverseNoteCub = NoteDocCub(os, idNirTarziu).Where(p => p.Storno).ToList();
            s.Check($"DIR-V8 ({eticheta}) cu o dată ULTERIOARĂ înregistrării stornoul trece, iar rândurile inverse "
                + "cad la data stornării — regula nu s-a schimbat, doar capătul de la care se măsoară",
                nir.Stare == StareDocument.Stornat
                && inverseCub.Count > 0 && inverseCub.All(p => p.Data == Zi(2, 10))
                && inverseNoteCub.Count > 0 && inverseNoteCub.All(p => p.Data == Zi(2, 10)));
        }

        // ── DIR-V11: al patrulea registru ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var pif = os.CreateObject<PunereInFunctiune>();
            pif.Data = Zi(1, 25);
            pif.DataInregistrare = Zi(2, 12);
            pif.PredatorId = idUnitate;
            pif.PrimitorId = idGestA;
            var l = os.CreateObject<PunereInFunctiuneDetaliu>();
            l.Document = pif;
            l.ImobilizareId = idFisa;
            l.TipMaterialId = idTipF;
            l.Fel = FelLiniePif.Intrare;
            l.Valoare = 3600m;
            l.Cantitate = 1m;
            l.Metoda = MetodaAmortizare.Liniara;
            l.DurataLuni = 36;
            l.MetodaFiscala = MetodaAmortizare.Liniara;
            l.DurataFiscalaLuni = 36;
            l.CategorieFiscala = CategorieFiscala.Standard;
            l.UtilizareExclusiva = true;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);

            var randuri = Atlas.Conta.BackOffice.Module.Cub.Citiri.Imobilizari.Randuri(os, [idFisa], DateOnly.MaxValue);
            var refuzLuna = s.Refuz(() => MotorOperare.Storneaza(os, pif, Zi(3, 5)));
            Console.WriteLine($"     MĂSURAT (DIR-V11/{eticheta}): {randuri.Count} rânduri de fișă la "
                + $"{s.Ziua(randuri.Select(r => r.Rand.Data).FirstOrDefault())}; stornoul pe {Zi(3, 5):dd.MM.yyyy} a ieșit "
                + $"cu „{s.PrimaLinie(refuzLuna)}”.");
            s.Check($"DIR-V11 ({eticheta}) al patrulea registru urmează aceeași regulă: rândul PIF-ului întârziat e "
                + $"datat la înregistrare ({Zi(2, 12):dd.MM.yyyy}), iar stornoul e cerut în LUNA ÎNREGISTRĂRII "
                + "(02), nu în luna documentului — situația fișei e o sumă de rânduri ≤ dată",
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Imobilizari.Randuri(os, [idFisa], DateOnly.MaxValue) is { Count: 1 } randuriCub
                && randuriCub[0].Rand.Data == Zi(2, 12)
                && refuzLuna != null && refuzLuna.Contains($"02.{An}"));

            MotorOperare.Storneaza(os, pif, Zi(2, 20));
            s.Check($"DIR-V11b ({eticheta}) stornoul din luna înregistrării trece și scrie rândul invers la data lui",
                pif.Stare == StareDocument.Stornat
                && Atlas.Conta.BackOffice.Module.Cub.Citiri.Imobilizari.Randuri(os, [idFisa], DateOnly.MaxValue)
                    .Where(r => r.Rand.Storno).Count(r => r.Rand.Data == Zi(2, 20)) == 1);
        }

        // ── DIR-V12: calea Import1C — câmpul nu se culege deloc ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var btr = os.CreateObject<NotaTransfer>();
            btr.Data = Zi(2, 18);
            btr.PredatorId = idGestA;
            btr.PrimitorId = idGestB;
            btr.NumarPV = Marcaj;
            var linie = os.CreateObject<DocumentDetaliu>();
            linie.Document = btr;
            linie.TipMaterialId = idTipStoc;
            linie.LotId = idLotB;
            linie.Cantitate = 2m;
            os.CommitChanges();
            s.Check($"DIR-V12a ({eticheta}) documentul venit pe o cale care nu culege câmpul intră cu `default`",
                btr.DataInregistrare == default);
            MotorOperare.Opereaza(os, btr);
            var stocCub = StocDocCub(os, btr.ID);
            s.Check($"DIR-V12 ({eticheta}) normalizarea din motor („necules ⇒ data documentului”) e garanția pentru "
                + "ORICE cale — Import1C, Migrare, documentele generate: registrele cad la `Data`, exact ca înainte "
                + "de felie, deci baseline-ul importului rămâne identic",
                btr.DataInregistrare == Zi(2, 18) && stocCub.Count > 0 && stocCub.All(p => p.Data == Zi(2, 18)));
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataDir(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var fise = os.GetObjectsQuery<Imobilizare>()
                .Count(f => f.NumarInventar.StartsWith(Marcaj));
            s.Check($"DIR-V13 ({eticheta}) fără reziduu: nicio perioadă {An}, niciun document și nicio fișă rămase — "
                + "scena e re-rulabilă identic",
                perioade == 0 && documente == 0 && fise == 0);
        }
    }
}
