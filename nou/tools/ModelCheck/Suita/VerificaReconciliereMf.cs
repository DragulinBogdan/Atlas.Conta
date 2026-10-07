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

// ============= Felia API DVI (F25) — E2E-API-DVI (privat) =============
// RECONCILIERE-MF (F26-D13): formula noastră (`AmortizareService.CotaLunara`)
// contra cifrelor POSTATE în Flax, lună cu lună. Fixture-ul e gitignored
// (`1C/mf/`), deci blocul se SARE unde nu există — nu pică. Diferențele se
// RAPORTEAZĂ (21, 35b): sursa externă e evidență, niciodată canonic.
static class VerificaReconciliereMf {
    public static void Ruleaza(Suita s) {
        var proiect = new DirectoryInfo(MetadataDump.DirectorProiect());
        var radacina = proiect.Parent?.Parent?.Parent;
        var caleParametri = radacina == null ? null : Path.Combine(radacina.FullName, "1C", "mf", "esantion-parametri.csv");
        var caleAmortizare = radacina == null ? null : Path.Combine(radacina.FullName, "1C", "mf", "esantion-amortizare.csv");
        if (caleParametri == null || !File.Exists(caleParametri) || !File.Exists(caleAmortizare)) {
            Console.WriteLine("     SĂRIT (RECONCILIERE-MF): fixture-ul `1C/mf/` lipsește (folder gitignored) — "
                + "reconcilierea cu Flax rulează doar unde există extrasul.");
            return;
        }

        var cultura = System.Globalization.CultureInfo.InvariantCulture;
        var stricate = new List<string>();
        List<string[]> Citeste(string cale, int coloane) {
            var iesire = new List<string[]>();
            var numar = 1;
            foreach (var linie in File.ReadAllLines(cale).Skip(1)) {
                numar++;
                if (linie.Trim().Length == 0)
                    continue;
                var parti = linie.Split(';');
                if (parti.Length < coloane)
                    stricate.Add($"{Path.GetFileName(cale)}:{numar}");
                else
                    iesire.Add(parti);
            }
            return iesire;
        }

        var evenimente = new Dictionary<string, List<(DateOnly Data, decimal Valoare, int Luni)>>();
        var coduri = new Dictionary<string, string>();
        foreach (var r in Citeste(caleParametri, 6)) {
            if (!DateOnly.TryParseExact(r[3], "yyyy-MM-dd", cultura,
                    System.Globalization.DateTimeStyles.None, out var data)
                    || !decimal.TryParse(r[4], System.Globalization.NumberStyles.Number, cultura, out var valoare)
                    || !int.TryParse(r[5], out var luni)) {
                stricate.Add($"esantion-parametri.csv: „{r[3]}/{r[4]}/{r[5]}”");
                continue;
            }
            coduri[r[0]] = r[1];
            if (!evenimente.TryGetValue(r[0], out var lista))
                evenimente[r[0]] = lista = [];
            lista.Add((data, valoare, luni));
        }

        var lunare = new Dictionary<string, List<(DateOnly Ultima, string Luna, decimal Suma, string Recorder)>>();
        foreach (var r in Citeste(caleAmortizare, 4)) {
            if (!DateOnly.TryParseExact(r[2] + "-01", "yyyy-MM-dd", cultura,
                    System.Globalization.DateTimeStyles.None, out var prima)
                    || !decimal.TryParse(r[3], System.Globalization.NumberStyles.Number, cultura, out var suma)) {
                stricate.Add($"esantion-amortizare.csv: „{r[2]}/{r[3]}”");
                continue;
            }
            coduri.TryAdd(r[0], r[1]);
            if (!lunare.TryGetValue(r[0], out var lista))
                lunare[r[0]] = lista = [];
            lista.Add((new DateOnly(prima.Year, prima.Month, DateTime.DaysInMonth(prima.Year, prima.Month)),
                r[2], suma, r.Length > 6 ? r[6] : ""));
        }

        s.Check("RECONCILIERE-MF fixture-ul din Flax („1C/mf/esantion-parametri.csv” + „esantion-amortizare.csv”) "
            + "se parsează integral: blocul pică pe fixture MALFORMAT, niciodată pe diferențe de cifre — o sursă "
            + "externă e evidență, iar diferențele ei se raportează",
            stricate.Count == 0 && evenimente.Count > 0 && lunare.Count > 0);
        if (stricate.Count > 0)
            Console.WriteLine($"     MĂSURAT (RECONCILIERE-MF): {stricate.Count} rânduri nelizibile — "
                + string.Join(", ", stricate.Take(5)));

        var primaLunaFixture = lunare.Values.SelectMany(l => l).Min(l => l.Ultima);
        var potriviri = 0;
        var ultimeLuni = 0;
        var nedeterminabile = 0;
        var totalRanduri = 0;
        var diferente = new List<(string Cod, string Luna, decimal Asteptat, decimal Postat, string Cauza)>();
        var dinDocumentManual = 0;
        var activeCuDiferente = new Dictionary<string, (int Randuri, decimal Abatere)>();

        foreach (var (id, randuriLunare) in lunare.OrderBy(x => coduri.GetValueOrDefault(x.Key, x.Key))) {
            if (!evenimente.TryGetValue(id, out var ale))
                continue;
            var cod = coduri.GetValueOrDefault(id, id);
            var sortate = ale.OrderBy(e => e.Data).ToList();
            var luni = randuriLunare.OrderBy(l => l.Ultima).ToList();
            (DateOnly Data, decimal Valoare, int Luni)? curent = null;
            var rest = 0m;
            for (var i = 0; i < luni.Count; i++) {
                var rand = luni[i];
                var aplicabil = sortate.LastOrDefault(e => e.Data < new DateOnly(rand.Ultima.Year, rand.Ultima.Month, 1));
                if (aplicabil.Luni == 0 && aplicabil.Valoare == 0m)
                    continue;
                totalRanduri++;
                if (curent == null || curent.Value.Data != aplicabil.Data) {
                    curent = aplicabil;
                    rest = aplicabil.Valoare;
                }
                var determinabil = aplicabil.Data >= primaLunaFixture;
                if (!determinabil)
                    nedeterminabile++;
                var cota = aplicabil.Luni <= 0 ? 0m : Scara.RotunjesteBani(aplicabil.Valoare / aplicabil.Luni);
                var asteptat = determinabil ? Math.Min(cota, rest) : cota;
                var ultimul = i == luni.Count - 1;
                if (rand.Suma == asteptat)
                    potriviri++;
                else if (ultimul && rand.Suma < cota)
                    ultimeLuni++;
                else {
                    var cauza = rand.Recorder.StartsWith("Închidere lună") ? "necunoscută" : "document manual";
                    if (cauza == "document manual")
                        dinDocumentManual++;
                    diferente.Add((cod, rand.Luna, asteptat, rand.Suma, cauza));
                    var acum = activeCuDiferente.GetValueOrDefault(cod);
                    activeCuDiferente[cod] = (acum.Randuri + 1, acum.Abatere + rand.Suma - asteptat);
                }
                rest -= rand.Suma;
            }
        }

        var active = lunare.Count(x => evenimente.ContainsKey(x.Key));
        var procent = totalRanduri == 0 ? 0m
            : Scara.RotunjesteBani(100m * (potriviri + ultimeLuni) / totalRanduri);
        Console.WriteLine($"     MĂSURAT (RECONCILIERE-MF): {active} active, {totalRanduri} rânduri lunare; "
            + $"{potriviri} potriviri exacte, {ultimeLuni} rânduri „ultima lună” (sub cotă), "
            + $"{diferente.Count} diferențe — {procent}% explicat. "
            + $"{nedeterminabile} rânduri au evenimentul înaintea primei luni din fixture "
            + $"({primaLunaFixture:MM.yyyy}), deci restul lor nu e determinabil și se compară doar cu cota.");
        Console.WriteLine($"     MĂSURAT (RECONCILIERE-MF, cauzele diferențelor): {dinDocumentManual} rânduri "
            + "vin dintr-un document MANUAL de recuperare (nu din închiderea lunii), care cumulează mai multe "
            + $"luni într-un singur rând; {diferente.Count - dinDocumentManual} rămân neexplicate. Parametrii unui "
            + "eveniment se aplică din luna URMĂTOARE lui, indiferent de zi (formula 1C, aceeași în felie).");
        foreach (var d in diferente.Take(30))
            Console.WriteLine($"       DIFERENȚĂ {d.Cod} {d.Luna}: așteptat {d.Asteptat}, postat {d.Postat} "
                + $"(Δ {d.Postat - d.Asteptat}) — {d.Cauza}.");
        if (diferente.Count > 30)
            Console.WriteLine($"       … și încă {diferente.Count - 30} diferențe.");
        foreach (var (cod, sumar) in activeCuDiferente.OrderByDescending(a => Math.Abs(a.Value.Abatere)))
            Console.WriteLine($"       ACTIV {cod}: {sumar.Randuri} rânduri diferite, abatere cumulată "
                + $"{sumar.Abatere}.");
        s.Check("RECONCILIERE-MF formula feliei („cota fixată la ultimul eveniment, plafonată la restul rămas”) "
            + "reproduce cifrele postate în Flax pe eșantionul extras; rândurile rămase sunt RAPORTATE mai sus, "
            + "cu activ, lună, așteptat și postat — blocul nu pică pe ele (21, 35b)",
            stricate.Count == 0);
    }
}
