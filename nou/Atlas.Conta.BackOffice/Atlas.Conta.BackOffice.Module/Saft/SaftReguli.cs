using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Proiectii;

namespace Atlas.Conta.BackOffice.Module.Saft;

// FUNCȚIILE LEGII (felia 16, D16-D3) — identitatea SAF-T a unui partener, a
// societății raportoare, tipul fiscal al facturii, metoda de plată, simbolul de
// cont și tipul de cont. Toate PURE: fără `IObjectSpace`, fără bază, testabile
// direct în ModelCheck.
//
// DE CE cod, și nu politică (D16-D3): definițiile sunt ale legii (OpANAF
// 1783/2021 + nomenclatoarele ANAF), nu variază per client și nu au variantă
// „per profil" — exact ca `D394Proiectii.TipPartener`, care e precedentul de
// formă al fișierului ăstuia. Ce variază per bază e o singură decizie —
// `Societate.RaporteazaCnp` (confidențialitatea CNP-ului) — și ea intră ca
// PARAMETRU, nu ca ramură ascunsă.
public static class SaftReguli {

    // ── Constantele lui `TaxInformation` (S.TI.1) ───────────────────────────
    // „For transactions registered in GeneralLedgerEntries section which are not
    // relevant to be reported for taxs, it will be used TaxType 000 and TaxCode
    // 000000" (structura §B.6, verbatim).
    public const string TaxTypeTva = "300";
    public const string TaxTypeNefiscal = "000";
    public const string TaxCodeNefiscal = "000000";
    // Închiderea lunară de TVA (ITV): rândurile 4426/4427 = 4423/4424 nu sunt
    // operațiuni taxabile (n-au rând în `RegistruTva`), dar ating conturile de
    // TVA — codul lor e `TVA_NoteContabile` (riscul 4 al contractului).
    public const string TaxCodeInchidereTva = "380200";

    // Regimul fiscal al partenerului (`Nomenclator_Regim_fiscal`, `TaxIDStructure.TaxType`).
    public const string RegimFiscalNormal = "100010";
    public const string RegimFiscalTvaLaIncasare = "100040";

    // ── Identitatea partenerului (§B.4: prefix de tip + cod) ────────────────

    /// <summary>
    /// `CustomerID` / `SupplierID` / `RegistrationNumber` al unui PARTENER și
    /// motivul alegerii. Ordinea ramurilor e a §B.4, cu o singură precizare care
    /// nu e în text și pe care datele o cer:
    ///
    /// **prefixul `00` cere un CUI ROMÂNESC VALID**, nu doar flag-ul
    /// `InregistratTva`. Validarea sintactică a lui `00` e „max 10 cifre, fără
    /// `RO`, cu cifră de control" — deci un partener german înregistrat în DE, cu
    /// codul `DE123456789`, ar fi ieșit `00DE123456789` și ar fi fost respins.
    /// Cu gardul de față, DE-ul cu cod RO (înregistrare directă, art. 316) cade
    /// tot pe `00` (codul LUI e un CUI românesc — aceeași concluzie ca fixul 3 al
    /// D394), iar DE-ul cu cod german cade pe `01`. Consecință asumată: un
    /// partener român cu CUI greșit (typo) iese pe `04` + codul intern, cu
    /// avertisment — un identificator onest, nu unul inventat.
    /// </summary>
    public static (string Id, FelIdSaft Fel) IdPartener(
        TipPersoana tipPersoana, string tara, bool inregistratTva,
        string codFiscal, string codIntern, Guid id, bool raporteazaCnp) {

        var codTara = Partener.NormalizeazaTara(tara);
        // Aceeași normalizare ca la D394 (71b): trim, majuscule, spații scoase,
        // prefixul `RO` tăiat pe RO SAU pe înregistrat. O singură definiție.
        var cui = D394Proiectii.NormalizeazaCui(codFiscal, tara, inregistratTva);

        if (cui != null && (inregistratTva || codTara == "RO") && CuiValid(cui))
            return ("00" + cui, FelIdSaft.CuiRoman);

        // `03` e OPT-IN pe bază (`Societate.RaporteazaCnp`, D16-D1): CNP-ul e dat
        // cu caracter personal, iar formularul acceptă `04` pentru „PF care nu-și
        // declară CNP-ul pe tranzacții".
        if (tipPersoana == TipPersoana.Fizica && raporteazaCnp && CnpValid(cui))
            return ("03" + cui, FelIdSaft.Cnp);

        if (codTara != "RO") {
            var codStrain = CodStrain(codFiscal, codTara);
            if (codStrain.Length > 0) {
                var ue = EsteUe(codTara);
                var fel = ue
                    ? (inregistratTva ? FelIdSaft.UeInregistrat : FelIdSaft.UeNeinregistrat)
                    : (inregistratTva ? FelIdSaft.NonUeInregistrat : FelIdSaft.NonUeNeinregistrat);
                // Grafia de pe fișier e ISO (`CodTaraSaft`): logica de mai sus
                // lucrează pe codul CULES (ca `CodStrain` să taie și prefixul
                // `EL` din codul VIES), dar identificatorul iese într-o singură
                // grafie per țară — vezi fixul F10.
                return (Prefix(fel) + CodTaraSaft(codTara) + codStrain, fel);
            }
            // Cod gol pe un partener străin: nu există „străin fără cod" în §B.4
            // — cade pe `04` + codul intern, ca orice partener neidentificat.
        }

        var intern = new string((codIntern ?? "").Where(char.IsLetterOrDigit).ToArray());
        if (intern.Length == 0)
            intern = id.ToString("N");
        return ("04" + Taie(intern, 30), FelIdSaft.CodIntern);
    }

    /// <summary>Suprasarcina pe entitate (comoditate pentru apelanții care au deja obiectele).</summary>
    public static (string Id, FelIdSaft Fel) IdPartener(Partener p, Societate societate) =>
        IdPartener(p.TipPersoana, p.Tara, p.InregistratTva, p.CodFiscal, p.Cod, p.ID,
            societate?.RaporteazaCnp ?? false);

    static string Prefix(FelIdSaft fel) => fel switch {
        FelIdSaft.CuiRoman => "00",
        FelIdSaft.UeInregistrat => "01",
        FelIdSaft.NonUeInregistrat => "02",
        FelIdSaft.Cnp => "03",
        FelIdSaft.UeNeinregistrat => "05",
        FelIdSaft.NonUeNeinregistrat => "06",
        _ => "04",
    };

    // Codul străin, fără spații și fără prefixul de țară duplicat: exemplele ANAF
    // sunt `01EL123456789` / `01HU12345678`, adică prefix + ISO2 + codul FĂRĂ
    // literele de țară din el (VIES le scrie lipite).
    static string CodStrain(string codFiscal, string codTara) {
        var cod = new string((codFiscal ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (cod.StartsWith(codTara, StringComparison.Ordinal))
            cod = cod[codTara.Length..];
        // Grecia se scrie `GR` în ISO și `EL` în VIES: ambele grafii se taie.
        else if ((codTara is "GR" or "EL") && (cod.StartsWith("EL", StringComparison.Ordinal) || cod.StartsWith("GR", StringComparison.Ordinal)))
            cod = cod[2..];
        return new string(cod.Where(char.IsLetterOrDigit).ToArray());
    }

    static string Taie(string valoare, int maxim) =>
        valoare.Length <= maxim ? valoare : valoare[..maxim];

    /// <summary>
    /// Codul de țară pe FIȘIER: ISO 3166-1 alpha-2, cu singura corecție pe care
    /// nomenclatoarele o cer — Grecia se scrie `GR` în ISO și `EL` în VIES, iar
    /// `Partener.Tara` poate purta oricare dintre grafii (conectorul 1C aduce ce
    /// scrie în sursă).
    /// <para>
    /// MĂSURAT cu validatorul oficial (D16-V3, fixul F10 al review-ului):
    /// `01EL123456789`, `01GR123456789` și chiar `Country = EL` trec TOATE, cu 0
    /// atenționări. Deci oracolul nu impune alegerea — și tocmai de aceea trebuie
    /// făcută una: fără ea, doi parteneri greci culeși diferit ar ieși cu două
    /// grafii în ACELAȘI fișier, iar identitatea lor n-ar mai fi comparabilă. Se
    /// alege ISO (`GR`), fiindcă `Country` din `AddressStructure` e ISO 3166-1
    /// prin definiție, iar identificatorul și adresa aceluiași partener n-au
    /// niciun motiv să se contrazică.
    /// </para>
    /// </summary>
    public static string CodTaraSaft(string tara) {
        var cod = Partener.NormalizeazaTara(tara);
        return cod == "EL" ? "GR" : cod;
    }

    /// <summary>
    /// UE fără România (26 de state) — `TariUe` (D4-D1) minus RO, plus grafia
    /// `EL` a Greciei, pe care VIES o folosește ca prefix de cod de TVA.
    /// </summary>
    public static bool EsteUe(string codTara) {
        var cod = Partener.NormalizeazaTara(codTara);
        return cod != "RO" && (TariUe.Contine(cod) || cod == "EL");
    }

    // ── Societatea raportoare ───────────────────────────────────────────────

    /// <summary>
    /// `CustomerID`/`SupplierID` al RAPORTORULUI — latura liberă a fiecărui rând
    /// de GL și de plată (GL.19/GL.20 COM: „the unique code of the reporting
    /// taxpayer will be filled in"). Mereu `00` + CUI: raportorul e rezident.
    /// </summary>
    public static string IdSocietate(string codFiscal, string tara) {
        var cui = D394Proiectii.NormalizeazaCui(codFiscal, tara, true);
        return cui == null ? null : "00" + cui;
    }

    /// <summary>Suprasarcina pe entitate.</summary>
    public static string IdSocietate(Societate societate) =>
        IdSocietate(societate?.CodFiscal, societate?.Tara);

    /// <summary>
    /// `Header.Company.RegistrationNumber` (S.CMH.1) — ALTĂ regulă decât
    /// prefixele de partener: `RO`+CUI dacă societatea e înregistrată în scopuri
    /// de TVA, altfel CUI-ul gol (mesajele validatorului, §B.4).
    /// </summary>
    public static string RegistrationNumberSocietate(string codFiscal, string tara, bool inregistratTva) {
        var cui = D394Proiectii.NormalizeazaCui(codFiscal, tara, true);
        return cui == null ? null : (inregistratTva ? "RO" : "") + cui;
    }

    /// <summary>Suprasarcina pe entitate.</summary>
    public static string RegistrationNumberSocietate(Societate societate) =>
        societate == null
            ? null
            : RegistrationNumberSocietate(societate.CodFiscal, societate.Tara, societate.InregistratTva);

    /// <summary>
    /// `TaxRegistration.TaxType` al unui partener (`Nomenclator_Regim_fiscal`):
    /// doar pe partenerii identificați cu `00` (înregistrați în scopuri de TVA în
    /// România) — pentru ceilalți `TaxRegistration` lipsește cu totul.
    /// </summary>
    public static string RegimFiscalPartener(FelIdSaft fel, bool inregistratTva, bool tvaLaIncasare) {
        if (fel != FelIdSaft.CuiRoman || !inregistratTva)
            return null;
        return tvaLaIncasare ? RegimFiscalTvaLaIncasare : RegimFiscalNormal;
    }

    /// <summary>Suprasarcina pe entitate.</summary>
    public static string RegimFiscalPartener(Partener p, Societate societate) =>
        RegimFiscalPartener(IdPartener(p, societate).Fel, p.InregistratTva, p.TvaLaIncasare);

    // ── Facturi și plăți ────────────────────────────────────────────────────

    /// <summary>
    /// `InvoiceType` (S.I.9) din cele 6 coduri admise: `381` = „factură storno
    /// (factură cu semnul minus INDIFERENT de motivul stornării)" — deci și
    /// stornoul meta-operației, și retururile (care sunt storno prin construcție,
    /// 46a: valori negative pe corespondența originală). Restul: `380`.
    /// </summary>
    public static string InvoiceType(bool storno, bool esteRetur) =>
        storno || esteRetur ? "381" : "380";

    /// <summary>
    /// Tuplul `(PaymentMethod, PaymentMechanism)` — singura regulă de CORELARE
    /// din schemă pe care validatorul chiar o impune („Pentru PaymentMechanism
    /// [@0@], valoarea PaymentMethod [@1@] nu este permisa!"). Numerarul (casă,
    /// chitanță) ⇒ `01`/`10`; ordinul de plată ⇒ `03`/`42` (transfer bancar);
    /// cecul ⇒ `03`/`20`.
    /// </summary>
    public static (string PaymentMethod, string PaymentMechanism) MetodaPlata(TipInstrumentPlata instrument) =>
        instrument switch {
            TipInstrumentPlata.DispozitieCasa or TipInstrumentPlata.Chitanta => ("01", "10"),
            TipInstrumentPlata.Cec => ("03", "20"),
            _ => ("03", "42"),
        };

    // ── Planul de conturi ───────────────────────────────────────────────────

    /// <summary>
    /// `AccountID` din simbolul intern: cifrele și literele, FĂRĂ puncte
    /// (`302.02.00` ⇒ `3020200`). NU se taie niciun segment — validatorul
    /// potrivește pe sinteticul din plan prin PREFIX, deci un analitic mai lung
    /// trece; o tăiere ar pierde informație care nu se mai poate reconstitui.
    /// </summary>
    public static string SimbolSaft(string simbol) =>
        simbol == null ? null : new string(simbol.Where(char.IsLetterOrDigit).ToArray());

    /// <summary>
    /// `AccountType` (MF.GLA.7, valori în ROMÂNĂ, restricționate de schemă) din
    /// `Cont.Functie` (FCTCONT legacy). Necunoscut ⇒ `Bifunctional` — valoarea
    /// care nu minte despre sensul soldului; apelantul pune avertismentul.
    /// </summary>
    public static string TipCont(string functie) => (functie ?? "").Trim().ToUpperInvariant() switch {
        "D" => "Activ",
        "C" => "Pasiv",
        _ => "Bifunctional",
    };

    /// <summary>Funcția e recunoscută (altfel `TipCont` dă default-ul, cu avertisment).</summary>
    public static bool FunctieCunoscuta(string functie) =>
        (functie ?? "").Trim().ToUpperInvariant() is "D" or "C" or "B";

    // ── Validatoarele RO (structura §D.5) ───────────────────────────────────

    /// <summary>
    /// CUI valid: 2–10 cifre (prefixul `RO`/`R` tăiat), cheia `753217532`
    /// aliniată la DREAPTA corpului, `(Σ × 10) mod 11` cu `10 ⇒ 0`, comparat cu
    /// ultima cifră. (Probat pe `4221306` — Ministerul Finanțelor, exemplul din
    /// §B.4.)
    /// </summary>
    public static bool CuiValid(string cui) {
        if (string.IsNullOrWhiteSpace(cui))
            return false;
        var c = cui.Trim().ToUpperInvariant();
        if (c.StartsWith("RO", StringComparison.Ordinal))
            c = c[2..];
        else if (c.StartsWith("R", StringComparison.Ordinal))
            c = c[1..];
        if (c.Length is < 2 or > 10 || !c.All(char.IsAsciiDigit))
            return false;
        const string cheie = "753217532";
        var corp = c[..^1];
        var chei = cheie[^corp.Length..];
        var suma = 0;
        for (var i = 0; i < corp.Length; i++)
            suma += (corp[i] - '0') * (chei[i] - '0');
        var control = suma * 10 % 11;
        if (control == 10)
            control = 0;
        return control == c[^1] - '0';
    }

    /// <summary>
    /// CNP/NIF valid: exact 13 cifre, prima ≠ 0, cheia `279146358279` pe primele
    /// 12, `Σ mod 11` cu `10 ⇒ 1`, comparat cu a 13-a. Data nașterii NU se
    /// verifică (NIF-urile nerezidenților n-o poartă — §B.4 le pune pe același
    /// format).
    /// </summary>
    public static bool CnpValid(string cnp) {
        if (cnp == null || cnp.Length != 13 || !cnp.All(char.IsAsciiDigit) || cnp[0] == '0')
            return false;
        const string cheie = "279146358279";
        var suma = 0;
        for (var i = 0; i < 12; i++)
            suma += (cnp[i] - '0') * (cheie[i] - '0');
        var control = suma % 11;
        if (control == 10)
            control = 1;
        return control == cnp[12] - '0';
    }

    // ── Nomenclatorul mișcărilor de stoc (felia 17, D17-D2) ─────────────────

    /// <summary>
    /// Cele 19 tipuri de mișcare admise de D406 S — ACELAȘI nomenclator pentru
    /// `MovementType` ȘI pentru `MovementSubType` (xlsx `SAFT_Nomenclator`,
    /// foaia „Nomenclator stocuri"); o valoare din afara listei e eroare FATALĂ
    /// la validare, nu avertisment.
    ///
    /// Sursa UNICĂ a două lucruri: descrierea din `MovementTypeTable` (fișierul o
    /// cere lângă cod) și gardianul politicii (`PoliticaMiscareSaft.CodMiscare`).
    /// Deci cod, nu date: lista e a legii, nu variază per client și nu are
    /// variantă per profil — exact criteriul lui `IdPartener`/`MetodaPlata` de
    /// mai sus. Ce variază per client e CARE tip de document produce care cod, și
    /// aia e politica.
    ///
    /// Cheile sunt string, nu int, fiindcă asta e forma din fișier (`100` și
    /// `101` sunt coduri distincte, iar comparația se face pe text).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> CoduriMiscare =
        new Dictionary<string, string>(StringComparer.Ordinal) {
            ["10"] = "Achiziție",
            ["20"] = "Producție",
            ["30"] = "Vânzare",
            ["40"] = "Retur produse vândute",
            ["50"] = "Retur produse achiziționate",
            ["60"] = "Reduceri comerciale primite",
            ["70"] = "Consum",
            ["80"] = "Transfer intern",
            ["90"] = "Cheltuieli ulterioare capitalizate",
            ["100"] = "Diferențe de preț în plus",
            ["101"] = "Diferențe de preț în minus",
            ["110"] = "Plus de inventar",
            ["120"] = "Minus de inventar",
            ["130"] = "Ajustări pentru depreciere",
            ["140"] = "Reluări ale ajustărilor pentru depreciere",
            ["150"] = "Bunuri acordate cu titlu gratuit",
            ["160"] = "Bunuri degradate",
            ["170"] = "Bunuri expirate",
            ["180"] = "Alte tranzacții (fără cantitate)",
        };

    /// <summary>
    /// Codul e din nomenclator. Gol/null ⇒ FALS: absența codului e o decizie
    /// proprie (excludere deliberată, cu motiv), nu un cod valid — cine o admite
    /// o tratează explicit, nu prin această funcție.
    /// </summary>
    public static bool EsteCodMiscare(string cod) =>
        !string.IsNullOrEmpty(cod) && CoduriMiscare.ContainsKey(cod);
}

// Motivul pentru care un identificator a ieșit cum a ieșit — enum MIC, intern
// feliei: avertismentele și probele au nevoie de el, sârma nu (pe DTO pleacă
// identificatorul, nu ramura). De aceea NU stă în `BusinessObjects/Comun/Enums.cs`
// (unde ar fi intrat în `metadata.json` fără niciun consumator în client).
public enum FelIdSaft {
    /// <summary>`00` + CUI — operator economic din România.</summary>
    CuiRoman = 1,
    /// <summary>`01` + ISO2 + cod — operator UE înregistrat în scopuri de TVA.</summary>
    UeInregistrat = 2,
    /// <summary>`02` + ISO2 + cod — operator non-UE înregistrat.</summary>
    NonUeInregistrat = 3,
    /// <summary>`03` + CNP — persoană fizică, doar cu `Societate.RaporteazaCnp`.</summary>
    Cnp = 4,
    /// <summary>`04` + cod intern — partener neidentificat fiscal.</summary>
    CodIntern = 5,
    /// <summary>`05` + ISO2 + cod — operator UE NEînregistrat.</summary>
    UeNeinregistrat = 6,
    /// <summary>`06` + ISO2 + cod — operator non-UE NEînregistrat.</summary>
    NonUeNeinregistrat = 7,
}
