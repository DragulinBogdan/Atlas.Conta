using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.ModelCheck;

// Catalogul REGIM (106): regimul pe stare, scris de mână din regula comenzilor.
sealed class ScenariiRegim(Func<IObjectSpace> deschide, Action<string, bool> check,
    bool privat, Action<IObjectSpace, int, int> inchide)
    : ScenaDocumente(deschide, check, privat, inchide, "REGIM", 2013) {
    const string CmdAnuleaza = nameof(ComandaDocument.AnuleazaOperarea);
    const string CmdStorneaza = nameof(ComandaDocument.Storneaza);
    const string CmdCorecteaza = nameof(ComandaDocument.Corecteaza);
    const string CmdOpereaza = nameof(ComandaDocument.Opereaza);
    const string CmdValideaza = nameof(ComandaDocument.Valideaza);

    bool StergereaPeStare(Guid id) => CuSpatiu(os => {
        var doc = os.GetObjectByKey<Document>(id);
        return RegimDocument.MotivStergere(doc) == RegimDocument.Calculeaza(os, doc).Motiv(RegimDocument.Sterge);
    });

    protected override void Executa() {
        var nota = Nota(Ianuarie, new LinieNtcScena(Serviciu, ContFurnizor, 100));
        var r = Regim(nota.Id);
        var stergere = StergereaPeStare(nota.Id) && r.Poate(RegimDocument.Sterge);
        Verifica("SC-REGIM-01", "draft: editabil; Operează, Validează, Șterge disponibile; Anulează, Stornează, Corectează, Stinge refuzate cu motiv",
            r.Editabil && r.Poate(CmdOpereaza) && r.Poate(CmdValideaza) && r.Poate(RegimDocument.Sterge)
            && Refuzata(r, CmdAnuleaza, "Operat") && Refuzata(r, CmdStorneaza, "Operat") && Refuzata(r, CmdCorecteaza, "operat")
            && Refuzata(r, RegimDocument.Stinge, "operată")
            && r.Comenzi.Keys.All(RegimDocument.ComenziCunoscute.Contains));

        Opereaza(nota.Id);
        r = Regim(nota.Id);
        stergere &= StergereaPeStare(nota.Id) && Refuzata(r, RegimDocument.Sterge, "stornează");
        Verifica("SC-REGIM-02", "operat fără dependenți: needitabil; Anulează, Stornează, Corectează, Stinge disponibile; Operează, Validează, Șterge refuzate",
            !r.Editabil && r.Poate(CmdAnuleaza) && r.Poate(CmdStorneaza) && r.Poate(CmdCorecteaza) && r.Poate(RegimDocument.Stinge)
            && Refuzata(r, CmdOpereaza, "deja operat") && Refuzata(r, CmdValideaza, "deja operat")
            && Refuzata(r, RegimDocument.Sterge, "stornează"));

        Cost("NTC", nota.Id, 10);

        var factura = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
        Opereaza(factura.Id);
        Cost("FCT", factura.Id, 12);
        var plata = Trezorerie(incasare: false, 100);
        Opereaza(plata.Id);
        Imperecheaza(plata.Id, factura.Id, 100, Ianuarie);
        var rf = Regim(factura.Id);
        var rp = Regim(plata.Id);
        Verifica("SC-REGIM-03", "împerecherea refuză Anulează, Stornează și Corectează pe ambele documente, cu motivul împerecherilor",
            Refuzata(rf, CmdAnuleaza, "imperecheri") && Refuzata(rf, CmdStorneaza, "imperecheri") && Refuzata(rf, CmdCorecteaza, "imperecheri")
            && Refuzata(rp, CmdAnuleaza, "imperecheri") && Refuzata(rp, CmdStorneaza, "imperecheri") && !rp.Editabil);

        var dto = CuSpatiu(os => FacturaIntrareApply.Citeste(os, factura.Id));
        Verifica("SC-REGIM-04", "ReadDto-ul FCT expune exact regimul",
            dto.PoateEdita == rf.Editabil && dto.PoateOpera == rf.Poate(CmdOpereaza)
            && dto.PoateAnula == rf.Poate(CmdAnuleaza) && dto.PoateStorna == rf.Poate(CmdStorneaza));

        var stoc = Factura(Ianuarie, new LinieFctScena(2, 50, Stoc: true));
        var rezultat = Opereaza(stoc.Id);
        if (rezultat.ConexId is Guid nir) {
            Opereaza(nir);
            var rs = Regim(stoc.Id);
            Verifica("SC-REGIM-05", "conexul operat refuză Anulează și Stornează pe sursă, cu motivul conexelor",
                Refuzata(rs, CmdAnuleaza, "conexe") && Refuzata(rs, CmdStorneaza, "conexe") && Refuzata(rs, CmdCorecteaza, "conexe"));
        }
        else
            Verifica("SC-REGIM-05", "factura de stoc generează NIR conex", false);

        var libera = Nominalizata("LIBERA");
        var rn = Regim(libera.Factura);
        Verifica("SC-REGIM-15", "nominalizare 40 fără legătură: Anulează, Stornează și Corectează refuzate cu PARTIDA_CU_DEPENDENTI",
            Refuzata(rn, CmdAnuleaza, CoduriRefuz.PartidaCuDependenti) && Refuzata(rn, CmdStorneaza, CoduriRefuz.PartidaCuDependenti)
            && Refuzata(rn, CmdCorecteaza, CoduriRefuz.PartidaCuDependenti));
        Refuza("SC-REGIM-15", () => Storneaza(libera.Factura, Ianuarie), CoduriRefuz.PartidaCuDependenti);
        Verifica("SC-REGIM-15", "un regim care ar pierde dependentul permanent e abatere pentru SC-X-24, nu refuz pe data cerută",
            ClasaRefuz(libera.Factura, ComandaDocument.Storneaza, Ianuarie, CoduriRefuz.PartidaCuDependenti) == RefuzNepromis.Abatere
            && ClasaRefuz(libera.Factura, ComandaDocument.Corecteaza, Februarie, CoduriRefuz.PartidaCuDependenti) == RefuzNepromis.Abatere);

        var legata = Nominalizata("LEGATA");
        var legatura = Imperecheaza(legata.Nota, legata.Factura, 40, Ianuarie);

        InchideIanuarie();
        r = Regim(nota.Id);
        Verifica("SC-REGIM-06", "perioada închisă refuză doar Anulează, cu motivul perioadei; Stornează și Corectează rămân disponibile",
            Refuzata(r, CmdAnuleaza, "închisă") && r.Poate(CmdStorneaza) && r.Poate(CmdCorecteaza));

        rp = Regim(plata.Id);
        rf = Regim(factura.Id);
        Verifica("SC-REGIM-09", "împerecherea vie din luna închisă nu refuză Stornează și Corectează; Anulează e refuzată cu motivul perioadei",
            Refuzata(rp, CmdAnuleaza, "închisă") && rp.Poate(CmdStorneaza) && rp.Poate(CmdCorecteaza)
            && Refuzata(rf, CmdAnuleaza, "închisă") && rf.Poate(CmdStorneaza) && rf.Poate(CmdCorecteaza));
        Storneaza(plata.Id, Februarie);
        rf = Regim(factura.Id);
        Verifica("SC-REGIM-10", "stornarea plății inversează împerecherea; pe factură legătura inversată nu refuză Stornează și Corectează",
            Regim(plata.Id).Stare == StareDocument.Stornat && rf.Poate(CmdStorneaza) && rf.Poate(CmdCorecteaza));
        var corectie = Corecteaza(factura.Id);
        Verifica("SC-REGIM-10", "corecția facturii cu istoric de împerecheri reușește: original stornat, corecție Draft",
            Regim(factura.Id).Stare == StareDocument.Stornat && Regim(corectie).Stare == StareDocument.Draft);

        Storneaza(nota.Id, Februarie);
        r = Regim(nota.Id);
        Verifica("SC-REGIM-07", "stornat: needitabil, nicio comandă disponibilă",
            !r.Editabil && r.Comenzi.Count >= 6 && r.Comenzi.Values.All(motiv => motiv != null));
        stergere &= StergereaPeStare(nota.Id) && Refuzata(r, RegimDocument.Sterge, "stornat");
        Verifica("SC-REGIM-08", "ștergerea se decide numai pe stare: componenta ieftină dă motivul regimului pe Draft, Operat și Stornat",
            stergere);

        var rl = Regim(legata.Factura);
        Verifica("SC-REGIM-14", "limita 106-r7: legătura manuală de 40 acoperă exact nominalizarea, fără transfer în cub; regimul oferă Stornează și Corectează",
            rl.Poate(CmdStorneaza) && rl.Poate(CmdCorecteaza) && CuSpatiu(os => {
                var imp = os.GetObjectByKey<Imperechere>(legatura);
                return imp.Suma == 40 && !imp.Autogenerat && imp.TranzactieCubId == null;
            }));
        SoldPartida("SC-REGIM-14", Partida(legata.Factura, ContFurnizor, legata.Partener).Value, Ianuarie, -60);
        var intacta = Amprenta(legata.Factura);
        Refuza("SC-REGIM-14", () => Storneaza(legata.Factura, Februarie), CoduriRefuz.PartidaCuDependenti);
        Refuza("SC-REGIM-14", () => Corecteaza(legata.Factura), CoduriRefuz.PartidaCuDependenti);
        Verifica("SC-REGIM-14", "refuz atomic: postările facturii intacte, legătura neinversată", intacta == Amprenta(legata.Factura)
            && CuSpatiu(os => !os.GetObjectsQuery<Imperechere>().Any(i => i.InverseazaId == legatura)));

        if (!Privat) return;
        Comanda(os => { var p = os.CreateObject<PerioadaFiscala>(); p.An = An; p.Luna = 3; os.CommitChanges(); });
        Declarata("SC-REGIM-11", FormularFiscal.D300, 2);
        Declarata("SC-REGIM-12", FormularFiscal.D394, 3);
    }

    protected override int LimiteRegim => 2;

    // FCT servicii 100 și NTC care nominalizează FIFO 40 pe partida ei, pe un furnizor propriu.
    (Guid Factura, Guid Nota, Guid Partener) Nominalizata(string sufix) {
        var initial = Furnizor;
        Furnizor = CuSpatiu(os => {
            var partener = os.CreateObject<Partener>();
            partener.Cod = Marcaj + "-" + sufix; partener.Denumire = partener.Cod;
            os.CommitChanges(); return partener.ID;
        });
        try {
            var factura = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
            Opereaza(factura.Id);
            var nominalizare = Nota(Ianuarie, new LinieNtcScena(ContFurnizor, Serviciu, 40, Furnizor));
            Opereaza(nominalizare.Id);
            return (factura.Id, nominalizare.Id, Furnizor);
        }
        finally { Furnizor = initial; }
    }

    void Cost(string tip, Guid id, int plafon) {
        var citiri = CapturaSql.Masoara(() => Regim(id));
        Console.WriteLine($"     MĂSURAT (106f): regimul unui {tip} operat fără dependenți: {citiri.Count} instrucțiuni SQL, "
            + $"{citiri.Sum(c => c.Durata.TotalMilliseconds):0.0} ms [{string.Join("; ", citiri.Select(c => string.Join("+", CapturaSql.Tabele([c.Text]))))}].");
        Verifica("SC-REGIM-13", $"{tip} operat fără dependenți: regimul se citește în cel mult {plafon} instrucțiuni SQL", citiri.Count <= plafon);
    }

    void Declarata(string id, FormularFiscal formular, int luna) {
        var fiscala = Factura(new(An, luna, 5), new LinieFctScena(1, 100, "N21", Stoc: false));
        Opereaza(fiscala.Id);
        var inainte = Regim(fiscala.Id);
        Comanda(os => FiscalitateService.ConfirmaDepunerea(os, formular, An, luna,
            Fiscale.Versiune(os, formular, new(An, luna, 1), new DateOnly(An, luna, 1).AddMonths(1).AddDays(-1)), "ModelCheck"));
        var r = Regim(fiscala.Id);
        Verifica(id, $"{formular} depus, luna înregistrării deschisă: Anulează devine refuzată cu TVA_DEJA_DECLARATA; Stornează și Corectează rămân disponibile",
            inainte.Poate(CmdAnuleaza) && Refuzata(r, CmdAnuleaza, "TVA_DEJA_DECLARATA") && r.Poate(CmdStorneaza) && r.Poate(CmdCorecteaza));
        Refuza(id, () => Anuleaza(fiscala.Id), "TVA_DEJA_DECLARATA");
    }

    protected override void CurataNomenclatoare(IObjectSpace os, Purja purja) =>
        purja.Adauga(os.GetObjectsQuery<DepunereDeclaratie>().Where(d => d.Perioada >= An * 100 && d.Perioada < (An + 1) * 100));
}
