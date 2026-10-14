# 120 — Lanțul de migrații e de unică folosință până la „rotund”; recomprimarea lui este a șaptea condiție

- Data: 2026-10-13
- Stare: activă
- Execuție: hotărâtă de owner 2026-10-13, la discuția redenumirii coloanei `Latura` de pe `Postare`. Amendează 091 (g); precizează 102 (e) și 110 (i). Recomprimarea finală nu este făcută
- Arii: dezvoltare, verificare
- Amendează: 91 (g)
- Docs: `docs/stare-curenta/dezvoltare.md`; precedentele: 102-r1 (C102), D9-D6 din `docs/nucleu/tr-d9-taierea-contract.md`, C111-D7 din `docs/nucleu/c111-coordonate-contract.md`.

## Regula durabilă

**Până la prima bază care trebuie să supraviețuiască, lanțul de migrații descrie drumul, nu schema. La „rotund” îl înlocuiește o singură migrație de bază.**

(a) **Până atunci, migrații incrementale.** O felie care schimbă schema adaugă o migrație obișnuită, fără transformări de date (102 e). Bazele existente o primesc prin migrație sau se recreează (102 b). Lanțul nu se recomprimă la fiecare felie.

(b) **A șaptea condiție a lui „rotund”** (091 g): lanțul este recomprimat într-o singură migrație de bază, generată din model, plus SQL-ul pe care modelul nu-l declară. Nu rămân în el redenumiri, corecții sau adăugiri din evoluția PoC-ului.

(c) **Proba.** Schema unei baze construite din lanțul vechi și schema uneia construite din migrația nouă sunt identice. Comparația este pe schemă, nu pe date.

(d) **Termenul.** Recomprimarea se face înaintea feliei de migrare (091 f). De la prima bază care trebuie păstrată, lanțul este istoric și nu se mai recomprimă.

(e) **SQL-ul pe care modelul nu-l declară stă într-un singur loc, pe care migrația îl cheamă.** Se mută acolo cel târziu la recomprimarea finală, ca regenerarea părții EF să nu ceară transcrierea lui de mână.

## Context

Lanțul a fost recomprimat de trei ori în `InitialCreate`, de fiecare dată cu bazele recreate: la C102 (102-r1), la tăiere (D9-D6) și la C111 (C111-D7). După C111, pasul 4 al TR-D9b a adăugat `OriginiPartide` ca migrație incrementală. Cele două regimuri alternau fără o regulă care să spună când se aplică fiecare; 102 (e) tranșase comprimarea o singură dată, pentru felia de atunci.

Întrebarea a apărut la redenumirea membrilor `Latura` în `Sens`. Coloana de pe `Postare` este scrisă de mână în `InitialCreate`; o migrație de redenumire ar lăsa schema canonică să spună numele vechi și apoi să-l corecteze. Owner-ul a numit asta scurgere a evoluției din greenfield în proiectul final și a cerut curățenia după PoC, ca regulă.

## Tranșări

1. **Recomprimare la fiecare schimbare de schemă.** Respinsă: cere la fiecare felie recrearea bazelor de referință, a clonelor de măsurare și a bazelor de import.
2. **Migrațiile incrementale rămân în proiectul final.** Respinsă: lanțul canonic ar purta redenumirile și corecțiile PoC-ului.
3. **Incrementale până la „rotund”, apoi o singură recomprimare** (aleasă, owner).
4. **Unde stă obligația.** Condiție a lui „rotund”, nu rând în registrul restanțelor (owner): un singur loc ține locul câte unei restanțe pentru fiecare redenumire.
5. **SQL-ul scris de mână, inline sau separat.** Separat (owner): cele trei recomprimări de până acum l-au transcris de fiecare dată în `InitialCreate`.

## Ce rămâne deschis

- **Proba din (c) nu există încă** ca unealtă; se scrie odată cu recomprimarea.
- **Mutarea din (e) nu este făcută.** SQL-ul stă azi în patru blocuri în `InitialCreate` (tabela `Postare` partiționată cu indecșii ei, `cub_partida_id`, `PostareVizual`). Forma locului (fișiere `.sql` încorporate sau o clasă chemată din migrație) se alege la mutare.
