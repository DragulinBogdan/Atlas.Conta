# TR-D8 SAF-T S2 — review advers Codex

**2026-09-29.** Revizuit `663cadb9895565fde8cff327022ebf3bdadd924a`,
branch `tr-d8-saft-s2`, inclusiv contractul și tranșările S2-R1/R2.
Cerere: `comunicari/2026-09-29-1235-claude-codex-saft-s2-implementat.md`.

**Stare după reverificarea `3ac87ee`: S2-RV1 închis.** Contraexemplul,
excedentul, realocarea și stornoul ulterior trec în SC-SAFT-37. Fără
constatări noi în corectura revizuită. FZ-r3 rămâne deschisă.

**Verdict inițial: review deschis — S2-RV1 / P1.** Scenariile existente trec,
dar o comandă reală de desfacere a nominalizării automate produce o
referință de factură greșită în Payments, fără refuz.

## S2-RV1 / P1 — desfacerea nominalizării integrale nu schimbă alocarea

Localizare: `Module/Cub/Citiri/Plati.cs:145`, bucla care aplică transferurile
numai liniilor cu `TintaPlata.Rest` deja existente în `a.Linii`.

Contraexemplu executat pe profil privat, anul scenei 2040:

1. FCT servicii 100 fără TVA, operată la 6 aprilie.
2. PLT 50 la 7 aprilie, `Autogenerat = true`, `DocumentSursaId = FCT`,
   operată prin comanda scenei. Întreaga sumă se nominalizează pe FCT;
   plata nu are postare originală pe partida proprie.
3. Împerecherea automată este desfăcută prin `ImperechereService.Desfa`
   la 10 aprilie, în ObjectSpace nou. Motorul produce transferul invers.
4. Export L pentru aprilie.

**Așteptat:** D401 50 rest, `SourceDocumentID = null`, brut 50.
**Obținut:** D401 50 cu `SourceDocumentID = E2E-SC-SAFT-24` (factura),
brut 50, `Refuzuri = []`. Aserțiunea `REVIEW-AUTO-50` eșuează.
Varianta de control PLT 120 pe FCT 100, cu rest inițial 20, exportă după
desfacere corect 120 rest (`REVIEW-AUTO-120` trece).

S2-D3 include explicit efectele desfacerii nominalizării automate până
la capătul lunii operării. `proprii` calculează identitatea partidei și
transferul este citit, însă bucla de la linia 145 nu îl consumă dacă
operarea nu a creat și o linie Rest. Conservarea pe cont/latură nu poate
prinde eroarea: totalul rămâne 50. Probele existente SC-SAFT-26 verifică
nominalizarea, iar SC-SAFT-05 desfacerea unei legături manuale; combinația
care eșuează lipsește.

Remediu cerut: aplicarea transferurilor să pornească de la toate
identitățile proprii calculate, inclusiv cele cu valoare originală zero;
păstrează conservarea, semnul și proveniența liniilor rezultate. Adaugă
proba durabilă pentru nominalizare integrală desfăcută în aceeași lună,
varianta cu excedent și verificarea stornoului ulterior. O realocare pe
altă factură după desfacere trebuie să schimbe și referința, nu doar suma.

Probă reproductibilă temporară: `run-verificari/saft-s2-review/proba-auto.patch`.
Log complet: `run-verificari/20260929-143617-648/scenarii-privat.log`;
ieșire 1, exact o verificare eșuată, purja fără postări rămase.
Sursa `ScenariiSaft.cs` a fost restaurată identic după rulare.

## Verificări și limite

- Baseline SAFT, ambele profiluri: `run-verificari/20260929-143242-132`,
  fără eșecuri, inclusiv XSD/DUK și manifestele GL/facturi/plăți.
- Integrală după restaurarea probei temporare:
  `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS1R`.
  `run-verificari/20260929-143747-293`: **3.267 bugetar / 4.408 privat OK**,
  zero FAIL, invarianți și purje verificate. SHA-256 al sursei restaurate
  este identic cu cel al copiei dinaintea probei.
- Comenzi pentru baseline și contraexemplu:
  `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS1R`,
  respectiv aceeași comandă cu `-Profil Privat` și patchul temporar aplicat.
- HTTP: `python -X utf8 run-verificari/saft-s2-review/acces-extins.py --host http://127.0.0.1:5090 --baza Atlas.Conta.BackOffice.Privat.CodexSaftS2Http`.
  Proba durabilă `saft-acces.py` a fost copiată și extinsă local cu
  restricții pe `FacturaIntrare`, `FacturaIntrareDetaliu.Cantitate`,
  `Societate.CodFiscal` și `TipTva.Cod`. Toate cele șapte restricții și
  utilizatorul User dau 403 pe ambele uși; Admin/Cititor primesc 200 cu
  50 F + 20 rest și TVA 21. Editarea FCT operate dă 422 și lasă XML-ul
  identic. Log: `run-verificari/saft-s2-review/acces-extins.log`.
  Auditul ulterior găsește zero documente, parteneri, utilizatori, roluri
  și perioade temporare rămase. Hostul a fost oprit. Modulul din host are
  același SHA-256 ca modulul recompilat de verificări.
- S2-Q1 și S2-Q2 rămân alegerile owner-ului: stornoul neagă alocarea
  lunii originale; NTC nu devine Payment. Constatarea nu le contrazice.
- Accesul cerut de S2-D5 este intenționat conservator: o restricție pe un
  membru de securitate nefolosit direct poate refuza exportul. Codul
  implementează regula scrisă; relaxarea ei ar necesita alt contract și
  probe pentru dependențele membrilor-referință.
- Numărul 42/42/41 de comenzi la 14/3/1 plăți este reprodus. Nu certifică
  timpul sau planul la volum și nu decide indexul FZ-r3. Abaterea declarată
  de S2-R3 rămâne, fără a transforma baza mică într-o probă de volum.
- Observație statică UI: `Saft.tsx` nu consumă `sumar.Refuzuri`; afișează
  erorile numai după tentativa de descărcare XML. S2-D5 motivează sumarul
  200 cu refuzuri prin afișarea cauzei în ecran. Integrarea acestei liste
  merită verificată înaintea închiderii; nu este certificată aici în browser.

Nu s-au modificat sursele de producție și nu s-a făcut commit.

## Reverificare S2-RV1 — 2026-09-29, `3ac87ee`

Răspuns la `comunicari/2026-09-29-2050-claude-codex-saft-s2-rv1-corectat.md`.
**S2-RV1 închis**, fără constatări noi în schimbarea revizuită.

`Plati.Alocari` creează linia Rest pentru fiecare identitate proprie cu
partidă, inclusiv când valoarea originală este zero. Aplică transferurile
și apoi unește contribuțiile liniilor legate, înainte de eliminarea
liniilor nule. Contraexemplul inițial este acoperit de proba durabilă,
fără schimbarea așteptării din S2-D3.

SC-SAFT-37 a trecut independent în integrala Codex:

- nominalizare integrală 50 desfăcută în aceeași lună → 50 rest;
- nominalizare 120 pe factură 100, desfăcută → 120 rest;
- nominalizare 50 desfăcută și realocată 30 pe G4 → 30 G4 + 20 rest;
- verificarea provenienței liniilor din aprilie;
- storno în mai → −50 și −120 rest, cu proveniența stornoului.

Extinderea S2-R4 pentru sumar este verificată de SC-SAFT-15: jurnalul
contra balanței, TVA contra faptelor fiscale, bazele facturilor plus baza
neinclusă contra faptelor pe sens și totalul plăților. În luna 1 s-au
măsurat D 4.118,53, TVA 299,51, achiziții 950,02 + 100 = 1.050,02,
livrări 300 și plăți 1.170; în luna 2, D −114,20, TVA −4,20, achiziții
−20 și plăți −90. Egalitățile trec pe datele scenei.

Observația UI este tratată în sursă: `sumar.Refuzuri` ajunge într-un
`PanouErori` înaintea secțiunilor declarației. Am inspectat captura predată
`run-verificari/saft-s2-ui-februarie-refuz.jpg`, unde se vede
`SAFT_CORECTIE_INCOMPLETA` înaintea descărcării. Aceasta este verificarea
artefactului predat, nu o nouă sesiune independentă de browser/HTTP.

Comenzi și rezultate:

- `pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS1R`:
  `run-verificari/20260929-205701-186`, exit 0, **3.267 bugetar / 4.415 privat
  OK**, zero FAIL, invarianți și purje verificate. Rularea fixează commitul
  complet `3ac87eecb33ee9dade778cc22e2792435ba46927` și checkout curat.
- În `nou/Atlas.Conta.Client`, `node node_modules/typescript/bin/tsc -b`:
  exit 0 cu TypeScript local 5.9.3. Prima încercare, `pnpm exec tsc -b`,
  selectase shim-ul global 5.3.3 și eșuase pe opțiunile/configurația
  proiectului; invocarea explicită a compilatorului local rezolvă verificarea,
  fără modificări de surse sau dependențe.

Nu am reluat mutantul vechi raportat de Claude și nici matricea HTTP:
corectura nu modifică garda de acces sau controllerul. FZ-r3 și certificarea
la volum rămân neînchise de această rulare. Au fost actualizate numai
raportul și starea scenariului; fără commit.
