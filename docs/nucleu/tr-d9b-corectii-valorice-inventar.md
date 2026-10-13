# TR-D9b-CV — inventarul corecțiilor valorice pe lot

- Tip: analiza
- Data: 2026-10-12
- Alimentează: [contractul TR-D9b-CV](tr-d9b-corectii-valorice-contract.md)

Inventarul prin CodeGraph identifică primitivele existente și diferența față
de comportamentul cerut. Baza Git este
`96ccc5acbb4048d7ba41f60924863d631e83daca`, cu modificările locale ale
pachetului originii partidei, inclusiv redenumirile `UnitateSold`/`Sens`.
Acesta este un inventar de sursă, nu rezultatul executării corecțiilor.
Nu s-au citit baze și nu s-a reluat importul.

În tabel, `Module/` înseamnă
`nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/`, iar `Nucleu/`
înseamnă `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/`.

## Ținta deja decisă

- Invarianții III și VI, reconciliați prin 090: postări noi de valoare;
  ieșirea operată nu se recalculează, prețul de intrare nu se rescrie.
- 090 (j), (k): repartizare proporțională, Hamilton ierarhic, fără valoare
  rămasă pe stocul epuizat; partea consumată primește ajustare distinctă.
- 091 (h) precizează stornoul din 090 (i): inversarea originalului și
  compensarea efectelor active sunt componente distincte ale aceleiași
  comenzi atomice. SC-X-05/06/07 fixează ordinea operațiilor.
- 110 (k) mută acest mecanism în TR-D9b. C111/111 fixează forma curentă:
  un repartitor pe postare, lot pe capătul intern, proveniența separată de
  coordonate. Registrele tipate 111 (f) nu sunt prerechizit.

## Surse și consecințe

| Loc verificat | Fapt din sursă | Ce cere felia |
|---|---|---|
| `Nucleu/Masura/Evaluare.cs:4` | ieșirea parțială folosește `cantitate × sold.Net / sold.Cantitate`; ultima ia restul | ieșirile viitoare vor consuma soldul ajustat; cele vechi rămân fapte |
| `Module/Declaratii/DeclarantBonConsum.cs:45` | sold local pe cheia lot/cont/produs/repartitor, actualizat în secvența liniilor; decizie `ValoareIesire` | martorul de consum și explicația pot verifica noua valoare fără algoritm paralel |
| `Nucleu/Masura/Repartizare.cs:4,14` | Hamilton simplu și ierarhic, total semnat, ponderi nenegative; egalitățile păstrează ordinea intrării | refolosire; ordinea beneficiarilor trebuie fixată înaintea apelului |
| `Module/Cub/Citiri/Loturi.cs:56,77,113` | sume pe lot/cont/produs/repartitor; citire directă sau cumul cu snapshot | nu se împarte delta numai după soldul disponibil și nu se însumează cantitatea peste stoc și consum |
| `Module/Cub/Citiri/Loturi.cs:129,161` | gardul intermediar și gardul retragerii judecă **cantitatea**, inclusiv zile ulterioare | o delta cu cantitate zero nu certifică valoarea nenegativă sau absența valorii pe stoc gol |
| `Module/Cub/Materializare.cs:83` | storno selectează cauzatele și postările cu `Atribuit` spre ele, apoi inversează și verifică conservarea | nu există aici repartizarea compensării active din 091 (h) |
| `Nucleu/Cub/Storno.cs:16,57` | inversă exactă, perechi remapate, selecție cu un singur salt prin `Atribuit` | primitivă de inversare, nu motor complet de compensare |
| `nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu.Teste/StornoTeste.cs:144` | `StornoulCuAtribuitDinAltDocumentNuECauzaStrainaDarNuEBalansat` așteaptă `ConservareValoare` | contraexemplu executabil deja prezent: selecția atribuitului nu aduce singură contrapartida corectă |
| `Module/Cub/Materializare.cs:246` | retragerea șterge tranzacțiile proprii Operare/Transfer după gardurile curente | trebuie judecate explicit referințele corecțiilor și dependențele valorice; nu este suficient gardul cantitativ |
| `Module/Motor/MotorOperare.cs:226` | stornoul trece prin stare, dată, perioadă și garduri înaintea materializării | integrarea se face pe aceeași comandă atomică, fără serviciu care scrie separat |
| `Module/Declaratii/Coduri.cs:7` | există `CoduriRefuz`, inclusiv `LOT_LIPSA`, `VALOARE_ZERO`, refuzuri de partidă/suport | noile coduri CV trebuie adăugate aici; nu se afirmă că produsul nu are coduri stabile |
| `Module/Declaratii/DeclarantDvi.cs:39` | declară taxa și baza fiscală; nu alocă valoare pe lot | costul vamal rămâne pachet ulterior, cu același mecanism valoric |
| `Module/BusinessObjects/Documente/Dvi.cs:28,36,41,81` | legături n:m la facturi; `Baza`/`Taxa` sunt valoarea în vamă/TVA; facturile legate trebuie operate | legătura la factură nu identifică alocările de cost pe lot; TVA în vamă nu este taxa vamală de capitalizat |
| `Module/BusinessObjects/Documente/FacturaIntrare.cs:18,68,125,168` | document fiscal primit, scadență; pregătire prin preț × cantitate; gard de cantitate pozitivă; frunza participă la nașterea lotului | producător comercial reutilizabil, dar necesită fel explicit de linie, sursă și pregătire valorică distinctă |
| `Module/Declaratii/DeclarantFacturaIntrare.cs:20,74,103,129` | declară netul și fiscalul, nominalizează partida; pentru stoc tratează linia ca recepție | corecția trebuie distribuită pe beneficiari fără recepție nouă, fără suprimarea tacită a fiscalului |
| `Module/Declaratii/DeclarantNotaContabila.cs` și `Module/BusinessObjects/Documente/NotaContabila.cs` | conturi explicite și nominalizări de partide; nu oferă culegerea sursei și distribuția corecției de lot | NTC nu înlocuiește producătorul și mecanismul lipsă |
| `Module/Declaratii/DeclarantNir.Diferenta.cs` | diferența față de recepție folosește constatarea și contrapartida diferenței | nu este delta comercială asupra datoriei furnizorului |
| `Nucleu/Conservare.cs:28,85,137,160` | Storno admite perechi cu același Sens și valori opuse; cauza străină cere atribuire; Transfer cere sumă zero pe cont/sens; Operare admite valori negative | diminuarea păstrează D/C în roșu; Conservare singură nu certifică forma compensării, corectată înR1 |
| `Module/Cub/Randuri.cs:19,62` | cauza postării și documentul tranzacției sunt stocate distinct; atribuirea și referința inversei sunt persistate | modelul admite cauză diferită de proprietar; după R6 compensarea are cauza și proprietarul corecției, deci stornoul ei nu folosește această libertate |

CodeGraph a precedat și căutarea complementară `rg` pentru
`Atribuit|Reevalu|CorectieValor|CorectiePret` în declaranți, documente și
nucleu. În aceste directoare nu s-a găsit un producător de corecție
valorică pe lot; `Atribuit` apare în modelul postării și în selecția
stornoului. În materializare este folosit și la nominalizarea de partidă
(`Imperecheaza`, linia 201). Nu se poate rezerva global sensul câmpului
exclusiv pentru corecțiile de lot.

## Traseul de extins, fără implementare în acest pas

Document tipat → operand închis cu postările eligibile și efectele active →
repartizare pură → declarație balansată + explicație → materializarea
existentă. Comenzile folosesc ObjectSpace propriu prin `OperareApi`.
Cititorul distribuției trebuie să păstreze ID-urile postărilor, nu doar
soldurile; snapshot-ul agregat nu poate ține loc de proveniență.

Pentru storno: inversa exactă a originalului + compensări balansate,
calculate din alocările persistate rămase active. Simplul `Concat` al
atribuitelor nu rezolvă SC-X-05 și poate include din nou o ajustare deja
neutralizată. Refuzul de conservare existent rămâne protecție, nu se slăbește.

## Contradicții și limite de interpretare

1. Formularea veche „inversul cauzat ∪ atribuit” este incompletă pentru
   comportamentul cerut; 091 (h) stabilește ținta. Testul de nucleu care
   demonstrează nebalansarea nu dovedește că scenariul economic e interzis.
2. `BCS.md` încă descrie istoric regim dual/gardian de registru, deși
   TR-D9a și codul curent folosesc cubul. Nu s-au rescris pasajele neatinse.
3. Limita curentă privind reziduul lotului gol după RLF sau documente
   retroactive nu este rezolvată de simpla definire a acestei felii.
   Contractul delimitează lanțul FCT/NIR/BCS; nu declară închisă întreaga
   reevaluare din TR-D9b.
4. Primitivele verificate anterior sunt reutilizabile; corecția valorică
   și compensarea ei **nu sunt încă verificate ca mecanism de produs**.
   DVI pe loturi începe după această verificare, nu doar după redactare.
5. Prima versiune a contractului CV cita interdicția semnului negativ din
   090 (f). S-D14 din [TR-D7a](tr-d7a-strangler-contract.md) o amendează
   explicit, iar `Conservare.VerificaSemnul` confirmă starea curentă:
   diminuarea în Operare păstrează D/C cu valoare negativă. Contractul și
   SC-CVL-04 au fost corectate, fără schimbarea codului.
6. Propunerea inițială de tip nefiscal CVL este înlocuită de extensia FCT
   în [analiza producătorului și compensării](tr-d9b-corectii-valorice-producator-si-compensare.md).
   Este propunere pentru review, nu producător deja implementat. Matricea
   folosește două salturi pentru definiția familiei; storno A găsește
   compensarea într-un salt, iar storno B înlocuiește selecția atribuitelor
   cu compensarea activă. Câmpurile existente nu implementează gardurile.
7. [Review-ul R1 și inventarul cititorilor](tr-d9b-corectii-valorice-review-r1.md)
   confirmă incompatibilitatea primei matrice cu proveniența persistată,
   explicația și filtrul contabil. Compensarea propusă este acum Operare
   separată în aceeași comandă atomică; stornarea antedatată a familiei
   se refuză înaintea sumei. Exportul SAF-T are blocaje q0/multicont și
   o diferență cauză/proprietar în reconcilierea stocurilor, inclusiv fără
   TVA. Punctul2 nu este gata de acceptare.
8. Owner-ul a fixat la 2026-10-13 apartenența compensării la documentul și
   linia corecției (R6). Diferența cauză/proprietar de la punctul7 dispare
   pentru acest caz; rămân blocajele q0/multicont, separarea perechilor de
   compensare în evenimentul de factură și amendarea lui 090 (a), care cere
   încă exact o tranzacție `Operare` pe document.
9. Tot la 2026-10-13: blocajele SAF-T devin restanța CV-r1, cu refuzurile
   existente păstrate (CV-D9). Recepția este beneficiar la storno ca orice
   consum; selecția atribuitelor ar lua alocarea și inversa ei fără perechi
   (CV-D5, SC-CVL-30). Explicația are contract propriu (CV-D8): ierarhia
   deciziilor din nucleu este închisă, deci felurile noi se adaugă acolo.
10. [Decizia 119](../decizii/119-compensarea-valorica-apartine-corectiei.md), 2026-10-13, amendează 090 (a) și (i):
    compensarea este `Operare` a corecției, iar stornoul beneficiarului
    conține numai inversa lui. Punctele 1 și 8 rămân ca istoric al constatării.

Inventarul nu certifică UI, securitate sau performanță. Nu schimbă schema,
politicile, bazele și rezultatele pachetului originii aflat în review.
