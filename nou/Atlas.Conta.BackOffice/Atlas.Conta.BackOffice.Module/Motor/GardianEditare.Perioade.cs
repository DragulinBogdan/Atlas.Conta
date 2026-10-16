using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Motor;

public sealed partial class GardianEditare {
    // (n) Perioada fiscală (F27-D1, E3-D7): `An`/`Luna` sunt identitatea verigii (o lună rescrisă ar muta granița
    // sub documentele operate); starea lunii și a exercițiului o scrie doar `PerioadaService`, pe ușa non-secured.
    static void VerificaPerioadaFiscala(IObjectSpace os, PerioadaFiscala perioada, ICollection<string> erori) {
        if (EsteSters(os, perioada)) {
            if (perioada.Inchisa)
                erori.Add($"Perioada {EtichetaPerioada(perioada)} e închisă — o perioadă închisă nu se șterge. "
                    + "Redeschideți-o întâi.");
            if (os.GetObjectsQuery<InchiderePerioada>().Any(i => i.PerioadaId == perioada.ID))
                erori.Add($"Perioada {EtichetaPerioada(perioada)} are istoric de închideri — "
                    + "ștergerea ei ar rupe urma operațiilor motorului.");
            return;
        }
        if (perioada.An < AnMinimPerioada || perioada.An > AnMaximPerioada)
            erori.Add($"Anul perioadei trebuie să fie între {AnMinimPerioada} și {AnMaximPerioada}.");
        if (perioada.Luna < 1 || perioada.Luna > 12)
            erori.Add("Luna perioadei trebuie să fie între 1 și 12.");
        if (os.IsNewObject(perioada)) {
            if (perioada.Inchisa || perioada.InchisaLa != null || perioada.InchisaPrimaOara != null
                    || perioada.ExercitiuInchis || perioada.ExercitiuInchisLa != null)
                erori.Add("O perioadă nouă se creează DESCHISĂ — închiderea o face doar motorul (Închide / Redeschide).");
            if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.An == perioada.An && p.Luna == perioada.Luna))
                erori.Add($"Perioada {EtichetaPerioada(perioada)} există deja — o lună are o singură verigă în lanț.");
            if (os.GetObjectsQuery<PerioadaFiscala>().Any(p => p.Inchisa
                    && (p.An > perioada.An || p.An == perioada.An && p.Luna > perioada.Luna)))
                erori.Add($"Perioada {EtichetaPerioada(perioada)} ar sta înaintea unei perioade închise — lanțul se "
                    + "închide în ordine, deci o lună nouă se adaugă numai după ultima lună închisă.");
            return;
        }
        var originale = Originale(os, perioada);
        if (originale == null)
            return;
        if ((originale[nameof(PerioadaFiscala.An)] as int?) != perioada.An
                || (originale[nameof(PerioadaFiscala.Luna)] as int?) != perioada.Luna)
            erori.Add($"Luna unei perioade existente nu se schimbă ({EtichetaPerioada(perioada)}) — "
                + "ștergeți perioada greșită și creați-o pe cea corectă.");
        if ((originale[nameof(PerioadaFiscala.Inchisa)] as bool?) != perioada.Inchisa
                || (originale[nameof(PerioadaFiscala.InchisaLa)] as DateTime?) != perioada.InchisaLa
                || (originale[nameof(PerioadaFiscala.InchisaPrimaOara)] as DateTime?) != perioada.InchisaPrimaOara
                || (originale[nameof(PerioadaFiscala.ExercitiuInchis)] as bool?) != perioada.ExercitiuInchis
                || (originale[nameof(PerioadaFiscala.ExercitiuInchisLa)] as DateTime?) != perioada.ExercitiuInchisLa)
            erori.Add("Închiderea perioadei și a exercițiului o face doar motorul (Închide / Redeschide).");
    }

    // Aceleași margini ca pe API (`PerioadeController.AnMinim/AnMaxim`).
    const int AnMinimPerioada = 2000, AnMaximPerioada = 2100;

    static string EtichetaPerioada(PerioadaFiscala perioada) => $"{perioada.Luna:00}/{perioada.An}";
}
