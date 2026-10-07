# Consultare: cubul, registrele sau amândouă (înaintea TR-D9a)

- Data: 2026-10-05. Stare: **toate cele trei runde rulate (2026-10-05), răspunsurile în
  `raspunsuri/r1-*.md`, `r2-*.md`, `r3-*.md`; decizia e la owner.**
- Întrebarea owner-ului: înainte de ștergerea registrelor, ce câștigăm și ce
  pierdem cu cubul; rămâne cubul singur, cubul ca autoritate cu registre
  derivate sau registrele.
- Analiști: Claude Fable 5.1 și GPT-6 Astra, independent.
- Rezultatul intră în decizia 110 sau ca amendament la contractul TR-D9a.
  Consultarea nu e sursă de adevăr; aprobă numai owner-ul.

## Protocolul

| Runda | Ce primește analistul | Unde rulează | Fișier |
|---|---|---|---|
| 1, oarbă | numai promptul: domeniul, formele, opțiunile, criteriile, cazurile | director gol, în afara repo-ului | `1-runda-oarba.md` |
| 2, informată | răspunsul lui din runda 1, dosarul, repo în citire | repo, numai citire | `2-runda-informata.md`, `dosar.md` |
| 3, critică | raportul de runda 2 al celuilalt | repo, numai citire | `3-critica-incrucisata.md` |

De ce două trepte: runda oarbă arată ce s-ar alege fără costul deja plătit;
runda informată aduce faptele pe care le are numai proiectul. Diferența
dintre ele, cu cauza numită de analist, e semnalul.

Reguli:

- runda 1 nu spune ce a ales echipa; runda 2 o spune explicit;
- sesiunea care a scris 090 și contractul TR-D9a nu analizează, ci
  orchestrează; dosarul ei e declarat părtinitor și analiștii îl auditează;
- răspunsurile ajung la owner integral, alăturate, înaintea oricărui rezumat;
- o singură rulare o dată; nicio rulare grea pe baze în timpul consultării.

## Opțiunile și criteriile

Opțiunile A (numai cubul), B (cubul autoritate, registrele proiecție
materializată și cale principală de citire), C (numai registrele), D (regimul
dual, termen de referință). Criteriile: coerența modelului, simplitatea
implementării, simplitatea reprezentării unui document, portița notelor
punctuale, loc pentru optimizări de viteză, explicabilitatea, reversibilitatea.

Cazurile de probă: factura de intrare, NIR conex, asamblarea, nota contabilă
manuală, stornoul în perioadă închisă, citirile grele, Custodia, ALOP.

## Rularea

Efortul de raționament: ridicat la ambele modele.

Rundele 2 și 3 NU rulează în directorul de lucru, ci pe o clonă a commit-ului
în afara lui. În directorul de lucru se încarcă singure, cu greutate de
instrucțiune: `CLAUDE.md`, `AGENTS.md`, memoria automată a proiectului (legată
de cale, nu de `CLAUDE.md`) și hook-urile. Pe clonă, cele două fișiere se
mută sub `docs/`, unde rămân citibile ca dovadă. Clona nu are `comunicari/`
și nici directoarele de rulări, fiindcă sunt neurmărite.

`claude --bare` ar ocoli tot, dar cere `ANTHROPIC_API_KEY`; cu autentificarea
de abonament nu pornește. Fără el, instrucțiunile globale din
`~/.claude/CLAUDE.md` rămân încărcate; sunt generale, nu despre cub.

Comenzile se verifică la prima rulare; forma lor:

```bash
D=/d/Dev/Atlas.Conta/docs/consultations/2026-10-05-cub-vs-registre
K=/d/Temp/conta-analiza
mkdir -p $D/raspunsuri /d/Temp/consultare-goala

# runda 1, din director gol
cd /d/Temp/consultare-goala
codex exec --sandbox read-only --color never --skip-git-repo-check \
  -c model_reasoning_effort=high -o $D/raspunsuri/r1-astra.md \
  "$(cat $D/1-runda-oarba.md)" < /dev/null > /dev/null 2>&1
claude -p --model claude-fable-5-1 --effort high --tools "" \
  < $D/1-runda-oarba.md > $D/raspunsuri/r1-fable.md

# clona pentru rundele 2 și 3
git clone --branch tr-d9-taierea /d/Dev/Atlas.Conta $K
cd $K && mkdir -p docs/instructiuni-proiect \
  && mv CLAUDE.md AGENTS.md docs/instructiuni-proiect/ \
  && cp $D/dosar.md docs/dosar.md

# runda 2, pe clonă, numai citire; promptul = runda 2 + răspunsul propriu din runda 1
codex exec --sandbox read-only --color never -C $K \
  -c model_reasoning_effort=high -o $D/raspunsuri/r2-astra.md \
  "$(cat $D/2-runda-informata.md $D/raspunsuri/r1-astra.md)" < /dev/null > /dev/null 2>&1
cd $K && cat $D/2-runda-informata.md $D/raspunsuri/r1-fable.md | \
  claude -p --model claude-fable-5-1 --effort high --tools "Read,Grep,Glob" \
  > $D/raspunsuri/r2-fable.md

# runda 3: la fel ca runda 2, cu 3-critica-incrucisata.md + raportul celuilalt
```

Rulările de runda 2 pot depăși zece minute: proces detașat, nu task de fundal.

## Răspunsurile

`raspunsuri/r1-fable.md`, `r1-astra.md`, `r2-*.md`, `r3-*.md`. Sinteza
alăturată se scrie numai după ce owner-ul le-a citit.
