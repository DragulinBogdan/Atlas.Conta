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

// Probele review-ului advers al feliei 26 (`IMO-R*`, decizia 87). Scena stă în 2028/1–12,
// în afara ferestrei 2027/5–12 a blocurilor `E2E-IMO` / `E2E-API-IMO`.
static class VerificaReviewF26 {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "RVW-F26";
        const int An = 2028;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipF = privat ? "214" : "214.00.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);
        string Refuz(Action actiune) {
            try { actiune(); return null; }
            catch (OperareException e) { return e.Message; }
        }
        string Prima(string mesaj) => mesaj?.Split('\n')[0] ?? "<ACCEPTAT>";

        // Purja documentelor și a fișelor scenei (între probe); scena de repartitori/perioade rămâne.
        void CurataDocumente(IObjectSpace os) {
            var pj = new Purja(os);
            var start = Zi(1, 1);
            var sfarsit = Zi(12, 31);
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= start && d.Data <= sfarsit).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruImobilizari>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Imobilizare>()
                .Where(f => f.NumarInventar.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }
        void Curata(IObjectSpace os) {
            CurataDocumente(os);
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace())
            Curata(os);

        // ── Scena comună: perioadele 2028, unitatea, locul, un centru de cost, codul economic ──
        Guid idUnitate, idGestiune, idCentru, idCodEc, idTipF, idPolitica;
        using (var os = s.Provider.CreateObjectSpace()) {
            s.Check($"IMO-R0 ({eticheta}) precondiție: 2028 e liber (nicio perioadă, niciun document) — altfel "
                + "cifrele de mai jos s-ar măsura peste conținut străin",
                !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An)
                && !os.GetObjectsQuery<Document>().Any(d => d.Data >= new DateOnly(An, 1, 1)
                    && d.Data <= new DateOnly(An, 12, 31)));
            for (var luna = 1; luna <= 12; luna++) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
                p.Inchisa = false;
            }
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-UI";
            unitate.Denumire = "Unitate review F26";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-MAG";
            gestiune.Denumire = "Gestiune review F26";
            var centru = os.CreateObject<Gestiune>();
            centru.Cod = Marcaj + "-CC";
            centru.Denumire = "Centru de cost review F26";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic review F26";
            os.CommitChanges();
            idUnitate = unitate.ID;
            idGestiune = gestiune.ID;
            idCentru = centru.ID;
            idCodEc = codEc.ID;
            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            idTipF = tipF.ID;
            idPolitica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID).ID;
        }

        Imobilizare Fisa(IObjectSpace os, string sufix) {
            var f = os.CreateObject<Imobilizare>();
            f.NumarInventar = Marcaj + "-" + sufix;
            f.Denumire = "Fișă review " + sufix;
            f.TipMaterialId = idTipF;
            f.LocId = idGestiune;
            if (!privat)
                f.CodEconomicId = idCodEc;
            return f;
        }
        PunereInFunctiune Pif(IObjectSpace os, DateOnly data) {
            var p = os.CreateObject<PunereInFunctiune>();
            p.Data = data;
            p.PredatorId = idUnitate;
            p.PrimitorId = idGestiune;
            return p;
        }
        PunereInFunctiuneDetaliu LiniePif(IObjectSpace os, PunereInFunctiune pif, Guid fisaId, FelLiniePif fel,
                decimal valoare) {
            var l = os.CreateObject<PunereInFunctiuneDetaliu>();
            l.Document = pif;
            l.ImobilizareId = fisaId;
            l.TipMaterialId = idTipF;
            l.Fel = fel;
            l.Valoare = valoare;
            l.Cantitate = 1m;
            return l;
        }
        void Parametri(PunereInFunctiuneDetaliu l, int durata) {
            l.Metoda = MetodaAmortizare.Liniara;
            l.DurataLuni = durata;
            l.MetodaFiscala = MetodaAmortizare.Liniara;
            l.DurataFiscalaLuni = durata;
            l.CategorieFiscala = CategorieFiscala.Standard;
            l.UtilizareExclusiva = true;
        }
        PunereInFunctiuneDetaliu Intrare(IObjectSpace os, PunereInFunctiune pif, Guid fisaId, decimal valoare,
                int durata) {
            var l = LiniePif(os, pif, fisaId, FelLiniePif.Intrare, valoare);
            Parametri(l, durata);
            return l;
        }
        AmortizareLunara Amo(IObjectSpace os, int luna) {
            var r = AmortizareService.Incearca(os, An, luna, idUnitate);
            if (r.Document == null)
                throw new OperareException($"Scena review: luna {luna:00}/{An} n-a fost generată ({r.Motiv}).");
            os.CommitChanges();
            MotorOperare.Opereaza(os, r.Document);
            return r.Document;
        }
        IesireImobilizare Cas(IObjectSpace os, DateOnly data, params Guid[] fise) {
            var politica = os.GetObjectByKey<PoliticaAmortizare>(idPolitica);
            var cas = os.CreateObject<IesireImobilizare>();
            cas.Data = data;
            cas.Cauza = CauzaIesire.Casare;
            cas.PredatorId = idGestiune;
            cas.PrimitorId = idUnitate;
            foreach (var fisaId in fise) {
                var fisa = os.GetObjectByKey<Imobilizare>(fisaId);
                foreach (var linie in AmortizareService.LiniiIesire(os, fisaId, data, politica)) {
                    var l = os.CreateObject<IesireImobilizareDetaliu>();
                    l.Document = cas;
                    l.ImobilizareId = fisaId;
                    l.TipMaterialId = idTipF;
                    l.Fel = linie.Fel;
                    l.Valoare = linie.Valoare;
                    l.Cantitate = 1m;
                    l.ContDebitId = linie.ContDebitId;
                    l.ContCreditId = linie.ContCreditId;
                    l.RepartitorDebitId = idGestiune;
                    l.RepartitorCreditId = idGestiune;
                    l.CodEconomicId = fisa.CodEconomicId;
                }
            }
            os.CommitChanges();
            return cas;
        }
        List<LinieAmortizare> LiniiLuna(IObjectSpace os, int luna) =>
            AmortizareService.Previzualizeaza(os, An, luna).Linii.ToList();

        // ── IMO-R1: două linii `Intrare` pe aceeași fișă în același PIF ───────────
        using (var os = s.Provider.CreateObjectSpace()) {
            var f = Fisa(os, "R1");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, f.ID, 3600m, 36);
            Intrare(os, pif, f.ID, 3600m, 36);
            os.CommitChanges();
            var refuz = Refuz(() => ImoFixture.OpereazaCuSuport(os, pif));
            var randuri = CubScena.Fisa(os, [f.ID]).Count;
            var situatie = AmortizareService.Situatie(os, f.ID, Zi(1, 31));
            Console.WriteLine($"     MĂSURAT (IMO-R1/{eticheta}): operare → „{Prima(refuz)}”; rânduri `Intrare` pe "
                + $"fișă: {randuri}; brut la 31.01: {situatie.Valoare}; fișa: {f.Stare}.");
            s.Check($"IMO-R1 ({eticheta}) două linii `Intrare` pe ACEEAȘI fișă în același PIF sunt refuzate: fișa e "
                + "`Noua` pentru amândouă, deci gardianul de stare nu vede linia-soră, iar brutul s-ar dubla",
                refuz != null);
        }

        // ── IMO-R2: anularea / stornarea CAS cu AMO operată pentru o lună ULTERIOARĂ ──
        Guid idR2A, idR2B, idR2C, idCasA, idCasC;
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var a = Fisa(os, "R2A");
            var b = Fisa(os, "R2B");
            var c = Fisa(os, "R2C");
            os.CommitChanges();
            idR2A = a.ID; idR2B = b.ID; idR2C = c.ID;
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, a.ID, 3600m, 36);
            Intrare(os, pif, b.ID, 3600m, 36);
            Intrare(os, pif, c.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            Amo(os, 2);
            var casA = Cas(os, Zi(3, 10), a.ID);
            MotorOperare.Opereaza(os, casA);
            var casC = Cas(os, Zi(3, 12), c.ID);
            MotorOperare.Opereaza(os, casC);
            idCasA = casA.ID;
            idCasC = casC.ID;
            var martie = Amo(os, 3);
            var aprilie = Amo(os, 4);
            s.Check($"IMO-R2 ({eticheta}) precondiție: după ieșirile din martie, martie și aprilie se amortizează doar "
                + "pe fișa rămasă (B)",
                martie.Detalii.Count == 1 && aprilie.Detalii.Count == 1
                && martie.Detalii.OfType<AmortizareLunaraDetaliu>().Single().ImobilizareId == b.ID);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var casA = os.GetObjectByKey<IesireImobilizare>(idCasA);
            var refuzAnulare = Refuz(() => MotorOperare.AnuleazaOperarea(os, casA));
            Console.WriteLine($"     MĂSURAT (IMO-R2a/{eticheta}): anularea ieșirii din 10.03 cu aprilie operată → "
                + $"„{Prima(refuzAnulare)}”; fișa A: {os.GetObjectByKey<Imobilizare>(idR2A).Stare}.");
            s.Check($"IMO-R2a ({eticheta}) anularea unei ieșiri e refuzată cât timp există amortizare OPERATĂ pentru "
                + "o lună ulterioară ieșirii: acele luni s-au generat FĂRĂ fișa ieșită, iar readucerea ei în "
                + "funcțiune lasă lunile lipsă (simetricul refuzului de la operarea PIF-ului retroactiv)",
                refuzAnulare != null);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var casC = os.GetObjectByKey<IesireImobilizare>(idCasC);
            var refuzStorno = Refuz(() => MotorOperare.Storneaza(os, casC, Zi(4, 20)));
            Console.WriteLine($"     MĂSURAT (IMO-R2b/{eticheta}): stornarea ieșirii din 12.03 pe 20.04, cu aprilie "
                + $"operată → „{Prima(refuzStorno)}”; fișa C: {os.GetObjectByKey<Imobilizare>(idR2C).Stare}.");
            s.Check($"IMO-R2b ({eticheta}) stornarea unei ieșiri e refuzată pe același gardian ca anularea",
                refuzStorno != null);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var mai = LiniiLuna(os, 5);
            var situatieA = AmortizareService.Situatie(os, idR2A, Zi(4, 30));
            var situatieC = AmortizareService.Situatie(os, idR2C, Zi(4, 30));
            Console.WriteLine($"     MĂSURAT (IMO-R2c/{eticheta}): mai are {mai.Count} linii "
                + $"(A: {mai.Any(l => l.ImobilizareId == idR2A)}, C: {mai.Any(l => l.ImobilizareId == idR2C)}); "
                + $"A la 30.04: cumulat {situatieA.Amortizare}, luni {situatieA.Luni}; "
                + $"C la 30.04: cumulat {situatieC.Amortizare}, luni {situatieC.Luni}.");
            s.Check($"IMO-R2c ({eticheta}) după cele două corecții refuzate, mai se amortizează tot doar pe B; o "
                + "fișă readusă în funcțiune cu lunile martie/aprilie lipsă ar reapărea în mai cu `Luni` = 1 și "
                + "cota veche — o gaură de două luni pe care nimic n-o mai semnalează",
                mai.Count == 1);
        }

        // ── IMO-R3: stornarea CAS se datează în luna ieșirii ──
        Guid idCasR3, idFisaR3;
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var d = Fisa(os, "R3");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, d.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            Amo(os, 2);
            var cas = Cas(os, Zi(3, 20), d.ID);
            MotorOperare.Opereaza(os, cas);
            idCasR3 = cas.ID;
            idFisaR3 = d.ID;
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuzTarziu = Refuz(() => MotorOperare.Storneaza(os,
                os.GetObjectByKey<IesireImobilizare>(idCasR3), Zi(4, 15)));
            Console.WriteLine($"     MĂSURAT (IMO-R3a/{eticheta}): stornarea ieșirii din 20.03 pe 15.04 → „{Prima(refuzTarziu)}”.");
            s.Check($"IMO-R3a ({eticheta}) stornarea unei ieșiri într-o lună ULTERIOARĂ e refuzată: rândul invers "
                + "datat în aprilie ar lăsa situația la 31.03 cu „brut 0” și fișa ar dispărea din luna următoare, "
                + "apoi ar reapărea în mai — o lună de cheltuială pierdută tăcut",
                refuzTarziu != null);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var d = os.GetObjectByKey<Imobilizare>(idFisaR3);
            MotorOperare.Storneaza(os, os.GetObjectByKey<IesireImobilizare>(idCasR3), Zi(3, 31));
            var martie = Amo(os, 3);
            var aprilie = AmortizareService.Previzualizeaza(os, An, 4);
            var laMartie = AmortizareService.Situatie(os, d.ID, Zi(3, 31));
            Console.WriteLine($"     MĂSURAT (IMO-R3b/{eticheta}): stornată pe 31.03 — fișa: {d.Stare}, ieșire "
                + $"{d.DataIesire?.ToString() ?? "<null>"}; martie {martie.Detalii.Count} linii; aprilie = "
                + $"{aprilie.Motiv?.ToString() ?? "<se generează>"}, {aprilie.Linii.Count} linii; situația la 31.03: "
                + $"brut {laMartie.Valoare}, cumulat {laMartie.Amortizare}.");
            s.Check($"IMO-R3b ({eticheta}) stornată în luna ei, ieșirea n-a existat: fișa e în funcțiune, situația la "
                + "31.03 are brutul întreg și aprilie o amortizează",
                d.Stare == StareImobilizare.InFunctiune && laMartie.Valoare == 3600m
                && aprilie.Motiv == null && aprilie.Linii.Any(l => l.ImobilizareId == d.ID));
        }

        // ── IMO-R4: stornarea AMO se datează în luna ei; regenerarea nu dublează situația lunii ──
        Guid idAmoR4, idFisaR4;
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var e = Fisa(os, "R4");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, e.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            idAmoR4 = Amo(os, 2).ID;
            idFisaR4 = e.ID;
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuzTarziu = Refuz(() => MotorOperare.Storneaza(os,
                os.GetObjectByKey<AmortizareLunara>(idAmoR4), Zi(3, 15)));
            Console.WriteLine($"     MĂSURAT (IMO-R4a/{eticheta}): stornarea lui februarie pe 15.03 → „{Prima(refuzTarziu)}”.");
            s.Check($"IMO-R4a ({eticheta}) stornarea unei amortizări într-o lună ULTERIOARĂ e refuzată: rândul invers "
                + "datat în martie ar lăsa situația la 29.02 cu două luni amortizate după regenerare",
                refuzTarziu != null);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            MotorOperare.Storneaza(os, os.GetObjectByKey<AmortizareLunara>(idAmoR4), Zi(2, 29));
            var februarieNou = Amo(os, 2);
            var laFebruarie = AmortizareService.Situatie(os, idFisaR4, Zi(2, 29));
            var registru = ImobilizariApply.Registru(os, Zi(2, 29)).Linii.Single(l => l.ImobilizareId == idFisaR4);
            var martie = LiniiLuna(os, 3).Single(l => l.ImobilizareId == idFisaR4);
            Console.WriteLine($"     MĂSURAT (IMO-R4b/{eticheta}): februarie stornată pe 29.02 și regenerată "
                + $"({februarieNou.Numar}); situația la 29.02: cumulat {laFebruarie.Amortizare}, luni {laFebruarie.Luni}; "
                + $"registrul la 29.02: cumulat {registru.AmortizareCumulata}, net {registru.NetContabil}; "
                + $"martie = {martie.Contabil}.");
            s.Check($"IMO-R4b ({eticheta}) stornată în luna ei și regenerată, situația fișei la 29.02 e o lună "
                + "amortizată (100,00 / 1 lună), nu două",
                laFebruarie.Amortizare == 100m && laFebruarie.Luni == 1
                && registru.AmortizareCumulata == 100m && martie.Contabil == 100m);
        }

        // ── IMO-R5: `Revizuire` cu durata SUB lunile deja amortizate ──────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var f = Fisa(os, "R5");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, f.ID, 1200m, 12);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            Amo(os, 2);
            Amo(os, 3);
            Amo(os, 4);
            var revizuire = Pif(os, Zi(5, 10));
            var l = LiniePif(os, revizuire, f.ID, FelLiniePif.Revizuire, 0m);
            Parametri(l, 2);
            os.CommitChanges();
            var refuz = Refuz(() => ImoFixture.OpereazaCuSuport(os, revizuire));
            var mai = refuz == null ? Amo(os, 5) : null;
            var iunie = refuz == null ? LiniiLuna(os, 6).SingleOrDefault(x => x.ImobilizareId == f.ID) : null;
            Console.WriteLine($"     MĂSURAT (IMO-R5/{eticheta}): revizuire la durata 2 după 3 luni amortizate → "
                + $"„{Prima(refuz)}”; mai = {mai?.Detalii.OfType<AmortizareLunaraDetaliu>().Single().Valoare.ToString() ?? "-"}; "
                + $"iunie = {iunie?.Contabil.ToString() ?? "-"} (rest {1200m - 400m}).");
            s.Check($"IMO-R5 ({eticheta}) o revizuire cu durata (contabilă sau fiscală) mai mică sau egală cu lunile "
                + "deja amortizate e refuzată: `luni_rămase = durată − consumate` ar fi ≤ 0, iar formula ar vărsa "
                + "tot restul într-o singură lună, tăcut",
                refuz != null);
        }

        // ── IMO-R6: centrul de cost schimbat pe fișă ÎNTRE generare și operare ────
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var g = Fisa(os, "R6");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, g.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            var r = AmortizareService.Incearca(os, An, 2, idUnitate);
            os.CommitChanges();
            g.CentruCostId = idCentru;
            os.CommitChanges();
            var refuz = Refuz(() => MotorOperare.Opereaza(os, r.Document));
            var nota = CubScena.Note(os, r.Document.ID).FirstOrDefault(p => p.Debit);
            Console.WriteLine($"     MĂSURAT (IMO-R6/{eticheta}): operarea draftului cu centrul de cost schimbat pe fișă → "
                + $"„{Prima(refuz)}”; centrul pe debitul postat: "
                + $"{(nota == null ? "<fără notă>" : nota.CentruCost == idCentru ? "cel nou" : nota.CentruCost == null ? "<null>" : "altul")}.");
            s.Check($"IMO-R6 ({eticheta}) centrul de cost e dimensiune a notei (`DimensiuniCulese`), la fel ca locul și "
                + "codul economic: un draft generat înaintea schimbării lui pe fișă ori e refuzat ca stale, ori "
                + "postează cu centrul CURENT — nu cu unul vechi copiat la generare",
                refuz != null || CubScena.Note(os, r.Document.ID).Any(p => p.Debit && p.CentruCost == idCentru));
        }

        // ── IMO-R7: fișă pe un tip material care NU e de clasă de imobilizări ─────
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var tipStrain = os.GetObjectsQuery<TipMaterial>()
                .First(t => t.Clasa.Natura != NaturaClasa.Imobilizare && t.ContImplicitId != null);
            var h = Fisa(os, "R7");
            h.TipMaterialId = tipStrain.ID;
            string refuzGardian;
            try {
                GardianEditare.Verifica(os);
                refuzGardian = null;
            }
            catch (OperareException e) {
                refuzGardian = e.Message;
            }
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            var l = Intrare(os, pif, h.ID, 3600m, 36);
            l.TipMaterialId = tipStrain.ID;
            os.CommitChanges();
            var refuzPif = Refuz(() => ImoFixture.OpereazaCuSuport(os, pif));
            Console.WriteLine($"     MĂSURAT (IMO-R7/{eticheta}): fișă pe tipul {tipStrain.Cod} (natura "
                + $"{os.GetObjectByKey<ClasaProdus>(tipStrain.ClasaId)?.Natura}) → gardian „{Prima(refuzGardian)}”, "
                + $"PIF „{Prima(refuzPif)}”; fișa: {h.Stare}.");
            s.Check($"IMO-R7 ({eticheta}) fișa cere un tip material de clasă de IMOBILIZĂRI (F26-D1: „clasă F”): "
                + "gardianul fișei sau operarea PIF-ului refuză un tip de stoc/serviciu — altfel CAS ar credita "
                + "contul implicit al unui tip străin, iar o politică de amortizare pe el ar amortiza marfă",
                refuzGardian != null || refuzPif != null);
        }

        // ── IMO-R8: ieșirea unei fișe fără nicio amortizare (cumulat 0) ───────────
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var i = Fisa(os, "R8");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, i.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            var cas = Cas(os, Zi(2, 20), i.ID);
            var refuz = Refuz(() => MotorOperare.Opereaza(os, cas));
            var noteCub = CubScena.Note(os, cas.ID);
            Console.WriteLine($"     MĂSURAT (IMO-R8/{eticheta}): ieșire fără amortizare → „{Prima(refuz)}”; "
                + $"{cas.Detalii.Count} linii, debite: [{string.Join(", ", noteCub.Where(p => p.Debit).Select(p => p.Valoare))}].");
            s.Check($"IMO-R8 ({eticheta}) ieșirea unei fișe neamortizate postează DOAR nota valorii rămase: linia "
                + "„amortizare cumulată” de 0 nu lasă un rând de zero în registrul contabil (D6 omite doar linia "
                + "de net 0; cumulatul 0 nu e tratat)",
                refuz == null && noteCub.Count > 0 && noteCub.All(p => p.Valoare != 0m));
        }

        // ── IMO-R9: anularea unei modernizări din luna unei AMO deja operate (nu depinde de ea) ──
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            var j = Fisa(os, "R9");
            os.CommitChanges();
            var pif = Pif(os, Zi(1, 5));
            Intrare(os, pif, j.ID, 3600m, 36);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            Amo(os, 2);
            var modernizare = Pif(os, Zi(2, 10));
            LiniePif(os, modernizare, j.ID, FelLiniePif.Modernizare, 1650m);
            os.CommitChanges();
            var refuzOperare = Refuz(() => ImoFixture.OpereazaCuSuport(os, modernizare));
            string refuzAnulare = null;
            if (refuzOperare == null)
                using (var osAnulare = s.Provider.CreateObjectSpace())
                    refuzAnulare = Refuz(() => MotorOperare.AnuleazaOperarea(osAnulare,
                        osAnulare.GetObjectByKey<PunereInFunctiune>(modernizare.ID)));
            Console.WriteLine($"     MĂSURAT (IMO-R9/{eticheta}): modernizare pe 10.02 cu februarie operată → operare "
                + $"„{Prima(refuzOperare)}”, anulare „{Prima(refuzAnulare)}”.");
            s.Check($"IMO-R9 ({eticheta}) OBSERVAȚIE (nu defect de fond): modernizarea din luna unei amortizări deja "
                + "operate se operează (luna evenimentului postează cota veche), dar anularea ei e refuzată de "
                + "`VerificaFaraFapteUlterioare` pe `Data >=`, deși rândul lunar din 29.02 nu s-a calculat pe ea — "
                + "criteriul de dependență al anulării (data) e mai strict decât cel al operării (luna)",
                refuzOperare == null && refuzAnulare != null);
        }

        // ── AMO-V0…V8 (F27-D4): recuperarea amortizării fișei puse în funcțiune întârziat ─
        // Aritmetica de referință, PURĂ: cotele celor `luni` luni, una după alta, ca în `Grafic`.
        decimal Recuperat(MetodaAmortizare metoda, decimal valoare, int durata, int luni) {
            var rest = valoare;
            var total = 0m;
            for (var i = 0; i < luni && rest > 0m; i++) {
                var cota = AmortizareService.CotaLunara(
                    new BazaAmortizare(metoda, valoare, durata, i, rest, valoare, i));
                total += cota;
                rest -= cota;
            }
            return total;
        }
        void ParametriMetoda(PunereInFunctiuneDetaliu l, int durata, MetodaAmortizare metoda,
                CategorieFiscala categorie, bool exclusiv) {
            l.Metoda = metoda;
            l.DurataLuni = durata;
            l.MetodaFiscala = metoda;
            l.DurataFiscalaLuni = durata;
            l.CategorieFiscala = categorie;
            l.UtilizareExclusiva = exclusiv;
        }
        AmortizareLunaraDetaliu Linie(AmortizareLunara amo, Guid fisaId) =>
            amo.Detalii.OfType<AmortizareLunaraDetaliu>().Single(l => l.ImobilizareId == fisaId);

        Guid idLaTimp, idLiniar, idDegresiv, idAccelerat, idVehicul;
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataDocumente(os);
            s.Check($"AMO-V0 ({eticheta}) precondiție: 12/{An - 1} nu există (01/{An} e capăt de lanț) și toate cele "
                + $"12 luni {An} sunt deschise — altfel închiderea scenei n-ar avea același înțeles",
                !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An - 1 && p.Luna == 12)
                && os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An && !p.Inchisa) == 12);
        }

        // Martorul: aceeași fișă, înregistrată LA TIMP, amortizată lună de lună.
        using (var os = s.Provider.CreateObjectSpace()) {
            var laTimp = Fisa(os, "AMO-LT");
            os.CommitChanges();
            idLaTimp = laTimp.ID;
            var pif = Pif(os, Zi(1, 5));
            pif.DataInregistrare = Zi(1, 5);
            Intrare(os, pif, laTimp.ID, 6000m, 60);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);
            Amo(os, 2);
        }
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 1, Marcaj);
        using (var os = s.Provider.CreateObjectSpace())
            s.InchideAcceptTot(os, An, 2, Marcaj);

        // Cele patru fișe întârziate: același PIF, cu `Data` în ianuarie (închis) și
        // `DataInregistrare` în martie (deschis).
        Guid idAmoMartie;
        using (var os = s.Provider.CreateObjectSpace()) {
            var liniar = Fisa(os, "AMO-LIN");
            var degresiv = Fisa(os, "AMO-DEG");
            var accelerat = Fisa(os, "AMO-ACC");
            var vehicul = Fisa(os, "AMO-VEH");
            os.CommitChanges();
            idLiniar = liniar.ID; idDegresiv = degresiv.ID;
            idAccelerat = accelerat.ID; idVehicul = vehicul.ID;
            var pif = Pif(os, Zi(1, 5));
            pif.DataInregistrare = Zi(3, 10);
            Intrare(os, pif, liniar.ID, 6000m, 60);
            ParametriMetoda(LiniePif(os, pif, degresiv.ID, FelLiniePif.Intrare, 6000m), 60,
                MetodaAmortizare.Degresiva, CategorieFiscala.Standard, true);
            ParametriMetoda(LiniePif(os, pif, accelerat.ID, FelLiniePif.Intrare, 6000m), 60,
                MetodaAmortizare.Accelerata, CategorieFiscala.Standard, true);
            ParametriMetoda(LiniePif(os, pif, vehicul.ID, FelLiniePif.Intrare, 240000m), 60,
                MetodaAmortizare.Liniara, CategorieFiscala.VehiculPersoaneMax9Locuri, false);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);

            var martie = Amo(os, 3);
            idAmoMartie = martie.ID;
            var lt = Linie(martie, idLaTimp);
            var lin = Linie(martie, idLiniar);
            var randuri = CubScena.Fisa(os, [idLiniar, idLaTimp], martie.ID);
            var randLiniar = randuri.Single(r => r.Rand.ImobilizareId == idLiniar).Rand;
            var randLaTimp = randuri.Single(r => r.Rand.ImobilizareId == idLaTimp).Rand;
            Console.WriteLine($"     MĂSURAT (AMO-V1/{eticheta}): martie — la timp {lt.Valoare}/{lt.Luni} luni, "
                + $"întârziată {lin.Valoare}/{lin.Luni} luni; rândurile de registru: {randLaTimp.Luni} / "
                + $"{randLiniar.Luni} luni, datate {randLaTimp.Data:dd.MM.yyyy}.");
            s.Check($"AMO-V1 ({eticheta}) fișa pusă în funcțiune pe {Zi(1, 5):dd.MM.yyyy} și înregistrată pe "
                + $"{Zi(3, 10):dd.MM.yyyy} intră în amortizarea lunii 03/{An} cu DOUĂ luni (februarie + martie): "
                + "200,00 = 2 × 100,00, iar rândul de registru poartă `Luni` = 2, nu 1. Fișa înregistrată la timp "
                + "rămâne pe o lună — recuperarea e datorată, nu un mod nou de calcul",
                lin.Luni == 2 && lin.Valoare == 200m && lin.ValoareFiscala == 200m
                && CubScena.Fisa(os, [idLiniar], martie.ID) is [var randLiniarCub]
                && randLiniarCub.Rand.Luni == 2 && randLiniarCub.Rand.Amortizare == 200m
                && lt.Luni == 1 && lt.Valoare == 100m
                && CubScena.Fisa(os, [idLaTimp], martie.ID) is [var randLaTimpCub] && randLaTimpCub.Rand.Luni == 1);

            var deg = Linie(martie, idDegresiv);
            var acc = Linie(martie, idAccelerat);
            var degAsteptat = Recuperat(MetodaAmortizare.Degresiva, 6000m, 60, 2);
            var accAsteptat = Recuperat(MetodaAmortizare.Accelerata, 6000m, 60, 2);
            Console.WriteLine($"     MĂSURAT (AMO-V2/{eticheta}): degresiv {deg.Valoare} (aritmetica pură "
                + $"{degAsteptat}), accelerat {acc.Valoare} (pură {accAsteptat}).");
            s.Check($"AMO-V2 ({eticheta}) recuperarea se calculează ITERATIV, nu ca `n × cota`: pe degresiv și pe "
                + "accelerat suma celor două luni egalează la ban suma cotelor lunare ale aritmeticii pure, cu "
                + "pragurile (anul degresivului, cele 12 luni ale acceleratului) avansate la fiecare pas",
                deg.Luni == 2 && deg.Valoare == degAsteptat && degAsteptat == 300m
                && acc.Luni == 2 && acc.Valoare == accAsteptat && accAsteptat == 500m);

            var veh = Linie(martie, idVehicul);
            Console.WriteLine($"     MĂSURAT (AMO-V3/{eticheta}): vehiculul neexclusiv — fiscal "
                + $"{veh.ValoareFiscala}, deductibil {veh.ValoareDeductibila}.");
            s.Check($"AMO-V3 ({eticheta}) deductibilul se calculează pe SUMA lunii, cu regula valabilă la sfârșitul "
                + "ei: plafonul lunar e al LUNII DE DECLARARE, deci se aplică O SINGURĂ dată pe suma recuperată, "
                + "nu o dată pe fiecare lună recuperată (privat: 8.000 fiscal ⇒ 1.500 deductibil; pe bugetar nu "
                + "există regulă de deductibilitate, deci deductibilul E fiscalul)",
                veh.Luni == 2 && veh.ValoareFiscala == 8000m
                && veh.ValoareDeductibila == (privat ? 1500m : 8000m));
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var aprilie = Amo(os, 4);
            var lin = Linie(aprilie, idLiniar);
            var lt = Linie(aprilie, idLaTimp);
            var sitLiniar = AmortizareService.Situatie(os, idLiniar, Zi(4, 30));
            var sitLaTimp = AmortizareService.Situatie(os, idLaTimp, Zi(4, 30));
            Console.WriteLine($"     MĂSURAT (AMO-V4/{eticheta}): aprilie — întârziată {lin.Valoare}/{lin.Luni}, "
                + $"la timp {lt.Valoare}/{lt.Luni}; la 30.04 — întârziată {sitLiniar.Amortizare}/{sitLiniar.Luni} "
                + $"luni, la timp {sitLaTimp.Amortizare}/{sitLaTimp.Luni} luni.");
            s.Check($"AMO-V4 ({eticheta}) după recuperare fișa revine la ritmul de o lună, iar situația ei la 30.04 "
                + "e IDENTICĂ cu a fișei înregistrate la timp (3 luni, 300,00): întârzierea de evidență nu lasă "
                + "urmă în grafic",
                lin.Luni == 1 && lin.Valoare == 100m && lt.Luni == 1 && lt.Valoare == 100m
                && sitLiniar.Luni == 3 && sitLiniar.Amortizare == 300m
                && sitLaTimp.Luni == 3 && sitLaTimp.Amortizare == 300m);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var refuz = Refuz(() => AmortizareService.Incearca(os, An, 6, idUnitate));
            var raport = AmortizareService.Previzualizeaza(os, An, 6);
            Console.WriteLine($"     MĂSURAT (AMO-V5/{eticheta}): 06/{An} cu 05 negenerată → „{Prima(refuz)}”, "
                + $"raport {raport.Motiv}.");
            s.Check($"AMO-V5 ({eticheta}) recuperarea NU e o cale de a sări luni: 06/{An}, cu mai negenerată deși "
                + "avea fișe eligibile, cade pe același gardian `LunaLipsa` ca înainte — se recuperează doar ce "
                + "n-a avut cum să fie amortizat, nu ce n-a fost amortizat",
                refuz != null && raport.Motiv == MotivNegenerare.LunaLipsa);
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = AmortizareService.Genereaza(os, An, 5, idUnitate);
            os.CommitChanges();
            Linie(draft, idLiniar).Luni = 2;
            os.CommitChanges();
            var refuz = Refuz(() => MotorOperare.Opereaza(os, draft));
            Console.WriteLine($"     MĂSURAT (AMO-V6/{eticheta}): `Luni` schimbat pe draft → „{Prima(refuz)}”.");
            s.Check($"AMO-V6 ({eticheta}) `Luni` intră în cheia anti-stale a operării: o linie culeasă cu alte luni "
                + "decât cele recalculate e refuzată, ca oricare dintre cele trei cifre — altfel registrul ar "
                + "putea primi un cumulat de luni pe care nimic nu l-a calculat",
                refuz != null && refuz.Contains("nu mai corespund"));
            os.Delete(draft.Detalii.ToList());
            os.Delete(draft);
            os.CommitChanges();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            var ultimaZiAprilie = Zi(4, 30);
            var aprilie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == ultimaZiAprilie && a.Stare == StareDocument.Operat);
            MotorOperare.Storneaza(os, aprilie, Zi(4, 30));
            MotorOperare.Storneaza(os, os.GetObjectByKey<AmortizareLunara>(idAmoMartie), Zi(3, 31));
            var inverse = CubScena.Fisa(os, [idLiniar], idAmoMartie).Where(r => r.Rand.Storno).Select(r => r.Rand).ToList();
            var situatie = AmortizareService.Situatie(os, idLiniar, Zi(4, 30));
            var martieLibera = AmortizareService.Previzualizeaza(os, An, 3);
            Console.WriteLine($"     MĂSURAT (AMO-V7/{eticheta}): {inverse.Count} rând invers cu "
                + $"{inverse[0].Luni} luni și {inverse[0].Amortizare}; la 30.04 — {situatie.Amortizare} / "
                + $"{situatie.Luni} luni; martie = {martieLibera.Motiv?.ToString() ?? "<se generează>"}.");
            s.Check($"AMO-V7 ({eticheta}) stornoul unei amortizări cu recuperare scrie `-2` luni, nu `-1`: situația "
                + "fișei revine la zero luni și zero cumulat, iar luna redevine liberă — lunile sunt o coloană a "
                + "registrului, deci se inversează ca oricare alta",
                CubScena.Fisa(os, [idLiniar], idAmoMartie).Where(r => r.Rand.Storno).ToList() is [var inversCub]
                && inversCub.Rand.Luni == -2 && inversCub.Rand.Amortizare == -200m
                && situatie.Luni == 0 && situatie.Amortizare == 0m && martieLibera.Motiv == null);
        }

        // Scena își desface urmele: perioadele se redeschid, istoricul și snapshot-urile se purjează.
        using (var os = s.Provider.CreateObjectSpace())
            PerioadaService.Redeschide(os, An, 2, "probă AMO: desfacerea scenei", null, Marcaj);
        using (var os = s.Provider.CreateObjectSpace())
            PerioadaService.Redeschide(os, An, 1, "probă AMO: desfacerea scenei", null, Marcaj);
        using (var os = s.Provider.CreateObjectSpace()) {
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An).Select(p => p.ID).ToList();
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Executa();
            s.Check($"AMO-V8 ({eticheta}) scena se desface complet: cele două luni închise sunt din nou deschise, "
                + "fără istoric și fără snapshot rămas — proba e re-rulabilă identic",
                os.GetObjectsQuery<PerioadaFiscala>().Count(p => p.An == An && !p.Inchisa) == 12
                && !os.GetObjectsQuery<InchiderePerioada>()
                    .Any(i => perioadeIds.Contains(i.PerioadaId)));
        }

        // ── Curățenia finală ──────────────────────────────────────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            Curata(os);
            s.Check($"IMO-R — curățenie finală ({eticheta}): nicio fișă, nicio perioadă și niciun document 2028 rămase",
                !os.GetObjectsQuery<Imobilizare>().Any(f => f.NumarInventar.StartsWith(Marcaj))
                && !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An)
                && !os.GetObjectsQuery<Document>().Any(d => d.Data >= new DateOnly(An, 1, 1)
                    && d.Data <= new DateOnly(An, 12, 31)));
        }
    }
}
