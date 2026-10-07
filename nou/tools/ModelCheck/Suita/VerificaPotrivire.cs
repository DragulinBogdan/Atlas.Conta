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
// F24-P1…P7 — POTRIVIREA politicilor, ca funcții PURE pe fapte fabricate
// ---------------------------------------------------------------------------
// `Potrivire` e SINGURA definiție a potrivirii: o consumă motorul și cele cinci
// foste oglinzi de mână (F24-D5). Probele de aici exercită axele care nu se pot
// fabrica economic cu documente reale — semnul care scoate rândul din joc la
// TOATE nivelurile, dublura, natura necunoscută — și fixează divergența
// DECLARATĂ față de a doua definiție a potrivirii de stoc, care trăia în
// `DescarcareService` și a murit: căderea pe regula generică de stoc e păzită
// de `Natura == Stoc`, acolo nu era.
static class VerificaPotrivire {
    public static void Ruleaza(Suita s) {
        var divergente = new List<string>();
        void Cere(string ce, bool ok) {
            if (!ok)
                divergente.Add(ce);
        }
        void Regula(string enunt) {
            s.Check(enunt + (divergente.Count > 0 ? $" — divergențe: {string.Join("; ", divergente)}" : ""),
                divergente.Count == 0);
            divergente.Clear();
        }

        var tipUnu = Guid.NewGuid();
        var tipDoi = Guid.NewGuid();
        var tipTrei = Guid.NewGuid();
        var clasaUnu = Guid.NewGuid();
        var clasaDoi = Guid.NewGuid();
        RegulaContareFapt Rc(Guid? tipMaterial, NaturaClasa? naturaFiltru, int? semnFiltru) =>
            new(Guid.NewGuid(), tipMaterial, naturaFiltru, semnFiltru, false, SursaCont.Explicit, null,
                SursaCont.Explicit, null, false, null, null, null);
        RegulaStocFapt Rs(LaturaDocument latura, Guid? clasaId, TipStoc tipStoc, int semn) =>
            new(Guid.NewGuid(), latura, clasaId, tipStoc, semn, false);
        LinieFapt LinieDe(Guid tipMaterial, Guid? clasaId, NaturaClasa? natura, int semn) =>
            new(tipMaterial, clasaId, natura, semn, null, null);

        // ── F24-P1: semnul scoate rândul din joc la TOATE nivelurile ─────────────
        var rExactMinus = Rc(tipUnu, null, -1);
        var rNaturaMinus = Rc(null, NaturaClasa.Stoc, -1);
        var rGenericPlus = Rc(null, null, +1);
        IReadOnlyList<RegulaContareFapt> reguliSemn = [rExactMinus, rNaturaMinus, rGenericPlus];
        var pePlus = Potrivire.Contare(reguliSemn, LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, +1));
        var peMinus = Potrivire.Contare(reguliSemn, LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, -1));
        Cere("linia + cade pe generic", pePlus.Castigator?.Id == rGenericPlus.Id && pePlus.Nivel == NivelContare.Generic);
        Cere("exactul de minus e eliminat pe semn", pePlus.Candidati[0].Motiv == MotivEliminare.SemnNepotrivit);
        Cere("natura de minus e eliminată pe semn", pePlus.Candidati[1].Motiv == MotivEliminare.SemnNepotrivit);
        Cere("câștigătorul n-are motiv", pePlus.Candidati[2].Motiv == null);
        Cere("linia − ia exactul", peMinus.Castigator?.Id == rExactMinus.Id
            && peMinus.Nivel == NivelContare.TipMaterialExact);
        Cere("natura rămâne nivel mai slab pe −", peMinus.Candidati[1].Motiv == MotivEliminare.NivelMaiSlab);
        Cere("genericul de plus iese pe semn la −", peMinus.Candidati[2].Motiv == MotivEliminare.SemnNepotrivit);
        Console.WriteLine($"     MĂSURAT (F24-P1): linie + ⇒ {pePlus.Nivel}, linie − ⇒ {peMinus.Nivel}.");
        Regula("F24-P1 `RegulaContare`: filtrul de SEMN se aplică ÎNAINTEA nivelurilor de specificitate — "
            + "o linie pozitivă sare peste regula exactă de minus ȘI peste cea de natură, și cade pe regula "
            + "generică de plus (axa pe care gardul VIR o oglindea, iar cele trei garduri 38c nu — 64)");

        // ── F24-P2: Tip exact bate Natura bate generic ───────────────────────────
        var rExact = Rc(tipUnu, null, null);
        var rExactDublura = Rc(tipUnu, null, null);
        var rAltTip = Rc(tipDoi, null, null);
        var rNaturaStoc = Rc(null, NaturaClasa.Stoc, null);
        var rNaturaServiciu = Rc(null, NaturaClasa.Serviciu, null);
        var rGeneric = Rc(null, null, null);
        IReadOnlyList<RegulaContareFapt> reguliNivel =
            [rAltTip, rNaturaServiciu, rGeneric, rNaturaStoc, rExact, rExactDublura];
        var peExact = Potrivire.Contare(reguliNivel, LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, +1));
        var peNatura = Potrivire.Contare(reguliNivel, LinieDe(tipTrei, clasaUnu, NaturaClasa.Stoc, +1));
        var peGeneric = Potrivire.Contare(reguliNivel, LinieDe(tipTrei, clasaUnu, NaturaClasa.Imobilizare, +1));
        var peNecunoscut = Potrivire.Contare([rNaturaStoc], LinieDe(tipTrei, null, null, +1));
        var faraReguli = Potrivire.Contare([], LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, +1));
        Cere("exactul câștigă", peExact.Castigator?.Id == rExact.Id && peExact.Nivel == NivelContare.TipMaterialExact);
        Cere("alt Tip ⇒ TipMaterialDiferit", peExact.Candidati[0].Motiv == MotivEliminare.TipMaterialDiferit);
        Cere("altă natură ⇒ NaturaDiferita", peExact.Candidati[1].Motiv == MotivEliminare.NaturaDiferita);
        Cere("genericul ⇒ NivelMaiSlab", peExact.Candidati[2].Motiv == MotivEliminare.NivelMaiSlab);
        Cere("natura potrivită ⇒ NivelMaiSlab", peExact.Candidati[3].Motiv == MotivEliminare.NivelMaiSlab);
        Cere("al doilea exact ⇒ Dublura", peExact.Candidati[5].Motiv == MotivEliminare.Dublura);
        Cere("fără exact ⇒ natura", peNatura.Castigator?.Id == rNaturaStoc.Id && peNatura.Nivel == NivelContare.Natura);
        Cere("fără natură potrivită ⇒ generic", peGeneric.Castigator?.Id == rGeneric.Id
            && peGeneric.Nivel == NivelContare.Generic);
        Cere("natura necunoscută nu potrivește un filtru de natură", peNecunoscut.Castigator == null
            && peNecunoscut.Nivel == NivelContare.Niciuna
            && peNecunoscut.Candidati[0].Motiv == MotivEliminare.NaturaDiferita);
        Cere("fără reguli ⇒ Niciuna", faraReguli.Castigator == null && faraReguli.Nivel == NivelContare.Niciuna
            && faraReguli.Candidati.Count == 0);
        Console.WriteLine($"     MĂSURAT (F24-P2): {peExact.Nivel} / {peNatura.Nivel} / {peGeneric.Nivel} / "
            + $"{peNecunoscut.Nivel} / {faraReguli.Nivel}.");
        Regula("F24-P2 `RegulaContare`: TipMaterial exact bate `NaturaFiltru`, care bate regula generică (26c); "
            + "câștigătorul e PRIMUL de la cel mai înalt nivel (al doilea rând exact e `Dublura`), rândurile "
            + "valide de sub el sunt `NivelMaiSlab`, iar o linie cu Tipul lipsă din nomenclator (natură "
            + "necunoscută) NU potrivește niciun filtru de natură");

        // ── F24-P3: stoc, per latură ─────────────────────────────────────────────
        var rsSpecMarfuri = Rs(LaturaDocument.Predator, clasaUnu, TipStoc.Marfuri, -1);
        var rsSpecConsum = Rs(LaturaDocument.Predator, clasaUnu, TipStoc.Consum, -1);
        var rsPredGeneric = Rs(LaturaDocument.Predator, null, TipStoc.Magazie, -1);
        var rsPrimGeneric = Rs(LaturaDocument.Primitor, null, TipStoc.Consum, +1);
        IReadOnlyList<RegulaStocFapt> reguliStoc = [rsSpecMarfuri, rsPredGeneric, rsSpecConsum, rsPrimGeneric];
        var stocSpecific = Potrivire.Stoc(reguliStoc, LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, -1));
        var stocGeneric = Potrivire.Stoc(reguliStoc, LinieDe(tipUnu, clasaDoi, NaturaClasa.Stoc, -1));
        var oLatura = Potrivire.Stoc([rsPredGeneric], LinieDe(tipUnu, clasaDoi, NaturaClasa.Stoc, -1));
        Cere("două laturi", stocSpecific.Count == 2);
        Cere("predatorul ia clasa exactă, cu AMBELE reguli specifice",
            stocSpecific[0].Latura == LaturaDocument.Predator && stocSpecific[0].Nivel == NivelStoc.ClasaExacta
            && stocSpecific[0].Reguli.Count == 2);
        Cere("primitorul rămâne pe generic", stocSpecific[1].Latura == LaturaDocument.Primitor
            && stocSpecific[1].Nivel == NivelStoc.Generic && stocSpecific[1].Reguli.Count == 1);
        Cere("altă clasă ⇒ genericul laturii", stocGeneric[0].Nivel == NivelStoc.Generic
            && stocGeneric[0].Reguli[0].Id == rsPredGeneric.Id);
        Cere("o latură fără reguli nu apare", oLatura.Count == 1 && oLatura[0].Latura == LaturaDocument.Predator);
        Console.WriteLine($"     MĂSURAT (F24-P3): clasă exactă ⇒ {stocSpecific[0].Nivel} ({stocSpecific[0].Reguli.Count} "
            + $"reguli), altă clasă ⇒ {stocGeneric[0].Nivel}.");
        Regula("F24-P3 `RegulaStoc`: potrivirea e PER LATURĂ, regulile specifice pe Clasa liniei bat regula "
            + "generică, iar TOATE regulile specifice ale laturii trag (o latură poate scrie în două registre); "
            + "o latură fără nicio regulă nu apare în rezultat");

        // ── F24-P4: divergența DECLARATĂ față de a doua definiție, moartă ────────
        var neStoc = Potrivire.Stoc([rsPredGeneric], LinieDe(tipUnu, clasaDoi, NaturaClasa.Serviciu, -1));
        var deStoc = Potrivire.Stoc([rsPredGeneric], LinieDe(tipUnu, clasaDoi, NaturaClasa.Stoc, -1));
        var faraGeneric = Potrivire.Stoc([rsSpecMarfuri], LinieDe(tipUnu, clasaDoi, NaturaClasa.Stoc, -1));
        Cere("linia ne-Stoc nu primește TipStoc din genericul laturii", neStoc[0].Nivel == NivelStoc.Niciuna
            && neStoc[0].Reguli.Count == 0 && neStoc[0].Motiv == MotivStoc.NaturaNuEsteStoc);
        Cere("linia de Stoc îl primește", deStoc[0].Nivel == NivelStoc.Generic);
        Cere("fără generic pe latură ⇒ FaraRegula", faraGeneric[0].Nivel == NivelStoc.Niciuna
            && faraGeneric[0].Motiv == MotivStoc.FaraRegula);
        Console.WriteLine($"     MĂSURAT (F24-P4): Natura=Serviciu ⇒ {neStoc[0].Nivel}/{neStoc[0].Motiv}, "
            + $"Natura=Stoc ⇒ {deStoc[0].Nivel}.");
        Regula("F24-P4 divergența DECLARATĂ (F24-D5): căderea pe regula generică de stoc e păzită de "
            + "`Natura == Stoc` — a doua definiție a potrivirii de stoc, moartă odată cu felia, "
            + "întorcea `TipStoc`-ul genericului și pentru o clasă ne-Stoc; motivul distinge „natura nu e stoc” "
            + "de „latura n-are regulă generică”");

        // ── F24-P5: rezolvarea contului, cu sursa care l-a dat ───────────────────
        var contExplicit = Guid.NewGuid();
        var contTip = Guid.NewGuid();
        var contPredator = Guid.NewGuid();
        var contPrimitor = Guid.NewGuid();
        var laturiPline = new LaturiFapt(contPredator, contPrimitor);
        var laturiGoale = new LaturiFapt(null, null);
        Cere("Explicit", Potrivire.Cont(SursaCont.Explicit, contExplicit, contTip, laturiPline)
            == new RezolvareCont(contExplicit, SursaRezolvata.Explicit));
        Cere("TipMaterial", Potrivire.Cont(SursaCont.TipMaterial, contExplicit, contTip, laturiPline)
            == new RezolvareCont(contTip, SursaRezolvata.TipMaterial));
        Cere("RepartitorPredator", Potrivire.Cont(SursaCont.RepartitorPredator, contExplicit, contTip, laturiPline)
            == new RezolvareCont(contPredator, SursaRezolvata.RepartitorPredator));
        Cere("RepartitorPrimitor", Potrivire.Cont(SursaCont.RepartitorPrimitor, contExplicit, contTip, laturiPline)
            == new RezolvareCont(contPrimitor, SursaRezolvata.RepartitorPrimitor));
        Cere("fallback pe contul explicit", Potrivire.Cont(SursaCont.TipMaterial, contExplicit, null, laturiGoale)
            == new RezolvareCont(contExplicit, SursaRezolvata.FallbackExplicit));
        Cere("nerezolvat", Potrivire.Cont(SursaCont.RepartitorPredator, null, contTip, laturiGoale)
            == new RezolvareCont(null, SursaRezolvata.Nerezolvat));
        Regula("F24-P5 `SursaCont`: contul vine din sursa DECLARATĂ (TipMaterial / repartitorul unei laturi / "
            + "explicit), iar contul explicit al regulii e fallback-ul când sursa nu rezolvă; fără nici sursă, "
            + "nici fallback, rezolvarea e `Nerezolvat` (refuzul cu mesaj al motorului)");

        // ── F24-P6: implicitul de TVA ────────────────────────────────────────────
        var tvaN21 = new TipTvaFapt(Guid.NewGuid(), "N21", RegimTva.Normal, true, 21m, null, null);
        var tvaN11 = new TipTvaFapt(Guid.NewGuid(), "N11", RegimTva.Normal, true, 11m, null, null);
        var tvaSdd = new TipTvaFapt(Guid.NewGuid(), "SDD", RegimTva.Scutit, true, 0m, null, null);
        var tvaMort = new TipTvaFapt(Guid.NewGuid(), "N19", RegimTva.Normal, false, 19m, null, null);
        var tipuriTva = new[] { tvaN21, tvaN11, tvaSdd, tvaMort }.ToDictionary(t => t.Id);
        var ziDoc = new DateOnly(2026, 6, 15);
        PoliticaTvaImplicitFapt Rt(ClasaFiscalaPartener? clasaFiscala, DateOnly? deLa, Guid tipTvaId) =>
            new(Guid.NewGuid(), clasaFiscala, deLa, tipTvaId, false);
        var randGeneric = Rt(null, null, tvaN21.Id);
        var randUe = Rt(ClasaFiscalaPartener.Ue, null, tvaSdd.Id);
        var randViitor = Rt(ClasaFiscalaPartener.Ue, new DateOnly(2027, 1, 1), tvaN11.Id);
        IReadOnlyList<PoliticaTvaImplicitFapt> randuri = [randGeneric, randUe, randViitor];
        var peUe = Potrivire.TvaImplicit(randuri, ClasaFiscalaPartener.Ue, ziDoc, null, null, null, tipuriTva, false);
        var randVechi = Rt(ClasaFiscalaPartener.InregistratRo, null, tvaN21.Id);
        var randNou = Rt(ClasaFiscalaPartener.InregistratRo, new DateOnly(2026, 1, 1), tvaN11.Id);
        var peEgalitate = Potrivire.TvaImplicit([randVechi, randNou], ClasaFiscalaPartener.InregistratRo,
            ziDoc, null, null, null, tipuriTva, false);
        var randMort = Rt(ClasaFiscalaPartener.Ue, null, tvaMort.Id);
        var peInactiv = Potrivire.TvaImplicit([randMort], ClasaFiscalaPartener.Ue, ziDoc, null, tvaN21.Id,
            null, tipuriTva, false);
        var peProdus = Potrivire.TvaImplicit([], null, ziDoc, tvaN21.Id, null, tvaN11.Id, tipuriTva, false);
        var peRegim = Potrivire.TvaImplicit([], null, ziDoc, tvaSdd.Id, null, tvaN11.Id, tipuriTva, false);
        var peFaraPartener = Potrivire.TvaImplicit(randuri, null, ziDoc, null, null, null, tipuriTva, true);
        Cere("clasa exactă bate genericul", peUe.Rezultat.TipTvaId == tvaSdd.Id
            && peUe.Rezultat.Sursa == SursaImplicit.Politica && peUe.RandPolitica?.Id == randUe.Id);
        Cere("genericul rămâne nivel mai slab", peUe.Candidati[0].Motiv == MotivEliminare.NivelMaiSlab);
        Cere("rândul cu dată viitoare iese", peUe.Candidati[2].Motiv == MotivEliminare.DataViitoare);
        Cere("la egalitate de clasă câștigă `ValabilDeLa` cel mai recent",
            peEgalitate.Rezultat.TipTvaId == tvaN11.Id);
        var randGeamanA = Rt(ClasaFiscalaPartener.Ue, null, tvaN21.Id);
        var randGeamanB = Rt(ClasaFiscalaPartener.Ue, null, tvaN11.Id);
        var peDublura = Potrivire.TvaImplicit([randGeamanA, randGeamanB], ClasaFiscalaPartener.Ue,
            ziDoc, null, null, null, tipuriTva, false);
        Cere("două rânduri pe aceeași cheie ⇒ `Dublura`, nu `NivelMaiSlab`",
            peDublura.Candidati.Count(k => k.Motiv == MotivEliminare.Dublura) == 1
            && peDublura.Candidati.All(k => k.Motiv != MotivEliminare.NivelMaiSlab));
        Cere("tipul INACTIV sare treapta, cu motiv", peInactiv.Rezultat.TipTvaId == tvaN21.Id
            && peInactiv.Rezultat.Sursa == SursaImplicit.Ancora
            && peInactiv.Rezultat.Motiv.Contains("INACTIV")
            && peInactiv.Candidati[0].Motiv == MotivEliminare.Inactiv);
        Cere("cota produsului peste același regim", peProdus.Rezultat.TipTvaId == tvaN11.Id
            && peProdus.Rezultat.Sursa == SursaImplicit.Produs);
        Cere("regimul bate cota la regimuri diferite", peRegim.Rezultat.TipTvaId == tvaSdd.Id
            && peRegim.Rezultat.Sursa == SursaImplicit.Partener);
        Cere("fără partener rămân doar rândurile generice", peFaraPartener.Rezultat.TipTvaId == tvaN21.Id
            && peFaraPartener.Rezultat.Motiv.Contains("Fără partener vizibil")
            && peFaraPartener.Candidati[1].Motiv == MotivEliminare.ClasaDiferita);
        Console.WriteLine($"     MĂSURAT (F24-P6): UE ⇒ {peUe.Rezultat.Sursa}, inactiv ⇒ {peInactiv.Rezultat.Sursa}, "
            + $"produs ⇒ {peProdus.Rezultat.Sursa}, regim ⇒ {peRegim.Rezultat.Sursa}.");
        Regula("F24-P6 implicitul de TVA (F23-D2), pe funcția pură: rândul de politică cel mai SPECIFIC bate "
            + "genericul și, la egalitate de clasă, `ValabilDeLa` cel mai recent; rândul cu dată viitoare și cel "
            + "de altă clasă ies cu motiv; tipul INACTIV sare TREAPTA (nu caută al doilea rând); cota produsului "
            + "se impune doar la ACELAȘI regim, altfel regimul bate; fără partener vizibil rămân doar rândurile "
            + "generice");

        // ── F24-P7: filtrul de natură al conexului ───────────────────────────────
        var conexTot = new PoliticaConexFapt(Guid.NewGuid(), Guid.NewGuid(), false, null);
        var conexStoc = new PoliticaConexFapt(Guid.NewGuid(), Guid.NewGuid(), true, NaturaClasa.Stoc);
        Cere("filtrul null trece tot", Potrivire.Conex(conexTot, LinieDe(tipUnu, clasaUnu, NaturaClasa.Serviciu, +1))
            && Potrivire.Conex(conexTot, LinieDe(tipUnu, null, null, +1)));
        Cere("filtrul Stoc trece doar Stoc",
            Potrivire.Conex(conexStoc, LinieDe(tipUnu, clasaUnu, NaturaClasa.Stoc, +1))
            && !Potrivire.Conex(conexStoc, LinieDe(tipUnu, clasaUnu, NaturaClasa.Serviciu, +1))
            && !Potrivire.Conex(conexStoc, LinieDe(tipUnu, null, null, +1)));
        Regula("F24-P7 `PoliticaConex`: filtrul de natură null trece TOATE liniile, filtrul pe o natură trece "
            + "doar liniile ei (o factură doar de servicii nu produce NIR)");
    }
}
