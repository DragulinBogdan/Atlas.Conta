# 117 — Postarea își poartă ordinalul în tranzacție; unitatea fără document se departajează pe el

- Data: 2026-10-09
- Stare: **activă**; hotărâtă de owner 2026-10-09. Amendează 116 (a), (b) și (c); închide 116-r1.
- Docs: `docs/stare-curenta/domeniu-si-operare.md`, `limite-curente.md`; catalogul `docs/nucleu/scenarii/CITIRI.md` (SC-CIT-113).

## Regula durabilă

**Ordinea postărilor unei tranzacții e dată persistată, nu o proprietate a ID-urilor.**

(a) **Coloana.** `Postare.Ordinal` e poziția postării în tranzacția care a scris-o, de la 1, fără goluri și fără dubluri. O dă scrierea (`Randuri.Scrie`, chemată de `Materializare.Scrie`), din ordinea postărilor tranzacției nucleului. Nucleul nu o cunoaște: ordinea e cea a listei lui.

(b) **Nu e coordonată.** Ordinalul nu intră în sold, în nicio grupare, în identitatea unității sau a perechii. Nu depinde de 111 și nu o anticipează.

(c) **Ordinea FIFO** a unităților este: data deschiderii, ID-ul documentului deschizător, ordinalul postării de deschidere, apoi identificatorul unității. Ordinalul contează numai între unitățile fără document deschizător. Toate sunt postări ale aceleiași tranzacții, fiindcă deschiderea e unică pe bază (094), deci ordinalul singur le ordonează. Nucleul îl primește pe candidat (`Disponibil.Deschidere`), iar motorul îl citește prin `Cub.Citiri.Partide.Deschideri`, numai când la FIFO vin cel puțin două partide fără document.

(d) **ID-urile postărilor nu mai poartă ordine.** Sortarea lor la scriere (116 (a)) iese; ID-ul rămâne Guid v7 generat la creare.

(e) **Invariantul.** `INV-CUB` refuză cu `CITIRE_ORDINAL_INVALID` tranzacția ale cărei postări nu sunt numărate de la 1 fără goluri (`Invarianti.VerificaOrdinale`).

(f) **Baza nu dă valoare implicită.** O scriere care nu numește ordinalul e refuzată de bază. Migrația numără rândurile existente în ordinea ID-ului: la rândurile scrise după 116 e ordinea tranzacției, la cele mai vechi e una întâmplătoare, dar stabilă.

(g) **Lista de evidență.** `PostareVizual` arată ordinalul, ca orice coloană a postării.

(h) **Contractul celui care deschide** rămâne cel din 116 (d). Loturile inițiale rămân neacoperite (116 (e), 116-r2).

## Context

116 a făcut două importuri identice ducând ordinea deschiderii în ID-urile postărilor: la scriere, ID-urile se sortau și se atribuiau în ordinea tranzacției. Owner-ul a numit atunci forma pe termen lung: ordinea persistată pe postare (116-r1), amânată „odată cu 111”.

Amânarea venea dintr-o citire prea largă a regulii „nu atinge coordonatele postării; 111 e propusă”. 111 vorbește despre coordonatele de sold (un singur repartitor, partenerul fiscal, registrele tipate); ordinalul nu e una dintre ele. `Pereche`, tot un ordinal pe postare, a intrat prin 110 cu 111 deja propusă. Owner-ul a ridicat amânarea la 2026-10-09: „câștigul e clar cu ordine explicită prin ordinal”.

Cu ordinea purtată de ID nu exista nicio probă ulterioară că ID-urile sunt în ordinea tranzacției. Guid v7 din .NET ordonează numai între milisecunde: din 20.000 generate la rând, 19.950 de perechi vecine au căzut în aceeași milisecundă și 9.936 au ieșit în ordine inversă.

## Tranșări

1. **Ordinalul pe `Postare`** (aleasă, owner). Verificabil în `INV-CUB`, independent de generatorul de ID-uri și de drumul de scriere, inclusiv de scrierea în lot din felia de migrare (091-r4).
2. **Generator Guid v7 monoton în Atlas.DXF** (contor în cei 12 biți de după timp). Nealeasă ca mecanism al ordinii: ar ține ordinea creării într-un singur proces, dar ordinea ar rămâne neverificabilă după scriere. Rămâne o îmbunătățire posibilă a generatorului, fără ca motorul să se sprijine pe ea.
3. **Snowflake.** Nealeasă: chei `bigint` în toată schema, în API, în client și în identitatea unităților, plus un identificator de nod coordonat între hosturi și unelte, pentru ce dă deja un v7 cu contor.
4. **Index unic pe (tranzacție, ordinal).** Neadăugat: pe tabela partiționată ar cere și cheia partiției, iar postările unei tranzacții stau în mai multe spații. Unicitatea o verifică `INV-CUB`.

## Probe

- Nucleul: 192 de teste; `FaraDocumentOrdinalulDeschideriiBateId`.
- Catalog: SC-CIT-113 pe ambele profiluri. Cu citirea ordinalului oprită, scena pică.
- `INV-CUB`: trei mutanți noi, câte unul pe fiecare ramură a invariantului (`ORDINAL-BAZA`, `ORDINAL-GOL`, `ORDINAL-DUBLU`), prinși pe ambele profiluri.
- ModelCheck integral: 3.578 bugetar / 4.859 privat OK, zero FAIL (`run-verificari/20261009-225831-260/`). Prima rulare a picat pe `STR-VIZUAL-1`, care cere pe `PostareVizual` fiecare coloană a postării; de aici (g).
- Migrația, pe o clonă a importului de ianuarie scris cu 116 (`.Flax.ProfO1m`): 18.770 de tranzacții și 81.774 de postări numărate, niciun ordinal invalid; aplicată și retrasă pe trei baze.
- Două importuri pe ianuarie 2025 cu același binar, pe baze noi (`.Flax.ProfR1`, `.ProfR2`), și un import de control cu binarul de dinainte (`.ProfO3`), în aceeași seară:

| | cu 116 (control) | cu 117 |
|---|---|---|
| Linii diferite în raportul de reconciliere între două rulări | 0 | 0 |
| Linii diferite față de raportul cu 116 | — | 0 |
| Legături diferite | 0 | 0 |
| Tranzacții / postări / împerecheri | 18.770 / 81.774 / 2.152 | aceleași |
| Partidele pe chei naturale (amprentă) | aceeași | aceeași |
| Verificări picate | 10 | 10 în ambele |
| Ordinale invalide | — | 0 |
| Durata importului | 498 s | 506 s și 506 s |

Duratele sunt câte o rulare pe baze diferite, deci numai orientative. În după-amiaza aceleiași zile binarul cu 116 dăduse 441 s și 430 s. Diferența până la 498 s nu e a codului: o are și binarul de control. Nu e nici a conexiunii: toate rulările au mers prin `localhost`, iar un al treilea import cu 117 și `GSS Encryption Mode=Disable`, ca după-amiaza, a dat 507 s. Timpul din server pe aceleași 2,36 milioane de apeluri a crescut de la 119 s la 144 s la control, iar procesorul clientului de la 222 s la 253 s. Între cele două momente instanța nativă a devenit serviciu Windows; cauza n-a fost cercetată mai departe.

## Ce rămâne deschis

- **116-r2** — loturile inițiale cu același produs, gestiune, cont și dată se departajează pe identificatorul lotului. Pe ianuarie 2025 sunt 45 de astfel de perechi (90 din 6.817 loturi inițiale pe gestiune), 24 cu prețuri diferite. Se poate închide citind ordinalul postării de deschidere în ordinea FIFO a loturilor; citirea e pe drumul fiecărei ieșiri de stoc, deci cere măsurătoare.
