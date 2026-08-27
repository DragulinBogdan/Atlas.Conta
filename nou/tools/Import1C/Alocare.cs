using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Import1C;

// SUPAPA DE IMPORT (decizia 48a): pinul pe lot vs. soldurile netate.
//
// Netarea deschiderii (47d) a rearanjat soldurile ȘI prețurile ÎN INTERIORUL
// grupei produs × depozit — deliberat, ca Atlas să nu pornească cu solduri
// negative. Consecința: un document 2025 care pin-uiește lotul original din 1C
// poate găsi lotul golit, deși grupa are marfa. Decizia 37d („pinul e intenția,
// fără fallback FIFO") rămâne INTACTĂ în motor — artefactul e al importului,
// deci se plătește în import, aici.
//
// Regula: ce acoperă pinul se ia de pe pin; deficitul se realocă FIFO în
// interiorul produs × gestiune (grupa în care netarea a conservat sumele, deci
// acoperirea există prin construcție — mai puțin grupele sărite la deschidere).
// Restul rămas după FIFO NU e excepție aici: se întoarce apelantului, care
// decide (backorder, raport, refuz). Gardianul de sold al motorului rămâne
// autoritatea finală la operare.
//
// **Disponibilul e SOLDUL LA DATA documentului** — soldul cumulat până la ea
// inclusiv. Până la ordinea cronologică a buclei (pasul 1 al lotului de robustețe)
// era minimul sumelor cumulate pe zilele ≥ data: importul scria documentele
// grupate pe TIP, deci în momentul în care se planifica o vânzare de pe 3 ianuarie
// registrul putea conține deja transferul de pe 17, iar un „sold la dată" ar fi
// spus că lotul are marfă pe care operarea ar fi refuzat-o la 17. Prudența aia
// costa exact ce trebuia să apere: vânzarea de pe 3 rămânea fără linie de stoc
// deși la data ei lotul era acoperit. De când unitățile lunii se execută în
// ordinea sursei, mișcările viitoare nu se mai scriu înaintea celor trecute, deci
// minimul pe viitor n-are obiect. Gardianul de sold al motorului (prefix-sum pe
// zile, 25d) rămâne autoritatea finală la operare: o alocare învechită e refuzată
// zgomotos, nu strecurată.
// Ce a alocat deja ACELAȘI document pe fiecare lot, încă necomis (registrul nu
// vede liniile lui): cantitatea — ca a doua linie să nu re-aloce ce a prins prima
// — ȘI valoarea cu care motorul o va scrie, ca a doua linie să vadă soldul
// VALORIC de după prima (D18-D2: linia care golește lotul preia restul). Cheia e
// lotul (grupa produs × gestiune × registru e fixă în interiorul unei alocări,
// iar un document nu iese din același lot pe două gestiuni).
sealed class AlocatInDocument {
    readonly Dictionary<Guid, SoldStoc> peLot = [];
    public SoldStoc Ia(Guid lotId) => peLot.GetValueOrDefault(lotId);
    public void Adauga(Guid lotId, decimal cantitate, decimal valoare) =>
        peLot[lotId] = peLot.GetValueOrDefault(lotId) + new SoldStoc(cantitate, valoare);
}

sealed class AlocareIesire {
    // Diagnostic, nu eșec (§12.1): se raportează per lună.
    public int Realocari { get; private set; }
    public decimal CantitateRealocata { get; private set; }
    public int PinuriGoale { get; private set; }
    public decimal Nedescarcat { get; private set; }

    // Produsele pe care supapa chiar le-a ATINS: pinul a fost înlocuit cu FIFO,
    // sau ieșirea n-a avut acoperire. Amândouă schimbă costul cu care Atlas
    // descarcă față de 1C, deci sunt exact mulțimea în care contractul lunar are
    // voie să vadă o diferență de VALOARE la cantitate exactă (§8.3). E o
    // evidență a ceea ce s-a întâmplat, nu o categorie presupusă.
    //
    // Se PERSISTĂ (tabela de legături „1C:ProdusRealocat", cheia = id-ul Atlas al
    // produsului): altfel verdictul contractului ar depinde de faptul că rularea
    // curentă a importat sau a sărit documentele — o reluare care sare tot n-ar
    // mai ști ce a atins supapa și ar raporta drept nejustificate exact
    // diferențele pe care ea le-a produs.
    public HashSet<Guid> ProduseRealocate { get; } = [];

    // Marcajele DECISE, dar încă nescrise. Alocarea se calculează în
    // ObjectSpace-ul de PLANIFICARE, care se aruncă fără commit (e o interogare de
    // solduri, nu o scriere) — o legătură scrisă acolo n-ajungea niciodată în bază
    // (măsurat: 0 rânduri „1C:ProdusRealocat" după rulări care chiar realocaseră).
    // Deci marcajul se AMÂNĂ aici și se scrie în ObjectSpace-ul care se comite —
    // al documentului, odată cu draftul și legătura lui (§12.4).
    readonly HashSet<Guid> inAsteptare = [];

    // Marcaje rămase nescrise fiindcă unitatea de import n-a materializat nimic
    // (planificare urmată de eșec sau de skip): se abandonează la finalul unității,
    // ca evidența să nu rămână ÎNAINTEA faptelor. Se numără — o cifră mare
    // înseamnă că se planifică mult și se scrie puțin.
    public int MarcajeAbandonate { get; private set; }

    public const string ViewRealocate = "ProdusRealocat";

    public void Incarca(IObjectSpace os) {
        foreach (var tinta in Legaturi.Incarca(os, ViewRealocate).Values)
            ProduseRealocate.Add(tinta);
    }

    // Produsul NĂSCUT de o asamblare intră în aceeași mulțime, din același motiv:
    // lotul lui poartă un preț pus de ATLAS (valoarea consumurilor noastre,
    // distribuită de invariantul |Σ produse − Σ consumuri| ≤ 0,005), nu cifra cu
    // care 1C își evaluează produsul. Nu e o presupunere — e construcția
    // mecanismului, la fel ca la netarea deschiderii.
    //
    // De ce marcaj și nu o sumă înregistrată în registrul divergențelor: delta de
    // evaluare a asamblării nu e o proprietate a CHEII, ci una PER BUCATĂ a
    // lotului, și pleacă odată cu bucățile. Măsurat pe asamblarea din octombrie a
    // produsului …A680: 8 bucăți produse cu o deltă de −719,96 (adică −89,995 pe
    // bucată), două vândute între timp, iar pe cheie au rămas 6 × (−89,995) =
    // −539,97 — exact abaterea observată. O sumă fixă înregistrată la producție se
    // învechește la prima ieșire, iar dacă marfa se și mută în alt depozit
    // înregistrarea rămâne pe cheia veche, unde nu mai e nimic de explicat.
    // Marcajul n-are niciuna dintre cele două probleme: e pe PRODUS, nu pe cheie,
    // și nu afirmă nicio cifră care să se poată învechi.
    public void MarcheazaEvaluatDeAtlas(Guid produsId) => Marcheaza(produsId);

    // Marchează produsul — decizia se ia la alocare, scrierea se amână.
    void Marcheaza(Guid produsId) {
        if (!ProduseRealocate.Contains(produsId))
            inAsteptare.Add(produsId);
    }

    // Scrie marcajele amânate în ObjectSpace-ul documentului, ÎNAINTEA commit-ului
    // lui. Setul din memorie se actualizează abia la `Confirma` (după commit): un
    // commit picat nu are voie să lase evidența „scrisă" doar în RAM.
    public void Persista(IObjectSpace os) {
        foreach (var produsId in inAsteptare)
            if (!ProduseRealocate.Contains(produsId))
                Legaturi.Leaga(os, ViewRealocate, produsId.ToString(), produsId);
    }

    public void Confirma() {
        ProduseRealocate.UnionWith(inAsteptare);
        inAsteptare.Clear();
    }

    public void RenuntaLaNepersistate() {
        MarcajeAbandonate += inAsteptare.Count;
        inAsteptare.Clear();
    }

    int realocariLaStart;
    decimal cantitateLaStart;

    public void IncepeLuna() {
        realocariLaStart = Realocari;
        cantitateLaStart = CantitateRealocata;
    }

    public (int Realocari, decimal Cantitate) DeltaLunii() =>
        (Realocari - realocariLaStart, CantitateRealocata - cantitateLaStart);

    // `dejaAlocat` = alocările deja făcute în ACELAȘI document, încă necomise
    // (registrul nu le vede — pattern-ul DescarcareService, 38b). Se ACTUALIZEAZĂ
    // aici: apelantul îl ține per document și îl transmite la fiecare linie.
    //
    // `Valoare` pe fiecare alocare = PREDICȚIA valorii pe care motorul o va
    // materializa pe linia de ieșire (riscul 5 al contractului F18): aceeași
    // regulă ca `StocService.AplicaValoareIesire` — `preț lot × cantitate`
    // rotunjit la bani, iar pe alocarea care GOLEȘTE lotul la data documentului
    // tot soldul valoric rămas (`StocService.ValoareGolire`, funcția comună).
    // Punțile și divergențele calculate din ea declară exact ce postează motorul,
    // nu cifra cu cenții vechi.
    public (IReadOnlyList<(Guid LotId, decimal Cantitate, decimal Valoare)> Alocari, decimal Ramas) Aloca(
            IObjectSpace os, Guid? lotDoritId, Guid produsId, Guid gestiuneId, TipStoc tipStoc,
            DateOnly data, decimal cantitate, AlocatInDocument dejaAlocat) {
        var alocari = new List<(Guid LotId, decimal Cantitate, decimal Valoare)>();
        var ramas = cantitate;
        if (cantitate <= 0)
            return (alocari, 0m);

        // O singură interogare per cerere, pentru toată grupa produs × gestiune ×
        // registru: pinul și FIFO-ul se servesc din aceleași rânduri (înainte era
        // un `Sold` per lot). Valoarea vine din aceleași rânduri (D18-D2).
        var randuri = os.GetObjectsQuery<RegistruStoc>()
            .Where(r => r.Lot.ProdusId == produsId && r.RepartitorId == gestiuneId
                && r.TipStoc == tipStoc)
            .Select(r => new { r.LotId, r.Data, r.Cantitate, r.Valoare, DataLot = r.Lot.Data, PretLot = r.Lot.PretUnitar })
            .ToList();
        var loturi = randuri.GroupBy(r => r.LotId)
            .Select(g => new {
                LotId = g.Key,
                DataLot = g.Min(x => x.DataLot),
                PretLot = g.First().PretLot,
                Disponibil = Disponibil(g.Select(x => (x.Data, x.Cantitate, x.Valoare)), data),
            })
            .ToDictionary(x => x.LotId);

        // Soldul lotului de dinaintea liniei curente: registrul la dată minus ce a
        // luat deja documentul (pe ambele axe).
        SoldStoc Inainte(Guid lotId) => loturi.TryGetValue(lotId, out var l)
            ? new SoldStoc(l.Disponibil.Cantitate - dejaAlocat.Ia(lotId).Cantitate,
                l.Disponibil.Valoare - dejaAlocat.Ia(lotId).Valoare)
            : default;
        decimal Liber(Guid lotId) => Math.Max(0m, Inainte(lotId).Cantitate);

        void Ia(Guid lotId, decimal cantitateLuata) {
            var valoare = StocService.ValoareGolire(Inainte(lotId), cantitateLuata)
                ?? Scara.RotunjesteBani(cantitateLuata * loturi[lotId].PretLot);
            alocari.Add((lotId, cantitateLuata, valoare));
            dejaAlocat.Adauga(lotId, cantitateLuata, valoare);
            ramas -= cantitateLuata;
        }

        // 1. Pinul, cât acoperă.
        if (lotDoritId is { } pin) {
            var iaDePePin = Math.Min(ramas, Liber(pin));
            if (iaDePePin > 0)
                Ia(pin, iaDePePin);
            else
                PinuriGoale++;
        }

        // 2. Deficitul, FIFO în produs × gestiune (vechimea lotului, apoi ID-ul —
        //    aceeași ordine ca `StocService.AlocaFifoTolerant`).
        var inainteDeFifo = ramas;
        foreach (var lot in loturi.Values.OrderBy(l => l.DataLot).ThenBy(l => l.LotId)) {
            if (ramas <= 0)
                break;
            if (lotDoritId == lot.LotId)
                continue;
            var liber = Liber(lot.LotId);
            if (liber > 0)
                Ia(lot.LotId, Math.Min(ramas, liber));
        }
        // Realocare = doar deplasarea cerută de netare: fără pin, FIFO e pickingul
        // normal al modelului, nu o supapă.
        if (lotDoritId != null && inainteDeFifo > ramas) {
            Realocari++;
            CantitateRealocata += inainteDeFifo - ramas;
            Marcheaza(produsId);
        }
        // Ieșirea neacoperită (returul fără marfă în stoc) atinge la fel evaluarea
        // produsului: 1C a scos marfa la costul lotului lui, Atlas n-a scos-o
        // deloc. Contractul lunar are nevoie de mulțimea asta ca să poată numi
        // diferența, nu s-o presupună.
        if (ramas > 0)
            Marcheaza(produsId);

        Nedescarcat += ramas;
        return (alocari, ramas);
    }

    // Soldul lotului la sfârșitul zilei `data`: ce marfă exista în momentul
    // documentului. Mișcările de după (dacă totuși există — un document al lunii
    // scris deja, cu timestamp mai mare) nu scad disponibilul: le acoperă
    // gardianul motorului la operare.
    // Pe ambele axe: valoarea la dată e soldul pe care linia care golește lotul îl
    // preia integral (D18-D2) — aceeași convenție (prefix-sum ≤ dată) ca
    // `StocService.SolduriLaData`.
    static SoldStoc Disponibil(IEnumerable<(DateOnly Data, decimal Cantitate, decimal Valoare)> miscari, DateOnly data) {
        var laData = miscari.Where(m => m.Data <= data).ToList();
        return new SoldStoc(laData.Sum(m => m.Cantitate), laData.Sum(m => m.Valoare));
    }
}
