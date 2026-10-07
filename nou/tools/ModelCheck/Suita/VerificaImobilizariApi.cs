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

// Imobilizările parcurse prin CONTRACTUL feliei: `PifApply.LiniiSursa` →
// `PifApply.Aplica` → `ComenziDocument` → `AmoApply.Previzualizeaza`/`Genereaza`/
// `Citeste` (`Stale`) → revizuire → `Regenereaza` → `CasApply.Aplica` →
// `ImobilizariApply.Fisa`/`Registru`. Endpoint-urile din host sunt transport
// peste EXACT acest cod.
//
// Semantica de MOTOR (aritmetica, registrele, gardienii de operare) e acoperită
// de blocul `E2E-IMO`; aici se probează UȘA. Blocul stă IMEDIAT după el, care
// tocmai a purjat fereastra 05–12/2027 — precondiția de mai jos o re-măsoară, ca
// mutarea blocului să iasă ca FAIL, nu ca cifre peste conținut străin.
//
// CE NU SE POATE PROBA AICI: gate-urile de acces. ObjectSpace-urile lui
// ModelCheck sunt NESECURIZATE, deci nici `GardianEditare` (armat pe familia
// securizată) și nici verdictele 403/404 ale ușii nu se aplică — de aceea
// refuzul de ștergere al fișei se cere EXPLICIT gardianului, iar 404/403 au
// proba lor în `nou/tools/ProbeHttp/refuzuri.ps1`, pe host viu.
static class VerificaImobilizariApi {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-API-IMO";
        const int An = 2027;
        var eticheta = privat ? "privat" : "bugetar";
        var codTipF = privat ? "214" : "214.00.00";
        DateOnly Zi(int luna, int zi) => new(An, luna, zi);
        // Marginile ferestrei ca VARIABILE: un apel de funcție locală n-are ce căuta
        // într-un arbore de expresie (CS8110).
        var primaZi = new DateOnly(An, 5, 1);
        var ultimaZi = new DateOnly(An, 12, 31);
        var septStart = new DateOnly(An, 9, 1);
        var septEnd = new DateOnly(An, 9, 30);
        using var os = s.Provider.CreateObjectSpace();

        // Purjă FIZICĂ (F13-D2) ÎNCRUCIȘATĂ, oglinda lui `CurataImo`: fereastra
        // 05–12/2027 e a amândurora, iar rămășițele oricăruia ar fi măsurate de celălalt.
        void CurataApiImo() {
            var pj = new Purja(os);
            var start = Zi(5, 1);
            var sfarsit = Zi(12, 31);
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= start && d.Data <= sfarsit).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruImobilizari>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                .Where(r => docIds.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Imobilizare>()
                .Where(f => f.NumarInventar.StartsWith(Marcaj) || f.NumarInventar.StartsWith("E2E-IMO")).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj) || r.Cod.StartsWith("E2E-IMO")).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<SursaFinantare>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodFunctional>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Proiect>()
                .Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            var tipuriScena = os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)).Select(t => t.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<PoliticaAmortizare>()
                .Where(p => tipuriScena.Contains(p.TipMaterialId)).ToList());
            pj.Adauga(os.GetObjectsQuery<TipMaterial>()
                .Where(t => t.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(p => p.An == An && p.Luna >= 5).ToList());
            pj.Executa();
        }
        CurataApiImo();

        var tipF = os.FirstOrDefault<TipMaterial>(t => t.Cod == codTipF);
        var politicaF = tipF == null ? null
            : os.FirstOrDefault<PoliticaAmortizare>(p => p.TipMaterialId == tipF.ID);
        s.Check($"Api IMO ({eticheta}) — precondiție: tipul de clasă F al profilului are rând de politică de "
            + "amortizare, iar fereastra 05–12/2027 e liberă (blocul `E2E-IMO` de dinainte tocmai a purjat-o). "
            + "Dacă proba asta pică, blocul a fost mutat, iar cifrele de mai jos ar fi măsurate peste conținut străin",
            tipF != null && politicaF != null
            && !os.GetObjectsQuery<Imobilizare>().Any(f => f.NumarInventar.StartsWith("E2E"))
            && !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An && p.Luna >= 5)
            && !os.GetObjectsQuery<Document>().Any(d => d.Data >= primaZi && d.Data <= ultimaZi));

        // ── Scena ─────────────────────────────────────────────────────────────────
        foreach (var luna in new[] { 5, 6, 7, 8, 9, 10, 11, 12 }) {
            var p = os.CreateObject<PerioadaFiscala>();
            p.An = An;
            p.Luna = luna;
            p.Inchisa = false;
        }
        var furnizor = os.CreateObject<Partener>();
        furnizor.Cod = Marcaj + "-FURN";
        furnizor.Denumire = "Furnizor de imobilizări, ușa API";
        furnizor.Tara = "RO";
        var gestiune = os.CreateObject<Gestiune>();
        gestiune.Cod = Marcaj + "-MAG";
        gestiune.Denumire = "Gestiune probă, ușa API";
        var unitate = os.CreateObject<UnitateInterna>();
        unitate.Cod = Marcaj + "-UI";
        unitate.Denumire = "Unitate probă, ușa API";
        var codEc = os.CreateObject<CodEconomic>();
        codEc.Cod = Marcaj + "-CE";
        codEc.Denumire = "Cod economic probă, ușa API";
        var sursaFin = os.CreateObject<SursaFinantare>();
        sursaFin.Cod = Marcaj + "-SF";
        sursaFin.Denumire = "Sursă de finanțare probă, ușa API";
        var codFn = os.CreateObject<CodFunctional>();
        codFn.Cod = Marcaj + "-CF";
        codFn.Denumire = "Cod funcțional probă, ușa API";
        var proiect = os.CreateObject<Proiect>();
        proiect.Cod = Marcaj + "-PR";
        proiect.Denumire = "Proiect probă, ușa API";
        os.CommitChanges();

        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = Marcaj + "-FCT";
        fct.Data = Zi(5, 4);
        fct.Predator = furnizor;
        fct.Primitor = gestiune;
        var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
        linieFct.Document = fct;
        linieFct.TipMaterial = tipF;
        linieFct.Cantitate = 1m;
        linieFct.PretUnitar = 3600m;
        linieFct.CodEconomicId = codEc.ID;
        linieFct.SursaFinantareId = sursaFin.ID;
        linieFct.CodFunctionalId = codFn.ID;
        linieFct.ProiectId = proiect.ID;
        os.CommitChanges();
        var idLinieSursa = linieFct.ID;
        ComenziDocument.Sistem(os).Opereaza(fct.ID);

        // Fișa se creează DIRECT pe ObjectSpace: nomenclatorul e pe OData, care nu se
        // exersează in-process, iar gardianul ei are deja probă în `E2E-IMO`.
        var fisa = os.CreateObject<Imobilizare>();
        fisa.NumarInventar = Marcaj + "-1";
        fisa.Denumire = "Fișă probă a ușii de imobilizări";
        fisa.TipMaterialId = tipF.ID;
        fisa.LocId = gestiune.ID;
        // Pe planul bugetar contul de cheltuială cu amortizarea cere Cod economic
        // (F26-r16, proba `IMO-V31c`); pe privat nimeni nu-l cere.
        if (!privat)
            fisa.CodEconomicId = codEc.ID;
        os.CommitChanges();
        var idFisa = fisa.ID;

        // ── API-IMO-V1: prefill-ul liniei sursă ───────────────────────────────────
        var inainte = PifApply.LiniiSursa(os, Zi(5, 1), Zi(5, 31), null, false);
        var candidat = inainte.Candidati.FirstOrDefault(c => c.LinieId == idLinieSursa);
        Console.WriteLine($"     MĂSURAT (API-IMO-V1/{eticheta}): {inainte.Candidati.Count} candidați, "
            + $"linia scenei = {candidat?.Valoare} valoare / {candidat?.Consumat} consumat / {candidat?.Rest} rest; "
            + $"mai sunt = {inainte.MaiSunt}.");
        s.Check($"API-IMO-V1 ({eticheta}) `linii-sursa` propune linia de factură OPERATĂ de clasă de imobilizări cu "
            + "restul ei neconsumat: prefill-ul punerii în funcțiune e 3.600, iar plafonul de pagină n-a fost atins",
            candidat != null && candidat.Valoare == 3600m && candidat.Consumat == 0m && candidat.Rest == 3600m
            && candidat.Numar == Marcaj + "-FCT" && candidat.PartenerId == furnizor.ID
            && candidat.TipMaterialCod == codTipF && !inainte.MaiSunt);

        // ── API-IMO-V2: agregatul cules, cu tipul SERVER-OWNED ────────────────────
        var idPif = PifApply.Aplica(os, null, new PifWriteDto {
            Data = Zi(5, 5),
            PredatorId = unitate.ID,
            PrimitorId = gestiune.ID,
            Linii = {
                new PifLinieWriteDto {
                    ImobilizareId = idFisa, Fel = "Intrare", LinieSursaId = idLinieSursa, Valoare = 3600m,
                    Metoda = "Liniara", DurataLuni = 36,
                    MetodaFiscala = "Liniara", DurataFiscalaLuni = 36,
                    CategorieFiscala = "Standard", UtilizareExclusiva = true
                }
            }
        });
        var citPif = PifApply.Citeste(os, idPif);
        Console.WriteLine($"     MĂSURAT (API-IMO-V2/{eticheta}): {citPif.Linii.Count} linii, total {citPif.Total}, "
            + $"fel {citPif.Linii[0].Fel}, tip {citPif.Linii[0].TipMaterialCod}, "
            + $"sursă {citPif.Linii[0].LinieSursaNumar}.");
        s.Check($"API-IMO-V2 ({eticheta}) PIF-ul cules prin `Aplica`: tipul liniei e al FIȘEI (payload-ul n-are "
            + "câmpul — contul de imobilizare a intrat deja în politică și în note), cantitatea e 1, iar parametrii "
            + "de amortizare ajung pe linie ca fapte datate",
            citPif is { Linii.Count: 1, Total: 3600m, PoateEdita: true, PoateOpera: true }
            && citPif.Linii[0].TipMaterialId == tipF.ID && citPif.Linii[0].Fel == "Intrare"
            && citPif.Linii[0].NumarInventar == Marcaj + "-1"
            && citPif.Linii[0].LinieSursaId == idLinieSursa
            && citPif.Linii[0].DurataLuni == 36 && citPif.Linii[0].Metoda == "Liniara"
            && citPif.Linii[0].CategorieFiscala == "Standard");

        // ── API-IMO-V3: plafonul liniei sursă, văzut de panou ─────────────────────
        var dupaPif = PifApply.LiniiSursa(os, Zi(5, 1), Zi(5, 31), null, false);
        var cuToate = PifApply.LiniiSursa(os, Zi(5, 1), Zi(5, 31), null, true);
        var epuizat = cuToate.Candidati.FirstOrDefault(c => c.LinieId == idLinieSursa);
        Console.WriteLine($"     MĂSURAT (API-IMO-V3/{eticheta}): fără `toate` = "
            + $"{dupaPif.Candidati.Count(c => c.LinieId == idLinieSursa)} apariții; cu `toate` = "
            + $"{epuizat?.Consumat} consumat / {epuizat?.Rest} rest.");
        s.Check($"API-IMO-V3 ({eticheta}) după punerea în funcțiune linia sursă e EPUIZATĂ: dispare din panou, iar "
            + "cu `toate=true` se vede cu restul 0 — aceeași aritmetică a consumului ca gardianul de operare "
            + "(`ConsumatPeLinieSursa`), nu o a doua formulă",
            !dupaPif.Candidati.Any(c => c.LinieId == idLinieSursa)
            && epuizat != null && epuizat.Consumat == 3600m && epuizat.Rest == 0m);

        ComenziDocument.Sistem(os).Opereaza(idPif);

        // ── API-IMO-V4: luna punerii în funcțiune nu se amortizează ───────────────
        var prevMai = AmoApply.Previzualizeaza(os, An, 5);
        Console.WriteLine($"     MĂSURAT (API-IMO-V4/{eticheta}): motiv {prevMai.Motiv} "
            + $"(„{prevMai.MotivEticheta}”), {prevMai.Linii.Count} linii.");
        s.Check($"API-IMO-V4 ({eticheta}) previzualizarea lunii PIF iese `FaraFise`, cu eticheta din model lângă "
            + "numele membrului — ecranul n-o scrie din cod (57a)",
            prevMai.Motiv == nameof(MotivNegenerare.FaraFise)
            && !string.IsNullOrWhiteSpace(prevMai.MotivEticheta) && prevMai.MotivEticheta != prevMai.Motiv
            && prevMai.Linii.Count == 0 && prevMai.TotalContabil == 0m && prevMai.BlocantId == null);

        // ── API-IMO-V5: previzualizarea și generarea lunii următoare ──────────────
        var cotaInitiala = Math.Round(3600m / 36m, 2);
        var prevIunie = AmoApply.Previzualizeaza(os, An, 6);
        var linieIunie = prevIunie.Linii.FirstOrDefault();
        Console.WriteLine($"     MĂSURAT (API-IMO-V5/{eticheta}): {prevIunie.Linii.Count} linii, "
            + $"contabil {linieIunie?.Contabil}, conturi {linieIunie?.ContCheltuialaSimbol} = "
            + $"{linieIunie?.ContAmortizareSimbol}, loc „{linieIunie?.LocDenumire}”, "
            + $"cod economic {(linieIunie?.CodEconomicId == null ? "<niciunul>" : "pus")}.");
        s.Check($"API-IMO-V5 ({eticheta}) previzualizarea lunii dă cele TREI cifre, conturile REZOLVATE din politică "
            + "(cu simbolurile lor, ca ecranul să nu le afirme din cod) și locul fișei; totalurile sunt pe server (42c)",
            prevIunie.Motiv == null && prevIunie.Linii.Count == 1
            && linieIunie.Contabil == cotaInitiala && linieIunie.Fiscal == cotaInitiala
            && linieIunie.Deductibil == cotaInitiala
            && linieIunie.ContCheltuialaId == politicaF.ContCheltuialaAmortizareId
            && linieIunie.ContAmortizareId == politicaF.ContAmortizareId
            && !string.IsNullOrWhiteSpace(linieIunie.ContCheltuialaSimbol)
            && !string.IsNullOrWhiteSpace(linieIunie.ContAmortizareSimbol)
            && linieIunie.LocId == gestiune.ID && linieIunie.LocDenumire == gestiune.Denumire
            && linieIunie.CodEconomicId == (privat ? (Guid?)null : codEc.ID)
            && prevIunie.TotalContabil == cotaInitiala && prevIunie.TotalFiscal == cotaInitiala
            && prevIunie.TotalDeductibil == cotaInitiala);

        var generatIunie = AmoApply.Genereaza(os,
            new GenerareAmoRequestDto { An = An, Luna = 6, UnitateId = unitate.ID });
        if (generatIunie.DocumentId == null)
            throw new OperareException($"Scena API-IMO: iunie n-a fost generată ({generatIunie.Motiv}).");
        var idAmoIunie = generatIunie.DocumentId.Value;
        var citIunie = AmoApply.Citeste(os, idAmoIunie);
        s.Check($"API-IMO-V6 ({eticheta}) `genereaza` scrie draftul lunii, iar `Citeste` îl dă cu `Stale: false` — "
            + "verdictul anti-stale e cel al GARDIANULUI (`LiniileCorespund`), nu o formulă geamănă",
            citIunie is { Stale: false, Linii.Count: 1, PoateOpera: true, PoateRegenera: true, An: An, Luna: 6 }
            && citIunie.TotalContabil == cotaInitiala
            && citIunie.UnitateId == unitate.ID
            && citIunie.Linii[0].ImobilizareId == idFisa);

        // ── API-IMO-V7: un eveniment operat DUPĂ generare face draftul STALE ─────
        // Revizuirea e datată în MAI, nu în iunie: parametrii noi curg din luna de
        // DUPĂ eveniment (F26-D7, formula observată în Flax), deci o revizuire din
        // iunie ar fi schimbat iulie, nu luna al cărei draft îl probăm.
        var idRevizuire = PifApply.Aplica(os, null, new PifWriteDto {
            Data = Zi(5, 20),
            PredatorId = unitate.ID,
            PrimitorId = gestiune.ID,
            Linii = {
                new PifLinieWriteDto {
                    ImobilizareId = idFisa, Fel = "Revizuire", Valoare = 0m,
                    Metoda = "Liniara", DurataLuni = 48,
                    MetodaFiscala = "Liniara", DurataFiscalaLuni = 48,
                    CategorieFiscala = "Standard", UtilizareExclusiva = true
                }
            }
        });
        ComenziDocument.Sistem(os).Opereaza(idRevizuire);
        var dupaRevizuire = AmoApply.Citeste(os, idAmoIunie);
        Console.WriteLine($"     MĂSURAT (API-IMO-V7/{eticheta}): după revizuirea duratei la 48, draftul lunii "
            + $"are Stale = {dupaRevizuire.Stale}.");
        s.Check($"API-IMO-V7 ({eticheta}) un eveniment operat DUPĂ generare face draftul STALE, iar ecranul o află "
            + "ÎNAINTE de a apăsa butonul — cifra lunii se recalculează din registru la fiecare citire, nu se "
            + "citește de pe document",
            dupaRevizuire.Stale == true);
        s.CheckRefuza($"API-IMO-V8 ({eticheta}) operarea unui draft stale e refuzată de gardian — `Stale` și refuzul "
            + "citesc ACELAȘI criteriu",
            () => ComenziDocument.Sistem(os).Opereaza(idAmoIunie));

        // ── API-IMO-V9: regenerarea, cu cota nouă ────────────────────────────────
        var cotaRevizuita = Math.Round(3600m / 48m, 2);
        var regenerat = AmoApply.Regenereaza(os, idAmoIunie);
        if (regenerat.DocumentId == null)
            throw new OperareException($"Scena API-IMO: regenerarea lui iunie n-a produs document ({regenerat.Motiv}).");
        var idAmoIunie2 = regenerat.DocumentId.Value;
        var citRegen = AmoApply.Citeste(os, idAmoIunie2);
        Console.WriteLine($"     MĂSURAT (API-IMO-V9/{eticheta}): draft nou {idAmoIunie2 != idAmoIunie}, "
            + $"contabil {citRegen.TotalContabil}, Stale = {citRegen.Stale}; draftul vechi mai există: "
            + $"{AmoApply.Citeste(os, idAmoIunie) != null}.");
        s.Check($"API-IMO-V9 ({eticheta}) `regenereaza` calculează ÎNTÂI și șterge după (un refuz ar fi lăsat "
            + "draftul vechi intact): draftul nou poartă cota revizuită și nu mai e stale, iar cel vechi a dispărut",
            idAmoIunie2 != idAmoIunie && citRegen.Stale == false
            && citRegen.TotalContabil == cotaRevizuita
            && AmoApply.Citeste(os, idAmoIunie) == null);

        ComenziDocument.Sistem(os).Opereaza(idAmoIunie2);
        s.Check($"API-IMO-V10 ({eticheta}) după operare `Stale` e `null`, nu `false`: cifra e deja în registru, iar "
            + "întrebarea n-ar mai avea sens",
            AmoApply.Citeste(os, idAmoIunie2).Stale == null);

        // ── API-IMO-V11: luna următoare, raportul lunii ocupate, refuzul regenerării ──
        // Cota e FIXATĂ la ultimul eveniment (F26-D7): iulie o repetă pe a lui iunie.
        var cotaIulie = cotaRevizuita;
        var generatIulie = AmoApply.Genereaza(os,
            new GenerareAmoRequestDto { An = An, Luna = 7, UnitateId = unitate.ID });
        if (generatIulie.DocumentId == null)
            throw new OperareException($"Scena API-IMO: iulie n-a fost generată ({generatIulie.Motiv}).");
        var idAmoIulie = generatIulie.DocumentId.Value;
        var citIulie = AmoApply.Citeste(os, idAmoIulie);
        ComenziDocument.Sistem(os).Opereaza(idAmoIulie);
        var iarasiIulie = AmoApply.Genereaza(os,
            new GenerareAmoRequestDto { An = An, Luna = 7, UnitateId = unitate.ID });
        Console.WriteLine($"     MĂSURAT (API-IMO-V11/{eticheta}): iulie = {citIulie.TotalContabil}; a doua "
            + $"generare = {iarasiIulie.Motiv} pe documentul {iarasiIulie.BlocantId == idAmoIulie}.");
        s.Check($"API-IMO-V11 ({eticheta}) luna următoare repetă cota FIXATĂ la ultimul eveniment, iar o a doua "
            + "`genereaza` pe aceeași lună e un RAPORT (`AmortizareVie` + documentul blocant), nu o eroare și nu "
            + "un al doilea draft",
            citIulie.TotalContabil == cotaIulie
            && iarasiIulie.DocumentId == null
            && iarasiIulie.Motiv == nameof(MotivNegenerare.AmortizareVie)
            && iarasiIulie.BlocantId == idAmoIulie
            && !string.IsNullOrWhiteSpace(iarasiIulie.MotivEticheta));
        s.CheckRefuza($"API-IMO-V12 ({eticheta}) `regenereaza` pe o amortizare OPERATĂ e refuzată: ar fi însemnat "
            + "ștergerea unor rânduri de registru",
            () => AmoApply.Regenereaza(os, idAmoIulie));

        // ── API-IMO-V13: ieșirea, cu liniile produse de server ───────────────────
        var cumulat = cotaRevizuita + cotaIulie;
        var ramas = 3600m - cumulat;
        var idCas = CasApply.Aplica(os, null, new CasWriteDto {
            Data = Zi(8, 20), Cauza = "Casare",
            PredatorId = gestiune.ID, PrimitorId = unitate.ID,
            Fise = { idFisa }
        });
        var citCas = CasApply.Citeste(os, idCas);
        Console.WriteLine($"     MĂSURAT (API-IMO-V13/{eticheta}): {citCas.Linii.Count} linii — "
            + string.Join("; ", citCas.Linii.Select(l => $"{l.Fel} {l.Valoare} "
                + $"({l.ContDebitSimbol} = {l.ContCreditSimbol})")) + $"; total {citCas.Total}.");
        s.Check($"API-IMO-V13 ({eticheta}) CAS-ul se culege ca MULȚIME DE FIȘE, iar cele două note le produce "
            + "serverul din registru și din politică: cumulatul pe contul de amortizare, restul pe cheltuiala cu "
            + "cedarea, ambele contra contului implicit al tipului; nimic din ele nu e alegerea operatorului",
            citCas is { Linii.Count: 2, Cauza: "Casare", Fise.Count: 1 }
            && citCas.Total == cumulat + ramas
            && citCas.Linii.Any(l => l.Fel == nameof(FelLinieIesire.AmortizareCumulata) && l.Valoare == cumulat
                && l.ContDebitId == politicaF.ContAmortizareId && l.ContCreditId == tipF.ContImplicitId)
            && citCas.Linii.Any(l => l.Fel == nameof(FelLinieIesire.ValoareRamasa) && l.Valoare == ramas
                && l.ContDebitId == politicaF.ContCheltuialaCedareId && l.ContCreditId == tipF.ContImplicitId));
        ComenziDocument.Sistem(os).Opereaza(idCas);

        // ── API-IMO-V14: fișa la DATĂ ────────────────────────────────────────────
        var fisaIulie = ImobilizariApply.Fisa(os, idFisa, Zi(7, 31));
        Console.WriteLine($"     MĂSURAT (API-IMO-V14/{eticheta}): {fisaIulie.Randuri.Count} rânduri "
            + $"({string.Join(", ", fisaIulie.Randuri.Select(r => $"{r.DocumentTip}/{r.Fel}"))}); situația la "
            + $"31.07 = {fisaIulie.Situatie.Valoare} brut, {fisaIulie.Situatie.Amortizare} cumulat, "
            + $"{fisaIulie.Situatie.NetContabil} net, {fisaIulie.Situatie.Luni} luni, durata "
            + $"{fisaIulie.Situatie.DurataLuni}; starea fișei = {fisaIulie.Stare}.");
        s.Check($"API-IMO-V14 ({eticheta}) fișa e situația la DATĂ plus rândurile ≤ dată, cu documentul fiecărui "
            + "rând identificat prin codul ANCOREI (vocabularul de rutare al clientului): ieșirea din august nu "
            + "intră în cifrele lui 31.07, deși STAREA fișei e deja cea de azi",
            fisaIulie.Randuri.Count == 4
            && fisaIulie.Randuri[0].DocumentTip == "PIF" && fisaIulie.Randuri[0].Fel == nameof(FelMiscareImobilizare.Intrare)
            && fisaIulie.Randuri.Count(r => r.DocumentTip == "AMO") == 2
            && fisaIulie.Randuri.Any(r => r.Fel == nameof(FelMiscareImobilizare.Revizuire) && r.DurataLuni == 48)
            && fisaIulie.Situatie.Valoare == 3600m && fisaIulie.Situatie.Amortizare == cumulat
            && fisaIulie.Situatie.NetContabil == ramas && fisaIulie.Situatie.Luni == 2
            && fisaIulie.Situatie.DurataLuni == 48 && fisaIulie.Situatie.Metoda == nameof(MetodaAmortizare.Liniara)
            && fisaIulie.DurataFiscalaMinLuni == null && fisaIulie.DurataFiscalaMaxLuni == null
            && fisaIulie.Stare == nameof(StareImobilizare.Iesita)
            && fisaIulie.NumarInventar == Marcaj + "-1" && fisaIulie.TipMaterialCod == codTipF
            && fisaIulie.LocDenumire == gestiune.Denumire);

        var fisaFinal = ImobilizariApply.Fisa(os, idFisa, Zi(12, 31));
        s.Check($"API-IMO-V15 ({eticheta}) după ieșire situația fișei e ZERO pe toate coloanele — rândul `Iesire` "
            + "anulează exact ce cumulaseră evenimentele",
            fisaFinal.Situatie.Valoare == 0m && fisaFinal.Situatie.Amortizare == 0m
            && fisaFinal.Situatie.NetContabil == 0m && fisaFinal.Situatie.AmortizareDeductibila == 0m
            && fisaFinal.Randuri.Count == 5 && fisaFinal.DataIesire == Zi(8, 20));

        // ── API-IMO-V16: registrul, cu totalurile pe server ──────────────────────
        var registru = ImobilizariApply.Registru(os, Zi(7, 31));
        var randFisa = registru.Linii.FirstOrDefault(l => l.ImobilizareId == idFisa);
        Console.WriteLine($"     MĂSURAT (API-IMO-V16/{eticheta}): {registru.Linii.Count} fișe în registru la "
            + $"31.07; linia scenei = {randFisa?.Brut} brut / {randFisa?.AmortizareCumulata} cumulat / "
            + $"{randFisa?.NetContabil} net / {randFisa?.Luni} luni.");
        s.Check($"API-IMO-V16 ({eticheta}) registrul dă o linie per fișă pusă în funcțiune, calculată dintr-o "
            + "SINGURĂ citire a registrului, iar totalurile plicului sunt ale serverului — TS nu adună (42c)",
            randFisa != null && randFisa.Brut == 3600m && randFisa.BrutFiscal == 3600m
            && randFisa.AmortizareCumulata == cumulat && randFisa.AmortizareFiscalaCumulata == cumulat
            && randFisa.DeductibilCumulat == cumulat
            && randFisa.NetContabil == ramas && randFisa.NetFiscal == ramas && randFisa.Luni == 2
            && randFisa.NumarInventar == Marcaj + "-1" && randFisa.LocDenumire == gestiune.Denumire
            && registru.TotalBrut == registru.Linii.Sum(l => l.Brut)
            && registru.TotalNetContabil == registru.Linii.Sum(l => l.NetContabil)
            && registru.TotalDeductibilCumulat == registru.Linii.Sum(l => l.DeductibilCumulat));

        // ── API-IMO-V17: listele, cu agregatele în SQL ───────────────────────────
        var randPif = PifApply.Lista(os).Single(d => d.Id == idPif);
        var randCas = CasApply.Lista(os).Single(d => d.Id == idCas);
        var randAmo = AmoApply.Lista(os).Single(d => d.Id == idAmoIulie);
        Console.WriteLine($"     MĂSURAT (API-IMO-V17/{eticheta}): PIF {randPif.Total}/{randPif.NrLinii} linii; "
            + $"CAS {randCas.Total}/{randCas.NrFise} fișe; AMO {randAmo.TotalContabil}/{randAmo.An}-{randAmo.Luna}.");
        s.Check($"API-IMO-V17 ({eticheta}) listele își iau cifrele prin JOIN pe agregat, cu starea și cauza traduse "
            + "în SQL (`CASE`) ca filtrarea și sortarea să rămână server-side; luna AMO iese din `Data`, nu din "
            + "coloane persistate",
            randPif is { Total: 3600m, NrLinii: 1, Stare: "Operat" }
            && randCas is { NrFise: 1, Cauza: "Casare", Stare: "Operat" } && randCas.Total == cumulat + ramas
            && randAmo is { An: An, Luna: 7, Stare: "Operat", NrLinii: 1 }
            && randAmo.TotalContabil == cotaIulie);

        // ── API-IMO-V18: refuzurile ușii ─────────────────────────────────────────
        s.CheckRefuza($"API-IMO-V18 ({eticheta}) PUT pe un PIF OPERAT ⇒ refuz de DOMENIU: agregatul nu se mai "
            + "modifică, se anulează sau se stornează",
            () => PifApply.Aplica(os, idPif, new PifWriteDto {
                Data = Zi(5, 5), PredatorId = unitate.ID, PrimitorId = gestiune.ID,
                Linii = {
                    new PifLinieWriteDto {
                        ImobilizareId = idFisa, Fel = "Intrare", Valoare = 1m,
                        Metoda = "Liniara", DurataLuni = 36, MetodaFiscala = "Liniara", DurataFiscalaLuni = 36,
                        CategorieFiscala = "Standard", UtilizareExclusiva = true
                    }
                }
            }));
        s.CheckRefuza($"API-IMO-V19 ({eticheta}) un `Fel` de linie care nu e membru de enum ⇒ refuz la GRANIȚĂ, "
            + "înaintea oricărui `CreateObject`, cu valorile acceptate enumerate",
            () => PifApply.Aplica(os, null, new PifWriteDto {
                Data = Zi(9, 1), PredatorId = unitate.ID, PrimitorId = gestiune.ID,
                Linii = { new PifLinieWriteDto { ImobilizareId = idFisa, Fel = "Casare", Valoare = 1m } }
            }));

        var tipFaraPolitica = os.CreateObject<TipMaterial>();
        tipFaraPolitica.Cod = Marcaj + "-TIPF";
        tipFaraPolitica.Denumire = "Tip de imobilizări fără politică (ușa CAS)";
        tipFaraPolitica.ClasaId = tipF.ClasaId;
        tipFaraPolitica.ContImplicitId = tipF.ContImplicitId;
        os.CommitChanges();
        var fisaFaraPolitica = os.CreateObject<Imobilizare>();
        fisaFaraPolitica.NumarInventar = Marcaj + "-FP";
        fisaFaraPolitica.Denumire = "Fișă pe un tip fără politică de amortizare";
        fisaFaraPolitica.TipMaterialId = tipFaraPolitica.ID;
        fisaFaraPolitica.LocId = gestiune.ID;
        os.CommitChanges();
        var idFaraPolitica = fisaFaraPolitica.ID;
        var refuzFaraPolitica = s.Refuz(() => CasApply.Aplica(os, null, new CasWriteDto {
            Data = Zi(9, 10), Cauza = "Casare", PredatorId = gestiune.ID, PrimitorId = unitate.ID,
            Fise = { idFaraPolitica }
        }));
        Console.WriteLine($"     MĂSURAT (API-IMO-V20/{eticheta}): „{refuzFaraPolitica?.Split('\n')[0] ?? "ACCEPTAT"}”.");
        s.Check($"API-IMO-V20 ({eticheta}) CAS pe o fișă al cărei tip n-are politică ⇒ refuz la CULEGERE, cu ACELAȘI "
            + "text ca gardianul de operare: conturile ieșirii vin exclusiv din politică, iar ecranul află motivul "
            + "înainte de a apăsa butonul",
            refuzFaraPolitica != null
            && refuzFaraPolitica.Contains("n-are rând de politică de amortizare")
            && refuzFaraPolitica.Contains(Marcaj + "-FP"));
        string refuzFaraPoliticaXaf;
        using (var osXaf = s.Provider.CreateObjectSpace()) {
            new Atlas.Conta.BackOffice.Module.Culegere.CulegereLaCommitXaf().OnObjectSpaceCreated(osXaf);
            new GardianEditare().OnObjectSpaceCreated(osXaf);
            var casXaf = osXaf.CreateObject<IesireImobilizare>();
            casXaf.Data = Zi(9, 10);
            casXaf.DataInregistrare = casXaf.Data;
            casXaf.Cauza = CauzaIesire.Casare;
            casXaf.Predator = osXaf.GetObjectByKey<Repartitor>(gestiune.ID);
            casXaf.Primitor = osXaf.GetObjectByKey<Repartitor>(unitate.ID);
            var linieCasXaf = osXaf.CreateObject<IesireImobilizareDetaliu>();
            linieCasXaf.Document = casXaf;
            linieCasXaf.Imobilizare = osXaf.GetObjectByKey<Imobilizare>(idFaraPolitica);
            linieCasXaf.ImobilizareId = idFaraPolitica;
            linieCasXaf.TipMaterialId = tipFaraPolitica.ID;
            refuzFaraPoliticaXaf = s.Refuz(() => osXaf.CommitChanges());
            osXaf.Rollback();
        }
        Console.WriteLine($"     MĂSURAT (104c-C1/{eticheta}): „{refuzFaraPoliticaXaf?.Split('\n')[0] ?? "SALVAT"}”.");
        s.Check($"104c-C1 ({eticheta}) aceeași CAS fără politică pe calea XAF (L3 la Committing, apoi gardianul) e refuzată "
            + "la salvare cu textul ușii API: regula e a culegerii, nu a adaptorului",
            refuzFaraPoliticaXaf != null && refuzFaraPoliticaXaf == refuzFaraPolitica
            && !os.GetObjectsQuery<IesireImobilizareDetaliu>().Any(l => l.ImobilizareId == idFaraPolitica));
        s.CheckRefuza($"API-IMO-V21 ({eticheta}) aceeași fișă de două ori în `Fise` ⇒ refuz de DOMENIU (mulțimea e "
            + "reconciliată server-side, deci un id repetat ar fi trecut tăcut ca unul singur)",
            () => CasApply.Aplica(os, null, new CasWriteDto {
                Data = Zi(9, 10), Cauza = "Casare", PredatorId = gestiune.ID, PrimitorId = unitate.ID,
                Fise = { idFaraPolitica, idFaraPolitica }
            }));
        os.CommitChanges();
        s.Check($"API-IMO-V22 ({eticheta}) după refuzuri, ObjectSpace-ul n-a rămas cu obiecte pe jumătate "
            + "construite, pe care commit-ul următor le-ar fi persistat: niciun antet fără linii în septembrie, "
            + "nicio linie de ieșire pe fișa refuzată",
            !os.GetObjectsQuery<PunereInFunctiune>().Any(d => d.Data >= septStart && d.Data <= septEnd)
            && !os.GetObjectsQuery<IesireImobilizare>().Any(d => d.Data >= septStart && d.Data <= septEnd)
            && !os.GetObjectsQuery<IesireImobilizareDetaliu>().Any(l => l.ImobilizareId == idFaraPolitica));

        // Gardianul fișei: ObjectSpace PROPRIU, aruncat — ușa OData nu dublează regula,
        // o ridică `GardianEditare` pe ușa comună.
        using (var osGardian = s.Provider.CreateObjectSpace()) {
            osGardian.Delete(osGardian.GetObjectByKey<Imobilizare>(idFisa));
            string refuzStergere = null;
            try { GardianEditare.Verifica(osGardian); }
            catch (OperareException e) { refuzStergere = e.Message; }
            Console.WriteLine($"     MĂSURAT (API-IMO-V23/{eticheta}): ștergerea unei fișe mișcate → "
                + $"„{refuzStergere?.Split('\n')[0] ?? "ACCEPTATĂ"}”.");
            s.Check($"API-IMO-V23 ({eticheta}) ștergerea unei fișe care a fost mișcată e refuzată de GARDIAN, pe ușa "
                + "comună — CRUD-ul nomenclatorului stă pe OData tocmai fiindcă regula nu se rescrie per ușă",
                refuzStergere != null && refuzStergere.Contains("se șterge doar cât e Nouă"));
        }

        // ── Curățenia finală ─────────────────────────────────────────────────────
        CurataApiImo();
        s.Check($"Api IMO ({eticheta}): scena nu lasă urme — fișele, documentele, registrele și perioadele ei sunt "
            + "purjate FIZIC, iar fereastra rămâne liberă pentru blocul următor și pentru Import1C",
            !os.GetObjectsQuery<Imobilizare>().Any(f => f.NumarInventar.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == An && p.Luna >= 5)
            && !os.GetObjectsQuery<Document>()
                .Any(d => d.Data >= primaZi && d.Data <= ultimaZi));
    }
}
