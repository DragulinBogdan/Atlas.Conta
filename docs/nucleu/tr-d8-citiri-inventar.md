# TR-D8 — Inventarul de intrare și ordinea portării

2026-09-25. În lucru: IMO și rapoartele contabile generale sunt portate,
inclusiv snapshot-ul contabil. Cititorul operațional și raportul de stoc
sunt portate și verificate; snapshot-urile de stoc și partide folosesc cubul.
Fiscalul și restul verificărilor transversale rămân deschise.
Owner-ul a autorizat continuarea după NIR și Deschiderea generică.
095 fixează acum ordinea DEC → contract IMO → PIF/AMO/CAS complete → TR-D8;
DEC și PIF/AMO/CAS au declaranți; partidele folosesc UrmarestePartide (096),
fișa și nominalizarea cu suport sunt implementate conform 097.
Surse: 090(f), 091(g/i/j), `nucleu-transfer.md` §3–4,
`nucleu-coordonate-rapoarte.md`, T-r11, T-r15, N-r8 și S-r2.

## Intrările comune

| Întrebare | Domeniul obligatoriu |
|---|---|
| Rulaje și solduri contabile | Carte=Contabil, toate partițiile relevante; exclude Transfer și postările de storno care inversează Transfer; sume D/C distincte, filtrare înaintea agregării |
| Sold și mișcări pe lot | Carte=Contabil, lot, cont, produs, gestiune; include Transfer și inversul; cantitate aditivă în domeniul produsului/UM, valoare semnată după latură |
| Sold și mișcări pe partidă | Carte=Contabil, cont, partener, unitate; include Transfer și Deschidere; sensul din sold și rolul contului |
| Fapte TVA | CodTva și PerioadaDeclarare, indiferent de Carte; Bază și Taxă distincte; contraponderile fiscale fără cod nu sunt fapte |

`Module/Cub/Citiri` va fi intrarea comună. Testul de arhitectură 091-r3
interzice citirile directe în consumatorii de producție; adaptoarele de
scriere rămân scriitori, iar verificările ModelCheck pot inspecta cubul
independent. Citirile motorului existente vor folosi aceleași intrări.
Fără proiecții persistate suplimentare până la o măsurătoare care le cere;
orice snapshot se reconstruiește exact din postări (091-i).

## Consumatorii identificați în cod

| Suprafață | Sursa actuală / dependența | Portare |
|---|---|---|
| `ContabilProiectii` | RegistruContabil și `SolduriService.AtomiCumulati`; balanță, fișă, jurnal, sold parteneri | Intrarea contabilă, atributele documentului separat, etichete prin join după agregare |
| `StocProiectii.SoldStoc` | `SolduriService.MiscariCumulate`, cheia Lot/Repartitor/TipStoc | Intrarea de lot; contract explicit pentru compatibilitatea TipStoc și pentru gestiunile reale/virtuale |
| `ImperecheriProiectii.DocumenteCuRest` | șase ramuri de documente, TotalStingere și Imperechere | Sold de partidă; o poziție de deschidere n-are obligatoriu document; avansurile rămân unități distincte |
| `TvaProiectii` | RegistruTva, inclusiv snapshot Regim/Cota și data fizică | Fapte Bază/Taxă; se verifică proveniența istorică înaintea eliminării sursei vechi |
| `D300Proiectii`, `D394Proiectii` | registrul fiscal/agregatele TVA | Consumă aceeași intrare fiscală, păstrează avertismentele și numărătorile fiscale |
| `SaftProiectii` | rapoartele pe cont, TVA, stoc și documente; GLA/GLE, facturi, Payments, MovementOfGoods, PhysicalStock | Raport cu raport; probe numerice și DUK, fără import ca gate |
| `SolduriService` | snapshot-uri contabile, stoc, partide; materializare, reconstrucție și referințe | Sold pe coordonate, probe de egalitate la graniță și după reconstrucție |
| Imobilizări | `Cub.Citiri.Imobilizari`, ambele cărți; parametri din evenimentele istorice | AMO/CAS și API Imo portate (097), SC-IMO-01…25 verzi; registrul vechi rămâne în scriere duală și diagnostic |

## Capcane concrete înaintea comutării

1. **Deschiderea**: forma aditivă T-D7 este amendată de 094, aprobată
   explicit de owner. [DES-B2](tr-d7b-deschidere-contract.md) înlocuiește
   soldul bloc cu detaliile și refuză diferențele; mecanismul este implementat.
   Cititorii trebuie să includă aceste postări fără document. StocService
   încă citește registrele; consumul loturilor deschise exclusiv în cub
   depinde de portarea cititorului operațional, nu doar a raportului.
2. **Storno mixt ASM**: `Materializare.Storneaza` reunește postările
   Operare și Transfer într-o singură tranzacție Storno.
   `Postare.InversaDinId/Spatiu` persistă acum identitatea originalului
   (097, D8-B3), separată de `Atribuit`. SC-CIT-04/05 probează ASM mixt și
   BTR pe intrarea comună. Istoricul fără origine și împerecherea rămân de
   verificat înaintea comutării consumatorilor generali.
3. **Acoperirea producătorilor**: la inventarul inițial DEC n-avea declarant,
   iar PIF/CAS/AMO erau la TR-D9. 095 devansează producătorii compleți;
   PIF cu suport obligatoriu este aprobat și implementat (097), împreună
   cu AMO/CAS și cititorul de fișă. Balanța de azi le include încă notele
   din registre.
   Înaintea comutării generale trebuie delimitat explicit domeniul portat;
   nu se livrează o balanță care le omite tăcut și nici o reuniune ad-hoc
   cub + registre. Această dependență nu împiedică construirea cititorilor
   comuni și probarea lor pe tipurile migrate.
4. **Fiscal**: postarea persistă TipTvaId/Sens/Rol, dar raportul de azi
   cere Regim/Cota istorice și data fizică. Se verifică regula versiunii
   tipului TVA și atribuirea temporală la storno/corecție înaintea portării;
   nu se înlocuiește un snapshot cu nomenclatorul curent fără probă.
5. **Reconcilierea ASM**: delimitarea T-r15 este aprobată de owner numai
   pentru regimul dual (D8-B4): Operare ASM exclusă nominal din (a),
   diferențele și numărul documentelor/postărilor raportate în (h). Proba independentă
   D 345/C 301 rămâne autoritatea pentru balanța cubului.

## Ordine propusă pentru execuție

1. Închiderea Deschiderii generice și catalogul ei.
2. Contractul TR-D8 cu domeniul cititorilor, proveniența stornoului,
   acoperirea producătorilor și delimitarea ASM; catalog numeric înaintea codului.
3. Cititorii comuni + test de arhitectură, probe DVI și ASM mixt.
4. Balanță, fișă de cont, jurnal, sold pe unitate; apoi fiscal/SAF-T,
   închidere și reconstrucție, în ordinea dependențelor de mai sus.
5. Auditul compact al deciziei (S-r2), măsurători A/B pe aceeași bază,
   verificări integrale pe ambele profiluri și review advers.

Inventarul nu declară TR-D8 închis și nu schimbă regulile aprobate.
Starea fiecărei portări este consemnată în contract și catalog.

## Prima felie de execuție — D8-B3 (2026-09-24)

Diagnosticul de proveniență este separat de cititor. Seed-ul raportează
numărul problemelor ca avertisment, fără refuz; blocarea activării aparține
feliei care comută primul raport. SC-CIT-07…10 acoperă migrarea, ambiguitatea,
avertismentul și costul în comenzi SQL. Niciun consumator general nu este
declarat portat aici. Corectură de delimitare: 2026-09-25.

Prima portare contabilă trebuie să cuprindă împreună
`ContabilProiectii.Atomi/Balanta/SoldParteneri` și sursa snapshot-urilor
contabile din `SolduriService`, inclusiv reconstrucția. `AtomiCumulati`
concatenează snapshot-ul cu mișcările ulterioare: comutarea numai a
atomilor ar amesteca sursele. Activarea trebuie să diagnosticheze istoricul
incomplet și să trateze explicit snapshot-urile existente înaintea citirii.

Fișa și jurnalul se proiectează apoi pe postări/tranzacții, conform
`nucleu-coordonate-rapoarte.md`: contrapartida nu este obligatoriu unică
într-o tranzacție cu mai multe postări. DTO-urile actuale de perechi,
sortarea/paginarea și securitatea SQL trebuie portate împreună cu API/client.
Acestea sunt dependențe de implementare ale contractului aprobat.

## Stare efectivă după felia contabilă (2026-09-25)

Tabelul de mai sus păstrează inventarul surselor de la intrare. În cod,
`ContabilProiectii` și snapshot-ul contabil au fost comutate împreună.
`Cub.Citiri.Activare` este gardul de pornire a hosturilor; verificarea nu
este în bucla fiecărui raport. `SaftProiectii` folosește deja aceiași atomi
pentru Customers/Suppliers și aceeași balanță pentru GLA; celelalte secțiuni
rămân nominal pe lista de portat.

`Fapte.SolduriLoturi`, evaluarea ieșirilor și gardul zilnic folosesc cubul.
Citirea R necesară absorbției ASM este separată și rămâne din registre.
`DescarcareService`/pinurile FCL și `StocProiectii` sunt portate și verificate.
`SolduriService` scrie și reconstruiește snapshot-ul de stoc din intrarea
comună de lot. `CumulPerioade.Citeste` combină referința și fereastra în
aceeași instrucțiune pentru cele trei domenii. Raportul, FIFO și pinurile
folosesc `Loturi.Cumulate`; evaluarea ieșirii transmite explicit granița
anterioară documentului exclus. Citirea secured și excluderea istorică
fără graniță sigură recitesc postările.
`StocService` citește registrul separat, fără acest snapshot, pentru regimul
dual. Restul consumatorilor enumerați rămâne obligatoriu înaintea închiderii.

### Partide — felia curentă

Raportul general `PartideCuRest`, API/client `/partide`, disponibilul din
`ImperechereService`, candidații NTC/trezorerie și snapshot-ul din
`SolduriService` folosesc cubul. Reconstrucția păstrează cheia completă și
raportează diferențele înaintea rescrierii. `Asignari` rămâne numai suprafață
de diagnostic a legăturilor vechi în review-ul perioadei, nu sursă de rest.
Decizia 101 cere efect verificabil pentru comenzile noi; proveniența
transferului este păstrată pe legătură pentru desfacere exactă.
`Fapte.Sursa` și nominalizarea automată citesc tot cubul, cu sold efectiv
separat de disponibilul minim pe datele ulterioare. Validare: integrale
3.107 bugetar / 4.103 privat OK, HTTP și browser, metadata/OpenAPI fără drift;
dovezile sunt în [review-ul propriu](tr-d8-review-codex.md). 101-r1 este închisă.

### Fiscal — 103 / D8-B8

`Cub.Citiri.Fiscale` este intrarea comună pentru jurnale/decont, D300,
D394 și TaxInformation SAF-T. `FiscalitateService` atribuie perioadele la
scriere, păstrează reperele corecției și confirmă explicit depunerea.
`RegistruTva` rămâne martor în regimul dual, nu sursă a acestor cititori.
SAF-T SourceDocuments în întregime, celelalte componente ale exportului,
reconcilierea și auditul transversal rămân pe inventarul TR-D8. Protecția
valorilor din TaxInformation nu certifică automat toate sumele facturii
exportate de SourceDocuments.
