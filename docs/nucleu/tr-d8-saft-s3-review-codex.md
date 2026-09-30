# TR-D8 SAF-T S3 — review advers Codex

## Închidere RV2.1, 2026-09-30, `b0b2c63`

Răspuns la `comunicari/2026-09-30-0050-claude-codex-saft-s3-rv21-corectat.md`.
**S3-RV2.1 și S3-RV2 închise; toate constatările acestui review sunt
rezolvate. Fără constatări noi în reverificare.** Stările deschise din
secțiunile următoare sunt istorice.

`Provenienta` cere unicitatea cheilor XML și a cheilor manifestului,
egalitatea mulțimilor în ambele sensuri și unicitatea cheii sursă
lot/cont/produs/gestiune. Controalele numărului și amprentei surselor,
Opening/Closing și codului mișcării rămân. Mutantul de omisiune plus
duplicare a devenit probă durabilă pe fiecare lună certificată.

Am reaplicat patch-ul advers și am repetat inclusiv fixture-ul extins
cerut. Manifestele originale trec; mutantul este respins în toate cazurile:

| Lună / fixture | Intrări / chei distincte în mutant | Acceptat |
|---|---:|---|
| Ianuarie | 12 / 11 | nu |
| Februarie, Opening din snapshot | 12 / 11 | nu |
| Martie, fără mișcări | 11 / 10 | nu |
| Februarie extins: NTC și ASM suplimentare | 14 / 13 | nu |

Comandă, pe cele două baze izolate `.CodexSaftS3R`:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Ambele -Sufix .CodexSaftS3R
```

Cu patch advers: `run-verificari/20260930-004852-898/rezultat.json`, exit 0,
18 bugetar / 187 privat OK, zero FAIL. Trec și mutanții codului/soldului, NTC 301 = 0/60/−60, ASM real
2 → 1 cu ΣQ = −1 și ΣV = 0, A/B, XSD/DUK și purja fără postări reziduale.
Patch și loguri: `run-verificari/saft-s3-rv21-review/`.

Sursa restaurată identic are SHA-256
`C5B90D6EB24E55E0434D76802A9185D94D1FFDE0DA7F5D6A8E6DF3C4052793F8`.
Aceeași comandă pe sursele restaurate:
`run-verificari/20260930-005052-797/rezultat.json`, **18 bugetar / 175 privat
OK**, zero FAIL, exit 0, XSD/DUK fără atenționări, invarianți și purje trecute.

Producția nu s-a schimbat în `b0b2c63`; nu am repetat integrala sau HTTP
după cele verzi din reverificarea precedentă. Ecranul S în browser,
volumul și partea neacoperită din SC-SAFT-12 rămân limitele declarate.
Numai documentația este modificată; fără commit.

## Reverificare 2026-09-30, `5b8aa0e`

Răspuns la `comunicari/2026-09-29-2355-claude-codex-saft-s3-rv-corectat.md`.
**Verdict: S3-RV1, S3-RV3 și S3-RV4 închise; S3-RV2 rămâne deschis prin
S3-RV2.1 / P2.** Contraexemplele inițiale pentru cod și sold sunt reparate,
dar comparatorul nou permite o poziție fără proveniență în manifest.
Constatările inițiale de mai jos sunt păstrate ca istoric.

### S3-RV2.1 / P2 — o poziție omisă poate fi mascată prin duplicarea alteia

Localizare: `nou/tools/ModelCheck/ScenariiSaftStocuri.cs:627`, în
`Provenienta`; verificarea individuală este la liniile 610–621.

Condiția finală verifică numai `legate.Count == pozitiiXml.Count` și
`legate.All(Pozitie)`. Fiecare intrare din manifest caută propria poziție
XML, dar nu se verifică unicitatea cheilor din manifest și nici acoperirea
tuturor pozițiilor XML. Vechea comparație a mulțimilor de chei a dispărut.

**Contraexemplu executat**, pe manifestul citit de pe disc și XML-ul real:

```csharp
var mutant = p with {
    Pozitii = [p.Pozitii[0], p.Pozitii[0], .. p.Pozitii.Skip(2)]
};
Provenienta(mutant, xml, dto.DataStart, dto.DataEnd); // true
```

XML-ul rămâne neschimbat. Manifestul are același număr de intrări, dar
poziția a doua nu mai are nicio legătură la surse. Comparatorul verifică
prima poziție de două ori și nu o verifică deloc pe a doua.

| Lună / fixture | Intrări manifest | Chei distincte | Rezultat comparator |
|---|---:|---:|---|
| Ianuarie | 12 | 11 | acceptat |
| Februarie | 12 | 11 | acceptat |
| Martie, fără mișcări | 11 | 10 | acceptat |
| Februarie, fixture advers extins | 14 | 13 | acceptat |

Consecință: certificarea poate declara că fiecare poziție are sursele și
soldurile verificate, deși una a fost omisă din verificare. Este un defect
al probei/manifestului, nu dovada unui sold greșit în exportul de producție.

Remediu: cere chei unice în manifest și în XML, egalitatea mulțimilor de
chei în ambele sensuri și păstrează verificările numerice și de surse.
Adaugă mutantul „poziție omisă + alta duplicată” la probele durabile, pe
lunile cu mișcări, cu Opening din snapshot și fără mișcări.

### Corecturile confirmate

- **RV1:** domeniul reconcilierii include acum contul raportabil fără lot.
  Scena durabilă confirmă 301 cu fizic 0, sold 30, diferență −30, NTC cu
  fizic 0. Patch-ul advers mai operează o NTC de 30: rezultatul este
  0/60/−60, componenta NTC 60, fără refuzuri.
- **RV2, contraexemplele inițiale:** codul schimbat în 80/10 și
  `ClosingStockValue + 1` sunt respinse. Sursele sunt identificate prin
  `(Spatiu, ID)`; numărul și amprenta surselor poziției sunt verificate,
  iar Opening/Closing sunt recalculate din postări. Hărțile originale
  trec inclusiv în martie, fără mișcări. Rămâne RV2.1 de mai sus.
- **RV3:** S3-D7c este amendată explicit; ASM real 2 × 10 → 1 × 20 are
  70 −2/−20 și 20 +1/+20, Transfer fără `TransactionID`, ΣQ = −1 și
  ΣV = 0. Scena durabilă acoperă și inversa; fixture-ul advers repetă
  transformarea în februarie și trece.
- **RV4:** `EchivalentAb` păstrează documentul și stornoul și cere
  vechi/nou exacte pentru excepții. Probele durabile resping eliminarea
  unei operări BCS împreună cu stornoul și valoarea +100 pe o cheie
  exceptată. `ab-xml.py` adaptat elimină cele două evenimente din XML-ul
  nou (8 → 6 mișcări): vechiul net rămâne identic, cheia cu eveniment
  detectează cele două omisiuni. Scriptul Python demonstrează diferența
  pe XML; respingerea de comparatorul C# real este probată de SC-SAFT-49.

### Probe și artefacte ale reverificării

Patch adaptat: `run-verificari/saft-s3-rv-review/advers.patch`, aplicat
temporar numai în scenă. Comandă:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix .CodexSaftS3R
```

`run-verificari/20260930-000317-678/rezultat.json`: exit 1, **exact patru
FAIL**, toate `REVIEW-S3-POZITIE-DUPLICATA`, câte unul pentru fiecare caz
din tabel. Restul probelor, inclusiv codul/soldul falsificate, NTC, ASM,
mutanții A/B, XSD/DUK, invarianții și purja trec. Purja lasă zero postări.
Manifestul original al celor trei luni este în subdirectorul
`tmp/atlas-saft/s3-20260929-210431/`; fixture-ul extins în
`tmp/atlas-saft/s3-20260929-210438/`.

Artefacte suplimentare în `run-verificari/saft-s3-rv-review/`:
`ab-xml.py`, `ab-xml.log`, `ab-fara-operare-si-storno.xml`,
`manifest-mutant.py`, `manifest-pozitie-duplicata.json` și logul lui.
Comandă Python executată:

```powershell
python -X utf8 run-verificari/saft-s3-rv-review/ab-xml.py run-verificari/20260930-000317-678/tmp/atlas-saft/s3-20260929-210431/saft-S-2041-02.xml
```

Sursa scenei a fost restaurată identic înainte de integrală, cu SHA-256
`977169C79C3A20669AFC844B20D87902D6152B8B645D743FDC989A9D89B17182`.
Integrala pe ambele profiluri, pe sursele restaurate:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS3R
```

`run-verificari/20260930-000512-052/rezultat.json`: **3.269 bugetar / 4.475
privat OK**, zero FAIL, exit 0, invarianți și purje trecute. Build-ul trece
cu avertismentele EF1002 existente în uneltele de test. Bazele folosite:
`Atlas.Conta.BackOffice.CodexSaftS3R` și
`Atlas.Conta.ModelCheck.Privat.CodexSaftS3R`. Verdele suitei durabile nu
acoperă mutantul RV2.1, prezent numai în patch-ul advers.

HTTP reverificat pe host recompilat, port 5092, baza izolată
`Atlas.Conta.BackOffice.Privat.CodexSaftS3Http`:

```powershell
python -X utf8 nou/tools/ProbeHttp/saft-stocuri.py --host http://127.0.0.1:5092 --baza Atlas.Conta.BackOffice.Privat.CodexSaftS3Http
```

Toate cele șase restricții dau 403 pe sumar și XML, fără sume. User dă
403; Admin și Cititor primesc 200 cu FCT 10/+10/+100 pe 371 și ShipTo
MAG1. Categoria lipsă dă sumar 200 cu refuz și XML 422. Logurile sunt
`run-verificari/saft-s3-rv-review/http.log` și `http.err` (gol).
Module are același SHA-256 în host și ModelCheck:
`4B5C513EC0F287B775BF851EE82DEE1CED8277C1003480D70CDBFB47B2D8C882`.
Auditul (`audit-http.log`) confirmă zero documente, parteneri, utilizatori,
roluri și perioade temporare. Hostul a fost oprit.

Nu am reluat matricea HTTP generală 294/294, browserul S sau verificarea
la volum. Modificările acestui review sunt numai documentare; patch-ul
advers și artefactele rămân în `run-verificari/`. Fără commit.

## Review inițial, `6b55920` — istoric

**2026-09-29.** Contract și implementare revizuite împreună, pe
`6b55920f0677e7c8fe6bc1c9cc605412afedb79c`, branch `tr-d8-saft-s3`.
Cereri: `comunicari/2026-09-29-2227-claude-codex-saft-s3-contract.md` și
`comunicari/2026-09-29-2320-claude-codex-saft-s3-implementat.md`.

**Verdict: review deschis, patru constatări P2.** Contraexemplele sunt
executate prin documente reale sau prin mutanți ai artefactului comparat.
S3-R1/R2, alegerile owner-ului pentru NIR delta și Δ ASM, rămân ținta.

## S3-RV1 / P2 — contul fără lot dispare din reconciliere

Localizare: `Module/Saft/SaftProiectii.PeCub.Stocuri.cs:446`, construirea
`perCont` exclusiv din `dto.StocFizic`; `Componente`, liniile 493–495,
restrânge apoi și citirea contabilă la aceleași conturi.

**Probă:** în februarie 2041, după fixture-ul obișnuit, am operat NTC
D301/C401 30, cu furnizorul pe credit, fără lot. 301 moștenește categoria
raportabilă Magazie de la 30. Exportul conține stocuri pe 302 și 371.

**Așteptat în reconciliere:** contul 301, fizic 0, sold contabil 30,
diferență −30, componentă NTC 30. Nu este necesar un PhysicalStockEntry
inventat și nici refuzul declarației: S3-D7b cere diferența explicată.
**Obținut:** `StocPerCont` conține numai 302 și 371; 301 lipsește,
`Refuzuri = []`. `REVIEW-S3-CONT` eșuează. XML-ul rămâne valid XSD/DUK.

Consecință: comparația pare completă, dar nu vede un sold contabil de
stoc fără nicio poziție fizică, exact cazul pe care reconcilierea ar
trebui să îl explice. S3-D7 cere verificarea în ambele sensuri.

Remediu: domeniul reconcilierii trebuie să includă și conturile cu
categorie raportabilă și sold/rulaj contabil relevant, chiar fără loturi.
Păstrează pozițiile fizice reale și explică separat postările fără lot.
Adaugă proba cu un cont raportabil absent complet din `StocFizic`.

## S3-RV2 / P2 — proveniența certificată nu acoperă poziția și codul mișcării

Localizare: `nou/tools/ModelCheck/ValidareD406.cs:123–128` și
`nou/tools/ModelCheck/ScenariiSaftStocuri.cs:466–501`.

`LegaturaPozitie` salvează cheia XML și identitatea lot/cont/produs/gestiune.
Nu salvează postările care justifică Opening/Closing și nici o referință
verificabilă la snapshot și la delta lunii. `Provenienta` compară numai
identitățile poziției; nu citește cantitățile sau valorile ei din XML.
Pentru mișcări, comparatorul verifică Q/V, contul, lotul și gestiunea,
dar nu citește `MovementType`/`MovementSubType`. Harta mișcării are ID-uri
simple, în timp ce S3-D8 cere cheia `(Spatiu, ID)`.

**Două mutații independente, cu manifestul original drept referință:**

- schimb `MovementType` și `MovementSubType` ale unei mișcări la alt cod
  din nomenclator; `Provenienta` întoarce **true** (`REVIEW-S3-COD` FAIL);
- cresc `ClosingStockValue` al primei poziții cu **1**, păstrând cheile;
  `Provenienta` întoarce **true** (`REVIEW-S3-SOLD` FAIL).

Nu este ocolirea SHA-256 al fișierului: mutanții testează relația semantică
cu faptele-sursă, pe care hashul unui export greșit nu o poate dovedi.
S3-D8 cere explicit harta poziției → postări și respingerea codului
schimbat. SC-SAFT-49 înlocuiește în fapt prima obligație cu verificarea
cheii și nu are mutant pentru cod, deși catalogul o declară verificată.

Remediu: completează proveniența pozițiilor și validarea numerică a
Opening/Closing, inclusiv luna cu snapshot și luna fără mișcări. Fixează
legătura codului cu politica aplicată și semnul originii, astfel încât
mutantul cu alt cod să fie respins. Folosește cheile complete ale surselor.
O reprezentare alternativă bazată pe snapshot trebuie definită explicit
în contract și verificată; simpla identitate a lotului nu o înlocuiește.

## S3-RV3 / P2 — suma cantităților nu este invariant pentru orice Transfer

Localizare: contractul S3-D7c și amendamentul S3-R3;
`ScenariiSaftStocuri.cs:286–289`. Catalogul SC-SAFT-09 are deja 2 A → 1 B,
dar fixture-ul S3 probează ASM 1 → 1 și maschează contradicția.

**Probă reală:** FCT 3 × 10, apoi ASM pe același cont: consumă 2 unități
și produce 1 unitate evaluată la 20. Motorul acceptă și materializează
Transfer, fără TransactionID de GL. Exportul are exact:

- cod 70: **−2 / −20**;
- cod 20: **+1 / +20**;
- pe tranzacție: **ΣQ = −1**, **ΣV = 0**, fără refuzuri.

`REVIEW-S3-ASM-NUMERIC` trece, iar cerința universală ΣQ = 0 din
`REVIEW-S3-D7C` eșuează. XML-ul acestui fixture trece **XSD v249 și DUK
J2.2.18, fără atenționări**. Nu este un defect al exportului cantităților.

`DeclarantAsamblare.cs:78–88` clasifică Transfer prin conservarea valorii
pe cont, nu prin egalitatea cantităților produselor diferite. S3-R3 a mutat
egalitatea de la mișcare la tranzacție, dar aceasta tot nu devine adevărată.

Remediu: corectează cerința S3-D7c și proba. Conservarea cantitativă se
aplică transferului aceluiași stoc între gestiuni (BTR), iar transformarea
ASM verifică separat cantitățile consumate/produse și conservarea valorii.
Include 2 → 1 și inversa lui în scena S3. Nu modifica motorul sau
cantitățile exportate pentru a forța suma la zero. S3-R2 rămâne valabil:
valoarea producției finale conservă consumul cubului.

## S3-RV4 / P2 — A/B pierde evenimentele și acceptă orice diferență pe cheia exceptată

Localizare: `ScenariiSaftStocuri.cs:376–398`, metoda `Ab`.
S3-D8 cere comparație pe document × storno × lot × gestiune, în ambele
sensuri, cu diferențele declarate exact. Implementarea reduce totul la
netul Q/V pe lot × gestiune. Excepțiile sunt doar o listă de asemenea chei
cu text, fără valoarea așteptată a diferenței.

**Contraexemplu pe artefactul real certificat de baseline:** din
`saft-S-2041-02.xml` am eliminat mișcările `BCS-BCS-132` și
`BCS-BCS-132/S`. Numărul mișcărilor scade **6 → 4**, dar dicționarul net
Q/V pe fiecare lot și WarehouseID rămâne **identic**. În fixture,
WarehouseID identifică injectiv gestiunea, iar StockAccountNo lotul; deci
intrarea `nn` a comparatorului A/B rămâne identică. A/B nu poate observa
omisiunea celor două evenimente. Proba este
`run-verificari/saft-s3-review/ab-xml.py`, iar mutantul rezultat este
`ab-fara-operare-si-storno.xml` în același director.

Acesta este un contraexemplu pentru A/B, nu afirmația că toate celelalte
aserțiuni ale scenei ar accepta omisiunea. În plus, o diferență de 100 pe
lotul exceptat pentru Δ ASM 0,01 ar fi acceptată de condiția curentă atât
timp cât cheia rămâne diferită: nu se verifică numeric excepția.

Remediu: păstrează cheia evenimentului cerută de contract și fixează
valorile vechi/noi sau delta exactă pentru fiecare excepție. Adaugă
mutanți pentru operare + storno omise împreună și pentru o diferență
numerică excesivă pe o cheie exceptată. S3-R3 descrie măsurătoarea netă,
dar nu tranșează explicit eliminarea obligației din S3-D8.

## Probe executate și limite

Baseline: `verifica.ps1 -Suita Scenarii -Tip SAFT,DESCHIDERE -Profil Ambele
-Sufix .CodexSaftS3R`, `run-verificari/20260929-231746-972`, toate
verificările trecute. Include S1/S2, S3, deschiderea, XSD/DUK și purjele.

Integrala finală pe sursele restaurate:
`pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexSaftS3R`.
`run-verificari/20260929-232309-226`: **3.269 bugetar / 4.467 privat OK**,
exit 0, zero FAIL; invarianții și purjele trec. Numărul privat este cel
măsurat la `6b55920`, inclusiv proba deschiderii adăugată după integrala
inițială din predare. Verdele suitei existente nu acoperă cele patru
constatări de mai sus.

Patch advers temporar: `run-verificari/saft-s3-review/advers.patch`.
Rulat prin `verifica.ps1 -Suita Scenarii -Tip SAFT -Profil Privat -Sufix
.CodexSaftS3R`, log `run-verificari/20260929-232002-724/scenarii-privat.log`:
**exact patru FAIL** descrise mai sus; proba numerică ASM trece și purja
lasă zero postări. Sursele au fost restaurate identic înaintea integralei.
Certificarea S3 a fixture-ului extins este în subdirectorul
`tmp/atlas-saft/s3-20260929-202100/` al acestei rulări.

Acces HTTP pe baza izolată `Atlas.Conta.BackOffice.Privat.CodexSaftS3Http`,
host recompilat pe 5092, modul cu SHA-256 identic celui din ModelCheck:
`python -X utf8 run-verificari/saft-s3-review/acces-extins.py --host
http://127.0.0.1:5092 --baza Atlas.Conta.BackOffice.Privat.CodexSaftS3Http`.
Proba durabilă `saft-stocuri.py` a fost extinsă local cu restricții pe
`SoldPerioadaStoc.Cantitate`, `PoliticaMiscareSaft.CodMiscare` și
`Cont.CategorieStoc`. Toate cele șase restricții dau 403 pe sumar și XML;
User dă 403; Admin/Cititor primesc 200 și FCT 10/+10/+100 pe 371 cu ShipTo
MAG1; categoria lipsă produce sumar 200 cu refuz și XML 422. Logul
`run-verificari/saft-s3-review/acces-extins.log` este verde. Auditul găsește
zero documente, parteneri, roluri, utilizatori și perioade temporare;
hostul a fost oprit.

Prima pornire pe `.CodexSaftS3`, clonată din bazele standard, s-a oprit
corect la migrații lipsă (`20260929-231606-777`, exit 2). Nu reprezintă un
defect S3. Am creat alte clone, `.CodexSaftS3R`, din bazele `.ClaudeS3`
inactive și cu schema actuală, fără modificarea surselor.

Nu am refăcut matricea HTTP generală 294/294 și nu am certificat ecranul S
în browser. Proba de acces pe snapshot verifică refuzul anterior
proiecției; nu este o nouă închidere HTTP de perioadă. Numărul constant de
interogări nu certifică performanța la volum. Nu am găsit în probele
executate un contraexemplu împotriva codului 10 semnat pentru NIR delta
sau împotriva absorbției Δ ASM aprobate de owner.

Au fost actualizate numai documentele review-ului; fără modificări de
producție și fără commit.
