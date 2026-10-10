"""Adoptarea kitului de guvernanță (118): antetele deciziilor, marcajele, indexul.

Rulare din rădăcina repo-ului: python -X utf8 nou/tools/docs/normalizeaza-antete.py [--scrie]
Fără --scrie afișează raportul și nu modifică nimic. Rularea e idempotentă.
"""
import argparse
import pathlib
import re
import sys
from collections import defaultdict

CAMPURI = ["Data", "Stare", "Execuție", "Arii", "Amendează", "Închide", "Docs"]
CAMP_RE = re.compile(r"^-?\s*\**(Data|Stare|Execuție|Executie|Arii|Amendează|Amendeaza|Închide|Inchide|Docs)\**\s*:\**\s*(.*?)\s*$")
TITLU_RE = re.compile(r"^#\s+(?:Decizia\s+)?(\d{1,3})\s*[.—-]*\s*(.*)$")
VOCAB_RE = re.compile(r"^(propusă|activă|amendată de [\d, și]+|depășită de \d+)$")
SUBPUNCT_RE = re.compile(r"^(\s*-?\s*\**)\((?P<lit>[a-k])\)(\**)(?P<rest>.*)$")
MARCAJ_RE = re.compile(r"^\[(amendat|depășit) de \d+")
DATA_NOTA_RE = re.compile(r"\s*\(primul commit în jurnal\)")
ZGOMOT_RE = re.compile(r"\d{4}-\d{2}-\d{2}|PR #\d+|[A-Z][A-Za-z0-9]*-[A-Z]+\d+\w*|§\s*\d+|\b[A-Z]\d+\b|\d+\s*[–-]\s*\d+|pașii[^,;]*|pasul \d+|pas \d+")
DEBUG = set()
CITARE_RE = re.compile(r"\((\d{2,3})([a-k])?(?:\s*\(([a-k])\))?(?:[,/ ]+(?:\d{2,3})?[a-k]?(?:-r\d+)?)*\)|\((\d{2,3})-r\d+\)|\(decizia (\d{1,3})")


def citeste(p):
    return p.read_text(encoding="utf-8-sig")


def scrie(p, text):
    p.write_text(text, encoding="utf-8", newline="\n")


class Decizie:
    def __init__(self, cale):
        self.cale = cale
        self.numar = int(cale.name[:3])
        self.text = citeste(cale)
        self.linii = self.text.split("\n")
        self.camp = {}
        self.ordine = []
        self.titlu = ""
        self.i_titlu = -1
        self.i_antet_start = -1
        self.i_antet_end = -1
        self.probleme = []
        self._parseaza()

    def _parseaza(self):
        for i, l in enumerate(self.linii):
            m = TITLU_RE.match(l)
            if m and l.startswith("# "):
                self.i_titlu = i
                if int(m.group(1)) != self.numar:
                    self.probleme.append(f"titlul spune {m.group(1)}, fișierul e {self.numar}")
                self.titlu = m.group(2).strip()
                break
        if self.i_titlu < 0:
            self.probleme.append("fără titlu")
            return
        i = self.i_titlu + 1
        while i < len(self.linii) and not self.linii[i].strip():
            i += 1
        self.i_antet_start = i
        while i < len(self.linii):
            m = CAMP_RE.match(self.linii[i])
            if not m:
                if self.camp and self.linii[i].startswith("- ") and "Stare" in self.camp:
                    self.camp["Stare"] += "; " + self.linii[i][2:].strip()
                    i += 1
                    while i < len(self.linii) and self.linii[i].strip() and not self.linii[i].startswith(("#", "- ")):
                        self.camp["Stare"] += " " + self.linii[i].strip()
                        i += 1
                    continue
                break
            nume = m.group(1).replace("Executie", "Execuție").replace("Amendeaza", "Amendează").replace("Inchide", "Închide")
            valoare = m.group(2)
            i += 1
            while i < len(self.linii) and self.linii[i].strip() and not CAMP_RE.match(self.linii[i]) and not self.linii[i].startswith(("#", "- ")):
                valoare += " " + self.linii[i].strip()
                i += 1
            self.camp[nume] = valoare
            self.ordine.append(nume)
        self.i_antet_end = i
        if i < len(self.linii) and self.linii[i].startswith("- "):
            self.probleme.append(f"linie-bullet după antet: {self.linii[i][:60]}")
        for c in ("Data", "Stare"):
            if c not in self.camp:
                self.probleme.append(f"fără câmpul {c}")

    @property
    def e_normalizata(self):
        return "Arii" in self.camp

    def sectiune_regula(self):
        start = None
        for i, l in enumerate(self.linii):
            if l.startswith("## Regula durabilă"):
                start = i + 1
            elif start is not None and l.startswith("## "):
                return start, i
        return (start, len(self.linii)) if start is not None else (None, None)

    def linia_subpunct(self, lit):
        """(linie, poziția etichetei) pentru `(lit)`: la început de linie, altfel inline, înaintea primului `---`."""
        s, e = self.sectiune_regula()
        if s is None:
            return None
        for i in range(s, e):
            m = SUBPUNCT_RE.match(self.linii[i])
            if m and m.group("lit") == lit:
                return i, self.linii[i].index(f"({lit})")
        for i in range(s, e):
            if self.linii[i].startswith("---"):
                break
            m = re.search(rf"(?<![\w*])\({lit}\)(?=\s|$)", self.linii[i])
            if m:
                return i, m.start()
        return None


def curata(s):
    return s.replace("**", "").strip()


def interpreteaza_stare(d):
    """Întoarce (vocab, execuție, marcaje_proprii[(lit, verb, de)], amendata_de[int])."""
    s = curata(d.camp.get("Stare", ""))
    marcaje, de = [], set()
    m = re.match(r"^depășită de (\d+)", s)
    if m:
        return f"depășită de {int(m.group(1))}", s[m.end():].lstrip(" ;,.").strip(), marcaje, de
    m = re.match(r"^amendată de ([\d]+(?:\s*(?:,|și)\s*\d+)*)", s)
    if m:
        de |= {int(x) for x in re.findall(r"\d+", m.group(1))}
        rest = s[m.end():]
    else:
        rest = re.sub(r"^(activă|aprobată[^;.]*)[;,.]?\s*", "", s) if re.match(r"^(activă|aprobată)", s) else s
    for mm in re.finditer(r"\(?\b([a-k])\)?(?:/([a-k]))?\)?\s+(?:e\s+)?(amendată?|depășită?)\s+(?:de|prin)\s+(\d{1,3})([a-k])?\b", s):
        verb = "amendat" if mm.group(3).startswith("amend") else "depășit"
        for lit in (mm.group(1), mm.group(2)):
            if lit:
                marcaje.append((lit, verb, int(mm.group(4))))
                de.add(int(mm.group(4)))
    vocab = f"amendată de {', '.join(str(x) for x in sorted(de))}" if de else "activă"
    if re.match(r"^propusă", s):
        vocab = "propusă"
    executie = rest.strip(" ;,.")
    if re.match(r"^aprobată", s):
        executie = s.strip()
    return vocab, executie, marcaje, de


def extrage_amendeaza(d, numere_existente):
    """Din textul Stare: {N: set(litere)} pentru verbele amendează/depășește; litere goale = integral."""
    s = curata(d.camp.get("Stare", ""))
    s = ZGOMOT_RE.sub(" ", s)
    s = re.sub(r"\((?![^()]*(?:amendează|depășește|închide))[^()]*[a-zăîâșț]{3,}[^()]*\)", " ", s)
    rezultat = defaultdict(set)
    integral_depasite = []
    for mm in re.finditer(r"(amendează|depășește)\s+([^;]*)", s, re.I):
        verb_norm = mm.group(1).lower()
        verb, seg = verb_norm, mm.group(2)
        seg = re.sub(r"\b(invarian\w+|constatarea|TR-\w+|T-\w+|IM-\w+|[A-Z]{1,4}-[A-Z]\d+)\b[^,;]*", " ", seg)
        if d.numar in DEBUG:
            print(f"   [{d.numar}] {verb}: {seg!r}")
        gasit = False
        for n, lits in re.findall(r"\b(\d{1,3})\s*((?:\(\s*[a-k](?:\s*[/,]\s*[a-k])*\s*\)(?:\s*,\s*(?=\()|\s*(?=\()|\s*(?=\S)|\s*$))+|[a-k]\b)", seg):
            n = int(n)
            if n >= d.numar or n not in numere_existente:
                continue
            gasit = True
            for lit in re.findall(r"[a-k]", lits):
                rezultat[n].add((lit, "amendat" if verb == "amendează" else "depășit"))
        for n in re.findall(r"(?<![\w(-])(\d{1,3})(?![\w(-])", seg):
            n = int(n)
            if n >= d.numar or n not in numere_existente or n in rezultat:
                continue
            if verb == "amendează":
                rezultat[n]  # integral: fără litere
                gasit = True
            else:
                integral_depasite.append(n)
        if not gasit and seg.strip():
            d.probleme.append(f"{verb} fără referință parsabilă: „{seg.strip()[:70]}”")
    return rezultat, integral_depasite


def extrage_inchide(d, restante):
    s = curata(d.camp.get("Stare", ""))
    ids, neinchise, lipsa = [], [], []
    for mm in re.finditer(r"închide\s+([^;]*)", s):
        seg = mm.group(1)
        partial = "parțial" in seg
        for tok in re.findall(r"\b([A-Z0-9]+-r\d+|\d{2,3}[a-k])\b", seg):
            if tok not in restante:
                lipsa.append(tok)
            elif not restante[tok].startswith("închis"):
                neinchise.append(tok)
            elif tok not in ids:
                ids.append(tok)
        if partial:
            d.probleme.append(f"închide parțial: „{seg.strip()[:70]}”")
    return ids, neinchise, lipsa


def citeste_restante(cale):
    r = {}
    for l in citeste(cale).split("\n"):
        m = re.match(r"^\|\s*([^|]+?)\s*\|[^|]*\|\s*([^|]*?)\s*\|\s*$", l)
        if m and m.group(1) not in ("Id", "---"):
            r[m.group(1)] = m.group(2).replace("`", "").strip().lower()
    return r


def arii_din_pagini(dir_stare):
    arii = defaultdict(set)
    for p in sorted(dir_stare.glob("*.md")):
        if p.name in ("termeni.md",):
            continue
        slug = p.stem
        for m in CITARE_RE.finditer(citeste(p)):
            n = m.group(1) or m.group(4) or m.group(5)
            if n:
                arii[int(n)].add(slug)
    return arii


def antet_nou(d, vocab, executie, arii, amendeaza, inchide):
    docs = d.camp.get("Docs", "")
    data = DATA_NOTA_RE.sub("", curata(d.camp.get("Data", "")))
    linii = [f"- Data: {data}", f"- Stare: {vocab}"]
    if executie:
        linii.append(f"- Execuție: {executie}")
    linii.append(f"- Arii: {', '.join(sorted(arii))}".rstrip())
    if amendeaza:
        parti = []
        for n in sorted(amendeaza):
            lits = sorted({lit for lit, _ in amendeaza[n]})
            parti.append(f"{n} ({', '.join(lits)})" if lits else f"{n}")
        linii.append(f"- Amendează: {', '.join(parti)}")
    if inchide:
        linii.append(f"- Închide: {', '.join(inchide)}")
    if docs:
        linii.append(f"- Docs: {docs}")
    return linii


def marcheaza(d, lit, verb, de, raport):
    gasit = d.linia_subpunct(lit)
    if gasit is None:
        raport.append(f"  {d.numar:>3} ({lit}): sub-punctul nu e găsit în „Regula durabilă” → marcaj ne-pus [{verb} de {de}]")
        return False
    i, pos = gasit
    linie = d.linii[i]
    dupa = pos + len(f"({lit})")
    if linie.startswith("**", dupa):
        dupa += 2
    rest = linie[dupa:]
    if MARCAJ_RE.match(rest.strip()):
        return False
    d.linii[i] = f"{linie[:dupa]} [{verb} de {de}]{'' if rest.startswith(' ') else ' '}{rest}"
    return True


def genereaza_index(decizii, readme):
    text = citeste(readme)
    grupe = {"active": [], "amendate": [], "depășite": []}
    for d in sorted(decizii.values(), key=lambda x: x.numar):
        st = d.camp["Stare"]
        g = "depășite" if st.startswith("depășită") else "amendate" if st.startswith("amendată") else "active"
        rel = f"arhiva/{d.cale.name}" if d.cale.parent.name == "arhiva" else d.cale.name
        grupe[g].append(f"| {d.numar} | [{d.titlu}]({rel}) | {d.camp['Data']} | {st} |")
    bloc = ["<!-- gen:index -->"]
    for g, titlu in (("active", "Active"), ("amendate", "Amendate parțial"), ("depășite", "Depășite integral")):
        bloc += [f"### {titlu} ({len(grupe[g])})", "", "| # | Decizia | Data | Stare |", "|---|---|---|---|"] + grupe[g] + [""]
    bloc.append("<!-- /gen:index -->")
    bloc_text = "\n".join(bloc)
    if "<!-- gen:index -->" in text:
        return re.sub(r"<!-- gen:index -->.*?<!-- /gen:index -->", lambda _: bloc_text, text, flags=re.S)
    return re.sub(r"\| # \| Decizia \| Data \| Stare \|\n\|---\|---\|---\|---\|\n(?:\|.*\|\n?)+", lambda _: bloc_text + "\n", text)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--scrie", action="store_true")
    ap.add_argument("--radacina", default=".")
    ap.add_argument("--arata", default="")
    ap.add_argument("--debug", default="")
    args = ap.parse_args()
    DEBUG.update(int(x) for x in args.debug.split(",") if x)
    rad = pathlib.Path(args.radacina)
    dir_dec = rad / "docs" / "decizii"
    decizii = {}
    for p in sorted(list(dir_dec.glob("[0-9][0-9][0-9]-*.md")) + list((dir_dec / "arhiva").glob("[0-9][0-9][0-9]-*.md"))):
        d = Decizie(p)
        decizii[d.numar] = d
    numere = set(decizii)
    restante = citeste_restante(dir_dec / "restante.md")
    arii = arii_din_pagini(rad / "docs" / "stare-curenta")

    raport = []
    marcaje_de_pus = defaultdict(set)
    amendata_de = defaultdict(set)
    nr_normalizate = 0
    for d in sorted(decizii.values(), key=lambda x: x.numar):
        if d.probleme:
            raport += [f"  {d.numar:>3}: {pb}" for pb in d.probleme]
        if d.e_normalizata:
            continue
        nr_normalizate += 1
        vocab, executie, marcaje_proprii, de = interpreteaza_stare(d)
        amendeaza, dep_integral = extrage_amendeaza(d, numere)
        for n in dep_integral:
            raport.append(f"  {d.numar:>3}: „depășește {n}” integral — NE-APLICAT, de tranșat de mână")
        inchide, neinchise, lipsa = extrage_inchide(d, restante)
        for t in neinchise:
            raport.append(f"  {d.numar:>3}: închide {t}, dar registrul o ține „{restante[t]}” → rămâne doar în Execuție")
        for t in lipsa:
            raport.append(f"  {d.numar:>3}: închide {t}, care nu e în restante.md")
        for lit, verb, cine in marcaje_proprii:
            marcaje_de_pus[d.numar].add((lit, verb, cine))
        for n, lits in amendeaza.items():
            amendata_de[n].add(d.numar)
            for lit, verb in lits:
                marcaje_de_pus[n].add((lit, verb, d.numar))
        d._nou = (vocab, executie, arii.get(d.numar, set()), amendeaza, inchide, de)

    for d in decizii.values():
        if not hasattr(d, "_nou"):
            continue
        vocab, executie, ar, amendeaza, inchide, de = d._nou
        de = de | amendata_de.get(d.numar, set())
        if vocab == "activă" and de:
            vocab = f"amendată de {', '.join(str(x) for x in sorted(de))}"
        elif vocab.startswith("amendată") and de:
            vocab = f"amendată de {', '.join(str(x) for x in sorted(de))}"
        if not ar:
            raport.append(f"  {d.numar:>3}: Arii goale (nicio pagină de stare curentă nu o citează)")
        if len(executie) > 200:
            raport.append(f"  {d.numar:>3}: Execuție de {len(executie)} caractere → de strâns la curățenie")
        d._nou = (vocab, executie, ar, amendeaza, inchide, de)

    nr_marcaje = 0
    for n, lista in marcaje_de_pus.items():
        pe_cheie = {}
        for lit, verb, de in lista:
            if (lit, de) not in pe_cheie or verb == "depășit":
                pe_cheie[(lit, de)] = verb
        lista = {(lit, verb, de) for (lit, de), verb in pe_cheie.items()}
        for lit, verb, de in sorted(lista):
            if marcheaza(decizii[n], lit, verb, de, raport):
                nr_marcaje += 1

    for d in decizii.values():
        if not hasattr(d, "_nou"):
            continue
        vocab, executie, ar, amendeaza, inchide, _ = d._nou
        linii = antet_nou(d, vocab, executie, ar, amendeaza, inchide)
        d.linii[d.i_antet_start:d.i_antet_end] = linii
        d.linii[d.i_titlu] = f"# {d.numar} — {d.titlu}"
        d.camp["Stare"], d.camp["Data"] = vocab, DATA_NOTA_RE.sub("", curata(d.camp.get("Data", "")))

    print(f"decizii: {len(decizii)}; normalizate acum: {nr_normalizate}; marcaje puse: {nr_marcaje}")
    print(f"depășite integral: {sum(1 for d in decizii.values() if d.camp['Stare'].startswith('depășită'))}; amendate: {sum(1 for d in decizii.values() if d.camp['Stare'].startswith('amendată'))}")
    print("de revizuit:")
    print("\n".join(raport) if raport else "  (nimic)")

    for n in [int(x) for x in args.arata.split(",") if x]:
        d = decizii[n]
        print(f"--- {d.cale.name} ---")
        j = d.i_antet_start
        while j < len(d.linii) and d.linii[j].startswith("- "):
            j += 1
        print("\n".join(d.linii[d.i_titlu:j]))
        for l in d.linii:
            if re.search(r"\) \[(amendat|depășit) de", l):
                print("  ", l[:140])
    if args.scrie:
        for d in decizii.values():
            scrie(d.cale, "\n".join(d.linii))
        scrie(dir_dec / "README.md", genereaza_index(decizii, dir_dec / "README.md"))
        print("scris.")
    else:
        print("(raport; rulează cu --scrie ca să aplici)")


if __name__ == "__main__":
    sys.exit(main())
