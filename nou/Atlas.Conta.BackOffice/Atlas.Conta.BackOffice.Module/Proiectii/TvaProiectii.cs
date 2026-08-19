using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExtreme.AspNet.Data;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

// Jurnalele de TVA (JT-D7) — a doua familie de rapoarte care trăiește pe un
// registru append-only, după balanță/fișă/jurnal (felia 9). Aceleași reguli, cu
// aceleași motive: `IQueryable` pur (`DataSourceLoader` pune sortarea și
// paginarea DEASUPRA, SQL-ul se execută o dată, server-side), nimic nu se
// calculează în client (42c), iar `Storno` NU se filtrează niciodată — registrul
// e append-only și suma lui algebrică e adevărul (R-D7).
//
// ═══ De ce agregarea e a JURNALULUI, nu a registrului ═══
// `RegistruTva` are un rând per LINIE de document (JT-D1), fiindcă asta cere
// SAF-T. Un jurnal de cumpărări/vânzări listează însă FACTURI: cinci poziții cu
// aceeași cotă sunt un singur rând de jurnal, cu baza și TVA-ul lor însumate.
// Agregarea trăiește deci AICI, ca proiecție, iar registrul rămâne granular
// pentru cine are nevoie de granularitate.
//
// ═══ `IgnoreAutoIncludes()`: de ce NU e aici ═══
// Balanța și registrul-jurnal îl cer explicit fiindcă `BackOfficeDbContext` pune
// `AutoInclude()` pe cele 16 navigații de dimensiuni ale lui `RegistruContabil`
// (41c). `RegistruTva` NU are niciuna — deliberat, cu motivul scris în context
// (DbSet-ul lui: „consumatorii reali sunt proiecțiile, care își fac join-urile
// explicit în `Select`"). Nu există include de ignorat, deci apelul ar fi decor.
// Verificat, nu presupus: singurul `AutoInclude` din context e pe
// `RegistruContabilEntitate`.

// Un rând de jurnal = o pereche (Document × TipTva), pe un singur `Sens`.
// PLAT prin construcție (deciziile 6/7).
public sealed class JurnalTvaRand : IRandCuDocument {
    // NENUL în registru (JT-D1: jurnalul n-are rânduri de deschidere). Interfața
    // `IRandCuDocument` îl cere `Guid?` — se implementează explicit mai jos, ca
    // pe sârmă contractul să rămână cel adevărat: documentul EXISTĂ întotdeauna.
    public Guid DocumentId { get; set; }
    Guid? IRandCuDocument.DocumentId => DocumentId;
    public string DocumentNumar { get; set; }
    // Nu vine din SQL (sub TPT nu există discriminator, iar ancora `TipDocument`
    // se caută după numele clasei CLR — R-D8/60b): se completează în memorie
    // peste pagină, prin `ContabilProiectii.CompleteazaTipDocument`.
    public string DocumentTip { get; set; }
    public DateOnly Data { get; set; }

    // Contrapartida laturii cerute de politică. Nullable din același motiv ca pe
    // registru (riscul 4 din design: `SursaContrapartida` care nu e o latură ⇒
    // gaura se raportează, nu se refuză). Tipul de bază e `Repartitor`, deci pe
    // `Decont` e chiar ANGAJATUL — jurnalul arată onest ce știe modelul.
    public Guid? PartenerId { get; set; }
    public string PartenerDenumire { get; set; }
    // Doar `Partener` are cod fiscal (`Repartitor` e baza TPT) — as-cast, adică
    // LEFT JOIN pe frunză: un angajat sau o gestiune îl lasă gol, nu rupe rândul.
    public string PartenerCodFiscal { get; set; }

    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    // STRING pe sârmă, ca `Stare`/`TipStoc`/`Sens` în restul proiecțiilor:
    // contractul nu depinde de ordinea membrilor unui enum, iar filtrarea din
    // grilă vine tot ca text.
    public string Regim { get; set; }
    // SNAPSHOT-uri (JT-D3): au intrat în aritmetica bazei și a TVA-ului, deci se
    // citesc de pe RÂND, nu din nomenclatorul de azi.
    public decimal Cota { get; set; }
    // ETICHETĂ, deci join la CITIRE (JT-D3) — și DIRECȚIONALĂ: codul ANAF diferă
    // între livrare și achiziție, iar nomenclatorul se schimbă cu anul de
    // raportare (o declarație se generează cu nomenclatorul în vigoare atunci).
    public string CodSafT { get; set; }

    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
}

// Un rând de decont = o pereche (Sens × TipTva), peste o perioadă.
public sealed class DecontTvaRand {
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public string CodSafT { get; set; }
    // Câte rânduri de REGISTRU (linii de document) stau în spatele cifrei —
    // urma spre granularitatea SAF-T, nu o cifră de declarație.
    public int Randuri { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
}

public static class TvaProiectii {

    // ── Jurnalul de cumpărări / de vânzări (JT-D7) ──────────────────────────
    //
    // Un singur ecran parametrizat pe `Sens`, ca `PLT`/`INC` (57a): jurnalul de
    // cumpărări și cel de vânzări sunt aceeași proiecție pe laturi diferite.
    // `sens` e OBLIGATORIU — un jurnal fără sens ar amesteca cumpărările cu
    // vânzările, deci n-are default (refuzul e în controller, cu 400).
    //
    // `dataStart`/`dataEnd` sunt filtre SIMPLE (ca la registrul-jurnal, R-D9),
    // nu granițe de agregare: un jurnal n-are noțiune de „sold inițial". De aceea
    // sunt opționale — și de aceea filtrarea se face pe RÂNDURI, ÎNAINTEA
    // grupării, ceea ce dă exact comportamentul cerut de JT-D5: rândul de storno
    // cade în luna stornării, iar jurnalul lunii deja declarate rămâne cum a fost
    // declarat.
    public static IQueryable<JurnalTvaRand> JurnalTva(
        IObjectSpace os, SensTva sens, DateOnly? dataStart = null, DateOnly? dataEnd = null) {

        var randuri = os.GetObjectsQuery<RegistruTva>().Where(r => r.Sens == sens);
        if (dataStart is DateOnly ds) randuri = randuri.Where(r => r.Data >= ds);
        if (dataEnd is DateOnly de) randuri = randuri.Where(r => r.Data <= de);

        // ═══ Cheia de grupare: (Document × TipTva) — plus snapshot-urile ═══
        // Contractul JT-D7 e „un rând per (Document × TipTva)". `PartenerId`,
        // `Regim` și `Cota` apar totuși în cheie, și NU lărgesc mulțimea: sunt
        // FUNCȚIONAL DETERMINATE de ea prin construcție — toate rândurile fiscale
        // ale unui document se scriu într-o singură operare, din același `TipTva`,
        // iar `Storneaza` copiază snapshot-ul rândului original (nu re-derivă din
        // politica de azi — MotorOperare, comentariul de la rândurile inverse).
        //
        // De ce în cheie și nu ca agregate: sunt `Guid?`, enum și decimal. Postgres
        // n-are `MIN(uuid)`, iar un `MIN` pe enum ar fi un pariu pe traducere — pe
        // când cheia e o coloană simplă, tradusă întotdeauna. Bonus semantic: un
        // grup nu poate purta NICIODATĂ un snapshot care nu e al lui; dacă vreodată
        // invariantul de mai sus s-ar rupe, jurnalul ar arăta două rânduri onest
        // etichetate, nu unul cu snapshot-ul ales la întâmplare de un `MIN`.
        //
        // `Data` e singura care NU poate intra în cheie: pe un document stornat ea
        // CHIAR variază (originalul la data operării, inversul la data stornării).
        // `Min` peste mulțimea deja FILTRATĂ dă exact ce trebuie — luna curentă a
        // rândurilor care au rămas după filtru.
        var agregate = randuri
            .GroupBy(r => new { r.DocumentId, r.TipTvaId, r.PartenerId, r.Regim, r.Cota })
            .Select(g => new {
                g.Key.DocumentId,
                g.Key.TipTvaId,
                g.Key.PartenerId,
                g.Key.Regim,
                g.Key.Cota,
                Data = g.Min(r => r.Data),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            });

        // Codul ANAF e direcțional (JT-D3): `sens` e constant pe toată interogarea,
        // deci decizia se ia o dată, aici, și intră în SQL ca ramură de `CASE` pe un
        // parametru — nu ca două proiecții aproape identice.
        var esteAchizitie = sens == SensTva.Achizitie;

        // ═══ Join-urile de etichetă sunt LEFT, toate trei ═══
        // Lecția review-ului D4 al feliei 9, cuvânt cu cuvânt: un rând NU are voie
        // să dispară dintr-un raport fiindcă i-a dispărut ETICHETA. Cu INNER, o
        // factură al cărei partener a fost șters (soft delete) sau făcut invizibil
        // de securitate ar fi ieșit tăcut din jurnal, iar totalurile ar fi mințit —
        // exact în raportul care ajunge într-o declarație fiscală. Cu LEFT, rândul
        // rămâne cu eticheta goală, iar clientul îl poate marca onest.
        // `TipTva` la fel: e nomenclator editabil, deci ștergibil.
        return from a in agregate
               join d in os.GetObjectsQuery<Document>() on a.DocumentId equals d.ID into grupDoc
               from d in grupDoc.DefaultIfEmpty()
               join p in os.GetObjectsQuery<Repartitor>()
                   on a.PartenerId equals (Guid?)p.ID into grupPartener
               from p in grupPartener.DefaultIfEmpty()
               join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
               from t in grupTip.DefaultIfEmpty()
               select new JurnalTvaRand {
                   DocumentId = a.DocumentId,
                   DocumentNumar = d == null ? null : d.Numar,
                   // Se completează în memorie peste pagină (R-D8) — vezi mai sus.
                   DocumentTip = null,
                   Data = a.Data,
                   PartenerId = a.PartenerId,
                   PartenerDenumire = p == null ? null : p.Denumire,
                   PartenerCodFiscal = p == null ? null : (p as Partener).CodFiscal,
                   TipTvaId = a.TipTvaId,
                   TipTvaCod = t == null ? null : t.Cod,
                   TipTvaDenumire = t == null ? null : t.Denumire,
                   // Enum → string ÎN SQL (`CASE`), ca `TipStoc` pe `SoldStoc`:
                   // filtrarea și sortarea din grilă rămân server-side. Lanțul e
                   // scris INLINE, în ambele proiecții, deliberat: o metodă
                   // statică n-ar fi traductibilă, iar un `Expression` partajat
                   // ar cere LINQKit, respins la 42c. Cele două copii se schimbă
                   // ÎMPREUNĂ — și acoperă TOATE valorile `RegimTva`, ca un
                   // membru nou să nu se strecoare tăcut pe ultima ramură.
                   Regim = a.Regim == RegimTva.Normal ? "Normal"
                       : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                       : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                       : a.Regim == RegimTva.Scutit ? "Scutit"
                       : "Neimpozabil",
                   Cota = a.Cota,
                   CodSafT = t == null ? null : (esteAchizitie ? t.CodSafTAchizitie : t.CodSafTLivrare),
                   Baza = a.Baza,
                   Tva = a.Tva
               };
    }

    // Ordinea jurnalului, DECLARATĂ — și TOTALĂ (lecția dublă a feliei 9).
    //
    // Declarată, fiindcă `DataSourceLoader` își inventează o sortare când cererea
    // n-are `sort=` (primul membru numit „Id"), iar EF o compilează ca `OrderBy`,
    // care ȘTERGE ordonarea proiecției (demonstrația mecanică, cu sursele citate:
    // `Proiectii/OrdineLista.cs`). Aici e doar un DEFAULT: `sort=` de la client are
    // prioritate, iar selectorii de mai jos se adaugă ca tiebreak — un jurnal n-are
    // sold curent de rupt, deci grila poate sorta ce vrea.
    //
    // Totală, fiindcă `ORDER BY` pe cheie ne-unică sub `LIMIT/OFFSET` n-are ordine
    // garantată: un rând poate apărea pe două pagini sau pe niciuna. `Data` întâi
    // (un jurnal se citește cronologic), apoi cheia de grupare — care e chiar
    // (Document × TipTva), din motivul scris la `GroupBy`.
    public static SortingInfo[] OrdineJurnalTva() => new[] {
        OrdineLista.Crescator(nameof(JurnalTvaRand.Data)),
        OrdineLista.Crescator(nameof(JurnalTvaRand.DocumentId)),
        OrdineLista.Crescator(nameof(JurnalTvaRand.TipTvaId))
    };

    // ── Decontul (JT-D7) ────────────────────────────────────────────────────
    //
    // ═══ Cheia e `TipTva`, NU (Regim × Cota) — și asta e load-bearing ═══
    // Prima formulare a designului spunea „per (Sens × Regim × Cota)", și ar fi
    // fost greșită: `SDD` (scutit CU drept de deducere) și `SFD` (scutit FĂRĂ
    // drept) au ACELAȘI regim și aceeași cotă 0, dar coduri SAF-T diferite
    // (310314 vs 310326) și rânduri diferite în D300. O grupare pe regim×cotă
    // le-ar fi FUZIONAT, adică ar fi produs exact cifra pe care declarația n-o
    // poate folosi.
    // `TipTva` E identitatea de raportare — el poartă mapările ANAF — deci el e
    // cheia. Regimul și cota rămân coloane afișate.
    //
    // Ambele date sunt filtre simple, ca la jurnal (un decont pe o perioadă e o
    // însumare peste rândurile ei, nu o agregare cu graniță internă).
    public static IQueryable<DecontTvaRand> DecontTva(
        IObjectSpace os, DateOnly? dataStart = null, DateOnly? dataEnd = null) {

        var randuri = os.GetObjectsQuery<RegistruTva>();
        if (dataStart is DateOnly ds) randuri = randuri.Where(r => r.Data >= ds);
        if (dataEnd is DateOnly de) randuri = randuri.Where(r => r.Data <= de);

        // Snapshot-urile intră în cheie din același motiv tehnic ca la jurnal
        // (enum și decimal, nu agregate) — dar aici cu o nuanță semantică proprie,
        // fiindcă mulțimea se întinde peste MAI MULTE documente: dacă `Cota` unui
        // `TipTva` a fost editată între două luni ale perioadei, rândurile vechi
        // poartă vechea cotă. Cheia le ține SEPARATE, ceea ce e chiar ce cere D300
        // (liniile lui sunt per cotă) — spre deosebire de un `MIN`, care ar fi
        // etichetat toată suma cu una dintre cote, la întâmplare.
        var agregate = randuri
            .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota })
            .Select(g => new {
                g.Key.Sens,
                g.Key.TipTvaId,
                g.Key.Regim,
                g.Key.Cota,
                Randuri = g.Count(),
                Baza = g.Sum(r => r.Baza),
                Tva = g.Sum(r => r.Tva)
            });

        // LEFT, ca peste tot: `TipTva` e nomenclator editabil, iar o cifră de
        // decont nu are voie să dispară fiindcă i-a dispărut eticheta.
        return from a in agregate
               join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
               from t in grupTip.DefaultIfEmpty()
               select new DecontTvaRand {
                   Sens = a.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                   TipTvaId = a.TipTvaId,
                   TipTvaCod = t == null ? null : t.Cod,
                   TipTvaDenumire = t == null ? null : t.Denumire,
                   // A doua copie a lanțului — vezi motivul la jurnal.
                   Regim = a.Regim == RegimTva.Normal ? "Normal"
                       : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                       : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                       : a.Regim == RegimTva.Scutit ? "Scutit"
                       : "Neimpozabil",
                   Cota = a.Cota,
                   // Direcțional (JT-D3), aici cu `Sens` VARIABIL per grup — deci
                   // un `CASE` adevărat, nu o ramură pe parametru ca la jurnal.
                   CodSafT = t == null ? null
                       : (a.Sens == SensTva.Achizitie ? t.CodSafTAchizitie : t.CodSafTLivrare),
                   Randuri = a.Randuri,
                   Baza = a.Baza,
                   Tva = a.Tva
               };
    }

    // Ordinea decontului — declarată și TOTALĂ, ca la jurnal. `Sens` și `TipTvaId`
    // sunt cheia de raportare; `Cota` și `Regim` o completează, fiindcă aici (spre
    // deosebire de jurnal, unde un document are o singură operare) mulțimea se
    // întinde peste mai multe documente și un snapshot editat între timp poate
    // CHIAR produce două rânduri pe același `TipTva` — vezi `GroupBy`.
    public static SortingInfo[] OrdineDecontTva() => new[] {
        OrdineLista.Crescator(nameof(DecontTvaRand.Sens)),
        OrdineLista.Crescator(nameof(DecontTvaRand.TipTvaId)),
        OrdineLista.Crescator(nameof(DecontTvaRand.Cota)),
        OrdineLista.Crescator(nameof(DecontTvaRand.Regim))
    };
}
