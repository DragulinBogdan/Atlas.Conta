namespace Atlas.Conta.BackOffice.Module.BusinessObjects;

// PROVENIENȚA unui rând de politică sau de nomenclator seed-uit (felia 23,
// F23-D4). Pe cele 12 politici, pe `PoliticaTvaImplicit` și pe cele patru
// nomenclatoare pe care seed-ul le tratează ca ale lui (`TipTva`, `Cont`,
// `ClasaProdus`, `TipMaterial`). NU pe repartitori (structura clientului) și
// NU pe `Societate` (rândul se creează gol, nu se rescrie).
//
// Timbrul e PROPRIETATEA rândului (83a). Cele două jumătăți ale contractului,
// ambele mecanice:
//   * SEED-ul pune `true` pe rândurile pe care le CREEAZĂ și ALINIAZĂ la cod
//     rândurile care îl poartă (`ContaSeeder.Aliniaza`), fără să atingă cheia;
//   * GARDIANUL pune `false` la orice scriere venită pe ușa securizată
//     (`GardianEditare`), ca un câmp server-owned inversat; `true` cules pe un
//     rând NOU se refuză — proveniența n-o declară clientul.
// Pe ușa de sistem (motor, Import1C, ModelCheck) nu se întâmplă nimic.
//
// Limita DECLARATĂ (81-r3/83e): un rând seed-uit și editat manual ÎNAINTE de
// migrația feliei 23 e marcat `DinSeed` de backfill, deci re-seed-ul îl
// corectează — divergența lui e nedetectabilă retroactiv (nu există istoric al
// câmpului). De aici înainte, nu.
//
// Ce cumpără flag-ul: proprietatea rândului la re-seed, vizibilitatea
// („seed / manual" în grilă) și raportul de profil (`VerificareProfilService`).
public interface ICuProvenienta {
    bool DinSeed { get; set; }
}
