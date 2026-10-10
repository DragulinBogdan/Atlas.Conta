# C111, pasul 1b — așteptările dinaintea formei noi

**Data:** 2026-10-10. **Stare:** făcut. Contractul:
[C111](c111-coordonate-contract.md), regulile de oprire 2–4. Niciun cod de
produs schimbat; unealta `--amprente` are trei secțiuni noi.

Urmele, neversionate: `run-verificari/c111-pas1b/`.

## Amprenta analitică

`ModelCheck --amprente` scrie acum și:

| Secțiune | Cheia | Pe forma de azi |
|---|---|---|
| `analitic-repartitor.txt` | cont × repartitor de sold × lună | derivat după D5: titularul pe postarea cu partidă, altfel gestiunea reală, altfel nimic pe postarea cu fapt fiscal, altfel partenerul |
| `analitic-fiscal.txt` | cont × partener fiscal × loc fiscal × rol × lună | partenerul postării cu fapt fiscal; locul = gestiunea reală a postării |
| `analitic-contrapondere.txt` | cont × marcaj × lună | gestiunea virtuală, cu `Furnizor` și `Client` contopite în „terț” |

Pe baza de referință `Atlas.Conta.Import1C.Flax.C111P0`:

| Secțiune | Rânduri | SHA-256 |
|---|---|---|
| `analitic-repartitor.txt` | 6.289 | `EAD12A96…C5F6A26E` |
| `analitic-fiscal.txt` | 6.580 | `55AA2155…B35DFE1E` |
| `analitic-contrapondere.txt` | 6 | `8464B875…3CD41A94` |

Cele cinci secțiuni ale pasului 0 au aceleași amprente ca atunci.

## Diferențele așteptate după pasul 2

Pe import, față de fișierele de mai sus:

- **Locul fiscal.** 722 de postări cu fapt fiscal n-au azi gestiune (14.484
  au). După pasul 2 îl primesc din latura internă a documentului (C111-Q8),
  deci rândurile lor din `analitic-fiscal` trec de pe locul „-” pe un loc
  numit. Comparația se face și cu locul scos din cheie, unde trebuie să iasă
  identică.
- **S7, S8, S10.** Flax n-are decont în ianuarie, deci S10 nu apare.
  Viramentele și descărcările cu primitor intern n-au fost numărate pe baza
  de referință; diferențele lor, dacă apar, se numesc rând cu rând la pasul 6.

Orice altă diferență oprește pasul.

## Martorul A → B (C111-R7)

Pe o copie a bazei de referință, o postare contabilă fără unitate, fără fapt
fiscal și fără partener a primit alt repartitor intern, de același fel. Baza
are 5.891 de asemenea postări.

| Secțiune | Rânduri diferite |
|---|---|
| balanța, stocul, partidele, D300, D394 | 0 |
| `analitic-repartitor` | 3 |
| `analitic-fiscal`, `analitic-contrapondere` | 0 |

Cele cinci secțiuni ale pasului 0 nu văd mutația; secțiunea analitică o
vede. Copia e ștearsă.

## Catalogul

[`scenarii/C111.md`](scenarii/C111.md): 22 de rânduri, SC-C111-01…22, câte
unul pe forma din D5 și pe schimbările S1, S2, S7, S8 și S10. 21 sunt
specificate și neimplementate; SC-C111-22 e martorul de mai sus. Trei rânduri
numesc un fapt de stabilit la implementare: latura internă a decontului,
atingerea lui S10 prin culegere și scena care produce stornoul peste mai
multe tranzacții.
