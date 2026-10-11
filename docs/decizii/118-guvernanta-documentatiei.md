# 118 — Guvernanța documentației: kitul unificat dă forma, Atlas.Conta îl aplică pe structura lui, cu gardieni care intră pe rând

- Data: 2026-10-10
- Stare: activă
- Execuție: forma kitului și cele 12 tranșări din §12 confirmate de owner 2026-10-10; aprobată de owner 2026-10-10; unealta `docskit` (118-r2) în repo-ul `DocsKit`
- Arii: dezvoltare-si-validare
- Amendează: 91 (l)
- Închide: 118-r6, 118-r9
- Docs: kitul `~/.claude/PROJECT_GOVERNANCE.md` (§1–§12, versiunea din 2026-10-10); unealta `docskit` (repo `DocsKit`, 0.2.0) cu `docs/docskit.json`; `nou/tools/docs/normalizeaza-antete.py` (doar normalizarea antetelor); `docs/stare-curenta/termeni.md`; `docs/decizii/README.md` (indexul generat)

## Regula durabilă

**Forma documentației e dată de kitul de guvernanță. Atlas.Conta îl aplică pe structura pe care o are deja, fără mutări în masă; ce se poate deriva se generează, iar fiecare regulă de formă primește gardian mecanic în momentul în care intră.**

(a) **Kitul e canonul formei.** Regulile de formă ale documentației, ale deciziilor, ale restanțelor, ale contractelor și ale codului sunt cele din kit; această decizie fixează doar aplicarea lor aici. Fondul nu se schimbă: invarianții, principiile transversale și regulile de domeniu rămân unde sunt. Un text vechi se aduce la formă la atingere sau printr-o trecere unică asistată de script, niciodată prin rescriere de mână în masă.

(b) **Antetul deciziei are câmpuri fixe**: `Data`, `Stare`, `Execuție`, `Arii`, `Amendează`, `Închide`, `Docs`, câte o linie. `Stare` ia o singură valoare din vocabularul închis `propusă` | `activă` | `amendată de N, M` | `depășită de N`; aprobarea, PR-ul și review-ul stau în `Execuție`. Titlul e `# N — afirmația`. Pentru deciziile 1–75 data e cea a primului commit în jurnal.

(c) **Sub-punctul amendat sau depășit poartă marcajul în loc**, la început: `(c) [amendat de 104] …`, `(d) [depășit de 102] …`. Textul lui rămâne; formularea curentă stă în pagina de stare curentă. `Amendează: N (x)` în antetul lui M și marcajul `[amendat de M]` în N (x) sunt cele două capete ale aceleiași legături și se verifică împreună.

(d) **Decizia depășită integral** primește banner sub titlu și se mută în `decizii/arhiva/`, cu același nume de fișier; linkurile se repară în același PR. Astăzi nicio decizie nu e depășită integral.

(e) **Indexul deciziilor e generat** între `<!-- gen:index -->` și `<!-- /gen:index -->` în `decizii/README.md`, în trei grupe: active, amendate, depășite. Linia scrisă de mână la fiecare decizie nouă dispare. Până la unealtă, generatorul e `normalizeaza-antete.py`, idempotent, rulat din rădăcina repo-ului cu `--scrie`.

(f) **`Arii` numește paginile stării curente** pe care decizia le atinge. Prima populare vine din citările `(Nx)` ale paginilor; o decizie necitată de nicio pagină are `Arii` gol și apare în raport, nu se completează din imaginație. Blocul generat `gen:decizii` din pagini vine odată cu unealta.

(g) **Termenii** stau în `stare-curenta/termeni.md`, pe forma din kit §3.4: domeniu, tehnic, identificatori; definiție de o propoziție, trimitere la regulă și la decizie. Un termen intră la prima lui folosire într-o pagină de stare curentă, în același commit.

(h) **Tipul documentului îl dă antetul**: `Tip: contract | analiza | review | catalog | note` pe orice fișier din afara `decizii/` și `stare-curenta/`. Folderele pe arie (`nucleu/`, `api/`, `privat/`, `import/`, `dim/`) rămân; `felii/` și `analize/` nu se creează acum. Un fișier fără `Tip` e jurnal: nu i se cer câmpuri, linkurile i se verifică.

(i) **Restanțele** păstrează triajul din 091 (k) ca vocabular de stări și primesc categoria `docs` pentru restanțele formei; `închisă prin N` e singura stare de închidere. `Închide: X` în antetul lui N și starea rândului X în registru sunt cele două capete ale aceleiași legături.

(j) **Unde suntem se generează**: `docs/stare.md` cu secțiunile din kit §4; `CLAUDE.md` §Stare devine un paragraf cu felia curentă și linkul spre `stare.md`; cronologia rămâne în `istoric-plan-de-lucru.md`, care își păstrează numele. Amendează 91 (l): nu „un paragraf pe arie", ci unul singur. Intră în vigoare când unealta produce `stare.md`.

(k) **Gardienii intră pe rând, la prag zero.** Unealta (nume de lucru `docskit`, repo propriu, dotnet global tool din feed-ul local, Markdig) are `gen`, `check`, `build`, `metrics`, pe specificația din kit §8. Proiectul nu are CI: `check` verde e condiție a PR-ului, rulat local, cu ieșirea atașată. Curățenia e comanda `/docs-curatenie`, la închiderea fiecărei felii și la orice decizie care amendează sau depășește o alta; nu există hook de sesiune.

(l) **Aria restanței e o coloană a registrului.** Fiecare rând din `restante.md` numește o singură pagină de stare curentă, din cele declarate ca arii în `docs/docskit.json`: `domeniu-si-operare` (motorul), `politici-si-fiscalitate`, `api-si-client` (XAF și React), `dezvoltare-si-validare`; `limite-curente` și `README` sunt transversale, nu arii. Deciziile rămân pe `Arii`; nu există etichetă de zonă, nici pe decizie, nici pe restanță. Registrul rămâne unul singur: vederea unei arii e filtrul pe coloană, iar `stare.md` grupează lista `activă` pe arie, într-un singur fișier. `migrare` și `docs` rămân stări de triaj, nu arii. Gardianul `arii`, separat de `restante` și la `eroare` din prima zi, cere aria din vocabular pe fiecare rând; `restante` rămâne la avertisment cât are datoria 118-r1. Ce citește o sesiune la început rămâne `stare.md`, sub 10 KB; fișiere pe arie apar doar dacă pragul cade.

## Context

Owner-ul a parcurs documentația pe structura din 2026-09-09 (invarianți, stare curentă pe responsabilități, decizii cu „Regula durabilă", restanțe cu nume) și a cerut trei lucruri: un glosar al termenilor, separarea explicită a deciziilor depășite cu o curățenie periodică, și o randare HTML navigabilă fără referințe externe. În paralel, din `product-fead` (Flax) a extras un kit de gestiune cu principii, tipuri de document cu criteriu de închidere, vedere generată, gardieni mecanici și șabloane.

Măsurătorile pe jurnal la 2026-10-10: 117 decizii, două formate de antet (100 vechi, 17 noi), 16 decizii la care clasa stării din antet diferă de cea din index, linia `Stare` ajunsă la 859 de caractere în decizia 110, `stare-curenta/README.md` cu „Actualizat" la 2026-09-27 și ultimul commit la 2026-10-07. „Picior" apare de 11 ori în stare curentă fără nicio definiție.

## Tranșări

Cele 12 tranșări între structura Atlas și kitul Flax sunt în kit §12, cu motivul fiecăreia. Cele care schimbă practica de aici: `Stare` vocabular închis cu ciclul de viață în `Execuție`; marcajul în sub-punct, nu îndoirea textului; indexul generat; depășitele în `arhiva/`; `Tip` în antet; restanțele rămân tabel cu rândul păstrat la închidere; o unealtă instalată o dată, în locul scripturilor copiate per proiect; `stare.md` generat în locul §Stare scris de mână.

Respins: hook de sesiune pentru curățenie (agresiv, nu e momentul); mutarea deciziilor în foldere pe componentă; renumerotarea sau redenumirea deciziilor; copierea kitului în repo înainte de a avea repo-ul unealtei (118-r4).

**Termenii (owner, 2026-10-10, pe review-ul ciornei glosarului).** Cuvântul din docs se alege pentru expresivitate; în cod intră doar în felia care atinge oricum conceptul. Tranșările, de aplicat în glosar prin 118-r6:

1. **latură** = a documentului, `Predator`/`Primitor`. Debit/credit pe postare = **sens**, în familia `SensTva`, `SensStingere`; nu „contare", care e regula de atribuire a conturilor. Nucleul are `enum Latura { Debit, Credit }` în 88 de fișiere: se redenumește `Sens` în C111 (118-r7). „Latura pereche" a viramentului = documentul pereche (65).
2. **partener fiscal** = partenerul din blocul fiscal al postării pe capătul intern cu fapt fiscal; nu poartă partidă (111). Respins: „contrapartidă fiscală".
3. **capăt** = una din cele două postări ale unei mișcări (`Capat`, 129 de apariții; T-D13); „picior" rămâne sinonim de vorbire. Respins: „proiecție", termenul citirilor (42c).
4. **unitate de sold** = obiectul nominalizat pe care se ține soldul (lot, partidă, fișă; N-D6); se desparte de `UnitateInterna`, de dimensiunea bugetară și de unitatea de măsură. `Unitate` din cod devine `UnitateSold` în TR-D9b (118-r8).
5. **deschidere** = tranzacția de deschidere a bazei; „sold inițial" rămâne coloana balanței; derivate cu rânduri proprii: documentul deschizător (114), partida deschisă (101). Respins: „SoldInitial" pentru tranzacție.
6. **corecție** = comanda peste graniță (F27-D6); **retragere** = Operat înapoi în Draft fără dependenți, numită azi „corecția directă" (104f, invarianți III), de redenumit la atingere, în vocabularul lui 106 (k).
7. **profil** rămâne; „profilul de validare" devine „politica de validare" (`PoliticaValidare`). Respins: „șablon", fiindcă profilul e realiniat continuu prin `DinSeed`.
8. **golire valorică** = eliminarea reziduului valoric al lotului golit (75, D9-D3, `Atribuit`); **reevaluare** rămâne a imobilizărilor (87). Respins: „închidere" (a treia după perioadă și TVA) și „regularizare" (D300, avans).
9. **explicație** = cea persistată pe tranzacție (X-D4); endpoint-ul `explica` se numește „explicarea configurației".
10. **registru** doar pentru rapoartele cu nume legal (registrul-jurnal, registrul imobilizărilor); cubul e „cubul"; README-ul stării curente și invarianții se corectează la atingere.
11. **nucleu** = `Atlas.Conta.Nucleu`; **motorul de operare** = `MotorOperare`; două clase `Motor` cu același nume e defect de cod, de rezolvat la atingere.
12. **contract** niciodată singur: contractul feliei, contractul laturilor, rezultatul motorului.
13. **gate** = doar suita integrală ModelCheck; pentru drepturi e „refuz", pentru felie e „regula de oprire".
14. Restul intrărilor `[de confirmat]` se rezolvă prin calificare, nu prin cuvinte noi: regim pe stare / regim de TVA; rolul de terț (`RolTert`), rolul liniei, rolul coloanei, rolul de securitate; dimensiune = analiticele laturii, coordonată = axa postării; materializare = scrierea contractului în cub la operare, snapshot = soldurile materializate la închiderea perioadei de referință (88). În tabelul identificatorilor: TR = trecerea pe cub, C = feliile de consolidare de după 091, sufixele „-B" și „-D" sunt același lucru, un pin aprobat de owner, numerotat diferit de la TR-D8; schema etichetelor din review-ul F27 nu e descrisă nicăieri și se spune ca atare.

## Review

Fără review advers la scriere: decizie de formă, fără efect pe motor sau schemă. Owner-ul decide dacă cere unul la aprobare.

## Ce rămâne deschis

**118-r1 — Curățenia antetelor normalizate.** Scriptul a păstrat verbatim în `Execuție` tot ce nu a putut clasifica: 20 de decizii cu `Execuție` peste 200 de caractere, 34 cu `Arii` gol, 11 închideri declarate în text pe restanțe pe care registrul le ține `după PoC` sau `activă` (73, 76, 77, 81, 90), 7 identificatori de restanță absenți din registru (D4-r14, 73-r1, 74-r4, 74-r6, 74-r9, 77k, 81k, T-r7). Se strâng la prima `/docs-curatenie`, nu de mână acum. Triggerul: prima rulare a lui `check`. Prima rulare (2026-10-10, docskit 0.1.0) a numărat, pe gardienii rămași la `avertisment`: `trimiteri` 27 (10 `Amendează: N` fără sub-punct care cer `depășită de`, 11 marcaje nepuse pe sub-puncte găsite inline, 3 `amendată de M` fără `Amendează` în M, 3 sub-puncte amendate care nu există), `restante` 219 (stări `închisă de`, `închisă la`, `închisă AAAA-LL-ZZ` în loc de `închisă prin N`; 6 id-uri fără corp la sursă: 52h, 62g/66j, 68j, N-r6, 110-r4, FZ-r5), `cod` 3 (`// 53b`, `// 3d` ×2 spre sub-puncte inexistente), `cai` 9 (căi vechi din 034, 068, 076, 089, din paginile de dezvoltare; `c111-coordonate-contract.md` vine cu C111), `antete` 1 (111 fără „Regula durabilă”, propusă). Fiecare gardian trece la `eroare` când ajunge la zero. Aria din (l) e exact atât de bună cât citările paginilor: decizia 110 nu e citată de nicio pagină de stare curentă (`Arii: README`), iar 32 de decizii au `Arii` gol; intră în aceeași curățenie.

**118-r2 — Unealta.** Repo propriu, `gen` și `check` întâi, `build` și `metrics` după; proba de compatibilitate pe linkurile cu ancoră existente. Triggerul: aprobarea acestei decizii. Livrat 2026-10-10: repo `D:\Dev\Personale\DocsKit` (C#, net10, Markdig, System.CommandLine; 16 teste pe o fixtură), instalat ca dotnet global tool `docskit` 0.1.0 din `nupkg/`; `gen` scrie indexul (byte-identic cu al scriptului la prima rulare), blocul `gen:decizii` pe cele 6 pagini, `docs/README.md` cu lista celor 283 de fișiere și `docs/stare.md`, idempotent (a doua rulare scrie zero); `check` rulează cei zece gardieni din kit §8 în 0,8 s; cele 80 de linkuri cu ancoră se rezolvă pe slug GitHub cu diacritice (două erau rupte și s-au reparat). La `eroare` din prima zi: `linkuri`, `generat`, `actualizat`, `orfane`, `termeni`; restul la `avertisment` cu cifrele în 118-r1. `build` și `metrics` rămân (ies cu cod 2). Scriptul de normalizare nu mai scrie indexul.

**118-r3 — `Tip` pe fișierele vechi și `consultari/`.** Fișierele din `nucleu/`, `api/`, `privat/`, `import/`, `dim/` primesc `Tip` la atingere sau într-o trecere unică; `consultations/` se absoarbe în `consultari/` cu `decizie.md` per consultare. Triggerul: `check` cu gardianul de antete activat.

**118-r4 — `docs-init`, `/docs-curatenie` și casa kitului.** Comenzile globale se extrag din kit după ce unealta se așază; kitul se mută în repo-ul unealtei, iar proiectul păstrează un pointer, nu o copie. Triggerul: 118-r2 închisă.

**118-r5 — Memoria agentului.** Predările din memorie trec în contracte și în istoric; indexul rămâne un rând per fir (kit §6.19, §11.6). Triggerul: prima felie închisă după 118-r2.

**118-r6 — Tranșările de termeni aplicate în glosar.** Cele 14 puncte din „Tranșări · Termenii" intră în `termeni.md`: intrările „capăt", „unitate de sold", „sens", „retragere", „golire valorică", „partener fiscal" cu rândurile lor, cele 22 de marcaje `[de confirmat]` scoase, paginile stării curente aliniate la atingere („picior" → „capăt", „corecția directă" → „retragere", „profilul de validare" → „politica de validare"). Triggerul: sesiunea următoare, pe context nou. Închisă 2026-10-10: glosarul are 65 de intrări (7 noi: sens, partener fiscal, document deschizător, partidă deschisă, retragere, golire valorică, nucleu; 3 redenumite: picior → capăt, unitate → unitate de sold, motor → motor de operare), zero marcaje; paginile rămân la atingere, cu aparițiile de aliniat: „picior" 11, „corecția directă" 1, „profilul de validare" 1, „registre" pentru cub în README și invarianți III.

**118-r7 — `Latura` din nucleu devine `Sens`.** `enum Latura { Debit, Credit }` din `Atlas.Conta.Nucleu/Cub/Latura.cs`, folosit din 88 de fișiere, se redenumește mecanic în felia C111, care schimbă oricum forma postării; `Latura` rămâne doar a documentului. Triggerul: C111 pasul care atinge `Postare`.

**118-r8 — `Unitate` devine `UnitateSold`.** 896 de apariții; redenumirea intră în TR-D9b, felia unităților, nu înainte. Triggerul: contractul TR-D9b.

**118-r9 — Deciziile și restanțele pe zone.** O sesiune pe motor nu are nevoie de deciziile și restanțele de UI (React/XAF), și invers; generatul (`stare.md`, blocurile paginilor) și registrul restanțelor trebuie să poată fi citite pe zona feliei curente. De tranșat: zona vine din `Arii` (paginile stării curente) sau dintr-o etichetă nouă pe decizie și pe restanță; registrul se sparge în fișiere pe zonă sau rămâne unul singur cu vederi generate pe zonă; `stare.md` primește secțiuni pe zonă sau fișiere `stare-<zona>.md`. Criteriul: ce citește o sesiune la început trebuie să rămână sub 10 KB. Triggerul: sesiunea următoare, pe context nou, cu owner-ul. Tranșată cu owner-ul 2026-10-11, regula în (l): aria vine din `Arii`, ca o coloană a registrului; registru unic, citit prin filtrul pe coloană; `stare.md` pe arie, un singur fișier; `migrare` și `docs` rămân stări. Respinsă eticheta nouă de zonă (a doua taxonomie lângă `Arii`) și spargerea registrului pe fișiere (reclasificarea ar muta rânduri). Popularea: 223 de rânduri din decizia-sursă sau din prefixul contractului, 153 clasificate după titlu, verificate de owner prin sondaj. Închisă prin 118.

## Istoric

- 2026-10-10: scrisă ca `propusă`, cu normalizarea celor 117 antete făcută de script în același PR.
- 2026-10-10: 118-r6 închisă, tranșările de termeni aplicate în `termeni.md`.
- 2026-10-10: aprobată de owner; `Stare: activă`.
- 2026-10-10: generatul subțiat pentru context (docskit 0.1.1): `stare.md` 41 → 9 KB, lista fișierelor 57 → 30 KB, blocurile paginilor 29 → 3 KB, titlurile din index tăiate la 120 de caractere; 118-r9 deschisă.
- 2026-10-10: 118-r2 livrată parțial (`gen`, `check`); (j) intră în vigoare: `docs/stare.md` e generat, §Stare din `CLAUDE.md` e un paragraf, textul vechi e în istoric.
- 2026-10-11: 118-r9 tranșată și închisă prin (l): aria restanței ca o coloană a registrului, `stare.md` pe arie; kitul §3.2, §4, §8 și docskit 0.2.1 (gardianul `arii` la `eroare`; `\|` în celulele registrului).
- 2026-10-11: docskit 0.5.0 aduce gardianul `tabele`; cele patru linii goale care rupeau registrul de la `100-r1` în jos (47 de rânduri randate ca text) sunt scoase, iar gardianul intră la `eroare` pe zero constatări (k). Folderul `generated` iese din citările de cod și din metrici înaintea liniei zero.
- 2026-10-11: la cererea owner-ului ies din metrici și `nou/tools` (ModelCheck, Import1C) și fișierul generat `UnitatiMasuraUnEce.cs`; C# măsurat scade de la 577 la 359 de fișiere, cu 11 peste plafon în loc de 34. Excluderea fiind comună cu citările, 50 din cele 126 de citări `// N` rămân neverificate de gardianul `cod`.
