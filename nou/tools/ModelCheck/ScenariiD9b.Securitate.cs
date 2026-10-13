using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;
using C = Atlas.Conta.BackOffice.Module.Cub;
using P = Atlas.Conta.BackOffice.Module.Cub.Citiri.Partide;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiD9b {
    sealed class SesiuneOrigini : IDisposable {
        readonly SecuredEFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext> provider;
        readonly IObjectSpace logon;
        readonly SecurityStrategyComplex strategie;
        public SesiuneOrigini(string conexiune, string nume) {
            var autentificare = new AuthenticationStandard(typeof(ApplicationUser), typeof(AuthenticationStandardLogonParameters));
            strategie = new(typeof(ApplicationUser), typeof(PermissionPolicyRole), autentificare);
            provider = new(strategie, (b, _) => b.UseNpgsql(conexiune).UseChangeTrackingProxies()
                .UseObjectSpaceLinkProxies().UseLazyLoadingProxies());
            autentificare.SetLogonParameters(new AuthenticationStandardLogonParameters(nume, ""));
            logon = provider.CreateNonsecuredObjectSpace(); strategie.Logon(logon);
        }
        public IObjectSpace Deschide() {
            var os = provider.CreateObjectSpace();
            new GardianEditare(strategie).OnObjectSpaceCreated(os); return os;
        }
        public IObjectSpace Sistem() => provider.CreateNonsecuredObjectSpace();
        public void Dispose() { logon.Dispose(); provider.Dispose(); }
    }

    void Securitate() {
        Titular("VIZIBILITATE"); var f = Factura(100); var plata = Plata(f, 40);
        var vizibila = Factura(70);
        var id = Partida(f, ContFurnizor).Value;
        var conexiune = CuSpatiu(os => ((EFCoreObjectSpace)os).DbContext.Database.GetConnectionString());
        var admin = Marcaj + "-ADMIN"; var cititor = Marcaj + "-CITITOR";
        try {
            Comanda(os => {
                foreach (var nume in new[] { admin, cititor }) {
                    var rol = os.CreateObject<PermissionPolicyRole>(); rol.Name = nume;
                    rol.IsAdministrative = nume == admin;
                    rol.PermissionPolicy = SecurityPermissionPolicy.ReadOnlyAllByDefault;
                    if (nume == cititor) {
                        rol.AddObjectPermissionFromLambda<C.Postare>(SecurityOperations.Read, p => p.DocumentId == f, SecurityPermissionState.Deny);
                        rol.AddObjectPermissionFromLambda<Document>(SecurityOperations.Read, d => d.ID == f, SecurityPermissionState.Deny);
                    }
                    var user = os.CreateObject<ApplicationUser>(); user.UserName = nume; user.SetPassword(""); user.Roles.Add(rol);
                    os.CommitChanges();
                    ((ISecurityUserWithLoginInfo)user).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, os.GetKeyValueAsString(user));
                }
                os.CommitChanges();
            });
            using (var sesiune = new SesiuneOrigini(conexiune, cititor))
            using (var os = sesiune.Deschide()) {
                Verifica("SC-D9B-10", "rolul vede plata, nu suportul facturii", P.Postari(os).Any(p => p.DocumentId == plata)
                    && !P.Postari(os).Any(p => p.DocumentId == f));
                var r = ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: Ianuarie).Single(x => x.UnitateId == id);
                Verifica("SC-D9B-10", "originea și eticheta ascunse, valoarea vizibilă 40 păstrată", !P.Origini(os).Any(o => o.UnitateId == id)
                    && r.DocumentId == null && r.Rest == 40);
                var a = D9bCititorA.Origini(os).Where(o => o.PartenerId == Furnizor).OrderBy(o => o.UnitateId).ToArray();
                var b = P.Origini(os).Where(o => o.PartenerId == Furnizor).OrderBy(o => o.UnitateId).ToArray();
                Verifica("SC-D9B-10", "A/B pe rol restrictiv: aceeași origine vizibilă70, fără originea ascunsă100",
                    a.Length == 1 && a[0].DocumentId == vizibila && a[0].UnitateId != id
                    && System.Text.Json.JsonSerializer.Serialize(a) == System.Text.Json.JsonSerializer.Serialize(b)
                    && ImperecheriProiectii.PartideCuRest(os, Furnizor, laData: Ianuarie).Single(x => x.DocumentId == vizibila).Rest == 70);
                var sql = CapturaSql.Comenzi(() => _ = P.Origini(os).Where(o => o.PartenerId == Furnizor).Take(50).ToArray());
                Verifica("SC-D9B-10", "vizibilitatea este EXISTS în SQL, fără hash și fără ramură de nul", sql.Any(s => s.Contains("EXISTS")
                    && s.Contains("\"OriginiPartide\"") && s.Contains("LIMIT"))
                    && !sql.Any(s => s.Contains("cub_partida_id")) && !sql.SelectMany(ProbeRamuraDeNul.Gaseste).Any());
                using var sistem = sesiune.Sistem();
                var direct = CapturaSql.Comenzi(() => _ = P.Origini(sistem).Where(o => o.PartenerId == Furnizor).Take(50).ToArray());
                Verifica("SC-D9B-10", "ușa de sistem a hostului citește originea direct, fără Postare, inclusiv pe cea ascunsă rolului",
                    direct.Any(s => s.Contains("\"OriginiPartide\"")) && !direct.Any(s => s.Contains("\"Postare\""))
                    && P.Origini(sistem).Any(o => o.UnitateId == id));
            }
            using var administrator = new SesiuneOrigini(conexiune, admin);
            foreach (var operatie in new[] { "Create", "Write", "Delete" }) {
                var refuz = false;
                using (var os = administrator.Deschide()) {
                    var o = operatie == "Create" ? os.CreateObject<C.OriginePartidaPersistata>()
                        : os.GetObjectByKey<C.OriginePartidaPersistata>(id);
                    if (operatie == "Create") { o.ID = Guid.NewGuid(); o.ContId = Cont(ContFurnizor); o.PartenerId = Furnizor; o.DocumentId = f; }
                    if (operatie == "Write") o.PartenerId = Client;
                    if (operatie == "Delete") os.Delete(o);
                    try { os.CommitChanges(); }
                    catch (Exception e) when (e.Message.Contains("cubului se scriu doar de motor")) { refuz = true; }
                }
                Verifica("SC-D9B-10", $"admin securizat: {operatie} refuzat fără efecte", refuz && CuSpatiu(os => {
                    var origini = os.GetObjectsQuery<C.OriginePartidaPersistata>().Where(o => o.DocumentId == f).ToArray();
                    return origini.Length == 1 && origini[0].ID == id && origini[0].PartenerId == Furnizor;
                }));
            }
        }
        finally {
            Comanda(os => {
                os.Delete(os.GetObjectsQuery<ApplicationUser>().Where(u => u.UserName == admin || u.UserName == cititor).ToList());
                os.Delete(os.GetObjectsQuery<PermissionPolicyRole>().Where(r => r.Name == admin || r.Name == cititor).ToList());
                os.CommitChanges();
            });
        }
    }
}
