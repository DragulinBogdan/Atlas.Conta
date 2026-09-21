using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api;

/// <summary>Rezolvarea unei referințe primite pe sârmă; refuzul e de domeniu (422), cu fraza unică din <see cref="Refuzuri"/> (F22-D6).</summary>
public static class Rezolva {
    /// <summary>Referința obligatorie: inexistentă, invizibilă sau de alt tip al ierarhiei ⇒ <see cref="OperareException"/>; <paramref name="rol"/> numește FK-ul în mesaj.</summary>
    public static T Cere<T>(IObjectSpace os, Guid id, string rol) where T : class {
        var rand = RandDupaCheie.Oricare(os, typeof(T), id);
        return rand as T ?? throw new OperareException(RandDupaCheie.Refuz(rand, typeof(T), rol, id));
    }

    /// <summary>Referința opțională: <c>null</c> ⇒ <c>null</c>, fără interogare; <c>Guid.Empty</c> e valoare culeasă, deci se rezolvă.</summary>
    public static T Optional<T>(IObjectSpace os, Guid? id, string rol) where T : class =>
        id == null ? null : Cere<T>(os, id.Value, rol);
}
