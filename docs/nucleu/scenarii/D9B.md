# D9B — originea partidei

- Tip: catalog
- Actualizat: 2026-10-14
- Contract: [TR-D9b](../tr-d9b-contract.md), D9B-D1…D7

Catalogul separă **A, comportamentul existent**, de **P, proiecția
persistată cerută de contract**. Verde pe A nu certifică P. Din pasul 4,
produsul scrie și citește tabela `OriginiPartide`; validarea P este în
[raportul implementării](../tr-d9b-pas4-origini.md).
Proveniența așteptărilor: contractul D9B, identitatea din 092, ordinea FIFO
din 114–117, retragerea și stornoul existente. Sumele sunt RON fără TVA;
aceste probe nu restrâng funcționalitatea privată din celelalte cataloage.

## Fixture și comenzi

`ScenariiD9b`, codul `D9B`, marcaj `E2E-SC-D9B`, anul propriu 1977,
verificat liber la pregătire. Facturile sunt de servicii, 628 = 401 la
privat, 628.00.00 = 401.01.00 la bugetar, cu codul economic cerut de profil.
Fiecare caz folosește titulari proprii. Comenzile documentelor trec prin
`ComenziDocument.Sistem` în ObjectSpace-uri noi, aceeași coajă ca API-ul.
Nu este probă HTTP. Deschiderea și alocarea la partida inițială folosesc
intrările existente `Materializare.Deschide` / `Imperecheaza`, tranzacționat.
Curățenia din `finally` include deschiderea identificată prin titularii
marcajului; nu inserează manual în cub și nu citește registre vechi.

Data implicită este 05.01.1977. Soldurile semnate sunt debit minus credit;
tabelul exprimă resturile în magnitudine. Probele citesc atât agregatul
partidei, cât și `PartideCuRest`, cu așteptări numerice constante.

## Catalog

| ID | Montaj și așteptare | Rezultat așteptat | Stare A | Stare P |
|---|---|---|---|---|
| SC-D9B-01 | FCT100, PLT nominalizată40 → rest60; exact o origine FCT, zero origine proprie PLT. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-02 | A100/B70, plata40 pe A → 60/70, două origini. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-03a | O NTC, două datorii100 pe același cont, titulari diferiți → două origini; stingere100 a primei → 0/100. | acceptat | verificat ambele, check SC-D9B-03a | verificat ambele |
| SC-D9B-03b | O plată40/60, două conturi cu partide (401/4111, respectiv401.01.00/411.01.01), politici proprii pe tipurile liniilor → două origini și resturi40/60 chiar dacă explicația declară una. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-04 | FCT100, PLT liberă40, împerechere40 → 60/0 și două origini; desfacere → 100/40; transferurile au sumă zero per cont și sens. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-05 | FCT100, PLT40:60; desfacerea legăturii și retragerea plății:100; reoperare:60. Separat, retragerea FCT fără dependenți elimină originea; reoperare la 07.01 cu DataInregistrare aliniată → același ID, exact o origine, noua dată. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-06 | FCT100 la05.01, storno10.01 → rest0, o origine păstrată; citirea la05.01 →100. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-07 | Inițiale60/40, referințe diferite; plată20 alocată primei →40/40, documente nule; storno →60/40. Retragerea înaintea stornoului refuză și păstrează postările. | acceptat; retragere refuzată, familia „stingere”, cod STINGERE_DESCHIDERE_INVALIDA emis de gardul motorului | verificat ambele | verificat ambele |
| SC-D9B-08 | Facturi60/40 în aceeași zi, în această ordine de creare; NTC30 fără pin →30/40. Inițiale30/50 în aceeași zi, prima cu ID mai mare; NTC40 →0/40 după ordinal. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-09 | Închidere ianuarie după FCT100/PLT40 →60 direct și snapshot; PLT20 în februarie →40, istoricul ianuarie rămâne60. | acceptat | verificat ambele | verificat ambele |
| SC-D9B-10 | Rol cu plata40 și factura70 vizibile, suportul facturii100 invizibil → seturi de origini A/B egale, exact originea70; rest40 fără document FCT și rest70; CRUD proiecție refuzat inclusiv admin securizat. | citire filtrată; CRUD refuzat fără efecte | origini A/B verificate ambele în pasul 5 | verificat ambele; HTTP privat verde |
| SC-D9B-11 | Eșec înainte de commit →0 origini/postări/tranzacții noi; retry →1 origine; concurență fără dublare. | eșec atomic, reluare acceptată | fără proiecție în termenul A | verificat ambele |
| SC-D9B-12 | FCT100, NTC nominalizată40 și storno în aceeași zi; retragere FCT →0 origini proprii, două postări străine +40/−40; reoperare →1 origine, rest100. | acceptat | verificat ambele | verificat ambele |

SC-D9B-05 nu ocolește gardul legăturilor: plata autogenerată creează
`Imperechere`, iar retragerea cere întâi comanda de desfacere/ștergere a
legăturii. Această precondiție a fost observată prin rulare. Nici gardul,
nici așteptarea numerică nu au fost schimbate.

## Validarea termenului A

Comanda executată:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip D9B,CITIRI,DESCHIDERE,PLT,NTC -Profil Ambele -Sufix .CodexD9bRename
```

Rulare finală verde: `run-verificari/20261012-125206-497/rezultat.json`,
inclusiv SC-D9B-03b și regresia catalogului existent selectat.
Pe fiecare profil: toate check-urile scenei verzi, `INV-CUB` verificat
pe 13 scene înainte de purjă, zero postări reziduale, `RAMURA-NUL` verde. Două rulări
anterioare au semnalat precondiții greșite ale fixture-ului (legătura plății,
apoi DataInregistrare); sunt păstrate în loguri, nu sunt rezultate de produs.

## Probe adăugate pentru P

SC-D9B-10 rulează și pe ObjectSpace securizat, cu rolul care ascunde factura
și postările ei: plata rămâne vizibilă, iar restul40 nu primește documentul
sau eticheta facturii. Adminul securizat nu poate crea, modifica ori șterge
origini. SQL-ul verificat are `EXISTS`, `LIMIT`, fără hash și fără ramură
de nul. Pe ușa de sistem a hostului, deschisă din aceeași sesiune
restrictivă, SQL-ul de origine nu atinge `Postare` și întoarce și originea
ascunsă rolului. Calea HTTP privată este `origini-partide.py`, prin
`verifica.ps1 -Suita D9bHttp -Profil Privat -Sufix .CodexD9bOrigine`:
FCT100, PLT40 și împerechere40 prin API, aceleași valori și etichete;
fără endpoint OData, inclusiv pentru numele găsite în metadata EDM.

SC-D9B-11 injectează căderea atât înainte de salvare, cât și după
`SaveChanges`, înaintea commit-ului tranzacției. În ambele cazuri rămân
zero origini, postări și tranzacții noi, documentul Draft; retry produce
o origine. Originea incompatibilă din ObjectSpace este refuzată la
materializare cu `ORIGINE_PARTIDA_INCONSISTENTA`, nu de declarant.
PK și cheia naturală refuză dublarea; două comenzi simultane produc o
singură operare. Notele cu 2/51 titulari verifică 2/51 origini, credit200/
5100 și număr constant de citiri de origine.

Invariantul pe faptele scenelor ucide mutanții de origine lipsă, document
străin, referință inițială diferită și ordinal inițial greșit. La final,
purja trebuie să lase zero postări și zero origini.

Validare P: integrala `run-verificari/20261012-150800-674/`, 3.756 bugetar /
5.158 privat OK, zero FAIL, mutanții existenți uciși, zero
postări și origini reziduale. HTTP privat:
`run-verificari/20261012-150627-604/`, matrice327/327 și SC-D9B-10 verde.

Review-ul independent a găsit lipsa mutantului pentru originea fără suport
și ramura de nul a citirii vechi complete sub rol restrictiv. Proba compară
acum seturile complete de origini A/B, cu factura vizibilă 70 ca martor
pozitiv; resturile 40/70 sunt așteptări independente pe B. Comparația
restului complet A/B la volum acoperă sistemul și adminul securizat.
Gardianul și excepțiile lui sunt neschimbate.

Integrala finală `run-verificari/20261012-161454-010/` are **3.758 bugetar /
5.160 privat OK**, zero FAIL. Mutantul `ORIGINE-FARA-SUPORT`, SC-D9B-10
revizuit, `RAMURA-NUL` și curățenia sunt verzi pe ambele profiluri.
Corecțiile și comparația A/B sunt în [pasul 5](../tr-d9b-pas5-comparatie.md).
Reverificarea Claude a corectat recunoașterea ușii de sistem (D9B-R3);
integrala de după, `run-verificari/20261012-201158-086/`, are **3.759 bugetar /
5.161 privat OK**, zero FAIL, iar HTTP 327/327 cu SC-D9B-10 verde este în
`run-verificari/20261012-205322-837/`. Nu s-a făcut commit; acceptarea
owner-ului rămâne înaintea lui.
Măsurătoarea A și condițiile reproducerii sunt în
[raportul pasului 3](../tr-d9b-pas3-referinta.md).
