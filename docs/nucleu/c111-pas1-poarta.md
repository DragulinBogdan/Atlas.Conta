# C111, pasul 1 — poarta coordonatelor

- Tip: analiza
- Data: 2026-10-10
- Alimentează: 111 (raport de pas al contractului C111)

**Data:** 2026-10-10. **Stare:** făcut. Contractul:
[C111](c111-coordonate-contract.md), D9 și D10. Linia de bază:
[pasul 0](c111-pas0-linia-de-baza.md). Nicio schimbare de schemă și nicio
schimbare de comportament.

Urmele, neversionate: `run-verificari/c111-pas1/` și
`run-verificari/20261010-133651-308/`.

## Rezumat

| Ce | Cifra | Față de pasul 0 |
|---|---|---|
| Integrala ModelCheck, bugetar | 3.579 OK, 0 FAIL | +1, proba `C111-D10` |
| Integrala ModelCheck, privat | 4.860 OK, 0 FAIL | +1, proba `C111-D10` |
| Amprentele bazei de import, cu binarul nou | cele cinci secțiuni | identice |
| Raportul SAF-T S al bazei de import | 74 de linii comparate | identic, în afara căii fișierului |
| Metadata clientului și tipurile OpenAPI | regenerate | fără diferențe |

## Poarta

`Postare.Partener` și `Postare.Gestiune` poartă `[Obsolete]` cu diagnosticul
`C111`. În `Module` diagnosticul e eroare de compilare. Îl dezactivează prin
`#pragma` numai `Cub/Citiri` (9 fișiere), `Cub/Randuri.cs` și lista de
coloane din `UI/ContaUiBaseline.cs`. Proba `C111-D10` verifică lista și
faptul că niciun proiect de producție nu tace diagnosticul.

Compilatorul a numit 59 de rânduri în 19 fișiere. 43 erau deja în cititori
sau în scriitor, iar 2 sunt lista de coloane. Celelalte 14, în 9 fișiere, au
trecut pe cititori numiți:

| Consumatorul | Înainte | Acum |
|---|---|---|
| `Motor/Fapte.cs`, unitățile sursei | proiecție pe `Partener` | `Partide.UnitatiAle` |
| `Motor/Fapte.cs`, partidele disponibile | grupare pe `Partener` | `Partide.MiscariPeTitulari`, cu `Titular` |
| `Motor/ImperechereService.cs` | cheie de grupare cu `Partener` | `Partide.Titular` |
| `Proiectii/ImperecheriProiectii.cs` | `Nete` local | `Partide.Nete`, cu `Titular` |
| `Cub/Materializare.cs` | filtru `Partener != null` | `Partide.CuTitular` |
| `Cub/Materializare.StingereDeschidere.cs` | comparație pe `Partener` | `Partide.Titular` |
| `Motor/DiagnosticTvaService.cs` | `nameof(p.Partener)` | `Fiscale.MembruPartener` |
| `Motor/ImobilizariFapte.cs` | proiecție pe ambele coloane | `Imobilizari.PozitiiCitite`, cu `Tert` și `Loc` |
| `Proiectii/ContabilProiectii.cs`, atomii și fișa | ambele coloane | `Contabil.Analitice`, cu `AxaPartener` și `AxaGestiune` |
| `Saft/SaftProiectii.PeCub.Stocuri.cs` | `Gestiune` | `Loturi.PostariCitite`, cu `Loc` |

`PostareJurnal.Partener` a devenit `TertComercial` (C111-R9); cei patru
consumatori ai lui din `SaftProiectii.PeCub.cs` îl citesc sub numele nou.

## Ce a rămas pentru pasul 2

- DTO-urile cititorilor cu o singură clasă își păstrează numele:
  `FaptFiscal.Partener`, `SoldPartida.Partener`, `PostarePlata.Partener`,
  `SoldLot.GestiuneId`. Sunt alimentate din `Cub/Citiri`, deci la pasul 2 se
  schimbă numai sursa lor.
- `ModelCheck` și `Import1C` tac diagnosticul: scenele compară rânduri întregi
  și își rescriu așteptările la pasul 2 (D10).
- Nucleul (`Coordonate`, `Capat`) și declaranții sunt neatinși.
- Dacă `[Obsolete]` schimbă ceva în modelul XAF al listei de postări n-am
  probat în browser; integrala, care construiește modelul hostului Blazor, și
  metadata clientului sunt neschimbate. Atributul dispare la pasul 2.

## Verificarea

- `scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .C111P1
  -PregatesteBaze`; bazele cu sufixul `.C111P1` sunt șterse.
- `ModelCheck --amprente Atlas.Conta.Import1C.Flax.C111P0 2025 1`, comparat cu
  `run-verificari/c111-pas0/amprente/`.
- `Import1C --saft-s 2025 1` pe aceeași bază, raportul comparat cu cel al
  pasului 0.
- `ModelCheck --dump-metadata` și `pnpm verifica:drift`.
