using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiDecl {
    void RotunjireaPartilor() {
        Verifica("SC-DECL-12", "cinci bunuri de 0,60 pe un rând de 3 lei: 1, 1, 1, 0, 0, nu 5 lei corectați pe primul",
            RotunjireDeclaratie.Lei(3m, [0.6m, 0.6m, 0.6m, 0.6m, 0.6m]).SequenceEqual([1m, 1m, 1m, 0m, 0m]));
        Verifica("SC-DECL-12", "bunuri cu semne diferite (2,60 și −2,40) pe un rând de 0 lei: 2 și −2",
            RotunjireDeclaratie.Lei(0m, [2.6m, -2.4m]).SequenceEqual([2m, -2m]));
        var aleator = new Random(122);
        var abateri = new List<string>();
        for (var caz = 0; caz < 500 && abateri.Count == 0; caz++) {
            var parti = Enumerable.Range(0, 1 + aleator.Next(8))
                .Select(_ => aleator.Next(5) == 0 ? 0m : (aleator.Next(-50000, 100000) / 100m)).ToList();
            var total = RotunjireDeclaratie.Lei(parti.Sum());
            var cote = RotunjireDeclaratie.Lei(total, parti);
            if (cote.Sum() != total)
                abateri.Add($"caz {caz}: Σ {cote.Sum()} ≠ {total}");
            for (var i = 0; i < parti.Count; i++)
                if (cote[i] != decimal.Truncate(cote[i]) || Math.Abs(cote[i] - parti[i]) >= 2m
                        || cote[i] != 0m && Math.Sign(cote[i]) != Math.Sign(parti[i]))
                    abateri.Add($"caz {caz}: {parti[i]} → {cote[i]}");
        }
        Verifica("SC-DECL-12", "500 de rânduri aleatoare: lei întregi, suma egală cu rândul, semnul păstrat, abatere sub 2 lei "
            + $"({string.Join("; ", abateri.Take(3))})", abateri.Count == 0);
    }

    void DepunereaFisierului() {
        Opereaza(VanzareNumerotata(Luna(12), 100.40m, "N21"));
        Opereaza(VanzareNumerotata(Luna(12), 200.40m, "N21", primitor: Furnizor));
        var (inceput, sfarsit) = Perioada(12);
        var dto = CuSpatiu(os => D394Proiectii.D394(os, inceput, sfarsit));
        Verifica("SC-DECL-13", "doi parteneri L 21% cu 100,40 și 200,40: fișierul are 300 de lei, totalul rotunjit o dată 301",
            dto.DiferenteRotunjire is [{ Tip: "L", Cota: 21, Randuri: 2, Baza: -1m, Tva: 0m }]);
        ExportaD394("SC-DECL-13", inceput, sfarsit);

        var fisier = CuSpatiu(os => D394Xml.Genereaza(os, inceput, sfarsit, new ParametriXmlD394(PersoaneAfiliate: true)));
        Verifica("SC-DECL-14", "fișierul refăcut din parametrii exportului are aceeași amprentă",
            CuSpatiu(os => FisiereDeclaratii.Verifica(os, FormularFiscal.D394, An, 12, fisier.Sha256, fisier.Parametri)).Count == 0);
        Verifica("SC-DECL-14", "alți parametri dau alt fișier",
            CuSpatiu(os => FisiereDeclaratii.Verifica(os, FormularFiscal.D394, An, 12, fisier.Sha256,
                FisiereDeclaratii.Serializeaza(new ParametriFisierD394(new ParametriXmlD394(), Antet0())))) is [var diferit]
                && diferit.StartsWith("DEPUNERE_FISIER_DIFERIT"));
        Antet(s => s.Telefon = "0268111111");
        Verifica("SC-DECL-14", "antetul schimbat după export: fișierul se reface din antetul exportului, cu aceeași amprentă",
            CuSpatiu(os => FisiereDeclaratii.Verifica(os, FormularFiscal.D394, An, 12, fisier.Sha256, fisier.Parametri)).Count == 0);
        Antet();
        Verifica("SC-DECL-14", "amprenta fără parametri e refuzată la confirmare",
            Refuzat(() => Comanda(os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 12,
                fisier.VersiuneExportata, Marcaj, fisier.Sha256, null)), "DEPUNERE_DATE"));
        Comanda(os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 12,
            fisier.VersiuneExportata, Marcaj, fisier.Sha256, fisier.Parametri));
        Verifica("SC-DECL-14", "confirmarea păstrează amprenta și parametrii fișierului",
            CuSpatiu(os => os.GetObjectsQuery<DepunereDeclaratie>()
                .Where(d => d.Formular == FormularFiscal.D394 && d.Perioada == An * 100 + 12)
                .Select(d => new { d.Sha256Fisier, d.ParametriFisier }).ToList())
                is [var d] && d.Sha256Fisier == fisier.Sha256 && d.ParametriFisier == fisier.Parametri);
        var cuFisier = CuSpatiu(os => FiscalitateService.Active(os).Single(d => d.Perioada == An * 100 + 12).ID);
        Verifica("SC-DECL-14", "aceeași cerere confirmată din nou e idempotentă",
            CuSpatiu(os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 12,
                fisier.VersiuneExportata, Marcaj, fisier.Sha256, fisier.Parametri)) == cuFisier);
        var faraFisier = CuSpatiu(os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 12,
            fisier.VersiuneExportata, Marcaj));
        Verifica("SC-DECL-15", "reconfirmarea aceleiași versiuni fără fișier o înlocuiește pe cea cu fișier, care rămâne retrasă în istoric",
            CuSpatiu(os => os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Perioada == An * 100 + 12)
                .Select(d => new { d.ID, d.RetrasaLa, d.Inlocuieste, d.Sha256Fisier }).ToList()) is var toate
            && toate.Count == 2
            && toate.Single(d => d.ID == cuFisier) is { RetrasaLa: not null }
            && toate.Single(d => d.ID == faraFisier) is { RetrasaLa: null, Sha256Fisier: null } nou && nou.Inlocuieste == cuFisier);
        var atasat = CuSpatiu(os => FiscalitateService.ConfirmaDepunerea(os, FormularFiscal.D394, An, 12,
            fisier.VersiuneExportata, Marcaj, fisier.Sha256, fisier.Parametri));
        Verifica("SC-DECL-15", "fișierul se atașează ulterior prin reconfirmare", atasat != faraFisier
            && CuSpatiu(os => FiscalitateService.Active(os).Where(d => d.Perioada == An * 100 + 12)
                .Select(d => d.Sha256Fisier).ToList()) is [var sha] && sha == fisier.Sha256);
        Verifica("SC-DECL-15", "retragerea fără motiv e refuzată",
            Refuzat(() => Comanda(os => FiscalitateService.RetrageDepunerea(os, FormularFiscal.D394, An, 12,
                fisier.VersiuneExportata, " ", Marcaj)), "DEPUNERE_DATE"));
        Comanda(os => FiscalitateService.RetrageDepunerea(os, FormularFiscal.D394, An, 12,
            fisier.VersiuneExportata, "confirmată din greșeală", Marcaj));
        Verifica("SC-DECL-15", "D394 retrasă: luna nu mai e depusă, iar retragerea a doua oară e refuzată",
            !CuSpatiu(os => FiscalitateService.EsteDepusa(os, FormularFiscal.D394, An * 100 + 12))
            && Refuzat(() => Comanda(os => FiscalitateService.RetrageDepunerea(os, FormularFiscal.D394, An, 12,
                fisier.VersiuneExportata, "din nou", Marcaj)), "DEPUNERE_NECONFIRMATA"));

        var (ianuarie, ianuarieSfarsit) = Perioada(1);
        var d300 = CuSpatiu(os => D300Xml.Genereaza(os, ianuarie, ianuarieSfarsit,
            new ParametriD300(SoldPlataPrecedent: 12.34m), new ParametriXmlD300(Temei: 2, ProRata: 50m, BifaInterne: false)));
        Verifica("SC-DECL-14", "D300 refăcut din externi și datele de perioadă are aceeași amprentă",
            d300.Continut != null
            && CuSpatiu(os => FisiereDeclaratii.Verifica(os, FormularFiscal.D300, An, 1, d300.Sha256, d300.Parametri)).Count == 0);
    }

    AntetDeclaratie Antet0() => CuSpatiu(AntetDeclaratie.Citeste);

    static bool Refuzat(Action actiune, string cod) {
        try { actiune(); return false; }
        catch (OperareException e) { return e.Message.StartsWith(cod, StringComparison.Ordinal); }
    }
}
