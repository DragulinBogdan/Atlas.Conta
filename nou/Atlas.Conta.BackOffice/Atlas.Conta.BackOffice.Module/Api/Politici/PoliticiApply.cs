using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Politici;

// Felia 23 (F23-D8): ambalarea raportului de profil. Ca toate Apply-urile,
// stă în Module — testabil din ModelCheck fără host HTTP, cu controllerul
// subțire (55b).
//
// Zero reguli proprii: conținutul e al lui `VerificareProfilService`, care la
// rândul lui cheamă chiar funcțiile seed-ului pentru golurile de mapare (nu o
// a doua copie care ar putea rămâne în urmă).
public static class PoliticiApply {
    /// <summary>
    /// Tipurile pe care raportul le CITEȘTE, deci exact cele pe care ruta cere
    /// dreptul de citire (F23-D8, amendat după măsurătoarea pasului 2). Lista e
    /// cea a configurației (`Politici.TipuriConfigurabile`, 83i) — aceeași pe
    /// care o citesc rolul și raportul, ca adăugarea unei politici noi să nu
    /// poată uita gate-ul; `Partener` și `Produs` se adaugă explicit fiindcă raportul le
    /// citește pentru categoria „tip de TVA inactiv referit ca implicit", dar nu
    /// poartă timbru de proveniență (structura clientului, nu profilul).
    ///
    /// DE CE există gate-ul, când listele obișnuite răspund 200 filtrat (69g):
    /// raportul nu e o listă, e un VERDICT peste starea profilului. Măsurat pe
    /// host înainte de amendament: un `User` care nu vede nimic primea 20 de
    /// constatări afirmând că tipuri de TVA existente „nu există în bază" —
    /// filtrarea tăcută producea un raport plauzibil și FALS, exact defectul pe
    /// care 73g îl descrie pentru fișierul SAF-T. Un verdict fals e mai rău
    /// decât un refuz.
    /// </summary>
    public static IReadOnlyList<Type> TipuriCitite { get; } =
        BusinessObjects.Politici.TipuriConfigurabile
            .Concat([typeof(Partener), typeof(Produs)])
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .ToList();

    /// <summary>
    /// Raportul, pe ușa dată. Se cheamă cu un ObjectSpace NON-SECURED, după ce
    /// apelantul a verificat dreptul de citire pe `TipuriCitite`: cifra trebuie
    /// să fie a bazei, nu a rândurilor vizibile — altfel „lipsește" ar însemna
    /// de fapt „nu-ți e vizibil" (vezi comentariul de mai sus).
    /// </summary>
    public static ConstatareProfilDto[] Verificare(IObjectSpace os) =>
        VerificareProfilService.Raporteaza(os)
            .Select(c => new ConstatareProfilDto {
                Tabel = c.Tabel,
                Cheie = c.Cheie,
                Fel = c.Fel.ToString(),
                Mesaj = c.Mesaj,
            })
            .ToArray();
}
