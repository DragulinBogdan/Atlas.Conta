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

// ============= Felia API ITV (F21) — E2E-API-ITV (privat) =============
// Închiderea de TVA parcursă prin CONTRACTUL feliei: `Previzualizeaza` →
// `Genereaza` → `Citeste` → `Regenereaza` → `ComenziDocument` → storno →
// `Sterge`. Endpoint-urile din host sunt transport peste EXACT acest cod.
//
// Semantica de MOTOR e deja acoperită de blocul `E2E-ITV` de mai sus (lunile
// 9/10): transferul pe minim, excedentul într-un singur sens, idempotența,
// cronologia, anti-stale. Aici se probează UȘA, și mai ales cele patru
// cusături care, dacă se rup, fac ecranul să MINTĂ:
//   * previzualizarea și generarea sunt ACELAȘI calcul, nu două (F21-D2a);
//   * `Stale` din ReadDto spune exact ce refuză gardianul anti-stale la
//     operare (F21-D6) — altfel butonul „Operează" promite ce serverul neagă;
//   * `Regenereaza` chiar înlocuiește draftul: dacă draftul vechi ar mai
//     apărea la a doua interogare a idempotenței, generarea ar fi întors
//     `InchidereVie` și luna ar fi rămas blocată (regula de oprire F21-D11 —
//     proba de mai jos e chiar detectorul);
//   * ITV a IEȘIT din felia NTC (F21-D5), pe toate cele patru uși.
//
// Luna de lucru e NOIEMBRIE 2026, decembrie ține nota de control a lui
// F21-D5. Blocul stă IMEDIAT după `E2E-ITV` și înaintea scenelor ASM
// (noiembrie) și RLF/RDC (decembrie), care folosesc aceleași luni: la
// punctul ăsta ele n-au rulat încă, iar blocul de față își purjează scena
// FIZIC înainte ca ele să pornească. Precondiția nu se presupune, se
// MĂSOARĂ — solduri 4426/4427 zero la 30.11 și 31.12 și NICIO închidere vie
// în bază (`E2E-ITV` tocmai a probat că nu mai există niciuna).
static class E2eApiItv {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajApiItv = "E2E-API-ITV";

            void CurataApiItv(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>()
                    .Where(r => r.Cod.StartsWith(MarcajApiItv)).Select(r => r.ID).ToList();
                // Laturile ITV sunt unitatea marcată (predator = primitor), deci
                // filtrul pe laturi prinde și închiderile, nu doar FCT/FCL/NTC.
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                var docIds = docs.Select(d => d.ID).ToList();
                pj.Adauga(os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruTva>()
                    .Where(r => docIds.Contains(r.DocumentId)).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                    .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                    .Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                    .Where(d => docIds.Contains(d.DocumentId)).ToList());
                foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                    pj.Adauga(doc);
                pj.Adauga(os.GetObjectsQuery<Repartitor>()
                    .Where(r => r.Cod.StartsWith(MarcajApiItv)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataApiItv(os);

                Cont ContApiItv(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
                var cont4423 = ContApiItv("4423");
                var cont4424 = ContApiItv("4424");
                var cont4426 = ContApiItv("4426");
                var cont4427 = ContApiItv("4427");
                var contTranzit = ContApiItv("581");
                var contCasa = ContApiItv("5311");
                var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
                var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
                var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
                var n21 = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");

                decimal SoldDebitorApiCub(Cont cont, DateOnly la) => CubScena.SoldCont(os, cont.ID, la);
                decimal SoldCreditorApiCub(Cont cont, DateOnly la) => -SoldDebitorApiCub(cont, la);

                var finalNoi = new DateOnly(2026, 11, 30);
                var finalDec = new DateOnly(2026, 12, 31);
                s.Check("F21-D9 precondiție — BLOCUL ĂSTA TREBUIE SĂ RĂMÂNĂ ÎNAINTEA SCENELOR `E2E-ASM` (noiembrie) "
                    + "și `E2E-RET` RLF/RDC (decembrie), care lucrează în ACELEAȘI luni: dacă proba de față pică, "
                    + "cineva a mutat blocul după ele (sau a adăugat documente în 11/12 mai devreme în fișier), "
                    + "iar cifrele exacte de mai jos și cei TREI gardieni cronologici ar fi măsurați peste conținut "
                    + "străin. MĂSURAT aici: NICIUN document, de ORICE tip, datat în 11–12/2026 (clauza asta prinde "
                    + "și mutarea după `E2E-ASM`, care are ZERO contare și deci n-ar fi mișcat soldurile — soldurile "
                    + "singure măsurau doar `E2E-RET`), soldurile cumulate 4426/4427 sunt 0 la 30.11 ȘI la "
                    + "31.12.2026, iar baza n-are NICIO închidere de TVA vie",
                    !os.GetObjectsQuery<Document>().Any(d => d.Data >= new DateOnly(2026, 11, 1) && d.Data <= finalDec)
                    && SoldDebitorApiCub(cont4426, finalNoi) == 0m && SoldCreditorApiCub(cont4427, finalNoi) == 0m
                    && SoldDebitorApiCub(cont4426, finalDec) == 0m && SoldCreditorApiCub(cont4427, finalDec) == 0m
                    && !os.GetObjectsQuery<InchidereTva>().Any(d => d.Stare != StareDocument.Stornat));

                var furnizor = os.CreateObject<Partener>();
                furnizor.Cod = MarcajApiItv + "-FURN";
                furnizor.Denumire = "Furnizor probă felie ITV";
                var client = os.CreateObject<Partener>();
                client.Cod = MarcajApiItv + "-CL";
                client.Denumire = "Client probă felie ITV";
                var gestiune = os.CreateObject<Gestiune>();
                gestiune.Cod = MarcajApiItv + "-MAG";
                gestiune.Denumire = "Gestiune probă felie ITV";
                var unitate = os.CreateObject<UnitateInterna>();
                unitate.Cod = MarcajApiItv + "-UI";
                unitate.Denumire = "Unitate probă felie ITV";
                os.CommitChanges();

                // ── (1) Previzualizarea pe o lună fără TVA: `FaraSold`, cu cifra ──
                var prevGol = InchidereTvaApply.Previzualizeaza(os, 2026, 11);
                s.Check("F21-D9.1 — `Previzualizeaza` pe o lună fără TVA: motivul e `FaraSold`, iar soldurile ies 0/0 "
                    + "NENULE ca valori (nu `null`: profilul ARE conturi, doar luna n-are ce închide — distincția "
                    + "față de `ProfilInert` e chiar ce trebuie să spună ecranul)",
                    prevGol.Motiv == nameof(MotivNegenerare.FaraSold)
                    && prevGol.Sold4426 == 0m && prevGol.Sold4427 == 0m
                    && prevGol.Transfer == 0m && prevGol.DePlata == 0m && prevGol.DeRecuperat == 0m
                    && prevGol.InchidereVieId == null);

                // TVA-ul lunii: FCT servicii 100 (4426 = 21) + FCL 200 (4427 = 42).
                var fctNoi = os.CreateObject<FacturaIntrare>();
                fctNoi.Numar = MarcajApiItv + "-FF1";
                fctNoi.Data = new DateOnly(2026, 11, 3);
                fctNoi.Predator = furnizor;
                fctNoi.Primitor = gestiune;
                var linieFctNoi = os.CreateObject<FacturaIntrareDetaliu>();
                linieFctNoi.Document = fctNoi;
                linieFctNoi.TipMaterial = tip628;
                linieFctNoi.Cantitate = 1m;
                linieFctNoi.PretUnitar = 100m;
                linieFctNoi.TipTva = n21;

                var fclNoi = os.CreateObject<FacturaIesire>();
                fclNoi.Data = new DateOnly(2026, 11, 4);
                fclNoi.Predator = unitate;
                fclNoi.Primitor = client;
                var linieFclNoi = os.CreateObject<FacturaIesireDetaliu>();
                linieFclNoi.Document = fclNoi;
                linieFclNoi.TipMaterial = tip704;
                linieFclNoi.Cantitate = 1m;
                linieFclNoi.PretUnitar = 200m;
                linieFclNoi.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctNoi);
                MotorOperare.Opereaza(os, fclNoi);

                var prevNoi = InchidereTvaApply.Previzualizeaza(os, 2026, 11);
                var liniiAsteptate = InchidereTvaService.CalculeazaLinii(21m, 42m);
                s.Check("F21-D9.1 — după FCT+FCL în lună: `Motiv == null` (luna se poate închide), soldurile 21/42 și "
                    + "cele trei linii IDENTICE cu `CalculeazaLinii(solduri)` — previzualizarea nu e o copie a "
                    + "calculului, e ACELAȘI calcul (42c). Anti-vacuitate: transferul și de-plata sunt NENULE, "
                    + "iar de-recuperat e 0 (egalitatea ar fi trecut și pe zero de ambele părți)",
                    prevNoi.Motiv == null && prevNoi.Sold4426 == 21m && prevNoi.Sold4427 == 42m
                    && prevNoi.Transfer == liniiAsteptate.Transfer
                    && prevNoi.DePlata == liniiAsteptate.DePlata
                    && prevNoi.DeRecuperat == liniiAsteptate.DeRecuperat
                    && prevNoi.Transfer == 21m && prevNoi.DePlata == 21m && prevNoi.DeRecuperat == 0m);

                // ── (2) Generarea: draftul are EXACT liniile previzualizate ──
                var rezGen = InchidereTvaApply.Genereaza(os,
                    new GenerareItvRequestDto { An = 2026, Luna = 11, UnitateId = unitate.ID });
                var idItv = rezGen.DocumentId ?? Guid.Empty;
                var citit = InchidereTvaApply.Citeste(os, idItv);
                // Commit-ul e al Apply-ului (F21-D7): se probează dintr-un OS NOU,
                // fiindcă în cel curent draftul ar fi vizibil și necomis.
                bool persistat;
                using (var osAlt = s.Provider.CreateObjectSpace())
                    persistat = osAlt.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItv);
                s.Check("F21-D9.2 — `Genereaza` ⇒ document COMIS (vizibil dintr-un alt ObjectSpace), fără motiv, iar "
                    + "liniile draftului sunt EXACT cele previzualizate: cusătura previzualizare ↔ generare "
                    + "(rolurile citite prin conturile POLITICII, nu prin simbol — 29)",
                    rezGen.DocumentId != null && rezGen.Motiv == null && persistat
                    && rezGen.Transfer == prevNoi.Transfer && rezGen.DePlata == prevNoi.DePlata
                    && rezGen.DeRecuperat == prevNoi.DeRecuperat
                    && citit != null && citit.Linii.Count == 2
                    && citit.Transfer == 21m && citit.DePlata == 21m && citit.DeRecuperat == 0m
                    && citit.Linii.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4426.ID && l.Valoare == 21m)
                    && citit.Linii.Any(l => l.ContDebitId == cont4427.ID && l.ContCreditId == cont4423.ID && l.Valoare == 21m)
                    && citit.An == 2026 && citit.Luna == 11 && citit.Data == finalNoi
                    && citit.UnitateId == unitate.ID && citit.Stare == "Draft" && citit.Numar == null);

                // ── (3) A doua generare, cronologia ca refuz ȘI ca motiv ──
                var rezGen2 = InchidereTvaApply.Genereaza(os,
                    new GenerareItvRequestDto { An = 2026, Luna = 11, UnitateId = unitate.ID });
                s.Check("F21-D9.3 — a doua generare pe aceeași lună ⇒ RAPORT, nu eroare: `DocumentId == null`, motivul "
                    + "`InchidereVie` și `InchidereVieId` = chiar draftul existent (fără el ecranul n-ar putea face link)",
                    rezGen2.DocumentId == null && rezGen2.Motiv == nameof(MotivNegenerare.InchidereVie)
                    && rezGen2.InchidereVieId == idItv
                    && rezGen2.Sold4426 == 21m && rezGen2.Sold4427 == 42m);
                s.CheckRefuza("F21-D9.3 — `Incearca` pe o lună ANTERIOARĂ unei închideri vii ⇒ refuz ZGOMOTOS "
                    + "(comanda nu raportează cronologia, o refuză — 46c)",
                    () => InchidereTvaService.Incearca(os, 2026, 10, unitate.ID));
                var prevInapoi = InchidereTvaApply.Previzualizeaza(os, 2026, 10);
                s.Check("F21-D9.3 — ACEEAȘI cronologie, pe ușa de RAPORT, iese ca MOTIV `NeCronologica` cu documentul "
                    + "care blochează (id + etichetă + stare): raportul răspunde „de ce nu se poate”, nu aruncă (F21-D2b)",
                    prevInapoi.Motiv == nameof(MotivNegenerare.NeCronologica)
                    && prevInapoi.InchidereVieId == idItv
                    && prevInapoi.InchidereVieStare == "Draft"
                    && prevInapoi.InchidereVieNumar == finalNoi.ToString("dd.MM.yyyy"));

                // ── (3b) Simetricul cronologiei la GENERARE: draft anterior neoperat ──
                // Review 79 F1, gardianul 2c. Fără el se putea genera DECEMBRIE cât
                // noiembrie era încă Draft: soldurile cumulate la 31.12 le conțin pe
                // ale lui noiembrie, deci decembrie le-ar fi închis o dată, iar
                // operarea de mai târziu a draftului de noiembrie încă o dată
                // (4423/4424 dublate). Ușile diferă printr-un singur bit, ca la 2b:
                // raportul spune motivul, comanda refuză.
                var prevDec = InchidereTvaApply.Previzualizeaza(os, 2026, 12);
                s.Check("F21-D9.3b (79 F1) — cu un draft NEOPERAT pe noiembrie, previzualizarea lui DECEMBRIE dă "
                    + "`DraftAnterior` cu draftul care blochează (id + etichetă + stare), nu `Motiv == null`. "
                    + "Anti-vacuitate: soldurile lunii ies TOTUȘI (21/42 — cumulatul, care le conține pe ale lui "
                    + "noiembrie: exact cifra care s-ar fi închis a doua oară), deci verdictul nu e o consecință a "
                    + "lipsei de sold",
                    prevDec.Motiv == nameof(MotivNegenerare.DraftAnterior)
                    && prevDec.InchidereVieId == idItv
                    && prevDec.InchidereVieStare == "Draft"
                    && prevDec.InchidereVieNumar == finalNoi.ToString("dd.MM.yyyy")
                    && prevDec.Sold4426 == 21m && prevDec.Sold4427 == 42m);
                s.CheckRefuza("F21-D9.3b (79 F1) — ACELAȘI gardian, la COMANDĂ, refuză zgomotos: `Incearca` pe "
                    + "decembrie cât noiembrie e Draft ⇒ excepție de domeniu, nu un draft al lunii următoare",
                    () => InchidereTvaService.Incearca(os, 2026, 12, unitate.ID));
                s.Check("F21-D9.3b — refuzul n-a lăsat nimic în urmă: nicio închidere pe decembrie",
                    !os.GetObjectsQuery<InchidereTva>()
                        .Any(d => d.Data >= new DateOnly(2026, 12, 1)));

                // ── (4) `Stale`: DTO-ul și gardianul spun același lucru; regenerarea ──
                // `Stale == false` e o AFIRMAȚIE despre ce face operarea, deci se
                // măsoară contra dry-run-ului, nu doar contra unei formule (79 M4:
                // criteriul din DTO era mai STRICT decât al gardianului și diverge în
                // ambele sensuri). Sensul celălalt (`true` ⇔ eroarea apare) e mai jos.
                List<string> eroriDryProaspat;
                using (var osDry = s.Provider.CreateObjectSpace())
                    eroriDryProaspat = ComenziDocument.Sistem(osDry).Valideaza(idItv).ToList();
                s.Check("F21-D9.4 — pe draftul proaspăt: `Stale == false` ȘI dry-run-ul motorului NU conține eroarea "
                    + "anti-stale (criteriul din DTO e chiar cel al gardianului, `LiniiPotrivescSoldurile` — 79 M4), "
                    + "soldurile CURENTE din DTO sunt cele ale motorului (21/42) și afordanțele sunt cele de Draft",
                    citit.Stale == false && citit.Sold4426Curent == 21m && citit.Sold4427Curent == 42m
                    && !eroriDryProaspat.Any(e => e.Contains("s-au schimbat de la generare"))
                    && citit.PoateOpera && citit.PoateSterge && citit.PoateRegenera
                    && !citit.PoateAnula && !citit.PoateStorna);

                // Încă o factură în lună, DUPĂ generare: soldurile nu mai sunt cele
                // din linii (fix-ul anti-stale de la 1C-a, văzut acum prin ușă).
                var fctNoi2 = os.CreateObject<FacturaIntrare>();
                fctNoi2.Numar = MarcajApiItv + "-FF2";
                fctNoi2.Data = new DateOnly(2026, 11, 20);
                fctNoi2.Predator = furnizor;
                fctNoi2.Primitor = gestiune;
                var linieFctNoi2 = os.CreateObject<FacturaIntrareDetaliu>();
                linieFctNoi2.Document = fctNoi2;
                linieFctNoi2.TipMaterial = tip628;
                linieFctNoi2.Cantitate = 1m;
                linieFctNoi2.PretUnitar = 50m;
                linieFctNoi2.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctNoi2);

                var citStale = InchidereTvaApply.Citeste(os, idItv);
                // Dry-run pe un OS de UNICĂ FOLOSINȚĂ: `Valideaza` SCRIE (25b/55b).
                List<string> eroriDry;
                using (var osDry = s.Provider.CreateObjectSpace())
                    eroriDry = ComenziDocument.Sistem(osDry).Valideaza(idItv).ToList();
                s.Check("F21-D9.4 — după încă o factură în lună: `Stale == true` ÎN DTO **și** dry-run-ul dă exact "
                    + "eroarea gardianului anti-stale — ecranul spune același lucru pe care operarea îl refuză "
                    + "(dacă cele două ar diverge, butonul „Operează” ar promite ce serverul neagă)",
                    citStale.Stale == true
                    && citStale.Sold4426Curent == 31.5m && citStale.Sold4427Curent == 42m
                    && eroriDry.Any(e => e.Contains("s-au schimbat de la generare")));

                var rezRegen = InchidereTvaApply.Regenereaza(os, idItv);
                var idItvNou = rezRegen.DocumentId ?? Guid.Empty;
                var citRegen = InchidereTvaApply.Citeste(os, idItvNou);
                s.Check("F21-D9.4 — `Regenereaza` ⇒ draft NOU (alt id) pe cifra curentă, `Stale == false`. Proba e chiar "
                    + "DETECTORUL cusăturii F21-D11, azi pe alt mecanism (79 F2): generarea se face ÎNAINTEA "
                    + "ștergerii, în ACEEAȘI tranzacție, iar draftul de față e scos din interogarea de idempotență "
                    + "prin `Incearca(…, inlocuieste: id)` — dacă parametrul n-ar ajunge acolo, generarea ar întoarce "
                    + "`InchidereVie` și `DocumentId` ar fi null",
                    rezRegen.DocumentId != null && rezRegen.Motiv == null && idItvNou != idItv
                    && citRegen != null && citRegen.Stale == false
                    && citRegen.Transfer == 31.5m && citRegen.DePlata == 10.5m && citRegen.DeRecuperat == 0m
                    && citRegen.Linii.Count == 2);
                s.Check("F21-D9.4 — draftul vechi e șters FIZIC (104f), iar `Citeste` pe el întoarce null",
                    !os.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItv)
                    && InchidereTvaApply.Citeste(os, idItv) == null);

                // ── (4b) Politică incompletă ⇒ `Stale` n-are VERDICT (79 M4) ──
                // Sensul invers al lui M4: dacă politica pierde un cont DUPĂ generare,
                // gardianul refuză din PRIMA ramură („cere politica … completă"), deci
                // un `Stale == false` acolo ar fi fost un „draftul e bun" despre un
                // draft neoperabil. `null` = „nu se poate spune", nu „e în regulă".
                var politicaItv = os.FirstOrDefault<PoliticaInchidereTva>(
                    p => p.TipDocument.ClrType == nameof(InchidereTva));
                var contDePlataSalvat = politicaItv.ContDePlataId;
                try {
                    politicaItv.ContDePlataId = null;
                    os.CommitChanges();
                    var citFaraPolitica = InchidereTvaApply.Citeste(os, idItvNou);
                    List<string> eroriFaraPolitica;
                    using (var osDry = s.Provider.CreateObjectSpace())
                        eroriFaraPolitica = ComenziDocument.Sistem(osDry).Valideaza(idItvNou).ToList();
                    s.Check("F21-D9.4b (79 M4) — cu `PoliticaInchidereTva` incompletă (contul de plată golit DUPĂ "
                        + "generare): `Stale == null` în DTO — nu `false` —, iar dry-run-ul refuză cu prima ramură a "
                        + "gardianului („cere politica … completă”). Anti-vacuitate: documentul e tot Draft, deci "
                        + "`PoateOpera` rămâne `true` — afordanța e a STĂRII, verdictul e al motorului",
                        citFaraPolitica != null && citFaraPolitica.Stale == null && citFaraPolitica.PoateOpera
                        && eroriFaraPolitica.Any(e => e.Contains("politica de conturi")));
                }
                finally {
                    politicaItv.ContDePlataId = contDePlataSalvat;
                    os.CommitChanges();
                }
                s.Check("F21-D9.4b — politica restaurată: `Stale` redevine `false` (verdictul era al politicii, nu al "
                    + "draftului — restul blocului măsoară pe scena întreagă)",
                    InchidereTvaApply.Citeste(os, idItvNou).Stale == false);

                // ── (5) Operarea, storno-ul și redeschiderea lunii ──
                var rezOperare = ComenziDocument.Sistem(os).Opereaza(idItvNou);
                var citOperat = InchidereTvaApply.Citeste(os, idItvNou);
                s.Check("F21-D9.5 — `ComenziDocument.Opereaza` ⇒ Operat cu numărul din politică (seria „ITV-”, consumată la "
                    + "MATERIALIZARE — 53b), iar `Stale` devine `null`: pe un document operat cifra e deja în "
                    + "registru, deci întrebarea n-are sens (un `false` acolo ar fi fost o afirmație despre altceva)",
                    rezOperare.StareNoua == StareDocument.Operat
                    && citOperat.Stare == "Operat" && citOperat.Numar?.StartsWith("ITV-") == true
                    && citOperat.DataOperare != null && citOperat.Stale == null
                    && !citOperat.PoateOpera && !citOperat.PoateSterge && !citOperat.PoateRegenera
                    && citOperat.PoateAnula && citOperat.PoateStorna);
                s.Check("F21-D9.5 — după operare luna e închisă în REGISTRU: 4423 TVA de plată = 10,5, iar 4426/4427 "
                    + "revin la 0 pe 30.11",
                    SoldCreditorApiCub(cont4423, finalNoi) == 10.5m
                    && SoldDebitorApiCub(cont4426, finalNoi) == 0m && SoldCreditorApiCub(cont4427, finalNoi) == 0m);

                var prevDecDupaOperare = InchidereTvaApply.Previzualizeaza(os, 2026, 12);
                s.Check("F21-D9.3b (79 F1) — după OPERAREA lui noiembrie, decembrie nu mai spune `DraftAnterior`: "
                    + "verdictul era al draftului neoperat, nu ceva permanent al lunii (aici motivul devine "
                    + "`FaraSold` — noiembrie tocmai a dus 4426/4427 la 0, iar decembrie n-are TVA propriu)",
                    prevDecDupaOperare.Motiv == nameof(MotivNegenerare.FaraSold)
                    && prevDecDupaOperare.Sold4426 == 0m && prevDecDupaOperare.Sold4427 == 0m);

                // ── (5b) Cronologia la OPERARE (79 F1): perechea gardienilor 2b/2c ──
                // Cazul nu se mai poate construi prin generare (2b și 2c îl opresc în
                // ambele sensuri), dar rămâne ATINS prin ANULAREA operării: noiembrie
                // redevine Draft cât decembrie e deja Operat. Fără gardul din
                // `InchidereTva.ValideazaOperare` s-ar fi operat, închizând a doua oară
                // aceleași solduri (4423 dublat, reziduu negativ pe 4426/4427 mascat
                // de `Max(0, …)` din `Solduri` — defectul ar fi devenit invizibil pe
                // toate ușile feliei).
                var fctDec = os.CreateObject<FacturaIntrare>();
                fctDec.Numar = MarcajApiItv + "-FF-DEC";
                fctDec.Data = new DateOnly(2026, 12, 5);
                fctDec.Predator = furnizor;
                fctDec.Primitor = gestiune;
                var linieFctDec = os.CreateObject<FacturaIntrareDetaliu>();
                linieFctDec.Document = fctDec;
                linieFctDec.TipMaterial = tip628;
                linieFctDec.Cantitate = 1m;
                linieFctDec.PretUnitar = 100m;
                linieFctDec.TipTva = n21;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fctDec);

                var rezDec = InchidereTvaApply.Genereaza(os,
                    new GenerareItvRequestDto { An = 2026, Luna = 12, UnitateId = unitate.ID });
                var idItvDec = rezDec.DocumentId ?? Guid.Empty;
                ComenziDocument.Sistem(os).Opereaza(idItvDec);
                ComenziDocument.Sistem(os).AnuleazaOperarea(idItvNou);
                var citNovRedeschis = InchidereTvaApply.Citeste(os, idItvNou);
                List<string> eroriCronologie;
                using (var osDry = s.Provider.CreateObjectSpace())
                    eroriCronologie = ComenziDocument.Sistem(osDry).Valideaza(idItvNou).ToList();
                s.Check("F21-D9.5b (79 F1) — noiembrie Draft (prin anularea operării) cât decembrie e OPERAT: DTO-ul "
                    + "spune `PoateOpera == true` și `Stale == false` (afordanța e a STĂRII, iar liniile chiar se "
                    + "potrivesc cu soldurile revenite), dar dry-run-ul motorului dă exact eroarea cronologică — "
                    + "ecranul are „Verifică” tocmai pentru asta. Anti-vacuitate: eroarea anti-stale NU e printre "
                    + "ele, deci refuzul vine din gardul cronologic, nu din altă cauză",
                    citNovRedeschis.Stare == "Draft" && citNovRedeschis.PoateOpera && citNovRedeschis.Stale == false
                    && eroriCronologie.Any(e => e.Contains("operată pentru o lună ulterioară"))
                    && !eroriCronologie.Any(e => e.Contains("s-au schimbat de la generare")));
                s.CheckRefuza("F21-D9.5b (79 F1) — ACELAȘI gard pe ușa care SCRIE: `ComenziDocument.Opereaza` pe draftul de "
                    + "noiembrie ⇒ refuz (gardul stă în `ValideazaOperare`, deci apără toate ușile — XAF, API, consolă)",
                    () => ComenziDocument.Sistem(os).Opereaza(idItvNou));
                s.Check("F21-D9.5b — refuzul n-a lăsat rânduri-fantomă (33d) și n-a dublat 4423: la 31.12 soldul de "
                    + "plată e cel al UNEI singure închideri operate (decembrie, 0 — luna a ieșit pe recuperat), "
                    + "iar noiembrie n-a mai postat nimic",
                    CubScena.FaraNote(os, idItvNou)
                    && InchidereTvaApply.Citeste(os, idItvNou).Stare == "Draft");

                // ── (5c) `Regenereaza` refuzat ⇒ draftul rămâne INTACT (79 F2) ──
                // Prima formă ștergea și COMITEA înaintea generării: pe exact scena de
                // față (`Incearca` aruncă din cronologie) draftul dispărea definitiv,
                // pe o acțiune al cărei text promite că îl reface.
                var liniiInainteDeRefuz = InchidereTvaApply.Citeste(os, idItvNou).Linii
                    .Select(l => $"{l.Id}|{l.ContDebitId}|{l.ContCreditId}|{l.Valoare}").ToList();
                s.CheckRefuza("F21-D9.5c (79 F2) — `Regenereaza` pe un draft pe care gardienii îl refuză (aici: "
                    + "decembrie e închis, deci noiembrie nu se mai poate genera) ⇒ refuz de domeniu",
                    () => InchidereTvaApply.Regenereaza(os, idItvNou));
                var citDupaRefuz = InchidereTvaApply.Citeste(os, idItvNou);
                s.Check("F21-D9.5c (79 F2) — după refuz draftul e INTACT: există în interogări (deci nici măcar șters "
                    + "logic), e tot Draft, cu ACELEAȘI linii — id, conturi și valori — ca înainte de încercare. "
                    + "Refuzul vine ÎNAINTEA oricărui `Delete`, într-o singură tranzacție",
                    citDupaRefuz != null && citDupaRefuz.Stare == "Draft"
                    && os.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItvNou)
                    && citDupaRefuz.Linii.Count == 2
                    && citDupaRefuz.Linii.Select(l => $"{l.Id}|{l.ContDebitId}|{l.ContCreditId}|{l.Valoare}")
                        .SequenceEqual(liniiInainteDeRefuz));

                // Scena revine la starea pe care o cere restul blocului: decembrie
                // dispare (anulare + ștergere pe ușa proprie), noiembrie se re-operează.
                ComenziDocument.Sistem(os).AnuleazaOperarea(idItvDec);
                InchidereTvaApply.Sterge(os, idItvDec);
                ComenziDocument.Sistem(os).Opereaza(idItvNou);
                s.Check("F21-D9.5b/c — scena restaurată: închiderea lui decembrie a dispărut, noiembrie e din nou "
                    + "Operat (cu ACELAȘI număr — re-operarea nu consumă seria a doua oară, 53b) și luna e iar "
                    + "închisă în registru",
                    !os.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItvDec)
                    && InchidereTvaApply.Citeste(os, idItvNou).Stare == "Operat"
                    && InchidereTvaApply.Citeste(os, idItvNou).Numar == citOperat.Numar
                    && SoldDebitorApiCub(cont4426, finalNoi) == 0m && SoldCreditorApiCub(cont4427, finalNoi) == 0m);

                ComenziDocument.Sistem(os).Storneaza(idItvNou, finalNoi);
                var prevDupaStorno = InchidereTvaApply.Previzualizeaza(os, 2026, 11);
                s.Check("F21-D9.5 — storno la CHIAR data închiderii ⇒ luna redevine închiderabilă: previzualizarea dă "
                    + "din nou `Motiv == null` pe soldurile revenite (disciplina 46f, singura ei consecință pentru "
                    + "operator — indiciul de pe ecran)",
                    InchidereTvaApply.Citeste(os, idItvNou).Stare == "Stornat"
                    && prevDupaStorno.Motiv == null
                    && prevDupaStorno.Sold4426 == 31.5m && prevDupaStorno.Sold4427 == 42m
                    && prevDupaStorno.Transfer == 31.5m && prevDupaStorno.DePlata == 10.5m
                    && prevDupaStorno.DeRecuperat == 0m);

                // ── (5d) Perioada fiscală închisă (79 M2), gardianul 2d ──
                // Fără el previzualizarea spunea „se poate", generarea reușea, și abia
                // operarea refuza — iar draftul creat între timp devenea „închidere
                // vie", blocând cronologic toate lunile dinaintea lui. Același gardian
                // ca la operare (`GardianPerioada`), adus la GENERARE: la raport
                // motivul, la comandă refuzul.
                // F27-D1: perioada se închide prin COMANDA motorului, iar lanțul cere
                // toate lunile anterioare închise — 12/2026 nu e capăt de lanț.
                var luni2026 = Enumerable.Range(1, 12).ToArray();
                try {
                    s.InchideLant(os, 2026, luni2026);
                    var prevPerioadaInchisa = InchidereTvaApply.Previzualizeaza(os, 2026, 12);
                    s.Check("F21-D9.5d (79 M2) — cu perioada 12/2026 ÎNCHISĂ, previzualizarea lunii dă "
                        + "`PerioadaInchisa`, nu `Motiv == null`. Anti-vacuitate: soldurile ies TOTUȘI (52,5/42 — "
                        + "luna ARE ce închide, deci motivul nu e `FaraSold` deghizat), iar `InchidereVieId` e null "
                        + "(nu blochează un document, ci o perioadă)",
                        prevPerioadaInchisa.Motiv == nameof(MotivNegenerare.PerioadaInchisa)
                        && prevPerioadaInchisa.Sold4426 == 52.5m && prevPerioadaInchisa.Sold4427 == 42m
                        && prevPerioadaInchisa.InchidereVieId == null);
                    s.CheckRefuza("F21-D9.5d (79 M2) — la COMANDĂ același gardian refuză zgomotos, cu mesajul lui "
                        + "`GardianPerioada` — și nu se mai naște draftul care ar fi blocat cronologic lunile "
                        + "anterioare",
                        () => InchidereTvaService.Incearca(os, 2026, 12, unitate.ID));
                    s.Check("F21-D9.5d — refuzul n-a lăsat nicio închidere vie pe decembrie",
                        !os.GetObjectsQuery<InchidereTva>().Any(d => d.Data >= new DateOnly(2026, 12, 1)));
                }
                finally {
                    s.RedeschideLant(os, 2026, luni2026);
                    s.PurjaIstoricPerioade(os, 2026);
                }
                s.Check("F21-D9.5d — după REDESCHIDEREA perioadei motivul dispare (verdictul era al perioadei, nu al "
                    + "lunii): decembrie redevine generabil pe aceleași solduri",
                    InchidereTvaApply.Previzualizeaza(os, 2026, 12).Motiv == null);

                // ── (6) F21-D5: ITV a ieșit din felia NTC, pe toate cele patru uși ──
                // Nota de control (decembrie, conturi de trezorerie — nu atinge TVA-ul
                // lunii probate) există ca proba să NU fie vacuă: lista ITV trebuie să
                // conțină DOAR închideri, iar lista NTC să conțină nota și NU închiderea.
                var idNtcControl = NotaContabilaApply.Aplica(os, null, new NtcWriteDto {
                    Data = new DateOnly(2026, 12, 9),
                    PredatorId = unitate.ID,
                    PrimitorId = unitate.ID,
                    Linii = [new NtcLinieWriteDto {
                        TipMaterialId = tipTrz.ID, Descriere = MarcajApiItv + " notă de control",
                        ContDebitId = contTranzit.ID, ContCreditId = contCasa.ID, Valoare = 7m
                    }]
                });
                var rezGenFinal = InchidereTvaApply.Genereaza(os,
                    new GenerareItvRequestDto { An = 2026, Luna = 11, UnitateId = unitate.ID });
                var idItvFinal = rezGenFinal.DocumentId ?? Guid.Empty;

                var idsNtc = NotaContabilaApply.Lista(os).Select(x => x.Id).ToList();
                var idsItv = InchidereTvaApply.Lista(os).Select(x => x.Id).ToList();
                var sqlNtc = NotaContabilaApply.Lista(os).ToQueryString();
                Console.WriteLine("     MĂSURAT (F21-D5/filtrul NTC): " + string.Join(" | ", sqlNtc.Split('\n')
                    .Where(l => l.Contains("ClrType", StringComparison.Ordinal)).Select(l => l.Trim())));
                s.Check("F21-D5 — `NotaContabilaApply.Lista` NU mai conține închiderile de TVA, dar conține nota de "
                    + "control (anti-vacuitate: un filtru care ar fi golit lista ar fi trecut și el). Filtrul e "
                    + "SERVER-SIDE: `is` ajunge în SQL ca filtru pe discriminatorul `ClrType` cu valoarea `InchidereTva`, "
                    + "nu în memorie",
                    !idsNtc.Contains(idItvFinal) && idsNtc.Contains(idNtcControl)
                    && sqlNtc.Contains("\"ClrType\"") && sqlNtc.Contains("'InchidereTva'"));
                s.Check("F21-D5 — `InchidereTvaApply.Lista` conține DOAR închideri: nota de control lipsește, "
                    + "închiderea e acolo cu An/Luna DERIVATE din `Data` în SQL, iar fiecare rând e chiar o "
                    + "`InchidereTva`",
                    !idsItv.Contains(idNtcControl) && idsItv.Contains(idItvFinal)
                    && InchidereTvaApply.Lista(os).Where(x => x.Id == idItvFinal)
                        .All(x => x.An == 2026 && x.Luna == 11 && x.Stare == "Draft"
                            && x.UnitateDenumire == unitate.Denumire)
                    && os.GetObjectsQuery<InchidereTva>().Count(d => idsItv.Contains(d.ID)) == idsItv.Count);
                s.Check("F21-D5 — `NotaContabilaApply.Citeste` pe un id de închidere întoarce null (⇒ 404 pe "
                    + "`GET api/ntc/{id}`), deși documentul EXISTĂ ca notă (`InchidereTva : NotaContabila`)",
                    NotaContabilaApply.Citeste(os, idItvFinal) == null
                    && os.GetObjectsQuery<NotaContabila>().Any(d => d.ID == idItvFinal));
                s.Check("F21-D5 — `NotaContabilaApply.Candidati` pe un id de închidere întoarce null (⇒ 404 pe "
                    + "`GET api/ntc/{id}/candidati`), dar rămâne un panou pentru nota ADEVĂRATĂ",
                    NotaContabilaApply.Candidati(os, idItvFinal) == null
                    && NotaContabilaApply.Candidati(os, idNtcControl) != null);
                s.CheckRefuza("F21-D5 — `PUT api/ntc/{id}` pe o închidere DRAFT (`Aplica`) ⇒ refuz de domeniu: până "
                    + "acum reconcilierea accepta orice linii, iar gardianul anti-stale o prindea abia la OPERARE",
                    () => NotaContabilaApply.Aplica(os, idItvFinal, new NtcWriteDto {
                        Data = finalNoi, PredatorId = unitate.ID, PrimitorId = unitate.ID, Linii = []
                    }));
                s.CheckRefuza("F21-D5 — `DELETE api/ntc/{id}` pe o închidere (`Sterge`) ⇒ refuz de domeniu "
                    + "(ștergerea ei trece prin ecranul propriu)",
                    () => NotaContabilaApply.Sterge(os, idItvFinal));
                s.Check("F21-D5 — refuzul e neschimbat pentru o notă ADEVĂRATĂ: `Citeste`/`Sterge` merg mai departe "
                    + "pe nota de control (gardul e pe TIP, nu pe felie)",
                    NotaContabilaApply.Citeste(os, idNtcControl) != null
                    && s.Refuz(() => NotaContabilaApply.Sterge(os, idNtcControl)) == null);

                // ── Ștergerea închiderii pe ușa PROPRIE (F21-D7) ──
                InchidereTvaApply.Sterge(os, idItvFinal);
                s.Check("F21-D7 — `InchidereTvaApply.Sterge` pe Draft: documentul și liniile lui ies din interogări, "
                    + "iar luna redevine închiderabilă",
                    !os.GetObjectsQuery<InchidereTva>().Any(d => d.ID == idItvFinal)
                    && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idItvFinal)
                    && InchidereTvaApply.Previzualizeaza(os, 2026, 11).Motiv == null);
                // Cele două uși se probează SEPARAT (79 M8): o singură probă cu două
                // nume în titlu exercita doar una din ele.
                s.CheckRefuza("F21-D7 — `Regenereaza` pe un document care nu mai e Draft (cel STORNAT) ⇒ refuz de "
                    + "domeniu: „regenerarea” unui document operat ar fi însemnat ștergerea unor rânduri de "
                    + "registru (14)",
                    () => InchidereTvaApply.Regenereaza(os, idItvNou));
                s.CheckRefuza("F21-D7 — `Sterge` pe același document STORNAT ⇒ refuz de domeniu, pe ușa lui proprie "
                    + "(`DELETE api/itv/{id}`), nu doar pe cea de regenerare",
                    () => InchidereTvaApply.Sterge(os, idItvNou));
                s.Check("F21-D7 — cele două refuzuri n-au atins documentul: stornatul e tot acolo, tot Stornat",
                    InchidereTvaApply.Citeste(os, idItvNou)?.Stare == "Stornat");

                // ── (7) Unitatea ne-internă (F21-D2f) ──
                s.CheckRefuza("F21-D9.7 — `Incearca` cu o unitate care NU e `UnitateInterna` (un Partener) ⇒ refuz la "
                    + "GENERARE, nu abia la operare: oglinda gardului din `NotaContabila.ValideazaOperare`, adusă "
                    + "acolo unde cererea HTTP o poate greși",
                    () => InchidereTvaService.Incearca(os, 2026, 11, furnizor.ID));
                s.CheckRefuza("F21-D9.7 — același refuz pentru un id care nu e repartitor deloc (mesajul numește "
                    + "id-ul, nu se preface că e o unitate)",
                    () => InchidereTvaService.Incearca(os, 2026, 11, Guid.NewGuid()));
                s.CheckRefuza("F21-D9.7 (79 M8) — gardul e probat și pe UȘA APLICAȚIEI, nu doar pe serviciu: "
                    + "`InchidereTvaApply.Genereaza` cu `UnitateId` de Partener ⇒ refuz de domeniu (⇒ 422 pe "
                    + "`POST api/itv/genereaza`) — ăsta e drumul pe care îl parcurge cererea HTTP",
                    () => InchidereTvaApply.Genereaza(os,
                        new GenerareItvRequestDto { An = 2026, Luna = 11, UnitateId = furnizor.ID }));
                s.Check("F21-D9.7 — refuzurile de unitate n-au scris nimic: nicio închidere VIE pe noiembrie (cea "
                    + "stornată rămâne, e istoric)",
                    !os.GetObjectsQuery<InchidereTva>().Any(d => d.Data >= new DateOnly(2026, 11, 1)
                        && d.Stare != StareDocument.Stornat));

                // ── (9) Curățenia ──
                CurataApiItv(os);
                // Curățenia lasă ZERO închideri pe 11/12 — inclusiv cea STORNATĂ și
                // regenerata ei. O închidere vie rămasă aici ar fi aprins gardul
                // cronologic al scenelor de mai jos: premisa D300 („nicio închidere vie
                // ulterioară lunii scenei") și proba condiționată SAF-T ar fi picat pe
                // conținut STRĂIN, cu vinovatul la 13.000 de linii distanță. De aceea
                // se verifică pe TOATE stările, nu doar pe cele vii.
                s.Check("F21-D9.9 — curățenie finală felia ITV: purjă FIZICĂ, ZERO închideri de TVA rămase pe 11/12 "
                    + "2026 (inclusiv cea stornată și regenerata ei), fără "
                    + "repartitori de scenă, iar soldurile 4423/4426/4427 revin la 0 pe 31.12 — lunile 11/12 rămân "
                    + "CURATE pentru scenele ASM și RLF/RDC care urmează",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiItv))
                    && !os.GetObjectsQuery<InchidereTva>()
                        .Any(d => d.Data >= new DateOnly(2026, 11, 1))
                    && !os.GetObjectsQuery<InchidereTva>().Any(d => d.Stare != StareDocument.Stornat)
                    && !os.GetObjectsQuery<Document>()
                        .Any(d => d.Data >= new DateOnly(2026, 11, 1) && d.Numar != null
                            && d.Numar.StartsWith(MarcajApiItv))
                    && SoldDebitorApiCub(cont4426, finalDec) == 0m && SoldCreditorApiCub(cont4427, finalDec) == 0m
                    && SoldCreditorApiCub(cont4423, finalDec) == 0m);
            }
        }
    }
}
