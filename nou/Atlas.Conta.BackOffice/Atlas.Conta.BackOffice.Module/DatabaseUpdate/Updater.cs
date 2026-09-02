using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate {
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            // Profilul contabil e setare per bază (appsettings `ProfilContabil`,
            // decizia 35d) — selectează pachetul de seed, nu schema.
            var config = ObjectSpace.ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            var profil = Enum.TryParse<ProfilContabil>(config?["ProfilContabil"], true, out var p)
                ? p : ProfilContabil.Bugetar;
            ContaSeeder.Seed(ObjectSpace, profil, CitesteConventieRotunjire(config));

            // The code below creates users and roles for testing purposes only.
            // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
            // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
            // If a role doesn't exist in the database, create this role
            var defaultRole = CreateDefaultRole();
            var adminRole = CreateAdminRole();
            var cititoriRole = CreateCititoriRole();

            ObjectSpace.CommitChanges(); //This line persists created object(s).

            UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

            // If a user named 'User' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null) {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) => {
                    // Add the Users role to the user
                    user.Roles.Add(defaultRole);
                });
            }

            // If a user named 'Admin' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null) {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) => {
                    // Add the Administrators role to the user
                    user.Roles.Add(adminRole);
                });
            }

            // Utilizatorul „Cititor" (felia 22, F22-D7) — al treilea colț al
            // matricei de probare a refuzurilor. Fără el, 403-ul de scriere nu
            // era MĂSURABIL pe nicio ușă: `Admin` trece tot (rol administrativ),
            // iar `User` e refuzat mai devreme, de invizibilitate (404/422 pe
            // primul FK pe care nu-l vede — 72-r10/76-r5), deci nu ajunge
            // niciodată la întrebarea „ai voie să scrii?". `Cititor` vede TOT și
            // n-are voie să scrie NIMIC: exact cazul care izolează dreptul de
            // vizibilitate. Parolă goală, ca la ceilalți doi — dev-only.
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Cititor") == null) {
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Cititor", EmptyPassword, (user) => {
                    user.Roles.Add(cititoriRole);
                });
            }

            ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif
        }
        public override void UpdateDatabaseBeforeUpdateSchema() {
            base.UpdateDatabaseBeforeUpdateSchema();
        }
        // Convenția de rotunjire a banilor (decizia 51c) — appsettings
        // `ConventieRotunjire` (numele unui MidpointRounding). Cheie OPȚIONALĂ:
        // absentă ⇒ baza își păstrează convenția (AwayFromZero la bază nouă).
        // O valoare scrisă greșit NU se ignoră: ar seed-ui tăcut cu default-ul,
        // adică exact convenția pe care cineva a încercat s-o schimbe.
        static MidpointRounding? CitesteConventieRotunjire(Microsoft.Extensions.Configuration.IConfiguration config) {
            var text = config?["ConventieRotunjire"];
            if (string.IsNullOrWhiteSpace(text))
                return null;
            if (!Enum.TryParse<MidpointRounding>(text, true, out var conventie))
                throw new InvalidOperationException(
                    $"appsettings `ConventieRotunjire` = „{text}” nu e un MidpointRounding valid "
                    + $"({string.Join(", ", Enum.GetNames<MidpointRounding>())}).");
            return conventie;
        }
        PermissionPolicyRole CreateAdminRole() {
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if (adminRole == null) {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            return adminRole;
        }
        // Rolul „Cititori" (felia 22, F22-D7): Read pe TOT, niciun Write, Create
        // sau Delete. DEV-ONLY, ca `Default`/`Administrators` — trăiește în
        // blocul `#if !RELEASE` de mai sus și nu e un rol de producție (F22-D11).
        //
        // Forma aleasă e `PermissionPolicy = ReadOnlyAllByDefault`, nu o listă de
        // `AddTypePermissionsRecursively<...>(Read, Allow)`. Motivul e că
        // politica dă EXACT semantica cerută, la sursă:
        // `PermissionsContainer.IsOperationAllowByPolicy` (26.1.3,
        // `PermissionsContainer.cs:279-281`) acordă, pentru
        // `ReadOnlyAllByDefault`, DOAR `Read` și `Navigate` — deci navigația e
        // permisă fără nicio `AddNavigationPermission`, iar Write/Create/Delete
        // sunt refuzate pe orice tip, inclusiv pe tipurile care s-ar adăuga
        // mâine. O listă enumerată ar fi trebuit ținută la zi, și fiecare tip
        // uitat ar fi făcut proba să treacă din alt motiv decât cel probat.
        //
        // Nu primește permisiunile de „propriul ApplicationUser" ale rolului
        // `Default`: acolo ele sunt EXCEPȚII de la o politică deny-all (citirea
        // propriului user, scrierea parolei). Aici citirea e deja permisă peste
        // tot, iar scrierile alea sunt exact ce rolul trebuie să nu poată face —
        // un `Cititor` care ar putea scrie ceva n-ar mai fi oracolul lui 403.
        PermissionPolicyRole CreateCititoriRole() {
            PermissionPolicyRole cititoriRole =
                ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Cititori");
            if (cititoriRole == null) {
                cititoriRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                cititoriRole.Name = "Cititori";
                cititoriRole.PermissionPolicy = SecurityPermissionPolicy.ReadOnlyAllByDefault;
            }
            return cititoriRole;
        }
        PermissionPolicyRole CreateDefaultRole() {
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if (defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserObject.Key == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<AuditEFCoreWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);
            }
            return defaultRole;
        }
    }
}
