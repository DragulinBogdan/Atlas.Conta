using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Atlas.Conta.BackOffice.Module.Anaf;

// CLIENTUL REGISTRULUI ANAF „PlatitorTva" (felia 15, D15-D2) — singurul loc din
// repo care vorbește HTTP către afară.
//
// DE CE o clasă concretă, fără interfață și fără DI în Module: modulele XAF nu
// înregistrează servicii (constatare a explorării — tot ce e „serviciu" în
// Module e `static class` + `IObjectSpace`), iar o interfață cu o singură
// implementare n-ar apăra nimic: proba de deserializare (D15-V3) rulează pe
// fixture-uri prin `Interpreteaza`, adică pe EXACT calea pe care o folosește și
// `Interogheaza` — nu pe un dublu de test. Host-urile o construiesc: Blazor și
// WebApi prin `AddHttpClient<PlatitorTvaClient>()`, Import1C prin `new`.
//
// CONTRACTUL OFICIAL v9 (`doc_WS_V9.txt`), VERIFICAT pe date reale la
// implementare (2026-08-25, CUI 4221306 și 14399840):
//   POST https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva
//   body `[{ "cui": <NUMĂR>, "data": "AAAA-LL-ZZ" }]`, maxim 100 intrări/apel,
//   maxim 1 apel/secundă;
//   răspuns `{ found: [...], notFound: [<numere>] }`.
// Constatarea care contează: răspunsul REAL v9 **nu conține `cod`/`message`**
// (contractul le dă opționale — corect). De-aia deserializarea nu se sprijină
// pe ele: prezența lor e tolerată, absența e normalul.
//
// CE NU FACE: retry. Un lot eșuat iese ca dată (`EroareLotAnaf`), clasificat
// tranzitoriu/fatal, iar apelantul decide — REST-ul întoarce 503 pe tranzitoriu
// (D15-D4), Import1C reia lotul o dată (D15-D6). Un retry ascuns în client ar
// dubla apelurile pe secundă exact când ANAF ne cere să încetinim.
public class PlatitorTvaClient {
    public const string UrlImplicit = "https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva";

    // Plafoanele publicate de ANAF. Publice fiindcă apelanții își dimensionează
    // loturile după ele (D15-D4: 500 de ID-uri ≈ 5 apeluri ≈ 5 s).
    public const int MaximPerLot = 100;
    public static readonly TimeSpan PauzaIntreApeluri = TimeSpan.FromSeconds(1);

    readonly HttpClient http;
    readonly string url;

    public PlatitorTvaClient(HttpClient http, string url = UrlImplicit) {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.url = string.IsNullOrWhiteSpace(url) ? UrlImplicit : url;
    }

    // Opțiunile de (de)serializare: numele de pe sârmă sunt declarate explicit
    // pe DTO-uri (`[JsonPropertyName]`), iar câmpurile necunoscute se ignoră —
    // comportamentul implicit al `System.Text.Json`, pe care ne bazăm DELIBERAT:
    // ANAF adaugă câmpuri în răspuns fără să schimbe versiunea (`act`,
    // `statusRO_e_Factura`, `organFiscalCompetent` au apărut așa), iar un client
    // care ar pica pe ele ar fi o bombă cu ceas.
    static readonly JsonSerializerOptions Optiuni = new() {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // Interogarea completă: împarte în loturi de 100, așteaptă ≥ 1 s între
    // apeluri, NU paralelizează, iar un lot eșuat nu-i oprește pe ceilalți.
    //
    // `ConfigureAwait(false)` peste tot: acțiunea XAF (D15-D5) rulează metoda
    // asta SINCRON (Blazor XAF e sincron), iar o continuare care ar cere înapoi
    // SynchronizationContext-ul rendererului ar bloca circuitul. Într-o
    // bibliotecă e oricum forma corectă.
    public async Task<RaspunsPlatitorTva> Interogheaza(
            IReadOnlyList<long> cuiuri, DateOnly data, CancellationToken ct) {
        var gasiti = new List<DateAnaf>();
        var negasiti = new List<long>();
        var erori = new List<EroareLotAnaf>();
        if (cuiuri == null || cuiuri.Count == 0)
            return new RaspunsPlatitorTva(gasiti, negasiti, erori);

        // Deduplicare păstrând ordinea: două CUI-uri identice în același lot ar
        // consuma degeaba din bugetul de 100 și ar putea întoarce aceeași
        // intrare de două ori în `found`. Apelantul (serviciul) mapează oricum
        // CUI → parteneri, deci n-are nevoie de duplicate pe sârmă.
        var unice = new List<long>();
        var vazute = new HashSet<long>();
        foreach (var cui in cuiuri)
            if (vazute.Add(cui))
                unice.Add(cui);

        var ziua = data.ToString("yyyy-MM-dd");
        for (var start = 0; start < unice.Count; start += MaximPerLot) {
            var lot = unice.GetRange(start, Math.Min(MaximPerLot, unice.Count - start));
            if (start > 0)
                await Task.Delay(PauzaIntreApeluri, ct).ConfigureAwait(false);
            var (dateLot, negasitiLot, eroare) = await UnLot(lot, ziua, ct).ConfigureAwait(false);
            gasiti.AddRange(dateLot);
            negasiti.AddRange(negasitiLot);
            if (eroare != null)
                erori.Add(eroare);
        }
        return new RaspunsPlatitorTva(gasiti, negasiti, erori);
    }

    async Task<(IReadOnlyList<DateAnaf> Gasiti, IReadOnlyList<long> Negasiti, EroareLotAnaf Eroare)> UnLot(
            IReadOnlyList<long> lot, string ziua, CancellationToken ct) {
        var cerere = lot.Select(cui => new CerereSarma { Cui = cui, Data = ziua }).ToList();
        string continut;
        try {
            using var raspuns = await http.PostAsJsonAsync(url, cerere, Optiuni, ct).ConfigureAwait(false);
            if (!raspuns.IsSuccessStatusCode) {
                // Clasificarea cerută de D15-D2, cu precizarea din riscul 5 al
                // contractului: 429 („Too Many Requests") e TRANZITORIU deși e
                // 4xx — e chiar semnalul de rate-limit pe care contractul îl
                // anticipează când două cereri concurente ating ANAF în aceeași
                // secundă. Restul de 4xx (400 pe body greșit, 404 pe URL mutat)
                // sunt fatale: reluarea le-ar repeta identic.
                var tranzitorie = (int)raspuns.StatusCode >= 500
                    || raspuns.StatusCode == HttpStatusCode.TooManyRequests;
                return ([], [], new EroareLotAnaf(lot,
                    $"ANAF a răspuns {(int)raspuns.StatusCode} {raspuns.ReasonPhrase}.", tranzitorie));
            }
            continut = await raspuns.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) {
            // Anularea CERUTĂ de apelant nu e o eroare a lotului.
            throw;
        }
        catch (OperationCanceledException ex) {
            // `HttpClient.Timeout` ajunge tot aici (TaskCanceledException cu
            // TimeoutException înăuntru), fără ca `ct` să fie semnalat.
            return ([], [], new EroareLotAnaf(lot, $"Interogarea ANAF a expirat: {ex.Message}", true));
        }
        catch (HttpRequestException ex) {
            return ([], [], new EroareLotAnaf(lot, $"Rețea/ANAF indisponibil: {ex.Message}", true));
        }

        try {
            var (gasiti, negasiti, intrariStricate) = Interpreteaza(continut);
            var eroare = intrariStricate == 0 ? null : new EroareLotAnaf(lot,
                $"{intrariStricate} intrări din răspunsul ANAF n-au `date_generale` — ignorate.", false);
            return (gasiti, negasiti, eroare);
        }
        catch (JsonException ex) {
            return ([], [], new EroareLotAnaf(lot, $"Răspuns ANAF neinterpretabil: {ex.Message}", false));
        }
    }

    // Seam-ul de deserializare, PUBLIC: `Interogheaza` trece prin el, iar
    // D15-V3 îl probează pe fixture-uri salvate din răspunsuri reale. Așa proba
    // stă pe calea reală (66h) fără să atingă rețeaua — suita rămâne offline și
    // deterministă.
    //
    // `IntrariStricate` = intrări din `found` fără `date_generale` (deci fără
    // CUI): nu pot fi mapate pe niciun partener. Se numără, nu se ghicesc.
    public static (IReadOnlyList<DateAnaf> Gasiti, IReadOnlyList<long> Negasiti, int IntrariStricate)
            Interpreteaza(string json) {
        var raspuns = JsonSerializer.Deserialize<RaspunsSarma>(json, Optiuni);
        if (raspuns == null)
            return ([], [], 0);
        var gasiti = new List<DateAnaf>();
        var stricate = 0;
        foreach (var intrare in raspuns.Found ?? []) {
            var date = intrare?.DateGenerale;
            if (date == null) {
                stricate++;
                continue;
            }
            gasiti.Add(new DateAnaf(
                Cui: date.Cui,
                Denumire: date.Denumire,
                NrRegCom: date.NrRegCom,
                CodPostal: date.CodPostal,
                StareInregistrare: date.StareInregistrare,
                // `bool?`, nu `bool`: dacă blocul lipsește cu totul din răspuns,
                // „nu a spus" ≠ „a spus nu". Regula canonică din D15-D3 scrie
                // doar ce ANAF a declarat; absența iese ca avertisment.
                ScpTva: intrare.ScopTva?.ScpTva,
                TvaLaIncasare: intrare.Rtvai?.StatusTvaIncasare,
                Inactiv: intrare.StareInactiv?.StatusInactivi,
                DomiciliuFiscal: intrare.AdresaDomiciliuFiscal?.Catre(),
                SediuSocial: intrare.AdresaSediuSocial?.Catre()));
        }
        return (gasiti, raspuns.NotFound ?? [], stricate);
    }

    // ---------------- DTO-urile de sârmă (doar câmpurile CONSUMATE) ----------------
    // D15-D2: „răspunsul se deserializează într-un DTO care ține doar câmpurile
    // consumate + `cui`". `iban`, `cod_CAEN`, `forma_juridica`,
    // `statusRO_e_Factura`, `inregistrare_SplitTVA`, `perioade_TVA` etc. EXISTĂ
    // în răspuns și rămân DELIBERAT nemapate: niciun formular de azi nu le cere
    // (§„Ce NU intră"), iar o coloană pe care n-o umple nimeni e o coloană care
    // minte.

    sealed class CerereSarma {
        // NUMĂR, nu string — contractul v9, confirmat pe apelul real.
        [JsonPropertyName("cui")] public long Cui { get; set; }
        [JsonPropertyName("data")] public string Data { get; set; }
    }

    sealed class RaspunsSarma {
        // Opționale: răspunsul v9 real NU le trimite. Rămân mapate ca să nu
        // pice deserializarea dacă ANAF le reintroduce; nimic nu le citește.
        [JsonPropertyName("cod")] public int? Cod { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("found")] public List<IntrareSarma> Found { get; set; }
        [JsonPropertyName("notFound")] public List<long> NotFound { get; set; }
    }

    sealed class IntrareSarma {
        [JsonPropertyName("date_generale")] public DateGeneraleSarma DateGenerale { get; set; }
        [JsonPropertyName("inregistrare_scop_Tva")] public ScopTvaSarma ScopTva { get; set; }
        [JsonPropertyName("inregistrare_RTVAI")] public RtvaiSarma Rtvai { get; set; }
        [JsonPropertyName("stare_inactiv")] public StareInactivSarma StareInactiv { get; set; }
        [JsonPropertyName("adresa_sediu_social")] public AdresaSediuSarma AdresaSediuSocial { get; set; }
        [JsonPropertyName("adresa_domiciliu_fiscal")] public AdresaDomiciliuSarma AdresaDomiciliuFiscal { get; set; }
    }

    sealed class DateGeneraleSarma {
        [JsonPropertyName("cui")] public long Cui { get; set; }
        [JsonPropertyName("denumire")] public string Denumire { get; set; }
        [JsonPropertyName("nrRegCom")] public string NrRegCom { get; set; }
        [JsonPropertyName("codPostal")] public string CodPostal { get; set; }
        [JsonPropertyName("stare_inregistrare")] public string StareInregistrare { get; set; }
    }

    sealed class ScopTvaSarma {
        [JsonPropertyName("scpTVA")] public bool? ScpTva { get; set; }
    }

    sealed class RtvaiSarma {
        [JsonPropertyName("statusTvaIncasare")] public bool? StatusTvaIncasare { get; set; }
    }

    sealed class StareInactivSarma {
        [JsonPropertyName("statusInactivi")] public bool? StatusInactivi { get; set; }
    }

    // Cele două adrese poartă ACELEAȘI câmpuri cu prefix diferit (`s` = sediu
    // social, `d` = domiciliu fiscal) — de-aia două clase, nu una: `System.Text
    // .Json` leagă pe nume, iar numele diferă. Aplatizarea în `AdresaAnaf` le
    // face din nou una singură, ca `Aplica` să nu știe de prefixe.
    sealed class AdresaSediuSarma {
        [JsonPropertyName("sdenumire_Strada")] public string Strada { get; set; }
        [JsonPropertyName("snumar_Strada")] public string Numar { get; set; }
        [JsonPropertyName("sdenumire_Localitate")] public string Localitate { get; set; }
        [JsonPropertyName("scod_JudetAuto")] public string CodJudetAuto { get; set; }
        [JsonPropertyName("scod_Postal")] public string CodPostal { get; set; }
        [JsonPropertyName("sdetalii_Adresa")] public string Detalii { get; set; }
        public AdresaAnaf Catre() => new(Strada, Numar, Localitate, CodJudetAuto, CodPostal, Detalii);
    }

    sealed class AdresaDomiciliuSarma {
        [JsonPropertyName("ddenumire_Strada")] public string Strada { get; set; }
        [JsonPropertyName("dnumar_Strada")] public string Numar { get; set; }
        [JsonPropertyName("ddenumire_Localitate")] public string Localitate { get; set; }
        [JsonPropertyName("dcod_JudetAuto")] public string CodJudetAuto { get; set; }
        [JsonPropertyName("dcod_Postal")] public string CodPostal { get; set; }
        [JsonPropertyName("ddetalii_Adresa")] public string Detalii { get; set; }
        public AdresaAnaf Catre() => new(Strada, Numar, Localitate, CodJudetAuto, CodPostal, Detalii);
    }
}

// ---------------- Rezultatul, aplatizat (ce vede D15-D3) ----------------

// Datele unui partener, așa cum le-a declarat ANAF. Câmpurile de statut sunt
// `bool?`: `null` = ANAF n-a raportat (blocul lipsea din răspuns), și atunci
// regula „canonicul bate" NU se aplică — nu ghicim un „nu" dintr-o tăcere.
public sealed record DateAnaf(
    long Cui,
    string Denumire,
    string NrRegCom,
    string CodPostal,
    string StareInregistrare,
    bool? ScpTva,
    bool? TvaLaIncasare,
    bool? Inactiv,
    AdresaAnaf DomiciliuFiscal,
    AdresaAnaf SediuSocial);

// Adresa ANAF, fără prefixul sursei. `CodJudetAuto` = indicativul auto („CJ",
// „B"), pe care `JudeteRo.DupaCodAuto` îl duce la codul ISO al nomenclatorului.
public sealed record AdresaAnaf(
    string Strada,
    string Numar,
    string Localitate,
    string CodJudetAuto,
    string CodPostal,
    string Detalii);

// Un lot eșuat, ca DATĂ. `Tranzitorie` decide ce face apelantul: REST-ul dă 503
// (clientul poate reîncerca) vs. 422 (n-are rost), Import1C reia o dată.
public sealed record EroareLotAnaf(IReadOnlyList<long> Lot, string Mesaj, bool Tranzitorie);

public sealed record RaspunsPlatitorTva(
    IReadOnlyList<DateAnaf> Gasiti,
    IReadOnlyList<long> Negasiti,
    IReadOnlyList<EroareLotAnaf> Erori);
