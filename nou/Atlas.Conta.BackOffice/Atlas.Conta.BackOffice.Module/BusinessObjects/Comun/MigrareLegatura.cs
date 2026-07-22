using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Infrastructura pasului 4 (decizia 12): corelarea rândurilor legacy cu
// entitățile create de migrare — cheia idempotenței (re-rularea uneltei updatează
// în loc să dubleze) și evidența de proveniență. Nu e nomenclator de UI.
public class MigrareLegatura : BaseObject {
    // Tabela sursă din legacy (ex. "REPARTITORI", "GEST_SUMATOR", "GEST_GNMCL").
    public virtual string Tabela { get; set; }
    // Cheia primară legacy, ca text (id-uri int sau codmat).
    public virtual string CheieLegacy { get; set; }
    // Entitatea creată în modelul nou.
    public virtual Guid TintaId { get; set; }
}
