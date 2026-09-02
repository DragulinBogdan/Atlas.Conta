using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Bcs;

// Felia BCS: reconcilierea agregatului (scriere) + proiecțiile plate (citire).
// ZERO ASP.NET aici — controllerul din host e transport, iar ModelCheck
// exersează exact același cod pe `EFCoreObjectSpaceProvider` standalone
// (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe BCS în plus/în minus față de BTR ═══
// În PLUS: `MaterializeazaValori` — valoarea consumului se arată pe ecran la
// culegere, nu abia după operare (F6-D6, precedentul F5-D6). În MINUS: fără
// `NumarPV`/`DataPV` (BCS nu e `IDocumentCuPV`) și fără seam de culegere a
// loturilor — liniile de BCS nu declară `ILinieCareNasteLot`, deci nu nasc
// nimic; ele DESCARCĂ loturi existente.
public static class BonConsumApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, BcsWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        BonConsum doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<BonConsum>(os, existentId, "Bonul de consum");
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<BonConsum>();
        }

        // `Numar` NU se atinge (F6-D4): seria „BCS-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (GATE XAF D6) — gardianul de
        // Committing o și păzește pe tipurile cu politică de numerotare.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR/NIR): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Gestiune → loc de consum) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (locul de consum)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<BcsLinieWriteDto>());
        MaterializeazaValori(os, doc);

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa).
    //
    // FĂRĂ `LoturiCulegereService.CurataOrfane` (spre deosebire de NIR): liniile
    // de BCS nu declară `ILinieCareNasteLot`, deci nu nasc niciodată loturi —
    // lotul unei linii de consum e al altcuiva (l-a născut o recepție), iar
    // curățenia nu are ce să caute. Un apel ar fi inofensiv, dar mincinos.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<BonConsum>(os, id, "Bonul de consum");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, BonConsum doc, List<BcsLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            DocumentDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out detaliu))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
            }
            else {
                detaliu = os.CreateObject<DocumentDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");
            if (l.LotId is Guid lotId) {
                detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
            }
            else {
                detaliu.Lot = null;
                detaliu.LotId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca NIR/FCT).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            detaliu.Cantitate = l.Cantitate;
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // Valoarea liniei, materializată LA CULEGERE (GATE 53c: operatorul vede cât
    // costă consumul înainte să-l opereze) — de aceea `Valoare` nu e în WriteDto.
    //
    // Formula e GEAMĂNA lui `BonConsum.PregatesteOperare` (F6-D6), care o rescrie
    // la operare: preț lot × cantitate — prețul consumului NU se culege niciodată,
    // e al lotului descărcat (decizia 27d).
    //
    // Singura diferență față de hook: linia rămasă FĂRĂ lot se golește la 0.
    // Hook-ul iterează doar `Detalii.Where(d => d.LotId != null)` fiindcă la
    // operare o linie fără lot e oricum refuzată; aici draftul e legitim
    // incomplet, iar valoarea veche (a lotului scos de pe linie) ar minți pe ecran.
    static void MaterializeazaValori(IObjectSpace os, BonConsum doc) {
        foreach (var d in doc.Detalii) {
            // Liniile marcate spre ștergere în acest commit nu se mai ating.
            if (os.IsObjectToDelete(d))
                continue;
            var lot = d.LotId is Guid lotId ? os.GetObjectByKey<Lot>(lotId) : null;
            d.Valoare = lot != null ? Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar) : 0m;
        }
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        Rezolva.Cere<Repartitor>(os, id, rol);

    // Gardul de scară: `numeric(18, s)` ⇒ cel mult `s` zecimale și `18 − s` cifre
    // întregi. Aceeași formă pentru toate cele trei scări ale modelului (49e).
    static void VerificaScara(decimal valoare, int scara, string rol) {
        if (decimal.Round(valoare, scara) != valoare)
            throw new OperareException($"{rol} acceptă cel mult {scara} zecimale.");
        var limita = 1m;
        for (var i = 0; i < Scara.Precizie - scara; i++)
            limita *= 10m;
        if (Math.Abs(valoare) >= limita)
            throw new OperareException(
                $"{rol} depășește intervalul suportat ({Scara.Precizie - scara} cifre întregi).");
    }

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).
    // Direct pe `DocumentDetaliu`, fără `as`-cast: BCS n-are frunză (DIM-2 nu i-a
    // dat una — nu culege nicio dimensiune), deci toate liniile lui sunt de bază.

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e un bon de consum.
    public static BcsReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<BonConsum>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate, l.Valoare, l.ValoareTva
            })
            .ToList();

        // Affordance ONESTĂ pe stingeri (F3-D2): BCS nu e creanță și n-ar trebui
        // să poarte imperecheri, dar gardianul motorului
        // (`VerificaFaraImperecheri`) e generic pe `Document` — dacă totuși există
        // un link (import, compensare pe notă), refuzul se ARATĂ, nu se descoperă
        // la apăsarea butonului. Un `Any` mărginit per citire.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new BcsReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new BcsLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, Valoare = l.Valoare
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c).
    public static IQueryable<BcsListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<BonConsum>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new BcsListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // Enum → string ÎN SQL (`CASE`): filtrarea și sortarea rămân
                   // server-side, deși pe sârmă starea e text.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
