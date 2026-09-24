#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub;

public sealed record SoldInitial(Guid Cont, N.Latura Latura, decimal Valoare,
    bool Detaliat = false, N.Carte Carte = N.Carte.Contabil);
public sealed record LotInitial(Guid Cont, Guid Lot, Guid Gestiune, decimal Cantitate, decimal Valoare,
    N.Carte Carte = N.Carte.Contabil);
public sealed record PartidaInitiala(Guid Cont, Guid Partener, Guid Referinta, N.Latura Latura, decimal Valoare,
    N.Carte Carte = N.Carte.Contabil);

public static partial class Materializare {
    public const string DeschidereInvalida = "DESCHIDERE_INVALIDA";
    public const string DeschidereDiferenta = "DESCHIDERE_DIFERENTA";
    public const string DeschidereExistenta = "DESCHIDERE_EXISTENTA";

    /// <summary>Materializează soldurile inițiale detaliate în tranzacția apelantului, fără commit.</summary>
    public static Guid Deschide(IObjectSpace os, DateOnly data, IReadOnlyList<SoldInitial> solduri,
            IReadOnlyList<LotInitial> loturi, IReadOnlyList<PartidaInitiala> partide) {
        ArgumentNullException.ThrowIfNull(os);
        ArgumentNullException.ThrowIfNull(solduri);
        ArgumentNullException.ThrowIfNull(loturi);
        ArgumentNullException.ThrowIfNull(partide);
        CereTranzactie(os);
        GardianPerioada.VerificaDeschisa(os, data);
        if (os.ModifiedObjects.OfType<Tranzactie>().Any(t => t.Fel == N.FelTranzactie.Deschidere)
            || os.GetObjectsQuery<Tranzactie>().Any(t => t.Fel == N.FelTranzactie.Deschidere))
            throw new OperareException($"{DeschidereExistenta}: Baza are deja o deschidere.");
        if (solduri.Count == 0) RefuzaDeschidere("Lipsesc soldurile de control.");
        var chei = solduri.Select(s => (s.Carte, s.Cont, s.Latura)).ToArray();
        if (chei.Distinct().Count() != chei.Length) RefuzaDeschidere("Sold de control duplicat.");
        var idConturi = solduri.Select(s => s.Cont).Concat(loturi.Select(l => l.Cont))
            .Concat(partide.Select(p => p.Cont)).Distinct().ToArray();
        var conturi = os.GetObjectsQuery<Cont>().Where(c => idConturi.Contains(c.ID))
            .Select(c => new { c.ID, c.UrmarestePartide }).ToDictionary(c => c.ID, c => c.UrmarestePartide);
        if (conturi.Count != idConturi.Length) RefuzaDeschidere("Cont inexistent.");
        foreach (var s in solduri) {
            VerificaMasura(s.Valoare, N.Scara.Bani);
            if (!Enum.IsDefined(s.Carte) || !Enum.IsDefined(s.Latura)) RefuzaDeschidere("Carte sau latură invalidă.");
        }
        foreach (var grup in solduri.GroupBy(s => s.Carte))
            if (grup.Sum(s => s.Latura == N.Latura.Debit ? s.Valoare : -s.Valoare) != 0)
                RefuzaDeschidere($"Soldurile cărții {grup.Key} nu sunt echilibrate.");
        var idLoturi = loturi.Select(l => l.Lot).Distinct().ToArray();
        var fapteLot = os.GetObjectsQuery<Lot>().Where(l => idLoturi.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId, l.Data, l.Produs.TipMaterial.ContImplicitId })
            .ToDictionary(l => l.ID);
        var idGestiuni = loturi.Select(l => l.Gestiune).Distinct().ToArray();
        var gestiuni = os.GetObjectsQuery<Gestiune>().Where(g => idGestiuni.Contains(g.ID)).Select(g => g.ID).ToHashSet();
        var idParteneri = partide.Select(p => p.Partener).Distinct().ToArray();
        var parteneri = os.GetObjectsQuery<Repartitor>().Where(p => idParteneri.Contains(p.ID)
            && (p is Partener || p is Angajat)).Select(p => p.ID).ToHashSet();
        var postari = new List<N.Postare>();
        var unitati = new HashSet<(N.Carte, Guid, Guid?)>();
        foreach (var l in loturi) {
            VerificaMasura(l.Valoare, N.Scara.Bani); VerificaMasura(l.Cantitate, N.Scara.Cantitate);
            if (!fapteLot.TryGetValue(l.Lot, out var lot) || !gestiuni.Contains(l.Gestiune)
                || lot.ContImplicitId != l.Cont || lot.Data > data || lot.Data == default || l.Cantitate == 0
                || conturi[l.Cont])
                RefuzaDeschidere("Lot, cont, cantitate, dată sau gestiune invalidă.");
            if (!unitati.Add((l.Carte, l.Lot, l.Gestiune))) RefuzaDeschidere("Poziție de lot duplicată.");
            var u = new N.Unitate(l.Lot, N.FelUnitate.Lot, l.Cont, null, lot!.ProdusId, lot.Data);
            postari.Add(new(new N.Coordonate { Cont = l.Cont, Latura = N.Latura.Debit, Data = data,
                Carte = l.Carte, Gestiune = l.Gestiune, Produs = lot.ProdusId, Unitate = u },
                l.Cantitate, 0, l.Valoare, new(Guid.Empty, null)));
        }
        foreach (var p in partide) {
            VerificaMasura(p.Valoare, N.Scara.Bani);
            if (!conturi[p.Cont] || !parteneri.Contains(p.Partener)
                || p.Referinta == Guid.Empty || !Enum.IsDefined(p.Latura))
                RefuzaDeschidere("Partida cere cont urmărit pe partide, partener și referință valide.");
            var u = N.Unitate.DeschidePartida(p.Cont, p.Partener, p.Referinta, data);
            if (!unitati.Add((p.Carte, u.Id, null))) RefuzaDeschidere("Partidă duplicată.");
            postari.Add(new(new N.Coordonate { Cont = p.Cont, Latura = p.Latura, Data = data,
                Carte = p.Carte, Partener = p.Partener, Unitate = u }, 0, 0, p.Valoare, new(Guid.Empty, null)));
        }
        var detalii = postari.GroupBy(p => (p.Coordonate.Carte, p.Coordonate.Cont, p.Coordonate.Latura))
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Valoare));
        if (detalii.Keys.Except(chei).Any()) RefuzaDeschidere("Detaliere fără sold de control.");
        foreach (var s in solduri) {
            var cheie = (s.Carte, s.Cont, s.Latura);
            if (detalii.TryGetValue(cheie, out var suma) || s.Detaliat || conturi[s.Cont]) {
                if (!detalii.ContainsKey(cheie) || suma != s.Valoare)
                    throw new OperareException($"{DeschidereDiferenta}: {cheie}: control {s.Valoare}, detalii {suma}, diferență {s.Valoare - suma}.");
            }
            else postari.Add(new(new N.Coordonate { Cont = s.Cont, Latura = s.Latura, Data = data,
                Carte = s.Carte }, 0, 0, s.Valoare, new(Guid.Empty, null)));
        }
        var tranzactie = new N.Tranzactie(N.FelTranzactie.Deschidere, data, null, postari);
        var refuzuri = N.Conservare.Verifica(tranzactie);
        if (refuzuri.Count > 0) RefuzaDeschidere(string.Join("\n", Mesaje(refuzuri)));
        return Scrie(os, null, tranzactie);
    }

    static void CereTranzactie(IObjectSpace os) {
        if (os is not EFCoreObjectSpace ef || ef.DbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Comanda cere tranzacție explicită a apelantului.");
    }

    static void VerificaMasura(decimal valoare, int scara) {
        if (valoare < 0 || !N.Scara.EsteLa(valoare, scara)) RefuzaDeschidere("Măsură negativă sau în afara scării.");
    }

    [System.Diagnostics.CodeAnalysis.DoesNotReturn]
    static void RefuzaDeschidere(string mesaj) => throw new OperareException($"{DeschidereInvalida}: {mesaj}");
}
