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

// ═══════════════════════════════════════════════════════════════════════════
// Felia 27, pasul 6 — totalul scris la operare, partidele deschise la închidere,
// împerecherea ca fapt datat (F27-D7/D8). Scena stă în 2035, în afara tuturor
// celorlalte scene ale suitei.
//
// Ce se probează, în ordinea în care se citește regula:
//   * `TotalStingere` e scris de motor la operare și e Σ `LiniiCreanta` — inclusiv
//     pe `ReturClient`, unde filtrul taie liniile de cost;
//   * partidele deschise ale unei perioade DE REFERINȚĂ = restul fiecărui
//     document operat la sfârșitul ei, iar `DocumenteCuRest` citit prin ele e
//     IDENTIC cu cel citit integral („identic cu și fără partide");
//   * împerecherea e datată: ordinea față de înregistrare, perioada deschisă,
//     ștergerea doar în fereastra deschisă, desfacerea prin rând invers, și
//     inversarea automată la stornarea unui document cu stingeri închise.
// ══════════════════════════════════════════════════════════════════════════
// Felia 27, pasul 7 (F27-D2): constatările de CONȚINUT ale închiderii și
// acceptarea conștientă. Scena e a anului 2036 (plus 12/2035, ca fișa să aibă
// o punere în funcțiune dinaintea lunii probate).
// ══════════════════════════════════════════════════════════════════════════
static class VerificaAcceptare {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-ACC";
        const int An = 2036;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipVenit = privat ? "704" : "751.01.00";
        var codTipF = privat ? "214" : "214.00.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        // ─────────── curățenia de scenă (purjă FIZICĂ, F13-D2) ───────────
        void CurataAcc(IObjectSpace os) {
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

        // Severitatea unui fel, scrisă pe ObjectSpace PROPRIU (politica e stare a
        // bazei, iar scena care o schimbă are de obicei modificări necomise).
        void Politica(FelConstatareInchidere fel, SeveritateConstatare severitate) {
            using var os = s.Provider.CreateObjectSpace();
            var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == fel);
            rand.Severitate = severitate;
            os.CommitChanges();
        }

        string RefuzGardianAcc(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        List<PerioadaService.ConstatareInchidere> ConstatariLuna(int luna) {
            using var os = s.Provider.CreateObjectSpace();
            return PerioadaService.Verifica(os, An, luna).ToList();
        }

        PerioadaService.ConstatareInchidere Constatare(int luna, string prefixCheie) =>
            ConstatariLuna(luna).FirstOrDefault(c => c.Cheie == prefixCheie
                || c.Cheie.StartsWith(prefixCheie + ":"));

        // Linia refuzului care poartă o cheie anume: refuzul e o listă, iar proba
        // trebuie să spună CARE constatare a blocat, nu doar că textul o conține.
        static string LiniaCheii(string refuz, string cheie) =>
            refuz?.Split('\n').FirstOrDefault(l => l.Contains("[" + cheie + "]"));

        using (var os = s.Provider.CreateObjectSpace())
            CurataAcc(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>()
                .Count(p => p.An == An || (p.An == An - 1 && p.Luna == 12));
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31));
            var inchise = os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.Inchisa);
            Console.WriteLine($"     MĂSURAT (ACC-V0/{eticheta}): {perioade} perioade și {documente} documente în "
                + $"12/{An - 1}–12/{An}, {inchise} perioade închise în bază.");
            s.Check($"ACC-V0 ({eticheta}) precondiție: fereastra 12/{An - 1}–12/{An} e liberă și nicio perioadă a "
                + "bazei nu e închisă — altfel constatările de mai jos ar fi măsurate peste conținut străin",
                perioade == 0 && documente == 0 && inchise == 0);
        }

        // ── ACC-V1: seed-ul politicii, pe cheia ei ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var randuri = os.GetObjectsQuery<PoliticaInchidere>()
                .Select(p => new { p.Fel, p.Severitate, p.DinSeed }).ToList()
                .ToDictionary(p => p.Fel, p => (p.Severitate, p.DinSeed));
            var itvAsteptat = privat ? SeveritateConstatare.Blocant : SeveritateConstatare.Ignorat;
            Console.WriteLine($"     MĂSURAT (ACC-V1/{eticheta}): "
                + string.Join(", ", randuri.Select(r => $"{r.Key}={r.Value.Severitate}"
                    + (r.Value.DinSeed ? "" : " (fără timbru)"))) + ".");
            s.Check($"ACC-V1 ({eticheta}) seed-ul aliniază cele patru feluri, toate cu timbrul `DinSeed`: `ItvLipsa` "
                + $"= {itvAsteptat} (privatul e plătitor de TVA, bugetarul are ITV inert), `AmoLipsa` și "
                + "`DraftInPerioada` = Avertisment, `RestScadent` = Ignorat — severitatea e DATE, iar cele două "
                + "profiluri diferă de CONȚINUT, nu de mecanism",
                randuri.Count == 4
                && randuri[FelConstatareInchidere.ItvLipsa] == (itvAsteptat, true)
                && randuri[FelConstatareInchidere.AmoLipsa] == (SeveritateConstatare.Avertisment, true)
                && randuri[FelConstatareInchidere.DraftInPerioada] == (SeveritateConstatare.Avertisment, true)
                && randuri[FelConstatareInchidere.RestScadent] == (SeveritateConstatare.Ignorat, true));
        }

        // ── scena: fișa pusă în funcțiune în 12/2035 (lună închisă) + factura lui ianuarie ──
        Guid idClient, idUnitate, idFcl, idCodEc;
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
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var client = os.CreateObject<Partener>();
            client.Cod = Marcaj + "-CL";
            client.Denumire = "Client acceptare";
            client.CodFiscal = "RO33333342";
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-UI";
            unitate.Denumire = "Unitate acceptare";
            var gest = os.CreateObject<Gestiune>();
            gest.Cod = Marcaj + "-G";
            gest.Denumire = "Gestiune acceptare";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic acceptare";
            var fisa = os.CreateObject<Imobilizare>();
            fisa.NumarInventar = Marcaj + "-FISA";
            fisa.Denumire = "Fișă acceptare";
            fisa.TipMaterialId = tipF.ID;
            fisa.LocId = gest.ID;
            // Bugetarul cere codul economic pe contul de cheltuială cu amortizarea
            // (DIM-2): fișa îl poartă, iar AMO îl copiază pe linie.
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

            // Luna precedentă se închide ca SETUP (accept-all), ca 01/2036 să fie
            // veriga următoare a lanțului, nu un capăt.
            s.InchideAcceptTot(os, An - 1, 12, Marcaj);

            var fcl = os.CreateObject<FacturaIesire>();
            fcl.Numar = Marcaj + "-FCL";
            fcl.Data = Zi(1, 15);
            fcl.DataInregistrare = Zi(1, 15);
            fcl.DataScadenta = Zi(1, 20);
            fcl.Predator = unitate;
            fcl.Primitor = client;
            var linFcl = os.CreateObject<FacturaIesireDetaliu>();
            linFcl.Document = fcl;
            linFcl.TipMaterial = tipVenit;
            linFcl.Cantitate = 1m;
            linFcl.PretUnitar = 1000m;
            linFcl.CodEconomicId = codEc.ID;
            if (n21 != null)
                linFcl.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fcl);
            os.CommitChanges();

            idClient = client.ID; idUnitate = unitate.ID; idFcl = fcl.ID; idCodEc = codEc.ID;
            s.Check($"ACC — precondiție de scenă ({eticheta}): PIF-ul din 12/{An - 1} e operat, luna precedentă e "
                + $"închisă și factura de 1.000 din 15.01.{An} e operată cu scadența în ianuarie",
                pif.Stare == StareDocument.Operat && fcl.Stare == StareDocument.Operat
                && os.FirstOrDefault<PerioadaFiscala>(p => p.An == An - 1 && p.Luna == 12).Inchisa);
        }

        // ── ACC-V2/V3/V4: ITV lipsă, blocantul care nu se acceptă ──
        var itvLipsa = Constatare(1, "ITV-LIPSA");
        Console.WriteLine($"     MĂSURAT (ACC-V2/{eticheta}): ITV-LIPSA = "
            + $"{itvLipsa?.Severitate.ToString() ?? "<absentă>"}.");
        if (privat) {
            s.Check($"ACC-V2 ({eticheta}) luna cu TVA colectată și fără închidere de TVA produce `ITV-LIPSA` "
                + "BLOCANT: severitatea vine din politică, nu din cod",
                itvLipsa != null && itvLipsa.Severitate == SeveritateConstatare.Blocant
                && itvLipsa.Fel == nameof(FelConstatareInchidere.ItvLipsa));

            using (var os = s.Provider.CreateObjectSpace()) {
                var chei = PerioadaService.Verifica(os, An, 1).Select(c => c.Cheie).ToArray();
                var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 1, chei, null, Marcaj));
                var linia = LiniaCheii(refuz, "ITV-LIPSA");
                Console.WriteLine($"     MĂSURAT (ACC-V3/{eticheta}): refuzul cu TOATE cheile acceptate, linia "
                    + $"blocantului = „{linia ?? "<absentă>"}”.");
                s.Check($"ACC-V3 ({eticheta}) blocantul NU se acceptă: `Inchide` cu toate cheile în `acceptate` "
                    + "refuză tot, iar refuzul poartă lista întreagă în forma `Severitate: text [cheie]`",
                    linia != null && linia.StartsWith("Blocant:"));
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                var rez = InchidereTvaApply.Genereaza(os,
                    new GenerareItvRequestDto { An = An, Luna = 1, UnitateId = idUnitate });
                ComenziDocument.Sistem(os).Opereaza(rez.DocumentId ?? Guid.Empty);
            }
            s.Check($"ACC-V4 ({eticheta}) după generarea ȘI operarea închiderii de TVA pe lună, constatarea dispare "
                + "— un draft neoperat n-ar fi ajuns (nu scrie registre)",
                Constatare(1, "ITV-LIPSA") == null);
        }
        else {
            s.Check($"ACC-V2 ({eticheta}) profilul neplătitor n-are `PoliticaInchidereTva`, deci ITV e tip inert și "
                + "constatarea nu se emite deloc — seed-ul o ține pe `Ignorat`, iar mecanismul e același",
                itvLipsa == null);
        }

        // ── ACC-V5/V6: amortizarea lunii ──
        var amoLipsa = Constatare(1, "AMO-LIPSA");
        Console.WriteLine($"     MĂSURAT (ACC-V5/{eticheta}): AMO-LIPSA = "
            + $"{amoLipsa?.Severitate.ToString() ?? "<absentă>"}.");
        s.Check($"ACC-V5 ({eticheta}) fișa pusă în funcțiune în 12/{An - 1} face luna ianuarie amortizabilă, iar "
            + "lipsa amortizării operate e AVERTISMENT; cheia n-are sufix de unitate — calculul lunii e al "
            + "societății, nu al unei unități interne",
            amoLipsa != null && amoLipsa.Severitate == SeveritateConstatare.Avertisment
            && amoLipsa.Cheie == "AMO-LIPSA");

        using (var os = s.Provider.CreateObjectSpace()) {
            var rez = AmoApply.Genereaza(os,
                new GenerareAmoRequestDto { An = An, Luna = 1, UnitateId = idUnitate });
            ComenziDocument.Sistem(os).Opereaza(rez.DocumentId ?? Guid.Empty);
        }
        s.Check($"ACC-V6 ({eticheta}) după generarea și operarea amortizării lunii, constatarea dispare",
            Constatare(1, "AMO-LIPSA") == null);

        // ── ACC-V7…V11: draftul, acceptarea pe cheie, istoricul ──
        Guid idDraft;
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.CreateObject<NotaContabila>();
            draft.Numar = Marcaj + "-NTC";
            draft.Data = Zi(1, 28);
            draft.DataInregistrare = Zi(1, 28);
            draft.PredatorId = idUnitate;
            draft.PrimitorId = idUnitate;
            os.CommitChanges();
            idDraft = draft.ID;
        }
        var cheieDraft = $"DRAFT-IN-PERIOADA:{idDraft}";
        var draftConst = Constatare(1, "DRAFT-IN-PERIOADA");
        Console.WriteLine($"     MĂSURAT (ACC-V7/{eticheta}): constatarea draftului = "
            + $"„{draftConst?.Cheie ?? "<absentă>"}” / {draftConst?.Severitate.ToString() ?? "-"}.");
        s.Check($"ACC-V7 ({eticheta}) draftul cu data înregistrării în lună produce o constatare PER DOCUMENT, "
            + "cheiată pe id-ul lui, iar textul spune consecința (rămâne operabil, cu dată ulterioară)",
            draftConst != null && draftConst.Cheie == cheieDraft
            && draftConst.Severitate == SeveritateConstatare.Avertisment
            && draftConst.ObiectId == idDraft && draftConst.Text.Contains("operabil"));

        using (var os = s.Provider.CreateObjectSpace()) {
            var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 1, [], null, Marcaj));
            var linia = LiniaCheii(refuz, cheieDraft);
            Console.WriteLine($"     MĂSURAT (ACC-V8/{eticheta}): refuzul fără acceptare, linia draftului = "
                + $"„{linia ?? "<absentă>"}”.");
            s.Check($"ACC-V8 ({eticheta}) avertismentul NEACCEPTAT refuză închiderea, iar refuzul poartă lista "
                + "ÎNTREAGĂ cu cheile — ecranul o arată ca să se accepte constatări concrete, nu un flag de forțare",
                linia != null && linia.StartsWith("Avertisment:"));
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = PerioadaService.Inchide(os, An, 1, [cheieDraft, "CHEIE-CARE-NU-EXISTA"], null, Marcaj);
            Console.WriteLine($"     MĂSURAT (ACC-V9/{eticheta}): acceptările scrise = „{rand.Acceptari}”.");
            s.Check($"ACC-V9 ({eticheta}) cu cheia acceptată luna se închide, iar cheile care nu corespund niciunei "
                + "constatări de ACUM se ignoră — raportul e o fotografie, refuzul e al stării de acum",
                rand.Fel == FelInchiderePerioada.Inchidere
                && rand.Acceptari == "[\"" + cheieDraft + "\"]");
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var istoric = PerioadeApply.Istoric(os, An, 1);
            s.Check($"ACC-V10 ({eticheta}) istoricul lunii poartă acceptările ca listă de chei, alături de cine și "
                + "când — urma deciziei conștiente, nu doar a faptului",
                istoric.Length == 1 && istoric[0].Fel == nameof(FelInchiderePerioada.Inchidere)
                && istoric[0].De == Marcaj && istoric[0].Acceptari.Length == 1
                && istoric[0].Acceptari[0] == cheieDraft);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.GetObjectByKey<NotaContabila>(idDraft);
            var refuzInLuna = s.Refuz(() => MotorOperare.Opereaza(os, draft));
            s.Check($"ACC-V11a ({eticheta}) după închidere draftul NU se poate opera pe data lui de înregistrare — "
                + "granița rămâne absolută",
                refuzInLuna != null && refuzInLuna.Contains($"01/{An}"));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.GetObjectByKey<NotaContabila>(idDraft);
            draft.DataInregistrare = Zi(2, 3);
            var linie = os.CreateObject<NotaContabilaDetaliu>();
            linie.Document = draft;
            linie.TipMaterialId = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipVenit).ID;
            linie.Cantitate = 1m;
            linie.Valoare = 10m;
            linie.ContDebitId = os.FirstOrDefault<Cont>(c => c.Simbol == (privat ? "4111" : "411.01.01")).ID;
            linie.ContCreditId = os.FirstOrDefault<Cont>(c => c.Simbol == codTipVenit).ID;
            linie.RepartitorDebitId = idClient;
            linie.RepartitorCreditId = idUnitate;
            linie.CodEconomicId = idCodEc;
            os.CommitChanges();
            MotorOperare.Opereaza(os, draft);
            os.CommitChanges();
            s.Check($"ACC-V11 ({eticheta}) consecința scrisă în textul constatării E adevărată: cu data înregistrării "
                + "mutată în februarie draftul se operează, deci n-a devenit document mort (F27-D4)",
                draft.Stare == StareDocument.Operat
                && CubScena.Note(os, idDraft).All(p => p.Data == Zi(2, 3)));
        }

        // ── ACC-V12/V13: severitatea E politică ──
        Guid idDraftFeb;
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = os.CreateObject<NotaContabila>();
            draft.Numar = Marcaj + "-NTC2";
            draft.Data = Zi(2, 20);
            draft.DataInregistrare = Zi(2, 20);
            draft.PredatorId = idUnitate;
            draft.PrimitorId = idUnitate;
            os.CommitChanges();
            idDraftFeb = draft.ID;
        }
        s.Check($"ACC-V12a ({eticheta}) precondiție: februarie își vede draftul ca avertisment",
            Constatare(2, "DRAFT-IN-PERIOADA")?.Cheie == $"DRAFT-IN-PERIOADA:{idDraftFeb}");

        Politica(FelConstatareInchidere.DraftInPerioada, SeveritateConstatare.Ignorat);
        s.Check($"ACC-V12 ({eticheta}) cu felul pus pe `Ignorat` constatarea nu se mai EMITE deloc — nu e o "
            + "constatare tăcută, e una care nu există; mecanismul rămâne, conținutul e al bazei",
            Constatare(2, "DRAFT-IN-PERIOADA") == null);

        Politica(FelConstatareInchidere.DraftInPerioada, SeveritateConstatare.Blocant);
        using (var os = s.Provider.CreateObjectSpace()) {
            var cheie = $"DRAFT-IN-PERIOADA:{idDraftFeb}";
            var chei = PerioadaService.Verifica(os, An, 2).Select(c => c.Cheie).ToArray();
            var refuz = s.Refuz(() => PerioadaService.Inchide(os, An, 2, chei, null, Marcaj));
            var linia = LiniaCheii(refuz, cheie);
            Console.WriteLine($"     MĂSURAT (ACC-V13/{eticheta}): refuzul cu TOATE cheile acceptate, linia "
                + $"draftului = „{linia ?? "<absentă>"}”.");
            s.Check($"ACC-V13 ({eticheta}) același fapt ridicat la `Blocant` devine NEACCEPTABIL: aceeași cheie care "
                + "închidea luna acum nu mai trece, deși e acceptată",
                linia != null && linia.StartsWith("Blocant:"));
        }
        Politica(FelConstatareInchidere.DraftInPerioada, SeveritateConstatare.Avertisment);

        // ── ACC-V14/V15: restul scadent, și felul FĂRĂ politică ──
        s.Check($"ACC-V14a ({eticheta}) cu seed-ul pe `Ignorat`, factura scadentă și neîncasată NU produce nicio "
            + "constatare — arieratul nu împiedică închiderea, e informativ la cerere",
            Constatare(2, "REST-SCADENT") == null);

        Politica(FelConstatareInchidere.RestScadent, SeveritateConstatare.Avertisment);
        var restConst = Constatare(2, "REST-SCADENT");
        Console.WriteLine($"     MĂSURAT (ACC-V14/{eticheta}): REST-SCADENT = „{restConst?.Cheie ?? "<absentă>"}”.");
        s.Check($"ACC-V14 ({eticheta}) ridicat la `Avertisment`, felul arată exact documentul: factura din 15.01 cu "
            + "scadența în ianuarie și restul nestins la sfârșitul lui februarie",
            restConst != null && restConst.ObiectId == idFcl && restConst.Text.Contains("scadent"));

        using (var os = s.Provider.CreateObjectSpace()) {
            // Ștergerea e FIZICĂ: una logică ar fi „ștearsă de utilizator”, iar
            // seed-ul n-ar mai recrea rândul niciodată (decizia 4).
            new Purja(os).Adauga(os.GetObjectsQuery<PoliticaInchidere>()
                .Where(p => p.Fel == FelConstatareInchidere.RestScadent).ToList()).Executa();
        }
        var faraPolitica = Constatare(2, "REST-SCADENT");
        s.Check($"ACC-V15 ({eticheta}) un fel FĂRĂ rând de politică iese ca avertisment, dar SPUNE că politica "
            + "lipsește — un default tăcut ar fi ascuns o configurație pe jumătate",
            faraPolitica != null && faraPolitica.Severitate == SeveritateConstatare.Avertisment
            && faraPolitica.Text.Contains("fără politică"));

        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.CreateObject<PoliticaInchidere>();
            rand.Fel = FelConstatareInchidere.RestScadent;
            rand.Severitate = SeveritateConstatare.Ignorat;
            rand.DinSeed = true;
            os.CommitChanges();
        }

        // ── ACC-V16/V17: gardianul politicii ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var duplicat = os.CreateObject<PoliticaInchidere>();
            duplicat.Fel = FelConstatareInchidere.AmoLipsa;
            duplicat.Severitate = SeveritateConstatare.Blocant;
            var refuz = RefuzGardianAcc(os);
            Console.WriteLine($"     MĂSURAT (ACC-V16/{eticheta}): al doilea rând pe același fel = "
                + $"„{s.PrimaLinie(refuz)}”.");
            s.Check($"ACC-V16 ({eticheta}) al doilea rând pe același fel e refuzat de gardian, ÎNAINTEA indexului "
                + "unic — un fel are o singură severitate, altfel verdictul ar depinde de ce rând întoarce baza",
                refuz != null && refuz.Contains("AmoLipsa"));
        }

        bool timbruStins;
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.AmoLipsa);
            rand.Severitate = SeveritateConstatare.Blocant;
            GardianEditare.Verifica(os);
            timbruStins = !rand.DinSeed;
            os.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.AmoLipsa);
            rand.Severitate = SeveritateConstatare.Avertisment;
            rand.DinSeed = true;
            os.CommitChanges();
        }
        s.Check($"ACC-V17 ({eticheta}) editarea rândului pe ușa securizată îi STINGE timbrul `DinSeed` (F23-D4): "
            + "politica de închidere e o politică oarecare, fără ramură proprie de proveniență în gardian",
            timbruStins);

        // ── ACC-V18: curățenia ──
        using (var os = s.Provider.CreateObjectSpace())
            CurataAcc(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>()
                .Count(p => p.An == An || (p.An == An - 1 && p.Luna == 12));
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An - 1, 12, 1) && d.Data <= new DateOnly(An, 12, 31));
            var fise = os.GetObjectsQuery<Imobilizare>()
                .Count(f => f.NumarInventar.StartsWith(Marcaj));
            var politici = os.GetObjectsQuery<PoliticaInchidere>()
                .Select(p => new { p.Fel, p.Severitate, p.DinSeed }).ToList();
            var itvAsteptat = privat ? SeveritateConstatare.Blocant : SeveritateConstatare.Ignorat;
            s.Check($"ACC-V18 ({eticheta}) fără reziduu: nicio perioadă, niciun document și nicio fișă rămase, iar "
                + "politica e din nou exact cea de seed — scena e re-rulabilă identic",
                perioade == 0 && documente == 0 && fise == 0
                && politici.Count == 4 && politici.All(p => p.DinSeed)
                && politici.Single(p => p.Fel == FelConstatareInchidere.ItvLipsa).Severitate == itvAsteptat
                && politici.Single(p => p.Fel == FelConstatareInchidere.AmoLipsa).Severitate
                    == SeveritateConstatare.Avertisment
                && politici.Single(p => p.Fel == FelConstatareInchidere.DraftInPerioada).Severitate
                    == SeveritateConstatare.Avertisment
                && politici.Single(p => p.Fel == FelConstatareInchidere.RestScadent).Severitate
                    == SeveritateConstatare.Ignorat);
        }
    }
}
