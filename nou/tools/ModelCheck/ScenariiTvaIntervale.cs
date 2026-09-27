using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiTvaIntervale(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "R6", 2025) {
    DateOnly Iulie => new(An, 7, 15);
    DateOnly August => new(An, 8, 15);
    Guid vechi, nou, tipAvans;

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) {
        purja.Adauga(os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.TipTva.Cod.StartsWith(Marcaj)));
        purja.Adauga(os.GetObjectsQuery<PoliticaTva>().Where(p => !p.DinSeed && p.TipDocument.Cod == "FCT"
            && os.GetObjectsQuery<TipTva>().Any(t => t.Cod.StartsWith(Marcaj))));
        purja.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod.StartsWith(Marcaj)));
    }

    protected override void Executa() {
        Comanda(os => {
            for (var luna = 3; luna <= 9; luna++) {
                var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = luna;
            }
            foreach (var cota in new[] { 19m, 21m }) {
                var t = os.CreateObject<TipTva>(); t.Cod = Marcaj + cota; t.Denumire = t.Cod;
                t.Regim = RegimTva.Normal; t.Cota = cota;
                t.ContTvaDeductibilId = Cont(Privat ? "4426" : "473.01.09");
                if (Privat) t.ContTvaColectatId = Cont("4427");
                if (cota == 19) { t.ValabilPanaLa = new(An, 7, 31); vechi = t.ID; }
                else { t.ValabilDeLa = new(An, 8, 1); nou = t.ID; }
            }
            var tip = os.CreateObject<TipMaterial>(); tip.Cod = Marcaj + "-AV"; tip.Denumire = tip.Cod;
            var serviciu = os.GetObjectByKey<TipMaterial>(Tip(os, Serviciu));
            tip.ClasaId = serviciu.ClasaId; tip.ContImplicitId = serviciu.ContImplicitId;
            tip.RegularizareAvans = true; tipAvans = tip.ID;
            if (!Privat) {
                var p = os.CreateObject<PoliticaTva>();
                p.TipDocumentId = os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "FCT").ID;
                p.Directie = DirectieTva.Deductibil; p.SursaContrapartida = SursaCont.RepartitorPredator;
                p.ContrapartidaFallbackId = Cont(ContFurnizor);
            }
            os.CommitChanges();
        });
        var istoric = Creeaza(Iulie, vechi, 100);
        Verifica("SC-CIT-91", "draft fără mapare: avertisment SAF-T numai pe Privat",
            Raport(istoric.Id).Any(r => r.Cod == "TipTvaFaraCodSaft") == Privat);
        var faraMapare = Opereaza(istoric.Id).Mesaje;
        Verifica("SC-CIT-91", "operare și raport fără mapare: avertisment SAF-T numai pe Privat",
            faraMapare.Any(m => m.StartsWith("TipTvaFaraCodSaft: linia 1 —")) == Privat
            && Raport(istoric.Id).Any(r => r.Cod == "TipTvaFaraCodSaft") == Privat
            && faraMapare.All(m => !m.Contains(istoric.Linii[0].Id.ToString())));
        Comanda(os => { Mapeaza(os, vechi, 19); Mapeaza(os, nou, 21); os.CommitChanges(); });
        Verifica("SC-CIT-90", "100/19 în iulie nu are avertisment de interval sau aritmetic", !Raport(istoric.Id).Any());
        var automat = Creeaza(August, vechi, 100);
        var manual = Creeaza(August, vechi, 100, 19);
        Comanda(os => { os.GetObjectByKey<TipTva>(vechi).Cota = 21; os.CommitChanges(); });
        Verifica("SC-CIT-91", "istoricul rămâne corect aritmetic după editarea cotei",
            Raport(istoric.Id).Any(r => r.Cod == "TVA_CALIFICARE_MODIFICATA")
            && !Raport(istoric.Id).Any(r => r.Cod == "TVA_TAXA_DIFERITA_DE_COTA"));
        Verifica("SC-CIT-91b", "draftul marcat avertizează, cel automat nu",
            Raport(manual.Id).Any(r => r.Cod == "TVA_TAXA_DIFERITA_DE_COTA")
            && !Raport(automat.Id).Any(r => r.Cod == "TVA_TAXA_DIFERITA_DE_COTA"));
        Verifica("SC-CIT-91", "calificarea nouă pierde maparea, istoricul o păstrează",
            Raport(automat.Id).Any(r => r.Cod == "TipTvaFaraCodSaft") == Privat
            && !Raport(istoric.Id).Any(r => r.Cod == "TipTvaFaraCodSaft"));
        var mesaje = Opereaza(manual.Id).Mesaje; Opereaza(automat.Id);
        Verifica("SC-CIT-91b", "operarea păstrează 19 cules și recalculează 21 automat", CuSpatiu(os =>
            Fiscale.Fapte(os).Single(f => f.DocumentId == manual.Id).Tva == 19
            && Fiscale.Fapte(os).Single(f => f.DocumentId == automat.Id).Tva == 21)
            && mesaje.Any(m => m.StartsWith("TVA_TAXA_DIFERITA_DE_COTA: linia 1 —"))
            && mesaje.All(m => !m.Contains(manual.Linii[0].Id.ToString())));
        Storneaza(manual.Id, new(An, 9, 15));
        Verifica("SC-CIT-92", "compensarea din septembrie închide avertismentul din august",
            !Raport(manual.Id).Any() && CuSpatiu(os => DiagnosticTvaService.Citeste(os,
                new(An, 8, 1), new(An, 8, 31), includeCompensate: true, documentId: manual.Id))
                .Any(r => r.Stare == "Compensat" && r.Cod == "TVA_TAXA_DIFERITA_DE_COTA"));
        Comanda(os => { os.GetObjectByKey<TipTva>(vechi).Cota = 19; os.CommitChanges(); });
        Avansuri();
        Recalcul();
        Aritmetica();
        Implicite();
        Corectii();
        CorectieAvans();
        MaiMulteCote();
        SeedSiModel();
        if (Privat) RetururiSiLivrari();
    }

    FacturaScena Creeaza(DateOnly data, Guid tva, decimal baza, decimal? taxa = null, bool avans = false,
            Guid? sursa = null) {
        var cod = CuSpatiu(os => os.GetObjectByKey<TipTva>(tva).Cod);
        var f = Factura(data, new LinieFctScena(1, baza, cod, false));
        Comanda(os => {
            var doc = os.GetObjectByKey<FacturaIntrare>(f.Id);
            var l = os.GetObjectByKey<FacturaIntrareDetaliu>(f.Linii[0].Id);
            if (avans) l.TipMaterialId = tipAvans;
            l.LinieAvansId = sursa;
            CulegereDocument.Mapata(os, doc, l, null, taxa);
            os.CommitChanges();
        });
        return f;
    }
    IReadOnlyList<RandDiagnosticTva> Raport(Guid doc) => CuSpatiu(os => DiagnosticTvaService.Citeste(os, documentId: doc));

    void Mapeaza(IObjectSpace os, Guid tip, decimal cota) {
        if (!Privat) return;
        foreach (var sectiune in new[] { SectiuneTvaSaft.Facturi, SectiuneTvaSaft.GeneralLedger })
            foreach (var sens in new[] { SensTva.Achizitie, SensTva.Livrare }) {
                var m = os.CreateObject<MapareTvaSaft>();
                m.Versiune = Atlas.Conta.BackOffice.Module.Saft.MapariFiscale.Versiune;
                m.TipTvaId = tip; m.Regim = RegimTva.Normal; m.Cota = cota;
                m.Sectiune = sectiune; m.Sens = sens; m.Rol = Atlas.Conta.Nucleu.RolTva.Taxa;
                m.TaxType = "300"; m.TaxCode = "301101";
            }
    }

    void Avansuri() {
        var avans = Creeaza(Iulie, vechi, 100, avans: true); Opereaza(avans.Id);
        var finala = Creeaza(August, vechi, -100, avans: true, sursa: avans.Linii[0].Id);
        Verifica("SC-CIT-94", "avans pozitiv valid și regularizare la reperul lui din iulie", !Raport(avans.Id).Any() && !Raport(finala.Id).Any());
        Opereaza(finala.Id);
        Verifica("SC-CIT-94", "regularizarea rămâne în august cu -100/-19", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(f => f.DocumentId == finala.Id);
            return f.Baza == -100 && f.Tva == -19 && f.PerioadaD300 == An * 100 + 8;
        }));
        var fara = Creeaza(August, vechi, -100, avans: true);
        Verifica("SC-CIT-94", "fără sursă: avertisment fiscal, fără recomandare de cotă curentă",
            Raport(fara.Id).Any(r => r.Cod == "TVA_AVANS_FARA_REFERINTA"));
        var ajustare = Creeaza(August, nou, -40);
        Verifica("SC-CIT-93", "reducerea la cotă curentă cere tot proveniență",
            Raport(ajustare.Id).Any(r => r.Cod == "TVA_AJUSTARE_FARA_SURSA"));
        Storneaza(avans.Id, new(An, 9, 15));
        Verifica("SC-CIT-94a", "sursa compensată nu este redirecționată automat",
            Raport(finala.Id).Any(r => r.Cod == "TVA_AVANS_SURSA_COMPENSATA"));
    }

    void Recalcul() {
        var f = Creeaza(August, nou, 100, 19);
        Comanda(os => {
            var d = os.GetObjectByKey<Document>(f.Id); var l = os.GetObjectByKey<DocumentDetaliu>(f.Linii[0].Id);
            CulegereDocument.RecalculeazaTva(os, d, [l]); os.CommitChanges();
            Verifica("SC-CIT-91b", "recalcul explicit: 21 și marcaj stins", l.ValoareTva == 21 && !l.TvaCules);
            l.ValoareTva = 19.5m;
            CulegereDocument.LinieSchimbata(os, d, l, nameof(l.ValoareTva));
            Verifica("SC-CIT-91b", "taxa tastată aprinde marcajul", l.TvaCules);
            l.Cantitate = 2;
            CulegereDocument.LinieSchimbata(os, d, l, nameof(l.Cantitate));
            Verifica("SC-CIT-91b", "schimbarea bazei stinge marcajul și calculează 42", !l.TvaCules && l.ValoareTva == 42);
            l.ValoareTva = 40;
            CulegereDocument.LinieSchimbata(os, d, l, nameof(l.ValoareTva));
            l.ValoareTva = 0;
            CulegereDocument.LinieSchimbata(os, d, l, nameof(l.ValoareTva));
            Verifica("SC-CIT-91b", "zero în ecran revine la cotă fără marcaj", !l.TvaCules && l.ValoareTva == 42);
            os.CommitChanges();
        });
        using var os = Deschide();
        using var tx = TranzactieComanda.Incepe(os);
        try {
            ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw(
                "UPDATE \"DocumentDetalii\" SET \"TvaCules\" = true, \"ValoareTva\" = 0 WHERE \"ID\" = {0}", f.Linii[0].Id);
            Verifica("SC-CIT-91b", "CHECK refuză marcaj pe zero", false);
        } catch (PostgresException e) { Verifica("SC-CIT-91b", "CHECK refuză marcaj pe zero", e.SqlState == "23514"); }
    }

    void Aritmetica() {
        var rotunjit = Creeaza(August, nou, 0.02m);
        Adauga(rotunjit.Id, nou, 0.02m, false); Adauga(rotunjit.Id, nou, 0.02m, false);
        Opereaza(rotunjit.Id);
        Verifica("SC-CIT-91a", "cub: trei baze 0,02, taxă totală 0,01 fără avertisment", CuSpatiu(os =>
            Fiscale.Fapte(os).Where(f => f.DocumentId == rotunjit.Id).Sum(f => f.Tva) == 0.01m)
            && !Raport(rotunjit.Id).Any());
        var doc = Guid.NewGuid();
        var calificare = new CalificareTva(RegimTva.Normal, 21, false);
        LinieDiagnosticTva Linie(decimal baza, decimal taxa) => new(Guid.NewGuid(), doc, Furnizor,
            SensTva.Achizitie, calificare, August, baza, taxa, false, false, false, null, null,
            new(calificare, null, null));
        Verifica("SC-CIT-91", "pragul este strict mai mare de 0,01", DiagnosticTva.Verifica(
            [Linie(100, 21.01m), Linie(100, 21.02m)], Scara.ConventieBani).Count == 1);
        Verifica("SC-CIT-91", "repartizarea documentului: trei baze 0,02, taxa 0,01/0/0",
            !DiagnosticTva.Verifica([Linie(0.02m, 0.01m), Linie(0.02m, 0), Linie(0.02m, 0)], Scara.ConventieBani).Any());
        var ti = Linie(100, 19) with { Calificare = new(RegimTva.TaxareInversa, 21, false), TipActual = null };
        Verifica("SC-CIT-91", "TI deductibilă compară taxa o singură dată, fără autocolectare",
            DiagnosticTva.Verifica([ti], Scara.ConventieBani).Single().Cod == "TVA_TAXA_DIFERITA_DE_COTA");
        foreach (var regim in new[] { RegimTva.Capitalizat, RegimTva.Scutit, RegimTva.Neimpozabil }) {
            var inert = Linie(121, 0) with { Calificare = new(regim, 21, false), TipActual = null };
            Verifica("SC-CIT-91", $"{regim}: fără reconstrucție fictivă a taxei",
                !DiagnosticTva.Verifica([inert], Scara.ConventieBani).Any());
        }
        Verifica("SC-CIT-91", "TI colectată nu compară o taxă separată",
            !DiagnosticTva.Verifica([ti with { Sens = SensTva.Livrare, Taxa = 0 }], Scara.ConventieBani).Any());
    }
    void Implicite() {
        var a = new TipTvaFapt(vechi, "19", RegimTva.Normal, true, 19, null, null, false, null, new(An, 7, 31));
        var b = new TipTvaFapt(nou, "21", RegimTva.Normal, true, 21, null, null, false, new(An, 8, 1));
        var reguli = new[] { new PoliticaTvaImplicitFapt(Guid.NewGuid(), null, null, vechi, false),
            new PoliticaTvaImplicitFapt(Guid.NewGuid(), null, new(An, 8, 1), nou, false) };
        var tipuri = new Dictionary<Guid, TipTvaFapt> { [vechi] = a, [nou] = b };
        Verifica("SC-CIT-90a", "implicit 19 în iulie, 21 în august",
            Potrivire.TvaImplicit(reguli, null, Iulie, null, nou, null, tipuri, false).Rezultat.TipTvaId == vechi
            && Potrivire.TvaImplicit(reguli, null, August, null, nou, null, tipuri, false).Rezultat.TipTvaId == nou);
        tipuri[vechi] = a with { Activ = false };
        var refuz = Potrivire.TvaImplicit(reguli, null, Iulie, null, nou, null, tipuri, false);
        Verifica("SC-CIT-90a", "istoricul inactiv și ancora viitoare nu devin implicite",
            refuz.Rezultat.TipTvaId == null && refuz.Candidati.Any(c => c.Motiv == MotivEliminare.Inactiv)
            && refuz.Rezultat.Motiv.Contains("interval"));
        var f = Creeaza(August, nou, 100);
        Comanda(os => {
            var d = os.GetObjectByKey<FacturaIntrare>(f.Id); d.DataExigibilitate = Iulie;
            var p = os.GetObjectByKey<Partener>(Furnizor); p.TipTvaImplicitId = vechi;
            var l = os.GetObjectByKey<DocumentDetaliu>(f.Linii[0].Id); l.TipTva = null; l.TipTvaId = null;
            os.CommitChanges();
            CulegereDocument.Mapata(os, d, l, null, null);
            Verifica("SC-CIT-90a", "L3 propune la exigibilitate, nu la data facturii", l.TipTvaId == vechi);
            d.DataExigibilitate = August; CulegereDocument.Normalizeaza(os, d);
            Verifica("SC-CIT-90a", "schimbarea datei păstrează alegerea existentă", l.TipTvaId == vechi);
            p.TipTvaImplicitId = null; os.CommitChanges();
        });
    }

    void Corectii() {
        var f = Creeaza(August, nou, 100, 19); Opereaza(f.Id);
        var c = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(f.Id, new(An, 9, 15), MotivCorectie.EroareMateriala).CorectieId);
        Comanda(os => {
            var d = os.GetObjectByKey<Document>(c); var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == c);
            Verifica("SC-CIT-91b", "corecția copiază taxa și marcajul", l.TvaCules && l.ValoareTva == 19);
            CulegereDocument.RecalculeazaTva(os, d, [l]); os.CommitChanges();
        });
        Opereaza(c);
        Verifica("SC-CIT-90", "corecția în septembrie păstrează exigibilitatea august și taxa netă 21", CuSpatiu(os => {
            var fapte = Fiscale.Fapte(os).Where(x => x.DocumentId == f.Id || x.DocumentId == c).ToArray();
            return fapte.Sum(x => x.Tva) == 21 && fapte.All(x => x.PerioadaD300 == An * 100 + 8)
                && fapte.All(x => !x.RegularizareD300);
        }));
    }

    void CorectieAvans() {
        var avans = Creeaza(Iulie, vechi, 100, avans: true); Opereaza(avans.Id);
        var finala = Creeaza(August, vechi, -100, avans: true, sursa: avans.Linii[0].Id);
        var corectie = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(avans.Id,
            new(An, 9, 15), MotivCorectie.EroareMateriala).CorectieId);
        Opereaza(corectie);
        Verifica("SC-CIT-94a", "corecția avansului nu redirecționează referința, sursa veche este compensată",
            Raport(finala.Id).Any(r => r.Cod == "TVA_AVANS_SURSA_COMPENSATA")
            && CuSpatiu(os => os.GetObjectByKey<FacturaIntrareDetaliu>(finala.Linii[0].Id).LinieAvansId) == avans.Linii[0].Id);
        Comanda(os => {
            os.GetObjectByKey<FacturaIntrareDetaliu>(finala.Linii[0].Id).LinieAvansId =
                os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == corectie).ID;
            os.CommitChanges();
        });
        Verifica("SC-CIT-94a", "alegerea explicită a sursei noi elimină avertismentul", !Raport(finala.Id).Any());
    }

    void MaiMulteCote() {
        var noua = CuSpatiu(os => {
            var t = os.CreateObject<TipTva>(); t.Cod = Marcaj + "9"; t.Denumire = t.Cod;
            t.Cota = 9; t.Regim = RegimTva.Normal; t.ValabilPanaLa = new(An, 7, 31);
            t.ContTvaDeductibilId = Cont(Privat ? "4426" : "473.01.09");
            if (Privat) t.ContTvaColectatId = Cont("4427");
            Mapeaza(os, t.ID, 9); os.CommitChanges(); return t.ID;
        });
        var avans = Creeaza(Iulie, vechi, 100, avans: true);
        var a9 = Adauga(avans.Id, noua, 100, true); Opereaza(avans.Id);
        var finala = Creeaza(August, nou, 300);
        var r19 = Adauga(finala.Id, vechi, -100, true, avans.Linii[0].Id, -19);
        var r9 = Adauga(finala.Id, noua, -100, true, a9, -9);
        Verifica("SC-CIT-94", "două cote pe aceeași factură, surse explicite, fără ambiguitate", !Raport(finala.Id).Any());
        Comanda(os => { os.GetObjectByKey<FacturaIntrareDetaliu>(r19).LinieAvansId = a9; os.CommitChanges(); });
        Verifica("SC-CIT-94", "sursa greșită nu este aleasă din nou după cotă",
            Raport(finala.Id).Any(r => r.Cod == "TVA_AVANS_CALIFICARE_DIFERITA" && r.LinieId == r19));
        Comanda(os => { os.GetObjectByKey<FacturaIntrareDetaliu>(r19).LinieAvansId = avans.Linii[0].Id; os.CommitChanges(); });
        Opereaza(finala.Id);
        Verifica("SC-CIT-94", "regularizare -200/-28, total final 100/35", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Where(x => x.DocumentId == finala.Id).ToArray();
            return f.Sum(x => x.Baza) == 100 && f.Sum(x => x.Tva) == 35
                && f.Where(x => x.Baza < 0).Sum(x => x.Tva) == -28;
        }));
        var partiala = Creeaza(August, vechi, -40, avans: true, sursa: avans.Linii[0].Id);
        var candidat = CuSpatiu(os => AvansuriCulegere.Candidati(os, os.GetObjectByKey<Document>(partiala.Id)).Select(l => l.ID).ToArray());
        Verifica("SC-CIT-92", "reutilizarea parțială permisă; lookup-ul oferă ambele linii active",
            !Raport(partiala.Id).Any() && candidat.Contains(a9) && candidat.Contains(avans.Linii[0].Id));
    }

    Guid Adauga(Guid doc, Guid tva, decimal baza, bool avans, Guid? sursa = null, decimal? taxa = null) => CuSpatiu(os => {
        var d = os.GetObjectByKey<FacturaIntrare>(doc);
        var l = os.CreateObject<FacturaIntrareDetaliu>(); l.Document = d;
        l.Pozitie = d.Detalii.Count; l.TipMaterialId = avans ? tipAvans : Tip(os, Serviciu);
        l.Cantitate = 1; l.PretUnitar = baza; l.TipTvaId = tva; l.CodEconomicId = Economic; l.LinieAvansId = sursa;
        CulegereDocument.Mapata(os, d, l, null, taxa); os.CommitChanges(); return l.ID;
    });

    void SeedSiModel() => Comanda(os => {
        var tipuri = os.GetObjectsQuery<TipTva>().Where(t => t.DinSeed).ToArray();
        var istoric = Privat ? new[] { "N19", "TI19" } : new[] { "CAP19" };
        var actual = Privat ? new[] { "N21", "N11", "TI21", "NED21", "IMP21", "IMP11", "IMPTI21", "IMPTI11" }
            : new[] { "CAP21", "CAP11" };
        Verifica("SC-CIT-93a", "seed-ul aliniază capetele inclusiv, fără activarea cotelor istorice",
            tipuri.Where(t => istoric.Contains(t.Cod)).All(t => t.ValabilDeLa == null
                && t.ValabilPanaLa == new DateOnly(2025, 7, 31) && !t.Activ)
            && tipuri.Where(t => actual.Contains(t.Cod)).All(t => t.ValabilDeLa == new DateOnly(2025, 8, 1) && t.ValabilPanaLa == null));
        var db = ((EFCoreObjectSpace)os).DbContext;
        var tabela = db.Model.GetRelationalModel().Tables.Single(t => t.Name == "DocumentDetalii");
        Verifica("SC-CIT-94a", "TPH: o coloană și un singur FK NO ACTION pentru ambele frunze",
            tabela.Columns.Count(c => c.Name == "LinieAvansId") == 1
            && tabela.ForeignKeyConstraints.Count(f => f.Columns.Any(c => c.Name == "LinieAvansId")) == 1
            && tabela.ForeignKeyConstraints.Single(f => f.Columns.Any(c => c.Name == "LinieAvansId")).OnDeleteAction
                == Microsoft.EntityFrameworkCore.Migrations.ReferentialAction.NoAction);
    });

    void RetururiSiLivrari() {
        var livrare = CuSpatiu(os => {
            var d = os.CreateObject<FacturaIesire>(); d.Data = August; d.DataExigibilitate = Iulie;
            d.PredatorId = Loc; d.PrimitorId = Client;
            var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = 100; l.TipTvaId = vechi;
            CulegereDocument.Mapata(os, d, l, null, 19); os.CommitChanges(); return d.ID;
        });
        Opereaza(livrare);
        Verifica("SC-CIT-90", "livrare august exigibilă în iulie, 100/19, fără interval fals", !Raport(livrare).Any());
        Comanda(os => { os.GetObjectByKey<TipTva>(vechi).ValabilPanaLa = new(An, 7, 14); os.CommitChanges(); });
        Verifica("SC-CIT-90", "editarea retroactivă descoperă impactul pe Emise, în iulie", CuSpatiu(os =>
            DiagnosticTvaService.Citeste(os, new(An, 7, 1), new(An, 7, 31), vechi)
                .Any(r => r.DocumentId == livrare && r.Registru == "Emise" && r.Cod == "TVA_IN_AFARA_INTERVALULUI")
            && !DiagnosticTvaService.Citeste(os, new(An, 8, 1), new(An, 8, 31), vechi).Any(r => r.DocumentId == livrare)));
        Comanda(os => { os.GetObjectByKey<TipTva>(vechi).ValabilPanaLa = new(An, 7, 31); os.CommitChanges(); });
        var retur = CuSpatiu(os => {
            var d = os.CreateObject<ReturClient>(); d.Data = new(An, 9, 15); d.PredatorId = Client; d.PrimitorId = Magazie;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, "704"); l.Valoare = 40; l.Cantitate = 1; l.TipTvaId = vechi;
            CulegereDocument.Mapata(os, d, l, null, 7.60m); os.CommitChanges(); return d.ID;
        });
        Opereaza(retur);
        Verifica("SC-CIT-90b", "RDC -40/-7,60 păstrează marcajul, fără interval sau eroare aritmetică", CuSpatiu(os => {
            var f = Fiscale.Fapte(os).Single(x => x.DocumentId == retur);
            var r = Raport(retur);
            return f.Baza == -40 && f.Tva == -7.60m && f.PerioadaD300 == An * 100 + 9
                && os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == retur).TvaCules
                && r.Count == 1 && r[0].Cod == "TVA_AJUSTARE_FARA_SURSA";
        }));
        var intrare = Factura(Iulie, new LinieFctScena(1, 100, Marcaj + "19", true, TaxaCuleasa: 19));
        var nir = Opereaza(intrare.Id).ConexId!.Value; Opereaza(nir);
        var rlf = CuSpatiu(os => {
            var d = os.CreateObject<ReturFurnizor>(); d.Data = new(An, 9, 15); d.PredatorId = Magazie; d.PrimitorId = Furnizor;
            var l = os.CreateObject<DocumentDetaliu>(); l.Document = d;
            l.TipMaterialId = Tip(os, Stoc); l.LotId = intrare.Linii[0].Lot; l.Cantitate = 0.4m; l.TipTvaId = vechi;
            CulegereDocument.Mapata(os, d, l, null, 8.40m); os.CommitChanges(); return d.ID;
        });
        Opereaza(rlf);
        Verifica("SC-CIT-90b", "RLF -40/-8,40 la 19% păstrează taxa și avertizează separat aritmetica",
            Raport(rlf).Any(r => r.Cod == "TVA_AJUSTARE_FARA_SURSA")
            && Raport(rlf).Any(r => r.Cod == "TVA_TAXA_DIFERITA_DE_COTA")
            && !Raport(rlf).Any(r => r.Cod == "TVA_IN_AFARA_INTERVALULUI"));
        Verifica("SC-CIT-91b", "conexul nu copiază marcajul sau taxa culeasă", CuSpatiu(os =>
            os.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == nir).All(l => !l.TvaCules && l.ValoareTva == 0)));
    }

}
