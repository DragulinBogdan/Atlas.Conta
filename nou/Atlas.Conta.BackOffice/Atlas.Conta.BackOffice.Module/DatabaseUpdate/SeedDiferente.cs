using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.DatabaseUpdate;

internal static class SeedDiferente {
    // 099-r1 (129): imputarea minusului de inventar, ca la NIR; cheltuiala și plusul rămân pe regula de contare.
    internal static void LDI(IObjectSpace os, string imputat, string personal, params string[] clase) {
        var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == "LDI");
        Guid Cont(string simbol) => os.FirstOrDefault<Cont>(c => c.Simbol == simbol)?.ID
            ?? throw new InvalidOperationException($"Politica LDI cere contul {simbol} în profil.");
        foreach (var cod in clase) {
            var clasa = os.FirstOrDefault<ClasaProdus>(c => c.Cod == cod)
                ?? throw new InvalidOperationException($"Clasa {cod} lipsește din profil.");
            ContaSeeder.Aliniaza<PoliticaDiferenta>(os, $"LDI/{CauzaDiferentei.Imputabila}/{cod}",
                p => p.TipDocumentId == tip.ID && p.ClasaId == clasa.ID && p.Cauza == CauzaDiferentei.Imputabila, p => {
                    p.TipDocumentId = tip.ID; p.ClasaId = clasa.ID; p.Cauza = CauzaDiferentei.Imputabila;
                    p.ContId = Cont(imputat); p.ContPersonalId = Cont(personal);
                });
        }
    }

    // T-r2: reziduul lotului golit de retur; minusul e neimputabil, plusul e venit.
    internal static void Reziduu(IObjectSpace os, string codTip, string minus, string plus, params string[] clase) {
        var tip = os.FirstOrDefault<TipDocument>(t => t.Cod == codTip);
        foreach (var cod in clase) {
            var clasa = os.FirstOrDefault<ClasaProdus>(c => c.Cod == cod)
                ?? throw new InvalidOperationException($"Clasa {cod} lipsește din profil.");
            foreach (var (cauza, simbol) in new[] { (CauzaDiferentei.Neimputabila, minus), (CauzaDiferentei.Plus, plus) }) {
                var cont = os.FirstOrDefault<Cont>(c => c.Simbol == simbol)?.ID
                    ?? throw new InvalidOperationException($"Politica {codTip} cere contul {simbol} în profil.");
                ContaSeeder.Aliniaza<PoliticaDiferenta>(os, $"{codTip}/{cauza}/{cod}",
                    p => p.TipDocumentId == tip.ID && p.ClasaId == clasa.ID && p.Cauza == cauza, p => {
                        p.TipDocumentId = tip.ID; p.ClasaId = clasa.ID; p.Cauza = cauza; p.ContId = cont; p.ContPersonalId = null;
                    });
            }
        }
    }

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
