# Decizia 65 — Pasul 5, felia 8 — Decont

- **Data**: 2026-08-16 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §65
- **Docs**: docs/api/p5-felia-dec-pereche-contract.md

---

**Pasul 5, felia 8 — Decont (DEC) prin API + legătura explicită de pereche
a viramentului — executată** (contract + închidere:
`docs/api/p5-felia-dec-pereche-contract.md`, F8-D1…D15; flux-ancoră complet
în browser pe baza Privat de import, review advers cu 2 defecte de fond
fixate + unul găsit de main peste raport). Ridică excluderea F6-D12 (DEC) și
închide gaura DESCHISĂ la 64k (al doilea picior cules manual genera tăcut un
al treilea document și dubla postarea pe 581). Tranșările:
(a) **DEC: singura schimbare de model e o DECLARAȚIE** —
`DecontDetaliu : ILinieCuPretUnitar` (`PretUnitar` există din 3a) ⇒ zero
migrație pe partea A; consecința e că valoarea și TVA-ul se văd la culegere,
iar normalizarea cantității pro-forma `0 → 1` (32d) se mută și în `Aplica`,
adică devine VIZIBILĂ — exact ce cerea nota din `Comun/Interfete.cs`.
Cele trei blocaje numite la F6-D1 s-au tranșat: aderarea de mai sus, `Cont`
+ `Angajament` + `Repartitor` expuse **ReadOnly** în OData (`Repartitor` pe
BAZA TPT — `ILinieCuPostareExplicita` e tipată pe bază, un lookup pe o
derivată ar minți prin omisiune), și editorul cu grupul colapsat „Postare
explicită". `Numar` e server-owned (DEC are `PoliticaNumerotare` în ambele
profiluri — invers față de FCT); regulile de TVA sunt cele ale F2/F4, fără
a treia variantă. Filtrul `Sumator = false` din lookup-ul de conturi e
AFORDANȚĂ, nu validare: autoritatea rămâne motorul (F8-D14).
(b) **Perechea: o coloană, o parte scrisă, citire derivată.**
`LaturaPerecheId` (FK self pe `DocumentTrezorerie`, `Restrict`) — motorul o
pune pe COPIL la generare, operatorul o culege pe latura manuală. Se scrie o
singură parte fiindcă cealaltă e Operată, iar gardianul de Committing (55a)
refuză scrierea pe ne-Draft: o legătură bidirecțională ar cere o ușă
non-secured pentru un simplu link. **Suprimarea generării întreabă în AMBELE
sensuri** („arăt eu spre cineva SAU mă arată cineva pe mine") — fără
jumătatea a doua, două picioare culese înainte de operare ar genera un al
treilea la operarea PRIMULUI: gaura, mutată cu o zi mai devreme.
(c) **Gardian simetric + avertisment consultativ, nu refuz.**
`VerificaFaraLaturaPerecheOperata` e oglinda exactă a gardianului de grup
conex (pointer ≈ copil, țintă ≈ sursă). Peste el, hook polimorf nou
`Document.MesajeDupaOperare`, consumat de `OperareApi` (poarta unică a
ambelor căi): la operarea unui virament care ȘI-A GENERAT perechea, dacă
există picioare compatibile descoperite, mesajul le enumeră cu calea
corectă. Refuzul e exclus prin construcție — două viramente identice între
aceleași conturi, în aceeași zi, sunt legitime (64k), deci niciun criteriu
de CONȚINUT nu poate distinge cazul.
(d) **Verificarea main-ului a scos două defecte pe care raportele le
ratau.** (1) Legătura RECIPROCĂ (`A→B` cu `B→A`) trecea validarea („nu e un
al treilea document") și producea un deadlock fără ieșire: după operare
fiecare picior îl blochează pe celălalt, iar linkul nu se mai poate șterge.
(2) **Avertismentul era MORT exact pe scenariul canonic**: criteriul era
„fără pereche", iar draftul autogenerat E o pereche ⇒ piciorul ignorat de
operator se excludea singur din listă. Criteriul corect e „fără pereche
OPERATĂ" (581 se închide doar cu al doilea picior operat), cu draftul
blocant NUMIT în mesaj și în candidați.
(e) **Review advers — D1: un pointer STORNAT era numărat ca pereche
definitivă.** Registrele lui sunt inversate (581 redeschis, perechea nu s-a
produs), deci felia avea două criterii pentru aceeași întrebare — gardianul
era singurul corect. Consecințele porneau amândouă de la un storno, unealta
normală de corecție la perioadă închisă (decizia 14): piciorul descoperit
dispărea din candidați ȘI din avertisment, cu remediu IMPOSIBIL la legarea
manuală (un stornat nu se editează și nu se șterge), iar re-operarea sursei
nu mai regenera perechea. Fix: **o legătură contează doar cu celălalt capăt
`Draft` sau `Operat`**, cu roluri despărțite — `PerecheId` (descriptivă, ce
arăt operatorului) vs `PerecheActivaId` (decizională). Gardianul rămâne pe
`Operat`, și nu e inconsecvență: **el apără REGISTRE, suprimarea apără
DRAFTURI**.
(f) **D2: câmpul cules care face panoul să mintă.** `LaturaPerecheId` se
golește cu un click pe copilul autogenerat; panoul, citind exclusiv
`Pereche`, declara apoi „latura pereche lipsește, 581 rămâne deschis" despre
un document cu perechea OPERATĂ și prescria un remediu pe care motorul îl
refuză — urmat mai departe, ducea la a doua postare. Fix: citirea perechii
ține cont și de copilul autogenerat (`DocumentSursaId`), iar ReadDto capătă
`PerecheActiva` (server-computed) pe care ramifică clientul — zero predicat
nou în TS. Corolar aplicat de main: **latura GENERATĂ a altui virament nu se
mai oferă și nu se mai acceptă** ca pereche a unui al treilea document
(aparține transferului ei prin grupul conex, chiar cu linkul golit).
(g) Constatări documentate: picioarele de dinaintea migrației apar
necuplate acolo unde ambele au fost culese manual (o deducere euristică a
perechilor istorice e exact ce refuză 64k); afordanța e deliberat mai
îngustă decât regula pe linkul propriu spre un stornat; lookup-ul XAF al
legăturii e nefiltrat (refuzul de la operare e zgomotos, nu tăcut);
conturile explicite degenerate pe DEC (`X = X`, cont sumator) rămân
afordanță, nu regulă.
