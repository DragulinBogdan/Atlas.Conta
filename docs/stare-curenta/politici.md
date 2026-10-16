# Politici

**Actualizat: 2026-10-16.** [Index](README.md)

Configurarea profilului, implicitele, proveniența și explicarea configurației.

## Configurarea profilului

Profilul și rotunjirea sunt fixe per bază de date. Politicile sunt date care
parametrizează mecanisme scrise în cod; nu introduc tipuri de documente,
câmpuri, formule contabile executabile sau ecrane dinamice. (4, 51c, 52a)

`TipDocument` este ancora dintre codul documentului și clasa CLR, în relație
unu-la-unu. Crearea și ștergerea sunt refuzate, iar `Cod` și `ClrType` sunt
controlate de server. Implicitul TVA al ancorei este editabil. (20, 81e)

Politicile sunt editabile prin OData în limita permisiunilor și a gărzilor
de domeniu. Rolul `Configurator` (seed-uit, și pe RELEASE) citește tot și
scrie doar tipurile din `Politici.TipuriConfigurabile` — lista explicită a
tipurilor cu proveniență, consumată și de raportul de profil și de
gate-urile de citire; permisiunile rolului se reaplică la fiecare seed. Toate
tabelele de politici au editor React; `Cont` rămâne doar citire pe OData. (81e, 81k, 84c, 84d)

| Configurare | Condiții impuse |
|---|---|
| Tip TVA | Cotă între 0 și 100; referințele pentru implicite trebuie să indice tipuri active (81e) |
| Regulă de contare / politică TVA | Sursa de cont explicită cere cont explicit (81e) |
| Regulă de contare | Filtrul de semn este `-1`, `1` sau absent; un filtru de tip material exclude completarea filtrului de natură (81e). Filtrul doar potrivește: regula nu poartă și nu schimbă semnul valorii, care e al liniei (113) |
| Regulă de stoc | Semnul este `-1` sau `1` (81e) |
| Orice politică sau nomenclator cu proveniență | Fiecare câmp enum poartă un membru definit; enum-urile fără membru 0 nu acceptă valoarea implicită scrisă prin omisiune (84i) |
| Tip TVA | Ștergerea unui tip referit (implicite, ancoră, parteneri, produse, linii de document, registrul TVA) se refuză, ca dezactivarea (84j) |
| Numerotare | Serie nevidă, următorul număr cel puțin 1; formatul opțional folosește argumentele număr și serie (81e) |
| Scadență | Număr de zile nenegativ (81e) |
| Închidere TVA | Cele patru conturi sunt toate completate sau toate absente (81e) |
| Mapare D300 | Destinații de operații; fără mapare simultană la un rând și un strămoș al său; rândul e coerent cu maparea D394 a perechii (69b, 81e, 123 b) |
| Mapare D394 | Operația și sensul trebuie să formeze o combinație permisă; fără operație, motivul excluderii e obligatoriu; ținta e coerentă cu rândurile D300 ale perechii, iar importul se mapează numai ca excludere (71c, 81e, 123 a, b) |
| Politică de amortizare | Un rând per tip material de clasă de imobilizări (natura verificată la commit): contul de amortizare, cheltuiala cu amortizarea și cheltuiala la cedare; fișa fără politică blochează generarea lunară și ieșirea (87d) |
| Regulă de deductibilitate | Categoria fiscală, felul (plafon lunar sau procent), valoarea, valabilitatea de la o dată cu temei; aplicată doar utilizării neexclusive când e marcată așa (87d) |
| Închidere de perioadă | Un singur rând per fel de constatare; gardianul refuză al doilea înaintea indexului unic (F27-D2) |
| Închiderea conturilor | Rândul duce soldul descendenților contului-sursă pe contul-destinație, în ordinea rândurilor, `Lunar` sau `Anual` (numai în decembrie, după cele lunare); destinația nu stă în sursă, își păstrează coordonatele obligatorii (15) prin repartitorul și coordonatele declarate pe rând, iar sursa × momentul e unic (E3-D4) |

Seed-ul închiderii conturilor: privatul închide lunar 6 și 7 în 121, fără
coordonate, și n-are rânduri anuale (121 trece în anul nou); bugetarul
închide lunar 6 și 7 în 121.00.00 și anual 121 și 489 în 117.00.00, cu
repartitorul, unitatea, proiectul, codurile funcțional și economic și sursa
de finanțare, iar 520.02 și 521.02 în 521.03.00, cu codurile funcțional și
economic și sursa de finanțare. (E3-D2, E3-D4, E3-D9)

Unicitățile politicilor se aplică rândurilor active, cu tratarea explicită a
axelor opționale. `Repartitor.Cod` nu este cheie unică globală, deși toate
familiile stau pe aceeași tabelă: datele importate au coduri legitim
repetate între familii. (81c, 81-r4, F28-r2)

## Proveniență și verificarea configurației

`DinSeed` marchează proveniența pe entitățile care implementează
`ICuProvenienta`. Seed-ul îl setează la creare și ALINIAZĂ la fiecare trecere
rândurile care îl poartă (timbrul e proprietate); o modificare efectivă prin
ObjectSpace securizat îl stinge, după care rândul e al clientului și nu se
mai atinge; utilizatorul nu îl poate activa la creare; reseed-ul nu
reactivează timbrul. (81d, 83a, 84a)

Rândul `ICuProvenienta` șters pe ușa securizată se șterge fizic și lasă un
`RefuzSeed` (tip + cheia din indexurile unice, serializată; zecimalele fără
scară, deci `21.0000` citit din bază și `21` din seed dau aceeași cheie). Seed-ul nu
recreează un rând refuzat și îl numără la „șterse”; golurile de mapare
D300/D394 citesc tot refuzurile. Cheia poartă numele coloanelor: un refuz
scris înaintea redenumirii unei coloane din cheie nu mai potrivește rândul,
iar baza care îl ține se recreează (102b). Refuzul se șterge de rolul
`Configurator`, iar rândul revine la următorul re-seed. Raportul de profil nu
are categoria „referință spre un rând șters”: FK-ul refuză ștergerea rândului
referit (valoarea 2 a lui `FelConstatare` e retrasă). Avertismentul D394
pentru partenerul scos din uz e `PartenerInactiv`. (83a, 83j, 104f, 104i)

Marcajul nu este permisiune, jurnal de audit sau criteriu pentru corecții
automate. Rândurile istorice marcate prin backfill nu permit reconstituirea
modificărilor manuale anterioare. Un PATCH fără modificări efective poate
răspunde cu succes fără a constitui o scriere. (81d, 81-r3, 81-r5)

Verificarea profilului citește configurația completă după verificarea
dreptului de citire asupra tuturor tipurilor implicate, inclusiv partener și
produs. Lipsa unui drept produce 403, nu un raport aparent complet din date
filtrate. Raportul semnalează rândurile manuale, tipurile de TVA inactive
referite ca implicite, politicile lipsă și golurile de mapare. Listele de rânduri manuale sunt
limitate la 200 per tabel. Verificarea nu repară datele. (81g)

## Explicarea configurației

`GET api/politici/explica` explică CONFIGURAȚIA pe o linie ipotetică (tip de
document, tip de material, semn, dată, laturi, partener, produs), fără
document: pentru fiecare mecanism — contare, stoc per latură, TVA, conex,
implicitul de TVA, validare, scadență, numerotare — rândul câștigător,
nivelul potrivirii, candidații eliminați cu motivul lor, proveniența
fiecărui rând și o concluzie formată pe server. Potrivirea e aceeași funcție
pură pe care o consumă motorul (`Motor/Potrivire.cs`). Blocul de contare
declară postarea explicită (NTC, DEC) și rezervele — ce ar refuza operarea
peste potrivire: gardul de nivel minim al tipului, natura interzisă de
profilul de validare și, pe un tip care contează prin reguli, linia fără nicio
regulă (`REGULA_CONTARE_LIPSA`, 112e). Dreptul de citire se cere pe toate tipurile
configurabile și pe nomenclatoarele citite, înaintea calculului pe contextul
nesecurizat; o referință inexistentă sau invizibilă e refuzată înaintea
calculului. Panoul `/politici/explica` afișează răspunsul fără niciun calcul
în client și se deschide precompletat din grilele de politici. (84e–h)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (15; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 4, 20, 51, 52, 69, 71 (amendată de 72), 81, 83 (amendată de 84, 86, 104), 84, 87 (amendată de 97), 102, 104 (amendată de 106), 112, 113, 123.
<!-- /gen:decizii -->
