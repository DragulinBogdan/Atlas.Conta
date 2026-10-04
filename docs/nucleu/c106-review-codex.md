# C106 — review advers Codex

Data: 2026-10-04. Cod: `908d3c8` (PR #17), diferența față de `7ee5470`.
Contract: [106](../decizii/106-regim-pe-stare.md), inclusiv (i)/(j);
catalog: [REGIM](scenarii/REGIM.md).

**Verdict inițial: necesită corecturi — patru observații P2.** Trei sunt
confirmate prin comenzile reale; a patra este constatare statică XAF.
Integrala verde nu acoperă contraexemplele. Nu s-au modificat regulile
aprobate sau codul de producție.

Reverificare la `5941b28`: R1…R4 închise tehnic;
[RV1](c106-review-codex-rv1.md) consemnează observația C106-R5 asupra
probei SC-X-24 și delimitarea limitei 106-r7, încă dependentă de aprobarea
owner-ului.

Reverificarea finală la `86978f7`: [RV2](c106-review-codex-rv2.md) închide
și C106-R5; limita 106-r7 este acceptată la comandă prin decizia owner-ului.

În referințele de mai jos, `Module/` înseamnă
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`.

## C106-R1 — P2: împerecherile istorice blochează stornarea/corecția validă

Loc: `Module/Api/RegimDocument.cs:64–69`, `:113–115`;
`Module/Motor/MotorOperare.cs:776–779`. Calea reală diferită:
`Module/Motor/ImperechereService.cs:63–79`, apelată la storno.

Regimul aplică același `Any` peste toate împerecherile la anulare, storno
și corecție. Motorul de storno lucrează numai cu împerecherile vii: cele
din perioada închisă se inversează automat, iar cele deja inversate nu
mai blochează. O comandă validă este astfel retrasă din XAF și din
`Poate*`; motivul cere ștergerea unor împerecheri care nu mai pot fi șterse.

Măsurat pe ambele profiluri în scena REGIM, anul 2013:

- FCT servicii 100, PLT 100, împerechere 100 în ianuarie; după închiderea
  lui ianuarie, regimul refuză `Storneaza` pe PLT, dar stornarea în februarie
  reușește (`C106-ADVERS-R1a`).
- După acea inversare, regimul refuză `Corecteaza` pe FCT din cauza
  istoricului împerecherii; comanda reușește și creează corecția Draft
  (`C106-ADVERS-R1b`). Originalele rămân imuabile.

**Contradicție de contract:** 106(b) spune aceleași dependențe pentru cele
trei comenzi, în timp ce [088(j)](../decizii/088-p5-felia27-perioade-solduri.md)
prevede explicit inversarea împerecherilor închise la storno. Codul și
probele păstrează 088(j); nu se poate declara că predicatele sunt deja
aceleași. Corectura trebuie să tranșeze explicit textul 106(b) și să
extragă predicatul citibil al stornării din mecanismul existent, fără a
restrânge tăcut motorul la regula anulării. Proba permanentă trebuie să
includă împerechere vie în lună deschisă, închisă și deja inversată.

## C106-R2 — P2: lipsesc dependențele și restricția de lună ale imobilizărilor

Loc: `Module/Api/RegimDocument.cs:64–69`, `:76`; gardieni omiși:
`Module/BusinessObjects/Documente/Imobilizari.cs:226–236`.

PIF/CAS nu contribuie aceste refuzuri prin hook, iar hook-ul AMO decide
regenerarea. Regimul generic permite comenzi interzise de dependenții
fișei sau de combinația lună proprie/perioadă închisă. Acestea sunt
gardieni de dependență și perioadă din 106(i), nu validări valorice.

Măsurat pe ambele profiluri în fixture-urile IMO, anul 2019:

- PIF cu AMO ulterioară: `AnuleazaOperarea` și `Storneaza` disponibile în
  regim; ambele comenzi refuzate cu „fapte ulterioare nestornate”
  (`C106-ADVERS-R2b`).
- PIF în ianuarie închis: `Storneaza` și `Corecteaza` disponibile; storno
  în ianuarie refuzat pe perioadă închisă, corecția în februarie refuzată
  pentru că trebuie făcută în luna documentului (`C106-ADVERS-R2a` și
  `SC-IMO-16`). Nu există o altă dată validă de ales.

Amprentele postărilor originale sunt identice după refuzuri. Cere
predicate comune cu gardienii actuali, expuse prin contribuția tipului,
și probe de regim pentru PIF/AMO/CAS; nu `switch` pe frunze în nucleu.

## C106-R3 — P2: anularea rămâne disponibilă după declararea TVA

Loc: `Module/Api/RegimDocument.cs:65–66`. Gardian omis:
`Module/Motor/FiscalitateService.cs:18–24`, apelat de motor la linia 589.

Pe FCT servicii 100 + TVA 21, după confirmarea D300/D394 pentru ianuarie,
**înaintea închiderii lunii**, regimul oferă anularea. Comanda refuză
`TVA_DEJA_DECLARATA`, iar originalele rămân intacte (`C106-ADVERS-R3`,
fixture-ul fiscal privat, anul 2023). Depunerea există deja când se citește
regimul; nu este concurență sau un refuz dependent de suma comenzii.

Refuzul trebuie proiectat din același predicat fiscal folosit de comandă.
Sunt necesare probe separate pentru depunere D300 și D394, cu perioada
înregistrării deschisă, astfel încât garda perioadei să nu mascheze lipsa.
Egalitatea ReadDto ↔ regim nu detectează singură eroarea: ambele expun
același răspuns greșit.

## C106-R4 — P2: dezactivarea controllerului lasă acțiunile comune modificate

Loc: `Module/Controllers/RegimDocumentController.cs:23–27`; scrierea
stării la `:40–43`, inclusiv `DeleteObjectsViewController.DeleteAction`
prin maparea de la `:48–49`.

`OnDeactivated` dezabonează evenimentele, dar nu elimină cheia
`Enabled["Regim"]`, nu restaurează tooltip-urile și nu golește dicționarul.
La reutilizarea aceluiași Frame pentru o vedere care nu este de document,
controllerul de regim nu mai rulează, iar acțiunea comună Delete păstrează
refuzul documentului Operat/Stornat. Dacă următoarea vedere este o listă
de documente, controllerul listei recalculează Enabled, dar memorează ca
tooltip original motivul lăsat de detaliu. Controllerul listei are deja
curățare proprie la dezactivare; aceasta lipsește pe detaliu.

Constatare statică, susținută de ciclul de viață documentat de
[DevExpress — modificarea acțiunilor altor controllere](https://docs.devexpress.com/eXpressAppFramework/112728):
instanțele controllerelor se reutilizează între vederile aceluiași Frame,
iar modificările trebuie retrase în `OnDeactivated`. Nu este prezentată
ca reproducere în browser în acest review.

Cere restaurarea acțiunilor efectiv atinse și o probă în browser:
document Operat → nomenclator în același Frame, apoi document Draft/listă;
disponibilitatea și tooltip-ul inițial trebuie să revină.

## Verificări și limite

Integrala pe sursele nemodificate, bazele `.CodexXReview`:
**3.410 bugetar / 4.620 privat OK**, zero FAIL, exit 0.
Manifest: `run-verificari/20261004-124343-747/rezultat.json`.
Cele două probe MF în plus față de predare au fixture-ul disponibil aici.

Probele adverse au confirmat R1/R2 pe ambele profiluri și R3 pe privat:
`run-verificari/20261004-125150-783/rezultat.json`, exit 0. Ele asertează
**existența contradicției**; `OK C106-ADVERS` confirmă defectul, nu
conformitatea regimului. Patch-ul reproductibil, scriptul de injectare și
copiile originale: `run-verificari/codex-c106-review/`. Cele trei surse
ModelCheck au fost apoi restaurate octet cu octet; codul de producție nu
a fost modificat.

După restaurare, rebuild și REGIM pe ambele profiluri: exit 0, fără FAIL,
manifest `run-verificari/20261004-125440-153/rezultat.json`.

Comenzi:

```powershell
dotnet restore nou/tools/ModelCheck/ModelCheck.csproj --force-evaluate --no-http-cache --nologo
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip REGIM,IMO,FISCALE -Profil Ambele -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip REGIM -Profil Ambele -Sufix .CodexXReview
```

Prima încercare de build a găsit Atlas.DXF 26.1.4.9 în cache și a eșuat pe
`Columns`; restore a adus 26.1.4.10, apoi build-ul și integrala au trecut.
Este o problemă locală de dependență, nu una dintre observațiile C106.

Inspecția celor 18 adaptori Apply nu a găsit o mapare greșită de comandă;
excepția `DscApply.PoateEdita = false` este prevăzută de contract.
`RestNedescarcat` citește liniile FCL/DSC și include drafturile care acoperă
cantitatea: nu reconstruiește un sold contabil din documente. Nu s-a
produs un benchmark nou al costului. Probele structurale Regim/Linii au
trecut în integrală; nu s-a injectat un tip nou în model.

Nu s-au repetat HTTP și browser în acest review. Refuzurile de securitate
și randarea tooltip-ului rămân dovezile predării, iar tranziția de Frame din
R4 trebuie probată la corectură. Nu se redeschid TR-D8 X-RI1…4 sau
restanțele declarate 106-r2/r3/r5/r6. Fără commit.
