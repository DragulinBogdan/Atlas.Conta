# TR-D8 — intervale TVA și avertismente, R6

Data: 2026-09-26
Stare: specificație pentru review, fără cod R6. Regula de produs este 103(h).
Surse: invarianții II/III/IV; decizia 103; review-ul fiscal 103 din 26.09,
§3; scenariile SC-CIT-90…94 din `scenarii/CITIRI.md`.

## Domeniu și comportament

TipTva primește două date opționale, cu limite incluzive. Null înseamnă
interval deschis la acel capăt. Un interval inversat este configurație
malformată; refuzul salvării lui nu este refuz fiscal al documentului.
Seed-urile existente rămân cu interval deschis; nu deducem calendarul legal
din codul ori procentul tipului și nu închidem tipurile automat.

Avertismentul de interval compară `DataExigibilitate` cu intervalul curent.
Pentru faptul scris, data și calificarea sunt cele înghețate în cub. Pentru
draft, sunt cele pe care le-ar folosi operarea, fără scriere de postări.
Editarea pe loc a cotei, regimului sau importului rămâne permisă. Raportul
arată calificarea istorică versus cea curentă; nu recalculează sumele.

Serviciul comun de avertismente întoarce cod, motiv, document/linie,
direcție, stare, tip TVA, repere și valori istorice/curente. Operarea îl
expune în `OperareRezultat.Mesaje`, după calea existentă de informare;
UI și XAF arată mesajele fără a transforma succesul în refuz. Raportul
REST folosește ObjectSpace secured. Pentru operate, măsurile vin exclusiv
din `Cub.Citiri.Fiscale`; pentru drafturi se citește explicit agregatul,
nu se inventează fapte fiscale persistate.

Raportul poate fi deschis după salvarea tipului TVA și oricând ulterior,
filtrat pe tip și perioadă. Nu scanăm toate documentele la fiecare salvare
sau la pornirea hostului. După modificarea politicii, UI indică raportul
de impact. Operatele cu calificare diferită se disting de drafturile care
vor prelua politica curentă la operare. Gruparea este Emise / Primite-Vamă;
remediul este alegerea utilizatorului, nu comanda raportului.

## Regularizarea avansului

Propunere de implementare minimă: `TipMaterial.RegularizareAvans` boolean
și referință opțională `FacturaAvansId` pe liniile fiscale de factură,
expusă motorului printr-un contract de linie. Câmpul nu intră pe baza
generică `DocumentDetaliu`: este culegere specifică, conform invariantului II.
FCT și FCL sunt suprafețele inițiale; retururile nu devin regularizări de
avans prin simplul semn negativ. Aceeași coloană fizică TPH poate fi mapată
pe proprietățile omoloage ale celor două frunze; aceasta trebuie probată
în model înaintea implementării mecanismului.

Marca se configurează pe tip, nu concomitent pe clasă și tip, pentru a
evita încă o regulă de prioritate. Cu marcă și referință validă, comparația
folosește cota/exigibilitatea istorice ale avansului. Data documentului final
și perioadele D300/D394 ale regularizării rămân cele ale documentului final;
referința nu mută suma negativă în luna avansului și nu recalculează TVA.

Fără referință: `TVA_AVANS_FARA_REFERINTA`, fără presupunerea că trebuie
folosită cota curentă. Referință nevizibilă/inexistentă/neoperată, alt
partener/sens sau propria factură: diagnostic separat, fără divulgarea
datelor sursei și fără alegerea automată a altei facturi. FK-ul nu permite
ștergerea fizică a unei surse referite; mecanismul curent de ștergere logică
nu șterge faptele istorice.

**Caz de tranșat în review:** o factură de avans poate avea mai multe cote
sau repere fiscale. Un singur `FacturaAvansId` nu identifică atunci cota
liniei regularizate. Nu alegem `First`, cota maximă ori o potrivire după
valoare. Varianta minimă propusă emite `TVA_AVANS_REFERINTA_AMBIGUA` și nu
pretinde validarea cotei. Varianta completă referă linia/faptul avansului
sau o repartizare între fapte; aceasta depășește relația cerută acum.
R6 nu se implementează înaintea review-ului acestei delimitări.

Pentru o sursă corectată tehnic, calificarea se citește după identitatea
fiscală și compensarea originalului/inversei, fără numărarea inversei ca
avans distinct. Dacă nu rezultă o calificare unică activă, avertismentul
rămâne de ambiguitate. Nu introducem consumul/restul avansului în această
felie; validarea regularizării peste valoarea avansului cere contract separat.

## Suprafețe

| Zonă | Modificare propusă |
|---|---|
| Entități și EF | TipTva: interval; TipMaterial: marcă; liniile FCT/FCL: referință opțională; migrație și validarea intervalului |
| Motor | Serviciu comun de avertismente pe fapte/draft; informările operării, fără refuz fiscal și fără rescriere |
| Raport | Proiecție secured, document/linie și direcție; calificare istorică/actuală, motiv și remediu orientativ |
| API | REST raport de impact; DTO/Apply și lookup-ul referinței FCT/FCL; rezultatul existent Mesaje |
| UI/XAF | Editor interval/marcă, selecție avans, avertismente după operare și acces la raport după editarea tipului |
| Contracte generate | Metadata/OpenAPI/types; UI nu calculează avertismente sau TVA |
| Probe | SC-CIT-90…94; integral ambele profiluri, HTTP secured, browser și regenerare fără drift |

Nu se schimbă Nucleul pentru diagnosticarea intervalului: acesta primește
în continuare calificarea rezolvată, fără dependență de nomenclatoare.
La implementare se actualizează `politici-si-fiscalitate` și `limite-curente`
cu acoperirea reală. Până atunci, aceste documente nu descriu R6 ca livrat.
