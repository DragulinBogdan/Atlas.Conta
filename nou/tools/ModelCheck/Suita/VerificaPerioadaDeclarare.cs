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
// Felia 27, pasul 4a (F27-D5): perioada de DECLARARE pe registrul fiscal, iar
// rectificativa ca DERIVAT din două timestamp-uri. Scena e a anului 2033.
// ══════════════════════════════════════════════════════════════════════════
static class VerificaPerioadaDeclarare {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-PDT";
        const int An = 2033;
        var eticheta = privat ? "privat" : "bugetar";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        // Neplătitorul n-are `PoliticaTva`, deci registrul fiscal îi rămâne gol și
        // regula de declarare întârziată n-are pe ce să se aplice: ancoră, nu scenă.
        if (!privat) {
            using var osB = s.Provider.CreateObjectSpace();
            var politici = osB.GetObjectsQuery<PoliticaTva>().Count();
            Console.WriteLine($"     MĂSURAT (PDT-V0/{eticheta}): {politici} rânduri `PoliticaTva`, "
                + $"{Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Fapte(osB).Count()} fapte fiscale.");
            s.Check($"PDT-V0 ({eticheta}) profilul neplătitor n-are nicio `PoliticaTva`, deci `RegistruTva` e gol și "
                + "atribuirea fiscală e inertă — perioada de declarare nu schimbă nimic acolo unde nu există "
                + "fapte fiscale",
                politici == 0 && CubScena.FaraFapte(osB));
            return;
        }

        void CurataPdt(IObjectSpace os) {
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<DepunereDeclaratie>()
                .Where(d => d.Perioada / 100 == An).ToList());
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
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
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID)).OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).Select(p => p.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        List<Atlas.Conta.BackOffice.Module.Cub.Citiri.FaptFiscal> FiscaleCub(IObjectSpace os, Guid docId) =>
            CubScena.Fapte(os, docId).OrderBy(f => f.Storno).ToList();

        using (var os = s.Provider.CreateObjectSpace())
            CurataPdt(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            Console.WriteLine($"     MĂSURAT (PDT-V0/{eticheta}): {perioade} perioade și {documente} documente în "
                + $"{An}, {inchise} perioade închise în bază.");
            s.Check($"PDT-V0 ({eticheta}) precondiție: anul {An} e liber și nicio perioadă a bazei nu e închisă — "
                + $"altfel închiderea lui 01/{An} n-ar fi capăt de lanț, iar jurnalele lunii ar fi măsurate peste "
                + "conținut străin",
                perioade == 0 && documente == 0 && inchise == 0);
        }

        Guid idFurnizor, idClient, idGestiune, idSediu, idTipCheltuiala, idTipVenit, idN21, idPoliticaFcl;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 1, 2, 3, 4 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-F";
            furnizor.Denumire = "Furnizor perioadă de declarare";
            furnizor.CodFiscal = "RO33333339";
            furnizor.InregistratTva = true;
            var client = os.CreateObject<Partener>();
            client.Cod = Marcaj + "-C";
            client.Denumire = "Client perioadă de declarare";
            client.CodFiscal = "RO44444442";
            client.InregistratTva = true;
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-G";
            gestiune.Denumire = "Gestiune perioadă de declarare";
            os.CommitChanges();
            idFurnizor = furnizor.ID;
            idClient = client.ID;
            idGestiune = gestiune.ID;
            idSediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU").ID;
            idTipCheltuiala = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628").ID;
            idTipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704").ID;
            idN21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21").ID;
            var politicaFct = os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == "FCT");
            var politicaFcl = os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == "FCL");
            idPoliticaFcl = politicaFcl.ID;
            s.Check($"PDT-V0b ({eticheta}) seed-ul a pus regula de declarare pe DIRECȚIE: deductibilul (FCT) declară "
                + "în perioada ÎNREGISTRĂRII (art. 301 — fără rectificativă), colectatul (FCL) în perioada "
                + "FAPTULUI (factura noastră rămâne fiscal a lunii ei, deci se rectifică)",
                politicaFct.Directie == DirectieTva.Deductibil
                && politicaFcl.Directie == DirectieTva.Colectat);
        }

        // ── PDT-V1: perioada DESCHISĂ a faptului câștigă întotdeauna ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + "-FCT0";
            fct.Data = Zi(3, 3);
            fct.DataInregistrare = Zi(3, 10);
            fct.PredatorId = idFurnizor;
            fct.PrimitorId = idGestiune;
            var linie = os.CreateObject<FacturaIntrareDetaliu>();
            linie.Document = fct;
            linie.TipMaterialId = idTipCheltuiala;
            linie.Cantitate = 1m;
            linie.PretUnitar = 100m;
            linie.TipTvaId = idN21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            var randuri = FiscaleCub(os, fct.ID);
            Console.WriteLine($"     MĂSURAT (PDT-V1/{eticheta}): {randuri.Count} rânduri, `Data` "
                + $"{s.Ziua(randuri[0].DataJurnal())}, perioada {randuri[0].PerioadaLuna:00}/{randuri[0].PerioadaAn}, "
                + $"`ScrisLa` {randuri[0].ScrisLa:yyyy-MM-dd HH:mm:ss}.");
            var randuriCub = FiscaleCub(os, fct.ID);
            s.Check($"PDT-V1 ({eticheta}) faptul dintr-o perioadă DESCHISĂ ({Zi(3, 3):dd.MM.yyyy}, înregistrat pe "
                + $"{Zi(3, 10):dd.MM.yyyy}) se declară în perioada LUI, oricare ar fi politica — regula de "
                + "declarare întârziată se aplică doar peste o graniță închisă",
                randuriCub.Count == 1 && randuriCub[0].DataJurnal() == Zi(3, 3)
                && randuriCub[0].PerioadaAn == An && randuriCub[0].PerioadaLuna == 3);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            s.InchideAcceptTot(os, An, 1, Marcaj);
            var p = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 1);
            s.Check($"PDT — precondiție de scenă ({eticheta}): 01/{An} se închide (capăt de lanț) și își reține "
                + "`InchisaPrimaOara` — reperul contra căruia se citește conținutul de rectificativă",
                p.Inchisa && p.InchisaPrimaOara != null);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check("PDT — închiderea contabilă singură nu confirmă D394",
                !TvaProiectii.Rectificativa(os, An, 1).EsteRectificativa
                && TvaProiectii.Rectificativa(os, An, 1).ConfirmataLa == null);
            FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 1,
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Versiune(os, FormularFiscal.D394, new(An, 1, 1), new(An, 1, 31)), Marcaj);
        }

        // ── PDT-V2…V4: faptul întârziat, pe ambele direcții ──
        Guid idFctTarziu, idFclTarziu;
        using (var os = s.Provider.CreateObjectSpace()) {
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + "-FCT1";
            fct.Data = Zi(1, 20);
            fct.DataInregistrare = Zi(2, 5);
            fct.PredatorId = idFurnizor;
            fct.PrimitorId = idGestiune;
            var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
            linieFct.Document = fct;
            linieFct.TipMaterialId = idTipCheltuiala;
            linieFct.Cantitate = 1m;
            linieFct.PretUnitar = 300m;
            linieFct.TipTvaId = idN21;

            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Numar = Marcaj + "-FCL1";
            fcl.Data = Zi(1, 20);
            fcl.DataInregistrare = Zi(2, 5);
            fcl.PredatorId = idSediu;
            fcl.PrimitorId = idClient;
            var linieFcl = os.CreateObject<FacturaIesireDetaliu>();
            linieFcl.Document = fcl;
            linieFcl.TipMaterialId = idTipVenit;
            linieFcl.Cantitate = 1m;
            linieFcl.PretUnitar = 500m;
            linieFcl.TipTvaId = idN21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            MotorOperare.Opereaza(os, fcl);
            idFctTarziu = fct.ID;
            idFclTarziu = fcl.ID;

            var rFct = FiscaleCub(os, fct.ID);
            var rFcl = FiscaleCub(os, fcl.ID);
            Console.WriteLine($"     MĂSURAT (PDT-V2/{eticheta}): FCT `Data` {s.Ziua(rFct[0].DataJurnal())} perioada "
                + $"{rFct[0].PerioadaLuna:00}/{rFct[0].PerioadaAn}; FCL `Data` {s.Ziua(rFcl[0].DataJurnal())} perioada "
                + $"{rFcl[0].PerioadaLuna:00}/{rFcl[0].PerioadaAn}.");
            var rFctCub = FiscaleCub(os, fct.ID);
            var rFclCub = FiscaleCub(os, fcl.ID);
            s.Check($"PDT-V2 ({eticheta}) factura de intrare cu `Data` {Zi(1, 20):dd.MM.yyyy} (perioadă ÎNCHISĂ) "
                + $"înregistrată pe {Zi(2, 5):dd.MM.yyyy} păstrează `Data` FAPTULUI pe rând, dar se declară în "
                + $"02/{An} — `PerioadaInregistrarii`, adică exact dreptul de deducere exercitat la primire",
                rFctCub.Count == 1 && rFctCub[0].DataJurnal() == Zi(1, 20)
                && rFctCub[0].PerioadaAn == An && rFctCub[0].PerioadaLuna == 2 && rFctCub[0].Baza == 300m);
            s.Check($"PDT-V3 ({eticheta}) factura de ieșire, cu ACELEAȘI date, se declară în 01/{An} — "
                + "`PerioadaFaptului`: factura noastră aparține fiscal lunii ei, iar consecința e o rectificativă, "
                + "nu o mutare tăcută",
                rFclCub.Count == 1 && rFclCub[0].DataJurnal() == Zi(1, 20)
                && rFclCub[0].PerioadaAn == An && rFclCub[0].PerioadaLuna == 1 && rFclCub[0].Baza == 500m);
            s.Check($"PDT-V4 ({eticheta}) `ScrisLa` e populat pe toate rândurile scenei, în UTC — fără el "
                + "rectificativa n-ar avea al doilea timestamp",
                rFctCub.Count + rFclCub.Count == 2
                && rFctCub.Concat(rFclCub).All(f => f.ScrisLa != default && f.ScrisLa.Kind != DateTimeKind.Local));
        }

        // ── PDT-V5/V6: conținutul de rectificativă ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var rect = TvaProiectii.Rectificativa(os, An, 1);
            var rectFebruarie = TvaProiectii.Rectificativa(os, An, 2);
            Console.WriteLine($"     MĂSURAT (PDT-V5/{eticheta}): 01/{An} — {rect.Randuri.Count} rânduri, "
                + $"{rect.Agregat.Count} poziții de agregat, bază {rect.Agregat.Sum(a => a.Baza)}, TVA "
                + $"{rect.Agregat.Sum(a => a.Tva)}; 02/{An} — rectificativă: {rectFebruarie.EsteRectificativa}.");
            s.Check($"PDT-V5 ({eticheta}) conținutul de rectificativă al lui 01/{An} e EXACT rândul facturii de "
                + "ieșire întârziate: declarat în perioada închisă, scris după închiderea ei. Nu există flag — e "
                + "diferența dintre `ScrisLa` și `InchisaPrimaOara`",
                rect.EsteRectificativa && rect.ConfirmataLa != null
                && rect.Randuri.Count == 1 && rect.Randuri[0].DocumentId == idFclTarziu
                && rect.Randuri[0].Sens == "Livrare" && rect.Randuri[0].Baza == 500m
                && rect.Randuri[0].Tva == 105m && !rect.Randuri[0].Storno
                && rect.Agregat.Count == 1 && rect.Agregat[0].Baza == 500m && rect.Agregat[0].Tva == 105m
                && rect.Agregat[0].Randuri == 1);
            s.Check($"PDT-V6 ({eticheta}) 02/{An} NU e rectificativă deși are cifre scrise târziu: n-a fost închisă "
                + "niciodată, deci n-are declarație depusă de rectificat — reperul lipsește, nu cifrele",
                !rectFebruarie.EsteRectificativa && rectFebruarie.ConfirmataLa == null
                && rectFebruarie.Randuri.Count == 0 && rectFebruarie.Agregat.Count == 0);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Numar = Marcaj + "-FCL2";
            fcl.Data = Zi(1, 21);
            fcl.DataInregistrare = Zi(2, 6);
            fcl.PredatorId = idSediu;
            fcl.PrimitorId = idClient;
            var linie = os.CreateObject<FacturaIesireDetaliu>();
            linie.Document = fcl;
            linie.TipMaterialId = idTipVenit;
            linie.Cantitate = 1m;
            linie.PretUnitar = 700m;
            linie.TipTvaId = idN21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl);
            var randuriCub = FiscaleCub(os, fcl.ID);
            s.Check($"PDT-V7 ({eticheta}) D300 nedepus: livrarea 700/147 rămâne în ianuarie",
                randuriCub.Count == 1 && randuriCub[0].PerioadaLuna == 1 && randuriCub[0].Baza == 700m && randuriCub[0].Tva == 147m);
        }

        // ── PDT-V8…V11: consumatorii filtrează pe PERIOADA DE DECLARARE ──
        using (var os = s.Provider.CreateObjectSpace()) {
            List<Guid> Jurnal(SensTva sens, int luna) =>
                TvaProiectii.JurnalTva(os, sens, Zi(luna, 1), Zi(luna, DateTime.DaysInMonth(An, luna)))
                    .ToList().Select(r => r.DocumentId).ToList();

            var vanzariIanuarie = Jurnal(SensTva.Livrare, 1);
            var vanzariFebruarie = Jurnal(SensTva.Livrare, 2);
            var cumparariIanuarie = Jurnal(SensTva.Achizitie, 1);
            var cumparariFebruarie = Jurnal(SensTva.Achizitie, 2);
            Console.WriteLine($"     MĂSURAT (PDT-V8/{eticheta}): jurnal vânzări 01 = {vanzariIanuarie.Count} "
                + $"rânduri, 02 = {vanzariFebruarie.Count}; cumpărări 01 = {cumparariIanuarie.Count}, 02 = "
                + $"{cumparariFebruarie.Count}.");
            s.Check($"PDT-V8 ({eticheta}) jurnalul de VÂNZĂRI pe 01/{An} conține factura de ieșire întârziată, deși "
                + "ea a fost scrisă în februarie: jurnalul e pe perioada de DECLARARE, iar ea a rămas a faptului",
                vanzariIanuarie.Contains(idFclTarziu) && !vanzariFebruarie.Contains(idFclTarziu));
            s.Check($"PDT-V9 ({eticheta}) jurnalul de CUMPĂRĂRI pe 02/{An} conține factura de intrare întârziată, "
                + $"iar cel pe 01/{An} nu — aceeași regulă, cealaltă politică",
                cumparariFebruarie.Contains(idFctTarziu) && !cumparariIanuarie.Contains(idFctTarziu));

            var randJurnal = TvaProiectii.JurnalTva(os, SensTva.Livrare, Zi(1, 1), Zi(1, 31))
                .ToList().Single(r => r.DocumentId == idFclTarziu);
            s.Check($"PDT-V9b ({eticheta}) rândul de jurnal poartă AMBELE coordonate: `Data` faptului "
                + $"({Zi(1, 20):dd.MM.yyyy}) și perioada de declarare (01/{An}) — diferența dintre ele e chiar "
                + "ce trebuie să vadă contabilul",
                randJurnal.DataDocument == Zi(1, 20) && randJurnal.DataInregistrare == Zi(2, 5) && randJurnal.PerioadaAn == An && randJurnal.PerioadaLuna == 1);

            var d300 = D300Proiectii.D300(os, Zi(1, 1), Zi(1, 31), new ParametriD300());
            var d394 = D394Proiectii.D394(os, Zi(1, 1), Zi(1, 31));
            var d300Trimestru = D300Proiectii.D300(os, Zi(1, 1), Zi(3, 31), new ParametriD300());
            Console.WriteLine($"     MĂSURAT (PDT-V10/{eticheta}): D300 01/{An} rectificativă = "
                + $"{d300.Rectificativa} cu {d300.DiferenteDeclarat.Count} poziții (bază "
                + $"{d300.DiferenteDeclarat.Sum(a => a.Baza)}); D394 = {d394.Rectificativa} cu "
                + $"{d394.DiferenteDeclarat.Count}; pe trimestru = {d300Trimestru.Rectificativa}.");
            s.Check($"PDT-V10 ({eticheta}) D300 nu folosește rectificativa D394",
                !d300.Rectificativa && d300.DiferenteDeclarat.Count == 0);
            s.Check($"PDT-V11 ({eticheta}) D394 detectează ambele facturi emise în ianuarie după confirmare",
                d394.Rectificativa && d394.DiferenteDeclarat.Count == 1
                && d394.DiferenteDeclarat[0].Baza == 1200m && d394.DiferenteDeclarat[0].Tva == 252m);
            s.Check($"PDT-V11b ({eticheta}) pe un interval de MAI MULTE luni întrebarea n-are subiect (nu există O "
                + "declarație depusă): `Rectificativa` e falsă și lista goală, declarat ca limită",
                !d300Trimestru.Rectificativa && d300Trimestru.DiferenteDeclarat.Count == 0);
        }

        // ── PDT-V12: stornoul e fapt al perioadei stornării ──
        using (var os = s.Provider.CreateObjectSpace()) {
            MotorOperare.Storneaza(os, os.GetObjectByKey<FacturaIesire>(idFclTarziu), Zi(2, 10));
            var inverse = FiscaleCub(os, idFclTarziu).Where(r => r.Storno).ToList();
            Console.WriteLine($"     MĂSURAT (PDT-V12/{eticheta}): {inverse.Count} rânduri inverse, `Data` "
                + $"{s.Ziua(inverse[0].DataJurnal())}, perioada {inverse[0].PerioadaLuna:00}/{inverse[0].PerioadaAn}.");
            var inverseCub = FiscaleCub(os, idFclTarziu).Where(f => f.Storno).ToList();
            s.Check($"PDT-V12 ({eticheta}) rândul invers al unei facturi declarate în 01/{An} cade în 02/{An}, nu în "
                + "01: stornoul e un fapt al perioadei stornării (JT-D5), iar declarația deja depusă rămâne cum a "
                + "fost depusă. Excepția cu motiv e a corecției (F27-D6, pasul 5)",
                inverseCub.Count == 1 && inverseCub[0].DataJurnal() == Zi(2, 10)
                && inverseCub[0].PerioadaAn == An && inverseCub[0].PerioadaLuna == 2
                && inverseCub[0].Baza == -500m && inverseCub[0].ScrisLa != default);
        }

        // ── PDT-V13: redeschiderea nu șterge reperul ──
        using (var os = s.Provider.CreateObjectSpace()) {
            PerioadaService.Redeschide(os, An, 1, "probă: reperul rectificativei", null, Marcaj);
            var p = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 1);
            var rect = TvaProiectii.Rectificativa(os, An, 1);
            s.Check($"PDT-V13 ({eticheta}) redeschiderea lui 01/{An} NU stinge `InchisaPrimaOara`, deci conținutul "
                + "de rectificativă rămâne detectabil — ce a fost declarat o dată rămâne reperul, oricâte "
                + "redeschideri urmează",
                !p.Inchisa && p.InchisaPrimaOara != null
                && rect.EsteRectificativa && rect.Randuri.Count == 2);
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataPdt(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var politica = os.GetObjectByKey<PoliticaTva>(idPoliticaFcl);
            s.Check($"PDT-V14 ({eticheta}) fără reziduu: nicio perioadă {An}, niciun document rămas, iar politica "
                + "de seed e la valoarea ei — scena e re-rulabilă identic",
                perioade == 0 && documente == 0
                && politica.Directie == DirectieTva.Colectat);
        }
    }
}
