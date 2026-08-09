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
            doc = os.GetObjectByKey<T>(existentId)
                ?? throw new OperareException($"{Fel<T>()} {existentId} nu există.");
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
        var doc = os.GetObjectByKey<T>(id)
            ?? throw new OperareException($"{Fel<T>()} {id} nu există.");
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

            detaliu.TipMaterial = os.GetObjectByKey<TipMaterial>(l.TipMaterialId)
                ?? throw new OperareException($"Tipul (contul/clasa) {l.TipMaterialId} nu există.");

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
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static TNomenclator Nomenclator<TNomenclator>(IObjectSpace os, Guid? id, string rol)
        where TNomenclator : class {
        if (id == null)
            return null;
        return os.GetObjectByKey<TNomenclator>(id.Value)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclator.");
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        os.GetObjectByKey<Repartitor>(id)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclatorul de repartitori.");

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

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).

    // `null` dacă documentul nu există SAU nu e de tipul cerut (sub TPT,
    // `GetObjectsQuery<Plata>` nu vede încasările — filtrarea e în SQL).
    public static TrezorerieReadDto Citeste<T>(IObjectSpace os, Guid id) where T : DocumentTrezorerie {
        var h = os.GetObjectsQuery<T>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.TipInstrument, d.NumarExtras, d.DataExtras,
                d.Autogenerat, d.DocumentSursaId,
                // LEFT JOIN pe documentul-sursă: plata autogenerată → numărul
                // FACTURII care a generat-o; null pe o plată culeasă manual.
                DocumentSursaNumar = d.DocumentSursa.Numar
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
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            DocumentSursaNumar = h.DocumentSursaNumar,
            DocumentSursaTip = ApiProiectii.CodTip(os, h.DocumentSursaId),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
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
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
