# 125 — Citirea de sold pe o listă de loturi se face pe lot, nu pe fereastra de dată

- Data: 2026-10-16
- Stare: propusă
- Execuție: firul import-1c-perf, pasul 1 (2026-10-16), branch `import-1c-perf`; măsurat pe Import1C ianuarie–martie 2025, pe 5446, bază nouă la fiecare rulare (`run-verificari/profilare-import/b0`, `b1`, `b2`)
- Arii: stoc, import
- Amendează: —
- Închide: 107-r6
- Docs: SC-CIT-115 din `docs/nucleu/scenarii/CITIRI.md`; `docs/stare-curenta/stoc.md` §Citirea stocului; `docs/stare-curenta/import.md` §Opțiunile rulării

## Regula durabilă

**Soldul cerut pe o listă de loturi se citește pe lot. Filtrul de lot intră înaintea grupării, iar fereastra de dată e filtru, nu cale de acces.**

(a) **Loturile cerute coboară în ambele ramuri ale cumulului.** `Loturi.Cumulate(…, loturi)` restrânge la ele și snapshot-ul, și postările ferestrei, înaintea grupării. Pe ramura postărilor, data ferestrei e `"Data" + 0`, ca indexul pe dată să nu poată fi ales. Citirea fără loturi rămâne neschimbată. Apelanții care știu loturile le dau aici, nu filtrează rezultatul: operandul motorului (`Fapte.SolduriLoturi`) și gardul de sold intermediar (`Loturi.VerificaSoldIntermediar`).

(b) **Rezultatul e al citirii întregi.** Citirea pe loturi dă exact rândurile cumulului întreg restrâns la acele loturi, cu snapshot, cu excluderea unui document și cu granița lui (SC-CIT-115).

## Context

107-r6 măsura importul la 25 → 50 min pe lună. Pe main d5a5d6fa, cu un import nou pe 5446 (`--recreeaza --continua`), lunile au ieșit 6:10, 9:18 și 14:02, adică 24, 35 și 46 ms pe document. Creșterea e liniară cu luna, deci anul crește pătratic: extrapolat, decembrie ar avea ~39 min, iar anul ~4,5 h.

Cauza: cele două citiri de sold pe lot luau ~80% din timpul bazei. Importul pornește auto-prepare (`Max Auto Prepare=300`), iar după cinci execuții Postgres trece instrucțiunea pe planul generic. Pentru un tablou de loturi necunoscut, planul generic presupune 10 elemente și estimează o fereastră îngustă pe dată, așa că alege `IX_Postare_Stoc_Data` (cost 42 față de 58 pe `(Unitate, Data)`). Fără lună închisă (lunile importului pică pe contract), fereastra pornește la 1 ianuarie și e parcursă întreagă la fiecare apel: 9,7 ms în martie, față de 0,5 ms cu planul specific valorilor.

## Tranșări

Măsurat pe ianuarie–martie, pe aceeași comandă, cu bază nouă la fiecare rulare:

| Rulare | 01 | 02 | 03 | ms/document | Total proces |
|---|---|---|---|---|---|
| b0, main | 6:10 | 9:18 | 14:02 | 24 / 35 / 46 | ~29:30 (oprit în aprilie) |
| b1, `plan_cache_mode=force_custom_plan` pe conexiune | 5:58 | 6:30 | 7:35 | 23,5 / 24,6 / 24,7 | 20:58 |
| b2, regula (a) | 5:06 | 5:25 | 6:19 | 20,1 / 20,5 / 20,6 | 17:43 |

- `force_custom_plan` pe conexiunea importului, respins: oprește creșterea, dar re-planifică fiecare instrucțiune, iar planificarea urcă la 264 s din 1258. Capcana planului generic ar fi rămas în citire pentru orice host care pornește auto-prepare.
- Încercat pe SQL, cu plan generic forțat, și respins: filtrul de lot coborât fără schimbarea ferestrei nu schimbă planul; `unnest(loturi) JOIN Postare` duce la hash join pe toată fereastra; un index parțial pe lot nu schimbă estimarea.
- Cu (a), Postgres păstrează pe citire planul specific valorilor (planul generic estimat pe 10 loturi iese mai scump decât cel real pe 1–2). Citirea costă ~0,2 ms plus planificarea; restul instrucțiunilor rămân pe planul generic.
- Echivalența: b1 și b2 au aceleași tranzacții, postări și împerecheri (60.884 / 242.742 / 9.446), raportul de reconciliere identic, aceleași legături și aceleași amprente pe chei naturale, pe partide și pe stoc (cont × produs × gestiune × zi).

## Review

Fără review advers: schimbarea e locală citirii, iar echivalența e probată pe import și în SC-CIT-115. ModelCheck integral verde pe ambele profiluri.

## Ce rămâne deschis

- 125-r1: costul pe document rămas (~20 ms), din care CPU-ul procesului .NET e ~52% din timpul rulării.
- 125-r2: lunile de după martie nu sunt remăsurate; altă creștere pe lună (partide, TVA, contractul lunar) s-ar vedea abia pe o lună târzie.

## Istoric

- 2026-10-16: scrisă la pasul 1 al firului import-1c-perf; propusă owner-ului.
