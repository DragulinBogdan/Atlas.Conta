using System.Diagnostics;
using System.Text;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// X-D5 / X-D3: scara transversală — k unități în luna măsurată, m luni închise de istoric, faptele „o dată per bază”
// în prima lună; reconcilierea integrală, INV-CUB și ASM-B7 rulează pe baza de volum înaintea purjei.
sealed partial class PerfCub(Func<IObjectSpace> deschide, Action<string, bool> check, bool privat,
        Action<IObjectSpace, int, int> inchide, int an, int istoric, int unitatiIstoric, int[] trepte,
        Func<PerfCub.Punct, IReadOnlyList<PerfCub.Masura>> masoaraProces, string director, DbContextOptions<BackOfficeEFCoreDbContext> optiuni)
    : ScenaDocumente(deschide, check, privat, inchide, "PC" + istoric, an) {

    public const string Utilizator = "E2E-PERF-ADMIN";

    public List<Masura> Masuri { get; } = [];
    public List<DurataComanda> Comenzi { get; } = [];
    public Volum VolumFinal { get; private set; }
    protected override int UltimulAn => An + 1;

    string Venit => Privat ? "704" : "751.01.00";
    string Activ => Privat ? "214" : "214.00.00";
    string Capital => Privat ? "1012" : "117.00.00";
    string Tva21 => Privat ? "N21" : null;
    (int An, int Luna) Masurata => istoric == 12 ? (An + 1, 1) : (An, Math.Max(istoric, 1) + 1);
    int numarProdus, treapta;
    readonly string[] filtru = Environment.GetEnvironmentVariable("PERF_CUB_OPERATII") is { Length: > 0 } f ? f.Split(',') : null;
    Guid furnizorOdata, partenerNota;
    readonly List<LinieScena> loturiLuna = [];
    readonly List<Guid> loturiDeschise = [];

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja pj) {
        var deschideri = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId == null).Select(t => t.ID).ToList();
        pj.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => deschideri.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        pj.AdaugaCheie<C.Tranzactie>(deschideri);
    }

    protected override void Executa() {
        var societate = CuSpatiu(os => os.GetObjectsQuery<Societate>().Select(x => new {
            x.CodFiscal, x.InregistratTva, x.Tara, x.Denumire, x.ContactNume, x.ContactPrenume, x.Telefon, x.ContBancarId,
        }).First());
        var iban = Privat ? CuSpatiu(os => os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban) : null;
        Comanda(os => {
            if (Privat) {
                var soc = os.GetObjectsQuery<Societate>().First();
                var banca = os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA");
                banca.Iban = "RO49AAAA1B31007593840000";
                soc.CodFiscal = "12345674"; soc.InregistratTva = true; soc.Tara = "RO";
                soc.Denumire = "Atlas Probă PerfCub SRL"; soc.ContactNume = "Popescu"; soc.ContactPrenume = "Ion";
                soc.Telefon = "0264000000"; soc.ContBancarId = banca.ID;
            }
            foreach (var (a, l) in Enumerable.Range(3, 10).Select(l => (An, l)).Concat(Enumerable.Range(1, 12).Select(l => (An + 1, l)))) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = a; p.Luna = l;
            }
            if (os.FirstOrDefault<ApplicationUser>(u => u.UserName == Utilizator) == null) {
                var rol = os.CreateObject<PermissionPolicyRole>(); rol.Name = Utilizator; rol.IsAdministrative = true;
                var u = os.CreateObject<ApplicationUser>(); u.UserName = Utilizator; u.SetPassword(""); u.Roles.Add(rol);
                os.CommitChanges();
                ((ISecurityUserWithLoginInfo)u).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, os.GetKeyValueAsString(u));
            }
            os.CommitChanges();
        });
        try { Scena(); }
        finally {
            Comanda(os => {
                if (Privat) {
                    var soc = os.GetObjectsQuery<Societate>().First();
                    soc.CodFiscal = societate.CodFiscal; soc.InregistratTva = societate.InregistratTva; soc.Tara = societate.Tara;
                    soc.Denumire = societate.Denumire; soc.ContactNume = societate.ContactNume; soc.ContactPrenume = societate.ContactPrenume;
                    soc.Telefon = societate.Telefon; soc.ContBancarId = societate.ContBancarId;
                    os.GetObjectsQuery<ContPropriu>().Single(c => c.Cod == "BANCA").Iban = iban;
                }
                if (!Pastrata) {
                    foreach (var u in os.GetObjectsQuery<ApplicationUser>().Where(u => u.UserName == Utilizator).ToList()) os.Delete(u);
                    foreach (var r in os.GetObjectsQuery<PermissionPolicyRole>().Where(r => r.Name == Utilizator).ToList()) os.Delete(r);
                }
                os.CommitChanges();
            });
        }
    }

    void Scena() {
        var ceas = Stopwatch.StartNew();
        var (anM, lunaM) = Masurata;
        var prima = new DateOnly(An, 1, 1);
        if (istoric > 0) Deschidere(prima);
        var lung = Factura(prima, new LinieFctScena(1000, 1, Tva21));
        Dateaza(lung.Id, prima);
        Opereaza(Opereaza(lung.Id).ConexId.Value);
        void ConsumLung(DateOnly d) { var b = Consum(lung.Linii[0].Lot.Value, 1); Dateaza(b, d); Opereaza(b); }
        var fisa = Odata(new DateOnly(An, 1, 5));
        void Luna(int a, int l) {
            ConsumLung(new DateOnly(a, l, 28));
            if (a * 12 + l > An * 12 + 1) Cronometrat("AMO", () => Opereaza(Amo(a, l)));
        }
        for (var luna = 1; luna <= istoric; luna++) {
            for (var u = 0; u < unitatiIstoric; u++) Unitate(new DateOnly(An, luna, 1 + u % 27), false);
            Luna(An, luna);
            var l = luna;
            Cronometrat("INCHIDERE", () => Inchide(An, l));
        }
        if (istoric == 0) ConsumLung(new DateOnly(An, 1, 28));
        Luna(anM, lunaM);
        var sfarsit = new DateOnly(anM, lunaM, 28);
        var bcsDraft = Consum(lung.Linii[0].Lot.Value, 1); Dateaza(bcsDraft, sfarsit);
        var ntcDraft = Nota(sfarsit, new LinieNtcScena(ContFurnizor, Serviciu, 75, Furnizor)).Id;
        Console.WriteLine($"     PERFCUB istoric {istoric} luni × {unitatiIstoric} unități: {ceas.Elapsed.TotalSeconds:0} s");
        var facute = 0;
        foreach (var k in trepte) {
            treapta = k;
            for (; facute < k; facute++) Unitate(new DateOnly(anM, lunaM, 1 + facute % 27), true);
            Comanda(os => ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw("ANALYZE"));
            // Închiderea lunii măsurate cere luna precedentă închisă: la m = 0 lanțul o refuză.
            foreach (var op in Operatii.Where(o => (Privat || !o.NumaiPrivat) && (filtru == null || filtru.Contains(o.Cod.Split('-')[0]))
                         && (istoric > 0 || o.Ruta != Scriere))) {
                var masuri = masoaraProces(new Punct(istoric, anM, lunaM, k, op.Cod, k == trepte[^1], Privat, Marcaj, bcsDraft, ntcDraft,
                    Furnizor, Client, Cont(ContFurnizor), Cont(ContClient), Loc));
                Masuri.AddRange(masuri);
                foreach (var m in masuri) Controleaza(op, m, k);
            }
        }
        Lung(sfarsit);
        Comanda(os => ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw("ANALYZE"));
        VerificaVolumul(fisa);
    }

    void Cronometrat(string tip, Action actiune) {
        var ceas = Stopwatch.StartNew();
        var sql = CapturaSql.Masoara(actiune);
        Comenzi.Add(new(istoric, treapta, tip, ceas.Elapsed.TotalMilliseconds, sql.Count, sql.Sum(c => c.Durata.TotalMilliseconds)));
    }

    void Dateaza(Guid doc, DateOnly d) => Comanda(os => {
        var x = os.GetObjectByKey<Document>(doc); x.Data = d; x.DataInregistrare = d; os.CommitChanges();
    });

    Guid Fcl(DateOnly d, decimal baza) => CuSpatiu(os => {
        var x = os.CreateObject<FacturaIesire>(); x.Data = d; x.DataInregistrare = d; x.PredatorId = Loc; x.PrimitorId = Client;
        var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = x; l.Pozitie = 1;
        l.TipMaterialId = Tip(os, Venit); l.Cantitate = 1; l.PretUnitar = baza; l.CodEconomicId = Economic;
        if (Tva21 != null) l.TipTvaId = os.GetObjectsQuery<TipTva>().Single(t => t.Cod == Tva21).ID;
        os.CommitChanges(); return x.ID;
    });

    Guid Asm(DateOnly d, LinieScena consum, decimal cantitate = 1, decimal valoare = 10, string tip = null) => CuSpatiu(os => {
        var x = os.CreateObject<Asamblare>(); x.Data = d; x.DataInregistrare = d; x.PredatorId = Magazie; x.PrimitorId = Magazie;
        var c = os.CreateObject<AsamblareDetaliu>(); c.Document = x; c.Pozitie = 1; c.Directie = DirectieAsamblare.Consum;
        c.LotId = consum.Lot; c.Cantitate = cantitate; c.TipMaterialId = os.GetObjectByKey<Produs>(consum.Produs!.Value).TipMaterialId!.Value;
        var l = os.CreateObject<AsamblareDetaliu>(); l.Document = x; l.Pozitie = 2; l.Directie = DirectieAsamblare.Produs;
        l.TipMaterialId = Tip(os, tip ?? Stoc); l.Cantitate = 1; l.PretEvaluare = valoare;
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-ASM" + ++numarProdus; p.Denumire = p.Cod; p.UM = "BUC";
        p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
        l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
        os.CommitChanges(); return x.ID;
    });

    // O unitate: FCT (stoc 10 × 10 + serviciu 50) cu NIR conex, PLT 70 legată 50, BCS 2, BTR 1, DSC 1 (privat), ASM 1, FCL 100 cu INC 60 legată.
    void Unitate(DateOnly d, bool masurata) {
        var f = Factura(d, new LinieFctScena(10, 10, Tva21), new LinieFctScena(1, 50, Tva21, Stoc: false));
        Dateaza(f.Id, d);
        Guid nir = default;
        Cronometrat("FCT", () => nir = Opereaza(f.Id).ConexId.Value);
        Cronometrat("NIR", () => Opereaza(nir));
        var lot = f.Linii[0];
        if (masurata) loturiLuna.Add(lot);
        var plt = Trezorerie(false, 70).Id; Dateaza(plt, d); Cronometrat("PLT", () => Opereaza(plt));
        Cronometrat("IMPERECHERE", () => Imperecheaza(plt, f.Id, 50, d));
        var bcs = Consum(lot.Lot.Value, 2); Dateaza(bcs, d); Cronometrat("BCS", () => Opereaza(bcs));
        var btr = Iesire(true, (lot, 1)).Id; Dateaza(btr, d); Cronometrat("BTR", () => Opereaza(btr));
        if (Privat) { var dsc = Iesire(false, (lot, 1)).Id; Dateaza(dsc, d); Cronometrat("DSC", () => Opereaza(dsc)); }
        var asm = Asm(d, lot); Cronometrat("ASM", () => Opereaza(asm));
        var fcl = Fcl(d, 100); Cronometrat("FCL", () => Opereaza(fcl));
        var inc = Trezorerie(true, 60).Id; Dateaza(inc, d); Cronometrat("INC", () => Opereaza(inc));
        Cronometrat("IMPERECHERE", () => Imperecheaza(inc, fcl, 60, d));
    }

    Guid Partener(string sufix, string contImplicit = null) => CuSpatiu(os => {
        var p = os.CreateObject<Partener>(); p.Cod = Marcaj + sufix; p.Denumire = p.Cod;
        if (contImplicit != null) p.ContImplicitId = Cont(contImplicit);
        os.CommitChanges(); return p.ID;
    });

    Lot LotDeschis(IObjectSpace os, DateOnly data, decimal pret) {
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-D" + ++numarProdus; p.Denumire = p.Cod;
        p.UM = "BUC"; p.TipMaterialId = Tip(os, Stoc);
        var l = os.CreateObject<Lot>(); l.Produs = p; l.GestiuneId = Magazie; l.Data = data; l.PretUnitar = pret;
        return l;
    }

    // Deschiderea bazei: bloc de solduri, două loturi și o partidă de furnizor; un consum și o stingere pe ele.
    void Deschidere(DateOnly data) {
        furnizorOdata = Partener("-O-FURN");
        var referinta = Guid.NewGuid();
        var ancora = CuSpatiu(os => os.GetObjectsQuery<Cont>().First(c => c.RolTert == RolTertCont.Niciunul && c.Simbol.StartsWith("891")).ID);
        Comanda(os => {
            var a = LotDeschis(os, data, 10); var b = LotDeschis(os, data, 20);
            os.CommitChanges();
            loturiDeschise.AddRange([a.ID, b.ID]);
            using var tx = TranzactieComanda.Incepe(os);
            var analiza = new N.Analiza(null, Economic, null, null, null, null);
            C.Materializare.Deschide(os, data,
                [new(Cont(Stoc), N.Latura.Debit, 100, true), new(ancora, N.Latura.Credit, 100),
                    new(Cont(ContFurnizor), N.Latura.Credit, 150, true), new(ancora, N.Latura.Debit, 150)],
                [new(Cont(Stoc), a.ID, Magazie, 4, 40), new(Cont(Stoc), b.ID, Magazie, 3, 60)],
                [new(Cont(ContFurnizor), furnizorOdata, referinta, N.Latura.Credit, 150) { Analiza = analiza }]);
            os.CommitChanges(); tx.Commit();
        });
        var consum = Consum(loturiDeschise[0], 1); Dateaza(consum, data.AddDays(4)); Opereaza(consum);
        var unitati = Furnizor; Furnizor = furnizorOdata;
        var plata = Trezorerie(false, 20).Id;
        Furnizor = unitati;
        Dateaza(plata, data.AddDays(4)); Opereaza(plata);
        var partida = N.Unitate.DeschidePartidaInitiala(Cont(ContFurnizor), furnizorOdata, referinta, data).Id;
        Comanda(os => {
            using var tx = TranzactieComanda.Incepe(os);
            C.Materializare.Imperecheaza(os, os.GetObjectByKey<Document>(plata), partida, 20, data.AddDays(4));
            os.CommitChanges(); tx.Commit();
        });
    }

    // Faptele „o dată per bază” (X-D5 a), pe parteneri proprii, în prima lună a scenei.
    Guid Odata(DateOnly d) {
        var unitati = Furnizor;
        if (furnizorOdata == default) furnizorOdata = Partener("-O-FURN");
        partenerNota = Partener("-O-NTC");
        Comanda(os => { os.GetObjectByKey<Repartitor>(Loc).Calitati |= CalitateRepartitor.Comisie; os.CommitChanges(); });

        Furnizor = partenerNota;
        var nota = Nota(d, new LinieNtcScena(ContFurnizor, Serviciu, 40, partenerNota)); Opereaza(nota.Id);
        var serviciu = Factura(d.AddDays(1), new LinieFctScena(1, 100, Stoc: false)); Dateaza(serviciu.Id, d.AddDays(1)); Opereaza(serviciu.Id);
        Imperecheaza(nota.Id, serviciu.Id, 40, d.AddDays(1));

        Furnizor = furnizorOdata;
        var receptie = Factura(d, new LinieFctScena(10, 10)); Dateaza(receptie.Id, d);
        Opereaza(Opereaza(receptie.Id).ConexId.Value);
        var ldi = CuSpatiu(os => {
            var x = os.CreateObject<ListaDiferenteInventar>(); x.Data = d.AddDays(2); x.DataInregistrare = x.Data;
            x.PredatorId = Magazie; x.PrimitorId = Loc;
            var l = os.CreateObject<ListaDiferenteInventarDetaliu>(); l.Document = x; l.Pozitie = 1;
            l.Directie = DirectieDiferenta.Minus; l.Cantitate = 2; l.TipMaterialId = Tip(os, Stoc);
            l.CodEconomicId = Economic; l.LotId = receptie.Linii[0].Lot;
            os.CommitChanges(); return x.ID;
        });
        Opereaza(ldi);
        // ASM pe conturi diferite (Operare) și trei transformări 1 + 1 + 1 din 3 × 3,333333 (absorbția Δ a regimului dual).
        var materie = Factura(d, new LinieFctScena(4, 10, Tip: Privat ? "301" : "302.03.00")); Dateaza(materie.Id, d);
        Opereaza(Opereaza(materie.Id).ConexId.Value);
        Opereaza(Asm(d.AddDays(2), materie.Linii[0], 4, 40, Privat ? "345" : "303.01.00"));
        var treime = Factura(d, new LinieFctScena(3, 3.333333m)); Dateaza(treime.Id, d);
        Opereaza(Opereaza(treime.Id).ConexId.Value);
        foreach (var valoare in new[] { 3.33m, 3.33m, 3.34m }) Opereaza(Asm(d.AddDays(2), treime.Linii[0], 1, valoare, Stoc));
        if (Privat) {
            var marfa = Factura(d, new LinieFctScena(10, 10, Tip: "371")); Dateaza(marfa.Id, d);
            Opereaza(Opereaza(marfa.Id).ConexId.Value);
            var retur = CuSpatiu(os => {
                var x = os.CreateObject<ReturFurnizor>(); x.Data = d.AddDays(2); x.DataInregistrare = x.Data;
                x.PredatorId = Magazie; x.PrimitorId = furnizorOdata;
                var l = os.CreateObject<DocumentDetaliu>(); l.Document = x; l.Pozitie = 1;
                l.TipMaterialId = Tip(os, "371"); l.Cantitate = 2; l.LotId = marfa.Linii[0].Lot;
                os.CommitChanges(); return x.ID;
            });
            Opereaza(retur);
            var import = Factura(d, new LinieFctScena(1, 60, "IMP")); Dateaza(import.Id, d);
            Opereaza(Opereaza(import.Id).ConexId.Value);
            var vama = Partener("-O-VAMA", "446");
            var dvi = CuSpatiu(os => {
                var x = os.CreateObject<Dvi>(); x.Numar = Marcaj + "-MRN"; x.Data = d.AddDays(2); x.DataInregistrare = x.Data;
                x.PredatorId = vama; x.PrimitorId = Loc;
                var l = os.CreateObject<DocumentDetaliu>(); l.Document = x; l.Pozitie = 1;
                l.TipMaterialId = Tip(os, Stoc); l.Valoare = 60; l.TipTvaId = Tva("IMP21");
                var leg = os.CreateObject<DviFactura>(); leg.DviId = x.ID; leg.FacturaId = import.Id;
                os.CommitChanges(); return x.ID;
            });
            Opereaza(dvi);
        }
        Furnizor = unitati;
        var stornata = Fcl(d, 100); Opereaza(stornata); Storneaza(stornata, d.AddDays(3));

        var suport = Nota(d, new LinieNtcScena(Activ, Capital, 1200, Magazie, Loc)); Opereaza(suport.Id);
        var fisa = CuSpatiu(os => {
            var f = os.CreateObject<Imobilizare>(); f.NumarInventar = Marcaj + "-F1"; f.Denumire = f.NumarInventar;
            f.TipMaterialId = Tip(os, Activ); f.LocId = Magazie; f.CodEconomicId = Economic;
            os.CommitChanges(); return f.ID;
        });
        var pif = CuSpatiu(os => {
            var p = os.CreateObject<PunereInFunctiune>(); p.Data = d; p.DataInregistrare = d; p.PredatorId = Loc; p.PrimitorId = Magazie;
            var l = os.CreateObject<PunereInFunctiuneDetaliu>(); l.Document = p; l.Pozitie = 1;
            l.ImobilizareId = fisa; l.TipMaterialId = Tip(os, Activ); l.Cantitate = 1; l.Valoare = 1200; l.ValoareFiscala = 900;
            l.Fel = FelLiniePif.Intrare; l.Metoda = MetodaAmortizare.Liniara; l.MetodaFiscala = MetodaAmortizare.Liniara;
            l.DurataLuni = 24; l.DurataFiscalaLuni = 36; l.CategorieFiscala = CategorieFiscala.Standard; l.UtilizareExclusiva = true;
            os.CommitChanges(); return p.ID;
        });
        Opereaza(pif);
        return fisa;
    }

    Guid Amo(int a, int l) => CuSpatiu(os => {
        var d = AmortizareService.Genereaza(os, a, l, Loc) ?? throw new InvalidOperationException($"PerfCub cere o AMO eligibilă în {a}-{l:00}.");
        os.CommitChanges(); return d.ID;
    });

    // Documentul lung: un consum cu o linie din fiecare lot al lunii măsurate; mărimea explicației lui se raportează.
    void Lung(DateOnly d) {
        var doc = CuSpatiu(os => {
            var x = os.CreateObject<BonConsum>(); x.Data = d; x.DataInregistrare = d; x.PredatorId = Magazie; x.PrimitorId = Loc;
            var pozitie = 0;
            foreach (var lot in loturiLuna) {
                var l = os.CreateObject<DocumentDetaliu>(); l.Document = x; l.Pozitie = ++pozitie;
                l.TipMaterialId = Tip(os, Stoc); l.LotId = lot.Lot; l.Cantitate = 1;
            }
            os.CommitChanges(); return x.ID;
        });
        treapta = loturiLuna.Count;
        Cronometrat("BCS-LUNG", () => Opereaza(doc));
        var octeti = CuSpatiu(os => ((EFCoreObjectSpace)os).DbContext.Database.SqlQuery<int>(
            $"""SELECT octet_length("Explicatie"::text) AS "Value" FROM "Tranzactie" WHERE "DocumentId" = {doc} AND "Explicatie" IS NOT NULL""").Single());
        var toate = CuSpatiu(os => ((EFCoreObjectSpace)os).DbContext.Database.SqlQuery<int>(
            $"""SELECT octet_length("Explicatie"::text) AS "Value" FROM "Tranzactie" WHERE "Explicatie" IS NOT NULL""").ToList());
        Explicatii = new(loturiLuna.Count, octeti, toate.Count, toate.Sum(x => (long)x), toate.Max());
        Console.WriteLine($"     MĂSURAT (perfcub explicație m{istoric}): documentul de {Explicatii.Linii} linii are {Explicatii.Octeti} octeți "
            + $"({(double)Explicatii.Octeti / Explicatii.Linii:0} per linie); {Explicatii.Tranzactii} explicații, {Explicatii.Total} octeți în total, maxim {Explicatii.Maxim}.");
    }

    public sealed record MarimeExplicatii(int Linii, int Octeti, int Tranzactii, long Total, int Maxim);
    public MarimeExplicatii Explicatii { get; private set; }
}
