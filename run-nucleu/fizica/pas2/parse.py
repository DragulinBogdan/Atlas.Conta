# -*- coding: utf-8 -*-
"""Parserul iesirilor EXPLAIN (pas 2): mediana calda, min/max, buffers, indexii atinsi."""
import os, re, sys, json, statistics, shutil

BASE = os.path.dirname(os.path.abspath(__file__))
FORME = ["f0", "f1", "f2", "f3", "f2s", "f3s"]

RE_EXEC = re.compile(r"Execution Time: ([\d.]+) ms")
RE_PLAN = re.compile(r"Planning Time: ([\d.]+) ms")
RE_BUF = re.compile(r"Buffers: shared hit=(\d+)(?: read=(\d+))?")
RE_IDX = re.compile(r"(?:Index Scan|Index Only Scan|Bitmap Index Scan|Parallel Index Scan|"
                    r"Parallel Index Only Scan|Parallel Bitmap Index Scan)"
                    r"(?: Backward)? (?:using|on) ([A-Za-z0-9_\"\.]+)")


def citeste(path):
    txt = open(path, encoding="utf-8", errors="replace").read()
    if "ERROR" in txt:
        return None
    ex = [float(x) for x in RE_EXEC.findall(txt)]
    if not ex:
        return None
    pl = [float(x) for x in RE_PLAN.findall(txt)]
    # bufferele nodului radacina al FIECAREI instructiuni: prima linie Buffers dupa
    # inceputul planului. Aproximare: primele len(ex) aparitii, in ordine.
    bufs = RE_BUF.findall(txt)[: len(ex)] or []
    hit = sum(int(a) for a, _ in bufs)
    read = sum(int(b) for _, b in bufs if b)
    idx = sorted(set(i.strip('"') for i in RE_IDX.findall(txt)))
    return {"exec": sum(ex), "plan": sum(pl), "hit": hit, "read": read,
            "idx": idx, "stmt": len(ex)}


def serie(rad, forma, q):
    d = os.path.join(rad, forma)
    runs = []
    for r in range(1, 7):
        p = os.path.join(d, "%s.r%d.txt" % (q, r))
        if not os.path.exists(p):
            break
        v = citeste(p)
        if v is None:
            return {"eroare": open(p, encoding="utf-8", errors="replace").read()[:400]}
        runs.append(v)
    if not runs:
        return None
    calde = runs[1:] if len(runs) > 1 else runs
    t = [x["exec"] for x in calde]
    return {"n": len(runs), "mediana": statistics.median(t), "min": min(t), "max": max(t),
            "plan": statistics.median([x["plan"] for x in calde]),
            "hit": calde[0]["hit"], "read": calde[0]["read"],
            "idx": calde[0]["idx"], "stmt": runs[0]["stmt"],
            "o_rulare": len(runs) == 1}


def main(scara):
    rad = os.path.join(BASE, "out", "raw-" + scara)
    rez = {}
    for forma in FORME:
        d = os.path.join(rad, forma)
        if not os.path.isdir(d):
            continue
        quri = sorted(set(f.split(".r")[0] for f in os.listdir(d)))
        for q in quri:
            s = serie(rad, forma, q)
            if s:
                rez.setdefault(q, {})[forma] = s
    with open(os.path.join(BASE, "out", "rezumat-" + scara + ".json"), "w") as fh:
        json.dump(rez, fh, indent=1)

    # EXPLAIN-ul rularii a 3-a, integral, in pas2/explain/
    exp = os.path.join(BASE, "explain")
    os.makedirs(exp, exist_ok=True)
    for forma in FORME:
        d = os.path.join(rad, forma)
        if not os.path.isdir(d):
            continue
        for f in os.listdir(d):
            if f.endswith(".r3.txt"):
                q = f.split(".r")[0]
                shutil.copyfile(os.path.join(d, f),
                                os.path.join(exp, "%s-%s-%s.txt" % (q, forma, scara)))

    ordine = ["q01", "q02", "q02b", "q03", "q03c", "q04", "q04b", "q05", "q06",
              "q07", "q08", "q09", "q09b", "q10", "q11", "q12", "q12b", "q13"]
    print("### %s — mediana ms (min–max), 5 rulari calde" % scara)
    cap = "| Q | " + " | ".join(f.upper() for f in FORME) + " |"
    print(cap)
    print("|" + "---|" * (len(FORME) + 1))
    for q in ordine:
        if q not in rez:
            continue
        cel = []
        for f in FORME:
            s = rez[q].get(f)
            if not s:
                cel.append("—")
            elif "eroare" in s:
                cel.append("EROARE")
            else:
                m = "%.1f" % s["mediana"]
                cel.append("%s (%.1f–%.1f)%s" % (m, s["min"], s["max"], "*" if s["o_rulare"] else ""))
        print("| %s | %s |" % (q.upper(), " | ".join(cel)))

    print()
    print("### %s — shared hit/read (prima rulare calda)" % scara)
    print(cap)
    print("|" + "---|" * (len(FORME) + 1))
    for q in ordine:
        if q not in rez:
            continue
        cel = []
        for f in FORME:
            s = rez[q].get(f)
            cel.append("—" if not s or "eroare" in s else "%d/%d" % (s["hit"], s["read"]))
        print("| %s | %s |" % (q.upper(), " | ".join(cel)))

    print()
    print("### %s — indexii atinsi de planificator" % scara)
    folos = {}
    for q, pf in rez.items():
        for f, s in pf.items():
            if "eroare" in s:
                continue
            for i in s["idx"]:
                folos.setdefault(i, set()).add("%s/%s" % (f, q.upper()))
    for i in sorted(folos):
        print("- `%s`: %s" % (i, ", ".join(sorted(folos[i]))))


main(sys.argv[1] if len(sys.argv) > 1 else "x1")
