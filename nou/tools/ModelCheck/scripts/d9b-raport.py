import argparse
import hashlib
import json
import statistics
from pathlib import Path


def citeste(cale):
    return json.loads(cale.read_text(encoding="utf-8-sig"))


def sha(cale):
    return hashlib.sha256(cale.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--citiri", type=Path, required=True)
    parser.add_argument("--scriere-a", type=Path, required=True)
    parser.add_argument("--scriere-b", type=Path, required=True)
    parser.add_argument("--destinatie", type=Path, required=True)
    args = parser.parse_args()
    erori = []

    def cere(conditie, mesaj):
        print(("OK   " if conditie else "FAIL ") + mesaj)
        if not conditie:
            erori.append(mesaj)

    comparatii = citeste(args.citiri / "comparatie.json")
    cere(len(comparatii) == 48, "48 perechi de citire A/B")
    cere(all(x["Egal"] for x in comparatii), "zero diferențe A/B de rezultate")
    cere(all(x["Acceptat"] for x in comparatii), "toate pragurile citirii respectate")
    medii = [citeste(p / "scriere-mediu.json") for p in (args.scriere_a, args.scriere_b)]
    for i, m in enumerate(medii):
        cere(m["FapteInainte"] == m["FapteDupa"], f"scriere {'AB'[i]} restaurează faptele și originile")
    cere(medii[0]["FapteDupa"] == medii[1]["FapteInainte"], "aceeași bază, schemă și fapte la A și B")
    cere(json.loads(medii[0]["FapteInainte"])["baza"] == "Atlas.Conta.ModelCheck.Privat.CodexD9bB",
         "comenzile A/B rulează pe baza B dedicată")
    cere(medii[0]["ModuleSha256"] != medii[1]["ModuleSha256"], "binare A/B distincte și identificate")
    scrieri = []
    for cate in (2, 51):
        a, b = [citeste(p / f"scriere-{cate}.json") for p in (args.scriere_a, args.scriere_b)]
        cere(a["Document"] == b["Document"], f"aceeași notă {cate} în ambele variante")
        cere(all(len(x["Masuri"]) == 6 and [m["Runda"] for m in x["Masuri"]] == list(range(6)) for x in (a, b)),
             f"{cate}: o încălzire și cinci comenzi măsurate în fiecare variantă")
        ma = statistics.median(m["Ms"] for m in a["Masuri"][1:])
        mb = statistics.median(m["Ms"] for m in b["Masuri"][1:])
        prag = max(1.15 * ma, ma + 10)
        cere(mb <= prag, f"{cate}: B {mb:.3f} ms <= {prag:.3f} ms; A {ma:.3f} ms")
        interogari = [sum(s.lstrip().startswith("SELECT") and '"OriginiPartide"' in s for s in m["Sql"])
                      for m in b["Masuri"]]
        cere(interogari == [1] * 6, f"{cate}: o citire de origine per comandă B")
        scrieri.append({"Partide": cate, "Document": a["Document"], "A": ma, "B": mb, "Prag": prag,
                        "Acceptat": mb <= prag, "InterogariOriginiB": interogari,
                        "MasuriA": [{k: v for k, v in m.items() if k != "Sql"} for m in a["Masuri"]],
                        "MasuriB": [{k: v for k, v in m.items() if k != "Sql"} for m in b["Masuri"]]})
    trepte = []
    for p in sorted(args.citiri.glob("d9b-f*"), key=lambda x: int(x.name[5:])):
        manifest = citeste(p / "manifest.json")
        masuri = citeste(p / "masuri.json")
        cere(len(masuri) == 144, f"f{manifest['Factor']}: toate cele 144 citiri prezente")
        cere(all(m["Eroare"] is None for m in masuri), f"f{manifest['Factor']}: fără erori de măsurare/plan")
        trepte.append({"Manifest": manifest, "Masuri": masuri})
    cere(len(trepte) == 4 and trepte[-1]["Manifest"]["Postari"] >= 5_000_000, "patru trepte până la cinci milioane")
    cere(trepte[-1]["Manifest"]["FapteMd5"] == json.loads(medii[0]["FapteInainte"])["postari"],
         "comenzile folosesc aceleași fapte ca treapta maximă de citire")
    cere(trepte[-1]["Manifest"]["ModuleSha256"] == medii[1]["ModuleSha256"],
         "același binar de produs B la citire și scriere")
    artefacte = []
    for p in (args.citiri, args.scriere_a, args.scriere_b):
        rezultat = p / "rezultat.json"
        cere(citeste(rezultat)["exit"] == 0, f"verifica.ps1 verde: {p.name}")
        artefacte.append({"Director": str(p), "RezultatSha256": sha(rezultat)})
    raport = {"Citiri": comparatii, "Trepte": trepte, "Scrieri": scrieri, "MediiScriere": medii,
              "Artefacte": artefacte, "Erori": erori}
    args.destinatie.write_text(json.dumps(raport, ensure_ascii=False, indent=2), encoding="utf-8")
    return 1 if erori else 0


if __name__ == "__main__":
    raise SystemExit(main())
