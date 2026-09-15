using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Implicite;

// Proiecția feliei 23 (F23-D6): ambalarea rezultatului lui `ImpliciteService`
// într-un DTO. Stă în Module, ca toate Apply-urile (55b), din două motive:
// e testabilă din ModelCheck fără host HTTP, și ține controllerul SUBȚIRE —
// acolo rămâne doar transportul (uși, gate-uri, coduri de stare).
//
// Nicio regulă proprie aici: rezolvarea e a serviciului, o singură dată, pentru
// toți cei trei apelanți (controllerul XAF de linie nouă, cele cinci Apply-uri
// prin wrapper-ul din `TvaService`, endpoint-ul ăsta).
public static class ImpliciteApply {
    /// <summary>
    /// Ancora `TipDocument` după COD (FCT, FCL…). Vocabularul de pe sârmă e codul
    /// ancorei, nu numele clasei CLR (decizia 6 — moștenirea e detaliu de
    /// persistență) și nici `Guid`-ul ei, pe care clientul n-are de unde să-l
    /// știe la deschiderea unei linii.
    ///
    /// Ușa o alege APELANTUL, și nu e indiferentă: pe una securizată `null` ar fi
    /// însemnat deopotrivă „codul nu există" și „ancora nu-ți e vizibilă", iar
    /// 400-ul controllerului ar fi mințit despre cauză pentru al doilea caz
    /// (măsurat pe `User`, care nu vede niciun rând din `TipDocument`). De aceea
    /// endpoint-ul îl cheamă pe ușa NON-SECURED — codul e vocabular de rutare,
    /// pe care clientul tocmai l-a trimis — și păstrează REZOLVAREA securizată.
    ///
    /// Potrivirea e case-insensitive în MEMORIE: ancora are ~20 de rânduri, deci
    /// citirea listei e mai ieftină decât un `ToUpper` tradus în SQL, iar
    /// comparația nu depinde de colația bazei (77a: colațiile ne-deterministe
    /// au deja rupt `LIKE` o dată).
    /// </summary>
    public static Guid? IdTipDocument(IObjectSpace os, string cod) {
        if (string.IsNullOrWhiteSpace(cod))
            return null;
        var cautat = cod.Trim();
        return os.GetObjectsQuery<TipDocument>()
            .Select(t => new { t.ID, t.Cod })
            .ToList()
            .Where(t => string.Equals(t.Cod, cautat, StringComparison.OrdinalIgnoreCase))
            .Select(t => (Guid?)t.ID)
            .FirstOrDefault();
    }

    /// <summary>
    /// Implicitul de TVA pentru contextul cerut. `os` e ușa SECURIZATĂ: implicitul
    /// se propune peste nomenclatoarele pe care utilizatorul le VEDE, iar un
    /// partener invizibil cade pe ramura „fără partener" a serviciului — același
    /// text ca pentru unul inexistent (80a, fără oracol de existență).
    /// </summary>
    public static RezultatImplicitDto TipTva(IObjectSpace os, Guid tipDocumentId,
            Guid? partenerId, Guid? produsId, DateOnly data) {
        var rezultat = ImpliciteService.TipTva(os, tipDocumentId, partenerId, produsId, data);
        var dto = new RezultatImplicitDto {
            TipTvaId = rezultat.TipTvaId,
            // Numele enum-ului, nu valoarea numerică (57a): un `int` pe sârmă ar
            // fi obligat clientul să țină a doua copie a enum-ului.
            Sursa = rezultat.Sursa.ToString(),
            Motiv = rezultat.Motiv,
        };
        if (rezultat.TipTvaId is not Guid id)
            return dto;
        // Eticheta vine din ACELAȘI răspuns (vezi comentariul DTO-ului), citită
        // proiectat pe ușa dată — dacă tipul propus nu i-ar fi vizibil celui care
        // întreabă, rămân doar id-ul și motivul, nu o etichetă inventată.
        var tip = os.GetObjectsQuery<TipTva>()
            .Where(t => t.ID == id)
            .Select(t => new { t.Cod, t.Denumire, t.Cota })
            .FirstOrDefault();
        if (tip == null)
            return dto;
        dto.TipTvaCod = tip.Cod;
        dto.TipTvaDenumire = tip.Denumire;
        dto.Cota = tip.Cota;
        return dto;
    }
}
