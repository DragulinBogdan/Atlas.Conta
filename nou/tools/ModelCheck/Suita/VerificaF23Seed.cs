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
// F23-V3 — SEED-ul implicitelor, pe funcția REALĂ
// ---------------------------------------------------------------------------
// Ca la D17-V1: numărul nu e o constantă scrisă în probă, e cardinalitatea
// tabelului de seed × „fiecare rând al lui există exact o dată în bază”. Un rând
// adăugat în tabel fără rând în bază (sau invers) pică aici.
static class VerificaF23Seed {
    public static void Ruleaza(Suita s, bool privat) {
        if (!privat) {
            using var osB = s.Provider.CreateObjectSpace();
            var randuriB = osB.GetObjectsQuery<PoliticaTvaImplicit>().Count();
            var coduriInactiveB = osB.GetObjectsQuery<TipTva>().Where(t => !t.Activ).Select(t => t.Cod)
                .ToList().OrderBy(c => c, StringComparer.Ordinal).ToList();
            var activeB = osB.GetObjectsQuery<TipTva>().Count(t => t.Activ);
            Console.WriteLine($"     MĂSURAT (F23-V3/bugetar): {randuriB} implicite de TVA; {activeB} tipuri "
                + $"active, {coduriInactiveB.Count} inactive ({string.Join(", ", coduriInactiveB)}).");
            s.Check("F23-V3 (bugetar) profilul n-are rânduri de implicit (totul e capitalizat, ancora ajunge — "
                + "F23-D2), iar din cele patru cote capitalizate DOAR `CAP19` (istorică) e stinsă din culegere; "
                + "restul rămân active",
                randuriB == 0 && activeB == 3
                && coduriInactiveB.SequenceEqual(new[] { "CAP19" }));
            return;
        }

        var asteptate = ContaSeeder.ImpliciteTvaPrivat;
        List<(string Tip, ClasaFiscalaPartener? Clasa, DateOnly? DeLa, string Tva, bool DinSeed)> randuriBd;
        using (var os = s.Provider.CreateObjectSpace())
            randuriBd = os.GetObjectsQuery<PoliticaTvaImplicit>()
                .Select(p => new {
                    Tip = p.TipDocument.Cod, p.ClasaFiscala, p.ValabilDeLa, Tva = p.TipTva.Cod, p.DinSeed })
                .ToList()
                .Select(p => (p.Tip, p.ClasaFiscala, p.ValabilDeLa, p.Tva, p.DinSeed)).ToList();
        Console.WriteLine($"     MĂSURAT (F23-V3/privat): {randuriBd.Count} rânduri (tabel: {asteptate.Count}); "
            + string.Join(", ", randuriBd
                .OrderBy(r => r.Tip, StringComparer.Ordinal).ThenBy(r => (int?)r.Clasa)
                .Select(r => $"{r.Tip}×{r.Clasa?.ToString() ?? "orice"}"
                    + $"{(r.DeLa == null ? "" : "@" + r.DeLa.Value.ToString("dd.MM.yyyy"))}→{r.Tva}"
                    + $"{(r.DinSeed ? "" : " (MANUAL)")}")) + ".");
        s.Check("F23-V3 (privat) seed-ul scrie EXACT tabelul F23-D2 — 11 rânduri, câte unul pentru fiecare pereche "
            + "(tip × clasă fiscală) SIGURĂ în lege: livrarea scutită pe FCL/RDC × UE/extra-UE, taxarea inversă "
            + "intracomunitară pe FCT/RLF × UE, iar de la 83f/g achiziția de la neînregistratul RO (NIM) și cea "
            + "extra-UE (IMP) pe FCT/RLF; toate „dintotdeauna” (niciunul nu depinde de cotă) și toate cu timbrul "
            + "seed-ului",
            randuriBd.Count == asteptate.Count && randuriBd.Count == 11
            && randuriBd.Select(r => (r.Tip, r.Clasa, r.DeLa)).Distinct().Count() == 11
            && asteptate.All(a => randuriBd.Count(r => r.Tip == a.TipDocument && r.Clasa == a.Clasa
                && r.DeLa == null && r.Tva == a.TipTva) == 1)
            && randuriBd.All(r => r.DinSeed));

        List<string> codInactive;
        int codActive;
        using (var os = s.Provider.CreateObjectSpace()) {
            codInactive = os.GetObjectsQuery<TipTva>().Where(t => !t.Activ).Select(t => t.Cod)
                .ToList().OrderBy(c => c, StringComparer.Ordinal).ToList();
            codActive = os.GetObjectsQuery<TipTva>().Count(t => t.Activ);
        }
        Console.WriteLine($"     MĂSURAT (F23-V3/activi): {codActive} tipuri active, {codInactive.Count} inactive "
            + $"({string.Join(", ", codInactive)}).");
        s.Check("F23-V3 (privat) cotele ISTORICE ies din culegere: `N19` și `TI19` (până la 31.07.2025) sunt stinse "
            + "de seed, restul rămân active. Un tip inactiv NU invalidează istoria — rămâne pe documentele lui —, "
            + "doar nu se mai propune",
            codInactive.SequenceEqual(new[] { "N19", "TI19" }) && codActive >= 5);

        // Idempotența, pe FUNCȚIA REALĂ (nu pe o copie a tabelului).
        using (var osSeed = s.Provider.CreateObjectSpace()) {
            ContaSeeder.SeedPoliticiTvaImplicitPrivat(osSeed);
            osSeed.CommitChanges();
        }
        int dupaReseed;
        using (var os = s.Provider.CreateObjectSpace())
            dupaReseed = os.GetObjectsQuery<PoliticaTvaImplicit>().Count();
        Console.WriteLine($"     MĂSURAT (F23-V3/re-seed): {randuriBd.Count} → {dupaReseed} rânduri după a doua "
            + "rulare a seed-ului real.");
        s.Check("F23-V3 (privat) `SeedPoliticiTvaImplicit` e IDEMPOTENT pe cheia indexului (tip × clasă × "
            + "valabilitate): `--forceUpdate` pe o bază deja seed-uită nu adaugă un al doilea rând, care ar fi "
            + "făcut rezolvarea nedeterministă",
            dupaReseed == 11);

        // „Șters de utilizator” (104i): ștergerea trece prin gardian, care lasă
        // `RefuzSeed` — politica e date (decizia 4), iar un rând pe care clientul
        // l-a scos nu se recreează pe la spatele lui.
        Guid idSters;
        int dupaStergere;
        using (var os = s.Provider.CreateObjectSpace()) {
            var idFcl = os.FirstOrDefault<TipDocument>(t => t.Cod == "FCL").ID;
            var rand = os.GetObjectsQuery<PoliticaTvaImplicit>().ToList()
                .First(p => p.TipDocumentId == idFcl && p.ClasaFiscala == ClasaFiscalaPartener.Ue
                    && p.ValabilDeLa == null);
            idSters = rand.ID;
            new GardianEditare().OnObjectSpaceCreated(os);
            os.Delete(rand);
            os.CommitChanges();
        }
        using (var osSeed = s.Provider.CreateObjectSpace()) {
            ContaSeeder.SeedPoliticiTvaImplicitPrivat(osSeed);
            osSeed.CommitChanges();
        }
        using (var os = s.Provider.CreateObjectSpace())
            dupaStergere = os.GetObjectsQuery<PoliticaTvaImplicit>().Count();
        Console.WriteLine($"     MĂSURAT (F23-V3/șters de utilizator): FCL×Ue șters, apoi seed re-rulat ⇒ "
            + $"{dupaStergere} rânduri vii (așteptat 10, adică NU s-a recreat).");
        s.Check("F23-V3 (privat) rândul ȘTERS de utilizator NU se recreează la re-seed (aceeași disciplină ca "
            + "mapările D300/D394): politica e DATE, iar ștergerea e o decizie a clientului — seed-ul o respectă "
            + "și o SPUNE în consolă, nu o anulează tăcut",
            dupaStergere == 10);

        // Restaurarea: refuzul se purjează, apoi seed-ul recreează rândul cu timbru.
        using (var os = s.Provider.CreateObjectSpace())
            new Purja(os).Adauga(os.GetObjectsQuery<RefuzSeed>().Where(r => r.Tip == nameof(PoliticaTvaImplicit)))
                .Executa();
        using (var osSeed = s.Provider.CreateObjectSpace()) {
            ContaSeeder.SeedPoliticiTvaImplicitPrivat(osSeed);
            osSeed.CommitChanges();
        }
        int dupaRestaurare, cuTimbru;
        using (var os = s.Provider.CreateObjectSpace()) {
            dupaRestaurare = os.GetObjectsQuery<PoliticaTvaImplicit>().Count();
            cuTimbru = os.GetObjectsQuery<PoliticaTvaImplicit>().Count(p => p.DinSeed);
        }
        Console.WriteLine($"     MĂSURAT (F23-V3/restaurare): {dupaRestaurare} rânduri, {cuTimbru} cu timbru.");
        s.Check("F23-V3 (privat) restaurare: după purja refuzului, seed-ul recreează rândul — 11 rânduri, "
            + "toate cu `DinSeed`; baza rămâne exact cum a găsit-o proba",
            dupaRestaurare == 11 && cuTimbru == 11);
    }
}
