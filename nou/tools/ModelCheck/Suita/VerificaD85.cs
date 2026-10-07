using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

// ---------------------------------------------------------------------------
// D85 — modul de acces al ListView-urilor XAF Blazor (decizia 85)
// ---------------------------------------------------------------------------
// M1/M2 citesc modelul REAL al aplicației (module + `Model.xafml` al hostului);
// R1–R3 rulează colecțiile exact cum le creează XAF (`CollectionSource` /
// `PropertyCollectionSource` pe `IObjectSpace`), cu comenzile SQL numărate.
static class VerificaD85 {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        const string Marcaj = "D85";
        var sql = NumaratorSql.Instanta;
        static string Mesaj(Action actiune) {
            try { actiune(); return "fără excepție"; }
            catch (Exception e) { return $"{e.GetType().Name}: {e.Message}"; }
        }

        // ── modelul aplicației ───────────────────────────────────────────────────
        IModelApplication model = null;
        try {
            model = ModelAplicatie.Incarca(s.ConnectionString, s.Profil);
        }
        catch (Exception ex) {
            Console.WriteLine($"     D85: modelul aplicației nu s-a construit — {ex.GetType().Name}: {ex.Message}");
        }
        s.Check($"D85-M0 ({eticheta}) modelul aplicației XAF Blazor se construiește fără circuit (host + `Setup()`, "
            + "aceeași cale ca `--updateDatabase`)", model != null);
        var listViews = model?.Views.OfType<IModelListView>().ToList() ?? [];
        IModelListView Lv(string id) => listViews.FirstOrDefault(v => v.Id == id);
        // Ce cere grila Blazor pentru o coloană vizibilă (`DxGridListEditorBase.RequiredProperties`
        // + `ListEditor.GetDisplayablePropertyName`): calea spre DefaultProperty-ul referinței.
        static bool Vizibila(IModelColumn c) => !c.Index.HasValue || c.Index > -1 || c.GroupIndex > -1;
        static string NumeAfisat(ITypeInfo ti, string proprietate)
            => ReflectionHelper.FindDisplayableMemberDescriptor(ti, proprietate)?.BindingName ?? proprietate;
        string[] Coloane(IModelListView lv) => lv.Columns.Where(Vizibila)
            .Select(c => NumeAfisat(lv.ModelClass.TypeInfo, c.PropertyName)).Distinct().ToArray();

        // ---- D85-M1: modurile per view ----
        string[] culegere = [
            nameof(FacturaIntrareDetaliu), nameof(NirDetaliu), nameof(FacturaIesireDetaliu),
            nameof(ListaDiferenteInventarDetaliu), nameof(DecontDetaliu), nameof(DescarcareGestiuneDetaliu),
            nameof(NotaContabilaDetaliu), nameof(AsamblareDetaliu)];
        var moduriCulegere = culegere.Select(n => (Id: n + "_ListView", Mod: Lv(n + "_ListView")?.DataAccessMode)).ToList();
        var generic = Lv(nameof(Document) + "_" + nameof(Document.Detalii) + "_ListView")?.DataAccessMode;
        var radacini = (model?.BOModel ?? Enumerable.Empty<IModelClass>())
            .Where(c => c.TypeInfo?.Type != null && typeof(Document).IsAssignableFrom(c.TypeInfo.Type)
                && !c.TypeInfo.IsAbstract && c.DefaultListView != null)
            .Select(c => (c.Name, c.DefaultListView.Id, Mod: c.DefaultListView.DataAccessMode)).ToList();
        var dataView = listViews.Where(v => v.DataAccessMode == CollectionSourceDataAccessMode.DataView).Select(v => v.Id).ToList();
        Console.WriteLine($"     MĂSURAT (D85-M1/{eticheta}): Options={model?.Options.DataAccessMode}; "
            + $"{listViews.Count} ListView-uri; culegere: {string.Join(", ", moduriCulegere.Select(m => $"{m.Id}={m.Mod}"))}; "
            + $"Document_Detalii_ListView={generic}; rădăcini de document: {radacini.Count} "
            + $"({string.Join("/", radacini.Select(r => r.Mod).Distinct())}); DataView: {dataView.Count}");
        s.Check($"D85-M1 ({eticheta}) `Options.DataAccessMode` = Server; cele opt grile de culegere tipizate și "
            + "`Document_Detalii_ListView` sunt Client explicit; ListView-ul implicit al fiecărei clase de document "
            + "e Server; niciun view nu e DataView",
            model != null && model.Options.DataAccessMode == CollectionSourceDataAccessMode.Server
            && moduriCulegere.All(m => m.Mod == CollectionSourceDataAccessMode.Client)
            && generic == CollectionSourceDataAccessMode.Client
            && radacini.Count > 0 && radacini.All(r => r.Mod == CollectionSourceDataAccessMode.Server)
            && dataView.Count == 0);

        ProbeLinii.Verifica(model, eticheta, s.Check);

        // ---- D85-M2: precondițiile modurilor de VIEW (ServerView / InstantFeedbackView) ----
        var referite = (model?.Views.OfType<IModelDetailView>() ?? [])
            .SelectMany(dv => dv.Items.OfType<IModelMemberViewItem>())
            .Select(i => i.View?.Id).Where(id => id != null).ToHashSet();
        var serverViews = listViews
            .Where(v => v.DataAccessMode is CollectionSourceDataAccessMode.ServerView or CollectionSourceDataAccessMode.InstantFeedbackView)
            .ToList();
        var probleme = new List<string>();
        foreach (var lv in serverViews) {
            var ti = lv.ModelClass.TypeInfo;
            foreach (var col in lv.Columns.Where(Vizibila)) {
                var nume = NumeAfisat(ti, col.PropertyName);
                var mi = ti.FindMember(nume);
                if (mi == null || !(mi.IsPersistent || mi.IsAliased))
                    probleme.Add($"{lv.Id}.{col.PropertyName} cere `{nume}`, care nu e mapat/[Calculated]");
                // O referință fără DefaultProperty afișabil cade pe cheie: coloana ar arăta un GUID.
                var tinta = ti.FindMember(col.PropertyName)?.MemberTypeInfo;
                if (tinta?.IsDomainComponent == true
                    && (tinta.DefaultMember == null || tinta.DefaultMember.IsKey
                        || !(tinta.DefaultMember.IsPersistent || tinta.DefaultMember.IsAliased)))
                    probleme.Add($"{lv.Id}.{col.PropertyName} → `{tinta.Name}` fără DefaultProperty afișabil");
            }
            var nested = referite.Contains(lv.Id) || model.BOModel.Any(c => c.AllMembers.Any(m => lv.Id == $"{c.Name}_{m.Name}_ListView"));
            if (nested)
                probleme.Add($"{lv.Id} e nested");
        }
        Console.WriteLine($"     MĂSURAT (D85-M2/{eticheta}): view-uri în mod de VIEW: "
            + $"{string.Join(", ", serverViews.Select(v => $"{v.Id}={v.DataAccessMode} [{string.Join(";", Coloane(v))}]"))}"
            + (probleme.Count > 0 ? $"; probleme: {string.Join("; ", probleme)}" : "; fără probleme"));
        s.Check($"D85-M2 ({eticheta}) pe fiecare ListView ServerView/InstantFeedbackView, tot ce cere grila pentru "
            + "coloanele vizibile (inclusiv DefaultProperty-ul referințelor) e mapat sau [Calculated] — altfel "
            + "`RemoveNotSupportedProperties` scoate tăcut coloana din proiecție și rândul aruncă "
            + "IndexOutOfRange — și niciunul nu e nested",
            model != null && probleme.Count == 0);

        // ── scena: trei loturi (unul în culegere) + două FCT draft ───────────────
        void CurataD85(IObjectSpace os) {
            new Purja(os)
                .Adauga(os.GetObjectsQuery<Document>().Where(d => d.Numar.StartsWith(Marcaj + "-")))
                .Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod == Marcaj))
                .Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod == Marcaj))
                .Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj + "-")))
                .Executa();
        }
        using (var os = s.Provider.CreateObjectSpace())
            CurataD85(os);
        Guid furnizorId, gestiuneId;
        using (var os = s.Provider.CreateObjectSpace()) {
            var produs = os.CreateObject<Produs>();
            produs.Cod = Marcaj;
            produs.Denumire = "Produs D85";
            produs.UM = "BUC";
            produs.TipMaterial = os.GetObjectsQuery<TipMaterial>().OrderBy(t => t.Cod).First();
            var gestiune = os.CreateObject<Gestiune>();
            gestiune.Cod = Marcaj + "-GEST";
            gestiune.Denumire = "Gestiune D85";
            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = Marcaj + "-FURN";
            furnizor.Denumire = "Furnizor D85";
            var lotCulegere = os.CreateObject<Lot>();
            lotCulegere.Produs = produs; lotCulegere.Gestiune = gestiune;
            var lotRotund = os.CreateObject<Lot>();
            lotRotund.Produs = produs; lotRotund.Gestiune = gestiune;
            lotRotund.Data = new DateOnly(2026, 3, 5); lotRotund.PretUnitar = 10m;
            var lotJumatate = os.CreateObject<Lot>();
            lotJumatate.Produs = produs; lotJumatate.Gestiune = gestiune;
            lotJumatate.Data = new DateOnly(2026, 11, 20); lotJumatate.PretUnitar = 12.34565m;
            for (var i = 1; i <= 2; i++) {
                var fct = os.CreateObject<FacturaIntrare>();
                fct.Numar = $"{Marcaj}-{i}";
                fct.Data = new DateOnly(2026, 3, i);
                fct.Predator = furnizor;
                fct.Primitor = gestiune;
            }
            os.CommitChanges();
            furnizorId = furnizor.ID;
            gestiuneId = gestiune.ID;
        }

        // ---- D85-R1: ServerView pe Lot — eticheta calculată în SELECT, identică cu getter-ul ----
        // Provider configurat ca hostul (`AddSecuredEFCore`): `UseXafCalculatedProperties` pune
        // expandarea `[Calculated]` în compilatorul de interogări EF — și în WHERE, și în SELECT.
        using var providerHost = new EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext>(
            (builder, _) => builder
                .UseNpgsql(s.ConnectionString)
                .UseChangeTrackingProxies()
                .UseObjectSpaceLinkProxies()
                .UseLazyLoadingProxies()
                .UseXafCalculatedProperties()
                .UseXafServiceProviderContainer(Microsoft.Extensions.DependencyInjection.ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(new Microsoft.Extensions.DependencyInjection.ServiceCollection()))
                .AddInterceptors(NumaratorSql.Instanta));
        var perechi = new List<(string Sql, string Cs)>();
        string selectEticheta = null, eroareR1 = null;
        var cautate = -1;
        using (var os = providerHost.CreateObjectSpace()) {
            try {
                var tiLot = os.TypesInfo.FindTypeInfo(typeof(Lot));
                var cs = new CollectionSource(os, typeof(Lot), CollectionSourceDataAccessMode.ServerView);
                cs.Criteria[Marcaj] = CriteriaOperator.Parse("Produs.Cod = ?", Marcaj);
                cs.DisplayableProperties = string.Join(";",
                    new[] { nameof(Lot.Eticheta), nameof(Lot.Produs), nameof(Lot.Data), nameof(Lot.PretUnitar) }
                        .Select(p => NumeAfisat(tiLot, p)));
                cs.Sorting = [new DevExpress.Xpo.SortProperty(nameof(Lot.Data), DevExpress.Xpo.DB.SortingDirection.Ascending)];
                sql.Reseteaza();
                var sursa = (DevExpress.Data.Linq.EntityServerModeSource)cs.Collection;
                sursa.ExceptionThrown += (_, e) => eroareR1 ??= $"server-mode: {e.Exception}";
                sursa.InconsistencyDetected += (_, e) => eroareR1 ??= $"server-mode (inconsistență)";
                var lista = ((System.ComponentModel.IListSource)sursa).GetList();
                for (var i = 0; i < Math.Min(lista.Count, 20); i++) {
                    // Un eșec de traducere e înghițit de server-mode (`ServerModeCache.Fatal`): rândul iese null.
                    if (lista[i] is not EFCoreServerModeViewRecord rand) {
                        eroareR1 ??= "rând null — server-mode a eșuat tăcut: " + Mesaj(() => ((DevExpress.Data.IListServer)lista).GetAllFilteredAndSortedRows());
                        break;
                    }
                    perechi.Add((rand[nameof(Lot.Eticheta)] as string, ((Lot)os.GetObject((object)rand)).Eticheta));
                }
                selectEticheta = sql.Comenzi.FirstOrDefault(c => c.Contains("round(", StringComparison.OrdinalIgnoreCase) && c.Contains("date_part", StringComparison.OrdinalIgnoreCase));
                // Căutarea din lookup / caseta de filtrare = criteriu pe DisplayMember, tradus în SQL.
                cautate = os.GetObjects(typeof(Lot), CriteriaOperator.Parse("Contains([Eticheta], ?) And Produs.Cod = ?", "culegere", Marcaj)).Count;
            }
            catch (Exception ex) {
                eroareR1 = $"{ex.GetType().Name}: {ex.Message}";
            }
        }
        Console.WriteLine($"     MĂSURAT (D85-R1/{eticheta}): {perechi.Count} rânduri — "
            + string.Join(" | ", perechi.Select(p => $"SQL „{p.Sql}” vs C# „{p.Cs}”"))
            + $"; Contains(Eticheta,'culegere') ⇒ {cautate}"
            + (eroareR1 != null ? $"; EROARE {eroareR1}" : ""));
        if (selectEticheta != null)
            Console.WriteLine($"     SQL (D85-R1): {selectEticheta.Replace("\n", " ").Replace("\r", "")}");
        s.Check($"D85-R1 ({eticheta}) o pagină ServerView pe `Lot` cu `Eticheta` [Calculated] se citește fără excepție, "
            + "eticheta vine din SELECT (date_part/round în SQL) și e IDENTICĂ literal cu getter-ul C# pe fiecare rând "
            + "(inclusiv „(în culegere)” și rotunjirea jumătății), iar `Contains(Eticheta, …)` se traduce — "
            + "căutarea pe lookup-ul de lot e vie",
            eroareR1 == null && perechi.Count == 3 && selectEticheta != null
            && perechi.All(p => !string.IsNullOrEmpty(p.Sql) && p.Sql == p.Cs)
            && perechi.Count(p => p.Sql.EndsWith("(în culegere)")) == 1
            && perechi.Any(p => p.Sql.EndsWith("12.3457"))
            && cautate == 1);

        // ---- D85-R1 (RegistruStoc_ListView real): proiecția păstrează tot ce cere grila ----
        var lvRs = Lv(nameof(RegistruStoc) + "_ListView");
        string eroareRs = null;
        var ceruteRs = lvRs == null ? [] : Coloane(lvRs);
        string[] proiectateRs = [];
        int randuriRs = 0, comenziRs = 0;
        if (lvRs != null)
            using (var os = providerHost.CreateObjectSpace()) {
                try {
                    var cs = new CollectionSource(os, typeof(RegistruStoc), lvRs.DataAccessMode);
                    cs.DisplayableProperties = string.Join(";", ceruteRs);
                    sql.Reseteaza();
                    var lista = ((System.ComponentModel.IListSource)cs.Collection).GetList();
                    proiectateRs = cs.DisplayableProperties.Split(';');
                    for (var i = 0; i < Math.Min(lista.Count, 20); i++) {
                        var rand = (XafDataViewRecord)lista[i];
                        foreach (var nume in ceruteRs)
                            _ = rand[nume];
                        randuriRs++;
                    }
                    comenziRs = sql.Numar;
                }
                catch (Exception ex) {
                    eroareRs = $"{ex.GetType().Name}: {ex.Message}";
                }
            }
        var lipsaRs = ceruteRs.Where(c => !proiectateRs.Contains(c)).ToList();
        Console.WriteLine($"     MĂSURAT (D85-R1/{eticheta}, RegistruStoc_ListView={lvRs?.DataAccessMode}): cerute "
            + $"[{string.Join(";", ceruteRs)}], proiectate [{string.Join(";", proiectateRs)}], {randuriRs} rânduri citite, "
            + $"{comenziRs} comenzi SQL" + (eroareRs != null ? $"; EROARE {eroareRs}" : ""));
        s.Check($"D85-R1 ({eticheta}) pe `RegistruStoc_ListView` real, fiecare coloană cerută de grilă rămâne în proiecție "
            + "și o pagină se citește pe toate coloanele fără excepție",
            lvRs != null && eroareRs == null && lipsaRs.Count == 0);

        // ---- D9-P4-LISTA-1 (Postare_ListView real): geamănul probei de pe `RegistruStoc_ListView` ----
        var lvPostari = Lv(nameof(Atlas.Conta.BackOffice.Module.Cub.Postare) + "_ListView");
        string eroarePostari = null;
        var cerutePostari = lvPostari == null ? [] : Coloane(lvPostari);
        string[] proiectatePostari = [];
        if (lvPostari != null)
            using (var os = providerHost.CreateObjectSpace()) {
                try {
                    var cs = new CollectionSource(os, typeof(Atlas.Conta.BackOffice.Module.Cub.Postare), lvPostari.DataAccessMode);
                    cs.DisplayableProperties = string.Join(";", cerutePostari);
                    var lista = ((System.ComponentModel.IListSource)cs.Collection).GetList();
                    proiectatePostari = cs.DisplayableProperties.Split(';');
                    for (var i = 0; i < Math.Min(lista.Count, 20); i++) {
                        var rand = (XafDataViewRecord)lista[i];
                        foreach (var nume in cerutePostari)
                            _ = rand[nume];
                    }
                }
                catch (Exception ex) {
                    eroarePostari = $"{ex.GetType().Name}: {ex.Message}";
                }
            }
        var asteptatePostari = Atlas.Conta.BackOffice.Module.UI.ContaUiBaseline.ColoanePostari.Select(c => c.Cale).ToArray();
        var vizibilePostari = lvPostari == null ? [] : lvPostari.Columns.Where(Vizibila).OrderBy(c => c.Index).Select(c => c.PropertyName).ToArray();
        var lvImbricata = Lv("Tranzactie_Postari_ListView");
        var navigarePostari = model == null ? [] : ((DevExpress.ExpressApp.SystemModule.IModelApplicationNavigationItems)model).NavigationItems.AllItems
            .Where(i => i.View?.Id == lvPostari?.Id).Select(i => (i.Parent?.Parent as DevExpress.ExpressApp.SystemModule.IModelNavigationItem)?.Caption ?? "").ToList();
        Console.WriteLine($"     MĂSURAT (D9-P4-LISTA-1/{eticheta}, Postare_ListView={lvPostari?.DataAccessMode}): vizibile "
            + $"[{string.Join(";", vizibilePostari)}], cerute [{string.Join(";", cerutePostari)}], proiectate [{string.Join(";", proiectatePostari)}], "
            + $"navigare [{string.Join(";", navigarePostari)}]; Tranzactie_Postari_ListView={lvImbricata?.DataAccessMode}, "
            + $"editare {lvImbricata?.AllowEdit}/{lvImbricata?.AllowNew}/{lvImbricata?.AllowDelete}"
            + (eroarePostari != null ? $"; EROARE {eroarePostari}" : ""));
        s.Check($"D9-P4-LISTA-1 ({eticheta}) `Postare_ListView` real e în mod ServerView, sub grupul de navigare „Registre”, cu exact "
            + "coloanele listei de evidență, fiecare rămasă în proiecție, iar o pagină se citește fără excepție; lista imbricată a "
            + "tranzacției e Client, fără editare",
            lvPostari != null && lvPostari.DataAccessMode == CollectionSourceDataAccessMode.ServerView && eroarePostari == null
            && vizibilePostari.SequenceEqual(asteptatePostari) && cerutePostari.All(proiectatePostari.Contains)
            && navigarePostari.Count == 1 && navigarePostari[0] == "Registre"
            && lvImbricata is { DataAccessMode: CollectionSourceDataAccessMode.Client, AllowEdit: false, AllowNew: false, AllowDelete: false });

        // ---- D85-R2: Server (entități) — pagina = interogare + COUNT, fără interogare per rând ----
        (int Pagina1, int Pagina2, int Randuri, string Eroare) Pagini(Type tip, IModelListView lv, bool prefetch) {
            using var os = s.Provider.CreateObjectSpace();
            ((EFCoreObjectSpace)os).PreFetchReferenceProperties = prefetch;
            try {
                var coloane = Coloane(lv);
                var ti = os.TypesInfo.FindTypeInfo(tip);
                var cs = new CollectionSource(os, tip, CollectionSourceDataAccessMode.Server);
                cs.DisplayableProperties = string.Join(";", coloane);
                cs.Sorting = [new DevExpress.Xpo.SortProperty(nameof(Document.Data), DevExpress.Xpo.DB.SortingDirection.Descending)];
                sql.Reseteaza();
                var lista = ((System.ComponentModel.IListSource)cs.Collection).GetList();
                var total = lista.Count;
                int Citeste(int de, int pana) {
                    sql.Reseteaza();
                    for (var i = de; i < Math.Min(pana, total); i++)
                        foreach (var nume in coloane)
                            _ = ti.FindMember(nume)?.GetValue(lista[i]);
                    return sql.Numar;
                }
                var p1 = Citeste(0, 20);
                var p2 = Citeste(20, 40);
                return (p1, p2, total, null);
            }
            catch (Exception ex) {
                return (-1, -1, 0, $"{ex.GetType().Name}: {ex.Message}");
            }
        }
        var lvFct = Lv(nameof(FacturaIntrare) + "_ListView");
        var lvLot = Lv(nameof(Lot) + "_ListView");
        var fctCu = lvFct == null ? default : Pagini(typeof(FacturaIntrare), lvFct, prefetch: true);
        var fctFara = lvFct == null ? default : Pagini(typeof(FacturaIntrare), lvFct, prefetch: false);
        var lotCu = lvLot == null ? default : Pagini(typeof(Lot), lvLot, prefetch: true);
        Console.WriteLine($"     MĂSURAT (D85-R2/{eticheta}): FacturaIntrare [{string.Join(";", lvFct == null ? [] : Coloane(lvFct))}] "
            + $"{fctCu.Randuri} rânduri — cu PreFetch: pagina 1 = {fctCu.Pagina1}, pagina 2 = {fctCu.Pagina2} comenzi; "
            + $"fără PreFetch: {fctFara.Pagina1} / {fctFara.Pagina2}"
            + (fctCu.Eroare != null ? $"; EROARE {fctCu.Eroare}" : "")
            + $" | Lot [{string.Join(";", lvLot == null ? [] : Coloane(lvLot))}] {lotCu.Randuri} rânduri — "
            + $"pagina 1 = {lotCu.Pagina1}, pagina 2 = {lotCu.Pagina2} comenzi" + (lotCu.Eroare != null ? $"; EROARE {lotCu.Eroare}" : ""));
        s.Check($"D85-R2 ({eticheta}) o pagină `Server` pe `FacturaIntrare_ListView` (coloanele vizibile din model, "
            + "PreFetchReferenceProperties ca în host) costă ≤ 3 comenzi SQL pe pagină, fără interogare per rând; "
            + "la fel pe `Lot_ListView` cu `Eticheta` vizibilă (Produs prefetch-uit, eticheta compusă fără SQL per rând)",
            lvFct != null && lvLot != null && fctCu.Eroare == null && lotCu.Eroare == null
            && fctCu.Randuri >= 2 && lotCu.Randuri >= 3
            && fctCu.Pagina1 is >= 1 and <= 3 && fctCu.Pagina2 <= 3
            && lotCu.Pagina1 is >= 1 and <= 3 && lotCu.Pagina2 <= 3);

        // ---- D85-R3: culegerea nested = Client, vede liniile nesalvate ----
        int vazuteClient = -1, vazuteServer = -1;
        string eroareR3 = null, eroareServerR3 = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            try {
                var doc = os.CreateObject<FacturaIntrare>();
                doc.Numar = Marcaj + "-DRY";
                doc.Data = new DateOnly(2026, 3, 9);
                doc.Predator = os.GetObjectByKey<Partener>(furnizorId);
                doc.Primitor = os.GetObjectByKey<Gestiune>(gestiuneId);
                for (var i = 0; i < 2; i++) {
                    var linie = os.CreateObject<FacturaIntrareDetaliu>();
                    linie.Cantitate = 1m;
                    doc.Detalii.Add(linie);
                }
                var membru = os.TypesInfo.FindTypeInfo(typeof(FacturaIntrare)).FindMember(nameof(Document.Detalii));
                var client = new PropertyCollectionSource(os, typeof(FacturaIntrare), doc, membru,
                    CollectionSourceDataAccessMode.Client, CollectionSourceMode.Normal);
                vazuteClient = client.List.Count;
                try {
                    var server = new PropertyCollectionSource(os, typeof(FacturaIntrare), doc, membru,
                        CollectionSourceDataAccessMode.Server, CollectionSourceMode.Normal);
                    vazuteServer = server.List.Count;
                }
                catch (Exception ex) {
                    eroareServerR3 = $"{ex.GetType().Name}: {ex.Message.Split('\n')[0]}";
                }
            }
            catch (Exception ex) {
                eroareR3 = $"{ex.GetType().Name}: {ex.Message}";
            }
            finally {
                os.Rollback();
            }
        }
        Console.WriteLine($"     MĂSURAT (D85-R3/{eticheta}): master cu 2 linii nesalvate — colecția Client vede "
            + $"{vazuteClient}, colecția Server vede {(eroareServerR3 ?? vazuteServer.ToString())}"
            + (eroareR3 != null ? $"; EROARE {eroareR3}" : ""));
        s.Check($"D85-R3 ({eticheta}) dovadă de cod pentru (c): o colecție nested Client vede liniile nesalvate ale "
            + "master-ului, o colecție Server pe același master nu le vede (interoghează baza) — în UI XAF Blazor "
            + "salvează antetul înaintea primei linii, dar liniile în lucru și grupare + adăugare rămân ale modului Client",
            eroareR3 == null && vazuteClient == 2 && vazuteServer != 2);

        using (var os = s.Provider.CreateObjectSpace())
            CurataD85(os);
        using (var os = s.Provider.CreateObjectSpace())
            s.Check($"D85 ({eticheta}) scena nu lasă urme",
                !os.GetObjectsQuery<Produs>().Any(p => p.Cod == Marcaj)
                && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj + "-"))
                && !os.GetObjectsQuery<Document>().Any(d => d.Numar.StartsWith(Marcaj + "-")));
    }
}
