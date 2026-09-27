using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>Rezultatul unei comenzi de document, ca date: starea după comandă, conexul generat și informările.</summary>
public sealed record OperareRezultat(
    Guid DocumentId,
    StareDocument StareNoua,
    Guid? ConexId,
    IReadOnlyList<string> Mesaje);

/// <summary>Rezultatul corecției: originalul stornat și draftul nou, cu codul tipului.</summary>
public sealed record CorectieRezultat(
    Guid OriginalId,
    Guid CorectieId,
    StareDocument StareOriginal,
    string TipCod);

public enum ComandaDocument {
    Opereaza,
    AnuleazaOperarea,
    Storneaza,
    Corecteaza,
    Valideaza
}

/// <summary>Coaja comenzii (104b): verifică dreptul operatorului, apoi rulează motorul în contextul ales de ea.</summary>
public sealed class ComenziDocument {
    readonly INonSecuredObjectSpaceFactory fabrica;
    readonly IObjectSpace contextSistem;
    readonly IDreptComanda drept;

    public ComenziDocument(INonSecuredObjectSpaceFactory fabrica, IDreptComanda drept) {
        this.fabrica = fabrica ?? throw new ArgumentNullException(nameof(fabrica));
        this.drept = drept ?? throw new ArgumentNullException(nameof(drept));
    }

    ComenziDocument(IObjectSpace contextSistem) {
        this.contextSistem = contextSistem ?? throw new ArgumentNullException(nameof(contextSistem));
        drept = DreptComanda.Sistem;
    }

    /// <summary>Ușa de sistem a uneltelor standalone: contextul apelantului, fără operator (104-r2).</summary>
    public static ComenziDocument Sistem(IObjectSpace os) => new(os);

    public OperareRezultat Opereaza(Guid documentId) =>
        Executa(documentId, ComandaDocument.Opereaza, os => {
            using var tx = TranzactieComanda.Incepe(os);
            var doc = Incarca(os, documentId);
            var conex = MotorOperare.Opereaza(os, doc);
            var mesaje = new List<string>();
            if (conex != null)
                mesaje.Add($"S-a generat documentul conex {Eticheta(os, conex)}.");
            mesaje.AddRange(doc.MesajeDupaOperare(os));
            if (doc.CuTva()) {
                var pozitii = doc.Detalii.OrderBy(l => l.Pozitie).ThenBy(l => l.ID)
                    .Select((l, i) => (l.ID, Pozitie: i + 1)).ToDictionary(l => l.ID, l => l.Pozitie);
                mesaje.AddRange(DiagnosticTvaService.Citeste(os, documentId: doc.ID)
                    .Select(r => $"{r.Cod}: linia {pozitii[r.LinieId]} — {r.Motiv}"));
            }
            tx.Commit();
            return new OperareRezultat(doc.ID, doc.Stare, conex?.ID, mesaje);
        });

    public OperareRezultat AnuleazaOperarea(Guid documentId) =>
        Executa(documentId, ComandaDocument.AnuleazaOperarea, os => {
            using var tx = TranzactieComanda.Incepe(os);
            var doc = Incarca(os, documentId);
            MotorOperare.AnuleazaOperarea(os, doc);
            tx.Commit();
            return new OperareRezultat(doc.ID, doc.Stare, null, Array.Empty<string>());
        });

    public OperareRezultat Storneaza(Guid documentId, DateOnly dataStorno) =>
        Executa(documentId, ComandaDocument.Storneaza, os => {
            using var tx = TranzactieComanda.Incepe(os);
            var doc = Incarca(os, documentId);
            MotorOperare.Storneaza(os, doc, dataStorno);
            tx.Commit();
            return new OperareRezultat(doc.ID, doc.Stare, null, Array.Empty<string>());
        });

    public CorectieRezultat Corecteaza(Guid documentId, DateOnly dataCorectie, MotivCorectie motiv) =>
        Executa(documentId, ComandaDocument.Corecteaza, os => {
            var (storno, corectie) = CorectieService.Corecteaza(os, documentId, dataCorectie, motiv);
            return new CorectieRezultat(storno.ID, corectie.ID, storno.Stare,
                MotorOperare.GasesteTipDocument(os, corectie).Cod);
        });

    /// <summary>Dry-run: fazele calculează și validează, fără materializare; lista goală = documentul trece.</summary>
    public IReadOnlyList<string> Valideaza(Guid documentId) =>
        Executa(documentId, ComandaDocument.Valideaza, os => MotorOperare.Valideaza(os, Incarca(os, documentId)));

    T Executa<T>(Guid documentId, ComandaDocument comanda, Func<IObjectSpace, T> motor) {
        drept.Cere(documentId, comanda);
        if (contextSistem != null)
            return motor(contextSistem);
        using var os = fabrica.CreateNonSecuredObjectSpace(typeof(Document));
        return motor(os);
    }

    static Document Incarca(IObjectSpace os, Guid documentId) =>
        Rezolva.Cere<Document>(os, documentId, "Documentul");

    static string Eticheta(IObjectSpace os, Document doc) {
        if (!string.IsNullOrWhiteSpace(doc.Numar))
            return doc.Numar;
        try {
            return MotorOperare.GasesteTipDocument(os, doc).Cod;
        }
        catch (OperareException) {
            return doc.ClrType;
        }
    }
}
