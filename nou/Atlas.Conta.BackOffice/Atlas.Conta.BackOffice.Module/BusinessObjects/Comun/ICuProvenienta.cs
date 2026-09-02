namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// PROVENIENȚA unui rând de politică sau de nomenclator seed-uit (felia 23,
// F23-D4). Pe cele 12 politici, pe `PoliticaTvaImplicit` și pe cele patru
// nomenclatoare pe care seed-ul le tratează ca ale lui (`TipTva`, `Cont`,
// `ClasaProdus`, `TipMaterial`). NU pe repartitori (structura clientului) și
// NU pe `Societate` (rândul se creează gol, nu se rescrie).
//
// Cele două jumătăți ale contractului, ambele mecanice:
//   * SEED-ul pune `true` pe rândurile pe care le CREEAZĂ și pe cele pe care le
//     GĂSEȘTE pe cheia lui (`ContaSeeder.Seedat`);
//   * GARDIANUL pune `false` la orice scriere venită pe ușa securizată
//     (`GardianEditare`), ca un câmp server-owned inversat; `true` cules pe un
//     rând NOU se refuză — proveniența n-o declară clientul.
// Pe ușa de sistem (motor, Import1C, ModelCheck) nu se întâmplă nimic.
//
// Limita DECLARATĂ: un rând seed-uit și editat manual ÎNAINTE de migrația
// feliei 23 va fi marcat `DinSeed` la prima trecere a seed-ului — divergența lui
// e nedetectabilă retroactiv (nu există istoric al câmpului). De aici înainte,
// nu.
//
// Ce cumpără flag-ul azi: vizibilitatea („seed / manual" în grilă) și raportul
// de profil (`VerificareProfilService`, F23-D8). Semantica de re-seed NU se
// schimbă în felia asta — „seed-ul corectează DOAR rândurile `DinSeed`" rămâne
// restanță cu nume.
public interface ICuProvenienta {
    bool DinSeed { get; set; }
}
