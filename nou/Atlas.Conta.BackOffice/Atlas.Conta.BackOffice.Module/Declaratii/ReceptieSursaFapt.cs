#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Declaratii;

public sealed record LinieReceptieSursa(Guid Linie, Guid TipMaterial, N.Capat Stoc, decimal Cantitate, decimal Valoare);
public sealed record ConstatareReceptie(Guid? LinieSursa, CauzaDiferentei? Cauza, Guid? Imputat);
public sealed record PoliticaDiferentaFapt(Guid Clasa, CauzaDiferentei Cauza, Guid Cont, Guid? ContPersonal);
public sealed record ReceptieSursaFapt(Guid Document, Guid Tranzactie,
    IReadOnlyList<LinieReceptieSursa> Linii,
    IReadOnlyDictionary<Guid, ConstatareReceptie> Constatari,
    IReadOnlyList<PoliticaDiferentaFapt> Politici,
    IReadOnlySet<CauzaDiferentei> CauzePermise);
