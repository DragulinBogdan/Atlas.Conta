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

// Felia 26 (E2E-IMO): scena stă în 2027/5–12, în afara perioadelor seed-uite (precedentul `D17-V2`); conturile se CITESC din politică.
// Felia 27, pasul 1 (F27-D1/D2): perioada ca LANȚ, cu închiderea și
// redeschiderea ca operații ale motorului. Scena stă în 2030 — în afara
// perioadelor seed-uite (2026) și a tuturor celorlalte scene (2027 imobilizări,
// 2028 review F26, 2029 gardianul de perioadă al motorului). Lunile 1 și 2 NU se
// creează deliberat: absența lor face din 03/2030 CAPĂTUL lanțului.
static class VerificaPerioade {
    public static void Ruleaza(Suita s, bool privat) {
        const string Marcaj = "E2E-PER";
        const int An = 2030;
        var eticheta = privat ? "privat" : "bugetar";

        void CurataPer(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ, în ordinea dependențelor.
            // Snapshot-urile scrise de comanda de închidere (F27-D3) ies ÎNTÂI:
            // FK-urile lor spre cont/repartitor sunt `Restrict`.
            for (var luna = 1; luna <= 12; luna++)
                SolduriService.Elimina(os, An, luna);
            var pj = new Purja(os);
            var docIds = os.GetObjectsQuery<Document>()
                .Where(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31))
                .Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>()
                .Where(d => docIds.Contains(d.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<Document>()
                .Where(d => docIds.Contains(d.ID)).ToList());
            var perioadeIds = os.GetObjectsQuery<PerioadaFiscala>()
                .Where(x => x.An == An).Select(x => x.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => perioadeIds.Contains(i.PerioadaId)).ToList());
            pj.Adauga(os.GetObjectsQuery<PerioadaFiscala>()
                .Where(x => x.An == An).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            pj.Executa();
        }

        // Calea REALĂ: dispecerul din gardian, nu corpul regulii.
        static string RefuzGardianPer(IObjectSpace os) {
            try {
                GardianEditare.Verifica(os);
                return null;
            }
            catch (OperareException e) {
                return e.Message;
            }
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataPer(os);

        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(x => x.An == An);
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            Console.WriteLine($"     MĂSURAT (PER-V0/{eticheta}): {perioade} perioade și {documente} documente în {An} după purjă.");
            s.Check($"PER-V0 ({eticheta}) precondiție: anul {An} e liber (nicio perioadă, niciun document) — altfel "
                + "lanțul probat mai jos ar fi măsurat peste conținut străin",
                perioade == 0 && documente == 0);
        }

        Guid idGestiuneA, idGestiuneB;
        using (var os = s.Provider.CreateObjectSpace()) {
            foreach (var luna in new[] { 3, 4, 5 }) {
                var x = os.CreateObject<PerioadaFiscala>();
                x.An = An;
                x.Luna = luna;
            }
            var a = os.CreateObject<Gestiune>();
            a.Cod = Marcaj + "-G1";
            a.Denumire = "Gestiune probă perioade 1";
            var b = os.CreateObject<Gestiune>();
            b.Cod = Marcaj + "-G2";
            b.Denumire = "Gestiune probă perioade 2";
            os.CommitChanges();
            idGestiuneA = a.ID;
            idGestiuneB = b.ID;
        }

        // ── PER-V1…V5: lanțul, prin comanda motorului ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var refuzNecontigua = s.Refuz(() => PerioadaService.Inchide(os, An, 4, [], null, Marcaj));
            s.Check($"PER-V1 ({eticheta}) închiderea lui 04/{An} cu 03/{An} DESCHISĂ e refuzată: perioadele se închid "
                + "în LANȚ, iar o lună sărită ar lăsa granița falsă pentru tot ce se operează înaintea ei",
                refuzNecontigua != null && refuzNecontigua.Contains($"03/{An}"));

            var prima = PerioadaService.Inchide(os, An, 3, [], null, Marcaj);
            var p3 = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 3);
            s.Check($"PER-V2 ({eticheta}) 03/{An} se închide deși 02/{An} nu EXISTĂ: perioada absentă e închisă prin "
                + "absență, deci dă capătul lanțului; `InchisaLa` și `InchisaPrimaOara` se nasc egale, iar istoricul "
                + "primește un singur rând `Închidere`, fără motiv",
                p3.Inchisa && p3.InchisaLa != null && p3.InchisaPrimaOara == p3.InchisaLa
                && p3.InchisaLa == prima.La && prima.Fel == FelInchiderePerioada.Inchidere && prima.Motiv == null
                && os.GetObjectsQuery<InchiderePerioada>().Count(i => i.PerioadaId == p3.ID) == 1);

            var refuzDouaOri = s.Refuz(() => PerioadaService.Inchide(os, An, 3, [], null, Marcaj));
            s.Check($"PER-V3 ({eticheta}) a doua închidere a lui 03/{An} e refuzată — comanda RERULEAZĂ verificarea, "
                + "deci „deja închisă” e blocant, nu operație idempotentă tăcută",
                refuzDouaOri != null && refuzDouaOri.Contains("deja închisă"));

            var refuzFaraMotiv = s.Refuz(() => PerioadaService.Redeschide(os, An, 3, "   ", null, Marcaj));
            s.Check($"PER-V4 ({eticheta}) redeschiderea fără motiv e refuzată: redeschiderea e o excepție care se "
                + "justifică în scris, nu un buton",
                refuzFaraMotiv != null && refuzFaraMotiv.Contains("motiv"));

            PerioadaService.Inchide(os, An, 4, [], null, Marcaj);
            var refuzUrmatoareaInchisa = s.Refuz(() => PerioadaService.Redeschide(os, An, 3, "probă", null, Marcaj));
            s.Check($"PER-V5 ({eticheta}) cu 04/{An} închisă, redeschiderea lui 03/{An} e refuzată — se redeschide doar "
                + "ULTIMA perioadă închisă, deci cascada e explicită și un „stale” pe lunile deja închise devine "
                + "imposibil prin construcție",
                refuzUrmatoareaInchisa != null && refuzUrmatoareaInchisa.Contains($"04/{An}"));
            PerioadaService.Redeschide(os, An, 4, "probă: eliberez lanțul", null, Marcaj);
        }

        // ── PER-V6: gardianul motorului vede bitul scris de comandă ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var doc = os.CreateObject<NotaTransfer>();
            doc.Data = new DateOnly(An, 3, 15);
            doc.PredatorId = idGestiuneA;
            doc.PrimitorId = idGestiuneB;
            doc.NumarPV = Marcaj;
            os.CommitChanges();
            var refuzMotor = s.Refuz(() => MotorOperare.Opereaza(os, doc));
            s.Check($"PER-V6 ({eticheta}) după închidere, operarea unui document datat în 03/{An} e refuzată cu TEXTUL "
                + "gardianului de perioadă — comanda de închidere și hot path-ul motorului citesc același bit",
                refuzMotor == $"Perioada 03/{An} e închisă.");
        }

        // ── PER-V7: închidere → redeschidere → închidere ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var p3 = os.FirstOrDefault<PerioadaFiscala>(x => x.An == An && x.Luna == 3);
            var primaOara = p3.InchisaPrimaOara;
            PerioadaService.Redeschide(os, An, 3, "probă: eroare materială", null, Marcaj);
            var deschisaCurat = !p3.Inchisa && p3.InchisaLa == null && p3.InchisaPrimaOara == primaOara;
            var aDoua = PerioadaService.Inchide(os, An, 3, [], null, Marcaj);
            var istoric = os.GetObjectsQuery<InchiderePerioada>()
                .Where(i => i.PerioadaId == p3.ID).OrderBy(i => i.La).ToList();
            Console.WriteLine($"     MĂSURAT (PER-V7/{eticheta}): istoric 03/{An} = "
                + string.Join(" → ", istoric.Select(i => i.Fel.ToString())) + ".");
            s.Check($"PER-V7 ({eticheta}) închidere → redeschidere → închidere pe 03/{An}: `InchisaPrimaOara` rămâne a "
                + "PRIMEI închideri (reperul rectificativei, nu se șterge la redeschidere), `InchisaLa` e a celei de-a "
                + "doua, iar istoricul are trei rânduri în ordine, cu motiv DOAR pe redeschidere",
                deschisaCurat
                && p3.InchisaPrimaOara == primaOara && p3.InchisaLa == aDoua.La
                && istoric.Count == 3
                && istoric[0].Fel == FelInchiderePerioada.Inchidere
                && istoric[1].Fel == FelInchiderePerioada.Redeschidere
                && istoric[2].Fel == FelInchiderePerioada.Inchidere
                && istoric[0].Motiv == null && istoric[2].Motiv == null
                && !string.IsNullOrWhiteSpace(istoric[1].Motiv));
        }

        // ── PER-V8: gardianul de editare, pe CALEA REALĂ (dispecerul) ──
        // Fiecare caz pe ObjectSpace propriu, necomis: refuzul e verdictul, nu efectul.
        using (var os = s.Provider.CreateObjectSpace()) {
            var x = os.CreateObject<PerioadaFiscala>();
            x.An = An;
            x.Luna = 9;
            x.Inchisa = true;
            s.Check($"PER-V8a ({eticheta}) crearea unei perioade DEJA închise e refuzată: starea e a motorului, deci "
                + "nu se poate naște închisă pe ușa securizată",
                RefuzGardianPer(os)?.Contains("DESCHISĂ") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var x = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 5);
            x.Inchisa = true;
            s.Check($"PER-V8b ({eticheta}) trecerea lui `Inchisa` pe o perioadă existentă e refuzată cu fraza „o face "
                + "doar motorul” — exact regula pe care registrele o au de la decizia 14",
                RefuzGardianPer(os)?.Contains("doar motorul") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var x = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 5);
            x.An = An + 1;
            s.Check($"PER-V8c ({eticheta}) mutarea lunii unei perioade existente e refuzată: `(An, Luna)` e identitatea "
                + "verigii, iar rescrierea ei ar muta granița sub documentele deja operate",
                RefuzGardianPer(os)?.Contains("nu se schimbă") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var x = os.CreateObject<PerioadaFiscala>();
            x.An = An;
            x.Luna = 5;
            s.Check($"PER-V8d ({eticheta}) a doua verigă pe aceeași lună e refuzată de gardian, înainte ca indexul unic "
                + "să o refuze de bază",
                RefuzGardianPer(os)?.Contains("există deja") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var p5 = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 5);
            var rand = os.CreateObject<InchiderePerioada>();
            rand.PerioadaId = p5.ID;
            rand.Fel = FelInchiderePerioada.Inchidere;
            rand.La = DateTime.UtcNow;
            s.Check($"PER-V8e ({eticheta}) crearea unui rând de istoric direct e refuzată: istoricul închiderilor e "
                + "append-only ȘI exclusiv al motorului, ca cele patru registre",
                RefuzGardianPer(os)?.Contains("doar de motor") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var p3 = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 3);
            var rand = os.GetObjectsQuery<InchiderePerioada>().First(i => i.PerioadaId == p3.ID);
            os.Delete(rand);
            s.Check($"PER-V8f ({eticheta}) ștergerea unui rând de istoric e refuzată pe aceeași frază — append-only "
                + "înseamnă și „nu se rescrie urma”",
                RefuzGardianPer(os)?.Contains("doar de motor") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var p3 = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 3);
            os.Delete(p3);
            s.Check($"PER-V8g ({eticheta}) ștergerea unei perioade ÎNCHISE e refuzată: granița nu dispare prin "
                + "ștergerea verigii",
                RefuzGardianPer(os)?.Contains("nu se șterge") == true);
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var p4 = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 4);
            os.Delete(p4);
            var refuzIstoric = RefuzGardianPer(os);
            s.Check($"PER-V8h ({eticheta}) ștergerea unei perioade DESCHISE care are istoric e tot refuzată (04/{An} a "
                + "fost închisă și redeschisă): FK-ul e `Restrict`, iar urma nu rămâne orfană",
                refuzIstoric != null && refuzIstoric.Contains("istoric"));
        }
        using (var os = s.Provider.CreateObjectSpace()) {
            var p5 = os.FirstOrDefault<PerioadaFiscala>(y => y.An == An && y.Luna == 5);
            os.Delete(p5);
            s.Check($"PER-V8i ({eticheta}) control POZITIV: o perioadă deschisă fără istoric se șterge — gardianul "
                + "refuză granița și urma, nu nomenclatorul",
                RefuzGardianPer(os) == null);
        }

        // ── PER-V9: unicitatea e și a BAZEI, nu doar a gardianului ──
        using (var os = s.Provider.CreateObjectSpace()) {
            var x = os.CreateObject<PerioadaFiscala>();
            x.An = An;
            x.Luna = 5;
            string violare = null;
            try {
                os.CommitChanges();
            }
            catch (Exception e) {
                for (var ex = e; ex != null; ex = ex.InnerException)
                    if (ex.Message.Contains("IX_PerioadeFiscale_An_Luna"))
                        violare = ex.Message;
            }
            s.Check($"PER-V9 ({eticheta}) duplicatul `(An, Luna)` e refuzat și de BAZĂ (indexul unic), pe o cale fără gardian — fără el `VerificaDeschisa` ar alege nedeterminist între "
                + "două rânduri",
                violare != null);
        }

        using (var os = s.Provider.CreateObjectSpace())
            CurataPer(os);
        using (var os = s.Provider.CreateObjectSpace()) {
            var perioade = os.GetObjectsQuery<PerioadaFiscala>().Count(x => x.An == An);
            var istoric = os.GetObjectsQuery<InchiderePerioada>().Count();
            var documente = os.GetObjectsQuery<Document>()
                .Count(d => d.Data >= new DateOnly(An, 1, 1) && d.Data <= new DateOnly(An, 12, 31));
            s.Check($"PER-V10 ({eticheta}) fără reziduu: nicio perioadă, niciun rând de istoric și niciun document {An} "
                + "rămase după purjă — scena e re-rulabilă identic",
                perioade == 0 && documente == 0 && istoric == 0);
        }
    }
}
