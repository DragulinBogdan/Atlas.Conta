using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Itv;

// Felia ITV: ușa de CITIRE și cele trei comenzi proprii (previzualizare,
// generare, regenerare) peste `InchidereTvaService`. ZERO ASP.NET aici —
// controllerul din host e transport, iar ModelCheck exersează exact același cod.
//
// ═══ Ce e ALTFEL față de toate celelalte felii ═══
// Nu există `Aplica`. Documentul nu se culege: antetul e server-owned integral
// (`Numar` la materializare, `Data` = ultima zi a lunii, laturile = unitatea
// cerută), iar liniile sunt calculate din soldurile registrului. „Modificarea"
// unei închideri e REGENERAREA ei, nu un PUT.
//
// ═══ CONTRACT DE APELANT ═══
// `Previzualizeaza`/`Genereaza`/`Regenereaza` rulează pe ușa NON-SECURED
// (58c: serviciul scrie câmpuri server-owned, iar cifrele lui sunt ale
// motorului — soldurile calculate pe rânduri filtrate de permisiuni ar fi o
// cifră FALSĂ prezentată ca „nu e nimic de închis", argumentul 73g). Gate-ul de
// autorizare rămâne al controllerului, ÎNAINTE (55b). `Sterge` rulează pe ușa
// SECURED, ca la NTC — gardianul de Committing e ultima autoritate.
//
// `Lista`/`Citeste` sunt proiecții PLATE (42c): `Select` înainte de
// materializare, nicio navigație enumerată în afara query-ului (25b).
public static class InchidereTvaApply {

    // ═══════════════════════ Citire ═══════════════════════

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). DOAR `InchidereTva`: sub TPT interogarea pe
    // frunză NU întoarce alte note (perechea lui F21-D5, unde felia NTC le
    // exclude explicit pe astea).
    //
    // `An`/`Luna` ies din `Data` ÎN SQL (`EXTRACT`), nu în memorie: sunt coloanele
    // pe care le filtrează grila, iar modelul nu le are ca date.
    public static IQueryable<ItvListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<InchidereTva>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new ItvListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   An = d.Data.Year,
                   Luna = d.Data.Month,
                   // Enum → string ÎN SQL (`CASE`): filtrarea și sortarea rămân
                   // server-side, deși pe sârmă starea e text (57a).
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   DataOperare = d.DataOperare,
                   // Laturile sunt AMBELE unitatea care închide luna; se arată una.
                   UnitateDenumire = d.Predator.Denumire,
                   Total = (decimal?)t.Total ?? 0m
               };
    }

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o închidere de TVA.
    public static ItvReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<InchidereTva>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, UnitateDenumire = d.Predator.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Liniile pe BAZA detaliului cu frunza prin `as` (TPT ⇒ LEFT JOIN), ca la
        // NTC: o închidere importată/istorică ar putea purta linii de tip bază, iar
        // pe frunză singură ar fi ieșit `Linii: []` cu `Total` nenul.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID,
                Descriere = (l as NotaContabilaDetaliu).Descriere,
                ContDebitId = (l as NotaContabilaDetaliu).ContDebitId,
                ContDebitSimbol = (l as NotaContabilaDetaliu).ContDebit.Simbol,
                ContCreditId = (l as NotaContabilaDetaliu).ContCreditId,
                ContCreditSimbol = (l as NotaContabilaDetaliu).ContCredit.Simbol,
                l.Valoare, l.ValoareTva
            })
            .ToList();

        // Rolurile liniilor prin CONTURILE POLITICII (29), niciodată prin simbol —
        // și prin ACELEAȘI perechi ca gardianul anti-stale din
        // `InchidereTva.ValideazaOperare`. Fără politică (profil inert) rămân 0:
        // acolo nu există documente ITV, deci cazul e teoretic.
        var politica = os.FirstOrDefault<PoliticaInchidereTva>(
            p => p.TipDocument.ClrType == nameof(InchidereTva));
        decimal Suma(Guid? debit, Guid? credit) => debit == null || credit == null
            ? 0m
            : linii.Where(l => l.ContDebitId == debit && l.ContCreditId == credit).Sum(l => l.Valoare);
        var transfer = Suma(politica?.ContColectataId, politica?.ContDeductibilaId);
        var dePlata = Suma(politica?.ContColectataId, politica?.ContDePlataId);
        var deRecuperat = Suma(politica?.ContDeRecuperatId, politica?.ContDeductibilaId);

        // Cifra MOTORULUI la `Data` documentului — aceeași funcție (`Solduri`) pe
        // care o cheamă gardianul; o a doua formulă aici ar fi divergeat tăcut.
        var (sold4426, sold4427) = politica?.ContDeductibilaId == null || politica.ContColectataId == null
            ? (0m, 0m)
            : InchidereTvaService.Solduri(
                os, politica.ContDeductibilaId.Value, politica.ContColectataId.Value, h.Data);

        // `Stale` DOAR pe Draft: pe Operat/Stornat cifra e deja în registru, iar
        // soldurile „curente" o includ — întrebarea n-ar mai avea sens. Criteriul
        // e EXACT cel al gardianului (`LiniiPotrivescSoldurile`, 79 M4) — nu o
        // formulă geamănă; pe politică incompletă gardianul refuză din prima
        // ramură, deci aici nu există verdict (`null`), nu un `false` liniștitor.
        bool? stale = null;
        var politicaCompleta = politica?.ContDeductibilaId != null && politica.ContColectataId != null
            && politica.ContDePlataId != null && politica.ContDeRecuperatId != null;
        if (h.Stare == StareDocument.Draft && politicaCompleta)
            stale = !InchidereTvaService.LiniiPotrivescSoldurile(
                new InchidereTvaService.LiniiInchidere(transfer, dePlata, deRecuperat), sold4426, sold4427);

        // Affordance ONESTĂ (57d): deși `CapacitateStingere` iese dicționar GOL pe
        // ITV (liniile n-au repartitori), afordanța se scrie pe aceeași sursă ca
        // gardianul de anulare/storno, nu pe presupunerea că n-are imperecheri.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new ItvReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            An = h.Data.Year, Luna = h.Data.Month,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            UnitateId = h.PredatorId, UnitateDenumire = h.UnitateDenumire,
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            Transfer = transfer, DePlata = dePlata, DeRecuperat = deRecuperat,
            Sold4426Curent = sold4426, Sold4427Curent = sold4427,
            Stale = stale,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateSterge = h.Stare == StareDocument.Draft,
            PoateRegenera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new ItvLinieReadDto {
                Id = l.ID,
                Descriere = l.Descriere,
                ContDebitId = l.ContDebitId, ContDebitSimbol = l.ContDebitSimbol,
                ContCreditId = l.ContCreditId, ContCreditSimbol = l.ContCreditSimbol,
                Valoare = l.Valoare
            }).ToList()
        };
    }

    // ═══════════════════════ Comenzi ═══════════════════════

    // Dry-run-ul generării: NU scrie nimic. Motivul iese ca nume de membru enum
    // (57a), iar documentul care blochează vine cu eticheta lui, ca ecranul să
    // poată face link în loc să afișeze un GUID.
    public static PrevizualizareItvDto Previzualizeaza(IObjectSpace os, int an, int luna) {
        var r = InchidereTvaService.Previzualizeaza(os, an, luna);
        // Simbolurile conturilor vin din POLITICĂ (29), ca ecranul să nu le
        // afirme din cod (review 79 M6): pe un profil cu alte conturi de
        // închidere etichetele ar fi numit conturi care nu sunt ale lui.
        var simboluri = os.GetObjectsQuery<PoliticaInchidereTva>()
            .Where(p => p.TipDocument.ClrType == nameof(InchidereTva))
            .Select(p => new {
                Deductibila = p.ContDeductibila.Simbol, Colectata = p.ContColectata.Simbol,
                DePlata = p.ContDePlata.Simbol, DeRecuperat = p.ContDeRecuperat.Simbol
            })
            .FirstOrDefault();
        var dto = new PrevizualizareItvDto {
            An = an, Luna = luna,
            Motiv = r.Motiv?.ToString(),
            Sold4426 = r.Sold4426, Sold4427 = r.Sold4427,
            Transfer = r.Linii.Transfer, DePlata = r.Linii.DePlata, DeRecuperat = r.Linii.DeRecuperat,
            InchidereVieId = r.InchidereVieId,
            SimbolDeductibila = simboluri?.Deductibila, SimbolColectata = simboluri?.Colectata,
            SimbolDePlata = simboluri?.DePlata, SimbolDeRecuperat = simboluri?.DeRecuperat
        };
        if (r.InchidereVieId is Guid blocantId) {
            var blocant = os.GetObjectsQuery<InchidereTva>()
                .Where(d => d.ID == blocantId)
                .Select(d => new { d.Numar, d.Stare, d.Data })
                .FirstOrDefault();
            // Draftul n-are încă număr (seria se consumă la materializare, 53b) —
            // eticheta cade pe dată, ca peste tot (`Eticheta`).
            dto.InchidereVieNumar = string.IsNullOrWhiteSpace(blocant?.Numar)
                ? blocant?.Data.ToString("dd.MM.yyyy")
                : blocant.Numar;
            dto.InchidereVieStare = blocant?.Stare.ToString();
        }
        return dto;
    }

    // Comanda de generare. Commit CONDIȚIONAT: fără draft nu s-a schimbat nimic,
    // iar un commit gol ar fi fost zgomot pe o cerere care e, de fapt, un raport.
    // 200 și când `Motiv != null` — „luna e deja închisă" / „n-are ce închide" e
    // un răspuns ADEVĂRAT, nu o eroare (precedentele `DscId = null` la backorder,
    // 58, și lotul ANAF, 72e). 422 rămâne al refuzurilor de DOMENIU pe care le
    // aruncă serviciul: cronologia (46c), `TRZ` lipsă, unitatea ne-internă.
    public static GenerareItvRezultatDto Genereaza(IObjectSpace os, GenerareItvRequestDto cerere) {
        if (cerere == null)
            throw new OperareException("Lipsește corpul cererii.");
        var r = InchidereTvaService.Incearca(os, cerere.An, cerere.Luna, cerere.UnitateId);
        if (r.Document != null)
            os.CommitChanges();
        return Rezultat(r);
    }

    // Regenerarea unui DRAFT: o generare nouă pe aceeași lună și aceeași unitate
    // (`PredatorId` al draftului — unitatea nu se cere din nou, F21-D4), care
    // ÎNLOCUIEȘTE draftul vechi. DOAR pe Draft: pe un document Operat
    // „regenerarea" ar fi însemnat ștergerea unor rânduri de registru, adică
    // exact ce interzice 14.
    //
    // O SINGURĂ tranzacție, în ordinea „întâi se calculează, apoi se șterge"
    // (review 79 F2): prima formă ștergea și COMITEA înainte de `Incearca`, iar
    // un refuz al gardienilor (cronologie, unitate ștearsă între timp) lăsa
    // draftul pierdut definitiv pe o acțiune al cărei text promite că îl reface.
    // `Incearca(…, inlocuieste: id)` nu numără draftul de față ca „închidere
    // vie", deci idempotența nu mai depinde de filtrul ștergerii amânate; dacă
    // refuză, aruncă ÎNAINTE de orice `Delete`, iar draftul rămâne intact.
    //
    // Cazul „luna nu mai are ce închide" (`FaraSold`, de pildă după stornarea
    // documentelor ei) e LEGITIM: draftul vechi e depășit și se șterge, iar
    // raportul iese cu `DocumentId = null`.
    public static GenerareItvRezultatDto Regenereaza(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<InchidereTva>(os, id, "Închiderea de TVA");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Închiderea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se regenerează. "
                + "Anulați operarea sau stornați-o, apoi generați luna din nou.");

        var r = InchidereTvaService.Incearca(os, doc.Data.Year, doc.Data.Month, doc.PredatorId,
            inlocuieste: doc.ID);

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
        return Rezultat(r);
    }

    // Ștergerea draftului, ca la NTC: pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), ștergere AMÂNATĂ (60a). FĂRĂ curățenie de loturi
    // — liniile închiderii nu nasc loturi, deci un apel ar fi fost inofensiv, dar
    // mincinos.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<InchidereTva>(os, id, "Închiderea de TVA");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Închiderea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-o.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    static GenerareItvRezultatDto Rezultat(InchidereTvaService.RezultatInchidere r) =>
        new() {
            DocumentId = r.Document?.ID,
            Motiv = r.Motiv?.ToString(),
            InchidereVieId = r.InchidereVieId,
            Sold4426 = r.Sold4426, Sold4427 = r.Sold4427,
            Transfer = r.Linii.Transfer, DePlata = r.Linii.DePlata, DeRecuperat = r.Linii.DeRecuperat
        };

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;
}
