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

// ============ Felia 16, pas 1: modelul SAF-T — D16-V1 ============
// Ce probează: `Societate` (unicitate, lungimile de adresă identice cu ale
// partenerului, județul doar pe RO, baza contabilă din lista ANAF, seed-ul care
// NU rescrie), nomenclatorul `UnitateMasura` (2.163 de coduri UN/ECE, seed
// autoritar), puntea pură `UnitatiMasuraRo.Rezolva`, formatul lui `Produs.CodNc`
// și `Cont.RolTert` seed-uit DOAR la privat.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaAdresaPartener`.
// Partea de `RolTert` e singura care se ramifică pe profil — și tocmai de aceea
// rulează pe amândouă: „bugetarul n-are niciun rol" e o afirmație care trebuie
// probată, nu presupusă.
static class VerificaSaftModel {
    public static void Ruleaza(Suita suita, bool privat) {
        // ---------------- Modelul: adresa societății == adresa partenerului ----------------
        using (var osModel = suita.Provider.CreateObjectSpace()) {
            var model = ((EFCoreObjectSpace)osModel).DbContext.Model;
            var societate = model.FindEntityType(typeof(Societate));
            var partener = model.FindEntityType(typeof(Partener));
            // Faptul de FOND (D16-D1): amândouă ies prin același `AddressStructure`,
            // deci amândouă trebuie să taie la aceleași lungimi. Comparația e cu
            // `AdresaSaft.Lungimi` (sursa unică, reflecție pe `Partener`), nu cu o
            // listă de numere scrisă aici — altfel proba ar fi fost a treia listă.
            var campuriText = AdresaSaft.CampuriAdresa
                .Where(c => c != nameof(Partener.JudetId)).ToList();
            var potrivite = campuriText.All(c =>
                societate.FindProperty(c) != null
                && societate.FindProperty(c).GetMaxLength() == AdresaSaft.Lungimi[c]
                && partener.FindProperty(c).GetMaxLength() == AdresaSaft.Lungimi[c]);
            Console.WriteLine("     MĂSURAT (D16-V1/lungimi): "
                + string.Join(", ", campuriText.Select(c =>
                    $"{c} soc={societate.FindProperty(c)?.GetMaxLength()?.ToString() ?? "-"}"
                    + $"/part={partener.FindProperty(c)?.GetMaxLength()?.ToString() ?? "-"}"
                    + $"/AdresaSaft={AdresaSaft.Lungimi[c]}"))
                + $"; JudetId pe Societate: {societate.FindProperty(nameof(Societate.JudetId)) != null}.");
            suita.Check("D16-V1 `Societate` poartă ACELEAȘI 6 câmpuri de adresă ca `Partener`, cu `MaxLength` "
                + "identice cu `AdresaSaft.Lungimi` (o singură sursă a lungimilor SAF-T, prin reflecție — "
                + "nu două liste care pot deriva)",
                potrivite && societate.FindProperty(nameof(Societate.JudetId)) != null
                && AdresaSaft.CampuriAdresa.Count == 6);
            var unitate = model.FindEntityType(typeof(UnitateMasura));
            suita.Check("D16-V1 `UnitateMasura`: `Cod` UNIC, index nefiltrat; FK-urile noi "
                + "(`Produs.UnitateMasura`, `Societate.Judet`, `Societate.ContBancar`) sunt Restrict — "
                + "un nomenclator șters nu golește tăcut un câmp care ajunge în fișier",
                unitate != null
                && unitate.GetIndexes().Any(i => i.IsUnique
                    && i.Properties.Select(x => x.Name).SequenceEqual([nameof(UnitateMasura.Cod)])
                    && i.GetFilter() == null)
                && model.FindEntityType(typeof(Produs)).GetForeignKeys().Any(fk =>
                    fk.PrincipalEntityType.ClrType == typeof(UnitateMasura)
                    && fk.DeleteBehavior == DeleteBehavior.ClientNoAction)
                && societate.GetForeignKeys().Count(fk => fk.DeleteBehavior == DeleteBehavior.ClientNoAction) == 2);
        }

        // ---------------- Nomenclatorul de unități de măsură (D16-D2) ----------------
        using (var os = suita.Provider.CreateObjectSpace()) {
            var numar = os.GetObjectsQuery<UnitateMasura>().Count();
            var dupaCod = os.GetObjectsQuery<UnitateMasura>()
                .Where(u => u.Cod == "H87" || u.Cod == "KGM" || u.Cod == "LTR" || u.Cod == "MTR"
                    || u.Cod == "MTQ" || u.Cod == "MTK")
                .ToDictionary(u => u.Cod, u => u.Denumire, StringComparer.Ordinal);
            Console.WriteLine($"     MĂSURAT (D16-V1/UM): {numar} unități seed-uite "
                + $"(lista în cod: {UnitatiMasuraUnEce.Toate.Count}); "
                + string.Join(", ", dupaCod.OrderBy(x => x.Key, StringComparer.Ordinal)
                    .Select(x => $"{x.Key}=„{x.Value}”")) + ".");
            suita.Check($"D16-V1 ({(privat ? "privat" : "bugetar")}) nomenclatorul `UnitateMasura` e al NUCLEULUI — "
                + $"{ContaSeeder.UnitatiMasuraAsteptate} de coduri UN/ECE pe AMBELE profiluri (unitățile nu țin "
                + "de planul de conturi), iar codurile uzuale sunt cele din nomenclatorul ANAF",
                numar == ContaSeeder.UnitatiMasuraAsteptate
                && UnitatiMasuraUnEce.Toate.Count == ContaSeeder.UnitatiMasuraAsteptate
                && UnitatiMasuraUnEce.Toate.Select(u => u.Cod).Distinct().Count() == numar
                && dupaCod.Count == 6
                && dupaCod["H87"] == "bucată" && dupaCod["KGM"] == "kilogram"
                && dupaCod["LTR"] == "litru" && dupaCod["MTR"] == "metru"
                && dupaCod["MTQ"] == "metru cub" && dupaCod["MTK"] == "metru pătrat");
        }

        // Seed-ul RESCRIE câmpurile ne-cheie, ca `SeedJudete` — nomenclatorul e
        // `[ForbidCRUD]`, deci seed-ul e singura lui autoritate. Proba trece prin
        // funcția REALĂ.
        {
            string dupaStricare, dupaReseed;
            int numarDupaReseed;
            Guid idInainte, idDupa;
            using (var osStrica = suita.Provider.CreateObjectSpace()) {
                var kg = osStrica.FirstOrDefault<UnitateMasura>(u => u.Cod == "KGM");
                idInainte = kg.ID;
                kg.Denumire = "kilogram GREȘIT";
                osStrica.CommitChanges();
            }
            using (var osCitire = suita.Provider.CreateObjectSpace())
                dupaStricare = osCitire.FirstOrDefault<UnitateMasura>(u => u.Cod == "KGM").Denumire;
            using (var osSeed = suita.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedUnitatiMasura(osSeed);
                osSeed.CommitChanges();
            }
            using (var osCitire = suita.Provider.CreateObjectSpace()) {
                var kg = osCitire.FirstOrDefault<UnitateMasura>(u => u.Cod == "KGM");
                dupaReseed = kg.Denumire;
                idDupa = kg.ID;
                numarDupaReseed = osCitire.GetObjectsQuery<UnitateMasura>().Count();
            }
            Console.WriteLine($"     MĂSURAT (D16-V1/seed UM): KGM „{dupaStricare}” → „{dupaReseed}” după "
                + $"re-seed; {numarDupaReseed} unități; ID păstrat: {idDupa == idInainte}.");
            suita.Check("D16-V1 `SeedUnitatiMasura` e IDEMPOTENT și AUTORITAR: pe `Cod` existent rescrie `Denumire` "
                + "(o traducere stricată revine), fără rânduri noi și fără să schimbe ID-ul referit de produse",
                dupaStricare == "kilogram GREȘIT" && dupaReseed == "kilogram"
                && numarDupaReseed == ContaSeeder.UnitatiMasuraAsteptate && idDupa == idInainte);
        }

        // ---------------- Puntea pură UM liberă → cod UN/ECE (D16-D2) ----------------
        suita.Check("D16-V1 `UnitatiMasuraRo.Rezolva` normalizează grafia românească (trim, majuscule, diacritice, "
            + "punct final): „Bucată”/„BUC.”/„buc” → H87, „kg” → KGM, „mp” → MTK, „ora” → HUR, „to” → TNE",
            UnitatiMasuraRo.Rezolva("Bucată") == "H87" && UnitatiMasuraRo.Rezolva("BUC.") == "H87"
            && UnitatiMasuraRo.Rezolva(" buc ") == "H87" && UnitatiMasuraRo.Rezolva("kg") == "KGM"
            && UnitatiMasuraRo.Rezolva("mp") == "MTK" && UnitatiMasuraRo.Rezolva("m2") == "MTK"
            && UnitatiMasuraRo.Rezolva("ora") == "HUR" && UnitatiMasuraRo.Rezolva("to") == "TNE"
            && UnitatiMasuraRo.Rezolva("litri") == "LTR" && UnitatiMasuraRo.Rezolva("pereche") == "PR");
        // „xyz” NU e valoarea de probă potrivită pentru „nerezolvabil”, deși pare:
        // `XYZ` E cod UN/ECE („Ambalaj compozit, recipient din sticlă în ambalaj
        // solid din plastic", rec21). Constatare a suitei la prima rulare — de-aia
        // proba de nerezolvabil folosește un token căutat ÎMPOTRIVA listei.
        suita.Check("D16-V1 `Rezolva` acceptă și codul UN/ECE ca atare, indiferent de caz („kgm” → KGM, „h87” → H87, "
            + "„xyz” → XYZ, ambalaj compozit), și întoarce `null` pe ce nu poate rezolva fără să ghicească — "
            + "un produs nerezolvat e avertisment în fișier, nu o valoare inventată",
            UnitatiMasuraRo.Rezolva("kgm") == "KGM" && UnitatiMasuraRo.Rezolva("h87") == "H87"
            && UnitatiMasuraRo.Rezolva("XSX") == "XSX" && UnitatiMasuraRo.Rezolva("xyz") == "XYZ"
            && UnitatiMasuraRo.Rezolva("bidon") == null && UnitatiMasuraRo.Rezolva("navete") == null
            && UnitatiMasuraRo.Rezolva("") == null
            && UnitatiMasuraRo.Rezolva("   ") == null && UnitatiMasuraRo.Rezolva(null) == null);
        // Coliziunea MĂSURATĂ peste cele 2.163 de coduri: „mc" e și cod UN/ECE
        // (microgram), și abrevierea românească pentru metru cub. Grafia câștigă —
        // altfel fiecare produs vândut la metru cub ar fi ieșit în fișier ca
        // microgram: sintactic valid, deci trecut de validator, și fals.
        suita.Check("D16-V1 coliziunea „mc” (cod UN/ECE = microgram vs. grafia RO = metru cub) se rezolvă în "
            + "favoarea GRAFIEI: „mc”/„m3” → MTQ; „set”, cealaltă coliziune, cade la fel pe SET pe ambele căi",
            UnitatiMasuraRo.Rezolva("mc") == "MTQ" && UnitatiMasuraRo.Rezolva("m3") == "MTQ"
            && UnitatiMasuraRo.Rezolva("MC") == "MTQ"
            && UnitatiMasuraRo.Rezolva("set") == "SET" && UnitatiMasuraRo.Rezolva("SET") == "SET");

        // ---------------- Societatea: rândul unic (D16-D1) ----------------
        using (var os = suita.Provider.CreateObjectSpace()) {
            var toate = os.GetObjectsQuery<Societate>().ToList();
            Console.WriteLine($"     MĂSURAT (D16-V1/societate): {toate.Count} rând(uri); "
                + $"Tara=„{toate.FirstOrDefault()?.Tara}”, BazaContabila=„{toate.FirstOrDefault()?.BazaContabila}”, "
                + $"RaporteazaCnp={toate.FirstOrDefault()?.RaporteazaCnp}.");
            suita.Check($"D16-V1 ({(privat ? "privat" : "bugetar")}) seed-ul creează UN rând `Societate`, gol, cu "
                + "default-urile de pe tip (Tara RO, BazaContabila A, RaporteazaCnp fals) — antetul există pe "
                + "orice bază, chiar dacă nimeni nu l-a completat încă",
                toate.Count == 1 && toate[0].Tara == "RO"
                && toate[0].BazaContabila == Societate.BazaContabilaImplicita
                && !toate[0].RaporteazaCnp);
        }
        using (var osAlDoilea = suita.Provider.CreateObjectSpace()) {
            osAlDoilea.CreateObject<Societate>();
            string mesaj = null;
            try { GardianEditare.Verifica(osAlDoilea); } catch (OperareException e) { mesaj = e.Message; }
            Console.WriteLine($"     MĂSURAT (D16-V1/unicitate): al doilea rând → „{mesaj ?? "<NU A ARUNCAT>"}”.");
            suita.Check("D16-V1 `GardianEditare.VerificaSocietate` refuză AL DOILEA rând de societate (unicitatea nu "
                + "se poate exprima ca index — n-ai pe ce coloană; gardianul e singura ei formă, și e pe ușa "
                + "comună, deci și POST-ul prin OData cade acolo)",
                mesaj != null && mesaj.Contains("societate"));
            osAlDoilea.Rollback();
        }

        // ---------------- Societatea: județul e al României, baza contabilă e a ANAF ----------------
        using (var osGard = suita.Provider.CreateObjectSpace()) {
            var s = osGard.GetObjectsQuery<Societate>().First();
            s.Judet = osGard.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
            string mesajRo = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajRo = e.Message; }
            s.Tara = "DE";
            string mesajDe = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajDe = e.Message; }
            s.Tara = "RO";
            s.BazaContabila = "XYZ";
            string mesajBaza = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajBaza = e.Message; }
            s.BazaContabila = "ONG";
            string mesajOng = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajOng = e.Message; }
            Console.WriteLine($"     MĂSURAT (D16-V1/garduri): RO + RO-CJ → „{mesajRo ?? "tace"}”; "
                + $"DE + RO-CJ → „{mesajDe ?? "<NU A ARUNCAT>"}”; baza „XYZ” → "
                + $"„{mesajBaza ?? "<NU A ARUNCAT>"}”; baza „ONG” → „{mesajOng ?? "tace"}”.");
            suita.Check("D16-V1 gardurile societății: județ pe țară ≠ RO ⇒ refuz (SAF-T validează `Region` contra "
                + "listei ISO 3166-2:RO, iar valoarea ajunge și în `AuditFileRegion`); `BazaContabila` în afara "
                + "celor 12 valori `TaxAccountingBasis` ⇒ refuz, una din listă („ONG”) ⇒ tace",
                mesajRo == null && mesajDe != null && mesajDe.Contains("județ")
                && mesajBaza != null && mesajBaza.Contains("XYZ") && mesajOng == null);
            osGard.Rollback();
        }

        // ---------------- Societatea: seed-ul NU rescrie (riscul 12 al contractului) ----------------
        {
            string denumireInainte, dupaSeed;
            int numarDupaSeed;
            using (var osCitire = suita.Provider.CreateObjectSpace())
                denumireInainte = osCitire.GetObjectsQuery<Societate>().First().Denumire;
            using (var osCompleteaza = suita.Provider.CreateObjectSpace()) {
                osCompleteaza.GetObjectsQuery<Societate>().First().Denumire = "PROBĂ D16 SRL";
                osCompleteaza.CommitChanges();
            }
            using (var osSeed = suita.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedSocietate(osSeed);
                osSeed.CommitChanges();
            }
            using (var osCitire = suita.Provider.CreateObjectSpace()) {
                dupaSeed = osCitire.GetObjectsQuery<Societate>().First().Denumire;
                numarDupaSeed = osCitire.GetObjectsQuery<Societate>().Count();
            }
            // Curățenie: rândul NU se purjează (e singletonul bazei, creat de seed) —
            // se pune la loc valoarea de dinaintea probei.
            using (var osRestaurare = suita.Provider.CreateObjectSpace()) {
                osRestaurare.GetObjectsQuery<Societate>().First().Denumire = denumireInainte;
                osRestaurare.CommitChanges();
            }
            Console.WriteLine($"     MĂSURAT (D16-V1/seed societate): „{denumireInainte ?? "<gol>"}” → "
                + $"„PROBĂ D16 SRL” → după re-seed „{dupaSeed}”; {numarDupaSeed} rând(uri).");
            suita.Check("D16-V1 `SeedSocietate` CREEAZĂ dar nu RESCRIE: pe o bază cu societatea completată, "
                + "`--forceUpdate` n-o golește și nu adaugă un al doilea rând (spre deosebire de seed-urile de "
                + "nomenclator, aici conținutul e al clientului, nu al legii)",
                dupaSeed == "PROBĂ D16 SRL" && numarDupaSeed == 1);
        }

        // ---------------- `Produs.CodNc`: exact 8 cifre, sau gol (D16-D2) ----------------
        using (var osProdus = suita.Provider.CreateObjectSpace()) {
            var p = osProdus.CreateObject<Produs>();
            p.Cod = "E2E-F16-NC";
            p.Denumire = "Probă cod NC";
            p.CodNc = "1234567";
            string mesajScurt = null;
            try { GardianEditare.Verifica(osProdus); } catch (OperareException e) { mesajScurt = e.Message; }
            p.CodNc = "1234567A";
            string mesajLitera = null;
            try { GardianEditare.Verifica(osProdus); } catch (OperareException e) { mesajLitera = e.Message; }
            p.CodNc = "12345678";
            string mesajBun = null;
            try { GardianEditare.Verifica(osProdus); } catch (OperareException e) { mesajBun = e.Message; }
            p.CodNc = null;
            string mesajGol = null;
            try { GardianEditare.Verifica(osProdus); } catch (OperareException e) { mesajGol = e.Message; }
            Console.WriteLine($"     MĂSURAT (D16-V1/CodNc): „1234567” → „{mesajScurt ?? "<NU A ARUNCAT>"}”; "
                + $"„1234567A” → „{mesajLitera ?? "<NU A ARUNCAT>"}”; „12345678” → „{mesajBun ?? "tace"}”; "
                + $"gol → „{mesajGol ?? "tace"}”.");
            suita.Check("D16-V1 `Produs.CodNc` are exact 8 cifre sau e gol — pe ușa GARDIANULUI, nu doar prin regula "
                + "XAF: `Produs` e CRUD pe OData, iar validarea XAF nu rulează pe API (55b), deci un PUT ar fi "
                + "putut scrie „ABC” în `ProductCommodityCode`",
                mesajScurt != null && mesajScurt.Contains("NC")
                && mesajLitera != null && mesajBun == null && mesajGol == null);
            osProdus.Rollback();
        }

        // ---------------- `Cont.RolTert`: politică per PROFIL (D16-D3) ----------------
        using (var osRol = suita.Provider.CreateObjectSpace()) {
            var conturi = osRol.GetObjectsQuery<Cont>()
                .Where(c => c.RolTert != RolTertCont.Niciunul)
                .Select(c => new { c.Simbol, c.RolTert })
                .ToList();
            if (!privat) {
                Console.WriteLine($"     MĂSURAT (D16-V1/RolTert bugetar): {conturi.Count} conturi cu rol de terț.");
                suita.Check("D16-V1 (bugetar) NICIUN cont n-are rol de terț: SAF-T (D406) nu are bază contabilă pentru "
                    + "planul instituțiilor publice, deci `Customers`/`Suppliers` n-au ce alimenta — absența e "
                    + "PROBATĂ, nu presupusă",
                    conturi.Count == 0);
            }
            else {
                var dupaSimbol = conturi.ToDictionary(c => c.Simbol, c => c.RolTert, StringComparer.Ordinal);
                RolTertCont Rol(string simbol) =>
                    dupaSimbol.TryGetValue(simbol, out var r) ? r : RolTertCont.Niciunul;
                Console.WriteLine($"     MĂSURAT (D16-V1/RolTert privat): {conturi.Count} conturi cu rol "
                    + $"({conturi.Count(c => c.RolTert == RolTertCont.Client)} client / "
                    + $"{conturi.Count(c => c.RolTert == RolTertCont.Furnizor)} furnizor); "
                    + $"411={Rol("411")}, 4111={Rol("4111")}, 401={Rol("401")}, 4091={Rol("4091")}, "
                    + $"419={Rol("419")}, 4426={Rol("4426")}, 461={Rol("461")}, 462={Rol("462")}, "
                    + $"41={Rol("41")}, 40={Rol("40")}.");
                suita.Check("D16-V1 (privat) `Cont.RolTert` se seed-uiește pe PREFIX, deci prinde și analiticele "
                    + "planului: 411/4111/4118/413/418/419 ⇒ Client, 401/403/404/405/408/409/4091 ⇒ Furnizor",
                    Rol("411") == RolTertCont.Client && Rol("4111") == RolTertCont.Client
                    && Rol("4118") == RolTertCont.Client && Rol("413") == RolTertCont.Client
                    && Rol("418") == RolTertCont.Client && Rol("419") == RolTertCont.Client
                    && Rol("401") == RolTertCont.Furnizor && Rol("403") == RolTertCont.Furnizor
                    && Rol("404") == RolTertCont.Furnizor && Rol("405") == RolTertCont.Furnizor
                    && Rol("408") == RolTertCont.Furnizor && Rol("409") == RolTertCont.Furnizor
                    && Rol("4091") == RolTertCont.Furnizor && Rol("4094") == RolTertCont.Furnizor);
                suita.Check("D16-V1 (privat) ce NU e terț comercial rămâne `Niciunul`: TVA-ul (4426/4427), debitorii "
                    + "și creditorii diverși (461/462 — angajatul de pe decont trebuie să iasă în `Neincluse` cu "
                    + "cauză, nu în `Customers`) și GRUPELE sumatoare 40/41 (soldurile se citesc pe conturile "
                    + "care poartă partenerul)",
                    Rol("4426") == RolTertCont.Niciunul && Rol("4427") == RolTertCont.Niciunul
                    && Rol("461") == RolTertCont.Niciunul && Rol("462") == RolTertCont.Niciunul
                    && Rol("463") == RolTertCont.Niciunul && Rol("467") == RolTertCont.Niciunul
                    && Rol("40") == RolTertCont.Niciunul && Rol("41") == RolTertCont.Niciunul
                    && Rol("4") == RolTertCont.Niciunul);
            }
        }

        // ---------------- `Cont.Functie`: funcția contabilă e a LEGII (pasul 3) ----------------
        // `AccountType` (MF.GLA.7) are trei valori admise, iar ele se citesc din
        // `Functie`. La bugetar coloana venea din CPLAN; la privat planul OMFP n-o
        // purta deloc, deci TOT planul ieșea „Bifunctional" — gaură de DATE, închisă
        // cu coloana `Functie` (D/C/B) în resursa de seed, din anexa OMFP 1802.
        using (var osFunctie = suita.Provider.CreateObjectSpace()) {
            var conturi = osFunctie.GetObjectsQuery<Cont>()
                .Select(c => new { c.Simbol, c.Functie })
                .ToList();
            var necunoscute = conturi.Where(c => !SaftReguli.FunctieCunoscuta(c.Functie)).ToList();
            string F(string simbol) =>
                conturi.FirstOrDefault(c => c.Simbol == simbol)?.Functie ?? "(lipsă)";
            Console.WriteLine($"     MĂSURAT (D16-V1/Functie {(privat ? "privat" : "bugetar")}): {conturi.Count} conturi "
                + $"({conturi.Count(c => c.Functie == "D")} D / {conturi.Count(c => c.Functie == "C")} C / "
                + $"{conturi.Count(c => c.Functie == "B")} B / {necunoscute.Count} necunoscute"
                + $"{(necunoscute.Count > 0 ? ": " + string.Join(", ", necunoscute.Take(10).Select(c => c.Simbol)) : "")}); "
                + $"411={F("411")}, 401={F("401")}, 4426={F("4426")}, 4427={F("4427")}, 4428={F("4428")}, 121={F("121")}.");
            suita.Check("D16-V1 `Cont.Functie` e completă pe TOT planul, într-o singură convenție (D/C/B — cea a resursei "
                + "bugetare, adoptată și de cea privată): niciun cont cu funcție necunoscută, deci `AccountType` "
                + "(MF.GLA.7) nu mai iese „Bifunctional” din lipsă de date",
                necunoscute.Count == 0 && conturi.Count > 0);
            if (privat) {
                suita.Check("D16-V1 (privat) funcția vine din anexa OMFP 1802, pe fiecare cont: clienții `D` (activ), "
                    + "furnizorii `C` (pasiv), TVA-ul deductibil `D` și cel colectat `C` — iar `SaftReguli.TipCont` "
                    + "le traduce în Activ/Pasiv, valorile pe care schema le acceptă",
                    F("411") == "D" && F("4111") == "D" && F("401") == "C" && F("409") == "D"
                    && F("4426") == "D" && F("4427") == "C" && F("607") == "D" && F("707") == "C"
                    && SaftReguli.TipCont(F("411")) == "Activ" && SaftReguli.TipCont(F("401")) == "Pasiv");
                suita.Check("D16-V1 (privat) conturile pe care anexa OMFP le dă „A/P” rămân bifuncționale — 121 (profit sau "
                    + "pierdere), 117 (rezultatul reportat), 4428 (TVA neexigibilă), 5121 (contul curent) și "
                    + "diferențele de preț: `B` e o valoare a legii, nu o necunoscută",
                    F("121") == "B" && F("117") == "B" && F("4428") == "B" && F("5121") == "B"
                    && F("378") == "B" && F("308") == "B");
            }
        }
    }
}
