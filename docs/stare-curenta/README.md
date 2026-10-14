# Atlas.Conta — starea curentă și regulile aplicabile

**Actualizat: 2026-10-15.** Referința este implementarea din workspace,
inclusiv modificările locale. Documentele descriu regulile în forma lor
actuală, organizate pe responsabilități.

## Harta documentației

| Document | Conținut |
|---|---|
| [Documente](documente.md) | Modelul comun, ciclul de viață (culegere, operare, anulare, storno, corecție), data înregistrării, tipurile și documentele generate |
| [Motorul de postare](motor.md) | Declarația fluxului, contarea și dimensiunile, coordonatele, cubul persistat, scrierea serială, precizia |
| [Stoc](stoc.md) | Loturi, FIFO, recepția și diferențele ei, transformarea, inventarul, corecția valorii lotului |
| [Partide și stingere](partide.md) | Partidele deschise, stingerea, împerecherea, nominalizarea, decontul |
| [Imobilizări](imobilizari.md) | Fișa, punerea în funcțiune, amortizarea contabilă și fiscală, ieșirea |
| [Perioade](perioade.md) | Perioada fiscală ca lanț, închiderea, soldurile materializate |
| [Politici](politici.md) | Configurarea profilului, implicitele, proveniența, explicarea configurației |
| [TVA și declarații](tva.md) | TVA la operare, implicitele TVA, închiderea lunară, D300 și D394, citirea fiscală |
| [SAF-T](saft.md) | D406, profilurile L și S |
| [Raportare](raportare.md) | Proiecțiile contabile și de stoc, soldul pe parteneri, explicația deciziei |
| [Nomenclatoare și ANAF](nomenclatoare.md) | Societatea, partenerii, produsele, sincronizarea ANAF |
| [API](api.md) | Salvare și comenzi, securitate și refuzuri, OData, audit, contracte generate |
| [Ecrane](ecrane.md) | XAF Blazor (liste, grile, regim) și React (formulare, selectoare, cache) |
| [Dezvoltare](dezvoltare.md) | Organizarea sursei, model și persistență, migrații, codegen, build |
| [Verificare](verificare.md) | ModelCheck, scenariile, probele, documentația |
| [Import și migrare](import.md) | Import1C, reconcilierea cu legacy |
| [Limite curente](limite-curente.md) | Ce nu face încă o funcționalitate livrată, în forma văzută de utilizator (transversală) |
| [Termeni](termeni.md) | Glosarul: termeni de domeniu, termeni tehnici, cum se citesc identificatorii (transversală) |

## Produsul

Atlas.Conta acoperă contabilitatea și gestiunea, cu documente operaționale,
trezorerie, raportare pe registre și configurarea politicilor. Profilul privat
este sursa principală de cerințe; profilul bugetar este un pachet de seed
funcțional peste același model și același motor.

O bază de date aparține unui client. Profilul contabil și convenția de
rotunjire sunt fixate per bază. Diferențele de profil se exprimă prin date și
validări, fără clase de document distincte pentru fiecare profil.

Backend-ul folosește .NET, XAF și EF Core, cu PostgreSQL. `Module` conține
modelul și regulile comune. WebApi și XAF Blazor folosesc același modul.
Documentele se culeg în XAF Blazor; clientul React, cu DevExtreme, acoperă
citirile, proiecțiile, consolele comenzilor și editorii de politici și
nomenclatoare. (104d)

## Reguli fundamentale

1. Operația economică intră ca document cu sursă, destinație, antet și linii.
   O relație de stingere nu este document; un raport este o proiecție.
2. Identitatea și valorile comune aparțin bazei. Culegerea specifică aparține
   tipului concret. Motorul primește comportamentul specific prin contracte.
3. Cubul de postări e singurul scriitor și singura sursă a soldurilor și a
   raportării. Postările sunt complet rezolvate la operare; citirea nu
   reexecută politica de postare. (110)
4. Structura și mecanismele sunt cod. Politicile aleg parametrii unor
   mecanisme existente și nu descriu câmpuri, ecrane sau expresii executabile.
5. Lotul are identitate și preț fixat la operare. Ieșirile folosesc loturi;
   valoarea deja postată nu se recalculează retroactiv.
6. Sursele externe furnizează date și dovezi pentru reconciliere. Ele nu
   definesc modelul și nu justifică ajustări ascunse ale cifrelor.

[Invarianții proiectului](../invarianti.md) rămân criteriul de verificare a
oricărei extinderi. Paginile de aici aplică aceste reguli la mecanismele
curente și precizează limitele implementării.

## Acoperirea funcțională

Ce e livrat și ce nu, rând cu rând, cu ținta, aria, etapa și scenariile, e
în [catalogul funcționalităților](../functionalitati.md); ce nu face încă o
funcționalitate livrată e în [limitele curente](limite-curente.md). Paginile
de arie descriu regulile, nu acoperirea. (121)

## Întreținerea acestor pagini

O regulă are o singură descriere detaliată, în pagina responsabilității sale;
celelalte pagini trimit la ea. La modificarea comportamentului se actualizează
în același commit regula din pagina ariei, rândul din catalog, limitele
afectate și data documentului. O funcționalitate planificată nu se prezintă
ca disponibilă: rămâne rând `deschisă` în catalog.

Secțiunile unei pagini sunt pe subiect, nu pe felii sau pe date. Limitele
mecanismului, fără efect direct pentru utilizator, stau la sfârșitul paginii
ariei, sub `## Limitele implementării`; cele văzute de utilizator, în
[limitele curente](limite-curente.md). (121)

Aceste pagini nu păstrează alternative respinse, succesiuni de implementare
sau liste de modificări. Fiecare regulă poartă la sfârșit, între paranteze,
identificatorul deciziei din care vine (`(42b)`, `(76-r1)`): puntea spre
„de ce", în [jurnal](../decizii/README.md). Link-urile spre cod indică
locul regulii, nu constituie o a doua specificație.
