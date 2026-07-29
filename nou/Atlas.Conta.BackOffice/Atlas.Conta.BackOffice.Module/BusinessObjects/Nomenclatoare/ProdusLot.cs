using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 13: identificare specifică pe lot. Produs = catalogul (fost sumator);
// Lot = fost codmat, creat de linia de intrare, preț unitar fix.

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Denumire))]
public class Produs : BaseObject {
    public virtual string Cod { get; set; }
    public virtual string Denumire { get; set; }
    public virtual string UM { get; set; }
    public virtual Guid? TipMaterialId { get; set; }
    public virtual TipMaterial TipMaterial { get; set; }
}

[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Eticheta))]
public class Lot : BaseObject {
    // GATE XAF (D4): identitatea lizibilă a lotului — singurul tip țintă de lookup
    // care nu avea DefaultProperty, deci apărea „Castle.Proxies.LotProxy" pe
    // FCT/FCL/DSC/LDI/ASM (restanța 40d). Cele trei atribute care distING loturile
    // aceluiași produs: proveniența (data) și prețul de intrare (identificarea
    // specifică — decizia 13). NotMapped: nu e stare, e o proiecție de afișare.
    // Navigația Produs se citește lazy, cu guard — pe lookup-uri și grile de
    // nomenclator; DELIBERAT fără AutoInclude (loturile trec prin hot-path-ul
    // pickingului, unde eticheta nu se afișează niciodată).
    [NotMapped]
    public string Eticheta => $"{Produs?.Denumire} · {Data:dd.MM.yyyy} · {PretUnitar:0.####}";

    public virtual Guid ProdusId { get; set; }
    public virtual Produs Produs { get; set; }
    // Preț fix la creare = Valoare/Cantitate de pe linia de intrare (testul bazei §3).
    public virtual decimal PretUnitar { get; set; }
    public virtual Guid GestiuneId { get; set; }
    public virtual Gestiune Gestiune { get; set; }
    public virtual DateOnly Data { get; set; }
    public virtual DateOnly? DataExpirare { get; set; }
    public virtual string LotFabricatie { get; set; }
    // Linia care a creat lotul (NIR / plus de inventar / raport de producție).
    // Coloană FĂRĂ constrângere FK (intenționat): linia își referă lotul prin
    // LotId, iar lotul linia-mamă — un FK real pe ambele sensuri ar face ciclu
    // de inserție (EF nu sparge cicluri, iar ObjectSpace-ul XAF comite totul
    // într-un singur SaveChanges). Provenința e întreținută de CreeazaLot/motor.
    public virtual Guid? LinieIntrareId { get; set; }
}
