# REGIM — regimul pe stare (decizia 106)

**Actualizat: 2026-10-04.** Proba: `ModelCheck --scenarii REGIM [privat]`
(`tools/ModelCheck/ScenariiRegim.cs`, anul 2013). Scenariul e transversal:
folosește NTC, FCT, PLT și conexul NIR ca subiecți, iar așteptarea e regula
comenzilor (106b), nu postările.

Regula: Draft → editabil; Operează, Validează, Șterge. Operat → needitabil;
Anulează, Stornează și Corectează urmează gardienii citibili ai retragerii
(106k): anularea îi citește pe toți, la data înregistrării; stornarea și
corecția numai pe cei care refuză la orice dată. Stornat → nimic. Tipul adaugă comenzile proprii:
NTC → Stinge pe Operat; AMO, ITV → Regenerează pe Draft; ASM → Distribuie pe
Draft cu linii de consum și de produs; FCL → Generează descărcarea pe Operat,
cu gestiune de descărcare și rest nedescărcat.

| Id | Scenariu | Așteptare | Proba | Proveniență | Rezultat așteptat | Stare |
|---|---|---|---|---|---|---|
| SC-REGIM-01 | NTC draft, o linie 628 = 401, 100 | editabil; Operează, Validează, Șterge disponibile; Anulează, Stornează, Corectează, Stinge refuzate cu motiv | `SC-REGIM-01` | regulă (106b, 106c) | acceptat | implementat |
| SC-REGIM-02 | NTC operat, fără dependenți | needitabil; Anulează, Stornează, Corectează, Stinge disponibile; Operează, Validează, Șterge refuzate | `SC-REGIM-02` | regulă (106b, 48b) | acceptat | implementat |
| SC-REGIM-03 | FCT servicii 100 operată; PLT 100 operată; împerechere 100 în luna deschisă | pe ambele: Anulează, Stornează, Corectează refuzate cu motivul împerecherilor; PLT needitabilă | `SC-REGIM-03` | regulă (31d, 106b) | acceptat | implementat |
| SC-REGIM-04 | ReadDto-ul FCT al facturii din 03 | `PoateEdita`, `PoateOpera`, `PoateAnula`, `PoateStorna` egale cu regimul | `SC-REGIM-04` | regulă (106d) | acceptat | implementat |
| SC-REGIM-05 | FCT stoc 2 × 50 operată; NIR conex operat | pe FCT: Anulează, Stornează, Corectează refuzate cu motivul conexelor | `SC-REGIM-05` | regulă (26d, 106b) | acceptat | implementat |
| SC-REGIM-06 | ianuarie închisă; NTC din 02 | Anulează refuzată cu motivul perioadei; Stornează și Corectează disponibile | `SC-REGIM-06` | regulă (14, 106b) | acceptat | implementat |
| SC-REGIM-07 | NTC din 06 stornată în februarie | needitabil; nicio comandă disponibilă | `SC-REGIM-07` | regulă (106b) | acceptat | implementat |
| SC-REGIM-08 | NTC din 01, 02 și 07 | pe Draft, Operat și Stornat, `MotivStergere` e exact motivul regimului pentru Șterge | `SC-REGIM-08` | regulă (106j) | acceptat | implementat |
| SC-REGIM-09 | ianuarie închisă; FCT și PLT din 03, împerecherea lor vie | pe ambele: Anulează refuzată cu motivul perioadei; Stornează și Corectează disponibile | `SC-REGIM-09` | regulă (088j, 106k) | acceptat | implementat |
| SC-REGIM-10 | PLT din 09 stornată în februarie; apoi FCT corectată | stornarea reușește și inversează împerecherea; pe FCT legătura inversată nu refuză Stornează și Corectează; corecția dă original Stornat și corecție Draft | `SC-REGIM-10` | regulă (088j, 106k) | acceptat | implementat |
| SC-REGIM-11 | privat: FCT servicii 100 + TVA 21 în februarie; D300 februarie depus; luna deschisă | Anulează disponibilă înaintea depunerii, refuzată după, cu `TVA_DEJA_DECLARATA`; Stornează și Corectează disponibile; comanda refuză la fel | `SC-REGIM-11` | regulă (103, 106k) | acceptat | implementat |
| SC-REGIM-12 | privat: aceeași factură în martie; numai D394 martie depus | ca 11, pe D394 | `SC-REGIM-12` | regulă (103, 106k) | acceptat | implementat |
| SC-REGIM-13 | NTC și FCT operate, fără dependenți | regimul se citește în cel mult 10, respectiv 12 instrucțiuni SQL | `SC-REGIM-13` | cost asumat (106f, 106k) | acceptat | implementat |
| SC-REGIM-14 | furnizor propriu; FCT servicii 100; NTC de 40, debit contul de furnizor pe acel furnizor, credit cheltuiala, care nominalizează FIFO 40 pe partida facturii; legătură manuală NTC → FCT de exact 40, fără transfer în cub; ianuarie închisă | limita 106-r7: regimul oferă Stornează și Corectează; ambele comenzi refuză `PARTIDA_CU_DEPENDENTI` în februarie; postările facturii intacte, legătura neinversată; sold partidă −60 | `SC-REGIM-14` | limită acceptată (106k, 106-r7) | refuzat la comandă | implementat |
| SC-REGIM-15 | aceeași pereche FCT 100 + NTC 40 pe alt furnizor, fără legătură | Anulează, Stornează și Corectează refuzate de regim cu `PARTIDA_CU_DEPENDENTI`; comanda refuză la fel; pentru SC-X-24 refuzul e abatere, nu refuz pe data cerută | `SC-REGIM-15` | regulă (106k) | acceptat | implementat |

Împerecherea vie într-o lună deschisă e SC-REGIM-03, cea dintr-o lună
închisă 09, cea inversată 10. Imobilizările au probele regimului în
[IMO](IMO.md) (SC-IMO-16, SC-IMO-17).

Conformitatea regim ↔ comandă pe tot catalogul e
[SC-X-24](README.md#lanțurile-transversale-sc-x-): helperii comuni ai
scenelor citesc regimul înaintea fiecărei comenzi Anulează, Stornează și
Corectează și compară. Regimul care refuză o comandă reușită e abatere fără
excepții. Comanda refuzată deși regimul o oferea e abatere, dacă
`ProbeRegim.Clasifica` nu o dovedește altfel pe starea documentului:

- pe valori: `STOC_INSUFICIENT` și `POZITIE_FARA_FISA_NEGATIVA`, nominal;
- pe data cerută, numai la stornare și corecție: luna imobilizărilor cât
  luna documentului e deschisă; `PARTIDA_CU_DEPENDENTI` cât nominalizarea e
  activă la data cerută și stinsă la orice dată ulterioară (SC-NTC-22);
- pe limita 106-r7: `PARTIDA_CU_DEPENDENTI` care dispare când partenerii
  legăturilor vii, calculați de probă din `Imperechere`, sunt lăsați
  deoparte. Scena declară câte asemenea refuzuri probează (`LimiteRegim`;
  REGIM declară 2, restul 0), iar orice alt număr pică SC-X-24.

Un dependent permanent pierdut de regim sau luna închisă a unui document de
imobilizări nu intră în nicio clasă: SC-REGIM-15 și SC-IMO-16 o probează pe
clasificator. Scenele de concurență cheamă comanda direct: acolo diferența e
concurență declarată.

Probele structurale (106e), în afara catalogului: fiecare comandă din
toolbar-ul DetailView-ului unui `Document` numește o comandă a regimului
prin sufixul ID-ului; nicio affordance din `Api/` nu se calculează din
`StareDocument`; niciun controller nu poartă cheia „Stare”.

Review advers 2026-10-04 la `908d3c8`: [C106-R1…R4](../c106-review-codex.md),
corectate prin 106 (k). SC-REGIM-03 și 06 au rămas neschimbate; cazurile
review-ului sunt 09–12 și SC-IMO-16/17.

Reverificare Codex la `5941b28`: [RV1](../c106-review-codex-rv1.md),
R1…R4 închise tehnic. C106-R5 (excepția globală pentru
`PARTIDA_CU_DEPENDENTI` din SC-X-24) e corectată prin clasificarea de mai
sus; proba adversă din RV1 a devenit SC-REGIM-14. Owner, 2026-10-04: limita
106-r7 rămâne la comandă.

Reverificare Codex la `86978f7`: [RV2](../c106-review-codex-rv2.md),
C106-R5 închisă. REGIM/NTC/IMO trec pe ambele profiluri; mutantul care
omite nominalizările din regimul stornării pică SC-X-24 în scena NTC,
fără a selecta aserțiunea explicită SC-REGIM-15.
