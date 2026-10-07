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
// F23-V5 — RAPORTUL de verificare a profilului (F23-D8)
// ---------------------------------------------------------------------------
// Seed-ul ARUNCĂ, raportul ARATĂ. Proba are trei timpi: baza curată n-are ce
// raporta; o scenă produce câte o constatare din fiecare categorie pe care felia
// o poate PRODUCE fără host (`RandManual` / `TipTvaInactivReferit`); după
// curățenie raportul tace din nou — altfel ar fi un
// jurnal care se acumulează, nu o funcție de starea bazei.
static class VerificaF23Raport {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-F23";
        var eticheta = privat ? "privat" : "bugetar";
        var cronometru = System.Diagnostics.Stopwatch.StartNew();
        IReadOnlyList<ConstatareProfil> initiale;
        using (var os = s.Provider.CreateObjectSpace())
            initiale = VerificareProfilService.Raporteaza(os);
        cronometru.Stop();
        Console.WriteLine($"     MĂSURAT (F23-V5/{eticheta}/bază curată): {initiale.Count} constatări în "
            + $"{cronometru.ElapsedMilliseconds} ms"
            + (initiale.Count == 0 ? "." : " — " + string.Join("; ", initiale.Take(10)
                .Select(c => $"{c.Tabel}/{c.Cheie}/{c.Fel}")) + "."));
        s.Check($"F23-V5 ({eticheta}) pe o bază seed-uită și neatinsă raportul e GOL: fiecare rând de politică "
            + "poartă timbrul seed-ului, niciun implicit nu țintește "
            + "un tip inactiv, fiecare tip cu politică de TVA are ancoră și nicio mapare nu lipsește",
            initiale.Count == 0);

        // ---- Scena ----
        // (a) trece prin GARDIAN, fiindcă exact el stinge timbrul; (b) și (c) se scriu
        // pe ușa de SISTEM, cu `DinSeed = true` pus de mână, ca rândurile de scenă să
        // NU producă ele însele `RandManual` — categoria (a) rămâne a singurului rând
        // care chiar a fost editat.
        Guid idScadenta = Guid.Empty, idTvaProba, idImplicitProba;
        var zileInainte = 0;
        string cheieScadenta = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            var scadenta = os.GetObjectsQuery<PoliticaScadenta>().ToList().FirstOrDefault(p => p.DinSeed);
            if (scadenta == null)
                Console.WriteLine($"     MĂSURAT (F23-V5/{eticheta}): profilul n-are `PoliticaScadenta` "
                    + "seed-uită — categoria `RandManual` rămâne nesondată pe el.");
            else {
                idScadenta = scadenta.ID;
                zileInainte = scadenta.ZileDefault;
                cheieScadenta = scadenta.TipDocument?.Cod;
                scadenta.ZileDefault = zileInainte + 1;
                // Prin GARDIAN, nu prin scriere directă: exact drumul pe care umblă un
                // PATCH de OData, adică drumul care stinge timbrul.
                GardianEditare.Verifica(os);
                os.CommitChanges();
            }
        }
        string codInactiv = null;
        using (var os = s.Provider.CreateObjectSpace()) {
            var inactiv = os.GetObjectsQuery<TipTva>().ToList().FirstOrDefault(t => !t.Activ);
            codInactiv = inactiv?.Cod;
            if (inactiv != null) {
                var partener = os.CreateObject<Partener>();
                partener.Cod = Marcaj + "-RAP";
                partener.Denumire = "Partener probă F23 (implicit inactiv)";
                partener.Tara = "RO";
                partener.TipTvaImplicit = inactiv;
            }
            var tvaProba = os.CreateObject<TipTva>();
            tvaProba.Cod = Marcaj + "-TVA";
            tvaProba.Denumire = "Tip TVA probă F23";
            tvaProba.Cota = 21m;
            tvaProba.DinSeed = true;
            idTvaProba = tvaProba.ID;
            var implicitProba = os.CreateObject<PoliticaTvaImplicit>();
            implicitProba.TipDocument = os.GetObjectsQuery<TipDocument>().ToList().First(t => t.Cod == "BTR");
            implicitProba.ClasaFiscala = ClasaFiscalaPartener.ExtraUe;
            implicitProba.TipTva = tvaProba;
            implicitProba.DinSeed = true;
            idImplicitProba = implicitProba.ID;
            os.CommitChanges();
        }
        // 104g: tipul referit nu se șterge — FK `Restrict`, refuz de domeniu tradus (39a), pe ambele
        // căi EF: dependentul neîncărcat (refuzul vine din bază) și dependentul urmărit în același OS.
        Atlas.Conta.BackOffice.Module.BusinessObjects.MesajeConstraintRo.Aplica();
        string RefuzFk(bool cuDependentUrmarit) {
            using var os = s.Provider.CreateObjectSpace();
            if (cuDependentUrmarit)
                _ = os.GetObjectByKey<PoliticaTvaImplicit>(idImplicitProba);
            try {
                os.Delete(os.GetObjectByKey<TipTva>(idTvaProba));
                os.CommitChanges();
                return null;
            }
            catch (Exception e) {
                var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(e);
                return violare == null
                    ? $"<netradus: {e.GetType().Name}: {e.Message}>"
                    : Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare);
            }
        }
        var refuzFkBaza = RefuzFk(false);
        var refuzFkUrmarit = RefuzFk(true);
        bool EsteRefuzFk(string m) => m != null && m.StartsWith("Nu se poate șterge înregistrarea");

        IReadOnlyList<ConstatareProfil> dupaScena;
        using (var os = s.Provider.CreateObjectSpace())
            dupaScena = VerificareProfilService.Raporteaza(os);
        var manuale = dupaScena.Where(c => c.Fel == FelConstatare.RandManual).ToList();
        var inactiveReferite = dupaScena.Where(c => c.Fel == FelConstatare.TipTvaInactivReferit).ToList();
        Console.WriteLine($"     MĂSURAT (F23-V5/{eticheta}/scenă): {dupaScena.Count} constatări — "
            + $"{manuale.Count} RandManual, {inactiveReferite.Count} TipTvaInactivReferit; "
            + string.Join("; ", dupaScena.Select(c => $"{c.Tabel}/{c.Cheie}/{c.Fel}"))
            + $"; ștergerea tipului referit → bază: „{refuzFkBaza ?? "<ACCEPTATĂ>"}”, urmărit: „{refuzFkUrmarit ?? "<ACCEPTATĂ>"}”.");
        s.Check($"F23-V5 ({eticheta}) categoriile pe care felia le poate PRODUCE fără host apar exact o "
            + "dată fiecare, cu tabelul și cheia LIZIBILE: (a) politica de scadență editată PRIN GARDIAN e "
            + $"`RandManual` pe „{cheieScadenta ?? "(nesondat)"}” (timbrul se stinge la scriere); (b) partenerul "
            + $"cu implicit „{codInactiv ?? "(niciun tip stins)"}” e `TipTvaInactivReferit`; (c, 104g) tipul de TVA "
            + "referit de un implicit NU se poate șterge: FK `Restrict`, refuz de domeniu tradus pe ambele căi EF",
            (cheieScadenta == null
                ? manuale.Count == 0
                : manuale.Count == 1 && manuale[0].Tabel == "Politici de scadență"
                    && manuale[0].Cheie == cheieScadenta)
            && (codInactiv == null
                ? inactiveReferite.Count == 0
                : inactiveReferite.Count == 1 && inactiveReferite[0].Tabel == "Parteneri"
                    && inactiveReferite[0].Cheie == Marcaj + "-RAP")
            && EsteRefuzFk(refuzFkBaza) && EsteRefuzFk(refuzFkUrmarit));

        // ---- Curățenia: purjă FIZICĂ + restaurarea timbrului ----
        using (var os = s.Provider.CreateObjectSpace()) {
            var pj = new Purja(os);
            pj.Adauga(os.GetObjectsQuery<PoliticaTvaImplicit>().Where(p => p.ID == idImplicitProba));
            pj.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.ID == idTvaProba));
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)));
            pj.Executa();
        }
        if (idScadenta != Guid.Empty)
            using (var os = s.Provider.CreateObjectSpace()) {
                var scadenta = os.GetObjectByKey<PoliticaScadenta>(idScadenta);
                scadenta.ZileDefault = zileInainte;
                // Reaprinderea timbrului pe ușa de SISTEM (fără gardian) — exact felul
                // în care seed-ul l-ar fi pus, dacă rândul ar fi lipsit.
                scadenta.DinSeed = true;
                os.CommitChanges();
            }
        IReadOnlyList<ConstatareProfil> finale;
        using (var os = s.Provider.CreateObjectSpace())
            finale = VerificareProfilService.Raporteaza(os);
        Console.WriteLine($"     MĂSURAT (F23-V5/{eticheta}/după curățenie): {finale.Count} constatări"
            + (finale.Count == 0 ? "." : " — " + string.Join("; ", finale
                .Select(c => $"{c.Tabel}/{c.Cheie}/{c.Fel}")) + "."));
        s.Check($"F23-V5 ({eticheta}) raportul e o FUNCȚIE de starea bazei, nu un jurnal care se acumulează: după "
            + "purja fizică a scenei și restaurarea timbrului, numărul de constatări revine la cel dinaintea "
            + "probei",
            finale.Count == initiale.Count);
    }
}
