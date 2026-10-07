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

// ============ Felia 14 (D394): modelul + politica de așezare — D4-V1 ============
// Pasul 1 al feliei: identitatea fiscală a partenerului (D4-D1), politica
// `MapareD394` `(TipTva × Sens) → tip de operațiune` (D4-D2) și moartea coloanei
// `CategorieD394` (D4-D8). Proiecția (D4-V2…V7) vine în pașii următori.
//
// Local function, apelată din AMBELE căi de profil, ca `VerificaD300Seed`.
static class VerificaD394Seed {
    public static void Ruleaza(Suita s, bool privat) {
        // ---------------- Modelul EF (D4-D1 / D4-D8) ----------------
        using (var osModel = s.Provider.CreateObjectSpace()) {
            var model = ((EFCoreObjectSpace)osModel).DbContext.Model;
            var tipTva = model.FindEntityType(typeof(TipTva));
            var partener = model.FindEntityType(typeof(Partener));
            s.Check("D4-V1 `CategorieD394` a MURIT: coloana nu mai există în modelul EF al lui `TipTva` "
                + "(tipul de operațiune e direcțional ⇒ politică `MapareD394`, nu atribut pe tip)",
                tipTva != null && tipTva.FindProperty("CategorieD394") == null);
            s.Check("D4-V1 identitatea fiscală a partenerului = 4 câmpuri pe FRUNZA `Partener` (nu pe `Repartitor`): "
                + "TipPersoana, Tara, InregistratTva, TvaLaIncasare",
                partener != null
                && new[] { nameof(Partener.TipPersoana), nameof(Partener.Tara),
                        nameof(Partener.InregistratTva), nameof(Partener.TvaLaIncasare) }
                    .All(n => partener.FindProperty(n) != null)
                && model.FindEntityType(typeof(Repartitor)).FindProperty(nameof(Partener.Tara)) == null);
            var mapare = model.FindEntityType(typeof(MapareD394));
            s.Check("D4-V1 `MapareD394`: o singură mapare per pereche — index UNIC nefiltrat pe (TipTvaId, Sens)",
                mapare != null && mapare.GetIndexes().Any(i => i.IsUnique
                    && i.Properties.Select(p => p.Name).SequenceEqual([nameof(MapareD394.TipTvaId), nameof(MapareD394.Sens)])
                    && i.GetFilter() == null));
        }

        // ---------------- Lista UE (D4-D1) ----------------
        s.Check("D4-V1 `TariUe`: 27 de state membre, cu RO/DE/GR, fără UK/US",
            TariUe.Coduri.Count == 27 && TariUe.Contine("RO") && TariUe.Contine("DE") && TariUe.Contine("GR")
            && !TariUe.Contine("GB") && !TariUe.Contine("US") && !TariUe.Contine(null));

        // ---------------- Normalizarea și gardul pe `Tara` (D4-D1) ----------------
        // În memorie, fără commit: setterul normalizează, `GardianEditare` refuză
        // formatul greșit — pe aceeași ușă pe care o folosește OData (55b).
        using (var osTara = s.Provider.CreateObjectSpace()) {
            var p1 = osTara.CreateObject<Partener>();
            p1.Cod = "E2E-D394-T1"; p1.Denumire = "Probă țară";
            var implicitTara = p1.Tara;
            var implicitTip = p1.TipPersoana;
            p1.Tara = " de ";
            var normalizat = p1.Tara;
            p1.Tara = "";
            var golit = p1.Tara;
            string mesajOk = null;
            try { GardianEditare.Verifica(osTara); } catch (OperareException e) { mesajOk = e.Message; }
            p1.Tara = "ROM";
            string mesajRefuz = null;
            try { GardianEditare.Verifica(osTara); } catch (OperareException e) { mesajRefuz = e.Message; }
            Console.WriteLine($"     MĂSURAT (D4-V1/Tara): implicit „{implicitTara}”/{implicitTip}; „ de ” → „{normalizat}”; "
                + $"„” → „{golit}”; gardian pe „ROM” → „{mesajRefuz ?? "<NU A ARUNCAT>"}”.");
            s.Check("D4-V1 `Partener.Tara`: default RO, `TipPersoana` default Juridica; setterul normalizează "
                + "(trim + majuscule, gol ⇒ RO); `GardianEditare` refuză un cod care nu e ISO-2 („ROM”) "
                + "și tace pe unul valid",
                implicitTara == "RO" && implicitTip == TipPersoana.Juridica
                && normalizat == "DE" && golit == "RO"
                && mesajOk == null && mesajRefuz != null && mesajRefuz.Contains("ROM"));
            osTara.Rollback();
        }

        // ---------------- Politica de așezare (D4-D2) ----------------
        using var os = s.Provider.CreateObjectSpace();
        var mapari = os.GetObjectsQuery<MapareD394>().ToList();
        if (!privat) {
            s.Check("D4-V1 (bugetar) profilul n-are nicio mapare D394 — nu produce rânduri de registru fiscal",
                mapari.Count == 0);
            return;
        }

        TipOperatiuneD394? Tip(string codTip, SensTva sens) => mapari
            .Where(m => m.TipTva.Cod == codTip && m.Sens == sens)
            .Select(m => (TipOperatiuneD394?)m.Tip).SingleOrDefault();

        s.Check("D4-V1 (privat) tabelul D4-D2 e seed-uit integral: 13 mapări, toate cu ținte permise (niciun AÎ/N), o singură mapare per pereche",
            mapari.Count == 13 && mapari.All(m => MapareD394.TintaPermisa(m.Tip, m.Sens))
            && mapari.Select(m => (m.TipTvaId, m.Sens)).Distinct().Count() == 13);
        s.Check("D4-V1 (privat) cotele normale N21/N11/N9/N19: livrare → L, achiziție → A",
            new[] { "N21", "N11", "N9", "N19" }.All(c =>
                Tip(c, SensTva.Livrare) == TipOperatiuneD394.L && Tip(c, SensTva.Achizitie) == TipOperatiuneD394.A));
        s.Check("D4-V1 (privat) taxarea inversă TI21/TI19: livrare → V, achiziție → C",
            new[] { "TI21", "TI19" }.All(c =>
                Tip(c, SensTva.Livrare) == TipOperatiuneD394.V && Tip(c, SensTva.Achizitie) == TipOperatiuneD394.C));
        s.Check("D4-V1 (privat) NED21 doar pe achiziție → A; SDD/SFD/NIM nemapate pe ambele sensuri",
            Tip("NED21", SensTva.Achizitie) == TipOperatiuneD394.A && Tip("NED21", SensTva.Livrare) == null
            && new[] { "SDD", "SFD", "NIM" }.All(c =>
                Tip(c, SensTva.Livrare) == null && Tip(c, SensTva.Achizitie) == null));
        s.Check("D4-V1 (privat) perechile (TipTva × Sens) nemapate sunt EXACT cele din `ContaSeeder.NemapateD394Privat`, toate cu motiv",
            os.GetObjectsQuery<TipTva>().ToList()
                .SelectMany(t => new[] { SensTva.Achizitie, SensTva.Livrare }.Select(s => (t.Cod, Sens: s)))
                .Where(p => Tip(p.Cod, p.Sens) == null)
                .OrderBy(p => p.Cod).ThenBy(p => p.Sens)
                .SequenceEqual(ContaSeeder.NemapateD394Privat.Select(n => (n.TipTva, n.Sens))
                    .OrderBy(p => p.Item1).ThenBy(p => p.Item2))
            && ContaSeeder.NemapateD394Privat.All(n => !string.IsNullOrWhiteSpace(n.Motiv)));

        string RuleazaGardianul() {
            using var osGard = s.Provider.CreateObjectSpace();
            try {
                ContaSeeder.VerificaD394(osGard, ProfilContabil.Privat);
                return null;
            }
            catch (InvalidOperationException e) { return e.Message; }
        }
        int MapariVii() {
            using var osNum = s.Provider.CreateObjectSpace();
            return osNum.GetObjectsQuery<MapareD394>().Count();
        }
        void SqlBrut(FormattableString sql) {
            using var osSql = s.Provider.CreateObjectSpace();
            ((EFCoreObjectSpace)osSql).DbContext.Database.ExecuteSql(sql);
        }
        s.Check("D4-V1 (privat) gardianul de seed `ContaSeeder.VerificaD394` tace pe profilul seed-uit",
            RuleazaGardianul() == null);

        // ══════ Ținta interzisă: AÎ/N nu se mapează (D4-D2) ══════
        // Regula XAF apără culegerea; gardianul de seed apără `--updateDatabase`.
        // Proba trece prin funcția reală, pe o pereche liberă (SDD/Livrare — fără
        // conflict cu indexul unic), și șterge FIZIC în urma ei.
        {
            Guid idProba;
            using (var osScrie = s.Provider.CreateObjectSpace()) {
                var proba = osScrie.CreateObject<MapareD394>();
                proba.TipTva = osScrie.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
                proba.Sens = SensTva.Livrare;
                proba.Tip = TipOperatiuneD394.AI;
                osScrie.CommitChanges();
                idProba = proba.ID;
            }
            var mesajGard = RuleazaGardianul();
            SqlBrut($"DELETE FROM \"MapariD394\" WHERE \"ID\" = {idProba}");
            var mesajDupa = RuleazaGardianul();
            Console.WriteLine($"     MĂSURAT (D4-V1/țintă): gardian pe (SDD, Livrare) → AÎ: „{mesajGard ?? "<NU A ARUNCAT>"}”; "
                + $"după curățare → „{mesajDupa ?? "tace"}”.");
            s.Check("D4-V1 (privat) o mapare cu ținta AÎ e REFUZATĂ de `ContaSeeder.VerificaD394` (AÎ se derivă din "
                + "partener, nu se mapează) și gardul tace după ce proba dispare; `MapareD394.TintaPermisa` refuză și N",
                mesajGard != null && mesajGard.Contains("SDD") && mesajGard.Contains("AI")
                && mesajDupa == null && MapariVii() == 13
                && !MapareD394.TintaPermisa(TipOperatiuneD394.N, SensTva.Achizitie) && !MapareD394.TintaPermisa(TipOperatiuneD394.AI, SensTva.Achizitie)
                && MapareD394.TintaPermisa(TipOperatiuneD394.LS, SensTva.Livrare));
            // Fix 5 al review-ului advers: a doua axă a gardului — tipul trebuie să
            // fie COERENT cu sensul. `(SDD, Achiziție) → L` (pereche liberă, tip de
            // livrare pe achiziție) e refuzată de aceeași funcție reală, ca o mapare
            // incoerentă să nu așeze achiziții în `facturiL` și să rupă tăcut
            // cusătura per sens.
            Guid idProbaSens;
            using (var osScrie = s.Provider.CreateObjectSpace()) {
                var proba = osScrie.CreateObject<MapareD394>();
                proba.TipTva = osScrie.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
                proba.Sens = SensTva.Achizitie;
                proba.Tip = TipOperatiuneD394.L;
                osScrie.CommitChanges();
                idProbaSens = proba.ID;
            }
            var mesajSens = RuleazaGardianul();
            SqlBrut($"DELETE FROM \"MapariD394\" WHERE \"ID\" = {idProbaSens}");
            var mesajSensDupa = RuleazaGardianul();
            Console.WriteLine($"     MĂSURAT (D4-V1/sens): gardian pe (SDD, Achiziție) → L: „{mesajSens ?? "<NU A ARUNCAT>"}”; "
                + $"după curățare → „{mesajSensDupa ?? "tace"}”.");
            s.Check("D4-V1 (privat, fix 5) `TintaPermisa(tip, sens)` are DOUĂ axe: L/V/LS doar pe livrare, A/C/AS doar pe "
                + "achiziție — (SDD, Achiziție) → L e REFUZATĂ de `ContaSeeder.VerificaD394` cu mesajul care numește "
                + "perechea, gardul tace după curățare, iar funcția refuză și în unitate (L/Achiziție, A/Livrare, "
                + "LS/Achiziție, AS/Livrare) și acceptă perechile coerente",
                mesajSens != null && mesajSens.Contains("SDD") && mesajSens.Contains("Achizitie") && mesajSens.Contains("L")
                && mesajSensDupa == null && MapariVii() == 13
                && !MapareD394.TintaPermisa(TipOperatiuneD394.L, SensTva.Achizitie)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.A, SensTva.Livrare)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.LS, SensTva.Achizitie)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.AS, SensTva.Livrare)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.V, SensTva.Achizitie)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.C, SensTva.Livrare)
                && MapareD394.TintaPermisa(TipOperatiuneD394.L, SensTva.Livrare)
                && MapareD394.TintaPermisa(TipOperatiuneD394.A, SensTva.Achizitie)
                && MapareD394.TintaPermisa(TipOperatiuneD394.V, SensTva.Livrare)
                && MapareD394.TintaPermisa(TipOperatiuneD394.C, SensTva.Achizitie)
                && MapareD394.TintaPermisa(TipOperatiuneD394.AS, SensTva.Achizitie)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.N, SensTva.Livrare)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.AI, SensTva.Livrare));
        }

        // ══════ F5 (104i): ștergerea e o decizie, nu o gaură ══════
        {
            using (var osTinta = s.Provider.CreateObjectSpace()) {
                var ned = osTinta.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
                var tinta = osTinta.FirstOrDefault<MapareD394>(m => m.TipTvaId == ned.ID && m.Sens == SensTva.Achizitie);
                new GardianEditare().OnObjectSpaceCreated(osTinta);
                osTinta.Delete(tinta);
                osTinta.CommitChanges();
            }
            int Refuzuri() {
                using var osNum = s.Provider.CreateObjectSpace();
                return osNum.GetObjectsQuery<RefuzSeed>().Count(r => r.Tip == nameof(MapareD394));
            }
            var refuzuri = Refuzuri();
            var dupaStergere = MapariVii();
            void Reseed() {
                using var osSeed = s.Provider.CreateObjectSpace();
                ContaSeeder.SeedMapareD394Privat(osSeed);
                osSeed.CommitChanges();
            }
            Reseed();
            var dupaReseed = MapariVii();
            var mesajVerificare = RuleazaGardianul();
            SqlBrut($"DELETE FROM \"RefuzuriSeed\" WHERE \"Tip\" = 'MapareD394'");
            Reseed();
            var dupaRestaurare = MapariVii();
            var mesajFinal = RuleazaGardianul();
            Console.WriteLine($"     MĂSURAT (D4-V1/F5): mapări vii 13 → {dupaStergere} după ștergere pe NED21/Achiziție "
                + $"(refuzuri: {refuzuri}) → {dupaReseed} după re-seed → "
                + $"{dupaRestaurare} după restaurare; gardian după re-seed: „{mesajVerificare ?? "tace"}”.");
            s.Check("D4-V1 (F5, 104i) ștergerea unei mapări D394 lasă un `RefuzSeed` și rămâne ștearsă la re-seed "
                + "(12 vii, nu 13), gardianul o citește ca „nemapată de utilizator”; după ștergerea refuzului revin la 13",
                refuzuri == 1
                && dupaStergere == 12 && dupaReseed == 12 && mesajVerificare == null
                && dupaRestaurare == 13 && mesajFinal == null);
        }
    }
}
