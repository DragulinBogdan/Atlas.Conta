# REGIM — regimul pe stare (decizia 106)

**Actualizat: 2026-10-04.** Proba: `ModelCheck --scenarii REGIM [privat]`
(`tools/ModelCheck/ScenariiRegim.cs`, anul 2013). Scenariul e transversal:
folosește NTC, FCT, PLT și conexul NIR ca subiecți, iar așteptarea e regula
comenzilor (106b), nu postările.

Regula: Draft → editabil; Operează, Validează, Șterge. Operat → needitabil;
Anulează cere perioada înregistrării deschisă și niciun dependent (latură
pereche operată, conex operat, împerechere); Stornează și Corectează cer doar
lipsa dependenților. Stornat → nimic. Tipul adaugă comenzile proprii:
NTC → Stinge pe Operat; AMO, ITV → Regenerează pe Draft; ASM → Distribuie pe
Draft cu linii de consum și de produs; FCL → Generează descărcarea pe Operat,
cu gestiune de descărcare și rest nedescărcat.

| Id | Scenariu | Așteptare | Proba | Proveniență | Rezultat așteptat | Stare |
|---|---|---|---|---|---|---|
| SC-REGIM-01 | NTC draft, o linie 628 = 401, 100 | editabil; Operează, Validează, Șterge disponibile; Anulează, Stornează, Corectează, Stinge refuzate cu motiv | `SC-REGIM-01` | regulă (106b, 106c) | acceptat | implementat |
| SC-REGIM-02 | NTC operat, fără dependenți | needitabil; Anulează, Stornează, Corectează, Stinge disponibile; Operează, Validează, Șterge refuzate | `SC-REGIM-02` | regulă (106b, 48b) | acceptat | implementat |
| SC-REGIM-03 | FCT servicii 100 operată; PLT 100 operată; împerechere 100 | pe ambele: Anulează, Stornează, Corectează refuzate cu motivul împerecherilor; PLT needitabilă | `SC-REGIM-03` | regulă (31d, 106b) | acceptat | implementat |
| SC-REGIM-04 | ReadDto-ul FCT al facturii din 03 | `PoateEdita`, `PoateOpera`, `PoateAnula`, `PoateStorna` egale cu regimul | `SC-REGIM-04` | regulă (106d) | acceptat | implementat |
| SC-REGIM-05 | FCT stoc 2 × 50 operată; NIR conex operat | pe FCT: Anulează, Stornează, Corectează refuzate cu motivul conexelor | `SC-REGIM-05` | regulă (26d, 106b) | acceptat | implementat |
| SC-REGIM-06 | ianuarie închisă; NTC din 02 | Anulează refuzată cu motivul perioadei; Stornează și Corectează disponibile | `SC-REGIM-06` | regulă (14, 106b) | acceptat | implementat |
| SC-REGIM-07 | NTC din 06 stornată în februarie | needitabil; nicio comandă disponibilă | `SC-REGIM-07` | regulă (106b) | acceptat | implementat |

Probele structurale (106e), în afara catalogului: fiecare comandă din
toolbar-ul DetailView-ului unui `Document` numește o comandă a regimului
prin sufixul ID-ului; nicio affordance din `Api/` nu se calculează din
`StareDocument`; niciun controller nu poartă cheia „Stare”.
