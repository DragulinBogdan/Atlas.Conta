# Decizia 57 — Pasul 5, felia 3 — trezoreria prin API

- **Data**: 2026-08-09 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §57
- **Docs**: docs/api/p5-felia-trz-contract.md

---

**Pasul 5, felia 3 — trezoreria prin API (PLT/INC + imperecheri + plata
automată) — executată** (contract: `docs/api/p5-felia-trz-contract.md`,
F3-D1…D9; flux-ancoră complet în browser pe baza Privat, review advers fără
defecte de fond, fix-urile aplicate). Deblochează STINGERILE în client.
Tranșările:
(a) **Nucleu UNIC generic `TrezorerieApply<T : DocumentTrezorerie>`** (o
implementare, două rute `api/plt`/`api/inc`, filtrarea pe tip prin TPT):
`Numar` SERVER-OWNED (PLT/INC au PoliticaNumerotare — invers față de FCT,
nu e în WriteDto), `Valoare` CULEASĂ pe linie (trezoreria n-are
PregatesteOperare), `TipInstrument` string pe sârmă cu parse pe NUME (nu
numere) ÎNAINTE de CreateObject (refuzul post-create ar lăsa document orfan).
Frunza `DocumentTrezorerieDetaliu` = cele 4 dimensiuni.
(b) **`ImperechereApply`**: Creeaza (traducerea ID→entități la graniță —
serviciul cere entități, abatere documentată de la 42b; scară pe Sumă
înaintea serviciului — motorul rotunjește tăcut), Sterge (liber, 31d),
Stingeri = `StingeriDto` într-un apel (Total/Asignat/Ramas din serviciu +
rândurile cu rol/celălalt în SQL prin CASE). Invarianții rămân în
`ImperechereService.ValideazaCreare` (refuz Plata↔Plata, capacitate per
contrapartidă, NTC stingător — 49a); gardianul re-validează la Committing.
(c) **Proiecția de REST `DocumenteCuRest`** (atomii 42c: unpivot Concat pe
ambele laturi + Brut) = UNION pe 5 ramuri CONCRETE (FCT/FCL/PLT/INC/DEC,
`Tip` literal — TPT fără discriminator), Stare=Operat, Rest>0, filtru pe
contrapartidă; **`ReturClient` EXCLUS deliberat** (override-ul `LiniiCreanta`
ar face GROUP BY-ul universal să divergă de `ImperechereService.Total`);
`Asignat` NEfuzionat pe atom (calea caldă a motorului), consistența
`Rest == Ramas` verificată per rând în ModelCheck.
(d) **Affordances oneste transversal** (F3-D2): `PoateAnula/PoateStorna`
țin cont de imperecheri (helper `AreImperecheri` — oglinda
`VerificaFaraImperecheri`) în FCT/NIR/PLT/INC; TrzReadDto expune
Asignat/Ramas. Închide restanța „affordances mincinoase pe documente
imperecheate" din F2.
(e) **`GenereazaPlata` ridicat în DTO-urile FCT** (excluderea F2 închisă):
+GenereazaPlata/Plata* în Write+Read; plata autogenerată apare în `Copii[]`
cu Tip="PLT", clientul o leagă la `/plt/{id}`; numărul din `PlataNumar`
(calea motorului non-secured — legitim).
(f) OData: `ContPropriu` ReadOnly (conturi proprii = politică), `Angajat`
CRUD. Client: felii `plt`/`inc` (nucleu partajat parametrizat pe {rută,
titlu, laturi ca JSX}), selectorul Partener/Angajat cu deducerea felului
prin sondă OData de existență, `PanouStingeri` (Total/Asignat/Rest din
server, candidați filtrați pe rol, confirmare inline nu window.confirm),
secțiunea „Plată automată" pe FCT. **Regula canonică a vocabularului de
câmpuri, cimentată**: garda `if (e.event)` pe TOATE widget-urile DevExtreme
— formularul e sursa de adevăr, widget-ul raportează exclusiv acțiunile
omului (schimbarea programatică ar șterge câmpurile abia scrise; bug găsit
la smoke, generalizarea regulii F2).
(g) **Fix colateral de FOND** (pre-existent din spike, scos de smoke):
`EsteSters` în gardian — sub ștergerea amânată `IsDeletedObject` e fals la
Committing (GCRecord pus abia la SavingChanges), deci DELETE de imperechere
era judecat ca EDITARE și refuzat pe orice cale secured (inclusiv UI-ul XAF).
(h) Enum labels prin `[XafDisplayName]` pe membri (o sursă pentru XAF +
React); dump-ul enum în ordinea de declarație. Datorii minore documentate
în contract §Închidere (perf proiecție pe baza de import — de măsurat;
editarea plății autogenerate; link 581).
