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

// ============ Scenariul e2e felia 7: viramentul intern (transferul 581) ============
// Contract p5-felia-vir, ancorele F7-D9. Transferul de bani între conturile
// proprii (casă ↔ bancă) — amânarea declarată la decizia 31f. Viramentul NU e un
// tip de document nou (F7-D1): e o PERECHE PLT+INC pe ACELEAȘI laturi (predator =
// contul sursă, primitor = contul destinație pe ambele picioare), fiindcă cele
// două picioare sunt confirmate de documente diferite, la date diferite (foaia de
// vărsământ azi, extrasul mâine). Contul 581 („viramente interne") ține diferența
// de timp și se închide singur când ambele picioare sunt operate.
//
// Fluxul probat, cap-coadă prin API: culegere prin `TrezorerieApply.Aplica<Plata>`
// → refuzurile cuplajului laturi↔natură prin dry-run → operare (581 = 5311, zero
// stoc, dimensiunea Repartitor = contul propriu AL PICIORULUI — F7-D5b) → latura
// pereche autogenerată în `Copii[]` → operarea ei (5121/770 = 581, ZERO
// imperecheri — F7-D5) → 581 închis la 0 → anulare, regenerare, storno pe ambele.
static class E2eVirament {
    public static void Ruleaza(Suita s) {
        const string MarcajAvir = "E2E-AVIR";

        void CurataAvir(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            // Toate documentele blocului ating cel puțin un repartitor marcat (inclusiv
            // latura pereche autogenerată: aceleași laturi ca sursa) — aceeași cheie de
            // curățenie ca la CurataApiTrz.
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAvir)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            // Copiii (latura pereche) înaintea părinților — FK-ul DocumentSursa.
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajAvir)).ToList());
            // Regulile-fixtură ale probei de semn (F2) — înaintea Tipului lor, altfel
            // FK-ul le-ar ține în viață după o rulare întreruptă.
            var tipuriAvir = os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajAvir)).Select(t => t.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<RegulaContare>()
                .Where(r => r.TipMaterialId != null && tipuriAvir.Contains(r.TipMaterialId.Value)).ToList());
            // Tipul-fixtură (natura Virament, FĂRĂ regulă) se șterge DUPĂ linii.
            pj.Adauga(os.GetObjectsQuery<TipMaterial>().Where(t => t.Cod.StartsWith(MarcajAvir)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajAvir + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataAvir(os);

            var tipVir = os.FirstOrDefault<TipMaterial>(t => t.Cod == "VIR");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var cont581 = os.FirstOrDefault<Cont>(c => c.Simbol == "581");
            var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
            var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            var ancoraPlt = os.FirstOrDefault<TipDocument>(t => t.Cod == "PLT");
            var ancoraInc = os.FirstOrDefault<TipDocument>(t => t.Cod == "INC");

            // ── (a) Seed-ul F7-D6: Clasa/Tipul VIR + cele două reguli ale perechii ──
            s.Check("Seed F7-D6: Clasa/Tipul „VIR” (Natura=Virament) cu contul de tranzit 581 legat EXPLICIT (derivările din simbol nu ating codul „VIR”)",
                tipVir != null && tipVir.Clasa.Cod == "VIR" && tipVir.Clasa.Natura == NaturaClasa.Virament
                && cont581 != null && tipVir.ContImplicitId == cont581.ID);
            var regulaPltVir = os.FirstOrDefault<RegulaContare>(
                r => r.TipDocumentId == ancoraPlt.ID && r.TipMaterialId == tipVir.ID);
            var regulaIncVir = os.FirstOrDefault<RegulaContare>(
                r => r.TipDocumentId == ancoraInc.ID && r.TipMaterialId == tipVir.ID);
            s.Check("Seed F7-D6: PLT×VIR = 581 (TipMaterial) / contul propriu PREDATOR fără fallback; INC×VIR oglindit (destinație / 581)",
                regulaPltVir != null && regulaPltVir.SursaContDebit == SursaCont.TipMaterial
                && regulaPltVir.SursaContCredit == SursaCont.RepartitorPredator && regulaPltVir.ContCreditId == null
                && regulaIncVir != null && regulaIncVir.SursaContDebit == SursaCont.RepartitorPrimitor
                && regulaIncVir.ContDebitId == null && regulaIncVir.SursaContCredit == SursaCont.TipMaterial);
            s.Check("Seed F7-D6: garda de idempotență e PER RÂND — rândul generic al fiecărui tip există exact o dată (nu s-a duplicat la re-seed, nici n-a înghițit rândul nou)",
                os.GetObjectsQuery<RegulaContare>().Count(r => r.TipDocumentId == ancoraPlt.ID && r.TipMaterialId == null) == 1
                && os.GetObjectsQuery<RegulaContare>().Count(r => r.TipDocumentId == ancoraInc.ID && r.TipMaterialId == null) == 1);
            s.Check("Seed F7-D6: viramentul NU primește reguli de STOC (Natura ≠ Stoc ⇒ motorul nu caută nimic)",
                !os.GetObjectsQuery<RegulaStoc>().Any(r => r.TipDocument.Cod == "PLT" || r.TipDocument.Cod == "INC"));

            // Fixture: DOUĂ conturi proprii distincte (marcate — cheia de curățenie) +
            // un partener (contrapartida „greșită” a probelor de cuplaj) + un Tip de
            // natura Virament FĂRĂ rând de regulă, adică exact riscul pin-uit al feliei
            // („profil cu Tipul VIR seed-uit dar fără politica lui”). Fixtura înlocuiește
            // ștergerea temporară a regulii seed-uite: probează ACELAȘI gard, fără să
            // atingă seed-ul bazei.
            var casaVir = os.CreateObject<ContPropriu>();
            casaVir.Cod = MarcajAvir + "-CASA";
            casaVir.Denumire = "Casa probă virament";
            casaVir.ContImplicit = cont531;
            var bancaVir = os.CreateObject<ContPropriu>();
            bancaVir.Cod = MarcajAvir + "-BANCA";
            bancaVir.Denumire = "Banca probă virament";
            bancaVir.ContImplicit = cont770;
            bancaVir.EsteBanca = true;
            var partenerVir = os.CreateObject<Partener>();
            partenerVir.Cod = MarcajAvir + "-P";
            partenerVir.Denumire = "Partener probă virament";
            var codEcVir = os.CreateObject<CodEconomic>();
            codEcVir.Cod = MarcajAvir + "-CE";
            codEcVir.Denumire = "Cod economic probă virament";
            var tipVirFaraRegula = os.CreateObject<TipMaterial>();
            tipVirFaraRegula.Cod = MarcajAvir + "-VIR2";
            tipVirFaraRegula.Denumire = "Virament fără politică (fixtură)";
            tipVirFaraRegula.Clasa = tipVir.Clasa;
            tipVirFaraRegula.ContImplicit = cont581;
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (`PregatesteOperare` SCRIE).
            IReadOnlyList<string> DryRunAvir(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // ── (b) Ancora 1: viramentul cules prin API, cu affordance-ul de FORMĂ ──
            var writeVir = new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 10),
                PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
                TipInstrument = "DispozitieCasa",
                NumarExtras = "FV-AVIR-1", DataExtras = new DateOnly(2026, 4, 10),
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipVir.ID, Valoare = 500m, CodEconomicId = codEcVir.ID } }
            };
            var idVirPlt = TrezorerieApply.Aplica<Plata>(os, null, writeVir);
            var virPlt = TrezorerieApply.Citeste<Plata>(os, idVirPlt);
            s.Check("F7-D9 ancora 1: virament cules prin Apply<Plata> (CASA → BANCA, linie VIR) — WriteDto NEATINS, iar Citeste întoarce EsteVirament = true, calculat pe SERVER",
                virPlt is { EsteVirament: true, Stare: "Draft", Numar: null, Total: 500m, Autogenerat: false }
                && virPlt.PredatorId == casaVir.ID && virPlt.PrimitorId == bancaVir.ID
                && virPlt.Linii.Count == 1 && virPlt.Linii[0].TipMaterialCod == "VIR"
                && virPlt.Linii[0].Valoare == 500m && virPlt.Linii[0].CodEconomicId == codEcVir.ID);
            s.Check("F7-D9 ancora 1: dry-run pe viramentul valid → listă goală (cuplajul laturi↔natură se potrivește în ambele sensuri)",
                DryRunAvir(idVirPlt).Count == 0);

            // Martorul: aceeași casă, dar contrapartidă PARTENER și linie TRZ = plată
            // obișnuită. `EsteVirament` cere AMBELE laturi conturi proprii, nu doar una.
            var idPltNormala = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 10),
                PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
            });
            s.Check("F7-D7: plata OBIȘNUITĂ din același cont propriu rămâne EsteVirament = false, iar dry-run-ul ei e curat (nicio regresie de laturi)",
                TrezorerieApply.Citeste<Plata>(os, idPltNormala) is { EsteVirament: false }
                && DryRunAvir(idPltNormala).Count == 0);

            // ── (c) Ancora 2: cele patru refuzuri, prin dry-run ─────────────────────
            var politicaPlt = os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT");
            var serieInainte = politicaPlt.UrmatorulNumar;

            var idRefuzTrz = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 11),
                PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
            });
            var eroriTrzPeContPropriu = DryRunAvir(idRefuzTrz);
            s.Check("F7-D9 ancora 2a: contrapartidă cont propriu + linie TRZ → refuz (fără cuplaj, linia ar cădea pe regula GENERICĂ a tipului și ar posta „destinație = sursă” pe FIECARE picior — dublă postare tăcută)",
                eroriTrzPeContPropriu.Count == 1
                && eroriTrzPeContPropriu[0].Contains("toate liniile trebuie să fie de virament"));

            var idRefuzVirPePartener = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 11),
                PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipVir.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
            });
            var eroriVirPePartener = DryRunAvir(idRefuzVirPePartener);
            s.Check("F7-D9 ancora 2b: linie VIR + contrapartidă partener → refuz (cuplajul e verificat în AMBELE sensuri; altfel 581 ar rămâne deschis pe veci)",
                eroriVirPePartener.Count == 1
                && eroriVirPePartener[0].Contains("cer contrapartidă cont propriu"));

            var idRefuzCatreSine = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 11),
                PredatorId = casaVir.ID, PrimitorId = casaVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipVir.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
            });
            var eroriCatreSine = DryRunAvir(idRefuzCatreSine);
            s.Check("F7-D9 ancora 2c: Predator == Primitor → refuz (581 = 581 pe același cont propriu, iar dimensiunea implicită n-ar mai distinge nimic)",
                eroriCatreSine.Count == 1
                && eroriCatreSine[0].Contains("același repartitor"));

            var idRefuzFaraRegula = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 11),
                PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipVirFaraRegula.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
            });
            var eroriFaraRegula = DryRunAvir(idRefuzFaraRegula);
            s.Check("F7-D9 ancora 2d: linie de natura Virament FĂRĂ regulă potrivită → refuz explicit (oglinda 38c: potrivirea ar cădea pe regula generică a tipului, fără niciun zgomot)",
                eroriFaraRegula.Count == 1
                && eroriFaraRegula[0].Contains("nu are regulă de contare potrivită"));

            var idsRefuz = new List<Guid> { idRefuzTrz, idRefuzVirPePartener, idRefuzCatreSine, idRefuzFaraRegula };
            s.Check("F7-D9 ancora 2: niciun refuz n-a lăsat rânduri-fantomă și n-a consumat serie (dry-run = calculează+validează, fără materializare — 33d)",
                idsRefuz.All(i => TrezorerieApply.Citeste<Plata>(os, i).Numar == null)
                && idsRefuz.All(i => CubScena.FaraNote(os, i))
                && idsRefuz.All(i => CubScena.FaraStoc(os, i))
                && os.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocument.Cod == "PLT").UrmatorulNumar == serieInainte);
            foreach (var idRefuz in idsRefuz)
                TrezorerieApply.Sterge<Plata>(os, idRefuz);
            TrezorerieApply.Sterge<Plata>(os, idPltNormala);

            // ── Review advers F2: gardul trebuie să OGLINDEASCĂ matcher-ul motorului ──
            // Motorul filtrează ÎNTÂI pe `SemnFiltru` și abia din supraviețuitori alege
            // Tip exact → NaturaFiltru → generic. Pe trezorerie `Cantitate` = 0, deci
            // semnul e 0 și orice rând cu SemnFiltru ±1 iese din joc. `RegulaContare` e
            // dată EDITABILĂ în XAF: un rând de virament cu semn pus din greșeală ar
            // trece de un gard care ignoră semnul, iar motorul ar cădea pe regula
            // GENERICĂ a tipului — exact dubla postare tăcută pe care gardul o previne.
            var regulaSemn = os.CreateObject<RegulaContare>();
            regulaSemn.TipDocument = ancoraPlt;
            regulaSemn.TipMaterial = tipVirFaraRegula;
            regulaSemn.SursaContDebit = SursaCont.TipMaterial;
            regulaSemn.SursaContCredit = SursaCont.RepartitorPredator;
            regulaSemn.SemnFiltru = 1;
            os.CommitChanges();
            var idRefuzSemn = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 11),
                PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipVirFaraRegula.ID, Valoare = 10m, CodEconomicId = codEcVir.ID } }
            });
            var eroriSemn = DryRunAvir(idRefuzSemn);
            s.Check("Review advers F2: rândul de virament cu `SemnFiltru = +1` NU e o potrivire (motorul îl exclude, semnul liniei de trezorerie e 0) ⇒ gardul refuză în continuare, în loc să lase documentul pe regula generică",
                eroriSemn.Count == 1 && eroriSemn[0].Contains("nu are regulă de contare potrivită"));
            TrezorerieApply.Sterge<Plata>(os, idRefuzSemn);
            os.CommitChanges();
            // F13-D2: regula de probă e artefact de scenă, nu obiect de probă — purjă FIZICĂ.
            new Purja(os).Adauga(regulaSemn).Executa();

            // ── (d) Ancora 3: operarea piciorului de IEȘIRE ─────────────────────────
            var rezVirPlt = ComenziDocument.Sistem(os).Opereaza(idVirPlt);
            var idVirInc = rezVirPlt.ConexId ?? Guid.Empty;
            var notePicior1Cub = CubScena.Note(os, idVirPlt);
            s.Check("F7-D9 ancora 3: piciorul de ieșire postează 581 (tranzit) = 531.01.01 (contul propriu SURSĂ), 500 — un singur rând, ZERO stoc",
                notePicior1Cub.Count == 2 && notePicior1Cub.Nota(cont581.ID, cont531.ID, 500m)
                && CubScena.FaraStoc(os, idVirPlt));
            s.Check("F7-D9 ancora 3 (F7-D5b): dimensiunea Repartitor = contul propriu AL PICIORULUI pe AMBELE laturi ale rândului — default-ul „debit←Predator/credit←Primitor” ar fi atribuit ieșirea contului DESTINAȚIE",
                notePicior1Cub.Count == 2 && notePicior1Cub.All(p => p.Gestiune == casaVir.ID));

            // --- NUC-PLT-VIR (B-D5, pas 4): ambele capete pe contul propriu AL PICIORULUI ---
            ProbeNucleu.Proba(os, s.Check, "NUC-PLT-VIR", [os.GetObjectByKey<Plata>(idVirPlt)]);

            // ── (e) Ancora 4: latura pereche ────────────────────────────────────────
            var virPltOperat = TrezorerieApply.Citeste<Plata>(os, idVirPlt);
            s.Check("F7-D9 ancora 4: latura pereche apare în Copii[] ca INC, Draft, Autogenerat, fără număr (seria se consumă la propria operare)",
                rezVirPlt.StareNoua == StareDocument.Operat && rezVirPlt.ConexId != null
                && virPltOperat.Numar?.StartsWith("PLT-") == true
                && virPltOperat.Copii.Count == 1 && virPltOperat.Copii[0].Id == idVirInc
                && virPltOperat.Copii[0].Tip == "INC" && virPltOperat.Copii[0].Stare == "Draft"
                && virPltOperat.Copii[0].Autogenerat && virPltOperat.Copii[0].Numar == null);
            var virInc = TrezorerieApply.Citeste<Incasare>(os, idVirInc);
            s.Check("F7-D9 ancora 4: perechea are laturile IDENTICE (NEinversate — direcția o poartă TIPUL), NumarExtras/DataExtras GOALE (fiecare picior are extrasul lui) și linia clonată cu dimensiuni",
                virInc is { EsteVirament: true, Autogenerat: true, Stare: "Draft", Numar: null,
                    NumarExtras: null, DataExtras: null, Total: 500m }
                && virInc.PredatorId == casaVir.ID && virInc.PrimitorId == bancaVir.ID
                && virInc.Data == new DateOnly(2026, 4, 10) && virInc.TipInstrument == "DispozitieCasa"
                && virInc.DocumentSursaId == idVirPlt && virInc.DocumentSursaTip == "PLT"
                && virInc.Linii.Count == 1 && virInc.Linii[0].TipMaterialId == tipVir.ID
                && virInc.Linii[0].Valoare == 500m && virInc.Linii[0].CodEconomicId == codEcVir.ID);

            // Pivotul polimorf al lui F7-D5, direct pe contract: viramentul nu stinge și
            // nu poate fi stins; plata obișnuită își păstrează plafonul de dinainte.
            var idPltMartor = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 10),
                PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
            });
            s.Check("F7-D5: `CapacitateStingere` e NULL pe AMBELE picioare ale viramentului și rămâne plafonul obișnuit (o contrapartidă) pe plata către partener",
                os.GetObjectByKey<Plata>(idVirPlt).CapacitateStingere(os) == null
                && os.GetObjectByKey<Incasare>(idVirInc).CapacitateStingere(os) == null
                && os.GetObjectByKey<Plata>(idPltMartor).CapacitateStingere(os) is { Count: 1 });
            TrezorerieApply.Sterge<Plata>(os, idPltMartor);

            // ── (f) Ancora 5 + 8: operarea laturii pereche ──────────────────────────
            // Operatorul îi pune extrasul lui și o operează — perechea E editabilă prin
            // aceeași rută (`PoateEdita` = funcție de stare, F5-D8b).
            TrezorerieApply.Aplica<Incasare>(os, idVirInc, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 12),
                PredatorId = casaVir.ID, PrimitorId = bancaVir.ID,
                TipInstrument = "DispozitieCasa",
                NumarExtras = "EX-AVIR-2", DataExtras = new DateOnly(2026, 4, 12),
                Linii = { new TrezorerieLinieWriteDto {
                    Id = virInc.Linii[0].Id, TipMaterialId = tipVir.ID,
                    Valoare = 500m, CodEconomicId = codEcVir.ID } }
            });
            var rezVirInc = ComenziDocument.Sistem(os).Opereaza(idVirInc);
            var notePicior2Cub = CubScena.Note(os, idVirInc);
            s.Check("F7-D9 ancora 5: piciorul de INTRARE postează 770.00.00 (contul propriu DESTINAȚIE) = 581, la data lui; Repartitor = contul propriu al ACESTUI picior pe ambele laturi",
                rezVirInc.StareNoua == StareDocument.Operat
                && notePicior2Cub.Count == 2 && notePicior2Cub.Nota(cont770.ID, cont581.ID, 500m)
                && notePicior2Cub.All(p => p.Data == new DateOnly(2026, 4, 12))
                && notePicior2Cub.All(p => p.Gestiune == bancaVir.ID)
                && CubScena.FaraStoc(os, idVirInc));
            s.Check("F7-D9 ancora 8: latura pereche OPERATĂ nu generează un al treilea document — gardul `Autogenerat` din `GenereazaSecundar` taie ping-pong-ul",
                rezVirInc.ConexId == null
                && !os.GetObjectsQuery<Document>().Any(d => d.DocumentSursaId == idVirInc));

            // --- NUC-INC-VIR (B-D5, pas 4): latura pereche e document propriu, deci se declara ---
            ProbeNucleu.Proba(os, s.Check, "NUC-INC-VIR", [os.GetObjectByKey<Incasare>(idVirInc)]);

            var idsPicioare = new List<Guid> { idVirPlt, idVirInc };
            s.Check("F7-D9 ancora 5: EXACT două rânduri de registru contabil pe toată perechea, ZERO rânduri de stoc, iar 581 se închide singur la 0 după ambele picioare",
                CubScena.Note(os, [.. idsPicioare]).Count == 4
                && CubScena.SoldCont(os, cont581.ID, null, idsPicioare) == 0m
                && idsPicioare.All(i => CubScena.FaraStoc(os, i)));
            s.Check("F7-D9 ancora 5: ZERO imperecheri create la operarea laturii pereche (F7-D5 — un link cu propria sursă ar fi blocat anularea/stornarea AMBELOR picioare)",
                !os.GetObjectsQuery<Imperechere>().Any(i => idsPicioare.Contains(i.DocumentStingatorId) || idsPicioare.Contains(i.DocumentId)));
            var virIncOperat = TrezorerieApply.Citeste<Incasare>(os, idVirInc);
            s.Check("F7-D9 ancora 5: affordance-ul rămâne onest — piciorul operat, fără imperecheri, se poate anula/storna; numărul vine din seria INC-",
                virIncOperat is { Stare: "Operat", Asignat: 0m, PoateAnula: true, PoateStorna: true, EsteVirament: true }
                && virIncOperat.Numar?.StartsWith("INC-") == true
                && virIncOperat.NumarExtras == "EX-AVIR-2");

            // ── Review advers F1: „nu poate fi STINS” — IMPUS, nu doar afirmat ──────
            // `CapacitateStingere` = null îl scoate din rolul de STINGĂTOR; rolul de
            // STINS e altul, și argumentul „contrapartida unei plăți normale nu apare pe
            // laturile lui” e FALS pentru `NotaContabila`: capacitățile ei sunt
            // repartitorii EXPLICIȚI ai liniilor (49a), deci pot cădea pe orice latură,
            // inclusiv pe conturile proprii ale unui picior de virament. O astfel de
            // stingere i-ar bloca definitiv anularea/stornarea, iar clientul ascunde
            // panoul de stingeri pe virament (F7-D8) ⇒ operatorul n-ar vedea DE CE.
            var unitateAvir = os.CreateObject<UnitateInterna>();
            unitateAvir.Cod = MarcajAvir + "-UI";
            unitateAvir.Denumire = "Unitate probă virament";
            os.CommitChanges();

            // Martorul de NONREGRESIE: plată OBIȘNUITĂ (contrapartidă partener) care
            // poartă ACELAȘI cont propriu pe latura ei — nota de mai jos trebuie să o
            // stingă normal. Gardul țintește viramentul, nu nota.
            var idPltStins = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 4, 13),
                PredatorId = casaVir.ID, PrimitorId = partenerVir.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 70m, CodEconomicId = codEcVir.ID } }
            });
            ComenziDocument.Sistem(os).Opereaza(idPltStins);

            var ntcAvir = os.CreateObject<NotaContabila>();
            ntcAvir.Data = new DateOnly(2026, 4, 13);
            ntcAvir.Predator = unitateAvir;
            ntcAvir.Primitor = unitateAvir;
            var linieNtcAvir = os.CreateObject<NotaContabilaDetaliu>();
            linieNtcAvir.Document = ntcAvir;
            linieNtcAvir.TipMaterial = tipTrz;
            linieNtcAvir.Descriere = "Probă: repartitor explicit = contul propriu al viramentului";
            linieNtcAvir.ContDebit = cont581;
            linieNtcAvir.ContCredit = cont531;
            linieNtcAvir.RepartitorDebit = casaVir;
            linieNtcAvir.RepartitorCredit = casaVir;
            linieNtcAvir.CodEconomic = codEcVir;
            linieNtcAvir.Valoare = 200m;
            os.CommitChanges();
            MotorOperare.Opereaza(os, ntcAvir);
            s.Check("Review advers F1 (premisa): nota operată CHIAR poartă contul propriu al viramentului ca CONTRAPARTIDĂ stingibilă — invariantul de contrapartidă (31d) n-ar fi oprit-o",
                ntcAvir.Stare == StareDocument.Operat
                && ntcAvir.CapacitateStingere(os).ContainsKey(casaVir.ID));
            s.CheckRefuza("Review advers F1: nota NU poate stinge un picior de virament — rolul de STINS e polimorf (`PoateFiStins`), altfel piciorul rămânea blocat la anulare/storno fără explicație în UI",
                () => ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Plata>(idVirPlt), 50m));
            s.CheckRefuza("Review advers F1: nici celălalt picior (INC) nu se lasă stins — gardul e pe TIP, nu pe direcție",
                () => ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Incasare>(idVirInc), 50m));
            s.CheckRefuza("101: nota fără partidă comună nu stinge plata", () =>
                ImperechereService.Imperecheaza(os, ntcAvir, os.GetObjectByKey<Plata>(idPltStins), 50m));
            s.Check("101: nota fără partidă comună păstrează restul 70 și nu creează legătură",
                ImperechereService.Ramas(os, idPltStins) == 70m
                && os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentStingatorId == ntcAvir.ID) == 0);

            // Corolarul F1: proiecția de REST chiar întorcea picioarele de virament când
            // e filtrată pe un cont propriu (PLT → contrapartida e primitorul, INC →
            // predatorul). Contractul (F7-D7) susținea că e imposibil structural; nu e.
            var restPeCasa = ImperecheriProiectii.DocumenteCuRest(os, casaVir.ID).ToList();
            var restPeBanca = ImperecheriProiectii.DocumenteCuRest(os, bancaVir.ID).ToList();
            var restPePartener = ImperecheriProiectii.DocumenteCuRest(os, partenerVir.ID).ToList();
            s.Check("Review advers F1 (corolar): `DocumenteCuRest` NU mai întoarce picioarele de virament pe niciunul dintre conturile proprii care le sunt contrapartidă (INC pe casă, PLT pe bancă)",
                !restPeCasa.Any(r => r.DocumentId == idVirInc)
                && !restPeBanca.Any(r => r.DocumentId == idVirPlt)
                // Proba NU e vacuă: ambele picioare îndeplinesc TOATE celelalte criterii
                // ale proiecției (Operat, Rest = totalul lor pe veci) — le scoate exclusiv
                // anti-join-ul nou.
                && ImperechereService.Ramas(os, idVirPlt) == 0m
                && ImperechereService.Ramas(os, idVirInc) == 0m);
            s.Check("Review advers F1 (corolar, nonregresie): anti-join-ul e ȚINTIT — plata obișnuită rămâne candidat pe contrapartida ei, cu restul calculat de proiecție egal cu al serviciului",
                restPePartener.Count(r => r.DocumentId == idPltStins) == 1
                && restPePartener.Single(r => r.DocumentId == idPltStins).Rest == ImperechereService.Ramas(os, idPltStins));

            // Legătura se desface înaintea scenariului de anulare/storno: gardianul de
            // imperecheri (31d) ar refuza anularea plății martor, iar curățenia finală
            // are nevoie de documente fără dependenți.
            foreach (var idStingere in os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == ntcAvir.ID).Select(i => i.ID).ToArray())
                ImperechereService.Sterge(os, idStingere);
            MotorOperare.AnuleazaOperarea(os, ntcAvir);
            ComenziDocument.Sistem(os).AnuleazaOperarea(idPltStins);

            // ── (g) Ancora 7: anulare, regenerare, storno ───────────────────────────
            ComenziDocument.Sistem(os).AnuleazaOperarea(idVirInc);
            s.Check("F7-D9 ancora 7: anularea laturii pereche o readuce în Draft, fără rânduri proprii",
                TrezorerieApply.Citeste<Incasare>(os, idVirInc) is { Stare: "Draft" }
                && CubScena.FaraNote(os, idVirInc));
            ComenziDocument.Sistem(os).AnuleazaOperarea(idVirPlt);
            s.Check("F7-D9 ancora 7: anularea SURSEI șterge draftul autogenerat (gardienii de grup existenți — artefact al operării, se regenerează la re-operare)",
                TrezorerieApply.Citeste<Incasare>(os, idVirInc) == null
                && TrezorerieApply.Citeste<Plata>(os, idVirPlt) is { Stare: "Draft", Copii.Count: 0 }
                && CubScena.FaraNote(os, idVirPlt));

            var rezReoperare = ComenziDocument.Sistem(os).Opereaza(idVirPlt);
            var idVirInc2 = rezReoperare.ConexId ?? Guid.Empty;
            s.Check("Risc pin-uit (ping-pong): re-operarea sursei după anulare regenerează EXACT o latură pereche, nu una în plus",
                idVirInc2 != Guid.Empty && idVirInc2 != idVirInc
                && TrezorerieApply.Citeste<Plata>(os, idVirPlt).Copii.Count == 1);
            ComenziDocument.Sistem(os).Opereaza(idVirInc2);
            // Piciorul-copil se stornează ÎNTÂI: gardianul de grup refuză stornarea
            // sursei cât timp copilul e Operat (nu și când e Stornat).
            ComenziDocument.Sistem(os).Storneaza(idVirInc2, new DateOnly(2026, 4, 20));
            ComenziDocument.Sistem(os).Storneaza(idVirPlt, new DateOnly(2026, 4, 20));
            var idsPicioare2 = new List<Guid> { idVirPlt, idVirInc2 };
            var noteStornoCub = CubScena.Note(os, [.. idsPicioare2]).Where(p => p.Storno).ToList();
            s.Check("F7-D9 ancora 7: storno pe AMBELE picioare — câte un rând invers marcat Storno la data stornării, corespondența neschimbată, 581 rămâne închis la 0",
                TrezorerieApply.Citeste<Plata>(os, idVirPlt) is { Stare: "Stornat" }
                && TrezorerieApply.Citeste<Incasare>(os, idVirInc2) is { Stare: "Stornat" }
                && noteStornoCub.Count == 4
                && noteStornoCub.All(p => p.Valoare == -500m && p.Data == new DateOnly(2026, 4, 20))
                && CubScena.SoldCont(os, cont581.ID, null, idsPicioare2) == 0m);

            CurataAvir(os);
            s.Check("Curățenie finală felia virament (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajAvir))
                && !os.GetObjectsQuery<TipMaterial>().Any(t => t.Cod.StartsWith(MarcajAvir))
                && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(MarcajAvir)));
        }
    }
}
