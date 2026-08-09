using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
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
    // Oglinda lui `Lot.Eticheta` (care e [NotMapped], deci inaccesibil în SQL):
    // aceleași reguli, compuse după materializarea câmpurilor plate. Orice
    // schimbare acolo se reflectă aici — cusătura e documentată în ambele.
    // O SINGURĂ copie pentru toate feliile (BTR/FCT/NIR): eticheta lotului apare
    // pe orice linie care referă un lot, iar un al doilea adevăr de afișare ar
    // diverge tăcut de model.
    public static string EtichetaLot(string produs, DateOnly? data, decimal? pretUnitar) {
        if (data == null)
            return null;
        var denumire = produs ?? "(produs nedefinit)";
        var pret = pretUnitar ?? 0m;
        return data == default(DateOnly) && pret == 0m
            ? $"{denumire} (în culegere)"
            : $"{denumire} · {data:dd.MM.yyyy} · {pret:0.####}";
    }

    // Grupul conex al unui document. Coloanele plate vin dintr-o proiecție;
    // CODUL TIPULUI nu poate veni din SQL — sub TPT nu există discriminator, iar
    // ancora `TipDocument` se găsește după NUMELE CLASEI CLR
    // (`MotorOperare.GasesteTipDocument`), care nu e o coloană. Alternativa
    // traductibilă (`d is NIR ? "NIR" : …`) ar îngheța lista tipurilor în cod,
    // exact ce evită ancora. Rezolvarea se face deci în memorie, pe o mulțime
    // MĂRGINITĂ prin construcție: grupul conex al unui document are 0–2 copii
    // (clona conexă + secundarul autogenerat).
    public static List<DocumentCopilDto> Copii(IObjectSpace os, Guid id) {
        var randuri = os.GetObjectsQuery<Document>()
            .Where(d => d.DocumentSursaId == id)
            .OrderBy(d => d.Data).ThenBy(d => d.ID)
            .Select(d => new { d.ID, d.Numar, d.Stare, d.Autogenerat })
            .ToList();
        return randuri.Select(r => new DocumentCopilDto {
            Id = r.ID,
            Tip = TipCopil(os, r.ID),
            Numar = r.Numar,
            Stare = r.Stare.ToString(),
            Autogenerat = r.Autogenerat
        }).ToList();
    }

    static string TipCopil(IObjectSpace os, Guid id) {
        var copil = os.GetObjectByKey<Document>(id);
        if (copil == null)
            return null;
        try {
            return MotorOperare.GasesteTipDocument(os, copil).Cod;
        }
        catch (OperareException) {
            // Ancoră de seed lipsă: nu e motiv să pice CITIREA documentului.
            return copil.GetType().Name;
        }
    }
}
