using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Ldi;

// Felia LDI: reconcilierea agregatului (scriere) + proiecțiile plate (citire).
// ZERO ASP.NET aici — controllerul din host e transport, iar ModelCheck
// exersează exact același cod pe `EFCoreObjectSpaceProvider` standalone
// (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// Culegerea (golirea câmpurilor celeilalte direcții, loturi, valori) e a
// `CulegereDocument` (104c). Minusul descarcă lotul pinuit; plusul își naște
// lotul din `ProdusId`, iar `LotId` din payload se ignoră (F6-D5).
public static class ListaDiferenteInventarApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, LdiWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        ListaDiferenteInventar doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<ListaDiferenteInventar>(os, existentId, "Lista de diferențe");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<ListaDiferenteInventar>();
        }

        // `Numar` NU se atinge (F6-D4): seria „LDI-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (GATE XAF D6).
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Gestiune → comisie) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea inventariată)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (comisia de inventariere)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<LdiLinieWriteDto>());
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
        return doc.ID;
    }

    // Pre-check de DOMENIU pe Draft (gardianul de Committing rămâne plasa).
    // FĂRĂ refuzul pe `Autogenerat` din NIR: LDI nu e niciodată artefactul unei
    // operări (nu e țintă de `PoliticaConex` și niciun tip nu-l produce ca
    // secundar), deci n-ar avea ce apăra.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<ListaDiferenteInventar>(os, id, "Lista de diferențe");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, ListaDiferenteInventar doc, List<LdiLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunzele LDI: un LDI
        // istoric/importat poate purta linii de tip BAZĂ (importul le-a scris ca
        // atare), iar payload-ul e adevărul agregatului — reconcilierea trebuie
        // să le vadă, ca să le poată șterge.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            // Review advers F6-M4: pe liniile EXISTENTE, tipul se judecă ÎNAINTEA
            // parse-ului de direcție — o linie de tip BAZĂ (LDI istoric) iese din
            // ReadDto cu `Directie` null, iar parse-ul ar refuza-o cu mesajul de
            // enum („direcția «» nu există") în locul celui acționabil de mai jos.
            ListaDiferenteInventarDetaliu detaliu;
            CulegereDocument.Amprenta? inainte = null;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as ListaDiferenteInventarDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de listă de diferențe (tip vechi) — ștergeți-o din "
                        + "document și culegeți-o din nou.");
                inainte = CulegereDocument.Urmareste(os, doc, detaliu);
                detaliu.Directie = ApiEnum.Directie(l.Directie);
            }
            else {
                // Parse-ul enumerării ÎNAINTE de `CreateObject` (F6-D5, precedentul
                // `ApiEnum`): direcția decide tot ce urmează, iar un refuz după
                // creare ar lăsa o linie orfană în ObjectSpace-ul viu.
                var directie = ApiEnum.Directie(l.Directie);
                detaliu = os.CreateObject<ListaDiferenteInventarDetaliu>();
                detaliu.Document = doc;
                detaliu.Directie = directie;
            }
            ApiLinie.TipMaterial(os, detaliu, l.TipMaterialId);
            detaliu.Cantitate = l.Cantitate;

            if (l.ProdusId is Guid produsId) {
                detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }
            detaliu.PretEvaluare = l.PretEvaluare;
            detaliu.DataExpirare = l.DataExpirare;
            detaliu.LotFabricatie = l.LotFabricatie;

            // Pe plus lotul e server-owned; `LotId` din payload e doar ecoul citirii (F6-D5).
            if (detaliu.Directie == DirectieDiferenta.Minus) {
                if (l.LotId is Guid lotId) {
                    detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
                }
                else {
                    detaliu.Lot = null;
                    detaliu.LotId = null;
                }
            }

            // Dimensiunea frunzei (DIM-2) + angajamentul de pe bază — pe NAVIGAȚIE,
            // ca restul FK-urilor: existența se validează cu mesaj de domeniu, nu
            // cu violare de FK.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;
            detaliu.Angajament = Nomenclator<Angajament>(os, l.AngajamentId, "Angajamentul");
            if (l.AngajamentId == null) detaliu.AngajamentId = null;

            CulegereDocument.Mapata(os, doc, detaliu, inainte, null);
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol)
            where T : class => Rezolva.Optional<T>(os, id, rol);

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        Rezolva.Cere<Repartitor>(os, id, rol);

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o listă de diferențe.
    public static LdiReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<ListaDiferenteInventar>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Pe BAZA detaliului: liniile de tip bază (import, istoric) apar în `Linii`, cu valorile frunzei null.
        // `as` nu filtrează pe tip; sigur fiindcă liniile unui document sunt frunza lui sau baza (F28-H, 89).
        // Valorile frunzei sunt nullable explicit: pe o linie de bază vin null, iar un tip valoare ar pica la materializare.
        // Numele membrului se compune în memorie, după materializare.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID,
                Directie = (DirectieDiferenta?)(l as ListaDiferenteInventarDetaliu).Directie,
                l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                ProdusId = (l as ListaDiferenteInventarDetaliu).ProdusId,
                ProdusCod = (l as ListaDiferenteInventarDetaliu).Produs.Cod,
                ProdusDenumire = (l as ListaDiferenteInventarDetaliu).Produs.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate,
                PretEvaluare = (l as ListaDiferenteInventarDetaliu).PretEvaluare,
                l.Valoare,
                DataExpirare = (l as ListaDiferenteInventarDetaliu).DataExpirare,
                LotFabricatie = (l as ListaDiferenteInventarDetaliu).LotFabricatie,
                CodEconomicId = (l as ListaDiferenteInventarDetaliu).CodEconomicId,
                CodEconomicCod = (l as ListaDiferenteInventarDetaliu).CodEconomic.Cod,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca să
        // dea EXACT ce dă `Lista` chiar dacă documentul poartă linii de tip bază.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        var regim = RegimDocument.Calculeaza(os, id);

        return new LdiReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            DataInregistrare = h.DataInregistrare,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            PoateEdita = regim.Editabil,
            PoateOpera = regim.Poate(ComandaDocument.Opereaza),
            Corectie = ApiProiectii.Corectie(os, id),
            PoateAnula = regim.Poate(ComandaDocument.AnuleazaOperarea),
            PoateStorna = regim.Poate(ComandaDocument.Storneaza),
            Linii = linii.Select(l => new LdiLinieReadDto {
                Id = l.ID,
                Directie = l.Directie?.ToString(),
                TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, PretEvaluare = l.PretEvaluare, Valoare = l.Valoare,
                DataExpirare = l.DataExpirare, LotFabricatie = l.LotFabricatie,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                AngajamentId = l.AngajamentId, AngajamentCod = l.AngajamentCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<LdiListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<ListaDiferenteInventar>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new LdiListDto {
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
