# Unde suntem

Generat de `docskit gen` din antete, registru și cod; nu se editează. Cronologia e în jurnal; comparația paginilor cu ultimul commit o face `docskit check` (gardianul `actualizat`).

## Rezumat

| Ce | Cât |
|---|---|
| Decizii | 120: 87 active, 33 amendate parțial, 0 depășite, 0 propuse |
| Restanțe | 383: 284 deschise, 99 închise |
| Pagini de stare curentă | 7 |
| Contracte de felie (`Tip: contract`) | 3, deschise 2 |
| Cataloage vii (`Tip: catalog`) | 5 |
| Fișiere Markdown în `docs/` | 321 |
| Citări de decizii din cod (`// N`) | 71 |

## Decizii în vigoare pe arie

- **domeniu-si-operare** (52): 10, 13, 14, 15, 16 (amendată de 89), 17, 20, 22, 23, 25, 26 (amendată de 112), 27, 30, 31 (amendată de 76, 88), 32, 33, 36 (amendată de 71), 37, 41, 42 (amendată de 104, 106), 46 (amendată de 113), 48 (amendată de 76), 49, 54, 55, 62, 64 (amendată de 84), 65, 75, 76, 82, 86, 87 (amendată de 97), 89, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 93, 95, 97 (amendată de 98), 98, 100 (amendată de 102), 101 (amendată de 102), 102, 104 (amendată de 106), 106, 109, 111, 112, 114 (amendată de 116), 115, 116 (amendată de 117), 117, 119
- **politici-si-fiscalitate** (22): 20, 36 (amendată de 71), 42 (amendată de 104, 106), 46 (amendată de 113), 56 (amendată de 77, 81), 66, 69, 70, 71 (amendată de 72), 72, 73 (amendată de 74, 105), 74 (amendată de 105), 79 (amendată de 80), 80, 81, 83 (amendată de 84, 86, 104), 84, 86, 87 (amendată de 97), 103, 111, 113
- **api-si-client** (26): 39, 42 (amendată de 104, 106), 43 (amendată de 104), 53, 55, 56 (amendată de 77, 81), 57, 58, 60, 66, 68, 70, 77, 79 (amendată de 80), 80, 81, 84, 85, 86, 87 (amendată de 97), 89, 103, 104 (amendată de 106), 106, 108 (amendată de 110), 111
- **dezvoltare-si-validare** (34): 14, 15, 21, 23, 26 (amendată de 112), 30, 34, 36 (amendată de 71), 39, 42 (amendată de 104, 106), 43 (amendată de 104), 45 (amendată de 75), 47 (amendată de 107), 49, 50, 56 (amendată de 77, 81), 66, 70, 80, 81, 84, 85, 87 (amendată de 97), 89, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 91 (amendată de 107, 118, 120), 102, 104 (amendată de 106), 106, 107, 108 (amendată de 110), 115, 118, 120
- **README** (6): 42 (amendată de 104, 106), 76, 91 (amendată de 107, 118, 120), 103, 104 (amendată de 106), 110 (amendată de 113)
- **limite-curente** (29): 37, 41, 42 (amendată de 104, 106), 43 (amendată de 104), 46 (amendată de 113), 53, 55, 72, 75, 77, 80, 81, 84, 85, 87 (amendată de 97), 89, 91 (amendată de 107, 118, 120), 94, 95, 97 (amendată de 98), 102, 103, 104 (amendată de 106), 106, 107, 108 (amendată de 110), 109, 115, 116 (amendată de 117)
- **(fără arie)** (32): 1, 2, 3 (amendată de 89), 4, 5, 6, 7, 8, 9, 11, 12, 18, 19, 24, 28, 29, 35, 38 (amendată de 81, 84), 40, 44, 51, 52, 59, 61, 63, 67, 78, 88 (amendată de 103), 92 (amendată de 102), 96 (amendată de 100, 102), 99, 105

Titlurile stau în [indexul deciziilor](decizii/README.md); o decizie se deschide din fișierul ei.

## Decizii depășite

Niciuna.

## Restanțe deschise, pe stare

| Stare | Câte |
|---|---|
| activă | 44 |
| după PoC | 206 |
| migrare | 24 |
| depășită de 091 | 2 |
| docs | 4 |
| (în afara vocabularului) | 4 |

### Lista `activă` (44, prag 45)

**domeniu-si-operare** (33)

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
- `CV-r2` Închiderea exercițiului ca operație explicită (obligatorie pe bugetar, utilă pe privat)…

**politici-si-fiscalitate** (5)

- `F27-r4` închiderea de an ca operație distinctă (121 → 1174/117, soldurile de deschidere ale anului nou) (88)
- `T-r6` Custodie pe bugetar: explicit neacoperită la LDI…
- `103-r2` Intervale TVA și avertismente/raport de impact, cu referință la linia avansului (103h)…
- `103-r3` Validarea L3 a semnului taxei culese față de baza FCT/FCL și înlocuirea proxy-ului ILinieCuAvans pen…
- `CV-r1` Reprezentarea SAF-T a corecției valorice pe lot: factura corectivă (tipul 380/381/384, cantitatea, d…

**dezvoltare-si-validare** (6)

- `S-r4` snapshot-ul EF declară cheia ID și FK_Postare_Tranzactie_TranzactieId pe părinte, baza are cheia (Sp…
- `091-r2` recensământul pe clona Flax per tip rămas (NTC, ITV, RDC, RLF, ASM, LDI, NIR), o dată, înaintea scen…
- `107-r6` performanța Import1C: 25 → 50 min/lună pe Flax față de 8–10 la baseline 2026-09-21 (~5×)…
- `091-r6` lanțurile transversale fără realizare care să probeze definiția din X.md: SC-X-01 (refuz pe NIR cone…
- `106-r5` StateMachine și ViewVariants scoase din Startup.cs și RequiredModuleTypes…
- `118-r2` Unealta docskit (repo propriu, dotnet global tool): gen, check întâi, build, metrics după…

## Felii deschise

- [TR-D9b — unitățile de sold: contract și primul pachet de execuție](nucleu/tr-d9b-contract.md) — pași livrați 0 din 0; decizii: 090, 092, 102, 110, 111, 114, 115, 116, 117, 118
- [TR-D9b-CV — corecții valorice pe lot, fără schimbarea cantității](nucleu/tr-d9b-corectii-valorice-contract.md) — pași livrați 0 din 0; decizii: 088 (h, amendată de 103), 090, 091, 103, 104, 110, 111, 118, 119

## Decizii invocate din cod

Decizia (numărul de citări `// N` din cod): 13 (1), 15 (1), 22 (1), 26 (1), 33 (1), 42 (2), 43 (1), 46 (1), 48 (2), 49 (1), 53 (1), 55 (1), 57 (4), 77 (1), 79 (3), 82 (2), 83 (1), 85 (1), 86 (1), 87 (2), 89 (3), 90 (4), 91 (2), 92 (1), 97 (1), 98 (2), 99 (2), 101 (1), 102 (1), 103 (2), 104 (7), 106 (1), 107 (7), 109 (1), 112 (1), 115 (5), 119 (1).

## Paginile stării curente

| Pagina | Actualizat |
|---|---|
| [README](stare-curenta/README.md) | 2026-10-14 |
| [api-si-client](stare-curenta/api-si-client.md) | 2026-10-13 |
| [dezvoltare-si-validare](stare-curenta/dezvoltare-si-validare.md) | 2026-10-14 |
| [domeniu-si-operare](stare-curenta/domeniu-si-operare.md) | 2026-10-14 |
| [limite-curente](stare-curenta/limite-curente.md) | 2026-10-14 |
| [politici-si-fiscalitate](stare-curenta/politici-si-fiscalitate.md) | 2026-10-13 |
| [termeni](stare-curenta/termeni.md) | 2026-10-13 |

## Cataloagele vii

| Catalog | Actualizat |
|---|---|
| [Catalogul funcționalităților](functionalitati.md) | 2026-10-14 |
| [C111 — coordonatele postării (transversal)](nucleu/scenarii/C111.md) | 2026-10-11 |
| [CVL — corecția valorii lotului fără schimbarea cantității](nucleu/scenarii/CVL.md) | 2026-10-14 |
| [D9B — originea partidei](nucleu/scenarii/D9B.md) | 2026-10-14 |
| [X — scenarii transversale](nucleu/scenarii/X.md) | 2026-10-14 |

## Ce nu poate ști

Starea mediilor și a bazelor, ramurile nemerse în main, ce e în lucru necomis, și dacă o regulă scrisă chiar e implementată: astea le spun probele, nu docs.
