# Q14 — scrierea unui document, pe fiecare formă

Documentul: FCT `FA/EU-2500084872`, 49 de linii. În F0 = **99 de rânduri**
(50 contabile + 49 fiscale + 0 de stoc). În cub = **198 de postări**
(2 × 50 contabile + 2 × 49 fiscale) **plus un rând de `cub."Tranzactie"`**.
Cubul scrie exact de două ori mai multe rânduri, prin construcție.

Fiecare rulare e o tranzacție cu `ROLLBACK`; verificat după serie că numărul de
rânduri al tuturor tabelelor e neschimbat, pe ambele baze. 6 rulări, prima
aruncată, mediana celor 5 calde. Textele: `pas2/cub/q14.sql`, `f0/q14.sql`.

**Corecție de scară, declarată:** replicarea ×10 a pasului 1 a păstrat ACELAȘI
`DocumentId` pe toate cele zece copii. Fără filtru, Q14 la ×10 ar fi scris zece
documente, nu unul, și cifra ar fi măsurat altceva. Ambele texte au primit
`Data >= 2025-01-01` (neutru la ×1, documentul e din 2025) și `LIMIT 1` pe
tranzacție. Verificat: 198 de postări scrise la ×1 ȘI la ×10.

## Cifrele (mediana a 5 rulări calde, ms)

| Formă | ×1 | ×10 | ce verifică la scriere |
|---|---|---|---|
| F0 (3 registre) | 4,88 (4,63–5,47) | 5,31 (4,93–6,45) | **24 de triggere FK**, 4,0 ms din 4,88 |
| F1 (plat, 18 indexi) | 6,55 (5,93–10,93) | 5,18 (3,78–12,03) | niciunul; plătește 18 indexi |
| **F2 (partiționat, 4+4 indexi)** | **1,47** (1,34–1,62) | **1,68** (1,49–1,73) | niciunul |
| F3 (+ RANGE pe an) | 1,58 (1,42–3,44) | 1,66 (1,55–2,21) | niciunul |
| F2 + FK `NOT VALID` | 4,28 (4,14–4,98) | — | **9 triggere FK**, 2,8 ms din 4,28 |

## Ce spun

**Cubul partiționat scrie de 3,3× mai repede decât registrele de azi, deși scrie
de două ori mai multe rânduri** (1,47 ms față de 4,88 la ×1; 1,68 față de 5,31
la ×10). Diferența nu vine din inserție, ci din ce se verifică pe drum.

**Costul e în FK-uri, și el se transferă aproape integral.** Pe F0, cele 24 de
triggere consumă **4,0 din cele 4,88 ms** — 82% din timpul de scriere e
verificarea coordonatelor, nu scrierea lor. Cel mai scump la ×1:
`FK_RegistruContabil_DocumentDetalii_DetaliuId` (0,51 ms la 50 de apeluri),
apoi `..._Produse_DimensiuniCredit_MaterialId` (0,45) și
`..._Documente_DocumentId` (0,41). Pe cub, punând FK-uri echivalente, F2 urcă de
la 1,47 la **4,28 ms**, din care 2,8 ms în cele 9 triggere. Concluzia onestă:
**cubul nu e mai ieftin la scriere pentru că e cub, ci pentru că are jumătate
din numărul de FK-uri de verificat** — 9 în loc de 24 — iar asta e o consecință
a coloanelor de dimensiune care nu se mai dublează debit/credit (16 coloane
plate în F0 devin 10 coordonate pe postare).

Chiar și cu FK-uri, cubul rămâne cu ~12% mai ieftin decât F0 pe aceeași scriere,
scriind dublul rândurilor.

**F1 (portarea naivă) e cea mai scumpă formă la scriere**, 6,55 ms — mai scumpă
decât F0, fără niciun FK. Cei 18 indexi b-tree, câte unul pe fiecare coloană
cheie, se plătesc la fiecare inserție. Exact eroarea pe care o face astăzi
`RegistruContabil` cu cei 24 de indexi ai lui, mutată pe cub.

**Coloana polimorfă devine FK-abilă prin partiție.** `Unitate` înseamnă
documentul-partidă pe Contabil și lotul pe Stoc — o singură cheie străină pe
tabela logică e imposibilă. Pe F2, partiția pe `Spatiu` permite
`Postare_Contabil.Unitate → Documente` și `Postare_Stoc.Unitate → Loturi` ca
două constrângeri distincte, verificate de bază. E un argument pentru F2
independent de orice cifră de timp: **partiționarea pe spațiu cumpără
integritate, nu doar viteză.** SQL-ul: `pas2/fk-f2.sql`.

## Ce NU s-a măsurat

- FK-urile la ×10 (spec: „fără FK la ×10"). Costul unui trigger FK e o căutare
  pe PK-ul țintei, iar nomenclatoarele n-au fost replicate — cifra de la ×10 ar
  fi fost identică cu cea de la ×1, fără să probeze nimic.
- Încărcarea în bloc a unei luni (`INSERT … SELECT`, o dată), cerută de FZ-D8.
  Cifra echivalentă există din pasul 1: transformarea integrală
  `public → f1."Postare"` (1,16 M postări) și replicările ×10 (≈25 s per
  trecere de an), în `pas1/out/`.
- `VACUUM`/bloat după scrieri repetate: toate rulările fac `ROLLBACK`, deci
  produc rânduri moarte pe care niciuna dintre forme nu le plătește în serie.
