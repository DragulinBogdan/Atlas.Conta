namespace Atlas.Conta.BackOffice.Module.Api.Politici;

// Felia 24 (F24-D6) — EXPLICAȚIA configurației pe o linie IPOTETICĂ, pe sârmă.
//
// Ce e: raportul funcțiilor pure din `Motor/Potrivire.cs` pe fapte fabricate din
// parametrii cererii. Nu e planul unui DOCUMENT (acela e pasul 5 al contractului
// de izolare a motorului) — de aceea nimic de aici nu are `DocumentId` și nimic
// nu atinge registrele.
//
// Trei convenții, aceleași ca pe restul feliilor:
//   * enum-urile pleacă STRING (numele membrului, 57a/63); eticheta lizibilă e
//     în `metadata.json`, o singură sursă pentru XAF și React;
//   * rândurile poartă coduri și simboluri, nu `Guid`-uri de dereferențiat —
//     explicația se CITEȘTE (`Id` rămâne doar ca ancoră pentru „deschide rândul");
//   * fiecare rând poartă `DinSeed`, ca în grilele de politică („seed / manual").
//
// Zero calcul în TS (42c): `Concluzie` e formată pe server, fiindcă e aceeași
// frază pe care ar trebui s-o inventeze clientul din patru câmpuri.

/// <summary>Contul rezolvat al unei laturi, cu sursa care l-a dat efectiv.</summary>
public sealed class ContRezolvatDto {
    // `null` = sursa declarată n-a rezolvat și nici contul explicit nu există:
    // pe un document real, aici motorul refuză operarea cu mesaj.
    public string Simbol { get; set; }
    public string Denumire { get; set; }
    // Numele membrului `SursaRezolvata`.
    public string Sursa { get; set; }
}

public sealed class RegulaContareRandDto {
    public Guid Id { get; set; }
    public string TipMaterial { get; set; }
    public string NaturaFiltru { get; set; }
    public int? SemnFiltru { get; set; }
    public bool PastreazaSemn { get; set; }
    public string SursaContDebit { get; set; }
    public string ContDebit { get; set; }
    public string SursaContCredit { get; set; }
    public string ContCredit { get; set; }
    public bool DinSeed { get; set; }
}

public sealed class CandidatContareDto {
    public RegulaContareRandDto Regula { get; set; }
    // Numele membrului `MotivEliminare`; `null` = rândul CÂȘTIGĂTOR.
    public string Motiv { get; set; }
}

public sealed class ExplicaContareDto {
    public RegulaContareRandDto Castigator { get; set; }
    // Numele membrului `NivelContare`.
    public string Nivel { get; set; }
    public CandidatContareDto[] Candidati { get; set; }
    public ContRezolvatDto ContDebit { get; set; }
    public ContRezolvatDto ContCredit { get; set; }
    // Tipul documentului declară `IDocumentCuPostareExplicita` (NTC): conturile
    // CULESE pe linie bat regula, iar pe un tip fără reguli ele sunt singura
    // postare (32a extins). Explicația NU calculează contul — nu e o linie
    // reală, deci n-are ce cont cules să arate.
    public bool PostareExplicita { get; set; }
    public string Concluzie { get; set; }
}

public sealed class RegulaStocRandDto {
    public Guid Id { get; set; }
    public string Clasa { get; set; }
    public string TipStoc { get; set; }
    public int Semn { get; set; }
    public bool DinSeed { get; set; }
}

public sealed class ExplicaStocDto {
    // Numele membrului `LaturaDocument`.
    public string Latura { get; set; }
    // Numele membrului `NivelStoc`.
    public string Nivel { get; set; }
    public RegulaStocRandDto[] Reguli { get; set; }
    // Numele membrului `MotivStoc`; `null` când latura chiar scrie în registru.
    public string Motiv { get; set; }
    public string Concluzie { get; set; }
}

public sealed class ExplicaTvaDto {
    // `null` = tipul n-are `PoliticaTva`, deci pasul TVA din motor nici nu rulează.
    public Guid? Id { get; set; }
    public string Directie { get; set; }
    public string SursaContrapartida { get; set; }
    public string ContrapartidaFallback { get; set; }
    public ContRezolvatDto Contrapartida { get; set; }
    public bool DinSeed { get; set; }
    public string Concluzie { get; set; }
}

public sealed class ExplicaConexDto {
    // `null` = tipul nu generează niciun document conex.
    public Guid? Id { get; set; }
    public string Tinta { get; set; }
    public string TintaDenumire { get; set; }
    public bool InverseazaLaturi { get; set; }
    public string NaturaFiltru { get; set; }
    // Linia ipotetică ar trece filtrul de natură al politicii.
    public bool Trece { get; set; }
    public bool DinSeed { get; set; }
    public string Concluzie { get; set; }
}

public sealed class RandTvaImplicitDto {
    public Guid Id { get; set; }
    public string ClasaFiscala { get; set; }
    public DateOnly? ValabilDeLa { get; set; }
    public string TipTva { get; set; }
    public bool DinSeed { get; set; }
}

public sealed class CandidatTvaImplicitDto {
    public RandTvaImplicitDto Rand { get; set; }
    public string Motiv { get; set; }
}

public sealed class ExplicaImplicitDto {
    public Guid? TipTvaId { get; set; }
    public string TipTva { get; set; }
    // Numele membrului `SursaImplicit`.
    public string Sursa { get; set; }
    // Fraza gata formată de `ImpliciteService` (notele + verdictul).
    public string Motiv { get; set; }
    public RandTvaImplicitDto RandPolitica { get; set; }
    public CandidatTvaImplicitDto[] Candidati { get; set; }
    public string Concluzie { get; set; }
}

public sealed class ExplicaValidareDto {
    public Guid Id { get; set; }
    public bool CereClasificatieBugetara { get; set; }
    public string NaturaInterzisa { get; set; }
    public bool DinSeed { get; set; }
}

public sealed class ExplicaScadentaDto {
    public Guid Id { get; set; }
    public int ZileDefault { get; set; }
    public bool DinSeed { get; set; }
}

public sealed class ExplicaNumerotareDto {
    public Guid Id { get; set; }
    public string Serie { get; set; }
    public string Format { get; set; }
    public int UrmatorulNumar { get; set; }
    public bool DinSeed { get; set; }
}

/// <summary>Ce s-ar întâmpla cu o linie ca aceasta, mecanism cu mecanism.</summary>
public sealed class ExplicatieDto {
    public string TipDocument { get; set; }
    public string TipDocumentDenumire { get; set; }
    public string TipMaterial { get; set; }
    public string TipMaterialDenumire { get; set; }
    public string Clasa { get; set; }
    // Numele membrului `NaturaClasa`; `null` = Tipul n-are clasă în nomenclator.
    public string Natura { get; set; }
    public int Semn { get; set; }
    public DateOnly Data { get; set; }
    public string Predator { get; set; }
    public string Primitor { get; set; }
    public string Partener { get; set; }
    public string Produs { get; set; }

    public ExplicaContareDto Contare { get; set; }
    public ExplicaStocDto[] Stoc { get; set; }
    public ExplicaTvaDto Tva { get; set; }
    public ExplicaConexDto Conex { get; set; }
    public ExplicaImplicitDto Implicit { get; set; }
    // Cele trei rânduri simple ale tipului: `null` = tipul n-are rândul.
    public ExplicaValidareDto Validare { get; set; }
    public ExplicaScadentaDto Scadenta { get; set; }
    public ExplicaNumerotareDto Numerotare { get; set; }
}

/// <summary>Cererea, cu FK-urile deja rezolvate pe ușa securizată de controller.</summary>
public sealed record ExplicaCerere(Guid TipDocumentId, Guid TipMaterialId, int Semn, DateOnly Data,
    Guid? PredatorId, Guid? PrimitorId, Guid? PartenerId, Guid? ProdusId);
