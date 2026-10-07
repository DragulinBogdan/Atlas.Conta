# FCT — factură de intrare

**2026-09-23: primul lot independent verificat pe ambele profiluri.** Reguli: 025 (c,d),
031 (e), 088 (f–j), 090 (a,c,d,h,j), 091 (a–e). Politicile de TVA sunt cele
din `docs/privat/p1-tva-design.md`. Valorile de mai jos sunt constante ale
scenariului; registrele și normalizările nu stabilesc așteptarea.

Fixture: anul 2002 liber, MAG1, furnizor propriu scenei, produse/loturi noi.
FCT se culege și operează prin comenzile reale. NIR-ul conex se operează
numai când se probează fluxul fizic dual; nu se schimbă `PosteazaInCub`.
Privat: material 302, furnizor 401, TVA deductibil 4426, servicii 628.
Bugetar: 302.01.00, 401.01.00, 628.00.00; TVA CAP21 se capitalizează.
Profilul bugetar nu are `PoliticaTva`: costul brut nu poartă coordonate
fiscale; separarea bază/taxă și reperul fiscal se probează la privat.
Aceasta corectează ipoteza inițială a catalogului conform P1 §4/§6,
nu schimbă regula sau implementarea motorului.
Conturile bugetare nu au `RolTert`: partida în cub se verifică la privat;
la bugetar se verifică postările fără unitate de terț. Toate valorile sunt RON.

| ID | Pași și așteptări numerice | Rezultat | Proveniență | Proba | Stare |
|---|---|---|---|---|---|
| SC-FCT-01 | 05.01: recepție 10 × 10 fără TVA; D stoc 100/+10 pe lot, C furnizor 100/−10 pe gestiunea virtuală Furnizor. Dry-run fără scriere; o singură partidă 401 de 100 la privat. Operarea NIR nu dublează cubul. | acceptat | 090 (h), SC-X-02 | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-02 | Două loturi: 4 × 10 și 2 × 15 → 40 și 30, datorie 70. | acceptat | recensământ: 5.652 FCT cu mai multe linii | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-03 | Storno în 20.01 al unei recepții 10/100 cu NIR încă Draft: invers −10/−100, sold lot și datorie zero; postările originale păstrate. | acceptat | 090 (i), 025 (d) | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-04 | Factură serviciu net100+TVA21 în ianuarie, ianuarie închis; storno în05.02: datoria ianuarie rămâne121, februarie−121; privat cost100/TVA21, bugetar cost121; fiscalul privat al stornoului poartă februarie. | acceptat | 088, 090 (i) | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-05 | Anulare FCT 10/100 cu NIR Draft: zero tranzacții/postări/registre FCT, sursa Draft, conexul Draft eliminat. Lotul de culegere se păstrează în implementarea duală (025c), fără sold. | acceptat; limita față de țintă explicită | 025 (c), invariant III | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-06 | Serviciu 100 cu TVA21: privat datorie121/TVA21, bugetar cost121. După închiderea lui ianuarie: corecție EroareMateriala în februarie, original inversat și draft legat; schimbă netul la80 și operează → privat cost80/TVA16,80/datorie96,80; bugetar cost96,80. Fiscalul privat al corecției rămâne în ianuarie; originalul nemodificat. | acceptat | 088 (h) | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-07 | Privat N21: 100+21 și N11: 50+5,50 → cost150, TVA26,50, datorie176,50. Bugetar CAP21/CAP11: cost121+55,50, datorie176,50, fără coordonate fiscale. | acceptat | recensământ: 42 FCT multicotă | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-FCT-08 | Privat: două linii N21 cu net0,01 fiecare → taxa documentului rotunjită0,00, datorie0,02. Separat, net100 și TVA culeasă21,01 → datorie121,01 (politica fără toleranță). | acceptat | 090 (j), S-D15 | `ScenariiFct` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCT-10 | Privat TI21: net100 cu taxă autolichidată21 (4426 = 4427). Totalul de stins pe antet și pe cub este100, iar plata autogenerată plătește100, nu121. | acceptat | 102 (d): găsit de INV-CUB pe faptele scenei (review Codex R1) | `ScenariiFct` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCT-11 | Privat: trei linii de serviciu net 10,03 la N21, fără taxă culeasă. Taxa documentului 6,32, repartizată 2,11 / 2,11 / 2,10 pe 4426; datoria 401 = 36,41 = totalul documentului = totalul de stins. | acceptat | 109 (a,b) | `ScenariiFct` / ID-ul rândului | verificat privat; neaplicabil bugetar |
| SC-FCT-12 | Bugetar, `PoliticaValidare` a clasificației oprită pe durata probei, ca gardul să judece singur. Recepție 10 × 10 pe 302.01.00 de la furnizorul cu 401.01.00 (cere cod economic), linia fără cod economic și fără angajament: dry-run și operare refuzate cu „Contul 401.01.00 (credit, linia cu …) cere: Cod economic."; factura rămâne Draft, fără postări, fără NIR conex. Înaintea pasului 2 (din cod, nemăsurat) factura trecea, iar analiza se cerea la operarea NIR-ului conex. | refuzat, atomic | D9-A3, schimbarea 5 | `ScenariiFct` / ID-ul rândului | verificat bugetar; neaplicabil privat |
| SC-FCT-13 | Aceeași recepție cu angajament pe linie și fără cod economic: acceptată; D stoc 100/+10 și C furnizor 100/−10, fără cod economic pe postări; NIR conex operat, lot 10/100. | acceptat | D9-D4: angajamentul ține loc de cod economic | `ScenariiFct` / ID-ul rândului | verificat bugetar; neaplicabil privat |
| SC-FCT-14 | Aceeași recepție cu cod economic explicit: acceptată; aceleași două postări, cu codul economic pe amândouă; lot 10/100. | acceptat | D9-D4 | `ScenariiFct` / ID-ul rândului | verificat bugetar; neaplicabil privat |
| SC-FCT-09a/b/c | Număr lipsă / cantitate zero / lot lipsă: coduri NUMAR_LIPSA / CANTITATE_NEPOZITIVA / LOT_LIPSA prin declarație, refuz pe comandă și zero efecte. | refuzat | declarant și gardienii entității | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |
| SC-X-01 | FCT 10/100 → NIR operat → BCS 4/40 → storno FCT: refuz conex operat. Storno NIR cu deltă zero: acceptat, fără inversă economică; sold cub 6/60, datorie 100 și BCS 40 intacte. Registrul lotului rămâne −4: limită duală acceptată, poate refuza alte operații pe lot până la TR-D9 (098-r3). | FCT refuzat; NIR acceptat | 098(a), 099: NIR inversează numai delta proprie | `ScenariiFct` / ID-ul rândului | verificat pe ambele profiluri aplicabile |

Probele asertează rândurile individuale, inclusiv latura, data, gestiunea,
unitatea, cauza, baza/taxa și perioada fiscală. Citirile sunt sume directe
de test peste cub, nu cititorii de producție. Fișa/balanța/`Sold` și explicația
persistată rămân la TR-D8; reevaluările/compensările la TR-D9; concurența,
taxarea inversă, avansurile și valută necesită rânduri proprii înainte de
certificarea ciclului complet. Stingerea este probată în grupul PLT/INC.

Recensământ read-only la 2026-09-23, `Atlas.Conta.Import1C.Flax`, script
`recensamant-tipuri.sql`: 19.035 documente, 36.996 linii, maxim49/document,
5.652 documente multiline, 42 multicotă, 325 linii cu cantitate sau valoare
negativă, zero corecții legate și zero date de înregistrare diferite.
Liniile negative sunt o întrebare pentru catalog, nu o regulă importată;
încadrarea lor ca reducere/retur/corecție rămâne de documentat separat.

Validare finală: 51 verificări independente bugetar / 63 privat.
Include verificările comune de nemodificare la storno/corecție.
Gate integral pe ambele profiluri: `run-verificari/20260923-010027-252/rezultat.json`,
1.699 / 2.089 verificări, zero eșecuri.

Extensie SC-X-14 (2026-09-23): FCT NED21: bază 100 și taxă 21 pe cost (brut 121); TI21: bază 100 și taxă deductibilă 21, fără fapt colectat (B-r4).
Sunt probate operarea și stornoul în aceeași lună, cu măsuri inverse, prin
matrice independente și martori de duplicare/lipsă. Aceasta nu certifică
ciclul complet al regimurilor speciale peste perioadă.

Completare transversală SC-NIR-36/FCT: pe bugetar, furnizor cu cont
408.00.00, FCT serviciu 100 fără TVA → D628.00.00 100 / C408.00.00 100,
partida furnizorului −100. UrmarestePartide este proprietatea contului și
are efect și în afara NIR. Proba rulează în catalogul NIR.

## Extinderea partidelor bugetare — 2026-09-25

Decizia 100 activează urmărirea pe 401.01.00, 404.01.00 și 411.01.01,
fără rol comercial SAF-T. Așteptările de partidă (rest, transfer, storno,
corecție) ale ciclurilor comune se aplică acum ambelor profiluri.
Mențiunile anterioare „numai privat” descriu acoperirea de la data probării
inițiale; fiscalul, DSC și contul 419 rămân specifice profilului privat.
Probele TR-D8 SC-CIT-41…45 verifică separat politica și istoricul.

## TR-D9a, pasul 2 (2026-10-05): SC-X-01 și limita 098-r3

Măsurat în SC-X-27 ([README](README.md#lanțurile-transversale-sc-x-)): după
stornarea NIR-ului acoperit cu consum, registrul lotului e −4, dar nicio
operație ulterioară pe lot nu e refuzată. Garda de sold a registrului rulează
numai pe tipurile din afara cubului, iar acestea nu au reguli de stoc.
Formularea „poate refuza alte operații pe lot" din rândul SC-X-01 nu se
confirmă pe starea de azi.
