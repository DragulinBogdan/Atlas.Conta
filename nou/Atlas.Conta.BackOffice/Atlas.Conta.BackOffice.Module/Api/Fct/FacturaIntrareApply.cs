using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Fct;

// Nucleul feliei FCT: reconcilierea agregatului (scriere) și proiecțiile plate
// (citire). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
// Culegerea (precompletări, valori, loturi) e a `CulegereDocument` (104c).
public static class FacturaIntrareApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, FacturaIntrareWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");
        // Parse-ul enum-ului ÎNAINTE de a atinge ObjectSpace-ul (F3-D5): pe calea
        // de CREARE un refuz de după `CreateObject` ar lăsa o factură orfană în
        // OS-ul viu al apelantului, pe care un commit ulterior ar persista-o.
        var plataTipInstrument = ApiEnum.TipInstrumentOptional(dto.PlataTipInstrument);

        FacturaIntrare doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<FacturaIntrare>(os, existentId, "Factura de intrare");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<FacturaIntrare>();
        }

        // Numărul furnizorului: CULES (FCT n-are politică de numerotare), deci
        // spre deosebire de BTR intră din payload. `ValideazaOperare` îl cere.
        doc.Numar = dto.Numar;
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR): rezolvarea validează existența
        // cu mesaj de domeniu, regulile XAF de culegere stau pe navigație, iar pe
        // o entitate urmărită navigația încărcată ar rescrie la fixup un FK setat
        // direct. TIPUL laturii (Partener → Gestiune) NU se verifică aici: e
        // invariant al OPERĂRII (`FacturaIntrare.ValideazaOperare`) — un draft
        // are voie să fie incomplet/greșit până la operare.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (furnizorul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea)");
        doc.DataScadenta = dto.DataScadenta;
        doc.DataPrimire = dto.DataPrimire;
        doc.DataExigibilitate = dto.DataExigibilitate;
        doc.NumarPV = dto.NumarPV;
        doc.DataPV = dto.DataPV;
        doc.CodCpv = dto.CodCpv;
        doc.Valuta = dto.Valuta;
        doc.Curs = dto.Curs;

        // Plata automată (F3-D5, ridicarea excluderii F2): parametrii culeși ai
        // documentului SECUNDAR (`FacturaIntrare.GenereazaSecundar` — 31e). Nu se
        // validează aici nimic în plus: `ValideazaOperare` cere contul propriu
        // dacă bifa e pusă, iar draftul are voie să fie incomplet până la operare.
        doc.GenereazaPlata = dto.GenereazaPlata;
        // Navigația, ca la laturi: existența se validează cu mesaj de domeniu.
        if (dto.PlataContPropriuId is Guid contPropriuId) {
            doc.PlataContPropriu = Rezolva.Cere<ContPropriu>(os, contPropriuId, "Contul propriu (casă/bancă)");
        }
        else {
            doc.PlataContPropriu = null;
            doc.PlataContPropriuId = null;
        }
        doc.PlataNumar = dto.PlataNumar;
        doc.PlataData = dto.PlataData;
        // NULLABLE pe model: absența din payload NU devine `OrdinPlata` aici —
        // default-ul îl aplică `GenereazaSecundar`, la generare (ApiEnum).
        doc.PlataTipInstrument = plataTipInstrument;

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<FacturaIntrareLinieWriteDto>());
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
        return doc.ID;
    }

    // Pre-check de DOMENIU pe Draft (gardianul de Committing rămâne plasa).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<FacturaIntrare>(os, id, "Factura de intrare");
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
    static void ReconciliazaLinii(IObjectSpace os, FacturaIntrare doc,
        List<FacturaIntrareLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar liniile de tip FCT:
        // un draft vechi poate purta o linie de tip BAZĂ (refuzată oricum la
        // operare — `FacturaIntrare.ValideazaOperare`), iar payload-ul e adevărul
        // agregatului, deci reconcilierea o curăță în loc s-o lase invizibilă.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            FacturaIntrareDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as FacturaIntrareDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de factură de intrare (tip vechi) — ștergeți-o "
                        + "din document și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<FacturaIntrareDetaliu>();
                detaliu.Document = doc;
            }

            var inainte = CulegereDocument.Urmareste(os, doc, detaliu);
            ApiLinie.TipMaterial(os, detaliu, l.TipMaterialId);

            // Produsul e mecanismul lotului (GATE XAF D1): îl consumă
            // `LoturiCulegereService` după reconciliere. `LotId` NU se atinge —
            // e server-owned pe FCT.
            if (l.ProdusId is Guid produsId) {
                detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }

            detaliu.Cantitate = l.Cantitate;
            detaliu.PretUnitar = l.PretUnitar;
            detaliu.CodCpv = l.CodCpv;
            detaliu.DataExpirare = l.DataExpirare;
            detaliu.LotFabricatie = l.LotFabricatie;

            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = Rezolva.Cere<TipTva>(os, tipTvaId, "Tipul de TVA");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            if (l.AngajamentId is Guid angajamentId) {
                detaliu.Angajament = Rezolva.Cere<Angajament>(os, angajamentId, "Angajamentul");
            }
            else {
                detaliu.Angajament = null;
                detaliu.AngajamentId = null;
            }

            // Dimensiunile frunzei (DIM-2) — pe NAVIGAȚIE, ca restul FK-urilor:
            // existența se validează cu mesaj de domeniu, nu cu violare de FK.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;
            detaliu.SursaFinantare = Nomenclator<SursaFinantare>(os, l.SursaFinantareId, "Sursa de finanțare");
            if (l.SursaFinantareId == null) detaliu.SursaFinantareId = null;
            detaliu.CodFunctional = Nomenclator<CodFunctional>(os, l.CodFunctionalId, "Codul funcțional");
            if (l.CodFunctionalId == null) detaliu.CodFunctionalId = null;
            detaliu.Proiect = Nomenclator<Proiect>(os, l.ProiectId, "Proiectul");
            if (l.ProiectId == null) detaliu.ProiectId = null;

            detaliu.LinieAvans = l.LinieAvansId is Guid avans
                ? Rezolva.Cere<DocumentDetaliu>(os, avans, "Linia avansului") : null;
            if (l.LinieAvansId == null) detaliu.LinieAvansId = null;
            CulegereDocument.Mapata(os, doc, detaliu, inainte, l.ValoareTva);
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
    // sau nu e o factură de intrare.
    public static FacturaIntrareReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<FacturaIntrare>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                // `as` nu filtrează pe tip: null pe alt repartitor fiindcă `CodFiscal` e doar al lui Partener (F28-J, 89).
                PredatorCodFiscal = (d.Predator as Partener).CodFiscal,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.DataPrimire, d.DataExigibilitate, d.DataScadenta, d.NumarPV, d.DataPV, d.CodCpv, d.Valuta, d.Curs,
                // Parametrii plății automate (F3-D5).
                d.GenereazaPlata, d.PlataContPropriuId,
                PlataContPropriuDenumire = d.PlataContPropriu.Denumire,
                d.PlataNumar, d.PlataData, d.PlataTipInstrument,
                d.Autogenerat, d.DocumentSursaId
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Liniile se citesc pe tipul DERIVAT: identitatea liniei de FCT e frunza
        // (PretUnitar/Produs/dimensiuni), iar o linie de bază pe un draft vechi e
        // oricum refuzată la operare — o vede prima reconciliere, care o curăță.
        var linii = os.GetObjectsQuery<FacturaIntrareDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.ProdusId, ProdusCod = l.Produs.Cod, ProdusDenumire = l.Produs.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate, l.PretUnitar, l.Valoare, l.ValoareTva, l.LinieAvansId, l.TvaCules,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod, TipTvaDenumire = l.TipTva.Denumire,
                TipTvaCota = (decimal?)l.TipTva.Cota,
                l.DataExpirare, l.LotFabricatie, l.CodCpv,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod,
                l.CodEconomicId, CodEconomicCod = l.CodEconomic.Cod,
                l.SursaFinantareId, SursaFinantareCod = l.SursaFinantare.Cod,
                l.CodFunctionalId, CodFunctionalCod = l.CodFunctional.Cod,
                l.ProiectId, ProiectCod = l.Proiect.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului, nu pe liniile proiectate mai sus:
        // e definiția modelului (`Document.Total`) și trebuie să dea EXACT ce dă
        // `Lista` — un draft vechi cu o linie de tip bază ar fi făcut cele două
        // cifre să difere tăcut, iar operatorul le vede una lângă alta.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        var copii = ApiProiectii.Copii(os, id);
        var regim = RegimDocument.Calculeaza(os, id);

        return new FacturaIntrareReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            DataInregistrare = h.DataInregistrare,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PredatorCodFiscal = h.PredatorCodFiscal,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            DataPrimire = h.DataPrimire ?? h.DataInregistrare, DataExigibilitate = h.DataExigibilitate,
            DataScadenta = h.DataScadenta, NumarPV = h.NumarPV, DataPV = h.DataPV,
            CodCpv = h.CodCpv, Valuta = h.Valuta, Curs = h.Curs,
            GenereazaPlata = h.GenereazaPlata,
            PlataContPropriuId = h.PlataContPropriuId,
            PlataContPropriuDenumire = h.PlataContPropriuDenumire,
            PlataNumar = h.PlataNumar, PlataData = h.PlataData,
            // Enum → STRING pe sârmă (convenția `Stare`); null rămâne null.
            PlataTipInstrument = h.PlataTipInstrument?.ToString(),
            Total = total,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            PoateEdita = regim.Editabil,
            PoateOpera = regim.Poate(ComandaDocument.Opereaza),
            Corectie = ApiProiectii.Corectie(os, id),
            PoateAnula = regim.Poate(ComandaDocument.AnuleazaOperarea),
            PoateStorna = regim.Poate(ComandaDocument.Storneaza),
            Copii = copii,
            Linii = linii.Select(l => new FacturaIntrareLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, PretUnitar = l.PretUnitar,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                LinieAvansId = l.LinieAvansId, TvaCules = l.TvaCules,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                TipTvaDenumire = l.TipTvaDenumire, TipTvaCota = l.TipTvaCota,
                DataExpirare = l.DataExpirare, LotFabricatie = l.LotFabricatie, CodCpv = l.CodCpv,
                AngajamentId = l.AngajamentId, AngajamentCod = l.AngajamentCod,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                SursaFinantareId = l.SursaFinantareId, SursaFinantareCod = l.SursaFinantareCod,
                CodFunctionalId = l.CodFunctionalId, CodFunctionalCod = l.CodFunctionalCod,
                ProiectId = l.ProiectId, ProiectCod = l.ProiectCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului și abia apoi materializează (43c).
    //
    // `Total` prin JOIN PE AGREGAT, nu subquery corelat (42c), și BRUT
    // (Σ Valoare + ValoareTva) — ca `Document.Total`. Agregatul se face pe BAZA
    // detaliului: o linie de tip vechi contribuie la total, ca în model.
    public static IQueryable<FacturaIntrareListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<FacturaIntrare>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new FacturaIntrareListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // Enum → string ÎN SQL (`CASE`): starea e string pe sârmă, dar
                   // filtrarea și sortarea rămân server-side.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   DataScadenta = d.DataScadenta,
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
