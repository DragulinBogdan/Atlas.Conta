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

static class VerificaPartide {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-PAR";
        const int An = 2035;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipVenit = privat ? "704" : "751.01.00";
        var codContCasa = privat ? "5311" : "531.01.01";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        // ─────────── curățenia de scenă (purjă FIZICĂ, F13-D2) ───────────
        void CurataPar(IObjectSpace os) {
            // Snapshot-urile ȘI partidele întâi: FK-urile lor spre document sunt
            // `Restrict`, deci un rând rămas ar bloca purja documentelor.
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var pj = new Purja(os);
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            // Rândurile INVERSE înaintea celor pe care le desfac (`InverseazaId` e
            // FK `Restrict`, ca legătura de corecție).
            foreach (var imp in os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId))
                    .OrderByDescending(i => i.InverseazaId != null))
                pj.Adauga(imp);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID)).OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            var produsIds = os.GetObjectsQuery<Produs>()
                .Where(x => x.Cod.StartsWith(Marcaj)).Select(x => x.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Lot>()
                .Where(l => produsIds.Contains(l.ProdusId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>()
                .Where(x => produsIds.Contains(x.ID)).ToList());
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
        string RefuzGardianPar(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        // Restul, citit prin proiecție, filtrat pe contrapartida scenei: celelalte
        // scene ale suitei trăiesc în bază și n-au voie să intre în cifre.
        List<(string Tip, string Numar, decimal Total, decimal Ramas)> Rest(IObjectSpace os, Guid contrapartida,
                DateOnly? laData = null) =>
            ImperecheriProiectii.DocumenteCuRest(os, contrapartida, null, laData)
                .ToList()
                .OrderBy(r => r.Numar, StringComparer.Ordinal)
                .Select(r => (r.Tip, r.Numar, r.Total, Ramas: r.Rest))
                .ToList();

        string Bani(decimal v) => v.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

        string Serializeaza(List<(string Tip, string Numar, decimal Total, decimal Ramas)> randuri) =>
            string.Join(" | ", randuri.Select(r => $"{r.Tip} {r.Numar}: {Bani(r.Total)}/{Bani(r.Ramas)}"));

        Dictionary<Guid, decimal> Partide(IObjectSpace os, int an, int luna) =>
            os.GetObjectsQuery<PartidaDeschisa>().Where(p => p.An == an && p.Luna == luna)
                .Select(p => new { p.DocumentId, p.Rest }).ToList()
                .Where(p => p.DocumentId != null).GroupBy(p => p.DocumentId.Value).ToDictionary(g => g.Key, g => g.Sum(p => p.Rest));

        // Totalul de control: Σ `LiniiCreanta` pe liniile PERSISTATE, adică exact
        // definiția pe care motorul o scrie — dar calculată pe altă cale decât
        // `MotorOperare` (LINQ pe query, nu pe navigația `Detalii` în memorie).
        decimal SumaCreanta(IObjectSpace os, Guid documentId) {
            var doc = os.GetObjectByKey<Document>(documentId);
            var linii = doc.LiniiCreanta(
                os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == documentId));
            return linii.Select(d => (decimal?)(d.Valoare + d.ValoareTva)).Sum() ?? 0m;
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataPar(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            var partide = os.GetObjectsQuery<PartidaDeschisa>().Count();
            Console.WriteLine($"     MĂSURAT (PAR-V0/{eticheta}): {perioade} perioade și {documente} documente în "
                + $"{An}, {inchise} perioade închise în bază, {partide} partide deschise.");
            s.Check($"PAR-V0 ({eticheta}) precondiție: anul {An} e liber, nicio perioadă a bazei nu e închisă și nu "
                + "există nicio partidă — altfel „identic cu și fără partide” ar fi măsurat peste conținut străin",
                perioade == 0 && documente == 0 && inchise == 0 && partide == 0);
        }

        // ═════════════════════ scena: ianuarie ═════════════════════
        Guid idClient, idCasa, idFclA, idFclB, idInc1, idImpA, idImpB;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 1, 2, 3 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
            }
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipVenit);
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var client = os.CreateObject<Partener>();
            client.Cod = Marcaj + "-CL";
            client.Denumire = "Client partide";
            client.CodFiscal = "RO33333341";
            var casa = os.CreateObject<ContPropriu>();
            casa.Cod = Marcaj + "-CS";
            casa.Denumire = "Casa partide";
            casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == codContCasa);
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic partide";
            os.CommitChanges();
            s.Check($"PAR — precondiție de profil ({eticheta}): tipul de venit („{codTipVenit}”) și TRZ sunt în seed, "
                + "deci scena de creanțe e aceeași pe ambele profiluri",
                tipVenit != null && tipTrz != null && sediu != null
                && casa.ContImplicit != null);

            FacturaIesire Fcl(string sufix, int zi, decimal valoare) {
                var f = os.CreateObject<FacturaIesire>();
                f.Numar = Marcaj + sufix;
                f.Data = Zi(1, zi);
                f.DataInregistrare = Zi(1, zi);
                f.Predator = sediu;
                f.Primitor = client;
                var l = os.CreateObject<FacturaIesireDetaliu>();
                l.Document = f;
                l.TipMaterial = tipVenit;
                l.Cantitate = 1m;
                l.PretUnitar = valoare;
                l.CodEconomicId = codEc.ID;
                return f;
            }

            Incasare Inc(string sufix, int luna, int zi, decimal valoare) {
                var i = os.CreateObject<Incasare>();
                i.Data = new DateOnly(An, luna, zi);
                i.DataInregistrare = new DateOnly(An, luna, zi);
                i.Predator = client;
                i.Primitor = casa;
                i.TipInstrument = TipInstrumentPlata.Chitanta;
                var l = os.CreateObject<DocumentTrezorerieDetaliu>();
                l.Document = i;
                l.TipMaterial = tipTrz;
                l.Valoare = valoare;
                l.CodEconomicId = codEc.ID;
                return i;
            }

            var fclA = Fcl("-FCL-A", 5, 100m);
            var fclB = Fcl("-FCL-B", 6, 250m);
            var inc1 = Inc("-INC1", 1, 10, 120m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclA);
            MotorOperare.Opereaza(os, fclB);
            MotorOperare.Opereaza(os, inc1);
            os.CommitChanges();

            idClient = client.ID; idCasa = casa.ID;
            idFclA = fclA.ID; idFclB = fclB.ID; idInc1 = inc1.ID;

            Console.WriteLine($"     MĂSURAT (PAR-V1/{eticheta}): TotalStingere — FCL A {fclA.TotalStingere}, "
                + $"FCL B {fclB.TotalStingere}, INC 1 {inc1.TotalStingere}.");
            s.Check($"PAR-V1 ({eticheta}) `TotalStingere` e scris de motor la operare din partidele cubului, "
                + "în sensul de stins al fiecărui document",
                ImperechereService.Total(os, idFclA) == 100m
                && ImperechereService.Total(os, idFclB) == 250m
                && ImperechereService.Total(os, idInc1) == 120m);

            // Stingerea lui ianuarie: încasarea de 120 închide A integral și B parțial.
            var impA = ImperechereService.Imperecheaza(os, inc1, fclA, 100m, null, Zi(1, 10));
            var impB = ImperechereService.Imperecheaza(os, inc1, fclB, 20m, null, Zi(1, 10));
            idImpA = impA.ID; idImpB = impB.ID;
            s.Check($"PAR-V2 ({eticheta}) imperecherea manuală poartă data cerută, iar restul scade pe ambele "
                + "laturi (A stinsă integral, B 230, încasarea 0)",
                impA.Data == Zi(1, 10) && impB.Data == Zi(1, 10)
                && ImperechereService.Ramas(os, idFclA) == 0m
                && ImperechereService.Ramas(os, idFclB) == 230m
                && ImperechereService.Ramas(os, idInc1) == 0m);

            s.CheckRefuza($"PAR-V3 ({eticheta}) imperechere datată ÎNAINTEA înregistrării documentelor → refuz "
                + "(faptul de stingere nu poate precede intrarea în evidență)",
                () => ImperechereService.Imperecheaza(os, inc1, fclB, 1m, null, Zi(1, 4)));
        }

        // ═════════════════════ închiderea lui ianuarie ═════════════════════
        string restInaintea;
        using (var os = s.Provider.CreateObjectSpace()) {
            restInaintea = Serializeaza(Rest(os, idClient));
            Console.WriteLine($"     MĂSURAT (PAR-V4/{eticheta}): `DocumenteCuRest` înainte de închidere — "
                + $"{restInaintea}.");
            s.Check($"PAR-V4 ({eticheta}) fără nicio perioadă închisă proiecția citește integral: doar B are rest "
                + "(230), A și încasarea sunt stinse",
                restInaintea == $"FCL {Marcaj}-FCL-B: 250.00/230.00");
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            s.InchideAcceptTot(os, An, 1, Marcaj);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var partide = Partide(os, An, 1);
            Console.WriteLine($"     MĂSURAT (PAR-V5/{eticheta}): partidele lui 01/{An} — {partide.Count} rânduri, "
                + $"Σ rest {partide.Values.Sum():0.00}.");
            s.Check($"PAR-V5 ({eticheta}) închiderea materializează partidele deschise: EXACT documentele cu rest ≠ 0 "
                + "la sfârșitul perioadei (B 230), nu și cele stinse integral",
                partide.Count == 1 && partide.TryGetValue(idFclB, out var restB) && restB == 230m);

            var restDupa = Serializeaza(Rest(os, idClient));
            s.Check($"PAR-V6 ({eticheta}) „identic cu și fără partide”: proiecția citită prin partida lui 01/{An} dă "
                + "EXACT rândurile și cifrele citite integral înainte de închidere",
                restDupa == restInaintea);
        }

        // ═════════════════════ februarie: stingerea restului ═════════════════════
        Guid idInc2;
        using (var os = s.Provider.CreateObjectSpace()) {
            var client = os.GetObjectByKey<Repartitor>(idClient);
            var casa = os.GetObjectByKey<Repartitor>(idCasa);
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var codEc = os.FirstOrDefault<CodEconomic>(c => c.Cod == Marcaj + "-CE");
            var inc2 = os.CreateObject<Incasare>();
            inc2.Data = Zi(2, 5);
            inc2.DataInregistrare = Zi(2, 5);
            inc2.Predator = client;
            inc2.Primitor = casa;
            inc2.TipInstrument = TipInstrumentPlata.Chitanta;
            var l = os.CreateObject<DocumentTrezorerieDetaliu>();
            l.Document = inc2;
            l.TipMaterial = tipTrz;
            l.Valoare = 230m;
            l.CodEconomicId = codEc.ID;
            os.CommitChanges();
            MotorOperare.Opereaza(os, inc2);
            os.CommitChanges();
            idInc2 = inc2.ID;

            s.CheckRefuza($"PAR-V7 ({eticheta}) imperechere datată într-o perioadă ÎNCHISĂ → refuz (stingerea se "
                + "scrie doar în fereastra deschisă)",
                () => ImperechereService.Imperecheaza(os, inc2, os.GetObjectByKey<Document>(idFclB), 1m,
                    null, Zi(1, 20)));

            ImperechereService.Imperecheaza(os, inc2, os.GetObjectByKey<Document>(idFclB), 230m, null, Zi(2, 10));
            var restLaZi = Serializeaza(Rest(os, idClient));
            var restLa31Ian = Serializeaza(Rest(os, idClient, Zi(1, 31)));
            Console.WriteLine($"     MĂSURAT (PAR-V8/{eticheta}): la zi „{restLaZi}”, la 31.01 „{restLa31Ian}”.");
            s.Check($"PAR-V8 ({eticheta}) stingerea din fereastra deschisă coboară restul partidei la zero, dar "
                + $"citirea la 31.01 rămâne cea a partidei — `laData` e graniță, nu filtru",
                restLaZi == "" && restLa31Ian == $"FCL {Marcaj}-FCL-B: 250.00/230.00");
        }

        // ═════════════════════ desfacerea: rând invers, nu ștergere ═════════════
        Guid idInvers;
        using (var os = s.Provider.CreateObjectSpace()) {
            os.Delete(os.GetObjectByKey<Imperechere>(idImpB));
            var refuz = RefuzGardianPar(os);
            Console.WriteLine($"     MĂSURAT (PAR-V9/{eticheta}): ștergerea directă a stingerii din ianuarie → "
                + $"„{refuz ?? "<ACCEPTATĂ>"}”.");
            s.Check($"PAR-V9 ({eticheta}) o imperechere dintr-o perioadă ÎNCHISĂ nu se șterge — gardianul trimite "
                + "la rândul invers",
                refuz != null && refuz.Contains("nu se șterge"));
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var invers = ImperechereService.Desfa(os, idImpB, Zi(2, 15));
            idInvers = invers.ID;
            Console.WriteLine($"     MĂSURAT (PAR-V10/{eticheta}): rând invers {invers.Suma:0.00} la "
                + $"{invers.Data:dd.MM.yyyy}; rest B {ImperechereService.Ramas(os, idFclB):0.00}, "
                + $"rest INC1 {ImperechereService.Ramas(os, idInc1):0.00}.");
            s.Check($"PAR-V10 ({eticheta}) desfacerea scrie un rând INVERS (−20, legat de original, nu autogenerat) "
                + "în fereastra deschisă; `Asignat` însumează algebric, deci restul se eliberează pe AMBELE "
                + "documente (B 20, încasarea 20)",
                invers.Suma == -20m && invers.Data == Zi(2, 15) && invers.InverseazaId == idImpB
                && !invers.Autogenerat
                && ImperechereService.Ramas(os, idFclB) == 20m
                && ImperechereService.Ramas(os, idInc1) == 20m);

            var randuri = Rest(os, idClient);
            Console.WriteLine($"     MĂSURAT (PAR-V11/{eticheta}): după desfacere — {Serializeaza(randuri)}.");
            s.Check($"PAR-V11 ({eticheta}) proiecția vede desfacerea pe AMBELE documente, deși niciunul nu e "
                + "înregistrat după referință: documentul stins integral la închidere (deci absent din partide) "
                + "reintră prin fereastra deschisă, cu restul eliberat",
                randuri.Count == 2
                && randuri.Any(r => r.Tip == "FCL" && r.Numar == Marcaj + "-FCL-B" && r.Total == 250m && r.Ramas == 20m)
                && randuri.Any(r => r.Tip == "INC" && r.Total == 120m && r.Ramas == 20m));

            s.CheckRefuza($"PAR-V12 ({eticheta}) a doua desfacere a aceleiași imperecheri → refuz (legătura e 1:1)",
                () => ImperechereService.Desfa(os, idImpB, Zi(2, 16)));
            s.CheckRefuza($"PAR-V13 ({eticheta}) desfacerea unui rând INVERS → refuz (nu se desface desfacerea)",
                () => ImperechereService.Desfa(os, idInvers, Zi(2, 16)));
        }

        // ═════════════════════ stornarea unui document imperecheat ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var inc1 = os.GetObjectByKey<Document>(idInc1);
            MotorOperare.Storneaza(os, inc1, Zi(2, 20));
            var legaturi = os.GetObjectsQuery<Imperechere>()
                .Where(i => i.DocumentStingatorId == idInc1 || i.DocumentId == idInc1)
                .Select(i => new { i.Suma, i.Data, i.InverseazaId }).ToList();
            Console.WriteLine($"     MĂSURAT (PAR-V14/{eticheta}): după storno — {legaturi.Count} legături, "
                + $"Σ {legaturi.Sum(l => l.Suma):0.00}.");
            s.Check($"PAR-V14 ({eticheta}) stornarea unui document cu stingeri în perioadă ÎNCHISĂ nu le cere "
                + "șterse: motorul scrie rândurile inverse la data stornării (stingerea vie de 100 se anulează, "
                + "cea deja desfăcută nu se inversează a doua oară) și Σ ajunge zero",
                inc1.Stare == StareDocument.Stornat
                && legaturi.Count == 4 && legaturi.Sum(l => l.Suma) == 0m
                && legaturi.Count(l => l.InverseazaId != null && l.Data == Zi(2, 20)) == 1
                && ImperechereService.Ramas(os, idFclA) == 100m);
        }

        // ═════════════════════ fereastra deschisă: refuzurile rămân ═════════════
        Guid idFclC, idInc3;
        using (var os = s.Provider.CreateObjectSpace()) {
            var client = os.GetObjectByKey<Repartitor>(idClient);
            var casa = os.GetObjectByKey<Repartitor>(idCasa);
            var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipVenit);
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var codEc = os.FirstOrDefault<CodEconomic>(c => c.Cod == Marcaj + "-CE");

            var fclC = os.CreateObject<FacturaIesire>();
            fclC.Numar = Marcaj + "-FCL-C";
            fclC.Data = Zi(2, 22);
            fclC.DataInregistrare = Zi(2, 22);
            fclC.Predator = sediu;
            fclC.Primitor = client;
            var lc = os.CreateObject<FacturaIesireDetaliu>();
            lc.Document = fclC;
            lc.TipMaterial = tipVenit;
            lc.Cantitate = 1m;
            lc.PretUnitar = 70m;
            lc.CodEconomicId = codEc.ID;

            var inc3 = os.CreateObject<Incasare>();
            inc3.Data = Zi(2, 23);
            inc3.DataInregistrare = Zi(2, 23);
            inc3.Predator = client;
            inc3.Primitor = casa;
            inc3.TipInstrument = TipInstrumentPlata.Chitanta;
            var li = os.CreateObject<DocumentTrezorerieDetaliu>();
            li.Document = inc3;
            li.TipMaterial = tipTrz;
            li.Valoare = 70m;
            li.CodEconomicId = codEc.ID;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclC);
            MotorOperare.Opereaza(os, inc3);
            os.CommitChanges();
            idFclC = fclC.ID; idInc3 = inc3.ID;

            var impC = ImperechereService.Imperecheaza(os, inc3, fclC, 70m, null, Zi(2, 24));
            var refuzStorno = s.Refuz(() => MotorOperare.Storneaza(os, fclC, Zi(2, 25)));
            var refuzAnulare = s.Refuz(() => MotorOperare.AnuleazaOperarea(os, fclC));
            Console.WriteLine($"     MĂSURAT (PAR-V15/{eticheta}): storno → „{refuzStorno ?? "<ACCEPTAT>"}”; "
                + $"anulare → „{refuzAnulare ?? "<ACCEPTATĂ>"}”.");
            s.Check($"PAR-V15 ({eticheta}) cu stingerea în fereastra DESCHISĂ, storno-ul și anularea rămân refuzate "
                + "cu textul de azi — ce se poate șterge se cere șters, nu se inversează",
                refuzStorno != null && refuzStorno.Contains("ștergeți-le întâi")
                && refuzAnulare != null && refuzAnulare.Contains("ștergeți-le întâi"));

            // Ștergerea e liberă în fereastra deschisă — și ea redeschide anularea.
            ImperechereService.Sterge(os, impC.ID);
            MotorOperare.AnuleazaOperarea(os, fclC);
            s.Check($"PAR-V16 ({eticheta}) ștergerea stingerii din fereastra deschisă trece, anularea merge, iar "
                + "`TotalStingere` revine la null: totalul e al documentului OPERAT, nu al draftului",
                fclC.Stare == StareDocument.Draft && fclC.TotalStingere == null
                && ImperechereService.Total(os, idFclC) == 0m);
        }

        // ═════════════════════ redeschiderea și reconstrucția ═════════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var inainte = Partide(os, An, 1);
            PerioadaService.Redeschide(os, An, 1, "Probă partide", null, Marcaj);
            var dupaRedeschidere = Partide(os, An, 1);
            s.InchideAcceptTot(os, An, 1, Marcaj);
            var dupaReinchidere = Partide(os, An, 1);
            s.Check($"PAR-V17 ({eticheta}) redeschiderea ȘTERGE partidele perioadei, iar re-închiderea le rescrie "
                + "IDENTIC (aceeași mulțime, același rest) — partida e proiecție rescrisă, nu urmă",
                inainte.Count == 1 && dupaRedeschidere.Count == 0
                && dupaReinchidere.Count == inainte.Count
                && dupaReinchidere.All(p => inainte.TryGetValue(p.Key, out var v) && v == p.Value));
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var raport = SolduriService.Reconstruieste(os);
            var rand = raport.Referinte.Single(r => r.An == An && r.Luna == 1);
            Console.WriteLine($"     MĂSURAT (PAR-V18/{eticheta}): partide existente {rand.PartideExistente}, "
                + $"recalculate {rand.PartideRecalculate}, diferite {rand.PartideDiferite}, "
                + $"Δrest {rand.DiferentaRest:0.00}.");
            s.Check($"PAR-V18 ({eticheta}) `Reconstruieste` acoperă și partidele și nu găsește nicio diferență pe "
                + "perioada de referință — mulțimea și restul coincid cu recalculul",
                rand.PartideExistente == rand.PartideRecalculate && rand.PartideDiferite == 0
                && rand.DiferentaRest == 0m);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            // Coruperea unui `Rest` trebuie să fie VĂZUTĂ: un raport care tace pe o
            // partidă greșită n-ar proba nimic.
            var partida = os.GetObjectsQuery<PartidaDeschisa>()
                .First(p => p.An == An && p.Luna == 1 && p.DocumentId == idFclB);
            partida.Rest += 5m;
            os.CommitChanges();
            var raport = SolduriService.Reconstruieste(os);
            var rand = raport.Referinte.Single(r => r.An == An && r.Luna == 1);
            Console.WriteLine($"     MĂSURAT (PAR-V19/{eticheta}): după coruperea unui rest — diferite "
                + $"{rand.PartideDiferite}, Δrest {rand.DiferentaRest:0.00}.");
            s.Check($"PAR-V19 ({eticheta}) un `Rest` corupt e RAPORTAT (o partidă diferită, Δ 5,00) și apoi rescris — "
                + "raportul se ia pe ce era în bază, rescrierea vine după el",
                rand.PartideDiferite == 1 && rand.DiferentaRest == 5m
                && Partide(os, An, 1)[idFclB] == 230m);
        }

        // ═════════════════════ soldurile pe partener ═════════════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            var solduri = ContabilProiectii.SoldParteneri(os, Zi(1, 31), null, idClient).ToList();
            var balanta = ContabilProiectii
                .Balanta(os, new DateOnly(An, 1, 1), Zi(1, 31), analitic: true, repartitorId: idClient)
                .ToList();
            var dinSolduri = solduri
                .Select(s => $"{s.ContId}:{s.SoldDebitor:0.00}/{s.SoldCreditor:0.00}")
                .OrderBy(x => x, StringComparer.Ordinal).ToList();
            var dinBalanta = balanta
                .Where(b => b.SoldFinalDebit != 0m || b.SoldFinalCredit != 0m)
                .Select(b => $"{b.ContId}:{b.SoldFinalDebit:0.00}/{b.SoldFinalCredit:0.00}")
                .OrderBy(x => x, StringComparer.Ordinal).ToList();
            var creanta = solduri.Sum(s => s.SoldDebitor) - solduri.Sum(s => s.SoldCreditor);
            Console.WriteLine($"     MĂSURAT (PAR-V20/{eticheta}): {solduri.Count} rânduri de sold pe clientul "
                + $"scenei la 31.01, net {creanta:0.00}; balanța analitică dă {dinBalanta.Count} rânduri.");
            s.Check($"PAR-V20 ({eticheta}) `SoldParteneri` la o dată e EXACT partea de sold a balanței analitice pe "
                + "aceeași cheie (cont × repartitor), la cent și la rând — nu un al doilea adevăr",
                dinSolduri.SequenceEqual(dinBalanta));
            s.Check($"PAR-V21 ({eticheta}) dimensiunea Partener este distinctă de Gestiune (D8-B1): "
                + "creanța urmărită are D 350/C 120, net debitor 230",
                solduri.Count == 1 && solduri.Sum(s => s.Credit) == 120m
                    && solduri.Sum(s => s.Debit) == 350m && creanta == 230m);
        }

        // ═════════════════════ ReturClient: totalul filtrat prin hook ═════════
        // Doar pe privat: returul de la client cere tipurile planului OMFP (371 de
        // stoc, 707 de venit) și politica lui de TVA.
        if (privat)
            using (var os = s.Provider.CreateObjectSpace()) {
                var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
                var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");
                var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
                var client = os.GetObjectByKey<Repartitor>(idClient);

                var gest = os.CreateObject<Gestiune>();
                gest.Cod = Marcaj + "-G";
                gest.Denumire = "Gestiune partide";
                var furnizor = os.CreateObject<Partener>();
                furnizor.Cod = Marcaj + "-F";
                furnizor.Denumire = "Furnizor partide";
                furnizor.CodFiscal = "RO33333342";
                var produs = os.CreateObject<Produs>();
                produs.Cod = Marcaj + "-P";
                produs.Denumire = "Marfă partide";
                produs.UM = "BUC";
                produs.TipMaterial = tip371;
                os.CommitChanges();

                // Lotul original al livrării pe care returul o stornează.
                var nir = os.CreateObject<NIR>();
                nir.Data = Zi(2, 25);
                nir.DataInregistrare = Zi(2, 25);
                nir.Predator = furnizor;
                nir.Primitor = gest;
                var linNir = os.CreateObject<DocumentDetaliu>();
                linNir.Document = nir;
                linNir.TipMaterial = tip371;
                linNir.Cantitate = 10m;
                linNir.Valoare = 100m;
                var lot = linNir.CreeazaLot(os, produs, gest);
                os.CommitChanges();
                MotorOperare.Opereaza(os, nir);
                os.CommitChanges();

                // RDC: DOUĂ roluri într-un document — venit stornat (fără lot) și
                // cost (pe lotul original). `LiniiCreanta` taie al doilea rol.
                var rdc = os.CreateObject<ReturClient>();
                rdc.Data = Zi(2, 26);
                rdc.DataInregistrare = Zi(2, 26);
                rdc.Predator = client;
                rdc.Primitor = gest;
                var lVenit = os.CreateObject<DocumentDetaliu>();
                lVenit.Document = rdc;
                lVenit.TipMaterial = tip707;
                lVenit.Cantitate = 1m;
                lVenit.Valoare = 100m;
                lVenit.TipTvaId = n21.ID;
                var lCost = os.CreateObject<DocumentDetaliu>();
                lCost.Document = rdc;
                lCost.TipMaterial = tip371;
                lCost.LotId = lot.ID;
                lCost.Cantitate = 3m;
                os.CommitChanges();
                MotorOperare.Opereaza(os, rdc);
                os.CommitChanges();

                var brut = os.GetObjectsQuery<DocumentDetaliu>()
                    .Where(d => d.DocumentId == rdc.ID)
                    .Select(d => (decimal?)(d.Valoare + d.ValoareTva)).Sum() ?? 0m;
                var creantaRdc = SumaCreanta(os, rdc.ID);
                var apare = ImperecheriProiectii.DocumenteCuRest(os).Any(r => r.DocumentId == rdc.ID);
                Console.WriteLine($"     MĂSURAT (PAR-V22/{eticheta}): RDC {rdc.Numar} — total scris "
                    + $"{Bani(rdc.TotalStingere ?? 0m)}, Σ linii de creanță {Bani(creantaRdc)}, Σ TOATE liniile "
                    + $"{Bani(brut)}; în `DocumenteCuRest`: {apare}.");
                s.Check($"PAR-V22/SC-CIT-64 ({eticheta}): RDC are totalul de stins 121 din cub, fără costul 30; liniile de creanță rămân −121",
                    creantaRdc == -121m && brut == -151m
                    && ImperechereService.Total(os, rdc.ID) == 121m);
                var candidatRdc = ImperecheriProiectii.DocumenteCuRest(os).SingleOrDefault(r => r.DocumentId == rdc.ID);
                s.Check($"PAR-V23/SC-CIT-64 ({eticheta}): returul cu partidă proprie este datorie 121 în raport",
                    apare && candidatRdc?.Rest == 121m && candidatRdc.Sens == "Datorie");

            }

        // ═════════════════════ curățenia: scena nu rămâne în bază ═════════════
        using (var os = s.Provider.CreateObjectSpace()) {
            s.RedeschideLant(os, An, 1);
            s.PurjaIstoricPerioade(os, An);
        }
        using (var os = s.Provider.CreateObjectSpace())
            CurataPar(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check($"PAR-V24 ({eticheta}) scena nu lasă reziduu: anul {An} e din nou gol, nicio perioadă a bazei "
                + "nu rămâne închisă și nicio partidă nu supraviețuiește",
                os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An) == 0
                && !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.Inchisa)
                && os.GetObjectsQuery<PartidaDeschisa>().Count() == 0
                && os.GetObjectsQuery<Document>().Count(d =>
                    d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31)) == 0);
        }
    }
}
