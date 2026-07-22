using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Nucleul generic (deciziile 1, 2, 22). Motoarele de stoc și contare consumă
// DOAR această clasă și DocumentDetaliu — orice câmp de aici e justificat de o
// formulă de stoc, o regulă contabilă sau un motor transversal (testul bazei).
[NavigationItem("Documente")]
public abstract class Document : BaseObject {
    public virtual string Numar { get; set; }
    public virtual DateOnly Data { get; set; }

    public virtual Guid PredatorId { get; set; }
    public virtual Repartitor Predator { get; set; }
    public virtual Guid PrimitorId { get; set; }
    public virtual Repartitor Primitor { get; set; }

    // Decizia 14: Draft → Operat → (Stornat); la operare motorul scrie registrele.
    public virtual StareDocument Stare { get; set; }
    public virtual DateTime? DataOperare { get; set; }

    // Decizia 17: legătura conex sursă→generat (ex. FacturaIntrare → NIR);
    // anularea operează pe tot grupul (00 §8).
    public virtual Guid? DocumentSursaId { get; set; }
    public virtual Document DocumentSursa { get; set; }
    public virtual bool Autogenerat { get; set; }

    [DevExpress.ExpressApp.DC.Aggregated]
    public virtual ObservableCollection<DocumentDetaliu> Detalii { get; set; } = new();

    [NotMapped]
    public decimal Total => Detalii.Sum(d => d.Valoare);

    // Hooks polimorfe consumate DOAR de motorul de operare (decizia 14).
    // Primesc IObjectSpace și lucrează pe FK-uri, nu pe navigații — contextul
    // apelant (UI, harness, viitorul Web API) nu garantează lazy loading.
    // Lanțul de valori aparține derivatei (testul bazei §3): aici derivata
    // materializează `Valoare` pe linii înainte de scrierea registrelor
    // (ex. NotaTransfer/BonConsum: preț lot × cantitate).
    public virtual void PregatesteOperare(DevExpress.ExpressApp.IObjectSpace os) { }

    // Convenția 00 §5 (dimensiunea Repartitor default pe notă: debit←Predator,
    // credit←Primitor) devine default POLIMORF — ultimul nivel al coalesce-ului
    // din motor. Decont o ajustează: creditul (contul de avans 542) urmărește
    // titularul, nu primitorul justificării.
    public virtual Guid RepartitorImplicitDebit() => PredatorId;
    public virtual Guid RepartitorImplicitCredit() => PrimitorId;

    // Documentul SECUNDAR (00 §7 — plata automată legacy, decizia 31): spre
    // deosebire de conexul din PoliticaConex (clonă filtrată pe natură, trăiește
    // în motor), secundarul se construiește din date CULESE pe derivată
    // (grupul DECONT_* al facturii), deci generarea e hook de tip. Motorul îl
    // marchează Autogenerat + DocumentSursa și îl tratează ca pe orice copil
    // al grupului conex (ștergere la anulare, refuz cât e operat).
    public virtual Document GenereazaSecundar(DevExpress.ExpressApp.IObjectSpace os) => null;

    // Invariantele proprii tipului, verificate de motor înainte de operare.
    // Baza impune doar ce cere orice document; obligativitățile per tip se
    // adaugă în override (validarea declarativă completă vine la 3c/3d).
    public virtual void ValideazaOperare(DevExpress.ExpressApp.IObjectSpace os, ICollection<string> erori) {
        if (Detalii.Count == 0)
            erori.Add("Documentul nu are nicio linie.");
        if (PredatorId == Guid.Empty || PrimitorId == Guid.Empty)
            erori.Add("Predatorul și primitorul sunt obligatorii.");
    }
}

// Bază concretă: NIR/BonConsum/NotaTransfer o folosesc direct (testul bazei §6);
// derivate de detaliu există doar unde schema diferă.
public class DocumentDetaliu : BaseObject {
    [Browsable(false)]
    public virtual Guid DocumentId { get; set; }
    public virtual Document Document { get; set; }

    // Cheia contării; Clasa (cheia regulilor de stoc) = TipMaterial.Clasa.
    public virtual Guid TipMaterialId { get; set; }
    public virtual TipMaterial TipMaterial { get; set; }

    // Cheia stocului (decizia 13); null pe tipurile fără stoc.
    public virtual Guid? LotId { get; set; }
    public virtual Lot Lot { get; set; }

    // SEMNATĂ — limbajul motorului e semn × cantitate; LDI poartă direcția în semn.
    public virtual decimal Cantitate { get; set; }

    // Valoarea de postare în registre (stoc valoric + note) — capătul lanțului
    // de valori; derivatele o calculează (testul bazei §3).
    public virtual decimal Valoare { get; set; }

    // Ancoră spre execuția bugetară (modul separat) — testul bazei §7.1.
    public virtual Guid? AngajamentId { get; set; }
    public virtual Angajament Angajament { get; set; }

    // Set parțial; rezolvarea completă se face la generarea registrelor (decizia 15).
    public virtual Dimensiuni Dimensiuni { get; set; } = new();

    // Decizia 25c: lotul se naște LA CULEGERE pe linia de intrare (NIR manual,
    // FacturaIntrare pentru lanțul conex, plus de inventar, producție) — baza nu
    // poartă ProdusId, deci produsul ales intră direct pe Lot. Motorul îl
    // finalizează la operare (PretUnitar = Valoare/Cantitate, Data, atribute).
    public Lot CreeazaLot(DevExpress.ExpressApp.IObjectSpace os, Produs produs, Gestiune gestiune) {
        var lot = os.CreateObject<Lot>();
        lot.Produs = produs;
        lot.Gestiune = gestiune;
        lot.LinieIntrareId = ID;
        Lot = lot;
        return lot;
    }
}
