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

// ============ Scenariul e2e: compensarea (NTC pe rolul de stingător) ============
// Decizia 48b: Compensarea din 1C (869/an) e o notă contabilă operată care
// STINGE — 401 = 4111 pe același partener stinge simultan datoria și creanța.
// Acoperă: rolul de stingător declarat polimorf (facturile NU pot stinge),
// plafonul PER CONTRAPARTIDĂ (nota dublă stinge de două ori), invariantul de
// contrapartidă reformulat (repartitorii expliciți ai liniilor), refuzul pe
// notă needitată/nepotrivită și gardianul de anulare cât există stingeri.
static class E2eCompensare {
    public static void Ruleaza(Suita s) {
        {
            const string MarcajCmp = "E2E-CMP";

            void CurataCmp(IObjectSpace os) {
                // F13-D2: curățenia de scenă = purjă FIZICĂ (`Purja.cs`), nu `os.Delete`.
                var pj = new Purja(os);
                var repIds = os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajCmp)).Select(r => r.ID).ToList();
                var docs = os.GetObjectsQuery<Document>()
                    .Where(d => repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId)).ToList();
                // Nota are laturi INTERNE (partenerul stă pe linii), deci nu e prinsă
                // de filtrul pe laturi — se adaugă prin numărul propriu.
                docs.AddRange(os.GetObjectsQuery<NotaContabila>().Where(d => d.Numar.StartsWith(MarcajCmp)));
                var docIds = docs.Select(d => d.ID).Distinct().ToList();
                pj.Adauga(os.GetObjectsQuery<Imperechere>()
                    .Where(i => docIds.Contains(i.DocumentStingatorId) || docIds.Contains(i.DocumentId)).ToList());
                pj.Adauga(os.GetObjectsQuery<RegistruContabil>().Where(r => r.DocumentId != null && docIds.Contains(r.DocumentId.Value)).ToList());
                pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => docIds.Contains(d.DocumentId)).ToList());
                pj.Adauga(docs);
                pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(MarcajCmp)).ToList());
                pj.Executa();
            }

            using (var os = s.Provider.CreateObjectSpace()) {
                CurataCmp(os);

                var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
                var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
                var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
                // Piciorul de trezorerie al matricei de perechi (review F2), de la
                // sfârșitul blocului: aceeași scenă, aceeași contrapartidă.
                var casaCmp = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
                var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
                var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
                var cont401 = os.FirstOrDefault<Cont>(c => c.Simbol == "401");
                var cont4111 = os.FirstOrDefault<Cont>(c => c.Simbol == "4111");

                s.Check("Seed privat: partenerul generic de retail CF (surogatul RVA — decizia 48b), fără ContImplicit propriu",
                    os.FirstOrDefault<Partener>(p => p.Cod == "CF") is { Denumire: "CONSUMATOR FINAL", ContImplicitId: null });
                TipTva Tva(string cod) => os.FirstOrDefault<TipTva>(t => t.Cod == cod);
                s.Check("Seed privat: cotele istorice N19/TI19 (importul 1C aduce un an dinaintea Legii 141/2025), cu conturile 4426/4427 și SAF-T null",
                    Tva("N19") is { Cota: 19m, Regim: RegimTva.Normal, CodSafTLivrare: null, CodSafTAchizitie: null }
                    && Tva("N19").ContTvaDeductibilId != null && Tva("N19").ContTvaColectatId != null
                    && Tva("TI19") is { Cota: 19m, Regim: RegimTva.TaxareInversa, CodSafTLivrare: null }
                    && Tva("TI19").ContTvaDeductibilId != null);

                // Partenerul X e furnizor pe o factură și client pe alta — cazul real
                // al compensării; Y e martorul care nu apare pe notă.
                var partenerX = os.CreateObject<Partener>();
                partenerX.Cod = MarcajCmp + "-X";
                partenerX.Denumire = "Partener compensare X";
                var partenerY = os.CreateObject<Partener>();
                partenerY.Cod = MarcajCmp + "-Y";
                partenerY.Denumire = "Partener compensare Y";
                os.CommitChanges();

                var fct = os.CreateObject<FacturaIntrare>();
                fct.Numar = MarcajCmp + "-FF";
                fct.Data = new DateOnly(2026, 5, 4);
                fct.Predator = partenerX;
                fct.Primitor = mag1;
                var linieFct = os.CreateObject<FacturaIntrareDetaliu>();
                linieFct.Document = fct;
                linieFct.TipMaterial = tip628;
                linieFct.Cantitate = 1m;
                linieFct.PretUnitar = 100m;

                var fcl = os.CreateObject<FacturaIesire>();
                fcl.Data = new DateOnly(2026, 5, 5);
                fcl.Predator = sediu;
                fcl.Primitor = partenerX;
                var linieFcl = os.CreateObject<FacturaIesireDetaliu>();
                linieFcl.Document = fcl;
                linieFcl.TipMaterial = tip704;
                linieFcl.Cantitate = 1m;
                linieFcl.PretUnitar = 100m;

                var fclY = os.CreateObject<FacturaIesire>();
                fclY.Data = new DateOnly(2026, 5, 5);
                fclY.Predator = sediu;
                fclY.Primitor = partenerY;
                var linieFclY = os.CreateObject<FacturaIesireDetaliu>();
                linieFclY.Document = fclY;
                linieFclY.TipMaterial = tip704;
                linieFclY.Cantitate = 1m;
                linieFclY.PretUnitar = 100m;
                os.CommitChanges();
                MotorOperare.Opereaza(os, fct);
                MotorOperare.Opereaza(os, fcl);
                MotorOperare.Opereaza(os, fclY);

                // Nota de compensare: laturi interne (partenerul stă pe LINIE, nu pe
                // latură — validarea NTC o cere), 401 = 4111 pe X, valoare parțială.
                NotaContabila NotaCompensare(decimal valoare, Repartitor repartitor) {
                    var n = os.CreateObject<NotaContabila>();
                    n.Numar = MarcajCmp + "-C" + valoare;
                    n.Data = new DateOnly(2026, 5, 6);
                    n.Predator = sediu;
                    n.Primitor = sediu;
                    var linie = os.CreateObject<NotaContabilaDetaliu>();
                    linie.Document = n;
                    linie.TipMaterial = tipTrz;
                    linie.Descriere = "Compensare " + repartitor.Cod;
                    linie.ContDebit = cont401;
                    linie.ContCredit = cont4111;
                    linie.RepartitorDebit = repartitor;
                    linie.RepartitorCredit = repartitor;
                    linie.Valoare = valoare;
                    os.CommitChanges();
                    return n;
                }

                var ntcDraft = NotaCompensare(60m, partenerX);
                s.CheckRefuza("Nota NEOPERATĂ nu stinge (invariantul „ambele operate” neatins)",
                    () => ImperechereService.Imperecheaza(os, ntcDraft, fct, 10m));
                s.CheckRefuza("Factura NU e stingător — rolul e declarat de tip (CapacitateStingere), nu de FK",
                    () => ImperechereService.Imperecheaza(os, fcl, fct, 10m));

                // 82: capacitatea manuală + proveniența nu înscriu NTC în
                // stingerea automată. O înscriere accidentală ar consuma plafonul.
                ntcDraft.Autogenerat = true;
                ntcDraft.DocumentSursa = fct;
                os.CommitChanges();
                MotorOperare.Opereaza(os, ntcDraft);
                s.Check("82: nota cu sursă și capacitate de stingere NU împerechează automat",
                    ntcDraft.CapacitateStingere(os)?.Count > 0
                    && !os.GetObjectsQuery<Imperechere>().Any(i => i.DocumentStingatorId == ntcDraft.ID));
                // Restul scenei continuă pe nota manuală originală.
                ntcDraft.Autogenerat = false;
                ntcDraft.DocumentSursa = null;
                os.CommitChanges();
                var ntc = ntcDraft;
                var noteCompensareCub = CubScena.Note(os, ntc.ID).Where(p => !p.Storno).ToList();
                s.Check("Nota de compensare operată: 401 = 4111 pe X (60), fără stoc și fără TVA",
                    ntc.Stare == StareDocument.Operat
                    && noteCompensareCub.Count(p => p.Debit && p.Cont == cont401.ID && p.Valoare == 60m) == 1
                    && noteCompensareCub.Nota(cont401.ID, cont4111.ID, 60m));

                s.CheckRefuza("Contrapartida stinsă trebuie să apară pe liniile notei (factura lui Y nu se compensează cu nota lui X)",
                    () => ImperechereService.Imperecheaza(os, ntc, fclY, 10m));

                ImperechereService.Imperecheaza(os, ntc, fct, 60m);
                ImperechereService.Imperecheaza(os, ntc, fcl, 60m);
                s.Check("Nota DUBLĂ stinge de două ori (60 pe datoria X + 60 pe creanța X); ambele facturi rămân cu 40",
                    os.GetObjectsQuery<Imperechere>().Count(i => i.DocumentStingatorId == ntc.ID) == 2
                    && ImperechereService.Ramas(os, fct.ID) == 40m
                    && ImperechereService.Ramas(os, fcl.ID) == 40m);

                s.CheckRefuza("Plafonul PER CONTRAPARTIDĂ e consumat (2 × 60): a treia stingere se refuză",
                    () => {
                        var altaFcl = os.CreateObject<FacturaIesire>();
                        altaFcl.Data = new DateOnly(2026, 5, 7);
                        altaFcl.Predator = sediu;
                        altaFcl.Primitor = partenerX;
                        var l = os.CreateObject<FacturaIesireDetaliu>();
                        l.Document = altaFcl;
                        l.TipMaterial = tip704;
                        l.Cantitate = 1m;
                        l.PretUnitar = 50m;
                        os.CommitChanges();
                        MotorOperare.Opereaza(os, altaFcl);
                        ImperechereService.Imperecheaza(os, ntc, altaFcl, 10m);
                    });

                s.CheckRefuza("Gardianul de anulare acoperă nota pe rolul de stingător (coloana DocumentStingator)",
                    () => MotorOperare.AnuleazaOperarea(os, ntc));

                foreach (var idStingere in os.GetObjectsQuery<Imperechere>().Where(i => i.DocumentStingatorId == ntc.ID).Select(i => i.ID).ToArray())
            ImperechereService.Sterge(os, idStingere);
                MotorOperare.AnuleazaOperarea(os, ntc);
                s.Check("După ștergerea stingerilor nota se anulează normal (link fără registre proprii — 31d)",
                    ntc.Stare == StareDocument.Draft
                    && CubScena.FaraNote(os, ntc.ID));

                // ═══ F19-D16 (review F2): acoperire pe SPAȚIUL de perechi ═══
                // Lecția scrisă în contract: „un invariant de ne-regresie se măsoară
                // pe SPAȚIUL de cazuri, nu pe suita existentă". Prima versiune a lui
                // F19-D16 a trecut cu toate verificările verzi, iar review-ul a găsit
                // pe HTTP că trezoreria ÎȘI SCHIMBĂ comportamentul (`PLT → FCL` și
                // `INC → FCT` primesc acum 422) — fiindcă nicio verificare nu atingea
                // acele perechi. Aici se măsoară perechile (stingător × stins) pe
                // AMBELE verdicte, cu documente reale pe ACEEAȘI contrapartidă:
                // contrapartida comună nu mai e explicația refuzului, sensul e.
                //
                // Verdictul REFUZ pe `PLT → FCL` / `INC → FCT` e corect contabil (a
                // credita 401 nu stinge o factură de furnizor; a debita 4111 nu
                // stinge una de client) — vechea permisivitate era o gaură din
                // aceeași familie. Pe Flax cade exact 1 imperechere / 700,00 din
                // 46.056 (F19-D16, pin-ul „Adăugat după review").
                DocumentTrezorerie TrzCmp(bool incasare, decimal valoare, DateOnly data) {
                    DocumentTrezorerie d = incasare ? os.CreateObject<Incasare>() : os.CreateObject<Plata>();
                    d.Data = data;
                    d.Predator = incasare ? (Repartitor)partenerX : casaCmp;
                    d.Primitor = incasare ? (Repartitor)casaCmp : partenerX;
                    d.TipInstrument = TipInstrumentPlata.Chitanta;
                    var l = os.CreateObject<DocumentTrezorerieDetaliu>();
                    l.Document = d;
                    l.TipMaterial = tipTrz;
                    l.Valoare = valoare;
                    os.CommitChanges();
                    MotorOperare.Opereaza(os, d);
                    return d;
                }
                var pltCmp = TrzCmp(incasare: false, 30m, new DateOnly(2026, 5, 8));
                var pltCmp2 = TrzCmp(incasare: false, 30m, new DateOnly(2026, 5, 8));
                var incCmp = TrzCmp(incasare: true, 30m, new DateOnly(2026, 5, 9));
                var incCmp2 = TrzCmp(incasare: true, 30m, new DateOnly(2026, 5, 9));
                s.Check("F19-D16 (spațiul de perechi) — PREMISA: cei patru stingători de trezorerie și cele două facturi "
                    + "stau pe ACEEAȘI contrapartidă (X), toate operate cu rest > 0; deci orice refuz de mai jos e al "
                    + "SENSULUI, nu al contrapartidei comune",
                    new[] { pltCmp, pltCmp2, incCmp, incCmp2 }.All(d =>
                        d.CapacitateStingere(os).ContainsKey(partenerX.ID)
                        && ImperechereService.Ramas(os, d.ID) == 30m)
                    && ImperechereService.Ramas(os, fct.ID) == 100m
                    && ImperechereService.Ramas(os, fcl.ID) == 100m);
                // Fiecare pereche se măsoară IZOLAT: acceptarea se șterge imediat, ca
                // plafonul consumat de o pereche să nu explice verdictul următoarei.
                void Accepta(string nume, Document stingator, Document stins) {
                    var imp = ImperechereService.Imperecheaza(os, stingator, stins, 10m);
                    s.Check(nume, imp.Suma == 10m);
                    ImperechereService.Sterge(os, imp.ID);
                }
                Accepta("F19-D16 (perechi, TRECE): PLT → FCT — plata are plafon pe `Datorie`, factura furnizorului "
                    + "consumă `Datorie` (comportament NESCHIMBAT)", pltCmp, fct);
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ — SCHIMBARE de comportament, declarată): PLT → FCL, aceeași "
                    + "contrapartidă și rest suficient pe amândouă — a credita 4111 nu stinge o factură de client. "
                    + "Trecea înainte de F19-D16; pe Flax e 1 imperechere / 700,00 din 46.056",
                    () => ImperechereService.Imperecheaza(os, pltCmp, fcl, 10m));
                Accepta("F19-D16 (perechi, TRECE): INC → FCL — încasarea are plafon pe `Creanta`, factura clientului "
                    + "consumă `Creanta` (comportament NESCHIMBAT)", incCmp, fcl);
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ — SCHIMBARE de comportament, declarată): INC → FCT — a debita 401 "
                    + "din încasare nu stinge o factură de furnizor",
                    () => ImperechereService.Imperecheaza(os, incCmp, fct, 10m));
                s.CheckRefuza("SC-CIT-63/F19-D16 (101): PLT pe 401 → INC pe 4111 refuză conturile diferite",
                    () => ImperechereService.Imperecheaza(os, pltCmp, incCmp, 10m));
                s.CheckRefuza("SC-CIT-63/F19-D16 (101): INC pe 4111 → PLT pe 401 refuză conturile diferite",
                    () => ImperechereService.Imperecheaza(os, incCmp, pltCmp, 10m));
                s.Check("SC-CIT-63: ambele refuzuri păstrează partidele la 30",
                    ImperechereService.Ramas(os, pltCmp.ID) == 30m && ImperechereService.Ramas(os, incCmp.ID) == 30m);
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ — regulă PREEXISTENTĂ, nu a sensului): PLT → PLT",
                    () => ImperechereService.Imperecheaza(os, pltCmp, pltCmp2, 10m));
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ — regulă PREEXISTENTĂ): INC → INC",
                    () => ImperechereService.Imperecheaza(os, incCmp, incCmp2, 10m));
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ): FCT nu e stingător (rolul e al TIPULUI — 48b), deși poartă "
                    + "contrapartida", () => ImperechereService.Imperecheaza(os, fct, pltCmp, 10m));
                s.CheckRefuza("F19-D16 (perechi, REFUZĂ): FCL nu e stingător",
                    () => ImperechereService.Imperecheaza(os, fcl, incCmp, 10m));
                s.Check("F19-D16 (perechi): măsurarea n-a lăsat nicio imperechere în urmă — fiecare verdict a fost "
                    + "izolat, deci niciun refuz nu se explică prin plafonul consumat de perechea dinainte",
                    !os.GetObjectsQuery<Imperechere>().Any(i =>
                        i.DocumentStingatorId == pltCmp.ID || i.DocumentStingatorId == incCmp.ID)
                    && ImperechereService.Ramas(os, fct.ID) == 100m
                    && ImperechereService.Ramas(os, fcl.ID) == 100m);

                CurataCmp(os);
                s.Check("Curățenie finală compensare (fără reziduuri e2e)",
                    !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(MarcajCmp))
                    && !os.GetObjectsQuery<NotaContabila>().Any(d => d.Numar.StartsWith(MarcajCmp)));
            }
        }
    }
}
