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

// Declarația vamală parcursă prin CONTRACTUL feliei: `Aplica` (creare cu
// facturi) → `Citeste` → `FacturiCandidate` → `Aplica` (schimbă legăturile) →
// `ComenziDocument` → refuzul PUT-ului pe document operat → storno. Endpoint-urile
// din host sunt transport peste EXACT acest cod.
//
// Semantica de MOTOR (planul, registrele, D300, gardianul legăturii) e acoperită
// de blocul `E2E-DVI`; aici se probează UȘA. Blocul stă IMEDIAT după `E2E-DVI`,
// care tocmai a purjat FEBRUARIE 2026 și a măsurat că nu mai există nicio
// declarație — precondiția de mai jos o re-măsoară, ca mutarea blocului să iasă
// ca FAIL, nu ca cifre peste conținut străin.
//
// CE NU SE POATE PROBA AICI: gate-urile de acces. ObjectSpace-urile lui
// ModelCheck sunt NESECURIZATE, deci nici `GardianEditare` (armat pe familia
// securizată) și nici verdictele 403/404 ale ușii nu se aplică — legătura
// scrisă prin `FacturiIds` trece aici fără gardian. Ambele au proba lor:
// gardianul în `E2E-DVI` (prin `GardianEditare.Verifica`), gate-urile în
// `nou/tools/ProbeHttp/refuzuri.ps1`, pe host viu.
static class VerificaApiDvi {
    public static void Ruleaza(Suita s) {
        const string Marcaj = "E2E-API-DVI";
        using var os = s.Provider.CreateObjectSpace();

        void CurataApiDvi() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<DviFactura>()
                .Where(f => docIds.Contains(f.DviId) || docIds.Contains(f.FacturaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }
        CurataApiDvi();

        var febStart = new DateOnly(2026, 2, 1);
        var febEnd = new DateOnly(2026, 2, 28);
        var imp = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP");
        var imp21 = os.FirstOrDefault<TipTva>(t => t.Cod == "IMP21");
        var impTi21 = os.FirstOrDefault<TipTva>(t => t.Cod == "IMPTI21");
        var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tip626 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "626");
        var cont446 = os.FirstOrDefault<Cont>(c => c.Simbol == "446");
        s.Check("Api DVI — precondiție: tipurile de import ale profilului există, iar FEBRUARIE 2026 e liberă "
            + "(blocul `E2E-DVI` de dinainte tocmai a purjat-o). Dacă proba asta pică, blocul a fost mutat "
            + "înaintea lui, iar cifrele de mai jos ar fi măsurate peste conținut străin",
            imp != null && imp21 != null && impTi21 != null && n21 != null
            && tip628 != null && tip626 != null && cont446 != null
            && !os.GetObjectsQuery<Dvi>().Any()
            && !os.GetObjectsQuery<Document>().Any(d => d.Data >= febStart && d.Data <= febEnd));

        // ---------------- Scena: trei clase fiscale de furnizor ----------------
        var extern1 = os.CreateObject<Partener>();
        extern1.Cod = Marcaj + "-EXT";
        extern1.Denumire = "Furnizor extra-UE probă Api DVI";
        extern1.Tara = "MA";
        var partenerRo = os.CreateObject<Partener>();
        partenerRo.Cod = Marcaj + "-RO";
        partenerRo.Denumire = "Furnizor român probă Api DVI";
        partenerRo.Tara = "RO";
        partenerRo.InregistratTva = true;
        var partenerUe = os.CreateObject<Partener>();
        partenerUe.Cod = Marcaj + "-UE";
        partenerUe.Denumire = "Furnizor UE probă Api DVI";
        partenerUe.Tara = "DE";
        var vama = os.CreateObject<Partener>();
        vama.Cod = Marcaj + "-VAMA";
        vama.Denumire = "Biroul vamal probă Api DVI";
        vama.Tara = "RO";
        vama.ContImplicit = cont446;
        var gestiune = os.CreateObject<Gestiune>();
        gestiune.Cod = Marcaj + "-MAG";
        gestiune.Denumire = "Gestiune probă Api DVI";
        var unitate = os.CreateObject<UnitateInterna>();
        unitate.Cod = Marcaj + "-UI";
        unitate.Denumire = "Unitate probă Api DVI";
        os.CommitChanges();

        // Facturile candidate: două extra-UE (subiectele legăturii), una RO și una
        // UE (mulțimea pe care filtrul implicit trebuie s-o EXCLUDĂ).
        Guid FacturaOperata(string sufix, Partener furnizor, DateOnly data, TipTva tipTva,
                params (TipMaterial Tip, decimal Pret)[] linii) {
            var fct = os.CreateObject<FacturaIntrare>();
            fct.Numar = Marcaj + sufix;
            fct.Data = data;
            fct.Predator = furnizor;
            fct.Primitor = gestiune;
            foreach (var l in linii) {
                var linie = os.CreateObject<FacturaIntrareDetaliu>();
                linie.Document = fct;
                linie.TipMaterial = l.Tip;
                linie.Cantitate = 1m;
                linie.PretUnitar = l.Pret;
                linie.TipTva = tipTva;
            }
            os.CommitChanges();
            MotorOperare.Opereaza(os, fct);
            return fct.ID;
        }

        var idFf1 = FacturaOperata("-FF1", extern1, new DateOnly(2026, 2, 5), imp, (tip628, 1000m));
        // Două tipuri pe aceeași factură, cu dominant CLAR: `TipMaterialSugeratId` e
        // tipul cu Σ `Valoare` maximă, nu primul rând. Amândouă sunt Servicii, deci
        // amândouă au regulă de contare pe FCT: o linie care n-ar posta nimic e
        // pierdere tăcută de valoare, iar declarantul o refuză (B-r10).
        var idFf2 = FacturaOperata("-FF2", extern1, new DateOnly(2026, 2, 6), imp,
            (tip626, 100m), (tip628, 600m));
        // Tot pe `IMP` (cotă 0), inclusiv furnizorii RO/UE: scena n-are nevoie de
        // TVA ca să probeze filtrul de CLASĂ FISCALĂ, iar o mișcare pe 4426 în
        // februarie ar polua soldurile CUMULATE ale blocurilor de închidere dacă o
        // rulare s-ar rupe înainte de curățenie.
        var idFf3 = FacturaOperata("-FF3", partenerRo, new DateOnly(2026, 2, 7), imp, (tip628, 300m));
        var idFf4 = FacturaOperata("-FF4", partenerUe, new DateOnly(2026, 2, 8), imp, (tip628, 400m));
        // O factură DRAFT: candidații sunt doar cele OPERATE.
        var fctDraft = os.CreateObject<FacturaIntrare>();
        fctDraft.Numar = Marcaj + "-FF5";
        fctDraft.Data = new DateOnly(2026, 2, 9);
        fctDraft.Predator = extern1;
        fctDraft.Primitor = gestiune;
        var linieDraft = os.CreateObject<FacturaIntrareDetaliu>();
        linieDraft.Document = fctDraft;
        linieDraft.TipMaterial = tip628;
        linieDraft.Cantitate = 1m;
        linieDraft.PretUnitar = 50m;
        linieDraft.TipTva = imp;
        os.CommitChanges();
        var idFf5 = fctDraft.ID;

        // ---------------- (1) Crearea agregatului, cu legături ----------------
        var scriere = new DviWriteDto {
            Numar = "26ROBV" + Marcaj,
            Data = new DateOnly(2026, 2, 18),
            PredatorId = vama.ID,
            PrimitorId = unitate.ID,
            Linii = {
                new DviLinieWriteDto { TipMaterialId = tip628.ID, TipTvaId = imp21.ID, Valoare = 1000m, ValoareTva = 210m },
                // Taxa lăsată la 0 se calculează din cotă la culegere, ca la operare (48b, 104c).
                new DviLinieWriteDto { TipMaterialId = tip628.ID, TipTvaId = impTi21.ID, Valoare = 500m }
            },
            FacturiIds = { idFf1 }
        };
        var idDvi = DviApply.Aplica(os, null, scriere);
        var cit = DviApply.Citeste(os, idDvi);
        bool persistat;
        using (var osAlt = s.Provider.CreateObjectSpace())
            persistat = osAlt.GetObjectsQuery<Dvi>().Any(d => d.ID == idDvi);
        Console.WriteLine($"     MĂSURAT (Api DVI/creare): Baza {cit?.Baza}, Tva {cit?.Tva}, "
            + $"{cit?.Linii.Count} linii, {cit?.Facturi.Count} factură/facturi legate "
            + $"({string.Join("; ", cit?.Facturi.Select(f => $"{f.Numar}/{f.Stare}/{f.Valoare}") ?? [])}).");
        s.Check("Api DVI: `Aplica` ⇒ document COMIS (vizibil dintr-un alt ObjectSpace), cu antetul cules "
            + "(MRN-ul e al declarației, nu o serie server-owned), liniile pe BAZA detaliului și legătura "
            + "scrisă prin `FacturiIds`. `Baza`/`Tva` vin de pe SERVER (42c): 1500 valoare în vamă și 315 "
            + "taxă — taxa lăsată la 0 pe a doua linie se calculează din cotă încă de la culegere (104c)",
            persistat && cit != null && cit.Id == idDvi && cit.Stare == "Draft"
            && cit.Numar == scriere.Numar && cit.Data == scriere.Data
            && cit.PredatorId == vama.ID && cit.PredatorDenumire == vama.Denumire
            && cit.PrimitorId == unitate.ID && cit.PrimitorDenumire == unitate.Denumire
            && cit.Linii.Count == 2 && cit.Baza == 1500m && cit.Tva == 315m
            && cit.Linii.Any(l => l.TipTvaCod == "IMP21" && l.TipTvaCota == 21m && l.Valoare == 1000m && l.ValoareTva == 210m)
            && cit.Linii.Any(l => l.TipTvaCod == "IMPTI21" && l.Valoare == 500m && l.ValoareTva == 105m)
            && cit.PoateEdita && cit.PoateOpera && !cit.PoateAnula && !cit.PoateStorna);
        s.Check("Api DVI: factura legată se citește cu identitatea ei — numărul, data, furnizorul EXTERN (nu "
            + "biroul vamal, care e latura declarației), STAREA (DVI-r2: o factură anulată după legare "
            + "rămâne legată, iar ecranul trebuie s-o arate) și totalul brut al facturii",
            cit.Facturi.Count == 1
            && cit.Facturi[0].FacturaId == idFf1
            && cit.Facturi[0].Numar == Marcaj + "-FF1"
            && cit.Facturi[0].PartenerDenumire == extern1.Denumire
            && cit.Facturi[0].Stare == "Operat"
            && cit.Facturi[0].Valoare == 1000m);
        var randLista = DviApply.Lista(os).Single(d => d.Id == idDvi);
        s.Check("Api DVI: `Lista` dă aceleași cifre ca agregatul (join pe agregat, nu subquery corelat), plus "
            + "numărul de facturi legate; starea e tradusă ÎN SQL",
            randLista.Stare == "Draft" && randLista.Numar == scriere.Numar
            && randLista.Baza == 1500m && randLista.Tva == 315m && randLista.NrFacturi == 1
            && randLista.PredatorDenumire == vama.Denumire);

        // ---------------- (2) Candidații ----------------
        var plicImplicit = DviApply.FacturiCandidate(os, febStart, febEnd, null, toate: false, dviId: idDvi);
        var candidatiImplicit = plicImplicit.Candidati;
        var candidatiToti = DviApply.FacturiCandidate(os, febStart, febEnd, null, toate: true, dviId: idDvi).Candidati;
        var candidatiFaraDvi = DviApply.FacturiCandidate(os, febStart, febEnd, null, toate: false, dviId: null).Candidati;
        var candidatiPartener = DviApply.FacturiCandidate(os, febStart, febEnd, partenerRo.ID, toate: true, dviId: idDvi).Candidati;
        var ff2 = candidatiImplicit.SingleOrDefault(c => c.FacturaId == idFf2);
        Console.WriteLine($"     MĂSURAT (Api DVI/candidați): implicit {candidatiImplicit.Count} "
            + $"({string.Join(", ", candidatiImplicit.Select(c => c.Numar + "/" + c.ClasaFiscala))}); "
            + $"`toate` {candidatiToti.Count} ({string.Join(", ", candidatiToti.Select(c => c.Numar + "/" + c.ClasaFiscala))}); "
            + $"fără dviId {candidatiFaraDvi.Count}; pe partenerul RO {candidatiPartener.Count}.");
        s.Check("Api DVI: candidații impliciți sunt facturile OPERATE ale perioadei cu furnizor EXTRA-UE — "
            + "factura deja legată la declarația din rută e exclusă, cea RO și cea UE cad pe clasa fiscală "
            + "(funcția LEGII, `ClasaFiscala.APartenerului`, nu o coloană), iar draftul nu e candidat",
            candidatiImplicit.Count == 1 && ff2 != null && ff2.FacturaId == idFf2
            && ff2.ClasaFiscala == nameof(ClasaFiscalaPartener.ExtraUe)
            && !plicImplicit.MaiSunt
            && !candidatiImplicit.Any(c => c.FacturaId == idFf1 || c.FacturaId == idFf3
                || c.FacturaId == idFf4 || c.FacturaId == idFf5));
        s.Check("Api DVI: candidatul poartă furnizorul, totalul brut al facturii și TIPUL DOMINANT al liniilor "
            + "ei (Σ `Valoare` maximă — 628 cu 600 bate 626 cu 100), ca linia nouă a declarației să nu ceară "
            + "un lookup pe care serverul îl poate răspunde",
            ff2.Numar == Marcaj + "-FF2" && ff2.PartenerId == extern1.ID
            && ff2.PartenerDenumire == extern1.Denumire
            && ff2.Valoare == 700m && ff2.TipMaterialSugeratId == tip628.ID);
        s.Check("Api DVI: `toate` ridică DOAR filtrul de clasă fiscală — intră și factura RO, și cea UE, cu "
            + "clasele lor; exclusul legăturii și cel al stării rămân. Fără `dviId` factura legată reintră "
            + "(ecranul unei declarații NOI n-are ce exclude), iar `partenerId` restrânge la furnizorul cerut",
            candidatiToti.Count == 3
            && candidatiToti.Any(c => c.FacturaId == idFf3 && c.ClasaFiscala == nameof(ClasaFiscalaPartener.InregistratRo))
            && candidatiToti.Any(c => c.FacturaId == idFf4 && c.ClasaFiscala == nameof(ClasaFiscalaPartener.Ue))
            && !candidatiToti.Any(c => c.FacturaId == idFf1 || c.FacturaId == idFf5)
            && candidatiFaraDvi.Count == 2
            && candidatiFaraDvi.Any(c => c.FacturaId == idFf1)
            && candidatiPartener.Count == 1 && candidatiPartener[0].FacturaId == idFf3);
        s.Check("Api DVI: perioada e un FILTRU real — o lună fără facturi dă zero candidați, nu toată baza",
            DviApply.FacturiCandidate(os, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31),
                null, toate: true, dviId: idDvi).Candidati.Count == 0);

        // Trunchierea, pe scena mică: plafonul e parametru INTERN (nu vine de pe
        // sârmă), tocmai ca proba să-l poată coborî sub numărul rândurilor.
        var plicPlafonat = DviApply.FacturiCandidate(os, febStart, febEnd, null,
            toate: true, dviId: idDvi, plafon: 2);
        var plicLaFix = DviApply.FacturiCandidate(os, febStart, febEnd, null,
            toate: true, dviId: idDvi, plafon: 3);
        Console.WriteLine($"     MĂSURAT (Api DVI/plafon): plafon 2 → {plicPlafonat.Candidati.Count} candidați, "
            + $"MaiSunt {plicPlafonat.MaiSunt}; plafon 3 (exact câte are scena) → {plicLaFix.Candidati.Count} "
            + $"candidați, MaiSunt {plicLaFix.MaiSunt}.");
        s.Check("Api DVI: trunchierea NU e tăcută — sub numărul facturilor perioadei plicul întoarce "
            + "`MaiSunt = true` și exact `plafon` candidați, deci ecranul poate cere o perioadă mai îngustă în "
            + "loc să creadă că a văzut tot. Plafonul cade pe INTEROGARE, înaintea filtrului de clasă fiscală "
            + "(`ClasaFiscala.APartenerului` nu se traduce în SQL), iar la fix pe numărul rândurilor `MaiSunt` "
            + "rămâne false — semnalul nu e un fals pozitiv de margine",
            plicPlafonat.MaiSunt && plicPlafonat.Candidati.Count == 2
            && !plicLaFix.MaiSunt && plicLaFix.Candidati.Count == 3);
        s.CheckRefuza("Api DVI: perioada inversată e refuz de DOMENIU, nu o listă goală care ar părea un răspuns",
            () => DviApply.FacturiCandidate(os, febEnd, febStart, null, toate: true, dviId: null));

        // ---------------- (3) Actualizarea: legăturile ca DIFERENȚĂ ----------------
        DviWriteDto Rescrie(DviReadDto sursa, List<Guid> facturi, Guid? tipTvaPrimaLinie = null) => new() {
            Numar = sursa.Numar, Data = sursa.Data,
            PredatorId = sursa.PredatorId, PrimitorId = sursa.PrimitorId,
            Linii = sursa.Linii.Select((l, i) => new DviLinieWriteDto {
                Id = l.Id, TipMaterialId = l.TipMaterialId,
                TipTvaId = i == 0 && tipTvaPrimaLinie != null ? tipTvaPrimaLinie : l.TipTvaId,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva
            }).ToList(),
            FacturiIds = facturi
        };

        DviApply.Aplica(os, idDvi, Rescrie(cit, new List<Guid> { idFf2 }));
        var citSchimbat = DviApply.Citeste(os, idDvi);
        Console.WriteLine($"     MĂSURAT (Api DVI/actualizare): facturi legate acum — "
            + $"{string.Join(", ", citSchimbat.Facturi.Select(f => f.Numar))}; "
            + $"{os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi)} rând(uri) de legătură vii.");
        s.Check("Api DVI: `FacturiIds` e agregatul ÎNTREG, nu un delta de client — factura scoasă din payload "
            + "se DEZLEAGĂ, cea adăugată se leagă, iar liniile rămân neatinse (round-trip fără pierderi)",
            citSchimbat.Facturi.Count == 1 && citSchimbat.Facturi[0].FacturaId == idFf2
            && citSchimbat.Facturi[0].Valoare == 700m
            && os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi) == 1
            && citSchimbat.Linii.Count == 2 && citSchimbat.Baza == 1500m && citSchimbat.Tva == 315m);
        s.CheckRefuza("Api DVI: aceeași factură de două ori în `FacturiIds` ⇒ refuz de DOMENIU (payload-ul e "
            + "reconciliat server-side, deci un id repetat ar fi trecut tăcut ca unul singur)",
            () => DviApply.Aplica(os, idDvi, Rescrie(citSchimbat, new List<Guid> { idFf2, idFf2 })));
        s.CheckRefuza("Api DVI: `FacturiIds` cu un id inexistent ⇒ refuz de domeniu cu fraza unică a "
            + "referinței invizibile (F22-D6), nu violare de FK",
            () => DviApply.Aplica(os, idDvi, Rescrie(citSchimbat, new List<Guid> { Guid.NewGuid() })));
        s.CheckRefuza("Api DVI: Id de linie STRĂIN ⇒ refuz (agregatul nu adoptă linii din alt document)", () => {
            var payload = Rescrie(citSchimbat, new List<Guid> { idFf2 });
            payload.Linii[0].Id = Guid.NewGuid();
            DviApply.Aplica(os, idDvi, payload);
        });

        // Regresie MĂSURATĂ la scrierea blocului: `Rezolva.Cere` chemat DUPĂ
        // `CreateObject` lăsa o legătură fără factură în ObjectSpace-ul viu, pe care
        // primul commit de după o persista — FK 23503 brut, la o cerere fără
        // legătură cu cea refuzată. Rezolvările s-au mutat înaintea creării (F3-D5).
        DviApply.Aplica(os, idDvi, Rescrie(citSchimbat, new List<Guid> { idFf2 }));
        s.Check("Api DVI: după cele trei refuzuri, un `Aplica` VALID trece, iar agregatul e neschimbat — niciun "
            + "obiect pe jumătate construit (legătură fără factură, linie fără tip) n-a rămas în ObjectSpace "
            + "de la refuz, pe care commit-ul următor l-ar fi persistat",
            os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi) == 1
            && !os.GetObjectsQuery<DviFactura>().Any(f => f.FacturaId == Guid.Empty)
            && DviApply.Citeste(os, idDvi) is { Linii.Count: 2, Baza: 1500m, Tva: 315m });

        // ---------------- (4) Refuzul de OPERARE prin ușă ----------------
        DviApply.Aplica(os, idDvi, Rescrie(citSchimbat, new List<Guid> { idFf2 }, tipTvaPrimaLinie: n21.ID));
        using (var osDry = s.Provider.CreateObjectSpace())
            s.CheckRefuza("Api DVI: o linie culeasă cu un tip de TVA care nu e `DeImport` (N21) trece de "
                + "culegere — draftul are voie să fie greșit — și se oprește la OPERARE, cu refuzul TIPULUI. "
                + "Felia nu-l duplică: o a doua sursă a aceleiași reguli ar diverge tăcut (42a)",
                () => ComenziDocument.Sistem(osDry).Opereaza(idDvi));
        DviApply.Aplica(os, idDvi, Rescrie(citSchimbat, new List<Guid> { idFf2 }, tipTvaPrimaLinie: imp21.ID));

        // ---------------- (5) Operarea, refuzul PUT-ului, stornoul ----------------
        var rezOperare = ComenziDocument.Sistem(os).Opereaza(idDvi);
        var citOperat = DviApply.Citeste(os, idDvi);
        Console.WriteLine($"     MĂSURAT (Api DVI/operare): stare {rezOperare.StareNoua}, conex "
            + $"{rezOperare.ConexId?.ToString() ?? "<niciunul>"}; Baza {citOperat.Baza}, Tva {citOperat.Tva}.");
        s.Check("Api DVI: `opereaza` prin ușă ⇒ `Operat`, FĂRĂ document conex (declarația nu generează nimic), "
            + "iar `Tva` crește la 315 — taxa liniei cu amânarea plății s-a născut din cotă la operare "
            + "(`PregatesteOperare`), deci cifra citită după comandă e cea a motorului, nu cea culeasă. "
            + "Afordanțele se întorc pe cele de document operat",
            rezOperare.StareNoua == StareDocument.Operat && rezOperare.ConexId == null
            && citOperat.Stare == "Operat" && citOperat.DataOperare != null
            && citOperat.Baza == 1500m && citOperat.Tva == 315m
            && !citOperat.PoateEdita && !citOperat.PoateOpera
            && citOperat.PoateAnula && citOperat.PoateStorna);
        s.CheckRefuza("Api DVI: PUT pe o declarație OPERATĂ ⇒ refuz de DOMENIU înaintea oricărei modificări "
            + "(pre-check-ul de Draft din `Aplica`; pe ușa securizată `GardianEditare` rămâne plasa) — "
            + "inclusiv o cerere care ar vrea doar să schimbe legăturile",
            () => DviApply.Aplica(os, idDvi, Rescrie(citOperat, new List<Guid> { idFf1 })));
        s.CheckRefuza("Api DVI: DELETE pe o declarație OPERATĂ ⇒ același refuz de domeniu",
            () => DviApply.Sterge(os, idDvi));
        s.Check("Api DVI: refuzurile n-au lăsat urme — legătura rămâne cea de dinainte",
            os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi && f.FacturaId == idFf2) == 1
            && os.GetObjectsQuery<DviFactura>().Count(f => f.DviId == idDvi) == 1);

        var rezStorno = ComenziDocument.Sistem(os).Storneaza(idDvi, new DateOnly(2026, 2, 26));
        var citStornat = DviApply.Citeste(os, idDvi);
        s.Check("Api DVI: `storneaza` prin ușă ⇒ `Stornat` pe sârmă (string, nu ordinalul enum-ului), fără "
            + "nicio afordanță de scriere, iar legăturile rămân înghețate cu documentul",
            rezStorno.StareNoua == StareDocument.Stornat && citStornat.Stare == "Stornat"
            && !citStornat.PoateEdita && !citStornat.PoateOpera
            && !citStornat.PoateAnula && !citStornat.PoateStorna
            && citStornat.Facturi.Count == 1 && citStornat.Facturi[0].FacturaId == idFf2
            && DviApply.Lista(os).Single(d => d.Id == idDvi).Stare == "Stornat");

        // ---------------- (6) Ștergerea unui draft, cu legături ----------------
        var idDraft = DviApply.Aplica(os, null, new DviWriteDto {
            Numar = "26ROBV" + Marcaj + "-B",
            Data = new DateOnly(2026, 2, 19),
            PredatorId = vama.ID,
            PrimitorId = unitate.ID,
            Linii = { new DviLinieWriteDto { TipMaterialId = tip628.ID, TipTvaId = imp21.ID, Valoare = 200m, ValoareTva = 42m } },
            FacturiIds = { idFf1 }
        });
        DviApply.Sterge(os, idDraft);
        s.Check("Api DVI: ștergerea unui draft duce cu ea liniile ȘI legăturile — nicio legătură orfană, iar "
            + "facturile rămân intacte (legătura e evidență, nu parte din ele)",
            DviApply.Citeste(os, idDraft) == null
            && !os.GetObjectsQuery<DviFactura>().Any(f => f.DviId == idDraft)
            && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idDraft)
            && os.GetObjectByKey<FacturaIntrare>(idFf1).Stare == StareDocument.Operat);

        CurataApiDvi();
        s.Check("Api DVI: scena nu lasă urme — partenerii, documentele, legăturile și registrele lor sunt "
            + "purjate FIZIC",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Dvi>().Any()
            && !os.GetObjectsQuery<DviFactura>().Any()
            && CubScena.FapteIntre(os, febStart, febEnd).Count == 0);
    }
}
