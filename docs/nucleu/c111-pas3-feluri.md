# C111, pasul 3 — felurile de repartitor pe cont

- Tip: analiza
- Data: 2026-10-10
- Alimentează: 111 (raport de pas al contractului C111)

**Data:** 2026-10-10. **Stare:** implementat și verificat pe catalog, pe
ambele profiluri. Contractul: [C111](c111-coordonate-contract.md), D4 și S6.

Urmele, neversionate: `run-verificari/20261010-213045-004/` (integrala).

## Ce s-a schimbat

- **Modelul.** `Cont.FeluriRepartitor` e o mulțime peste felurile de
  repartitor plus `Niciunul`; mulțimea vidă e `Nedeclarat`.
  `DimensiuneFlags.Repartitor` a ieșit. Coloana e în `InitialCreate`.
- **Gardul.** `GardAnaliza` judecă prezența și felul repartitorului de sold,
  pe capetele mișcărilor contabile, pe postările de deschidere și pe
  contraponderea de inventar a NIR-ului. Contraponderea e scutită numai de
  prezență. Conturile păzite sunt cele care declară feluri sau alte
  dimensiuni, fiecare familie independent.
- **Faptul contului.** `ContFapt.CereRepartitor` însemna la acest pas
  „declară feluri și nu admite lipsa”. Era greșit: pe conturile cu „partener
  sau niciunul” terțul documentului nu mai ajungea pe picior. Corectat la
  review-ul închiderii (C111-F-R1, 2026-10-11): faptul e `DeclaraRepartitor`,
  adevărat pe orice cont care declară feluri.
- **Seed-ul bugetar.** Cele 20 de conturi cu defalcarea R din CPLAN își
  declară felurile dintr-un tabel pe simbol; seed-ul cade dacă defalcarea și
  tabelul nu se potrivesc. Seed-ul privat nu declară nimic.

| Feluri | Conturi |
|---|---|
| partener | 401.01.00, 401.02.00, 404.01.00, 411.01.01, 411.02.08, 442.08, 442.08.01, 458.01.00, 458.05.01, 458.05.02, 461.01.09, 774, 804.90.00, 805.00.00 |
| partener și angajat | 462.01.01, 462.01.09 |
| partener sau niciunul | 437.01.00, 437.02.00, 803.00.01 |
| cont propriu | 552.00.00 |

## Verificarea

| Probă | Rezultat |
|---|---|
| Integrala ModelCheck, bugetar | 3.651 OK, 0 FAIL |
| Integrala ModelCheck, privat | 5.053 OK, 0 FAIL |
| Modelul față de migrație | fără schimbări nemigrate |
| Metadata clientului, OpenAPI, tipurile generate | regenerate |

Integrala a rulat pe baze recreate din migrația cu coloana nouă. Suita
nucleului n-a fost rerulată: nucleul nu e atins.

Bazele de dezvoltare sunt recreate din `InitialCreate` (C111-Q16):
`Atlas.Conta.BackOffice` și `Atlas.Conta.BackOffice.Privat` pe 5444,
`Atlas.Conta.BackOffice` și `Atlas.Conta.ModelCheck.Privat` pe 5446. Integrala
fără sufix, pe ele, dă aceleași cifre: 3.651 și 5.053, zero FAIL. Clonele de
măsurare și bazele de import de pe 5444 sunt neatinse, pe lanțul vechi; cele
de import se refac la pasul 6.

Probele din D4:

- `GARD-ANALIZA`, direct pe gard: felul nepotrivit e refuzat (S6); mulțimea cu
  `Niciunul` admite lipsa și felul declarat; contul care declară numai feluri
  e păzit; două feluri declarate le admit pe amândouă; pe contrapondere felul
  unui repartitor prezent se judecă.
- SC-C111-30: seed-ul pe fiecare profil; deschiderea pe un cont care declară
  cont propriu, cu repartitor absent, extern și cont propriu; note pe conturi
  care declară partener, respectiv partener sau niciunul; contul nedeclarat.
- SC-PLT-09 și SC-INC-09 rulează pe gardul nou: contul propriu declară cont
  propriu, cel de terț partener.

## Așteptările schimbate

S6 are o urmare pe bugetar care nu era numită: nota contabilă pe un cont de
terți fără partener pe linie e refuzată. Latura fără repartitor explicit ia
locul notei, care e intern, iar contul declară numai feluri externe. Înainte
flag-ul era mulțumit de orice repartitor.

Circa 30 de note din scenele existente foloseau 401.01.00 sau 411.01.01
drept contrapartidă generică, fără partener. Pe bugetar ele stau acum pe
542.01.00, cont cu partide care nu declară feluri; pe privat rămân pe 401 și
4111. Sumele și forma scenelor nu se schimbă; se schimbă contul.

| Scena | Ce s-a mutat |
|---|---|
| `ScenariiFisa` (SC-CIT-20…23, 34) | toate notele scenei |
| `ScenariiNtc` (SC-NTC-01…06, 15, 18, 19) | notele fără partener; cele cu partener rămân pe 401.01.00 |
| `ScenariiPartideCub` (SC-CIT-43) | nota fără partener explicit |
| `ScenariiRegim` (SC-REGIM-01) | nota de probă a regimului |
| `VerificaApiNtc` | linia negativă fără repartitori, linia de probă a refuzurilor, nota cu cont propriu și cea cu unitate internă pe linie, nota fără contrapartidă |

Cazurile au fost strânse într-o singură trecere, cu gardul instrumentat
temporar să înregistreze în loc să refuze; instrumentarea nu e în cod.

## Ce nu e probat

- Felurile celor 14 conturi fără mișcare în legacy 2026 sunt din natura
  contului.
- Pe 461.01.09 angajatul nu e admis; confirmat de owner (C111-Q21, 2026-10-11).
- Gardul pe feluri n-a rulat pe import: baza de import e privată și nu
  declară nimic.
