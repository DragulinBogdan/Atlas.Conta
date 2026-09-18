using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Trz;

// Adaptorul de graniță al STINGERII (F3-D3). ZERO ASP.NET aici — controllerul
// `api/imperecheri` din host e transport subțire peste cele trei metode, iar
// ModelCheck exersează exact acest cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: rulează în ObjectSpace-ul SECURED al apelantului și
// COMITE. Autorizarea = securitatea XAF + gardianul de Committing
// (`GardianEditare.VerificaImperechere`), care re-rulează `ValideazaCreare` pe
// obiectul nou. Dubla rulare (aici, prin serviciu, și acolo, la commit) e
// BENIGNĂ: validarea e pură (citește, nu scrie) și e aceeași funcție — a doua
// trecere e plasa care prinde și scrierile care NU vin pe calea asta.
public static class ImperechereApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // ABATERE DOCUMENTATĂ de la 42b („entitățile nu traversează granița, cheile
    // da"): `ImperechereService.Imperecheaza` cere ENTITĂȚI, fiindcă invarianții
    // lui sunt polimorfi (`CapacitateStingere`, `LiniiCreanta`) — pe id-uri ar fi
    // trebuit să reconstruiască tipul, adică exact ce face `GetObjectByKey`.
    // Traducerea cheie → entitate se face deci AICI, la graniță, o dată, cu
    // mesaje de DOMENIU pentru id-urile inexistente (altfel serviciul ar arunca
    // pe null-guard cu un mesaj despre „ambele sunt obligatorii", care într-un
    // API înseamnă altceva).
    public static ImperechereReadDto Creeaza(IObjectSpace os, ImperechereWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");
        // Scara ÎNAINTE de serviciu: `Creeaza` din motor rotunjește tăcut suma
        // la bani (ca plafonul validat să fie cel persistat), ceea ce pe calea
        // internă e corect — dar pe sârmă o sumă cu trei zecimale e o eroare de
        // CLIENT și merită refuz explicit, ca la BTR/FCT.
        VerificaScara(dto.Suma, Scara.Bani, "Suma imperecheată");

        var stingator = Rezolva.Cere<Document>(os, dto.DocumentStingatorId, "Documentul care stinge");
        var document = Rezolva.Cere<Document>(os, dto.DocumentId, "Documentul stins");

        // Restul invarianților (ambele operate, sensuri opuse, contrapartidă
        // comună, plafoane) rămân EXCLUSIV în serviciu — un al doilea exemplar
        // aici ar diverge tăcut.
        var imperechere = ImperechereService.Imperecheaza(os, stingator, document, dto.Suma,
            dto.ContrapartidaId, Zi(dto.Data));
        return Din(imperechere);
    }

    // F27-D8: desfacerea unei imperecheri dintr-o perioadă închisă. Comandă de
    // MOTOR (scrie un rând pe care ușa securizată îl refuză), deci rulează pe
    // ObjectSpace-ul non-secured al apelantului; dreptul l-a verificat ruta.
    public static ImperechereReadDto Desfa(IObjectSpace os, Guid imperechereId,
            DesfaImperechereRequestDto cerere) =>
        Din(ImperechereService.Desfa(os, imperechereId, Zi(cerere?.Data)));

    static DateOnly Zi(DateOnly? data) =>
        data is DateOnly d && d != default ? d : DateOnly.FromDateTime(DateTime.Today);

    static ImperechereReadDto Din(Imperechere imperechere) => new() {
        Id = imperechere.ID,
        DocumentStingatorId = imperechere.DocumentStingatorId,
        DocumentId = imperechere.DocumentId,
        Suma = imperechere.Suma,
        Data = imperechere.Data,
        InverseazaId = imperechere.InverseazaId,
        Autogenerat = imperechere.Autogenerat
    };

    // Ștergerea e LIBERĂ (31d): legătura n-are registre proprii, iar dispariția
    // ei doar eliberează restul celor două documente (și deblochează
    // anularea/stornarea lor — vezi `ApiProiectii.AreImperecheri`).
    public static void Sterge(IObjectSpace os, Guid imperechereId) {
        var imperechere = Rezolva.Cere<Imperechere>(os, imperechereId, "Imperecherea");
        os.Delete(imperechere);
        os.CommitChanges();
    }

    // ═══════════════════════ Citire ═══════════════════════

    // Panoul de stingeri al unui document: numerele + rândurile, într-un singur
    // apel (F3-D3). `null` dacă documentul nu există / nu e vizibil (F22-D1).
    public static StingeriDto Stingeri(IObjectSpace os, Guid documentId) {
        var doc = os.GetObjectByKey<Document>(documentId);
        if (doc == null)
            return null;

        // Proiecție PLATĂ, cu rolul rezolvat ÎN SQL (`CASE`): un singur query
        // acoperă ambele laturi, iar `Celalalt*` iese direct cu numărul părții
        // opuse (LEFT JOIN pe navigație, nu enumerare lazy — 25b/41c).
        var randuri = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == documentId || i.DocumentId == documentId)
            // CRONOLOGIC după data CELEILALTE părți (F3 §Închidere: ordinea pe Id
            // era arbitrară — Guid): operatorul citește panoul ca istoric al
            // stingerilor. Id-ul rămâne tiebreak stabil pentru două din aceeași zi.
            .OrderBy(i => i.DocumentStingatorId == documentId ? i.Document.Data : i.DocumentStingator.Data)
            .ThenBy(i => i.ID)
            .Select(i => new {
                i.ID, i.Suma, i.Autogenerat, i.Data, i.InverseazaId,
                EsteStingator = i.DocumentStingatorId == documentId,
                CelalaltId = i.DocumentStingatorId == documentId ? i.DocumentId : i.DocumentStingatorId,
                CelalaltNumar = i.DocumentStingatorId == documentId
                    ? i.Document.Numar : i.DocumentStingator.Numar
            })
            .ToList();
        // Ce rând e deja desfăcut: o singură interogare pe mulțimea MĂRGINITĂ a
        // stingerilor documentului, nu un predicat per rând.
        var ids = randuri.Select(r => r.ID).ToList();
        var desfacute = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.InverseazaId != null && ids.Contains(i.InverseazaId.Value))
            .Select(i => i.InverseazaId.Value)
            .ToList()
            .ToHashSet();
        // Perioada e a BAZEI, nu a clientului (42c): panoul primește verdictul,
        // nu regula. O singură rezolvare per lună atinsă.
        var deschise = randuri.Select(r => (r.Data.Year, r.Data.Month)).Distinct()
            .ToDictionary(x => x, x => PerioadaDeschisa(os, new DateOnly(x.Year, x.Month, 1)));

        var tipuri = ApiProiectii.CoduriTip(os, randuri.Select(r => r.CelalaltId).ToList());

        return new StingeriDto {
            DocumentId = documentId,
            // Afordanța de SENS a panoului (F19-D16, review F3): sensul pe care
            // trebuie să-l poarte candidații, calculat AICI din hook-ul polimorf
            // — clientul îl pasează pe ruta proiecției, nu îl deduce. Vezi
            // `StingeriDto.SensCandidati` pentru de ce e `Opus`.
            SensCandidati = doc.SensDeStins(os)?.Opus().ToString(),
            // SURSA DE ADEVĂR = serviciul, nu o a doua agregare aici: `Total`
            // trece prin `LiniiCreanta` (ReturClient), iar `Asignat` numără
            // AMBELE coloane. Trei apeluri, deci patru interogări mărginite —
            // e o citire de detaliu, nu de listă.
            Total = ImperechereService.Total(os, documentId),
            Asignat = ImperechereService.Asignat(os, documentId),
            Ramas = ImperechereService.Ramas(os, documentId),
            Imperecheri = randuri.Select(r => new StingereRandDto {
                Id = r.ID,
                EsteStingator = r.EsteStingator,
                CelalaltDocumentId = r.CelalaltId,
                Data = r.Data,
                InverseazaId = r.InverseazaId,
                Desfacuta = desfacute.Contains(r.ID),
                PerioadaDeschisa = deschise[(r.Data.Year, r.Data.Month)],
                CelalaltTip = tipuri.TryGetValue(r.CelalaltId, out var cod) ? cod : null,
                CelalaltNumar = r.CelalaltNumar,
                Suma = r.Suma,
                Autogenerat = r.Autogenerat
            }).ToList()
        };
    }

    static bool PerioadaDeschisa(IObjectSpace os, DateOnly data) {
        try {
            GardianPerioada.VerificaDeschisa(os, data);
            return true;
        }
        catch (OperareException) {
            return false;
        }
    }

    // Gardul de scară — al treilea exemplar (BTR/FCT + trezorerie): `numeric(18,s)`
    // ⇒ cel mult `s` zecimale și `18 − s` cifre întregi (49e). CUSĂTURĂ: cele trei
    // copii sunt identice; se unifică atunci când o a patra felie ar cere-o.
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
}
