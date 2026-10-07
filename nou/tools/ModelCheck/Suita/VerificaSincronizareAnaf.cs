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

// ============ Felia 15, pas 2: merge-ul ANAF — D15-V2 + D15-V3 ============
// Ce probează: regula de merge (D15-D3) pe parteneri FABRICAȚI, fără bază și
// fără rețea, și deserializarea răspunsului v9 (D15-D2) pe fixture-uri, dintre
// care unul REAL (salvat la implementare din `webservicesp.anaf.ro`).
//
// DE CE offline: suita trebuie să rămână deterministă (V4 din contract face
// apelul real, pe calea HTTP, în afara ei). `Aplica` e o funcție pură tocmai ca
// să poată fi probată așa — singura ei legătură cu baza, lookup-ul de județ,
// intră ca `Func<string, Judet>`, aici o mână de obiecte fabricate.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaAdresaPartener`.
// Rulează identic pe cele două profiluri (nu atinge planul de conturi) — dar
// tocmai de aceea se cheamă din amândouă: regula nu are voie să depindă de ele.
static class VerificaSincronizareAnaf {
    public static void Ruleaza(Suita s) {
        // Nomenclatorul de județe, fabricat: `Aplica` nu cunoaște `IObjectSpace`.
        var judete = JudeteRo.Toate.ToDictionary(
            j => j.Cod,
            j => new Judet { ID = Guid.NewGuid(), Cod = j.Cod, Denumire = j.Denumire, CodAuto = j.CodAuto, CodCnp = j.CodCnp },
            StringComparer.Ordinal);
        Judet CautaJudet(string cod) => judete.GetValueOrDefault(cod);
        var timbru = new DateTime(2026, 8, 25, 9, 0, 0, DateTimeKind.Utc);

        // Adresa ANAF de referință (Cluj), în forma aplatizată de client.
        AdresaAnaf AdresaCluj(string strada = "Str. Avram Iancu", string judetAuto = "CJ") =>
            new(strada, "1", "Cluj-Napoca", judetAuto, "400089", "Ap. 3");
        DateAnaf DateCluj(AdresaAnaf adresa = null, string denumire = "PROBĂ SRL", string nrRegCom = "J12/999/2003",
                bool? scpTva = true, bool? tvaIncasare = false, bool? inactiv = false) =>
            new(12345, denumire, nrRegCom, "400089", "INREGISTRAT", scpTva, tvaIncasare, inactiv,
                adresa ?? AdresaCluj(), null);

        // Partenerul care nu schimbă nimic în afara adresei: denumirea, nr. reg. com.
        // și cele trei flag-uri canonice coincid deja cu ANAF, ca numărătoarea de
        // `Modificari` să măsoare EXACT blocul de adresă.
        Partener PartenerAliniat() => new() {
            Cod = "V2", Denumire = "PROBĂ SRL", RegistruComert = "J12/999/2003",
            Tara = "RO", CodFiscal = "RO12345",
            InregistratTva = true, TvaLaIncasare = false, InactivFiscal = false,
        };

        // --- (1) Blocul gol se umple integral: 6 câmpuri, 6 modificări ---
        {
            var p = PartenerAliniat();
            var r = SincronizareAnafService.Aplica(p, DateCluj(), suprascrie: false, timbru, CautaJudet);
            var campuri = r.Modificari.Select(m => m.Camp).OrderBy(x => x, StringComparer.Ordinal).ToList();
            Console.WriteLine($"     MĂSURAT (D15-V2/bloc gol): modificări [{string.Join(", ", campuri)}]; "
                + $"județ {p.Judet?.Cod}; timbru {p.DataSincronizareAnaf:O}; diferențe {r.Diferente.Count}.");
            s.Check("D15-V2 bloc de adresă GOL ⇒ se umplu toate cele 6 câmpuri (5 de text + județul), fiecare cu "
                + "`Modificare` de la null; nicio `Diferenta`; timbrul se pune chiar dacă restul câmpurilor "
                + "coincideau deja cu ANAF",
                campuri.SequenceEqual(["CodPostal", "DetaliiAdresa", "Judet", "Localitate", "Numar", "Strada"])
                && p.Strada == "Str. Avram Iancu" && p.Numar == "1" && p.Localitate == "Cluj-Napoca"
                && p.CodPostal == "400089" && p.DetaliiAdresa == "Ap. 3"
                && p.Judet?.Cod == "RO-CJ" && p.JudetId == judete["RO-CJ"].ID
                && r.Modificari.All(m => m.Vechi == null)
                && r.Diferente.Count == 0 && r.Avertismente.Count == 0
                && p.DataSincronizareAnaf == timbru && r.Gasit && r.Cui == 12345);
        }

        // --- (2) Riscul 3 al contractului: câmp GOL individual se umple și el ---
        // Un partener cu doar `Localitate` culeasă NU e „bloc gol" — și totuși
        // strada trebuie să se umple, fiindcă regula e per CÂMP: gol ⇒ umple.
        // Diferența e rezervată valorilor NE-goale care nu se potrivesc.
        {
            var p = PartenerAliniat();
            p.Localitate = "Cluj-Napoca";
            var r = SincronizareAnafService.Aplica(p, DateCluj(), suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/gol individual): modificări "
                + $"[{string.Join(", ", r.Modificari.Select(m => m.Camp))}]; diferențe {r.Diferente.Count}.");
            s.Check("D15-V2 (riscul 3) blocul PARȚIAL completat nu blochează umplerea: localitatea culeasă și egală "
                + "nu produce nimic, restul câmpurilor goale se umplu — „gol ⇒ umple” e per câmp, nu per bloc",
                r.Modificari.Count == 5 && !r.Modificari.Any(m => m.Camp == nameof(Partener.Localitate))
                && p.Strada == "Str. Avram Iancu" && r.Diferente.Count == 0);
        }

        // --- (3) Valoare NE-goală diferită ⇒ `Diferenta`, obiectul NEATINS ---
        {
            var p = PartenerAliniat();
            p.Strada = "Str. Altundeva";
            p.Localitate = "Cluj Napoca";   // fără cratimă: diferență reală, ordinală
            var r = SincronizareAnafService.Aplica(p, DateCluj(), suprascrie: false, timbru, CautaJudet);
            var diferente = r.Diferente.Select(d => d.Camp).OrderBy(x => x, StringComparer.Ordinal).ToList();
            Console.WriteLine($"     MĂSURAT (D15-V2/diferit): diferențe [{string.Join(", ", diferente)}]; "
                + $"strada rămasă „{p.Strada}”, localitatea „{p.Localitate}”.");
            s.Check("D15-V2 valoare culeasă NE-goală și diferită ⇒ `Diferenta` (camp, cules, anaf) și obiectul rămâne "
                + "NEATINS; egalitatea e ORDINALĂ (o cratimă lipsă e o diferență, nu un fleac de normalizat)",
                diferente.SequenceEqual([nameof(Partener.Localitate), nameof(Partener.Strada)])
                && p.Strada == "Str. Altundeva" && p.Localitate == "Cluj Napoca"
                && r.Diferente.Any(d => d.Camp == nameof(Partener.Strada)
                    && d.Cules == "Str. Altundeva" && d.Anaf == "Str. Avram Iancu"));
        }

        // --- (4) `suprascrie` ⇒ scris, cu valoarea VECHE în `Modificare` (riscul 2) ---
        {
            var p = PartenerAliniat();
            p.Strada = "Str. Altundeva";
            p.Denumire = "Denumirea contabilului SRL";
            var r = SincronizareAnafService.Aplica(p, DateCluj(), suprascrie: true, timbru, CautaJudet);
            var strada = r.Modificari.FirstOrDefault(m => m.Camp == nameof(Partener.Strada));
            var denumire = r.Modificari.FirstOrDefault(m => m.Camp == nameof(Partener.Denumire));
            Console.WriteLine($"     MĂSURAT (D15-V2/suprascrie): strada „{strada?.Vechi}” → „{strada?.Nou}”; "
                + $"denumirea „{denumire?.Vechi}” → „{denumire?.Nou}”; diferențe {r.Diferente.Count}.");
            s.Check("D15-V2 (riscul 2) `suprascrie` scrie peste valoarea culeasă — inclusiv `Denumire` —, dar fiecare "
                + "scriere iese ca `Modificare` cu valoarea VECHE: lotul cu suprascriere e ireversibil, deci "
                + "trebuie să lase listă, nu contor",
                strada is { Vechi: "Str. Altundeva", Nou: "Str. Avram Iancu" }
                && denumire is { Vechi: "Denumirea contabilului SRL", Nou: "PROBĂ SRL" }
                && p.Strada == "Str. Avram Iancu" && p.Denumire == "PROBĂ SRL"
                && r.Diferente.Count == 0);
        }

        // --- (5) Canonicul (D4-r1) bate culegerea, în ambele sensuri ---
        {
            var p = PartenerAliniat();
            p.InregistratTva = true;
            p.TvaLaIncasare = false;
            p.InactivFiscal = false;
            var r = SincronizareAnafService.Aplica(p,
                DateCluj(scpTva: false, tvaIncasare: true, inactiv: true), suprascrie: false, timbru, CautaJudet);
            var scp = r.Modificari.FirstOrDefault(m => m.Camp == nameof(Partener.InregistratTva));
            Console.WriteLine($"     MĂSURAT (D15-V2/canonic): InregistratTva „{scp?.Vechi}” → „{scp?.Nou}”; "
                + $"TvaLaIncasare {p.TvaLaIncasare}; InactivFiscal {p.InactivFiscal}; diferențe {r.Diferente.Count}.");
            s.Check("D15-V2 pe axa TVA registrul ANAF e CANONIC (D4-r1): `scpTVA` fals peste `InregistratTva` adevărat "
                + "se SCRIE (nu se raportează ca diferență), la fel `statusTvaIncasare` și `statusInactivi`; "
                + "schimbarea rămâne ca `Modificare`",
                scp is { Vechi: "da", Nou: "nu" } && !p.InregistratTva
                && p.TvaLaIncasare && p.InactivFiscal
                && r.Diferente.Count == 0
                && r.Modificari.Count(m => m.Camp is nameof(Partener.InregistratTva)
                    or nameof(Partener.TvaLaIncasare) or nameof(Partener.InactivFiscal)) == 3);
        }

        // --- (6) `Denumire` culeasă NU se rescrie implicit; goală se umple ---
        {
            var pCules = PartenerAliniat();
            pCules.Denumire = "Denumirea contabilului SRL";
            var rCules = SincronizareAnafService.Aplica(pCules, DateCluj(), suprascrie: false, timbru, CautaJudet);
            var pGol = PartenerAliniat();
            pGol.Denumire = "   ";
            var rGol = SincronizareAnafService.Aplica(pGol, DateCluj(), suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/denumire): culeasă → „{pCules.Denumire}” "
                + $"(diferență: {rCules.Diferente.Any(d => d.Camp == nameof(Partener.Denumire))}); "
                + $"goală → „{pGol.Denumire}”.");
            s.Check("D15-V2 `Denumire` e ETICHETA contabilului: una culeasă și diferită rămâne NEATINSĂ și iese ca "
                + "`Diferenta`; una GOALĂ se umple din ANAF (n-are ce pierde — „gol ⇒ umple” e titlul lui D15-D3)",
                pCules.Denumire == "Denumirea contabilului SRL"
                && rCules.Diferente.Any(d => d.Camp == nameof(Partener.Denumire) && d.Anaf == "PROBĂ SRL")
                && pGol.Denumire == "PROBĂ SRL"
                && rGol.Modificari.Any(m => m.Camp == nameof(Partener.Denumire) && m.Vechi == null));
        }

        // --- (7) Județ necunoscut ⇒ avertisment, câmpul rămâne ---
        {
            var p = PartenerAliniat();
            var r = SincronizareAnafService.Aplica(p, DateCluj(AdresaCluj(judetAuto: "XX")),
                suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/județ): „{r.Avertismente.FirstOrDefault()}”; "
                + $"JudetId {p.JudetId?.ToString() ?? "null"}.");
            s.Check("D15-V2 indicativ auto necunoscut („XX”, care nu e în ISO 3166-2:RO) ⇒ AVERTISMENT nominal și "
                + "câmpul rămâne gol — un județ nerezolvat nu se ghicește, se raportează; restul adresei se "
                + "umple normal",
                p.JudetId == null && p.Judet == null
                && r.Avertismente.Any(a => a.Contains("XX") && a.Contains("nerezolvat"))
                && p.Strada == "Str. Avram Iancu"
                && !r.Modificari.Any(m => m.Camp == nameof(Partener.Judet)));
        }

        // --- (8) Trunchierea la lungimea SAF-T, cu avertisment ---
        {
            var lunga = new string('A', 90);
            var p = PartenerAliniat();
            var r = SincronizareAnafService.Aplica(p, DateCluj(AdresaCluj(strada: lunga)),
                suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/trunchiere): strada 90 → {p.Strada?.Length}; "
                + $"„{r.Avertismente.FirstOrDefault()}”.");
            s.Check("D15-V2 valoarea ANAF mai lungă decât coloana se TAIE la `MaxLength` (70 = `StreetName` din "
                + "SAF-T) și lasă avertisment — lungimea se citește prin reflecție din model, nu dintr-o a doua "
                + "listă scrisă cu mâna",
                p.Strada?.Length == 70 && p.Strada == new string('A', 70)
                && r.Avertismente.Any(a => a.Contains("Strada") && a.Contains("90") && a.Contains("70")));
        }

        // --- (9) ANAF TACE pe un bloc întreg ⇒ câmpul canonic rămâne, cu avertisment ---
        // Întărire față de litera lui D15-D3 („se scrie ÎNTOTDEAUNA"): pe orice
        // răspuns REAL blocul există, dar dacă lipsește, un `bool` implicit fals ar
        // DE-înregistra în masă parteneri plătitori de TVA. „Nu a spus" ≠ „a spus nu".
        {
            var p = PartenerAliniat();
            p.InregistratTva = true;
            var r = SincronizareAnafService.Aplica(p, DateCluj(scpTva: null), suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/tăcere): InregistratTva rămas {p.InregistratTva}; "
                + $"„{r.Avertismente.FirstOrDefault(a => a.Contains("scopuri"))}”.");
            s.Check("D15-V2 blocul `inregistrare_scop_Tva` ABSENT din răspuns ⇒ `InregistratTva` rămâne neschimbat + "
                + "avertisment; regula „canonic” se aplică la ce ANAF a DECLARAT, nu la tăcerea lui",
                p.InregistratTva
                && r.Avertismente.Any(a => a.Contains("n-a raportat"))
                && !r.Modificari.Any(m => m.Camp == nameof(Partener.InregistratTva)));
        }

        // --- (10) `notFound` (riscul 4): nicio scriere, FĂRĂ timbru ---
        {
            var p = PartenerAliniat();
            p.InregistratTva = true;
            var r = SincronizareAnafService.Negasit(p, 12345);
            Console.WriteLine($"     MĂSURAT (D15-V2/negăsit): timbru {p.DataSincronizareAnaf?.ToString("O") ?? "null"}; "
                + $"„{r.Avertismente.FirstOrDefault()}”.");
            s.Check("D15-V2 (riscul 4) CUI în `notFound` (ANAF răspunde 200 și pe coduri radiate) ⇒ ZERO scrieri, "
                + "`InregistratTva` rămâne ce era, TIMBRUL nu se pune (n-a fost o sincronizare) și iese avertisment",
                p.DataSincronizareAnaf == null && p.InregistratTva
                && r.Modificari.Count == 0 && r.Diferente.Count == 0
                && !r.Gasit && r.Avertismente.Count == 1 && r.Avertismente[0].Contains("nu figurează"));
        }

        // --- (11) Sursa adresei: domiciliul fiscal bate sediul social, dacă are localitate ---
        {
            var sediu = new AdresaAnaf("Şos. Virtuţii", "148", "Sector 6 Mun. Bucureşti", "B", "60787", "spatiul E47");
            var domiciliu = new AdresaAnaf("Str. Gara Herăstrău", "6", "Sector 2 Mun. Bucureşti", "B", "", "Cladirea Globalworth Square");
            var pAmbele = PartenerAliniat();
            SincronizareAnafService.Aplica(pAmbele,
                new DateAnaf(1, "X", "", "12345", "", true, false, false, domiciliu, sediu),
                suprascrie: false, timbru, CautaJudet);
            var pDoarSediu = PartenerAliniat();
            SincronizareAnafService.Aplica(pDoarSediu,
                new DateAnaf(1, "X", "", "12345", "", true, false, false, null, sediu),
                suprascrie: false, timbru, CautaJudet);
            var pFaraLocalitate = PartenerAliniat();
            var rFara = SincronizareAnafService.Aplica(pFaraLocalitate,
                new DateAnaf(1, "X", "", "12345", "", true, false, false,
                    new AdresaAnaf("Str. Fără Oraș", "1", "", "CJ", "", ""), null),
                suprascrie: false, timbru, CautaJudet);
            Console.WriteLine($"     MĂSURAT (D15-V2/sursă): ambele → „{pAmbele.Strada}”/cod poștal "
                + $"„{pAmbele.CodPostal}”; doar sediu → „{pDoarSediu.Strada}”; fără localitate → "
                + $"„{pFaraLocalitate.Strada ?? "null"}”, „{rFara.Avertismente.FirstOrDefault()}”.");
            s.Check("D15-V2 sursa adresei = domiciliul fiscal DACĂ are localitate (singurul câmp obligatoriu al lui "
                + "`AddressStructure`), altfel sediul social; codul poștal gol pe adresa aleasă cade pe "
                + "`date_generale.codPostal` (cazul real 14399840); nicio adresă cu localitate ⇒ avertisment, "
                + "zero scrieri de adresă",
                pAmbele.Strada == "Str. Gara Herăstrău" && pAmbele.CodPostal == "12345"
                && pDoarSediu.Strada == "Şos. Virtuţii" && pDoarSediu.CodPostal == "60787"
                && pFaraLocalitate.Strada == null && pFaraLocalitate.Localitate == null
                && rFara.Avertismente.Any(a => a.Contains("nicio adresă")));
        }

        // --- (12) `CuiInterogabil` și candidatura (riscul 7) ---
        {
            var pfaCuCui = new Partener { Cod = "V2-PFA", Tara = "RO", CodFiscal = "RO 30123456", TipPersoana = TipPersoana.Fizica };
            var pfCnp = new Partener { Cod = "V2-CNP", Tara = "RO", CodFiscal = "1850101123456", TipPersoana = TipPersoana.Fizica };
            var strain = new Partener { Cod = "V2-DE", Tara = "DE", CodFiscal = "DE123456789" };
            var faraCod = new Partener { Cod = "V2-FARA", Tara = "RO", CodFiscal = "  " };
            var cuLitere = new Partener { Cod = "V2-LIT", Tara = "RO", CodFiscal = "RO12A45" };
            Console.WriteLine($"     MĂSURAT (D15-V2/CUI): „RO 123 45” → {SincronizareAnafService.CuiInterogabil("RO 123 45")}; "
                + $"PFA → {SincronizareAnafService.CuiInterogabil(pfaCuCui)}; "
                + $"CNP → „{SincronizareAnafService.Interogabilitate(pfCnp).Motiv}”; "
                + $"DE → „{SincronizareAnafService.Interogabilitate(strain).Motiv}”; "
                + $"fără cod → „{SincronizareAnafService.Interogabilitate(faraCod).Motiv}”; "
                + $"cu litere → „{SincronizareAnafService.Interogabilitate(cuLitere).Motiv}”.");
            s.Check("D15-V2 (riscul 7) `CuiInterogabil`: „RO 123 45” ⇒ 12345 (prefix tăiat, spații ignorate); un PFA "
                + "cu CUI E candidat (înregistrat bate tot — 71b); CNP-ul de 13 cifre NU se interoghează, cu motiv "
                + "propriu; țara ≠ RO, codul absent și codul cu litere ies necandidate, fiecare cu motivul lui",
                SincronizareAnafService.CuiInterogabil("RO 123 45") == 12345
                && SincronizareAnafService.CuiInterogabil("  ro12345  ") == 12345
                && SincronizareAnafService.CuiInterogabil(pfaCuCui) == 30123456
                && SincronizareAnafService.CuiInterogabil(pfCnp) == null
                && SincronizareAnafService.Interogabilitate(pfCnp).Motiv.Contains("numeric personal")
                && SincronizareAnafService.CuiInterogabil(strain) == null
                && SincronizareAnafService.Interogabilitate(strain).Motiv.Contains("DE")
                && SincronizareAnafService.Interogabilitate(faraCod).Motiv.Contains("fără cod fiscal")
                && SincronizareAnafService.CuiInterogabil(cuLitere) == null
                && SincronizareAnafService.CuiInterogabil("1") == null
                && SincronizareAnafService.CuiInterogabil("12345678901") == null);
        }

        // ---------------- D15-V3: deserializarea răspunsului v9 ----------------
        // Fixture-urile trec prin `PlatitorTvaClient.Interpreteaza`, adică prin
        // EXACT funcția pe care o cheamă `Interogheaza` după ce citește corpul HTTP
        // — proba stă pe calea reală (66h), doar sursa octeților diferă.
        {
            var dosar = Path.Combine(AppContext.BaseDirectory, "Fixtures");
            string Fixture(string nume) => File.ReadAllText(Path.Combine(dosar, nume));

            // (a) Răspuns REAL v9, salvat la implementare (2026-08-25): un CUI găsit
            // (14399840, plătitor de TVA, cu ambele adrese) și unul inexistent.
            // Constatarea care contează: răspunsul real NU are `cod`/`message`.
            var (gasitiReal, negasitiReal, stricateReal) = PlatitorTvaClient.Interpreteaza(Fixture("anaf-real-v9.json"));
            var real = gasitiReal.FirstOrDefault();
            Console.WriteLine($"     MĂSURAT (D15-V3/real): {gasitiReal.Count} găsiți, {negasitiReal.Count} negăsiți, "
                + $"{stricateReal} stricate; „{real?.Denumire}” CUI {real?.Cui}, scpTVA {real?.ScpTva}, "
                + $"domiciliu „{real?.DomiciliuFiscal?.Strada}” {real?.DomiciliuFiscal?.CodJudetAuto}, "
                + $"sediu cod poștal „{real?.SediuSocial?.CodPostal}”.");
            s.Check("D15-V3 răspunsul REAL v9 (fixture salvat din `webservicesp.anaf.ro`, CUI 14399840 + unul "
                + "inexistent) se deserializează integral, DEȘI n-are `cod`/`message` — contractul le dă "
                + "opționale, iar realitatea nu le trimite deloc",
                gasitiReal.Count == 1 && negasitiReal.Count == 1 && negasitiReal[0] == 99999999 && stricateReal == 0
                && real is { Cui: 14399840, Denumire: "DANTE INTERNATIONAL SA", NrRegCom: "J2002000372404" }
                && real.ScpTva == true && real.TvaLaIncasare == false && real.Inactiv == false
                && real.DomiciliuFiscal is { Localitate: "Sector 2 Mun. Bucureşti", CodJudetAuto: "B", Numar: "6" }
                && real.DomiciliuFiscal.Detalii == "Cladirea Globalworth Square"
                && real.SediuSocial is { CodPostal: "60787", CodJudetAuto: "B" });

            // Cusătura V3 → V2: datele REALE trec prin merge-ul real.
            {
                var p = new Partener { Cod = "V3", Tara = "RO", CodFiscal = "RO14399840" };
                var r = SincronizareAnafService.Aplica(p, real, suprascrie: false, timbru, CautaJudet);
                Console.WriteLine($"     MĂSURAT (D15-V3/cusătură): „{p.Denumire}”, „{p.Strada} {p.Numar}”, "
                    + $"{p.Localitate}, județ {p.Judet?.Cod}, TVA {p.InregistratTva}; "
                    + $"modificări {r.Modificari.Count}, avertismente {r.Avertismente.Count}.");
                s.Check("D15-V3 cusătura cu D15-V2: datele REALE ale ANAF trec prin `Aplica` pe un partener gol — "
                    + "domiciliul fiscal devine adresa, indicativul „B” devine RO-B prin `JudeteRo.DupaCodAuto`, "
                    + "iar `scpTVA` adevărat bifează `InregistratTva`; zero avertismente pe un răspuns complet",
                    p.Denumire == "DANTE INTERNATIONAL SA" && p.RegistruComert == "J2002000372404"
                    && p.Strada == "Str. Gara Herăstrău" && p.Numar == "6"
                    && p.Localitate == "Sector 2 Mun. Bucureşti" && p.Judet?.Cod == "RO-B"
                    && p.InregistratTva && !p.TvaLaIncasare && !p.InactivFiscal
                    && p.DataSincronizareAnaf == timbru
                    && r.Avertismente.Count == 0 && r.Diferente.Count == 0);
            }

            // (b) Răspuns fără `cod`/`message` ȘI fără blocurile de statut/domiciliu:
            // tot ce lipsește iese ca `null`, nu ca „fals".
            var (gasitiFara, negasitiFara, stricateFara) = PlatitorTvaClient.Interpreteaza(Fixture("anaf-fara-cod.json"));
            var fara = gasitiFara.FirstOrDefault();
            Console.WriteLine($"     MĂSURAT (D15-V3/fără cod): {gasitiFara.Count} găsiți, {negasitiFara.Count} negăsiți; "
                + $"scpTVA {fara?.ScpTva?.ToString() ?? "null"}, domiciliu "
                + $"{(fara?.DomiciliuFiscal == null ? "null" : "prezent")}, sediu „{fara?.SediuSocial?.Localitate}”.");
            s.Check("D15-V3 răspuns fără `cod`/`message` și cu blocuri ÎNTREGI absente (`inregistrare_scop_Tva`, "
                + "`adresa_domiciliu_fiscal`): parsează, iar absența iese ca `null` — nu ca `false`, care ar fi "
                + "„ANAF a spus că nu e plătitor de TVA”",
                gasitiFara.Count == 1 && negasitiFara.Count == 0 && stricateFara == 0
                && fara is { Cui: 4221306, Denumire: "MINISTERUL FINANTELOR" }
                && fara.ScpTva == null && fara.TvaLaIncasare == null && fara.Inactiv == null
                && fara.DomiciliuFiscal == null
                && fara.SediuSocial is { Localitate: "Sector 5 Mun. Bucureşti", CodJudetAuto: "B" });

            // (c) Răspuns cu `cod`/`message` PREZENTE și cu câmpuri necunoscute la
            // fiecare nivel (inclusiv obiecte imbricate și `null`-uri).
            var (gasitiNec, negasitiNec, stricateNec) = PlatitorTvaClient.Interpreteaza(Fixture("anaf-necunoscute.json"));
            var nec = gasitiNec.FirstOrDefault();
            Console.WriteLine($"     MĂSURAT (D15-V3/necunoscute): {gasitiNec.Count} găsiți, {negasitiNec.Count} negăsiți; "
                + $"„{nec?.Denumire}”, scpTVA {nec?.ScpTva}, TVA la încasare {nec?.TvaLaIncasare}, "
                + $"inactiv {nec?.Inactiv}, județ „{nec?.DomiciliuFiscal?.CodJudetAuto}”.");
            s.Check("D15-V3 câmpurile NECUNOSCUTE (la nivel de răspuns, de intrare, de bloc și în obiecte imbricate) "
                + "se ignoră, iar `cod`/`message` PREZENTE nu deranjează: ANAF adaugă câmpuri fără să schimbe "
                + "versiunea, iar un client care ar pica pe ele ar fi o bombă cu ceas",
                gasitiNec.Count == 1 && negasitiNec.Count == 1 && negasitiNec[0] == 999999999 && stricateNec == 0
                && nec is { Cui: 12345, Denumire: "PROBĂ CÂMPURI NECUNOSCUTE SRL", NrRegCom: "J12/999/2003" }
                && nec.ScpTva == true && nec.TvaLaIncasare == true && nec.Inactiv == true
                && nec.DomiciliuFiscal is { CodJudetAuto: "CJ", Localitate: "Cluj-Napoca", Detalii: "Ap. 3" });

            // (d) Intrare fără `date_generale` (deci fără CUI): se NUMĂRĂ, nu se
            // ghicește — nu poate fi mapată pe niciun partener.
            var (gasitiStricat, _, stricate) = PlatitorTvaClient.Interpreteaza(
                """{"found":[{"inregistrare_scop_Tva":{"scpTVA":true}},{"date_generale":{"cui":7,"denumire":"OK"}}],"notFound":[]}""");
            Console.WriteLine($"     MĂSURAT (D15-V3/stricat): {gasitiStricat.Count} utilizabile, {stricate} fără `date_generale`.");
            s.Check("D15-V3 o intrare din `found` fără `date_generale` (fără CUI) nu se poate lega de niciun partener: "
                + "se NUMĂRĂ ca intrare stricată (iese ca eroare de lot, fatală) și restul lotului trece",
                gasitiStricat.Count == 1 && gasitiStricat[0].Cui == 7 && stricate == 1);
        }
    }
}
