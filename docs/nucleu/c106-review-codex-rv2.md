# C106 — închiderea observației R5

2026-10-04, Codex. Cod verificat: `86978f7`, peste `5941b28`.
Predare: `comunicari/2026-10-04-1434-claude-codex-c106-rv1-corectat.md`.

**C106-R5 închisă. Fără observații noi în diferența revizuită.**
R1…R4 rămân închise tehnic conform [RV1](c106-review-codex-rv1.md).
106-r7 rămâne o limită acceptată la comandă, conform tranșării owner-ului
din 2026-10-04, consemnată în decizia 106(k). Nu mai este o aprobare
restantă a acestui review.

## Ce a fost reverificat

- `ProbeRegim.RefuzLaComanda` și excepția globală pe mesaj au dispărut.
  Clasificarea citește nominalizările la data cerută și la netul final;
  dependentul permanent fără legături este abatere. Cazul temporal
  SC-NTC-22 trece separat, iar luna închisă a imobilizărilor nu este
  tratată drept refuz dependent numai de data aleasă.
- Limita 106-r7 are clasificare și contor distinct. REGIM declară exact
  două refuzuri; celelalte scene au implicit zero. Un număr diferit pică
  verificarea. Partenerii legăturilor vii sunt calculați de probă, fără
  apelul `ImperechereService.Motive`.
- SC-REGIM-14 păstrează contraexemplul numeric din RV1: FCT 100, NTC 40,
  legătură manuală exact 40, fără transfer cub, sold −60. Comenzile
  refuză atomic; originalele și legătura rămân intacte. SC-REGIM-15
  verifică dependentul permanent fără legătură.
- Decizia, catalogul și limitele includ explicit nominalizarea acoperită
  integral de o legătură manuală fără transfer. Disponibilitatea oferită
  de regim în acest caz nu garantează acceptarea stornării sursei.

Diferența `5941b28..86978f7` nu modifică producția: schimbările de cod sunt
în ModelCheck. Pentru această reverificare am rulat REGIM, NTC și IMO pe
ambele profiluri, nu încă o integrală a producției.

## Mutant executat, nu doar clasificator apelat direct

Am eliminat temporar numai ramura
`nominalizari.Value.Motiv(DateOnly.MaxValue, imperecheri.Parteneri)` din
`GardieniRetragere.Citeste`, la stornare. Gardienii comenzii și
clasificatorul au rămas neatinși.

Rularea **NTC / Bugetar** a raportat două abateri
„Storneaza oferită de regim, refuzată de comandă: PARTIDA_CU_DEPENDENTI”
și exact un test eșuat: **SC-X-24**, exit 1. SC-REGIM-15 nu a fost
selectat în această rulare, deci nu aserțiunea lui explicită a detectat
mutantul. Cazul temporal a rămas numărat separat, o dată; zero limite
106-r7 în scena NTC.

Această probă acoperă omisiunea predicatului din compoziția regimului;
nu este o campanie de mutații asupra tuturor cititorilor cubului.
Sursa a fost restaurată octet cu octet. Patch reproductibil:
`run-verificari/codex-c106-rv2/mutant.patch`; `git apply --check` a trecut.

## Dovezi și comenzi

- Surse nemodificate, REGIM/NTC/IMO, ambele profiluri: exit 0, zero FAIL,
  `run-verificari/20261004-143958-747/rezultat.json`.
  NTC: 16/17 comenzi, câte un refuz pe data cerută, zero limite.
  REGIM: 6/8 comenzi, exact două refuzuri pe limita 106-r7 pe fiecare profil.
- Mutant, NTC/Bugetar: exit 1 așteptat, eșec SC-X-24,
  `run-verificari/20261004-144210-847/rezultat.json`; purjă fără reziduu.
- Integrala **3.459/4.680 OK** este dovada predării Claude din
  `run-verificari/20261004-142733-206/rezultat.json`, exit 0, pe modificările
  de lucru înaintea commitului `86978f7`; nu o prezint ca rulare nouă Codex.

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip REGIM,NTC,IMO -Profil Ambele -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip NTC -Profil Bugetar -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip REGIM,NTC -Profil Ambele -Sufix .CodexXReview
```

A doua comandă rulează mutantul; a treia reconstruiește și verifică după
restaurare: exit 0 pe ambele profiluri, zero FAIL,
`run-verificari/20261004-144258-785/rezultat.json`.
Nu s-au repetat browserul, HTTP sau driftul, producția fiind
neschimbată de această corectură. Fără modificări finale de cod și fără commit.
