#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed partial class DeclarantNir : IDeclarant {
    public static readonly DeclarantNir Instanta = new();
    public string SursaValoareDeclarata => SurseValoare.Receptie;
    DeclarantNir() { }
    public bool ConteazaPrinReguli => true;
    public bool PermiteDeclaratieFaraMiscari(Operand operand) => operand.ReceptieSursa is not null;

    public N.Declaratie? Declara(Operand operand, N.Rotunjire rotunjire, ICollection<N.Refuz> refuzuri) {
        ArgumentNullException.ThrowIfNull(operand);
        ArgumentNullException.ThrowIfNull(rotunjire);
        ArgumentNullException.ThrowIfNull(refuzuri);
        if (operand.ReceptieSursa is not null) return DeclaraDiferenta(operand, rotunjire, refuzuri);
        var doc = operand.Document;
        var miscari = new List<N.Miscare>();
        var decizii = new List<N.Decizie>();
        var partide = new Dictionary<Guid, N.Unitate>();
        if (operand.Linii.Count == 0)
            refuzuri.Add(new(CoduriRefuz.LiniiLipsa, "Recepția cere cel puțin o linie.", null));
        foreach (var linie in operand.Linii) {
            var initial = refuzuri.Count;
            void Refuza(string cod, string mesaj) => refuzuri.Add(new(cod, mesaj, linie.Id));
            if (linie.Cantitate <= 0) Refuza(CoduriRefuz.CantitateNepozitiva, "Cantitatea recepționată trebuie să fie pozitivă.");
            if (linie.Natura != NaturaClasa.Stoc) Refuza(CoduriRefuz.NaturaNepotrivita, "Recepția cere natura Stoc.");
            if (linie.Lot is not { } lot) { Refuza(CoduriRefuz.LotLipsa, "Recepția cere lot."); continue; }
            if (lot.GestiuneId != doc.Primitor.Id)
                Refuza(CoduriRefuz.ReceptieStructuraInvalida, "Lotul trebuie să aparțină gestiunii primitoare.");
            if (lot.TipMaterialId != linie.TipMaterialId || linie.TipProdusCulesId is Guid tip && tip != linie.TipMaterialId
                    || linie.ProdusId is Guid produs && produs != lot.ProdusId)
                Refuza(CoduriRefuz.ProdusAltTip, "Produsul și tipul liniei trebuie să corespundă lotului.");
            var propriu = lot.LinieIntrareId == linie.Id;
            if (propriu && linie.PretUnitar is <= 0)
                Refuza(CoduriRefuz.ValoareNepozitiva, "Prețul de recepție trebuie să fie pozitiv.");
            var valoare = propriu
                ? linie.PretUnitar is decimal pret ? rotunjire.Bani(pret * linie.Cantitate) : linie.Valoare
                : rotunjire.Bani(lot.PretUnitar * linie.Cantitate);
            if (valoare < 0) Refuza(CoduriRefuz.ValoareNepozitiva, "Valoarea recepției nu poate fi negativă.");
            var contare = Contari.Rezolva(operand, linie, refuzuri);
            if (refuzuri.Count != initial) continue;
            var cont = contare!.Value;
            if (cont.ContDebit != lot.ContImplicitId) {
                Refuza(CoduriRefuz.ContStocLipsa, "Contul de recepție trebuie să fie contul lotului."); continue;
            }
            Partide.Numeste(operand, cont.ContCredit, doc.Predator.Id, linie.Id, partide, decizii);
            var externCapat = Terti.Capat(operand, new N.Capat {
                Cont = cont.ContCredit, Gestiune = N.GestiuniVirtuale.Furnizor, Produs = lot.ProdusId,
                Analiza = Contari.Analiza(linie.Analiza, cont.Regula.OverrideCredit, cont.Regula.Comun),
            }, doc.Predator.Id, partide);
            var internCapat = new N.Capat {
                Cont = cont.ContDebit, Gestiune = doc.Primitor.Id, Produs = lot.ProdusId,
                Unitate = new N.Unitate(lot.Id, N.FelUnitate.Lot, cont.ContDebit, null, lot.ProdusId, lot.Data),
                Analiza = Contari.Analiza(linie.Analiza, cont.Regula.OverrideDebit, cont.Regula.Comun),
            };
            miscari.Add(new(externCapat, internCapat, linie.Cantitate, 0, valoare, new(doc.Id, linie.Id)));
            decizii.Add(new N.ContRezolvat(linie.Id, cont.ContDebit, cont.SursaDebit.ToString()));
            decizii.Add(new N.ContRezolvat(linie.Id, cont.ContCredit, cont.SursaCredit.ToString()));
        }
        return refuzuri.Count > 0 ? null : new(doc.Id, doc.DataInregistrare, miscari, decizii,
            [operand.PerioadaDeschisa, operand.VersiunePolitica]);
    }
}
