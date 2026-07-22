# Inventar legacy (pasul 1 din plan) — index

Rama generală: **decizia 21** — legacy = direcție și evidență de
funcționalitate, nu sursă canonică de seed. Extrasele brute (proceduri,
config): `db/export/`.

| Fișier | Acoperă | Stare |
|---|---|---|
| `00-motor-postare.md` | staging, formule, stoc, note, conex, plăți auto, anulare, aprobări, reguli hardcodate, refolosiri de câmpuri | complet (întrebări deschise în §13) |
| `01-factura-intrare.md` | FCT → `FacturaIntrare` (+ import extern Tethys) | complet |
| `02-nir.md` | NIR → `NIR` (nașterea loturilor, +1 stoc) | complet |
| `03-bon-consum.md` | BCS → `BonConsum` (−magazie/+consum) | complet (fără date vii) |
| `04-nota-transfer.md` | BTR → `NotaTransfer` (primul vertical slice recomandat) | complet (fără date vii) |
| `05-lista-diferente-inventar.md` | LDI → `ListaDiferenteInventar` (bidirecțional pe loturi) | complet |
| `06-decont.md` | DEC → `Decont` (postare explicită pe linie; flux avans) | complet |
| `07-factura-iesire.md` | FCT IESIRE → `FacturaIesire` (creanță, fără stoc) | complet |
| `08-raport-productie.md` | BPR → `RaportProductie` | AMÂNAT (decizia 19) — slot rezervat |
| `09-plati-imperechere.md` | BREGISTRU/casierie → `Plata`/`Incasare`/`Imperechere` | complet |
| `10-nomenclatoare.md` | Repartitori, Clasă/Tip, Produs/Lot, CPLAN | complet |
| `11-testul-bazei.md` | **pasul 2**: câmpurile `Document`/`DocumentDetaliu` de bază, verdict per câmp + tranșarea lanțului de valori și a dimensiunilor | complet (tranșat) |

Închis: BF = variantă de `FacturaIntrare` (confirmat, decizia 19) — același
flux cu conex NIR; combo-urile BF (254/295/296) se tratează ca evidență FCT.
Neacoperit încă (conștient): modulul aprobări în profunzime (00 §9),
închiderea de perioadă/an (`spInchidereStocuri`, `spNotaInchidere*` — exportate
în `db/export/proc/`, relevante la migrare ca model pentru solduri de
deschidere).

Pasul 2 (testul bazei) e închis — `11-testul-bazei.md`, decizia 22 în
CLAUDE.md. Pasul următor din plan: **pasul 3 — modelul nou** (clase XAF bază +
derivate TPT, validare declarativă, tabelele de politică).
