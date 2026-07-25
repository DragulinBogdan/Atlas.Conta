using Microsoft.Data.SqlClient;

namespace Import1C;

// PASUL 2 al feliei 1C-c: cititorii documentelor anului 2025 din view-urile
// `[flax]` (a doua jumătate a lui `FlaxDb`; prima ține nomenclatoarele, balanțele
// și măturile pre-flight). Aceleași reguli ca acolo: CONTRACT DE COLOANE (nu
// dependență de codul SkyContaSource), LTRIM peste tot, cititorul EXPUNE datele
// și nu decide nimic — politica de import e a handler-elor (pașii 3–5).
//
// Trei alegeri de formă, toate măsurate/verificate pe date (25.07.2026):
//
//  1. **Granularitatea = LUNA, nu documentul.** Fiecare cititor de secțiune
//     întoarce TOATE liniile lunii într-o singură interogare (join pe antet, ca
//     filtrul de lună și `Posted` să fie unul singur); handler-ul le grupează pe
//     `DocumentId`. Motivul e măsurat: registrul contabil per document costă
//     13 ms (note) + 57 ms (subconto) — pe 130.000 de documente ar fi ~2,5 ore
//     doar citire; aceleași date pe lună ies în 0,15 s + 0,67 s (≈10 s pe tot
//     anul). Vezi `RanduriNotaPeLuna` / `SubcontoNotaPeLuna`.
//
//  2. **`Posted <> 0x00` e filtrul, și e verificat, nu presupus**: pe 2025 NICIUN
//     document cu `Posted = 0` nu are rânduri în registrul contabil (verificat pe
//     APR/VZ/BTR/EXT), iar perioada notei coincide întotdeauna cu luna
//     documentului — deci filtrul nu poate ascunde postări. Invariantul rămâne
//     verificat la fiecare rulare de pre-flight (§C: „documente care postează ≤
//     antete Posted"), fiindcă e ipoteza pe care stă tot importul.
//
//  3. **Referințele polimorfe circulă ca `FlaxRef`** (tip + nume + id + descriere).
//     Capcană verificată: `X_Id` e NON-NULL (plin de zerouri) când referința e
//     GOALĂ — testul se face pe `X_Type`, niciodată pe id. Numele tipului vine
//     dintr-un CASE peste coloanele tipizate ale view-ului (aceeași rețetă ca
//     `TipuriRecorder`), nu dintr-un dicționar de TypeRef-uri hardcodate: un tip
//     pe care view-ul nu-l expune iese cu `Tip = null` și `TipRef` nenul — semnal
//     citibil, nu tăcere.

// Referință polimorfă 1C. `TipRef` = TypeRef-ul brut (hex), `Tip` = numele
// tipului dacă view-ul are coloană tipizată pentru el, `Descriere` = eticheta 1C
// (din ea se parsează, de pildă, data documentului creator de lot — 47d).
record FlaxRef(string TipRef, string Tip, string Id, string Descriere);

// ======================= Antete + secțiuni, per tip 1C =======================
// Convenție: `Id` = KeyField hex (cheia de legătură `1C:<view>`), `Numar`/`Data`
// din antet, iar liniile poartă `DocumentId` (= ParentRef) + `Linie` (= LineNo).
// `LineNo` E CUVÂNT REZERVAT T-SQL (`SET LINENO`) — se scrie `[LineNo]`.

// -- AprovizionareMarfuriSiServiciiPrimite → FCT + NIR conex (§4) --
// `Valuta` + `Curs` (`ValutaDoc` / `CursDeDecontari`) NU sunt decor: sumele din
// SECȚIUNI sunt în valuta documentului, iar registrul contabil e în lei — pe
// 2025, 1.270 de facturi în EUR și 4 în USD. Verificat pe eșantion:
// `round(Suma × Curs, 2)` reproduce EXACT rândul de notă (961,73 × 4,9741 =
// 4.783,74). La documentele în lei cursul iese 0 sau 1 — se normalizează la 1.
record FlaxAprovizionare(string Id, string Numar, DateTime Data, string PartenerId,
    string NumarFactura, string SeriaFactura, DateTime? DataFacturii, DateTime? DataScadenta,
    string DepozitId, decimal SumaDocument, bool SumaIncludeTva, bool TaxareInversa,
    bool CalcTva, string PoliticaTva, string TipOperatiune, string Valuta, decimal Curs) {
    // Cursul de aplicat sumelor din secțiuni: 1 pentru documentele în lei.
    public decimal CursLei => Curs <= 0m ? 1m : Curs;

    public decimal InLei(decimal suma) => Math.Round(suma * CursLei, 2);
}

record FlaxAprovizionareMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string CotaTva, decimal Cantitate, decimal Pret, decimal Suma,
    decimal SumaTva, string DepozitId);

record FlaxAprovizionareServiciu(string DocumentId, int Linie, string NomenclatorId,
    string Explicatie, decimal Cantitate, decimal Pret, decimal Suma, string CotaTva,
    decimal SumaTva, string ContCheltuieli, string PartenerDimensiuneId);

// -- VanzareMarfuriSiServiciiPrestate → FCL + DSC (§4) --
record FlaxVanzare(string Id, string Numar, DateTime Data, string PartenerId,
    string NumarFactura, string SeriaFactura, DateTime? DataScadenta, string DepozitId,
    decimal SumaDocument, bool SumaIncludeTva, string TipOperatiune);

record FlaxVanzareMarfa(string DocumentId, int Linie, string NomenclatorId, string ContEvidenta,
    string ContVenituri, string CotaTva, decimal Cantitate, decimal Pret, decimal Suma,
    decimal SumaTva, string DepozitId);

record FlaxVanzareServiciu(string DocumentId, int Linie, string NomenclatorId, string Explicatie,
    decimal Cantitate, decimal Pret, decimal Suma, string CotaTva, decimal SumaTva,
    string ContVenituri);

// -- TransferDeMarfuri → BTR --
record FlaxTransfer(string Id, string Numar, DateTime Data,
    string DepozitExpeditorId, string DepozitDestinatarId);

record FlaxTransferMarfa(string DocumentId, int Linie, string NomenclatorId, string ContEvidenta,
    string ContEvidentaNou, decimal Cantitate, decimal Pret);

// -- BonDeConsum → BCS. Antetul NU are cont de cheltuială (verificat pe view):
// contul e per LINIE (`ContCheltuieli`), iar linia are și depozitul ei. Liniile
// n-au preț/sumă — costul vine din lot, ca la BCS-ul Atlas (27d).
record FlaxBonConsum(string Id, string Numar, DateTime Data, string DepozitId);

record FlaxBonConsumMaterial(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string ContCheltuieli, decimal Cantitate, string DepozitId);

// -- MarireStocDeMarfuri / DiminuareStocDeMarfuri → LDI ± --
// Un singur cuplu de record-uri pentru ambele: schemele diferă doar prin
// `TipOperatiune` (numai la Diminuare) — null la Mărire.
record FlaxAjustareStoc(string Id, string Numar, DateTime Data, string DepozitId,
    decimal SumaDocument, string ContCheltuieli, string TipOperatiune);

record FlaxAjustareStocMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string CotaTva, decimal Cantitate, decimal Pret, decimal Suma);

// -- ReturDeLaClient → RDC --
record FlaxReturClient(string Id, string Numar, DateTime Data, string PartenerId,
    string NumarFactura, string SeriaFactura, DateTime? DataScadenta, string DepozitId,
    decimal SumaDocument, bool SumaIncludeTva, string TipOperatiune, FlaxRef DocumentBaza);

record FlaxReturClientMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string ContVenituri, string CotaTva, decimal Cantitate, decimal Pret,
    decimal Suma, decimal SumaTva, decimal SumaAchizitie, string DepozitId,
    FlaxRef DocumentVanzare);

record FlaxReturClientServiciu(string DocumentId, int Linie, string NomenclatorId,
    string Explicatie, decimal Cantitate, decimal Pret, decimal Suma, string CotaTva,
    decimal SumaTva, string ContVenituri, FlaxRef DocumentVanzare);

// -- ReturLaFurnizor → RLF. `DocumentIntrare` de pe linie identifică LOTUL
// original (46e); antetul are și un `DocBaza` NEpolimorf (nvarchar + _ID), sărit.
record FlaxReturFurnizor(string Id, string Numar, DateTime Data, string PartenerId,
    string NumarFactura, string SeriaFactura, string DepozitId, decimal SumaDocument,
    string TipDeRetur, string TipOperatiune);

record FlaxReturFurnizorMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string CotaTva, decimal Cantitate, decimal Pret, decimal Suma,
    decimal SumaTva, FlaxRef DocumentIntrare);

record FlaxReturFurnizorServiciu(string DocumentId, int Linie, string NomenclatorId,
    string Explicatie, decimal Cantitate, decimal Pret, decimal Suma, string CotaTva,
    decimal SumaTva, string ContCheltuieli, FlaxRef DocumentIntrare);

// -- Asamblare / Dezasamblare → ASM (46d) --
// **Direcția se INVERSEAZĂ între cele două view-uri**, verificat pe subconto-ul
// registrului (`KindRef = "Loturi"`, latura debit poartă lotul NOU):
//   * Asamblare:    `Articole` = PRODUSUL (1 per document, debit, lot nou
//     „AsamblareSED…"), `Subasamble` = CONSUMURILE (n, credit, loturi existente);
//   * Dezasamblare: exact pe dos — `Articole` = CONSUMUL (1, credit),
//     `Subasamble` = PRODUSELE (n, debit, lot nou „DezasamblareSED…").
// De aici și forma coloanelor: prețul de evaluare există DOAR pe
// `Asamblare_Articole`, iar `CotaDeValoare` doar pe `Dezasamblare_Subasamble` —
// adică fiecare view îl poartă pe latura care produce. Secțiunea consumatoare
// n-are valoare proprie nicăieri: costul vine din lot, ca la ASM-ul Atlas (46d).
// Câmpurile absente ies 0; numele secțiunii, nu al record-ului, dă direcția.
record FlaxAsamblare(string Id, string Numar, DateTime Data,
    string DepozitArticoleId, string DepozitSubasambleId);

record FlaxAsamblareLinie(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, decimal Cantitate, decimal Pret, string DepozitId,
    decimal CotaDeValoare);

// -- RaportDeVanzariCuAmanunt → surogat FCL + DSC (§12.2) --
record FlaxRaportAmanunt(string Id, string Numar, DateTime Data, string DepozitId,
    string CasierieId, string ContCasa, decimal SumaDocument, bool SumaIncludeTva,
    decimal SumaNumerar, decimal SumaCard, decimal SumaCec, decimal SumaTichete,
    decimal SumaVirament, string TipOperatiune);

record FlaxRaportAmanuntMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string ContVenituri, string CotaTva, decimal Cantitate, decimal Pret,
    decimal Suma, decimal SumaTva, string DepozitId);

record FlaxRaportAmanuntServiciu(string DocumentId, int Linie, string NomenclatorId,
    string Explicatie, decimal Cantitate, decimal Pret, decimal Suma, string CotaTva,
    decimal SumaTva, string ContVenituri);

record FlaxRaportAmanuntFactura(string DocumentId, int Linie, FlaxRef Factura,
    string PartenerId, string BonFiscal, decimal SumaFaraTva, decimal Tva, decimal Suma);

record FlaxRaportAmanuntInchidere(string DocumentId, int Linie, string TipDePlata,
    decimal Suma, string ContDeEvidenta);

// -- AvizDeIesire / AvizDeIntrare → surogat (§12.2) --
// Liniile avizelor NU au depozit propriu: gestiunea e a antetului.
record FlaxAvizIesire(string Id, string Numar, DateTime Data, string PartenerId,
    string DepozitId, string NumarFactura, string SeriaFactura, decimal SumaDocument,
    bool SumaIncludeTva, string TipOperatiune);

record FlaxAvizIesireMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string ContVenituri, string ContCheltuieli, string CotaTva,
    decimal Cantitate, decimal Pret, decimal Suma, decimal SumaTva);

record FlaxAvizIntrare(string Id, string Numar, DateTime Data, string PartenerId,
    string DepozitId, string NumarFactura, string SeriaFactura, decimal SumaDocument,
    string TipOperatiune);

record FlaxAvizIntrareMarfa(string DocumentId, int Linie, string NomenclatorId,
    string ContEvidenta, string CotaTva, decimal Cantitate, decimal Pret, decimal Suma,
    decimal SumaTva);

// -- ExtrasDeCont → PLT / INC per RÂND (§12.2) --
// Direcția e `SumaIntrare` XOR `SumaIesire` (verificat: niciun rând pe 2025 nu
// are ambele). Documentul STINS nu stă în `DocBaza` (acolo sunt doar ordinele de
// plată/încasare — 2.806 rânduri din 32.600), ci în subconto-ul 3 al contului
// corespondent (23.233 rânduri): de aceea se expun toate cele trei dimensiuni
// brute, iar alegerea rămâne a handler-ului (pasul 5).
record FlaxExtras(string Id, string Numar, DateTime Data, string ContBancarId,
    string ContDeBanca);

record FlaxExtrasRand(string DocumentId, int Linie, decimal SumaIntrare, decimal SumaIesire,
    string ContContrapartida, string TipOperatiuneId, string TipOperatiune, FlaxRef Partener,
    FlaxRef DocumentBaza, FlaxRef Dimensiune1, FlaxRef Dimensiune2, FlaxRef Dimensiune3,
    string Comentariu);

// -- Plata / Incasare → PLT / INC. Aceeași formă pentru ambele (`Sens` spune
// care view a produs rândul, ca handler-ul să nu depindă de apelant).
record FlaxTrezorerie(string Id, string Numar, DateTime Data, string CasierieId,
    string ContCasa, string ContBancarId, FlaxRef Partener, decimal SumaDocument,
    string TipOperatiune, string Explicatie);

record FlaxTrezorerieRand(string DocumentId, int Linie, FlaxRef DocumentBaza,
    decimal SumaPlatii, decimal SumaDeDecontari, decimal SumaTva, string ContDecontari);

// -- Compensare → NTC + Imperechere (§12.2) --
record FlaxCompensare(string Id, string Numar, DateTime Data, string ContDebit,
    string ContCredit, string PartenerDebitId, string PartenerCreditId, decimal SumaDocument,
    bool FaraRulaj);

record FlaxCompensareRand(string DocumentId, int Linie, FlaxRef DocumentBaza, decimal Suma,
    decimal Sold, string PartenerId);

// -- Operatia → NTC (postare explicită per linie, 46b) --
record FlaxOperatie(string Id, string Numar, DateTime Data, decimal SumaOperatia,
    string Explicatie);

record FlaxOperatieRand(string DocumentId, int Linie, string ContDebit, string ContCredit,
    decimal Suma, decimal CantitateDebit, decimal CantitateCredit, string Explicatie,
    string PartenerDebitId, string PartenerCreditId, string PersoanaDebitId,
    string PersoanaCreditId);

// -- Salarii / CasareMF / InchidereLunaDeExercitiu → NTC din registrul contabil.
// Doar antetul: rândurile lor vin din `RanduriNotaPeLuna` (§B), transcrise exact.
record FlaxDocumentSimplu(string Id, string Numar, DateTime Data);

// ======================= Registrul contabil per document =======================

record FlaxRandNota(int Linie, string ContDebit, string ContCredit, decimal Suma,
    decimal CantitateDebit, decimal CantitateCredit, string Explicatie);

// Un rând de subconto: `Fel` = tipul de analitic („Loturi", „Nomenclator",
// „Depozite", „Parteneri", „Documente"…), `Correspond` 0 = DEBIT / 1 = CREDIT.
// `ValoareText` acoperă analiticele care sunt ENUM în 1C, nu referință (cotele
// de TVA, impozitele) — acolo `Valoare` e null prin construcție.
record FlaxSubcontoNota(int Linie, int Correspond, string Fel, FlaxRef Valoare,
    string ValoareText);

// Volumul anului per view — intrarea verificării de acoperire din pre-flight.
record FlaxVolumAntet(string View, int Antete, int Postate);

// Linii de document fără cotă de TVA decodată — detectorul de view-uri STALE.
record FlaxLiniiFaraCota(string Sectiune, int Linii);

partial class FlaxDb {

    // ======================= Fereastra lunii =======================
    // Un singur filtru pentru toate cititoarele: luna documentului + Posted.
    // Secțiunile se alătură antetului (`h`) tocmai ca filtrul să rămână unul.

    const string FiltruLuna = "where h.DateTime >= @de and h.DateTime < @pana and h.Posted <> 0x00";
    const string OrdineAntete = "order by h.DateTime, h.Number, h.KeyField";
    const string OrdineLinii = "order by s.ParentRef, s.[LineNo]";

    static (string, object)[] Fereastra(int an, int luna) =>
        [("@de", new DateTime(an, luna, 1)), ("@pana", new DateTime(an, luna, 1).AddMonths(1))];

    // Data „goală" a lui 1C (0001-01-01) iese din view cu corecția de an aplicată
    // — adică exact `2001-01-01` (verificat: 43 de scadențe pe 2025 au fix
    // valoarea asta, niciuna alta sub 2010). Nu e NULL niciodată, deci fără
    // traducerea asta o scadență necompletată ar intra ca dată reală în trecut.
    static readonly DateTime DataGoala = new(2001, 1, 1);

    static DateTime Data(SqlDataReader r, int i) => r.GetDateTime(i);

    static DateTime? DataOpt(SqlDataReader r, int i) =>
        r.IsDBNull(i) || r.GetDateTime(i) == DataGoala ? null : r.GetDateTime(i);

    // ---- Referințe polimorfe ----
    // Ocupă 4 coloane consecutive: TypeRef (hex), numele tipului (CASE peste
    // coloanele tipizate ale view-ului), id-ul, descrierea.
    static string ColoaneRef(string prefix, string[] tipuri, string[] enumuri = null) {
        var cazuri = tipuri.Select(t => $"when {prefix}_{t}_ID is not null then '{t}'")
            .Concat((enumuri ?? []).Select(e => $"when {prefix}_{e} is not null then '{e}'"))
            .ToList();
        var nume = cazuri.Count == 0 ? "null" : "case " + string.Join(" ", cazuri) + " end";
        return $"convert(varchar(10), {prefix}_Type, 2), {nume}, {prefix}_Id, {prefix}_Desc";
    }

    static FlaxRef Referinta(SqlDataReader r, int i) {
        var tipRef = Text(r, i);
        // Capcana: `_Id` e non-null și plin de zerouri când referința e goală.
        return tipRef == null || tipRef.All(c => c == '0')
            ? null
            : new FlaxRef(tipRef, Text(r, i + 1), Hex(r, i + 2), Text(r, i + 3));
    }

    // Numele tipurilor de document care pot apărea ca ținte polimorfe. Listele
    // sunt PER VIEW (fiecare view expune doar ce a avut nevoie generatorul) —
    // o listă comună ar produce coloane inexistente.
    static readonly string[] TipuriDocVanzare =
        ["AvizDeIesire", "VanzareMarfuriSiServiciiPrestate", "RaportDeVanzariCuAmanunt"];
    static readonly string[] TipuriDocIntrare = [
        "ReturDeLaClient", "AvizDeIntrare", "MarireStocDeMarfuri",
        "AprovizionareMarfuriSiServiciiPrimite",
    ];
    static readonly string[] TipuriDocDecontare = [
        "VanzareMarfuriSiServiciiPrestate", "IntroducereaSoldurilor", "ReturDeLaClient",
        "ReturLaFurnizor", "AprovizionareMarfuriSiServiciiPrimite",
    ];
    static readonly string[] TipuriDocCompensare = [
        "AprovizionareMarfuriSiServiciiPrimite", "AvizDeIesire", "ExtrasDeCont", "Import",
        "Incasare", "IntroducereaSoldurilor", "Plata", "ReturDeLaClient", "ReturLaFurnizor",
        "VanzareMarfuriSiServiciiPrestate",
    ];

    // ======================= 1. Aprovizionare (FCT + NIR) =======================

    public List<FlaxAprovizionare> Aprovizionari(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.NumarFactura,
                        h.SeriaFactura, h.DataFacturii, h.DataScadenta, h.Depozit_ID,
                        h.SumaDocument, h.SumaIncludeTVA, h.TaxareInversa, h.CalcTVA,
                        h.PoliticaTVA, h.TipOperatiune, ltrim(rtrim(h.ValutaDoc)),
                        h.CursDeDecontari
                 from flax.AprovizionareMarfuriSiServiciiPrimite h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAprovizionare(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4),
                Text(r, 5), DataOpt(r, 6), DataOpt(r, 7), Hex(r, 8), Dec(r, 9), Bit(r, 10),
                Bit(r, 11), Bit(r, 12), Text(r, 13), Text(r, 14), Text(r, 15), Dec(r, 16)),
            Fereastra(an, luna));

    public List<FlaxAprovizionareMarfa> AprovizionariMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.CotaTVA, s.[Count], s.Pret, s.Suma, s.SumaTVA, s.Depozit_ID
                 from flax.AprovizionareMarfuriSiServiciiPrimite_Marfuri s
                 join flax.AprovizionareMarfuriSiServiciiPrimite h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxAprovizionareMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Text(r, 4), Dec(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8), Hex(r, 9)),
            Fereastra(an, luna));

    public List<FlaxAprovizionareServiciu> AprovizionariServicii(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, s.Explicatie, s.[Count],
                        s.Pret, s.Suma, s.CotaTVA, s.SumaTVA, ltrim(rtrim(s.ContCheltuieli)),
                        s.ExtDimension1_Partenerii_ID
                 from flax.AprovizionareMarfuriSiServiciiPrimite_Servicii s
                 join flax.AprovizionareMarfuriSiServiciiPrimite h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxAprovizionareServiciu(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Dec(r, 4), Dec(r, 5), Dec(r, 6), Text(r, 7), Dec(r, 8), Text(r, 9), Hex(r, 10)),
            Fereastra(an, luna));

    // ======================= 2. Vânzare (FCL + DSC) =======================

    public List<FlaxVanzare> Vanzari(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.NumarFactura,
                        h.SeriaFactura, h.DataScadenta, h.Depozit_ID, h.SumaDocument,
                        h.SumaIncludeTVA, h.TipOperatiune
                 from flax.VanzareMarfuriSiServiciiPrestate h {FiltruLuna} {OrdineAntete}",
            r => new FlaxVanzare(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4),
                Text(r, 5), DataOpt(r, 6), Hex(r, 7), Dec(r, 8), Bit(r, 9), Text(r, 10)),
            Fereastra(an, luna));

    public List<FlaxVanzareMarfa> VanzariMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.ContVenituri)), s.CotaTVA, s.[Count], s.Pret, s.Suma,
                        s.SumaTVA, s.Depozit_ID
                 from flax.VanzareMarfuriSiServiciiPrestate_Marfuri s
                 join flax.VanzareMarfuriSiServiciiPrestate h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxVanzareMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4),
                Text(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8), Dec(r, 9), Hex(r, 10)),
            Fereastra(an, luna));

    public List<FlaxVanzareServiciu> VanzariServicii(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, s.Explicatie, s.[Count],
                        s.Pret, s.Suma, s.CotaTVA, s.SumaTVA, ltrim(rtrim(s.ContVenituri))
                 from flax.VanzareMarfuriSiServiciiPrestate_Servicii s
                 join flax.VanzareMarfuriSiServiciiPrestate h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxVanzareServiciu(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Dec(r, 4),
                Dec(r, 5), Dec(r, 6), Text(r, 7), Dec(r, 8), Text(r, 9)),
            Fereastra(an, luna));

    // ======================= 3. Transfer (BTR) =======================

    public List<FlaxTransfer> Transferuri(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.DepozitExpeditor_ID,
                        h.DepozitDestinatar_ID
                 from flax.TransferDeMarfuri h {FiltruLuna} {OrdineAntete}",
            r => new FlaxTransfer(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Hex(r, 4)),
            Fereastra(an, luna));

    public List<FlaxTransferMarfa> TransferuriMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.NewContEvidenta)), s.[Count], s.Pret
                 from flax.TransferDeMarfuri_Marfuri s
                 join flax.TransferDeMarfuri h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxTransferMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4),
                Dec(r, 5), Dec(r, 6)),
            Fereastra(an, luna));

    // ======================= 4. Bon de consum (BCS) =======================

    public List<FlaxBonConsum> BonuriConsum(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Depozit_ID
                 from flax.BonDeConsum h {FiltruLuna} {OrdineAntete}",
            r => new FlaxBonConsum(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3)),
            Fereastra(an, luna));

    public List<FlaxBonConsumMaterial> BonuriConsumMateriale(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.ContCheltuieli)), s.[Count], s.Depozit_ID
                 from flax.BonDeConsum_Materiale s
                 join flax.BonDeConsum h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxBonConsumMaterial(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Text(r, 4), Dec(r, 5), Hex(r, 6)),
            Fereastra(an, luna));

    // ======================= 5. Mărire / Diminuare stoc (LDI ±) =======================

    public List<FlaxAjustareStoc> MaririStoc(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Depozit_ID, h.SumaDocument,
                        ltrim(rtrim(h.ContCheltuieli))
                 from flax.MarireStocDeMarfuri h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAjustareStoc(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Dec(r, 4),
                Text(r, 5), null),
            Fereastra(an, luna));

    public List<FlaxAjustareStocMarfa> MaririStocMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.CotaTVA, s.[Count], s.Pret, s.Suma
                 from flax.MarireStocDeMarfuri_Marfuri s
                 join flax.MarireStocDeMarfuri h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            CitesteAjustareMarfa, Fereastra(an, luna));

    // Simetria e ieftină: pe 2025 nu există niciun document de diminuare, dar
    // cititorul face ca apariția unuia să fie o linie de handler, nu o felie.
    public List<FlaxAjustareStoc> DiminuariStoc(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Depozit_ID, h.SumaDocument,
                        ltrim(rtrim(h.ContCheltuieli)), h.TipOperatiune
                 from flax.DiminuareStocDeMarfuri h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAjustareStoc(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Dec(r, 4),
                Text(r, 5), Text(r, 6)),
            Fereastra(an, luna));

    public List<FlaxAjustareStocMarfa> DiminuariStocMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.CotaTVA, s.[Count], s.Pret, s.Suma
                 from flax.DiminuareStocDeMarfuri_Marfuri s
                 join flax.DiminuareStocDeMarfuri h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            CitesteAjustareMarfa, Fereastra(an, luna));

    static FlaxAjustareStocMarfa CitesteAjustareMarfa(SqlDataReader r) =>
        new(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4), Dec(r, 5), Dec(r, 6),
            Dec(r, 7));

    // ======================= 6. Retur de la client (RDC) =======================

    public List<FlaxReturClient> RetururiClient(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.NumarFactura,
                        h.SeriaFactura, h.DataScadenta, h.Depozit_ID, h.SumaDocument,
                        h.SumaIncludeTVA, h.TipOperatiune,
                        {ColoaneRef("h.DocBaza", ["VanzareMarfuriSiServiciiPrestate"])}
                 from flax.ReturDeLaClient h {FiltruLuna} {OrdineAntete}",
            r => new FlaxReturClient(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4),
                Text(r, 5), DataOpt(r, 6), Hex(r, 7), Dec(r, 8), Bit(r, 9), Text(r, 10),
                Referinta(r, 11)),
            Fereastra(an, luna));

    public List<FlaxReturClientMarfa> RetururiClientMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.ContVenituri)), s.CotaTVA, s.[Count], s.Pret, s.Suma,
                        s.SumaTVA, s.SumaAchizitie, s.Depozit_ID,
                        {ColoaneRef("s.DocumentVanzare", TipuriDocVanzare)}
                 from flax.ReturDeLaClient_Marfuri s
                 join flax.ReturDeLaClient h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxReturClientMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4),
                Text(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8), Dec(r, 9), Dec(r, 10), Hex(r, 11),
                Referinta(r, 12)),
            Fereastra(an, luna));

    public List<FlaxReturClientServiciu> RetururiClientServicii(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, s.Explicatie, s.[Count],
                        s.Pret, s.Suma, s.CotaTVA, s.SumaTVA, ltrim(rtrim(s.ContVenituri)),
                        {ColoaneRef("s.DocumentVanzare", ["VanzareMarfuriSiServiciiPrestate"])}
                 from flax.ReturDeLaClient_Servicii s
                 join flax.ReturDeLaClient h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxReturClientServiciu(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Dec(r, 4), Dec(r, 5), Dec(r, 6), Text(r, 7), Dec(r, 8), Text(r, 9),
                Referinta(r, 10)),
            Fereastra(an, luna));

    // ======================= 7. Retur la furnizor (RLF) =======================

    public List<FlaxReturFurnizor> RetururiFurnizor(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.NumarFactura,
                        h.SeriaFactura, h.Depozit_ID, h.SumaDocument, h.TipDeRetur,
                        h.TipOperatiune
                 from flax.ReturLaFurnizor h {FiltruLuna} {OrdineAntete}",
            r => new FlaxReturFurnizor(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4),
                Text(r, 5), Hex(r, 6), Dec(r, 7), Text(r, 8), Text(r, 9)),
            Fereastra(an, luna));

    public List<FlaxReturFurnizorMarfa> RetururiFurnizorMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.CotaTVA, s.[Count], s.Pret, s.Suma, s.SumaTVA,
                        {ColoaneRef("s.DocumentIntrare", TipuriDocIntrare)}
                 from flax.ReturLaFurnizor_Marfuri s
                 join flax.ReturLaFurnizor h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxReturFurnizorMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Text(r, 4), Dec(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8), Referinta(r, 9)),
            Fereastra(an, luna));

    public List<FlaxReturFurnizorServiciu> RetururiFurnizorServicii(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, s.Explicatie, s.[Count],
                        s.Pret, s.Suma, s.CotaTVA, s.SumaTVA, ltrim(rtrim(s.ContCheltuieli)),
                        {ColoaneRef("s.DocumentIntrare", ["AprovizionareMarfuriSiServiciiPrimite"])}
                 from flax.ReturLaFurnizor_Servicii s
                 join flax.ReturLaFurnizor h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxReturFurnizorServiciu(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Dec(r, 4), Dec(r, 5), Dec(r, 6), Text(r, 7), Dec(r, 8), Text(r, 9),
                Referinta(r, 10)),
            Fereastra(an, luna));

    // ======================= 8. Asamblare / Dezasamblare (ASM) =======================

    public List<FlaxAsamblare> Asamblari(int an, int luna) => AntetAsamblare("Asamblare", an, luna);

    public List<FlaxAsamblare> Dezasamblari(int an, int luna) =>
        AntetAsamblare("Dezasamblare", an, luna);

    List<FlaxAsamblare> AntetAsamblare(string view, int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.DepozitArticole_ID,
                        h.DepozitSubasamble_ID
                 from flax.{view} h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAsamblare(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Hex(r, 4)),
            Fereastra(an, luna));

    // Asamblare: Articole = PRODUSUL (`Pret` = evaluarea lotului nou).
    public List<FlaxAsamblareLinie> AsamblariArticole(int an, int luna) =>
        LiniiAsamblare("Asamblare", "Articole", "s.Pret", "0", an, luna);

    // Asamblare: Subasamble = CONSUMURILE — fără preț, costul vine din loturi.
    public List<FlaxAsamblareLinie> AsamblariSubasamble(int an, int luna) =>
        LiniiAsamblare("Asamblare", "Subasamble", "0", "0", an, luna);

    // Dezasamblare: Articole = CONSUMUL (view-ul nici n-are coloană `Pret`).
    public List<FlaxAsamblareLinie> DezasamblariArticole(int an, int luna) =>
        LiniiAsamblare("Dezasamblare", "Articole", "0", "0", an, luna);

    // Dezasamblare: Subasamble = PRODUSELE, evaluate prin `CotaDeValoare`
    // (cheia de distribuție a valorii consumului între ele).
    public List<FlaxAsamblareLinie> DezasamblariSubasamble(int an, int luna) =>
        LiniiAsamblare("Dezasamblare", "Subasamble", "0", "s.CotaDeValoare", an, luna);

    List<FlaxAsamblareLinie> LiniiAsamblare(string view, string sectiune, string pret,
            string cota, int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.[Count], {pret}, s.Depozit_ID, {cota}
                 from flax.{view}_{sectiune} s
                 join flax.{view} h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxAsamblareLinie(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Dec(r, 4),
                Dec(r, 5), Hex(r, 6), Dec(r, 7)),
            Fereastra(an, luna));

    // ======================= 9. Raport de vânzări cu amănuntul (RVA) =======================

    public List<FlaxRaportAmanunt> RapoarteAmanunt(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Depozit_ID, h.Casierie_ID,
                        ltrim(rtrim(h.ContCasa)), h.SumaDocument, h.SumaIncludeTVA,
                        h.SumaNumerar, h.SumaCard, h.SumaCec, h.SumaTichetelorDeMasa,
                        h.SumaVirament, h.TipOperatiune
                 from flax.RaportDeVanzariCuAmanunt h {FiltruLuna} {OrdineAntete}",
            r => new FlaxRaportAmanunt(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Hex(r, 4),
                Text(r, 5), Dec(r, 6), Bit(r, 7), Dec(r, 8), Dec(r, 9), Dec(r, 10), Dec(r, 11),
                Dec(r, 12), Text(r, 13)),
            Fereastra(an, luna));

    public List<FlaxRaportAmanuntMarfa> RapoarteAmanuntMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.ContVenituri)), s.CotaTVA, s.[Count], s.Pret, s.Suma,
                        s.SumaTVA, s.Depozit_ID
                 from flax.RaportDeVanzariCuAmanunt_Marfuri s
                 join flax.RaportDeVanzariCuAmanunt h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxRaportAmanuntMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Text(r, 4), Text(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8), Dec(r, 9), Hex(r, 10)),
            Fereastra(an, luna));

    public List<FlaxRaportAmanuntServiciu> RapoarteAmanuntServicii(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, s.Explicatie, s.[Count],
                        s.Pret, s.Suma, s.CotaTVA, s.SumaTVA, ltrim(rtrim(s.ContVenituri))
                 from flax.RaportDeVanzariCuAmanunt_Servicii s
                 join flax.RaportDeVanzariCuAmanunt h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxRaportAmanuntServiciu(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3),
                Dec(r, 4), Dec(r, 5), Dec(r, 6), Text(r, 7), Dec(r, 8), Text(r, 9)),
            Fereastra(an, luna));

    public List<FlaxRaportAmanuntFactura> RapoarteAmanuntFacturi(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo],
                        {ColoaneRef("s.Factura",
                            ["VanzareMarfuriSiServiciiPrestate", "IntroducereaSoldurilor"])},
                        s.Partener_ID, s.BonFiscal, s.SumaFaraTVA, s.TVA, s.Suma
                 from flax.RaportDeVanzariCuAmanunt_Facturi s
                 join flax.RaportDeVanzariCuAmanunt h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxRaportAmanuntFactura(Hex(r, 0), Int(r, 1), Referinta(r, 2), Hex(r, 6),
                Text(r, 7), Dec(r, 8), Dec(r, 9), Dec(r, 10)),
            Fereastra(an, luna));

    public List<FlaxRaportAmanuntInchidere> RapoarteAmanuntInchidere(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.TipDePlata, s.Suma,
                        ltrim(rtrim(s.ContDeEvidenta))
                 from flax.RaportDeVanzariCuAmanunt_InchidereAmanunt s
                 join flax.RaportDeVanzariCuAmanunt h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxRaportAmanuntInchidere(Hex(r, 0), Int(r, 1), Text(r, 2), Dec(r, 3),
                Text(r, 4)),
            Fereastra(an, luna));

    // ======================= 10. Avize =======================

    public List<FlaxAvizIesire> AvizeIesire(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.Depozit_ID,
                        h.NumarFactura, h.SeriaFactura, h.SumaDocument, h.SumaIncludeTVA,
                        h.TipOperatiune
                 from flax.AvizDeIesire h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAvizIesire(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Hex(r, 4),
                Text(r, 5), Text(r, 6), Dec(r, 7), Bit(r, 8), Text(r, 9)),
            Fereastra(an, luna));

    public List<FlaxAvizIesireMarfa> AvizeIesireMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        ltrim(rtrim(s.ContVenituri)), ltrim(rtrim(s.ContCheltuieli)), s.CotaTVA,
                        s.[Count], s.Pret, s.Suma, s.SumaTVA
                 from flax.AvizDeIesire_Marfuri s
                 join flax.AvizDeIesire h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxAvizIesireMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4),
                Text(r, 5), Text(r, 6), Dec(r, 7), Dec(r, 8), Dec(r, 9), Dec(r, 10)),
            Fereastra(an, luna));

    public List<FlaxAvizIntrare> AvizeIntrare(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Partener_ID, h.Depozit_ID,
                        h.NumarFactura, h.SeriaFactura, h.SumaDocument, h.TipOperatiune
                 from flax.AvizDeIntrare h {FiltruLuna} {OrdineAntete}",
            r => new FlaxAvizIntrare(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Hex(r, 4),
                Text(r, 5), Text(r, 6), Dec(r, 7), Text(r, 8)),
            Fereastra(an, luna));

    public List<FlaxAvizIntrareMarfa> AvizeIntrareMarfuri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.Nomenclator_ID, ltrim(rtrim(s.ContEvidenta)),
                        s.CotaTVA, s.[Count], s.Pret, s.Suma, s.SumaTVA
                 from flax.AvizDeIntrare_Marfuri s
                 join flax.AvizDeIntrare h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxAvizIntrareMarfa(Hex(r, 0), Int(r, 1), Hex(r, 2), Text(r, 3), Text(r, 4),
                Dec(r, 5), Dec(r, 6), Dec(r, 7), Dec(r, 8)),
            Fereastra(an, luna));

    // ======================= 11. Extras de cont =======================

    public List<FlaxExtras> Extrase(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.ContBancar_ID,
                        ltrim(rtrim(h.ContDeBanca))
                 from flax.ExtrasDeCont h {FiltruLuna} {OrdineAntete}",
            r => new FlaxExtras(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4)),
            Fereastra(an, luna));

    static readonly string[] TipuriDimExtras1 = [
        "CheltuieliInAvans", "Departamente", "Casierii", "ImobilizariNecorporale",
        "ConturiBancare", "PersoaneFizice", "Partenerii",
    ];
    static readonly string[] TipuriDimExtras2 = [
        "Venituri", "Cheltuieli", "Partenerii", "Contracte", "GrupeNomenclator",
    ];
    static readonly string[] TipuriDimExtras3 = [
        "Import", "VanzareMarfuriSiServiciiPrestate", "IntroducereaSoldurilor",
        "ReturDeLaClient", "Cheltuieli", "ReturLaFurnizor",
        "AprovizionareMarfuriSiServiciiPrimite",
    ];

    public List<FlaxExtrasRand> ExtraseRanduri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], s.SumaIntrare, s.SumaIesire,
                        ltrim(rtrim(s.CorAccount)), s.TipOperatiune_ID, s.TipOperatiune,
                        {ColoaneRef("s.Partener", ["Partenerii"])},
                        {ColoaneRef("s.DocBaza", ["OrdinDeIncasare", "OrdinDePlata"])},
                        {ColoaneRef("s.ExtDimension1", TipuriDimExtras1, ["CoteTVA", "Impozite"])},
                        {ColoaneRef("s.ExtDimension2", TipuriDimExtras2, ["CoteTVA", "Impozite"])},
                        {ColoaneRef("s.ExtDimension3", TipuriDimExtras3, ["CoteTVA", "Impozite"])},
                        s.Comentariu
                 from flax.ExtrasDeCont_Details s
                 join flax.ExtrasDeCont h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxExtrasRand(Hex(r, 0), Int(r, 1), Dec(r, 2), Dec(r, 3), Text(r, 4),
                Hex(r, 5), Text(r, 6), Referinta(r, 7), Referinta(r, 11), Referinta(r, 15),
                Referinta(r, 19), Referinta(r, 23), Text(r, 27)),
            Fereastra(an, luna));

    // ======================= 12. Plata / Incasare =======================

    public List<FlaxTrezorerie> Plati(int an, int luna) => AntetTrezorerie("Plata", an, luna);

    public List<FlaxTrezorerie> Incasari(int an, int luna) => AntetTrezorerie("Incasare", an, luna);

    List<FlaxTrezorerie> AntetTrezorerie(string view, int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.Casierie_ID,
                        ltrim(rtrim(h.ContCasa)), h.ContBancar_ID,
                        {ColoaneRef("h.Partener", ["Casierii", "Partenerii"])},
                        h.SumaDocument, h.TipOperatiune, h.Explicatie
                 from flax.{view} h {FiltruLuna} {OrdineAntete}",
            r => new FlaxTrezorerie(Hex(r, 0), Text(r, 1), Data(r, 2), Hex(r, 3), Text(r, 4),
                Hex(r, 5), Referinta(r, 6), Dec(r, 10), Text(r, 11), Text(r, 12)),
            Fereastra(an, luna));

    public List<FlaxTrezorerieRand> PlatiRanduri(int an, int luna) =>
        RanduriTrezorerie("Plata", an, luna);

    public List<FlaxTrezorerieRand> IncasariRanduri(int an, int luna) =>
        RanduriTrezorerie("Incasare", an, luna);

    List<FlaxTrezorerieRand> RanduriTrezorerie(string view, int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo],
                        {ColoaneRef("s.DocBaza", TipuriDocDecontare)},
                        s.SumaPlatii, s.SumaDeDecontari, s.SumaTVA,
                        ltrim(rtrim(s.ContDeEvidentaDecontariCuPartener))
                 from flax.{view}_Details s
                 join flax.{view} h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxTrezorerieRand(Hex(r, 0), Int(r, 1), Referinta(r, 2), Dec(r, 6),
                Dec(r, 7), Dec(r, 8), Text(r, 9)),
            Fereastra(an, luna));

    // ======================= 13. Compensare =======================

    public List<FlaxCompensare> Compensari(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, ltrim(rtrim(h.ContDt)),
                        ltrim(rtrim(h.ContCr)), h.PartenerDt_ID, h.PartenerCr_ID,
                        h.SumaDocument, h.FaraRulaj
                 from flax.Compensare h {FiltruLuna} {OrdineAntete}",
            r => new FlaxCompensare(Hex(r, 0), Text(r, 1), Data(r, 2), Text(r, 3), Text(r, 4),
                Hex(r, 5), Hex(r, 6), Dec(r, 7), Bit(r, 8)),
            Fereastra(an, luna));

    public List<FlaxCompensareRand> CompensariDebit(int an, int luna) =>
        RanduriCompensare("Debit", an, luna);

    public List<FlaxCompensareRand> CompensariCredit(int an, int luna) =>
        RanduriCompensare("Credit", an, luna);

    List<FlaxCompensareRand> RanduriCompensare(string sectiune, int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo],
                        {ColoaneRef("s.DocBaza", TipuriDocCompensare)},
                        s.Suma, s.Sold, s.Partener_ID
                 from flax.Compensare_{sectiune} s
                 join flax.Compensare h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxCompensareRand(Hex(r, 0), Int(r, 1), Referinta(r, 2), Dec(r, 6),
                Dec(r, 7), Hex(r, 8)),
            Fereastra(an, luna));

    // ======================= 14. Operatia (nota manuală) =======================

    public List<FlaxOperatie> Operatii(int an, int luna) =>
        Query($@"select h.KeyField, h.Number, h.DateTime, h.SumaOperatia, h.Explicatie
                 from flax.Operatia h {FiltruLuna} {OrdineAntete}",
            r => new FlaxOperatie(Hex(r, 0), Text(r, 1), Data(r, 2), Dec(r, 3), Text(r, 4)),
            Fereastra(an, luna));

    // Din cele trei niveluri de subconto per latură se aduc doar cele care devin
    // DIMENSIUNI Atlas azi (Repartitor: partener sau persoană fizică); restul
    // (cheltuieli/venituri/contracte/departamente) n-au corespondent în modelul
    // privat — se adaugă aditiv dacă apare cerința (decizia 21).
    public List<FlaxOperatieRand> OperatiiRanduri(int an, int luna) =>
        Query($@"select s.ParentRef, s.[LineNo], ltrim(rtrim(s.AccountDr)),
                        ltrim(rtrim(s.AccountCr)), s.Suma, s.CountDr, s.CountCr,
                        coalesce(s.Explicatie_S, s.Explicatie_Desc),
                        s.ExtDimensionDr1_Partenerii_ID, s.ExtDimensionCr1_Partenerii_ID,
                        s.ExtDimensionDr1_PersoaneFizice_ID, s.ExtDimensionCr1_PersoaneFizice_ID
                 from flax.Operatia_Details s
                 join flax.Operatia h on h.KeyField = s.ParentRef
                 {FiltruLuna} {OrdineLinii}",
            r => new FlaxOperatieRand(Hex(r, 0), Int(r, 1), Text(r, 2), Text(r, 3), Dec(r, 4),
                Dec(r, 5), Dec(r, 6), Text(r, 7), Hex(r, 8), Hex(r, 9), Hex(r, 10), Hex(r, 11)),
            Fereastra(an, luna));

    // ======================= 15. Antete fără secțiuni proprii =======================
    // Salarii, CasareMF și închiderea de lună: rândurile lor se transcriu din
    // registrul contabil (§B), antetul dă doar identitatea + data + numărul.

    public List<FlaxDocumentSimplu> Salarii(int an, int luna) => Antet("Salarii", an, luna);

    public List<FlaxDocumentSimplu> CasariMF(int an, int luna) => Antet("CasareMF", an, luna);

    public List<FlaxDocumentSimplu> InchideriLuna(int an, int luna) =>
        Antet("InchidereLunaDeExercitiu", an, luna);

    List<FlaxDocumentSimplu> Antet(string view, int an, int luna) =>
        Query($"select h.KeyField, h.Number, h.DateTime from flax.{view} h {FiltruLuna} {OrdineAntete}",
            r => new FlaxDocumentSimplu(Hex(r, 0), Text(r, 1), Data(r, 2)),
            Fereastra(an, luna));

    // =============== B. Registrul contabil + subconto, per document ===============
    // Sursa identității de LOT pentru tipurile ale căror secțiuni NU poartă lotul
    // (BTR/BCS/LDI/DSC — 1C îl ține doar ca subconto) și sursa de transcriere a
    // notelor pentru familia NTC.
    //
    // Forma implicită e PE LUNĂ, indexată pe documentul-sursă: măsurat pe ianuarie
    // 2025, luna întreagă costă 0,15 s (31.797 rânduri de notă) + 0,67 s (170.573
    // rânduri de subconto), pe când aceleași date cerute document cu document
    // costă 13 ms + 57 ms FIECARE. Variantele per document rămân pentru
    // diagnostic punctual — nu au ce căuta în bucla lunii.
    //
    // Ordonarea e COMPLETĂ (până la departajare), nu doar pe `LineNo`: un rând de
    // notă are un subconto per FEL pe fiecare latură, iar fără departajare cele
    // două forme întorceau aceleași rânduri în ordine diferită (prins de
    // `--cititori`). Ordinea trebuie să fie a datelor, nu a planului de execuție —
    // altfel identitatea de lot ar depinde de calea pe care a citit-o handler-ul.

    static string SqlRanduriNota(string cheie, string filtru) => $@"
        select {cheie}, n.[LineNo], ltrim(rtrim(n.ContDebit)), ltrim(rtrim(n.ContCredit)),
               n.Suma, n.CountDt, n.CountCt, n.Explicatie_Desc
        from flax.NoteContabile n
        {filtru}
        order by n.[LineNo], n.ContDebit, n.ContCredit, n.Suma";

    static FlaxRandNota CitesteRandNota(SqlDataReader r) =>
        new(Int(r, 1), Text(r, 2), Text(r, 3), Dec(r, 4), Dec(r, 5), Dec(r, 6), Text(r, 7));

    public Dictionary<string, List<FlaxRandNota>> RanduriNotaPeLuna(int an, int luna) =>
        Grupeaza(Query(
            SqlRanduriNota("n.DocReferinta_Id", "where n.Period >= @de and n.Period < @pana"),
            r => (Hex(r, 0), CitesteRandNota(r)),
            Fereastra(an, luna)));

    public List<FlaxRandNota> RanduriNota(string recorderHexId) =>
        Query(SqlRanduriNota("n.DocReferinta_Id", "where n.DocReferinta_Id = @id"),
            CitesteRandNota, ("@id", DinHex(recorderHexId)));

    // Ținte posibile ale unui subconto. Catalogele + documentele care pot fi LOT
    // (`KindRef = "Loturi"` ⇒ `Value` = documentul care a creat lotul); cotele de
    // TVA și impozitele sunt ENUM-uri 1C, nu referințe — ies prin `ValoareText`.
    static readonly string[] TipuriSubconto = [
        "Casierii", "Cheltuieli", "CheltuieliInAvans", "Contracte", "ConturiBancare",
        "Departamente", "Depozite", "GrupeNomenclator", "ImobilizariCorporale",
        "ImobilizariNecorporale", "Nomenclator", "Partenerii", "PersoaneFizice",
        "TipuriDeOperatiuniTrezoreriale", "Venituri",
        "AprovizionareMarfuriSiServiciiPrimite", "AvizDeIesire", "AvizDeIntrare",
        "ExtrasDeCont", "Import", "Incasare", "IntroducereaSoldurilor", "MarireStocDeMarfuri",
        "Plata", "ReturDeLaClient", "ReturLaFurnizor", "TransferDeMarfuri",
        "VanzareMarfuriSiServiciiPrestate", "Asamblare", "Dezasamblare",
    ];

    static string SqlSubconto(string cheie, string filtru) => $@"
        select {cheie}, d.[LineNo], d.Correspond, ltrim(rtrim(d.KindRef)),
               {ColoaneRef("d.Value", TipuriSubconto, ["CoteTVA", "Impozite"])},
               coalesce(d.Value_CoteTVA, d.Value_Impozite)
        from flax.DefalcareNote d
        {filtru}
        order by d.[LineNo], d.Correspond, d.KindRef, d.Value_Id";

    static FlaxSubcontoNota CitesteSubconto(SqlDataReader r) =>
        new(Int(r, 1), Int(r, 2), Text(r, 3), Referinta(r, 4), Text(r, 8));

    public Dictionary<string, List<FlaxSubcontoNota>> SubcontoNotaPeLuna(int an, int luna) =>
        Grupeaza(Query(
            SqlSubconto("d.Recorder_Id", "where d.Period >= @de and d.Period < @pana"),
            r => (Hex(r, 0), CitesteSubconto(r)),
            Fereastra(an, luna)));

    public List<FlaxSubcontoNota> SubcontoNota(string recorderHexId) =>
        Query(SqlSubconto("d.Recorder_Id", "where d.Recorder_Id = @id"),
            CitesteSubconto, ("@id", DinHex(recorderHexId)));

    static Dictionary<string, List<T>> Grupeaza<T>(List<(string Cheie, T Element)> randuri) {
        var rezultat = new Dictionary<string, List<T>>(StringComparer.Ordinal);
        foreach (var (cheie, element) in randuri) {
            if (!rezultat.TryGetValue(cheie, out var lista))
                rezultat[cheie] = lista = [];
            lista.Add(element);
        }
        return rezultat;
    }

    // =============== C. Volumul anului per view (acoperirea cititorilor) ===============
    // Se compară cu recensământul Recorder (`TipuriRecorder`) în pre-flight:
    // documentele care POSTEAZĂ trebuie să fie o submulțime a antetelor Posted,
    // altfel filtrul `Posted <> 0x00` al cititorilor ar ascunde postări.

    public static readonly string[] ViewuriDocument = [
        "AprovizionareMarfuriSiServiciiPrimite", "VanzareMarfuriSiServiciiPrestate",
        "TransferDeMarfuri", "BonDeConsum", "MarireStocDeMarfuri", "DiminuareStocDeMarfuri",
        "ReturDeLaClient", "ReturLaFurnizor", "Asamblare", "Dezasamblare",
        "RaportDeVanzariCuAmanunt", "AvizDeIesire", "AvizDeIntrare",
        "ExtrasDeCont", "Plata", "Incasare", "Compensare",
        "Operatia", "Salarii", "CasareMF", "InchidereLunaDeExercitiu",
    ];

    public List<FlaxVolumAntet> VolumeAntete(int an) {
        var bucati = ViewuriDocument.Select(v =>
            $@"select '{v}', count(*), sum(case when Posted <> 0x00 then 1 else 0 end)
               from flax.{v} where DateTime >= @de and DateTime < @pana");
        return Query(string.Join("\n                union all\n                ", bucati),
            r => new FlaxVolumAntet(Text(r, 0), Int(r, 1), Int(r, 2)),
            ("@de", new DateTime(an, 1, 1)), ("@pana", new DateTime(an + 1, 1, 1)));
    }

    // =============== D. Detectorul de view-uri STALE (cote de 21%) ===============
    // `CotaTVA` din view-uri e decodată printr-un CASE peste GUID-urile cotelor,
    // fixat la generare: elementele create pentru cota de 21% (1 august 2025) NU
    // sunt în CASE, deci ies NULL. Ocolirea prin dicționar propriu de GUID-uri ar
    // rupe contractul de coloane (§2) — singura reparație corectă e regenerarea
    // view-urilor. Verificat: 0 linii pe lunile 1–7, 48.361 pe 8–12.

    static readonly (string View, string Sectiune)[] SectiuniCuCota = [
        ("AprovizionareMarfuriSiServiciiPrimite", "Marfuri"),
        ("AprovizionareMarfuriSiServiciiPrimite", "Servicii"),
        ("VanzareMarfuriSiServiciiPrestate", "Marfuri"),
        ("VanzareMarfuriSiServiciiPrestate", "Servicii"),
        ("ReturDeLaClient", "Marfuri"), ("ReturDeLaClient", "Servicii"),
        ("ReturLaFurnizor", "Marfuri"), ("ReturLaFurnizor", "Servicii"),
        ("RaportDeVanzariCuAmanunt", "Marfuri"), ("RaportDeVanzariCuAmanunt", "Servicii"),
    ];

    public List<FlaxLiniiFaraCota> LiniiFaraCotaTva(int an, int panaLa) {
        var bucati = SectiuniCuCota.Select(x =>
            $@"select '{x.View}_{x.Sectiune}', count(*)
               from flax.{x.View}_{x.Sectiune} s
               join flax.{x.View} h on h.KeyField = s.ParentRef
               {FiltruLuna} and s.CotaTVA is null");
        return Query(string.Join("\n                union all\n                ", bucati),
            r => new FlaxLiniiFaraCota(Text(r, 0), Int(r, 1)),
            ("@de", new DateTime(an, 1, 1)), ("@pana", new DateTime(an, 1, 1).AddMonths(panaLa)));
    }
}
