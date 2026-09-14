using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.EF;
// `IgnoreQueryFilters` — raportul trebuie să vadă și rândurile ȘTERSE LOGIC,
// fiindcă exact ele sunt capătul mort al unei referințe de politică.
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>
/// O constatare a raportului de profil: unde, pe ce rând, ce fel de problemă și
/// ce înseamnă. `Cheie` e eticheta LIZIBILĂ a rândului (codul tipului de
/// document, codul tipului de TVA…), nu un `Guid`: raportul se citește, nu se
/// dereferențiază.
/// </summary>
public sealed record ConstatareProfil(string Tabel, string Cheie, FelConstatare Fel, string Mesaj);

// RAPORTUL DE VERIFICARE A PROFILULUI (felia 23, F23-D8).
//
// Împărțirea muncii cu seed-ul, care e conținutul deciziei: SEED-ul ARUNCĂ,
// raportul ARATĂ. Golurile de mapare sunt chiar funcțiile pe care le cheamă
// `--updateDatabase` (`ContaSeeder.GoluriMapari`), nu o a doua copie care ar
// putea rămâne în urmă.
//
// De ce e nevoie de el: de la felia 23 politicile se editează din client, iar
// nomenclatoarele seed-uite poartă un timbru de proveniență. Fără raport,
// divergențele dintre profilul livrat și baza clientului rămân invizibile până
// când o declarație iese greșită. Cu el, „ce s-a schimbat față de profil?" are
// un răspuns, iar „de ce nu se generează X?" are o cauză.
//
// Ce NU e: nu refuză nimic și nu repară nimic. Un rând `RandManual` e o
// constatare, nu o eroare — clientul are voie să-și modifice politicile
// (decizia 4). Raportul spune doar CE s-a abătut de la seed.
//
// Citește prin FK-uri + dicționare încărcate o dată (25b): nicio navigație lazy
// în timpul enumerării, deși aici n-ar fi hot-path — disciplina e a modulului,
// nu a performanței.
public static class VerificareProfilService {
    // Câte rânduri se enumeră per tabel înainte de a rezuma restul. Pe o bază
    // migrată dar încă ne-seed-uită TOATE rândurile au `DinSeed = false`, iar
    // planul de conturi singur ar produce mii de constatări identice — un raport
    // care nu se poate citi nu e un raport. Rezumatul păstrează cifra.
    const int MaximPerTabel = 200;

    public static IReadOnlyList<ConstatareProfil> Raporteaza(IObjectSpace os) {
        var constatari = new List<ConstatareProfil>();
        var profil = os.GetObjectsQuery<SetareProfil>().FirstOrDefault()?.Profil
            ?? ProfilContabil.Privat;

        // Etichetele, INCLUSIV ale rândurilor șterse logic: sunt numele
        // majorității rândurilor de politică, iar un tip șters e chiar cazul pe
        // care categoria (b) îl caută.
        var coduriTip = CoduriCuSterse<TipDocument>(os, t => t.Cod);
        var coduriTva = CoduriCuSterse<TipTva>(os, t => t.Cod);
        var coduriMaterial = CoduriCuSterse<TipMaterial>(os, t => t.Cod);
        string CodTip(Guid id) => coduriTip.GetValueOrDefault(id) ?? "(tip necunoscut)";
        string CodTva(Guid id) => coduriTva.GetValueOrDefault(id) ?? "(tip TVA necunoscut)";
        string CodMaterial(Guid id) => coduriMaterial.GetValueOrDefault(id) ?? "(tip material necunoscut)";

        RanduriManuale(os, constatari, CodTip, CodTva, CodMaterial);
        ReferinteSterse(os, constatari, CodTip, CodTva);
        TipuriInactiveReferite(os, constatari, CodTip);
        AncoreLipsa(os, constatari, CodTip);
        foreach (var gol in ContaSeeder.GoluriMapari(os, profil))
            constatari.Add(new ConstatareProfil("Mapări", "(profil)", FelConstatare.MapareLipsa, gol));
        return constatari;
    }

    static Dictionary<Guid, string> CoduriCuSterse<T>(IObjectSpace os, Func<T, string> cod)
            where T : BaseObject =>
        os.GetObjectsQuery<T>().IgnoreQueryFilters().ToList()
            .ToDictionary(x => x.ID, x => cod(x) ?? "(fără cod)");

    /// <summary>Id-urile rândurilor ȘTERSE LOGIC (văzute doar peste filtrul global).</summary>
    static HashSet<Guid> Sterse<T>(IObjectSpace os) where T : BaseObject {
        var vii = os.GetObjectsQuery<T>().Select(x => x.ID).ToList().ToHashSet();
        return os.GetObjectsQuery<T>().IgnoreQueryFilters().Select(x => x.ID).ToList()
            .Where(id => !vii.Contains(id)).ToHashSet();
    }

    // ── (a) Rândurile care NU mai sunt ale seed-ului ────────────────────────
    // `DinSeed = false` = creat de client SAU editat de client (gardianul stinge
    // timbrul la orice scriere securizată — F23-D4). Cele două nu se disting, și
    // nici nu trebuie: pentru raport amândouă înseamnă „aici baza s-a abătut de
    // la profilul livrat".
    static void RanduriManuale(IObjectSpace os, List<ConstatareProfil> constatari,
            Func<Guid, string> codTip, Func<Guid, string> codTva, Func<Guid, string> codMaterial) {
        var etichete = Etichete(codTip, codTva, codMaterial);
        foreach (var tip in Politici.TipuriConfigurabile) {
            if (!etichete.TryGetValue(tip, out var tabel))
                throw new InvalidOperationException(
                    $"Tipul configurabil `{tip.Name}` n-are etichetă în raportul de profil (83i).");
            var manuale = os.GetObjects(tip, CriteriaOperator.Parse("DinSeed = ?", false))
                .Cast<ICuProvenienta>().ToList();
            foreach (var rand in manuale.Take(MaximPerTabel))
                constatari.Add(new ConstatareProfil(tabel.Nume, tabel.Eticheta(rand),
                    FelConstatare.RandManual,
                    "Rândul nu poartă timbrul seed-ului: a fost creat sau modificat pe această bază."));
            if (manuale.Count > MaximPerTabel)
                constatari.Add(new ConstatareProfil(tabel.Nume, "(rezumat)", FelConstatare.RandManual,
                    $"Încă {manuale.Count - MaximPerTabel} rânduri fără timbrul seed-ului "
                    + $"(din {manuale.Count} în total) — nelistate."));
        }
    }

    static (string Nume, Func<object, string> Eticheta) Tabel<T>(string nume, Func<T, string> eticheta) =>
        (nume, o => eticheta((T)o));

    // Numele de tabel și eticheta LIZIBILĂ a rândului, per tip configurabil
    // (83i). Un tip din `TipuriConfigurabile` care lipsește de aici oprește
    // raportul — cheia unui rând nu se inventează dintr-un `ToString()`.
    static Dictionary<Type, (string Nume, Func<object, string> Eticheta)> Etichete(
            Func<Guid, string> codTip, Func<Guid, string> codTva, Func<Guid, string> codMaterial) => new() {
        [typeof(TipDocument)] = Tabel<TipDocument>("Tipuri de document",
            t => t.Cod ?? t.Denumire ?? "(fără cod)"),
        [typeof(TipTva)] = Tabel<TipTva>("Tipuri de TVA", t => t.Cod ?? t.Denumire ?? "(fără cod)"),
        [typeof(Cont)] = Tabel<Cont>("Plan de conturi", c => c.Simbol ?? "(fără simbol)"),
        [typeof(ClasaProdus)] = Tabel<ClasaProdus>("Clase de produs", c => c.Cod ?? "(fără cod)"),
        [typeof(TipMaterial)] = Tabel<TipMaterial>("Tipuri de material", t => t.Cod ?? "(fără cod)"),
        [typeof(RegulaStoc)] = Tabel<RegulaStoc>("Reguli de stoc",
            r => $"{codTip(r.TipDocumentId)} / {r.Latura}"),
        [typeof(RegulaContare)] = Tabel<RegulaContare>("Reguli de contare",
            r => codTip(r.TipDocumentId)),
        [typeof(PoliticaConex)] = Tabel<PoliticaConex>("Politici conex",
            p => codTip(p.TipDocumentSursaId)),
        [typeof(PoliticaScadenta)] = Tabel<PoliticaScadenta>("Politici de scadență",
            p => codTip(p.TipDocumentId)),
        [typeof(PoliticaValidare)] = Tabel<PoliticaValidare>("Politici de validare",
            p => codTip(p.TipDocumentId)),
        [typeof(PoliticaTva)] = Tabel<PoliticaTva>("Politici de TVA", p => codTip(p.TipDocumentId)),
        [typeof(PoliticaInchidereTva)] = Tabel<PoliticaInchidereTva>("Politici de închidere TVA",
            p => codTip(p.TipDocumentId)),
        [typeof(PoliticaNumerotare)] = Tabel<PoliticaNumerotare>("Politici de numerotare",
            p => codTip(p.TipDocumentId)),
        [typeof(PoliticaMiscareSaft)] = Tabel<PoliticaMiscareSaft>("Politici de mișcare SAF-T",
            p => $"{codTip(p.TipDocumentId)} / {p.TipStoc}"),
        [typeof(PoliticaTvaImplicit)] = Tabel<PoliticaTvaImplicit>("Implicite de TVA",
            p => Cheia(codTip, p)),
        [typeof(MapareD300)] = Tabel<MapareD300>("Mapări D300",
            m => $"{codTva(m.TipTvaId)} / {m.Sens}"),
        [typeof(MapareD394)] = Tabel<MapareD394>("Mapări D394",
            m => $"{codTva(m.TipTvaId)} / {m.Sens}"),
        [typeof(PoliticaAmortizare)] = Tabel<PoliticaAmortizare>("Politici de amortizare",
            p => codMaterial(p.TipMaterialId)),
        [typeof(RegulaDeductibilitate)] = Tabel<RegulaDeductibilitate>("Reguli de deductibilitate",
            r => $"{r.Categorie} de la {r.DeLa:dd.MM.yyyy}"),
    };

    static string Cheia(Func<Guid, string> codTip, PoliticaTvaImplicit p) =>
        $"{codTip(p.TipDocumentId)} × {p.ClasaFiscala?.ToString() ?? "orice clasă"}"
        + (p.ValabilDeLa == null ? "" : $" de la {p.ValabilDeLa:dd.MM.yyyy}");

    // ── (b) Referințe spre rânduri ȘTERSE LOGIC ────────────────────────────
    // Ștergerea e amânată (60a), iar filtrul global ascunde rândul șters — deci
    // o politică ce îl referă arată cu navigația goală și FK-ul plin. Motorul o
    // citește ca „fără cont" / „fără tip" și tace. Aici se strigă.
    static void ReferinteSterse(IObjectSpace os, List<ConstatareProfil> constatari,
            Func<Guid, string> codTip, Func<Guid, string> codTva) {
        var tipuriSterse = Sterse<TipDocument>(os);
        var tvaSterse = Sterse<TipTva>(os);
        var conturiSterse = Sterse<Cont>(os);
        var claseSterse = Sterse<ClasaProdus>(os);
        var materialeSterse = Sterse<TipMaterial>(os);
        var randuriSterse = Sterse<RandD300>(os);

        void Ref(string tabel, string cheie, Guid? id, HashSet<Guid> sterse, string camp) {
            if (id is Guid g && g != Guid.Empty && sterse.Contains(g))
                constatari.Add(new ConstatareProfil(tabel, cheie, FelConstatare.ReferintaStearsa,
                    $"Câmpul „{camp}” arată spre un rând ȘTERS din nomenclator — motorul îl citește ca "
                    + "absent și tace. Alegeți alt rând sau reactivați-l."));
        }

        foreach (var r in os.GetObjectsQuery<RegulaStoc>().ToList()) {
            var cheie = $"{codTip(r.TipDocumentId)} / {r.Latura}";
            Ref("Reguli de stoc", cheie, r.TipDocumentId, tipuriSterse, "Tip document");
            Ref("Reguli de stoc", cheie, r.ClasaId, claseSterse, "Clasă");
        }
        foreach (var r in os.GetObjectsQuery<RegulaContare>().ToList()) {
            var cheie = codTip(r.TipDocumentId);
            Ref("Reguli de contare", cheie, r.TipDocumentId, tipuriSterse, "Tip document");
            Ref("Reguli de contare", cheie, r.TipMaterialId, materialeSterse, "Tip material");
            Ref("Reguli de contare", cheie, r.ContDebitId, conturiSterse, "Cont debitor");
            Ref("Reguli de contare", cheie, r.ContCreditId, conturiSterse, "Cont creditor");
        }
        foreach (var p in os.GetObjectsQuery<PoliticaConex>().ToList()) {
            var cheie = codTip(p.TipDocumentSursaId);
            Ref("Politici conex", cheie, p.TipDocumentSursaId, tipuriSterse, "Tip document sursă");
            Ref("Politici conex", cheie, p.TipDocumentTintaId, tipuriSterse, "Tip document țintă");
        }
        foreach (var p in os.GetObjectsQuery<PoliticaScadenta>().ToList())
            Ref("Politici de scadență", codTip(p.TipDocumentId), p.TipDocumentId, tipuriSterse, "Tip document");
        foreach (var p in os.GetObjectsQuery<PoliticaValidare>().ToList())
            Ref("Politici de validare", codTip(p.TipDocumentId), p.TipDocumentId, tipuriSterse, "Tip document");
        foreach (var p in os.GetObjectsQuery<PoliticaNumerotare>().ToList())
            Ref("Politici de numerotare", codTip(p.TipDocumentId), p.TipDocumentId, tipuriSterse, "Tip document");
        foreach (var p in os.GetObjectsQuery<PoliticaTva>().ToList()) {
            var cheie = codTip(p.TipDocumentId);
            Ref("Politici de TVA", cheie, p.TipDocumentId, tipuriSterse, "Tip document");
            Ref("Politici de TVA", cheie, p.ContrapartidaFallbackId, conturiSterse, "Contrapartidă fallback");
        }
        foreach (var p in os.GetObjectsQuery<PoliticaInchidereTva>().ToList()) {
            var cheie = codTip(p.TipDocumentId);
            Ref("Politici de închidere TVA", cheie, p.TipDocumentId, tipuriSterse, "Tip document");
            Ref("Politici de închidere TVA", cheie, p.ContDeductibilaId, conturiSterse, "Cont TVA deductibilă");
            Ref("Politici de închidere TVA", cheie, p.ContColectataId, conturiSterse, "Cont TVA colectată");
            Ref("Politici de închidere TVA", cheie, p.ContDePlataId, conturiSterse, "Cont TVA de plată");
            Ref("Politici de închidere TVA", cheie, p.ContDeRecuperatId, conturiSterse, "Cont TVA de recuperat");
        }
        foreach (var p in os.GetObjectsQuery<PoliticaMiscareSaft>().ToList())
            Ref("Politici de mișcare SAF-T", $"{codTip(p.TipDocumentId)} / {p.TipStoc}",
                p.TipDocumentId, tipuriSterse, "Tip document");
        foreach (var p in os.GetObjectsQuery<PoliticaTvaImplicit>().ToList()) {
            var cheie = Cheia(codTip, p);
            Ref("Implicite de TVA", cheie, p.TipDocumentId, tipuriSterse, "Tip document");
            Ref("Implicite de TVA", cheie, p.TipTvaId, tvaSterse, "Tip TVA");
        }
        foreach (var m in os.GetObjectsQuery<MapareD300>().ToList()) {
            var cheie = $"{codTva(m.TipTvaId)} / {m.Sens}";
            Ref("Mapări D300", cheie, m.TipTvaId, tvaSterse, "Tip TVA");
            Ref("Mapări D300", cheie, m.RandId, randuriSterse, "Rând D300");
        }
        foreach (var m in os.GetObjectsQuery<MapareD394>().ToList())
            Ref("Mapări D394", $"{codTva(m.TipTvaId)} / {m.Sens}", m.TipTvaId, tvaSterse, "Tip TVA");
        foreach (var t in os.GetObjectsQuery<TipDocument>().ToList())
            Ref("Tipuri de document", t.Cod ?? "?", t.TipTvaImplicitId, tvaSterse, "Tip TVA implicit");
        foreach (var t in os.GetObjectsQuery<TipMaterial>().ToList())
            Ref("Tipuri de material", t.Cod ?? "?", t.ContImplicitId, conturiSterse, "Cont implicit");
    }

    // ── (c) Implicite care țintesc un `TipTva` INACTIV ─────────────────────
    // Gardianul le refuză la scriere (F23-D5), dar o bază poate ajunge aici pe
    // ușa de sistem (seed vechi, import, migrare). Rezolvarea le SARE cu motiv;
    // raportul spune unde, ca motivul să nu rămână doar în indiciul liniei.
    static void TipuriInactiveReferite(IObjectSpace os, List<ConstatareProfil> constatari,
            Func<Guid, string> codTip) {
        var inactive = os.GetObjectsQuery<TipTva>().Where(t => !t.Activ)
            .Select(t => new { t.ID, t.Cod }).ToList()
            .ToDictionary(t => t.ID, t => t.Cod ?? t.ID.ToString());
        if (inactive.Count == 0)
            return;
        void Ref(string tabel, string cheie, Guid? id, string unde) {
            if (id is Guid g && inactive.TryGetValue(g, out var cod))
                constatari.Add(new ConstatareProfil(tabel, cheie, FelConstatare.TipTvaInactivReferit,
                    $"Tipul de TVA „{cod}” e INACTIV, dar e implicit {unde} — rezolvarea îl sare, "
                    + "iar linia rămâne fără tip de TVA propus."));
        }
        foreach (var t in os.GetObjectsQuery<TipDocument>().ToList())
            Ref("Tipuri de document", t.Cod ?? "?", t.TipTvaImplicitId, "pe ancora tipului");
        foreach (var p in os.GetObjectsQuery<PoliticaTvaImplicit>().ToList())
            Ref("Implicite de TVA", Cheia(codTip, p), p.TipTvaId, "pe rândul de politică");
        foreach (var p in os.GetObjectsQuery<Partener>().Where(x => x.TipTvaImplicitId != null).ToList())
            Ref("Parteneri", p.Cod ?? "?", p.TipTvaImplicitId, "pe partener");
        foreach (var p in os.GetObjectsQuery<Produs>().Where(x => x.TipTvaImplicitId != null).ToList())
            Ref("Produse", p.Cod ?? "?", p.TipTvaImplicitId, "pe produs");
    }

    // ── (d) Tip cu `PoliticaTva`, dar fără ancoră de culegere ──────────────
    // `PoliticaTva` spune „tipul ăsta POSTEAZĂ TVA"; ancora `TipTvaImplicit`
    // spune „iată ce se propune la culegere". Un tip cu prima și fără a doua e
    // configurat pe jumătate: motorul postează, dar operatorul culege fiecare
    // linie de mână — și, mai rău, o linie uitată fără tip nu produce niciun
    // rând de TVA, tăcut.
    static void AncoreLipsa(IObjectSpace os, List<ConstatareProfil> constatari, Func<Guid, string> codTip) {
        var cuAncora = os.GetObjectsQuery<TipDocument>()
            .Where(t => t.TipTvaImplicitId != null).Select(t => t.ID).ToList().ToHashSet();
        foreach (var tipId in os.GetObjectsQuery<PoliticaTva>().Select(p => p.TipDocumentId).ToList())
            if (!cuAncora.Contains(tipId))
                constatari.Add(new ConstatareProfil("Tipuri de document", codTip(tipId),
                    FelConstatare.PoliticaLipsa,
                    "Tipul postează TVA (are politică de TVA), dar n-are tip de TVA implicit — "
                    + "fiecare linie se culege de mână, iar una uitată nu produce niciun rând de TVA."));
    }
}
