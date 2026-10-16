# 123 — D300 și D394 se verifică încrucișat pe aceleași fapte: maparea D394 e completă și coerentă cu decontul, iar diferențele dintre declarații se explică

- Data: 2026-10-16
- Stare: activă
- Execuție: branch-ul `d123-punte-d300-d394`; owner-ul a aprobat la 2026-10-16 verificarea încrucișată, cu refuz la salvare pe maparea incoerentă; (d) și (e) pe branch-ul `d123-r1-n9-excludere-d300`, aprobate de owner la 2026-10-16
- Arii: tva, politici, ecrane
- Amendează: 122 (b)
- Închide: 123-r1
- Docs: catalogul `docs/nucleu/scenarii/DECL.md` (SC-DECL-16…20); cercetarea formularelor `docs/api/d300-structura-2026.md`, `docs/api/d394-structura-2026.md` (§„Ce NU intră în `op1`”, §5.5)

## Regula durabilă

**D300 și D394 citesc aceleași fapte fiscale. Le despart fie o regulă a legii (perioada, operațiunile pe care D394 nu le cere), fie o greșeală de configurare. Regula legii se explică pe ecran, iar greșeala de configurare se refuză la salvare.**

(a) **Excluderea din D394 e explicită.** O pereche tip de TVA × sens are în `MapareD394` una din trei stări: tip de operațiune; exclusă, cu `Tip` gol și `Motiv` obligatoriu; nemapată. Faptele perechii excluse ies în `Excluse`, cu motivul, și nu opresc fișierul. Faptele perechii nemapate ies în `Neincluse` și îl opresc (E2-D5). Excluderea are prioritate față de verificările de partener: o operațiune pe care D394 nu o cere nu are nevoie de partener declarabil. Importul nu are ramură proprie în proiecție; se exclude prin mapare, iar (b) apără regula. Seed-ul scrie excluderile profilului ca rânduri `DinSeed`.

(b) **Coerența mapărilor se verifică la salvare.** Pentru aceeași pereche tip de TVA × sens, ținta D394 trebuie să fie permisă pentru fiecare rând D300 pe care îl alimentează perechea, după matricea de mai jos. Matricea ține de lege, deci e cod, lângă nomenclatorul rândurilor. Tipul de TVA de import (`DeImport`) se mapează în D394 numai ca excludere, oricare i-ar fi rândurile D300 (24, 25 sau 7 la amânarea plății). O mapare incoerentă se refuză la salvare, pe ambele uși (regula XAF și gardianul de editare), oricare dintre cele două mapări s-ar salva. Seed-ul aplică aceeași regulă. Perechea mapată pe un singur formular nu se refuză: faptele ei opresc exportul formularului care nu o are (E2-D5).

| Rând D300 | Ce conține | D394 permis |
|---|---|---|
| 1, 2, 3, 3.1, 4, 5, 5.1, 6, 7.1, 8 | intracomunitare (D390) și locul în afara României | exclusă |
| 7 | taxare inversă cu beneficiarul din România, altele decât intracomunitare | C |
| 9, 10, 11 | livrări taxabile interne | L, LS |
| 12, 12.1, 12.2 | achiziții cu taxare inversă internă | C |
| 13 | livrări cu taxare inversă internă | V |
| 14, 15 | scutite, cu sau fără drept de deducere | exclusă |
| 16 | regularizări ale taxei colectate | L, LS, C |
| 17, 18 | vânzări la distanță și servicii electronice (OSS) | exclusă |
| 24, 25 | achiziții taxabile interne și taxa plătită în vamă | A, AS |
| 29, 29.1 | achiziții scutite sau neimpozabile | AS sau exclusă |
| 33 | regularizări ale taxei deduse | A, AS, C |

(c) **Puntea D300 → D394 e informare pe ecranul D394.** Puntea e calculată pe sens × tip de TVA × cotă, din faptele celor două perioade. Pornește de la faptele din perioada D300, scade ce e numai în perioada D300 și adună ce e numai în perioada D394. Rezultatul se împarte în declarat, exclus (cu motivul) și neinclus (cu cauza). Decalajul de perioadă își numește documentele. Puntea se închide prin construcție: un rest nenul înseamnă un defect de cod, deci e probă în ModelCheck, nu mesaj pe ecran. `DiferenteRotunjire` rămâne rotunjirea internă a D394 (rânduri rotunjite pe partener față de totalul rotunjit o dată) și nu compară cu D300.

(d) **Excluderea din D300 e explicită, ca la D394.** O pereche tip de TVA × sens are în `MapareD300` una din trei stări: rânduri de operațiuni; exclusă, cu o singură mapare fără rând și cu `Motiv` obligatoriu; nemapată. Excluderea e pentru operațiunea reală pe care decontul n-o cere, iar faptele ei ies în `Excluse`, cu motivul, fără să oprească fișierul. Combinația care nu poate exista rămâne nemapată: faptul ei e o eroare și oprește fișierul (E2-D5). Excluderea e singura mapare a perechii, iar perechea exclusă din D300 se exclude și din D394; ambele reguli se verifică la salvare, ca (b). Profilul privat exclude livrarea în afara sferei TVA (`NIM`) și factura furnizorului de import (`IMP` pe achiziție, 83g).

(e) **Cota de 9% pe achiziție intră pe rd. 33.** Zona deductibilă a formularului 2026 n-are rând cu 9% (OPANAF 174/2026 a eliminat rd. 25.1), iar instrucțiunile rd. 33 cuprind „operațiuni pentru care exigibilitatea taxei intervine în perioada de raportare, dar pentru care se aplică cota de 24%, 20%, 19%, 9% sau 5%”. `N9` pe achiziție se mapează pe rd. 33 și rămâne `A` în D394, cu cota 9. Achiziția există legal: cota de 9% de până la 31.07.2025 ajunge în 2026 prin ajustări art. 287, regularizări art. 291 alin. (6) și facturi primite târziu, iar importul 1C o aduce pe `N9`. Cota tranzitorie pentru locuințe nu e deductibilă: cumpărătorul e persoana fizică (Legea 141/2025 art. III alin. (1)) sau primăria (alin. (4)). Termenul ei e 30.09.2026 (Legea 161/2026 art. I, MO 642/04.08.2026), deci `N9.ValabilPanaLa` = 30.09.2026. Matricea (b) nu se schimbă.

## Context

Owner-ul a aprobat la 122 (b) ca diferențele dintre D300 și D394 să apară pe ecran. Implementarea a arătat numai rotunjirea pe partener, calculată în D394, fără nicio legătură cu decontul. O neconcordanță reală nu era semnalată. Ambele proiecții citesc `Fiscale.Fapte`, dar ajung la cifre diferite pe trei căi independente: două tabele de mapare editate separat (`MapareD300`, `MapareD394`), două perioade (`PerioadaD300` pe înregistrare și exigibilitate, `PerioadaD394` pe primire și emisie, 103) și excluderile D394.

Analiza a scos la iveală un defect latent al E2. Perechile lăsate deliberat nemapate în D394 (scutitele, neimpozabilele, importul) cădeau în `Neincluse`, iar E2-D5 oprește fișierul la orice rând neinclus. O societate cu o livrare scutită sau cu o declarație vamală în lună nu putea exporta D394. Lista nemapatelor deliberate exista numai în seed, iar proiecția nu o citea.

## Tranșări

- Verificare pe mapări și pe fapte, nu tabel de corespondență rând D300 ↔ tip D394 pe cifre: D300 cuprinde operațiuni pe care D394 nu le cere, iar perioadele diferă legal. O comparație de totaluri ar fi semnalat diferențe legitime ca erori.
- Maparea incoerentă e refuz la salvare, nu avertisment: e o greșeală de configurare, nu o diferență de date (owner, 2026-10-16).
- Excluderea ca stare a mapării (`Tip` gol cu `Motiv`), după precedentul `PoliticaMiscareSaft.CodMiscare`. Lista din seed nu mai e citită de nimeni altcineva; o coloană `Exclus` separată de `Tip` ar fi permis combinația „tip și exclus”.
- Matricea e cod, nu politică: perechile permise decurg din formulare, ca rândurile D300 (`[ForbidCRUD]`). O versiune nouă de formular își aduce matricea.
- Perechea mapată pe un singur formular nu se refuză la salvare: cele două mapări se culeg separat, iar un refuz ar cere salvarea lor în același commit. Exportul prinde perechea prin E2-D5.
- Puntea nu blochează (principiul 21): decalajul de perioadă și excluderea sunt ale legii.
- (d) Excluderea D300 ca mapare fără rând, nu ca listă în seed citită de proiecție: aceeași formă ca (a), editabilă fără release. `MapareD300` are n rânduri per pereche, deci excluderea e un rând cu `RandId` NULL, unic per pereche prin indexul `NULLS NOT DISTINCT`, iar regula „singura mapare a perechii” oprește combinația „exclusă și pe rânduri”.
- (d) Combinațiile imposibile (`NED21` și importul pe livrare) rămân nemapate, nu excluse: un fapt al lor e o eroare de culegere și trebuie să oprească fișierul, nu să dispară cu un motiv.
- (e) N9 pe achiziție nu se refuză la culegere: ar fi blocat regularizările legale pe cota istorică de 9%.

## Ce rămâne deschis

Nimic: 123-r1 e închisă prin (e).

## Istoric

- 2026-10-16: (d) și (e) adăugate la tranșarea 123-r1, aprobată de owner. Sursele: OPANAF 174/2026, anexa 2, rd. 24, 25 și 33; Legea 141/2025 art. III; Legea 161/2026 art. I. Proba e SC-DECL-19 și SC-DECL-20. Semnalarea (d) a venit din aceeași analiză: perechea nemapată deliberat (NIM pe livrare, IMP pe achiziție) oprea D300, la fel cum oprea D394 înainte de (a).
- 2026-10-16: scrisă după semnalarea că ecranul aprobat la 122 (b) nu compara D300 cu D394; owner-ul a cerut verificarea încrucișată, cu refuz la salvare pe maparea incoerentă.
