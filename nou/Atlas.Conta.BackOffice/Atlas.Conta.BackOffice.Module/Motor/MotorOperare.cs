using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

// Gardienii opresc operațiunea cu mesaj pentru utilizator (XAF o afișează curat).
public class OperareException : UserFriendlyException {
    public OperareException(string message) : base(message) { }
}

// Decizia 14: motorul generic de operare — consumă DOAR clasa de bază; ce e
// specific tipului intră prin hooks (PregatesteOperare/ValideazaOperare) și
// prin politici (RegulaStoc/RegulaContare). Fiecare metodă publică e o
// tranzacție: un singur CommitChanges la final.
//
// Limitare asumată (single-operator back-office): verificarea de sold și
// commit-ul nu sunt serializate între utilizatori concurenți; la nevoie se
// adaugă advisory lock Postgres per cheie de stoc — aditiv, doar aici.
public static class MotorOperare {
    // Rezultatul fazelor „calculează + validează" (33d) — tot ce materializarea
    // are nevoie, fără ca nimic să fi fost scris în registre. Extras ca să existe
    // O SINGURĂ cale de reguli pentru operare și pentru dry-run (`Valideaza`,
    // spike pasul 5 / D3): ordinea fazelor și comportamentul lui `Opereaza` sunt
    // NESCHIMBATE — codul e mutat, nu rescris.
    sealed class PlanOperare {
        public TipDocument TipDoc;
        public Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> ClaseTip;
        public List<(DocumentDetaliu Detaliu, RegulaStoc Regula, MiscareStoc Miscare)> Miscari;
        public List<(DocumentDetaliu Detaliu, Guid ContDebit, Guid ContCredit,
            decimal Valoare, Dimensiuni DimensiuniDebit, Dimensiuni DimensiuniCredit)> Note;
        // Felia 11 (JT-D1): faptele fiscale ale liniilor, derivate de
        // `RegistruTvaService` tot în faza de CALCUL — un refuz al oricărui
        // gardian nu lasă rânduri-fantomă în ObjectSpace-ul apelantului (33d).
        public List<RegistruTvaService.RandTva> RanduriTva;
    }

    // Dry-run-ul comenzii de operare (D3): rulează EXACT fazele de calcul și
    // validare ale lui `Opereaza` (gard stare/perioadă → PregatesteOperare →
    // validare declarativă + hook-ul tipului → mișcări de stoc + gardianul de
    // sold → note → pasul TVA → dimensiuni obligatorii) și se oprește ÎNAINTE de
    // materializare. Listă goală = documentul trece toți gardienii.
    //
    // ATENȚIE (contract de apelant): dry-run-ul NU e read-only pe ObjectSpace-ul
    // primit — `PregatesteOperare` scrie `Valoare`/`Cantitate` pe linii, iar
    // gardienii pot lăsa alte instanțe atinse. Nimic nu se comite aici, dar
    // apelantul trebuie să folosească un ObjectSpace PROPRIU, aruncat după apel
    // (calea vie: OS non-secured creat de adaptorul `OperareApi`).
    public static IReadOnlyList<string> Valideaza(IObjectSpace os, Document doc) {
        try {
            CalculeazaSiValideaza(os, doc);
            return Array.Empty<string>();
        }
        catch (OperareException ex) {
            return ex.Message
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();
        }
    }

    static PlanOperare CalculeazaSiValideaza(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException("Doar un document în starea Draft poate fi operat.");
        GardianPerioada.VerificaDeschisa(os, doc.Data);

        doc.PregatesteOperare(os);
        var tipDoc = GasesteTipDocument(os, doc);

        // Clasa/natura/contul fiecărui Tip de pe linii, preîncărcate — motorul nu
        // se bazează pe navigații (contextul apelant nu garantează lazy loading).
        var idsTip = doc.Detalii.Select(d => d.TipMaterialId).Distinct().ToList();
        var claseTip = os.GetObjectsQuery<TipMaterial>()
            .Where(t => idsTip.Contains(t.ID))
            .Select(t => new { t.ID, t.ClasaId, t.Clasa.Natura, t.Denumire, t.ContImplicitId })
            .ToDictionary(t => t.ID, t => (t.ClasaId, t.Natura, t.Denumire, t.ContImplicitId));

        // Obligativitățile per tip (PoliticaValidare — profil de validare, 3d)
        // rulează generic, alături de invariantele proprii tipului din hook.
        var erori = new List<string>();
        ValideazaDeclarativ(os, doc, tipDoc, claseTip, erori);
        doc.ValideazaOperare(os, erori);
        if (erori.Count > 0)
            throw new OperareException(string.Join("\n", erori));

        // 1. Mișcările de stoc se CALCULEAZĂ întâi (delta), gardianul de sold
        //    le verifică, abia apoi se materializează rândurile. Per latură,
        //    regula specifică pe Clasa liniei bate regula generică (Clasa=null =
        //    orice clasă cu Natura=Stoc) — altfel s-ar aplica amândouă.
        var reguliStoc = os.GetObjectsQuery<RegulaStoc>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        var miscari = new List<(DocumentDetaliu Detaliu, RegulaStoc Regula, MiscareStoc Miscare)>();
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            foreach (var latura in reguliStoc.GroupBy(r => r.Latura)) {
                var aplicabile = latura.Where(r => r.ClasaId != null && r.ClasaId == info.ClasaId).ToList();
                if (aplicabile.Count == 0 && info.Natura == NaturaClasa.Stoc)
                    aplicabile = latura.Where(r => r.ClasaId == null).ToList();
                foreach (var regula in aplicabile) {
                    if (d.LotId == null)
                        throw new OperareException(
                            $"Linia cu {info.Denumire} intră în regulile de stoc dar nu are lot.");
                    var repartitorId = regula.Latura == LaturaDocument.Predator ? doc.PredatorId : doc.PrimitorId;
                    miscari.Add((d, regula, new MiscareStoc(
                        new CheieStoc(d.LotId.Value, repartitorId, regula.TipStoc), doc.Data, regula.Semn * d.Cantitate)));
                }
            }
        }
        StocService.VerificaSoldIntermediar(os, miscari.Select(m => m.Miscare).ToList());

        // 2. Rândurile contabile se CALCULEAZĂ și se validează tot înainte de
        //    materializare: potrivirea regulii pe linie = TipMaterial exact →
        //    NaturaFiltru → regula generică; fără regulă = linia nu contează pe
        //    acest tip de document (NotaTransfer — 23c; liniile de stoc pe FCT —
        //    recepția contează pe NIR). Conturile se rezolvă din sursa declarată
        //    (TipMaterial / repartitorul unei laturi), cu contul explicit
        //    fallback. Toți gardienii (sold, cont nerezolvabil, dimensiuni
        //    obligatorii) refuză ÎNAINTE de primul rând creat — un refuz nu
        //    lasă nimic în ObjectSpace-ul apelantului.
        var reguliContare = os.GetObjectsQuery<RegulaContare>().Where(r => r.TipDocumentId == tipDoc.ID).ToList();
        var repartitorPredator = os.GetObjectByKey<Repartitor>(doc.PredatorId);
        var repartitorPrimitor = os.GetObjectByKey<Repartitor>(doc.PrimitorId);
        // Dimensiunea Material = Produsul lotului liniei (analitic de stoc) —
        // default de motor pe ambele laturi, ca repartitorul implicit al
        // header-ului; liniile fără lot rămân pe ce s-a cules.
        var idsLoturi = doc.Detalii.Where(d => d.LotId != null).Select(d => d.LotId.Value).Distinct().ToList();
        var produsPerLot = os.GetObjectsQuery<Lot>()
            .Where(l => idsLoturi.Contains(l.ID))
            .Select(l => new { l.ID, l.ProdusId })
            .ToDictionary(l => l.ID, l => l.ProdusId);
        var note = new List<(DocumentDetaliu Detaliu, Guid ContDebit, Guid ContCredit,
            decimal Valoare, Dimensiuni DimensiuniDebit, Dimensiuni DimensiuniCredit)>();
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            // Filtrul de semn (LDI): regula se aplică doar liniilor cu semnul
            // cerut; nepotrivirea scoate regula din joc la TOATE nivelurile de
            // specificitate (o linie de plus sare peste regula exactă de minus
            // și cade pe regula generică de plus).
            var semn = Math.Sign(d.Cantitate);
            var candidati = reguliContare.Where(r => r.SemnFiltru == null || r.SemnFiltru == semn).ToList();
            var regula = candidati.FirstOrDefault(r => r.TipMaterialId == d.TipMaterialId)
                ?? candidati.FirstOrDefault(r => r.TipMaterialId == null && r.NaturaFiltru == info.Natura)
                ?? candidati.FirstOrDefault(r => r.TipMaterialId == null && r.NaturaFiltru == null);
            // Postarea explicită pe linie (Decont — inventar 06): contul setat
            // pe linie bate rezolvarea declarativă; contract de interfață, nu
            // mecanism generic — doar tipurile care o declară o au.
            var explicita = d as ILinieCuPostareExplicita;
            // Mecanismul 32a EXTINS (NotaContabila — design FAZA 1C §5): postarea
            // explicită COMPLETĂ (ambele conturi) bate și ABSENȚA regulii — NTC
            // n-are nicio RegulaContare, fiecare linie își poartă nota. Extensia
            // e OPT-IN pe tipul DOCUMENTULUI (IDocumentCuPostareExplicita —
            // review advers 1C-a): o linie străină cu conturi explicite atașată
            // unui tip fără reguli (BTR/NIR/ASM) rămâne sărită, ca înainte —
            // altfel ar injecta note arbitrare sau dublă postare pe lanțul conex.
            // Tipurile CU reguli (Decont) rămân neschimbate: regula lor se
            // potrivește, iar contul explicit continuă să o bată punctual.
            if (regula == null && (doc is not IDocumentCuPostareExplicita
                    || explicita?.ContDebitId == null || explicita.ContCreditId == null))
                continue;
            // Când regula lipsește, conturile explicite sunt garantat nenule mai
            // sus, deci ramura de rezolvare declarativă nici nu se evaluează.
            var contDebit = explicita?.ContDebitId
                ?? RezolvaCont(regula.SursaContDebit, regula.ContDebitId,
                    info.ContImplicitId, repartitorPredator, repartitorPrimitor)
                ?? throw new OperareException(
                    $"Contul debitor nu se poate rezolva pentru linia cu {info.Denumire} ({tipDoc.Cod}, sursă {regula.SursaContDebit}).");
            var contCredit = explicita?.ContCreditId
                ?? RezolvaCont(regula.SursaContCredit, regula.ContCreditId,
                    info.ContImplicitId, repartitorPredator, repartitorPrimitor)
                ?? throw new OperareException(
                    $"Contul creditor nu se poate rezolva pentru linia cu {info.Denumire} ({tipDoc.Cod}, sursă {regula.SursaContCredit}).");
            // Repartitorul explicit al liniei (aceeași trăsătură) intră ca
            // nivel maxim; default-ul de capăt e polimorf (00 §5 pe bază,
            // Decont mută creditul pe titular) + Materialul din lot.
            var materialImplicit = d.LotId != null && produsPerLot.TryGetValue(d.LotId.Value, out var produsId)
                ? produsId : (Guid?)null;

            // Fără regulă (NTC) coalesce-ul sare peste nivelurile ei de
            // override/comun — Rezolva ignoră sursele null.
            var dimensiuniLinie = d.DimensiuniCulese();
            var dimensiuniDebit = DimensiuniResolver.Rezolva(
                new Dimensiuni { RepartitorId = explicita?.RepartitorDebitId },
                dimensiuniLinie, regula?.DimensiuniOverrideDebit(), regula?.DimensiuniComun(),
                new Dimensiuni { RepartitorId = doc.RepartitorImplicitDebit(os), MaterialId = materialImplicit });
            var dimensiuniCredit = DimensiuniResolver.Rezolva(
                new Dimensiuni { RepartitorId = explicita?.RepartitorCreditId },
                dimensiuniLinie, regula?.DimensiuniOverrideCredit(), regula?.DimensiuniComun(),
                new Dimensiuni { RepartitorId = doc.RepartitorImplicitCredit(os), MaterialId = materialImplicit });

            // Normalizarea cu semnul filtrului: valoarea liniei poartă semnul
            // cantității (LDI minus = negativă), dar conturile regulii deja
            // codifică direcția — nota se postează pozitivă. Fără regulă nu
            // există filtru de semn: valoarea culeasă se postează CA ATARE
            // (nota storno de import rămâne negativă).
            // Excepția declarativă: `PastreazaSemn` (FAZA 1C §7) — corespondența
            // de STORNO a retururilor (RLF/RDC) postează minus pe corespondența
            // ORIGINALĂ, deci semnul liniei trece nealterat prin normalizare.
            note.Add((d, contDebit, contCredit,
                regula != null && regula.PastreazaSemn ? d.Valoare : (regula?.SemnFiltru ?? +1) * d.Valoare,
                dimensiuniDebit, dimensiuniCredit));
        }

        // Pasul TVA (P1, design §4): postarea 4426/4427 e INDEPENDENTĂ de
        // potrivirea regulii principale — liniile de stoc ale FCT nu au regulă
        // de contare (netul postează pe NIR-ul conex), dar TVA-ul lor deductibil
        // se postează pe factură. Fără rând PoliticaTva pe tip = niciun rând TVA
        // (profilul bugetar rămâne neschimbat). Rândul e per linie (DetaliuId ca
        // tot restul); dimensiunile folosesc același coalesce, fără override-uri
        // de regulă: linie → default polimorf header (+ Materialul din lot).
        var politicaTva = os.FirstOrDefault<PoliticaTva>(p => p.TipDocumentId == tipDoc.ID);
        if (politicaTva != null) {
            var idsTipTva = doc.Detalii.Where(d => d.TipTvaId != null && d.ValoareTva != 0m)
                .Select(d => d.TipTvaId.Value).Distinct().ToList();
            var tipuriTva = os.GetObjectsQuery<TipTva>()
                .Where(t => idsTipTva.Contains(t.ID))
                .Select(t => new { t.ID, t.Cod, t.Regim, t.ContTvaDeductibilId, t.ContTvaColectatId })
                .ToDictionary(t => t.ID, t => (t.Cod, t.Regim, t.ContTvaDeductibilId, t.ContTvaColectatId));
            foreach (var d in doc.Detalii) {
                if (d.TipTvaId == null || d.ValoareTva == 0m)
                    continue;
                // Geamănul gardului din `RegistruTvaService` (review advers D4):
                // un `TipTva` șters logic din nomenclator lăsa liniile care-l referă
                // să pice cu `KeyNotFoundException`, adică o excepție brută în loc
                // de un refuz de domeniu cu remediu.
                if (!tipuriTva.TryGetValue(d.TipTvaId.Value, out var tva))
                    throw new OperareException(
                        "Tipul de TVA al unei linii nu mai există în nomenclator (a fost șters) — "
                        + "reatribuiți-l pe linie înainte de operare.");
                if (tva.Regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                    continue;
                Guid ContTva(Guid? id, string rol) => id ?? throw new OperareException(
                    $"Tipul de TVA {tva.Cod} nu are contul de TVA {rol} configurat.");
                Guid contDebit, contCredit;
                if (tva.Regim == RegimTva.TaxareInversa) {
                    // Autolichidare: 4426 = 4427 indiferent de direcție, sold zero.
                    contDebit = ContTva(tva.ContTvaDeductibilId, "deductibilă");
                    contCredit = ContTva(tva.ContTvaColectatId, "colectată");
                }
                else {
                    var contrapartida = RezolvaCont(politicaTva.SursaContrapartida,
                            politicaTva.ContrapartidaFallbackId, null, repartitorPredator, repartitorPrimitor)
                        ?? throw new OperareException(
                            $"Contrapartida rândului de TVA nu se poate rezolva ({tipDoc.Cod}, sursă {politicaTva.SursaContrapartida}).");
                    if (politicaTva.Directie == DirectieTva.Deductibil) {
                        contDebit = ContTva(tva.ContTvaDeductibilId, "deductibilă");
                        contCredit = contrapartida;
                    }
                    else {
                        contDebit = contrapartida;
                        contCredit = ContTva(tva.ContTvaColectatId, "colectată");
                    }
                }
                var materialTva = d.LotId != null && produsPerLot.TryGetValue(d.LotId.Value, out var produsTva)
                    ? produsTva : (Guid?)null;

                var dimensiuniLinie = d.DimensiuniCulese();

                note.Add((d, contDebit, contCredit, d.ValoareTva,
                    DimensiuniResolver.Rezolva(dimensiuniLinie,
                        new Dimensiuni { RepartitorId = doc.RepartitorImplicitDebit(os), MaterialId = materialTva }),
                    DimensiuniResolver.Rezolva(dimensiuniLinie,
                        new Dimensiuni { RepartitorId = doc.RepartitorImplicitCredit(os), MaterialId = materialTva })));
            }
        }

        // Faptele fiscale (felia 11, JT-D1): registrul de TVA e derivat de
        // serviciul-insulă `RegistruTvaService`, pe același criteriu de politică
        // ca pasul de mai sus — dar ACOPERĂ MAI MULT: rândurile `Scutit`,
        // `Neimpozabil` și `Capitalizat` nu postează nimic contabil, însă apar
        // legal în jurnalul de cumpărări/vânzări și în D300. Derivarea stă tot în
        // faza de CALCUL (33d), deci un refuz de mai jos n-o materializează.
        var randuriTva = RegistruTvaService.Deriva(os, doc, tipDoc, doc.Detalii);

        // Dimensiunile obligatorii per cont (decizia 15: flag-urile de defalcare
        // din plan = date de validare) se verifică pe seturile REZOLVATE, per
        // latură — abia aici se știe și contul, și rezultatul coalesce-ului.
        VerificaDimensiuniObligatorii(os, note, claseTip);

        return new PlanOperare {
            TipDoc = tipDoc, ClaseTip = claseTip, Miscari = miscari, Note = note, RanduriTva = randuriTva
        };
    }

    // Întoarce documentul conex generat (draft autogenerat, decizia 17) sau null.
    public static Document Opereaza(IObjectSpace os, Document doc) {
        var plan = CalculeazaSiValideaza(os, doc);
        var tipDoc = plan.TipDoc;
        var claseTip = plan.ClaseTip;
        var miscari = plan.Miscari;
        var note = plan.Note;

        // 3. Materializarea — toți gardienii au trecut.
        //
        //    Numărul se consumă ABIA ACUM (GATE XAF D6, alinierea cu propriul
        //    principiu 33d): asignat înaintea gardienilor, un refuz lăsa
        //    `doc.Numar` completat și `PoliticaNumerotare.UrmatorulNumar`
        //    incrementat în ObjectSpace-ul VIU al apelantului — necomise, dar un
        //    Save ulterior (UI-ul rulează motorul în OS-ul View-ului) le
        //    persista și rupea seria fiscală cu un gol. Ordinea față de
        //    conex/secundar e neschimbată: niciunul nu citește `Numar` (clona
        //    header-ului copiază doar data și laturile, iar plata automată își
        //    ia numărul din câmpul cules `PlataNumar`).
        AsignaNumar(os, doc, tipDoc);
        //    Scadența default (PoliticaScadenta, 30c) e tot o SCRIERE pe document,
        //    deci aparține aceleiași faze (review advers D8): înaintea gardienilor,
        //    un refuz o lăsa scrisă în ObjectSpace-ul viu al apelantului, pe care
        //    un Save ulterior o persista pe un document rămas Draft.
        AplicaScadenta(os, doc, tipDoc);

        //    Întâi finalizarea loturilor născute de liniile documentului (NIR
        //    manual, FacturaIntrare pentru lanțul conex, plus de inventar,
        //    producție): lotul e creat la culegere de linia de intrare (baza nu
        //    poartă ProdusId — testul bazei §2); motorul îi fixează prețul
        //    (= Valoare/Cantitate, decizia 13), data și atributele culese.
        var idsDetalii = doc.Detalii.Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>().Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            var linie = doc.Detalii.First(d => d.ID == lot.LinieIntrareId);
            if (linie.Cantitate <= 0)
                throw new OperareException("O linie care creează lot trebuie să aibă cantitate pozitivă.");
            // Împărțirea e sursa istorică a scării nemărginite (vezi `Scara`):
            // `decimal` păstrează toate zecimalele câtului, iar prețul se
            // propaga în fiecare `Valoare = Preț × Cantitate` de mai departe.
            // Prețul rămâne FIN (6 zecimale — identificarea specifică), doar
            // mărginit; coloana e oricum `numeric(18,6)`, rotunjirea aici ține
            // instanța din ObjectSpace-ul viu egală cu ce se persistă.
            lot.PretUnitar = Scara.RotunjestePret(linie.Valoare / linie.Cantitate);
            lot.Data = doc.Data;
            if (linie is ILinieCuAtributeLot atribute) {
                lot.DataExpirare = atribute.DataExpirare;
                lot.LotFabricatie = atribute.LotFabricatie;
            }
        }

        foreach (var (detaliu, regula, miscare) in miscari) {
            var rand = os.CreateObject<RegistruStoc>();
            rand.Data = doc.Data;
            rand.TipStoc = miscare.Cheie.TipStoc;
            rand.LotId = miscare.Cheie.LotId;
            rand.RepartitorId = miscare.Cheie.RepartitorId;
            rand.Cantitate = miscare.Cantitate;
            rand.Valoare = regula.Semn * detaliu.Valoare;
            rand.Document = doc;
            rand.Detaliu = detaliu;
        }

        foreach (var n in note) {
            var rand = os.CreateObject<RegistruContabil>();
            rand.Data = doc.Data;
            rand.ContDebitId = n.ContDebit;
            rand.ContCreditId = n.ContCredit;
            rand.Valoare = n.Valoare;
            rand.AplicaDimensiuniDebit(n.DimensiuniDebit);
            rand.AplicaDimensiuniCredit(n.DimensiuniCredit);
            rand.Document = doc;
            rand.Detaliu = n.Detaliu;
        }

        foreach (var t in plan.RanduriTva) {
            var rand = os.CreateObject<RegistruTva>();
            rand.Data = doc.Data;
            rand.Document = doc;
            rand.DetaliuId = t.DetaliuId;
            rand.Sens = t.Sens;
            rand.PartenerId = t.PartenerId;
            rand.TipTvaId = t.TipTvaId;
            rand.Regim = t.Regim;
            rand.Cota = t.Cota;
            rand.Baza = t.Baza;
            rand.Tva = t.Tva;
        }

        // 4. Documentul conex (decizia 17, 00 §6): draft autogenerat în aceeași
        //    tranzacție cu operarea sursei; utilizatorul îl completează și îl
        //    operează separat (abia atunci mișcă registre și primește număr).
        Document conex = null;
        var politicaConex = os.FirstOrDefault<PoliticaConex>(p => p.TipDocumentSursaId == tipDoc.ID);
        if (politicaConex != null)
            conex = GenereazaConex(os, doc, politicaConex,
                doc.Detalii.ToDictionary(d => d.ID, d => claseTip.GetValueOrDefault(d.TipMaterialId).Natura));

        // 5. Documentul secundar (decizia 31 — plata automată din 00 §7):
        //    construit de derivată din datele culese (hook), tratat ca orice
        //    copil autogenerat al grupului conex.
        var secundar = doc.GenereazaSecundar(os);
        if (secundar != null) {
            secundar.DocumentSursa = doc;
            secundar.Autogenerat = true;
        }

        doc.Stare = StareDocument.Operat;
        doc.DataOperare = DateTime.UtcNow;

        // 6. Plata autogenerată își creează imperecherea cu sursa la PROPRIA
        //    operare (ambele părți au registre abia acum) — echivalentul
        //    GEST_DECONTARI.AUTOGENERAT din plata automată legacy. Suma =
        //    restul stingibil (documentul poate fi deja parțial imperecheat).
        //
        //    Condiția „documentul STINGE" e polimorfă (F7-D5): nu orice document
        //    de trezorerie autogenerat cu sursă e o plată automată de factură —
        //    latura pereche a unui virament e tot asta, dar ea nu închide nicio
        //    datorie (`CapacitateStingere` = null pe virament). Fără gard,
        //    imperecherea cu propria sursă ar bloca anularea/stornarea ambelor
        //    picioare și ar polua panoul de stingeri.
        //
        //    Poarta întreabă AMBELE jumătăți ale rolului, pe copil ȘI pe sursă:
        //    „copilul stinge?" (`CapacitateStingere`) și „sursa se lasă stinsă?"
        //    (`PoateFiStins`). Fără a doua, un operator care editează laturile
        //    sau Tipul liniilor pe draftul autogenerat al perechii (până nu mai e
        //    virament) ar trece poarta, iar `Creeaza` ar arunca „nu împart
        //    aceeași contrapartidă" pe o încasare pe care el doar voia s-o
        //    opereze. Refuzul de imperechere nu are ce căuta pe calea operării.
        if (doc is DocumentTrezorerie trezorerie && trezorerie.Autogenerat && trezorerie.DocumentSursaId != null) {
            var sursa = os.GetObjectByKey<Document>(trezorerie.DocumentSursaId.Value);
            if (trezorerie.CapacitateStingere(os) != null && sursa.PoateFiStins(os)) {
                var suma = Math.Min(
                    doc.Detalii.Sum(d => d.Valoare + d.ValoareTva) - ImperechereService.Asignat(os, doc.ID),
                    ImperechereService.Ramas(os, sursa.ID));
                if (suma > 0)
                    ImperechereService.Creeaza(os, trezorerie, sursa, suma, autogenerat: true);
            }
        }

        os.CommitChanges();
        return conex ?? secundar;
    }

    // Obligativitățile per tip din PoliticaValidare (3d): reguli de PROFIL, nu
    // de structură (decizia 29) — la privat rândurile lipsesc și nimic nu se
    // cere. Angajamentul SAU codul economic satisface clasificația bugetară
    // (aceeași alternativă ca în validarea hardcodată pe care o înlocuiește).
    static void ValideazaDeclarativ(IObjectSpace os, Document doc, TipDocument tipDoc,
        Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip,
        ICollection<string> erori) {
        var politica = os.FirstOrDefault<PoliticaValidare>(p => p.TipDocumentId == tipDoc.ID);
        if (politica == null)
            return;
        foreach (var d in doc.Detalii) {
            var info = claseTip.GetValueOrDefault(d.TipMaterialId);
            if (politica.CereClasificatieBugetara && d.AngajamentId == null && d.DimensiuniCulese().CodEconomicId == null)
                erori.Add($"Linia cu {info.Denumire} cere clasificație bugetară: angajament sau cod economic.");
            if (politica.NaturaInterzisa != null && info.Natura == politica.NaturaInterzisa)
                erori.Add($"Liniile cu natura {politica.NaturaInterzisa} nu sunt permise pe {tipDoc.Cod} (linia cu {info.Denumire}).");
        }
    }

    // Decizia 15: flag-urile de defalcare din plan (R/M/E/B/F/P) = dimensiuni
    // obligatorii per cont, verificate pe rândul de registru rezolvat (per
    // latură). Punte până la modulul de angajamente: angajamentul liniei ține
    // loc de cod economic (clasificația trăiește în angajament; când modulul
    // apare, rezolvarea va materializa CodEconomic din angajament și puntea moare).
    static void VerificaDimensiuniObligatorii(IObjectSpace os,
        List<(DocumentDetaliu Detaliu, Guid ContDebit, Guid ContCredit,
            decimal Valoare, Dimensiuni DimensiuniDebit, Dimensiuni DimensiuniCredit)> note,
        Dictionary<Guid, (Guid ClasaId, NaturaClasa Natura, string Denumire, Guid? ContImplicitId)> claseTip) {
        if (note.Count == 0)
            return;
        var idsConturi = note.SelectMany(n => new[] { n.ContDebit, n.ContCredit }).Distinct().ToList();
        var conturi = os.GetObjectsQuery<Cont>()
            .Where(c => idsConturi.Contains(c.ID))
            .Select(c => new { c.ID, c.Simbol, c.DimensiuniObligatorii })
            .ToDictionary(c => c.ID, c => (c.Simbol, c.DimensiuniObligatorii));
        var lipsuri = new List<string>();
        foreach (var n in note) {
            var denumire = claseTip.GetValueOrDefault(n.Detaliu.TipMaterialId).Denumire;
            var (simbolDebit, flagsDebit) = conturi[n.ContDebit];
            VerificaLatura(simbolDebit, flagsDebit, n.DimensiuniDebit, n.Detaliu.AngajamentId, "debit", denumire, lipsuri);
            var (simbolCredit, flagsCredit) = conturi[n.ContCredit];
            VerificaLatura(simbolCredit, flagsCredit, n.DimensiuniCredit, n.Detaliu.AngajamentId, "credit", denumire, lipsuri);
        }
        if (lipsuri.Count > 0)
            throw new OperareException(string.Join("\n", lipsuri));
    }

    static void VerificaLatura(string simbol, DimensiuneFlags flags, Dimensiuni dims,
        Guid? angajamentId, string latura, string denumireLinie, ICollection<string> lipsuri) {
        if (flags == DimensiuneFlags.Niciuna)
            return;
        var lipsa = new List<string>();
        if (flags.HasFlag(DimensiuneFlags.Repartitor) && dims.RepartitorId == null)
            lipsa.Add("Repartitor");
        if (flags.HasFlag(DimensiuneFlags.Material) && dims.MaterialId == null)
            lipsa.Add("Material");
        if (flags.HasFlag(DimensiuneFlags.CodFunctional) && dims.CodFunctionalId == null)
            lipsa.Add("Cod funcțional");
        if (flags.HasFlag(DimensiuneFlags.CodEconomic) && dims.CodEconomicId == null && angajamentId == null)
            lipsa.Add("Cod economic");
        if (flags.HasFlag(DimensiuneFlags.SursaFinantare) && dims.SursaFinantareId == null)
            lipsa.Add("Sursă de finanțare");
        if (flags.HasFlag(DimensiuneFlags.Unitate) && dims.UnitateId == null)
            lipsa.Add("Unitate");
        if (flags.HasFlag(DimensiuneFlags.Proiect) && dims.ProiectId == null)
            lipsa.Add("Proiect");
        if (flags.HasFlag(DimensiuneFlags.CentruCost) && dims.CentruCostId == null)
            lipsa.Add("Centru de cost");
        if (lipsa.Count > 0)
            lipsuri.Add($"Contul {simbol} ({latura}, linia cu {denumireLinie}) cere: {string.Join(", ", lipsa)}.");
    }

    // Sursa declarativă a contului unei laturi (testul bazei §7.2); contul
    // explicit al regulii e valoare directă sau fallback când sursa nu rezolvă.
    // ContImplicit stă pe baza Repartitor (decizia 31): partener 401/404/411,
    // cont propriu 5xx/770, angajat 542.
    static Guid? RezolvaCont(SursaCont sursa, Guid? contExplicit, Guid? contTipMaterial,
        Repartitor repartitorPredator, Repartitor repartitorPrimitor) => sursa switch {
        SursaCont.TipMaterial => contTipMaterial ?? contExplicit,
        SursaCont.RepartitorPredator => repartitorPredator?.ContImplicitId ?? contExplicit,
        SursaCont.RepartitorPrimitor => repartitorPrimitor?.ContImplicitId ?? contExplicit,
        _ => contExplicit,
    };

    // Clonarea 00 §6: header (cu InverseazaLaturi), DOAR liniile care trec
    // filtrul de natură; liniile clonate poartă aceleași loturi și dimensiuni.
    // Fără linii eligibile nu se generează nimic (o factură doar de servicii
    // nu produce NIR).
    static Document GenereazaConex(IObjectSpace os, Document sursa, PoliticaConex politica,
        IReadOnlyDictionary<Guid, NaturaClasa> naturaPerLinie) {
        var linii = sursa.Detalii
            .Where(d => politica.NaturaFiltru == null || naturaPerLinie.GetValueOrDefault(d.ID) == politica.NaturaFiltru)
            .ToList();
        if (linii.Count == 0)
            return null;

        var tipTinta = os.GetObjectByKey<TipDocument>(politica.TipDocumentTintaId);
        var tipClr = typeof(Document).Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == tipTinta.ClrType && typeof(Document).IsAssignableFrom(t))
            ?? throw new OperareException($"Clasa documentului conex ({tipTinta?.ClrType}) nu există.");
        var conex = (Document)os.CreateObject(tipClr);
        conex.Data = sursa.Data;
        conex.PredatorId = politica.InverseazaLaturi ? sursa.PrimitorId : sursa.PredatorId;
        conex.PrimitorId = politica.InverseazaLaturi ? sursa.PredatorId : sursa.PrimitorId;
        conex.DocumentSursa = sursa;
        conex.Autogenerat = true;
        // DIM-2: liniile clonei se nasc pe FRUNZA declarată a țintei ([TipDetaliu]
        // — aceeași declarație pe care o consumă UI-ul, 40a); o linie de bază ar
        // face PreiaDimensiuni no-op și clona ar pierde dimensiunile culese.
        var tipDetaliu = tipClr.GetCustomAttribute<UI.TipDetaliuAttribute>(inherit: false)?.TipDetaliu
            ?? typeof(DocumentDetaliu);
        foreach (var s in linii) {
            var d = (DocumentDetaliu)os.CreateObject(tipDetaliu);
            d.Document = conex;
            d.TipMaterialId = s.TipMaterialId;
            d.LotId = s.LotId;
            d.Cantitate = s.Cantitate;
            d.Valoare = s.Valoare;
            // TipTva se clonează ca informație; ValoareTva NU — TVA-ul liniei
            // se postează pe documentul sursă (P1, design §4/§6: NIR-ul duce
            // netul, factura duce rândurile 4426).
            d.TipTvaId = s.TipTvaId;
            d.AngajamentId = s.AngajamentId;
            d.PreiaDimensiuni(s.DimensiuniCulese());
        }
        return conex;
    }

    // Corecția directă din decizia 14: întoarcerea în Draft, permisă DOAR fără
    // dependenți (simularea eliminării rândurilor proprii ține soldurile ≥ 0 și
    // niciun alt document nu a atins loturile create) și în perioadă deschisă.
    public static void AnuleazaOperarea(IObjectSpace os, Document doc) {
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi anulat.");
        GardianPerioada.VerificaDeschisa(os, doc.Data);
        VerificaFaraLaturaPerecheOperata(os, doc);
        VerificaFaraConexeOperate(os, doc);
        VerificaFaraImperecheri(os, doc);
        StergeConexeDraftAutogenerate(os, doc);

        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID).ToList();
        var randuriContabile = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();
        // Faptele fiscale se șterg pe aceeași cale ca celelalte două registre
        // (JT-D5): niciun gardian propriu — TVA-ul nu e o resursă cu sold, deci
        // n-are ce verifica înainte, spre deosebire de stoc.
        var randuriTva = os.GetObjectsQuery<RegistruTva>().Where(r => r.DocumentId == doc.ID).ToList();

        // Simularea eliminării: delta goală, rândurile proprii excluse, dar
        // cheile lor re-verificate de la prima dată afectată.
        if (randuriStoc.Count > 0) {
            var primaData = randuriStoc.Min(r => r.Data);
            var santinele = randuriStoc
                .Select(r => new CheieStoc(r.LotId, r.RepartitorId, r.TipStoc)).Distinct()
                .Select(c => new MiscareStoc(c, primaData, 0m)).ToList();
            StocService.VerificaSoldIntermediar(os, santinele, randuriStoc.Select(r => r.ID).ToList());
        }

        // Loturile create de liniile documentului nu au voie să fi fost atinse
        // de altcineva (nici măcar cu mișcări care lasă soldul ≥ 0).
        var idsDetalii = doc.Detalii.Select(d => d.ID).ToList();
        foreach (var lot in os.GetObjectsQuery<Lot>().Where(l => l.LinieIntrareId != null && idsDetalii.Contains(l.LinieIntrareId.Value)).ToList()) {
            if (os.GetObjectsQuery<RegistruStoc>().Any(r => r.LotId == lot.ID && r.DocumentId != doc.ID))
                throw new OperareException(
                    $"Lotul {lot.Produs?.Denumire} din {lot.Data:yyyy-MM-dd} e folosit de alte documente — folosiți storno.");
        }

        os.Delete(randuriStoc);
        os.Delete(randuriContabile);
        os.Delete(randuriTva);
        doc.Stare = StareDocument.Draft;
        doc.DataOperare = null;
        os.CommitChanges();
    }

    // Storno (decizia 14): rânduri inverse la data stornării, registrele rămân
    // append-only. Singura cale de corecție peste graniță (perioada documentului
    // închisă sau dependenți existenți), cât timp perioada stornării e deschisă
    // și soldurile rămân ≥ 0 din data stornării încolo.
    public static void Storneaza(IObjectSpace os, Document doc, DateOnly dataStorno) {
        if (doc.Stare != StareDocument.Operat)
            throw new OperareException("Doar un document Operat poate fi stornat.");
        if (dataStorno < doc.Data)
            throw new OperareException("Data stornării nu poate preceda data documentului.");
        GardianPerioada.VerificaDeschisa(os, dataStorno);
        VerificaFaraLaturaPerecheOperata(os, doc);
        VerificaFaraConexeOperate(os, doc);
        VerificaFaraImperecheri(os, doc);
        StergeConexeDraftAutogenerate(os, doc);

        var randuriStoc = os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId == doc.ID).ToList();
        var randuriContabile = os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId == doc.ID).ToList();
        var randuriTva = os.GetObjectsQuery<RegistruTva>().Where(r => r.DocumentId == doc.ID).ToList();

        var delta = randuriStoc
            .Select(r => new MiscareStoc(new CheieStoc(r.LotId, r.RepartitorId, r.TipStoc), dataStorno, -r.Cantitate))
            .ToList();
        StocService.VerificaSoldIntermediar(os, delta);

        foreach (var r in randuriStoc) {
            var invers = os.CreateObject<RegistruStoc>();
            invers.Data = dataStorno;
            invers.TipStoc = r.TipStoc;
            invers.LotId = r.LotId;
            invers.RepartitorId = r.RepartitorId;
            invers.Cantitate = -r.Cantitate;
            invers.Valoare = -r.Valoare;
            invers.Storno = true;
            invers.Document = doc;
            invers.DetaliuId = r.DetaliuId;
        }
        foreach (var r in randuriContabile) {
            var invers = os.CreateObject<RegistruContabil>();
            invers.Data = dataStorno;
            invers.NumarNota = r.NumarNota;
            invers.ContDebitId = r.ContDebitId;
            invers.ContCreditId = r.ContCreditId;
            invers.Valoare = -r.Valoare;
            invers.AplicaDimensiuniDebit(r.DimensiuniDebit());
            invers.AplicaDimensiuniCredit(r.DimensiuniCredit());
            invers.Storno = true;
            invers.Document = doc;
            invers.DetaliuId = r.DetaliuId;
        }
        // Faptele fiscale se inversează la fel (JT-D5): jurnalul nu filtrează
        // NICIODATĂ `Storno` — registrul e append-only și suma lui algebrică e
        // adevărul (R-D7). Consecință acceptată: rândul invers cade în luna
        // stornării, deci jurnalul lunii deja declarate rămâne cum a fost declarat.
        // Identitatea fiscală (`Sens`/`TipTva`/`Regim`/`Cota`/partener) se copiază
        // ca atare — snapshot-ul rândului original, nu o re-derivare din politica
        // de azi, care între timp poate fi alta.
        foreach (var r in randuriTva) {
            var invers = os.CreateObject<RegistruTva>();
            invers.Data = dataStorno;
            invers.Sens = r.Sens;
            invers.Document = doc;
            invers.DetaliuId = r.DetaliuId;
            invers.PartenerId = r.PartenerId;
            invers.TipTvaId = r.TipTvaId;
            invers.Regim = r.Regim;
            invers.Cota = r.Cota;
            invers.Baza = -r.Baza;
            invers.Tva = -r.Tva;
            invers.Storno = true;
        }

        doc.Stare = StareDocument.Stornat;
        os.CommitChanges();
    }

    // Oglinda exactă a gardianului de grup conex pentru legătura de pereche
    // (F8-D9): pointer-ul ≈ copil, ținta ≈ sursă. Piciorul care DECLARĂ legătura
    // e frunza și se anulează liber (nimeni nu depinde de el); ținta nu, cât timp
    // pointer-ul e OPERAT — altfel ținta ar reveni în Draft, s-ar re-opera și ar
    // genera din nou o pereche, lângă cea deja operată: dublă postare pe 581.
    //
    // Se apelează ÎNAINTEA gardianului de grup conex, deliberat: perechea
    // autogenerată e acoperită de amândoi (are și `DocumentSursaId`, și link),
    // iar mesajul ăsta e cel specific — „latura pereche" spune operatorului
    // exact ce vede pe ecran.
    static void VerificaFaraLaturaPerecheOperata(IObjectSpace os, Document doc) {
        if (os.GetObjectsQuery<DocumentTrezorerie>()
                .Any(x => x.LaturaPerecheId == doc.ID && x.Stare == StareDocument.Operat))
            throw new OperareException(
                "Documentul e declarat latura pereche a unui virament încă operat — "
                + "anulați/stornați întâi acel document.");
    }

    // Anularea/stornarea operează pe grupul conex (00 §8): copiii cu registre
    // (Operat) se anulează/stornează întâi — refuz conservator; copiii DRAFT
    // autogenerați sunt un artefact al operării anulate și se șterg odată cu ea
    // (re-operarea sursei generează un draft proaspăt).
    static void VerificaFaraConexeOperate(IObjectSpace os, Document doc) {
        if (os.GetObjectsQuery<Document>().Any(x => x.DocumentSursaId == doc.ID && x.Stare == StareDocument.Operat))
            throw new OperareException(
                "Documentul are documente generate (conexe) încă operate — anulați/stornați întâi acele documente.");
    }

    // Stingerea leagă REGISTRELE celor două documente (decizia 17); anularea
    // sau stornarea uneia dintre părți ar lăsa imperecherea fără acoperire —
    // refuz conservator: utilizatorul șterge întâi imperecherile (link simplu,
    // fără registre proprii), apoi corectează documentul.
    static void VerificaFaraImperecheri(IObjectSpace os, Document doc) {
        if (os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == doc.ID || i.DocumentId == doc.ID))
            throw new OperareException(
                "Documentul are imperecheri (stingeri) — ștergeți-le întâi, apoi anulați/stornați.");
    }

    static void StergeConexeDraftAutogenerate(IObjectSpace os, Document doc) {
        var stersi = false;
        foreach (var copil in os.GetObjectsQuery<Document>()
            .Where(x => x.DocumentSursaId == doc.ID && x.Autogenerat && x.Stare == StareDocument.Draft).ToList()) {
            os.Delete(os.GetObjectsQuery<DocumentDetaliu>().Where(d => d.DocumentId == copil.ID).ToList());
            os.Delete(copil);
            stersi = true;
        }
        // Review advers F5-F4: de când draftul conex e editabil (F5-D8b), operatorul
        // poate adăuga pe el linii MANUALE, iar acelea nasc loturi proprii la
        // culegere. Ștergerea de aici se face în ObjectSpace-ul non-secured al
        // motorului, unde nu rulează niciun controller de culegere — fără pasul
        // ăsta loturile lor ar rămâne orfane (preț 0, dată 0001-01-01, fără linie
        // mamă), permanent în nomenclator și în lookup-uri. Fără impact contabil
        // (fără rânduri de stoc nu intră în picking), dar e exact zgomotul pe
        // care curățenia există să-l prevină.
        if (stersi)
            LoturiCulegereService.CurataOrfane(os);
    }

    // Ancora TipDocument după numele CLR al clasei — reutilizabilă (motor,
    // DescarcareService, TvaService): totul se cheiază pe TipDocument.ID.
    internal static TipDocument GasesteTipDocument(IObjectSpace os, string clrType) =>
        os.FirstOrDefault<TipDocument>(t => t.ClrType == clrType)
            ?? throw new OperareException($"Lipsește ancora TipDocument pentru clasa {clrType} (seed).");

    // EF Core dă proxy-uri de change-tracking — numele CLR real e pe tipul de bază.
    internal static TipDocument GasesteTipDocument(IObjectSpace os, Document doc) {
        var tip = doc.GetType();
        while (tip.Assembly.IsDynamic || tip.Name.EndsWith("Proxy"))
            tip = tip.BaseType;
        return GasesteTipDocument(os, tip.Name);
    }

    static void AsignaNumar(IObjectSpace os, Document doc, TipDocument tipDoc) {
        if (!string.IsNullOrWhiteSpace(doc.Numar))
            return;
        var politica = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tipDoc.ID);
        if (politica == null)
            throw new OperareException(
                $"Documentul nu are număr și tipul {tipDoc.Cod} nu are politică de numerotare.");
        var n = politica.UrmatorulNumar;
        politica.UrmatorulNumar = n + 1;
        doc.Numar = string.IsNullOrWhiteSpace(politica.Format)
            ? $"{politica.Serie}{n}"
            : string.Format(politica.Format, n, politica.Serie);
    }

    // Scadența cu default de politică (inventar 07): se aplică doar când nu a
    // fost culeasă — politica e default, nu constrângere. Tipurile fără rând de
    // politică (ex. FCT — scadența furnizorului se culege) rămân neatinse.
    static void AplicaScadenta(IObjectSpace os, Document doc, TipDocument tipDoc) {
        if (doc is not IDocumentCuScadenta scadenta || scadenta.DataScadenta != null)
            return;
        var politica = os.FirstOrDefault<PoliticaScadenta>(p => p.TipDocumentId == tipDoc.ID);
        if (politica != null)
            scadenta.DataScadenta = doc.Data.AddDays(politica.ZileDefault);
    }
}
