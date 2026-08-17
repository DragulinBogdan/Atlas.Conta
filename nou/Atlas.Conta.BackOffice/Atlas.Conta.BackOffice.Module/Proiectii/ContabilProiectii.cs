using System.Runtime.CompilerServices;
using System.Text;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExtreme.AspNet.Data;
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

// Un nod al balanței pliate pe planul de conturi (BP-D1). Aceleași opt cifre ca
// `BalantaRand` — deliberat, ca ecranul să se citească la fel la orice nivel —
// plus poziția în arbore.
//
// `Nivel`/`ParinteId` sunt ale ARBORELUI RETURNAT, nu ale planului: un cont al
// cărui părinte e invizibil (șters logic sau tăiat de securitate) devine
// rădăcină, iar sub `nivelMaxim` nodurile de dedesubt nu se mai întorc. Ambele
// cazuri păstrează invariantul care contează: fiecare frunză contribuie la
// EXACT o rădăcină, deci Σ peste rădăcini == Σ peste balanța plată.
public sealed class BalantaPlanRand {
    public Guid ContId { get; set; }
    // Null = rădăcină în arborele ÎNTORS.
    public Guid? ParinteId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    // 0 = rădăcină. Adâncimea e măsurată pe lanțul de părinți VIZIBILI.
    public int Nivel { get; set; }
    // Calculat peste mulțimea ÎNTOARSĂ: sub `nivelMaxim`, ultimul nivel păstrat
    // iese cu `AreCopii = false`, ca ecranul să nu ofere o expandare goală.
    public bool AreCopii { get; set; }
    // Contul are rânduri de registru PE EL, nu doar prin descendenți. Legitim
    // (nimic nu interzice postarea pe un cont sumator — filtrul din lookup-ul
    // Decontului e afordanță, nu validare), dar înseamnă că cifrele nodului nu
    // sunt suma copiilor afișați: diferența e mișcarea lui proprie.
    public bool AreMiscareProprie { get; set; }

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
        //
        // ═══ Join-ul pe `Cont` e LEFT, nu INNER (review advers D4) ═══
        // Un atom NU are voie să se piardă tăcut fiindcă i-a dispărut ETICHETA.
        // `Cont` n-are `ForbidCRUD`, deci e ștergibil din XAF (soft delete,
        // `GCRecord`), iar securitatea îl poate face invizibil — cu INNER JOIN
        // linia ieșea din balanță, `Σ RulajDebit != Σ RulajCredit` în footer fără
        // nicio explicație, iar fișa ACELUIAȘI cont mergea perfect (ea joinează
        // LEFT). Cu LEFT, partida dublă rămâne întreagă și contul nerezolvat apare
        // cu simbolul gol — clientul îl marchează onest.
        // Nu e o scurgere nouă: rândurile agregate vin din `RegistruContabil`
        // citit prin ObjectSpace-ul SECURIZAT (registru invizibil ⇒ balanță
        // goală), iar denumirea contului rămâne null. E exact ce face de la
        // început `RegistruJurnal` (`r.ContDebit.Simbol` ⇒ LEFT JOIN în EF).
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
            // LEFT și pe `Cont`, din motivul de mai jos (review advers D4).
            etichetate =
                from a in agregate
                join c in conturi on a.ContId equals c.ID into grupCont
                from c in grupCont.DefaultIfEmpty()
                join r in os.GetObjectsQuery<Repartitor>()
                    on a.RepartitorId equals (Guid?)r.ID into grupRep
                from r in grupRep.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId,
                    ContSimbol = c == null ? null : c.Simbol,
                    ContDenumire = c == null ? null : c.Denumire,
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
                join c in conturi on a.ContId equals c.ID into grupCont
                from c in grupCont.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId,
                    ContSimbol = c == null ? null : c.Simbol,
                    ContDenumire = c == null ? null : c.Denumire,
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

    // Ordinea balanței, în forma consumată de `DataSourceLoader` — și, mai
    // important, o ordine TOTALĂ (review advers D2).
    //
    // Fără ea, biblioteca își pune singură ordinea („`Id`"-ul convenției EF, care
    // pe `BalantaRand` nimerește `ContId`) — cheie UNICĂ în modul sintetic, dar
    // REPETATĂ în cel analitic, unde cheia de grupare e `Cont × Repartitor`. Un
    // `ORDER BY` pe cheie ne-unică sub `LIMIT/OFFSET` n-are ordine garantată: un
    // rând poate apărea pe două pagini sau pe niciuna. (Postgres nu randomizează,
    // deci defectul nu se manifesta la o rulare oarecare — dar garanția lipsea,
    // iar asta se repară, nu se speră.)
    //
    // `ContSimbol` întâi, fiindcă e ordinea în care se citește o balanță; `ContId`
    // și `RepartitorId` sunt tiebreak-urile care o fac totală (simbolul nu e unic
    // prin schemă, iar `Cont` invizibil/șters îl lasă chiar null — D4).
    public static SortingInfo[] OrdineBalanta(bool analitic) => analitic
        ? new[] {
            OrdineLista.Crescator(nameof(BalantaRand.ContSimbol)),
            OrdineLista.Crescator(nameof(BalantaRand.ContId)),
            OrdineLista.Crescator(nameof(BalantaRand.RepartitorId))
        }
        : new[] {
            OrdineLista.Crescator(nameof(BalantaRand.ContSimbol)),
            OrdineLista.Crescator(nameof(BalantaRand.ContId))
        };

    // ── Balanța pliată pe planul de conturi (BP-D1…BP-D5) ───────────────────
    //
    // Felia 9 a lăsat rollup-ul deschis cu un motiv (R-D5): pe un grup de grilă
    // rulajele se pot însuma, dar SOLDURILE nu — netarea nu e aditivă. De aceea
    // clientul poartă totaluri de grup exclusiv pe rulaje, iar „balanța pe clase"
    // nu se poate obține grupând balanța plată. Aici e mecanismul care lipsea, și
    // toată felia stă într-o singură propoziție:
    //
    //   ═══ se cumulează cifrele BRUTE în sus pe arbore, se NETEAZĂ la fiecare
    //       nod. Niciodată invers. ═══
    //
    // Contul 4 („Terți") cu 401 pe sold creditor 200 și 411 pe sold debitor 100 dă
    // `C 100` la nivelul grupei — nu „C200 și D100 alături", și nici „C300".
    // Netarea se poate face doar din sumele brute ale nodului, care sunt aditive.
    //
    // Agregarea frunzelor NU se rescrie: e chiar `Balanta(...)` de mai sus (BP-D2).
    // O a doua agregare, oricât de asemănătoare, ar fi un al doilea adevăr care
    // diverge tăcut de primul la prima schimbare — aceeași regulă care ține un
    // singur `AtomContabil`. Pliul de deasupra e în MEMORIE, deliberat: planul e
    // mărginit prin construcție (1.679 de sintetice la bugetar, ~700 la privat),
    // recursivitatea pe arbore în SQL ar cere CTE recursiv scris de mână, iar
    // rezultatul e oricum ne-paginabil (BP-D3).
    //
    // BP-D3 — de ce NU trece prin `DataSourceLoader`: un arbore nu se pagina. Un
    // nod fără strămoșii lui în pagină e un rând orfan, iar `LIMIT/OFFSET` peste o
    // mulțime pliată taie exact strămoșii. Se întoarce deci tabloul ÎNTREG, iar
    // mărginirea e a datelor, nu a paginării: numărul de noduri ≤ numărul de
    // conturi ale planului.
    //
    // BP-D4 — modul ANALITIC nu se pliază. Cheia lui e `Cont × Repartitor`, adică
    // o a doua ierarhie: pliată pe arborele de conturi ar amesteca două axe
    // („clasa 4 pe furnizorul X" nu e un nod al planului). Dimensiunile rămân
    // FILTRE, ca la balanța plată, și se aplică tot pe atomi, înaintea agregării.
    public static List<BalantaPlanRand> BalantaPlan(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, int? nivelMaxim = null,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null) {

        // Frunzele: exact balanța sintetică, cu aceiași parametri de proiecție.
        var frunze = Balanta(os, dataStart, dataEnd, analitic: false,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId).ToList();

        // Planul, proiectat plat (fără navigații): patru coloane peste câteva mii
        // de rânduri. Citit prin ObjectSpace-ul SECURIZAT ca tot restul — un cont
        // invizibil pur și simplu lipsește de aici, iar consecința e tratată mai
        // jos ca rădăcină, nu ca rând pierdut (lecția review-ului D4 din felia 9).
        var plan = os.GetObjectsQuery<Cont>()
            .Select(c => new { c.ID, c.Simbol, c.Denumire, c.ParinteId })
            .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire, c.ParinteId));

        var noduri = new Dictionary<Guid, BalantaPlanRand>();

        BalantaPlanRand Nod(Guid contId) {
            if (noduri.TryGetValue(contId, out var existent))
                return existent;
            var gasit = plan.TryGetValue(contId, out var info);
            var nod = new BalantaPlanRand {
                ContId = contId,
                ContSimbol = gasit ? info.Simbol : null,
                ContDenumire = gasit ? info.Denumire : null,
                // Părintele contează doar dacă e el însuși vizibil: altfel nodul
                // devine rădăcină. Alternativa (păstrarea unui `ParinteId` care nu
                // se rezolvă) ar produce un rând invizibil în arborele clientului,
                // adică exact pierderea tăcută pe care D4 a interzis-o.
                ParinteId = gasit && info.ParinteId is Guid parinte && plan.ContainsKey(parinte)
                    ? parinte : null
            };
            noduri.Add(contId, nod);
            return nod;
        }

        foreach (var frunza in frunze) {
            Nod(frunza.ContId).AreMiscareProprie = true;
            // Urcarea: cifrele brute ale frunzei intră în ea însăși ȘI în fiecare
            // strămoș. `vizitate` nu e paranoia decorativă — `Cont.Parinte` e o
            // navigație editabilă din UI, iar un ciclu introdus din greșeală ar
            // transforma raportul într-o buclă infinită pe server.
            var vizitate = new HashSet<Guid>();
            for (Guid? id = frunza.ContId; id is Guid contId && vizitate.Add(contId); id = Nod(contId).ParinteId) {
                var nod = Nod(contId);
                nod.InitialDebit += frunza.InitialDebit;
                nod.InitialCredit += frunza.InitialCredit;
                nod.RulajDebit += frunza.RulajDebit;
                nod.RulajCredit += frunza.RulajCredit;
            }
        }

        // Adâncimea, pe lanțul de părinți DIN MULȚIMEA CONSTRUITĂ (un `ParinteId`
        // rămas nerezolvat ar opri urcarea, dar nu poate exista: `Nod` l-a creat).
        foreach (var nod in noduri.Values) {
            var nivel = 0;
            var vizitate = new HashSet<Guid> { nod.ContId };
            var parinte = nod.ParinteId;
            while (parinte is Guid pid && noduri.TryGetValue(pid, out var sus) && vizitate.Add(pid)) {
                nivel++;
                parinte = sus.ParinteId;
            }
            nod.Nivel = nivel;
        }

        // Netarea — la NIVELUL NODULUI, din sumele lui brute cumulate (BP-D1).
        foreach (var nod in noduri.Values) {
            var netInitial = nod.InitialDebit - nod.InitialCredit;
            var netFinal = netInitial + nod.RulajDebit - nod.RulajCredit;
            nod.SoldInitialDebit = netInitial > 0m ? netInitial : 0m;
            nod.SoldInitialCredit = netInitial < 0m ? -netInitial : 0m;
            nod.SoldFinalDebit = netFinal > 0m ? netFinal : 0m;
            nod.SoldFinalCredit = netFinal < 0m ? -netFinal : 0m;
        }

        // Trunchierea pe adâncime (BP-D5: „balanță pe clase" = `nivelMaxim=1`).
        // E gratuită fiindcă cifrele descendenților sunt DEJA în strămoși — se
        // taie rânduri, nu sume. Arborele rămâne închis în sus: părintele are
        // întotdeauna `Nivel` mai mic, deci nu poate fi tăiat înaintea copilului.
        var pastrate = nivelMaxim is int max
            ? noduri.Values.Where(n => n.Nivel < max).ToList()
            : noduri.Values.ToList();
        var ramase = pastrate.Select(n => n.ContId).ToHashSet();

        // `AreCopii` se calculează peste mulțimea PĂSTRATĂ, nu peste plan: sub
        // trunchiere, ultimul nivel afișat nu mai are ce expanda.
        foreach (var nod in pastrate)
            nod.AreCopii = false;
        foreach (var nod in pastrate)
            if (nod.ParinteId is Guid pid && ramase.Contains(pid))
                noduri[pid].AreCopii = true;

        // Ordine totală și deterministă: simbolul e ordinea în care se citește o
        // balanță; `ContId` e tiebreak-ul (simbolul nu e unic prin schemă, iar
        // contul fără etichetă îl are chiar null).
        return pastrate
            .OrderBy(n => n.ContSimbol ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(n => n.ContId)
            .ToList();
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
    //     `SecurityQueryCompiler`) — deci calea asta NU are voie să ruleze pe
    //     încredere. Premisa scrisă aici la prima versiune („citirea nu e
    //     filtrată") era pur și simplu FALSĂ: probat cu token pe un utilizator
    //     fără nicio permisiune, balanța și `/api/odata/Cont` întorceau gol, iar
    //     fișa întorcea registrul complet — cu `ContrapartidaId` pe fiecare rând,
    //     adică toată cartea mare, plimbându-te din contrapartidă în
    //     contrapartidă.
    //     Fixul e FAIL CLOSED, în două bucăți, ambele obligatorii și amândouă în
    //     controller (`FisaContController`): contul se rezolvă prin ObjectSpace-ul
    //     SECURIZAT (invizibil ⇒ 404), iar echivalența celor două căi se
    //     DOVEDEȘTE per cerere prin `CaleaBrutaEchivalenta` de mai jos (diferă ⇒
    //     403). Proiecția rămâne deci utilizabilă doar când s-a arătat că vede
    //     exact ce ar vedea calea securizată.
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
    // Ordinea e FIXĂ (R-D6): `Data, Id, Sens DESC` — aceeași în fereastră, în
    // `OrderBy`-ul LINQ de deasupra ȘI în `OrdineFisa()` (forma pe care o consumă
    // `DataSourceLoader`), ca `LIMIT/OFFSET`-ul paginării să taie exact secvența pe
    // care s-a cumulat soldul. `Sens DESC` pune „D" înaintea lui „C" (lexicografic
    // 'D' > 'C') — singurele două valori posibile; cuplajul e deliberat și se
    // schimbă în TOATE cele trei locuri odată.
    //
    // De ce trei locuri și nu unul: `OrderBy`-ul LINQ serveşte consumatorii DIRECȚI
    // (`.ToList()`), dar `DataSourceLoader` îl ȘTERGE — vezi demonstrația din
    // `OrdineLista`. Sub paginare, singurul care ajunge la Postgres e cel declarat
    // prin `OrdineFisa()`.
    //
    // `repartitorNul` (D3): a TREIA valoare a filtrului pe repartitor. `Guid?`
    // poate exprima doar „un repartitor anume" și „fără filtru" — dar rândul
    // „fără repartitor" al balanței analitice e o cheie de grupare LEGITIMĂ
    // (LEFT JOIN-ul o păstrează deliberat, iar pe baza de import e chiar
    // majoritară: deschiderea s-a scris fără dimensiuni — 47c — și anul 2025 a
    // trecut fără dimensiuni culese pe linie — DIM-2). Fără santinelă,
    // drill-down-ul pe acel rând deschidea fișa NEfiltrată — cu ultimul sold
    // curent egal cu soldul SINTETIC, nu cu al rândului clicat.
    public static IQueryable<FisaContRand> FisaCont(
        IObjectSpace os, Guid contId, DateOnly dataStart, DateOnly dataEnd,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, bool repartitorNul = false) {

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
        // Santinela D3, la ACELAȘI nivel ca celelalte filtre (deci înaintea
        // ferestrei): „exact rândurile fără repartitor pe latura lor". Cu
        // `repartitorId` dat simultan ar fi o contradicție — controllerul o
        // refuză cu 400, ca cererea să nu întoarcă tăcut o fișă goală.
        if (repartitorNul)
            filtre.Append("\n          AND a.\"RepartitorId\" IS NULL");
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
            //
            // ATENȚIE: ordinea asta e a consumatorilor DIRECȚI. `DataSourceLoader` o
            // ȘTERGE (`OrderBy` peste `OrderBy` = `ApplyOrdering` în EF Core) și pune
            // în loc `Id`-ul singur — de asta calea de API declară `OrdineFisa()`.
            .OrderBy(r => r.Data).ThenBy(r => r.Id).ThenByDescending(r => r.Sens);
    }

    // ═══ Gate-ul fail-closed al căii SQL brut (review advers D1) ═══
    //
    // Premisa „registrul nu e filtrat pe rând" NU se re-afirmă, se DOVEDEȘTE — și
    // se dovedește PER CERERE, fiindcă e o proprietate a utilizatorului curent, nu
    // a codului: numără rândurile de registru ale contului prin
    // `GetObjectsQuery<RegistruContabil>()` (deci prin filtrele de securitate XAF)
    // și le compară cu ce vede SQL-ul brut pe EXACT aceeași mulțime.
    //
    // De ce o simplă numărătoare e o dovadă, nu o euristică: interogarea securizată
    // e literalmente `predicatul de mai jos ∧ criteriile de securitate`, deci poate
    // doar SCOATE rânduri, niciodată adăuga. Egalitate ⟺ criteriile n-au scos
    // nimic ⟺ calea brută vede exact ce ar vedea calea securizată. Orice altă
    // relație (inclusiv „securizatul vede mai mult", adică o divergență de
    // predicat, ex. semantica lui `GCRecord`) e o NEpotrivire și se refuză.
    //
    // Deliberat NU se compară mulțimea filtrată pe dimensiuni: cea NEfiltrată e
    // supermulțimea ei, deci verificarea e mai strictă, nu mai laxă.
    //
    // Alternativa ieftină — un API XAF „utilizatorul are citire NERESTRICȚIONATĂ pe
    // tipul T" — a fost căutată și respinsă: `IRequestSecurityStrategy.CanRead` (și
    // `IsGranted` cu `PermissionRequest`) răspund la „are permisiune", nu la „e
    // lipsită de criterii pe rând", iar un `true` de acolo nu exclude un
    // `ObjectPermission` cu criteriu. Regula fiind fail-closed, „nu pot determina
    // cu certitudine" înseamnă refuz — deci se plătește numărătoarea (ieftină:
    // indecșii pe `ContDebitId`/`ContCreditId` există și sunt folosiți).
    //
    // ZONA ASTA NU E ACOPERITĂ DE ModelCheck și nici nu poate fi: unealta rulează
    // pe un `EFCoreObjectSpaceProvider` standalone, NEsecurizat — acolo cele două
    // căi sunt egale prin construcție, deci un check ar trece și cu gate-ul șters.
    // Proba lui e HTTP, cu token, pe doi utilizatori cu drepturi diferite (vezi
    // contractul feliei).
    public static bool CaleaBrutaEchivalenta(IObjectSpace os, Guid contId, DateOnly dataEnd) {
        var securizat = os.GetObjectsQuery<RegistruContabil>()
            .Count(r => (r.ContDebitId == contId || r.ContCreditId == contId) && r.Data <= dataEnd);

        // `AS "Value"`: forma pe care `SqlQuery<T>` o cere pentru un tip SCALAR.
        // `long`, nu `int`: `COUNT(*)` e `bigint` pe Postgres.
        // Parametrizat ca restul fișei — `contId` intră de două ori ca două
        // argumente distincte, din aceeași prudență ca acolo.
        const string sql = """
            SELECT COUNT(*) AS "Value"
            FROM "RegistruContabil" r
            WHERE r."GCRecord" = 0
              AND (r."ContDebitId" = {0} OR r."ContCreditId" = {1})
              AND r."Data" <= {2}
            """;
        var brut = ((EFCoreObjectSpace)os).DbContext.Database
            .SqlQuery<long>(FormattableStringFactory.Create(sql, contId, contId, dataEnd))
            .Single();

        return securizat == brut;
    }

    // Ordinea fișei în forma pe care o consumă `DataSourceLoader` — aceeași ca
    // fereastra SQL și ca `OrderBy`-ul de mai sus (R-D6). Metodă, nu câmp: fiecare
    // apelant primește propriul array, ca nimeni să nu poată muta ordinea altuia.
    public static SortingInfo[] OrdineFisa() => new[] {
        OrdineLista.Crescator(nameof(FisaContRand.Data)),
        OrdineLista.Crescator(nameof(FisaContRand.Id)),
        OrdineLista.Descrescator(nameof(FisaContRand.Sens))
    };

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
            // primară a ordonării. Tocmai fiindcă îl ADAUGĂ ca `OrderBy` (nu
            // `ThenBy`), ordinea de mai jos NU supraviețuiește încărcării paginate:
            // fără `sort=` de la client, biblioteca ar ordona după `Id` singur, adică
            // după ordinea de INSERARE, nu după `Data`. Calea de API declară deci
            // `OrdineJurnal()` — vezi `OrdineLista`.
            .OrderBy(r => r.Data).ThenBy(r => r.Id);
    }

    // Ordinea implicită a jurnalului (cronologică), în forma consumată de
    // `DataSourceLoader`. Spre deosebire de fișă, aici e doar un DEFAULT: dacă
    // cererea poartă `sort=`, sortarea clientului câștigă (R-D9).
    public static SortingInfo[] OrdineJurnal() => new[] {
        OrdineLista.Crescator(nameof(JurnalRand.Data)),
        OrdineLista.Crescator(nameof(JurnalRand.Id))
    };

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
