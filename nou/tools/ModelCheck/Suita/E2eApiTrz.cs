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

// ============ Felia Api Trz: PLT/INC prin nucleul generic (F3-D1/D5/D9) ============
// Marcaj E2E-API-TRZ. Blocul e2e de deasupra probează MOTORUL pe trezorerie;
// acesta probează CONTRACTUL API-ului peste el — aceleași obiecte de seed
// (CASA/TREZ, Tipul tehnic TRZ), dar cu documentele construite exclusiv din
// WriteDto și citite exclusiv prin proiecții plate. Ce exersează în plus față de
// feliile BTR/FCT:
//   * `Numar` SERVER-OWNED (PLT/INC au PoliticaNumerotare) — nici măcar nu e în
//     WriteDto; apare abia după operare, din serie. Invers față de FCT;
//   * `Valoare` CULEASĂ pe linie (trezoreria n-are `PregatesteOperare`);
//   * nucleul GENERIC pe `T : DocumentTrezorerie` — o singură implementare, două
//     rute, cu filtrarea pe tip în SQL (`Citeste<Plata>` nu vede o
//     încasare);
//   * `TipInstrument` ca STRING pe sârmă, în ambele sensuri (round-trip, CASE în
//     listă, refuz de domeniu la valoare necunoscută);
//   * F3-D5: parametrii plății automate în DTO-urile FCT → plata autogenerată în
//     `Copii[]` → citită pe ruta ei → operată → imperecherea automată.
static class E2eApiTrz {
    public static void Ruleaza(Suita s) {
        const string MarcajApiTrz = "E2E-ATRZ";

        // Review F3-D5a: `TrezorerieApply.Lista` traduce `TipInstrument` în string cu un
        // CASE care are ULTIMA ramură fallback („Chitanta") — un membru NOU de enum ar
        // apărea tăcut ca „Chitanta" în grilă. Gardianul (analogul scării numerice): dacă
        // enum-ul crește, testul pică zgomotos și cere actualizarea CASE-ului + a
        // parse-ului `ApiEnum` + a etichetelor.
        s.Check("Gardian F3-D5a: TipInstrumentPlata are exact membrii mapați în CASE-ul din Lista",
            Enum.GetNames<TipInstrumentPlata>().OrderBy(n => n)
                .SequenceEqual(new[] { "Cec", "Chitanta", "DispozitieCasa", "OrdinPlata" }));

        void CurataApiTrz(IObjectSpace os) {
            // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
            var pj = new Purja(os);
            // Toate documentele blocului ating cel puțin un repartitor marcat (inclusiv
            // plata autogenerată: TREZ → furnizorul marcat), deci marcajul de repartitor
            // e cheia de curățenie — ca la CurataTrz.
            var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiTrz)).Select(r => r.ID).ToList();
            var docs = os.GetObjectsQuery<Document>()
                .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
            var docIds = docs.Select(d => d.ID).ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
            foreach (var doc in docs.OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajApiTrz)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod == MarcajApiTrz + "-CE").ToList());
            pj.Executa();
        }

        using (var os = s.Provider.CreateObjectSpace()) {
            CurataApiTrz(os);

            var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
            var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
            var tipServicii = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628.00.00");
            var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
            var trezoreria = os.FirstOrDefault<ContPropriu>(c => c.Cod == "TREZ");
            var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401.01.00");
            var cont411 = os.FirstOrDefault<Cont>(c => c.Simbol == "411.01.01");
            var cont531 = os.FirstOrDefault<Cont>(c => c.Simbol == "531.01.01");
            var cont770 = os.FirstOrDefault<Cont>(c => c.Simbol == "770.00.00");

            var furnizor = os.CreateObject<Partener>();
            furnizor.Cod = MarcajApiTrz + "-FURN";
            furnizor.Denumire = "Furnizor probă felia Api Trz";
            var client = os.CreateObject<Partener>();
            client.Cod = MarcajApiTrz + "-CL";
            client.Denumire = "Client probă felia Api Trz";
            var codEc = os.CreateObject<CodEconomic>();
            codEc.Cod = MarcajApiTrz + "-CE";
            codEc.Denumire = "Cod economic probă felia Api Trz";
            os.CommitChanges();

            // Dry-run-ul își cere ObjectSpace-ul PROPRIU (contractul lui
            // MotorOperare.Valideaza: `PregatesteOperare` SCRIE pe linii).
            IReadOnlyList<string> DryRunTrz(Guid docId) {
                using var osDry = s.Provider.CreateObjectSpace();
                return ComenziDocument.Sistem(osDry).Valideaza(docId);
            }

            // ── (a) Plata culeasă manual ────────────────────────────────────────────
            var writePlt = new TrezorerieWriteDto {
                Data = new DateOnly(2026, 3, 14),
                PredatorId = casa.ID,
                PrimitorId = furnizor.ID,
                TipInstrument = "DispozitieCasa",
                NumarExtras = "EX-API-1",
                DataExtras = new DateOnly(2026, 3, 14),
                Linii = {
                    // Tipul tehnic TRZ e DEFAULT DE CULEGERE în client (F3-D7), nu o
                    // validare de server — de aceea vine prin payload ca oricare altul.
                    new TrezorerieLinieWriteDto {
                        TipMaterialId = tipTrz.ID, Valoare = 150m, CodEconomicId = codEc.ID
                    }
                }
            };
            var idPlt = TrezorerieApply.Aplica<Plata>(os, null, writePlt);
            var plt = TrezorerieApply.Citeste<Plata>(os, idPlt);
            s.Check("Apply PLT creare → header plat; NUMĂRUL rămâne NULL — PLT are PoliticaNumerotare ⇒ server-owned (invers față de FCT, unde e cules)",
                plt != null && plt.Id == idPlt && plt.Stare == "Draft" && plt.Numar == null
                && plt.Data == new DateOnly(2026, 3, 14) && plt.DataOperare == null
                && plt.PredatorId == casa.ID && plt.PredatorDenumire == casa.Denumire
                && plt.PrimitorId == furnizor.ID && plt.PrimitorDenumire == furnizor.Denumire
                && plt.TipInstrument == "DispozitieCasa"
                && plt.NumarExtras == "EX-API-1" && plt.DataExtras == new DateOnly(2026, 3, 14)
                && plt.Total == 150m
                && !plt.Autogenerat && plt.DocumentSursaId == null && plt.DocumentSursaNumar == null
                && plt.DocumentSursaTip == null
                && plt.Copii.Count == 0
                && plt.PoateEdita && plt.PoateOpera && !plt.PoateAnula && !plt.PoateStorna);
            s.Check("Linia PLT: `Valoare` CULEASĂ (trezoreria n-are PregatesteOperare) + dimensiunea frunzei, proiectate plat",
                plt.Linii.Count == 1 && plt.Linii[0].TipMaterialId == tipTrz.ID
                && plt.Linii[0].TipMaterialCod == "TRZ" && plt.Linii[0].Valoare == 150m
                && plt.Linii[0].CodEconomicId == codEc.ID && plt.Linii[0].CodEconomicCod == codEc.Cod
                && plt.Linii[0].SursaFinantareId == null && plt.Linii[0].CodFunctionalId == null
                && plt.Linii[0].ProiectId == null && plt.Linii[0].AngajamentId == null);
            s.Check("Dry-run (Valideaza) pe draftul PLT valid → listă goală", DryRunTrz(idPlt).Count == 0);

            // Reconcilierea colecției + refuzurile de contract (pe calea de ACTUALIZARE,
            // ca un Apply refuzat să nu poată lăsa reziduu în ObjectSpace-ul viu).
            writePlt.Linii[0].Id = plt.Linii[0].Id;
            writePlt.Linii.Add(new TrezorerieLinieWriteDto {
                TipMaterialId = tipTrz.ID, Valoare = 20m, CodEconomicId = codEc.ID
            });
            TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt);
            s.Check("Reconciliere: linia nouă (fără Id) se adaugă, Total urcă la 170",
                TrezorerieApply.Citeste<Plata>(os, idPlt) is { Linii.Count: 2, Total: 170m });
            s.CheckRefuza("Apply cu Id de linie străin → refuz (agregatul nu adoptă linii din alt document)",
                () => TrezorerieApply.Aplica<Plata>(os, idPlt, new TrezorerieWriteDto {
                    Data = writePlt.Data, PredatorId = casa.ID, PrimitorId = furnizor.ID,
                    Linii = { new TrezorerieLinieWriteDto {
                        Id = Guid.NewGuid(), TipMaterialId = tipTrz.ID, Valoare = 1m } }
                }));
            s.CheckRefuza("Apply cu valoare în afara scării numeric(18,2) → refuz de domeniu, nu DbUpdateException",
                () => TrezorerieApply.Aplica<Plata>(s.OsCuGardian(), idPlt, new TrezorerieWriteDto {
                    Data = writePlt.Data, PredatorId = casa.ID, PrimitorId = furnizor.ID,
                    Linii = { new TrezorerieLinieWriteDto {
                        TipMaterialId = tipTrz.ID, Valoare = 1.005m } }
                }));
            writePlt.Linii.RemoveAt(1);
            TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt);
            s.Check("Reconciliere: linia absentă din payload se ȘTERGE; un Apply refuzat n-a lăsat reziduu",
                TrezorerieApply.Citeste<Plata>(os, idPlt) is { Linii.Count: 1, Total: 150m });

            // Laturile NU se verifică la scriere (draftul are voie să fie greșit) — le
            // refuză OPERAREA, prin hook-ul tipului. Proba pe un draft cu laturi inversate.
            var idPltInvers = TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 3, 14),
                PredatorId = furnizor.ID, PrimitorId = casa.ID,
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 10m, CodEconomicId = codEc.ID } }
            });
            s.Check("TipInstrument absent din payload ⇒ OrdinPlata (default-ul convenției F3-D1)",
                TrezorerieApply.Citeste<Plata>(os, idPltInvers).TipInstrument == "OrdinPlata");
            var eroriInvers = DryRunTrz(idPltInvers);
            // Intenția probei rămâne aceeași (Apply acceptă laturile inversate, OPERAREA
            // le refuză), dar de la F7-D3 contul propriu e contrapartidă LEGALĂ pe PLT/
            // INC (viramentul intern) ⇒ „primitorul e casa" nu mai e greșeală în sine;
            // documentul rămâne refuzat pentru PREDATOR (un partener nu poate fi contul
            // din care se plătește). Cuplajul cu natura liniilor nu se aprinde: linia e
            // `TRZ`, iar predatorul-partener face documentul non-virament.
            s.Check("Apply acceptă laturile inversate (validarea lor e a OPERĂRII) — dry-run-ul o raportează ca DATE; de la F7-D3 rămâne DOAR refuzul predatorului (contul propriu e contrapartidă legală)",
                eroriInvers.Count == 1
                && eroriInvers.Any(e => e.StartsWith(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PredatorNepotrivit, StringComparison.Ordinal))
                && !eroriInvers.Any(e => e.Contains(Atlas.Conta.BackOffice.Module.Declaratii.CoduriRefuz.PrimitorNepotrivit)));
            TrezorerieApply.Sterge<Plata>(os, idPltInvers);
            s.Check("Sterge pe draft: documentul și liniile lui dispar împreună",
                TrezorerieApply.Citeste<Plata>(os, idPltInvers) == null
                && !os.GetObjectsQuery<DocumentDetaliu>().Any(d => d.DocumentId == idPltInvers));

            // ── Comanda pe plată: numărul din serie + contarea din laturi ───────────
            var rezPlt = ComenziDocument.Sistem(os).Opereaza(idPlt);
            s.Check("ComenziDocument.Opereaza pe PLT → Operat, fără conex/secundar",
                rezPlt.StareNoua == StareDocument.Operat && rezPlt.ConexId == null && rezPlt.Mesaje.Count == 0);
            plt = TrezorerieApply.Citeste<Plata>(os, idPlt);
            s.Check("După operare numărul vine DIN SERIE (PLT-), nu din payload; affordances inversate",
                plt.Stare == "Operat" && plt.Numar?.StartsWith("PLT-") == true && plt.DataOperare != null
                && !plt.PoateEdita && !plt.PoateOpera && plt.PoateAnula && plt.PoateStorna);
            var notePltCub = CubScena.Note(os, idPlt);
            s.Check("PLT contează din laturi: 401.01.00 (fallback furnizor) = 531.01.01 (ContImplicit CASA), 150",
                notePltCub.Count == 2 && notePltCub.Nota(cont401.ID, cont531.ID, 150m));
            s.Check("PLT nu mișcă stoc",
                CubScena.FaraStoc(os, idPlt));
            // --- NUC-PLT-API (B-D5, pas 4): declarantul pe documentul operat prin usa API ---
            ProbeNucleu.Proba(os, s.Check, "NUC-PLT-API", [os.GetObjectByKey<Plata>(idPlt)]);
            s.CheckRefuza("Apply peste PLT Operat → refuz de DOMENIU (pre-check, înaintea gardianului generic)",
                () => TrezorerieApply.Aplica<Plata>(os, idPlt, writePlt));
            s.CheckRefuza("Sterge peste PLT Operat → același refuz de domeniu",
                () => TrezorerieApply.Sterge<Plata>(os, idPlt));

            var listaPlt = TrezorerieApply.Lista<Plata>(os).Where(x => x.Id == idPlt).ToList();
            s.Check("Lista PLT → Stare ȘI TipInstrument ca text (CASE în SQL), Autogenerat, Total din agregat",
                listaPlt.Count == 1 && listaPlt[0].Stare == "Operat"
                && listaPlt[0].TipInstrument == "DispozitieCasa" && !listaPlt[0].Autogenerat
                && listaPlt[0].Numar == plt.Numar && listaPlt[0].Total == 150m
                && listaPlt[0].PredatorDenumire == casa.Denumire
                && listaPlt[0].PrimitorDenumire == furnizor.Denumire);
            s.Check("Lista PLT → filtrarea/sortarea se traduc în SQL peste proiecție (sondă: filtru pe enum-ul textual + sort + take)",
                TrezorerieApply.Lista<Plata>(os).Where(x => x.TipInstrument == "DispozitieCasa")
                    .OrderByDescending(x => x.Data).Take(1).ToList().Count == 1);

            // ── (b) Încasarea: același nucleu, laturi oglindite ─────────────────────
            var idInc = TrezorerieApply.Aplica<Incasare>(os, null, new TrezorerieWriteDto {
                Data = new DateOnly(2026, 3, 15),
                PredatorId = client.ID, PrimitorId = casa.ID,
                TipInstrument = "Chitanta",
                Linii = { new TrezorerieLinieWriteDto {
                    TipMaterialId = tipTrz.ID, Valoare = 80m, CodEconomicId = codEc.ID } }
            });
            s.Check("Genericul filtrează pe TIP (discriminatorul `ClrType`): Citeste<Plata> pe un id de încasare → null (o rută nu adoptă documentele celeilalte)",
                TrezorerieApply.Citeste<Plata>(os, idInc) == null
                && TrezorerieApply.Citeste<Incasare>(os, idInc) != null
                && !TrezorerieApply.Lista<Plata>(os).Any(x => x.Id == idInc)
                && TrezorerieApply.Lista<Incasare>(os).Any(x => x.Id == idInc));
            ComenziDocument.Sistem(os).Opereaza(idInc);
            var inc = TrezorerieApply.Citeste<Incasare>(os, idInc);
            var noteIncApiCub = CubScena.Note(os, idInc);
            s.Check("Apply<Incasare> + operare: număr din seria proprie (INC-), contare oglindită 531.01.01 = 411.01.01 (fallback client), 80",
                inc.Stare == "Operat" && inc.Numar?.StartsWith("INC-") == true
                && inc.TipInstrument == "Chitanta" && inc.Total == 80m
                && noteIncApiCub.Count == 2 && noteIncApiCub.Nota(cont531.ID, cont411.ID, 80m));
            ProbeNucleu.Proba(os, s.Check, "NUC-INC-API", [os.GetObjectByKey<Incasare>(idInc)]);

            // ── (c) Enum-ul pe sârmă: valoare necunoscută = refuz de domeniu ────────
            s.CheckRefuza("TipInstrument necunoscut → refuz cu valorile valide enumerate (nu conversie tăcută la 0, nu ArgumentException)",
                () => TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                    Data = new DateOnly(2026, 3, 14), PredatorId = casa.ID, PrimitorId = furnizor.ID,
                    TipInstrument = "Bilet la ordin" }));
            s.CheckRefuza("TipInstrument dat ca NUMĂR („3”) → tot refuz: contractul e pe nume, nu pe ordinea membrilor",
                () => TrezorerieApply.Aplica<Plata>(os, null, new TrezorerieWriteDto {
                    Data = new DateOnly(2026, 3, 14), PredatorId = casa.ID, PrimitorId = furnizor.ID,
                    TipInstrument = "3" }));
            s.Check("Un Apply refuzat pe enum NU lasă document orfan în ObjectSpace (parse înaintea CreateObject)",
                os.GetObjectsQuery<Plata>().Count(p => p.PrimitorId == furnizor.ID) == 1);

            // ── (d) F3-D5: plata automată, cap-coadă prin API ───────────────────────
            var writeFct = new FacturaIntrareWriteDto {
                Numar = MarcajApiTrz + "-FF1",
                Data = new DateOnly(2026, 3, 16),
                PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                // Grupul DECONT_*, ridicat din excluderea F2.
                GenereazaPlata = true,
                PlataContPropriuId = trezoreria.ID,
                PlataNumar = "OP-API-9",
                PlataData = new DateOnly(2026, 3, 17),
                PlataTipInstrument = "Cec",
                // Doar linii de SERVICIU ⇒ conexul NIR nu se generează (n-are linii
                // eligibile), deci singurul copil e SECUNDARUL — plata.
                Linii = { new FacturaIntrareLinieWriteDto {
                    TipMaterialId = tipServicii.ID, Cantitate = 1m, PretUnitar = 100m,
                    CodEconomicId = codEc.ID } }
            };
            var idFctPlata = FacturaIntrareApply.Aplica(os, null, writeFct);
            var fctCitit = FacturaIntrareApply.Citeste(os, idFctPlata);
            s.Check("F3-D5: parametrii plății automate fac ROUND-TRIP prin DTO-urile FCT (bifă, cont propriu + denumire, număr, dată, instrument ca STRING)",
                fctCitit.GenereazaPlata && fctCitit.PlataContPropriuId == trezoreria.ID
                && fctCitit.PlataContPropriuDenumire == trezoreria.Denumire
                && fctCitit.PlataNumar == "OP-API-9" && fctCitit.PlataData == new DateOnly(2026, 3, 17)
                && fctCitit.PlataTipInstrument == "Cec");
            s.CheckRefuza("Instrument de plată necunoscut pe FCT → același refuz (helper de parse COMUN cu trezoreria)",
                () => FacturaIntrareApply.Aplica(os, idFctPlata, new FacturaIntrareWriteDto {
                    Numar = writeFct.Numar, Data = writeFct.Data,
                    PredatorId = furnizor.ID, PrimitorId = mag1.ID, PlataTipInstrument = "OP" }));
            var idContPropriuInexistent = Guid.NewGuid();
            s.CheckRefuza("Cont propriu inexistent pe FCT → refuz cu mesaj de domeniu (nu violare de FK)",
                () => FacturaIntrareApply.Aplica(os, idFctPlata, new FacturaIntrareWriteDto {
                    Numar = writeFct.Numar, Data = writeFct.Data,
                    PredatorId = furnizor.ID, PrimitorId = mag1.ID,
                    GenereazaPlata = true, PlataContPropriuId = idContPropriuInexistent }));

            var rezFct = ComenziDocument.Sistem(os).Opereaza(idFctPlata);
            s.Check("Operarea FCT (numai servicii ⇒ fără NIR conex) întoarce SECUNDARUL: draftul de plată",
                rezFct.StareNoua == StareDocument.Operat && rezFct.ConexId != null);
            var idPlataAuto = rezFct.ConexId.Value;
            fctCitit = FacturaIntrareApply.Citeste(os, idFctPlata);
            s.Check("Citeste.Copii → plata autogenerată: Tip „PLT” din ancora TipDocument, numărul CULES pe factură, Draft, Autogenerat",
                fctCitit.Copii.Count == 1 && fctCitit.Copii[0].Id == idPlataAuto
                && fctCitit.Copii[0].Tip == "PLT" && fctCitit.Copii[0].Numar == "OP-API-9"
                && fctCitit.Copii[0].Stare == "Draft" && fctCitit.Copii[0].Autogenerat);

            var plataAuto = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
            s.Check("Citeste<Plata> pe copil: header din grupul DECONT_* (TREZ→furnizor, Cec, data plății) + link înapoi la factură prin numărul ei",
                plataAuto != null && plataAuto.Autogenerat && plataAuto.Stare == "Draft"
                && plataAuto.Numar == "OP-API-9" && plataAuto.Data == new DateOnly(2026, 3, 17)
                && plataAuto.TipInstrument == "Cec"
                && plataAuto.PredatorId == trezoreria.ID && plataAuto.PrimitorId == furnizor.ID
                && plataAuto.DocumentSursaId == idFctPlata
                && plataAuto.DocumentSursaNumar == writeFct.Numar
                && plataAuto.DocumentSursaTip == "FCT"
                && plataAuto.Total == 121m);
            s.Check("Liniile plății autogenerate păstrează Tipul SURSEI (628, nu TRZ — F3-D7 e convenție de client), valoarea BRUTĂ și dimensiunea clonată",
                plataAuto.Linii.Count == 1 && plataAuto.Linii[0].TipMaterialId == tipServicii.ID
                && plataAuto.Linii[0].TipMaterialCod == "628.00.00"
                && plataAuto.Linii[0].Valoare == 121m
                && plataAuto.Linii[0].CodEconomicId == codEc.ID);

            ComenziDocument.Sistem(os).Opereaza(idPlataAuto);
            var impAuto = os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == idPlataAuto).ToList();
            s.Check("Operarea plății autogenerate prin ComenziDocument → imperecherea automată pe BRUT (121), factura stinsă integral",
                impAuto.Count == 1 && impAuto[0].DocumentId == idFctPlata && impAuto[0].Suma == 121m
                && impAuto[0].Autogenerat && ImperechereService.Ramas(os, idFctPlata) == 0m);
            var plataOperata = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
            s.Check("Plata autogenerată operată: numărul rămâne CEL CULES pe factură (AsignaNumar onorează un număr existent — nu se consumă serie), contare 401 = 770",
                plataOperata.Stare == "Operat" && plataOperata.Numar == "OP-API-9"
                && CubScena.Note(os, idPlataAuto).Nota(cont401.ID, cont770.ID, 121m));
            ProbeNucleu.Proba(os, s.Check, "NUC-PLT-API-FCT", [os.GetObjectByKey<Plata>(idPlataAuto)]);
            s.Check("Affordance onestă pe FCT (F2-D5): copilul PLT operat blochează anularea/stornarea facturii",
                FacturaIntrareApply.Citeste(os, idFctPlata) is { PoateAnula: false, PoateStorna: false });

            // ── (e) F3-D2/F3-D3: affordances ONESTE + stingerile prin API ───────────
            // Perechea de mai jos era MARCAJUL limitei pasului 1 (affordance optimist
            // lângă refuzul motorului), scrisă ca să pice când intră fix-ul. Aici e
            // adusă la noul adevăr: DTO-ul spune același lucru ca gardianul.
            s.Check("F3-D2: affordance ONESTĂ — plata cu imperechere NU se mai anunță anulabilă (oglinda lui VerificaFaraImperecheri)",
                !plataOperata.PoateAnula && !plataOperata.PoateStorna);
            s.CheckRefuza("…iar motorul chiar refuză: anularea plății cu imperechere",
                () => ComenziDocument.Sistem(os).AnuleazaOperarea(idPlataAuto));
            s.Check("F3-D2: numerele stingerii pe ReadDto-ul trezoreriei (Total/Asignat/Ramas din serviciu — TS nu le calculează)",
                plataOperata.Total == 121m && plataOperata.Asignat == 121m && plataOperata.Ramas == 0m);

            // Panoul de stingeri, citit din AMBELE capete ale ACELEIAȘI legături.
            var stingeriPlataAuto = ImperechereApply.Stingeri(os, idPlataAuto);
            s.Check("StingeriDto pe plata autogenerată: rolul de STINGĂTOR, celălalt document tipat „FCT” din ancoră, marcat Autogenerat",
                stingeriPlataAuto is { Total: 121m, Asignat: 121m, Ramas: 0m }
                && stingeriPlataAuto.Imperecheri.Count == 1
                && stingeriPlataAuto.Imperecheri[0].EsteStingator
                && stingeriPlataAuto.Imperecheri[0].CelalaltDocumentId == idFctPlata
                && stingeriPlataAuto.Imperecheri[0].CelalaltTip == "FCT"
                && stingeriPlataAuto.Imperecheri[0].CelalaltNumar == writeFct.Numar
                && stingeriPlataAuto.Imperecheri[0].Suma == 121m
                && stingeriPlataAuto.Imperecheri[0].Autogenerat);
            var stingeriFct = ImperechereApply.Stingeri(os, idFctPlata);
            s.Check("StingeriDto pe factură: ACELAȘI rând, cu rolul INVERSAT (EsteStingator false) și celălalt document tipat „PLT”",
                stingeriFct is { Total: 121m, Asignat: 121m, Ramas: 0m }
                && stingeriFct.Imperecheri.Count == 1
                && stingeriFct.Imperecheri[0].Id == stingeriPlataAuto.Imperecheri[0].Id
                && !stingeriFct.Imperecheri[0].EsteStingator
                && stingeriFct.Imperecheri[0].CelalaltDocumentId == idPlataAuto
                && stingeriFct.Imperecheri[0].CelalaltTip == "PLT"
                && stingeriFct.Imperecheri[0].CelalaltNumar == "OP-API-9");
            s.Check("Stingeri pe un id inexistent → null (nu excepție)",
                ImperechereApply.Stingeri(os, Guid.NewGuid()) == null);
            s.Check("F3-D4: documentul STINS INTEGRAL nu e candidat — filtrul Rest > 0 se aplică DUPĂ calcul, în SQL",
                !ImperecheriProiectii.DocumenteCuRest(os).Any(r => r.DocumentId == idFctPlata));

            // Ștergerea link-ului e LIBERĂ (31d) și deblochează anularea — exact fluxul
            // clientului: după DELETE, butonul Anulează redevine activ.
            ImperechereApply.Sterge(os, stingeriPlataAuto.Imperecheri[0].Id);
            var plataFaraLink = TrezorerieApply.Citeste<Plata>(os, idPlataAuto);
            s.Check("Sterge imperecherea → restul revine pe ambele documente ȘI affordance-ul se redeschide",
                plataFaraLink is { PoateAnula: true, PoateStorna: true, Asignat: 0m, Ramas: 121m }
                && ImperechereService.Ramas(os, idFctPlata) == 121m
                && ImperechereApply.Stingeri(os, idFctPlata).Imperecheri.Count == 0);
            s.CheckRefuza("Sterge pe o imperechere inexistentă → refuz de domeniu (nu NullReference)",
                () => ImperechereApply.Sterge(os, Guid.NewGuid()));

            // Creare prin API pe lanțul MANUAL: plata culeasă (150, casa → furnizor)
            // stinge parțial factura ACELUIAȘI furnizor (121).
            var creata = ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 100m });
            var panouPlt = ImperechereApply.Stingeri(os, idPlt);
            var panouFct = ImperechereApply.Stingeri(os, idFctPlata);
            s.Check("ImperechereApply.Creeaza → link ne-autogenerat; restul scade pe AMBELE părți (plata 50, factura 21)",
                creata.DocumentStingatorId == idPlt && creata.DocumentId == idFctPlata
                && creata.Suma == 100m && !creata.Autogenerat
                && panouPlt is { Total: 150m, Asignat: 100m, Ramas: 50m }
                && panouFct is { Total: 121m, Asignat: 100m, Ramas: 21m });
            s.Check("Rolurile în panou: plata e STINGĂTOR (celălalt FCT), factura e stinsă (celălalt PLT)",
                panouPlt.Imperecheri.Single() is { EsteStingator: true, CelalaltTip: "FCT", Autogenerat: false }
                && panouFct.Imperecheri.Single() is { EsteStingator: false, CelalaltTip: "PLT" });

            // Invarianții NU se rescriu în adaptor — refuzurile vin din
            // `ImperechereService.ValideazaCreare`, prin aceeași cale ca UI-ul.
            s.CheckRefuza("Creeaza peste restul stingibil al facturii (21 rămași, se cer 40) → refuz",
                () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                    DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 40m }));
            s.CheckRefuza("Creeaza fără contrapartidă comună (încasarea clientului × factura furnizorului) → refuz",
                () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                    DocumentStingatorId = idInc, DocumentId = idFctPlata, Suma = 10m }));
            s.CheckRefuza("Creeaza Plata↔Plata (același sens) → refuz",
                () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                    DocumentStingatorId = idPlt, DocumentId = idPlataAuto, Suma = 10m }));
            s.CheckRefuza("Creeaza cu document inexistent → mesaj de DOMENIU la graniță (traducerea cheie → entitate, abaterea de la 42b)",
                () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                    DocumentStingatorId = idPlt, DocumentId = Guid.NewGuid(), Suma = 10m }));
            s.CheckRefuza("Creeaza cu sumă în afara scării numeric(18,2) → refuz de domeniu, nu rotunjire tăcută",
                () => ImperechereApply.Creeaza(os, new ImperechereWriteDto {
                    DocumentStingatorId = idPlt, DocumentId = idFctPlata, Suma = 1.005m }));
            s.Check("Un Creeaza refuzat n-a lăsat link fantomă (validarea precede CreateObject, în serviciu)",
                ImperechereApply.Stingeri(os, idFctPlata).Imperecheri.Count == 1);

            // ── (f) F3-D4: proiecția de rest, în oglindă cu serviciul ───────────────
            var cuRest = ImperecheriProiectii.DocumenteCuRest(os).ToList();
            s.Check("F3-D9: proiecția DocumenteCuRest == ImperechereService.Ramas pe FIECARE rând (un al doilea adevăr ar fi un defect)",
                cuRest.Count > 0 && cuRest.All(r => r.Rest == ImperechereService.Ramas(os, r.DocumentId)));
            var idsRdcOperate = os.GetObjectsQuery<ReturClient>()
                .Where(d => d.Stare == StareDocument.Operat).Select(d => d.ID).ToList();
            // F27-D7: RDC a INTRAT în uniune (a șasea ramură). Totalul lui nu se mai
            // agregă din toate liniile — e totalul partidelor din cub, prin `LiniiCreanta` —,
            // deci proiecția nu mai poate diverge de serviciu, iar amânarea e închisă.
            s.Check($"F3-D4/F27-D7: uniunea acoperă EXACT cele șase tipuri concrete, RDC inclus (totalul lui e cel scris "
                + $"prin `LiniiCreanta`, nu Σ tuturor liniilor) — {idsRdcOperate.Count} retururi operate în bază",
                cuRest.All(r => r.Tip is "FCT" or "FCL" or "PLT" or "INC" or "DEC" or "RDC"));
            var randFct = cuRest.Single(r => r.DocumentId == idFctPlata);
            var randPlt = cuRest.Single(r => r.DocumentId == idPlt);
            s.Check("F3-D4: rândurile poartă tipul, contrapartida (latura partener, nu contul propriu) și cele trei numere",
                randFct is { Tip: "FCT", Total: 121m, Asignat: 100m, Rest: 21m }
                && randFct.ContrapartidaId == furnizor.ID && randFct.ContrapartidaDenumire == furnizor.Denumire
                && randPlt is { Tip: "PLT", Total: 150m, Asignat: 100m, Rest: 50m }
                && randPlt.ContrapartidaId == furnizor.ID);
            var cuRestFurnizor = ImperecheriProiectii.DocumenteCuRest(os, furnizor.ID).ToList();
            s.Check("F3-D4: filtrul pe contrapartidă dă candidații unui singur partener (factura + plățile furnizorului, fără încasarea clientului)",
                cuRestFurnizor.Count > 0 && cuRestFurnizor.All(r => r.ContrapartidaId == furnizor.ID)
                && cuRestFurnizor.Any(r => r.DocumentId == idFctPlata)
                && cuRestFurnizor.Any(r => r.DocumentId == idPlt)
                && !cuRestFurnizor.Any(r => r.DocumentId == idInc));
            // ═══ F19-D16 (review F3): afordanța de SENS a panourilor CLASICE ═══
            // Sensul nu se deduce în TS (42c): vine server-computed pe `StingeriDto`,
            // din hook-ul polimorf, iar clientul îl pasează ca atare pe parametrul
            // `sens` al proiecției. Fără el panourile clasice filtrau DOAR pe TIP —
            // măsurat pe baza Privat, 87 din 353 de contrapartide au documente pe AMBELE
            // sensuri, deci un panou de Încasare oferea zeci de facturi de furnizor cu
            // buton „Stinge” care duc garantat la 422.
            var panouSensFct = ImperechereApply.Stingeri(os, idFctPlata);
            var panouSensPlt = ImperechereApply.Stingeri(os, idPlt);
            s.Check("F19-D16 (F3): `StingeriDto.SensCandidati` = `Opus(SensDeStins)`, o singură formulă pentru ambele "
                + "roluri — factura de furnizor (consumă `Datorie`) cere candidați cu literalul `Creanta` (plăți), plata "
                + "(consumă `Creanta`) cere `Datorie` (facturi de furnizor, deconturi)",
                panouSensFct.SensCandidati == nameof(SensStingere.Creanta)
                && panouSensPlt.SensCandidati == nameof(SensStingere.Datorie));
            var candFctSens = ImperecheriProiectii
                .DocumenteCuRest(os, furnizor.ID, Enum.Parse<SensStingere>(panouSensFct.SensCandidati)).ToList();
            var candPltSens = ImperecheriProiectii
                .DocumenteCuRest(os, furnizor.ID, Enum.Parse<SensStingere>(panouSensPlt.SensCandidati)).ToList();
            s.Check("F19-D16 (F3, cusătura MĂSURATĂ — riscul 2 al contractului, capătul panourilor VECHI): filtrată cu "
                + "`SensCandidati`, proiecția oferă DOAR documente pe care serviciul le acceptă — fiecare candidat al "
                + "facturii chiar are plafon pe sensul pe care factura îl consumă (`Datorie`), iar factura nu se oferă pe "
                + "sine; simetric, panoul plății oferă factura și niciun document de trezorerie de același sens",
                candFctSens.Count > 0
                && candFctSens.All(r => r.Tip == "PLT")
                && candFctSens.All(r => os.GetObjectByKey<Document>(r.DocumentId).CapacitateStingere(os) is { } cap
                    && cap.TryGetValue(furnizor.ID, out var plafon) && plafon[SensStingere.Datorie] > 0m)
                && candFctSens.Any(r => r.DocumentId == idPlt)
                && !candFctSens.Any(r => r.DocumentId == idFctPlata)
                && candPltSens.Any(r => r.DocumentId == idFctPlata)
                && !candPltSens.Any(r => r.DocumentId == idPlt));

            s.Check("F3-D4: proiecția rămâne IQueryable — filtrarea/sortarea/paginarea se traduc în SQL peste uniune (sondă: filtru pe tip + sort + take)",
                ImperecheriProiectii.DocumenteCuRest(os).Where(r => r.Tip == "PLT")
                    .OrderByDescending(r => r.Rest).Take(1).ToList().Count == 1);

            ImperechereApply.Sterge(os, creata.Id);
            s.Check("Sterge link-ul manual → restul revine integral pe ambele (150 / 121)",
                ImperechereService.Ramas(os, idPlt) == 150m
                && ImperechereService.Ramas(os, idFctPlata) == 121m);

            CurataApiTrz(os);
            s.Check("Curățenie finală felia Api Trz (fără reziduuri e2e)",
                !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajApiTrz))
                && !os.GetObjectsQuery<FacturaIntrare>().Any(d => d.Numar.StartsWith(MarcajApiTrz)));
        }
    }
}
