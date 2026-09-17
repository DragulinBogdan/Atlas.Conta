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
    // Perioada de DECLARARE (F27-D5) — cea pe care jurnalul FILTREAZĂ. Diferă de
    // `Data` doar pentru faptele înregistrate după închiderea lunii lor.
    public int PerioadaAn { get; set; }
    public int PerioadaLuna { get; set; }

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
    // Rândul provine din STORNAREA documentului, nu din operarea lui (JT-D5).
    // E în CHEIA de grupare, nu doar o coloană — vezi `JurnalTva`.
    public bool Storno { get; set; }
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

// Un rând al conținutului de rectificativă: fapt fiscal declarat în perioada P,
// dar scris după ce P fusese închisă prima oară (F27-D5).
public sealed class RectificativaTvaRand {
    public Guid DocumentId { get; set; }
    public string DocumentNumar { get; set; }
    public DateOnly DocumentData { get; set; }
    // Data faptului fiscal (a documentului, ori a stornării pe rândul invers).
    public DateOnly Data { get; set; }
    public string Sens { get; set; }
    public Guid TipTvaId { get; set; }
    public string TipTvaCod { get; set; }
    public string TipTvaDenumire { get; set; }
    public string Regim { get; set; }
    public decimal Cota { get; set; }
    public decimal Baza { get; set; }
    public decimal Tva { get; set; }
    public bool Storno { get; set; }
    public DateTime ScrisLa { get; set; }
}

// Conținutul de rectificativă al unei perioade — DERIVAT, nu flag (F27-D5).
public sealed class RectificativaTva {
    public int An { get; set; }
    public int Luna { get; set; }
    // Reperul: momentul primei închideri. Redeschiderea NU îl șterge, deci ce a
    // fost declarat o dată rămâne reperul.
    public DateTime? InchisaPrimaOara { get; set; }
    // Perioada e DESCHISĂ acum (redeschisă după prima declarare, ori încă
    // nedefinită ca închidere): conținutul de mai jos devine rectificativă abia
    // la re-închidere, iar ecranul o spune.
    public bool PerioadaDeschisa { get; set; }
    public bool EsteRectificativa { get; set; }
    public List<RectificativaTvaRand> Randuri { get; set; } = [];
    // Pe cheia DECONTULUI (Sens × TipTva × Regim × Cotă) — „diferențele față de
    // declarat" se citesc în aceleași coordonate ca decontul depus.
    public List<DecontTvaRand> Agregat { get; set; } = [];
}

public static class TvaProiectii {

    // ── Filtrul de perioadă al TUTUROR consumatorilor fiscali (F27-D5) ──────
    //
    // Jurnalele, decontul, D300, D394 și SAF-T filtrează pe PERIOADA DE
    // DECLARARE, nu pe data faptului: un fapt înregistrat după închiderea lunii
    // lui se declară acolo unde spune politica, iar raportul lunii trebuie să-l
    // conțină. Capetele rămân DATE (perioada cerută de ecran e tot un interval
    // de zile) și se reduc la luna lor; ambele opționale, ca la jurnal.
    //
    // `An * 100 + Luna` e aritmetică pe două coloane `int`, deci se traduce
    // integral în SQL — spre deosebire de orice construcție `DateOnly` peste ele.
    public static IQueryable<RegistruTva> IntreLuni(
        IQueryable<RegistruTva> randuri, DateOnly? dataStart, DateOnly? dataEnd) {

        if (dataStart is DateOnly ds) {
            var de = ds.Year * 100 + ds.Month;
            randuri = randuri.Where(r => r.PerioadaAn * 100 + r.PerioadaLuna >= de);
        }
        if (dataEnd is DateOnly df) {
            var panaLa = df.Year * 100 + df.Month;
            randuri = randuri.Where(r => r.PerioadaAn * 100 + r.PerioadaLuna <= panaLa);
        }
        return randuri;
    }

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

        var randuri = IntreLuni(
            os.GetObjectsQuery<RegistruTva>().Where(r => r.Sens == sens), dataStart, dataEnd);

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
        // ═══ `Storno` E ÎN CHEIE — și ăsta e miezul (review advers D3) ═══
        // Spre deosebire de celelalte trei, `Storno` NU e funcțional determinat de
        // pereche: un document stornat are AMBELE seturi, iar ele sunt două FAPTE
        // FISCALE DISTINCTE, la date diferite. Fără el în cheie, o interogare care
        // cuprinde și operarea, și stornarea (banalul „jurnalul pe anul 2025") le
        // neta într-un singur rând de zero lei, datat în luna operării: factura
        // apărea ca „factură de 0,00", iar stornarea dispărea complet ca eveniment.
        // Totalurile perioadei rămâneau corecte — se pierdea exact granularitatea
        // per document pe care o cere D394.
        //
        // Corolar: `Data = Min` redevine EXACTĂ. Într-un grup, toate rândurile
        // originale poartă `doc.Data` și toate inversele `dataStorno`, deci minimul
        // e chiar data grupului, nu o alegere între două date diferite.
        //
        // `PerioadaAn`/`PerioadaLuna` intră în cheie din același motiv ca `Storno`
        // (F27-D5): jurnalul filtrează pe ele, iar un grup care ar amesteca două
        // perioade de declarare (corecția cu motiv, F27-D6) ar cădea întreg în
        // fereastra oricăreia dintre ele.
        var agregate = randuri
            .GroupBy(r => new {
                r.DocumentId, r.TipTvaId, r.PartenerId, r.Regim, r.Cota, r.Storno,
                r.PerioadaAn, r.PerioadaLuna
            })
            .Select(g => new {
                g.Key.DocumentId,
                g.Key.TipTvaId,
                g.Key.PartenerId,
                g.Key.Regim,
                g.Key.Cota,
                g.Key.Storno,
                g.Key.PerioadaAn,
                g.Key.PerioadaLuna,
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
                   PerioadaAn = a.PerioadaAn,
                   PerioadaLuna = a.PerioadaLuna,
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
                   Tva = a.Tva,
                   Storno = a.Storno
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
        OrdineLista.Crescator(nameof(JurnalTvaRand.TipTvaId)),
        // A patra cheie, din același motiv pentru care `Storno` e în grupare: un
        // document stornat produce DOUĂ rânduri pe aceeași pereche, iar fără ea
        // ordinea n-ar mai fi totală — exact condiția sub care `LIMIT/OFFSET`
        // poate dubla sau sări un rând (lecția feliei 9).
        OrdineLista.Crescator(nameof(JurnalTvaRand.Storno))
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

        var randuri = IntreLuni(os.GetObjectsQuery<RegistruTva>(), dataStart, dataEnd);

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

    // ── Conținutul de rectificativă (F27-D5) ────────────────────────────────
    //
    // Nu există flag de rectificativă nicăieri: e DIFERENȚA dintre două
    // timestamp-uri. Un rând declarat în P și scris după ce P a fost închisă
    // prima oară e, prin definiție, o cifră pe care declarația depusă n-o
    // conținea. Perioada niciodată închisă n-are reper, deci n-are rectificativă.
    //
    // Redeschiderea NU stinge `InchisaPrimaOara` (F27-D1), deci un P redeschis
    // și reînchis își păstrează reperul original — altfel toată corecția lui ar
    // fi devenit tăcut „declarație inițială".
    //
    // Întoarce liste MATERIALIZATE, ca D300/D394: e un raport, nu o grilă.
    public static RectificativaTva Rectificativa(IObjectSpace os, int an, int luna) {
        var rezultat = new RectificativaTva { An = an, Luna = luna };
        var perioada = os.GetObjectsQuery<PerioadaFiscala>()
            .Where(p => p.An == an && p.Luna == luna)
            .Select(p => new { p.InchisaPrimaOara, p.Inchisa })
            .FirstOrDefault();
        rezultat.InchisaPrimaOara = perioada?.InchisaPrimaOara;
        rezultat.PerioadaDeschisa = perioada is { Inchisa: false };
        if (rezultat.InchisaPrimaOara is not DateTime reper)
            return rezultat;

        var randuri = os.GetObjectsQuery<RegistruTva>()
            .Where(r => r.PerioadaAn == an && r.PerioadaLuna == luna && r.ScrisLa > reper);

        // LEFT pe amândouă, ca peste tot: un rând nu iese dintr-un raport fiscal
        // fiindcă i-a dispărut eticheta.
        rezultat.Randuri = (from r in randuri
                            join d in os.GetObjectsQuery<Document>() on r.DocumentId equals d.ID into grupDoc
                            from d in grupDoc.DefaultIfEmpty()
                            join t in os.GetObjectsQuery<TipTva>() on r.TipTvaId equals t.ID into grupTip
                            from t in grupTip.DefaultIfEmpty()
                            select new RectificativaTvaRand {
                                DocumentId = r.DocumentId,
                                DocumentNumar = d == null ? null : d.Numar,
                                DocumentData = d == null ? default : d.Data,
                                Data = r.Data,
                                Sens = r.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                                TipTvaId = r.TipTvaId,
                                TipTvaCod = t == null ? null : t.Cod,
                                TipTvaDenumire = t == null ? null : t.Denumire,
                                Regim = r.Regim == RegimTva.Normal ? "Normal"
                                    : r.Regim == RegimTva.Capitalizat ? "Capitalizat"
                                    : r.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                                    : r.Regim == RegimTva.Scutit ? "Scutit"
                                    : "Neimpozabil",
                                Cota = r.Cota,
                                Baza = r.Baza,
                                Tva = r.Tva,
                                Storno = r.Storno,
                                ScrisLa = r.ScrisLa
                            })
            .OrderBy(x => x.ScrisLa).ThenBy(x => x.DocumentId).ThenBy(x => x.TipTvaId)
            .ToList();
        rezultat.EsteRectificativa = rezultat.Randuri.Count > 0;

        rezultat.Agregat = (from a in randuri
                                .GroupBy(r => new { r.Sens, r.TipTvaId, r.Regim, r.Cota })
                                .Select(g => new {
                                    g.Key.Sens, g.Key.TipTvaId, g.Key.Regim, g.Key.Cota,
                                    Randuri = g.Count(), Baza = g.Sum(r => r.Baza), Tva = g.Sum(r => r.Tva)
                                })
                            join t in os.GetObjectsQuery<TipTva>() on a.TipTvaId equals t.ID into grupTip
                            from t in grupTip.DefaultIfEmpty()
                            select new DecontTvaRand {
                                Sens = a.Sens == SensTva.Achizitie ? "Achizitie" : "Livrare",
                                TipTvaId = a.TipTvaId,
                                TipTvaCod = t == null ? null : t.Cod,
                                TipTvaDenumire = t == null ? null : t.Denumire,
                                Regim = a.Regim == RegimTva.Normal ? "Normal"
                                    : a.Regim == RegimTva.Capitalizat ? "Capitalizat"
                                    : a.Regim == RegimTva.TaxareInversa ? "TaxareInversa"
                                    : a.Regim == RegimTva.Scutit ? "Scutit"
                                    : "Neimpozabil",
                                Cota = a.Cota,
                                CodSafT = t == null ? null
                                    : (a.Sens == SensTva.Achizitie ? t.CodSafTAchizitie : t.CodSafTLivrare),
                                Randuri = a.Randuri,
                                Baza = a.Baza,
                                Tva = a.Tva
                            })
            .OrderBy(x => x.Sens).ThenBy(x => x.TipTvaId).ThenBy(x => x.Cota).ThenBy(x => x.Regim)
            .ToList();
        return rezultat;
    }

    // Perioada cerută de un raport ACOPERĂ EXACT o lună calendaristică — condiția
    // sub care „rectificativă?" are înțeles (F27-D5): întrebarea e despre O
    // declarație depusă, iar un interval de mai multe luni n-are una.
    public static (int An, int Luna)? LunaExacta(DateOnly dataStart, DateOnly dataEnd) =>
        dataStart.Day == 1 && dataStart.Year == dataEnd.Year && dataStart.Month == dataEnd.Month
            && dataEnd.Day == DateTime.DaysInMonth(dataEnd.Year, dataEnd.Month)
            ? (dataStart.Year, dataStart.Month)
            : null;
}
