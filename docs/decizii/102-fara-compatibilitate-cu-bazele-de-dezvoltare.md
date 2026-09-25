# 102. Fără compatibilitate cu bazele de dezvoltare până la prima producție

- Data: 2026-09-25
- Stare: aprobată de owner, activă; amendează 092(b)(c), 096(d) și 096-r1,
  100(b), 101(d) și contractul TR-D8 D8-B3 (completarea provenienței și
  refuzul la activare)
- Docs: contractul feliei `docs/nucleu/c102-curatenie-contract.md`;
  `comunicari/2026-09-25-1237-claude-codex-tr-d8-partide-review.md` (D-1, D-4);
  `docs/nucleu/tr-d8-citiri-contract.md`.

## Regula durabilă

(a) Nu există bază de producție. Bazele existente (clonele Privat, bazele de
import, bazele de probă) sunt artefacte ale dezvoltării. O schimbare de model,
motor sau politică nu poartă cost de compatibilitate cu datele persistate
înaintea ei. Asta exclude căile de upgrade, citirea duală a formelor vechi,
diagnosticul istoricului și transformările de date în migrații.

(b) O bază care nu mai corespunde codului se recreează: seed și scenarii,
respectiv importul reluat prin comenzi. Nu se repară prin cod de producție.
Rețeta de recreare este unealtă de dezvoltare, nu cale a hosturilor.

(c) Codul de compatibilitate existent se scoate:
- cheia veche a partidei `(document, cont)` (092b);
- snapshot-urile fără proveniență din cub (`DinCub`, `CITIRE_SNAPSHOT_VECHI`);
- legăturile de împerechere fără efect verificat (`EfectCubVerificat`, ramura
  de desfacere a legăturilor vechi, diagnosticul lor din 101(d));
- completarea provenienței stornourilor vechi prin migrație (D8-B3);
- refuzul istoricului fără unități de partidă (096-r1, 100(b)).

(d) Un invariant verificat până acum pe date vechi devine probă în ModelCheck
dacă scrierea nu îl garantează prin construcție. Hosturile nu scanează
istoria la pornire. Proba se rulează pe calea reală, ca orice probă de motor.

(e) Migrațiile EF rămân canonice pentru schemă (23a), fără transformări de
date. Comprimarea istoricului migrațiilor într-o bază inițială se tranșează
în felia de curățenie (102-r1).

(f) Sursele externe rămân evidență (21). Refuzurile datelor incomplete ale
conectorului de import aparțin importului (091-r4), nu bazei. Ele nu intră
sub (c).

(g) Regula ține până la prima bază de producție. Atunci compatibilitatea
redevine obligatorie, prin decizie nouă, cu migrare de date și probă de upgrade.

## Context și alegere

Review-ul advers al feliei partidelor (TR-D8, 2026-09-25) a găsit două defecte
care există numai din cauza compatibilității:

- D-1: după migrație, snapshot-urile vechi marcate `DinCub=false` opresc ambele
  hosturi la pornire. Reconstrucția există numai în hosturi.
- D-4: legăturile dinainte de 101, citite din SQL fără tracker, pot fi inversate
  de două ori la storno peste închidere.

Codul de împerechere are trei ramuri: legătura veche, asocierea și transferul.
Prima există numai pentru date care nu trebuie păstrate.

Owner-ul: „Nu avem nicio producție, bazele există doar din evoluția dev-ului
nostru." Regula continuă preferința deja aplicată schemei (greenfield, fără
migrare de date). D-1 și D-4 dispar odată cu codul scos. D-2 și D-3 rămân
defecte reale, sub 101-r1.

## Felia de curățenie, înaintea continuării TR-D8

Owner-ul a acceptat o sesiune de curățenie înaintea snapshot-ului de stoc,
din trei motive:

- snapshot-ul de stoc ar replica tiparul `DinCub`;
- corecturile D-2/D-3 ating chiar codul care se simplifică;
- stare-curenta și deciziile descriu azi căi care dispar.

Starea curentă a TR-D8 se comite întâi, ca bază. Felia are contract scurt,
regulă de oprire și review la închidere.

## Restanțe

- 102-r1: scoaterea codului de la (c), cu probele de la (d), și decizia (e).
- 102-r2: alinierea deciziilor, a stare-curenta și a restanțelor cu (c).
- 102-r3: rețeta de recreare a bazelor de dezvoltare și recrearea lor.
