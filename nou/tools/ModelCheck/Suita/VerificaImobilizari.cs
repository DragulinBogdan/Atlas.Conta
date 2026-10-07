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

static class VerificaImobilizari {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-IMO";
        const int An = 2027;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipF = privat ? "214" : "214.00.00";
        var codTipF2 = privat ? "2133" : "213.03.00";
        var simbolFurnizorImobilizari = privat ? "404" : "404.01.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);

        // ── Curățenia scenei (purjă FIZICĂ — F13-D2), încrucișată cu blocul de API ──
        void CurataImo(IObjectSpace os) {
            var pj = new Purja(os);
            var start = Zi(5, 1);
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
                .Where(f => f.NumarInventar.StartsWith(Marcaj) || f.NumarInventar.StartsWith("E2E-API-IMO")).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj) || r.Cod.StartsWith("E2E-API-IMO")).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<SursaFinantare>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodFunctional>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Proiect>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            // Tipul de material de scenă (fișa fără politică) și regulile de scenă.
            var tipuriScena = os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)).Select(t => t.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<PoliticaAmortizare>()
                .Where(p => tipuriScena.Contains(p.TipMaterialId)).ToList());
            pj.Adauga(os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegulaDeductibilitate>()
                .Where(r => r.Temei.StartsWith(Marcaj)).ToList());
            // Perioadele 2027/5–12 sunt artefact de scenă (seed-ul acoperă doar 2026).
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An && p.Luna >= 5).ToList());
            pj.Executa();
        }

        // Calea REALĂ: dispecerul `IVerificabilLaCommit` din gardian, nu corpul regulii.
        static string RefuzGardianImo(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataImo(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var primaZi = Zi(5, 1);
            var ultimaZi = Zi(12, 31);
            var documenteInScena = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= primaZi && d.Data <= ultimaZi);
            Console.WriteLine($"     MĂSURAT (IMO-V0/{eticheta}): {documenteInScena} documente în 05–12/{An} după purjă.");
            s.Check($"IMO — precondiție ({eticheta}): lunile 05–12/{An} sunt libere după purjă; 2027 e în afara "
                + "perioadelor seed-uite, deci niciun alt scenariu al suitei n-a scris acolo — altfel cifrele de "
                + "mai jos ar fi măsurate peste conținut străin",
                documenteInScena == 0);
        }

        // ── IMO-V1: seed-ul (ancorele, numerotarea, politica, catalogul, tipurile) ──
        using (var os = s.Provider.CreateObjectSpace()) {
            TipDocument Tip(string cod) => os.FirstOrDefault<TipDocument>(t => t.Cod == cod);
            var pif = Tip("PIF");
            var cas = Tip("CAS");
            var amo = Tip("AMO");
            string Serie(TipDocument t) => t == null ? null
                : os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == t.ID)?.Serie;
            Console.WriteLine($"     MĂSURAT (IMO-V1/{eticheta}): ancore PIF={pif?.ClrType ?? "<lipsă>"}, "
                + $"CAS={cas?.ClrType ?? "<lipsă>"}, AMO={amo?.ClrType ?? "<lipsă>"}; "
                + $"serii {Serie(pif)}/{Serie(cas)}/{Serie(amo)}.");
            s.Check($"IMO-V1 ({eticheta}) ancorele PIF/CAS/AMO există în NUCLEU cu ClrType-ul clasei și au fiecare "
                + "seria proprie de numerotare — spre deosebire de DSC/ITV, imobilizările sunt active pe AMBELE "
                + "profiluri (amortizarea e a lor; doar deductibilitatea fiscală e a privatului)",
                pif?.ClrType == nameof(PunereInFunctiune) && cas?.ClrType == nameof(IesireImobilizare)
                && amo?.ClrType == nameof(AmortizareLunara)
                && Serie(pif) == "PIF-" && Serie(cas) == "CAS-" && Serie(amo) == "AMO-");

            var faraReguli = new[] { pif, cas, amo }.All(t =>
                !os.GetObjectsQuery<RegulaContare>().Any(r => r.TipDocumentId == t.ID)
                && !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocumentId == t.ID)
                && os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == t.ID) == null
                && os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == t.ID) == null);
            s.Check($"IMO-V2 ({eticheta}) PIF/CAS/AMO n-au nicio regulă de contare sau de stoc, nicio politică de TVA "
                + "și niciun conex: postarea lor e EXPLICITĂ pe linie, din `PoliticaAmortizare` — un rând de "
                + "`RegulaContare` ar fi a doua sursă a acelorași conturi",
                faraReguli);

            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            var tipF2 = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF2);
            var politicaF = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID);
            string Simbol(Guid? id) => id == null ? "<null>" : os.GetObjectByKey<Cont>(id.Value)?.Simbol;
            var numarPolitici = os.GetObjectsQuery<PoliticaAmortizare>().Count();
            var terenuri = privat ? os.FirstOrDefault<TipMaterial>(t => t.Cod == "211") : null;
            Console.WriteLine($"     MĂSURAT (IMO-V3/{eticheta}): {numarPolitici} rânduri `PoliticaAmortizare`; "
                + $"{codTipF} → amortizare {Simbol(politicaF?.ContAmortizareId)}, "
                + $"cheltuială {Simbol(politicaF?.ContCheltuialaAmortizareId)}, "
                + $"cedare {Simbol(politicaF?.ContCheltuialaCedareId)}; "
                + $"tip {codTipF2} = {(tipF2 == null ? "<lipsă>" : "prezent")}"
                + (privat ? $"; terenuri 211 cu politică: {os.GetObjectsQuery<PoliticaAmortizare>().Any(p => p.TipMaterialId == (terenuri != null ? terenuri.ID : Guid.Empty))}" : "") + ".");
            s.Check($"IMO-V3 ({eticheta}) fiecare tip material de clasă F amortizabil are rând `PoliticaAmortizare` cu "
                + "cele trei conturi REZOLVATE din planul profilului (28x / cheltuiala cu amortizarea / cheltuiala "
                + "cu cedarea), toate `DinSeed`; terenurile NU au rând — absența e o afirmație, nu o omisiune",
                tipF != null && tipF2 != null && politicaF != null && politicaF.DinSeed
                && politicaF.ContAmortizareId != null && politicaF.ContCheltuialaAmortizareId != null
                && politicaF.ContCheltuialaCedareId != null
                && os.GetObjectsQuery<PoliticaAmortizare>().All(p => p.ContAmortizareId != null
                    && p.ContCheltuialaAmortizareId != null && p.ContCheltuialaCedareId != null)
                && (!privat || (terenuri != null
                    && !os.GetObjectsQuery<PoliticaAmortizare>().Any(p => p.TipMaterialId == terenuri.ID))));

            var reguli = os.GetObjectsQuery<RegulaDeductibilitate>().ToList();
            var vehicul = reguli.FirstOrDefault(r => r.Categorie == CategorieFiscala.VehiculPersoaneMax9Locuri);
            Console.WriteLine($"     MĂSURAT (IMO-V4/{eticheta}): {reguli.Count} reguli de deductibilitate; "
                + $"vehicul = {vehicul?.Fel.ToString() ?? "<lipsă>"} {vehicul?.Valoare} de la {vehicul?.DeLa:dd.MM.yyyy}; "
                + $"sedii: {reguli.Count(r => r.Categorie == CategorieFiscala.SediuSocialInLocuinta)}.");
            s.Check($"IMO-V4 ({eticheta}) limitările fiscale sunt DATE cu valabilitate în timp (`DeLa`, `Temei`): "
                + "3 rânduri pe privat (plafonul vehiculelor, sediul în locuință 2024 și 2026), ZERO pe bugetar — "
                + "o cifră de lege nu intră în cod (F26-D16)",
                privat
                    ? reguli.Count == 3 && vehicul != null && vehicul.Fel == FelDeductibilitate.PlafonLunar
                        && vehicul.Valoare == 1500m && vehicul.DoarNeexclusiv
                        && reguli.All(r => r.DinSeed && !string.IsNullOrWhiteSpace(r.Temei))
                        && reguli.Count(r => r.Categorie == CategorieFiscala.SediuSocialInLocuinta
                            && r.Fel == FelDeductibilitate.Procent) == 2
                    : reguli.Count == 0);

            var catalog = os.GetObjectsQuery<ClasificareImobilizari>().ToList();
            var calculatoare = catalog.FirstOrDefault(c => c.Cod == "2.2.9.");
            var cladiri = catalog.FirstOrDefault(c => c.Cod == "1.1.1.");
            var grupa1 = catalog.FirstOrDefault(c => c.Cod == "1.");
            Console.WriteLine($"     MĂSURAT (IMO-V5/{eticheta}): {catalog.Count} rânduri de catalog; "
                + $"2.2.9. = {calculatoare?.DurataMinAni}–{calculatoare?.DurataMaxAni} ani (grupa {calculatoare?.Grupa}); "
                + $"1.1.1. = {cladiri?.DurataMinAni}–{cladiri?.DurataMaxAni}; "
                + $"1. = {(grupa1?.DurataMinAni == null ? "fără bandă" : "cu bandă")}.");
            s.Check($"IMO-V5 ({eticheta}) catalogul HG 2139/2004 e seed-uit cu cele 590 de poziții CU COD ale "
                + "CSV-ului ANAF (din 598 de rânduri; cele 8 sub-variante „mediu neutru / mediu coroziv” n-au cod "
                + "propriu în sursă și se RAPORTEAZĂ, nu primesc un cod inventat), cu banda duratei normale pe "
                + "pozițiile de detaliu și FĂRĂ bandă pe grupe — o bandă inventată ar fi devenit un refuz fals",
                catalog.Count == 590
                && calculatoare is { DurataMinAni: 2, DurataMaxAni: 4, Grupa: "2" }
                && cladiri is { DurataMinAni: 40, DurataMaxAni: 60, Grupa: "1" }
                && grupa1 is { DurataMinAni: null, DurataMaxAni: null });
        }

        // ── Scena ─────────────────────────────────────────────────────────────────
        Guid idFisa1, idFisa2, idFisa3, idFisa4, idLinieSursa, idFct, idPifIntrare, idPifModernizare;
        Guid idTipF, idGestiune, idUnitate, idClasificare, idFurnizor, idCodEc, idSursaFin, idCodFn, idProiect;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 5, 6, 7, 8, 9, 10, 11, 12 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
                p.Inchisa = false;
            }

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-FURN";
            furnizor.Denumire = "Furnizor de imobilizări probă F26";
            furnizor.Tara = "RO";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-MAG";
            gestiune.Denumire = "Gestiune probă F26";
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-UI";
            unitate.Denumire = "Unitate probă F26";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-CE";
            codEc.Denumire = "Cod economic probă F26";
            var sursaFin = os.CreateObject<SursaFinantare>();
            sursaFin.Cod = Marcaj + "-SF";
            sursaFin.Denumire = "Sursă de finanțare probă F26";
            var codFn = os.CreateObject<CodFunctional>();
            codFn.Cod = Marcaj + "-CF";
            codFn.Denumire = "Cod funcțional probă F26";
            var proiect = os.CreateObject<Proiect>();
            proiect.Cod = Marcaj + "-PR";
            proiect.Denumire = "Proiect probă F26";
            os.CommitChanges();

            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            idTipF = tipF.ID;
            idGestiune = gestiune.ID;
            idUnitate = unitate.ID;
            idFurnizor = furnizor.ID;
            idCodEc = codEc.ID;
            idSursaFin = sursaFin.ID;
            idCodFn = codFn.ID;
            idProiect = proiect.ID;
            idClasificare = os.FirstOrDefault<ClasificareImobilizari>(c => c.Cod == "2.2.9.").ID;

            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + "-FCT";
            fct.Data = Zi(5, 4);
            fct.Predator = furnizor;
            fct.Primitor = gestiune;
            var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
            linieFct.Document = fct;
            linieFct.TipMaterial = tipF;
            linieFct.Cantitate = 1m;
            linieFct.PretUnitar = 3600m;
            // Defalcarea lui 404 (BFEPR la bugetar) + clasificația cerută de `PoliticaValidare`.
            linieFct.CodEconomicId = codEc.ID;
            linieFct.SursaFinantareId = sursaFin.ID;
            linieFct.CodFunctionalId = codFn.ID;
            linieFct.ProiectId = proiect.ID;
            os.CommitChanges();
            idFct = fct.ID;
            idLinieSursa = linieFct.ID;

            var conex = MotorOperare.Opereaza(os, fct);
            var contFurnizor = os.FirstOrDefault<Cont>(c => c.Simbol == simbolFurnizorImobilizari);
            var noteFctCub = CubScena.Note(os, fct.ID);
            Console.WriteLine($"     MĂSURAT (IMO-V6/{eticheta}): {noteFctCub.Count} postări pe factură; "
                + $"conex = {conex?.GetType().Name ?? "<niciunul>"}; mișcări de stoc: {CubScena.Stoc(os, fct.ID).Count}; "
                + $"lot pe linie: {linieFct.LotId != null}.");
            s.Check($"IMO-V6 ({eticheta}) achiziția unei imobilizări postează contul implicit al tipului contra "
                + "contului furnizorilor de imobilizări, NU naște lot și NU generează NIR (conexul filtrează pe "
                + "natura Stoc) — premisa feliei: valoarea e deja pe 21x când fișa se pune în funcțiune",
                noteFctCub.Count == 2 && noteFctCub.Nota(tipF.ContImplicitId, contFurnizor.ID, 3600m)
                && conex == null && CubScena.FaraStoc(os, fct.ID) && linieFct.LotId == null);

            // Cele patru fișe ale scenei.
            Imobilizare Fisa(string sufix, string denumire, Guid? clasificare) {
                var f = os.CreateObject<Imobilizare>();
                f.NumarInventar = Marcaj + "-" + sufix;
                f.Denumire = denumire;
                f.TipMaterialId = tipF.ID;
                f.LocId = gestiune.ID;
                f.ClasificareId = clasificare;
                return f;
            }
            var fisa1 = Fisa("1", "Mobilier de birou probă F26", null);
            var fisa2 = Fisa("2", "A doua fișă pe aceeași linie de factură", null);
            var fisa3 = Fisa("3", "Fișă cu clasificare din catalog", idClasificare);
            var fisa4 = Fisa("4", "Fișă de deschidere (migrare)", null);
            os.CommitChanges();
            idFisa1 = fisa1.ID;
            idFisa2 = fisa2.ID;
            idFisa3 = fisa3.ID;
            idFisa4 = fisa4.ID;
            s.Check($"IMO-V7 ({eticheta}) fișa nouă se naște în starea `Noua`, fără dată de punere în funcțiune și "
                + "fără dată de ieșire — cele trei sunt server-owned, scrise de hook-ul de registru",
                fisa1.Stare == StareImobilizare.Noua && fisa1.DataPunereInFunctiune == null
                && fisa1.DataIesire == null);
        }

        // ── IMO-V8…V12: punerea în funcțiune ──────────────────────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            var unitate = os.GetObjectByKey<Repartitor>(idUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idGestiune);
            var tipF = os.GetObjectByKey<TipMaterial>(idTipF);

            PunereInFunctiune Pif(DateOnly data) {
                var p = os.CreateObject<PunereInFunctiune>();
                p.Data = data;
                p.Predator = unitate;
                p.Primitor = gestiune;
                return p;
            }
            PunereInFunctiuneDetaliu Linie(PunereInFunctiune pif, Guid fisaId, FelLiniePif fel, decimal valoare) {
                var l = os.CreateObject<PunereInFunctiuneDetaliu>();
                l.Document = pif;
                l.ImobilizareId = fisaId;
                l.TipMaterialId = tipF.ID;
                l.Fel = fel;
                l.Valoare = valoare;
                l.Cantitate = 1m;
                return l;
            }
            void Parametri(PunereInFunctiuneDetaliu l, int durata, int durataFiscala) {
                l.Metoda = MetodaAmortizare.Liniara;
                l.DurataLuni = durata;
                l.MetodaFiscala = MetodaAmortizare.Liniara;
                l.DurataFiscalaLuni = durataFiscala;
                l.CategorieFiscala = CategorieFiscala.Standard;
                l.UtilizareExclusiva = true;
            }

            var pifIntrare = Pif(Zi(5, 5));
            var linieIntrare = Linie(pifIntrare, idFisa1, FelLiniePif.Intrare, 3600m);
            linieIntrare.LinieSursaId = idLinieSursa;
            Parametri(linieIntrare, 36, 36);
            os.CommitChanges();
            idPifIntrare = pifIntrare.ID;

            ImoFixture.OpereazaCuSuport(os, pifIntrare);
            var randuri = CubScena.Fisa(os, [idFisa1]);
            var fisa1 = os.GetObjectByKey<Imobilizare>(idFisa1);
            var rand = randuri.SingleOrDefault()?.Rand;
            Console.WriteLine($"     MĂSURAT (IMO-V8/{eticheta}): {CubScena.Note(os, pifIntrare.ID).Count} postări contabile, "
                + $"{randuri.Count} rânduri de fișă; rândul = {rand?.Fel} {rand?.Valoare}/{rand?.ValoareFiscala}, luni {rand?.Luni}, "
                + $"{rand?.Metoda}/{rand?.DurataLuni} luni, fiscal {rand?.MetodaFiscala}/{rand?.DurataFiscalaLuni}, "
                + $"categoria {rand?.CategorieFiscala}; "
                + $"numărul documentului = {pifIntrare.Numar}; fișa = {fisa1.Stare} din {fisa1.DataPunereInFunctiune}.");
            s.Check($"IMO-V8 ({eticheta}) punerea în funcțiune NU postează nimic (zero note) și scrie UN rând de "
                + "registru complet — brut contabil și fiscal, parametrii, locul la data faptului —, iar fișa trece "
                + "în `InFunctiune` cu data punerii în funcțiune; numărul vine din seria tipului",
                CubScena.Fisa(os, [idFisa1]) is [var randCub]
                && randCub.Rand.Fel == FelMiscareImobilizare.Intrare && randCub.Rand.Valoare == 3600m && randCub.Rand.ValoareFiscala == 3600m
                && randCub.Rand.Amortizare == 0m && randCub.Rand.Luni == 0
                && randCub.Rand.Metoda == MetodaAmortizare.Liniara && randCub.Rand.DurataLuni == 36
                && randCub.Rand.MetodaFiscala == MetodaAmortizare.Liniara && randCub.Rand.DurataFiscalaLuni == 36
                && randCub.Rand.CategorieFiscala == CategorieFiscala.Standard && randCub.Rand.UtilizareExclusiva == true
                && CubScena.LocFisa(os, idFisa1, pifIntrare.ID) is [var locCub] && locCub == idGestiune
                && randCub.DocumentId == pifIntrare.ID && !randCub.Rand.Storno
                && pifIntrare.Numar != null && pifIntrare.Numar.StartsWith("PIF-")
                && fisa1.Stare == StareImobilizare.InFunctiune
                && fisa1.DataPunereInFunctiune == Zi(5, 5));

            // Plafonul liniei sursă: linia de 3.600 e consumată integral.
            var pifPlafon = Pif(Zi(5, 6));
            var liniePlafon = Linie(pifPlafon, idFisa2, FelLiniePif.Intrare, 1m);
            liniePlafon.LinieSursaId = idLinieSursa;
            Parametri(liniePlafon, 36, 36);
            os.CommitChanges();
            s.CheckRefuza($"IMO-V9 ({eticheta}) a doua fișă pe ACEEAȘI linie de factură, cu valoarea deja consumată "
                + "integral, e refuzată: „nu se culege dublu” e plafon, nu convenție de ecran",
                () => ImoFixture.OpereazaCuSuport(os, pifPlafon));

            // Inițialele se culeg DOAR pe o intrare fără linie sursă (deschidere).
            liniePlafon.AmortizareInitiala = 10m;
            os.CommitChanges();
            s.CheckRefuza($"IMO-V10 ({eticheta}) cifrele inițiale pe o intrare CU linie sursă sunt refuzate: "
                + "valoarea vine de pe factură, deci activul e nou — un cumulat inițial acolo ar fi istoric inventat",
                () => ImoFixture.OpereazaCuSuport(os, pifPlafon));
            liniePlafon.LinieSursaId = null;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifPlafon);
            var fisa2 = os.GetObjectByKey<Imobilizare>(idFisa2);
            var deschidereScurta = AmortizareService.Situatie(os, idFisa2, Zi(5, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V10b/{eticheta}): fără linie sursă — brut "
                + $"{deschidereScurta.Valoare}, cumulat {deschidereScurta.Amortizare}, fișa {fisa2.Stare}.");
            s.Check($"IMO-V10b ({eticheta}) fără linie sursă valoarea e culeasă liber și cifrele inițiale sunt "
                + "permise — deschiderea și migrarea sunt chiar cazul lor",
                deschidereScurta.Valoare == 1m && deschidereScurta.Amortizare == 10m
                && fisa2.Stare == StareImobilizare.InFunctiune);
            MotorOperare.AnuleazaOperarea(os, pifPlafon);
            s.Check($"IMO-V10c ({eticheta}) anularea unei INTRĂRI readuce fișa în `Noua` și îi șterge data punerii "
                + "în funcțiune — simetricul exact al materializării",
                fisa2.Stare == StareImobilizare.Noua && fisa2.DataPunereInFunctiune == null
                && CubScena.Fisa(os, [idFisa2]).Count == 0);

            // Banda catalogului pe fișa cu clasificare (2.2.9. = 2–4 ani).
            var pifBanda = Pif(Zi(5, 7));
            var linieBanda = Linie(pifBanda, idFisa3, FelLiniePif.Intrare, 1000m);
            Parametri(linieBanda, 60, 60);
            os.CommitChanges();
            s.CheckRefuza($"IMO-V11 ({eticheta}) durata fiscală în afara benzii catalogului (2.2.9. = 2–4 ani, "
                + "adică 24–48 de luni) e refuzată: durata normală de funcționare e obligatorie fiscal, iar banda "
                + "o dă clasificarea de pe fișă",
                () => ImoFixture.OpereazaCuSuport(os, pifBanda));
            linieBanda.DurataFiscalaLuni = 36;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifBanda);
            s.Check($"IMO-V12 ({eticheta}) aceeași linie, cu durata fiscală în bandă, trece — verificarea e a "
                + "BENZII, nu a unei durate fixe",
                os.GetObjectByKey<Imobilizare>(idFisa3).Stare == StareImobilizare.InFunctiune);

            // Starea greșită a fișei, pe ambele sensuri.
            var pifGresit = Pif(Zi(5, 8));
            var linieGresit = Linie(pifGresit, idFisa1, FelLiniePif.Intrare, 100m);
            Parametri(linieGresit, 36, 36);
            os.CommitChanges();
            s.CheckRefuza($"IMO-V13 ({eticheta}) o a doua INTRARE pe o fișă deja în funcțiune e refuzată "
                + "(intrarea cere `Noua`)", () => ImoFixture.OpereazaCuSuport(os, pifGresit));
            linieGresit.Fel = FelLiniePif.Modernizare;
            linieGresit.ImobilizareId = idFisa4;
            os.CommitChanges();
            s.CheckRefuza($"IMO-V14 ({eticheta}) modernizarea unei fișe încă `Noua` e refuzată (cere `InFunctiune`) "
                + "— simetricul lui V13", () => ImoFixture.OpereazaCuSuport(os, pifGresit));
            os.Delete(pifGresit.Detalii.ToList());
            os.Delete(pifGresit);
            os.Delete(pifPlafon.Detalii.ToList());
            os.Delete(pifPlafon);
            os.CommitChanges();
        }

        // ── IMO-V15…V17: gardianul fișei și al registrului ────────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            var fisa1 = os.GetObjectByKey<Imobilizare>(idFisa1);
            var altTip = os.GetObjectsQuery<TipMaterial>().First(t => t.ID != idTipF);
            fisa1.TipMaterialId = altTip.ID;
            var refuzTip = RefuzGardianImo(os);
            Console.WriteLine($"     MĂSURAT (IMO-V15/{eticheta}): „{refuzTip?.Split('\n')[0] ?? "ACCEPTAT"}”.");
            s.Check($"IMO-V15 ({eticheta}) tipul (contul) fișei se schimbă DOAR cât e `Noua`: pe o fișă în funcțiune "
                + "contul de imobilizare a intrat deja în politica de amortizare și în note, iar gardianul îl "
                + "refuză pe starea ORIGINALĂ din persistență, nu pe cea din formular",
                refuzTip != null && refuzTip.Contains("Nouă"));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var fisa1 = os.GetObjectByKey<Imobilizare>(idFisa1);
            fisa1.Denumire = "Denumire schimbată pe o fișă în funcțiune";
            var locNou = os.GetObjectByKey<Repartitor>(idUnitate);
            fisa1.LocId = locNou.ID;
            var refuzTransfer = RefuzGardianImo(os);
            Console.WriteLine($"     MĂSURAT (IMO-V16a/{eticheta}): transfer + redenumire → "
                + $"„{refuzTransfer?.Split('\n')[0] ?? "ACCEPTAT"}”.");
            s.Check($"IMO-V16 ({eticheta}) denumirea și LOCUL rămân editabile pe o fișă în funcțiune: transferul e "
                + "administrativ (F26-D8) — nu mișcă valoare, nu postează, iar istoricul lui sunt rândurile lunare",
                refuzTransfer == null);
            fisa1.LocId = idGestiune;
            os.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var fisa1 = os.GetObjectByKey<Imobilizare>(idFisa1);
            os.Delete(fisa1);
            var refuzStergere = RefuzGardianImo(os);
            Console.WriteLine($"     MĂSURAT (IMO-V17/{eticheta}): ștergere în funcțiune → "
                + $"„{refuzStergere?.Split('\n')[0] ?? "ACCEPTAT"}”.");
            s.Check($"IMO-V17 ({eticheta}) fișa se șterge doar cât e `Noua` și fără rânduri de registru",
                refuzStergere != null);
        }

        // ── IMO-V19…V23: modernizarea, revizuirea și situația ─────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            var unitate = os.GetObjectByKey<Repartitor>(idUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idGestiune);
            var tipF = os.GetObjectByKey<TipMaterial>(idTipF);

            var pifModernizare = os.CreateObject<PunereInFunctiune>();
            pifModernizare.Data = Zi(6, 10);
            pifModernizare.Predator = unitate;
            pifModernizare.Primitor = gestiune;
            var linieModernizare = os.CreateObject<PunereInFunctiuneDetaliu>();
            linieModernizare.Document = pifModernizare;
            linieModernizare.ImobilizareId = idFisa1;
            linieModernizare.TipMaterialId = tipF.ID;
            linieModernizare.Fel = FelLiniePif.Modernizare;
            linieModernizare.Valoare = 1650m;
            linieModernizare.Cantitate = 1m;
            os.CommitChanges();
            idPifModernizare = pifModernizare.ID;
            ImoFixture.OpereazaCuSuport(os, pifModernizare);

            var pifRevizuire = os.CreateObject<PunereInFunctiune>();
            pifRevizuire.Data = Zi(7, 11);
            pifRevizuire.Predator = unitate;
            pifRevizuire.Primitor = gestiune;
            var linieRevizuire = os.CreateObject<PunereInFunctiuneDetaliu>();
            linieRevizuire.Document = pifRevizuire;
            linieRevizuire.ImobilizareId = idFisa1;
            linieRevizuire.TipMaterialId = tipF.ID;
            linieRevizuire.Fel = FelLiniePif.Revizuire;
            linieRevizuire.Valoare = 0m;
            linieRevizuire.Cantitate = 1m;
            linieRevizuire.Metoda = MetodaAmortizare.Liniara;
            linieRevizuire.DurataLuni = 48;
            linieRevizuire.MetodaFiscala = MetodaAmortizare.Liniara;
            linieRevizuire.DurataFiscalaLuni = 48;
            linieRevizuire.CategorieFiscala = CategorieFiscala.Standard;
            linieRevizuire.UtilizareExclusiva = true;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifRevizuire);

            var randModernizare = CubScena.Fisa(os, [idFisa1], idPifModernizare).Single().Rand;
            var randRevizuire = CubScena.Fisa(os, [idFisa1], pifRevizuire.ID).Single().Rand;
            Console.WriteLine($"     MĂSURAT (IMO-V19/{eticheta}): modernizare {randModernizare.Valoare} "
                + $"(parametri: {randModernizare.Metoda?.ToString() ?? "null"}/{randModernizare.DurataLuni?.ToString() ?? "null"}); "
                + $"revizuire {randRevizuire.Valoare}, durata {randRevizuire.DurataLuni}.");
            s.Check($"IMO-V19 ({eticheta}) modernizarea adaugă brut FĂRĂ parametri (null = „neschimbați”), iar "
                + "revizuirea schimbă parametrii cu valoare ZERO: durata e FAPT DATAT, nu edit de nomenclator — "
                + "de aceea recalculul unei luni vechi rămâne reproductibil",
                CubScena.Fisa(os, [idFisa1], idPifModernizare) is [var modernizareCub]
                && modernizareCub.Rand.Fel == FelMiscareImobilizare.Modernizare && modernizareCub.Rand.Valoare == 1650m
                && modernizareCub.Rand.Metoda == null && modernizareCub.Rand.DurataLuni == null
                && CubScena.Fisa(os, [idFisa1], pifRevizuire.ID) is [var revizuireCub]
                && revizuireCub.Rand.Fel == FelMiscareImobilizare.Revizuire && revizuireCub.Rand.Valoare == 0m
                && revizuireCub.Rand.DurataLuni == 48 && revizuireCub.Rand.MetodaFiscala == MetodaAmortizare.Liniara);

            var situatie = AmortizareService.Situatie(os, idFisa1, Zi(12, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V20/{eticheta}): brut {situatie.Valoare}, brut fiscal "
                + $"{situatie.ValoareFiscala}, cumulat {situatie.Amortizare}, luni {situatie.Luni}, "
                + $"durata {situatie.DurataLuni}, metoda {situatie.Metoda}, categoria {situatie.CategorieFiscala}.");
            s.Check($"IMO-V20 ({eticheta}) situația la o dată = sumele coloanelor + parametrii ULTIMULUI eveniment, "
                + "cu coalesce înapoi: brutul cumulează intrarea și modernizarea (3.600 + 1.650 = 5.250), lunile "
                + "sunt 0, durata e cea a revizuirii (48), iar categoria fiscală vine de pe intrare, pe care "
                + "modernizarea n-a atins-o",
                situatie.Valoare == 5250m && situatie.ValoareFiscala == 5250m && situatie.Amortizare == 0m
                && situatie.Luni == 0 && situatie.DurataLuni == 48 && situatie.Metoda == MetodaAmortizare.Liniara
                && situatie.CategorieFiscala == CategorieFiscala.Standard && situatie.UtilizareExclusiva == true);

            // OS propriu: o anulare refuzată lasă schimbări în tracker (33d).
            using (var osAnulare = s.Provider.CreateObjectSpace()) {
                var pifIntrareAnulat = osAnulare.GetObjectByKey<PunereInFunctiune>(idPifIntrare);
                s.CheckRefuza($"IMO-V21 ({eticheta}) anularea INTRĂRII, cu o modernizare ulterioară operată pe "
                    + "aceeași fișă, e refuzată de FRUNZĂ (`EliminaRegistrul`), nu de motor: dependențele "
                    + "registrului propriu le cunoaște tipul, iar nucleul rămâne agnostic",
                    () => MotorOperare.AnuleazaOperarea(osAnulare, pifIntrareAnulat));
            }

            // Perioada închisă rămâne graniță și pentru registrul al patrulea.
            var decembrie = os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == 12);
            decembrie.Inchisa = true;
            os.CommitChanges();
            s.CheckRefuza($"IMO-V22 ({eticheta}) stornarea într-o lună ÎNCHISĂ e refuzată — perioada închisă e "
                + "graniță absolută și pentru imobilizări",
                () => MotorOperare.Storneaza(os, pifRevizuire, Zi(12, 5)));
            decembrie.Inchisa = false;
            os.CommitChanges();

            MotorOperare.Storneaza(os, pifRevizuire, pifRevizuire.Data);
            var dupaStorno = AmortizareService.Situatie(os, idFisa1, Zi(12, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V23/{eticheta}): după storno-ul revizuirii — durata "
                + $"{dupaStorno.DurataLuni}, rânduri {CubScena.Fisa(os, [idFisa1], pifRevizuire.ID).Count}.");
            s.Check($"IMO-V23 ({eticheta}) stornarea revizuirii adaugă rândul invers (append-only) ȘI scoate "
                + "parametrii ei din situație: durata revine la 36, cea a intrării — un eveniment stornat nu mai "
                + "descrie activul",
                CubScena.Fisa(os, [idFisa1], pifRevizuire.ID).Count == 2
                && dupaStorno.DurataLuni == 36 && dupaStorno.Valoare == 5250m);

            MotorOperare.AnuleazaOperarea(os, pifModernizare);
            var dupaAnulare = AmortizareService.Situatie(os, idFisa1, Zi(12, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V24/{eticheta}): după anularea modernizării — brut "
                + $"{dupaAnulare.Valoare}, rânduri {CubScena.Fisa(os, [idFisa1], idPifModernizare).Count}.");
            s.Check($"IMO-V24 ({eticheta}) anularea modernizării șterge rândul ei (corecție directă, fără "
                + "dependenți) și brutul revine la 3.600; fișa rămâne în funcțiune — doar intrarea o readuce `Noua`",
                CubScena.Fisa(os, [idFisa1], idPifModernizare).Count == 0
                && dupaAnulare.Valoare == 3600m
                && os.GetObjectByKey<Imobilizare>(idFisa1).Stare == StareImobilizare.InFunctiune);
        }

        // ── IMO-V25…V27: ieșirea din patrimoniu ───────────────────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            var unitate = os.GetObjectByKey<Repartitor>(idUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idGestiune);
            var tipF = os.GetObjectByKey<TipMaterial>(idTipF);

            // Deschidere: valoare culeasă + cumulat inițial, ca ieșirea să aibă două note nenule.
            var pifDeschidere = os.CreateObject<PunereInFunctiune>();
            pifDeschidere.Data = Zi(9, 1);
            pifDeschidere.Predator = unitate;
            pifDeschidere.Primitor = gestiune;
            var linieDeschidere = os.CreateObject<PunereInFunctiuneDetaliu>();
            linieDeschidere.Document = pifDeschidere;
            linieDeschidere.ImobilizareId = idFisa4;
            linieDeschidere.TipMaterialId = tipF.ID;
            linieDeschidere.Fel = FelLiniePif.Intrare;
            linieDeschidere.Valoare = 2400m;
            linieDeschidere.Cantitate = 1m;
            linieDeschidere.AmortizareInitiala = 900m;
            linieDeschidere.AmortizareFiscalaInitiala = 900m;
            linieDeschidere.LuniAmortizateInitial = 9;
            linieDeschidere.Metoda = MetodaAmortizare.Liniara;
            linieDeschidere.DurataLuni = 24;
            linieDeschidere.MetodaFiscala = MetodaAmortizare.Liniara;
            linieDeschidere.DurataFiscalaLuni = 24;
            linieDeschidere.CategorieFiscala = CategorieFiscala.Standard;
            linieDeschidere.UtilizareExclusiva = true;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifDeschidere);
            var deschidere = AmortizareService.Situatie(os, idFisa4, Zi(9, 30));
            s.Check($"IMO-V25 ({eticheta}) intrarea de DESCHIDERE poartă cumulatul și lunile deja amortizate: "
                + "situația pornește de la 2.400 brut, 900 cumulat, 9 luni — migrarea unei fișe cu istoric nu cere "
                + "mecanism separat, e tot un eveniment datat",
                deschidere.Valoare == 2400m && deschidere.Amortizare == 900m
                && deschidere.AmortizareFiscala == 900m && deschidere.Luni == 9);

            var politica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID);
            var cas = os.CreateObject<IesireImobilizare>();
            cas.Data = Zi(10, 20);
            cas.Cauza = CauzaIesire.Casare;
            cas.Predator = gestiune;
            cas.Primitor = unitate;
            IesireImobilizareDetaliu LinieCas(FelLinieIesire fel, Guid? contDebit, decimal valoare) {
                var l = os.CreateObject<IesireImobilizareDetaliu>();
                l.Document = cas;
                l.ImobilizareId = idFisa4;
                l.TipMaterialId = tipF.ID;
                l.Fel = fel;
                l.Valoare = valoare;
                l.Cantitate = 1m;
                l.ContDebitId = contDebit;
                l.ContCreditId = tipF.ContImplicitId;
                l.RepartitorDebitId = gestiune.ID;
                l.RepartitorCreditId = gestiune.ID;
                return l;
            }
            LinieCas(FelLinieIesire.AmortizareCumulata, politica.ContAmortizareId, 900m);
            LinieCas(FelLinieIesire.ValoareRamasa, politica.ContCheltuialaCedareId, 1500m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, cas);

            var fisa4 = os.GetObjectByKey<Imobilizare>(idFisa4);
            var noteCasCub = CubScena.Note(os, cas.ID);
            var randIesire = CubScena.Fisa(os, null, cas.ID).Single().Rand;
            Console.WriteLine($"     MĂSURAT (IMO-V26/{eticheta}): {noteCasCub.Count} postări; rândul de ieșire = "
                + $"{randIesire.Valoare}/{randIesire.Amortizare}; fișa = {fisa4.Stare} din {fisa4.DataIesire}.");
            s.Check($"IMO-V26 ({eticheta}) ieșirea postează DOUĂ note pe conturile din `PoliticaAmortizare` "
                + "(amortizarea cumulată contra contului de imobilizare, valoarea rămasă pe cheltuiala cu cedarea) "
                + "și scrie UN rând de registru cu toate cifrele NEGATIVE; fișa devine `Iesita`",
                noteCasCub.Any(p => p.Debit && p.Cont == politica.ContAmortizareId && p.Valoare == 900m)
                && noteCasCub.Any(p => p.Debit && p.Cont == politica.ContCheltuialaCedareId && p.Valoare == 1500m)
                && noteCasCub.Where(p => p.Credit).All(p => p.Cont == tipF.ContImplicitId)
                && noteCasCub.Rulaj(tipF.ContImplicitId, N.Latura.Credit) == 2400m
                && CubScena.Fisa(os, [idFisa4], cas.ID) is [var randIesireCub]
                && randIesireCub.Rand.Fel == FelMiscareImobilizare.Iesire && randIesireCub.Rand.Valoare == -2400m
                && randIesireCub.Rand.ValoareFiscala == -2400m && randIesireCub.Rand.Amortizare == -900m
                && randIesireCub.Rand.AmortizareFiscala == -900m && randIesireCub.Rand.Luni == 0
                && fisa4.Stare == StareImobilizare.Iesita && fisa4.DataIesire == Zi(10, 20));

            var netDupaIesire = AmortizareService.Situatie(os, idFisa4, Zi(12, 31));
            s.Check($"IMO-V27 ({eticheta}) după ieșire situația fișei e ZERO pe toate coloanele: rândul de ieșire "
                + "anulează exact ce cumulaseră evenimentele — cusătura pe care se va sprijini registrul "
                + "imobilizărilor și SAF-T Assets",
                netDupaIesire.Valoare == 0m && netDupaIesire.ValoareFiscala == 0m
                && netDupaIesire.Amortizare == 0m && netDupaIesire.AmortizareFiscala == 0m);

            // Rolul de STINS e polimorf (86g): nici PIF, nici CAS nu închid vreo datorie.
            var contPropriu = os.GetObjectsQuery<ContPropriu>().OrderBy(c => c.Cod).ToList().First();
            var plata = os.CreateObject<Plata>();
            plata.Data = Zi(10, 25);
            plata.PredatorId = contPropriu.ID;
            plata.PrimitorId = idFurnizor;
            plata.TipInstrument = TipInstrumentPlata.OrdinPlata;
            var liniePlata = os.CreateObject<DocumentTrezorerieDetaliu>();
            liniePlata.Document = plata;
            liniePlata.TipMaterialId = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ").ID;
            liniePlata.Valoare = 100m;
            liniePlata.CodEconomicId = idCodEc;
            liniePlata.SursaFinantareId = idSursaFin;
            liniePlata.CodFunctionalId = idCodFn;
            liniePlata.ProiectId = idProiect;
            os.CommitChanges();
            MotorOperare.Opereaza(os, plata);
            // F27-D8: imperecherea e fapt datat — data se dă explicit, altfel „azi"
            // ar precede înregistrarea documentelor scenei și refuzul de ORDINE ar
            // veni înaintea celui de rol.
            var refuzPif = s.Refuz(() => ImperechereService.Imperecheaza(os, plata, pifDeschidere, 100m, null, Zi(10, 25)));
            var refuzCas = s.Refuz(() => ImperechereService.Imperecheaza(os, plata, cas, 100m, null, Zi(10, 25)));
            Console.WriteLine($"     MĂSURAT (IMO-V29/{eticheta}): stingere PIF → „{refuzPif ?? "<ACCEPTATĂ>"}”; "
                + $"stingere CAS → „{refuzCas ?? "<ACCEPTATĂ>"}”.");
            s.Check($"IMO-V29 ({eticheta}) nici punerea în funcțiune, nici ieșirea nu stau pe rolul de document "
                + "STINS: `PoateFiStins` e false pe amândouă (86g) — PIF nu postează nimic, iar notele ieșirii "
                + "sting valoarea activului, nu un terț. Fără declarație, o notă cu repartitori expliciți ar fi "
                + "putut cădea pe laturile lor și ar fi „stins” un document fără rest",
                refuzPif != null && refuzPif.Contains("nu poate fi stins")
                && refuzCas != null && refuzCas.Contains("nu poate fi stins")
                && os.GetObjectsQuery<Imperechere>()
                    .Count(i => i.DocumentId == pifDeschidere.ID || i.DocumentId == cas.ID) == 0);

            // Fișa ieșită nu mai primește niciun eveniment.
            var pifPeIesita = os.CreateObject<PunereInFunctiune>();
            pifPeIesita.Data = Zi(11, 3);
            pifPeIesita.Predator = unitate;
            pifPeIesita.Primitor = gestiune;
            var liniePeIesita = os.CreateObject<PunereInFunctiuneDetaliu>();
            liniePeIesita.Document = pifPeIesita;
            liniePeIesita.ImobilizareId = idFisa4;
            liniePeIesita.TipMaterialId = tipF.ID;
            liniePeIesita.Fel = FelLiniePif.Modernizare;
            liniePeIesita.Valoare = 100m;
            liniePeIesita.Cantitate = 1m;
            os.CommitChanges();
            s.CheckRefuza($"IMO-V26b ({eticheta}) o modernizare pe o fișă `Iesita` e refuzată: după ieșire fișa nu "
                + "mai primește niciun eveniment — starea ei e materializată de hook, nu culeasă",
                () => ImoFixture.OpereazaCuSuport(os, pifPeIesita));
            os.Delete(pifPeIesita.Detalii.ToList());
            os.Delete(pifPeIesita);
            os.CommitChanges();

            MotorOperare.AnuleazaOperarea(os, cas);
            var fisaDupaAnulare = os.GetObjectByKey<Imobilizare>(idFisa4);
            Console.WriteLine($"     MĂSURAT (IMO-V28/{eticheta}): după anularea ieșirii — {fisaDupaAnulare.Stare}, "
                + $"data ieșirii {fisaDupaAnulare.DataIesire?.ToString("dd.MM.yyyy") ?? "<null>"}.");
            s.Check($"IMO-V28 ({eticheta}) anularea ieșirii readuce fișa `InFunctiune`, îi șterge data ieșirii și "
                + "rândul de registru: corecția directă e simetrică cu materializarea",
                fisaDupaAnulare.Stare == StareImobilizare.InFunctiune && fisaDupaAnulare.DataIesire == null
                && CubScena.Fisa(os, [idFisa4], cas.ID).Count == 0
                && AmortizareService.Situatie(os, idFisa4, Zi(12, 31)).Valoare == 2400m);
        }

        // ── IMO-V30…V51: amortizarea lunară ───────────────────────────────────────
        // Scenă PROPRIE: fișele V8–V28 poartă evenimente stornate sau anulate în
        // mijlocul lunilor 5–8, iar baza lunară le-ar citi ca parametri vii.
        using (var os = s.Provider.CreateObjectSpace())
            CurataImo(os);

        Guid idAmoUnitate, idAmoGestiune, idAmoLoc2, idAmoTip, idAmoPolitica, idAmoCodEc;
        Guid idFa, idFb, idFc, idFd, idFe, idFf, idFg;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 5, 6, 7, 8, 9, 10, 11, 12 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
                p.Inchisa = false;
            }
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-AMO-UI";
            unitate.Denumire = "Unitate AMO probă F26";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-AMO-MAG";
            gestiune.Denumire = "Gestiune AMO probă F26";
            var loc2 = os.CreateObject<Gestiune>();
            loc2.Cod = Marcaj + "-AMO-MAG2";
            loc2.Denumire = "A doua gestiune AMO probă F26";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = Marcaj + "-AMO-CE";
            codEc.Denumire = "Cod economic al amortizării, probă F26";
            os.CommitChanges();
            idAmoUnitate = unitate.ID;
            idAmoGestiune = gestiune.ID;
            idAmoLoc2 = loc2.ID;
            idAmoCodEc = codEc.ID;

            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            idAmoTip = tipF.ID;
            idAmoPolitica = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID).ID;

            Imobilizare Fisa(string sufix, string denumire) {
                var f = os.CreateObject<Imobilizare>();
                f.NumarInventar = Marcaj + "-AMO-" + sufix;
                f.Denumire = denumire;
                f.TipMaterialId = tipF.ID;
                f.LocId = gestiune.ID;
                return f;
            }
            var fa = Fisa("A", "Fișă liniară 3.600 / 36");
            var fb = Fisa("B", "Autoturism 90.000 / 60, neexclusiv");
            var fc = Fisa("C", "Autoturism 120.000 / 60, neexclusiv");
            var fd = Fisa("D", "Autoturism 120.000 / 60, exclusiv");
            var fe = Fisa("E", "Fișă cu valoare reziduală 600");
            var ff = Fisa("F", "Fișă cu durata fiscală 24 și contabilă 36");
            var fg = Fisa("G", "Fișă cu durata contabilă 24 și fiscală 36");
            os.CommitChanges();
            idFa = fa.ID; idFb = fb.ID; idFc = fc.ID; idFd = fd.ID;
            idFe = fe.ID; idFf = ff.ID; idFg = fg.ID;

            var pif = os.CreateObject<PunereInFunctiune>();
            pif.Data = Zi(5, 5);
            pif.Predator = unitate;
            pif.Primitor = gestiune;
            void Intrare(Guid fisaId, decimal valoare, int durata, int durataFiscala,
                    CategorieFiscala categorie, bool exclusiv, decimal? reziduala,
                    decimal amoInitiala, decimal amoFiscalaInitiala, int luniInitiale) {
                var l = os.CreateObject<PunereInFunctiuneDetaliu>();
                l.Document = pif;
                l.ImobilizareId = fisaId;
                l.TipMaterialId = tipF.ID;
                l.Fel = FelLiniePif.Intrare;
                l.Valoare = valoare;
                l.Cantitate = 1m;
                l.Metoda = MetodaAmortizare.Liniara;
                l.DurataLuni = durata;
                l.ValoareReziduala = reziduala;
                l.MetodaFiscala = MetodaAmortizare.Liniara;
                l.DurataFiscalaLuni = durataFiscala;
                l.CategorieFiscala = categorie;
                l.UtilizareExclusiva = exclusiv;
                l.AmortizareInitiala = amoInitiala;
                l.AmortizareFiscalaInitiala = amoFiscalaInitiala;
                l.LuniAmortizateInitial = luniInitiale;
            }
            Intrare(idFa, 3600m, 36, 36, CategorieFiscala.Standard, true, null, 0m, 0m, 0);
            Intrare(idFb, 90000m, 60, 60, CategorieFiscala.VehiculPersoaneMax9Locuri, false, null, 0m, 0m, 0);
            Intrare(idFc, 120000m, 60, 60, CategorieFiscala.VehiculPersoaneMax9Locuri, false, null, 0m, 0m, 0);
            Intrare(idFd, 120000m, 60, 60, CategorieFiscala.VehiculPersoaneMax9Locuri, true, null, 0m, 0m, 0);
            Intrare(idFe, 3600m, 36, 36, CategorieFiscala.Standard, true, 600m, 0m, 0m, 0);
            Intrare(idFf, 3600m, 36, 24, CategorieFiscala.Standard, true, null, 2400m, 3600m, 24);
            Intrare(idFg, 3600m, 24, 36, CategorieFiscala.Standard, true, null, 3600m, 2400m, 24);
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);

            var raportMai = AmortizareService.Previzualizeaza(os, An, 5);
            Console.WriteLine($"     MĂSURAT (IMO-V30/{eticheta}): previzualizarea lunii punerii în funcțiune = "
                + $"{raportMai.Motiv?.ToString() ?? "<se generează>"}, {raportMai.Linii.Count} linii.");
            s.Check($"IMO-V30 ({eticheta}) luna PUNERII în funcțiune nu se amortizează: previzualizarea ei dă "
                + "`FaraFise`, fiindcă eligibilitatea cere data punerii STRICT înaintea primei zile a lunii "
                + "(prima amortizare e luna de după — regula citită la ban din 1C, 51 din 51 de active)",
                raportMai.Motiv == MotivNegenerare.FaraFise && raportMai.Linii.Count == 0);
        }

        // Codul economic al cheltuielii cu amortizarea stă pe FIȘĂ (F26-r16): pe planul
        // bugetar contul de cheltuială îl cere prin defalcare, pe cel privat nimeni nu-l cere.
        var prefixAmo = Marcaj + "-AMO-";
        if (!privat)
            using (var os = s.Provider.CreateObjectSpace()) {
                var faraCod = AmortizareService.Genereaza(os, An, 6, idAmoUnitate);
                os.CommitChanges();
                var refuzFaraCod = s.Refuz(() => MotorOperare.Opereaza(os, faraCod));
                Console.WriteLine($"     MĂSURAT (IMO-V31c/{eticheta}): fișe fără cod economic, operare → "
                    + $"„{refuzFaraCod?.Split(Environment.NewLine[^1])[0] ?? "<ACCEPTATĂ>"}”.");
                s.Check($"IMO-V31c ({eticheta}) fișa FĂRĂ cod economic oprește operarea pe planul bugetar: contul "
                    + "de cheltuială cu amortizarea are defalcarea `E` (decizia 15), iar dimensiunea obligatorie "
                    + "a notei nu se poate inventa în generator — capcana rămâne probă, nu comentariu",
                    refuzFaraCod != null && refuzFaraCod.Contains("Cod economic"));
                os.Delete(faraCod.Detalii.ToList());
                os.Delete(faraCod);
                os.CommitChanges();

                foreach (var fisa in os.GetObjectsQuery<Imobilizare>()
                        .Where(f => f.NumarInventar.StartsWith(prefixAmo)).ToList())
                    fisa.CodEconomicId = idAmoCodEc;
                os.CommitChanges();
            }

        AmortizareLunara AmoOperata(IObjectSpace os, int luna) {
            var rezultat = AmortizareService.Incearca(os, An, luna, idAmoUnitate);
            if (rezultat.Document == null)
                throw new OperareException($"Scena AMO: luna {luna:00}/{An} n-a fost generată ({rezultat.Motiv}).");
            os.CommitChanges();
            MotorOperare.Opereaza(os, rezultat.Document);
            return rezultat.Document;
        }

        // Cele trei cifre ale lunii, pe fiecare formă de fișă (IMO-V31).
        decimal deductibilVehiculNeexclusiv = privat ? 1500m : 2000m;
        using (var os = s.Provider.CreateObjectSpace()) {
            var amo = AmortizareService.Genereaza(os, An, 6, idAmoUnitate);
            os.CommitChanges();

            var linii = amo.Detalii.OfType<AmortizareLunaraDetaliu>()
                .ToDictionary(l => l.ImobilizareId);
            var politica = os.GetObjectByKey<PoliticaAmortizare>(idAmoPolitica);
            string Cifre(Guid fisa) => linii.TryGetValue(fisa, out var l)
                ? $"{l.Valoare}/{l.ValoareFiscala}/{l.ValoareDeductibila}" : "<lipsă>";
            Console.WriteLine($"     MĂSURAT (IMO-V31/{eticheta}): {linii.Count} linii generate; "
                + $"A {Cifre(idFa)}, B {Cifre(idFb)}, C {Cifre(idFc)}, D {Cifre(idFd)}, E {Cifre(idFe)}, "
                + $"F {Cifre(idFf)}, G {Cifre(idFg)}; conturile liniei A = "
                + $"{os.GetObjectByKey<Cont>(linii[idFa].ContDebitId ?? Guid.Empty)?.Simbol} = "
                + $"{os.GetObjectByKey<Cont>(linii[idFa].ContCreditId ?? Guid.Empty)?.Simbol}; "
                + $"conturile liniei G = {linii[idFg].ContDebitId?.ToString() ?? "<null>"}.");
            s.Check($"IMO-V31 ({eticheta}) generatorul scrie o linie per fișă eligibilă, cu DEBIT cheltuiala cu "
                + "amortizarea și CREDIT contul de amortizare, ambele din `PoliticaAmortizare`, și cu locul fișei "
                + "pe ambii repartitori — nicio aritmetică nu trăiește în altă parte",
                linii.Count == 7
                && linii[idFa].Valoare == 100m && linii[idFa].ValoareFiscala == 100m
                && linii[idFa].ValoareDeductibila == 100m
                && linii[idFa].ContDebitId == politica.ContCheltuialaAmortizareId
                && linii[idFa].ContCreditId == politica.ContAmortizareId
                && linii[idFa].RepartitorDebitId == idAmoGestiune
                && linii[idFa].RepartitorCreditId == idAmoGestiune);
            s.Check($"IMO-V32 ({eticheta}) deductibilul e o a TREIA cifră, nu o a doua postare: autoturismul de "
                + "90.000 / 60 (1.500 lunar) intră integral, cel de 120.000 / 60 (2.000 lunar) se taie la plafonul "
                + $"lunar al vehiculelor ({deductibilVehiculNeexclusiv} pe {eticheta}), iar aceeași fișă cu "
                + "UTILIZARE EXCLUSIVĂ trece plafonul neatinsă — regula se alege pe (categorie, utilizare), din date",
                linii[idFb].Valoare == 1500m && linii[idFb].ValoareFiscala == 1500m
                && linii[idFb].ValoareDeductibila == 1500m
                && linii[idFc].Valoare == 2000m && linii[idFc].ValoareFiscala == 2000m
                && linii[idFc].ValoareDeductibila == deductibilVehiculNeexclusiv
                && linii[idFd].Valoare == 2000m && linii[idFd].ValoareDeductibila == 2000m);
            s.Check($"IMO-V33 ({eticheta}) valoarea reziduală scade DOAR baza contabilă: 3.600 cu reziduală 600 pe "
                + "36 de luni dă 83,33 contabil și 100,00 fiscal — fiscul nu recunoaște reziduala",
                linii[idFe].Valoare == 83.33m && linii[idFe].ValoareFiscala == 100m);
            s.Check($"IMO-V34 ({eticheta}) cele două durate curg independent: fișa cu fiscalul deja consumat "
                + "postează contabil 100,00 și fiscal 0, iar fișa cu contabilul consumat rămâne pe document cu "
                + "contabil 0, fiscal 100,00 și conturile NULE — o notă de zero în registru ar fi zgomot, dar "
                + "faptul fiscal al lunii trebuie să existe",
                linii[idFf].Valoare == 100m && linii[idFf].ValoareFiscala == 0m
                && linii[idFf].ContDebitId != null
                && linii[idFg].Valoare == 0m && linii[idFg].ValoareFiscala == 100m
                && linii[idFg].ContDebitId == null && linii[idFg].ContCreditId == null
                && linii[idFg].RepartitorDebitId == idAmoGestiune);

            {
                MotorOperare.Opereaza(os, amo);
                var noteAmoCub = CubScena.Note(os, amo.ID);
                Console.WriteLine($"     MĂSURAT (IMO-V31b/{eticheta}): {noteAmoCub.Count} postări pentru "
                    + $"{linii.Count} linii, {CubScena.Fisa(os, null, amo.ID).Count} rânduri de fișă; "
                    + $"numărul = {amo.Numar}.");
                s.Check($"IMO-V31b ({eticheta}) operarea postează o notă per linie cu contabil NENUL (6 note "
                    + "pentru 7 linii — linia fără cifră contabilă e sărită de motor pe conturile nule) și "
                    + "scrie un rând de registru per fișă, cu cele TREI cifre, `Luni` 1 și locul la data faptului",
                    noteAmoCub.Count == 12
                    && noteAmoCub.All(p => p.Debit ? p.Cont == politica.ContCheltuialaAmortizareId : p.Cont == politica.ContAmortizareId)
                    && CubScena.Fisa(os, linii.Values.Select(l => (Guid)l.ImobilizareId), amo.ID) is { Count: 7 } randuriAmoCub
                    && randuriAmoCub.Single(r => r.Rand.ImobilizareId == idFa).Rand is { Fel: FelMiscareImobilizare.Amortizare,
                        Amortizare: 100m, AmortizareFiscala: 100m, AmortizareDeductibila: 100m, Luni: 1, Valoare: 0m }
                    && CubScena.LocFisa(os, idFa, amo.ID) is [var locACub] && locACub == idAmoGestiune
                    && randuriAmoCub.Single(r => r.Rand.ImobilizareId == idFg).Rand.AmortizareFiscala == 100m
                    && amo.Numar != null && amo.Numar.StartsWith("AMO-"));

                var codPeLinie = linii[idFa].CodEconomicId;
                var codPeNotaCub = noteAmoCub.Where(p => p.Debit).Select(p => p.CodEconomic).Distinct().ToList();
                Console.WriteLine($"     MĂSURAT (IMO-V31d/{eticheta}): cod economic pe fișă "
                    + $"{(privat ? "<null>" : os.GetObjectByKey<CodEconomic>(idAmoCodEc)?.Cod)}, pe linie "
                    + $"{(codPeLinie == null ? "<null>" : "prezent")}, pe debitele postate "
                    + $"{string.Join("/", codPeNotaCub.Select(c => c == null ? "<null>" : "prezent"))}.");
                s.Check($"IMO-V31d ({eticheta}) codul economic al fișei ajunge pe linia amortizării și de acolo, "
                    + "prin `DimensiuniCulese`, pe latura de DEBIT a rândului de registru — pe bugetar e "
                    + "dimensiunea pe care o cere contul, pe privat rămâne null fiindcă nimeni nu o cere",
                    privat
                        ? codPeLinie == null && codPeNotaCub.All(c => c == null)
                        : codPeLinie == idAmoCodEc && codPeNotaCub.Count == 1 && codPeNotaCub[0] == idAmoCodEc);
            }
        }

        // Lanțul lunar complet: cronologie, storno, ieșire — pe AMBELE profiluri, de când
        // codul economic al fișei alimentează nota bugetară (IMO-V31c/V31d).
        // Cota e FIXĂ la ultimul eveniment; modernizarea o mută (IMO-V35…V36).
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 7, 8 })
                AmoOperata(os, luna);
            var situatie = AmortizareService.Situatie(os, idFa, Zi(8, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V35/{eticheta}): după august — cumulat {situatie.Amortizare}, "
                + $"luni {situatie.Luni}, net {situatie.NetContabil}.");
            s.Check($"IMO-V35 ({eticheta}) cota rămâne FIXĂ de la ultimul eveniment: trei luni × 100,00 fac 300,00 "
                + "cumulat și 3 luni — o recalculare „rest / rest” ar fi produs derivă (665,17 → 665,18 în 1C)",
                situatie.Amortizare == 300m && situatie.Luni == 3 && situatie.NetContabil == 3300m);

            var unitate = os.GetObjectByKey<Repartitor>(idAmoUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idAmoGestiune);
            var pifModernizare = os.CreateObject<PunereInFunctiune>();
            pifModernizare.Data = Zi(8, 15);
            pifModernizare.Predator = unitate;
            pifModernizare.Primitor = gestiune;
            var linieModernizare = os.CreateObject<PunereInFunctiuneDetaliu>();
            linieModernizare.Document = pifModernizare;
            linieModernizare.ImobilizareId = idFa;
            linieModernizare.TipMaterialId = idAmoTip;
            linieModernizare.Fel = FelLiniePif.Modernizare;
            linieModernizare.Valoare = 1650m;
            linieModernizare.Cantitate = 1m;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifModernizare);

            var septembrie = AmoOperata(os, 9);
            var linieA = septembrie.Detalii.OfType<AmortizareLunaraDetaliu>().Single(l => l.ImobilizareId == idFa);
            Console.WriteLine($"     MĂSURAT (IMO-V36/{eticheta}): septembrie, fișa A = {linieA.Valoare} "
                + $"(brut {AmortizareService.Situatie(os, idFa, Zi(9, 30)).Valoare}).");
            s.Check($"IMO-V36 ({eticheta}) modernizarea din mijlocul lunii (15.08) rebazează calculul lunii "
                + "URMĂTOARE: (3.600 + 1.650 − 300) / (36 − 3) = 150,00 — baza se citește la SFÂRȘITUL lunii "
                + "evenimentului (formula 1C: luna evenimentului postează încă cota veche), indiferent de zi",
                linieA.Valoare == 150m && linieA.ValoareFiscala == 150m && linieA.ValoareDeductibila == 150m);
        }

        // Regula de deductibilitate cu `DeLa` în mijlocul vieții activului (probată la IMO-V40).
        using (var os = s.Provider.CreateObjectSpace()) {
            var regula = os.CreateObject<RegulaDeductibilitate>();
            regula.Categorie = CategorieFiscala.VehiculPersoaneMax9Locuri;
            regula.DoarNeexclusiv = true;
            regula.Fel = FelDeductibilitate.PlafonLunar;
            regula.Valoare = 1000m;
            regula.DeLa = Zi(10, 1);
            regula.Temei = Marcaj + " — plafon de scenă, valabil din 10/2027";
            os.CommitChanges();
        }

        // Anti-stale-ul operării și regenerarea (IMO-V38…V40).
        Guid idAmoOctombrie;
        using (var os = s.Provider.CreateObjectSpace()) {
            var draft = AmortizareService.Genereaza(os, An, 10, idAmoUnitate);
            os.CommitChanges();
            var linieVeche = draft.Detalii.OfType<AmortizareLunaraDetaliu>().Single(l => l.ImobilizareId == idFa);
            var valoareLaGenerare = linieVeche.Valoare;

            var unitate = os.GetObjectByKey<Repartitor>(idAmoUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idAmoGestiune);
            var pifRevizuire = os.CreateObject<PunereInFunctiune>();
            pifRevizuire.Data = Zi(9, 30);
            pifRevizuire.Predator = unitate;
            pifRevizuire.Primitor = gestiune;
            var linieRevizuire = os.CreateObject<PunereInFunctiuneDetaliu>();
            linieRevizuire.Document = pifRevizuire;
            linieRevizuire.ImobilizareId = idFa;
            linieRevizuire.TipMaterialId = idAmoTip;
            linieRevizuire.Fel = FelLiniePif.Revizuire;
            linieRevizuire.Valoare = 0m;
            linieRevizuire.Cantitate = 1m;
            linieRevizuire.Metoda = MetodaAmortizare.Liniara;
            linieRevizuire.DurataLuni = 48;
            linieRevizuire.MetodaFiscala = MetodaAmortizare.Liniara;
            linieRevizuire.DurataFiscalaLuni = 48;
            linieRevizuire.CategorieFiscala = CategorieFiscala.Standard;
            linieRevizuire.UtilizareExclusiva = true;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pifRevizuire);

            Console.WriteLine($"     MĂSURAT (IMO-V38/{eticheta}): draftul lui octombrie poartă {valoareLaGenerare} "
                + "pe fișa A, iar revizuirea a mutat durata pe 48.");
            s.CheckRefuza($"IMO-V38 ({eticheta}) un draft de amortizare generat ÎNAINTE de o revizuire operată nu se "
                + "mai poate opera: gardianul recalculează mulțimea liniilor din registru și refuză — altfel luna "
                + "ar posta cifre care nu mai descriu nicio bază",
                () => MotorOperare.Opereaza(os, draft));

            var regenerat = AmortizareService.Incearca(os, An, 10, idAmoUnitate, draft.ID);
            os.Delete(draft.Detalii.ToList());
            os.Delete(draft);
            os.CommitChanges();
            MotorOperare.Opereaza(os, regenerat.Document);
            idAmoOctombrie = regenerat.Document.ID;
            var linieNoua = regenerat.Document.Detalii.OfType<AmortizareLunaraDetaliu>()
                .Single(l => l.ImobilizareId == idFa);
            var linieC = regenerat.Document.Detalii.OfType<AmortizareLunaraDetaliu>()
                .Single(l => l.ImobilizareId == idFc);
            Console.WriteLine($"     MĂSURAT (IMO-V39/{eticheta}): octombrie regenerat — fișa A "
                + $"{linieNoua.Valoare}, fișa C deductibil {linieC.ValoareDeductibila} "
                + $"(septembrie: {deductibilVehiculNeexclusiv}).");
            s.Check($"IMO-V39 ({eticheta}) regenerarea cu `inlocuieste` trece pe lângă gardianul „luna are deja o "
                + "amortizare” și produce cifra nouă: (5.250 − 450) / (48 − 4) = 109,09 — durata revizuită e fapt "
                + "datat, iar recalculul rămâne reproductibil",
                linieNoua.Valoare == 109.09m && linieNoua.ValoareFiscala == 109.09m
                && linieNoua.ValoareDeductibila == 109.09m);
            s.Check($"IMO-V40 ({eticheta}) o regulă de deductibilitate cu `DeLa` în mijlocul vieții activului "
                + "schimbă DOAR lunile de după: plafonul de scenă (1.000 din 10/2027) taie deductibilul "
                + "autoturismului de 120.000 în octombrie, iar septembrie rămâne cum a fost postat — legea se "
                + "exprimă în date, iar cifrele deja postate sunt fapte",
                linieC.ValoareDeductibila == 1000m && deductibilVehiculNeexclusiv != 1000m);
        }

        // Storno-ul amortizării și cronologia strictă (IMO-V41…V42).
        using (var os = s.Provider.CreateObjectSpace()) {
            var octombrie = os.GetObjectByKey<AmortizareLunara>(idAmoOctombrie);
            MotorOperare.Storneaza(os, octombrie, Zi(10, 31));
            var contabile = CubScena.Note(os, idAmoOctombrie);
            var imobilizari = CubScena.Fisa(os, null, idAmoOctombrie);
            var situatieA = AmortizareService.Situatie(os, idFa, Zi(10, 31));
            Console.WriteLine($"     MĂSURAT (IMO-V41/{eticheta}): după storno — {contabile.Count} postări "
                + $"contabile ({contabile.Count(p => p.Storno)} inverse), {imobilizari.Count} rânduri de "
                + $"fișă ({imobilizari.Count(r => r.Rand.Storno)} inverse); cumulat A {situatieA.Amortizare}.");
            s.Check($"IMO-V41 ({eticheta}) storno-ul amortizării adaugă rânduri INVERSE în AMBELE registre "
                + "(contabil și imobilizări), append-only, iar situația fișei revine la cea de dinaintea lunii — "
                + "al patrulea registru urmează exact ciclul de viață al celorlalte trei",
                contabile.Count > 0 && contabile.Count(p => p.Storno) == contabile.Count / 2
                && imobilizari.Count > 0 && imobilizari.Count(r => r.Rand.Storno) == imobilizari.Count / 2
                && situatieA.Amortizare == 450m && situatieA.Luni == 4);

            var refacut = AmoOperata(os, 10);
            idAmoOctombrie = refacut.ID;
            s.Check($"IMO-V42 ({eticheta}) după storno luna redevine LIBERĂ: „amortizare vie” nu vede documentul "
                + "stornat, deci octombrie se regenerează și se operează din nou, cu aceleași cifre",
                refacut.Detalii.OfType<AmortizareLunaraDetaliu>().Single(l => l.ImobilizareId == idFa)
                    .Valoare == 109.09m);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var dataAugust = Zi(8, 31);
            var august = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataAugust && a.Stare == StareDocument.Operat);
            s.CheckRefuza($"IMO-V43 ({eticheta}) anularea amortizării lui august, cu lunile următoare operate, e "
                + "refuzată de FRUNZĂ: cronologia amortizării e strictă, fiindcă fiecare lună se calculează pe "
                + "cumulatul celei dinainte",
                () => MotorOperare.AnuleazaOperarea(os, august));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var dataSeptembrie = Zi(9, 30);
            var septembrie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataSeptembrie && a.Stare == StareDocument.Operat);
            s.CheckRefuza($"IMO-V44 ({eticheta}) stornarea amortizării lui septembrie, cu octombrie operată, e "
                + "refuzată pe același gardian — corecția se face de la capătul cronologiei spre trecut",
                () => MotorOperare.Storneaza(os, septembrie, Zi(10, 31)));
        }

        // Cronologia la GENERARE: luna lipsă și perioada închisă (IMO-V45…V46).
        using (var os = s.Provider.CreateObjectSpace()) {
            var raportDecembrie = AmortizareService.Previzualizeaza(os, An, 12);
            Console.WriteLine($"     MĂSURAT (IMO-V45/{eticheta}): decembrie cu noiembrie negenerată = "
                + $"{raportDecembrie.Motiv}, {raportDecembrie.Linii.Count} linii însoțitoare.");
            s.Check($"IMO-V45 ({eticheta}) decembrie, cu noiembrie negenerată deși avea fișe eligibile, iese ca "
                + "`LunaLipsa` la RAPORT — un ecran trebuie să poată spune „nu se poate, fiindcă…”, nu să "
                + "primească o excepție",
                raportDecembrie.Motiv == MotivNegenerare.LunaLipsa && raportDecembrie.Linii.Count > 0);
            s.CheckRefuza($"IMO-V45b ({eticheta}) aceeași stare, la COMANDĂ, e refuz zgomotos — raportul și comanda "
                + "pun aceleași întrebări, în aceeași ordine, și diferă printr-un singur bit",
                () => AmortizareService.Incearca(os, An, 12, idAmoUnitate));

            var noiembrie = os.FirstOrDefault<PerioadaFiscala>(p => p.An == An && p.Luna == 11);
            noiembrie.Inchisa = true;
            os.CommitChanges();
            var raportInchis = AmortizareService.Previzualizeaza(os, An, 11);
            Console.WriteLine($"     MĂSURAT (IMO-V46/{eticheta}): noiembrie închisă = {raportInchis.Motiv}.");
            s.Check($"IMO-V46 ({eticheta}) perioada închisă e graniță și la GENERARE, nu doar la operare: un draft "
                + "într-o lună închisă n-ar putea fi operat niciodată, dar ar sta ca „amortizare vie” și ar bloca "
                + "cronologia lunilor dinaintea lui",
                raportInchis.Motiv == MotivNegenerare.PerioadaInchisa);
            s.CheckRefuza($"IMO-V46b ({eticheta}) comanda pe luna închisă aruncă, ca la închiderea de TVA",
                () => AmortizareService.Incearca(os, An, 11, idAmoUnitate));
            noiembrie.Inchisa = false;
            os.CommitChanges();
        }

        // Transferul, ieșirea și lunile de după (IMO-V47…V49).
        using (var os = s.Provider.CreateObjectSpace()) {
            var fisaE = os.GetObjectByKey<Imobilizare>(idFe);
            fisaE.LocId = idAmoLoc2;
            os.CommitChanges();

            var unitate = os.GetObjectByKey<Repartitor>(idAmoUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idAmoGestiune);
            var politica = os.GetObjectByKey<PoliticaAmortizare>(idAmoPolitica);
            var tipF = os.GetObjectByKey<TipMaterial>(idAmoTip);
            var situatieA = AmortizareService.Situatie(os, idFa, Zi(11, 15));

            var cas = os.CreateObject<IesireImobilizare>();
            cas.Data = Zi(11, 15);
            cas.Cauza = CauzaIesire.Casare;
            cas.Predator = gestiune;
            cas.Primitor = unitate;
            IesireImobilizareDetaliu LinieCas(FelLinieIesire fel, Guid? contDebit, decimal valoare) {
                var l = os.CreateObject<IesireImobilizareDetaliu>();
                l.Document = cas;
                l.ImobilizareId = idFa;
                l.TipMaterialId = tipF.ID;
                l.Fel = fel;
                l.Valoare = valoare;
                l.Cantitate = 1m;
                l.ContDebitId = contDebit;
                l.ContCreditId = tipF.ContImplicitId;
                l.RepartitorDebitId = gestiune.ID;
                l.RepartitorCreditId = gestiune.ID;
                return l;
            }
            LinieCas(FelLinieIesire.AmortizareCumulata, politica.ContAmortizareId, situatieA.Amortizare);
            var linieRest = LinieCas(FelLinieIesire.ValoareRamasa, politica.ContCheltuialaCedareId,
                situatieA.NetContabil - 10m);
            os.CommitChanges();
            Console.WriteLine($"     MĂSURAT (IMO-V47/{eticheta}): cumulat {situatieA.Amortizare}, net "
                + $"{situatieA.NetContabil}; linia de rest culeasă cu {linieRest.Valoare}.");
            s.CheckRefuza($"IMO-V47 ({eticheta}) ieșirea cu linia de valoare rămasă greșită e refuzată: serviciul "
                + "recalculează cele două linii din registru la data documentului și cere potrivirea exactă — "
                + "același anti-stale ca la închiderea de TVA",
                () => MotorOperare.Opereaza(os, cas));
            linieRest.Valoare = situatieA.NetContabil;
            os.CommitChanges();
            MotorOperare.Opereaza(os, cas);

            var fisaA = os.GetObjectByKey<Imobilizare>(idFa);
            var noteCasCub = CubScena.Note(os, cas.ID);
            var randIesire = CubScena.Fisa(os, [idFa], cas.ID).Single().Rand;
            Console.WriteLine($"     MĂSURAT (IMO-V48/{eticheta}): {noteCasCub.Count} postări"
                + $"; rândul de ieșire {randIesire.Valoare}/{randIesire.Amortizare}; fișa {fisaA.Stare}.");
            s.Check($"IMO-V48 ({eticheta}) casarea unei fișe amortizate parțial descarcă exact cumulatul lunilor "
                + "postate (5 luni: 100 + 100 + 100 + 150 + 109,09 = 559,09) și trece restul pe cheltuiala cu "
                + "cedarea; rândul de registru e negativ pe toate coloanele, iar fișa devine `Iesita`",
                situatieA.Amortizare == 559.09m && situatieA.NetContabil == 4690.91m
                && noteCasCub.Any(p => p.Debit && p.Cont == politica.ContAmortizareId && p.Valoare == 559.09m)
                && noteCasCub.Any(p => p.Debit && p.Cont == politica.ContCheltuialaCedareId && p.Valoare == 4690.91m)
                && CubScena.Fisa(os, [idFa], cas.ID) is [var randIesireCub]
                && randIesireCub.Rand.Valoare == -5250m && randIesireCub.Rand.Amortizare == -559.09m
                && fisaA.Stare == StareImobilizare.Iesita && fisaA.DataIesire == Zi(11, 15));

            var amoNoiembrie = AmoOperata(os, 11);
            var liniiNoiembrie = amoNoiembrie.Detalii.OfType<AmortizareLunaraDetaliu>().ToList();
            var linieE = liniiNoiembrie.Single(l => l.ImobilizareId == idFe);
            Console.WriteLine($"     MĂSURAT (IMO-V49/{eticheta}): noiembrie are {liniiNoiembrie.Count} linii; "
                + $"fișa A prezentă: {liniiNoiembrie.Any(l => l.ImobilizareId == idFa)}; "
                + $"locul fișei E pe linie/postări = "
                + $"{os.GetObjectByKey<Repartitor>(linieE.RepartitorDebitId ?? Guid.Empty)?.Cod}/"
                + $"{string.Join(",", CubScena.LocFisa(os, idFe, amoNoiembrie.ID).Select(g => os.GetObjectByKey<Repartitor>(g)?.Cod))}.");
            s.Check($"IMO-V49 ({eticheta}) luna IEȘIRII nu se amortizează (fișa casată pe 15.11 lipsește din "
                + "amortizarea lui noiembrie), iar transferul administrativ al unei alte fișe mută postarea și "
                + "rândul de registru pe NOUL loc — istoricul locului sunt rândurile lunare (F26-D8)",
                liniiNoiembrie.Count == 6 && !liniiNoiembrie.Any(l => l.ImobilizareId == idFa)
                && linieE.RepartitorDebitId == idAmoLoc2
                && CubScena.LocFisa(os, idFe, amoNoiembrie.ID) is [var locECub] && locECub == idAmoLoc2);

            var casTarziu = os.CreateObject<IesireImobilizare>();
            casTarziu.Data = Zi(11, 20);
            casTarziu.Cauza = CauzaIesire.Vanzare;
            casTarziu.Predator = gestiune;
            casTarziu.Primitor = unitate;
            var situatieB = AmortizareService.Situatie(os, idFb, Zi(11, 20));
            var lTarziu = os.CreateObject<IesireImobilizareDetaliu>();
            lTarziu.Document = casTarziu;
            lTarziu.ImobilizareId = idFb;
            lTarziu.TipMaterialId = tipF.ID;
            lTarziu.Fel = FelLinieIesire.AmortizareCumulata;
            lTarziu.Valoare = situatieB.Amortizare;
            lTarziu.Cantitate = 1m;
            lTarziu.ContDebitId = politica.ContAmortizareId;
            lTarziu.ContCreditId = tipF.ContImplicitId;
            lTarziu.RepartitorDebitId = gestiune.ID;
            lTarziu.RepartitorCreditId = gestiune.ID;
            var lTarziuRest = os.CreateObject<IesireImobilizareDetaliu>();
            lTarziuRest.Document = casTarziu;
            lTarziuRest.ImobilizareId = idFb;
            lTarziuRest.TipMaterialId = tipF.ID;
            lTarziuRest.Fel = FelLinieIesire.ValoareRamasa;
            lTarziuRest.Valoare = situatieB.NetContabil;
            lTarziuRest.Cantitate = 1m;
            lTarziuRest.ContDebitId = politica.ContCheltuialaCedareId;
            lTarziuRest.ContCreditId = tipF.ContImplicitId;
            lTarziuRest.RepartitorDebitId = gestiune.ID;
            lTarziuRest.RepartitorCreditId = gestiune.ID;
            os.CommitChanges();
            s.CheckRefuza($"IMO-V50 ({eticheta}) o ieșire într-o lună cu amortizarea deja OPERATĂ e refuzată: luna "
                + "ieșirii nu se amortizează, deci amortizarea aceea ar rămâne pe o fișă care nu mai există în "
                + "patrimoniu",
                () => MotorOperare.Opereaza(os, casTarziu));
            os.Delete(casTarziu.Detalii.ToList());
            os.Delete(casTarziu);
            os.CommitChanges();

            var amoDecembrie = AmoOperata(os, 12);
            var liniiDecembrie = amoDecembrie.Detalii.OfType<AmortizareLunaraDetaliu>().ToList();
            Console.WriteLine($"     MĂSURAT (IMO-V51/{eticheta}): decembrie are {liniiDecembrie.Count} linii; "
                + $"fișa A prezentă: {liniiDecembrie.Any(l => l.ImobilizareId == idFa)}.");
            s.Check($"IMO-V51 ({eticheta}) fișa ieșită nu mai apare în NICIO lună ulterioară, iar cronologia "
                + "completată (noiembrie operată) deblochează decembrie — eligibilitatea se citește din DATE, nu "
                + "din starea materializată a fișei",
                liniiDecembrie.Count == 6 && !liniiDecembrie.Any(l => l.ImobilizareId == idFa));
        }

        // Ieșirea ca DEPENDENT al lunii dinaintea ei (IMO-V51b…V51d; defect găsit la smoke-ul XAF).
        Guid idCasB;
        using (var os = s.Provider.CreateObjectSpace()) {
            var dataDecembrie = Zi(12, 31);
            var decembrie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataDecembrie && a.Stare == StareDocument.Operat);
            MotorOperare.Storneaza(os, decembrie, dataDecembrie);

            var unitate = os.GetObjectByKey<Repartitor>(idAmoUnitate);
            var gestiune = os.GetObjectByKey<Repartitor>(idAmoGestiune);
            var politica = os.GetObjectByKey<PoliticaAmortizare>(idAmoPolitica);
            var tipF = os.GetObjectByKey<TipMaterial>(idAmoTip);
            var situatieB = AmortizareService.Situatie(os, idFb, Zi(12, 20));
            var casB = os.CreateObject<IesireImobilizare>();
            casB.Data = Zi(12, 20);
            casB.Cauza = CauzaIesire.Vanzare;
            casB.Predator = gestiune;
            casB.Primitor = unitate;
            foreach (var (fel, cont, valoare) in new[] {
                    (FelLinieIesire.AmortizareCumulata, politica.ContAmortizareId, situatieB.Amortizare),
                    (FelLinieIesire.ValoareRamasa, politica.ContCheltuialaCedareId, situatieB.NetContabil) }) {
                var l = os.CreateObject<IesireImobilizareDetaliu>();
                l.Document = casB;
                l.ImobilizareId = idFb;
                l.TipMaterialId = tipF.ID;
                l.Fel = fel;
                l.Valoare = valoare;
                l.Cantitate = 1m;
                l.ContDebitId = cont;
                l.ContCreditId = tipF.ContImplicitId;
                l.RepartitorDebitId = gestiune.ID;
                l.RepartitorCreditId = gestiune.ID;
            }
            os.CommitChanges();
            MotorOperare.Opereaza(os, casB);
            idCasB = casB.ID;
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var dataNoiembrie = Zi(11, 30);
            var noiembrie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataNoiembrie && a.Stare == StareDocument.Operat);
            s.CheckRefuza($"IMO-V51b ({eticheta}) anularea amortizării lui noiembrie, cu decembrie stornată dar cu "
                + "o IEȘIRE operată pe 20.12 pe o fișă a ei, e refuzată: cumulatul descărcat de ieșire s-a calculat "
                + "pe rândul lunar al lui noiembrie — faptele ulterioare ale fișelor sunt dependenți, nu doar "
                + "lunile următoare",
                () => MotorOperare.AnuleazaOperarea(os, noiembrie));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var dataNoiembrie = Zi(11, 30);
            var noiembrie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataNoiembrie && a.Stare == StareDocument.Operat);
            s.CheckRefuza($"IMO-V51c ({eticheta}) stornarea aceleiași luni e refuzată pe același gardian",
                () => MotorOperare.Storneaza(os, noiembrie, Zi(12, 31)));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            MotorOperare.Storneaza(os, os.GetObjectByKey<IesireImobilizare>(idCasB), Zi(12, 31));
            var dataNoiembrie = Zi(11, 30);
            var noiembrie = os.GetObjectsQuery<AmortizareLunara>()
                .Single(a => a.Data == dataNoiembrie && a.Stare == StareDocument.Operat);
            MotorOperare.AnuleazaOperarea(os, noiembrie);
            s.Check($"IMO-V51d ({eticheta}) după stornarea ieșirii, noiembrie se anulează: dependenții stornați nu "
                + "mai contează, corecția merge de la capătul cronologiei spre trecut",
                noiembrie.Stare == StareDocument.Draft
                && CubScena.Fisa(os, null, noiembrie.ID).Count == 0);
        }

        // ── IMO-V52: fișa eligibilă fără politică de amortizare ───────────────────
        using (var os = s.Provider.CreateObjectSpace())
            CurataImo(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 5, 6 }) {
                var p = os.CreateObject<PerioadaFiscala>();
                p.An = An;
                p.Luna = luna;
                p.Inchisa = false;
            }
            var unitate = os.CreateObject<UnitateInterna>();
            unitate.Cod = Marcaj + "-FP-UI";
            unitate.Denumire = "Unitate probă fără politică";
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-FP-MAG";
            gestiune.Denumire = "Gestiune probă fără politică";
            var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
            var tipNou = os.CreateObject<TipMaterial>();
            tipNou.Cod = Marcaj + "-TIPF";
            tipNou.Denumire = "Tip de imobilizări fără politică de amortizare";
            tipNou.ClasaId = tipF.ClasaId;
            tipNou.ContImplicitId = tipF.ContImplicitId;
            os.CommitChanges();

            var fisa = os.CreateObject<Imobilizare>();
            fisa.NumarInventar = Marcaj + "-FP-1";
            fisa.Denumire = "Fișă pe un tip fără politică";
            fisa.TipMaterialId = tipNou.ID;
            fisa.LocId = gestiune.ID;
            os.CommitChanges();

            var pif = os.CreateObject<PunereInFunctiune>();
            pif.Data = Zi(5, 5);
            pif.Predator = unitate;
            pif.Primitor = gestiune;
            var l = os.CreateObject<PunereInFunctiuneDetaliu>();
            l.Document = pif;
            l.ImobilizareId = fisa.ID;
            l.TipMaterialId = tipNou.ID;
            l.Fel = FelLiniePif.Intrare;
            l.Valoare = 1200m;
            l.Cantitate = 1m;
            l.Metoda = MetodaAmortizare.Liniara;
            l.DurataLuni = 12;
            l.MetodaFiscala = MetodaAmortizare.Liniara;
            l.DurataFiscalaLuni = 12;
            l.CategorieFiscala = CategorieFiscala.Standard;
            l.UtilizareExclusiva = true;
            os.CommitChanges();
            ImoFixture.OpereazaCuSuport(os, pif);

            var faraPolitica = AmortizareService.Previzualizeaza(os, An, 6);
            Console.WriteLine($"     MĂSURAT (IMO-V52/{eticheta}): {faraPolitica.Motiv} pe „{faraPolitica.Detaliu}”.");
            s.Check($"IMO-V52 ({eticheta}) o fișă ELIGIBILĂ al cărei tip n-are rând de `PoliticaAmortizare` "
                + "oprește luna INTEGRAL, cu numărul de inventar în verdict: conturile vin exclusiv din politică, "
                + "iar o lună generată fără fișa aceea ar fi tăcut despre o cheltuială nepostată",
                faraPolitica.Motiv == MotivNegenerare.FisaFaraPolitica
                && faraPolitica.Detaliu == Marcaj + "-FP-1");

            var politicaNoua = os.CreateObject<PoliticaAmortizare>();
            politicaNoua.TipMaterialId = tipNou.ID;
            var politicaF = os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID);
            politicaNoua.ContAmortizareId = politicaF.ContAmortizareId;
            politicaNoua.ContCheltuialaAmortizareId = politicaF.ContCheltuialaAmortizareId;
            politicaNoua.ContCheltuialaCedareId = politicaF.ContCheltuialaCedareId;
            os.CommitChanges();
            var cuPolitica = AmortizareService.Previzualizeaza(os, An, 6);
            Console.WriteLine($"     MĂSURAT (IMO-V52b/{eticheta}): după rândul de politică — "
                + $"{cuPolitica.Motiv?.ToString() ?? "<se generează>"}, {cuPolitica.Linii.Count} linii, "
                + $"{cuPolitica.Linii.FirstOrDefault()?.Contabil}.");
            s.Check($"IMO-V52b ({eticheta}) un rând nou de politică deblochează luna fără release: politica e DATE, "
                + "iar generatorul o citește la fiecare rulare",
                cuPolitica.Motiv == null && cuPolitica.Linii.Count == 1
                && cuPolitica.Linii[0].Contabil == 100m);
        }

        // ── IMO-V53…V55: aritmetica PURĂ ──────────────────────────────────────────
        {
            decimal Cota(MetodaAmortizare metoda, decimal valoare, int luniRamase, int luniDeLaEveniment,
                    decimal rest, decimal brut, int luniDeLaPunere) =>
                AmortizareService.CotaLunara(new BazaAmortizare(metoda, valoare, luniRamase, luniDeLaEveniment,
                    rest, brut, luniDeLaPunere));

            List<decimal> Grafic(MetodaAmortizare metoda, decimal valoare, int luni, decimal brut) {
                var rest = valoare;
                var sume = new List<decimal>();
                for (var i = 0; i < luni + 2 && rest > 0m; i++) {
                    var suma = Cota(metoda, valoare, luni, i, rest, brut, i);
                    sume.Add(suma);
                    rest -= suma;
                }
                return sume;
            }

            var citan = Grafic(MetodaAmortizare.Liniara, 21950.68m, 33, 21950.68m);
            var zebra = Grafic(MetodaAmortizare.Liniara, 3455.11m, 36, 3455.11m);
            var centruIt = Cota(MetodaAmortizare.Liniara, 636538.78m, 507, 0, 636538.78m, 636538.78m, 0);
            var invertor = Cota(MetodaAmortizare.Liniara, 47323.77m, 36, 0, 47323.77m, 47323.77m, 0);
            Console.WriteLine($"     MĂSURAT (IMO-V53/{eticheta}): Citan {citan.Count} luni "
                + $"({citan[0]} × {citan.Count - 1} + {citan[^1]}, total {citan.Sum()}); "
                + $"Zebra {zebra.Count} luni ({zebra[0]} × {zebra.Count - 1} + {zebra[^1]}, total {zebra.Sum()}); "
                + $"CENTRU IT {centruIt}; invertor {invertor}.");
            s.Check($"IMO-V53 ({eticheta}) funcția PURĂ reproduce la ban cifrele reale din 1C: cota rotunjită în "
                + "JOS lasă o lună SUPLIMENTARĂ cu restul (Citan: 33 × 665,17 + 0,07), cota rotunjită în SUS "
                + "scurtează ultima lună (Zebra: 35 × 95,98 + 95,81), iar totalul postat egalează exact valoarea "
                + "de amortizat",
                citan.Count == 34 && citan.Take(33).All(s => s == 665.17m) && citan[^1] == 0.07m
                && citan.Sum() == 21950.68m
                && zebra.Count == 36 && zebra.Take(35).All(s => s == 95.98m) && zebra[^1] == 95.81m
                && zebra.Sum() == 3455.11m
                && centruIt == 1255.50m && invertor == 1314.55m);

            var accelerataInceput = Cota(MetodaAmortizare.Accelerata, 3600m, 36, 0, 3600m, 3600m, 0);
            var accelerataLuna12 = Cota(MetodaAmortizare.Accelerata, 3600m, 36, 11, 1950m, 3600m, 11);
            var accelerataDupa = Cota(MetodaAmortizare.Accelerata, 1800m, 24, 0, 1800m, 3600m, 12);
            Console.WriteLine($"     MĂSURAT (IMO-V54/{eticheta}): accelerata 3.600 / 36 = "
                + $"{accelerataInceput} (luna 1), {accelerataLuna12} (luna 12), {accelerataDupa} (luna 13).");
            s.Check($"IMO-V54 ({eticheta}) accelerata pune 50 % din brut în primele 12 luni (150,00 pe lună pentru "
                + "3.600) și trece apoi la liniar pe restul / lunile rămase (1.800 / 24 = 75,00) — pragul e "
                + "lunile de la PUNEREA în funcțiune, nu de la ultimul eveniment",
                accelerataInceput == 150m && accelerataLuna12 == 150m && accelerataDupa == 75m);

            var degresiva = Grafic(MetodaAmortizare.Degresiva, 60000m, 60, 60000m);
            var peAni = Enumerable.Range(0, 5).Select(a => degresiva.Skip(a * 12).Take(12).Sum()).ToList();
            Console.WriteLine($"     MĂSURAT (IMO-V55/{eticheta}): degresiva AD1 60.000 / 60 pe ani = "
                + string.Join(" / ", peAni) + $"; total {degresiva.Sum()} în {degresiva.Count} luni.");
            s.Check($"IMO-V55 ({eticheta}) degresiva AD1 pe 5 ani (k = 1,5) dă 18.000 / 12.600 / 9.800 / 9.800 / "
                + "9.800: din anul în care rata degresivă nu mai bate media rămasă graficul trece la liniar și "
                + "rămâne acolo, a douăsprezecea lună a fiecărui an absoarbe restul anului, iar totalul e exact "
                + "valoarea de amortizat",
                degresiva.Count == 60 && degresiva.Sum() == 60000m
                && peAni[0] == 18000m && peAni[1] == 12600m && peAni[2] == 9800m
                && peAni[3] == 9800m && peAni[4] == 9800m);
        }

        // ── IMO-V56…V57: `Deductibil` pur ─────────────────────────────────────────
        {
            var vehicul = CategorieFiscala.VehiculPersoaneMax9Locuri;
            var sediu = CategorieFiscala.SediuSocialInLocuinta;
            var data = new DateOnly(2027, 6, 30);
            var plafon = new List<RegulaSnapshot> {
                new(vehicul, true, FelDeductibilitate.PlafonLunar, 1500m, new DateOnly(2012, 2, 1), null),
            };
            var doua = new List<RegulaSnapshot> {
                new(sediu, true, FelDeductibilitate.Procent, 0m, new DateOnly(2024, 1, 1), null),
                new(sediu, true, FelDeductibilitate.Procent, 50m, new DateOnly(2026, 1, 1), null),
            };
            var expirat = new List<RegulaSnapshot> {
                new(vehicul, true, FelDeductibilitate.PlafonLunar, 1500m, new DateOnly(2012, 2, 1),
                    new DateOnly(2020, 12, 31)),
            };
            var ambele = new List<RegulaSnapshot> {
                new(vehicul, true, FelDeductibilitate.PlafonLunar, 1500m, new DateOnly(2012, 2, 1), null),
                new(vehicul, true, FelDeductibilitate.Procent, 50m, new DateOnly(2012, 2, 1), null),
            };
            decimal D(decimal fiscal, CategorieFiscala categorie, bool exclusiv, List<RegulaSnapshot> reguli,
                    DateOnly cand) => AmortizareService.Deductibil(fiscal, categorie, exclusiv, reguli, cand);

            var faraRegula = D(2000m, vehicul, false, [], data);
            var cuPlafon = D(2000m, vehicul, false, plafon, data);
            var laExclusiv = D(2000m, vehicul, true, plafon, data);
            var altaCategorie = D(2000m, CategorieFiscala.Standard, false, plafon, data);
            var inainteDeLa = D(2000m, vehicul, false, plafon, new DateOnly(2011, 12, 31));
            var deLa2025 = D(1000m, sediu, false, doua, new DateOnly(2025, 6, 30));
            var deLa2027 = D(1000m, sediu, false, doua, data);
            var dupaPanaLa = D(2000m, vehicul, false, expirat, data);
            var plafonApoiProcent = D(2000m, vehicul, false, ambele, data);
            Console.WriteLine($"     MĂSURAT (IMO-V56/{eticheta}): fără regulă {faraRegula}, plafon {cuPlafon}, "
                + $"exclusiv {laExclusiv}, altă categorie {altaCategorie}, înainte de `DeLa` {inainteDeLa}, "
                + $"2025 {deLa2025}, 2027 {deLa2027}, după `PanaLa` {dupaPanaLa}, plafon+procent "
                + $"{plafonApoiProcent}.");
            s.Check($"IMO-V56 ({eticheta}) mecanismul deductibilității e complet declarativ: fără regulă "
                + "deductibilul E fiscalul; plafonul taie, procentul scade; `DoarNeexclusiv` se sare la utilizare "
                + "exclusivă; o regulă a altei categorii nu se aplică; `DeLa` maxim ≤ dată câștigă per "
                + "(categorie, fel); `PanaLa` expirat nu se mai aplică; plafonul se aplică ÎNAINTEA procentului",
                faraRegula == 2000m && cuPlafon == 1500m && laExclusiv == 2000m && altaCategorie == 2000m
                && inainteDeLa == 2000m && deLa2025 == 0m && deLa2027 == 500m && dupaPanaLa == 2000m
                && plafonApoiProcent == 750m);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var reguli = os.GetObjectsQuery<RegulaDeductibilitate>()
                .Select(r => new { r.Categorie, r.DoarNeexclusiv, r.Fel, r.Valoare, r.DeLa, r.PanaLa })
                .ToList()
                .Select(r => new RegulaSnapshot(r.Categorie, r.DoarNeexclusiv, r.Fel, r.Valoare, r.DeLa, r.PanaLa))
                .ToList();
            var sediu = CategorieFiscala.SediuSocialInLocuinta;
            var in2025 = AmortizareService.Deductibil(1000m, sediu, false, reguli, new DateOnly(2025, 6, 30));
            var in2027 = AmortizareService.Deductibil(1000m, sediu, false, reguli, new DateOnly(2027, 6, 30));
            Console.WriteLine($"     MĂSURAT (IMO-V57/{eticheta}): {reguli.Count} reguli reale; sediu social în "
                + $"locuință, neexclusiv — 2025: {in2025}, 2027: {in2027} (din 1.000 fiscal).");
            s.Check($"IMO-V57 ({eticheta}) regulile REALE ale profilului, citite din bază, se aplică pe dată: pe "
                + "privat amortizarea sediului social în locuință e nedeductibilă în 2025 (Legea 296/2023) și "
                + "deductibilă 50 % din 2026; pe bugetar, unde nu există impozit pe profit și deci nicio regulă, "
                + "deductibilul E fiscalul — aceeași funcție, două profiluri",
                privat ? in2025 == 0m && in2027 == 500m : in2025 == 1000m && in2027 == 1000m);
        }

        // Cheia regulii = selecția motorului: (categorie, fel, DeLa), pe calea reală a commit-ului.
        {
            Atlas.Conta.BackOffice.Module.BusinessObjects.MesajeConstraintRo.Aplica();
            var deLaCheie = new DateOnly(2099, 1, 1);
            string ComiteRegula(params (FelDeductibilitate Fel, decimal Valoare)[] reguli) {
                using var os = s.Provider.CreateObjectSpace();
                new GardianEditare().OnObjectSpaceCreated(os);
                foreach (var (fel, valoare) in reguli) {
                    var regula = os.CreateObject<RegulaDeductibilitate>();
                    regula.Categorie = CategorieFiscala.VehiculPersoaneMax9Locuri;
                    regula.DoarNeexclusiv = true;
                    regula.Fel = fel;
                    regula.Valoare = valoare;
                    regula.DeLa = deLaCheie;
                    regula.Temei = Marcaj + " — cheia regulii";
                }
                try {
                    os.CommitChanges();
                    return null;
                }
                catch (Exception e) {
                    var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(e);
                    return violare != null
                        ? Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)
                        : e.GetBaseException().Message;
                }
            }
            var douaFeluri = ComiteRegula((FelDeductibilitate.PlafonLunar, 1500m), (FelDeductibilitate.Procent, 50m));
            var acelasiFel = ComiteRegula((FelDeductibilitate.Procent, 60m));
            int reguliCheie;
            using (var os = s.Provider.CreateObjectSpace())
                reguliCheie = os.GetObjectsQuery<RegulaDeductibilitate>().Count(r => r.DeLa == deLaCheie);
            Console.WriteLine($"     MĂSURAT (IMO-V58/{eticheta}): plafon + procent pe aceeași categorie și dată → "
                + $"„{douaFeluri ?? "acceptat"}”; al doilea procent → „{acelasiFel ?? "<A TRECUT>"}”; "
                + $"{reguliCheie} rânduri pe dată.");
            s.Check($"IMO-V58 ({eticheta}) unicitatea regulii de deductibilitate e cheia selecției motorului "
                + "(categorie, fel, `DeLa`, 087g): un plafon și un procent din aceeași zi pe aceeași categorie sunt "
                + "acceptate, al doilea rând pe aceeași categorie, fel și dată e refuzat de bază cu mesaj de domeniu",
                douaFeluri == null && acelasiFel != null && acelasiFel.Contains("Există deja") && reguliCheie == 2);
        }

        // ── Curățenia finală ──────────────────────────────────────────────────────
        using (var os = s.Provider.CreateObjectSpace()) {
            CurataImo(os);
            var primaZi = Zi(5, 1);
            var ultimaZi = Zi(12, 31);
            s.Check($"IMO — curățenie finală ({eticheta}): nicio fișă, niciun rând de registru și nicio perioadă "
                + "2027/5–12 rămasă din scenă",
                !os.GetObjectsQuery<Imobilizare>().Any(f => f.NumarInventar.StartsWith(Marcaj))
                && !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An && p.Luna >= 5)
                && !os.GetObjectsQuery<Document>().Any(d => d.Data >= primaZi && d.Data <= ultimaZi));
        }
    }
}
