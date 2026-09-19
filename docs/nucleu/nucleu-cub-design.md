# Nucleul ca ledger n-dimensional (design greenfield)

Stare: **DECIS — decizia 090 (2026-09-20,
`docs/decizii/090-nucleu-cub-de-postari.md`); pașii 1–3 din §11 FĂCUȚI.** Decizia owner-ului la închidere: XAF și React/SPA sunt
ÎNGHEȚATE (se păstrează doar pentru confirmări); ținta e izolarea motorului
pe regulile de aici, luată în ordinea din §11, câte un pas per sesiune. Designul e gândit fără XAF și fără constrângerile
implementării curente; ce transferăm din ea și în ce ordine se tranșează
separat. Se judecă întâi contra `docs/invarianti.md` (I–VI); unde le
amendează, e spus explicit în §10.

## 1. Diagnosticul de la care pleacă

Conceptele curente sunt corecte (structură tipată per document, motorul pe
bază, politica ca date, registre append-only, gard de perioadă, FIFO pe lot
persistat). Două lucruri au fost greșite la nivel de MODEL, nu doar de
implementare, și XAF le-a încurajat pe amândouă:

- **Documentul e centrul de greutate, nu înregistrarea.** Patru registre cu
  patru forme; închiderea, stornoul, snapshot-ul și soldul se scriu de patru
  ori.
- **Polimorfismul prin ierarhia ORM în loc de vocabular semantic.** Motorul
  știe „cum să citească" fiecare tip prin hook-uri; politica e cheiată pe
  tip × câmp.

Legacy-ul (predator → primitor, semn pe cantitate) avea FORMA corectă de flux
și a eșuat pentru că a pus fluxul la INTRARE, ca structură configurabilă. Aici
fluxul stă la IEȘIRE: documentul rămâne tipat și nu se configurează; cubul e
rezultat. Cârjele legacy (indexi, tabele intermediare, pseudo-cache) erau
prețul MOMENTULUI evaluării (regula la citire = interpretare), nu al
generalizării. Motorul care materializează mută regula la scriere
(compilare); citirea doar adună.

## 2. Nucleul (pur, zero dependențe de EF/XAF/HTTP)

```
Postare  = { Coordonate[], Cantitate, ValoareValuta, Valoare,   // măsuri aditive
             Cauza, Atribuit?, Unitate? }                        // proveniență
Tranzactie = Postare[]  balansată per spațiu de conservare
Contract = { Postari[], Decizii[], Ipoteze[] } | Refuzuri
Motor(operand închis) → Contract
```

- **Operand închis**: documentul + tot ce influențează rezultatul, rezolvat
  și înghețat (versiunea politicii, loturile și soldurile lor, starea
  perioadei, a partenerului). DTO, nu entitate urmărită.
- **Validarea = motorul fără reținere; operarea = motorul sub serializare,
  cu ipotezele reverificate.** O singură computație, două ieșiri.
- **Deciziile** (alocarea FIFO, costul, contul rezolvat) și **ipotezele**
  (ce stare s-a citit) sunt în contract explicit; fără ele simularea minte.

Coordonatele (gran = reuniunea interogărilor; enumerate contra rapoartelor
reale în `nucleu-coordonate-rapoarte.md`, 2026-09-19): Cont, Latura (D/C,
doar pe spațiul contabil — stornoul în roșu e legal, rulajele brute sunt
cifre de raport), Data (a înregistrării), Partener, Gestiune (reale —
depozit, casă/bancă, loc — + VIRTUALE: Furnizor, Client, Consum, Producție…),
Produs, Unitate, CodTva (tip@versiune × sens × rol Bază/Taxă),
PerioadaDeclarare, Valuta, Carte (Contabil | Fiscal), Analiză ×6
(CodFunctional, CodEconomic, SursaFinantare, UnitateOrganizatorica, Proiect,
CentruCost), + Cauza (document, linie, tranzacție) / Atribuit ca legături.
Valuta și Atribuit nu au încă raport real care să le probeze (§6 al
enumerării).

## 3. Trei feluri de reguli, nu n

- **Conservarea** — structurală, universală, în cod: cantitatea pe spațiul
  stocului (cu sink-uri), valuta pe spațiul partidelor, valoarea pe
  contabilitate. O tranzacție e o mulțime de mișcări care se anulează.
- **Rezolvarea coordonatelor** — politica, date versionate pe perioadă:
  UNDE merge fluxul (cont, cod TVA, carte). Partenerul se scrie pe postarea
  de terț (contul cu rol), nu „contrapartida pe fiecare latură" ca azi —
  altfel soldurile pe partener nu sunt sume (amendat 2026-09-19, enumerarea
  §5.4).
- **Admisibilitatea** — gardienii: DACĂ fluxul e permis în starea curentă
  (perioadă închisă, stoc negativ, partener blocat). Parte cod, parte date.

Nu există „fapte" ca vocabular; forma unică e mișcarea unei măsuri dintr-o
coordonată în alta.

## 4. Unitatea nominalizată

Lotul, partida deschisă și fișa de imobilizare sunt același concept: unitatea
a cărei fracție ΣValoare / Σmăsură primară e costul, cursul sau valoarea
rămasă.

| Stoc | Terți | Imobilizări |
|---|---|---|
| Lot | Partidă | Fișă (cantitate 1) |
| Cantitate | Sumă în valută | 1 |
| FIFO nominalizează lotul | Împerecherea nominalizează partida | — |
| Cost unitar = ΣVal/ΣCant | Curs partidă = ΣRON/ΣValută | Valoare rămasă = ΣVal |
| Corecție de preț = δ × distribuție | Reevaluare lunară = δ curs × sold | Reevaluare = δ × distribuție; AMO = ieșire doar de valoare |

- **Cantitatea e structurală** (pe lot, mereu, nominalizată FIFO: trasabilitate).
- **Valoarea ieșirii e politică**, redusă la un parametru: unitatea de
  evaluare (lot = FIFO; gestiune × produs sau produs = cost mediu). Costul e
  aceeași fracție peste unitatea aleasă. Dacă două evaluări coexistă,
  valoarea capătă coordonata Carte; cantitatea nu.
- Fișa: registrul de imobilizări = cubul filtrat pe unități de tip fișă.

## 5. Reevaluarea (o singură regulă)

Postare doar de valoare (Cantitate = 0) distribuită peste postările
spațiului stocului ale unității, proporțional cu cantitatea, cu `Atribuit`
per postare. Contrapartida o aduce documentul de corecție prin fluxul lui
propriu (furnizorul lui, vama la DVI). Instanțe: corecția retroactivă de
preț, DVI (taxe vamale pe loturi), transportul pe lot, diferența de curs,
reevaluarea imobilizării (Carte=Contabil, 105; nimic în Carte=Fiscal).

Cazul rulat (intrare 100 @ 10, ieșire 60, corecție la 12): O1 rămâne
neatins; 371 +80, 607 +120, 401 −200; lotul ajunge la 12; sink pe an închis
→ 1174 prin politică (F27-r1 devine rând de politică).

## 6. Rotunjirea e disciplină de motor, nu reziduu

- **TVA**: unitatea legală e documentul × cotă (e-Factura, SAF-T) — acolo se
  DECIDE și se rotunjește taxa; se POSTEAZĂ însă per linie, prin
  `Repartizeaza(taxa documentului, bazele liniilor)`, fiindcă SAF-T cere
  `TaxAmount` obligatoriu pe fiecare linie de factură și de GL (amendat
  2026-09-19, enumerarea §5.3). Σ pe document e exactă prin construcție ⇒
  nimic de reconciliat. Pe facturile primite taxa e DATĂ, validată cu
  toleranță din politică, niciodată recalculată.
- **Valoarea liniei** = round2(cant × preț), decizie la operare; prețul nu e
  măsură.
- **Ieșirea FIFO** se evaluează pe raportul CURENT al unității, în secvență;
  ultima unitate ia restul fără caz special. ⇒ invariant: **cantitate zero
  pe unitate ⇒ valoare zero** (fără valoare orfană). Ține DOAR cu evaluare
  la scriere serializată — argumentul cel mai concret pentru materializare.
- **Distribuția proporțională** naiv per postare lasă bani orfani ⇒
  **repartizare ierarhică** (Hamilton): totalul per coordonată exact, apoi
  restul mai mare în interiorul coordonatei. O singură primitivă
  `Repartizeaza(total, ponderi) → cote` cu Σcote = total, folosită de
  reevaluare, discount global, DVI, transport.
- Modul de rotunjire și scările rămân înghețate per bază (schimbarea lor
  schimbă decizii).

**Reziduul postat apare doar din politică**: diferența de curs (665/765),
diferența de preț, toleranța la împerechere (658/758 sub prag).

## 7. Storno, perioadă, snapshot

- **Storno** = inversul exact al postărilor **cauzate ∪ atribuite**
  documentului, scris ca TRANZACȚIE DISTINCTĂ pe același document (fel
  `Operare | Storno`, cu data și `ScrisLa` proprii) — jurnalul TVA, D394 și
  SAF-T grupează pe ea, nu pe un flag de postare (amendat 2026-09-19,
  enumerarea §5.2). Definiția e completă doar cu `Atribuit` (fără el,
  stornoul după o reevaluare lasă rest pe coordonata greșită — demonstrat pe
  cifre).
- **Perioada e gard**; corecția în perioadă închisă = storno legat +
  document nou datat în perioada deschisă.
- **Snapshot** la granițele de perioadă = sumă parțială cu invariant de
  egalitate. Orice număr persistat e ori fapt (postare), ori sumă verificată.
- **Re-proiecția rămâne unealtă** (rulează motorul peste documentele unei
  perioade deschise, compară cu postările, raportează), nu sursă. Prețul
  acceptat față de legacy: o regulă schimbată nu re-proiectează istoricul.
- **Read model-uri** permise doar deterministe din postări, cu comandă de
  reconstrucție și probă de egalitate.

## 8. Amortizarea (aliniat 2026-09-19)

- **Contabilă**: postată de AMO lunar, Carte=Contabil (ieșire doar de valoare
  din fișă spre 6811/2813).
- **Fiscală**: regulă din politică (metodă, durată din catalog), dar
  **postată** de același AMO în Carte=Fiscal — e decizie (depinde de politica
  versionată și de evenimentele fișei), ajunge în D101 și nu are voie să se
  miște; are nevoie de aceeași disciplină de rotunjire (ultima lună ia restul);
  valoarea fiscală rămasă e raportul unității. Cărțile diverg natural la
  reevaluare.
- **Deductibilă**: CITIRE cu politică (plafon per lună, nereportabil) pe
  granul fișă × lună; nu intră în niciun raport de unitate. Înghețată în
  contractul documentului care o consumă (§9).

## 9. Note contabile și impozitul pe profit (aliniat 2026-09-19)

- **Cubul este jurnalul.** Formula debitor/creditor/sumă e randare (o
  tranzacție multi-picior nu se descompune unic); SAF-T GeneralLedgerEntries
  cere forma cubului. Tabela de note doar ca read model reconstructibil.
- **Nota contabilă** = document tipat, escape hatch, prin motor (balans,
  gard, storno, proveniență). Regulă structurală: nu postează cantitate și nu
  atinge o coordonată cu unitate fără să numească unitatea. Închiderile de
  conturi = note generate de comanda de închidere.
- **Impozitul pe profit** = document generat la închidere (691/4411 sunt
  fluxuri). Contractul lui e Registrul de evidență fiscală:
  `Ajustare { Nume, Fel = Citire | Postare, Valoare, Proveniență[] }`.
  Deductibilul e ajustare de fel Citire, cu detaliu per fișă, înghețat la
  declarare; rectificativa = storno + document nou, contractul vechi rămâne
  proba. Ce poartă valoare rămasă între perioade e UNITATE în Carte=Fiscal cu
  postări: pierderea fiscală de reportat (FIFO pe an, termen, plafon 70%),
  sponsorizarea de reportat, rezerva legală.

## 10. Invarianți provabili (gate-uri) și raportul cu `invarianti.md`

- Nucleul compilează fără EF/XAF/HTTP (test de arhitectură).
- Orice document operat are exact o tranzacție, balansată per spațiu.
- Stornoul e inversul exact al cauzat ∪ atribuit.
- Cantitate zero pe unitate ⇒ valoare zero.
- Snapshot = suma postărilor la graniță.
- O schimbare de politică nu modifică nicio postare din trecut.
- Motorul pe faptele unei baze reale reproduce baseline-ul Import1C.

Față de invarianți: I rămâne (orice postare are document; împerecherea DEVINE
operație, flux între partide — amendament); II se întărește (motorul nu
cunoaște frunzele pentru că nu cunoaște documente, ci contracte); III–VI
neatinse în literă, de reverificat la scrierea deciziei.

## 11. Granițe declarate și ce rămâne de probat

- **Producția multi-nivel**: reevaluarea aterizează pe sink-ul de consum și
  NU se propagă în costul lotului produs (regulă separată, dacă va fi cerută).
- **Tranziția de politică de evaluare** (medie → FIFO) cere o reevaluare de
  aliniere a loturilor la comutare; nescris.
- **Enumerarea coordonatelor contra rapoartelor reale** — FĂCUTĂ 2026-09-19,
  `nucleu-coordonate-rapoarte.md`: „minimal" se ține; „reconstruibil" se
  ține cu patru amendamente structurale scoase de review-ul advers (Latura
  D/C, stornoul ca tranzacție distinctă, taxa postată per linie, partenerul
  pe postarea de terț — aplicate în §2, §3, §6, §7) și cu restul din §5 de
  acolo (analiză ×6, CodTva compus, Gestiune lărgită, Cauza la linie,
  parametrii unității ca evenimente, deductibilul și `Luni` ca limită, linia
  de stoc a FCT ca limită, TipStoc re-cheiat, perioada de declarare
  nerescrisă, partida fără literali de tip, numele `Unitate`). Rămân
  neprobate de rapoarte: Valuta, Atribuit.
- **Fizica** — FĂCUTĂ 2026-09-19, `nucleu-fizica.md` (review advers
  aplicat): forma = un cub logic partiționat LIST pe `Spatiu` (F2), aleasă pe
  STRUCTURĂ (FK per partiție pe `Unitate` polimorfă, scrierea 1,7 contra 5,3
  ms cu 9 FK-uri în loc de 24, 4 indexi per uz + identitate în loc de 24), nu
  pe viteză de citire — la ×10, fără `Sold`, F2 nu e mai rapid decât
  registrele de azi; `Sold` unic pe toate coordonatele cumpără 4–9× cub contra
  cub, dar e de 2,1× mai mare decât snapshot-ul de azi la aceeași graniță și
  nemăsurat contra lui pe același gran; cub 1,5–1,8× discul registrelor; 8
  din 13 rapoarte reconstruite identic, 4 probează doar fidelitatea
  transformării, partidele diferă (ce e „o partidă" — pasul 3); împerecherea
  ca tranzacție datată = propunere netestată. Rămân FZ-r1…r10.
- **Transferul** — FĂCUT 2026-09-19, `nucleu-transfer.md` (review advers cu
  6 MAJOR aplicate; probele în `run-nucleu/transfer/`): partida = unitate
  deschisă de postarea pe contul de terț, cu unitatea numită pe linie și
  partenerul scris din unitate; împerecherea = nominalizare la operare +
  document `Împerechere` cu tranzacție de fel `Transfer` (exclusă din
  rapoartele pe cont); linia FCT postează recepția (NIR-ul conex dispare);
  o singură postare de stoc, `TipStoc` dispare structural; clasificarea
  transferă / rescrie / dispare; ordinea TR-D5…D10 (nucleu pur → declarația
  fluxului pilot → strangler per tip ca dată → citiri → unități și tăiere)
  cu Import1C ca probă supremă. Contractul IM e depășit. TR-D5 = decizia 090
  (2026-09-20). Rămân TR-r2…r12.
