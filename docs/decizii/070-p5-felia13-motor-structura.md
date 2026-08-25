# Decizia 70 — Pasul 5, felia 13 — motor/structură post-D300 (69-r4/r7/r5 + 67e)

- **Data**: 2026-08-25
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §70
- **Docs**: docs/api/p5-felia-motor-structura-contract.md (F13-D1…D6 + §Închidere)

---

**Pasul 5, felia 13 — executată** (contract + închidere:
`docs/api/p5-felia-motor-structura-contract.md`; review advers cu 7 constatări:
0 de fond, 3 medii — toate fixate în cod —, 4 minore — 2 fixate, 2 documentate
ca limite). Felia nu adaugă funcționalitate: închide trei restanțe de MOTOR/
STRUCTURĂ lăsate de D300 (69-r4, 69-r7, 69-r5) și una veche de nomenclator
(67e). Explorarea read-only a precedat contractul; cauzele mecanice sunt
consemnate în contract §Cauzele mecanice.

(a) **Taxarea inversă are SENS; sursa sensului e `PoliticaTva.Directie`**
(69-r4). Regula de fond (Cod fiscal art. 331): furnizorul emite factura FĂRĂ
TVA; beneficiarul autolichidează. `TvaService.CalculeazaValori` primește
`DirectieTva? directie` (parametru EXPLICIT, fără default — un apelant nou
trebuie să se întrebe pe ce latură e, nu să moștenească tăcut achiziția):
`TaxareInversa × Colectat` ⇒ `Valoare = net; ValoareTva = 0`; `× Deductibil` ⇒
autolichidare ca înainte; `directie == null` (tip fără `PoliticaTva`) ⇒
comportamentul vechi, care oricum nu postează. Sensul NU e câmp pe `TipTva` și
NU e hook pe frunză: e deja proprietatea tipului de document (36b). Toți cei
9 apelanți (5 `PregatesteOperare`: FCT/FCL/DEC/RLF/RDC; 3 Apply; controllerul
XAF de culegere) trec direcția, rezolvată O DATĂ per document
(`TvaService.DirectiePentru`). Ramura TI din pasul contabil al motorului
(`MotorOperare`) cere `Directie == Deductibil` ca gard EXPLICIT — o valoare
intrată pe altă cale n-are voie să reînvie 4426 = 4427. RLF (Deductibil)
rămâne autolichidare; RDC (Colectat) nu stornează o taxă inexistentă.

(b) **Gardul care strigă** (62f): TVA nenul pe o linie TI × Colectat =
refuz de domeniu în DOUĂ locuri, aceeași propoziție: în motor
(`VerificaTvaCulesTaxareInversa`, chemat din `CalculeazaSiValideaza` — acoperă
`Opereaza` și dry-run-ul `Valideaza`, calea XAF și API deodată, 42a) și la PUT
în `FacturaIesireApply` (review, defect 1: altfel draftul salvat cu TVA pe o
linie TI ar minți în ReadDto până la operare). Subtilitatea de ordine:
`PregatesteOperare` aduce `ValoareTva` la 0 ÎNAINTE de validare, deci gardul
capturează valorile culese înaintea pregătirii și citește `TipTvaId` după
(RDC își golește deliberat identitatea fiscală pe liniile de cost). Mesajul
spune „poartă TVA", nu „are TVA cules" — pe o factură operată de vechiul motor
valoarea a pus-o motorul, nu operatorul, și gardul nu le distinge.

(c) **Consecințe în raportare**: `RegistruTvaService` NESCHIMBAT (citește
`ValoareTva`, acum 0 nativ; rândul FCL-TI apare în jurnal cu bază și TVA 0 —
68, „liniile fără TVA apar legal"). Excepția „TI pe livrare" din
`D300Proiectii` a DISPĂRUT odată cu cauza; D3-V3 e rescris pe premisa nouă
(`pierdutTva == 0` nativ). Import1C: `tipTva = null` pe liniile FCL-TI
RĂMÂNE, cu motivul rescris — „ca în sursă" (1C nu scrie rând de TVA), nu „motorul
inventează 4426 = 4427"; păstrarea lui TI pe linie ar muta baza pe rd. 13 al
D300 — decizie de import, nu a feliei.

(d) **Datele de dinainte de felie NU se migrează; se RAPORTEAZĂ** (review,
defect 2). O FCL-TI operată pre-F13 are `ValoareTva ≠ 0`, 4426 = 4427 în
registru și TVA pe rândul fiscal; D300 o arată acum ca avertisment de TVA
pierdut, anularea + re-operarea o refuză (gardul din (b)), storno-ul e corect
(inversează din registru). ModelCheck poartă un inventar
(`MĂSURAT (F13-D1, date pre-F13)`), zero pe bazele de probă; pe o bază reală
cifra e a utilizatorului (anulare + golire + re-operare, sau storno). Baza de
import n-are astfel de linii (Import1C nu trece TI pe FCL).

(e) **ModelCheck șterge ca host-ul** (69-r7). `UseDeferredDeletion` are DOUĂ
suprasarcini care nu se substituie: pe `ModelBuilder` (filtrul global
`GCRecord = 0`, rulează oricum din `OnModelCreating`) și pe
`DbContextOptionsBuilder` (interceptorul care traduce `Deleted` în
`UPDATE … GCRecord = 1`) — a doua lipsea din harness. Acum e pe ambele
builder-e; restul lui `AddEFCore` NU s-a adus (deliberat — D2 e despre
ștergere). **Curățenia de scenă = infrastructură, nu probă ⇒ purjă FIZICĂ**
prin `Purja` (SQL brut pe Id-uri, rădăcina TPT + cascadele DB, dependenții
`NO ACTION` explicit, reluare în pase pentru `RESTRICT`, eșec zgomotos dacă
ceva din afara scenei referă; `IgnoreQueryFilters` la culegere, ca reziduul
rulărilor anterioare — PK-urile deterministe ale fișei — să se auto-vindece).
`os.Delete` rămâne DOAR unde ștergerea logică e obiectul probei: F5 (69b —
maparea D300 ștearsă prin OS rămâne în tabelă cu `GCRecord = 1` și NU reînvie
la re-seed) și 57f (MĂSURAT: la `Committing`, `IsObjectToDelete` e adevărat
iar `IsDeletedObject` încă fals — `GCRecord` îl pune interceptorul abia în
`SavingChanges` — deci `EsteSters` din gardian e corect; întrebarea nu se
putea pune cât ștergerea era fizică). Baza ModelCheck acumulează `GCRecord = 1`
între rulări — acceptat; orice SQL brut care CITEȘTE pune `GCRecord = 0`
explicit (66). Excepție păstrată deliberat: scena D4 a balanței simulează
încă prin `UPDATE` invizibilitatea contului prin SECURITATE, pe care harness-ul
n-o are. Defect de harness găsit pe drum: proba DIM-3 (mapare plată) lăsa o
`RegulaContare` goală ștearsă logic PER RULARE, culeasă la rularea următoare de
`StergeReguliContareStricate` — purjează acum fizic; nu era scurgere de
producție.

(f) **Un singur 400 pe sârmă: `EroriDto`** (69-r5).
`InvalidModelStateResponseFactory` traduce `ModelState` în `EroriDto` —
„{câmp}: {mesaj}", câmpul gol și rădăcina JSON `$` omise; mesajul lipsă (excepție
de server) ⇒ „Valoare invalidă." (nu scurge detalii). Statusul rămâne 400 (422 e
al comenzilor). Declarat pe `ContaApiController` (Swashbuckle deduplică pe
status × tip, atributele per acțiune rămân) ȘI pe `AuthenticationController`,
fiindcă fabrica e globală pe toate controllerele `[ApiController]`. OData
(`DataController : ODataController`, fără `[ApiController]`) NU e atins.
`nucleu/http.ts` nu s-a schimbat funcțional (trata deja `Erori[]` pe 400);
ramura generică rămâne plasă pentru non-JSON. Probat pe HTTP: dată malformată,
JSON invalid, câmp netipizabil ⇒ `400 {Erori}`; GUID malformat pe rută cu
`:guid` ⇒ 404 (rutare, nu binding — documentat, nu „reparat").

(g) **Gardian de ciclu pe `Cont.Parinte`** (67e) în `GardianEditare`: lanțul
de părinți se urcă prin navigație (sursa primară — părintele poate fi nou în
același commit) cu FK-ul ca plasă, `HashSet` de vizitate, limită 64; ciclu
(prin contul scris sau nu) și lanț mai lung decât limita = refuz cu lanțul în
mesaj. Garda de vizitare din `BalantaPliata` RĂMÂNE (apărare în adâncime).
Probat pe ambele profiluri (ciclu direct, prin doi, două conturi noi în același
commit, lanț legitim, contul șters sărit). **Constatare de contract**: `Cont` e
`ReadOnly` pe OData (F8-D4), deci gardianul e azi doar pe calea XAF; iar
refuzurile gardianului pe scrierile OData ies prin `UserFriendlyExceptionFilter`
al DevExpress ca `400 text/plain`, nu `422 {Erori}` — clauza „OData: 422" din
contract era falsă; vezi (i).

(h) **Proba supremă (D5)**: Import1C `--recreeaza` integral pe codul feliei —
rezultatul se consemnează în contract §Închidere (așteptarea argumentată:
raport identic cu baseline-ul DIM-4 `reconciliere-20260807-174221.txt`,
fiindcă importul nu trece TI pe FCL).

(i) **Rămase, cu nume**: **70-r1** refuzurile gardianului pe scrierile OData ale
nomenclatoarelor vii ies `400 text/plain` (filtrul DX), nu `422 EroriDto` —
un filtru global `IUserFriendlyException → EroriDto` e fixul, dar schimbă
contractul lookup-urilor (`dxStore.ts`) — decizie proprie; **70-r2** smoke-ul
vizual al gardianului de ciclu din ecranul XAF `Cont` (editorul de lookup n-a
răspuns la automatizare; câmpul brut `Parinte Id` vizibil — 41b nu prinde
`Cont`); **70-r3** poziția „linia N" din mesajul gardului e ordinea enumerării
`doc.Detalii`, fără criteriu explicit; **70-r4** `DirectiePentru` = 2 interogări
per `ObjectChanged` pe calea XAF (cache-uibil per view; Apply-urile o rezolvă
o dată per agregat); **70-r5** mesajele de model binding sunt în engleză
(`ModelBindingMessageProvider`, felie proprie); **70-r6** `TvaSuprascris` (56)
rămâne; filtrarea lookup-ului TipTva pe FCL ca afordanță (TI e legitim pe FCL,
deci NU se filtrează — dar UI-ul ar putea explica de ce TVA-ul e 0).
Excluse deliberat din felie (decizii proprii): 64h, 36f, 51e, 63f.
