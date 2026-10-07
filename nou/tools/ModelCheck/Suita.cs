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

sealed class Suita {
    public Suita(string[] args, ProfilContabil profil, string connectionString, HashSet<string> filtruScenarii) {
        Args = args;
        Profil = profil;
        ConnectionString = connectionString;
        FiltruScenarii = filtruScenarii;
    }

    public string[] Args { get; }
    public ProfilContabil Profil { get; }
    public bool Privat => Profil == ProfilContabil.Privat;
    public string ConnectionString { get; }
    public HashSet<string> FiltruScenarii { get; }
    public DbContextOptions<BackOfficeEFCoreDbContext> Opts { get; set; }
    public EFCoreObjectSpaceProvider<BackOfficeEFCoreDbContext> Provider { get; set; }
    public int Esecuri { get; private set; }

    public IObjectSpace Deschide() => Provider.CreateObjectSpace();

    public void Check(string nume, bool ok) {
        Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {nume}");
        if (!ok)
            Esecuri++;
    }

    // Mesajul refuzului de domeniu, sau null dacă acțiunea a trecut.
    public string Refuz(Action actiune) {
        try { actiune(); return null; }
        catch (OperareException e) { return e.Message; }
    }

    public void CheckRefuza(string nume, Action actiune) {
        try {
            actiune();
            Check(nume, false);
        }
        catch (OperareException e) {
            Console.WriteLine($"OK   {nume} — „{e.Message.Split('\n')[0]}”");
        }
    }

    public void Rezumat() {
        Console.WriteLine(Esecuri == 0 ? "\nToate verificările au trecut." : $"\n{Esecuri} verificări EȘUATE.");
        Environment.ExitCode = Esecuri == 0 ? 0 : 1;
    }

    // Felia 27 (F27-D1): `Inchisa` nu se mai scrie direct nicăieri — probele închid
    // și redeschid prin comanda motorului, singura ușă. Lanțul cere ordinea, deci
    // redeschiderea se face în ORDINE INVERSĂ. Închiderea de scenă acceptă toate
    // constatările de conținut (F27-D2, `InchideAcceptTot`); probele `PER-V*` închid
    // explicit cu `[]` — acolo lista goală E ce se măsoară.
    public void InchideLant(IObjectSpace os, int an, params int[] luni) {
        foreach (var luna in luni)
            InchideAcceptTot(os, an, luna);
    }

    public void RedeschideLant(IObjectSpace os, int an, params int[] luni) {
        foreach (var luna in luni.Reverse())
            PerioadaService.Redeschide(os, an, luna, "Probă ModelCheck", null, "ModelCheck");
    }

    // Istoricul rămâne append-only în produs; în harness e reziduu de scenă, deci se
    // purjă FIZIC (F13-D2), ca orice scenă.
    public void PurjaIstoricPerioade(IObjectSpace os, int an) {
        var ids = os.GetObjectsQuery<PerioadaFiscala>()
            .Where(p => p.An == an).Select(p => p.ID).ToList();
        new Purja(os).Adauga(os.GetObjectsQuery<InchiderePerioada>()
            .Where(i => ids.Contains(i.PerioadaId))).Executa();
    }

    // 104c: regulile culegerii (scara coloanei) sunt ale gardianului, pe ușa securizată a API-ului.
    public IObjectSpace OsCuGardian() {
        var o = Provider.CreateObjectSpace();
        new GardianEditare().OnObjectSpaceCreated(o);
        return o;
    }

    // Scenele suitei nu fac decontul de TVA — subiectul lor e altul (corecția,
    // partidele, snapshot-urile), iar a genera o ITV în fiecare ar schimba chiar
    // cifrele contabile pe care le măsoară. `ItvLipsa` e însă BLOCANT în seed-ul
    // privat, iar blocantul nu se acceptă. Ieșirea e cea a operatorului real care
    // vrea să închidă o lună fără decontul ei: COBOARĂ severitatea în politică,
    // închide, o pune la loc — deci mecanismul rămâne cel probat, nu unul ocolit.
    // Valoarea de SEED e subiectul blocului `ACC-V*`, care o citește neatinsă.
    public InchiderePerioada InchideAcceptTot(IObjectSpace os, int an, int luna, string de = "ModelCheck") {
        var severitate = SeveritateItvLipsa();
        SeteazaSeveritateItvLipsa(SeveritateConstatare.Avertisment);
        try {
            return PerioadaService.Inchide(os, an, luna,
                PerioadaService.Verifica(os, an, luna).Select(c => c.Cheie).ToArray(), null, de);
        }
        finally {
            SeteazaSeveritateItvLipsa(severitate);
        }
    }

    // Severitatea unui fel de constatare, pe ObjectSpace PROPRIU: scena care închide
    // are de obicei modificări necomise pe al ei, iar un `CommitChanges` străin
    // le-ar scrie înainte de vreme.
    public SeveritateConstatare SeveritateItvLipsa() {
        using var os = Provider.CreateObjectSpace();
        return os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.ItvLipsa)?.Severitate
            ?? SeveritateConstatare.Avertisment;
    }

    public void SeteazaSeveritateItvLipsa(SeveritateConstatare severitate) {
        using var os = Provider.CreateObjectSpace();
        var rand = os.FirstOrDefault<PoliticaInchidere>(p => p.Fel == FelConstatareInchidere.ItvLipsa);
        if (rand == null || rand.Severitate == severitate)
            return;
        rand.Severitate = severitate;
        os.CommitChanges();
    }

    // Ajutoare de TIPĂRIRE ale blocului de mai sus (doar pentru liniile `MĂSURAT`).
    public string PrimaLinie(string mesaj) => mesaj == null ? "<a trecut>" : mesaj.Split('\n')[0];

    public string Ziua(DateOnly data) => data == default ? "-" : data.ToString("dd.MM.yyyy");
}
