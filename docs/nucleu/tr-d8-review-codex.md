# TR-D8 — review propriu Codex

2026-09-25. Review al implementării în lucru peste `c10d0fe`, înainte de
închiderea etapei. D8-B6 A este aprobată de owner și consemnată în 100.
Review-ul nu certifică întregul TR-D8. Nu a fost trimis un mesaj intermediar către Claude.

## Probleme găsite și corectate

1. **Refuzul stocului venea prea târziu la anulare/storno.** Verificarea
   din materializator putea refuza după modificarea tracker-ului registrelor.
   Un apel ulterior pe același ObjectSpace vedea acele schimbări necomise.
   `Loturi.VerificaRetragere` verifică acum inversa cantitativă înaintea
   ștergerii conexelor, inversării împerecherilor și modificării registrelor.
   Gardul materializatorului rămâne pentru tranzacția completă. Regresia
   BTR verifică refuzul urmat de anulare/storno valid în același ObjectSpace.
2. **Refuzul cubului venea după asignarea numărului.** `Opereaza` cere
   refuzurile declarației înaintea numerotării și a materializării.
   Aceasta dublează momentan contractarea; costul trebuie inclus în
   măsurătoarea de performanță a etapei, înaintea unei optimizări.
3. **Explicația S3 din SAF-T avea altă sursă decât totalul.** Totalul
   balanței fusese portat, dar componentele pe tipuri de document încă
   citeau RegistruContabil. Deschiderea canonică a expus diferența în
   D17-V6/F7. Componenta contabilă folosește acum `Contabil.Postari`,
   inclusiv deschiderea și excluderea transferurilor; proba nu a fost slăbită.
4. **Scenele vechi nu aveau suportul de stoc în cub.** Loturile sintetice
   din probele BCS/BTR/FCL/LDI și SAF-T au primit deschidere canonică sau
   recepție FCT → NIR. Suplimentarea stocului nu creează o a doua deschidere.
   Registrele păstrate în fixture sunt oracolul explicit al regimului dual.
5. **Catalogul FIFO promitea mai mult decât verifica.** SC-CIT-39 verifică
   acum explicit excluderea altui cont și a altei gestiuni, prin mutații
   controlate cu rollback, pe lângă prioritatea pinului și alocarea locală.
6. **Ștergerea împerecherii nu elibera cubul.** Cu FCT 100 și PLT automată
   100, ștergerea linkului lăsa factura stinsă în cub. Comanda de ștergere
   scrie transferul compensator și elimină linkul în aceeași tranzacție.
   Ușile API/XAF păstrează gate-ul Delete; CRUD-ul direct este refuzat
   independent de dreptul de a citi Postare. SC-CIT-46/48 verifică
   soldurile și păstrarea postărilor inițiale.
7. **Inversa pregătită de corecție era invizibilă gardului de dependențe.**
   F27-R3b refuza factura complet stinsă în luna închisă, deși aceeași
   comandă pregătise inversa stingerii. Gardul include rândurile noi din
   tracker, fără dublarea celor persistate; tăierea zilnică rămâne.
8. **Stornoul nominalizării automate putea dubla desfacerea.** La storno
   direct, inversa Operare eliberează deja factura. La storno după desfacere,
   transferul compensator este atribuit nominalizării inițiale și se
   inversează împreună cu aceasta. SC-CIT-47 verifică ambele căi peste
   închidere, inclusiv păstrarea soldului din ianuarie.
9. **Reîmperecherea manuală pe aceeași factură prelua atribuirea automată.**
   Proba suplimentară SC-CIT-46 a reprodus două solduri greșite după storno
   și un refuz ulterior PARTIDA_CU_DEPENDENTI
   (`run-verificari/20260925-073311-635/scenarii-bugetar.log`). Atribuirea
   se aplică acum numai desfacerii nominalizării automate; transferul
   manual și inversa lui se compensează separat.

10. **Partida era confundată cu documentul în raport/snapshot.** Raportul
    general și snapshot-ul păstrează unitate × cont × partener, inclusiv
    deschiderile fără document și identitatea istorică. SC-CIT-49…54 verifică
    două partide ale aceleiași note, deschiderea și reconstrucția cu diferență
    introdusă deliberat; citirea secured nu consumă snapshot-ul global.
11. **Legătura putea exista fără efect în cub.** Decizia 101 este aprobată.
    Comanda refuză ținta incompatibilă, ambiguă sau insuficientă înaintea
    scrierii; o nominalizare existentă se asociază fără a o dubla. Proveniența
    transferului permite desfacerea exactă a fiecărei legături, chiar când
    două transferuri privesc aceeași pereche. SC-CIT-55…60 verifică refuzul
    repetat în același ObjectSpace, diagnosticul istoric și asocierile parțiale.
12. **Soldul final ascundea indisponibilitatea la o dată anterioară.**
    Transferul și asocierea unei nominalizări verifică și efectele ulterioare.
    Pentru nominalizarea automată, disponibilul sursei este minimul pe fiecare
    dată deja scrisă. SC-CIT-61/62 păstrează stingerile viitoare și nu consumă
    retroactiv suma eliberată ulterior.
13. **Plata automată încă citea sursa din registre.** FCT 159,5 cu recepție
    și serviciu nominaliza numai 100 cât timp NIR era Draft. `Fapte.Sursa`
    citește acum partidele sursei din cub; integralul verifică plata înaintea
    operării NIR, apoi operează recepția pentru comparația cu oracolul dual.
    F27-R3c păstrează și cazul facturii cu înregistrare întârziată.
14. **Transferul general pierdea ținta deschiderii.** Când comanda indică
    explicit o partidă de deschidere, aceasta este ținta, fără a cere un
    document fictiv. SC-DES-11…14 verifică consumul succesiv și inversarea.

15. **Gardul de acoperire confunda costul fără partidă cu o lipsă.**
    Controlul pe o latură contabilă fixă nu este suficient la vânzare/retur.
    Se compară acum totalul de decontare al antetului cu valoarea partidelor
    Operare. Antetul servește numai diagnosticului de activare; raportul,
    disponibilul și snapshot-ul nu citesc cifra lui. SC-CIT-43/65 disting
    lipsa reală de unități de costul legitim fără partidă.
16. **Probe vechi cereau comportamentul înlocuit de 101.** F19-D16 admitea
    PLT 401 ↔ INC 4111 fără transfer; SC-CIT-63 cere refuz cu solduri intacte.
    Proba NUC-PLT-SPLIT folosea o plată deliberat nemigrată; verifică acum
    refuzul ei și folosește o plată pe cub pentru nominalizarea 61/60.
    PAR-V22/23 păstrează totalul RDC −121, dar verifică datoria 121 din cub,
    fără cei 30 ai costului. Nu se elimină probele numerice.

## Limite care împiedică închiderea TR-D8

- **Partidele bugetare:** D8-B6 A este aprobată (100). Seed-ul activează
  401.01.00, 404.01.00 și 411.01.01 DinSeed; cititorul comun și diagnosticul
  istoriei sunt implementate și verificate (100-r1 închisă). Raportul și
  snapshot-ul sunt acum portate; gardul refuză și politicile manuale care
  produc documente stingibile fără partide. Validarea feliei 101 este mai jos.
- **Snapshot-ul de stoc:** este încă derivat din RegistruStoc. Cititorul
  nou de lot și raportul citesc integral cubul și nu îl consumă. Trebuie
  portate împreună cheia, proveniența, materializarea și reconstrucția;
  citirea R din ASM trebuie să rămână separată în regimul dual.
- **Fiscal/SAF-T:** TVA, D300/D394 și majoritatea secțiunilor SAF-T nu sunt
  încă portate. Versiunea istorică TVA trebuie păstrată, nu recitită din
  nomenclatorul curent. Portarea GLA/Customers/Suppliers și S3 contabil
  nu certifică întregul export.
- **Activarea stocului și politica modificată în mers:** diagnosticul
  contabil de la pornire nu dovedește încă toate transferurile cantitative.
  Trebuie probat istoricul de stoc incomplet și refuzată producerea unor
  fapte noi numai în registre după dezactivarea `PosteazaInCub`.
- **Concurența pe lot:** probele numerice secvențiale și gardul zilnic nu
  dovedesc serializarea a două consumuri concurente. Este necesară o probă
  pe două conexiuni, inclusiv cursa consum–retragere, înainte de certificare.
- **Regula de oprire:** auditul S-r2, controlul arhitectural 091-r3,
  măsurătorile A/B și verificarea tuturor consumatorilor din inventar
  rămân obligatorii. Scrierea registrelor nu se elimină aici.

## Validare finală a feliei partidelor (101)

- ModelCheck integral, rulat secvențial: **3.107 bugetar / 4.103 privat OK**,
  zero FAIL, exit 0, build fără avertismente. Dovezi:
  `run-verificari/20260925-091707-216/integral-bugetar.log` și
  `run-verificari/20260925-092938-922/rezultat.json`.
  Prima pereche de rulări a expus așteptări private vechi, corectate conform
  101; privatul a fost rerulat integral. Nu prezentăm primul rezultat agregat
  ca fiind verde. După validarea finală s-au schimbat numai comentarii C#,
  documentația și o așteptare HTTP (405 pe ruta OData fără POST).
- Client: metadata/OpenAPI/types regenerate și build reușit;
  regenerarea repetată păstrează cele trei hash-uri. Loguri
  `run-verificari/trd8-101-client-final.log` și `trd8-101-drift-final.log`.
  Avertismentul Vite despre dimensiunea bundle-ului rămâne.
- HTTP, `citiri-cub.py --partide`: raportul și panoul respectă permisiunile
  înainte de închidere, după închidere și după reconstrucție. Admin/Cititor
  = 125, restricție pe rând = 100, membrul Valoare refuzat = 0, User fără
  rânduri. Două partide pe același cont și document păstrează partenerii,
  100 și 25. Log: `run-verificari/trd8-101-http-report2.log`.
  Browserul confirmă cele două rânduri, contul, data, sensul și documentul.
- HTTP, `partide-cub.py`: ștergere User 404 / Cititor 403 / Admin 204;
  creare User/Cititor 403, document lipsă 404, OData POST 405;
  comanda autorizată 40 → 201, cererea 61 peste restul 60 → 422 fără efect;
  ștergerea și anularea păstrează FCT −100 / PLT zero. Browserul confirmă
  total 100 / asignat 40 / rest 60 și candidatul cu disponibil 60.
  Log: `run-verificari/trd8-101-http-payment2.log`.
  Migrațiile au fost aplicate pe ambele baze izolate CodexBCS.
  Curățarea a fost verificată independent: zero documente active și zero
  parteneri/roluri temporare ale probelor în baza privată izolată.
- Verificarea XAF de mai jos este cea din felia precedentă, nu o nouă
  rulare XAF după 101. Matricea generală `refuzuri.ps1` rămâne necertificată
  pe baza goală; probele țintite nu o înlocuiesc.

## Validare anterioară (100, contabil și stoc)

- ModelCheck integral: **3.036 bugetar / 4.026 privat OK**, zero FAIL,
  exit 0, build fără avertismente. C# nemodificat în timpul rulării:
  `run-verificari/20260925-073729-162/rezultat.json`.
  După această rulare s-au corectat numai comentarii și eticheta probei
  RolTert; logica verificată a rămas aceeași.
- Client: metadata/OpenAPI/types regenerate, build reușit; a doua generare
  păstrează toate cele trei hash-uri. Loguri `trd8-partide-client` și
  `trd8-partide-drift` în `run-verificari/`. Vite păstrează avertismentul
  privind dimensiunea bundle-ului.
- HTTP SC-CIT-25: două execuții complete, înainte de închidere, după
  închidere și după reconstrucție. Admin/Cititor = 125, restricția pe rând
  = 100, restricția pe Valoare = 0; User primește liste goale.
  `run-verificari/trd8-http-final.log`; rolurile și perioadele temporare
  au fost eliminate. Comandă: `python -X utf8 nou/tools/ProbeHttp/citiri-cub.py`.
- Raport de stoc prin HTTP și browser, pe FCT → NIR real: cont 371,
  cantitate 3, valoare 10, cost unitar afișat 3,333333 și total 10,00.
  Fixture-ul a fost anulat/șters prin API; baza izolată are zero documente active.
- SC-CIT-46/48 HTTP: User 404, Cititor 403, Admin DELETE 204; FCT −100,
  PLT +100 după desfacere și PLT zero după anulare. Aceeași comandă prin
  acțiunea XAF „Șterge împerecherea”: rând dispărut din grilă, aceleași
  solduri verificate independent în cub. Proba durabilă
  `nou/tools/ProbeHttp/partide-cub.py` (opțional `--prin-xaf`); loguri
  `trd8-partide-http.log` / `trd8-partide-xaf.log`. Zero documente și
  parteneri activi ai fixture-ului după curățare; hosturile de test oprite.
  Hostul XAF a cerut profilul Windows normal pentru Data Protection;
  cheile existente nu au fost șterse sau reconfigurate.
- **Limită de validare:** `nou/tools/ProbeHttp/refuzuri.ps1` a fost pornit,
  dar matricea completă nu poate fi declarată trecută pe baza goală.
  După pregătirea ITV, descoperirea se oprește la lipsa unei împerecheri;
  proba DVI de mai jos presupune și peste 500 de facturi candidate
  (`MaiSunt=true`). Aceste precondiții țin de vechea bază populată/importată.
  Proba nouă SC-CIT-25 nu este prezentată ca înlocuitor al întregii matrice.

**Verdict:** felia contabilă, cititorul operațional/raportul de stoc și
raportul/snapshot-ul/citirile operaționale ale partidelor au probe verzi în
domeniul de mai sus. 101-r1 este închisă. Etapa TR-D8 rămâne deschisă; limitele
nominale din review sunt lucru obligatoriu, nu excepții aprobate.
