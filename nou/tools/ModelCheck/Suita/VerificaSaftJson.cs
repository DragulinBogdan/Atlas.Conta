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

// ============ Felia 16, pas 4b: serializarea JSON — D16-V2 (d) ============
// De ce e o PROBĂ și nu un detaliu de host: coliziunea `PartenerID`/`PartenerId`
// de pe `SaftFactura` a ieșit **500 pe calea reală** (V4) fiindcă
// `System.Text.Json` refuză două proprietăți ale căror nume diferă doar prin caz
// — iar suita, care probase fiecare cifră a declarației la cent, nu serializase
// niciodată DTO-ul. O structură care se calculează corect și nu se poate trimite
// nu e o proiecție terminată.
//
// Opțiunile sunt ALE HOST-ULUI, nu unele „echivalente": `JsonApi.Optiuni` e
// aceeași configurare pe care `Startup` o dă lui `Mvc.JsonOptions`
// (`JsonSerializerDefaults.Web` + `PropertyNamingPolicy = null`). Cu altă
// politică de nume proba ar fi trecut și 500-ul ar fi rămas neatins: exact acele
// coliziuni DEPIND de politică.
//
// Se probează AMBELE forme — declarația întreagă (ce scrie fișierul, ce serveau
// ușile până la pasul 4b) și SUMARUL (ce pleacă azi pe sârmă) — plus întoarcerea:
// un JSON care se scrie dar nu se citește înapoi cu aceleași contoare n-ar fi
// contract, ar fi text.
static class VerificaSaftJson {
    public static void Ruleaza(Suita s, SaftDto dto, string eticheta) {
        var sumar = SaftProiectii.Sumar(dto);
        string jsonIntreg = null, jsonSumar = null, eroare = null;
        try {
            jsonIntreg = JsonSerializer.Serialize(dto, JsonApi.Optiuni);
            jsonSumar = JsonSerializer.Serialize(sumar, JsonApi.Optiuni);
        }
        catch (Exception e) {
            eroare = $"{e.GetType().Name}: {e.Message}";
        }
        double KiB(string s) => s == null ? 0 : Encoding.UTF8.GetByteCount(s) / 1024.0;
        Console.WriteLine($"     MĂSURAT (D16-V2 JSON, {eticheta}): întreg {KiB(jsonIntreg):N1} KiB / sumar "
            + $"{KiB(jsonSumar):N1} KiB{(eroare == null ? "" : $" — EROARE {eroare}")}.");
        s.Check($"D16-V2 (d, {eticheta}) `SaftDto` ȘI `SaftSumarDto` se SERIALIZEAZĂ cu opțiunile host-ului "
            + "(`JsonApi.Optiuni`: Web + nume CLR exacte) — nicio coliziune de nume, niciun ciclu, niciun tip "
            + "pe care `System.Text.Json` să nu-l știe scrie",
            eroare == null && jsonIntreg != null && jsonSumar != null);
        if (eroare != null)
            return;

        // Întoarcerea: aceleași contoare după deserializare. Probează implicit și
        // numele de pe sârmă (`Rezumat`, nu `rezumat`) — cu politica greșită,
        // proprietățile s-ar pierde tăcut în obiectul întors.
        var dtoInapoi = JsonSerializer.Deserialize<SaftDto>(jsonIntreg, JsonApi.Optiuni);
        var sumarInapoi = JsonSerializer.Deserialize<SaftSumarDto>(jsonSumar, JsonApi.Optiuni);
        s.Check($"D16-V2 (d, {eticheta}) round-trip: DTO-ul citit înapoi are ACELEAȘI contoare pe fiecare secțiune "
            + "(conturi, terți, jurnale/tranzacții/linii GL, facturi per sens, plăți, produse, `Neincluse`, "
            + "avertismente) și același `Rezumat` pe cusăturile de bani",
            dtoInapoi != null
            && dtoInapoi.Conturi.Count == dto.Conturi.Count
            && dtoInapoi.Clienti.Count == dto.Clienti.Count && dtoInapoi.Furnizori.Count == dto.Furnizori.Count
            && dtoInapoi.Jurnale.Count == dto.Jurnale.Count
            && dtoInapoi.Jurnale.Sum(j => j.Tranzactii.Count) == dto.Jurnale.Sum(j => j.Tranzactii.Count)
            && dtoInapoi.Jurnale.Sum(j => j.Tranzactii.Sum(t => t.Linii.Count))
                == dto.Jurnale.Sum(j => j.Tranzactii.Sum(t => t.Linii.Count))
            && dtoInapoi.FacturiEmise.Count == dto.FacturiEmise.Count
            && dtoInapoi.FacturiPrimite.Count == dto.FacturiPrimite.Count
            && dtoInapoi.Plati.Count == dto.Plati.Count && dtoInapoi.Produse.Count == dto.Produse.Count
            && dtoInapoi.Taxe.Count == dto.Taxe.Count && dtoInapoi.Unitati.Count == dto.Unitati.Count
            && dtoInapoi.TipuriAnaliza.Count == dto.TipuriAnaliza.Count
            && dtoInapoi.Neincluse.Count == dto.Neincluse.Count
            && dtoInapoi.Avertismente.Count == dto.Avertismente.Count
            && dtoInapoi.Neaplicabil == dto.Neaplicabil && dtoInapoi.An == dto.An && dtoInapoi.Luna == dto.Luna
            && dtoInapoi.DataStart == dto.DataStart && dtoInapoi.DataEnd == dto.DataEnd
            && dtoInapoi.Rezumat.TotalDebit == dto.Rezumat.TotalDebit
            && dtoInapoi.Rezumat.TotalCredit == dto.Rezumat.TotalCredit
            && dtoInapoi.Rezumat.TvaRegistru == dto.Rezumat.TvaRegistru
            && dtoInapoi.Rezumat.ClosingGla == dto.Rezumat.ClosingGla
            && (dto.Header == null) == (dtoInapoi.Header == null)
            && (dto.Header == null || dtoInapoi.Header.RegistrationNumber == dto.Header.RegistrationNumber));

        // `Sumar` e funcție PURĂ pe DTO ⇒ contoarele lui TREBUIE să fie lungimile
        // listelor pe care nu le mai trimite; acolo unde `Rezumat` are aceeași cifră
        // (`NumarClienti`, `Tranzactii`, `LiniiGl`…), egalitatea e o CUSĂTURĂ, nu o
        // presupunere — clientul citește o singură sursă și n-are voie să numere (42c).
        s.Check($"D16-V2 (d, {eticheta}) `SaftProiectii.Sumar` = numărătoarea serverului: fiecare contor al sumarului "
            + "e lungimea listei corespunzătoare din declarație, iar unde `Rezumat` are deja cifra ea COINCIDE; "
            + "`Rezumat`/avertismentele trec neschimbate, antetul întreg (`Neincluse` pleacă AGREGAT — F20-D5, "
            + "cusătura de mai jos)",
            sumar.Conturi == dto.Conturi.Count && sumar.Clienti == dto.Clienti.Count
            && sumar.Furnizori == dto.Furnizori.Count && sumar.CoduriTaxa == dto.Taxe.Count
            && sumar.Unitati == dto.Unitati.Count && sumar.Produse == dto.Produse.Count
            && sumar.TipuriAnaliza == dto.TipuriAnaliza.Count && sumar.Jurnale == dto.Jurnale.Count
            && sumar.FacturiEmise == dto.FacturiEmise.Count && sumar.FacturiPrimite == dto.FacturiPrimite.Count
            && sumar.Plati == dto.Plati.Count
            && sumar.Tranzactii == dto.Rezumat.Tranzactii && sumar.LiniiGl == dto.Rezumat.LiniiGl
            && sumar.Clienti == dto.Rezumat.NumarClienti && sumar.Furnizori == dto.Rezumat.NumarFurnizori
            && sumar.FacturiEmise == dto.Rezumat.NumarFacturiEmise
            && sumar.FacturiPrimite == dto.Rezumat.NumarFacturiPrimite
            && sumar.Plati == dto.Rezumat.NumarPlati && sumar.Produse == dto.Rezumat.NumarProduse
            && ReferenceEquals(sumar.Rezumat, dto.Rezumat)
            && ReferenceEquals(sumar.Avertismente, dto.Avertismente)
            && ReferenceEquals(sumar.Header, dto.Header)
            && sumar.Neaplicabil == dto.Neaplicabil && sumar.An == dto.An && sumar.Luna == dto.Luna
            && sumar.DataStart == dto.DataStart && sumar.DataEnd == dto.DataEnd
            && sumarInapoi != null && sumarInapoi.LiniiGl == sumar.LiniiGl
            // Motivul pentru care sumarul există: e mai MIC decât declarația — cerut
            // doar acolo unde declarația chiar are liste (pe una `Neaplicabil`, ambele
            // sunt goale, iar cele 13 contoare o fac cu câțiva octeți mai lungă;
            // MĂSURAT: 1,1 KiB vs 1,1 KiB). Pe scenă 54,8 → 6,0 KiB; pe o lună reală
            // 38,6 MiB → zeci de KiB.
            && (dto.Neaplicabil != null
                || Encoding.UTF8.GetByteCount(jsonSumar) < Encoding.UTF8.GetByteCount(jsonIntreg)));

        // ---------------- F20-D5: `Neincluse` pleacă AGREGAT per cauză ----------------
        // Singura listă a sumarului nemărginită de nimic (mii de rânduri pe o lună
        // reală) — clientul o tăia la 200 fără să spună (73-r10 / 74-r7). Cusătura:
        // agregatul nu are voie să piardă NIMIC din lista plată, care rămâne în
        // `SaftDto` (fișierul). Oracolul cifrei e scris aici din nou, independent de
        // `AgregaNeincluse`: aceeași definiție, altă implementare — dacă agregarea
        // ratează o familie de cifre, cele două nu mai cad la fel.
        static decimal CifraOracol(SaftNeinclus n) =>
            n.Valoare != null ? n.Valoare.Value
            : n.Baza != null ? Math.Abs(n.Baza.Value)
            : Math.Abs(n.Debit ?? 0m) + Math.Abs(n.Credit ?? 0m);
        var cauzePlat = dto.Neincluse.Select(n => n.Cauza ?? "").Distinct(StringComparer.Ordinal)
            .OrderBy(c => c, StringComparer.Ordinal).ToList();
        // Exemplele = PRIMELE ≤ 20 ale grupului, în ordinea listei plate — comparate
        // pe REFERINȚĂ (agregatul nu clonează rândurile, le arată).
        var exempleSuntPrimele = sumar.Neincluse.All(a =>
            a.Exemple.Count == Math.Min(a.Numar, NeinclusAgregat.MaximExemple)
            && a.Exemple.SequenceEqual(dto.Neincluse
                .Where(n => (n.Cauza ?? "") == a.Cauza).Take(NeinclusAgregat.MaximExemple)));
        var ordineAgregate = sumar.Neincluse.SequenceEqual(sumar.Neincluse
            .OrderByDescending(a => a.Numar).ThenBy(a => a.Cauza, StringComparer.Ordinal));
        Console.WriteLine($"     MĂSURAT (F20-D5 Neincluse, {eticheta}): {dto.Neincluse.Count} rânduri plate "
            + $"({dto.Neincluse.Sum(n => n.Randuri)} de registru) ⇒ {sumar.Neincluse.Count} cauze ["
            + string.Join(", ", sumar.Neincluse.Select(a => $"{a.Cauza}×{a.Numar}/{a.Randuri}r/Σ{a.Suma:N2}"
                + (a.Cantitate is decimal cant ? $"/{cant:0.###}" : "")))
            + $"]; {sumar.Neincluse.Sum(a => a.Exemple.Count)} exemple.");
        s.Check($"F20-D5 ({eticheta}) `SaftSumarDto.Neincluse` = lista plată AGREGATĂ per cauză, FĂRĂ pierdere: "
            + "Σ `Numar` == `SaftDto.Neincluse.Count`, Σ `Randuri` == Σ pe lista plată, Σ `Suma` == Σ cifrei "
            + "fiecărui rând (S: `Valoare` SEMNATĂ; L: |`Baza`|, iar pe solduri de terți |`Debit`|+|`Credit`|), "
            + "Σ `Cantitate` idem; exact aceleași CAUZE; `Cantitate` e nenulă exact pe cauzele care au rânduri "
            + "cu cantitate (convenția `SaftAvertisment.Suma`); exemplele = PRIMELE ≤ 20 în ordinea listei; "
            + "agregatele ordonate `Numar` desc + `Cauza` ordinal; round-trip-ul JSON le păstrează",
            sumar.Neincluse.Sum(a => a.Numar) == dto.Neincluse.Count
            && sumar.Neincluse.Sum(a => a.Randuri) == dto.Neincluse.Sum(n => n.Randuri)
            && sumar.Neincluse.Sum(a => a.Suma) == dto.Neincluse.Sum(CifraOracol)
            && sumar.Neincluse.Sum(a => a.Cantitate ?? 0m) == dto.Neincluse.Sum(n => n.Cantitate ?? 0m)
            && sumar.Neincluse.Select(a => a.Cauza).OrderBy(c => c, StringComparer.Ordinal)
                .SequenceEqual(cauzePlat, StringComparer.Ordinal)
            && sumar.Neincluse.All(a => (a.Cantitate != null)
                == dto.Neincluse.Any(n => (n.Cauza ?? "") == a.Cauza && n.Cantitate != null))
            && exempleSuntPrimele && ordineAgregate
            && sumarInapoi != null && sumarInapoi.Neincluse.Count == sumar.Neincluse.Count
            && sumarInapoi.Neincluse.Sum(a => a.Numar) == dto.Neincluse.Count
            && sumarInapoi.Neincluse.Sum(a => a.Suma) == sumar.Neincluse.Sum(a => a.Suma));
    }
}
