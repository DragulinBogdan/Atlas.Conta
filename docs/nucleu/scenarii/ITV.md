# ITV — închidere TVA

**2026-09-23: catalog specificat înaintea implementării pasului 3.**
Reguli:046(c),088(f–h),090(i),091(a–e), contractul TR-D7b T-D3.
ITV moștenește postarea explicită de la NTC; nu primește declarant separat.
Generatorul și verificarea soldurilor rămân pe `SolduriService` până la
TR-D8. Așteptările de aici sunt numerice și independente de acea citire.

Fixture: anul 2007 liber, unitate internă `E2E-SC-ITV-LOC`,
ObjectSpace nou per comandă și curățenie în `finally`. Soldurile se nasc
din documente reale operate, nu din inserții în registre/cub. Pentru a
izola postarea închiderii, NTC poate pregăti soldurile explicite de TVA;
un scenariu separat păstrează lanțul FCT/FCL → ITV. Generatorul primește
unitatea internă proprie scenei, astfel încât și ITV intră în curățenie.
Fiecare rând folosește o stare inițială curată sau inversează documentele
anterioare. Nu se comută `PosteazaInCub` și nu se adaugă politică bugetară.

Privat: politica are 4426 deductibilă, 4427 colectată, 4423 de plată,
4424 de recuperat. Închiderea este contabilă: cantitate 0, fără partidă,
lot, partener, cotă, sens TVA, rol TVA sau perioadă de declarare pe postări.
Nu dublează jurnalul fiscal al facturilor și nu închide perioada fiscală.
Bugetar: profil inert fără politică de închidere TVA.
Probele sunt în `ScenariiItv`; validarea este consemnată la final.

| Id | Scenariu și așteptare numerică | Rezultat așteptat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-ITV-01 | Solduri la 31.01:4426 D21 și4427 C42. Generare:4427=4426:21 și4427=4423:21. Dry-run fără scriere; operare patru postări, sold4426/4427 zero, 4423 C21. | acceptat | 046(c), T-D3 | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-02 | Solduri4426 D42 și4427 C21. Generare:4427=4426:21 și4424=4426:21. După operare4426/4427 zero, 4424 D21. | acceptat | 046(c), T-D3 | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-03 | Solduri4426 D21 și4427 C21: o singură linie4427=4426:21, două postări, fără linie cu valoarea0 pe4423/4424. | acceptat | 046(c) | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-04 | SC-ITV-01 operat31.01, storno tot 31.01: ambele perechi−21. Solduri4426 D21/4427 C42/4423 zero; originale nemodificate. Repetarea stornării refuzată. Regenerarea lunii produce iar21+21. | acceptat, apoi refuzat prin gardul de stare | 046(c),090(i) | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-05 | Anulare în perioadă deschisă a închiderii21+21: Draft, zero postări proprii; solduri inițiale21/42; reoperarea închide iar la zero. | acceptat | invariant III,091(b) | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-06 | Închiderea21+21 operată; ianuarie închis; anulare refuzată, storno05.02 acceptat. La31.01 TVA închisă și4423 C21; la 05.02 solduri4426 D21/4427 C42/4423 zero. | acceptat pentru storno | 088(f,g),090(i) | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-07 | După închiderea lui ianuarie, corectează ITV21+21 pe 05.02. Originalul se inversează exact în februarie; draftul păstrează data 31.01, DataInregistrare 05.02, CorecteazaId și motiv. Operarea înlocuitorului trebuie să refacă închiderea21+21 fără modificarea lui ianuarie. | acceptat | 088(h),091(b); observația de mai jos | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-08 | Generează din21/42, apoi operează un document suplimentar cu TVA deductibilă 7. Draftul21+21 este refuzat fără postări; regenerarea trebuie să dea transfer28/de plată14. | refuzat prin gardianul existent de solduri, apoi acceptat | 046(c), anti-stale | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-09 | Zero solduri: FaraSold, niciun draft. O închidere vie: InchidereVie, niciun duplicat. Închidere ulterioară vie sau draft anterior: refuz cronologic fără scriere. | refuzat/negenerare cu motiv existent | 046(c) | `ScenariiItv`, ID-ul rândului | verificat, privat |
| SC-ITV-10 | Profil bugetar fără PoliticaInchidereTva: ProfilInert; niciun document sau postare nouă. | negenerare cu motiv ProfilInert | 046(c), politica profilului | `ScenariiItv`, ID-ul rândului | verificat, bugetar |
| SC-ITV-11 | FCT serviciu100+TVA 21 și FCL serviciu200+TVA 42 în ianuarie → ITV21+21. Facturile și postările lor fiscale rămân intacte; închiderea nu adaugă fapte TVA. | acceptat | T-D3,091(b), politicile fiscale ale profilului | `ScenariiItv`, ID-ul rândului | verificat, privat |

## Corecția peste perioadă

SC-ITV-07 a reprodus refuzul incorect: validarea citea soldurile până la
`Data` originalului (31.01), unde TVA era deja închisă, ignorând stornoul
înregistrat în februarie. Validarea și citirea `Stale` folosesc acum
același reper: `DataInregistrare` pentru corecția legată, `Data` pentru
închiderea obișnuită. Astfel, originalul și închiderea lui ianuarie rămân
intacte, iar corecția verifică soldurile disponibile la data postării sale.
Sursa soldurilor rămâne `SolduriService`, conform T-D3, până la TR-D8.

## Recensământ și validare

Citit read-only pe 2026-09-23 pe `Atlas.Conta.Import1C.Flax`, cu
`recensamant-ntc-itv.sql`: 12 documente, 24 linii, exact două pe fiecare;
zero linii negative, zero perechi cu același cont, zero repartitori
expliciți pe linii și zero date de înregistrare diferite. Recensământul
nu acoperă ramura cu o singură linie sau corecția peste perioadă.

ITV este activat în seed numai pe privat. Citirile de producție și
proiecțiile reconstruibile rămân la TR-D8; soldurile din probe sunt
adunări directe peste cub.

2026-09-23, `verifica.ps1 -Suita Scenarii -Tip NTC,ITV -Profil Ambele
-Sufix .CodexBCS`: 119 verificări ITV privat / 2 bugetar, zero eșecuri.
Manifest: `run-verificari/20260923-114840-607/rezultat.json`.
Corecția SC-ITV-07 a fost mai întâi reprodusă ca eșec, apoi verificată
cu soldurile 21/42 citite la data înregistrării înlocuitorului.

Gate integral final, 2026-09-23: `verifica.ps1 -Suita Integral -Profil
Ambele -Sufix .CodexBCS`, **1.740 OK bugetar / 2.305 OK privat, 0 FAIL**,
exit 0; build cu 0 avertismente. Manifest:
`run-verificari/20260923-115007-397/rezultat.json`. Curățenie verificată
read-only: zero repartitori ai scenelor NTC/ITV/API-NTC, zero postări și
perioade din 2006–2007 pe ambele baze de test.
