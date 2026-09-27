# 101. Împerecherea cere efect verificabil pe partidă

- Data: 2026-09-25
- Stare: aprobată de owner, activă; restrânge delimitarea duală S-D13; (d) depășit de 102; 101-r1 redeschisă
- Docs: `docs/nucleu/tr-d8-citiri-contract.md` D8-B7; 090(d/e), 092, 100.

## Regula durabilă

(a) O împerechere nouă nu poate consuma un disponibil numai în registrul
legăturilor. Acceptarea cere fie efectul solicitat în cub, fie dovada
nominalizării deja scrise, fără dublare. Refuzul este atomic.

(b) Transferul păstrează contul și partenerul. Lipsa contului/partenerului
comun, disponibilul insuficient și ținta ambiguă sunt refuzuri explicite.
Stingerea între conturi diferite se exprimă prin postările unei note,
nu printr-un Transfer care ar modifica soldul contabil.

(c) Disponibilul și candidații afișați urmează efectele din cub.
Nominalizarea făcută la operare nu se consumă din nou printr-o legătură
manuală ulterioară. Asocierea documentară nu devine o a doua scădere a soldului.

(d) Legăturile istorice fără efect sunt diagnosticate, nu convertite
implicit în postări. Raportul și snapshot-ul nu le scad din partide.
Registrele continuă să fie scrise până la TR-D9.

## Context și alegere

Portarea raportului a găsit două probe vechi în care legătura era acceptată,
dar `Transferuri.Muta` o sărea: PLT 70 legată cu 50 de o notă fără partidă
compatibilă (rest vechi 20, cub 70), respectiv INC 100 legată cu 60 de
debitul unui alt cont (rest vechi 40, cub 100).
Owner-ul a ales A: efect obligatoriu pe partidă și alinierea panourilor.
Păstrarea temporară a panourilor duale, varianta B, nu a fost aleasă.

## Restanțe

101-r1 este închisă la 2026-09-25: implementare atomică, citiri operaționale,
candidați, diagnostic istoric și probe pe ambele profiluri. Validarea și
limitele etapei sunt în `docs/nucleu/tr-d8-review-codex.md`; TR-D8 rămâne deschis.

101-r1 este redeschisă de owner la 2026-09-25, după review-ul advers:
(1) nominalizarea FIFO a notei contabile citește soldul la data notei, fără
zilele ulterioare deja scrise, și poate inversa sensul unei partide stinse
mai târziu; (2) partida proprie deschisă prin Transfer (desfacerea
nominalizării automate) apare între candidați, dar comanda caută ținte numai
în Operare. Ambele cer scenarii înaintea corecturii.
