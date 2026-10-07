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

// Review advers pasul 7 (felia 27, F27-D2) — probele F27-RA*, scena 2037. Blocul
// coboară `ItvLipsa` la Avertisment pe durata scenei (ca `InchideAcceptTot`) și
// pune totul la loc în `finally`; NU atinge alte politici decât cele probate.
static class VerificaReviewAcceptare {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-RA";
        const int An = 2037;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipVenit = privat ? "704" : "751.01.00";
        var codTipF = privat ? "214" : "214.00.00";
        var codContCasa = privat ? "5311" : "531.01.01";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        void CurataRa(IObjectSpace os) {
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            SolduriService.Elimina(os, An - 1, 12);
            var pj = new Purja(os);
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruImobilizari>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            foreach (var imp in os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentId) || docIds.Contains(i.DocumentStingatorId))
                    .OrderByDescending(i => i.InverseazaId != null))
                pj.Adauga(imp);
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>()
                    .Where(d => docIds.Contains(d.ID)).OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Imobilizare>()
                .Where(f => f.NumarInventar.StartsWith(Marcaj)).ToList());
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An || (p.An == An - 1 && p.Luna == 12)).Select(p => p.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => perioadeIds.Contains(p.ID)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        SeveritateConstatare? Sev(FelConstatareInchidere fel) {
            using var os = s.Provider.CreateObjectSpace();
            return os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == fel)?.Severitate;
        }
        void Politica(FelConstatareInchidere fel, SeveritateConstatare severitate) {
            using var os = s.Provider.CreateObjectSpace();
            var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == fel);
            if (rand == null) {
                rand = os.CreateObject<PoliticaInchidere>();
                rand.Fel = fel;
                rand.DinSeed = true;
            }
            rand.Severitate = severitate;
            os.CommitChanges();
        }
        List<PerioadaService.ConstatareInchidere> Constatari(int an, int luna) {
            using var os = s.Provider.CreateObjectSpace();
            return PerioadaService.Verifica(os, an, luna).ToList();
        }
        PerioadaService.ConstatareInchidere Constatare(int an, int luna, string prefix) =>
            Constatari(an, luna).FirstOrDefault(c => c.Cheie == prefix || c.Cheie.StartsWith(prefix + ":"));
        InchiderePerioada InchideTot(IObjectSpace os, int an, int luna) =>
            PerioadaService.Inchide(os, an, luna,
                PerioadaService.Verifica(os, an, luna).Select(c => c.Cheie).ToArray(), null, Marcaj);
        static string LiniaCheii(string refuz, string cheie) =>
            refuz?.Split('\n').FirstOrDefault(l => l.Contains("[" + cheie + "]"));

        var itvSeed = Sev(FelConstatareInchidere.ItvLipsa);
        var amoSeed = Sev(FelConstatareInchidere.AmoLipsa);
        var draftSeed = Sev(FelConstatareInchidere.DraftInPerioada);
        var restSeed = Sev(FelConstatareInchidere.RestScadent);
        try {
            using (var os = s.Provider.CreateObjectSpace())
                CurataRa(os);
            Politica(FelConstatareInchidere.ItvLipsa, SeveritateConstatare.Avertisment);

            // ── scena ──
            Guid idClient, idUnitate, idCasa, idCodEc, idTipVenit, idTipTrz, idN21;
            using (var os = s.Provider.CreateObjectSpace()) {
                var precedenta = os.CreateObject<PerioadaFiscala>();
                precedenta.An = An - 1;
                precedenta.Luna = 12;
                foreach (var luna in new[] { 1, 2, 3 }) {
                    var p = os.CreateObject<PerioadaFiscala>();
                    p.An = An;
                    p.Luna = luna;
                }
                var tipVenit = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipVenit);
                var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
                var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
                var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
                var client = os.CreateObject<Partener>();
                client.Cod = Marcaj + "-CL";
                client.Denumire = "Client review";
                client.CodFiscal = "RO33333343";
                var unitate = os.CreateObject<UnitateInterna>();
                unitate.Cod = Marcaj + "-UI";
                unitate.Denumire = "Unitate review";
                var gest = os.CreateObject<Gestiune>();
                gest.Cod = Marcaj + "-G";
                gest.Denumire = "Gestiune review";
                var casa = os.CreateObject<ContPropriu>();
                casa.Cod = Marcaj + "-CS";
                casa.Denumire = "Casa review";
                casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == codContCasa);
                var codEc = os.CreateObject<CodEconomic>();
                codEc.Cod = Marcaj + "-CE";
                codEc.Denumire = "Cod economic review";
                var fisa = os.CreateObject<Imobilizare>();
                fisa.NumarInventar = Marcaj + "-FISA";
                fisa.Denumire = "Fișă review";
                fisa.TipMaterialId = tipF.ID;
                fisa.LocId = gest.ID;
                fisa.CodEconomicId = codEc.ID;
                os.CommitChanges();

                var pif = os.CreateObject<PunereInFunctiune>();
                pif.Data = new DateOnly(An - 1, 12, 5);
                pif.DataInregistrare = new DateOnly(An - 1, 12, 5);
                pif.PredatorId = unitate.ID;
                pif.PrimitorId = gest.ID;
                var linPif = os.CreateObject<PunereInFunctiuneDetaliu>();
                linPif.Document = pif;
                linPif.ImobilizareId = fisa.ID;
                linPif.TipMaterialId = tipF.ID;
                linPif.Fel = FelLiniePif.Intrare;
                linPif.Valoare = 3600m;
                linPif.Cantitate = 1m;
                linPif.Metoda = MetodaAmortizare.Liniara;
                linPif.DurataLuni = 36;
                linPif.MetodaFiscala = MetodaAmortizare.Liniara;
                linPif.DurataFiscalaLuni = 36;
                linPif.CategorieFiscala = CategorieFiscala.Standard;
                linPif.UtilizareExclusiva = true;
                os.CommitChanges();
                ImoFixture.OpereazaCuSuport(os, pif);
                os.CommitChanges();
                idClient = client.ID; idUnitate = unitate.ID; idCasa = casa.ID; idCodEc = codEc.ID;
                idTipVenit = tipVenit.ID; idTipTrz = tipTrz.ID; idN21 = n21?.ID ?? Guid.Empty;
            }

            // ── RA3a: AMO-LIPSA pe luna PIF-ului (nicio fișă eligibilă) ──
            s.Check($"F27-RA3a ({eticheta}) luna PIF-ului (12/{An - 1}) n-are fișe eligibile (PIF < prima zi a lunii "
                + "cere luna următoare), deci nu emite `AMO-LIPSA`",
                Constatare(An - 1, 12, "AMO-LIPSA") == null);
            using (var os = s.Provider.CreateObjectSpace())
                InchideTot(os, An - 1, 12);

            // ── RA3b…d: AMO în stările Draft / Operat / Stornat ──
            Guid idAmo;
            using (var os = s.Provider.CreateObjectSpace()) {
                var rez = AmoApply.Genereaza(os, new GenerareAmoRequestDto { An = An, Luna = 1, UnitateId = idUnitate });
                idAmo = rez.DocumentId ?? Guid.Empty;
            }
            var amoDraft = Constatare(An, 1, "AMO-LIPSA");
            Console.WriteLine($"     MĂSURAT (F27-RA3b/{eticheta}): AMO draft ⇒ „{amoDraft?.Text}”.");
            s.Check($"F27-RA3b ({eticheta}) AMO DRAFT ⇒ constatarea rămâne, cu textul de draft și obiectul draftului",
                idAmo != Guid.Empty && amoDraft != null && amoDraft.Text.Contains("DRAFT") && amoDraft.ObiectId == idAmo);
            var drafturiIan = Constatari(An, 1)
                .Count(c => c.Fel == nameof(FelConstatareInchidere.DraftInPerioada));
            s.Check($"F27-RA6a ({eticheta}) draftul AMO raportat de familia lui NU se mai repetă ca "
                + "`DRAFT-IN-PERIOADA` — un fapt, o constatare — și nu intră nici în numărătoarea familiei",
                amoDraft?.ObiectId == idAmo && drafturiIan == 0
                && Constatare(An, 1, $"DRAFT-IN-PERIOADA:{idAmo}") == null);
            Politica(FelConstatareInchidere.AmoLipsa, SeveritateConstatare.Ignorat);
            s.Check($"F27-RA6c ({eticheta}) cu felul `AmoLipsa` pe `Ignorat` (nu se caută, deci nu se raportează) "
                + "draftul AMO apare ca `DRAFT-IN-PERIOADA`: excluderea urmează raportarea EFECTIVĂ, nu tipul "
                + "documentului",
                Constatare(An, 1, "AMO-LIPSA") == null
                && Constatare(An, 1, $"DRAFT-IN-PERIOADA:{idAmo}") != null);
            if (amoSeed != null)
                Politica(FelConstatareInchidere.AmoLipsa, amoSeed.Value);
            using (var os = s.Provider.CreateObjectSpace())
                ComenziDocument.Sistem(os).Opereaza(idAmo);
            s.Check($"F27-RA3c ({eticheta}) AMO Operat ⇒ constatarea dispare", Constatare(An, 1, "AMO-LIPSA") == null);
            using (var os = s.Provider.CreateObjectSpace()) {
                MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(idAmo), Zi(1, 31));
                os.CommitChanges();
            }
            var amoStornat = Constatare(An, 1, "AMO-LIPSA");
            s.Check($"F27-RA3d ({eticheta}) AMO Stornat ⇒ constatarea REAPARE ca „lipsește” (stornatul nu e o "
                + "amortizare vie)",
                amoStornat != null && amoStornat.Text.Contains("lipsește"));
            using (var os = s.Provider.CreateObjectSpace()) {
                var rez = AmoApply.Genereaza(os, new GenerareAmoRequestDto { An = An, Luna = 1, UnitateId = idUnitate });
                ComenziDocument.Sistem(os).Opereaza(rez.DocumentId ?? Guid.Empty);
            }

            // ── RA1: acceptarea pe o fotografie veche ──
            Guid idD1, idD2;
            using (var os = s.Provider.CreateObjectSpace()) {
                var d1 = os.CreateObject<NotaContabila>();
                d1.Numar = Marcaj + "-D1";
                d1.Data = Zi(1, 10);
                d1.DataInregistrare = Zi(1, 10);
                d1.PredatorId = idUnitate;
                d1.PrimitorId = idUnitate;
                os.CommitChanges();
                idD1 = d1.ID;
            }
            var fotografie = Constatari(An, 1).Select(c => c.Cheie).ToArray();
            using (var os = s.Provider.CreateObjectSpace()) {
                var d2 = os.CreateObject<NotaContabila>();
                d2.Numar = Marcaj + "-D2";
                d2.Data = Zi(1, 20);
                d2.DataInregistrare = Zi(1, 20);
                d2.PredatorId = idUnitate;
                d2.PrimitorId = idUnitate;
                os.CommitChanges();
                idD2 = d2.ID;
            }
            var cheieD2 = $"DRAFT-IN-PERIOADA:{idD2}";
            using (var os = s.Provider.CreateObjectSpace()) {
                var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 1, fotografie, null, Marcaj));
                var linia = LiniaCheii(refuz, cheieD2);
                Console.WriteLine($"     MĂSURAT (F27-RA1a/{eticheta}): draftul apărut DUPĂ fotografie ⇒ „{linia}”.");
                s.Check($"F27-RA1a ({eticheta}) un draft apărut între verificare și închidere refuză închiderea cu "
                    + "cheia lui, deși toate cheile fotografiei sunt acceptate",
                    fotografie.Contains($"DRAFT-IN-PERIOADA:{idD1}") && linia != null && linia.StartsWith("Avertisment:"));
            }
            using (var os = s.Provider.CreateObjectSpace()) {
                os.Delete(os.GetObjectByKey<Document>(idD2));
                os.CommitChanges();
            }
            using (var os = s.Provider.CreateObjectSpace()) {
                var rand = PerioadaService.Inchide(os, An, 1, fotografie.Append(cheieD2).ToArray(), null, Marcaj);
                Console.WriteLine($"     MĂSURAT (F27-RA1b/{eticheta}): acceptări scrise = {rand.Acceptari}.");
                s.Check($"F27-RA1b ({eticheta}) draftul șters (logic) între timp ⇒ cheia lui acceptată se ignoră și "
                    + "nu ajunge în istoric; luna se închide pe cheile care mai există",
                    rand.Acceptari != null && rand.Acceptari.Contains(idD1.ToString())
                    && !rand.Acceptari.Contains(idD2.ToString()));
            }

            // ── RA2: ITV în stările Draft / Operat / Stornat / operat pe altă lună (privat) ──
            Guid idFclT;
            using (var os = s.Provider.CreateObjectSpace()) {
                var fcl = os.CreateObject<FacturaIesire>();
                fcl.Numar = Marcaj + "-FCL-T";
                fcl.Data = Zi(2, 15);
                fcl.DataInregistrare = Zi(2, 15);
                fcl.DataScadenta = Zi(2, 20);
                fcl.PredatorId = idUnitate;
                fcl.PrimitorId = idClient;
                var lin = os.CreateObject<FacturaIesireDetaliu>();
                lin.Document = fcl;
                lin.TipMaterialId = idTipVenit;
                lin.Cantitate = 1m;
                lin.PretUnitar = 1000m;
                lin.CodEconomicId = idCodEc;
                if (idN21 != Guid.Empty)
                    lin.TipTvaId = idN21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fcl);
                os.CommitChanges();
                idFclT = fcl.ID;
            }
            if (privat) {
                var lipsa = Constatare(An, 2, "ITV-LIPSA");
                s.Check($"F27-RA2a ({eticheta}) luna cu TVA colectată și fără ITV ⇒ „lipsește”",
                    lipsa != null && lipsa.Text.Contains("lipsește"));
                Guid idItv;
                using (var os = s.Provider.CreateObjectSpace()) {
                    var rez = InchidereTvaApply.Genereaza(os, new GenerareItvRequestDto { An = An, Luna = 2, UnitateId = idUnitate });
                    idItv = rez.DocumentId ?? Guid.Empty;
                }
                var draft = Constatare(An, 2, "ITV-LIPSA");
                s.Check($"F27-RA2b ({eticheta}) ITV DRAFT ⇒ textul de draft, obiectul = draftul",
                    idItv != Guid.Empty && draft != null && draft.Text.Contains("DRAFT") && draft.ObiectId == idItv);
                var drafturiFeb = Constatari(An, 2)
                    .Count(c => c.Fel == nameof(FelConstatareInchidere.DraftInPerioada));
                s.Check($"F27-RA6b ({eticheta}) draftul ITV raportat de familia lui NU se mai repetă ca "
                    + "`DRAFT-IN-PERIOADA` și nu intră nici în numărătoarea familiei",
                    draft?.ObiectId == idItv && drafturiFeb == 0
                    && Constatare(An, 2, $"DRAFT-IN-PERIOADA:{idItv}") == null);
                using (var os = s.Provider.CreateObjectSpace())
                    ComenziDocument.Sistem(os).Opereaza(idItv);
                s.Check($"F27-RA2c ({eticheta}) ITV Operat ⇒ constatarea dispare", Constatare(An, 2, "ITV-LIPSA") == null);
                using (var os = s.Provider.CreateObjectSpace()) {
                    MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(idItv), Zi(2, 28));
                    os.CommitChanges();
                }
                var stornat = Constatare(An, 2, "ITV-LIPSA");
                s.Check($"F27-RA2d ({eticheta}) ITV Stornat ⇒ „lipsește” din nou",
                    stornat != null && stornat.Text.Contains("lipsește"));
                Guid idItvMar;
                using (var os = s.Provider.CreateObjectSpace()) {
                    var rez = InchidereTvaApply.Genereaza(os, new GenerareItvRequestDto { An = An, Luna = 3, UnitateId = idUnitate });
                    idItvMar = rez.DocumentId ?? Guid.Empty;
                    if (idItvMar != Guid.Empty)
                        ComenziDocument.Sistem(os).Opereaza(idItvMar);
                }
                MotivNegenerare? motivFeb;
                using (var os = s.Provider.CreateObjectSpace())
                    motivFeb = InchidereTvaService.Previzualizeaza(os, An, 2).Motiv;
                var necron = Constatare(An, 2, "ITV-LIPSA");
                Console.WriteLine($"     MĂSURAT (F27-RA2e/{eticheta}): ITV martie operat, februarie fără ⇒ motiv "
                    + $"{motivFeb}, constatare „{necron?.Text}”.");
                s.Check($"F27-RA2e ({eticheta}) ITV operat pe luna URMĂTOARE, februarie fără ⇒ februarie tot „lipsește” "
                    + "(previzualizarea spune NeCronologica, constatarea nu se pierde)",
                    idItvMar != Guid.Empty && motivFeb == MotivNegenerare.NeCronologica
                    && necron != null && necron.Text.Contains("lipsește"));
                using (var os = s.Provider.CreateObjectSpace()) {
                    MotorOperare.Storneaza(os, os.GetObjectByKey<Document>(idItvMar), Zi(3, 31));
                    os.CommitChanges();
                }
            }

            // ── RA4: REST-SCADENT — scadența lipsă, la ultima zi, stinsă la ultima zi, după P ──
            Politica(FelConstatareInchidere.RestScadent, SeveritateConstatare.Avertisment);
            Guid idA, idB, idC, idD;
            using (var os = s.Provider.CreateObjectSpace()) {
                FacturaIesire Fcl(string sufix, int zi, DateOnly? scadenta) {
                    var f = os.CreateObject<FacturaIesire>();
                    f.Numar = Marcaj + sufix;
                    f.Data = Zi(2, zi);
                    f.DataInregistrare = Zi(2, zi);
                    f.DataScadenta = scadenta;
                    f.PredatorId = idUnitate;
                    f.PrimitorId = idClient;
                    var l = os.CreateObject<FacturaIesireDetaliu>();
                    l.Document = f;
                    l.TipMaterialId = idTipVenit;
                    l.Cantitate = 1m;
                    l.PretUnitar = 500m;
                    l.CodEconomicId = idCodEc;
                    if (idN21 != Guid.Empty)
                        l.TipTvaId = idN21;
                    return f;
                }
                var a = Fcl("-FCL-A", 10, null);
                var b = Fcl("-FCL-B", 10, Zi(2, 28));
                var c = Fcl("-FCL-C", 10, Zi(2, 15));
                var d = Fcl("-FCL-D", 10, Zi(3, 5));
                os.CommitChanges();
                foreach (var f in new[] { a, b, c, d })
                    MotorOperare.Opereaza(os, f);
                os.CommitChanges();
                var inc = os.CreateObject<Incasare>();
                inc.Data = Zi(2, 28);
                inc.DataInregistrare = Zi(2, 28);
                inc.PredatorId = idClient;
                inc.PrimitorId = idCasa;
                inc.TipInstrument = TipInstrumentPlata.Chitanta;
                var li = os.CreateObject<DocumentTrezorerieDetaliu>();
                li.Document = inc;
                li.TipMaterialId = idTipTrz;
                li.Valoare = ImperechereService.Ramas(os, c.ID);
                li.CodEconomicId = idCodEc;
                os.CommitChanges();
                MotorOperare.Opereaza(os, inc);
                os.CommitChanges();
                ImperechereService.Imperecheaza(os, inc, c, li.Valoare, null, Zi(2, 28));
                idA = a.ID; idB = b.ID; idC = c.ID; idD = d.ID;
            }
            var restante = Constatari(An, 2).Where(c => c.Fel == nameof(FelConstatareInchidere.RestScadent))
                .Select(c => c.ObiectId).ToHashSet();
            Console.WriteLine($"     MĂSURAT (F27-RA4/{eticheta}): REST-SCADENT pe "
                + $"A={restante.Contains(idA)} B={restante.Contains(idB)} C={restante.Contains(idC)} "
                + $"D={restante.Contains(idD)} T={restante.Contains(idFclT)}.");
            s.Check($"F27-RA4 ({eticheta}) fără scadență ⇒ nu; scadentă la ultima zi și neîncasată ⇒ da; stinsă "
                + "integral exact la ultima zi ⇒ nu; scadentă după P ⇒ nu; factura din 15.02 cu scadența 20.02 ⇒ da",
                !restante.Contains(idA) && restante.Contains(idB) && !restante.Contains(idC)
                && !restante.Contains(idD) && restante.Contains(idFclT));
            Politica(FelConstatareInchidere.RestScadent, SeveritateConstatare.Ignorat);

            // ── RA7: `acceptate` null ⇒ ca lista goală, refuz de domeniu, nu excepție de cod ──
            using (var os = s.Provider.CreateObjectSpace()) {
                string refuz;
                try { PerioadaService.Inchide(os, An, 2, null, null, Marcaj); refuz = null; }
                catch (OperareException e) { refuz = e.Message; }
                catch (Exception e) { refuz = "EXCEPȚIE DE COD: " + e.GetType().Name; }
                s.Check($"F27-RA7 ({eticheta}) `acceptate = null` e tratat ca listă goală: refuz de domeniu cu lista, nu "
                    + "excepție de cod",
                    refuz != null && refuz.StartsWith("Avertisment:"));
            }

            // ── RA8: rândul de politică ȘTERS ⇒ „fără politică” ──
            using (var os = s.Provider.CreateObjectSpace()) {
                os.Delete(os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.RestScadent));
                os.CommitChanges();
            }
            var faraPolitica = Constatare(An, 2, "REST-SCADENT");
            s.Check($"F27-RA8 ({eticheta}) rândul de politică șters ⇒ felul se caută și iese ca avertisment „fără politică”",
                faraPolitica != null && faraPolitica.Text.Contains("fără politică"));
            using (var os = s.Provider.CreateObjectSpace()) {
                new Purja(os).Adauga(os.GetObjectsQuery<PoliticaInchidere>()
                    .Where(p => p.Fel == FelConstatareInchidere.RestScadent).ToList()).Executa();
            }
            Politica(FelConstatareInchidere.RestScadent, SeveritateConstatare.Ignorat);

            // ── RA9: severitate în afara enum-ului ⇒ gardianul generic o refuză ──
            using (var os = s.Provider.CreateObjectSpace()) {
                var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.AmoLipsa);
                rand.Severitate = (SeveritateConstatare)7;
                var refuz = s.Refuz(() => GardianEditare.Verifica(os));
                s.Check($"F27-RA9 ({eticheta}) `Severitate` = 7 (nedefinită) e refuzată de gardianul de enum-uri înainte "
                    + "de commit", refuz != null);
            }

            using (var os = s.Provider.CreateObjectSpace())
                InchideTot(os, An, 2);

            // ── RA5: plafonul de 200 și rândul de REZUMAT ca acceptare în bloc ──
            using (var os = s.Provider.CreateObjectSpace()) {
                for (var i = 0; i < 201; i++) {
                    var d = os.CreateObject<NotaContabila>();
                    d.Numar = $"{Marcaj}-M{i:000}";
                    d.Data = Zi(3, 1 + i % 28);
                    d.DataInregistrare = Zi(3, 1 + i % 28);
                    d.PredatorId = idUnitate;
                    d.PrimitorId = idUnitate;
                }
                os.CommitChanges();
            }
            var martie = Constatari(An, 3);
            var listate = martie.Count(c => c.Fel == nameof(FelConstatareInchidere.DraftInPerioada)
                && !c.Cheie.EndsWith(":REZUMAT"));
            var rezumat = martie.FirstOrDefault(c => c.Cheie == "DRAFT-IN-PERIOADA:REZUMAT");
            Console.WriteLine($"     MĂSURAT (F27-RA5a/{eticheta}): {listate} drafturi listate, rezumat = "
                + $"„{rezumat?.Text}” / {rezumat?.Severitate}.");
            s.Check($"F27-RA5a ({eticheta}) 201 drafturi ⇒ 200 listate + rândul de rezumat, cu cheie proprie, cu "
                + "severitatea din politică (Avertisment) și cu cifra celor nelistate în text",
                listate == 200 && rezumat != null && rezumat.Severitate == SeveritateConstatare.Avertisment
                && rezumat.Text.Contains("Încă 1 rând de același fel (din 201 în total)")
                && rezumat.Text.Contains("acceptarea acestui rând le acceptă pe toate"));
            Politica(FelConstatareInchidere.DraftInPerioada, SeveritateConstatare.Blocant);
            var rezumatBlocant = Constatare(An, 3, "DRAFT-IN-PERIOADA:REZUMAT");
            string liniaBlocant;
            using (var os = s.Provider.CreateObjectSpace()) {
                var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 3,
                    PerioadaService.Verifica(os, An, 3).Select(c => c.Cheie).ToArray(), null, Marcaj));
                liniaBlocant = LiniaCheii(refuz, "DRAFT-IN-PERIOADA:REZUMAT");
            }
            Console.WriteLine($"     MĂSURAT (F27-RA5b/{eticheta}): rezumatul pe fel BLOCANT ⇒ „{liniaBlocant}”.");
            s.Check($"F27-RA5b ({eticheta}) cu felul pe BLOCANT rezumatul devine el însuși blocant, iar închiderea e "
                + "refuzată chiar cu cheia lui acceptată — severitatea rezumatului urmează politica familiei",
                rezumatBlocant != null && rezumatBlocant.Severitate == SeveritateConstatare.Blocant
                && liniaBlocant != null && liniaBlocant.StartsWith("Blocant:"));
            Politica(FelConstatareInchidere.DraftInPerioada, SeveritateConstatare.Avertisment);
            string liniaFaraRezumat;
            using (var os = s.Provider.CreateObjectSpace()) {
                var fara = PerioadaService.Verifica(os, An, 3).Select(c => c.Cheie)
                    .Where(c => c != "DRAFT-IN-PERIOADA:REZUMAT").ToArray();
                var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 3, fara, null, Marcaj));
                liniaFaraRezumat = LiniaCheii(refuz, "DRAFT-IN-PERIOADA:REZUMAT");
            }
            using (var os = s.Provider.CreateObjectSpace()) {
                var rand = PerioadaService.Inchide(os, An, 3,
                    PerioadaService.Verifica(os, An, 3).Select(c => c.Cheie).ToArray(), null, Marcaj);
                s.Check($"F27-RA5c ({eticheta}) rândul de rezumat e o acceptare ÎN BLOC a celor nelistate: neacceptat "
                    + "refuză închiderea cu linia lui, acceptat o permite și intră în istoric",
                    liniaFaraRezumat != null && rand.Fel == FelInchiderePerioada.Inchidere
                    && rand.Acceptari.Contains(":REZUMAT"));
            }
        }
        finally {
            if (itvSeed != null)
                Politica(FelConstatareInchidere.ItvLipsa, itvSeed.Value);
            if (amoSeed != null)
                Politica(FelConstatareInchidere.AmoLipsa, amoSeed.Value);
            if (draftSeed != null)
                Politica(FelConstatareInchidere.DraftInPerioada, draftSeed.Value);
            if (restSeed != null)
                Politica(FelConstatareInchidere.RestScadent, restSeed.Value);
            using var os = s.Provider.CreateObjectSpace();
            CurataRa(os);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>()
                .Count(p => p.An == An || (p.An == An - 1 && p.Luna == 12));
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31));
            var politici = os.GetObjectsQuery<PoliticaInchidere>().Select(p => new { p.Fel, p.Severitate, p.DinSeed }).ToList();
            s.Check($"F27-RA10 ({eticheta}) fără reziduu și politica exact ca înainte de scenă",
                perioade == 0 && documente == 0 && politici.Count == 4 && politici.All(p => p.DinSeed)
                && politici.Single(p => p.Fel == FelConstatareInchidere.ItvLipsa).Severitate == itvSeed
                && politici.Single(p => p.Fel == FelConstatareInchidere.RestScadent).Severitate == restSeed);
        }
    }
}
