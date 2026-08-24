# Decizia 24 — Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem pe EF Core

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: DEPĂȘITĂ de 54c (owned-ul a murit din Conta); rămân valabile: EF Core nu XPO, maparea NuGet Atlas.DXF, versiunile flotante EF
- **Rezumat durabil**: `CLAUDE.md` §24

---

**Owned `Dimensiuni` sub XAF/EF Core — limitarea e gestionabilă, rămânem
pe EF Core (nu XPO).** Docs XAF declară owned types „nesuportate", dar
verificat empiric (26.1.3, UI Blazor + surse DevExpress): TypesInfo,
startup, securitate, ListView/DetailView funcționează; singura rupere
reală era la creare — DbContext-ul XAF cere notificări complete
(`ChangingAndChangedNotificationsWithOriginalValues` + proxies), iar
instanța `new Dimensiuni()` e POCO fără `INotifyPropertyChanging`.
Fix (promovat în Atlas.DXF.EfCore 26.1.3.2, pachet referit din Module):
`Dimensiuni : OwnedObjectBase` (baza implementează
`INotifyPropertyChanging/Changed` + helper `SetPropertyValue`; aici rămân
doar perechile backing-field + proprietate virtuală), iar maparea folosește
`OwnsOneRequired` (OwnsOne + navigație required). Owner-ii noi creați în
cod trebuie să fie proxy (`CreateProxy`, cum face XAF/ObjectSpace implicit).
Rețeta completă: xaf-kb `recipes/atlas-dxf/efcore-owned-types.md`.
NuGet: `nou/nuget.config` mapează Atlas.DXF.* → feed Atlas; EF Core /
Npgsql / System.Security.Cryptography.Xml au trecut pe versiuni flotante
`10.0.*` (cerință de compatibilitate cu pachetul).
Round-trip insert/update/materializare all-null verificat în ModelCheck;
fără drift de migrații. Proprietățile Dimensiuni apar în UI XAF ca
read-only ToString (inofensiv); editarea lor în back-office (când va fi
nevoie, la 3c/3d — RegulaContare) se rezolvă aditiv: wrappers delegați
`[NotMapped]` sau ecran React. Tierul Web API pentru React (pasul 5)
rămâne de validat pe owned la momentul lui — DTO-urile plate (decizia
6/7) fac oricum flattening explicit.
