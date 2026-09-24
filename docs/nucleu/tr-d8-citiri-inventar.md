# TR-D8 — Inventarul de intrare și ordinea portării

2026-09-24. În lucru: cititorul fișei IMO este portat, intrarea contabilă
comună este probată; rapoartele generale nu sunt comutate.
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
