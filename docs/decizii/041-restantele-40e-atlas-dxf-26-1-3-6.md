# Decizia 41 — Felia „restanțele 40e

- **Data**: 2026-07-24 (primul commit în jurnal)
- **Stare**: activă; (e) Atlas.DXF e PINNAT, nu flotant (53h)
- **Rezumat durabil**: `CLAUDE.md` §41

---

**Felia „restanțele 40e → Atlas.DXF 26.1.3.6" — executată; genericul
promovat în bibliotecă, consumul în Conta; smoke UI browser + review advers
cu fix-urile aplicate. Restanțele 40e închise. Tranșările:**
(a) **Enforcement de fond `ForbidCRUD` în bibliotecă**:
`ForbidCrudCapabilitiesController` (Atlas.DXF.Core, înregistrat cu
`[IncludeController]` pe modulul Blazor — pattern-ul cross-assembly
existent) taie `AllowEdit/New/Delete` pe view-urile din Context-ul
atributului; potrivirea oglindește algoritmul DevExpress AppearanceContext
(tokenizare `,`/`;`, „Any" INVERSEAZĂ — fidel sursei, inert pentru uzul
ForbidCRUD). `RegistruReadOnlyController` din Conta a murit. Limitare
documentată: consumator non-Blazor al Core primește doar hiding-ul.
(b) **`HideForeignKeys()` pe EntityFluent + HierarchyFluent** (convenția:
scalar `{Nav}Id` Guid/int/long cu navigație pereche; orfanii rămân —
`Lot.LinieIntrareId` intenționat vizibil). În Conta
(`ContaUiBaseline.AscundeFkuriBrute`): ierarhiile Document/DocumentDetaliu
+ FK-urile proprii per tip (inclusiv header-ele descoperite
`FacturaIntrare.PlataContPropriuId`, `FacturaIesire.GestiuneDescarcareId`).
Nuanțe: descoperirea Hierarchy vede DOAR membrii bazei (derivatele cer
For<T>); expansiunea owned generează scalari cu path nested
(`DimensiuniDebit.RepartitorId`) — ascunși explicit, HideForeignKeys nu-i
vede.
(c) **Afișarea owned-urilor**: `OwnedObjectBase.ToString()` în bibliotecă
(numele tipului real, de-proxificat — walk pe BaseType cât
`Assembly.IsDynamic`); `Dimensiuni.ToString()` în Conta = enumerarea
compactă a dimensiunilor setate (prefixe R/M/CF/CE/F/U/P/CC pe `Cod`),
navigația citită DOAR la FK non-null. `[ExpandObjectMembers(InDetailView)]`
pe `RegistruContabil.DimensiuniDebit/Credit` FUNCȚIONEAZĂ pe owned
(verificat în browser — membrii apar inline, read-only; motivul pentru
care nu se auto-expanda: sub EF Core `IsAggregated` = doar atributul
`[Aggregated]`, nu ownership-ul). Review advers → **AutoInclude pe
navigațiile interne ale owned-urilor registrului**
(`ConfigureDimensiuniEager`): PreFetch-ul XAF nu coboară în owned ⇒ lazy
per instanță (N+1 pe pagină) + lazy-load pe OS disposed la render târziu;
DOAR registrul — liniile de document și regulile de contare rămân lazy
(hot-path-ul motorului nu afișează etichete).
(d) **Imperechere — 40e tranșat ALTFEL decât schița**: nu ForbidCRUD +
acțiune dedicată (popup non-persistent = mașinărie grea), ci New prin UI
PERMIS dar validat la commit: `ImperechereService.ValideazaCreare` (extras
din `Creeaza`, aceleași verificări + null-guard pe navigații) apelat de
`ImperechereController` în `ObjectSpace.Committing`; Edit blocat
(AllowEdit=false + refuz pe modificate la commit — re-validarea sumei ar
cere excluderea propriului rând); Delete liber (31d); `Autogenerat`
read-only. Review advers → invariant NOU în serviciu: refuz
Plata↔Plata / Incasare↔Incasare (sensul opus e obligatoriu;
Plata↔Incasare rămâne — avans↔regularizare); re-evaluarea capabilităților
și pe `Committed` (după Save view-ul rămânea editabil). Limitare asumată,
comentată: două link-uri noi în același commit nu se văd reciproc la
Σ≤rest (operator unic, ca 25f).
(e) Proces: pachetele ies prin `build\pack-and-push.ps1` (bump automat din
tag-uri git; push-ul îl rulează utilizatorul — clasificatorul de permisiuni
blochează push-ul extern); Conta referă flotant `26.1.3.*` ⇒ consumul =
restore (**[CORECȚIE 53h]** azi e PINNAT `26.1.3.7` în
`Directory.Packages.props` — un upgrade cere bump explicit). Testul ToString
din bibliotecă simulează proxy-ul cu
Reflection.Emit (Proxies tranzitiv e 8.x vs EF 10 — pariu de versiune
refuzat); calea reală EF e acoperită de ModelCheck + smoke UI în Conta.
