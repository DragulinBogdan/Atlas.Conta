# ASM — transformarea produselor și conservarea cantității

2026-09-23. **Aprobat de owner; implementat și verificat, review advers aplicat.** Delimitarea (a)/(h) este aprobată la TR-D8, 2026-09-24 (D8-B4, T-r15). Completează T-D2
din `tr-d7b-tipuri-ramase-contract.md`. ASM se activează pe cub după
verificarea scenariilor. LDI și NIR nu sunt implementate
în această schimbare.

## ASM-B1 — Contraexemplul

Două bucăți din produsul A, lot LA, cost total 100, devin o bucată din
produsul B, lot nou LB, valoare culeasă 100. Valoarea se conservă; cantitățile
reale sunt A: −2, B: +1. Nu există conservare pe produs fără un capăt al
transformării în afara gestiunii reale.

T-D2 cere loturile ambelor laturi și refolosirea formei mixte. Forma actuală
nu descrie transformarea:

- `Miscare` poartă o singură cantitate, aplicată cu semne opuse celor două
  capete (`Nucleu/Cub/Miscare.cs:3–23`).
- `Mutare` are aceeași restricție și același cont pe ambele capete
  (`Nucleu/Motor/Mutare.cs:3–31`).
- `Conservare.VerificaCantitatea` cere suma zero pe fiecare produs
  (`Nucleu/Conservare.cs:64–75`); transferul cere suplimentar suma zero pe
  cont × produs (77–95).
- `Declaratie` admite numai mișcări și mutări; nu are un grup de intrări și
  ieșiri cu cantități independente și cauză proprie pe fiecare linie.

Acesta este un contraexemplu al reprezentării literale din T-D2, nu o
execuție eșuată a unui declarant ASM: declarantul încă nu există.

Recensământ read-only pe `Atlas.Conta.Import1C.Flax.TrD7b`, prin
`scenarii/recensamant-asm-ldi-nir.sql`: 1.228 ASM, 4.694 linii, toate cu
valoarea semnată totală zero. 1.225 au cantități nete nenule pe produs;
537 au chiar suma cantităților nenulă, 367 folosesc mai multe conturi,
541 au mai multe consumuri, 113 mai multe produse rezultate. Datele sunt
evidență pentru combinatorică, nu așteptări ale motorului nou (091d).

## ASM-B2 — Forma recomandată: capăt virtual de transformare

Se adaugă `GestiuniVirtuale.Transformare`, identitate deterministă, recunoscută
de `Este`. Este gestiunea transformării, fără rând nou în planul de conturi.
Conservarea cantității pe produs și pe cont × produs rămâne neschimbată.

Fiecare linie ASM produce:

1. o postare reală cu lot, produs, gestiunea Predator, cantitatea și valoarea
   proprii liniei; cauza rămâne documentul și linia respectivă;
2. o contrapondere numai de cantitate în `Transformare`, pe același cont și
   produs, fără unitate, valoare și valoare în valută zero. Cantitatea este
   inversul celei reale; cauza și analiza sunt ale aceleiași linii.

Ambele sunt în `Carte=Contabil`, fără TVA, perioadă fiscală sau partener.
`Transformare.Postari` refuză cu `ArgumentException` un capăt fără lot,
cu produs/cont incompatibil cu lotul, fără gestiune reală sau cu carte,
terț, TVA ori valută străine acestei forme.
Postarea reală este în `Spatiu=Stoc`; cea virtuală fără lot este în
`Spatiu=Contabil`, conform partiționării existente. Cititorii cantitativi
trebuie să selecteze gestiunea reală; suma cantității peste întreg cubul
este, intenționat, zero. Postările virtuale au rulaj valoric zero.

### Exemplul pe același cont: un Transfer

| Latură | Cont | Gestiune | Produs | Unitate | Cantitate | Valoare | Cauză |
|---|---|---|---|---|---:|---:|---|
| D | 371 | magazie | A | LA | −2 | −100 | consum |
| D | 371 | Transformare | A | — | +2 | 0 | consum |
| D | 371 | magazie | B | LB | +1 | +100 | produs |
| D | 371 | Transformare | B | — | −1 | 0 | produs |

Σ valoare pe 371/D = 0; Σ cantitate pe fiecare produs = 0. Loturile reale
scad cu 2/100, respectiv cresc cu 1/100. Rulajele pe cont exclud Transfer.

### Exemplul pe conturi diferite: o Operare

| Latură | Cont | Gestiune | Produs | Unitate | Cantitate | Valoare | Cauză |
|---|---|---|---|---|---:|---:|---|
| C | 301 | magazie | A | LA | −2 | 100 | consum |
| C | 301 | Transformare | A | — | +2 | 0 | consum |
| D | 345 | magazie | B | LB | +1 | 100 | produs |
| D | 345 | Transformare | B | — | −1 | 0 | produs |

Contabil: D 345 / C 301: 100. Fără 711; BPR rămâne rezervat (019), în afara
acestei felii. Simbolurile sunt exemple, rezolvate din conturile loturilor.

## ASM-B3 — Mai multe conturi, fără perechi inventate între linii

T-D2 nu definește perechea consum → produs pentru n consumuri și m produse.
Liniile se grupează după contul lotului. Clasificarea se face o singură
dată, înaintea absorbției, pe valoarea produsă culeasă P_g și consumul
registrelor R_g (ASM-B6):

- grup cu valoare produsă egală cu valoarea consumată: toate liniile lui
  intră în Transfer, pe debit, cu valori semnate;
- celelalte grupuri intră integral în Operare: consum pe credit, produs
  pe debit, valori pozitive;
- fiecare contrapondere cantitativă rămâne în tranzacția liniei reale;
- documentul întreg trebuie să aibă valoarea produsă egală cu cea consumată.

Extragerea grupurilor balansate lasă Operare balansată. Rezultă cel mult
o Operare și un Transfer, fără împărțirea cantității sau a cauzei unei
linii între ele și fără o matrice arbitrară de alocare între componente.

Exemplu mixt: 371 consum 60 și produs 60 → Transfer; 301 consum 40 și
345 produs 40 → Operare D 345 / C 301: 40.

Consecință aprobată explicit: 371 consum 60, 371 produs 100 și 301 consum
40 intră integral în Operare: D 371: 100, C 371: 60, C 301: 40.
Rulajul pe 371 este 100/60, variația soldului +40. Nu se deduce automat
un transfer de 60 în interiorul liniei de produs; documentul nu culege
acea alocare. Acesta este un amendament de precizie la T-D2, nu o regulă
deja stabilită de contractul anterior.

## ASM-B4 — Declarația și evaluarea

Nucleul primește o primitivă pură pentru un grup de transformare, cu linii
tipate consum/produs, capăt real, cantitate, valoare și cauză per linie.
`Declaratie` o admite alături de `Miscari`/`Mutari`. Motorul produce forma
de mai sus și verifică aceleași conservări. Nu primește documente EF,
simboluri de cont sau un comutator pe tipul ASM.

Declarantul evaluează consumurile prin `Evaluare.Iesire`, în ordinea stabilă
a liniilor și cu soldul rămas per lot. Produsele folosesc valoarea culeasă
la `PretEvaluare`, rotunjită la bani. Gardul P = R și absorbția temporară
în cub sunt definite în ASM-B6. Diferența de culegere refuză atomic cu
`ASAMBLARE_NEBALANSATA`, cu R, P și diferența; operatorul redistribuie.
`DistribuieValoarea` rămâne comanda explicită, prețul nu devine pondere.
La scara monetară 2, toleranța 0,005 nu permite o diferență nenulă de bani.
Consum și produs sunt obligatorii; cantitățile absolute sunt pozitive;
lotul produs se naște pe propria linie. Refuzurile specifice se probează
și prin `Materializare.Refuzuri` cu coduri stabile.

Suprafață: primitiva nouă, `Declaratie`, `Motor`, `GestiuniVirtuale`, teste
de proprietate; `DeclarantAsamblare`, hook pe Asamblare, seed pe ambele
profiluri; adaptări strict necesare în materializare pentru lista nouă,
scena `ScenariiAsm` și înregistrarea ei. Fapte rămâne generic și în bugetul
de interogări. Nu se schimbă schema cubului, API-ul, UI-ul sau Import1C.

Stornoul selectează tranzacțiile Transfer de stoc ale documentului prin
prezența unei unități Lot, apoi TOATE postările acelor tranzacții, inclusiv
contraponderile fără unitate. Transferurile împerecherilor sunt excluse.
Materializarea verifică `Conservare` pe invers înainte de scriere.
Stornoul inversează exact ambele feluri și contraponderile cantitative;
anularea șterge ambele tranzacții. Cititorii pe cont și stornoul unui
Transfer păstrează obligația comună TR-D8/N-r8, cu probă mixtă obligatorie.

## ASM-B5 — Probe înaintea activării

Catalogul `scenarii/ASM.md` cere postările de mai sus ca mulțime exactă,
soldurile reale și absența valorii pe contraponderi. Nucleul probează
generativ n→m, cantități și produse diferite, grupuri mixte, conservarea
valorii/cantității și inversarea. Eliminarea unei contraponderi, schimbarea
produsului sau contului ei trebuie detectate. Rulajele fără Transfer
trebuie să rămână 0 în primul exemplu și 100 în al doilea.
Verificarea perechii pe cont × produs × cauză se aplică și în Operare și
Storno; nu se bazează numai pe conservarea suplimentară a Transferului.

Alternative: relaxarea conservării cantității pentru transformări ar cere
un contract distinct și un marcaj verificabil al transformării; eliminarea
globală a conservării ar slăbi și recepția/consumul. Nu sunt implementate.
Forma aprobată este ASM-B2 împreună cu regula n→m din ASM-B3.

## ASM-B6 — Varianta A, numai în regimul dual

R = consum evaluat de registre, C = consum evaluat pe raportul curent,
P = valoarea produselor culese. Se verifică întâi ΣP = ΣR pe ușa entității
și explicit pe ușa declarației. R se calculează din aceleași fapte închise,
cu preț înghețat și golire, fără interogări per linie și fără a presupune
că apelantul a executat pregătirea entității.

Pentru fiecare grup pe cont, Δ_g = C_g − R_g. Dacă are produse, Δ_g se
atribuie ultimului produs al grupului, în ordinea `(Pozitie, ID)`. Dacă
este numai consum în partea Operare, Δ_g se atribuie ultimului produs din
Operare. Toate contribuțiile se însumează per linie înaintea gardului:
valoarea finală a fiecărui produs trebuie să fie strict pozitivă.

Un Δ nenul fără ancoră eligibilă refuză atomic, fără reclasificare automată
(`ASAMBLARE_DELTA_FARA_ANCORA`). Cazul include grupul numai-consum cu
R_g = P_g = 0 și C_g > 0, clasificat Transfer. Produsul final nepozitiv
refuză cu `ASAMBLARE_PRODUS_NEPOZITIV`. Lipsa uneia dintre direcții sau
lotul produs nepropriu refuză cu `ASAMBLARE_STRUCTURA_INVALIDA`.

Absorbția modifică numai postarea din cub: `PretEvaluare`, valoarea liniei
și prețul lotului din registre rămân intacte. Deciziile raportează document,
linie țintă, cont sursă, R, C, P și contribuția Δ; suma pe linie explică
valoarea finală. Stornoul inversează valorile istorice ajustate; corecția
evaluează din nou, după inversare, și poate avea alt Δ.

Deciziile sunt în contractul execuției; nu sunt încă persistate (S-r2).
Diagnosticul istoric nu le reconstruiește presupunând cauza diferenței.

La TR-D9 distribuirea folosește calculul pur disponibil înaintea gardului
de balansare, fără dependență de registre; absorbția se scoate pentru
operații noi. Istoricul cu Δ rămâne și se inversează exact. Proba-capcană
2/10,01 trebuie să refuze înainte de redistribuire în ambele regimuri.

## ASM-B7 — Reziduul valoric transversal al regimului dual

Formula nouă primește azi soldul registrelor. Pe lot 3/10, trei documente
BCS sau ASM de câte 1 consumă în cub 3,33 + 3,34 + 3,34: sursa ajunge la
0/−0,01. Regula țintă rămâne 0/0; rezultatul dual este excepție explicită,
nu toleranță generală. FCL este afectat numai prin lanțul FCL → DSC.

Diagnosticul `--reconciliere-cub` compară valorile pe lot × gestiune × cont,
Carte=Contabil, la aceeași dată, cu inversări semnate și proveniență.
D final = D inițial + Σ diferențe la intrări − Σ diferențe la ieșiri.
Istoricul incomplet și diferențele neexplicate se raportează distinct;
nu toate diferențele sunt declarate automat N-r3. Se listează pozițiile
cu cantitate zero și valoare nenulă. Contraponderile Transformare sunt
excluse structural numai din diagnosticul comparabil, cu numărul raportat.

Implementarea diagnosticului compară Magazie/Marfuri și numără separat
rândurile celorlalte tipuri de stoc și postările cubului identificate în
afara acestui domeniu. Cubul nu stochează `TipStoc`: identificarea se face
prin mișcările registrelor pe lot × gestiune × document de grup × dată ×
storno. O cheie cu cel puțin o mișcare Magazie/Marfuri rămâne în comparație;
lipsa corespondentului nu justifică excluderea și rămâne istoric incomplet.
Registrele nu au cont istoric pe rând:
acesta se rezolvă din tipul materialului curent. Proveniența permite
investigarea, nu certifică retroactiv N-r3 sau Δ; diferențele complete sunt
etichetate neexplicate, iar lipsa corespondenței ca istoric incomplet.
Acest diagnostic este raport și nu schimbă exit-ul `--reconciliere-cub`:
codul de ieșire depinde numai de (a)–(g), conform T-D10.

**Delimitare aprobată de owner la TR-D8 (2026-09-24, T-r15).** Înlocuiește
amânarea din 2026-09-23: Operare ASM se exclude nominal numai din comparația
contabilă (a), iar (h) raportează separat numărul documentelor/postărilor și
diferențele pe cont/latură/lună. Contractul complet și riscul asumat sunt în
`tr-d8-citiri-contract.md` §D8-B4. `NUC-ASM-RECONCILIERE` cere în (h) exact
D 40/C 40 față de zero în registre pentru documentul mixt, 1 document și
4 postări Operare (2 economice și 2 contraponderi zero), cu (a)–(g) fără
diferențe și exit 0. Probele independente rămân obligatorii; efectele ASM
intră integral în cititorii contabili. Excepția este numai pentru regimul
dual și nu declară egalitatea completă între cub și registre.

Restanța T-r13 are două condiții de închidere: evaluări noi pe propriul
sold complet al cubului și tratamentul explicit al soldurilor istorice
deja divergente. Trecerea cititorului la TR-D8 nu șterge reziduurile vechi.
TR-D8 probează și filtrarea structurală a contraponderilor în jurnal,
fișă, GL și MovementOfGoods (T-r14).

## Verificare

Scenariile și manifestele de execuție sunt în [ASM.md](scenarii/ASM.md).
Suita integrală după review: 2.003 verificări bugetar / 3.097 privat,
zero FAIL, exit 0 comun. Nucleu: 178/178, zero omise.
Citirea operandului: 12 interogări atât la 2, cât și la 51 de linii.
T-r12 este închisă prin aplicarea review-ului; aprobarea delimitării T-r15
și verificarea ei sunt consemnate în D8-B4 și catalogul ASM. LDI/NIR nu sunt incluse în această implementare.
