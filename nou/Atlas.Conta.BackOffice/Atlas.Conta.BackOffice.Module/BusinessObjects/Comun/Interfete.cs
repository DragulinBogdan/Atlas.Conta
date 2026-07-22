namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// Contracte pe derivate (testul bazei §7): câmpuri care apar pe mai multe tipuri
// dar nu trec testul deciziei 2 — nu intră în bază.

public interface IDocumentCuScadenta {
    DateOnly? DataScadenta { get; set; }
}

public interface IDocumentCuPV {
    string NumarPV { get; set; }
    DateOnly? DataPV { get; set; }
}

// Linia de intrare care culege atribute de lot (testul bazei §2: DATA_EXPIRARE
// și LOT_FABRICATIE aparțin Lotului); motorul le copiază pe Lot la finalizarea
// lui (operare) — contract de bază, ca să nu cunoască derivatele.
public interface ILinieCuAtributeLot {
    DateOnly? DataExpirare { get; set; }
    string LotFabricatie { get; set; }
}

// Trăsătura PROPRIE a Decontului (inventar 06, nuanța deciziei 15): linia
// poartă postarea explicită — cont și repartitor, per latură — ca date de
// primă clasă. Motorul o consultă înaintea rezolvării declarative (SursaCont)
// și a default-urilor de dimensiuni; NU e mecanism generic de override —
// doar tipurile care declară interfața o au.
public interface ILinieCuPostareExplicita {
    Guid? ContDebitId { get; }
    Guid? ContCreditId { get; }
    Guid? RepartitorDebitId { get; }
    Guid? RepartitorCreditId { get; }
}
