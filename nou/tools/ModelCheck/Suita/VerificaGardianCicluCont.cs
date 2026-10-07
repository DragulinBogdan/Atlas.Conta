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

// ============ F13-D4: gardianul de ciclu pe `Cont.Parinte` (restanța 67e) ============
// Ca la 57f: `GardianEditare` trăiește pe familia SECURED a host-urilor și NU e
// înregistrat aici (ModelCheck e un provider standalone, fără `XafApplication`),
// deci proba echivalentă e să chemi exact funcția pe care host-ul o leagă de
// `Committing` (`GardianEditare.Verifica`) DIN `Committing`-ul acestui
// ObjectSpace — același obiect, aceeași fază.
//
// Nomenclator pur ⇒ nicio dependență de profil: rulează identic pe amândouă.
// Conturile scenei poartă marcajul `E2E-CIC` și se purjează FIZIC (F13-D2) la
// început (rulare eșuată anterior) și la sfârșit.
static class VerificaGardianCicluCont {
    public static void Ruleaza(Suita s) {
        const string MarcajCic = "E2E-CIC";
        using var os = s.Provider.CreateObjectSpace();

        void CurataCic() {
            // Părinții și copiii intră în ACELAȘI pas: FK-ul `ParinteId` e `Restrict`
            // (104g), iar `Purja` îl rezolvă prin reluarea în pase.
            new Purja(os).Adauga(os.GetObjectsQuery<Cont>()
                .Where(c => c.Simbol.StartsWith(MarcajCic)))
                .Adauga(os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nameof(Cont) && r.Cheie.Contains(MarcajCic)))
                .Executa();
        }
        CurataCic();

        Cont ContCic(IObjectSpace o, string sufix, Cont parinte = null) {
            var c = o.CreateObject<Cont>();
            c.Simbol = MarcajCic + sufix;
            c.Denumire = "Cont probă ciclu " + sufix;
            c.Parinte = parinte;
            return c;
        }

        // --- (1) Lanț legitim de 3 niveluri, cu părinții creați în ACELAȘI commit ---
        // Cele două cerințe ale contractului se suprapun deliberat: pe obiecte NOI
        // FK-ul scalar nu e încă fixat (se completează la SaveChanges), deci dacă
        // gardianul ar fi urmărit lanțul doar pe `ParinteId` n-ar fi văzut nimic aici
        // — nici ciclul, nici lipsa lui. Trecerea probează că sursa primară e
        // navigația, exact ca la liniile de document.
        var cRadacina = ContCic(os, "-1");
        var cMijloc = ContCic(os, "-2", cRadacina);
        var cFrunza = ContCic(os, "-3", cMijloc);
        string refuzLegitim = null;
        void LaCommittingCic(object _, System.ComponentModel.CancelEventArgs __) {
            try { GardianEditare.Verifica(os); }
            catch (OperareException e) { refuzLegitim = e.Message; }
        }
        os.Committing += LaCommittingCic;
        os.CommitChanges();
        os.Committing -= LaCommittingCic;
        s.Check("F13-D4: lanț legitim de 3 niveluri, TOATE conturile create în același commit "
            + "(părintele e el însuși nou, deci vizibil doar prin navigație) — gardianul TACE",
            refuzLegitim == null
            && os.GetObjectsQuery<Cont>().Count(c => c.Simbol.StartsWith(MarcajCic)) == 3
            && cMijloc.ParinteId == cRadacina.ID && cFrunza.ParinteId == cMijloc.ID);

        // Fiecare refuz pe un ObjectSpace PROPRIU, aruncat: comitul nu ajunge
        // niciodată la `SaveChanges` (gardianul aruncă din `Committing`), deci scena
        // rămâne exact cum a lăsat-o pasul (1) — nimic de rollback-uit.
        string RefuzCiclu(Action<IObjectSpace> pregateste) {
            using var osProba = s.Provider.CreateObjectSpace();
            osProba.Committing += (_, __) => GardianEditare.Verifica(osProba);
            pregateste(osProba);
            try {
                osProba.CommitChanges();
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        // --- (2) Ciclu DIRECT: A.Parinte = A, pe un cont deja persistat ---
        var mesajDirect = RefuzCiclu(o => {
            var a = o.GetObjectByKey<Cont>(cRadacina.ID);
            a.Parinte = a;
        });
        s.Check("F13-D4: ciclu DIRECT (A.Parinte = A) → refuz de domeniu, cu lanțul în mesaj",
            mesajDirect != null && mesajDirect.Contains("propriul strămoș")
            && mesajDirect.Contains($"{MarcajCic}-1 → {MarcajCic}-1"));

        // --- (3) Ciclu prin DOI: A → B, iar B → A din scena persistată ---
        var mesajDoi = RefuzCiclu(o => {
            var a = o.GetObjectByKey<Cont>(cRadacina.ID);
            a.Parinte = o.GetObjectByKey<Cont>(cMijloc.ID);
        });
        s.Check("F13-D4: ciclu prin DOI (A → B → A, cu B → A deja în bază) → refuz, iar mesajul poartă "
            + "lanțul ÎNTREG, nu doar contul — gardul spune CE l-a închis",
            mesajDoi != null && mesajDoi.Contains("propriul strămoș")
            && mesajDoi.Contains($"{MarcajCic}-1 → {MarcajCic}-2 → {MarcajCic}-1"));

        // --- (4) Ciclu între două conturi NOI, în același commit ---
        // Cazul pe care numai urmărirea prin navigație îl poate prinde: niciunul
        // dintre cele două n-are încă rând în bază, deci nicio interogare nu i-ar fi
        // văzut legătura.
        var mesajNoi = RefuzCiclu(o => {
            var x = ContCic(o, "-N1");
            var y = ContCic(o, "-N2", x);
            x.Parinte = y;
        });
        s.Check("F13-D4: ciclu între două conturi NOI în ACELAȘI commit → refuz (lanțul se urmărește prin "
            + "navigație, nu prin FK: rândurile nu există încă în bază)",
            mesajNoi != null && mesajNoi.Contains("propriul strămoș")
            && mesajNoi.Contains($"{MarcajCic}-N1") && mesajNoi.Contains($"{MarcajCic}-N2"));

        // --- (5) Contul ȘTERS nu se mai judecă ---
        // 55a/57f: la `Committing` ștergerea e `EntityState.Deleted`. Un cont șters
        // care era prins într-un lanț nu trebuie să blocheze commitul — ștergerea e
        // chiar ieșirea din lanț.
        var refuzStergere = RefuzCiclu(o => o.Delete(o.GetObjectByKey<Cont>(cFrunza.ID)));
        s.Check("F13-D4: ștergerea unui cont din lanț NU trece prin gardul de ciclu",
            refuzStergere == null);
        s.Check("F13-D4: contul șters a dispărut fizic (104f)",
            !os.GetObjectsQuery<Cont>().Any(c => c.ID == cFrunza.ID));

        Console.WriteLine("     MĂSURAT (F13-D4) — mesajele de domeniu, exact cum le vede operatorul:\n"
            + $"       direct:      {mesajDirect?.Replace("\n", " | ") ?? "<gardianul a tăcut>"}\n"
            + $"       prin doi:    {mesajDoi?.Replace("\n", " | ") ?? "<gardianul a tăcut>"}\n"
            + $"       două noi:    {mesajNoi?.Replace("\n", " | ") ?? "<gardianul a tăcut>"}");

        CurataCic();
        s.Check("Curățenie finală F13-D4 (fără reziduuri E2E-CIC)",
            !os.GetObjectsQuery<Cont>().Any(c => c.Simbol.StartsWith(MarcajCic)));
    }
}
