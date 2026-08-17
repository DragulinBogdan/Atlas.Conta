using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Felia 11 (JT-D1…JT-D4): derivarea faptelor fiscale ale unui document —
// ce ajunge în `RegistruTva`, un rând per linie.
//
// DE CE serviciu-insulă (invariantul II) și nu cod inline în motor: derivarea are
// DOI consumatori — motorul, la operare, și comanda de backfill (JT-D8), care o
// aplică pe documente DEJA operate, fără să le re-opereze. O a doua implementare
// ar diverge tăcut de prima exact ca `AtomContabil` sau `TvaService.CalculeazaValori`
// dacă ar fi fost duplicate: aceeași regulă, două locuri, două adevăruri.
//
// Nota de proiectare asumată: serviciul își încarcă SINGUR `PoliticaTva` și
// dicționarul de `TipTva`, deși motorul le are deja în mână pentru rândul
// CONTABIL de TVA. E o interogare în plus per operare, pe tabele mici și
// indexate — plătită deliberat, ca serviciul să fie folosibil IDENTIC din
// backfill, unde nimeni nu i-a preîncărcat nimic. Alternativa (parametru cu
// politica preîncărcată) ar fi cuplat serviciul de ordinea fazelor motorului.
//
// Lucrează pe FK-uri + proiecții (regula 25b): nicio navigație lazy nu se atinge
// — contextul apelant (backfill pe consolă, ObjectSpace non-secured al API-ului)
// nu garantează lazy loading.
public static class RegistruTvaService {
    // Descriere, NU entitate: derivarea nu creează nimic în ObjectSpace.
    // Materializarea aparține fazei de materializare a motorului (33d) — un refuz
    // al oricărui gardian nu are voie să lase rânduri-fantomă în ObjectSpace-ul
    // apelantului.
    public readonly record struct RandTva(
        Guid DetaliuId, SensTva Sens, Guid? PartenerId, Guid TipTvaId,
        RegimTva Regim, decimal Cota, decimal Baza, decimal Tva);

    // Forma folosită de MOTOR: primește ce are deja rezolvat (tipul documentului,
    // liniile pe care tocmai le-a pregătit `PregatesteOperare`).
    public static List<RandTva> Deriva(IObjectSpace os, Document doc, TipDocument tipDoc,
        IReadOnlyCollection<DocumentDetaliu> linii) {
        var randuri = new List<RandTva>();

        // JT-D2 — criteriul de generare cere AMBELE jumătăți, fiindcă fiecare face
        // o muncă pe care cealaltă n-o poate face:
        //  • POLITICA e ce ține profilul BUGETAR inert (n-are niciun rând
        //    `PoliticaTva`, deci n-are jurnale — corect, neplătitor), deși liniile
        //    lui CHIAR poartă `TipTva` (CAP21 e implicitul FCT/FCL/DEC acolo). Un
        //    criteriu bazat doar pe linie ar fi fabricat un jurnal de TVA pentru un
        //    neplătitor. Tot ea ține în afară NIR-ul conex — care clonează
        //    `TipTvaId` ca informație (26b) dar nu postează TVA — și închiderea
        //    lunară (ITV: notă contabilă, fără politică), fiindcă închiderea mișcă
        //    4426/4427 fără să fie o OPERAȚIUNE TAXABILĂ.
        //  • TIPUL LINIEI e ce distinge o linie fiscală de una fără regim declarat.
        // Consecință asumată, măsurată nu umplută (JT-D6, verificarea 3): linia
        // FĂRĂ `TipTva` de pe un document CU politică rămâne în afara jurnalului —
        // e o gaură a datelor, nu a modelului, și un regim presupus ar fi o
        // invenție.
        var politica = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDoc.ID);
        if (politica == null)
            return randuri;

        // Sensul jurnalului NU e o a doua axă de configurare: se derivă din
        // direcția pe care profilul a declarat-o deja pentru tip.
        var sens = politica.Directie == DirectieTva.Deductibil ? SensTva.Achizitie : SensTva.Livrare;

        // Contrapartida laturii declarate de politică. `Explicit`/`TipMaterial` nu
        // sunt laturi ⇒ partener null: se RAPORTEAZĂ, nu se refuză (riscul 4 din
        // design). Un refuz ar face documentul neoperabil pentru o preocupare
        // strict de RAPORTARE, iar postarea contabilă a aceluiași rând merge
        // perfect (contrapartida se rezolvă din contul fallback).
        Guid? partenerId = politica.SursaContrapartida switch {
            SursaCont.RepartitorPredator => doc.PredatorId,
            SursaCont.RepartitorPrimitor => doc.PrimitorId,
            _ => null
        };

        var idsTipTva = linii.Where(d => d.TipTvaId != null).Select(d => d.TipTvaId.Value).Distinct().ToList();
        if (idsTipTva.Count == 0)
            return randuri;
        // JT-D3 — `Regim` și `Cota` se citesc prin PROIECȚIE și se copiază pe rând:
        // au intrat în aritmetica bazei, iar `TipTva` e nomenclator editabil (o cotă
        // corectată în 2026 n-are voie să rescrie jurnalul lui 2025). Etichetele
        // (denumire partener, cod fiscal, codurile SAF-T) rămân join la citire.
        var tipuri = os.GetObjectsQuery<TipTva>()
            .Where(t => idsTipTva.Contains(t.ID))
            .Select(t => new { t.ID, t.Regim, t.Cota })
            .ToDictionary(t => t.ID, t => (t.Regim, t.Cota));

        foreach (var d in linii) {
            if (d.TipTvaId == null)
                continue;
            var (regim, cota) = tipuri[d.TipTvaId.Value];
            var (baza, tva) = Cifre(regim, cota, d.Valoare, d.ValoareTva);
            randuri.Add(new RandTva(d.ID, sens, partenerId, d.TipTvaId.Value, regim, cota, baza, tva));
        }
        return randuri;
    }

    // Forma folosită de BACKFILL (JT-D8), pe un document deja operat: rezolvă
    // singură tipul documentului și liniile. Aceeași funcție de derivare — nu o a
    // doua cale.
    public static List<RandTva> Deriva(IObjectSpace os, Document doc) =>
        Deriva(os, doc, MotorOperare.GasesteTipDocument(os, doc), doc.Detalii);

    // JT-D4 — patru cazuri, o formulă fiecare.
    static (decimal Baza, decimal Tva) Cifre(RegimTva regim, decimal cota, decimal valoare, decimal valoareTva) {
        switch (regim) {
            case RegimTva.Capitalizat:
                // Singurul regim care CALCULEAZĂ: aici `Valoare` e BRUTĂ (TVA-ul e
                // capitalizat în cost — `TvaService`), deci baza se desface înapoi.
                // Rezultatul e TVA NEDEDUCTIBILĂ: apare în jurnal și în D300 ca
                // atare, distinsă prin `Regim`, care e chiar pe rând.
                //
                // ORDINEA contează: se rotunjește BAZA, apoi TVA-ul e DIFERENȚA —
                // așa `Baza + Tva == Valoare` EXACT, pe fiecare rând. Formula
                // simetrică (ambele rotunjite din cotă) ar fi lăsat un reziduu de
                // bani mărunți între jurnal și valoarea documentului, exact tipul de
                // diferență pe care felia există s-o facă imposibilă.
                //
                // Cota 0 pe regim capitalizat (configurație plauzibilă — riscul 2 din
                // design) cade natural: împărțire la 1 ⇒ Baza = Valoare, Tva = 0.
                var baza = Scara.RotunjesteBani(valoare / (1 + cota / 100m));
                return (baza, valoare - baza);
            case RegimTva.Normal:
            case RegimTva.TaxareInversa:
                // Cifrele sunt deja rezolvate pe linie (și deja rotunjite la bani de
                // `TvaService`): `Valoare` e NETUL, `ValoareTva` taxa. Nu se
                // recalculează din cotă — pe FCT/FCL/DEC un `ValoareTva` CULES bate
                // rotunjirea noastră (36a/48b), iar jurnalul trebuie să arate cifra
                // documentului real, nu pe a noastră.
                return (valoare, valoareTva);
            default:
                // Scutit / Neimpozabil: baza EXISTĂ (și e chiar motivul de existență
                // al registrului — nu postează nimic, dar apare legal în jurnal și în
                // D300), taxa e zero.
                return (valoare, 0m);
        }
    }
}
