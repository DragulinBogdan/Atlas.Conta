# NTC — notă contabilă

**2026-09-23: catalog specificat înaintea implementării pasului 3.**
Reguli: 090 (c–e,i), 091 (a–e), contractul TR-D7b T-D3 și 088 (f–h).
NTC este activat pe cub pe ambele profiluri prin `DeclarantNotaContabila`.
Așteptările de mai jos sunt constante, nu rezultate ale registrelor.

Fixture: anul 2006 liber, repartitori cu marcaj `E2E-SC-NTC`,
ObjectSpace nou pentru fiecare comandă, curățenie în `finally`.
Documentele au laturi interne; partenerii se culeg pe liniile explicite.
Privat: 628, 401, 4111, 419, 302. Bugetar: 628.00.00, 401.01.00,
411.01.01, 302.01.00. Conturile bugetare nu au `RolTert`: probele de
partidă sunt private; postarea explicită și ciclul fără partidă sunt comune.
Toate valorile sunt RON; `Cantitate = ValoareValuta = 0`. Postările NTC
sunt contabile, fără coordonate de TVA, inclusiv când contul este 4426/4427.

## Scenarii

`P(document, cont, partener)` numește semantic partida, cu identitatea
stabilită de decizia 092, descrisă mai jos. Soldul este D − C.
Fiecare scenariu pornește fără solduri pe partenerul său, cu excepția
documentelor menționate explicit. `D x = C y : v` păstrează valoarea v,
inclusiv semnul; nu inversează conturile pentru o valoare negativă.
Probele sunt în `ScenariiNtc`; validarea este consemnată la final.

| Id | Scenariu și așteptare numerică | Rezultat așteptat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-NTC-01 | 05.01, 628 = 401 : 100, fără repartitor pe linii. Dry-run fără scriere; operare: D628 100, C401 100, fără partidă/partener. | acceptat | T-D3 fallback A | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-02 | Trei linii: 628 = 401 : 100; 628 = 401 : −25; 401 = 401 : 40. Șase postări, cu valorile 100/100, −25/−25, 40/40; sold 628 +75, 401 −75. Cauza fiecărei linii păstrată; analiticele culese copiate pe ambele picioare. | acceptat | T-D3; recensământ: 3.983 linii negative, 104 cu același cont | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-03 | Storno pe 20.01 al notei simple de 100: D628 −100, C401 −100; net zero, originale nemodificate. Repetarea stornării se refuză fără scriere. | acceptat, apoi refuzat prin gardul de stare | 090 (i), 091 (b) | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-04 | Nota simplă de 100, ianuarie închis; anulare refuzată; storno pe 05.02: −100 pe fiecare latură. La31.01 sold628 +100/401 −100; la 05.02 ambele zero. | acceptat pentru storno; anulare refuzată | 088 (f,g), 090 (i) | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-05 | Anulare în ianuarie a notei simple de 100: Draft, zero tranzacții/postări/registre proprii. Varianta cu partener pe 401 deschide P cu sold−100, apoi anularea elimină toate postările acestei partide. | acceptat | invariant III, 091 (b) | `ScenariiNtc`, ID-ul rândului | verificat; varianta cu partidă numai privat |
| SC-NTC-06 | Nota de 100, ianuarie închis; corecție EroareMateriala pe 05.02: original inversat−100 și draft legat. Schimbă valoarea la 80 și operează: ianuarie rămâne100, februarie net−20, cumulat 80; original nemodificat. | acceptat | 088 (h) | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-07 | Două FCT de servicii fără TVA ale aceluiași furnizor, 03.01:60 și04.01:40. Pe05.01 NTC 401 = 628 :75, partener pe debit. D401 60 pe prima partidă și15 pe a doua; C628 total75. Solduri0 și−25; fără partidă proprie NTC. | acceptat | T-D3 B, 090 (e) | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-08 | Aceleași FCT60+40, NTC cu două linii debit401 de70 și50. Prima alocă60+10; a doua30 și20 pe P(NTC,401,furnizor). Soldurile surselor0/0, partida proprie+20; nicio reutilizare a celor60 sau40. | acceptat | T-D3, plafon în secvența liniilor | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-09 | FCT60 din 03.01 și40 din 10.01; NTC75 înregistrat 05.01. Numai60 disponibil:60 pe prima,15 pe propria; factura din 10.01 rămâne−40. Separat, egalitatea datelor se departajează prin Guid.CompareTo al unităților, nu ordinea SQL. | acceptat | 090 (e), N-D7 | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-10 | FCT fără TVA de 100; NTC 628 = 401 :−30 cu furnizor pe credit. C401 −30 pe partida facturii, D628 −30; datoria rămasă70. Conturile/laturile nu se inversează. | acceptat | T-D3, S-D14 | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-11 | FCT100 și NTC 628 = 401 :20 cu același furnizor. Creditul20 nu stinge o datorie creditoare: P(FCT) rămâne−100; se deschide P(NTC) cu−20. | acceptat | 090 (d,e), sensul restului | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-12 | NTC inițială 4111 = 419 :100, același client pe ambele laturi, fără solduri anterioare: două partide+100/−100. NTC regularizare419 = 4111 :60 numește ambele unități: solduri+40/−40. Storno regularizare inversează exact alocarea60 și redeschide+100/−100. | acceptat | T-D3 regularizare avans, 090 (d,e) | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-13 | Nota N are două linii628 = 401 :100, furnizor A respectiv B. Două partide distincte cu sold−100 fiecare. O notă de stingere pentru A cu 100 închide numai partida A; B rămâne−100. | acceptat, cu cheie extinsă conform092 | recensământ:78 combinații document–cont cu parteneri multipli; N-D6 | `ScenariiNtc`, ID-ul rândului | verificat, privat; decis prin092 |
| SC-NTC-14 | 628 = 302 :50 fără lot: D62850/C30250, Q0, unitate null, spațiul Contabil, zero mișcări de stoc. | acceptat | T-D3 fallback stoc; recensământ:3.365 picioare 3xx fără lot | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-15 | 401 = 628 :100 fără partener, deși există o FCT100. Nota nu consumă partida facturii; aceasta rămâne−100. Partener și unitate null pe notă; nicio partidă inventată. | acceptat | T-D3 fallback A | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-16 | FCT100 → NTC401 = 628 :40 nominalizat pe ea → anulare/storno FCT. Refuz cât nominalizarea este activă; zero efecte suplimentare. Storno NTC readuce partida la−100, apoi storno FCT o închide. | refuzat, apoi acceptat după inversarea nominalizării | 090 (e) | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-17 | FCT100 → NTC401 = 628 :40, nominalizat integral. Împerecherea ulterioară cu aceeași factură nu mută încă40: fără Transfer nou; partida rămâne−60. | acceptat cu transfer sărit și motiv verificabil | T-D3, lipsa partidei proprii | `ScenariiNtc`, ID-ul rândului | verificat, privat |
| SC-NTC-18 | Notă fără linii / fără unul dintre conturi / valoare zero / latură de document externă: refuz și zero efecte. Linii negative și conturi egale nu sunt refuzuri. | refuzat: LINII_LIPSA / CONT_EXPLICIT_LIPSA / VALOARE_ZERO / PREDATOR_NEPOTRIVIT | T-D3, contractul laturilor | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |
| SC-NTC-19 | Data documentului05.01, DataInregistrare 05.02; nota100 postează numai în februarie. La31.01 zero; la 05.02 sold628+100/401−100; partida proprie, dacă există, deschisă05.02. | acceptat | 088 (f), T-D3 | `ScenariiNtc`, ID-ul rândului | verificat, ambele profiluri |

SC-NTC-22 (așteptare scrisă la review, înaintea gardianului temporal): FCT100
în03.01 → NTC de stingere40 în05.01 → storno NTC în20.01. Storno FCT în10.01
trebuie refuzat, fiindcă între10.01 și19.01 nominalizarea este încă activă;
soldul partidei trebuie să rămână−60 în acel interval. Storno FCT în20.01
este permis după inversarea NTC; soldul final este0. Starea: verificat,
proba `ScenariiNtc`, numai privat.

## Identitatea partidei: decizia 092

Owner-ul a ales cheia `(document, cont, partener)`. Decizia
[092](../../decizii/092-identitatea-partidei-include-partenerul.md) amendează
N-D6: aceeași notă poate deschide partide distincte pentru parteneri diferiți
pe același cont. SC-NTC-13 trebuie acceptat, fără a amesteca soldurile.

Identitățile deja persistate nu se rescriu. Adaptorul recunoaște cheia
istorică și o păstrează la nominalizarea sursei și la transfer. SC-NTC-20
probează cheia istorică fixată pe octeți, respinge partenerul greșit și
transferă40 de pe o plată nouă către partida istorică de100.
Flax rămâne nemodificat.

## Recensământ și limite

Citit read-only pe 2026-09-23, `Atlas.Conta.Import1C.Flax`, cu
`recensamant-ntc-itv.sql`; aceleași rezultate pe clona `.TrD7b`:
7.818 note, 23.003 linii, 5.778 note multiline, maximum 228 linii/notă;
3.983 linii negative, 104 cu conturi egale, 5.593 cu repartitor explicit pe
cel puțin o latură, zero date de înregistrare diferite. Pe conturile cu
RolTert: 5.592 picioare cu partener și 7.816 fără repartitor; acestea din
urmă diferă de cifra istorică 7.824 din T-D3. Nu am modificat importul și
nu interpretez diferența ca acoperire câștigată. Scriptul păstrează
definiția măsurării. Sunt 3.365 picioare 3xx fără lot. Cele 78 combinații
document–cont cu parteneri multipli au maximum 82 parteneri.

Citirile independente din scenă adună cubul la fiecare graniță;
cititorii de producție, fișa/balanța și reconstruirea proiecțiilor rămân
TR-D8. Concurența pe partidă rămâne condiție pentru „rotund” 091(g).
Activarea folosește politica din seed. Gardianul nucleului cere unitate
pentru cantitate nenulă, deci SC-NTC-14 nu justifică relaxarea conservării.

## Validare executată

2026-09-23, `verifica.ps1 -Suita Scenarii -Tip NTC,ITV -Profil Ambele
-Sufix .CodexBCS`: 39 verificări NTC bugetar / 96 privat, zero eșecuri.
Manifest: `run-verificari/20260923-114840-607/rezultat.json`.
`verifica.ps1 -Suita Nucleu -Sufix .CodexBCS`: 165/165 teste,
zero eșecuri și zero avertismente; include noua proprietate a identității
pe 500 de cazuri. Manifest: `run-verificari/20260923-114932-068/rezultat.json`.

Diagnosticul de reconciliere recunoaște identitățile istorice și pe cele
noi; o nominalizare FIFO nu este confundată cu deschiderea partidei.
Egalitatea resturilor documentelor cu registrele vechi nu certifică FIFO:
registrele păstrează împerecherea manuală; scenariile verifică numeric cubul.

Gate integral final, 2026-09-23: `verifica.ps1 -Suita Integral -Profil
Ambele -Sufix .CodexBCS`, **1.740 OK bugetar / 2.305 OK privat, 0 FAIL**,
exit 0; build cu 0 avertismente. Manifest:
`run-verificari/20260923-115007-397/rezultat.json`. Curățenie verificată
read-only: zero repartitori ai scenelor NTC/ITV/API-NTC, zero postări și
perioade din 2006–2007 pe ambele baze de test.
