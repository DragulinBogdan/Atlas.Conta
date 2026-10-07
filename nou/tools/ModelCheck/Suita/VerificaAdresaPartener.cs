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

// ============ Felia 15, pas 1: adresa structurată + județele — D15-V1 ============
// Ce probează: modelul (cele 8 câmpuri pe FRUNZA `Partener`, cu lungimile
// SAF-T ajunse până în coloana Postgres), nomenclatorul `Judet` seed-uit de
// NUCLEU pe ambele profiluri, conversiile pure din `JudeteRo` (indicativ auto /
// cod CNP / denumire → cod ISO) și cele două garduri noi din `GardianEditare`.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaD394Seed`.
static class VerificaAdresaPartener {
    public static void Ruleaza(Suita s, bool privat) {
        // ---------------- Modelul EF (D15-D1) ----------------
        using (var osModel = s.Provider.CreateObjectSpace()) {
            var model = ((EFCoreObjectSpace)osModel).DbContext.Model;
            var partener = model.FindEntityType(typeof(Partener));
            var repartitor = model.FindEntityType(typeof(Repartitor));
            // Câmp · lungimea SAF-T (`AddressStructure`); null = necaracter.
            (string Camp, int? Lungime)[] campuri = [
                (nameof(Partener.Strada), 70),
                (nameof(Partener.Numar), 18),
                (nameof(Partener.DetaliiAdresa), 70),
                (nameof(Partener.Localitate), 35),
                (nameof(Partener.CodPostal), 18),
                (nameof(Partener.JudetId), null),
                (nameof(Partener.DataSincronizareAnaf), null),
                (nameof(Partener.InactivFiscal), null),
            ];
            s.Check("D15-V1 adresa structurată = 8 câmpuri pe FRUNZA `Partener` (nu pe `Repartitor`: gestiunea și "
                + "angajatul n-au adresă fiscală), cu `MaxLength` = lungimile SAF-T (70/18/70/35/18)",
                partener != null && repartitor != null
                && campuri.All(c => partener.FindProperty(c.Camp) != null
                    && repartitor.FindProperty(c.Camp) == null
                    && partener.FindProperty(c.Camp).GetMaxLength() == c.Lungime));
            var judet = model.FindEntityType(typeof(Judet));
            s.Check("D15-V1 `Judet`: `Cod` UNIC, index nefiltrat; FK-ul `Partener.Judet` e Restrict (nomenclatorul "
                + "nu golește tăcut adresa)",
                judet != null
                && judet.GetIndexes().Any(i => i.IsUnique
                    && i.Properties.Select(x => x.Name).SequenceEqual([nameof(Judet.Cod)])
                    && i.GetFilter() == null)
                && partener.GetForeignKeys().Any(fk =>
                    fk.PrincipalEntityType.ClrType == typeof(Judet)
                    && fk.DeleteBehavior == DeleteBehavior.ClientNoAction));
        }

        // Lungimile ajunse în SCHEMĂ, nu doar în model (F14: `FieldSize` nu produce
        // lungime în EF — de-aia câmpurile poartă `[MaxLength]`). Citit din
        // `information_schema`, adică din baza pe care rulează suita.
        using (var osSql = s.Provider.CreateObjectSpace()) {
            var coloane = ((EFCoreObjectSpace)osSql).DbContext.Database
                .SqlQuery<string>($@"
                SELECT column_name || '=' || data_type || '(' || COALESCE(character_maximum_length::text, '?') || ')'
                FROM information_schema.columns
                WHERE table_name = 'Repartitori'
                  AND column_name IN ('Strada', 'Numar', 'DetaliiAdresa', 'Localitate', 'CodPostal')
                ORDER BY column_name")
                .ToList();
            Console.WriteLine($"     MĂSURAT (D15-V1/schemă): {string.Join(", ", coloane)}.");
            s.Check("D15-V1 lungimile SAF-T ajung în COLOANĂ: `character varying(n)` pe toate cele cinci câmpuri "
                + "de text ale adresei (probă pe `information_schema`, nu pe modelul EF)",
                coloane.OrderBy(c => c, StringComparer.Ordinal).SequenceEqual([
                    "CodPostal=character varying(18)", "DetaliiAdresa=character varying(70)",
                    "Localitate=character varying(35)", "Numar=character varying(18)",
                    "Strada=character varying(70)"]));
        }

        // ---------------- Lista și conversiile pure (D15-D1) ----------------
        s.Check("D15-V1 `JudeteRo`: 42 de subdiviziuni (41 de județe + București), coduri ISO/auto/CNP toate "
            + "distincte, iar `Cod` == „RO-” + `CodAuto` pe fiecare rând",
            JudeteRo.Toate.Count == ContaSeeder.JudeteAsteptate && JudeteRo.Toate.Count == 42
            && JudeteRo.Toate.Select(j => j.Cod).Distinct().Count() == 42
            && JudeteRo.Toate.Select(j => j.CodAuto).Distinct().Count() == 42
            && JudeteRo.Toate.Select(j => j.CodCnp).Distinct().Count() == 42
            && JudeteRo.Toate.All(j => j.Cod == "RO-" + j.CodAuto));
        s.Check("D15-V1 `DupaCodAuto` (sursa ANAF `*cod_JudetAuto`): „CJ” → RO-CJ, „ b ” → RO-B, indicativ "
            + "necunoscut/gol → null",
            JudeteRo.DupaCodAuto("CJ") == "RO-CJ" && JudeteRo.DupaCodAuto(" b ") == "RO-B"
            && JudeteRo.DupaCodAuto("XX") == null && JudeteRo.DupaCodAuto("") == null
            && JudeteRo.DupaCodAuto(null) == null);
        s.Check("D15-V1 `DupaCodCnp` (sursa 1C `CodJudet`): 40 → RO-B, 12 → RO-CJ, 8 → RO-BV, 15 → RO-DB, "
            + "29 → RO-PH, 51 → RO-CL, 52 → RO-GR; sectoarele Capitalei (41–46) → RO-B; 0/50/99 → null",
            JudeteRo.DupaCodCnp(40) == "RO-B" && JudeteRo.DupaCodCnp(12) == "RO-CJ"
            && JudeteRo.DupaCodCnp(8) == "RO-BV" && JudeteRo.DupaCodCnp(15) == "RO-DB"
            && JudeteRo.DupaCodCnp(29) == "RO-PH" && JudeteRo.DupaCodCnp(51) == "RO-CL"
            && JudeteRo.DupaCodCnp(52) == "RO-GR" && JudeteRo.DupaCodCnp(41) == "RO-B"
            && JudeteRo.DupaCodCnp(46) == "RO-B"
            && JudeteRo.DupaCodCnp(0) == null && JudeteRo.DupaCodCnp(50) == null && JudeteRo.DupaCodCnp(99) == null);
        s.Check("D15-V1 `DupaDenumire` normalizează (trim, majuscule, fără diacritice — inclusiv ş/ţ cu cedilă din "
            + "bazele legacy —, cratime și spații multiple pliate, prefixele MUNICIPIUL/JUDEȚUL tăiate): "
            + "„Municipiul Bucureşti”/„BUCURESTI” → RO-B, „Bistrita Nasaud”/„Bistrița-Năsăud” → RO-BN, "
            + "„Judetul  Caras - Severin” → RO-CS; ce nu e județ → null",
            JudeteRo.DupaDenumire("Municipiul Bucureşti") == "RO-B"
            && JudeteRo.DupaDenumire("BUCURESTI") == "RO-B"
            && JudeteRo.DupaDenumire("București") == "RO-B"
            && JudeteRo.DupaDenumire("Bistrita Nasaud") == "RO-BN"
            && JudeteRo.DupaDenumire("Bistrița-Năsăud") == "RO-BN"
            && JudeteRo.DupaDenumire("JUDEȚUL DÂMBOVIȚA") == "RO-DB"
            && JudeteRo.DupaDenumire("Judetul  Caras - Severin") == "RO-CS"
            && JudeteRo.DupaDenumire("Satu-Mare") == "RO-SM"
            && JudeteRo.DupaDenumire("Sector 3") == null && JudeteRo.DupaDenumire("   ") == null
            && JudeteRo.DupaDenumire(null) == null);

        // ---------------- Seed-ul de nucleu (D15-D1) ----------------
        using (var os = s.Provider.CreateObjectSpace()) {
            var judete = os.GetObjectsQuery<Judet>().ToList();
            s.Check($"D15-V1 ({(privat ? "privat" : "bugetar")}) nomenclatorul `Judet` e al NUCLEULUI — 42 de rânduri "
                + "identice cu `JudeteRo` pe ambele profiluri (împărțirea administrativă nu ține de planul de "
                + "conturi); RO-B și RO-CJ prezente cu toate cele trei coduri",
                judete.Count == 42
                && judete.Any(j => j.Cod == "RO-B" && j.Denumire == "București" && j.CodAuto == "B" && j.CodCnp == 40)
                && judete.Any(j => j.Cod == "RO-CJ" && j.Denumire == "Cluj" && j.CodAuto == "CJ" && j.CodCnp == 12)
                && judete.All(j => JudeteRo.Toate.Any(t => t.Cod == j.Cod && t.Denumire == j.Denumire
                    && t.CodAuto == j.CodAuto && t.CodCnp == j.CodCnp)));
        }

        // Seed-ul RESCRIE câmpurile ne-cheie (tiparul `RandD300`): nomenclatorul e
        // `[ForbidCRUD]` tocmai fiindcă seed-ul e singura lui cale de scriere, deci
        // trebuie să fie și singura lui autoritate. Proba trece prin funcția REALĂ.
        {
            string dupaStricare, dupaReseed;
            int codCnpDupaStricare, codCnpDupaReseed, judeteDupaReseed;
            Guid idInainte, idDupa;
            using (var osStrica = s.Provider.CreateObjectSpace()) {
                var cluj = osStrica.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
                idInainte = cluj.ID;
                cluj.Denumire = "Cluj GREȘIT";
                cluj.CodCnp = 999;
                osStrica.CommitChanges();
            }
            using (var osCitire = s.Provider.CreateObjectSpace()) {
                var cluj = osCitire.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
                dupaStricare = cluj.Denumire;
                codCnpDupaStricare = cluj.CodCnp;
            }
            using (var osSeed = s.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedJudete(osSeed);
                osSeed.CommitChanges();
            }
            using (var osCitire = s.Provider.CreateObjectSpace()) {
                var cluj = osCitire.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
                dupaReseed = cluj.Denumire;
                codCnpDupaReseed = cluj.CodCnp;
                idDupa = cluj.ID;
                judeteDupaReseed = osCitire.GetObjectsQuery<Judet>().Count();
            }
            Console.WriteLine($"     MĂSURAT (D15-V1/seed): RO-CJ „{dupaStricare}”/{codCnpDupaStricare} → "
                + $"„{dupaReseed}”/{codCnpDupaReseed} după re-seed; {judeteDupaReseed} județe; "
                + $"ID păstrat: {idDupa == idInainte}.");
            s.Check("D15-V1 `SeedJudete` e IDEMPOTENT și AUTORITAR: pe `Cod` existent rescrie Denumire/CodAuto/CodCnp "
                + "(o denumire stricată revine), fără să creeze rânduri noi și fără să schimbe ID-ul referit de "
                + "adresele existente",
                dupaStricare == "Cluj GREȘIT" && codCnpDupaStricare == 999
                && dupaReseed == "Cluj" && codCnpDupaReseed == 12
                && judeteDupaReseed == 42 && idDupa == idInainte);
        }

        // ---------------- Gardul „județul e al adreselor din România” (D15-D1) ----------------
        using (var osGard = s.Provider.CreateObjectSpace()) {
            var p = osGard.CreateObject<Partener>();
            p.Cod = "E2E-F15-G1";
            p.Denumire = "Probă județ";
            p.Judet = osGard.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
            string mesajRo = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajRo = e.Message; }
            p.Tara = "DE";
            string mesajDe = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajDe = e.Message; }
            p.Judet = null;
            string mesajDeFaraJudet = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajDeFaraJudet = e.Message; }
            Console.WriteLine($"     MĂSURAT (D15-V1/județ): RO + RO-CJ → „{mesajRo ?? "tace"}”; "
                + $"DE + RO-CJ → „{mesajDe ?? "<NU A ARUNCAT>"}”; DE fără județ → „{mesajDeFaraJudet ?? "tace"}”.");
            s.Check("D15-V1 `GardianEditare.VerificaPartener` refuză județul pe o adresă din afara României "
                + "(SAF-T validează `Region` contra listei ISO 3166-2:RO) și tace pe RO cu județ și pe DE fără "
                + "județ; gardul citește NAVIGAȚIA, nu FK-ul (pe un partener nou scalarul nu e încă fixat)",
                mesajRo == null && mesajDe != null && mesajDe.Contains("județ") && mesajDe.Contains("DE")
                && mesajDeFaraJudet == null);
            osGard.Rollback();
        }

        // ---------------- Timbrul server-owned (D15-D1, în siajul lui `Autogenerat`) ----------------
        {
            var timbru = new DateTime(2026, 8, 25, 10, 30, 0, DateTimeKind.Utc);
            // Partenerul-probă se naște pe ușa NON-SECURED (gardianul nu e abonat pe
            // OS-urile standalone ale suitei — cazul (5) din antetul gardianului),
            // exact ca la seed/import.
            Guid idProba;
            using (var osCreare = s.Provider.CreateObjectSpace()) {
                var p = osCreare.CreateObject<Partener>();
                p.Cod = "E2E-F15-ANAF";
                p.Denumire = "Probă timbru ANAF";
                osCreare.CommitChanges();
                idProba = p.ID;
            }
            // (1) Pe un partener NOU, timbrul pre-completat e refuzat: altfel un
            // partener născut prin OData și-ar fabrica proveniența.
            string mesajNou = null;
            using (var osNou = s.Provider.CreateObjectSpace()) {
                var p = osNou.CreateObject<Partener>();
                p.Cod = "E2E-F15-ANAF-NOU";
                p.Denumire = "Probă timbru pe partener nou";
                p.DataSincronizareAnaf = timbru;
                try { GardianEditare.Verifica(osNou); } catch (OperareException e) { mesajNou = e.Message; }
                osNou.Rollback();
            }
            // (1b) `InactivFiscal` e din aceeași familie (review F2): pre-completat
            // pe un partener nou = refuz.
            string mesajInactivNou = null;
            using (var osNou = s.Provider.CreateObjectSpace()) {
                var p = osNou.CreateObject<Partener>();
                p.Cod = "E2E-F15-ANAF-INACTIV";
                p.Denumire = "Probă inactiv pe partener nou";
                p.InactivFiscal = true;
                try { GardianEditare.Verifica(osNou); } catch (OperareException e) { mesajInactivNou = e.Message; }
                osNou.Rollback();
            }
            // (2) Pe ușa SECURED adresa se culege liber, timbrul NU, inactivul NU.
            string mesajAdresa = null, mesajTimbru = null, mesajInactiv = null;
            using (var osSecured = s.Provider.CreateObjectSpace()) {
                var p = osSecured.GetObjectByKey<Partener>(idProba);
                p.Localitate = "Cluj-Napoca";
                p.Strada = "Avram Iancu";
                try { GardianEditare.Verifica(osSecured); } catch (OperareException e) { mesajAdresa = e.Message; }
                p.DataSincronizareAnaf = timbru;
                try { GardianEditare.Verifica(osSecured); } catch (OperareException e) { mesajTimbru = e.Message; }
                osSecured.Rollback();
            }
            using (var osSecured = s.Provider.CreateObjectSpace()) {
                var p = osSecured.GetObjectByKey<Partener>(idProba);
                p.InactivFiscal = !p.InactivFiscal;
                try { GardianEditare.Verifica(osSecured); } catch (OperareException e) { mesajInactiv = e.Message; }
                osSecured.Rollback();
            }
            // (3) Pe ușa NON-SECURED (fără gardian) timbrul se scrie și se persistă —
            // acolo va rula serviciul de sincronizare (58c).
            using (var osNonSecured = s.Provider.CreateObjectSpace()) {
                var p = osNonSecured.GetObjectByKey<Partener>(idProba);
                p.DataSincronizareAnaf = timbru;
                p.InactivFiscal = true;
                osNonSecured.CommitChanges();
            }
            DateTime? citit;
            bool inactivCitit;
            using (var osCitire = s.Provider.CreateObjectSpace()) {
                var p = osCitire.GetObjectByKey<Partener>(idProba);
                citit = p.DataSincronizareAnaf;
                inactivCitit = p.InactivFiscal;
            }
            using (var osCuratenie = s.Provider.CreateObjectSpace()) {
                new Purja(osCuratenie)
                    .Adauga(osCuratenie.GetObjectsQuery<Partener>().Where(x => x.Cod.StartsWith("E2E-F15-")))
                    .Executa();
            }
            bool ramas;
            using (var osVerif = s.Provider.CreateObjectSpace())
                ramas = osVerif.GetObjectsQuery<Partener>().Any(x => x.Cod.StartsWith("E2E-F15-"));
            Console.WriteLine($"     MĂSURAT (D15-V1/timbru): pe partener nou → „{mesajNou ?? "<NU A ARUNCAT>"}”; "
                + $"adresă pe secured → „{mesajAdresa ?? "tace"}”; timbru pe secured → "
                + $"„{mesajTimbru ?? "<NU A ARUNCAT>"}”; pe non-secured citit înapoi: {citit:O} / "
                + $"inactiv {inactivCitit}; probe rămase: {ramas}.");
            s.Check("D15-V1 `DataSincronizareAnaf` e SERVER-OWNED: refuzat pe ușa secured atât pre-completat pe un "
                + "partener nou, cât și schimbat pe unul existent; adresa culeasă alături trece; pe ușa non-secured "
                + "se scrie și se persistă (împreună cu `InactivFiscal`)",
                mesajNou != null && mesajAdresa == null
                && mesajTimbru != null && mesajTimbru.Contains("ANAF")
                && citit == timbru && inactivCitit && !ramas);
            Console.WriteLine($"     MĂSURAT (D15-V1/inactiv): pe partener nou → „{mesajInactivNou ?? "<NU A ARUNCAT>"}”; "
                + $"schimbat pe secured → „{mesajInactiv ?? "<NU A ARUNCAT>"}”.");
            s.Check("D15-V1 (F2) `InactivFiscal` e SERVER-OWNED ca timbrul: refuzat pe ușa secured pre-completat pe "
                + "un partener nou și schimbat pe unul existent; pe ușa non-secured se scrie (proba de mai sus)",
                mesajInactivNou != null && mesajInactivNou.Contains("inactiv")
                && mesajInactiv != null && mesajInactiv.Contains("inactiv") && inactivCitit);
        }
    }
}
