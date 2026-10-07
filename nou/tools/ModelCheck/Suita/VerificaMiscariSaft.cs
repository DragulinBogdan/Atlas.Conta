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

// ============ Felia 17, pas 1: PoliticaMiscareSaft — D17-V1 ============
// Ce probează: (a) nomenclatorul de mișcări (`SaftReguli.CoduriMiscare`, D17-D2)
// — pur, deci identic pe ambele profiluri; (b) modelul EF al politicii (D17-D1),
// cu AMBELE indexuri de unicitate, fiindcă `Semn` e nullabil și un singur index
// n-ar fi acoperit dublura „orice semn"; (c) seed-ul privat, pe FUNCȚIA REALĂ
// (idempotent la re-rulare); (d) gardianul, pe cele trei reguli ale lui; (e)
// refuzul unicității pe CALEA REALĂ — commit contra bazei, nu doar adnotarea din
// model: un index parțial cu filtru scris greșit trece de orice probă de model și
// pică abia în producție.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaSaftModel`:
// „bugetarul n-are nicio politică de mișcare" e o afirmație care se probează.
static class VerificaMiscariSaft {
    public static void Ruleaza(Suita s, bool privat) {
        // ---------------- Nomenclatorul legii (D17-D2) ----------------
        // Cele 19 coduri sunt sursa UNICĂ a două lucruri: descrierea din
        // `MovementTypeTable` și gardianul politicii. O valoare din afara listei nu e
        // avertisment la validare, ci respingerea fișierului ÎNTREG.
        Console.WriteLine($"     MĂSURAT (D17-V1/nomenclator): {SaftReguli.CoduriMiscare.Count} coduri "
            + $"({string.Join(", ", SaftReguli.CoduriMiscare.Keys)}).");
        s.Check("D17-V1 `SaftReguli.CoduriMiscare` = cele 19 tipuri de mișcare din D406 S, cu descrieri "
            + "nevide și distincte; `EsteCodMiscare` acceptă doar codurile din listă, iar golul/null NU e "
            + "cod valid (absența codului e o EXCLUDERE deliberată, tratată explicit, nu un cod)",
            SaftReguli.CoduriMiscare.Count == 19
            && SaftReguli.CoduriMiscare.Keys.OrderBy(k => k, StringComparer.Ordinal).SequenceEqual(
                new[] { "10", "100", "101", "110", "120", "130", "140", "150", "160", "170", "180",
                        "20", "30", "40", "50", "60", "70", "80", "90" })
            && SaftReguli.CoduriMiscare.Values.All(v => !string.IsNullOrWhiteSpace(v))
            && SaftReguli.CoduriMiscare.Values.Distinct(StringComparer.Ordinal).Count() == 19
            && SaftReguli.CoduriMiscare["10"] == "Achiziție" && SaftReguli.CoduriMiscare["70"] == "Consum"
            && SaftReguli.CoduriMiscare["101"] == "Diferențe de preț negative"
            && SaftReguli.CoduriMiscare["180"] == "Alte tranzacții"
            && SaftReguli.EsteCodMiscare("10") && SaftReguli.EsteCodMiscare("180")
            && !SaftReguli.EsteCodMiscare("999") && !SaftReguli.EsteCodMiscare("1")
            && !SaftReguli.EsteCodMiscare("") && !SaftReguli.EsteCodMiscare(null));

        // ---------------- Modelul EF (D17-D1) ----------------
        using (var osModel = s.Provider.CreateObjectSpace()) {
            var entitate = ((EFCoreObjectSpace)osModel).DbContext.Model
                .FindEntityType(typeof(PoliticaMiscareSaft));
            var indexuri = entitate?.GetIndexes().ToList() ?? [];
            var tripleta = indexuri.FirstOrDefault(i => i.IsUnique
                && i.Properties.Select(p => p.Name).SequenceEqual(
                    [nameof(PoliticaMiscareSaft.TipDocumentId), nameof(PoliticaMiscareSaft.TipStoc),
                     nameof(PoliticaMiscareSaft.Semn)]));
            var perechea = indexuri.FirstOrDefault(i => i.IsUnique
                && i.Properties.Select(p => p.Name).SequenceEqual(
                    [nameof(PoliticaMiscareSaft.TipDocumentId), nameof(PoliticaMiscareSaft.TipStoc)]));
            Console.WriteLine($"     MĂSURAT (D17-V1/model): {indexuri.Count} indexuri; "
                + $"tripleta filtrată pe „{tripleta?.GetFilter() ?? "<LIPSĂ>"}”, "
                + $"perechea filtrată pe „{perechea?.GetFilter() ?? "<LIPSĂ>"}”; "
                + $"CodMiscare MaxLength={entitate?.FindProperty(nameof(PoliticaMiscareSaft.CodMiscare))?.GetMaxLength()}, "
                + $"Motiv MaxLength={entitate?.FindProperty(nameof(PoliticaMiscareSaft.Motiv))?.GetMaxLength()}, "
                + $"Semn nullabil={entitate?.FindProperty(nameof(PoliticaMiscareSaft.Semn))?.IsNullable}.");
            s.Check("D17-V1 `PoliticaMiscareSaft`: DOUĂ indexuri unice — tripleta (TipDocumentId, "
                + "TipStoc, Semn) nefiltrată și perechea (TipDocumentId, TipStoc) pe `Semn IS NULL`. "
                + "Al doilea NU e redundant: în Postgres `NULL <> NULL`, deci fără el două "
                + "rânduri „orice semn” pe aceeași pereche ar fi trecut, iar potrivirea ar fi devenit "
                + "nedeterministă",
                tripleta != null && tripleta.GetFilter() == null
                && perechea != null && perechea.GetFilter() == "\"Semn\" IS NULL");
            s.Check("D17-V1 `PoliticaMiscareSaft`: `CodMiscare` NULLABIL cu `MaxLength 9` (lungimea lui "
                + "`MovementType` din XSD), `Motiv` nullabil 256, `Semn` nullabil (null = orice semn)",
                entitate.FindProperty(nameof(PoliticaMiscareSaft.CodMiscare)) is { IsNullable: true } cod
                && cod.GetMaxLength() == 9
                && entitate.FindProperty(nameof(PoliticaMiscareSaft.Motiv)) is { IsNullable: true } motiv
                && motiv.GetMaxLength() == 256
                && entitate.FindProperty(nameof(PoliticaMiscareSaft.Semn)).IsNullable);
        }

        // ---------------- Seed-ul per profil (D17-D1) ----------------
        using (var os = s.Provider.CreateObjectSpace()) {
            var randuri = os.GetObjectsQuery<PoliticaMiscareSaft>()
                .Select(p => new { Tip = p.TipDocument.Cod, p.TipStoc, p.Semn, p.CodMiscare, p.RolTert, p.Motiv })
                .ToList();
            if (!privat) {
                Console.WriteLine($"     MĂSURAT (D17-V1/bugetar): {randuri.Count} politici de mișcare SAF-T.");
                s.Check("D17-V1 (bugetar) profilul n-are NICIO politică de mișcare SAF-T: declarația D406 (L și S) "
                    + "e neaplicabilă planului instituțiilor publice (73c), deci nici mișcările lui de stoc n-au "
                    + "cod — absența e PROBATĂ, nu presupusă",
                    randuri.Count == 0);
                return;
            }

            var asteptate = ContaSeeder.MiscariSaftPrivat;
            string Cod(string tip, TipStoc stoc, int? semn) => randuri
                .Where(r => r.Tip == tip && r.TipStoc == stoc && r.Semn == semn)
                .Select(r => r.CodMiscare ?? "<null>").SingleOrDefault() ?? "<LIPSĂ>";
            RolTertSaft? Rol(string tip, TipStoc stoc, int? semn) => randuri
                .Where(r => r.Tip == tip && r.TipStoc == stoc && r.Semn == semn)
                .Select(r => (RolTertSaft?)r.RolTert).SingleOrDefault();
            Console.WriteLine($"     MĂSURAT (D17-V1/privat): {randuri.Count} politici (tabel: {asteptate.Count}); "
                + string.Join(", ", randuri
                    .OrderBy(r => r.Tip, StringComparer.Ordinal).ThenBy(r => r.TipStoc).ThenBy(r => r.Semn)
                    .Select(r => $"{r.Tip}/{r.TipStoc}/{r.Semn?.ToString() ?? "±"}→{r.CodMiscare ?? "EXCLUS"}"
                        + $"{(r.RolTert == RolTertSaft.Niciunul ? "" : "/" + r.RolTert)}")) + ".");

            // Numărul NU e o constantă scrisă în probă: e cardinalitatea tabelului de
            // seed × „fiecare rând al lui există exact o dată în bază". O politică
            // adăugată în tabel fără rând în bază (sau invers) pică aici.
            s.Check("D17-V1 (privat) seed-ul scrie EXACT tabelul S3-D3 — 23 de rânduri, câte unul pentru fiecare "
                + "(tip × categorie × semn) pe care profilul chiar îl produce: 9 tipuri (FCT/NIR/BTR/BCS/LDI/DSC/ASM/"
                + "RLF/RDC) × categoriile `Magazie`+`Marfuri`, plus `Consum` la BCS",
                randuri.Count == 23 && asteptate.Count == 23
                && randuri.Select(r => (r.Tip, r.TipStoc, r.Semn)).Distinct().Count() == 23
                && asteptate.All(a => randuri.Any(r => r.Tip == a.Tip && r.TipStoc == a.TipStoc && r.Semn == a.Semn
                    && r.CodMiscare == a.Cod && r.RolTert == a.Rol && r.Motiv == a.Motiv))
                && randuri.All(r => asteptate.Any(a => a.Tip == r.Tip && a.TipStoc == r.TipStoc && a.Semn == r.Semn)));

            s.Check("D17-V1 (privat) fiecare cod seed-uit e din nomenclator, iar singurul rând FĂRĂ cod are motiv "
                + "scris — invariantul V („nimic nu se pierde”): excluderea se citește, nu se ghicește",
                randuri.All(r => r.CodMiscare == null || SaftReguli.EsteCodMiscare(r.CodMiscare))
                && randuri.Count(r => r.CodMiscare == null) == 1
                && randuri.Where(r => r.CodMiscare == null).All(r => !string.IsNullOrWhiteSpace(r.Motiv)));

            s.Check("D17-V1 (privat) intrările de marfă și ieșirile către terți poartă ROLUL: NIR ± → `10` "
                + "Achiziție/Furnizor, DSC − → `30` Vânzare/Client, RLF − → `50` Retur către furnizor/Furnizor, "
                + "RDC + → `40` Retur de la client/Client — rolul e al politicii, identificatorii îi pune legea",
                Cod("FCT", TipStoc.Magazie, null) == "10" && Cod("FCT", TipStoc.Marfuri, null) == "10"
                && Rol("FCT", TipStoc.Marfuri, null) == RolTertSaft.Furnizor
                && Cod("NIR", TipStoc.Magazie, null) == "10" && Cod("NIR", TipStoc.Marfuri, null) == "10"
                && Rol("NIR", TipStoc.Magazie, null) == RolTertSaft.Furnizor
                && Cod("DSC", TipStoc.Magazie, -1) == "30" && Cod("DSC", TipStoc.Marfuri, -1) == "30"
                && Rol("DSC", TipStoc.Marfuri, -1) == RolTertSaft.Client
                && Cod("RLF", TipStoc.Magazie, -1) == "50" && Rol("RLF", TipStoc.Magazie, -1) == RolTertSaft.Furnizor
                && Cod("RDC", TipStoc.Marfuri, +1) == "40" && Rol("RDC", TipStoc.Marfuri, +1) == RolTertSaft.Client);

            s.Check("D17-V1 (privat) mișcările INTERNE n-au terț (`Niciunul`) și își iau semnul de unde îl au: "
                + "BTR = `80` pe AMBELE picioare (`Semn` null — transferul e aceeași mișcare, nu două), LDI "
                + "plus `110` / minus `120`, ASM produs `20` / consum `70`",
                Cod("BTR", TipStoc.Magazie, null) == "80" && Cod("BTR", TipStoc.Marfuri, null) == "80"
                && Rol("BTR", TipStoc.Magazie, null) == RolTertSaft.Niciunul
                && Cod("LDI", TipStoc.Magazie, +1) == "110" && Cod("LDI", TipStoc.Marfuri, +1) == "110"
                && Cod("LDI", TipStoc.Magazie, -1) == "120" && Cod("LDI", TipStoc.Marfuri, -1) == "120"
                && Cod("ASM", TipStoc.Magazie, +1) == "20" && Cod("ASM", TipStoc.Marfuri, +1) == "20"
                && Cod("ASM", TipStoc.Magazie, -1) == "70" && Cod("ASM", TipStoc.Marfuri, -1) == "70"
                && new[] { "BTR", "LDI", "ASM" }.All(t => randuri.Where(r => r.Tip == t)
                    .All(r => r.RolTert == RolTertSaft.Niciunul)));

            s.Check("D17-V1 (privat) BCS: ieșirea din magazie e `70` Consum, iar INTRAREA în `Consum` e EXCLUSĂ "
                + "deliberat (cod null + motiv) — consumul rămâne pe responsabil (27a), dar nu mai e stoc în "
                + "magazie; declarată, ar fi dublat ieșirea aceleiași linii",
                Cod("BCS", TipStoc.Magazie, -1) == "70" && Cod("BCS", TipStoc.Marfuri, -1) == "70"
                && Cod("BCS", TipStoc.Consum, +1) == "<null>"
                && randuri.Single(r => r.Tip == "BCS" && r.TipStoc == TipStoc.Consum)
                    .Motiv.Contains("responsabil"));

        }

        // ---------------- Re-seed-ul nu dublează (funcția REALĂ) ----------------
        {
            int inainte, dupa;
            using (var osCitire = s.Provider.CreateObjectSpace())
                inainte = osCitire.GetObjectsQuery<PoliticaMiscareSaft>().Count();
            using (var osSeed = s.Provider.CreateObjectSpace()) {
                ContaSeeder.SeedPoliticiMiscareSaftPrivat(osSeed);
                osSeed.CommitChanges();
            }
            using (var osCitire = s.Provider.CreateObjectSpace())
                dupa = osCitire.GetObjectsQuery<PoliticaMiscareSaft>().Count();
            Console.WriteLine($"     MĂSURAT (D17-V1/re-seed): {inainte} → {dupa} politici după a doua rulare a "
                + "seed-ului real.");
            s.Check("D17-V1 (privat) `SeedPoliticiMiscareSaft` e IDEMPOTENT pe cheia (tip × registru × semn): "
                + "`--forceUpdate` pe o bază deja seed-uită nu adaugă un al doilea rând (care ar fi făcut "
                + "potrivirea ambiguă)",
                inainte == 23 && dupa == 23);
        }

        // ---------------- Gardianul (D17-D1), pe ușa comună ----------------
        // `TipStoc.Custodie` = cheie LIBERĂ (nimic nu o produce azi): probele de gard
        // nu se ating de rândurile seed-uite și n-au nevoie de curățenie — niciun
        // commit, doar `Rollback`.
        using (var osGard = s.Provider.CreateObjectSpace()) {
            var p = osGard.CreateObject<PoliticaMiscareSaft>();
            p.TipDocument = osGard.FirstOrDefault<TipDocument>(t => t.Cod == "BCS");
            p.TipStoc = TipStoc.Custodie;
            p.Semn = +1;
            p.CodMiscare = "999";
            string mesajCod = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajCod = e.Message; }
            p.CodMiscare = null;
            string mesajFaraMotiv = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajFaraMotiv = e.Message; }
            p.Motiv = "custodia nu se raportează încă";
            string mesajExcludere = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajExcludere = e.Message; }
            p.CodMiscare = "80";
            p.Motiv = null;
            p.Semn = 2;
            string mesajSemn = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajSemn = e.Message; }
            p.Semn = null;
            string mesajBun = null;
            try { GardianEditare.Verifica(osGard); } catch (OperareException e) { mesajBun = e.Message; }
            Console.WriteLine($"     MĂSURAT (D17-V1/gardian): cod „999” → „{mesajCod ?? "<NU A ARUNCAT>"}”; "
                + $"cod null fără motiv → „{mesajFaraMotiv ?? "<NU A ARUNCAT>"}”; cod null cu motiv → "
                + $"„{mesajExcludere ?? "tace"}”; Semn = 2 → „{mesajSemn ?? "<NU A ARUNCAT>"}”; "
                + $"cod „80” + Semn null → „{mesajBun ?? "tace"}”.");
            s.Check("D17-V1 gardianul politicii, pe ușa COMUNĂ (nu doar regula XAF — politica e editabilă și "
                + "validarea XAF nu rulează pe API, 55b): cod în afara nomenclatorului ⇒ refuz (fișierul ar fi "
                + "respins INTEGRAL), cod null fără motiv ⇒ refuz (excluderea deliberată se citește), `Semn = 2` "
                + "⇒ refuz (o politică ce nu s-ar potrivi niciodată, tăcut); rândurile corecte trec",
                mesajCod != null && mesajCod.Contains("999")
                && mesajFaraMotiv != null && mesajFaraMotiv.Contains("motiv")
                && mesajExcludere == null
                && mesajSemn != null && mesajSemn.Contains("2")
                && mesajBun == null);
            osGard.Rollback();
        }

        // ---------------- Unicitatea, pe CALEA REALĂ (66h) ----------------
        // Nu adnotarea din model (aceea s-a măsurat mai sus), ci COMMIT-ul contra
        // bazei: un filtru de index scris greșit trece de orice probă de model și
        // pică abia în producție. Cele două chei se probează separat, fiindcă le
        // apără indexuri diferite — a doua e chiar cea pe care semantica lui NULL în
        // SQL ar fi lăsat-o descoperită. Niciun rând nu ajunge comis (tranzacția
        // eșuată se derulează), deci nu e nevoie de purjă.
        //
        // MĂSURAT la prima rulare: fără linia de mai jos, traducerea iese în ENGLEZĂ
        // („A 'PoliticaMiscareSaft' record with the same … already exists.").
        // Template-urile RO sunt statice pe PROCES și le instalează HOST-urile la
        // pornire (`MesajeConstraintRo.Aplica()` în ambele `Startup`-uri, 39a);
        // ModelCheck nu e host, deci ar fi măsurat un mesaj pe care niciun operator
        // nu-l vede. Apelul e idempotent și e chiar jumătatea de host a probei.
        Atlas.Conta.BackOffice.Module.BusinessObjects.MesajeConstraintRo.Aplica();
        string Duplica(string tip, TipStoc stoc, int? semn, string cod) {
            using var osDup = s.Provider.CreateObjectSpace();
            var p = osDup.CreateObject<PoliticaMiscareSaft>();
            p.TipDocument = osDup.FirstOrDefault<TipDocument>(t => t.Cod == tip);
            p.TipStoc = stoc;
            p.Semn = semn;
            p.CodMiscare = cod;
            try {
                osDup.CommitChanges();
                return null;
            }
            catch (Exception e) {
                var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(e);
                return violare != null
                    ? Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)
                    : e.GetBaseException().Message;
            }
        }
        {
            var mesajSemnFix = Duplica("BCS", TipStoc.Magazie, -1, "70");
            var mesajSemnNull = Duplica("NIR", TipStoc.Magazie, null, "10");
            var mesajLiber = Duplica("BCS", TipStoc.Magazie, +1, "110");
            Console.WriteLine($"     MĂSURAT (D17-V1/unicitate): al doilea (BCS, Magazie, −1) → "
                + $"„{mesajSemnFix ?? "<A TRECUT>"}”; al doilea (NIR, Magazie, orice semn) → "
                + $"„{mesajSemnNull ?? "<A TRECUT>"}”; (BCS, Magazie, +1) — cheie liberă → "
                + $"„{mesajLiber ?? "acceptat"}”.");
            s.Check("D17-V1 (privat) al doilea rând pe aceeași cheie e REFUZAT de bază, pe ambele forme ale ei: "
                + "semn fix (BCS/Magazie/−1) și „orice semn” (NIR/Magazie/null — cazul pe care `NULL <> NULL` "
                + "l-ar fi lăsat să treacă); refuzul iese ca mesaj de DOMENIU prin `ConstraintViolationTranslator` "
                + "(39a), nu ca 23505 brut, iar o cheie liberă rămâne acceptată",
                mesajSemnFix != null && mesajSemnFix.Contains("Există deja")
                && mesajSemnNull != null && mesajSemnNull.Contains("Există deja")
                && mesajLiber == null);
            // Rândul de cheie liberă a fost chiar COMIS (proba era că trece) — purjă
            // FIZICĂ, ca orice artefact de scenă (70e).
            using (var osCuratenie = s.Provider.CreateObjectSpace()) {
                var tipBcs = osCuratenie.FirstOrDefault<TipDocument>(t => t.Cod == "BCS").ID;
                new Purja(osCuratenie)
                    .Adauga(osCuratenie.GetObjectsQuery<PoliticaMiscareSaft>()
                        .Where(p => p.TipDocumentId == tipBcs && p.TipStoc == TipStoc.Magazie && p.Semn == 1))
                    .Executa();
            }
            int dupaCuratenie;
            using (var osCitire = s.Provider.CreateObjectSpace())
                dupaCuratenie = osCitire.GetObjectsQuery<PoliticaMiscareSaft>().Count();
            s.Check("D17-V1 (privat) curățenie: baza rămâne cu cele 23 de politici seed-uite (rândul de probă "
                + "purjat FIZIC — o ștergere logică ar fi fost citită de seed-ul următor ca decizie a "
                + "utilizatorului)",
                dupaCuratenie == 23);
        }
    }
}
