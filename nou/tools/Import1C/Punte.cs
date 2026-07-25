using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Import1C;

// PASUL 3 al feliei 1C-c: NTC-puntea — transcrierea diferențelor pe care
// documentul tipizat nu le poate exprima.
//
// Principiul, și e singurul care ține importul onest: **puntea corectează FORMA
// (perechea de conturi), niciodată VALOAREA**. Diferențele de valoare dintre
// Atlas și 1C pe conturile de stoc sunt consecința netării deschiderii (47d) —
// §8.3 le declară diferențe JUSTIFICATE, purtate înainte și raportate; o punte
// care le-ar „repara" ar rupe legătura dintre registrul de stoc și cel contabil
// exact acolo unde reconcilierea trebuie să fie lizibilă.
//
// Mecanica: se acumulează, pe simbol de cont, mișcarea de DEBIT semnată a ceea
// ce ar fi trebuit (ținta 1C) minus ceea ce postează Atlas. Ce rămâne se împerechează
// și devine nota. Când ținta și actualul au aceeași pereche de conturi, handlerul
// nu declară nimic — diferența de valoare rămâne unde îi e locul, în raport.
sealed class Punte {
    readonly Dictionary<string, decimal> delta = new(StringComparer.Ordinal);
    // Etichetele de raport se numără LOCAL și se varsă în contorul global doar
    // dacă nota chiar se scrie: altfel „am declarat o țintă" (ceea ce facem
    // pentru fiecare rând al fiecărei facturi) ar arăta ca „am găsit o
    // diferență", iar raportul ar fi zgomot pur.
    readonly Dictionary<string, int> categorii = new(StringComparer.Ordinal);
    string categorie;

    const decimal Eps = 0.005m;

    public IReadOnlyDictionary<string, int> Categorii => categorii;

    // Categoria e eticheta de RAPORT a diferenței (ce anume n-a încăput în
    // documentul tipizat) — se pune înaintea perechii de declarații.
    public Punte Categoria(string numeleDiferentei) {
        categorie = numeleDiferentei;
        return this;
    }

    // Rândul pe care sursa îl are (ținta) — conturile deja mapate pe OMFP.
    public void Tinta1C(string debit, string credit, decimal valoare) {
        Adauga(debit, valoare);
        Adauga(credit, -valoare);
        if (categorie != null)
            categorii[categorie] = categorii.GetValueOrDefault(categorie) + 1;
    }

    // Rândul pe care îl postează Atlas pentru aceeași realitate economică.
    public void ActualAtlas(string debit, string credit, decimal valoare) {
        Adauga(debit, -valoare);
        Adauga(credit, valoare);
    }

    void Adauga(string simbol, decimal miscareDebit) {
        if (simbol == null)
            return;
        delta[simbol] = delta.GetValueOrDefault(simbol) + miscareDebit;
    }

    public bool AreCeva => delta.Values.Any(v => Math.Abs(v) >= Eps);

    // Împerecherea: conturile rămase în debit se sting cu cele rămase în credit,
    // lacom, în ordine stabilă. Reziduul (o punte care nu se echilibrează) NU se
    // scrie: ar fi o notă cu debit ≠ credit, adică o eroare de raportat, nu de
    // ascuns.
    public IReadOnlyList<(string Debit, string Credit, decimal Valoare)> Randuri(out decimal reziduu) {
        reziduu = delta.Values.Sum();
        var randuri = new List<(string, string, decimal)>();
        if (Math.Abs(reziduu) >= Eps)
            return randuri;
        var debite = delta.Where(x => x.Value >= Eps)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => (Cont: x.Key, Suma: x.Value)).ToList();
        var credite = delta.Where(x => x.Value <= -Eps)
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => (Cont: x.Key, Suma: -x.Value)).ToList();
        int i = 0, j = 0;
        while (i < debite.Count && j < credite.Count) {
            var suma = Math.Min(debite[i].Suma, credite[j].Suma);
            randuri.Add((debite[i].Cont, credite[j].Cont, suma));
            debite[i] = (debite[i].Cont, debite[i].Suma - suma);
            credite[j] = (credite[j].Cont, credite[j].Suma - suma);
            if (debite[i].Suma < Eps)
                i++;
            if (credite[j].Suma < Eps)
                j++;
        }
        return randuri;
    }
}

// Contoarele de punte ale rulării: câte diferențe, de ce fel, cu ce valoare —
// diagnosticul care spune dacă importul e „câteva artefacte" sau „o mapare
// greșită sistematic". Se raportează per lună și la final.
sealed class ContorPunti {
    readonly Dictionary<string, int> peCategorie = new(StringComparer.Ordinal);
    readonly Dictionary<string, (int Randuri, decimal Valoare)> pePereche = new(StringComparer.Ordinal);

    public int Note { get; private set; }
    public int Nedeclarate { get; private set; }

    public void NumaraNota(IReadOnlyList<(string Debit, string Credit, decimal Valoare)> randuri,
            IReadOnlyDictionary<string, int> categorii) {
        Note++;
        foreach (var (categorie, n) in categorii)
            peCategorie[categorie] = peCategorie.GetValueOrDefault(categorie) + n;
        foreach (var r in randuri) {
            var cheie = $"{r.Debit} = {r.Credit}";
            var (n, v) = pePereche.GetValueOrDefault(cheie);
            pePereche[cheie] = (n + 1, v + r.Valoare);
        }
    }

    public void NumaraNedeclarata() => Nedeclarate++;

    public void Raporteaza() {
        if (Note == 0 && peCategorie.Count == 0)
            return;
        Console.WriteLine($"  Punți NTC: {Note} note scrise, {Nedeclarate} dezechilibrate (nescrise). "
            + "Etichetele și perechile de mai jos numără DOAR notele scrise.");
        foreach (var c in peCategorie.OrderByDescending(x => x.Value))
            Console.WriteLine($"    {c.Value,8} × {c.Key}");
        foreach (var p in pePereche.OrderByDescending(x => Math.Abs(x.Value.Valoare)).Take(20))
            Console.WriteLine($"    {p.Value.Randuri,8} rânduri {p.Key,-24} Σ {p.Value.Valoare,15:N2}");
    }
}

static class Punti {
    // Nota-punte a unui document 1C: `NotaContabila` cu postare explicită pe
    // linie (46b), laturi interne, numerotare proprie derivată din numărul 1C.
    // Se importă ca document de sine stătător, deci idempotentă și re-operabilă
    // exact ca documentul pe care îl însoțește.
    //
    // `cheie` e cheia de idempotență, iar alegerea ei e o decizie, nu o formalitate:
    // când sursa PRODUCE un document tipizat, puntea primește cheia lui + sufix;
    // când NU produce (o factură al cărei conținut stă într-o secțiune pe care n-o
    // citim, un transfer în aceeași gestiune), puntea preia CHEIA SURSEI. Altfel
    // cheia sursei n-ar fi legată de nimic, iar rularea următoare ar replanifica
    // documentul la nesfârșit — și ar rescrie puntea de fiecare dată.
    public static void Scrie(BuclaImport bucla, string view, string cheie, string numar1C,
            DateOnly data, Punte punte, ContorPunti contor, Action<string> avert) {
        if (!punte.AreCeva)
            return;
        var randuri = punte.Randuri(out var reziduu);
        if (randuri.Count == 0) {
            contor.NumaraNedeclarata();
            avert($"1C:{view}/{cheie}: puntea NTC nu se echilibrează (reziduu {reziduu:N2}) "
                + "— nu se scrie. Diferența rămâne vizibilă în reconciliere.");
            return;
        }
        var stare = bucla.ImportaDocument(view, cheie, os => {
            var catalog = bucla.Catalog;
            var nota = os.CreateObject<NotaContabila>();
            nota.Data = data;
            nota.Numar = $"{numar1C}-P";
            nota.PredatorId = catalog.SediuId;
            nota.PrimitorId = catalog.SediuId;
            foreach (var (debit, credit, valoare) in randuri) {
                var linie = os.CreateObject<NotaContabilaDetaliu>();
                linie.Document = nota;
                linie.TipMaterialId = catalog.TipTrezorerieId;
                linie.ContDebitId = catalog.Plan[debit];
                linie.ContCreditId = catalog.Plan[credit];
                linie.Valoare = valoare;
                linie.Descriere = $"Punte import 1C {numar1C}";
            }
            return nota;
        });
        // Contorizarea vine DUPĂ import, nu înainte: o punte deja scrisă de o
        // rulare anterioară nu e o diferență găsită acum.
        if (stare == StareImport.Importat)
            contor.NumaraNota(randuri, punte.Categorii);
    }
}
