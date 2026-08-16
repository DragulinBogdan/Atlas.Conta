using System.Runtime.CompilerServices;
using System.Text;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// Raportarea trăiește pe REGISTRE (decizia 35d, cusătura 42c): balanța, fișa de
// cont și registrul-jurnal citesc `RegistruContabil` — append-only, plat din
// DIM-3 — și nu ating niciodată documentele polimorf. `IQueryable` pur, ca
// `DataSourceLoader` să pună sortarea/paginarea DEASUPRA, iar SQL-ul să se
// execute o singură dată, server-side.
//
// Regula de aur (42c): nimic nu se calculează în client. Soldurile de mai jos
// SUNT rezultatul — TypeScript-ul le afișează, nu le însumează.

// ═══ Atomul de raportare (R-D1) ═══
// `RegistruContabil` e o tabelă de PERECHI (cont debitor, cont creditor,
// valoare). Orice raport pe cont are deci nevoie de forma unpivotată: un rând
// produce DOI atomi — unul pe debit, unul pe credit — prin `Concat`. Precedentul
// exact e `ImperecheriProiectii.Asignari` (57c), care unpivotează laturile
// trezoreriei; tipul e NUMIT din același motiv: `Concat` cere aceeași formă pe
// ambele laturi.
//
// Atomul poartă dimensiunile LATURII LUI (cele 8 FK-uri `Debit*` pe atomul de
// debit, `Credit*` pe cel de credit) — asta face analiticul și filtrele posibile
// fără nicio coloană nouă.
//
// NU e DTO de sârmă: niciun controller nu-l întoarce și nu apare în niciun
// `ProducesResponseType`, deci nu intră în codegen. Public doar ca proiecțiile
// din alte fișiere ale modulului să-l poată consuma (o a doua definiție ar
// diverge tăcut de prima).
public sealed class AtomContabil {
    public DateOnly Data { get; set; }
    public Guid ContId { get; set; }
    // Exact una dintre ele e nenulă pe fiecare atom: unpivot-ul păstrează
    // partida dublă (Σ debit == Σ credit peste toți atomii, fără filtre).
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    public Guid? RepartitorId { get; set; }
    public Guid? MaterialId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? UnitateId { get; set; }
    public Guid? ProiectId { get; set; }
    public Guid? CentruCostId { get; set; }
}

// Rezultatul agregării, deja etichetat, înainte de netare. Există ca tip NUMIT
// fiindcă cele două moduri (R-D4) au chei de grupare diferite — deci interogări
// diferite — dar aceeași ieșire: netarea de deasupra se scrie O SINGURĂ dată.
//
// În modul sintetic `Repartitor*` ies constant null — dar ca simple COLOANE
// proiectate, niciodată ca CHEIE de join. Distincția e cea care a costat un
// refuz de la Postgres în prima variantă (o singură interogare, cu
// `RepartitorId = null` în modul sintetic): EF emite un `NULL` fără tip, pe care
// Postgres îl tipizează `text`, iar join-ul pe `Repartitor` pica cu „operator
// does not exist: text = uuid".
sealed class AgregatBalanta {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }
}

// Un rând de balanță. PLAT prin construcție (deciziile 6/7).
// `Initial*`/`Rulaj*` sunt sumele BRUTE pe latură; `Sold*` sunt cifrele NETATE
// (R-D4) — ambele perechi se expun fiindcă balanța de verificare le arată pe
// toate patru, iar netarea nu se poate reface în client (nu e aditivă).
public sealed class BalantaRand {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    // Goale în modul sintetic (`analitic=false`) — cheia e doar contul.
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal SoldInitialDebit { get; set; }
    public decimal SoldInitialCredit { get; set; }

    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }

    public decimal SoldFinalDebit { get; set; }
    public decimal SoldFinalCredit { get; set; }
}

// Contractul rândurilor care poartă un document-sursă. Codul de tip NU poate veni
// din SQL — sub TPT nu există discriminator, iar ancora `TipDocument` se caută
// după numele clasei CLR (R-D8/60b) — deci se completează în memorie, peste
// pagina deja materializată. Interfața există ca aceeași completare să se scrie
// O SINGURĂ dată pentru fișă și jurnal: două copii ar diverge tăcut (regula care
// a urcat `CoduriTip` în `ApiProiectii`).
public interface IRandCuDocument {
    Guid? DocumentId { get; }
    string DocumentTip { get; set; }
}

// Un rând de fișă de cont = un ATOM (o latură a unui rând de registru), plus
// soldul curent cumulat până la el inclusiv.
//
// ATENȚIE la identitate: `Id` e al RÂNDULUI DE REGISTRU, nu al rândului de fișă
// — un rând care are ACELAȘI cont pe ambele laturi (legitim: reclasificări
// analitice) produce, corect, două rânduri de fișă cu același `Id` și `Sens`
// diferit. Cheia de grilă e deci perechea (`Id`, `Sens`), niciodată `Id` singur.
//
public sealed class FisaContRand : IRandCuDocument {
    public Guid Id { get; set; }
    public DateOnly Data { get; set; }
    public string NumarNota { get; set; }
    // "D" / "C" — STRING, ca `Stare`/`TipStoc` pe restul proiecțiilor: contractul
    // nu depinde de ordinea membrilor unui enum, iar filtrarea din grilă vine
    // tot ca text.
    public string Sens { get; set; }
    // Exact una e nenulă pe fiecare rând (unpivot-ul R-D1).
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    // Soldul cumulat (cu semn: pozitiv = debitor) al contului până la acest rând
    // INCLUSIV, calculat de fereastra SQL peste TOATĂ perioada `<= dataEnd` —
    // deci include soldul inițial fără o a doua interogare.
    public decimal SoldCurent { get; set; }
    public Guid ContrapartidaId { get; set; }
    public string ContrapartidaSimbol { get; set; }
    // Repartitorul LATURII acestui rând (dimensiunea de debit pe atomul de debit).
    public string RepartitorDenumire { get; set; }
    // Null = rând de deschidere scris de migrare (25e/34d) — se afișează ca atare.
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string DocumentNumar { get; set; }
    public bool Storno { get; set; }
}

// Rândul BRUT al interogării SQL a fișei — tipul pe care îl materializează
// `SqlQuery<T>`, NU cel care pleacă pe sârmă. Există separat de `FisaContRand`
// din două motive care se cer amândouă:
//
//  1. **Forma impusă de EF.** `SqlQuery<T>` înregistrează tipul în model ca
//     entitate AD-HOC (`AdHocMapper.AddEntityType`), iar contextul rulează cu
//     `UseChangeTrackingProxies` (cerință XAF, decizia 24). Convenția de
//     proxy-uri se aplică deci și acestui tip și refuză, la FINALIZAREA
//     modelului, orice entitate `sealed` sau cu proprietăți ne-virtuale:
//     „Entity type … is sealed. 'UseChangeTrackingProxies' requires all entity
//     types to be public, unsealed, have virtual properties…" (probat: excepție
//     la primul apel).
//  2. **Ce iese pe sârmă dacă tipul ăsta AR FI DTO-ul.** Fiind entitate în
//     modelul XAF, instanțele materializate sunt proxy-uri care implementează și
//     interfețele XAF — iar serializatorul le vede: probat live, răspunsul
//     căpăta pe FIECARE rând `ObjectSpace`, `SecurityProcessor`, `LazyLoader`,
//     `SecuredKeyValuePairs`, `WriteProtectedKeyValuePairs`,
//     `IsDeleteProhibitedBeforeFirstObjectChange` — câmpuri care nu există în
//     schema OpenAPI (Swashbuckle citește tipul DECLARAT), deci payload umflat
//     și divergent de contractul generat.
//
// Fixul e o proiecție `Select` deasupra, spre `FisaContRand`: EF o compune în
// aceeași interogare (nimic nu se materializează în plus), tipul de pe sârmă
// redevine POCO sigilat, iar proxy-ul nu se mai construiește niciodată.
// NU se expune în niciun `ProducesResponseType` și nu intră în codegen.
public class FisaContSql {
    public virtual Guid Id { get; set; }
    public virtual DateOnly Data { get; set; }
    public virtual string NumarNota { get; set; }
    public virtual string Sens { get; set; }
    public virtual decimal Debit { get; set; }
    public virtual decimal Credit { get; set; }
    public virtual decimal SoldCurent { get; set; }
    public virtual Guid ContrapartidaId { get; set; }
    public virtual string ContrapartidaSimbol { get; set; }
    public virtual string RepartitorDenumire { get; set; }
    public virtual Guid? DocumentId { get; set; }
    public virtual string DocumentTip { get; set; }
    public virtual string DocumentNumar { get; set; }
    public virtual bool Storno { get; set; }
}

// Un rând de registru-jurnal = rândul BRUT al registrului (R-D9), nu atomul:
// listarea cronologică arată nota așa cum a fost scrisă (debit ↔ credit pe
// aceeași linie); unpivotat, fiecare notă ar apărea de două ori.
public sealed class JurnalRand : IRandCuDocument {
    public Guid Id { get; set; }
    public DateOnly Data { get; set; }
    public string NumarNota { get; set; }
    public Guid ContDebitId { get; set; }
    public string ContDebitSimbol { get; set; }
    public Guid ContCreditId { get; set; }
    public string ContCreditSimbol { get; set; }
    public decimal Valoare { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string DocumentNumar { get; set; }
    public bool Storno { get; set; }
}

public static class ContabilProiectii {

    // ── Atomii (R-D1) ───────────────────────────────────────────────────────
    public static IQueryable<AtomContabil> Atomi(IObjectSpace os) {
        // `IgnoreAutoIncludes()` EXPLICIT: `BackOfficeDbContext` pune
        // `AutoInclude()` pe cele 16 navigații de dimensiuni ale registrului
        // (motivul 41c — grilele XAF le afișează pe fiecare rând). Aici nu se
        // afișează nimic: se agregă. Fără el, orice traversare a entității ar
        // risca 16 JOIN-uri pe o tabelă de sute de mii de rânduri, de două ori
        // (o dată per latură a uniunii). Proiecția plată de mai jos ar tăia
        // include-urile oricum, dar intenția se scrie, nu se presupune —
        // altfel prima refactorizare care întoarce entitatea le readuce tăcut.
        var registru = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes();

        return registru
            .Select(r => new AtomContabil {
                Data = r.Data,
                ContId = r.ContDebitId,
                Debit = r.Valoare,
                Credit = 0m,
                RepartitorId = r.DebitRepartitorId,
                MaterialId = r.DebitMaterialId,
                CodFunctionalId = r.DebitCodFunctionalId,
                CodEconomicId = r.DebitCodEconomicId,
                SursaFinantareId = r.DebitSursaFinantareId,
                UnitateId = r.DebitUnitateId,
                ProiectId = r.DebitProiectId,
                CentruCostId = r.DebitCentruCostId
            })
            .Concat(registru
                .Select(r => new AtomContabil {
                    Data = r.Data,
                    ContId = r.ContCreditId,
                    Debit = 0m,
                    Credit = r.Valoare,
                    RepartitorId = r.CreditRepartitorId,
                    MaterialId = r.CreditMaterialId,
                    CodFunctionalId = r.CreditCodFunctionalId,
                    CodEconomicId = r.CreditCodEconomicId,
                    SursaFinantareId = r.CreditSursaFinantareId,
                    UnitateId = r.CreditUnitateId,
                    ProiectId = r.CreditProiectId,
                    CentruCostId = r.CreditCentruCostId
                }));
        // Rândurile de STORNO intră (R-D7): registrul e append-only, iar soldul
        // E suma lui algebrică — rândurile inverse se anulează singure (aceeași
        // regulă, cu același motiv, ca la `SoldStoc`). Rândurile de deschidere
        // (`DocumentId == null` — 25e/34d) intră la fel: proiecția nu atinge
        // navigația `Document`, deci nu are pe ce să pice.
    }

    // ── Balanța de verificare (R-D2/R-D3/R-D4) ──────────────────────────────
    //
    // `dataStart`/`dataEnd` NU sunt filtre peste rezultat: `dataStart` e granița
    // dinăuntrul agregării care decide ce ÎNSEAMNĂ soldul inițial (R-D2). La fel
    // dimensiunile — „balanța pe proiectul X" înseamnă „însumează doar mișcările
    // proiectului X", iar coloana Proiect nici nu mai există după `GROUP BY`.
    public static IQueryable<BalantaRand> Balanta(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, bool analitic = false,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null) {

        // Tot ce s-a întâmplat până la sfârșitul perioadei: soldul inițial e
        // partea de dinainte de `dataStart`, nu o a doua interogare.
        var atomi = Atomi(os).Where(a => a.Data <= dataEnd);

        // Filtrele, aplicate PE ATOMI, deci ÎNAINTE de agregare și fiecare pe
        // LATURA LUI: atomul de debit poartă dimensiunile de debit, cel de
        // credit pe ale lui — un rând cu proiectul X doar pe debit contribuie,
        // corect, numai la debitul contului lui. Consecință asumată: sub un
        // filtru de dimensiune Σ debit != Σ credit; partida dublă e un invariant
        // al balanței NEfiltrate.
        if (repartitorId is Guid vRep) atomi = atomi.Where(a => a.RepartitorId == vRep);
        if (materialId is Guid vMat) atomi = atomi.Where(a => a.MaterialId == vMat);
        if (codFunctionalId is Guid vCf) atomi = atomi.Where(a => a.CodFunctionalId == vCf);
        if (codEconomicId is Guid vCe) atomi = atomi.Where(a => a.CodEconomicId == vCe);
        if (sursaFinantareId is Guid vSf) atomi = atomi.Where(a => a.SursaFinantareId == vSf);
        if (unitateId is Guid vUn) atomi = atomi.Where(a => a.UnitateId == vUn);
        if (proiectId is Guid vPr) atomi = atomi.Where(a => a.ProiectId == vPr);
        if (centruCostId is Guid vCc) atomi = atomi.Where(a => a.CentruCostId == vCc);

        // O SINGURĂ agregare, cu sume condiționate (R-D3), în AMBELE moduri.
        // Forma naivă (două agregări + join) ar cere FULL OUTER JOIN —
        // inexprimabil în LINQ — și ar pierde exact conturile cu sold inițial dar
        // fără mișcare în perioadă. EF traduce în `SUM(CASE WHEN … ELSE 0 END)`:
        // o singură trecere peste tabelă.
        //
        // Cheia de grupare (R-D4): `Cont` sintetic, `Cont × Repartitor` analitic.
        // Modul NU se poate deduce, fiindcă netarea nu e aditivă — 401 cu
        // furnizorul A pe debit 100 și B pe credit 200 dă analitic `D100/C200`,
        // sintetic `C100`; ambele corecte, la niveluri diferite. Două chei =
        // două interogări, cu join-urile pe REZULTATUL agregat (42c, ca
        // `SoldStoc`): etichetele se atașează la câteva sute de rânduri, nu la
        // sutele de mii de atomi.
        var conturi = os.GetObjectsQuery<Cont>();
        IQueryable<AgregatBalanta> etichetate;

        if (analitic) {
            var agregate = atomi
                .GroupBy(a => new { a.ContId, a.RepartitorId })
                .Select(g => new {
                    g.Key.ContId,
                    g.Key.RepartitorId,
                    InitialDebit = g.Sum(a => a.Data < dataStart ? a.Debit : 0m),
                    InitialCredit = g.Sum(a => a.Data < dataStart ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Data >= dataStart ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Data >= dataStart ? a.Credit : 0m)
                });
            // LEFT JOIN pe Repartitor: dimensiunea poate lipsi de pe atom, iar
            // grupul „fără repartitor" e o linie legitimă de balanță analitică.
            etichetate =
                from a in agregate
                join c in conturi on a.ContId equals c.ID
                join r in os.GetObjectsQuery<Repartitor>()
                    on a.RepartitorId equals (Guid?)r.ID into grupRep
                from r in grupRep.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId, ContSimbol = c.Simbol, ContDenumire = c.Denumire,
                    RepartitorId = a.RepartitorId,
                    RepartitorDenumire = r == null ? null : r.Denumire,
                    InitialDebit = a.InitialDebit, InitialCredit = a.InitialCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }
        else {
            var agregate = atomi
                .GroupBy(a => a.ContId)
                .Select(g => new {
                    ContId = g.Key,
                    InitialDebit = g.Sum(a => a.Data < dataStart ? a.Debit : 0m),
                    InitialCredit = g.Sum(a => a.Data < dataStart ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Data >= dataStart ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Data >= dataStart ? a.Credit : 0m)
                });
            // Niciun join pe `Repartitor`: modul sintetic nu plătește nimic
            // pentru o cheie pe care n-o folosește. `Repartitor*` ies constant
            // null — inofensiv AICI (coloană proiectată, citită direct în
            // `Guid?`), spre deosebire de varianta unificată de dinainte, unde
            // același null intra ca CHEIE de join și Postgres îl tipiza `text`.
            etichetate =
                from a in agregate
                join c in conturi on a.ContId equals c.ID
                select new AgregatBalanta {
                    ContId = a.ContId, ContSimbol = c.Simbol, ContDenumire = c.Denumire,
                    RepartitorId = null, RepartitorDenumire = null,
                    InitialDebit = a.InitialDebit, InitialCredit = a.InitialCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }

        // Netarea, calculată în SELECT — server-side, la nivelul cheii de grupare
        // (R-D4), o singură dată pentru ambele moduri. `let` se compune în aceeași
        // interogare (verificat în SQL-ul generat: `CASE WHEN … > 0 …`).
        return from a in etichetate
               let netInitial = a.InitialDebit - a.InitialCredit
               let netFinal = a.InitialDebit - a.InitialCredit + a.RulajDebit - a.RulajCredit
               select new BalantaRand {
                   ContId = a.ContId,
                   ContSimbol = a.ContSimbol,
                   ContDenumire = a.ContDenumire,
                   RepartitorId = a.RepartitorId,
                   RepartitorDenumire = a.RepartitorDenumire,
                   InitialDebit = a.InitialDebit,
                   InitialCredit = a.InitialCredit,
                   SoldInitialDebit = netInitial > 0m ? netInitial : 0m,
                   SoldInitialCredit = netInitial < 0m ? -netInitial : 0m,
                   RulajDebit = a.RulajDebit,
                   RulajCredit = a.RulajCredit,
                   SoldFinalDebit = netFinal > 0m ? netFinal : 0m,
                   SoldFinalCredit = netFinal < 0m ? -netFinal : 0m
               };
    }

    // ── Fișa de cont (R-D6) ─────────────────────────────────────────────────
    //
    // ═══ SINGURUL loc din repo cu SQL BRUT — de ce, și ce cere în schimb ═══
    //
    // Soldul curent per rând e prin definiție `sold_inițial + Σ(rândurile de
    // dinainte)`, adică o FUNCȚIE DE FEREASTRĂ. LINQ nu are window functions, iar
    // cele trei alternative cad toate (R-D6): cumulul în client încalcă „nimic nu
    // se calculează în TS" (42c) și minte la orice paginare; agregatul corelat per
    // rând e O(n²) pe 305k rânduri; fără sold curent, fișa nu mai e fișă.
    //
    // Ce cere SQL-ul brut în schimb, și e onorat mai jos:
    //
    //  1. **Soft delete-ul NU se mai adaugă singur.** Calea LINQ trece prin
    //     `HasQueryFilter(GCRecord == 0)` pus de XAF (`EFCoreDeferredDeletionRegistration`
    //     — șters ⇒ 1). În SQL brut nu există niciun automatism, deci `"GCRecord" = 0`
    //     se scrie EXPLICIT pe registru ȘI pe fiecare tabelă alăturată. Fără el,
    //     fișa ar arăta rânduri șterse și ar diverge TĂCUT de balanță — exact
    //     defectul pe care felia asta există să-l prevină.
    //  2. **Parametrizare strictă.** Zero interpolare de valori în text:
    //     `FormattableStringFactory` construiește formatul cu `{n}`, iar EF le
    //     transformă în parametri (`SqlQuery(FormattableString)`). Singurele
    //     bucăți compuse dinamic sunt NUMELE de coloane ale filtrelor, alese
    //     dintr-un set literal închis, scris aici — niciodată din request.
    //  3. **Securitatea XAF e ocolită** (`SqlQuery` nu trece prin
    //     `SecurityQueryCompiler`). Acceptabil AICI fiindcă `RegistruContabil`
    //     n-are restricții pe rând — nimeni n-are Write pe registre (42a), iar
    //     citirea nu e filtrată. Dacă asta se schimbă vreodată, fișa e PRIMUL loc
    //     care trebuie reevaluat.
    //
    // ═══ Forma: trei niveluri, fiecare cu un motiv ═══
    //   (a) unpivot-ul (R-D1) pe UN cont: `ContDebitId = @cont` ⇒ sens „D",
    //       `ContCreditId = @cont` ⇒ sens „C". Un rând cu ACELAȘI cont pe ambele
    //       laturi produce, corect, două rânduri. Tăiat aici la `Data <= dataEnd`.
    //   (b) fereastra, peste TOT ce e `<= dataEnd` — de asta soldul curent
    //       include soldul inițial fără o a doua interogare scalară. Filtrele de
    //       dimensiune se aplică la ACEST nivel, adică ÎNAINTE de fereastră: „fișa
    //       contului pe proiectul X" are și soldul inițial al proiectului X
    //       (aceeași regulă ca la balanță — filtrul e parametru de proiecție, nu
    //       filtru de grilă).
    //   (c) `Data >= dataStart` + etichetele, în exterior — așa rândurile de
    //       dinainte de perioadă contribuie la sold fără să se afișeze.
    //
    // Join-urile de etichete sunt LEFT prin construcție: rândul aparține contului
    // nostru indiferent de starea CONTRApartidei, a repartitorului sau a
    // documentului — o etichetă lipsă nu are voie să scoată un rând din fișă (ar
    // rupe atât soldul, cât și cusătura cu balanța).
    //
    // Ordinea e FIXĂ (R-D6): `Data, Id, Sens DESC` — aceeași în fereastră și în
    // `OrderBy`-ul LINQ de deasupra, ca `LIMIT/OFFSET`-ul paginării să taie exact
    // secvența pe care s-a cumulat soldul. `Sens DESC` pune „D" înaintea lui „C"
    // (lexicografic 'D' > 'C') — singurele două valori posibile; cuplajul e
    // deliberat și se schimbă în ambele locuri odată.
    public static IQueryable<FisaContRand> FisaCont(
        IObjectSpace os, Guid contId, DateOnly dataStart, DateOnly dataEnd,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null) {

        var argumente = new List<object>();
        // Placeholder-ul poziţional: valoarea intră în lista de argumente, în text
        // ajunge doar indicele. Fiecare apel = un parametru nou (nu se refolosesc
        // indici între ramurile uniunii — asigurare ieftină contra oricărei
        // subtilităţi de substituţie din EF).
        string P(object valoare) {
            argumente.Add(valoare);
            return "{" + (argumente.Count - 1) + "}";
        }

        // Coloanele de dimensiuni, proiectate cu NUME UNIFORME pe ambele ramuri
        // ale uniunii (prefixul laturii dispare): filtrul de la nivelul (b) se
        // scrie astfel o singură dată și cade automat pe latura corectă — atomul
        // de debit poartă dimensiunile de debit, cel de credit pe ale lui
        // (riscul 7 din contract).
        var dimensiuni = new[] {
            "RepartitorId", "MaterialId", "CodFunctionalId", "CodEconomicId",
            "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId"
        };
        string Dimensiuni(string prefix) =>
            string.Concat(dimensiuni.Select(d =>
                $",\n            r.\"Dimensiuni{prefix}_{d}\" AS \"{d}\""));

        var latura = new StringBuilder();
        foreach (var (semn, contPropriu, contOpus, debit, credit) in new[] {
            ("D", "ContDebitId", "ContCreditId", "r.\"Valoare\"", "CAST(0 AS numeric(18,2))"),
            ("C", "ContCreditId", "ContDebitId", "CAST(0 AS numeric(18,2))", "r.\"Valoare\"")
        }) {
            if (latura.Length > 0)
                latura.Append("\n        UNION ALL\n");
            latura.Append($"""
                        SELECT
                            r."ID" AS "Id",
                            r."Data" AS "Data",
                            r."NumarNota" AS "NumarNota",
                            CAST('{semn}' AS text) AS "Sens",
                            {debit} AS "Debit",
                            {credit} AS "Credit",
                            r."{contOpus}" AS "ContrapartidaId",
                            r."DocumentId" AS "DocumentId",
                            r."Storno" AS "Storno"{Dimensiuni(semn == "D" ? "Debit" : "Credit")}
                        FROM "RegistruContabil" r
                        WHERE r."GCRecord" = 0 AND r."{contPropriu}" = {P(contId)} AND r."Data" <= {P(dataEnd)}
                """);
        }

        var filtre = new StringBuilder();
        void Filtru(string coloana, Guid? valoare) {
            if (valoare is Guid v)
                filtre.Append($"\n          AND a.\"{coloana}\" = {P(v)}");
        }
        Filtru("RepartitorId", repartitorId);
        Filtru("MaterialId", materialId);
        Filtru("CodFunctionalId", codFunctionalId);
        Filtru("CodEconomicId", codEconomicId);
        Filtru("SursaFinantareId", sursaFinantareId);
        Filtru("UnitateId", unitateId);
        Filtru("ProiectId", proiectId);
        Filtru("CentruCostId", centruCostId);

        // `DocumentTip` iese constant NULL din SQL și se completează în memorie
        // peste pagină (R-D8) — coloana există ca forma rezultatului să
        // corespundă exact tipului cerut de `SqlQuery<T>`. Consecință asumată,
        // documentată și în controller: filtrarea/sortarea de grilă pe
        // `DocumentTip` „vede" tot null, fiindcă tipul nu e o coloană.
        var sql = $"""
            SELECT
                f."Id" AS "Id",
                f."Data" AS "Data",
                f."NumarNota" AS "NumarNota",
                f."Sens" AS "Sens",
                f."Debit" AS "Debit",
                f."Credit" AS "Credit",
                f."SoldCurent" AS "SoldCurent",
                f."ContrapartidaId" AS "ContrapartidaId",
                cp."Simbol" AS "ContrapartidaSimbol",
                rep."Denumire" AS "RepartitorDenumire",
                f."DocumentId" AS "DocumentId",
                CAST(NULL AS text) AS "DocumentTip",
                doc."Numar" AS "DocumentNumar",
                f."Storno" AS "Storno"
            FROM (
                SELECT
                    a.*,
                    SUM(a."Debit" - a."Credit") OVER (
                        ORDER BY a."Data", a."Id", a."Sens" DESC
                        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                    ) AS "SoldCurent"
                FROM (
            {latura}
                ) a
                WHERE 1 = 1{filtre}
            ) f
            LEFT JOIN "Conturi" cp ON cp."ID" = f."ContrapartidaId" AND cp."GCRecord" = 0
            LEFT JOIN "Repartitori" rep ON rep."ID" = f."RepartitorId" AND rep."GCRecord" = 0
            LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
            WHERE f."Data" >= {P(dataStart)}
            """;

        // `DbContext` e proprietate publică pe `EFCoreObjectSpace` (surse 26.1.3,
        // EFCoreObjectSpace.cs:91) — aceeași clasă și pe calea secured (nu există
        // `SecuredEFCoreObjectSpace`; securitatea intră prin serviciile
        // contextului, tocmai de ce nota (3) de mai sus e necesară).
        var dbContext = ((EFCoreObjectSpace)os).DbContext;
        return dbContext.Database
            .SqlQuery<FisaContSql>(FormattableStringFactory.Create(sql, argumente.ToArray()))
            // Proiecția spre POCO-ul sigilat, compusă în aceeași interogare: fără
            // ea, pe sârmă ar pleca proxy-ul de entitate al lui `FisaContSql`, cu
            // membrii XAF/EF cu tot (vezi comentariul tipului).
            .Select(r => new FisaContRand {
                Id = r.Id,
                Data = r.Data,
                NumarNota = r.NumarNota,
                Sens = r.Sens,
                Debit = r.Debit,
                Credit = r.Credit,
                SoldCurent = r.SoldCurent,
                ContrapartidaId = r.ContrapartidaId,
                ContrapartidaSimbol = r.ContrapartidaSimbol,
                RepartitorDenumire = r.RepartitorDenumire,
                DocumentId = r.DocumentId,
                DocumentTip = r.DocumentTip,
                DocumentNumar = r.DocumentNumar,
                Storno = r.Storno
            })
            // `OrderBy` LINQ, nu `ORDER BY` în textul de mai sus: EF îmi împachetează
            // SQL-ul ca subinterogare, iar o ordonare din interiorul subinterogării
            // NU e garantată de Postgres când deasupra vine `LIMIT/OFFSET`. Aici
            // ordinea ajunge pe nivelul EXTERIOR, unde paginarea o respectă. Și e
            // pusă după `Select` ca `DataSourceLoader` să compună peste numele
            // proprietăților DTO-ului, nu peste ale tipului intern.
            .OrderBy(r => r.Data).ThenBy(r => r.Id).ThenByDescending(r => r.Sens);
    }

    // ── Registrul-jurnal (R-D9) ─────────────────────────────────────────────
    // Rândurile BRUTE, cronologic — nu atomii: aici nota se citește așa cum a fost
    // scrisă (debit ↔ credit pe aceeași linie), iar unpivot-ul ar dubla-o.
    // LINQ normal (n-are sold curent, deci n-are nevoie de fereastră), deci și
    // soft delete-ul, și securitatea rămân pe calea obișnuită.
    //
    // `dataStart`/`dataEnd` sunt aici filtre SIMPLE (spre deosebire de balanță și
    // fișă, unde `dataStart` definește soldul inițial) — o listare cronologică
    // n-are noțiune de „inițial", deci ambele sunt opționale.
    public static IQueryable<JurnalRand> RegistruJurnal(
        IObjectSpace os, DateOnly? dataStart = null, DateOnly? dataEnd = null) {

        // `IgnoreAutoIncludes()` din același motiv ca la atomi: `BackOfficeDbContext`
        // pune `AutoInclude()` pe cele 16 navigații de dimensiuni ale registrului
        // (41c). Aici se proiectează plat, deci nu e nevoie de niciuna.
        var randuri = os.GetObjectsQuery<RegistruContabil>().IgnoreAutoIncludes();
        if (dataStart is DateOnly ds) randuri = randuri.Where(r => r.Data >= ds);
        if (dataEnd is DateOnly de) randuri = randuri.Where(r => r.Data <= de);

        return randuri
            .Select(r => new JurnalRand {
                Id = r.ID,
                Data = r.Data,
                NumarNota = r.NumarNota,
                ContDebitId = r.ContDebitId,
                ContDebitSimbol = r.ContDebit.Simbol,
                ContCreditId = r.ContCreditId,
                ContCreditSimbol = r.ContCredit.Simbol,
                Valoare = r.Valoare,
                DocumentId = r.DocumentId,
                // Se completează în memorie peste pagină (R-D8) — vezi mai jos.
                DocumentTip = null,
                // Navigație NULLABLE (rândurile de deschidere, 25e/34d): EF
                // generează LEFT JOIN, deci iese `null`, nu o excepție (riscul 8).
                DocumentNumar = r.Document.Numar,
                Storno = r.Storno
            })
            // Ordinea implicită e cronologică; sortarea din grilă e PERMISĂ aici
            // (jurnalul n-are sold curent de rupt) și se așază deasupra —
            // `DataSourceLoader` adaugă propriul `OrderBy`, care devine cheia
            // primară a ordonării.
            .OrderBy(r => r.Data).ThenBy(r => r.Id);
    }

    // ── Codul de tip al documentului, peste pagina materializată (R-D8) ─────
    // Partajat de fișă și jurnal: ambele afișează documentul-sursă cu link, iar
    // sub TPT codul de tip nu e o coloană (ancora `TipDocument` se caută după
    // numele clasei CLR — 60b). O SINGURĂ implementare; două ar diverge tăcut.
    //
    // Se apelează DUPĂ `Incarca`, adică pe pagină (max. 500 de rânduri), nu pe
    // toată perioada — `CoduriTip` face un singur query polimorf pe mulțime
    // (varianta `GetObjectByKey` în buclă a fost măsurată la ~11s pe 335 de
    // rânduri, 60b). Rândurile de deschidere (`DocumentId == null`) se sar din
    // start: n-au document și nu trebuie să pice pe nimic.
    public static void CompleteazaTipDocument(IObjectSpace os, IEnumerable<IRandCuDocument> randuri) {
        var cuDocument = randuri?.Where(r => r?.DocumentId != null).ToList();
        if (cuDocument == null || cuDocument.Count == 0)
            return;
        var tipuri = ApiProiectii.CoduriTip(os,
            cuDocument.Select(r => r.DocumentId.Value).Distinct().ToList());
        foreach (var rand in cuDocument)
            rand.DocumentTip = tipuri.GetValueOrDefault(rand.DocumentId.Value);
    }
}
