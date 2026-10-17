# 131 — Cubul adună; faptul datat e supapă cu nume, construită la primul consumator, nu registru de acumulare și nu postare de zero

- Data: 2026-10-17
- Stare: activă
- Execuție: sesiunea de discuții de după „rotund” (130), branch `consultare-131-fapt-datat`; owner-ul a ales varianta la 2026-10-17 și a cerut decizia în aceeași zi
- Arii: motor, imobilizari, dezvoltare
- Amendează: —
- Docs: `docs/consultari/2026-10-17-efecte-si-registre/` (sinteza Codex, analiza Claude cu proba specificată în §4); `docs/consultations/2026-10-05-cub-vs-registre/` (runda anterioară); `docs/stare-curenta/motor.md` §Limitele implementării

## Regula durabilă

**Cubul primește numai ce se adună. Un fapt a cărui citire e „valoarea valabilă la data d” nu intră în cub sub nicio formă; destinația lui e faptul datat, o supapă cu nume care se construiește la primul consumator din catalog. Registre de acumulare separate nu se adaugă.**

(a) **Ce intră în cub.** Valoare, cantitate și valoare în valută, pe coordonate, cu cauză: tot ce e sold, rulaj, cost, rest sau valoare rămasă rămâne sumă pe cub (090 a, invariantul III). Registrul contabil al unui sistem clasic e proiecția cubului pe `Carte=Contabil`; agregarea lui sunt snapshot-urile de referință și lema soldului (N-D9), iar un al doilea read model nu se adaugă fără cifră nouă (FZ-r1).

(b) **Ce nu intră în cub.** Un fapt neaditiv (metodă, durată, loc, preț de listă, curs, statut fiscal, parametru de politică) nu se scrie: (1) ca postare cu măsuri zero folosită drept ancoră de eveniment; (2) ca coordonată nouă pe `Coordonate`; (3) ca stare pe nomenclator scrisă de motor la operare, fără istoric. Cele trei forme există azi (revizuirea PIF, locul fișei, starea fișei) și sunt tolerate ca moștenire; o felie care le atinge le mută pe (c), o felie nouă nu le mai adaugă.

(c) **Faptul datat, supapa cu nume.** Contractul, fixat acum și construit la primul consumator: cheie tipată → resurse tipate, `ValabilDeLa`, `Cauza` (document și linie) sau fără document (nomenclator, politică), `RetrasDe` (stornoul cauzei retrage faptul, nu îl neagă), `ScrisLa`. Citirea e „valabil la d” = ultimul fapt neretras cu `ValabilDeLa ≤ d`, departajat pe `ScrisLa`, și „istoricul cheii”. Fiecare fel de fapt e clasă și tabelă proprie peste o bază comună, cu cititor generic (invariantul IV: structura e cod); faptele sunt a doua ieșire a declarantului, scrise de `Materializare` în aceeași comandă; nucleul pur nu le cunoaște. Gardianul perioadei judecă `ValabilDeLa` ca pe data postării; dependenții se refuză ca azi.

(d) **Fără registre de acumulare separate.** Un registru de acumulare pe domeniu (stoc, TVA, vânzări, rezervări) e exclus cât timp catalogul nu are o cerință al cărei sold nu e sumă pe cub. Argumentul e măsurat: pe Flax două registre pentru aceeași valoare au divergat cu 585.404,66 (090 g, TR-r12); redeschiderea cere decizie proprie, nu amendament aici.

(e) **Primul consumator deschide supapa.** Candidații din catalog, în ordinea probabilă: cursul valutar (F-VAL-01), istoricul statutului de TVA al partenerului (D4-r1), reevaluarea și deprecierea (F-IMO-04), salariile (F-SAL-01), istoricul politicilor (D9-r3). Felia care îl deschide construiește primitiva, amendează invarianții I și 090 (a) pentru documentul cu fapte și fără tranzacție, și migrează parametrii și locul fișei pe ea (131-r2). Proba comparativă din `analiza-claude.md` §4 e spec-ul acelei felii; nu se rulează înainte (131-r1).

(f) **Canarii de review.** O felie care ar adăuga o postare de zero, o coordonată neaditivă sau o scriere a motorului pe nomenclator se oprește și deschide (e), în loc să cârpească. Verificarea e la review, pe diff, fără probă.

## Context

Owner-ul a încheiat exportul 1C și a observat structura lui: registru contabil, registre de acumulare și registre de informații. Instinctul: registrul contabil e subset al cubului și adaugă agregarea și istoricul; teama: cubul ca „one size fits all” devine punct îngust cu cost crescător. Codex a inventariat (sinteza din dosar), Claude a analizat pe cod: agregarea există (snapshot-uri), istoricul lipsește și e făcut de mână în trei feluri; cele 13 registre de acumulare din Flax n-au consumator în catalog, cele ~20 de registre de informații de domeniu au șase.

## Tranșări

- Varianta V1 (primitiva acum, cu probă pe imobilizări) a fost analizată și amânată de owner, la 2026-10-17: fără câștig concret estimat azi, singurul loc măsurabil fiind cititorul imobilizărilor, care funcționează și e acoperit de SC-IMO. Proba rămâne scrisă ca spec.
- Varianta V2 (cele trei familii ale lui 1C) e respinsă pe catalog, nu pe principiu: (d).
- Varianta V0 (status quo formalizat) e respinsă fiindcă postarea de zero nu se generalizează: cursul, statutul fiscal și salariile n-au unitate pe care s-o ancoreze.
- Regula se fixează acum, înainte de primul consumator, fiindcă e ieftină cât nu e cod și fiindcă teama owner-ului se rezolvă printr-un gard, nu printr-o construcție.

## Review

Fără review Codex: decizia tranșează pe analiză, fără cod și fără schimbare de schemă; owner-ul a ales varianta direct.

## Ce rămâne deschis

- 131-r1: proba comparativă pe parametrii de amortizare (`analiza-claude.md` §4) se rulează doar odată cu felia primului consumator, ca spec al ei.
- 131-r2: migrarea celor trei forme tolerate (postările zero ale revizuirii, locul și starea fișei) pe faptul datat, în aceeași felie sau imediat după; amendarea invarianților I și 090 (a).

## Istoric

- 2026-10-17: scrisă la cererea owner-ului după analiza din dosar; owner-ul a ales „gard acum, primitiva la primul consumator”; activă.
