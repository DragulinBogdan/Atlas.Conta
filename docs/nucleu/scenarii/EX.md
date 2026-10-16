# EX — închiderea exercițiului

**2026-10-16: pașii 1 și 2 ai E3 (închiderea lunară a conturilor de
rezultat, exercițiul închis).** Reguli: contractul
[E3](../e3-inchiderea-exercitiului-contract.md), E3-D3…E3-D5, E3-D7, E3-D9.
Pasul 3 adaugă rândurile balanței pe an.

Fixture: anii 1975 și 1976 liberi (orice istoric anterior ar intra în
soldurile cumulate ale lunii), unitate internă `E2E-SC-EX-LOC`, ObjectSpace nou per
comandă și curățenie în `finally`. Soldurile se nasc din note contabile
operate: cheltuiala pe 628 (privat) / 628.00.00 (bugetar), venitul pe 704 /
714.00.00, contrapartida pe contul de partide fără feluri de repartitor. Pe
bugetar nota poartă codul economic al scenei. Fiecare rând inversează
documentele lui, iar `SC-EX-00` verifică la intrare și la ieșire că luna n-are
sold de închis.

| Id | Scenariu și așteptare numerică | Rezultat așteptat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-EX-00 | Înaintea scenelor: previzualizarea lui 01/1975 dă `FaraSold`, fără linii, iar verificarea lunii n-are `REZ-NEINCHIS`; după scena de bază, luna e iar fără sold. | negenerare cu motiv | E3-D3, E3-D5 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-01 | Cheltuială 100 și venit 40 în ianuarie. Previzualizarea: două linii, 140, fără document. Generarea: draft cu dry-run acceptat și zero efecte. Operarea: 628 C100 și 704 D40 pe cheia-sursă (repartitorul unității, codul economic pe bugetar); rezultatul D100 / C40, fără repartitor și cod pe privat, cu ele pe bugetar. 6 și 7 la zero, rezultatul D60. | acceptat | E3-D3, E3-D4 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-02 | Cu închiderea vie a lunii, a doua generare dă `InchidereVie` cu documentul care blochează, fără duplicat. | negenerare cu motiv | E3-D3 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-03 | Stornoul închiderii readuce 628 D100, 704 C40, rezultatul 0; luna se generează din nou. | acceptat | E3-D3, 14 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-04 | Două cheltuieli, 30 pe unitate și 20 pe gestiunea scenei: o linie per cheie-sursă. Pe privat destinația pliază cele două chei într-una; pe bugetar repartitorul păstrat le ține separate. Rezultatul D50. | acceptat | E3-D3, E3-D4 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-05 | `REZ-NEINCHIS` e blocant fără document, are ca obiect draftul (care nu se repetă ca `DRAFT-IN-PERIOADA`), dispare după operare și revine, cu închiderea operată ca obiect, când o cheltuială de 7 se operează după ea. | constatare blocantă | E3-D5 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-06 | Draft pe cheltuiala 10, apoi încă 5 operat: operarea draftului refuzată fără postări, citirea îl dă depășit; regenerarea îl înlocuiește în aceeași tranzacție, iar noul document închide 15. | refuzat, apoi acceptat | E3-D3 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-07 | Draft în ianuarie: generarea lui februarie e refuzată, raportul spune `DraftAnterior`; închiderea vie în februarie: generarea lui ianuarie e refuzată, raportul spune `NeCronologica`. | refuzat cronologic | E3-D3 | `ScenariiEx`, ID-ul rândului | verificat, ambele |
| SC-EX-20 | Exercițiul 1975 fără decembrie definit dă numai `DECEMBRIE-NEDEFINIT`; cu decembrie definit și deschis, numai `DECEMBRIE-DESCHIS`; exercițiul 1976 dă în plus `EXERCITIU-PRECEDENT-DESCHIS:1975`, apoi, după închiderea lui 1975 și a lui decembrie 1976, nicio constatare. | constatări structurale blocante | E3-D7 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-21 | Decembrie 1975: cheltuială 100 și venit 40; pe bugetar și 489 C30, 520.02 D25. Previzualizarea pune rândurile `Anual` după cele `Lunar` (bugetar: trei linii anuale, privat: niciuna). Închiderea e datată 31.12, nu postează în 1976; pe bugetar 520.02 la zero și 521.03 D25. | acceptat | E3-D7, E3-D9 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-22 | Un rând `Anual` care nu e în seed (privat 121 → 1171; bugetar 520.01 → 521.01.00, cu codurile și sursa) se aplică în decembrie 1976 fără cod: privat 121 la zero și 1171 D70 (rezultatul cumulat pe doi ani); bugetar 520.01 la zero, 521.01 D15, 117 D40. Linia poartă coordonatele rândului. | acceptat | E3-D9 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-23 | Decembrie închis (cu `REZ-NEINCHIS` coborât de harness) fără închiderea conturilor: exercițiul dă numai `EX-SOLD-REZIDUAL`, blocant, cu conturile-sursă în text; închiderea e refuzată și cu cheia acceptată. | constatare blocantă | E3-D7 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-24 | Cu exercițiul închis, redeschiderea lui decembrie e refuzată. Cu 1975 și 1976 închise: redeschiderea lui 1975 e refuzată (doar ultimul), a lui 1976 fără motiv e refuzată; 1976, apoi 1975 se redeschid, decembrie rămâne închis, istoricul poartă motivul; a doua redeschidere a lui 1975 e refuzată. | refuzat, apoi acceptat | E3-D7 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-25 | Închiderea exercițiului 1975: decembrie are `ExercitiuInchis`, momentul și rândul de istoric `InchidereExercitiu`; a doua închidere dă `EXERCITIU-INCHIS`; nicio tranzacție `Deschidere` în 1975–1976. | acceptat | E3-D7, E3-D8 | `ScenariiEx.Exercitiu`, ID-ul rândului | verificat, ambele |
| SC-EX-10 | Seed-ul: rândurile lunare 6 și 7 → rezultat; privatul fără rânduri anuale; bugetarul 121 și 489 → 117.00.00, 520.02 și 521.02 → 521.03.00; `REZ-NEINCHIS` blocant. | seed | E3-D2, E3-D4, E3-D5, E3-D9 | `ScenariiEx`, ID-ul rândului | verificat, ambele |

Gardianul politicii (destinația în sursă, unicitatea sursei × moment,
coordonatele obligatorii ale destinației) e probat în suita F23, ca `E3-G1`;
starea exercițiului pe calea securizată, în suita perioadelor, ca `PER-V8e`.
`CV_AN_INCHIS` pe exercițiul închis (E3-D10) e probat în nucleu, pe ambele
ramuri.
Pe baza de import (`Atlas.Conta.Import1C.Flax`, ianuarie 2025, rulat la
2026-10-16), `ModelCheck --irz Atlas.Conta.Import1C.Flax 2025 1` dă 4.601
linii: 1C închide pe cont, iar postările originale stau pe material ×
repartitor. Netul pe cont e zero, în afară de 607 (−504,03) și 6028 (−9,84),
divergențele explicate ale importului. Cheia completă e hotărârea owner-ului
(contractul E3, tranșările pasului 1).
