using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

// Seed 3a (deciziile 18/20/21): ancorele TipDocument, Clasă/Tip curățat,
// planul de conturi sintetic, repartitori/gestiuni minimali, perioadele
// fiscale și politicile DOAR pentru NotaTransfer. Politicile se definesc pe
// funcționalitate (decizia 21) — legacy e direcție, nu canon.
public static class ContaSeeder {
    public static void Seed(IObjectSpace os) {
        SeedTipuriDocument(os);
        SeedClasaTip(os);
        SeedPlanConturi(os);
        SeedRepartitori(os);
        SeedPerioadeFiscale(os);
        SeedPoliticiNotaTransfer(os);
        os.CommitChanges();
    }

    // Decizia 20: nomenclatorul de tipuri oglindește clasele 1:1 — doar ancoră FK + UI.
    static void SeedTipuriDocument(IObjectSpace os) {
        (string Cod, string Denumire, string ClrType)[] tipuri = [
            ("FCT", "Factură intrare", nameof(FacturaIntrare)),
            ("FCL", "Factură ieșire", nameof(FacturaIesire)),
            ("NIR", "Notă de intrare-recepție", nameof(NIR)),
            ("BCS", "Bon de consum", nameof(BonConsum)),
            ("BTR", "Notă de transfer", nameof(NotaTransfer)),
            ("BPR", "Raport de producție", nameof(RaportProductie)),
            ("LDI", "Listă diferențe inventar", nameof(ListaDiferenteInventar)),
            ("DEC", "Decont", nameof(Decont)),
            ("PLT", "Plată", nameof(Plata)),
            ("INC", "Încasare", nameof(Incasare)),
        ];
        foreach (var t in tipuri) {
            if (os.FirstOrDefault<TipDocument>(x => x.Cod == t.Cod) == null) {
                var tip = os.CreateObject<TipDocument>();
                tip.Cod = t.Cod;
                tip.Denumire = t.Denumire;
                tip.ClrType = t.ClrType;
            }
        }
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

    // Minimul pentru vertical slice-ul NotaTransfer: două gestiuni + o unitate.
    static void SeedRepartitori(IObjectSpace os) {
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG1") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG1";
            g.Denumire = "Magazia centrală";
        }
        if (os.FirstOrDefault<Gestiune>(x => x.Cod == "MAG2") == null) {
            var g = os.CreateObject<Gestiune>();
            g.Cod = "MAG2";
            g.Denumire = "Magazia secundară";
        }
        if (os.FirstOrDefault<UnitateInterna>(x => x.Cod == "SEDIU") == null) {
            var u = os.CreateObject<UnitateInterna>();
            u.Cod = "SEDIU";
            u.Denumire = "Sediul central";
        }
    }

    // Pivotul gardienilor din decizia 14; anul curent de lucru, deschis.
    static void SeedPerioadeFiscale(IObjectSpace os) {
        if (os.GetObjectsCount(typeof(PerioadaFiscala), null) > 0)
            return;
        for (int luna = 1; luna <= 12; luna++) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = 2026;
            p.Luna = luna;
            p.Inchisa = false;
        }
    }

    // Inventar 04: −1 predator / +1 primitor, ACELAȘI tip stoc (magazie);
    // Clasa=null ⇒ regula acoperă toate clasele cu Natura=Stoc.
    // Contare: NICIUN rând — la plan sintetic transferul nu mișcă conturi;
    // mutarea între gestiuni trăiește în registrul de stoc (+ dimensiunea
    // Repartitor), nu în note 3xx=3xx (zgomotul legacy nu se preia).
    static void SeedPoliticiNotaTransfer(IObjectSpace os) {
        var btr = os.FirstOrDefault<TipDocument>(x => x.Cod == "BTR");
        if (os.FirstOrDefault<PoliticaNumerotare>(x => x.TipDocument.Cod == "BTR") == null) {
            var numerotare = os.CreateObject<PoliticaNumerotare>();
            numerotare.TipDocument = btr;
            numerotare.Serie = "BTR-";
            numerotare.UrmatorulNumar = 1;
        }
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
}
