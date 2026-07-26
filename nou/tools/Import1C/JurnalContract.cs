namespace Import1C;

// Raportul INTEGRAL al contractului lunar, scris pe disc.
//
// Consola rămâne plafonată deliberat (30 chei de stoc detaliate, 15 justificate):
// o lună cu sute de diferențe ar îneca raportul pe care îl citește omul. Dar
// „detaliem 30 din 48" înseamnă că restul nu există nicăieri — iar o listă fără
// cap nu se poate diagnostica. Fișierul de aici e complementul: TOATE cheile
// nejustificate, toate conturile picate, categoriile justificate agregate, lună
// cu lună, într-un fișier per rulare, anunțat în raportul fiecărei luni.
//
// Se scrie cu `AutoFlush`: o rulare care se oprește la prima lună picată (stopul
// dur din §12.4) trebuie să lase pe disc exact ce a măsurat până acolo.
sealed class JurnalContract : IDisposable {
    readonly StreamWriter scriitor;

    public string Cale { get; }

    public JurnalContract(string director) {
        Cale = Path.Combine(director, $"reconciliere-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
        scriitor = new StreamWriter(Cale, append: false) { AutoFlush = true };
        scriitor.WriteLine($"Raportul integral al contractului de reconciliere — "
            + $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        scriitor.WriteLine("Consola e plafonată; aici sunt TOATE diferențele, lună cu lună.");
    }

    public void Scrie(string linie) => scriitor.WriteLine(linie);

    public void Dispose() => scriitor.Dispose();
}
