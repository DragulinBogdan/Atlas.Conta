# -*- coding: utf-8 -*-
"""FZ-r2: medianele fisei de la Sold, pe functiile din parse.py (aceeasi disciplina FZ-D8)."""
import os, sys

_src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "parse.py"), encoding="utf-8").read()
_ns = {"__file__": os.path.join(os.path.dirname(os.path.abspath(__file__)), "parse.py")}
exec(compile(_src[:_src.rfind("main(")], "parse.py", "exec"), _ns)  # parse.py isi ruleaza main() la import
serie = _ns["serie"]

BASE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out", "raw-fzr2")
for scara in sys.argv[1:] or ["x1", "x10"]:
    rad = os.path.join(BASE, scara)
    print("== %s" % scara)
    print("| forma/q | mediana | min | max | plan | hit | read | indexi |")
    print("|---|---|---|---|---|---|---|---|")
    for forma in ["f0", "f2", "f3", "f2s", "f3s"]:
        d = os.path.join(rad, forma)
        if not os.path.isdir(d):
            continue
        for q in sorted(set(f.split(".")[0] for f in os.listdir(d))):
            s = serie(rad, forma, q)
            if not s or "eroare" in s:
                print(forma, q, s); continue
            print("| %s/%s | %.1f | %.1f | %.1f | %.1f | %d | %d | %s |" % (
                forma, q, s["mediana"], s["min"], s["max"], s["plan"], s["hit"], s["read"], ",".join(s["idx"])))
