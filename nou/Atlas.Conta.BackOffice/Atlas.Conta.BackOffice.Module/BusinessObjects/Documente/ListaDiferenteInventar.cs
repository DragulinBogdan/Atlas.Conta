namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// LDI (05): plusuri și minusuri pe aceeași listă; singurul tip bidirecțional pe
// loturi — minusul descarcă un lot, plusul CREEAZĂ lot (cu preț de evaluare).
public class ListaDiferenteInventar : Document {
}

public class ListaDiferenteInventarDetaliu : DocumentDetaliu {
    // Direcția explicită (testul bazei §4) — se materializează în semnul
    // Cantitate-ii din bază; UI-ul culege cantitatea pozitivă.
    public virtual DirectieDiferenta Directie { get; set; }
    // Prețul de evaluare cules la plus (lotul nou se naște cu el).
    public virtual decimal? PretEvaluare { get; set; }
}
