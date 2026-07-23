using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Stingerea (decizia 17): m2m plată↔document cu sume parțiale. Invarianții
// trăiesc aici — aceeași cale pentru UI, harness și viitorul Web API, ca la
// MotorOperare. `ramas` e calcul (Total − Σ imperecheri), nu coloană.
//
// Un document poate apărea în AMBELE roluri (Plata de avans ↔ Incasarea de
// regularizare: încasarea stă pe latura de document), deci suma asignată se
// numără pe ambele coloane.
public static class ImperechereService {
    // Totalul documentului, din liniile PERSISTATE (nu navigația Detalii —
    // apelanții nu garantează lazy loading, iar imperecherea se face pe
    // documente deja operate, deci comise). BRUT (P1, design §3): plata stinge
    // Valoare + ValoareTva; la regimurile capitalizate ValoareTva e 0.
    public static decimal Total(IObjectSpace os, Guid documentId) =>
        os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => d.DocumentId == documentId)
            .Select(d => (decimal?)(d.Valoare + d.ValoareTva)).Sum() ?? 0m;

    public static decimal Asignat(IObjectSpace os, Guid documentId) =>
        os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentTrezorerieId == documentId || i.DocumentId == documentId)
            .Select(i => (decimal?)i.Suma).Sum() ?? 0m;

    public static decimal Ramas(IObjectSpace os, Guid documentId) =>
        Total(os, documentId) - Asignat(os, documentId);

    // Tranzacția publică (UI / Web API): validează, creează, comite.
    public static Imperechere Imperecheaza(IObjectSpace os,
        DocumentTrezorerie trezorerie, Document document, decimal suma) {
        var imperechere = Creeaza(os, trezorerie, document, suma, autogenerat: false);
        os.CommitChanges();
        return imperechere;
    }

    // Fără commit — motorul o cheamă din tranzacția operării plății autogenerate.
    internal static Imperechere Creeaza(IObjectSpace os,
        DocumentTrezorerie trezorerie, Document document, decimal suma, bool autogenerat) {
        // Validarea rulează ÎNAINTE de CreateObject — comportamentul existent
        // (motorul nu lasă rând-fantomă pe eșec) rămâne exact.
        ValideazaCreare(os, trezorerie, document, suma);
        var imperechere = os.CreateObject<Imperechere>();
        imperechere.DocumentTrezorerie = trezorerie;
        imperechere.Document = document;
        imperechere.Suma = suma;
        imperechere.Autogenerat = autogenerat;
        return imperechere;
    }

    // Invarianții stingerii, extrași ca să fie refolosibili de gardianul UI
    // (ImperechereController: New generic e permis, dar validat la commit —
    // decizia 31d). Aruncă UserFriendlyException (OperareException) cu mesaj de
    // business. Aceleași verificări ca înainte; în plus null-guard pe navigații
    // (culegerea prin UI le poate lăsa goale — motorul le trimite mereu setate).
    internal static void ValideazaCreare(IObjectSpace os,
        DocumentTrezorerie trezorerie, Document document, decimal suma) {
        if (trezorerie == null || document == null)
            throw new OperareException(
                "Imperecherea leagă o plată/încasare de un document — ambele sunt obligatorii.");
        if (trezorerie.Stare != StareDocument.Operat || document.Stare != StareDocument.Operat)
            throw new OperareException("Imperecherea leagă două documente operate (registrele lor există).");
        if (document.ID == trezorerie.ID)
            throw new OperareException("Un document nu se poate stinge pe el însuși.");
        if (suma <= 0)
            throw new OperareException("Suma imperecheată trebuie să fie pozitivă.");
        // Contrapartida plății (furnizor/client/angajat) trebuie să apară pe
        // documentul stins — echivalentul grupării pe partener din legacy
        // (spDecontariObligatii); acoperă și lanțul avans↔decont↔regularizare.
        var contrapartida = trezorerie.GetContrapartidaId();
        if (document.PredatorId != contrapartida && document.PrimitorId != contrapartida)
            throw new OperareException(
                "Plata/încasarea și documentul stins nu împart aceeași contrapartidă (partener/angajat).");
        var ramasTrezorerie = Ramas(os, trezorerie.ID);
        if (suma > ramasTrezorerie)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul neasignat al plății/încasării ({ramasTrezorerie:0.##}).");
        var ramasDocument = Ramas(os, document.ID);
        if (suma > ramasDocument)
            throw new OperareException(
                $"Suma imperecheată ({suma:0.##}) depășește restul nestins al documentului ({ramasDocument:0.##}).");
    }
}
