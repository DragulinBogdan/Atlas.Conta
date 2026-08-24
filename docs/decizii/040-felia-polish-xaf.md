# Decizia 40 — Felia polish XAF

- **Data**: 2026-07-23 (primul commit în jurnal)
- **Stare**: activă; (d) SmartLookup REVERTAT la lookup standard (53h); (e) închise de 41
- **Rezumat durabil**: `CLAUDE.md` §40

---

**Felia polish XAF — executată; smoke UI real (browser) + review advers cu
fix-urile aplicate. Backlog-ul 38e închis.** Tranșările:
(a) **Mecanismul „tipul de detaliu declarat per document"**:
`[TipDetaliu(typeof(...))]` pe cele 5 derivate cu detaliu propriu
(FCT/FCL/LDI/DEC/DSC) + `TipDetaliuViewUpdater`
(ModelNodesGeneratorUpdater<ModelDetailViewItemsNodesGenerator>) comută
`IModelMemberViewItem.View` al colecției `Detalii` pe ListView-ul
derivatei — New creează derivata, coloanele arată schema ei. Extenderii
Atlas.DXF (`NewObjectTarget` etc.) NU rezolvau asta (controlează doar
popup-vs-tab) — mecanism propriu în `Module/UI/`. Capcană fixată: la
generare `View` NU e null (XAF leagă implicit view-ul bazei
`Document_Detalii_ListView`) — se suprascrie DOAR default-ul tipat pe
bază; alegerile explicite din Model Editor rămân. Baseline EntityFluent
(`ContaUiBaseline`, discovery manual în `AddGeneratorUpdaters`): ordinea
coloanelor per tip, FK-urile brute ascunse (HideMembers = toate view-urile
tipului, inclusiv export), TipTva/ValoareTva ascunse pe LDI/DSC. Smoke UI:
New tipizat + dialogul liniei cu câmpurile derivatei + `TipTvaImplicit`
precompletat (CAP21) — inclusiv smoke-ul TVA din 37f/38e.
(b) **Validare de culegere**: `RuleRequiredField` (context Save) pe
NAVIGAȚIILE `Document.Predator/Primitor` și `DocumentDetaliu.TipMaterial`
(FK-urile Guid nu pot purta regula — Guid.Empty nu e null), mesaje RO;
New pe linie cu laturile goale → refuz prietenos în locul FK violation
(calea `CommitMasterObject`, exact bug-ul 38e). Review advers: motorul
rulează în ObjectSpace-ul View-ului ⇒ commit-ul operării trece prin
aceleași reguli, iar validarea (în Committing) pica DUPĂ materializare →
„operare-fantomă" în OS-ul viu, comisă de un Save ulterior fără motor.
Fix: `DocumentOperareController.Executa` comite culegerea (cu validare)
ÎNAINTE de motor; + refuz în `ValideazaOperare` de bază pe linii cu
`TipMaterialId` gol (înaintea materializării — 33d). NOTĂ pas 5: obiectele
motorului trec validarea prin fixup/lazy-load pe entități Added
(nedocumentat ca garanție EF) — de re-verificat la tierul Web API.
(c) **Read-only post-Draft pe toate căile liniilor**:
`DocumentDetaliiEditareController` (ListView nested: AllowEdit/New/Delete
când masterul nu e Draft) + geamănul pe DetailView-ul LINIEI (bypass prin
row-click, găsit de review); re-evaluare pe `ObjectSpace.Committed`.
Limitare asumată (ca la gardianul de header): stale multi-tab — Committed
nu se propagă între ObjectSpace-uri; fix-ul de fond ar fi gardian în
Committing pe server, la nevoie.
(d) **Registre + lookup-uri**: `[ForbidCRUD("ListView","DetailView")]` pe
RegistruStoc/Contabil (ascunde acțiunile) + `RegistruReadOnlyController`
de FOND (AllowEdit/New/Delete=false pe orice view — appearance-ul nu se
evaluează pe lista goală și nu oprea Save-ul prin dialogul „modificări
nesalvate"); verificat în UI. `SmartLookupPropertyEditor`
(`AtlasEditorAliases`, opt-in per proprietate) pe 13 navigații mari:
conturile (plan 1.679) de pe TipMaterial/Repartitor/TipTva/RegulaContare/
DecontDetaliu, `DocumentDetaliu.TipMaterial`, `FacturaIesireDetaliu.Produs`,
laturile `Predator/Primitor`; match exact pe DefaultProperty → auto-select,
altfel popup pre-filtrat. `Lot` sărit — nu are DefaultProperty (de decis).
La orice problemă de componentă: revert per proprietate la lookup-ul
standard (scoți `[EditorAlias]`), repararea se face în Atlas.DXF.
**[STARE ACTUALĂ, decizia 53h] SmartLookup a fost REVERTAT integral** la
lookup-ul standard DevExpress (commit `98ce1d0`, memoria
„smartlookup-fallback-standard"): toate cele ~20 de `[EditorAlias]` folosesc
`EditorAliases.LookupPropertyEditor`; paragraful de mai sus descrie starea
de dinaintea revert-ului. `Lot` ARE acum DefaultProperty (`Eticheta`, 53).
(e) **Semnalate, netranșate**: `Imperechere` creabilă din UI prin New
generic — ocolește invarianții `ImperechereService` (31d); de tranșat
(probabil ForbidCRUD + acțiune dedicată, cel târziu la pasul 5). Drafturile
vechi cu linii de tip BAZĂ se afișează acum prin ListView-ul tipizat
(cosmetic; operarea oricum le refuză — 38c). FK-urile brute rămân vizibile
pe tipurile ne-anotate (NIR/BTR/BCS/PLT/INC). `Dimensiuni` se afișează ca
„Castle.Proxies.DimensiuniProxy" pe registre (ToString de adăugat).
**[CORECȚII, decizia 53h]** `Imperechere` — tranșat la 41d (validare la
commit); `Dimensiuni.ToString()` — rezolvat la 41c; **FK-urile brute pe
tipurile ne-anotate erau de fapt DEJA ascunse** de
`ForHierarchy<Document>()/<DocumentDetaliu>().HideForeignKeys()` (41b) —
doar FK-urile PROPRII derivatelor cereau declarație per tip; nota era
inexactă.
