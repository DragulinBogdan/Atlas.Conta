# Runda 2 (informată): repartitorul pe postare, pe cod

Primești trei lucruri: răspunsul tău din runda 1
(`docs/consultare/runda1-proprie.md`), fișa de fapte
(`docs/consultare/fapte.md`) și acces de citire la o clonă a repo-ului, la
commit-ul `717c3b5`. Instrucțiunile proiectului pentru agenți sunt mutate în
`docs/instructiuni-proiect/`.
Prefixele `M` și `N` din sarcini sunt cele definite în fișa de fapte.

## Ce afli abia acum

Azi e implementată forma 1. Varianta 1b e aprobată și neimplementată. Cheia
de pereche din forma 5 e aprobată pentru alt scop și neimplementată. Owner-ul
a propus forma 4 și vede câștig de generalitate în forma 3. Sesiunea care
orchestrează consultarea a recomandat forma 5 și a respins forma 2, deci fișa
de fapte poate fi părtinitoare. Nu ți se cere să confirmi și nici să răstorni.

## Circularitatea, pe care trebuie s-o ocolești

Catalogul de scenarii, testele nucleului și ModelCheck au fost scrise sub
forma 1. Nu le folosi ca oracol. Le folosești numai ca listă de impact: ce
s-ar rescrie.

Probele admise sunt patru:

1. regula contabilă și cerințele declarațiilor, judecate independent de cod;
2. regulile nucleului, fiecare judecată separat: principiu sau artefact al
   formei de azi;
3. cazuri numerice construite de tine;
4. datele reale din fișa de fapte §3.

## Reguli de lucru

- Numai citire: fără modificări, fără build, fără ModelCheck, fără rulări pe
  baze. Ai voie cu citire de fișiere, căutare și `git log`.
- Fiecare afirmație de fapt poartă `fișier:linie`. Ce nu poți ancora
  marchezi `[judecată]`.
- Nu citi `comunicari/` și `docs/consultations/`.

## Sarcinile

1. **Runda 1.** Ia lista ta de fapte care ți-ar schimba verdictul și răspunde
   la fiecare din cod sau din fișa de fapte.
2. **Nucleul.** Citește `Coordonate`, `Capat`, `Miscare`, `Postare`,
   `GestiuniVirtuale` și `Conservare`. Pentru fiecare regulă care depinde de
   `Partener` sau de `Gestiune`, spune: principiu care rămâne în orice formă,
   sau artefact al celor două coloane tipate. Spune ce ar trebui să
   recunoască nucleul în locul gestiunii virtuale.
3. **Declaranții.** Pentru fiecare declarant din `M/Declaratii/`, un rând de
   tabel: ce poartă azi fiecare picior (partener, gestiune reală, gestiune
   virtuală), care ar fi capătul de ieșire, capătul de intrare și gruparea în
   forma 4. Marchează cazurile în care capătul nu e o latură a antetului și
   pe cele în care latura nu deosebește capetele.
4. **Cititorii.** Pornește de la fișa de fapte §4. Clasează fiecare folosire
   a partenerului și a gestiunii: cheie de sold, atribut de eveniment, fapt
   fiscal, identitate de partidă sau de lot. Spune ce s-ar rupe în formele 3,
   4 și 5 și ce ar deveni mai simplu.
5. **Snapshot-urile.** Citește `M/Motor/SolduriService.cs`. Spune cum se
   schimbă cheia și cardinalitatea în fiecare formă.
6. **Fiscalul.** Citește `M/Declaratii/Fiscal.cs`, `M/Cub/Citiri/Fiscale.cs`,
   `M/Proiectii/D394Proiectii.cs`, `M/Proiectii/TvaProiectii.cs` și
   invarianții fiscali. Spune dacă partenerul fiscal poate fi mutat în blocul
   fiscal sau dedus, și ce se întâmplă la storno, la corecție și la inversa
   fiscală.
7. **Cazuri proprii.** Cel puțin cinci cazuri numerice, cu postările scrise
   așa cum le scrie codul de azi și în forma pe care o susții. Spune unde
   diferă soldurile și ce poate greși în tăcere.
8. **Declarația contului.** Azi sunt trei locuri: `RolTert`,
   `UrmarestePartide` și flag-ul `Repartitor`. Citește seed-ul planului pe
   ambele profiluri (`M/DatabaseUpdate/`). Spune dacă se pot uni într-un
   singur „fel de repartitor suportat" și dă conturile din seed pe care
   unirea le-ar strica.
9. **Pașii aprobați.** Pasul 2c (D9-A10) și pasul 7b (D9-A2) duc spre fiecare
   formă sau în sens opus? Implementarea lui 2c acum ar crea lucru de desfăcut?
10. **Locul în plan.** Trei așezări: în TR-D9a, înaintea pasului 7, cât
    migrația și bazele se recreează oricum; în TR-D9b; după. Spune riscul
    fiecăreia și ce text de contract s-ar schimba.
11. **Fișa de fapte.** Spune ce omite, ce înclină și ce e greșit.

## Ce livrezi

1. Verdictul pe fiecare formă: se acceptă, se acceptă cu schimbări sau se
   respinge. La respingere dai contraexemplul.
2. Ce ți-ai schimbat față de runda 1 și din ce cauză.
3. Forma pe care o recomanzi, cu coloanele ei exacte pe `Postare` și cu
   regula de completare într-un paragraf.
4. Lista de schimbări cerute, în ordinea priorității.
5. Ce ar trebui măsurat sau prototipat înainte de decizie, cu costul în zile.
6. Încrederea, de la 1 la 5, și ce o limitează.

Nu fi diplomat. Cel mult 3.500 de cuvinte, în română.
