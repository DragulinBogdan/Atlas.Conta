# Decizia 31 — Felia 3c-5 — Plata/Incasare + Imperechere

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §31

---

**Felia 3c-5 — Plata/Incasare + Imperechere — executată; validată e2e în
ModelCheck (FCT cu DECONT_* → plată autogenerată + imperechere automată →
gardieni → încasare manuală cu imperechere manuală → avans 542 → storno).
Tranșările:**
(a) **Laturile tipizate, liniile = defalcarea sumei** (echivalentul BREG_P):
Plata = ContPropriu → Partener/Angajat, Incasare invers; linia poartă
`Valoare` culeasă direct + `Dimensiuni`, fără stoc/lot/cantitate. Tipul
liniei la culegere manuală = tehnicul `TRZ` (Clasa TRZ, Natura=Tehnica,
fără ContImplicit); liniile autogenerate păstrează Tipul liniei sursă.
(b) **`ContImplicit` urcă pe baza `Repartitor`** (testul apartenenței:
intră în rezolvarea declarativă pentru ORICE latură) — partener 401/404/411,
ContPropriu 5xx/770, Angajat 542 (avans); `SursaCont.Partener*` redenumit
`Repartitor*` (valorile int neschimbate); migrarea mută datele existente.
(c) **Contare: un rând generic per tip, ambele conturi din laturi** —
PLT: debit=RepartitorPrimitor (fallback 401.01.00), credit=
RepartitorPredator FĂRĂ fallback (cont propriu fără cont = eroare clară);
INC: oglindit (fallback 411.01.01 pe credit). Conturi proprii seed
(legacy `casierie`): CASA→531.01.01, TREZ→770.00.00 (bancă).
(d) **Imperecherea NU e document** — link între două documente OPERATE;
invarianții trăiesc în `ImperechereService` (motor, aceeași cale pentru
UI/Web API): ambele Operat, suma>0, Σ imperecheri ≤ totalul fiecărei
părți, contrapartida trezoreriei (latura non-ContPropriu) apare pe
documentul stins — acoperă și lanțul avans↔decont↔regularizare (un doc
poate sta pe AMBELE roluri; Asignat numără ambele coloane). `ramas` =
calcul. Gardian nou în motor: anulare/storno refuzate cât există
imperecheri (link-ul se șterge liber, fără registre proprii).
(e) **Plata automată (00 §7) = hook `Document.GenereazaSecundar`** — spre
deosebire de conexul-clonă din PoliticaConex, secundarul se construiește
din date CULESE pe derivată: FCT cu `GenereazaPlata` → draft `Plata`
autogenerat (header din DECONT_*, liniile clonează defalcarea facturii:
valoare+dimensiuni+angajament); motorul îl marchează
Autogenerat+DocumentSursa → copil normal al grupului conex (șters la
anularea sursei, blochează anularea cât e operat). La operarea plății
autogenerate motorul creează imperecherea automată pe restul stingibil.
`Opereaza` întoarce `conex ?? secundar`. GenereazaChitanta rămâne
neactivat (nu are cont propriu cules — se tratează la fluxul BF, aditiv).
(f) Amânate, documentate: transferul între conturi proprii (pereche
PLT+INC conexă, 581 — 09 §4), importul extraselor de trezorerie
(xml_trezor — la API, ca importul FCT), imperecherea pe poziții
(GEST_DEFALCARE_DECONTARI, 00 §13.3 — nivelul de document ajunge),
obligativitatea clasificației bugetare pe liniile de plată (validarea
declarativă 3d, ca 29b).
