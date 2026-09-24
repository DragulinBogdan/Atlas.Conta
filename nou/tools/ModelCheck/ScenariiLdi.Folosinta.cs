using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp.EFCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiLdi {
    const string TipFolosinta = "303.02.00", CostFolosinta = "603";

    Action Folosinta() {
        if (Privat) return () => { };
        var fct = Receptioneaza(new LinieFctScena(4, 25, "CAP0", Tip: TipFolosinta));
        var lot = fct.Linii[0];
        var amprenta = Amprenta(fct.Id);
        Sold("SC-LDI-18", lot, 4, 100);
        Verifica("SC-LDI-18", "FCT/NIR: o singură recepție în cub, gestiune reală", CuSpatiu(os =>
            os.GetObjectsQuery<C.Postare>().Count(p => p.Unitate == lot.Lot && p.Gestiune == Magazie) == 1));
        var btr = Iesire(true, (lot, 2)); Opereaza(btr.Id);
        RandScena[] transfer = [
            new(Cont(TipFolosinta), N.Latura.Debit, -50, -2, Magazie, lot.Lot, lot.Produs,
                Linie: btr.Linii[0].Id, Spatiu: N.Spatiu.Stoc),
            new(Cont(TipFolosinta), N.Latura.Debit, 50, 2, Destinatie, lot.Lot, lot.Produs,
                Linie: btr.Linii[0].Id, Spatiu: N.Spatiu.Stoc)];
        Postari("SC-LDI-18", btr.Id, N.FelTranzactie.Transfer, Ianuarie, transfer);
        Sold("SC-LDI-18", lot, 2, 50); Sold("SC-LDI-18", lot, 2, 50, gest: Destinatie);
        var minus = CulegeLa(Destinatie, Ianuarie,
            new Linie(DirectieDiferenta.Minus, 1, Lot: lot, Tip: TipFolosinta));
        Opereaza(minus.Id);
        var minusRand = Randuri(minus, 0, -1, 25, TipFolosinta, CostFolosinta, Destinatie);
        Postari("SC-LDI-18", minus.Id, N.FelTranzactie.Operare, Ianuarie, minusRand);
        var bcs = Consum(lot.Lot!.Value, 1, Destinatie);
        var linieBcs = CuSpatiu(os => {
            var l = os.GetObjectsQuery<DocumentDetaliu>().Single(l => l.DocumentId == bcs);
            l.TipMaterialId = Tip(os, TipFolosinta); os.CommitChanges(); return l.ID;
        });
        Opereaza(bcs);
        RandScena[] consum = [
            new(Cont(TipFolosinta), N.Latura.Credit, 25, -1, Destinatie, lot.Lot, lot.Produs,
                Linie: linieBcs, Spatiu: N.Spatiu.Stoc),
            new(Cont(CostFolosinta), N.Latura.Debit, 25, 1, Loc, lot.Lot, lot.Produs,
                Linie: linieBcs, Spatiu: N.Spatiu.Stoc)];
        Postari("SC-LDI-18", bcs, N.FelTranzactie.Operare, new(An, 1, 10), consum);
        Sold("SC-LDI-18", lot, 2, 50); Sold("SC-LDI-18", lot, 0, 0, gest: Destinatie);
        Verifica("SC-LDI-18", "registrele păstrează Folosinta pe cele două gestiuni", CuSpatiu(os => {
            var r = os.GetObjectsQuery<RegistruStoc>().Where(r => r.LotId == lot.Lot).ToList();
            return r.Where(r => r.RepartitorId == Magazie || r.RepartitorId == Destinatie).All(r => r.TipStoc == TipStoc.Folosinta)
                && r.Where(r => r.RepartitorId == Magazie).Sum(r => r.Cantitate) == 2
                && r.Where(r => r.RepartitorId == Destinatie).Sum(r => r.Cantitate) == 0;
        }));
        using (var os = Deschide()) {
            var raport = DiagnosticValoriStoc.Citeste(((EFCoreObjectSpace)os).DbContext, new(An, 1, 31), [lot.Lot.Value]);
            Verifica("SC-LDI-18", "diagnostic comun complet, două gestiuni, fără diferențe", !raport.AreAbateri
                && raport.Pozitii.Count == 2 && raport.IstoricFolosinta.Any(r => r.Cheie.Tip == TipStoc.Folosinta));
        }
        var insuf = CulegeLa(Destinatie, Ianuarie, new Linie(DirectieDiferenta.Minus, 3, Lot: lot, Tip: TipFolosinta));
        RefuzDeclaratie("SC-LDI-18", insuf.Id, "STOC_INSUFICIENT");
        Refuza("SC-LDI-18", () => Opereaza(insuf.Id), "Sold negativ"); FaraEfecte("SC-LDI-18", insuf.Id);
        var stamp = Amprenta(btr.Id);
        Refuza("SC-LDI-18", () => Storneaza(btr.Id, new(An, 1, 20)), "Sold negativ");
        Verifica("SC-LDI-18", "transferul cu ieșiri rămâne intact", stamp == Amprenta(btr.Id));
        var data = new DateOnly(An, 1, 20);
        Storneaza(bcs, data); Storneaza(minus.Id, data); Storneaza(btr.Id, new(An, 1, 21));
        Postari("SC-LDI-18", bcs, N.FelTranzactie.Storno, data, Inverse(consum));
        Postari("SC-LDI-18", minus.Id, N.FelTranzactie.Storno, data, Inverse(minusRand));
        Postari("SC-LDI-18", btr.Id, N.FelTranzactie.Storno, new(An, 1, 21), Inverse(transfer));
        Sold("SC-LDI-18", lot, 4, 100); Sold("SC-LDI-18", lot, 0, 0, gest: Destinatie);
        Verifica("SC-LDI-18", "recepția originală nemodificată", amprenta == Amprenta(fct.Id));
        var plus = CulegeLa(Destinatie, Ianuarie, new Linie(DirectieDiferenta.Plus, 1, 25, Tip: TipFolosinta));
        Opereaza(plus.Id); Anuleaza(plus.Id); FaraEfecte("SC-LDI-19", plus.Id); Opereaza(plus.Id);
        var plusRand = Randuri(plus, 0, 1, 25, TipFolosinta, CostFolosinta, Destinatie);
        Postari("SC-LDI-19", plus.Id, N.FelTranzactie.Operare, Ianuarie, plusRand);
        Storneaza(plus.Id, data); Postari("SC-LDI-19", plus.Id, N.FelTranzactie.Storno, data, Inverse(plusRand));
        Sold("SC-LDI-19", plus.Linii[0], 0, 0, gest: Destinatie);
        IstoricFolosinta();
        var corectie = CulegeLa(Destinatie, Ianuarie, new Linie(DirectieDiferenta.Plus, 2, 25, Tip: TipFolosinta));
        Opereaza(corectie.Id);
        return () => CorectieFolosinta(corectie);
    }

    void CorectieFolosinta(FacturaScena original) {
        Refuza("SC-LDI-21", () => Anuleaza(original.Id), "închis");
        var id = Corecteaza(original.Id);
        var nou = CuSpatiu(os => {
            var doc = os.GetObjectByKey<ListaDiferenteInventar>(id);
            var l = doc.Detalii.OfType<ListaDiferenteInventarDetaliu>().Single();
            l.Cantitate = 3; l.PretEvaluare = 20;
            Verifica("SC-LDI-21", "corecție legată, lot nou în aceeași gestiune", doc.CorecteazaId == original.Id
                && doc.PredatorId == Destinatie && l.LotId != original.Linii[0].Lot);
            os.CommitChanges(); return new FacturaScena(id, [new(l.ID, l.LotId, l.ProdusId)]);
        });
        Opereaza(id);
        Postari("SC-LDI-21", original.Id, N.FelTranzactie.Storno, Februarie,
            Inverse(Randuri(original, 0, 2, 50, TipFolosinta, CostFolosinta, Destinatie)));
        Postari("SC-LDI-21", id, N.FelTranzactie.Operare, Februarie,
            Randuri(nou, 0, 3, 60, TipFolosinta, CostFolosinta, Destinatie));
        Sold("SC-LDI-21", original.Linii[0], 2, 50, gest: Destinatie);
        Sold("SC-LDI-21", original.Linii[0], 0, 0, Februarie, Destinatie);
        Sold("SC-LDI-21", nou.Linii[0], 3, 60, Februarie, Destinatie);
    }

    void IstoricFolosinta() {
        var regula = CuSpatiu(os => os.GetObjectsQuery<RegulaStoc>()
            .Single(r => r.TipDocument.Cod == "LDI" && r.Clasa.Cod == "OF" && r.Latura == LaturaDocument.Predator).ID);
        void Schimba(TipStoc tip) => Comanda(os => { os.GetObjectByKey<RegulaStoc>(regula).TipStoc = tip; os.CommitChanges(); });
        var original = Culege(new Linie(DirectieDiferenta.Plus, 1, 25, Tip: TipFolosinta));
        try { Schimba(TipStoc.Magazie); Opereaza(original.Id); }
        finally { Schimba(TipStoc.Folosinta); }
        var istoric = CuSpatiu(os => os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == original.Id)
            .Select(r => new { r.ID, r.TipStoc, r.LotId, r.RepartitorId, r.Cantitate, r.Valoare }).Single());
        Verifica("SC-LDI-22", "fixture real: cheia veche Magazie", istoric.TipStoc == TipStoc.Magazie);
        var btr = Iesire(true, (original.Linii[0], 1));
        RefuzDeclaratie("SC-LDI-22", btr.Id, "STOC_INSUFICIENT");
        Refuza("SC-LDI-22", () => Opereaza(btr.Id), "Sold negativ"); FaraEfecte("SC-LDI-22", btr.Id);
        Storneaza(original.Id, new(An, 1, 20));
        Postari("SC-LDI-22", original.Id, N.FelTranzactie.Storno, new(An, 1, 20),
            Inverse(Randuri(original, 0, 1, 25, TipFolosinta, CostFolosinta)));
        Verifica("SC-LDI-22", "storno pe cheia istorică; original intact după schimbarea politicii", CuSpatiu(os => {
            var r = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == original.Id).ToList();
            return r.Count == 2 && r.All(r => r.TipStoc == TipStoc.Magazie && r.LotId == istoric.LotId
                && r.RepartitorId == istoric.RepartitorId) && r.Sum(r => r.Cantitate) == 0 && r.Sum(r => r.Valoare) == 0
                && r.Any(r => r.ID == istoric.ID && r.Cantitate == 1 && r.Valoare == 25 && !r.Storno);
        }));
        using var os = Deschide();
        var raport = DiagnosticValoriStoc.Citeste(((EFCoreObjectSpace)os).DbContext, new(An, 1, 31), [istoric.LotId]);
        Verifica("SC-LDI-22", "diagnostic: sold net zero nu ascunde cheia veche și inversarea ei", !raport.AreAbateri
            && raport.IstoricFolosinta.Count == 2 && raport.IstoricFolosinta.All(r => r.Cheie.Tip == TipStoc.Magazie)
            && raport.Linii().Count(l => l.StartsWith("CHEIE ISTORICĂ")) == 2);
    }
}
