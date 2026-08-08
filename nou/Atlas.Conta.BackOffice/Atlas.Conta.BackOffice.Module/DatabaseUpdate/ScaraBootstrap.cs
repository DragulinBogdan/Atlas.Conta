using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// BOOTSTRAP-UL CONVENȚIEI DE ROTUNJIRE (deciziile 51c / 52a), partajat de TOATE
// host-urile care servesc cereri: motorul rotunjește la materializare
// (`Scara.RotunjesteBani`), iar convenția e dată a BAZEI, nu constantă de cod.
// Se citește O DATĂ, înainte de primul request — Blazor.Server și WebApi trebuie
// să ajungă la aceeași valoare, altfel două procese peste aceeași bază ar posta
// jumătățile de ban în sensuri diferite.
//
// Pe calea `--updateDatabase` NU se apelează: acolo o fixează seed-ul însuși
// (`SetareProfil`).
public static class ScaraBootstrap {
    // Citirea e best-effort (bază încă nemigrată / rând absent ⇒ warning și
    // default-ul AwayFromZero, adică exact comportamentul de dinainte de
    // 51c); APLICAREA stă în afara try-ului — o convenție citită și refuzată
    // de `FixeazaConventia` e o contradicție reală, nu o problemă de acces.
    public static void FixeazaConventia(
            string connectionString,
            Action<string, Exception> avertizeaza,
            Action<string> informeaza = null) {
        MidpointRounding? conventie = null;
        try {
            if (!string.IsNullOrEmpty(connectionString)) {
                // `UseConnectionString` (extensia XAF), NU `UseNpgsql`:
                // connection string-ul din appsettings poartă prefixul
                // `EFCoreProvider=Postgres;`, pe care Npgsql îl refuză —
                // citirea pica în catch, iar convenția rămânea tăcut pe
                // default. Extensia scoate prefixul, alege providerul din el
                // și pune aceleași proxy-uri ca restul aplicației.
                var builder = new DbContextOptionsBuilder<BackOfficeEFCoreDbContext>();
                builder.UseConnectionString(connectionString);
                using var context = new BackOfficeEFCoreDbContext(builder.Options);
                conventie = context.SetariProfil.AsNoTracking()
                    .Select(s => (MidpointRounding?)s.RotunjireBani).FirstOrDefault();
            }
        }
        catch (Exception e) {
            avertizeaza($"Convenția de rotunjire nu a putut fi citită din bază; "
                + $"se folosește {Scara.ConventieBani}.", e);
            return;
        }
        if (conventie == null) {
            avertizeaza($"Baza nu are rând SetareProfil (încă ne-seed-uită?); convenția de "
                + $"rotunjire rămâne {Scara.ConventieBani}.", null);
            return;
        }
        Scara.FixeazaConventia(conventie.Value);
        informeaza?.Invoke($"Convenția de rotunjire a banilor: {Scara.ConventieBani} (din SetareProfil).");
    }
}
