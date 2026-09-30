# SAF-B8 — corecția B8-RV1.2

Data: 2026-09-30. Implementare și verificare: Codex, la cererea owner-ului.
**B8-RV1.2 este rezolvată; RV1/RV1.1, RV2 și RV3 sunt închise.**
Reverificată de Claude la 2026-09-30, independent de rapoartele de mai jos:
hash-urile sursei și ale DLL-ului, identitatea patch ↔ worktree, 3.269 / 4.521
OK cu zero FAIL numărate în loguri, cele 21 de probe pe privat (0 pe bugetar),
cele 111 rânduri ale raportului identice pe (scenă, profil, lună, secțiune,
clasă) cu raportul anterior. Comisă pe `tr-d8-saft-ab-rv1`. SAFT-r4/r5 și
gate-ul transversal TR-D8 rămân delimitările lor existente.

## Schimbarea

`Clasificator.Factura` verifică ambele ramuri SAF-B5 prin `FacturaDinCub`.
Martorul nu apelează `SaftPeCub` și nu recalculează așteptarea din DTO:

- identifică un singur eveniment Operare/Storno al documentului în luna comparată;
- verifică numărul, data, tipul, contul furnizorului, partenerul și legătura evenimentului;
- brutul este suma semnată a postărilor furnizorului;
- cheile liniilor sunt exact cele din sursă, fără omiteri și duplicate;
- fiecare linie are contul din postări, cantitatea comercială din detaliul real,
  baza și TVA din faptele fiscale ale evenimentului (sau contrapartida nefiscală);
- codul și tipul taxei urmează maparea fiscală; netul și brutul se verifică
  și pe total, inclusiv autocolectarea;
- ramura cu linii adăugate păstrează numărul și contul facturii vechi, precum
  și multisetul liniilor ei; fiecare linie adăugată are postare pe lot.

Valorile vin din `Postare`, faptele fiscale și `DocumentDetaliu`; antetul
(`SaftReguli.InvoiceTypeEveniment`, `SimbolSaft`) și codul/tipul taxei
(`MapariFiscale`) au oracol comun cu producția — maparea rămâne acoperită de
catalogul SAFT.md, nu de acest martor.

Comparația A/B păstrează domeniul semantic documentat. Martorul întărește
clasele SAF-B5; nu transformă A/B într-un comparator integral de XML.
Schema, manifestul și catalogul numeric își păstrează rolurile.

## Probe și rezultat

S-au adăugat **21 de probe adverse**, pe cele două facturi reale care
declanșează ramurile: DES (10) și D16-V2 (11). Sunt respinse:
50 → 500 și 100/TVA 21 → 500/TVA 105 cu totaluri coerente; cantitate +1;
cont de linie, cod de taxă, taxă cu brut coerent, număr, cont și tip de factură
schimbate; linie omisă; duplicare cu totaluri păstrate; redistribuirea lui 1
între liniile D16-V2, păstrând totalul. Exporturile originale trec.
Probele rulează în fiecare lună cu o factură din cele două clase.

Integrala pe starea cu ambele rute:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexB8Rv12 -PregatesteBaze
```

Rezultat: **3.269 / 4.521 OK**, zero FAIL, exit 0.
Manifest: `run-verificari/saft-b8-rv12/20260930-233316-630/rezultat.json`
(rularea copiată din worktree înaintea scoaterii lui; `20260930-233202-864`
alături).
DLL SHA-256: `63F74EC253C44E41539D5248FB94A78A8CBFFEE1C61A21C2FF9DD1EDBE56491F`.

Raportul regenerat are **111 diferențe**, toate clasificate; numărătorile
pe clase sunt identice cu raportul anterior. Pe scene: D16-V2 20,
D17-V2 12, DES 23, SAFT 28, SAFT-S 28. Nicio diferență legitimă nu a fost
eliminată ca să treacă martorul. Auditul este
`run-verificari/saft-b8-rv12/audit.json`, raportul brut `saft-ab-final.md`
din același director; rândurile actualizate sunt în `tr-d8-saft-ab.md`.

Prima lansare (`20260930-233202-864` în worktree) a compilat, apoi s-a oprit
cu exit 2 înaintea scenariilor: clona bugetară din baza nesufixată avea două
migrații lipsă. Numai bazele proprii `.CodexB8Rv12` au fost recreate din
bazele `.CodexSaftS3R`, cu schema verificată; rularea de mai sus este cea finală.

## Surse și predare

Codul este comis pe branch-ul istoric `tr-d8-saft-ab-rv1`, peste `3a4372d`,
fișierul `nou/tools/ModelCheck/SaftAb.cs`, SHA-256 sursă
`5E9069B1D8A3C63E491472BB91375745839D2AE84A48C0D75871491B4BF89968`
(lucrat în worktree-ul `run-verificari/saft-b8-rv12-worktree`, scos după
commit). Patch-ul reproductibil și copia sursei sunt arhivate în
`run-verificari/saft-b8-rv12/corectie.patch`, respectiv `SaftAb.cs`.

Checkout-ul curent `tr-d8-saft-b8` păstrează producția și ModelCheck
neschimbate față de `b3272de`; aici se actualizează raportul și starea
constatărilor. Ruta veche rămâne scoasă din produs. Nu s-au repetat perf,
HTTP sau browserul, fiindcă această schimbare atinge doar comparatorul
istoric. Kitul ANAF din worktree este o copie, fără joncțiune.
