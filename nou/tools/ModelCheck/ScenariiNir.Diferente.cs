using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiNir {
    Guid nirDeCorectat;
    Guid? functionalNir, finantareNir;
    FacturaScena facturaDeCorectat;
    string Clarificare => Privat ? "473" : "473.01.09";
    string Nesosite => Privat ? "408" : "408.00.00";
    string Drum => Privat ? "322" : "351.01.00";
    string Imputat => Privat ? "461" : "461.01.09";

    (FacturaScena F, Guid Nir) Constatat(decimal q, CauzaDiferentei? cauza = null, Guid? tert = null,
            string tva = null, decimal pret = 25) {
        var f = Factura(Ianuarie, new LinieFctScena(4, pret, tva ?? (Privat ? "SFD" : "CAP0")));
        var nir = Opereaza(f.Id).ConexId!.Value;
        Schimba(nir, q, cauza, tert);
        return (f, nir);
    }
    void Schimba(Guid nir, decimal q, CauzaDiferentei? cauza = null, Guid? tert = null) => Comanda(os => {
        var l = os.GetObjectByKey<NIR>(nir).Detalii.OfType<NirDetaliu>().Single();
        l.CodFunctionalId = functionalNir; l.SursaFinantareId = finantareNir;
        l.Cantitate = q; l.CauzaDiferentei = cauza; l.PartenerDiferentaId = tert; os.CommitChanges();
    });
    decimal Net(Guid doc, string cont, N.FelTranzactie? fel = null) => CuSpatiu(os => {
        var id = Cont(cont);
        return os.GetObjectsQuery<C.Postare>().Where(p => p.DocumentId == doc && p.Cont == id
            && p.Carte == N.Carte.Contabil && (fel == null || p.Tranzactie.Fel == fel))
            .Sum(p => (decimal?)(p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)) ?? 0;
    });
    void Delta(string scena, Guid nir, string cont, decimal suma) {
        Verifica(scena, "delta pe contul cauzei și invers pe stoc", Net(nir, cont) == suma && Net(nir, Stoc) == -suma);
        Verifica(scena, "fără postare TVA", CuSpatiu(os => !os.GetObjectsQuery<C.Postare>()
            .Any(p => p.DocumentId == nir && p.TipTvaId != null)));
        Verifica(scena, "proveniența sursei este persistată", CuSpatiu(os =>
            os.GetObjectByKey<NIR>(nir).TranzactieReceptieSursaId != null));
    }

    void Diferente() {
        if (!Privat) Comanda(os => {
            var f = os.CreateObject<CodFunctional>(); f.Cod = Marcaj; f.Denumire = "Funcțional NIR"; functionalNir = f.ID;
            var b = os.CreateObject<SursaFinantare>(); b.Cod = Marcaj; b.Denumire = "Finanțare NIR"; finantareNir = b.ID;
            os.CommitChanges();
        });
        var (f, nir) = Constatat(3);
        var stamp = Amprenta(f.Id);
        Verifica("SC-NIR-18", "dry-run parțial acceptat", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(nir)).Count == 0);
        FaraEfecte("SC-NIR-18", nir); Opereaza(nir); Delta("SC-NIR-18", nir, Clarificare, 25);
        var lipsa = Explicatia(nir, N.FelTranzactie.Operare).Origini.Single().Explicatie.Linii().Single().Iesiri.Single();
        Verifica("SC-CIT-98", "NIR-minus: ieșirea 1/25 e declarată din recepția facturii, fără sold citit",
            lipsa is { Cantitate: 1, Valoare: 25, SoldInainte: null, Sursa: SurseValoare.Receptie }
            && lipsa.Unitate.Id == f.Linii[0].Lot);
        Sold("SC-NIR-18", f.Linii[0], 3, 75); Datorie("SC-NIR-18", f.Id, 100);
        Verifica("SC-NIR-18", "factura intactă", stamp == Amprenta(f.Id));
        Refuza("SC-NIR-18", () => Anuleaza(f.Id), "conex");
        var (zero, nzero) = Constatat(0); Opereaza(nzero); Delta("SC-NIR-20", nzero, Clarificare, 100);
        Sold("SC-NIR-20", zero.Linii[0], 0, 0);
        var (plus, nplus) = Constatat(4);
        var lotNou = AdaugaPlus(nplus, 1, 10); Opereaza(nplus);
        Sold("SC-NIR-19", plus.Linii[0], 4, 100); Sold("SC-NIR-19", lotNou, 1, 10);
        Verifica("SC-NIR-19", "plus 10 pe 408, factura 100 pe furnizor", Net(nplus, Nesosite) == -10 && Net(plus.Id, ContFurnizor) == -100);
        var partidaPlus = N.Unitate.DeschidePartida(Cont(Nesosite), Furnizor, nplus, Ianuarie);
        SoldPartida("SC-NIR-19", partidaPlus.Id, Ianuarie, -10);
        var (mix, nmix) = Constatat(3); var lotMix = AdaugaPlus(nmix, 1, 10); Opereaza(nmix);
        Sold("SC-NIR-09b", mix.Linii[0], 3, 75); Sold("SC-NIR-09b", lotMix, 1, 10);
        Verifica("SC-NIR-09b", "grup 85/25/100/10", Net(nmix, Stoc) == -15 && Net(nmix, Clarificare) == 25
            && Net(nmix, Nesosite) == -10 && Net(mix.Id, ContFurnizor) == -100);
        foreach (var cauza in new[] { CauzaDiferentei.Perisabilitate, CauzaDiferentei.Neimputabila, CauzaDiferentei.PeDrum }) {
            var (fc, nc) = Constatat(3, cauza); Opereaza(nc);
            Delta("SC-NIR-21/" + cauza, nc, cauza == CauzaDiferentei.PeDrum ? Drum : Privat ? "602" : "602.01.00", 25);
            Sold("SC-NIR-21", fc.Linii[0], 3, 75);
        }
        foreach (var cauza in new[] { CauzaDiferentei.Imputabila, CauzaDiferentei.Plus }) {
            var (_, nc) = Constatat(3, cauza);
            Refuza("SC-NIR-21/refuz", () => Opereaza(nc), DeclarantNir.CauzaInvalida); FaraEfecte("SC-NIR-21/refuz", nc);
        }
        var (fi, ni) = Constatat(3, CauzaDiferentei.Imputabila, Client); Opereaza(ni);
        Delta("SC-NIR-22", ni, Imputat, 25);
        SoldPartida("SC-NIR-22", N.Unitate.DeschidePartida(Cont(Imputat), Client, ni, Ianuarie).Id, Ianuarie, 25);
        Identitate(); PoliticaIstorica(); PeDrumCumulativ(); RefuzStoc(); DouaActive();
        Capitalizat(); Avans();
        ImputariDistincte(); Concurenta(); ReconciliereDelta(); ZeroIstoric();
        ProvenientaCorectiei(); CitireaSursei(); AnalizaIstorica(); ImputatInert(); FacturaPe408();
        AcoperireStoc();
        (facturaDeCorectat, nirDeCorectat) = Constatat(3); Opereaza(nirDeCorectat);
    }

    LinieScena AdaugaPlus(Guid nir, decimal q, decimal pret) => CuSpatiu(os => {
        var doc = os.GetObjectByKey<NIR>(nir);
        var l = os.CreateObject<NirDetaliu>(); l.Document = doc; l.Pozitie = doc.Detalii.Count + 1;
        l.TipMaterialId = Tip(os, Stoc); l.Cantitate = q; l.PretUnitar = pret; l.CodEconomicId = Economic;
        l.CodFunctionalId = functionalNir; l.SursaFinantareId = finantareNir;
        var p = os.CreateObject<Produs>(); p.Cod = Marcaj + "-P" + ++numar; p.Denumire = p.Cod; p.UM = "BUC";
        p.TipMaterialId = l.TipMaterialId; l.ProdusId = p.ID;
        var lot = l.CreeazaLot(os, p, os.GetObjectByKey<Gestiune>(Magazie));
        os.CommitChanges(); return new LinieScena(l.ID, lot.ID, p.ID);
    });

    void Identitate() {
        foreach (var caz in new[] { "lot", "stearsa", "duplicata" }) {
            var (_, nir) = Constatat(3);
            var alta = Culege((4, 25, null));
            Comanda(os => {
                var d = os.GetObjectByKey<NIR>(nir); var l = d.Detalii.OfType<NirDetaliu>().Single();
                if (caz == "lot") l.LotId = alta.Linii[0].Lot;
                if (caz == "stearsa") os.Delete(l);
                if (caz == "duplicata") {
                    var c = os.CreateObject<NirDetaliu>(); c.Document = d; c.TipMaterialId = l.TipMaterialId;
                    c.LotId = l.LotId; c.Cantitate = l.Cantitate; c.LinieSursaReceptieId = l.LinieSursaReceptieId;
                    c.CodEconomicId = l.CodEconomicId; c.CodFunctionalId = l.CodFunctionalId; c.SursaFinantareId = l.SursaFinantareId;
                }
                os.CommitChanges();
            });
            RefuzDeclaratie("SC-NIR-23/" + caz, nir, CoduriRefuz.NirDeltaStructura);
            Refuza("SC-NIR-23/" + caz, () => Opereaza(nir), caz == "stearsa" ? "Documentul nu are nicio linie" : CoduriRefuz.NirDeltaStructura); FaraEfecte("SC-NIR-23/" + caz, nir);
        }
    }

    void PoliticaIstorica() {
        var (f, nir) = Constatat(3); Opereaza(nir);
        var politica = CuSpatiu(os => os.GetObjectsQuery<PoliticaDiferenta>().Single(p => p.TipDocument.Cod == "NIR"
            && p.Clasa.Cod == "M" && p.Cauza == CauzaDiferentei.InClarificare).ID);
        var contVechi = Cont(Clarificare);
        Comanda(os => { os.GetObjectByKey<PoliticaDiferenta>(politica).ContId = Cont(Drum); os.CommitChanges(); });
        try {
            var nou = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(nir, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
            Verifica("SC-NIR-24", "storno inversează contul vechi", Net(nir, Clarificare) == 0 && Net(nir, Drum) == 0);
            Schimba(nou, 2); Opereaza(nou); Delta("SC-NIR-24", nou, Drum, 50); Sold("SC-NIR-24", f.Linii[0], 2, 50);
        }
        finally { Comanda(os => { os.GetObjectByKey<PoliticaDiferenta>(politica).ContId = contVechi; os.CommitChanges(); }); }
        var (fa, na) = Constatat(3); Opereaza(na);
        var tip = CuSpatiu(os => os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "NIR").ID);
        Comanda(os => { os.GetObjectByKey<TipDocument>(tip).PosteazaInCub = false; os.CommitChanges(); });
        try { Anuleaza(na); FaraEfecte("SC-NIR-26", na); Sold("SC-NIR-26", fa.Linii[0], 4, 100); }
        finally { Comanda(os => { os.GetObjectByKey<TipDocument>(tip).PosteazaInCub = true; os.CommitChanges(); }); }
        Opereaza(na); Delta("SC-NIR-26", na, Clarificare, 25);
    }

    void PeDrumCumulativ() {
        var (f, nir) = Constatat(3, CauzaDiferentei.PeDrum); Opereaza(nir);
        var nou = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(nir, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
        Schimba(nou, 4, CauzaDiferentei.PeDrum); Opereaza(nou);
        Sold("SC-NIR-25", f.Linii[0], 4, 100);
        Verifica("SC-NIR-25", "drum stins, noua deltă zero, proveniență păstrată fără Autogenerat",
            Net(nir, Drum) == 0 && CuSpatiu(os => !os.GetObjectByKey<NIR>(nou).Autogenerat
                && os.GetObjectByKey<NIR>(nou).TranzactieReceptieSursaId != null
                && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == nou)));
        Refuza("SC-NIR-25", () => Storneaza(f.Id, new(An, 1, 25)), C.ReceptiiConexe.Activa);
    }

    void RefuzStoc() {
        var (f, initial) = Constatat(4); Opereaza(initial);
        var bcs = Consum(f.Linii[0].Lot!.Value, 2); Opereaza(bcs);
        var nir = CuSpatiu(os => ComenziDocument.Sistem(os).Corecteaza(initial, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
        Schimba(nir, 0);
        Verifica("SC-NIR-stoc", "dry-run vede consumul existent", CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(nir))
            .Any(e => e.Contains(C.ReceptiiConexe.Stoc)));
        Refuza("SC-NIR-stoc", () => Opereaza(nir), C.ReceptiiConexe.Stoc); FaraEfecte("SC-NIR-stoc", nir);
    }

    void DouaActive() {
        var (f, nir) = Constatat(3); Opereaza(nir);
        var alt = CuSpatiu(os => {
            var n = os.CreateObject<NIR>(); n.Data = Ianuarie; n.DataInregistrare = Ianuarie;
            n.PredatorId = Furnizor; n.PrimitorId = Magazie; n.SursaReceptieiId = f.Id;
            var l = os.CreateObject<NirDetaliu>(); l.Document = n; l.TipMaterialId = Tip(os, Stoc);
            l.LotId = f.Linii[0].Lot; l.Cantitate = 4; l.LinieSursaReceptieId = f.Linii[0].Id;
            l.CodEconomicId = Economic; os.CommitChanges(); return n.ID;
        });
        Refuza("SC-NIR-27", () => Opereaza(alt), C.ReceptiiConexe.Activa); FaraEfecte("SC-NIR-27", alt);
    }

    void CorectieConexaInchisa() {
        var nou = Corecteaza(nirDeCorectat); Schimba(nou, 2); Opereaza(nou);
        Sold("SC-NIR-28", facturaDeCorectat.Linii[0], 3, 75);
        Sold("SC-NIR-28", facturaDeCorectat.Linii[0], 2, 50, Februarie);
        Verifica("SC-NIR-28", "corecția păstrează sursa și nu se declară autogenerată", CuSpatiu(os => {
            var d = os.GetObjectByKey<NIR>(nou);
            return !d.Autogenerat && d.SursaReceptieiId == facturaDeCorectat.Id;
        }));
    }

    void ImputariDistincte() {
        var f = Factura(Ianuarie, new LinieFctScena(4, 10, Privat ? "SFD" : "CAP0"),
            new LinieFctScena(4, 15, Privat ? "SFD" : "CAP0"));
        var nir = Opereaza(f.Id).ConexId!.Value;
        Comanda(os => {
            var linii = os.GetObjectByKey<NIR>(nir).Detalii.OfType<NirDetaliu>().OrderBy(l => l.Pozitie).ToList();
            foreach (var l in linii) { l.CodFunctionalId = functionalNir; l.SursaFinantareId = finantareNir; }
            linii[0].Cantitate = 3; linii[0].CauzaDiferentei = CauzaDiferentei.Imputabila; linii[0].PartenerDiferentaId = Furnizor;
            linii[1].Cantitate = 3; linii[1].CauzaDiferentei = CauzaDiferentei.Imputabila; linii[1].PartenerDiferentaId = Client;
            os.CommitChanges();
        });
        Opereaza(nir);
        SoldPartida("SC-NIR-22/doi", N.Unitate.DeschidePartida(Cont(Imputat), Furnizor, nir, Ianuarie).Id, Ianuarie, 10);
        SoldPartida("SC-NIR-22/doi", N.Unitate.DeschidePartida(Cont(Imputat), Client, nir, Ianuarie).Id, Ianuarie, 15);
        var angajat = CuSpatiu(os => {
            var a = os.CreateObject<Angajat>(); a.Cod = Marcaj + "-ANG"; a.Denumire = "Imputat NIR";
            os.CommitChanges(); return a.ID;
        });
        var (_, n) = Constatat(3, CauzaDiferentei.Imputabila, angajat);
        if (!Privat) {
            Comanda(os => { os.GetObjectByKey<NIR>(n).Detalii.OfType<NirDetaliu>().Single().CodFunctionalId = null; os.CommitChanges(); });
            Verifica("SC-NIR-22/analiza", "imputarea fără clasificația cerută este refuzată în dry-run",
                CuSpatiu(os => ComenziDocument.Sistem(os).Valideaza(n)).Any(e => e.Contains("428.01.02") && e.Contains("Cod funcțional")));
            Refuza("SC-NIR-22/analiza", () => Opereaza(n), "Cod funcțional"); FaraEfecte("SC-NIR-22/analiza", n);
            Schimba(n, 3, CauzaDiferentei.Imputabila, angajat);
        }
        Opereaza(n);
        var cont = Privat ? "461" : "428.01.02";
        SoldPartida("SC-NIR-22/angajat", N.Unitate.DeschidePartida(Cont(cont), angajat, n, Ianuarie).Id, Ianuarie, 25);
    }

    void Capitalizat() {
        var cod = Privat ? "N21" : "CAP21";
        var id = Tva(cod);
        var anterior = CuSpatiu(os => os.GetObjectByKey<TipTva>(id).Regim);
        if (Privat) Comanda(os => { os.GetObjectByKey<TipTva>(id).Regim = RegimTva.Capitalizat; os.CommitChanges(); });
        try {
            var (cap, ncap) = Constatat(3, tva: cod); Opereaza(ncap);
            Delta("SC-NIR-29", ncap, Clarificare, 30.25m); Sold("SC-NIR-29", cap.Linii[0], 3, 90.75m);
        }
        finally { if (Privat) Comanda(os => { os.GetObjectByKey<TipTva>(id).Regim = anterior; os.CommitChanges(); }); }
    }

    void Concurenta() {
        var (f, nir) = Constatat(3);
        var alt = CuSpatiu(os => {
            var n = os.CreateObject<NIR>(); n.Data = Ianuarie; n.DataInregistrare = Ianuarie;
            n.PredatorId = Furnizor; n.PrimitorId = Magazie; n.SursaReceptieiId = f.Id;
            var l = os.CreateObject<NirDetaliu>(); l.Document = n; l.TipMaterialId = Tip(os, Stoc);
            l.LotId = f.Linii[0].Lot; l.Cantitate = 2; l.LinieSursaReceptieId = f.Linii[0].Id;
            l.CodEconomicId = Economic; os.CommitChanges(); return n.ID;
        });
        Task<string> concurent;
        using (var os = Deschide()) using (var tx = TranzactieComanda.Incepe(os)) {
            MotorOperare.Opereaza(os, os.GetObjectByKey<Document>(nir)); os.CommitChanges();
            concurent = Task.Run(() => { try { Opereaza(alt); return "OK"; } catch (Exception e) { return e.Message; } });
            var db = ((EFCoreObjectSpace)os).DbContext;
            var pid = ((Npgsql.NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
            var ceas = System.Diagnostics.Stopwatch.StartNew();
            var blocat = false;
            while (ceas.Elapsed < TimeSpan.FromSeconds(5) && !concurent.IsCompleted) {
                db.Database.ExecuteSqlRaw("SELECT pg_stat_clear_snapshot()");
                blocat = db.Database.SqlQuery<int>($"""SELECT count(*)::int AS "Value" FROM pg_stat_activity WHERE {pid} = ANY(pg_blocking_pids(pid))""").Single() > 0;
                if (blocat) break;
                Thread.Sleep(30);
            }
            Verifica("SC-NIR-27/concurent", "a doua comandă așteaptă tranzacția primei", blocat);
            tx.Commit();
        }
        Verifica("SC-NIR-27/concurent", "după commit, al doilea cumul este refuzat", concurent.GetAwaiter().GetResult().Contains(C.ReceptiiConexe.Activa));
        FaraEfecte("SC-NIR-27/concurent", alt); Sold("SC-NIR-27/concurent", f.Linii[0], 3, 75);
    }

    void ReconciliereDelta() {
        var (f, nir) = Constatat(3); Opereaza(nir);
        var (f0, n0) = Constatat(4); Opereaza(n0);
        using var os = Deschide(); var db = ((EFCoreObjectSpace)os).DbContext;
        Guid[] set = [f.Id, nir, f0.Id, n0];
        var raport = ReconciliereCub.Nir(db, set);
        Verifica("SC-NIR-31", "exact un grup cu deltă, inclusiv diferența 401 de 25",
            raport.Grupuri == 1 && raport.Diferente.Any(r => r.Cheie.Contains(" C " + ContFurnizor + " ") && r.Delta == 25));
        var abateri = ReconciliereCub.Ruleaza(db, set);
        Verifica("SC-NIR-31", "(a)/(b) rămân verzi, grupul fără deltă rămâne comparabil", abateri.Count == 0);
        var lot = f0.Linii[0].Lot!.Value;
        using var tx = db.Database.BeginTransaction();
        db.Database.ExecuteSqlInterpolated($"""UPDATE "Postare" SET "Valoare" = "Valoare" + 1 WHERE "DocumentId" = {f0.Id} AND "Unitate" = {lot} AND "Spatiu" = 2""");
        Verifica("SC-NIR-31", "capcana grupului fără deltă nu este exclusă", ReconciliereCub.Ruleaza(db, set).Any(r => r.Litera == "(a) contabil"));
        tx.Rollback();
    }

    void Avans() {
        var cod = Privat ? "4091" : "409.01.01";
        var f = Factura(Ianuarie, new LinieFctScena(1, 100, Privat ? "SFD" : "CAP0", false, Tip: cod));
        var rezultat = Opereaza(f.Id);
        Verifica("SC-NIR-30/avans", "avansul 100 nu generează NIR, lot sau postări cantitative",
            rezultat.ConexId == null && Net(f.Id, cod) == 100 && CuSpatiu(os =>
                !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == f.Id && p.Cantitate != 0)
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(l => l.DocumentId == f.Id && l.LotId != null)));
        Verifica("SC-NIR-30/avans", "totalul de stins este datoria 100, nu și creanța avansului", CuSpatiu(os =>
            os.GetObjectByKey<FacturaIntrare>(f.Id).TotalStingere == 100m && ImperechereService.Total(os, f.Id) == 100m));
        void Stadiu(string pas, decimal ramas) => Verifica("SC-NIR-37", $"{pas}: panou, listă și serviciu 100/{ramas}/{100 - ramas}; creanța avansului rămâne 100",
            CuSpatiu(os => {
                var panou = Atlas.Conta.BackOffice.Module.Api.Trz.ImperechereApply.Stingeri(os, f.Id);
                var rand = Atlas.Conta.BackOffice.Module.Proiectii.ImperecheriProiectii
                    .DocumenteCuRest(os, Furnizor, SensStingere.Datorie).SingleOrDefault(r => r.DocumentId == f.Id);
                return panou.Total == 100m && panou.Ramas == ramas && panou.Asignat == 100m - ramas
                    && ImperechereService.Ramas(os, f.Id) == ramas && ImperechereService.Asignat(os, f.Id) == 100m - ramas
                    && (ramas == 0m ? rand == null : rand is { Total: 100m } && rand.Rest == ramas && rand.Asignat == 100m - ramas)
                    && (!Privat || C.Citiri.Partide.Solduri(os, DateOnly.MaxValue)
                        .Where(s => s.ContId == Cont(cod) && s.PartenerId == Furnizor).Sum(s => s.Debit - s.Credit) == 100m);
            }));
        Stadiu("nestins", 100m);
        var p40 = Trezorerie(false, 40); Opereaza(p40.Id);
        Imperecheaza(p40.Id, f.Id, 40, Ianuarie);
        Stadiu("stingere parțială 40", 60m);
        var p60 = Trezorerie(false, 60); Opereaza(p60.Id);
        var integral = Imperecheaza(p60.Id, f.Id, 60, Ianuarie);
        Stadiu("stingere integrală", 0m);
        Comanda(os => ImperechereService.Desfa(os, integral, Ianuarie));
        Stadiu("după desfacerea stingerii de 60", 60m);
    }

    void ZeroIstoric() {
        var (f, nir) = Constatat(4);
        var politica = CuSpatiu(os => os.GetObjectsQuery<PoliticaConex>().Single(p => p.TipDocumentSursa.Cod == "FCT").ID);
        var tip = CuSpatiu(os => os.GetObjectsQuery<TipDocument>().Single(t => t.Cod == "FCT").ID);
        var valori = CuSpatiu(os => {
            var v = ((EFCoreObjectSpace)os).DbContext.Entry(os.GetObjectByKey<PoliticaConex>(politica)).CurrentValues;
            return v.Properties.ToDictionary(p => p.Name, p => v[p]);
        });
        Comanda(os => { os.Delete(os.GetObjectByKey<PoliticaConex>(politica)); os.GetObjectByKey<TipDocument>(tip).PosteazaInCub = false; os.CommitChanges(); });
        try {
            Opereaza(nir); Sold("SC-NIR-30", f.Linii[0], 4, 100);
            Verifica("SC-NIR-30", "dovada istorică ține fără politica conexului și cu flagul sursei oprit", CuSpatiu(os =>
                os.GetObjectByKey<NIR>(nir).TranzactieReceptieSursaId != null
                && !os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == nir)));
        }
        finally {
            Comanda(os => {
                var p = os.CreateObject<PoliticaConex>();
                ((EFCoreObjectSpace)os).DbContext.Entry(p).CurrentValues.SetValues(valori);
                os.GetObjectByKey<TipDocument>(tip).PosteazaInCub = true; os.CommitChanges();
            });
        }
    }
}
