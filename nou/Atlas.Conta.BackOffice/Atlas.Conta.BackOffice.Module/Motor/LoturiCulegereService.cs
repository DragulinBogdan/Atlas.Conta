using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// F2-D1: mecanismul de CULEGERE al loturilor de pe factura de intrare, extras din
// `FacturaIntrareLoturiController` ca serviciu pe `IObjectSpace` pur — o singură
// logică de naștere/sincronizare/curățenie, apelată din (a) controllerul XAF
// (adaptor subțire pe `ObjectSpace.Committing`) și (b) `FacturaIntrareApply`
// înainte de commit, unde nu rulează niciun ViewController. Motorul doar
// FINALIZEAZĂ lotul la operare (26e) — nașterea lui e a culegerii.
//
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
public static class LoturiCulegereService {
    public static void Sincronizeaza(IObjectSpace os, FacturaIntrare doc) {
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
            // operatorului ⇒ lotul PROPRIU al liniei dispare — DAR numai dacă e
            // un lot pe care culegerea l-a născut și motorul nu l-a atins încă.
            //
            // Review advers D1: `ProdusId` e coloană NOUĂ (migrația
            // FacturaIntrareProdus), deci pe TOATE liniile preexistente (34.289
            // în baza de import) e null deși linia are lot FINALIZAT. Fără
            // distincția de mai jos, orice commit pe un astfel de draft — inclusiv
            // cel pe care `Opereaza` îl face necondiționat, sau tranziția
            // Operat→Draft a anulării — ștergea lotul istoric (ID, dată reală,
            // preț, poziție FIFO) fără nicio eroare, iar documentul rămânea
            // neoperabil.
            if (linie.ProdusId == null && lot != null && Finalizat(lot)) {
                // Self-healing: lotul finalizat E sursa de adevăr; linia își ia
                // produsul de la el (culegerea veche nu-l avea).
                linie.ProdusId = lot.ProdusId;
                continue;
            }
            if (linie.ProdusId == null || natura != NaturaClasa.Stoc) {
                if (lot != null && !Finalizat(lot))
                    LoturiLiniiSterse.StergeLot(os, linie, lot);
                else if (lot != null && linie.LotId == lot.ID) {
                    // Review advers F2-D3: Tipul mutat pe NE-stoc pe un draft
                    // re-deschis (post-anulare) — lotul FINALIZAT nu se șterge
                    // (are istorie), dar linia devenită de serviciu nu are voie
                    // să-l mai refere: dimensiunea Material s-ar rezolva din
                    // lotul irelevant (33b), iar lotul ar rămâne legat de o
                    // linie ne-stoc. Referința se rupe, lotul rămâne.
                    linie.Lot = null;
                    linie.LotId = null;
                }
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

    // Curățenia loturilor rămase fără linie-mamă, pe calea în care documentul
    // dispare fără ca liniile lui să fie încărcate (ListView / `Sterge` prin API).
    public static void CurataOrfane(IObjectSpace os) {
        // Review advers D5: în ListView colecția `Detalii` NU e încărcată, iar
        // EF Core NU marchează dependenții `Deleted` la `Remove` (cascada se face
        // în Postgres, la SaveChanges) — deci `GetObjectsToDelete` întoarce listă
        // goală și controllerul era no-op exact pe calea pentru care a fost scris.
        // Liniile se citesc explicit după documentele marcate spre ștergere.
        var idsDocumente = os.GetObjectsToDelete(true).OfType<Document>().Select(d => d.ID).ToList();
        var idsSterse = LoturiLiniiSterse.Ids(os);
        if (idsDocumente.Count > 0)
            idsSterse.AddRange(os.GetObjectsQuery<FacturaIntrareDetaliu>()
                .Where(d => idsDocumente.Contains(d.DocumentId))
                .Select(d => d.ID)
                .ToList());
        idsSterse = idsSterse.Distinct().ToList();
        if (idsSterse.Count == 0)
            return;
        var loturi = os.GetObjectsQuery<Lot>()
            .Where(l => l.LinieIntrareId != null && idsSterse.Contains(l.LinieIntrareId.Value))
            .ToList();
        LoturiLiniiSterse.Curata(os, idsSterse, loturi);
    }

    // Lotul FINALIZAT a trecut prin motor (26e: `PretUnitar = Valoare/Cantitate`,
    // `Data` = data documentului), deci e stare contabilă — un seam de UI nu are
    // voie să-l șteargă. Lotul născut la culegere și încă neoperat e recunoscibil
    // exact prin lipsa acestor două (vezi și `Lot.Eticheta`: „(în culegere)").
    static bool Finalizat(Lot lot) => lot.Data != default || lot.PretUnitar != 0;

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
        // Lotul FINALIZAT de motor (review GATE D1) nu moare cu draftul — DAR
        // (review advers F2-D4) dacă a rămas FĂRĂ NICIO URMĂ (anularea i-a șters
        // rândurile de registru, nicio linie vie nu-l mai referă), păstrarea lui
        // ar fi zgomot ireversibil în nomenclator, exact ce curățenia există să
        // prevină. Query-urile văd doar rândurile vii (filtrul deferred deletion);
        // liniile în curs de ștergere din ACEST commit se exclud explicit.
        var spreStergere = new HashSet<Guid>(idsSterse);
        foreach (var linie in os.GetObjectsToDelete(true).OfType<DocumentDetaliu>())
            spreStergere.Add(linie.ID);
        foreach (var id in idsSterse)
            foreach (var lot in candidati.Where(l => l.LinieIntrareId == id).ToList()) {
                if (os.IsObjectToDelete(lot))
                    continue;
                if (lot.Data == default && lot.PretUnitar == 0) {
                    os.Delete(lot);
                    continue;
                }
                if (os.GetObjectsQuery<RegistruStoc>().Any(r => r.LotId == lot.ID))
                    continue;
                var referinte = os.GetObjectsQuery<DocumentDetaliu>()
                    .Where(d => d.LotId == lot.ID)
                    .Select(d => d.ID)
                    .ToList();
                if (referinte.All(spreStergere.Contains))
                    os.Delete(lot);
            }
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
