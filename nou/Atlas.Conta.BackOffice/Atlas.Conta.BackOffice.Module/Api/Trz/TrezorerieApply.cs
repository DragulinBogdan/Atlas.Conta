using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Trz;

// Nucleul feliei TREZORERIE: reconcilierea agregatului (scriere) și proiecțiile
// plate (citire), GENERIC pe `T : DocumentTrezorerie` (F3-D1). ZERO ASP.NET aici
// — controllerele `api/plt` / `api/inc` din host sunt transport subțire peste
// `Aplica<Plata>` / `Aplica<Incasare>`, iar ModelCheck exersează exact acest cod
// pe `EFCoreObjectSpaceProvider` standalone (precedentul: motorul — docs 113709).
//
// De ce generic și nu două copii: `Plata` și `Incasare` au aceeași schemă și
// aceeași frunză de detaliu (`DocumentTrezorerieDetaliu` — DIM-2/Î1); tot ce le
// deosebește sunt laturile, iar acelea se validează la OPERARE, în hook-urile
// tipului. Un al doilea exemplar al reconcilierii ar diverge tăcut.
// Genericul rămâne traductibil în SQL: `GetObjectsQuery<T>` rezolvă tipul la
// RUNTIME (`EFCoreObjectSpace.GetQuery(objectType)`), deci sub TPT ajunge
// `DbSet<Plata>`/`DbSet<Incasare>` — nu se materializează nimic în memorie.
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului și COMIT. Gardianul de Committing e ultima autoritate —
// pre-check-ul de Draft există ca mesajul să fie al DOMENIULUI și ca refuzul să
// vină înaintea oricărei modificări de stare în ObjectSpace-ul viu.
public static class TrezorerieApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica<T>(IObjectSpace os, Guid? id, TrezorerieWriteDto dto)
        where T : DocumentTrezorerie {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");
        // Parse-ul enum-ului ÎNAINTE de a atinge ObjectSpace-ul: pe calea de
        // CREARE un refuz de după `CreateObject` ar lăsa un document orfan în
        // OS-ul viu al apelantului, pe care un commit ulterior l-ar persista
        // (aceeași grijă ca „numărul consumat la materializare" — GATE XAF D6).
        var tipInstrument = ApiEnum.TipInstrument(dto.TipInstrument);

        T doc;
        if (id is Guid existentId) {
            // `GetObjectByKey<T>` filtrează și pe TIP sub TPT: un id de încasare
            // cerut pe ruta plăților nu „adoptă" documentul, ci întoarce null.
            doc = Rezolva.Cere<T>(os, existentId, Fel<T>());
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<T>();
        }

        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR/FCT): (1) rezolvarea validează
        // existența cu mesaj de domeniu, (2) regulile XAF de culegere stau pe
        // navigație, (3) pe o entitate urmărită, navigația încărcată ar rescrie
        // la fixup un FK setat direct.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul");
        doc.TipInstrument = tipInstrument;
        doc.NumarExtras = dto.NumarExtras;
        doc.DataExtras = dto.DataExtras;
        // F8-D11: legătura de pereche e câmp CULES, aplicat ca oricare altul.
        // NAVIGAȚIA, nu FK-ul scalar (regula de mai sus): existența țintei se
        // validează cu mesaj de domeniu, nu cu violare de FK la commit. E
        // SINGURA verificare făcută aici — cele șapte reguli ale legăturii
        // (virament de ambele părți, tip opus, aceleași laturi, fără triplete,
        // fără reciprocitate, fără self-link) rămân în motor (F8-D8), unde le
        // vede și calea XAF, și importul.
        if (dto.LaturaPerecheId is Guid perecheId) {
            // Self-link-ul se refuză AICI, deși e regulă de motor (F8-D8, punctul
            // 1): apply-ul cunoaște `id`, iar acceptarea lui ar produce un draft
            // pe care operatorul nu-l poate nici opera, nici șterge (`Sterge` îl
            // refuză: se arată pe sine ca pereche) — o culegere care fabrică o
            // fundătură. Mesajul e IDENTIC cu al motorului: o singură formulare
            // pentru aceeași greșeală, pe orice cale.
            if (perecheId == doc.ID)
                throw new OperareException(
                    "Latura pereche nu poate fi documentul însuși — alegeți celălalt picior al viramentului.");
            doc.LaturaPereche = Rezolva.Cere<DocumentTrezorerie>(os, perecheId, "Documentul indicat ca latură pereche");
        }
        else {
            doc.LaturaPereche = null;
            doc.LaturaPerecheId = null;
        }
        // `Numar` NU se atinge: PLT/INC au PoliticaNumerotare ⇒ server-owned
        // (F3-D1) — nici nu e în WriteDto, nici gardianul nu l-ar accepta.

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<TrezorerieLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa). Fără curățenie de loturi (trezoreria nu poartă
    // stoc) și fără gardian de imperecheri: un link cere ambele documente
    // OPERATE (31d), deci un draft nu poate avea niciunul.
    public static void Sterge<T>(IObjectSpace os, Guid id) where T : DocumentTrezorerie {
        var doc = Rezolva.Cere<T>(os, id, Fel<T>());
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        // F8-D11: piciorul ARĂTAT ca pereche de alt document nu se șterge cât
        // timp legătura există — altfel pointerul ar rămâne să arate spre un
        // document dispărut, iar suprimarea generării (F8-D7) l-ar cita.
        //
        // Pre-check-ul e SINGURUL gard real, nu doar un mesaj mai frumos: FK-ul
        // `Restrict` (F8-D6) nu se atinge niciodată pe calea asta, fiindcă
        // modelul folosește ștergere AMÂNATĂ (`UseDeferredDeletion`, 60a) —
        // rândul rămâne în tabelă cu `GCRecord` setat. Refuzul e de DOMENIU, cu
        // documentul numit și cu ieșirea: legătura se ține pe o singură parte,
        // deci se șterge de acolo.
        var pointer = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.LaturaPerecheId == id)
            .Select(x => new { x.ID, x.Numar, x.Data })
            .FirstOrDefault();
        // Cazul degenerat: documentul se arată PE SINE. `Aplica` îl refuză acum pe
        // loc (vezi acolo), deci pe calea API nu se mai poate naște — dar rândul
        // poate veni din altă parte (UI-ul XAF, un import vechi), iar pre-check-ul
        // de mai sus l-ar prinde cu mesajul greșit („e declarat latura pereche a
        // lui …" — a lui însuși). Mesajul propriu îi dă ieșirea: goliți câmpul,
        // apoi ștergeți.
        if (pointer != null && pointer.ID == id)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} se declară PE SINE latură pereche — goliți întâi "
                + "câmpul „latura pereche”, apoi ștergeți documentul.");
        if (pointer != null)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} e declarat latura pereche a lui "
                + $"{Eticheta(pointer.Numar, pointer.Data)} — ștergeți întâi acea legătură.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, DocumentTrezorerie doc,
        List<TrezorerieLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunza: un draft
        // vechi (sau un import) poate purta o linie de tip BAZĂ, iar payload-ul e
        // adevărul agregatului — reconcilierea o curăță în loc s-o lase invizibilă.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            DocumentTrezorerieDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as DocumentTrezorerieDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de trezorerie (tip vechi) — ștergeți-o "
                        + "din document și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<DocumentTrezorerieDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o sumă în afara lui numeric(18,2) ar ieși ca DbUpdateException brută
            // din Postgres. Refuzăm cu mesaj de domeniu (ca BTR/FCT).
            VerificaScara(l.Valoare, Scara.Bani, "Valoarea");
            // CULEASĂ, nu calculată: trezoreria n-are `PregatesteOperare` (F3-D1).
            // Pozitivitatea e invariant al OPERĂRII (`DocumentTrezorerie`), nu al
            // culegerii — un draft în lucru are voie să aibă o linie pe 0.
            detaliu.Valoare = l.Valoare;
            // `Cantitate`/`Lot`/`TipTva`/`ValoareTva` NU se ating: n-au semantică
            // pe trezorerie și nu sunt în WriteDto. Pe liniile clonate din factură
            // (plata autogenerată) rămân exact cum le-a lăsat motorul.

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
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static TNomenclator Nomenclator<TNomenclator>(IObjectSpace os, Guid? id, string rol)
            where TNomenclator : class => Rezolva.Optional<TNomenclator>(os, id, rol);

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        Rezolva.Cere<Repartitor>(os, id, rol);

    // Gardul de scară — geamănul celui din `FacturaIntrareApply`: `numeric(18,s)`
    // ⇒ cel mult `s` zecimale și `18 − s` cifre întregi (49e).
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

    // Numele TIPULUI în mesajele de domeniu — genericul nu trebuie să vorbească
    // despre „documentul de trezorerie" când operatorul e pe ecranul de plăți.
    static string Fel<T>() where T : DocumentTrezorerie =>
        typeof(T) == typeof(Plata) ? "Plata"
        : typeof(T) == typeof(Incasare) ? "Încasarea"
        : "Documentul de trezorerie";

    static string Eticheta(Document doc) => Eticheta(doc.Numar, doc.Data);

    // Varianta pe câmpuri PLATE: mesajele care numesc un ALT document îl citesc
    // dintr-o proiecție (25b — nicio navigație materializată pentru un mesaj).
    static string Eticheta(string numar, DateOnly data) =>
        string.IsNullOrWhiteSpace(numar) ? $"({data:dd.MM.yyyy})" : numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).

    // `null` dacă documentul nu există / nu e vizibil (F22-D1) SAU nu e de tipul cerut (sub TPT,
    // `GetObjectsQuery<Plata>` nu vede încasările — filtrarea e în SQL).
    public static TrezorerieReadDto Citeste<T>(IObjectSpace os, Guid id) where T : DocumentTrezorerie {
        var h = os.GetObjectsQuery<T>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.TipInstrument, d.NumarExtras, d.DataExtras,
                d.Autogenerat, d.DocumentSursaId, d.LaturaPerecheId,
                // LEFT JOIN pe documentul-sursă: plata autogenerată → numărul
                // FACTURII care a generat-o; null pe o plată culeasă manual.
                DocumentSursaNumar = d.DocumentSursa.Numar,
                // F7-D7: predicatul de virament, în ACELAȘI query (test de tip
                // pe laturi — sub TPT devine JOIN + IS NOT NULL, nu o a doua
                // interogare). Formula e cea a domeniului
                // (`DocumentTrezorerie.EsteVirament`): AMBELE laturi conturi
                // proprii, nu doar contrapartida — un draft cu laturile
                // inversate are contrapartida cont propriu fără să fie virament.
                EsteVirament = d.Predator is ContPropriu && d.Primitor is ContPropriu
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza adusă prin `as`
        // (TPT ⇒ LEFT JOIN în SQL) — uniformitatea citirii e regula feliilor
        // (pattern-ul NIR): o linie de tip bază (draft vechi, import) apare cu
        // dimensiunile null, în loc să dispară din `Linii` lăsând `Total` nenul.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.Valoare,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod,
                CodEconomicId = (l as DocumentTrezorerieDetaliu).CodEconomicId,
                CodEconomicCod = (l as DocumentTrezorerieDetaliu).CodEconomic.Cod,
                SursaFinantareId = (l as DocumentTrezorerieDetaliu).SursaFinantareId,
                SursaFinantareCod = (l as DocumentTrezorerieDetaliu).SursaFinantare.Cod,
                CodFunctionalId = (l as DocumentTrezorerieDetaliu).CodFunctionalId,
                CodFunctionalCod = (l as DocumentTrezorerieDetaliu).CodFunctional.Cod,
                ProiectId = (l as DocumentTrezorerieDetaliu).ProiectId,
                ProiectCod = (l as DocumentTrezorerieDetaliu).Proiect.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca
        // să dea EXACT ce dă `Lista` și ce vede `ImperechereService.Total`.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordance onestă pe AMBELE condiții ale motorului (F3-D2): grupul
        // conex (copil operat) ȘI stingerile — plata care a stins o factură nu
        // se anulează până nu se șterge imperecherea.
        var copii = ApiProiectii.Copii(os, id);
        var faraCopiiOperati = copii.All(c => c.Stare != nameof(StareDocument.Operat));
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);
        var (pereche, perecheActiva) = Pereche<T>(os, id);
        // Oglinda gardianului `MotorOperare.VerificaFaraLaturaPerecheOperata`
        // (F8-D9). Grupul conex („copil operat") îl acoperea DOAR pentru perechea
        // AUTOGENERATĂ, care e și copil; legătura DECLARATĂ manual n-are
        // `DocumentSursa`, deci fără asta affordance-ul spunea „se poate anula"
        // despre un document pe care motorul îl refuză — exact clasa de minciună
        // închisă la F3-D2/F5-D8b. CUSĂTURĂ: predicatul e identic cu al
        // gardianului (pointer OPERAT); dacă acolo se schimbă, aici minte.
        var faraLaturaPerecheOperata = !os.GetObjectsQuery<DocumentTrezorerie>()
            .Any(x => x.LaturaPerecheId == id && x.Stare == StareDocument.Operat);

        return new TrezorerieReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            TipInstrument = h.TipInstrument.ToString(),
            NumarExtras = h.NumarExtras, DataExtras = h.DataExtras,
            Total = total,
            // Numerele stingerii DIN SERVICIU (F3-D2), nu recalculate aici:
            // `Asignat` numără ambele roluri, `Ramas` = Total − Asignat cu
            // `Total` trecut prin `LiniiCreanta`. Pe trezorerie cele două
            // `Total`-uri coincid (niciun override) — ModelCheck o verifică; a
            // doua agregare locală ar fi fost un al doilea adevăr. Trei
            // interogări mărginite, doar pe CITIREA de detaliu (nu în `Lista`).
            Asignat = ImperechereService.Asignat(os, id),
            Ramas = ImperechereService.Ramas(os, id),
            EsteVirament = h.EsteVirament,
            LaturaPerecheId = h.LaturaPerecheId, Pereche = pereche,
            PerecheActiva = perecheActiva,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            DocumentSursaNumar = h.DocumentSursaNumar,
            DocumentSursaTip = ApiProiectii.CodTip(os, h.DocumentSursaId),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri
                && faraLaturaPerecheOperata,
            PoateStorna = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri
                && faraLaturaPerecheOperata,
            Copii = copii,
            Linii = linii.Select(l => new TrezorerieLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                Valoare = l.Valoare,
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
    // `Total` prin JOIN PE AGREGAT, nu subquery corelat (42c), pe BAZA
    // detaliului; LEFT JOIN-ul păstrează drafturile fără linii.
    public static IQueryable<TrezorerieListDto> Lista<T>(IObjectSpace os)
        where T : DocumentTrezorerie {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<T>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new TrezorerieListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // Enum → string ÎN SQL (`CASE`): pe sârmă sunt text, dar
                   // filtrarea și sortarea rămân server-side (grila e remote).
                   // `ToString()` pe enum nu e o traducere garantată la toți
                   // providerii — expresia explicită e.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   TipInstrument = d.TipInstrument == TipInstrumentPlata.OrdinPlata ? "OrdinPlata"
                       : d.TipInstrument == TipInstrumentPlata.Cec ? "Cec"
                       : d.TipInstrument == TipInstrumentPlata.DispozitieCasa ? "DispozitieCasa"
                       : "Chitanta",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Autogenerat = d.Autogenerat,
                   // Aceeași formulă ca în `Citeste` (predicatul domeniului:
                   // AMBELE laturi conturi proprii), tot în SQL — sub TPT testul
                   // de tip devine LEFT JOIN pe `ContPropriu` + IS NOT NULL.
                   EsteVirament = d.Predator is ContPropriu && d.Primitor is ContPropriu,
                   Total = (decimal?)t.Total ?? 0m
               };
    }

    // ═══════════════════════ Latura pereche (F8-D11) ═══════════════════════

    // Perechea REZOLVATĂ a unui picior, pentru ReadDto. Derivarea „link propriu
    // SAU cine mă arată SAU grupul conex autogenerat" NU se rescrie aici: e a
    // domeniului (`DocumentTrezorerie.PerecheId`, F8-D6) și are alți consumatori
    // — suprimarea generării (F8-D7), gardianul de anulare/storno (F8-D9) și
    // avertismentul (F8-D10). Un al doilea exemplar ar diverge tăcut de ei exact
    // în cazul pentru care felia există.
    //
    // Întoarce PERECHEA (descriptivă — o vede și dacă e stornată, ca s-o poată
    // deschide) ȘI dacă e ACTIVĂ (`PerecheActivaId`, adică 581 chiar s-a închis).
    // Clientul ramifică pe boolean, nu pe stare — zero predicat de domeniu în TS.
    //
    // Costul: o materializare a documentului (`GetObjectByKey`, TPT) pe CITIREA
    // de detaliu — alături de cele trei interogări ale stingerii, aceeași
    // categorie. Al doilea apel (cel „activ") se face DOAR când perechea găsită
    // e stornată: filtrele lui sunt o submulțime a celor descriptive, deci o
    // pereche negăsită descriptiv nu poate fi găsită activ, iar una găsită și
    // ne-stornată trece prin exact același filtru la aceeași sursă. NU intră în
    // `Lista` (acolo ar fi al doilea agregat pe rând).
    static (LaturaPerecheDto Pereche, bool Activa) Pereche<T>(IObjectSpace os, Guid id)
        where T : DocumentTrezorerie {
        var doc = os.GetObjectByKey<T>(id);
        if (doc?.PerecheId(os) is not Guid perecheId)
            return (null, false);
        // Proiecție PLATĂ pe celălalt picior — nu-l materializăm ca entitate doar
        // ca să-i citim numărul.
        var p = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.ID == perecheId)
            .Select(x => new { x.ID, x.Numar, x.Stare })
            .FirstOrDefault();
        if (p == null)
            return (null, false);
        // Perechea ACTIVĂ poate fi un ALT document decât cea descriptivă (linkul
        // meu arată spre unul stornat, dar altcineva mă arată pe mine) — de asta
        // se întreabă domeniul, nu se deduce din starea celui afișat.
        var activa = p.Stare != StareDocument.Stornat || doc.PerecheActivaId(os) != null;
        return (new LaturaPerecheDto {
            Id = p.ID,
            // Codul ancorei `TipDocument` (PLT/INC) — rezolvat POLIMORF, ca la
            // „Generat din" (D-6b): clientul rutează prin `rutaTip`, nu presupune
            // că perechea unei plăți e mereu la `/inc/`.
            Tip = ApiProiectii.CodTip(os, p.ID),
            Numar = p.Numar,
            Stare = p.Stare.ToString()
        }, activa);
    }

    // Plafonul listei de candidați: e sursa unui SelectBox, nu o grilă paginată
    // (fără `DataSourceLoader`, deci fără `take` de la client). Mulțimea e deja
    // îngustă prin construcție — picioare neîmperecheate între EXACT aceleași
    // două conturi proprii.
    const int PlafonCandidati = 50;

    // Picioarele care pot fi declarate pereche pentru un document aflat în
    // culegere (F8-D11). `T` = tipul RUTEI, `TOpus` = tipul candidaților.
    //
    // De ce două tipuri și nu unul: query-ul trebuie să filtreze tipul ÎN SQL
    // (sub TPT nu există discriminator, iar mulțimea „viramente între două
    // conturi" nu e mărginită pe o bază reală — o materializare cu filtrare în
    // memorie ar fi al doilea `CoduriTip` per rând, 60b), deci tipul opus trebuie
    // să fie cunoscut la COMPILARE. Transportul îl are (ruta e concretă), dar
    // AUTORITATEA rămâne contractul domeniului `TipLaturaPereche()` (F8-D8), care
    // e verificat mai jos: cele două declarații nu pot diverge tăcut.
    public static IReadOnlyList<CandidatPerecheDto> CandidatiPereche<T, TOpus>(
        IObjectSpace os, Guid predatorId, Guid primitorId, Guid? exclusId)
        where T : DocumentTrezorerie, new()
        where TOpus : DocumentTrezorerie {

        // Instanța e DETAȘATĂ (niciodată în ObjectSpace): `TipLaturaPereche` e o
        // funcție pură a tipului, nu a stării. Nepotrivirea e bug de wiring, nu
        // configurare — pică zgomotos, la primul apel.
        var tipContract = new T().TipLaturaPereche();
        if (tipContract != typeof(TOpus))
            throw new InvalidOperationException(
                $"Ruta {typeof(T).Name} cere candidați de tip {typeof(TOpus).Name}, "
                + $"dar contractul domeniului declară {tipContract.Name}.");

        // Criteriul e „fără pereche OPERATĂ", nu „fără pereche" — ACELAȘI ca al
        // avertismentului (F8-D10), și din același motiv: în fluxul canonic 64k
        // piciorul căutat și-a generat singur un draft care-l arată, deci un
        // anti-join pe „e arătat de cineva" ar goli lista exact când operatorul
        // culege al doilea picior. Contabil, 581 se închide doar când al doilea
        // picior e OPERAT.
        //
        // Doar pointer-ul OPERAT îl scoate definitiv (perechea s-a produs). Cel
        // STORNAT nu contează (fix review D1): are registrele inversate, deci 581
        // e redeschis, iar candidatul chiar E descoperit — plus că refuzul ar fi
        // fost o fundătură (un pointer stornat nu se mai poate nici edita, nici
        // șterge, deci remediul „ștergeți acea legătură" e imposibil). Cel Draft
        // îl lasă în listă, dar cu `PerecheDraftNumar` completat mai jos.
        // Anti-join pe aceeași tabelă (forma din `DocumenteCuRest`), nu subquery
        // corelat (42c).
        var cuPerecheOperata = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.LaturaPerecheId != null && x.Stare == StareDocument.Operat)
            .Select(x => x.LaturaPerecheId.Value);

        var candidati = os.GetObjectsQuery<TOpus>()
            // Laturile sunt filtrul PRIMAR (F7-D1: cele două picioare stau pe
            // aceleași laturi, direcția o poartă tipul) — vin de la client, deci
            // predicatul de virament NU se presupune: e verificat aici, în aceeași
            // formă cu al domeniului (AMBELE laturi conturi proprii).
            .Where(d => d.PredatorId == predatorId && d.PrimitorId == primitorId
                && d.Predator is ContPropriu && d.Primitor is ContPropriu
                // Linkul PROPRIU rămâne excludere absolută, indiferent de stare:
                // F8-D8 punctul 5 refuză o țintă care declară deja pe altcineva,
                // iar remediul nu e al nostru (e legătura ei).
                && d.LaturaPerecheId == null
                && !cuPerecheOperata.Contains(d.ID)
                // O latură GENERATĂ de motor aparține deja transferului ei, prin
                // grupul conex — chiar dacă linkul i-a fost golit (câmpul e cules,
                // iar golirea e la un click). Fără predicatul ăsta ar apărea în
                // lista altui document, iar legarea ar produce exact dublarea pe
                // 581 pe care felia o închide. Oglinda lui în validare (F8-D8)
                // rămâne autoritatea; aici e afordanța care nu ademenește.
                && !(d.Autogenerat && d.DocumentSursaId != null)
                // Draft ȘI Operat: validarea legăturii (F8-D8) nu cere stare —
                // ambele sunt legături legitime (piciorul cules înainte de
                // operare, sau cel deja operat de ieri). `Stornat` nu se OFERĂ:
                // nu e regulă de refuz (rămâne al motorului), e afordanță — un
                // picior stornat nu mai are ce închide pe 581.
                && (d.Stare == StareDocument.Draft || d.Stare == StareDocument.Operat));
        if (exclusId is Guid exclus)
            candidati = candidati.Where(d => d.ID != exclus);

        // `Total` prin JOIN PE AGREGAT, ca în `Lista` (42c).
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        var lista = (from d in candidati
                     join t in totaluri on d.ID equals t.DocumentId into agregat
                     from t in agregat.DefaultIfEmpty()
                     // Cronologic DESCENDENT: piciorul căutat e aproape întotdeauna
                     // cel recent (foaia de vărsământ de ieri, extrasul de azi).
                     orderby d.Data descending, d.ID descending
                     select new CandidatPerecheDto {
                         Id = d.ID,
                         Numar = d.Numar,
                         Data = d.Data,
                         Stare = d.Stare == StareDocument.Draft ? "Draft" : "Operat",
                         Total = (decimal?)t.Total ?? 0m
                     }).Take(PlafonCandidati).ToList();

        // Draftul care blochează legarea, pe mulțimea DEJA plafonată (≤ 50): o
        // interogare mărginită, nu un subquery corelat per rând.
        //
        // `exclusId` se aplică ȘI aici, nu doar pe candidați: după ce operatorul
        // salvează legătura, documentul CURENT devine el însuși pointer Draft
        // spre candidatul pe care tocmai l-a ales — s-ar fi întors ca „blocat de
        // draftul (data)" (numărul e gol pe draft), adică documentul deschis pe
        // ecran blocându-și propria alegere.
        var ids = lista.Select(c => c.Id).ToList();
        var drafturiBlocante = os.GetObjectsQuery<DocumentTrezorerie>()
            .Where(x => x.LaturaPerecheId != null && ids.Contains(x.LaturaPerecheId.Value)
                && x.Stare == StareDocument.Draft
                && (exclusId == null || x.ID != exclusId))
            .Select(x => new { Tinta = x.LaturaPerecheId.Value, x.Numar, x.Data })
            .ToList();
        foreach (var candidat in lista) {
            var blocant = drafturiBlocante.FirstOrDefault(x => x.Tinta == candidat.Id);
            if (blocant != null)
                candidat.PerecheDraftNumar = Eticheta(blocant.Numar, blocant.Data);
        }
        return lista;
    }
}
