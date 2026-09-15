namespace Atlas.Conta.BackOffice.Module.Api.Politici;

// Felia 23 (F23-D8) — raportul de verificare a profilului, pe sârmă.
//
// Oglinda plată a lui `ConstatareProfil` (Module/Motor): aceleași patru câmpuri,
// cu `Fel` ca STRING (numele enum-ului `FelConstatare`) — convenția feliilor
// pentru enum-uri (57a), cu eticheta lizibilă în `metadata.json`, o singură
// sursă pentru XAF și React.
//
// Nu e o listă de erori: `RandManual` e o CONSTATARE, nu un refuz — clientul are
// voie să-și schimbe politicile (decizia 4). Raportul spune doar ce s-a abătut
// de la profilul livrat; seed-ul rămâne cel care aruncă.
public sealed class ConstatareProfilDto {
    // Numele tabelului de politică, așa cum îl vede operatorul („Politica de
    // scadență", „Mapări"), nu numele CLR.
    public string Tabel { get; set; }
    // Eticheta LIZIBILĂ a rândului (codul tipului de document, codul tipului de
    // TVA…): raportul se citește, nu se dereferențiază — de aceea nu poartă
    // `Guid`-ul rândului.
    public string Cheie { get; set; }
    public string Fel { get; set; }
    public string Mesaj { get; set; }
}
