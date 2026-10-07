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

// F24-E1…E7 — EXPLICAȚIA configurației, pe seed-ul PRIVAT
// ---------------------------------------------------------------------------
// `ExplicaApply` n-are algoritm propriu (F24-D6): întreabă `Potrivire` pe fapte
// fabricate din parametrii cererii, deci probele de aici sunt ale AMBALAJULUI și
// ale profilului — că o linie de stoc pe FCT „nu contează" și pleacă pe NIR, că
// semnul schimbă regula pe LDI, că NTC declară postarea explicită. Plus proba de
// CONSISTENȚĂ (42c): conturile explicației sunt ACELEAȘI cu cele pe care motorul
// chiar le postează în cub pe un document echivalent — o explicație
// care minte e mai rea decât niciuna.
//
// Doar pe privat: bugetarul n-are lanțul FCT→NIR și n-are viramentul, deci
// aceleași enunțuri ar fi acolo probe ale altui profil, nu ale rutei.
static class VerificaF24Explica {
    public static void Ruleaza(Suita s) {
        const string Marcaj = "E2E-F24X";
        var azi = new DateOnly(2026, 5, 5);
        using var os = s.Provider.CreateObjectSpace();

        void Curata(IObjectSpace osC) {
            var pj = new Purja(osC);
            var repIds = osC.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var docs = osC.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(osC.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(osC.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)));
            pj.Adauga(osC.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)));
            pj.Executa();
        }
        Curata(os);

        var fctTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCT");
        var ldiTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "LDI");
        var pltTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "PLT");
        var ntcTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "NTC");
        var decTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "DEC");
        var tipStoc = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
        var tipServiciu = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");

        // Furnizorul poartă ContImplicit 401: fără el, creditul regulii FCT/Serviciu
        // ar cădea pe contul EXPLICIT al regulii (tot 401, dar din altă sursă) —
        // proba ar spune adevărul pe simbol și ar rata sursa.
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-FURN";
        furnizor.Denumire = "Furnizor probă F24 (explică)";
        furnizor.ContImplicit = cont401;
        os.CommitChanges();

        ExplicatieDto Explica(TipDocument tip, TipMaterial material, int semn) =>
            ExplicaApply.Explica(os, new ExplicaCerere(tip.ID, material.ID, semn, azi,
                furnizor.ID, mag1.ID, null, null));

        // ── F24-E1: linia de stoc pe FCT nu contează, dar pleacă pe NIR ───────────
        var fctStoc = Explica(fctTip, tipStoc, +1);
        Console.WriteLine($"     MĂSURAT (F24-E1): FCT × {tipStoc.Cod} ⇒ nivel {fctStoc.Contare.Nivel}, "
            + $"{fctStoc.Contare.Candidati.Length} candidați, conex {fctStoc.Conex.Tinta}/trece="
            + $"{fctStoc.Conex.Trece} — „{fctStoc.Contare.Concluzie}”");
        s.Check("F24-E1 explicația spune ce spune motorul pe linia de STOC a facturii de intrare: nicio regulă de "
            + "contare (recepția contează pe NIR — 26a), deci `Castigator` null și concluzia „linia nu contează”, "
            + "iar blocul de conex arată că o linie ca aceasta TRECE filtrul de natură al politicii FCT → NIR",
            fctStoc.Contare.Castigator == null
            && fctStoc.Contare.Nivel == nameof(NivelContare.Niciuna)
            && fctStoc.Contare.ContDebit == null && fctStoc.Contare.ContCredit == null
            && fctStoc.Contare.Concluzie.Contains("nu contează")
            && fctStoc.Conex.Tinta == "NIR" && fctStoc.Conex.Trece
            && fctStoc.Conex.NaturaFiltru == nameof(NaturaClasa.Stoc)
            && fctStoc.Natura == nameof(NaturaClasa.Stoc)
            && fctStoc.Stoc.Length == 0);

        // ── F24-E2: linia de serviciu — regula pe natură, cu sursele conturilor ───
        var fctServiciu = Explica(fctTip, tipServiciu, +1);
        Console.WriteLine($"     MĂSURAT (F24-E2): FCT × {tipServiciu.Cod} ⇒ nivel {fctServiciu.Contare.Nivel}, "
            + $"debit {fctServiciu.Contare.ContDebit.Simbol}/{fctServiciu.Contare.ContDebit.Sursa}, credit "
            + $"{fctServiciu.Contare.ContCredit.Simbol}/{fctServiciu.Contare.ContCredit.Sursa}, conex trece="
            + $"{fctServiciu.Conex.Trece}; TVA: „{fctServiciu.Tva.Concluzie}”");
        s.Check("F24-E2 pe linia de SERVICIU a aceleiași facturi câștigă regula pe `NaturaFiltru`, iar conturile ies "
            + "cu SURSA fiecăruia: debitul din contul implicit al Tipului (628), creditul din contul implicit al "
            + "repartitorului PREDATOR (401 de pe furnizor, nu fallback-ul explicit al regulii); aceeași linie NU "
            + "trece filtrul conexului, deci nu naște NIR",
            fctServiciu.Contare.Nivel == nameof(NivelContare.Natura)
            && fctServiciu.Contare.Castigator.NaturaFiltru == nameof(NaturaClasa.Serviciu)
            && fctServiciu.Contare.ContDebit.Simbol == "628"
            && fctServiciu.Contare.ContDebit.Sursa == nameof(SursaRezolvata.TipMaterial)
            && fctServiciu.Contare.ContCredit.Simbol == "401"
            && fctServiciu.Contare.ContCredit.Sursa == nameof(SursaRezolvata.RepartitorPredator)
            && fctServiciu.Contare.Concluzie.Contains("628 = 401")
            && !fctServiciu.Conex.Trece
            && fctServiciu.Tva.Directie == nameof(DirectieTva.Deductibil));

        // ── F24-E3: pe LDI, SEMNUL schimbă regula ────────────────────────────────
        var ldiPlus = Explica(ldiTip, tipStoc, +1);
        var ldiMinus = Explica(ldiTip, tipStoc, -1);
        var plusInCandidatiiMinusului = ldiMinus.Contare.Candidati
            .FirstOrDefault(k => k.Regula.Id == ldiPlus.Contare.Castigator.Id);
        Console.WriteLine($"     MĂSURAT (F24-E3): LDI × {tipStoc.Cod} ⇒ +1: nivel {ldiPlus.Contare.Nivel} "
            + $"({ldiPlus.Contare.ContDebit.Simbol} = {ldiPlus.Contare.ContCredit.Simbol}); −1: nivel "
            + $"{ldiMinus.Contare.Nivel} ({ldiMinus.Contare.ContDebit.Simbol} = "
            + $"{ldiMinus.Contare.ContCredit.Simbol}); regula de plus e eliminată pe −1 cu motivul "
            + $"{plusInCandidatiiMinusului?.Motiv}; stoc: {ldiPlus.Stoc.Length} latură(i)");
        s.Check("F24-E3 `SemnFiltru` e o axă a potrivirii, nu o notă de subsol: pe lista de inventar aceeași pereche "
            + "(tip document × Tip) dă DOUĂ reguli diferite după semnul liniei — plusul cade pe regula de natură "
            + "(intrare contra 7588), minusul pe regula EXACTĂ derivată 6xx = 3xx —, iar regula de plus apare în "
            + "candidații minusului cu motivul `SemnNepotrivit`",
            ldiPlus.Contare.Castigator != null && ldiMinus.Contare.Castigator != null
            && ldiPlus.Contare.Castigator.Id != ldiMinus.Contare.Castigator.Id
            && ldiPlus.Contare.Nivel == nameof(NivelContare.Natura)
            && ldiPlus.Contare.Castigator.SemnFiltru == +1
            && ldiMinus.Contare.Nivel == nameof(NivelContare.TipMaterialExact)
            && ldiMinus.Contare.Castigator.SemnFiltru == -1
            && plusInCandidatiiMinusului?.Motiv == nameof(MotivEliminare.SemnNepotrivit)
            && ldiPlus.Stoc.Length == 1
            && ldiPlus.Stoc[0].Latura == nameof(LaturaDocument.Predator)
            && ldiPlus.Stoc[0].Reguli.Length > 0);

        // ── F24-E4: viramentul — regula EXACTĂ pe Tipul tehnic ───────────────────
        var pltVir = Explica(pltTip, tipVir, +1);
        Console.WriteLine($"     MĂSURAT (F24-E4): PLT × VIR ⇒ nivel {pltVir.Contare.Nivel}, debit "
            + $"{pltVir.Contare.ContDebit.Simbol}/{pltVir.Contare.ContDebit.Sursa}; natura {pltVir.Natura}");
        s.Check("F24-E4 viramentul intern (64) se vede ca REGULĂ EXACTĂ pe Tipul tehnic `VIR`: exact nivelul pe care "
            + "gardul de trezorerie îl cere (`Generic`/`Niciuna` ⇒ refuz), iar debitul vine din contul de tranzit "
            + "al Tipului — niciun simbol în motor",
            pltVir.Contare.Nivel == nameof(NivelContare.TipMaterialExact)
            && pltVir.Natura == nameof(NaturaClasa.Virament)
            && pltVir.Contare.ContDebit.Sursa == nameof(SursaRezolvata.TipMaterial)
            && pltVir.Contare.ContDebit.Simbol == "581");

        // ── F24-E5: NTC declară postarea explicită, FCT nu ───────────────────────
        var ntc = Explica(ntcTip, tipServiciu, +1);
        Console.WriteLine($"     MĂSURAT (F24-E5): NTC ⇒ postare explicită {ntc.Contare.PostareExplicita}, "
            + $"nivel {ntc.Contare.Nivel} — „{ntc.Contare.Concluzie}”; FCT ⇒ "
            + $"{fctServiciu.Contare.PostareExplicita}");
        var dec = Explica(decTip, tipServiciu, +1);
        Console.WriteLine($"     MĂSURAT (F24-E5/DEC): postare explicită {dec.Contare.PostareExplicita}, nivel "
            + $"{dec.Contare.Nivel} — „{dec.Contare.Concluzie}”");
        s.Check("F24-E5 nota contabilă n-are nicio regulă de contare, dar tipul ei declară "
            + "`IDocumentCuPostareExplicita` (32a extins): explicația NU spune „linia nu contează” — spune că nota "
            + "o dau conturile culese pe linie. Steagul e al TIPULUI, nu al liniei: pe FCT rămâne stins",
            ntc.Contare.PostareExplicita && ntc.Contare.Castigator == null
            && ntc.Contare.Nivel == nameof(NivelContare.Niciuna)
            && !ntc.Contare.Concluzie.Contains("nu contează")
            && ntc.Contare.Concluzie.Contains("EXPLICIT")
            && !fctServiciu.Contare.PostareExplicita);
        s.Check("F24-E5 (DEC) postarea explicită are DOUĂ forme, iar explicația le distinge: pe NTC o poartă "
            + "TIPUL (fără nicio regulă), pe Decont o poartă LINIA (32a) — acolo regula generică se aplică și "
            + "totuși contul cules pe linie o bate punctual. Steagul se ridică din `[TipDetaliu]`, nu din "
            + "interfața documentului, deci Decontul nu mai era invizibil",
            dec.Contare.PostareExplicita && dec.Contare.Castigator != null
            && dec.Contare.Concluzie.Contains("bate punctual"));

        // ── F24-E6: consistența cu motorul (42c) ─────────────────────────────────
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = "F24X-1";
        fct.Data = azi;
        fct.Predator = furnizor;
        fct.Primitor = mag1;
        var linie = os.CreateObject<FacturaIntrareDetaliu>();
        linie.Document = fct;
        linie.TipMaterial = tipServiciu;
        linie.Cantitate = 1m;
        linie.PretUnitar = 100m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fct);
        string Simbol(Guid id) =>
            os.GetObjectsQuery<Cont>().Where(c => c.ID == id).Select(c => c.Simbol).FirstOrDefault();
        var noteCub = CubScena.Note(os, fct.ID).Where(p => !p.Storno).ToList();
        Console.WriteLine($"     MĂSURAT (F24-E6): motorul a postat {noteCub.Count} postări — "
            + $"{string.Join(", ", noteCub.Where(p => p.Debit).Select(p => Simbol(p.Cont)))} = "
            + $"{string.Join(", ", noteCub.Where(p => p.Credit).Select(p => Simbol(p.Cont)))}; explicația spunea "
            + $"{fctServiciu.Contare.ContDebit.Simbol} = {fctServiciu.Contare.ContCredit.Simbol}");
        s.Check("F24-E6 CONSISTENȚĂ (42c): pe un document REAL echivalent (FCT cu o linie de serviciu, același "
            + "furnizor și aceeași gestiune), postarea pe care motorul o scrie în cub are EXACT "
            + "conturile pe care explicația le anunțase — proba că „Explică” nu e o a doua rezolvare, ci aceeași",
            noteCub.Count(p => p.Debit) == 1 && noteCub.Count(p => p.Credit) == 1
            && Simbol(noteCub.Single(p => p.Debit).Cont) == fctServiciu.Contare.ContDebit.Simbol
            && Simbol(noteCub.Single(p => p.Credit).Cont) == fctServiciu.Contare.ContCredit.Simbol);

        // ── F24-E7: implicitul din explicație == implicitul culegerii ────────────
        var explicatieCuPartener = ExplicaApply.Explica(os, new ExplicaCerere(fctTip.ID, tipServiciu.ID, +1, azi,
            furnizor.ID, mag1.ID, furnizor.ID, null));
        var laCulegere = ImpliciteService.TipTva(os, fctTip.ID, furnizor.ID, null, azi);
        Console.WriteLine($"     MĂSURAT (F24-E7): explicație ⇒ {explicatieCuPartener.Implicit.TipTva}/"
            + $"{explicatieCuPartener.Implicit.Sursa} ({explicatieCuPartener.Implicit.Candidati.Length} candidați); "
            + $"culegere ⇒ {laCulegere.Sursa}");
        s.Check("F24-E7 blocul `Implicit` al explicației e ACELAȘI verdict pe care îl primește linia la culegere: "
            + "`ImpliciteService.TipTva` a devenit wrapper-ul lui `.Rezultat`, deci tipul, sursa și motivul nu pot "
            + "diverge — iar explicația arată în plus rândurile candidate cu motivul eliminării",
            explicatieCuPartener.Implicit.TipTvaId == laCulegere.TipTvaId
            && explicatieCuPartener.Implicit.Sursa == laCulegere.Sursa.ToString()
            && explicatieCuPartener.Implicit.Motiv == laCulegere.Motiv
            && explicatieCuPartener.Implicit.Candidati.Length > 0);

        // ── F24-E8: explicația declară ce ar refuza GARDUL clasei de document ────
        // Tipuri NOI, exact scenariul din review: un Configurator adaugă Tipul, dar
        // rândul de contare per Tip nu există încă. Motorul refuză operarea (38c/64);
        // fără rezervă, explicația ar fi anunțat tocmai postarea greșită pe care
        // gardul o previne (credit = contul de STOC al Tipului, la preț de vânzare).
        var fclTip = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL");
        var tipNouStoc = os.CreateObject<TipMaterial>();
        tipNouStoc.Cod = Marcaj + "-STOC";
        tipNouStoc.Denumire = "Tip de stoc fără regulă de vânzare";
        tipNouStoc.Clasa = os.GetObjectByKey<TipMaterial>(tipStoc.ID).Clasa;
        var tipNouVir = os.CreateObject<TipMaterial>();
        tipNouVir.Cod = Marcaj + "-VIR";
        tipNouVir.Denumire = "Cont de tranzit fără regulă exactă";
        tipNouVir.Clasa = os.GetObjectByKey<TipMaterial>(tipVir.ID).Clasa;
        os.CommitChanges();

        var fclFaraRegula = Explica(fclTip, tipNouStoc, +1);
        var pltFaraRegula = Explica(pltTip, tipNouVir, +1);
        Console.WriteLine($"     MĂSURAT (F24-E8): FCL × Tip de stoc NOU ⇒ nivel {fclFaraRegula.Contare.Nivel}, "
            + $"{(fclFaraRegula.Contare.Rezerve ?? []).Length} rezervă(e) — „{fclFaraRegula.Contare.Concluzie}”; "
            + $"PLT × Tip de virament NOU ⇒ nivel {pltFaraRegula.Contare.Nivel}, "
            + $"{(pltFaraRegula.Contare.Rezerve ?? []).Length} rezervă(e) — „{pltFaraRegula.Contare.Concluzie}”");
        s.Check("F24-E8 „Explică” nu anunță o postare pe care MOTORUL o refuză: nivelul minim de contare cerut de "
            + "tip e acum contract DECLARAT pe clasa documentului (`GardContare`), citit de gardul din "
            + "`Document.ValideazaOperare` ȘI de explicație. Pe un Tip de stoc fără rând de vânzare, FCL-ul "
            + "spune că regula câștigătoare AR posta, dar operarea ar fi refuzată — nu „se postează 4111 = 3xx”",
            (fclFaraRegula.Contare.Rezerve ?? []).Length == 1
            && fclFaraRegula.Contare.Rezerve[0].Contains("regulă de contare de vânzare")
            && !fclFaraRegula.Contare.Concluzie.Contains("Se postează")
            && fclFaraRegula.Contare.Concluzie.Contains("refuzată"));
        s.Check("F24-E8 (VIR) aceeași rezervă pe cealaltă declarație: viramentul cere o regulă cel puțin pe "
            + "NATURĂ (64), deci un Tip de virament nou, care cade pe genericul trezoreriei, ar posta „destinație "
            + "= sursă” pe ambele picioare — explicația o spune înainte ca operatorul să încerce",
            (pltFaraRegula.Contare.Rezerve ?? []).Length == 1
            && pltFaraRegula.Contare.Rezerve[0].Contains("regulă de contare potrivită")
            && pltFaraRegula.Contare.Concluzie.Contains("refuzată"));
        // Tipul care ARE rândul rămâne fără rezervă: gardul nu e un avertisment permanent.
        s.Check("F24-E8 (control) pe Tipul care ARE rândul exact, rezerva dispare: `Rezerve` e o listă de refuzuri "
            + "REALE, nu o notă de subsol pe tot blocul",
            (Explica(pltTip, tipVir, +1).Contare.Rezerve ?? []).Length == 0
            && (Explica(fctTip, tipServiciu, +1).Contare.Rezerve ?? []).Length == 0);

        Curata(os);
        s.Check("F24-E1…E8: scena nu lasă urme (furnizorul, factura și Tipurile de probă se purjează FIZIC)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<TipMaterial>().Any(t => t.Cod.StartsWith(Marcaj)));
    }
}
