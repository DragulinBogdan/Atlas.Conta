using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Atlas.Conta.BackOffice.ModelCheck;
using Atlas.Conta.BackOffice.Module.Anaf;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Asm;
using Atlas.Conta.BackOffice.Module.Api.Bcs;
using Atlas.Conta.BackOffice.Module.Api.Btr;
using Atlas.Conta.BackOffice.Module.Api.Dec;
using Atlas.Conta.BackOffice.Module.Api.Dsc;
using Atlas.Conta.BackOffice.Module.Api.Dvi;
using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.Api.Amo;
using Atlas.Conta.BackOffice.Module.Api.Cas;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.Api.Imo;
using Atlas.Conta.BackOffice.Module.Api.Itv;
using Atlas.Conta.BackOffice.Module.Api.Ldi;
using Atlas.Conta.BackOffice.Module.Api.Nir;
using Atlas.Conta.BackOffice.Module.Api.Ntc;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.Api.Pif;
using Atlas.Conta.BackOffice.Module.Api.Politici;
using Atlas.Conta.BackOffice.Module.Api.Rdc;
using Atlas.Conta.BackOffice.Module.Api.Rlf;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.DatabaseUpdate;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using Atlas.Conta.BackOffice.Module.Saft;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.EFCore;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;
using SecurityPermissionPolicy = DevExpress.Persistent.Base.SecurityPermissionPolicy;
using SecurityPermissionState = DevExpress.Persistent.Base.SecurityPermissionState;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.ModelCheck;

static class E2eSondeBaza {
    public static async Task Ruleaza(Suita s) {
        using (var ctx = new BackOfficeEFCoreDbContext(s.Opts)) {
            Console.WriteLine($"TipuriDocument:  {await ctx.TipuriDocument.CountAsync()}");
            Console.WriteLine($"ClaseProduse:    {await ctx.ClaseProduse.CountAsync()}");
            Console.WriteLine($"TipuriMaterial:  {await ctx.TipuriMaterial.CountAsync()}");
            Console.WriteLine($"Conturi:         {await ctx.Conturi.CountAsync()} (din care cu defalcare: {await ctx.Conturi.CountAsync(c => c.DimensiuniObligatorii != DimensiuneFlags.Niciuna)})");
            Console.WriteLine($"Repartitori:     {await ctx.Repartitori.CountAsync()}");
            Console.WriteLine($"PerioadeFiscale: {await ctx.PerioadeFiscale.CountAsync()}");
            Console.WriteLine($"TipuriTva:       {await ctx.TipuriTva.CountAsync()}");
            Console.WriteLine($"ReguliStoc:      {await ctx.ReguliStoc.CountAsync()}");
            Console.WriteLine($"ReguliContare:   {await ctx.ReguliContare.CountAsync()}");

            // ═══ F20-D1 — ORACOLUL căutării fără diacritice ═══
            // Coloana `Cautare` e GENERATĂ de Postgres (`lower` + `translate`);
            // `Cautare.Normalizeaza` e aceeași transformare în C#, iar clientul o va
            // aplica literalului tastat înainte de a-l trimite. Dacă cele două ar
            // diverge (un caracter în plus în tabel, un `lower` care nu coincide),
            // căutarea n-ar da eroare — ar TĂCEA, ceea ce e mult mai rău. Proba: pentru
            // TOATE rândurile, valoarea citită din bază == valoarea calculată în
            // memorie. Entitățile se descoperă din model exact ca în `OnModelCreating`
            // (interfața + proprietatea DECLARATĂ pe tip — coloana e a bazei),
            // deci un nomenclator nou intră automat și în probă.
            {
                s.Check("Cautare: tabelul De/La are lungimi egale",
                    Cautare.De.Length == Cautare.La.Length);
                s.Check("Cautare: tabelul De n-are caractere duplicate",
                    Cautare.De.Distinct().Count() == Cautare.De.Length);

                var conn = ctx.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                    await conn.OpenAsync();
                var totalRanduri = 0;
                var totalEntitati = 0;
                foreach (var et in ctx.Model.GetEntityTypes()) {
                    var clr = et.ClrType;
                    if (clr == null || !typeof(ICuCautare).IsAssignableFrom(clr))
                        continue;
                    if (et.FindProperty(Cautare.NumeColoana)?.DeclaringType != et)
                        continue;
                    var tabel = et.GetTableName();
                    if (tabel == null)
                        continue;
                    var obiectStocare = Microsoft.EntityFrameworkCore.Metadata.StoreObjectIdentifier
                        .Table(tabel, et.GetSchema());
                    string Coloana(string membru) => et.FindProperty(membru)?.GetColumnName(obiectStocare);
                    var colCod = Cautare.NumeCod(clr) is { } numeCod ? Coloana(numeCod) : null;
                    var colDenumire = Coloana("Denumire");
                    var colCautare = Coloana(Cautare.NumeColoana);

                    using var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        $"select {(colCod == null ? "null" : $"\"{colCod}\"")}, \"{colDenumire}\", \"{colCautare}\" "
                        + $"from \"{tabel}\"";
                    var neconforme = 0;
                    var randuri = 0;
                    string exemplu = null;
                    using (var cititor = await cmd.ExecuteReaderAsync()) {
                        while (await cititor.ReadAsync()) {
                            randuri++;
                            var cod = cititor.IsDBNull(0) ? null : cititor.GetString(0);
                            var denumire = cititor.IsDBNull(1) ? null : cititor.GetString(1);
                            var dinBaza = cititor.IsDBNull(2) ? null : cititor.GetString(2);
                            var asteptat = Cautare.Compune(cod, denumire);
                            if (dinBaza == asteptat)
                                continue;
                            neconforme++;
                            exemplu ??= $"„{cod}” / „{denumire}” ⇒ SQL „{dinBaza}” vs C# „{asteptat}”";
                        }
                    }
                    totalRanduri += randuri;
                    totalEntitati++;
                    s.Check($"Cautare == Normalizeaza pe {clr.Name} ({randuri} rânduri)"
                        + (neconforme > 0 ? $" — {neconforme} neconforme, ex. {exemplu}" : ""), neconforme == 0);
                }
                Console.WriteLine($"Cautare: {totalEntitati} entități ICuCautare, {totalRanduri} rânduri verificate.");
            }

            // ═══ 77-r2 — `Cod`/`Simbol` + `Denumire` OBLIGATORII pe orice nomenclator căutabil ═══
            // Trei uși, trei probe: (1) SCHEMA — pe fiecare `ICuCautare` coloana de cod
            // și `Denumire` sunt NOT NULL și poartă CHECK-ul „ne-gol” (descoperite din
            // model exact ca mai sus, deci un nomenclator nou intră automat);
            // (2) GARDIANUL — pe ușa secured refuză cu mesajul CÂMPULUI (nu al
            // constraint-ului), inclusiv pe spații, și tace pe un rând complet;
            // (3) BAZA — pe ușa fără gardian (ModelCheck e exact „calea standalone”
            // din antetul gardianului) commit-ul pică pe constraint, iar refuzul iese
            // tradus ca mesaj de domeniu (39a), nu ca 23502/23514 brut.
            {
                var lipsuri = new List<string>();
                var entitati = 0;
                // CHECK-urile nu stau în modelul „read-optimized" de runtime — se citesc
                // din modelul design-time (același din care iese migrația).
                var modelDesign = Microsoft.EntityFrameworkCore.Infrastructure.AccessorExtensions
                    .GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>(ctx).Model;
                foreach (var et in modelDesign.GetEntityTypes()) {
                    var clr = et.ClrType;
                    if (clr == null || !typeof(ICuCautare).IsAssignableFrom(clr))
                        continue;
                    if (et.FindProperty(Cautare.NumeColoana)?.DeclaringType != et)
                        continue;
                    entitati++;
                    var tabel = et.GetTableName();
                    var reguli = et.GetCheckConstraints().Select(c => c.Name).ToHashSet(StringComparer.Ordinal);
                    foreach (var coloana in new[] { Cautare.NumeCod(clr), Cautare.NumeDenumire }) {
                        if (coloana == null)
                            continue;
                        var prop = et.FindProperty(coloana);
                        if (prop == null || prop.IsNullable)
                            lipsuri.Add($"{clr.Name}.{coloana} nullable");
                        if (!reguli.Contains(Cautare.NumeRegulaNeGol(tabel, coloana)))
                            lipsuri.Add($"{clr.Name}.{coloana} fără CHECK");
                    }
                }
                s.Check($"77-r2 schema: pe toate cele {entitati} entități ICuCautare coloana de cod și `Denumire` sunt "
                    + "NOT NULL și poartă CHECK-ul „ne-gol” (`btrim <> ''`)"
                    + (lipsuri.Count > 0 ? $" — lipsesc: {string.Join(", ", lipsuri)}" : ""),
                    entitati >= 10 && lipsuri.Count == 0);

                // (2) Gardianul — în memorie, fără commit, ca proba pe `Tara`.
                string mesajGol, mesajSpatii, mesajCont, mesajComplet = null;
                using (var osG = s.Provider.CreateObjectSpace()) {
                    var p = osG.CreateObject<Partener>();
                    p.Cod = null; p.Denumire = null;
                    mesajGol = s.Refuz(() => GardianEditare.Verifica(osG));
                    p.Cod = "   "; p.Denumire = "\t";
                    mesajSpatii = s.Refuz(() => GardianEditare.Verifica(osG));
                    p.Cod = "77R2"; p.Denumire = "Partener 77-r2";
                    var c = osG.CreateObject<Cont>();
                    c.Simbol = ""; c.Denumire = "Cont fără simbol";
                    mesajCont = s.Refuz(() => GardianEditare.Verifica(osG));
                    c.Simbol = "9977";
                    mesajComplet = s.Refuz(() => GardianEditare.Verifica(osG));
                }
                Console.WriteLine($"     MĂSURAT (77-r2/gardian): gol → „{mesajGol}”; spații → „{mesajSpatii}”; "
                    + $"cont fără simbol → „{mesajCont}”; complet → „{mesajComplet ?? "acceptat"}”.");
                s.Check("77-r2 gardian: `Partener` fără cod și denumire e refuzat cu AMBELE mesaje ale câmpurilor; "
                    + "spațiile/tab-ul nu sunt valori; `Cont` fără `Simbol` e refuzat pe numele LUI de cod; "
                    + "rândurile complete trec",
                    mesajGol != null && mesajGol.Contains("„Cod” este obligatoriu") && mesajGol.Contains("„Denumire” este obligatorie")
                    && mesajSpatii != null && mesajSpatii.Contains("„Cod”") && mesajSpatii.Contains("„Denumire”")
                    && mesajCont != null && mesajCont.Contains("„Simbol” este obligatoriu") && !mesajCont.Contains("„Cod”")
                    && mesajComplet == null);

                // (3) Baza — ușa FĂRĂ gardian. Template-urile RO sunt ale host-ului
                // (idempotent, ca la D17-V1).
                Atlas.Conta.BackOffice.Module.BusinessObjects.MesajeConstraintRo.Aplica();
                string Comite(Action<IObjectSpace> pregateste) {
                    using var osB = s.Provider.CreateObjectSpace();
                    pregateste(osB);
                    try {
                        osB.CommitChanges();
                        return null;
                    }
                    catch (Exception e) {
                        var violare = Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationTranslator.TryTranslate(e);
                        return violare != null
                            ? Atlas.DXF.EfCore.Database.Exceptions.ConstraintViolationMessages.Format(violare)
                            : e.GetBaseException().Message;
                    }
                }
                var mesajNull = Comite(os => { var g = os.CreateObject<Gestiune>(); g.Cod = "77R2-NULL"; g.Denumire = null; });
                var mesajBlank = Comite(os => { var g = os.CreateObject<Gestiune>(); g.Cod = "  "; g.Denumire = "Gestiune 77-r2"; });
                var mesajProdus = Comite(os => { var pr = os.CreateObject<Produs>(); pr.Cod = "77R2"; pr.Denumire = ""; });
                Console.WriteLine($"     MĂSURAT (77-r2/bază): Denumire null → „{mesajNull ?? "<A TRECUT>"}”; Cod „  ” → "
                    + $"„{mesajBlank ?? "<A TRECUT>"}”; Produs cu Denumire „” → „{mesajProdus ?? "<A TRECUT>"}”.");
                s.Check("77-r2 baza (ușa fără gardian): NULL pică pe NOT NULL, „  ”/„” pică pe CHECK-ul `btrim <> ''` "
                    + "(pe `Repartitori` — tabela ierarhiei TPH — și pe `Produse`), iar refuzul iese ca mesaj de domeniu "
                    + "tradus (39a: „este obligatoriu” / numele regulii `CK_…_negol`), nu ca 23502/23514 brut",
                    mesajNull != null && mesajNull.Contains("obligatori")
                    && mesajBlank != null && mesajBlank.Contains(Cautare.NumeRegulaNeGol("Repartitori", "Cod"))
                    && mesajProdus != null && mesajProdus.Contains(Cautare.NumeRegulaNeGol("Produse", "Denumire")));
                s.Check("77-r2 nimic nu rămâne în bază după probele picate",
                    !ctx.Repartitori.Any(r => r.Cod == "77R2-NULL" || r.Denumire == "Gestiune 77-r2")
                    && !ctx.Produse.Any(r => r.Cod == "77R2"));
            }

            // ═══ 22 (F22-D6) — fraza unică a referinței nerezolvabile ═══
            // Ce se probează: pe ușa securizată `GetObjectByKey` întoarce `null` și
            // pentru „nu există", și pentru „există dar securitatea îl ascunde"
            // (`SecurityQueryCompiler` înfășoară orice query, inclusiv `Find`), deci
            // mesajul nu are voie să afirme prima cauză. Cele ~84 de rezolvări de FK ale
            // feliilor trec acum printr-un helper unic, iar fraza LUI e contractul.
            //
            // Ce NU se probează aici (declarat, F22-D9): ModelCheck n-are strategie de
            // securitate — OS-urile lui sunt neautentificate, deci cauza „invizibil" nu
            // se poate PRODUCE. Aceea se măsoară pe HTTP, cu utilizatorul `User`. Aici
            // se fixează doar textul și forma refuzului (domeniu, nu acces).
            {
                using var osR = s.Provider.CreateObjectSpace();
                var idFantoma = Guid.NewGuid();
                var asteptat = $"Furnizorul ({idFantoma}) nu există sau nu e vizibil(ă) "
                    + "pentru utilizatorul curent.";
                var mesajCere = s.Refuz(() => Rezolva.Cere<Partener>(osR, idFantoma, "Furnizorul"));
                var mesajOptional = s.Refuz(() => Rezolva.Optional<Partener>(osR, idFantoma, "Furnizorul"));
                Partener peNull = null;
                var mesajNullOptional = s.Refuz(() => peNull = Rezolva.Optional<Partener>(osR, null, "Furnizorul"));
                // Gol ≠ absent: `Guid.Empty` e o valoare CULEASĂ care nu se rezolvă, deci
                // refuz — semantica de care depinde proba NTC pe `TipMaterialId` gol.
                var mesajGolOptional = s.Refuz(() =>
                    Rezolva.Optional<TipMaterial>(osR, Guid.Empty, "Tipul (contul/clasa)"));
                Console.WriteLine($"     MĂSURAT (F22-D6): Cere → „{mesajCere ?? "<A TRECUT>"}”; "
                    + $"Optional(null) → „{mesajNullOptional ?? "acceptat, null"}”; "
                    + $"Optional(Guid.Empty) → „{mesajGolOptional ?? "<A TRECUT>"}”.");
                s.Check("F22-D6: `Rezolva.Cere` pe un id nerezolvabil refuză ca DOMENIU (`OperareException`, adică "
                    + "422 pe sârmă — referința nu e subiectul cererii, deci nu e 404), cu fraza care spune "
                    + "adevărul („nu există SAU nu e vizibil(ă)”), purtând rolul și id-ul; `Optional` are "
                    + "exact același text",
                    mesajCere == asteptat && mesajOptional == asteptat);
                s.Check("F22-D6: `Optional` cu id absent (null) nu interoghează și nu refuză; `Guid.Empty` NU e "
                    + "„absent” — e o valoare culeasă care nu se rezolvă, deci primește aceeași frază",
                    mesajNullOptional == null && peNull == null
                    && mesajGolOptional == $"Tipul (contul/clasa) ({Guid.Empty}) nu există sau nu e "
                        + "vizibil(ă) pentru utilizatorul curent.");
                s.Check("F22-D6: fraza e a lui `Refuzuri`, o singură sursă — helper-ul nu-și scrie propriul text",
                    Refuzuri.ReferintaInvizibila("Furnizorul", idFantoma) == asteptat);
            }

            // ═══ 78 — căutarea fără diacritice pe PROIECȚII (`DataSourceLoader`) ═══
            // Perechea oracolului F20-D1 de mai sus: acolo se probează COLOANA generată
            // (ușa OData a nomenclatoarelor), aici REscrierea predicatelor de string ale
            // `DataSourceLoader` (`CautareFiltru` + funcția `Cautare.FaraDiacritice`,
            // tradusă de EF) — adică exact ce pun FilterRow și căutarea din HeaderFilter
            // peste listele de documente. Scena: partener cu ambele familii de grafii
            // (Ț-virguliță, Î) pe un FCT draft; proiecția e cea REALĂ a feliei
            // (`FacturaIntrareApply.Lista`), încărcată prin aceeași bibliotecă în care
            // compilarea rulează și pe HTTP.
            {
                CautareFiltru.Inregistreaza();
                using var os = s.Provider.CreateObjectSpace();
                var partener78 = os.CreateObject<Partener>();
                partener78.Cod = "P78-DIA";
                partener78.Denumire = "Țestoasa Înțeleaptă P78";
                var gestiune78 = os.CreateObject<Gestiune>();
                gestiune78.Cod = "P78-GST";
                gestiune78.Denumire = "Gestiune probă 78";
                var fct78 = os.CreateObject<FacturaIntrare>();
                fct78.Numar = "P78-FCT";
                fct78.Data = new DateOnly(2026, 3, 3);
                fct78.Predator = partener78;
                fct78.Primitor = gestiune78;
                os.CommitChanges();

                List<Guid> PrinLoader(string operatie, string valoare) {
                    var optiuni = new DataSourceLoadOptionsBase {
                        Take = 1000,
                        Filter = new object[] { "PredatorDenumire", operatie, valoare },
                    };
                    return DataSourceLoader.Load(FacturaIntrareApply.Lista(os), optiuni)
                        .data.Cast<FacturaIntrareListDto>().Select(r => r.Id).ToList();
                }

                s.Check("78: `contains` prin `DataSourceLoader` e insensibil la diacritice și caz în AMBELE sensuri — "
                    + "literalul fără diacritice găsește rândul cu diacritice și invers, inclusiv grafia veche "
                    + "cu sedilă (ţ U+0163) pentru virgulița din bază (ț U+021B)",
                    PrinLoader("contains", "testoasa inteleapta").Contains(fct78.ID)
                    && PrinLoader("contains", "Țestoasa ÎNȚeleaptă").Contains(fct78.ID)
                    && PrinLoader("contains", "ţestoasa").Contains(fct78.ID));
                s.Check("78: `startswith`/`endswith` trec prin aceeași normalizare, iar `notcontains` e negația exactă",
                    PrinLoader("startswith", "țes").Contains(fct78.ID)
                    && PrinLoader("startswith", "TES").Contains(fct78.ID)
                    && PrinLoader("endswith", "p78").Contains(fct78.ID)
                    && !PrinLoader("notcontains", "testoasa").Contains(fct78.ID));

                // Cusătura de CONTRACT: traducerea funcției emite EXACT fragmentul SQL
                // al coloanei generate — ambele ies din `Cautare.FragmentSql`, iar SQL-ul
                // interogării reale trebuie să conțină prefixul și sufixul fragmentului
                // (între ele stă coloana, redată de EF cu alias-ul lui).
                var marcaj = "\u0001";
                var fragment = Cautare.FragmentSql(marcaj);
                var taietura = fragment.IndexOf(marcaj, StringComparison.Ordinal);
                var sqlProba = FacturaIntrareApply.Lista(os)
                    .Where(r => Cautare.FaraDiacritice(r.PredatorDenumire).Contains("proba"))
                    .ToQueryString();
                s.Check("78: traducerea EF a lui `FaraDiacritice` = fragmentul SQL al coloanei generate "
                    + "(`translate(lower(…), De, La)` — o singură ortografie, `Cautare.FragmentSql`)",
                    sqlProba.Contains(fragment[..taietura]) && sqlProba.Contains(fragment[(taietura + 1)..]));

                new Purja(os)
                    .Adauga(new[] { fct78 })
                    .Adauga(new[] { partener78 })
                    .Adauga(new[] { gestiune78 })
                    .Executa();
                s.Check("78: nimic nu rămâne în bază după scenă",
                    !ctx.Repartitori.Any(r => r.Cod == "P78-DIA" || r.Cod == "P78-GST"));
            }

            // DIM-3: garda mapării PLATE — [Column] trebuie să conserve schema fostului
            // owned (round-trip insert/reread/update pe FK-ul plat al regulii de contare;
            // o nepotrivire de nume de coloană ar pica aici, nu în producție).
            var tipDoc = await ctx.TipuriDocument.FirstAsync();
            var repartitor = await ctx.Repartitori.FirstAsync();
            var proba = ctx.CreateProxy<RegulaContare>(); // XAF creează entitățile proxy — proba la fel
            proba.TipDocumentId = tipDoc.ID;
            proba.ComunRepartitorId = repartitor.ID;
            ctx.ReguliContare.Add(proba);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
            s.Check("Coloana plată (DimensiuniComun_RepartitorId) → FK persistat și recitit", recitita.ComunRepartitorId == repartitor.ID);
            s.Check("Value object construit din coloanele plate", recitita.DimensiuniComun().RepartitorId == repartitor.ID);

            recitita.ComunRepartitorId = null;
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();
            recitita = await ctx.ReguliContare.SingleAsync(r => r.ID == proba.ID);
            s.Check("Update pe coloana plată → schimbare detectată", recitita.ComunRepartitorId == null);

            // Proba e artefact de harness, nu obiect de probă ⇒ purjă FIZICĂ (F13-D2).
            ctx.ChangeTracker.Clear();
            await ctx.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"ReguliContare\" WHERE \"ID\" = {proba.ID}");
        }
    }
}
