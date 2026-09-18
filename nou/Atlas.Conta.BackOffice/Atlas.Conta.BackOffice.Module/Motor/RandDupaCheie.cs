using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;

namespace Atlas.Conta.BackOffice.Module.Motor;

/// <summary>Rândul după cheie, căutat pe rădăcina ierarhiei EF a tipului cerut, cu tipul verificat după.</summary>
// 89: identity map-ul EF e per rădăcină, iar prefetch-ul XAF întoarce intrarea urmărită fără verificarea tipului.
public static class RandDupaCheie {
    /// <summary>Rândul cu cheia dată, de orice tip al ierarhiei lui <paramref name="tip"/>; <c>null</c> = inexistent sau invizibil.</summary>
    public static object Oricare(IObjectSpace os, Type tip, object id) {
        var radacina = Radacina(os, tip);
        // Negăsit pe rădăcină ⇒ cheia nu e urmărită, deci frunza merge la bază, cu permisiunile tipului cerut.
        return os.GetObjectByKey(radacina, id) ?? (radacina == tip ? null : os.GetObjectByKey(tip, id));
    }

    /// <summary>Rândul ca <typeparamref name="T"/>; <c>null</c> și când există, dar e de alt tip.</summary>
    public static T Ca<T>(IObjectSpace os, object id) where T : class =>
        Oricare(os, typeof(T), id) as T;

    /// <summary>Rândul ca <typeparamref name="T"/>, sau <c>null</c> cu refuzul de domeniu adăugat în <paramref name="erori"/>.</summary>
    public static T Cere<T>(IObjectSpace os, Guid id, string rol, ICollection<string> erori) where T : class {
        var rand = Oricare(os, typeof(T), id);
        if (rand is T gasit)
            return gasit;
        erori.Add(Refuz(rand, typeof(T), rol, id));
        return null;
    }

    /// <summary>Refuzul pentru un rând care nu e <paramref name="cerut"/>: invizibil dacă lipsește, altfel tipul găsit.</summary>
    public static string Refuz(object rand, Type cerut, string rol, object id) =>
        rand == null
            ? Api.Refuzuri.ReferintaInvizibila(rol, id)
            : Api.Refuzuri.RandDeAltTip(rol, id, rand.GetType(), cerut);

    /// <summary>Rolul unui FK în refuz: „membrul” pe clasa care îl declară (aceeași formă ca regula (o) a gardianului).</summary>
    public static string RolFk(Type dependent, string membru) {
        var tip = Api.Refuzuri.TipReal(dependent);
        return $"„{CaptionMembru(tip, membru)}” pe {Api.Refuzuri.Caption(tip)}";
    }

    static string CaptionMembru(Type tip, string membru) {
        try {
            var caption = DevExpress.ExpressApp.Utils.CaptionHelper.GetMemberCaption(tip, membru);
            return string.IsNullOrWhiteSpace(caption) ? membru : caption;
        }
        catch {
            return membru;
        }
    }

    static Type Radacina(IObjectSpace os, Type tip) =>
        (os as EFCoreObjectSpace)?.DbContext.Model.FindEntityType(tip)?.GetRootType().ClrType ?? tip;
}
