using System.Collections.ObjectModel;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

// S-D1: POCO EF, nu `EntitateConta` — cubul e append-only (fără timbru optimist).
public class Tranzactie {
    public virtual Guid ID { get; set; }

    public virtual Guid? DocumentId { get; set; }

    public virtual N.FelTranzactie Fel { get; set; }

    public virtual DateOnly Data { get; set; }

    public virtual DateTime ScrisLa { get; set; }

    public virtual ObservableCollection<Postare> Postari { get; set; } = new();
}
