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
// F24-G1…G3 — gardianul, după review-ul advers al feliei 24
// ---------------------------------------------------------------------------
// Trei găuri măsurate pe cod, toate de felul „un gard care tace devine capcană"
// (62f): enum-urile fără membru 0, care se scriau tăcut pe ușa OData (`Latura`
// = 0 e o A TREIA latură pentru potrivirea de stoc, `Directie` = 0 face o
// achiziție să colecteze TVA); ȘTERGEREA unui `TipTva` referit, care trecea deși
// dezactivarea aceluiași rând e refuzată; și explicația care anunța o postare pe
// care profilul de validare o interzice.
static class VerificaF24Gardian {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        var sarite = new List<string>();

        string Refuza(Action<IObjectSpace> pregateste) {
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

        Guid idTipDoc, idCont, idRandOperatiuni;
        using (var os = s.Provider.CreateObjectSpace()) {
            idTipDoc = os.GetObjectsQuery<TipDocument>().Select(t => t.ID).First();
            idCont = os.GetObjectsQuery<Cont>().Select(c => c.ID).First();
            idRandOperatiuni = os.GetObjectsQuery<RandD300>()
                .Where(r => r.Fel == FelRandD300.Operatiuni).Select(r => r.ID).First();
        }

        // ---- F24-G1: enum fără membru definit, pe orice `ICuProvenienta` ----
        var stocFaraLatura = Refuza(os => {
            var r = os.CreateObject<RegulaStoc>();
            r.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            r.TipStoc = TipStoc.Magazie;
            r.Semn = 1;
        });
        var tvaFaraDirectie = Refuza(os => {
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            p.SursaContrapartida = SursaCont.Explicit;
            p.ContrapartidaFallback = os.GetObjectByKey<Cont>(idCont);
        });
        var d300FaraSens = Refuza(os => {
            var m = os.CreateObject<MapareD300>();
            m.Rand = os.GetObjectByKey<RandD300>(idRandOperatiuni);
        });
        var randValid = Refuza(os => {
            var r = os.CreateObject<RegulaStoc>();
            r.TipDocument = os.GetObjectByKey<TipDocument>(idTipDoc);
            r.Latura = LaturaDocument.Primitor;
            r.TipStoc = TipStoc.Magazie;
            r.Semn = 1;
        });
        Console.WriteLine($"     MĂSURAT (F24-G1/{eticheta}): `RegulaStoc` fără latură → "
            + $"„{stocFaraLatura ?? "<NU A ARUNCAT>"}”; `PoliticaTva` fără direcție → "
            + $"„{tvaFaraDirectie ?? "<NU A ARUNCAT>"}”; `MapareD300` fără sens → "
            + $"„{d300FaraSens ?? "<NU A ARUNCAT>"}”; rând complet → „{randValid ?? "acceptat"}”.");
        s.Check($"F24-G1 ({eticheta}) enum-ul fără membru 0 nu se mai scrie TĂCUT: gardianul refuză generic, prin "
            + "reflecție pe orice `ICuProvenienta`, orice proprietate de enum a cărei valoare nu e definită — "
            + "`RegulaStoc.Latura` = 0 ar fi fost o a treia latură în potrivirea de stoc, `PoliticaTva.Directie` "
            + "= 0 ar fi colectat TVA pe o achiziție, `MapareD300.Sens` = 0 n-ar fi potrivit niciodată. Regula e "
            + "a FORMEI, nu a unui tip; `[Flags]` rămâne în afara, iar rândul complet trece",
            stocFaraLatura != null && stocFaraLatura.Contains("valoare validă")
            && stocFaraLatura.Contains(nameof(RegulaStoc.Latura))
            && tvaFaraDirectie != null && tvaFaraDirectie.Contains("valoare validă")
            && tvaFaraDirectie.Contains(nameof(PoliticaTva.Directie))
            && d300FaraSens != null && d300FaraSens.Contains("valoare validă")
            && d300FaraSens.Contains(nameof(MapareD300.Sens))
            && randValid == null);

        // ---- F24-G2: ștergerea unui `TipTva` REFERIT ----
        string codReferit = null, refuzStergere = null, codLiber = null, stergereLibera = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            var referit = os.GetObjectsQuery<TipDocument>().Where(t => t.TipTvaImplicitId != null)
                .Select(t => t.TipTvaImplicitId.Value).FirstOrDefault();
            if (referit == Guid.Empty)
                sarite.Add("ștergerea unui `TipTva` referit (niciun tip de document n-are ancoră de TVA)");
            else {
                codReferit = os.GetObjectByKey<TipTva>(referit).Cod;
                refuzStergere = Refuza(o => o.Delete(o.GetObjectByKey<TipTva>(referit)));
            }
            var ancore = os.GetObjectsQuery<TipDocument>().Where(t => t.TipTvaImplicitId != null)
                .Select(t => t.TipTvaImplicitId.Value).ToList();
            var implicite = os.GetObjectsQuery<PoliticaTvaImplicit>().Select(p => p.TipTvaId).ToList();
            var peLinii = os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.TipTvaId != null)
                .Select(d => d.TipTvaId.Value).Distinct().ToList();
            var peJurnal = Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Postari(os)
                .Select(p => p.TipTvaId.Value).Distinct().ToList();
            var peParteneri = os.GetObjectsQuery<Partener>().Where(p => p.TipTvaImplicitId != null)
                .Select(p => p.TipTvaImplicitId.Value).ToList();
            var peProduse = os.GetObjectsQuery<Produs>().Where(p => p.TipTvaImplicitId != null)
                .Select(p => p.TipTvaImplicitId.Value).ToList();
            var ocupate = ancore.Concat(implicite).Concat(peLinii).Concat(peJurnal)
                .Concat(peParteneri).Concat(peProduse).ToHashSet();
            var liber = os.GetObjectsQuery<TipTva>().ToList().FirstOrDefault(t => !ocupate.Contains(t.ID));
            if (liber == null)
                sarite.Add("ștergerea unui `TipTva` LIBER (profilul n-are niciun tip nereferit)");
            else {
                codLiber = liber.Cod;
                var idLiber = liber.ID;
                stergereLibera = Refuza(o => o.Delete(o.GetObjectByKey<TipTva>(idLiber)));
            }
        }
        Console.WriteLine($"     MĂSURAT (F24-G2/{eticheta}): ștergerea lui {codReferit ?? "<sărit>"} (referit) → "
            + $"„{refuzStergere ?? "<NU A ARUNCAT>"}”; ștergerea lui {codLiber ?? "<sărit>"} (liber) → "
            + $"„{stergereLibera ?? "acceptată"}”.");
        s.Check($"F24-G2 ({eticheta}) ȘTERGEREA unui `TipTva` referit se refuză ca DEZACTIVAREA lui: alternativa "
            + "corectă era deja păzită, iar cea distructivă trecea — un click ar fi lăsat implicitele fără țintă "
            + "și ar fi oprit updater-ul următor pe „lipsește din bază tipul de TVA”. Tipul NEREFERIT rămâne "
            + "ștergibil: gardul e pe referințe, nu pe nomenclator"
            + (sarite.Count > 0 ? $" — sărite: {string.Join("; ", sarite)}" : ""),
            (codReferit == null
                || (refuzStergere != null && refuzStergere.Contains("șterge") && refuzStergere.Contains("referit")))
            && (codLiber == null || stergereLibera == null));

        // ---- F24-G3: explicația declară ce ar refuza profilul de validare ----
        using (var os = s.Provider.CreateObjectSpace()) {
            var interdictie = os.GetObjectsQuery<PoliticaValidare>().Where(p => p.NaturaInterzisa != null)
                .Select(p => new { p.TipDocumentId, p.NaturaInterzisa, Cod = p.TipDocument.Cod })
                .FirstOrDefault();
            if (interdictie == null) {
                Console.WriteLine($"     MĂSURAT (F24-G3/{eticheta}): profilul n-are nicio `PoliticaValidare` cu "
                    + "natură interzisă — rezerva nu se poate măsura aici (e a celuilalt profil).");
            }
            else {
                var natura = interdictie.NaturaInterzisa.Value;
                var tipMaterial = os.GetObjectsQuery<TipMaterial>().Where(t => t.Clasa.Natura == natura)
                    .Select(t => new { t.ID, t.Cod }).First();
                var explicatie = ExplicaApply.Explica(os, new ExplicaCerere(interdictie.TipDocumentId,
                    tipMaterial.ID, +1, new DateOnly(2026, 5, 5), null, null, null, null));
                var rezerve = explicatie.Contare.Rezerve ?? [];
                Console.WriteLine($"     MĂSURAT (F24-G3/{eticheta}): {interdictie.Cod} × {tipMaterial.Cod} "
                    + $"(natura {natura} INTERZISĂ) ⇒ {rezerve.Length} rezervă(e) — "
                    + $"„{explicatie.Contare.Concluzie}”");
                s.Check($"F24-G3 ({eticheta}) „Explică” nu anunță o postare pe care motorul o REFUZĂ: pe un tip al "
                    + "cărui profil de validare interzice natura liniei, concluzia blocului de contare nu mai "
                    + "spune „se postează”, ci că regula câștigătoare AR posta, dar operarea ar fi refuzată — "
                    + "rezerva vine din același rând `PoliticaValidare` pe care motorul îl citește (33c)",
                    rezerve.Any(r => r.Contains("Profilul de validare interzice"))
                    && !explicatie.Contare.Concluzie.Contains("Se postează")
                    && explicatie.Contare.Concluzie.Contains("refuzată"));
            }
        }
    }
}
