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
// F28 — TPH pe Document/DocumentDetaliu/Repartitor, discriminatorul `ClrType` (89)
// ---------------------------------------------------------------------------
static class VerificaF28 {
    public static void Ruleaza(Suita s, bool privat) {
        var eticheta = privat ? "privat" : "bugetar";
        Type[] ierarhii = [typeof(Document), typeof(DocumentDetaliu), typeof(Repartitor)];
        using var os = s.Provider.CreateObjectSpace();
        var ctxF28 = ((EFCoreObjectSpace)os).DbContext;
        var model = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions
            .GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>(ctxF28).Model;
        var radacini = ierarhii.Select(t => model.FindEntityType(t)).ToList();
        var tipuri = radacini.ToDictionary(r => r.ClrType, r => r.GetDerivedTypesInclusive().ToList());
        var concrete = tipuri.ToDictionary(p => p.Key, p => p.Value.Where(t => !t.ClrType.IsAbstract).ToList());
        List<string> Valori(FormattableString sql) => ctxF28.Database.SqlQuery<string>(sql).ToList();

        // ---- F28-A: 1:1 între clasele concrete ale ierarhiei `Document` și seed-ul `TipDocument` ----
        var seed = os.GetObjectsQuery<TipDocument>().Select(t => new { t.Cod, t.ClrType }).ToList();
        var documenteConcrete = concrete[typeof(Document)];
        var faraClasa = seed.Where(s => !documenteConcrete.Any(t => t.ClrType.Name == s.ClrType
            && (string)t.GetDiscriminatorValue() == s.ClrType)).Select(s => $"{s.Cod}={s.ClrType}").ToList();
        var faraAncora = documenteConcrete.Where(t => !seed.Any(s => s.ClrType == (string)t.GetDiscriminatorValue()))
            .Select(t => $"{t.ClrType.Name}={t.GetDiscriminatorValue()}").ToList();
        var discriminatorAltNume = tipuri.Values.SelectMany(v => v).Where(t => !t.ClrType.IsAbstract)
            .Where(t => (string)t.GetDiscriminatorValue() != t.ClrType.Name).Select(t => t.ClrType.Name).ToList();
        Console.WriteLine($"     MĂSURAT (F28-A/{eticheta}): {seed.Count} rânduri TipDocument, {documenteConcrete.Count} clase "
            + $"concrete de document; fără clasă: [{string.Join(", ", faraClasa)}]; fără ancoră: "
            + $"[{string.Join(", ", faraAncora)}]; discriminator ≠ numele clasei: [{string.Join(", ", discriminatorAltNume)}].");
        s.Check($"F28-A ({eticheta}) fiecare `TipDocument` din seed are clasa concretă mapată în ierarhia `Document` cu "
            + "discriminatorul == `ClrType`, fiecare clasă concretă are ancoră în seed, iar pe toate cele trei ierarhii "
            + "discriminatorul e numele scurt al clasei (tipurile abstracte se sar: n-au rânduri)",
            seed.Count > 0 && seed.Count == documenteConcrete.Count
            && faraClasa.Count == 0 && faraAncora.Count == 0 && discriminatorAltNume.Count == 0);

        // ---- F28-B: fără prefix de tip — coloana = numele proprietății, o singură tabelă per ierarhie ----
        var abateri = new List<string>();
        foreach (var r in radacini) {
            var tabela = r.GetTableName();
            var lista = tipuri[r.ClrType];
            foreach (var et in lista) {
                if (et.GetTableName() != tabela)
                    abateri.Add($"{et.ClrType.Name} → tabela {et.GetTableName()}");
                foreach (var p in et.GetDeclaredProperties())
                    if (p.GetColumnName() != p.Name)
                        abateri.Add($"{et.ClrType.Name}.{p.Name} → {p.GetColumnName()}");
            }
            var coloaneModel = lista.SelectMany(et => et.GetDeclaredProperties()).Select(p => p.GetColumnName())
                .ToHashSet(StringComparer.Ordinal);
            var coloaneBaza = Valori($"SELECT column_name::text AS \"Value\" FROM information_schema.columns WHERE table_name = {tabela}")
                .ToHashSet(StringComparer.Ordinal);
            if (!coloaneBaza.SetEquals(coloaneModel))
                abateri.Add($"{tabela}: bază ≠ model (doar în bază: {string.Join(",", coloaneBaza.Except(coloaneModel))}; "
                    + $"doar în model: {string.Join(",", coloaneModel.Except(coloaneBaza))})");
            Console.WriteLine($"     MĂSURAT (F28-B/{eticheta}): {tabela} — {lista.Count} tipuri, {coloaneBaza.Count} coloane în bază.");
        }
        s.Check($"F28-B ({eticheta}) pe `Documente`, `DocumentDetalii` și `Repartitori` fiecare proprietate a ierarhiei "
            + "are coloana numită ca ea (niciun prefix de tip), toate tipurile stau pe tabela rădăcinii, iar coloanele "
            + "bazei (`information_schema`) sunt exact cele ale modelului"
            + (abateri.Count > 0 ? $" — abateri: {string.Join("; ", abateri)}" : ""),
            abateri.Count == 0);

        // ---- F28-C: indexul pe discriminator, în model și în bază ----
        var faraIndex = new List<string>();
        foreach (var r in radacini) {
            var tabela = r.GetTableName();
            if (!r.GetIndexes().Any(i => i.Properties.Count == 1 && i.Properties[0].Name == nameof(Document.ClrType)))
                faraIndex.Add($"{tabela} (model)");
            if (!Valori($"SELECT indexdef::text AS \"Value\" FROM pg_indexes WHERE tablename = {tabela}")
                    .Any(d => d.EndsWith("(\"ClrType\")", StringComparison.Ordinal)))
                faraIndex.Add($"{tabela} (pg_indexes)");
        }
        s.Check($"F28-C ({eticheta}) indexul pe `ClrType` există pe cele trei tabele, în modelul EF și în `pg_indexes`"
            + (faraIndex.Count > 0 ? $" — lipsă: {string.Join(", ", faraIndex)}" : ""),
            faraIndex.Count == 0);

        // ---- F28-F: `ClrType` e al EF — setter ne-public, completat la `Add`, fără gol în bază ----
        var setterePublice = tipuri.Values.SelectMany(v => v)
            .Where(et => et.ClrType.GetProperty(nameof(Document.ClrType))?.GetSetMethod(false) != null)
            .Select(et => et.ClrType.Name).ToList();
        var laAdd = new List<string>();
        using (var osNou = s.Provider.CreateObjectSpace())
            foreach (var et in concrete.Values.SelectMany(v => v)) {
                var obiect = osNou.CreateObject(et.ClrType);
                var valoare = (string)et.ClrType.GetProperty(nameof(Document.ClrType)).GetValue(obiect);
                if (valoare != et.ClrType.Name)
                    laAdd.Add($"{et.ClrType.Name}={valoare ?? "null"}");
            }
        var straine = new List<string>();
        foreach (var r in radacini) {
            var tabela = r.GetTableName();
            var numeConcrete = concrete[r.ClrType].Select(t => t.ClrType.Name).ToHashSet(StringComparer.Ordinal);
            var sql = "SELECT DISTINCT COALESCE(\"ClrType\", '<null>')::text AS \"Value\" FROM \"" + tabela + "\"";
            straine.AddRange(ctxF28.Database.SqlQueryRaw<string>(sql).ToList().Where(v => !numeConcrete.Contains(v)).Select(v => $"{tabela}: „{v}”"));
        }
        Console.WriteLine($"     MĂSURAT (F28-F/{eticheta}): {concrete.Values.Sum(v => v.Count)} tipuri concrete create prin "
            + $"ObjectSpace; setter public: [{string.Join(", ", setterePublice)}]; greșit la Add: [{string.Join(", ", laAdd)}]; "
            + $"valori în bază fără clasă concretă: [{string.Join(", ", straine)}].");
        s.Check($"F28-F ({eticheta}) `ClrType` nu se poate scrie din cod (niciun setter public pe nicio clasă a celor trei "
            + "ierarhii), EF îl completează la `CreateObject` cu numele clasei pe fiecare tip concret, iar în bază nu există "
            + "valoare NULL, goală sau fără clasă concretă. Fără FK spre `TipDocument.ClrType` (fallback-ul din F28-D1), "
            + "ancora în seed o ține doar F28-A",
            setterePublice.Count == 0 && laAdd.Count == 0 && straine.Count == 0);

        // ---- F28-G: `ClrType` în modelul XAF — persistent, read-only, cu caption „Tip” ----
        IModelApplication modelXaf = null;
        try {
            modelXaf = ModelAplicatie.Incarca(s.ConnectionString, s.Profil);
        }
        catch (Exception ex) {
            Console.WriteLine($"     F28-G: modelul aplicației nu s-a construit — {ex.GetType().Name}: {ex.Message}");
        }
        var probleme = new List<string>();
        foreach (var baza in ierarhii) {
            var clasa = modelXaf?.BOModel.GetClass(baza);
            var membru = clasa?.FindMember(nameof(Document.ClrType));
            var info = clasa?.TypeInfo.FindMember(nameof(Document.ClrType));
            if (membru == null || info == null) {
                probleme.Add($"{baza.Name}: lipsește din model");
                continue;
            }
            if (!info.IsPersistent)
                probleme.Add($"{baza.Name}: nepersistent");
            if (membru.AllowEdit)
                probleme.Add($"{baza.Name}: editabil");
            if (membru.Caption != "Tip")
                probleme.Add($"{baza.Name}: caption „{membru.Caption}”");
        }
        static bool InLayout(IModelNode nod) {
            for (var i = 0; i < nod.NodeCount; i++) {
                var copil = nod.GetNode(i);
                if (copil is IModelLayoutViewItem li && li.ViewItem?.Id == nameof(Document.ClrType) || InLayout(copil))
                    return true;
            }
            return false;
        }
        static bool ColoanaTip(IModelListView lv)
            => lv?.Columns[nameof(Document.ClrType)] is { } c && (!c.Index.HasValue || c.Index > -1);
        bool DinIerarhii(IModelObjectView v) => v.ModelClass?.TypeInfo?.Type is { } t && ierarhii.Any(b => b.IsAssignableFrom(t));
        var detailViews = (modelXaf?.Views.OfType<IModelDetailView>() ?? []).Where(DinIerarhii).ToList();
        var dvCuTip = detailViews.Where(v => v.Layout != null && InLayout(v.Layout)).Select(v => v.Id).ToList();
        var listeIerarhii = (modelXaf?.Views.OfType<IModelListView>() ?? []).Where(DinIerarhii).ToList();
        var listeCuTip = listeIerarhii.Where(ColoanaTip).Select(v => v.Id).OrderBy(id => id, StringComparer.Ordinal).ToList();
        string[] cuTipCerut = [nameof(Document) + "_ListView", nameof(DocumentTrezorerie) + "_ListView", nameof(Repartitor) + "_ListView", nameof(Repartitor) + "_LookupListView",
            nameof(DocumentDetaliu) + "_LookupListView"];
        Console.WriteLine($"     MĂSURAT (F28-G/{eticheta}): {detailViews.Count} DetailView-uri și {listeIerarhii.Count} ListView-uri "
            + $"pe cele trei ierarhii; „Tip” în layout: [{string.Join(", ", dvCuTip)}]; coloana „Tip” vizibilă: "
            + $"[{string.Join(", ", listeCuTip)}]");
        if (detailViews.Count == 0 || listeIerarhii.Count <= cuTipCerut.Length)
            probleme.Add("prea puține view-uri pe ierarhii");
        if (dvCuTip.Count > 0)
            probleme.Add($"„Tip” în layout-ul: {string.Join(", ", dvCuTip)}");
        if (!listeCuTip.SequenceEqual(cuTipCerut.OrderBy(id => id, StringComparer.Ordinal)))
            probleme.Add($"coloana „Tip” vizibilă pe [{string.Join(", ", listeCuTip)}]");
        s.Check($"F28-G ({eticheta}) `ClrType` e membru PERSISTENT în modelul aplicației pe `Document`, `DocumentDetaliu` și "
            + "`Repartitor` (deci trece precondițiile D85-M2 pe ServerView), read-only (`AllowEdit` = false) și cu caption „Tip”; "
            + "lipsește din layout-ul oricărui DetailView al celor trei ierarhii și e coloană vizibilă EXACT pe listele care "
            + "amestecă tipuri: `Document_ListView`, `DocumentTrezorerie_ListView`, `Repartitor_ListView`, `Repartitor_LookupListView` și "
            + "`DocumentDetaliu_LookupListView` (fără DefaultProperty, XAF îi generează coloanele scurte)"
            + (probleme.Count > 0 ? $" — {string.Join("; ", probleme)}" : ""),
            modelXaf != null && probleme.Count == 0);

        // ---- Scena F28-D…K: repartitori, documente cu câte o linie de tipul declarat, FK-uri spre frunze, pe ușa de sistem ----
        const string MarcajF28 = "F28-PROBA";
        void CurataF28(IObjectSpace osC) =>
            new Purja(osC)
                .Adauga(osC.GetObjectsQuery<DviFactura>().Where(x => x.Dvi.Numar.StartsWith(MarcajF28)))
                .Adauga(osC.GetObjectsQuery<Document>().Where(d => d.Numar != null && d.Numar.StartsWith(MarcajF28)))
                .Adauga(osC.GetObjectsQuery<Imobilizare>().Where(i => i.NumarInventar.StartsWith(MarcajF28)))
                .Adauga(osC.GetObjectsQuery<ApplicationUserLoginInfo>().Where(l => l.User.UserName.StartsWith(MarcajF28)))
                .Adauga(osC.GetObjectsQuery<ApplicationUser>().Where(u => u.UserName.StartsWith(MarcajF28)))
                .Adauga(osC.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajF28)))
                .Executa();
        Guid idPartener, idGestiune, idContPropriu, idNir, idDvi, idFcl, idIncasare;
        var liniiScena = new Dictionary<string, string>(StringComparer.Ordinal);
        using (var osS = s.Provider.CreateObjectSpace()) {
            CurataF28(osS);
            var partener = osS.CreateObject<Partener>();
            partener.Cod = MarcajF28 + "-P"; partener.Denumire = "Partener probă F28";
            var gestiune = osS.CreateObject<Gestiune>();
            gestiune.Cod = MarcajF28 + "-G"; gestiune.Denumire = "Gestiune probă F28";
            var contPropriu = osS.CreateObject<ContPropriu>();
            contPropriu.Cod = MarcajF28 + "-CP"; contPropriu.Denumire = "Cont propriu probă F28";
            var angajat = osS.CreateObject<Angajat>();
            angajat.Cod = MarcajF28 + "-A"; angajat.Denumire = "Angajat probă F28";
            var tipMaterialId = osS.GetObjectsQuery<TipMaterial>().OrderBy(t => t.Cod).Select(t => t.ID).First();
            var fisa = osS.CreateObject<Imobilizare>();
            fisa.NumarInventar = MarcajF28 + "-FISA"; fisa.Denumire = "Fișă probă F28";
            fisa.TipMaterialId = tipMaterialId; fisa.Loc = gestiune; fisa.Responsabil = angajat;
            var documente = new Dictionary<Type, Document>();
            foreach (var et in concrete[typeof(Document)]) {
                var d = (Document)osS.CreateObject(et.ClrType);
                d.Numar = $"{MarcajF28}-{et.ClrType.Name}";
                d.Data = d.DataInregistrare = new DateOnly(2031, 1, 15);
                d.Predator = gestiune;
                d.Primitor = gestiune;
                var tipLinie = (Attribute.GetCustomAttribute(et.ClrType, typeof(Atlas.Conta.BackOffice.Module.UI.TipDetaliuAttribute), false)
                    as Atlas.Conta.BackOffice.Module.UI.TipDetaliuAttribute)?.TipDetaliu ?? typeof(DocumentDetaliu);
                var linie = (DocumentDetaliu)osS.CreateObject(tipLinie);
                linie.Document = d;
                linie.TipMaterialId = tipMaterialId;
                linie.Cantitate = 1m;
                linie.Valoare = 1m;
                if (tipLinie.GetProperty(nameof(PunereInFunctiuneDetaliu.ImobilizareId)) is { } imobilizare
                        && imobilizare.PropertyType == typeof(Guid))
                    imobilizare.SetValue(linie, fisa.ID);
                documente[et.ClrType] = d;
                liniiScena[et.ClrType.Name] = tipLinie.Name;
            }
            ((FacturaIesire)documente[typeof(FacturaIesire)]).GestiuneDescarcare = gestiune;
            ((FacturaIntrare)documente[typeof(FacturaIntrare)]).PlataContPropriu = contPropriu;
            ((Plata)documente[typeof(Plata)]).LaturaPereche = (Incasare)documente[typeof(Incasare)];
            var legatura = osS.CreateObject<DviFactura>();
            legatura.Dvi = (Dvi)documente[typeof(Dvi)];
            legatura.Factura = (FacturaIntrare)documente[typeof(FacturaIntrare)];
            osS.CommitChanges();
            (idPartener, idGestiune, idContPropriu) = (partener.ID, gestiune.ID, contPropriu.ID);
            (idNir, idDvi, idFcl, idIncasare) = (documente[typeof(NIR)].ID, documente[typeof(Dvi)].ID,
                documente[typeof(FacturaIesire)].ID, documente[typeof(Incasare)].ID);
        }

        // ---- F28-K: ierarhia utilizatorilor XAF (TPH) — utilizator nou + login în același commit, pe calea gardianului ----
        string refuzUtilizator;
        using (var osU = s.Provider.CreateObjectSpace()) {
            var utilizator = osU.CreateObject<ApplicationUser>();
            utilizator.UserName = MarcajF28 + "-U";
            var login = osU.CreateObject<ApplicationUserLoginInfo>();
            login.LoginProviderName = DevExpress.ExpressApp.Security.SecurityDefaults.PasswordAuthentication;
            login.ProviderUserKey = utilizator.ID.ToString();
            login.User = utilizator;
            refuzUtilizator = s.Refuz(() => GardianEditare.Verifica(osU));
            if (refuzUtilizator == null)
                osU.CommitChanges();
        }
        using (var osU = s.Provider.CreateObjectSpace()) {
            var loginuri = osU.GetObjectsQuery<ApplicationUserLoginInfo>()
                .Count(l => l.User.UserName == MarcajF28 + "-U");
            Console.WriteLine($"     MĂSURAT (F28-K/{eticheta}): gardian „{refuzUtilizator ?? "acceptat"}”; login-uri comise pe "
                + $"utilizatorul de probă: {loginuri}.");
            s.Check($"F28-K ({eticheta}) ierarhia utilizatorilor XAF e tot TPH: un `ApplicationUser` nou și "
                + "`ApplicationUserLoginInfo` spre el, în același commit, trec regula (o) a gardianului (ținta nouă găsită în "
                + "tracker, de tipul cerut) și se comit",
                refuzUtilizator == null && loginuri == 1);
        }

        // ---- F28-H/I/J: integritatea tipului în bază, pe ușa de sistem (aceleași interogări ca `--dump-integritate-tph`) ----
        var probeTph = IntegritateTph.Probe(ctxF28);
        var rezultateTph = IntegritateTph.Ruleaza(ctxF28, probeTph);
        List<RezultatTph> FamilieTph(string familie) => rezultateTph.Where(r => r.Familie == familie).ToList();
        string Incalcari(List<RezultatTph> lista) =>
            string.Join("; ", lista.Where(r => r.Incalcari != 0).Select(r => $"{r.Eticheta}: {r.Incalcari}"));
        var linii = FamilieTph(IntegritateTph.Linii);
        var fkuri = FamilieTph(IntegritateTph.Fk);
        var coloane = FamilieTph(IntegritateTph.Coloane);
        Console.WriteLine($"     MĂSURAT (F28-H/{eticheta}): {linii.Count} tipuri concrete de document, {linii.Sum(r => r.Verificate)} linii "
            + $"verificate (scena: {string.Join(", ", liniiScena.OrderBy(p => p.Key).Select(p => $"{p.Key}→{p.Value}"))}); "
            + $"încălcări [{Incalcari(linii)}].");
        s.Check($"F28-H ({eticheta}) nicio linie nu are `ClrType` în afara tipului de detaliu declarat de documentul ei "
            + "(`[TipDetaliu]`, cu subtipurile) sau a lui `DocumentDetaliu` — deci `as` pe frunza documentului citește doar "
            + "linii ale frunzei sau ale bazei; fiecare tip concret are cel puțin o linie verificată"
            + (Incalcari(linii) is { Length: > 0 } h ? $" — {h}" : ""),
            linii.Count == concrete[typeof(Document)].Count && linii.All(r => r.Verificate > 0 && r.Incalcari == 0));
        Console.WriteLine($"     MĂSURAT (F28-I/{eticheta}): {fkuri.Count} FK-uri spre frunze, {fkuri.Count(r => r.Verificate > 0)} cu "
            + $"rânduri, {fkuri.Sum(r => r.Verificate)} rânduri verificate "
            + $"[{string.Join(", ", fkuri.Select(r => $"{r.Eticheta}: {r.Verificate}"))}]; încălcări [{Incalcari(fkuri)}].");
        s.Check($"F28-I ({eticheta}) în bază, ținta fiecărui FK spre frunză (descoperit prin `GardianEditare.FkSpreFrunze`) are "
            + "discriminatorul frunzei sau al unui subtip, și pe rândurile scrise pe ușa de sistem, care nu trec prin regula (o); "
            + "cele 9 FK-uri acoperite, cel puțin 7 cu rânduri din scenă"
            + (Incalcari(fkuri) is { Length: > 0 } i ? $" — {i}" : ""),
            fkuri.Count >= 9 && fkuri.Count(r => r.Verificate > 0) >= 7 && fkuri.All(r => r.Incalcari == 0));
        var coloaneIerarhii = radacini.ToDictionary(r => r.GetTableName(),
            r => coloane.Count(c => c.Eticheta.StartsWith(r.GetTableName() + ".", StringComparison.Ordinal)));
        var directieAsm = Valori($"SELECT count(*)::text AS \"Value\" FROM \"DocumentDetalii\" WHERE \"ClrType\" = 'AsamblareDetaliu' AND \"Directie\" IS NOT NULL").Single();
        Console.WriteLine($"     MĂSURAT (F28-J/{eticheta}): {coloane.Count} coloane de tip derivat "
            + $"[{string.Join(", ", coloaneIerarhii.Select(p => $"{p.Key} {p.Value}"))}], {coloane.Sum(r => r.Verificate)} rânduri "
            + $"de alt tip verificate; `Directie` (enum ne-nullable în CLR) scrisă pe liniile ASM: {directieAsm}; "
            + $"încălcări [{Incalcari(coloane)}].");
        s.Check($"F28-J ({eticheta}) o coloană declarată pe tipuri derivate e NULL pe rândurile oricărui alt tip al ierarhiei "
            + "(EF nu scrie coloana fratelui, nici pe tipurile valoare) — pe toate ierarhiile TPH ale modelului"
            + (Incalcari(coloane) is { Length: > 0 } j ? $" — {j}" : ""),
            coloaneIerarhii.Values.All(n => n > 0) && coloane.Sum(r => r.Verificate) > 0 && directieAsm != "0"
            && coloane.All(r => r.Incalcari == 0));

        // ---- F28-D: FK spre frunză — tipul țintei îl ține gardianul, generic prin metadata EF ----
        string RefuzF28(Action<IObjectSpace> pregateste) {
            using var osG = s.Provider.CreateObjectSpace();
            try {
                pregateste(osG);
                GardianEditare.Verifica(osG);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
            finally {
                osG.Rollback();
            }
        }
        var acoperite = ctxF28.Model.GetEntityTypes()
            .SelectMany(et => GardianEditare.FkSpreFrunze(et).Where(fk => fk.DeclaringEntityType == et)
                .Select(fk => $"{et.ClrType.Name}.{fk.Properties[0].Name}→{fk.PrincipalEntityType.ClrType.Name}"))
            .ToHashSet(StringComparer.Ordinal);
        string[] asteptate = [
            "DviFactura.FacturaId→FacturaIntrare", "DviFactura.DviId→Dvi",
            "DocumentTrezorerie.LaturaPerecheId→DocumentTrezorerie", "Lot.GestiuneId→Gestiune",
            "Imobilizare.ResponsabilId→Angajat", "Societate.ContBancarId→ContPropriu",
            "FacturaIesire.GestiuneDescarcareId→Gestiune", "FacturaIntrare.PlataContPropriuId→ContPropriu",
        ];
        var neacoperite = asteptate.Where(a => !acoperite.Contains(a)).ToList();
        var lotPePartener = RefuzF28(o => o.CreateObject<Lot>().GestiuneId = idPartener);
        var lotPeGestiune = RefuzF28(o => o.CreateObject<Lot>().GestiuneId = idGestiune);
        var lotPeInexistent = RefuzF28(o => o.CreateObject<Lot>().GestiuneId = Guid.NewGuid());
        var lotPePartenerNou = RefuzF28(o => {
            var p = o.CreateObject<Partener>();
            p.Cod = MarcajF28 + "-PN"; p.Denumire = "Partener nou F28";
            o.CreateObject<Lot>().GestiuneId = p.ID;
        });
        var lotPeGestiuneNoua = RefuzF28(o => {
            var g = o.CreateObject<Gestiune>();
            g.Cod = MarcajF28 + "-GN"; g.Denumire = "Gestiune nouă F28";
            o.CreateObject<Lot>().GestiuneId = g.ID;
        });
        Societate SocietateF28(IObjectSpace o) =>
            o.GetObjectsQuery<Societate>().ToList().FirstOrDefault() ?? o.CreateObject<Societate>();
        var societatePePartener = RefuzF28(o => SocietateF28(o).ContBancarId = idPartener);
        var societatePeContPropriu = RefuzF28(o => SocietateF28(o).ContBancarId = idContPropriu);
        var fclPePartener = RefuzF28(o => o.GetObjectsQuery<FacturaIesire>().ToList()
            .First(d => d.Numar == MarcajF28 + "-" + nameof(FacturaIesire)).GestiuneDescarcareId = idPartener);
        bool RefuzTip(string mesaj, Guid id) => mesaj != null && mesaj.Contains($"({id}) e ") && mesaj.Contains(", nu ");
        bool FaraRefuzTip(string mesaj) => mesaj == null || !mesaj.Contains("rândul ales");
        Console.WriteLine($"     MĂSURAT (F28-D/{eticheta}): {acoperite.Count} FK-uri spre frunze descoperite "
            + $"[{string.Join(", ", acoperite.OrderBy(a => a))}]; Lot→Partener „{lotPePartener ?? "<acceptat>"}”; "
            + $"Lot→Gestiune „{lotPeGestiune ?? "acceptat"}”; Lot→inexistent „{lotPeInexistent ?? "<acceptat>"}”; "
            + $"Lot→Partener nou „{lotPePartenerNou ?? "<acceptat>"}”; Lot→Gestiune nouă „{lotPeGestiuneNoua ?? "acceptat"}”; "
            + $"Societate.ContBancar→Partener „{societatePePartener ?? "<acceptat>"}”; Societate.ContBancar→ContPropriu "
            + $"„{societatePeContPropriu ?? "acceptat"}”; FacturaIesire.GestiuneDescarcare→Partener „{fclPePartener ?? "<acceptat>"}”.");
        s.Check($"F28-D ({eticheta}) gardianul refuză cu mesaj de domeniu un FK spre frunză îndreptat spre un rând de ALT tip "
            + "(Lot→Partener din bază și nou în același commit, Societate.ContBancar→Partener, FacturaIesire.GestiuneDescarcare→"
            + "Partener) sau spre un id inexistent, lasă să treacă ținta de tipul cerut (Lot→Gestiune din bază și nouă, "
            + "Societate.ContBancar→ContPropriu), iar regula, descoperită din metadata EF, acoperă cele opt FK-uri de azi"
            + (neacoperite.Count > 0 ? $" — neacoperite: {string.Join(", ", neacoperite)}" : ""),
            neacoperite.Count == 0
            && RefuzTip(lotPePartener, idPartener) && lotPeGestiune == null
            && lotPeInexistent != null && lotPeInexistent.Contains("nu există")
            && lotPePartenerNou != null && lotPePartenerNou.Contains(", nu ") && FaraRefuzTip(lotPeGestiuneNoua)
            && RefuzTip(societatePePartener, idPartener) && FaraRefuzTip(societatePeContPropriu)
            && RefuzTip(fclPePartener, idPartener));

        var tipSters = RefuzF28(o => o.Delete(o.GetObjectsQuery<TipDocument>().ToList().First(t => t.Cod == "FCT")));
        var tipClrSchimbat = RefuzF28(o =>
            o.GetObjectsQuery<TipDocument>().ToList().First(t => t.Cod == "FCT").ClrType = nameof(FacturaIesire));
        Console.WriteLine($"     MĂSURAT (F28-D/{eticheta}/TipDocument): ștergere → „{tipSters ?? "<acceptat>"}”; "
            + $"`ClrType` schimbat → „{tipClrSchimbat ?? "<acceptat>"}”.");
        s.Check($"F28-D ({eticheta}) pe ușa comună `TipDocument` nu se șterge și nu-și schimbă `ClrType` — fără FK, ancora "
            + "documentelor e codul (decizia 20), iar crearea unui rând nou rămâne refuzată integral (F23-V4)",
            tipSters != null && tipSters.Contains("nu se șterge")
            && tipClrSchimbat != null && tipClrSchimbat.Contains("o scrie release-ul"));

        // ---- F28-L/M: rândul după cheie pe un ObjectSpace CU prefetch (ca pe hosturi), cu ținta urmărită sau nu ----
        EFCoreObjectSpace OsCuPrefetch() {
            var o = (EFCoreObjectSpace)s.Provider.CreateObjectSpace();
            o.PreFetchReferenceProperties = true;
            return o;
        }
        string Rezultat(Action actiune) {
            try { actiune(); return null; }
            catch (OperareException e) { return e.Message; }
            catch (Exception e) { return $"EXCEPȚIE {e.GetType().Name}: {e.Message.Split('\n')[0]}"; }
        }
        string CereF28<T>(Guid id, bool urmarit) where T : class {
            using var o = OsCuPrefetch();
            if (urmarit)
                o.GetObjectByKey(ctxF28.Model.FindEntityType(typeof(T)).GetRootType().ClrType, id);
            return Rezultat(() => Rezolva.Cere<T>(o, id, "Referința F28"));
        }
        var gestiunePePartener = new[] { true, false }.Select(u => CereF28<Gestiune>(idPartener, u)).ToList();
        var facturaPeNir = new[] { true, false }.Select(u => CereF28<FacturaIntrare>(idNir, u)).ToList();
        var gestiunePeGestiune = new[] { true, false }.Select(u => CereF28<Gestiune>(idGestiune, u)).ToList();
        var gestiuneInexistenta = CereF28<Gestiune>(Guid.NewGuid(), false);
        var tipMaterialF28 = CereF28<TipMaterial>(ctxF28.Set<TipMaterial>().Select(t => t.ID).First(), false);
        Console.WriteLine($"     MĂSURAT (F28-L/{eticheta}): Cere<Gestiune>(Partener) urmărit „{gestiunePePartener[0] ?? "<acceptat>"}”, "
            + $"neurmărit „{gestiunePePartener[1] ?? "<acceptat>"}”; Cere<FacturaIntrare>(NIR) urmărit „{facturaPeNir[0] ?? "<acceptat>"}”, "
            + $"neurmărit „{facturaPeNir[1] ?? "<acceptat>"}”; Cere<Gestiune>(Gestiune) „{gestiunePeGestiune[0] ?? "acceptat"}” / "
            + $"„{gestiunePeGestiune[1] ?? "acceptat"}”; inexistent „{gestiuneInexistenta ?? "<acceptat>"}”; "
            + $"Cere<TipMaterial> „{tipMaterialF28 ?? "acceptat"}”.");
        s.Check($"F28-L ({eticheta}) `Rezolva.Cere<T>` pe un ObjectSpace cu `PreFetchReferenceProperties` refuză de DOMENIU, cu fraza "
            + "regulii (o), un id de alt tip al aceleiași ierarhii (Gestiune←Partener, FacturaIntrare←NIR), identic cu ținta "
            + "urmărită sau nu (fără `InvalidCastException`); ținta de tipul cerut și tipul fără ierarhie trec, inexistentul "
            + "rămâne pe fraza unică",
            gestiunePePartener.All(m => RefuzTip(m, idPartener)) && gestiunePePartener.Distinct().Count() == 1
            && facturaPeNir.All(m => RefuzTip(m, idNir)) && facturaPeNir.Distinct().Count() == 1
            && gestiunePeGestiune.All(m => m == null) && tipMaterialF28 == null
            && gestiuneInexistenta != null && gestiuneInexistenta.Contains("nu există"));

        string dviPeSine;
        using (var o = OsCuPrefetch()) {
            var legatura = o.CreateObject<DviFactura>();
            legatura.DviId = idDvi;
            legatura.FacturaId = idDvi;
            dviPeSine = Rezultat(() => GardianEditare.Verifica(o));
            o.Rollback();
        }
        string laturaPeFcl;
        using (var o = OsCuPrefetch()) {
            o.GetObjectByKey(typeof(Document), idFcl);
            var incasare = (Incasare)o.GetObjectByKey(typeof(Document), idIncasare);
            incasare.LaturaPerecheId = idFcl;
            var erori = new List<string>();
            laturaPeFcl = Rezultat(() => incasare.ValideazaOperare(o, erori)) ?? string.Join("\n", erori);
            o.Rollback();
        }
        int Aparitii(string mesaj, string text) => mesaj == null ? 0 : mesaj.Split('\n').Count(l => l.Contains(text));
        Console.WriteLine($"     MĂSURAT (F28-M/{eticheta}): DviFactura{{Dvi = Factura = DVI}} „{dviPeSine ?? "<acceptat>"}”; "
            + $"Incasare.LaturaPereche = FCL urmărită „{laturaPeFcl.Replace('\n', '|')}”.");
        s.Check($"F28-M ({eticheta}) regulile entității pe cheie (`DviFactura.Verifica`, `DocumentTrezorerie.ValideazaOperare`) "
            + "refuză de domeniu o țintă de alt tip deja urmărită pe un ObjectSpace cu prefetch — o singură dată fraza „rândul "
            + "ales”, fără `InvalidCastException`",
            RefuzTip(dviPeSine, idDvi) && !dviPeSine.StartsWith("EXCEPȚIE") && Aparitii(dviPeSine, "rândul ales") == 1
            && RefuzTip(laturaPeFcl, idFcl) && !laturaPeFcl.StartsWith("EXCEPȚIE") && Aparitii(laturaPeFcl, "rândul ales") == 1);

        // ---- F28-N: plasa pe sursă — niciun `GetObjectByKey` pe o frunză sau pe un parametru generic în codul produsului ----
        var scanare = ScanareGetObjectByKey.Scaneaza(ctxF28.Model,
            Path.GetFullPath(Path.Combine(MetadataDump.DirectorProiect(), "..", "..", "Atlas.Conta.BackOffice")));
        Console.WriteLine($"     MĂSURAT (F28-N/{eticheta}): {scanare.Fisiere} fișiere, {scanare.Apeluri} apeluri `GetObjectByKey` cu tip "
            + $"explicit; încălcări {scanare.Incalcari.Count}: [{string.Join("; ", scanare.Incalcari)}].");
        s.Check($"F28-N ({eticheta}) în `nou/Atlas.Conta.BackOffice` niciun `GetObjectByKey<T>`/`GetObjectByKey(typeof(T), …)` n-are T "
            + "frunză a unei ierarhii EF sau parametru generic — rândul după cheie trece prin `RandDupaCheie` (rădăcina + `is`)",
            scanare.Fisiere > 100 && scanare.Apeluri > 50 && scanare.Incalcari.Count == 0);

        // ---- F28-E: cititorul tipului dă aceleași coduri ca `ClasaReala` + ancora, pe toate tipurile concrete ----
        using (var osE = s.Provider.CreateObjectSpace()) {
            var documente = osE.GetObjectsQuery<Document>()
                .Where(d => d.Numar != null && d.Numar.StartsWith(MarcajF28)).ToList();
            var codPeClrType = osE.GetObjectsQuery<TipDocument>().Select(t => new { t.ClrType, t.Cod }).ToList()
                .Where(t => t.ClrType != null).GroupBy(t => t.ClrType).ToDictionary(g => g.Key, g => g.First().Cod);
            var inexistent = Guid.NewGuid();
            var ids = documente.Select(d => d.ID).Append(inexistent).ToList();
            var coduri = CititorTipDocument.Coduri(osE, ids);
            var clase = CititorTipDocument.Clase(osE, ids);
            var diferente = documente
                .Where(d => {
                    var reala = MotorOperare.ClasaReala(d);
                    return coduri.GetValueOrDefault(d.ID) != codPeClrType.GetValueOrDefault(reala.Name)
                        || clase.GetValueOrDefault(d.ID) != reala.Name
                        || CititorTipDocument.Clasa(clase.GetValueOrDefault(d.ID)) != reala;
                })
                .Select(d => $"{MotorOperare.ClasaReala(d).Name}: cod „{coduri.GetValueOrDefault(d.ID)}”, "
                    + $"clasă „{clase.GetValueOrDefault(d.ID)}”")
                .ToList();
            var tipuriAcoperite = documente.Select(d => MotorOperare.ClasaReala(d)).Distinct().Count();
            Console.WriteLine($"     MĂSURAT (F28-E/{eticheta}): {documente.Count} documente pe {tipuriAcoperite} tipuri concrete; "
                + $"diferențe [{string.Join("; ", diferente)}]; id inexistent → cod "
                + $"„{(coduri.TryGetValue(inexistent, out var codLipsa) ? codLipsa ?? "null" : "<lipsă>")}”.");
            s.Check($"F28-E ({eticheta}) `CititorTipDocument` (o proiecție pe discriminator + ancora `TipDocument`) dă, pe câte un "
                + "document din FIECARE tip concret, exact codul obținut prin materializare (`ClasaReala` + ancora), aceeași "
                + "clasă (și `Clasa` o rezolvă înapoi la tipul CLR), iar un id "
                + "inexistent rămâne în contract cu `null`",
                documente.Count == concrete[typeof(Document)].Count && tipuriAcoperite == documente.Count
                && diferente.Count == 0
                && coduri.ContainsKey(inexistent) && coduri[inexistent] == null && !clase.ContainsKey(inexistent));
            CurataF28(osE);
        }
        using (var osF = s.Provider.CreateObjectSpace())
            s.Check($"F28 — curățenie finală ({eticheta}): niciun document și niciun repartitor de probă rămas",
                !osF.GetObjectsQuery<Document>().Any(d => d.Numar != null && d.Numar.StartsWith(MarcajF28))
                && !osF.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajF28)));
    }
}
