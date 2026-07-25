using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Pachetul de profil PRIVAT (P1, deciziile 29/35): plan OMFP 1802 (sintetice
// grad 1–3), Clasă/Tip minimal pe simboluri OMFP, TVA structural (TipTva cu
// conturile 4426/4427 ca date + PoliticaTva per tip de document) și politicile
// de contare cu derivările PROPRII profilului (371→607, 345→711, 381→608;
// plus de inventar 3xx = 7588, nu 791). Diferă de bugetar prin CONȚINUT, nu
// prin mecanisme (decizia 29c) — mecanismele stau în ContaSeeder.
internal static class ProfilPrivat {
    // Mapările 3xx→6xx/7xx care nu urmează schimbarea primei cifre (29c):
    // mărfuri 371→607, produse finite 345→711 (descărcarea inversează venitul
    // aferent costurilor), ambalaje 381→608.
    static readonly Dictionary<string, string> Derivari6xxExceptii = new() {
        ["371"] = "607",
        ["345"] = "711",
        ["381"] = "608",
    };

    internal static void Seed(IObjectSpace os) {
        SeedClasaTip(os);
        SeedPlanConturi(os);
        ContaSeeder.SeedRepartitoriMinimali(os);
        SeedPartenerRetail(os);
        // Derivările interoghează BAZA — nomenclatoarele se comit întâi (30e).
        os.CommitChanges();
        ContaSeeder.SeedContImplicitTipMaterial(os);
        SeedTipTva(os);
        SeedPoliticiNotaTransfer(os);
        SeedPoliticiFacturaIntrareNir(os);
        SeedPoliticiBonConsum(os);
        SeedPoliticiListaDiferente(os);
        SeedPoliticiFacturaIesire(os);
        SeedPoliticiTrezorerie(os);
        SeedPoliticiDecont(os);
        SeedPoliticiNotaContabila(os);
        SeedPoliticiInchidereTva(os);
        SeedPoliticiAsamblare(os);
        // După FCL: derivarea de vânzare presupune genericul FCL deja creat
        // (altfel guard-ul „fără regulă FCL" din SeedPoliticiFacturaIesire ar
        // vedea rândurile de vânzare și n-ar mai crea genericul de servicii).
        SeedPoliticiDescarcare(os);
        // Retururile derivă 6xx = 3xx pe RDC (independent de FCL/DSC — cheia e
        // TipDocument), deci pot sta oriunde după nomenclatoare.
        SeedPoliticiRetururi(os);
        os.CommitChanges();
        // PoliticaTva referă TipTva comise mai sus.
        SeedPoliticiTva(os);
        // Default TipTva de CULEGERE: N21 referit — după commit-ul TipTva.
        SeedTipTvaImplicit(os);
        os.CommitChanges();
    }

    // Clasă/Tip minimal privat: Cod tip = simbol OMFP (același mecanism de
    // derivare a contului implicit ca la bugetar — 10 §2 / decizia 26b).
    static void SeedClasaTip(IObjectSpace os) {
        (string Cod, string Denumire, NaturaClasa Natura)[] clase = [
            ("MP", "Materii prime", NaturaClasa.Stoc),
            ("M", "Materiale consumabile", NaturaClasa.Stoc),
            ("OI", "Obiecte de inventar", NaturaClasa.Stoc),
            ("PF", "Produse finite", NaturaClasa.Stoc),
            ("MF", "Mărfuri", NaturaClasa.Stoc),
            ("AMB", "Ambalaje", NaturaClasa.Stoc),
            ("S", "Servicii și utilități", NaturaClasa.Serviciu),
            ("C", "Alte cheltuieli", NaturaClasa.Cheltuiala),
            ("F", "Imobilizări", NaturaClasa.Imobilizare),
            ("VEN", "Venituri", NaturaClasa.Serviciu),
            ("T", "TVA", NaturaClasa.Tehnica),
            ("TRZ", "Trezorerie", NaturaClasa.Tehnica),
        ];
        var claseMap = new Dictionary<string, ClasaProdus>();
        foreach (var c in clase) {
            var clasa = os.FirstOrDefault<ClasaProdus>(x => x.Cod == c.Cod);
            if (clasa == null) {
                clasa = os.CreateObject<ClasaProdus>();
                clasa.Cod = c.Cod;
                clasa.Denumire = c.Denumire;
                clasa.Natura = c.Natura;
            }
            claseMap[c.Cod] = clasa;
        }

        (string Clasa, string Cod, string Denumire)[] tipuri = [
            ("MP", "301", "Materii prime"),
            ("M", "302", "Materiale consumabile"),
            // Gradul II al lui 302: 1C ține stocul pe `302.1`/`302.8`, iar Cod-ul
            // Tipului E simbolul de cont (decizia 26b) — fără ele, pozițiile de
            // deschidere de pe aceste conturi n-ar putea deveni loturi. Derivările
            // (ContImplicit din simbol, 6xx=3xx → 6021/6028) le prind automat.
            ("M", "3021", "Materiale auxiliare"),
            ("M", "3024", "Piese de schimb"),
            ("M", "3028", "Alte materiale consumabile"),
            ("OI", "303", "Materiale de natura obiectelor de inventar"),
            ("PF", "345", "Produse finite"),
            ("MF", "371", "Mărfuri"),
            ("AMB", "381", "Ambalaje"),
            ("S", "605", "Energie și apă"),
            ("S", "611", "Întreținere și reparații"),
            ("S", "612", "Redevențe, locații de gestiune și chirii"),
            ("S", "613", "Prime de asigurare"),
            ("S", "614", "Studii și cercetări"),
            ("S", "622", "Comisioane și onorarii"),
            ("S", "623", "Protocol, reclamă și publicitate"),
            ("S", "625", "Deplasări, detașări și transferări"),
            ("S", "626", "Poștale și telecomunicații"),
            ("S", "628", "Alte servicii executate de terți"),
            ("C", "635", "Alte impozite, taxe și vărsăminte asimilate"),
            ("F", "208", "Alte imobilizări necorporale"),
            ("F", "214", "Mobilier, aparatură birotică, alte active corporale"),
            // Nivelul de contare al facturării (30b): contul de venit = alegerea
            // Tipului; simboluri OMFP (704/706/707/708 — nu 751/750 ca la bugetar).
            ("VEN", "704", "Venituri din servicii prestate"),
            ("VEN", "706", "Venituri din redevențe, locații de gestiune și chirii"),
            ("VEN", "707", "Venituri din vânzarea mărfurilor"),
            ("VEN", "708", "Venituri din activități diverse"),
            // Tipul convențional al liniilor de trezorerie culese manual (31c):
            // codul nu e simbol de cont — rămâne fără ContImplicit.
            ("TRZ", "TRZ", "Operațiune de trezorerie"),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipMaterial>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipMaterial>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.Clasa = claseMap[t.Clasa];
            }
        }
    }

    // Partenerul generic de retail (decizia 48b): vânzarea cu amănuntul nu are
    // client identificat, dar factura are nevoie de o latură — surogatul RVA
    // (raportul zilnic de vânzări → FCL) o primește pe acesta. Fără ContImplicit
    // propriu: fallback-ul 4111 al regulii de facturare e exact contul corect.
    static void SeedPartenerRetail(IObjectSpace os) {
        if (os.FirstOrDefault<Partener>(p => p.Cod == "CF") != null)
            return;
        var consumatorFinal = os.CreateObject<Partener>();
        consumatorFinal.Cod = "CF";
        consumatorFinal.Denumire = "CONSUMATOR FINAL";
    }

    // Planul OMFP 1802 (sursa: nomenclatorul ANAF PlanConturiBalSocCom, format
    // Account,ParentAccount,Denumire — numele poate conține virgule, deci
    // split cu limită). OMFP nu poartă funcție/defalcare: DimensiuniObligatorii
    // pornesc goale (design §5 — editabile ca date când apare nevoia);
    // Sumator = are copii.
    static void SeedPlanConturi(IObjectSpace os) {
        if (os.GetObjectsCount(typeof(Cont), null) > 0)
            return;

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Atlas.Conta.BackOffice.Module.DatabaseUpdate.SeedData.plan-conturi-omfp.csv")
            ?? throw new InvalidOperationException("Resursa plan-conturi-omfp.csv lipsește.");
        using var reader = new StreamReader(stream);

        var conturi = new Dictionary<string, Cont>();
        reader.ReadLine(); // header
        string line;
        while ((line = reader.ReadLine()) != null) {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var f = line.Split(',', 3);
            var cont = os.CreateObject<Cont>();
            cont.Simbol = f[0];
            cont.Denumire = f[2];
            // CSV-ul e ordonat pe nivel (părinții înaintea copiilor).
            if (f[1].Length > 0 && conturi.TryGetValue(f[1], out var parinte)) {
                cont.Parinte = parinte;
                parinte.Sumator = true;
            }
            conturi[f[0]] = cont;
        }
    }

    // Nomenclatorul TipTva (design §2): cotele Legii 141/2025 (21 standard,
    // 11 redusă, 9 tranzitoriu locuințe până la 31.07.2026) + regimurile.
    // Conturile de TVA sunt DATE per profil; codurile SAF-T (D406) vin din
    // nomenclatorul ANAF (RO_SAFT_SchemaDefCod 16.02.2026), direcționale:
    // livrare (seria 310xxx) / achiziție deductibilă integral (301xxx) /
    // nedeductibilă (351xxx). Categoriile D394 sunt direcționale la nivel de
    // operațiune — se fixează la proiecția D394 (checklist 35c), rămân date.
    static void SeedTipTva(IObjectSpace os) {
        var conturi = os.GetObjectsQuery<Cont>()
            .Where(c => c.Simbol == "4426" || c.Simbol == "4427" || c.Simbol == "4428")
            .ToDictionary(c => c.Simbol, c => c);
        var tva4426 = conturi["4426"];
        var tva4427 = conturi["4427"];
        var tva4428 = conturi["4428"];

        (string Cod, string Denumire, decimal Cota, RegimTva Regim,
            bool Conturi, string SafTLivrare, string SafTAchizitie)[] tipuri = [
            ("N21", "TVA 21% (standard)", 21m, RegimTva.Normal, true, "310344", "301104"),
            ("N11", "TVA 11% (redusă)", 11m, RegimTva.Normal, true, "310351", "301105"),
            ("N9", "TVA 9% (tranzitoriu locuințe, până la 31.07.2026)", 9m, RegimTva.Normal, true, "310310", "301102"),
            ("TI21", "Taxare inversă 21%", 21m, RegimTva.TaxareInversa, true, "310312", "300906"),
            // Cotele ISTORICE (standard 19% până la 31.07.2025) — necesare
            // importului 1C, care aduce un an fiscal complet dinaintea Legii
            // 141/2025 (FAZA 1C §1). Codurile SAF-T rămân NULL: nomenclatorul
            // ANAF e cel în vigoare, iar D406-ul pe perioade vechi nu e o
            // proiecție a acestui sistem; se completează dacă apare vreodată.
            ("N19", "TVA 19% (standard, istoric — până la 31.07.2025)", 19m, RegimTva.Normal, true, null, null),
            ("TI19", "Taxare inversă 19% (istoric — până la 31.07.2025)", 19m, RegimTva.TaxareInversa, true, null, null),
            ("NED21", "Achiziție fără drept de deducere 21% (TVA capitalizat)", 21m, RegimTva.Capitalizat, false, null, "351104"),
            ("SDD", "Scutit cu drept de deducere", 0m, RegimTva.Scutit, false, "310314", null),
            ("SFD", "Scutit fără drept de deducere", 0m, RegimTva.Scutit, false, "310326", null),
            ("NIM", "Neimpozabil (în afara sferei TVA)", 0m, RegimTva.Neimpozabil, false, "310324", null),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipTva>(x => x.Cod == t.Cod) != null)
                continue;
            var tip = os.CreateObject<TipTva>();
            tip.Cod = t.Cod;
            tip.Denumire = t.Denumire;
            tip.Cota = t.Cota;
            tip.Regim = t.Regim;
            tip.CodSafTLivrare = t.SafTLivrare;
            tip.CodSafTAchizitie = t.SafTAchizitie;
            if (t.Conturi) {
                tip.ContTvaDeductibil = tva4426;
                tip.ContTvaColectat = tva4427;
                // REZERVAT (design §8): TVA la încasare / facturi nesosite.
                tip.ContTvaNeexigibil = tva4428;
            }
        }
    }

    // Postarea TVA per tip (design §4): FCT/DEC deduc contra pasivului laturii
    // predator (furnizor 401 / titular 542), FCL colectează contra creanței
    // laturii primitor (client 4111). Tipurile fără rând (NIR, BTR, BCS, LDI,
    // PLT, INC) nu postează TVA.
    static void SeedPoliticiTva(IObjectSpace os) {
        void Politica(string codTip, DirectieTva directie, SursaCont sursa, string fallback) {
            if (os.FirstOrDefault<PoliticaTva>(p => p.TipDocument.Cod == codTip) != null)
                return;
            var p = os.CreateObject<PoliticaTva>();
            p.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == codTip);
            p.Directie = directie;
            p.SursaContrapartida = sursa;
            p.ContrapartidaFallback = os.FirstOrDefault<Cont>(c => c.Simbol == fallback);
        }
        Politica("FCT", DirectieTva.Deductibil, SursaCont.RepartitorPredator, "401");
        Politica("DEC", DirectieTva.Deductibil, SursaCont.RepartitorPredator, "542");
        Politica("FCL", DirectieTva.Colectat, SursaCont.RepartitorPrimitor, "4111");
        // Retururile (FAZA 1C §7): aceeași direcție ca documentul stornat, dar
        // contrapartida stă pe latura INVERSĂ (RLF: furnizorul e primitor;
        // RDC: clientul e predator). ValoareTva negativă ⇒ 4426 = 401 cu −TVA,
        // respectiv 4111 = 4427 cu −TVA — exact rândurile 1C.
        Politica("RLF", DirectieTva.Deductibil, SursaCont.RepartitorPrimitor, "401");
        Politica("RDC", DirectieTva.Colectat, SursaCont.RepartitorPredator, "4111");
    }

    // Profilul de validare privat: la P2 nu mai are NICIUN rând — clasificația
    // bugetară nu se aplică (33c), iar interdicția FCL⊘Stoc a fost preluată de
    // descărcarea de gestiune (37e); curățarea rândului P1 stă în
    // SeedPoliticiDescarcare, ca pas explicit de updater.

    // Datoria P1 (design §8): default TipTva per tip de document, aplicat la
    // CULEGERE (nu în motor — un rând PoliticaTva doar-pentru-default ar activa
    // pasul TVA). FCT/FCL/DEC + retururile RLF/RDC → N21; setat DOAR unde null (editările manuale nu
    // se ating). Rulează după commit-ul TipTva (N21 referit prin FK).
    static void SeedTipTvaImplicit(IObjectSpace os) {
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        if (n21 == null)
            return;
        foreach (var cod in new[] { "FCT", "FCL", "DEC", "RLF", "RDC" }) {
            var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == cod);
            if (tip != null && tip.TipTvaImplicitId == null)
                tip.TipTvaImplicitId = n21.ID;
        }
    }

    // Registrele private, ca rânduri de politică: generic → Magazie, mărfurile pe
    // registrul propriu. INCREMENTAL per (latură × clasă), nu „există un rând ⇒
    // gata": tipurile seed-uite înaintea P2 (BTR/BCS — feliile 3b/3c) au primit
    // doar rândul generic, iar un lot de MARFĂ trăiește în registrul Marfuri (așa
    // îl scriu NIR/LDI/DSC/ASM/retururile și deschiderea importului 1C). Fără
    // rândul MF, orice transfer sau consum de marfă ar căuta soldul în Magazie și
    // ar cădea pe gardianul de sold — gaură de profil scoasă la iveală de import
    // (decizia 21/45f), nu schimbare de semantică.
    static void SeedReguliStoc(IObjectSpace os, TipDocument tipDoc, LaturaDocument latura, int semn,
            params (string Clasa, TipStoc TipStoc)[] reguli) {
        foreach (var r in reguli) {
            var clasaId = r.Clasa == null
                ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa)?.ID;
            var exista = clasaId == null
                ? os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID
                    && x.Latura == latura && x.ClasaId == null)
                : os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID
                    && x.Latura == latura && x.ClasaId == clasaId);
            if (exista != null)
                continue;
            var regula = os.CreateObject<RegulaStoc>();
            regula.TipDocument = tipDoc;
            regula.Latura = latura;
            regula.ClasaId = clasaId;
            regula.TipStoc = r.TipStoc;
            regula.Semn = semn;
        }
    }

    // Registrele „generic + mărfuri" folosite de aproape toate tipurile private.
    static readonly (string Clasa, TipStoc TipStoc)[] MagazieSiMarfuri =
        [(null, TipStoc.Magazie), ("MF", TipStoc.Marfuri)];

    // Transferul (23c): ± pe același registru, fără contare la plan sintetic.
    static void SeedPoliticiNotaTransfer(IObjectSpace os) {
        var btr = os.FirstOrDefault<TipDocument>(x => x.Cod == "BTR");
        ContaSeeder.SeedNumerotare(os, "BTR", "BTR-");
        SeedReguliStoc(os, btr, LaturaDocument.Predator, -1, MagazieSiMarfuri);
        SeedReguliStoc(os, btr, LaturaDocument.Primitor, +1, MagazieSiMarfuri);
    }

    // Lanțul de cumpărare (26a, sub TVA structural — design §6): recepția
    // contează pe NIR la NET (3xx = 401), factura postează liniile non-stoc
    // net + rândurile 4426 per linie (inclusiv ale liniilor de stoc).
    static void SeedPoliticiFacturaIntrareNir(IObjectSpace os) {
        var fct = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCT");
        var nir = os.FirstOrDefault<TipDocument>(x => x.Cod == "NIR");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
        var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404");

        ContaSeeder.StergeReguliContareStricate(os);
        ContaSeeder.SeedNumerotare(os, "NIR", "NIR-");

        var conex = os.FirstOrDefault<PoliticaConex>(x => x.TipDocumentSursa.Cod == "FCT");
        if (conex == null) {
            conex = os.CreateObject<PoliticaConex>();
            conex.TipDocumentSursa = fct;
            conex.TipDocumentTinta = nir;
        }
        conex.InverseazaLaturi = false;
        conex.NaturaFiltru = NaturaClasa.Stoc;

        // Stoc NIR: +1 pe primitor; generic → Magazie, mărfurile pe registrul
        // propriu (restul claselor speciale bugetare nu există la privat).
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "NIR") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = nir;
                regula.Latura = LaturaDocument.Primitor;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = +1;
            }
        }

        // Contare NIR: 3xx (contul Tipului) = furnizor, la NET.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "NIR") == null) {
            var receptie = os.CreateObject<RegulaContare>();
            receptie.TipDocument = nir;
            receptie.NaturaFiltru = NaturaClasa.Stoc;
            receptie.SursaContDebit = SursaCont.TipMaterial;
            receptie.SursaContCredit = SursaCont.RepartitorPredator;
            receptie.ContCredit = cont401;
        }

        // Contare FCT: doar naturile care NU trec pe NIR, la net; 404 la imobilizări.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "FCT") == null) {
            (NaturaClasa Natura, Cont Fallback)[] reguli = [
                (NaturaClasa.Serviciu, cont401),
                (NaturaClasa.Cheltuiala, cont401),
                (NaturaClasa.Imobilizare, cont404),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaContare>();
                regula.TipDocument = fct;
                regula.NaturaFiltru = r.Natura;
                regula.SursaContDebit = SursaCont.TipMaterial;
                regula.SursaContCredit = SursaCont.RepartitorPredator;
                regula.ContCredit = r.Fallback;
            }
        }
    }

    // Consumul (27): −Magazie / +Consum; contarea 6xx = 3xx derivată din simbol,
    // cu excepțiile profilului (607/711/608).
    static void SeedPoliticiBonConsum(IObjectSpace os) {
        var bcs = os.FirstOrDefault<TipDocument>(x => x.Cod == "BCS");
        ContaSeeder.SeedNumerotare(os, "BCS", "BCS-");
        // Ieșirea din registrul în care STĂ lotul (generic Magazie, marfă
        // Marfuri — vezi nota de la SeedReguliStoc); intrarea în Consum e
        // aceeași pentru orice clasă, deci un singur rând generic.
        SeedReguliStoc(os, bcs, LaturaDocument.Predator, -1, MagazieSiMarfuri);
        SeedReguliStoc(os, bcs, LaturaDocument.Primitor, +1, (null, TipStoc.Consum));
        ContaSeeder.SeedContare6xxDin3xx(os, bcs, null, Derivari6xxExceptii);
    }

    // Inventarierea (28): +1 pe predator, direcția în semn. Minus = 6xx = 3xx
    // (cu excepțiile profilului), plus = 3xx = 7588 „Alte venituri din
    // exploatare" (decizia 29c — la privat NU 791).
    static void SeedPoliticiListaDiferente(IObjectSpace os) {
        var ldi = os.FirstOrDefault<TipDocument>(x => x.Cod == "LDI");
        ContaSeeder.SeedNumerotare(os, "LDI", "LDI-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "LDI") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = ldi;
                regula.Latura = LaturaDocument.Predator;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = +1;
            }
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "LDI" && x.TipMaterialId == null) == null) {
            var plus = os.CreateObject<RegulaContare>();
            plus.TipDocument = ldi;
            plus.NaturaFiltru = NaturaClasa.Stoc;
            plus.SemnFiltru = +1;
            plus.SursaContDebit = SursaCont.TipMaterial;
            plus.SursaContCredit = SursaCont.Explicit;
            plus.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "7588");
        }
        ContaSeeder.SeedContare6xxDin3xx(os, ldi, -1, Derivari6xxExceptii);
    }

    // Facturarea (30): pur creanță până la P2; 4111 = 7xx net + 4111 = 4427
    // per linie (prin PoliticaTva). Scadență default +30.
    static void SeedPoliticiFacturaIesire(IObjectSpace os) {
        var fcl = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCL");
        ContaSeeder.SeedNumerotare(os, "FCL", "FCL-");
        if (os.FirstOrDefault<PoliticaScadenta>(x => x.TipDocument.Cod == "FCL") == null) {
            var scadenta = os.CreateObject<PoliticaScadenta>();
            scadenta.TipDocument = fcl;
            scadenta.ZileDefault = 30;
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "FCL") == null) {
            var facturare = os.CreateObject<RegulaContare>();
            facturare.TipDocument = fcl;
            facturare.SursaContDebit = SursaCont.RepartitorPrimitor;
            facturare.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            facturare.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    // Descărcarea de gestiune (P2, design §3/§6): tip nou DSC generat pe loturi
    // din FCL (DescarcareService). Stoc: −1 pe predator (gestiunea de
    // descărcare), aceeași mapare de registre ca NIR/LDI privat (generic →
    // Magazie, MF → Marfuri). Cost: 6xx = 3xx per Tip cu excepțiile profilului
    // (607=371, 711=345, 608=381). Pe FCL: derivarea de VÂNZARE — pe liniile de
    // stoc creditul e venitul (371→707, 345→701, 381→708), nu contul de stoc;
    // genericul FCL rămâne pentru servicii. Plus curățarea rândului P1 care
    // interzicea natura Stoc pe FCL (37e — descărcarea o preia acum).
    static void SeedPoliticiDescarcare(IObjectSpace os) {
        var dsc = os.FirstOrDefault<TipDocument>(x => x.Cod == "DSC");
        var fcl = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCL");

        ContaSeeder.SeedNumerotare(os, "DSC", "DSC-");

        // Stoc DSC: −1 pe predator; generic → Magazie, mărfurile pe registrul
        // propriu (oglindește EXACT rândurile NIR/LDI privat).
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "DSC") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = dsc;
                regula.Latura = LaturaDocument.Predator;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = -1;
            }
        }

        // Costul descărcării: 6xx = 3xx per Tip, excepțiile profilului (60x=30x).
        ContaSeeder.SeedContare6xxDin3xx(os, dsc, null, Derivari6xxExceptii);

        // Vânzarea pe FCL: creditul = venitul (nu contul de stoc); fallback 708.
        ContaSeeder.SeedContareVanzare(os, fcl, "4111",
            new Dictionary<string, string> { ["371"] = "707", ["345"] = "701", ["381"] = "708" }, "708");

        // Pas explicit de updater: FCL nu mai interzice natura Stoc (rândul P1
        // există în bazele seed-uite atunci). CereClasificatieBugetara nu se
        // setează la privat, deci rândul se șterge întreg; idempotent.
        var validareFcl = os.FirstOrDefault<PoliticaValidare>(
            x => x.TipDocument.Cod == "FCL" && x.NaturaInterzisa == NaturaClasa.Stoc);
        if (validareFcl != null) {
            if (validareFcl.CereClasificatieBugetara)
                validareFcl.NaturaInterzisa = null;
            else
                os.Delete(validareFcl);
        }
    }

    // Trezoreria (31): conturi proprii OMFP (casa 5311, banca 5121); contare
    // din laturi — PLT: beneficiar (fallback 401) = cont propriu (fără
    // fallback); INC: oglindit (fallback 4111 pe plătitor).
    static void SeedPoliticiTrezorerie(IObjectSpace os) {
        var casa = os.FirstOrDefault<ContPropriu>(x => x.Cod == "CASA");
        if (casa == null) {
            casa = os.CreateObject<ContPropriu>();
            casa.Cod = "CASA";
            casa.Denumire = "Casa în lei";
            casa.EsteBanca = false;
            casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "5311");
        }
        var banca = os.FirstOrDefault<ContPropriu>(x => x.Cod == "BANCA");
        if (banca == null) {
            banca = os.CreateObject<ContPropriu>();
            banca.Cod = "BANCA";
            banca.Denumire = "Cont curent la bancă în lei";
            banca.EsteBanca = true;
            banca.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "5121");
        }

        ContaSeeder.SeedNumerotare(os, "PLT", "PLT-");
        ContaSeeder.SeedNumerotare(os, "INC", "INC-");

        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "PLT") == null) {
            var plata = os.CreateObject<RegulaContare>();
            plata.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == "PLT");
            plata.SursaContDebit = SursaCont.RepartitorPrimitor;
            plata.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
            plata.SursaContCredit = SursaCont.RepartitorPredator;
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "INC") == null) {
            var incasare = os.CreateObject<RegulaContare>();
            incasare.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == "INC");
            incasare.SursaContDebit = SursaCont.RepartitorPrimitor;
            incasare.SursaContCredit = SursaCont.RepartitorPredator;
            incasare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
        }
    }

    // Nota contabilă (FAZA 1C §5): SINGURA politică e numerotarea — identic cu
    // bugetarul (nota e neutră față de profil: fără stoc, fără contare, fără
    // TVA/scadență/validare; postarea explicită a liniei e completă).
    static void SeedPoliticiNotaContabila(IObjectSpace os) {
        ContaSeeder.SeedNumerotare(os, "NTC", "NTC-");
    }

    // Închiderea lunară de TVA (FAZA 1C §6): numerotare proprie + conturile
    // închiderii ca DATE (4426/4427/4423/4424 OMFP). Motorul nu cunoaște niciun
    // simbol — fără rândul ăsta ITV e tip inert (cazul bugetar).
    static void SeedPoliticiInchidereTva(IObjectSpace os) {
        ContaSeeder.SeedNumerotare(os, "ITV", "ITV-");
        if (os.FirstOrDefault<PoliticaInchidereTva>(p => p.TipDocument.Cod == "ITV") != null)
            return;
        var politica = os.CreateObject<PoliticaInchidereTva>();
        politica.TipDocument = os.FirstOrDefault<TipDocument>(t => t.Cod == "ITV");
        politica.ContDeductibila = os.FirstOrDefault<Cont>(c => c.Simbol == "4426");
        politica.ContColectata = os.FirstOrDefault<Cont>(c => c.Simbol == "4427");
        politica.ContDePlata = os.FirstOrDefault<Cont>(c => c.Simbol == "4423");
        politica.ContDeRecuperat = os.FirstOrDefault<Cont>(c => c.Simbol == "4424");
    }

    // Asamblarea (FAZA 1C §7): kitting n→m pe stoc, într-o gestiune. Stoc: UN
    // SINGUR set de reguli, +1 pe predator — SEMNUL LINIEI dă direcția (consum
    // −, produs +, materializat în PregatesteOperare, mecanismul LDI 28a);
    // regula spune doar latura și registrul. Aceeași mapare de registre ca
    // NIR/LDI/DSC privat (generic → Magazie, MF → Marfuri).
    // FĂRĂ RegulaContare: la plan sintetic marfă→marfă (371=371) e zgomot
    // (raționamentul 23c, ca la NotaTransfer) — valoarea se mută între loturi,
    // nu între conturi. Producția reală (345=711) primește reguli la cerință.
    static void SeedPoliticiAsamblare(IObjectSpace os) {
        var asm = os.FirstOrDefault<TipDocument>(x => x.Cod == "ASM");
        ContaSeeder.SeedNumerotare(os, "ASM", "ASM-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "ASM") != null)
            return;
        (string Clasa, TipStoc TipStoc)[] reguli = [
            (null, TipStoc.Magazie),
            ("MF", TipStoc.Marfuri),
        ];
        foreach (var r in reguli) {
            var regula = os.CreateObject<RegulaStoc>();
            regula.TipDocument = asm;
            regula.Latura = LaturaDocument.Predator;
            regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
            regula.TipStoc = r.TipStoc;
            regula.Semn = +1;
        }
    }

    // Retururile (FAZA 1C §7, rezoluția spike-ului storno): corespondența
    // ORIGINALĂ cu valori NEGATIVE. Liniile se culeg pozitive și se semnează la
    // operare (PregatesteOperare), deci regulile spun doar LATURA și registrul —
    // semnul liniei face direcția. `PastreazaSemn` scoate normalizarea de semn
    // din motor pe rândurile astea (singura extensie de motor a feliei).
    //   RLF: stoc +1 pe PREDATOR (gestiunea) × linia −q ⇒ −q (marfa iese);
    //        contare 3xx = 401 cu −V; TVA 4426 = 401 cu −TVA (PoliticaTva).
    //   RDC: stoc −1 pe PRIMITOR (gestiunea) × linia −q ⇒ +q (marfa revine pe
    //        lotul original); venit 4111 = 70x cu −V, cost 607 = 371 cu −cost,
    //        TVA 4111 = 4427 cu −TVA. Liniile de venit (Natura=Serviciu) nu
    //        sunt atinse de regulile generice de stoc (Natura=Stoc).
    // Mapările de registre oglindesc NIR/LDI/DSC privat (generic → Magazie,
    // MF → Marfuri).
    static void SeedPoliticiRetururi(IObjectSpace os) {
        var rlf = os.FirstOrDefault<TipDocument>(x => x.Cod == "RLF");
        var rdc = os.FirstOrDefault<TipDocument>(x => x.Cod == "RDC");
        ContaSeeder.SeedNumerotare(os, "RLF", "RLF-");
        ContaSeeder.SeedNumerotare(os, "RDC", "RDC-");

        void ReguliStoc(TipDocument tipDoc, LaturaDocument latura, int semn) {
            if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocumentId == tipDoc.ID) != null)
                return;
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("MF", TipStoc.Marfuri),
            ];
            foreach (var r in reguli) {
                var regula = os.CreateObject<RegulaStoc>();
                regula.TipDocument = tipDoc;
                regula.Latura = latura;
                regula.Clasa = r.Clasa == null ? null : os.FirstOrDefault<ClasaProdus>(c => c.Cod == r.Clasa);
                regula.TipStoc = r.TipStoc;
                regula.Semn = semn;
            }
        }
        ReguliStoc(rlf, LaturaDocument.Predator, +1);
        ReguliStoc(rdc, LaturaDocument.Primitor, -1);

        // RLF: stornarea achiziției — contul de stoc al Tipului = furnizorul.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocumentId == rlf.ID) == null) {
            var retur = os.CreateObject<RegulaContare>();
            retur.TipDocument = rlf;
            retur.NaturaFiltru = NaturaClasa.Stoc;
            retur.PastreazaSemn = true;
            retur.SursaContDebit = SursaCont.TipMaterial;
            retur.SursaContCredit = SursaCont.RepartitorPrimitor;
            retur.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
        }

        // RDC, liniile de venit: stornarea vânzării — clientul (predator) =
        // contul de venit al Tipului, FĂRĂ fallback (Tip fără cont = eroare
        // clară la operare, filozofia 30b).
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocumentId == rdc.ID && x.TipMaterialId == null) == null) {
            var venit = os.CreateObject<RegulaContare>();
            venit.TipDocument = rdc;
            venit.NaturaFiltru = NaturaClasa.Serviciu;
            venit.PastreazaSemn = true;
            venit.SursaContDebit = SursaCont.RepartitorPredator;
            venit.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");
            venit.SursaContCredit = SursaCont.TipMaterial;
        }

        // RDC, liniile de cost: costul REVINE — 6xx = 3xx per Tip cu excepțiile
        // profilului (607=371, 711=345, 608=381), valoarea negativă a liniei.
        ContaSeeder.SeedContare6xxDin3xx(os, rdc, semnFiltru: null, Derivari6xxExceptii, pastreazaSemn: true);
    }

    // Decontul (32): debit din contul Tipului (fără fallback), credit = avansul
    // titularului (542 OMFP ca fallback); TVA-ul justificat postează 4426 = 542
    // prin PoliticaTva.
    static void SeedPoliticiDecont(IObjectSpace os) {
        var dec = os.FirstOrDefault<TipDocument>(x => x.Cod == "DEC");
        ContaSeeder.SeedNumerotare(os, "DEC", "DEC-");
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "DEC") == null) {
            var justificare = os.CreateObject<RegulaContare>();
            justificare.TipDocument = dec;
            justificare.SursaContDebit = SursaCont.TipMaterial;
            justificare.SursaContCredit = SursaCont.RepartitorPredator;
            justificare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "542");
        }
    }
}
