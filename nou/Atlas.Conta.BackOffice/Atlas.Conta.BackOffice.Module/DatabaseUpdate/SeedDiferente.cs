using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

internal static class SeedDiferente {
    internal static void NIR(IObjectSpace os, string clarificare, string plus, string imputat,
            string personal, params (string Clasa, string Cheltuiala, string Drum)[] clase) {
        var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == "NIR");
        Guid Cont(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol)?.ID
            ?? throw new InvalidOperationException($"Politica NIR cere contul {simbol} în profil.");
        foreach (var (cod, cheltuiala, drum) in clase) {
            var clasa = os.FirstOrDefault<ClasaProdus>(c => c.Cod == cod);
            if (clasa == null) throw new InvalidOperationException($"Clasa {cod} lipsește din profil.");
            foreach (var cauza in Enum.GetValues<CauzaDiferentei>()) {
                var simbol = cauza switch {
                    CauzaDiferentei.InClarificare => clarificare,
                    CauzaDiferentei.Plus => plus,
                    CauzaDiferentei.Imputabila => imputat,
                    CauzaDiferentei.PeDrum => drum,
                    _ => cheltuiala,
                };
                if (simbol == null) continue;
                var cont = Cont(simbol);
                Guid? angajat = cauza == CauzaDiferentei.Imputabila ? Cont(personal) : null;
                ContaSeeder.Aliniaza<PoliticaDiferenta>(os, $"NIR/{cauza}/{cod}",
                    p => p.TipDocumentId == tip.ID && p.ClasaId == clasa.ID && p.Cauza == cauza, p => {
                        p.TipDocumentId = tip.ID; p.ClasaId = clasa.ID; p.Cauza = cauza;
                        p.ContId = cont; p.ContPersonalId = angajat;
                    });
            }
        }
    }
}
