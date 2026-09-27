using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using Atlas.Conta.BackOffice.Module.Saft;
using N = Atlas.Conta.Nucleu;
using DevExtreme.AspNet.Data;

namespace Atlas.Conta.BackOffice.Module.Proiectii;


public sealed class JurnalTvaRand : IRandCuDocument {
    public Guid DocumentId { get; set; }
    Guid? IRandCuDocument.DocumentId => DocumentId;
    public string DocumentNumar { get; set; }
    public string DocumentTip { get; set; }
    public DateOnly Data { get; set; }
    public Guid DocumentFiscalId { get; set; }
    public DateOnly DataDocument { get; set; }
    public DateOnly? DataPrimire { get; set; }
    public DateOnly DataExigibilitate { get; set; }
    public DateOnly DataInregistrare { get; set; }
    public int PerioadaD394 { get; set; }
    public bool InversaTehnica { get; set; }
    public bool RegularizareD300 { get; set; }
    public int PerioadaAn { get; set; }
    public int PerioadaLuna { get; set; }

    public Guid? PartenerId { get; set; }
    public string PartenerDenumire { get; set; }
    public string PartenerCodFiscal { get; set; }

    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public string CodSafT { get; set; }

    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public bool Storno { get; set; }
}

public sealed class DecontTvaRand {
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public string CodSafT { get; set; }
    public int Randuri { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
}

public sealed class RectificativaTvaRand {
    public Guid DocumentId { get; set; }
    public string DocumentNumar { get; set; }
    public DateOnly DocumentData { get; set; }
    public DateOnly Data { get; set; }
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public bool Storno { get; set; }
    public DateTime ScrisLa { get; set; }
}

public sealed class RectificativaTva {
    public int An { get; set; }
    public int Luna { get; set; }
    public DateTime? ConfirmataLa { get; set; }
    public bool PerioadaDeschisa { get; set; }
    public bool EsteRectificativa { get; set; }
    public List<RectificativaTvaRand> Randuri { get; set; } = [];
    public List<DecontTvaRand> Agregat { get; set; } = [];
}

public static class TvaProiectii {

    public static IQueryable<FaptFiscal> IntreLuni(
        IQueryable<FaptFiscal> randuri, DateOnly? dataStart, DateOnly? dataEnd) =>
        Fiscale.IntreLuni(randuri, dataStart, dataEnd);

    public static IQueryable<RegistruTva> IntreLuni(
        IQueryable<RegistruTva> randuri, DateOnly? dataStart, DateOnly? dataEnd) {

        if (dataStart is DateOnly ds) {
            var de = ds.Year * 100 + ds.Month;
            randuri = randuri.Where(r => r.PerioadaAn * 100 + r.PerioadaLuna >= de);
        }
        if (dataEnd is DateOnly df) {
            var panaLa = df.Year * 100 + df.Month;
            randuri = randuri.Where(r => r.PerioadaAn * 100 + r.PerioadaLuna <= panaLa);
        }
        return randuri;
    }

    public static IQueryable<JurnalTvaRand> JurnalTva(
        IObjectSpace os, SensTva sens, DateOnly? dataStart = null, DateOnly? dataEnd = null) {

        var randuri = IntreLuni(
            Fiscale.Fapte(os).Where(r => r.Sens == sens), dataStart, dataEnd);

        var agregate = randuri
            .GroupBy(r => new {
                r.DocumentId, r.TipTvaId, r.PartenerId, r.Regim, r.Cota, r.DeImport, r.Storno,
                r.PerioadaAn, r.PerioadaLuna, r.PerioadaD394, r.DocumentFiscalId,
                r.DataDocument, r.DataPrimire, r.DataExigibilitate, r.DataInregistrare,
                r.InversaTehnica, r.RegularizareD300
            })
            .Select(g => new {
                g.Key.DocumentId,
                g.Key.TipTvaId,
                g.Key.PartenerId,
                g.Key.Regim,
                g.Key.Cota,
                g.Key.Storno,
                g.Key.PerioadaAn,
                g.Key.PerioadaLuna, g.Key.DeImport, g.Key.PerioadaD394, g.Key.DocumentFiscalId,
                g.Key.DataDocument, g.Key.DataPrimire, g.Key.DataExigibilitate, g.Key.DataInregistrare,
                g.Key.InversaTehnica, g.Key.RegularizareD300,
                Data = g.Min(r => r.Data),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            });


        return from a in agregate
               join d in os.GetObjectsQuery<Document>() on a.DocumentId equals d.ID into grupDoc
               from d in grupDoc.DefaultIfEmpty()
               join p in os.GetObjectsQuery<Repartitor>()
                   on a.PartenerId equals (Guid?)p.ID into grupPartener
               from p in grupPartener.DefaultIfEmpty()
               join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
               from t in grupTip.DefaultIfEmpty()
               select new JurnalTvaRand {
                   DocumentId = a.DocumentId,
                   DocumentNumar = d == null ? null : d.Numar,
                   DocumentTip = null,
                   Data = a.Data,
                   DocumentFiscalId = a.DocumentFiscalId,
                   DataDocument = a.DataDocument, DataPrimire = a.DataPrimire,
                   DataExigibilitate = a.DataExigibilitate, DataInregistrare = a.DataInregistrare,
                   PerioadaD394 = a.PerioadaD394, InversaTehnica = a.InversaTehnica,
                   RegularizareD300 = a.RegularizareD300,
                   PerioadaAn = a.PerioadaAn,
                   PerioadaLuna = a.PerioadaLuna,
                   PartenerId = a.PartenerId,
                   PartenerDenumire = p == null ? null : p.Denumire,
                   PartenerCodFiscal = p == null ? null : (p as Partener).CodFiscal,
                   TipTvaId = a.TipTvaId,
                   TipTvaCod = t == null ? null : t.Cod,
                   TipTvaDenumire = t == null ? null : t.Denumire,
                   Regim = a.Regim == RegimTva.Normal ? "Normal"
                       : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                       : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                       : a.Regim == RegimTva.Scutit ? "Scutit"
                       : "Neimpozabil",
                   Cota = a.Cota,
                   CodSafT = os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.Versiune == MapariFiscale.Versiune
                       && m.Sectiune == SectiuneTvaSaft.Facturi && m.TipTvaId == a.TipTvaId
                       && m.Regim == a.Regim && m.Cota == a.Cota && m.DeImport == a.DeImport
                       && m.Sens == sens && m.Rol == N.RolTva.Taxa).Select(m => m.TaxCode).FirstOrDefault(),
                   Baza = a.Baza,
                   Tva = a.Tva,
                   Storno = a.Storno
               };
    }

    public static SortingInfo[] OrdineJurnalTva() => new[] {
        OrdineLista.Crescator(nameof(JurnalTvaRand.Data)),
        OrdineLista.Crescator(nameof(JurnalTvaRand.DocumentId)),
        OrdineLista.Crescator(nameof(JurnalTvaRand.TipTvaId)),
        OrdineLista.Crescator(nameof(JurnalTvaRand.Storno))
    };

    public static IQueryable<DecontTvaRand> DecontTva(
        IObjectSpace os, DateOnly? dataStart = null, DateOnly? dataEnd = null) {

        var randuri = IntreLuni(Fiscale.Fapte(os), dataStart, dataEnd);

        var agregate = randuri
            .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota, r.DeImport })
            .Select(g => new {
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Regim,
                g.Key.Cota, g.Key.DeImport,
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            });

        return from a in agregate
               join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
               from t in grupTip.DefaultIfEmpty()
               select new DecontTvaRand {
                   Sens = a.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                   TipTvaId = a.TipTvaId,
                   TipTvaCod = t == null ? null : t.Cod,
                   TipTvaDenumire = t == null ? null : t.Denumire,
                   Regim = a.Regim == RegimTva.Normal ? "Normal"
                       : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                       : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                       : a.Regim == RegimTva.Scutit ? "Scutit"
                       : "Neimpozabil",
                   Cota = a.Cota,
                   CodSafT = os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.Versiune == MapariFiscale.Versiune
                       && m.Sectiune == SectiuneTvaSaft.Facturi && m.TipTvaId == a.TipTvaId
                       && m.Regim == a.Regim && m.Cota == a.Cota && m.DeImport == a.DeImport
                       && m.Sens == a.Sens && m.Rol == N.RolTva.Taxa).Select(m => m.TaxCode).FirstOrDefault(),
                   Randuri = a.Randuri,
                   Baza = a.Baza,
                   Tva = a.Tva
               };
    }

    public static SortingInfo[] OrdineDecontTva() => new[] {
        OrdineLista.Crescator(nameof(DecontTvaRand.Sens)),
        OrdineLista.Crescator(nameof(DecontTvaRand.TipTvaId)),
        OrdineLista.Crescator(nameof(DecontTvaRand.Cota)),
        OrdineLista.Crescator(nameof(DecontTvaRand.Regim))
    };

    public static RectificativaTva Rectificativa(IObjectSpace os, int an, int luna) {
        var rezultat = new RectificativaTva { An = an, Luna = luna };
        rezultat.PerioadaDeschisa = os.GetObjectsQuery<PerioadaFiscala>()
            .Any(p => p.An == an && p.Luna == luna && !p.Inchisa);
        var codPerioada = an * 100 + luna;
        var reper = os.GetObjectsQuery<DepunereDeclaratie>()
            .Where(d => d.Formular == FormularFiscal.D394 && d.Perioada == codPerioada)
            .Select(d => (DateTime?)d.ConfirmataLa).Max();
        rezultat.ConfirmataLa = reper;
        if (reper == null) return rezultat;
        var randuri = Fiscale.Fapte(os).Where(r => r.PerioadaD394 == codPerioada && r.ScrisLa > reper.Value);

        rezultat.Randuri = (from r in randuri
                            join d in os.GetObjectsQuery<Document>() on r.DocumentId equals d.ID into grupDoc
                            from d in grupDoc.DefaultIfEmpty()
                            join t in os.GetObjectsQuery<TipTva>() on r.TipTvaId equals t.ID into grupTip
                            from t in grupTip.DefaultIfEmpty()
                            select new RectificativaTvaRand {
                                DocumentId = r.DocumentId,
                                DocumentNumar = d == null ? null : d.Numar,
                                DocumentData = d == null ? default : d.Data,
                                Data = r.Data,
                                Sens = r.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                                TipTvaId = r.TipTvaId,
                                TipTvaCod = t == null ? null : t.Cod,
                                TipTvaDenumire = t == null ? null : t.Denumire,
                                Regim = r.Regim == RegimTva.Normal ? "Normal"
                                    : r.Regim == RegimTva.Capitalizat ? "Capitalizat"
                                    : r.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                                    : r.Regim == RegimTva.Scutit ? "Scutit"
                                    : "Neimpozabil",
                                Cota = r.Cota,
                                Baza = r.Baza,
                                Tva = r.Tva,
                                Storno = r.Storno,
                                ScrisLa = r.ScrisLa
                            })
            .OrderBy(x => x.ScrisLa).ThenBy(x => x.DocumentId).ThenBy(x => x.TipTvaId)
            .ToList();
        rezultat.EsteRectificativa = rezultat.Randuri.Count > 0;

        rezultat.Agregat = (from a in randuri
                                .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota, r.DeImport })
                                .Select(g => new {
                                    g.Key.Sens, g.Key.TipTvaId, g.Key.Regim, g.Key.Cota, g.Key.DeImport,
                                    Randuri = g.Count(), Baza = g.Sum(r => r.Baza), Tva = g.Sum(r => r.Tva)
                                })
                            join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
                            from t in grupTip.DefaultIfEmpty()
                            select new DecontTvaRand {
                                Sens = a.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                                TipTvaId = a.TipTvaId,
                                TipTvaCod = t == null ? null : t.Cod,
                                TipTvaDenumire = t == null ? null : t.Denumire,
                                Regim = a.Regim == RegimTva.Normal ? "Normal"
                                    : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                                    : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                                    : a.Regim == RegimTva.Scutit ? "Scutit"
                                    : "Neimpozabil",
                                Cota = a.Cota,
                                CodSafT = os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.Versiune == MapariFiscale.Versiune
                       && m.Sectiune == SectiuneTvaSaft.Facturi && m.TipTvaId == a.TipTvaId
                       && m.Regim == a.Regim && m.Cota == a.Cota && m.DeImport == a.DeImport
                       && m.Sens == a.Sens && m.Rol == N.RolTva.Taxa).Select(m => m.TaxCode).FirstOrDefault(),
                                Randuri = a.Randuri,
                                Baza = a.Baza,
                                Tva = a.Tva
                            })
            .OrderBy(x => x.Sens).ThenBy(x => x.TipTvaId).ThenBy(x => x.Cota).ThenBy(x => x.Regim)
            .ToList();
        return rezultat;
    }

    public static (int An, int Luna)? LunaExacta(DateOnly dataStart, DateOnly dataEnd) =>
        dataStart.Day == 1 && dataStart.Year == dataEnd.Year && dataStart.Month == dataEnd.Month
            && dataEnd.Day == DateTime.DaysInMonth(dataEnd.Year, dataEnd.Month)
            ? (dataStart.Year, dataStart.Month)
            : null;
}
