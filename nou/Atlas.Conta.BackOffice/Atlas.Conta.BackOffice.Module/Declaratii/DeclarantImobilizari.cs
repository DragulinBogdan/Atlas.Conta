#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed class DeclarantImobilizari : IDeclarant {
    public static readonly DeclarantImobilizari Instanta = new();
    DeclarantImobilizari() { }

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        var miscari = new List<N.Miscare>();
        var mutari = new List<N.Mutare>();
        var folosit = new Dictionary<N.ReferintaPostare, decimal>();
        var peDimensiuni = new Dictionary<(N.Capat, N.Latura), decimal>();
        var iesiriFiscale = new HashSet<Guid>();
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Documentul cere cel puțin o fișă.", null));
        foreach (var linie in operand.Linii) {
            var cules = linie.Imobilizare;
            if (cules == null || !operand.Fise.TryGetValue(cules.Fisa, out var fisa)) {
                refuzuri.Add(new(CoduriRefuz.FisaLipsa, "Fișa liniei lipsește.", linie.Id));
                continue;
            }
            var politicaLipsa = cules switch {
                PifCules p => (p.AmortizareInitiala != 0m || p.AmortizareFiscalaInitiala != 0m)
                    && fisa.ContAmortizare == Guid.Empty,
                AmoCules => fisa.ContAmortizare == Guid.Empty || fisa.ContCheltuiala == Guid.Empty,
                CasCules => fisa.ContAmortizare == Guid.Empty || fisa.ContCedare == Guid.Empty,
                _ => true,
            };
            if (fisa.Cont == Guid.Empty || politicaLipsa) {
                refuzuri.Add(new(CoduriRefuz.PoliticaAmortizareLipsa,
                    "Fișa cere conturile politicii folosite de operație.", linie.Id));
                continue;
            }
            var cauza = new N.Cauza(operand.Document.Id, linie.Id);
            N.Capat Capat(Guid cont, N.Carte carte, bool nominalizat) => new() {
                Cont = cont, Carte = carte, Gestiune = fisa.Loc, Analiza = linie.Analiza,
                Unitate = nominalizat ? new(fisa.Id, N.FelUnitate.Fisa, cont, null, null, fisa.Deschisa) : null,
            };
            void Nota(Guid debit, bool fisaDebit, Guid credit, bool fisaCredit, decimal suma, N.Carte carte) {
                if (suma != 0m)
                    miscari.Add(new(Capat(credit, carte, fisaCredit), Capat(debit, carte, fisaDebit),
                        0m, 0m, suma, cauza));
            }
            switch (cules) {
                case PifCules pif:
                    if (pif.Fel == FelLiniePif.Revizuire) {
                        var cap = Capat(fisa.Cont, N.Carte.Contabil, true);
                        mutari.Add(new(cap, cap, N.Latura.Debit, 0m, 0m, 0m, cauza));
                        break;
                    }
                    if (linie.Valoare <= 0m || pif.Fiscal < 0m || pif.AmortizareInitiala < 0m
                            || pif.AmortizareFiscalaInitiala < 0m) {
                        refuzuri.Add(new(CoduriRefuz.ValoareNepozitiva, "Valorile intrării nu sunt valide.", linie.Id));
                        break;
                    }
                    Aloca(fisa.Cont, N.Latura.Debit, linie.Valoare, pif.LinieSursa);
                    Aloca(fisa.ContAmortizare, N.Latura.Credit, pif.AmortizareInitiala, null);
                    Nota(fisa.Cont, true, fisa.Cont, false, pif.Fiscal, N.Carte.Fiscal);
                    Nota(fisa.ContAmortizare, false, fisa.ContAmortizare, true,
                        pif.AmortizareFiscalaInitiala, N.Carte.Fiscal);
                    break;
                case AmoCules amo:
                    Nota(fisa.ContCheltuiala, false, fisa.ContAmortizare, true, linie.Valoare, N.Carte.Contabil);
                    Nota(fisa.ContCheltuiala, false, fisa.ContAmortizare, true, amo.Fiscal, N.Carte.Fiscal);
                    break;
                case CasCules cas:
                    Nota(cas.Fel == FelLinieIesire.AmortizareCumulata ? fisa.ContAmortizare : fisa.ContCedare,
                        cas.Fel == FelLinieIesire.AmortizareCumulata, fisa.Cont, true, linie.Valoare, N.Carte.Contabil);
                    if (iesiriFiscale.Add(fisa.Id)) {
                        Nota(fisa.ContAmortizare, true, fisa.Cont, true, fisa.AmortizareFiscala, N.Carte.Fiscal);
                        Nota(fisa.ContCedare, false, fisa.Cont, true,
                            fisa.BrutFiscal - fisa.AmortizareFiscala, N.Carte.Fiscal);
                    }
                    break;
            }

            void Aloca(Guid cont, N.Latura latura, decimal suma, Guid? sursa) {
                var ramas = suma;
                foreach (var suport in operand.Suporturi.Where(s => s.Capat.Cont == cont && s.Latura == latura
                        && (sursa == null || s.Linie == sursa)).OrderBy(s => s.Data).ThenBy(s => s.Referinta.Id)) {
                    var plafon = operand.DisponibilNominalizare.FirstOrDefault(d => d.Capat == suport.Capat);
                    var disponibil = latura == N.Latura.Debit ? plafon?.DebitNetMinim : plafon?.CreditNetMinim;
                    var cheie = (suport.Capat, latura);
                    var ia = Math.Min(ramas, Math.Min(suport.Disponibil - folosit.GetValueOrDefault(suport.Referinta),
                        (disponibil ?? 0m) - peDimensiuni.GetValueOrDefault(cheie)));
                    if (ia <= 0m) continue;
                    var nominalizat = suport.Capat with {
                        Unitate = new(fisa.Id, N.FelUnitate.Fisa, cont, null, null, fisa.Deschisa),
                    };
                    mutari.Add(new(suport.Capat, nominalizat, latura, 0m, 0m, ia, cauza) { Suport = suport.Referinta });
                    folosit[suport.Referinta] = folosit.GetValueOrDefault(suport.Referinta) + ia;
                    peDimensiuni[cheie] = peDimensiuni.GetValueOrDefault(cheie) + ia;
                    ramas -= ia;
                    if (ramas == 0m) break;
                }
                if (ramas > 0m)
                    refuzuri.Add(new(CoduriRefuz.SuportInsuficient,
                        $"Suport contabil nenominalizat insuficient pe contul {cont}: cerut {suma}, lipsă {ramas}.", linie.Id));
            }
        }
        return refuzuri.Count > 0 ? null : new(operand.Document.Id, operand.Document.DataInregistrare,
            miscari, mutari, [], [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }
}
