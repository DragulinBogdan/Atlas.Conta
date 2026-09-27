using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

public sealed class RecalculTvaRequestDto {
    public List<Guid> Linii { get; set; } = [];
}

public static class RecalculTvaApply {
    public static void Aplica(IObjectSpace os, Guid documentId, IReadOnlyCollection<Guid> ids) {
        var doc = Rezolva.Cere<Document>(os, documentId, "Documentul");
        var linii = os.GetObjectsQuery<DocumentDetaliu>().Where(l => l.DocumentId == documentId && ids.Contains(l.ID)).ToArray();
        if (ids.Count == 0 || linii.Length != ids.Distinct().Count())
            throw new OperareException("Selecția liniilor nu este disponibilă pe document.");
        CulegereDocument.RecalculeazaTva(os, doc, linii);
        CulegereDocument.Normalizeaza(os, doc);
        os.CommitChanges();
    }
}
