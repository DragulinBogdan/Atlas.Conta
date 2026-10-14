# TR-D9b — reluarea pe privat și primul pas: originea partidei

Tip: analiza
Data: 2026-10-12
Alimentează: contractul TR-D9b; 090, 110 (k), 111 (aprobarea), 118-r7 și 118-r8

Owner-ul a aprobat revenirea la TR-D9b după închiderea conceptuală a
[Q1–Q4 pentru 111(f)](2026-10-12-111f-q3-q4-provenienta-si-limite.md).
Această notă pregătește contractul și delimitează primul pas executabil;
nu este contract aprobat sau raport de implementare.

## 1. Punctul de pornire

Nu există încă un contract propriu TR-D9b. Există
[măsurătorile pasului 0](../nucleu/tr-d9b-pas0-masuratori.md), deciziile și
restanțele transferate din TR-D9a. Formularea din conversație „reluăm
contractul” înseamnă aici pregătirea lui, nu reluarea unei implementări.

[111](../decizii/111-repartitor-unic-si-registre-tipate.md) a separat
coordonatele în C111, deja închisă. TR-D9b se scrie pe postarea cu repartitor
unic; nu repetă acel pas. Aprobarea 111 cere explicit originea scrisă a
partidei și preia criteriul de formă F27-r16/F28-r1.

Ținta largă rămâne cea din
[110](../decizii/110-taierea-cubul-singurul-scriitor.md): unitățile,
corecțiile valorice cu `Atribuit`, valuta, unitatea de evaluare și costul
vamal pe lot. Nu devin toate condiții de închidere a primului pas.
Redenumirile `Unitate` → `UnitateSold` și `Latura` → `Sens` rămân în
TR-D9b, împreună, conform
[118](../decizii/118-guvernanta-documentatiei.md); contractul le separă de
schimbarea comportamentului pentru a putea verifica diferențele.

## 2. Primul rezultat propus

**Originea partidei se scrie la nașterea ei și se citește direct.**
Utilizatorul vede aceeași factură, același rest și aceeași ordine de
stingere; citirea nu mai caută documentul deschizător în toate postările
partidei. Beneficiul este propriu fluxului privat de facturi și plăți.

Sursele consultate prin CodeGraph, pe baza `96ccc5a`:

- `Cub/Citiri/Partide.cs`: `Origini` recunoaște originea din postări și din
  identitatea calculată; `CuOrigine`, `PropriiAle` și `MiscariPePartidele`
  folosesc originea la citire.
- `Proiectii/ImperecheriProiectii.cs`: `PartideCuRest` leagă soldurile de
  origine, apoi de document și de etichetele contului și repartitorului.

Acestea sunt constatări de sursă, nu măsurători noi. Cifrele istorice
2,9 s → 0,8 s la 651.644 de partide din aprobarea 111 justifică direcția,
dar nu sunt rezultatul codului curent și nu certifică performanța viitoare.

## 3. Delimitarea de pus în contract

1. Originea descrie identitatea și proveniența; nu persistă încă un sold.
   Restul rămâne suma din cub, inclusiv transferurile pe unitate.
2. Se păstrează identitatea actuală a partidei pe document, cont și
   titular. Originea explicită nu schimbă partida în „o partidă pe fiecare
   poziție de factură”. Alocările procedurale pe poziții din 111(f) au alt rol.
3. Deschiderea fără document rămâne legitimă. Nu se inventează facturi;
   data de naștere și ordinea de stingere rămân cele declarate de sursă.
4. Stingerea, transferul și stornoul nu devin origini noi ale partidei
   numite. Originea nu se mută la ultimul document care a atins soldul.
5. Scrierea originii și a efectelor comenzii este atomică. Retragerea,
   reoperarea și refuzul se probează explicit, inclusiv absența originilor
   active fără suport și a dublurilor. Regula exactă de păstrare/ștergere
   și cheile străine se fixează după inventarierea producătorilor.
6. Citirea securizată păstrează regulile curente: originea nu dezvăluie
   documente sau etichete ascunse și nu ocolește filtrarea postărilor.
7. Prima livrare nu adaugă angajamente, ordonanțări sau legături
   procedurale pe factura și plata private. Nici TVA-ul, nici valuta deja
   existente nu sunt restrânse de convențiile numerice ale probelor noi.

Verificarea contra [invarianților](../invarianti.md): I păstrează documentul
și excepția deschiderii; II și IV cer un contract explicit de producere,
fără ramificare pe profil; III păstrează cubul ca sursă a soldului și
granița perioadei. V și VI nu cer schimbări. Nu este necesară amendarea
invarianților pentru această țintă; forma fizică trebuie să respecte aceeași
demarcație.

## 4. Așteptări numerice pentru primul pas

Sunt propuneri pentru catalog, nu scenarii deja implementate sau rulate.
Fixture-urile intră prin documentele reale și comenzile produsului.

| Caz | Fapte | Așteptare |
|---|---|---|
| Factură și plată nominalizată | factură 100; plată 40 | rest 60; originea partidei este factura |
| Două facturi ale aceluiași titular | A = 100, B = 70; plata 40 numește A | A = 60, B = 70; două origini distincte |
| Împerechere ulterioară | factură 100, plată liberă 40, apoi transfer de 40 | factura rămâne cu 60, partida proprie a plății cu 0; transferul nu schimbă originile și nu mărește rulajele pe cont |
| Deschidere | partidă inițială 100 fără document; plată 40 | rest 60; originea nu capătă factura fictivă sau plata drept document deschizător |
| Retragere și reoperare | factura 100; plata nominalizată 40, retrasă și reoperată | resturile succesive 60 → 100 → 60; fără dublarea originii |
| Storno | factură 100 fără dependenți, apoi storno integral | rest 0; originea istorică rămâne identificabilă |
| Perioadă și securitate | aceleași sume înainte/după închidere; rol cu vizibilitate restrânsă | soldul permis nu se schimbă prin snapshot; originea nu extinde vizibilitatea rolului |

Catalogul existent rămâne proba regresiei pentru celelalte forme de
document, sensuri și monede. Cifrele se adaptează conturilor profilului,
nu se deduc din rezultatul implementării.

## 5. Ordinea de execuție propusă

1. Scrierea contractului TR-D9b, cu inventarul producătorilor și
   consumatorilor unității de sold. Primul pachet închis este originea
   partidei; restul țintelor au etape și criterii proprii. Pentru origine
   trebuie acoperite declararea, deschiderea, materializarea, retragerea,
   stornoul, snapshot-ul și citirea securizată. Se fixează forma fizică,
   identitatea, cardinalitatea și ciclul de viață înaintea codului.
2. Redenumirile aprobate de 118, într-un pas mecanic verificabil, separat
   de persistență și de modificările cititorilor.
3. Probele numerice și termenul A de performanță pe forma C111; scrierea
   originii și mutarea cititorilor; termenul B pe aceeași bază și aceleași
   date. Se măsoară și scrierea, nu doar citirea accelerată.
4. Închiderea pasului cu catalogul pe ambele profiluri, forma SQL,
   verificările de securitate afectate și review advers. Abia apoi se
   detaliază următorul comportament privat din TR-D9b.

Performanța cere atât forma interogării (originea nu se redescoperă prin
parcurgerea postărilor), cât și rezultate A/B pe aceeași bază, cu citirea
integrală și pagina separat. Pragurile se fixează în contract înaintea
implementării; nu se aleg după rezultatul B. Nu se închide F27-r16/F28-r1
doar pentru că o pagină mică răspunde repede.

Validările de produs vor folosi
[rețeta comună](../stare-curenta/verificare.md), prin
`nou/tools/ModelCheck/scripts/verifica.ps1`: scenariile afectate, nucleul
dacă este atins, integrala pe ambele profiluri și infrastructura relevantă.
Sufix propus: `.CodexD9bOrigine`; bazele și compatibilitatea schemei se
verifică înaintea rulării. Comenzile exacte pe scene se fixează în contract
după inventar. Import1C nu este gate și nu se scrie în Flax.

## 6. Ce rămâne deschis

Contractul fizic și review-ul lui; pragurile de performanță; împărțirea
restului TR-D9b în pași cu beneficii private distincte. Originea explicită
nu închide automat reevaluarea, costul vamal, valuta, restanțele de
stingere sau raportul soldurilor fără unitate. Aprobările Q1–Q4 rămân
păstrate pentru etapa procedurală, fără a bloca acest traseu.

Această reluare schimbă numai documentația. Nu au fost rulate scenarii de
produs, nu s-au accesat baze și nu s-au schimbat codul, schema sau invarianții.

La cererea ulterioară „pornește primul pas”, contractul a fost redactat în
[TR-D9b](../nucleu/tr-d9b-contract.md), împreună cu
[inventarul de sursă](../nucleu/tr-d9b-pas1-inventar.md). Acesta este pasul
documentar de pornire; forma fizică și pragurile sunt propuneri concrete
pentru execuție, iar implementarea și review-ul rămân distincte.
