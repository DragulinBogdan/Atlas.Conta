namespace Import1C;

// Auto-testul CONTRACTULUI DE COLOANE (design §2), pereche cu `--sabotaj`.
//
// Cititorii din `FlaxDocumente.cs` sunt SQL construit din nume de coloane: o
// coloană redenumită sau dispărută la regenerarea view-urilor nu se vede la
// compilare, iar în bucla lunii ar apărea ca eșec al unui handler — adică drept
// gaură de DATE, nu drept ruptură de contract. Flag-ul `--cititori` cheamă
// fiecare cititor o dată pe o lună reală și raportează numărul de rânduri: toate
// verde = view-urile mai au forma pe care s-a scris importul.
//
// Rulează opt-in (nu la fiecare rulare): e o probă de formă, nu de conținut, iar
// costul e o interogare per secțiune. Regenerarea view-urilor = motivul să-l
// rulezi, exact ca precondiția „log curat la generare" din §2.
static class SmokeCititori {

    public static void Executa(FlaxDb flax, int an, int luna, Action<string, bool> check) {
        Console.WriteLine($"\n=== SMOKE CITITORI (luna {luna:00}/{an}, contractul de coloane §2) ===");
        var esecuri = 0;
        var randuri = 0;

        void Proba<T>(string nume, Func<List<T>> cititor) {
            try {
                var lista = cititor();
                randuri += lista.Count;
                Console.WriteLine($"  {nume,-46} {lista.Count,8} rânduri");
            }
            catch (Exception ex) {
                esecuri++;
                Console.WriteLine($"  {nume,-46} EȘEC: {ex.Message}");
            }
        }

        Proba("Partenerii (identitate fiscală, D394)", () => flax.ParteneriEsantion());
        List<(string PartenerId, FlaxAdresa Adresa)> adrese = null;
        Proba("Adrese partener (InformatiaDeContact, D15-D6)", () => adrese = flax.AdreseEsantion());
        List<FlaxOrganizatie> organizatii = null;
        Proba("Organizatii (societatea raportoare, D16-D6)", () => organizatii = flax.Organizatii());
        List<FlaxNomenclator> nomenclator = null;
        Proba("Nomenclator (UM + NIC, D16-D6)", () => nomenclator = flax.NomenclatorEsantion());
        Proba("Aprovizionari", () => flax.Aprovizionari(an, luna));
        Proba("AprovizionariMarfuri", () => flax.AprovizionariMarfuri(an, luna));
        Proba("AprovizionariServicii", () => flax.AprovizionariServicii(an, luna));
        Proba("Vanzari", () => flax.Vanzari(an, luna));
        Proba("VanzariMarfuri", () => flax.VanzariMarfuri(an, luna));
        Proba("VanzariServicii", () => flax.VanzariServicii(an, luna));
        Proba("Transferuri", () => flax.Transferuri(an, luna));
        Proba("TransferuriMarfuri", () => flax.TransferuriMarfuri(an, luna));
        Proba("BonuriConsum", () => flax.BonuriConsum(an, luna));
        Proba("BonuriConsumMateriale", () => flax.BonuriConsumMateriale(an, luna));
        Proba("MaririStoc", () => flax.MaririStoc(an, luna));
        Proba("MaririStocMarfuri", () => flax.MaririStocMarfuri(an, luna));
        Proba("DiminuariStoc", () => flax.DiminuariStoc(an, luna));
        Proba("DiminuariStocMarfuri", () => flax.DiminuariStocMarfuri(an, luna));
        Proba("RetururiClient", () => flax.RetururiClient(an, luna));
        Proba("RetururiClientMarfuri", () => flax.RetururiClientMarfuri(an, luna));
        Proba("RetururiClientServicii", () => flax.RetururiClientServicii(an, luna));
        Proba("RetururiFurnizor", () => flax.RetururiFurnizor(an, luna));
        Proba("RetururiFurnizorMarfuri", () => flax.RetururiFurnizorMarfuri(an, luna));
        Proba("RetururiFurnizorServicii", () => flax.RetururiFurnizorServicii(an, luna));
        Proba("Asamblari", () => flax.Asamblari(an, luna));
        Proba("AsamblariArticole", () => flax.AsamblariArticole(an, luna));
        Proba("AsamblariSubasamble", () => flax.AsamblariSubasamble(an, luna));
        Proba("Dezasamblari", () => flax.Dezasamblari(an, luna));
        Proba("DezasamblariArticole", () => flax.DezasamblariArticole(an, luna));
        Proba("DezasamblariSubasamble", () => flax.DezasamblariSubasamble(an, luna));
        Proba("RapoarteAmanunt", () => flax.RapoarteAmanunt(an, luna));
        Proba("RapoarteAmanuntMarfuri", () => flax.RapoarteAmanuntMarfuri(an, luna));
        Proba("RapoarteAmanuntServicii", () => flax.RapoarteAmanuntServicii(an, luna));
        Proba("RapoarteAmanuntFacturi", () => flax.RapoarteAmanuntFacturi(an, luna));
        Proba("RapoarteAmanuntInchidere", () => flax.RapoarteAmanuntInchidere(an, luna));
        Proba("AvizeIesire", () => flax.AvizeIesire(an, luna));
        Proba("AvizeIesireMarfuri", () => flax.AvizeIesireMarfuri(an, luna));
        Proba("AvizeIntrare", () => flax.AvizeIntrare(an, luna));
        Proba("AvizeIntrareMarfuri", () => flax.AvizeIntrareMarfuri(an, luna));
        Proba("Extrase", () => flax.Extrase(an, luna));
        Proba("ExtraseRanduri", () => flax.ExtraseRanduri(an, luna));
        Proba("Plati", () => flax.Plati(an, luna));
        Proba("PlatiRanduri", () => flax.PlatiRanduri(an, luna));
        Proba("Incasari", () => flax.Incasari(an, luna));
        Proba("IncasariRanduri", () => flax.IncasariRanduri(an, luna));
        Proba("Compensari", () => flax.Compensari(an, luna));
        Proba("CompensariDebit", () => flax.CompensariDebit(an, luna));
        Proba("CompensariCredit", () => flax.CompensariCredit(an, luna));
        Proba("Operatii", () => flax.Operatii(an, luna));
        Proba("OperatiiRanduri", () => flax.OperatiiRanduri(an, luna));
        Proba("Salarii", () => flax.Salarii(an, luna));
        Proba("CasariMF", () => flax.CasariMF(an, luna));
        Proba("InchideriLuna", () => flax.InchideriLuna(an, luna));

        // Adresele (D15-D6): eșantionul se TIPĂREȘTE, nu doar se numără. Maparea
        // e pe coloane fizice numerotate (`Field1`, `Field3`, …), al căror
        // înțeles nu-l apără compilatorul și nici numărul de rânduri — singura
        // probă onestă e ca omul să vadă că în coloana „județ" scrie un județ.
        // Și cele două forme (una per partener, una în lot) trebuie să dea
        // ACELAȘI rând pentru același partener: altfel materializarea și
        // reclasificarea ar scrie adrese diferite pentru același om.
        if (adrese is { Count: > 0 }) {
            Console.WriteLine($"  {"  → eșantion (cod poștal | județ/cod CNP | localitate | stradă | nr | clădire)",-46}");
            foreach (var (id, a) in adrese.Take(8))
                Console.WriteLine($"      {id[..8]}… {a.CodPostal ?? "–"} | {a.JudetDenumire ?? "–"}"
                    + $"/{a.CodJudetCnp?.ToString() ?? "–"} | {a.Localitate ?? "–"} | {a.Strada ?? "–"}"
                    + $" | {a.Numar ?? "–"} | {a.Cladire ?? "–"}");
            var (unId, unaAdresa) = adrese[0];
            var perPartener = flax.AdresaPartener(unId);
            var inLot = flax.AdreseParteneri(adrese.Select(x => x.PartenerId));
            check($"Smoke cititori: adresa lui {unId[..8]}… e aceeași pe cele trei căi "
                + "(eșantion / per partener / în lot)",
                perPartener == unaAdresa && inLot.GetValueOrDefault(unId) == unaAdresa);
            check($"Smoke cititori: forma în lot întoarce {inLot.Count} adrese pentru "
                + $"{adrese.Count} parteneri cu adresă (una per partener, fără dubluri)",
                inLot.Count == adrese.Count);
        }

        // Societatea (D16-D6): cele patru cititoare ale antetului sunt legate de
        // ACELAȘI obiect (organizația), pe registre diferite. Se tipăresc, la fel
        // ca eșantionul de adrese și pentru același motiv: adresa societății iese
        // tot din coloane fizice numerotate, iar singura probă onestă e ca omul să
        // vadă că în coloana „județ" scrie un județ. Numărul de organizații se
        // AFIȘEAZĂ, nu se verifică: „câte sunt" e un fapt al bazei-sursă, iar
        // decizia (care raportează) e a operatorului, nu a smoke-ului.
        if (organizatii is { Count: > 0 }) {
            foreach (var o in organizatii) {
                var a = flax.AdresaOrganizatie(o.Id);
                var (telefon, email) = flax.ContactOrganizatie(o.Id);
                var (nume, functie) = flax.ConducatorulOrganizatiei(o.Id);
                Console.WriteLine($"      {o.Cod} „{o.DenumireCompleta ?? o.Denumire}” CUI {o.CodUnic ?? "–"} "
                    + $"RegCom {o.RegCom ?? "–"}");
                Console.WriteLine($"        adresă: {a?.JudetDenumire ?? "–"} | {a?.Localitate ?? "–"} | "
                    + $"{a?.Strada ?? "–"} | {a?.Numar ?? "–"}; tel {telefon ?? "–"}; "
                    + $"mail {email ?? "–"}; conducător {nume ?? "–"} ({functie ?? "–"})");
            }
            check($"Smoke cititori: cele {organizatii.Count} organizații au KeyField și denumire "
                + "(contractul de coloane al antetului SAF-T)",
                organizatii.All(o => o.Id != null && (o.Denumire ?? o.DenumireCompleta) != null));
            // Fixul C4 al review-ului: contractul de coloane nu se termină la
            // „coloana există". Antetul SAF-T are nevoie de UN raportor cu UN cod
            // fiscal — `Societate1C` refuză oricum să scrie ceva pe mai multe
            // organizații, iar un `CodUnic` gol face fișierul nedepozabil. Deci
            // se VERIFICĂ, nu doar se afișează: dacă baza-sursă are alt număr de
            // organizații, asta e o ruptură de premisă a importului, nu o
            // observație de rulare.
            check($"Smoke cititori: `flax.Organizatii` are EXACT o organizație ({organizatii.Count}) și ea are "
                + "cod unic — premisa antetului SAF-T (`--societate` scrie un singur raportor)",
                organizatii.Count == 1 && organizatii.All(o => !string.IsNullOrWhiteSpace(o.CodUnic)));
        }

        // Nomenclatorul: `NIC` e coloana NOUĂ a feliei 16 și n-are alt apărător
        // decât proba asta. Cele două forme (una per id, una în lot) trebuie să
        // dea ACELAȘI rând — altfel materializarea și pasul `--um-nc` ar scrie
        // unități și coduri NC diferite pentru același produs.
        if (nomenclator is { Count: > 0 }) {
            Console.WriteLine($"  {"  → eșantion nomenclator (cod | UM | NIC)",-46}");
            foreach (var n in nomenclator.Take(8))
                Console.WriteLine($"      {n.Cod ?? "–"} | {n.UM ?? "–"} | {n.Nic ?? "–"}");
            var unul = nomenclator[0];
            var inLot = flax.NomenclatoareDupaIds(nomenclator.Select(n => n.Id));
            check($"Smoke cititori: nomenclatorul {unul.Cod} e identic pe cele două căi "
                + "(per id / în lot), inclusiv pe `UM` și `NIC`",
                flax.NomenclatorDupaId(unul.Id) == unul && inLot.GetValueOrDefault(unul.Id) == unul);
            check($"Smoke cititori: forma în lot întoarce {inLot.Count} nomenclatoare pentru "
                + $"{nomenclator.Count} cerute (una per id, fără dubluri)",
                inLot.Count == nomenclator.Count);
        }

        // Registrul contabil: forma PE LUNĂ (cea folosită de handler-e) și cea per
        // document (diagnostic) trebuie să dea EXACT aceleași rânduri — altfel una
        // dintre ele minte, iar identitatea de lot ar depinde de calea aleasă.
        Dictionary<string, List<FlaxRandNota>> note = null;
        Dictionary<string, List<FlaxSubcontoNota>> subconto = null;
        Proba("RanduriNotaPeLuna (documente)", () => (note = flax.RanduriNotaPeLuna(an, luna)).Keys.ToList());
        Proba("SubcontoNotaPeLuna (documente)", () => (subconto = flax.SubcontoNotaPeLuna(an, luna)).Keys.ToList());
        if (note != null)
            Console.WriteLine($"  {"  → rânduri de notă",-46} {note.Sum(x => x.Value.Count),8} rânduri");
        if (subconto != null)
            Console.WriteLine($"  {"  → rânduri de subconto",-46} {subconto.Sum(x => x.Value.Count),8} rânduri");

        if (note is { Count: > 0 } && subconto != null) {
            var doc = note.Keys.OrderBy(k => k, StringComparer.Ordinal).First();
            var pePerechi = flax.RanduriNota(doc);
            var subPerechi = flax.SubcontoNota(doc);
            check($"Smoke cititori: RanduriNota({doc[..8]}…) per document = forma pe lună "
                + $"({pePerechi.Count} vs {note[doc].Count} rânduri)",
                pePerechi.SequenceEqual(note[doc]));
            check($"Smoke cititori: SubcontoNota({doc[..8]}…) per document = forma pe lună "
                + $"({subPerechi.Count} vs {subconto.GetValueOrDefault(doc)?.Count ?? 0} rânduri)",
                subPerechi.SequenceEqual(subconto.GetValueOrDefault(doc) ?? []));
        }

        check($"Smoke cititori: toate cele 55 de cititoare ale lunii {luna:00}/{an} rulează pe "
            + $"view-urile reale ({esecuri} eșecuri, {randuri} rânduri citite)", esecuri == 0);
    }
}
