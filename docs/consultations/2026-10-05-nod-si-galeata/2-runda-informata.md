# Runda 2 (informată): cele două amendamente, pe cod

Primești trei lucruri: răspunsul tău din runda 1
(`docs/consultare/runda1-proprie.md`), fișa de fapte
(`docs/consultare/fapte.md`) și acces de citire la o clonă a repo-ului, la
commit-ul `a5df5fe`. Instrucțiunile proiectului pentru agenți sunt mutate în
`docs/instructiuni-proiect/`.

## Ce afli abia acum

Azi sunt implementate forma X la asamblare și regula P la poziția fără
unitate. Owner-ul înclină spre forma Y pe același cont și spre regula Q.
Sesiunea care orchestrează consultarea a propus ea însăși Y și Q, deci fișa
de fapte e părtinitoare. Nu ți se cere să confirmi și nici să răstorni.

## Circularitatea, pe care trebuie s-o ocolești

Catalogul de scenarii, testele nucleului și ModelCheck au fost scrise sub
forma X și regula P. Nu le folosi ca oracol. Le folosești numai ca listă de
impact: ce s-ar rescrie.

Probele admise sunt patru:

1. regula contabilă, judecată independent de cod;
2. invarianții nucleului, fiecare judecat separat: principiu sau artefact al
   formei de azi;
3. cazuri numerice construite de tine, nu luate din catalog;
4. practica din sursa reală, din fișa de fapte §3.

## Reguli de lucru

- Numai citire: fără modificări, fără build, fără ModelCheck, fără rulări pe
  baze. Ai voie cu citire de fișiere, căutare și `git log`.
- Fiecare afirmație de fapt poartă `fișier:linie`. Ce nu poți ancora
  marchezi `[judecată]`.
- Nu citi `comunicari/`. Ignoră `docs/dosar.md`, e al altei consultări.

## Sarcinile

1. **Runda 1.** Ia lista ta de fapte care ți-ar schimba verdictul și răspunde
   la fiecare din cod sau din fișa de fapte.
2. **Forma Y pe nucleu.** Citește `Miscare`, `Mutare`, `Transformare` și
   `Conservare`. Spune dacă forma Y trece prin nucleul de azi fără
   modificări. Spune ce verificări rămân fără obiect și ce gard nou trebuie,
   și unde stă: în nucleu sau în declarant.
3. **Cazuri proprii.** Construiește cel puțin patru cazuri numerice și scrie
   postările în ambele forme: 2 A devin 1 B pe același cont; consum din două
   loturi FIFO cu rotunjire la împărțirea valorii; storno după ce produsul a
   fost consumat parțial; corecție după închiderea lunii. Spune unde diferă
   soldurile, rulajele și ce poate greși în tăcere.
4. **Cititorii.** Găsește fiecare cititor care însumează pe cont sau pe cont
   și produs fără filtru pe gestiunea reală sau pe unitate. Pentru fiecare,
   spune ce ar întoarce sub forma Y. Pornește de la apelurile filtrului
   contraponderii din fișa de fapte §1.
5. **Invarianții.** Pentru fiecare verificare din `Conservare` și pentru
   fiecare invariant din `M/Cub/Citiri/Invarianti.cs`, spune: principiu care
   rămâne, artefact al formei X, sau artefact al regulii P.
6. **Clasificarea scenariilor.** Pentru rândurile SC-ASM din
   `docs/nucleu/scenarii/ASM.md` și pentru SC-NTC-01, 14, 15 și SC-IMO-27,
   28, 31, spune la fiecare: așteptarea vine din regula contabilă și rămâne,
   sau vine din formă și se rescrie. Tabel scurt.
7. **Regula Q pe cod.** Așază cele trei regimuri peste codul de azi. Spune ce
   comportament se schimbă cu sămânța propusă. Judecă: închiderea unei găleți
   cu sold; costul blocajului serial dacă regimul suport s-ar extinde;
   ciocnirea de nume cu steagul `Unitate`; dacă regimul trebuie să fie pe
   cont sau pe cont și fel de unitate.
8. **Interacțiunea.** Postările nodului stau pe cont de stoc, fără lot, în
   gestiune virtuală. Spune dacă „gestiunea virtuală" e un discriminator
   suficient față de găleată, în scriere și în citire, și ce alternativă ar
   fi mai sigură.
9. **Contul lotului.** Contul vine din tipul de material al produsului.
   Spune ce înseamnă „același cont" în aceste condiții și dacă gardul formei
   Y poate fi ocolit sau poate refuza pe nedrept.
10. **Locul în plan.** Amendamentul 1 în locul punctului ASM din pasul 6 al
    TR-D9a, sau după tăiere. Amendamentul 2 în TR-D9a sau în TR-D9b. Spune
    riscul fiecărei așezări și ce text de contract s-ar schimba.
11. **Fișa de fapte.** Spune ce omite sau înclină.

## Ce livrezi

1. Verdictul pe fiecare amendament: se acceptă, se acceptă cu schimbări sau
   se respinge. La respingere dai contraexemplul.
2. Ce ți-ai schimbat față de runda 1 și din ce cauză.
3. Lista de schimbări cerute, în ordinea priorității.
4. Ce ar trebui măsurat sau prototipat înainte de decizie, cu costul în zile.
5. Încrederea, de la 1 la 5, pe fiecare verdict și ce o limitează.

Nu fi diplomat. Cel mult 3.500 de cuvinte, în română.
