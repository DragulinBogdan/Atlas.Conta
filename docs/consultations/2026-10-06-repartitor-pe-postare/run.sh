#!/usr/bin/env bash
# Folosire: bash run.sh <pas>...   pași: clona r1-astra r1-fable r2-astra r2-fable
export PATH="/usr/bin:/c/Users/bobo/.local/bin:/c/Users/bobo/AppData/Local/OpenAI/Codex/bin/12219cbfbcbddde7:$PATH"
D=/d/Dev/Atlas.Conta/docs/consultations/2026-10-06-repartitor-pe-postare
K=/d/Temp/conta-analiza
G=/d/Temp/consultare-goala
A=/d/Temp/conta-analiza-arhiva
R=$D/raspunsuri
L=$R/run.log
COMMIT=717c3b5
mkdir -p $R $G $A
pas() { echo "$(date +%H:%M:%S) $*" >> $L; }
gata() { [ -s "$R/$1.md" ] && [ "$(wc -c < "$R/$1.md")" -gt 2000 ]; }
SCURT="Citeste fisierul docs/consultare/sarcina.md si executa sarcinile de acolo. Raspunsul tau final este raportul cerut, in romana."

for P in "$@"; do case $P in
clona)
  cd $K || exit 1
  mkdir -p $A/$(date +%Y%m%d-%H%M)
  mv docs/consultare docs/dosar.md $A/$(date +%Y%m%d-%H%M)/ 2>/dev/null
  rm -rf docs/instructiuni-proiect
  git checkout -q -- . && git fetch -q /d/Dev/Atlas.Conta tr-d9-taierea && git checkout -q --detach $COMMIT || { pas "clona ESEC"; exit 1; }
  mkdir -p docs/instructiuni-proiect docs/consultare
  mv CLAUDE.md AGENTS.md docs/instructiuni-proiect/ 2>/dev/null
  cp $D/fapte.md docs/consultare/fapte.md
  cp $D/2-runda-informata.md docs/consultare/sarcina.md
  pas "clona la $(git log --oneline -1 | cut -c1-60)";;
r1-astra)
  gata r1-astra && { pas "r1-astra exista, sar"; continue; }
  cd $G; pas "r1-astra start"
  codex exec --sandbox read-only --color never --skip-git-repo-check \
    -c model_reasoning_effort=high -o $R/r1-astra.md \
    "$(cat $D/1-runda-oarba.md)" < /dev/null > $R/r1-astra.out 2>&1
  pas "r1-astra exit=$? bytes=$(wc -c < $R/r1-astra.md 2>/dev/null)";;
r1-fable)
  gata r1-fable && { pas "r1-fable exista, sar"; continue; }
  cd $G; pas "r1-fable start"
  claude -p --model claude-fable-5-1 --effort high --tools "" \
    < $D/1-runda-oarba.md > $R/r1-fable.md 2> $R/r1-fable.err
  pas "r1-fable exit=$? bytes=$(wc -c < $R/r1-fable.md 2>/dev/null)";;
r2-astra)
  gata r2-astra && { pas "r2-astra exista, sar"; continue; }
  gata r1-astra || { pas "r2-astra: lipseste r1"; exit 1; }
  cd $K; cp $R/r1-astra.md docs/consultare/runda1-proprie.md; pas "r2-astra start"
  codex exec --sandbox read-only --color never -C $K \
    -c model_reasoning_effort=high -o $R/r2-astra.md \
    "$SCURT" < /dev/null > $R/r2-astra.out 2>&1
  pas "r2-astra exit=$? bytes=$(wc -c < $R/r2-astra.md 2>/dev/null)";;
r2-fable)
  gata r2-fable && { pas "r2-fable exista, sar"; continue; }
  gata r1-fable || { pas "r2-fable: lipseste r1"; exit 1; }
  cd $K; cp $R/r1-fable.md docs/consultare/runda1-proprie.md; pas "r2-fable start"
  echo "$SCURT" | claude -p --model claude-fable-5-1 --effort high --tools "Read,Grep,Glob" \
    > $R/r2-fable.md 2> $R/r2-fable.err
  pas "r2-fable exit=$? bytes=$(wc -c < $R/r2-fable.md 2>/dev/null)";;
esac; done
pas "GATA $*"
