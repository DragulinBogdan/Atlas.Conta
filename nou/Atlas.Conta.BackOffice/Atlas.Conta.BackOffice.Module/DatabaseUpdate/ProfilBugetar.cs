using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Pachetul de profil BUGETAR (P1, design §5): conținutul seed-ului 3a–3d,
// mutat, nemodificat — plan CPLAN + Clasă/Tip + politici (deciziile 23–33).
// TVA: neplătitor — doar rânduri TipTva Capitalizat (TVA-ul furnizorului intră
// în Valoare, exact comportamentul de dinainte de P1) și NICIO PoliticaTva
// (zero rânduri de TVA în registre).
internal static class ProfilBugetar {
    internal static void Seed(IObjectSpace os) {
        SeedClasaTip(os);
        SeedPlanConturi(os);
        ContaSeeder.SeedRepartitoriMinimali(os);
        SeedTipTva(os);
        // Derivările de mai jos interoghează BAZA (GetObjectsQuery) — rândurile
        // de nomenclator create mai sus trebuie comise întâi, altfel tipurile
        // noi nu primesc cont la primul updater (vizibil doar la fresh install
        // sau la adăugarea de tipuri).
        os.CommitChanges();
        ContaSeeder.SeedContImplicitTipMaterial(os);
        SeedPoliticiNotaTransfer(os);
        SeedPoliticiFacturaIntrareNir(os);
        SeedPoliticiBonConsum(os);
        SeedPoliticiListaDiferente(os);
        SeedPoliticiFacturaIesire(os);
        SeedPoliticiTrezorerie(os);
        SeedPoliticiDecont(os);
        SeedPoliticiNotaContabila(os);
        SeedPoliticiValidare(os);
        SeedTipTvaImplicit(os);
        os.CommitChanges();
    }

    // Datoria P1 (design §8): default TipTva de CULEGERE per tip de document.
    // Bugetarul e neplătitor (fără PoliticaTva, fără postare de TVA în motor),
    // dar culegerea are nevoie de un default — CAP21 (capitalizat 21%) pe
    // FCT/FCL/DEC, setat DOAR unde null. CAP21 e comis în SeedTipTva mai sus.
    static void SeedTipTvaImplicit(IObjectSpace os) {
        var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");
        if (cap21 == null)
            return;
        foreach (var cod in new[] { "FCT", "FCL", "DEC" }) {
            var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == cod);
            if (tip != null && tip.TipTvaImplicitId == null)
                tip.TipTvaImplicitId = cap21.ID;
        }
    }

    // Neplătitor: cota furnizorului se capitalizează în Valoare. Rândurile
    // acoperă cotele în vigoare + cele istorice (documente retroactive și
    // ancorele e2e la 19%) — sunt DATE, editabile.
    static void SeedTipTva(IObjectSpace os) {
        (string Cod, string Denumire, decimal Cota)[] tipuri = [
            ("CAP21", "TVA capitalizat 21%", 21m),
            ("CAP19", "TVA capitalizat 19% (istoric)", 19m),
            ("CAP11", "TVA capitalizat 11%", 11m),
            ("CAP0", "Fără TVA / scutit", 0m),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipTva>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipTva>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.Cota = t.Cota;
                tip.Regim = RegimTva.Capitalizat;
            }
        }
    }

    // Obligativitățile per tip (3d) — parte din PROFILUL de validare bugetar
    // (decizia 29): clasificația bugetară per linie pe documentele de angajare
    // și plată (fostele validări hardcodate FCT/DEC — 29b/32d — plus PLT — 31f;
    // INC nu: veniturile n-au angajamente, iar defalcarea E a conturilor de
    // trezorerie cere oricum codul economic la nivel de cont). FCL: în acest
    // profil facturarea nu descarcă gestiune — natura Stoc e interzisă (30a).
    // Upsert: valorile definesc politica, se impun și pe rândurile existente.
    static void SeedPoliticiValidare(IObjectSpace os) {
        PoliticaValidare Politica(string cod) {
            var p = os.FirstOrDefault<PoliticaValidare>(x => x.TipDocument.Cod == cod);
            if (p == null) {
                p = os.CreateObject<PoliticaValidare>();
                p.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == cod);
            }
            return p;
        }
        foreach (var cod in new[] { "FCT", "DEC", "PLT" })
            Politica(cod).CereClasificatieBugetara = true;
        Politica("FCL").NaturaInterzisa = NaturaClasa.Stoc;
    }

    // Inventar 10 §2, curățat: clasele tehnice (TVA/Diferențe) separate de stoc
    // prin Natura; Tipul poartă simbolul de cont ca și cod (nivelul de contare).
    static void SeedClasaTip(IObjectSpace os) {
        (string Cod, string Denumire, NaturaClasa Natura)[] clase = [
            ("M", "Materiale", NaturaClasa.Stoc),
            ("P", "Produse", NaturaClasa.Stoc),
            ("S", "Servicii", NaturaClasa.Serviciu),
            ("C", "Cheltuieli", NaturaClasa.Cheltuiala),
            ("SA", "Salarii", NaturaClasa.Cheltuiala),
            ("F", "Mijloace fixe", NaturaClasa.Imobilizare),
            ("OI", "Obiecte de inventar", NaturaClasa.Stoc),
            ("B", "Financiare, cheltuieli extraordinare", NaturaClasa.Cheltuiala),
            ("G", "Bonusuri, gratuități", NaturaClasa.Stoc),
            ("V", "Diferențe de curs valutar", NaturaClasa.Tehnica),
            ("K", "Cursanți", NaturaClasa.Tehnica),
            ("T", "TVA", NaturaClasa.Tehnica),
            ("DP", "Diferență preț", NaturaClasa.Tehnica),
            ("OF", "Obiecte în folosință", NaturaClasa.Stoc),
            ("MF", "Mărfuri", NaturaClasa.Stoc),
            ("MC", "Materiale custodie", NaturaClasa.Stoc),
            ("MN", "Alte materiale", NaturaClasa.Stoc),
            ("D", "Combustibil", NaturaClasa.Stoc),
            ("MED", "Medicamente", NaturaClasa.Stoc),
            ("MS", "Materiale sanitare", NaturaClasa.Stoc),
            ("DEZ", "Dezinfectanți", NaturaClasa.Stoc),
            ("PS", "Piese de schimb", NaturaClasa.Stoc),
            ("L", "Lubrifianți", NaturaClasa.Stoc),
            ("VEN", "Venituri", NaturaClasa.Serviciu),
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

        // Cod tip = simbolul de cont din denumirea legacy (GEST_TIP_MATERIAL);
        // rândul de zgomot „Stornare Medicamente" (dublură 302.09.00.1) nu se preia.
        (string Clasa, string Cod, string Denumire)[] tipuri = [
            ("M", "302.01.00", "Materiale auxiliare"),
            ("M", "302.03.00", "Materiale pentru ambalat"),
            ("M", "302.08.00", "Alte materiale consumabile"),
            ("S", "411.01.01", "Clienți cu termen sub un an"),
            ("S", "461.01.01", "Debitori sub 1 an — creanțe comerciale"),
            ("S", "471.00.00", "Cheltuieli înregistrate în avans"),
            ("S", "472.00.00", "Venituri înregistrate în avans"),
            ("C", "401.01.00", "Furnizori sub 1 an"),
            ("C", "602.02.00", "Cheltuieli privind combustibilul"),
            ("C", "602.04.00", "Cheltuieli privind piesele de schimb"),
            ("C", "602.08.00", "Cheltuieli privind alte materiale consumabile"),
            ("C", "610.00.00", "Cheltuieli privind energia și apa"),
            ("C", "612.00.00", "Cheltuieli cu chiriile"),
            ("C", "613.00.00", "Cheltuieli cu primele de asigurare"),
            ("C", "614.00.00", "Cheltuieli cu deplasări, detașări, transferări"),
            ("C", "622.00.00", "Cheltuieli privind comisioanele și onorariile"),
            ("C", "623.00.00", "Cheltuieli de protocol, reclamă și publicitate"),
            ("C", "626.00.00", "Cheltuieli poștale și taxe de telecomunicații"),
            ("C", "628.00.00", "Alte cheltuieli cu serviciile executate de terți"),
            ("C", "629.01.00", "Alte cheltuieli autorizate prin dispoziții legale"),
            ("C", "654.00.00", "Pierderi din creanțe și debitori diverși"),
            ("C", "679.00.00", "Alte cheltuieli"),
            ("F", "205.00.00", "Concesiuni, brevete, licențe, mărci — amortizabile"),
            ("F", "208.01.00", "Programe informatice — amortizabile"),
            ("F", "208.02.00", "Alte active fixe necorporale"),
            ("F", "214.00.00", "Mobilier, aparatură birotică, alte active fixe corporale"),
            ("F", "231.00.00", "Active fixe în curs de execuție"),
            ("F", "682.01.09", "Cheltuieli cu activele fixe corporale neamortizabile"),
            ("OI", "303.01.00", "Obiecte de inventar în magazie"),
            ("V", "765.02.00", "Venituri din diferențe de curs valutar"),
            ("T", "442.06.00", "TVA deductibilă"),
            ("T", "442.07.00", "TVA colectată"),
            ("OF", "303.02.00", "Obiecte de inventar în folosință"),
            ("D", "302.02.00.2", "Motorină"),
            ("D", "409.01.01", "Furnizori-debitori pentru cumpărări de bunuri"),
            ("D", "532.04.00", "Bonuri valorice pentru carburant"),
            ("D", "532.08.00", "Alte valori"),
            ("MED", "302.09.00.1", "Medicamente"),
            ("MS", "302.09.00.2", "Materiale sanitare"),
            ("DEZ", "302.09.00.4", "Dezinfectanți"),
            ("PS", "302.04.00", "Piese de schimb"),
            ("L", "302.02.00.3", "Lubrifianți"),
            // Nivelul de contare al facturării (inventar 07: contul de venit
            // ales pe linie devine politică per Clasă/Tip — testul bazei §7.2);
            // simboluri din planul bugetar (751/750, nu 704/706 ca la privat).
            ("VEN", "751.01.00", "Venituri din prestări de servicii și alte activități"),
            ("VEN", "750.02.00", "Alte venituri din proprietate (chirii)"),
            ("VEN", "751.04.00", "Diverse venituri"),
            // Tipul convențional al liniilor de plată/încasare culese manual
            // (decizia 31c): linia e defalcarea sumei, nu un material — conturile
            // vin din laturile documentului, nu din Tip (regula PLT/INC e
            // generică). Codul nu e simbol de cont — rămâne fără ContImplicit.
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

    // Planul sintetic complet din CPLAN (decizia 10; nomenclator, nu politică —
    // decizia 18). Defalcarea legacy (CPLAN_DEFALCARE) devine DimensiuniObligatorii.
    static void SeedPlanConturi(IObjectSpace os) {
        if (os.GetObjectsCount(typeof(Cont), null) > 0)
            return;

        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("Atlas.Conta.BackOffice.Module.DatabaseUpdate.SeedData.plan-conturi.csv")
            ?? throw new InvalidOperationException("Resursa plan-conturi.csv lipsește.");
        using var reader = new StreamReader(stream);

        var conturi = new Dictionary<string, Cont>();
        reader.ReadLine(); // header
        string line;
        while ((line = reader.ReadLine()) != null) {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            var f = line.Split('|');
            var cont = os.CreateObject<Cont>();
            cont.Simbol = f[0];
            cont.Denumire = f[1];
            cont.Functie = f[3];
            cont.Sumator = f[4] == "1";
            cont.DimensiuniObligatorii = ParseDefalcare(f[5]);
            // CSV-ul e ordonat pe nivel (părinții înaintea copiilor).
            if (f[2].Length > 0 && conturi.TryGetValue(f[2], out var parinte))
                cont.Parinte = parinte;
            conturi[f[0]] = cont;
        }
    }

    // Legenda CPLAN_DEFALCARE: R=Repartitor, M=Material, F=Funcțional, E=Economic,
    // B=Sursă de finanțare, P=Proiect; T (Titlu) = nivel al clasificației
    // economice → CodEconomic; S=Standard (fără defalcare).
    static DimensiuneFlags ParseDefalcare(string cod) {
        if (cod is "S" or "")
            return DimensiuneFlags.Niciuna;
        var flags = DimensiuneFlags.Niciuna;
        foreach (var c in cod) {
            flags |= c switch {
                'R' => DimensiuneFlags.Repartitor,
                'M' => DimensiuneFlags.Material,
                'F' => DimensiuneFlags.CodFunctional,
                'E' => DimensiuneFlags.CodEconomic,
                'B' => DimensiuneFlags.SursaFinantare,
                'P' => DimensiuneFlags.Proiect,
                'T' => DimensiuneFlags.CodEconomic,
                _ => throw new InvalidOperationException($"Cod de defalcare necunoscut: '{c}' în '{cod}'."),
            };
        }
        return flags;
    }

    // Inventar 04: −1 predator / +1 primitor, ACELAȘI tip stoc (magazie);
    // Clasa=null ⇒ regula acoperă toate clasele cu Natura=Stoc.
    // Contare: NICIUN rând — la plan sintetic transferul nu mișcă conturi;
    // mutarea între gestiuni trăiește în registrul de stoc (+ dimensiunea
    // Repartitor), nu în note 3xx=3xx (zgomotul legacy nu se preia).
    static void SeedPoliticiNotaTransfer(IObjectSpace os) {
        var btr = os.FirstOrDefault<TipDocument>(x => x.Cod == "BTR");
        ContaSeeder.SeedNumerotare(os, "BTR", "BTR-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "BTR") != null)
            return;
        var iesire = os.CreateObject<RegulaStoc>();
        iesire.TipDocument = btr;
        iesire.Latura = LaturaDocument.Predator;
        iesire.TipStoc = TipStoc.Magazie;
        iesire.Semn = -1;
        var intrare = os.CreateObject<RegulaStoc>();
        intrare.TipDocument = btr;
        intrare.Latura = LaturaDocument.Primitor;
        intrare.TipStoc = TipStoc.Magazie;
        intrare.Semn = +1;
    }

    // Politicile lanțului de cumpărare (inventar 01/02, curățate pe decizia 21).
    // Tranșarea întrebării 00 §13.1 (cine postează): recepția contează pe NIR
    // (3xx = furnizor), FacturaIntrare postează DOAR liniile care nu trec pe
    // NIR (servicii/cheltuieli/imobilizări) — fără dublă postare, granița e
    // Natura clasei (aceeași care alimentează filtrul conexului).
    static void SeedPoliticiFacturaIntrareNir(IObjectSpace os) {
        var fct = os.FirstOrDefault<TipDocument>(x => x.Cod == "FCT");
        var nir = os.FirstOrDefault<TipDocument>(x => x.Cod == "NIR");
        var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
        var cont404 = os.FirstOrDefault<Cont>(c => c.Simbol == "404.01.00");

        ContaSeeder.StergeReguliContareStricate(os);

        // FCT nu are numerotare (poartă numărul furnizorului); NIR-ul primește
        // număr propriu la operare.
        ContaSeeder.SeedNumerotare(os, "NIR", "NIR-");

        // Conexul FCT→NIR (00 §6): fără swap de laturi (TIP_DESCARCARE=0),
        // trec doar liniile purtătoare de stoc. Upsert: valorile definesc
        // politica, deci se impun și pe rândul existent.
        var conex = os.FirstOrDefault<PoliticaConex>(x => x.TipDocumentSursa.Cod == "FCT");
        if (conex == null) {
            conex = os.CreateObject<PoliticaConex>();
            conex.TipDocumentSursa = fct;
            conex.TipDocumentTinta = nir;
        }
        conex.InverseazaLaturi = false;
        conex.NaturaFiltru = NaturaClasa.Stoc;

        // Reguli stoc NIR (inventar 02): +1 pe primitor; generic → Magazie,
        // clasele cu registru propriu (gratuități/folosință/mărfuri/custodie)
        // au rând specific — regula specifică bate genericul în motor.
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "NIR") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("G", TipStoc.Gratuit),
                ("OF", TipStoc.Folosinta),
                ("MF", TipStoc.Marfuri),
                ("MC", TipStoc.Custodie),
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

        // Contare NIR: 3xx (contul Tipului) = furnizor (ContImplicit al
        // partenerului predator, fallback 401), valoarea cu TVA capitalizat.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "NIR") == null) {
            var receptie = os.CreateObject<RegulaContare>();
            receptie.TipDocument = nir;
            receptie.NaturaFiltru = NaturaClasa.Stoc;
            receptie.SursaContDebit = SursaCont.TipMaterial;
            receptie.SursaContCredit = SursaCont.RepartitorPredator;
            receptie.ContCredit = cont401;
        }

        // Contare FCT: doar naturile care NU trec pe NIR; debit = contul
        // Tipului (6xx/47x/2xx), credit = furnizorul (404 la imobilizări).
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

    // Politicile consumului (inventar 03, defa 65). Stoc: consumul nu „dispare" —
    // alimentează DOUĂ registre simultan: −1 Magazie pe predator (gestiunea),
    // +1 Consum pe primitor (locul de consum, util la obiecte date în folosință
    // / responsabilități).
    static void SeedPoliticiBonConsum(IObjectSpace os) {
        var bcs = os.FirstOrDefault<TipDocument>(x => x.Cod == "BCS");
        ContaSeeder.SeedNumerotare(os, "BCS", "BCS-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "BCS") == null) {
            var iesire = os.CreateObject<RegulaStoc>();
            iesire.TipDocument = bcs;
            iesire.Latura = LaturaDocument.Predator;
            iesire.TipStoc = TipStoc.Magazie;
            iesire.Semn = -1;
            var consum = os.CreateObject<RegulaStoc>();
            consum.TipDocument = bcs;
            consum.Latura = LaturaDocument.Primitor;
            consum.TipStoc = TipStoc.Consum;
            consum.Semn = +1;
        }

        // Contarea consumului: 6xx = 3xx per Clasă/Tip, derivată din simbol
        // (helper comun cu minusul de inventar); fără filtru de semn — liniile
        // de consum sunt întotdeauna pozitive.
        ContaSeeder.SeedContare6xxDin3xx(os, bcs, null);
    }

    // Politicile inventarierii (inventar 05, defa 270). Stoc: +1 pe predator
    // (gestiunea inventariată) — direcția vine din semnul cantității, LDI e
    // singurul tip din setul țintă unde semnul chiar diferențiază; tip stoc per
    // clasă ca la NIR (magazie generic / folosință / custodie). Contarea se
    // desparte pe direcție prin SemnFiltru: minusul = cheltuială (6xx = 3xx,
    // aceeași derivare ca la consum), plusul = venit (3xx = 791 — CPLAN nu are
    // 758; 791 „Venituri din valorificarea unor bunuri ale statului" e
    // echivalentul din planul instituțiilor publice).
    static void SeedPoliticiListaDiferente(IObjectSpace os) {
        var ldi = os.FirstOrDefault<TipDocument>(x => x.Cod == "LDI");
        ContaSeeder.SeedNumerotare(os, "LDI", "LDI-");
        if (os.FirstOrDefault<RegulaStoc>(x => x.TipDocument.Cod == "LDI") == null) {
            (string Clasa, TipStoc TipStoc)[] reguli = [
                (null, TipStoc.Magazie),
                ("OF", TipStoc.Folosinta),
                ("MC", TipStoc.Custodie),
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
        // Plusul: un singur rând generic — debitul se rezolvă din contul
        // Tipului liniei (3xx), creditul e venitul explicit.
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "LDI" && x.TipMaterialId == null) == null) {
            var plus = os.CreateObject<RegulaContare>();
            plus.TipDocument = ldi;
            plus.NaturaFiltru = NaturaClasa.Stoc;
            plus.SemnFiltru = +1;
            plus.SursaContDebit = SursaCont.TipMaterial;
            plus.SursaContCredit = SursaCont.Explicit;
            plus.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "791.00.00");
        }
        // Minusul: cheltuială per Tip, doar pe liniile negative.
        ContaSeeder.SeedContare6xxDin3xx(os, ldi, -1);
    }

    // Politicile facturării (inventar 07): pur creanță — NICIO regulă de stoc.
    // Numerotare proprie (serie fiscală — spre deosebire de FCT, care poartă
    // numărul furnizorului); formula de header legacy `DATA_SCADENTA = data+30`
    // devine politică de scadență. Contare: un singur rând generic 411 = 7xx —
    // debitul se particularizează prin ContImplicit al clientului (ex. 461
    // debitori), fallback 411.01.01; creditul vine din contul Tipului liniei
    // (clasa VEN) — fără fallback: un Tip fără cont e eroare semnalată la operare.
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
            facturare.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
            facturare.SursaContCredit = SursaCont.TipMaterial;
        }
    }

    // Politicile trezoreriei (inventar 09, decizia 31): plățile/încasările sunt
    // documente pur contabile — fără reguli de stoc; registrul de casă/bancă e
    // registrul contabil al lor. Un rând generic de contare per tip, cu ambele
    // conturi din laturi (ContImplicit pe Repartitor): PLT — beneficiarul
    // (401/404/542, fallback 401) = contul propriu (5xx/770, FĂRĂ fallback: un
    // cont propriu fără cont e eroare clară la operare); INC — oglindit, cu
    // fallback 411 pe plătitor. Conturile proprii (legacy `casierie`) primesc
    // rânduri minime: casa în lei + finanțarea de la buget (trezoreria).
    static void SeedPoliticiTrezorerie(IObjectSpace os) {
        var casa = os.FirstOrDefault<ContPropriu>(x => x.Cod == "CASA");
        if (casa == null) {
            casa = os.CreateObject<ContPropriu>();
            casa.Cod = "CASA";
            casa.Denumire = "Casa în lei";
            casa.EsteBanca = false;
            casa.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
        }
        var trez = os.FirstOrDefault<ContPropriu>(x => x.Cod == "TREZ");
        if (trez == null) {
            trez = os.CreateObject<ContPropriu>();
            trez.Cod = "TREZ";
            trez.Denumire = "Trezorerie — finanțare de la buget";
            trez.EsteBanca = true;
            trez.ContImplicit = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");
        }

        ContaSeeder.SeedNumerotare(os, "PLT", "PLT-");
        ContaSeeder.SeedNumerotare(os, "INC", "INC-");

        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "PLT") == null) {
            var plata = os.CreateObject<RegulaContare>();
            plata.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == "PLT");
            plata.SursaContDebit = SursaCont.RepartitorPrimitor;
            plata.ContDebit = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            plata.SursaContCredit = SursaCont.RepartitorPredator;
        }
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "INC") == null) {
            var incasare = os.CreateObject<RegulaContare>();
            incasare.TipDocument = os.FirstOrDefault<TipDocument>(x => x.Cod == "INC");
            incasare.SursaContDebit = SursaCont.RepartitorPrimitor;
            incasare.SursaContCredit = SursaCont.RepartitorPredator;
            incasare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
        }
    }

    // Nota contabilă (FAZA 1C §5): SINGURA politică e numerotarea. Fără reguli
    // de stoc și fără reguli de contare — postarea explicită a liniei e completă
    // (motorul o acceptă chiar și în absența regulii); fără PoliticaTva /
    // Scadenta / Validare: nota e neutră față de profil.
    static void SeedPoliticiNotaContabila(IObjectSpace os) {
        ContaSeeder.SeedNumerotare(os, "NTC", "NTC-");
    }

    // Politicile decontului (inventar 06): justificarea avansurilor — fără
    // stoc, numerotare proprie. Contare: un rând generic — debit din contul
    // Tipului liniei (cheltuiala aleasă), FĂRĂ fallback (Tip fără cont și
    // linie fără cont explicit = eroare clară la operare); credit = contul de
    // avans al titularului (ContImplicit al angajatului predator, fallback
    // 542.01.00). Postarea explicită pe linie (trăsătura proprie DEC) bate
    // ambele în motor.
    static void SeedPoliticiDecont(IObjectSpace os) {
        var dec = os.FirstOrDefault<TipDocument>(x => x.Cod == "DEC");
        ContaSeeder.SeedNumerotare(os, "DEC", "DEC-");
        if (os.FirstOrDefault<RegulaContare>(x => x.TipDocument.Cod == "DEC") == null) {
            var justificare = os.CreateObject<RegulaContare>();
            justificare.TipDocument = dec;
            justificare.SursaContDebit = SursaCont.TipMaterial;
            justificare.SursaContCredit = SursaCont.RepartitorPredator;
            justificare.ContCredit = os.FirstOrDefault<Cont>(c => c.Simbol == "542.01.00");
        }
    }
}
