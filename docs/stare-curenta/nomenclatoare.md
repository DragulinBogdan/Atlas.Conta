# Nomenclatoare și ANAF

**Actualizat: 2026-10-15.** [Index](README.md)

Societatea, partenerii, produsele și sincronizarea cu ANAF.

## Societatea și nomenclatoarele de bază

`Societate` este configurație singleton editabilă. Seed-ul creează forma
goală și nu suprascrie datele completate. Adresa este structurată, ca la
partener; județul se aplică adreselor din România. Județele și unitățile de
măsură standard sunt nomenclatoare de bază, expuse pentru citire. (72b, 73a, 73b)

Antetul declarațiilor fiscale stă tot pe societate și se culege o singură
dată în XAF: codul CAEN (patru cifre), periodicitatea TVA (lunar, implicit,
sau trimestrial), declarantul (nume, prenume, funcție, adresă), cine întocmește
D394 (persoană juridică sau fizică, denumire, CUI sau CNP, calitate; gol
înseamnă societatea însăși) și acordul de consultare D394. Banca din antetul
D300 e denumirea contului bancar al societății, iar contul e IBAN-ul lui.
(E2-D4)

## Produsul și unitatea lui

Produsul poate avea cod NC de opt cifre și unitate standard. Referința la
unitatea standard coexistă cu textul UM legacy. Normalizarea este explicită
și nu ghicește corespondențe. Rolul și funcția contului sunt date utilizate
în proiecțiile fiscale. (73b, 73c) Unitatea produsului (standard și text) se
poate completa oricând, dar nu se mai schimbă după ce produsul are postări
în cub sau apare pe o linie operată: cantitățile istorice au sensul unității
de la operare. Pentru altă unitate se creează alt produs. (S1-R5)

## Sincronizarea cu ANAF

Sincronizarea ANAF folosește clientul PlatitorTva v9 injectat de host.
Candidații au CUI normalizabil la 2–10 cifre; persoanele fizice/CNP și
partenerii străini nu intră în acest flux. (72c)

Datele canonice privind înregistrarea TVA sunt actualizate din ANAF.
Denumirea și componentele adresei completează golurile; diferențele față de
valori deja culese sunt prezentate. Suprascrierea cere opțiune explicită și
păstrează valorile vechi/noi în jurnal. Un rezultat negăsit nu primește
ștampilă de sincronizare. Importul nu suprascrie clasificarea fiscală
canonică a unui partener deja sincronizat. (72d, 72g)

Endpoint-ul de lot acceptă până la 500 de identificatori și raportează și
pozițiile omise. Clientul extern împarte cererile în loturi de cel mult 100,
cu limitare de ritm în interiorul cererii. Erorile tranzitorii sunt 503,
iar refuzurile de domeniu 422. (72c, 72e)

## Limitele implementării

- Limitarea ritmului ANAF e pe cerere; cererile simultane nu se coordonează.
  (72-r1)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (2; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 72, 73 (amendată de 74, 105).
<!-- /gen:decizii -->
