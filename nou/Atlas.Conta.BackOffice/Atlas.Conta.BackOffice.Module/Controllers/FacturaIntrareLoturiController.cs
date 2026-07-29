using System.ComponentModel;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// GATE XAF (D2, GOL 1 din contract): nașterea LOTULUI la culegere pe factura de
// intrare. `CreeazaLot` (decizia 25c/26e) exista în model cu ZERO apelanți din UI,
// deci validarea „liniile de stoc își creează lotul la culegere (alegeți produsul)"
// era neîndeplinibilă din ecran, iar ocolirea manuală (Lot ales din Nomenclatoare)
// producea divergență tăcută TVA↔cost — două prețuri independente pe același lot.
//
// De ce `ObjectSpace.Committing` și nu `ObjectChanged` la alegerea produsului:
// lotul are nevoie de identitatea DEFINITIVĂ a liniei (`Lot.LinieIntrareId = linia.ID`)
// și de latura primitoare a header-ului; ambele se pot schimba în timpul culegerii,
// iar commit-ul e singurul moment în care starea e coerentă. Verificat pe surse
// (DevExpress 26.1.3): `BaseObjectSpace.CommitChanges` ridică `Committing` ÎNAINTE
// de `DoCommit`, iar `EFCoreObjectSpace.DoCommit` citește `GetModifiedObjects()`
// din ChangeTracker-ul VIU și abia apoi apelează `DbContext.SaveChanges()` — deci
// obiectele create/șterse din acest handler intră în ACELAȘI commit (nu există
// gardian `isCommitting`: `CreateObject`/`Delete` fac direct DbContext.Add/Remove).
//
// Prețul și data lotului NU se ating aici: le finalizează motorul la operare
// (26e — PretUnitar = Valoare/Cantitate, Data = data documentului). Un lot de pe
// un draft e deliberat NEFINALIZAT în bază (preț 0, dată 0001-01-01): nu are rânduri
// de stoc, deci nu intră în picking și nu poate fi consumat până la operare.
public class FacturaIntrareLoturiController : ObjectViewController<DetailView, FacturaIntrare> {
    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.Committing += OnCommitting;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committing -= OnCommitting;
        base.OnDeactivated();
    }

    void OnCommitting(object sender, CancelEventArgs e) {
        var os = ObjectSpace;
        var doc = ViewCurrentObject;

        // Liniile ȘTERSE în acest commit — inclusiv cele cascadate de ștergerea
        // documentului Draft întreg (EF marchează Deleted dependenții încărcați la
        // `Remove`, iar colecția Detalii e încărcată în DetailView).
        var idsSterse = LoturiLiniiSterse.Ids(os);

        // Liniile VII se ating DOAR cât documentul e Draft: după operare loturile
        // sunt finalizate de motor (preț/dată), iar liniile sunt read-only prin
        // gardieni. Pe calea „Operează" handler-ul rulează de două ori — o dată pe
        // commit-ul culegerii (Draft → creează/sincronizează) și o dată pe commit-ul
        // motorului (Stare deja Operat → nu face nimic).
        var vii = doc != null && doc.Stare == StareDocument.Draft && !os.IsObjectToDelete(doc)
            ? doc.Detalii.OfType<FacturaIntrareDetaliu>().Where(d => !os.IsObjectToDelete(d)).ToList()
            : new List<FacturaIntrareDetaliu>();

        if (idsSterse.Count == 0 && vii.Count == 0)
            return;

        var loturiProprii = LoturiProprii(os, vii.Select(d => d.ID).Concat(idsSterse).ToList());

        // Natura Clasei per Tip prin PROIECȚIE (disciplina 25b — pattern-ul exact
        // din FacturaIntrare.ValideazaOperare): nicio navigație lazy în enumerare.
        var idsTip = vii.Select(d => d.TipMaterialId).Distinct().ToList();
        var naturi = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.Clasa.Natura })
            .ToDictionary(t => t.ID, t => t.Natura);

        // Gestiunea lotului = latura PRIMITOARE a facturii (FCT: Partener → Gestiune).
        var gestiune = doc != null && doc.PrimitorId != Guid.Empty
            ? os.GetObjectByKey<Repartitor>(doc.PrimitorId) as Gestiune
            : null;

        foreach (var linie in vii) {
            var lot = loturiProprii.FirstOrDefault(l => l.LinieIntrareId == linie.ID && !os.IsObjectToDelete(l));
            // Tip necules / necunoscut = culegere INCOMPLETĂ, nu decizie: lotul
            // existent se lasă în pace (regula de Save cere oricum Tipul, iar un
            // commit refuzat de validare nu are voie să distrugă lotul liniei).
            if (!naturi.TryGetValue(linie.TipMaterialId, out var natura))
                continue;

            // Produsul golit sau Tipul mutat pe ne-stoc = decizie explicită a
            // operatorului ⇒ lotul PROPRIU al liniei dispare.
            if (linie.ProdusId == null || natura != NaturaClasa.Stoc) {
                if (lot != null)
                    LoturiLiniiSterse.StergeLot(os, linie, lot);
                continue;
            }

            if (lot == null) {
                // Primitor lipsă sau ne-Gestiune ⇒ skip GRAȚIOS (contract D2): lotul
                // se naște la commit-ul următor, iar refuzul de la operare rămâne
                // plasa. Nu blocăm culegerea pentru o latură încă necompletată
                // (iar `Lot.GestiuneId` e NOT NULL — un lot fără gestiune n-ar
                // trece nici inserția).
                if (gestiune == null)
                    continue;
                var produs = os.GetObjectByKey<Produs>(linie.ProdusId.Value);
                if (produs == null)
                    continue;
                loturiProprii.Add(linie.CreeazaLot(os, produs, gestiune));
                continue;
            }

            // Sincronizare pe lotul propriu: produsul reales pe linie și gestiunea
            // schimbată pe header se propagă; preț/dată rămân motorului (26e).
            if (lot.ProdusId != linie.ProdusId.Value) {
                var produs = os.GetObjectByKey<Produs>(linie.ProdusId.Value);
                if (produs != null)
                    lot.Produs = produs;
            }
            if (gestiune != null && lot.GestiuneId != gestiune.ID)
                lot.Gestiune = gestiune;
            // Linia trebuie să refere lotul PROPRIU (Lot devine read-only în UI la
            // pasul 3, dar un draft vechi poate purta încă altă referință).
            if (linie.LotId != lot.ID)
                linie.Lot = lot;
        }

        LoturiLiniiSterse.Curata(os, idsSterse, loturiProprii);
    }

    static List<Lot> LoturiProprii(IObjectSpace os, List<Guid> idsLinii) {
        if (idsLinii.Count == 0)
            return new List<Lot>();
        // Loturile deja PERSISTATE ale acestor linii.
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.LinieIntrareId != null && idsLinii.Contains(l.LinieIntrareId.Value))
            .ToList();
        // …plus cele create în ObjectSpace-ul VIU și încă nesalvate: un commit
        // refuzat de validare (contextul Save rulează tot în Committing) le lasă
        // Added, iar query-ul de mai sus lovește baza și nu le vede — fără asta,
        // commit-ul următor ar naște un al doilea lot pentru aceeași linie.
        foreach (var lot in os.ModifiedObjects.OfType<Lot>())
            if (lot.LinieIntrareId != null && idsLinii.Contains(lot.LinieIntrareId.Value) && !loturi.Contains(lot))
                loturi.Add(lot);
        return loturi;
    }
}

// Curățenia loturilor rămase fără linie-mamă, partajată de cele două ferestre din
// care o linie de FCT poate dispărea. FK-ul invers linie←lot nu există în schemă
// (26e — ar face ciclu de inserție), deci întreținerea proveniența e integral a
// noastră: un lot al cărui `LinieIntrareId` nu mai are linie e orfan (preț 0, fără
// rânduri de stoc) și ar polua nomenclatorul de loturi și lookup-urile.
static class LoturiLiniiSterse {
    public static List<Guid> Ids(IObjectSpace os) =>
        os.GetObjectsToDelete(true).OfType<FacturaIntrareDetaliu>().Select(d => d.ID).ToList();

    public static void Curata(IObjectSpace os, List<Guid> idsSterse, List<Lot> candidati) {
        // NUMAI loturile PROPRII ale liniilor șterse (`LinieIntrareId`), niciodată
        // un lot străin (un lot pinuit pe linie prin LotId rămâne al altcuiva).
        foreach (var id in idsSterse)
            foreach (var lot in candidati.Where(l => l.LinieIntrareId == id).ToList())
                if (!os.IsObjectToDelete(lot))
                    os.Delete(lot);
    }

    public static void StergeLot(IObjectSpace os, DocumentDetaliu linie, Lot lot) {
        // Linia REFERĂ lotul prin FK real (`DocumentDetaliu.LotId`) — referința se
        // rupe înaintea ștergerii, altfel SaveChanges pică pe constrângere.
        // Navigația întâi, apoi FK-ul: fixup-ul EF nu are voie să reînvie referința.
        if (linie.LotId == lot.ID) {
            linie.Lot = null;
            linie.LotId = null;
        }
        os.Delete(lot);
    }
}

// Geamănul de pe LISTĂ (același tipar ca `DocumentDetaliiEditareController` +
// `DocumentDetaliuDetailEditareController`): ștergerea unui document se face de
// obicei din ListView, unde controllerul de mai sus nici nu e activ, iar liniile
// cascadate ar lăsa loturile orfane. Aici NU se creează și nu se sincronizează
// nimic (într-o listă nu se culege nici produs, nici latură) — doar curățenia.
// Țintă `Document`, nu `FacturaIntrare`: acoperă și un eventual ListView pe baza
// ierarhiei; filtrul pe `FacturaIntrareDetaliu` din `Ids` face selecția.
public class DocumenteLoturiCuratenieController : ObjectViewController<ListView, Document> {
    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.Committing += OnCommitting;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committing -= OnCommitting;
        base.OnDeactivated();
    }

    void OnCommitting(object sender, CancelEventArgs e) {
        var os = ObjectSpace;
        var idsSterse = LoturiLiniiSterse.Ids(os);
        if (idsSterse.Count == 0)
            return;
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.LinieIntrareId != null && idsSterse.Contains(l.LinieIntrareId.Value))
            .ToList();
        LoturiLiniiSterse.Curata(os, idsSterse, loturi);
    }
}
