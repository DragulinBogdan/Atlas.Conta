# EX — închiderea exercițiului

**2026-10-16: pasul 1 al E3 (închiderea lunară a conturilor de rezultat).**
Reguli: contractul [E3](../e3-inchiderea-exercitiului-contract.md), E3-D3,
E3-D4, E3-D5. Pașii 2 și 3 adaugă rândurile exercițiului închis și ale
balanței pe an.

Fixture: anul 1975 liber (orice istoric anterior ar intra în soldurile
cumulate ale lunii), unitate internă `E2E-SC-EX-LOC`, ObjectSpace nou per
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
| SC-EX-10 | Seed-ul: rândurile lunare 6 și 7 → rezultat; privatul fără rânduri anuale; bugetarul 121 și 489 → 117.00.00, 520.02 și 521.02 → 521.03.00; `REZ-NEINCHIS` blocant. | seed | E3-D2, E3-D4, E3-D5, E3-D9 | `ScenariiEx`, ID-ul rândului | verificat, ambele |

Gardianul politicii (destinația în sursă, unicitatea sursei × moment,
coordonatele obligatorii ale destinației) e probat în suita F23, ca `E3-G1`.
Pe baza de import (`Atlas.Conta.Import1C.Flax`, ianuarie 2025, rulat la
2026-10-16), `ModelCheck --irz Atlas.Conta.Import1C.Flax 2025 1` dă 4.601
linii: 1C închide pe cont, iar postările originale stau pe material ×
repartitor. Netul pe cont e zero, în afară de 607 (−504,03) și 6028 (−9,84),
divergențele explicate ale importului. Cheia completă e hotărârea owner-ului
(contractul E3, tranșările pasului 1).
