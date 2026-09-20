#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

/// <summary>
/// PLT și INC (B-D5): o mișcare per linie de defalcare — banii trec de pe contul
/// unei laturi pe al celeilalte, iar partida terțului se nominalizează pe
/// documentul-sursă cât ține restul lui, apoi pe a documentului însuși. Plata și
/// încasarea diferă prin regula de contare și prin latura care poartă terțul, nu
/// prin cod.
/// </summary>
public sealed class DeclarantTrezorerie : IDeclarant {
    public static readonly DeclarantTrezorerie Instanta = new();

    DeclarantTrezorerie() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);

        var doc = operand.Document;
        var esteVirament = EContPropriu(doc.Predator.Fel) && EContPropriu(doc.Primitor.Fel);
        Laturile(doc, refuzuri);
        Liniile(operand, esteVirament, refuzuri);

        var contari = new ContareLinie?[operand.Linii.Count];
        for (var i = 0; i < operand.Linii.Count; i++)
            contari[i] = Contari.Rezolva(operand, operand.Linii[i], refuzuri);
        if (refuzuri.Count > 0)
            return null;

        var tert = ETert(doc.Predator.Fel) ? doc.Predator.Id
            : ETert(doc.Primitor.Fel) ? doc.Primitor.Id
            : (Guid?)null;
        var miscari = new List<N.Miscare>(operand.Linii.Count);
        var decizii = new List<N.Decizie>();
        var ipoteze = new List<N.Ipoteza>();
        var rest = operand.RestPartidaSursa ?? 0m;
        var citite = new List<Guid>();
        var alePartidelorSursei = new Dictionary<Guid, decimal>();
        var proprieDeclarata = false;

        for (var i = 0; i < operand.Linii.Count; i++) {
            var linie = operand.Linii[i];
            var contare = contari[i]!.Value;
            decizii.Add(new N.ContRezolvat(linie.Id, contare.ContDebit, contare.SursaDebit.ToString()));
            decizii.Add(new N.ContRezolvat(linie.Id, contare.ContCredit, contare.SursaCredit.ToString()));

            var (gestiuneDebit, gestiuneCredit) = Gestiuni(doc, contare.Regula, esteVirament);
            var debit = new N.Capat {
                Cont = contare.ContDebit,
                Gestiune = gestiuneDebit,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideDebit, contare.Regula.Comun),
            };
            var credit = new N.Capat {
                Cont = contare.ContCredit,
                Gestiune = gestiuneCredit,
                Analiza = Contari.Analiza(linie.Analiza, contare.Regula.OverrideCredit, contare.Regula.Comun),
            };

            // B-D8 pct. 10: partida se deschide DOAR pe contul cu `RolTert`.
            var peDebit = tert is not null && Partide.ARolTert(operand, contare.ContDebit);
            var contTert = peDebit ? contare.ContDebit
                : tert is not null && Partide.ARolTert(operand, contare.ContCredit) ? contare.ContCredit
                : (Guid?)null;
            if (contTert is not Guid cont || tert is not Guid partener) {
                miscari.Add(new N.Miscare(credit, debit, 0m, 0m, linie.Valoare, new N.Cauza(doc.Id, linie.Id)));
                continue;
            }

            var proprie = Partide.Proprie(operand, cont, partener);
            var bucati = new List<(N.Unitate Partida, decimal Suma)>(2);
            // TR-D2a: stingerea E postarea care numește partida stinsă; restul liniei
            // rămâne avans pe partida proprie, ca azi excedentul. Plafonul e ce ține
            // partida sursei pe ACEST cont, nu restul documentului-sursă (MAJOR-1).
            if (Partide.Sursa(operand, cont, partener) is { } sursa) {
                if (!citite.Contains(cont)) {
                    ipoteze.Add(new N.SoldUnitateCitit(sursa.Unitate, sursa.Sold));
                    citite.Add(cont);
                }
                if (!alePartidelorSursei.TryGetValue(cont, out var alPartidei))
                    alePartidelorSursei[cont] = alPartidei = Math.Abs(sursa.Sold.Net);
                var plafon = Math.Min(rest, alPartidei);
                if (plafon > 0m)
                    try {
                        var nominalizare = N.Fifo.Nominalizeaza(
                            linie.Valoare, [new N.Disponibil(sursa.Unitate, plafon)]);
                        foreach (var alocare in nominalizare.Alocari) {
                            bucati.Add((alocare.Unitate, alocare.Masura));
                            decizii.Add(new N.AlocareFifo(linie.Id, alocare.Unitate, alocare.Masura));
                            rest -= alocare.Masura;
                            alePartidelorSursei[cont] -= alocare.Masura;
                        }
                        if (nominalizare.Ramas > 0m)
                            bucati.Add((proprie, nominalizare.Ramas));
                    }
                    catch (N.RefuzException e) {
                        // Refuzul e al liniei, nu al documentului: iterarea continuă
                        // ca să iasă TOATE refuzurile, nu primul (B-D2, MINOR-1).
                        refuzuri.Add(new N.Refuz(e.Refuz.Cod, e.Refuz.Mesaj, linie.Id));
                        continue;
                    }
            }
            if (bucati.Count == 0)
                bucati.Add((proprie, linie.Valoare));

            if (!proprieDeclarata && bucati.Any(b => b.Partida.Id == proprie.Id)) {
                decizii.Add(new N.PartidaDeschisa(linie.Id, proprie));
                proprieDeclarata = true;
            }
            foreach (var (partida, suma) in bucati)
                miscari.Add(new N.Miscare(
                    peDebit ? credit : credit with { Partener = partener, Unitate = partida },
                    peDebit ? debit with { Partener = partener, Unitate = partida } : debit,
                    0m,
                    0m,
                    suma,
                    new N.Cauza(doc.Id, linie.Id)));
        }
        if (refuzuri.Count > 0)
            return null;
        ipoteze.Add(operand.PerioadaDeschisa);
        ipoteze.Add(operand.VersiunePolitica);
        return new N.Declaratie(doc.Id, doc.DataInregistrare, miscari, decizii, ipoteze);
    }

    static void Laturile(DocumentFapt doc, ICollection<N.Refuz> refuzuri) {
        if (!EParte(doc.Predator.Fel))
            refuzuri.Add(new N.Refuz(CoduriRefuz.PredatorNepotrivit,
                "Predatorul unui document de trezorerie e un cont propriu, un partener sau un angajat.", null));
        if (!EParte(doc.Primitor.Fel))
            refuzuri.Add(new N.Refuz(CoduriRefuz.PrimitorNepotrivit,
                "Primitorul unui document de trezorerie e un cont propriu, un partener sau un angajat.", null));
        if (!EContPropriu(doc.Predator.Fel) && !EContPropriu(doc.Primitor.Fel))
            refuzuri.Add(new N.Refuz(CoduriRefuz.ContPropriuLipsa,
                "Una dintre laturi e contul propriu (casă/bancă) din care sau în care se mișcă banii.", null));
        // B-r2: tipul declară pe CARE latură stă contul propriu (plata predător,
        // încasarea primitor); tipul care nu declară lasă alegerea deschisă.
        else if (doc.LaturaContPropriu is LaturaDocument latura
                && !EContPropriu(latura == LaturaDocument.Predator ? doc.Predator.Fel : doc.Primitor.Fel))
            refuzuri.Add(new N.Refuz(CoduriRefuz.LaturaContPropriuNepotrivita,
                $"Tipul cere contul propriu pe latura {latura}, dar acolo stă "
                + $"{(latura == LaturaDocument.Predator ? doc.Predator.Fel : doc.Primitor.Fel)}.", null));
        if (doc.Predator.Id == doc.Primitor.Id)
            refuzuri.Add(new N.Refuz(CoduriRefuz.LaturiIdentice,
                "Predatorul și primitorul sunt același repartitor.", null));
    }

    static void Liniile(Operand operand, bool esteVirament, ICollection<N.Refuz> refuzuri) {
        if (operand.Linii.Count == 0) {
            refuzuri.Add(new N.Refuz(CoduriRefuz.LiniiLipsa,
                "Documentul de trezorerie se cere cu cel puțin o linie de defalcare.", null));
            return;
        }
        var deVirament = 0;
        foreach (var linie in operand.Linii) {
            if (linie.Valoare <= 0m)
                refuzuri.Add(new N.Refuz(CoduriRefuz.ValoareNepozitiva,
                    "Fiecare linie poartă o valoare pozitivă (defalcarea sumei).", linie.Id));
            if (linie.Natura == NaturaClasa.Virament)
                deVirament++;
        }
        if (esteVirament ? deVirament != operand.Linii.Count : deVirament > 0)
            refuzuri.Add(new N.Refuz(CoduriRefuz.ViramentMixt,
                "Cuplajul laturi ↔ natură: viramentul intern cere ambele laturi conturi proprii ȘI toate "
                + "liniile de natură Virament.", null));
    }

    // B-D8 pct. 9: gestiunea stă pe piciorul PROPRIU — cel al cărui cont îl ia
    // regula de la repartitorul intern; piciorul de terț rămâne fără ea. La
    // virament ambele picioare sunt ale aceluiași cont propriu, iar regula îl
    // numește prin latura care nu e contul de tranzit (`GetContPropriuId` de azi).
    static (Guid? Debit, Guid? Credit) Gestiuni(DocumentFapt doc, RegulaContareFapt regula, bool esteVirament) {
        var peDebit = Numit(doc, regula.SursaContDebit);
        var peCredit = Numit(doc, regula.SursaContCredit);
        if (esteVirament) {
            var propriu = peDebit is null ? peCredit : peCredit is null ? peDebit : null;
            return (propriu, propriu);
        }
        var intern = EContPropriu(doc.Predator.Fel) ? doc.Predator.Id
            : EContPropriu(doc.Primitor.Fel) ? doc.Primitor.Id
            : (Guid?)null;
        return intern is null ? (null, null)
            : peDebit == intern ? (intern, null)
            : peCredit == intern ? (null, intern)
            : (null, null);
    }

    static Guid? Numit(DocumentFapt doc, SursaCont sursa) => sursa switch {
        SursaCont.RepartitorPredator => doc.Predator.Id,
        SursaCont.RepartitorPrimitor => doc.Primitor.Id,
        _ => null,
    };

    static bool EContPropriu(FelRepartitor? fel) => fel == FelRepartitor.ContPropriu;

    static bool ETert(FelRepartitor? fel) => fel is FelRepartitor.Partener or FelRepartitor.Angajat;

    static bool EParte(FelRepartitor? fel) => EContPropriu(fel) || ETert(fel);
}
