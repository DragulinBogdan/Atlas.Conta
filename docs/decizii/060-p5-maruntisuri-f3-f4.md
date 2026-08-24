# Decizia 60 — Mărunțișurile F3/F4 golite

- **Data**: 2026-08-09 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §60

---

**Mărunțișurile F3/F4 golite** (itemii minori din contractele §Închidere;
ModelCheck verde ambele profiluri, probele live în browser + curl pe baza
de dev). Tranșările:
(a) **M2**: violările de constraint DB se traduc acum și pe WebApi — catch
în `ContaApiController.Domeniu` (`ConstraintViolationTranslator.TryTranslate`
→ 422 + EroriDto; modelul EF dedus din excepție), template-urile RO extrase
în `MesajeConstraintRo` (Module), consumate de AMBELE host-uri.
Constatare la probă: calea exotică originală (DELETE FCL draft cu linie
referită de DSC manual) NU se mai reproduce — DELETE prin API e ștergere
AMÂNATĂ (GCRecord=1, probat live cu DSC injectat prin SQL), deci FK-ul nu
se atinge; catch-ul rămâne plasa pentru orice violare viitoare.
(b) **Fix de FOND scos de sanity-check (falsul „minor")**: `CoduriTip`
făcea GetObjectByKey per document (interogarea TPT completă per rând) —
pe panoul de stingeri al unui extras de import cu 335 de rânduri = **11s
la cald**; presupunerea „mulțime mărginită" nu ține pe trezorerie. Fix:
materializare POLIMORFĂ într-un SINGUR query pe bază (sub TPT, EF întoarce
tipul derivat corect; designul ancorei pe numele clasei CLR neatins) —
**11,3s → 0,185s (61×)**, addendum în `p5-perf-masuratori.md`.
(c) **Ordinea `Stingeri`**: cronologic după DATA celeilalte părți (CASE în
SQL), Id tiebreak — panoul se citește ca istoric; era arbitrară pe Guid.
(d) **M3**: check-uri ModelCheck pe „Id repetat în payload" și „linie de
tip BAZĂ prin Id" + proba că linia de bază absentă din payload e curățată
de reconciliere.
(e) **D-5b**: indiciu în panoul „Grup conex" pe draftul autogenerat
(anularea sursei îl șterge cu tot cu modificări), verificat în browser.
(f) **Mesajul „nimic de generat + rest"**: NEREPRODUCTIBIL — probat pe un
backorder real (produs fără stoc → operare → generare manuală): mesajul se
afișează și PERSISTĂ după recitire; constatarea din F4 rămâne istorică.
Artefactele de probă (FCL-2 backorder, FCT PROBA-D5B-1 + PLT autogenerat)
rămân pe baza de dev, ca la feliile anterioare.
