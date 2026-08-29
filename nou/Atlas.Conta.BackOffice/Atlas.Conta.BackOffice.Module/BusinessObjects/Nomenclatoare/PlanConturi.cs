using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Decizia 10: plan doar sintetic; analiticele se derivă din dimensiuni la raportare.
[NavigationItem("Nomenclatoare")]
[XafDefaultProperty(nameof(Simbol))]
public class Cont : BaseObject, ICuCautare {
    public virtual string Simbol { get; set; }
    public virtual string Denumire { get; set; }
    public virtual Guid? ParinteId { get; set; }
    public virtual Cont Parinte { get; set; }
    // FCTCONT legacy (D/C/B) — funcția contului la validarea soldului.
    public virtual string Functie { get; set; }
    // SUMATOR legacy: însumare vs soldare pe părinți.
    public virtual bool Sumator { get; set; }
    // Decizia 15: dimensiuni obligatorii per cont (fostele flag-uri R/M/E/B/F/P).
    public virtual DimensiuneFlags DimensiuniObligatorii { get; set; }

    // Felia 16 (D16-D3): pe ce conturi stă un client și pe ce conturi un
    // furnizor — sursa listelor `Customers`/`Suppliers` din SAF-T și a perechii
    // `CustomerID`/`SupplierID` de pe fiecare rând de registru. POLITICĂ per
    // profil (seed), nu simbol în cod; bugetarul îl lasă `Niciunul` peste tot.
    [XafDisplayName("Rol de terț (SAF-T)")]
    public virtual RolTertCont RolTert { get; set; }

    // F20-D1 — coloana GENERATĂ de căutare fără diacritice; valoarea e a
    // BAZEI de date (vezi `Cautare` / `ICuCautare`), EF n-o scrie niciodată.
    [XafDisplayName("Căutare")]
    [VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
    public virtual string Cautare { get; set; }
}
