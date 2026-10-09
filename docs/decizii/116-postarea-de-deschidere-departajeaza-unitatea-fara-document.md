# 116 — Unitatea fără document se departajează pe postarea ei de deschidere; ID-urile postărilor urmează ordinea tranzacției

- Data: 2026-10-09
- Stare: **activă, amendată de 117** ((a): ID-urile postărilor nu mai poartă ordine; (b), (c): departajează ordinalul postării de deschidere, nu ID-ul ei); hotărâtă de owner 2026-10-09. 116-r1 e închisă prin 117. Amendează 114 (a) și (d); închide 114-r1 și 107-r11.
- Docs: `docs/stare-curenta/domeniu-si-operare.md`, `limite-curente.md`; catalogul `docs/nucleu/scenarii/CITIRI.md` (SC-CIT-113).

## Regula durabilă

**Ordinea în care deschiderea își primește unitățile e ordinea în care FIFO le consumă la aceeași dată.**

(a) **Scrierea.** Postările unei tranzacții primesc ID-uri crescătoare în ordinea tranzacției (`Materializare.Scrie`). ID-ul rămâne Guid v7 generat la scriere; nu se adaugă coloană și nu se schimbă nicio coordonată a postării.

(b) **Ordinea FIFO** a unităților este: data deschiderii, ID-ul documentului deschizător, ID-ul postării de deschidere, apoi identificatorul unității. Postarea de deschidere contează numai între unitățile fără document deschizător, care rămân primele la data lor (114 (a)).

(c) **Nucleul primește postarea pe candidat** (`Disponibil.Deschidere`), ca originea (114 (b)): identitatea unității nu se schimbă. Motorul o citește din cub prin `Cub.Citiri.Partide.Deschideri`, numai când la FIFO vin cel puțin două partide fără document.

(d) **Contractul celui care deschide.** Ordinea în care dă partidele deschiderii e ordinea lor FIFO la egalitate de dată, deci trebuie să fie aceeași la fiecare rulare pe aceeași sursă. Import1C le dă ordonate pe cheia sursei (simbol, partener, document 1C).

(e) **Loturile inițiale nu sunt acoperite.** Lotul inițial e un rând creat înaintea deschiderii, de cel care o cere; la aceeași dată, departajarea lui finală rămâne identificatorul lotului (116-r2).

## Context

După 115, două importuri ale aceluiași binar mai difereau într-un singur loc pe ianuarie 2025: 60 lei care cădeau pe una sau pe alta dintre două facturi ale aceluiași furnizor, născute în aceeași zi și nenumite de sursă (114-r1).

Motorul e determinist pe document: pe aceeași bază, același document dă aceleași postări. Diferența apărea numai între două baze. FIFO nu consumă în ordinea sosirii, ci sortează candidații pe chei explicite. Pentru cele două partide, data și documentul erau egale, așa că decidea identificatorul partidei. Identificatorul e SHA-256 peste referință, cont și partener (092); referința vine din cheia 1C și e stabilă, dar contul și partenerul sunt Guid-uri generate la fiecare import.

Ordinea de intrare a deschiderii era deja stabilă în conector, dar nu se păstra: postarea de deschidere n-are document, n-are linie, `Pereche` e interzisă pe deschidere, iar ID-urile Guid v7 scrise în aceeași tranzacție nu aveau ordine (114 (d)).

## Tranșări

1. **Ordinea postării ca coloană pe `Postare`** (ordinalul în tranzacție). E forma pe care owner-ul o consideră corectă pe termen lung. Amânată: e o coloană nouă pe postare, iar coordonatele postării nu se ating cât 111 e propusă (116-r1).
2. **ID-ul postării de deschidere** (aleasă acum, owner). Nu cere schemă și continuă 114: ordinea stă în ID-urile noastre. Costă o citire în plus la notele care aleg FIFO între cel puțin două partide fără document.
3. **Referința ca departajare.** Nealeasă: referința nu e persistată, ci intră numai în hash-ul identificatorului.
4. **ID-uri stabile de cont și de partener la import.** Nealeasă: ar face hash-ul reproductibil, dar ordinea ar rămâne una întâmplătoare, iar schimbarea ar fi a întregului conector (091-r4).

## Probe

- Nucleul: 192 de teste; `FaraDocumentPostareaDeDeschidereBateId`.
- Catalog: SC-CIT-113 pe ambele profiluri. Cu citirea postării de deschidere oprită, scena pică.
- ModelCheck integral: 3.572 bugetar / 4.853 privat OK, zero FAIL (`run-verificari/20261009-143216-304/`).
- Două importuri pe ianuarie 2025 cu același binar, pe baze noi (`.Flax.ProfO1`, `.ProfO2`):

| | după 115 | cu 116 |
|---|---|---|
| Linii diferite în raportul de reconciliere între două rulări | 4 | 0 |
| Legături diferite (împerecheri, stingeri pe partide inițiale) | 0 | 0 |
| Tranzacții / postări / împerecheri | 18.770 / 81.774 / — | 18.770 / 81.774 / 2.152, aceleași |
| Partidele pe chei naturale (amprentă) | — | aceeași |
| Verificări picate | 10 | 10 în ambele |
| Durata importului | 441 s | 441 s și 430 s |

Cei 60 de lei cad în ambele rulări pe aceeași factură (`…AD4E82D11438`, prima în ordinea sursei). Rămân diferență față de sursă, etichetată 107-r9: rândul 1C nu numește documentul.

Duratele sunt pe baze diferite, deci numai orientative: citirea în plus nu se vede în durata importului.

În jurnalul importului mai diferă numai numele bazei, ID-ul tranzacției de deschidere și un eșantion de adrese afișat de proba cititorilor, care nu e ordonat. Niciuna nu intră în raportul de reconciliere.

## Ce rămâne deschis

- **116-r1** — ordinea postării în tranzacție persistată pe `Postare`, în locul ordinii purtate de ID. Se judecă împreună cu 111.
- **116-r2** — loturile inițiale cu același produs, gestiune, cont și dată se departajează pe identificatorul lotului, dat de cel care le creează. Pe ianuarie 2025 nu s-a văzut nicio diferență între două importuri: 1C numește lotul, iar loturile inițiale își poartă data reală.
