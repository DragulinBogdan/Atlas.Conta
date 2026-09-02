using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Fcl;

// Nucleul feliei FCL: reconcilierea agregatului (scriere) și proiecțiile plate
// (citire). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe FCL, față de FCT (F4-D1) ═══
// Pe tierul API nu rulează NICIUN ViewController, deci seam-urile de culegere se
// apelează EXPLICIT, în ordinea din UI:
//   1. maparea câmpurilor culese (inclusiv PINUL de lot — cules pe FCL, spre
//      deosebire de FCT unde lotul e server-owned);
//   2. `TvaService.AplicaTipTvaImplicit` — doar pe liniile NOI fără TipTva în
//      payload (culegerea explicită, inclusiv golirea deliberată, bate default-ul);
//   3. `TvaService.CalculeazaLaCulegere` — scrie `Valoare`/`ValoareTva` din
//      `PretUnitar × Cantitate` (GATE 53c), apoi override-ul manual, dacă vine.
// Al patrulea seam al FCT — `LoturiCulegereService` — LIPSEȘTE aici prin
// contract: FCL nu naște loturi, doar le referă. De aceea `Sterge` nu are nici
// `CurataOrfane`: n-are ce lăsa orfan.
//
// Ce NU face: nu generează descărcarea. DSC-ul se naște exclusiv prin
// `DescarcareService` (hook-ul `GenereazaSecundar` la operare + comanda manuală
// de la pasul 2 al feliei) — Apply e strict culegere.
public static class FacturaIesireApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, FacturaIesireWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        FacturaIesire doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<FacturaIesire>(os, existentId, "Factura de ieșire");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<FacturaIesire>();
        }

        // `Numar` NU se atinge: FCL are PoliticaNumerotare (serie fiscală) ⇒
        // server-owned (F4-D1) — nici nu e în WriteDto, nici gardianul de
        // Committing nu l-ar accepta pe o cale secured.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR/FCT/TRZ): rezolvarea validează
        // existența cu mesaj de domeniu, regulile XAF de culegere stau pe
        // navigație, iar pe o entitate urmărită navigația încărcată ar rescrie
        // la fixup un FK setat direct. TIPUL laturii (intern → Partener) NU se
        // verifică aici: e invariant al OPERĂRII (`FacturaIesire.ValideazaOperare`)
        // — un draft are voie să fie incomplet/greșit până la operare.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (emitentul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (clientul)");
        // Necules ⇒ politica aplică default-ul (+30) la operare (decizia 30c).
        doc.DataScadenta = dto.DataScadenta;

        // Gestiunea de descărcare (P2 §4): obligatorie doar când există linii de
        // stoc — cerința e a operării, aici se validează doar existența.
        if (dto.GestiuneDescarcareId is Guid gestiuneId) {
            doc.GestiuneDescarcare = Rezolva.Cere<Gestiune>(os, gestiuneId, "Gestiunea de descărcare");
        }
        else {
            doc.GestiuneDescarcare = null;
            doc.GestiuneDescarcareId = null;
        }

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<FacturaIesireLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa). Fără curățenie de loturi: FCL nu naște loturi,
    // le REFERĂ (pinul) — un lot referit de o linie ștearsă rămâne al lui.
    // Fără gardian de imperecheri: un link cere ambele documente OPERATE (31d),
    // deci un draft nu poate avea niciunul.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<FacturaIesire>(os, id, "Factura de ieșire");
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
    static void ReconciliazaLinii(IObjectSpace os, FacturaIesire doc,
        List<FacturaIesireLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar liniile de tip FCL:
        // un draft vechi poate purta o linie de BAZĂ (refuzată oricum la operare
        // — „detaliu generic", review P2 defect 7), iar payload-ul e adevărul
        // agregatului, deci reconcilierea o curăță în loc s-o lase invizibilă.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();
        // Latura fiscală a tipului (F13-D1) — pe FCL e `Colectat`, deci liniile
        // cu regim de taxare inversă rămân cu `ValoareTva = 0` încă de la
        // culegere; rezolvată o dată pentru tot agregatul.
        var directieTva = TvaService.DirectiePentru(os, doc);

        foreach (var l in linii) {
            FacturaIesireDetaliu detaliu;
            var noua = false;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as FacturaIesireDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de factură de ieșire (tip vechi) — ștergeți-o "
                        + "din document și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<FacturaIesireDetaliu>();
                detaliu.Document = doc;
                noua = true;
            }

            // Starea DE DINAINTEA mapării, pentru semantica recalculului de mai
            // jos: în UI recalculul TVA se declanșează DOAR la schimbarea bazei
            // (Cantitate/PretUnitar) sau a TipTva — un Save care nu le atinge NU
            // pierde override-ul de ValoareTva.
            var bazaVeche = noua ? 0m : detaliu.PretUnitar * detaliu.Cantitate;
            Guid? tipTvaVechi = noua ? null : detaliu.TipTvaId;

            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");

            // „General!" — identitatea liniei de stoc (P2 §4). Obligativitatea pe
            // liniile de stoc e a OPERĂRII; aici doar existența.
            if (l.ProdusId is Guid produsId) {
                detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }

            // „Specific?" — PINUL de lot, cules (spre deosebire de FCT). Că
            // lotul aparține produsului și că are sold în gestiunea de descărcare
            // se verifică la OPERARE: pinul e intenția magazinului, iar un draft
            // are voie s-o poarte înainte ca stocul să existe.
            if (l.LotId is Guid lotId) {
                detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
            }
            else {
                detaliu.Lot = null;
                detaliu.LotId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca BTR/FCT).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            VerificaScara(l.PretUnitar, Scara.Pret, "Prețul unitar");
            detaliu.Cantitate = l.Cantitate;
            detaliu.PretUnitar = l.PretUnitar;
            detaliu.Descriere = l.Descriere;

            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = Rezolva.Cere<TipTva>(os, tipTvaId, "Tipul de TVA");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            // Dimensiunea frunzei (DIM-2) — pe NAVIGAȚIE, ca restul FK-urilor:
            // existența se validează cu mesaj de domeniu, nu cu violare de FK.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;

            // Default-ul de TipTva al tipului de document (37f) — DOAR pe liniile
            // noi al căror payload n-a dat un TipTva: pe o linie existentă,
            // golirea lui e decizie explicită a operatorului, iar re-aplicarea
            // default-ului ar face-o imposibilă.
            if (noua && l.TipTvaId == null)
                TvaService.AplicaTipTvaImplicit(os, doc, detaliu);

            // Lanțul de valori, materializat LA CULEGERE (GATE 53c): operatorul
            // confruntă hârtia înainte de operare. `PregatesteOperare` îl rescrie
            // la operare din aceeași formulă — de aceea `Valoare` nu e în WriteDto.
            // Recalculul e CONDIȚIONAT de declanșatorii din UI (baza sau TipTva
            // schimbate) — altfel un PUT care editează doar header-ul ar pierde
            // tăcut override-ul de ValoareTva salvat anterior (clientul nu poate
            // distinge „valoarea citită e override" de „e calculată", deci nu o
            // retrimite).
            var bazaNoua = detaliu.PretUnitar * detaliu.Cantitate;
            if (noua || bazaNoua != bazaVeche || detaliu.TipTvaId != tipTvaVechi)
                TvaService.CalculeazaLaCulegere(os, directieTva, detaliu, bazaNoua);
            // Override-ul operatorului, DUPĂ calcul (oglinda fluxului UI): pe
            // factura EMISĂ rotunjirea aparține documentului (e-Factura, agregarea
            // retailului), nu recalculului nostru — regula 36a, uniformizată prin
            // decizia 48b. La operare îl păstrează `pastreazaTvaCules: true`.
            if (l.ValoareTva is decimal valoareTva) {
                VerificaScara(valoareTva, Scara.Bani, "Valoarea TVA");
                // Ca la FCT (review F2-D1/D7): override-ul are sens DOAR pe
                // regimurile care postează TVA separat. Pe Capitalizat TVA-ul e
                // deja în `Valoare` (l-ar număra de două ori în Total); pe
                // Scutit/Neimpozabil/fără TipTva, `PregatesteOperare` l-ar șterge
                // oricum la operare — acceptarea lui ar minți operatorul.
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
                // F13-D1 (review, defect 1): pe LIVRARE taxarea inversă nu poartă TVA —
                // motorul ar refuza oricum la operare, dar un draft salvat cu 63 lei de
                // TVA pe o linie TI ar minți în ReadDto (`Total` = net + TVA) până atunci.
                // Aceeași propoziție ca în motor, aici la PUT, cât operatorul e pe formular.
                if (valoareTva != 0 && regim == RegimTva.TaxareInversa && directieTva == DirectieTva.Colectat)
                    throw new OperareException(
                        "Taxarea inversă pe livrare nu poartă TVA; linia are TVA "
                        + $"{valoareTva:N2} — lăsați valoarea goală.");
                detaliu.ValoareTva = valoareTva;
            }
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol)
            where T : class => Rezolva.Optional<T>(os, id, rol);

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

    // Pe FCL numărul lipsește până la operare (serie fiscală, GATE XAF D6) —
    // eticheta cade pe dată, ca la trezorerie.
    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════ Backorder: descărcarea de gestiune ═══════════════════
    //
    // Calea MANUALĂ a descărcării (F4-D3): la operarea facturii, DSC-ul conex se
    // generează automat (`FacturaIesire.GenereazaSecundar` → `DescarcareService`,
    // în tranzacția operării), dar pozițiile fără stoc la acea dată rămân
    // nedescărcate. Comanda de aici re-rulează generatorul pentru RESTUL
    // neacoperit — geamănul acțiunii XAF „Generează descărcarea"
    // (`FacturaIesireDescarcareController`), pe aceeași cale de motor.
    //
    // CONTRACT DE APELANT (42b): rulează pe un ObjectSpace NON-SECURED, propriu
    // comenzii. `DescarcareService` scrie `Autogenerat` și `DocumentSursa` — două
    // câmpuri SERVER-OWNED pe care `GardianEditare` le refuză pe orice cale
    // secured, deci ușa secured nu e o opțiune. Gate-ul de autorizare („cine
    // comandă") rămâne al controllerului, ÎNAINTEA acestui apel (spike D-F1:
    // secured/non-secured răspunde la „cum scrie motorul", nu la „cine comandă").
    // Comite: draftul generat trebuie să existe ca să poată fi deschis și operat.
    public static GenerareDescarcareRezultatDto GenereazaDescarcare(IObjectSpace os, Guid id, DateOnly data) {
        var fcl = Rezolva.Cere<FacturaIesire>(os, id, "Factura de ieșire");
        // Refuz de DOMENIU: pe un draft nu există încă acoperire de generat
        // (oglinda lui `ActualizeazaDisponibilitatea` din controllerul XAF), iar
        // pe o factură stornată descărcarea n-ar avea ce acoperi.
        if (fcl.Stare != StareDocument.Operat)
            throw new OperareException(
                $"Descărcarea de gestiune se generează doar pentru o factură OPERATĂ — "
                + $"{Eticheta(fcl)} e în starea „{fcl.Stare}”.");

        var dsc = DescarcareService.Genereaza(os, fcl, data);
        if (dsc != null)
            os.CommitChanges();

        // Restul se recalculează DUPĂ commit: draftul abia generat CONTEAZĂ la
        // acoperire (design §5, anti-dublare), deci ce rămâne aici e exact ce mai
        // așteaptă marfă.
        return new GenerareDescarcareRezultatDto {
            DscId = dsc?.ID,
            Resturi = Resturi(os, fcl).Where(r => r.Rest > 0).ToList()
        };
    }

    // Proiecția de acoperire per linie de stoc (F4-D4), tradusă din
    // `DescarcareService.RestNedescarcat` (care întoarce tupluri cu ID-uri).
    // Întoarce TOATE liniile de stoc, inclusiv cele acoperite integral: tabelul
    // din client arată starea acoperirii, nu doar lipsa. Comanda de generare
    // filtrează ea `Rest > 0` — acolo întrebarea e „ce mai așteaptă".
    public static IReadOnlyList<RestNedescarcatRandDto> RestNedescarcat(IObjectSpace os, Guid id) {
        var fcl = Rezolva.Cere<FacturaIesire>(os, id, "Factura de ieșire");
        return Resturi(os, fcl);
    }

    // Denumirile produselor într-un SINGUR query pe mulțimea implicată (fără
    // N+1): proiecția serviciului dă doar ID-uri, iar rândurile sunt mărginite
    // de numărul de linii de stoc ale facturii.
    static List<RestNedescarcatRandDto> Resturi(IObjectSpace os, FacturaIesire fcl) {
        var randuri = DescarcareService.RestNedescarcat(os, fcl);
        if (randuri.Count == 0)
            return new List<RestNedescarcatRandDto>();

        var idsProdus = randuri.Where(r => r.ProdusId != null)
            .Select(r => r.ProdusId.Value).Distinct().ToList();
        var denumiri = idsProdus.Count == 0
            ? new Dictionary<Guid, string>()
            : os.GetObjectsQuery<Produs>()
                .Where(p => idsProdus.Contains(p.ID))
                .Select(p => new { p.ID, p.Denumire })
                .ToDictionary(x => x.ID, x => x.Denumire);

        return randuri.Select(r => new RestNedescarcatRandDto {
            LinieId = r.LinieId,
            ProdusId = r.ProdusId,
            ProdusDenumire = r.ProdusId != null ? denumiri.GetValueOrDefault(r.ProdusId.Value) : null,
            LotId = r.LotId,
            Cantitate = r.Cantitate,
            Acoperit = r.Acoperit,
            Rest = r.RestNeacoperit
        }).ToList();
    }

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o factură de ieșire.
    public static FacturaIesireReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<FacturaIesire>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                // TPT: cast-ul devine LEFT JOIN pe tabela `Partener` — null pe
                // orice alt tip de repartitor.
                PrimitorCodFiscal = (d.Primitor as Partener).CodFiscal,
                d.DataScadenta,
                d.GestiuneDescarcareId,
                GestiuneDescarcareDenumire = d.GestiuneDescarcare.Denumire,
                d.Autogenerat, d.DocumentSursaId
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Liniile se citesc pe tipul DERIVAT (ca la FCT): identitatea liniei de
        // FCL e frunza (PretUnitar/Produs/CodEconomic), iar o linie de bază pe un
        // draft vechi e refuzată la operare — o vede prima reconciliere, care o
        // curăță. Contează și pentru scriere: un Id de linie de bază întors la
        // citire ar fi refuzat la următorul PUT („tip vechi").
        var linii = os.GetObjectsQuery<FacturaIesireDetaliu>()
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
                l.Descriere, l.Cantitate, l.PretUnitar, l.Valoare, l.ValoareTva,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod, TipTvaDenumire = l.TipTva.Denumire,
                TipTvaCota = (decimal?)l.TipTva.Cota,
                l.CodEconomicId, CodEconomicCod = l.CodEconomic.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului, nu pe liniile proiectate mai sus:
        // e definiția modelului (`Document.Total`) și trebuie să dea EXACT ce dă
        // `Lista` — un draft vechi cu o linie de tip bază ar fi făcut cele două
        // cifre să difere tăcut, iar operatorul le vede una lângă alta.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordance ONESTĂ (F2-D5 + F3-D2): gardianul de grup refuză anularea/
        // stornarea cât timp există un copil OPERAT (aici: o descărcare operată),
        // iar `VerificaFaraImperecheri` cât timp factura e stinsă de o încasare.
        // Copiii sunt oricum calculați pentru DTO — consecința se arată, nu se
        // descoperă la refuz.
        var copii = ApiProiectii.Copii(os, id);
        var faraCopiiOperati = copii.All(c => c.Stare != nameof(StareDocument.Operat));
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        // Backorder (F4-D4): affordance ONESTĂ pe comanda de generare — aceleași
        // trei condiții pe care le-ar întâlni comanda (Operat, gestiune aleasă,
        // rest > 0). Ordinea contează: cele două verificări ieftine
        // scurt-circuitează, deci proiecția de acoperire (care încarcă entitatea
        // și enumerează liniile) NU se plătește pe drafturi și nici pe facturile
        // de servicii — exact cazul majoritar.
        var poateGeneraDescarcare = h.Stare == StareDocument.Operat
            && h.GestiuneDescarcareId != null
            && RestNedescarcat(os, id).Any(r => r.Rest > 0);

        return new FacturaIesireReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            PrimitorCodFiscal = h.PrimitorCodFiscal,
            DataScadenta = h.DataScadenta,
            GestiuneDescarcareId = h.GestiuneDescarcareId,
            GestiuneDescarcareDenumire = h.GestiuneDescarcareDenumire,
            Total = total,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            PoateGeneraDescarcare = poateGeneraDescarcare,
            Copii = copii,
            Linii = linii.Select(l => new FacturaIesireLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Descriere = l.Descriere,
                Cantitate = l.Cantitate, PretUnitar = l.PretUnitar,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                TipTvaDenumire = l.TipTvaDenumire, TipTvaCota = l.TipTvaCota,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului și abia apoi materializează (43c).
    //
    // `Total` prin JOIN PE AGREGAT, nu subquery corelat (42c), și BRUT
    // (Σ Valoare + ValoareTva) — ca `Document.Total`. Agregatul se face pe BAZA
    // detaliului: o linie de tip vechi contribuie la total, ca în model.
    public static IQueryable<FacturaIesireListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<FacturaIesire>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new FacturaIesireListDto {
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
