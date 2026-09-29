# TR-D8 SAF-T S3 — review advers Codex

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
