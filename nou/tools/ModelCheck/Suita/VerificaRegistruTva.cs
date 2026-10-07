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

// ============ Felia 11 (jurnalele de TVA): registrul fiscal (JT-D1…JT-D6) ============
// Al treilea registru se judecă exact ca primele două: nu prin scenă, ci prin
// CUSĂTURĂ — Σ TVA-ului fiscal al unui document == Σ postării lui contabile pe
// conturile de TVA (JT-D6). Aia e proba că registrul nu e „al doilea adevăr”:
// dacă derivarea și postarea ar diverge, felia ar produce un jurnal frumos și
// fals. Cusătura e per DOCUMENT, nu pe perioadă, fiindcă agregatul de perioadă ar
// include închiderea lunară (ITV mișcă 4426/4427 fără să fie operațiune taxabilă)
// și ar cere excluderi scrise de mână; per document, ITV pur și simplu nu apare.
//
// Local function, apelată din AMBELE căi de profil (ca `VerificaBalanta`) —
// `cuTva` = true doar pe privat, singurul profil care are rânduri `PoliticaTva`.
// Universalele rulează peste TOATĂ baza, nu peste scenă.
static class VerificaRegistruTva {
    public static void Ruleaza(Suita s, bool cuTva) {
        const string MarcajJt = "E2E-JTVA";
        using var os = s.Provider.CreateObjectSpace();

        void CurataJt() {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => d.Numar.StartsWith(MarcajJt)).Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruTva>().Where(r => ids.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)).ToList());
            var loturi = os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(MarcajJt)).ToList();
            var idsLot = loturi.Select(l => l.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => (r.DocumentId != null && ids.Contains(r.DocumentId.Value)) || idsLot.Contains(r.LotId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList());
            pj.Adauga(loturi);
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(MarcajJt)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajJt)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(MarcajJt)).ToList());
            pj.Executa();
        }
        CurataJt();

        List<Atlas.Conta.BackOffice.Module.Cub.Citiri.FaptFiscal> FiscalCub(Document d) => CubScena.Fapte(os, d.ID);

        // ---------------- Scena (doar profilul privat) ----------------
        // Scena e ÎMPĂRȚITĂ deliberat în două: partea de construcție lasă documentele
        // OPERATE, iar storno-ul și anularea vin abia DUPĂ universale. Motivul e că
        // universalele rulează peste toată baza, iar restul suitei își curăță scenele
        // — dacă și asta ar face-o înainte, cusătura JT-D6 s-ar măsura pe zero
        // documente (poartă vacuă), sau, mai rău, doar pe unul stornat, unde ambele
        // părți se netează la zero și o derivare greșită simetric ar trece neobservată.
        FacturaIntrare fctA = null, fctB = null, fctC = null;
        FacturaIesire fclD = null;
        Document conexB = null;
        var dataStorno = new DateOnly(2026, 7, 24);
        // Perioada scenei — filtrul cu care se citesc proiecțiile mai jos. Storno-ul
        // cade DELIBERAT în afara ei (iulie), ca jurnalul lunii deja declarate să se
        // vadă „cum a fost declarat" (JT-D5).
        var pStart = new DateOnly(2026, 3, 1);
        var pEnd = new DateOnly(2026, 3, 31);
        if (cuTva) {
            var mag = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
            var tip302 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "302");
            var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var ti21 = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
            var sdd = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
            var conturiTvaScena = new[] { n21.ContTvaDeductibilId, n21.ContTvaColectatId }
                .Where(id => id != null).Select(id => id.Value).ToList();

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajJt + "-FURN";
            furnizor.Denumire = "Furnizor probă jurnal TVA";
            // Codul fiscal e SETAT deliberat: pe `RegistruTva` partenerul e tipat
            // `Repartitor` (baza ierarhiei — pe Decont e chiar angajatul), deci jurnalul îl
            // scoate prin as-cast pe frunza `Partener`. Cu câmpul gol, proba ar fi
            // trecut comparând null cu null.
            furnizor.CodFiscal = "RO12345678";
            var produs = os.CreateObject<Produs>();
            produs.Cod = MarcajJt;
            produs.Denumire = "Produs probă jurnal TVA";
            produs.UM = "BUC";
            produs.TipMaterial = tip302;
            os.CommitChanges();

            FacturaIntrare Factura(string sufix, DateOnly data) {
                var f = os.CreateObject<FacturaIntrare>();
                f.Numar = MarcajJt + sufix;
                f.Data = data;
                f.Predator = furnizor;
                f.Primitor = mag;
                return f;
            }
            FacturaIntrareDetaliu Linie(FacturaIntrare f, TipMaterial tip, TipTva tva, decimal pret, decimal cantitate = 1m) {
                var d = os.CreateObject<FacturaIntrareDetaliu>();
                d.Document = f;
                d.TipMaterial = tip;
                d.Cantitate = cantitate;
                d.PretUnitar = pret;
                d.TipTva = tva;
                return d;
            }

            // --- Cele patru regimuri, pe UN singur document ---
            // Prețul liniei capitalizate e ales ca desfacerea bazei să NU fie exactă:
            // 82,644628 × 1,21 = 100,00 brut, iar 100,00 / 1,21 = 82,6446… — cazul în
            // care o formulă simetrică ar lăsa un ban pe drum.
            fctA = Factura("-F1", new DateOnly(2026, 3, 20));
            var lNormal = Linie(fctA, tip628, n21, 100m);
            var lInversa = Linie(fctA, tip628, ti21, 50m);
            var lScutit = Linie(fctA, tip628, sdd, 70m);
            var lCapitalizat = Linie(fctA, tip628, ned21, 82.644628m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctA);

            var fiscalACub = FiscalCub(fctA);
            Atlas.Conta.BackOffice.Module.Cub.Citiri.FaptFiscal RandCub(DocumentDetaliu d) =>
                fiscalACub.SingleOrDefault(f => f.DetaliuId == d.ID);
            s.Check("JT-D4: cele patru regimuri pe un singur document → patru rânduri fiscale, câte o formulă fiecare "
                + "(Normal 100/21, TaxareInversa 50/10,5, Scutit 70/0, Capitalizat 100 brut → 82,64/17,36)",
                fiscalACub.Count == 4
                && RandCub(lNormal) is { Regim: RegimTva.Normal, Cota: 21m, Baza: 100m, Tva: 21m }
                && RandCub(lInversa) is { Regim: RegimTva.TaxareInversa, Cota: 21m, Baza: 50m, Tva: 10.5m }
                && RandCub(lScutit) is { Regim: RegimTva.Scutit, Cota: 0m, Baza: 70m, Tva: 0m }
                && RandCub(lCapitalizat) is { Regim: RegimTva.Capitalizat, Cota: 21m, Baza: 82.64m, Tva: 17.36m });
            s.Check("JT-D1/JT-D3: sensul vine din direcția politicii (FCT deduce → Achiziție), partenerul din latura "
                + "declarată (RepartitorPredator → furnizorul), data e a documentului, rândurile nu sunt storno",
                fiscalACub.Count > 0
                && fiscalACub.All(f => f.Sens == SensTva.Achizitie && f.PartenerId == furnizor.ID
                    && f.DataDocument == fctA.Data && !f.Storno && f.DocumentId == fctA.ID));

            var noteTvaACub = CubScena.Note(os, fctA.ID).Where(p => !p.Storno && conturiTvaScena.Contains(p.Cont)).ToList();
            s.Check("MOTIVUL DE EXISTENȚĂ al registrului (design, „de ce nu o proiecție peste RegistruContabil”): liniile "
                + "Scutit și Capitalizat nu produc NICIUN rând contabil de TVA, dar AU rând fiscal — o proiecție peste "
                + "4426/4427 le-ar fi pierdut tăcut, adică raport incomplet cu aparență de raport complet",
                RandCub(lScutit) != null && RandCub(lCapitalizat) != null
                && !noteTvaACub.Any(p => p.LinieId == lScutit.ID || p.LinieId == lCapitalizat.ID)
                && noteTvaACub.Count > 0);
            s.Check("JT-D4, Capitalizat: se rotunjește BAZA, iar TVA-ul e DIFERENȚA — deci Baza + Tva == valoarea BRUTĂ a "
                + "liniei, EXACT, fără reziduu de rotunjire (82,64 + 17,36 == 100,00)",
                lCapitalizat.Valoare == 100m && lCapitalizat.ValoareTva == 0m
                && RandCub(lCapitalizat).Baza + RandCub(lCapitalizat).Tva == lCapitalizat.Valoare);

            // --- Conexul, pe o factură cu linie de stoc ---
            fctB = Factura("-F2", new DateOnly(2026, 3, 21));
            var linieStoc = Linie(fctB, tip302, n21, 10m, cantitate: 5m);
            linieStoc.ProdusId = produs.ID;
            linieStoc.CreeazaLot(os, produs, mag);
            os.CommitChanges();
            conexB = MotorOperare.Opereaza(os, fctB);
            MotorOperare.Opereaza(os, conexB);
            s.Check("JT-D2, cealaltă jumătate a criteriului: NIR-ul conex CLONEAZĂ TipTvaId (ca informație — 26b), dar "
                + "tipul lui n-are rând PoliticaTva ⇒ zero rânduri fiscale; TVA-ul rămâne al facturii, cu un rând",
                conexB is NIR { Stare: StareDocument.Operat }
                && conexB.Detalii.All(d => d.TipTvaId == n21.ID)
                && FiscalCub(conexB).Count == 0 && FiscalCub(fctB).Count == 1);

            // --- A treia factură: contopirea + perechea SDD/SFD (pentru proiecții) ---
            // Două linii cu ACELAȘI TipTva (jurnalul le contopește într-un rând: un
            // jurnal listează facturi, nu poziții) și două cu regim și cotă IDENTICE
            // dar TipTva diferit — perechea care demonstrează de ce cheia decontului e
            // `TipTva` și nu (Regim × Cota).
            var sfd = os.FirstOrDefault<TipTva>(t => t.Cod == "SFD");
            fctC = Factura("-F3", new DateOnly(2026, 3, 22));
            Linie(fctC, tip628, n21, 40m);
            Linie(fctC, tip628, n21, 60m);
            Linie(fctC, tip628, sdd, 30m);
            Linie(fctC, tip628, sfd, 20m);
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctC);
            s.Check("Premisa contopirii: patru linii, dintre care DOUĂ cu același TipTva (N21) — registrul are patru "
                + "rânduri (granularitatea SAF-T, JT-D1), iar SDD/SFD au regim și cotă identice (Scutit, 0)",
                FiscalCub(fctC).Count == 4
                && FiscalCub(fctC).Count(f => f.TipTvaId == n21.ID) == 2
                && sdd.Regim == sfd.Regim && sdd.Cota == sfd.Cota && sdd.ID != sfd.ID);

            // --- O LIVRARE, ca sensul să fie exercitat pe AMBELE laturi ---
            // Fără ea, direcționalitatea codului SAF-T (JT-D3) s-ar fi „verificat"
            // doar pe ramura de achiziție — adică pe jumătate.
            var sediuJt = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
            var tip704Jt = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
            var clientJt = os.CreateObject<Partener>();
            clientJt.Cod = MarcajJt + "-CLIENT";
            clientJt.Denumire = "Client probă jurnal TVA";
            fclD = os.CreateObject<FacturaIesire>();
            // Numărul se pre-completează deliberat: FCL ARE politică de numerotare, iar
            // `AsignaNumar` onorează numărul cules (55a) — altfel documentul ar fi ieșit
            // cu seria fiscală și curățenia scenei (care caută marcajul în `Numar`) l-ar
            // fi lăsat în urmă.
            fclD.Numar = MarcajJt + "-V1";
            fclD.Data = new DateOnly(2026, 3, 23);
            fclD.Predator = sediuJt;
            fclD.Primitor = clientJt;
            var lVanzare = os.CreateObject<FacturaIesireDetaliu>();
            lVanzare.Document = fclD;
            lVanzare.TipMaterial = tip704Jt;
            lVanzare.Cantitate = 1m;
            lVanzare.PretUnitar = 200m;
            lVanzare.TipTva = n21;
            os.CommitChanges();
            MotorOperare.Opereaza(os, fclD);
            s.Check("Cealaltă latură: o LIVRARE cu același TipTva (N21) produce un rând fiscal cu Sens = Livrare și "
                + "partenerul din latura declarată de politică (RepartitorPrimitor → clientul)",
                FiscalCub(fclD).Count == 1
                && FiscalCub(fclD)[0] is { Sens: SensTva.Livrare, Baza: 200m, Tva: 42m } r0Cub
                && r0Cub.PartenerId == clientJt.ID && r0Cub.TipTvaId == n21.ID);
        }
        else {
            // Scenă MINIMĂ pe bugetar, cerută de premisa verificării 2: pe o bază
            // curată la capătul suitei nu mai există niciun document operat, deci
            // „zero rânduri fiscale” ar fi fost adevărat și pe o derivare complet
            // greșită. Un singur document, cu TipTva pe linie, îi dă dinți.
            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var cap21 = os.FirstOrDefault<TipTva>(t => t.Cod == "CAP21");

            var furnizorB = os.CreateObject<Partener>();
            furnizorB.Cod = MarcajJt + "-FURN";
            furnizorB.Denumire = "Furnizor probă jurnal TVA";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajJt + "-CE";
            codEc.Denumire = "Cod economic probă jurnal TVA";
            os.CommitChanges();

            fctA = os.CreateObject<FacturaIntrare>();
            fctA.Numar = MarcajJt + "-F1";
            fctA.Data = new DateOnly(2026, 3, 20);
            fctA.Predator = furnizorB;
            fctA.Primitor = mag1;
            var linieB = os.CreateObject<FacturaIntrareDetaliu>();
            linieB.Document = fctA;
            linieB.TipMaterial = tipServicii;
            linieB.Cantitate = 1m;
            linieB.PretUnitar = 100m;
            linieB.TipTva = cap21;
            linieB.CodEconomicId = codEc.ID; // clasificația cerută de PoliticaValidare bugetară
            os.CommitChanges();
            MotorOperare.Opereaza(os, fctA);
            s.Check("Premisa verificării 2, construită explicit: pe profilul bugetar o factură cu TipTva pe linie "
                + "(CAP21, regim Capitalizat) se operează normal și postează brutul (121) — dar NU produce niciun "
                + "rând fiscal, fiindcă tipul ei n-are PoliticaTva",
                fctA.Stare == StareDocument.Operat && linieB.TipTvaId == cap21.ID
                && linieB.Valoare == 121m && FiscalCub(fctA).Count == 0);
        }

        // ---------------- Universalele: peste TOATĂ baza, nu peste scenă ----------------

        // Conturile de TVA se citesc ca DATE (TipTva.ContTvaDeductibil/Colectat —
        // decizia 29: motorul e agnostic la plan, deci și proba lui). Nu 4426/4427.
        var conturiTva = os.GetObjectsQuery<TipTva>()
            .Select(t => new { t.ContTvaDeductibilId, t.ContTvaColectatId }).ToList()
            .SelectMany(t => new[] { t.ContTvaDeductibilId, t.ContTvaColectatId })
            .Where(id => id != null).Select(id => id.Value).Distinct().ToList();

        var fiscaleCub = Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Fapte(os).ToList();
        var docIdsFiscaleCub = fiscaleCub.Select(f => f.DocumentId).Distinct().ToList();

        var numeCuPolitica = os.GetObjectsQuery<PoliticaTva>().Select(p => p.TipDocument.ClrType).ToList();
        var operate = os.GetObjectsQuery<Document>().Where(d => d.Stare == StareDocument.Operat)
            .Select(d => new { d.ID, d.ClrType }).ToList();
        var idsOperate = operate.Select(d => d.ID).ToList();
        var idsCuPolitica = operate.Where(d => numeCuPolitica.Contains(d.ClrType)).Select(d => d.ID).ToHashSet();
        var liniiOperate = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(d => idsOperate.Contains(d.DocumentId))
            .Select(d => new { d.DocumentId, d.TipTvaId }).ToList();

        // VERIFICAREA 2 — bugetarul rămâne GOL, cu premisa verificată (altfel proba
        // n-ar avea dinți: „zero rânduri” e trivial pe o bază fără linii fiscale).
        if (!cuTva) {
            var cuTipTva = liniiOperate.Count(l => l.TipTvaId != null);
            s.Check("VERIFICAREA 2: profilul bugetar n-are NICIUN rând fiscal, deși premisa are dinți — există "
                + $"{cuTipTva} linii de documente OPERATE care CHIAR poartă TipTva (CAP21 e implicitul FCT/FCL/DEC "
                + "acolo). Criteriul JT-D2 cere ȘI politica, iar profilul neplătitor n-are niciun rând PoliticaTva: "
                + "un criteriu bazat doar pe linie ar fi fabricat un jurnal de TVA pentru un neplătitor",
                fiscaleCub.Count == 0 && numeCuPolitica.Count == 0 && cuTipTva > 0);
        }

        // VERIFICAREA 3 — găurile declarate se MĂSOARĂ, nu se umplu cu presupuneri.
        // (a) JT-D2: linia fără TipTva de pe un document cu politică rămâne în afara
        //     jurnalului — gaură a DATELOR, nu a modelului.
        // (b) Riscul 4 din design: `SursaContrapartida` care nu e o latură
        //     (Explicit/TipMaterial) ⇒ partener null, raportat nu refuzat.
        // Check-ul asertează doar coerența numărătorii (sunt măsurători, nu
        // invarianți) — plus un invariant care ÎNCAPE aici și chiar are dinți:
        // partenerul, când există, e una dintre laturile documentului lui.
        var liniiFaraTipTva = liniiOperate.Count(l => idsCuPolitica.Contains(l.DocumentId) && l.TipTvaId == null);
        var faraPartener = fiscaleCub.Count(f => f.PartenerId == null);
        Console.WriteLine($"     MĂSURAT (JT-D6 verificarea 3): {liniiFaraTipTva} linii fără TipTva pe documente operate "
            + $"ale unui tip cu PoliticaTva (din {liniiOperate.Count} linii operate în bază); "
            + $"{faraPartener} fapte fiscale fără partener (din {fiscaleCub.Count}).");
        var laturiCub = os.GetObjectsQuery<Document>()
            .Where(d => docIdsFiscaleCub.Contains(d.ID))
            .Select(d => new { d.ID, d.PredatorId, d.PrimitorId })
            .ToDictionary(d => d.ID, d => (d.PredatorId, d.PrimitorId));
        s.Check("VERIFICAREA 3: găurile declarate sunt numărabile și coerente (liniile fără TipTva ⊆ liniile operate, "
            + "rândurile fără partener ⊆ rândurile fiscale), iar partenerul — când există — e chiar una dintre "
            + "laturile documentului, adică repartitorul laturii cerute de politică, nu un terț inventat",
            liniiFaraTipTva <= liniiOperate.Count && fiscaleCub.Count(f => f.PartenerId == null) <= fiscaleCub.Count
            && fiscaleCub.Where(f => f.PartenerId != null).All(f =>
                laturiCub.TryGetValue(f.DocumentId, out var l)
                && (f.PartenerId == l.PredatorId || f.PartenerId == l.PrimitorId)));

        // ---------------- Scena, partea a doua: storno și anulare ----------------
        if (cuTva) {
            MotorOperare.Storneaza(os, fctA, dataStorno);
            var dupaStornoCub = FiscalCub(fctA);
            s.Check("JT-D5, storno: patru rânduri inverse la DATA STORNĂRII (Storno = true, bază/TVA cu semn schimbat), "
                + "identitatea fiscală copiată ca snapshot — suma algebrică pe document e zero pe ambele coloane",
                dupaStornoCub.Count == 8 && dupaStornoCub.Count(f => f.Storno) == 4
                && dupaStornoCub.Where(f => f.Storno).All(f => f.Data == dataStorno)
                && dupaStornoCub.Sum(f => f.Baza) == 0m && dupaStornoCub.Sum(f => f.Tva) == 0m
                && dupaStornoCub.Where(f => f.Storno).All(s => dupaStornoCub.Any(o => !o.Storno
                    && o.DetaliuId == s.DetaliuId && o.TipTvaId == s.TipTvaId && o.Sens == s.Sens
                    && o.Regim == s.Regim && o.Cota == s.Cota && o.PartenerId == s.PartenerId
                    && o.Baza == -s.Baza && o.Tva == -s.Tva)));

            // ═══ REGRESIE, review advers D3 ═══
            // Interogarea CEA MAI FIREASCĂ a jurnalului — anul întreg, adică perioada
            // care cuprinde ȘI operarea (martie) ȘI stornarea (iulie) — era exact cea
            // pe care felia o rata: fără `Storno` în cheia de grupare, originalul și
            // inversul se netau într-un singur rând de 0,00 lei, datat în martie.
            // Factura apărea ca „factură de zero lei", iar stornarea dispărea complet
            // ca eveniment. Totalurile perioadei rămâneau corecte — se pierdea exact
            // granularitatea per document pe care o cere D394.
            //
            // Scena de dinainte nu putea s-o prindă, fiindcă era construită să nu poată:
            // toate probele de jurnal foloseau perioade care conțin doar una dintre
            // cele două date.
            var anul = TvaProiectii.JurnalTva(os, SensTva.Achizitie,
                    new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31))
                .Where(r => r.DocumentId == fctA.ID).ToList();
            s.Check("REGRESIE D3: pe anul întreg — perioada care cuprinde și operarea (20.03), și stornarea (24.07) — "
                + "factura cu patru regimuri dă OPT rânduri de jurnal, nu patru de zero lei: patru originale datate "
                + "în martie și patru de storno datate în iulie, fiecare cu semnul lui. `Storno` e în cheia de "
                + "grupare tocmai fiindcă, spre deosebire de partener/regim/cotă, NU e determinat de perechea "
                + "(Document × TipTva) — sunt două fapte fiscale distincte, la date distincte",
                anul.Count == 8
                && anul.Count(r => !r.Storno) == 4 && anul.Count(r => r.Storno) == 4
                && anul.Where(r => !r.Storno).All(r => r.Data == fctA.Data)
                && anul.Where(r => r.Storno).All(r => r.Data == dataStorno)
                // …și fiecare original își are inversul exact, pe aceeași identitate
                // fiscală: netarea rămâne corectă ca SUMĂ, doar că nu mai e impusă.
                && anul.Where(r => r.Storno).All(s => anul.Any(o => !o.Storno
                    && o.TipTvaId == s.TipTvaId && o.Baza == -s.Baza && o.Tva == -s.Tva))
                && anul.Sum(r => r.Baza) == 0m && anul.Sum(r => r.Tva) == 0m);

            MotorOperare.AnuleazaOperarea(os, conexB);
            MotorOperare.AnuleazaOperarea(os, fctB);
            s.Check("JT-D5, anulare: corecția directă șterge rândurile fiscale odată cu celelalte două registre — "
                + "documentul revenit în Draft nu mai are nicio urmă în jurnal",
                fctB.Stare == StareDocument.Draft && FiscalCub(fctB).Count == 0
                && CubScena.FaraNote(os, fctB.ID));
        }

        // ---------------- Proiecțiile (JT-D7) ----------------
        // Rulate DUPĂ storno și anulare, deliberat: jurnalul trebuie citit peste un
        // registru care conține deja rânduri inverse (fctA, stornată în IULIE) și un
        // document care și-a pierdut rândurile (fctB, anulată). Perioada scenei e
        // MARTIE, deci storno-ul cade în afara ei — exact JT-D5: „jurnalul lunii deja
        // declarate rămâne cum a fost declarat".
        if (cuTva) {
            var n21P = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
            var sddP = os.FirstOrDefault<TipTva>(t => t.Cod == "SDD");
            var sfdP = os.FirstOrDefault<TipTva>(t => t.Cod == "SFD");

            var jurnale = new Dictionary<SensTva, List<JurnalTvaRand>>();
            foreach (var sens in new[] { SensTva.Achizitie, SensTva.Livrare })
                jurnale[sens] = TvaProiectii.JurnalTva(os, sens, pStart, pEnd).ToList();
            var fapteMartieCub = CubScena.FapteIntre(os, pStart, pEnd);
            var cusaturaOkCub = true;
            var chiarAgregaCub = false;
            foreach (var sens in new[] { SensTva.Achizitie, SensTva.Livrare }) {
                var brutCub = fapteMartieCub.Where(f => f.Sens == sens).ToList();
                cusaturaOkCub &= jurnale[sens].Sum(r => r.Baza) == brutCub.Sum(f => f.Baza)
                    && jurnale[sens].Sum(r => r.Tva) == brutCub.Sum(f => f.Tva)
                    && jurnale[sens].Count == brutCub.Select(f => (f.DocumentId, f.TipTvaId)).Distinct().Count()
                    && jurnale[sens].All(r => r.Data >= pStart && r.Data <= pEnd);
                chiarAgregaCub |= brutCub.Count > jurnale[sens].Count && jurnale[sens].Count > 0;
            }
            s.Check("VERIFICAREA 4 (JT-D7): pe fiecare sens, jurnalul == registrul — Σ bază și Σ TVA identice, iar "
                + "numărul de rânduri de jurnal == numărul de perechi (Document × TipTva) DISTINCTE ale perioadei; "
                + "toate rândurile cad în perioadă (storno-ul din iulie nu se strecoară în jurnalul lui martie). "
                + "Premisa: agregarea chiar contopește rânduri pe cel puțin o latură",
                cusaturaOkCub && chiarAgregaCub
                && jurnale[SensTva.Achizitie].Count > 0 && jurnale[SensTva.Livrare].Count > 0);

            var jurnalAch = jurnale[SensTva.Achizitie];
            var randuriC = jurnalAch.Where(r => r.DocumentId == fctC.ID).ToList();
            var randuriA = jurnalAch.Where(r => r.DocumentId == fctA.ID).ToList();
            s.Check("VERIFICAREA 5 (JT-D7): jurnalul AGREGĂ, nu pierde — factura cu patru linii de TipTva-uri DIFERITE "
                + "dă patru rânduri, iar cea cu două linii de ACELAȘI TipTva le contopește (40 + 60 = 100 bază, 21 "
                + "TVA), deci trei rânduri din patru linii; Σ per document se conservă în ambele cazuri",
                randuriA.Count == 4
                && randuriC.Count == 3
                && randuriC.Single(r => r.TipTvaId == n21P.ID) is { Baza: 100m, Tva: 21m }
                && randuriC.Sum(r => r.Baza) == 150m
                && randuriC.All(r => r.DocumentNumar == fctC.Numar && r.PartenerId == fctC.PredatorId));

            // Etichetele (JT-D3): join la citire, nu snapshot.
            s.Check("JT-D3: etichetele se rezolvă la CITIRE — denumirea și codul fiscal ale partenerului, codul și "
                + "denumirea tipului de TVA — iar cota și regimul vin de pe RÂND (snapshot), nu din nomenclatorul de azi",
                randuriC.All(r => r.PartenerDenumire == fctC.Predator.Denumire
                    && r.PartenerCodFiscal == (fctC.Predator as Partener).CodFiscal)
                && randuriC.Single(r => r.TipTvaId == sfdP.ID) is { TipTvaCod: "SFD", Regim: "Scutit", Cota: 0m }
                && randuriC.Single(r => r.TipTvaId == n21P.ID) is { TipTvaCod: "N21", Regim: "Normal", Cota: 21m });

            // ═══ Decontul: aceeași cifră, cealaltă față ═══
            var decont = TvaProiectii.DecontTva(os, pStart, pEnd).ToList();
            var decontOk = true;
            foreach (var (sens, eticheta) in new[] {
                (SensTva.Achizitie, "Achizitie"), (SensTva.Livrare, "Livrare")
            }) {
                var dinJurnal = jurnale[sens]
                    .GroupBy(r => new { r.TipTvaId, r.Regim, r.Cota })
                    .Select(g => (g.Key.TipTvaId, g.Key.Regim, g.Key.Cota,
                        Baza: g.Sum(x => x.Baza), Tva: g.Sum(x => x.Tva)))
                    .OrderBy(x => x.TipTvaId).ThenBy(x => x.Cota).ToList();
                var dinDecont = decont.Where(r => r.Sens == eticheta)
                    .Select(r => (r.TipTvaId, r.Regim, r.Cota, r.Baza, r.Tva))
                    .OrderBy(x => x.TipTvaId).ThenBy(x => x.Cota).ToList();
                decontOk &= dinJurnal.SequenceEqual(dinDecont);
            }
            s.Check("VERIFICAREA 6 (JT-D7): decontul == jurnalul, pe aceeași perioadă și pe ambele sensuri — aceleași "
                + "chei (TipTva × Regim × Cotă) cu aceleași sume. Sunt cele două fețe ale ACELUIAȘI registru, deci "
                + "o divergență ar însemna că una dintre agregări minte",
                decontOk && decont.Count > 0
                && decont.Sum(r => r.Randuri) == fapteMartieCub.Count);

            // ═══ MOTIVUL pentru care cheia decontului e `TipTva` ═══
            var scutite = decont.Where(r => r.Sens == "Achizitie"
                && (r.TipTvaId == sddP.ID || r.TipTvaId == sfdP.ID)).ToList();
            s.Check("VERIFICAREA 7 (JT-D7, corectura designului): SDD (scutit CU drept de deducere) și SFD (FĂRĂ drept) "
                + "au ACELAȘI regim și aceeași cotă 0, dar coduri SAF-T diferite și rânduri diferite în D300 — și dau "
                + "DOUĂ rânduri de decont, nu unul. O grupare pe (Regim × Cotă), cum spunea prima formulare, le-ar fi "
                + "fuzionat, adică ar fi produs exact cifra pe care declarația n-o poate folosi",
                scutite.Count == 2
                && scutite.Select(r => r.TipTvaCod).OrderBy(c => c).SequenceEqual(new[] { "SDD", "SFD" })
                && scutite.Select(r => (r.Regim, r.Cota)).Distinct().Count() == 1
                // Bazele DIFERĂ, deci fuziunea chiar ar fi pierdut informație: SDD
                // adună liniile ambelor facturi ale scenei (70 pe F1 + 30 pe F3), SFD
                // are doar linia de 20 de pe F3. Un singur rând ar fi arătat 120 pe un
                // cod SAF-T ales la întâmplare dintre cele două.
                && scutite.Single(r => r.TipTvaCod == "SDD").Baza == 100m
                && scutite.Single(r => r.TipTvaCod == "SFD").Baza == 20m
                && scutite.All(r => r.Tva == 0m));

            // ═══ Codul SAF-T e DIRECȚIONAL (JT-D3) ═══
            var n21Ach = jurnalAch.First(r => r.TipTvaId == n21P.ID);
            var n21Liv = jurnale[SensTva.Livrare].First(r => r.TipTvaId == n21P.ID);
            s.Check("VERIFICAREA 8 (JT-D3): codul SAF-T e o etichetă DIRECȚIONALĂ, rezolvată la citire din același "
                + "nomenclator — ACELAȘI TipTva (N21) iese cu codul de achiziție pe jurnalul de cumpărări și cu cel "
                + "de livrare pe cel de vânzări, iar cele două chiar diferă",
                n21Ach.CodSafT == n21P.CodSafTAchizitie && n21Liv.CodSafT == n21P.CodSafTLivrare
                && !string.IsNullOrEmpty(n21Ach.CodSafT) && n21Ach.CodSafT != n21Liv.CodSafT
                && decont.Where(r => r.TipTvaId == n21P.ID)
                    .All(r => r.CodSafT == (r.Sens == "Achizitie" ? n21P.CodSafTAchizitie : n21P.CodSafTLivrare)));

            // ═══ Prin `DataSourceLoader`, PAGINAT (lecția care a produs două defecte
            //     în felia 9) ═══
            // Aici hazardul e mai mare decât la balanță, nu mai mic: `JurnalTvaRand`
            // n-are niciun membru numit „Id", deci convenția EF a bibliotecii
            // (`EFSorting.IsEFCodeFirstConventionalKey`) nu se aplică, iar fallback-ul
            // ei alege PRIMA proprietate dintr-un tip sortabil în ordinea
            // int → long → Guid …, adică `DocumentId` — cheie care se repetă de trei
            // ori pe o singură factură cu trei tipuri de TVA. Fără `OrdineJurnalTva()`,
            // `ORDER BY "DocumentId"` sub `LIMIT/OFFSET` n-are ordine garantată.
            // Filtrul pe numărul documentului e chiar ce pune grila pe o coloană de
            // ieșire (legitim) și ține proba mărginită la scenă.
            List<JurnalTvaRand> JurnalPrinLoader(int skip, int take) {
                var optiuni = new DataSourceLoadOptionsBase {
                    Skip = skip, Take = take,
                    Filter = new object[] { "DocumentNumar", "startswith", MarcajJt }
                };
                OrdineLista.AplicaOrdineImplicita(optiuni, TvaProiectii.OrdineJurnalTva());
                return DataSourceLoader.Load(
                    TvaProiectii.JurnalTva(os, SensTva.Achizitie, pStart, pEnd), optiuni)
                    .data.Cast<JurnalTvaRand>().ToList();
            }
            var totul = JurnalPrinLoader(0, 1000);
            string CheieJt(JurnalTvaRand r) => $"{r.DocumentId}|{r.TipTvaId}";
            var pagini = new List<JurnalTvaRand>();
            for (var skip = 0; skip < totul.Count; skip += 2)
                pagini.AddRange(JurnalPrinLoader(skip, 2));
            s.Check("VERIFICAREA 9 (regresia feliei 9): prin `DataSourceLoader`, paginat din 2 în 2, jurnalul reproduce "
                + "EXACT mulțimea unei singure cereri — fără duplicate, fără rânduri sărite — și în ordinea DECLARATĂ; "
                + "premisa (cheia pe care ar inventa-o biblioteca, `DocumentId`, chiar se repetă) e verificată în "
                + "aceeași trecere",
                totul.Count == 7
                && totul.GroupBy(r => r.DocumentId).Any(g => g.Count() > 1)
                && pagini.Count == totul.Count
                && pagini.Select(CheieJt).Distinct().Count() == pagini.Count
                && pagini.Select(CheieJt).SequenceEqual(totul.Select(CheieJt))
                // …iar ordinea declarată e chiar cea cerută: cronologic, apoi pe cheia
                // de grupare.
                && totul.Select(r => r.Data).SequenceEqual(totul.Select(r => r.Data).OrderBy(d => d)));
        }

        CurataJt();
        s.Check("Curățenie finală felia registru TVA (fără reziduuri e2e)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajJt))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(MarcajJt))
            && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajJt))
            && !os.GetObjectsQuery<Document>().Any(d => d.Numar.StartsWith(MarcajJt)));
    }
}
