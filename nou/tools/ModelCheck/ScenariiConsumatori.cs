using System.ComponentModel;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.UI;
using DevExpress.Data.Filtering;
using DevExpress.EntityFrameworkCore.Security;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using C = Atlas.Conta.BackOffice.Module.Cub;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>Consumatorii din produs pe cub (TR-D9a pasul 4): gardul scrierii, drepturile rolurilor, lista XAF, refuzurile de ștergere.</summary>
sealed class ScenariiConsumatori(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide, string conexiune)
    : ScenaDocumente(deschide, check, privat, inchide, "P4", 1995) {
    const string FrazaGard = "cubului se scriu doar de motor";
    string Admin => Marcaj + "-ADMIN";

    sealed class Sesiune : IDisposable {
        readonly SecuredEFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext> furnizor;
        readonly IObjectSpace logon;
        public SecurityStrategyComplex Strategie { get; }

        public Sesiune(string conexiune, string utilizator) {
            var autentificare = new AuthenticationStandard(typeof(ApplicationUser), typeof(AuthenticationStandardLogonParameters));
            Strategie = new SecurityStrategyComplex(typeof(ApplicationUser), typeof(PermissionPolicyRole), autentificare);
            furnizor = new(Strategie, (b, _) => b.UseNpgsql(conexiune).UseChangeTrackingProxies()
                .UseObjectSpaceLinkProxies().UseLazyLoadingProxies());
            autentificare.SetLogonParameters(new AuthenticationStandardLogonParameters(utilizator, ""));
            logon = furnizor.CreateNonsecuredObjectSpace();
            Strategie.Logon(logon);
        }

        public IObjectSpace Securizat() {
            var os = furnizor.CreateObjectSpace();
            new GardianEditare(Strategie).OnObjectSpaceCreated(os);
            return os;
        }

        public IObjectSpace Sistem() => furnizor.CreateNonsecuredObjectSpace();

        public void Dispose() { logon.Dispose(); furnizor.Dispose(); }
    }

    sealed record Brut(Guid Tranzactie, Guid Postare);

    protected override void Executa() {
        CreeazaAdmin();
        try {
            var operata = Receptioneaza(new LinieFctScena(10, 10));
            Gard(operata.Id);
            Roluri();
            Lista(operata.Id);
            Vizual(operata);
            LotCuMiscari();
            TipTvaReferit();
            FisaCuMiscari();
        }
        finally { StergeAdmin(); }
    }

    protected override void CurataCubSuplimentar(IObjectSpace os, Purja purja) {
        var brute = os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId == null && t.Fel == N.FelTranzactie.Operare
            && t.Data >= new DateOnly(An, 1, 1) && t.Data <= new DateOnly(An, 12, 31)).Select(t => t.ID).ToList();
        purja.AdaugaCheie<C.Postare>(os.GetObjectsQuery<C.Postare>().Where(p => brute.Contains(p.TranzactieId)).Select(p => p.ID).ToList());
        purja.AdaugaCheie<C.Tranzactie>(brute);
    }

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        purja.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod.StartsWith(Marcaj)));

    void CreeazaAdmin() => Comanda(os => {
        if (os.FirstOrDefault<ApplicationUser>(u => u.UserName == Admin) != null) return;
        var rol = os.CreateObject<PermissionPolicyRole>(); rol.Name = Admin; rol.IsAdministrative = true;
        var u = os.CreateObject<ApplicationUser>(); u.UserName = Admin; u.SetPassword(""); u.Roles.Add(rol);
        os.CommitChanges();
        ((ISecurityUserWithLoginInfo)u).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, os.GetKeyValueAsString(u));
        os.CommitChanges();
    });

    void StergeAdmin() => Comanda(os => {
        foreach (var u in os.GetObjectsQuery<ApplicationUser>().Where(u => u.UserName == Admin).ToList()) os.Delete(u);
        foreach (var r in os.GetObjectsQuery<PermissionPolicyRole>().Where(r => r.Name == Admin).ToList()) os.Delete(r);
        os.CommitChanges();
    });

    C.Postare PostareBruta(IObjectSpace os, Guid tranzactie, Action<C.Postare> completeaza = null) {
        var p = os.CreateObject<C.Postare>();
        p.TranzactieId = tranzactie; p.Spatiu = N.Spatiu.Contabil; p.Data = Ianuarie; p.Cont = Cont(Serviciu);
        p.Latura = N.Latura.Debit; p.Carte = N.Carte.Contabil; p.Valoare = 1m;
        completeaza?.Invoke(p);
        return p;
    }

    static C.Tranzactie TranzactieBruta(IObjectSpace os, DateOnly data) {
        var t = os.CreateObject<C.Tranzactie>();
        t.Fel = N.FelTranzactie.Operare; t.Data = data; t.ScrisLa = DateTime.UtcNow;
        return t;
    }

    Brut ScrieBrut(IObjectSpace os, Action<C.Postare> completeaza = null) {
        var t = TranzactieBruta(os, Ianuarie);
        var p = PostareBruta(os, t.ID, completeaza);
        os.CommitChanges();
        return new(t.ID, p.ID);
    }

    void StergeBrut(Brut brut) => Comanda(os => {
        os.Delete(os.GetObjectsQuery<C.Postare>().Where(p => p.ID == brut.Postare).ToList());
        os.Delete(os.GetObjectsQuery<C.Tranzactie>().Where(t => t.ID == brut.Tranzactie).ToList());
        os.CommitChanges();
    });

    bool ExistaTranzactie(Guid id) => CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Any(t => t.ID == id));
    bool ExistaPostare(Guid id) => CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Any(p => p.ID == id));

    // D9-D9: șase scrieri refuzate pe ușa securizată, cu administratorul; aceeași scriere trece pe ușa de sistem.
    void Gard(Guid operata) {
        using var admin = new Sesiune(conexiune, Admin);
        var (tranzactie, data) = CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Where(t => t.DocumentId == operata)
            .Select(t => new { t.ID, t.Data }).ToList().Select(t => (t.ID, t.Data)).First());
        var (postare, valoare) = CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.TranzactieId == tranzactie)
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Valoare }).ToList().Select(p => (p.ID, p.Valoare)).First());

        string Refuz(Action<IObjectSpace> scrie) {
            using var os = admin.Securizat();
            try { scrie(os); os.CommitChanges(); return null; }
            catch (OperareException e) { return e.Message; }
            catch (Exception e) { return $"{e.GetType().Name}: {e.Message}"; }
        }
        void Caz(int numar, string ce, string refuz, bool neatins) {
            Console.WriteLine($"     MĂSURAT (D9-P4-GARD-{numar}): „{refuz?.Split('\n')[0] ?? "ACCEPTATĂ"}”.");
            Verifica($"D9-P4-GARD-{numar}", $"{ce} pe ușa securizată, cu administratorul: refuzată la commit, baza neatinsă",
                refuz?.Contains(FrazaGard) == true && neatins);
        }

        Guid nou = default;
        var refuz = Refuz(os => nou = TranzactieBruta(os, Ianuarie).ID);
        Caz(1, "crearea unei `Tranzactie`", refuz, nou != default && !ExistaTranzactie(nou));
        nou = default;
        refuz = Refuz(os => nou = PostareBruta(os, tranzactie).ID);
        Caz(2, "crearea unei `Postare`", refuz, nou != default && !ExistaPostare(nou));
        refuz = Refuz(os => { var t = os.GetObjectsQuery<C.Tranzactie>().Single(t => t.ID == tranzactie); t.Data = t.Data.AddDays(1); });
        Caz(3, "modificarea unei `Tranzactie`", refuz,
            CuSpatiu(os => os.GetObjectsQuery<C.Tranzactie>().Where(t => t.ID == tranzactie).Select(t => t.Data).Single()) == data);
        refuz = Refuz(os => { var p = os.GetObjectsQuery<C.Postare>().Single(p => p.ID == postare); p.Valoare += 1m; });
        Caz(4, "modificarea unei `Postare`", refuz,
            CuSpatiu(os => os.GetObjectsQuery<C.Postare>().Where(p => p.ID == postare).Select(p => p.Valoare).Single()) == valoare);
        refuz = Refuz(os => os.Delete(os.GetObjectsQuery<C.Postare>().Single(p => p.ID == postare)));
        Caz(5, "ștergerea unei `Postare`", refuz, ExistaPostare(postare));
        refuz = Refuz(os => os.Delete(os.GetObjectsQuery<C.Tranzactie>().Single(t => t.ID == tranzactie)));
        Caz(6, "ștergerea unei `Tranzactie`", refuz, ExistaTranzactie(tranzactie));

        Brut brut = null;
        string eroare = null;
        try {
            using var os = admin.Sistem();
            brut = ScrieBrut(os);
        }
        catch (Exception e) { eroare = $"{e.GetType().Name}: {e.Message}"; }
        Verifica("D9-P4-MOTOR-1", "aceeași creare de `Tranzactie` și `Postare`, pe ușa de sistem a aceluiași furnizor securizat, se scrie"
            + (eroare == null ? "" : $" — {eroare}"), brut != null && ExistaTranzactie(brut.Tranzactie) && ExistaPostare(brut.Postare));
        if (brut != null) StergeBrut(brut);

        var serviciu = Factura(Ianuarie, new LinieFctScena(1, 5, Stoc: false));
        eroare = null;
        try {
            using var os = admin.Sistem();
            ComenziDocument.Sistem(os).Opereaza(serviciu.Id);
        }
        catch (Exception e) { eroare = $"{e.GetType().Name}: {e.Message}"; }
        Verifica("D9-P4-MOTOR-2", "motorul operează un document pe ușa de sistem a furnizorului securizat și îi scrie postările"
            + (eroare == null ? "" : $" — {eroare}"), CuSpatiu(os =>
                os.GetObjectsQuery<C.Tranzactie>().Any(t => t.DocumentId == serviciu.Id)
                && os.GetObjectsQuery<C.Postare>().Any(p => p.DocumentId == serviciu.Id)));
    }

    // Pin 5: drepturile rolurilor bazei pe cub, citite din strategia de securitate.
    void Roluri() {
        Comanda(os => { Updater.SeedRolConfigurator(os); os.CommitChanges(); });
        using var admin = new Sesiune(conexiune, Admin);
        using var os = admin.Sistem();
        Type[] cub = [typeof(C.Postare), typeof(C.Tranzactie)];
        var roluri = os.GetObjectsQuery<PermissionPolicyRole>().ToList().Where(r => r.Name != Admin).OrderBy(r => r.Name).ToList();
        var s = admin.Strategie;
        bool Citeste(PermissionPolicyRole rol, Type tip) => s.CanReadByRole(rol, tip, os);
        bool Scrie(PermissionPolicyRole rol, Type tip) =>
            s.CanCreateByRole(rol, tip, os) || s.CanWriteByRole(rol, tip, os) || s.CanDeleteByRole(rol, tip, os);
        string Drepturi(PermissionPolicyRole rol, Type tip) => (Citeste(rol, tip) ? "R" : "-") + (s.CanCreateByRole(rol, tip, os) ? "C" : "-")
            + (s.CanWriteByRole(rol, tip, os) ? "W" : "-") + (s.CanDeleteByRole(rol, tip, os) ? "D" : "-");
        foreach (var rol in roluri)
            Console.WriteLine($"     MĂSURAT (D9-P4-ROL): {rol.Name} (administrativ {rol.IsAdministrative}, politica {rol.PermissionPolicy}): "
                + string.Join(", ", cub.Prepend(typeof(Document)).Select(t => $"{t.Name} {Drepturi(rol, t)}")));
        var cititori = roluri.Where(r => Citeste(r, typeof(Document))).ToList();
        Verifica("D9-P4-ROL-1", $"fiecare rol care citește documentele ({string.Join(", ", cititori.Select(r => r.Name))}) "
            + "citește `Postare` și `Tranzactie`", cititori.Count > 0 && cititori.All(r => cub.All(t => Citeste(r, t))));
        var scriitori = roluri.Where(r => !r.IsAdministrative && cub.Any(t => Scrie(r, t))).Select(r => r.Name).ToList();
        Verifica("D9-P4-ROL-2", "niciun rol neadministrativ nu creează, modifică sau șterge `Postare` și `Tranzactie`"
            + (scriitori.Count == 0 ? "" : $" — scriu: {string.Join(", ", scriitori)}"), scriitori.Count == 0);
        var configurator = roluri.SingleOrDefault(r => r.Name == "Configurator");
        Verifica("D9-P4-ROL-3", "rolul Configurator citește `Postare` și `Tranzactie` și nu le scrie",
            configurator != null && cub.All(t => Citeste(configurator, t) && !Scrie(configurator, t)));
    }

    // D9-A12: lista de evidență pe view — paritatea, rândurile, etichetele, gardul la activare, gardianul.
    void Vizual(FacturaScena operata) {
        var model = CuSpatiu(os => ((EFCoreObjectSpace)os).DbContext.Model);
        var coloanePostare = model.FindEntityType(typeof(C.Postare)).GetProperties().Select(p => (p.Name, p.ClrType)).ToList();
        var coloaneVizual = model.FindEntityType(typeof(C.PostareVizual)).GetProperties().Select(p => (p.Name, p.ClrType)).ToHashSet();
        var lipsa = coloanePostare.Where(c => !coloaneVizual.Contains(c)).Select(c => c.Name).ToList();
        Console.WriteLine($"     MĂSURAT (STR-VIZUAL-1): {coloanePostare.Count} proprietăți mapate pe `Postare`, {coloaneVizual.Count} pe "
            + $"`PostareVizual`; lipsă [{string.Join(", ", lipsa)}]; view `{model.FindEntityType(typeof(C.PostareVizual)).GetViewName()}`.");
        Verifica("STR-VIZUAL-1", "fiecare proprietate mapată a lui `Postare` există pe `PostareVizual` cu același nume și tip",
            coloanePostare.Count > 40 && lipsa.Count == 0);

        var (postari, vizuale) = CuSpatiu(os => (os.GetObjectsQuery<C.Postare>().Count(), os.GetObjectsQuery<C.PostareVizual>().Count()));
        Console.WriteLine($"     MĂSURAT (STR-VIZUAL-2): {postari} postări, {vizuale} rânduri în view.");
        Verifica("STR-VIZUAL-2", "view-ul are exact câte un rând pentru fiecare postare din bază", postari > 0 && vizuale == postari);

        var virtuale = typeof(N.GestiuniVirtuale).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(Guid)).ToDictionary(p => (Guid)p.GetValue(null), p => p.Name);
        var (peVirtuala, partide) = CuSpatiu(os => (
            os.GetObjectsQuery<C.PostareVizual>().Where(p => p.DocumentId == operata.Id && p.Gestiune != null).ToList()
                .Where(p => virtuale.ContainsKey(p.Gestiune.Value)).ToList(),
            os.GetObjectsQuery<C.PostareVizual>().Where(p => p.DocumentId == operata.Id && p.FelUnitate == N.FelUnitate.Partida).ToList()));
        Console.WriteLine($"     MĂSURAT (STR-VIZUAL-3): FCT pe gestiune virtuală [{string.Join("; ", peVirtuala.Select(p => $"{p.ContSimbol} {p.GestiuneCod}"))}]; "
            + $"FCT pe partidă [{string.Join("; ", partide.Select(p => $"{p.ContSimbol} {p.UnitateCod}"))}].");
        Verifica("STR-VIZUAL-3", "eticheta gestiunii virtuale e numele constantei nucleului (FCT: Furnizor), iar unitatea de "
            + "partidă e codul partenerului și data deschiderii, ISO (FCT pe furnizor)",
            peVirtuala.Count > 0 && peVirtuala.All(p => p.GestiuneCod == virtuale[p.Gestiune.Value])
            && peVirtuala.Any(p => p.GestiuneCod == nameof(N.GestiuniVirtuale.Furnizor))
            && partide.Count > 0 && partide.All(p => p.PartenerCod != null
                && p.UnitateCod == $"{p.PartenerCod}/{p.UnitateDeschisa:yyyy-MM-dd}" && p.ContSimbol.StartsWith("401")));

        var restrictionat = Marcaj + "-RAND";
        Comanda(os => {
            var rol = os.CreateObject<PermissionPolicyRole>(); rol.Name = restrictionat;
            rol.PermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy.ReadOnlyAllByDefault;
            rol.AddObjectPermissionFromLambda<C.Postare>(SecurityOperations.Read, p => p.Valoare > 100m,
                DevExpress.Persistent.Base.SecurityPermissionState.Deny);
            var u = os.CreateObject<ApplicationUser>(); u.UserName = restrictionat; u.SetPassword(""); u.Roles.Add(rol);
            os.CommitChanges();
            ((ISecurityUserWithLoginInfo)u).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, os.GetKeyValueAsString(u));
            os.CommitChanges();
        });
        try {
            (List<string> Lipsuri, int Randuri, string Refuz) Lista(string utilizator) {
                using var sesiune = new Sesiune(conexiune, utilizator);
                using var os = sesiune.Securizat();
                var lipsuri = C.Citiri.Vizibilitate.AccesLista(os, sesiune.Strategie);
                var cs = new CollectionSource(os, typeof(C.PostareVizual), CollectionSourceDataAccessMode.ServerView);
                var refuz = Atlas.Conta.BackOffice.Module.Controllers.PostareVizualController.Aplica(cs, lipsuri);
                return (lipsuri, ((IListSource)cs.Collection).GetList().Count, refuz);
            }
            var rand = Lista(restrictionat);
            var admin = Lista(Admin);
            Console.WriteLine($"     MĂSURAT (STR-VIZUAL-4): rolul cu criteriu de rând → lipsuri [{string.Join(", ", rand.Lipsuri)}], "
                + $"{rand.Randuri} rânduri, „{rand.Refuz}”; administratorul → lipsuri [{string.Join(", ", admin.Lipsuri)}], {admin.Randuri} rânduri.");
            Verifica("STR-VIZUAL-4", "un rol cu criteriu de rând pe `Postare` primește lipsuri și lista goală, cu fraza porților; "
                + "administratorul n-are lipsuri și vede rândurile",
                rand.Lipsuri.Count > 0 && rand.Randuri == 0 && rand.Refuz?.StartsWith("Nu aveți dreptul de a citi") == true
                && admin.Lipsuri.Count == 0 && admin.Randuri > 0 && admin.Refuz == null);
        }
        finally {
            Comanda(os => {
                foreach (var u in os.GetObjectsQuery<ApplicationUser>().Where(u => u.UserName == restrictionat).ToList()) os.Delete(u);
                foreach (var r in os.GetObjectsQuery<PermissionPolicyRole>().Where(r => r.Name == restrictionat).ToList()) {
                    foreach (var t in r.TypePermissions.ToList()) os.Delete(t);
                    os.Delete(r);
                }
                os.CommitChanges();
            });
        }

        using var sesiuneAdmin = new Sesiune(conexiune, Admin);
        var (idVizual, valoare) = CuSpatiu(os => os.GetObjectsQuery<C.PostareVizual>().Where(p => p.DocumentId == operata.Id)
            .OrderBy(p => p.ID).Select(p => new { p.ID, p.Valoare }).ToList().Select(p => (p.ID, p.Valoare)).First());
        string Refuz(Action<IObjectSpace> scrie) {
            using var os = sesiuneAdmin.Securizat();
            try { scrie(os); os.CommitChanges(); return null; }
            catch (OperareException e) { return e.Message; }
            catch (Exception e) { return $"{e.GetType().Name}: {e.Message}"; }
        }
        bool ExistaVizual(Guid id) => CuSpatiu(os => os.GetObjectsQuery<C.PostareVizual>().Any(p => p.ID == id));
        void Caz(int numar, string ce, string refuz, bool neatins) {
            Console.WriteLine($"     MĂSURAT (STR-VIZUAL-5.{numar}): „{refuz?.Split('\n')[0] ?? "ACCEPTATĂ"}”.");
            Verifica($"STR-VIZUAL-5.{numar}", $"{ce} pe ușa securizată, cu administratorul: refuzată la commit, baza neatinsă",
                refuz?.Contains(FrazaGard) == true && neatins);
        }
        Guid nou = default;
        var refuzCreare = Refuz(os => {
            var p = os.CreateObject<C.PostareVizual>(); p.ID = Guid.NewGuid(); nou = p.ID;
            p.TranzactieId = Guid.NewGuid(); p.Data = Ianuarie; p.Cont = Cont(Serviciu); p.Valoare = 1m;
        });
        Caz(1, "crearea unei `PostareVizual`", refuzCreare, nou != default && !ExistaVizual(nou));
        var refuzModificare = Refuz(os => { var p = os.GetObjectsQuery<C.PostareVizual>().Single(p => p.ID == idVizual); p.Valoare += 1m; });
        Caz(2, "modificarea unei `PostareVizual`", refuzModificare,
            CuSpatiu(os => os.GetObjectsQuery<C.PostareVizual>().Where(p => p.ID == idVizual).Select(p => p.Valoare).Single()) == valoare);
        var refuzStergere = Refuz(os => os.Delete(os.GetObjectsQuery<C.PostareVizual>().Single(p => p.ID == idVizual)));
        Caz(3, "ștergerea unei `PostareVizual`", refuzStergere, ExistaVizual(idVizual));
    }

    // D9-D9: o pagină ServerView pe coloanele listei, cu rânduri din amândouă partițiile, regăsite după cheie.
    void Lista(Guid operata) {
        var coloane = ContaUiBaseline.ColoanePostari.Select(c => c.Cale).ToArray();
        using var furnizor = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>((b, _) => b.UseNpgsql(conexiune)
            .UseChangeTrackingProxies().UseObjectSpaceLinkProxies().UseLazyLoadingProxies().UseXafCalculatedProperties()
            .UseXafServiceProviderContainer(new ServiceCollection().BuildServiceProvider()));
        using var os = furnizor.CreateObjectSpace();
        string eroare = null;
        var spatii = new HashSet<N.Spatiu>();
        int randuri = 0, regasite = 0;
        string[] proiectate = [];
        try {
            var cs = new CollectionSource(os, typeof(C.Postare), CollectionSourceDataAccessMode.ServerView);
            cs.Criteria["document"] = CriteriaOperator.Parse("DocumentId = ?", operata);
            cs.DisplayableProperties = string.Join(";", coloane);
            var lista = ((IListSource)cs.Collection).GetList();
            proiectate = cs.DisplayableProperties.Split(';');
            for (var i = 0; i < lista.Count; i++) {
                if (lista[i] is not XafDataViewRecord rand) { eroare ??= "rând null: server-mode a eșuat tăcut"; break; }
                foreach (var nume in coloane) _ = rand[nume];
                randuri++;
                if (os.GetObject((object)rand) is C.Postare p && p.DocumentId == operata) { regasite++; spatii.Add(p.Spatiu); }
            }
        }
        catch (Exception e) { eroare = $"{e.GetType().Name}: {e.Message}"; }
        var asteptate = CuSpatiu(x => x.GetObjectsQuery<C.Postare>().Count(p => p.DocumentId == operata));
        Console.WriteLine($"     MĂSURAT (D9-P4-LISTA-2): cerute [{string.Join(";", coloane)}], proiectate [{string.Join(";", proiectate)}], "
            + $"{randuri} rânduri din {asteptate}, {regasite} regăsite după cheie, spații [{string.Join(", ", spatii.Order())}]"
            + (eroare == null ? "" : $"; EROARE {eroare}"));
        Verifica("D9-P4-LISTA-2", "o pagină ServerView pe `Postare`, pe coloanele listei: toate rămân în proiecție, rândurile unui "
            + "document operat vin din amândouă partițiile și fiecare se regăsește după cheie",
            eroare == null && coloane.All(proiectate.Contains) && randuri == asteptate && regasite == randuri
            && spatii.SetEquals([N.Spatiu.Contabil, N.Spatiu.Stoc]));
    }

    FacturaScena FacturaAnulata() {
        var f = Factura(Ianuarie, new LinieFctScena(10, 10));
        var nir = Opereaza(f.Id).ConexId!.Value;
        Opereaza(nir); Anuleaza(nir); Anuleaza(f.Id);
        return f;
    }

    // I4: lotul propriu al liniei șterse rămâne cât are postări.
    void LotCuMiscari() {
        bool Exista(Guid? lot) => CuSpatiu(os => os.GetObjectsQuery<Lot>().Any(l => l.ID == lot));
        bool Finalizat(Guid? lot) => CuSpatiu(os => os.GetObjectsQuery<Lot>().Any(l => l.ID == lot && l.PretUnitar != 0m));

        var liber = FacturaAnulata();
        var lotLiber = liber.Linii[0].Lot;
        var premisa = Finalizat(lotLiber) && CuSpatiu(os => !C.Citiri.Loturi.AreMiscari(os, lotLiber!.Value));
        Comanda(os => FacturaIntrareApply.Sterge(os, liber.Id));
        Verifica("D9-P4-I4-LOT-1", "trece: lotul finalizat fără nicio postare se șterge odată cu linia lui", premisa && !Exista(lotLiber));

        var purtat = FacturaAnulata();
        var lot = purtat.Linii[0];
        var brut = CuSpatiu(os => ScrieBrut(os, p => {
            p.Spatiu = N.Spatiu.Stoc; p.Cont = Cont(Stoc); p.FelUnitate = N.FelUnitate.Lot; p.Unitate = lot.Lot;
            p.UnitateDeschisa = Ianuarie; p.Produs = lot.Produs; p.Gestiune = Magazie; p.Cantitate = 1m; p.Valoare = 10m;
        }));
        premisa = Finalizat(lot.Lot) && CuSpatiu(os => C.Citiri.Loturi.AreMiscari(os, lot.Lot!.Value));
        Comanda(os => FacturaIntrareApply.Sterge(os, purtat.Id));
        Verifica("D9-P4-I4-LOT-2", "refuză: lotul cu o postare în cub (și niciun rând de registru) rămâne după ștergerea liniei lui",
            premisa && Exista(lot.Lot) && CuSpatiu(os => !os.GetObjectsQuery<Document>().Any(d => d.ID == purtat.Id)));
        StergeBrut(brut);
    }

    string RefuzStergere<T>(Guid id) where T : EntitateConta {
        using var os = Deschide();
        os.Delete(os.GetObjectsQuery<T>().Single(x => x.ID == id));
        try { GardianEditare.Verifica(os); return null; }
        catch (OperareException e) { return e.Message; }
    }

    // I4: tipul de TVA purtat de o postare nu se șterge.
    void TipTvaReferit() {
        var tip = CuSpatiu(os => {
            var t = os.CreateObject<TipTva>(); t.Cod = Marcaj + "-TVA"; t.Denumire = t.Cod; t.Cota = 21m; t.Regim = RegimTva.Normal;
            os.CommitChanges(); return t.ID;
        });
        var liber = RefuzStergere<TipTva>(tip);
        Verifica("D9-P4-I4-TVA-1", "trece: tipul de TVA nereferit se poate șterge" + (liber == null ? "" : $" — {liber}"), liber == null);

        var martor = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 1m));
        var perioada = An * 100 + 1;
        var brut = CuSpatiu(os => ScrieBrut(os, p => {
            p.DocumentId = martor.Id; p.LinieId = martor.Linii[0].Id; p.TipTvaId = tip;
            p.SensTva = N.SensTva.Achizitie; p.RolTva = N.RolTva.Baza; p.RegimTva = N.RegimTva.Normal; p.CotaTva = 21m; p.DeImport = false;
            p.DocumentFiscalId = martor.Id; p.DataDocument = Ianuarie; p.DataExigibilitate = Ianuarie; p.DataPrimire = Ianuarie;
            p.DataInregistrare = Ianuarie; p.PerioadaDeclarare = perioada; p.PerioadaD394 = perioada;
        }));
        var refuz = RefuzStergere<TipTva>(tip);
        Console.WriteLine($"     MĂSURAT (D9-P4-I4-TVA-2): „{refuz?.Split('\n')[0] ?? "ACCEPTATĂ"}”.");
        Verifica("D9-P4-I4-TVA-2", "refuză: tipul de TVA purtat de o postare (fără linie de document și fără rând de registru) nu se șterge",
            refuz?.Contains("rânduri din jurnalul de TVA") == true && !refuz.Contains("linii de document"));
        StergeBrut(brut);
    }

    // I4: fișa care e unitatea unei postări nu se șterge.
    void FisaCuMiscari() {
        var fisa = CuSpatiu(os => {
            var f = os.CreateObject<Imobilizare>(); f.NumarInventar = Marcaj + "-F1"; f.Denumire = f.NumarInventar;
            f.TipMaterialId = Tip(os, Privat ? "214" : "214.00.00"); f.LocId = Magazie; f.CodEconomicId = Economic;
            os.CommitChanges(); return f.ID;
        });
        var liber = RefuzStergere<Imobilizare>(fisa);
        Verifica("D9-P4-I4-FISA-1", "trece: fișa Nouă fără postări și fără linii se poate șterge" + (liber == null ? "" : $" — {liber}"),
            liber == null);

        var brut = CuSpatiu(os => ScrieBrut(os, p => {
            p.FelUnitate = N.FelUnitate.Fisa; p.Unitate = fisa; p.UnitateDeschisa = Ianuarie;
        }));
        var refuz = RefuzStergere<Imobilizare>(fisa);
        Console.WriteLine($"     MĂSURAT (D9-P4-I4-FISA-2): „{refuz?.Split('\n')[0] ?? "ACCEPTATĂ"}”.");
        Verifica("D9-P4-I4-FISA-2", "refuză: fișa Nouă care e unitatea unei postări (fără niciun rând de registru) nu se șterge",
            refuz?.Contains("are rânduri de registru") == true);
        StergeBrut(brut);
    }
}
