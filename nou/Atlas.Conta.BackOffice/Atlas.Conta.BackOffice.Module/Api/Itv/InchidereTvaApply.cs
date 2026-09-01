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

    // `null` dacă documentul nu există (sau nu e o închidere de TVA).
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
        // e cel al gardianului, exprimat prin `CalculeazaLinii` (o singură
        // aritmetică, F21-D2a): ce s-ar genera ACUM față de ce scrie pe document.
        bool? stale = null;
        if (h.Stare == StareDocument.Draft && politica?.ContDeductibilaId != null) {
            var acum = InchidereTvaService.CalculeazaLinii(sold4426, sold4427);
            stale = acum.Transfer != transfer || acum.DePlata != dePlata
                || acum.DeRecuperat != deRecuperat;
        }

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
        var dto = new PrevizualizareItvDto {
            An = an, Luna = luna,
            Motiv = r.Motiv?.ToString(),
            Sold4426 = r.Sold4426, Sold4427 = r.Sold4427,
            Transfer = r.Linii.Transfer, DePlata = r.Linii.DePlata, DeRecuperat = r.Linii.DeRecuperat,
            InchidereVieId = r.InchidereVieId
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

    // Regenerarea unui DRAFT: ștergerea lui, apoi o generare nouă pe aceeași lună
    // și aceeași unitate (`PredatorId` al draftului — unitatea nu se cere din nou,
    // F21-D4). DOAR pe Draft: pe un document Operat „regenerarea" ar fi însemnat
    // ștergerea unor rânduri de registru, adică exact ce interzice 14.
    //
    // DOUĂ COMMIT-URI SECVENȚIALE, nu o tranzacție, și e o alegere, nu o scăpare:
    // ștergerea amânată (60a) pune `GCRecord`, iar filtrul global al
    // ObjectSpace-ului ascunde draftul vechi abia DUPĂ commit — fără primul
    // commit, gardianul de idempotență al serviciului l-ar fi văzut încă viu și
    // ar fi întors `InchidereVie`. (Cusătura e MĂSURATĂ în ModelCheck: dacă
    // filtrul n-ar ascunde draftul șters, proba pică zgomotos.)
    //
    // Cazul „ștergerea a reușit, generarea a dat `FaraSold`" e LEGITIM (luna nu
    // mai are ce închide, de pildă după ce documentele ei au fost stornate) și
    // iese ca raport cu `DocumentId = null`. Fereastra dintre commit-uri e
    // limitarea asumată 25f (fără serializare între operatori concurenți).
    public static GenerareItvRezultatDto Regenereaza(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<InchidereTva>(id)
            ?? throw new OperareException($"Închiderea de TVA {id} nu există.");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Închiderea {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se regenerează. "
                + "Anulați operarea sau stornați-o, apoi generați luna din nou.");

        var an = doc.Data.Year;
        var luna = doc.Data.Month;
        var unitateId = doc.PredatorId;

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();

        var r = InchidereTvaService.Incearca(os, an, luna, unitateId);
        if (r.Document != null)
            os.CommitChanges();
        return Rezultat(r);
    }

    // Ștergerea draftului, ca la NTC: pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), ștergere AMÂNATĂ (60a). FĂRĂ curățenie de loturi
    // — liniile închiderii nu nasc loturi, deci un apel ar fi fost inofensiv, dar
    // mincinos.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<InchidereTva>(id)
            ?? throw new OperareException($"Închiderea de TVA {id} nu există.");
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
