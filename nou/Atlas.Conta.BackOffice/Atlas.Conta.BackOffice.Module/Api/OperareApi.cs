using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

// Rezultatul unei comenzi de document, ca DATE (decizia 42b / spike D2):
// motorul întoarce ENTITĂȚI (documentul conex), adaptorul le traduce în chei și
// mesaje — singura formă care traversează sârma și singura care supraviețuiește
// închiderii ObjectSpace-ului comenzii.
//   - `StareNoua` = starea documentului DUPĂ comandă (Operat/Draft/Stornat);
//   - `ConexId` = documentul conex/secundar generat în aceeași tranzacție
//     (draft autogenerat — decizia 17/31e), pe care apelantul îl deschide;
//   - `Mesaje` = informări pentru operator (NU erori: erorile ies ca
//     `OperareException`, cu liniile cumulate pe „\n").
public sealed record OperareRezultat(
    Guid DocumentId,
    StareDocument StareNoua,
    Guid? ConexId,
    IReadOnlyList<string> Mesaje);

// Adaptorul „comandă prin ID" peste MotorOperare (decizia 42b): puntea dintre
// tierul apelant (Web API la pasul 5, DocumentOperareController azi) și motor e
// ID-ul în ambele sensuri — nicio entitate nu trece granița.
//
// CONTRACT DE APELANT (42b, „secvență nu cuib"): `os` e ObjectSpace-ul
// COMENZII, creat de apelant și aruncat după ea — pe căile vii un OS
// NON-SECURED (`INonSecuredObjectSpaceFactory`), fiindcă motorul scrie registre
// și schimbă `Stare`, adică exact ce refuză gardianul de Committing pe
// ObjectSpace-urile secured (`GardianEditare`). Culegerea se comite ÎNAINTE, în
// OS-ul secured al apelantului; comanda e faza a doua, tranzacția integral a
// motorului. Căile standalone (ModelCheck/Import1C/Migrare) pot folosi orice
// ObjectSpace — gardianul nu e activ acolo.
public static class OperareApi {
    public static OperareRezultat Opereaza(IObjectSpace os, Guid documentId) {
        var doc = Incarca(os, documentId);
        var conex = MotorOperare.Opereaza(os, doc);
        var mesaje = new List<string>();
        if (conex != null)
            mesaje.Add($"S-a generat documentul conex {Eticheta(os, conex)}.");
        // Informările proprii tipului (F8-D10), DUPĂ commit-ul motorului: hook-ul
        // vede lumea finală (inclusiv copiii tocmai generați) și, prin contract,
        // nu aruncă — o operare cu registre deja comise nu poate eșua din cauza
        // unui mesaj.
        mesaje.AddRange(doc.MesajeDupaOperare(os));
        return new OperareRezultat(doc.ID, doc.Stare, conex?.ID, mesaje);
    }

    public static OperareRezultat AnuleazaOperarea(IObjectSpace os, Guid documentId) {
        var doc = Incarca(os, documentId);
        MotorOperare.AnuleazaOperarea(os, doc);
        return new OperareRezultat(doc.ID, doc.Stare, null, Array.Empty<string>());
    }

    public static OperareRezultat Storneaza(IObjectSpace os, Guid documentId, DateOnly dataStorno) {
        var doc = Incarca(os, documentId);
        MotorOperare.Storneaza(os, doc, dataStorno);
        return new OperareRezultat(doc.ID, doc.Stare, null, Array.Empty<string>());
    }

    // Dry-run (D3): fazele calculează+validează, fără materializare și fără
    // commit. Listă goală = documentul trece toți gardienii. Apelantul dă un OS
    // PROPRIU, aruncat după apel — `PregatesteOperare` scrie pe linii (vezi
    // `MotorOperare.Valideaza`).
    public static IReadOnlyList<string> Valideaza(IObjectSpace os, Guid documentId) {
        var doc = Incarca(os, documentId);
        return MotorOperare.Valideaza(os, doc);
    }

    // Încărcarea POLIMORFĂ pe baza TPT: `GetObjectByKey<Document>` întoarce
    // instanța tipului derivat real (același apel îl face motorul pe
    // `DocumentSursaId` la imperecherea automată — MotorOperare.Opereaza).
    static Document Incarca(IObjectSpace os, Guid documentId) =>
        Rezolva.Cere<Document>(os, documentId, "Documentul");

    // Eticheta unui draft autogenerat: numărul dacă îl are (nu-l are — numărul
    // se consumă abia la propria operare, GATE XAF D6), altfel codul tipului.
    static string Eticheta(IObjectSpace os, Document doc) {
        if (!string.IsNullOrWhiteSpace(doc.Numar))
            return doc.Numar;
        try {
            return MotorOperare.GasesteTipDocument(os, doc).Cod;
        }
        catch (OperareException) {
            return doc.GetType().Name;
        }
    }
}
