namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class PerfCub {
    /// <summary>Scena rămâne în bază după rulare, cu utilizatorul ei administrativ (D9-A1).</summary>
    public bool Pastrata { get; init; }
    protected override bool Pastreaza => Pastrata;

    public (int An, int Luna) LunaMasurata => Masurata;

    // Cheile de control legate numai de cont sau de gestiune cresc cu factorul; cele legate de partenerul scenei rămân.
    public static readonly Dictionary<string, bool> CresteCuFactorul = new() {
        ["BAL"] = false, ["FISA"] = false, ["SPART"] = false, ["PREST"] = false,
        ["JRN"] = true, ["STOC"] = true,
        ["FIFO"] = false, ["PDISP"] = false, ["RECON"] = false,
    };

    /// <summary>Așteptările scenei păstrate la treapta k, după documentul lung, cu factorul f de multiplicare.</summary>
    public Dictionary<string, decimal> AsteptatLaScara(string operatie, int k, int f) {
        var cod = operatie.Split('-')[0];
        var asteptat = Asteptat(operatie, k, false);
        if (cod == "STOC") {
            // Documentul lung consumă o bucată de 10 lei din fiecare lot al lunii măsurate, după ultima treaptă.
            asteptat["stoc.cantitate"] -= k; asteptat["stoc.valoare"] -= 10 * k;
            asteptat["consum.cantitate"] += k; asteptat["consum.valoare"] += 10 * k;
        }
        return CresteCuFactorul[cod] ? asteptat.ToDictionary(a => a.Key, a => a.Value * f) : asteptat;
    }
}
