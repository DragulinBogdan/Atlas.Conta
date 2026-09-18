using System.Collections.Concurrent;
using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Politici;

// Felia 24 (F24-D6) — „de ce ar posta linia asta 371 = 401?", răspuns fără
// document.
//
// Zero reguli proprii: verdictele sunt ale funcțiilor PURE din
// `Motor/Potrivire.cs` (F24-D5), pe fapte fabricate din parametrii cererii.
// Explicația nu poate diverge de motor fiindcă e ACEEAȘI funcție — ce se adaugă
// aici e ambalajul (coduri, simboluri, fraze).
//
// Calculul cere un ObjectSpace NON-SECURED (73g/80e): pe cel filtrat explicația
// n-ar fi goală, ar fi FALSĂ. Gate-ul de citire pe tipurile citite îl ia
// controllerul, înainte, pe ușa securizată.
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
        Rezolva.Optional<TipMaterial>(securizat, cerere.TipMaterialId, "Tipul (contul/clasa)");
        Rezolva.Optional<Repartitor>(securizat, cerere.PredatorId, "Predatorul");
        Rezolva.Optional<Repartitor>(securizat, cerere.PrimitorId, "Primitorul");
        Rezolva.Optional<Partener>(securizat, cerere.PartenerId, "Partenerul");
        Rezolva.Optional<Produs>(securizat, cerere.ProdusId, "Produsul");
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
        var validare = os.GetObjectsQuery<PoliticaValidare>()
            .Where(p => p.TipDocumentId == cerere.TipDocumentId)
            .Select(p => new { p.ID, p.CereClasificatieBugetara, p.NaturaInterzisa, p.DinSeed })
            .FirstOrDefault();
        var scadenta = os.GetObjectsQuery<PoliticaScadenta>()
            .Where(p => p.TipDocumentId == cerere.TipDocumentId)
            .Select(p => new ExplicaScadentaDto { Id = p.ID, ZileDefault = p.ZileDefault, DinSeed = p.DinSeed })
            .FirstOrDefault();
        var numerotare = os.GetObjectsQuery<PoliticaNumerotare>()
            .Where(p => p.TipDocumentId == cerere.TipDocumentId)
            .Select(p => new ExplicaNumerotareDto {
                Id = p.ID,
                Serie = p.Serie,
                Format = p.Format,
                UrmatorulNumar = p.UrmatorulNumar,
                DinSeed = p.DinSeed,
            })
            .FirstOrDefault();

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
            Contare = Contare(contare, contDebit, contCredit, PostareExplicita(tip.ClrType),
                Rezerve(tip.ClrType, contare.Nivel, linie.Natura, validare?.NaturaInterzisa),
                conturi, tipuriMaterial),
            Stoc = stoc.Select(s => Stoc(s, clase)).ToArray(),
            ConcluzieStoc = stoc.Count == 0
                ? "Tipul n-are nicio regulă de stoc, pe nicio latură."
                : $"Tipul scrie în registrul de stoc pe {(stoc.Count == 1 ? "o latură" : $"{stoc.Count} laturi")}.",
            ConcluzieValidare = validare == null
                ? "Tipul n-are profil de validare."
                : $"Profilul de validare: clasificația bugetară e "
                    + $"{(validare.CereClasificatieBugetara ? "obligatorie" : "neobligatorie")}, natura "
                    + $"interzisă e {validare.NaturaInterzisa?.ToString() ?? "niciuna"}.",
            ConcluzieScadenta = scadenta == null
                ? "Tipul n-are politică de scadență."
                : $"Scadența neculeasă se completează la {scadenta.ZileDefault} zile de la data documentului.",
            ConcluzieNumerotare = numerotare == null
                ? "Tipul n-are politică de numerotare."
                : $"Numărul îl dă seria „{numerotare.Serie}”, de la {numerotare.UrmatorulNumar} — server-owned.",
            Tva = Tva(politicaTva?.ID, politicaTva?.Directie, politicaTva?.SursaContrapartida,
                politicaTva?.ContrapartidaFallbackId, politicaTva?.DinSeed ?? false, contrapartida, conturi),
            Conex = Conex(os, conex, linie),
            Implicit = Implicit(implicitTva, tipuriTva),
            Validare = validare == null ? null : new ExplicaValidareDto {
                Id = validare.ID,
                CereClasificatieBugetara = validare.CereClasificatieBugetara,
                DinSeed = validare.DinSeed,
                NaturaInterzisa = validare.NaturaInterzisa?.ToString(),
            },
            Scadenta = scadenta,
            Numerotare = numerotare,
        };
    }

    // ═══ Blocurile ═══════════════════════════════════════════════════════════

    static ExplicaContareDto Contare(PotrivireContare potrivire, RezolvareCont debit, RezolvareCont credit,
            FelPostare postare, IReadOnlyList<string> rezerve,
            IReadOnlyDictionary<Guid, (string Simbol, string Denumire)> conturi,
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
            PostareExplicita = postare != FelPostare.Regula,
            Rezerve = [.. rezerve],
        };
        bloc.Concluzie = ConcluzieContare(potrivire.Castigator != null, postare, rezerve,
            bloc.ContDebit, bloc.ContCredit);
        return bloc;
    }

    static string ConcluzieContare(bool areRegula, FelPostare postare, IReadOnlyList<string> rezerve,
            ContRezolvatDto debit, ContRezolvatDto credit) {
        var refuz = rezerve.Count == 0 ? "" : " Operarea ar fi refuzată: " + string.Join(" ", rezerve);
        if (!areRegula)
            return postare switch {
                FelPostare.Document =>
                    "Nicio regulă de contare nu se potrivește, dar tipul poartă postarea EXPLICIT pe linie: "
                        + "nota o dau conturile culese pe fiecare linie.",
                FelPostare.Linie =>
                    "Nicio regulă de contare nu se potrivește: postarea o dau conturile culese pe linie.",
                _ => "Linia nu contează pe acest tip de document: nicio regulă de contare nu se potrivește.",
            } + refuz;
        var explicita = postare switch {
            FelPostare.Document =>
                " Tipul poartă postarea EXPLICIT pe linie: conturile culese acolo bat rezolvarea de mai sus.",
            FelPostare.Linie =>
                " Regula se aplică, dar contul cules pe linie o bate punctual.",
            _ => "",
        };
        if (debit.Simbol == null || credit.Simbol == null)
            return $"Regula se potrivește, dar {(debit.Simbol == null ? "contul debitor" : "contul creditor")} "
                + "nu se rezolvă din sursa declarată și regula n-are cont explicit — pe un document real "
                + "operarea ar fi refuzată." + explicita + refuz;
        return (rezerve.Count == 0
            ? $"Se postează {debit.Simbol} = {credit.Simbol}."
            : $"Regula câștigătoare ar posta {debit.Simbol} = {credit.Simbol}, dar operarea ar fi refuzată: "
                + string.Join(" ", rezerve)) + explicita;
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

    // De unde vin conturile notei: din regulă, din conturile culese pe linie
    // fiindcă TIPUL declară postarea fără regulă (NTC, 32a extins) sau fiindcă
    // LINIA lui o poartă punctual (DEC, 32a). Explicația n-are instanță, deci
    // n-are ce conturi culese să arate — spune doar cine bate pe cine.
    enum FelPostare { Regula, Document, Linie }

    static FelPostare PostareExplicita(string clrType) {
        var clasa = ClasaDocumentului(clrType);
        if (clasa == null)
            return FelPostare.Regula;
        if (typeof(IDocumentCuPostareExplicita).IsAssignableFrom(clasa))
            return FelPostare.Document;
        var detaliu = clasa.GetCustomAttribute<TipDetaliuAttribute>(false)?.TipDetaliu;
        return detaliu != null && typeof(ILinieCuPostareExplicita).IsAssignableFrom(detaliu)
            ? FelPostare.Linie : FelPostare.Regula;
    }

    // Ce ar mai refuza operarea peste potrivire: gardul DECLARAT al clasei de
    // document (38c/64) și natura interzisă de profilul de validare (33c).
    static List<string> Rezerve(string clrType, NivelContare nivel, NaturaClasa? natura,
            NaturaClasa? naturaInterzisa) {
        var rezerve = new List<string>();
        if (ClasaDocumentului(clrType)?.GetCustomAttribute<GardContareAttribute>(false)
                is GardContareAttribute gard
                && (gard.Natura == null || gard.Natura == natura) && nivel < gard.NivelMinim)
            rezerve.Add(gard.Mesaj);
        if (naturaInterzisa != null && naturaInterzisa == natura)
            rezerve.Add($"Profilul de validare interzice liniile de natura {naturaInterzisa} pe acest tip "
                + "— operarea ar fi refuzată.");
        return rezerve;
    }

    static readonly ConcurrentDictionary<string, Type> claseDocument = new();

    static Type ClasaDocumentului(string clrType) =>
        clrType == null ? null : claseDocument.GetOrAdd(clrType, nume => typeof(Document).Assembly
            .GetTypes().FirstOrDefault(t => t.Name == nume && typeof(Document).IsAssignableFrom(t)));

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
