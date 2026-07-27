using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using DevExpress.Persistent.BaseImpl.EF;

namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Setarea de PROFIL a bazei (decizia 51c): un singur rând, scris de seed.
//
// Până acum profilul trăia doar în appsettings (`ProfilContabil`) — selecție de
// pachet de seed, verificată indirect prin ancora planului (`VerificaProfil`).
// Convenția de rotunjire a banilor nu se poate deduce însă din date: e o alegere
// care se aplică la MATERIALIZARE (`Scara.RotunjesteBani`), deci trebuie citită
// din bază înainte ca motorul să scrie primul rând de registru — și ÎNGHEȚATĂ
// pe viață: schimbarea ei pe o bază vie amestecă istoricul (jumătățile de ban
// deja postate au fost decise cu cealaltă regulă).
//
// Rândul e infrastructură, nu nomenclator de UI (ca `MigrareLegatura`): fără
// NavigationItem — se editează prin seed / migrare de date, deliberat.
public class SetareProfil : BaseObject {
    // Oglindește appsettings `ProfilContabil` — gardianul din seed refuză o bază
    // seed-uită cu alt profil decât cel configurat (completează `VerificaProfil`,
    // care se uită doar la ancora planului de conturi).
    public virtual ProfilContabil Profil { get; set; }

    // Convenția de rotunjire a valorilor POSTATE (scara Bani). AwayFromZero =
    // rotunjirea comercială (ce fac sursele de date: 1C, facturile furnizorilor);
    // ToEven = rotunjirea bancară, care compensează statistic jumătățile de ban
    // în loc să le împingă mereu în același sens. Vezi `Scara` pentru mecanică.
    public virtual MidpointRounding RotunjireBani { get; set; }
}
