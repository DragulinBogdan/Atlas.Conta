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

// ---------------------------------------------------------------------------
// F23-V4 — GARDIANUL: tabelul F23-D5, rând cu rând, pe ușa COMUNĂ
// ---------------------------------------------------------------------------
// De ce aici și nu ca reguli XAF: de la felia 23 politicile se editează prin
// OData, iar validarea XAF nu rulează pe API (55b). `GardianEditare.Verifica` e
// `public static` și se cheamă direct pe un ObjectSpace fără securitate — exact
// „calea standalone” din antetul gardianului.
//
// Fiecare probă rulează pe ObjectSpace-ul EI, cu `Rollback` la final: nimic nu se
// comite, deci nu e nevoie de curățenie, iar proba NEGATIVĂ („rândul valid
// trece”) poate cere mesaj null fără să înghită erorile altei probe.
static class VerificaF23Gardian {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        var sarite = new List<string>();

        string RefuzF23(Action<IObjectSpace> pregateste) {
            using var osG = s.Provider.CreateObjectSpace();
            try {
                pregateste(osG);
                GardianEditare.Verifica(osG);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
            finally {
                osG.Rollback();
            }
        }

        // ---- Proveniența (F23-D4), regula INTERFEȚEI ----
        var provNou = RefuzF23(os => {
            var p = os.CreateObject<PoliticaScadenta>();
            p.TipDocument = os.GetObjectsQuery<TipDocument>().First();
            p.ZileDefault = 30;
            p.DinSeed = true;
        });
        bool timbruStins = false, aAruncat = false, areScadenta = false;
        var zileInainte = 0;
        using (var osProv = s.Provider.CreateObjectSpace()) {
            var rand = osProv.GetObjectsQuery<PoliticaScadenta>().ToList().FirstOrDefault(p => p.DinSeed);
            if (rand == null)
                sarite.Add("proveniența pe rând EXISTENT (profilul n-are nicio `PoliticaScadenta` seed-uită)");
            else {
                areScadenta = true;
                zileInainte = rand.ZileDefault;
                rand.ZileDefault = zileInainte + 1;
                try { GardianEditare.Verifica(osProv); } catch (OperareException) { aAruncat = true; }
                timbruStins = !rand.DinSeed;
            }
            osProv.Rollback();
        }
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/proveniență): rând NOU cu `DinSeed = true` → "
            + $"„{provNou ?? "<NU A ARUNCAT>"}”; rând seed-uit editat ({zileInainte} → {zileInainte + 1} zile) → "
            + $"{(aAruncat ? "a ARUNCAT" : "acceptat")}, `DinSeed` devine {(timbruStins ? "false" : "TOT true")}.");
        s.Check($"F23-V4 ({eticheta}) proveniența e un câmp SERVER-OWNED INVERSAT: pe un rând NOU „Din seed” cules "
            + "de client se REFUZĂ (și-ar fabrica proveniența, arătând în raportul de profil ca livrat), iar pe "
            + "un rând EXISTENT editarea nu se refuză — se TIMBREAZĂ, gardianul stingând flag-ul. Regula e a "
            + "INTERFEȚEI `ICuProvenienta`, chemată înaintea switch-ului pe tip",
            provNou != null && provNou.Contains("proveniența") && provNou.Contains("Din seed")
            && (!areScadenta || (!aAruncat && timbruStins)));

        // ---- `TipDocument` = ancora claselor (decizia 20) ----
        var tipDocNou = RefuzF23(os => {
            var t = os.CreateObject<TipDocument>();
            t.Cod = "ZZZ"; t.Denumire = "Tip inventat"; t.ClrType = "Nimic";
        });
        var tipDocCod = RefuzF23(os => {
            var t = os.GetObjectsQuery<TipDocument>().ToList().First(x => x.Cod == "FCT");
            t.Cod = "FCT2";
        });
        var tipDocDenumire = RefuzF23(os => {
            var t = os.GetObjectsQuery<TipDocument>().ToList().First(x => x.Cod == "FCT");
            t.Denumire = (t.Denumire ?? "") + " (editat)";
        });
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/TipDocument): rând nou → "
            + $"„{tipDocNou ?? "<NU A ARUNCAT>"}”; `Cod` schimbat → „{tipDocCod ?? "<NU A ARUNCAT>"}”; "
            + $"`Denumire` schimbată → „{tipDocDenumire ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) `TipDocument` rămâne ANCORA: un rând NOU se refuză (n-ar avea clasă în "
            + "spate), `Cod` schimbat pe un rând existent se refuză (politicile îl referă prin el, iar schimbarea "
            + "l-ar rupe tăcut) — dar `Denumire` rămâne editabilă, fiindcă ea chiar e politică",
            tipDocNou != null && tipDocNou.Contains("nu se creează")
            && tipDocCod != null && tipDocCod.Contains("identitatea")
            && tipDocDenumire == null);

        // ---- `TipTva`: cota e procent; dezactivarea unui implicit e refuzată ----
        var tvaCota = RefuzF23(os => os.GetObjectsQuery<TipTva>().ToList().First().Cota = 150m);
        var tvaCotaBuna = RefuzF23(os => os.GetObjectsQuery<TipTva>().ToList().First().Cota = 12m);
        string codAncora = null, tvaDezactivare = null, tvaDezactivareLibera = null;
        using (var osT = s.Provider.CreateObjectSpace()) {
            var idAncora = osT.GetObjectsQuery<TipDocument>().Where(t => t.Cod == "FCT")
                .Select(t => t.TipTvaImplicitId).FirstOrDefault();
            codAncora = osT.GetObjectsQuery<TipTva>().Where(t => t.ID == idAncora).Select(t => t.Cod)
                .FirstOrDefault();
            // Un tip ACTIV care nu e implicit NICĂIERI: dezactivarea lui e legitimă,
            // deci e proba negativă. Setul de referințe e cel din gardian.
            var referite = new HashSet<Guid>(
                osT.GetObjectsQuery<TipDocument>().Where(t => t.TipTvaImplicitId != null)
                    .Select(t => t.TipTvaImplicitId.Value).ToList()
                .Concat(osT.GetObjectsQuery<PoliticaTvaImplicit>().Select(p => p.TipTvaId).ToList())
                .Concat(osT.GetObjectsQuery<Partener>().Where(p => p.TipTvaImplicitId != null)
                    .Select(p => p.TipTvaImplicitId.Value).ToList())
                .Concat(osT.GetObjectsQuery<Produs>().Where(p => p.TipTvaImplicitId != null)
                    .Select(p => p.TipTvaImplicitId.Value).ToList()));
            var liber = osT.GetObjectsQuery<TipTva>().Where(t => t.Activ).Select(t => t.ID).ToList()
                .FirstOrDefault(id => !referite.Contains(id));
            if (idAncora is Guid ancora && ancora != Guid.Empty)
                tvaDezactivare = RefuzF23(os => os.GetObjectByKey<TipTva>(ancora).Activ = false);
            else
                sarite.Add("dezactivarea unui tip ANCORAT (FCT n-are `TipTvaImplicit`)");
            if (liber != Guid.Empty)
                tvaDezactivareLibera = RefuzF23(os => os.GetObjectByKey<TipTva>(liber).Activ = false);
            else
                sarite.Add("dezactivarea unui tip NEREFERIT (toate tipurile active sunt implicite undeva)");
        }
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/TipTva): `Cota = 150` → „{tvaCota ?? "<NU A ARUNCAT>"}”; "
            + $"`Cota = 12` → „{tvaCotaBuna ?? "acceptat"}”; dezactivarea ancorei ({codAncora}) → "
            + $"„{tvaDezactivare ?? "<NESONDAT>"}”; dezactivarea unui tip nereferit → "
            + $"„{tvaDezactivareLibera ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) `TipTva`: cota e un procent (150 se refuză, 12 trece), iar dezactivarea unui "
            + "tip REFERIT ca implicit se refuză CU LISTA referințelor — un implicit care țintește un inactiv ar "
            + "fi sărit tăcut la culegere (62f: un gard care tace devine capcană). Un tip nereferit rămâne liber "
            + "să fie stins",
            tvaCota != null && tvaCota.Contains("150") && tvaCotaBuna == null
            && (tvaDezactivare == null
                || (tvaDezactivare.Contains("ancora") && tvaDezactivare.Contains("FCT")))
            && tvaDezactivareLibera == null);

        // ---- `PoliticaTvaImplicit`: ținta activă + MESAJUL unicității ----
        string implInactiv = null, implBun = null, implDublu = null;
        using (var osI = s.Provider.CreateObjectSpace()) {
            var idInactiv = osI.GetObjectsQuery<TipTva>().Where(t => !t.Activ).Select(t => t.ID)
                .FirstOrDefault();
            var idActiv = osI.GetObjectsQuery<TipTva>().Where(t => t.Activ).Select(t => t.ID).FirstOrDefault();
            var idBtr = osI.GetObjectsQuery<TipDocument>().Where(t => t.Cod == "BTR").Select(t => t.ID)
                .FirstOrDefault();
            if (idInactiv != Guid.Empty)
                implInactiv = RefuzF23(os => {
                    var p = os.CreateObject<PoliticaTvaImplicit>();
                    p.TipDocument = os.GetObjectByKey<TipDocument>(idBtr);
                    p.ClasaFiscala = ClasaFiscalaPartener.InregistratRo;
                    p.TipTva = os.GetObjectByKey<TipTva>(idInactiv);
                });
            else
                sarite.Add("implicit spre tip INACTIV (profilul n-are niciun `TipTva` stins)");
            implBun = RefuzF23(os => {
                var p = os.CreateObject<PoliticaTvaImplicit>();
                p.TipDocument = os.GetObjectByKey<TipDocument>(idBtr);
                p.ClasaFiscala = ClasaFiscalaPartener.InregistratRo;
                p.TipTva = os.GetObjectByKey<TipTva>(idActiv);
            });
        }
        if (privat)
            implDublu = RefuzF23(os => {
                var p = os.CreateObject<PoliticaTvaImplicit>();
                p.TipDocument = os.GetObjectsQuery<TipDocument>().ToList().First(t => t.Cod == "FCL");
                p.ClasaFiscala = ClasaFiscalaPartener.Ue;
                p.TipTva = os.GetObjectsQuery<TipTva>().ToList().First(t => t.Cod == "SDD");
            });
        else
            sarite.Add("al doilea rând pe aceeași cheie (bugetarul n-are rânduri de implicit)");
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/PoliticaTvaImplicit): spre tip inactiv → "
            + $"„{implInactiv ?? "<NESONDAT>"}”; cheie liberă spre tip activ → „{implBun ?? "acceptat"}”; al "
            + $"doilea rând pe FCL×Ue → „{implDublu ?? "<NESONDAT>"}”.");
        s.Check($"F23-V4 ({eticheta}) `PoliticaTvaImplicit`: un implicit nu poate ținti un tip INACTIV (gardul "
            + "geamăn al celui de mai sus, pe cealaltă direcție — altfel ar fi ocolibil în doi pași), iar un al "
            + "doilea rând pe aceeași cheie primește MESAJUL gardianului înaintea lui `23505` brut (indexul "
            + "rămâne plasa, 60a); o cheie liberă spre un tip activ trece",
            (implInactiv == null || implInactiv.Contains("inactiv"))
            && implBun == null
            && (implDublu == null || (implDublu.Contains("deja") && implDublu.Contains("FCL"))));

        // ---- cheia rândului ȘTERS se reface (81, 104f) ----
        string stersDublu = null; var stersRefacut = false; var stersRamas = 0; Guid idCheie;
        using (var osS = s.Provider.CreateObjectSpace()) {
            idCheie = osS.GetObjectsQuery<TipDocument>().Where(t => t.Cod == "BTR").Select(t => t.ID).First();
            var idActiv = osS.GetObjectsQuery<TipTva>().Where(t => t.Activ).Select(t => t.ID).First();
            var valabil = new DateOnly(2026, 1, 15);
            PoliticaTvaImplicit Rand(IObjectSpace os) {
                var p = os.CreateObject<PoliticaTvaImplicit>();
                p.TipDocument = os.GetObjectByKey<TipDocument>(idCheie);
                p.ClasaFiscala = ClasaFiscalaPartener.NeinregistratRo;
                p.ValabilDeLa = valabil;
                p.TipTva = os.GetObjectByKey<TipTva>(idActiv);
                return p;
            }
            var primul = Rand(osS);
            osS.CommitChanges();
            osS.Delete(primul);
            osS.CommitChanges();
            using (var os2 = s.Provider.CreateObjectSpace()) {
                Rand(os2);
                try { GardianEditare.Verifica(os2); os2.CommitChanges(); stersRefacut = true; }
                catch (Exception e) { stersDublu = e.Message; }
            }
            stersRamas = osS.GetObjectsQuery<PoliticaTvaImplicit>()
                .Count(p => p.TipDocumentId == idCheie && p.ClasaFiscala == ClasaFiscalaPartener.NeinregistratRo
                    && p.ValabilDeLa == valabil);
            new Purja(osS).Adauga(osS.GetObjectsQuery<PoliticaTvaImplicit>()
                .Where(p => p.TipDocumentId == idCheie && p.ClasaFiscala == ClasaFiscalaPartener.NeinregistratRo)
                .ToList()).Executa();
        }
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/șters): a doua politică pe cheia rândului șters → "
            + $"„{stersDublu ?? "acceptată și comisă"}”; rânduri pe cheie = {stersRamas}.");
        s.Check($"F23-V4 ({eticheta}) un rând de politică ȘTERS nu e dublu: ștergerea e fizică, cheia se reface",
            stersDublu == null && stersRefacut && stersRamas == 1);

        // ---- `PoliticaTva` / `RegulaContare` / `RegulaStoc` ----
        Guid idCont, idTipDoc, idTipMaterial;
        using (var osR = s.Provider.CreateObjectSpace()) {
            idCont = osR.GetObjectsQuery<Cont>().Select(c => c.ID).First();
            idTipDoc = osR.GetObjectsQuery<TipDocument>().Select(t => t.ID).First();
            idTipMaterial = osR.GetObjectsQuery<TipMaterial>().Select(t => t.ID).First();
        }
        // `Directie`/`Latura`/`TipStoc` se culeg EXPLICIT de la felia 24: enum-urile
        // fără membru 0 sunt refuzate generic (F24-G1), deci un rând „valid" care le
        // lăsa pe default nu mai măsura regula pe care o probează.
        var ptvaExplicit = RefuzF23(os => {
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.Directie = DirectieTva.Deductibil;
            p.SursaContrapartida = SursaCont.Explicit;
        });
        var ptvaBun = RefuzF23(os => {
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.Directie = DirectieTva.Deductibil;
            p.SursaContrapartida = SursaCont.Explicit;
            p.ContrapartidaFallback = os.GetObjectByKey<Cont>(idCont);
        });
        RegulaContare RcNoua(IObjectSpace os) {
            var r = os.CreateObject<RegulaContare>();
            r.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            // `SursaCont.Explicit` e valoarea 0, deci default-ul unui rând nou: probele
            // NEGATIVE trebuie s-o schimbe, altfel ar măsura chiar refuzul de mai jos.
            r.SursaContDebit = SursaCont.TipMaterial;
            r.SursaContCredit = SursaCont.TipMaterial;
            return r;
        }
        var rcAmbele = RefuzF23(os => {
            var r = RcNoua(os);
            r.TipMaterial = os.GetObjectByKey<TipMaterial>(idTipMaterial);
            r.NaturaFiltru = NaturaClasa.Serviciu;
        });
        var rcSemn = RefuzF23(os => RcNoua(os).SemnFiltru = 2);
        var rcExplicit = RefuzF23(os => RcNoua(os).SursaContDebit = SursaCont.Explicit);
        var rcBuna = RefuzF23(os => {
            var r = RcNoua(os);
            r.TipDocument = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "BCS");
            r.SemnFiltru = -1;
        });
        RegulaStoc RsNoua(IObjectSpace os) {
            var r = os.CreateObject<RegulaStoc>();
            r.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            r.Latura = LaturaDocument.Predator;
            r.TipStoc = TipStoc.Magazie;
            return r;
        }
        var rsSemn = RefuzF23(os => RsNoua(os).Semn = 0);
        var rsBuna = RefuzF23(os => RsNoua(os).Semn = 1);
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/reguli): `PoliticaTva` Explicit fără cont → "
            + $"„{ptvaExplicit ?? "<NU A ARUNCAT>"}”, cu cont → „{ptvaBun ?? "acceptat"}”; `RegulaContare` "
            + $"tip+natură → „{rcAmbele ?? "<NU A ARUNCAT>"}”, SemnFiltru=2 → „{rcSemn ?? "<NU A ARUNCAT>"}”, "
            + $"debit Explicit fără cont → „{rcExplicit ?? "<NU A ARUNCAT>"}”, regulă validă → "
            + $"„{rcBuna ?? "acceptat"}”; `RegulaStoc` Semn=0 → „{rsSemn ?? "<NU A ARUNCAT>"}”, Semn=+1 → "
            + $"„{rsBuna ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) invarianții de POTRIVIRE: sursa „Explicit” fără cont (pe `PoliticaTva` și pe "
            + "latura debitoare a `RegulaContare`) e o contrapartidă care nu se rezolvă niciodată; tipul de "
            + "material ȘI filtrul de natură pe aceeași regulă sunt trepte ALTERNATIVE (26c), nu „mai specific”; "
            + "`SemnFiltru = 2` e o regulă care nu s-ar potrivi NICIODATĂ, tăcut; `RegulaStoc.Semn` e direcția, "
            + "deci −1 sau +1, fără „orice semn” — rândurile valide trec",
            ptvaExplicit != null && ptvaExplicit.Contains("Explicit") && ptvaBun == null
            && rcAmbele != null && rcAmbele.Contains("ALTERNATIVE")
            && rcSemn != null && rcSemn.Contains("2")
            && rcExplicit != null && rcExplicit.Contains("debitor")
            && rcBuna == null
            && rsSemn != null && rsSemn.Contains("0") && rsBuna == null);

        // ---- `PoliticaNumerotare` (cu DEVIEREA declarată pe `Format`) / `PoliticaScadenta` ----
        PoliticaNumerotare NumNoua(IObjectSpace os) {
            var p = os.CreateObject<PoliticaNumerotare>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.Serie = "F23";
            p.UrmatorulNumar = 1;
            return p;
        }
        var numSerie = RefuzF23(os => NumNoua(os).Serie = "   ");
        var numContor = RefuzF23(os => NumNoua(os).UrmatorulNumar = 0);
        var numFormat = RefuzF23(os => NumNoua(os).Format = "{2}-{0}");
        var numFaraFormat = RefuzF23(os => NumNoua(os).Format = null);
        var scadNegativa = RefuzF23(os => {
            var p = os.CreateObject<PoliticaScadenta>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.ZileDefault = -1;
        });
        var scadBuna = RefuzF23(os => {
            var p = os.CreateObject<PoliticaScadenta>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.ZileDefault = 30;
        });
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/numerotare+scadență): `Serie` goală → "
            + $"„{numSerie ?? "<NU A ARUNCAT>"}”; `UrmatorulNumar = 0` → „{numContor ?? "<NU A ARUNCAT>"}”; "
            + $"`Format` = „{{2}}-{{0}}” → „{numFormat ?? "<NU A ARUNCAT>"}”; `Format` gol → "
            + $"„{numFaraFormat ?? "acceptat"}”; `ZileDefault = -1` → „{scadNegativa ?? "<NU A ARUNCAT>"}”; "
            + $"`ZileDefault = 30` → „{scadBuna ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) `PoliticaNumerotare` — cu DEVIEREA declarată de la F23-D5: `Serie` nevidă "
            + "(numărul se compune din ea), contor ≥ 1, și un `Format` CULES compunabil („{2}” aruncă la operare, "
            + "adică un document care nu se mai poate opera); dar `Format` GOL TRECE — e opțional în motor și "
            + "seed-ul nu-l scrie niciodată, deci regula literală ar fi refuzat rândurile propriului seed. "
            + "`PoliticaScadenta`: zile ≥ 0",
            numSerie != null && numSerie.Contains("serie")
            && numContor != null && numContor.Contains("de la 1")
            && numFormat != null && numFormat.Contains("compune")
            && numFaraFormat == null
            && scadNegativa != null && scadNegativa.Contains("-1") && scadBuna == null);

        // ---- `PoliticaInchidereTva`: cele patru conturi sunt un SET ----
        List<Guid> conturi;
        using (var osC = s.Provider.CreateObjectSpace())
            conturi = osC.GetObjectsQuery<Cont>().Select(c => c.ID).Take(4).ToList();
        string inchTrei = null, inchPatru = null, inchZero = null;
        if (conturi.Count == 4) {
            void Inchidere(IObjectSpace os, int cate) {
                var p = os.CreateObject<PoliticaInchidereTva>();
                p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
                if (cate > 0) p.ContDeductibila = os.GetObjectByKey<Cont>(conturi[0]);
                if (cate > 1) p.ContColectata = os.GetObjectByKey<Cont>(conturi[1]);
                if (cate > 2) p.ContDePlata = os.GetObjectByKey<Cont>(conturi[2]);
                if (cate > 3) p.ContDeRecuperat = os.GetObjectByKey<Cont>(conturi[3]);
            }
            inchTrei = RefuzF23(os => Inchidere(os, 3));
            inchPatru = RefuzF23(os => Inchidere(os, 4));
            inchZero = RefuzF23(os => Inchidere(os, 0));
        }
        else
            sarite.Add("politica de închidere TVA (baza n-are patru conturi)");
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/închidere TVA): 3 din 4 conturi → "
            + $"„{inchTrei ?? "<NESONDAT>"}”; 4 din 4 → „{inchPatru ?? "acceptat"}”; niciunul → "
            + $"„{inchZero ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) `PoliticaInchidereTva`: cele patru conturi sunt un SET — serviciul cere setul "
            + "COMPLET ca să genereze ceva (46c), deci o politică pe jumătate culeasă nu e „în lucru”, e un tip "
            + "inert care ARATĂ configurat; ori toate, ori niciunul",
            inchTrei != null && inchTrei.Contains("3 din 4") && inchPatru == null && inchZero == null);

        // ---- `MapareD300` / `MapareD394`: o regulă, două uși ----
        // Pe privat perechea (NED21 × Livrare) e NEMAPATĂ deliberat (regimul e
        // exclusiv de achiziție), deci proba negativă nu lovește gardul de dublă
        // numărare; pe bugetar niciun tip n-are mapări.
        var codTvaMapare = privat ? "NED21" : "CAP0";
        Guid idTvaMapare;
        using (var osM = s.Provider.CreateObjectSpace())
            idTvaMapare = osM.GetObjectsQuery<TipTva>().Where(t => t.Cod == codTvaMapare).Select(t => t.ID)
                .FirstOrDefault();
        string d300Total = null, d300Operatiuni = null;
        if (idTvaMapare != Guid.Empty) {
            void Mapare(IObjectSpace os, string codRand) {
                var m = os.CreateObject<MapareD300>();
                m.TipTva = os.GetObjectByKey<TipTva>(idTvaMapare);
                m.Sens = SensTva.Livrare;
                m.Rand = os.GetObjectsQuery<RandD300>().ToList().First(r => r.Cod == codRand);
            }
            d300Total = RefuzF23(os => Mapare(os, "19"));
            d300Operatiuni = RefuzF23(os => Mapare(os, "9"));
        }
        else
            sarite.Add($"maparea D300 (profilul n-are tipul {codTvaMapare})");
        var d394Gresit = RefuzF23(os => {
            var m = os.CreateObject<MapareD394>();
            m.TipTva = os.GetObjectsQuery<TipTva>().ToList().First();
            m.Sens = SensTva.Achizitie;
            m.Tip = TipOperatiuneD394.L;
        });
        var d394Bun = RefuzF23(os => {
            var m = os.CreateObject<MapareD394>();
            m.TipTva = os.GetObjectsQuery<TipTva>().ToList().First();
            m.Sens = SensTva.Livrare;
            m.Tip = TipOperatiuneD394.L;
        });
        Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/mapări): D300 spre rd. 19 (TOTAL) → "
            + $"„{d300Total ?? "<NESONDAT>"}”; spre rd. 9 (operațiuni) → „{d300Operatiuni ?? "acceptat"}”; D394 "
            + $"`L` pe achiziție → „{d394Gresit ?? "<NU A ARUNCAT>"}”; `L` pe livrare → „{d394Bun ?? "acceptat"}”.");
        s.Check($"F23-V4 ({eticheta}) mapările: aceleași reguli ca atributele XAF de pe clasă, chemate prin "
            + "funcțiile lor STATICE (`MapareD300.EsteDeOperatiuni`, `MapareD394.TintaPermisa`) — un corp, două "
            + "uși (77k). Un rând de TOTAL nu se alimentează din mapări (se calculează), iar `L` e permis doar pe "
            + "livrare",
            (d300Total == null || (d300Total.Contains("rd. 19") && d300Operatiuni == null))
            && d394Gresit != null && d394Gresit.Contains("Achizitie") && d394Bun == null);

        if (sarite.Count > 0)
            Console.WriteLine($"     MĂSURAT (F23-V4/{eticheta}/sărite): {sarite.Count} probe fără subiect pe "
                + $"acest profil — {string.Join("; ", sarite)}.");
    }
}
