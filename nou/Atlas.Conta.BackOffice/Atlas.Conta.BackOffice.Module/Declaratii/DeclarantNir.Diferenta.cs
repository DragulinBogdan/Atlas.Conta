#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed partial class DeclarantNir {
    public const string Structura = CoduriRefuz.NirDeltaStructura;
    public const string CauzaInvalida = CoduriRefuz.NirDeltaCauza;
    public const string PoliticaLipsa = CoduriRefuz.NirDeltaPolitica;

    static N.Declaratie? DeclaraDiferenta(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        var sursa = operand.ReceptieSursa!;
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var consumate = new List<N.VersiunePolitica>();
        var partide = new HashSet<Guid>();
        foreach (var original in sursa.Linii)
            if (sursa.Constatari.Values.Count(c => c.LinieSursa == original.Linie) != 1)
                refuzuri.Add(new(Structura, "Fiecare linie recepționată de factură trebuie păstrată o singură dată.", original.Linie));
        foreach (var linie in operand.Linii) {
            var initial = refuzuri.Count;
            void Refuza(string cod, string mesaj) => refuzuri.Add(new(cod, mesaj, linie.Id));
            var constatare = sursa.Constatari[linie.Id];
            var original = sursa.Linii.SingleOrDefault(l => l.Linie == constatare.LinieSursa);
            if (constatare.LinieSursa != null && original == null) {
                Refuza(Structura, "Linia-sursă nu aparține recepției facturii."); continue;
            }
            if (linie.Lot is not { } lot || linie.Natura != NaturaClasa.Stoc) {
                Refuza(Structura, "Constatarea cere un lot de stoc."); continue;
            }
            if (lot.GestiuneId != doc.Primitor.Id || lot.TipMaterialId != linie.TipMaterialId
                    || linie.ProdusId is Guid produs && produs != lot.ProdusId
                    || linie.Cantitate < 0)
                Refuza(Structura, "Cantitatea, produsul, tipul și gestiunea trebuie să corespundă lotului.");
            N.Capat stoc;
            decimal q, v;
            if (original != null) {
                stoc = original.Stoc;
                if (stoc.Unitate?.Id != lot.Id || stoc.Produs != lot.ProdusId
                        || original.TipMaterial != linie.TipMaterialId || stoc.Gestiune != doc.Primitor.Id)
                    Refuza(Structura, "Lotul recepționat se corectează pe factură, nu prin reidentificarea conexului.");
                q = linie.Cantitate - original.Cantitate;
                v = (q == 0 ? original.Valoare : rotunjire.Bani(original.Valoare * linie.Cantitate / original.Cantitate))
                    - original.Valoare;
            }
            else {
                if (lot.LinieIntrareId != linie.Id || linie.Cantitate <= 0 || linie.PretUnitar is not > 0)
                    Refuza(Structura, "Linia adăugată cere lot propriu, cantitate și preț pozitive.");
                var contare = Contari.Rezolva(operand, linie, refuzuri);
                if (contare is not { } cont || cont.ContDebit != lot.ContImplicitId) {
                    Refuza(Structura, "Plusul cere contul lotului."); continue;
                }
                consumate.Add(PoliticiConsumate.Versiunea(cont.Regula));
                stoc = new N.Capat { Cont = cont.ContDebit, Gestiune = doc.Primitor.Id, Produs = lot.ProdusId,
                    Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, cont.ContDebit, null, lot.ProdusId, lot.Data),
                    Analiza = Contari.Analiza(linie.Analiza, cont.Regula.OverrideDebit, cont.Regula.Comun) };
                q = linie.Cantitate;
                v = rotunjire.Bani(linie.PretUnitar.GetValueOrDefault() * q);
            }
            if (refuzuri.Count != initial || q == 0 && v == 0) continue;
            var cauza = constatare.Cauza ?? (q < 0 ? CauzaDiferentei.InClarificare : CauzaDiferentei.Plus);
            if (!sursa.CauzePermise.Contains(cauza) || (q > 0) != (cauza == CauzaDiferentei.Plus)) {
                Refuza(CauzaInvalida, "Cauza diferenței nu este permisă pe acest sens."); continue;
            }
            var politici = sursa.Politici.Where(p => p.Clasa == linie.ClasaId && p.Cauza == cauza).ToList();
            if (politici.Count != 1) { Refuza(PoliticaLipsa, "Cauza și clasa cer o singură politică de diferență."); continue; }
            Guid? partener = cauza == CauzaDiferentei.Imputabila ? constatare.Imputat
                : cauza == CauzaDiferentei.Plus ? doc.Predator.Id : null;
            var tert = partener is Guid id ? operand.Repartitori.GetValueOrDefault(id) : null;
            if (cauza == CauzaDiferentei.Imputabila && tert?.Fel is not (FelRepartitor.Partener or FelRepartitor.Angajat)) {
                Refuza(CauzaInvalida, "Lipsa imputabilă cere furnizorul, transportatorul sau angajatul pe linie."); continue;
            }
            var politica = politici[0];
            consumate.Add(PoliticiConsumate.Versiunea(politica));
            var contDiferenta = tert?.Fel == FelRepartitor.Angajat ? politica.ContPersonal ?? politica.Cont : politica.Cont;
            if (!operand.Conturi.ContainsKey(contDiferenta)
                    || cauza == CauzaDiferentei.Imputabila && !Partide.Urmareste(operand, contDiferenta)) {
                Refuza(PoliticaLipsa, "Contul diferenței trebuie să existe; imputarea cere urmărire pe partide."); continue;
            }
            N.Unitate? partida = partener is Guid titular && Partide.Urmareste(operand, contDiferenta)
                ? Partide.Proprie(operand, contDiferenta, titular) : null;
            if (partida != null && partide.Add(partida.Id)) decizii.Add(new N.PartidaDeschisa(linie.Id, partida));
            var a = stoc.Analiza;
            var analiza = Contari.Analiza(linie.Analiza, new Dimensiuni {
                CodFunctionalId = a.CodFunctional, CodEconomicId = a.CodEconomic, SursaFinantareId = a.SursaFinantare,
                UnitateId = a.UnitateOrganizatorica, ProiectId = a.Proiect, CentruCostId = a.CentruCost }, null);
            var diferenta = new N.Capat { Cont = contDiferenta, Gestiune = N.GestiuniVirtuale.Inventar,
                Produs = lot.ProdusId, Partener = partener, Unitate = partida, Analiza = analiza };
            miscari.Add(q < 0 ? new(stoc, diferenta, -q, 0, -v, new(doc.Id, linie.Id))
                : new(diferenta, stoc, q, 0, v, new(doc.Id, linie.Id)));
            decizii.Add(new N.ContRezolvat(linie.Id, contDiferenta, $"Diferență: {cauza}"));
            if (q < 0)
                decizii.Add(new N.ValoareDeclarata(linie.Id, stoc.Unitate!, -q, -v, SurseValoare.Receptie));
        }
        return refuzuri.Count > 0 || miscari.Count == 0 ? null : new(doc.Id, doc.DataInregistrare, miscari, decizii,
            PoliticiConsumate.Ipoteze(operand, decizii, [.. consumate]));
    }
}
