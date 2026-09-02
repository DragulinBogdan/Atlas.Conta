using Atlas.Conta.BackOffice.Module.BusinessObjects;
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
// ═══ Ce e propriu feliei: DIRECȚIA conduce culegerea ═══
// Fiecare linie e un plus SAU un minus, iar cele două n-au aceleași câmpuri.
// Reconcilierea aplică deci DOUĂ contracte diferite pe aceeași frunză:
//   * MINUS — descarcă un lot EXISTENT: se aplică pinul `LotId`, iar câmpurile
//     plusului (produs, preț de evaluare, atributele lotului) se GOLESC (F6-D3).
//     Golirea e persistată, nu doar ignorată — lecția F5 („inert devine adevărat,
//     nu doar afirmat"): un produs rămas pe linie din starea de plus l-ar citi
//     validarea de coerență Tip↔Produs și ar putea face documentul permanent
//     ne-operabil, printr-un câmp pe care UI-ul nu-l mai arată.
//   * PLUS — NAȘTE lotul din `ProdusId`, prin `LoturiCulegereService`, în
//     gestiunea INVENTARIATĂ (predatorul — hook-ul `GestiuneLoturiCulese`,
//     F6-D2). `LotId` din payload se IGNORĂ: e server-owned.
// Gardul de direcție trăiește în SERVICIU (`ILinieCareNasteLot.NasteLot` —
// F6-D3), nu aici: culegerea golește câmpurile, serviciul curăță lotul.
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
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Gestiune → comisie) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea inventariată)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (comisia de inventariere)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<LdiLinieWriteDto>());

        // Seam-ul de culegere al loturilor (F2-D1, generalizat la F5-D3): naște
        // lotul plusului din `ProdusId` și curăță lotul propriu al liniilor care
        // nu mai nasc (minusul — gardul `NasteLot`, F6-D3). Pinul liniei de minus
        // rămâne NEATINS (gardul de lot străin).
        LoturiCulegereService.Sincronizeaza(os, doc);

        // Valoarea liniei, materializată ABIA ACUM: pe plus formula are nevoie de
        // lotul pe care `Sincronizeaza` tocmai l-a născut (vezi `MaterializeazaValori`).
        MaterializeazaValori(os, doc);

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), apoi curățenia loturilor NĂSCUTE LA CULEGERE ale
    // liniilor care dispar (loturile plusurilor). Ordinea contează: `CurataOrfane`
    // citește `GetObjectsToDelete`, deci trebuie să vadă ștergerile DEJA marcate,
    // dar înaintea commit-ului (loturile intră în același SaveChanges). Loturile
    // FINALIZATE de motor nu se ating niciodată — inclusiv lotul pinuit de o linie
    // de minus, care nici măcar nu e al liniilor de aici.
    //
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
        LoturiCulegereService.CurataOrfane(os);
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
            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca NIR/FCT).
            // Semnul NU se verifică aici: culegerea e pozitivă prin contract, iar
            // cantitatea unei linii deja operate e semnată — `ValideazaOperare`
            // cere doar „≠ 0", și nu inventăm un refuz peste el.
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            detaliu.Cantitate = l.Cantitate;

            if (detaliu.Directie == DirectieDiferenta.Minus) {
                // Câmpurile PLUSULUI se golesc — persistat, nu doar ignorat
                // (F6-D3). Navigația ȘI FK-ul scalar: fixup-ul EF nu are voie să
                // reînvie referința dintr-o navigație încă încărcată.
                detaliu.Produs = null;
                detaliu.ProdusId = null;
                detaliu.PretEvaluare = null;
                detaliu.DataExpirare = null;
                detaliu.LotFabricatie = null;
                // Pinul lotului descărcat — singura direcție pe care `LotId` se
                // aplică (F6-D5).
                if (l.LotId is Guid lotId) {
                    detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
                }
                else {
                    detaliu.Lot = null;
                    detaliu.LotId = null;
                }
            }
            else {
                // Produsul e mecanismul lotului (F6-D2): îl consumă
                // `LoturiCulegereService` după reconciliere.
                if (l.ProdusId is Guid produsId) {
                    detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
                }
                else {
                    detaliu.Produs = null;
                    detaliu.ProdusId = null;
                }
                if (l.PretEvaluare is decimal pret)
                    VerificaScara(pret, Scara.Pret, "Prețul de evaluare");
                detaliu.PretEvaluare = l.PretEvaluare;
                detaliu.DataExpirare = l.DataExpirare;
                detaliu.LotFabricatie = l.LotFabricatie;
                // `LotId`/`Lot` NU se ating: pe plus lotul e server-owned, îl
                // gestionează serviciul de culegere (F6-D5). Valoarea din payload
                // e ecoul ReadDto-ului, nu o intenție a operatorului.
            }

            // Dimensiunea frunzei (DIM-2) + angajamentul de pe bază — pe NAVIGAȚIE,
            // ca restul FK-urilor: existența se validează cu mesaj de domeniu, nu
            // cu violare de FK.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;
            detaliu.Angajament = Nomenclator<Angajament>(os, l.AngajamentId, "Angajamentul");
            if (l.AngajamentId == null) detaliu.AngajamentId = null;
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // Valoarea liniei, materializată LA CULEGERE (GATE 53c: operatorul vede
    // efectul NET al inventarului înainte de operare) — de aceea `Valoare` nu e
    // în WriteDto.
    //
    // Formula e GEAMĂNA lui `ListaDiferenteInventar.PregatesteOperare` (F6-D6),
    // care o rescrie la operare, cu o singură deosebire deliberată: `Cantitate`
    // NU se atinge aici. Semnarea cantității e fapta OPERĂRII (28a) — culegerea o
    // ține pozitivă, ca UI-ul. Valoarea, în schimb, e semnată de pe acum: `Total`-ul
    // draftului trebuie să arate efectul net (minusuri − plusuri), nu suma
    // absolută. De aceea formula folosește `Math.Abs(Cantitate)` explicit: e
    // idempotentă și pe un document re-cules după operare+anulare, unde linia
    // poartă deja cantitatea semnată.
    //
    // Rulează DUPĂ `Sincronizeaza`: abia atunci linia de plus are lotul născut
    // (nu contează pentru formulă — plusul se evaluează la `PretEvaluare` — dar
    // ordinea e aceeași ca pe NIR, și minusul depinde de pinul rămas după gard).
    //
    // Doar FRUNZELE (`OfType`), ca în hook: liniile de tip BAZĂ ale LDI-urilor
    // istorice/importate n-au direcție și n-au de unde lua un preț, iar
    // rescrierea valorii lor ar fi exact clasa de defect a review-ului GATE D1 —
    // pierdere tăcută de dată contabilă reală.
    static void MaterializeazaValori(IObjectSpace os, ListaDiferenteInventar doc) {
        foreach (var d in doc.Detalii.OfType<ListaDiferenteInventarDetaliu>()) {
            // Liniile marcate spre ștergere în acest commit nu se mai ating.
            if (os.IsObjectToDelete(d))
                continue;
            if (d.Directie == DirectieDiferenta.Minus) {
                // Minusul se evaluează la prețul lotului DESCĂRCAT. Fără lot
                // (draft incomplet — operarea îl va refuza) valoarea se golește:
                // valoarea veche, a lotului scos de pe linie, ar minți pe ecran.
                var lot = d.LotId is Guid lotId ? os.GetObjectByKey<Lot>(lotId) : null;
                d.Valoare = lot != null
                    ? Scara.RotunjesteBani(-Math.Abs(d.Cantitate) * lot.PretUnitar)
                    : 0m;
            }
            else if (d.Directie == DirectieDiferenta.Plus) {
                // Plusul se evaluează la prețul CULES (lotul nou se naște cu el;
                // validarea de operare îl cere pozitiv — 28e).
                d.Valoare = Scara.RotunjesteBani(Math.Abs(d.Cantitate) * (d.PretEvaluare ?? 0m));
            }
            // Direcția nesetată nu ajunge aici prin `Aplica` (parse-ul o refuză);
            // dacă totuși există pe un draft vechi, valoarea ei rămâne cum e.
        }
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol)
            where T : class => Rezolva.Optional<T>(os, id, rol);

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

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o listă de diferențe.
    public static LdiReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<ListaDiferenteInventar>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza adusă prin `as`
        // (TPT ⇒ LEFT JOIN în SQL): LDI-urile ISTORICE (importul 1C) poartă linii
        // de tip BAZĂ, iar pe frunză singură ar fi ieșit `Linii: []` cu `Total`
        // nenul (constatarea F5 pe NIR). NULLABLE EXPLICIT pe TOATE valorile
        // frunzei — inclusiv pe `Directie`: pe o linie de bază cast-ul dă null,
        // iar un enum non-nullable ar pica la materializare. Numele membrului se
        // compune în memorie, după materializare.
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

        // Affordance ONESTĂ pe stingeri (F3-D2): LDI nu e creanță și n-ar trebui
        // să poarte imperecheri, dar gardianul motorului
        // (`VerificaFaraImperecheri`) e generic pe `Document` — dacă totuși există
        // un link, refuzul se ARATĂ, nu se descoperă la apăsarea butonului.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new LdiReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
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
