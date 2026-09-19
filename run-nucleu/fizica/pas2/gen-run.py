# -*- coding: utf-8 -*-
"""Generatorul harness-ului de masurare (pas 2).

Pentru fiecare (forma, interogare) scrie un fisier cu EXPLAIN (ANALYZE, BUFFERS)
per instructiune, plus run.sh care le ruleaza de 6 ori, in ordinea ceruta de spec:
toate interogarile pe o forma, apoi forma urmatoare.
"""
import os, re, shutil

BASE = os.path.dirname(os.path.abspath(__file__))
F0 = os.path.join(BASE, "..", "f0")
CUB = os.path.join(BASE, "cub")
RUN = os.path.join(BASE, "run")

TABELA = {"f1": 'f1."Postare"', "f2": 'f2."Postare"', "f3": 'f3."Postare"',
          "f2s": 'f2."Postare"', "f3s": 'f3."Postare"'}

# qid -> fisierul sursa per forma. None = forma nu are interogarea.
QURI = ["q01", "q02", "q02b", "q03", "q03c", "q04", "q04b", "q05", "q06",
        "q07", "q08", "q09", "q09b", "q10", "q11", "q12", "q12b", "q13"]

CUB_S = {"q01": "q01s", "q02": "q02s", "q05": "q05s", "q06": "q06s", "q11": "q11s"}
DOAR_F0 = {"q02b", "q04b"}
DOAR_CUB = {"q03c", "q09b"}


def sursa(forma, qid):
    if forma == "f0":
        if qid in DOAR_CUB:
            return None
        p = os.path.join(F0, qid + ".sql")
        return p if os.path.exists(p) else None
    if qid in DOAR_F0:
        return None
    if forma in ("f2s", "f3s"):
        if qid not in CUB_S:
            return None
        return os.path.join(CUB, CUB_S[qid] + ".sql")
    p = os.path.join(CUB, qid + ".sql")
    return p if os.path.exists(p) else None


def instructiuni(text):
    """Taie fisierul in instructiuni SQL. Comentariile `--` se scot INTAI: contin ';'.
    Niciun literal din lotul asta nu contine '--' (uuid-urile au cratime simple)."""
    curat = re.sub(r"--[^\n]*", "", text)
    bucati = [b.strip() for b in curat.split(";")]
    return [b for b in bucati if re.search(r"(?im)^\s*select\b", b)]


def main():
    if os.path.isdir(RUN):
        shutil.rmtree(RUN)
    os.makedirs(RUN)
    lista = {}
    for forma in ["f0", "f1", "f2", "f3", "f2s", "f3s"]:
        d = os.path.join(RUN, "sql", forma)
        os.makedirs(d)
        ids = []
        for qid in QURI:
            src = sursa(forma, qid)
            if not src:
                continue
            text = open(src, encoding="utf-8").read()
            if forma != "f0":
                text = text.replace("{T}", TABELA[forma])
            stmts = instructiuni(text)
            if not stmts:
                continue
            with open(os.path.join(d, qid + ".sql"), "w", encoding="utf-8", newline="\n") as fh:
                for s in stmts:
                    fh.write("EXPLAIN (ANALYZE, BUFFERS)\n" + s + ";\n")
            ids.append(qid)
        lista[forma] = ids
        with open(os.path.join(RUN, "lista-" + forma + ".txt"), "w", newline="\n") as fh:
            fh.write("\n".join(ids) + "\n")

    sh = ["#!/bin/bash", "# Pas 2: seria de masurare. $1 = baza, $2 = scara (x1/x10).",
          "DB=\"$1\"; SC=\"$2\"", "OUT=/tmp/pas2out/$SC", "rm -rf $OUT; mkdir -p $OUT",
          "echo \"=== START $SC $(date -Is) ===\"",
          "for FORMA in f0 f1 f2 f3 f2s f3s; do",
          "  mkdir -p $OUT/$FORMA",
          "  while read -r Q; do",
          "    [ -z \"$Q\" ] && continue",
          "    SRC=/tmp/pas2/sql/$FORMA/$Q.sql",
          "    [ -f \"$SRC\" ] || continue",
          "    T0=$(date +%s)",
          "    for R in 1 2 3 4 5 6; do",
          "      psql -U postgres -d \"$DB\" -X -q -f \"$SRC\" > $OUT/$FORMA/$Q.r$R.txt 2>&1",
          "      if [ $R -eq 1 ]; then",
          "        T1=$(date +%s)",
          "        if [ $((T1-T0)) -gt 60 ]; then echo \"LENT $FORMA/$Q $((T1-T0))s: o singura rulare\"; break; fi",
          "      fi",
          "    done",
          "    echo \"gata $FORMA/$Q $(date -Is)\"",
          "  done < /tmp/pas2/lista-$FORMA.txt",
          "done",
          "echo \"=== STOP $SC $(date -Is) ===\"",
          "touch /tmp/pas2out/$SC.DONE"]
    with open(os.path.join(RUN, "run.sh"), "w", newline="\n") as fh:
        fh.write("\n".join(sh) + "\n")

    for f, ids in lista.items():
        print(f, len(ids), " ".join(ids))


main()
