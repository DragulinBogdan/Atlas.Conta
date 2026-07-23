namespace Atlas.Conta.BackOffice.Module.UI;

// Colecția `Detalii` a lui Document e tipată pe BAZĂ (DocumentDetaliu), deci
// butonul New al listei nested creează linia de bază, iar coloanele nu arată
// câmpurile derivatei. Atributul declară, per tip de document, ce clasă de
// detaliu poartă — TipDetaliuViewUpdater comută ListView-ul colecției pe cel al
// tipului derivat, astfel New-ul creează derivata și coloanele reflectă schema ei.
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TipDetaliuAttribute : Attribute {
    public TipDetaliuAttribute(Type tipDetaliu) => TipDetaliu = tipDetaliu;

    public Type TipDetaliu { get; }
}
