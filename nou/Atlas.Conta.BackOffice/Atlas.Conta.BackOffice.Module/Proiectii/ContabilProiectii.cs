using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
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
}
