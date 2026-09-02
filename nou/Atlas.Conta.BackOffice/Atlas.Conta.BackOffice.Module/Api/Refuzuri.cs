using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Utils;

namespace Atlas.Conta.BackOffice.Module.Api;

// Operația de acces despre care se pune întrebarea „am voie?" (felia 22, F22-D1).
// NU e o copie a `SecurityOperations` (acelea sunt string-uri ale DevExpress):
// e vocabularul NOSTRU, cel care alege fraza de refuz și pe care îl poartă
// `RefuzAcces` mai jos, ca apelantul (controller, filtru OData) să poată decide
// codul HTTP fără să reparseze un mesaj.
public enum OperatieAcces {
    Citire,
    Creare,
    Modificare,
    Stergere
}

// ═══ Mesajele de refuz de ACCES — o singură sursă în Module (F22-D4) ═══
//
// De ce aici și nu în WebApi: aceleași fraze le pune controllerul (gate-ul
// explicit al ușii de scriere, D2), le aruncă `GardianEditare` (pasul zero, D3)
// și le traduce filtrul OData (D4). Trei uși, un text — altfel „n-ai voie" sună
// diferit după cum a intrat cererea, exact defectul pe care felia îl închide.
//
// Ce NU e aici: refuzurile de DOMENIU (`OperareException`, 422). Un refuz de
// domeniu presupune că ai avut dreptul să formulezi cererea; refuzul de acces
// spune că n-ai avut. Cele două nu se pot substitui și nu se pot amesteca într-o
// listă comună (F22-D1: „un 422 nu poate ascunde un refuz de permisiune").
public static class Refuzuri {
    // 404 — subiectul cererii nu e vizibil. DELIBERAT nedistins de „inexistent":
    // altfel API-ul devine un oracol de existență pentru rândurile pe care
    // securitatea le ascunde (F22-D1). Fraza e aceeași și când rândul chiar nu
    // există — asta e ideea, nu o scăpare de formulare.
    public const string Invizibil =
        "Înregistrarea nu există sau nu e vizibilă pentru utilizatorul curent.";

    // 403 — subiectul e vizibil (sau întrebarea e pe TIP), dar operația nu-ți e
    // permisă. Numele clasei vine din captionul XAF, ca operatorul să citească
    // „Factură de intrare", nu `FacturaIntrare` și cu atât mai puțin
    // `FacturaIntrareProxy`.
    public static string FaraDrept(OperatieAcces operatie, Type tip) =>
        $"Nu aveți dreptul de a {Verb(operatie)} „{Caption(tip)}”.";

    // 422 — referința pe care apelantul n-o VEDE (F22-D6). Nu e subiectul
    // cererii, deci nu e 404; e o cerere pe care apelantul n-o poate formula
    // corect. Dar mesajul spune adevărul: pe ușa securizată `GetObjectByKey`
    // întoarce `null` și pentru inexistent, și pentru invizibil
    // (`SecurityQueryCompiler` înfășoară orice query cu predicatul de
    // securitate), iar vechiul „nu există" afirma ceva ce codul nu putea ști.
    public static string ReferintaInvizibila(string rol, object id) =>
        $"{rol} ({id}) nu există sau nu e vizibil(ă) pentru utilizatorul curent.";

    static string Verb(OperatieAcces operatie) => operatie switch {
        OperatieAcces.Citire => "citi",
        OperatieAcces.Creare => "crea",
        OperatieAcces.Modificare => "modifica",
        OperatieAcces.Stergere => "șterge",
        _ => "efectua operația pe"
    };

    // Captionul XAF al clasei, cu DOUĂ plase:
    //   (1) proxy-ul de change-tracking al EF e un tip dinamic derivat
    //       (`FacturaIntrareProxy`) — mesajul poartă numele clasei de DOMENIU,
    //       nu al proxy-ului (aceeași grijă ca în `GardianEditare`);
    //   (2) fără model de aplicație (ModelCheck, unelte de consolă)
    //       `CaptionHelper.GetClassCaption` întoarce chiar numele complet primit
    //       (`CaptionHelperImplementer.cs:273-292`: fără `BoModel` iese
    //       `classFullName`), care ca text de UI e mai rău decât numele scurt.
    //       Deci: caption ≠ numele complet ⇒ caption; altfel numele CLR scurt.
    public static string Caption(Type tip) {
        var real = TipReal(tip);
        if (real == null)
            return "";
        var numeComplet = NumeComplet(real);
        try {
            var caption = CaptionHelper.GetClassCaption(numeComplet);
            if (!string.IsNullOrWhiteSpace(caption)
                    && !string.Equals(caption, numeComplet, StringComparison.Ordinal))
                return caption;
        }
        catch {
            // Captionul e cosmetic: dacă infrastructura XAF nu e pornită, refuzul
            // rămâne refuz — nu se transformă în altă excepție decât cea de acces.
        }
        return real.Name;
    }

    // Numele sub care XAF ține clasa în model. `FindTypeInfo` e sursa corectă
    // (poate întoarce tipul generat pentru membri non-persistenți); dacă
    // `XafTypesInfo` nu e inițializat, `FullName`-ul CLR e o aproximare bună.
    static string NumeComplet(Type tip) {
        try {
            var typeInfo = XafTypesInfo.Instance?.FindTypeInfo(tip);
            if (!string.IsNullOrEmpty(typeInfo?.FullName))
                return typeInfo.FullName;
        }
        catch {
        }
        return tip.FullName ?? tip.Name;
    }

    // Clasa de domeniu din spatele unui eventual proxy EF. Testul principal e
    // `Assembly.IsDynamic` (proxy-urile se emit într-un assembly dinamic — exact
    // criteriul din `GardianEditare.VerificaCodDenumire`); numele („…Proxy",
    // „Castle…") e a doua plasă, pentru generatoarele care ar emite într-un
    // assembly real.
    public static Type TipReal(Type tip) {
        for (var pas = 0; tip != null && pas < 8; pas++) {
            var dinamic = tip.Assembly.IsDynamic
                || tip.Name.Contains("Proxy", StringComparison.Ordinal)
                || tip.Namespace?.Contains("Castle", StringComparison.Ordinal) == true;
            if (!dinamic || tip.BaseType == null)
                return tip;
            tip = tip.BaseType;
        }
        return tip;
    }
}

// Refuzul de ACCES, ca excepție (F22-D3/D4).
//
// `IUserFriendlySecurityException` e un marker PUR (`DevExpress.ExpressApp\
// Exceptions.cs:45-46`: `interface IUserFriendlySecurityException :
// IUserFriendlyException {}`, ambele fără membri) — nu cere nimic de
// implementat, dar e exact eticheta pe care `UserFriendlyExceptionFilter` o
// caută ca să răspundă 403 în loc de 400. Ridicând-o noi, refuzul gardianului
// iese pe sârmă cu codul dreptului, nu cu cel al domeniului — inversarea din
// 77k, unde `User` primea 422 de la gardian ÎNAINTE ca securitatea din
// `SaveChanges` să apuce să spună 403.
//
// De ce NU derivă din `OperareException`: aceea e domeniul (422). Un refuz de
// acces care s-ar prinde în `catch (OperareException)` ar fi înghițit de
// oricare dintre acumulatoarele de erori de domeniu și raportat ca regulă de
// business încălcată. Aici tocmai separarea e conținutul deciziei.
public sealed class RefuzAcces : Exception, IUserFriendlySecurityException {
    public RefuzAcces(OperatieAcces operatie, Type tip)
            : base(Refuzuri.FaraDrept(operatie, tip)) {
        Operatie = operatie;
        Tip = Refuzuri.TipReal(tip);
    }

    // Operația refuzată și clasa pe care s-a cerut — ca DATE, nu ca text:
    // apelantul (filtru, controller, log) nu trebuie să reparseze mesajul ca să
    // afle ce s-a întâmplat.
    public OperatieAcces Operatie { get; }
    public Type Tip { get; }
}
