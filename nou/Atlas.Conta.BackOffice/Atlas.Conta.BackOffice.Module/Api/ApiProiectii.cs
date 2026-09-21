using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

// Bucățile de proiecție PARTAJATE între felii — tot ce nu e al unui tip de
// document. Două feluri, cu reguli diferite:
//   * COMPUNERE în memorie (`EtichetaLot`): nu atinge baza, nu atinge navigații;
//     primește câmpuri deja PROIECTATE PLAT și le asamblează;
//   * SUB-PROIECȚII ale unui mecanism de BAZĂ (`Copii` — grupul conex, decizia
//     17): ating baza, dar aparțin bazei, nu unei felii. Trăiesc aici din
//     același motiv pentru care `DocumentCopilDto` a urcat în `ApiDtos`: al
//     doilea exemplar ar diverge tăcut de primul.
internal static class ApiProiectii {
    // 85g — aceeași compunere ca `Lot.Eticheta`, pe câmpuri proiectate plat.
    public static string EtichetaLot(string produs, DateOnly? data, decimal? pretUnitar)
        => data == null ? null : Lot.EtichetaLot(produs, data.Value, pretUnitar ?? 0m);

    // F27-D6 — legătura de corecție a unui document, ca DTO partajat: o singură
    // proiecție pentru toate cele 18 `*Apply`, nu 18 copii ale aceluiași join.
    // `null` = documentul nu corectează nimic (cazul majoritar, o citire pe
    // indexul filtrat).
    public static CorectieDto Corectie(IObjectSpace os, Guid id) {
        var rand = os.GetObjectsQuery<Document>()
            .Where(d => d.ID == id && d.CorecteazaId != null)
            .Select(d => new {
                OriginalId = d.CorecteazaId.Value, d.MotivCorectie,
                d.Corecteaza.Numar, Data = (DateOnly?)d.Corecteaza.Data
            })
            .FirstOrDefault();
        if (rand == null)
            return null;
        return new CorectieDto {
            OriginalId = rand.OriginalId,
            Eticheta = string.IsNullOrWhiteSpace(rand.Numar)
                ? rand.Data?.ToString("dd.MM.yyyy")
                : $"{rand.Numar} din {rand.Data:dd.MM.yyyy}",
            Motiv = rand.MotivCorectie?.ToString()
        };
    }

    // Grupul conex al unui document (0–2 copii: clona conexă + secundarul autogenerat).
    public static List<DocumentCopilDto> Copii(IObjectSpace os, Guid id) {
        var randuri = os.GetObjectsQuery<Document>()
            .Where(d => d.DocumentSursaId == id)
            .OrderBy(d => d.Data).ThenBy(d => d.ID)
            .Select(d => new { d.ID, d.Numar, d.Stare, d.Autogenerat })
            .ToList();
        var tipuri = CoduriTip(os, randuri.Select(r => r.ID).ToList());
        return randuri.Select(r => new DocumentCopilDto {
            Id = r.ID,
            Tip = tipuri.TryGetValue(r.ID, out var cod) ? cod : null,
            Numar = r.Numar,
            Stare = r.Stare.ToString(),
            Autogenerat = r.Autogenerat
        }).ToList();
    }

    public static Dictionary<Guid, string> CoduriTip(IObjectSpace os, IReadOnlyCollection<Guid> ids) =>
        CititorTipDocument.Coduri(os, ids);

    // Codul de tip al UNUI document (documentul-sursă din „Generat din").
    // Clientul rutează prin `rutaTip` (vocabular închis) — un tip fără felie
    // rămâne text, dar link-ul nu mai e hardcodat pe `/fct/` (D-6b).
    public static string CodTip(IObjectSpace os, Guid? documentId) =>
        documentId == null
            ? null
            : CoduriTip(os, new[] { documentId.Value }).GetValueOrDefault(documentId.Value);

    // Affordance ONESTĂ pe stingeri (F3-D2): oglinda API a gardianului
    // `MotorOperare.VerificaFaraImperecheri` — anularea și stornarea se refuză
    // cât timp documentul poartă un link pe ORICARE rol (31d). Trăiește aici, nu
    // în serviciu, fiindcă e o CITIRE de affordance (ca `Copii`), nu un
    // invariant: invarianții stingerii rămân în `ImperechereService`. CUSĂTURĂ:
    // predicatul e identic cu al gardianului — dacă acolo se schimbă (alt rol,
    // alt filtru), affordance-ul de aici minte până se schimbă la fel.
    public static bool AreImperecheri(IObjectSpace os, Guid documentId) =>
        os.GetObjectsQuery<Imperechere>()
            .Any(i => i.DocumentStingatorId == documentId || i.DocumentId == documentId);
}
