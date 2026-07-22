using Microsoft.Data.SqlClient;

namespace Migrare;

// Extrasul legacy pentru pasul 4 (decizia 12): NU istoricul de documente, ci
// deschiderea materializată de trecerea de an legacy în baza anului nou —
// nomenclatoarele + `solduri_repartitori` + LDI-ul administrativ de 31.12
// (spPreiaStocuriInitiale) + loturile (gest_gnmcl).

record LegacyRepartitor(int Id, string Nume, string Cont, string CodFiscal, string RegComert,
    bool GestInt, IReadOnlyList<int> Tipuri);

record LegacyCasierie(int Id, string Denumire, string ContCorespondent, bool EsteBanca);

record LegacyCod(string Cod, string Denumire);

record LegacyEntitate(int Id, string Denumire);

record LegacyPerioada(int An, int Luna, bool Inchisa);

record LegacySold(string Cont, decimal Debitor, decimal Creditor, int? RepartitorId,
    string CodFunctional, string CodEconomic, int? UnitateId, int? ProiectId);

// O linie a LDI-ului administrativ = un lot cu sold de deschidere.
record LegacyStocDeschidere(int Codmat, decimal Cantitate, decimal PretUnitar, decimal Valoare,
    int? TipMaterialId, int PredatorId, string CodFunctional, string CodEconomic,
    int? UnitateId, int? ProiectId);

record LegacyLot(int Codmat, int? SumatorId, string Denumire, string UM, DateTime? DataCod,
    DateTime? DataExpirare, string LotFabricatie, int? TipMaterialId);

record LegacyProdus(int Id, string Denumire, string UM, int? TipMaterialId);

class LegacyDb(string connectionString) : IDisposable {
    readonly SqlConnection conn = Open(connectionString);

    static SqlConnection Open(string cs) {
        var c = new SqlConnection(cs);
        c.Open();
        return c;
    }

    public void Dispose() => conn.Dispose();

    List<T> Query<T>(string sql, Func<SqlDataReader, T> map) {
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        var result = new List<T>();
        while (reader.Read())
            result.Add(map(reader));
        return result;
    }

    static string Text(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetValue(i).ToString().Trim() is { Length: > 0 } s ? s : null;
    static int? Int(SqlDataReader r, int i) => r.IsDBNull(i) ? null : Convert.ToInt32(r.GetValue(i));
    static decimal Dec(SqlDataReader r, int i) => r.IsDBNull(i) ? 0m : r.GetDecimal(i);
    static DateTime? Data(SqlDataReader r, int i) => r.IsDBNull(i) ? null : r.GetDateTime(i);
    static bool Bit(SqlDataReader r, int i) => !r.IsDBNull(i) && Convert.ToBoolean(r.GetValue(i));

    // Anul de deschidere = anul soldurilior materializate de trecerea de an.
    public int AnDeschidere() =>
        Query("select isnull(max(an), 0) from solduri_repartitori", r => r.GetInt32(0)).Single();

    public List<LegacyRepartitor> Repartitori() {
        var tipuri = Query("select ID_REPARTITORI, ID_REPARTITORI_TIPURI from REPARTITORI_CLASIFICATI",
                r => (Rep: r.GetInt32(0), Tip: r.GetInt32(1)))
            .GroupBy(x => x.Rep)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<int>)[.. g.Select(x => x.Tip)]);
        return Query("select ID_REPARTITORI, NUME, CONT, COD_FISCAL, REG_COMERT, GESTINT from REPARTITORI",
            r => new LegacyRepartitor(r.GetInt32(0), Text(r, 1), Text(r, 2), Text(r, 3), Text(r, 4),
                Bit(r, 5), tipuri.GetValueOrDefault(r.GetInt32(0), [])));
    }

    public List<LegacyCasierie> Casierie() =>
        Query("select COD_CB, DENUMIRE, CRSP_LEI, IS_BANCA from casierie",
            r => new LegacyCasierie(r.GetInt32(0), Text(r, 1), Text(r, 2), Bit(r, 3)));

    // Codurile bugetare: DISTINCT pe cod (planurile legacy repetă codul per tip
    // de buget); prima denumire întâlnită ajunge în nomenclator.
    public List<LegacyCod> CoduriFunctionale() =>
        Query(@"select ltrim(rtrim(COD_FUNCTIONAL)), max(DENUMIRE) from BG_PLAN_FUNCTIONAL
                where COD_FUNCTIONAL is not null group by ltrim(rtrim(COD_FUNCTIONAL))",
            r => new LegacyCod(Text(r, 0), Text(r, 1))).Where(c => c.Cod != null).ToList();

    public List<LegacyCod> CoduriEconomice() =>
        Query(@"select ltrim(rtrim(COD_ECONOMIC)), max(DENUMIRE) from BG_PLAN_ECONOMIC
                where COD_ECONOMIC is not null group by ltrim(rtrim(COD_ECONOMIC))",
            r => new LegacyCod(Text(r, 0), Text(r, 1))).Where(c => c.Cod != null).ToList();

    public List<LegacyEntitate> Unitati() =>
        Query("select id_oi_unitati, Denumire from OI_UNITATI",
            r => new LegacyEntitate(r.GetInt32(0), Text(r, 1)));

    public List<LegacyEntitate> Proiecte() =>
        Query("select id_oi_proiecte, Denumire from OI_PROIECTE",
            r => new LegacyEntitate(r.GetInt32(0), Text(r, 1)));

    public List<LegacyPerioada> Perioade() =>
        Query("select year(DATA_START), month(DATA_START), INCHISA from PERIOADE_FISCALE",
            r => new LegacyPerioada(r.GetInt32(0), r.GetInt32(1), Bit(r, 2)));

    // Maparea legacy id tip material → simbolul de cont (Cod-ul TipMaterial nou).
    public Dictionary<int, string> TipuriMaterial() =>
        Query("select ID_GEST_TIP_MATERIAL, CONT from GEST_TIP_MATERIAL where CONT is not null",
            r => (Id: r.GetInt32(0), Cont: Text(r, 1)))
            .Where(x => x.Cont != null)
            .ToDictionary(x => x.Id, x => x.Cont);

    public List<LegacySold> Solduri() =>
        Query(@"select CONT, SOLD_DEBITOR, SOLD_CREDITOR, ID_REPARTITORI,
                       COD_FUNCTIONAL, COD_ECONOMIC, ID_OI_UNITATI, ID_OI_PROIECTE
                from solduri_repartitori where CONT is not null",
            r => new LegacySold(Text(r, 0), Dec(r, 1), Dec(r, 2), Int(r, 3),
                Text(r, 4), Text(r, 5), Int(r, 6), Int(r, 7)));

    // Liniile LDI-ului administrativ de 31.12.(an-1) — stocul de deschidere
    // scris de spPreiaStocuriInitiale în baza anului nou.
    public List<LegacyStocDeschidere> StocDeschidere(DateTime dataDeschidere) =>
        Query($@"select i.CODMAT, i.CANTITATE, i.PRET_UNITAR, i.VALOARE_RECEPTIE_TVA,
                        i.ID_GEST_TIP_MATERIAL, d.ID_PREDATOR,
                        i.COD_FUNCTIONAL, i.COD_ECONOMIC, i.ID_OI_UNITATI, i.ID_OI_PROIECTE
                 from gest_itemsi i
                 join gest_docum d on d.id_gest_docum = i.id_gest_docum
                 join gest_tip_docum t on t.id_gest_tip_docum = d.id_gest_tip_docum
                 where t.COD_DOCUM = 'LDI' and d.DATA_DOCUM = '{dataDeschidere:yyyy-MM-dd}'
                   and d.STARE = 1 and i.STARE = 1 and i.CODMAT is not null",
            r => new LegacyStocDeschidere(r.GetInt32(0), Dec(r, 1), Dec(r, 2), Dec(r, 3),
                Int(r, 4), r.GetInt32(5), Text(r, 6), Text(r, 7), Int(r, 8), Int(r, 9)));

    public Dictionary<int, LegacyLot> Loturi() =>
        Query(@"select CODMAT, ID_GEST_SUMATOR, DENMAT, UM, DATA_COD, DATA_EXPIRARE,
                       LOT_FABRICATIE, ID_GEST_TIP_MATERIAL
                from gest_gnmcl",
            r => new LegacyLot(r.GetInt32(0), Int(r, 1), Text(r, 2), Text(r, 3),
                Data(r, 4), Data(r, 5), Text(r, 6), Int(r, 7)))
            .ToDictionary(l => l.Codmat);

    public Dictionary<int, LegacyProdus> Produse() =>
        Query("select ID_GEST_SUMATOR, DENMAT, UM, ID_GEST_TIP_MATERIAL from gest_sumator",
            r => new LegacyProdus(r.GetInt32(0), Text(r, 1), Text(r, 2), Int(r, 3)))
            .ToDictionary(p => p.Id);
}
