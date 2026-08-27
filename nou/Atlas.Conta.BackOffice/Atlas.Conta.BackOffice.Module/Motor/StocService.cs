using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Cheia unei poziții de stoc: registrul e sold prin SUM pe această cheie.
public readonly record struct CheieStoc(Guid LotId, Guid RepartitorId, TipStoc TipStoc);

// O mișcare de stoc încă nescrisă (delta) — limbajul verificărilor de sold.
public readonly record struct MiscareStoc(CheieStoc Cheie, DateOnly Data, decimal Cantitate);

// Soldul unei chei pe AMBELE axe ale registrului (cantitate + valoare) — limbajul
// regulii de golire (D18-D2).
public readonly record struct SoldStoc(decimal Cantitate, decimal Valoare) {
    public static SoldStoc operator +(SoldStoc a, SoldStoc b) => new(a.Cantitate + b.Cantitate, a.Valoare + b.Valoare);
}

public static class StocService {
    // ───────────── D18-D2: ieșirea care GOLEȘTE cheia preia valoarea rămasă ─────────────
    //
    // Identificarea specifică (OMFP 1802, decizia 13) spune că un lot consumat
    // integral iese cu valoarea lui INTEGRALĂ. `preț × cantitate` rotunjit la
    // bani pe fiecare ieșire lasă însă pe lot un rezidu de cenți (3 × 10,005 =
    // 30,02 în registru, iar 1 + 1 + 1 la 10,01 scot 30,03: lotul rămâne cu
    // 0 bucăți și −0,01 lei — `ReziduValoricFaraCantitate` în SAF-T S, 74g).
    // Regula: pe o mișcare NEGATIVĂ pe cheia (Lot × Repartitor × TipStoc), dacă
    // soldul cantitativ al cheii la data documentului (prefix-sum ≤ Data —
    // aceeași convenție ca gardianul 25d) ajunge, DUPĂ mișcare, exact la 0,
    // valoarea mișcării = tot soldul VALORIC de dinainte; altfel valoarea rămâne
    // cea calculată de frunză (preț × cantitate). Cenții cad pe contul de
    // cheltuială/venit al ieșirii, contul de stoc ajunge exact la 0 pe lot.
    //
    // O singură funcție PURĂ (`ValoareGolire`) e sursa regulii pentru amândoi
    // apelanții: motorul (`AplicaValoareIesire`, mai jos) și conectorul Import1C
    // (`AlocareIesire.Aloca`, care PREZICE valoarea pe care o va scrie motorul,
    // pentru punți și divergențe — riscul 5 al contractului F18).
    //
    // `null` = mișcarea NU golește cheia: apelantul își păstrează `preț ×
    // cantitate`. `cantitateIesita` e MAGNITUDINEA (> 0); o ieșire de 0 nu
    // golește nimic.
    public static decimal? ValoareGolire(SoldStoc soldInainte, decimal cantitateIesita) =>
        cantitateIesita > 0m && soldInainte.Cantitate == cantitateIesita ? soldInainte.Valoare : null;

    // Soldurile (cantitate + valoare) la sfârșitul zilei `data` pentru TOATE
    // cheile loturilor cerute, într-o singură interogare grupată. Rândurile
    // documentului `faraDocumentId` sunt excluse explicit: la re-operare după
    // anulare ele sunt deja șterse (ștergere amânată, filtru global), dar
    // excluderea ține regula corectă și dacă apelantul o cheamă cu rândurile
    // proprii încă vii (dry-run pe un document Operat n-are cum, dar costul e un
    // predicat).
    public static Dictionary<CheieStoc, SoldStoc> SolduriLaData(IObjectSpace os, IReadOnlyCollection<Guid> loturi,
        DateOnly data, Guid? faraDocumentId = null) {
        if (loturi.Count == 0)
            return new();
        var rows = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => loturi.Contains(r.LotId) && r.Data <= data);
        if (faraDocumentId is { } docId)
            rows = rows.Where(r => r.DocumentId != docId);
        return rows
            .GroupBy(r => new { r.LotId, r.RepartitorId, r.TipStoc })
            .Select(g => new { g.Key, Cantitate = g.Sum(r => r.Cantitate), Valoare = g.Sum(r => r.Valoare) })
            .ToList()
            .ToDictionary(x => new CheieStoc(x.Key.LotId, x.Key.RepartitorId, x.Key.TipStoc),
                x => new SoldStoc(x.Cantitate, x.Valoare));
    }

    // Pasul motorului (chemat din `CalculeazaSiValideaza`, DUPĂ `PregatesteOperare`
    // și ÎNAINTE de validare — 33d: valoarea finală a liniei trebuie să existe când
    // frunza își verifică invarianții, ex. ASM |Σproduse − Σconsumuri|). Primește
    // mișcările deja potrivite pe regulile de stoc (cheia și semnul sunt ale
    // REGULII, nu ale frunzei — de aceea pasul stă aici și nu în fiecare
    // `PregatesteOperare`: frunza nu știe pe ce `TipStoc` iese și nu are voie să
    // re-potrivească regulile, 42a). Liniile aceluiași document pe aceeași cheie
    // se acumulează în ordinea liniilor: a doua vede soldul după prima.
    //
    // Scrie `Valoare` pe LINIE (rândul de registru = `Semn × Valoare`, deci
    // `Valoare = −soldValoric × Semn`): pe BCS (Semn −1) rămâne pozitivă, pe
    // LDI/RLF/ASM (Semn +1, cantitate negativă) rămâne negativă — convenția de
    // semn a fiecărei frunze e păstrată. O linie cu mai multe mișcări (BCS:
    // −Magazie + Consum, BTR: −sursă + destinație) ia valoarea de pe PRIMA
    // mișcare negativă care golește; celelalte o poartă (restul se MUTĂ pe
    // consum/destinație — corect: valoarea nu se pierde).
    public static void AplicaValoareIesire(IObjectSpace os, Document doc,
        IReadOnlyList<(DocumentDetaliu Detaliu, RegulaStoc Regula, MiscareStoc Miscare)> miscari) {
        var loturiIesire = miscari.Where(m => m.Miscare.Cantitate < 0m)
            .Select(m => m.Miscare.Cheie.LotId).Distinct().ToList();
        if (loturiIesire.Count == 0)
            return;
        var solduri = SolduriLaData(os, loturiIesire, doc.Data, doc.ID);
        var acumulat = new Dictionary<CheieStoc, SoldStoc>();
        var deciseValori = new HashSet<Guid>();
        foreach (var (detaliu, regula, miscare) in miscari) {
            var cheie = miscare.Cheie;
            var inainte = solduri.GetValueOrDefault(cheie) + acumulat.GetValueOrDefault(cheie);
            if (miscare.Cantitate < 0m && !deciseValori.Contains(detaliu.ID)
                    && ValoareGolire(inainte, -miscare.Cantitate) is { } rest) {
                detaliu.Valoare = -rest * regula.Semn;
                deciseValori.Add(detaliu.ID);
            }
            acumulat[cheie] = acumulat.GetValueOrDefault(cheie)
                + new SoldStoc(miscare.Cantitate, regula.Semn * detaliu.Valoare);
        }
    }

    public static decimal Sold(IObjectSpace os, CheieStoc cheie, DateOnly? panaLa = null) {
        var rows = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.LotId == cheie.LotId && r.RepartitorId == cheie.RepartitorId && r.TipStoc == cheie.TipStoc);
        if (panaLa is { } d)
            rows = rows.Where(r => r.Data <= d);
        return rows.Sum(r => (decimal?)r.Cantitate) ?? 0m;
    }

    // Gardianul din decizia 14: după aplicarea mișcărilor `delta` (și, la
    // anulare, excluderea rândurilor `randuriEliminate`), soldul CUMULAT al
    // fiecărei chei afectate trebuie să rămână ≥ 0 la sfârșitul FIECĂREI zile
    // (granularitatea registrului e ziua — ordinea intra-zi nu e definită).
    // Acoperă și operarea retroactivă: un minus inserat în urmă nu are voie să
    // ducă vreun prefix ulterior sub zero.
    public static void VerificaSoldIntermediar(IObjectSpace os, IReadOnlyCollection<MiscareStoc> delta,
        IReadOnlyCollection<Guid> randuriEliminate = null) {
        var chei = delta.Select(m => m.Cheie).Distinct().ToList();
        var erori = new List<string>();
        foreach (var cheie in chei) {
            var existente = os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.LotId == cheie.LotId && r.RepartitorId == cheie.RepartitorId && r.TipStoc == cheie.TipStoc)
                .Select(r => new { r.ID, r.Data, r.Cantitate })
                .ToList();
            var miscari = existente
                .Where(r => randuriEliminate == null || !randuriEliminate.Contains(r.ID))
                .Select(r => (r.Data, r.Cantitate))
                .Concat(delta.Where(m => m.Cheie == cheie).Select(m => (m.Data, m.Cantitate)));

            decimal sold = 0;
            foreach (var zi in miscari.GroupBy(m => m.Data).OrderBy(g => g.Key)) {
                sold += zi.Sum(m => m.Cantitate);
                if (sold < 0) {
                    var lot = os.GetObjectByKey<Lot>(cheie.LotId);
                    var rep = os.GetObjectByKey<Repartitor>(cheie.RepartitorId);
                    erori.Add($"Sold negativ ({sold:0.####}) la {zi.Key:yyyy-MM-dd} pe lotul " +
                        $"{lot?.Produs?.Denumire} din {lot?.Data:yyyy-MM-dd} ({rep?.Denumire}, {cheie.TipStoc}).");
                    break;
                }
            }
        }
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));
    }

    // Picking auto-FIFO TOLERANT (decizia 13, extins la P2 §5): sparge o cerere
    // (produs × gestiune × TipStoc × dată × cantitate) pe loturile disponibile în
    // ordinea vechimii lotului și întoarce alocările + restul neacoperit (spre
    // deosebire de `AlocaFifo`, NU aruncă la insuficient — descărcarea de gestiune
    // lasă restul ca backorder). `dejaAlocat` = alocări per lot deja făcute în
    // aceeași generare, încă necomise (`Sold` nu le vede) — se SCADE din soldul
    // fiecărui lot ca a doua trecere să nu re-aloce ce a prins prima.
    public static (IReadOnlyList<(Guid LotId, decimal Cantitate)> Alocari, decimal Ramas) AlocaFifoTolerant(
        IObjectSpace os, Guid produsId, Guid gestiuneId, TipStoc tipStoc, DateOnly data, decimal cantitate,
        IReadOnlyDictionary<Guid, decimal> dejaAlocat = null) {
        var solduri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Lot.ProdusId == produsId && r.RepartitorId == gestiuneId
                && r.TipStoc == tipStoc && r.Data <= data)
            .GroupBy(r => r.LotId)
            .Select(g => new { LotId = g.Key, Sold = g.Sum(r => r.Cantitate) })
            .Where(x => x.Sold > 0)
            .ToList();

        var alocari = new List<(Guid, decimal)>();
        var ramas = cantitate;
        foreach (var s in solduri
            .Select(x => (Lot: os.GetObjectByKey<Lot>(x.LotId), x.Sold))
            .OrderBy(x => x.Lot.Data).ThenBy(x => x.Lot.ID)) {
            if (ramas <= 0)
                break;
            var disponibil = s.Sold - (dejaAlocat?.GetValueOrDefault(s.Lot.ID) ?? 0m);
            if (disponibil <= 0)
                continue;
            var alocat = Math.Min(ramas, disponibil);
            alocari.Add((s.Lot.ID, alocat));
            ramas -= alocat;
        }
        return (alocari, ramas);
    }

    // Picking auto-FIFO (decizia 13): sparge o cerere (produs × gestiune ×
    // cantitate) pe loturile disponibile la dată, în ordinea vechimii lotului.
    // Consumat de UI/derivate la culegere; gardianul de sold re-verifică oricum
    // la operare. Override-ul manual = utilizatorul alege alt lot pe linie.
    // Wrapper strict peste `AlocaFifoTolerant`: aruncă dacă rămâne rest.
    public static IReadOnlyList<(Lot Lot, decimal Cantitate)> AlocaFifo(IObjectSpace os,
        Guid produsId, Guid gestiuneId, TipStoc tipStoc, DateOnly data, decimal cantitate) {
        var (alocari, ramas) = AlocaFifoTolerant(os, produsId, gestiuneId, tipStoc, data, cantitate);
        if (ramas > 0)
            throw new OperareException(
                $"Stoc insuficient: lipsesc {ramas:0.####} din {cantitate:0.####} cerute la {data:yyyy-MM-dd}.");
        return alocari.Select(a => (os.GetObjectByKey<Lot>(a.LotId), a.Cantitate)).ToList();
    }
}
