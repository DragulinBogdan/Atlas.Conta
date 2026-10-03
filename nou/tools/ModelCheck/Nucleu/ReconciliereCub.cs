#nullable enable
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Gate-ul de reconciliere al fizicii pe tipurile migrate (S-D9.2): SQL brut pe
/// set, cubul persistat contra registrelor vechi, toleranță 0. Grupul unui tip
/// migrat e documentul lui ∪ recepțiile și corecțiile legate de faptul sursei.
/// </summary>
static class ReconciliereCub {
    public static int CodIesire(IReadOnlyCollection<Rand> randuri) => randuri.Count == 0 ? 0 : 1;

    /// <param name="Cheie">coordonata pe care se compară, gata de citit.</param>
    public sealed record Rand(string Litera, string Cheie, decimal Cub, decimal Registre) {
        public decimal Delta => Cub - Registre;

        public override string ToString() =>
            $"{Litera}  {Cheie}  cub={Numar(Cub)}  registre={Numar(Registre)}  Δ={Numar(Delta)}";

        static string Numar(decimal v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    }

    // Grupul folosește recepția istorică a sursei, inclusiv corecțiile NIR
    // care nu mai sunt autogenerate; politica actuală nu dovedește absorbția.
    const string Provenienta = "with receptii as (" + Module.Cub.Citiri.Receptii.LegaturiSql + """
        ), migrat as (
            select t."ID" as tip, t."Cod" as cod, t."ClrType" as clr
            from "TipuriDocument" t where t."PosteazaInCub"),
        cu_delta as (
            select distinct r.sursa as cap from receptii r
            join "Postare" p on p."DocumentId" = r.id
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            where t."Fel" = 1 and (p."Cantitate" <> 0 or p."Valoare" <> 0)),
        capTot as (
            select d."ID" as id, m.cod as grup
            from "Documente" d join migrat m on m.clr = d."ClrType"
            where d."Stare" = 1 {0}
              and not exists (select 1 from receptii r where r.id = d."ID")),
        conexTot as (
            select c."ID" as id, capTot.grup, capTot.id as cap, c."Stare" as stare
            from "Documente" c join receptii r on r.id = c."ID"
            join capTot on capTot.id = r.sursa where true {1})
        """;
    const string Grupul = Provenienta + """
        , cap as (select id, grup, id as cap from capTot),
        conex as (select id, grup, cap from conexTot),
        grup as (select id, grup, cap from cap union select id, grup, cap from conex)
        """;
    // O corecție stornată nu face incomplet cumulul nou operat; fără niciun cumul activ,
    // registrele recepției lipsesc și grupul se raportează ca incomplet.
    const string GrupulComplet = Provenienta + """
        , incomplet as (select cap from conexTot group by cap having not bool_or(stare = 1)),
        cap as (select id, grup, id as cap from capTot where id not in (select cap from incomplet)),
        conex as (select id, grup, cap from conexTot where cap not in (select cap from incomplet)),
        grup as (select id, grup, cap from cap union select id, grup, cap from conex)
        """;

    // Grupurile fără niciun cumul activ sunt raportate ca incomplete, nu ca Δ.
    static List<(string Grup, Guid Cap)> Incomplete(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{GrupulComplet}}
        select grup, cap from conexTot where cap in (select cap from incomplet) group by 1, 2 order by 1, 2
        """, set, cititor => (cititor.GetString(0), cititor.GetGuid(1)));

    public static IReadOnlyList<string> TipuriMigrate(DbContext ctx) {
        ArgumentNullException.ThrowIfNull(ctx);
        return Citeste(ctx,
            "select t.\"Cod\" from \"TipuriDocument\" t where t.\"PosteazaInCub\" "
            + "order by t.\"Cod\"",
            null,
            cititor => cititor.GetString(0));
    }

    /// <param name="documente">
    /// setul pe care se reconciliază; <c>null</c> = toată baza. Scena ModelCheck îl
    /// dă ca să nu măsoare documentele operate de alte scene cu tipul nemigrat.
    /// </param>
    /// <param name="note">
    /// ce nu s-a putut măsura și de ce: litera (f) e vacuă pe conturile atinse și de
    /// tipuri nemigrate — cubul n-are acolo decât jumătate din fapte.
    /// </param>
    public static List<Rand> Ruleaza(
            DbContext ctx, IReadOnlyCollection<Guid>? documente = null, ICollection<string>? note = null) {
        ArgumentNullException.ThrowIfNull(ctx);
        var set = documente?.Distinct().ToArray();
        var randuri = new List<Rand>();
        randuri.AddRange(Contabile(ctx, set));
        randuri.AddRange(Stocuri(ctx, set));
        randuri.AddRange(Fiscale(ctx, set));
        randuri.AddRange(Storno(ctx, set));
        randuri.AddRange(Balanta(ctx, set));
        randuri.AddRange(Numarul(ctx, set));
        randuri.AddRange(Partide(ctx, set, note));
        foreach (var grup in Incomplete(ctx, set).GroupBy(x => x.Grup))
            note?.Add($"(a)/(b): {grup.Select(x => x.Cap).Distinct().Count()} grupuri {grup.Key} cu conex "
                + "neoperat — NEINCLUSE (cubul are recepția, registrele nu; exemple: "
                + string.Join(", ", grup.Select(x => x.Cap.ToString()[..8]).Distinct().Take(5)) + ")");
        if (note is not null)
            foreach (var linie in Asm(ctx, documente).Linii()) note.Add(linie);
        if (note is not null)
            foreach (var linie in Nir(ctx, documente).Linii()) note.Add(linie);
        return randuri;
    }

    public sealed record DiagnosticAsm(long Documente, long Postari, IReadOnlyList<Rand> Diferente) {
        public IEnumerable<string> Linii() {
            yield return $"(h) ASM Operare: {Documente} documente, {Postari} postări contabile "
                + "(inclusiv contraponderile de valoare zero), excluse nominal din (a), numai în regimul dual.";
            yield return $"(h) ASM: {Diferente.Count} diferențe pe cont/latură/lună, raportate fără efect asupra exit-ului. "
                + "Probele independente pe cub rămân obligatorii; (a)–(g) nu certifică egalitatea completă cub–registre.";
            foreach (var rand in Diferente) yield return rand.ToString();
        }
    }

    public static DiagnosticAsm Asm(DbContext ctx, IReadOnlyCollection<Guid>? documente = null) {
        var set = documente?.Distinct().ToArray();
        var numere = Citeste(ctx, $$"""
            {{GrupulComplet}}
            select count(distinct p."DocumentId"), count(*)
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where g.grup = 'ASM' and t."Fel" = 1 and p."Carte" = 1
            """, set, cititor => (Documente: cititor.GetInt64(0), Postari: cititor.GetInt64(1))).Single();
        return new(numere.Documente, numere.Postari, Contabile(ctx, set, asm: true));
    }

    public sealed record DiagnosticNir(long Grupuri, IReadOnlyList<Rand> Diferente) {
        public IEnumerable<string> Linii() {
            yield return $"(h) NIR: {Grupuri} grupuri FCT + recepții/corecții cu deltă, excluse nominal din (a), numai în regimul dual.";
            yield return $"(h) NIR: {Diferente.Count} diferențe complete pe cont/latură/lună, inclusiv furnizorul; auditul (f) rămâne activ.";
            foreach (var rand in Diferente) yield return rand.ToString();
        }
    }
    public static DiagnosticNir Nir(DbContext ctx, IReadOnlyCollection<Guid>? documente = null) {
        var set = documente?.Distinct().ToArray();
        var nr = Citeste(ctx, $$"""
            {{GrupulComplet}}
            select count(distinct cap) from grup where cap in (select cap from cu_delta)
            """, set, r => r.GetInt64(0)).Single();
        return new(nr, Contabile(ctx, set, nir: true));
    }

    sealed record RandPartida(Guid Document, Guid Cont, Guid Unitate, Guid? Partener, DateOnly Deschisa);

    // (f) S-D13 — per PARTIDĂ, la ultima perioadă închisă: Σ cub (toate felurile,
    // `Data` ≤ sfârșitul perioadei) pe unitate = `PartideDeschise.Rest` al documentului
    // care a deschis-o. Adaptorul recunoaște identitatea 092;
    // nominalizarea FIFO nu face din documentul consumator un nou deschizător.
    // Se măsoară DOAR pe conturile ale căror documente sunt toate de tipuri migrate.
    static List<Rand> Partide(DbContext ctx, Guid[]? set, ICollection<string>? note) {
        var perioade = Citeste(ctx,
            "select max(\"An\" * 100 + \"Luna\") from \"PerioadeFiscale\" "
            + "where \"Inchisa\"",
            null,
            cititor => cititor.IsDBNull(0) ? (int?)null : cititor.GetInt32(0));
        if (perioade.Count == 0 || perioade[0] is not int perioada) {
            note?.Add("(f) partide: baza n-are nicio perioadă închisă — măsurătoarea e vacuă.");
            return [];
        }
        var an = perioada / 100;
        var luna = perioada % 100;
        var sfarsit = new DateOnly(an, luna, DateTime.DaysInMonth(an, luna));

        var conturi = Citeste(ctx, """
            with migrat as (
                select t."ClrType" as clr from "TipuriDocument" t
                where t."PosteazaInCub" and t."ClrType" is not null)
            select k."ID",
                   bool_and(d."ClrType" in (select clr from migrat)) as toate,
                   count(distinct d."ClrType") filter (
                       where d."ClrType" not in (select clr from migrat)) as nemigrate
            from "Conturi" k
            join "RegistruContabil" r
              on (r."ContDebitId" = k."ID" or r."ContCreditId" = k."ID")
             and r."DocumentId" is not null
            join "Documente" d on d."ID" = r."DocumentId"
            where k."UrmarestePartide"
            group by k."ID"
            """, null, cititor => (Cont: cititor.GetGuid(0), Toate: cititor.GetBoolean(1)));
        var eligibile = conturi.Where(c => c.Toate).Select(c => c.Cont).ToArray();
        if (eligibile.Length == 0) {
            note?.Add($"(f) partide: din {conturi.Count} conturi cu urmărire de partide, NICIUNUL nu e atins "
                + "exclusiv de tipuri migrate — măsurătoarea e vacuă (se exercită când tipurile "
                + "care mai postează pe ele intră în cub).");
            return [];
        }

        var deschizatori = Citeste(ctx, $$"""
            select distinct p."DocumentId", p."Cont", p."Unitate", p."Partener", d."DataInregistrare"
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join "Documente" d on d."ID" = p."DocumentId"
            where t."Fel" = 1 and p."Spatiu" = 1 and p."Unitate" is not null
              and p."Carte" = 1 and p."FelUnitate" = 2
              and p."Cont" = any(@cont) {{(set is null ? "" : "and p.\"DocumentId\" = any(@doc)")}}
            """, set, cititor => new RandPartida(
                cititor.GetGuid(0), cititor.GetGuid(1), cititor.GetGuid(2),
                cititor.IsDBNull(3) ? null : cititor.GetGuid(3),
                DateOnly.FromDateTime(cititor.GetDateTime(4))), eligibile);

        var randuri = new List<Rand>();
        var alUnitatii = new Dictionary<Guid, Guid>();
        foreach (var rand in deschizatori) {
            if (rand.Partener is not Guid partener)
                continue;
            var unitate = new N.Unitate(rand.Unitate, N.FelUnitate.Partida,
                rand.Cont, partener, null, rand.Deschisa);
            if (!Module.Cub.IdentitatiPartide.EsteProprie(unitate, rand.Document))
                continue;
            alUnitatii[rand.Unitate] = rand.Document;
        }

        var solduri = Citeste(ctx, """
            select p."Unitate",
                   sum(case when p."Latura" = 1 then p."Valoare" else -p."Valoare" end),
                   bool_or(t."Fel" = 4)
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            where p."Spatiu" = 1 and p."Unitate" is not null
              and p."Carte" = 1 and p."FelUnitate" = 2
              and p."Data" <= @sfarsit and p."Cont" = any(@cont)
            group by 1
            """, null, cititor => (Unitate: cititor.GetGuid(0), Net: cititor.GetDecimal(1), Initiala: cititor.GetBoolean(2)),
            eligibile, sfarsit);

        var alCubului = new Dictionary<Guid, decimal>();
        var initiale = new Dictionary<Guid, decimal>();
        foreach (var (unitate, net, initiala) in solduri) {
            if (net == 0m)
                continue;
            if (initiala) {
                initiale[unitate] = Math.Abs(net);
                continue;
            }
            if (!alUnitatii.TryGetValue(unitate, out var document)) {
                randuri.Add(new Rand("(f) partide",
                    $"unitatea {unitate.ToString()[..8]} are Σ ≠ 0 fără document deschizător în cub",
                    net, 0m));
                continue;
            }
            alCubului[document] = alCubului.GetValueOrDefault(document) + Math.Abs(net);
        }

        var alRegistrelor = Citeste(ctx, $$"""
            select pd."DocumentId", pd."Rest" from "PartideDeschise" pd
            join "Documente" d on d."ID" = pd."DocumentId"
            join "TipuriDocument" t on t."ClrType" = d."ClrType"
            where pd."An" = @an and pd."Luna" = @luna
              and t."PosteazaInCub"
              and exists (select 1 from "Postare" p
                          join "Tranzactie" tr on tr."ID" = p."TranzactieId"
                          where p."DocumentId" = pd."DocumentId" and tr."Fel" = 1
                            and p."Unitate" is not null and p."Cont" = any(@cont)
                            and p."Carte" = 1 and p."FelUnitate" = 2)
              {{(set is null ? "" : "and pd.\"DocumentId\" = any(@doc)")}}
            """, set, cititor => (Document: cititor.GetGuid(0), Ramas: cititor.GetDecimal(1)),
            eligibile, null, an, luna)
            .ToDictionary(x => x.Document, x => Math.Abs(x.Ramas));

        foreach (var document in alCubului.Keys.Union(alRegistrelor.Keys)) {
            var cub = alCubului.GetValueOrDefault(document);
            var registre = alRegistrelor.GetValueOrDefault(document);
            if (cub != registre)
                randuri.Add(new Rand("(f) partide",
                    $"documentul {document.ToString()[..8]} la {an}-{luna:00}", cub, registre));
        }
        // Partida deschisă de `Deschidere` nu are document: se compară pe unitate, numai pe baza întreagă.
        var initialeSnapshot = set is not null ? [] : Citeste(ctx, """
            select pd."UnitateId", pd."Rest" from "PartideDeschise" pd
            where pd."An" = @an and pd."Luna" = @luna and pd."DocumentId" is null and pd."ContId" = any(@cont)
            """, null, cititor => (Unitate: cititor.GetGuid(0), Ramas: cititor.GetDecimal(1)), eligibile, null, an, luna)
            .ToDictionary(x => x.Unitate, x => Math.Abs(x.Ramas));
        if (set is null)
            foreach (var unitate in initiale.Keys.Union(initialeSnapshot.Keys)) {
                var cub = initiale.GetValueOrDefault(unitate);
                var registre = initialeSnapshot.GetValueOrDefault(unitate);
                if (cub != registre)
                    randuri.Add(new Rand("(f) partide",
                        $"partida de deschidere {unitate.ToString()[..8]} la {an}-{luna:00}", cub, registre));
            }
        note?.Add($"(f) partide: {eligibile.Length} conturi eligibile din {conturi.Count}, "
            + $"perioada {an}-{luna:00}, {alUnitatii.Count} partide în cub, "
            + $"{alRegistrelor.Count} rânduri `PartideDeschise` comparate"
            + (set is null ? $", {initiale.Count} partide de deschidere contra {initialeSnapshot.Count} rânduri fără document." : "."));
        return randuri;
    }

    // (a) Σ Valoare per (grup, Cont, Latura, lună). Piciorul de stoc al unei
    // recepții stă pe partiția Stoc (`Spatiu = Stoc ⇔ Lot`, N-D2), deci suma e
    // peste AMBELE partiții — altfel jumătate din fiecare notă ar lipsi.
    static List<Rand> Contabile(DbContext ctx, Guid[]? set, bool asm = false, bool nir = false) => Citeste(ctx, $$"""
        {{GrupulComplet}},
        comparabil as (select * from grup where grup {{(asm ? "=" : "<>")}} 'ASM'
            and {{(nir ? "cap in (select cap from cu_delta)" : "cap not in (select cap from cu_delta)")}}),
        cub as (
            select g.grup, p."Cont" as cont, p."Latura" as latura,
                   date_trunc('month', p."Data")::date as luna, sum(p."Valoare") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join comparabil g on g.id = p."DocumentId"
            where {{(nir ? "t.\"Fel\" in (1, 2)" : "t.\"Fel\" = 1")}} and p."Carte" = 1
            group by 1, 2, 3, 4),
        reg as (
            select grup, cont, latura, luna, sum(v) as v from (
                select g.grup, r."ContDebitId" as cont, 1 as latura,
                       date_trunc('month', r."Data")::date as luna, r."Valoare" as v
                from "RegistruContabil" r join comparabil g on g.id = r."DocumentId"
                where true {{(nir ? "" : "and not r.\"Storno\"")}}
                union all
                select g.grup, r."ContCreditId", 2,
                       date_trunc('month', r."Data")::date, r."Valoare"
                from "RegistruContabil" r join comparabil g on g.id = r."DocumentId"
                where true {{(nir ? "" : "and not r.\"Storno\"")}}) x
            group by 1, 2, 3, 4)
        select coalesce(c.grup, r.grup),
               coalesce((select k."Simbol" from "Conturi" k where k."ID" = coalesce(c.cont, r.cont)),
                        coalesce(c.cont, r.cont)::text),
               case coalesce(c.latura, r.latura) when 1 then 'D' else 'C' end,
               to_char(coalesce(c.luna, r.luna), 'YYYY-MM'),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.cont = c.cont and r.latura = c.latura and r.luna = c.luna
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 4, 2, 3
        """, set, cititor => new Rand(
            nir ? "(h) NIR contabil" : asm ? "(h) ASM contabil" : "(a) contabil",
            $"{cititor.GetString(0)} {cititor.GetString(2)} {cititor.GetString(1)} {cititor.GetString(3)}",
            cititor.GetDecimal(4),
            cititor.GetDecimal(5)));

    // (b) Σ Cantitate per (grup, unitate = lot, lună) pe partiția Stoc, `Operare` ⊕ transferul
    // de stoc (T-D2): stocul se mișcă și când contul nu se schimbă. Capătul
    // virtual N-D4 poartă −q pe partiția Contabil și NU intră: în registre nu are
    // rând de stoc.
    static List<Rand> Stocuri(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{GrupulComplet}},
        cub as (
            select g.grup, p."Unitate" as lot, date_trunc('month', p."Data")::date as luna,
                   case when g.cap in (select cap from cu_delta) then 'net' when p."Cantitate" >= 0 then '+' else '-' end as semn,
                   sum(p."Cantitate") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where (t."Fel" in (1, 3) or g.cap in (select cap from cu_delta) and t."Fel" = 2) and p."Spatiu" = 2
            group by 1, 2, 3, 4),
        reg as (
            select g.grup, r."LotId" as lot, date_trunc('month', r."Data")::date as luna,
                   case when g.cap in (select cap from cu_delta) then 'net' when r."Cantitate" >= 0 then '+' else '-' end as semn,
                   sum(r."Cantitate") as v
            from "RegistruStoc" r join grup g on g.id = r."DocumentId"
            where (not r."Storno" or g.cap in (select cap from cu_delta))
            group by 1, 2, 3, 4)
        select coalesce(c.grup, r.grup),
               coalesce(left(coalesce(c.lot, r.lot)::text, 8), '(fără unitate)'),
               to_char(coalesce(c.luna, r.luna), 'YYYY-MM'),
               coalesce(c.semn, r.semn),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.lot = c.lot and r.luna = c.luna and r.semn = c.semn
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 3, 2, 4
        """, set, cititor => new Rand(
            "(b) stoc",
            $"{cititor.GetString(0)} lot={cititor.GetString(1)} {cititor.GetString(2)} "
            + $"semn={cititor.GetString(3)}",
            cititor.GetDecimal(4),
            cititor.GetDecimal(5)));

    // (c) Σ per (grup, TipTva, Sens, Rol, PerioadaDeclarare): în cub faptul fiscal
    // e ATRIBUT al postării interne (B-D8 pct. 5), în registre e rândul `RegistruTva`
    // cu cele două cifre (Bază, Taxă).
    static List<Rand> Fiscale(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        cub as (
            select g.grup, p."TipTvaId" as tip, p."SensTva" as sens, p."RolTva" as rol,
                   p."PerioadaDeclarare" as per, sum(p."Valoare") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where t."Fel" = 1 and p."TipTvaId" is not null
            group by 1, 2, 3, 4, 5),
        reg as (
            select grup, tip, sens, rol, per, sum(v) as v from (
                select g.grup, r."TipTvaId" as tip, r."Sens" as sens, 1 as rol,
                       r."PerioadaAn" * 100 + r."PerioadaLuna" as per, r."Baza" as v
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where not r."Storno"
                union all
                select g.grup, r."TipTvaId", r."Sens", 2,
                       r."PerioadaAn" * 100 + r."PerioadaLuna", r."Tva"
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where not r."Storno") x
            group by 1, 2, 3, 4, 5)
        select coalesce(c.grup, r.grup),
               coalesce((select k."Cod" from "TipuriTva" k where k."ID" = coalesce(c.tip, r.tip)),
                        coalesce(c.tip, r.tip)::text),
               case coalesce(c.sens, r.sens) when 1 then 'achizitie' else 'livrare' end,
               case coalesce(c.rol, r.rol) when 1 then 'baza' else 'taxa' end,
               coalesce(c.per, r.per),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.tip = c.tip and r.sens = c.sens and r.rol = c.rol and r.per = c.per
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 5, 2, 4
        """, set, cititor => new Rand(
            "(c) fiscal",
            $"{cititor.GetString(0)} {cititor.GetString(1)}/{cititor.GetString(2)}/{cititor.GetString(3)} "
            + $"per={cititor.GetInt32(4)}",
            cititor.GetDecimal(5),
            cititor.GetDecimal(6)));

    // (g) MAJOR-C — reperul fiscal al STORNOULUI: Σ per (grup, TipTva, Sens, Rol,
    // PerioadaDeclarare) pe postările `Fel = Storno` = rândurile `RegistruTva` cu
    // `Storno`. Corecția cu `EroareMateriala` re-ștampilează AMBELE la perioada
    // originalului, deci litera prinde divergența.
    static List<Rand> Storno(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        cub as (
            select g.grup, p."TipTvaId" as tip, p."SensTva" as sens, p."RolTva" as rol,
                   p."PerioadaDeclarare" as per, sum(p."Valoare") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where t."Fel" = 2 and p."TipTvaId" is not null
            group by 1, 2, 3, 4, 5),
        reg as (
            select grup, tip, sens, rol, per, sum(v) as v from (
                select g.grup, r."TipTvaId" as tip, r."Sens" as sens, 1 as rol,
                       r."PerioadaAn" * 100 + r."PerioadaLuna" as per, r."Baza" as v
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where r."Storno"
                union all
                select g.grup, r."TipTvaId", r."Sens", 2,
                       r."PerioadaAn" * 100 + r."PerioadaLuna", r."Tva"
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where r."Storno") x
            group by 1, 2, 3, 4, 5)
        select coalesce(c.grup, r.grup),
               coalesce((select k."Cod" from "TipuriTva" k where k."ID" = coalesce(c.tip, r.tip)),
                        coalesce(c.tip, r.tip)::text),
               case coalesce(c.sens, r.sens) when 1 then 'achizitie' else 'livrare' end,
               case coalesce(c.rol, r.rol) when 1 then 'baza' else 'taxa' end,
               coalesce(c.per, r.per),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.tip = c.tip and r.sens = c.sens and r.rol = c.rol and r.per = c.per
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 5, 2, 4
        """, set, cititor => new Rand(
            "(g) fiscal storno",
            $"{cititor.GetString(0)} {cititor.GetString(1)}/{cititor.GetString(2)}/{cititor.GetString(3)} "
            + $"per={cititor.GetInt32(4)}",
            cititor.GetDecimal(5),
            cititor.GetDecimal(6)));

    // (d) fiecare tranzacție e balansată pe fiecare `Carte` (conservarea, persistată).
    static List<Rand> Balanta(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}}
        select t."ID"::text, t."Fel", p."Carte",
               sum(case when p."Latura" = 1 then p."Valoare" else -p."Valoare" end)
        from "Postare" p
        join "Tranzactie" t on t."ID" = p."TranzactieId"
        join grup g on g.id = t."DocumentId"
        group by 1, 2, 3
        having sum(case when p."Latura" = 1 then p."Valoare" else -p."Valoare" end) <> 0
        order by 1, 3
        """, set, cititor => new Rand(
            "(d) balanță",
            $"tranzacția {cititor.GetString(0)[..8]} fel={cititor.GetInt16(1)} carte={cititor.GetInt16(2)}: "
            + "Σ D − Σ C",
            cititor.GetDecimal(3),
            0m));

    // (e) T-D2: un document operat al unui tip migrat are CEL MULT o `Operare`, CEL MULT
    // un `Transfer` de stoc (tranzacție `Transfer` cu postări în spațiul Stoc) și cel
    // puțin una din ele; și niciuna dintre cele două pe un document care nu e al unui tip
    // migrat — STAREA nu contează acolo: un document STORNAT își păstrează `Operare`,
    // cubul e append-only (S-D5).
    static List<Rand> Numarul(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        transferDeStoc as (
            select t."ID" as id, t."DocumentId" as doc
            from "Tranzactie" t
            where t."Fel" = 3
              and exists (select 1 from "Postare" p
                          where p."TranzactieId" = t."ID" and p."Spatiu" = 2)),
        peDocument as (
            select cap.id,
                   (select count(*) from "Tranzactie" t
                    where t."DocumentId" = cap.id and t."Fel" = 1) as operari,
                   (select count(*) from transferDeStoc s where s.doc = cap.id) as transferuri
            from cap),
        aleTipului as (
            select d."ID" as id from "Documente" d join migrat m on m.clr = d."ClrType"
            where d."Stare" in (1, 2)),
        straine as (
            select t."ID" as id from "Tranzactie" t
            where (t."Fel" = 1 or t."ID" in (select id from transferDeStoc))
              and (t."DocumentId" is null
                   or t."DocumentId" not in (select id from aleTipului)) {2})
        select 'document ' || id::text || ': tranzacții `Operare`', operari::numeric, 1::numeric
        from peDocument where operari > 1
        union all
        select 'document ' || id::text || ': tranzacții `Transfer` de stoc', transferuri::numeric, 1::numeric
        from peDocument where transferuri > 1
        union all
        select 'document ' || id::text || ': `Operare` sau `Transfer` de stoc', 0::numeric, 1::numeric
        from peDocument where operari + transferuri = 0
        union all
        select 'tranzacții `Operare` sau `Transfer` de stoc pe documente în afara tipurilor migrate',
               count(*)::numeric, 0::numeric
        from straine having count(*) > 0
        """, set, cititor => new Rand(
            "(e) număr", cititor.GetString(0), cititor.GetDecimal(1), cititor.GetDecimal(2)));

    static List<T> Citeste<T>(
            DbContext ctx, string sql, Guid[]? set, Func<System.Data.Common.DbDataReader, T> proiectie,
            Guid[]? conturi = null, DateOnly? sfarsit = null, int? an = null, int? luna = null) {
        var conn = ctx.Database.GetDbConnection();
        var deschisEu = conn.State != System.Data.ConnectionState.Open;
        if (deschisEu)
            conn.Open();
        try {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format(
                CultureInfo.InvariantCulture,
                sql,
                set is null ? "" : "and d.\"ID\" = any(@doc)",
                set is null ? "" : "and c.\"ID\" = any(@doc)",
                // Scena măsoară DOAR documentele ei: de la comutarea BCS/FCT prin seed,
                // restul bazei are legitim tranzacții de cub ale altor scene.
                set is null ? "" : "and t.\"DocumentId\" = any(@doc)");
            foreach (var (nume, valoare) in new (string, object?)[] {
                ("doc", set), ("cont", conturi), ("sfarsit", sfarsit), ("an", an), ("luna", luna) }) {
                if (valoare is null)
                    continue;
                var p = cmd.CreateParameter();
                p.ParameterName = nume;
                p.Value = valoare;
                cmd.Parameters.Add(p);
            }
            cmd.CommandTimeout = 0;
            using var cititor = cmd.ExecuteReader();
            var rezultat = new List<T>();
            while (cititor.Read())
                rezultat.Add(proiectie(cititor));
            return rezultat;
        }
        finally {
            if (deschisEu)
                conn.Close();
        }
    }

    public static string Raport(IReadOnlyList<Rand> randuri) {
        ArgumentNullException.ThrowIfNull(randuri);
        var text = new StringBuilder();
        text.AppendLine("Domeniu comparabil: (a) exclude nominal ASM Operare și grupurile FCT/NIR cu deltă în regimul dual; diferențele se raportează separat în (h).");
        foreach (var litera in new[] {
                "(a) contabil", "(b) stoc", "(c) fiscal", "(d) balanță", "(e) număr", "(f) partide",
                "(g) fiscal storno" }) {
            var aleLui = randuri.Where(r => r.Litera == litera).ToList();
            text.AppendLine($"{(aleLui.Count == 0 ? "OK  " : "FAIL")} {litera}: "
                + $"{aleLui.Count} rânduri cu Δ ≠ 0 (toleranță 0)");
            foreach (var rand in aleLui.Take(200))
                text.AppendLine($"       {rand}");
            if (aleLui.Count > 200)
                text.AppendLine($"       … încă {aleLui.Count - 200} rânduri");
        }
        return text.ToString().TrimEnd();
    }
}
