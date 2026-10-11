# Unde suntem

Generat de `docskit gen` din antete, registru și cod; nu se editează. Cronologia e în jurnal; comparația paginilor cu ultimul commit o face `docskit check` (gardianul `actualizat`).

## Rezumat

| Ce | Cât |
|---|---|
| Decizii | 118: 85 active, 33 amendate parțial, 0 depășite, 0 propuse |
| Restanțe | 380: 283 deschise, 97 închise |
| Pagini de stare curentă | 7 |
| Contracte de felie (`Tip: contract`) | 0, deschise 0 |
| Cataloage vii (`Tip: catalog`) | 0 |
| Fișiere Markdown în `docs/` | 295 |
| Citări de decizii din cod (`// N`) | 126 |

## Decizii în vigoare pe arie

- **domeniu-si-operare** (51): 10, 13, 14, 15, 16 (amendată de 89), 17, 20, 22, 23, 25, 26 (amendată de 112), 27, 30, 31 (amendată de 76, 88), 32, 33, 36 (amendată de 71), 37, 41, 42 (amendată de 104, 106), 46 (amendată de 113), 48 (amendată de 76), 49, 54, 55, 62, 64 (amendată de 84), 65, 75, 76, 82, 86, 87 (amendată de 97), 89, 90 (amendată de 91, 92, 93, 95, 96, 110), 93, 95, 97 (amendată de 98), 98, 100 (amendată de 102), 101 (amendată de 102), 102, 104 (amendată de 106), 106, 109, 111, 112, 114 (amendată de 116), 115, 116 (amendată de 117), 117
- **politici-si-fiscalitate** (22): 20, 36 (amendată de 71), 42 (amendată de 104, 106), 46 (amendată de 113), 56 (amendată de 77, 81), 66, 69, 70, 71 (amendată de 72), 72, 73 (amendată de 74, 105), 74 (amendată de 105), 79 (amendată de 80), 80, 81, 83 (amendată de 84, 86, 104), 84, 86, 87 (amendată de 97), 103, 111, 113
- **api-si-client** (26): 39, 42 (amendată de 104, 106), 43 (amendată de 104), 53, 55, 56 (amendată de 77, 81), 57, 58, 60, 66, 68, 70, 77, 79 (amendată de 80), 80, 81, 84, 85, 86, 87 (amendată de 97), 89, 103, 104 (amendată de 106), 106, 108 (amendată de 110), 111
- **dezvoltare-si-validare** (33): 14, 15, 21, 23, 26 (amendată de 112), 30, 34, 36 (amendată de 71), 39, 42 (amendată de 104, 106), 43 (amendată de 104), 45 (amendată de 75), 47 (amendată de 107), 49, 50, 56 (amendată de 77, 81), 66, 70, 80, 81, 84, 85, 87 (amendată de 97), 89, 90 (amendată de 91, 92, 93, 95, 96, 110), 91 (amendată de 107, 118), 102, 104 (amendată de 106), 106, 107, 108 (amendată de 110), 115, 118
- **README** (6): 42 (amendată de 104, 106), 76, 91 (amendată de 107, 118), 103, 104 (amendată de 106), 110 (amendată de 113)
- **limite-curente** (29): 37, 41, 42 (amendată de 104, 106), 43 (amendată de 104), 46 (amendată de 113), 53, 55, 72, 75, 77, 80, 81, 84, 85, 87 (amendată de 97), 89, 91 (amendată de 107, 118), 94, 95, 97 (amendată de 98), 102, 103, 104 (amendată de 106), 106, 107, 108 (amendată de 110), 109, 115, 116 (amendată de 117)
- **(fără arie)** (32): 1, 2, 3 (amendată de 89), 4, 5, 6, 7, 8, 9, 11, 12, 18, 19, 24, 28, 29, 35, 38 (amendată de 81, 84), 40, 44, 51, 52, 59, 61, 63, 67, 78, 88 (amendată de 103), 92 (amendată de 102), 96 (amendată de 100, 102), 99, 105

Titlurile stau în [indexul deciziilor](decizii/README.md); o decizie se deschide din fișierul ei.

## Decizii depășite

Niciuna.

## Restanțe deschise, pe stare

| Stare | Câte |
|---|---|
| activă | 41 |
| după PoC | 206 |
| migrare | 24 |
| depășită de 091 | 2 |
| docs | 6 |
| (în afara vocabularului) | 4 |

### Lista `activă` (41, prag 45)

**domeniu-si-operare** (32)

- `51e` PoliticaEvaluare (CMP) — sub 90c devine parametrul „unitatea de evaluare” al unității nominalizate
- `75-r4` PretEvaluare 6 zecimale pe cantități mari vs invariantul 46d
- `76-r1` netarea plafonului e per (repartitor × LATURĂ), nu per CONT (55 chei / 36 note amestecă conturi de c…
- `76-r2` caStins se scade din AMBELE sensuri — inatacabil azi, **devine real când un tip cu partener pe latur…
- `76-r3` perf AsignatFataDe (entități polimorfe, chemat de 2 × nr. contrapartide)…
- `86-r1` taxele vamale și accizele în costul de achiziție (ajustare de cost pe lot)
- `86-r2` anularea/stornarea unei FCT legate la o DVI operată nu se refuză
- `86-r11` DVI ca document stins: restul polimorf în ImperechereService.Total + ramura în DocumenteCuRest
- `F26-r18` anularea unui eveniment PIF din luna unei AMO operate e refuzată deși rândul lunar nu depinde de el…
- `F27-r16` forma proiecției DocumenteCuRest: candidații de la 59 (uniunea tuturor documentelor operate) și form…
- `F28-r1` simplificarea CandidatiPereche<T, TOpus> și a uniunii per tip din ImperecheriProiectii (jumătatea „t…
- `TR-r7` sink-urile bugetare Gratuit/Custodie și injectivitatea SAF-T rămân deschise…
- `TR-r8` rulajul brut per partidă nu e sumă sub tranzacția de transfer…
- `TR-r10` deschiderea generică fără document, cu loturi și partide…
- `TR-r11` Numar, DataScadenta, Autogenerat, DocumentSursa rămân atribute ale documentului scrise la operare, s…
- `N-r5` stornoul unei postări ATRIBUITE (reevaluare) intră în selecție (090i) și poartă cauza altui document…
- `N-r6` amendamente de literă ale contractului TR-D6a consemnate în cod: cazurile Decizie poartă Linie…
- `N-r7` ordinea FIFO a nucleului e Guid.CompareTo (componentă cu componentă) prin Fifo.Intai…
- `B-r6` Valuta/Curs sunt câmpuri de frunză pe FacturaIntrare fără interfață declarată: operandul le lasă nul…
- `B-r8` gestiunile virtuale (GestiuniVirtuale.Furnizor/Client/Consum) sunt constante deterministe fără rând…
- `S-r3` gestiunile virtuale n-au FK pe Postare (id-uri fără rând de nomenclator)…
- `S-r7` un lot născut de o linie „în roșu” (valoare negativă, cantitate pozitivă) se evaluează negativ tăcut…
- `S-r8` dry-run-ul nu fixează prețul și data lotului născut de document, deci contractul lui diferă de cel r…
- `T-r2` reziduul valoric lăsat de RLF pe lotul golit (valoare fiscală ≠ raportul lotului) contrazice 090 (j)…
- `T-r5` DVI deschide partidă pe contrapartida cu RolTert (401 probat…
- `107-r4` valuta: partidele inițiale în valută cu curs propriu (azi în lei, plafonate la rest la stingere) (10…
- `095-r1` DEC → contract IMO → PIF/AMO/CAS complete pe cub, inclusiv cartea fiscală și istoricul fișei, înaint…
- `097-r2` auditul anulării fizice a suportului eliberat; proveniența fără FK restrictiv (097, 091j)
- `099-r1` LDI adoptă cauza diferenței: minus imputabil pe partener, perisabilitate, neimputabilă; plus (099)
- `X-r3` Raportul de stoc listează capătul de consum al BCS (postarea de debit poartă lotul ca unitate, pe co…
- `NG-r1` Balanța și fișa contului filtrate pe gestiune sau pe material nu văd transferurile (BTR, ASM): valoa…
- `NG-r2` Contul lotului vine din tipul curent de material al produsului…

**politici-si-fiscalitate** (4)

- `F27-r4` închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88)
- `T-r6` Custodie pe bugetar: explicit neacoperită la LDI…
- `103-r2` Intervale TVA și avertismente/raport de impact, cu referință la linia avansului (103h)…
- `103-r3` Validarea L3 a semnului taxei culese față de baza FCT/FCL și înlocuirea proxy-ului ILinieCuAvans pen…

**dezvoltare-si-validare** (5)

- `S-r4` snapshot-ul EF declară cheia ID și FK_Postare_Tranzactie_TranzactieId pe părinte, baza are cheia (Sp…
- `091-r2` recensământul pe clona Flax per tip rămas (NTC, ITV, RDC, RLF, ASM, LDI, NIR), o dată, înaintea scen…
- `107-r6` performanța Import1C: 25 → 50 min/lună pe Flax față de 8–10 la baseline 2026-09-21 (~5×)…
- `106-r5` StateMachine și ViewVariants scoase din Startup.cs și RequiredModuleTypes…
- `118-r2` Unealta docskit (repo propriu, dotnet global tool): gen, check întâi, build, metrics după…

## Felii deschise

Niciun contract deschis declarat cu `Tip: contract`.

## Decizii invocate din cod

Decizia (numărul de citări `// N` din cod): 1 (3), 3 (2), 4 (1), 5 (1), 9 (1), 10 (2), 13 (1), 14 (2), 15 (2), 17 (1), 22 (1), 23 (2), 24 (1), 25 (1), 26 (4), 28 (1), 31 (1), 33 (1), 34 (1), 35 (1), 36 (1), 38 (3), 41 (2), 42 (3), 43 (1), 46 (2), 48 (2), 49 (1), 51 (1), 53 (1), 55 (1), 57 (6), 58 (1), 63 (1), 64 (1), 72 (1), 73 (1), 74 (1), 77 (4), 79 (4), 82 (3), 83 (4), 85 (4), 86 (1), 87 (2), 89 (4), 90 (4), 91 (3), 92 (1), 97 (1), 98 (2), 99 (2), 101 (1), 102 (1), 103 (2), 104 (7), 106 (1), 107 (7), 109 (1), 112 (1), 115 (5), 117 (1).

## Paginile stării curente

| Pagina | Actualizat |
|---|---|
| [README](stare-curenta/README.md) | 2026-10-10 |
| [api-si-client](stare-curenta/api-si-client.md) | 2026-10-11 |
| [dezvoltare-si-validare](stare-curenta/dezvoltare-si-validare.md) | 2026-10-11 |
| [domeniu-si-operare](stare-curenta/domeniu-si-operare.md) | 2026-10-11 |
| [limite-curente](stare-curenta/limite-curente.md) | 2026-10-11 |
| [politici-si-fiscalitate](stare-curenta/politici-si-fiscalitate.md) | 2026-10-11 |
| [termeni](stare-curenta/termeni.md) | 2026-10-10 |

## Ce nu poate ști

Starea mediilor și a bazelor, ramurile nemerse în main, ce e în lucru necomis, și dacă o regulă scrisă chiar e implementată: astea le spun probele, nu docs.
