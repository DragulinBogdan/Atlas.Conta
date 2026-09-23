using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed class ScenariiItv(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "ITV", 2007) {
    DateOnly Sfarsit => new(An, 1, 31);
    FacturaScena Baza(decimal deductibila, decimal colectata) {
        var d = Nota(Ianuarie, new LinieNtcScena("4426", "628", deductibila), new LinieNtcScena("628", "4427", colectata));
        Opereaza(d.Id); return d;
    }
    Guid Genereaza(int luna = 1) => CuSpatiu(os => {
        var d = InchidereTvaService.Genereaza(os, An, luna, Loc)
            ?? throw new InvalidOperationException("Scena cere un draft ITV.");
        os.CommitChanges(); return d.ID;
    });
    void Randuri(string id, Guid doc, N.FelTranzactie fel, DateOnly data, params (string Debit, string Credit, decimal V)[] linii) {
        using var os = Deschide();
        var detalii = os.GetObjectsQuery<NotaContabilaDetaliu>().Where(l => l.DocumentId == doc).ToList();
        var randuri = linii.SelectMany(l => {
            var linie = detalii.Single(d => d.ContDebitId == Cont(l.Debit) && d.ContCreditId == Cont(l.Credit));
            return new RandScena[] { new(Cont(l.Debit), N.Latura.Debit, l.V, Gestiune: Loc, Linie: linie.ID),
                new(Cont(l.Credit), N.Latura.Credit, l.V, Gestiune: Loc, Linie: linie.ID) };
        }).ToArray();
        Postari(id, doc, fel, data, randuri);
        Verifica(id, "numărul de linii și absența faptelor fiscale", detalii.Count == linii.Length
            && !os.GetObjectsQuery<RegistruTva>().Any(r => r.DocumentId == doc));
    }
    void Solduri(string id, DateOnly data, decimal deductibila, decimal colectata, decimal plata, decimal recuperat) {
        foreach (var (cont, asteptat) in new[] { ("4426", deductibila), ("4427", -colectata), ("4423", -plata), ("4424", recuperat) }) {
            var contId = Cont(cont);
            var net = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Cont == contId && p.Data <= data && p.Carte == N.Carte.Contabil)
                .Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare));
            Verifica(id, $"sold {cont} la {data}: {asteptat}", net == asteptat);
        }
    }
    protected override void Executa() {
        if (!Privat) {
            var r = CuSpatiu(os => InchidereTvaService.Incearca(os, An, 1, Loc));
            Verifica("SC-ITV-10", "profil inert, fără document", r.Motiv == MotivNegenerare.ProfilInert && r.Document == null);
            Verifica("SC-ITV-10", "zero ITV persistate", CuSpatiu(os => !os.GetObjectsQuery<InchidereTva>().Any(d => d.PredatorId == Loc)));
            return;
        }
        var gol = CuSpatiu(os => InchidereTvaService.Incearca(os, An, 1, Loc));
        Verifica("SC-ITV-09", "fără sold nu generează", gol.Document == null && gol.Motiv == MotivNegenerare.FaraSold);
        Varianta(21, 42, "SC-ITV-01", ("4427", "4426", 21), ("4427", "4423", 21));
        Varianta(42, 21, "SC-ITV-02", ("4427", "4426", 21), ("4424", "4426", 21));
        Varianta(21, 21, "SC-ITV-03", ("4427", "4426", 21));
        Stale(); Facturi(); PestePerioada();
    }
    void Varianta(decimal ded, decimal col, string id, params (string Debit, string Credit, decimal V)[] linii) {
        var baza = Baza(ded, col); var doc = Genereaza();
        Verifica(id, "dry-run acceptat", CuSpatiu(os => OperareApi.Valideaza(os, doc)).Count == 0);
        FaraEfecte(id, doc); Opereaza(doc);
        Randuri(id, doc, N.FelTranzactie.Operare, Sfarsit, linii);
        Solduri(id, Sfarsit, 0, 0, Math.Max(0, col - ded), Math.Max(0, ded - col));
        var vie = CuSpatiu(os => InchidereTvaService.Incearca(os, An, 1, Loc));
        Verifica("SC-ITV-09", "nu dublează închiderea vie", vie.Document == null && vie.Motiv == MotivNegenerare.InchidereVie);
        if (id == "SC-ITV-01") {
            Anuleaza(doc); FaraEfecte("SC-ITV-05", doc); Solduri("SC-ITV-05", Sfarsit, ded, col, 0, 0); Opereaza(doc);
        }
        Storneaza(doc, Sfarsit);
        Randuri("SC-ITV-04", doc, N.FelTranzactie.Storno, Sfarsit, linii.Select(l => (l.Debit, l.Credit, -l.V)).ToArray());
        Solduri("SC-ITV-04", Sfarsit, ded, col, 0, 0);
        if (id == "SC-ITV-01") {
            var inainte = Amprenta(doc);
            Refuza("SC-ITV-04", () => Storneaza(doc, Sfarsit), "Operat");
            Verifica("SC-ITV-04", "storno repetat fără scriere", Amprenta(doc) == inainte);
            var regenerat = Genereaza(); Opereaza(regenerat);
            Randuri("SC-ITV-04", regenerat, N.FelTranzactie.Operare, Sfarsit, linii); Storneaza(regenerat, Sfarsit);
        }
        Storneaza(baza.Id, Sfarsit); Solduri(id, Sfarsit, 0, 0, 0, 0);
    }
    void Stale() {
        var baza = Baza(21, 42); var doc = Genereaza();
        var plus = Nota(Ianuarie, new LinieNtcScena("4426", "628", 7)); Opereaza(plus.Id);
        Refuza("SC-ITV-08", () => Opereaza(doc), "s-au schimbat"); FaraEfecte("SC-ITV-08", doc);
        Verifica("SC-ITV-08", "citirea raportează stale", CuSpatiu(os => InchidereTvaApply.Citeste(os, doc).Stale) == true);
        var nou = CuSpatiu(os => InchidereTvaApply.Regenereaza(os, doc).DocumentId!.Value); Opereaza(nou);
        Randuri("SC-ITV-08", nou, N.FelTranzactie.Operare, Sfarsit, ("4427", "4426", 28), ("4427", "4423", 14));
        Solduri("SC-ITV-08", Sfarsit, 0, 0, 14, 0); Storneaza(nou, Sfarsit);
        var anterior = Genereaza();
        Refuza("SC-ITV-09", () => Genereaza(2), "draft");
        Comanda(os => InchidereTvaApply.Sterge(os, anterior));
        var ulterior = Genereaza(2);
        Refuza("SC-ITV-09", () => Genereaza(), "ulterioară");
        Comanda(os => InchidereTvaApply.Sterge(os, ulterior));
        Storneaza(plus.Id, Sfarsit); Storneaza(baza.Id, Sfarsit);
    }
    void Facturi() {
        var fct = Factura(Ianuarie, new LinieFctScena(1, 100, "N21", Stoc: false)); Opereaza(fct.Id);
        var fcl = CuSpatiu(os => {
            var d = os.CreateObject<FacturaIesire>(); d.Numar = Marcaj + "-FCL"; d.Data = Ianuarie;
            d.PredatorId = Magazie; d.PrimitorId = Client;
            var l = os.CreateObject<FacturaIesireDetaliu>(); l.Document = d; l.Pozitie = 1;
            l.TipMaterialId = Tip(os, "704"); l.Cantitate = 1; l.PretUnitar = 200;
            l.TipTvaId = Tva("N21"); os.CommitChanges(); return d.ID;
        });
        Opereaza(fcl);
        var amprenta = Amprenta(fct.Id) + Amprenta(fcl);
        var doc = Genereaza(); Opereaza(doc);
        Randuri("SC-ITV-11", doc, N.FelTranzactie.Operare, Sfarsit, ("4427", "4426", 21), ("4427", "4423", 21));
        Verifica("SC-ITV-11", "postările facturilor intacte", Amprenta(fct.Id) + Amprenta(fcl) == amprenta);
        Storneaza(doc, Sfarsit); Storneaza(fct.Id, Sfarsit); Storneaza(fcl, Sfarsit);
    }
    void PestePerioada() {
        Baza(21, 42); var doc = Genereaza(); Opereaza(doc); InchideIanuarie();
        Refuza("SC-ITV-06", () => Anuleaza(doc), "închis");
        var corectie = Corecteaza(doc); FaraEfecte("SC-ITV-07", corectie);
        Randuri("SC-ITV-06", doc, N.FelTranzactie.Storno, Februarie, ("4427", "4426", -21), ("4427", "4423", -21));
        Solduri("SC-ITV-06", Sfarsit, 0, 0, 21, 0); Solduri("SC-ITV-06", Februarie, 21, 42, 0, 0);
        Verifica("SC-ITV-07", "corecția legată păstrează datele", CuSpatiu(os => {
            var d = os.GetObjectByKey<Document>(corectie);
            return d.CorecteazaId == doc && d.Data == Sfarsit && d.DataInregistrare == Februarie && d.MotivCorectie == MotivCorectie.EroareMateriala;
        }));
        Verifica("SC-ITV-07", "corecția nu este stale", CuSpatiu(os => InchidereTvaApply.Citeste(os, corectie).Stale) == false);
        Opereaza(corectie);
        Randuri("SC-ITV-07", corectie, N.FelTranzactie.Operare, Februarie, ("4427", "4426", 21), ("4427", "4423", 21));
        Solduri("SC-ITV-07", Sfarsit, 0, 0, 21, 0); Solduri("SC-ITV-07", Februarie, 0, 0, 21, 0);
    }
}
