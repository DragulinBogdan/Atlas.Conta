using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// CV-D7: serializarea corecției cu consumul, stornarea și închiderea, pe conexiuni distincte; rollback-ul la eșec injectat.
sealed partial class ScenariiCvl {
    const string Esec = "E2E_SC_CVL_ESEC";

    void StornoDirect(Guid doc, DateOnly data) => Comanda(os => ComenziDocument.Sistem(os).Storneaza(doc, data));

    List<N.AlocareValorica> Alocari(Guid doc) =>
        Explicatia(doc, N.FelTranzactie.Operare).Origini.Single().Explicatie.Decizii.OfType<N.AlocareValorica>().ToList();

    void CorectieContraConsum() {
        foreach (var intaiCorectia in new[] { true, false }) {
            var id = "SC-CVL-21/" + (intaiCorectia ? "corecție → consum" : "consum → corecție");
            var caz = Lot();
            var a = Draft(Zi(15), (caz, 200m));
            var bon = Bon(caz, 60m, 15, opereaza: false);
            var r = intaiCorectia ? Serial(id, () => Opereaza(a), () => Opereaza(bon)) : Serial(id, () => Opereaza(bon), () => Opereaza(a));
            Verifica(id, "ambele comenzi trec", r == (Trecut, Trecut));
            Este(id, "aceeași stare în ambele ordini", caz, 40m, 480m, 720m, 1200m);
            if (intaiCorectia) {
                ValoriLinii(id, "bonul ia costul ajustat", bon, 720m);
                Verifica(id, "corecția are un singur beneficiar, stocul, cu 200; bonul nu primește alocare",
                    Alocari(a).Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(100m, 200m)])
                    && Valorice(a, N.FelTranzactie.Operare).Single().Repartitor == Magazie);
            }
            else {
                ValoriLinii(id, "bonul rămâne la costul inițial", bon, 600m);
                Verifica(id, "corecția alocă 80 stocului și 120 bonului",
                    Alocari(a).Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(40m, 80m), (60m, 120m)]));
            }
        }
    }

    void CorectieContraStorno() {
        foreach (var intaiCorectia in new[] { true, false }) {
            var id = "SC-CVL-22/" + (intaiCorectia ? "corecție → storno bon" : "storno bon → corecție");
            var caz = Lot();
            var bon = Bon(caz, 60m);
            var a = Draft(Zi(15), (caz, 200m));
            var r = intaiCorectia
                ? Serial(id, () => Opereaza(a), () => StornoDirect(bon, Zi(15)))
                : Serial(id, () => StornoDirect(bon, Zi(15)), () => Opereaza(a));
            Verifica(id, "ambele comenzi trec", r == (Trecut, Trecut));
            Este(id, "aceeași stare în ambele ordini, fără efect dublu", caz, 100m, 1200m, 0m, 1200m);
            var operari = Tranzactii(a).Count(t => t.Fel == N.FelTranzactie.Operare);
            Verifica(id, intaiCorectia ? "corecția primește compensarea de 120" : "corecția cade pe stocul intact, fără compensare",
                intaiCorectia
                    ? operari == 2 && Valorice(a, N.FelTranzactie.Operare).Where(p => p.Data == Zi(15) && p.Atribuit != null)
                        .Count(p => p.Valoare == -120m) == 2
                    : operari == 1 && Alocari(a).Select(d => (d.Pondere, d.Valoare)).SequenceEqual([(100m, 200m)]));
        }
    }

    void StornoContraStorno() {
        foreach (var intaiBonul in new[] { true, false }) {
            var id = "SC-CVL-29/" + (intaiBonul ? "storno bon → storno corecție" : "storno corecție → storno bon");
            var caz = Lot();
            var bon = Bon(caz, 60m);
            var a = Corectie(caz, 200m);
            var r = intaiBonul
                ? Serial(id, () => StornoDirect(bon, Zi(20)), () => StornoDirect(a, Zi(20)))
                : Serial(id, () => StornoDirect(a, Zi(20)), () => StornoDirect(bon, Zi(20)));
            Verifica(id, "ambele comenzi trec", r == (Trecut, Trecut));
            Este(id, "aceeași stare în ambele ordini", caz, 100m, 1000m, 0m, 1000m);
            var tranzactii = Tranzactii(a);
            var radacini = Valorice(a, N.FelTranzactie.Operare).Select(p => p.ID).ToHashSet();
            Verifica(id, intaiBonul
                    ? "compensarea de 120 pe corecție, apoi stornoul corecției inversează alocările și compensarea"
                    : "compensare zero; stornoul corecției inversează numai alocările",
                tranzactii.Count(t => t.Fel == N.FelTranzactie.Operare) == (intaiBonul ? 2 : 1)
                && tranzactii.Count(t => t.Fel == N.FelTranzactie.Storno) == 1
                && Valorice(a, N.FelTranzactie.Storno).Count == (intaiBonul ? 4 : 2)
                && Valorice(a, N.FelTranzactie.Storno).All(p => p.InversaDinId is Guid din && radacini.Contains(din)));
            Verifica(id, "nicio compensare orfană: fiecare atribuire a corecției țintește o postare existentă",
                CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.Tranzactie.DocumentId == a && p.Atribuit != null)
                    .All(p => os.GetObjectsQuery<C.Postare>().Any(t => t.ID == p.Atribuit))));
        }
    }

    (int Tranzactii, int Postari) Numara() => CuSpatiu(os =>
        (os.GetObjectsQuery<C.Tranzactie>().Count(), os.GetObjectsQuery<C.Postare>().Count()));

    // Eșecul vine din bază, pe inserarea tranzacției care îndeplinește condiția: comanda reală, fără cârlig în cod.
    string CuEsecInjectat(string conditie, Action comanda) {
        var creeaza = $"""
            CREATE FUNCTION e2e_sc_cvl_esec() RETURNS trigger LANGUAGE plpgsql AS $f$
            BEGIN IF {conditie} THEN RAISE EXCEPTION '{Esec}'; END IF; RETURN NULL; END $f$;
            CREATE TRIGGER e2e_sc_cvl_esec AFTER INSERT ON "Tranzactie" FOR EACH ROW EXECUTE FUNCTION e2e_sc_cvl_esec();
            """;
        Comanda(os => ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw(creeaza));
        try {
            comanda();
            return Trecut;
        }
        catch (Exception e) {
            for (var x = e; x != null; x = x.InnerException)
                if (x.Message.Contains(Esec, StringComparison.Ordinal)) return Esec;
            return $"{e.GetType().Name}: {e.Message.Split('\n')[0]}";
        }
        finally {
            Comanda(os => ((EFCoreObjectSpace)os).DbContext.Database.ExecuteSqlRaw("""
                DROP TRIGGER IF EXISTS e2e_sc_cvl_esec ON "Tranzactie";
                DROP FUNCTION IF EXISTS e2e_sc_cvl_esec();
                """));
        }
    }

    void EsecDupaCompensare() {
        var caz = Partial();
        var bon = BonulLui(caz);
        var a = Corectie(caz, 200m);
        var inainte = Numara();
        var rezultat = CuEsecInjectat($"""NEW."DocumentId" = '{a}'""", () => StornoDirect(bon, Zi(20)));
        Console.WriteLine($"     MĂSURAT (SC-CVL-23): storno bon cu eșec la scrierea compensării → „{rezultat}”.");
        Verifica("SC-CVL-23", "eșecul injectat la scrierea compensării oprește comanda", rezultat == Esec);
        Verifica("SC-CVL-23", "rollback integral: bonul Operat, zero tranzacții și postări noi",
            StareaDocumentului(bon) == StareDocument.Operat && Numara() == inainte);
        Este("SC-CVL-23", "starea dinaintea comenzii", caz, 40m, 480m, 720m, 1200m);

        var doua = Partial();
        var bon2 = BonulLui(doua);
        var (b, c) = (Corectie(doua, 200m), Corectie(doua, 100m));
        Este("SC-CVL-29", "două corecții active", doua, 40m, 520m, 780m, 1300m);
        inainte = Numara();
        var storno = $"'{Zi(20):yyyy-MM-dd}'";
        rezultat = CuEsecInjectat($"""
            NEW."DocumentId" IN ('{b}', '{c}') AND NEW."Data" = {storno}
            AND EXISTS (SELECT 1 FROM "Tranzactie" t WHERE t."DocumentId" IN ('{b}', '{c}')
                AND t."DocumentId" <> NEW."DocumentId" AND t."Data" = {storno})
            """, () => StornoDirect(bon2, Zi(20)));
        Console.WriteLine($"     MĂSURAT (SC-CVL-29): storno bon cu eșec după prima compensare → „{rezultat}”.");
        Verifica("SC-CVL-29", "eșecul injectat după scrierea primei compensări oprește comanda", rezultat == Esec);
        Verifica("SC-CVL-29", "rollback integral: bonul Operat, zero tranzacții noi pe bon și pe cele două corecții",
            StareaDocumentului(bon2) == StareDocument.Operat && Numara() == inainte
            && Tranzactii(bon2).Count == 1 && Tranzactii(b).Count == 1 && Tranzactii(c).Count == 1);
        Este("SC-CVL-29", "starea dinaintea comenzii", doua, 40m, 520m, 780m, 1300m);
        Storneaza(bon2, Zi(20));
        Este("SC-CVL-29", "după eliberare, același storno compensează 180", doua, 100m, 1300m, 0m, 1300m);
    }

    // Snapshot-ul de stoc e pe lot; cel contabil nu poartă lotul și partida, deci se compară cu suma faptelor pe repartitorii scenei.
    void SnapshotEste(string id, string pas, Caz caz, int luna, decimal q, decimal v, decimal c, decimal d) {
        var sfarsit = new DateOnly(An, luna, DateTime.DaysInMonth(An, luna));
        var (cantitate, valoare, snapshot, cub) = CuSpatiu(os => {
            var stoc = os.GetObjectsQuery<SoldPerioadaStoc>()
                .Where(s => s.An == An && s.Luna == luna && s.LotId == caz.Lot && s.GestiuneId == Magazie).ToList();
            var contabil = os.GetObjectsQuery<SoldPerioadaContabil>()
                .Where(s => s.An == An && s.Luna == luna && (s.RepartitorId == Loc || s.RepartitorId == Furnizor)).ToList()
                .GroupBy(s => (s.ContId, s.RepartitorId)).Select(g => (g.Key, Net: g.Sum(s => s.Debit - s.Credit)));
            var fapte = os.GetObjectsQuery<C.Postare>().Where(p => p.Carte == N.Carte.Contabil && p.Data <= sfarsit
                    && (p.Repartitor == Loc || p.Repartitor == Furnizor)).ToList()
                .GroupBy(p => (ContId: p.Cont, RepartitorId: p.Repartitor))
                .Select(g => (g.Key, Net: g.Sum(p => p.Sens == N.Sens.Debit ? p.Valoare : -p.Valoare)));
            static string Text(IEnumerable<((Guid, Guid?) Cheie, decimal Net)> randuri) =>
                string.Join("; ", randuri.Where(r => r.Net != 0m).OrderBy(r => r.Cheie).Select(r => $"{r.Cheie}={r.Net:0.00}"));
            return (stoc.Sum(s => s.Cantitate), stoc.Sum(s => s.Valoare), Text(contabil), Text(fapte));
        });
        Console.WriteLine($"     MĂSURAT ({id}): snapshot contabil {luna:00} [{snapshot}]; fapte [{cub}].");
        Verifica(id, $"{pas}: snapshot de stoc {luna:00} {q}/{v} (obținut {cantitate}/{valoare})", cantitate == q && valoare == v);
        Verifica(id, $"{pas}: snapshot-ul contabil {luna:00} coincide cu suma faptelor pe consum și pe furnizor",
            snapshot.Length > 0 && snapshot == cub);
        Este(id, $"{pas}: lectura la {sfarsit}", caz, q, v, c, d, sfarsit);
    }

    void CorectieInainteaInchiderii(Caz caz) {
        const string id = "SC-CVL-23/corecție → închidere";
        var a = Draft(Zi(15), (caz, 200m));
        var r = Serial(id, () => Opereaza(a), () => Inchide(An, 1));
        Verifica(id, "ambele comenzi trec", r == (Trecut, Trecut));
        SnapshotEste(id, "luna închisă poartă corecția", caz, 1, 40m, 480m, 720m, 1200m);
    }

    void InchidereaInainteaCorectiei(Caz caz) {
        const string id = "SC-CVL-23/închidere → corecție";
        var a = Draft(Zi(20, 2), (caz, 200m));
        var r = Serial(id, () => Inchide(An, 2), () => Opereaza(a));
        Verifica(id, "închiderea trece, corecția e refuzată de perioada închisă", r.Prima == Trecut && Refuzat(r.ADoua, "închis"));
        FaraEfecte(id, a);
        SnapshotEste(id, "luna închisă fără corecție", caz, 2, 40m, 400m, 600m, 1000m);
    }
}
