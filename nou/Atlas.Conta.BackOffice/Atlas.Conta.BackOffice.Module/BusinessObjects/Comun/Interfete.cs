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

// Linia care culege un PREȚ UNITAR — baza calculului de TVA la culegere
// (GATE XAF D5). Lanțul de valori trăiește pe derivate (testul bazei §3), dar
// capătul lui de intrare are aceeași formă pe toate liniile care se culeg
// financiar: baza = PretUnitar × Cantitate. Contract read-only: seam-ul de
// recalcul (TvaService.CalculeazaLaCulegere) doar CITEȘTE prețul; scrierea
// rămâne a proprietății virtuale de pe derivată.
// Implementat de FacturaIntrareDetaliu, FacturaIesireDetaliu (cele două ecrane
// ale gate-ului) și — din felia 8 (F8-D2) — DecontDetaliu: aderarea lui a fost
// pură declarație (`PretUnitar` exista din 3a), iar seam-ul de calcul la
// culegere e apelat de `DecontApply`. Tot acolo se face vizibilă și
// normalizarea cantității pro-forma 0→1 (32d) — „ce culegerea ar trebui să facă
// vizibil, nu în spate"; `Decont.PregatesteOperare` o păstrează, idempotentă,
// pentru calea XAF/import. În XAF nimic nu s-a schimbat: controllerul de
// recalcul se abonează pe TIPUL DOCUMENTULUI (FCT/FCL), nu pe interfața liniei.
public interface ILinieCuPretUnitar {
    decimal PretUnitar { get; }
}

// Linia care NAȘTE un lot la culegere (F5-D2): produsul ales de operator devine
// identitatea lotului nou, iar `LoturiCulegereService` face nașterea/
// sincronizarea/curățenia pe contractul ăsta — o singură logică pentru toate
// tipurile de INTRARE culese manual (decizia 25c: baza nu poartă ProdusId,
// produsul e caracteristică de frunză). Gestiunea lotului vine din hook-ul
// polimorf `Document.GestiuneLoturiCulese`.
//
// Numele spune INTENȚIA, nu forma. `FacturaIesireDetaliu` și
// `DescarcareGestiuneDetaliu` au și ele `ProdusId`, dar cu semantică OPUSĂ:
// acolo produsul e criteriul de PICKING dintr-un lot EXISTENT (decizia 37d,
// „General! + Specific?"). Dacă ar declara interfața, fiecare culegere de FCL
// ar naște loturi fantomă — marfă inventată în stoc, în loc de marfă aleasă
// din el. Interdicția e load-bearing: nu se declară pe ieșiri.
// Implementat de FacturaIntrareDetaliu, NirDetaliu și ListaDiferenteInventarDetaliu.
public interface ILinieCareNasteLot {
    Guid? ProdusId { get; set; }
    Produs Produs { get; set; }

    // F6-D3: linia care poartă o DIRECȚIE poate refuza nașterea. Pe LDI doar
    // PLUSUL naște lot (28d); minusul pinuiește un lot existent, iar un produs
    // rămas cules pe el ar naște lot-artefact pe draft. Default `true` — FCT și
    // NIR nasc necondiționat și nu se ating.
    bool NasteLot => true;
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

// Opt-in-ul DOCUMENTULUI pentru postarea fără regulă (review advers 1C-a):
// mecanismul 32a extins — o linie cu postare explicită COMPLETĂ postează și în
// absența oricărei RegulaContare — e valid DOAR pe tipurile care îl declară
// (NotaContabila și derivatele ei). Fără marker, o linie străină cu conturi
// explicite atașată programatic unui tip fără reguli (BTR, NIR, ASM) ar
// injecta note arbitrare — motorul o sare, ca înainte de extensie.
public interface IDocumentCuPostareExplicita { }
