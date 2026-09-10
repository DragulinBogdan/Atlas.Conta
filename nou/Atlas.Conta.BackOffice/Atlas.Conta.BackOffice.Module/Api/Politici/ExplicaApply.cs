using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.Api.Politici;

// Felia 24 (F24-D6) — „de ce ar posta linia asta 371 = 401?", răspuns fără
// document.
//
// Zero reguli proprii: verdictele sunt ale funcțiilor PURE din
// `Motor/Potrivire.cs` (F24-D5), pe fapte fabricate din parametrii cererii.
// Explicația nu poate diverge de motor fiindcă e ACEEAȘI funcție — ce se adaugă
// aici e ambalajul (coduri, simboluri, fraze).
//
// ═══ Ușa ═══
// Calculul cere un ObjectSpace NON-SECURED, ca raportul de profil (73g/80e): pe
// cel filtrat explicația n-ar fi goală, ar fi FALSĂ — o regulă invizibilă
// lipsește din candidați și verdictul devine altul. Prețul e gate-ul de citire
// pe TOATE tipurile citite, luat de controller ÎNAINTE, pe ușa securizată, plus
// vizibilitatea referințelor cererii (`CereVizibile`).
public static class ExplicaApply {
    /// <summary>
    /// Tipurile pe care explicația le CITEȘTE, deci exact cele pe care ruta cere
    /// dreptul de citire (F24-D6): configurația (`Politici.TipuriConfigurabile`)
    /// + `Partener`/`Produs` (implicitul de TVA) + `Repartitor` (conturile
    /// implicite ale laturilor, care apar ca surse rezolvate).
    /// </summary>
    public static IReadOnlyList<Type> TipuriCitite { get; } =
        PoliticiApply.TipuriCitite
            .Append(typeof(Repartitor))
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Referințele cererii, pe ușa SECURIZATĂ: ce nu vezi nu poate intra într-o
    /// explicație calculată non-secured (review advers F24). Refuz de DOMENIU
    /// (422), cu fraza unică a lui 80f — referința nu e subiectul cererii.
    /// </summary>
    public static void CereVizibile(IObjectSpace securizat, ExplicaCerere cerere) {
        Cere<TipMaterial>(securizat, cerere.TipMaterialId, "Tipul (contul/clasa)");
        Cere<Repartitor>(securizat, cerere.PredatorId, "Predatorul");
        Cere<Repartitor>(securizat, cerere.PrimitorId, "Primitorul");
        Cere<Partener>(securizat, cerere.PartenerId, "Partenerul");
        Cere<Produs>(securizat, cerere.ProdusId, "Produsul");
    }

    // `Any` pe nomenclator, nu `Rezolva.Cere` (deci nu `GetObjectByKey`): sub TPT
    // acela CASTEAZĂ rândul găsit, iar un id de altă frunză aruncă
    // `InvalidCastException` în loc să răspundă „nu e vizibil" (capcana măsurată
    // în `ImpliciteService.PartenerulDocumentului`). Fraza rămâne cea unică (80f).
    static void Cere<T>(IObjectSpace os, Guid? id, string rol) where T : BaseObject {
        if (id is Guid valoare && !os.GetObjectsQuery<T>().Any(o => o.ID == valoare))
            throw new OperareException(Refuzuri.ReferintaInvizibila(rol, valoare));
    }

    /// <summary>
    /// Explicația, pe ușa NON-SECURED, cu FK-urile deja trecute prin
    /// <see cref="CereVizibile"/>.
    /// </summary>
    public static ExplicatieDto Explica(IObjectSpace os, ExplicaCerere cerere) {
        var tip = os.GetObjectsQuery<TipDocument>()
            .Where(t => t.ID == cerere.TipDocumentId)
            .Select(t => new { t.Cod, t.Denumire, t.ClrType })
            .First();
        var material = os.GetObjectsQuery<TipMaterial>()
            .Where(t => t.ID == cerere.TipMaterialId)
            .Select(t => new { t.Cod, t.Denumire, ClasaCod = t.Clasa.Cod })
            .FirstOrDefault();

        var claseTip = Fapte.ClaseTip(os, [cerere.TipMaterialId]);
        var gasit = claseTip.TryGetValue(cerere.TipMaterialId, out var info);
        var linie = new LinieFapt(cerere.TipMaterialId, gasit ? info.ClasaId : null,
            gasit ? info.Natura : null, cerere.Semn, null, gasit ? info.ContImplicitId : null);
        var laturi = new LaturiFapt(ContImplicit(os, cerere.PredatorId), ContImplicit(os, cerere.PrimitorId));

        var contare = Potrivire.Contare(Fapte.ReguliContare(os, cerere.TipDocumentId), linie);
        var stoc = Potrivire.Stoc(Fapte.ReguliStoc(os, cerere.TipDocumentId), linie);
        var conex = Fapte.Conex(os, cerere.TipDocumentId);
        var politicaTva = os.GetObjectsQuery<PoliticaTva>()
            .Where(p => p.TipDocumentId == cerere.TipDocumentId)
            .Select(p => new { p.ID, p.Directie, p.SursaContrapartida, p.ContrapartidaFallbackId, p.DinSeed })
            .FirstOrDefault();
        var implicitTva = ImpliciteService.Explica(os, cerere.TipDocumentId, cerere.PartenerId,
            cerere.ProdusId, cerere.Data);

        var contDebit = contare.Castigator is RegulaContareFapt cd
            ? Potrivire.Cont(cd.SursaContDebit, cd.ContDebitId, linie.ContImplicitTipId, laturi)
            : new RezolvareCont(null, SursaRezolvata.Nerezolvat);
        var contCredit = contare.Castigator is RegulaContareFapt cc
            ? Potrivire.Cont(cc.SursaContCredit, cc.ContCreditId, linie.ContImplicitTipId, laturi)
            : new RezolvareCont(null, SursaRezolvata.Nerezolvat);
        var contrapartida = politicaTva == null
            ? new RezolvareCont(null, SursaRezolvata.Nerezolvat)
            : Potrivire.Cont(politicaTva.SursaContrapartida, politicaTva.ContrapartidaFallbackId, null, laturi);

        // Nomenclatoarele de afișare, în LOT: explicația e o citire, n-are voie
        // să facă un query per rând candidat.
        var conturi = Conturi(os, contare.Candidati
            .SelectMany(k => new[] { k.Regula.ContDebitId, k.Regula.ContCreditId })
            .Concat([contDebit.ContId, contCredit.ContId, contrapartida.ContId,
                politicaTva?.ContrapartidaFallbackId]));
        var tipuriMaterial = Dict(contare.Candidati.Select(k => k.Regula.TipMaterialId),
            ids => os.GetObjectsQuery<TipMaterial>().Where(t => ids.Contains(t.ID))
                .Select(t => new { t.ID, t.Cod }).ToList().ToDictionary(t => t.ID, t => t.Cod));
        var clase = Dict(stoc.SelectMany(s => s.Reguli).Select(r => r.ClasaId),
            ids => os.GetObjectsQuery<ClasaProdus>().Where(c => ids.Contains(c.ID))
                .Select(c => new { c.ID, c.Cod }).ToList().ToDictionary(c => c.ID, c => c.Cod));
        var tipuriTva = Dict(implicitTva.Candidati.Select(k => (Guid?)k.Rand.TipTvaId)
                .Concat([implicitTva.Rezultat.TipTvaId]),
            ids => os.GetObjectsQuery<TipTva>().Where(t => ids.Contains(t.ID))
                .Select(t => new { t.ID, t.Cod }).ToList().ToDictionary(t => t.ID, t => t.Cod));

        return new ExplicatieDto {
            TipDocument = tip.Cod,
            TipDocumentDenumire = tip.Denumire,
            TipMaterial = material?.Cod,
            TipMaterialDenumire = material?.Denumire,
            Clasa = material?.ClasaCod,
            Natura = linie.Natura?.ToString(),
            Semn = cerere.Semn,
            Data = cerere.Data,
            Predator = CodRepartitor(os, cerere.PredatorId),
            Primitor = CodRepartitor(os, cerere.PrimitorId),
            Partener = cerere.PartenerId is Guid idP
                ? os.GetObjectsQuery<Partener>().Where(p => p.ID == idP).Select(p => p.Cod).FirstOrDefault()
                : null,
            Produs = cerere.ProdusId is Guid idPr
                ? os.GetObjectsQuery<Produs>().Where(p => p.ID == idPr).Select(p => p.Cod).FirstOrDefault()
                : null,
            Contare = Contare(contare, contDebit, contCredit, EstePostareExplicita(tip.ClrType),
                conturi, tipuriMaterial),
            Stoc = stoc.Select(s => Stoc(s, clase)).ToArray(),
            Tva = Tva(politicaTva?.ID, politicaTva?.Directie, politicaTva?.SursaContrapartida,
                politicaTva?.ContrapartidaFallbackId, politicaTva?.DinSeed ?? false, contrapartida, conturi),
            Conex = Conex(os, conex, linie),
            Implicit = Implicit(implicitTva, tipuriTva),
            Validare = os.GetObjectsQuery<PoliticaValidare>()
                .Where(p => p.TipDocumentId == cerere.TipDocumentId)
                .Select(p => new ExplicaValidareDto {
                    Id = p.ID,
                    CereClasificatieBugetara = p.CereClasificatieBugetara,
                    DinSeed = p.DinSeed,
                    NaturaInterzisa = p.NaturaInterzisa.ToString(),
                })
                .FirstOrDefault(),
            Scadenta = os.GetObjectsQuery<PoliticaScadenta>()
                .Where(p => p.TipDocumentId == cerere.TipDocumentId)
                .Select(p => new ExplicaScadentaDto { Id = p.ID, ZileDefault = p.ZileDefault, DinSeed = p.DinSeed })
                .FirstOrDefault(),
            Numerotare = os.GetObjectsQuery<PoliticaNumerotare>()
                .Where(p => p.TipDocumentId == cerere.TipDocumentId)
                .Select(p => new ExplicaNumerotareDto {
                    Id = p.ID,
                    Serie = p.Serie,
                    Format = p.Format,
                    UrmatorulNumar = p.UrmatorulNumar,
                    DinSeed = p.DinSeed,
                })
                .FirstOrDefault(),
        };
    }

    // ═══ Blocurile ═══════════════════════════════════════════════════════════

    static ExplicaContareDto Contare(PotrivireContare potrivire, RezolvareCont debit, RezolvareCont credit,
            bool postareExplicita, IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi,
            IReadOnlyDictionary<Guid, string> tipuriMaterial) {
        var bloc = new ExplicaContareDto {
            Castigator = potrivire.Castigator is RegulaContareFapt r ? Rand(r, conturi, tipuriMaterial) : null,
            Nivel = potrivire.Nivel.ToString(),
            Candidati = potrivire.Candidati
                .Select(k => new CandidatContareDto {
                    Regula = Rand(k.Regula, conturi, tipuriMaterial),
                    Motiv = k.Motiv?.ToString(),
                })
                .ToArray(),
            ContDebit = potrivire.Castigator == null ? null : Rezolvat(debit, conturi),
            ContCredit = potrivire.Castigator == null ? null : Rezolvat(credit, conturi),
            PostareExplicita = postareExplicita,
        };
        bloc.Concluzie = ConcluzieContare(potrivire.Castigator != null, postareExplicita,
            bloc.ContDebit, bloc.ContCredit);
        return bloc;
    }

    static string ConcluzieContare(bool areRegula, bool postareExplicita, ContRezolvatDto debit,
            ContRezolvatDto credit) {
        if (!areRegula)
            return postareExplicita
                ? "Nicio regulă de contare nu se potrivește, dar tipul poartă postarea EXPLICIT pe linie: "
                    + "nota o dau conturile culese pe fiecare linie."
                : "Linia nu contează pe acest tip de document: nicio regulă de contare nu se potrivește.";
        var explicita = postareExplicita
            ? " Tipul poartă postarea EXPLICIT pe linie: conturile culese acolo bat rezolvarea de mai sus."
            : "";
        if (debit.Simbol == null || credit.Simbol == null)
            return $"Regula se potrivește, dar {(debit.Simbol == null ? "contul debitor" : "contul creditor")} "
                + "nu se rezolvă din sursa declarată și regula n-are cont explicit — pe un document real "
                + "operarea ar fi refuzată." + explicita;
        return $"Se postează {debit.Simbol} = {credit.Simbol}." + explicita;
    }

    static ExplicaStocDto Stoc(PotrivireStoc potrivire, IReadOnlyDictionary<Guid, string> clase) {
        var latura = potrivire.Latura == LaturaDocument.Predator ? "predator" : "primitor";
        var reguli = potrivire.Reguli
            .Select(r => new RegulaStocRandDto {
                Id = r.Id,
                Clasa = r.ClasaId is Guid id ? clase.GetValueOrDefault(id) : null,
                TipStoc = r.TipStoc.ToString(),
                Semn = r.Semn,
                DinSeed = r.DinSeed,
            })
            .ToArray();
        return new ExplicaStocDto {
            Latura = potrivire.Latura.ToString(),
            Nivel = potrivire.Nivel.ToString(),
            Reguli = reguli,
            Motiv = potrivire.Motiv?.ToString(),
            Concluzie = reguli.Length > 0
                ? $"Latura {latura} scrie în stoc: "
                    + string.Join(", ", reguli.Select(r => $"{r.TipStoc} ({(r.Semn > 0 ? "+" : "−")}{Math.Abs(r.Semn)})"))
                    + "."
                : potrivire.Motiv == MotivStoc.NaturaNuEsteStoc
                    ? $"Nu intră în stoc pe latura {latura}: natura clasei nu e Stoc, iar regula generică a "
                        + "laturii se aplică doar liniilor de stoc."
                    : $"Nu intră în stoc pe latura {latura}: nicio regulă de stoc.",
        };
    }

    static ExplicaTvaDto Tva(Guid? id, DirectieTva? directie, SursaCont? sursa, Guid? fallback, bool dinSeed,
            RezolvareCont contrapartida, IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi) {
        var rezolvat = directie == null ? null : Rezolvat(contrapartida, conturi);
        var sens = directie == DirectieTva.Deductibil ? "deductibil" : "colectată";
        return new ExplicaTvaDto {
            Id = id,
            Directie = directie?.ToString(),
            SursaContrapartida = sursa?.ToString(),
            ContrapartidaFallback = Simbol(conturi, fallback),
            Contrapartida = rezolvat,
            DinSeed = dinSeed,
            Concluzie = directie == null
                ? "Tipul nu postează TVA: n-are politică de TVA, deci pasul de TVA din motor nu rulează."
                : rezolvat.Simbol == null
                    ? $"Tipul postează TVA {sens}, dar contrapartida rândului de TVA nu se rezolvă din sursa "
                        + "declarată."
                    : $"Tipul postează TVA {sens}, cu contrapartida {rezolvat.Simbol}.",
        };
    }

    static ExplicaConexDto Conex(IObjectSpace os, PoliticaConexFapt? politica, LinieFapt linie) {
        if (politica is not PoliticaConexFapt p)
            return new ExplicaConexDto { Concluzie = "Tipul nu generează niciun document conex." };
        var tinta = os.GetObjectsQuery<TipDocument>()
            .Where(t => t.ID == p.TipDocumentTintaId)
            .Select(t => new { t.Cod, t.Denumire })
            .FirstOrDefault();
        var trece = Potrivire.Conex(p, linie);
        return new ExplicaConexDto {
            Id = p.Id,
            Tinta = tinta?.Cod,
            TintaDenumire = tinta?.Denumire,
            InverseazaLaturi = p.InverseazaLaturi,
            NaturaFiltru = p.NaturaFiltru?.ToString(),
            Trece = trece,
            DinSeed = os.GetObjectsQuery<PoliticaConex>().Where(x => x.ID == p.Id)
                .Select(x => x.DinSeed).FirstOrDefault(),
            Concluzie = trece
                ? $"La operare se generează {tinta?.Cod}, iar o linie ca aceasta ajunge pe el."
                : $"La operare se generează {tinta?.Cod}, dar o linie ca aceasta nu trece filtrul de natură "
                    + $"({p.NaturaFiltru}) — nu ajunge pe conex.",
        };
    }

    static ExplicaImplicitDto Implicit(PotrivireTvaImplicit potrivire,
            IReadOnlyDictionary<Guid, string> tipuriTva) {
        var cod = potrivire.Rezultat.TipTvaId is Guid id ? tipuriTva.GetValueOrDefault(id) : null;
        return new ExplicaImplicitDto {
            TipTvaId = potrivire.Rezultat.TipTvaId,
            TipTva = cod,
            Sursa = potrivire.Rezultat.Sursa.ToString(),
            Motiv = potrivire.Rezultat.Motiv,
            RandPolitica = potrivire.RandPolitica is PoliticaTvaImplicitFapt r ? Rand(r, tipuriTva) : null,
            Candidati = potrivire.Candidati
                .Select(k => new CandidatTvaImplicitDto {
                    Rand = Rand(k.Rand, tipuriTva),
                    Motiv = k.Motiv?.ToString(),
                })
                .ToArray(),
            Concluzie = cod == null
                ? "Pe o linie NOUĂ nu se propune niciun tip de TVA."
                : $"Pe o linie NOUĂ fără tip de TVA cules se propune „{cod}”.",
        };
    }

    // ═══ Ambalaje ════════════════════════════════════════════════════════════

    static RegulaContareRandDto Rand(RegulaContareFapt r,
            IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi,
            IReadOnlyDictionary<Guid, string> tipuriMaterial) =>
        new() {
            Id = r.Id,
            TipMaterial = r.TipMaterialId is Guid id ? tipuriMaterial.GetValueOrDefault(id) : null,
            NaturaFiltru = r.NaturaFiltru?.ToString(),
            SemnFiltru = r.SemnFiltru,
            PastreazaSemn = r.PastreazaSemn,
            SursaContDebit = r.SursaContDebit.ToString(),
            ContDebit = Simbol(conturi, r.ContDebitId),
            SursaContCredit = r.SursaContCredit.ToString(),
            ContCredit = Simbol(conturi, r.ContCreditId),
            DinSeed = r.DinSeed,
        };

    static RandTvaImplicitDto Rand(PoliticaTvaImplicitFapt r, IReadOnlyDictionary<Guid, string> tipuriTva) =>
        new() {
            Id = r.Id,
            ClasaFiscala = r.ClasaFiscala?.ToString(),
            ValabilDeLa = r.ValabilDeLa,
            TipTva = tipuriTva.GetValueOrDefault(r.TipTvaId),
            DinSeed = r.DinSeed,
        };

    static ContRezolvatDto Rezolvat(RezolvareCont rezolvare,
            IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi) {
        var cont = rezolvare.ContId is Guid id ? conturi.GetValueOrDefault(id) : default;
        return new ContRezolvatDto {
            Simbol = cont.Simbol,
            Denumire = cont.Denumire,
            Sursa = rezolvare.Sursa.ToString(),
        };
    }

    static string Simbol(IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi, Guid? id) =>
        id is Guid valoare ? conturi.GetValueOrDefault(valoare).Simbol : null;

    // ═══ Citiri de afișare ═══════════════════════════════════════════════════

    // Tipul de document declară postarea explicită pe linie: aceeași întrebare pe
    // care o pune motorul (`doc is IDocumentCuPostareExplicita`), pusă aici pe
    // CLASĂ — explicația n-are instanță, deci n-are ce conturi culese să arate.
    static bool EstePostareExplicita(string clrType) =>
        typeof(Document).Assembly.GetTypes()
            .Any(t => t.Name == clrType && typeof(Document).IsAssignableFrom(t)
                && typeof(IDocumentCuPostareExplicita).IsAssignableFrom(t));

    static Guid? ContImplicit(IObjectSpace os, Guid? repartitorId) =>
        repartitorId is Guid id
            ? os.GetObjectsQuery<Repartitor>().Where(r => r.ID == id).Select(r => r.ContImplicitId).FirstOrDefault()
            : null;

    static string CodRepartitor(IObjectSpace os, Guid? id) =>
        id is Guid valoare
            ? os.GetObjectsQuery<Repartitor>().Where(r => r.ID == valoare).Select(r => r.Cod).FirstOrDefault()
            : null;

    static Dictionary<Guid, (string Simbol, string Denumire)> Conturi(IObjectSpace os, IEnumerable<Guid?> ids) {
        var lista = Distincte(ids);
        return lista.Count == 0
            ? []
            : os.GetObjectsQuery<Cont>()
                .Where(c => lista.Contains(c.ID))
                .Select(c => new { c.ID, c.Simbol, c.Denumire })
                .ToList()
                .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire));
    }

    static Dictionary<Guid, string> Dict(IEnumerable<Guid?> ids,
            Func<List<Guid>, Dictionary<Guid, string>> citeste) {
        var lista = Distincte(ids);
        return lista.Count == 0 ? [] : citeste(lista);
    }

    static List<Guid> Distincte(IEnumerable<Guid?> ids) =>
        ids.Where(x => x != null).Select(x => x.Value).Distinct().ToList();
}
