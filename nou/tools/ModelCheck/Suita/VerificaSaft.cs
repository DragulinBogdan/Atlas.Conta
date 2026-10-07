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

// ============ Felia 16, pas 2: regulile + proiecția SAF-T — D16-V2 ============
// Ce probează: (a) funcțiile PURE ale legii (`SaftReguli`) pe toate ramurile lor,
// fără bază — deci identic pe ambele profiluri; (b) proiecția `SaftProiectii.SaftPeCub`
// pe o scenă privată completă (FCT+NIR, FCL+DSC, RDC, RLF, PLT cu imperechere,
// INC, NTC, DEC, storno, factură în valută, tip de TVA fără cod SAF-T), cu
// CUSĂTURILE la cent; (c) `Neaplicabil` pe profilul bugetar.
//
// Luna scenei e AUGUST 2026, ca la D300/D394 — și scena rulează DUPĂ ele, care își
// purjează documentele; premisa („luna e goală") se MĂSOARĂ, nu se presupune.
static class VerificaSaft {
    public static void Ruleaza(Suita suita, bool privat) {
        const string Marcaj = "E2E-SAFT";
        // Simbolul contului de probă al fixului F8 — în afara CSV-ului planului OMFP
        // și în afara oricărui prefix din listele de rol.
        const string SimbolContProbaSaft = "ZZZ" + Marcaj;
        using var os = suita.Provider.CreateObjectSpace();

        const int an = 2026, luna = 8;
        var pStart = new DateOnly(an, luna, 1);
        var pEnd = new DateOnly(an, luna, 31);
        var dataCreare = new DateOnly(2026, 9, 5);

        // ══════════ D16-V2 (a): funcțiile pure ale legii, fără bază ══════════
        suita.Check("D16-V2 `CuiValid` — cheia 753217532 aliniată la dreapta, (Σ×10) mod 11 cu 10⇒0: CUI-ul real "
            + "`4221306` (Ministerul Finanțelor, exemplul din §B.4) trece, cu și fără prefixul RO/R; `12345674` trece, "
            + "`12345678` NU (cifra de control e 4); lungimile în afara [2,10] și literele cad",
            SaftReguli.CuiValid("4221306") && SaftReguli.CuiValid("RO4221306") && SaftReguli.CuiValid("R4221306")
            && SaftReguli.CuiValid("12345674") && SaftReguli.CuiValid("33333338") && SaftReguli.CuiValid("22222229")
            && SaftReguli.CuiValid("66666665")
            && !SaftReguli.CuiValid("12345678") && !SaftReguli.CuiValid("4221307")
            && !SaftReguli.CuiValid("1") && !SaftReguli.CuiValid("12345678901")
            && !SaftReguli.CuiValid("ABC") && !SaftReguli.CuiValid("") && !SaftReguli.CuiValid(null));
        suita.Check("D16-V2 `CnpValid` — cheia 279146358279, Σ mod 11 cu 10⇒1, prima cifră ≠ 0, exact 13 cifre",
            SaftReguli.CnpValid("1800101123450")
            && !SaftReguli.CnpValid("1800101123456") && !SaftReguli.CnpValid("0800101123450")
            && !SaftReguli.CnpValid("180010112345") && !SaftReguli.CnpValid("18001011234A0")
            && !SaftReguli.CnpValid(null));

        (string Id, FelIdSaft Fel) Id(TipPersoana tip, string tara, bool inreg, string cui, string cod, bool cnp = false) =>
            SaftReguli.IdPartener(tip, tara, inreg, cui, cod, Guid.Empty, cnp);
        suita.Check("D16-V2 `IdPartener` §B.4 — `00`+CUI cere un CUI ROMÂNESC VALID (nu doar flag-ul): RO înregistrat și RO "
            + "neînregistrat cu CUI valid ⇒ 00; străinul cu cod RO (art. 316) ⇒ tot 00, cu prefixul RO tăiat; "
            + "UE cu cod propriu ⇒ 01/05 + ISO2 + cod FĂRĂ litera de țară duplicată; non-UE ⇒ 02/06; "
            + "`EL` e tratat ca stat membru (grafia VIES a Greciei), dar identificatorul iese cu grafia ISO `GR` "
            + "(fixul F10: DUK acceptă ambele, deci alegerea o face fișierul, ca să aibă O grafie per țară)",
            Id(TipPersoana.Juridica, "RO", true, "RO33333338", "F1") == ("0033333338", FelIdSaft.CuiRoman)
            && Id(TipPersoana.Juridica, "RO", false, "33333338", "F2") == ("0033333338", FelIdSaft.CuiRoman)
            && Id(TipPersoana.Juridica, "DE", true, "RO66666665", "F3") == ("0066666665", FelIdSaft.CuiRoman)
            && Id(TipPersoana.Juridica, "DE", true, "DE987654321", "F4") == ("01DE987654321", FelIdSaft.UeInregistrat)
            && Id(TipPersoana.Juridica, "DE", false, "DE123456789", "F5") == ("05DE123456789", FelIdSaft.UeNeinregistrat)
            && Id(TipPersoana.Juridica, "EL", false, "EL123456789", "F6") == ("05GR123456789", FelIdSaft.UeNeinregistrat)
            && Id(TipPersoana.Juridica, "GR", true, "EL987654321", "F6b") == ("01GR987654321", FelIdSaft.UeInregistrat)
            && SaftReguli.CodTaraSaft("EL") == "GR" && SaftReguli.CodTaraSaft("gr") == "GR"
            && SaftReguli.CodTaraSaft("de") == "DE" && SaftReguli.CodTaraSaft(null) == "RO"
            && Id(TipPersoana.Juridica, "US", true, "US-777", "F7") == ("02US777", FelIdSaft.NonUeInregistrat)
            && Id(TipPersoana.Juridica, "US", false, "US-999", "F8") == ("06US999", FelIdSaft.NonUeNeinregistrat));
        suita.Check("D16-V2 `IdPartener` — CNP-ul e OPT-IN pe bază (`Societate.RaporteazaCnp`): PF cu CNP valid iese `04`+cod "
            + "intern când baza nu-l raportează și `03`+CNP când îl raportează; PF fără cod ⇒ `04`+cod; codul intern se "
            + "filtrează la [A-Za-z0-9]; fără niciun cod ⇒ `04`+Id fără cratime; românul cu CUI INVALID cade tot pe `04` "
            + "(prefixul 00 ar fi respins de validator)",
            Id(TipPersoana.Fizica, "RO", false, "1800101123450", "PF-1") == ("04PF1", FelIdSaft.CodIntern)
            && Id(TipPersoana.Fizica, "RO", false, "1800101123450", "PF-1", cnp: true) == ("031800101123450", FelIdSaft.Cnp)
            && Id(TipPersoana.Fizica, "RO", false, null, "PF.0", cnp: true) == ("04PF0", FelIdSaft.CodIntern)
            && Id(TipPersoana.Juridica, "RO", true, "12345678", "X-1") == ("04X1", FelIdSaft.CodIntern)
            && SaftReguli.IdPartener(TipPersoana.Juridica, "RO", false, null, null,
                    new Guid("11111111-2222-3333-4444-555555555555"), false).Id == "04111111112222333344445555555555");
        suita.Check("D16-V2 `IdSocietate` / `RegistrationNumberSocietate` — DOUĂ reguli peste aceeași cifră: identificatorul de "
            + "partener e `00`+CUI, iar antetul cere `RO`+CUI la plătitor și CUI-ul gol la neplătitor (mesajele "
            + "validatorului, §B.4)",
            SaftReguli.IdSocietate("RO12345674", "RO") == "0012345674"
            && SaftReguli.RegistrationNumberSocietate("RO12345674", "RO", true) == "RO12345674"
            && SaftReguli.RegistrationNumberSocietate("12345674", "RO", false) == "12345674"
            && SaftReguli.IdSocietate(null, "RO") == null);
        suita.Check("D16-V2 `InvoiceType` (cele 6 coduri admise, S.I.9): `381` = „factură cu semnul minus INDIFERENT de motiv” "
            + "⇒ și stornoul meta-operației, și retururile (care sunt storno prin construcție, 46a); altfel `380`",
            SaftReguli.InvoiceType(storno: false, esteRetur: false) == "380"
            && SaftReguli.InvoiceType(storno: true, esteRetur: false) == "381"
            && SaftReguli.InvoiceType(storno: false, esteRetur: true) == "381"
            && SaftReguli.InvoiceType(storno: true, esteRetur: true) == "381");
        suita.Check("D16-V2 `MetodaPlata` — tuplele `PaymentMethod ↔ PaymentMechanism` pe care validatorul le IMPUNE: "
            + "numerar (dispoziție de casă, chitanță) ⇒ 01/10; ordin de plată ⇒ 03/42 (transfer bancar); cec ⇒ 03/20",
            SaftReguli.MetodaPlata(TipInstrumentPlata.DispozitieCasa) == ("01", "10")
            && SaftReguli.MetodaPlata(TipInstrumentPlata.Chitanta) == ("01", "10")
            && SaftReguli.MetodaPlata(TipInstrumentPlata.OrdinPlata) == ("03", "42")
            && SaftReguli.MetodaPlata(TipInstrumentPlata.Cec) == ("03", "20"));
        suita.Check("D16-V2 `SimbolSaft` — cifrele fără puncte, FĂRĂ nicio tăiere de segment (`302.02.00` ⇒ `3020200`); "
            + "`TipCont` — D/C/B ⇒ Activ/Pasiv/Bifuncțional, necunoscut ⇒ Bifuncțional + `FunctieCunoscuta` fals",
            SaftReguli.SimbolSaft("302.02.00") == "3020200" && SaftReguli.SimbolSaft("4111") == "4111"
            && SaftReguli.SimbolSaft(null) == null
            && SaftReguli.TipCont("D") == "Activ" && SaftReguli.TipCont("c") == "Pasiv"
            && SaftReguli.TipCont("B") == "Bifunctional" && SaftReguli.TipCont("X") == "Bifunctional"
            && SaftReguli.TipCont(null) == "Bifunctional"
            && SaftReguli.FunctieCunoscuta("B") && !SaftReguli.FunctieCunoscuta("X") && !SaftReguli.FunctieCunoscuta(null));
        suita.Check("D16-V2 `RegimFiscalPartener` (`Nomenclator_Regim_fiscal`): `TaxRegistration` există DOAR pe partenerii "
            + "`00` înregistrați — 100040 la TVA la încasare, 100010 altfel; pe `01`/`04` lipsește cu totul",
            SaftReguli.RegimFiscalPartener(FelIdSaft.CuiRoman, true, true) == "100040"
            && SaftReguli.RegimFiscalPartener(FelIdSaft.CuiRoman, true, false) == "100010"
            && SaftReguli.RegimFiscalPartener(FelIdSaft.CuiRoman, false, false) == null
            && SaftReguli.RegimFiscalPartener(FelIdSaft.UeInregistrat, true, false) == null
            && SaftReguli.RegimFiscalPartener(FelIdSaft.CodIntern, true, true) == null);

        // ══════════ D16-V2 (c): bugetarul — `Neaplicabil`, fără nicio interogare ══════════
        if (!privat) {
            var premisa = FctBugetaraOperata.Ruleaza(suita, os, Marcaj + "-BUG", new DateOnly(an, luna, 12));
            var gol = SaftProiectii.SaftPeCub(os, an, luna, dataCreare);
            Console.WriteLine($"     MĂSURAT (D16-V2 bugetar): „{gol.Neaplicabil}”; "
                + $"documentul scenei {premisa.Numar} {premisa.Stare} pe {premisa.Data}; societate completată: "
                + $"{!string.IsNullOrWhiteSpace(os.GetObjectsQuery<Societate>().First().CodFiscal)}.");
            suita.Check("D16-V2 (bugetar) SAF-T e NEAPLICABIL: planul instituțiilor publice nu e printre cele 12 "
                + "`TaxAccountingBasis`, deci proiecția întoarce un DTO GOL cu MOTIV — fără antet, fără secțiuni, "
                + "fără avertismente — și se oprește ÎNAINTE de orice interogare pe registre, deși luna are o FCT "
                + "OPERATĂ de scenă (rulează și pe o bază cu societatea necompletată)",
                gol.Neaplicabil != null && gol.Neaplicabil.Contains("bugetar")
                && gol.Header == null && gol.Conturi.Count == 0 && gol.Clienti.Count == 0 && gol.Furnizori.Count == 0
                && gol.Jurnale.Count == 0 && gol.FacturiEmise.Count == 0 && gol.FacturiPrimite.Count == 0
                && gol.Plati.Count == 0 && gol.Produse.Count == 0 && gol.Taxe.Count == 0 && gol.Unitati.Count == 0
                && gol.Neincluse.Count == 0 && gol.Avertismente.Count == 0
                && gol.Rezumat.Tranzactii == 0 && gol.Rezumat.RanduriRegistru == 0
                && premisa.Stare == StareDocument.Operat && premisa.Data.Year == an && premisa.Data.Month == luna);
            // Refuzul trebuie să fie AL SCRIITORULUI, nu doar al ecranului: un fișier
            // gol semnat cu CUI-ul cuiva ar fi o declarație falsă, nu o listă goală.
            string mesajScriere = null;
            try {
                using var flux = new MemoryStream();
                SaftXml.Scrie(gol, flux);
            }
            catch (InvalidOperationException e) {
                mesajScriere = e.Message;
            }
            Console.WriteLine($"     MĂSURAT (D16-V3 bugetar): `SaftXml.Scrie` → „{mesajScriere ?? "<A SCRIS FIȘIERUL>"}”.");
            suita.Check("D16-V3 (bugetar) `SaftXml.Scrie` REFUZĂ o declarație `Neaplicabil`, cu motivul proiecției — "
                + "traducerea în 422 rămâne a apelantului REST, dar decizia e a scriitorului, nu a ecranului",
                mesajScriere != null && mesajScriere.Contains("bugetar"));
            // Serializarea DTO-ului `Neaplicabil` (pasul 4b): ușa JSON îl traduce în
            // 422, dar forma trebuie să fie scriibilă oricum — un `Neaplicabil` care
            // ar arunca la serializare ar da 500 în loc de refuzul motivat.
            VerificaSaftJson.Ruleaza(suita, gol, "bugetar, Neaplicabil");
            PurjaFctBugetara.Ruleaza(suita, os, Marcaj + "-BUG");
            return;
        }

        // ══════════ D16-V2 (b): scena privată ══════════
        var mag1 = os.FirstOrDefault<Gestiune>(g => g.Cod == "MAG1");
        var sediu = os.FirstOrDefault<UnitateInterna>(u => u.Cod == "SEDIU");
        var banca = os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA");
        var casa = os.FirstOrDefault<ContPropriu>(c => c.Cod == "CASA");
        var tip628 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "628");
        var tip371 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "371");
        var tip704 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "704");
        var tip707 = os.FirstOrDefault<TipMaterial>(t => t.Cod == "707");
        var tipTrz = os.FirstOrDefault<TipMaterial>(t => t.Cod == "TRZ");
        var n21s = os.FirstOrDefault<TipTva>(t => t.Cod == "N21");
        Cont ContSimb(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol);
        var judetCj = os.FirstOrDefault<Judet>(j => j.Cod == "RO-CJ");
        var umBucata = os.FirstOrDefault<UnitateMasura>(u => u.Cod == "H87");

        var societate = os.GetObjectsQuery<Societate>().First();
        // Antetul e SINGLETONUL bazei — nu se purjează, se pune la loc (ca la D16-V1).
        var socInainte = (societate.Denumire, societate.CodFiscal, societate.InregistratTva, societate.Tara,
            societate.Strada, societate.Numar, societate.Localitate, societate.CodPostal, societate.JudetId,
            societate.ContactNume, societate.ContactPrenume, societate.Telefon, societate.Email,
            societate.ContBancarId, societate.RaporteazaCnp);
        var ibanInainte = banca.Iban;

        void Curata() {
            var pj = new Purja(os);
            var repIds = os.GetObjectsQuery<Repartitor>()
                .Where(r => r.Cod.StartsWith(Marcaj)).Select(r => r.ID).ToList();
            var idsSursa = os.GetObjectsQuery<Document>()
                .Where(d => d.Numar.StartsWith(Marcaj)
                    || repIds.Contains(d.PredatorId) || repIds.Contains(d.PrimitorId))
                .Select(d => d.ID).ToList();
            var ids = idsSursa.Concat(os.GetObjectsQuery<Document>()
                .Where(d => d.DocumentSursaId != null && idsSursa.Contains(d.DocumentSursaId.Value))
                .Select(d => d.ID).ToList()).Distinct().ToList();
            pj.Adauga(os.GetObjectsQuery<Imperechere>()
                .Where(i => ids.Contains(i.DocumentId) || ids.Contains(i.DocumentStingatorId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruTva>().Where(r => ids.Contains(r.DocumentId)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruContabil>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<RegistruStoc>()
                .Where(r => r.DocumentId != null && ids.Contains(r.DocumentId.Value)).ToList());
            pj.Adauga(os.GetObjectsQuery<DocumentDetaliu>().Where(d => ids.Contains(d.DocumentId)).ToList());
            foreach (var doc in os.GetObjectsQuery<Document>().Where(d => ids.Contains(d.ID)).ToList()
                         .OrderByDescending(d => d.DocumentSursaId != null))
                pj.Adauga(doc);
            pj.Adauga(os.GetObjectsQuery<Lot>().Where(l => l.Produs.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Produs>().Where(p => p.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<TipTva>().Where(t => t.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<CodEconomic>().Where(c => c.Cod.StartsWith(Marcaj)).ToList());
            pj.Adauga(os.GetObjectsQuery<Repartitor>().Where(r => r.Cod.StartsWith(Marcaj)).ToList());
            // Contul de probă al fixului F8 (adăugat pe bază, în afara planului).
            pj.Adauga(os.GetObjectsQuery<Cont>()
                .Where(c => c.Simbol == SimbolContProbaSaft).ToList());
            pj.Executa();
        }
        void RestaureazaSocietatea() {
            var s = os.GetObjectsQuery<Societate>().First();
            (s.Denumire, s.CodFiscal, s.InregistratTva, s.Tara, s.Strada, s.Numar, s.Localitate, s.CodPostal,
                s.JudetId, s.ContactNume, s.ContactPrenume, s.Telefon, s.Email, s.ContBancarId, s.RaporteazaCnp) = socInainte;
            os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA").Iban = ibanInainte;
            os.CommitChanges();
        }
        Curata();

        suita.Check("D16-V2 premisă: luna scenei e goală înainte de scenă (D300/D394 și-au purjat documentele) — altfel "
            + "cifrele exacte de mai jos ar fi măsurate peste conținut străin",
            CubScena.NoteIntre(os, pStart, pEnd) == 0 && CubScena.FapteIntre(os, pStart, pEnd).Count == 0);

        // ---------------- Societatea raportoare ----------------
        banca.Iban = "RO49AAAA1B31007593840000";
        societate.Denumire = "Atlas Probă SAF-T SRL";
        societate.CodFiscal = "12345674";
        societate.InregistratTva = true;
        societate.Tara = "RO";
        societate.Strada = "Str. Probei";
        societate.Numar = "1";
        societate.Localitate = "Cluj-Napoca";
        societate.CodPostal = "400000";
        societate.Judet = judetCj;
        societate.ContactNume = "Popescu";
        societate.ContactPrenume = "Ion";
        societate.Telefon = "0264000000";
        societate.Email = "probe@atlas.test";
        societate.ContBancar = banca;
        societate.RaporteazaCnp = false;
        os.CommitChanges();

        // ---------------- Nomenclatoarele scenei ----------------
        Partener Part(string sufix, string denumire, string cui, TipPersoana tipPersoana = TipPersoana.Juridica,
            bool inregistrat = true, bool cuAdresa = true) {
            var p = os.CreateObject<Partener>();
            p.Cod = Marcaj + sufix; p.Denumire = denumire; p.CodFiscal = cui;
            p.TipPersoana = tipPersoana; p.Tara = "RO"; p.InregistratTva = inregistrat;
            if (cuAdresa) {
                p.Strada = "Str. Partenerului"; p.Numar = "2"; p.Localitate = "Cluj-Napoca";
                p.CodPostal = "400001"; p.Judet = judetCj;
            }
            return p;
        }
        var furnizor = Part("-FURN", "Furnizor SAF-T SRL", "RO33333338");
        // Clientul fără localitate ⇒ `City` obligatoriu ⇒ avertisment `AdresaIncompleta`.
        var client = Part("-CL", "Client SAF-T SRL", "22222229", cuAdresa: false);
        var pf = Part("-PF", "Ionescu Maria", "1800101123450", TipPersoana.Fizica, inregistrat: false);
        // Fixul F6: partener plătit pe un cont FĂRĂ rol de terț (462 „Creditori
        // diverși"). `ContImplicit` bate fallback-ul politicii de trezorerie, deci
        // rândul plății iese `462 = 5121` — niciun `AccountID` de declarat în master
        // files, deci plata trebuie să cadă în `Neincluse`, nu să scrie un element gol.
        var partener462 = Part("-462", "Creditor divers SAF-T SRL", "98765438");
        partener462.ContImplicit = ContSimb("462");
        // Fixul F2: cei DOI parteneri ai compensării `401 = 4111`. Sunt parteneri
        // PROPRII probei (nu furnizorul și clientul de mai sus), ca apartenența la
        // `Suppliers`/`Customers` să fie dată EXCLUSIV de rândul ăsta.
        var compA = Part("-CMP-A", "Compensare A SRL", "76543210");
        var compB = Part("-CMP-B", "Compensare B SRL", "24680249");
        var angajat = os.CreateObject<Angajat>();
        angajat.Cod = Marcaj + "-ANG"; angajat.Denumire = "Titular decont SAF-T";

        Produs CreeazaProdusSaft(string sufix, string denumire, string codNc, UnitateMasura um, string umText) {
            var p = os.CreateObject<Produs>();
            p.Cod = Marcaj + sufix; p.Denumire = denumire; p.UM = umText;
            p.TipMaterial = tip371; p.CodNc = codNc; p.UnitateMasura = um;
            return p;
        }
        // Codul NC e REAL (`01012100`, cai de reproducție de rasă pură): validatorul
        // îl caută în nomenclatorul NC8 al anului, deci un „12345678” inventat pică.
        // Modelul NU are nomenclatorul (doar forma, 8 cifre — restanța numită în
        // contract); proba de aici e cea care ține minte că validarea de conținut e a
        // ANAF-ului.
        var produsA = CreeazaProdusSaft("-A", "Marfă SAF-T A", "01012100", umBucata, "BUC");
        var produsB = CreeazaProdusSaft("-B", "Marfă SAF-T B", null, null, "navete");

        // O dimensiune CULEASĂ, ca `Analysis`/`AnalysisTypeTable` să aibă ce declara
        // (la privat dimensiunile sunt opționale, deci scena și-o aduce pe a ei).
        var codEconomic = os.CreateObject<CodEconomic>();
        codEconomic.Cod = Marcaj + "-CE"; codEconomic.Denumire = "Cod economic de probă SAF-T";

        os.CommitChanges();

        // ---------------- Documentele lunii ----------------
        var dNir = new DateOnly(an, luna, 2);
        var dFct = new DateOnly(an, luna, 3);
        var dFcl = new DateOnly(an, luna, 10);
        var dTrz = new DateOnly(an, luna, 15);
        var dStorno = new DateOnly(an, luna, 25);

        // NIR manual: lotul produsului B (pentru retur).
        var nirB = os.CreateObject<NIR>();
        nirB.Numar = Marcaj + "-NIR-B"; nirB.Data = dNir; nirB.Predator = furnizor; nirB.Primitor = mag1;
        var linNirB = os.CreateObject<DocumentDetaliu>();
        linNirB.Document = nirB; linNirB.TipMaterial = tip371; linNirB.Cantitate = 10m; linNirB.Valoare = 60m;
        var lotB = linNirB.CreeazaLot(os, produsB, mag1);
        os.CommitChanges();
        MotorOperare.Opereaza(os, nirB);
        os.CommitChanges();

        // FCT: o linie de SERVICIU (628 = 401, postează) + o linie de STOC (recepția
        // contează pe NIR-ul conex — 26a, deci pe factură are DOAR rândul de TVA).
        var fct = os.CreateObject<FacturaIntrare>();
        fct.Numar = Marcaj + "-FCT"; fct.Data = dFct; fct.Predator = furnizor; fct.Primitor = mag1;
        var linFctServiciu = os.CreateObject<FacturaIntrareDetaliu>();
        linFctServiciu.Document = fct; linFctServiciu.TipMaterial = tip628;
        linFctServiciu.Cantitate = 1m; linFctServiciu.PretUnitar = 1000m; linFctServiciu.TipTva = n21s;
        var linFctStoc = os.CreateObject<FacturaIntrareDetaliu>();
        linFctStoc.Document = fct; linFctStoc.TipMaterial = tip371;
        linFctStoc.Cantitate = 10m; linFctStoc.PretUnitar = 30m; linFctStoc.TipTva = n21s;
        linFctStoc.Produs = produsA;
        var lotA = linFctStoc.CreeazaLot(os, produsA, mag1);
        os.CommitChanges();
        var nirConex = MotorOperare.Opereaza(os, fct);
        os.CommitChanges();
        if (nirConex != null) {
            nirConex.Numar = Marcaj + "-NIR-A";
            os.CommitChanges();
            MotorOperare.Opereaza(os, nirConex);
            os.CommitChanges();
        }
        suita.Check("D16-V2 premisă de scenă: FCT a generat NIR-ul conex, iar lotul mărfii s-a finalizat la 30 lei/buc "
            + "(linia de stoc a facturii postează DOAR TVA — recepția contează pe NIR, 26a)",
            nirConex is NIR && nirConex.Stare == StareDocument.Operat && lotA.PretUnitar == 30m);

        // FCT în VALUTĂ (avertisment, nu conversie) — și cu o linie de STOC al cărei
        // NIR conex rămâne DRAFT: lotul se naște, dar nu se recepționează niciodată,
        // deci linia n-are nici rând contabil propriu, nici recepție din care să-și
        // ia contul. E singurul caz în care `InvoiceLine.AccountID` chiar n-are
        // sursă — gardul `FaraContrapartida` trebuie să rămână probat pe el, după ce
        // liniile de stoc NORMALE și-au găsit contul prin NIR-ul operat.
        var fctEur = os.CreateObject<FacturaIntrare>();
        fctEur.Numar = Marcaj + "-FCT-EUR"; fctEur.Data = dFct; fctEur.Predator = furnizor; fctEur.Primitor = mag1;
        fctEur.Valuta = "EUR"; fctEur.Curs = 5m;
        var linFctEur = os.CreateObject<FacturaIntrareDetaliu>();
        linFctEur.Document = fctEur; linFctEur.TipMaterial = tip628;
        linFctEur.Cantitate = 1m; linFctEur.PretUnitar = 100m; linFctEur.TipTva = n21s;
        var linFctEurStoc = os.CreateObject<FacturaIntrareDetaliu>();
        linFctEurStoc.Document = fctEur; linFctEurStoc.TipMaterial = tip371;
        linFctEurStoc.Cantitate = 5m; linFctEurStoc.PretUnitar = 20m; linFctEurStoc.TipTva = n21s;
        linFctEurStoc.Produs = produsA;
        linFctEurStoc.CreeazaLot(os, produsA, mag1);
        os.CommitChanges();
        var nirEurDraft = MotorOperare.Opereaza(os, fctEur);
        os.CommitChanges();

        // ═══ Fixul L1: FCT cu TOATE liniile pe STOC și taxare inversă ═══
        // Cu TVA normal, rândul de TVA al facturii e `4426 = 401`, deci 401-ul apare
        // pe factură chiar și când toate liniile trec pe NIR. Cu TAXARE INVERSĂ (sau
        // scutit, sau intracomunitar) rândul e `4426 = 4427` și 401-ul nu mai apare
        // NICĂIERI pe factură — e pe NIR-ul conex, unde a contat recepția (26a).
        // Căutat doar pe rândurile facturii, `Invoice.AccountID` (M) n-avea sursă și
        // factura întreagă cădea în `Neincluse/ContFaraRol` (84 de facturi reale pe
        // o singură lună a bazei de import).
        var ti21 = os.FirstOrDefault<TipTva>(t => t.Cod == "TI21");
        var fctTi = os.CreateObject<FacturaIntrare>();
        fctTi.Numar = Marcaj + "-FCT-TI"; fctTi.Data = dFct; fctTi.Predator = furnizor; fctTi.Primitor = mag1;
        var linFctTi = os.CreateObject<FacturaIntrareDetaliu>();
        linFctTi.Document = fctTi; linFctTi.TipMaterial = tip371;
        linFctTi.Cantitate = 4m; linFctTi.PretUnitar = 25m; linFctTi.TipTva = ti21;
        linFctTi.Produs = produsA;
        linFctTi.CreeazaLot(os, produsA, mag1);
        os.CommitChanges();
        var nirTiConex = MotorOperare.Opereaza(os, fctTi);
        os.CommitChanges();
        if (nirTiConex != null) {
            nirTiConex.Numar = Marcaj + "-NIR-TI";
            os.CommitChanges();
            MotorOperare.Opereaza(os, nirTiConex);
            os.CommitChanges();
        }

        // ═══ Fixul L2: linie CAPITALIZATĂ (NED21, achiziție fără drept de deducere) ═══
        // `TvaService` pune TVA-ul ÎN cost: `DocumentDetaliu.Valoare` = BRUT (121),
        // `ValoareTva` = 0. `InvoiceLineAmount` trebuie să iasă NET (100), cu
        // `TaxBase 100` / `TaxAmount 21` alături — altfel factura declara brutul ca
        // net și `NetTotal` era umflat cu TVA-ul nedeductibil.
        var ned21 = os.FirstOrDefault<TipTva>(t => t.Cod == "NED21");
        var fctNed = os.CreateObject<FacturaIntrare>();
        fctNed.Numar = Marcaj + "-FCT-NED"; fctNed.Data = dFct; fctNed.Predator = furnizor; fctNed.Primitor = mag1;
        var linFctNed = os.CreateObject<FacturaIntrareDetaliu>();
        linFctNed.Document = fctNed; linFctNed.TipMaterial = tip628;
        linFctNed.Cantitate = 1m; linFctNed.PretUnitar = 100m; linFctNed.TipTva = ned21;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fctNed);
        os.CommitChanges();

        // FCL + DSC: o linie de stoc (produsul A din lotul recepționat) + un serviciu.
        var fcl = os.CreateObject<FacturaIesire>();
        fcl.Data = dFcl; fcl.Predator = sediu; fcl.Primitor = client; fcl.GestiuneDescarcare = mag1;
        var linFclStoc = os.CreateObject<FacturaIesireDetaliu>();
        linFclStoc.Document = fcl; linFclStoc.TipMaterial = tip371; linFclStoc.Produs = produsA;
        linFclStoc.Cantitate = 3m; linFclStoc.PretUnitar = 50m; linFclStoc.TipTva = n21s;
        var linFclServiciu = os.CreateObject<FacturaIesireDetaliu>();
        linFclServiciu.Document = fcl; linFclServiciu.TipMaterial = tip704;
        linFclServiciu.Cantitate = 1m; linFclServiciu.PretUnitar = 500m; linFclServiciu.TipTva = n21s;
        linFclServiciu.Descriere = "Serviciu de probă SAF-T";
        linFclServiciu.CodEconomic = codEconomic;
        os.CommitChanges();
        var dscDraft = MotorOperare.Opereaza(os, fcl);
        os.CommitChanges();
        if (dscDraft != null) {
            MotorOperare.Opereaza(os, dscDraft);
            os.CommitChanges();
        }

        // FCL către PERSOANA FIZICĂ, operată și STORNATĂ (⇒ două facturi: 380 și 381).
        var fclPf = os.CreateObject<FacturaIesire>();
        fclPf.Data = dFcl; fclPf.Predator = sediu; fclPf.Primitor = pf;
        var linFclPf = os.CreateObject<FacturaIesireDetaliu>();
        linFclPf.Document = fclPf; linFclPf.TipMaterial = tip704;
        linFclPf.Cantitate = 1m; linFclPf.PretUnitar = 400m; linFclPf.TipTva = n21s;
        os.CommitChanges();
        MotorOperare.Opereaza(os, fclPf);
        os.CommitChanges();
        MotorOperare.Storneaza(os, fclPf, dStorno);
        os.CommitChanges();

        // RLF: 2 buc din lotul B înapoi la furnizor (storno prin construcție ⇒ 381).
        var rlf = os.CreateObject<ReturFurnizor>();
        rlf.Data = dFcl; rlf.Predator = mag1; rlf.Primitor = furnizor;
        var linRlf = os.CreateObject<DocumentDetaliu>();
        linRlf.Document = rlf; linRlf.TipMaterial = tip371; linRlf.Lot = lotB;
        linRlf.Cantitate = 2m; linRlf.TipTva = n21s;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rlf);
        os.CommitChanges();

        // RDC: venit stornat (linie de factură) + cost pe lotul original (NU e linie
        // de factură — rămâne în GL, 68).
        var rdc = os.CreateObject<ReturClient>();
        rdc.Data = dFcl; rdc.Predator = client; rdc.Primitor = mag1;
        var linRdcVenit = os.CreateObject<DocumentDetaliu>();
        linRdcVenit.Document = rdc; linRdcVenit.TipMaterial = tip707; linRdcVenit.Valoare = 100m; linRdcVenit.TipTva = n21s;
        var linRdcCost = os.CreateObject<DocumentDetaliu>();
        linRdcCost.Document = rdc; linRdcCost.TipMaterial = tip371; linRdcCost.Lot = lotA; linRdcCost.Cantitate = 1m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, rdc);
        os.CommitChanges();

        // PLT (ordin de plată) către furnizor, cu imperechere pe FCT ⇒ `SourceDocumentID`.
        var plt = os.CreateObject<Plata>();
        plt.Data = dTrz; plt.Predator = banca; plt.Primitor = furnizor;
        plt.TipInstrument = TipInstrumentPlata.OrdinPlata;
        var linPlt = os.CreateObject<DocumentTrezorerieDetaliu>();
        linPlt.Document = plt; linPlt.TipMaterial = tipTrz; linPlt.Valoare = 1190m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, plt);
        os.CommitChanges();
        ImperechereService.Imperecheaza(os, plt, fct, 1190m, data: dTrz);

        // ═══ Fixul F1: PLT operată la 10 și STORNATĂ la 25 ═══
        // Fără imperechere (31d refuză stornarea unei plăți imperecheate). Motorul
        // scrie rândurile inverse la `dataStorno`, deci în luna scenei documentul are
        // DOUĂ jumătăți — iar secțiunea `Payments` trebuie să le declare pe amândouă,
        // a doua cu semnul minus. Fără spargere, plata ieșea o dată, POZITIV: o sumă
        // anulată declarată ca plătită.
        var pltStornata = os.CreateObject<Plata>();
        pltStornata.Data = dFcl; pltStornata.Predator = banca; pltStornata.Primitor = furnizor;
        pltStornata.TipInstrument = TipInstrumentPlata.OrdinPlata;
        var linPltStornata = os.CreateObject<DocumentTrezorerieDetaliu>();
        linPltStornata.Document = pltStornata; linPltStornata.TipMaterial = tipTrz; linPltStornata.Valoare = 70m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, pltStornata);
        os.CommitChanges();
        MotorOperare.Storneaza(os, pltStornata, dStorno);
        os.CommitChanges();

        // ═══ Fixul F6: PLT către un partener pe un cont FĂRĂ rol de terț (462) ═══
        var plt462 = os.CreateObject<Plata>();
        plt462.Data = dTrz; plt462.Predator = banca; plt462.Primitor = partener462;
        plt462.TipInstrument = TipInstrumentPlata.OrdinPlata;
        var linPlt462 = os.CreateObject<DocumentTrezorerieDetaliu>();
        linPlt462.Document = plt462; linPlt462.TipMaterial = tipTrz; linPlt462.Valoare = 33m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, plt462);
        os.CommitChanges();

        // INC (chitanță) de la client.
        var inc = os.CreateObject<Incasare>();
        inc.Data = dTrz; inc.Predator = client; inc.Primitor = casa;
        inc.TipInstrument = TipInstrumentPlata.Chitanta;
        var linInc = os.CreateObject<DocumentTrezorerieDetaliu>();
        linInc.Document = inc; linInc.TipMaterial = tipTrz; linInc.Valoare = 200m;
        os.CommitChanges();
        MotorOperare.Opereaza(os, inc);
        os.CommitChanges();

        // NTC: descriere pe linie (ajunge în `TransactionLine.Description`), angajatul
        // pe un cont de CLIENT (⇒ `Neincluse/RepartitorNePartener`) și o compensare
        // 401 = 4111 pe ACELAȘI partener (⇒ el apare în AMBELE liste de master files).
        var ntc = os.CreateObject<NotaContabila>();
        ntc.Numar = Marcaj + "-NTC"; ntc.Data = dTrz; ntc.Predator = sediu; ntc.Primitor = sediu;
        NotaContabilaDetaliu LinieNtc(string descriere, Cont debit, Cont credit, decimal valoare,
            Repartitor repDebit = null, Repartitor repCredit = null) {
            var d = os.CreateObject<NotaContabilaDetaliu>();
            d.Document = ntc; d.TipMaterial = tipTrz; d.Descriere = descriere;
            d.ContDebit = debit; d.ContCredit = credit; d.Valoare = valoare;
            d.RepartitorDebit = repDebit; d.RepartitorCredit = repCredit;
            return d;
        }
        var linNtcDescriere = LinieNtc("Notă de probă SAF-T", ContSimb("628"), ContSimb("5311"), 40m);
        LinieNtc("Creanță pe titularul de avans", ContSimb("4111"), ContSimb("707"), 50m, repDebit: angajat);
        LinieNtc("Compensare 401 = 4111 pe același partener", ContSimb("401"), ContSimb("4111"), 10m,
            repDebit: client, repCredit: client);
        // Fixul F2 (pin-uire): compensare între DOI parteneri diferiți — A pe debitul
        // lui 401, B pe creditul lui 4111. Fiecare latură își ia PARTENERUL PROPRIU,
        // iar rolul îl dă CONTUL ei: A e furnizor, B e client.
        LinieNtc("Compensare 401 = 4111 între doi parteneri", ContSimb("401"), ContSimb("4111"), 30m,
            repDebit: compA, repCredit: compB);
        os.CommitChanges();
        MotorOperare.Opereaza(os, ntc);
        os.CommitChanges();

        // DEC: descrierea liniei de decont ajunge tot în GL.
        var tipCheltuiala = os.GetObjectsQuery<TipMaterial>()
            .First(t => t.Clasa.Natura == NaturaClasa.Serviciu && t.ContImplicitId != null);
        var dec = os.CreateObject<Decont>();
        dec.Numar = Marcaj + "-DEC"; dec.Data = dTrz; dec.Predator = angajat; dec.Primitor = sediu;
        var linDec = os.CreateObject<DecontDetaliu>();
        linDec.Document = dec; linDec.TipMaterial = tipCheltuiala; linDec.Descriere = "Bon justificat SAF-T";
        linDec.PretUnitar = 60m; linDec.TipTva = n21s;
        os.CommitChanges();
        MotorOperare.Opereaza(os, dec);
        os.CommitChanges();

        // ══════════ Proiecția ══════════
        var saft = SaftProiectii.SaftPeCub(os, an, luna, dataCreare);
        var rez = saft.Rezumat;
        Console.WriteLine($"     MĂSURAT (D16-V2, {luna:00}.{an}): {rez.Tranzactii} tranzacții / {rez.LiniiGl} linii GL "
            + $"peste {rez.RanduriRegistru} rânduri de registru; jurnale [{string.Join(", ", saft.Jurnale.Select(j => $"{j.JournalID}×{j.Tranzactii.Count}"))}]; "
            + $"{saft.Clienti.Count} clienți / {saft.Furnizori.Count} furnizori; "
            + $"{saft.FacturiEmise.Count} facturi emise / {saft.FacturiPrimite.Count} primite / {saft.Plati.Count} plăți; "
            + $"{saft.Produse.Count} produse, {saft.Unitati.Count} UM, {saft.Taxe.Count} coduri de taxă, "
            + $"{saft.TipuriAnaliza.Count} tipuri de analiză.");
        Console.WriteLine($"     MĂSURAT (D16-V2 cusături): debit {rez.TotalDebit:N2} / credit {rez.TotalCredit:N2} vs "
            + $"registru {rez.ValoareRegistruContabil:N2}; TVA GL {rez.TvaGl:N2} + capitalizat {rez.TvaCapitalizat:N2} + "
            + $"fără cod {rez.TvaFaraCodSaft:N2} vs registru {rez.TvaRegistru:N2}; bază facturi "
            + $"A {rez.BazaFacturiAchizitie:N2} + neincluse {rez.BazaNeincluseAchizitie:N2} vs {rez.BazaRegistruAchizitie:N2}; "
            + $"L {rez.BazaFacturiLivrare:N2} + {rez.BazaNeincluseLivrare:N2} vs {rez.BazaRegistruLivrare:N2}; "
            + $"closing GLA {rez.ClosingGla:N2} vs balanță {rez.ClosingBalanta:N2}.");
        foreach (var n in saft.Neincluse)
            Console.WriteLine($"         NEINCLUS {n.Cauza} [{n.Sectiune}] {n.DocumentTip} {n.DocumentNumar} "
                + $"{n.ContSimbol} {n.RepartitorDenumire} bază {n.Baza:N2} TVA {n.Tva:N2} D {n.Debit:N2} C {n.Credit:N2}");
        foreach (var a in saft.Avertismente)
            Console.WriteLine($"         AVERTISMENT {a.Cod} ×{a.Numar}{(a.Suma is decimal s ? $" Σ {s:N2}" : "")}: "
                + $"{string.Join(" | ", a.Exemple)}");

        // ---------------- Serializarea JSON + sumarul (pasul 4b) ----------------
        VerificaSaftJson.Ruleaza(suita, saft, $"privat, {luna:00}.{an}");

        // ---------------- Antetul ----------------
        var h = saft.Header;
        Console.WriteLine($"     MĂSURAT (D16-V2 antet): {h.RegistrationNumber} „{h.Name}” {h.TaxAccountingBasis} "
            + $"{h.AuditFileRegion} {h.PeriodStart}/{h.PeriodStartYear} v{h.SoftwareVersion} IBAN {h.IBANNumber}.");
        suita.Check("D16-V2 antet: constantele de cod (2.0 / RO / Atlas / Atlas.Conta / L / RON / segment 1 din 1), "
            + "`AuditFileRegion` = județul societății, `TaxAccountingBasis` din nomenclator, `RegistrationNumber` = "
            + "`RO`+CUI (plătitor), adresa și contactul din `Societate`, IBAN-ul din contul bancar legat",
            h.AuditFileVersion == "2.0" && h.AuditFileCountry == "RO" && h.HeaderComment == "L"
            && h.SoftwareCompanyName == "Atlas" && h.SoftwareID == "Atlas.Conta"
            && !string.IsNullOrWhiteSpace(h.SoftwareVersion) && h.SoftwareVersion.Length <= 18
            && h.DefaultCurrencyCode == "RON" && h.SegmentIndex == "1" && h.TotalSegmentsInSequence == "1"
            && h.AuditFileDateCreated == dataCreare && h.AuditFileRegion == "RO-CJ"
            && h.TaxAccountingBasis == "A" && h.RegistrationNumber == "RO12345674"
            && h.Name == "Atlas Probă SAF-T SRL" && h.Address.City == "Cluj-Napoca" && h.Address.Region == "RO-CJ"
            && h.Address.Country == "RO" && h.ContactLastName == "Popescu" && h.ContactFirstName == "Ion"
            && h.IBANNumber == "RO49AAAA1B31007593840000"
            && h.PeriodStart == luna && h.PeriodStartYear == an && h.PeriodEnd == luna && h.PeriodEndYear == an
            && saft.Neaplicabil == null);

        // ---------------- Conturile ----------------
        SaftCont ContSaft(string simbol) => saft.Conturi.FirstOrDefault(c => c.AccountID == simbol);
        suita.Check("D16-V2 GeneralLedgerAccounts: `AccountID` = simbolul fără puncte, `AccountType` din `Functie`, iar "
            + "soldurile respectă `xs:choice` (debit XOR credit, niciodată amândouă) pe TOATE conturile",
            ContSaft("401") != null && ContSaft("4111") != null && ContSaft("707") != null
            && saft.Conturi.All(c => !(c.OpeningDebitBalance != null && c.OpeningCreditBalance != null))
            && saft.Conturi.All(c => !(c.ClosingDebitBalance != null && c.ClosingCreditBalance != null))
            && saft.Conturi.All(c => c.AccountType is "Activ" or "Pasiv" or "Bifunctional")
            && saft.Conturi.All(c => c.AccountID != null && !c.AccountID.Contains('.')));

        // ---------------- Clienți / furnizori ----------------
        SaftTert Tert(List<SaftTert> lista, string id) => lista.FirstOrDefault(t => t.Id == id);
        var furnizorSaft = Tert(saft.Furnizori, "0033333338");
        var clientSaft = Tert(saft.Clienti, "0022222229");
        var pfSaft = saft.Clienti.FirstOrDefault(t => t.PartenerId == pf.ID);
        Console.WriteLine($"     MĂSURAT (D16-V2 terți): furnizor {furnizorSaft?.Id} cont {furnizorSaft?.AccountID} "
            + $"C {furnizorSaft?.ClosingCreditBalance:N2}; client {clientSaft?.Id} cont {clientSaft?.AccountID}; "
            + $"PF {pfSaft?.Id} ({pfSaft?.FelId}); clientul și pe lista de furnizori: "
            + $"{Tert(saft.Furnizori, "0022222229") != null}.");
        suita.Check("D16-V2 Customers/Suppliers: partenerul se citește de pe RÂND (64h — dimensiunea laturii poartă "
            + "contrapartida), deci furnizorul ajunge în `Suppliers` pe contul 401 și clientul în `Customers` pe 4111, "
            + "fiecare cu `RegistrationNumber` = identificatorul lui și `TaxRegistration` doar la prefixul 00",
            furnizorSaft is { AccountID: "401", TaxType: "100010", RegistrationNumber: "0033333338" }
            && furnizorSaft.TaxRegistrationNumber == "33333338"
            && clientSaft is { AccountID: "4111", TaxType: "100010" }
            && saft.Furnizori.All(t => t.Id.StartsWith("0") && t.Address != null)
            && saft.Clienti.All(t => t.Address != null));
        suita.Check("D16-V2 (decizia 16) ACELAȘI partener poate fi și client, și furnizor — compensarea 401 = 4111 îl pune în "
            + "AMBELE liste, cu același `RegistrationNumber`; PF-ul iese cu prefixul `04` (baza nu raportează CNP-uri) "
            + "și cu `AdresaIncompleta` pe clientul fără localitate",
            Tert(saft.Furnizori, "0022222229") != null && clientSaft != null
            && pfSaft != null && pfSaft.Id.StartsWith("04") && pfSaft.FelId == nameof(FelIdSaft.CodIntern)
            && saft.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.AdresaIncompleta))
            && clientSaft.Address.City == "Nespecificat");
        suita.Check("D16-V2 `Neincluse`: repartitorul care NU e partener pe un cont de terț (angajatul pe 4111) iese cu "
            + "cauza și cu numele lui — soldul lui nu ajunge în `Customers`, dar nici nu dispare tăcut",
            saft.Neincluse.Any(n => n.Cauza == nameof(CauzaNeincludere.RepartitorNePartener)
                && n.ContSimbol == "4111" && n.RepartitorDenumire == "Titular decont SAF-T"));

        // ---------------- GL ----------------
        var toateLiniile = saft.Jurnale.SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).ToList();
        var tranzactii = saft.Jurnale.SelectMany(j => j.Tranzactii).ToList();
        suita.Check("D16-V2 GeneralLedgerEntries: un jurnal per TIP de document, o tranzacție per tranzacție de cub, O linie "
            + "per postare (S1-D3), echilibrată pe tranzacție, `RecordID` = 1..n în interiorul tranzacției, "
            + "`TaxInformation` pe FIECARE linie și `CurrencyAmount == Amount` (totul în RON)",
            saft.Jurnale.All(j => j.Tranzactii.Count > 0 && j.JournalID == j.Type)
            && rez.LiniiGl == rez.RanduriRegistru && rez.LiniiGl == toateLiniile.Count
            && tranzactii.All(t => t.Linii.Select(l => l.RecordID)
                .SequenceEqual(Enumerable.Range(1, t.Linii.Count).Select(i => i.ToString())))
            && tranzactii.All(t => t.Linii.Where(l => l.DebitCreditIndicator == "D").Sum(l => l.Amount)
                == t.Linii.Where(l => l.DebitCreditIndicator == "C").Sum(l => l.Amount))
            && toateLiniile.All(l => l.TaxInformation != null && l.CurrencyCode == "RON" && l.CurrencyAmount == l.Amount)
            && saft.Jurnale.Any(j => j.JournalID == "FCT") && saft.Jurnale.Any(j => j.JournalID == "NTC"));
        suita.Check("D16-V2 latura liberă: `CustomerID` ȘI `SupplierID` sunt NENULE pe fiecare linie și pe fiecare "
            + "tranzacție — ce nu e partener e raportorul (GL.19/GL.20 COM); pe conturile fără rol ies AMBELE cu "
            + "codul societății",
            toateLiniile.All(l => l.CustomerID == "0012345674" || l.SupplierID == "0012345674")
            && toateLiniile.All(l => !string.IsNullOrEmpty(l.CustomerID) && !string.IsNullOrEmpty(l.SupplierID))
            && tranzactii.All(t => !string.IsNullOrEmpty(t.CustomerID) && !string.IsNullOrEmpty(t.SupplierID)));
        var linieDescriere = toateLiniile.FirstOrDefault(l => l.DetaliuId == linNtcDescriere.ID);
        suita.Check("D16-V2 `Description` pe linia de GL: descrierea LINIEI-SURSĂ acolo unde frunza o are "
            + "(`NotaContabilaDetaliu`, `DecontDetaliu`, `FacturaIesireDetaliu`), altfel descrierea tranzacției "
            + "(denumirea tipului + numărul)",
            linieDescriere?.Description == "Notă de probă SAF-T"
            && toateLiniile.Any(l => l.Description == "Bon justificat SAF-T")
            && toateLiniile.All(l => !string.IsNullOrWhiteSpace(l.Description)));

        // ---------------- Facturi ----------------
        SaftFactura Fact(List<SaftFactura> lista, Guid docId, bool storno = false) =>
            lista.FirstOrDefault(f => f.DocumentId == docId && f.Storno == storno);
        var fFct = Fact(saft.FacturiPrimite, fct.ID);
        var fRlf = Fact(saft.FacturiPrimite, rlf.ID);
        var fFcl = Fact(saft.FacturiEmise, fcl.ID);
        var fPf = Fact(saft.FacturiEmise, fclPf.ID);
        var fPfStorno = Fact(saft.FacturiEmise, fclPf.ID, storno: true);
        var fRdc = Fact(saft.FacturiEmise, rdc.ID);
        Console.WriteLine($"     MĂSURAT (D16-V2 facturi): FCT {fFct?.InvoiceType} net {fFct?.NetTotal:N2} "
            + $"({fFct?.Linii.Count} linii) cont {fFct?.AccountID}; RLF {fRlf?.InvoiceType} net {fRlf?.NetTotal:N2}; "
            + $"FCL {fFcl?.InvoiceType} net {fFcl?.NetTotal:N2} ({fFcl?.Linii.Count} linii); PF {fPf?.InvoiceType} "
            + $"{fPf?.NetTotal:N2} / storno {fPfStorno?.InvoiceType} {fPfStorno?.NetTotal:N2}; "
            + $"RDC {fRdc?.InvoiceType} {fRdc?.NetTotal:N2} ({fRdc?.Linii.Count} linii).");
        var linieStocFct = fFct?.Linii.FirstOrDefault(l => l.DetaliuId == linFctStoc.ID);
        suita.Check("D16-V2 PurchaseInvoices: FCT iese `380` pe contul 401, cu SERVICIUL (1000, contul 628) și cu linia de "
            + "STOC (300) — contul ei nu e pe factură (recepția contează pe NIR, 26a), ci se citește din realitatea "
            + "MATERIALIZATĂ a conexului: lotul născut de linie → rândul de recepție al NIR-ului → contul lui de "
            + "DEBIT; RLF e `381` cu valori NEGATIVE (storno prin construcție, 46a)",
            fFct is { InvoiceType: "380", AccountID: "401", PartenerID: "0033333338" }
            && fFct.Linii.Count == 2 && fFct.NetTotal == 1300m && fFct.GrossTotal == 1573m
            && fFct.Linii.Any(l => l.InvoiceLineAmount == 1000m && l.AccountID == "628")
            && fFct.Linii.All(l => l.DebitCreditIndicator == "D")
            && linieStocFct is { InvoiceLineAmount: 300m, Quantity: 10m, UnitPrice: 30m }
            && linieStocFct.AccountID == "371"
            && !saft.Neincluse.Any(n => n.DetaliuId == linFctStoc.ID)
            && fRlf is { InvoiceType: "381" } && fRlf.NetTotal == -12m
            && fRlf.Linii.Single().InvoiceLineAmount == -12m && fRlf.Linii.Single().Quantity == 2m);
        // ---------------- L1 / L2 / L3 (fix-urile review-ului) ----------------
        var fTi = Fact(saft.FacturiPrimite, fctTi.ID);
        Console.WriteLine($"     MĂSURAT (D16-V2/L1 FCT all-stock TI): {(fTi == null ? "LIPSEȘTE din PurchaseInvoices" : $"cont {fTi.AccountID}, {fTi.Linii.Count} linii, net {fTi.NetTotal:N2}, linie cont {fTi.Linii.FirstOrDefault()?.AccountID}, taxă {fTi.Linii.FirstOrDefault()?.TaxInformation?.TaxCode}")}; "
            + $"rândurile FACTURII: [{string.Join(", ", saft.Jurnale.Where(j => j.JournalID == "FCT").SelectMany(j => j.Tranzactii).Where(t => t.DocumentId == fctTi.ID).SelectMany(t => t.Linii).Select(l => l.AccountID).Distinct())}].");
        suita.Check("D16-V2 (fixul L1, pe cub) FCT cu TOATE liniile pe STOC și TAXARE INVERSĂ: recepția stă pe FCT (SAF-B5), "
            + "deci tranzacția facturii poartă `371 = 401` și `4426 = 4427`, iar NIR-ul conex nu are GL — factura iese în "
            + "`PurchaseInvoices` pe 401, cu linia pe contul de stoc și cu codul SAF-T de achiziție al taxării inverse",
            nirTiConex is NIR { Stare: StareDocument.Operat }
            && fTi is { InvoiceType: "380", AccountID: "401", PartenerID: "0033333338" }
            && fTi.Linii.Count == 1 && fTi.NetTotal == 100m
            && fTi.Linii.Single().AccountID == "371"
            && fTi.Linii.Single().TaxInformation.TaxCode == ti21.CodSafTAchizitie
            && !saft.Neincluse.Any(n => n.DocumentId == fctTi.ID)
            && saft.Jurnale.SelectMany(j => j.Tranzactii).Where(t => t.DocumentId == fctTi.ID).SelectMany(t => t.Linii).ToList() is var liniiTi
            && liniiTi.Where(l => l.AccountID == "371" && l.DebitCreditIndicator == "D").Sum(l => l.Amount) == 100m
            && liniiTi.Where(l => l.AccountID == "401" && l.DebitCreditIndicator == "C").Sum(l => l.Amount) == 100m
            && !saft.Jurnale.SelectMany(j => j.Tranzactii).Any(t => t.DocumentId == nirTiConex.ID));

        var fNed = Fact(saft.FacturiPrimite, fctNed.ID);
        var linieNed = fNed?.Linii.SingleOrDefault();
        Console.WriteLine($"     MĂSURAT (D16-V2/L2 NED21 capitalizat): linie {linieNed?.InvoiceLineAmount:N2} "
            + $"(preț {linieNed?.UnitPrice:N2}), bază {linieNed?.TaxInformation?.TaxBase:N2} / taxă "
            + $"{linieNed?.TaxInformation?.TaxAmount:N2}; net {fNed?.NetTotal:N2} / brut {fNed?.GrossTotal:N2}; "
            + $"`DocumentDetaliu.Valoare` = {linFctNed.Valoare:N2}, `ValoareTva` = {linFctNed.ValoareTva:N2}.");
        suita.Check("D16-V2 (fixul L2) linia CAPITALIZATĂ (NED21) iese NETĂ: `TvaService` pune TVA-ul în cost "
            + "(`Valoare` = 121 brut, `ValoareTva` = 0), iar `RegistruTva` desface baza înapoi — deci "
            + "`InvoiceLineAmount` = 100, `TaxBase` = 100, `TaxAmount` = 21, `NetTotal` = 100 și `GrossTotal` = 121; "
            + "brutul declarat ca net ar fi umflat factura cu TVA-ul nedeductibil",
            linFctNed.Valoare == 121m && linFctNed.ValoareTva == 0m
            && fNed is { NetTotal: 100m, GrossTotal: 121m, AccountID: "401" }
            && linieNed is { InvoiceLineAmount: 100m, UnitPrice: 100m }
            && linieNed.TaxInformation.TaxBase == 100m && linieNed.TaxInformation.TaxAmount == 21m
            && linieNed.TaxInformation.TaxCode == ned21.CodSafTAchizitie);

        suita.Check("D16-V2 (S1-R4, înlocuiește fixul L3) factura `381` păstrează `InvoiceDate` = data documentului (10.08), "
            + "iar evenimentul stornării e în GL la data lui: tranzacția stornoului are `TransactionDate` = "
            + "`GLPostingDate` = 25.08, iar `Invoice.GLPostingDate` o numește",
            fPf.InvoiceDate == dFcl && fPfStorno.InvoiceDate == dFcl
            && fPf.Linii.All(l => l.TaxPointDate == dFcl)
            && fPfStorno.GLPostingDate == dStorno && fPf.GLPostingDate == dFcl
            && saft.Jurnale.SelectMany(j => j.Tranzactii).Where(t => t.DocumentId == fclPf.ID)
                .Select(t => (t.TransactionDate, t.GLPostingDate)).Order().SequenceEqual(new[] { (dFcl, dFcl), (dStorno, dStorno) }));

        var fEur = Fact(saft.FacturiPrimite, fctEur.ID);
        suita.Check("D16-V2 (SAF-B5, înlocuiește gardul `FaraContrapartida`) factura în valută cu NIR conex DRAFT: recepția stă "
            + "pe FCT, deci linia de stoc are contul 371 din postarea ei, nu `Neincluse`; factura e declarată în RON, "
            + "cu avertismentul `FacturaInValuta` (B-r6)",
            nirEurDraft is NIR { Stare: StareDocument.Draft }
            && fEur is { InvoiceType: "380", AccountID: "401", NetTotal: 200m, GrossTotal: 242m }
            && fEur.Linii.SingleOrDefault(l => l.DetaliuId == linFctEurStoc.ID) is { AccountID: "371", InvoiceLineAmount: 100m, Quantity: 5m }
            && !saft.Neincluse.Any(n => n.DocumentId == fctEur.ID)
            && Av(CodAvertismentSaft.FacturaInValuta) is { Numar: 1 } vEur && vEur.Exemple.Single().Contains("EUR"));
        suita.Check("D16-V2 SalesInvoices: FCL iese `380` pe 4111 cu 2 linii (marfă 150 + serviciu 500; tipul de TVA fără cod "
            + "refuză fișierul pe cub, SC-SAFT-14), `DebitCreditIndicator` = `C` pe vânzare; factura către PF are pereche de "
            + "STORNO (`381`, −400) — stornoul e o FACTURĂ PROPRIE (Document × Storno), nu o corecție a celei dintâi",
            fFcl is { InvoiceType: "380", AccountID: "4111" } && fFcl.Linii.Count == 2
            && fFcl.NetTotal == 650m && fFcl.Linii.All(l => l.DebitCreditIndicator == "C")
            && fPf is { InvoiceType: "380", NetTotal: 400m } && fPfStorno is { InvoiceType: "381", NetTotal: -400m }
            && fPf.PartenerID == fPfStorno.PartenerID);
        suita.Check("D16-V2 RDC: `381` cu DOAR linia de venit (−100) — linia de COST (fără `TipTva`, cu lot) e mișcare "
            + "internă venit↔stoc și rămâne în GL, exact cum cere 68",
            fRdc is { InvoiceType: "381", NetTotal: -100m } && fRdc.Linii.Count == 1
            && fRdc.Linii.Single().DetaliuId == linRdcVenit.ID
            && !saft.Neincluse.Any(n => n.DetaliuId == linRdcCost.ID));
        var identificatoriFacturi = saft.FacturiEmise.Select(f => (Lista: saft.Clienti, f.PartenerID))
            .Concat(saft.FacturiPrimite.Select(f => (Lista: saft.Furnizori, f.PartenerID))).ToList();
        suita.Check("D16-V2 cusătura master files: FIECARE `CustomerID`/`SupplierID` de pe facturi și de pe plăți există în "
            + "`Customers`/`Suppliers` — altfel fișierul ar referi un partener nedeclarat",
            identificatoriFacturi.All(x => x.Lista.Any(t => t.Id == x.PartenerID))
            && saft.Plati.SelectMany(p => p.Linii).All(l =>
                (l.CustomerID == "0012345674" || saft.Clienti.Any(t => t.Id == l.CustomerID))
                && (l.SupplierID == "0012345674" || saft.Furnizori.Any(t => t.Id == l.SupplierID))));

        // ---------------- Plăți ----------------
        var pPlt = saft.Plati.FirstOrDefault(p => p.DocumentId == plt.ID && !p.Storno);
        var pInc = saft.Plati.FirstOrDefault(p => p.DocumentId == inc.ID && !p.Storno);
        Console.WriteLine($"     MĂSURAT (D16-V2 plăți): PLT {pPlt?.PaymentMethod}/{pPlt?.PaymentMechanism} "
            + $"{pPlt?.GrossTotal:N2} sursă „{pPlt?.Linii.FirstOrDefault()?.SourceDocumentID}”; "
            + $"INC {pInc?.PaymentMethod}/{pInc?.PaymentMechanism} {pInc?.GrossTotal:N2}.");
        suita.Check("D16-V2 Payments: ordinul de plată iese 03/42 cu `D` pe linie și contul de furnizor, iar chitanța 01/10 "
            + "cu `C`; `SourceDocumentID` = numărul documentului stins fiindcă imperecherea plății e UNICĂ; "
            + "`TaxInformation` e `000/000000` (plata nu e operațiune taxabilă)",
            pPlt is { PaymentMethod: "03", PaymentMechanism: "42", GrossTotal: 1190m }
            && pPlt.Linii.Single() is { DebitCreditIndicator: "D", AccountID: "401", SupplierID: "0033333338" }
            && pPlt.Linii.Single().SourceDocumentID == fct.Numar
            && pInc is { PaymentMethod: "01", PaymentMechanism: "10", GrossTotal: 200m }
            && pInc.Linii.Single() is { DebitCreditIndicator: "C", AccountID: "4111", CustomerID: "0022222229" }
            && saft.Plati.SelectMany(p => p.Linii).All(l => l.TaxInformation.TaxCode == "000000"));

        var platiStornate = saft.Plati.Where(p => p.DocumentId == pltStornata.ID)
            .OrderBy(p => p.Storno).ToList();
        Console.WriteLine($"     MĂSURAT (D16-V2/F1 plată stornată): {platiStornate.Count} intrări — "
            + $"[{string.Join(", ", platiStornate.Select(p => $"{(p.Storno ? "storno" : "operare")} {p.TransactionDate:dd.MM} {p.GrossTotal:N2}"))}]; "
            + $"Σ semnată {platiStornate.Sum(p => p.GrossTotal):N2}; `Rezumat.TotalPlati` {rez.TotalPlati:N2}.");
        suita.Check("D16-V2 (fixul F1) plata STORNATĂ e o plată PROPRIE, cu semnul ei: aceeași unitate ca la facturi "
            + "(Document × Storno), aceeași dată a rândurilor ei (10.08 / 25.08), `PaymentRefNo` identic și "
            + "`PaymentLineAmount` negativ pe jumătatea de storno — Σ celor două e ZERO, deci nici `Payments`, nici "
            + "`Rezumat.TotalPlati` nu mai declară ca plătită o sumă anulată",
            platiStornate.Count == 2
            && platiStornate[0] is { Storno: false, GrossTotal: 70m }
            && platiStornate[1] is { Storno: true, GrossTotal: -70m }
            && platiStornate[0].TransactionDate == dFcl && platiStornate[1].TransactionDate == dStorno
            && platiStornate[0].PaymentRefNo == platiStornate[1].PaymentRefNo
            && platiStornate[1].Linii.Single().PaymentLineAmount == -70m
            // Indicatorul rămâne al DIRECȚIEI (`D` pe plată), semnul stă pe sumă.
            && platiStornate.All(p => p.Linii.All(l => l.DebitCreditIndicator == "D" && l.AccountID == "401"))
            && platiStornate.Sum(p => p.GrossTotal) == 0m
            && platiStornate[1].Description.Contains("storno"));

        Console.WriteLine($"     MĂSURAT (D16-V2/F6 plată pe cont fără rol): în `Plati` "
            + $"{saft.Plati.Count(p => p.DocumentId == plt462.ID)}; în `Neincluse` "
            + $"{saft.Neincluse.Count(n => n.DocumentId == plt462.ID)}; partenerul „{partener462.Denumire}” în "
            + $"master files: {saft.Clienti.Concat(saft.Furnizori).Any(t => t.PartenerId == partener462.ID)}; "
            + $"terți cu `AccountID` gol: {saft.Clienti.Concat(saft.Furnizori).Count(t => string.IsNullOrEmpty(t.AccountID))}.");
        suita.Check("D16-V2 (fixul F6) plata către un partener pe un cont FĂRĂ rol de terț (462 „Creditori diverși”) NU "
            + "produce o intrare de terț cu `AccountID` gol — care ar fi făcut fișierul invalid: plata iese în "
            + "`Neincluse/ContFaraRol [Payments]` cu avertisment, rândurile ei rămân în GL cu societatea pe ambele "
            + "identificatoare, iar NICIUN terț din master files n-are `AccountID` gol",
            saft.Plati.All(p => p.DocumentId != plt462.ID)
            && saft.Neincluse.Any(n => n.DocumentId == plt462.ID
                && n.Cauza == nameof(CauzaNeincludere.ContFaraRol) && n.Sectiune == "Payments")
            && Av(CodAvertismentSaft.PlataFaraContTert) is { Numar: 1 }
            && !saft.Clienti.Concat(saft.Furnizori).Any(t => t.PartenerId == partener462.ID)
            && saft.Clienti.Concat(saft.Furnizori).All(t => !string.IsNullOrEmpty(t.AccountID)
                && !string.IsNullOrEmpty(t.Id))
            && saft.Jurnale.SelectMany(j => j.Tranzactii).Where(t => t.DocumentId == plt462.ID)
                .SelectMany(t => t.Linii).All(l => l.CustomerID == "0012345674" && l.SupplierID == "0012345674"));

        // ---------------- F2: rândul cu DOI parteneri (pin-uire) ----------------
        var tertA = saft.Furnizori.FirstOrDefault(t => t.PartenerId == compA.ID);
        var tertB = saft.Clienti.FirstOrDefault(t => t.PartenerId == compB.ID);
        Console.WriteLine($"     MĂSURAT (D16-V2/F2 compensare între doi parteneri): A „{compA.Denumire}” → "
            + $"furnizor {tertA?.Id} cont {tertA?.AccountID} D {tertA?.ClosingDebitBalance:N2}; B „{compB.Denumire}” → "
            + $"client {tertB?.Id} cont {tertB?.AccountID} C {tertB?.ClosingCreditBalance:N2}; A în Customers: "
            + $"{saft.Clienti.Any(t => t.PartenerId == compA.ID)}; B în Suppliers: "
            + $"{saft.Furnizori.Any(t => t.PartenerId == compB.ID)}.");
        suita.Check("D16-V2 (fixul F2, PIN) pe rândul cu partener pe AMBELE laturi și cu AMBELE conturi de terț "
            + "(`401 = 4111`, A pe debit, B pe credit) fiecare latură își ia PARTENERUL PROPRIU, iar rolul îl dă "
            + "CONTUL laturii: A (debit 401) e FURNIZOR, B (credit 4111) e CLIENT — și niciunul nu apare în lista "
            + "celuilalt. Căderea pe latura cealaltă (64h) e rezerva pentru rândurile cu UN singur partener, nu regula",
            tertA is { AccountID: "401", ClosingDebitBalance: 30m } && tertA.Id == "0076543210"
            && tertB is { AccountID: "4111", ClosingCreditBalance: 30m } && tertB.Id == "0024680249"
            && !saft.Clienti.Any(t => t.PartenerId == compA.ID)
            && !saft.Furnizori.Any(t => t.PartenerId == compB.ID));

        // ---------------- Produse, unități, taxe, analiză ----------------
        var prodA = saft.Produse.FirstOrDefault(p => p.ProdusId == produsA.ID);
        var prodB = saft.Produse.FirstOrDefault(p => p.ProdusId == produsB.ID);
        Console.WriteLine($"     MĂSURAT (D16-V2 produse): A NC „{prodA?.ProductCommodityCode}” UM {prodA?.UOMBase} "
            + $"{prodA?.GoodsServicesID}; B NC „{prodB?.ProductCommodityCode}” UM {prodB?.UOMBase}; "
            + $"UOMTable [{string.Join(", ", saft.Unitati.Select(u => $"{u.UnitOfMeasure}=„{u.Description}”"))}]; "
            + $"TaxTable [{string.Join(", ", saft.Taxe.Select(t => $"{t.TaxType}/{t.TaxCode}@{t.TaxPercentage}"))}].");
        suita.Check("D16-V2 Products: doar produsele de pe liniile de factură EMISE; codul NC lipsă ⇒ „0” + avertisment, "
            + "unitatea de măsură lipsă ⇒ „H87” + avertisment, `ValuationMethod` = FIFO (51e), factor de conversie 1, "
            + "`GoodsServicesID` = 01 pe natura Stoc",
            prodA is { ProductCommodityCode: "01012100", UOMBase: "H87", UOMStandard: "H87", GoodsServicesID: "01",
                ValuationMethod: "FIFO", UOMToUOMBaseConversionFactor: 1m }
            && prodB is { ProductCommodityCode: "0", UOMBase: "H87" }
            && saft.Produse.Count == 2
            && saft.Unitati.Single() is { UnitOfMeasure: "H87", Description: "bucată" });
        suita.Check("D16-V2 AnalysisTypeTable + `Analysis`: câte o intrare per dimensiune FOLOSITĂ pe rândurile perioadei "
            + "(`AnalysisID` = codul nomenclatorului, descrierea = denumirea lui), iar pe linia de GL și pe linia de "
            + "factură dimensiunile sunt ale LATURII — nu se declară tipuri nefolosite",
            saft.TipuriAnaliza.Single() is { AnalysisType: "CE", AnalysisID: "E2E-SAFT-CE",
                AnalysisIDDescription: "Cod economic de probă SAF-T" }
            && fFcl.Linii.Any(l => l.Analiza.Any(a => a.AnalysisType == "CE" && a.AnalysisID == "E2E-SAFT-CE"))
            && toateLiniile.Any(l => l.Analiza.Any(a => a.AnalysisID == "E2E-SAFT-CE")));
        suita.Check("D16-V2 TaxTable: un rând per cod SAF-T FOLOSIT (`000000` nu se declară), cu cota din nomenclator, "
            + "`BaseRate` = 1 (`SAFBaseRate` e restricționat [0,1], nu „100”) și țara RO; tipul de TVA fără mapare "
            + "refuză fișierul (S1-D4, SC-SAFT-14), deci aici nu există nici avertisment, nici taxă „fără cod”",
            saft.Taxe.Count > 0 && saft.Taxe.All(t => t.TaxType == "300" && t.BaseRate == 1m && t.Country == "RO")
            && !saft.Taxe.Any(t => t.TaxCode == "000000")
            && !saft.Avertismente.Any(a => a.Cod == nameof(CodAvertismentSaft.TipTvaFaraCodSaft))
            && rez.TvaFaraCodSaft == 0m && saft.Refuzuri.Count == 0);

        // ---------------- Cusăturile (D16-D4) ----------------
        Console.WriteLine($"     MĂSURAT (D16-V2 cusături noi): conturi verificate {rez.ConturiVerificate} / "
            + $"diferite {rez.ConturiDiferite} / Σ|closing| {rez.SumaAbsolutaClosing:N2}; clienți "
            + $"{rez.ClosingClienti:N2} + neincluse {rez.NeincluseClienti:N2} vs GLA {rez.ClosingGlaClienti:N2}; "
            + $"furnizori {rez.ClosingFurnizori:N2} + {rez.NeincluseFurnizori:N2} vs {rez.ClosingGlaFurnizori:N2}; "
            + $"bază registru A {rez.BazaRegistruAchizitie:N2} / L {rez.BazaRegistruLivrare:N2} (TOATE tipurile).");
        suita.Check("D16-V2 cusătura 1 (partidă dublă): Σ `DebitAmount` == Σ `CreditAmount` == Σ `RegistruContabil.Valoare` "
            + "pe perioadă, semnat — rândurile de deschidere (fără document) nu intră în GL. Fixul F3: cele două "
            + "totaluri se numără din LINIILE EMISE, fiecare pe latura ei (`D` ⇒ `DebitAmount`, `C` ⇒ "
            + "`CreditAmount`), nu dintr-un acumulator comun care le făcea egale prin construcție",
            rez.TotalDebit == rez.TotalCredit && rez.TotalDebit == rez.ValoareRegistruContabil
            && rez.ValoareRegistruContabil > 0m
            && rez.TotalDebit == toateLiniile.Where(l => l.DebitCreditIndicator == "D").Sum(l => l.Amount)
            && rez.TotalCredit == toateLiniile.Where(l => l.DebitCreditIndicator == "C").Sum(l => l.Amount));
        suita.Check("D16-V2 cusătura 2 (TVA): Σ `TaxAmount` de pe rândurile de GL + TVA-ul capitalizat (care n-are rând "
            + "de taxă emis în GL) + taxa tipurilor fără cod SAF-T == Σ `RegistruTva.Tva` — nicio cifră fiscală nu se "
            + "pierde între cele două registre",
            rez.TvaGl + rez.TvaCapitalizat + rez.TvaFaraCodSaft == rez.TvaRegistru && rez.TvaRegistru != 0m);
        // Tipurile de factură se citesc pe CLASELE CLR (FCL/FCT/RDC/RLF), nu prin
        // `CoduriTip` — ca proba să nu depindă de aceeași funcție pe care o folosește
        // proiecția.
        List<Guid> IdsTip<T>() where T : Document => os.GetObjectsQuery<T>()
            .Where(d => d.Data >= pStart && d.Data <= pEnd).Select(d => d.ID).ToList();
        var idsFacturiScena = IdsTip<FacturaIesire>().Concat(IdsTip<FacturaIntrare>())
            .Concat(IdsTip<ReturClient>()).Concat(IdsTip<ReturFurnizor>()).ToHashSet();
        var bazaFaraSectiuneCub = Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.IntreLuni(
                Atlas.Conta.BackOffice.Module.Cub.Citiri.Fiscale.Fapte(os), pStart, pEnd).ToList()
            .Where(f => !idsFacturiScena.Contains(f.DocumentId)).Sum(f => f.Baza);
        suita.Check("D16-V2 cusătura 3 (facturi): pe FIECARE sens, Σ bazei rândurilor fiscale AȘEZATE pe linii de factură + "
            + "Σ bazei celor NEINCLUSE == Σ `RegistruTva.Baza` pe TOATE tipurile de document. Fixul F5: numitorul nu "
            + "mai e restrâns la tipurile de factură — asta măsura mulțimea care intra în fișier cu mulțimea care "
            + "intra în fișier; pe cub, baza fără factură (DEC) e termenul „fără factură” al cusăturii (S2-R4), egal cu "
            + "registrul fiscal al tipurilor fără secțiune citit independent",
            rez.BazaFacturiAchizitie + rez.BazaNeincluseAchizitie == rez.BazaRegistruAchizitie
            && rez.BazaFacturiLivrare + rez.BazaNeincluseLivrare == rez.BazaRegistruLivrare
            && rez.BazaRegistruAchizitie != 0m && rez.BazaRegistruLivrare != 0m
            && rez.BazaNeincluseAchizitie + rez.BazaNeincluseLivrare == bazaFaraSectiuneCub
            && bazaFaraSectiuneCub == 60m);
        suita.Check("D16-V2 cusătura 4 (solduri) — fixul F3, PER CONT: pentru fiecare cont din `GeneralLedgerAccounts` "
            + "Closing − Opening == rulajul net al liniilor GL emise pe cont (pe cub); `ConturiDiferite` = 0 din "
            + "`ConturiVerificate` > 0, cu Σ|closing| ca martor, recalculat aici din liniile fișierului",
            rez.ClosingGla == rez.ClosingBalanta
            && rez.ConturiDiferite == 0 && rez.ConturiVerificate > 0
            && rez.ConturiVerificate == saft.Conturi.Count
            && rez.SumaAbsolutaClosing > 0m
            && saft.Conturi.All(c => (c.ClosingDebitBalance ?? 0m) - (c.ClosingCreditBalance ?? 0m)
                - (c.OpeningDebitBalance ?? 0m) + (c.OpeningCreditBalance ?? 0m)
                == toateLiniile.Where(l => l.AccountID == c.AccountID).Sum(l => l.DebitCreditIndicator == "D" ? l.Amount : -l.Amount)));
        suita.Check("D16-V2 cusătura 5 (terți — fixul F4): Σ soldurilor finale declarate în `Customers` + Σ soldurilor "
            + "rămase în `Neincluse[Customers]` == Σ `Closing` din GLA pe conturile cu `RolTert == Client`; idem "
            + "`Suppliers`/Furnizor. E cusătura care leagă master files de planul de conturi — angajatul de pe 4111 "
            + "(care NU e partener) e chiar termenul care o face nebanală: fără el egalitatea n-ar ține",
            rez.ClosingClienti + rez.NeincluseClienti == rez.ClosingGlaClienti
            && rez.ClosingFurnizori + rez.NeincluseFurnizori == rez.ClosingGlaFurnizori
            && rez.ClosingGlaClienti != 0m && rez.ClosingGlaFurnizori != 0m
            && rez.NeincluseClienti != 0m);

        // ---------------- Avertismentele, agregate ----------------
        SaftAvertisment Av(CodAvertismentSaft cod) => saft.Avertismente.FirstOrDefault(a => a.Cod == cod.ToString());
        suita.Check("D16-V2 avertismente AGREGATE per cauză (un rând per cod, cu numărul, suma unde are sens și ≤ 5 exemple "
            + "nominale): cod NC lipsă, UM lipsă, adresă incompletă, factură în valută — fiecare NUMIT, niciunul "
            + "înlocuit cu o valoare tăcută (tipul TVA fără cod și linia fără contrapartidă nu mai există pe cub: "
            + "S1-D4 refuză, SAF-B5 dă contul recepției)",
            Av(CodAvertismentSaft.FaraCodNc) is { Numar: 1 } && Av(CodAvertismentSaft.FaraUnitateMasura) is { Numar: 1 }
            && Av(CodAvertismentSaft.AdresaIncompleta) != null
            && Av(CodAvertismentSaft.FacturaInValuta) is { Numar: 1 } valuta
                && valuta.Exemple.Single().Contains("EUR")
            && saft.Avertismente.All(a => !string.IsNullOrWhiteSpace(a.Mesaj)
                && a.Exemple.Count > 0 && a.Exemple.Count <= 5 && a.Numar >= a.Exemple.Count)
            && saft.Avertismente.Select(a => a.Cod).Distinct().Count() == saft.Avertismente.Count);

        // ---------------- `RaporteazaCnp`: aceeași PF, alt identificator ----------------
        societate.RaporteazaCnp = true;
        os.CommitChanges();
        var cuCnp = SaftProiectii.SaftPeCub(os, an, luna, dataCreare);
        var pfCuCnp = cuCnp.Clienti.FirstOrDefault(t => t.PartenerId == pf.ID);
        var facturiPf = cuCnp.FacturiEmise.Where(f => f.DocumentId == fclPf.ID).ToList();
        Console.WriteLine($"     MĂSURAT (D16-V2 RaporteazaCnp): {pfSaft?.Id} → {pfCuCnp?.Id} ({pfCuCnp?.FelId}).");
        suita.Check("D16-V2 `Societate.RaporteazaCnp` e o POLITICĂ a bazei, nu o constantă a generatorului: aceeași "
            + "persoană fizică iese `04`+cod intern cât timp baza nu raportează CNP-uri și `03`+CNP după ce o face — "
            + "identificatorul se schimbă ODATĂ în master files ȘI pe facturile ei",
            pfCuCnp is { FelId: nameof(FelIdSaft.Cnp) } && pfCuCnp.Id == "031800101123450"
            && facturiPf.Count == 2 && facturiPf.All(f => f.PartenerID == "031800101123450")
            && cuCnp.Rezumat.TotalDebit == rez.TotalDebit);
        societate.RaporteazaCnp = false;
        os.CommitChanges();

        // ---------------- Închiderea de TVA (riscul 4): `TaxCode 380200` ----------------
        // Proba e CONDIȚIONATĂ: generatorul refuză o închidere „în urmă" dacă baza are
        // deja una vie pentru o lună ulterioară (guard-ul cronologic 46c), iar scenele
        // anterioare pot lăsa exact asta. Se sare ZGOMOTOS, nu tăcut.
        InchidereTva itv = null;
        string motivItv = null;
        try {
            itv = InchidereTvaService.Genereaza(os, an, luna, sediu.ID);
            if (itv != null) {
                itv.Numar = Marcaj + "-ITV";
                os.CommitChanges();
                MotorOperare.Opereaza(os, itv);
                os.CommitChanges();
            }
            else {
                motivItv = "generatorul n-a avut ce închide (sold zero sau închidere vie pe lună)";
            }
        }
        catch (OperareException e) {
            motivItv = e.Message.Split('\n')[0];
        }
        if (itv != null) {
            var cuItv = SaftProiectii.SaftPeCub(os, an, luna, dataCreare);
            var liniiItv = cuItv.Jurnale.Where(j => j.JournalID == "ITV")
                .SelectMany(j => j.Tranzactii).SelectMany(t => t.Linii).ToList();
            Console.WriteLine($"     MĂSURAT (D16-V2 ITV): {liniiItv.Count} linii de închidere, coduri "
                + $"[{string.Join(", ", liniiItv.Select(l => l.TaxInformation.TaxCode).Distinct())}].");
            suita.Check("D16-V2 (riscul 4) rândurile închiderii lunare de TVA ies cu `TaxCode 380200` "
                + "(`TVA_NoteContabile`) — identificate prin TIPUL documentului (`ITV`), nu prin simbolul contului "
                + "(decizia 29: motorul și proiecțiile nu cunosc niciun simbol)",
                liniiItv.Count > 0 && liniiItv.All(l => l.TaxInformation.TaxCode == "380200"
                    && l.TaxInformation.TaxType == "300")
                && cuItv.Taxe.Any(t => t.TaxCode == "380200"));
        }
        else {
            Console.WriteLine($"     SĂRIT (D16-V2 ITV): {motivItv} — proba codului `380200` rămâne pentru V3/V5.");
        }

        // ---------------- F8: autoritatea seed-ului se oprește la granița planului ----------------
        // `SeedRolTert` e AUTORITAR (o corectare a listei trebuie să ajungă pe bazele
        // existente), dar `Cont` e nomenclator VIU: planul se completează pe bază cu
        // analitice proprii și conturi de decontare ale clientului. Trecerea de
        // dinainte le rescria și pe alea la fiecare `--updateDatabase`, tăcut — iar
        // efectul se vedea abia în D406, ca partener dispărut din `Customers`.
        // Proba rulează FUNCȚIA REALĂ (`ContaSeeder.SeedRolTertPrivat`), nu o imitație.
        var contAdaugat = os.CreateObject<Cont>();
        contAdaugat.Simbol = SimbolContProbaSaft;
        contAdaugat.Denumire = "Cont propriu al bazei, în afara planului OMFP";
        contAdaugat.Functie = "D";
        contAdaugat.RolTert = RolTertCont.Client;
        // Și jumătatea cealaltă: un cont AL PLANULUI, stricat cu mâna, trebuie să-și
        // recapete rolul la re-seed — altfel „nu atinge" ar fi devenit „nu mai face nimic".
        var cont401 = ContSimb("401");
        cont401.RolTert = RolTertCont.Niciunul;
        os.CommitChanges();
        var idContAdaugat = contAdaugat.ID;
        using (var osSeed = suita.Provider.CreateObjectSpace()) {
            ContaSeeder.SeedRolTertPrivat(osSeed);
            osSeed.CommitChanges();
        }
        RolTertCont rolAdaugatDupaSeed, rol401DupaSeed;
        using (var osCitire = suita.Provider.CreateObjectSpace()) {
            rolAdaugatDupaSeed = osCitire.GetObjectsQuery<Cont>().First(c => c.ID == idContAdaugat).RolTert;
            rol401DupaSeed = osCitire.FirstOrDefault<Cont>(c => c.Simbol == "401").RolTert;
        }
        Console.WriteLine($"     MĂSURAT (D16-V2/F8 SeedRolTert): contul „{SimbolContProbaSaft}” (în afara CSV-ului) "
            + $"Client → {rolAdaugatDupaSeed} după re-seed; contul 401 (în CSV) stricat la Niciunul → {rol401DupaSeed}.");
        suita.Check("D16-V2 (fixul F8) `SeedRolTert` rescrie DOAR conturile care sunt în CSV-ul planului (ca `Functie` din "
            + "`SeedPlanConturi`): un cont ADĂUGAT pe bază, cu simbol în afara resursei și cu `RolTert` pus de "
            + "operator, rămâne NEATINS; un cont AL PLANULUI, stricat cu mâna, își recapătă rolul — autoritatea se "
            + "păstrează, dar se oprește la granița seed-ului",
            rolAdaugatDupaSeed == RolTertCont.Client && rol401DupaSeed == RolTertCont.Furnizor);
        os.Refresh();
        var pjCont = new Purja(os);
        pjCont.Adauga(os.GetObjectsQuery<Cont>()
            .Where(c => c.Simbol == SimbolContProbaSaft).ToList());
        pjCont.Executa();

        // ---------------- Fișierul + oracolul DUK (D16-V3) ----------------
        // Se rulează AICI, cât scena e încă pe bază: proiecția se re-măsoară pe ceas,
        // iar `SaftXml` scrie chiar DTO-ul probat mai sus — fișierul validat de ANAF
        // e cel al scenei, nu unul fabricat pentru ocazie.
        var cronometru = Stopwatch.StartNew();
        var saftCronometrat = SaftProiectii.SaftPeCub(os, an, luna, dataCreare);
        var msProiectie = cronometru.Elapsed.TotalMilliseconds;
        VerificaSaftXml.Ruleaza(suita, saftCronometrat, an, luna, msProiectie);

        // ---------------- Curățenie ----------------
        RestaureazaSocietatea();
        Curata();
        suita.Check("Curățenie finală felia SAF-T (fără reziduuri e2e: repartitori, documente, loturi, produse, tipul de "
            + "TVA de probă; antetul societății și IBAN-ul contului bancar puși la loc)",
            !os.GetObjectsQuery<Repartitor>().Any(r => r.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Produs>().Any(p => p.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<TipTva>().Any(t => t.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<CodEconomic>().Any(c => c.Cod.StartsWith(Marcaj))
            && !os.GetObjectsQuery<Document>().Any(d => d.Numar != null && d.Numar.StartsWith(Marcaj))
            && os.GetObjectsQuery<Societate>().First().CodFiscal == socInainte.CodFiscal
            && os.FirstOrDefault<ContPropriu>(c => c.Cod == "BANCA").Iban == ibanInainte);
    }
}
