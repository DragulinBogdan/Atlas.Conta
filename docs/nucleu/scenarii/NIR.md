# NIR — recepție manuală și conexul facturii

2026-09-24. **Implementat, scenarii verificate pe ambele profiluri.** Contract: TR-D7b T-D5,
T-D13, 090(h), 091 și 093 pentru Folosinta. Proveniență: regulă și politica
profilului. Cititorii comuni rămân TR-D8; 408 pe aviz rămâne TR-r4/B-r5.

## Recensământ

Recensământul comun `recensamant-asm-ldi-nir.sql`, rezultat în
`run-nucleu/tr-d7b/pas5-asm/recensamant.json`: 17.814 NIR, 34.289 linii,
toate autogenerate, zero recepții manuale, zero întârziate. 5.131 documente
cu produse multiple, 27 cu conturi multiple. Aceste cifre nu probează
recepția manuală; așteptările de mai jos sunt independente.

## Reguli și matrice numerică

S = 302 privat / 302.01.00 bugetar; F = 401 / 401.01.00 din politica
NIR/Stoc. D S: +q, lot, produs, gestiunea primitoare; C F: −q pe Furnizor,
produs, partidă proprie și partener numai când contul are RolTert (privat).
Fără fapt TVA, fără Carte=Fiscal. Conturile și analizele vin din politică.
Lot propriu pe frunză: prețul cules × q, rotunjit. Liniile istorice de bază
păstrează valoarea culeasă când lotul este propriu; lotul străin folosește
prețul lui, conform regulii existente NIR.PregatesteOperare (F5-D6).

| ID | Scenariu și așteptare | Rezultat așteptat |
|---|---|---|
| SC-NIR-01 | Manual 6 × 12,5 = 75: două postări, lot 6/75, datorie 75; prețul lotului în draft 0, declarație deterministă, dry-run fără efecte. | acceptat |
| SC-NIR-02 | Două loturi, 2/20 și 3/60: patru postări, o partidă 401 de 80; unitățile de stoc distincte. | acceptat |
| SC-NIR-03 | Storno în ianuarie al recepției 6/75: lot 0/0 și datorie 0, original intact; repetare refuzată. | acceptat/refuz stare |
| SC-NIR-04 | Recepție 2/30 în ianuarie; storno în februarie după închidere: ianuarie 2/30, februarie 0/0. | acceptat |
| SC-NIR-05 | Anulare/reoperare 2/20: zero efecte proprii după anulare, apoi aceleași postări. | acceptat |
| SC-NIR-06 | Corecție peste închidere a recepției 2/30: invers 2/30, lot nou 3/60, partidă pe document nou. | acceptat |
| SC-NIR-07 | Manual 2/50 → BCS 1/25: storno recepției refuzat, după inversarea consumului storno admis; lot 0/0. | refuz dependență, apoi acceptat |
| SC-NIR-08 | Manual 2/50 → PLT 20 împerecheată: datorie 30; storno plată refuzat până la desfacerea împerecherii. Desfacerea reface 50; storno plată, apoi NIR reface 0. | acceptat/refuz împerechere activă |
| SC-NIR-09 | FCT 4/100 cu NIR conex: înainte/după operare, anulare/reoperare și storno NIR, cubul FCT rămâne neschimbat; NIR are zero tranzacții proprii. | exclus prin politica conexului |
| SC-NIR-10 | Autogenerat cu sursă migrată dar fără PoliticaConex potrivită: nu este exclus; recepție proprie 1/10. Autogenerat fără sursă: idem. | acceptat |
| SC-NIR-11 | Data fizică ianuarie, înregistrare februarie: 1/10 numai în februarie; operarea în ianuarie închis refuză. | acceptat/refuz perioadă |
| SC-NIR-12 | Cantitate zero/negativă, lot absent, preț nul/negativ, gestiune greșită, produs/tip incompatibil, regulă/cont absent. | cod stabil pe declarație, zero efecte |
| SC-NIR-13 | 2 și 51 linii: număr constant de interogări, ≤16, determinism. | acceptat |
| SC-NIR-14 | Cantitate 0,001 × preț 0,001: valoare rotunjită 0, păstrarea cantității; 3 × 3,333333 = 10. | acceptat |
| SC-NIR-15 | Bugetar Folosinta 2/50 în gestiune reală → BTR 1/25 în altă gestiune; privat Marfuri 371 2/50. | acceptat |
| SC-NIR-16 | Toate ciclurile NIR: fără fapte TVA, matrice SC-X-14; contul terț are o partidă per document/cont/partener. | acceptat |
| SC-NIR-17 | Operand pur, 6 bucăți: lot propriu cu preț 12,5 și valoare veche 999 → 75; linie de bază fără preț, valoare 42 → 42; lot străin cu preț 3, indiferent de prețul cules 12,5 → 18. Ambele convenții de rotunjire. | acceptat |

## Limite și execuție

Comanda selectivă:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip NIR -Profil Ambele -Sufix .CodexBCS`.
Rezultat: **114 bugetar / 121 privat OK**, zero FAIL, exit 0, build fără
avertismente: `run-verificari/20260924-005954-434/rezultat.json`.
SC-NIR-13 măsoară **10/10 interogări** pentru 2/51 linii pe fiecare profil.
SC-NIR-12 pentru configurare și SC-NIR-17 sunt probe pure pe operand,
în ambele convenții; celelalte folosesc comenzile reale și cubul persistat.

Rularea inițială `20260924-005250-658` a expus ordinea greșită din scena
PLT: storno înaintea desfacerii împerecherii manuale. Proba a fost corectată
să aserteze refuzul existent, desfacerea, apoi storno; motorul nu a fost
relaxat. Rularea intermediară `20260924-005501-993` trece înaintea adăugării
probelor pure de configurare și valoare.

Recepția manuală nu rezolvă avizul pe 408 sau factura ulterioară
pe un lot recepționat înainte: TR-r4/B-r5. Excluderea conexului cere toate
condițiile T-D5; simplul Autogenerat sau simpla DocumentSursa nu ajung.
Import1C rămâne unealtă de migrare, nu gate.

Regresie integrală finală:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexBCS`:
**2.293 bugetar / 3.342 privat OK**, zero FAIL, exit 0, build fără
avertismente: `run-verificari/20260924-010719-358/rezultat.json`.
Prima regresie integrală (`20260924-010044-906`) a găsit trei probe private
eșuate dintr-o premisă depășită: STR-CONFIG folosea NIR ca tip fără
declarant, îl opera și provoca apoi a doua operare din contextul vechi.
STR-CONFIG folosește acum DEC, încă fără declarant; verificările cubului
și reconcilierii FCT/NIR au rămas neschimbate și trec.
Nucleul nu a fost modificat de această felie; proba 178/178 rămâne cea din
`run-verificari/20260923-235301-386/rezultat.json`.
Raport: `run-nucleu/tr-d7b/pas5-nir/raport.md`.
