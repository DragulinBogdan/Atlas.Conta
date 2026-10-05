#!/usr/bin/env bash
export PATH="/usr/bin:/c/Users/bobo/.local/bin:/c/Users/bobo/AppData/Local/OpenAI/Codex/bin/12219cbfbcbddde7:$PATH"
D=/d/Dev/Atlas.Conta/docs/consultations/2026-10-05-nod-si-galeata
K=/d/Temp/conta-analiza
G=/d/Temp/consultare-goala
R=$D/raspunsuri
L=$R/run.log
pas() { echo "$(date +%H:%M:%S) $*" >> $L; }
SCURT="Citeste fisierul docs/consultare/sarcina.md si executa sarcinile de acolo. Raspunsul tau final este raportul cerut, in romana."

pas "START"
mkdir -p $G /d/Temp/conta-analiza-arhiva $K/docs/consultare
mv $K/docs/consultare/raport-*.md /d/Temp/conta-analiza-arhiva/ 2>/dev/null
cp $D/fapte.md $K/docs/consultare/fapte.md
cp $D/2-runda-informata.md $K/docs/consultare/sarcina.md

cd $G
pas "r1-astra start"
codex exec --sandbox read-only --color never --skip-git-repo-check \
  -c model_reasoning_effort=high -o $R/r1-astra.md \
  "$(cat $D/1-runda-oarba.md)" < /dev/null > $R/r1-astra.out 2>&1
pas "r1-astra exit=$? bytes=$(wc -c < $R/r1-astra.md 2>/dev/null)"

pas "r1-fable start"
claude -p --model claude-fable-5-1 --effort high --tools "" \
  < $D/1-runda-oarba.md > $R/r1-fable.md 2> $R/r1-fable.err
pas "r1-fable exit=$? bytes=$(wc -c < $R/r1-fable.md 2>/dev/null)"

cd $K
cp $R/r1-astra.md $K/docs/consultare/runda1-proprie.md
pas "r2-astra start"
codex exec --sandbox read-only --color never -C $K \
  -c model_reasoning_effort=high -o $R/r2-astra.md \
  "$SCURT" < /dev/null > $R/r2-astra.out 2>&1
pas "r2-astra exit=$? bytes=$(wc -c < $R/r2-astra.md 2>/dev/null)"

cp $R/r1-fable.md $K/docs/consultare/runda1-proprie.md
pas "r2-fable start"
echo "$SCURT" | claude -p --model claude-fable-5-1 --effort high --tools "Read,Grep,Glob" \
  > $R/r2-fable.md 2> $R/r2-fable.err
pas "r2-fable exit=$? bytes=$(wc -c < $R/r2-fable.md 2>/dev/null)"
pas "GATA"
