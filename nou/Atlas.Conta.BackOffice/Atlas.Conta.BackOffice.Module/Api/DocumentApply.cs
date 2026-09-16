using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;

namespace Atlas.Conta.BackOffice.Module.Api;

// F27-D4: cele două date ale documentului, într-un singur loc pentru toate
// `*Apply`-urile — implicitul („lipsă" pe sârmă = data documentului) și ordinea
// lor. Refuzul e de DOMENIU (422), ca orice altă regulă a culegerii.
public static class DocumentApply {
    public static void AplicaDate(Document doc, DateOnly data, DateOnly? dataInregistrare) {
        doc.Data = data;
        doc.DataInregistrare = dataInregistrare ?? data;
        if (doc.DataInregistrare < doc.Data)
            throw new OperareException("Data înregistrării nu poate preceda data documentului.");
    }

    /// <summary>Documentul generat intră în evidență la data lui; întoarce chiar documentul primit.</summary>
    // Fără asta, draftul generat ar purta `default` până la operare (motorul îl
    // normalizează abia atunci) și l-ar ARĂTA așa pe ecrane. F27-r9: editabilitatea
    // câmpului pe generate se decide separat.
    public static T Generat<T>(T doc) where T : Document {
        if (doc != null)
            doc.DataInregistrare = doc.Data;
        return doc;
    }
}
