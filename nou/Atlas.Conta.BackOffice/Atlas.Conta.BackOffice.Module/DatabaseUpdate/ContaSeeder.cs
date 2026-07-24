using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Profilul contabil = pachet de seed (decizia 29, materializată la P1):
// profil PER BAZĂ (aliniat cu bază-per-client — 35d), selectat din appsettings
// (`ProfilContabil`); fără profil în date, fără mixaj la runtime.
public enum ProfilContabil { Bugetar = 0, Privat = 1 }

// Nucleul NEUTRU al seed-ului (P1, design §5): ancorele TipDocument, perioadele
// fiscale, repartitorii minimali și MECANISMELE de derivare (Cod-Tip = simbol +
// tăierea segmentelor, 6xx=3xx) — tot ce nu depinde de planul de conturi.
// Conținutul dependent de plan (CSV, Clasă/Tip, politici, TipTva) trăiește în
// pachetele de profil: ProfilBugetar (CPLAN — conținutul de azi, mutat,
// nemodificat) și ProfilPrivat (OMFP 1802). Politicile se definesc pe
// funcționalitate (decizia 21) — legacy/1C sunt direcție, nu canon.
public static class ContaSeeder {
    public static void Seed(IObjectSpace os, ProfilContabil profil) {
        SeedTipuriDocument(os);
        SeedPerioadeFiscale(os);
        VerificaProfil(os, profil);
        if (profil == ProfilContabil.Bugetar)
            ProfilBugetar.Seed(os);
        else
            ProfilPrivat.Seed(os);
    }

    // Profilul e per bază: o bază seed-uită cu un plan nu se re-seed-uiește cu
    // altul. Ancorele: 891.01.00 există doar în CPLAN (bugetar), 4426 doar în
    // OMFP (simboluri fără segmente).
    static void VerificaProfil(IObjectSpace os, ProfilContabil profil) {
        if (os.GetObjectsCount(typeof(Cont), null) == 0)
            return;
        var ancora = profil == ProfilContabil.Bugetar ? "891.01.00" : "4426";
        if (os.FirstOrDefault<Cont>(c => c.Simbol == ancora) == null)
            throw new InvalidOperationException(
                $"Baza are alt plan de conturi decât profilul configurat ({profil}) — profilul contabil e per bază (decizia 35d).");
    }

    // Decizia 20: nomenclatorul de tipuri oglindește clasele 1:1 — doar ancoră FK + UI.
    static void SeedTipuriDocument(IObjectSpace os) {
        (string Cod, string Denumire, string ClrType)[] tipuri = [
            ("FCT", "Factură intrare", nameof(FacturaIntrare)),
            ("FCL", "Factură ieșire", nameof(FacturaIesire)),
            ("NIR", "Notă de intrare-recepție", nameof(NIR)),
            ("BCS", "Bon de consum", nameof(BonConsum)),
            ("BTR", "Notă de transfer", nameof(NotaTransfer)),
            ("BPR", "Raport de producție", nameof(RaportProductie)),
            ("LDI", "Listă diferențe inventar", nameof(ListaDiferenteInventar)),
            ("DEC", "Decont", nameof(Decont)),
            ("PLT", "Plată", nameof(Plata)),
            ("INC", "Încasare", nameof(Incasare)),
            // Al 11-lea derivat (P2, decizia 37a): ancora e în nucleu pentru
            // AMBELE profiluri; la bugetar rămâne tip inert (fără politici), ca BPR.
            ("DSC", "Descărcare de gestiune", nameof(DescarcareGestiune)),
            // Al 12-lea derivat (FAZA 1C §5): nota contabilă — ușa de import
            // (decizia 9) și tip de culegere manuală, în AMBELE profiluri.
            ("NTC", "Notă contabilă", nameof(NotaContabila)),
            // Al 13-lea derivat (FAZA 1C §6): închiderea lunară de TVA — notă
            // contabilă GENERATĂ. Ancora e în nucleu pentru AMBELE profiluri; la
            // bugetar rămâne tip inert (fără PoliticaInchidereTva, fără
            // numerotare), ca DSC/BPR.
            ("ITV", "Închidere TVA", nameof(InchidereTva)),
            // Al 14-lea derivat (FAZA 1C §7): asamblarea (kitting n→m pe stoc).
            // Ancora e în nucleu pentru AMBELE profiluri; la bugetar rămâne tip
            // inert (fără politici), ca DSC/ITV/BPR.
            ("ASM", "Asamblare", nameof(Asamblare)),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipDocument>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipDocument>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.ClrType = t.ClrType;
            }
        }
    }

    // Pivotul gardienilor din decizia 14; anul curent de lucru, deschis.
    static void SeedPerioadeFiscale(IObjectSpace os) {
        if (os.GetObjectsCount(typeof(PerioadaFiscala), null) > 0)
            return;
        for (int luna = 1; luna <= 12; luna++) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = 2026;
            p.Luna = luna;
            p.Inchisa = false;
        }
    }

    // Minimul pentru fluxurile e2e: două gestiuni, o unitate (loc de consum),
    // comisia de inventariere — identici în ambele profiluri.
    internal static void SeedRepartitoriMinimali(IObjectSpace os) {
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG1") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG1";
            g.Denumire = "Magazia centrală";
        }
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG2") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG2";
            g.Denumire = "Magazia secundară";
        }
        var sediu = os.FirstOrDefault<UnitateInterna>(x => x.Cod == "SEDIU");
        if (sediu == null) {
            sediu = os.CreateObject<UnitateInterna>();
            sediu.Cod = "SEDIU";
            sediu.Denumire = "Sediul central";
        }
        // Locul de consum e calitate transversală (decizia 16); sediul e primul
        // consumator — |= ca să nu strice calitățile adăugate din UI.
        sediu.Calitati |= CalitateRepartitor.LocConsum;

        var comisie = os.FirstOrDefault<UnitateInterna>(x => x.Cod == "COMISIE");
        if (comisie == null) {
            comisie = os.CreateObject<UnitateInterna>();
            comisie.Cod = "COMISIE";
            comisie.Denumire = "Comisia de inventariere";
        }
        comisie.Calitati |= CalitateRepartitor.Comisie;
    }

    // Tăierea segmentelor terminale spre sintetic (302.02.00.2 → 302.02.00 →
    // 302.02) — mecanismul comun al derivărilor din simbol; la OMFP simbolurile
    // n-au segmente, deci rămâne potrivirea exactă.
    internal static Guid? ContDinSimbol(IReadOnlyDictionary<string, Guid> conturi, string simbol) {
        while (simbol.Length > 0 && !conturi.ContainsKey(simbol))
            simbol = simbol.Contains('.') ? simbol[..simbol.LastIndexOf('.')] : "";
        return simbol.Length > 0 ? conturi[simbol] : null;
    }

    // Maparea Clasă/Tip → cont e date (decizia 4); Cod-ul Tipului E un simbol de
    // cont (10 §2), deci seed-ul o derivă: potrivire exactă, apoi tăierea
    // segmentelor terminale (detalierea sub sintetic aparține Tipului — decizia 10).
    internal static void SeedContImplicitTipMaterial(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        foreach (var tip in os.GetObjectsQuery<TipMaterial>().Where(t => t.ContImplicitId == null).ToList())
            tip.ContImplicitId = ContDinSimbol(conturi, tip.Cod);
    }

    // Contarea 6xx = 3xx per Clasă/Tip (echivalentul curat al celor 18 modele
    // legacy) — rând per TipMaterial cu Natura=Stoc (debitul diferă per Tip).
    // Debitul se DERIVĂ din simbolul contului de stoc: prima cifră 3→6
    // (301→601, 302.01→602.01), cu tăierea de segmente; `exceptii` acoperă
    // mapările care nu urmează schimbarea primei cifre (profilul privat:
    // 371→607, 345→711, 381→608 — decizia 29c). Incremental: tipurile fără
    // rând primesc regulă la fiecare updater; cele cu simbol non-3xx (bonuri
    // valorice 532/409) nu primesc — rând manual la nevoie (decizia 21).
    // Folosită de BCS (consum, fără filtru de semn) și LDI (minus, SemnFiltru=-1).
    internal static void SeedContare6xxDin3xx(IObjectSpace os, TipDocument tipDoc, int? semnFiltru,
        IReadOnlyDictionary<string, string> exceptii = null) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var acoperite = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDoc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
        var tipuriStoc = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.Clasa.Natura == NaturaClasa.Stoc)
            .Select(t => new { t.ID, t.Cod }).ToList();
        foreach (var tip in tipuriStoc) {
            if (acoperite.Contains(tip.ID) || !tip.Cod.StartsWith('3'))
                continue;
            var simbol = exceptii?.GetValueOrDefault(tip.Cod) ?? ('6' + tip.Cod[1..]);
            var contDebit = ContDinSimbol(conturi, simbol);
            if (contDebit == null)
                continue;
            var regula = os.CreateObject<RegulaContare>();
            regula.TipDocument = tipDoc;
            regula.TipMaterialId = tip.ID;
            regula.SemnFiltru = semnFiltru;
            regula.SursaContDebit = SursaCont.Explicit;
            regula.ContDebitId = contDebit;
            regula.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    // Derivarea de VÂNZARE pe FacturaIesire (P2, design §6) — mecanism în nucleu
    // (decizia 29c). Pe o linie de stoc regula generică FCL ar posta creditul pe
    // contul de STOC al Tipului (371); corecția: rând RegulaContare per TipMaterial
    // cu Natura=Stoc — debit `RepartitorPrimitor` (contul clientului, fallback
    // `fallbackDebit`, ca genericul), credit Explicit = contul de VENIT derivat din
    // `mapaVenit` (371→707, 345→701, 381→708…), altfel `fallbackVenit`. Potrivirea
    // exactă pe TipMaterial bate genericul în motor (26c), deci genericul rămâne
    // pentru servicii. Incremental, ca 6xx=3xx: tipurile deja acoperite se sar, iar
    // cele fără cont de venit rezolvabil se sar (același tratament ca `contDebit`
    // negăsit acolo). Costul descărcării (607/711 = 371/345) trăiește separat pe DSC.
    internal static void SeedContareVanzare(IObjectSpace os, TipDocument tipDoc, string fallbackDebit,
        IReadOnlyDictionary<string, string> mapaVenit, string fallbackVenit) {
        var conturi = os.GetObjectsQuery<Cont>().ToDictionary(c => c.Simbol, c => c.ID);
        var acoperite = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.TipDocumentId == tipDoc.ID && r.TipMaterialId != null)
            .Select(r => r.TipMaterialId.Value).ToList();
        var tipuriStoc = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.Clasa.Natura == NaturaClasa.Stoc)
            .Select(t => new { t.ID, t.Cod }).ToList();
        foreach (var tip in tipuriStoc) {
            if (acoperite.Contains(tip.ID))
                continue;
            var simbolVenit = mapaVenit.GetValueOrDefault(tip.Cod) ?? fallbackVenit;
            var contVenit = ContDinSimbol(conturi, simbolVenit);
            if (contVenit == null)
                continue;
            var regula = os.CreateObject<RegulaContare>();
            regula.TipDocument = tipDoc;
            regula.TipMaterialId = tip.ID;
            regula.SursaContDebit = SursaCont.RepartitorPrimitor;
            regula.ContDebitId = ContDinSimbol(conturi, fallbackDebit);
            regula.SursaContCredit = SursaCont.Explicit;
            regula.ContCreditId = contVenit;
        }
    }

    // Igienă (self-healing): o regulă cu sursă Explicit și fără cont debitor nu
    // poate rezolva niciodată — reziduu al evoluțiilor de schemă. Se șterge;
    // seed-urile incrementale o recreează corect.
    internal static void StergeReguliContareStricate(IObjectSpace os) {
        var stricate = os.GetObjectsQuery<RegulaContare>()
            .Where(r => r.SursaContDebit == SursaCont.Explicit && r.ContDebitId == null).ToList();
        if (stricate.Count > 0) {
            os.Delete(stricate);
            os.CommitChanges();
        }
    }

    internal static void SeedNumerotare(IObjectSpace os, string codTip, string serie) {
        if (os.FirstOrDefault<PoliticaNumerotare>(x => x.TipDocument.Cod == codTip) == null) {
            var numerotare = os.CreateObject<PoliticaNumerotare>();
            numerotare.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == codTip);
            numerotare.Serie = serie;
            numerotare.UrmatorulNumar = 1;
        }
    }
}
