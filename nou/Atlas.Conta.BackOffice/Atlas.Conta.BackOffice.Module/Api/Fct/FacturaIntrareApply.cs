using Atlas.Conta.BackOffice.Module.BusinessObjects;
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
//
// ═══ Ce face Apply pe FCT în plus față de BTR (F2-D1) ═══
// Pe tierul API nu rulează NICIUN ViewController: nici loturile, nici default-ul
// de TipTva, nici recalculul de valori. Cele trei seam-uri de culegere se apelează
// EXPLICIT, în ordinea din UI:
//   1. maparea câmpurilor culese;
//   2. `TvaService.AplicaTipTvaImplicit` — doar pe liniile NOI fără TipTva în
//      payload (culegerea explicită, inclusiv golirea deliberată, bate default-ul);
//   3. `TvaService.CalculeazaLaCulegere` — scrie `Valoare`/`ValoareTva` din
//      `PretUnitar × Cantitate` (GATE 53c: „server-owned" înseamnă că DTO-ul nu le
//      dă, nu că rămân 0), apoi override-ul manual de `ValoareTva`, dacă vine;
//   4. `LoturiCulegereService.Sincronizeaza` — nașterea/sincronizarea/curățenia
//      loturilor liniilor de stoc, ÎNAINTE de commit (echivalentul lui
//      `FacturaIntrareLoturiController.OnCommitting`).
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
            doc = os.GetObjectByKey<FacturaIntrare>(existentId)
                ?? throw new OperareException($"Factura de intrare {existentId} nu există.");
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
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR): rezolvarea validează existența
        // cu mesaj de domeniu, regulile XAF de culegere stau pe navigație, iar pe
        // o entitate urmărită navigația încărcată ar rescrie la fixup un FK setat
        // direct. TIPUL laturii (Partener → Gestiune) NU se verifică aici: e
        // invariant al OPERĂRII (`FacturaIntrare.ValideazaOperare`) — un draft
        // are voie să fie incomplet/greșit până la operare.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (furnizorul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea)");
        doc.DataScadenta = dto.DataScadenta;
        doc.NumarPV = dto.NumarPV;
        doc.DataPV = dto.DataPV;
        doc.CodCpv = dto.CodCpv;
        doc.Valuta = dto.Valuta;
        if (dto.Curs != null)
            VerificaScara(dto.Curs.Value, Scara.Pret, "Cursul valutar");
        doc.Curs = dto.Curs;

        // Plata automată (F3-D5, ridicarea excluderii F2): parametrii culeși ai
        // documentului SECUNDAR (`FacturaIntrare.GenereazaSecundar` — 31e). Nu se
        // validează aici nimic în plus: `ValideazaOperare` cere contul propriu
        // dacă bifa e pusă, iar draftul are voie să fie incomplet până la operare.
        doc.GenereazaPlata = dto.GenereazaPlata;
        // Navigația, ca la laturi: existența se validează cu mesaj de domeniu.
        if (dto.PlataContPropriuId is Guid contPropriuId) {
            doc.PlataContPropriu = os.GetObjectByKey<ContPropriu>(contPropriuId)
                ?? throw new OperareException(
                    $"Contul propriu (casă/bancă) {contPropriuId} nu există în nomenclator.");
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

        // Seam-ul de culegere al loturilor (F2-D1) — o singură logică, aceeași ca
        // în UI; pe calea asta e apelat explicit, fiindcă nu există ViewController.
        LoturiCulegereService.Sincronizeaza(os, doc);

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), apoi curățenia loturilor NĂSCUTE LA CULEGERE ale
    // liniilor care dispar — echivalentul `DocumenteLoturiCuratenieController`.
    // Ordinea contează: `CurataOrfane` citește `GetObjectsToDelete`, deci trebuie
    // să vadă ștergerile DEJA marcate, dar înaintea commit-ului (loturile intră
    // în același SaveChanges). Loturile FINALIZATE de motor nu se ating niciodată.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<FacturaIntrare>(id)
            ?? throw new OperareException($"Factura de intrare {id} nu există.");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        LoturiCulegereService.CurataOrfane(os);
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
            var noua = false;
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
                noua = true;
            }

            // Starea DE DINAINTEA mapării, pentru semantica recalculului de mai
            // jos: în UI recalculul TVA se declanșează DOAR la schimbarea bazei
            // (Cantitate/PretUnitar) sau a TipTva (`RecalculValoriCulegere`) —
            // un Save care nu le atinge NU pierde override-ul de ValoareTva.
            var bazaVeche = noua ? 0m : detaliu.PretUnitar * detaliu.Cantitate;
            Guid? tipTvaVechi = noua ? null : detaliu.TipTvaId;

            detaliu.TipMaterial = os.GetObjectByKey<TipMaterial>(l.TipMaterialId)
                ?? throw new OperareException($"Tipul (contul/clasa) {l.TipMaterialId} nu există.");

            // Produsul e mecanismul lotului (GATE XAF D1): îl consumă
            // `LoturiCulegereService` după reconciliere. `LotId` NU se atinge —
            // e server-owned pe FCT.
            if (l.ProdusId is Guid produsId) {
                detaliu.Produs = os.GetObjectByKey<Produs>(produsId)
                    ?? throw new OperareException($"Produsul {produsId} nu există în catalog.");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca BTR).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            VerificaScara(l.PretUnitar, Scara.Pret, "Prețul unitar");
            detaliu.Cantitate = l.Cantitate;
            detaliu.PretUnitar = l.PretUnitar;
            detaliu.CodCpv = l.CodCpv;
            detaliu.DataExpirare = l.DataExpirare;
            detaliu.LotFabricatie = l.LotFabricatie;

            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = os.GetObjectByKey<TipTva>(tipTvaId)
                    ?? throw new OperareException($"Tipul de TVA {tipTvaId} nu există.");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            if (l.AngajamentId is Guid angajamentId) {
                detaliu.Angajament = os.GetObjectByKey<Angajament>(angajamentId)
                    ?? throw new OperareException($"Angajamentul {angajamentId} nu există.");
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

            // Default-ul de TipTva al tipului de document (datoria P1 / 38d) —
            // DOAR pe liniile noi al căror payload n-a dat un TipTva: pe o linie
            // existentă, golirea lui e decizie explicită a operatorului, iar
            // re-aplicarea default-ului ar face-o imposibilă.
            if (noua && l.TipTvaId == null)
                TvaService.AplicaTipTvaImplicit(os, doc, detaliu);

            // Lanțul de valori, materializat LA CULEGERE (GATE 53c): operatorul
            // confruntă hârtia înainte de operare. `PregatesteOperare` îl rescrie
            // la operare din aceeași formulă — de aceea `Valoare` nu e în WriteDto.
            // Recalculul e CONDIȚIONAT de declanșatorii din UI (baza sau TipTva
            // schimbate) — altfel un PUT care editează doar header-ul ar pierde
            // tăcut override-ul de ValoareTva salvat anterior (clientul nu poate
            // distinge „valoarea citită e override" de „e calculată", deci nu o
            // retrimite — reziduul semnalat la pasul 4 al feliei).
            var bazaNoua = detaliu.PretUnitar * detaliu.Cantitate;
            if (noua || bazaNoua != bazaVeche || detaliu.TipTvaId != tipTvaVechi)
                TvaService.CalculeazaLaCulegere(os, detaliu, bazaNoua);
            // Override-ul operatorului, DUPĂ calcul (oglinda fluxului UI): factura
            // furnizorului bate rotunjirea noastră (regula 36a). La operare îl
            // păstrează `pastreazaTvaCules: true` — pe regimurile care postează TVA
            // separat; regimul Capitalizat îl normalizează la 0 (TVA-ul e în preț).
            if (l.ValoareTva is decimal valoareTva) {
                VerificaScara(valoareTva, Scara.Bani, "Valoarea TVA");
                // Review advers F2-D1/D7: override-ul are sens DOAR pe regimurile
                // care postează TVA separat (Normal/TaxareInversă). Pe Capitalizat
                // TVA-ul e deja în `Valoare` (l-ar număra de două ori în Total);
                // pe Scutit/Neimpozabil/fără TipTva, `PregatesteOperare` l-ar
                // șterge oricum la operare — acceptarea lui ar minți operatorul.
                // Negativul nu e TVA de intrare (stornarea are documentele ei).
                if (valoareTva < 0)
                    throw new OperareException("Valoarea TVA nu poate fi negativă.");
                // Prin FK, nu prin navigație: `AplicaTipTvaImplicit` setează doar
                // `TipTvaId`, iar navigația lazy nu e garantată pe toate căile (25b).
                var regim = detaliu.TipTvaId is Guid tipTvaLinieId
                    ? os.GetObjectByKey<TipTva>(tipTvaLinieId)?.Regim
                    : null;
                if (valoareTva != 0 && regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                    throw new OperareException(
                        "Valoarea TVA se completează manual doar pe un tip de TVA cu regim "
                        + "Normal sau Taxare inversă — regimul liniei nu poartă TVA separat.");
                detaliu.ValoareTva = valoareTva;
            }
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol) where T : class {
        if (id == null)
            return null;
        return os.GetObjectByKey<T>(id.Value)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclator.");
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        os.GetObjectByKey<Repartitor>(id)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclatorul de repartitori.");

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

    // `null` dacă documentul nu există (sau nu e o factură de intrare).
    public static FacturaIntrareReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<FacturaIntrare>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                // TPT: cast-ul devine LEFT JOIN pe tabela `Partener` — null pe
                // orice alt tip de repartitor.
                PredatorCodFiscal = (d.Predator as Partener).CodFiscal,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.DataScadenta, d.NumarPV, d.DataPV, d.CodCpv, d.Valuta, d.Curs,
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
                l.Cantitate, l.PretUnitar, l.Valoare, l.ValoareTva,
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

        // Affordance ONESTĂ (review advers F2-D5): gardianul de grup refuză
        // anularea/stornarea cât timp există un copil OPERAT — iar copiii sunt
        // deja calculați pentru DTO, deci consecința se arată, nu se descoperă
        // la refuz. F3-D2 închide și a doua condiție a motorului: STINGERILE
        // (`VerificaFaraImperecheri`) — factura stinsă de o plată nu se anulează
        // până nu se șterge link-ul.
        var copii = ApiProiectii.Copii(os, id);
        var faraCopiiOperati = copii.All(c => c.Stare != nameof(StareDocument.Operat));
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new FacturaIntrareReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PredatorCodFiscal = h.PredatorCodFiscal,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
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
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            Copii = copii,
            Linii = linii.Select(l => new FacturaIntrareLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, PretUnitar = l.PretUnitar,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva,
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
