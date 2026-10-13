# TR-D9b, pasul 2 — redenumirea tipurilor unității de sold și sensului

Tip: analiza
Data: 2026-10-12
Alimentează: contractul TR-D9b, D9B-D6; 118-r7 și 118-r8

[Contractul feliei](tr-d9b-contract.md). Baza: `96ccc5a`, peste documentația
necomisă a pașilor anteriori. Owner-ul a cerut continuarea cu pasul următor.
Nu este implementată încă persistența originii partidei.

## Schimbarea

Tipurile CLR `Atlas.Conta.Nucleu.Unitate` și `Atlas.Conta.Nucleu.Latura`
devin `UnitateSold` și `Sens`. Fișierele definițiilor poartă numele noi.
Consumatorii din nucleu, teste, Module, ModelCheck și Import1C folosesc
tipurile noi. Redenumirea a fost calculată pe simboluri C# prin Roslyn:
553 de înlocuiri în 128 de fișiere, fără înlocuire globală de text.

Membrii existenți numiți `Unitate` sau `Latura` nu se redenumesc în acest
pas. Astfel, `Coordonate.Unitate` are tipul `UnitateSold`, iar
`Coordonate.Latura` și `Postare.Latura` au tipul `Sens`.
Identificatorii coloanelor, membrii DTO și proprietățile folosite de UI
rămân cei actuali; laturile documentelor și celelalte sensuri ale
„unității” nu sunt atinse. `Sens.Debit = 1`, `Sens.Credit = 2`.

`Cub/Explicatie.cs` scrie explicit obiectul JSON `unitate` și câmpurile lui;
schimbarea tipului parametrului nu schimbă această formă. Discriminatorii
deciziilor și ipotezelor rămân aceiași, iar versiunea explicației este 2.
Nu există migrație sau schimbare de comportament contabil intenționată.

## Verificarea și mediul

Verificările de produs trec prin
`nou/tools/ModelCheck/scripts/verifica.ps1`. Comenzile pasului:

```powershell
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Nucleu
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bRename -PregatesteBaze
pwsh -NoProfile -File nou/tools/ModelCheck/scripts/verifica.ps1 -Suita Integral -Profil Ambele -Sufix .CodexD9bRename
```

Nucleul: **199/199**, zero eșecuri, zero teste sărite,
`run-verificari/20261012-114537-706/rezultat.json`.

Pregătirea bazelor a creat numai clona bugetară
`Atlas.Conta.BackOffice.CodexD9bRename`, apoi a raportat lipsa șablonului
`Atlas.Conta.ModelCheck.Privat`. Rularea fără `-PregatesteBaze` lasă
ModelCheck să creeze baza sa privată sufixată, conform rețetei existente.
Bazele sursă nu au fost modificate.

Procesul de verificare a cerut completarea variabilelor de mediu Windows
pentru NuGet și temporare în workspace. Pachetele vin din cache-ul local
`D:/NuGET/Global` și depozitele offline DevExpress existente. Auditul NuGet
care cere rețea a fost oprit numai pentru procesul de verificare.
Referințele flotante au selectat inițial un amestec incompatibil 26.1.4/26.1.5;
reluarea fixează, numai în proces, versiunile din `project.assets.json`
citit înaintea restaurărilor: DevExpress 26.1.4 și Atlas.DXF 26.1.4.10.
Configurația de pachete versionată a proiectului nu se schimbă. Manifestul
și fișierul temporar de fixare sunt în `run-verificari/d9b-redenumire/`.

Integrala: **3.678 bugetar / 5.080 privat OK**, zero FAIL, exit 0,
`run-verificari/20261012-115040-608/rezultat.json`. Sunt aceleași numere de
verificări ca la închiderea C111. `INV-CUB` a verificat 64/70 scene, cu
mutanții uciși și zero postări rămase după purjă; `RAMURA-NUL` este verde
pe ambele profiluri. Sunt incluse probele codec-ului explicației,
identității, FIFO, stornoului, perioadei și comenzilor de document.
Build-ul ModelCheck a avut zero erori și nouă avertismente pe cod neatins
ca logică (nulabilitate, SQL de probă și configurație XAF).

Instrumentul de urmărire a expirat la cinci minute, dar procesul și-a
încheiat rularea; verdictul de mai sus este din `rezultat.json` și jurnalele
ambelor profiluri, nu din expirarea instrumentului. Încercările oprite la
restaurare nu sunt probe ale codului și nu certifică scenariile.

Consumatorii din afara integralei au fost compilați separat, în același
mediu: `dotnet build nou/tools/Import1C/Import1C.csproj --nologo` și
`dotnet build nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/Atlas.Conta.BackOffice.WebApi.csproj --nologo`.
Ambele: exit 0, zero erori și zero avertismente; jurnalele sunt
`run-verificari/d9b-redenumire/import-build.log` și `webapi-build.log`.
Importul nu a fost executat.

Inventarul API a găsit numai parametrul fabricii
`ExplicatieUnitateDto.Din(UnitateSold)`, fără schimbarea câmpurilor DTO.
Nu s-au modificat captions, clientul generat, migrațiile sau WebApi.
Probele HTTP și regenerarea OpenAPI/metadata nu au fost rerulate pentru
această redenumire de tipuri interne.

## Limite

Pasul nu adaugă scenarii noi de domeniu: exercită catalogul existent și
testele nucleului. SC-D9B-01…12 rămân în pachetul originii. Review-ul advers
și commit-ul nu sunt făcute; restanțele 118-r7/r8 nu se închid numai prin
această predare. Redenumirea tuturor membrilor cu numele vechi ar implica
altă schimbare decât cele două tipuri CLR și rămâne explicit separată.
