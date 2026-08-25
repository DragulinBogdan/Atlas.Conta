using Microsoft.Data.SqlClient;

namespace Import1C;

// Sursa FAZEI 1C (decizia 45a): view-urile SkyConta din schema [flax] a bazei
// `EServicesFlx` — CONTRACT DE COLOANE, nu dependență de codul 1C. Structura
// generică 1C (_DocumentXXX/_CatalogXXX) rămâne ascunsă în spatele lor.
// Identitatea 1C = `KeyField` binary(16) (_IDRRef); circulă ca hex uppercase.
//
// Igienă obligatorie (design §2): codurile sunt nchar CU SPAȚII (trim), iar
// balanțele au un rând per cont×VALUTĂ ⇒ agregare peste valute.
//
// Cititorul NU filtrează pe flag-uri — le EXPUNE, iar politica de filtrare
// aparține importului (pașii 2-4): ce e zgomot depinde de destinație, iar un
// filtru ascuns în reader ar face din date o decizie invizibilă (decizia 21).
// Atenție la două capcane de encoding verificate pe date:
//  * `Folder` e inversat față de intuiție — convenția 1C `_Folder`: 0x01 =
//    ELEMENT, 0x00 = GRUP (122/122 persoane fizice au 0x01). Expus `EsteElement`.
//  * `Marked` e normal: 0x01 = marcat la ștergere (30/104 depozite). Expus `Marcat`.

record FlaxDepozit(string Id, string Cod, string Denumire, string TipDepozit,
    bool EsteElement, bool Marcat);

record FlaxCasierie(string Id, string Cod, string Denumire, string Valuta);

record FlaxContBancar(string Id, string Cod, string Denumire, string Iban, string Banca,
    string Valuta, string ContDeEvidenta, bool Marcat);

record FlaxPersoana(string Id, string Cod, string Nume, string Cnp,
    bool EsteElement, bool Marcat);

// Identitatea fiscală (felia D394, D4-D1): `PersJurFiz` și `PoliticaTva` sunt
// NUMELE enum-urilor 1C, așa cum le traduce view-ul („PersJur"/„PersFiz",
// „TVAlaEmitere"/„TVAlaIncasare", null = necompletat). `TaraIso` = `CodAlfa2`
// din catalogul `Tari` (join pe `Tara_ID`), NU descrierea — descrierea e text
// liber („Germania", „ROMANIA"). `DataTva` e null când 1C ține data goală
// (2001-01-01 = data vidă 1C după deplasarea de +2000 a stocării).
record FlaxPartener(string Id, string Cod, string Denumire, string CodUnic, string RegCom,
    string PersJurFiz, bool Client, bool Furnizor,
    string Cnp, string TaraIso, string PoliticaTva, DateTime? DataTva,
    bool Nerezident, bool Intracomunitar, bool NuIncludeInDec394);

record FlaxNomenclator(string Id, string Cod, string Denumire, string UM,
    bool Produs, bool Serviciu, bool TaxareInversa, string CotaTva);

record FlaxCont(string Cod, string Denumire, bool Sintetic, bool Extrabilantier);

record FlaxSold(string Cont, decimal SoldIni);

record FlaxSoldPartener(string Cont, string PartenerId, string PartenerDesc, decimal SoldIni);

// O poziție BalantaNivel3 pe cont de stoc = un LOT (decizia 13): produsul
// (Valoare1=Nomenclator) × documentul care l-a creat (Valoare2, polimorf pe 11
// tipuri) × depozitul (Valoare3=Depozite).
record FlaxPozitieStoc(string Cont, string NomenclatorId, string NomenclatorDesc,
    string DocTip, string DocId, string DocDesc, string DepozitId, string DepozitDesc,
    decimal Cantitate, decimal Valoare);

// Măturile fazei PRE-FLIGHT (decizia 48c): tot ce ating mișcările anului, ÎNAINTE
// de primul document — codurile de cont și tipurile de document-sursă (Recorder).
record FlaxCodMiscare(string Cod, int Randuri, int Documente);

record FlaxTipRecorder(string TypeRef, string Nume, int Documente, int Randuri);

// Clasa e PARȚIALĂ: nomenclatoarele + balanțele + măturile pre-flight stau aici,
// cititorii documentelor anului (pasul 2 al feliei 1C-c) în `FlaxDocumente.cs`.
// Helper-ele private (Query/Text/Dec/Bit/Hex/Int) sunt comune ambelor jumătăți.
partial class FlaxDb(string connectionString) : IDisposable {
    readonly SqlConnection conn = Open(connectionString);

    static SqlConnection Open(string cs) {
        var c = new SqlConnection(cs);
        c.Open();
        return c;
    }

    public void Dispose() => conn.Dispose();

    List<T> Query<T>(string sql, Func<SqlDataReader, T> map, params (string Nume, object Valoare)[] parametri) {
        using var cmd = new SqlCommand(sql, conn);
        foreach (var (nume, valoare) in parametri)
            cmd.Parameters.AddWithValue(nume, valoare ?? DBNull.Value);
        using var reader = cmd.ExecuteReader();
        var result = new List<T>();
        while (reader.Read())
            result.Add(map(reader));
        return result;
    }

    static int Int(SqlDataReader r, int i) => r.IsDBNull(i) ? 0 : Convert.ToInt32(r.GetValue(i));
    static string Text(SqlDataReader r, int i) =>
        r.IsDBNull(i) ? null : r.GetValue(i).ToString().Trim() is { Length: > 0 } s ? s : null;
    static decimal Dec(SqlDataReader r, int i) => r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i));
    // Data 1C „vidă" = 0001-01-01, stocată cu +2000 ⇒ 2001-01-01 în SQL (view-ul
    // deplasează înapoi doar anii > 3000, deci vidul rămâne 2001-01-01) ⇒ null.
    static DateTime? DataVida(SqlDataReader r, int i) =>
        r.IsDBNull(i) ? null : r.GetDateTime(i) is var d && d <= new DateTime(2001, 1, 1) ? null : d;
    // binary(1) = flag 1C; DBNull și 0x00 sunt false.
    static bool Bit(SqlDataReader r, int i) =>
        !r.IsDBNull(i) && r.GetValue(i) is byte[] { Length: > 0 } b && b[0] != 0;

    // binary(16) → hex uppercase fără prefix (identitatea 1C ca text).
    public static string Hex(byte[] b) => b == null ? null : Convert.ToHexString(b);
    static string Hex(SqlDataReader r, int i) => r.IsDBNull(i) ? null : Hex((byte[])r.GetValue(i));
    static byte[] DinHex(string hex) => Convert.FromHexString(hex);

    // ============================ Nomenclatoare ============================

    public List<FlaxDepozit> Depozite() =>
        Query(@"select KeyField, ltrim(rtrim(Code)), Description, TipDepozit, Folder, Marked
                from flax.Depozite",
            r => new FlaxDepozit(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3), Bit(r, 4), Bit(r, 5)));

    public List<FlaxCasierie> Casierii() =>
        Query(@"select KeyField, Code, Description, Valuta from flax.Casierii",
            r => new FlaxCasierie(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3)));

    // ATENȚIE: `flax.ConturiBancare` ține conturile TUTUROR (5.065 rânduri) —
    // proprii sunt DOAR cele cu OwnerId pe Organizatii (46), restul aparțin
    // partenerilor. Filtrul e obligatoriu, nu e igienă opțională.
    public List<FlaxContBancar> ConturiBancareProprii() =>
        Query(@"select KeyField, ltrim(rtrim(Code)), Description, ContDeDecontare, Banc,
                       Valuta, ContDeEvidenta, Marked
                from flax.ConturiBancare
                where OwnerId_Organizatii_ID is not null",
            r => new FlaxContBancar(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3), Text(r, 4),
                Text(r, 5), Text(r, 6), Bit(r, 7)));

    public List<FlaxPersoana> PersoaneFizice() =>
        Query(@"select KeyField, ltrim(rtrim(Code)), Description, CNP, Folder, Marked
                from flax.PersoaneFizice",
            r => new FlaxPersoana(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3), Bit(r, 4), Bit(r, 5)));

    public List<FlaxCont> PlanConturi() =>
        Query(@"select ltrim(rtrim(Code)), Description, Sintetic, Extrabilantier
                from flax.PlanConturi",
            r => new FlaxCont(Text(r, 0), Text(r, 1), Bit(r, 2), Bit(r, 3)));

    // Importul LA CERERE (pașii următori): partenerii (129k) și nomenclatorul
    // (312k) se aduc pe măsură ce documentele îi referă — nu în bloc
    // (precedentul „doar cele referite de deschidere", decizia 34e).
    const string SelectPartener = @"select p.KeyField, ltrim(rtrim(p.Code)), p.Description, p.CodUnic, p.RegCom,
                       p.PersJurFiz, p.Client, p.Furnizor,
                       p.CNP, t.CodAlfa2, p.PoliticaTVA, p.DataLuariiInEvidentaTVA,
                       p.Nerezident, p.Intracomunitar, p.NuIncludeInDec394
                from flax.Partenerii p
                    left join flax.Tari t on t.KeyField = p.Tara_ID";

    static FlaxPartener CitestePartener(SqlDataReader r) =>
        new(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3), Text(r, 4),
            Text(r, 5), Bit(r, 6), Bit(r, 7),
            Text(r, 8), Text(r, 9), Text(r, 10), DataVida(r, 11),
            Bit(r, 12), Bit(r, 13), Bit(r, 14));

    public FlaxPartener PartenerDupaId(string hexId) =>
        Query(SelectPartener + " where p.KeyField = @id", CitestePartener,
            ("@id", DinHex(hexId))).SingleOrDefault();

    // Proba contractului de coloane (`--cititori`): aceeași proiecție, primele
    // rânduri — o coloană dispărută la regenerarea view-urilor cade aici, nu în
    // mijlocul unei luni.
    public List<FlaxPartener> ParteneriEsantion(int cate = 20) =>
        Query(SelectPartener.Replace("select p.KeyField", $"select top {cate} p.KeyField"), CitestePartener);

    public FlaxNomenclator NomenclatorDupaId(string hexId) =>
        Query(@"select KeyField, ltrim(rtrim(Code)), Description, UM,
                       Produs, Serviciu, TaxareInversa, CotaTVA
                from flax.Nomenclator where KeyField = @id",
            r => new FlaxNomenclator(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3),
                Bit(r, 4), Bit(r, 5), Bit(r, 6), Text(r, 7)),
            ("@id", DinHex(hexId))).SingleOrDefault();

    // ============================ Balanțele ============================
    // Un rând per cont×valută ⇒ SUM peste valute (lei = coloanele fără sufix).
    // Rândurile care se anulează la agregare se exclud — nu poartă informație.

    public List<FlaxSold> SolduriDeschidere(DateTime period) =>
        Query(@"select ltrim(rtrim(Cont)), sum(SoldIni)
                from flax.Balanta
                where Period = @p and Cont is not null
                group by ltrim(rtrim(Cont))
                having sum(SoldIni) <> 0",
            r => new FlaxSold(Text(r, 0), Dec(r, 1)),
            ("@p", period));

    public List<FlaxSoldPartener> SolduriPartener(DateTime period) =>
        Query(@"select ltrim(rtrim(Cont)), Valoare1_Id, max(Valoare1_Desc), sum(SoldIni)
                from flax.BalantaNivel1
                where Period = @p and Cont is not null and Valoare1_Partenerii_ID is not null
                group by ltrim(rtrim(Cont)), Valoare1_Id
                having sum(SoldIni) <> 0",
            r => new FlaxSoldPartener(Text(r, 0), Hex(r, 1), Text(r, 2), Dec(r, 3)),
            ("@p", period));

    // Stocul de deschidere per LOT. `Valoare2` (documentul creator) e polimorf
    // pe 11 tipuri — se păstrează ca pereche (tip, id) BRUTĂ; maparea pe
    // documentul Atlas e treaba apelantului. Pozițiile cu cantitate dar valoare
    // 0 (sau invers) se păstrează — sunt semnal, nu zgomot.
    public List<FlaxPozitieStoc> StocDeschidere(DateTime period) =>
        Query(@"select ltrim(rtrim(Cont)), Valoare1_Id, max(Valoare1_Desc),
                       Valoare2_Type, Valoare2_Id, max(Valoare2_Desc),
                       Valoare3_Id, max(Valoare3_Desc),
                       sum(SoldIniCantitate), sum(SoldIni)
                from flax.BalantaNivel3
                where Period = @p and Cont like '3%'
                  and Valoare1_Nomenclator_ID is not null
                  and Valoare3_Depozite_ID is not null
                group by ltrim(rtrim(Cont)), Valoare1_Id, Valoare2_Type, Valoare2_Id, Valoare3_Id
                having sum(SoldIni) <> 0 or sum(SoldIniCantitate) <> 0",
            r => new FlaxPozitieStoc(Text(r, 0), Hex(r, 1), Text(r, 2),
                Hex(r, 3), Hex(r, 4), Text(r, 5), Hex(r, 6), Text(r, 7),
                Dec(r, 8), Dec(r, 9)),
            ("@p", period));

    // ==================== Fine de lună (pasul 6: reconcilierea lunară) ====================
    //
    // `Period` e LUNAR (verificat: 12 rânduri pe 2025 + 2026-01/02), iar soldul de
    // ÎNCHIDERE al unei luni e `SoldIni` al perioadei URMĂTOARE — verificat pe date,
    // nu presupus: pe toate conturile lui ianuarie 2025 `SoldIni + Rulaj` al lunii
    // = `SoldIni` al lui februarie, cu diferență zero. Se folosește forma
    // „SoldIni al lunii următoare" fiindcă e ACEEAȘI citire ca la deschidere
    // (aceleași filtre, aceeași agregare peste valute) — o a doua formulă ar fi o
    // a doua sursă de adevăr pentru același număr.
    //
    // Consecință de acoperire: decembrie 2025 se reconciliază contra perioadei
    // 2026-01-01, care există în sursă. Un cont căzut la zero nu mai are rând în
    // luna următoare (`having <> 0`) ⇒ apare ca sold 0, ceea ce e corect.

    public List<FlaxSold> SolduriLaFineDeLuna(int an, int luna) =>
        SolduriDeschidere(new DateTime(an, luna, 1).AddMonths(1));

    public List<FlaxPozitieStoc> StocLaFineDeLuna(int an, int luna) =>
        StocDeschidere(new DateTime(an, luna, 1).AddMonths(1));

    public List<FlaxPozitieStoc> StocFaraIdentitateLaFineDeLuna(int an, int luna) =>
        StocFaraIdentitate(new DateTime(an, luna, 1).AddMonths(1));

    // Rândurile de registru ale ÎNCHIDERII DE LUNĂ 1C, agregate pe corespondență.
    // Sunt rândurile pe care importul le-a SĂRIT (HandlereNote: 4427 = 4426/4423/
    // 4424, design §6) — reconcilierea le recitește din sursă, nu din contorul
    // importului: contractul (2) e forcing function-ul TVA-ului structural P1,
    // deci trebuie să compare Atlas cu SURSA, nu cu propria contabilitate a ceea
    // ce a sărit.
    public List<(string ContDebit, string ContCredit, decimal Suma)> SumeInchidereLuna(int an, int luna) =>
        Query(@"select ltrim(rtrim(ContDebit)), ltrim(rtrim(ContCredit)), sum(Suma)
                from flax.NoteContabile
                where Period >= @de and Period < @pana
                  and DocReferinta_InchidereLunaDeExercitiu_ID is not null
                  and ContDebit is not null and ContCredit is not null
                group by ltrim(rtrim(ContDebit)), ltrim(rtrim(ContCredit))",
            r => (Text(r, 0), Text(r, 1), Dec(r, 2)),
            ("@de", new DateTime(an, luna, 1)), ("@pana", new DateTime(an, luna, 1).AddMonths(1)));

    // ============================ Pre-flight ============================
    // Măturile care se fac ÎNAINTEA primului document (decizia 48c): triajul se
    // face pe TOT ce atinge anul, într-un raport unic, nu descoperit în mers.

    // Toate codurile de cont atinse de mișcările anului, pe ambele laturi.
    // `count(distinct DocReferinta_Id)` dă volumul real (un cod pe 3 rânduri ale
    // aceluiași document nu e mai important decât unul pe 3 documente).
    public List<FlaxCodMiscare> CoduriConturiMiscari(int an) =>
        Query(@"select Cont, count(*), count(distinct DocId)
                from (
                    select ltrim(rtrim(ContDebit)) as Cont, DocReferinta_Id as DocId
                    from flax.NoteContabile
                    where Period >= @de and Period < @pana and ContDebit is not null
                    union all
                    select ltrim(rtrim(ContCredit)), DocReferinta_Id
                    from flax.NoteContabile
                    where Period >= @de and Period < @pana and ContCredit is not null
                ) x
                where Cont <> ''
                group by Cont",
            r => new FlaxCodMiscare(Text(r, 0), Int(r, 1), Int(r, 2)),
            ("@de", new DateTime(an, 1, 1)), ("@pana", new DateTime(an + 1, 1, 1)));

    // Rândurile din perioade REZIDUALE (1C parchează artefacte pe ani imposibili
    // — `3999-11` e cunoscut din Balanta, 47f). Se numără separat ca să nu treacă
    // invizibile prin filtrarea pe an, nu se importă.
    public int RanduriPerioadeReziduale() =>
        Query(@"select count(*) from flax.NoteContabile where year(Period) > 3000",
            r => Int(r, 0)).Single();

    // Tipurile de document-sursă (Recorder) care au generat note în an.
    // Identitatea 1C a tipului e `DocReferinta_Type` (TypeRef binary(4)); numele
    // vine din coloana tipizată nenulă a rândului — CONTRACT DE COLOANE, ca tot
    // cititorul (§2). Un TypeRef fără coloană proprie în view iese cu nume NULL:
    // e semnal (tip pe care view-ul nu-l expune), nu eroare de citit.
    public static readonly string[] TipuriCuColoana = [
        "AprovizionareMarfuriSiServiciiPrimite", "VanzareMarfuriSiServiciiPrestate",
        "TransferDeMarfuri", "BonDeConsum", "MarireStocDeMarfuri", "DiminuareStocDeMarfuri",
        "ExtrasDeCont", "Plata", "Incasare", "Compensare",
        "ReturDeLaClient", "ReturLaFurnizor", "Asamblare", "Dezasamblare",
        "RaportDeVanzariCuAmanunt", "AvizDeIesire", "AvizDeIntrare",
        "Operatia", "Salarii", "CasareMF", "InchidereLunaDeExercitiu", "Import",
        "BonFiscal", "Stornare", "IntroducereaSoldurilor", "IntroducereSolduriInitialeMF",
        "IncasareCard", "ReevaluareMF",
    ];

    // Tipurile pe care view-urile VECHI nu le expuneau deloc (nici coloană
    // tipizată, nici view propriu), dar care POSTEAZĂ. Generația 26.07.2026 le-a
    // adăugat (view + coloană tipizată — sunt acum și în `TipuriCuColoana`, iar
    // antetul se citește tipizat prin `AnteteFostRaw`); dicționarul rămâne DOAR
    // fallback pe o bază cu view-uri vechi: un TypeRef fără coloană tipizată își
    // ia numele de aici, iar antetul cade pe structura generică (`AnteteRaw`).
    // Convenția 1C care le leagă (verificată empiric): TypeRef-ul E numărul
    // tabelei — 0x1DD1 = 7633 ⇒ `_Document7633`, 0x18C0 = 6336 ⇒ `_Document6336`.
    public static readonly IReadOnlyDictionary<string, (string Nume, int Tabela)> TipuriFaraColoana =
        new Dictionary<string, (string, int)>(StringComparer.OrdinalIgnoreCase) {
            ["00001DD1"] = ("IncasareCard", 7633),
            ["000018C0"] = ("ReevaluareMF", 6336),
        };

    // ---- Rezistența la REGENERAREA view-urilor SkyConta ----
    // Regenerarea din 25.07.2026 (precondiția 1C-d) a schimbat FORMA sursei, nu
    // doar conținutul ei: `DiminuareStocDeMarfuri` și-a pierdut și coloana
    // `DocReferinta_..._ID` din `NoteContabile`, și coloana `Posted` din view-ul
    // propriu (rămâne un antet fără flag de postare, cu 3 rânduri, niciunul în
    // 2025 — tipul are zero documente pe anul importat, ca la prima rulare).
    // O coloană inexistentă face interogarea să pice cu „Invalid column name",
    // adică toată unealta moare la pre-flight din cauza unui tip MORT.
    //
    // Regula adoptată: declarațiile de mai sus NU se scurtează (contractul de
    // coloane §2 rămâne ce ne așteptăm să găsim), dar se INTERSECTEAZĂ cu ce
    // expune baza acum, iar diferența se STRIGĂ o dată. Un tip declarat pe care
    // sursa nu-l mai expune devine astfel semnalul pe care designul îl prevede
    // (nume NULL în recensământ, zero antete), nu o oprire brutală.
    readonly Dictionary<string, bool> coloanaExista = new(StringComparer.OrdinalIgnoreCase);

    bool AreColoana(string obiect, string coloana) {
        var cheie = $"{obiect}.{coloana}";
        if (coloanaExista.TryGetValue(cheie, out var exista))
            return exista;
        return coloanaExista[cheie] = Query(
            "select count(*) from sys.columns where object_id = object_id(@o) and name = @c",
            r => Int(r, 0), ("@o", $"flax.{obiect}"), ("@c", coloana)).Single() > 0;
    }

    // Un view de document e citibil doar dacă poartă flagul de postare: filtrul
    // `Posted <> 0x00` e în toate interogările de antet (`FiltruLuna`).
    public bool ViewPostabil(string view) {
        if (AreColoana(view, "Posted"))
            return true;
        if (viewuriNepostabile.Add(view))
            Console.WriteLine($"  ATENȚIE: view-ul flax.{view} nu mai expune coloana `Posted` "
                + "(regenerare SkyConta) — tipul se tratează ca fiind FĂRĂ antete în fereastra "
                + "importată. Dacă sursa capătă documente de tipul ăsta, se pierd TĂCUT: "
                + "cititorul lui trebuie refăcut pe forma nouă a view-ului.");
        return false;
    }

    readonly HashSet<string> viewuriNepostabile = new(StringComparer.OrdinalIgnoreCase);

    // Lista declarată mai sus, INTERSECTATĂ cu ce expune view-ul chiar acum.
    // O regenerare a view-urilor SkyConta poate scoate coloane (25.07.2026:
    // `DiminuareStocDeMarfuri` și `Stornare` au dispărut din `NoteContabile`, deși
    // view-urile lor proprii există), iar un `case` pe o coloană inexistentă face
    // interogarea să pice cu „Invalid column name" — adică toată unealta moare la
    // pre-flight. Filtrarea o transformă în exact semnalul pe care designul îl
    // prevede: tipul rămâne DECLARAT, dar iese cu nume NULL, iar recensământul de
    // pre-flight îl raportează ca tip pe care view-ul nu-l expune. Declarația nu
    // se scurtează niciodată tăcut — tipurile pierdute se listează o dată.
    string[] tipuriCuColoanaVii;

    string[] TipuriCuColoanaVii() {
        if (tipuriCuColoanaVii != null)
            return tipuriCuColoanaVii;
        tipuriCuColoanaVii = TipuriCuColoana
            .Where(t => AreColoana("NoteContabile", $"DocReferinta_{t}_ID")).ToArray();
        var lipsa = TipuriCuColoana.Except(tipuriCuColoanaVii, StringComparer.Ordinal).ToList();
        if (lipsa.Count > 0)
            Console.WriteLine($"  ATENȚIE: view-ul flax.NoteContabile nu mai expune coloana "
                + $"DocReferinta_<tip>_ID pentru: {string.Join(", ", lipsa)} (regenerare SkyConta) "
                + "— tipurile rămân declarate, dar rândurile lor (dacă apar) ies fără nume în "
                + "recensământ, iar pre-flight-ul le raportează ca tipuri necunoscute.");
        return tipuriCuColoanaVii;
    }

    public List<FlaxTipRecorder> TipuriRecorder(int an) {
        var cazuri = string.Join("\n                                 ", TipuriCuColoanaVii()
            .Select(t => $"when DocReferinta_{t}_ID is not null then '{t}'"));
        return Query($@"select convert(varchar(10), DocReferinta_Type, 2),
                               max(case {cazuri} end),
                               count(distinct DocReferinta_Id), count(*)
                        from flax.NoteContabile
                        where Period >= @de and Period < @pana
                        group by DocReferinta_Type",
            r => new FlaxTipRecorder(Text(r, 0), Text(r, 1), Int(r, 2), Int(r, 3)),
            ("@de", new DateTime(an, 1, 1)), ("@pana", new DateTime(an + 1, 1, 1)))
            .Select(t => t.Nume == null && t.TypeRef != null
                    && TipuriFaraColoana.TryGetValue(t.TypeRef, out var cunoscut)
                ? t with { Nume = cunoscut.Nume } : t)
            .ToList();
    }

    // Reversul lui StocDeschidere: pozițiile de pe conturi de stoc cărora le
    // LIPSEȘTE produsul sau depozitul. Un lot fără produs nu poate exista în
    // Atlas (decizia 13) ⇒ nu sunt importabile — dar sunt bani reali în soldul
    // contabil, deci se citesc separat ca să se poată RAPORTA, nu ascunde
    // (contractul de reconciliere, 34f/45e).
    public List<FlaxPozitieStoc> StocFaraIdentitate(DateTime period) =>
        Query(@"select ltrim(rtrim(Cont)), Valoare1_Id, max(Valoare1_Desc),
                       Valoare2_Type, Valoare2_Id, max(Valoare2_Desc),
                       Valoare3_Id, max(Valoare3_Desc),
                       sum(SoldIniCantitate), sum(SoldIni)
                from flax.BalantaNivel3
                where Period = @p and Cont like '3%'
                  and (Valoare1_Nomenclator_ID is null or Valoare3_Depozite_ID is null)
                group by ltrim(rtrim(Cont)), Valoare1_Id, Valoare2_Type, Valoare2_Id, Valoare3_Id
                having sum(SoldIni) <> 0 or sum(SoldIniCantitate) <> 0",
            r => new FlaxPozitieStoc(Text(r, 0), Hex(r, 1), Text(r, 2),
                Hex(r, 3), Hex(r, 4), Text(r, 5), Hex(r, 6), Text(r, 7),
                Dec(r, 8), Dec(r, 9)),
            ("@p", period));
}
