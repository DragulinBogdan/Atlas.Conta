# Atlas.Conta — starea curentă și regulile aplicabile

**Actualizat: 2026-09-22.** Referința este implementarea din workspace,
inclusiv modificările locale. Documentele descriu regulile în forma lor
actuală, organizate pe responsabilități.

## Tranziția în curs

**Implementat azi:** cubul se scrie lângă registre pentru BCS, FCT, PLT,
INC, BTR, FCL și DSC (privat). Citirile produsului și snapshot-urile sunt
încă pe registre; regulile de mai jos despre registre și prețul lotului
descriu această implementare, nu ținta finală. Declarantul BCS evaluează
ieșirea pe raportul curent al lotului (90, TR-D7a).

**Ținta decisă:** cub canonic, cititori comuni la TR-D8, eliminarea
registrelor la TR-D9; scenarii independente ca gate (091). Nicio capacitate
planificată nu este prezentată ca implementată doar pentru că are decizie.

**Lucrarea în curs:** primul lot [BCS](../nucleu/scenarii/BCS.md) are scenarii
independente verificate pe ambele profiluri; cititorii pe cub și compensările
după reevaluare rămân deschise. `CLAUDE.md` §Stare indică pasul; contractul feliei
precizează domeniul și oprirea; catalogul de scenarii arată acoperirea.
Deciziile explică ținta, probele și codul dovedesc starea curentă. O
contradicție se consemnează explicit, nu se rezolvă prin alegerea tacită a
sursei convenabile.

## Harta documentației

| Document | Conținut |
|---|---|
| [Domeniu și operare](domeniu-si-operare.md) | Modelul documentelor, registre, dimensiuni, stoc, corecții, relații și particularitățile tipurilor |
| [Politici și fiscalitate](politici-si-fiscalitate.md) | Configurare, implicite, proveniență, TVA, închidere lunară, raportare, ANAF și SAF-T |
| [API și client](api-si-client.md) | Contracte REST/OData, securitate, audit, formulare, cache, căutare și ecranele disponibile |
| [Dezvoltare și validare](dezvoltare-si-validare.md) | Organizarea codului, persistență, seed, codegen, verificări și import |
| [Limite curente](limite-curente.md) | Funcționalități neacoperite și limite ale mecanismelor implementate |

## Produsul

Atlas.Conta acoperă contabilitatea și gestiunea, cu documente operaționale,
trezorerie, raportare pe registre și configurarea politicilor. Profilul privat
este sursa principală de cerințe; profilul bugetar este un pachet de seed
funcțional peste același model și același motor.

O bază de date aparține unui client. Profilul contabil și convenția de
rotunjire sunt fixate per bază. Diferențele de profil se exprimă prin date și
validări, fără clase de document distincte pentru fiecare profil.

Backend-ul folosește .NET, XAF și EF Core, cu PostgreSQL. `Module` conține
modelul și regulile comune. WebApi și XAF Blazor folosesc același modul;
clientul operațional este React cu DevExtreme.

## Reguli fundamentale

1. Operația economică intră ca document cu sursă, destinație, antet și linii.
   O relație de stingere nu este document; un raport este o proiecție.
2. Identitatea și valorile comune aparțin bazei. Culegerea specifică aparține
   tipului concret. Motorul primește comportamentul specific prin contracte.
3. Registrele sunt sursa soldurilor și a raportării. Rândurile sunt complet
   rezolvate la operare; citirea nu reexecută politica de postare.
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

| Arie | Disponibil în implementare |
|---|---|
| Documente | FCT, FCL, NIR, DSC, BTR, BCS, LDI, PLT, INC, DEC, NTC, ASM, RLF, RDC, DVI, PIF, CAS |
| Închidere TVA | Previzualizare, generare, regenerare și comenzi asupra rezultatului ITV |
| Imobilizări | Fișa cu situația la dată și registrul, punerea în funcțiune (intrare, modernizare, revizuire), ieșirea, amortizarea lunară generată cu cifra contabilă, fiscală și deductibilă, politica de amortizare, regulile de deductibilitate și catalogul HG 2139/2004 |
| Raportare | Sold stoc, balanță, balanță pe plan, fișă de cont, registru-jurnal, jurnale TVA, decont TVA, D300 și D394 |
| Fișiere fiscale | SAF-T L și S, cu sumar, avertismente, reconciliere și XML |
| Nomenclatoare în React | Parteneri, produse, societate și sincronizarea individuală a partenerului cu ANAF |
| Configurare în React | Implicite TVA, tipuri TVA, implicitul tipului de document, mișcări SAF-T, scadențe, numerotare, închidere TVA, politici de amortizare, reguli de deductibilitate, verificarea profilului și istoric de audit |
| Import | Import1C prin motor, legături de migrare și reconciliere; conectorul legacy este prototip |

Existența unei rute nu înseamnă CRUD complet: DSC se generează din factură,
iar ITV și AMO din comanda lunară. Particularitățile fiecărui tip sunt descrise în
[domeniu și operare](domeniu-si-operare.md). Acoperirea formularelor fiscale
și modulele absente sunt precizate în [limitele curente](limite-curente.md).

## Întreținerea acestor pagini

O regulă are o singură descriere detaliată, în pagina responsabilității sale;
celelalte pagini trimit la ea. La modificarea comportamentului se actualizează
direct regula, acoperirea și limitele afectate, precum și data documentului.
O funcționalitate planificată nu se prezintă ca disponibilă.

Aceste pagini nu păstrează alternative respinse, succesiuni de implementare
sau liste de modificări. Fiecare regulă poartă la sfârșit, între paranteze,
identificatorul deciziei din care vine (`(42b)`, `(76-r1)`): puntea spre
„de ce", în [jurnal](../decizii/README.md). Link-urile spre cod indică
locul regulii, nu constituie o a doua specificație.
