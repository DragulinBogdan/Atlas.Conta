# 100. Partide comerciale în politica bugetară

- Data: 2026-09-25
- Stare: aprobată de owner, activă; amendează 096(b); refuzul istoricului din (b) depășit de 102
- Docs: `docs/nucleu/tr-d8-citiri-contract.md` D8-B6; 090(d), 092, 096.

## Regula durabilă

(a) Politica bugetară activează `Cont.UrmarestePartide` pe conturile
`401.01.00`, `404.01.00` și `411.01.01` cu `DinSeed=true`.
`RolTert` rămâne neschimbat. Conturile manuale nu se aliniază implicit.

(b) Activarea citirilor refuză istoricul contabil fără unități de partidă
pe conturile urmărite. Seed-ul nu reconstruiește postări sau stingeri;
soldul unui cont nu este suficient pentru deducerea partidelor istorice.
Diagnosticul identifică pozițiile care cer remediere înaintea comutării.

(c) Lanțurile FCT/PLT, FCL/INC, NTC și deschiderea se probează numeric pe
ambele profiluri, inclusiv transferul și inversarea stingerii. Identitatea
rămâne document/cont/partener (092), independentă de rolul comercial SAF-T.

## Context și alegere

Cele trei conturi aveau urmărirea dezactivată în seed-ul bugetar. FCT 100
stinsă prin PLT 40 păstra soldul creditor 60, dar nu avea o partidă în cub.
Owner-ul a aprobat varianta A din D8-B6: activarea celor trei conturi și
refuzul istoricului fără unități. Varianta B, amânarea cititorului bugetar,
nu a fost aleasă.

## Restanțe

Acoperirea și probele sunt urmărite prin 100-r1. Diagnosticul istoriei
continuă restanța 096-r1; nu este o autorizare de rescriere a istoriei.
