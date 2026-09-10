using Atlas.Conta.BackOffice.Module.BusinessObjects;

namespace Atlas.Conta.BackOffice.Module.Motor;

// POTRIVIREA POLITICILOR PE O LINIE — funcții PURE pe fapte plate (F24-D5).
//
// Faptele sunt Id-uri și valori: nicio entitate, niciun `IObjectSpace`, deci
// aceeași funcție răspunde motorului, gardienilor frunzelor și explicației.
// Maparea entitate → fapt are o singură ortografie, în `Fapte`.
//
// `Nivel` e răspunsul de care au nevoie gardienii: un gard care oglindește o
// potrivire oglindește TOATE axele ei (64), deci întreabă potrivirea, nu o
// rescrie.

public readonly record struct RegulaContareFapt(Guid Id, Guid? TipMaterialId, NaturaClasa? NaturaFiltru,
    int? SemnFiltru, bool PastreazaSemn, SursaCont SursaContDebit, Guid? ContDebitId,
    SursaCont SursaContCredit, Guid? ContCreditId, bool DinSeed,
    Dimensiuni Comun, Dimensiuni OverrideDebit, Dimensiuni OverrideCredit);

public readonly record struct RegulaStocFapt(Guid Id, LaturaDocument Latura, Guid? ClasaId,
    TipStoc TipStoc, int Semn, bool DinSeed);

// `Natura`/`ClasaId` null = Tipul liniei nu e în nomenclator; nicio treaptă de
// natură și nicio regulă specifică pe clasă nu potrivesc.
public readonly record struct LinieFapt(Guid TipMaterialId, Guid? ClasaId, NaturaClasa? Natura,
    int Semn, Guid? LotId, Guid? ContImplicitTipId);

public readonly record struct LaturiFapt(Guid? ContImplicitPredatorId, Guid? ContImplicitPrimitorId);

public readonly record struct PoliticaConexFapt(Guid Id, Guid TipDocumentTintaId,
    bool InverseazaLaturi, NaturaClasa? NaturaFiltru);

public readonly record struct PoliticaTvaImplicitFapt(Guid Id, ClasaFiscalaPartener? ClasaFiscala,
    DateOnly? ValabilDeLa, Guid TipTvaId, bool DinSeed);

public readonly record struct TipTvaFapt(Guid Id, string Cod, RegimTva Regim, bool Activ);

// `Motiv` null = câștigătorul.
public readonly record struct CandidatContare(RegulaContareFapt Regula, MotivEliminare? Motiv);

public readonly record struct CandidatTvaImplicit(PoliticaTvaImplicitFapt Rand, MotivEliminare? Motiv);

public sealed record PotrivireContare(RegulaContareFapt? Castigator, NivelContare Nivel,
    IReadOnlyList<CandidatContare> Candidati);

public sealed record PotrivireStoc(LaturaDocument Latura, NivelStoc Nivel,
    IReadOnlyList<RegulaStocFapt> Reguli, MotivStoc? Motiv);

public readonly record struct RezolvareCont(Guid? ContId, SursaRezolvata Sursa);

public sealed record PotrivireTvaImplicit(ImpliciteService.RezultatImplicit Rezultat,
    PoliticaTvaImplicitFapt? RandPolitica, IReadOnlyList<CandidatTvaImplicit> Candidati);

public static class Potrivire {
    /// <summary>
    /// Regula de contare a liniei: filtrul de semn scoate rândul din joc la TOATE
    /// nivelurile, apoi TipMaterial exact bate `NaturaFiltru`, care bate regula
    /// generică (26c). Câștigătorul e PRIMUL de la cel mai înalt nivel, în
    /// ordinea listei primite.
    /// </summary>
    public static PotrivireContare Contare(IReadOnlyList<RegulaContareFapt> reguli, LinieFapt linie) {
        var niveluri = new NivelContare[reguli.Count];
        var motive = new MotivEliminare?[reguli.Count];
        var castigator = -1;
        for (var i = 0; i < reguli.Count; i++) {
            var r = reguli[i];
            if (r.SemnFiltru != null && r.SemnFiltru != linie.Semn) {
                motive[i] = MotivEliminare.SemnNepotrivit;
                continue;
            }
            NivelContare nivelul;
            if (r.TipMaterialId == linie.TipMaterialId)
                nivelul = NivelContare.TipMaterialExact;
            else if (r.TipMaterialId != null) {
                motive[i] = MotivEliminare.TipMaterialDiferit;
                continue;
            }
            else if (r.NaturaFiltru == null)
                nivelul = NivelContare.Generic;
            else if (r.NaturaFiltru == linie.Natura)
                nivelul = NivelContare.Natura;
            else {
                motive[i] = MotivEliminare.NaturaDiferita;
                continue;
            }
            niveluri[i] = nivelul;
            if (castigator < 0 || nivelul > niveluri[castigator])
                castigator = i;
        }

        var nivel = castigator < 0 ? NivelContare.Niciuna : niveluri[castigator];
        var candidati = new List<CandidatContare>(reguli.Count);
        for (var i = 0; i < reguli.Count; i++) {
            var motiv = motive[i];
            if (motiv == null && i != castigator)
                motiv = niveluri[i] < nivel ? MotivEliminare.NivelMaiSlab : MotivEliminare.Dublura;
            candidati.Add(new CandidatContare(reguli[i], motiv));
        }
        return new PotrivireContare(castigator < 0 ? null : reguli[castigator], nivel, candidati);
    }

    /// <summary>
    /// Regulile de stoc ale liniei, un element per latură prezentă în reguli:
    /// regulile specifice pe Clasa liniei bat regula generică, iar căderea pe
    /// generic e păzită de `Natura == Stoc`. Toate regulile specifice ale unei
    /// laturi trag, nu doar prima.
    /// </summary>
    public static IReadOnlyList<PotrivireStoc> Stoc(IReadOnlyList<RegulaStocFapt> reguli, LinieFapt linie) {
        var rezultat = new List<PotrivireStoc>();
        foreach (var latura in reguli.GroupBy(r => r.Latura)) {
            var specifice = latura.Where(r => r.ClasaId != null && r.ClasaId == linie.ClasaId).ToList();
            if (specifice.Count > 0) {
                rezultat.Add(new PotrivireStoc(latura.Key, NivelStoc.ClasaExacta, specifice, null));
                continue;
            }
            var generice = latura.Where(r => r.ClasaId == null).ToList();
            rezultat.Add(generice.Count > 0 && linie.Natura == NaturaClasa.Stoc
                ? new PotrivireStoc(latura.Key, NivelStoc.Generic, generice, null)
                : new PotrivireStoc(latura.Key, NivelStoc.Niciuna, [],
                    generice.Count > 0 ? MotivStoc.NaturaNuEsteStoc : MotivStoc.FaraRegula));
        }
        return rezultat;
    }

    /// <summary>
    /// Sursa declarativă a contului unei laturi (testul bazei §7.2), cu sursa
    /// care a dat efectiv contul: contul explicit al regulii e valoare directă
    /// sau fallback când sursa declarată nu rezolvă.
    /// </summary>
    public static RezolvareCont Cont(SursaCont sursa, Guid? contExplicit, Guid? contTipMaterial,
            LaturiFapt laturi) {
        var (dinSursa, sursaRezolvata) = sursa switch {
            SursaCont.TipMaterial => (contTipMaterial, SursaRezolvata.TipMaterial),
            SursaCont.RepartitorPredator => (laturi.ContImplicitPredatorId, SursaRezolvata.RepartitorPredator),
            SursaCont.RepartitorPrimitor => (laturi.ContImplicitPrimitorId, SursaRezolvata.RepartitorPrimitor),
            _ => (contExplicit, SursaRezolvata.Explicit),
        };
        if (dinSursa != null)
            return new RezolvareCont(dinSursa, sursaRezolvata);
        return contExplicit != null
            ? new RezolvareCont(contExplicit, SursaRezolvata.FallbackExplicit)
            : new RezolvareCont(null, SursaRezolvata.Nerezolvat);
    }

    /// <summary>Linia trece filtrul de natură al politicii de conex.</summary>
    public static bool Conex(PoliticaConexFapt politica, LinieFapt linie) =>
        politica.NaturaFiltru == null || politica.NaturaFiltru == linie.Natura;

    // Un singur text pentru „partenerul nu se vede", indiferent dacă lipsește de
    // pe document, nu există în bază sau e invizibil pe ușa securizată. NU e o
    // simplificare: distincția ar fi un ORACOL DE EXISTENȚĂ (80a).
    const string FaraPartener =
        "Fără partener vizibil pe document — rezolvarea a căzut pe rândurile generice.";

    /// <summary>
    /// Implicitul de TVA (F23-D2): regimul e al partenerului, cota e a
    /// produsului. `randuriTip` = TOATE rândurile tipului, nefiltrate — filtrul
    /// de dată și de clasă fiscală e aici, cu motiv per rând. Rândul câștigător
    /// e cel mai specific, apoi `ValabilDeLa` cel mai recent; tipul INACTIV sare
    /// TREAPTA, nu caută următorul rând.
    /// </summary>
    public static PotrivireTvaImplicit TvaImplicit(IReadOnlyList<PoliticaTvaImplicitFapt> randuriTip,
            ClasaFiscalaPartener? clasa, DateOnly data, Guid? candidatPartener, Guid? candidatAncora,
            Guid? candidatProdus, IReadOnlyDictionary<Guid, TipTvaFapt> tipuri, bool partenerLipsa) {
        var note = new List<string>();
        if (partenerLipsa)
            note.Add(FaraPartener);

        var randPolitica = randuriTip
            .Where(r => (r.ValabilDeLa == null || r.ValabilDeLa <= data)
                && (r.ClasaFiscala == null || r.ClasaFiscala == clasa))
            .OrderByDescending(r => r.ClasaFiscala != null)
            .ThenByDescending(r => r.ValabilDeLa ?? DateOnly.MinValue)
            .Select(r => (PoliticaTvaImplicitFapt?)r)
            .FirstOrDefault();
        var candidatPolitica = randPolitica?.TipTvaId;

        TipTvaFapt? Viu(Guid? id, string treapta) {
            if (id == null || !tipuri.TryGetValue(id.Value, out var info))
                return null;
            if (info.Activ)
                return info;
            note.Add($"Tipul „{info.Cod}” ({treapta}) e INACTIV și a fost sărit.");
            return null;
        }

        var partener = Viu(candidatPartener, "regimul propriu al partenerului");
        var politica = Viu(candidatPolitica, "politica tipului × clasa fiscală");
        var ancora = Viu(candidatAncora, "ancora tipului de document");
        var produs = Viu(candidatProdus, "cota proprie a produsului");

        var politicaInactiva = candidatPolitica is Guid idPolitica
            && tipuri.TryGetValue(idPolitica, out var tipPolitica) && !tipPolitica.Activ;
        var candidati = randuriTip.Select(r => new CandidatTvaImplicit(r,
            r.ValabilDeLa != null && r.ValabilDeLa > data ? MotivEliminare.DataViitoare
            : r.ClasaFiscala != null && r.ClasaFiscala != clasa ? MotivEliminare.ClasaDiferita
            : randPolitica is PoliticaTvaImplicitFapt castigator && castigator.Id != r.Id
                ? (r.ClasaFiscala != null) == (castigator.ClasaFiscala != null)
                    && r.ValabilDeLa == castigator.ValabilDeLa
                    ? MotivEliminare.Dublura : MotivEliminare.NivelMaiSlab
            : randPolitica == null ? MotivEliminare.NivelMaiSlab
            : politicaInactiva ? MotivEliminare.Inactiv
            : (MotivEliminare?)null)).ToList();

        var (r, sursaR) =
            partener != null ? (partener, SursaImplicit.Partener)
            : politica != null ? (politica, SursaImplicit.Politica)
            : ancora != null ? (ancora, SursaImplicit.Ancora)
            : ((TipTvaFapt?)null, SursaImplicit.Niciuna);

        // Produsul își impune COTA doar când regimul coincide; când regimurile
        // DIFERĂ, regimul bate cota.
        var rezultat =
            produs != null && r != null && produs.Value.Regim == r.Value.Regim
                ? new ImpliciteService.RezultatImplicit(produs.Value.Id, SursaImplicit.Produs,
                    Text(note, $"Cota produsului („{produs.Value.Cod}”) peste regimul dat de "
                        + $"{Eticheta(sursaR)} — același regim."))
            : r != null
                ? new ImpliciteService.RezultatImplicit(r.Value.Id, sursaR,
                    Text(note, produs != null
                        ? $"Regimul dat de {Eticheta(sursaR)} („{r.Value.Cod}”) bate cota produsului "
                            + $"(„{produs.Value.Cod}”) — regimuri diferite."
                        : $"Tipul vine din {Eticheta(sursaR)} („{r.Value.Cod}”)."))
            : produs != null
                ? new ImpliciteService.RezultatImplicit(produs.Value.Id, SursaImplicit.Produs,
                    Text(note, $"Nicio sursă de regim — rămâne cota produsului („{produs.Value.Cod}”)."))
            : new ImpliciteService.RezultatImplicit(null, SursaImplicit.Niciuna,
                Text(note, "Niciun implicit configurat pentru acest tip de document."));

        return new PotrivireTvaImplicit(rezultat, randPolitica, candidati);
    }

    static string Eticheta(SursaImplicit sursa) => sursa switch {
        SursaImplicit.Partener => "regimul propriu al partenerului",
        SursaImplicit.Produs => "cota proprie a produsului",
        SursaImplicit.Politica => "politica tipului × clasa fiscală",
        SursaImplicit.Ancora => "ancora tipului de document",
        _ => "nicio sursă",
    };

    // Notele stau ÎNAINTEA verdictului: ele explică de ce verdictul e cel care e.
    static string Text(List<string> note, string verdict) =>
        note.Count == 0 ? verdict : string.Join(" ", note.Append(verdict));
}
