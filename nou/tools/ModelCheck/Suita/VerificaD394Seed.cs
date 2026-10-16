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
                    && i.Properties.Select(p => p.Name).SequenceEqual([nameof(MapareD394.TipTvaId), nameof(MapareD394.SensTva)])
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

        TipOperatiuneD394? Tip(string codTip, SensTva sensTva) => mapari
            .Where(m => m.TipTva.Cod == codTip && m.SensTva == sensTva)
            .Select(m => m.Tip).SingleOrDefault();
        bool Exclusa(string codTip, SensTva sensTva) => mapari
            .Any(m => m.TipTva.Cod == codTip && m.SensTva == sensTva && m.Tip == null && !string.IsNullOrWhiteSpace(m.Motiv));
        var tabel = ContaSeeder.MapariD394Privat;
        var total = tabel.Count;

        s.Check($"D4-V1 (privat, 123 a) tabelul D4-D2 e seed-uit integral: {total} mapări (13 tipuri, restul excluderi cu motiv), "
            + "toate cu ținte permise, o singură mapare per pereche",
            mapari.Count == total && tabel.Count(t => t.Tip != null) == 13
            && mapari.All(m => MapareD394.TintaPermisa(m.Tip, m.SensTva) && MapareD394.AreMotiv(m))
            && mapari.Select(m => (m.TipTvaId, m.SensTva)).Distinct().Count() == total);
        s.Check("D4-V1 (privat) cotele normale N21/N11/N9/N19: livrare → L, achiziție → A",
            new[] { "N21", "N11", "N9", "N19" }.All(c =>
                Tip(c, SensTva.Livrare) == TipOperatiuneD394.L && Tip(c, SensTva.Achizitie) == TipOperatiuneD394.A));
        s.Check("D4-V1 (privat) taxarea inversă TI21/TI19: livrare → V, achiziție → C",
            new[] { "TI21", "TI19" }.All(c =>
                Tip(c, SensTva.Livrare) == TipOperatiuneD394.V && Tip(c, SensTva.Achizitie) == TipOperatiuneD394.C));
        s.Check("D4-V1 (privat, 123 a) NED21 pe achiziție → A, pe livrare exclusă; SDD/SFD/NIM și importul excluse pe ambele sensuri, cu motiv",
            Tip("NED21", SensTva.Achizitie) == TipOperatiuneD394.A && Exclusa("NED21", SensTva.Livrare)
            && new[] { "SDD", "SFD", "NIM", "IMP", "IMP21", "IMP11", "IMPTI21", "IMPTI11" }.All(c =>
                Exclusa(c, SensTva.Livrare) && Exclusa(c, SensTva.Achizitie)));

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
        // Rescrie ținta unei mapări seed-uite pe ușa de sistem, cheamă gardianul de seed și restaurează rândul.
        (string Gard, string Dupa) CuTinta(string cod, SensTva sens, TipOperatiuneD394? tip, string motivNou = null) {
            Guid id; TipOperatiuneD394? vechi; string motiv;
            using (var osScrie = s.Provider.CreateObjectSpace()) {
                var tipTva = osScrie.FirstOrDefault<TipTva>(t => t.Cod == cod);
                var m = osScrie.FirstOrDefault<MapareD394>(x => x.TipTvaId == tipTva.ID && x.SensTva == sens);
                (id, vechi, motiv) = (m.ID, m.Tip, m.Motiv);
                m.Tip = tip; m.Motiv = motivNou;
                osScrie.CommitChanges();
            }
            var gard = RuleazaGardianul();
            using (var osRestaurare = s.Provider.CreateObjectSpace()) {
                var m = osRestaurare.GetObjectByKey<MapareD394>(id);
                m.Tip = vechi; m.Motiv = motiv;
                osRestaurare.CommitChanges();
            }
            return (gard, RuleazaGardianul());
        }
        s.Check("D4-V1 (privat) gardianul de seed `ContaSeeder.VerificaD394` tace pe profilul seed-uit",
            RuleazaGardianul() == null);

        // ══════ Ținta interzisă: AÎ/N nu se mapează (D4-D2) ══════
        {
            var (mesajGard, mesajDupa) = CuTinta("SDD", SensTva.Livrare, TipOperatiuneD394.AI);
            Console.WriteLine($"     MĂSURAT (D4-V1/țintă): gardian pe (SDD, Livrare) → AÎ: „{mesajGard ?? "<NU A ARUNCAT>"}”; "
                + $"după restaurare → „{mesajDupa ?? "tace"}”.");
            s.Check("D4-V1 (privat) o mapare cu ținta AÎ e REFUZATĂ de `ContaSeeder.VerificaD394` (AÎ se derivă din "
                + "partener, nu se mapează) și gardul tace după restaurare; `MapareD394.TintaPermisa` refuză și N",
                mesajGard != null && mesajGard.Contains("SDD") && mesajGard.Contains("AI")
                && mesajDupa == null && MapariVii() == total
                && !MapareD394.TintaPermisa(TipOperatiuneD394.N, SensTva.Achizitie) && !MapareD394.TintaPermisa(TipOperatiuneD394.AI, SensTva.Achizitie)
                && MapareD394.TintaPermisa(TipOperatiuneD394.LS, SensTva.Livrare));
            var (mesajSens, mesajSensDupa) = CuTinta("SDD", SensTva.Achizitie, TipOperatiuneD394.L);
            Console.WriteLine($"     MĂSURAT (D4-V1/sens): gardian pe (SDD, Achiziție) → L: „{mesajSens ?? "<NU A ARUNCAT>"}”; "
                + $"după restaurare → „{mesajSensDupa ?? "tace"}”.");
            s.Check("D4-V1 (privat, fix 5) `TintaPermisa(tip, sens)` are DOUĂ axe: L/V/LS doar pe livrare, A/C/AS doar pe "
                + "achiziție — (SDD, Achiziție) → L e REFUZATĂ de `ContaSeeder.VerificaD394` cu mesajul care numește "
                + "perechea, gardul tace după restaurare, iar funcția refuză și în unitate și acceptă perechile coerente și excluderea",
                mesajSens != null && mesajSens.Contains("SDD") && mesajSens.Contains("Achizitie") && mesajSens.Contains("L")
                && mesajSensDupa == null && MapariVii() == total
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
                && MapareD394.TintaPermisa(null, SensTva.Livrare)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.N, SensTva.Livrare)
                && !MapareD394.TintaPermisa(TipOperatiuneD394.AI, SensTva.Livrare));
            var (mesajMotiv, _) = CuTinta("NED21", SensTva.Livrare, null);
            var (mesajExclusa, _) = CuTinta("NED21", SensTva.Livrare, null, "probă");
            var (mesajCoerenta, mesajCoerentaDupa) = CuTinta("N21", SensTva.Livrare, TipOperatiuneD394.V);
            var (mesajImport, _) = CuTinta("IMP21", SensTva.Achizitie, TipOperatiuneD394.A);
            Console.WriteLine($"     MĂSURAT (123 b): fără motiv: „{mesajMotiv ?? "<NU A ARUNCAT>"}”; N21/Livrare → V: „{mesajCoerenta ?? "<NU A ARUNCAT>"}”; "
                + $"IMP21/Achiziție → A: „{mesajImport ?? "<NU A ARUNCAT>"}”.");
            s.Check("SC-DECL-16 (seed, 123 a/b) gardianul de seed refuză excluderea fără motiv, perechea incoerentă cu D300 "
                + "(N21/Livrare → V, iar D300 o pune pe rd. 9) și importul mapat pe un tip (IMP21/Achiziție → A); "
                + "excluderea cu motiv trece",
                mesajMotiv != null && mesajMotiv.Contains("motiv")
                && mesajCoerenta != null && mesajCoerenta.Contains("N21") && mesajCoerenta.Contains("rd. 9")
                && mesajImport != null && mesajImport.Contains("IMP21") && mesajImport.Contains("import")
                && mesajExclusa == null && mesajCoerentaDupa == null && MapariVii() == total);
        }

        // ══════ 123 b pe ușa de editare: refuz la salvare, oricare mapare s-ar salva ══════
        {
            string PrinGardian(Action<IObjectSpace> scrie) {
                using var osG = s.Provider.CreateObjectSpace();
                try {
                    scrie(osG);
                    GardianEditare.Verifica(osG);
                    return null;
                }
                catch (OperareException e) { return e.Message; }
                finally { osG.Rollback(); }
            }
            MapareD394 D394(IObjectSpace o, string cod, SensTva sens) {
                var t = o.FirstOrDefault<TipTva>(x => x.Cod == cod);
                return o.FirstOrDefault<MapareD394>(x => x.TipTvaId == t.ID && x.SensTva == sens);
            }
            var peD394 = PrinGardian(o => D394(o, "N21", SensTva.Livrare).Tip = TipOperatiuneD394.V);
            var peD300 = PrinGardian(o => {
                var m = o.CreateObject<MapareD300>();
                m.TipTva = o.FirstOrDefault<TipTva>(x => x.Cod == "N21");
                m.SensTva = SensTva.Livrare;
                m.Rand = o.FirstOrDefault<RandD300>(r => r.Cod == "13");
            });
            var faraMotiv = PrinGardian(o => D394(o, "SDD", SensTva.Livrare).Motiv = null);
            var import = PrinGardian(o => {
                var m = D394(o, "IMPTI21", SensTva.Achizitie);
                m.Tip = TipOperatiuneD394.C; m.Motiv = null;
            });
            var coerenta = PrinGardian(o => D394(o, "SDD", SensTva.Livrare).Motiv = "probă de excludere coerentă");
            Console.WriteLine($"     MĂSURAT (SC-DECL-16, gardian): D394 N21/Livrare → V: „{peD394 ?? "<ACCEPTAT>"}”; "
                + $"D300 N21/Livrare → rd. 13: „{peD300 ?? "<ACCEPTAT>"}”; fără motiv: „{faraMotiv ?? "<ACCEPTAT>"}”; "
                + $"IMPTI21 → C: „{import ?? "<ACCEPTAT>"}”.");
            s.Check("SC-DECL-16 (gardianul de editare, 123 b) maparea incoerentă se refuză la salvare pe oricare formular: "
                + "D394 N21/Livrare → V (D300 rd. 9), D300 N21/Livrare → rd. 13 (D394 L), excluderea fără motiv și importul "
                + "mapat pe un tip; o excludere coerentă cu motiv se salvează, iar mapările rămân neatinse",
                peD394 != null && peD394.Contains("rd. 9")
                && peD300 != null && peD300.Contains("rd. 13")
                && faraMotiv != null && faraMotiv.Contains("motiv")
                && import != null && import.Contains("import")
                && coerenta == null && MapariVii() == total && RuleazaGardianul() == null);
        }

        // ══════ F5 (104i): ștergerea e o decizie, nu o gaură ══════
        {
            using (var osTinta = s.Provider.CreateObjectSpace()) {
                var ned = osTinta.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
                var tinta = osTinta.FirstOrDefault<MapareD394>(m => m.TipTvaId == ned.ID && m.SensTva == SensTva.Achizitie);
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
            Console.WriteLine($"     MĂSURAT (D4-V1/F5): mapări vii {total} → {dupaStergere} după ștergere pe NED21/Achiziție "
                + $"(refuzuri: {refuzuri}) → {dupaReseed} după re-seed → "
                + $"{dupaRestaurare} după restaurare; gardian după re-seed: „{mesajVerificare ?? "tace"}”.");
            s.Check("D4-V1 (F5, 104i) ștergerea unei mapări D394 lasă un `RefuzSeed` și rămâne ștearsă la re-seed "
                + "(una mai puțin), gardianul o citește ca „nemapată de utilizator”; după ștergerea refuzului revin toate",
                refuzuri == 1
                && dupaStergere == total - 1 && dupaReseed == total - 1 && mesajVerificare == null
                && dupaRestaurare == total && mesajFinal == null);
        }
    }
}
