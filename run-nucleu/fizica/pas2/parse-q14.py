# -*- coding: utf-8 -*-
"""Parserul Q14 (scrierea): mediana calda per forma, plus timpul triggerelor FK."""
import os, re, sys, statistics

BASE = os.path.dirname(os.path.abspath(__file__))
RE_EXEC = re.compile(r"Execution Time: ([\d.]+) ms")
RE_TRG = re.compile(r"Trigger for constraint (\S+?)(?: on \S+)?: time=([\d.]+) calls=(\d+)")
RE_TRG2 = re.compile(r"Trigger (?!for )(\S+?)(?: on \S+)?: time=([\d.]+) calls=(\d+)")


def serie(d, nume):
    t, trg = [], {}
    for r in range(1, 7):
        p = os.path.join(d, "%s.r%d.txt" % (nume, r))
        if not os.path.exists(p):
            continue
        txt = open(p, encoding="utf-8", errors="replace").read()
        ex = [float(x) for x in RE_EXEC.findall(txt)]
        if not ex:
            continue
        t.append(sum(ex))
        if r == 3:
            for m in list(RE_TRG.findall(txt)) + list(RE_TRG2.findall(txt)):
                trg[m[0]] = (float(m[1]), int(m[2]))
    if not t:
        return None
    calde = t[1:] if len(t) > 1 else t
    return {"n": len(t), "mediana": statistics.median(calde), "min": min(calde),
            "max": max(calde), "trg": trg}


def main(scara):
    d = os.path.join(BASE, "out", "raw-q14-" + scara)
    if not os.path.isdir(d):
        print("lipseste", d)
        return
    nume = ["f0", "f1", "f2", "f3"] + (["f2fk"] if scara == "x1" else [])
    print("### Q14 scrierea — %s (mediana a 5 rulari calde, ms)" % scara)
    print("| Forma | mediana | min | max | trigger FK (ms, total) |")
    print("|---|---|---|---|---|")
    for n in nume:
        s = serie(d, n)
        if not s:
            print("| %s | — | — | — | — |" % n)
            continue
        tt = sum(v[0] for v in s["trg"].values())
        det = "%.1f in %d triggere" % (tt, len(s["trg"])) if s["trg"] else "0 (fara FK)"
        print("| %s | %.2f | %.2f | %.2f | %s |" % (n, s["mediana"], s["min"], s["max"], det))
    for n in nume:
        s = serie(d, n)
        if s and s["trg"]:
            print()
            print("Triggerele %s (a treia rulare), descrescator:" % n)
            for k, v in sorted(s["trg"].items(), key=lambda x: -x[1][0]):
                print("- `%s`: %.2f ms, %d apeluri" % (k, v[0], v[1]))


main(sys.argv[1] if len(sys.argv) > 1 else "x1")
