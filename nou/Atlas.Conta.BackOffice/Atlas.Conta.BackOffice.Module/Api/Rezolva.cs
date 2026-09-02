using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

// ═══ Rezolvarea unei REFERINȚE primite pe sârmă (felia 22, F22-D6) ═══
//
// Fiecare `*Apply` primește DTO-uri cu FK-uri (`TipMaterialId`, `LotId`,
// `PredatorId`…) și le transformă în entități prin `GetObjectByKey`. Pe ușa
// securizată `null` are DOUĂ cauze indistinguibile — rândul nu există, sau
// există și securitatea îl ascunde (`SecurityQueryCompiler` înfășoară orice
// query, inclusiv `Find`). Cele ~84 de mesaje vechi afirmau prima cauză; când
// era a doua, spuneau ceva ce codul nu putea ști, iar operatorul căuta un rând
// pierdut în loc să ceară un drept (76-r5).
//
// Codul rămâne 422, nu 404: referința nu e SUBIECTUL cererii (acela e
// documentul din rută), deci nu se aplică regula „invizibil ⇒ 404". E o cerere
// pe care apelantul n-o poate formula corect — refuz de domeniu, cu mesaj
// onest. Fraza e una singură (`Refuzuri.ReferintaInvizibila`), pe toate feliile.
//
// De ce un helper și nu 84 de fraze la fața locului: fraza e o DECIZIE (ce
// spunem când nu putem distinge), nu o formulare locală. Cât timp era copiată,
// s-a și divergit — „nu există", „nu există în nomenclator", „nu există în
// nomenclatorul de repartitori", „nu există în catalog", patru texte pentru
// aceeași necunoaștere.
public static class Rezolva {
    // Referința OBLIGATORIE. `rol` e eticheta din mesajul vechi („Furnizorul",
    // „Tipul (contul/clasa)", „Lotul") — ea rămâne, fiindcă e singurul lucru
    // care spune CARE dintre cele zece FK-uri ale liniei e problema.
    public static T Cere<T>(IObjectSpace os, Guid id, string rol) where T : class =>
        os.GetObjectByKey<T>(id)
            ?? throw new OperareException(Refuzuri.ReferintaInvizibila(rol, id));

    // Referința OPȚIONALĂ: `null` ⇒ `null`, fără interogare. Semantica veche a
    // helper-elor `Nomenclator<T>` din felii, păstrată intactă — inclusiv faptul
    // că `Guid.Empty` NU e „absent": e o valoare culeasă care nu se rezolvă,
    // deci refuz (proba NTC pe `TipMaterialId = Guid.Empty` se bazează pe asta).
    //
    // Ce NU face: nu inventează un mesaj de „câmp obligatoriu". Absența unui
    // FK cerut se refuză acolo unde regula e a tipului, cu mesajul ei — aici e
    // doar rezolvarea, nu validarea.
    public static T Optional<T>(IObjectSpace os, Guid? id, string rol) where T : class =>
        id == null ? null : Cere<T>(os, id.Value, rol);
}
