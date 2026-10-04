# C106 — reverificarea corecturilor R1…R4

2026-10-04, Codex. Cod verificat: `5941b28`, peste `908d3c8`.
Predare: `comunicari/2026-10-04-1355-claude-codex-c106-review-corectat.md`.

**R1…R4 sunt corectate tehnic. Rămâne o observație P2 asupra probei
SC-X-24 (C106-R5), iar acceptarea limitei 106-r7 nu este certificată de
acest review.** Predarea spune explicit că 106(k) așteaptă aprobarea
owner-ului; trecerea lui 106-r7 în backlog „după PoC” nu constituie acea
aprobare.

## Reverificarea observațiilor inițiale

| Observație | Rezultat |
|---|---|
| R1, împerecheri închise/inversate | Închisă tehnic: `ImperechereService.Motive` separă anularea de stornare; SC-REGIM-03/09/10 verifică luna deschisă, închisă și istoricul inversat, inclusiv comenzi reușite. Clarificarea 106(b) prin (k) păstrează mecanismul 088(j). |
| R2, imobilizări | Închisă tehnic: predicatele dependenților și lunii sunt expuse prin `IDocumentCuRegistruPropriu`; PIF/AMO/CAS le folosesc și pe calea comenzii. SC-IMO-16/17 și catalogul IMO trec pe ambele profiluri. |
| R3, TVA declarată | Închisă: `FiscalitateService.MotivAnulare` este comun regimului și comenzii. SC-REGIM-11/12 verifică separat D300/D394, cu perioada deschisă, pe privat. |
| R4, restaurarea acțiunilor XAF | Închisă static: dicționar pe instanța `ActionBase`, eliminarea cheii Enabled, restaurarea tooltip-ului și golirea dicționarului la dezactivare. Nu am repetat browserul. Predarea delimitează corect că navigarea încercată nu reproduce schimbarea tipului de vedere în același Frame și că varianta veche nu a fost rulată în browser. |

Extragerea gardienilor păstrează locurile blocajelor și refuzurilor din
motor. Nu am găsit o regresie a comenzilor în diferența revizuită sau în
integrală. Costul SC-REGIM-13 rămâne 10 instrucțiuni pentru NTC și 12
pentru FCT în fixture-ul fără dependenți; aceasta nu este o măsurare de
performanță pe volum mare.

## C106-R5 — P2: excepția globală pentru PARTIDA_CU_DEPENDENTI ascunde dependenți permanenți

Loc: `nou/tools/ModelCheck/ProbeRegim.cs:44–49`, consumat de
`ScenaDocumente.cs:118–121`.

`RefuzLaComanda` acceptă orice mesaj care conține `PARTIDA_CU_DEPENDENTI`
la Stornează/Corectează. Nu primește data cerută, scena sau natura
dependenței. Prin urmare, un dependent permanent omis de regim trece la
fel ca SC-NTC-22, unde o dată ulterioară chiar permite comanda. Această
excepție ar ascunde și o regresie în citirea nominalizărilor fără legături,
nu numai limita declarată 106-r7.

Contraexemplu executat pe **ambele profiluri**, fără a modifica producția:

1. Furnizor propriu; FCT servicii 100, NTC debit furnizor/credit cheltuială
   40, în ianuarie 2013. NTC nominalizează FIFO 40 pe partida facturii;
   soldul verificat independent este −60.
2. Împerechere manuală NTC → FCT de **exact 40**, cu
   `Autogenerat = false`, `TranzactieCubId = null`. Nu există o sumă
   suplimentară de nominalizare în acest fixture.
3. Închidem ianuarie. Regimul FCT oferă Stornează și Corectează.
4. Stornarea și corecția în februarie refuză `PARTIDA_CU_DEPENDENTI`.
   Originalele sunt identice, iar inversa împerecherii nu rămâne după rollback.
5. SC-X-24 raportează totuși **„REGIM: 5 comenzi ... conforme”** pe bugetar
   și **7** pe privat. Ambele rulări au exit 0, zero FAIL.

Mecanismul refuzului este independent de data aleasă: în
`GardieniRetragere.cs:35`, partenerul legăturii vii este exclus integral din
nominalizări. La comandă, `ImperechereService.DesfaceEfect` nu inversează
postarea Operare a NTC pentru legătura manuală fără transfer. Nominalizarea
40 rămâne activă; nicio dată ulterioară nu o elimină.

Cere restrângerea excepției la scenariul temporal demonstrat, ori la o
clasificare verificabilă a refuzului. Limitele acceptate trebuie probate
și numărate separat; un mesaj global nu trebuie să transforme orice
diferență de această familie în „conform”. O probă de mutație care omite
un dependent permanent din regim trebuie să pice SC-X-24.

## Delimitarea lui 106-r7

Contraexemplul de mai sus măsoară și limita declarată în predare. El nu
cere o nominalizare suplimentară peste suma legăturii: nominalizarea și
împerecherea sunt ambele 40. Descrierea „partenerul are și o nominalizare
liberă” trebuie să includă explicit legătura manuală fără transfer cub.
Altfel, cititorul poate înțelege că o nominalizare acoperită integral de
legătură este sigură pentru stornarea sursei.

106(k) poate clarifica diferența dintre anulare și storno fără să accepte
automat această abatere de la regimul onest. Owner-ul decide separat dacă
106-r7 este amânată; review-ul nu deduce aprobarea din starea backlogului.
Nu redeschid R1…R4 pentru această limită și nu o prezint ca defect nou
nedeclarat al motorului. Comenzile păstrează refuzul atomic corect.

## Dovezi și comenzi

Integrala pe sursele nemodificate: **3.450 bugetar / 4.671 privat OK**,
zero FAIL, exit 0. Manifest:
`run-verificari/20261004-140600-307/rezultat.json`.

Proba adversă `C106-RV-NOM`: manifest
`run-verificari/20261004-141336-620/rezultat.json`, ambele profiluri, exit 0.
Liniile `OK C106-RV-NOM` asertează existența contradicției, nu conformitatea
regimului. Patch reproductibil și copie originală:
`run-verificari/codex-c106-rv/nominalizare.patch`, respectiv
`ScenariiRegim.cs.original`. `git apply --check` a trecut după restaurare.

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexXReview
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Scenarii -Tip REGIM -Profil Ambele -Sufix .CodexXReview
```

A doua comandă a fost folosită pentru proba adversă și repetată după
restaurarea octet cu octet a sursei. Nicio modificare de producție, nicio
scriere directă în cub/registre, nicio comutare `PosteazaInCub`. HTTP,
browser și drift nu au fost rerulate de Codex; rămân dovezile predării.
Fără commit.

Rebuild și REGIM după restaurare: ambele profiluri, exit 0;
`run-verificari/20261004-141445-416/rezultat.json`.
