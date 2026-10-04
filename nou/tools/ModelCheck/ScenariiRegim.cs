using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Fct;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
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

    RegimDocument Regim(Guid id) => CuSpatiu(os => RegimDocument.Calculeaza(os, id));
    static bool Refuzata(RegimDocument r, string comanda, string fragmentMotiv) =>
        !r.Poate(comanda) && (r.Motiv(comanda) ?? "").Contains(fragmentMotiv, StringComparison.OrdinalIgnoreCase);

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

        var factura = Factura(Ianuarie, new LinieFctScena(1, 100, Stoc: false));
        Opereaza(factura.Id);
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

        InchideIanuarie();
        r = Regim(nota.Id);
        Verifica("SC-REGIM-06", "perioada închisă refuză doar Anulează, cu motivul perioadei; Stornează și Corectează rămân disponibile",
            Refuzata(r, CmdAnuleaza, "închisă") && r.Poate(CmdStorneaza) && r.Poate(CmdCorecteaza));

        Storneaza(nota.Id, Februarie);
        r = Regim(nota.Id);
        Verifica("SC-REGIM-07", "stornat: needitabil, nicio comandă disponibilă",
            !r.Editabil && r.Comenzi.Count >= 6 && r.Comenzi.Values.All(motiv => motiv != null));
        stergere &= StergereaPeStare(nota.Id) && Refuzata(r, RegimDocument.Sterge, "stornat");
        Verifica("SC-REGIM-08", "ștergerea se decide numai pe stare: componenta ieftină dă motivul regimului pe Draft, Operat și Stornat",
            stergere);
    }
}
