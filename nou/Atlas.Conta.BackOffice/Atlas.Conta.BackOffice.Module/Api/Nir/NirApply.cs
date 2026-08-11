using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Nir;

// Felia NIR: reconcilierea agregatului (scriere, F5-D8) + proiecțiile plate
// (citire, F2-D3). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe NIR în plus/în minus față de FCT ═══
// Pe tierul API nu rulează NICIUN ViewController, deci seam-ul de culegere al
// loturilor se apelează EXPLICIT (`LoturiCulegereService.Sincronizeaza`,
// echivalentul `DocumenteLoturiCulegereController.OnCommitting`). În MINUS față
// de FCT: NIR-ul nu culege TVA (F5-D5 — n-are `PoliticaTva`, deci nici
// `AplicaTipTvaImplicit`, nici `CalculeazaLaCulegere`) și nu culege `Numar`
// (are `PoliticaNumerotare`, seria e a operării).
public static class NirApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, NirWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        NIR doc;
        if (id is Guid existentId) {
            doc = os.GetObjectByKey<NIR>(existentId)
                ?? throw new OperareException($"NIR-ul {existentId} nu există.");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<NIR>();
        }

        // `Numar` NU se atinge (F5-D8): seria „NIR-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (GATE XAF D6) — gardianul de
        // Committing o și păzește pe tipurile cu politică de numerotare.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la FCT/BTR): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Partener → Gestiune) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (furnizorul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<NirLinieWriteDto>());

        // Seam-ul de culegere al loturilor (F2-D1, generalizat la F5-D3):
        // nașterea/sincronizarea/curățenia loturilor liniilor de stoc, ÎNAINTE de
        // commit. Liniile clonei conexe (lot STRĂIN) rămân neatinse — gardul
        // F5-D3, riscul propriu al feliei.
        LoturiCulegereService.Sincronizeaza(os, doc);

        // Valoarea liniei, materializată ABIA ACUM: formula depinde de lotul pe
        // care `Sincronizeaza` tocmai l-a născut/legat (vezi `MaterializeazaValori`).
        MaterializeazaValori(os, doc);

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), apoi curățenia loturilor NĂSCUTE LA CULEGERE ale
    // liniilor care dispar. Ordinea contează: `CurataOrfane` citește
    // `GetObjectsToDelete`, deci trebuie să vadă ștergerile DEJA marcate, dar
    // înaintea commit-ului (loturile intră în același SaveChanges). Loturile
    // FINALIZATE de motor nu se ating niciodată — inclusiv lotul STRĂIN al unei
    // clone conexe, care nici măcar nu e al liniilor de aici.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<NIR>(id)
            ?? throw new OperareException($"NIR-ul {id} nu există.");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");
        // Review advers F2: draftul AUTOGENERAT nu se șterge de mână. E artefactul
        // operării facturii (26d), iar când factura are numai linii de stoc el
        // poartă SINGURA postare a datoriei (`3xx = 401`) și singura intrare în
        // stoc — factura însăși duce doar rândul de TVA (26a). Ștergerea lui ar
        // lăsa o factură OPERATĂ, cu Total pe ecran, fără datorie și fără marfă
        // în registre, fără ca nimic să-i spună operatorului. Proprietarul
        // artefactului e motorul: anularea sursei îl șterge, re-operarea îl
        // regenerează. F5-D8b a tranșat EDITAREA lui (recepția parțială e flux de
        // producție) — anihilarea e altă decizie și nu se moștenește din ea.
        if (doc.Autogenerat)
            throw new OperareException(
                $"NIR-ul {Eticheta(doc)} e generat automat din factura-sursă — nu se șterge de aici. "
                + "Anulați operarea facturii: NIR-ul dispare odată cu ea, iar re-operarea îl regenerează.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        LoturiCulegereService.CurataOrfane(os);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, NIR doc, List<NirLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunzele NIR: un NIR
        // istoric/importat poartă linii de tip BAZĂ (pe care `Citeste` le arată
        // deliberat, iar `ValideazaOperare` NU le refuză — F5-D7), și payload-ul
        // e adevărul agregatului, deci reconcilierea trebuie să le vadă.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            NirDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as NirDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de NIR (tip vechi) — ștergeți-o din document "
                        + "și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<NirDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.TipMaterial = os.GetObjectByKey<TipMaterial>(l.TipMaterialId)
                ?? throw new OperareException($"Tipul (contul/clasa) {l.TipMaterialId} nu există.");

            // Produsul e mecanismul lotului (F5-D2): îl consumă
            // `LoturiCulegereService` după reconciliere. `LotId` NU se atinge —
            // e server-owned (F5-D4).
            //
            // Review advers F3: pe linia cu lot STRĂIN (clona conexă) produsul e
            // declarat peste tot „inert" — dar inert însemna „nefolosit de
            // serviciu", nu „negolit". Diferența nu e de stil: persistat, îl
            // citește validarea de coerență Tip↔Produs, deci un apelant al API-ului
            // (import, script, felie viitoare) care pune acolo un produs de alt Tip
            // face NIR-ul PERMANENT ne-operabil, cu un mesaj care arată spre un
            // câmp pe care UI-ul îl afișează read-only. Marfa liniei conexe e a
            // lotului moștenit — aici o GOLIM, ca inerția să fie adevărată.
            var lotStrainAlLiniei = detaliu.LotId is Guid lotExistentId
                && os.GetObjectByKey<Lot>(lotExistentId)?.LinieIntrareId != detaliu.ID;
            if (lotStrainAlLiniei) {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }
            else if (l.ProdusId is Guid produsId) {
                detaliu.Produs = os.GetObjectByKey<Produs>(produsId)
                    ?? throw new OperareException($"Produsul {produsId} nu există în catalog.");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca FCT/BTR).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            VerificaScara(l.PretUnitar, Scara.Pret, "Prețul unitar");
            detaliu.Cantitate = l.Cantitate;
            detaliu.PretUnitar = l.PretUnitar;
            detaliu.DataExpirare = l.DataExpirare;
            detaliu.LotFabricatie = l.LotFabricatie;

            // `TipTva`/`ValoareTva` NU se ating (F5-D5): NIR-ul nu culege TVA,
            // iar clona conexă își păstrează TipTva-ul informativ primit de la
            // factură — un PUT nu are voie să i-l șteargă.

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

    // Lanțul de valori, materializat LA CULEGERE (GATE 53c: operatorul confruntă
    // hârtia înainte de operare) — de aceea `Valoare` nu e în WriteDto.
    //
    // Formula e GEAMĂNA lui `NIR.PregatesteOperare` (F5-D6), care o rescrie la
    // operare: cele două cazuri ale recepției, cu o formulă fiecare.
    //   (a) lot STRĂIN (clona conexă): `Cantitate × lot.PretUnitar` — recepția
    //       PARȚIALĂ e legitimă (operatorul scade cantitatea primită), iar
    //       valoarea trebuie să-l urmeze pe ecran, nu abia după operare;
    //   (b) lot PROPRIU sau încă nenăscut (recepție manuală): `PretUnitar ×
    //       Cantitate` — prețul cules pe linie. Cazul „lot null" cade corect pe
    //       ramura asta: lotul nu s-a putut naște încă (latura primitoare
    //       necompletată, skip-ul grațios al serviciului), dar prețul cules e
    //       deja adevărul liniei.
    //
    // Rulează DUPĂ `Sincronizeaza`: abia atunci liniile manuale au lot, iar
    // proveniența lui (propriu vs străin) e decidabilă.
    //
    // Doar FRUNZELE, ca în hook (`else if (d is NirDetaliu nd)`): liniile de tip
    // BAZĂ ale NIR-urilor istorice/importate n-au de unde lua un preț, iar
    // rescrierea valorii lor pe un PUT care nici măcar nu le poate atinge
    // (reconcilierea le refuză pe Id) ar fi exact clasa de defect a review-ului
    // GATE D1 — pierdere tăcută de dată contabilă reală.
    static void MaterializeazaValori(IObjectSpace os, NIR doc) {
        foreach (var d in doc.Detalii.OfType<NirDetaliu>()) {
            // Liniile marcate spre ștergere în acest commit nu se mai ating.
            if (os.IsObjectToDelete(d))
                continue;
            var lot = d.LotId is Guid lotId ? os.GetObjectByKey<Lot>(lotId) : null;
            d.Valoare = lot != null && lot.LinieIntrareId != d.ID
                ? Scara.RotunjesteBani(d.Cantitate * lot.PretUnitar)
                : Scara.RotunjesteBani(d.PretUnitar * d.Cantitate);
        }
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

    // `null` dacă documentul nu există (sau nu e un NIR).
    public static NirReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<NIR>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.Autogenerat, d.DocumentSursaId,
                // LEFT JOIN pe documentul-sursă: null pe un NIR cules manual.
                DocumentSursaNumar = d.DocumentSursa.Numar
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza NIR (DIM-2) adusă
        // prin `as` (TPT ⇒ LEFT JOIN în SQL): clona conexă generată azi se naște
        // pe `NirDetaliu` ([TipDetaliu]), dar NIR-urile ISTORICE (importul/clonele
        // pre-DIM-2) poartă linii de tip BAZĂ — pe frunză singură ar fi ieșit
        // `Linii: []` cu `Total` nenul (constatarea pasului 3 al feliei).
        // Dimensiunile sunt null pe liniile de bază — exact ce poartă.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                ProdusId = (l as NirDetaliu).ProdusId,
                ProdusCod = (l as NirDetaliu).Produs.Cod,
                ProdusDenumire = (l as NirDetaliu).Produs.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                // Proveniența lotului, decisă ÎN SQL (F5-D8): lotul e STRĂIN dacă
                // nu s-a născut din linia asta. Comparația se face pe `LinieIntrareId`
                // (coloană fără FK — 26e), nu pe prezența produsului.
                LotLinieIntrareId = l.Lot.LinieIntrareId,
                // NULLABLE explicit: pe o linie de tip BAZĂ cast-ul TPT dă null,
                // iar un `decimal` non-nullable ar pica la materializare.
                l.Cantitate, PretUnitar = (decimal?)(l as NirDetaliu).PretUnitar,
                l.Valoare, l.ValoareTva,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod,
                DataExpirare = (l as NirDetaliu).DataExpirare,
                LotFabricatie = (l as NirDetaliu).LotFabricatie,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod,
                CodEconomicId = (l as NirDetaliu).CodEconomicId,
                CodEconomicCod = (l as NirDetaliu).CodEconomic.Cod,
                SursaFinantareId = (l as NirDetaliu).SursaFinantareId,
                SursaFinantareCod = (l as NirDetaliu).SursaFinantare.Cod,
                CodFunctionalId = (l as NirDetaliu).CodFunctionalId,
                CodFunctionalCod = (l as NirDetaliu).CodFunctional.Cod,
                ProiectId = (l as NirDetaliu).ProiectId,
                ProiectCod = (l as NirDetaliu).Proiect.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca
        // să dea EXACT ce dă `Lista` chiar dacă documentul ar purta o linie de
        // tip bază (NIR cules manual înainte de frunza DIM-2).
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordance ONESTĂ pe stingeri (F3-D2): NIR-ul nu e creanță și n-ar
        // trebui să poarte imperecheri, dar gardianul motorului
        // (`VerificaFaraImperecheri`) e generic pe `Document` — dacă totuși
        // există un link (import, compensare pe notă), refuzul se ARATĂ, nu se
        // descoperă la apăsarea butonului. Un `Any` mărginit per citire.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new NirReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            DocumentSursaNumar = h.DocumentSursaNumar,
            DocumentSursaTip = ApiProiectii.CodTip(os, h.DocumentSursaId),
            // F5-D8b: affordance-ul e o afirmație despre CE POATE FACE SERVERUL
            // (42e), nu despre ce alegea să expună o felie anterioară. Review-ul
            // F2-D5 îl ținea FALS fiindcă tierul n-avea NICIO cale de scriere pe
            // NIR (F2-D3); felia 5 o adaugă, deci affordance-ul redevine funcție
            // de stare — iar `false` ar deveni el însuși minciuna, inversă față
            // de cea pe care F2-D5 o prevenea.
            //
            // Fără condiție pe `Autogenerat`: Draftul CONEX e proiectat să fie
            // deschis în editare (26d), iar F5-D4 (gardul de lot străin) e chiar
            // contractul care face editarea lui sigură. Recepția PARȚIALĂ —
            // operatorul scade pe NIR-ul generat cantitatea chiar primită — e
            // flux de producție, nu accident.
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new NirLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                // Fără lot nu există „străin": linia manuală abia începută are
                // produs cules și lotul încă nenăscut — nu e recepție moștenită.
                LotStrain = l.LotId != null && l.LotLinieIntrareId != l.ID,
                Cantitate = l.Cantitate, PretUnitar = l.PretUnitar ?? 0m,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                DataExpirare = l.DataExpirare, LotFabricatie = l.LotFabricatie,
                AngajamentId = l.AngajamentId, AngajamentCod = l.AngajamentCod,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                SursaFinantareId = l.SursaFinantareId, SursaFinantareCod = l.SursaFinantareCod,
                CodFunctionalId = l.CodFunctionalId, CodFunctionalCod = l.CodFunctionalCod,
                ProiectId = l.ProiectId, ProiectCod = l.ProiectCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<NirListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<NIR>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new NirListDto {
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
                   Autogenerat = d.Autogenerat,
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
