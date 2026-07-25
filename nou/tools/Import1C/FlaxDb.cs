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

record FlaxPartener(string Id, string Cod, string Denumire, string CodUnic, string RegCom,
    string PersJurFiz, bool Client, bool Furnizor);

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

class FlaxDb(string connectionString) : IDisposable {
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

    static string Text(SqlDataReader r, int i) =>
        r.IsDBNull(i) ? null : r.GetValue(i).ToString().Trim() is { Length: > 0 } s ? s : null;
    static decimal Dec(SqlDataReader r, int i) => r.IsDBNull(i) ? 0m : Convert.ToDecimal(r.GetValue(i));
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
    public FlaxPartener PartenerDupaId(string hexId) =>
        Query(@"select KeyField, ltrim(rtrim(Code)), Description, CodUnic, RegCom,
                       PersJurFiz, Client, Furnizor
                from flax.Partenerii where KeyField = @id",
            r => new FlaxPartener(Hex(r, 0), Text(r, 1), Text(r, 2), Text(r, 3), Text(r, 4),
                Text(r, 5), Bit(r, 6), Bit(r, 7)),
            ("@id", DinHex(hexId))).SingleOrDefault();

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
