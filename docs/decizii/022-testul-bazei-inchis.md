# Decizia 22 — Testul bazei

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Docs**: db/inventar/11-testul-bazei.md

## Regula durabilă

**Testul bazei — închis** (`db/inventar/11-testul-bazei.md`). Baza
`Document`: Numar, Data, PredatorId, PrimitorId, Stare(+DataOperare),
DocumentSursaId + Autogenerat, Total. Baza `DocumentDetaliu`: TipMaterialId
(Clasa se derivă din Tip), LotId?, Cantitate SEMNATĂ, Valoare (valoarea de
postare), AngajamentId? (+ TipTvaId?/ValoareTva din 36a). Tranșări:
(a) doar `Valoare` pe bază, intermediarele (preț/cotă/net) per derivată;
(b) granița dimensiunilor nu e în schema bazei ci în validare + metadata
(azi: frunze, 54c); (c) AngajamentId FK nullable, modulul angajamente
separat; (d) contul pe linia Factura* = politică (SursaCont), nu câmp;
(e) DECONT_* = câmpuri pe FacturaIntrare; scadența/PV = interfețe
(`IDocumentCuScadenta`, `IDocumentCuPV`); NR_NOTA moare (numărul notei
aparține registrului).

---

**Testul bazei (pasul 2) — închis; rezultatul canonic în
`db/inventar/11-testul-bazei.md`.** Baza `Document`: Numar, Data,
PredatorId, PrimitorId, Stare(+DataOperare), DocumentSursaId+Autogenerat,
Total calculat. Baza `DocumentDetaliu`: TipMaterialId (Clasa se derivă din
Tip — un singur FK), LotId?, Cantitate (SEMNATĂ — direcția LDI devine
`Directie` pe derivată, materializată în semn), Valoare, AngajamentId?,
Dimensiuni (owned, integral, tot nullable). Tranșări: (a) lanțul de valori
se taie la capăt — doar `Valoare` (valoarea de postare în registre,
unifică RECEPTIE_TVA/LIVRARE) e în bază, intermediarele
(pret/cotă/TVA/valoare fără TVA) sunt per derivată; TVA separat nu e în
bază — adăugare ulterioară aditivă. (b) granița dimensiunilor nu e în
schemă: owned type integral pe bază, ce diferă per tip = validare
declarativă + metadata UI. (c) AngajamentId = FK nullable pe bază;
modulul angajamente (head+detaliu+self-ref, tipuri legal/buget
anual/multianual) se proiectează separat. (d) contul pe linia Factura* =
politică per tip partener și/sau Clasă-Tip; câmp explicit doar ca fallback
ulterior, aditiv. (e) DECONT_* = câmpuri persistate pe FacturaIntrare;
DataScadenta și NumarPV/DataPV = interfețe (`IDocumentCuScadenta`,
`IDocumentCuPV`). NR_NOTA moare de pe document (numărul notei aparține
registrului contabil).
