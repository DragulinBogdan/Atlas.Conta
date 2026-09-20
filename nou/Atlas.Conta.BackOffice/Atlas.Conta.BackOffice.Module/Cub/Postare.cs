using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

// S-D1: POCO EF, nu `BaseObject`. `Spatiu` e cheia partiției (S-D2), calculată
// la materializare din `postare.Spatiu()`; `UnitateDeschisa` e amendamentul la
// FZ-D2 (cheia FIFO `(Deschisa, Id)`, partida n-are rând de nomenclator).
public class Postare {
    public virtual Guid ID { get; set; }

    public virtual N.Spatiu Spatiu { get; set; }

    public virtual Guid TranzactieId { get; set; }
    public virtual Tranzactie Tranzactie { get; set; }

    public virtual Guid? DocumentId { get; set; }
    public virtual Guid? LinieId { get; set; }

    public virtual DateOnly Data { get; set; }
    public virtual Guid Cont { get; set; }
    public virtual N.Latura Latura { get; set; }
    public virtual Guid? Partener { get; set; }
    public virtual Guid? Gestiune { get; set; }
    public virtual Guid? Produs { get; set; }
    public virtual Guid? Unitate { get; set; }
    public virtual DateOnly? UnitateDeschisa { get; set; }

    public virtual Guid? TipTvaId { get; set; }
    public virtual N.SensTva? SensTva { get; set; }
    public virtual N.RolTva? RolTva { get; set; }
    public virtual int? PerioadaDeclarare { get; set; }

    public virtual Guid? Valuta { get; set; }
    public virtual N.Carte Carte { get; set; }

    public virtual Guid? CodFunctional { get; set; }
    public virtual Guid? CodEconomic { get; set; }
    public virtual Guid? SursaFinantare { get; set; }
    public virtual Guid? UnitateOrganizatorica { get; set; }
    public virtual Guid? Proiect { get; set; }
    public virtual Guid? CentruCost { get; set; }

    public virtual Guid? Atribuit { get; set; }

    public virtual decimal Cantitate { get; set; }
    public virtual decimal ValoareValuta { get; set; }
    public virtual decimal Valoare { get; set; }
}
